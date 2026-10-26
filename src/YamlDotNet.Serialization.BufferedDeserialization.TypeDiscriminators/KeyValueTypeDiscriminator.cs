using System;
using System.Collections.Generic;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;

namespace YamlDotNet.Serialization.BufferedDeserialization.TypeDiscriminators;

internal class KeyValueTypeDiscriminator : ITypeDiscriminator
{
	private readonly string targetKey;

	private readonly IDictionary<string, Type> typeMapping;

	public Type BaseType { get; private set; }

	public KeyValueTypeDiscriminator(Type baseType, string targetKey, IDictionary<string, Type> typeMapping)
	{
		foreach (KeyValuePair<string, Type> item in typeMapping)
		{
			if (!baseType.IsAssignableFrom(item.Value))
			{
				throw new ArgumentOutOfRangeException("typeMapping", $"{item.Value} is not a assignable to {baseType}");
			}
		}
		BaseType = baseType;
		this.targetKey = targetKey;
		this.typeMapping = typeMapping;
	}

	public bool TryDiscriminate(IParser parser, out Type? suggestedType)
	{
		if (parser.TryFindMappingEntry((Scalar scalar2) => targetKey == scalar2.Value, out Scalar _, out ParsingEvent value) && value is Scalar scalar && typeMapping.TryGetValue(scalar.Value, out Type value2))
		{
			suggestedType = value2;
			return true;
		}
		suggestedType = null;
		return false;
	}
}
