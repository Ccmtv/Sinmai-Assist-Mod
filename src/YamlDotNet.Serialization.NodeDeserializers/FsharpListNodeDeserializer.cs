using System;
using System.Collections;
using YamlDotNet.Core;
using YamlDotNet.Helpers;

namespace YamlDotNet.Serialization.NodeDeserializers;

internal sealed class FsharpListNodeDeserializer : INodeDeserializer
{
	private readonly ITypeInspector typeInspector;

	private readonly INamingConvention enumNamingConvention;

	public FsharpListNodeDeserializer(ITypeInspector typeInspector, INamingConvention enumNamingConvention)
	{
		this.typeInspector = typeInspector;
		this.enumNamingConvention = enumNamingConvention;
	}

	public bool Deserialize(IParser parser, Type expectedType, Func<IParser, Type, object?> nestedObjectDeserializer, out object? value, ObjectDeserializer rootDeserializer)
	{
		if (!FsharpHelper.IsFsharpListType(expectedType))
		{
			value = false;
			return false;
		}
		Type type = expectedType.GetGenericArguments()[0];
		Type t = expectedType.GetGenericTypeDefinition().MakeGenericType(type);
		ArrayList arrayList = new ArrayList();
		CollectionNodeDeserializer.DeserializeHelper(type, parser, nestedObjectDeserializer, arrayList, canUpdate: true, enumNamingConvention, typeInspector);
		Array array = Array.CreateInstance(type, arrayList.Count);
		arrayList.CopyTo(array, 0);
		object obj = FsharpHelper.CreateFsharpListFromArray(t, type, array);
		value = obj;
		return true;
	}
}
