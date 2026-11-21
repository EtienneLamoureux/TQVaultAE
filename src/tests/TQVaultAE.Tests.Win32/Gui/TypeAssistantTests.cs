using System.ComponentModel;
using AwesomeAssertions;
using TQVaultAE.GUI.Components;

namespace TQVaultAE.Tests.Win32.Gui;

public sealed class TypeAssistantTests
{
	[Fact]
	public async Task TextChanged_RaisesIdledOnceTypingPauses()
	{
		using var assistant = new TypeAssistant { WaitingMilliSeconds = 20 };
		var idled = new TaskCompletionSource();
		assistant.Idled += (_, _) => idled.TrySetResult();

		assistant.TextChanged();

		await idled.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
	}

	[Fact]
	public async Task DisposedWithItsContainer_StopsItsTimer()
	{
		// The form's components dispose the assistant, and its timer with it: a pending wait no longer fires.
		using var container = new Container();
		var assistant = new TypeAssistant(container) { WaitingMilliSeconds = 50 };
		var raised = false;
		assistant.Idled += (_, _) => raised = true;

		assistant.TextChanged();
		container.Dispose();
		await Task.Delay(300, TestContext.Current.CancellationToken);

		raised.Should().BeFalse();
	}
}
