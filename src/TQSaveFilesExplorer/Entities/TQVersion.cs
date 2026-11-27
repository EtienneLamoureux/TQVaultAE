using System;

namespace TQ.SaveFilesExplorer.Entities;

[Flags]
public enum TQVersion
{
	TQ = 1 << 0,
	TQIT = 1 << 1,
	TQAE = 1 << 2,
	TQ_All = TQ | TQIT | TQAE,
}
