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
using TQVaultAE.GUI.Components;
using TQVaultAE.GUI.Models.SearchDialogAdvanced;
using TQVaultAE.Presentation;

namespace TQVaultAE.GUI.Tooltip
{
	public partial class BagButtonLabelTooltip : BaseTooltip
	{
		private static object syncObj = new object();

		private static BagButtonLabelTooltip _Current = null;
		private readonly Rectangle CurrentWorkingArea;

		internal Control AnchorControl { get; private set; }
		internal string Label { get; private set; }


#if DEBUG
		// For Design Mode
		public BagButtonLabelTooltip() => InitializeComponent();
#endif

		private BagButtonLabelTooltip(
			MainForm instance
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

			// Fill it outside of screen to avoid flickering
			this.Location = new Point(0, this.CurrentWorkingArea.Height);

			this.scalingLabel.Font = fontService.GetFont(15F, GraphicsUnit.Point);

			var bgimg = this.UIService.LoadBitmap(@"INGAMEUI\HEALTHMANAOVERLAY01_NEW.TEX");
			this.Size = new Size(bgimg.Width, bgimg.Height);
			this.BackgroundImage = bgimg;
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

		public static BagButtonLabelTooltip ShowTooltip(IServiceProvider serviceProvider, BagButtonBase anchorControl)
		{
			var label = anchorControl.Sack?.BagButtonIconInfo?.Label;
			if (anchorControl is null) return null;
			if (string.IsNullOrWhiteSpace(label)) return null;

			BagButtonLabelTooltip newTT;
			lock (syncObj)
			{
				HideTooltip();
				newTT = new BagButtonLabelTooltip(
					serviceProvider.GetService<MainForm>()
					, serviceProvider.GetService<IItemProvider>()
					, serviceProvider.GetService<IFontService>()
					, serviceProvider.GetService<IUIService>()
					, serviceProvider.GetService<ITranslationService>()
				)
				{
					AnchorControl = anchorControl,
					Label = label
				};
				_Current = newTT;
				newTT.Show();
			}
			return newTT;
		}

		#endregion

		private void ItemTooltip_Load(object sender, EventArgs e)
		{
			this.SuspendLayout();

			this.scalingLabel.Text = this.Label;

			// Adjust size to text
			var txtsize = TextRenderer.MeasureText(this.Label, this.scalingLabel.Font);// May vary with font

			if (txtsize.Width > this.Width)
				this.Width = txtsize.Width + 25;// +20 for marging

			if (txtsize.Height > this.Height - 25)
				this.Height = txtsize.Height + 20;// +20 for marging

			// Move it on top of anchor
			var loc = this.AnchorControl.PointToScreen(Point.Empty);

			loc.Y -= this.Size.Height + 5;

			var halfbutton = this.AnchorControl.Size.Width / 2;

			loc.X += halfbutton;
			loc.X -= this.Size.Width / 2;

			this.Location = loc;

			this.ResumeLayout();
		}
	}
}
