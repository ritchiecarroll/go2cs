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

    The instruments a leg runs (run-behavioral.ps1, CNR, the sweep) dot-source src/_paths.ps1, which can set
    GoTargetArch itself where the job's environment leaves it empty. So, read from the environment, the probe also asks
    _paths.ps1, in a child process, which arch those instruments will build at, and fails by name when it is not the
    expected one: a leg whose instruments build one flavour while this probe reads another is two legs under one label
    (D2's finding, COORD 2026-11-25: on osx-arm64 the behavioral steps built darwin/arm64 while the probe read amd64).

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

# What the instruments build: _paths.ps1's own answer in this environment, read in a child of the same edition so its
# pins never touch this process. Only when the flavour comes from the environment: an explicit -GoTargetArch (push-nuget's
# per-pair reading) is passed to every build it describes, so no _paths.ps1 pin reaches those builds.
$instrumentArch = $null
if (-not $GoTargetArch) {
    $paths = Join-Path $PSScriptRoot '../src/_paths.ps1'
    # By name, not by this process's path: a pwsh installed as a dotnet global tool runs as the dotnet host.
    $shell = if ($PSVersionTable.PSEdition -eq 'Core') { 'pwsh' } else { 'powershell' }
    $read = & $shell -NoProfile -NonInteractive -ExecutionPolicy Bypass -Command ". '$($paths -replace "'", "''")'; 'GoTargetArch=' + [string]`$env:GoTargetArch" 2>&1 | Out-String
    if ($LASTEXITCODE -ne 0 -or $read -notmatch '(?m)^GoTargetArch=([a-z0-9]*)\s*$') {
        throw "${Label}: the flavour probe could not read the arch src/_paths.ps1 binds (exit $LASTEXITCODE):`n$read"
    }
    $instrumentArch = if ($Matches[1]) { $Matches[1] } else { 'amd64' }
    $line = "flavour probe, ${Label}: src/_paths.ps1 binds the instruments to GoTargetArch '$instrumentArch'; expected $Expected"
    Write-Host "==> $line"
    if ($env:GITHUB_STEP_SUMMARY) { "    $line" | Add-Content -Path $env:GITHUB_STEP_SUMMARY }
}

if ($instrumentArch -and $instrumentArch -ne $Expected) {
    throw "${Label}: src/_paths.ps1 binds the instruments to GOARCH=$instrumentArch, not $Expected, while this probe reads the job's binding. The leg's instruments would build another flavour than the one its results are read under, so this stops here."
}

if ($compiled.Count -ne 1 -or $compiled[0] -ne $Expected) {
    throw "${Label}: the flavour compiles GOARCH=$(if ($compiled.Count) { $compiled -join '+' } else { 'nothing' }), not $Expected. Its results would be read under the wrong architecture, so this stops here."
}
