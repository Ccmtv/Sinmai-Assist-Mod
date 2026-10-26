using System.Text;

namespace YamlDotNet.Core.ObjectPool;

internal class StringBuilderPooledObjectPolicy : IPooledObjectPolicy<StringBuilder>
{
	public int InitialCapacity { get; set; } = 100;

	public int MaximumRetainedCapacity { get; set; } = 4096;

	public StringBuilder Create()
	{
		return new StringBuilder(InitialCapacity);
	}

	public bool Return(StringBuilder obj)
	{
		if (obj.Capacity > MaximumRetainedCapacity)
		{
			return false;
		}
		obj.Clear();
		return true;
	}
}
