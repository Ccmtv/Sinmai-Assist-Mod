using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;
using HarmonyLib;
using MAI2.Util;
using Manager;
using Monitor;
using SinmaiAssist.Config;
using UnityEngine;
using CustomAutoPlayConfig = SinmaiAssist.Config.MainConfig.CustomAutoPlayConfig;

namespace SinmaiAssist.Cheat;

public class AutoPlay
{
    public enum AutoPlayMode
    {
        None,
        Critical,
        Perfect,
        Great,
        Good,
        Random,
        RandomAllPerfect,
        RandomFullComboPlus,
        RandomFullCombo
    }

    private sealed class SongPlan
    {
        public AutoPlayMode Mode;

        public int Revision;

        public int NoteCount;

        public int BreakCount;

        public readonly Dictionary<int, NoteJudge.ETiming> Timings = new Dictionary<int, NoteJudge.ETiming>();
    }

    private static readonly ReadOnlyCollection<NoteJudge.ETiming> RandCriticalTiming = Array.AsReadOnly(new NoteJudge.ETiming[5]
    {
        NoteJudge.ETiming.FastPerfect2nd,
        NoteJudge.ETiming.FastPerfect,
        NoteJudge.ETiming.Critical,
        NoteJudge.ETiming.LatePerfect,
        NoteJudge.ETiming.LatePerfect2nd
    });

    private static readonly ReadOnlyCollection<NoteJudge.ETiming> RandomJudgeTiming = Array.AsReadOnly(new NoteJudge.ETiming[10]
    {
        NoteJudge.ETiming.Critical,
        NoteJudge.ETiming.FastPerfect,
        NoteJudge.ETiming.FastPerfect2nd,
        NoteJudge.ETiming.LatePerfect,
        NoteJudge.ETiming.LatePerfect2nd,
        NoteJudge.ETiming.LateGreat2nd,
        NoteJudge.ETiming.FastGreat2nd,
        NoteJudge.ETiming.LateGood,
        NoteJudge.ETiming.FastGood,
        NoteJudge.ETiming.TooLate
    });

    private static readonly NoteJudge.ETiming[] SmallPerfectTimings = new NoteJudge.ETiming[4]
    {
        NoteJudge.ETiming.FastPerfect2nd,
        NoteJudge.ETiming.FastPerfect,
        NoteJudge.ETiming.LatePerfect,
        NoteJudge.ETiming.LatePerfect2nd
    };

    private static readonly NoteJudge.ETiming[] GreatTimings = new NoteJudge.ETiming[2]
    {
        NoteJudge.ETiming.FastGreat,
        NoteJudge.ETiming.LateGreat
    };

    private static readonly NoteJudge.ETiming[] GoodTimings = new NoteJudge.ETiming[2]
    {
        NoteJudge.ETiming.FastGood,
        NoteJudge.ETiming.LateGood
    };

    private static readonly Dictionary<int, SongPlan> Plans = new Dictionary<int, SongPlan>();

    private static readonly FieldInfo SlideNoteIndexField = AccessTools.Field(typeof(SlideRoot), "NoteIndex");

    private static int _planRevision;

    private static int _planSeed;

    private static bool _hasPending;

    private static NoteJudge.ETiming _pendingTiming;

    public static AutoPlayMode autoPlayMode = AutoPlayMode.None;

    public static bool DisableUpdate = false;

    /// <summary>
    /// true  = 原版全自动（旧行为）：自动打所有 note、星星自动滑、触摸被屏蔽。
    /// false = 仅判定模式（当前行为）：自己打，命中后只改判定，星星要自己滑。
    /// </summary>
    public static bool ClassicMode = false;

    public static bool IsAutoPlay()
    {
        return autoPlayMode != AutoPlayMode.None;
    }

    /// <summary>
    /// 设置改变后清空本曲的判定方案，下一首歌会按新数量重新生成。
    /// 当前歌曲中途修改数量时，后续音符也会按新方案重新分配。
    /// </summary>
    public static void InvalidatePlans()
    {
        _planRevision++;
        Plans.Clear();
        _hasPending = false;
    }

    private static bool IsCustomMode(AutoPlayMode mode)
    {
        return mode == AutoPlayMode.Random
            || mode == AutoPlayMode.RandomAllPerfect
            || mode == AutoPlayMode.RandomFullComboPlus
            || mode == AutoPlayMode.RandomFullCombo;
    }

    private static CustomAutoPlayConfig GetCustomConfig()
    {
        var cheat = SinmaiAssist.MainConfig?.Cheat;
        if (cheat == null)
        {
            return null;
        }
        if (cheat.CustomAutoPlay == null)
        {
            cheat.CustomAutoPlay = new CustomAutoPlayConfig();
        }
        return cheat.CustomAutoPlay;
    }

    private static bool GetIncludeBreak(AutoPlayMode mode, CustomAutoPlayConfig config)
    {
        return mode switch
        {
            AutoPlayMode.RandomAllPerfect => config.AllPerfectIncludeBreak,
            AutoPlayMode.RandomFullComboPlus => config.FullComboPlusIncludeBreak,
            AutoPlayMode.RandomFullCombo => config.FullComboIncludeBreak,
            AutoPlayMode.Random => config.RandomIncludeBreak,
            _ => false
        };
    }

    private static void GetCounts(AutoPlayMode mode, CustomAutoPlayConfig config, out int perfect, out int great, out int good, out int miss)
    {
        perfect = 0;
        great = 0;
        good = 0;
        miss = 0;
        switch (mode)
        {
        case AutoPlayMode.RandomAllPerfect:
            perfect = config.AllPerfectSmallPerfect;
            break;
        case AutoPlayMode.RandomFullComboPlus:
            perfect = config.FullComboPlusSmallPerfect;
            great = config.FullComboPlusGreat;
            break;
        case AutoPlayMode.RandomFullCombo:
            perfect = config.FullComboSmallPerfect;
            great = config.FullComboGreat;
            good = config.FullComboGood;
            break;
        case AutoPlayMode.Random:
            perfect = config.RandomSmallPerfect;
            great = config.RandomGreat;
            good = config.RandomGood;
            miss = config.RandomMiss;
            break;
        }
        perfect = Math.Max(0, perfect);
        great = Math.Max(0, great);
        good = Math.Max(0, good);
        miss = Math.Max(0, miss);
    }

    private static bool IsBreakNote(NoteData note)
    {
        NotesTypeID.Def type = note.type.getEnum();
        return type.isBreak() || type.isExBreak();
    }

    private static NoteJudge.ETiming PickSmallPerfect(System.Random random)
    {
        return SmallPerfectTimings[random.Next(SmallPerfectTimings.Length)];
    }

    private static NoteJudge.ETiming PickGreat(System.Random random)
    {
        return GreatTimings[random.Next(GreatTimings.Length)];
    }

    private static NoteJudge.ETiming PickGood(System.Random random)
    {
        return GoodTimings[random.Next(GoodTimings.Length)];
    }

    private static bool IsSlideNote(NoteData note)
    {
        return note.type.getEnum().isSlide();
    }

    private static void Shuffle<T>(List<T> list, System.Random random)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = random.Next(i + 1);
            T swap = list[i];
            list[i] = list[j];
            list[j] = swap;
        }
    }

    private static bool TryBuildPlan(NotesManager manager)
    {
        if (manager == null)
        {
            return false;
        }
        int monitorId = manager.GetPlayerID();
        Plans.Remove(monitorId);
        if (!IsCustomMode(autoPlayMode))
        {
            return false;
        }
        CustomAutoPlayConfig config = GetCustomConfig();
        if (config == null)
        {
            return false;
        }
        NoteDataList noteData = manager.getReader()?.GetNoteList();
        if (noteData == null || noteData.Count == 0)
        {
            return false;
        }

        List<NoteData> notes = new List<NoteData>(noteData.Count);
        int breakCount = 0;
        foreach (NoteData note in noteData)
        {
            NotesTypeID.Def type = note.type.getEnum();
            if (!type.isValid() || type == NotesTypeID.Def.ConnectSlide)
            {
                continue;
            }
            notes.Add(note);
            if (IsBreakNote(note))
            {
                breakCount++;
            }
        }
        if (notes.Count == 0)
        {
            return false;
        }

        bool includeBreak = GetIncludeBreak(autoPlayMode, config);
        List<NoteData> pool = new List<NoteData>(notes.Count);
        List<NoteData> perfectPool = new List<NoteData>(notes.Count);
        foreach (NoteData note in notes)
        {
            if (!includeBreak && IsBreakNote(note))
            {
                continue;
            }
            pool.Add(note);
            if (!IsSlideNote(note))
            {
                perfectPool.Add(note);
            }
        }

        GetCounts(autoPlayMode, config, out int perfect, out int great, out int good, out int miss);
        int remaining = pool.Count;
        perfect = Math.Min(perfect, perfectPool.Count);
        remaining -= perfect;
        great = Math.Min(great, remaining);
        remaining -= great;
        good = Math.Min(good, remaining);
        remaining -= good;
        miss = Math.Min(miss, remaining);

        System.Random random = new System.Random(unchecked(Environment.TickCount * 31 + monitorId * 977 + _planRevision * 7919 + _planSeed++));

        SongPlan plan = new SongPlan
        {
            Mode = autoPlayMode,
            Revision = _planRevision,
            NoteCount = notes.Count,
            BreakCount = breakCount
        };

        Shuffle(perfectPool, random);
        HashSet<int> used = new HashSet<int>();
        for (int i = 0; i < perfect; i++)
        {
            NoteData note = perfectPool[i];
            plan.Timings[note.indexNote] = PickSmallPerfect(random);
            used.Add(note.indexNote);
        }

        List<NoteData> otherPool = new List<NoteData>(pool.Count - perfect);
        foreach (NoteData note in pool)
        {
            if (!used.Contains(note.indexNote))
            {
                otherPool.Add(note);
            }
        }
        Shuffle(otherPool, random);

        int index = 0;
        for (int i = 0; i < great; i++)
        {
            plan.Timings[otherPool[index++].indexNote] = PickGreat(random);
        }
        for (int i = 0; i < good; i++)
        {
            plan.Timings[otherPool[index++].indexNote] = PickGood(random);
        }
        for (int i = 0; i < miss; i++)
        {
            plan.Timings[otherPool[index++].indexNote] = NoteJudge.ETiming.TooLate;
        }
        foreach (NoteData note in notes)
        {
            if (!plan.Timings.ContainsKey(note.indexNote))
            {
                plan.Timings[note.indexNote] = NoteJudge.ETiming.Critical;
            }
        }

        Plans[monitorId] = plan;
        return true;
    }

    private static bool TryBuildPlan(int monitorId)
    {
        return TryBuildPlan(NotesManager.Instance(monitorId));
    }

    private static bool TryGetPlannedTiming(int monitorId, int noteIndex, out NoteJudge.ETiming timing)
    {
        timing = NoteJudge.ETiming.Critical;
        if (!IsCustomMode(autoPlayMode))
        {
            return false;
        }
        if (!Plans.TryGetValue(monitorId, out SongPlan plan) || plan.Mode != autoPlayMode || plan.Revision != _planRevision)
        {
            if (!TryBuildPlan(monitorId) || !Plans.TryGetValue(monitorId, out plan))
            {
                return false;
            }
        }
        if (!plan.Timings.TryGetValue(noteIndex, out timing))
        {
            return false;
        }
        return true;
    }

    private static NoteJudge.ETiming GetTimingForNote(NoteBase note)
    {
        if (IsCustomMode(autoPlayMode) && TryGetPlannedTiming(note.MonitorId, note.GetNoteIndex(), out NoteJudge.ETiming planned))
        {
            return planned;
        }
        if (note.ExNote && (autoPlayMode == AutoPlayMode.RandomAllPerfect || autoPlayMode == AutoPlayMode.RandomFullComboPlus || autoPlayMode == AutoPlayMode.RandomFullCombo))
        {
            return NoteJudge.ETiming.Critical;
        }
        return GameManager.AutoJudge();
    }

    private static NoteJudge.ETiming GetTimingForSlide(SlideRoot slide)
    {
        if (IsCustomMode(autoPlayMode) && SlideNoteIndexField != null)
        {
            int noteIndex = (int)SlideNoteIndexField.GetValue(slide);
            if (TryGetPlannedTiming(slide.MonitorId, noteIndex, out NoteJudge.ETiming planned))
            {
                return planned;
            }
        }
        if (autoPlayMode == AutoPlayMode.RandomAllPerfect || autoPlayMode == AutoPlayMode.RandomFullComboPlus || autoPlayMode == AutoPlayMode.RandomFullCombo)
        {
            return NoteJudge.ETiming.Critical;
        }
        return GameManager.AutoJudge();
    }

    private static void BeginPending(int monitorId, int noteIndex)
    {
        _hasPending = false;
        if (!IsCustomMode(autoPlayMode))
        {
            return;
        }
        if (TryGetPlannedTiming(monitorId, noteIndex, out NoteJudge.ETiming timing))
        {
            _pendingTiming = timing;
            _hasPending = true;
        }
    }

    private static void BeginPending(NoteBase note)
    {
        if (note == null)
        {
            _hasPending = false;
            return;
        }
        BeginPending(note.MonitorId, note.GetNoteIndex());
    }

    private static void BeginPending(SlideRoot slide)
    {
        if (slide == null || SlideNoteIndexField == null)
        {
            _hasPending = false;
            return;
        }
        BeginPending(slide.MonitorId, (int)SlideNoteIndexField.GetValue(slide));
    }

    private static void EndPending()
    {
        _hasPending = false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(GameManager), "IsAutoPlay")]
    public static bool SetIsAutoPlay(ref bool __result)
    {
        if (ClassicMode)
        {
            __result = IsAutoPlay();
            return false;
        }
        if (IsAutoPlay())
        {
            __result = false;
            return false;
        }
        return true;
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(GameManager), "set_AutoPlay")]
    public static void AutoPlayUpdate()
    {
        GameManager.AutoPlayMode autoPlay = GameManager.AutoPlay;
        if (!DisableUpdate)
        {
            autoPlayMode = (AutoPlayMode)autoPlay;
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(NotesManager), "loadScore")]
    public static void NotesManagerLoadScore(NotesManager __instance)
    {
        if (__instance == null)
        {
            return;
        }
        Plans.Remove(__instance.GetPlayerID());
        TryBuildPlan(__instance);
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(NotesManager), "reset")]
    public static void NotesManagerReset(NotesManager __instance)
    {
        if (__instance != null)
        {
            Plans.Remove(__instance.GetPlayerID());
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(NotesManager), "StartPlay")]
    public static void NotesManagerStartPlay()
    {
        Plans.Clear();
        _hasPending = false;
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(GameManager), "AutoJudge")]
    public static void AutoJudge(ref NoteJudge.ETiming __result)
    {
        if (_hasPending)
        {
            __result = _pendingTiming;
            return;
        }
        int num = UnityEngine.Random.Range(0, 1000);
        int num2 = 2;
        if (num < 350)
        {
            num2 += UnityEngine.Random.Range(-2, 3);
        }
        switch (autoPlayMode)
        {
        case AutoPlayMode.Critical:
            __result = NoteJudge.ETiming.Critical;
            break;
        case AutoPlayMode.Perfect:
            __result = ((UnityEngine.Random.Range(0, 2) == 0) ? NoteJudge.ETiming.LatePerfect2nd : NoteJudge.ETiming.FastPerfect2nd);
            break;
        case AutoPlayMode.Great:
            __result = ((UnityEngine.Random.Range(0, 2) == 0) ? NoteJudge.ETiming.LateGreat : NoteJudge.ETiming.FastGreat);
            break;
        case AutoPlayMode.Good:
            __result = ((UnityEngine.Random.Range(0, 2) != 0) ? NoteJudge.ETiming.FastGood : NoteJudge.ETiming.LateGood);
            break;
        case AutoPlayMode.Random:
            __result = RandomJudgeTiming[UnityEngine.Random.Range(0, RandomJudgeTiming.Count)];
            break;
        case AutoPlayMode.RandomAllPerfect:
            __result = RandCriticalTiming[num2];
            break;
        case AutoPlayMode.RandomFullComboPlus:
            if (num >= 10)
            {
                __result = RandCriticalTiming[num2];
            }
            else if (UnityEngine.Random.Range(0, 2) == 1)
            {
                __result = NoteJudge.ETiming.LateGreat;
            }
            else
            {
                __result = NoteJudge.ETiming.FastGreat;
            }
            break;
        case AutoPlayMode.RandomFullCombo:
            if (num >= 80)
            {
                __result = RandCriticalTiming[num2];
            }
            else if (num >= 20)
            {
                if (UnityEngine.Random.Range(0, 2) == 1)
                {
                    __result = NoteJudge.ETiming.LateGreat;
                }
                else
                {
                    __result = NoteJudge.ETiming.FastGreat;
                }
            }
            else if (UnityEngine.Random.Range(0, 2) == 1)
            {
                __result = NoteJudge.ETiming.LateGood;
            }
            else
            {
                __result = NoteJudge.ETiming.FastGood;
            }
            break;
        default:
            __result = NoteJudge.ETiming.TooFast;
            break;
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(NoteBase), "SetAutoPlayJudge")]
    public static bool NoteBaseAutoPlayJudge(NoteBase __instance)
    {
        if (!ClassicMode && !(__instance is TouchNoteB))
        {
            return true;
        }
        if (!IsAutoPlay())
        {
            return true;
        }
        float appearMsec = (float)AccessTools.Field(typeof(NoteBase), "AppearMsec").GetValue(__instance);
        MethodInfo playJudgeSe = AccessTools.Method(typeof(NoteBase), "PlayJudgeSe");
        if (NotesManager.GetCurrentMsec() > appearMsec - 4.1666665f && IsAutoPlay())
        {
            NoteJudge.ETiming timing = GetTimingForNote(__instance);
            AccessTools.Field(typeof(NoteBase), "JudgeResult").SetValue(__instance, timing);
            playJudgeSe.Invoke(__instance, null);
        }
        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(NoteBase), "Judge")]
    public static void NoteBaseJudgePrefix(NoteBase __instance)
    {
        BeginPending(__instance);
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(NoteBase), "Judge")]
    public static void NoteBaseJudgePostfix()
    {
        EndPending();
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(SlideRoot), "Judge")]
    public static bool SlideRootJudge(SlideRoot __instance, ref bool __result)
    {
        if (!IsAutoPlay())
        {
            return true;
        }
        if (!(bool)AccessTools.Method(typeof(SlideRoot), "IsNoteCheckTimeStart").Invoke(__instance, new object[1] { Singleton<GamePlayManager>.Instance.GetGameScore(__instance.MonitorId).UserOption.GetJudgeTimingFrame() }))
        {
            __result = false;
            return false;
        }
        FieldInfo judgeTimingDiffMsec = AccessTools.Field(typeof(SlideRoot), "JudgeTimingDiffMsec");
        FieldInfo judgeResult = AccessTools.Field(typeof(SlideRoot), "JudgeResult");
        FieldInfo tailMsec = AccessTools.Field(typeof(SlideRoot), "TailMsec");
        FieldInfo lastWaitTime = AccessTools.Field(typeof(SlideRoot), "lastWaitTime");
        float diffMsec = NotesManager.GetCurrentMsec() - (float)tailMsec.GetValue(__instance) + (float)lastWaitTime.GetValue(__instance);
        judgeTimingDiffMsec.SetValue(__instance, diffMsec);
        NoteJudge.ETiming timing = GetTimingForSlide(__instance);
        if (timing != NoteJudge.ETiming.End)
        {
            if (timing == NoteJudge.ETiming.TooFast)
            {
                timing = NoteJudge.ETiming.FastGood;
            }
            judgeResult.SetValue(__instance, timing);
            __result = true;
            return false;
        }
        __result = false;
        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(SlideRoot), "NoteCheck")]
    public static void SlideRootNoteCheckPrefix(SlideRoot __instance)
    {
        if (ClassicMode)
        {
            BeginPending(__instance);
        }
        else
        {
            EndPending();
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(SlideRoot), "NoteCheck")]
    public static void SlideRootNoteCheckPostfix()
    {
        EndPending();
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(SlideRoot), "PlayJudgeSe")]
    public static void SlideRootPlayJudgeSePrefix(SlideRoot __instance)
    {
        if (!IsCustomMode(autoPlayMode) || SlideNoteIndexField == null)
        {
            return;
        }
        int noteIndex = (int)SlideNoteIndexField.GetValue(__instance);
        if (TryGetPlannedTiming(__instance.MonitorId, noteIndex, out NoteJudge.ETiming timing) && timing == NoteJudge.ETiming.TooLate)
        {
            AccessTools.Field(typeof(SlideRoot), "JudgeResult").SetValue(__instance, timing);
        }
    }

    /// <summary>
    /// 原版全自动模式专用：玩家触摸 touch note 时的判定处理。
    /// </summary>
    [HarmonyPrefix]
    [HarmonyPatch(typeof(TouchNoteB), "Judge")]
    public static bool TouchNoteBJudge(TouchNoteB __instance, ref bool __result)
    {
        if (!ClassicMode)
        {
            BeginPending(__instance);
            return true;
        }
        _hasPending = false;
        if (!IsAutoPlay())
        {
            return true;
        }
        if ((bool)AccessTools.Method(typeof(NoteBase), "IsNoteCheckTimeStart").Invoke(__instance, new object[1] { Singleton<GamePlayManager>.Instance.GetGameScore(__instance.MonitorId).UserOption.GetJudgeTimingFrame() }))
        {
            __result = false;
            return false;
        }
        FieldInfo judgeTimingDiffMsec = AccessTools.Field(typeof(NoteBase), "JudgeTimingDiffMsec");
        FieldInfo judgeResult = AccessTools.Field(typeof(NoteBase), "JudgeResult");
        FieldInfo appearMsec = AccessTools.Field(typeof(NoteBase), "AppearMsec");
        FieldInfo judgeType = AccessTools.Field(typeof(NoteBase), "JudgeType");
        FieldInfo buttonId = AccessTools.Field(typeof(NoteBase), "ButtonId");
        FieldInfo touchArea = AccessTools.Field(typeof(TouchNoteB), "TouchArea");
        MethodInfo playJudgeSe = AccessTools.Method(typeof(TouchNoteB), "PlayJudgeSe");
        float diffMsec = NotesManager.GetCurrentMsec() - (float)appearMsec.GetValue(__instance);
        judgeTimingDiffMsec.SetValue(__instance, diffMsec);
        NoteJudge.ETiming timing = NoteJudge.GetJudgeTiming(ref diffMsec, Singleton<GamePlayManager>.Instance.GetGameScore(__instance.MonitorId).UserOption.GetJudgeTimingFrame(), (NoteJudge.EJudgeType)judgeType.GetValue(__instance));
        if (IsCustomMode(autoPlayMode))
        {
            timing = GetTimingForNote(__instance);
        }
        else if (autoPlayMode == AutoPlayMode.RandomAllPerfect || autoPlayMode == AutoPlayMode.RandomFullComboPlus || autoPlayMode == AutoPlayMode.RandomFullCombo)
        {
            timing = NoteJudge.ETiming.Critical;
        }
        judgeResult.SetValue(__instance, timing);
        TouchSensorType sensorType = (TouchSensorType)touchArea.GetValue(__instance);
        if (timing != NoteJudge.ETiming.End)
        {
            playJudgeSe.Invoke(__instance, null);
            int num = (int)buttonId.GetValue(__instance);
            switch (sensorType)
            {
            case TouchSensorType.B:
                InputManager.SetUsedThisFrame(__instance.MonitorId, (InputManager.TouchPanelArea)(8 + num));
                break;
            case TouchSensorType.E:
                InputManager.SetUsedThisFrame(__instance.MonitorId, (InputManager.TouchPanelArea)(26 + num));
                break;
            case TouchSensorType.A:
                InputManager.SetUsedThisFrame(__instance.MonitorId, (InputManager.TouchPanelArea)num);
                break;
            case TouchSensorType.D:
                InputManager.SetUsedThisFrame(__instance.MonitorId, (InputManager.TouchPanelArea)(18 + num));
                break;
            case TouchSensorType.C:
                InputManager.SetUsedThisFrame(__instance.MonitorId, InputManager.TouchPanelArea.C1);
                InputManager.SetUsedThisFrame(__instance.MonitorId, InputManager.TouchPanelArea.C2);
                break;
            }
            __result = true;
            return false;
        }
        return false;
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(TouchNoteB), "Judge")]
    public static void TouchNoteBJudgePostfix()
    {
        EndPending();
    }

    /// <summary>
    /// 原版全自动模式专用：随机模式下的 touch note 补判。
    /// </summary>
    [HarmonyPostfix]
    [HarmonyPatch(typeof(TouchNoteB), "NoteCheck")]
    public static void NoteCheck(TouchNoteB __instance)
    {
        if (!ClassicMode)
        {
            return;
        }
        if (!IsAutoPlay())
        {
            return;
        }
        FieldInfo judgeResult = AccessTools.Field(typeof(NoteBase), "JudgeResult");
        MethodInfo playJudgeSe = AccessTools.Method(typeof(TouchNoteB), "PlayJudgeSe");
        float appearMsec = (float)AccessTools.Field(typeof(NoteBase), "AppearMsec").GetValue(__instance);
        if (NotesManager.GetCurrentMsec() > appearMsec - 4.1666665f && IsAutoPlay())
        {
            if (IsCustomMode(autoPlayMode))
            {
                judgeResult.SetValue(__instance, GetTimingForNote(__instance));
                playJudgeSe.Invoke(__instance, null);
            }
            else if (autoPlayMode == AutoPlayMode.RandomAllPerfect || autoPlayMode == AutoPlayMode.RandomFullComboPlus || autoPlayMode == AutoPlayMode.RandomFullCombo)
            {
                judgeResult.SetValue(__instance, NoteJudge.ETiming.Critical);
                playJudgeSe.Invoke(__instance, null);
            }
        }
    }

    private static void ApplyPlannedHoldResult(NoteBase note)
    {
        if (ClassicMode || !IsCustomMode(autoPlayMode))
        {
            return;
        }
        if (!TryGetPlannedTiming(note.MonitorId, note.GetNoteIndex(), out NoteJudge.ETiming timing))
        {
            return;
        }
        if (timing != NoteJudge.ETiming.TooLate)
        {
            NoteJudge.ETiming head;
            if (note is HoldNote hold)
            {
                head = hold.GetJudgeHeadResult();
            }
            else if (note is BreakHoldNote breakHold)
            {
                head = breakHold.GetJudgeHeadResult();
            }
            else if (note is TouchHoldC touchHold)
            {
                head = touchHold.GetJudgeHeadResult();
            }
            else
            {
                return;
            }
            if (head == NoteJudge.ETiming.End || head == NoteJudge.ETiming.TooLate || head == NoteJudge.ETiming.TooFast)
            {
                return;
            }
        }
        AccessTools.Field(typeof(NoteBase), "JudgeResult").SetValue(note, timing);
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(HoldNote), "SetAutoPlayJudge")]
    public static void HoldNoteSetAutoPlayJudgePrefix(HoldNote __instance)
    {
        if (ClassicMode)
        {
            BeginPending(__instance);
        }
        else
        {
            EndPending();
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(HoldNote), "SetAutoPlayJudge")]
    public static void HoldNoteSetAutoPlayJudgePostfix()
    {
        EndPending();
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(HoldNote), "JudgeTotalResult")]
    public static void HoldNoteJudgeTotalResultPrefix(HoldNote __instance)
    {
        if (ClassicMode)
        {
            BeginPending(__instance);
        }
        else
        {
            EndPending();
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(HoldNote), "JudgeTotalResult")]
    public static void HoldNoteJudgeTotalResultPostfix(HoldNote __instance)
    {
        ApplyPlannedHoldResult(__instance);
        EndPending();
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(BreakHoldNote), "SetAutoPlayJudge")]
    public static void BreakHoldNoteSetAutoPlayJudgePrefix(BreakHoldNote __instance)
    {
        if (ClassicMode)
        {
            BeginPending(__instance);
        }
        else
        {
            EndPending();
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(BreakHoldNote), "SetAutoPlayJudge")]
    public static void BreakHoldNoteSetAutoPlayJudgePostfix()
    {
        EndPending();
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(BreakHoldNote), "JudgeTotalResult")]
    public static void BreakHoldNoteJudgeTotalResultPrefix(BreakHoldNote __instance)
    {
        if (ClassicMode)
        {
            BeginPending(__instance);
        }
        else
        {
            EndPending();
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(BreakHoldNote), "JudgeTotalResult")]
    public static void BreakHoldNoteJudgeTotalResultPostfix(BreakHoldNote __instance)
    {
        ApplyPlannedHoldResult(__instance);
        EndPending();
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(TouchHoldC), "SetAutoPlayJudge")]
    public static void TouchHoldCSetAutoPlayJudgePrefix(TouchHoldC __instance)
    {
        if (ClassicMode)
        {
            BeginPending(__instance);
        }
        else
        {
            EndPending();
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(TouchHoldC), "SetAutoPlayJudge")]
    public static void TouchHoldCSetAutoPlayJudgePostfix()
    {
        EndPending();
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(TouchHoldC), "JudgeTotalResult")]
    public static void TouchHoldCJudgeTotalResultPrefix(TouchHoldC __instance)
    {
        if (ClassicMode)
        {
            BeginPending(__instance);
        }
        else
        {
            EndPending();
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(TouchHoldC), "JudgeTotalResult")]
    public static void TouchHoldCJudgeTotalResultPostfix(TouchHoldC __instance)
    {
        ApplyPlannedHoldResult(__instance);
        EndPending();
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(HoldNote), "JudgeHoldHead")]
    public static void HoldNoteJudgeHoldHeadPrefix(HoldNote __instance)
    {
        BeginPending(__instance);
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(HoldNote), "JudgeHoldHead")]
    public static void HoldNoteJudgeHoldHeadPostfix()
    {
        EndPending();
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(BreakHoldNote), "JudgeHoldHead")]
    public static void BreakHoldNoteJudgeHoldHeadPrefix(BreakHoldNote __instance)
    {
        BeginPending(__instance);
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(BreakHoldNote), "JudgeHoldHead")]
    public static void BreakHoldNoteJudgeHoldHeadPostfix()
    {
        EndPending();
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(TouchHoldC), "JudgeHoldHead")]
    public static void TouchHoldCJudgeHoldHeadPrefix(TouchHoldC __instance)
    {
        BeginPending(__instance);
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(TouchHoldC), "JudgeHoldHead")]
    public static void TouchHoldCJudgeHoldHeadPostfix()
    {
        EndPending();
    }

    /// <summary>
    /// 仅判定模式专用：玩家实际命中时（tap / hold 头 / break）把判定改成当前模式的结果。
    /// End / TooLate / TooFast 保持原样，所以没打中依然会 Miss。
    /// </summary>
    [HarmonyPostfix]
    [HarmonyPatch(typeof(NoteJudge), "GetJudgeTiming")]
    public static void JudgeTimingPostfix(ref NoteJudge.ETiming __result)
    {
        if (ClassicMode)
        {
            return;
        }
        if (!IsAutoPlay())
        {
            return;
        }
        if (_hasPending && __result != NoteJudge.ETiming.End && __result != NoteJudge.ETiming.TooLate && __result != NoteJudge.ETiming.TooFast)
        {
            __result = _pendingTiming;
            return;
        }
        if (IsCustomMode(autoPlayMode))
        {
            return;
        }
        NoteJudge.ETiming timing = __result;
        if (timing != NoteJudge.ETiming.End && timing != NoteJudge.ETiming.TooLate && timing != NoteJudge.ETiming.TooFast)
        {
            __result = GameManager.AutoJudge();
        }
    }
}
