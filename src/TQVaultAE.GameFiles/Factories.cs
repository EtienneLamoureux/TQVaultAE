//-----------------------------------------------------------------------
// <copyright file="Factories.cs" company="TQVaultAE">
//     Copyright (c) 2026 TQVaultAE Contributors. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

// Entry points of the game file readers. Signatures are part of the contract; the bodies live in partial files:
//   ArzDatabaseFactory.Open(string path)                          -> ArzDatabase.cs
//   ArcArchiveFactory.Open(string path)                           -> ArcArchive.cs
//   TextDatabaseFactory.Load(string textArchivePath, string lang) -> TextDatabase.cs

namespace TQVaultAE.GameFiles;

/// <summary>Opens database.arz files.</summary>
public static partial class ArzDatabaseFactory;

/// <summary>Opens .arc archives.</summary>
public static partial class ArcArchiveFactory;

/// <summary>Loads Text_XX.arc.</summary>
public static partial class TextDatabaseFactory;
