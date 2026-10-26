using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using YamlDotNet.Core;

namespace YamlDotNet.Serialization.TypeInspectors;

internal sealed class ReadablePropertiesTypeInspector : ReflectionTypeInspector
{
	private sealed class ReflectionPropertyDescriptor : IPropertyDescriptor
	{
		private readonly PropertyInfo propertyInfo;

		private readonly ITypeResolver typeResolver;

		public string Name => propertyInfo.Name;

		public bool Required => propertyInfo.IsRequired();

		public Type Type => propertyInfo.PropertyType;

		public Type? TypeOverride { get; set; }

		public Type? ConverterType { get; set; }

		public bool AllowNulls => propertyInfo.AcceptsNull();

		public int Order { get; set; }

		public bool CanWrite => propertyInfo.CanWrite;

		public ScalarStyle ScalarStyle { get; set; }

		public ReflectionPropertyDescriptor(PropertyInfo propertyInfo, ITypeResolver typeResolver)
		{
			this.propertyInfo = propertyInfo ?? throw new ArgumentNullException("propertyInfo");
			this.typeResolver = typeResolver ?? throw new ArgumentNullException("typeResolver");
			ScalarStyle = ScalarStyle.Any;
			YamlConverterAttribute customAttribute = propertyInfo.GetCustomAttribute<YamlConverterAttribute>();
			if (customAttribute != null)
			{
				ConverterType = customAttribute.ConverterType;
			}
		}

		public void Write(object target, object? value)
		{
			propertyInfo.SetValue(target, value, null);
		}

		public T? GetCustomAttribute<T>() where T : Attribute
		{
			Attribute[] allCustomAttributes = propertyInfo.GetAllCustomAttributes<T>();
			return (T)allCustomAttributes.FirstOrDefault();
		}

		public IObjectDescriptor Read(object target)
		{
			object obj = propertyInfo.ReadValue(target);
			Type type = TypeOverride ?? typeResolver.Resolve(Type, obj);
			return new ObjectDescriptor(obj, type, Type, ScalarStyle);
		}
	}

	private readonly ITypeResolver typeResolver;

	private readonly bool includeNonPublicProperties;

	public ReadablePropertiesTypeInspector(ITypeResolver typeResolver)
		: this(typeResolver, includeNonPublicProperties: false)
	{
	}

	public ReadablePropertiesTypeInspector(ITypeResolver typeResolver, bool includeNonPublicProperties)
	{
		this.typeResolver = typeResolver ?? throw new ArgumentNullException("typeResolver");
		this.includeNonPublicProperties = includeNonPublicProperties;
	}

	private static bool IsValidProperty(PropertyInfo property)
	{
		if (property.CanRead)
		{
			return property.GetGetMethod(nonPublic: true).GetParameters().Length == 0;
		}
		return false;
	}

	public override IEnumerable<IPropertyDescriptor> GetProperties(Type type, object? container)
	{
		return type.GetProperties(includeNonPublicProperties).Where(IsValidProperty).Select<PropertyInfo, IPropertyDescriptor>((Func<PropertyInfo, IPropertyDescriptor>)((PropertyInfo p) => new ReflectionPropertyDescriptor(p, typeResolver)));
	}
}
