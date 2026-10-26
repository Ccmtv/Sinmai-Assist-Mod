using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using MelonLoader;
using Process;

namespace SinmaiAssist.Common;

public class ChangeFadeStyle
{
	[HarmonyTranspiler]
	[HarmonyPatch(typeof(FadeProcess), "OnStart")]
	public static IEnumerable<CodeInstruction> TranspilerFade(IEnumerable<CodeInstruction> instructions)
	{
		List<CodeInstruction> list = new List<CodeInstruction>(instructions);
		bool flag = false;
		for (int i = 0; i < list.Count - 1; i++)
		{
			if (list[i].opcode == OpCodes.Ldstr && list[i].operand.ToString().StartsWith("Process/ChangeScreen/Prefabs/ChangeScreen_"))
			{
				string text = "02";
				if (list[i].operand.ToString().EndsWith("02"))
				{
					text = "01";
				}
				list[i].operand = "Process/ChangeScreen/Prefabs/ChangeScreen_" + text;
				flag = true;
				break;
			}
		}
		if (!flag)
		{
			MelonLogger.Warning("Failed to patch ChangeFadeStyle, Method Not Found!");
		}
		return list;
	}

	[HarmonyTranspiler]
	[HarmonyPatch(typeof(NextTrackProcess), "OnStart")]
	public static IEnumerable<CodeInstruction> TranspilerNextTrack(IEnumerable<CodeInstruction> instructions)
	{
		List<CodeInstruction> list = new List<CodeInstruction>(instructions);
		bool flag = false;
		for (int i = 0; i < list.Count - 1; i++)
		{
			if (list[i].opcode == OpCodes.Ldstr && list[i].operand.ToString().StartsWith("Process/ChangeScreen/Prefabs/ChangeScreen_"))
			{
				string text = "02";
				if (list[i].operand.ToString().EndsWith("02"))
				{
					text = "01";
				}
				list[i].operand = "Process/ChangeScreen/Prefabs/ChangeScreen_" + text;
				flag = true;
				break;
			}
		}
		if (!flag)
		{
			MelonLogger.Warning("Failed to patch ChangeFadeStyle, Method Not Found!");
		}
		return list;
	}
}
