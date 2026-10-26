using System;
using YamlDotNet.Core.ObjectPool;

namespace YamlDotNet.Core;

internal sealed class StringLookAheadBuffer : ILookAheadBuffer, IResettable
{
	public string Value { get; set; } = string.Empty;

	public int Position { get; private set; }

	public int Length => Value.Length;

	public bool EndOfInput => IsOutside(Position);

	public char Peek(int offset)
	{
		int index = Position + offset;
		if (!IsOutside(index))
		{
			return Value[index];
		}
		return '\0';
	}

	private bool IsOutside(int index)
	{
		return index >= Value.Length;
	}

	public void Skip(int length)
	{
		if (length < 0)
		{
			throw new ArgumentOutOfRangeException("length", "The length must be positive.");
		}
		Position += length;
	}

	public bool TryReset()
	{
		Position = 0;
		Value = string.Empty;
		return true;
	}
}
