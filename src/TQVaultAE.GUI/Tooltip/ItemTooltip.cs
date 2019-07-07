﻿using System;
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

	public partial class ItemTooltip : Form
	{
		private static Dictionary<Item, ItemTooltip> ItemTooltipOpened = new Dictionary<Item, ItemTooltip>();
		private static Dictionary<(Item, float), (Bitmap Bmp, ToFriendlyNameResult Data)> ToImage = new Dictionary<(Item, float), (Bitmap, ToFriendlyNameResult)>();
		private Item FocusedItem;
		private ItemService ItemService;

		private SackPanel SackPanel;
		private ResultsDialog ResultsDialog;

		internal ToFriendlyNameResult Data { get; private set; }

		private ItemTooltip(MainForm instance, Item focusedItem, ItemService itemService, SackPanel sackPanel = null, ResultsDialog resultsDialog = null)
		{
			InitializeComponent();
			this.Owner = instance;
			FocusedItem = focusedItem;
			ItemService = itemService;

			SackPanel = sackPanel;
			ResultsDialog = resultsDialog;

			SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint, true);

			var wa = Screen.FromControl(this).WorkingArea;

			// Fill it outside of screen to avoid flickering
			this.Location = new Point(0, wa.Height);
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

		#region Factory

		public static ItemTooltip ShowTooltip(MainForm instance, Item focusedItem, SackPanel sackPanel)
		{
			ItemTooltip _Current;
			lock (ToImage)
			{
				HideTooltip();
				_Current = new ItemTooltip(instance, focusedItem, new ItemService(MainForm.userContext), sackPanel);
				ItemTooltipOpened.Add(focusedItem, _Current);
				_Current.Show();
			}
			return _Current;
		}

		public static ItemTooltip ShowTooltip(MainForm instance, Item focusedItem, ResultsDialog resultsDialog)
		{
			ItemTooltip _Current;
			lock (ToImage)
			{
				HideTooltip();
				_Current = new ItemTooltip(instance, focusedItem, new ItemService(MainForm.userContext), resultsDialog: resultsDialog);
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
			var key = (this.FocusedItem, UIService.UI.Scale);

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

			this.Data = this.ItemService.GetFriendlyNames(FocusedItem, FriendlyNamesExtraScopes.ItemFullDisplay);

			// Fullname
			AddRow(Data.FullName, FocusedItem.GetColorTag(Data.BaseItemInfoDescription), style: FontStyle.Bold);

			// Artifact Level
			if (Data.Item.IsArtifact)
				AddRow(Data.ArtifactClass);

			// Relic Completion
			if (Data.Item.IsRelic)
				AddRow(Data.RelicCompletionFormat, FocusedItem.GetColorTag(Data.BaseItemInfoDescription));

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
			foreach (var str in Data.PrefixAttributes) AddRow(str);
			foreach (var str in Data.BaseAttributes) AddRow(str);
			foreach (var str in Data.SuffixAttributes) AddRow(str);

			// formula Artifact Details goes after formula attributes
			if (Data.FormulaeArtifactAttributes.Any())
			{
				AddRow(TOOLTIPSPACER);
				AddRow(Data.FormulaeArtifactName, FocusedItem.GetColorTag(Data.BaseItemInfoDescription), style: FontStyle.Bold);
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
				AddRow(Item.ItemAtlantis, TQColor.Green.Color());
			// Add the Ragnarok clause
			else if (Data.Item.IsRagnarok)
				AddRow(Item.ItemRagnarok, TQColor.Green.Color());
			// Add the Immortal Throne clause
			else if (Data.Item.IsImmortalThrone)
				AddRow(Item.ItemIT, TQColor.Green.Color());

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

		internal const string TOOLTIPDELIM = @"TOOLTIPDELIM";
		internal const string TOOLTIPSPACER = @"TOOLTIPSPACER";
		private void AddRow(string friendlyName = TOOLTIPSPACER, Color? FGColor = null, float fontSize = 10F, FontStyle style = FontStyle.Regular, Color? BGColor = null)
		{
			Control row = MakeRow(ref friendlyName, FGColor, fontSize, style, BGColor: this.flowLayoutPanelFriendlyNames.BackColor);

			this.flowLayoutPanelFriendlyNames.Controls.Add(row);
		}

		internal static Control MakeRow(ref string friendlyName, Color? FGColor, float fontSize, FontStyle style, Color? BGColor)
		{
			friendlyName = friendlyName ?? string.Empty;
			Control row = null;
			if (friendlyName == TOOLTIPSPACER)
			{
				row = new Label()
				{
					Text = " ",
					Font = FontHelper.GetFontAlbertusMTLight(fontSize, style, UIService.UI.Scale),
					AutoSize = true,
				};
			}
			else if (friendlyName == TOOLTIPDELIM)
			{
				row = new Label()
				{
					Text = string.Empty,
					BackColor = TQColor.DarkGray.Color(),
					BorderStyle = BorderStyle.FixedSingle,
					Height = 3,
					Anchor = AnchorStyles.Left | AnchorStyles.Right,
					Margin = new Padding(2, 3, 2, 3),
				};
			}
			else
			{
				// If there is a color tag in the middle
				if (friendlyName.LastIndexOf('{') > 0)
				{
					var multiColors = friendlyName.Split('{').Where(t => !string.IsNullOrEmpty(t)).ToArray();
					if (multiColors.Count() > 1)
					{
						row = new FlowLayoutPanel()
						{
							AutoSize = true,
							AutoSizeMode = AutoSizeMode.GrowOnly,
							FlowDirection = FlowDirection.LeftToRight,
							Padding = new Padding(0),
							Anchor = AnchorStyles.Left,

							BorderStyle = BorderStyle.None,
							Margin = new Padding(0),
							BackColor = BGColor.Value,
						};
						row.SuspendLayout();
						foreach (var coloredSegment in multiColors)
						{
							// IsColorTagged
							if (coloredSegment.First() == '^')
							{
								var segTxt = '{' + coloredSegment;
								row.Controls.Add(MakeSingleColorLabel(segTxt, FGColor, fontSize, style, BGColor));
							}
							else
								row.Controls.Add(MakeSingleColorLabel(coloredSegment, FGColor, fontSize, style, BGColor));
						}
						row.ResumeLayout();
					}
				}
				else
					row = MakeSingleColorLabel(friendlyName, FGColor, fontSize, style, BGColor);
			}

			return row;
		}

		internal static Label MakeSingleColorLabel(string friendlyName, Color? FGColor, float fontSize, FontStyle style, Color? BGColor = null)
		{
			// Single Color
			FGColor = TQColorHelper.GetColorFromTaggedString(friendlyName)?.Color() ?? FGColor ?? TQColor.White.Color();// Color Tag take précédence
			var txt = TQColorHelper.RemoveLeadingColorTag(friendlyName);
			var row = new Label()
			{
				Text = txt,
				ForeColor = FGColor.Value,
				Font = FontHelper.GetFontAlbertusMTLight(fontSize, style, UIService.UI.Scale),
				AutoSize = true,
				Anchor = AnchorStyles.Left,
				BackColor = BGColor.Value,

				//BackColor = Color.Red,
				BorderStyle = BorderStyle.None,// BorderStyle.FixedSingle
				Margin = new Padding(0),
			};

			if (BGColor.HasValue) row.BackColor = BGColor.Value;

			return row;
		}

		private void ItemTooltip_Load(object sender, EventArgs e)
		{
			var usize = UIService.UI.ItemUnitSize;

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
