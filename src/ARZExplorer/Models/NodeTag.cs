using System.Collections.Generic;
using System.Drawing;
using System.Media;
using System.Windows.Forms;
using TQVaultAE.Domain.Entities;

namespace ArzExplorer.Models;

internal class NodeTag
{
	internal RecordId Thread;
	internal string Text;
	internal string TextU => Text.ToUpper();

	internal int TokIdx;
	internal RecordId Key;
	internal TreeNode thisNode;
	internal TQFileInfo File;
	internal byte[] RawData;
	internal List<string> RecordText = new();
	internal Bitmap Bitmap;
	internal SoundPlayer SoundPlayer;
}
