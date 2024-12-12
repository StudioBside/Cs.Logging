namespace Du.Presentation.Util;

using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using Du.Core.Interfaces;

/// <summary>
/// frame�� page�� ��� ����Ǵµ�, ���� �̺�Ʈ ������ ����ø��� �Ź� ����ϴ� ���� ���ŷӱ� ������
/// �̺�Ʈ �ڵ鷯�� ���� �� �������� ���� ����ϰ� vm���� frame�� ���� content�� Ȯ���Ͽ� �̺�Ʈ�� �����մϴ�.
/// </summary>
public class AppExitNotifier
{
    private Frame frame = null!;

    public void SetFrame(Frame frame)
    {
        this.frame = frame;

        //Application.Current.Exit += this.OnExit;
        Application.Current.MainWindow.Closing += this.OnClosing;
    }

    //// ------------------------------------------------------------------

    private void OnClosing(object? sender, CancelEventArgs e)
    {
        if (this.frame.Content is Page pageContent &&
            pageContent.DataContext is IAppExitHandler handler)
        {
            handler.OnClosing(e);
        }
    }
}
