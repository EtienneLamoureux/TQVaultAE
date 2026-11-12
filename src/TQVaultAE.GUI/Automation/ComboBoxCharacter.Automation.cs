//-----------------------------------------------------------------------
// <copyright file="ComboBoxCharacter.Automation.cs" company="None">
//     Copyright (c) Brandon Wallace and Jesse Calhoun. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
using TQVaultAE.Application;
using TQVaultAE.GUI.Components.Panels;

namespace TQVaultAE.GUI.Components.Character;

/// <summary>
/// Debug-only access to the archive toggle of the character menu for the automation endpoint.
/// </summary>
public partial class ComboBoxCharacter
{
	/// <summary>
	/// Ticks or unticks "Archived" in the character menu like a click, for the selected character only, which moves
	/// its save folder to or from the archive folder. Only test characters (<see cref="Automation.AutomationTestNames"/>).
	/// </summary>
	/// <returns>The character, whether it is archived now, and its save folder.</returns>
	internal (string Name, bool IsArchived, string? PlayerFile) AutomationSetArchived(bool archived)
	{
		if (this.SelectedItem is not PlayerSave ps)
			throw new InvalidOperationException("No character is selected.");
		if (!Automation.AutomationTestNames.IsTestFile(ps.Name))
			throw new InvalidOperationException($"'{ps.Name}' is not a test character: only names starting with {Automation.AutomationTestNames.Accepted} may be archived.");

		this.archiveToolStripMenuItem.Checked = archived;
		return (ps.Name, ps.IsArchived, ps.Folder);
	}

	/// <summary>
	/// Clicks "Archive all" (or "Unarchive all") in the menu of a tag, which moves the save folders of the characters
	/// having that tag. Only test tags (names starting with <c>claude-</c>) whose characters are all test characters.
	/// </summary>
	/// <returns>The characters having the tag, and whether each one is archived now.</returns>
	internal IReadOnlyList<(string Name, bool IsArchived, string Folder)> AutomationSetArchivedByTag(string tag, bool archived)
	{
		if (!tag.StartsWith("claude-", StringComparison.OrdinalIgnoreCase))
			throw new InvalidOperationException($"'{tag}' is not a test tag: only tags starting with 'claude-' may be used.");

		var tagged = this.Items.OfType<PlayerSave>().Where(ps => ps.Tags.ContainsKey(tag)).ToList();
		var real = tagged.Where(ps => !Automation.AutomationTestNames.IsTestFile(ps.Name)).Select(ps => ps.Name).ToList();
		if (real.Count != 0)
			throw new InvalidOperationException($"The tag '{tag}' is on characters that are not test characters: {string.Join(", ", real)}.");

		this.RefreshMenuTagList();
		var tagItem = this.tagsToolStripMenuItem.DropDownItems.OfType<ToolStripMenuItem>()
			.FirstOrDefault(i => i.Text == tag && (i.Name?.StartsWith(TAGKEY, StringComparison.Ordinal) ?? false))
			?? throw new InvalidOperationException($"No tag '{tag}' in the menu.");
		var entryPrefix = archived ? "tagArchiveAllItem" : "tagUnArchiveAllItem";
		var entry = tagItem.DropDownItems.OfType<ToolStripMenuItem>().First(i => i.Name?.StartsWith(entryPrefix, StringComparison.Ordinal) ?? false);
		entry.PerformClick();

		return tagged.Select(ps => (ps.Name, ps.IsArchived, ps.Folder)).ToList();
	}

	/// <summary>
	/// Opens the character list like a click on its button, scrolled to a character.
	/// </summary>
	/// <param name="name">The character to scroll to; empty for the selected one.</param>
	/// <returns>The open list.</returns>
	internal Control AutomationOpenDropDown(string? name)
	{
		if (this.DropDownLoc is null)
			AdjustLocation();
		if (!this.DropDown.Visible)
			DropDownOpen();

		if (!string.IsNullOrEmpty(name))
		{
			var item = this.DropDown.Items.FirstOrDefault(i => i.Item is PlayerSave ps && string.Equals(ps.Name, name, StringComparison.OrdinalIgnoreCase))
				?? throw new InvalidOperationException($"No character '{name}' in the list.");
			this.DropDown.ScrollControlIntoView(item);
		}

		return this.DropDown;
	}

	/// <summary>
	/// Draws the menu of the tools button next to the character list as it opens for the selected character (Archived
	/// tick, tags and their ticks), with the sub menus along <paramref name="open"/>; nothing is clicked.
	/// </summary>
	internal Bitmap AutomationRenderMenu(IReadOnlyList<string> open)
	{
		this.RefreshMenuTagList();
		this.RefreshContent();
		return AutomationMenus.Render(this.contextMenuStrip, open);
	}
}
