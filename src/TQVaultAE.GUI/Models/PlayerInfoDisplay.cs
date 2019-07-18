using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using TQVaultAE.GUI.Components;
using TQVaultAE.Presentation;
using TQVaultAE.Domain.Entities;
using TQVaultAE.Config;
using Microsoft.Extensions.DependencyInjection;

namespace TQVaultAE.GUI.Models
{
	/// <summary>
	/// Displays character information
	/// </summary>
	public class PlayerInfoDisplay
	{

		private double _startX = 0;
		private double _startY = 0;
		private Font _font;

		private readonly StringFormat _playerInfoAlignment;
		private readonly SolidBrush _whiteBrush = new SolidBrush(Color.White);
		private readonly SolidBrush _yellowGreenBrush = new SolidBrush(Color.YellowGreen);

		private RectangleF _editButton = new RectangleF(10, 100, 30, 20);
		private static Brush _editNoHighlight = new SolidBrush(Color.FromArgb(0x52, 0x38, 0x12));
		private Brush _editBckgrnd = _editNoHighlight;

		private readonly Pen _blackBorderPen = new Pen(Color.FromArgb(0xb8,0x8e,0x4b), 1);
		private readonly StringFormat _editTextAlignment;

		private readonly VaultPanel _stashPanel;
		private readonly IServiceProvider ServiceProvider;
		private readonly Settings _settings;

		internal PlayerInfoDisplay(IServiceProvider serviceProvider, Settings s, VaultPanel p, Font f, double x, double y)
		{
			this.ServiceProvider = serviceProvider;

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

		}

		/// <summary>
		/// updates font being used, needed if window is resized
		/// </summary>
		/// <param name="f"></param>
		public void UpdateFont(Font f) => _font = f;

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
				var dlg = this.ServiceProvider.GetService<CharacterEditDialog>();
				dlg.PlayerCollection = _stashPanel.Player;
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

			var startTextX = Convert.ToSingle(rect.Right * _startX);
			var startTextY = Convert.ToSingle(rect.Bottom * _startY);

			#region display character edit button

			if (_settings.AllowCharacterEdit)
			{
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

			#endregion

			printData(Resources.CurrentLevel, playerInfo.CurrentLevel);
			printData(Resources.Class, Resources.ResourceManager.GetString(playerInfo.Class));
			printData(Resources.CurrentXP, playerInfo.CurrentXP);
			printData(Resources.DifficultyUnlocked, GetDifficultyDisplayName(playerInfo.DifficultyUnlocked));
			printData(Resources.Money, playerInfo.Money);
			printData(Resources.SkillPoints, playerInfo.SkillPoints);
			printData(Resources.AttributesPoints, playerInfo.AttributesPoints);
			printData(Resources.BaseStrength, playerInfo.BaseStrength);
			printData(Resources.BaseDexterity, playerInfo.BaseDexterity);
			printData(Resources.BaseIntelligence, playerInfo.BaseIntelligence);
			printData(Resources.BaseHealth, playerInfo.BaseHealth);
			printData(Resources.BaseMana, playerInfo.BaseMana);
			printData(Resources.PlayTimeInSeconds, playerInfo.PlayTimeInSeconds);
			printData(Resources.NumberOfDeaths, playerInfo.NumberOfDeaths);
			printData(Resources.NumberOfKills, playerInfo.NumberOfKills);
			printData(Resources.ExperienceFromKills, playerInfo.ExperienceFromKills);
			printData(Resources.HealthPotionsUsed, playerInfo.HealthPotionsUsed);
			printData(Resources.ManaPotionsUsed, playerInfo.ManaPotionsUsed);
			printData(Resources.NumHitsReceived, playerInfo.NumHitsReceived);
			printData(Resources.NumHitsInflicted, playerInfo.NumHitsInflicted);
			printData(Resources.CriticalHitsInflicted, playerInfo.CriticalHitsInflicted);
			printData(Resources.CriticalHitsReceived, playerInfo.CriticalHitsReceived);

			void printData(string label, object data) // Local function
			{
				label = string.Format("{0}:", label);
				data = data ?? string.Empty;
				e.Graphics.DrawString(label, _font, _whiteBrush, startTextX, startTextY, _playerInfoAlignment);
				e.Graphics.DrawString(data.ToString(), _font, _yellowGreenBrush, startTextX, startTextY);
				startTextY = startTextY + _font.Height;
			}
		}

	}
}
