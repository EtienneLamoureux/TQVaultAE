using Microsoft.Extensions.DependencyInjection;
using System;
using System.Drawing;
using System.Windows.Forms;
using TQVaultAE.Domain.Contracts.Providers;
using TQVaultAE.Domain.Contracts.Services;
using TQVaultAE.GUI.Components;

namespace TQVaultAE.GUI.Tooltip
{
	public partial class BagButtonLabelTooltip : BaseTooltip
	{
		private static object syncObj = new object();

		private static BagButtonLabelTooltip _Current = null;
		private string _Label;
		private Control _AnchorControl;
		private readonly Rectangle CurrentWorkingArea;
		private readonly Bitmap bgimg;

		internal Control AnchorControl
		{
			set
			{
				_AnchorControl = value;

				// Move it on top of anchor
				var loc = this._AnchorControl.PointToScreen(Point.Empty);

				loc.Y -= this.Size.Height + 5;

				var halfbutton = this._AnchorControl.Size.Width / 2;

				loc.X += halfbutton;
				loc.X -= this.Size.Width / 2;

				this.Location = loc;
			}
		}

		internal string Label
		{
			set
			{
				_Label = value;

				this.scalingLabel.Text = _Label;

				// Adjust size to text
				var txtsize = TextRenderer.MeasureText(this._Label, this.scalingLabel.Font);// May vary with font

				if (txtsize.Width > this.Width)
					this.Width = txtsize.Width + 25;// +20 for marging

				if (txtsize.Height > this.Height - 25)
					this.Height = txtsize.Height + 20;// +20 for marging

			}
		}


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

			this.scalingLabel.Font = fontService.GetFont(15F, GraphicsUnit.Point);

			this.bgimg = this.UIService.LoadBitmap(@"INGAMEUI\HEALTHMANAOVERLAY01_NEW.TEX");

			this.Size = new Size(bgimg.Width, bgimg.Height);
			this.BackgroundImage = bgimg;
		}

		// to avoid Mainform lost focus with this.TopMost = false
		protected override bool ShowWithoutActivation => true;

		public static void HideTooltip()
		{
			lock (syncObj)
			{
				if (_Current is not null)
					_Current.Hide();
			}
		}

		#region Factory

		public static BagButtonLabelTooltip ShowTooltip(IServiceProvider serviceProvider, BagButtonBase anchorControl)
		{
			var label = anchorControl.Sack?.BagButtonIconInfo?.Label;
			if (anchorControl is null) return null;
			if (string.IsNullOrWhiteSpace(label)) return null;

			lock (syncObj)
			{
				HideTooltip();
				if (_Current is null)
					_Current = new BagButtonLabelTooltip(
						serviceProvider.GetService<MainForm>()
						, serviceProvider.GetService<IItemProvider>()
						, serviceProvider.GetService<IFontService>()
						, serviceProvider.GetService<IUIService>()
						, serviceProvider.GetService<ITranslationService>()
					);
				_Current.AnchorControl = anchorControl;
				_Current.Label = label;
				_Current.Show();
			}
			return _Current;
		}

		#endregion


	}
}
