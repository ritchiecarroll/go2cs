#Requires -Version 7
<#
.SYNOPSIS
    Gate for go.lib's roll-forward default (src/core/golib/buildTransitive/go.lib.targets, THE ROLL-FORWARD).

.DESCRIPTION
    Restores the RollForward fixture from ONE feed into a FRESH package cache, builds it per arm, and reads the
    runtimeconfig.json each build writes:

      GREEN       an executable gets "rollForward": "Major".
      USER WINS   with -p:RollForward=LatestPatch the project's own value is written instead.
      LIBRARY     with -p:OutputType=Library the property stays unset (a library chooses no runtime).
      CONTROL     the arm can fail: with -p:RollForward=Disable the runtimeconfig reads "Disable".

    With -DotnetRoot pointing at a .NET that has NO 10.0 runtime (an isolated .NET 11 install, say), the GREEN
    build is also RUN under that root and must start and print its runtime major; the CONTROL build must fail to
    start with the missing-framework error. With -TargetsFile (pre-pack), a PUBLISHED arm also builds the fixture
    from the package as published, without the working-tree targets, and expects no rollForward key, and, under
    -DotnetRoot, the same failure to start: that is the red this change turns green.

    Exit 0 only when every arm reads as expected. Nothing outside a temporary work directory is written.

.PARAMETER Version
    The go.lib package version to restore (the fixture's GoPackageVersion).

.PARAMETER Source
    The feed: a local folder of .nupkg files (a pack rehearsal's output) or a URL. Default nuget.org.

.PARAMETER TargetsFile
    PRE-PACK ONLY: import this go.lib.targets explicitly, to test the working-tree file against an
    already-published go.lib that does not carry the roll-forward default yet.

.PARAMETER DotnetRoot
    Optional: a dotnet root with no Microsoft.NETCore.App 10.0 runtime, used to RUN the built program. The build
    itself always uses the dotnet on PATH.

.EXAMPLE
    pwsh ./test-roll-forward.ps1 -Version 1.26.11.1 -TargetsFile ../../../core/golib/buildTransitive/go.lib.targets -DotnetRoot D:\dotnet11
#>
param(
    [Parameter(Mandatory)] [string] $Version,
    [string] $Source = 'https://api.nuget.org/v3/index.json',
    [string] $TargetsFile,
    [string] $DotnetRoot,
    [switch] $KeepWork
)

$ErrorActionPreference = 'Stop'

$fixture = $PSScriptRoot
$work = Join-Path ([System.IO.Path]::GetTempPath()) ("roll-forward-" + [System.Guid]::NewGuid().ToString('N').Substring(0, 8))
New-Item -ItemType Directory -Path $work | Out-Null
Copy-Item (Join-Path $fixture 'RollForward.csproj'), (Join-Path $fixture 'Program.cs') $work

if (Test-Path $Source) { $Source = (Resolve-Path $Source).Path }

if ($DotnetRoot) {
    $DotnetRoot = (Resolve-Path $DotnetRoot).Path
    $tenRuntime = Get-ChildItem (Join-Path $DotnetRoot 'shared/Microsoft.NETCore.App') -Directory -ErrorAction SilentlyContinue |
        Where-Object { $_.Name -like '10.*' }
    if ($tenRuntime) { throw "-DotnetRoot $DotnetRoot carries a 10.0 runtime ($($tenRuntime.Name -join ', ')); the run arms need a root without one" }
}

# A feed of ONE source and a cache nothing else has touched: a green read must come from the package
# under test, never from a machine-wide cache or a second feed.
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
Remove-Item Env:DOTNET_ROLL_FORWARD -ErrorAction SilentlyContinue

$project = Join-Path $work 'RollForward.csproj'
$withTargets = @()
if ($TargetsFile) { $withTargets = @("-p:GoLibTargets=$((Resolve-Path $TargetsFile).Path)") }

function Build-Arm([string[]] $Extra) {
    Remove-Item -Recurse -Force (Join-Path $work 'bin'), (Join-Path $work 'obj') -ErrorAction SilentlyContinue
    $log = & dotnet build $project -c Debug "-p:GoPackageVersion=$Version" -nologo -v:m @Extra 2>&1 | ForEach-Object { "$_" }
    if ($LASTEXITCODE -ne 0) { $log | Select-String ' error ' | Select-Object -First 5 | ForEach-Object { Write-Host "    $_" }; return $null }
    return Join-Path $work 'bin/Debug/net10.0'
}

function Read-RollForward([string] $OutDir) {
    $config = Join-Path $OutDir 'RollForward.runtimeconfig.json'
    if (-not (Test-Path $config)) { return '<no runtimeconfig>' }
    $options = (Get-Content -Raw $config | ConvertFrom-Json).runtimeOptions
    if ($null -eq $options.rollForward) { return '<unset>' }
    return [string]$options.rollForward
}

function Invoke-Program([string] $OutDir) {
    $exe = Join-Path $OutDir ('RollForward' + $(if ($IsWindows) { '.exe' } else { '' }))
    $saved = $env:DOTNET_ROOT, $env:DOTNET_ROOT_X64
    try {
        $env:DOTNET_ROOT = $DotnetRoot; $env:DOTNET_ROOT_X64 = $DotnetRoot
        $out = & $exe 2>&1 | ForEach-Object { "$_" }
        return [pscustomobject]@{ Code = $LASTEXITCODE; Text = ($out -join "`n") }
    } finally {
        $env:DOTNET_ROOT, $env:DOTNET_ROOT_X64 = $saved
    }
}

$verdicts = @()
Write-Host "Roll-forward gate: go.lib $Version from $Source$(if ($TargetsFile) { ", working-tree targets" })$(if ($DotnetRoot) { ", run under $DotnetRoot" })"

# GREEN: an executable rolls forward by major.
$out = Build-Arm $withTargets
if (-not $out) { $verdicts += 'FAIL (GREEN): the fixture did not build' }
else {
    $value = Read-RollForward $out
    if ($value -ne 'Major') { $verdicts += "FAIL (GREEN): runtimeconfig rollForward is $value, want Major" }
    elseif ($DotnetRoot) {
        $run = Invoke-Program $out
        Write-Host "    $($run.Text)"
        if ($run.Code -ne 0 -or $run.Text -notmatch 'ROLL-FORWARD: ok runtime=(\d+)' -or [int]$Matches[1] -lt 11) {
            $verdicts += "FAIL (GREEN): under $DotnetRoot the program did not run on a newer major (exit $($run.Code))"
        } else { $verdicts += "PASS (GREEN): rollForward Major, ran on runtime $($Matches[1])" }
    } else { $verdicts += 'PASS (GREEN): rollForward Major' }
}

# USER WINS: the project's own value is kept.
$out = Build-Arm ($withTargets + '-p:RollForward=LatestPatch')
$value = if ($out) { Read-RollForward $out } else { '<build failed>' }
$verdicts += $(if ($value -eq 'LatestPatch') { 'PASS (USER WINS): rollForward LatestPatch' } else { "FAIL (USER WINS): rollForward is $value, want LatestPatch" })

# LIBRARY: no runtime of its own, so the property stays unset.
$libValue = (& dotnet msbuild $project -restore "-p:GoPackageVersion=$Version" -p:OutputType=Library -nologo -getProperty:RollForward @withTargets 2>&1 | Select-Object -Last 1)
$libValue = "$libValue".Trim()
$verdicts += $(if ($LASTEXITCODE -eq 0 -and $libValue -eq '') { 'PASS (LIBRARY): RollForward unset' } else { "FAIL (LIBRARY): RollForward reads '$libValue' (exit $LASTEXITCODE), want unset" })

# CONTROL: the arm can fail. Disable is written as such and, on a root without 10.0, cannot start.
$out = Build-Arm ($withTargets + '-p:RollForward=Disable')
$value = if ($out) { Read-RollForward $out } else { '<build failed>' }
if ($value -ne 'Disable') { $verdicts += "FAIL (CONTROL): rollForward is $value, want Disable" }
elseif ($DotnetRoot) {
    $run = Invoke-Program $out
    if ($run.Code -eq 0 -or $run.Text -notmatch "Microsoft\.NETCore\.App', version '10\.") {
        $verdicts += "FAIL (CONTROL): with rollForward Disable the program started under $DotnetRoot (exit $($run.Code))"
    } else { $verdicts += "PASS (CONTROL): rollForward Disable, failed to start as required (exit $($run.Code))" }
} else { $verdicts += 'PASS (CONTROL): rollForward Disable' }

# PUBLISHED (pre-pack only): the package as published, without the working-tree targets -- the red.
if ($TargetsFile) {
    $out = Build-Arm @()
    $value = if ($out) { Read-RollForward $out } else { '<build failed>' }
    if ($value -ne '<unset>') { $verdicts += "NOTE (PUBLISHED): go.lib $Version already writes rollForward $value" }
    elseif ($DotnetRoot) {
        $run = Invoke-Program $out
        if ($run.Code -eq 0) { $verdicts += "FAIL (PUBLISHED): the published package's build started under $DotnetRoot; the red did not reproduce" }
        else { $verdicts += "PASS (PUBLISHED): no rollForward, failed to start under $DotnetRoot (exit $($run.Code)), the red this change turns green" }
    } else { $verdicts += 'PASS (PUBLISHED): no rollForward key' }
}

$verdicts | ForEach-Object { Write-Host $_ }
if (-not $KeepWork) { Remove-Item -Recurse -Force $work -ErrorAction SilentlyContinue }
if (@($verdicts | Where-Object { $_ -like 'FAIL*' }).Count -gt 0) { exit 1 }
exit 0
