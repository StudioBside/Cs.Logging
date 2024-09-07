﻿namespace Binder.Bases;

using System.Windows.Navigation;
using Binder.Interfaces;
using CommunityToolkit.Mvvm.ComponentModel;

public abstract class ViewModelBase : ObservableObject, INavigationAware
{
    private string title = string.Empty;

    public string Title
    {
        get => this.title;
        set => this.SetProperty(ref this.title, value);
    }

    public virtual void OnNavigating(object sender, NavigatingCancelEventArgs navigationEventArgs)
    {
    }

    public virtual void OnNavigated(object sender, NavigationEventArgs navigatedEventArgs)
    {
    }
}
