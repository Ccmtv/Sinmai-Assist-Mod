using System;

namespace YamlDotNet.Serialization.TypeResolvers;

internal class StaticTypeResolver : ITypeResolver
{
	public virtual Type Resolve(Type staticType, object? actualValue)
	{
		if (actualValue != null)
		{
			if (actualValue.GetType().IsEnum)
			{
				return staticType;
			}
			switch (actualValue.GetType().GetTypeCode())
			{
			case TypeCode.Boolean:
				return typeof(bool);
			case TypeCode.Char:
				return typeof(char);
			case TypeCode.SByte:
				return typeof(sbyte);
			case TypeCode.Byte:
				return typeof(byte);
			case TypeCode.Int16:
				return typeof(short);
			case TypeCode.UInt16:
				return typeof(ushort);
			case TypeCode.Int32:
				return typeof(int);
			case TypeCode.UInt32:
				return typeof(uint);
			case TypeCode.Int64:
				return typeof(long);
			case TypeCode.UInt64:
				return typeof(ulong);
			case TypeCode.Single:
				return typeof(float);
			case TypeCode.Double:
				return typeof(double);
			case TypeCode.Decimal:
				return typeof(decimal);
			case TypeCode.String:
				return typeof(string);
			case TypeCode.DateTime:
				return typeof(DateTime);
			}
		}
		return staticType;
	}
}
