using System;
using System.IO;
using System.Reflection;
using HarmonyLib;
using MAI2System;
using MelonLoader;
using SinmaiAssist.Attributes;
using SinmaiAssist.Cheat;
using SinmaiAssist.Common;
using SinmaiAssist.Config;
using SinmaiAssist.Fix;
using SinmaiAssist.GUI;
using SinmaiAssist.Utils;
using UnityEngine;

namespace SinmaiAssist;

public class SinmaiAssist : MelonMod
{
	private MainGUI _mainGUI;

	private static bool _isPatchFailed = false;

	private static ConfigManager<MainConfig> _mainConfigManager;

	private static ConfigManager<KeyBindConfig> _keyBindConfigManager;

	public static MainConfig MainConfig;

	public static KeyBindConfig KeyBindConfig;

	public static string GameID = "Unknown";

	public static uint GameVersion = 0u;

	public override void OnInitializeMelon()
	{
		if (!Directory.Exists("./Sinmai-Assist"))
		{
			Directory.CreateDirectory("./Sinmai-Assist");
		}
		if (File.Exists("./Sinmai-Assist/Unity.log"))
		{
			File.Delete("Sinmai-Assist/Unity.log");
		}
		File.WriteAllText("./Sinmai-Assist/Unity.log", "");
		Application.logMessageReceived += OnLogMessageReceived;
		PrintLogo();
		_mainGUI = new MainGUI();
		try
		{
			KeyBindConfig.Converter customConverter = new KeyBindConfig.Converter();
			_mainConfigManager = new ConfigManager<MainConfig>("./Sinmai-Assist/Config.yml");
			_keyBindConfigManager = new ConfigManager<KeyBindConfig>("./Sinmai-Assist/KeyBindConfig.yml", customConverter);
			MainConfig = _mainConfigManager.GetConfig();
			KeyBindConfig = _keyBindConfigManager.GetConfig();
			I18n.EnglishMode = MainConfig.ModSetting.English;
			AutoPlay.ClassicMode = MainConfig.Cheat.ClassicAutoPlay;
			DummyLoginPanel.DummyUserId = MainConfig.Common.DummyLogin.DefaultUserId.ToString();
			DebugPanel.UnityLogger = MainConfig.Common.UnityLogger.Enable;
			MelonLogger.Msg("Config Load Complete.");
		}
		catch (Exception arg)
		{
			MelonLogger.Error($"Error initializing mod config: \n{arg}");
			return;
		}
		File.Delete("Sinmai-Assist/WebCameraList.txt");
		WebCamDevice[] devices = WebCamTexture.devices;
		string text = "\nConnected Web Cameras:\n";
		for (int i = 0; i < devices.Length; i++)
		{
			WebCamDevice webCamDevice = devices[i];
			text = text + "Name: " + webCamDevice.name + "\n";
			text += $"ID: {i}\n";
			WebCamTexture webCamTexture = new WebCamTexture(webCamDevice.name);
			webCamTexture.Play();
			text += $"Resolution: {webCamTexture.width}x{webCamTexture.height}\n";
			text += $"FPS: {webCamTexture.requestedFPS}\n";
			webCamTexture.Stop();
			text += "\n";
		}
		File.AppendAllText("Sinmai-Assist/WebCameraList.txt", text);
		try
		{
			GameID = (string)typeof(ConstParameter).GetField("GameIDStr", BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy).GetValue(null);
			GameVersion = (uint)typeof(ConstParameter).GetField("NowGameVersion", BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy).GetValue(null);
			if (GameVersion < 24000)
			{
				MelonLogger.Warning($"Using untested version ({GameVersion}) maybe case some unexcepted problems!");
			}
		}
		catch (Exception obj)
		{
			MelonLogger.Error("Failed to get GameIDStr and GameVersion");
			MelonLogger.Error(obj);
		}
		MelonLogger.Msg($"GameInfo: {GameID} {GameVersion} ");
		if (MainConfig.ModSetting.SafeMode)
		{
			MelonLogger.Warning("Safe mode is enabled, Disable all patch");
			return;
		}
		Patch(typeof(HarmonyLibPatch), noLoggerPrint: true);
		if (MainConfig.Common.DummyLogin.Enable)
		{
			if (GameID.Equals("SDGB"))
			{
				Patch(typeof(DummyChimeLogin));
			}
			else
			{
				if (File.Exists("DEVICE/aime.txt"))
				{
					DummyLoginPanel.DummyLoginCode = File.ReadAllText("DEVICE/aime.txt").Trim();
				}
				Patch(typeof(DummyAimeLogin));
			}
		}
		if (MainConfig.Common.CustomCameraId.Enable)
		{
			if (MainConfig.Common.DummyLogin.Enable)
			{
				MelonLogger.Warning("DummyLogin enabled, CustomCameraId has been automatically disabled.");
			}
			else
			{
				Patch(typeof(CustomCameraId));
			}
		}
		if (MainConfig.Common.AutoBackupData)
		{
			Patch(typeof(AutoBackupData));
		}
		if (MainConfig.Common.InfinityTimer)
		{
			Patch(typeof(InfinityTimer));
		}
		if (MainConfig.Common.InfinityTimerLegacy)
		{
			Patch(typeof(InfinityTimerLegacy));
		}
		if (MainConfig.Common.DisableBackground)
		{
			Patch(typeof(DisableBackground));
		}
		if (MainConfig.Common.DisableMask)
		{
			Patch(typeof(DisableMask));
		}
		if (MainConfig.Common.SinglePlayer.Enable)
		{
			Patch(typeof(SinglePlayer));
		}
		if (MainConfig.Common.ForceQuickRetry)
		{
			Patch(typeof(ForceQuickRetry));
		}
		if (MainConfig.Common.ForwardATouchRegionToButton)
		{
			Patch(typeof(ForwardATouchRegionToButton));
		}
		if (MainConfig.Common.BlockCoin)
		{
			Patch(typeof(BlockCoin));
		}
		if (MainConfig.Common.SkipWarningScreen)
		{
			Patch(typeof(SkipWarningScreen));
		}
		if (MainConfig.Common.SkipFade)
		{
			Patch(typeof(SkipFade));
		}
		if (MainConfig.Common.NetworkLogger.Enable)
		{
			Patch(typeof(NetworkLogger));
		}
		if (MainConfig.Common.CustomVersionText.Enable)
		{
			Patch(typeof(CustomVersionText));
		}
		if (MainConfig.Common.IgnoreAnyGameInformation)
		{
			Patch(typeof(IgnoreAnyGameInformation));
		}
		if (MainConfig.Common.ChangeDefaultOption)
		{
			Patch(typeof(ChangeDefaultOption));
		}
		if (MainConfig.Common.ChangeFadeStyle)
		{
			Patch(typeof(ChangeFadeStyle));
		}
		if (MainConfig.Common.ChangeGameSettings.Enable)
		{
			Patch(typeof(ChangeGameSettings));
		}
		if (MainConfig.Fix.DisableEnvironmentCheck)
		{
			Patch(typeof(DisableEnvironmentCheck));
		}
		if (MainConfig.Fix.DisableEncryption)
		{
			Patch(typeof(DisableEncryption));
		}
		if (MainConfig.Fix.DisableReboot)
		{
			Patch(typeof(DisableReboot));
		}
		if (MainConfig.Fix.DisableIniClear)
		{
			Patch(typeof(DisableIniClear));
		}
		if (MainConfig.Fix.FixDebugInput)
		{
			Patch(typeof(FixDebugInput));
		}
		if (MainConfig.Fix.FixCheckAuth)
		{
			Patch(typeof(FixCheckAuth));
		}
		if (MainConfig.Fix.ForceAsServer)
		{
			Patch(typeof(ForceAsServer));
		}
		if (MainConfig.Fix.SkipCakeHashCheck)
		{
			Patch(typeof(SkipCakeHashCheck));
		}
		if (MainConfig.Fix.SkipSpecialNumCheck)
		{
			Patch(typeof(SkipSpecialNumCheck));
		}
		if (MainConfig.Fix.SkipVersionCheck)
		{
			Patch(typeof(SkipVersionCheck));
		}
		if (MainConfig.Fix.RestoreCertificateValidation)
		{
			Patch(typeof(RestoreCertificateValidation));
		}
		if (MainConfig.Fix.RewriteNoteJudgeTiming.Enable)
		{
			Patch(typeof(RewriteNoteJudgeTiming));
		}
		if (MainConfig.Cheat.AutoPlay)
		{
			Patch(typeof(AutoPlay));
		}
		if (MainConfig.Cheat.FastSkip)
		{
			Patch(typeof(FastSkip));
		}
		if (MainConfig.Cheat.ChartController)
		{
			Patch(typeof(ChartController));
		}
		if (MainConfig.Cheat.AllCollection)
		{
			Patch(typeof(AllCollection));
		}
		if (MainConfig.Cheat.UnlockMusic.Enable)
		{
			Patch(typeof(UnlockMusic));
		}
		if (MainConfig.Cheat.UnlockMaster.Enable)
		{
			Patch(typeof(UnlockMaster));
		}
		if (MainConfig.Cheat.UnlockUtage.Enable)
		{
			Patch(typeof(UnlockUtage));
		}
		if (MainConfig.Cheat.UnlockEvent)
		{
			Patch(typeof(UnlockEvent));
		}
		if (MainConfig.Cheat.ResetLoginBonusRecord)
		{
			Patch(typeof(ResetLoginBonusRecord));
		}
		if (MainConfig.Cheat.ForceCurrentIsBest)
		{
			Patch(typeof(ForceCurrentIsBest));
		}
		if (MainConfig.Cheat.SetAllCharacterAsSameAndLock)
		{
			Patch(typeof(SetAllCharacterAsSameAndLock));
		}
		if (MainConfig.Cheat.RewriteLoginBonusStamp.Enable)
		{
			Patch(typeof(RewriteLoginBonusStamp));
		}
		Patch(typeof(PrintUserData));
		Patch(typeof(InputManager));
		Patch(typeof(GameMessageManager));
		if (_isPatchFailed)
		{
			PatchFailedWarn();
		}
		MelonLogger.Msg("Loading completed");
	}

	public static void SaveMainConfig()
	{
		_mainConfigManager?.Save();
	}

	public override void OnApplicationQuit()
	{
	}

	public override void OnGUI()
	{
		_mainGUI.OnGUI();
		if (MainConfig.Common.ShowFPS)
		{
			ShowFPS.OnGUI();
		}
		if (MainConfig.ModSetting.ShowInfo)
		{
			ShowVersionInfo.OnGUI();
		}
	}

	private void OnLogMessageReceived(string condition, string stackTrace, LogType type)
	{
		string text = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [{type}] {condition}\n{stackTrace}";
		if (MainConfig.Common.UnityLogger.Enable)
		{
			File.AppendAllText(System.IO.Path.Combine("Sinmai-Assist/Unity.log"), text + "\n");
		}
		if (MainConfig.Common.UnityLogger.Enable && MainConfig.Common.UnityLogger.PrintToConsole)
		{
			switch (type)
			{
			case LogType.Error:
			case LogType.Exception:
				MelonLogger.Error($"[UnityLogger] [{type}]: {condition}\n{stackTrace}");
				break;
			case LogType.Warning:
				MelonLogger.Warning($"[UnityLogger] [{type}]: {condition}\n{stackTrace}");
				break;
			default:
				MelonLogger.Msg($"[UnityLogger] [{type}]: {condition}\n{stackTrace}");
				break;
			}
		}
	}

	private static bool Patch(Type type, bool noLoggerPrint = false)
	{
		try
		{
			EnableGameVersionAttribute customAttribute = type.GetCustomAttribute<EnableGameVersionAttribute>();
			if (customAttribute != null && !customAttribute.ShouldEnable())
			{
				MelonLogger.Warning($"Patch: {type} skipped ,Game version need Min {customAttribute.MinGameVersion} Max {customAttribute.MaxGameVersion}");
				return false;
			}
			if (!noLoggerPrint)
			{
				MelonLogger.Msg($"> Patch: {type}");
			}
			HarmonyLib.Harmony.CreateAndPatchAll(type);
			return true;
		}
		catch (Exception ex)
		{
			MelonLogger.Error($"Patch: {type} failed.");
			MelonLogger.Error(ex.Message);
			MelonLogger.Error(ex.Source);
			MelonLogger.Error(ex.TargetSite);
			MelonLogger.Error(ex.InnerException);
			MelonLogger.Error(ex.StackTrace);
			_isPatchFailed = true;
			return false;
		}
	}

	private static void PrintLogo()
	{
		MelonLogger.Msg("\n\r\n   _____ _       __  ___      _       ___              _      __ \r\n  / ___/(_)___  /  |/  /___ _(_)     /   |  __________(_)____/ /_\r\n  \\__ \\/ / __ \\/ /|_/ / __ `/ /_____/ /| | / ___/ ___/ / ___/ __/\r\n ___/ / / / / / /  / / /_/ / /_____/ ___ |(__  |__  ) (__  ) /_  \r\n/____/_/_/ /_/_/  /_/\\__,_/_/     /_/  |_/____/____/_/____/\\__/  \r\n                                                                 \r\n=================================================================\r\n Version: 1.0.0 (787a437) Build Date: 2025-11-29T12:08:47.5657070+08:00\r\n Author: WYH2004 (Slim) & Error063");
		MelonLogger.Warning("\n\r\n=================================================================\r\nThis is a cheat mod. Use at your own risk!\r\nThis is a free and open-source mod. Resale is strictly prohibited.\r\nIf you paid for this mod, you are stupid.\r\n=================================================================");
	}

	private static void PatchFailedWarn()
	{
		MelonLogger.Warning("\r\n=================================================================\r\nFailed to patch some methods.\r\nPlease ensure that you are using an unmodified version of Assembly-CSharp.dll with a version greater than 1.40.0,\r\nas modifications or lower versions can cause function mismatches, preventing the required functions from being found.\r\nCheck for conflicting mods, or enabled incompatible options.\r\nIf you believe this is an error, please report the issue to the mod author.\r\n=================================================================");
	}
}
