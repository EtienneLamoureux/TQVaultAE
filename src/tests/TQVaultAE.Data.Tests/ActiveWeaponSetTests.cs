using System.Text;
using AwesomeAssertions;
using TQVaultAE.Domain.Entities;

namespace TQVaultAE.Tests.Data;

/// <summary>The weapon set in hand: the save's <c>useAlternate</c> and the weapons that count.</summary>
public sealed class ActiveWeaponSetTests : IDisposable
{
	private readonly SaveFixtures _fixtures = new();

	public void Dispose() => this._fixtures.Dispose();

	/// <summary>The test character with <c>useAlternate</c> set to <paramref name="value"/>.</summary>
	private static byte[] WithUseAlternate(int value)
	{
		var chr = File.ReadAllBytes(SaveFixtures.PlayerChr);
		var key = Encoding.ASCII.GetBytes("useAlternate");
		var at = chr.AsSpan().IndexOf(key) + key.Length;
		BitConverter.GetBytes(value).CopyTo(chr, at);
		return chr;
	}

	/// <summary>The useAlternate value written in a player file.</summary>
	private static int SavedUseAlternate(byte[] chr)
	{
		var key = Encoding.ASCII.GetBytes("useAlternate");
		return BitConverter.ToInt32(chr, chr.AsSpan().IndexOf(key) + key.Length);
	}

	[Fact]
	public void LoadFile_UseAlternate_IsRead()
	{
		this._fixtures.LoadPlayer().UseAlternate.Should().BeFalse();
		this._fixtures.Reload(WithUseAlternate(1)).UseAlternate.Should().BeTrue();
	}

	[Fact]
	public void Encode_UseAlternate_KeepsTheSavedValue()
	{
		var pc = this._fixtures.Reload(WithUseAlternate(1));

		var encoded = this._fixtures.PlayerProvider.Encode(pc);

		this._fixtures.Reload(encoded).UseAlternate.Should().BeTrue();
	}

	[Theory]
	[InlineData(0, true)]
	[InlineData(1, false)]
	public void Encode_WeaponSetInHandChanged_WritesIt(int saved, bool inHand)
	{
		var pc = this._fixtures.Reload(WithUseAlternate(saved));
		pc.UseAlternate = inHand;

		var encoded = this._fixtures.PlayerProvider.Encode(pc);

		SavedUseAlternate(encoded).Should().Be(inHand ? 1 : 0);
		encoded.Length.Should().Be(WithUseAlternate(saved).Length);
	}

	[Fact]
	public void Encode_UnchangedNonZeroValue_KeepsItsBytes()
	{
		// Any value but 0 means the second set: an unchanged one is not rewritten as 1.
		var pc = this._fixtures.Reload(WithUseAlternate(7));

		SavedUseAlternate(this._fixtures.PlayerProvider.Encode(pc)).Should().Be(7);
	}

	[Theory]
	[InlineData(0, false, false)]
	[InlineData(1, false, false)]
	[InlineData(2, false, true)]
	[InlineData(3, false, true)]
	[InlineData(0, true, true)]
	[InlineData(1, true, true)]
	[InlineData(2, true, false)]
	[InlineData(3, true, false)]
	public void IsInInactiveWeaponSlot_FollowsTheSetInHand(int row, bool useAlternate, bool expected)
		=> new Item { PositionX = Item.WeaponSlotIndicator, PositionY = row }.IsInInactiveWeaponSlot(useAlternate).Should().Be(expected);

	[Fact]
	public void IsInInactiveWeaponSlot_ArmorSlot_IsNeverInactive()
		=> new Item { PositionX = 0, PositionY = 2 }.IsInInactiveWeaponSlot(false).Should().BeFalse();
}
