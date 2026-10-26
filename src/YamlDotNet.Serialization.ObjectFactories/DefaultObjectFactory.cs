using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using YamlDotNet.Serialization.Callbacks;

namespace YamlDotNet.Serialization.ObjectFactories;

internal sealed class DefaultObjectFactory : ObjectFactoryBase
{
	private readonly Dictionary<Type, ConcurrentDictionary<Type, MethodInfo[]>> _stateMethods = new Dictionary<Type, ConcurrentDictionary<Type, MethodInfo[]>>
	{
		{
			typeof(OnDeserializedAttribute),
			new ConcurrentDictionary<Type, MethodInfo[]>()
		},
		{
			typeof(OnDeserializingAttribute),
			new ConcurrentDictionary<Type, MethodInfo[]>()
		},
		{
			typeof(OnSerializedAttribute),
			new ConcurrentDictionary<Type, MethodInfo[]>()
		},
		{
			typeof(OnSerializingAttribute),
			new ConcurrentDictionary<Type, MethodInfo[]>()
		}
	};

	private readonly Dictionary<Type, Type> DefaultGenericInterfaceImplementations = new Dictionary<Type, Type>
	{
		{
			typeof(IEnumerable<>),
			typeof(List<>)
		},
		{
			typeof(ICollection<>),
			typeof(List<>)
		},
		{
			typeof(IList<>),
			typeof(List<>)
		},
		{
			typeof(IDictionary<, >),
			typeof(Dictionary<, >)
		}
	};

	private readonly Dictionary<Type, Type> DefaultNonGenericInterfaceImplementations = new Dictionary<Type, Type>
	{
		{
			typeof(IEnumerable),
			typeof(List<object>)
		},
		{
			typeof(ICollection),
			typeof(List<object>)
		},
		{
			typeof(IList),
			typeof(List<object>)
		},
		{
			typeof(IDictionary),
			typeof(Dictionary<object, object>)
		}
	};

	private readonly Settings settings;

	public DefaultObjectFactory()
		: this(new Dictionary<Type, Type>(), new Settings())
	{
	}

	public DefaultObjectFactory(IDictionary<Type, Type> mappings)
		: this(mappings, new Settings())
	{
	}

	public DefaultObjectFactory(IDictionary<Type, Type> mappings, Settings settings)
	{
		foreach (KeyValuePair<Type, Type> mapping in mappings)
		{
			if (!mapping.Key.IsAssignableFrom(mapping.Value))
			{
				throw new InvalidOperationException($"Type '{mapping.Value}' does not implement type '{mapping.Key}'.");
			}
			DefaultNonGenericInterfaceImplementations.Add(mapping.Key, mapping.Value);
		}
		this.settings = settings;
	}

	public override object Create(Type type)
	{
		if (type.IsInterface())
		{
			Type value2;
			if (type.IsGenericType())
			{
				if (DefaultGenericInterfaceImplementations.TryGetValue(type.GetGenericTypeDefinition(), out Type value))
				{
					type = value.MakeGenericType(type.GetGenericArguments());
				}
			}
			else if (DefaultNonGenericInterfaceImplementations.TryGetValue(type, out value2))
			{
				type = value2;
			}
		}
		try
		{
			return Activator.CreateInstance(type, settings.AllowPrivateConstructors);
		}
		catch (Exception innerException)
		{
			string message = "Failed to create an instance of type '" + type.FullName + "'.";
			throw new InvalidOperationException(message, innerException);
		}
	}

	public override void ExecuteOnDeserialized(object value)
	{
		ExecuteState(typeof(OnDeserializedAttribute), value);
	}

	public override void ExecuteOnDeserializing(object value)
	{
		ExecuteState(typeof(OnDeserializingAttribute), value);
	}

	public override void ExecuteOnSerialized(object value)
	{
		ExecuteState(typeof(OnSerializedAttribute), value);
	}

	public override void ExecuteOnSerializing(object value)
	{
		ExecuteState(typeof(OnSerializingAttribute), value);
	}

	private void ExecuteState(Type attributeType, object value)
	{
		if (value != null)
		{
			Type type = value.GetType();
			MethodInfo[] stateMethods = GetStateMethods(attributeType, type);
			MethodInfo[] array = stateMethods;
			foreach (MethodInfo methodInfo in array)
			{
				methodInfo.Invoke(value, null);
			}
		}
	}

	private MethodInfo[] GetStateMethods(Type attributeType, Type valueType)
	{
		ConcurrentDictionary<Type, MethodInfo[]> concurrentDictionary = _stateMethods[attributeType];
		return concurrentDictionary.GetOrAdd(valueType, delegate(Type type)
		{
			MethodInfo[] methods = type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			return methods.Where((MethodInfo x) => x.GetCustomAttributes(attributeType, inherit: true).Any()).ToArray();
		});
	}
}
