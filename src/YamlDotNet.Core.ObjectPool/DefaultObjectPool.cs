using System;
using System.Collections.Concurrent;
using System.Threading;

namespace YamlDotNet.Core.ObjectPool;

internal class DefaultObjectPool<T> : ObjectPool<T> where T : class
{
	private readonly Func<T> createFunc;

	private readonly Func<T, bool> returnFunc;

	private readonly int maxCapacity;

	private int numItems;

	private protected readonly ConcurrentQueue<T> items = new ConcurrentQueue<T>();

	private protected T? fastItem;

	public DefaultObjectPool(IPooledObjectPolicy<T> policy)
		: this(policy, Environment.ProcessorCount * 2)
	{
	}

	public DefaultObjectPool(IPooledObjectPolicy<T> policy, int maximumRetained)
	{
		createFunc = policy.Create;
		returnFunc = policy.Return;
		maxCapacity = maximumRetained - 1;
	}

	public override T Get()
	{
		T result = fastItem;
		if (result == null || Interlocked.CompareExchange(ref fastItem, null, result) != result)
		{
			if (items.TryDequeue(out result))
			{
				Interlocked.Decrement(ref numItems);
				return result;
			}
			return createFunc();
		}
		return result;
	}

	public override void Return(T obj)
	{
		ReturnCore(obj);
	}

	private protected bool ReturnCore(T obj)
	{
		if (!returnFunc(obj))
		{
			return false;
		}
		if (fastItem != null || Interlocked.CompareExchange(ref fastItem, obj, null) != null)
		{
			if (Interlocked.Increment(ref numItems) <= maxCapacity)
			{
				items.Enqueue(obj);
				return true;
			}
			Interlocked.Decrement(ref numItems);
			return false;
		}
		return true;
	}
}
