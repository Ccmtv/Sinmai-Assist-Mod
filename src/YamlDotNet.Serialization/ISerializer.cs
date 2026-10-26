using System;
using System.IO;
using YamlDotNet.Core;

namespace YamlDotNet.Serialization;

internal interface ISerializer
{
	string Serialize(object? graph);

	string Serialize(object? graph, Type type);

	void Serialize(TextWriter writer, object? graph);

	void Serialize(TextWriter writer, object? graph, Type type);

	void Serialize(IEmitter emitter, object? graph);

	void Serialize(IEmitter emitter, object? graph, Type type);
}
