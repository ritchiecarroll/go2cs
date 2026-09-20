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
# One extra is NOT decoration: `wall_s` is the observed integer wall for EVERY row whatever its
# word. `sweep_s` answers "may this row be scheduled on this number" and is deliberately
# non-integer when the row earned no cost; `wall_s` answers "how long did it take", which stays
# a fact for a row that timed out. The concatenation banks the hand-stopped row `net` with
# `sweep_s := wall_s`, and `net` is EXPECTED to TIMEOUT -- so without this column there is nothing
# to substitute from and the basis is refused either way.
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
#
# ---------------------------------------------------------------------------------------------------
# ENVIRONMENT, FOR A WORKER RUNNING THIS UNDER POWERSHELL CORE
#
# A `pwsh` installed as a DOTNET TOOL carries a net8 apphost and DIES AT LAUNCH if DOTNET_ROOT already
# points at the .NET 10 root (measured on the gate box). So DOTNET_ROOT is set INSIDE the child pwsh
# and never in the environment that launches it. This script does not set it for you: it is the
# caller's, because only the caller knows which shell it is starting.
#
# The pins this script asserts are GOROOT, GOTOOLCHAIN=local, CGO_ENABLED=0 and PATH -- see the block
# above the preflight for why PATH is one of them.
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

    # The TSV to write: a FILE PATH, never a directory. The write is this script's LAST
    # statement ([System.IO.File]::WriteAllText below), so a directory throws AFTER every row
    # has run and the whole leg is lost at its final line. Its parent must already exist.
    [Parameter(Mandatory)][string] $Out,

    # The commit -Tree must be detached at. Required: the leg's readings are only comparable if every
    # worker ran the same tree, and "the tip" is not a thing a script may assume.
    [Parameter(Mandatory)][string] $ExpectTip,

    # Where the per-row evidence and the TSV land. MUST be outside any git work tree -- it is the only
    # thing that survives the throwaway worktree being removed.
    [Parameter(Mandatory)][string] $Scratch,

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

# ⚠ NATIVE STDERR IS A TERMINATING ERROR IN 5.1 UNDER ErrorActionPreference=Stop, AND THE GOOD PATH IS
# THE ONE THAT WRITES IT. `git rev-parse --is-inside-work-tree` on a directory OUTSIDE a repository --
# which is exactly what the scratch must be -- prints "fatal: not a git repository" and PowerShell
# turns that into a NativeCommandError that kills the script. Measured: the guard refused its own
# success case. So every native git call goes through here, where stderr is discarded and the VERDICT
# is the exit code, read immediately.
function GitTry([string[]] $gitArgs) {
    $old = $ErrorActionPreference
    $ErrorActionPreference = 'Continue'
    try {
        $o = & git @gitArgs 2>$null
        $c = $LASTEXITCODE
        return [pscustomobject]@{ Out = (($o | Out-String).Trim()); Code = $c }
    } finally { $ErrorActionPreference = $old }
}

# ⚠ THE SAME FAULT GitTry EXISTS FOR, AT THE CALLS WHOSE TEXT IS THE MEASUREMENT.
# GitTry can discard stderr because its verdict is the EXIT CODE. The preflight's `go version` calls
# cannot: their OUTPUT is what the pin is asserted on, and the message the pin exists to catch --
# "go: ..\go.mod requires go >= 1.24 (running go 1.23.1; GOTOOLCHAIN=local)" -- arrives on stderr.
# Under Stop that message KILLS the preflight four lines before the Deny that would have named it,
# which is exactly what :192's comment warns about: a guard that dies is not a guard that refused.
# (C2 1e2adb3d, finding B. My own commit's comment stated the rule and left two call sites under it.)
function NativeFirstLine([scriptblock] $sb) {
    $old = $ErrorActionPreference
    $ErrorActionPreference = 'Continue'
    try {
        $o = & $sb 2>&1
        $ls = @($o | ForEach-Object { [string] $_ })
        if ($ls.Count -eq 0) { return '' }
        return $ls[0]
    } finally { $ErrorActionPreference = $old }
}

# For the calls whose verdict IS the exit code and whose stderr is noise -- GitTry's shape, generalised.
function NativeQuiet([scriptblock] $sb) {
    $old = $ErrorActionPreference
    $ErrorActionPreference = 'Continue'
    try {
        $o = & $sb 2>$null
        return (($o | Out-String).Trim())
    } finally { $ErrorActionPreference = $old }
}

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

# ---------------------------------------------------------------- the throwaway-worktree guard (ruled)
# `-test-action all` PUBLISHES the proof page, the index row and the README badge -- measured on the
# one-row dry run, where a SINGLE row re-banked bufio's proof page to 1.24.13 and removed 25 lines from
# the shared index. Those are the ROW ACT's artifacts; the recon leg produces READINGS. So the leg runs
# in a LINKED WORKTREE that is discarded whole afterwards, and this script refuses to run anywhere else.
#
# ⚠ The wrapper NEVER repairs the tree itself: no `git checkout --`, no `git clean`. A tool that cleans
# up after itself inside a work tree is one bad path away from discarding someone's work, and the
# discard is the caller's act on a tree it created for this.
$gd  = GitTry @('-C', $Tree, 'rev-parse', '--git-dir')
$gcd = GitTry @('-C', $Tree, 'rev-parse', '--git-common-dir')
if ($gd.Code -ne 0 -or $gcd.Code -ne 0) { Deny "'$Tree' is not a git work tree" }
# A LINKED worktree's --git-dir is <common>/worktrees/<name>; the MAIN checkout's two are equal.
if ($gd.Out -eq $gcd.Out) {
    Deny "'$Tree' is a MAIN checkout (--git-dir == --git-common-dir), not a linked worktree. The leg publishes validation artifacts and its tree is discarded afterwards -- run it in a linked worktree created for this list."
}
$sym = GitTry @('-C', $Tree, 'symbolic-ref', '-q', 'HEAD')
if ($sym.Code -eq 0 -and $sym.Out) { Deny "'$Tree' HEAD is on branch '$($sym.Out)' -- a recon tree is DETACHED, so nothing can be committed from it by habit" }
$head = GitTry @('-C', $Tree, 'rev-parse', 'HEAD')
if ($head.Out -ne $ExpectTip) { Deny "'$Tree' is at $($head.Out), not the expected tip $ExpectTip -- the leg's readings are comparable only across one tree" }

# The scratch must OUTLIVE the tree, so it must not be inside one.
# ⚠ The SUCCESS case here is git FAILING (not a repository). Read the code, never the stream.
if (-not (Test-Path -LiteralPath $Scratch)) { New-Item -ItemType Directory -Path $Scratch -Force | Out-Null }
$sc = GitTry @('-C', $Scratch, 'rev-parse', '--is-inside-work-tree')
if ($sc.Code -eq 0 -and $sc.Out -eq 'true') { Deny "the scratch '$Scratch' is inside a git work tree -- it holds the only copy of this leg's evidence and must survive the tree's removal" }

# The pin BY OUTPUT, never by the string handed in. GOTOOLCHAIN=local is load-bearing on this fleet:
# R measured, and i9 reproduced, that a 1.24.13 SDK's own go.exe answers the MACHINE pin without it.
$env:GOROOT       = $GoRoot
$env:GOTOOLCHAIN  = 'local'
$env:CGO_ENABLED  = '0'

# ⚠ THE SHAPE CHECKS COME FIRST, AND EACH REFUSES BY NAME. Without them an absent GOROOT throws out of
# the `&` call under ErrorActionPreference=Stop and the run dies rc 1 with NO refusal line -- measured,
# and it made the pin control look like it had fired when it had only crashed. A guard that dies is not
# a guard that refused.
# ⚠ THE EXECUTABLE SUFFIX IS NOT ALWAYS `.exe`, AND HARDCODING IT MISNAMES THE REFUSAL. The first cut
# joined 'bin/go.exe': on a non-Windows worker that path does not exist while 'bin/go' does, so the
# preflight refused a perfectly valid GOROOT with "not a GOROOT" -- a refusal naming a cause that is
# not the cause, which is worse than no refusal because it sends the reader to the wrong place.
# The runner's OS need not match a row's marker (ruled), so a linux worker taking bulk rows is a
# legitimate configuration, not a hypothetical.
function Find-Exe([string] $dir, [string] $stem) {
    foreach ($c in @((Join-Path $dir "$stem.exe"), (Join-Path $dir $stem))) {
        if (Test-Path -LiteralPath $c -PathType Leaf) { return $c }
    }
    return $null
}

$goVer = Join-Path $GoRoot 'VERSION'
if (-not (Test-Path -LiteralPath $GoRoot)) { Deny "no GOROOT at '$GoRoot'" }
$goExe = Find-Exe (Join-Path $GoRoot 'bin') 'go'
if (-not $goExe) { Deny "'$GoRoot/bin' holds neither 'go' nor 'go.exe' -- not a GOROOT" }
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

$goVersion = NativeFirstLine { & $goExe version }
$pathGo    = (Get-Command go -ErrorAction SilentlyContinue)
if (-not $pathGo) { Deny "no 'go' resolvable on PATH after prepending '$GoRoot\bin'" }
$pathGoVer = NativeFirstLine { & go version }
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

$converter = Find-Exe (Join-Path $Tree 'src/go2cs') 'go2cs'
if (-not $converter) { $converter = Find-Exe (Join-Path $Tree 'src/go2cs/bin/Release/net10.0') 'go2cs' }
if (-not $converter) { Deny "no converter binary ('go2cs' or 'go2cs.exe') under '$Tree/src/go2cs' -- build it at this tree first" }
Write-Host "  converter         : $converter"

Write-Host ''
if (-not (Test-SummaryContract)) { Deny 'the summary-line contract FAILED -- refusing to parse rows with an unchecked predicate' }

# ---------------------------------------------------------------- the rows
$rows = @(Get-Content -LiteralPath $NameList |
          ForEach-Object { $_.Trim() } |
          Where-Object { $_ -and -not $_.StartsWith('#') })
if ($rows.Count -eq 0) { Deny "the name list holds no rows -- a basis over no rows is clean by construction" }

# ---------------------------------------------------------------- the per-row deadline floors
# ⚠ DERIVED FROM THE SWEEP, NEVER COPIED. shardmap.py derives the reserved set from this same table
# for a stated reason -- "a copied list drifted twice in the map's short life (crypto/tls joined the
# table, two floors moved)" -- and a second copy here would be the third drift waiting to happen.
# A row without a floor gets the sweep's own default, which is this script's -TestTimeout.
$sweepPath = Join-Path $Tree 'src/run-validated-sweep.ps1'
if (-not (Test-Path -LiteralPath $sweepPath)) { Deny "no run-validated-sweep.ps1 at '$sweepPath' -- the deadline floors are derived from it, not carried here" }
$sweepSrc = [System.IO.File]::ReadAllText($sweepPath)
$anchor = [regex]::Match($sweepSrc, '\$longTimeouts\s*=\s*@\{')
if (-not $anchor.Success) { Deny "cannot derive the deadline floors: no `$longTimeouts table in run-validated-sweep.ps1" }
$close = $sweepSrc.IndexOf('}', $anchor.Index + $anchor.Length)
if ($close -lt 0) { Deny "the `$longTimeouts table is unterminated" }
$tableText = $sweepSrc.Substring($anchor.Index + $anchor.Length, $close - ($anchor.Index + $anchor.Length))
$Floors = @{}
foreach ($m in [regex]::Matches($tableText, "'([^']+)'\s*=\s*'([^']+)'")) { $Floors[$m.Groups[1].Value] = $m.Groups[2].Value }
# ⚠ A derivation that reads ZERO entries is the silent-empty shape the generator documents (a
# non-greedy match yielding 6 of 11, then 1, then 0). An empty table would silently give every row
# the default floor and under-run the long ones.
if ($Floors.Count -lt 5) { Deny "derived only $($Floors.Count) deadline floor(s) from run-validated-sweep.ps1 -- the table is 11 rows; refusing to run on a floor set that did not parse" }

# ⚠ THE FAN-OUT: a relocated row's floor is inherited by its SUCCESSOR(S) (e0d5121e2 section 1).
# `crypto/internal/mlkem768` is a floor row AND one of the ten relocated paths, and it fans out to
# two successors -- so its floor must reach both or they run at the default and are killed short.
# ⚠⚠ THIS IS A COPY, AND IT IS LABELLED AS ONE. Owner: C1. Source of record: C1's relocation table,
# ruled the map of record at `350a301a` and seated at `4de76ded06`. The first cut of this file carried
# a ten-entry copy that C2 measured already wrong in four ways (one row absent, one target wrong, two
# missing their second half); the second cut deleted nine of them and kept only the floored one. Both
# were wrong answers to the same question, and COORD ruled the third: carry the WHOLE map here, as a
# copy that says whose it is, and make the DURABLE form a data file.
#
# ⚠ THE DURABLE FORM IS NOT THIS. `docs/phase4/hopA-inputs/relocations.tsv` (one line per arc) lands
# with C1's roster seat after the leg, and the sweep's own $longTimeouts re-path is derived from it in
# that same commit. This map and C2's reserved-set derivation switch to reading that file in commits
# that land WITH the seat -- the correction and the act that would expose it are one landing. Until
# then a copy is what exists, so it is named rather than disguised.
#
# ⚠ SOURCE -> TARGETS, NEVER 1:1. Three of the ten rows SPLIT, and `crypto/internal/fips140test`
# receives arcs from three different rows -- so a table keyed either way drops arcs silently. Every
# arm of a split inherits the floor (e0d5121e2 section 1): a budget copied is an over-estimate, which
# is the safe direction; a budget split is a guess.
#
# ⚠ COUNTS, MEASURED FROM THE SET RATHER THAN CARRIED: 10 rows, 13 arcs, 11 distinct targets, 3 splits.
# C1's prose and COORD's ruling both say 14 arcs (and C1 "four of the ten split"); the table they
# publish lists 13 across 3 splits, and their own 11-distinct-targets figure is consistent only with
# 13 (13 arcs - 3 into fips140test + 1 = 11; 14 would give 12). 10 rows + 4 splits = 14 is arithmetic
# that closes on itself. The SET below is C1's table verbatim; only the count is corrected.
$FlooredSuccessors = @{
    'crypto/internal/alias'              = @('crypto/internal/fips140test')
    'crypto/internal/bigmod'             = @('crypto/internal/fips140/bigmod')
    'crypto/internal/edwards25519'       = @('crypto/internal/fips140/edwards25519', 'crypto/internal/fips140test')
    'crypto/internal/edwards25519/field' = @('crypto/internal/fips140/edwards25519/field')
    'crypto/internal/mlkem768'           = @('crypto/internal/fips140/mlkem', 'crypto/mlkem')
    'crypto/internal/nistec'             = @('crypto/internal/fips140/nistec', 'crypto/internal/fips140test')
    'internal/concurrent'                = @('internal/sync')
    'internal/weak'                      = @('weak')
    'runtime/internal/math'              = @('internal/runtime/math')
    'runtime/internal/sys'               = @('internal/runtime/sys')
}
$inherited = 0
foreach ($old in $FlooredSuccessors.Keys) {
    if ($Floors.ContainsKey($old)) {
        foreach ($new in $FlooredSuccessors[$old]) {
            if (-not $Floors.ContainsKey($new)) { $Floors[$new] = $Floors[$old]; $inherited++ }
        }
    }
}
# ⚠ THE ASSERTION STAYS, and it is what makes a stale copy loud instead of silent. A floored row that
# neither exists at the tree nor has a mapping REFUSES and is named, rather than running at the default
# floor and being killed short. That is exactly the transition ahead: when the roster seat re-paths the
# sweep's table, any floored row this copy does not cover stops the leg instead of under-running it.
$orphanFloors = @()
foreach ($k in @($Floors.Keys)) {
    if ($FlooredSuccessors.ContainsKey($k)) { continue }          # mapped; its successors carry it
    if (-not (Test-Path -LiteralPath (Join-Path $Tree ("src/core/" + $k)))) { $orphanFloors += $k }
}
if ($orphanFloors.Count -gt 0) {
    Deny ("floored row(s) absent from the tree with no successor mapping: " + ($orphanFloors -join ', ') +
          " -- each would run at the DEFAULT floor and be killed short. Map them here, or re-path the sweep's table, before running the leg.")
}
Write-Host "  deadline floors   : $($Floors.Count) derived from the sweep ($inherited inherited by a floored relocation)"

Write-Host ''
Write-Host "  rows in list      : $($rows.Count)"
Write-Host "  output            : $Out"
if ($DryRun) { Write-Host '  MODE              : DRY RUN -- one row, nothing written' -ForegroundColor Cyan }

$emit = New-Object System.Collections.Generic.List[string]
$emit.Add("row`tword`tverdicts`tsweep_s`tfirst_in_list`trc`tdiverged`tplatform`ttree`twall_s")

$treeSha = (GitTry @('-C', $Tree, 'rev-parse', 'HEAD')).Out
# ⚠ THE PLATFORM IS GOOS/GOARCH BY `go env` OUTPUT, NOT .NET's OSVersion.Platform. The first cut
# defaulted to the latter and emitted `Win32NT` -- caught on the i7's Core-edition run, and masked in
# my own testing because I passed -Platform explicitly every time, so the default was never exercised.
# `windows/amd64` is the roster's own spelling and the one every other reading in this campaign uses.
$plat = $Platform
if (-not $plat) {
    $goos   = NativeQuiet { & go env GOOS }
    $goarch = NativeQuiet { & go env GOARCH }
    if ($goos -and $goarch) { $plat = "$goos/$goarch" }
}
if (-not $plat) { Deny "cannot determine the platform -- `go env GOOS/GOARCH` produced nothing, and a row's reading is not comparable without it" }

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

    # The row's own deadline floor where it has one, the sweep's default otherwise.
    $rowTimeout = $TestTimeout
    if ($Floors.ContainsKey($row)) { $rowTimeout = $Floors[$row]; Write-Host "     floor: $rowTimeout (derived)" }

    # ⚠⚠ THE CONVERTER'S STDERR IS A TERMINATING ERROR UNDER Stop, AND FAILING IS THE MEASUREMENT.
    # `GitTry` above carries this exact diagnosis for git: in 5.1 a native command's stderr becomes a
    # NativeCommandError, which ErrorActionPreference='Stop' raises. I applied it to every git call and
    # NOT here -- to the one native call in this file whose FAILURE is a reading rather than a fault.
    #
    # Measured 2026-09-20, twice, at row 4 of 16: `crypto/mlkem` fails its `dotnet publish` (the ruled
    # CS0311 row), the converter says so on stderr, and the SCRIPT DIED -- mid-list, before the exit
    # code was read and before the classifier ran.
    #
    # ⚠ So the CONVERT and BUILD arms twenty lines below were UNREACHABLE BY CONSTRUCTION: a row cannot
    # be classified as failing, because failing is what stopped the run. Every control I had ever run
    # against the classifier used a row that SUCCEEDED, which is why nothing caught it. That is the
    # mirror of this file's own banked lesson -- there, a guard whose SUCCESS case wrote to stderr
    # passed all its refusal controls; here, a classifier whose FAILURE cases write to stderr can never
    # reach its failure arms.
    #
    # ⚠ `2>&1` alone does NOT fix it: the merged record is still an ErrorRecord and Stop raises it. The
    # preference must be lowered around the call. It is lowered ONLY around the call, and restored in a
    # finally, so every other statement keeps Stop.
    #
    # ⚠ And unlike GitTry, stderr is KEPT rather than discarded (`2>&1`, not `2>$null`): the classifier
    # reads the converter's stderr TEXT to separate CONVERT from BUILD.
    $started = Get-Date
    # ⚠ RESET PER ROW. $rc was assigned ONLY inside the try, so an invocation that threw before the
    # $LASTEXITCODE read -- or that never launched a process -- handed the classifier and the TSV the
    # PREVIOUS row's exit code, silently, and the row was classified from a number it did not produce.
    # Lowering the preference makes that narrow; narrow is not closed. (C2 1e2adb3d, finding A.)
    $rc = $null
    $prevEap = $ErrorActionPreference
    $ErrorActionPreference = 'Continue'
    try {
        $output = & $converter -tests -test-action all -test-config $TestConfig -test-timeout $rowTimeout `
                               -go2cspath (Join-Path $Tree 'src') @extra $goDir $outDir 2>&1
        # CAPTURED ON THE VERY NEXT LINE, before anything touches $? or a pipe. Floor 7, and the fault
        # five lanes hit in one night.
        $rc = $LASTEXITCODE
    } catch {
        # ⚠ WITHOUT THIS CATCH THE GUARD BELOW IS UNREACHABLE FOR THE CASE IT NAMES. A throw out of
        # the invocation propagates past `finally` and out of the loop, so the run dies rather than
        # refusing -- the very shape this file calls "a guard that dies is not a guard that refused".
        # The error text is kept as the row's output so the evidence capture still has something to
        # write, and $rc is left $null so the refusal below fires and NAMES the row.
        $output = @("RECON: the converter invocation threw: $($_.Exception.Message)")
    } finally { $ErrorActionPreference = $prevEap }
    # The file's own idiom, one line down from where it is already used on $diverged: refuse rather
    # than carry a value this row did not produce.
    if ($rc -isnot [int]) { Deny "row '$row' produced no exit code -- the invocation threw before `$LASTEXITCODE could be read. Refusing rather than classifying the row on a stale number." }
    $elapsed = [int] ((Get-Date) - $started).TotalSeconds

    $lines = @($output | ForEach-Object { [string] $_ })
    $v = Get-VerdictCount $lines

    # ---- the artifacts, read BEFORE the classifier, because two of them DECIDE it
    $cmpSrc = Join-Path $outDir 'go2cs_test_comparison.json'
    $resSrc = Join-Path $outDir 'go2cs_test_results.json'
    $resTail = @()
    if (Test-Path -LiteralPath $resSrc) { $resTail = @(Get-Content -LiteralPath $resSrc -Tail 400) }

    # ---- the word: the outcome class this cost was measured under (COORD's vocabulary)
    #
    # ⚠⚠ BUILD AND TIMEOUT ARE rc-GUARDED, and TIMEOUT is decided from the RESULTS TAIL, never from a
    # substring of the output. C2 measured the defect in the first cut: the two arms carried no rc
    # guard, so a row that PASSES whose output merely CONTAINS "timed out" was classed TIMEOUT -- and
    # since TIMEOUT now forces a non-integer sweep_s, that row leaves the plan. Eight rows in the
    # 207-row set carry the literal in their test sources, including `net` and `net/http`, which are
    # two the basis most needs. A deadline kill STATES ITSELF in the results file
    # (`"action":"timeout"`), so the authoritative signal is the artifact, not the console.
    $diverged = ''
    $timedOut = @($resTail | Where-Object { $_ -match '"action"\s*:\s*"timeout"' }).Count -gt 0
    if     ($rc -ne 0 -and ($lines -match 'Conversion failed|unresolved dynamic')) { $word = 'CONVERT' }
    elseif ($rc -ne 0 -and ($lines -match 'error CS[0-9]+'))                       { $word = 'BUILD' }
    elseif ($timedOut)                                                             { $word = 'TIMEOUT' }
    elseif ($null -eq $v.Count)                                                    { $word = 'NOVERDICT' }
    else {
        # ⚠ DISTINCT DIVERGING TEST NAMES from the comparison JSON -- not output lines, which count
        # mentions rather than tests. Ordinal/case-sensitive: legal Go test names differ only by case.
        $diverged = 0
        if (Test-Path -LiteralPath $cmpSrc) {
            try {
                $jj = Get-Content -LiteralPath $cmpSrc -Raw | ConvertFrom-Json
                $goMap = @{}
                foreach ($p in $jj.go.PSObject.Properties)     { $goMap[$p.Name] = [string] $p.Value }
                $csMap = @{}
                foreach ($p in $jj.csharp.PSObject.Properties) { $csMap[$p.Name] = [string] $p.Value }
                # ⚠ THE DISCLOSED ONES ARE SUBTRACTED, AND THEY ARE PROSE, NOT NAMES. `disclosed` is a
                # list of SENTENCES -- "TestReadStringAllocs (alloc-profile): at-most-one AllocsPerRun
                # assert: ..." -- so a membership test against the whole string never matches and every
                # disclosed row reads DIVERGED. Measured on bufio: 1 diverging name, and it IS the
                # disclosed one, so the NET is 0 and the row is a PASS. The name is the leading token.
                $disclosedNames = New-Object System.Collections.Generic.HashSet[string] ([System.StringComparer]::Ordinal)
                foreach ($s in @($jj.disclosed)) {
                    if ($s -and ($s -match '^(\S+)')) { $null = $disclosedNames.Add($Matches[1]) }
                }
                $names = New-Object System.Collections.Generic.HashSet[string] ([System.StringComparer]::Ordinal)
                foreach ($k in $goMap.Keys) { $null = $names.Add($k) }
                foreach ($k in $csMap.Keys) { $null = $names.Add($k) }
                $d = 0
                foreach ($n in $names) {
                    if ($disclosedNames.Contains($n)) { continue }
                    $g = ''; if ($goMap.ContainsKey($n)) { $g = $goMap[$n] }
                    $c = ''; if ($csMap.ContainsKey($n)) { $c = $csMap[$n] }
                    if (-not [string]::Equals($g, $c, [System.StringComparison]::Ordinal)) { $d++ }
                }
                # `diverged` is the NET, UNDISCLOSED count: a disclosed divergence is accounted for by
                # the roster and is not a finding. The artifact's own `matched`/`status` corroborate it,
                # and a disagreement is reported rather than resolved silently.
                $diverged = $d
                if ($d -eq 0 -and $jj.matched -ne $true) {
                    Write-Host "     !! net diverged 0 but the artifact says matched=$($jj.matched) status=$($jj.status)" -ForegroundColor Yellow
                }
            } catch { $diverged = 'UNREAD' }
        }
        # ⚠ AN UNREADABLE ARTIFACT IS NOT A PASS. The first cut fell to PASS whenever $diverged was not
        # an int, so a comparison JSON nobody could read produced the basis's strongest verdict --
        # while `verdicts`, twenty lines down, refuses to guess for exactly the stated reason. `word`
        # is the only record of WHICH verdict a cost was measured under, and a reader filtering on it
        # would have seen a pass over an artifact that was never read. NOVERDICT is the honest class:
        # the row ran, and this instrument cannot say what it did.
        if     ($diverged -isnot [int]) { $word = 'NOVERDICT' }
        elseif ($diverged -gt 0)        { $word = 'DIVERGED' }
        else                            { $word = 'PASS' }
    }

    # ---- verdicts: cross-checked, and NEVER 0 on a failure to read.
    # shardmap reads a non-integer as None and still schedules the row; an APPROXIMATION poisons every
    # derived figure. So a disagreement or a miss emits the word NOMATCH, not a number.
    $verdicts = 'NOMATCH'
    if ($null -ne $v.Count) {
        $verdicts = $v.Count
        if (Test-Path -LiteralPath $cmpSrc) {
            try {
                $j = Get-Content -LiteralPath $cmpSrc -Raw | ConvertFrom-Json
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

    # ⚠ wall_s IS THE OBSERVED WALL FOR EVERY ROW, WHATEVER ITS WORD, AND IT IS ALWAYS AN INTEGER.
    # `sweep_s` answers "what may this row be SCHEDULED on" and is deliberately non-integer when the
    # row earned no cost. `wall_s` answers the different question "how long did this actually take",
    # which is still a fact for a row that timed out or produced no verdict -- and it is the only
    # number the hand-stopped row `net` can ever supply, because `net` is expected to TIMEOUT and its
    # cost in both 1.23 passes was a lower bound produced by stopping it. The concatenation banks
    # `net` with `sweep_s := wall_s`; without this column there is nothing to substitute FROM, and the
    # basis is refused either way (C2 `7c71a87f`, COORD's ruling).
    $wallS  = $elapsed
    $sweepS = $elapsed
    if ($word -eq 'TIMEOUT') {
        # A deadline kill is not a cost: the row did not finish, so its wall is a floor the operator
        # imposed, not a measurement of the row. NON-INTEGER -- which the generator REFUSES (:280, by
        # name and by value); the banked basis excludes such rows at the concatenation.
        $sweepS = 'UNMEASURED'
    }
    if ($word -eq 'NOVERDICT') {
        # No summary line means the row produced no verdict. Its WALL is real -- and is emitted as
        # wall_s -- but a cost banked under no verdict is a number with no evidence behind it, so the
        # generator REFUSES it and the concatenation excludes the row.
        $sweepS = 'UNMEASURED'
    }

    # ⚠ AN EMPTY FIELD READS AS A ZERO. A row that produced no comparison artifact at all -- BUILD,
    # CONVERT, TIMEOUT, NOVERDICT -- left `diverged` at its initial empty string, and an empty column
    # in a file three lanes are concatenated into is the empty-counter class this fleet banked twice
    # tonight. `n/a` and not `UNREAD`: UNREAD means an artifact EXISTED and could not be read, which
    # is a different and worse fact than never having produced one.
    if ($diverged -eq '') { $diverged = 'n/a' }

    # ---- CAPTURE THE EVIDENCE BEFORE THE TREE IS DISCARDED (ruled)
    # The worktree is removed when the list is done, and with it every artifact this row produced.
    # The diverged sets are the leg's readings; losing them would make the run unrepeatable without
    # re-running it. Keyed by row, under the scratch, which the guard proved is outside any work tree.
    $rowKey = ($row -replace '[\\/]', '__')
    $rowDir = Join-Path $Scratch $rowKey
    if (-not (Test-Path -LiteralPath $rowDir)) { New-Item -ItemType Directory -Path $rowDir -Force | Out-Null }

    # $cmpSrc / $resSrc / $resTail are read ABOVE, before the classifier, because the tail DECIDES
    # the TIMEOUT arm. One definition, used twice: the classifier reads it, and this copies it out.
    if (Test-Path -LiteralPath $cmpSrc) { Copy-Item -LiteralPath $cmpSrc -Destination (Join-Path $rowDir 'go2cs_test_comparison.json') -Force }
    if ($resTail.Count -gt 0) {
        # The TAIL, because the results file is large and its end is where a deadline kill states
        # itself -- C1's rule: an empty C# column has three causes, and this artifact separates one.
        [System.IO.File]::WriteAllText((Join-Path $rowDir 'results-tail.txt'), (($resTail -join "`n") + "`n"))
    }

    # The summary line itself, and the whole stdout when there ISN'T one -- a row with no summary is
    # exactly the row whose output someone will want to read, and it is the row whose tree is discarded.
    if ($v.Line) { [System.IO.File]::WriteAllText((Join-Path $rowDir 'summary.txt'), $v.Line + "`n") }
    else         { [System.IO.File]::WriteAllText((Join-Path $rowDir 'output-no-summary.txt'), (($lines -join "`n") + "`n")) }

    Write-Host ("     {0,-10} verdicts={1,-8} {2}s  rc={3}   evidence -> {4}" -f $word, $verdicts, $elapsed, $rc, $rowKey)
    if ($DryRun -and $v.Line) {
        Write-Host ''
        Write-Host '     the MATCHED summary line, beside the row it produced:' -ForegroundColor Cyan
        Write-Host "       $($v.Line)"
    }

    $emit.Add("$row`t$word`t$verdicts`t$sweepS`t$first`t$rc`t$diverged`t$plat`t$treeSha`t$wallS")

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
