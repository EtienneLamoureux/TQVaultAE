namespace TQVaultAE.Automation;

/// <summary>
/// Runs the automation tools on the UI thread of the app's main form: the endpoint answers on thread pool threads, and
/// Windows Forms controls may only be touched from the thread that created them.
/// </summary>
public static class UiThread
{
	/// <summary>
	/// Runs <paramref name="action"/> on the UI thread of <paramref name="form"/> and returns its result; exceptions
	/// thrown by the action reach the caller unchanged.
	/// </summary>
	/// <param name="form">The main form, whose handle is created.</param>
	/// <param name="action">The work to do on the UI thread.</param>
	/// <param name="timeout">How long to wait for the UI thread, or null to wait as long as it takes.</param>
	/// <param name="enter">Runs first on the UI thread when the call comes from another thread (to set its culture).</param>
	/// <exception cref="UiThreadHangException">The UI thread did not answer within <paramref name="timeout"/>.</exception>
	public static T Run<TForm, T>(TForm form, Func<TForm, T> action, TimeSpan? timeout = null, Action? enter = null)
		where TForm : Control
	{
		ArgumentNullException.ThrowIfNull(form);
		ArgumentNullException.ThrowIfNull(action);
		if (!form.InvokeRequired)
			return action(form);

		T Body()
		{
			enter?.Invoke();
			return action(form);
		}

		if (timeout is not { } limit)
			return form.Invoke(Body);

		// A Func delegate, not a lambda: the lambda would bind to BeginInvoke(Action) and lose the result.
		var pending = form.BeginInvoke(new Func<T>(Body));
		if (!pending.AsyncWaitHandle.WaitOne(limit))
			throw new UiThreadHangException(limit);

		return (T)form.EndInvoke(pending)!;
	}
}

/// <summary>The UI thread did not answer an automation call in time: it is blocked.</summary>
public sealed class UiThreadHangException : TimeoutException
{
	public UiThreadHangException()
	{
	}

	public UiThreadHangException(string message)
		: base(message)
	{
	}

	public UiThreadHangException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	public UiThreadHangException(TimeSpan limit)
		: base($"The UI thread did not answer within {limit.TotalSeconds:0.#} s: it is blocked (hang).")
	{
	}
}
