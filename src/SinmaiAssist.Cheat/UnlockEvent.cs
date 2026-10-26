using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using HarmonyLib;
using Net.Packet;
using Net.Packet.Mai2;
using Net.VO;
using Net.VO.Mai2;

namespace SinmaiAssist.Cheat;

public class UnlockEvent
{
	[HarmonyPostfix]
	[HarmonyPatch(typeof(PacketGetGameEvent), "Proc")]
	public static void Unlock(PacketGetGameEvent __instance, ref PacketState __result)
	{
		if (__result == PacketState.Done)
		{
			NetQuery<GameEventRequestVO, GameEventResponseVO> netQuery = __instance.Query as NetQuery<GameEventRequestVO, GameEventResponseVO>;
			List<GameEvent> list = new List<GameEvent>();
			ReadAllEvents("./Sinmai_Data/StreamingAssets/", ref list);
			if (Directory.Exists("./option"))
			{
				ReadAllEvents("./option", ref list);
			}
			netQuery.Response.gameEventList = list.ToArray();
			((Action<GameEvent[]>)typeof(PacketGetGameEvent).GetField("_onDone", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(__instance))?.Invoke(netQuery.Response.gameEventList ?? Array.Empty<GameEvent>());
		}
	}

	private static void ReadAllEvents(string path, ref List<GameEvent> list)
	{
		foreach (DirectoryInfo item in new DirectoryInfo(path).EnumerateDirectories("*", SearchOption.AllDirectories))
		{
			string path2 = Path.Combine(item.FullName, "event");
			if (!int.TryParse(item.Name.Substring(1), out var _) || !Directory.Exists(path2))
			{
				continue;
			}
			foreach (DirectoryInfo item2 in new DirectoryInfo(path2).EnumerateDirectories("*", SearchOption.AllDirectories))
			{
				if (item2.Name.StartsWith("event") && int.TryParse(item2.Name.Replace("event", ""), out var result2) && result2 > 0)
				{
					list.Add(new GameEvent
					{
						id = result2,
						startDate = "2000-01-01 00:00:00",
						endDate = "2077-07-21 11:45:14",
						type = 1
					});
				}
			}
		}
	}
}
