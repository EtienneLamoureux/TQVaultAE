using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using TQVaultAE.Domain.Contracts.Services;
using TQVaultAE.GUI.Helpers;

using TQVaultAE.Presentation;
using System.Collections.ObjectModel;
using TQVaultAE.Domain.Entities;
using TQVaultAE.GUI.Components;
using System.Linq;
using System.IO;

namespace TQVaultAE.GUI
{
	public partial class BagButtonSettings : VaultForm
	{
		// DI
		private readonly ITranslationService TranslationService;
		private readonly IIconService iconService;
		private readonly ILogger<BagButtonSettings> Log;

		// Drag & Drop stuff
		private bool IsDragging = false;
		private Point DragPosition;

		// Form object model crossing simplification
		public readonly List<FlowLayoutPanel> PicsPanelList;
		public readonly List<GroupBox> GroupBoxList;

		// Remove duplicate
		List<string> alreadyAddedPics = new List<string>();

		// From Database
		private ReadOnlyCollection<IconInfo> Pictures;

		private BagButtonIconInfo _DefaultBagButtonIconInfo = new()
		{
			DisplayMode = BagButtonDisplayMode.Default
		};

		#region CurrentBagButton

		private BagButtonBase _CurrentBagButton;
		private bool IsLoaded;

		internal BagButtonBase CurrentBagButton
		{
			set
			{
				_CurrentBagButton = value;
				ApplySelectedBagPath();
			}
		}

		#endregion

#if DEBUG
		// For Design Mode
		public BagButtonSettings() => InitializeComponent();
#endif

		public BagButtonSettings(MainForm instance
			, ITranslationService translationService
			, IIconService iconService
			, ILogger<BagButtonSettings> log
		) : base(instance.ServiceProvider)
		{
			this.Owner = instance;
			this.TranslationService = translationService;
			this.iconService = iconService;
			this.Log = log;

			this.InitializeComponent();

			this.MinimizeBox = false;
			this.NormalizeBox = false;
			this.MaximizeBox = true;

			// Map [Esc] and [Enter]
			this.CancelButton = this.cancelButton;
			this.AcceptButton = this.applyButton;

			#region Translations

			this.Text = Resources.BagButtonSettingsTitle;

			// Selected button
			this.scalingLabelSelectedButton.Text = Resources.BagButtonSettingsSelectedButton;

			// Button properties
			this.groupBoxEdit.Text = Resources.BagButtonSettingsButtonProperties;
			this.scalingRadioButtonDefaultIcon.Text = Resources.BagButtonSettingsUseDefault;
			this.scalingRadioButtonCustomIcon.Text = Resources.BagButtonSettingsCustomIcon;
			this.scalingCheckBoxNumber.Text = Resources.BagButtonSettingsNumber;
			this.scalingCheckBoxLabel.Text = Resources.BagButtonSettingsLabel;

			// Selected pictures
			this.groupBoxSelectedPictures.Text = Resources.BagButtonSettingsSelectedPictures;
			this.radioButtonPictureSimple.Text = Resources.BagButtonSettingsModeSimple;
			this.radioButtonPictureDetailed.Text = Resources.BagButtonSettingsModeDetailed;
			this.groupBoxOffBitmap.Text = Resources.BagButtonSettingsOffBitmap;
			this.groupBoxOnBitmap.Text = Resources.BagButtonSettingsOnBitmap;
			this.groupBoxOverBitmap.Text = Resources.BagButtonSettingsOverBitmap;
			this.groupBoxSimpleImage.Text = Resources.BagButtonSettingsImageBitmap;

			// Available pictures
			this.groupBoxAvailablePictures.Text = Resources.BagButtonSettingsAvailablePictures;
			this.scalingLabelDragDropNotice.Text = Resources.BagButtonSettingsDragDropNotice;

			// Tabs
			this.tabPageSkills.Text = TranslationService.TranslateXTag("tagWindowName02");
			this.tabPageRelics.Text = TranslationService.TranslateXTag("tagTutorialTip18Title");
			this.tabPageArtifacts.Text = TranslationService.TranslateXTag("xtagTutorialTip05Title");
			this.tabPageJewellery.Text = Resources.GlobalJewellery;
			this.tabPageScrolls.Text = TranslationService.TranslateXTag("xtagTutorialTip06Title");
			this.tabPagePotions.Text = TranslationService.TranslateXTag("tagTutorialTip14Title");
			this.tabPageButtons.Text = Resources.GlobalButtons;
			this.tabPageMisc.Text = Resources.GlobalMiscellaneous;

			// Exit
			this.applyButton.Text = TranslationService.TranslateXTag("tagMenuButton07");
			this.cancelButton.Text = TranslationService.TranslateXTag("tagMenuButton06");

			#endregion

			#region Custom font & Scaling

			this.Font = FontService.GetFont(9F, FontStyle.Regular, GraphicsUnit.Point);

			var title = FontService.GetFont(12F, FontStyle.Regular, GraphicsUnit.Point);
			this.scalingLabelSelectedButton.Font = title;
			this.groupBoxEdit.Font = title;
			this.groupBoxSelectedPictures.Font = title;
			this.groupBoxAvailablePictures.Font = title;
			this.cancelButton.Font = title;
			this.applyButton.Font = title;

			var text = FontService.GetFontLight(11.25F, FontStyle.Regular, GraphicsUnit.Point);
			this.scalingLabelSelectedButtonValue.Font = text;
			this.groupBoxEdit.ProcessAllControls(ctr => ctr.Font = text);
			this.groupBoxSelectedPictures.ProcessAllControls(ctr => ctr.Font = text);
			this.groupBoxAvailablePictures.ProcessAllControls(ctr => ctr.Font = text);

			ScaleControl(this.UIService, this.cancelButton);
			ScaleControl(this.UIService, this.applyButton);

			#endregion

#if DEBUG
			this.scalingTextBoxDebug.Visible = true;
#endif

			this.PicsPanelList = new()
			{
				this.flowLayoutPanelPicsArtifacts,
				this.flowLayoutPanelPicsButtons,
				this.flowLayoutPanelPicsJewellery,
				this.flowLayoutPanelPicsMisc,
				this.flowLayoutPanelPicsPotions,
				this.flowLayoutPanelPicsRelics,
				this.flowLayoutPanelPicsScrolls,
				this.flowLayoutPanelPicsSkills,
			};

			this.GroupBoxList = new()
			{
				groupBoxOnBitmap,
				groupBoxOffBitmap,
				groupBoxOverBitmap,
				groupBoxSimpleImage
			};

		}

		private void ApplySelectedBagPath()
		{
			var vaultPanel = _CurrentBagButton.Parent as VaultPanel;
			this.scalingLabelSelectedButtonValue.Text = string.Format("{0} - {1} - ({2})"
				, vaultPanel.Player.PlayerName
				, _CurrentBagButton.ButtonNumber + 1
				, _CurrentBagButton.Sack?.BagButtonIconInfo?.Label
			);
		}

		private void BagButtonSettings_Load(object sender, EventArgs e)
		{
			this.Pictures = this.iconService.GetIconDatabase();// Singleton

			ApplyIconInfo();

			if (!IsLoaded)
			{

				TogglePicturePickup();

				this.PicsPanelList.ForEach(p => p.SuspendLayout());

				this.SuspendLayout();

				foreach (var pic in this.Pictures) AddPicture(pic);

				this.PicsPanelList.ForEach(p => p.ResumeLayout(false));
				this.PicsPanelList.ForEach(p => p.PerformLayout());

				DisplayPictures();

				this.ResumeLayout(false);
				this.PerformLayout();

				IsLoaded = true;
			}
		}

		/// <summary>
		/// Adjuste form display at first load
		/// </summary>
		private void ApplyIconInfo()
		{
			var info = _CurrentBagButton.Sack.BagButtonIconInfo;

			if (info is null)
				info = _DefaultBagButtonIconInfo;


			this.scalingTextBoxLabel.Text = info.Label;

			if (info.DisplayMode.HasFlag(BagButtonDisplayMode.CustomIcon))
				this.scalingRadioButtonCustomIcon.Checked = true;
			if (info.DisplayMode.HasFlag(BagButtonDisplayMode.Label))
				this.scalingCheckBoxLabel.Checked = true;
			if (info.DisplayMode.HasFlag(BagButtonDisplayMode.Number))
				this.scalingCheckBoxNumber.Checked = true;

			if (info.DisplayMode == BagButtonDisplayMode.Default)
				this.scalingRadioButtonDefaultIcon.Checked = true;

			// Adjust Simple pickup or Detailed => Goes Detailed if non Default, user can change pickup view afterward
			if (!this.scalingRadioButtonDefaultIcon.Checked)
			{
				// Apply previously saved icons
				var prevOn = this.Pictures.Where(p => p.Own(info.On)).FirstOrDefault();
				var prevOff = this.Pictures.Where(p => p.Own(info.Off)).FirstOrDefault();
				var prevOver = this.Pictures.Where(p => p.Own(info.Over)).FirstOrDefault();

				if (prevOn is not null)
				{
					this.pictureBoxOn.Image = prevOn.OnBitmap;
					this.pictureBoxOn.Tag = prevOn;
				}

				if (prevOff is not null)
				{
					this.pictureBoxOff.Image = this.pictureBoxSimple.Image = prevOff.OffBitmap;
					this.pictureBoxOff.Tag = this.pictureBoxSimple.Tag = prevOff;
				}

				if (prevOver is not null)
				{
					this.pictureBoxOver.Image = prevOver.OffBitmap;
					this.pictureBoxOver.Tag = prevOver;
				}

				return;
			}

			// Reset image picker
			this.pictureBoxOn.Image = this.pictureBoxOff.Image =
			this.pictureBoxSimple.Image = this.pictureBoxOver.Image = null;
		}


		void TogglePicturePickup()
		{
			if (this.radioButtonPictureSimple.Checked)
			{
				groupBoxOnBitmap.Visible = false;
				groupBoxOffBitmap.Visible = false;
				groupBoxOverBitmap.Visible = false;
				groupBoxSimpleImage.Visible = true;
			}
			else
			{
				groupBoxOnBitmap.Visible = true;
				groupBoxOffBitmap.Visible = true;
				groupBoxOverBitmap.Visible = true;
				groupBoxSimpleImage.Visible = false;
			}
		}

		private void DisplayPictures()
		{
			this.PicsPanelList.ForEach(p => p.SuspendLayout());

			this.PicsPanelList.ForEach(p => p.ProcessAllControls(pb => DisplayPicture(pb)));

			this.PicsPanelList.ForEach(p => p.ResumeLayout(false));
			this.PicsPanelList.ForEach(p => p.PerformLayout());
		}

		private void DisplayPicture(Control picturebox)
		{
			var pb = picturebox as PictureBox;
			var info = pb.Tag as IconInfo;
			var resourceid = pb.Image.Tag.ToString();

			if (this.radioButtonPictureSimple.Checked)
			{
				pb.Visible = (resourceid == info.Off); // Display Off pics only for Simple mode
				return;
			}

			pb.Visible = true;// Display all
		}


		private void AddPicture(IconInfo info)
		{
			if (info.Off is not null)
				AddPicture(info, info.OffBitmap, info.Off);

			if (info.On is not null)
				AddPicture(info, info.OnBitmap, info.On);

			if (info.Over is not null)
				AddPicture(info, info.OverBitmap, info.Over);
		}


		private void AddPicture(IconInfo info, Bitmap bmp, string resourceId)
		{
			if (alreadyAddedPics.Contains(resourceId)) return;// Because an instance of IconInfo may contain resourceId multiple time

			if (bmp is not null)
			{
				var picOff = new PictureBox()
				{
					SizeMode = PictureBoxSizeMode.Zoom, // "Zoom" ensure aspect ratio on resize
					Size = new Size(32, 32), // force image size for picking. Source Bmp may vary
					Image = bmp,
					Tag = info,
					BorderStyle = BorderStyle.FixedSingle,
				};

				bmp.Tag = resourceId;

				// Magnifier
				picOff.MouseEnter += PicOff_MouseEnter;
				picOff.MouseLeave += PicOff_MouseLeave;


				// Drag & Drop events
				picOff.MouseDown += PicOff_MouseDown;
				picOff.MouseMove += PicOff_MouseMove;
				picOff.MouseUp += PicOff_MouseUp;

				// Allocate per category
				switch (info.Category)
				{
					case IconCategory.Misc:
						this.flowLayoutPanelPicsMisc.Controls.Add(picOff);
						break;
					case IconCategory.Artifacts:
						this.flowLayoutPanelPicsArtifacts.Controls.Add(picOff);
						break;
					case IconCategory.Relics:
						this.flowLayoutPanelPicsRelics.Controls.Add(picOff);
						break;
					case IconCategory.Jewellery:
						this.flowLayoutPanelPicsJewellery.Controls.Add(picOff);
						break;
					case IconCategory.Potions:
						this.flowLayoutPanelPicsPotions.Controls.Add(picOff);
						break;
					case IconCategory.Scrolls:
						this.flowLayoutPanelPicsScrolls.Controls.Add(picOff);
						break;
					case IconCategory.Skills:
						this.flowLayoutPanelPicsSkills.Controls.Add(picOff);
						break;
					case IconCategory.Buttons:
						this.flowLayoutPanelPicsButtons.Controls.Add(picOff);
						break;
					case IconCategory.Helmets:
						this.flowLayoutPanelPicsHelmets.Controls.Add(picOff);
						break;
					case IconCategory.Shields:
						this.flowLayoutPanelPicsShields.Controls.Add(picOff);
						break;
					case IconCategory.Armbands:
						this.flowLayoutPanelPicsArmbands.Controls.Add(picOff);
						break;
					case IconCategory.Greaves:
						this.flowLayoutPanelPicsGreaves.Controls.Add(picOff);
						break;
				}
				alreadyAddedPics.Add(resourceId);
			}
		}

		private void PicOff_MouseEnter(object sender, EventArgs e)
		{
			if (IsDragging) return;

			var picB = sender as PictureBox;
			var picFilePath = picB.Image.Tag as string;
			var picName = Path.GetFileNameWithoutExtension(picFilePath);
			picName = picName[0] + new string(picName.ToLower().Skip(1).ToArray());// Titlecase
			var info = picB.Tag as IconInfo;

			// Find original size
			Size? orig = null;
			if (info.On == picFilePath) orig = info?.OnBitmap?.Size;
			else if (info.Off == picFilePath) orig = info?.OffBitmap?.Size;
			else if (info.Over == picFilePath) orig = info?.OverBitmap?.Size;

			// Compute Position of Magnifier relative to picB
			var loc = this.PointToClient(picB.PointToScreen(Point.Empty));
			this.iconMagnifier.Location = new Point(loc.X, loc.Y + 32);// Locate below icon

			this.iconMagnifier.pictureBox.Image = picB.Image;
			this.iconMagnifier.scalingLabelFilename.Text = picName;
			this.iconMagnifier.scalingLabelSize.Text = orig is not null ? string.Format("{0} x {1}", orig?.Width, orig?.Height) : string.Empty;

			this.iconMagnifier.Visible = true;
		}

		private void PicOff_MouseLeave(object sender, EventArgs e)
		{
			if (IsDragging) return;

			this.iconMagnifier.Visible = false;
		}

		#region Drag & Drop

		private void PicOff_MouseDown(object sender, MouseEventArgs e)
		{
			var picB = sender as PictureBox;
			var info = picB.Tag as IconInfo;
			IsDragging = true;
			pictureBoxDrag.Location = this.PointToClient(Cursor.Position);
			pictureBoxDrag.Image = picB.Image;
			pictureBoxDrag.Tag = info;
			pictureBoxDrag.Visible = true;
		}

		private void PicOff_MouseMove(object sender, MouseEventArgs e)
		{
			if (IsDragging)
			{
				DragPosition = this.PointToClient(Cursor.Position);
				pictureBoxDrag.Location = new Point(DragPosition.X - 5, DragPosition.Y - 5);// Slight delta
			}
		}

		private void PicOff_MouseUp(object sender, MouseEventArgs e)
		{
			IsDragging = false;
			pictureBoxDrag.Visible = false;

			var info = pictureBoxDrag.Tag as IconInfo;
			var bmp = pictureBoxDrag.Image;

			var found = flowLayoutPanelPicturePickup.GetChildAtPoint(
				flowLayoutPanelPicturePickup.PointToClient(Cursor.Position)
				, GetChildAtPointSkip.Invisible | GetChildAtPointSkip.Transparent
			);

			if (this.radioButtonPictureDetailed.Checked)
			{
				if (found == this.groupBoxOnBitmap)
				{
					pictureBoxOn.Image = bmp;
					pictureBoxOn.Tag = info;
					pictureBoxOn.Image.Tag = info.On;
				}
				if (found == this.groupBoxOffBitmap)
				{
					pictureBoxOff.Image = bmp;
					pictureBoxOff.Tag = info;
					pictureBoxOff.Image.Tag = info.Off;
				}
				if (found == this.groupBoxOverBitmap)
				{
					pictureBoxOver.Image = bmp;
					pictureBoxOver.Tag = info;
					pictureBoxOver.Image.Tag = info.Over;
				}
			}

			if (this.radioButtonPictureSimple.Checked)
			{
				if (found == this.groupBoxSimpleImage)
				{
					pictureBoxSimple.Image = pictureBoxOff.Image = bmp;// Which is Off
					pictureBoxSimple.Image.Tag = pictureBoxOff.Image.Tag = info.Off;// Which is Off

					pictureBoxSimple.Tag = pictureBoxOver.Tag = pictureBoxOff.Tag = pictureBoxOn.Tag = info;

					pictureBoxOn.Image = info.OnBitmap;
					if (pictureBoxOn.Image is not null)// May happen on rare exception (TEX Filename rule not consitent)
						pictureBoxOn.Image.Tag = info.On;

					// Apply only if different
					if (info.IsOverSameAsOthers)
					{
						pictureBoxOver.Image = null;
					}
					else
					{
						pictureBoxOver.Image = info.OverBitmap;
						if (pictureBoxOver.Image is not null)
							pictureBoxOver.Image.Tag = info.Over;
					}
				}
			}

			if (found is not null)
				this.scalingRadioButtonCustomIcon.Checked = true;

#if DEBUG
			this.scalingTextBoxDebug.Text = bmp.Tag.ToString() + $" - ({bmp.Size.Width}x{bmp.Size.Height})";
#endif
		}

		#endregion

		private void radioButtonPictureSimple_CheckedChanged(object sender, EventArgs e)
		{ TogglePicturePickup(); DisplayPictures(); }

		private void radioButtonPictureDetailed_CheckedChanged(object sender, EventArgs e)
		{ TogglePicturePickup(); DisplayPictures(); }

		private void applyButton_Click(object sender, EventArgs e)
		{
			var dispmode = GetDisplayMode();

			// Validation
			if (this.pictureBoxOff.Image is null || this.pictureBoxOn.Image is null
				|| (dispmode.HasFlag(BagButtonDisplayMode.Label) && string.IsNullOrWhiteSpace(this.scalingTextBoxLabel.Text)))
			{
				MessageBox.Show(
					Resources.BagButtonSettingsValidationMessage,
					Resources.BagButtonSettingsValidationCaption,
					MessageBoxButtons.OK,
					MessageBoxIcon.Error,
					MessageBoxDefaultButton.Button1,
					PlayerPanel.RightToLeftOptions);

				this.DialogResult = DialogResult.None;
				return;
			}

			this._CurrentBagButton.Sack.IsModified = true;

			if (dispmode == BagButtonDisplayMode.Default)
			{
				// Reset to default settings
				this._CurrentBagButton.OnBitmap = Resources.inventorybagup01;
				this._CurrentBagButton.OffBitmap = Resources.inventorybagdown01;
				this._CurrentBagButton.OverBitmap = Resources.inventorybagover01;

				this._CurrentBagButton.Sack.BagButtonIconInfo = null;
			}
			else
			{
				// New settings
				var save = new BagButtonIconInfo()
				{
					DisplayMode = dispmode,
					Label = this.scalingTextBoxLabel.Text.Trim(),
					Off = this.pictureBoxOff.Image.Tag as string,
					On = this.pictureBoxOn.Image.Tag as string,
					Over = this.pictureBoxOver?.Image?.Tag as string,
				};

				this._CurrentBagButton.Sack.BagButtonIconInfo = save;
				this._CurrentBagButton.ApplyIconInfo(this._CurrentBagButton.Sack);

			}

			this.DialogResult = DialogResult.OK;
			this.Close();
		}

		private BagButtonDisplayMode GetDisplayMode()
		{
			var val = BagButtonDisplayMode.Default;

			if (this.scalingRadioButtonDefaultIcon.Checked) return val;

			if (this.scalingRadioButtonCustomIcon.Checked) val |= BagButtonDisplayMode.CustomIcon;
			if (this.scalingCheckBoxLabel.Checked) val |= BagButtonDisplayMode.Label;
			if (this.scalingCheckBoxNumber.Checked) val |= BagButtonDisplayMode.Number;

			return val;
		}

		private void cancelButton_Click(object sender, EventArgs e)
		{
			this.DialogResult = DialogResult.Cancel;
			this.Close();
		}

		private void checkBoxLabel_CheckedChanged(object sender, EventArgs e)
		{
			this.scalingTextBoxLabel.Enabled = this.scalingCheckBoxLabel.Checked;

			if (!this.scalingTextBoxLabel.Enabled) this.scalingTextBoxLabel.Text = string.Empty;
		}

		private void scalingRadioButtonDefaultIcon_CheckedChanged(object sender, EventArgs e)
		{
			if (this.scalingRadioButtonDefaultIcon.Checked)
			{
				this.scalingCheckBoxLabel.Checked =
				this.scalingCheckBoxNumber.Checked = false;

				this.pictureBoxOn.Image = Resources.inventorybagup01;
				this.pictureBoxOff.Image = this.pictureBoxSimple.Image = Resources.inventorybagdown01;
				this.pictureBoxOver.Image = Resources.inventorybagover01;
			}
		}

	}
}
