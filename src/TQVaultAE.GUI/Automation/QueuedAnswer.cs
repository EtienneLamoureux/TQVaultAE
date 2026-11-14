//-----------------------------------------------------------------------
// <copyright file="QueuedAnswer.cs" company="None">
//     Copyright (c) Brandon Wallace and Jesse Calhoun. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace TQVaultAE.GUI.Automation;

/// <summary>
/// An answer queued by <c>queue_dialog_answers</c>: a button of a message box, or a text typed in a text input dialog
/// before its OK button (<see cref="Text"/> set, <see cref="Result"/> OK).
/// </summary>
internal readonly record struct QueuedAnswer(DialogResult Result, string? Text = null)
{
	/// <summary>The prefix of a text answer.</summary>
	public const string TextPrefix = "text:";

	/// <summary>Reads one answer: a button name (OK, Cancel, Yes, No, Abort, Retry, Ignore) or <c>text:</c> and the text.</summary>
	/// <exception cref="ArgumentException">Not a known answer.</exception>
	public static QueuedAnswer Parse(string answer)
	{
		if (answer.StartsWith(TextPrefix, StringComparison.OrdinalIgnoreCase))
			return new QueuedAnswer(DialogResult.OK, answer[TextPrefix.Length..]);

		return Enum.TryParse<DialogResult>(answer, true, out var result) && result != DialogResult.None && !int.TryParse(answer, out _)
			? new QueuedAnswer(result)
			: throw new ArgumentException($"Unknown answer '{answer}'. Use OK, Cancel, Yes, No, Abort, Retry, Ignore or text:<text>.");
	}

	/// <inheritdoc/>
	public override string ToString() => this.Text is null ? this.Result.ToString() : TextPrefix + this.Text;
}
