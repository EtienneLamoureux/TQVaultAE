using System.ComponentModel;

namespace TQVaultAE.GUI.Components;

/// <summary>
/// Raises <see cref="Idled"/> once typing has paused for <see cref="WaitingMilliSeconds"/>, on a thread pool thread.
/// </summary>
// Inspired from https://stackoverflow.com/questions/33776387/dont-raise-textchanged-while-continuous-typing
public class TypeAssistant : Component
{
	private readonly System.Threading.Timer waitingTimer;

	public event EventHandler Idled = delegate { };

	[DefaultValue(1000)]
	public int WaitingMilliSeconds { get; set; } = 1000;

	public TypeAssistant()
	{
		this.waitingTimer = new System.Threading.Timer(p =>
		{
			this.Idled(this, EventArgs.Empty);
		});
	}

	/// <summary>A type assistant disposed with <paramref name="container"/> (the form's designer components).</summary>
	public TypeAssistant(IContainer container) : this()
		=> container.Add(this);

	public void TextChanged()
		=> this.waitingTimer.Change(this.WaitingMilliSeconds, System.Threading.Timeout.Infinite);

	protected override void Dispose(bool disposing)
	{
		if (disposing)
			this.waitingTimer.Dispose();
		base.Dispose(disposing);
	}
}
