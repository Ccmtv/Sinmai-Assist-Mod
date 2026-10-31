using System.Text;
using AMDaemon;
using MAI2.Util;
using MAI2System;
using SinmaiAssist.Utils;
using UnityEngine;

namespace SinmaiAssist.GUI;

public class ShowVersionInfo
{
	private static readonly StringBuilder VersionText = new StringBuilder();

	private static readonly GUIStyle TextShadow = new GUIStyle
	{
		fontSize = 24,
		alignment = TextAnchor.UpperLeft,
		normal = new GUIStyleState
		{
			textColor = Color.black
		}
	};

	private static readonly GUIStyle TextStyle = new GUIStyle
	{
		fontSize = 24,
		alignment = TextAnchor.UpperLeft,
		normal = new GUIStyleState
		{
			textColor = Color.white
		}
	};

	public static void OnGUI()
	{
		VersionText.Clear();
		VersionText.AppendLine("Sinmai-Assist 1.1.1 (787a437)");
		VersionText.AppendLine("Powered by MelonLoader");
		VersionText.AppendLine($"客户端版本：{SinmaiAssist.GameID} {SinmaiAssist.GameVersion}");
		VersionText.AppendLine("数据版本：" + Singleton<SystemConfig>.Instance.config.dataVersionInfo.versionNo.versionString + " " + Singleton<SystemConfig>.Instance.config.dataVersionInfo.versionNo.releaseNoAlphabet);
		VersionText.AppendLine("当前服务器：" + Server.GetTitleServerUri());
		VersionText.AppendLine($"Keychip：{AMDaemon.System.KeychipId}");
		VersionText.AppendLine("用户 ID：" + User.GetUserIdString(0L) + " | " + User.GetUserIdString(1L));
		if (SinmaiAssist.MainConfig.ModSetting.SafeMode)
		{
			VersionText.AppendLine("安全模式");
		}
		string text = I18n.T(VersionText.ToString());
		UnityEngine.GUI.Label(new Rect(12f, 42f, 500f, 30f), text, TextShadow);
		UnityEngine.GUI.Label(new Rect(10f, 40f, 500f, 30f), text, TextStyle);
	}
}
