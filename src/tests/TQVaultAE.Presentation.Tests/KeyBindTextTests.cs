using System.Globalization;
using AwesomeAssertions;
using TQVaultAE.Application.KeyBinds;
using TQVaultAE.Presentation;

namespace TQVaultAE.Tests.Presentation;

/// <summary>
/// <see cref="KeyBindText"/>: every action and group has a name in English and French, and the gestures read as the
/// keyboard names them.
/// </summary>
public class KeyBindTextTests
{
	private static KeyGesture G(string text)
		=> KeyGesture.TryParse(text, out var gesture) ? gesture : throw new ArgumentException(text);

	private static T InCulture<T>(string name, Func<T> read)
	{
		// The UI culture of this thread only: Resources.Culture is shared by the tests running in parallel.
		var previous = CultureInfo.CurrentUICulture;
		CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo(name);
		try
		{
			return read();
		}
		finally
		{
			CultureInfo.CurrentUICulture = previous;
		}
	}

	[Theory]
	[InlineData("en")]
	[InlineData("fr")]
	public void EveryActionAndCategory_HasItsOwnName(string culture)
	{
		// The strings of that culture alone, without falling back to English: English is the neutral resources.
		var cultureInfo = culture == "en" ? CultureInfo.InvariantCulture : CultureInfo.GetCultureInfo(culture);
		var missing = Enum.GetNames<KeyBindAction>().Select(a => $"KeyBindAction{a}")
			.Concat(Enum.GetNames<KeyBindCategory>().Select(c => $"KeyBindCategory{c}"))
			.Where(name => Resources.ResourceManager.GetResourceSet(cultureInfo, createIfNotExists: true, tryParents: false)?.GetString(name) is null);

		missing.Should().BeEmpty("a missing string falls back to the enum name, or to English");
		InCulture(culture, () => Enum.GetValues<KeyBindAction>().Select(KeyBindText.Action).ToList()).Should().OnlyHaveUniqueItems();
		InCulture(culture, () => Enum.GetValues<KeyBindCategory>().Select(KeyBindText.Category).ToList()).Should().OnlyHaveUniqueItems();
	}

	[Fact]
	public void French_TranslatesTheActions()
		=> InCulture("fr", () => KeyBindText.Action(KeyBindAction.ItemDuplicateClick)).Should().Be("Reproduire l'objet cliqué");

	[Theory]
	[InlineData("Ctrl+Alt+Click", "Ctrl+Alt+Click")]
	[InlineData("Shift+Left", "Shift+Left arrow")]
	[InlineData("Ctrl+Oemplus", "Ctrl++")]
	[InlineData("Ctrl+Add", "Ctrl+Num +")]
	[InlineData("D5", "5")]
	[InlineData("NumPad5", "Num 5")]
	[InlineData("Back", "Backspace")]
	[InlineData("F5", "F5")]
	public void Gesture_English(string gesture, string expected)
		=> InCulture("en", () => KeyBindText.Gesture(G(gesture))).Should().Be(expected);

	[Theory]
	[InlineData("Ctrl+Alt+Click", "Ctrl+Alt+Clic")]
	[InlineData("Shift+Click", "Maj+Clic")]
	[InlineData("Escape", "Échap")]
	public void Gesture_French(string gesture, string expected)
		=> InCulture("fr", () => KeyBindText.Gesture(G(gesture))).Should().Be(expected);

	[Fact]
	public void Gesture_None_ReadsAsNone()
		=> InCulture("en", () => KeyBindText.Gesture(KeyGesture.None)).Should().Be("(none)");

	[Fact]
	public void Conflict_NamesTheGestureAndTheActions()
	{
		var map = KeyBindMap.Defaults();
		map.Set(KeyBindAction.ItemDuplicate, 0, G("C"));

		InCulture("en", () => KeyBindText.Conflict(map.Conflicts().Single()))
			.Should().Be("C is used by: Copy the item under the cursor, Duplicate the item under the cursor");
	}
}
