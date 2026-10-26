using System;
using System.Globalization;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;

namespace YamlDotNet.Serialization.Converters;

internal class DateTime8601Converter : IYamlTypeConverter
{
	private readonly IFormatProvider provider;

	private readonly ScalarStyle scalarStyle;

	public DateTime8601Converter()
		: this(ScalarStyle.Any)
	{
	}

	public DateTime8601Converter(ScalarStyle scalarStyle)
	{
		provider = CultureInfo.InvariantCulture;
		this.scalarStyle = scalarStyle;
	}

	public bool Accepts(Type type)
	{
		return type == typeof(DateTime);
	}

	public object ReadYaml(IParser parser, Type type, ObjectDeserializer rootDeserializer)
	{
		string value = parser.Consume<Scalar>().Value;
		DateTime dateTime = DateTime.ParseExact(value, "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);
		return dateTime;
	}

	public void WriteYaml(IEmitter emitter, object? value, Type type, ObjectSerializer serializer)
	{
		string value2 = ((DateTime)value).ToString("O", CultureInfo.InvariantCulture);
		emitter.Emit(new Scalar(AnchorName.Empty, TagName.Empty, value2, scalarStyle, isPlainImplicit: true, isQuotedImplicit: false));
	}
}
