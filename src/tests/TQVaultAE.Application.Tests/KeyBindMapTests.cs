using AwesomeAssertions;
using TQVaultAE.Application.KeyBinds;

namespace TQVaultAE.Tests.Application;

/// <summary>
/// <see cref="KeyGesture"/>, <see cref="KeyBindRegistry"/> and <see cref="KeyBindMap"/>: the gestures of the actions,
/// their changes, their conflicts and how they are saved.
/// </summary>
public class KeyBindMapTests
{
	private static KeyGesture G(string text)
		=> KeyGesture.TryParse(text, out var gesture) ? gesture : throw new ArgumentException(text);

	#region Gesture text

	[Theory]
	[InlineData("Ctrl+Alt+Click", "Ctrl+Alt+Click")]
	[InlineData("alt+ctrl+click", "Ctrl+Alt+click")]
	[InlineData(" Control + Home ", "Ctrl+Home")]
	[InlineData("Shift", "Shift")]
	[InlineData("Return", "Enter")]
	[InlineData("Next", "PageDown")]
	[InlineData("F5", "F5")]
	public void TryParse_ReadsModifiersAndKey_WritesThemInOrder(string text, string expected)
	{
		KeyGesture.TryParse(text, out var gesture).Should().BeTrue();

		gesture.ToString().Should().Be(expected);
	}

	[Theory]
	[InlineData("")]
	[InlineData("   ")]
	[InlineData("Ctrl+")]
	[InlineData("A+B")]
	[InlineData("W+Alt")]
	[InlineData("Ctrl+5A")]
	[InlineData("Ctrl+-")]
	public void TryParse_Unreadable_False(string text)
		=> KeyGesture.TryParse(text, out _).Should().BeFalse();

	[Fact]
	public void Equality_IgnoresTheCaseOfTheKeyAndTheAliases()
	{
		G("ctrl+home").Should().Be(G("Ctrl+HOME"));
		G("Return").Should().Be(G("Enter"));
		G("Ctrl+Home").Should().NotBe(G("Alt+Home"));
		G("Ctrl+Home").GetHashCode().Should().Be(G("ctrl+home").GetHashCode());
	}

	[Fact]
	public void Kinds_KeyClickMouseModifiersOnly()
	{
		G("Alt+W").IsKey.Should().BeTrue();
		G("Ctrl+Click").IsClick.Should().BeTrue();
		G("Ctrl+Click").IsKey.Should().BeFalse();
		G("RightClick").IsMouse.Should().BeTrue();
		G("Shift").IsModifiersOnly.Should().BeTrue();
		KeyGesture.None.IsNone.Should().BeTrue();
		KeyGesture.None.ToString().Should().BeEmpty();
	}

	#endregion

	#region Registry

	[Fact]
	public void Registry_EveryActionOnce()
		=> KeyBindRegistry.All.Select(d => d.Action).Should().BeEquivalentTo(Enum.GetValues<KeyBindAction>());

	[Fact]
	public void Registry_DefaultsSuitTheirKindAndFitTheSlots()
	{
		foreach (var definition in KeyBindRegistry.All.Where(d => d.Editable))
		{
			definition.Defaults.Should().OnlyContain(g => definition.Accepts(g), definition.Action.ToString());
			definition.Defaults.Count.Should().BeInRange(1, KeyBindMap.MaxGestures, definition.Action.ToString());
		}
	}

	[Fact]
	public void Registry_DefaultsHaveNoConflict()
		=> KeyBindMap.Defaults().Conflicts().Should().BeEmpty();

	[Fact]
	public void Registry_ClickShortcutsOfTheIssue_DuplicateOnCtrlAltClick_CopyOnAltClick_SelectStaysOnCtrlClick()
	{
		var map = KeyBindMap.Defaults();

		map.Matches(KeyBindAction.ItemDuplicateClick, G("Ctrl+Alt+Click")).Should().BeTrue();
		map.Matches(KeyBindAction.ItemCopyClick, G("Alt+Click")).Should().BeTrue();
		map.Matches(KeyBindAction.SelectItemClick, G("Ctrl+Click")).Should().BeTrue();
		map.Matches(KeyBindAction.Forge, G("Enter")).Should().BeTrue();
		map.Matches(KeyBindAction.Forge, G("Space")).Should().BeTrue();
	}

	#endregion

	#region Matching and editing

	[Fact]
	public void Matches_ExactModifiers()
	{
		var map = KeyBindMap.Defaults();

		map.Matches(KeyBindAction.SelectItemClick, G("Ctrl+Shift+Click")).Should().BeFalse();
		map.Matches(KeyBindAction.ItemCopy, G("Shift+C")).Should().BeFalse();
		map.Matches(KeyBindAction.ItemCopy, KeyGesture.None).Should().BeFalse();
	}

	[Fact]
	public void Find_FirstOfTheOfferedActions()
	{
		var map = KeyBindMap.Defaults();

		map.Find(G("Ctrl+Alt+Click"), KeyBindAction.ItemCopyClick, KeyBindAction.ItemDuplicateClick, KeyBindAction.SelectItemClick)
			.Should().Be(KeyBindAction.ItemDuplicateClick);
		map.Find(G("Click"), KeyBindAction.ItemCopyClick, KeyBindAction.SelectItemClick).Should().BeNull();
	}

	[Fact]
	public void Set_RebindsAndTheOldGestureNoLongerMatches()
	{
		var map = KeyBindMap.Defaults();

		map.Set(KeyBindAction.ItemDuplicate, 0, G("X")).Should().BeTrue();

		map.Matches(KeyBindAction.ItemDuplicate, G("X")).Should().BeTrue();
		map.Matches(KeyBindAction.ItemDuplicate, G("D")).Should().BeFalse();
		map.IsDefault(KeyBindAction.ItemDuplicate).Should().BeFalse();
	}

	[Fact]
	public void Set_WrongKindOrFixedOrSlot_Refused()
	{
		var map = KeyBindMap.Defaults();

		map.Set(KeyBindAction.ItemCopy, 0, G("Alt+Click")).Should().BeFalse("a key action takes keys");
		map.Set(KeyBindAction.ItemCopyClick, 0, G("Alt+C")).Should().BeFalse("a click action takes clicks");
		map.Set(KeyBindAction.PickUpItem, 0, G("Alt+Click")).Should().BeFalse("a fixed action cannot change");
		map.Set(KeyBindAction.ItemCopy, KeyBindMap.MaxGestures, G("X")).Should().BeFalse();

		map.ToOverrides().Should().BeEmpty("nothing changed");
	}

	[Fact]
	public void Set_SameGestureInTheOtherSlot_KeptOnce()
	{
		var map = KeyBindMap.Defaults();

		map.Set(KeyBindAction.ZoomIn, 0, G("Ctrl+Oemplus"));

		map.Get(KeyBindAction.ZoomIn).Should().Equal(G("Ctrl+Oemplus"));
	}

	[Fact]
	public void Set_None_Unbinds()
	{
		var map = KeyBindMap.Defaults();

		map.Set(KeyBindAction.Forge, 0, KeyGesture.None);
		map.Set(KeyBindAction.Forge, 1, KeyGesture.None);

		map.Get(KeyBindAction.Forge).Should().BeEmpty();
		map.Matches(KeyBindAction.Forge, G("Enter")).Should().BeFalse();
	}

	[Fact]
	public void IsDefault_SlotOrderDoesNotMatter()
	{
		var map = KeyBindMap.Defaults();

		map.Set(KeyBindAction.ZoomIn, 0, KeyGesture.None);
		map.Set(KeyBindAction.ZoomIn, 0, G("Ctrl+Oemplus"));
		map.Set(KeyBindAction.ZoomIn, 1, G("Ctrl+Add"));

		map.IsDefault(KeyBindAction.ZoomIn).Should().BeTrue();
	}

	[Fact]
	public void Reset_OneActionOrAll()
	{
		var map = KeyBindMap.Defaults();
		map.Set(KeyBindAction.ItemCopy, 0, G("X"));
		map.Set(KeyBindAction.Search, 0, G("Ctrl+G"));

		map.Reset(KeyBindAction.ItemCopy);
		map.IsDefault(KeyBindAction.ItemCopy).Should().BeTrue();
		map.IsDefault(KeyBindAction.Search).Should().BeFalse();

		map.ResetAll();
		map.ToOverrides().Should().BeEmpty();
	}

	[Fact]
	public void Clone_EditedWithoutChangingTheOriginal_CopyFromTakesTheEdits()
	{
		var live = KeyBindMap.Defaults();
		var edited = live.Clone();

		edited.Set(KeyBindAction.ItemCopy, 0, G("X"));
		live.Matches(KeyBindAction.ItemCopy, G("C")).Should().BeTrue();

		live.CopyFrom(edited);
		live.Matches(KeyBindAction.ItemCopy, G("X")).Should().BeTrue();
		edited.Set(KeyBindAction.ItemCopy, 0, G("Y"));
		live.Matches(KeyBindAction.ItemCopy, G("X")).Should().BeTrue("the copy holds its own slots");
	}

	#endregion

	#region Conflicts

	[Fact]
	public void Conflicts_SameGestureInOneScope_ListsBothActions()
	{
		var map = KeyBindMap.Defaults();

		map.Set(KeyBindAction.ItemDuplicate, 0, G("C"));

		var conflict = map.Conflicts().Should().ContainSingle().Subject;
		conflict.Gesture.Should().Be(G("C"));
		conflict.Actions.Should().Equal(KeyBindAction.ItemCopy, KeyBindAction.ItemDuplicate);
		map.HasConflict(KeyBindAction.ItemDuplicate, 0).Should().BeTrue();
		map.HasConflict(KeyBindAction.ItemCopy, 0).Should().BeTrue();
		map.HasConflict(KeyBindAction.Search, 0).Should().BeFalse();
	}

	[Fact]
	public void Conflicts_WithAFixedAction_Counted()
	{
		var map = KeyBindMap.Defaults();

		map.Set(KeyBindAction.SelectItemClick, 0, G("Click"));

		map.Conflicts().Should().ContainSingle()
			.Which.Actions.Should().Equal(KeyBindAction.PickUpItem, KeyBindAction.SelectItemClick);
	}

	[Fact]
	public void Conflicts_OtherScope_NotAConflict()
	{
		// Shift+Click selects a rectangle in the main window and spends every point in the character editor.
		var map = KeyBindMap.Defaults();

		map.Matches(KeyBindAction.SelectRectangle, G("Shift+Click")).Should().BeTrue();
		map.Matches(KeyBindAction.CharacterSpendAll, G("Shift+Click")).Should().BeTrue();
		map.Conflicts().Should().BeEmpty();
	}

	#endregion

	#region Persistence

	[Fact]
	public void ToOverrides_OnlyTheChangedActions_FromOverridesGivesTheSameMap()
	{
		var map = KeyBindMap.Defaults();
		map.Set(KeyBindAction.ItemDuplicateClick, 0, G("Ctrl+Shift+Click"));
		map.Set(KeyBindAction.ZoomIn, 1, G("Ctrl+Up"));
		map.Set(KeyBindAction.Forge, 0, KeyGesture.None);
		map.Set(KeyBindAction.Forge, 1, KeyGesture.None);

		var overrides = map.ToOverrides();

		overrides.Should().BeEquivalentTo(new Dictionary<string, string>
		{
			["ItemDuplicateClick"] = "Ctrl+Shift+Click",
			["ZoomIn"] = "Ctrl+Add, Ctrl+Up",
			["Forge"] = "",
		});
		var read = KeyBindMap.FromOverrides(overrides);
		foreach (var definition in KeyBindRegistry.All)
			read.Get(definition.Action).Should().Equal(map.Get(definition.Action), definition.Action.ToString());
	}

	[Fact]
	public void FromOverrides_UnknownFixedOrNumericNames_Skipped()
	{
		var map = KeyBindMap.FromOverrides(new Dictionary<string, string>
		{
			["NoSuchAction"] = "X",
			["PickUpItem"] = "Alt+Click",
			["3"] = "X",
			["itemcopy"] = "X",
		});

		map.ToOverrides().Should().BeEmpty();
	}

	[Fact]
	public void FromOverrides_UnreadableGestures_SkippedAndAllUnreadableKeepsTheDefault()
	{
		var map = KeyBindMap.FromOverrides(new Dictionary<string, string>
		{
			["ItemCopy"] = "Ctrl+-, X, Alt+Click",
			["ItemDelete"] = "???",
		});

		map.Get(KeyBindAction.ItemCopy).Should().Equal(G("X"));
		map.IsDefault(KeyBindAction.ItemDelete).Should().BeTrue();
	}

	[Fact]
	public void FromOverrides_MoreGesturesThanSlots_KeepsTheFirst()
	{
		var map = KeyBindMap.FromOverrides(new Dictionary<string, string> { ["Search"] = "Ctrl+F, Ctrl+G, Ctrl+H" });

		map.Get(KeyBindAction.Search).Should().Equal(G("Ctrl+F"), G("Ctrl+G"));
	}

	#endregion
}
