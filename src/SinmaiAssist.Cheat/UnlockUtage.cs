using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using MAI2System;
using Manager;
using Manager.MaiStudio;
using SinmaiAssist.Utils;

namespace SinmaiAssist.Cheat;

public class UnlockUtage
{
	private enum ForceUtageModeType
	{
		Normal,
		Force1P,
		Force2P
	}

	private static List<int> _needDoublePlayerMusicIdList = new List<int>();

	private static ForceUtageModeType _forceUtageMode = ForceUtageModeType.Normal;

	private static Dictionary<ForceUtageModeType, string> _utagePathMap = new Dictionary<ForceUtageModeType, string>
	{
		{
			ForceUtageModeType.Normal,
			""
		},
		{
			ForceUtageModeType.Force1P,
			"_L.ma2"
		},
		{
			ForceUtageModeType.Force2P,
			"_R.ma2"
		}
	};

	[HarmonyPrefix]
	[HarmonyPatch(typeof(GameManager), "CanUnlockUtageTotalJudgement")]
	public static bool ForceUnlockUtage(out ConstParameter.ResultOfUnlockUtageJudgement result1P, out ConstParameter.ResultOfUnlockUtageJudgement result2P)
	{
		result1P = ConstParameter.ResultOfUnlockUtageJudgement.Unlocked;
		result2P = ConstParameter.ResultOfUnlockUtageJudgement.Unlocked;
		return false;
	}

	[HarmonyPostfix]
	[HarmonyPatch(typeof(GameScoreList), "InitalizeUtage")]
	public static void InitalizeUtage(GameScoreList __instance, int monitorIndex)
	{
		if (!_needDoublePlayerMusicIdList.Contains(__instance.SessionInfo.musicId))
		{
			return;
		}
		if (_forceUtageMode == ForceUtageModeType.Normal)
		{
			if (monitorIndex == 0)
			{
				__instance.SessionInfo.dPScoreFilePathAdd = "_L.ma2";
			}
			if (monitorIndex == 1)
			{
				__instance.SessionInfo.dPScoreFilePathAdd = "_R.ma2";
			}
		}
		else
		{
			__instance.SessionInfo.dPScoreFilePathAdd = _utagePathMap[_forceUtageMode];
		}
	}

	[HarmonyPostfix]
	[HarmonyPatch(typeof(MusicData), "Init")]
	public static void ForceSetUtagePlayStyle(MusicData __instance)
	{
		if (SinmaiAssist.MainConfig.Cheat.UnlockUtage.UnlockDoublePlayerMusic && __instance.utagePlayStyle == UtagePlayStyle.DoublePlayerScore)
		{
			typeof(MusicData).GetProperty("utagePlayStyle", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).SetValue(__instance, UtagePlayStyle.SinglePlayerScore);
			_needDoublePlayerMusicIdList.Add(__instance.GetID());
		}
	}

	public static void SwitchUtageMode()
	{
		_forceUtageMode = (ForceUtageModeType)((int)(_forceUtageMode + 1) % 3);
		GameMessageManager.SendMessage(0, "Force Utage Mode: \n" + _forceUtageMode);
		GameMessageManager.SendMessage(1, "Force Utage Mode: \n" + _forceUtageMode);
	}
}
