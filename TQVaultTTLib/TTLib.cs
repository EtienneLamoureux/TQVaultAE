//-----------------------------------------------------------------------
// <copyright file="TTLib.cs" company="None">
//     Copyright (c) Brandon Wallace and Jesse Calhoun. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace TQVault
{
    using System.Windows.Forms;
    using VXPLibrary;

    /// <summary>
    /// ToolTip Activate Delegate
    /// </summary>
    /// <param name="windowHandle">window handle</param>
    /// <returns>tool tip text string</returns>
    public delegate string TTLibToolTipActivate(int windowHandle);

    /// <summary>
    /// Simple class for TQVault to access the VXPLibrary tooltip functions.
    /// </summary>
    public class TTLib
    {
        /// <summary>
        /// ToolTip Manager instance
        /// </summary>
        private VXPTooltipManager toolTipManager = new VXPTooltipManager();

        /// <summary>
        /// ToolTip Activation delegate
        /// </summary>
        private TTLibToolTipActivate onActivate;

        /// <summary>
        /// Used to indicate whether the tool tip delay is set.
        /// </summary>
        private bool noDelay;

        /// <summary>
        /// Gets or sets the tool tip activate callback
        /// </summary>
        public TTLibToolTipActivate ActivateCallback
        {
            get
            {
                return this.onActivate;
            }

            set
            {
                this.onActivate = value;
            }
        }

        /// <summary>
        /// Initializes the tooltips.  Call inside the Form_Load event and pass in the form handle
        /// </summary>
        /// <param name="mainForm">Main Windows forms instance</param>
        public void Initialize(Control mainForm)
        {
            this.toolTipManager.OnActivateCustomTooltip += new _IVXPTooltipManagerEvents_OnActivateCustomTooltipEventHandler(this.OnActivateCustomTooltip);

            // Changed by Th to 200.
            this.toolTipManager.ShowDelay = 200;
            this.toolTipManager.tool.ShowWhenEmpty = false;
            this.toolTipManager.tool.Autohide = false;
            this.toolTipManager.tool.BorderColor = 0x8e8c81;
            this.toolTipManager.tool.FadeHide = 0;
            this.toolTipManager.tool.FadeShow = 0;
            this.toolTipManager.tool.HasBorder = true;
            this.toolTipManager.tool.HasShadow = true;
            this.toolTipManager.tool.HasTail = false;
            this.toolTipManager.tool.Round = 2;
            this.toolTipManager.tool.Transparency = 220;
            this.toolTipManager.HideDelay = short.MaxValue;

            ////this.toolTipManager.DebugMode = true;
            this.toolTipManager.Activator = mainForm.Handle.ToInt32();
        }

        /// <summary>
        /// Adds a window to the tooltip manager
        /// </summary>
        /// <param name="control">control we are adding</param>
        /// <param name="tooltipText">tool tip text</param>
        public void AddWindowTooltip(Control control, string tooltipText)
        {
            this.toolTipManager.windows.Add(control.Handle.ToInt32(), tooltipText);
        }

        /// <summary>
        /// Changes the tool tip text
        /// </summary>
        /// <param name="newText">new tool tip text that we want to display</param>
        public void ChangeText(string newText)
        {
            if (this.noDelay)
            {
                // Changed by Th.
                this.toolTipManager.tool.Hide(true);
            }

            if (!string.IsNullOrEmpty(newText))
            {
                this.toolTipManager.tool.html.SetSourceText(newText);
                if (this.noDelay)
                {
                    // Changed by Th.
                    this.toolTipManager.tool.Show();
                }
            }
        }

        /// <summary>
        /// Sets tool tip for no delay
        /// </summary>
        public void SetNoDelay()
        {
            this.noDelay = true;
            this.toolTipManager.ShowDelay = 0;
        }

        /// <summary>
        /// Activatation event callback
        /// </summary>
        /// <param name="windowHandle">window handle</param>
        /// <param name="className">class name of the window</param>
        /// <param name="x">x position</param>
        /// <param name="y">y position</param>
        /// <param name="text">tool tip text</param>
        private void OnActivateCustomTooltip(int windowHandle, string className, short x, short y, ref string text)
        {
            TTLibToolTipActivate temp = this.onActivate;

            if (temp != null)
            {
                string ans = temp(windowHandle);
                if (!string.IsNullOrEmpty(ans))
                {
                    text = ans;
                }
            }
        }
    }
}