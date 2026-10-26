using System;
using System.Reflection;
using DB;
using HarmonyLib;
using MAI2.Util;
using Manager;
using Manager.MaiStudio;
using Manager.UserDatas;
using MelonLoader;
using Monitor;
using Process;

namespace SinmaiAssist.Cheat;

public class ForceCurrentIsBest
{
	[HarmonyPostfix]
	[HarmonyPatch(typeof(ResultProcess), "OnStart")]
	public static void Postfix(ResultProcess __instance)
	{
		try
		{
			FieldInfo fieldInfo = AccessTools.Field(typeof(ResultProcess), "_userData");
			FieldInfo fieldInfo2 = AccessTools.Field(typeof(ResultProcess), "_userScores");
			FieldInfo fieldInfo3 = AccessTools.Field(typeof(ResultProcess), "_musicID");
			FieldInfo fieldInfo4 = AccessTools.Field(typeof(ResultProcess), "_isNewRecord");
			FieldInfo fieldInfo5 = AccessTools.Field(typeof(ResultProcess), "_monitors");
			UserData[] array = (UserData[])fieldInfo.GetValue(__instance);
			UserScore[] array2 = (UserScore[])fieldInfo2.GetValue(__instance);
			int num = (int)fieldInfo3.GetValue(__instance);
			bool[] array3 = (bool[])fieldInfo4.GetValue(__instance);
			ResultMonitor[] array4 = (ResultMonitor[])fieldInfo5.GetValue(__instance);
			MusicData music = Singleton<DataManager>.Instance.GetMusic(num);
			if (music == null)
			{
				return;
			}
			for (int i = 0; i < array.Length; i++)
			{
				if (array[i] == null)
				{
					continue;
				}
				int num2 = GameManager.SelectDifficultyID[i];
				if (array[i].ScoreDic[num2].TryGetValue(num, out var value))
				{
					bool flag = music.utagePlayStyle == UtagePlayStyle.DoublePlayerScore;
					uint achivement = value.achivement;
					uint deluxscore = value.deluxscore;
					value.achivement = array2[i].achivement;
					value.combo = array2[i].combo;
					value.sync = array2[i].sync;
					value.deluxscore = array2[i].deluxscore;
					value.scoreRank = GameManager.GetClearRank((int)value.achivement, flag);
					int num3 = (flag ? 2020000 : 1010000);
					if (array2[i].achivement >= num3)
					{
						value.extNum1++;
					}
					else
					{
						value.extNum1 = 0u;
					}
					array3[i] = true;
					int fluctuation = (int)(value.deluxscore - deluxscore);
					int num4 = music.notesData[num2].maxNotes * 3;
					DeluxcorerankrateID deluxcoreRank = GameManager.GetDeluxcoreRank((num4 > 0) ? ((int)(value.deluxscore * 100) / num4) : 0);
					array4[i].SetDxScore(value.deluxscore, fluctuation, num4, deluxcoreRank);
					array4[i].SetMyBestAchievement(achivement, value.achivement - achivement, isNewRecord: true);
				}
			}
		}
		catch (Exception ex)
		{
			MelonLogger.Error("ForceCurrentIsBestMoudleError: " + ex.Message);
		}
	}
}
