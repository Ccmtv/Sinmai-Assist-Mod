using System;
using System.Collections;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;

namespace YamlDotNet.Serialization.NodeDeserializers;

internal abstract class DictionaryDeserializer
{
	private readonly bool duplicateKeyChecking;

	public DictionaryDeserializer(bool duplicateKeyChecking)
	{
		this.duplicateKeyChecking = duplicateKeyChecking;
	}

	private void TryAssign(IDictionary result, object key, object value, MappingStart propertyName)
	{
		if (duplicateKeyChecking && result.Contains(key))
		{
			throw new YamlException(propertyName.Start, propertyName.End, $"Encountered duplicate key {key}");
		}
		result[key] = value;
	}

	protected virtual void Deserialize(Type tKey, Type tValue, IParser parser, Func<IParser, Type, object?> nestedObjectDeserializer, IDictionary result, ObjectDeserializer rootDeserializer)
	{
		MappingStart property = parser.Consume<MappingStart>();
		MappingEnd @event;
		while (!parser.TryConsume<MappingEnd>(out @event))
		{
			object key = nestedObjectDeserializer(parser, tKey);
			object value = nestedObjectDeserializer(parser, tValue);
			IValuePromise valuePromise = value as IValuePromise;
			if (key is IValuePromise valuePromise2)
			{
				if (valuePromise == null)
				{
					valuePromise2.ValueAvailable += delegate(object? v)
					{
						result[v] = value;
					};
					continue;
				}
				bool hasFirstPart = false;
				valuePromise2.ValueAvailable += delegate(object? v)
				{
					if (hasFirstPart)
					{
						TryAssign(result, v, value, property);
					}
					else
					{
						key = v;
						hasFirstPart = true;
					}
				};
				valuePromise.ValueAvailable += delegate(object? v)
				{
					if (hasFirstPart)
					{
						TryAssign(result, key, v, property);
					}
					else
					{
						value = v;
						hasFirstPart = true;
					}
				};
				continue;
			}
			if (key == null)
			{
				throw new ArgumentException("Empty key names are not supported yet.", "key");
			}
			if (valuePromise == null)
			{
				TryAssign(result, key, value, property);
				continue;
			}
			valuePromise.ValueAvailable += delegate(object? v)
			{
				result[key] = v;
			};
		}
	}
}
