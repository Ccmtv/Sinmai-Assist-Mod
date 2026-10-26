using System;
using System.Reflection;
using HarmonyLib;
using MAI2.Util;
using Manager;
using Manager.UserDatas;
using MelonLoader;
using Monitor;
using Process;
using UnityEngine;

namespace SinmaiAssist.Cheat;

internal class FastSkip
{
	private enum GameSequence
	{
		Init,
		Sync,
		Start,
		StartWait,
		Play,
		PlayEnd,
		Result,
		ResultEnd,
		FinalWait,
		Release
	}

	public static bool CustomSkip;

	public static bool SkipButton;

	public static bool Force1Miss;

	public static int CustomAchivement;

	private static bool _isSkip;

	private static bool _Miss;

	[HarmonyPostfix]
	[HarmonyPatch(typeof(GameProcess), "OnUpdate")]
	public static void Skip(GameProcess __instance)
	{
		try
		{
			System.Type baseType = typeof(GameProcess).BaseType;
			GameSequence gameSequence = (GameSequence)typeof(GameProcess).GetField("_sequence", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(__instance);
			MethodInfo method = typeof(GameProcess).GetMethod("UpdateSubbMonitorData", BindingFlags.Instance | BindingFlags.NonPublic);
			MethodInfo method2 = typeof(GameProcess).GetMethod("SetRelease", BindingFlags.Instance | BindingFlags.NonPublic);
			MethodInfo method3 = typeof(GameProcess).GetMethod("IsPartyPlay", BindingFlags.Instance | BindingFlags.NonPublic);
			ProcessDataContainer processDataContainer = (ProcessDataContainer)baseType.GetField("container", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(__instance);
			if (gameSequence < GameSequence.Play || gameSequence >= GameSequence.Release || GameManager.IsNoteCheckMode)
			{
				return;
			}
			_isSkip = false;
			if (!DebugInput.GetKeyDown(KeyCode.Space) && !SkipButton)
			{
				return;
			}
			_isSkip = true;
			GameMonitor[] array = (GameMonitor[])typeof(GameProcess).GetField("_monitors", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(__instance);
			if (CustomSkip)
			{
				for (int i = 0; i < array.Length; i++)
				{
					array[i].Seek(0);
				}
				NotesManager.StartPlay(0f);
				NotesManager.Pause(isPause: true);
				bool partyPlay = (bool)method3.Invoke(__instance, null);
				Singleton<GamePlayManager>.Instance.Initialize(partyPlay);
				uint num = 0u;
				for (int j = 0; j < array.Length; j++)
				{
					if (Singleton<UserDataManager>.Instance.GetUserData(j).IsEntry)
					{
						array[j].ForceAchivement(CustomAchivement, 0);
						num += Singleton<GamePlayManager>.Instance.GetGameScore(j).MaxCombo;
					}
				}
				GameScoreList gameScore = Singleton<GamePlayManager>.Instance.GetGameScore(2);
				if (gameScore.IsEnable && !gameScore.IsHuman())
				{
					for (int k = 0; k < 2; k++)
					{
						if (Singleton<UserDataManager>.Instance.GetUserData(k).IsEntry && GameManager.SelectGhostID[k] != GhostManager.GhostTarget.MapGhost1_2P)
						{
							UserGhost ghostToEnum = Singleton<GhostManager>.Instance.GetGhostToEnum(GameManager.SelectGhostID[k]);
							gameScore.SetForceAchivement_Battle((float)GameManager.ConvAchiveIntToDecimal(ghostToEnum.Achievement));
							break;
						}
					}
				}
				for (int l = 0; l < array.Length; l++)
				{
					if (Singleton<UserDataManager>.Instance.GetUserData(l).IsEntry)
					{
						Singleton<GamePlayManager>.Instance.GetGameScore(l).SetChain(num);
					}
				}
			}
			if (!_isSkip)
			{
				return;
			}
			for (int m = 0; m < array.Length; m++)
			{
				if (Singleton<UserDataManager>.Instance.GetUserData(m).IsEntry)
				{
					method.Invoke(__instance, new object[1] { m });
					Message[] array2 = (Message[])typeof(GameProcess).GetField("_message", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(__instance);
					processDataContainer.processManager.SendMessage(array2[m]);
					Singleton<GamePlayManager>.Instance.SetSyncResult(m);
				}
			}
			method2.Invoke(__instance, null);
			SkipButton = false;
		}
		catch (Exception obj)
		{
			MelonLogger.Error(obj);
		}
	}

	[HarmonyPrefix]
	[HarmonyPatch(typeof(GameScoreList), "SetForceAchivement")]
	public static bool SetForceAchivement(int achivement, int dxscore, GameScoreList __instance)
	{
		decimal num = achivement;
		if (num >= 100.0m && num <= 100.4m)
		{
			num = 100.3m;
		}
		long num2;
		long num3;
		if (num > 100.0m)
		{
			num2 = (long)((decimal)__instance.ScoreTotal._allPerfectScore * (num - 1.0m) * 0.01m);
			num3 = __instance.ScoreTotal._breakBonusScore;
		}
		else
		{
			num2 = (long)((decimal)__instance.ScoreTotal._allPerfectScore * (num * 0.99m * 0.01m));
			num3 = (long)((decimal)__instance.ScoreTotal._breakBonusScore * num * 0.01m);
		}
		NoteJudge.ETiming[] array = new NoteJudge.ETiming[7]
		{
			NoteJudge.ETiming.Critical,
			NoteJudge.ETiming.FastGreat,
			NoteJudge.ETiming.FastGreat2nd,
			NoteJudge.ETiming.LateGreat,
			NoteJudge.ETiming.LateGreat2nd,
			NoteJudge.ETiming.LateGreat3rd,
			NoteJudge.ETiming.LateGood
		};
		NoteDataList noteList = NotesManager.Instance((int)typeof(GameScoreList).GetField("_monitorIndex", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(__instance)).getReader().GetNoteList();
		foreach (NoteData item in noteList)
		{
			if (!item.type.isBreakScore())
			{
				continue;
			}
			bool flag = false;
			NoteJudge.ETiming[] array2 = array;
			foreach (NoteJudge.ETiming eTiming in array2)
			{
				NoteScore.EScoreType eScoreType = GamePlayManager.NoteType2ScoreType(item.type);
				if (0m <= (decimal)(num2 - NoteScore.GetJudgeScore(eTiming, NoteScore.EScoreType.Break)) && 0m <= (decimal)(num3 - NoteScore.GetJudgeScore(eTiming, NoteScore.EScoreType.BreakBonus)))
				{
					num2 -= NoteScore.GetJudgeScore(eTiming, eScoreType);
					num3 -= NoteScore.GetJudgeScore(eTiming, NoteScore.EScoreType.BreakBonus);
					__instance.SetResult(item.indexNote, eScoreType, eTiming);
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				__instance.SetResult(item.indexNote, NoteScore.EScoreType.Break, NoteJudge.ETiming.TooFast);
				_Miss = true;
			}
		}
		int num4 = 0;
		int num5 = 0;
		long num6 = 0L;
		for (int j = 0; j < array.Length; j++)
		{
			long num7 = num2;
			long num8 = 0L;
			num8 += __instance.ScoreTotal.GetTapNum() * NoteScore.GetJudgeScore(array[j]);
			num8 += __instance.ScoreTotal.GetHoldNum() * NoteScore.GetJudgeScore(array[j], NoteScore.EScoreType.Hold);
			num8 += __instance.ScoreTotal.GetSlideNum() * NoteScore.GetJudgeScore(array[j], NoteScore.EScoreType.Slide);
			num8 += __instance.ScoreTotal.GetTouchNum() * NoteScore.GetJudgeScore(array[j], NoteScore.EScoreType.Touch);
			if (num8 <= num7)
			{
				num6 = num7 - num8;
				num5 = ((num4 != 0) ? (num4 - 1) : 0);
				break;
			}
			num4++;
		}
		if (num4 >= array.Length)
		{
			num4 = array.Length - 1;
		}
		foreach (NoteData item2 in noteList)
		{
			if (item2.type.isSlideScore())
			{
				NoteScore.EScoreType eScoreType2 = GamePlayManager.NoteType2ScoreType(item2.type);
				NoteJudge.ETiming eTiming2 = array[num4];
				NoteJudge.ETiming eTiming3 = array[num5];
				if (0m <= (decimal)num6 && 0m <= (decimal)(num2 - NoteScore.GetJudgeScore(eTiming3, eScoreType2)))
				{
					num6 -= NoteScore.GetJudgeScore(eTiming3, eScoreType2) - NoteScore.GetJudgeScore(eTiming2, eScoreType2);
					num2 -= NoteScore.GetJudgeScore(eTiming3, eScoreType2);
					__instance.SetResult(item2.indexNote, eScoreType2, eTiming3);
				}
				else if (0m <= (decimal)(num2 - NoteScore.GetJudgeScore(eTiming2, eScoreType2)))
				{
					num2 -= NoteScore.GetJudgeScore(eTiming2, eScoreType2);
					__instance.SetResult(item2.indexNote, eScoreType2, eTiming2);
				}
				else
				{
					__instance.SetResult(item2.indexNote, eScoreType2, NoteJudge.ETiming.TooFast);
					_Miss = true;
				}
			}
		}
		foreach (NoteData item3 in noteList)
		{
			if (item3.type.isHoldScore())
			{
				NoteScore.EScoreType eScoreType3 = GamePlayManager.NoteType2ScoreType(item3.type);
				NoteJudge.ETiming eTiming4 = array[num4];
				NoteJudge.ETiming eTiming5 = array[num5];
				if (0m <= (decimal)num6 && 0m <= (decimal)(num2 - NoteScore.GetJudgeScore(eTiming5, eScoreType3)))
				{
					num6 -= NoteScore.GetJudgeScore(eTiming5, eScoreType3) - NoteScore.GetJudgeScore(eTiming4, eScoreType3);
					num2 -= NoteScore.GetJudgeScore(eTiming5, eScoreType3);
					__instance.SetResult(item3.indexNote, eScoreType3, eTiming5);
				}
				else if (0m <= (decimal)(num2 - NoteScore.GetJudgeScore(eTiming4, eScoreType3)))
				{
					num2 -= NoteScore.GetJudgeScore(eTiming4, eScoreType3);
					__instance.SetResult(item3.indexNote, eScoreType3, eTiming4);
				}
				else
				{
					__instance.SetResult(item3.indexNote, eScoreType3, NoteJudge.ETiming.TooFast);
					_Miss = true;
				}
			}
		}
		foreach (NoteData item4 in noteList)
		{
			if (item4.type.isTapScore())
			{
				NoteScore.EScoreType eScoreType4 = GamePlayManager.NoteType2ScoreType(item4.type);
				NoteJudge.ETiming eTiming6 = array[num4];
				NoteJudge.ETiming eTiming7 = array[num5];
				if (Force1Miss && !_Miss)
				{
					__instance.SetResult(item4.indexNote, eScoreType4, NoteJudge.ETiming.TooFast);
					_Miss = true;
				}
				else if (0m < (decimal)num6 && 0m <= (decimal)(num2 - NoteScore.GetJudgeScore(eTiming7, eScoreType4)))
				{
					num6 -= NoteScore.GetJudgeScore(eTiming7, eScoreType4) - NoteScore.GetJudgeScore(eTiming6, eScoreType4);
					num2 -= NoteScore.GetJudgeScore(eTiming7, eScoreType4);
					__instance.SetResult(item4.indexNote, eScoreType4, eTiming7);
				}
				else if (0m <= (decimal)(num2 - NoteScore.GetJudgeScore(eTiming6, eScoreType4)))
				{
					num2 -= NoteScore.GetJudgeScore(eTiming6, eScoreType4);
					__instance.SetResult(item4.indexNote, eScoreType4, eTiming6);
				}
				else if (0m < (decimal)num2)
				{
					num2 -= NoteScore.GetJudgeScore(eTiming6, eScoreType4);
					__instance.SetResult(item4.indexNote, eScoreType4, eTiming6);
				}
				else
				{
					__instance.SetResult(item4.indexNote, eScoreType4, NoteJudge.ETiming.TooFast);
					_Miss = true;
				}
			}
		}
		return false;
	}
}


