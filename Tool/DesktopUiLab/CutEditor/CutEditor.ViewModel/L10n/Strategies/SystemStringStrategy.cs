﻿namespace CutEditor.ViewModel.L10n.Strategies;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Cs.Core.Util;
using Cs.Logging;
using CutEditor.Model.Detail;
using CutEditor.Model.L10n;
using CutEditor.Model.L10n.MappingTypes;
using Du.Core.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Shared.Templet.Strings;
using static CutEditor.Model.Enums;

internal sealed class SystemStringStrategy : IL10nStrategy
{
    private readonly ObservableCollection<L10nMappingString> mappings = new();
    private readonly int[] statistics = new int[EnumUtil<L10nMappingState>.Count];

    public L10nSourceType SourceType => L10nSourceType.SystemString;
    public IEnumerable<IL10nMapping> Mappings => this.mappings;
    public int SourceCount => this.mappings.Count;
    public IReadOnlyList<int> Statistics => this.statistics;

    public bool LoadOriginData(string name, VmL10n viewModel)
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

        viewModel.WriteLog($"시스템 스트링 카테골:{name} 전체 데이터 {this.mappings.Count}개.");
        return true;
    }

    public bool ImportFile(string fileFullPath, VmL10n viewModel, ISet<string> importedHeaders)
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
        Array.Clear(this.statistics);
        // -------------------- mapping data --------------------
        foreach (var imported in importedCuts)
        {
            if (dicMappings.TryGetValue(imported.Korean, out var mapping) == false)
            {
                Log.Warn($"매핑되지 않은 데이터입니다. key:{imported.Korean}");
                ++this.statistics[(int)L10nMappingState.MissingOrigin];
                continue;
            }
       
            mapping.SetImported(imported);
            ++this.statistics[(int)mapping.MappingState];
        }

        return true;
    }

    public bool SaveToFile(string name)
    {
        throw new NotImplementedException();
    }
}
