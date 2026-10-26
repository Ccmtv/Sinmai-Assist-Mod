using System;
using System.Collections.Generic;
using YamlDotNet.Serialization.Utilities;

namespace YamlDotNet.Serialization.ObjectGraphTraversalStrategies;

internal class RoundtripObjectGraphTraversalStrategy : FullObjectGraphTraversalStrategy
{
	private readonly TypeConverterCache converters;

	private readonly Settings settings;

	public RoundtripObjectGraphTraversalStrategy(IEnumerable<IYamlTypeConverter> converters, ITypeInspector typeDescriptor, ITypeResolver typeResolver, int maxRecursion, INamingConvention namingConvention, Settings settings, IObjectFactory factory)
		: base(typeDescriptor, typeResolver, maxRecursion, namingConvention, factory)
	{
		this.converters = new TypeConverterCache(converters);
		this.settings = settings;
	}

	protected override void TraverseProperties<TContext>(IObjectDescriptor value, IObjectGraphVisitor<TContext> visitor, TContext context, Stack<ObjectPathSegment> path, ObjectSerializer serializer)
	{
		if (!value.Type.HasDefaultConstructor(settings.AllowPrivateConstructors) && !converters.TryGetConverterForType(value.Type, out IYamlTypeConverter _))
		{
			throw new InvalidOperationException($"Type '{value.Type}' cannot be deserialized because it does not have a default constructor or a type converter.");
		}
		base.TraverseProperties(value, visitor, context, path, serializer);
	}
}
