# run-h10-recon.ps1 - the H10 RECON LEG: run one worker's rows through the per-package pipeline and
# bank the per-row TSV that shardmap.py --timings reads.
#
# Ruled at COORD c7f68b53e / the pipeline post: the recon leg runs `go2cs -tests -test-action all` per
# row, NOT run-validated-sweep.ps1. The reason is enumeration, not preference: the sweep selects among
# BANKED rows (`-Filter ROW -Exact` throws "No banked packages matched" for anything else), so it cannot
# reach the twelve successors or the unbanked rows at all. A basis measured on two instruments is not a
# basis.
#
# ---------------------------------------------------------------------------------------------------
# WHAT THIS WRITES, AND WHY EVERY COLUMN IS WHERE IT IS
#
# shardmap.py:262 reads FOUR columns BY NAME and refuses a header lacking any of them:
#     row · word · verdicts · sweep_s
# It also refuses the file outright on ANY CR byte (:249), refuses a non-integer sweep_s by name
# ("a row with no measured cost is UNSCHEDULED, never nominal", :280), and refuses the whole basis if
# the hand-stopped drop never fired (:308) -- so `net` MUST be measured and emitted even though its
# cost is discarded. The extras below are additive; the parser reads by name and ignores them.
#
# ---------------------------------------------------------------------------------------------------
# THE ONE-ATTEMPT CLOCK
#
# sweep_s is this script's clock around the ONE pipeline invocation for the row. The recon leg makes a
# single attempt per row, so the oracle re-run inflation measured in run-validated-sweep.ps1 (it resets
# $rowStarted at :1105 and re-takes $rowSecs at :1107, so an EXTERNAL clock there spans both attempts)
# cannot arise here by construction. A row whose Go oracle is unstable is a READING -- its word says so
# -- not a re-run.
#
# ---------------------------------------------------------------------------------------------------
# TWO FORMAT STRINGS, NOT ONE  (C2 f9da1c467-line, verified here at the source)
#
# The converter prints the summary from TWO sites, chosen by whether the row carries disclosures:
#     testConversion.go:8271  "...(%d skipped..., %d disclosed-divergent (%s), %d ...excluded).\n"
#     testConversion.go:8274  "...(%d skipped..., %d disclosed-unsupported declarations excluded).\n"
# A wrapper asserting ONE literal refuses every row of the other kind. The assertion below is on the
# COMMON PREFIX and the captured integer; the tail is deliberately variable.
[CmdletBinding()]
param(
    # One package path per line; blank lines and #-comments ignored. This worker's rows, hand-listed.
    [Parameter(Mandatory)][string] $NameList,

    # The worktree at the version tip. Its src/core is the SEED for every row -- C2 measured that a
    # BARE output root fails the hand-own gate (sync rc 1 bare, rc 0 seeded), so the root is never a
    # scratch directory.
    [Parameter(Mandatory)][string] $Tree,

    # The pinned GOROOT. Asserted BY OUTPUT below, never by this string.
    [Parameter(Mandatory)][string] $GoRoot,

    [Parameter(Mandatory)][string] $Out,

    [string] $TestConfig   = 'Release',
    [string] $TestTimeout  = '30m',
    [string] $Platform     = '',

    # Run ONE row, print the matched summary line beside the emitted TSV row, write nothing. The
    # acceptance vehicle: decidable without a 228-row leg.
    [switch] $DryRun,

    # Exercise every guard and the format assertion, run no row, write nothing.
    [switch] $SelfTest
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

function Deny([string] $m) { Write-Host ''; Write-Host "RECON REFUSED: $m" -ForegroundColor Red; exit 2 }

# ---------------------------------------------------------------- the summary-line contract
# ONE definition, consulted by the assertion AND by the per-row parse, so the check cannot drift from
# the thing it checks (R's barmatch() shape; C1 paid for the replica lesson at a7c20e7cb).
$SummaryRe = '^Validated (\d+) tests against go test \('

function Get-VerdictCount([string[]] $lines) {
    $hits = @($lines | Where-Object { $_ -match $SummaryRe })
    # EXACTLY ONE. Zero is a row that produced no summary; more than one means the emission changed
    # shape and a first-match read would silently pick one.
    if ($hits.Count -ne 1) { return [pscustomobject]@{ Count = $null; Line = $null; Hits = $hits.Count } }
    $null = $hits[0] -match $SummaryRe
    return [pscustomobject]@{ Count = [int] $Matches[1]; Line = $hits[0]; Hits = 1 }
}

# ---------------------------------------------------------------- the format assertion (ruled)
# "the wrapper asserts the pipeline's summary-line format it parses (a planted line that fails to parse
# must refuse, not read 0)". Both real shapes must PARSE; a planted near-miss must NOT.
function Test-SummaryContract {
    $withDisclosures    = 'Validated 41 tests against go test (3 skipped identically on both sides, 2 disclosed-divergent (alloc-profile), 1 disclosed-unsupported declarations excluded).'
    $withoutDisclosures = 'Validated 302 tests against go test (7 skipped identically on both sides, 0 disclosed-unsupported declarations excluded).'
    $planted            = 'Validated some tests against go test (malformed).'
    $alsoPlanted        = 'Verified 41 tests against go test (3 skipped).'

    $a = Get-VerdictCount @($withDisclosures)
    $b = Get-VerdictCount @($withoutDisclosures)
    $c = Get-VerdictCount @($planted)
    $d = Get-VerdictCount @($alsoPlanted)
    $e = Get-VerdictCount @($withDisclosures, $withoutDisclosures)   # two lines: must refuse, not pick

    $ok = ($a.Count -eq 41) -and ($b.Count -eq 302) -and ($null -eq $c.Count) -and ($null -eq $d.Count) -and ($null -eq $e.Count)

    Write-Host '  summary-line contract:'
    Write-Host ("    with disclosures      -> {0,-6}  (want 41)"  -f $(if ($null -eq $a.Count) { 'REFUSE' } else { $a.Count }))
    Write-Host ("    without disclosures   -> {0,-6}  (want 302)" -f $(if ($null -eq $b.Count) { 'REFUSE' } else { $b.Count }))
    Write-Host ("    planted non-numeric   -> {0,-6}  (want REFUSE)" -f $(if ($null -eq $c.Count) { 'REFUSE' } else { $c.Count }))
    Write-Host ("    planted wrong verb    -> {0,-6}  (want REFUSE)" -f $(if ($null -eq $d.Count) { 'REFUSE' } else { $d.Count }))
    Write-Host ("    TWO matching lines    -> {0,-6}  (want REFUSE)" -f $(if ($null -eq $e.Count) { 'REFUSE' } else { $e.Count }))
    return $ok
}

# ---------------------------------------------------------------- preflight
Write-Host ''
Write-Host 'H10 recon leg -- preflight'

if ($SelfTest) {
    Write-Host ''
    if (Test-SummaryContract) { Write-Host '  SELF-TEST PASSED -- both real shapes parse, all three planted shapes refuse'; exit 0 }
    Deny 'the summary-line contract FAILED its self-test -- the parse and its controls disagree'
}

if (-not (Test-Path -LiteralPath $NameList)) { Deny "no name list at '$NameList'" }
if (-not (Test-Path -LiteralPath $Tree))     { Deny "no tree at '$Tree'" }
if (-not (Test-Path -LiteralPath (Join-Path $Tree 'src/core'))) { Deny "'$Tree' has no src/core -- the output root must be the worktree, whose src/core is the seed" }

# The pin BY OUTPUT, never by the string handed in. GOTOOLCHAIN=local is load-bearing on this fleet:
# R measured, and i9 reproduced, that a 1.24.13 SDK's own go.exe answers the MACHINE pin without it.
$env:GOROOT       = $GoRoot
$env:GOTOOLCHAIN  = 'local'
$env:CGO_ENABLED  = '0'

# ⚠ THE SHAPE CHECKS COME FIRST, AND EACH REFUSES BY NAME. Without them an absent GOROOT throws out of
# the `&` call under ErrorActionPreference=Stop and the run dies rc 1 with NO refusal line -- measured,
# and it made the pin control look like it had fired when it had only crashed. A guard that dies is not
# a guard that refused.
$goExe = Join-Path $GoRoot 'bin/go.exe'
$goVer = Join-Path $GoRoot 'VERSION'
if (-not (Test-Path -LiteralPath $GoRoot)) { Deny "no GOROOT at '$GoRoot'" }
if (-not (Test-Path -LiteralPath $goExe))  { Deny "'$GoRoot' has no bin/go.exe -- not a GOROOT" }
if (-not (Test-Path -LiteralPath $goVer))  { Deny "'$GoRoot' has no VERSION file -- the second derivation of the pin is unavailable" }

# ⚠⚠ PATH IS PART OF THE PIN, AND ASSERTING THE BINARY BY ABSOLUTE PATH DOES NOT ASSERT IT.
# The converter SPAWNS `go` from PATH (it shells out to parse the package). Measured on the one-row
# dry run: with GOROOT and GOTOOLCHAIN set but PATH untouched, the converter reached this box's
# ambient go1.23.1 and every row died
#     "go: ..\go.mod requires go >= 1.24 (running go 1.23.1; GOTOOLCHAIN=local)"
# -- while the preflight below reported the pin MET, because it invoked $goExe by ABSOLUTE PATH.
# A gate that proves the SDK at $GoRoot is the right release proves nothing about the `go` the
# converter will actually run. So: prepend, and then assert THROUGH PATH as well, which is the
# resolution the converter performs.
$env:PATH = (Join-Path $GoRoot 'bin') + [System.IO.Path]::PathSeparator + $env:PATH

$goVersion = (& $goExe version) 2>&1 | Select-Object -First 1
$pathGo    = (Get-Command go -ErrorAction SilentlyContinue)
if (-not $pathGo) { Deny "no 'go' resolvable on PATH after prepending '$GoRoot\bin'" }
$pathGoVer = (& go version) 2>&1 | Select-Object -First 1
$verFile   = (Get-Content -LiteralPath $goVer -TotalCount 1)
Write-Host "  go version OUTPUT : $goVersion   (the SDK at -GoRoot, by absolute path)"
Write-Host "  go ON PATH        : $pathGoVer   (what the converter will SPAWN)"
Write-Host "  go resolved from  : $($pathGo.Source)"
Write-Host "  GOROOT VERSION    : $verFile   (the second derivation)"
# A refusal that does not NAME the value it refused on sends the reader back to reproduce it.
if ($goVersion  -notmatch 'go1\.24\.13') { Deny "the pin did not take -- 'go version' OUTPUT is '$goVersion', not the corpus pin go1.24.13" }
if ($pathGoVer  -notmatch 'go1\.24\.13') { Deny "PATH's go is '$pathGoVer' -- the converter spawns THIS one, not the SDK at -GoRoot" }
if ($verFile    -notmatch 'go1\.24\.13') { Deny "GOROOT/VERSION is '$verFile', not the corpus pin go1.24.13" }
# THREE derivations of one pin, and they must agree: the binary, the binary PATH resolves, and the file.
if ($pathGo.Source -notlike (Join-Path $GoRoot '*')) { Deny "PATH's go resolves to '$($pathGo.Source)', which is not under the pinned GOROOT '$GoRoot'" }

$freeGb = [int] ((Get-PSDrive -Name ($Tree.Substring(0,1))).Free / 1GB)
Write-Host "  disk free         : $freeGb GB (floor 25)"
if ($freeGb -lt 25) { Deny "$freeGb GB free is under the ruled 25 GB floor" }

$converter = Join-Path $Tree 'src/go2cs/go2cs.exe'
if (-not (Test-Path -LiteralPath $converter)) {
    $converter = Join-Path $Tree 'src/go2cs/bin/Release/net10.0/go2cs.exe'
}
if (-not (Test-Path -LiteralPath $converter)) { Deny "no converter binary under '$Tree/src/go2cs' -- build it at this tree first" }
Write-Host "  converter         : $converter"

Write-Host ''
if (-not (Test-SummaryContract)) { Deny 'the summary-line contract FAILED -- refusing to parse rows with an unchecked predicate' }

# ---------------------------------------------------------------- the rows
$rows = @(Get-Content -LiteralPath $NameList |
          ForEach-Object { $_.Trim() } |
          Where-Object { $_ -and -not $_.StartsWith('#') })
if ($rows.Count -eq 0) { Deny "the name list holds no rows -- a basis over no rows is clean by construction" }

Write-Host ''
Write-Host "  rows in list      : $($rows.Count)"
Write-Host "  output            : $Out"
if ($DryRun) { Write-Host '  MODE              : DRY RUN -- one row, nothing written' -ForegroundColor Cyan }

$emit = New-Object System.Collections.Generic.List[string]
$emit.Add("row`tword`tverdicts`tsweep_s`tfirst_in_list`trc`tdiverged`tplatform`ttree")

$treeSha = (& git -C $Tree rev-parse HEAD) 2>&1
$plat = $Platform
if (-not $plat) { $plat = "$([System.Environment]::OSVersion.Platform)" }

$i = 0
foreach ($row in $rows) {
    $i++
    $first = 0
    if ($i -eq 1) { $first = 1 }

    $goDir  = Join-Path $GoRoot  ("src/" + $row)
    $outDir = Join-Path $Tree    ("src/core/" + $row)

    # -test-allow-handown for `testing` ONLY: the pipeline refuses that row by design, and the refusal
    # text prescribes the flag. Never -tags (the corpus axis comes by doing nothing, resolveBuildTags
    # applies purego,math_big_pure_go to every -tests run) and never -test-filter (its own warning says
    # a filtered run publishes NO validation artifacts and is DIAGNOSTIC ONLY -- it would not be a row).
    $extra = @()
    if ($row -eq 'testing') { $extra = @('-test-allow-handown') }

    Write-Host ''
    Write-Host ("  -> {0,-44} [{1} of {2}]" -f $row, $i, $rows.Count)

    if ($DryRun -and $i -gt 1) { Write-Host '     (dry run: stopping after one row)'; break }

    $started = Get-Date
    $output  = & $converter -tests -test-action all -test-config $TestConfig -test-timeout $TestTimeout `
                            -go2cspath (Join-Path $Tree 'src') @extra $goDir $outDir 2>&1
    # CAPTURED ON THE VERY NEXT LINE, before anything touches $? or a pipe. Floor 7, and the fault five
    # lanes hit in one night.
    $rc      = $LASTEXITCODE
    $elapsed = [int] ((Get-Date) - $started).TotalSeconds

    $lines = @($output | ForEach-Object { [string] $_ })
    $v = Get-VerdictCount $lines

    # ---- the word: the outcome class this cost was measured under (COORD's vocabulary)
    $diverged = ''
    if ($rc -ne 0 -and ($lines -match 'Conversion failed|unresolved dynamic')) { $word = 'CONVERT' }
    elseif ($lines -match 'error CS[0-9]+')                                    { $word = 'BUILD' }
    elseif ($lines -match 'timed out|timeout exceeded')                        { $word = 'TIMEOUT' }
    elseif ($null -eq $v.Count)                                                { $word = 'NOVERDICT' }
    else {
        $dl = @($lines | Where-Object { $_ -match 'diverged' })
        if ($dl.Count -gt 0) { $word = 'DIVERGED'; $diverged = $dl.Count } else { $word = 'PASS'; $diverged = 0 }
    }

    # ---- verdicts: cross-checked, and NEVER 0 on a failure to read.
    # shardmap reads a non-integer as None and still schedules the row; an APPROXIMATION poisons every
    # derived figure. So a disagreement or a miss emits the word NOMATCH, not a number.
    $verdicts = 'NOMATCH'
    if ($null -ne $v.Count) {
        $verdicts = $v.Count
        $cmp = Join-Path $outDir 'go2cs_test_comparison.json'
        if (Test-Path -LiteralPath $cmp) {
            try {
                $j = Get-Content -LiteralPath $cmp -Raw | ConvertFrom-Json
                # ORDINAL by the sweep's own comment: legal Go verdict names differ ONLY BY CASE, so a
                # case-insensitive count COLLAPSES those pairs and undercounts with a plausible integer.
                $goNames = @($j.go.PSObject.Properties.Name)
                $mapCount = ($goNames | Sort-Object -CaseSensitive -Unique).Count
                # ⚠⚠ THE TWO NUMBERS DIFFER BY THE DISCLOSURES, BY CONSTRUCTION -- measured on the
                # one-row dry run, where bufio read "summary 80 vs map 81" and my first cross-check
                # called that a disagreement. testConversion.go:8271 passes
                # `len(goResults) - len(disclosed)` as the headline count while the map holds ALL of
                # goResults, so a bare equality test fires on EVERY row carrying a disclosure and
                # throws away a perfectly good cost. The relation is:
                #     map == summary + disclosed-divergent
                # The no-disclosure branch (:8274) prints no such group, and 0 is then correct.
                $disclosed = 0
                if ($v.Line -match '(\d+) disclosed-divergent') { $disclosed = [int] $Matches[1] }
                if ($mapCount -ne ($v.Count + $disclosed)) {
                    Write-Host ("     !! verdicts DISAGREE: map $mapCount != summary $($v.Count) + disclosed $disclosed -- emitting NOMATCH") -ForegroundColor Yellow
                    $verdicts = 'NOMATCH'
                }
            } catch {
                Write-Host '     !! comparison JSON unreadable -- emitting the summary count unchecked' -ForegroundColor Yellow
            }
        }
    }

    $sweepS = $elapsed
    if ($word -eq 'NOVERDICT') {
        # No summary line means the row produced no verdict. Its WALL is real, but a cost banked under
        # no verdict is a number with no evidence behind it, so the row is UNSCHEDULED by construction.
        $sweepS = 'UNMEASURED'
    }

    Write-Host ("     {0,-10} verdicts={1,-8} {2}s  rc={3}" -f $word, $verdicts, $elapsed, $rc)
    if ($DryRun -and $v.Line) {
        Write-Host ''
        Write-Host '     the MATCHED summary line, beside the row it produced:' -ForegroundColor Cyan
        Write-Host "       $($v.Line)"
    }

    $emit.Add("$row`t$word`t$verdicts`t$sweepS`t$first`t$rc`t$diverged`t$plat`t$treeSha")

    if ($DryRun) { break }
}

# ---------------------------------------------------------------- write, LF only
$text = ($emit -join "`n") + "`n"
$crs  = ([regex]::Matches($text, "`r")).Count
Write-Host ''
Write-Host "  CR bytes in the emission: $crs   (shardmap.py refuses the basis on ANY CR)"
if ($crs -ne 0) { Deny "the emission carries $crs CR byte(s); the generator refuses a basis on any CR" }

if ($DryRun) {
    Write-Host ''
    Write-Host '  DRY RUN -- the row that WOULD be emitted:'
    Write-Host "    $($emit[$emit.Count - 1])"
    Write-Host ''
    Write-Host '  nothing written.'
    exit 0
}

[System.IO.File]::WriteAllText($Out, $text)
Write-Host ''
Write-Host "  written: $Out ($($emit.Count - 1) row(s), LF only)"

# The hand-stopped drop must be able to fire, or shardmap refuses the whole basis by name.
$hasNet = @($emit | Where-Object { $_ -match '^net\t' }).Count
if ($hasNet -eq 0) {
    Write-Host ''
    Write-Host '  !! NOTE: no `net` row in this emission. shardmap.py refuses a basis in which the' -ForegroundColor Yellow
    Write-Host '     hand-stopped drop never fired, so the COMBINED basis must carry it from some worker.' -ForegroundColor Yellow
}
exit 0
