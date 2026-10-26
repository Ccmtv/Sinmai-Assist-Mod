using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Helpers;
using YamlDotNet.Serialization.Utilities;

namespace YamlDotNet.Serialization.NodeDeserializers;

internal sealed class ObjectNodeDeserializer : INodeDeserializer
{
	private readonly IObjectFactory objectFactory;

	private readonly ITypeInspector typeInspector;

	private readonly bool ignoreUnmatched;

	private readonly bool duplicateKeyChecking;

	private readonly ITypeConverter typeConverter;

	private readonly INamingConvention enumNamingConvention;

	private readonly bool enforceNullability;

	private readonly bool caseInsensitivePropertyMatching;

	private readonly bool enforceRequiredProperties;

	private readonly TypeConverterCache typeConverters;

	public ObjectNodeDeserializer(IObjectFactory objectFactory, ITypeInspector typeInspector, bool ignoreUnmatched, bool duplicateKeyChecking, ITypeConverter typeConverter, INamingConvention enumNamingConvention, bool enforceNullability, bool caseInsensitivePropertyMatching, bool enforceRequiredProperties, IEnumerable<IYamlTypeConverter> typeConverters)
	{
		this.objectFactory = objectFactory ?? throw new ArgumentNullException("objectFactory");
		this.typeInspector = typeInspector ?? throw new ArgumentNullException("typeInspector");
		this.ignoreUnmatched = ignoreUnmatched;
		this.duplicateKeyChecking = duplicateKeyChecking;
		this.typeConverter = typeConverter ?? throw new ArgumentNullException("typeConverter");
		this.enumNamingConvention = enumNamingConvention ?? throw new ArgumentNullException("enumNamingConvention");
		this.enforceNullability = enforceNullability;
		this.caseInsensitivePropertyMatching = caseInsensitivePropertyMatching;
		this.enforceRequiredProperties = enforceRequiredProperties;
		this.typeConverters = new TypeConverterCache(typeConverters);
	}

	public bool Deserialize(IParser parser, Type expectedType, Func<IParser, Type, object?> nestedObjectDeserializer, out object? value, ObjectDeserializer rootDeserializer)
	{
		if (!parser.TryConsume<MappingStart>(out var _))
		{
			value = null;
			return false;
		}
		Type type = Nullable.GetUnderlyingType(expectedType) ?? FsharpHelper.GetOptionUnderlyingType(expectedType) ?? expectedType;
		value = objectFactory.Create(type);
		objectFactory.ExecuteOnDeserializing(value);
		HashSet<string> hashSet = new HashSet<string>(StringComparer.Ordinal);
		HashSet<string> hashSet2 = new HashSet<string>(StringComparer.Ordinal);
		Mark start = Mark.Empty;
		MappingEnd event2;
		while (!parser.TryConsume<MappingEnd>(out event2))
		{
			Scalar propertyName = parser.Consume<Scalar>();
			if (duplicateKeyChecking && !hashSet.Add(propertyName.Value))
			{
				throw new YamlException(propertyName.Start, propertyName.End, "Encountered duplicate key " + propertyName.Value);
			}
			try
			{
				IPropertyDescriptor property = typeInspector.GetProperty(type, null, propertyName.Value, ignoreUnmatched, caseInsensitivePropertyMatching);
				if (property == null)
				{
					parser.SkipThisAndNestedEvents();
					continue;
				}
				hashSet2.Add(property.Name);
				object obj;
				if (property.ConverterType != null)
				{
					IYamlTypeConverter converterByType = typeConverters.GetConverterByType(property.ConverterType);
					obj = converterByType.ReadYaml(parser, property.Type, rootDeserializer);
				}
				else
				{
					obj = nestedObjectDeserializer(parser, property.Type);
				}
				if (obj is IValuePromise valuePromise)
				{
					object valueRef = value;
					valuePromise.ValueAvailable += delegate(object? v)
					{
						object value3 = typeConverter.ChangeType(v, property.Type, enumNamingConvention, typeInspector);
						NullCheck(value3, property, propertyName);
						property.Write(valueRef, value3);
					};
				}
				else
				{
					object value2 = typeConverter.ChangeType(obj, property.Type, enumNamingConvention, typeInspector);
					NullCheck(value2, property, propertyName);
					property.Write(value, value2);
				}
			}
			catch (SerializationException ex)
			{
				throw new YamlException(propertyName.Start, propertyName.End, ex.Message);
			}
			catch (YamlException)
			{
				throw;
			}
			catch (Exception innerException)
			{
				throw new YamlException(propertyName.Start, propertyName.End, "Exception during deserialization", innerException);
			}
			start = propertyName.End;
		}
		if (enforceRequiredProperties)
		{
			IEnumerable<IPropertyDescriptor> properties = typeInspector.GetProperties(type, value);
			List<string> list = new List<string>();
			foreach (IPropertyDescriptor item in properties)
			{
				if (item.Required && !hashSet2.Contains(item.Name))
				{
					list.Add(item.Name);
				}
			}
			if (list.Count > 0)
			{
				string text = string.Join(",", list);
				throw new YamlException(in start, in start, "Missing properties, '" + text + "' in source yaml.");
			}
		}
		objectFactory.ExecuteOnDeserialized(value);
		return true;
	}

	public void NullCheck(object value, IPropertyDescriptor property, Scalar propertyName)
	{
		if (enforceNullability && value == null && !property.AllowNulls)
		{
			throw new YamlException(propertyName.Start, propertyName.End, "Strict nullability enforcement error.", new NullReferenceException("Yaml value is null when target property requires non null values."));
		}
	}
}
