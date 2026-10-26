using System;
using System.Collections.Generic;
using MAI2.Util;
using Manager;

namespace SinmaiAssist.Utils;

public class Server
{
	private static readonly Dictionary<string, string> TitleServerList = new Dictionary<string, string>
	{
		{ "maimai-gm.wahlap.com", "CN Wahlap Official" },
		{ "aquadx.hydev.org", "AquaDX Network" },
		{ "bluedeer233.com", "Bluedeer Network" },
		{ "naominet.live", "RinNET" },
		{ "msm.moe", "Msm Aqua" }
	};

	public static string GetTitleServerUri()
	{
		try
		{
			string baseUri = Singleton<OperationManager>.Instance.GetBaseUri();
			if (string.IsNullOrEmpty(baseUri))
			{
				return "Unknown";
			}
			string host = new Uri(baseUri).Host;
			foreach (KeyValuePair<string, string> titleServer in TitleServerList)
			{
				if (host.Contains(titleServer.Key))
				{
					return titleServer.Value;
				}
			}
			return host;
		}
		catch (Exception ex) when (ex is NullReferenceException || ex is InvalidOperationException)
		{
			return "Unknown";
		}
	}
}
