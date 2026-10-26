using System;
using System.Collections.ObjectModel;
using System.Reflection;
using HarmonyLib;
using MAI2.Util;
using Manager;
using Monitor;
using UnityEngine;

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
    [HarmonyPatch(typeof(GameManager), "AutoJudge")]
    public static void AutoJudge(ref NoteJudge.ETiming __result)
    {
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
        float num = (float)AccessTools.Field(typeof(NoteBase), "AppearMsec").GetValue(__instance);
        bool flag = (bool)AccessTools.Field(typeof(NoteBase), "IsExNote").GetValue(__instance);
        MethodInfo methodInfo = AccessTools.Method(typeof(NoteBase), "PlayJudgeSe");
        if (NotesManager.GetCurrentMsec() > num - 4.1666665f && IsAutoPlay())
        {
            if ((autoPlayMode == AutoPlayMode.RandomAllPerfect || autoPlayMode == AutoPlayMode.RandomFullCombo || autoPlayMode == AutoPlayMode.RandomFullComboPlus) && flag)
            {
                AccessTools.Field(typeof(NoteBase), "JudgeResult").SetValue(__instance, NoteJudge.ETiming.Critical);
            }
            else
            {
                AccessTools.Field(typeof(NoteBase), "JudgeResult").SetValue(__instance, GameManager.AutoJudge());
            }
            methodInfo.Invoke(__instance, null);
        }
        return false;
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
        FieldInfo fieldInfo = AccessTools.Field(typeof(SlideRoot), "JudgeTimingDiffMsec");
        FieldInfo fieldInfo2 = AccessTools.Field(typeof(SlideRoot), "JudgeResult");
        FieldInfo fieldInfo3 = AccessTools.Field(typeof(SlideRoot), "TailMsec");
        FieldInfo fieldInfo4 = AccessTools.Field(typeof(SlideRoot), "lastWaitTime");
        FieldInfo fieldInfo5 = AccessTools.Field(typeof(SlideRoot), "JudgeType");
        FieldInfo fieldInfo6 = AccessTools.Field(typeof(SlideRoot), "lastWaitTimeForJudge");
        float _fMsec = NotesManager.GetCurrentMsec() - (float)fieldInfo3.GetValue(__instance) + (float)fieldInfo4.GetValue(__instance);
        fieldInfo.SetValue(__instance, _fMsec);
        NoteJudge.ETiming eTiming = (NoteJudge.ETiming)fieldInfo2.GetValue(__instance);
        eTiming = (IsAutoPlay() ? GameManager.AutoJudge() : NoteJudge.GetSlideJudgeTiming(ref _fMsec, Singleton<GamePlayManager>.Instance.GetGameScore(__instance.MonitorId).UserOption.GetJudgeTimingFrame(), (NoteJudge.EJudgeType)fieldInfo5.GetValue(__instance), (int)fieldInfo6.GetValue(__instance)));
        if (autoPlayMode == AutoPlayMode.RandomAllPerfect || autoPlayMode == AutoPlayMode.RandomFullComboPlus || autoPlayMode == AutoPlayMode.RandomFullCombo)
        {
            eTiming = NoteJudge.ETiming.Critical;
        }
        if (eTiming != NoteJudge.ETiming.End)
        {
            if (eTiming == NoteJudge.ETiming.TooFast)
            {
                eTiming = NoteJudge.ETiming.FastGood;
            }
            fieldInfo2.SetValue(__instance, eTiming);
            __result = true;
            return false;
        }
        __result = false;
        return false;
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
            return true;
        }
        if (!IsAutoPlay())
        {
            return true;
        }
        if ((bool)AccessTools.Method(typeof(NoteBase), "IsNoteCheckTimeStart").Invoke(__instance, new object[1] { Singleton<GamePlayManager>.Instance.GetGameScore(__instance.MonitorId).UserOption.GetJudgeTimingFrame() }))
        {
            __result = false;
            return false;
        }
        FieldInfo fieldInfo = AccessTools.Field(typeof(NoteBase), "JudgeTimingDiffMsec");
        FieldInfo fieldInfo2 = AccessTools.Field(typeof(NoteBase), "JudgeResult");
        FieldInfo fieldInfo3 = AccessTools.Field(typeof(NoteBase), "AppearMsec");
        FieldInfo fieldInfo4 = AccessTools.Field(typeof(NoteBase), "JudgeType");
        FieldInfo fieldInfo5 = AccessTools.Field(typeof(NoteBase), "ButtonId");
        FieldInfo fieldInfo6 = AccessTools.Field(typeof(TouchNoteB), "TouchArea");
        MethodInfo methodInfo = AccessTools.Method(typeof(TouchNoteB), "PlayJudgeSe");
        float _fMsec = NotesManager.GetCurrentMsec() - (float)fieldInfo3.GetValue(__instance);
        fieldInfo.SetValue(__instance, _fMsec);
        NoteJudge.ETiming eTiming = (NoteJudge.ETiming)fieldInfo2.GetValue(__instance);
        eTiming = NoteJudge.GetJudgeTiming(ref _fMsec, Singleton<GamePlayManager>.Instance.GetGameScore(__instance.MonitorId).UserOption.GetJudgeTimingFrame(), (NoteJudge.EJudgeType)fieldInfo4.GetValue(__instance));
        if (autoPlayMode == AutoPlayMode.RandomAllPerfect || autoPlayMode == AutoPlayMode.RandomFullComboPlus || autoPlayMode == AutoPlayMode.RandomFullCombo)
        {
            eTiming = NoteJudge.ETiming.Critical;
        }
        fieldInfo2.SetValue(__instance, eTiming);
        TouchSensorType touchSensorType = (TouchSensorType)fieldInfo6.GetValue(__instance);
        if (eTiming != NoteJudge.ETiming.End)
        {
            methodInfo.Invoke(__instance, null);
            int num = (int)fieldInfo5.GetValue(__instance);
            switch (touchSensorType)
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
        if (IsAutoPlay())
        {
            FieldInfo fieldInfo = AccessTools.Field(typeof(NoteBase), "JudgeResult");
            MethodInfo methodInfo = AccessTools.Method(typeof(TouchNoteB), "PlayJudgeSe");
            float num = (float)AccessTools.Field(typeof(NoteBase), "AppearMsec").GetValue(__instance);
            if ((autoPlayMode == AutoPlayMode.RandomAllPerfect || autoPlayMode == AutoPlayMode.RandomFullComboPlus || autoPlayMode == AutoPlayMode.RandomFullCombo) && NotesManager.GetCurrentMsec() > num - 4.1666665f && IsAutoPlay())
            {
                fieldInfo.SetValue(__instance, NoteJudge.ETiming.Critical);
                methodInfo.Invoke(__instance, null);
            }
        }
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
        NoteJudge.ETiming eTiming = __result;
        if (eTiming != NoteJudge.ETiming.End && eTiming != NoteJudge.ETiming.TooLate && eTiming != NoteJudge.ETiming.TooFast)
        {
            __result = GameManager.AutoJudge();
        }
    }
}
