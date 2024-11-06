﻿namespace CutEditor.Views;

using System.Windows.Controls;
using CutEditor.ViewModel;
using Microsoft.Extensions.DependencyInjection;

public sealed partial class PgCuts : Page
{
    public PgCuts()
    {
        this.InitializeComponent();
        this.DataContext = App.Current.Services.GetRequiredService<VmCuts>();
    }
}
