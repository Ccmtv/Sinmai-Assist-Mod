using System;

namespace SinmaiAssist.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class EnableGameVersionAttribute(uint minGameVersion, uint maxGameVersion = 99999u) : Attribute()
{
	public uint MinGameVersion { get; } = minGameVersion;

	public uint MaxGameVersion { get; } = maxGameVersion;

	public bool ShouldEnable()
	{
		if (SinmaiAssist.GameVersion >= MinGameVersion && SinmaiAssist.GameVersion <= MaxGameVersion)
		{
			return true;
		}
		return false;
	}
}
