using TQVaultAE.Application.Character;
using TQVaultAE.Application.Contracts.Services;
using TQVaultAE.Domain.Entities;
using TQVaultAE.Domain.Helpers;

namespace TQVaultAE.Services;

/// <summary>The auras the character's pets cast: the Pets section of the Buffs tab.</summary>
public partial class CharacterStatsService
{
	/// <summary>A skill that adds a skill to the pets of the summon it modifies (Triumph, Forged by Fire).</summary>
	private const string PetModifierClass = "SkillSecondary_PetModifier";

	/// <summary>
	/// The auras the pets cast on those around them, the character included. A summon (learned or granted by an item)
	/// spawns the pet of its level (<c>spawnObjects</c>), which casts the skills of its record at their levels
	/// (<c>skillName1</c>/<c>skillLevel1</c>...); a pet modifier the character learns gives that pet one more skill
	/// (<c>petSkillName</c>) at the modifier's level. Only the radius buffs reach the character: the buffs a pet keeps
	/// for itself, its attacks and the debuffs are left out. One source per aura, at the highest level found; off.
	/// </summary>
	private List<BuffSource> PetAuraSources(BuffSourceRequest request)
	{
		var auras = new Dictionary<RecordId, BuffSource>();

		foreach (var skill in request.Skills.Where(s => s.skillLevel > 0 && !string.IsNullOrEmpty(s.skillName)))
		{
			var id = skill.skillName.ToRecordId();
			var record = this._database.GetRecordFromFile(id);
			if (record is null)
				continue;

			var tree = request.Trees.FirstOrDefault(t => t.Contains(id));
			var inTree = tree?.Skills.FirstOrDefault(s => s.Id == id);
			int level = SkillStatRules.EffectiveLevel(skill.skillLevel, id, tree, request.Augments);
			var ownerTag = inTree?.NameTag is { Length: > 0 } tag ? tag : this.NameTag(id, record);
			var ownerIcon = inTree?.Icon is { Length: > 0 } icon ? icon : this.Icon(record);

			if (record.RecordType.Equals(PetModifierClass, StringComparison.OrdinalIgnoreCase))
				this.AddPetAura(auras, record.GetString("petSkillName", 0), level, ownerTag, ownerIcon, []);
			else
				this.AddSummonAuras(auras, record, level, ownerTag, ownerIcon, []);
		}

		var granted = request.Equipment
			.Where(item => !item.IsInInactiveWeaponSlot(request.UseAlternate))
			.SelectMany(item => this._itemProvider.GetGrantedSkillLevels(item).Select(s => (Item: item, s.SkillId, s.Level)))
			.ToList();
		foreach (var (id, level) in SkillStatRules.ItemSkills(granted.Select(g => (g.SkillId, g.Level))))
		{
			// Read in the map of the item granting it, like the granted skills.
			var origins = granted.Where(g => g.SkillId.Equals(id)).Select(g => g.Item).Distinct().ToList();
			using var modScope = this._itemMods.ScopeOrNone(origins[0]);
			if (this._database.GetRecordFromFile(id) is { } record)
				this.AddSummonAuras(auras, record, level, this.NameTag(id, record), this.Icon(record), origins);
		}

		return [.. auras.Values.OrderBy(a => a.Id, StringComparer.Ordinal)];
	}

	/// <summary>The auras of the pet a summon spawns at a level (the last one past the end of its list).</summary>
	private void AddSummonAuras(Dictionary<RecordId, BuffSource> auras, DBRecordCollection summon, int level, string ownerTag, string? ownerIcon,
		IReadOnlyList<Item> origins)
	{
		if (level <= 0 || summon.GetAllStrings("spawnObjects") is not { Length: > 0 } pets)
			return;

		var pet = this.Record(pets[Math.Min(level, pets.Length) - 1]);
		if (pet is null)
			return;

		foreach (var variable in pet.Where(v => v.Name.StartsWith("skillName", StringComparison.OrdinalIgnoreCase)
			&& v.DataType == VariableDataType.StringVar && v.NumberOfValues > 0))
		{
			int petSkillLevel = (int)OptionalValue(pet, "skillLevel" + variable.Name["skillName".Length..], 0);
			this.AddPetAura(auras, variable.GetString(0), petSkillLevel, ownerTag, ownerIcon, origins);
		}
	}

	/// <summary>A pet's skill as a source when it is a radius buff that adds something at that level.</summary>
	private void AddPetAura(Dictionary<RecordId, BuffSource> auras, string? petSkillName, int level, string ownerTag, string? ownerIcon,
		IReadOnlyList<Item> origins)
	{
		if (level <= 0 || string.IsNullOrEmpty(petSkillName))
			return;

		var id = petSkillName.ToRecordId();
		var record = this._database.GetRecordFromFile(id);
		if (record is null || !record.RecordType.Contains("Radius", StringComparison.OrdinalIgnoreCase) || this.RoleOf(record) != SkillStatRole.Buff)
			return;

		if (auras.TryGetValue(id, out var known) && known.Level >= level)
			return;

		var sum = this.SkillRecordAttributes(id, record, level);
		if (sum.Values.Count == 0)
			return;

		var buff = this.BuffRecord(record);
		auras[id] = new BuffSource
		{
			Id = "pet:" + id.Normalized,
			Group = BuffGroup.Pet,
			// The aura's own name, else the summon's or the modifier's that gives it (Battle Standard, Triumph).
			NameTag = record.GetString(SkillDisplayName, 0) is { Length: > 0 } own ? own
				: buff?.GetString(SkillDisplayName, 0) is { Length: > 0 } buffName ? buffName
				: ownerTag,
			Icon = ownerIcon ?? this.Icon(record) ?? this.Icon(buff) ?? FallbackIcon,
			RecordId = id,
			Level = level,
			Origins = origins,
			Sum = sum,
		};
	}
}
