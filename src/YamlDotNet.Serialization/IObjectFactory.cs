using System;
using System.Collections;

namespace YamlDotNet.Serialization;

internal interface IObjectFactory
{
	object Create(Type type);

	object? CreatePrimitive(Type type);

	bool GetDictionary(IObjectDescriptor descriptor, out IDictionary? dictionary, out Type[]? genericArguments);

	Type GetValueType(Type type);

	void ExecuteOnDeserializing(object value);

	void ExecuteOnDeserialized(object value);

	void ExecuteOnSerializing(object value);

	void ExecuteOnSerialized(object value);
}
