using System;
using System.Reflection;
using HarmonyLib;

namespace SinmaiAssist.Utils;

internal class CipherAES
{
	private static Type cipherAESType = AccessTools.TypeByName("CipherAES");

	public static byte[] Encrypt(byte[] data)
	{
		return (byte[])cipherAESType.GetMethod("Encrypt", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic).Invoke(null, new object[1] { data });
	}

	public static byte[] Decrypt(byte[] encryptData)
	{
		return (byte[])cipherAESType.GetMethod("Decrypt", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic).Invoke(null, new object[1] { encryptData });
	}
}
