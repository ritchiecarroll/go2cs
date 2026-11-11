<#
.SYNOPSIS
    Reads the GOARCH a corpus flavour compiles at, from MSBuild's own evaluation, and fails when it is not the
    expected one (seat S5b, DESIGN-linux-arm64-flavour.md section 6).

.DESCRIPTION
    The witness is internal/goarch: its Compile items carry exactly one zgoarch_<GOARCH>.cs, the file that defines
    the GOARCH every converted package reads. So the item set IS the flavour's architecture, and it is read the way
    the build reads it: from this process's environment (GoTargetOS and GoTargetArch, which MSBuild maps onto the
    properties) plus any -GoTargetOS and -GoTargetArch given here, as push-nuget passes them per flavour pair.

    An arch-split csproj defaults an EMPTY GoTargetArch to amd64. So a leg that should build linux/arm64 and lost its
    binding still compiles cleanly, at amd64, and reports its results under the arm64 label. This probe is what
    turns that into a failure.

.PARAMETER Expected
    The GOARCH the flavour must compile at: amd64 or arm64.

.PARAMETER GoTargetOS
    The flavour to evaluate, passed as -p:GoTargetOS. Omit it to evaluate the one the environment binds.

.PARAMETER GoTargetArch
    The architecture to evaluate, passed as -p:GoTargetArch, as push-nuget passes one per flavour pair (seat S6).
    Omit it to evaluate the one the environment binds.

.PARAMETER Label
    What the reading is for, in the failure and the summary line.
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory)] [ValidateSet('amd64', 'arm64')] [string] $Expected,
    [string] $GoTargetOS = '',
    [ValidateSet('', 'amd64', 'arm64')] [string] $GoTargetArch = '',
    [Parameter(Mandatory)] [string] $Label
)

$ErrorActionPreference = 'Stop'

$project = Join-Path $PSScriptRoot '../src/core/internal/goarch/internal.goarch.csproj'
$arguments = @('msbuild', $project, '-getProperty:GoTargetArch', '-getItem:Compile')
if ($GoTargetOS) { $arguments += "-p:GoTargetOS=$GoTargetOS" }
if ($GoTargetArch) { $arguments += "-p:GoTargetArch=$GoTargetArch" }

$text = & dotnet @arguments 2>&1 | Out-String
if ($LASTEXITCODE -ne 0) { throw "${Label}: the flavour probe could not evaluate ${project} (exit $LASTEXITCODE):`n$text" }

$evaluation = $text | ConvertFrom-Json
$property = "$($evaluation.Properties.GoTargetArch)"
$compiled = @($evaluation.Items.Compile | ForEach-Object { if ("$($_.Identity)" -match 'zgoarch_([a-z0-9]+)\.cs$') { $Matches[1] } } | Sort-Object -Unique)

$flavour = if ($GoTargetOS) { $GoTargetOS } elseif ($env:GoTargetOS) { $env:GoTargetOS } else { '(the csproj default)' }
$line = "flavour probe, ${Label}: GoTargetOS $flavour, GoTargetArch evaluates to '$property', internal/goarch compiles zgoarch for [$($compiled -join ', ')]; expected $Expected"
Write-Host "==> $line"
if ($env:GITHUB_STEP_SUMMARY) { "    $line" | Add-Content -Path $env:GITHUB_STEP_SUMMARY }

if ($compiled.Count -ne 1 -or $compiled[0] -ne $Expected) {
    throw "${Label}: the flavour compiles GOARCH=$(if ($compiled.Count) { $compiled -join '+' } else { 'nothing' }), not $Expected. Its results would be read under the wrong architecture, so this stops here."
}
