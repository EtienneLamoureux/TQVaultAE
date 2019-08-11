using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using TQVaultAE.Domain.Contracts.Services;
using TQVaultAE.Domain.Entities;
using TQVaultAE.Domain.Helpers;
using TQVaultAE.GUI.Components;
using TQVaultAE.Presentation;
using TQVaultAE.Services;
using Microsoft.Extensions.DependencyInjection;
using TQVaultAE.Domain.Contracts.Providers;

namespace TQVaultAE.GUI.Tooltip
{
	public partial class BagButtonTooltip : BaseTooltip
	{
		private static Dictionary<BagButtonBase, BagButtonTooltip> ItemTooltipOpened = new Dictionary<BagButtonBase, BagButtonTooltip>();
		private static Dictionary<(SackCollection Sack, float Scale), Bitmap> ToImage = new Dictionary<(SackCollection, float), Bitmap>();

		public BagButtonBase ButtonSack;

		private Rectangle CurrentWorkingArea;
		private int RightSide;
		private int LeftSide;

#if DEBUG
		// For Design Mode
		public BagButtonTooltip() => InitializeComponent();
#endif

		private BagButtonTooltip(MainForm instance, IItemProvider itemProvider, IFontService fontService, IUIService uiService, ITranslationService translationService) : base(itemProvider, fontService, uiService, translationService)
		{
			InitializeComponent();

			this.Owner = instance;

			SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint, true);

			this.CurrentWorkingArea = Screen.FromControl(this).WorkingArea;
			// Tooltip can goes multi-columns (especially for bloated Relic Vault)
			this.flowLayoutPanelFriendlyNames.MaximumSize = new Size(this.CurrentWorkingArea.Width, this.CurrentWorkingArea.Height);

			// Fill it outside of screen to avoid flickering
			this.Location = new Point(0, this.CurrentWorkingArea.Height);
		}

		// to avoid Mainform lost focus with this.TopMost = false
		protected override bool ShowWithoutActivation => true;

		public static void InvalidateCache(params SackCollection[] sack)
		{
			sack = sack.Where(s => s != null).ToArray();
			var cacheentrytoremove = ToImage.Where(c => sack.Contains(c.Key.Sack)).Select(c => c.Key).ToList();
			cacheentrytoremove.ForEach(c => ToImage.Remove(c));
		}

		public static void InvalidateCache(params Item[] items)
		{
			items = items.Where(i => i != null).ToArray();
			var cacheentrytoremove = ToImage
				.Where(c => c.Key.Sack.Intersect(items).Any())
				.Select(c => c.Key)
				.ToList();
			cacheentrytoremove.ForEach(c => ToImage.Remove(c));
		}

		public static void HideTooltip()
		{
			lock (ToImage)
			{
				var lst = ItemTooltipOpened.Where(f => f.Value.Visible).ToList();
				lst.Select(f => f.Value).ToList().ForEach(form => form.Close());
				lst.Select(f => f.Key).ToList().ForEach(key => ItemTooltipOpened.Remove(key));
			}
		}

		public static BagButtonTooltip ShowTooltip(IServiceProvider serviceProvider, BagButtonBase button)
		{
			BagButtonTooltip _Current;
			if (button?.Sack is null) return null;
			lock (ToImage)
			{
				HideTooltip();
				_Current = new BagButtonTooltip(
					serviceProvider.GetService<MainForm>()
					, serviceProvider.GetService<IItemProvider>()
					, serviceProvider.GetService<IFontService>()
					, serviceProvider.GetService<IUIService>()
					, serviceProvider.GetService<ITranslationService>()
				)
				{
					ButtonSack = button
				};
				ItemTooltipOpened.Add(button, _Current);
				_Current.Show();
			}
			return _Current;
		}


		/// <summary>
		/// Init the tooltip content for a sack.  Summarizes the items within the sack
		/// </summary>
		/// <param name="button">button the mouse is over.  Corresponds to a sack</param>
		/// <returns>string to be displayed in the tooltip</returns>
		public void FillSackToolTip()
		{
			var key = (this.ButtonSack.Sack, UIService.Scale);

			// Redraw
			if (ToImage.ContainsKey(key))
			{
				this.Controls.Clear();
				this.Padding = new Padding(0);
				this.Controls.Add(new PictureBox()
				{
					Image = ToImage[key],
					SizeMode = PictureBoxSizeMode.AutoSize,
					Dock = DockStyle.Fill,
				});
				return;
			}

			this.SuspendLayout();

			this.flowLayoutPanelFriendlyNames.SuspendLayout();

			if (ButtonSack.Sack.IsEmpty)
				AddRow(Resources.VaultGroupBoxEmpty, ItemStyle.Broken.Color());
			else
			{

				var itemlist = ButtonSack.Sack
					.Where(i => i.BaseItemId.Length != 0)// skip empty items
					.ToArray();
				var friendlylist = itemlist
					.Select(i => ItemProvider.GetFriendlyNames(i))
					.OrderBy(d => d.FullNameBagTooltipClean)
					.GroupBy(d => d.FullNameBagTooltip)
					.Select(g => new
					{
						FullName = g.Key.InsertAfterColorPrefix($"{g.Count()} x "),
						Data = g.First()
					})
					.ToArray();

				foreach (var item in friendlylist)
					AddRow(item.FullName, item.Data.Item.GetColor(item.Data.BaseItemInfoDescription));

			}

			this.flowLayoutPanelFriendlyNames.ResumeLayout();
			this.ResumeLayout();

			// Rasterize flowLayoutPanelFriendlyNames and cache it for fast redraw
			var w = this.flowLayoutPanelFriendlyNames.Width;
			var h = this.flowLayoutPanelFriendlyNames.Height;
			var raster = new Bitmap(w, h);
			this.flowLayoutPanelFriendlyNames.DrawToBitmap(raster, new Rectangle(0, 0, w, h));
			ToImage[key] = raster;
		}

		private void AddRow(string friendlyName, Color color)
		{
			Control row = MakeRow(this.UIService, this.FontService, friendlyName, color, 10F, FontStyle.Regular, BGColor: this.flowLayoutPanelFriendlyNames.BackColor);

			this.flowLayoutPanelFriendlyNames.Controls.Add(row);
		}

		private void BagButtonTooltip_Load(object sender, EventArgs e)
		{
			this.FillSackToolTip();

			// Move it under BagButton
			var loc = this.ButtonSack.PointToScreen(Point.Empty);
			loc.Y += this.ButtonSack.Size.Height;

			// Ajust position if tooltip size goes offscreen
			var bottom = loc.Y + this.Height;
			if (bottom > this.CurrentWorkingArea.Height)
			{
				// Maximize vertical view
				var offScreenHeight = bottom - this.CurrentWorkingArea.Height;
				if (loc.Y - offScreenHeight < 0)
					loc.Y = 0;// Do your best
				else
					loc.Y -= offScreenHeight;

				this.LeftSide = loc.X - this.Width;

				// Put tooltip on right side of button to avoid mouse pointer overlap
				loc.X += Convert.ToInt32(this.ButtonSack.Size.Width * UIService.Scale);

				this.RightSide = loc.X;

				// Capture mouse move when overing the button to allow dynamic tooltip placement on left<->right
				this.ButtonSack.MouseMove += ButtonSack_MouseMove;
			}

			this.Location = loc;
		}

		private void ButtonSack_MouseMove(object sender, MouseEventArgs e)
		{
			var loc = this.Location;

			if (e.Location.X > this.ButtonSack.Width / 2)
				loc.X = this.RightSide;
			else
				loc.X = this.LeftSide;

			this.Location = loc;
		}

		private void BagButtonTooltip_FormClosing(object sender, FormClosingEventArgs e) => this.ButtonSack.MouseMove -= ButtonSack_MouseMove;
	}
}
