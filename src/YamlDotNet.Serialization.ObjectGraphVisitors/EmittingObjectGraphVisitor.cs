using System;
using YamlDotNet.Core;

namespace YamlDotNet.Serialization.ObjectGraphVisitors;

internal sealed class EmittingObjectGraphVisitor : IObjectGraphVisitor<IEmitter>
{
	private readonly IEventEmitter eventEmitter;

	public EmittingObjectGraphVisitor(IEventEmitter eventEmitter)
	{
		this.eventEmitter = eventEmitter;
	}

	bool IObjectGraphVisitor<IEmitter>.Enter(IPropertyDescriptor? propertyDescriptor, IObjectDescriptor value, IEmitter context, ObjectSerializer serializer)
	{
		return true;
	}

	bool IObjectGraphVisitor<IEmitter>.EnterMapping(IObjectDescriptor key, IObjectDescriptor value, IEmitter context, ObjectSerializer serializer)
	{
		return true;
	}

	bool IObjectGraphVisitor<IEmitter>.EnterMapping(IPropertyDescriptor key, IObjectDescriptor value, IEmitter context, ObjectSerializer serializer)
	{
		return true;
	}

	void IObjectGraphVisitor<IEmitter>.VisitScalar(IObjectDescriptor scalar, IEmitter context, ObjectSerializer serializer)
	{
		eventEmitter.Emit(new ScalarEventInfo(scalar), context);
	}

	void IObjectGraphVisitor<IEmitter>.VisitMappingStart(IObjectDescriptor mapping, Type keyType, Type valueType, IEmitter context, ObjectSerializer serializer)
	{
		eventEmitter.Emit(new MappingStartEventInfo(mapping), context);
	}

	void IObjectGraphVisitor<IEmitter>.VisitMappingEnd(IObjectDescriptor mapping, IEmitter context, ObjectSerializer serializer)
	{
		eventEmitter.Emit(new MappingEndEventInfo(mapping), context);
	}

	void IObjectGraphVisitor<IEmitter>.VisitSequenceStart(IObjectDescriptor sequence, Type elementType, IEmitter context, ObjectSerializer serializer)
	{
		eventEmitter.Emit(new SequenceStartEventInfo(sequence), context);
	}

	void IObjectGraphVisitor<IEmitter>.VisitSequenceEnd(IObjectDescriptor sequence, IEmitter context, ObjectSerializer serializer)
	{
		eventEmitter.Emit(new SequenceEndEventInfo(sequence), context);
	}
}
