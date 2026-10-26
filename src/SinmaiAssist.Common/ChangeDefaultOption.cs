using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using DB;
using HarmonyLib;
using MAI2.Util;
using Manager;
using Manager.UserDatas;
using MelonLoader;
using SinmaiAssist.Utils;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace SinmaiAssist.Common;

public class ChangeDefaultOption
{
	private static string OptionFilePath = "Sinmai-Assist/DefaultOption.yml";

	[HarmonyPostfix]
	[HarmonyPatch(typeof(UserDataManager), "SetDefault")]
	public static void DefaultOption(UserDataManager __instance, ref long index)
	{
		if (!__instance.GetUserData(index).IsGuest())
		{
			return;
		}
		try
		{
			UserOption option = Singleton<UserDataManager>.Instance.GetUserData(index).Option;
			option.OptionKind = OptionKindID.Custom;
			if (!File.Exists(OptionFilePath))
			{
				MelonLogger.Warning("Path: \"" + OptionFilePath + "\" Not Found, Try to create it.");
				SaveOptionFile(index);
				return;
			}
			IDeserializer deserializer = new DeserializerBuilder().WithNamingConvention(CamelCaseNamingConvention.Instance).Build();
			using StreamReader input = new StreamReader(OptionFilePath);
			UserOption newOption = deserializer.Deserialize<UserOption>(input);
			UpdateUserOption(option, newOption);
		}
		catch (Exception obj)
		{
			MelonLogger.Error(obj);
		}
	}

	private static void UpdateUserOption(UserOption originalOption, UserOption newOption)
	{
		if (newOption == null)
		{
			return;
		}
		PropertyInfo[] properties = typeof(UserOption).GetProperties();
		foreach (PropertyInfo propertyInfo in properties)
		{
			if (propertyInfo.CanWrite)
			{
				object value = propertyInfo.GetValue(newOption);
				propertyInfo.SetValue(originalOption, value);
			}
		}
	}

	private static string RemoveUnwantedOption(string yaml)
	{
		string[] propertiesToRemove = new string[3] { "getNoteSpeed", "getSlideSpeed", "getTouchSpeed" };
		IEnumerable<string> values = from line in yaml.Split('\n')
			where !propertiesToRemove.Any((string prop) => line.TrimStart().StartsWith(prop))
			select line;
		return string.Join("\n", values);
	}

	public static void SaveOptionFile(long index)
	{
		UserOption option = Singleton<UserDataManager>.Instance.GetUserData(index).Option;
		option.OptionKind = OptionKindID.Custom;
		string yaml = new SerializerBuilder().WithNamingConvention(CamelCaseNamingConvention.Instance).Build().Serialize(option);
		yaml = RemoveUnwantedOption(yaml);
		if (File.Exists(OptionFilePath))
		{
			File.Delete(OptionFilePath);
		}
		File.WriteAllText(OptionFilePath, yaml);
		GameMessageManager.SendMessage((int)index, "User Option File Saved");
	}
}
