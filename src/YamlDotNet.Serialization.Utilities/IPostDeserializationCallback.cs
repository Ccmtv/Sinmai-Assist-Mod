namespace YamlDotNet.Serialization.Utilities;

internal interface IPostDeserializationCallback
{
	void OnDeserialization();
}
