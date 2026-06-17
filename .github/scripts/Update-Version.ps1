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

$buildPropsFile = "$SolutionRoot\Directory.Build.props"

if (Test-Path $buildPropsFile) {
    $content = Get-Content $buildPropsFile -Raw -Encoding UTF8
    
    $originalContent = $content
    
    if ($content -match '<Version[^>]*>.*</Version>') {
        $content = $content -replace '<Version[^>]*>.*</Version>', ('<Version Condition="''$(IsPackable)'' != ''false''">' + $newVersion + '</Version>')
        
        if ($content -ne $originalContent) {
            Set-Content -Path $buildPropsFile -Value $content -Encoding UTF8 -NoNewline
            Write-Host "Updated: $buildPropsFile"
        } else {
            Write-Host "Skipped (no changes): $buildPropsFile"
        }
    } else {
        Write-Host "Skipped (no <Version> tag): $buildPropsFile"
    }
} else {
    Write-Host "Error: Directory.Build.props not found at $buildPropsFile"
    exit 1
}

Write-Host ""
Write-Host "Version sync complete: $newVersion"
Write-Host ""
Write-Host "Files Updated:"
Write-Host "- 1 Directory.Build.props (version centralized for all projects)"
Write-Host "- All projects inherit version from single source"