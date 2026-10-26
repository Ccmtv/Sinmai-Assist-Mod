namespace YamlDotNet.Serialization;

internal interface INamingConvention
{
	string Apply(string value);

	string Reverse(string value);
}
