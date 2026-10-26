namespace YamlDotNet.Core.ObjectPool;

internal interface IPooledObjectPolicy<T> where T : notnull
{
	T Create();

	bool Return(T obj);
}
