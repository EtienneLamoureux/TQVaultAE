using System.Media;
using TQVaultAE.Domain.Entities;

namespace ArzExplorer.Models;

internal class NodeTag
{
	internal RecordId Thread;
	internal string Text;
	internal string TextU => Text.ToUpper();

	internal int TokIdx;
	internal RecordId Key;
	internal string DictionaryKey;
	internal TreeNode thisNode;
	internal TQFileInfo File;
	internal byte[] RawData;
	internal List<string> RecordText = new();
	internal Bitmap Bitmap;
	internal SoundPlayer SoundPlayer;
}