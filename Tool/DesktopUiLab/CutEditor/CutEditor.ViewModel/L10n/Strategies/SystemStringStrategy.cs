﻿namespace CutEditor.ViewModel.L10n.Strategies;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Cs.Logging;
using CutEditor.Model.ExcelFormats;
using CutEditor.Model.L10n;
using CutEditor.Model.L10n.MappingTypes;
using Du.Core.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shared.Templet.Strings;
using StringStorage.SystemStrings;
using static CutEditor.Model.Enums;
using static StringStorage.Enums;

internal sealed class SystemStringStrategy(VmL10n viewModel) : L10nStrategyBase(L10nSourceType.SystemString)
{
    private readonly ObservableCollection<L10nMappingString> mappings = new();
    private string categoryName = string.Empty;

    public override IReadOnlyList<IL10nMapping> Mappings => this.mappings;

    public override bool LoadOriginData(string name)
    {
        var categoryName = name.Split('_')[^1];
        if (StringTable.Instance.TryGetCategory(categoryName, out var category) == false)
        {
            Log.Warn($"시스템 스트링 카테고리를 찾을 수 없습니다. categoryName:{categoryName}");
            return false;
        }

        foreach (var element in category.Elements)
        {
            this.mappings.Add(new L10nMappingString(element.PrimeKey, element));
        }

        this.categoryName = categoryName;
        viewModel.WriteLog($"시스템 스트링 카테고리:{this.categoryName} 전체 데이터 {this.mappings.Count}개.");
        return true;
    }

    public override bool ImportFile(string fileFullPath, ISet<string> importedHeaders)
    {
        foreach (var mapping in this.mappings)
        {
            mapping.SetImported(null);
        }

        var reader = viewModel.Services.GetRequiredService<IExcelFileReader>();
        var importedCuts = new List<StringOutputExcelFormat>();
        if (reader.Read(fileFullPath, importedHeaders, importedCuts) == false)
        {
            Log.Error($"엑셀 파일 읽기에 실패했습니다. fileName:{fileFullPath}");
            return false;
        }

        // key로 매핑하지 않고, 한글 텍스트 자체로 매핑합니다.
        var dicMappings = this.mappings.ToDictionary(e => e.SourceData.Korean);
        this.ClearStatistics();
        // -------------------- mapping data --------------------
        foreach (var imported in importedCuts)
        {
            if (dicMappings.TryGetValue(imported.Korean, out var mapping) == false)
            {
                Log.Warn($"매핑되지 않은 데이터입니다. key:{imported.Korean}");
                this.IncreaseStatistics(L10nMappingState.MissingOrigin);
                continue;
            }
       
            mapping.SetImported(imported);
            this.IncreaseStatistics(mapping.MappingState);
        }

        return true;
    }

    public override bool SaveToFile(string name, L10nType l10nType)
    {
        var config = viewModel.Services.GetRequiredService<IConfiguration>();
        var stringDbPath = config["StringDbPath"] ?? throw new Exception("StringDbPath is not set in the configuration file.");

        if (SystemStringWriter.Create(stringDbPath, this.categoryName, out var writer) == false)
        {
            Log.Error($"시스템 스트링 database에 연결할 수 없습니다. categoryName:{this.categoryName}");
            return false;
        }

        using (writer)
        {
            foreach (var mapping in this.mappings)
            {
                var newValue = mapping.SourceData.Get(l10nType);
                writer.Upsert(mapping.SourceData.Korean, l10nType, newValue);
            }
        }

        return true;
    }
}
