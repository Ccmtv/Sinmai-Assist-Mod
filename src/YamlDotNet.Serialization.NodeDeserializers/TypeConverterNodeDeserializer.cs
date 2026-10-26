using System;
using System.Collections.Generic;
using YamlDotNet.Core;
using YamlDotNet.Serialization.Utilities;

namespace YamlDotNet.Serialization.NodeDeserializers;

internal sealed class TypeConverterNodeDeserializer : INodeDeserializer
{
	private readonly TypeConverterCache converters;

	public TypeConverterNodeDeserializer(IEnumerable<IYamlTypeConverter> converters)
	{
		this.converters = new TypeConverterCache(converters);
	}

	public bool Deserialize(IParser parser, Type expectedType, Func<IParser, Type, object?> nestedObjectDeserializer, out object? value, ObjectDeserializer rootDeserializer)
	{
		if (!converters.TryGetConverterForType(expectedType, out IYamlTypeConverter typeConverter))
		{
			value = null;
			return false;
		}
		value = typeConverter.ReadYaml(parser, expectedType, rootDeserializer);
		return true;
	}
}
