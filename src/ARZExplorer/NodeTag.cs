using System.Windows.Forms;
using TQVaultAE.Domain.Entities;

namespace ArzExplorer;

internal class NodeTag
{
	internal RecordId Thread;
	internal string Text;
	internal string TextU => Text.ToUpper();
	internal int RecIdx;
	internal int TokIdx;
	internal RecordId Key;
	internal TreeNode thisNode;
}
