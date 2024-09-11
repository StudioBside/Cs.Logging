﻿namespace Binder.Models;

using System;
using System.IO;
using System.Text;
using CommunityToolkit.Mvvm.ComponentModel;
using Du.Core.Util;

public sealed class BindFile : ObservableObject
{
    private readonly string filePath;
    private readonly List<Extract> extracts = new();
    private readonly List<ExtractEnum> extractEnums = new();

    public BindFile(string filePath)
    {
        this.filePath = filePath;
        this.Name = Path.GetFileNameWithoutExtension(filePath);
        this.LastWriteTime = File.GetLastWriteTime(filePath);

        using var document = JsonHelper.LoadJsonc(filePath);
        document.RootElement.GetArray("extracts", this.extracts, element => new Extract(element));
    }

    public string Name { get; }
    public DateTime LastWriteTime { get; }
    public IList<Extract> Extracts => this.extracts;
    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.AppendLine($"파일 이름: {this.Name}");
        sb.AppendLine($"수정 시각: {this.LastWriteTime}");

        return sb.ToString();
    }
}