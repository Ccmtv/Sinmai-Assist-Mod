using System;
using System.Collections;
using System.Collections.Generic;
using YamlDotNet.Core;

namespace YamlDotNet.Serialization.NodeDeserializers;

internal sealed class EnumerableNodeDeserializer : INodeDeserializer
{
	public bool Deserialize(IParser parser, Type expectedType, Func<IParser, Type, object?> nestedObjectDeserializer, out object? value, ObjectDeserializer rootDeserializer)
	{
		Type type;
		if (expectedType == typeof(IEnumerable))
		{
			type = typeof(object);
		}
		else
		{
			Type implementationOfOpenGenericInterface = expectedType.GetImplementationOfOpenGenericInterface(typeof(IEnumerable<>));
			if (implementationOfOpenGenericInterface != expectedType)
			{
				value = null;
				return false;
			}
			type = implementationOfOpenGenericInterface.GetGenericArguments()[0];
		}
		Type arg = typeof(List<>).MakeGenericType(type);
		value = nestedObjectDeserializer(parser, arg);
		return true;
	}
}
