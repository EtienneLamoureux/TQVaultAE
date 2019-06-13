using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TQVaultAE.DAL
{
	public static class IniProperties
	{
		private static bool? mShowEditingCopyFeatures = null;

		private static string mMod;

		private static string mForceGamePath;

		public static string GamePath
		{
			get
			{
				if (String.IsNullOrEmpty(mForceGamePath))
				{
					mForceGamePath = IniFile.getString("Main", "ForceGamePath");
				}
				return mForceGamePath;
			}
			set
			{
				mForceGamePath = value;
				IniFile.setString("Main", "ForceGamePath", value);
			}
		}

		public static string Mod
		{
			get
			{
				if (String.IsNullOrEmpty(mMod))
				{
					mMod = IniFile.getString("Main", "Mod");
				}
				return mMod;
			}
			set
			{
				mMod = value;
				IniFile.setString("Main", "mMod", value);
			}
		}

		public static bool ShowEditingCopyFeatures
		{
			get
			{
				if (mShowEditingCopyFeatures == null)
				{
					mShowEditingCopyFeatures = IniFile.getBool("Main", "ShowEditingCopyFeatures");
				}
				return (bool)mShowEditingCopyFeatures;
			}
			set
			{
				mShowEditingCopyFeatures = value;
				IniFile.setBool("Main", "showEditingCopyFeatures", value);
			}
		}
	}
}
