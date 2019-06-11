using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TQVaultAE.Data;
using TQVaultAE.Data;

namespace TQVaultAE.GUI.Services
{
	/// <summary>
	/// Win32 IGamePathResolver implementation
	/// </summary>
	public class GamePathResolverWin : IGamePathResolver
	{
		public string ResolveTQ()
		{
			string titanQuestGamePath = null;
			// We are either autodetecting or the path has not been set
			//
			// Detection logic for a GOG install of the anniversary edition ~Malgardian
			if (string.IsNullOrEmpty(titanQuestGamePath))
			{
				string[] path = { "SOFTWARE", "GOG.com", "Games", "1196955511", "PATH" };
				titanQuestGamePath = ReadRegistryKey(Microsoft.Win32.Registry.LocalMachine, path);
			}

			// Detection logic for a Steam install of the anniversary edition ~Malgardian
			if (string.IsNullOrEmpty(titanQuestGamePath))
			{
				string steamTQPath = "\\SteamApps\\common\\Titan Quest Anniversary Edition";

				string[] registryPath = { "Software", "Valve", "Steam", "SteamPath" };
				string steamPath = ReadRegistryKey(Microsoft.Win32.Registry.CurrentUser, registryPath).Replace("/", "\\");

				if (Directory.Exists(steamPath + steamTQPath))
				{
					titanQuestGamePath = steamPath + steamTQPath;
				}
				else
				{
					//further looking for Steam library
					//read libraryfolders.vdf
					Regex vdfPathRegex = new Regex("\"\\d+\"\t+\"([^\"]+)\"");  // "2"		"D:\\games\\Steam"
					string[] libFile = File.ReadAllLines(steamPath + "\\SteamApps\\libraryfolders.vdf");

					foreach (var line in libFile)
					{
						Match match = vdfPathRegex.Match(line.Trim());
						if (match.Success && Directory.Exists(match.Groups[1] + steamTQPath))
						{
							titanQuestGamePath = match.Groups[1] + steamTQPath;
							break;
						}
					}
				}
			}

			//Disc version detection logic -old
			if (string.IsNullOrEmpty(titanQuestGamePath))
			{
				string[] path = { "SOFTWARE", "Iron Lore", "Titan Quest", "Install Location" };
				titanQuestGamePath = ReadRegistryKey(Microsoft.Win32.Registry.LocalMachine, path);
			}

			if (string.IsNullOrEmpty(titanQuestGamePath) && !string.IsNullOrEmpty(IniProperties.GamePath))
			{
				titanQuestGamePath = IniProperties.GamePath;
			}

			if (string.IsNullOrEmpty(titanQuestGamePath))
			{
				throw new InvalidOperationException("Unable to locate Titan Quest installation directory. Please edit TQVaultAE.ini to contain a valid path in the option 'ForceGamePath'.");
			}

			return titanQuestGamePath;
		}

		public string ResolveTQIT()
		{
			string titanQuestGamePath = null;
			// We are either autodetecting or the path has not been set
			//
			// Detection logic for a GOG install of the anniversary edition ~Malgardian
			if (string.IsNullOrEmpty(titanQuestGamePath))
			{
				string[] path = { "SOFTWARE", "GOG.com", "Games", "1196955511", "PATH" };
				titanQuestGamePath = ReadRegistryKey(Microsoft.Win32.Registry.LocalMachine, path);
			}

			// Detection logic for a Steam install of the anniversary edition ~Malgardian
			if (string.IsNullOrEmpty(titanQuestGamePath))
			{
				string steamTQPath = "\\SteamApps\\common\\Titan Quest Anniversary Edition";

				string[] registryPath = { "Software", "Valve", "Steam", "SteamPath" };
				string steamPath = ReadRegistryKey(Microsoft.Win32.Registry.CurrentUser, registryPath).Replace("/", "\\");

				if (Directory.Exists(steamPath + steamTQPath))
				{
					titanQuestGamePath = steamPath + steamTQPath;
				}
				else
				{
					//further looking for Steam library
					//read libraryfolders.vdf
					Regex vdfPathRegex = new Regex("\"\\d+\"\t+\"([^\"]+)\"");  // "2"		"D:\\games\\Steam"
					string[] libFile = File.ReadAllLines(steamPath + "\\SteamApps\\libraryfolders.vdf");

					foreach (var line in libFile)
					{
						Match match = vdfPathRegex.Match(line.Trim());
						if (match.Success && Directory.Exists(match.Groups[1] + steamTQPath))
						{
							titanQuestGamePath = match.Groups[1] + steamTQPath;
							break;
						}
					}
				}
			}

			//Disc version detection logic -old
			if (string.IsNullOrEmpty(titanQuestGamePath))
			{
				string[] path = { "SOFTWARE", "Iron Lore", "Titan Quest", "Install Location" };
				titanQuestGamePath = ReadRegistryKey(Microsoft.Win32.Registry.LocalMachine, path);
			}

			if (string.IsNullOrEmpty(titanQuestGamePath) && !string.IsNullOrEmpty(IniProperties.GamePath))
			{
				titanQuestGamePath = IniProperties.GamePath;
			}

			if (string.IsNullOrEmpty(titanQuestGamePath))
			{
				throw new InvalidOperationException("Unable to locate Titan Quest installation directory. Please edit TQVaultAE.ini to contain a valid path in the option 'ForceGamePath'.");
			}

			return titanQuestGamePath;
		}

		/// <summary>
		/// Reads a value from the registry
		/// </summary>
		/// <param name="key">registry hive to be read</param>
		/// <param name="path">path of the value to be read</param>
		/// <returns>string value of the key and value that was read or empty string if there was a problem.</returns>
		string ReadRegistryKey(Microsoft.Win32.RegistryKey key, string[] path)
		{
			// The last item in the path array is the actual value name.
			int valueKey = path.Length - 1;

			// All other values in the path are keys which will need to be navigated.
			int lastSubKey = path.Length - 2;

			for (int i = 0; i <= lastSubKey; ++i)
			{
				key = key.OpenSubKey(path[i]);
				if (key == null)
				{
					return string.Empty;
				}
			}

			return (string)key.GetValue(path[valueKey]);
		}
	}
}
