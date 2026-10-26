using System;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;

namespace YamlDotNet.Serialization.NodeTypeResolvers;

internal class PreventUnknownTagsNodeTypeResolver : INodeTypeResolver
{
	bool INodeTypeResolver.Resolve(NodeEvent? nodeEvent, ref Type currentType)
	{
		if (nodeEvent != null && !nodeEvent.Tag.IsEmpty)
		{
			throw new YamlException(nodeEvent.Start, nodeEvent.End, $"Encountered an unresolved tag '{nodeEvent.Tag}'");
		}
		return false;
	}
}
