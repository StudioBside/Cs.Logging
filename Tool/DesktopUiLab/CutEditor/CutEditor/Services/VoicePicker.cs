﻿namespace CutEditor.Services;

using System.Threading.Tasks;
using CutEditor.Dialogs;
using CutEditor.Model;
using CutEditor.Model.Interfaces;
using Wpf.Ui;

internal sealed class VoicePicker(IContentDialogService contentDialogService)
    : IAssetPicker
{
    public async Task<IAssetPicker.PickResult> PickAsset(string? defaultValue)
    {
        var dialog = new AssetPickerDialog(
            "voice 파일 선택",
            AssetList.Instance.VoiceFiles,
            contentDialogService.GetDialogHost());
        var result = await dialog.ShowAsync();
        return result switch
        {
            Wpf.Ui.Controls.ContentDialogResult.Primary => new IAssetPicker.PickResult(dialog.Selected!.FileNameOnly, false),
            Wpf.Ui.Controls.ContentDialogResult.Secondary => new IAssetPicker.PickResult(null, false),
            _ => new IAssetPicker.PickResult(null, true),
        };
    }
}
