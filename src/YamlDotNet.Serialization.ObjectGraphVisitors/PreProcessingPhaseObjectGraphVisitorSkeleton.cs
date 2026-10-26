using System;
using System.Collections.Generic;
using System.Linq;
using YamlDotNet.Serialization.Utilities;

namespace YamlDotNet.Serialization.ObjectGraphVisitors;

internal abstract class PreProcessingPhaseObjectGraphVisitorSkeleton : IObjectGraphVisitor<Nothing>
{
	protected readonly IEnumerable<IYamlTypeConverter> typeConverters;

	private readonly TypeConverterCache typeConverterCache;

	public PreProcessingPhaseObjectGraphVisitorSkeleton(IEnumerable<IYamlTypeConverter> typeConverters)
	{
		typeConverterCache = new TypeConverterCache((IYamlTypeConverter[])(this.typeConverters = typeConverters?.ToArray() ?? Array.Empty<IYamlTypeConverter>()));
	}

	bool IObjectGraphVisitor<Nothing>.Enter(IPropertyDescriptor? propertyDescriptor, IObjectDescriptor value, Nothing context, ObjectSerializer serializer)
	{
		if (typeConverterCache.TryGetConverterForType(value.Type, out IYamlTypeConverter _))
		{
			return false;
		}
		if (value.Value is IYamlConvertible)
		{
			return false;
		}
		if (value.Value is IYamlSerializable)
		{
			return false;
		}
		return Enter(value, serializer);
	}

	bool IObjectGraphVisitor<Nothing>.EnterMapping(IPropertyDescriptor key, IObjectDescriptor value, Nothing context, ObjectSerializer serializer)
	{
		return EnterMapping(key, value, serializer);
	}

	bool IObjectGraphVisitor<Nothing>.EnterMapping(IObjectDescriptor key, IObjectDescriptor value, Nothing context, ObjectSerializer serializer)
	{
		return EnterMapping(key, value, serializer);
	}

	void IObjectGraphVisitor<Nothing>.VisitMappingEnd(IObjectDescriptor mapping, Nothing context, ObjectSerializer serializer)
	{
		VisitMappingEnd(mapping, serializer);
	}

	void IObjectGraphVisitor<Nothing>.VisitMappingStart(IObjectDescriptor mapping, Type keyType, Type valueType, Nothing context, ObjectSerializer serializer)
	{
		VisitMappingStart(mapping, keyType, valueType, serializer);
	}

	void IObjectGraphVisitor<Nothing>.VisitScalar(IObjectDescriptor scalar, Nothing context, ObjectSerializer serializer)
	{
		VisitScalar(scalar, serializer);
	}

	void IObjectGraphVisitor<Nothing>.VisitSequenceEnd(IObjectDescriptor sequence, Nothing context, ObjectSerializer serializer)
	{
		VisitSequenceEnd(sequence, serializer);
	}

	void IObjectGraphVisitor<Nothing>.VisitSequenceStart(IObjectDescriptor sequence, Type elementType, Nothing context, ObjectSerializer serializer)
	{
		VisitSequenceStart(sequence, elementType, serializer);
	}

	protected abstract bool Enter(IObjectDescriptor value, ObjectSerializer serializer);

	protected abstract bool EnterMapping(IPropertyDescriptor key, IObjectDescriptor value, ObjectSerializer serializer);

	protected abstract bool EnterMapping(IObjectDescriptor key, IObjectDescriptor value, ObjectSerializer serializer);

	protected abstract void VisitMappingEnd(IObjectDescriptor mapping, ObjectSerializer serializer);

	protected abstract void VisitMappingStart(IObjectDescriptor mapping, Type keyType, Type valueType, ObjectSerializer serializer);

	protected abstract void VisitScalar(IObjectDescriptor scalar, ObjectSerializer serializer);

	protected abstract void VisitSequenceEnd(IObjectDescriptor sequence, ObjectSerializer serializer);

	protected abstract void VisitSequenceStart(IObjectDescriptor sequence, Type elementType, ObjectSerializer serializer);
}
