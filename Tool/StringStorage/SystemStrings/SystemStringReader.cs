﻿namespace StringStorage.SystemStrings;

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Cs.Core;
using Cs.Logging;
using StringStorage.Translation;

/// <summary>
/// root 경로에 존재하는 zip 파일을 모두 연결해두는 (=모든 카테고리를 읽는) 클래스.
/// </summary>
public sealed class SystemStringReader : IDisposable
{
    private readonly Dictionary<string /*category*/, L10nReadOnlyDb> dbList = new();
    public bool Initialize(string dbRoot)
    {
        // root에 존재하는 zip 파일을 대상으로 초기 로딩
        var zipFiles = Directory.GetFiles(dbRoot, "*.zip");
        foreach (var zipFile in zipFiles)
        {
            var category = Path.GetFileNameWithoutExtension(zipFile);
            var db = new L10nReadOnlyDb(Path.Combine(dbRoot, category));
            this.dbList.Add(category, db);
        }

        Log.Debug($"SystemStringReader.Initialize. rootPath:{dbRoot} dbList:{this.dbList.Count}");
        return true;
    }

    public void Dispose()
    {
        foreach (var data in this.dbList.Values)
        {
            data.Dispose();
        }
    }

    public bool TryGetDb(string category, [MaybeNullWhen(false)] out L10nReadOnlyDb db)
    {
        return this.dbList.TryGetValue(category, out db);
    }

    public SingleTextSet GetTextSet(string category, string key)
    {
        if (this.dbList.TryGetValue(category, out var db) == false)
        {
            Log.Error($"string category not found. category:{category} key:{key}");
            return SingleTextSet.Empty;
        }

        return db.Get(key);
    }
}
