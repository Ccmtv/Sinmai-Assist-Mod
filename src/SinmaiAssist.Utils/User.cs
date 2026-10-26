using System;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using MAI2.Util;
using Manager;
using Manager.UserDatas;
using MelonLoader.TinyJSON;
using Net.VO.Mai2;

namespace SinmaiAssist.Utils;

public class User
{
	public static string ExportBackupData(long index, string fileName = null, bool sendMessage = true)
	{
		UserData userData = Singleton<UserDataManager>.Instance.GetUserData(index);
		if (userData.IsGuest())
		{
			if (sendMessage)
			{
				GameMessageManager.SendMessage((int)index, "Guest Account\nUnable to export backup");
			}
			return null;
		}
		string text = "Sinmai-Assist/UserBackup";
		string text2 = "{";
		text2 = text2 + "\"accessCode\": \"" + userData.Detail.AccessCode + "\",";
		text2 += $"\"userId\": {userData.Detail.UserID},";
		text2 = text2 + "\"userName\": \"" + userData.Detail.UserName + "\",";
		text2 += $"\"rating\": {userData.Detail.Rating}";
		text2 = text2 + ",\"userFavoriteList\": " + GetFavoriteList(userData);
		text2 = text2 + ",\"userCharacterList\": " + GetCharacterList(userData);
		text2 = text2 + ",\"userMusicDetailList\": " + GetMusicDetailList(userData);
		text2 += "}";
		string arg = DateTime.Now.ToString("yyMMddHHmmss");
		if (!Directory.Exists(Path.Combine(text)))
		{
			Directory.CreateDirectory(text);
		}
		if (string.IsNullOrEmpty(fileName))
		{
			fileName = $"User{userData.Detail.UserID}-{arg}.json";
		}
		File.WriteAllText(Path.Combine(text, fileName), text2);
		if (sendMessage)
		{
			GameMessageManager.SendMessage((int)index, "Export Backup Data:\n" + fileName);
		}
		return fileName;
	}

	public static string GetCharacterList(UserData userData)
	{
		return JSON.Dump(userData.CharaList.Export(), EncodeOptions.NoTypeHints);
	}

	public static string GetChargeList(UserData userData)
	{
		return JSON.Dump(userData.ChargeList, EncodeOptions.NoTypeHints);
	}

	public static string GetCourseList(UserData userData)
	{
		return JSON.Dump(userData.CourseList.Export(), EncodeOptions.NoTypeHints);
	}

	public static string GetMusicDetailList(UserData userData)
	{
		UserMusicDetail[] source = userData.ExportScoreDetailList();
		return string.Concat("[" + string.Join(", ", source.Select((UserMusicDetail detail) => $"{{\"musicId\": {detail.musicId}, " + $"\"level\": {(int)detail.level}, " + $"\"playCount\": {detail.playCount}, " + $"\"achievement\": {detail.achievement}, " + $"\"comboStatus\": {(int)detail.comboStatus}, " + $"\"syncStatus\": {(int)detail.syncStatus}, " + $"\"deluxscoreMax\": {detail.deluxscoreMax}, " + $"\"scoreRank\": {(int)detail.scoreRank}, " + $"\"extNum1\": {detail.extNum1}}}")), "]");
	}

	public static string GetFavoriteList(UserData userData)
	{
		return string.Concat(string.Concat(string.Concat(string.Concat("[{" + "\"iconList\": [" + string.Join(", ", userData.IconList.Select((Manager.UserDatas.UserItem item) => item.itemId)) + "]", ",\"plateList\": [", string.Join(", ", userData.PlateList.Select((Manager.UserDatas.UserItem item) => item.itemId)), "]"), ",\"titleList\": [", string.Join(", ", userData.TitleList.Select((Manager.UserDatas.UserItem item) => item.itemId)), "]"), ",\"frameList\": [", string.Join(", ", userData.FrameList.Select((Manager.UserDatas.UserItem item) => item.itemId)), "]"), "}]");
	}

	public static UserData GetUserData(long index)
	{
		return Singleton<UserDataManager>.Instance.GetUserData(index);
	}

	public static string GetUserIdString(long index)
	{
		ulong guestUserIDBase = GetGuestUserIDBase();
		ulong userID = Singleton<UserDataManager>.Instance.GetUserData(index).Detail.UserID;
		if (userID != guestUserIDBase)
		{
			return userID.ToString();
		}
		return "Guest";
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private static ulong GetGuestUserIDBase()
	{
		return 281474976710657uL;
	}
}
