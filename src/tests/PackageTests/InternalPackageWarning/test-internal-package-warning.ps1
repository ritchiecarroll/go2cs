#Requires -Version 7
<#
.SYNOPSIS
    Gate for GO2CS0014, go.lib's consumer analyzer (src/gen/go2cs-consumer-analyzers).

.DESCRIPTION
    Restores the InternalPackageWarning fixture from ONE feed into a FRESH package cache, then builds it five ways and
    counts GO2CS0014 in the compiler's own SARIF log (one result per diagnostic, so a count is not doubled by the
    build summary):

      (A) WARN       Internal.cs names go.internal.goversion's package class twice: exactly one GO2CS0014, a warning,
                     in Internal.cs, and the build and run succeed.
      (B) PUBLIC     without Internal.cs (-p:NameInternal=false) only a public package is named: none.
      (C) ERROR      -p:WarningsAsErrors=GO2CS0014: the build fails, and the error it fails with is GO2CS0014.
      (D) WITH GEN   go.gen also referenced (-p:GoWithGen=true): still exactly one. go2cs-gen's copy acts only in a
                     project that declares a Go package, so the two copies never both report.
      (E) OPT-OUT    -p:GoInternalPackageWarning=false: none.

    (A) and (C) are the arms that can fail against a go.lib without the analyzer, or a go.internal.* package without
    the [GoInternalRoot] stamp; (B) and (E) are the controls that would catch an analyzer reporting everything.

    Exit 0 only when every arm reads as expected. Nothing outside a temporary work directory is written.

.PARAMETER Version
    The version of go.lib, go.unicode.utf8 and go.internal.goversion to restore (the fixture's GoPackageVersion).

.PARAMETER GenVersion
    The go.gen version for arm (D). Default: Version.

.PARAMETER Source
    The feed: a local folder of .nupkg files (a pack rehearsal's output) or a URL. Default nuget.org.

.EXAMPLE
    pwsh ./test-internal-package-warning.ps1 -Version 1.26.10.4 -Source ../../../../artifacts/pack
#>
param(
    [Parameter(Mandatory)] [string] $Version,
    [string] $GenVersion,
    [string] $Source = 'https://api.nuget.org/v3/index.json',
    [switch] $KeepWork
)

$ErrorActionPreference = 'Stop'

if (-not $GenVersion) { $GenVersion = $Version }

$fixture = $PSScriptRoot
$work = Join-Path ([System.IO.Path]::GetTempPath()) ("internal-package-warning-" + [System.Guid]::NewGuid().ToString('N').Substring(0, 8))
New-Item -ItemType Directory -Path $work | Out-Null
Copy-Item (Join-Path $fixture 'InternalPackageWarning.csproj'), (Join-Path $fixture 'Program.cs'), (Join-Path $fixture 'Internal.cs') $work

if (Test-Path $Source) { $Source = (Resolve-Path $Source).Path }

# A feed of ONE source and a cache nothing else has touched: a read must come from the packages under test.
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

$project = Join-Path $work 'InternalPackageWarning.csproj'
$sarif = Join-Path $work 'diagnostics.sarif'
$common = @("-p:GoPackageVersion=$Version", "-p:GoGenVersion=$GenVersion", '-nologo', '-v:m', "-p:ErrorLog=$sarif%2Cversion=2.1")

# The GO2CS0014 results of the last build, as "level file:line".
function Read-Results {
    if (-not (Test-Path $sarif)) { return @() }
    $log = Get-Content $sarif -Raw | ConvertFrom-Json
    @($log.runs | ForEach-Object { $_.results } | Where-Object { $_.ruleId -eq 'GO2CS0014' } | ForEach-Object {
        $location = $_.locations[0].physicalLocation
        "$($_.level) $([System.IO.Path]::GetFileName(([uri]$location.artifactLocation.uri).LocalPath)):$($location.region.startLine)"
    })
}

function Invoke-Arm([string] $Name, [string[]] $Extra, [bool] $ExpectBuild, [string[]] $ExpectResults) {
    Remove-Item -Recurse -Force (Join-Path $work 'bin'), (Join-Path $work 'obj'), $sarif -ErrorAction SilentlyContinue
    $log = & dotnet build $project -c Debug @common @Extra 2>&1 | ForEach-Object { "$_" }
    $built = $LASTEXITCODE -eq 0
    $results = Read-Results
    $read = if ($results.Count -eq 0) { 'none' } else { $results -join ', ' }

    if ($built -ne $ExpectBuild) {
        $log | Select-String ' error ' | Select-Object -First 5 | ForEach-Object { Write-Host "    $_" }
        return "FAIL ($Name): expected the build to $(if ($ExpectBuild) { 'succeed' } else { 'fail' }); GO2CS0014: $read"
    }

    if (-not $ExpectBuild -and -not ($log -match 'error GO2CS0014')) { return "FAIL ($Name): the build failed, but not with GO2CS0014" }

    if ((@($results) -join '|') -ne (@($ExpectResults) -join '|')) {
        return "FAIL ($Name): GO2CS0014 read $read, expected $(if ($ExpectResults.Count -eq 0) { 'none' } else { $ExpectResults -join ', ' })"
    }

    if ($ExpectBuild) {
        $run = & dotnet run --project $project -c Debug --no-build @Extra 2>&1 | ForEach-Object { "$_" }
        if ($LASTEXITCODE -ne 0 -or -not ($run -match 'INTERNAL-PACKAGE-WARNING: ok')) { return "FAIL ($Name): the fixture did not run clean" }
    }

    return "PASS ($Name): GO2CS0014 $read"
}

Write-Host "Internal package warning gate: go.* $Version (go.gen $GenVersion) from $Source"
$verdicts = @()
$verdicts += Invoke-Arm '(A) WARN' @() $true @('warning Internal.cs:5')
$verdicts += Invoke-Arm '(B) PUBLIC' @('-p:NameInternal=false') $true @()
$verdicts += Invoke-Arm '(C) ERROR' @('-p:WarningsAsErrors=GO2CS0014') $false @('error Internal.cs:5')
$verdicts += Invoke-Arm '(D) WITH GEN' @('-p:GoWithGen=true') $true @('warning Internal.cs:5')
$verdicts += Invoke-Arm '(E) OPT-OUT' @('-p:GoInternalPackageWarning=false') $true @()

$verdicts | ForEach-Object { Write-Host $_ }
if (-not $KeepWork) { Remove-Item -Recurse -Force $work -ErrorAction SilentlyContinue }
if (@($verdicts | Where-Object { $_ -like 'FAIL*' }).Count -gt 0) { exit 1 }
exit 0
