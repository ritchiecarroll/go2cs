<#
.SYNOPSIS
    Build and run the standalone go2cs performance comparison runner.

.DESCRIPTION
    The runner (PerformanceRunner) builds each benchmark project three ways -- the original Go binary,
    the transpiled C# on the normal JIT runtime, and the transpiled C# as a Native AOT self-contained
    executable -- verifies all three produce identical output, then measures workload time (in-program,
    excludes startup), process wall time, and peak working set, and prints a markdown report.
    This wrapper builds the runner (fast, dependency-free), forwards all arguments to it, and on
    success mirrors README.md to docs/Performance.md so the GitHub Pages site (which only publishes
    the docs folder) can link the performance comparison. README.md here is the master copy.

    A full run including the Native AOT publishes takes several minutes; pass --no-aot while iterating.

.PARAMETER RunnerArgs
    Forwarded verbatim to PerformanceRunner. Examples:
      --filter <substr>     only matching projects
      --phase <list>        transpile,build,verify,measure,all
      --runs <n>            measured runs per variant (default 5)
      --no-aot              skip the Native AOT column (much faster)
      --update-readme       rewrite the results block in README.md
      --list                list matched projects

.EXAMPLE
    ./run-performance.ps1
    ./run-performance.ps1 --filter Fib --no-aot
    ./run-performance.ps1 --runs 10 --update-readme
#>
[CmdletBinding()]
param(
    [Parameter(ValueFromRemainingArguments = $true)]
    [string[]] $RunnerArgs
)

$ErrorActionPreference = "Stop"

# $ExeSuffix and the roots come from the shared definition at src\_paths.ps1 (F4,
# docs/PLAN-linux-operation.md).
. (Join-Path $PSScriptRoot '../../_paths.ps1')

$runnerProj = Join-Path $PSScriptRoot 'PerformanceRunner/PerformanceRunner.csproj'
$runnerExe  = Join-Path $PSScriptRoot "PerformanceRunner/bin/Debug/$NetVersion/PerformanceRunner$ExeSuffix"

Write-Host "==> building PerformanceRunner..." -ForegroundColor Cyan
& dotnet build $runnerProj -c Debug -clp:ErrorsOnly --nologo | Out-Null
if ($LASTEXITCODE -ne 0) { throw "PerformanceRunner build failed ($LASTEXITCODE)" }

# A GREEN build does not prove the runner is where this script expects it: the build writes to the
# TFM the projects declare, while the path above is composed from $NetVersion. Disagree on that and
# the suite measures NOTHING while every check above it passes -- and this wrapper is where that was
# first seen, dying in 20 seconds with an empty log (false-green route #6, CLAUDE.md). Explicit
# `exit 1` rather than `throw`: the exit CODE is the property that matters here, and it is the one
# thing a throw leaves to the host's discretion.
if (-not (Test-Path -LiteralPath $runnerExe)) {
    Write-Host "*** PerformanceRunner built, but no executable at the expected path ***" -ForegroundColor Red
    Write-Host "    expected: $runnerExe" -ForegroundColor Red
    Write-Host "    NOTHING RAN. Check that `$NetVersion ($NetVersion) matches the framework the build" -ForegroundColor Red
    Write-Host "    emitted -- it is derived from src/Directory.Build.props (see src/_paths.ps1)." -ForegroundColor Red
    exit 1
}

& $runnerExe @RunnerArgs
$runnerExit = $LASTEXITCODE

# Mirror the README into docs/ so it is reachable from the GitHub Pages site, which only
# publishes the docs folder. This README.md is the master; docs/Performance.md is the copy.
# The banner goes at the END: jekyll-titles-from-headings takes a page's title only from a heading
# at the very start of the file, so a banner above the H1 published the page with the site's default title.
if ($runnerExit -eq 0) {
    $readme   = Join-Path $PSScriptRoot "README.md"
    $docsCopy = Join-Path $RepoRoot 'docs/Performance.md'
    $banner   = "`r`n<!-- AUTO-COPIED from src/tests/Performance/README.md by run-performance.ps1 -- edit that file, not this one. -->`r`n"
    [IO.File]::WriteAllText($docsCopy, [IO.File]::ReadAllText($readme) + $banner)
    Write-Host "==> mirrored README.md to docs/Performance.md" -ForegroundColor Cyan
}
exit $runnerExit
