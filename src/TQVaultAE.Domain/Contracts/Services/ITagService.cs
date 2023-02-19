using System.Collections.Generic;
using System.Drawing;
using TQVaultAE.Domain.Entities;

namespace TQVaultAE.Domain.Contracts.Services
{
	/// <summary>
	/// Define all Tags actions
	/// </summary>
	public interface ITagService
	{
		Dictionary<string, Color> Tags { get; }
		bool AddTag(string tagName);
		bool AddTag(string tagName, byte r, byte g, byte b);
		bool DeleteTag(string tagName);
		bool UpdateTag(string tagNameOld, string tagNameNew, byte r, byte g, byte b);

		void ReadConfig();
		void SaveConfig();

		void LoadTags(PlayerSave ps);
		bool AssignTag(PlayerSave ps, string tagName);
		bool UnassignTag(PlayerSave ps, string tagName);
	}
}
