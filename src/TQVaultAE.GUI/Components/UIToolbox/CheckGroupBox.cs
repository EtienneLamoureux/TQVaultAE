// Copied from the CheckGroupBox 1.0.2 package (.NET Framework only) to build it for .NET 10.

using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;


namespace TQVaultAE.GUI.Components.UIToolbox;

/// <summary>
/// Define <see cref="CheckGroupBox"/> behaviour. e.g. <c><see cref="Enable"/> | <see cref="Show"/></c>
/// </summary>
public enum CheckGroupBoxCheckedBehavior
{
	DoNothing = 0,
	Disable = 1 << 0,
	Enable = 1 << 1,
	Hide = 1 << 2,
	Show = 1 << 3,
	EnableShow = Show | Enable,
	DisableHide = Disable | Hide
}

/// <summary>
/// CheckGroupBox is a GroupBox with an embeded CheckBox.
/// </summary>
[ToolboxBitmap(typeof(CheckGroupBox), "CheckGroupBox.bmp")]
public partial class CheckGroupBox : GroupBox
{
	// Constants
	private const int CHECKBOX_X_OFFSET = 10;
	private const int CHECKBOX_Y_OFFSET = 0;

	// Members
	private CheckGroupBoxCheckedBehavior m_bBehavior;

	/// <summary>
	/// CheckGroupBox public constructor.
	/// </summary>
	public CheckGroupBox()
	{
		this.InitializeComponent();
		this.m_bBehavior = CheckGroupBoxCheckedBehavior.DoNothing;
		this.m_checkBox.Parent = this;
		this.m_checkBox.Location = new System.Drawing.Point(CHECKBOX_X_OFFSET, CHECKBOX_Y_OFFSET);
		this.Checked = true;

		// Set the color of the CheckBox's text to the color of the label in a standard groupbox control.
		VisualStyleRenderer vsr = new VisualStyleRenderer(VisualStyleElement.Button.GroupBox.Normal);
		Color groupBoxTextColor = vsr.GetColor(ColorProperty.TextColor);
		this.m_checkBox.ForeColor = groupBoxTextColor;
	}

	#region Properties
	/// <summary>
	/// The text associated with the control.
	/// </summary>
	[AllowNull]
	public override string Text
	{
		get
		{
			if (this.Site is not null && this.Site.DesignMode == true)
			{
				// Design-time mode
				return this.m_checkBox.Text;
			}
			else
			{
				// Run-time
				return " "; // Set the text of the GroupBox to a space, so the gap appears before the CheckBox.
			}
		}
		set
		{
			base.Text = " "; // Set the text of the GroupBox to a space, so the gap appears before the CheckBox.
			this.m_checkBox.Text = value;
		}
	}

	/// <summary>
	/// Indicates whether the component is checked or not.
	/// </summary>
	[Description("Indicates whether the component is checked or not.")]
	[Category("Appearance")]
	[DefaultValue(true)]
	public bool Checked
	{
		get
		{
			return this.m_checkBox.Checked;
		}
		set
		{
			if (this.m_checkBox.Checked != value)
			{
				this.m_checkBox.Checked = value;
			}
		}
	}

	/// <summary>
	/// Indicates the state of the component.
	/// </summary>
	[Description("Indicates the state of the component.")]
	[Category("Appearance")]
	[DefaultValue(CheckState.Checked)]
	public CheckState CheckState
	{
		get
		{
			return this.m_checkBox.CheckState;
		}
		set
		{
			if (this.m_checkBox.CheckState != value)
			{
				this.m_checkBox.CheckState = value;
			}
		}
	}

	/// <summary>
	/// Determines the behavior when the CheckBox is checked.
	/// </summary>
	[Description("Determines the behavior when the CheckBox is checked.")]
	[Category("Appearance")]
	[DefaultValue(CheckGroupBoxCheckedBehavior.DoNothing)]
	public CheckGroupBoxCheckedBehavior CheckedBehavior
	{
		get
		{
			return this.m_bBehavior;
		}
		set
		{
			this.m_bBehavior = value;
		}
	}


	#endregion Properties

	#region Event Handlers
	/// <summary>
	/// Occurs whenever the Checked property of the CheckBox is changed.
	/// </summary>
	[Description("Occurs whenever the Checked property of the CheckBox is changed.")]
	public event EventHandler? CheckedChanged;

	/// <summary>
	/// Occurs whenever the CheckState property of the CheckBox is changed.
	/// </summary>
	[Description("Occurs whenever the CheckState property of the CheckBox is changed.")]
	public event EventHandler? CheckStateChanged;

	#endregion Event Handlers

	#region Events
	private void checkBox_CheckedChanged(object? sender, EventArgs e)
	{
		if (this.m_bBehavior != CheckGroupBoxCheckedBehavior.DoNothing)
		{
			foreach (Control control in this.Controls)
			{
				ApplyBehavior(control);
			}
		}

		if (CheckedChanged is not null)
		{
			CheckedChanged(sender, e);
		}
	}

	private void ApplyBehavior(Control control)
	{
		if (control == this.m_checkBox)
			return;

		if (this.m_checkBox.Checked)
		{
			if ((this.m_bBehavior & CheckGroupBoxCheckedBehavior.Disable) != CheckGroupBoxCheckedBehavior.DoNothing && control.Enabled)
				control.Enabled = false;

			else if ((this.m_bBehavior & CheckGroupBoxCheckedBehavior.Enable) != CheckGroupBoxCheckedBehavior.DoNothing && !control.Enabled)
				control.Enabled = true;

			if ((this.m_bBehavior & CheckGroupBoxCheckedBehavior.Hide) != CheckGroupBoxCheckedBehavior.DoNothing && control.Visible)
				control.Visible = false;

			else if ((this.m_bBehavior & CheckGroupBoxCheckedBehavior.Show) != CheckGroupBoxCheckedBehavior.DoNothing && !control.Visible)
				control.Visible = true;

			return;
		}

		// UnChecked
		if ((this.m_bBehavior & CheckGroupBoxCheckedBehavior.Disable) != CheckGroupBoxCheckedBehavior.DoNothing && !control.Enabled)
			control.Enabled = true;

		else if ((this.m_bBehavior & CheckGroupBoxCheckedBehavior.Enable) != CheckGroupBoxCheckedBehavior.DoNothing && control.Enabled)
			control.Enabled = false;

		if ((this.m_bBehavior & CheckGroupBoxCheckedBehavior.Hide) != CheckGroupBoxCheckedBehavior.DoNothing && !control.Visible)
			control.Visible = true;

		else if ((this.m_bBehavior & CheckGroupBoxCheckedBehavior.Show) != CheckGroupBoxCheckedBehavior.DoNothing && control.Visible)
			control.Visible = false;

	}

	private void checkBox_CheckStateChanged(object? sender, EventArgs e)
	{
		if (CheckStateChanged is not null)
		{
			CheckStateChanged(sender, e);
		}
	}

	private void CheckGroupBox_ControlAdded(object? sender, ControlEventArgs e)
	{
		if (this.m_bBehavior != CheckGroupBoxCheckedBehavior.DoNothing && e.Control is not null)
		{
			ApplyBehavior(e.Control);
		}
	}
	#endregion Events

	private void CheckGroupBox_ForeColorChanged(object? sender, EventArgs e)
	{
		this.m_checkBox.ForeColor = this.ForeColor;
	}

	private void CheckGroupBox_FontChanged(object? sender, EventArgs e)
	{
		// The title inherits the group font rather than holding a copy: when the DPI changes, Windows Forms scales the
		// group font, and would scale a copy a second time.
		this.m_checkBox.ResetFont();
	}
}
