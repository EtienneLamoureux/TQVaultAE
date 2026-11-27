using TQVaultAE.Application.Contracts.Providers;
using TQVaultAE.Application.Contracts.Services;
using TQVaultAE.Application.Results;
using TQVaultAE.Domain.Entities;
using TQVaultAE.Domain.Forge;

namespace TQVaultAE.Services;

/// <summary>
/// Forge: the rules with the item level requirements, the parts offered in God mode, and the forge itself.
/// </summary>
public class ForgeService(
	IItemProvider itemProvider
	, IItemTextFormatter itemText
	, IItemDatabaseService itemDatabaseService
) : IForgeService
{
	private static readonly ForgeSlot[] MaterialSlots = [ForgeSlot.Prefix, ForgeSlot.Suffix, ForgeSlot.Relic1, ForgeSlot.Relic2];

	private readonly IItemProvider ItemProvider = itemProvider;
	private readonly IItemTextFormatter ItemText = itemText;
	private readonly IItemDatabaseService ItemDatabaseService = itemDatabaseService;

	/// <inheritdoc/>
	public ForgeRules CreateRules()
		=> new(this.LevelRequirement);

	private int LevelRequirement(Item item)
		=> this.ItemProvider.GetRequirementVariables(item).TryGetValue(Variable.KEY_LEVELREQ, out var level) ? level.GetInt32(0) : 0;

	/// <inheritdoc/>
	public Item? Preview(ForgeSelection selection, bool godMode, Func<ForgeSlot, ForgePart?> godPart)
	{
		if (selection.Base is null)
			return null;

		var preview = selection.Base.Clone();
		if (selection.Prefix is not null || selection.Suffix is not null || selection.Relic1 is not null || selection.Relic2 is not null)
		{
			ForgeMerge.Apply(preview, selection, godMode, godPart);
			this.ItemProvider.GetDBData(preview);
		}

		return preview;
	}

	/// <inheritdoc/>
	public IReadOnlyList<ForgePart> GodModeParts(Item item)
	{
		var parts = new List<ForgePart> { ForgePart.Base };

		if (item.HasPrefix)
			parts.Add(ForgePart.Prefix);

		if (item.HasSuffix)
			parts.Add(ForgePart.Suffix);

		if (item.HasRelicOrCharmSlot1)
			parts.Add(ForgePart.Relic1);

		if (item.HasRelicOrCharmSlot2)
			parts.Add(ForgePart.Relic2);

		return parts;
	}

	/// <inheritdoc/>
	public ForgeResult Forge(
		ForgeRules rules
		, ForgeSelection selection
		, IReadOnlyDictionary<ForgeSlot, SackCollection?> sacks
		, bool godMode
		, Func<ForgeSlot, ForgePart?> godPart
		, bool hardcore)
	{
		if (rules.CheckForge(selection) is { } problem)
			return ForgeResult.Refused(problem);

		var forged = this.Merge(selection, sacks[ForgeSlot.Base]!, godMode, godPart);
		IReadOnlyList<Item> consumed = hardcore ? this.ConsumeMaterials(selection, sacks) : [];
		this.ReplaceSearchEntry(forged);

		return ForgeResult.Succeeded(forged, consumed);
	}

	/// <inheritdoc/>
	public ForgeResult Socket(ForgeRules rules, ForgeMode mode, ForgeSlot slot, Item relic, SackCollection relicSack, Item target, SackCollection targetSack)
	{
		if (rules.CheckSocket(mode, slot, relic, target) is { } problem)
			return ForgeResult.Refused(problem);

		// The relic or charm itself goes in whole (its shards and bonus): the same merge as the forge's relic slots.
		var selection = new ForgeSelection { [ForgeSlot.Base] = target, [slot] = relic };
		var socketed = this.Merge(selection, targetSack, godMode: false, _ => null);
		this.Consume(relic, relicSack);
		this.ReplaceSearchEntry(socketed);

		return ForgeResult.Succeeded(socketed, [relic]);
	}

	/// <summary>
	/// Merges the material items of <paramref name="selection"/> into its base item, reads the item's record data again
	/// and marks it and <paramref name="baseSack"/> modified.
	/// </summary>
	/// <returns>The base item, changed.</returns>
	private Item Merge(ForgeSelection selection, SackCollection baseSack, bool godMode, Func<ForgeSlot, ForgePart?> godPart)
	{
		var forged = selection.Base!;
		ForgeMerge.Apply(forged, selection, godMode, godPart);
		this.ItemProvider.GetDBData(forged);

		baseSack.IsModified = forged.IsModified = true;
		return forged;
	}

	/// <summary>The search keeps the item's display data: replace its entry so it shows the changed item.</summary>
	private void ReplaceSearchEntry(Item changed)
	{
		this.ItemText.InvalidateFriendlyNamesCache(changed);
		this.ItemDatabaseService.RemoveItemFromDatabase(changed);
		this.ItemDatabaseService.TryAddItemToDatabase(changed);
	}

	/// <summary>Hardcore mode: the material items are removed from their sacks and from the search.</summary>
	private List<Item> ConsumeMaterials(ForgeSelection selection, IReadOnlyDictionary<ForgeSlot, SackCollection?> sacks)
	{
		var consumed = new List<Item>();

		foreach (var slot in MaterialSlots)
		{
			if (selection[slot] is not { } item || sacks.GetValueOrDefault(slot) is not { } sack)
				continue;

			this.Consume(item, sack);
			consumed.Add(item);
		}

		return consumed;
	}

	/// <summary>Removes a material item from its sack and from the search.</summary>
	private void Consume(Item item, SackCollection sack)
	{
		sack.RemoveItem(item);
		this.ItemText.InvalidateFriendlyNamesCache(item);
		this.ItemDatabaseService.RemoveItemFromDatabase(item);
	}
}
