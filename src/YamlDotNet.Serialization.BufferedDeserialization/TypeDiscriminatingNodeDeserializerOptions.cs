using System;
using System.Collections.Generic;
using YamlDotNet.Serialization.BufferedDeserialization.TypeDiscriminators;

namespace YamlDotNet.Serialization.BufferedDeserialization;

internal class TypeDiscriminatingNodeDeserializerOptions : ITypeDiscriminatingNodeDeserializerOptions
{
	internal readonly List<ITypeDiscriminator> discriminators = new List<ITypeDiscriminator>();

	public void AddTypeDiscriminator(ITypeDiscriminator discriminator)
	{
		discriminators.Add(discriminator);
	}

	public void AddKeyValueTypeDiscriminator<T>(string discriminatorKey, IDictionary<string, Type> valueTypeMapping)
	{
		discriminators.Add(new KeyValueTypeDiscriminator(typeof(T), discriminatorKey, valueTypeMapping));
	}

	public void AddUniqueKeyTypeDiscriminator<T>(IDictionary<string, Type> uniqueKeyTypeMapping)
	{
		discriminators.Add(new UniqueKeyTypeDiscriminator(typeof(T), uniqueKeyTypeMapping));
	}
}
