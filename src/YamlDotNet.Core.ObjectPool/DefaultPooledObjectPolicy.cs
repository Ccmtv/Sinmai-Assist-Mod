namespace YamlDotNet.Core.ObjectPool;

internal class DefaultPooledObjectPolicy<T> : IPooledObjectPolicy<T> where T : class, new()
{
	public T Create()
	{
		return new T();
	}

	public bool Return(T obj)
	{
		if (obj is IResettable resettable)
		{
			return resettable.TryReset();
		}
		return true;
	}
}
