using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Windows.Forms;

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

		private RectangleF _editButton = new RectangleF(10, 100, 30, 20);
		private static Brush _editNoHighlight = new SolidBrush(Color.FromArgb(0x52, 0x38, 0x12));
		private Brush _editBckgrnd = _editNoHighlight;

		private Pen _blackBorderPen = new Pen(Color.FromArgb(0xb8,0x8e,0x4b), 1);
		private StringFormat _editTextAlignment;
		private int Xx;
		private int Yy;

		public PlayerInfoDisplay(Font f, double x, double y)
		{
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

		public void MouseClick(Panel p, object sender, MouseEventArgs e)
		{
			var equipmentPanelPoint = ((Control)sender).PointToScreen(e.Location);
			var stashPanelPoint = p.PointToClient(equipmentPanelPoint);

			if (_editButton.Contains(stashPanelPoint))
			{
				var dlg = new CharacterEditDialog();
				dlg.ShowDialog();
			}
		}

		public void MouseMove(Panel p, object sender, MouseEventArgs e)
		{
			var equipmentPanelPoint = ((Control)sender).PointToScreen(e.Location);
			var stashPanelPoint = p.PointToClient(equipmentPanelPoint); 

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

			var editSize = e.Graphics.MeasureString("Edit", _font);
			_editButton.X = startTextX;
			_editButton.Y = startTextY;
			_editButton.Height = editSize.Height + 5;
			_editButton.Width = editSize.Width + 5;
			//e.Graphics.FillRectangle(_yellowGreenBrush, _editButton);

			using (var path = RoundedRect(_editButton, 4))
			{
				e.Graphics.FillPath(_editBckgrnd, path);
				e.Graphics.DrawPath(_blackBorderPen, path);
				e.Graphics.DrawString("Edit", _font, Brushes.White, _editButton, _editTextAlignment);
			}

			startTextY = startTextY + _editButton.Height + 3;

			printData(e, "Current Level:", string.Format("{0}", playerInfo.CurrentLevel), startTextX, startTextY);

			startTextY = startTextY + _font.Height;//start new line
			printData(e, "XP:", string.Format("{0}", playerInfo.CurrentXP), startTextX, startTextY);

			startTextY = startTextY + _font.Height;
			printData(e, "Class:", string.Format("{0}", playerInfo.Class), startTextX, startTextY);

			startTextY = startTextY + _font.Height;
			printData(e, "Difficulty:", string.Format("{0}", GetDifficultyDisplayName(playerInfo.DifficultyUnlocked)), startTextX, startTextY);

			startTextY = startTextY + _font.Height;
			printData(e, "Skill Points:", string.Format("{0}", playerInfo.SkillPoints), startTextX, startTextY);

			startTextY = startTextY + _font.Height;
			printData(e, "Attribute Pts:", string.Format("{0}", playerInfo.AttributesPoints), startTextX, startTextY);

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

			startTextY = startTextY + _font.Height;
			printData(e, "Xx:", string.Format("{0}", Xx), startTextX, startTextY);
			startTextY = startTextY + _font.Height;
			printData(e, "Yx:", string.Format("{0}", Yy), startTextX, startTextY);
		}

		private void printData(PaintEventArgs e, string label, string data, float x, float y)
		{
			e.Graphics.DrawString(label, _font, _whiteBrush, x, y, _playerInfoAlignment);
			e.Graphics.DrawString(data, _font, _yellowGreenBrush, x, y);
		}


	}
}
