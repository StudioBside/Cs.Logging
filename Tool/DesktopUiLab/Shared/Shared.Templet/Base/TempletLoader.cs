﻿namespace Shared.Templet.Base;

using System;
using System.Linq;
using Cs.Core.Util;
using Cs.Logging;
using Newtonsoft.Json.Linq;

internal static class TempletLoader
{
    public static string TempletRootPath { get; set; } = string.Empty;

    public static void BuildContainer<T>(string filePath, Func<JToken, T?> factory)
        where T : class, ITemplet
    {
        var data = LoadDictionary(filePath, factory);
        TempletContainer<T>.SetData(data);
    }

    public static void BuildContainer<T>(IEnumerable<string> filePathList, Func<JToken, T?> factory)
          where T : class, ITemplet
    {
        List<T> templets = new();
        foreach (var filePath in filePathList)
        {
            var data = LoadList(filePath, factory);
            if (data == null)
            {
                continue;
            }

            templets.AddRange(data);
        }

        TempletContainer<T>.SetData(templets.ToDictionary(e => e.Key));
    }

    public static void BuildContainer<T>(string filePath, Func<JToken, T?> factory, Func<T, string> strKeySelector)
        where T : class, ITemplet
    {
        var data = LoadDictionary(filePath, factory);
        TempletContainer<T>.SetData(data, strKeySelector);
    }

    public static void BuildContainer<T>(IEnumerable<string> filePathList, Func<JToken, T?> factory, Func<T, string> strKeySelector)
          where T : class, ITemplet
    {
        Dictionary<int, T> templets = new();
        foreach (var filePath in filePathList)
        {
            var data = LoadDictionary(filePath, factory);
            if (data == null)
            {
                continue;
            }

            templets = templets.Union(data).ToDictionary(e => e.Key, e => e.Value);
        }

        TempletContainer<T>.SetData(templets, strKeySelector);
    }

    public static void BuildGroupContainer<T>(string filePath, string groupIdName)
    where T : class, IGroupTemplet, new()
    {
        BuildGroupContainer<T>(filePath, groupIdName, strKeySelector: null);
    }

    public static void BuildGroupContainer<T>(IEnumerable<string> filePathList, string groupIdName)
    where T : class, IGroupTemplet, new()
    {
        BuildGroupContainer<T>(filePathList, groupIdName, strKeySelector: null);
    }

    public static void BuildGroupContainer<T>(string filePath, string groupIdName, Func<T, string>? strKeySelector)
        where T : class, IGroupTemplet, new()
    {
        var fullPath = Path.Combine(TempletRootPath, filePath);
        var json = JsonUtil.Load(fullPath);
        if (json["Data"] is not JArray jArray)
        {
            Log.ErrorAndExit($"[TempletContainer] file loading failed. fileName:{filePath} type:{typeof(T).Name}");
            return;
        }

        Dictionary<int, T> templets = new();
        foreach (var jToken in jArray)
        {
            int groupId = jToken.GetInt32(groupIdName);
            if (templets.TryGetValue(groupId, out var templet) == false)
            {
                templet = new T();
                templet.LoadGroupData(groupId, jToken);
                templets.Add(groupId, templet);
            }

            templet.Load(jToken);
        }

        TempletContainer<T>.SetData(templets, strKeySelector);
    }

    public static void BuildGroupContainer<T>(IEnumerable<string> filePathList, string groupIdName, Func<T, string>? strKeySelector)
    where T : class, IGroupTemplet, new()
    {
        Dictionary<int, T> templets = new();

        foreach (var filePath in filePathList)
        {
            var fullPath = Path.Combine(TempletRootPath, filePath);
            var json = JsonUtil.Load(fullPath);
            if (json["Data"] is not JArray jArray)
            {
                Log.ErrorAndExit($"[TempletContainer] file loading failed. fileName:{filePath} type:{typeof(T).Name}");
                return;
            }

            foreach (var jToken in jArray)
            {
                int groupId = jToken.GetInt32(groupIdName);
                if (templets.TryGetValue(groupId, out var templet) == false)
                {
                    templet = new T();
                    templet.LoadGroupData(groupId, jToken);
                    templets.Add(groupId, templet);
                }

                templet.Load(jToken);
            }
        }

        TempletContainer<T>.SetData(templets, strKeySelector);
    }

    //// --------------------------------------------------------------------

    private static Dictionary<int, T> LoadDictionary<T>(string fileName, Func<JToken, T?> factory)
    where T : ITemplet
    {
        var fullPath = Path.Combine(TempletRootPath, fileName);
        var json = JsonUtil.Load(fullPath);
        if (json["Data"] is not JArray jArray)
        {
            Log.ErrorAndExit($"[TempletContainer] file loading failed. fileName:{fileName} type:{typeof(T).Name}");
            return new Dictionary<int, T>();
        }

        var data = new Dictionary<int, T>();
        foreach (JToken token in jArray)
        {
            var templet = factory(token);
            if (templet is null)
            {
                continue;
            }

            data.Add(templet.Key, templet);
        }

        return data;
    }

    private static List<T> LoadList<T>(string fileName, Func<JToken, T?> factory)
        where T : ITemplet
    {
        var fullPath = Path.Combine(TempletRootPath, fileName);
        var json = JsonUtil.Load(fullPath);
        if (json["Data"] is not JArray jArray)
        {
            Log.ErrorAndExit($"[TempletContainer] file loading failed. fileName:{fileName} type:{typeof(T).Name}");
            return new List<T>();
        }

        var data = new List<T>();
        foreach (JToken token in jArray)
        {
            var templet = factory(token);
            if (templet is null)
            {
                continue;
            }

            data.Add(templet);
        }

        return data;
    }
}
