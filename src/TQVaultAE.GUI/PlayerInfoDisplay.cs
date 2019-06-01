using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml.Linq;
using TQVaultAE.GUI.Properties;

using TQVaultData;

namespace TQVaultAE.GUI
{
	/// <summary>
	/// Displays character information
	/// </summary>
	public class PlayerInfoDisplay
	{

		private struct LabelData
		{
			public string Text;
			public int Handler;
		}

		private double _startX = 0;
		private double _startY = 0;
		private Font _font;
		private StringFormat _playerInfoAlignment;
		private SolidBrush _whiteBrush = new SolidBrush(Color.White);
		private SolidBrush _yellowGreenBrush = new SolidBrush(Color.YellowGreen);

		static Dictionary<string, LabelData> _labelKey = new Dictionary<string, LabelData>
		{
		};



		public PlayerInfoDisplay(Font f, double x, double y)
		{
			_font = f;
			_startX = x;
			_startY = y;
			_playerInfoAlignment = new StringFormat();
			_playerInfoAlignment.Alignment = StringAlignment.Far;//right justify

			// load titan quest class names, should be language specific.
			PlayerClass.LoadClassDataFile(Resources.CharacterClass);
			// load labels used to display character information, should be language specific
			LoadCharacterLabelFile(Resources.CharacterInfoDisplay);

		}


		private static void LoadCharacterLabelFile(string fileContents)
		{
			using (var sr = new StringReader(fileContents))
			{
				var data = sr.ReadLine();
				while (data != null)
				{
					var content = data.Split('=');
					if (content != null && content.Length > 1)
					{
						if (!_labelKey.ContainsKey(content[0]))
						{
							switch (content[0].ToUpper()) {
								case "GREATESTDAMAGEINFLICTED":
								case "GREATESTMONSTER":
								case "MAXLEVEL":
									//ignore for now
									break;
								case "CLASS":
									_labelKey.Add(
										content[0],
										new LabelData() { Text = content[1], Handler = 2 }
									); ;
									break;
								case "DIFFICULTYUNLOCKED":
									_labelKey.Add(
										content[0],
										new LabelData() { Text = content[1], Handler = 1 }
									); ;
									break;
								default:
									_labelKey.Add(
										content[0],
										new LabelData() { Text = content[1], Handler=0 }
									); ;
									break;
						    }
						}
					}
					data = sr.ReadLine();
				}
			}
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

			//serialize to xml so each property of PlayerInfo can be retrieved using label key
			//the text of the label should be language specific
			var playerXml = playerInfo.ToXElement<PlayerInfo>();

			foreach(var labelKey in _labelKey.Keys)
			{
				var elm = playerXml.Element(labelKey);
				if (elm != null)
				{
					var label = _labelKey[labelKey];
					var value = "";
					switch (label.Handler)
					{
						case 1:
							value = string.Format("{0}", GetDifficultyDisplayName(int.Parse(elm.Value)));
							break;
						case 2:
							value = string.Format("{0}", PlayerClass.GetClassDisplayName(elm.Value));
							break;
						default:
							value = string.Format("{0}", elm.Value);
							break;
					}
					//label.Text should be language specific
					printData(e, string.Format("{0}:", label.Text), value, startTextX, startTextY);
					startTextY = startTextY + _font.Height;
				}
			}
		}

		private void printData(PaintEventArgs e, string label, string data, float x, float y)
		{
			e.Graphics.DrawString(label, _font, _whiteBrush, x, y, _playerInfoAlignment);
			e.Graphics.DrawString(data, _font, _yellowGreenBrush, x, y);
		}


	}
}
