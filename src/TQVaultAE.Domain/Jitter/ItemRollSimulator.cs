//-----------------------------------------------------------------------
// <copyright file="ItemRollSimulator.cs" company="TQVaultAE">
//     Copyright (c) 2026 TQVaultAE Contributors. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using System.Runtime.CompilerServices;
using TQVaultAE.Domain.Entities;

namespace TQVaultAE.Domain.Jitter;

/// <summary>
/// Which record of an item a rolled value belongs to.
/// </summary>
public enum RollSource
{
	/// <summary>Base item record.</summary>
	Base,

	/// <summary>Prefix affix record.</summary>
	Prefix,

	/// <summary>Suffix affix record.</summary>
	Suffix,
}

/// <summary>
/// One value the simulator rolled.
/// </summary>
/// <param name="Attribute">Attribute that rolled it.</param>
/// <param name="Source">Record the value comes from.</param>
/// <param name="Variable">Variable name.</param>
/// <param name="Original">Database value.</param>
/// <param name="Rolled">In-game value.</param>
public sealed record RolledValue(RollAttribute Attribute, RollSource Source, string Variable, float Original, float Rolled);

/// <summary>
/// Smallest and largest in-game value a randomized variable can have, over every possible seed.
/// </summary>
/// <param name="Min">Lowest reachable value.</param>
/// <param name="Max">Highest reachable value.</param>
public readonly record struct ValueRange(float Min, float Max);

/// <summary>
/// Jittered copies of an item's records, as the game computes them.
/// </summary>
/// <param name="Base">Base item record with in-game values.</param>
/// <param name="Prefix">Prefix record with in-game values, or <see langword="null"/>.</param>
/// <param name="Suffix">Suffix record with in-game values, or <see langword="null"/>.</param>
/// <param name="Rolls">Every value that changed, in engine order.</param>
/// <param name="Ranges">Possible in-game values of every randomized variable, by record then variable name (case-insensitive).</param>
public sealed record ItemRollResult(DBRecordCollection? Base, DBRecordCollection? Prefix, DBRecordCollection? Suffix, IReadOnlyList<RolledValue> Rolls, IReadOnlyDictionary<RollSource, IReadOnlyDictionary<string, ValueRange>> Ranges)
{
	/// <summary>
	/// Possible in-game values of the randomized variables of one record (empty when none).
	/// </summary>
	public IReadOnlyDictionary<string, ValueRange> RangesOf(RollSource source)
		=> this.Ranges.TryGetValue(source, out var ranges) ? ranges : EmptyRanges;

	private static readonly IReadOnlyDictionary<string, ValueRange> EmptyRanges = new Dictionary<string, ValueRange>();
}

/// <summary>
/// Replays how the game rolls a new item's stats: one RNG seeded with the item seed,
/// shared by the five equipment stores in <see cref="EquipmentRollTable"/> order.
/// </summary>
/// <remarks>
/// Covers base, prefix and suffix of equipment (armor, jewellery, weapons, shields).
/// Socketed relics and charms are separate items, added after this and never scaled.
/// </remarks>
public static class ItemRollSimulator
{
	private const string LootRandomizerJitter = "lootRandomizerJitter";

	private const string AttributeScalePercent = "attributeScalePercent";

	/// <summary>Percent the base skill values roll at.</summary>
	private const float SkillStoreBaseJitterPercent = 20f;

	/// <summary>Reachable interval of a randomized value; damage pairs also keep the interval of their range.</summary>
	private sealed class Bound
	{
		public float Min;
		public float Max;
		public float RangeMin;
		public float RangeMax;
		public string? MinVariable;
	}

	/// <summary>Records and bounds of one source, during a roll.</summary>
	private sealed class Instance(DBRecordCollection? record, RollSource source)
	{
		public DBRecordCollection? Record { get; } = record;
		public RollSource Source { get; } = source;
		public Dictionary<string, Bound> Bounds { get; } = new(StringComparer.OrdinalIgnoreCase);
	}

	/// <summary>
	/// Rolls an item.
	/// </summary>
	/// <param name="seed">Item seed.</param>
	/// <param name="isArmor"><see langword="true"/> for <c>Armor</c> items (flag <c>+0x628</c>): helms, torsos, arms, legs.</param>
	/// <param name="baseRecord">Base item record.</param>
	/// <param name="prefix">Prefix record, or <see langword="null"/>.</param>
	/// <param name="suffix">Suffix record, or <see langword="null"/>.</param>
	/// <param name="table">Roll table; defaults to <see cref="EquipmentRollTable.Attributes"/>.</param>
	/// <returns>Jittered record copies and value ranges; the inputs are not modified.</returns>
	public static ItemRollResult Roll(uint seed, bool isArmor, DBRecordCollection? baseRecord, DBRecordCollection? prefix, DBRecordCollection? suffix, IReadOnlyList<RollAttribute>? table = null)
	{
		table ??= EquipmentRollTable.Attributes;

		var rng = TitanQuestRNG.FromItemSeed(seed);
		var rolled = RolledVariables(table);
		var b = new Instance(Clone(baseRecord, rolled), RollSource.Base);
		var p = new Instance(Clone(prefix, rolled), RollSource.Prefix);
		var s = new Instance(Clone(suffix, rolled), RollSource.Suffix);

		float prefixPct = p.Record?.GetSingle(LootRandomizerJitter, 0) ?? 0f;
		float suffixPct = s.Record?.GetSingle(LootRandomizerJitter, 0) ?? 0f;

		// 1. Store loads (jitter), in store order.
		foreach (var attr in table) {
			if (!IsLoaded(attr, isArmor))
				continue;

			if (attr.Scope == RollScope.None)
				continue; // AddJitter (and, for BaseAttackSpeed, the affix loaders) are no-ops.

			if (attr.Store is RollStore.Character or RollStore.Skill) {
				// Character and skill affixes: every non-zero affix value rolls, uncapped.
				RollFields(attr, p, v => ItemJitterService.RollCharAffix(v, prefixPct, rng), v => ItemJitterService.JitterBounds(v, prefixPct));
				RollFields(attr, s, v => ItemJitterService.RollCharAffix(v, suffixPct, rng), v => ItemJitterService.JitterBounds(v, suffixPct));
				if (attr.Scope == RollScope.All)
					AddJitter(attr, b, attr.BaseJitterPercent, rng);
				continue;
			}

			switch (attr.Scope) {
				case RollScope.All:
					AddJitter(attr, b, attr.BaseJitterPercent, rng);
					AddJitter(attr, p, prefixPct, rng);
					AddJitter(attr, s, suffixPct, rng);
					break;
				case RollScope.AffixOnlyArmor when isArmor:
					AddJitter(attr, p, attr.BaseJitterPercent, rng, positiveOnly: true);
					AddJitter(attr, s, attr.BaseJitterPercent, rng, positiveOnly: true);
					break;
			}
		}

		// 2. The base item's attributeScalePercent scales every store.
		float scale = Read(b.Record?[AttributeScalePercent]);
		if (scale != 0f) {
			foreach (var attr in table) {
				if (!IsLoaded(attr, isArmor) || !attr.Scales)
					continue;

				// Base armor protection and absorption scale the base item's armor only.
				ScaleFields(attr, b, scale);
				if (attr.Scope != RollScope.AffixOnlyArmor) {
					ScaleFields(attr, p, scale);
					ScaleFields(attr, s, scale);
				}
			}
		}

		// 3. The base Skill values roll last, at 20%, after scaling.
		foreach (var attr in table) {
			if (attr.Store == RollStore.Skill && attr.Scope == RollScope.All && IsLoaded(attr, isArmor))
				AddJitter(attr, b, SkillStoreBaseJitterPercent, rng);
		}

		var rolls = Diff(table, baseRecord, b.Record, RollSource.Base)
			.Concat(Diff(table, prefix, p.Record, RollSource.Prefix))
			.Concat(Diff(table, suffix, s.Record, RollSource.Suffix))
			.ToArray();
		var ranges = new[] { b, p, s }.ToDictionary(
			i => i.Source,
			i => (IReadOnlyDictionary<string, ValueRange>)i.Bounds.ToDictionary(kv => kv.Key, kv => new ValueRange(kv.Value.Min, kv.Value.Max), StringComparer.OrdinalIgnoreCase));

		return new ItemRollResult(b.Record, p.Record, s.Record, rolls, ranges);
	}

	/// <summary>
	/// The game's scaling of a value by p %: <c>(float)(int)((p / 100) * v) + v</c>, single precision.
	/// </summary>
	/// <param name="value">Value after jitter.</param>
	/// <param name="percent">Base item's <c>attributeScalePercent</c>.</param>
	/// <returns>Scaled value.</returns>
	public static float Scale(float value, float percent)
		=> (float)(int)((percent / 100f) * value) + value;

	private static bool IsLoaded(RollAttribute attr, bool isArmor)
		=> !((attr.Items == RollItemFilter.ArmorOnly && !isArmor) || (attr.Items == RollItemFilter.NonArmorOnly && isArmor));

	private static bool InstanceExists(RollAttribute attr, DBRecordCollection record)
	{
		if (attr.Requires is null)
			return true;

		foreach (var required in attr.Requires)
			if (Read(record[required]) == 0f)
				return false;
		return true;
	}

	private static void AddJitter(RollAttribute attr, Instance instance, float pct, TitanQuestRNG rng, bool positiveOnly = false)
	{
		// AddJitter's gate and cap.
		if (instance.Record is null || !(pct > 0f))
			return;

		if (pct >= ItemJitterService.MaxJitterPercent)
			pct = ItemJitterService.MaxJitterPercent;

		RollFields(attr, instance,
			v => positiveOnly && !(v > 0f) ? v : ItemJitterService.Jitter(v, pct, rng),
			v => positiveOnly && !(v > 0f) ? (v, v) : ItemJitterService.JitterBounds(v, pct));
	}

	private static void RollFields(RollAttribute attr, Instance instance, Func<float, float> roll, Func<float, (float Min, float Max)> bounds)
	{
		var record = instance.Record;
		if (record is null || !InstanceExists(attr, record))
			return; // LoadFromTable returns false: no instance, no draws.

		// Database and rolled values of this instance, by variable, for (min, range) pairs
		// (the record is updated in place, so the original min must be kept).
		Dictionary<string, (float Original, float Rolled)>? byName = null;

		foreach (var field in attr.Fields) {
			var variable = record[field.Variable];

			if (field.RangeOf is not null) {
				// Engine pair is (min, range): roll the range, in-game max = rolled min + rolled range.
				(float Original, float Rolled) minValues = default;
				var hasMin = byName is not null && byName.TryGetValue(field.RangeOf, out minValues);
				float min = hasMin ? minValues.Original : Read(record[field.RangeOf]);
				float max = Read(variable);
				float range = max > min ? max - min : 0f;
				if (field.RoundToInt)
					range = (uint)range;

				float rolledRange = range == 0f ? 0f : roll(range);
				var (rangeMin, rangeMax) = range == 0f ? (0f, 0f) : bounds(range);
				if (field.RoundToInt) {
					rolledRange = (uint)rolledRange;
					(rangeMin, rangeMax) = ((uint)rangeMin, (uint)rangeMax);
				}

				float rolledMin = hasMin ? minValues.Rolled : min;
				if (variable is not null && max > min) {
					Set(variable, rolledMin + rolledRange);
					var minBound = instance.Bounds.GetValueOrDefault(field.RangeOf);
					instance.Bounds[field.Variable] = new Bound {
						Min = (minBound?.Min ?? min) + rangeMin,
						Max = (minBound?.Max ?? min) + rangeMax,
						RangeMin = rangeMin,
						RangeMax = rangeMax,
						MinVariable = field.RangeOf,
					};
				}
				continue;
			}

			float value = Read(variable);
			if (value == 0f && field.DefaultFrom is not null)
				value = Read(record[field.DefaultFrom]);

			if (value == 0f)
				continue; // Jitter(0) returns before drawing.

			float rolled = roll(value);
			var (lo, hi) = bounds(value);
			if (field.RoundToInt) {
				rolled = (int)rolled;
				(lo, hi) = ((int)lo, (int)hi);
			}

			(byName ??= new(StringComparer.OrdinalIgnoreCase))[field.Variable] = (value, rolled);
			if (variable is not null) {
				Set(variable, rolled);
				instance.Bounds[field.Variable] = new Bound { Min = lo, Max = hi };
			}
		}
	}

	private static void ScaleFields(RollAttribute attr, Instance instance, float percent)
	{
		var record = instance.Record;
		if (record is null || !InstanceExists(attr, record))
			return;

		// Damage pairs are (min, range): only the min scales, so the max moves with it.
		Dictionary<string, float>? minDelta = null;
		foreach (var field in attr.Fields) {
			var variable = record[field.Variable];
			if (variable is null || !field.Scales)
				continue;

			var bound = instance.Bounds.GetValueOrDefault(field.Variable);
			float value = Read(variable);
			if (field.RangeOf is not null) {
				if (minDelta is not null && minDelta.TryGetValue(field.RangeOf, out var delta) && value != 0f)
					Set(variable, value + delta);

				if (bound is not null && instance.Bounds.GetValueOrDefault(field.RangeOf) is { } minBound) {
					bound.Min = minBound.Min + bound.RangeMin;
					bound.Max = minBound.Max + bound.RangeMax;
				}
				continue;
			}

			float scaled = Scale(value, percent);
			(minDelta ??= new(StringComparer.OrdinalIgnoreCase))[field.Variable] = scaled - value;
			Set(variable, scaled);

			if (bound is not null) {
				bound.Min = Scale(bound.Min, percent);
				bound.Max = Scale(bound.Max, percent);
			}
		}
	}

	private static IEnumerable<RolledValue> Diff(IReadOnlyList<RollAttribute> table, DBRecordCollection? original, DBRecordCollection? final, RollSource source)
	{
		if (original is null || final is null)
			yield break;

		var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
		foreach (var attr in table)
			foreach (var field in attr.Fields) {
				if (!seen.Add(field.Variable) || final[field.Variable] is not { } variable)
					continue;

				float before = Read(original[field.Variable]);
				float after = Read(variable);
				if (before != after)
					yield return new RolledValue(attr, source, variable.Name, before, after);
			}
	}

	private static void Set(Variable variable, float value)
		=> variable[0] = variable.DataType == VariableDataType.Integer ? (object)(int)value : value;

	private static float Read(Variable? variable)
		=> variable is null || variable.NumberOfValues == 0 || variable.DataType is not (VariableDataType.Integer or VariableDataType.Float)
			? 0f
			: variable.GetSingle(0);

	/// <summary>Names of the variables a roll can change (<see cref="RollField.Variable"/>), by table.</summary>
	private static readonly ConditionalWeakTable<IReadOnlyList<RollAttribute>, HashSet<string>> RolledVariablesByTable = [];

	private static HashSet<string> RolledVariables(IReadOnlyList<RollAttribute> table)
		=> RolledVariablesByTable.GetValue(table, static t => t
			.SelectMany(attr => attr.Fields)
			.Select(field => field.Variable)
			.ToHashSet(StringComparer.OrdinalIgnoreCase));

	/// <summary>
	/// Copy of <paramref name="record"/> the roll can change: only the <paramref name="rolled"/> variables are cloned, the
	/// others are shared with the database record, as nothing writes them. Cloning every variable of three records per item
	/// was most of the allocations of the search warm-up.
	/// </summary>
	private static DBRecordCollection? Clone(DBRecordCollection? record, HashSet<string> rolled)
	{
		if (record is null)
			return null;

		var copy = new DBRecordCollection(record.Id, record.RecordType, record.Count);
		foreach (var variable in record)
			copy.Set(rolled.Contains(variable.Name) ? variable.Clone() : variable);

		return copy;
	}
}
