using System;

namespace YamlDotNet.Serialization.Utilities;

internal interface ITypeConverter
{
	object? ChangeType(object? value, Type expectedType, INamingConvention enumNamingConvention, ITypeInspector typeInspector);
}
