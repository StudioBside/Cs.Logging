﻿namespace CutEditor;

using System.ComponentModel;
using System.Windows;
using CutEditor.ViewModel;
using Du.Presentation.Extensions;
using Du.Presentation.Util;
using Microsoft.Extensions.DependencyInjection;
using Wpf.Ui;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        this.InitializeComponent();
        this.LoadWindowState();

        var services = App.Current.Services;
        this.DataContext = services.GetRequiredService<VmMain>();

        var dialogService = services.GetRequiredService<IContentDialogService>();
        dialogService.SetDialogHost(this.RootContentDialog);

        var snackbarService = services.GetRequiredService<ISnackbarService>();
        snackbarService.SetSnackbarPresenter(this.SnackbarPresenter);

        PageRouterExtension.Instance.SetFrame(this.MyFrame);
        new AppExitNotifier().SetFrame(this.MyFrame); // 핸들러에 등록되면서 객체는 지워지지 않습니다.
        this.MyFrame.Navigate(new Uri("Views/PgHome.xaml", UriKind.Relative));
    }

    protected override void OnClosing(CancelEventArgs e)
    {
        base.OnClosing(e);
        this.SaveWindowState();
    }
    
    private void LoadWindowState()
    {
        if (Properties.Settings.Default.Width != 0)
        {
            this.Top = Properties.Settings.Default.Top;
            this.Left = Properties.Settings.Default.Left;
            this.Width = Properties.Settings.Default.Width;
            this.Height = Properties.Settings.Default.Height;

            // 화면 밖에 있는 경우 위치 수정
            var virtualScreenWidth = SystemParameters.VirtualScreenWidth;
            var virtualScreenHeight = SystemParameters.VirtualScreenHeight;
            if (this.Top < 0 || this.Left < 0 || this.Top + this.Height > virtualScreenHeight || this.Left + this.Width > virtualScreenWidth)
            {
                this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            }

            // int를 WindowState로 변환
            this.WindowState = (WindowState)Properties.Settings.Default.WindowState;
        }
    }

    private void SaveWindowState()
    {
        Properties.Settings.Default.Top = this.Top;
        Properties.Settings.Default.Left = this.Left;
        Properties.Settings.Default.Width = this.Width;
        Properties.Settings.Default.Height = this.Height;

        // WindowState를 int로 변환하여 저장
        Properties.Settings.Default.WindowState = (int)this.WindowState;

        Properties.Settings.Default.Save(); // 설정 저장
    }
}