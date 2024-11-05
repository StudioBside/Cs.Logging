﻿namespace CutEditor.Model.Detail;

using System.Drawing;
using NKM;
using static CutEditor.Model.Enums;

internal class CutOutputFormat
{
    public long Uid { get; set; }
    public string? ContentsTag { get; set; }
    public string? CutsceneStrId { get; set; }
    public DestAnchorType? JumpAnchorInfo { get; set; }
    public DestAnchorType? RewardAnchor { get; set; }
    public bool WaitClick { get; set; }
    public float WaitTime { get; set; }
    public int[]? BgFadeInStartCol { get; set; }
    public int[]? BgFadeInCol { get; set; }
    public float? BgFadeInTime { get; set; }
    public int[]? BgFadeOutCol { get; set; }
    public float? BgFadeOutTime { get; set; }
    public float? BgFlashBang { get; set; }
    public float? BgCrash { get; set; }
    public float? BgCrashTime { get; set; }
    public string? EndBgmFileName { get; set; }
    public string? BgFileName { get; set; }
    public string? StartBgmFileName { get; set; }
    public string? StartFxSoundName { get; set; }
    public CutsceneClearType? CutsceneClear { get; set; }
    public string? UnitStrId { get; set; }
    public string[]? UnitNameString { get; set; }
    public string? CameraOffset { get; set; }
    public string? CameraOffsetTime { get; set; }
    public string? UnitMotion { get; set; }
    public bool? UnitQuickSet { get; set; }
    public CutsceneUnitPos? UnitPos { get; set; }
    public EmotionEffect? EmotionEffect { get; set; }
    public string? UnitTalk_KOR { get; set; }
    public string? UnitTalk_ENG { get; set; }
    public string? UnitTalk_JPN { get; set; }
    public string? UnitTalk_CHN { get; set; }
    public float? TalkTime { get; set; }
    public int[]? TalkPositionControl { get; set; }
    public ChoiceOutputFormat[]? JumpAnchorData { get; set; }
    public bool? TalkAppend { get; set; }
    public TransitionEffect? TransitionEffect { get; set; }
    public TransitionControl? TransitionControl { get; set; }
    public string? TalkVoice { get; set; }
    public float? BgChangeTime { get; set; }
    public CutsceneAutoHighlight? AutoHighlight { get; set; }
    public CutsceneFilterType? FilterType { get; set; }
    public int? ArcpointId { get; set; }
    public CutsceneSoundLoopControl? StartFxSoundLoopControl { get; set; }
    public CutsceneSoundLoopControl? EndFxLoopControl { get; set; }

    public SlateControlType? SlateControlType { get; set; }
    public int? SlateSectionNo { get; set; }
    
    internal static float? EliminateZero(float source)
    {
        return Math.Abs(source) < 0.0001f
            ? null
            : source;
    }

    internal static int[]? ConvertColor(Color? color)
    {
        if (color is null)
        {
            return null;
        }

        return [
            color.Value.R,
            color.Value.G,
            color.Value.B,
            color.Value.A
        ];
    }
}