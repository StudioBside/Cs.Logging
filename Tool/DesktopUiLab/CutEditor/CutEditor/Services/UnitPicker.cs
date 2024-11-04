﻿namespace CutEditor.Services;

using System.Threading.Tasks;
using CutEditor.Dialogs;
using CutEditor.Model.Interfaces;
using Wpf.Ui;

internal sealed class UnitPicker(IContentDialogService contentDialogService)
    : IUnitPicker
{
    public async Task<IUnitPicker.PickResult> PickUnit()
    {
        var dialog = new UnitPickerDialog(contentDialogService.GetDialogHost());
        var result = await dialog.ShowAsync();
        return result switch
        {
            Wpf.Ui.Controls.ContentDialogResult.Primary => new IUnitPicker.PickResult(dialog.SelectedUnit, false),
            Wpf.Ui.Controls.ContentDialogResult.Secondary => new IUnitPicker.PickResult(null, false),
            _ => new IUnitPicker.PickResult(null, true),
        };
    }
}
