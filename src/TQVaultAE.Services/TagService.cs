using System;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Logging;
using TQVaultAE.Domain.Contracts.Services;
using TQVaultAE.Config.Tags;
using TQVaultAE.Domain.Entities;
using Newtonsoft.Json;
using System.Drawing;
using System.Collections.Generic;

namespace TQVaultAE.Services;

/// <summary>
/// ITagService implementation
/// </summary>
public class TagService : ITagService
{
	protected readonly ILogger Log;
	protected readonly IGamePathService GamePathService;
	protected TagConfig TagConfig;
	public Dictionary<string, Color> Tags => this.TagConfig.tags.OrderBy(t => t.name).ToDictionary(
		t => t.name,
		t => Color.FromArgb(t.color.R, t.color.G, t.color.B)
	);

	public TagService(ILogger<TagService> log, IGamePathService iGamePathService)
	{
		this.Log = log;
		this.GamePathService = iGamePathService;
		this.ReadConfig();
	}

	public bool AddTag(string tagName)
	{
		Color color = Color.Gold;// Default color
		return AddTag(tagName, color.R, color.G, color.B);
	}

	public bool AddTag(string tagName, byte r, byte g, byte b)
	{
		var found = this.TagConfig.tags.FirstOrDefault(t => t.name.Equals(tagName, StringComparison.OrdinalIgnoreCase));
		if (found is null)
		{
			this.TagConfig.tags.Add(new TagInfo
			{
				name = tagName,
				color = new TagInfoColor
				{
					B = b,
					G = g,
					R = r,
				}
			});
			SaveConfig();
			return true;
		}
		return false;
	}

	public bool AssignTag(PlayerSave ps, string tagName)
	{
		var foundTag = this.TagConfig.tags.SingleOrDefault(t => t.name == tagName);
		if (foundTag is null)
			return false;

		var foundMapping = this.TagConfig.mapping.SingleOrDefault(m => m.player == ps.Name && m.isTQIT == ps.IsImmortalThrone && m.isMod == ps.IsCustom);
		if (foundMapping is null)
		{
			// Create new entry for this character
			foundMapping = new TagMapItem
			{
				isMod = ps.IsCustom,
				isTQIT = ps.IsImmortalThrone,
				player = ps.Name,
			};
			this.TagConfig.mapping.Add(foundMapping);
		}

		// Already assigned ?
		if (foundMapping.tags.Contains(tagName))
			return false;

		foundMapping.tags.Add(tagName);

		SaveConfig();

		LoadTags(ps); // refresh ps

		return true;
	}

	public bool UnassignTag(PlayerSave ps, string tagName)
	{
		var found = this.TagConfig.mapping.SingleOrDefault(m => m.player == ps.Name && m.isTQIT == ps.IsImmortalThrone && m.isMod == ps.IsCustom && m.tags.Contains(tagName));
		if (found is not null)
		{
			found.tags.Remove(tagName);

			SaveConfig();

			LoadTags(ps); // refresh ps

			return true;
		}

		return false;
	}

	public bool DeleteTag(string tagName)
	{
		var foundTag = this.TagConfig.tags.SingleOrDefault(t => t.name == tagName);
		if (foundTag is null)
			return false;

		// Remove from mapping
		this.TagConfig.mapping.Where(m => m.tags.Contains(tagName)).ToList()
			.ForEach(m => m.tags.Remove(tagName));

		// Delete tag
		this.TagConfig.tags.Remove(foundTag);

		SaveConfig();
		return true;
	}

	public void LoadTags(PlayerSave ps)
	{
		ps.Tags.Clear();

		var foundMapping = this.TagConfig.mapping.SingleOrDefault(m => m.player == ps.Name && m.isTQIT == ps.IsImmortalThrone && m.isMod == ps.IsCustom);
		if (foundMapping is not null)
		{
			(
				from mt in foundMapping.tags
				join t in this.TagConfig.tags on mt equals t.name
				orderby t.name
				select t
			).ToList().ForEach(t =>
				ps.Tags.Add(t.name, Color.FromArgb(t.color.R, t.color.G, t.color.B))
			);
		}
	}

	public void ReadConfig()
	{
		string configFilePath = this.ResolveConfigFilePath();

		if (File.Exists(configFilePath))
		{
			var content = File.ReadAllText(configFilePath);
			this.TagConfig = JsonConvert.DeserializeObject<TagConfig>(content);
			return;
		}

		this.TagConfig = new TagConfig();
	}

	private string ResolveConfigFilePath() => Path.Combine(this.GamePathService.TQVaultConfigFolder, @"TagConfig.json");

	public void SaveConfig()
	{
		if (this.TagConfig is null)
			return;

		string configFilePath = this.ResolveConfigFilePath();

		var content = JsonConvert.SerializeObject(this.TagConfig, Formatting.Indented);
		File.WriteAllText(configFilePath, content);
	}


	public bool UpdateTag(string tagNameOld, string tagNameNew, byte r, byte g, byte b)
	{
		var found = this.TagConfig.tags.SingleOrDefault(m => m.name == tagNameOld);
		if (found is not null)
		{
			found.name = tagNameNew;
			found.color = new TagInfoColor
			{
				B = b,
				G = g,
				R = r,
			};

			// update references inside mapping
			foreach (var map in this.TagConfig.mapping)
			{
				if (map.tags.Contains(tagNameOld))
				{
					map.tags.Remove(tagNameOld);
					map.tags.Add(tagNameNew);
				}
			}

			SaveConfig();

			return true;
		}
		return false;
	}
}