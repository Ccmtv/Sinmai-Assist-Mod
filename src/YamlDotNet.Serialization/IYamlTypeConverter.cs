using System;
using YamlDotNet.Core;

namespace YamlDotNet.Serialization;

internal interface IYamlTypeConverter
{
	bool Accepts(Type type);

	object? ReadYaml(IParser parser, Type type, ObjectDeserializer rootDeserializer);

	void WriteYaml(IEmitter emitter, object? value, Type type, ObjectSerializer serializer);
}
