using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using TQVaultAE.Entities;
using TQVaultAE.Entities.Results;
using TQVaultAE.GUI.Components;
using TQVaultAE.Presentation;
using TQVaultAE.Services;

namespace TQVaultAE.GUI.Tooltip
{
	public partial class BagButtonTooltip : Form
	{
		private static Dictionary<BagButtonBase, BagButtonTooltip> ItemTooltipOpened = new Dictionary<BagButtonBase, BagButtonTooltip>();
		private static Dictionary<(SackCollection Sack, float Scale), Bitmap> ToImage = new Dictionary<(SackCollection, float), Bitmap>();
		private ItemService ItemService;
		public BagButtonBase ButtonSack;

		public BagButtonTooltip(MainForm instance, BagButtonBase button, ItemService itemService)
		{
			InitializeComponent();
			this.Owner = instance;
			ItemService = itemService;
			ButtonSack = button;

			SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint, true);

			var wa = Screen.FromControl(this).WorkingArea;

			// Fill it outside of screen to avoid flickering
			this.Location = new Point(0, wa.Height);
		}

		public static void InvalidateCache(params SackCollection[] sack)
		{
			var cacheentrytoremove = ToImage.Where(c => sack.Contains(c.Key.Sack)).Select(c => c.Key).ToList();
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

		public static BagButtonTooltip ShowTooltip(MainForm instance, BagButtonBase button)
		{
			BagButtonTooltip _Current;
			lock (ToImage)
			{
				HideTooltip();
				_Current = new BagButtonTooltip(instance, button, new ItemService(MainForm.userContext));
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
			var key = (this.ButtonSack.Sack, UIService.UI.Scale);

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
			{
				AddRow(Resources.VaultGroupBoxEmpty, ItemGfxHelper.Color(ItemStyle.Broken));
			}
			else
			{

				var itemlist = ButtonSack.Sack
					//.Where(i => !ButtonSack.Excluded.Contains(i))// skip the item being dragged ! No need anymore
					.Where(i => i.BaseItemId.Length != 0)// skip empty items
					.ToArray();
				var friendlylist = itemlist
					.Select(i => ItemService.GetFriendlyNames(i))
					.OrderByDescending(d => d.FullNameBagTooltip.Length)
					.GroupBy(d => d.FullNameBagTooltip)
					.Select(g => new
					{
						FullName = g.Key.InsertAfterColorPrefix($"{g.Count()} x "),
						Data = g.First()
					})
					.ToArray();

				foreach (var item in friendlylist)
					AddRow(item.FullName, ItemGfxHelper.GetColorTag(item.Data.Item, item.Data.BaseItemInfoDescription));

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

		private void AddRow(ToFriendlyNameResult infos) => AddRow(infos.FullNameBagTooltip, ItemGfxHelper.GetColorTag(infos.Item, infos.BaseItemInfoDescription));
		private void AddRow(string friendlyName, Color color)
		{
			Control row = ItemTooltip.MakeRow(ref friendlyName, color, 10F, FontStyle.Regular, BGColor: this.flowLayoutPanelFriendlyNames.BackColor);

			this.flowLayoutPanelFriendlyNames.Controls.Add(row);
		}

		private void BagButtonTooltip_Load(object sender, EventArgs e)
		{
			this.FillSackToolTip();

			var loc = this.ButtonSack.PointToScreen(Point.Empty);
			loc.Y += this.ButtonSack.Size.Height;
			this.Location = loc;
		}

	}
}
