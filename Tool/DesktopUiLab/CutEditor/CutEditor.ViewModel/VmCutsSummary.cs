﻿namespace CutEditor.ViewModel;

using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Cs.Core.Util;
using Cs.Logging;
using CutEditor.Model;
using CutEditor.ViewModel.Detail;
using Du.Core.Bases;
using Du.Core.Interfaces;
using Du.Core.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

public sealed class VmCutsSummary : VmPageBase
{
    private readonly ObservableCollection<VmCut> cuts = new();
    private readonly ObservableCollection<VmCut> selectedCuts = new();
    private readonly string name;
    private readonly CutUidGenerator uidGenerator;
    private readonly IServiceProvider services;
    private readonly IServiceScope serviceScope;

    public VmCutsSummary(IConfiguration config, IServiceProvider services)
    {
        this.serviceScope = services.CreateScope();
        this.services = services;
        this.CopyFileNameCommand = new RelayCommand(this.OnCopyFileName);
        this.GoToEditCommand = new RelayCommand<VmCut>(this.OnGoToEdit);

        if (VmGlobalState.Instance.VmCutsCreateParam is null)
        {
            throw new Exception($"VmCuts.CreateParam is not set in the GlobalState.");
        }

        var param = VmGlobalState.Instance.VmCutsCreateParam;

        if (param.CutScene is null)
        {
            this.name = param.NewFileName ?? throw new Exception("invalid createParam. newFileName is empty.");
            this.Title = $"새로운 파일 생성 - {this.name}";
        }
        else
        {
            this.name = param.CutScene.FileName;
            this.Title = $"{param.CutScene.Title} - {this.name}";
        }

        this.TextFileName = CutFileIo.GetTextFileName(this.name);
        if (File.Exists(this.TextFileName) == false)
        {
            Log.Debug($"cutscene file not found: {this.TextFileName}");
            this.uidGenerator = new CutUidGenerator(Enumerable.Empty<Cut>());
            return;
        }

        var json = JsonUtil.Load(this.TextFileName);
        json.GetArray("Data", this.cuts, (e, i) =>
        {
            var cut = new Cut(e);
            return new VmCut(cut, this.services);
        });

        this.uidGenerator = new CutUidGenerator(this.cuts.Select(e => e.Cut));

        Log.Info($"{this.name} 파일 로딩 완료. 총 컷의 개수:{this.cuts.Count}");

        var removeTargets = this.cuts.Where(e => e.DataType == Enums.CutDataType.Normal && e.Cut.UnitTalk.Korean.Length == 0).ToArray();
        if (removeTargets.Length > 0)
        {
            Log.Debug($"{this.name} 파일에서 대사가 없는 컷을 제거합니다. {removeTargets.Length}개");
            foreach (var vmCut in removeTargets)
            {
                this.cuts.Remove(vmCut);
            }
        }
    }

    public IList<VmCut> Cuts => this.cuts;
    public IList<VmCut> SelectedCuts => this.selectedCuts;
    public string TextFileName { get; }
    public ICommand CopyFileNameCommand { get; }
    public ICommand GoToEditCommand { get; }

    private string DebugName => $"[{this.name}]";

    public override void OnNavigating(object sender, Uri uri)
    {
        // 다른 페이지로의 네이게이션이 시작될 때 (= 지금 페이지가 닫힐 때)
        Log.Debug($"{this.DebugName} OnNavigating: {uri}");

        this.serviceScope.Dispose();
    }

    //// --------------------------------------------------------------------------------------------

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);

        //switch (e.PropertyName)
        //{
        //    case nameof(this.SelectedCuts):
        //        this.DeleteCommand.NotifyCanExecuteChanged();
        //        break;
        //}
    }

    private void OnCopyFileName()
    {
        var clipboardWriter = this.services.GetRequiredService<IClipboardWriter>();
        clipboardWriter.SetText(this.name);

        Log.Info($"{this.DebugName} 파일명을 클립보드에 복사했습니다.");
    }

    private void OnGoToEdit(VmCut? target)
    {
        if (target is null)
        {
            if (this.selectedCuts.Count == 0)
            {
                Log.Debug($"{this.DebugName} 선택된 컷이 없습니다.");
                return;
            }

            target = this.selectedCuts.First();
        }

        var cutscene = VmGlobalState.Instance.VmCutsCreateParam?.CutScene;
        if (cutscene is null)
        {
            Log.Error($"{this.DebugName} cutscene is null.");
            return;
        }

        VmGlobalState.Instance.ReserveVmCuts(new VmCuts.CrateParam
        {
            CutScene = cutscene,
            CutUid = target.Cut.Uid,
        });
        WeakReferenceMessenger.Default.Send(new NavigationMessage("Views/PgCuts.xaml"));
    }
}