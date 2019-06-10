using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml.Linq;
using TQVaultAE.DAL;
using TQVaultAE.GUI.Components;
using TQVaultAE.GUI.Properties;

using TQVaultData;

namespace TQVaultAE.GUI.Models
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
		private RectangleF _editButton = new RectangleF(10, 100, 30, 20);
		private static Brush _editNoHighlight = new SolidBrush(Color.FromArgb(0x52, 0x38, 0x12));
		private Brush _editBckgrnd = _editNoHighlight;

		private Pen _blackBorderPen = new Pen(Color.FromArgb(0xb8,0x8e,0x4b), 1);
		private StringFormat _editTextAlignment;

		private VaultPanel _stashPanel;

		private Settings _settings;

		internal PlayerInfoDisplay(Settings s, VaultPanel p, Font f, double x, double y)
		{
			_settings = s;
			_stashPanel = p;
			_font = f;
			_blackBorderPen.Alignment = PenAlignment.Inset; //<-
			_blackBorderPen.StartCap = _blackBorderPen.EndCap = LineCap.Round;
			_startX = x;
			_startY = y;
			_playerInfoAlignment = new StringFormat();
			_playerInfoAlignment.Alignment = StringAlignment.Far;//right justify

			_editTextAlignment = new StringFormat();
			_editTextAlignment.Alignment = StringAlignment.Center;
			_editTextAlignment.LineAlignment = StringAlignment.Center;


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
					return (Resources.Difficulty0);
				case 1:
					return (Resources.Difficulty1);
				case 2:
					return (Resources.Difficulty2);
				default:
					return ("unknown");
			}
		}

		private static Point ConvertMousePoint(Control sender, Panel p, Point location)
		{
			var equipmentPanelPoint = ((Control)sender).PointToScreen(location);
			return(p.PointToClient(equipmentPanelPoint));
		}

		public void MouseClick(Panel p, object sender, MouseEventArgs e)
		{
			if (e.Button != MouseButtons.Left) return;

			var stashPanelPoint = ConvertMousePoint((Control)sender, p, e.Location);

			if (_editButton.Contains(stashPanelPoint))
			{
				var dlg = new CharacterEditDialog(_stashPanel.Player);
				dlg.ShowDialog();
				p.Invalidate();
			}
		}

		public void MouseMove(Panel p, object sender, MouseEventArgs e)
		{
			if (e == null)
			{
				if (_editBckgrnd != _editNoHighlight)
				{
					_editBckgrnd = _editNoHighlight;
					p.Invalidate();
				}
				return;
			}
			var stashPanelPoint = ConvertMousePoint((Control)sender, p, e.Location);

			if (_editButton.Contains(stashPanelPoint))
			{
				if (_editBckgrnd != Brushes.Red)
				{
					_editBckgrnd = Brushes.Red;
					p.Invalidate();
				}
				return;
			}
			if (_editBckgrnd != _editNoHighlight)
			{
				_editBckgrnd = _editNoHighlight;
				p.Invalidate();
			}
		}


		/// <summary>
		/// creates a rounded edge rect
		/// </summary>
		/// <param name="bounds"></param>
		/// <param name="radius"></param>
		/// <returns></returns>
		public static GraphicsPath RoundedRect(RectangleF bounds, int radius)
		{
			var diameter = Convert.ToSingle(radius * 2);
			var size = new SizeF(diameter, diameter);
			var arc = new RectangleF(bounds.Location, size);
			var path = new GraphicsPath();

			if (radius == 0)
			{
				path.AddRectangle(bounds);
				return path;
			}

			// top left arc  
			path.AddArc(arc, 180, 90);

			// top right arc  
			arc.X = bounds.Right - diameter;
			path.AddArc(arc, 270, 90);

			// bottom right arc  
			arc.Y = bounds.Bottom - diameter;
			path.AddArc(arc, 0, 90);

			// bottom left arc 
			arc.X = bounds.Left;
			path.AddArc(arc, 90, 90);

			path.CloseFigure();
			return path;
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

			//test(e, rect);

			var startTextX = Convert.ToSingle(rect.Right * _startX);
			var startTextY = Convert.ToSingle(rect.Bottom * _startY);

			if (_settings.AllowCharacterEdit)
			{
				//display character edit button
				var editSize = e.Graphics.MeasureString(Resources.CharacterEditBtn, _font);
				_editButton.X = startTextX;
				_editButton.Y = startTextY;
				_editButton.Height = editSize.Height + 5;
				_editButton.Width = editSize.Width + 5;

				using (var path = RoundedRect(_editButton, 4))
				{
					e.Graphics.FillPath(_editBckgrnd, path);
					e.Graphics.DrawPath(_blackBorderPen, path);
					e.Graphics.DrawString(Resources.CharacterEditBtn, _font, Brushes.White, _editButton, _editTextAlignment);
				}

				startTextY = startTextY + _editButton.Height + 3;
			}

			//converts playerinfo to xml so it can be bound to Resource data file CharacterInfoDispaly.txt
			//Order of data displayed is controlled by order of the CharacterInfoDispaly.txt resource file.
			var playerXml = playerInfo.ToXElement<PlayerInfo>();

			foreach (var labelKey in _labelKey.Keys)
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
