<#
.SYNOPSIS
    Publishes the release outputs of the TQVaultAE suite, each in its own folder under -OutputDir.

.DESCRIPTION
    Every output is a self-contained single file, so the target machine needs no .NET runtime.
    The native libraries stay next to the executable; no .pdb files are produced.
    The Windows applications (net10.0-windows) are published for win-x64 only: they do not run on Linux.
    tqdb (net10.0) is published twice, for win-x64 and linux-x64.

    Output folders: TQVaultAE, ARZExplorer, TQSaveFilesExplorer, tqdb-win-x64, tqdb-linux-x64.

.EXAMPLE
    pwsh -File .github/scripts/Publish-Release.ps1 -OutputDir publish
#>
param(
    [Parameter(Mandatory = $false)]
    [string]$OutputDir = "publish",

    [Parameter(Mandatory = $false)]
    [string]$Configuration = "Release"
)

$ErrorActionPreference = "Stop"
$root = Resolve-Path "$PSScriptRoot\..\.."

$windowsApps = @(
    @{ Name = "TQVaultAE";           Project = "src/TQVaultAE.GUI/TQVaultAE.GUI.csproj" }
    @{ Name = "ARZExplorer";         Project = "src/ARZExplorer/ArzExplorer.csproj" }
    @{ Name = "TQSaveFilesExplorer"; Project = "src/TQSaveFilesExplorer/TQ.SaveFilesExplorer.csproj" }
)

$cliRuntimes = @("win-x64", "linux-x64")

function Invoke-Publish([string]$Project, [string]$Output, [string[]]$Arguments)
{
    Write-Host "Publishing $Project -> $Output"
    dotnet publish (Join-Path $root $Project) --configuration $Configuration --output $Output @Arguments
    if ($LASTEXITCODE -ne 0) { throw "dotnet publish failed for $Project ($LASTEXITCODE)" }
}

# Self-contained single file: the runtime and the managed assemblies go inside the executable.
# Next to it stay:
# - the native libraries (Magick.NET's Magick.Native-*.dll): left out of the bundle, so nothing is extracted
#   to %TEMP% on first run;
# - content files such as UserConfig.xml and log4net.config.
# No .pdb files are produced: stack traces in the logs show the methods, without line numbers.
# (Symbols embedded in a bundled assembly would not give line numbers either: the runtime cannot read them there.)
$singleFile = @(
    "--self-contained", "true",
    "-p:PublishSingleFile=true",
    "-p:EnableCompressionInSingleFile=true",
    "-p:DebugType=none"
)

foreach ($app in $windowsApps)
{
    Invoke-Publish $app.Project (Join-Path $OutputDir $app.Name) (@("--runtime", "win-x64") + $singleFile)
}

foreach ($rid in $cliRuntimes)
{
    Invoke-Publish "src/TQVaultAE.DbCli/TQVaultAE.DbCli.csproj" (Join-Path $OutputDir "tqdb-$rid") (@("--runtime", $rid) + $singleFile)
}

Write-Host ""
Write-Host "Published outputs:"
Get-ChildItem $OutputDir -Directory | ForEach-Object {
    $size = (Get-ChildItem $_.FullName -Recurse -File | Measure-Object Length -Sum).Sum / 1MB
    Write-Host ("  {0,-22} {1,8:N1} MB" -f $_.Name, $size)
}
