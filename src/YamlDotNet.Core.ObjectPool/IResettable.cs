namespace YamlDotNet.Core.ObjectPool;

internal interface IResettable
{
	bool TryReset();
}
