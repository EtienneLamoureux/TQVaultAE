using System.Collections.ObjectModel;
using TQVaultAE.Domain.Entities;

namespace TQVaultAE.Application.Contracts.Services;

public interface IIconService
{
	/// <summary>
	/// Return Icons from the database
	/// </summary>
	/// <returns></returns>
	ReadOnlyCollection<IconInfo> GetIconDatabase();
}