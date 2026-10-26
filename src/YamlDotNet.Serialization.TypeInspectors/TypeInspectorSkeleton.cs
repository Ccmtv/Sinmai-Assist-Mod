using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.Serialization;

namespace YamlDotNet.Serialization.TypeInspectors;

internal abstract class TypeInspectorSkeleton : ITypeInspector
{
	public abstract string GetEnumName(Type enumType, string name);

	public abstract string GetEnumValue(object enumValue);

	public abstract IEnumerable<IPropertyDescriptor> GetProperties(Type type, object? container);

	public IPropertyDescriptor GetProperty(Type type, object? container, string name, [MaybeNullWhen(true)] bool ignoreUnmatched, bool caseInsensitivePropertyMatching)
	{
		IEnumerable<IPropertyDescriptor> enumerable = ((!caseInsensitivePropertyMatching) ? (from p in GetProperties(type, container)
			where p.Name == name
			select p) : (from p in GetProperties(type, container)
			where p.Name.Equals(name, StringComparison.OrdinalIgnoreCase)
			select p));
		using IEnumerator<IPropertyDescriptor> enumerator = enumerable.GetEnumerator();
		if (!enumerator.MoveNext())
		{
			if (ignoreUnmatched)
			{
				return null;
			}
			throw new SerializationException("Property '" + name + "' not found on type '" + type.FullName + "'.");
		}
		IPropertyDescriptor current = enumerator.Current;
		if (enumerator.MoveNext())
		{
			throw new SerializationException("Multiple properties with the name/alias '" + name + "' already exists on type '" + type.FullName + "', maybe you're misusing YamlAlias or maybe you are using the wrong naming convention? The matching properties are: " + string.Join(", ", enumerable.Select((IPropertyDescriptor p) => p.Name).ToArray()));
		}
		return current;
	}
}
