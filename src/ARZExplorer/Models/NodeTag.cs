using System.Media;
using TQVaultAE.Domain.Entities;

namespace ArzExplorer.Models;

/// <summary>
/// Tag of a tree node: the file and the record or entry it shows, and what its view read, kept for the next selection.
/// </summary>
internal sealed class NodeTag
{
	internal string Text = string.Empty;
	internal string TextU => this.Text.ToUpperInvariant();

	internal RecordId Key = RecordId.Empty;
	internal required TreeNode Node; // The node that holds the tag.
	internal required TQFileInfo File;
	internal List<string> RecordText = [];
	internal Bitmap? Bitmap;
	internal SoundPlayer? SoundPlayer;
}
