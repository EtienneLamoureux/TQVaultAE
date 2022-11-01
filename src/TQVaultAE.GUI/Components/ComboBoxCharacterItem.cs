using SixLabors.Shapes;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Windows.Forms;
using TQVaultAE.Domain.Contracts.Providers;
using TQVaultAE.Domain.Contracts.Services;
using TQVaultAE.Domain.Entities;
using TQVaultAE.Domain.Helpers;
using TQVaultAE.GUI.Helpers;

namespace TQVaultAE.GUI.Components
{
	public partial class ComboBoxCharacterItem : UserControl
	{
		/// <summary>
		/// Default Height for this control
		/// </summary>
		internal const int BASE_HEIGHT = 60;

		private ComboBoxCharacterDropDown DropDown;
		private IDatabase Database;
		private ITagService TagService;
		private ITranslationService TranslationService;
		private IFontService FontService;

		/// <summary>
		/// a <see cref="PlayerSave"/> or a <see cref="string"/>
		/// </summary>
		public object Item { get; private set; }

		public ComboBoxCharacterItem()
		{
			InitializeComponent();

			this.scalingLabelTags.Text = $"{Presentation.Resources.GlobalTags} : ";
			this.scalingLabelArchived.Text = Presentation.Resources.GlobalArchived;

			this.ProcessAllControls(ctr =>
			{
				ctr.Click += Ctr_Click;
				ctr.MouseEnter += Ctr_MouseEnter;
				ctr.MouseLeave += Ctr_MouseLeave;
			});
		}

		private void Ctr_MouseLeave(object sender, EventArgs e)
		{
			this.BackColor = Color.White;
		}

		private void Ctr_MouseEnter(object sender, EventArgs e)
		{
			this.BackColor = SystemColors.MenuHighlight;
		}

		public event EventHandler SelectedItemChanged;
		private void Ctr_Click(object sender, EventArgs e)
		{
			SelectedItemChanged?.Invoke(this, EventArgs.Empty);
		}

		public void Init(ComboBoxCharacterDropDown dropDown, object item, ITranslationService translationService, IFontService fontService, IDatabase database, ITagService tagService)
		{
			this.Item = item;
			this.TranslationService = translationService;
			this.FontService = fontService;
			this.DropDown = dropDown;
			this.Database = database;
			this.TagService = tagService;

			this.scalingLabelCharName.Font = FontService.GetFont(this.scalingLabelCharName.Font.Size);
			this.scalingLabelPlayerHeader.Font =
			this.scalingLabelLevel.Font =
			this.scalingLabelCLassName.Font = FontService.GetFont(this.scalingLabelCLassName.Font.Size);
			this.scalingLabelTags.Font =
			this.scalingLabelMasteries.Font =
			this.scalingLabelModName.Font =
			this.scalingLabelArchived.Font = FontService.GetFontLight(this.scalingLabelArchived.Font.Size);

			this.scalingLabelArchived.Visible =
			this.bufferedFlowLayoutPanelTags.Visible =
			this.bufferedFlowLayoutPanelSubTitle.Visible =
			this.scalingLabelModName.Visible =
			this.scalingLabelArchived.Visible = false;

			this.scalingLabelCharName.Text = 
			this.scalingLabelPlayerHeader.Text =
			this.scalingLabelLevel.Text =
			this.scalingLabelCLassName.Text = string.Empty;
		}

		public void RefreshContent()
		{
			this.Invoke(_RefreshContent);
		}

		private void _RefreshContent()
		{
			if (this.Item is PlayerSave ps)
			{
				this.scalingLabelCharName.Text = ps.Name;
				this.scalingLabelPlayerHeader.Text = ps.Info?.HeaderVersion.ToString();

				this.scalingLabelLevel.Text = ps.Info?.CurrentLevel is null
					? string.Empty
					: $"{TranslationService.TranslateXTag("tagMenuImport05")} : {ps.Info.CurrentLevel}";

				this.scalingLabelCLassName.Text = ps.Info?.Class is null
					? string.Empty
					: TranslationService.TranslateXTag(ps.Info.Class, true, true);

				// Masteries
				this.scalingLabelMasteries.Text = string.Empty;
				List<string> masteryNames = new();
				var dbr = ps.Info?.ActiveMasteriesRecordNames;
				if (dbr?.Any() ?? false)
				{
					foreach (var recId in dbr)
					{
						var masteryInfo = this.Database.GetInfo(recId);
						var masteryName = this.TranslationService.TranslateXTag(masteryInfo.DescriptionTag);
						masteryNames.Add(masteryName);
					}
					this.scalingLabelMasteries.Text = masteryNames.JoinString(" + ");
					this.bufferedFlowLayoutPanelSubTitle.Visible = this.scalingLabelMasteries.Visible = true;
				}

				// Mod
				// TODO - How to link a character Save with a mod ?

				#region Tags

				this.scalingLabelArchived.Visible =
				this.bufferedFlowLayoutPanelTags.Visible = false;

				// Is Archived
				this.scalingLabelArchived.Visible = ps.IsArchived;

				this.ClearTagList();

				this.TagService.LoadTags(ps);

				if (ps.Tags.Any())
				{
					// refresh assigned tag list
					int idx = 0;
					foreach (var tag in ps.Tags)
					{
						var ctr = new ScalingLabel()
						{
							Name = ComboBoxCharacter.TAGKEY + idx++,
							Text = tag.Key,
							BackColor = tag.Value,
							Font = FontService.GetFontLight(this.scalingLabelArchived.Font.Size),
							AutoSize = true,
						};
						ctr.MouseEnter += Ctr_MouseEnter;
						ctr.MouseLeave += Ctr_MouseLeave;
						this.bufferedFlowLayoutPanelTags.Controls.Add(ctr);
					}
				}

				if (ps.Tags.Any() || ps.IsArchived)
					this.scalingLabelTags.Visible = this.bufferedFlowLayoutPanelTags.Visible = true;

				#endregion
			}
			else if (this.Item is string str)
			{
				this.scalingLabelCharName.Text = str;
				this.scalingLabelPlayerHeader.Text =
				this.scalingLabelLevel.Text =
				this.scalingLabelCLassName.Text = string.Empty;
			}
		}

		private void ClearTagList()
		{
			this.bufferedFlowLayoutPanelTags.Controls.OfType<ScalingLabel>()
				.Where(ctr => ctr.Name.StartsWith(ComboBoxCharacter.TAGKEY))
				.ToList().ForEach(ctr => this.bufferedFlowLayoutPanelTags.Controls.Remove(ctr));
		}
	}
}
