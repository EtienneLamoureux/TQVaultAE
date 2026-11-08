using System.Globalization;
using TQVaultAE.Application.Results;
using TQVaultAE.Application;

namespace TQVaultAE.Presentation;

/// <summary>
/// A message to show the user after loading a file.
/// </summary>
/// <param name="Text">Message text.</param>
/// <param name="Caption">Message box caption.</param>
/// <param name="IsError">An error (otherwise an information).</param>
public sealed record LoadMessage(string Text, string Caption, bool IsError);

/// <summary>
/// Turns the problems of a player or stash load into messages for the user.
/// </summary>
public static class LoadMessages
{
	/// <summary>
	/// Gets the messages for a player load: the file could not be parsed.
	/// </summary>
	/// <param name="result">The load result.</param>
	/// <returns>The messages, empty when the player loaded fine.</returns>
	public static IReadOnlyList<LoadMessage> ForPlayer(PlayerLoadResult result)
	{
		var messages = new List<LoadMessage>();

		if (result.Player?.ArgumentException is { } error)
			messages.Add(ParseError(result.PlayerFile, error));

		return messages;
	}

	/// <summary>
	/// Gets the messages for a stash load: the file is missing, or it could not be parsed.
	/// </summary>
	/// <param name="result">The load result.</param>
	/// <param name="notFoundDetail">What is missing, shown under the "stash not found" message (character or stash name).</param>
	/// <returns>The messages in the order to show them, empty when the stash loaded fine.</returns>
	public static IReadOnlyList<LoadMessage> ForStash(StashLoadResult result, string notFoundDetail)
	{
		var messages = new List<LoadMessage>();

		if (result.Stash is { StashFound: false })
			messages.Add(new LoadMessage(string.Concat(Resources.StashNotFoundMsg, "\n\n", notFoundDetail), Resources.StashNotFound, false));

		if (result.Stash?.ArgumentException is { } error)
			messages.Add(ParseError(result.StashFile, error));

		return messages;
	}

	/// <summary>
	/// Gets the messages for the load of a character's stash.
	/// </summary>
	/// <param name="result">The load result.</param>
	/// <param name="character">The character.</param>
	/// <returns>The messages in the order to show them.</returns>
	public static IReadOnlyList<LoadMessage> ForPlayerStash(StashLoadResult result, PlayerSave character)
		=> ForStash(result, $"Character : {character}");

	/// <summary>
	/// Gets the messages for the load of the transfer stash.
	/// </summary>
	/// <param name="result">The load result.</param>
	/// <returns>The messages in the order to show them.</returns>
	public static IReadOnlyList<LoadMessage> ForTransferStash(StashLoadResult result)
		=> ForStash(result, $"Transfer Stash\n\n{result.StashFile}");

	/// <summary>
	/// Gets the messages for the load of the relic vault stash.
	/// </summary>
	/// <param name="result">The load result.</param>
	/// <returns>The messages in the order to show them.</returns>
	public static IReadOnlyList<LoadMessage> ForRelicVaultStash(StashLoadResult result)
		=> ForStash(result, $"Relic Stash\n\n{result.StashFile}");

	/// <summary>
	/// Gets the message shown when a file cannot be read.
	/// </summary>
	/// <param name="file">The file.</param>
	/// <param name="exception">The read error.</param>
	/// <returns>The message text.</returns>
	public static string ReadError(string? file, Exception exception)
		=> string.Format(CultureInfo.InvariantCulture, Resources.MainFormReadError, file, exception.ToString());

	private static LoadMessage ParseError(string? file, ArgumentException error)
		=> new(
			string.Format(CultureInfo.CurrentUICulture, "{0}\n{1}\n{2}", Resources.MainFormPlayerReadError, file, error.Message)
			, Resources.GlobalError
			, true);
}
