namespace TQVaultAE.Application.KeyBinds;

/// <summary>
/// Every key and mouse binding of the application. The names are written in the configuration: never rename one.
/// </summary>
public enum KeyBindAction
{
	// Items
	PickUpItem,
	ItemMenu,
	ItemCopy,
	ItemDuplicate,
	ItemDelete,
	ItemCopyClick,
	ItemDuplicateClick,
	CancelDrag,

	// Selection
	SelectItemClick,
	SelectRectangle,
	SelectAll,
	SelectHighlighted,
	ClearSelection,
	MoveSelectionUp,
	MoveSelectionLeft,
	MoveSelectionDown,
	MoveSelectionRight,

	// Moving between the panels
	MoveToPlayer,
	MoveToVault,
	MoveToStash,

	// Window
	Search,
	CopyToClipboard,
	PasteFromClipboard,
	ZoomIn,
	ZoomOut,
	ZoomReset,
	RefreshPanel,
	ScrollVaultList,
	ResetZoomOnRestore,
	CloseWindow,

	// Forge
	Forge,

	// Character editor
	CharacterAddPoint,
	CharacterRemovePoint,
	CharacterSpendAll,
}

/// <summary>The group of an action in the key binds dialog.</summary>
public enum KeyBindCategory
{
	Items,
	Selection,
	Moving,
	Window,
	Forge,
	CharacterEditor,
}

/// <summary>Where the gestures of an action are read: two actions conflict only within the same scope.</summary>
public enum KeyBindScope
{
	/// <summary>The main window: its panels, the forge and its window keys.</summary>
	Main,

	/// <summary>The character editor.</summary>
	CharacterEditor,
}

/// <summary>What gestures an action takes.</summary>
public enum KeyBindKind
{
	/// <summary>Keys, with or without modifiers.</summary>
	Key,

	/// <summary>A left click with modifiers.</summary>
	Click,

	/// <summary>Shown for reference and for the conflicts, not changed: the mouse buttons, the wheel, the system keys.</summary>
	Fixed,
}

/// <summary>An action, where it belongs and its gestures by default.</summary>
/// <param name="Action">The action.</param>
/// <param name="Category">Its group in the dialog.</param>
/// <param name="Scope">Where its gestures are read.</param>
/// <param name="Kind">What gestures it takes.</param>
/// <param name="Defaults">Its gestures by default, at most <see cref="KeyBindMap.MaxGestures"/>.</param>
public sealed record KeyBindDefinition(
	KeyBindAction Action
	, KeyBindCategory Category
	, KeyBindScope Scope
	, KeyBindKind Kind
	, IReadOnlyList<KeyGesture> Defaults)
{
	/// <summary>The user may change its gestures.</summary>
	public bool Editable => this.Kind != KeyBindKind.Fixed;

	/// <summary>Whether <paramref name="gesture"/> suits this action: a key for a key action, a click for a click action.</summary>
	public bool Accepts(KeyGesture gesture)
		=> gesture.IsNone
		|| this.Kind switch
		{
			KeyBindKind.Key => gesture.IsKey,
			KeyBindKind.Click => gesture.IsClick,
			_ => false,
		};
}

/// <summary>
/// The catalog of the actions and their default gestures: the one place the application's keys are written.
/// </summary>
public static class KeyBindRegistry
{
	private static KeyGesture K(string key, KeyModifiers modifiers = KeyModifiers.None) => new(key, modifiers);

	private const KeyModifiers Ctrl = KeyModifiers.Ctrl;
	private const KeyModifiers Shift = KeyModifiers.Shift;
	private const KeyModifiers Alt = KeyModifiers.Alt;

	private static KeyBindDefinition D(KeyBindAction action, KeyBindCategory category, KeyBindKind kind, params KeyGesture[] defaults)
		=> new(action, category, category == KeyBindCategory.CharacterEditor ? KeyBindScope.CharacterEditor : KeyBindScope.Main, kind, defaults);

	/// <summary>Every action, in the order of the dialog.</summary>
	public static IReadOnlyList<KeyBindDefinition> All { get; } =
	[
		D(KeyBindAction.PickUpItem, KeyBindCategory.Items, KeyBindKind.Fixed, K(KeyGesture.Click)),
		D(KeyBindAction.ItemMenu, KeyBindCategory.Items, KeyBindKind.Fixed, K(KeyGesture.RightClick)),
		D(KeyBindAction.ItemCopy, KeyBindCategory.Items, KeyBindKind.Key, K("C")),
		D(KeyBindAction.ItemDuplicate, KeyBindCategory.Items, KeyBindKind.Key, K("D")),
		D(KeyBindAction.ItemCopyClick, KeyBindCategory.Items, KeyBindKind.Click, K(KeyGesture.Click, Alt)),
		D(KeyBindAction.ItemDuplicateClick, KeyBindCategory.Items, KeyBindKind.Click, K(KeyGesture.Click, Ctrl | Alt)),
		D(KeyBindAction.ItemDelete, KeyBindCategory.Items, KeyBindKind.Key, K("Back")),
		D(KeyBindAction.CancelDrag, KeyBindCategory.Items, KeyBindKind.Key, K("Escape")),

		D(KeyBindAction.SelectItemClick, KeyBindCategory.Selection, KeyBindKind.Click, K(KeyGesture.Click, Ctrl)),
		D(KeyBindAction.SelectRectangle, KeyBindCategory.Selection, KeyBindKind.Click, K(KeyGesture.Click, Shift)),
		D(KeyBindAction.SelectAll, KeyBindCategory.Selection, KeyBindKind.Key, K("A", Ctrl)),
		D(KeyBindAction.SelectHighlighted, KeyBindCategory.Selection, KeyBindKind.Key, K("S", Ctrl)),
		D(KeyBindAction.ClearSelection, KeyBindCategory.Selection, KeyBindKind.Key, K("D", Ctrl)),
		D(KeyBindAction.MoveSelectionUp, KeyBindCategory.Selection, KeyBindKind.Key, K("W", Alt)),
		D(KeyBindAction.MoveSelectionLeft, KeyBindCategory.Selection, KeyBindKind.Key, K("A", Alt)),
		D(KeyBindAction.MoveSelectionDown, KeyBindCategory.Selection, KeyBindKind.Key, K("S", Alt)),
		D(KeyBindAction.MoveSelectionRight, KeyBindCategory.Selection, KeyBindKind.Key, K("D", Alt)),

		D(KeyBindAction.MoveToPlayer, KeyBindCategory.Moving, KeyBindKind.Key, K("Right")),
		D(KeyBindAction.MoveToVault, KeyBindCategory.Moving, KeyBindKind.Key, K("Left")),
		D(KeyBindAction.MoveToStash, KeyBindCategory.Moving, KeyBindKind.Key, K("Down")),

		D(KeyBindAction.Search, KeyBindCategory.Window, KeyBindKind.Key, K("F", Ctrl)),
		D(KeyBindAction.CopyToClipboard, KeyBindCategory.Window, KeyBindKind.Key, K("C", Ctrl)),
		D(KeyBindAction.PasteFromClipboard, KeyBindCategory.Window, KeyBindKind.Key, K("V", Ctrl)),
		D(KeyBindAction.ZoomIn, KeyBindCategory.Window, KeyBindKind.Key, K("Add", Ctrl), K("Oemplus", Ctrl)),
		D(KeyBindAction.ZoomOut, KeyBindCategory.Window, KeyBindKind.Key, K("Subtract", Ctrl), K("OemMinus", Ctrl)),
		D(KeyBindAction.ZoomReset, KeyBindCategory.Window, KeyBindKind.Key, K("Home", Ctrl)),
		D(KeyBindAction.RefreshPanel, KeyBindCategory.Window, KeyBindKind.Key, K("F5")),
		D(KeyBindAction.ScrollVaultList, KeyBindCategory.Window, KeyBindKind.Fixed, K(KeyGesture.Wheel)),
		D(KeyBindAction.ResetZoomOnRestore, KeyBindCategory.Window, KeyBindKind.Fixed, K(string.Empty, Shift)),
		D(KeyBindAction.CloseWindow, KeyBindCategory.Window, KeyBindKind.Fixed, K("F4", Alt)),

		D(KeyBindAction.Forge, KeyBindCategory.Forge, KeyBindKind.Key, K("Enter"), K("Space")),

		D(KeyBindAction.CharacterAddPoint, KeyBindCategory.CharacterEditor, KeyBindKind.Fixed, K(KeyGesture.Click)),
		D(KeyBindAction.CharacterRemovePoint, KeyBindCategory.CharacterEditor, KeyBindKind.Fixed, K(KeyGesture.RightClick)),
		D(KeyBindAction.CharacterSpendAll, KeyBindCategory.CharacterEditor, KeyBindKind.Click, K(KeyGesture.Click, Shift)),
	];

	private static readonly Dictionary<KeyBindAction, KeyBindDefinition> ByAction = All.ToDictionary(d => d.Action);

	/// <summary>The definition of <paramref name="action"/>.</summary>
	public static KeyBindDefinition Get(KeyBindAction action) => ByAction[action];
}
