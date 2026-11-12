using TQVaultAE.Application;
using TQVaultAE.Application.Contracts.Providers;
using TQVaultAE.Application.Contracts.Services;
using TQVaultAE.Application.DTOs;
using TQVaultAE.Domain.Entities;
using TQVaultAE.Domain.Results;

namespace TQVaultAE.Services;

/// <summary>
/// Applies the character edit dialog to a character: level, experience, difficulty, attributes and masteries.
/// </summary>
public class CharacterEditService(IPathIO pathIO, IPlayerCollectionProvider playerCollectionProvider, IDatabase database, ISkillTreeProvider skillTreeProvider, IItemProvider itemProvider) : ICharacterEditService
{
	private readonly IPathIO _pathIO = pathIO;
	private readonly IPlayerCollectionProvider _playerCollectionProvider = playerCollectionProvider;
	private readonly IDatabase _database = database;
	private readonly ISkillTreeProvider _skillTreeProvider = skillTreeProvider;
	private readonly IItemProvider _itemProvider = itemProvider;

	/// <inheritdoc/>
	public IReadOnlyList<SkillAugment> GetGearSkillAugments(PlayerCollection player)
		=> player.EquipmentSack is not { } equipment ? [] : this.GetGearSkillAugments(equipment, player.UseAlternate);

	/// <inheritdoc/>
	public IReadOnlyList<SkillAugment> GetGearSkillAugments(IEnumerable<Item> equipment, bool useAlternate)
		=> [.. equipment
			.Where(item => !item.IsInInactiveWeaponSlot(useAlternate))
			.SelectMany(this._itemProvider.GetSkillAugments)];

	/// <inheritdoc/>
	public CharacterSkillEditor CreateSkillEditor(PlayerInfo info)
		=> new(info.SkillRecordList, info.MasteriesAllowed, info.SkillPoints, this._skillTreeProvider.GetMasteryTree);

	/// <inheritdoc/>
	public GrantedSkill? DescribeSkill(RecordId skillId, int level, SkillWindowContext? context = null)
		=> this._itemProvider.GetSkillAtLevel(skillId, level, context);

	/// <inheritdoc/>
	public string GetMasteryDescriptionTag(RecordId masteryId)
		=> this._database.GetInfo(masteryId)?.DescriptionTag ?? string.Empty;

	/// <inheritdoc/>
	public PlayerInfo BuildPlayerInfo(PlayerInfo source, CharacterEditRequest request)
	{
		var playerInfo = new PlayerInfo(this._pathIO)
		{
			CurrentLevel = request.Level,
			MasteriesAllowed = PlayerLevel.GetMasteriesAllowed(request.Level),
			CurrentXP = int.TryParse(request.XpText, out var xp) ? xp : source.CurrentXP,
			Money = source.Money > 0 ? source.Money : 0,
			DifficultyUnlocked = request.DifficultyUnlocked ?? source.DifficultyUnlocked,
			SkillPoints = request.SkillPoints,
			AttributesPoints = request.AttributePoints,
			BaseStrength = request.Strength,
			BaseDexterity = request.Dexterity,
			BaseIntelligence = request.Intelligence,
			BaseHealth = request.Health,
			BaseMana = request.Mana,
			Class = source.Class,
		};
		if (request.SkillRecords is not null && !request.ResetMasteries)
		{
			playerInfo.SkillRecordList.AddRange(request.SkillRecords);
			playerInfo.SkillsChanged = true;
			playerInfo.MasteriesChanged = request.MasteriesChanged;
			if (request.MasteriesChanged)
				playerInfo.Class = string.Empty;
		}
		else
			playerInfo.SkillRecordList.AddRange(source.SkillRecordList);

		if (request.ResetAttributes)
		{
			playerInfo.BaseStrength = PlayerLevel.MinStrength;
			playerInfo.BaseDexterity = PlayerLevel.MinDexterity;
			playerInfo.BaseIntelligence = PlayerLevel.MinIntelligence;
			playerInfo.BaseHealth = PlayerLevel.MinHealth;
			playerInfo.BaseMana = PlayerLevel.MinMana;

			// Return invested points into attributes point pool
			playerInfo.AttributesPoints = source.AttributesPoints + PlayerLevel.GetInvestedAttributePoints(
				source.BaseStrength, source.BaseDexterity, source.BaseIntelligence, source.BaseHealth, source.BaseMana);
		}

		playerInfo.MasteriesResetRequiered = request.ResetMasteries;

		if (playerInfo.MustResetMasteries)
		{
			playerInfo.ResetMasteries();

			// Adjust "skillPoints"
			playerInfo.SkillPoints += playerInfo.ReleasedSkillPoints;
			playerInfo.Class = string.Empty;
		}

		playerInfo.Money = PlayerLevel.GetMoneyAfterDifficultyChange(playerInfo.Money, source.DifficultyUnlocked, playerInfo.DifficultyUnlocked);

		return playerInfo;
	}

	/// <inheritdoc/>
	public bool Apply(PlayerCollection player, CharacterEditRequest request)
	{
		// A read-only character never changes.
		if (player.PlayerInfo is null || player.IsReadOnly)
			return false;

		var playerInfo = this.BuildPlayerInfo(player.PlayerInfo, request);
		this._playerCollectionProvider.CommitPlayerInfo(player, playerInfo);
		return true;
	}
}
