using System;

namespace YamlDotNet.Serialization;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
internal sealed class YamlConverterAttribute : Attribute
{
	public Type ConverterType { get; }

	public YamlConverterAttribute(Type converterType)
	{
		ConverterType = converterType;
	}
}
