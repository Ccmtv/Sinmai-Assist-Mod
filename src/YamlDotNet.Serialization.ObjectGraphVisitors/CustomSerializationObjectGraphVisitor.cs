using System.Collections.Generic;
using YamlDotNet.Core;
using YamlDotNet.Serialization.Utilities;

namespace YamlDotNet.Serialization.ObjectGraphVisitors;

internal sealed class CustomSerializationObjectGraphVisitor : ChainedObjectGraphVisitor
{
	private readonly TypeConverterCache typeConverters;

	private readonly ObjectSerializer nestedObjectSerializer;

	public CustomSerializationObjectGraphVisitor(IObjectGraphVisitor<IEmitter> nextVisitor, IEnumerable<IYamlTypeConverter> typeConverters, ObjectSerializer nestedObjectSerializer)
		: base(nextVisitor)
	{
		this.typeConverters = new TypeConverterCache(typeConverters);
		this.nestedObjectSerializer = nestedObjectSerializer;
	}

	public override bool Enter(IPropertyDescriptor? propertyDescriptor, IObjectDescriptor value, IEmitter context, ObjectSerializer serializer)
	{
		if (propertyDescriptor?.ConverterType != null)
		{
			IYamlTypeConverter converterByType = typeConverters.GetConverterByType(propertyDescriptor.ConverterType);
			converterByType.WriteYaml(context, value.Value, value.Type, serializer);
			return false;
		}
		if (typeConverters.TryGetConverterForType(value.Type, out IYamlTypeConverter typeConverter))
		{
			typeConverter.WriteYaml(context, value.Value, value.Type, serializer);
			return false;
		}
		if (value.Value is IYamlConvertible yamlConvertible)
		{
			yamlConvertible.Write(context, nestedObjectSerializer);
			return false;
		}
		if (value.Value is IYamlSerializable yamlSerializable)
		{
			yamlSerializable.WriteYaml(context);
			return false;
		}
		return base.Enter(propertyDescriptor, value, context, serializer);
	}
}
