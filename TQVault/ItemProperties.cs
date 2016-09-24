//-----------------------------------------------------------------------
// <copyright file="ItemProperties.cs" company="None">
//     Copyright (c) Brandon Wallace and Jesse Calhoun. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace TQVault
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Data;
    using System.Drawing;
    using System.Globalization;
    using System.Text;
    using System.Windows.Forms;
    using TQVault.Properties;
    using TQVaultData;

    /// <summary>
    /// Form for the item properties display
    /// </summary>
    internal partial class ItemProperties : VaultForm
    {
        /// <summary>
        /// Item instance of the item we are displaying
        /// </summary>
        private Item item;

        /// <summary>
        /// Flag indicating if we are showing the extra information
        /// </summary>
        private bool filterExtra;

        /// <summary>
        /// Initializes a new instance of the ItemProperties class.
        /// </summary>
        public ItemProperties()
        {
            this.InitializeComponent();

            this.Text = Resources.ItemPropertiesText;
            this.ok.Text = Resources.GlobalOK;
            this.label1.Text = Resources.ItemPropertiesLabel1;
            this.label2.Text = Resources.ItemPropertiesLabel2;
            this.label3.Text = Resources.ItemPropertiesLabel3;
            this.checkBox1.Text = Resources.ItemPropertiesCheckBox1Label;

            if (Settings.Default.EnableNewUI)
            {
                this.DrawCustomBorder = true;
            }
            else
            {
                this.Revert(new Size(950, 489));
            }
        }

        /// <summary>
        /// Sets the item for which we are displaying the properties.
        /// </summary>
        public Item Item
        {
            set
            {
                this.item = value;
            }
        }

        /// <summary>
        /// Reverts the form back to the original size and UI style.
        /// </summary>
        /// <param name="originalSize">Original size of the form.</param>
        protected override void Revert(Size originalSize)
        {
            this.DrawCustomBorder = false;
            this.ClientSize = originalSize;
            this.BackgroundImage = Resources.SplashScreen;
            this.BackgroundImageLayout = ImageLayout.Stretch;

            Font originalFont = new Font("Albertus MT", 9.0F, FontStyle.Regular, GraphicsUnit.Point, (byte)0);

            this.webBrowser1.Location = new Point(12, 123);
            this.webBrowser1.Size = new Size(292, 269);

            this.itemName.Location = new Point(15, 30);
            this.itemName.Size = new Size(730, 39);

            this.webBrowser2.Location = new Point(327, 123);
            this.webBrowser2.Size = new Size(292, 269);

            this.label1.Font = originalFont;
            this.label1.Location = new Point(324, 101);
            this.label1.Size = new Size(90, 14);

            this.label2.Font = originalFont;
            this.label2.Location = new Point(12, 101);
            this.label2.Size = new Size(107, 14);

            this.checkBox1.Font = originalFont;
            this.checkBox1.Location = new Point(763, 42);
            this.checkBox1.Size = new Size(106, 18);

            this.webBrowser3.Location = new Point(655, 123);
            this.webBrowser3.Size = new Size(262, 269);

            this.label3.Font = originalFont;
            this.label3.Location = new Point(655, 101);
            this.label3.Size = new Size(90, 14);

            this.ok.Revert(new Point(818, 424), new Size(87, 25));
        }

        /// <summary>
        /// Gets the name of the item
        /// </summary>
        /// <param name="item">item being displayed</param>
        /// <returns>string with the item name</returns>
        private static string GetName(Item item)
        {
            string itemName = Database.MakeSafeForHtml(item.ToString(true, false));
            string bgcolor = "#2e1f15";
            if (!Settings.Default.EnableNewUI)
            {
                bgcolor = "null";
            }

            Color color = item.GetColorTag(itemName);
            itemName = Item.ClipColorTag(itemName);
            itemName = string.Format(CultureInfo.InvariantCulture, "<font size=+1 color={0}><b>{1}</b></font>", Database.HtmlColor(color), itemName);
            return string.Format(CultureInfo.InvariantCulture, "<body bgcolor={0} text=white><font face=\"Albertus MT\" size=2>{1}", bgcolor, itemName);
        }

        /// <summary>
        /// Dialog load methond
        /// </summary>
        /// <param name="sender">sender object</param>
        /// <param name="e">EventArgs data</param>
        private void ItemProperties_Load(object sender, EventArgs e)
        {
            this.filterExtra = false;
            this.itemName.DocumentText = GetName(this.item);
            this.LoadProperties();
        }

        /// <summary>
        /// Loads the item properties
        /// </summary>
        private void LoadProperties()
        {
            string[] bareAttr;
            bareAttr = this.item.GetBareAttributes(this.filterExtra);
            string bgcolor = "#2e1f15";

            if (!Settings.Default.EnableNewUI)
            {
                bgcolor = "#2e291f";
            }

            // Base Item Attributes
            if (bareAttr[0].Length == 0)
            {
                this.webBrowser1.Hide();
                this.label2.Hide();
            }
            else
            {
                this.webBrowser1.DocumentText = string.Format(CultureInfo.InvariantCulture, "<body bgcolor={0} text=white><font face=\"Albertus MT\" size=1>{1}", bgcolor, bareAttr[0]);
                this.webBrowser1.Show();
                this.label2.Show();
            }

            // Prefix Attributes
            if (bareAttr[2].Length == 0)
            {
                this.webBrowser2.Hide();
                this.label1.Hide();
            }
            else
            {
                this.webBrowser2.DocumentText = string.Format(CultureInfo.InvariantCulture, "<body bgcolor={0} text=white><font face=\"Albertus MT\" size=1>{1}", bgcolor, bareAttr[2]);
                this.webBrowser2.Show();
                this.label1.Show();
            }

            // Suffix Attributes
            if (bareAttr[3].Length == 0)
            {
                this.webBrowser3.Hide();
                this.label3.Hide();
            }
            else
            {
                this.webBrowser3.DocumentText = string.Format(CultureInfo.InvariantCulture, "<body bgcolor={0} text=white><font face=\"Albertus MT\" size=1>{1}", bgcolor, bareAttr[3]);
                this.webBrowser3.Show();
                this.label3.Show();
            }
        }

        /// <summary>
        /// Handler for the OK button
        /// </summary>
        /// <param name="sender">sender object</param>
        /// <param name="e">EventArgs data</param>
        private void OK_Button_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// Handler for clicking the check box
        /// </summary>
        /// <param name="sender">sender object</param>
        /// <param name="e">EventArgs data</param>
        private void CheckBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (this.checkBox1.Checked)
            {
                if (this.filterExtra == false)
                {
                    this.filterExtra = true;
                    this.item.RefreshBareAttributes();
                    this.LoadProperties();
                }
            }
            else
            {
                if (this.filterExtra == true)
                {
                    this.filterExtra = false;
                    this.item.RefreshBareAttributes();
                    this.LoadProperties();
                }
            }
        }

        /// <summary>
        /// Item name completed handler
        /// </summary>
        /// <param name="sender">sender object</param>
        /// <param name="e">EventArgs data</param>
        private void ItemName_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
        }
    }
}