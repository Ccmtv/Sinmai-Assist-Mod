using System;
using System.Diagnostics;
using System.Text;

namespace YamlDotNet.Core.ObjectPool;

[DebuggerStepThrough]
internal static class StringBuilderPool
{
	internal readonly struct BuilderWrapper : IDisposable
	{
		public readonly StringBuilder Builder;

		private readonly ObjectPool<StringBuilder> pool;

		public BuilderWrapper(StringBuilder builder, ObjectPool<StringBuilder> pool)
		{
			Builder = builder;
			this.pool = pool;
		}

		public override string ToString()
		{
			return Builder.ToString();
		}

		public void Dispose()
		{
			pool.Return(Builder);
		}
	}

	private static readonly ObjectPool<StringBuilder> Pool = ObjectPool.Create(new StringBuilderPooledObjectPolicy
	{
		InitialCapacity = 16,
		MaximumRetainedCapacity = 1024
	});

	public static BuilderWrapper Rent()
	{
		StringBuilder builder = Pool.Get();
		return new BuilderWrapper(builder, Pool);
	}
}
