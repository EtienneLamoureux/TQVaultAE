using System.Drawing;
using TQVaultAE.Application.DTOs;
using TQVaultAE.Application.Session;

namespace TQVaultAE.Application.Contracts.Services;

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

	/// <summary>
	/// Checks a name for a new tag, or for a rename when <paramref name="currentName"/> is given. Names compare ignoring case.
	/// </summary>
	/// <param name="newName">The name typed by the user.</param>
	/// <param name="currentName">The name of the tag being renamed, or null for a new tag.</param>
	/// <returns>Why the name is refused, or <see cref="TagNameError.None"/>.</returns>
	TagNameError ValidateTagName(string? newName, string? currentName = null);

	void ReadConfig();
	void SaveConfig();

	void LoadTags(PlayerSave ps);
	bool AssignTag(PlayerSave ps, string tagName);
	bool UnassignTag(PlayerSave ps, string tagName);
}