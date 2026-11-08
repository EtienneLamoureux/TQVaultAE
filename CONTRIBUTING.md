# Contributing in TQVaultAE
- [Contributing in TQVaultAE](#contributing-in-tqvaultae)
  * [Prerequisite](#prerequisite)
  * [Game file formats](#game-file-formats)
  * [Driving the app from an AI agent](#driving-the-app-from-an-ai-agent)
  * [Style and format](#style-and-format)
  * [Contributions](#contributions)
    + [What to work on](#what-to-work-on)
      - [Note on unsolicited contributions](#note-on-unsolicited-contributions)
    + [Pull requests](#pull-requests)

## Prerequisite
This project use DDS to PNG conversion from nuget [Magick.NET-Q8-AnyCPU](https://github.com/dlemstra/Magick.NET) so you need to install [ImageMagick](https://imagemagick.org/script/download.php#gsc.tab=0) to resolve DLL during build.<br/>
The simple way to do that is to use command `winget install ImageMagick.Q8` before opening the solution.

## Game file formats
Working on the game file readers (`src/TQVaultAE.GameFiles`, used by `src/TQVaultAE.Data` and `src/TQVaultAE.DbCli`)? The `.arc`, `.arz`, text and `.tex` formats, the known pitfalls and the item stat randomization are documented in [Titan Quest Game File Formats](documentation/tq-file-formats.md).

## Driving the app from an AI agent
Debug builds start an [MCP](https://modelcontextprotocol.io) server inside TQVault, at `http://127.0.0.1:47017/mcp` (loopback only). It lets an AI agent such as Claude Code check UI changes on the running app without screenshots:
- read windows, vaults, characters and the items in each panel;
- answer dialogs;
- drag items through the panels' own code;
- capture a panel to PNG.

The repository's `.mcp.json` registers it as `tqvault`: start the Debug build, then open Claude Code at the repository root. Automation saves like a user does, so it works only in its own `claude-*` test vaults and characters. Release builds contain none of this. See [Automation endpoint](documentation/AUTOMATION.md).

## Style and format
TQVaultAE follows the standard C# format and import order. Refrain from submitting white-space changes mixed-in with your functional changes. 
**N.B.**: Braces are mandatory.

## Contributions
### What to work on
See our [list of outstanding issues](https://github.com/EtienneLamoureux/TQVaultAE/issues). It contains simple bugs, not so simple bugs and feature requests.

#### Note on unsolicited contributions
What if you want to work on something not in the list? No problem, we welcome any contribution to the project! With that said, please keep in mind that the community might suggest changes or refrain from accepting an unsolicited contribution if it strays too far from the goals of this project.

### Pull requests
Submit your pull request to the master branch of the [main fork](https://github.com/EtienneLamoureux/TQVaultAE). We consider any outstanding pull request ready to be merged in, so please make sure that the functionality is complete when submitting your change.  Furthermore, the commits will be squashed when merged and the main commit message will be re-written, so feel free to commit often.


