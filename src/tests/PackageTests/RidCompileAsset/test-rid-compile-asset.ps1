#Requires -Version 7
<#
.SYNOPSIS
    Gate for go.lib's RID-selected compile asset (src/core/golib/buildTransitive/go.lib.targets).

.DESCRIPTION
    Restores the RidCompileAsset fixture from ONE feed into a FRESH package cache, then checks, on the
    platform it runs on:

      GREEN      the fixture (which touches a type only this platform's go.syscall flavour defines)
                 builds RID-less and runs.
      RID        the same with an explicit -r <host rid>.
      PUBLISH    a framework-dependent `dotnet publish -r <host rid>`, then the PUBLISHED app is run.
      CONTROL    the arm can fail. On a platform whose flavour differs from lib/'s reference flavour
                 (linux), turning the swap off (-p:GoRidCompileAssets=false) must fail with CS0426.
                 On the reference platform (windows) there is nothing to swap FROM, so the control is
                 the no-op proof instead: the runtimes/<rid> twin is byte-identical to lib/.

    Exit 0 only when every arm reads as expected. Nothing outside a temporary work directory is
    written.

.PARAMETER Version
    The go.* package version to restore (the fixture's GoPackageVersion).

.PARAMETER Source
    The feed: a local folder of .nupkg files (a pack rehearsal's output) or a URL. Default nuget.org.

.PARAMETER TargetsFile
    PRE-PACK ONLY: import this go.lib.targets explicitly, to test the working-tree file against an
    already-published package set whose go.lib does not carry it yet.

.EXAMPLE
    pwsh ./test-rid-compile-asset.ps1 -Version 1.24.13.2 -Source ../../../artifacts/nupkg
#>
param(
    [Parameter(Mandatory)] [string] $Version,
    [string] $Source = 'https://api.nuget.org/v3/index.json',
    [string] $TargetsFile,
    [switch] $KeepWork
)

$ErrorActionPreference = 'Stop'

$fixture = $PSScriptRoot
$work = Join-Path ([System.IO.Path]::GetTempPath()) ("rid-compile-asset-" + [System.Guid]::NewGuid().ToString('N').Substring(0, 8))
New-Item -ItemType Directory -Path $work | Out-Null
Copy-Item (Join-Path $fixture 'RidCompileAsset.csproj'), (Join-Path $fixture 'Program.cs') $work

if (Test-Path $Source) { $Source = (Resolve-Path $Source).Path }

# A feed of ONE source and a cache nothing else has touched: a green read must come from the packages
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

$rid = (dotnet --info | Select-String -Pattern '^\s*RID:\s*(\S+)' | Select-Object -First 1).Matches.Groups[1].Value
if (-not $rid) { throw 'Could not read the SDK RID from dotnet --info' }
$isReferencePlatform = $rid.StartsWith('win')

$common = @("-p:GoPackageVersion=$Version", '-nologo', '-v:m')
if ($TargetsFile) { $common += "-p:GoLibTargets=$((Resolve-Path $TargetsFile).Path)" }

function Invoke-Arm([string] $Name, [string[]] $Extra, [bool] $ExpectBuild, [string] $ExpectError) {
    Remove-Item -Recurse -Force (Join-Path $work 'bin'), (Join-Path $work 'obj') -ErrorAction SilentlyContinue
    $log = & dotnet build (Join-Path $work 'RidCompileAsset.csproj') -c Debug @common @Extra 2>&1 | ForEach-Object { "$_" }
    $built = $LASTEXITCODE -eq 0
    $ref = ($log | Select-String 'RID-COMPILE-ASSET syscall reference: (.*)$' | Select-Object -First 1)
    if ($ref) { Write-Host "  [$Name] compile reference: $($ref.Matches.Groups[1].Value.Trim())" }

    if ($ExpectBuild) {
        if (-not $built) { $log | Select-String ' error ' | Select-Object -First 5 | ForEach-Object { Write-Host "    $_" }; return "FAIL ($Name): expected a green build" }
        $run = & dotnet run --project (Join-Path $work 'RidCompileAsset.csproj') -c Debug --no-build @Extra 2>&1 | ForEach-Object { "$_" }
        $runCode = $LASTEXITCODE
        $run | ForEach-Object { Write-Host "    $_" }
        if ($runCode -ne 0 -or -not ($run -match 'RID-COMPILE-ASSET: ')) { return "FAIL ($Name): the fixture did not run clean (exit $runCode)" }
        return "PASS ($Name)"
    }

    if ($built) { return "FAIL ($Name): expected the build to fail with $ExpectError, and it built" }
    if (-not ($log -match "error $ExpectError")) { return "FAIL ($Name): the build failed, but not with $ExpectError" }
    return "PASS ($Name): failed with $ExpectError as the control requires"
}

Write-Host "RID-selected compile asset gate: go.* $Version from $Source on $rid"
$verdicts = @()
$verdicts += Invoke-Arm 'GREEN rid-less' @() $true ''
$verdicts += Invoke-Arm "RID -r $rid" @('-r', $rid) $true ''

# PUBLISH: users publish, not only run. A framework-dependent publish for the host RID copies the
# RID-selected runtime asset flat into the output; the published app must start and reach the
# platform-only type from THAT folder, with no package cache behind it.
function Invoke-PublishArm {
    Remove-Item -Recurse -Force (Join-Path $work 'bin'), (Join-Path $work 'obj') -ErrorAction SilentlyContinue
    $out = Join-Path $work 'publish'
    $log = & dotnet publish (Join-Path $work 'RidCompileAsset.csproj') -c Release -r $rid --self-contained false -o $out @common 2>&1 | ForEach-Object { "$_" }
    if ($LASTEXITCODE -ne 0) { $log | Select-String ' error ' | Select-Object -First 5 | ForEach-Object { Write-Host "    $_" }; return "FAIL (PUBLISH -r $rid): the publish failed" }
    $run = & dotnet (Join-Path $out 'RidCompileAsset.dll') 2>&1 | ForEach-Object { "$_" }
    $runCode = $LASTEXITCODE
    $run | ForEach-Object { Write-Host "    $_" }
    if ($runCode -ne 0 -or -not ($run -match 'RID-COMPILE-ASSET: ')) { return "FAIL (PUBLISH -r $rid): the published app did not run clean (exit $runCode)" }
    return "PASS (PUBLISH -r $rid)"
}
$verdicts += Invoke-PublishArm

if ($isReferencePlatform) {
    $pkg = Join-Path $env:NUGET_PACKAGES "go.syscall/$Version"
    $lib = Get-FileHash (Join-Path $pkg 'lib/net10.0/syscall.dll')
    $twin = Get-FileHash (Join-Path $pkg "runtimes/$rid/lib/net10.0/syscall.dll")
    Write-Host "  [CONTROL no-op] lib/ $($lib.Hash.Substring(0, 16)) vs runtimes/$rid $($twin.Hash.Substring(0, 16))"
    $verdicts += if ($lib.Hash -eq $twin.Hash) { 'PASS (CONTROL no-op): the reference-platform twin is byte-identical to lib/' } else { 'FAIL (CONTROL no-op): the twin differs from lib/ on the reference platform' }
}
else {
    $verdicts += Invoke-Arm 'CONTROL opt-out' @('-p:GoRidCompileAssets=false') $false 'CS0426'
}

$verdicts | ForEach-Object { Write-Host $_ }
if (-not $KeepWork) { Remove-Item -Recurse -Force $work -ErrorAction SilentlyContinue } else { Write-Host "work kept: $work" }

if ($verdicts | Where-Object { $_ -like 'FAIL*' }) { exit 1 }
exit 0
