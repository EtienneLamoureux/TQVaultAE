using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using TQVaultAE.Domain.Contracts.Providers;
using TQVaultAE.Domain.Contracts.Services;
using TQVaultAE.Domain.Entities;
using TQVaultAE.Domain.Helpers;
using TQVaultAE.GUI.Models.SearchDialogAdvanced;
using TQVaultAE.Presentation;

namespace TQVaultAE.GUI.Tooltip
{

	public partial class SearchFiltersTooltip : BaseTooltip
	{
		private static object syncObj = new object();

		private static SearchFiltersTooltip _Current = null;
		private int LeftSide;
		private int RightSide;
		private readonly Rectangle CurrentWorkingArea;

		internal Control AnchorControl { get; private set; }
		internal List<BoxItem> Filters { get; private set; }
		internal SearchOperator Operator { get; private set; }


#if DEBUG
		// For Design Mode
		public SearchFiltersTooltip() => InitializeComponent();
#endif

		private SearchFiltersTooltip(
			SearchDialogAdvanced instance
			, IItemProvider itemProvider
			, IFontService fontService
			, IUIService uiService
			, ITranslationService translationService
		) : base(itemProvider, fontService, uiService, translationService)
		{
			this.Owner = instance;

			InitializeComponent();

			SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint, true);

			this.CurrentWorkingArea = Screen.FromControl(this).WorkingArea;
			// Tooltip can goes multi-columns
			this.flowLayoutPanelFriendlyNames.MaximumSize = new Size(this.CurrentWorkingArea.Width, this.CurrentWorkingArea.Height);

			// Fill it outside of screen to avoid flickering
			this.Location = new Point(0, this.CurrentWorkingArea.Height);
		}

		// to avoid Mainform lost focus with this.TopMost = false
		protected override bool ShowWithoutActivation => true;

		public static void HideTooltip()
		{
			lock (syncObj)
			{
				if (_Current != null) _Current.Close();
				_Current = null;
			}
		}

		#region Factory

		public static SearchFiltersTooltip ShowTooltip(IServiceProvider serviceProvider, Control anchorControl, List<BoxItem> filters, SearchOperator ope)
		{
			if (anchorControl is null) return null;
			if (!(filters?.Any() ?? false)) return null;

			SearchFiltersTooltip newTT;
			lock (syncObj)
			{
				HideTooltip();
				newTT = new SearchFiltersTooltip(
					serviceProvider.GetService<SearchDialogAdvanced>()
					, serviceProvider.GetService<IItemProvider>()
					, serviceProvider.GetService<IFontService>()
					, serviceProvider.GetService<IUIService>()
					, serviceProvider.GetService<ITranslationService>()
				)
				{
					AnchorControl = anchorControl,
					Filters = filters,
					Operator = ope
				};
				_Current = newTT;
				newTT.Show();
			}
			return newTT;
		}

		#endregion

		/// <summary>
		/// Init the tooltip content.
		/// </summary>
		public void FillToolTip()
		{
			this.SuspendLayout();
			this.flowLayoutPanelFriendlyNames.SuspendLayout();

			// Operator description
			var opeTranslate = this.Operator == SearchOperator.And ? Resources.SearchOperatorAnd : Resources.SearchOperatorOr;
			AddRow($"{Resources.SearchOperatorTitle} : {opeTranslate}", FGColor: TQColor.Aqua.Color(), style: FontStyle.Bold);
			if (this.Operator == SearchOperator.And)
				AddRow(Resources.SearchOperatorDescAnd);
			else
				AddRow(Resources.SearchOperatorDescOr);

			AddRow(TOOLTIPDELIM);

			// Filters descriptions

			var searchTermsGroup = // TODO Maybe multiple in the future using old notation
				from f in this.Filters
				where f.CheckedList is null
				let cleanName = Regex.Replace(f.Category.Text, @"[^\w]", string.Empty)
				group f by cleanName into grp
				orderby grp.Key
				select grp;

			if (searchTermsGroup.Any())
			{
				foreach (var term in searchTermsGroup)
				{
					AddRow(term.Key, FGColor: TQColor.Orange.Color(), style: FontStyle.Bold);
					foreach (var filter in term)
						AddRow(filter.DisplayValue, FGColor: TQColor.Green.Color(), style: FontStyle.Regular);
				}

				AddRow(TOOLTIPDELIM);
			}

			var catgroups =
				from f in this.Filters
				where f.CheckedList != null
				group f by f.Category into grp
				orderby grp.Key.Text
				select grp;

			var firstCategory = true;
			foreach (var category in catgroups)
			{
				if (firstCategory)
					firstCategory = false;
				else
					AddRow(TOOLTIPSPACER);

				AddRow(category.Key.Text, FGColor: category.Key.ForeColor, style: FontStyle.Bold);
				foreach (var filter in category)
					AddRow(filter.DisplayValue, FGColor: filter.CheckedList.ForeColor, style: FontStyle.Regular);
			}

			this.flowLayoutPanelFriendlyNames.ResumeLayout();
			this.ResumeLayout();
		}

		private void AddRow(string friendlyName = TOOLTIPSPACER, Color? FGColor = null, float fontSize = 10F, FontStyle style = FontStyle.Regular, Color? BGColor = null)
		{
			Control row = MakeRow(this.UIService, this.FontService, friendlyName, FGColor, fontSize, style, BGColor: this.flowLayoutPanelFriendlyNames.BackColor);
			this.flowLayoutPanelFriendlyNames.Controls.Add(row);
		}

		private void ItemTooltip_Load(object sender, EventArgs e)
		{
			this.FillToolTip();

			// Move it next to anchor
			var loc = this.AnchorControl.PointToScreen(Point.Empty);
			loc.Y += this.AnchorControl.Height;

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


		private void AnchorControl_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
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
