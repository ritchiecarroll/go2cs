#Requires -Version 7
<#
.SYNOPSIS
    Gate for go.lib's go.lib.symbols.targets: a consumer's dependency symbol files survive a SECOND single-file publish.

.DESCRIPTION
    Restores the PublishSymbols fixture from ONE feed into a FRESH package cache, then publishes it single-file and
    self-contained for the host's RID into ONE fixed folder, twice, with nothing changed between (the shape of a
    converted executable's publish profile). After each publish it counts the .pdb files beside the host and runs it.
    The program prints a frame of its own referenced library (lib/), whose file:line comes from that library's .pdb.

      GREEN      both publishes leave PublishSymbolsLib.pdb beside the host and the frame reads "Where.cs:<line>".
      CONTROL    with -p:GoKeepSymbolsLoose=false (the target off) the SECOND publish loses PublishSymbolsLib.pdb and
                 the frame reads ":0" -- the SDK's own behaviour, and the proof that the arm can fail. Measured
                 2026-10-05 with no go.lib at all: a plain single-file app's second unchanged publish keeps App.pdb
                 and deletes its library's .pdb (the bundle step is skipped; only the bundler names that file as
                 published, and the incremental publish clean removes it as an orphan).

    Exit 0 only when both arms read as expected. Nothing outside a temporary work directory is written.

.PARAMETER Version
    The go.lib package version to restore (the fixture's GoPackageVersion).

.PARAMETER Source
    The feed: a local folder of .nupkg files (a pack rehearsal's output) or a URL. Default nuget.org.

.PARAMETER TargetsFile
    PRE-PACK ONLY: import this go.lib.targets explicitly (it imports go.lib.symbols.targets beside it), to test the
    working-tree files against a go.lib that does not carry them yet.

.PARAMETER FallbackFolder
    Optional read-only package folder for the SDK's own packs (the self-contained runtime pack), so a box with no
    network can publish. go.lib must still come from -Source: the gate refuses a go.lib resolved anywhere else.

.EXAMPLE
    pwsh ./test-publish-symbols.ps1 -Version 1.24.13.3 -Source <feed> -TargetsFile ../../../core/golib/buildTransitive/go.lib.targets
#>
param(
    [Parameter(Mandatory)] [string] $Version,
    [string] $Source = 'https://api.nuget.org/v3/index.json',
    [string] $TargetsFile,
    [string] $FallbackFolder,
    [switch] $KeepWork
)

$ErrorActionPreference = 'Stop'

$fixture = $PSScriptRoot
$work = Join-Path ([System.IO.Path]::GetTempPath()) ("publish-symbols-" + [System.Guid]::NewGuid().ToString('N').Substring(0, 8))
New-Item -ItemType Directory -Path $work | Out-Null
Copy-Item (Join-Path $fixture 'PublishSymbols.csproj'), (Join-Path $fixture 'Program.cs') $work
Copy-Item -Recurse (Join-Path $fixture 'lib') (Join-Path $work 'lib')

if (Test-Path $Source) { $Source = (Resolve-Path $Source).Path }

$fallback = ''
if ($FallbackFolder) {
    $fallback = @"
  <fallbackPackageFolders>
    <add key="sdk-packs" value="$((Resolve-Path $FallbackFolder).Path)" />
  </fallbackPackageFolders>
"@
}

# ONE source and a cache nothing else has touched: a green read must come from the package under test.
@"
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <clear />
    <add key="under-test" value="$Source" />
  </packageSources>
$fallback
</configuration>
"@ | Set-Content -Path (Join-Path $work 'nuget.config') -Encoding utf8

$env:NUGET_PACKAGES = Join-Path $work 'packages'
$env:NUGET_HTTP_CACHE_PATH = Join-Path $work 'http-cache'
$env:DOTNET_CLI_TELEMETRY_OPTOUT = '1'

$rid = [System.Runtime.InteropServices.RuntimeInformation]::RuntimeIdentifier
$exe = if ($IsWindows) { 'PublishSymbols.exe' } else { 'PublishSymbols' }
$common = @("-p:GoPackageVersion=$Version", '-r', $rid, '-p:PublishSingleFile=true', '--self-contained', '-nologo', '-v:m')
if ($TargetsFile) { $common += "-p:GoLibTargets=$((Resolve-Path $TargetsFile).Path)" }
$project = Join-Path $work 'PublishSymbols.csproj'

function Read-Publish([string] $Out) {
    $pdbs = @(Get-ChildItem $Out -Filter '*.pdb' -File -ErrorAction SilentlyContinue | ForEach-Object Name)
    $line = (& (Join-Path $Out $exe) 2>&1 | ForEach-Object { "$_" } | Select-String 'PUBLISH-SYMBOLS:' | Select-Object -First 1)
    return [PSCustomObject]@{ Pdbs = $pdbs; Line = "$line" }
}

function Invoke-Arm([string] $Name, [string[]] $Extra, [bool] $ExpectKept) {
    $out = Join-Path $work "out-$($Name -replace '\W', '')"
    Remove-Item -Recurse -Force (Join-Path $work 'bin'), (Join-Path $work 'obj'), (Join-Path $work 'lib/bin'), (Join-Path $work 'lib/obj'), $out -ErrorAction SilentlyContinue
    $reads = @()
    foreach ($pass in 1, 2) {
        $log = & dotnet publish $project -c Release -o $out @common @Extra 2>&1 | ForEach-Object { "$_" }
        if ($LASTEXITCODE -ne 0) { $log | Select-String ' error ' | Select-Object -First 5 | ForEach-Object { Write-Host "    $_" }; return "FAIL ($Name): publish $pass did not succeed" }
        $read = Read-Publish $out
        Write-Host "    $Name publish ${pass}: .pdb beside the host = $($read.Pdbs -join ', ') :: $($read.Line)"
        $reads += $read
    }

    if (-not (Test-Path (Join-Path $work 'packages/go.lib'))) { return "FAIL ($Name): go.lib did not resolve from the feed under test" }

    $first, $second = $reads
    if (-not ($first.Pdbs -contains 'PublishSymbolsLib.pdb') -or $first.Line -notmatch 'Where\.cs:[1-9]') {
        return "FAIL ($Name): the FIRST publish already lacks the library's symbols ($($first.Line)); the fixture measures nothing"
    }
    $kept = ($second.Pdbs -contains 'PublishSymbolsLib.pdb') -and ($second.Line -match 'Where\.cs:[1-9]')
    if ($ExpectKept -and -not $kept) { return "FAIL ($Name): the second publish lost the library's symbol file ($($second.Line))" }
    if (-not $ExpectKept -and $kept) { return "FAIL ($Name): with the target off the second publish still kept the symbol file; this arm cannot fail" }
    return "PASS ($Name)"
}

Write-Host "Publish symbols gate: go.lib $Version from $Source ($rid)"
$verdicts = @()
$verdicts += Invoke-Arm 'GREEN' @() $true
$verdicts += Invoke-Arm 'CONTROL GoKeepSymbolsLoose=false' @('-p:GoKeepSymbolsLoose=false') $false

$verdicts | ForEach-Object { Write-Host $_ }
if (-not $KeepWork) { Remove-Item -Recurse -Force $work -ErrorAction SilentlyContinue }
if (@($verdicts | Where-Object { $_ -like 'FAIL*' }).Count -gt 0) { exit 1 }
exit 0
