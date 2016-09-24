//-----------------------------------------------------------------------
// <copyright file="ScalingRadioButton.cs" company="None">
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
    /// RadioButton class to support scaling of the fonts.
    /// </summary>
    public class ScalingRadioButton : RadioButton
    {
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
