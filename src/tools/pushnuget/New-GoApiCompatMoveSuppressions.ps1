<#
.SYNOPSIS
    Writes the CompatibilitySuppressions.xml of every packed go.* package whose public methods moved into a class
    marked [GoPromotedHost] since its baseline, and reports every other member the package lost.

.DESCRIPTION
    Run it over the merged packages of a pack-only run (push-nuget.ps1 -VersionSuffix local.1 -OutDir <dir>), in the
    tree that packed them. For each package it downloads the baseline the guard will compare with (the highest
    published version below -ReleaseVersion on its line), reads both with GoApiCompatMoves.psm1, and writes the
    package's entries to src/core/<its directory>/CompatibilitySuppressions.xml: one CP0002 per moved member per asset.

    Nothing is written for a member that left any other way. Each is listed with its reason, and the script exits 1:
    the guard will read it red, as it should, until someone announces it or puts it back.

    It writes only the files of this one kind of break. A package that already carries a suppression file is refused
    unless -Force is given, because that file's other entries would be lost.

    The guard is the judge of the result, not this script: pack again and read "Binary compatibility: CLEAN". An entry
    the guard does not need reads red there as unnecessary, and a break with no entry reads red by name.

.PARAMETER PackageDir
    The directory of merged .nupkg files (its _flavors subdirectory is not read).

.PARAMETER ReleaseVersion
    The release the packages stand for, such as 1.26.11.1. A rehearsal packs at the last published build, so its own
    version would pick the wrong baseline (GoApiCompat.psm1, Invoke-GoApiCompatGuard).

.PARAMETER Comment
    The header of each file: what announced the break, and when the file is to be deleted.

.PARAMETER ReportOnly
    Reads and reports, and writes no file.
#>
#Requires -Version 7.0
[CmdletBinding()]
param(
    [Parameter(Mandatory)][string]$PackageDir,
    [Parameter(Mandatory)][string]$ReleaseVersion,
    [Parameter(Mandatory)][string]$Comment,
    [string]$FlatContainer = 'https://api.nuget.org/v3-flatcontainer/',
    [string]$CoreDir,
    [switch]$ReportOnly,
    [switch]$Force
)
$ErrorActionPreference = 'Stop'
Set-StrictMode -Version 2
Import-Module (Join-Path $PSScriptRoot 'GoApiCompat.psm1') -Force
Import-Module (Join-Path $PSScriptRoot 'GoApiCompatMoves.psm1') -Force
Add-Type -AssemblyName System.IO.Compression
Add-Type -AssemblyName System.IO.Compression.FileSystem
Add-Type -AssemblyName System.Net.Http

if (-not $CoreDir) { $CoreDir = Join-Path $PSScriptRoot '../../core' }
$core = [System.IO.Path]::GetFullPath($CoreDir).TrimEnd('\', '/')

# Each project directory under src/core by the package id push-nuget.ps1 gives it: the map is built from the
# directories, because an id cannot say which of its dots were separators (Get-GoApiCompatSuppressionMap).
$directoryOf = @{}
foreach ($project in @(Get-ChildItem -LiteralPath $core -Recurse -File -Filter '*.csproj')) {
    $rel = $project.Directory.FullName.Substring($core.Length).TrimStart('\', '/') -replace '[\\/]', '.'
    if ($rel -match '(^|\.)(bin|obj)(\.|$)') { continue }
    $id = if ($rel -ceq 'golib') { 'go.lib' } else { "go.$rel" }
    if (-not $directoryOf.ContainsKey($id)) { $directoryOf[$id] = $project.Directory.FullName }
}

function Get-Identity([string]$Package) {
    $zip = [System.IO.Compression.ZipFile]::OpenRead($Package)
    try {
        $entry = @($zip.Entries | Where-Object { $_.FullName -notmatch '[\\/]' -and $_.FullName.EndsWith('.nuspec') })[0]
        $reader = New-Object System.IO.StreamReader($entry.Open())
        try { $xml = [xml]$reader.ReadToEnd() } finally { $reader.Dispose() }
        [pscustomobject]@{ Id = "$($xml.package.metadata.id)"; Version = "$($xml.package.metadata.version)" }
    }
    finally { $zip.Dispose() }
}

$work = Join-Path ([System.IO.Path]::GetTempPath()) ('go2cs-apicompat-moves-' + [guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory $work | Out-Null
$client = New-Object System.Net.Http.HttpClient
$client.Timeout = [TimeSpan]::FromMinutes(5)
$flat = $FlatContainer.TrimEnd('/') + '/'

$read = 0; $noBaseline = 0; $files = 0; $entryCount = 0
$byAsset = @{}
$refusedLines = New-Object System.Collections.Generic.List[string]
$problems = New-Object System.Collections.Generic.List[string]

try {
    foreach ($package in @(Get-ChildItem -LiteralPath $PackageDir -File -Filter '*.nupkg' | Sort-Object Name)) {
        $identity = Get-Identity $package.FullName
        $published = @(Get-GoApiCompatPublishedVersions $client $flat $identity.Id)
        $baselineVersion = Get-GoApiCompatBaselineVersion -Published $published -Current $ReleaseVersion
        if (-not $baselineVersion) { $noBaseline++; continue }

        $baseline = Save-GoApiCompatBaseline $client $flat $identity.Id $baselineVersion (Join-Path $work '_baselines')
        $found = Get-GoApiCompatPackageMoves -Baseline $baseline -Current $package.FullName -WorkDir $work
        $read++
        foreach ($refusal in $found.Refused) { $refusedLines.Add("$($identity.Id): $($refusal.Asset): $($refusal.Target) $($refusal.Reason) (baseline $baselineVersion)") }
        Remove-Item -LiteralPath $baseline -Force
        if (@($found.Entries).Count -eq 0) { continue }

        foreach ($entry in $found.Entries) { $byAsset[$entry.Asset -replace '/[^/]+$', ''] = 1 + [int]$byAsset[$entry.Asset -replace '/[^/]+$', ''] }
        $entryCount += @($found.Entries).Count
        $files++

        if (-not $directoryOf.ContainsKey($identity.Id)) { $problems.Add("$($identity.Id): no project directory under $core has this package id; its $(@($found.Entries).Count) entries were not written"); continue }
        $path = Join-Path $directoryOf[$identity.Id] 'CompatibilitySuppressions.xml'
        if ((Test-Path -LiteralPath $path) -and -not $Force -and -not $ReportOnly) { $problems.Add("$($identity.Id): $path exists; its entries would be lost (pass -Force to replace it)"); continue }
        if (-not $ReportOnly) { Write-GoApiCompatMoveSuppressions -Entries $found.Entries -Path $path -Comment $Comment }
        Write-Host ("  {0,-48} {1,5} entr{2}" -f $identity.Id, @($found.Entries).Count, $(if (@($found.Entries).Count -eq 1) { 'y' } else { 'ies' }))
    }
}
finally {
    $client.Dispose()
    Remove-Item -LiteralPath $work -Recurse -Force -ErrorAction SilentlyContinue
}

Write-Host ("Moves: {0} package(s) read against a baseline below {1}, {2} with none; {3} file(s){4}, {5} entr(ies)" -f `
    $read, $ReleaseVersion, $noBaseline, $files, $(if ($ReportOnly) { ' would be written' } else { ' written' }), $entryCount)
foreach ($folder in @($byAsset.Keys | Sort-Object)) { Write-Host ("  {0,-40} {1,6}" -f $folder, $byAsset[$folder]) }
foreach ($line in $problems) { Write-Host "PROBLEM: $line" -ForegroundColor Red }
if ($refusedLines.Count) {
    Write-Host "REFUSED: $($refusedLines.Count) removed member(s) are not moves and got no entry; the guard reads each one red:" -ForegroundColor Red
    foreach ($line in $refusedLines) { Write-Host "  $line" }
}
if ($problems.Count -or $refusedLines.Count) { exit 1 }
exit 0
