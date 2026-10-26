using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using YamlDotNet.Helpers;

namespace YamlDotNet.Serialization.TypeInspectors;

internal sealed class CachedTypeInspector : TypeInspectorSkeleton
{
	private readonly ITypeInspector innerTypeDescriptor;

	private readonly ConcurrentDictionary<Type, List<IPropertyDescriptor>> cache = new ConcurrentDictionary<Type, List<IPropertyDescriptor>>();

	private readonly ConcurrentDictionary<Type, ConcurrentDictionary<string, string>> enumNameCache = new ConcurrentDictionary<Type, ConcurrentDictionary<string, string>>();

	private readonly ConcurrentDictionary<object, string> enumValueCache = new ConcurrentDictionary<object, string>();

	public CachedTypeInspector(ITypeInspector innerTypeDescriptor)
	{
		this.innerTypeDescriptor = innerTypeDescriptor ?? throw new ArgumentNullException("innerTypeDescriptor");
	}

	public override string GetEnumName(Type enumType, string name)
	{
		ConcurrentDictionary<string, string> orAdd = enumNameCache.GetOrAdd(enumType, (Type _) => new ConcurrentDictionary<string, string>());
		return DictionaryExtensions.GetOrAdd(orAdd, name, delegate(string n, (Type enumType, ITypeInspector innerTypeDescriptor) context)
		{
			var (enumType2, typeInspector) = context;
			return typeInspector.GetEnumName(enumType2, n);
		}, (enumType, innerTypeDescriptor));
	}

	public override string GetEnumValue(object enumValue)
	{
		return DictionaryExtensions.GetOrAdd(enumValueCache, enumValue, delegate(object _, (object enumValue, ITypeInspector innerTypeDescriptor) context)
		{
			var (enumValue2, typeInspector) = context;
			return typeInspector.GetEnumValue(enumValue2);
		}, (enumValue, innerTypeDescriptor));
	}

	public override IEnumerable<IPropertyDescriptor> GetProperties(Type type, object? container)
	{
		return DictionaryExtensions.GetOrAdd(cache, type, delegate(Type t, (object container, ITypeInspector innerTypeDescriptor) context)
		{
			var (container2, typeInspector) = context;
			return typeInspector.GetProperties(t, container2).ToList();
		}, (container, innerTypeDescriptor));
	}
}
