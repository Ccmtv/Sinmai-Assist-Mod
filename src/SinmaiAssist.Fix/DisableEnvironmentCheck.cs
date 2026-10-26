using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using MelonLoader;
using Process;
using SinmaiAssist.Attributes;

namespace SinmaiAssist.Fix;

[EnableGameVersion(25000u, 99999u)]
public class DisableEnvironmentCheck
{
	[HarmonyTranspiler]
	[HarmonyPatch(typeof(WarningProcess), "OnStart")]
	public static IEnumerable<CodeInstruction> OnStart(IEnumerable<CodeInstruction> instructions)
	{
		List<CodeInstruction> list = instructions.ToList();
		int num = list.FindIndex((CodeInstruction inst) => inst.opcode == OpCodes.Ldsfld && inst.operand is FieldInfo fieldInfo && fieldInfo.Name == "OnceDisp");
		if (num == -1)
		{
			MelonLogger.Warning("[Disable Environment Check] Failed to patch DisableEnvironmentCheck, Method Not Found!");
			return list;
		}
		return list.Skip(num);
	}
}
