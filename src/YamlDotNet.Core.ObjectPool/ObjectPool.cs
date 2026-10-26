namespace YamlDotNet.Core.ObjectPool;

internal abstract class ObjectPool<T> where T : class
{
	public abstract T Get();

	public abstract void Return(T obj);
}
internal static class ObjectPool
{
	public static ObjectPool<T> Create<T>(IPooledObjectPolicy<T>? policy = null) where T : class, new()
	{
		return new DefaultObjectPool<T>(policy ?? new DefaultPooledObjectPolicy<T>());
	}

	public static ObjectPool<T> Create<T>(int maximumRetained, IPooledObjectPolicy<T>? policy = null) where T : class, new()
	{
		return new DefaultObjectPool<T>(policy ?? new DefaultPooledObjectPolicy<T>(), maximumRetained);
	}
}
