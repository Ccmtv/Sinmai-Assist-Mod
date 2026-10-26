using System;
using System.Reflection;
using YamlDotNet.Serialization;

namespace YamlDotNet.Helpers;

internal static class FsharpHelper
{
	private static bool IsFsharpCore(Type t)
	{
		return t.Namespace == "Microsoft.FSharp.Core";
	}

	public static bool IsOptionType(Type t)
	{
		if (IsFsharpCore(t))
		{
			return t.Name == "FSharpOption`1";
		}
		return false;
	}

	public static Type? GetOptionUnderlyingType(Type t)
	{
		if (!t.IsGenericType || !IsOptionType(t))
		{
			return null;
		}
		return t.GenericTypeArguments[0];
	}

	public static object? GetValue(IObjectDescriptor objectDescriptor)
	{
		if (!IsOptionType(objectDescriptor.Type))
		{
			throw new InvalidOperationException("Should not be called on non-Option<> type");
		}
		if (objectDescriptor.Value == null)
		{
			return null;
		}
		return objectDescriptor.Type.GetProperty("Value").GetValue(objectDescriptor.Value);
	}

	public static bool IsFsharpListType(Type t)
	{
		if (t.Namespace == "Microsoft.FSharp.Collections")
		{
			return t.Name == "FSharpList`1";
		}
		return false;
	}

	public static object? CreateFsharpListFromArray(Type t, Type itemsType, Array arr)
	{
		if (!IsFsharpListType(t))
		{
			return null;
		}
		MethodInfo methodInfo = t.Assembly.GetType("Microsoft.FSharp.Collections.ListModule").GetMethod("OfArray").MakeGenericMethod(itemsType);
		object[] parameters = new Array[1] { arr };
		return methodInfo.Invoke(null, parameters);
	}
}
