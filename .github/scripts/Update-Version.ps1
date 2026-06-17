param(
    [Parameter(Mandatory=$true)]
    [ValidateSet("Sync")]
    [string]$VersionType,

    [Parameter(Mandatory=$false)]
    [string]$SolutionRoot = $PSScriptRoot + "\..\..",

    [Parameter(Mandatory=$false)]
    [string]$VersionFile = "$SolutionRoot\version-info.json"
)

Write-Host "Version Management Script"
Write-Host "======================="
Write-Host "Version Type: $VersionType"
Write-Host "Solution Root: $SolutionRoot"
Write-Host "Version File: $VersionFile"
Write-Host ""

$versionInfo = @{}

if (Test-Path $VersionFile) {
    $jsonContent = Get-Content $VersionFile -Raw | ConvertFrom-Json
    $versionInfo = @{
        Major = $jsonContent.Major
        Minor = $jsonContent.Minor
        Build = $jsonContent.Build
        Revision = $jsonContent.Revision
    }
    Write-Host "Loaded version from file: $($versionInfo.Major).$($versionInfo.Minor).$($versionInfo.Build).$($versionInfo.Revision)"
} else {
    Write-Host "Error: version-info.json not found!"
    exit 1
}

$newVersion = "$($versionInfo.Major).$($versionInfo.Minor).$($versionInfo.Build).$($versionInfo.Revision)"
Write-Host "Syncing version: $newVersion"
Write-Host ""

$csprojFiles = @(
    "$SolutionRoot\src\TQVaultAE.Domain\TQVaultAE.Domain.csproj",
    "$SolutionRoot\src\TQVaultAE.Application\TQVaultAE.Application.csproj",
    "$SolutionRoot\src\TQVaultAE.Services\TQVaultAE.Services.csproj",
    "$SolutionRoot\src\TQVaultAE.Presentation\TQVaultAE.Presentation.csproj",
    "$SolutionRoot\src\TQVaultAE.Data\TQVaultAE.Data.csproj",
    "$SolutionRoot\src\TQVaultAE.Config\TQVaultAE.Config.csproj",
    "$SolutionRoot\src\TQVaultAE.Logs\TQVaultAE.Logs.csproj",
    "$SolutionRoot\src\TQVaultAE.GUI\TQVaultAE.GUI.csproj",
    "$SolutionRoot\src\TQSaveFilesExplorer\TQ.SaveFilesExplorer.csproj",
    "$SolutionRoot\src\ARZExplorer\ArzExplorer.csproj",
    "$SolutionRoot\src\TQVaultAE.Services.Win32\TQVaultAE.Services.Win32.csproj"
)

foreach ($file in $csprojFiles) {
    if (Test-Path $file) {
        $content = Get-Content $file -Raw -Encoding UTF8
        
        $originalContent = $content
        
        if ($content -match '<Version>.*</Version>') {
            $content = $content -replace '<Version>.*</Version>', "<Version>$newVersion</Version>"
            
            if ($content -ne $originalContent) {
                Set-Content -Path $file -Value $content -Encoding UTF8 -NoNewline
                Write-Host "Updated: $file"
            } else {
                Write-Host "Skipped (no changes): $file"
            }
        } else {
            Write-Host "Skipped (no <Version> tag): $file"
        }
    } else {
        Write-Host "File not found: $file"
    }
}

Write-Host ""
Write-Host "Version sync complete: $newVersion"
Write-Host ""
Write-Host "Files Updated:"
Write-Host "- 11 SDK-style csproj files (3 EXE + 8 DLL projects)"
Write-Host "- All projects now use SDK-style format with auto-generated assembly info"