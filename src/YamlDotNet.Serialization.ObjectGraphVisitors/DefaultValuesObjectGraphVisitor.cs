using System;
using System.Collections;
using System.ComponentModel;
using YamlDotNet.Core;

namespace YamlDotNet.Serialization.ObjectGraphVisitors;

internal sealed class DefaultValuesObjectGraphVisitor : ChainedObjectGraphVisitor
{
	private readonly DefaultValuesHandling handling;

	private readonly IObjectFactory factory;

	public DefaultValuesObjectGraphVisitor(DefaultValuesHandling handling, IObjectGraphVisitor<IEmitter> nextVisitor, IObjectFactory factory)
		: base(nextVisitor)
	{
		this.handling = handling;
		this.factory = factory;
	}

	private object? GetDefault(Type type)
	{
		return factory.CreatePrimitive(type);
	}

	public override bool EnterMapping(IPropertyDescriptor key, IObjectDescriptor value, IEmitter context, ObjectSerializer serializer)
	{
		DefaultValuesHandling defaultValuesHandling = handling;
		YamlMemberAttribute customAttribute = key.GetCustomAttribute<YamlMemberAttribute>();
		if (customAttribute != null && customAttribute.IsDefaultValuesHandlingSpecified)
		{
			defaultValuesHandling = customAttribute.DefaultValuesHandling;
		}
		if ((defaultValuesHandling & DefaultValuesHandling.OmitNull) != DefaultValuesHandling.Preserve && value.Value == null)
		{
			return false;
		}
		if ((defaultValuesHandling & DefaultValuesHandling.OmitEmptyCollections) != DefaultValuesHandling.Preserve && value.Value is IEnumerable enumerable)
		{
			IEnumerator enumerator = enumerable.GetEnumerator();
			bool flag = enumerator.MoveNext();
			if (enumerator is IDisposable disposable)
			{
				disposable.Dispose();
			}
			if (!flag)
			{
				return false;
			}
		}
		if ((defaultValuesHandling & DefaultValuesHandling.OmitDefaults) != DefaultValuesHandling.Preserve)
		{
			object objB = key.GetCustomAttribute<DefaultValueAttribute>()?.Value ?? GetDefault(key.Type);
			if (object.Equals(value.Value, objB))
			{
				return false;
			}
		}
		return base.EnterMapping(key, value, context, serializer);
	}
}
