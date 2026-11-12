using System.Text;
using static TQVaultAE.Tests.DbCli.DbCliFixtures;

namespace TQVaultAE.Tests.DbCli.Cli;

/// <summary>
/// A small game install in a temp folder (database.arz, item/audio archives, EN and FR text archives)
/// to run <c>tqdb</c> commands end to end through <see cref="TQVaultAE.DbCli.Program.Run"/>.
/// </summary>
internal sealed class FakeGameInstall : IDisposable
{
	public const string SwordA = """records\item\sword\a01.dbr""";
	public const string SwordB = """records\item\sword\a02.dbr""";
	public const string OldSwordA = """records\old\a01.dbr""";
	public const string Relic = """records\item\relic\r01.dbr""";
	public const string LootTable = """records\loot\table01.dbr""";

	/// <summary>Pixels of a 2x1 32-bit texture.</summary>
	public static readonly byte[] Pixels = [0, 0, 255, 255, 255, 0, 0, 255];

	public static readonly byte[] TextureA = Tex(1, GameDds(2, 1, 32, Pixels));
	public static readonly byte[] TextureAXPack = Tex(1, GameDds(2, 1, 32, [.. Pixels.Reverse()]));
	public static readonly byte[] AnimatedTexture = Tex(1, GameDds(2, 1, 32, Pixels), GameDds(2, 1, 32, Pixels));
	public static readonly byte[] BrokenTexture = "TEX\u0001 not a texture"u8.ToArray();
	public static readonly byte[] HitSound = Encoding.ASCII.GetBytes(string.Concat(Enumerable.Repeat("hit-sound-", 50)));
	public static readonly byte[] Mesh = [1, 2, 3, 4, 5, 6, 7, 8];

	public FakeGameInstall()
	{
		this.Root = Path.Combine(Path.GetTempPath(), $"tqdb-game-{Guid.NewGuid():N}");
		Directory.CreateDirectory(Path.Combine(this.Root, "Database"));
		File.WriteAllBytes(Path.Combine(this.Root, "Database", "database.arz"), BuildDatabase());

		this.WriteArc("""Resources\Items.arc""",
			Compressed("sword/a01.tex", TextureA),
			Compressed("sword/anim.tex", AnimatedTexture),
			Compressed("sword/bad.tex", BrokenTexture),
			Stored("sword/a01.msh", Mesh));
		this.WriteArc("""Resources\XPack\Items.arc""",
			Compressed("sword/a01.tex", TextureAXPack));
		this.WriteArc("""Audio\Sounds.arc""",
			Compressed("hit.mp3", HitSound, partSize: 128),
			Compressed("corrupt.wav", HitSound) with { Adler = 0xDEADBEEF });
		this.WriteArc("""Text\Text_EN.arc""",
			Stored("text/items.txt", Encoding.ASCII.GetBytes("tagSwordA=Alpha Blade\r\ntagSwordB=Beta Blade\r\ntagRelic=Relic of Tests\r\n")));
		this.WriteArc("""Text\Text_FR.arc""",
			Stored("text/items.txt", Encoding.ASCII.GetBytes("tagSwordA=Lame Alpha\r\n")));

		this.Out = Path.Combine(this.Root, "out");
	}

	/// <summary>Install folder.</summary>
	public string Root { get; }

	/// <summary>Output folder for <c>--out</c>, inside <see cref="Root"/> (an .arc written there would be found too).</summary>
	public string Out { get; }

	/// <summary>Writes an archive under the install.</summary>
	public string WriteArc(string relativePath, params ArcSpec[] entries)
	{
		var path = Path.Combine(this.Root, relativePath);
		Directory.CreateDirectory(Path.GetDirectoryName(path)!);
		File.WriteAllBytes(path, BuildArc(entries));
		return path;
	}

	/// <summary>Runs <c>tqdb --game &lt;root&gt; args</c>; returns the exit code and stderr.</summary>
	public (int Code, string Error) Run(params string[] args) => this.Run(CancellationToken.None, args);

	/// <summary>Runs with a cancellation token.</summary>
	public (int Code, string Error) Run(CancellationToken cancellationToken, params string[] args)
	{
		var error = new StringWriter();
		int code = TQVaultAE.DbCli.Program.Run(["--game", this.Root, .. args], error, cancellationToken);
		return (code, error.ToString());
	}

	/// <summary>Path under <see cref="Out"/>.</summary>
	public string OutPath(string relative) => Path.Combine(this.Out, relative);

	public void Dispose()
	{
		try
		{
			Directory.Delete(this.Root, recursive: true);
		}
		catch (IOException)
		{
		}
	}

	private static byte[] BuildDatabase()
	{
		static ArzVar Int(string name, params int[] values) => new(0, name, values);
		static ArzVar Float(string name, float value) => new(1, name, [BitConverter.SingleToInt32Bits(value)]);

		var strings = new List<string>();
		ArzVar Str(string name, params string[] values)
		{
			var raw = new int[values.Length];
			for (int i = 0; i < values.Length; i++)
			{
				int index = strings.IndexOf(values[i]);
				if (index < 0)
				{
					strings.Add(values[i]);
					index = strings.Count - 1;
				}
				raw[i] = index;
			}
			return new ArzVar(2, name, raw);
		}

		ArzRec[] records = [
			new(SwordA, "WeaponMelee_Sword", 0,
				Str("Class", "WeaponMelee_Sword"), Str("itemNameTag", "tagSwordA"), Int("itemLevel", 25),
				Float("offensivePhysicalMin", 12.5f), Str("bitmap", """Items\sword\a01.tex""")),
			new(SwordB, "WeaponMelee_Sword", 0,
				Str("Class", "WeaponMelee_Sword"), Str("itemNameTag", "tagSwordB"), Int("itemLevel", 40)),
			new(OldSwordA, "WeaponMelee_Sword", 0,
				Str("Class", "WeaponMelee_Sword"), Int("itemLevel", 1)),
			new(Relic, "ItemRelic", 0,
				Str("Class", "ItemRelic"), Str("description", "tagRelic"), Int("completedRelicLevel", 3)),
			new(LootTable, "LootItemTable_FixedWeight", 0,
				Str("Class", "LootItemTable_FixedWeight"), Str("lootName1", SwordA), Str("lootName2", SwordB), Int("lootWeight1", 10)),
		];
		return BuildArz(records, strings);
	}
}
