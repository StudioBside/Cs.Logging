﻿namespace CutEditor.Dialogs.BgFilePicker;

using System.Globalization;
using System.Windows;
using Du.Presentation.Converters;
using NKM;

public sealed class SelectedToStringConverter : ConverterMarkupExtension<SelectedToStringConverter>
{
    public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value switch
        {
            ElementType elementType => elementType.FileNameOnly,
            CURRENTTOWN currentTown => currentTown.ToString(),
            _ => DependencyProperty.UnsetValue,
        };
    }

    public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value switch
        {
            ElementType elementType => elementType.FileNameOnly,
            CURRENTTOWN currentTown => currentTown.ToString(),
            _ => DependencyProperty.UnsetValue,
        };
    }
}
