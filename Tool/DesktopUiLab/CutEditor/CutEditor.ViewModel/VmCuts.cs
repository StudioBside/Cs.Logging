﻿namespace CutEditor.ViewModel;

using System.Collections;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text;
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

public sealed class VmCuts : VmPageBase,
    IDragDropHandler,
    IClipboardHandler
{
    private static CutScene lastCutSceneHistory = null!;

    private readonly CutScene cutscene;
    private readonly ObservableCollection<VmCut> cuts = new();
    private readonly ObservableCollection<VmCut> selectedCuts = new();
    private readonly string fullFilePath;
    private readonly TempUidGenerator uidGenerator = new();
    private readonly IServiceProvider services;
    private bool allSectionUnit = true;
    private bool allSectionScreen;
    private bool allSectionCamera;

    public VmCuts(VmHome vmHome, IConfiguration config, IServiceProvider services)
    {
        this.cutscene = vmHome.SelectedCutScene ?? lastCutSceneHistory;
        this.Title = $"{this.cutscene.Title} - {this.cutscene.FileName}";
        this.services = services;
        this.BackCommand = new RelayCommand(this.OnBack);
        this.SaveCommand = new RelayCommand(this.OnSave);
        this.DeleteCommand = new RelayCommand(this.OnDelete);

        lastCutSceneHistory = this.cutscene;

        var path = config["CutFilesPath"] ?? throw new Exception("CutFilesPath is not set in the configuration file.");
        this.fullFilePath = Path.Combine(path, $"CLIENT_{this.cutscene.FileName}.exported");

        if (File.Exists(this.fullFilePath) == false)
        {
            Log.Debug($"cutscene file not found: {this.fullFilePath}");
            return;
        }

        var json = JsonUtil.Load(this.fullFilePath);
        json.GetArray("Data", this.cuts, (e, i) =>
        {
            var cut = new Cut(e, this.uidGenerator.Generate());
            return new VmCut(cut, services);
        });

        Log.Info($"cutscene loading finished. {this.cuts.Count} cuts loaded.");
    }

    public IList<VmCut> Cuts => this.cuts;
    public IList SelectedCuts => this.selectedCuts;
    public ICommand BackCommand { get; }
    public ICommand SaveCommand { get; }
    public ICommand DeleteCommand { get; }
    public bool AllSectionUnit
    {
        get => this.allSectionUnit;
        set
        {
            this.SetProperty(ref this.allSectionUnit, value);
            foreach (var cut in this.cuts)
            {
                cut.ShowUnitSection = value;
            }
        }
    }

    public bool AllSectionScreen
    {
        get => this.allSectionScreen;
        set
        {
            this.SetProperty(ref this.allSectionScreen, value);
            foreach (var cut in this.cuts)
            {
                cut.ShowScreenSection = value;
            }
        }
    }

    public bool AllSectionCamera
    {
        get => this.allSectionCamera;
        set
        {
            this.SetProperty(ref this.allSectionCamera, value);
            foreach (var cut in this.cuts)
            {
                cut.ShowCameraSection = value;
            }
        }
    }

    bool IDragDropHandler.HandleDrop(object listViewContext, IList selectedItems, object targetContext)
    {
        if (targetContext is not VmCut dropTarget)
        {
            Log.Error("targetContext is not VmCut.");
            return false;
        }

        var items = selectedItems.Cast<VmCut>().ToList();

        var dropIndex = this.cuts.IndexOf(dropTarget);
        var itemsIndex = items.Select(this.cuts.IndexOf).OrderByDescending(i => i).ToList();
        if (itemsIndex.Count == 0)
        {
            return false;
        }

        // drop 대상이 items에 속해있으면 에러
        if (itemsIndex.Contains(dropIndex))
        {
            //Log.Error("drop target is in the moving items.");
            return false;
        }

        // itemsIndex가 연속적이지 않으면 에러
        if (itemsIndex.Count > 1)
        {
            var min = itemsIndex.Min();
            var max = itemsIndex.Max();
            if (max - min + 1 != itemsIndex.Count)
            {
                Log.Error("items are not continuous.");
                return false;
            }
        }

        var movingItems = new List<VmCut>();
        foreach (var i in itemsIndex)
        {
            movingItems.Add(this.cuts[i]);
            this.cuts.RemoveAt(i);
        }

        // 더 아래로 내리는 경우는 목적지 인덱스가 바뀔테니 재계산 필요
        if (dropIndex > itemsIndex.Max())
        {
            dropIndex -= itemsIndex.Count - 1;
        }

        foreach (var item in movingItems)
        {
            this.cuts.Insert(dropIndex, item);
        }

        return true;
    }

    async Task<bool> IClipboardHandler.HandlePastedTextAsync(string text)
    {
        text = text.Trim();

        if (string.IsNullOrEmpty(text))
        {
            return false;
        }

        var sb = new StringBuilder();
        var tokens = text.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        sb.AppendLine($"다음의 텍스트를 이용해 {tokens.Length}개의 cut 데이터를 생성합니다.");
        sb.AppendLine();
        if (text.Length > 10)
        {
            sb.AppendLine($"{text[..10]} ... (and {text.Length - 10} more)");
        }
        else
        {
            sb.AppendLine(text);
        }

        var boolProvider = this.services.GetRequiredService<IUserInputProvider<bool>>();
        if (await boolProvider.PromptAsync("새로운 Cut을 만듭니다", sb.ToString()) == false)
        {
            return false;
        }

        foreach (var token in tokens)
        {
            var cut = new Cut(this.uidGenerator.Generate());
            cut.UnitTalk.Korean = token;
            this.cuts.Add(new VmCut(cut, this.services));
        }

        return true;
    }

    //// --------------------------------------------------------------------------------------------

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);

        //switch (e.PropertyName)
        //{
        //    case nameof(this.SearchKeyword):
        //        this.filteredList.Refresh(this.searchKeyword);
        //        break;

        //    case nameof(this.SelectedCutScene):
        //        this.StartEditCommand.NotifyCanExecuteChanged();
        //        break;
        //}
    }

    private void OnBack()
    {
        WeakReferenceMessenger.Default.Send(new NavigationMessage("GoBack"));
    }

    private void OnSave()
    {
        Log.Debug($"SaveCommand. selected:{this.selectedCuts.Count}");
    }

    private void OnDelete()
    {
        if (this.selectedCuts.Count == 0)
        {
            return;
        }

        var selectedClone = this.selectedCuts.ToArray();
        foreach (var data in selectedClone)
        {
            this.cuts.Remove(data);
        }

        this.selectedCuts.Clear();
    }
}