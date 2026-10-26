using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using MAI2.Util;
using Manager;
using Manager.MaiStudio;
using Manager.UserDatas;

namespace SinmaiAssist.Cheat;

public class AllCollection
{
	[HarmonyPostfix]
	[HarmonyPatch(typeof(UserData), "get_FrameList")]
	public static void FrameList(ref List<UserItem> __result, CollectionProcess __instance)
	{
		List<int> list = (from i in __result
			where i.stock > 0
			select i.itemId).ToList();
		foreach (KeyValuePair<int, FrameData> frame in Singleton<DataManager>.Instance.GetFrames())
		{
			if (!list.Contains(frame.Value.GetID()))
			{
				list.Add(frame.Value.GetID());
				__result.Add(new UserItem(frame.Value.GetID()));
			}
		}
	}

	[HarmonyPostfix]
	[HarmonyPatch(typeof(UserData), "get_IconList")]
	public static void IconList(ref List<UserItem> __result, CollectionProcess __instance)
	{
		List<int> list = (from i in __result
			where i.stock > 0
			select i.itemId).ToList();
		foreach (KeyValuePair<int, IconData> icon in Singleton<DataManager>.Instance.GetIcons())
		{
			if (!list.Contains(icon.Value.GetID()))
			{
				list.Add(icon.Value.GetID());
				__result.Add(new UserItem(icon.Value.GetID()));
			}
		}
	}

	[HarmonyPostfix]
	[HarmonyPatch(typeof(UserData), "get_PlateList")]
	public static void PlateList(ref List<UserItem> __result, CollectionProcess __instance)
	{
		List<int> list = (from i in __result
			where i.stock > 0
			select i.itemId).ToList();
		foreach (KeyValuePair<int, PlateData> plate in Singleton<DataManager>.Instance.GetPlates())
		{
			if (!list.Contains(plate.Value.GetID()))
			{
				list.Add(plate.Value.GetID());
				__result.Add(new UserItem(plate.Value.GetID()));
			}
		}
	}

	[HarmonyPostfix]
	[HarmonyPatch(typeof(UserData), "get_PartnerList")]
	public static void PartnerList(ref List<UserItem> __result, CollectionProcess __instance)
	{
		List<int> list = (from i in __result
			where i.stock > 0
			select i.itemId).ToList();
		foreach (KeyValuePair<int, PartnerData> partner in Singleton<DataManager>.Instance.GetPartners())
		{
			if (!list.Contains(partner.Value.GetID()))
			{
				list.Add(partner.Value.GetID());
				__result.Add(new UserItem(partner.Value.GetID()));
			}
		}
	}

	[HarmonyPostfix]
	[HarmonyPatch(typeof(UserData), "get_TitleList")]
	public static void TitleList(ref List<UserItem> __result, CollectionProcess __instance)
	{
		List<int> list = (from i in __result
			where i.stock > 0
			select i.itemId).ToList();
		foreach (KeyValuePair<int, TitleData> title in Singleton<DataManager>.Instance.GetTitles())
		{
			if (!list.Contains(title.Value.GetID()))
			{
				list.Add(title.Value.GetID());
				__result.Add(new UserItem(title.Value.GetID()));
			}
		}
	}
}
