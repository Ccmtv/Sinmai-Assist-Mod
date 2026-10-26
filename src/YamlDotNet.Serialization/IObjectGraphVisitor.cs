using System;

namespace YamlDotNet.Serialization;

internal interface IObjectGraphVisitor<TContext>
{
	bool Enter(IPropertyDescriptor? propertyDescriptor, IObjectDescriptor value, TContext context, ObjectSerializer serializer);

	bool EnterMapping(IObjectDescriptor key, IObjectDescriptor value, TContext context, ObjectSerializer serializer);

	bool EnterMapping(IPropertyDescriptor key, IObjectDescriptor value, TContext context, ObjectSerializer serializer);

	void VisitScalar(IObjectDescriptor scalar, TContext context, ObjectSerializer serializer);

	void VisitMappingStart(IObjectDescriptor mapping, Type keyType, Type valueType, TContext context, ObjectSerializer serializer);

	void VisitMappingEnd(IObjectDescriptor mapping, TContext context, ObjectSerializer serializer);

	void VisitSequenceStart(IObjectDescriptor sequence, Type elementType, TContext context, ObjectSerializer serializer);

	void VisitSequenceEnd(IObjectDescriptor sequence, TContext context, ObjectSerializer serializer);
}
