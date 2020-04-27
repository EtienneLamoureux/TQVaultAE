using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TQVaultAE.GUI.Components
{
	// Inspired from https://stackoverflow.com/questions/33776387/dont-raise-textchanged-while-continuous-typing
	public class TypeAssistant : Component
	{
		System.Threading.Timer waitingTimer;

		public event EventHandler Idled = delegate { };

		[DefaultValue(1000)]
		public int WaitingMilliSeconds { get; set; } = 1000;

		public TypeAssistant()
		{
			waitingTimer = new System.Threading.Timer(p =>
			{
				Idled(this, EventArgs.Empty);
			});
		}

		public void TextChanged()
			=> waitingTimer.Change(WaitingMilliSeconds, System.Threading.Timeout.Infinite);
	}
}
