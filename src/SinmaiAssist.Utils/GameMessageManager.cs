using System;
using DB;
using HarmonyLib;
using Manager;
using MelonLoader;
using Process;

namespace SinmaiAssist.Utils;

public class GameMessageManager
{
	private static IGenericManager _manager;

	[HarmonyPostfix]
	[HarmonyPatch(typeof(ProcessManager), "SetMessageManager")]
	public static void SetMessageManager(IGenericManager genericManager)
	{
		_manager = genericManager;
	}

	public static void SendMessage(int monitorId, string message, string title = null, WindowPositionID positionID = WindowPositionID.Middle, WindowMessageID messageID = WindowMessageID.CollectionAttentionEmptyFavorite)
	{
		try
		{
			WindowParam windowParam = new WindowParam
			{
				hideTitle = true,
				text = message,
				replaceText = true,
				changeSize = true,
				sizeID = WindowSizeID.Middle,
				positionID = positionID
			};
			if (title != null)
			{
				if (messageID == WindowMessageID.CollectionAttentionEmptyFavorite)
				{
					messageID = WindowMessageID.AimeUseNotice;
				}
				windowParam.replaceTitle = true;
				windowParam.hideTitle = false;
				windowParam.title = title;
			}
			_manager.Enqueue(monitorId, messageID, positionID, windowParam);
		}
		catch (Exception obj)
		{
			MelonLogger.Error(obj);
		}
	}

	public static void SendWarning(int monitorId, string message, string title, float lifeTime = 3000f)
	{
		try
		{
			WarningWindowInfo info = new WarningWindowInfo(title, message, lifeTime, monitorId);
			_manager.EnqueueWarning(info);
		}
		catch (Exception obj)
		{
			MelonLogger.Error(obj);
		}
	}

	public static IGenericManager GetMessageManager()
	{
		return _manager;
	}
}
