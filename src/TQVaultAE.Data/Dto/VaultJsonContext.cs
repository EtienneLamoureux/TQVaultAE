using System.Text.Json.Serialization;

namespace TQVaultAE.Data.Dto;

/// <summary>
/// Serialization code of the vault files, generated at build time: reading the first vaults in parallel no longer
/// waits for System.Text.Json to build their metadata by reflection. Same options as the application's
/// <see cref="System.Text.Json.JsonSerializerOptions"/>.
/// </summary>
[JsonSourceGenerationOptions(IncludeFields = true, PropertyNameCaseInsensitive = true, WriteIndented = true)]
[JsonSerializable(typeof(VaultDto))]
internal sealed partial class VaultJsonContext : JsonSerializerContext
{
}
