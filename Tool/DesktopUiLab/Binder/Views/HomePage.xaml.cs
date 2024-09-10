﻿namespace Binder.Views;

using System.Windows.Controls;
using Binder.ViewModels;
using Du.Core.Util;

public partial class HomePage : Page
{
    public HomePage()
    {
        this.InitializeComponent();
        this.DataContext = App.Current.Services.GetService<VmHome>();
    }
}
