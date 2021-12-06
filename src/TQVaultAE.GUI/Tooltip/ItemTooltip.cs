using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Input;
using TQVaultAE.Domain.Contracts.Providers;
using TQVaultAE.Domain.Contracts.Services;
using TQVaultAE.Domain.Entities;
using TQVaultAE.Domain.Helpers;
using TQVaultAE.Domain.Results;
using TQVaultAE.GUI.Components;
using TQVaultAE.Presentation;

namespace TQVaultAE.GUI.Tooltip
{

    public partial class ItemTooltip : BaseTooltip
	{
		private static Dictionary<Item, ItemTooltip> ItemTooltipOpened = new Dictionary<Item, ItemTooltip>();
		private static Dictionary<(Item Item, float Scale, bool AltView), (Bitmap Bmp, ToFriendlyNameResult Data)> ToImage = new Dictionary<(Item, float, bool), (Bitmap, ToFriendlyNameResult)>();
		internal Item FocusedItem { get; set; }
		internal SackPanel SackPanel { get; set; }
		internal ResultsDialog ResultsDialog { get; set; }

		internal ToFriendlyNameResult Data { get; private set; }

#if DEBUG
		// For Design Mode
		public ItemTooltip() => InitializeComponent();
#endif

		private ItemTooltip(MainForm instance, IItemProvider itemProvider, IFontService fontService, IUIService uiService, ITranslationService translationService) : base(itemProvider, fontService, uiService, translationService)
		{
			InitializeComponent();

			this.Owner = instance;

			SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint, true);

			var wa = Screen.FromControl(this).WorkingArea;

			// Fill it outside of screen to avoid flickering
			this.Location = new Point(0, wa.Height);
		}

		public static void InvalidateCache(params Item[] items)
		{
			items = items.Where(i => i != null).ToArray();
			var cacheentrytoremove = ToImage
				.Where(c => items.Contains(c.Key.Item))
				.Select(c => c.Key)
				.ToList();
			cacheentrytoremove.ForEach(c => ToImage.Remove(c));
		}

		// to avoid Mainform lost focus with this.TopMost = false
		protected override bool ShowWithoutActivation => true;

		public static void HideTooltip()
		{
			lock (ToImage)
			{
				var lst = ItemTooltipOpened.Where(f => f.Value.Visible).ToList();
				lst.Select(f => f.Value).ToList().ForEach(form => form.Close());
				lst.Select(f => f.Key).ToList().ForEach(key => ItemTooltipOpened.Remove(key));
			}
		}

		#region Factory

		public static ItemTooltip ShowTooltip(IServiceProvider serviceProvider, Item focusedItem, SackPanel sackPanel)
		{
			ItemTooltip _Current;
			lock (ToImage)
			{
				HideTooltip();
				_Current = new ItemTooltip(
					serviceProvider.GetService<MainForm>()
					, serviceProvider.GetService<IItemProvider>()
					, serviceProvider.GetService<IFontService>()
					, serviceProvider.GetService<IUIService>()
					, serviceProvider.GetService<ITranslationService>()
				)
				{
					FocusedItem = focusedItem,
					SackPanel = sackPanel,
				};
				ItemTooltipOpened.Add(focusedItem, _Current);
				_Current.Show();
			}
			return _Current;
		}

		public static ItemTooltip ShowTooltip(IServiceProvider serviceProvider, Item focusedItem, ResultsDialog resultsDialog)
		{
			ItemTooltip _Current;
			lock (ToImage)
			{
				HideTooltip();
				_Current = new ItemTooltip(
					serviceProvider.GetService<MainForm>()
					, serviceProvider.GetService<IItemProvider>()
					, serviceProvider.GetService<IFontService>()
					, serviceProvider.GetService<IUIService>()
					, serviceProvider.GetService<ITranslationService>()
				)
				{
					FocusedItem = focusedItem,
					ResultsDialog = resultsDialog,
				};
				ItemTooltipOpened.Add(focusedItem, _Current);
				_Current.Show();
			}
			return _Current;
		}

		#endregion

		/// <summary>
		/// Init the tooltip content.
		/// </summary>
		public void FillToolTip()
		{
			var key = (this.FocusedItem, UIService.Scale, Config.Settings.Default.EnableDetailedTooltipView);

			// Redraw
			if (ToImage.ContainsKey(key))
			{
				this.Controls.Clear();
				this.Padding = new Padding(0);
				this.Data = ToImage[key].Data;
				this.Controls.Add(new PictureBox()
				{
					Image = ToImage[key].Bmp,
					SizeMode = PictureBoxSizeMode.AutoSize,
					Dock = DockStyle.Fill,
				});
				return;
			}

			this.SuspendLayout();
			this.flowLayoutPanelFriendlyNames.SuspendLayout();

			this.Data = this.ItemProvider.GetFriendlyNames(FocusedItem, FriendlyNamesExtraScopes.ItemFullDisplay);

			// Fullname
			AddRow(Data.FullName, FocusedItem.GetColor(Data.BaseItemInfoDescription), style: FontStyle.Bold);

			// Artifact Level
			if (Data.Item.IsArtifact)
				AddRow(Data.ArtifactClass);

			// Relic Completion
			if (Data.Item.IsRelic)
				AddRow(Data.RelicCompletionFormat, FocusedItem.GetColor(Data.BaseItemInfoDescription));

			// Recipe Label
			if (Data.Item.IsFormulae)
				AddRow(Data.ArtifactRecipe);

			// Flavor text
			if (Data.FlavorText.Any())
			{
				foreach (var str in Data.FlavorText) AddRow(str);
				AddRow(TOOLTIPSPACER);
			}

			// Required Reagents
			if (Data.Item.IsFormulae)
			{
				AddRow(TOOLTIPSPACER);
				AddRow(Data.FormulaeFormat);
			}

			// Attributes
			if (Config.Settings.Default.EnableDetailedTooltipView)
			{
				string AdjustColor(string TQText, string label)
				{
					TQText = TQText.InsertAfterColorPrefix($"{label} : ");
					return TQText.HasColorPrefix() ? TQText : $"{ItemStyle.Relic.TQColor().ColorTag()}{TQText}";
				}

				// Detailed display
				if (Data.PrefixAttributes.Any())
				{
					AddRow(TOOLTIPSPACER);
					AddRow(AdjustColor(this.Data.PrefixInfoDescription, Resources.ItemPropertiesLabelPrefixProperties), style: FontStyle.Bold);
					foreach (var str in Data.PrefixAttributes) AddRow(str);
				}
				if (Data.BaseAttributes.Any())
				{
					AddRow(TOOLTIPSPACER);
					AddRow(AdjustColor(this.Data.BaseItemInfoDescription, Resources.ItemPropertiesLabelBaseItemProperties), style: FontStyle.Bold);
					foreach (var str in Data.BaseAttributes) AddRow(str);
				}
				if (Data.SuffixAttributes.Any())
				{
					AddRow(TOOLTIPSPACER);
					AddRow(AdjustColor(this.Data.SuffixInfoDescription, Resources.ItemPropertiesLabelSuffixProperties), style: FontStyle.Bold);
					foreach (var str in Data.SuffixAttributes) AddRow(str);
				}
			}
			else
			{
				// Classic display
				foreach (var str in Data.BaseAttributes) AddRow(str);
				foreach (var str in Data.PrefixAttributes) AddRow(str);
				foreach (var str in Data.SuffixAttributes) AddRow(str);
			}

			// formula Artifact Details goes after formula attributes
			if (Data.FormulaeArtifactAttributes.Any())
			{
				AddRow(TOOLTIPSPACER);
				AddRow(Data.FormulaeArtifactName, FocusedItem.GetColor(Data.BaseItemInfoDescription), style: FontStyle.Bold);
				AddRow(Data.FormulaeArtifactClass, ItemStyle.Broken.TQColor().Color());
				foreach (var str in Data.FormulaeArtifactAttributes) AddRow(str);
			}

			// Relic attributes after items attributes with delimiter
			if (Data.Item.HasRelic)
			{
				if (Data.Relic1Attributes.Any())
				{
					AddRow(TOOLTIPDELIM);
					AddRow(Data.RelicInfo1Description, ItemStyle.Relic.TQColor().Color(), style: FontStyle.Bold);// Name
					AddRow(Data.RelicInfo1CompletionResolved, ItemStyle.Relic.TQColor().Color());// Completion label
					foreach (var str in Data.Relic1Attributes) AddRow(str);
					AddRow(Data.RelicInfo1CompletionBonusResolved, ItemStyle.Relic.TQColor().Color());// Bonus Completion label
					foreach (var str in Data.RelicBonus1Attributes) AddRow(str);
				}
				if (Data.Relic2Attributes.Any())
				{
					AddRow(TOOLTIPDELIM);
					AddRow(Data.RelicInfo2Description, ItemStyle.Relic.TQColor().Color(), style: FontStyle.Bold);// Name
					AddRow(Data.RelicInfo2CompletionResolved, ItemStyle.Relic.TQColor().Color());// Completion label
					foreach (var str in Data.Relic2Attributes) AddRow(str);
					AddRow(Data.RelicInfo2CompletionBonusResolved, ItemStyle.Relic.TQColor().Color());// Bonus Completion label
					foreach (var str in Data.RelicBonus2Attributes) AddRow(str);
				}
			}
			else if (Data.Item.IsArtifact)
			{
				if (Data.RelicBonus1Attributes.Any())
				{
					AddRow(TOOLTIPSPACER);
					AddRow(Data.ArtifactBonus, ItemStyle.Relic.TQColor().Color());// Completion label
					foreach (var str in Data.RelicBonus1Attributes) AddRow(str);
				}
			}
			else if (Data.Item.IsRelic)
			{
				if (Data.RelicBonus1Attributes.Any())
				{
					AddRow(TOOLTIPSPACER);
					AddRow(Data.RelicBonusTitle, ItemStyle.Relic.TQColor().Color());// Completion label
					foreach (var str in Data.RelicBonus1Attributes) AddRow(str);
				}
			}

			AddRow(TOOLTIPDELIM);

			// Add the item seed
			AddRow(Data.ItemSeed, ItemStyle.Broken.TQColor().Color());

			// Add the Atlantis clause
			if (Data.Item.IsAtlantis)
				AddRow(this.TranslationService.ItemAtlantis, TQColor.Green.Color());
			// Add the Eternal Embers clause
			else if (Data.Item.IsEmbers)
				AddRow(this.TranslationService.ItemEmbers, TQColor.Green.Color());
			// Add the Ragnarok clause
			else if (Data.Item.IsRagnarok)
				AddRow(this.TranslationService.ItemRagnarok, TQColor.Green.Color());
			// Add the Immortal Throne clause
			else if (Data.Item.IsImmortalThrone)
				AddRow(this.TranslationService.ItemIT, TQColor.Green.Color());

			// ItemSet
			if (Data.ItemSet.Any())
			{
				AddRow(TOOLTIPDELIM);
				foreach (var str in Data.ItemSet) AddRow(str);
			}

			if (Data.Requirements.Any())
			{
				AddRow(TOOLTIPDELIM);
				// Requierments
				foreach (var str in Data.Requirements) AddRow(str, ItemStyle.Broken.TQColor().Color());
			}

			this.flowLayoutPanelFriendlyNames.ResumeLayout();
			this.ResumeLayout();

			// Rasterize flowLayoutPanelFriendlyNames and cache it for fast redraw
			var w = this.flowLayoutPanelFriendlyNames.Width;
			var h = this.flowLayoutPanelFriendlyNames.Height;
			var raster = new Bitmap(w, h);
			this.flowLayoutPanelFriendlyNames.DrawToBitmap(raster, new Rectangle(0, 0, w, h));
			ToImage[key] = (raster, Data);
		}

		private void AddRow(string friendlyName = TOOLTIPSPACER, Color? FGColor = null, float fontSize = 10F, FontStyle style = FontStyle.Regular, Color? BGColor = null)
		{
			Control row = MakeRow(this.UIService, this.FontService, friendlyName, FGColor, fontSize, style, BGColor: this.flowLayoutPanelFriendlyNames.BackColor);

			this.flowLayoutPanelFriendlyNames.Controls.Add(row);
		}

		private void ItemTooltip_Load(object sender, EventArgs e)
		{
			var usize = UIService.ItemUnitSize;
			var wa = Screen.FromControl(this).WorkingArea;

			this.FillToolTip();

			// Move it to view area from Mainform
			if (this.SackPanel != null)
			{
				var loc = this.SackPanel.PointToScreen(Point.Empty);
				var x = loc.X + ((this.FocusedItem.PositionX + this.FocusedItem.Size.Width) * usize);
				var y = loc.Y + (this.FocusedItem.PositionY * usize);

				// Adjust for Equipment panel weapon slot
				if (this.SackPanel.SackType == SackType.Equipment)
				{
					if (this.FocusedItem.IsInWeaponSlot)
					{
						var itemX = SackCollection.GetWeaponLocationOffset(this.FocusedItem.PositionY).X;
						var itemY = SackCollection.GetWeaponLocationOffset(this.FocusedItem.PositionY).Y;
						x = loc.X + ((itemX + 2) * usize);
						y = loc.Y + (itemY * usize);
					}

					if (this.FocusedItem.IsAmulet)
						// Adjust for the whole size of the amulet box.
						x += (SackCollection.GetEquipmentLocationSize(1).Width - this.FocusedItem.Size.Width) * usize;
				}

				// Adjust position to avoid flickering if the tooltip appears on top of the cursor.
				x += Cursor.Size.Width;

				// Ajust position if tooltip size goes offscreen
				var bottom = y + this.Height;
				if (bottom > wa.Height)
				{
					// Maximize vertical view
					var offScreenHeight = bottom - wa.Height;
					if (y - offScreenHeight < 0)
						y = 0;// Do your best
					else
						y -= offScreenHeight;
				}

				this.Location = new Point(Convert.ToInt32(x), Convert.ToInt32(y));
				return;
			}

			// Move it to view area from Search form
			if (this.ResultsDialog != null)
			{
				var loc = this.ResultsDialog.PointToScreen(Point.Empty);
				var x = loc.X + this.ResultsDialog.Width;
				var y = loc.Y;

				this.Location = new Point(Convert.ToInt32(x), Convert.ToInt32(y));
			}
		}
	}
}
