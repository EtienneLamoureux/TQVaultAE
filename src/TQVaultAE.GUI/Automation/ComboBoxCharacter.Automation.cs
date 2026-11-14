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
	/// <summary>The "Archived" entry of the tools menu.</summary>
	internal ToolStripMenuItem AutomationArchiveEntry => this.archiveToolStripMenuItem;

	/// <summary>The "Read-only" entry of the tools menu.</summary>
	internal ToolStripMenuItem AutomationReadOnlyEntry => this.readOnlyToolStripMenuItem;

	/// <summary>The "Duplicate Character" entry of the tools menu.</summary>
	internal ToolStripMenuItem AutomationDuplicateEntry => this.duplicateToolStripMenuItem;

	/// <summary>The "Mod link..." entry of the tools menu.</summary>
	internal ToolStripMenuItem AutomationModLinkEntry => this.modLinkToolStripMenuItem;

	/// <summary>The entries down to the Delete entry of a tag in the tools menu (Tags, the tag, Delete), as last built.</summary>
	internal IReadOnlyList<ToolStripItem> AutomationTagDeletePath(string tag)
	{
		var tagItem = this.tagsToolStripMenuItem.DropDownItems.OfType<ToolStripMenuItem>()
			.FirstOrDefault(i => i.Text == tag && (i.Name?.StartsWith(TAGKEY, StringComparison.Ordinal) ?? false))
			?? throw new InvalidOperationException($"No tag '{tag}' in the menu.");
		var delete = tagItem.DropDownItems.OfType<ToolStripMenuItem>().First(i => i.Name?.StartsWith("tagDeleteItem", StringComparison.Ordinal) ?? false);
		return [this.tagsToolStripMenuItem, tagItem, delete];
	}

	/// <summary>The tick of "Read-only" in the character menu, as it opens for the selected character.</summary>
	internal bool AutomationReadOnlyChecked
	{
		get
		{
			this.RefreshContent();
			return this.readOnlyToolStripMenuItem.Checked;
		}
	}

	/// <summary>
	/// Opens the character list and reads it as drawn: the text of the selected character's box, and each entry's
	/// texts (<see cref="ComboBoxCharacterItem.AutomationTexts"/>).
	/// </summary>
	internal (string Header, IReadOnlyList<object> Entries) AutomationEntries()
	{
		this.AutomationOpenDropDown(null);
		return (this.scalingLabelCharName.Text, [.. this.DropDown.Items.Select(item => item.AutomationTexts)]);
	}

	/// <summary>
	/// Opens the character list scrolled to <paramref name="name"/> and clicks the middle of its entry with mouse messages
	/// sent to the window under that point (a label of the entry), whose Click selects the character and closes the list.
	/// </summary>
	/// <returns>The entry's character.</returns>
	internal PlayerSave AutomationPick(string name)
	{
		this.AutomationOpenDropDown(name);
		var entry = this.DropDown.Items.First(i => i.Item is PlayerSave ps && string.Equals(ps.Name, name, StringComparison.OrdinalIgnoreCase));
		var (hit, point) = Automation.AutomationInput.ChildAt(entry, Automation.AutomationInput.Center(entry));
		Automation.AutomationInput.SendClick(hit, point, MouseButtons.Left, Keys.None);
		return (PlayerSave)entry.Item!;
	}

	/// <summary>Whether the character list is open.</summary>
	internal bool AutomationDropDownOpen => this.DropDown.Visible;

	/// <summary>Whether the drop-down entry of a character shows its "Read-only" badge.</summary>
	internal bool AutomationShowsReadOnlyBadge(PlayerSave save)
		=> this.DropDown.Items.FirstOrDefault(i => i.Item == save)?.AutomationShowsReadOnlyBadge ?? false;

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
	/// Clicks the tools button next to the character list with mouse messages, which opens its menu as for the user.
	/// </summary>
	/// <returns>The menu, open.</returns>
	internal ContextMenuStrip AutomationOpenToolsMenu()
	{
		Automation.AutomationInput.SendClick(this.scalingButtonTools, Automation.AutomationInput.Center(this.scalingButtonTools), MouseButtons.Left, Keys.None);
		return this.contextMenuStrip;
	}

	/// <summary>
	/// Why automation may not click the entry at the end of <paramref name="chain"/> of the tools menu, or null: the
	/// entries that change files only for test characters and test tags (<c>claude-</c>), Duplicate Character only with a
	/// queued test name, and never the global Archive all / Unarchive all nor a tag's, which move real characters
	/// (<c>tag_archive_all</c> checks the tag's characters first).
	/// </summary>
	internal string? AutomationRefuseMenuPath(IReadOnlyList<ToolStripItem> chain)
	{
		var entry = chain[^1];
		string name = entry.Name ?? string.Empty;
		if (entry == this.archiveAllToolStripMenuItemGlobal || entry == this.unarchiveAllToolStripMenuItemGlobal
			|| name.StartsWith("tagArchiveAllItem", StringComparison.Ordinal) || name.StartsWith("tagUnArchiveAllItem", StringComparison.Ordinal))
			return "Archive all / Unarchive all move real characters: not driven (tag_archive_all checks a test tag's characters first).";

		if (entry == this.duplicateToolStripMenuItem)
		{
			var pending = Automation.AutomationHost.PendingDialogAnswers;
			return pending.Count > 0 && pending[0].Text is { } newName && Automation.AutomationTestNames.IsTestFile(newName)
				? null
				: $"Queue the copy's name first, a test name: queue_dialog_answers('text:claude-...') (accepted: {Automation.AutomationTestNames.Accepted}).";
		}

		// A tag's entries (its tick, Color, Delete): test tags only.
		if (chain.Count >= 2 && chain[0] == this.tagsToolStripMenuItem && chain[1].Name?.StartsWith(TAGKEY, StringComparison.Ordinal) == true
			&& !(chain[1].Text ?? string.Empty).StartsWith("claude-", StringComparison.OrdinalIgnoreCase))
			return $"'{chain[1].Text}' is not a test tag: only tags starting with 'claude-'.";

		// Its tick changes the selected character; the other top-level entries too.
		bool tagEntry = chain.Count > 2 && chain[0] == this.tagsToolStripMenuItem;
		if (!tagEntry && !(this.SelectedItem is PlayerSave ps && Automation.AutomationTestNames.IsTestFile(ps.Name)))
			return $"Select a test character first (names starting with {Automation.AutomationTestNames.Accepted}).";

		return null;
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
