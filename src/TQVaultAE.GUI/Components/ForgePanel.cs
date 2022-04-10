using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TQVaultAE.Domain.Contracts.Services;
using TQVaultAE.Domain.Entities;

namespace TQVaultAE.GUI.Components
{
	public partial class ForgePanel : UserControl
	{
		private readonly IUIService UIService;
		private readonly IFontService FontService;
		internal Action CancelAction;
		internal Action ForgeAction;
		internal Item BaseItem;
		internal Item PrefixItem;
		internal Item SuffixItem;
		internal Item Relic1Item;
		internal Item Relic2Item;

		public ForgePanel()
		{
			InitializeComponent();
		}

		public ForgePanel(IUIService uIService, IFontService fontService)
		{
			InitializeComponent();

			this.UIService = uIService;
			this.FontService = fontService;

#if DEBUG
			//this.BorderStyle = BorderStyle.FixedSingle;
			//this.tableLayoutPanelForge.BorderStyle = BorderStyle.FixedSingle;
			//this.tableLayoutPanelForge.CellBorderStyle = TableLayoutPanelCellBorderStyle.Single;
#endif

			this.scalingLabelBaseItem.Font =
			this.scalingLabelPrefix.Font =
			this.scalingLabelRelic1.Font =
			this.scalingLabelRelic2.Font =
			this.scalingLabelSuffix.Font =
				FontService.GetFontLight(11F, UIService.Scale);

			// Scalling
			var size = TextRenderer.MeasureText(this.scalingLabelBaseItem.Text, this.scalingLabelBaseItem.Font);
			this.scalingLabelBaseItem.Height =
			this.scalingLabelPrefix.Height =
			this.scalingLabelRelic1.Height =
			this.scalingLabelRelic2.Height =
			this.scalingLabelSuffix.Height = size.Height;

			this.ForgeButton.Font = FontService.GetFontLight(12F);
			this.CancelButton.Font = FontService.GetFontLight(12F);

		}

		private void CancelButton_Click(object sender, EventArgs e)
		{
			if (this.CancelAction is not null)
				this.CancelAction();
		}

		private void ForgeButton_Click(object sender, EventArgs e)
		{
			// Merge item properties into Base item

			if (this.ForgeAction is not null)
				this.ForgeAction();
		}
	}
}
