using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TQVaultAE.DAL
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
