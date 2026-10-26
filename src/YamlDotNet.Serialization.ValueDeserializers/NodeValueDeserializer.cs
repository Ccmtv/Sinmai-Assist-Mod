using System;
using System.Collections.Generic;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization.Utilities;

namespace YamlDotNet.Serialization.ValueDeserializers;

internal sealed class NodeValueDeserializer : IValueDeserializer
{
	private readonly IList<INodeDeserializer> deserializers;

	private readonly IList<INodeTypeResolver> typeResolvers;

	private readonly ITypeConverter typeConverter;

	private readonly INamingConvention enumNamingConvention;

	private readonly ITypeInspector typeInspector;

	public NodeValueDeserializer(IList<INodeDeserializer> deserializers, IList<INodeTypeResolver> typeResolvers, ITypeConverter typeConverter, INamingConvention enumNamingConvention, ITypeInspector typeInspector)
	{
		this.deserializers = deserializers ?? throw new ArgumentNullException("deserializers");
		this.typeResolvers = typeResolvers ?? throw new ArgumentNullException("typeResolvers");
		this.typeConverter = typeConverter ?? throw new ArgumentNullException("typeConverter");
		this.enumNamingConvention = enumNamingConvention ?? throw new ArgumentNullException("enumNamingConvention");
		this.typeInspector = typeInspector;
	}

	public object? DeserializeValue(IParser parser, Type expectedType, SerializerState state, IValueDeserializer nestedObjectDeserializer)
	{
		parser.Accept<NodeEvent>(out var @event);
		Type typeFromEvent = GetTypeFromEvent(@event, expectedType);
		ObjectDeserializer rootDeserializer = (Type x) => DeserializeValue(parser, x, state, nestedObjectDeserializer);
		try
		{
			foreach (INodeDeserializer deserializer in deserializers)
			{
				if (deserializer.Deserialize(parser, typeFromEvent, (IParser r, Type t) => nestedObjectDeserializer.DeserializeValue(r, t, state, nestedObjectDeserializer), out object value, rootDeserializer))
				{
					return typeConverter.ChangeType(value, expectedType, enumNamingConvention, typeInspector);
				}
			}
		}
		catch (YamlException)
		{
			throw;
		}
		catch (Exception innerException)
		{
			throw new YamlException(@event?.Start ?? Mark.Empty, @event?.End ?? Mark.Empty, "Exception during deserialization", innerException);
		}
		throw new YamlException(@event?.Start ?? Mark.Empty, @event?.End ?? Mark.Empty, "No node deserializer was able to deserialize the node into type " + expectedType.AssemblyQualifiedName);
	}

	private Type GetTypeFromEvent(NodeEvent? nodeEvent, Type currentType)
	{
		foreach (INodeTypeResolver typeResolver in typeResolvers)
		{
			if (typeResolver.Resolve(nodeEvent, ref currentType))
			{
				break;
			}
		}
		return currentType;
	}
}
