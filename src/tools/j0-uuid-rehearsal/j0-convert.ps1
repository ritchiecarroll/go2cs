<#
.SYNOPSIS
    J0 step 1: convert google/uuid v1.6.0 and validate it against its own Go tests (docs/phase4/SIZING-j0-uuid.md).

.DESCRIPTION
    Runs `go2cs -tests -test-action all` over the module as the Go module cache holds it. The conversion's output
    goes to OutRoot, never into the source. It builds against the go2cs tree named by -Go2csPath (its core\golib
    and the converted standard library, as project references).

    The two traps the sizing found are REFUSALS here, by name:
      - an output under GOMODCACHE. The converter's own default (no output path) is the input directory, which
        would write .cs, .csproj, bin and obj into the module cache;
      - a missing or wrong -Go2csPath. Outside a checkout the converter cannot locate its runtime root, and every
        $(go2csPath)core reference dangles.

    The module's LICENSE is placed beside the converted project before conversion, which is the converter's own
    documented route ("add a LICENSE beside the project"): the packed package then carries Google's licence text.

    Use a converter built with R's M1 (module proof pages): its comparison record is keyed by the full import path,
    and a third-party proof page is written beside the conversion, never into a checkout's roster.
#>
#Requires -Version 5.1
[CmdletBinding()]
param(
    [Parameter(Mandatory)][string]$Converter,
    [string]$Go2csPath,
    [Parameter(Mandatory)][string]$OutRoot,
    [string]$ModulePath = 'github.com/google/uuid',
    [string]$ModuleVersion = 'v1.6.0',
    [string]$TestTimeout = '20m'
)

$ErrorActionPreference = 'Stop'

function Get-FullPath([string]$Path) { [System.IO.Path]::GetFullPath($Path).TrimEnd('\', '/') }

function Test-PathUnder([string]$Child, [string]$Parent) {
    $c = (Get-FullPath $Child) + [System.IO.Path]::DirectorySeparatorChar
    $p = (Get-FullPath $Parent) + [System.IO.Path]::DirectorySeparatorChar
    return $c.StartsWith($p, [StringComparison]::OrdinalIgnoreCase)
}

if (-not $Go2csPath) {
    throw "REFUSED: -Go2csPath is required. Outside a go2cs checkout the converter cannot locate its runtime root, and every `$(go2csPath)core reference in the converted project would dangle. Pass the checkout's src directory."
}
if (-not (Test-Path (Join-Path $Go2csPath 'core\golib\golib.csproj'))) {
    throw "REFUSED: -Go2csPath '$Go2csPath' is not a go2cs root (no core\golib\golib.csproj under it). Pass the checkout's src directory."
}
if (-not (Test-Path $Converter)) { throw "Converter not found: $Converter" }

$modCache = (& go env GOMODCACHE).Trim()
if (-not $modCache) { throw "go env GOMODCACHE returned nothing -- is go on PATH?" }

$moduleDir = Join-Path $modCache (($ModulePath -replace '/', '\') + "@$ModuleVersion")
if (-not (Test-Path (Join-Path $moduleDir 'go.mod'))) {
    throw "The module is not in the module cache: $moduleDir. Fetch it first (go mod download $ModulePath@$ModuleVersion)."
}

if (Test-PathUnder $OutRoot $modCache) {
    throw "REFUSED: -OutRoot '$OutRoot' is under GOMODCACHE ($modCache). The conversion must never write into the module cache."
}
if ((Get-FullPath $OutRoot) -ieq (Get-FullPath $moduleDir)) {
    throw "REFUSED: -OutRoot is the module directory itself."
}

$projectName = $ModulePath -replace '/', '.'
$outDir = Join-Path $OutRoot $projectName
New-Item -ItemType Directory -Force $outDir | Out-Null

$license = Join-Path $moduleDir 'LICENSE'
if (Test-Path $license) {
    Copy-Item $license (Join-Path $outDir 'LICENSE') -Force
    Write-Host "==> Placed the module's LICENSE beside the converted project"
}
else {
    Write-Warning "The module has no LICENSE file; the package will carry none."
}

Write-Host "==> Converting $ModulePath@$ModuleVersion"
Write-Host "    source    : $moduleDir"
Write-Host "    output    : $outDir"
Write-Host "    go2cspath : $Go2csPath"
Write-Host "    converter : $Converter"

& $Converter -tests -test-action all -test-config Release -test-timeout $TestTimeout -go2cspath $Go2csPath $moduleDir $outDir
$rc = $LASTEXITCODE

$record = Join-Path $outDir 'go2cs_test_comparison.json'
if (Test-Path $record) {
    $comparison = Get-Content $record -Raw | ConvertFrom-Json
    Write-Host "==> Comparison record: package '$($comparison.package)', status '$($comparison.status)', matched $($comparison.matched)"
}
else {
    Write-Host "==> No comparison record: the converted tests did not run (see the converter output above)."
}

Write-Host "==> Converter exit code $rc"
exit $rc
