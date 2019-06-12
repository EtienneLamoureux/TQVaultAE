using System;

namespace TQVaultAE.Entities
{
	public class PlayerInfoKeyPair
	{
		public byte KeyNameLength;
		public byte[] KeyId;
		public long KeyOffset;
		public long ValueOffset;
		public int Value4byte;
		public Type Type;
	}
}
