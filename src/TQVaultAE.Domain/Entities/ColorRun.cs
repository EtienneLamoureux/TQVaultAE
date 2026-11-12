namespace TQVaultAE.Domain.Entities;

/// <summary>A part of a game text written in one colour.</summary>
/// <param name="Text">The text of the part, without its colour tag.</param>
/// <param name="Code">The upper-case code of the colour tag it starts with (<c>Y</c> for <c>{^y}</c>);
/// <see langword="null"/> before the first tag.</param>
public readonly record struct ColorRun(string Text, char? Code);
