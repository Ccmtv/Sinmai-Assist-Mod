using System;
using YamlDotNet.Serialization.NamingConventions;

namespace YamlDotNet.Serialization.Utilities;

internal class ReflectionTypeConverter : ITypeConverter
{
	public object? ChangeType(object? value, Type expectedType, ITypeInspector typeInspector)
	{
		return ChangeType(value, expectedType, NullNamingConvention.Instance, typeInspector);
	}

	public object? ChangeType(object? value, Type expectedType, INamingConvention enumNamingConvention, ITypeInspector typeInspector)
	{
		return TypeConverter.ChangeType(value, expectedType, enumNamingConvention, typeInspector);
	}
}
