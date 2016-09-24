//-----------------------------------------------------------------------
// <copyright file="ScalingComboBox.cs" company="None">
//     Copyright (c) Brandon Wallace and Jesse Calhoun. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace TQVault
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Linq;
    using System.Text;
    using System.Windows.Forms;

    /// <summary>
    /// ComboBox class to support scaling of the fonts.
    /// </summary>
    public class ScalingComboBox : ComboBox
    {
        /// <summary>
        /// Reverts the basic settings of a control back to the original settings.
        /// </summary>
        /// <param name="location">New Location of the control</param>
        /// <param name="size">New Size of the control</param>
        public void Revert(Point location, Size size)
        {
            this.Font = new Font("Microsoft Sans Serif", 8.25F);
            this.Location = location;
            this.Size = size;
            this.BackColor = SystemColors.Window;
            this.ForeColor = SystemColors.WindowText;
        }

        /// <summary>
        /// Override of ScaleControl which supports font scaling.
        /// </summary>
        /// <param name="factor">SizeF for the scale factor</param>
        /// <param name="specified">BoundsSpecified value.</param>
        protected override void ScaleControl(SizeF factor, BoundsSpecified specified)
        {
            this.Font = new Font(this.Font.Name, this.Font.SizeInPoints * factor.Height, this.Font.Style);

            base.ScaleControl(factor, specified);
        }
    }
}
