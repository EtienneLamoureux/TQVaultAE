namespace TQVaultAE.Presentation
{
	using Microsoft.Extensions.Logging;
	using Newtonsoft.Json;
	using System.Linq;
	using System.IO;
	using TQVaultAE.Domain.Contracts.Services;
	using TQVaultAE.Presentation.Models;
	using TQVaultAE.Domain.Contracts.Providers;
	using System.Text.RegularExpressions;
	using System.Collections.ObjectModel;
	using TQVaultAE.Domain.Entities;

	/// <summary>
	/// Loads Titan Quest Icons.
	/// </summary>
	public class IconService : IIconService
	{
		private readonly ILogger Log;
		private readonly IDatabase Database;
		private readonly IUIService UIService;
		private ReadOnlyCollection<IconInfo> Pictures;

		public IconService(ILogger<IconService> log, IDatabase database, IUIService uiService)
		{
			this.Log = log;
			this.Database = database;
			this.UIService = uiService;
		}

		void InitIconList()
		{
			if (Pictures is not null) return;

			Log.LogDebug(@"START LOADING ICON DATABASE!");

			var configfile = JsonConvert.DeserializeObject<ConfRoot>(Resources.IconServiceList);

			// Build Keys
			var consolitatedFilekeys =
				from file in configfile.list
				let filename = file.fileName
				let arcpath = Database.ResolveArcFileName(filename)
				where File.Exists(arcpath)
				let arcfile = Database.ReadARCFile(arcpath)
				from key in arcfile.DirectoryEntries.Keys.Cast<string>()
				select filename + '\\' + key;

			// Regex Match
			var regexMatch =
				from file in configfile.list
				from img in file.imgMatch
				where img.IsRegex
				from key in consolitatedFilekeys
				let pattern = file.fileName.Replace(@"\", @"\\") + @"\\" + img.Pattern
				let match = Regex.Match(key, pattern)
				where match.Success
				let onrep = img.On.Split('|')
				let ofrep = img.Off.Split('|')
				let ovrep = img.Over.Split('|')
				let onID = string.IsNullOrEmpty(img.On) ? null : replace(key, onrep)
				let offID = string.IsNullOrEmpty(img.Off) ? null : replace(key, ofrep)
				let ovID = string.IsNullOrEmpty(img.Over) ? null : replace(key, ovrep)
				let resOn = Database.LoadResource(onID)
				let resOff = Database.LoadResource(offID)
				let resOver = Database.LoadResource(ovID)
				let iconinfo = new IconInfo(
					img.Category
					, onID
					, resOn is null ? null : this.UIService.LoadBitmap(onID, resOn)
					, offID
					, resOff is null ? null : this.UIService.LoadBitmap(offID, resOff)
					, ovID
					, resOver is null ? null : this.UIService.LoadBitmap(ovID, resOver)
				)
				where 
				(// Square Only for Shields
					iconinfo.Category == IconCategory.Shields
					&& iconinfo.OffBitmap.Size.Width == iconinfo.OffBitmap.Size.Height
				) 
				// Everything else
				|| iconinfo.Category != IconCategory.Shields
				select iconinfo;

			var literalMatch =
				from file in configfile.list
				from img in file.imgMatch
				where img.Literal?.Any() ?? false
				from lit in img.Literal
				let resID = file.fileName + '\\' + lit
				let res = Database.LoadResource(resID)
				let bmp = res is null ? null : this.UIService.LoadBitmap(resID, res)
				select new IconInfo(
					img.Category
					, resID
					, bmp
					, resID
					, bmp
					, resID
					, bmp
				);

			var result = regexMatch.Concat(literalMatch);

			var distinct =
				from ii in result
				group ii by new { ii.Off, ii.On, ii.Over } into grp
				select grp.First();

			// dispatch resourceid in Bitmap
			foreach (var info in distinct)
			{
				if (info.OnBitmap is null)
					Log.LogWarning(@"ON ""{On}"" not found !", info.On);
				else info.OnBitmap.Tag = info.On;

				if (info.OffBitmap is null)
					Log.LogWarning(@"OFF ""{Off}"" not found !", info.Off);
				else info.OffBitmap.Tag = info.Off;

				if (info.OverBitmap is null)
					Log.LogWarning(@"OVER ""{Over}"" not found !", info.Over);
				else info.OverBitmap.Tag = info.Over;
			}

			Log.LogInformation("{IconInfoCount} IconInfo Extracted !", result.Count());

			Pictures = distinct.ToList().AsReadOnly();

			Log.LogInformation("{IconInfoCount} IconInfo Reduced !", Pictures.Count);

			Log.LogDebug(@"STOP LOADING ICON DATABASE!");
		}

		private static string replace(string input, string[] onrep)
		{
			var oldval = onrep.First();
			var newval = onrep.Last();

			if (oldval == newval || oldval == string.Empty) return input;

			return input.Replace(oldval, newval);
		}

		public ReadOnlyCollection<IconInfo> GetIconDatabase()
		{
			InitIconList();
			return Pictures;
		}
	}
}