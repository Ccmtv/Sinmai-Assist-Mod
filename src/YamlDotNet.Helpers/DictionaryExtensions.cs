using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace YamlDotNet.Helpers;

internal static class DictionaryExtensions
{
	public static bool TryAdd<T, V>(this Dictionary<T, V> dictionary, T key, V value)
	{
		if (dictionary.ContainsKey(key))
		{
			return false;
		}
		dictionary.Add(key, value);
		return true;
	}

	public static TValue GetOrAdd<TKey, TValue, TArg>(this ConcurrentDictionary<TKey, TValue> dictionary, TKey key, Func<TKey, TArg, TValue> valueFactory, TArg arg)
	{
		if (dictionary == null)
		{
			throw new ArgumentNullException("dictionary");
		}
		if (key == null)
		{
			throw new ArgumentNullException("key");
		}
		if (valueFactory == null)
		{
			throw new ArgumentNullException("valueFactory");
		}
		TValue value;
		do
		{
			if (dictionary.TryGetValue(key, out value))
			{
				return value;
			}
			value = valueFactory(key, arg);
		}
		while (!dictionary.TryAdd(key, value));
		return value;
	}
}
