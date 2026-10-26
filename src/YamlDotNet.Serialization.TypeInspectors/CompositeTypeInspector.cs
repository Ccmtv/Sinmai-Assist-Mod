using System;
using System.Collections.Generic;
using System.Linq;

namespace YamlDotNet.Serialization.TypeInspectors;

internal sealed class CompositeTypeInspector : TypeInspectorSkeleton
{
	private readonly IEnumerable<ITypeInspector> typeInspectors;

	public CompositeTypeInspector(params ITypeInspector[] typeInspectors)
		: this((IEnumerable<ITypeInspector>)typeInspectors)
	{
	}

	public CompositeTypeInspector(IEnumerable<ITypeInspector> typeInspectors)
	{
		this.typeInspectors = typeInspectors?.ToList() ?? throw new ArgumentNullException("typeInspectors");
	}

	public override string GetEnumName(Type enumType, string name)
	{
		foreach (ITypeInspector typeInspector in typeInspectors)
		{
			try
			{
				return typeInspector.GetEnumName(enumType, name);
			}
			catch
			{
			}
		}
		throw new ArgumentOutOfRangeException("enumType,name", "Name not found on enum type");
	}

	public override string GetEnumValue(object enumValue)
	{
		if (enumValue == null)
		{
			throw new ArgumentNullException("enumValue");
		}
		foreach (ITypeInspector typeInspector in typeInspectors)
		{
			try
			{
				return typeInspector.GetEnumValue(enumValue);
			}
			catch
			{
			}
		}
		throw new ArgumentOutOfRangeException("enumValue", $"Value not found for ({enumValue})");
	}

	public override IEnumerable<IPropertyDescriptor> GetProperties(Type type, object? container)
	{
		return typeInspectors.SelectMany((ITypeInspector i) => i.GetProperties(type, container));
	}
}
