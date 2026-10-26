using System;

namespace YamlDotNet.Serialization.Callbacks;

[AttributeUsage(AttributeTargets.Method)]
internal sealed class OnDeserializingAttribute : Attribute
{
}
