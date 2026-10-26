using System;
using YamlDotNet.Core;

namespace YamlDotNet.Serialization.ObjectGraphVisitors;

internal abstract class ChainedObjectGraphVisitor : IObjectGraphVisitor<IEmitter>
{
	private readonly IObjectGraphVisitor<IEmitter> nextVisitor;

	protected ChainedObjectGraphVisitor(IObjectGraphVisitor<IEmitter> nextVisitor)
	{
		this.nextVisitor = nextVisitor;
	}

	public virtual bool Enter(IPropertyDescriptor? propertyDescriptor, IObjectDescriptor value, IEmitter context, ObjectSerializer serializer)
	{
		return nextVisitor.Enter(propertyDescriptor, value, context, serializer);
	}

	public virtual bool EnterMapping(IObjectDescriptor key, IObjectDescriptor value, IEmitter context, ObjectSerializer serializer)
	{
		return nextVisitor.EnterMapping(key, value, context, serializer);
	}

	public virtual bool EnterMapping(IPropertyDescriptor key, IObjectDescriptor value, IEmitter context, ObjectSerializer serializer)
	{
		return nextVisitor.EnterMapping(key, value, context, serializer);
	}

	public virtual void VisitScalar(IObjectDescriptor scalar, IEmitter context, ObjectSerializer serializer)
	{
		nextVisitor.VisitScalar(scalar, context, serializer);
	}

	public virtual void VisitMappingStart(IObjectDescriptor mapping, Type keyType, Type valueType, IEmitter context, ObjectSerializer serializer)
	{
		nextVisitor.VisitMappingStart(mapping, keyType, valueType, context, serializer);
	}

	public virtual void VisitMappingEnd(IObjectDescriptor mapping, IEmitter context, ObjectSerializer serializer)
	{
		nextVisitor.VisitMappingEnd(mapping, context, serializer);
	}

	public virtual void VisitSequenceStart(IObjectDescriptor sequence, Type elementType, IEmitter context, ObjectSerializer serializer)
	{
		nextVisitor.VisitSequenceStart(sequence, elementType, context, serializer);
	}

	public virtual void VisitSequenceEnd(IObjectDescriptor sequence, IEmitter context, ObjectSerializer serializer)
	{
		nextVisitor.VisitSequenceEnd(sequence, context, serializer);
	}
}
