using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace YamlDotNet;

internal static class ReflectionExtensions
{
	private static readonly Func<PropertyInfo, bool> IsInstance = (PropertyInfo property) => !(property.GetMethod ?? property.SetMethod).IsStatic;

	private static readonly Func<PropertyInfo, bool> IsInstancePublic = (PropertyInfo property) => IsInstance(property) && (property.GetMethod ?? property.SetMethod).IsPublic;

	private static readonly ConcurrentDictionary<Type, bool> typesHaveNullContext = new ConcurrentDictionary<Type, bool>();

	public static Type? BaseType(this Type type)
	{
		return type.GetTypeInfo().BaseType;
	}

	public static bool IsValueType(this Type type)
	{
		return type.GetTypeInfo().IsValueType;
	}

	public static bool IsGenericType(this Type type)
	{
		return type.GetTypeInfo().IsGenericType;
	}

	public static bool IsGenericTypeDefinition(this Type type)
	{
		return type.GetTypeInfo().IsGenericTypeDefinition;
	}

	public static Type? GetImplementationOfOpenGenericInterface(this Type type, Type openGenericType)
	{
		if (!openGenericType.IsGenericType || !openGenericType.IsInterface)
		{
			throw new ArgumentException("The type must be a generic type definition and an interface", "openGenericType");
		}
		if (IsGenericDefinitionOfType(type, openGenericType))
		{
			return type;
		}
		return type.FindInterfaces((Type t, object context) => IsGenericDefinitionOfType(t, context), openGenericType).FirstOrDefault();
		static bool IsGenericDefinitionOfType(Type t, object? context)
		{
			if (t.IsGenericType)
			{
				return t.GetGenericTypeDefinition() == (Type)context;
			}
			return false;
		}
	}

	public static bool IsInterface(this Type type)
	{
		return type.GetTypeInfo().IsInterface;
	}

	public static bool IsEnum(this Type type)
	{
		return type.GetTypeInfo().IsEnum;
	}

	public static bool IsRequired(this MemberInfo member)
	{
		return member.GetCustomAttributes(inherit: true).Any((object x) => x.GetType().FullName == "System.Runtime.CompilerServices.RequiredMemberAttribute");
	}

	public static bool HasDefaultConstructor(this Type type, bool allowPrivateConstructors)
	{
		BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.Public;
		if (allowPrivateConstructors)
		{
			bindingFlags |= BindingFlags.NonPublic;
		}
		if (!type.IsValueType)
		{
			return type.GetConstructor(bindingFlags, null, Type.EmptyTypes, null) != null;
		}
		return true;
	}

	public static bool IsAssignableFrom(this Type type, Type source)
	{
		return type.IsAssignableFrom(source.GetTypeInfo());
	}

	public static bool IsAssignableFrom(this Type type, TypeInfo source)
	{
		return type.GetTypeInfo().IsAssignableFrom(source);
	}

	public static TypeCode GetTypeCode(this Type type)
	{
		if (type.IsEnum())
		{
			type = Enum.GetUnderlyingType(type);
		}
		if (type == typeof(bool))
		{
			return TypeCode.Boolean;
		}
		if (type == typeof(char))
		{
			return TypeCode.Char;
		}
		if (type == typeof(sbyte))
		{
			return TypeCode.SByte;
		}
		if (type == typeof(byte))
		{
			return TypeCode.Byte;
		}
		if (type == typeof(short))
		{
			return TypeCode.Int16;
		}
		if (type == typeof(ushort))
		{
			return TypeCode.UInt16;
		}
		if (type == typeof(int))
		{
			return TypeCode.Int32;
		}
		if (type == typeof(uint))
		{
			return TypeCode.UInt32;
		}
		if (type == typeof(long))
		{
			return TypeCode.Int64;
		}
		if (type == typeof(ulong))
		{
			return TypeCode.UInt64;
		}
		if (type == typeof(float))
		{
			return TypeCode.Single;
		}
		if (type == typeof(double))
		{
			return TypeCode.Double;
		}
		if (type == typeof(decimal))
		{
			return TypeCode.Decimal;
		}
		if (type == typeof(DateTime))
		{
			return TypeCode.DateTime;
		}
		if (type == typeof(string))
		{
			return TypeCode.String;
		}
		return TypeCode.Object;
	}

	public static bool IsDbNull(this object value)
	{
		return value?.GetType()?.FullName == "System.DBNull";
	}

	public static Type[] GetGenericArguments(this Type type)
	{
		return type.GetTypeInfo().GenericTypeArguments;
	}

	public static PropertyInfo? GetPublicProperty(this Type type, string name)
	{
		return type.GetProperties(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public).FirstOrDefault((PropertyInfo p) => p.Name == name);
	}

	public static FieldInfo? GetPublicStaticField(this Type type, string name)
	{
		return type.GetRuntimeField(name);
	}

	public static IEnumerable<PropertyInfo> GetProperties(this Type type, bool includeNonPublic)
	{
		Func<PropertyInfo, bool> predicate = (includeNonPublic ? IsInstance : IsInstancePublic);
		if (!type.IsInterface())
		{
			return type.GetRuntimeProperties().Where<PropertyInfo>(predicate);
		}
		return new Type[1] { type }.Concat(type.GetInterfaces()).SelectMany((Type i) => i.GetRuntimeProperties().Where<PropertyInfo>(predicate));
	}

	public static IEnumerable<PropertyInfo> GetPublicProperties(this Type type)
	{
		return type.GetProperties(includeNonPublic: false);
	}

	public static IEnumerable<FieldInfo> GetPublicFields(this Type type)
	{
		return from f in type.GetRuntimeFields()
			where !f.IsStatic && f.IsPublic
			select f;
	}

	public static IEnumerable<MethodInfo> GetPublicStaticMethods(this Type type)
	{
		return from m in type.GetRuntimeMethods()
			where m.IsPublic && m.IsStatic
			select m;
	}

	public static MethodInfo GetPrivateStaticMethod(this Type type, string name)
	{
		return type.GetRuntimeMethods().FirstOrDefault((MethodInfo m) => !m.IsPublic && m.IsStatic && m.Name.Equals(name)) ?? throw new MissingMethodException("Expected to find a method named '" + name + "' in '" + type.FullName + "'.");
	}

	public static MethodInfo? GetPublicStaticMethod(this Type type, string name, params Type[] parameterTypes)
	{
		return type.GetRuntimeMethods().FirstOrDefault(delegate(MethodInfo m)
		{
			if (m.IsPublic && m.IsStatic && m.Name.Equals(name))
			{
				ParameterInfo[] parameters = m.GetParameters();
				if (parameters.Length == parameterTypes.Length)
				{
					return parameters.Zip(parameterTypes, (ParameterInfo pi, Type pt) => pi.ParameterType == pt).All((bool r) => r);
				}
				return false;
			}
			return false;
		});
	}

	public static MethodInfo? GetPublicInstanceMethod(this Type type, string name)
	{
		return type.GetRuntimeMethods().FirstOrDefault((MethodInfo m) => m.IsPublic && !m.IsStatic && m.Name.Equals(name));
	}

	public static MethodInfo? GetGetMethod(this PropertyInfo property, bool nonPublic)
	{
		MethodInfo methodInfo = property.GetMethod;
		if (!nonPublic && !methodInfo.IsPublic)
		{
			methodInfo = null;
		}
		return methodInfo;
	}

	public static MethodInfo? GetSetMethod(this PropertyInfo property)
	{
		return property.SetMethod;
	}

	public static IEnumerable<Type> GetInterfaces(this Type type)
	{
		return type.GetTypeInfo().ImplementedInterfaces;
	}

	public static bool IsInstanceOf(this Type type, object o)
	{
		if (!(o.GetType() == type))
		{
			return o.GetType().GetTypeInfo().IsSubclassOf(type);
		}
		return true;
	}

	public static Attribute[] GetAllCustomAttributes<TAttribute>(this PropertyInfo member)
	{
		return Attribute.GetCustomAttributes(member, typeof(TAttribute), inherit: true);
	}

	public static bool AcceptsNull(this MemberInfo member)
	{
		bool result = true;
		if (typesHaveNullContext.GetOrAdd(member.DeclaringType, delegate(Type t)
		{
			object[] customAttributes2 = t.GetCustomAttributes(inherit: true);
			return customAttributes2.Any((object x) => x.GetType().FullName == "System.Runtime.CompilerServices.NullableContextAttribute");
		}))
		{
			object[] customAttributes = member.GetCustomAttributes(inherit: true);
			result = customAttributes.Any((object x) => x.GetType().FullName == "System.Runtime.CompilerServices.NullableAttribute");
		}
		return result;
	}
}
