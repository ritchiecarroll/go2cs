#Requires -Version 7
<#
.SYNOPSIS
    Gate for go.lib's consumer usings (src/core/golib/buildTransitive/go.lib.targets).

.DESCRIPTION
    Restores the ConsumerUsings fixture from ONE feed into a FRESH package cache, then checks:

      GREEN      the fixture, whose Program.cs declares no using for golib, builds and runs: the
                 `go` namespace and the static `go.builtin` members arrive with the package.
      CONTROL    the arm can fail: with -p:GoConsumerUsings=false the same build must fail with
                 CS0246 (the golib type names no longer resolve).

    Exit 0 only when both arms read as expected. Nothing outside a temporary work directory is written.

.PARAMETER Version
    The go.lib package version to restore (the fixture's GoPackageVersion).

.PARAMETER Source
    The feed: a local folder of .nupkg files (a pack rehearsal's output) or a URL. Default nuget.org.

.PARAMETER TargetsFile
    PRE-PACK ONLY: import this go.lib.targets explicitly, to test the working-tree file against an
    already-published go.lib that does not carry the usings yet.

.EXAMPLE
    pwsh ./test-consumer-usings.ps1 -Version 1.24.13.3 -TargetsFile ../../../core/golib/buildTransitive/go.lib.targets
#>
param(
    [Parameter(Mandatory)] [string] $Version,
    [string] $Source = 'https://api.nuget.org/v3/index.json',
    [string] $TargetsFile,
    [switch] $KeepWork
)

$ErrorActionPreference = 'Stop'

$fixture = $PSScriptRoot
$work = Join-Path ([System.IO.Path]::GetTempPath()) ("consumer-usings-" + [System.Guid]::NewGuid().ToString('N').Substring(0, 8))
New-Item -ItemType Directory -Path $work | Out-Null
Copy-Item (Join-Path $fixture 'ConsumerUsings.csproj'), (Join-Path $fixture 'Program.cs') $work

if (Test-Path $Source) { $Source = (Resolve-Path $Source).Path }

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

$common = @("-p:GoPackageVersion=$Version", '-nologo', '-v:m')
if ($TargetsFile) { $common += "-p:GoLibTargets=$((Resolve-Path $TargetsFile).Path)" }
$project = Join-Path $work 'ConsumerUsings.csproj'

function Invoke-Arm([string] $Name, [string[]] $Extra, [bool] $ExpectBuild, [string] $ExpectError) {
    Remove-Item -Recurse -Force (Join-Path $work 'bin'), (Join-Path $work 'obj') -ErrorAction SilentlyContinue
    $log = & dotnet build $project -c Debug @common @Extra 2>&1 | ForEach-Object { "$_" }
    $built = $LASTEXITCODE -eq 0

    if ($ExpectBuild) {
        if (-not $built) { $log | Select-String ' error ' | Select-Object -First 5 | ForEach-Object { Write-Host "    $_" }; return "FAIL ($Name): expected a green build" }
        $run = & dotnet run --project $project -c Debug --no-build @Extra 2>&1 | ForEach-Object { "$_" }
        $runCode = $LASTEXITCODE
        $run | ForEach-Object { Write-Host "    $_" }
        if ($runCode -ne 0 -or -not ($run -match 'CONSUMER-USINGS: ok')) { return "FAIL ($Name): the fixture did not run clean (exit $runCode)" }
        return "PASS ($Name)"
    }

    if ($built) { return "FAIL ($Name): expected the build to fail with $ExpectError, and it built" }
    if (-not ($log -match "error $ExpectError")) { return "FAIL ($Name): the build failed, but not with $ExpectError" }
    return "PASS ($Name): failed with $ExpectError as the control requires"
}

Write-Host "Consumer usings gate: go.lib $Version from $Source"
$verdicts = @()
$verdicts += Invoke-Arm 'GREEN' @() $true ''
$verdicts += Invoke-Arm 'CONTROL GoConsumerUsings=false' @('-p:GoConsumerUsings=false') $false 'CS0246'

$verdicts | ForEach-Object { Write-Host $_ }
if (-not $KeepWork) { Remove-Item -Recurse -Force $work -ErrorAction SilentlyContinue }
if (@($verdicts | Where-Object { $_ -like 'FAIL*' }).Count -gt 0) { exit 1 }
exit 0
