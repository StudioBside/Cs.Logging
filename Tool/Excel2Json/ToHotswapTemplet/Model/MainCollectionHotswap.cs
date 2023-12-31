﻿namespace Excel2Json.ToHotswapTemplet.Model
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    using Cs.Core.Core;
    using Cs.Core.Util;
    using Cs.Exception;
    using Cs.Logging;
    using Cs.Perforce;
    using Cs.Protocol;

    using Excel2Json.Functions;

    using static Excel2Json.Enums;

    internal sealed class MainCollectionHotswap
    {
        public static readonly string ResultTableHeader;
        private readonly Dictionary<string /*fileName*/, OutputCollectionSet> collectionSet = new();

        static MainCollectionHotswap()
        {
            ResultTableHeader = string.Join(
               " | ",
               string.Empty,
               "Output File".PadRight(35),
               "Direction".PadRight(10),
               "FileType".PadRight(8),
               "Stable".PadRight(6),
               "bef.Size".PadRight(8),
               "aft.Size".PadRight(8),
               "Result".PadRight(15),
               string.Empty);
        }

        private MainCollectionHotswap(ExtractHotswap extract, FileOutputDirection fileOutDir, bool isStable)
        {
            this.Extract = extract;
            this.FileOutDirection = fileOutDir;
            this.IsStable = isStable;
        }

        public ExtractHotswap Extract { get; }
        public FileOutputDirection FileOutDirection { get; }
        public bool IsStable { get; }

        public bool NeedToRemove { get; private set; }
        internal string DebugName => $"[{this.Extract.DebugName} {this.FileOutDirection}]";

        public static MainCollectionHotswap? Create(ExtractHotswap extract, FileOutputDirection fileOutDir, bool isStable, long dataVersion)
        {
            if (fileOutDir == FileOutputDirection.All
                || fileOutDir == FileOutputDirection.Client)
            {
                ErrorContainer.Add($"핫 스왑 템플릿 파일 출력 방향은 All or Client을 사용할 수 없음");
                return null;
            }

            var result = new MainCollectionHotswap(extract, fileOutDir, isStable);

            foreach (var source in extract.Sources)
            {
                if (isStable == false && source.HasStableColumn == false)
                {
                    continue; // unstable 데이터를 뽑아야 하는데 시트에 아예 IsStable 지정 컬럼이 없는 경우 - 패스.
                }

                foreach (DataRow dataRow in source.DataTable.Rows)
                {
                    if (source.HasStableColumn)
                    {
                        var value = dataRow.GetString(SystemColumn.IsStable);
                        if (ParseStableColumnValue(value, out bool dataIsStable) == false)
                        {
                            ErrorContainer.Add($"{extract.DebugName} {SystemColumn.IsStable}컬럼 값이 올바르지 않습니다.");
                            return null;
                        }

                        if (isStable != dataIsStable)
                        {
                            continue;
                        }
                    }

                    var rowCollection = OutputCollectionHotswap.Create(extract.Columns, dataRow, fileOutDir);
                    if (rowCollection == null)
                    {
                        ErrorContainer.Add($"{extract.DebugName} 데이터 추출 실패");
                        return null;
                    }

                    // 출력 파일명 결정. groupby가 지정된 경우 데이터에 따라 다른 파일로 출력된다.
                    var fileName = extract.OutputFile;
                    var originalFileName = Path.GetFileName(fileName);
                    fileName = TryAddFileTypePrefix(result.FileOutDirection, fileName);

                    if (result.collectionSet.TryGetValue(fileName, out var collectionSet) == false)
                    {
                        collectionSet = new OutputCollectionSet(
                            extract,
                            fileName,
                            new List<OutputCollectionHotswap>(),
                            originalFileName,
                            dataVersion);
                        result.collectionSet.Add(fileName, collectionSet);
                    }

                    collectionSet.Rows.Add(rowCollection);
                }
            }

            if (isStable == false && result.collectionSet.Count <= 0)
            {
                result.NeedToRemove = true;
            }

            return result;
        }

        public static long GetDataVersion()
        {
            var now = ServiceTime.Recent.ToString("yyyyMMddHHmmss");
            if (long.TryParse(now, out long result) == false)
            {
                Log.ErrorAndExit($"DataVersion parse fail now:{now}");
                return 0;
            }

            return result;
        }

        public bool WriteTextFile(P4Commander p4Commander)
        {
            var config = Config.Instance;
            AtomicFlag result = new AtomicFlag(initialValue: true);
            Parallel.ForEach(this.collectionSet.Values, collectionSet =>
            {
                var template = ExtractHotswap.CreateTextTemplate();
                template.Add("model", collectionSet);

                string fullFilePath = this.BuildTextOutputFilePath(collectionSet.FileName);
                FileSystem.GuaranteePath(fullFilePath);

                string fileName = Path.GetFileName(fullFilePath);
                if (File.Exists(fullFilePath))
                {
                    File.SetAttributes(fullFilePath, FileAttributes.Normal);
                }

                try
                {
                    using var sw = new StreamWriter(fullFilePath, append: false, Encoding.UTF8);
                    sw.WriteLine(template.Render());
                }
                catch (Exception ex)
                {
                    Log.Error(ExceptionUtil.FlattenInnerExceptions(ex));
                    result.Off();
                    return;
                }

                if (string.IsNullOrEmpty(config.DeleteTargetExtension) == false)
                {
                    var deleteTargetName = Path.ChangeExtension(fullFilePath, config.DeleteTargetExtension);
                    if (File.Exists(deleteTargetName))
                    {
                        if (p4Commander.Delete(deleteTargetName, out var result) == false)
                        {
                            ErrorContainer.Add($"{this.DebugName} p4 delete 실패. p4Output:{result}");
                        }
                    }
                }

                long sourceSizeSum = this.Extract.Sources.Sum(e => e.FileSize);
                FileUtil.GetFileSize(fullFilePath, out var fileSize);
                var logHelper = new ResultLogHelper(this, collectionSet.OriginalFileName, "text", sourceSizeSum, fileSize);
                if (p4Commander.CheckIfOpened(fullFilePath))
                {
                    logHelper.WriteLog(FileWritingResult.AlreadyOpened);
                    return;
                }

                if (p4Commander.CheckIfChanged(fullFilePath, out bool changed) == false)
                {
                    ErrorContainer.Add($"{this.DebugName} 변경여부 확인 실패. fileName:{fileName}");
                    result.Off();
                    return;
                }

                if (changed == false)
                {
                    logHelper.WriteLog(FileWritingResult.NotChanged);
                    return;
                }

                if (p4Commander.OpenForEdit(fullFilePath, out string p4Output) == false)
                {
                    ErrorContainer.Add($"{this.DebugName} p4 edit 실패. p4Output:{p4Output}");
                    result.Off();
                    return;
                }

                logHelper.WriteLog(FileWritingResult.Changed);
            });

            return result.IsOn;
        }

        public void WriteBinFile(P4Commander p4Commander)
        {
            Parallel.ForEach(this.collectionSet.Values, collectionSet =>
            {
                string textFilePath = this.BuildTextOutputFilePath(collectionSet.FileName);
                if (File.Exists(textFilePath) == false)
                {
                    ErrorContainer.Add($"{this.DebugName} text 파일을 찾을 수 없습니다. textFilePath:{textFilePath}");
                    return;
                }

                string binFilePath = this.BuildBinOutputFilePath(collectionSet.FileName);
                FileUtil.GetFileSize(textFilePath, out var textFileSize);
                string tempFilePath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
                bool convertResult = Text2Binary.ConvertJsonBinary(textFilePath, tempFilePath);
                if (convertResult == false)
                {
                    ErrorContainer.Add($"{this.DebugName} text -> binary 변환 실패");
                    return;
                }

                FileSystem.GuaranteePath(binFilePath);

                string binFileName = Path.GetFileName(binFilePath);
                if (File.Exists(binFilePath))
                {
                    File.SetAttributes(binFilePath, FileAttributes.Normal);
                }

                var (beforeBytes, afterBytes) = Lz4Util.CompressNew(tempFilePath, binFilePath);
                var logHelper = new ResultLogHelper(this, collectionSet.OriginalFileName, "bytes", textFileSize, afterBytes);
                if (p4Commander.CheckIfOpened(binFilePath))
                {
                    logHelper.WriteLog(FileWritingResult.AlreadyOpened);
                    return;
                }

                if (p4Commander.CheckIfChanged(binFilePath, out bool changed) == false)
                {
                    ErrorContainer.Add($"{this.DebugName} 변경여부 확인 실패. fileName:{binFileName}");
                    return;
                }

                if (changed == false)
                {
                    logHelper.WriteLog(FileWritingResult.NotChanged);
                    return;
                }

                if (p4Commander.OpenForEdit(binFilePath, out string p4Output) == false)
                {
                    ErrorContainer.Add($"{this.DebugName} p4 edit 실패. p4Output:{p4Output}");
                    return;
                }

                logHelper.WriteLog(FileWritingResult.Changed);
            });
        }

        public void Remove(P4Commander p4Commander)
        {
            var fileName = TryAddFileTypePrefix(this.FileOutDirection, this.Extract.OutputFile);
            var removePath = this.BuildTextOutputFilePath(fileName);
            if (File.Exists(removePath))
            {
                if (p4Commander.Delete(removePath, out var p4output) == false)
                {
                    ErrorContainer.Add($"[{this.DebugName}] remove textFile failed. reason:{p4output}");
                    return;
                }

                var logHelper = new ResultLogHelper(this, this.Extract.OutputFile, "text", 0, 0);
                logHelper.WriteLog(FileWritingResult.Remove);
            }

            removePath = this.BuildBinOutputFilePath(fileName);
            if (File.Exists(removePath))
            {
                if (p4Commander.Delete(removePath, out var p4output) == false)
                {
                    ErrorContainer.Add($"[{this.DebugName}] remove binFile failed. reason:{p4output}");
                    return;
                }

                var logHelper = new ResultLogHelper(this, this.Extract.OutputFile, "bytes", 0, 0);
                logHelper.WriteLog(FileWritingResult.Remove);
            }
        }

        // 엑셀 stable 컬럼 상의 값을 해석하는 규칙입니다
        internal static bool ParseStableColumnValue(string value, out bool result)
        {
            if (string.IsNullOrEmpty(value))
            {
                // 컬럼이 비어있으면 stable로 간주합니다.
                result = true;
                return true;
            }

            if (bool.TryParse(value, out result) == false)
            {
                // bool로 해석할 수 없는 값이면 unstable입니다.
                result = false;
                return false;
            }

            return true;
        }

        internal static string TryAddFileTypePrefix(FileOutputDirection fileOutDir, string filePath)
        {
            var config = Config.Instance;

            var builder = new StringBuilder();
            using var writer = new StringWriter(builder);
            if (config.UseFileDirectionPrefix)
            {
                var prefix = fileOutDir switch
                {
                    FileOutputDirection.Server => "SERVER_",
                    FileOutputDirection.Client => "CLIENT_",
                    FileOutputDirection.Tool => "TOOL_",
                    _ => throw new Exception($"[FileDirectionPrefix] invalid outputType:{fileOutDir}"),
                };
                writer.Write(prefix);
            }

            if (config.UseFileTypePrefix)
            {
                writer.Write("JSON_");
            }

            var fileNameOnly = Path.GetFileName(filePath);
            var directoryName = Path.GetDirectoryName(filePath) ?? string.Empty;
            return Path.Combine(directoryName, $"{builder}{fileNameOnly}");
        }

        //// ------------------------------------------------------------------------------------

        private string BuildTextOutputFilePath(string fileNamePrefix)
        {
            var config = Config.Instance;
            var outputPath = this.Extract.GetFinalTextOutput(this.FileOutDirection);
            string fileName = this.IsStable
                ? $"{fileNamePrefix}{config.TextOutputExtension}"
                : $"{fileNamePrefix}.unstable{config.TextOutputExtension}";
            return Path.GetFullPath(Path.Combine(outputPath, fileName));
        }

        private string BuildBinOutputFilePath(string fileNamePrefix)
        {
            var config = Config.Instance;
            var outputPath = this.Extract.GetFinalBinOutput(this.FileOutDirection);
            string fileName = this.IsStable
                ? $"{fileNamePrefix}{config.BinOutputExtension}"
                : $"{fileNamePrefix}.unstable{config.BinOutputExtension}";
            return Path.GetFullPath(Path.Combine(outputPath, fileName));
        }

        private readonly struct ResultLogHelper
        {
            private readonly MainCollectionHotswap owner;
            private readonly string fileName;
            private readonly string fileType;
            private readonly long sizeBefore;
            private readonly long sizeAfter;

            public ResultLogHelper(MainCollectionHotswap owner, string fileName, string fileType, long sizeBefore, long sizeAfter)
            {
                this.owner = owner;
                this.fileName = fileName;
                this.fileType = fileType;
                this.sizeBefore = sizeBefore;
                this.sizeAfter = sizeAfter;
            }

            public FileWritingResult WriteLog(FileWritingResult result)
            {
                var trimmedFileName = this.fileName.Length > 32
                    ? $"...{this.fileName[^32..]}"
                    : this.fileName;

                var stableMark = this.owner.IsStable ? "O" : "X";
                Log.Info($" | {trimmedFileName,-35} | {this.owner.FileOutDirection,10} | {this.fileType,8} | {stableMark,6} | {this.sizeBefore.ToByteFormat(),8} | {this.sizeAfter.ToByteFormat(),8} | {result,15} |");
                return result;
            }
        }

        private sealed record OutputCollectionSet(
            ExtractHotswap Extract,
            string FileName,
            List<OutputCollectionHotswap> Rows,
            string OriginalFileName,
            long DataVersion);
    }
}
