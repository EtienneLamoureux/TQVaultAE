using System;
using TQVaultAE.Domain.Entities;

namespace TQVaultAE.Domain.Results
{
	public class LoadPlayerResult
	{
		public bool IsCustom;
		public string PlayerFile;
		public PlayerCollection Player;
		public Stash Stash;
		public string StashFile;
	}
}
