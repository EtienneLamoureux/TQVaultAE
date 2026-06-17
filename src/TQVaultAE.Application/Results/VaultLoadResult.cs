namespace TQVaultAE.Application.Results;

public class VaultLoadResult
{
	/// <summary>
	/// The loaded vault collection.
	/// </summary>
	public PlayerCollection Vault { get; set; }

	/// <summary>
	/// Path to the vault file.
	/// </summary>
	public string Filename { get; set; }

	/// <summary>
	/// Indicates whether the vault was loaded successfully.
	/// </summary>
	public bool VaultLoaded { get; set; }

	/// <summary>
	/// Indicates whether the vault load was successful.
	/// </summary>
	public bool IsSuccess => Vault != null;

	/// <summary>
	/// Any exception that occurred during loading.
	/// </summary>
	public Exception Error { get; set; }

	/// <summary>
	/// Argument exception if the file format was invalid.
	/// </summary>
	public ArgumentException ArgumentException { get; set; }

	/// <summary>
	/// Creates a successful result.
	/// </summary>
	public static VaultLoadResult Succeeded(PlayerCollection vault, string filename)
		=> new() { Vault = vault, Filename = filename, VaultLoaded = true };

	/// <summary>
	/// Creates a failed result with an argument exception.
	/// </summary>
	public static VaultLoadResult FailedArgument(string message)
		=> new() { ArgumentException = new ArgumentException(message), VaultLoaded = false };

	/// <summary>
	/// Creates a failed result.
	/// </summary>
	public static VaultLoadResult Failed(Exception ex)
		=> new() { Error = ex, VaultLoaded = false };
}
