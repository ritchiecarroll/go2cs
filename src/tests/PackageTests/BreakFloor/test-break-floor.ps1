#Requires -Version 7
<#
.SYNOPSIS
    Gate for go.lib's break floor (src/core/golib/buildTransitive/go.lib.targets, GoCheckBreakFloor).

.DESCRIPTION
    Restores the BreakFloor fixture from ONE feed into a FRESH package cache, beside go.fmt at -Version, then checks:

      BELOW      hashset at -BelowHashSet, built on a go.* release below the floor, must FAIL the build with
                 GO2CS3002 naming the package. This is the S1 deprecation's mechanism
                 (docs/phase4/RECORD-nuget-sibling-pins.md, arm h1): without the floor it restores and builds clean,
                 then throws MissingMethodException at its first loop over a slice.
      AT         hashset at -AtHashSet, built on the floor release or later, builds and runs.
      OPT-OUT    BELOW with -p:GoAllowBelowBreakFloor=true builds, with GO2CS3002 as a warning; run, it throws
                 MissingMethodException, which shows the risk the warning names is real.

    Exit 0 only when every arm reads as expected. Nothing outside a temporary work directory is written.

.PARAMETER Version
    The go.* release to restore go.fmt at (the fixture's GoPackageVersion).

.PARAMETER BelowHashSet
    A nugetgo.github.com.ritchiecarroll.hashset version built below the floor.

.PARAMETER AtHashSet
    A nugetgo.github.com.ritchiecarroll.hashset version built on the floor release or later.

.PARAMETER Source
    The feed: a local folder of .nupkg files (a pack rehearsal's output) or a URL. Default nuget.org.

.PARAMETER TargetsFile
    PRE-PACK ONLY: import this go.lib.targets explicitly, to test the working-tree file against an
    already-published go.lib that does not carry the floor yet.

.EXAMPLE
    (hashset 1.0.0's fourth and fifth revisions, as nuget.org spells them, built on go.* 1.26.10.2 and 1.26.10.3)
    pwsh ./test-break-floor.ps1 -Version 1.26.10.3 -BelowHashSet <revision 4> -AtHashSet <revision 5> -TargetsFile ../../../core/golib/buildTransitive/go.lib.targets
#>
param(
    [Parameter(Mandatory)] [string] $Version,
    [Parameter(Mandatory)] [string] $BelowHashSet,
    [Parameter(Mandatory)] [string] $AtHashSet,
    [string] $Source = 'https://api.nuget.org/v3/index.json',
    [string] $TargetsFile,
    [switch] $KeepWork
)

$ErrorActionPreference = 'Stop'

$fixture = $PSScriptRoot
$work = Join-Path ([System.IO.Path]::GetTempPath()) ("break-floor-" + [System.Guid]::NewGuid().ToString('N').Substring(0, 8))
New-Item -ItemType Directory -Path $work | Out-Null
Copy-Item (Join-Path $fixture 'BreakFloor.csproj'), (Join-Path $fixture 'Program.cs') $work

if (Test-Path $Source) { $Source = (Resolve-Path $Source).Path }

# A feed of ONE source and a cache nothing else has touched: a read must come from the packages under test, never
# from a machine-wide cache or a second feed.
@"
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <clear />
    <add key="under-test" value="$Source" />
  </packageSources>
</configuration>
"@ | Set-Content -Path (Join-Path $work 'nuget.config') -Encoding utf8

$env:NUGET_PACKAGES = Join-Path $work 'packages'
$env:NUGET_HTTP_CACHE_PATH = Join-Path $work 'http-cache'
$env:DOTNET_CLI_TELEMETRY_OPTOUT = '1'

$common = @("-p:GoPackageVersion=$Version", '-nologo', '-v:m')
if ($TargetsFile) { $common += "-p:GoLibTargets=$((Resolve-Path $TargetsFile).Path)" }
$project = Join-Path $work 'BreakFloor.csproj'
$hashsetId = 'nugetgo.github.com.ritchiecarroll.hashset'

# Builds one arm; returns the build log and whether it built. Each arm starts from no bin/ or obj/, so its restore
# and build read only its own HashSetVersion.
function Invoke-Build([string] $HashSet, [string[]] $Extra) {
    Remove-Item -Recurse -Force (Join-Path $work 'bin'), (Join-Path $work 'obj') -ErrorAction SilentlyContinue
    $log = & dotnet build $project -c Debug @common "-p:HashSetVersion=$HashSet" @Extra 2>&1 | ForEach-Object { "$_" }
    return @{ Log = $log; Built = ($LASTEXITCODE -eq 0) }
}

function Invoke-Run([string] $HashSet, [string[]] $Extra) {
    $run = & dotnet run --project $project -c Debug --no-build @common "-p:HashSetVersion=$HashSet" @Extra 2>&1 | ForEach-Object { "$_" }
    return @{ Log = $run; Code = $LASTEXITCODE }
}

function Show([string[]] $Lines, [string] $Pattern) {
    $Lines | Select-String $Pattern | Select-Object -First 5 | ForEach-Object { Write-Host "    $_" }
}

Write-Host "Break floor gate: go.fmt $Version, hashset $BelowHashSet (below) and $AtHashSet (at), from $Source"
$verdicts = @()

$b = Invoke-Build $BelowHashSet @()
Show $b.Log 'GO2CS3002'
if ($b.Built) { $verdicts += "FAIL (BELOW): hashset $BelowHashSet built; the floor did not stop it" }
elseif (-not ($b.Log -match "error GO2CS3002: $([regex]::Escape($hashsetId)) $([regex]::Escape($BelowHashSet)) was built on go\.\* ")) { Show $b.Log ' error '; $verdicts += "FAIL (BELOW): the build failed, but not with GO2CS3002 naming $hashsetId $BelowHashSet" }
else { $verdicts += "PASS (BELOW): GO2CS3002 named $hashsetId $BelowHashSet" }

$a = Invoke-Build $AtHashSet @()
if (-not $a.Built) { Show $a.Log ' error '; $verdicts += "FAIL (AT): hashset $AtHashSet did not build" }
elseif ($a.Log -match 'GO2CS3002') { Show $a.Log 'GO2CS3002'; $verdicts += "FAIL (AT): GO2CS3002 fired for a package at the floor" }
else {
    $r = Invoke-Run $AtHashSet @()
    $r.Log | ForEach-Object { Write-Host "    $_" }
    if ($r.Code -ne 0 -or -not ($r.Log -match 'BREAK-FLOOR: true 3')) { $verdicts += "FAIL (AT): the fixture did not run clean (exit $($r.Code))" }
    else { $verdicts += "PASS (AT): built and ran" }
}

$o = Invoke-Build $BelowHashSet @('-p:GoAllowBelowBreakFloor=true')
Show $o.Log 'GO2CS3002'
if (-not $o.Built) { Show $o.Log ' error '; $verdicts += "FAIL (OPT-OUT): did not build with GoAllowBelowBreakFloor=true" }
elseif (-not ($o.Log -match "warning GO2CS3002: $([regex]::Escape($hashsetId)) ")) { $verdicts += "FAIL (OPT-OUT): built, but without the GO2CS3002 warning" }
else {
    $r = Invoke-Run $BelowHashSet @('-p:GoAllowBelowBreakFloor=true')
    Show $r.Log 'MissingMethodException'
    if ($r.Code -eq 0 -or -not ($r.Log -match 'MissingMethodException')) { $verdicts += "FAIL (OPT-OUT): the run did not throw MissingMethodException (exit $($r.Code)); is $BelowHashSet below the floor?" }
    else { $verdicts += "PASS (OPT-OUT): built with the warning; the run threw MissingMethodException" }
}

$verdicts | ForEach-Object { Write-Host $_ }
if (-not $KeepWork) { Remove-Item -Recurse -Force $work -ErrorAction SilentlyContinue }
if (@($verdicts | Where-Object { $_ -like 'FAIL*' }).Count -gt 0) { exit 1 }
exit 0
