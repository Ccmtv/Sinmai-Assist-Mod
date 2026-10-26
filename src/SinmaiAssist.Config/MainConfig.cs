namespace SinmaiAssist.Config;

public class MainConfig
{
	public class CommonConfig
	{
		public UnityLoggerConfig UnityLogger { get; set; } = new UnityLoggerConfig();

		public bool AutoBackupData { get; set; }

		public bool InfinityTimer { get; set; }

		public bool InfinityTimerLegacy { get; set; }

		public bool DisableMask { get; set; }

		public bool DisableBackground { get; set; }

		public bool ShowFPS { get; set; } = true;

		public bool ForceQuickRetry { get; set; }

		public bool ForwardATouchRegionToButton { get; set; }

		public bool SkipFade { get; set; }

		public bool SkipWarningScreen { get; set; }

		public bool QuickBoot { get; set; }

		public bool BlockCoin { get; set; }

		public bool IgnoreAnyGameInformation { get; set; }

		public bool ChangeDefaultOption { get; set; }

		public bool ChangeFadeStyle { get; set; }

		public SinglePlayerConfig SinglePlayer { get; set; } = new SinglePlayerConfig();

		public NetworkLoggerConfig NetworkLogger { get; set; } = new NetworkLoggerConfig();

		public CustomVersionTextConfig CustomVersionText { get; set; } = new CustomVersionTextConfig();

		public DummyLoginConfig DummyLogin { get; set; } = new DummyLoginConfig();

		public CustomCameraIdConfig CustomCameraId { get; set; } = new CustomCameraIdConfig();

		public ChangeGameSettingsConfig ChangeGameSettings { get; set; } = new ChangeGameSettingsConfig();
	}

	public class CheatConfig
	{
		public bool AutoPlay { get; set; }

		public bool ClassicAutoPlay { get; set; }

		public bool FastSkip { get; set; }

		public bool ChartController { get; set; }

		public bool AllCollection { get; set; }

		public bool UnlockEvent { get; set; }

		public UnlockMusicConfig UnlockMusic { get; set; } = new UnlockMusicConfig();

		public UnlockMasterConfig UnlockMaster { get; set; } = new UnlockMasterConfig();

		public UnlockUtageConfig UnlockUtage { get; set; } = new UnlockUtageConfig();

		public bool ResetLoginBonusRecord { get; set; }

		public bool ForceCurrentIsBest { get; set; }

		public bool SetAllCharacterAsSameAndLock { get; set; }

		public RewriteLoginBonusStampConfig RewriteLoginBonusStamp { get; set; } = new RewriteLoginBonusStampConfig();
	}

	public class FixConfig
	{
		public bool DisableEnvironmentCheck { get; set; } = true;

		public bool DisableEncryption { get; set; }

		public bool DisableReboot { get; set; } = true;

		public bool DisableIniClear { get; set; } = true;

		public bool FixDebugInput { get; set; } = true;

		public bool FixCheckAuth { get; set; }

		public bool ForceAsServer { get; set; }

		public bool SkipCakeHashCheck { get; set; }

		public bool SkipSpecialNumCheck { get; set; } = true;

		public bool SkipVersionCheck { get; set; }

		public bool RestoreCertificateValidation { get; set; }

		public RewriteNoteJudgeTimingConfig RewriteNoteJudgeTiming { get; set; } = new RewriteNoteJudgeTimingConfig();
	}

	public class ModSettingConfig
	{
		public bool SafeMode { get; set; }

		public bool ShowInfo { get; set; } = true;

		public bool ShowPanel { get; set; } = true;
	}

	public class ChangeGameSettingsConfig
	{
		public bool Enable { get; set; }

		public bool CodeRead { get; set; }

		public bool IconPhoto { get; set; }

		public bool UploadPhoto { get; set; }

		public bool CharaSelect { get; set; }
	}

	public class SinglePlayerConfig
	{
		public bool Enable { get; set; }

		public bool HideSubMonitor { get; set; }
	}

	public class NetworkLoggerConfig
	{
		public bool Enable { get; set; } = true;

		public bool PrintToConsole { get; set; }
	}

	public class CustomVersionTextConfig
	{
		public bool Enable { get; set; }

		public string VersionText { get; set; } = "Sinmai-Assist";
	}

	public class UnlockMusicConfig
	{
		public bool Enable { get; set; }

		public bool SaveToUserData { get; set; }
	}

	public class UnlockMasterConfig
	{
		public bool Enable { get; set; }

		public bool SaveToUserData { get; set; }
	}

	public class UnlockUtageConfig
	{
		public bool Enable { get; set; }

		public bool UnlockDoublePlayerMusic { get; set; }
	}

	public class RewriteNoteJudgeTimingConfig
	{
		public bool Enable { get; set; }

		public float AdjustTiming { get; set; }

		public float JudgeTiming { get; set; }
	}

	public class DummyLoginConfig
	{
		public bool Enable { get; set; }

		public int DefaultUserId { get; set; }
	}

	public class CustomCameraIdConfig
	{
		public bool Enable { get; set; }

		public int ChimeCameraId { get; set; }

		public int LeftQrCameraId { get; set; }

		public int RightQrCameraId { get; set; }

		public int PhotoCameraId { get; set; }
	}

	public class RewriteLoginBonusStampConfig
	{
		public bool Enable { get; set; }

		public uint Point { get; set; }
	}

	public class UnityLoggerConfig
	{
		public bool Enable { get; set; } = true;

		public bool PrintToConsole { get; set; } = true;
	}

	public CommonConfig Common { get; set; } = new CommonConfig();

	public CheatConfig Cheat { get; set; } = new CheatConfig();

	public FixConfig Fix { get; set; } = new FixConfig();

	public ModSettingConfig ModSetting { get; set; } = new ModSettingConfig();
}
