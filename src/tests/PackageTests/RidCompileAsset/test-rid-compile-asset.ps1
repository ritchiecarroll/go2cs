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
      UNSHIPPED  a build for a RID no go.* package ships (-UnshippedRid, linux-riscv64 by default) must FAIL
                 with GO2CS3003 naming that RID, instead of compiling the reference (Windows) flavour.
                 It must fail the same way with the compile swap off (-p:GoRidCompileAssets=false).
      OPT-OUT    the same build with -p:GoAllowReferenceFlavor=true must build (compile-only: it is not run,
                 because the reference flavour is not the target platform's); set in a project file
                 (Directory.Build.props) instead, it must change nothing.
      ALPINE     -r linux-musl-x64 must build against the linux surface: the RID resolves to linux-x64
                 through the RID graph, as the host resolves it at run time.
      WIN-ARM64  -r win-arm64 must build with warning GO2CS3004: the Windows flavour is that platform's
                 operating system.

    Exit 0 only when every arm reads as expected. Nothing outside a temporary work directory is
    written.

.PARAMETER Version
    The go.* package version to restore (the fixture's GoPackageVersion).

.PARAMETER Source
    The feed: a local folder of .nupkg files (a pack rehearsal's output) or a URL. Default nuget.org.

.PARAMETER UnshippedRid
    A RID for which go.syscall ships no runtimes/<rid>/ twin; the UNSHIPPED and OPT-OUT arms build for it.
    The gate refuses to read those arms if the restored package does ship it, so a later flavour that ships
    this RID moves the arm to another RID rather than letting it pass vacuously. It was linux-arm64 until S6
    shipped that flavour (DESIGN-linux-arm64-flavour.md); linux-riscv64 is a linux RID of another system-call
    ABI that no flavour serves, so it reads GO2CS3003 as linux-arm64 did.

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
    [string] $UnshippedRid = 'linux-riscv64',
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

# The host's PORTABLE RID, as go.lib.targets reads it: a distro-built SDK reports a distro RID in `dotnet --info`
# (ubuntu.24.04-x64 for Ubuntu's packaged SDK), and no go.* package ships a runtimes/ folder for one.
$rid = "$(& dotnet msbuild (Join-Path $work 'RidCompileAsset.csproj') -nologo -getProperty:NETCoreSdkPortableRuntimeIdentifier)".Trim()
if (-not $rid) { $rid = (dotnet --info | Select-String -Pattern '^\s*RID:\s*(\S+)' | Select-Object -First 1).Matches.Groups[1].Value }
if (-not $rid) { throw 'Could not read the SDK RID from msbuild or dotnet --info' }
$isReferencePlatform = $rid.StartsWith('win')

$common = @("-p:GoPackageVersion=$Version", '-nologo', '-v:m')
if ($TargetsFile) {
    # The explicit import (GoLibTargets) reaches only a package set whose go.lib predates the file. A go.lib that
    # ships buildTransitive/go.lib.targets is imported AFTER the project body, so its targets and properties
    # override the explicit import and the gate reads the PUBLISHED file (measured 2026-11-09 on go.* 1.26.10.3: a
    # marker in the imported copy never printed). So the working-tree files replace the restored go.lib's own,
    # in this run's private package cache, and the copy is checked by hash before any arm reads it.
    $tf = (Resolve-Path $TargetsFile).Path
    & dotnet restore (Join-Path $work 'RidCompileAsset.csproj') @common 2>&1 | ForEach-Object { "$_" } | Select-String ' error ' | ForEach-Object { Write-Host "    $_" }
    if ($LASTEXITCODE -ne 0) { throw "-TargetsFile: the restore failed (exit $LASTEXITCODE)" }
    $bt = Join-Path $env:NUGET_PACKAGES "go.lib/$Version/buildTransitive"
    if (Test-Path (Join-Path $bt 'go.lib.targets')) {
        Copy-Item (Join-Path (Split-Path $tf) '*.targets') $bt -Force
        if ((Get-FileHash $tf).Hash -ne (Get-FileHash (Join-Path $bt 'go.lib.targets')).Hash) { throw '-TargetsFile: the restored go.lib.targets is not the working-tree file after the copy' }
        Write-Host "  [-TargetsFile] the restored go.lib $Version's buildTransitive/ now carries the working-tree files"
    }
    else { $common += "-p:GoLibTargets=$tf" }
}

function Invoke-Arm([string] $Name, [string[]] $Extra, [bool] $ExpectBuild, [string] $ExpectError, [string] $ExpectText = '', [switch] $BuildOnly, [string] $ExpectWarning = '') {
    Remove-Item -Recurse -Force (Join-Path $work 'bin'), (Join-Path $work 'obj') -ErrorAction SilentlyContinue
    $log = & dotnet build (Join-Path $work 'RidCompileAsset.csproj') -c Debug @common @Extra 2>&1 | ForEach-Object { "$_" }
    $built = $LASTEXITCODE -eq 0
    $ref = ($log | Select-String 'RID-COMPILE-ASSET syscall reference: (.*)$' | Select-Object -First 1)
    if ($ref) { Write-Host "  [$Name] compile reference: $($ref.Matches.Groups[1].Value.Trim())" }

    if ($ExpectBuild) {
        if (-not $built) { $log | Select-String ' error ' | Select-Object -First 5 | ForEach-Object { Write-Host "    $_" }; return "FAIL ($Name): expected a green build" }
        if ($ExpectWarning -and -not ($log -match "warning $ExpectWarning.*$([regex]::Escape($ExpectText))")) { return "FAIL ($Name): built, but without warning $ExpectWarning naming '$ExpectText'" }
        if ($BuildOnly) { return "PASS ($Name): built$(if ($ExpectWarning) { " with $ExpectWarning" }) (compile-only arm, not run)" }
        $run = & dotnet run --project (Join-Path $work 'RidCompileAsset.csproj') -c Debug --no-build @Extra 2>&1 | ForEach-Object { "$_" }
        $runCode = $LASTEXITCODE
        $run | ForEach-Object { Write-Host "    $_" }
        if ($runCode -ne 0 -or -not ($run -match 'RID-COMPILE-ASSET: ')) { return "FAIL ($Name): the fixture did not run clean (exit $runCode)" }
        return "PASS ($Name)"
    }

    if ($built) { return "FAIL ($Name): expected the build to fail with $ExpectError, and it built" }
    if (-not ($log -match "error $ExpectError")) { $log | Select-String ' error ' | Select-Object -First 5 | ForEach-Object { Write-Host "    $_" }; return "FAIL ($Name): the build failed, but not with $ExpectError" }
    if ($ExpectText -and -not ($log -match "error $ExpectError.*$([regex]::Escape($ExpectText))")) { return "FAIL ($Name): $ExpectError does not name '$ExpectText'" }
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

# THE REFUSAL ARMS, for RIDs the packages do not ship (go.lib.targets, GoRefuseUnshippedRid). Restoring for a RID
# the host is not needs nothing outside the feed: the builds are framework-dependent and UseAppHost=false, so no
# apphost pack is fetched. Each arm first checks that go.syscall's own runtimes/ folders make it meaningful, so a
# later flavour that ships one of these RIDs fails the arm by name instead of letting it pass vacuously.
$pkgRoot = Join-Path $env:NUGET_PACKAGES "go.syscall/$Version"
$ships = { param($r) Test-Path (Join-Path $pkgRoot "runtimes/$r") }
$noHost = @('-p:UseAppHost=false')
if (-not (Test-Path (Join-Path $pkgRoot 'runtimes'))) {
    $verdicts += "FAIL (UNSHIPPED): go.syscall $Version ships no runtimes/ folder, so it is not RID-split and no refusal arm proves anything"
}
elseif (& $ships $UnshippedRid) {
    $verdicts += "FAIL (UNSHIPPED): go.syscall $Version SHIPS runtimes/$UnshippedRid; pass -UnshippedRid a RID it does not ship"
}
else {
    # The fixture touches the reference (Windows) surface, the one an unshipped RID compiles against, so the only
    # thing that can fail the build is the refusal itself: without it this arm BUILDS, which is the silent defect.
    $unshipped = @('-r', $UnshippedRid, '-p:_RidCompileAssetRid=win-x64') + $noHost
    $verdicts += Invoke-Arm "UNSHIPPED -r $UnshippedRid" $unshipped $false 'GO2CS3003' $UnshippedRid
    # The refusal is its own target: turning the compile swap off does not turn it off.
    $verdicts += Invoke-Arm "UNSHIPPED swap off -r $UnshippedRid" ($unshipped + @('-p:GoRidCompileAssets=false')) $false 'GO2CS3003' $UnshippedRid
    $verdicts += Invoke-Arm "OPT-OUT -r $UnshippedRid" ($unshipped + @('-p:GoAllowReferenceFlavor=true')) $true '' -BuildOnly
    # The opt-out is honoured as a GLOBAL property only: set in a project file, it changes nothing.
    $dbp = Join-Path $work 'Directory.Build.props'
    Set-Content -Path $dbp -Value '<Project><PropertyGroup><GoAllowReferenceFlavor>true</GoAllowReferenceFlavor></PropertyGroup></Project>' -Encoding utf8
    $verdicts += Invoke-Arm "OPT-OUT in a project file -r $UnshippedRid" $unshipped $false 'GO2CS3003' $UnshippedRid
    Remove-Item $dbp
}

# ALPINE: linux-musl-x64 ships no folder of its own and RESOLVES to linux-x64 through the RID graph, as the host does
# at run time. The fixture touches the linux surface (the RID starts with linux), so it builds only if the compile
# asset was resolved through the graph and swapped to runtimes/linux-x64/.
if (-not (& $ships 'linux-x64') -or (& $ships 'linux-musl-x64')) {
    $verdicts += "FAIL (ALPINE): go.syscall $Version must ship runtimes/linux-x64 and not runtimes/linux-musl-x64 for this arm to mean anything"
}
else {
    $verdicts += Invoke-Arm 'ALPINE -r linux-musl-x64' (@('-r', 'linux-musl-x64') + $noHost) $true '' -BuildOnly
}

# WIN-ARM64: an unshipped RID of the reference flavour's own operating system builds, with warning GO2CS3004.
if (& $ships 'win-arm64') {
    $verdicts += "FAIL (WIN-ARM64): go.syscall $Version ships runtimes/win-arm64, so this arm proves nothing"
}
else {
    $verdicts += Invoke-Arm 'WIN-ARM64 -r win-arm64' (@('-r', 'win-arm64') + $noHost) $true '' 'win-arm64' -BuildOnly -ExpectWarning 'GO2CS3004'
}

$verdicts | ForEach-Object { Write-Host $_ }
if (-not $KeepWork) { Remove-Item -Recurse -Force $work -ErrorAction SilentlyContinue } else { Write-Host "work kept: $work" }

if ($verdicts | Where-Object { $_ -like 'FAIL*' }) { exit 1 }
exit 0
