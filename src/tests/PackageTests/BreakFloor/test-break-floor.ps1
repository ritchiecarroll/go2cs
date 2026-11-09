#Requires -Version 7
<#
.SYNOPSIS
    Gate for go.lib's break floor (src/core/golib/buildTransitive/go.lib.targets, GoCheckBreakFloor).

.DESCRIPTION
    Restores the BreakFloor fixture from ONE feed into a FRESH package cache, beside go.fmt at -Version, then checks:

      BELOW      hashset at -BelowHashSet, built on a go.* release below the floor, must FAIL the build with
                 GO2CS3002 naming the package, and leave bin/ empty: the floor stops the build before CoreCompile.
                 This is the S1 deprecation's mechanism (docs/phase4/RECORD-nuget-sibling-pins.md, arm h1): without
                 the floor it restores and builds clean, then throws MissingMethodException at its first loop over a
                 slice.
      AT         hashset at -AtHashSet, built on the floor release or later, builds and runs.
      OPT-OUT    BELOW with -p:GoAllowBelowBreakFloor=true builds, with GO2CS3002 as a warning; run, it throws
                 MissingMethodException, which shows the risk the warning names is real.
      LIBRARY    an application that references a library project, which references hashset at -BelowHashSet and
                 opts out: the library builds with the warning, and the application's own build FAILS with
                 GO2CS3002 naming hashset, which reached it through the ProjectReference, and leaves its bin/ empty.
      MULTI-TFM  BELOW with TargetFrameworks net10.0 and net10.0-windows: each inner build FAILS with GO2CS3002
                 naming hashset, and bin/ stays empty.

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
elseif (@(Get-ChildItem (Join-Path $work 'bin') -Recurse -File -ErrorAction SilentlyContinue).Count -gt 0) { $verdicts += "FAIL (BELOW): GO2CS3002 named $hashsetId, but bin/ holds files; the build did not stop" }
else { $verdicts += "PASS (BELOW): GO2CS3002 named $hashsetId $BelowHashSet, bin/ empty" }

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

# Arms that write their own projects next to the fixture's: the same feed and package cache, the same properties.
function Write-Project([string] $Path, [string] $Text) {
    New-Item -ItemType Directory -Force -Path (Split-Path $Path -Parent) | Out-Null
    Set-Content -Path $Path -Value $Text -Encoding utf8
}

$golibImport = '<Import Project="$(GoLibTargets)" Condition="''$(GoLibTargets)'' != ''''" />'
$libRoot = Join-Path $work 'library'
Write-Project (Join-Path $libRoot 'Lib/Lib.csproj') @"
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <GoAllowBelowBreakFloor>true</GoAllowBelowBreakFloor>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="go.fmt" Version="`$(GoPackageVersion)" />
    <PackageReference Include="$hashsetId" Version="`$(HashSetVersion)" />
  </ItemGroup>
  $golibImport
</Project>
"@
Write-Project (Join-Path $libRoot 'Lib/Lib.cs') 'namespace BreakFloorLib { public static class Entry { public static int Answer() => 42; } }'
Write-Project (Join-Path $libRoot 'App/App.csproj') @"
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net10.0</TargetFramework>
  </PropertyGroup>
  <ItemGroup>
    <ProjectReference Include="../Lib/Lib.csproj" />
  </ItemGroup>
  $golibImport
</Project>
"@
Write-Project (Join-Path $libRoot 'App/Program.cs') 'System.Console.WriteLine(BreakFloorLib.Entry.Answer());'
$l = & dotnet build (Join-Path $libRoot 'App/App.csproj') -c Debug @common "-p:HashSetVersion=$BelowHashSet" 2>&1 | ForEach-Object { "$_" }
$lBuilt = $LASTEXITCODE -eq 0
Show $l 'GO2CS3002'
$appErrors = @($l | Where-Object { $_ -match "error GO2CS3002: $([regex]::Escape($hashsetId)) $([regex]::Escape($BelowHashSet)) was built on go\.\* " -and $_ -match 'App\.csproj' })
$libWarnings = @($l | Where-Object { $_ -match "warning GO2CS3002: $([regex]::Escape($hashsetId)) " -and $_ -match 'Lib\.csproj' })
$appOutput = @(Get-ChildItem (Join-Path $libRoot 'App/bin') -Recurse -File -ErrorAction SilentlyContinue)
if ($lBuilt) { $verdicts += "FAIL (LIBRARY): the application built; the floor did not see hashset through the ProjectReference" }
elseif ($libWarnings.Count -eq 0) { Show $l ' error '; $verdicts += "FAIL (LIBRARY): the opted-out library did not warn with GO2CS3002" }
elseif ($appErrors.Count -eq 0) { Show $l ' error '; $verdicts += "FAIL (LIBRARY): the application failed, but not with GO2CS3002 naming $hashsetId $BelowHashSet" }
elseif ($appOutput.Count -gt 0) { $verdicts += "FAIL (LIBRARY): the application's bin/ holds $($appOutput.Count) file(s); the build did not stop" }
else { $verdicts += "PASS (LIBRARY): the library warned; the application failed with GO2CS3002 naming $hashsetId, bin/ empty" }

$multiRoot = Join-Path $work 'multi'
$multiProject = [System.IO.File]::ReadAllText((Join-Path $fixture 'BreakFloor.csproj')).Replace(
    '<TargetFramework>net10.0</TargetFramework>',
    '<TargetFrameworks>net10.0;net10.0-windows</TargetFrameworks><EnableWindowsTargeting>true</EnableWindowsTargeting>')
Write-Project (Join-Path $multiRoot 'BreakFloor.csproj') $multiProject
Copy-Item (Join-Path $fixture 'Program.cs') $multiRoot
$m = & dotnet build (Join-Path $multiRoot 'BreakFloor.csproj') -c Debug @common "-p:HashSetVersion=$BelowHashSet" 2>&1 | ForEach-Object { "$_" }
$mBuilt = $LASTEXITCODE -eq 0
Show $m 'GO2CS3002'
$named = "error GO2CS3002: $([regex]::Escape($hashsetId)) $([regex]::Escape($BelowHashSet)) was built on go\.\* "
$missingTfms = @('net10.0', 'net10.0-windows' | Where-Object { $tfm = $_; @($m | Where-Object { $_ -match $named -and $_ -match "TargetFramework=$([regex]::Escape($tfm))\]" }).Count -eq 0 })
$multiOutput = @(Get-ChildItem (Join-Path $multiRoot 'bin') -Recurse -File -ErrorAction SilentlyContinue)
if ($mBuilt) { $verdicts += "FAIL (MULTI-TFM): built; the floor did not stop it" }
elseif ($missingTfms.Count -gt 0) { Show $m ' error '; $verdicts += "FAIL (MULTI-TFM): no GO2CS3002 naming $hashsetId for $($missingTfms -join ', ')" }
elseif ($multiOutput.Count -gt 0) { $verdicts += "FAIL (MULTI-TFM): bin/ holds $($multiOutput.Count) file(s); the build did not stop" }
else { $verdicts += "PASS (MULTI-TFM): GO2CS3002 named $hashsetId in both inner builds, bin/ empty" }

$verdicts | ForEach-Object { Write-Host $_ }
if (-not $KeepWork) { Remove-Item -Recurse -Force $work -ErrorAction SilentlyContinue }
if (@($verdicts | Where-Object { $_ -like 'FAIL*' }).Count -gt 0) { exit 1 }
exit 0
