using System.Collections.Generic;
using TQVaultAE.Domain.Search;

namespace TQVaultAE.Domain.Contracts.Services
{
	public interface ISearchService
	{
		/// <summary>
		/// Searches loaded files based on the specified search string.  Internally normalized to UpperInvariant
		/// </summary>
		/// <param name="searchString">string that we are searching for</param>
		List<Result> Search(string searchString);
	}
}