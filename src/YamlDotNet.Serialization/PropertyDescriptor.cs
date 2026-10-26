using System;
using YamlDotNet.Core;

namespace YamlDotNet.Serialization;

internal sealed class PropertyDescriptor : IPropertyDescriptor
{
	private readonly IPropertyDescriptor baseDescriptor;

	public bool AllowNulls => baseDescriptor.AllowNulls;

	public string Name { get; set; }

	public bool Required => baseDescriptor.Required;

	public Type Type => baseDescriptor.Type;

	public Type? TypeOverride
	{
		get
		{
			return baseDescriptor.TypeOverride;
		}
		set
		{
			baseDescriptor.TypeOverride = value;
		}
	}

	public Type? ConverterType => baseDescriptor.ConverterType;

	public int Order { get; set; }

	public ScalarStyle ScalarStyle
	{
		get
		{
			return baseDescriptor.ScalarStyle;
		}
		set
		{
			baseDescriptor.ScalarStyle = value;
		}
	}

	public bool CanWrite => baseDescriptor.CanWrite;

	public PropertyDescriptor(IPropertyDescriptor baseDescriptor)
	{
		this.baseDescriptor = baseDescriptor;
		Name = baseDescriptor.Name;
	}

	public void Write(object target, object? value)
	{
		baseDescriptor.Write(target, value);
	}

	public T? GetCustomAttribute<T>() where T : Attribute
	{
		return baseDescriptor.GetCustomAttribute<T>();
	}

	public IObjectDescriptor Read(object target)
	{
		return baseDescriptor.Read(target);
	}
}
