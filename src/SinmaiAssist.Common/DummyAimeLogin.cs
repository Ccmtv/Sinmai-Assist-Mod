using System;
using System.IO;
using System.Text.RegularExpressions;
using AMDaemon;
using HarmonyLib;
using Mai2.Mai2Cue;
using Manager;
using Process.Entry;
using SinmaiAssist.GUI;
using SinmaiAssist.Utils;

namespace SinmaiAssist.Common;

public class DummyAimeLogin
{
	public static void ReadCard(string accesscode = null, string oldCode = null)
	{
		if (accesscode == null)
		{
			accesscode = DummyLoginPanel.DummyLoginCode.Trim();
		}
		if (!Directory.Exists("DEVICE"))
		{
			Directory.CreateDirectory("DEVICE");
		}
		if (accesscode.Length == 20 && Regex.IsMatch(accesscode, "^\\d+$"))
		{
			File.WriteAllText("DEVICE/aime.txt", accesscode);
			Keyboard.LongPressKey(Keys.Return, 300);
			if (oldCode != null && oldCode.Length == 20 && Regex.IsMatch(oldCode, "^\\d+$"))
			{
				File.WriteAllText("DEVICE/aime.txt", oldCode);
			}
		}
		else
		{
			GameMessageManager.SendMessage(0, "<color=\"red\">Failed to read Aime!");
			GameMessageManager.SendMessage(1, "<color=\"red\">Failed to read Aime!");
			SoundManager.PlaySE(Cue.SE_ENTRY_AIME_ERROR, 1);
		}
	}

	[HarmonyPrefix]
	[HarmonyPatch(typeof(AimeId), "Value", MethodType.Getter)]
	public static bool GetAimeId(ref uint __result)
	{
		if (DummyLoginPanel.UserIdLoginFlag)
		{
			__result = Convert.ToUInt32(DummyLoginPanel.DummyUserId);
			return false;
		}
		return true;
	}

	[HarmonyPostfix]
	[HarmonyPatch(typeof(TryAime), "Execute")]
	public static void ClearFlag()
	{
		DummyLoginPanel.UserIdLoginFlag = false;
	}
}
