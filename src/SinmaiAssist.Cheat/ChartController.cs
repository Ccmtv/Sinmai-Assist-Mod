using System;
using System.Reflection;
using DB;
using HarmonyLib;
using MAI2.Util;
using Manager;
using MelonLoader;
using Monitor;
using Process;

namespace SinmaiAssist.Cheat;

public class ChartController
{
	private enum GameSequence
	{
		Init,
		Sync,
		Start,
		StartWait,
		Play,
		PlayEnd,
		Result,
		ResultEnd,
		FinalWait,
		Release
	}

	public enum Button
	{
		None,
		Pause,
		Reset,
		TimeSkipAdd,
		TimeSkipAdd2,
		TimeSkipAdd3,
		TimeSkipSub,
		TimeSkipSub2,
		TimeSkipSub3,
		Set,
		Back
	}

	public static Button ButtonStatus;

	public static bool IsPlaying;

	public static double Timer;

	public static int RecordTime;

	private static MovieController _gameMovie;

	private static NotesManager _notesManager;

	private static GameProcess _gameProcess;

	private static GameMonitor[] _monitors;

	[HarmonyPostfix]
	[HarmonyPatch(typeof(GameProcess), "OnUpdate")]
	private static void OnUpdate(GameProcess __instance)
	{
		try
		{
			_gameProcess = __instance;
			GameSequence num = (GameSequence)typeof(GameProcess).GetField("_sequence", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(__instance);
			_monitors = (GameMonitor[])typeof(GameProcess).GetField("_monitors", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(_gameProcess);
			_gameMovie = (MovieController)typeof(GameProcess).GetField("_gameMovie", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(__instance);
			_notesManager = new NotesManager();
			if (!_notesManager.IsPlaying())
			{
				IsPlaying = true;
				Timer = 0.0;
				RecordTime = 0;
			}
			if (num == GameSequence.Play)
			{
				bool partyPlay = (bool)typeof(GameProcess).GetMethod("IsPartyPlay", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(__instance, null);
				if (IsPlaying)
				{
					Timer += GameManager.GetGameMSecAddD();
				}
				if (ButtonStatus == Button.Pause || DebugInput.GetKeyDown(SinmaiAssist.KeyBindConfig.ChartController.Pause.KeyCode))
				{
					if (IsPlaying)
					{
						SoundManager.PauseMusic(pause: true);
						GameMoviePause(pause: true);
						NotesManager.Pause(isPause: true);
					}
					else
					{
						SoundManager.PauseMusic(pause: false);
						GameMoviePause(pause: false);
						NotesManager.Pause(isPause: false);
					}
					IsPlaying = !IsPlaying;
				}
				else if (DebugInput.GetKey(SinmaiAssist.KeyBindConfig.ChartController.Backward.KeyCode) && !IsPlaying)
				{
					Singleton<GamePlayManager>.Instance.Initialize(partyPlay);
					TimeSkip(-16);
				}
				else if (DebugInput.GetKey(SinmaiAssist.KeyBindConfig.ChartController.Forward.KeyCode) && !IsPlaying)
				{
					Singleton<GamePlayManager>.Instance.Initialize(partyPlay);
					TimeSkip(16);
				}
				else if (DebugInput.GetKeyDown(SinmaiAssist.KeyBindConfig.ChartController.SetRecord.KeyCode) || ButtonStatus == Button.Set)
				{
					RecordTime = (int)Timer;
					MelonLogger.Msg($"Record Time: {RecordTime}");
				}
				else if (DebugInput.GetKeyDown(SinmaiAssist.KeyBindConfig.ChartController.ReturnRecord.KeyCode) || ButtonStatus == Button.Back)
				{
					int num2 = ((RecordTime == 0) ? 999999 : ((int)Timer - RecordTime));
					TimeSkip(-num2);
					TimeSkip(0);
					MelonLogger.Msg($"Time Jump: {RecordTime}({-num2})");
				}
				else if (ButtonStatus != Button.None)
				{
					Singleton<GamePlayManager>.Instance.Initialize(partyPlay);
					switch (ButtonStatus)
					{
					case Button.TimeSkipAdd:
						TimeSkip(100);
						break;
					case Button.TimeSkipAdd2:
						TimeSkip(1000);
						break;
					case Button.TimeSkipAdd3:
						TimeSkip(2500);
						break;
					case Button.TimeSkipSub:
						TimeSkip(-100);
						break;
					case Button.TimeSkipSub2:
						TimeSkip(-1000);
						break;
					case Button.TimeSkipSub3:
						TimeSkip(-2500);
						break;
					case Button.Reset:
						Singleton<GamePlayManager>.Instance.SetQuickRetryFrag(flag: true);
						break;
					}
				}
			}
			ReUpdate();
		}
		catch (Exception obj)
		{
			MelonLogger.Error(obj);
		}
	}

	private static void TimeSkip(int addMsec)
	{
		GameMoviePause(pause: true);
		NotesManager.Pause(isPause: true);
		if (addMsec >= 0)
		{
			Timer += addMsec;
		}
		else
		{
			Timer = ((Timer + (double)addMsec >= 0.0) ? (Timer + (double)addMsec) : 0.0);
		}
		GameMovieSetSeekFrame(Timer);
		SoundManager.SeekMusic((int)Timer);
		for (int i = 0; i < _monitors.Length; i++)
		{
			_monitors[i].Seek((int)Timer);
		}
		int num = 91;
		NotesManager.StartPlay((int)Timer + num);
		NotesManager.Pause(isPause: true);
		if (IsPlaying)
		{
			SoundManager.PauseMusic(pause: false);
			GameMoviePause(pause: false);
			NotesManager.Pause(isPause: false);
		}
		else
		{
			GameMoviePause(pause: true);
		}
		_gameProcess.UpdateNotes();
	}

	private static void ReUpdate()
	{
		int[] array = new int[2] { -1, -1 };
		int[] array2 = new int[2] { -1, -1 };
		ProcessDataContainer processDataContainer = (ProcessDataContainer)typeof(GameProcess).BaseType.GetField("container", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(_gameProcess);
		for (int i = 0; i < _monitors.Length; i++)
		{
			if (Singleton<GamePlayManager>.Instance.GetGameScore(i).IsTrackSkip || Singleton<GamePlayManager>.Instance.IsQuickRetry())
			{
				for (int j = 0; j < 35; j++)
				{
					InputManager.SetUsedThisFrame(i, (InputManager.TouchPanelArea)j);
				}
			}
			_monitors[i].ViewUpdate();
			if (!Singleton<UserDataManager>.Instance.GetUserData(i).IsEntry)
			{
				continue;
			}
			if (array[i] != _monitors[i].GetPushPhase())
			{
				array[i] = _monitors[i].GetPushPhase();
				switch (array[i])
				{
				case -1:
					processDataContainer.processManager.ForcedCloseWindow(i);
					break;
				case 0:
					processDataContainer.processManager.EnqueueMessage(i, WindowMessageID.TrackSkip3Second, WindowPositionID.Middle);
					break;
				case 1:
					processDataContainer.processManager.EnqueueMessage(i, WindowMessageID.TrackSkip2Second, WindowPositionID.Middle);
					break;
				case 2:
					processDataContainer.processManager.EnqueueMessage(i, WindowMessageID.TrackSkip1Second, WindowPositionID.Middle);
					break;
				case 3:
					processDataContainer.processManager.CloseWindow(i);
					break;
				}
			}
			if (array2[i] != _monitors[i].GetPushPhaseRetry())
			{
				array2[i] = _monitors[i].GetPushPhaseRetry();
				switch (array2[i])
				{
				case -1:
					processDataContainer.processManager.ForcedCloseWindow(i);
					break;
				case 0:
					processDataContainer.processManager.EnqueueMessage(i, WindowMessageID.QuickRetry2Second, WindowPositionID.Middle);
					break;
				case 1:
					processDataContainer.processManager.EnqueueMessage(i, WindowMessageID.QuickRetry1Second, WindowPositionID.Middle);
					break;
				case 2:
					processDataContainer.processManager.CloseWindow(i);
					break;
				}
			}
		}
		Singleton<GamePlayManager>.Instance.PlayLastUpdate();
	}

	private static void GameMoviePause(bool pause)
	{
		MethodInfo method = _gameMovie.GetType().GetMethod("Pause");
		if (method.GetParameters().Length == 1)
		{
			method.Invoke(_gameMovie, new object[1] { pause });
			return;
		}
		method.Invoke(_gameMovie, new object[2] { 0, pause });
		method.Invoke(_gameMovie, new object[2] { 1, pause });
	}

	private static void GameMovieSetSeekFrame(double msec)
	{
		MethodInfo method = _gameMovie.GetType().GetMethod("SetSeekFrame");
		if (method.GetParameters().Length == 1)
		{
			method.Invoke(_gameMovie, new object[1] { msec });
			return;
		}
		method.Invoke(_gameMovie, new object[2] { 0, msec });
		method.Invoke(_gameMovie, new object[2] { 1, msec });
	}
}
