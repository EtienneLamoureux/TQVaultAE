# TQVaultAE.DbCli (`tqdb`)

Command-line tool to search the Titan Quest AE database (`database.arz`) and archives (`*.arc`) and export JSON, TSV, `.dbr` text or raw assets.

User documentation: [documentation/DBCLI.md](../../documentation/DBCLI.md).

```
dotnet build src/TQVaultAE.DbCli/TQVaultAE.DbCli.csproj
src/TQVaultAE.DbCli/bin/Debug/net10.0/tqdb.exe --help
```

## Layout

| Folder | Content |
|---|---|
| `Core/` | Readers: `.arz` (memory-mapped, parallel `ReadMany`), `.arc`, `Text_XX.arc`, game locator, `.tex` conversion, display names. `Contracts.cs` and `Factories.cs` are the only surface the CLI layer uses. |
| `Commands/` | `ArgParser` (sub-command, `--long`/`-s`, `--opt=value`, repeatable options, `--`, generated help, usage errors = exit 2), `CliContext` (global options, lazily opened game data, outputs) and one class per command. |
| `Query/` | `Pattern` (glob / `/regex/` to cached compiled `Regex`), `WhereClause`, `Projection`, `RecordQuery` (table filters before decompression, value filters on worker threads), `RecordSearch` (plan, run, sort, limit, follow), `ArchiveSelection`. |
| `Export/` | Streaming sinks: `JsonRecordSink` (`Utf8JsonWriter`, json/jsonl), `TsvRecordSink` (wide or long format), `DbrRecordSink` (stream or one file per record), `TableSink` (flat rows), `PathSanitizer`. |

## Adding a command
1. Implement `ICommand` in `Commands/` (declare options in `Spec`; help is generated from it).
2. Register it in `Program.CreateCommands()`.
3. Document it in `documentation/DBCLI.md`.
4. Test the logic (not the wiring) in `src/tests/TQVaultAE.DbCli.Tests/Cli/`, with the in-memory fakes of `CliFakes.cs`.
