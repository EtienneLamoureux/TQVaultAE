using System.Reflection;
using System.Windows.Forms;
using AwesomeAssertions;

namespace TQVaultAE.Tests.Win32.Gui;

/// <summary>
/// The queue of automation answers (<c>AutomationHost</c>): tools queue answers from request threads while the UI thread
/// takes them for the message boxes.
/// </summary>
public sealed class DialogAnswerQueueTests : IDisposable
{
	private static readonly Assembly Gui = typeof(TQVaultAE.GUI.Program).Assembly;
	private static readonly Type Host = Gui.GetType("TQVaultAE.GUI.Automation.AutomationHost", throwOnError: true)!;
	private static readonly Type Answer = Gui.GetType("TQVaultAE.GUI.Automation.QueuedAnswer", throwOnError: true)!;

	public DialogAnswerQueueTests() => Call("ClearDialogAnswers");

	public void Dispose() => Call("ClearDialogAnswers");

	private static object? Call(string name, params object?[] args)
		=> Host.GetMethod(name, BindingFlags.Public | BindingFlags.Static)!.Invoke(null, args);

	private static object NewAnswer(DialogResult result) => Activator.CreateInstance(Answer, result, null)!;

	private static void Queue(params DialogResult[] results)
	{
		var list = (System.Collections.IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(Answer))!;
		foreach (var result in results)
			list.Add(NewAnswer(result));
		Call("QueueDialogAnswers", list);
	}

	private static DialogResult? Take(MessageBoxButtons buttons)
	{
		object?[] args = ["caption", "message", buttons, DialogResult.None, null];
		var taken = (bool)Call("TryTakeDialogAnswer", args)!;
		return taken ? (DialogResult)args[3]! : null;
	}

	private static int PendingCount
		=> ((System.Collections.ICollection)Host.GetProperty("PendingDialogAnswers", BindingFlags.Public | BindingFlags.Static)!.GetValue(null)!).Count;

	[Fact]
	public void QueueDialogAnswerFirst_GoesBeforeTheQueuedAnswers()
	{
		Queue(DialogResult.No);
		Call("QueueDialogAnswerFirst", NewAnswer(DialogResult.Yes));

		Take(MessageBoxButtons.YesNo).Should().Be(DialogResult.Yes);
		Take(MessageBoxButtons.YesNo).Should().Be(DialogResult.No);
		Take(MessageBoxButtons.YesNo).Should().BeNull("the queue is empty");
	}

	[Fact]
	public void TryTakeDialogAnswer_AnswerNotOneOfTheButtons_StaysQueued()
	{
		Queue(DialogResult.Yes);

		Take(MessageBoxButtons.OK).Should().BeNull("an OK-only box does not take the answer meant for a question");
		Take(MessageBoxButtons.YesNo).Should().Be(DialogResult.Yes);
	}

	[Fact]
	public void WithdrawDialogAnswer_RemovesOnlyTheFirstEqualAnswer()
	{
		Queue(DialogResult.OK, DialogResult.Cancel, DialogResult.OK);

		Call("WithdrawDialogAnswer", NewAnswer(DialogResult.OK));

		Take(MessageBoxButtons.OKCancel).Should().Be(DialogResult.Cancel);
		Take(MessageBoxButtons.OKCancel).Should().Be(DialogResult.OK);
		PendingCount.Should().Be(0);
	}

	[Fact]
	public async Task QueueFromSeveralThreads_WhileAnswersAreTaken_LosesNoAnswer()
	{
		// An answer put first while other threads queue and take answers: none is lost or taken twice.
		const int perThread = 2000;
		var ct = TestContext.Current.CancellationToken;
		var taken = 0;
		using var start = new ManualResetEventSlim();
		var first = Task.Run(() =>
		{
			start.Wait(ct);
			for (var i = 0; i < perThread; i++)
				Call("QueueDialogAnswerFirst", NewAnswer(DialogResult.OK));
		}, ct);
		var last = Task.Run(() =>
		{
			start.Wait(ct);
			for (var i = 0; i < perThread; i++)
				Queue(DialogResult.OK);
		}, ct);
		var taker = Task.Run(() =>
		{
			start.Wait(ct);
			for (var i = 0; i < perThread; i++)
				if (Take(MessageBoxButtons.OK) is not null)
					taken++;
		}, ct);

		start.Set();
		await Task.WhenAll(first, last, taker);

		(taken + PendingCount).Should().Be(2 * perThread);
	}
}
