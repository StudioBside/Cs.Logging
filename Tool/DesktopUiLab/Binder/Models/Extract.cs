﻿namespace Binder.Models;

using System.Collections.Generic;
using System.Text.Json;
using Du.Core.Util;
using static Binder.Models.Enums;

public sealed class Extract
{
    private readonly List<Source> sources = new();
    private readonly List<Uniqueness> uniquenesses = new();
    private string outputFile = string.Empty;
    private string outputGroupBy = string.Empty;
    private string outputFilePrefix = string.Empty;
    private OutputDirection fileOutDirection = OutputDirection.All;
    private bool excludeToolOutput;
    ////// clientOutputType
    ////// bindRoot
    private CustomOutputPath? customOutputPath;
    private DuplicationCleaner? duplicationCleaner;

    public Extract(JsonElement element)
    {
        this.outputFile = element.GetString("outputFile");
        this.outputGroupBy = element.GetString("outputGroupBy", string.Empty);
        element.GetArray("sources", this.sources, element => new Source(element));
        element.GetArray("uniquenesses", this.uniquenesses, element => new Uniqueness(element));
        this.outputFilePrefix = element.GetString("outputFilePrefix", string.Empty);
        this.fileOutDirection = element.GetEnum<OutputDirection>("fileOutDirection");
        this.excludeToolOutput = element.GetBoolean("excludeToolOutput", false);
        if (element.TryGetProperty("customOutputPath", out var subElement))
        {
            this.customOutputPath = new CustomOutputPath(subElement);
        }

        if (element.TryGetProperty("duplicationCleaner", out subElement))
        {
            this.duplicationCleaner = new DuplicationCleaner(subElement);
        }
    }

    public IList<Source> Sources => this.sources;
    public string OutputFile { get => this.outputFile; set => this.outputFile = value; }

    public override string ToString()
    {
        return $"output:{this.outputFile}";
    }

    //// --------------------------------------------------------------------------------------------

    public sealed class Source
    {
        private string excelFile = string.Empty;
        private string sheetName = string.Empty;
        private string beginCell = "A1";

        public Source()
        {
        }

        public Source(JsonElement element)
        {
            this.excelFile = element.GetString("excelFile");
            this.sheetName = element.GetString("sheetName");
            this.beginCell = element.GetString("beginCell");
        }

        public string ExcelFile { get => this.excelFile; set => this.excelFile = value; }
        public string SheetName { get => this.sheetName; set => this.sheetName = value; }
        public string BeginCell { get => this.beginCell; set => this.beginCell = value; }
    }

    public sealed class Uniqueness
    {
        private readonly string name = string.Empty;
        private readonly List<string> columnNames = new();

        public Uniqueness(JsonElement element)
        {
            this.name = element.GetString("name");
            element.GetArray("columnNames", this.columnNames);
        }

        public string Name => this.name;
        public IList<string> ColumnNames => this.columnNames;
    }

    // 패처씬이 사용하는 스트링이 별도의 위치로 export한다.
    public sealed class CustomOutputPath
    {
        private string serverTextOutput = string.Empty;
        private string serverBinOutput = string.Empty;
        private string clientTextOutput = string.Empty;
        private string clientBinOutput = string.Empty;

        public CustomOutputPath(JsonElement element)
        {
            this.serverTextOutput = element.GetString("serverTextOutput", string.Empty);
            this.serverBinOutput = element.GetString("serverBinOutput", string.Empty);
            this.clientTextOutput = element.GetString("clientTextOutput", string.Empty);
            this.clientBinOutput = element.GetString("clientBinOutput", string.Empty);
        }

        public string ServerTextOutput => this.serverTextOutput;
        public string ServerBinOutput => this.serverBinOutput;
        public string ClientTextOutput => this.clientTextOutput;
        public string ClientBinOutput => this.clientBinOutput;
    }

    public sealed class DuplicationCleaner
    {
        private readonly OutputDirection fileOutDirection = OutputDirection.Client;
        private readonly List<string> columnNames = new();

        public DuplicationCleaner(JsonElement element)
        {
            this.fileOutDirection = element.GetEnum<OutputDirection>("fileOutDirection");
            element.GetArray("columnNames", this.columnNames);
        }

        internal OutputDirection FileOutDirection => this.fileOutDirection;
        internal IList<string> ColumnNames => this.columnNames;
    }
}
