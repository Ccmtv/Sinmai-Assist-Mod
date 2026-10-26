using System;
using System.Collections;
using HarmonyLib;
using Manager;
using UnityEngine;

namespace SinmaiAssist.Common;

public class CustomCameraId
{
	private static CameraParameter _gameCameraParam;

	private static CameraParameter _qrCameraParam;

	[HarmonyPrefix]
	[HarmonyPatch(typeof(CameraManager), "CameraInitialize")]
	public static bool CameraInitialize(CameraManager __instance, ref IEnumerator __result)
	{
		__result = CameraInitialize(__instance);
		return false;
	}

	[HarmonyPostfix]
	[HarmonyPatch(typeof(CameraManager), "Initialize")]
	public static void SetCameraResolution(CameraManager __instance)
	{
		if (SinmaiAssist.GameID != "SDEZ")
		{
			WebCamDevice webCamDevice = WebCamTexture.devices[(SinmaiAssist.GameID == "SDGB") ? SinmaiAssist.MainConfig.Common.CustomCameraId.ChimeCameraId : SinmaiAssist.MainConfig.Common.CustomCameraId.LeftQrCameraId];
			WebCamTexture webCamTexture = new WebCamTexture(webCamDevice.name);
			webCamTexture.Play();
			_qrCameraParam = new CameraParameter(webCamTexture.width, webCamTexture.height, (int)webCamTexture.requestedFPS);
			AccessTools.Field(typeof(CameraManager), "QrCameraParam").SetValue(__instance, _qrCameraParam);
			webCamTexture.Stop();
		}
		WebCamDevice webCamDevice2 = WebCamTexture.devices[SinmaiAssist.MainConfig.Common.CustomCameraId.PhotoCameraId];
		WebCamTexture webCamTexture2 = new WebCamTexture(webCamDevice2.name);
		webCamTexture2.Play();
		_gameCameraParam = new CameraParameter(webCamTexture2.width, webCamTexture2.height, (int)webCamTexture2.requestedFPS);
		AccessTools.Field(typeof(CameraManager), "GameCameraParam").SetValue(__instance, _gameCameraParam);
		webCamTexture2.Stop();
	}

	private static IEnumerator CameraInitialize(CameraManager __instance)
	{
		WebCamTexture[] array = new WebCamTexture[Math.Max(2, WebCamTexture.devices.Length)];
		int num = ((SinmaiAssist.MainConfig.Common.CustomCameraId.LeftQrCameraId < WebCamTexture.devices.Length) ? SinmaiAssist.MainConfig.Common.CustomCameraId.LeftQrCameraId : 0);
		int num2 = ((SinmaiAssist.MainConfig.Common.CustomCameraId.RightQrCameraId < WebCamTexture.devices.Length) ? SinmaiAssist.MainConfig.Common.CustomCameraId.RightQrCameraId : 0);
		int num3 = ((SinmaiAssist.MainConfig.Common.CustomCameraId.PhotoCameraId < WebCamTexture.devices.Length) ? SinmaiAssist.MainConfig.Common.CustomCameraId.PhotoCameraId : 0);
		int num4 = ((SinmaiAssist.MainConfig.Common.CustomCameraId.ChimeCameraId < WebCamTexture.devices.Length) ? SinmaiAssist.MainConfig.Common.CustomCameraId.ChimeCameraId : 0);
		string gameID = SinmaiAssist.GameID;
		if (!(gameID == "SDGB"))
		{
			if (gameID == "SDEZ")
			{
				array[num] = new WebCamTexture(WebCamTexture.devices[num].name, _qrCameraParam.Width, _qrCameraParam.Height, _qrCameraParam.Fps);
				array[num2] = new WebCamTexture(WebCamTexture.devices[num2].name, _qrCameraParam.Width, _qrCameraParam.Height, _qrCameraParam.Fps);
				CameraManager.DeviceId[0] = num;
				CameraManager.DeviceId[1] = num2;
				CameraManager.DeviceId[2] = num3;
			}
			else
			{
				CameraManager.DeviceId[0] = num3;
			}
		}
		else
		{
			array[num4] = new WebCamTexture(WebCamTexture.devices[num4].name, _qrCameraParam.Width, _qrCameraParam.Height, _qrCameraParam.Fps);
			CameraManager.DeviceId[0] = num4;
			CameraManager.DeviceId[1] = num3;
		}
		array[num3] = new WebCamTexture(WebCamTexture.devices[num3].name, _gameCameraParam.Width, _gameCameraParam.Height, _gameCameraParam.Fps);
		if (WebCamTexture.devices.Length == 1)
		{
			array[1] = array[0];
		}
		AccessTools.Field(typeof(CameraManager), "_webcamtex").SetValue(__instance, array);
		__instance.isAvailableCamera = new bool[array.Length];
		__instance.cameraProcMode = new CameraManager.CameraProcEnum[array.Length];
		for (int i = 0; i < array.Length; i++)
		{
			__instance.isAvailableCamera[i] = true;
			__instance.cameraProcMode[i] = CameraManager.CameraProcEnum.Good;
		}
		CameraManager.IsReady = true;
		yield break;
	}
}
