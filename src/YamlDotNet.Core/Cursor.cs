using System.Diagnostics;

namespace YamlDotNet.Core;

[DebuggerStepThrough]
internal sealed class Cursor
{
	public long Index { get; private set; }

	public long Line { get; private set; }

	public long LineOffset { get; private set; }

	public Cursor()
	{
		Line = 1L;
	}

	public Cursor(Cursor cursor)
	{
		Index = cursor.Index;
		Line = cursor.Line;
		LineOffset = cursor.LineOffset;
	}

	public Mark Mark()
	{
		return new Mark(Index, Line, LineOffset + 1);
	}

	public void Skip()
	{
		Index++;
		LineOffset++;
	}

	public void SkipLineByOffset(int offset)
	{
		Index += offset;
		Line++;
		LineOffset = 0L;
	}

	public void ForceSkipLineAfterNonBreak()
	{
		if (LineOffset != 0L)
		{
			Line++;
			LineOffset = 0L;
		}
	}
}
