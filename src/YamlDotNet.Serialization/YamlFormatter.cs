using System;
using System.Globalization;

namespace YamlDotNet.Serialization;

internal class YamlFormatter
{
	public static YamlFormatter Default { get; } = new YamlFormatter();

	public NumberFormatInfo NumberFormat { get; set; } = new NumberFormatInfo
	{
		CurrencyDecimalSeparator = ".",
		CurrencyGroupSeparator = "_",
		CurrencyGroupSizes = new int[1] { 3 },
		CurrencySymbol = string.Empty,
		CurrencyDecimalDigits = 99,
		NumberDecimalSeparator = ".",
		NumberGroupSeparator = "_",
		NumberGroupSizes = new int[1] { 3 },
		NumberDecimalDigits = 99,
		NaNSymbol = ".nan",
		PositiveInfinitySymbol = ".inf",
		NegativeInfinitySymbol = "-.inf"
	};

	public virtual Func<object, ITypeInspector, INamingConvention, string> FormatEnum { get; set; } = delegate(object value, ITypeInspector typeInspector, INamingConvention enumNamingConvention)
	{
		string empty = string.Empty;
		empty = ((value != null) ? typeInspector.GetEnumValue(value) : string.Empty);
		return enumNamingConvention.Apply(empty);
	};

	public virtual Func<object, bool> PotentiallyQuoteEnums { get; set; } = (object _) => true;

	public string FormatNumber(object number)
	{
		return Convert.ToString(number, NumberFormat);
	}

	public string FormatNumber(double number)
	{
		return number.ToString("G", NumberFormat);
	}

	public string FormatNumber(float number)
	{
		return number.ToString("G", NumberFormat);
	}

	public string FormatBoolean(object boolean)
	{
		if (!boolean.Equals(true))
		{
			return "false";
		}
		return "true";
	}

	public string FormatDateTime(object dateTime)
	{
		return ((DateTime)dateTime).ToString("o", CultureInfo.InvariantCulture);
	}

	public string FormatTimeSpan(object timeSpan)
	{
		return ((TimeSpan)timeSpan/*cast due to .constrained prefix*/).ToString();
	}
}
