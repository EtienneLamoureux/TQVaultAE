using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.IO;
using TQVaultAE.Logging;

namespace TQVaultAE.DAL
{
	static class IniFile
	{
		private static readonly log4net.ILog Log = Logger.Get(typeof(IniFile));

		private static String INI_FILENAME = Directory.GetCurrentDirectory() + "\\TQVaultAE.ini";

		[DllImport("kernel32", CharSet = CharSet.Unicode)]
		private static extern int GetPrivateProfileString(string section, string key,
			string defaultValue, StringBuilder value, int size, string filePath);

		[DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
		static extern int GetPrivateProfileString(string section, string key, string defaultValue,
			[In, Out] char[] value, int size, string filePath);

		[DllImport("kernel32.dll", CharSet = CharSet.Auto)]
		private static extern int GetPrivateProfileSection(string section, IntPtr keyValue,
			int size, string filePath);

		[DllImport("kernel32", CharSet = CharSet.Unicode, SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool WritePrivateProfileString(string section, string key,
			string value, string filePath);

		public static int capacity = 65535;

		private static Dictionary<string, Dictionary<string, string>> defaultValues = new Dictionary<string, Dictionary<string, string>>
			{
				{
					"Main", new Dictionary<string,string>() {
						{ "ShowEditingCopyFeatures", "Y" },
						{ "ForceGamePath", "" },
						{ "Mod", "" },
					}
				}
			};

		static IniFile()
		{
			try
			{
				if (!File.Exists(IniFile.INI_FILENAME))
				{
					//create file
					FileStream ini = File.OpenWrite(INI_FILENAME);

					using (var sw = new StreamWriter(ini))
					{
						sw.WriteLine("# Created by TQVaultAE");
						sw.WriteLine("# Use Y and N for booleans");
						sw.WriteLine("# Don't use any kind of quotes ");
					}
					ini.Close();
				}

				//add default properties
				foreach (var section in defaultValues.Keys)
				{
					foreach (var prop in defaultValues[section])
					{
						if (String.IsNullOrEmpty(readValue(section, prop.Key, null)))
						{
							writeValue(section, prop.Key, prop.Value);
						}
					}
				}
			}
			catch (Exception ex)
			{
				Log.Error("INI file creation failed !", ex);
			}
		}

		public static string[] readKeys(string section)
		{
			// first line will not recognize if ini file is saved in UTF-8 with BOM 
			while (true)
			{
				char[] chars = new char[capacity];
				int size = GetPrivateProfileString(section, null, "", chars, capacity, IniFile.INI_FILENAME);

				if (size == 0)
				{
					return null;
				}

				if (size < capacity - 2)
				{
					string result = new String(chars, 0, size);
					string[] keys = result.Split(new char[] { '\0' }, StringSplitOptions.RemoveEmptyEntries);
					return keys;
				}

				capacity = capacity * 2;
			}
		}

		private static string[] readKeyValuePairs(string section)
		{
			while (true)
			{
				IntPtr returnedString = Marshal.AllocCoTaskMem(capacity * sizeof(char));
				int size = GetPrivateProfileSection(section, returnedString, capacity, IniFile.INI_FILENAME);

				if (size == 0)
				{
					Marshal.FreeCoTaskMem(returnedString);
					return null;
				}

				if (size < capacity - 2)
				{
					string result = Marshal.PtrToStringAuto(returnedString, size - 1);
					Marshal.FreeCoTaskMem(returnedString);
					string[] keyValuePairs = result.Split('\0');
					return keyValuePairs;
				}

				Marshal.FreeCoTaskMem(returnedString);
				capacity = capacity * 2;
			}
		}

		private static string readValue(string section, string key, string defaultValue = "")
		{
			var value = new StringBuilder(capacity);
			GetPrivateProfileString(section, key, defaultValue, value, value.Capacity, IniFile.INI_FILENAME);
			return value.ToString();
		}

		public static string[] readSections()
		{
			// first line will not recognize if ini file is saved in UTF-8 with BOM 
			while (true)
			{
				char[] chars = new char[capacity];
				int size = GetPrivateProfileString(null, null, "", chars, capacity, IniFile.INI_FILENAME);

				if (size == 0)
				{
					return null;
				}

				if (size < capacity - 2)
				{
					string result = new String(chars, 0, size);
					string[] sections = result.Split(new char[] { '\0' }, StringSplitOptions.RemoveEmptyEntries);
					return sections;
				}

				capacity = capacity * 2;
			}
		}

		private static bool writeValue(string section, string key, string value)
		{
			bool result = WritePrivateProfileString(section, key, value, IniFile.INI_FILENAME);
			return result;
		}

		public static bool deleteSection(string section)
		{
			bool result = WritePrivateProfileString(section, null, null, IniFile.INI_FILENAME);
			return result;
		}

		public static bool deleteKey(string section, string key)
		{
			bool result = WritePrivateProfileString(section, key, null, IniFile.INI_FILENAME);
			return result;
		}

		public static bool getBool(string section, string key)
		{
			string v = readValue(section, key);
			if (string.IsNullOrEmpty(v))
			{
				v = defaultValues[section][key];
			}
			if (v.ToUpperInvariant().StartsWith("Y") || v.StartsWith("1"))
			{
				return true;
			}
			return false;
		}

		public static string getString(string section, string key)
		{
			string v = readValue(section, key);
			if (string.IsNullOrEmpty(v))
			{
				return defaultValues[section][key];
			}

			return v;
		}

		public static void setBool(string section, string key, bool value)
		{
			writeValue(section, key, value ? "Y" : "N");
		}

		public static void setString(string section, string key, string value)
		{
			writeValue(section, key, value);
		}

	}
}
