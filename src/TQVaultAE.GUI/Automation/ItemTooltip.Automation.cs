using TQVaultAE.Domain.Entities;
using TQVaultAE.GUI.Automation;
using TQVaultAE.Presentation;

namespace TQVaultAE.GUI.Tooltips;

/// <summary>
/// Entry point of the Debug tool <c>tooltip_text_dump</c>: builds an item tooltip without showing it
/// and records the rows its own code creates, panel by panel, before they are drawn.
/// </summary>
public partial class ItemTooltip
{
	/// <summary>Receives the rows of the panels built by <see cref="FillToolTip"/>, when set.</summary>
	internal ItemTooltipRecorder? AutomationRecorder { get; private set; }
}
