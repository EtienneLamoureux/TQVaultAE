namespace TQVaultAE.Presentation.Win32;

/// <summary>
/// The files built into this assembly that are the same in every language (<c>Resources/Fonts</c>,
/// <c>Resources/Files</c>): kept out of the translated resources.
/// </summary>
public static class EmbeddedAssets
{
	/// <summary>The Albertus MT font file.</summary>
	public static byte[] AlbertusMT => ReadBytes("Fonts.albertus mt.ttf");

	/// <summary>The Albertus MT Light font file.</summary>
	public static byte[] AlbertusMTLight => ReadBytes("Fonts.albertus mt light.ttf");

	/// <summary>The icon list read by <see cref="IconService"/> (JSON).</summary>
	public static string IconServiceList
	{
		get
		{
			using var reader = new StreamReader(Open("IconServiceList.json"));
			return reader.ReadToEnd();
		}
	}

	private static byte[] ReadBytes(string name)
	{
		using var stream = Open(name);
		using var copy = new MemoryStream();
		stream.CopyTo(copy);
		return copy.ToArray();
	}

	private static Stream Open(string name)
		=> typeof(EmbeddedAssets).Assembly.GetManifestResourceStream(name)
			?? throw new InvalidOperationException($"The embedded file \"{name}\" is missing.");
}
