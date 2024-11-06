﻿namespace CutEditor.ViewModel;

using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CutEditor.Model;
using CutEditor.Model.Interfaces;
using CutEditor.ViewModel.Detail;
using Du.Core.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using NKM;
using static CutEditor.Model.Enums;
using static CutEditor.ViewModel.Enums;

public sealed class VmCut : ObservableObject
{
    private readonly ChoiceUidGenerator choiceUidGenerator;
    private readonly IServiceProvider services;
    private bool showUnitSection;
    private bool showScreenSection;
    private bool showCameraSection;
    private CutDataType dataType;
    private bool screenCrashFlyoutOpen;
    private bool slateFlyoutOpen;

    public VmCut(Cut cut, IServiceProvider services)
    {
        this.Cut = cut;
        cut.PropertyChanged += this.Cut_PropertyChanged;
        this.dataType = cut.Choices.Count > 0
            ? CutDataType.Branch
            : CutDataType.Normal;

        this.services = services;
        this.PickUnitCommand = new AsyncRelayCommand(this.OnPickUnit);
        this.PickArcpointCommand = new AsyncRelayCommand(this.OnPickArcpoint);
        this.PickBgmACommand = new AsyncRelayCommand(this.OnPickBgmA);
        this.PickBgmBCommand = new AsyncRelayCommand(this.OnPickBgmB);
        this.PickSfxACommand = new AsyncRelayCommand(this.OnPickSfxA);
        this.PickSfxBCommand = new AsyncRelayCommand(this.OnPickSfxB);
        this.PickVoiceCommand = new AsyncRelayCommand(this.OnPickVoice);
        this.PickAmbientSoundCommand = new AsyncRelayCommand(this.OnPickAmbientSound);
        this.PickBgFileNameCommand = new AsyncRelayCommand(this.OnPickBgFileName);
        this.AddChoiceOptionCommand = new RelayCommand(this.OnAddChoiceOption, () => this.Cut.Choices.Count < 5);
        this.EditChoiceOptionCommand = new AsyncRelayCommand<ChoiceOption>(this.OnEditchoiceOption);
        this.DeleteChoiceOptionCommand = new RelayCommand<ChoiceOption>(this.OnDeleteChoiceOption, _ => this.Cut.Choices.Count > 1);
        this.SetAnchorCommand = new RelayCommand<DestAnchorType>(e => this.Cut.JumpAnchor = e);
        this.SetEmotionEffectCommand = new RelayCommand<EmotionEffect>(e => this.Cut.EmotionEffect = e);
        this.SetUnitMotionCommand = new RelayCommand<string>(this.OnSetUnitMotion);
        this.SetUnitNameStringCommand = new AsyncRelayCommand(this.OnSetUnitNameString);
        this.SetTransitionEffectCommand = new RelayCommand<TransitionEffect>(e => this.Cut.TransitionEffect = e);
        this.SetTransitionControlCommand = new RelayCommand<TransitionControl>(this.OnSetTransitionControl);
        this.SetAutoHighlightCommand = new RelayCommand<CutsceneAutoHighlight>(e => this.Cut.AutoHighlight = e);
        this.SetFilterTypeCommand = new RelayCommand<CutsceneFilterType>(e => this.Cut.FilterType = e);
        this.SetCutsceneClearCommand = new RelayCommand<CutsceneClearType>(e => this.Cut.CutsceneClear = e);
        this.OpenScreenCrashFlyoutCommand = new RelayCommand(() => this.ScreenCrashFlyoutOpen = true);
        this.ClearScreenFlashCrashCommand = new RelayCommand(this.OnClearScreenFlashCrash);
        this.OpenSlateFlyoutCommand = new RelayCommand(() => this.SlateFlyoutOpen = true);
        this.ClearSlateFlyoutCommand = new RelayCommand(this.OnClearSlateControl);
        this.SetStartFxLoopCommand = new RelayCommand<CutsceneSoundLoopControl>(e => this.Cut.StartFxLoopControl = e);
        this.SetEndFxLoopCommand = new RelayCommand<CutsceneSoundLoopControl>(e => this.Cut.EndFxLoopControl = e);
        this.EditBgFadeCommand = new AsyncRelayCommand(this.OnEditBgFade);
        this.SetTalkPositionControlCommand = new RelayCommand<TalkPositionControlType>(e => this.Cut.TalkPositionControl = e);

        this.showUnitSection = cut.HasUnitData();
        this.showScreenSection = cut.HasScreenBoxData();

        this.choiceUidGenerator = new(cut.Uid);
        this.choiceUidGenerator.Initialize(cut.Choices);

        if (this.Cut.Choices is ObservableCollection<ChoiceOption> choices)
        {
            choices.CollectionChanged += (s, e) =>
            {
                this.AddChoiceOptionCommand.NotifyCanExecuteChanged();
                this.DeleteChoiceOptionCommand.NotifyCanExecuteChanged();
            };
        }
    }

    public Cut Cut { get; }
    public IRelayCommand PickUnitCommand { get; }
    public ICommand PickArcpointCommand { get; }
    public ICommand PickBgmACommand { get; }
    public ICommand PickBgmBCommand { get; }
    public ICommand PickSfxACommand { get; }
    public ICommand PickSfxBCommand { get; }
    public ICommand PickVoiceCommand { get; }
    public ICommand PickAmbientSoundCommand { get; }
    public ICommand PickBgFileNameCommand { get; }
    public IRelayCommand AddChoiceOptionCommand { get; }
    public ICommand EditChoiceOptionCommand { get; }
    public IRelayCommand DeleteChoiceOptionCommand { get; }
    public ICommand SetAnchorCommand { get; }
    public ICommand SetEmotionEffectCommand { get; }
    public ICommand SetUnitMotionCommand { get; }
    public ICommand SetUnitNameStringCommand { get; }
    public ICommand SetTransitionEffectCommand { get; }
    public ICommand SetTransitionControlCommand { get; }
    public ICommand SetAutoHighlightCommand { get; }
    public ICommand SetFilterTypeCommand { get; }
    public ICommand SetCutsceneClearCommand { get; }
    public ICommand OpenScreenCrashFlyoutCommand { get; }
    public ICommand ClearScreenFlashCrashCommand { get; }
    public ICommand OpenSlateFlyoutCommand { get; }
    public ICommand ClearSlateFlyoutCommand { get; }
    public ICommand SetStartFxLoopCommand { get; }
    public ICommand SetEndFxLoopCommand { get; }
    public ICommand EditBgFadeCommand { get; }
    public ICommand SetTalkPositionControlCommand { get; }
    public bool ShowUnitSection
    {
        get => this.showUnitSection;
        set => this.SetProperty(ref this.showUnitSection, value);
    }

    public bool ShowScreenSection
    {
        get => this.showScreenSection;
        set => this.SetProperty(ref this.showScreenSection, value);
    }

    public bool ShowCameraSection
    {
        get => this.showCameraSection;
        set => this.SetProperty(ref this.showCameraSection, value);
    }

    public string SummaryText => this.Cut.GetSummaryText();

    public CutDataType DataType
    {
        get => this.dataType;
        set => this.SetProperty(ref this.dataType, value);
    }

    public bool ScreenCrashFlyoutOpen
    {
        get => this.screenCrashFlyoutOpen;
        set => this.SetProperty(ref this.screenCrashFlyoutOpen, value);
    }

    public bool SlateFlyoutOpen
    {
        get => this.slateFlyoutOpen;
        set => this.SetProperty(ref this.slateFlyoutOpen, value);
    }

    //// --------------------------------------------------------------------------------------------

    private void Cut_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(Model.Cut.EmotionEffect):
                break;
        }
    }

    private async Task OnPickUnit()
    {
        var unitpicker = this.services.GetRequiredService<IUnitPicker>();
        var result = await unitpicker.PickUnit();
        if (result.IsCanceled)
        {
            return;
        }

        this.Cut.Unit = result.Unit;
    }

    private async Task OnPickArcpoint()
    {
        var picker = this.services.GetRequiredService<IArcpointPicker>();
        var result = await picker.Pick();
        if (result.IsCanceled)
        {
            return;
        }

        this.Cut.Arcpoint = result.Data;
    }

    private async Task OnPickBgmA()
    {
        var bgmpicker = this.services.GetRequiredKeyedService<IAssetPicker>("bgm");
        var result = await bgmpicker.PickAsset();
        if (result.IsCanceled)
        {
            return;
        }

        this.Cut.StartBgmFileName = result.AssetFile;
    }

    private async Task OnPickBgmB()
    {
        var bgmpicker = this.services.GetRequiredKeyedService<IAssetPicker>("bgm");
        var result = await bgmpicker.PickAsset();
        if (result.IsCanceled)
        {
            return;
        }

        this.Cut.EndBgmFileName = result.AssetFile;
    }

    private async Task OnPickSfxA()
    {
        var sfxPicker = this.services.GetRequiredKeyedService<IAssetPicker>("sfx");
        var result = await sfxPicker.PickAsset();
        if (result.IsCanceled)
        {
            return;
        }

        this.Cut.StartFxSoundName = result.AssetFile;
    }

    private async Task OnPickSfxB()
    {
        var sfxPicker = this.services.GetRequiredKeyedService<IAssetPicker>("sfx");
        var result = await sfxPicker.PickAsset();
        if (result.IsCanceled)
        {
            return;
        }

        this.Cut.EndFxSoundName = result.AssetFile;
    }

    private async Task OnPickVoice()
    {
        var voicePicker = this.services.GetRequiredKeyedService<IAssetPicker>("voice");
        var result = await voicePicker.PickAsset();
        if (result.IsCanceled)
        {
            return;
        }

        this.Cut.TalkVoice = result.AssetFile;
    }

    private async Task OnPickAmbientSound()
    {
        var sfxPicker = this.services.GetRequiredKeyedService<IAssetPicker>("sfx");
        var result = await sfxPicker.PickAsset();
        if (result.IsCanceled)
        {
            return;
        }

        this.Cut.AmbientSound = result.AssetFile;
    }

    private async Task OnPickBgFileName()
    {
        var bgFilePicker = this.services.GetRequiredKeyedService<IAssetPicker>("bgFile");
        var result = await bgFilePicker.PickAsset(this.Cut.BgFileName);
        if (result.IsCanceled)
        {
            return;
        }

        this.Cut.BgFileName = result.AssetFile;
    }

    private void OnAddChoiceOption()
    {
        long newUid = this.choiceUidGenerator.Generate();
        var newChoice = new ChoiceOption();
        newChoice.InitializeUid(this.Cut.Uid, newUid);
        newChoice.Text.Korean = newChoice.UidString;

        this.Cut.Choices.Add(newChoice);
    }

    private void OnDeleteChoiceOption(ChoiceOption? target)
    {
        if (target is null)
        {
            throw new Exception($"remove target is null");
        }

        this.Cut.Choices.Remove(target);
    }

    private void OnSetUnitMotion(string? unitMotion)
    {
        this.Cut.UnitMotion = unitMotion;
    }

    private async Task OnSetUnitNameString()
    {
        var defaultValue = string.Join(", ", this.Cut.UnitNames);
        var userInputProvider = this.services.GetRequiredService<IUserInputProvider<string>>();
        var list = await userInputProvider.PromptAsync("유닛 이름을 입력하세요", "유닛 이름", defaultValue);

        this.Cut.UnitNames.Clear();
        if (string.IsNullOrWhiteSpace(list))
        {
            return;
        }

        foreach (var token in list.Split(','))
        {
            this.Cut.UnitNames.Add(token.Trim());
        }
    }

    private void OnSetTransitionControl(TransitionControl transitionControl)
    {
        if (transitionControl == TransitionControl.NONE)
        {
            this.Cut.TransitionEffect = null;
            this.Cut.TransitionControl = null;
            return;
        }

        this.Cut.TransitionControl = transitionControl;
    }

    private async Task OnEditchoiceOption(ChoiceOption? target)
    {
        if (target is null)
        {
            throw new Exception($"remove target is null");
        }

        IUserInputProvider<string> userInputProvider = this.services.GetRequiredService<IUserInputProvider<string>>();
        string defaultValue = target.Text.Korean;
        var result = await userInputProvider.PromptAsync("선택지 텍스트를 입력하세요", "선택지 텍스트", defaultValue);
        if (string.IsNullOrWhiteSpace(result))
        {
            return;
        }

        target.Text.Korean = result;
    }

    private void OnClearScreenFlashCrash()
    {
        this.Cut.BgFlashBang = 0f;
        this.Cut.BgCrash = 0f;
        this.Cut.BgCrashTime = 0f;
    }

    private void OnClearSlateControl()
    {
        this.Cut.SlateControlType = SlateControlType.NONE;
        this.Cut.SlateSectionNo = 0;
    }

    private async Task OnEditBgFade()
    {
        var editor = this.services.GetRequiredService<IModelEditor<BgFadeInOut>>();
        var result = await editor.EditAsync(this.Cut.BgFadeInOut);

        if (result.IsCanceled)
        {
            return;
        }

        this.Cut.BgFadeInOut = result.Data;
    }
}