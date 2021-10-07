using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using TQVaultAE.Domain.Contracts.Services;
using TQVaultAE.Domain.Entities;
using TQVaultAE.Domain.Helpers;
using TQVaultAE.Presentation;
using Microsoft.Extensions.DependencyInjection;
using TQVaultAE.Domain.Contracts.Providers;
using TQVaultAE.Domain.Search;

namespace TQVaultAE.GUI.Tooltip
{
	public partial class FoundResultsTooltip : BaseTooltip
	{
		private static Dictionary<(string ResultCollectionHash, float Scale), Bitmap> ToImage = new Dictionary<(string, float), Bitmap>();

		private Rectangle CurrentWorkingArea;
		private int RightSide;
		private int LeftSide;
		private static FoundResultsTooltip _Current = null;
		public Control AnchorControl { get; private set; }
		public IEnumerable<Result> ResultsToDisplay { get; private set; }

		// to avoid Mainform lost focus with this.TopMost = false
		protected override bool ShowWithoutActivation => true;

#if DEBUG
		// For Design Mode
		public FoundResultsTooltip() => InitializeComponent();
#endif

		private FoundResultsTooltip(SearchDialogAdvanced instance, IItemProvider itemProvider, IFontService fontService, IUIService uiService, ITranslationService translationService) : base(itemProvider, fontService, uiService, translationService)
		{
			InitializeComponent();

			this.Owner = instance;

			SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint, true);

			this.CurrentWorkingArea = Screen.FromControl(this).WorkingArea;

			// Tooltip can goes multi-columns
			this.flowLayoutPanelFriendlyNames.MaximumSize = new Size(this.CurrentWorkingArea.Width, this.CurrentWorkingArea.Height);

			// Fill it outside of screen to avoid flickering
			this.Location = new Point(0, this.CurrentWorkingArea.Height);
		}


		public static void InvalidateCache(params IEnumerable<Result>[] resultlistCollection)
		{
			var hashlisttoremove = resultlistCollection.Where(s => s?.Any() ?? false).Select(l => MakeHash(l));

			var cacheentrytoremove = ToImage
				.Where(c => hashlisttoremove.Contains(c.Key.ResultCollectionHash)).Select(c => c.Key)
				.ToList();

			cacheentrytoremove.ForEach(c => ToImage.Remove(c));
		}

		private static string MakeHash(IEnumerable<Result> list)
		{
			var input = string.Join(string.Empty, list.Select(bi => bi.IdString).ToArray());
			return input.MakeMD5();
		}

		public static void HideTooltip()
		{
			lock (ToImage)
			{
				if (_Current != null) _Current.Close();
				_Current = null;
			}
		}

		public static FoundResultsTooltip ShowTooltip(IServiceProvider serviceProvider, Control anchorControl, IEnumerable<Result> results)
		{
			if (anchorControl is null) return null;
			if (!(results?.Any() ?? false)) return null;

			FoundResultsTooltip newTT;
			lock (ToImage)
			{
				HideTooltip();
				newTT = new FoundResultsTooltip(
					serviceProvider.GetService<SearchDialogAdvanced>()
					, serviceProvider.GetService<IItemProvider>()
					, serviceProvider.GetService<IFontService>()
					, serviceProvider.GetService<IUIService>()
					, serviceProvider.GetService<ITranslationService>()
				)
				{
					AnchorControl = anchorControl,
					ResultsToDisplay = results,
				};
				_Current = newTT;
				newTT.Show();
			}
			return _Current;
		}


		/// <summary>
		/// Init the tooltip content for a sack.  Summarizes the items within the sack
		/// </summary>
		/// <param name="button">button the mouse is over.  Corresponds to a sack</param>
		/// <returns>string to be displayed in the tooltip</returns>
		public void FillToolTip()
		{
			// Tooltip is grotesque at some point & you need to fine tune your search
			if (ResultsToDisplay.Count() > 250)
				AddRow(Resources.SearchTooManyResultToDisplay, ItemStyle.Broken.Color());
			else
			{
				var key = (MakeHash(this.ResultsToDisplay), UIService.Scale);

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

				if (!ResultsToDisplay.Any())
					AddRow(Resources.VaultGroupBoxEmpty, ItemStyle.Broken.Color());
				else
				{

					var friendlylist = ResultsToDisplay
						.Select(i => i.FriendlyNames)
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
		}

		private void AddRow(string friendlyName, Color color)
		{
			Control row = MakeRow(this.UIService, this.FontService, friendlyName, color, 10F, FontStyle.Regular, BGColor: this.flowLayoutPanelFriendlyNames.BackColor);
			this.flowLayoutPanelFriendlyNames.Controls.Add(row);
		}

		private void ItemTooltip_Load(object sender, EventArgs e)
		{
			this.FillToolTip();

			// Move it under BagButton
			var loc = this.AnchorControl.PointToScreen(Point.Empty);
			loc.Y += this.AnchorControl.Size.Height;

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
				loc.X += Convert.ToInt32(this.AnchorControl.Size.Width * UIService.Scale);

				this.RightSide = loc.X;

				// Capture mouse move when overing the button to allow dynamic tooltip placement on left<->right
				this.AnchorControl.MouseMove += AnchorControl_MouseMove;
			}

			this.Location = loc;
		}

		private void AnchorControl_MouseMove(object sender, MouseEventArgs e)
		{
			var loc = this.Location;

			if (e.Location.X > this.AnchorControl.Width / 2)
				loc.X = this.RightSide;
			else
				loc.X = this.LeftSide;

			this.Location = loc;
		}

		private void ItemTooltip_FormClosing(object sender, FormClosingEventArgs e) => this.AnchorControl.MouseMove -= AnchorControl_MouseMove;
	}
}
