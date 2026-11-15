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

		// Merge item properties into Base item
		var forged = selection.Base!;
		ForgeMerge.Apply(forged, selection, godMode, godPart);
		this.ItemProvider.GetDBData(forged);

		sacks[ForgeSlot.Base]!.IsModified = forged.IsModified = true;

		IReadOnlyList<Item> consumed = hardcore ? this.ConsumeMaterials(selection, sacks) : [];

		// The search keeps the item's display data: replace its entry so it shows the forged item.
		this.ItemText.InvalidateFriendlyNamesCache(forged);
		this.ItemDatabaseService.RemoveItemFromDatabase(forged);
		this.ItemDatabaseService.TryAddItemToDatabase(forged);

		return ForgeResult.Succeeded(forged, consumed);
	}

	/// <summary>Hardcore mode: the material items are removed from their sacks and from the search.</summary>
	private List<Item> ConsumeMaterials(ForgeSelection selection, IReadOnlyDictionary<ForgeSlot, SackCollection?> sacks)
	{
		var consumed = new List<Item>();

		foreach (var slot in MaterialSlots)
		{
			if (selection[slot] is not { } item || sacks.GetValueOrDefault(slot) is not { } sack)
				continue;

			sack.RemoveItem(item);
			this.ItemText.InvalidateFriendlyNamesCache(item);
			this.ItemDatabaseService.RemoveItemFromDatabase(item);
			consumed.Add(item);
		}

		return consumed;
	}
}
