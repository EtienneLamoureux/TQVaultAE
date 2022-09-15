using System.Collections.Generic;

namespace TQVaultAE.Config.Tags;

public class TagMapItem
{
	public string player;
	public bool isTQIT;
	public bool isMod;
	public List<string> tags = new();
}