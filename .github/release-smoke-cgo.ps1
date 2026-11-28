<#
.SYNOPSIS
    release-smoke's cgo job: convert, build and run cgo Phase 3's end-to-end program on this runner, arm by arm, both
    ways, and refuse a green that measured nothing.

.DESCRIPTION
    The cgo path a release ships (COORD's ruling, 2026-11-14, the 3.1 release-path seat): a -cgo conversion is ONE
    package and builds against a go2cs clone, never the go.* packages (src/go2cs/main.go's -cgo help: not combinable
    with -recurse; src/go2cs/csproj-template.xml references $(go2csPath)'s go2cs-gen and golib). So this reads the TREE
    the release is cut from: go builds the program with this runner's C compiler and the probe library on the link line,
    go2cs converts it, the .NET SDK builds it, and each arm runs both ways. The arms and their expectations are the
    test's own (src/go2cs/cgoPhase3_test.go, cgoP3Arms); the job is cgo-e2e's TestCgoPhase3ProgramRunsAsGoDoes, run
    on every release-smoke rather than on demand.

    The build env is set HERE, so a step that forgets it cannot skip the test into a pass: GO2CS_CGO_BUILD=1 (the build
    tests run), GO2CS_REQUIRE_DOTNET=1 and GO2CS_REQUIRE_CC=1 (a missing SDK or compiler reads red, not skipped).
    .github/cgo-probe.ps1 builds the probe first and sets GO2CS_REQUIRE_CGO_PROBE=1.

    THE VACUITY GUARD. go test exits 0 when the test SKIPS, when the filter names no test ("no tests to run") and when
    a renamed test leaves the filter matching nothing. So the verdict is PASS only when go test exited 0, the test's
    own top-level line is "--- PASS", at least one arm ran, and every arm that started (=== RUN) passed (--- PASS).
    Anything else exits 1 with the reason.

.PARAMETER Filter
    The go test -run expression. The default names the one test; another value is for proving the guard (a filter that
    matches nothing must read red).
#>
[CmdletBinding()]
param(
    [string] $Filter = '^TestCgoPhase3ProgramRunsAsGoDoes$',
    [string] $Test = 'TestCgoPhase3ProgramRunsAsGoDoes',
    [string] $Timeout = '60m'
)

$ErrorActionPreference = 'Continue'

$goos = if ($env:GOOS_TARGET) { $env:GOOS_TARGET } else { "$(& go env GOOS)".Trim() }
$rid = if ($env:RID) { $env:RID } else { 'local' }
$art = Join-Path $PWD 'artifacts'
New-Item -ItemType Directory -Force -Path $art | Out-Null
$log = Join-Path $art "release-smoke-cgo-$goos-$rid.log"

$env:GO2CS_CGO_BUILD = '1'
$env:GO2CS_REQUIRE_DOTNET = '1'
$env:GO2CS_REQUIRE_CC = '1'

# The instrument line, as cgo-e2e prints it: the compiler go would hand cmd/cgo, and whether go's default turns cgo on.
$cc = "$(& go env CC)".Trim()
$ccFields = @($cc -split '\s+' | Where-Object { $_ })
$ccVersion = if ($ccFields.Count -and (Get-Command $ccFields[0] -ErrorAction SilentlyContinue)) { "$(& $ccFields[0] @($ccFields | Select-Object -Skip 1) --version 2>&1 | Select-Object -First 1)" } else { 'NOT FOUND on PATH' }
$instrument = "go env CC = $cc ($ccVersion); go env CGO_ENABLED = $("$(& go env CGO_ENABLED)".Trim())"
Write-Host "==> $instrument"

$clock = [Diagnostics.Stopwatch]::StartNew()
Push-Location 'src/go2cs'
try {
    & go test -buildvcs=false -count=1 -v -timeout $Timeout -run $Filter . *>&1 | Tee-Object -FilePath $log
    $runExit = $LASTEXITCODE
}
finally { Pop-Location }
$seconds = [math]::Round($clock.Elapsed.TotalSeconds)

$lines = @(Get-Content -LiteralPath $log -ErrorAction SilentlyContinue)
$name = [regex]::Escape($Test)
$top = @($lines | Where-Object { $_ -match "^\s*--- (PASS|FAIL|SKIP): $name \(" } | ForEach-Object { $_.Trim() })
$runs = @($lines | Where-Object { $_ -match "^\s*=== RUN\s+$name/" }).Count
$passes = @($lines | Where-Object { $_ -match "^\s*--- PASS: $name/" }).Count
$failed = @($lines | Where-Object { $_ -match "^\s*--- FAIL: $name/" } | ForEach-Object { ($_.Trim() -split '\s+')[2] })

$verdict =
    if ($runExit -ne 0) { "FAIL: go test exited $runExit$(if ($failed.Count) { "; failing arm(s): $($failed -join ', ')" })" }
    elseif (-not $top.Count) { "FAIL: NOT RUN -- no '--- PASS: $Test' line: no test ran (filter '$Filter' matched nothing, or the test was renamed)" }
    elseif ($top[0] -notmatch '^--- PASS:') { "FAIL: NOT RUN -- $($top[0]): the test did not run its arms, so there is nothing to read" }
    elseif ($runs -eq 0) { "FAIL: NOT RUN -- $Test passed with no arm started" }
    elseif ($passes -ne $runs) { "FAIL: $passes of $runs arm(s) passed" }
    else { "PASS: $passes/$runs arm(s) ran as Go does or were refused as their table says, in $seconds s" }
Write-Host "==> release-smoke cgo $goos/${rid}: $verdict"

# Every arm's outcome by name, then each READING the test logs (Go's exit and output, then go2cs's).
$outcomes = @($lines | Where-Object { $_ -match '^\s*--- (PASS|FAIL|SKIP):' } | ForEach-Object { $_.Trim() })
$readings = @($lines | Where-Object { $_ -match 'READING ' } | ForEach-Object { $_.Trim() })
if ($readings.Count) { $outcomes += @('', 'Readings:') + $readings }

$summary = @(
    "## Release smoke cgo -- $goos ($rid)"
    ''
    $instrument
    ''
    "$verdict -- filter ``$Filter``, go test exit ``$runExit``"
    ''
    '```'
) + $outcomes + @('```')
if ($env:GITHUB_STEP_SUMMARY) {
    Add-Content -Path $env:GITHUB_STEP_SUMMARY -Value $summary
    & ./.github/annotate-summary.ps1 -Title "release-smoke cgo $goos/$rid" -Lines $summary
}

if ($verdict -like 'PASS:*') { exit 0 }
exit 1
