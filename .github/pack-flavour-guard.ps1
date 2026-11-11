<#
.SYNOPSIS
    The os-matrix pack job's flavour guard: each flavour push-nuget.ps1 packs builds at the architecture of its
    GOOS/GOARCH pair, linux-arm64 at linux/arm64 and every other RID at amd64 (seat S6, DESIGN-linux-arm64-flavour.md
    section 3; COORD ruling 2026-11-10).

.DESCRIPTION
    push-nuget.ps1 builds one flavour per pair of its $ridFlavors map, passing -p:GoTargetOS and -p:GoTargetArch, so a
    pair at the wrong architecture packs a wrong package that compiles: arm64 code under an x64 RID, or the linux/amd64
    flavour's x64 system calls under linux-arm64. This guard reads the map from push-nuget.ps1 itself and refuses:
    - a value that is not a GOOS/GOARCH pair (the map before S6 named a GOOS alone, and its builds took the
      architecture from the environment);
    - no linux-arm64 entry, or linux-arm64 at any pair but linux/arm64;
    - any other RID at an architecture other than amd64 (both Mac RIDs ship the darwin/amd64 bytes).
    Then, for each distinct pair, .github/flavour-probe.ps1 reads from MSBuild's own evaluation that internal/goarch
    compiles at that pair's architecture when given the same -p:GoTargetOS and -p:GoTargetArch the pack passes.
    -NoProbe stops after the map (the suite src/tools/pushnuget/Test-PackFlavourGuard.ps1 runs it that way).
#>
[CmdletBinding()]
param(
    [string] $PushNuget = (Join-Path $PSScriptRoot '../src/push-nuget.ps1'),
    [switch] $NoProbe
)

$ErrorActionPreference = 'Stop'

$text = [System.IO.File]::ReadAllText($PushNuget)
$block = [regex]::Match($text, '(?s)\$ridFlavors = \[ordered\]@\{(.*?)\}')
if (-not $block.Success) { throw "pack flavour guard: no `$ridFlavors = [ordered]@{ ... } map in $PushNuget" }
$map = [ordered]@{}
foreach ($m in [regex]::Matches($block.Groups[1].Value, "'(?<rid>[^']+)'\s*=\s*'(?<flavour>[^']*)'")) { $map[$m.Groups['rid'].Value] = $m.Groups['flavour'].Value }
if ($map.Count -eq 0) { throw "pack flavour guard: the `$ridFlavors map in $PushNuget has no entries" }

if (-not $map.Contains('linux-arm64')) { throw 'pack flavour guard: push-nuget.ps1 packs no linux-arm64 flavour (no linux-arm64 entry, at linux/arm64, in its map)' }
foreach ($rid in $map.Keys) {
    $flavour = $map[$rid]
    if ($flavour -notmatch '^(?<goos>[a-z0-9]+)/(?<arch>[a-z0-9]+)$') {
        throw "pack flavour guard: $rid maps to '$flavour', not a GOOS/GOARCH pair, so its build would take the architecture from the environment"
    }
    $want = if ($rid -ceq 'linux-arm64') { 'linux/arm64' } else { "$($Matches['goos'])/amd64" }
    if ($flavour -cne $want) { throw "pack flavour guard: $rid packs at $flavour, not ${want}: linux-arm64 packs at linux/arm64 and every other RID at amd64" }
}
foreach ($rid in $map.Keys) { Write-Host "==> pack flavour guard: $rid packs at $($map[$rid])" }

if ($NoProbe) { return }
foreach ($pair in @($map.Values | Select-Object -Unique)) {
    $goos, $arch = $pair -split '/', 2
    & (Join-Path $PSScriptRoot 'flavour-probe.ps1') -Expected $arch -GoTargetOS $goos -GoTargetArch $arch -Label "the $pair pack flavour"
}
