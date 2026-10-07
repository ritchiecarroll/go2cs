#Requires -Version 7
<#
.SYNOPSIS
    Control for go.lib.symbols.targets under Native AOT: the target must leave a PublishAot publish exactly as it was.

.DESCRIPTION
    The target marks every published .pdb ExcludeFromSingleFile, and it is meant to be inert under Native AOT (its
    condition requires PublishAot != true): an AOT publish has no single-file bundle and its own symbol handling.
    This publishes the PublishSymbols fixture with PublishAot=true and PublishSingleFile=true for the host's RID TWICE,
    once with the target on (the default) and once with -p:GoKeepSymbolsLoose=false (its off switch), each into a fresh
    folder from a fresh build, and compares the two publish folders' file lists.

      PASS   the lists are identical (the target changed nothing under AOT) and the AOT executable runs.
      FAIL   the lists differ (the differences are printed), or a publish or the run fails.

    A Native AOT publish needs the platform's native toolchain (clang on Linux, Xcode on macOS, the C++ build tools on
    Windows). Exit 0 only on PASS. Nothing outside a temporary work directory is written.

.PARAMETER Version
    The go.lib package version to restore (the fixture's GoPackageVersion).

.PARAMETER Source
    The feed: a local folder of .nupkg files or a URL. Default nuget.org. go.* resolve from it only.

.PARAMETER TargetsFile
    PRE-PACK ONLY: import this go.lib.targets explicitly, as test-publish-symbols.ps1 does.
#>
param(
    [Parameter(Mandatory)] [string] $Version,
    [string] $Source = 'https://api.nuget.org/v3/index.json',
    [string] $TargetsFile,
    [switch] $KeepWork
)

$ErrorActionPreference = 'Stop'

$fixture = $PSScriptRoot
$work = Join-Path ([System.IO.Path]::GetTempPath()) ("publish-symbols-aot-" + [System.Guid]::NewGuid().ToString('N').Substring(0, 8))
New-Item -ItemType Directory -Path $work | Out-Null
Copy-Item (Join-Path $fixture 'PublishSymbols.csproj'), (Join-Path $fixture 'Program.cs') $work
Copy-Item -Recurse (Join-Path $fixture 'lib') (Join-Path $work 'lib')

if (Test-Path $Source) { $Source = (Resolve-Path $Source).Path }

# The same source rule as test-publish-symbols.ps1: go.* from the source under test only, the SDK's packs (the runtime
# and the ILCompiler packs an AOT publish restores) from nuget.org, and nuget.org listed once when it IS the source.
$nugetOrg = 'https://api.nuget.org/v3/index.json'
$sources = if ($Source.TrimEnd('/') -eq $nugetOrg) { "    <add key=`"under-test`" value=`"$nugetOrg`" />" }
           else { "    <add key=`"under-test`" value=`"$Source`" />`n    <add key=`"nuget.org`" value=`"$nugetOrg`" />" }
$mapping = if ($Source.TrimEnd('/') -eq $nugetOrg) { '    <packageSource key="under-test"><package pattern="*" /></packageSource>' }
           else { "    <packageSource key=`"under-test`"><package pattern=`"go.*`" /></packageSource>`n    <packageSource key=`"nuget.org`"><package pattern=`"*`" /></packageSource>" }
@"
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <clear />
$sources
  </packageSources>
  <packageSourceMapping>
$mapping
  </packageSourceMapping>
</configuration>
"@ | Set-Content -Path (Join-Path $work 'nuget.config') -Encoding utf8

$env:NUGET_PACKAGES = Join-Path $work 'packages'
$env:NUGET_HTTP_CACHE_PATH = Join-Path $work 'http-cache'
$env:DOTNET_CLI_TELEMETRY_OPTOUT = '1'

$rid = [System.Runtime.InteropServices.RuntimeInformation]::RuntimeIdentifier
$exe = if ($IsWindows) { 'PublishSymbols.exe' } else { 'PublishSymbols' }
$common = @("-p:GoPackageVersion=$Version", '-r', $rid, '-p:PublishAot=true', '-p:PublishSingleFile=true', '-nologo', '-v:m')
if ($TargetsFile) { $common += "-p:GoLibTargets=$((Resolve-Path $TargetsFile).Path)" }
$project = Join-Path $work 'PublishSymbols.csproj'

function Invoke-Publish([string] $Name, [string[]] $Extra) {
    $out = Join-Path $work "out-$Name"
    Remove-Item -Recurse -Force (Join-Path $work 'bin'), (Join-Path $work 'obj'), (Join-Path $work 'lib/bin'), (Join-Path $work 'lib/obj'), $out -ErrorAction SilentlyContinue
    $log = & dotnet publish $project -c Release -o $out @common @Extra 2>&1 | ForEach-Object { "$_" }
    if ($LASTEXITCODE -ne 0) {
        $log | Select-String ' error ' | Select-Object -First 5 | ForEach-Object { Write-Host "    $_" }
        return [PSCustomObject]@{ Ok = $false; Files = @(); Line = '' }
    }
    $files = @(Get-ChildItem $out -File -Recurse | ForEach-Object { [System.IO.Path]::GetRelativePath($out, $_.FullName) } | Sort-Object)
    $line = (& (Join-Path $out $exe) 2>&1 | ForEach-Object { "$_" } | Select-String 'PUBLISH-SYMBOLS:' | Select-Object -First 1)
    Write-Host "    $Name ($($files.Count) file(s)): $($files -join ', ') :: $line"
    return [PSCustomObject]@{ Ok = $true; Files = $files; Line = "$line" }
}

Write-Host "Publish symbols AOT control: go.lib $Version from $Source ($rid)"
$on = Invoke-Publish 'target-on' @()
$off = Invoke-Publish 'target-off' @('-p:GoKeepSymbolsLoose=false')

$verdict =
    if (-not $on.Ok -or -not $off.Ok) { 'FAIL (AOT control): a Native AOT publish did not succeed' }
    elseif (-not (Test-Path (Join-Path $work 'packages/go.lib'))) { 'FAIL (AOT control): go.lib did not resolve from the feed under test' }
    elseif ($on.Line -notmatch 'PUBLISH-SYMBOLS:') { 'FAIL (AOT control): the AOT executable did not run' }
    elseif (Compare-Object $on.Files $off.Files) {
        "FAIL (AOT control): the target changed the AOT publish list: $((Compare-Object $on.Files $off.Files | ForEach-Object { "$($_.SideIndicator) $($_.InputObject)" }) -join '; ')"
    }
    else { "PASS (AOT control): the publish list is identical with the target on and off ($($on.Files.Count) file(s))" }

Write-Host $verdict
if (-not $KeepWork) { Remove-Item -Recurse -Force $work -ErrorAction SilentlyContinue }
if ($verdict -like 'FAIL*') { exit 1 }
exit 0
