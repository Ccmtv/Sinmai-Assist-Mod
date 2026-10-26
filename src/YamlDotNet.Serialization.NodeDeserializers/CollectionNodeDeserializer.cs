using System;
using System.Collections;
using System.Collections.Generic;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Helpers;
using YamlDotNet.Serialization.Utilities;

namespace YamlDotNet.Serialization.NodeDeserializers;

internal sealed class CollectionNodeDeserializer : INodeDeserializer
{
	private readonly IObjectFactory objectFactory;

	private readonly INamingConvention enumNamingConvention;

	private readonly ITypeInspector typeInspector;

	public CollectionNodeDeserializer(IObjectFactory objectFactory, INamingConvention enumNamingConvention, ITypeInspector typeInspector)
	{
		this.objectFactory = objectFactory ?? throw new ArgumentNullException("objectFactory");
		this.enumNamingConvention = enumNamingConvention;
		this.typeInspector = typeInspector;
	}

	public bool Deserialize(IParser parser, Type expectedType, Func<IParser, Type, object?> nestedObjectDeserializer, out object? value, ObjectDeserializer rootDeserializer)
	{
		bool canUpdate = true;
		Type implementationOfOpenGenericInterface = expectedType.GetImplementationOfOpenGenericInterface(typeof(ICollection<>));
		Type type;
		IList list;
		if (implementationOfOpenGenericInterface != null)
		{
			Type[] genericArguments = implementationOfOpenGenericInterface.GetGenericArguments();
			type = genericArguments[0];
			value = objectFactory.Create(expectedType);
			list = value as IList;
			if (list == null)
			{
				Type implementationOfOpenGenericInterface2 = expectedType.GetImplementationOfOpenGenericInterface(typeof(IList<>));
				canUpdate = implementationOfOpenGenericInterface2 != null;
				list = (IList)Activator.CreateInstance(typeof(GenericCollectionToNonGenericAdapter<>).MakeGenericType(type), value);
			}
		}
		else
		{
			if (!typeof(IList).IsAssignableFrom(expectedType))
			{
				value = null;
				return false;
			}
			type = typeof(object);
			value = objectFactory.Create(expectedType);
			list = (IList)value;
		}
		DeserializeHelper(type, parser, nestedObjectDeserializer, list, canUpdate, enumNamingConvention, typeInspector);
		return true;
	}

	internal static void DeserializeHelper(Type tItem, IParser parser, Func<IParser, Type, object?> nestedObjectDeserializer, IList result, bool canUpdate, INamingConvention enumNamingConvention, ITypeInspector typeInspector, Action<int, object?>? promiseResolvedHandler = null)
	{
		parser.Consume<SequenceStart>();
		SequenceEnd @event;
		while (!parser.TryConsume<SequenceEnd>(out @event))
		{
			ParsingEvent current = parser.Current;
			object obj = nestedObjectDeserializer(parser, tItem);
			if (obj is IValuePromise valuePromise)
			{
				if (!canUpdate)
				{
					throw new ForwardAnchorNotSupportedException(current?.Start ?? Mark.Empty, current?.End ?? Mark.Empty, "Forward alias references are not allowed because this type does not implement IList<>");
				}
				int index = result.Add(tItem.IsValueType() ? Activator.CreateInstance(tItem) : null);
				if (promiseResolvedHandler != null)
				{
					valuePromise.ValueAvailable += delegate(object? v)
					{
						promiseResolvedHandler(index, v);
					};
				}
				else
				{
					valuePromise.ValueAvailable += delegate(object? v)
					{
						result[index] = TypeConverter.ChangeType(v, tItem, enumNamingConvention, typeInspector);
					};
				}
			}
			else
			{
				result.Add(TypeConverter.ChangeType(obj, tItem, enumNamingConvention, typeInspector));
			}
		}
	}
}
