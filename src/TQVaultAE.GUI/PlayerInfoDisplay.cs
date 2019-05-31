using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using TQVaultAE.GUI.Properties;

using TQVaultData;

namespace TQVaultAE.GUI
{
	/// <summary>
	/// Displays character information
	/// </summary>
	public class PlayerInfoDisplay
	{

		private double _startX = 0;
		private double _startY = 0;
		private Font _font;
		private StringFormat _playerInfoAlignment;
		private SolidBrush _whiteBrush = new SolidBrush(Color.White);
		private SolidBrush _yellowGreenBrush = new SolidBrush(Color.YellowGreen);


		public PlayerInfoDisplay(Font f, double x, double y)
		{
			_font = f;
			_startX = x;
			_startY = y;
			_playerInfoAlignment = new StringFormat();
			_playerInfoAlignment.Alignment = StringAlignment.Far;//right justify

			// load titan quest class names, should be language specific.
			PlayerClass.LoadClassDataFile(Resources.CharacterClass);

		}


		/// <summary>
		/// updates font being used, needed if window is resized
		/// </summary>
		/// <param name="f"></param>
		public void UpdateFont(Font f)
		{
			_font = f;
		}

		/// <summary>
		/// Display name for character unlocked difficulty
		/// </summary>
		/// <param name="d">Difficulty number</param>
		/// <returns>Display name</returns>
		private string GetDifficultyDisplayName(int d)
		{

			switch (d)
			{
				case 0:
					return ("Normal");
				case 1:
					return ("Epic");
				case 2:
					return ("Legendary");
				default:
					return ("unknown");
			}
		}


		/// <summary>
		/// draws character information to rect area
		/// </summary>
		/// <param name="e">paint event</param>
		/// <param name="rect">rect area to draw</param>
		/// <param name="playerInfo">character information to display</param>
		public void DisplayPlayerInfo(PaintEventArgs e, Rectangle rect, PlayerInfo playerInfo)
		{
			if (playerInfo == null) return;

			var startTextX = Convert.ToSingle(rect.Right * _startX);
			var startTextY = Convert.ToSingle(rect.Bottom * _startY);

			printData(e, "Current Level:", string.Format("{0}", playerInfo.CurrentLevel), startTextX, startTextY);

			startTextY = startTextY + _font.Height;//start new line
			printData(e, "XP:", string.Format("{0}", playerInfo.CurrentXP), startTextX, startTextY);

			

			startTextY = startTextY + _font.Height;
			printData(e, "Class:", string.Format("{0}", PlayerClass.GetClassDisplayName(playerInfo.Class)), startTextX, startTextY);

			startTextY = startTextY + _font.Height;
			printData(e, "Difficulty:", string.Format("{0}", GetDifficultyDisplayName(playerInfo.DifficultyUnlocked)), startTextX, startTextY);

			startTextY = startTextY + _font.Height;
			printData(e, "Skill Points:", string.Format("{0}", playerInfo.SkillPoints), startTextX, startTextY);

			startTextY = startTextY + _font.Height;
			printData(e, "Attribute Points:", string.Format("{0}", playerInfo.AttributesPoints), startTextX, startTextY);

			startTextY = startTextY + _font.Height;
			printData(e, "Base Str:", string.Format("{0}", playerInfo.BaseStrength), startTextX, startTextY);

			startTextY = startTextY + _font.Height;
			printData(e, "Base Dex:", string.Format("{0}", playerInfo.BaseDexterity), startTextX, startTextY);

			startTextY = startTextY + _font.Height;
			printData(e, "Base Int:", string.Format("{0}", playerInfo.BaseIntelligence), startTextX, startTextY);

			startTextY = startTextY + _font.Height;
			printData(e, "Base Health:", string.Format("{0}", playerInfo.BaseHealth), startTextX, startTextY);

			startTextY = startTextY + _font.Height;
			printData(e, "Base Mana:", string.Format("{0}", playerInfo.BaseMana), startTextX, startTextY);

			startTextY = startTextY + _font.Height;
			printData(e, "Play Time:", string.Format("{0}", playerInfo.PlayTimeInSeconds), startTextX, startTextY);

			startTextY = startTextY + _font.Height;
			printData(e, "Deaths:", string.Format("{0}", playerInfo.NumberOfDeaths), startTextX, startTextY);

			startTextY = startTextY + _font.Height;
			printData(e, "Kills:", string.Format("{0}", playerInfo.NumberOfKills), startTextX, startTextY);

			startTextY = startTextY + _font.Height;
			printData(e, "XP From Kills:", string.Format("{0}", playerInfo.ExperienceFromKills), startTextX, startTextY);

			startTextY = startTextY + _font.Height;
			printData(e, "Health Pots Used:", string.Format("{0}", playerInfo.HealthPotionsUsed), startTextX, startTextY);

			startTextY = startTextY + _font.Height;
			printData(e, "Energy Pots Used:", string.Format("{0}", playerInfo.ManaPotionsUsed), startTextX, startTextY);

			startTextY = startTextY + _font.Height;
			printData(e, "Hits Recv:", string.Format("{0}", playerInfo.NumHitsReceived), startTextX, startTextY);

			startTextY = startTextY + _font.Height;
			printData(e, "Hits Inflicted:", string.Format("{0}", playerInfo.NumHitsInflicted), startTextX, startTextY);

			//startTextY = startTextY + _font.Height;
			//printData(e, "Greatest Dmg:", string.Format("{0}", playerInfo.GreatestDamageInflicted), startTextX, startTextY);

			//startTextY = startTextY + _font.Height;
			//printData(e, "Monster:", string.Format("{0}", playerInfo.GreatestMonster), startTextX, startTextY);

			startTextY = startTextY + _font.Height;
			printData(e, "Crit Hits:", string.Format("{0}", playerInfo.CriticalHitsInflicted), startTextX, startTextY);

			startTextY = startTextY + _font.Height;
			printData(e, "Crit Hits Recv:", string.Format("{0}", playerInfo.CriticalHitsReceived), startTextX, startTextY);
		}

		private void printData(PaintEventArgs e, string label, string data, float x, float y)
		{
			e.Graphics.DrawString(label, _font, _whiteBrush, x, y, _playerInfoAlignment);
			e.Graphics.DrawString(data, _font, _yellowGreenBrush, x, y);
		}


	}
}
