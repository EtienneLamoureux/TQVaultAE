using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TQ.SaveFilesExplorer.Helpers
{
	public static class TQPath
	{

		#region various path

		public static string SaveDirectoryTQITModded
		{
			get
			{
				var p = $@"{PersonalFolderTQIT}\SaveData\User";
				return Directory.Exists(p) ? p : null;
			}
		}

		public static string[] SaveDirectoryTQITModdedPlayers
		{
			get
			{
				var p = SaveDirectoryTQITModded;
				return p is null ? Array.Empty<string>() : Directory.GetDirectories(p);
			}
		}

		public static string[] SaveDirectoryTQITModdedTransferStash
		{
			get
			{
				var sys = SaveDirectoryTQITTransferStash;
				return sys is null ? Array.Empty<string>() : Directory.GetDirectories(sys);
			}
		}

		public static string SaveDirectoryTQITTransferStash
		{
			get
			{
				var p = $@"{PersonalFolderTQIT}\SaveData\Sys";
				return Directory.Exists(p) ? p : null;
			}
		}

		public static string DefaultSaveDirectory
		{
			get
			{
				string path = SaveDirectoryTQIT;
				if (path != null)
					return path;
				return SaveDirectoryTQ;
			}
		}

		public static string SaveDirectoryTQ
		{
			get
			{
				var p = $@"{PersonalFolderTQ}\SaveData\Main";
				return Directory.Exists(p) ? p : null;
			}
		}

		public static string SaveDirectoryTQIT
		{
			get
			{
				var p = $@"{PersonalFolderTQIT}\SaveData\Main";
				return Directory.Exists(p) ? p : null;
			}
		}


		/// <summary>
		/// Gets the Immortal Throne personal folder
		/// </summary>
		public static string PersonalFolderTQIT
		{
			get
			{
				return System.IO.Path.Combine(System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "My Games"), "Titan Quest - Immortal Throne");
			}
		}

		/// <summary>
		/// Gets the Titan Quest Character personnal folder.
		/// </summary>
		public static string PersonalFolderTQ
		{
			get
			{
				return System.IO.Path.Combine(System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "My Games"), "Titan Quest");
			}
		}

		#endregion
	}
}
