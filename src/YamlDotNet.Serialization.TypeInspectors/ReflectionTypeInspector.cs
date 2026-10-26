using System;

namespace YamlDotNet.Serialization.TypeInspectors;

internal abstract class ReflectionTypeInspector : TypeInspectorSkeleton
{
	public override string GetEnumName(Type enumType, string name)
	{
		return name;
	}

	public override string GetEnumValue(object enumValue)
	{
		if (enumValue == null)
		{
			return string.Empty;
		}
		return enumValue.ToString();
	}
}
