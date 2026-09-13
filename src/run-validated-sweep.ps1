# run-validated-sweep.ps1 - re-validate every banked Phase-4 package against `go test`.
#
# The charter's operational gate: after any change to golib, go2cs-gen, the converter or the
# corpus, every already-validated package must still validate -- verdict for verdict, at its
# exact banked count. This script is that gate. (Three narrow, named exceptions, each PROVEN from
# the run's own evidence before it is granted: a row that declares HOST-CONDITIONAL verdicts may
# exceed its banked floor by exactly those named rows (Test-HostConditionalDelta); a row with a
# registered capability-conditional block may fall short by exactly that block when the capability
# is ABSENT and both runtimes collapse together (Test-CapabilityAbsentDelta); and the same row may
# fall short by exactly that block when the capability is PRESENT but the converted side cannot
# produce it inside the test's own deadline AND the package's committed manifest already discloses
# the block root as `host-limit` (Test-HostLimitDelta). Any other count movement is still a failure.)
#
# ONE further exception, and it is not a count exception at all: a row whose failure set is
# ORACLE-ONLY -- every divergence Go=fail with C#=pass, no converted-side failure, no deadline or
# crash signature, both sides' entry counts complete -- is RE-RUN ONCE before it fails, because the
# three-run flake standard is a property of measurement and applies to whichever side moved
# (Test-OracleOnlyFailure, coordinator ruling 2026-09-02, from a crypto/tls run whose converted side
# passed 3,644 rows and whose oracle flaked eight). A clean re-run banks and says so loudly; a second
# oracle-only result is `oracle unstable on this host`, counted apart from FAIL and still non-zero.
# The roster grows NO arm for this: no annotation, no count moves, no classification bucket.
#
# The roster is READ FROM docs/ValidatedTestPackages.md rather than hardcoded, so it can never
# drift from the table a banking commit just updated: the table is the single source of truth for
# which packages are banked and what counts they carry. (The parsing lives in _roster.ps1, guarded
# by check-roster-format.ps1.)
#
# A banked count is a fact about (package, OS): Go runs a different test set per GOOS, so a row's
# count on Linux need not be its count on Windows and neither number is wrong. The columns are the
# WINDOWS record; a row that has validated elsewhere carries that OS's arithmetic as an annotation,
# and this script validates against whichever expectation its target OS puts in force. Where a
# non-Windows run has no annotation to bank against it reports comparison-validated-at-count --
# neither a pass nor a drift failure. On Windows none of that machinery moves: the columns answer
# for every row, exactly as before.
#
#   ./run-validated-sweep.ps1                       # every banked package
#   ./run-validated-sweep.ps1 -Filter compress      # just the ones whose path contains "compress"
#   ./run-validated-sweep.ps1 -TestTimeout 15m      # slower machine / contended box (a value LARGER
#                                                   #   than a $longTimeouts floor raises that too)
#   ./run-validated-sweep.ps1 -SkipBuild            # reuse the current go2cs.exe as-is
#   ./run-validated-sweep.ps1 -IgnoreDiskPreflight  # proceed on a nearly-full drive anyway
#   ./run-validated-sweep.ps1                       # DEFAULT since 2026-09-02: Release, tiering
#                                                   #   OFF, per-row `execution:` annotations
#                                                   #   RESPECTED (three rows opt back into tiering
#                                                   #   via release-tiered) -- the bank-eligible path
#   ./run-validated-sweep.ps1 -TestConfig Release   # A/B measurement, not a bank-eligible sweep --
#                                                   #   EXPLICITLY passing either flag (even the
#                                                   #   default's own value) makes EVERY row publish
#                                                   #   under it, annotated or NOT, superseding the
#                                                   #   per-row annotations. That is the difference
#                                                   #   between this line and the one above: same
#                                                   #   config, different treatment of the roster.
#                                                   #   Untiered by default; add -TestTiered to opt
#                                                   #   back in, exactly like the pipeline's own
#                                                   #   -test-config/-test-tiered it threads to
[CmdletBinding()]
param(
    [string] $Filter,
    # WELL above the pipeline's 2-minute default ON PURPOSE. regexp and strings legitimately run
    # for minutes (regexp exercises the whole RE2 corpus; strings' TestCompareStrings alone is
    # ~110 s), and at the default they report as failures when they are merely slow -- a false
    # red that costs an investigation every time someone hits it.
    [string] $TestTimeout = '10m',
    [switch] $SkipBuild,
    # -Filter matches by EXACT package path instead of the default substring -like. A per-package
    # campaign driver needs this: substring 'io' sweeps bufio, io/fs and every other 'io'-bearing
    # row alongside io itself, so a driver iterating the roster one package at a time re-sweeps
    # large rows repeatedly. Substring stays the interactive default, unchanged.
    [switch] $Exact,
    [switch] $IgnoreDiskPreflight,
    # RELEASE-HOP RE-DERIVATION (H10), coordinator ruling 2026-09-13. At a hop the corpus moves to a
    # release whose counts nobody has banked, so this gate's central comparison -- a measured count
    # against a banked floor -- has nothing true to say: Go adds and removes tests between releases,
    # so a count BELOW its floor is not a lost verdict and a count ABOVE it is not an unexplained
    # gain. -Hop replaces that ONE comparison with a RECORD, and changes nothing else.
    #
    # ⚠ It is NOT an amnesty, and that distinction is the whole design. Everything that is not a count
    # comparison still fails exactly as it did: a build error, a host death, an empty results file, a
    # deadline kill, an infrastructure-blocked package, an oracle-unstable row, the disk floor -- and
    # the toolchain pin. The pin especially: -Hop does NOT touch it, because the guard's own throw
    # already names the hop path ("or, if the corpus is deliberately moving to $runningRelease, bump
    # `<GoStdLibVersion>` in version.props first"). H2 is therefore this mode's PRECONDITION, and a
    # -Hop run attempted before H2 refuses on that unmodified guard, correctly.
    #
    # Two further properties, each ruled rather than chosen here:
    #   * the row population is the census SKELETON, not the banked roster (Get-HopSkeletonRows)
    #   * "ran and produced counts" and "ran here, and there are no eligible tests" are TWO WORDS in
    #     the record, decided by what the run PRODUCED and never by a table -- because the incoming
    #     release's `n/a` annotations are DERIVED from the second word, and a broken row that
    #     annotated itself platform-exclusive would poison that derivation at its source
    [switch] $Hop,
    # Sweep-wide A/B measurement (docs/phase4 tiering/configuration census), 2026-09-02: generalized
    # from the old blanket -ReleaseTC0 switch to mirror the pipeline's own -test-config/-test-tiered
    # (the SAME flags -- Get-RosterExecutionArgs and this pair now emit identical argument shapes for
    # the Release+untiered case, so there are not two mechanisms to keep in sync, only one). Runs
    # EVERY row under the given config, annotated or not, and OVERRIDES a row's own per-row
    # `execution: release-tc0` roster annotation for the duration of this sweep. Distinct from that
    # annotation (owner ruling 2026-08-30), which opts ONE row in and IS bank-eligible: this blanket
    # form is not, because a row banked under a config its own roster line does not declare cannot be
    # reproduced from the table. Default 'Debug' changes NOTHING -- the roster's meaning, and every
    # row's invocation, stay character-for-character what they were before this parameter existed.
    [ValidateScript({
        if ($_ -notin @('Debug', 'Release')) {
            throw "-TestConfig must be 'Debug' or 'Release' (got '$_') -- the converter itself only recognizes those two."
        }
        $true
    })]
    # DEFAULT FLIPPED to Release 2026-09-02 (owner ruling: the validation configuration of record is
    # Release with tiering off; Debug stays available by flag; the defaults flip after the Release
    # census, which is complete -- docs/phase4/CENSUS-release-tc0-delta.md). The paragraph above's
    # "Default 'Debug' changes NOTHING" no longer describes the default; it still describes what an
    # EXPLICIT -TestConfig does, which is what the override predicate below now keys on.
    [string] $TestConfig = 'Release',
    # Meaningless with -TestConfig Debug (same rule as the converter's own -test-tiered). With
    # -TestConfig Release, opts back IN to the CLR's default tiered JIT -- Release's own default here
    # is DOTNET_TieredCompilation=0, since a verdict that depends on JIT promotion timing is not
    # reproducible run to run (the same reasoning -test-config's own commit recorded).
    [switch] $TestTiered,
    # Split the (already Filter/Exact/Applicable-filtered) row set into -ShardCount contiguous,
    # roster-order pieces and run only the -ShardIndex'th (1-based) -- owner ruling 2026-09-02, this
    # host's own known thermal limit: a ~2-hour continuous full-roster run is exactly the load that
    # trips it, so a multi-hour census is broken into shards with a cooldown gap BETWEEN separate
    # invocations (the gap is the caller's job, not this script's -- it is not a sleep this process
    # would hold a build lock through). Both omitted (the default) runs the whole set in one piece,
    # unchanged from before this parameter existed. `-ShardCount 1` is accepted as a no-op spelling.
    [ValidateRange(1, [int]::MaxValue)]
    [int] $ShardCount = 1,
    [ValidateRange(1, [int]::MaxValue)]
    [int] $ShardIndex = 1
)

if ($ShardIndex -gt $ShardCount) {
    Write-Host "*** -ShardIndex $ShardIndex exceeds -ShardCount $ShardCount ***" -ForegroundColor Red
    exit 1
}

$ErrorActionPreference = 'Stop'

# ---- disk preflight -----------------------------------------------------------------------------
# Three separate 2026-08-13 incidents traced back to a full repo drive, and NOT ONE of them named the
# disk in its own output: writes failed MID-RUN and surfaced as corpus FAILURES (false reds nobody
# could reproduce), and a failed write left a TRACKED file TRUNCATED, which then reads as real drift.
# Both shapes cost an investigation before anyone thought to look at free space, so this names the
# number first. GetPathRoot + DriveInfo is the portable pair: 'D:\' on Windows, '/' elsewhere.
$freeGB = [math]::Round(([System.IO.DriveInfo]::new([System.IO.Path]::GetPathRoot($PSScriptRoot))).AvailableFreeSpace / 1GB, 1)

if ($freeGB -lt 25) {
    Write-Host "*** DISK PREFLIGHT: $freeGB GB free on the repo drive -- below the 25 GB floor ***" -ForegroundColor Red
    Write-Host '    Below this, writes fail mid-run: builds and conversions report FALSE REDS, and a' -ForegroundColor Red
    Write-Host '    partial write leaves a TRACKED FILE TRUNCATED (three such incidents, 2026-08-13).' -ForegroundColor Red
    Write-Host '    Free space, or pass -IgnoreDiskPreflight to proceed with unmeasurable results.' -ForegroundColor Red

    if (-not $IgnoreDiskPreflight) { exit 1 }
}

# Roots, the converter path and the executable suffix come from one shared definition (src\_paths.ps1)
# so this gate cannot disagree with the behavioral instruments about where anything is -- and so it
# carries no backslash literal, which off Windows fails SILENTLY rather than loudly (F4,
# docs/PLAN-linux-operation.md). Every path below is joined with a single forward-slash child
# argument: PowerShell 5.1 and 7+ both normalize that to the host separator, so the strings handed to
# the converter are byte-identical to the ones the backslash literals produced on Windows.
. (Join-Path $PSScriptRoot '_paths.ps1')
# The roster reader (columns, host-conditional names, per-OS expectations). Dot-sourced AFTER
# _paths.ps1, which is what pins $env:GoTargetOS on a Linux host -- the variable Get-SweepTargetGoos
# reads to decide which OS's expectation this run is measuring against.
. (Join-Path $PSScriptRoot '_roster.ps1')

# ---- cgo pin, WHOLE RUN, every platform (coordinator ruling 2026-09-03, mailbox a29200e47) -------
# Both comparison sides share ONE cgo state and the converted side can only be the corpus's, which
# is emitted CGO_ENABLED=0 (CLAUDE.md's emission-state rule). The state matters TWICE: it selects
# which Go FILES convert (os/user, net, plugin, reflect -- the rows the retired per-package table
# below first pinned), and it selects which BRANCH the ORACLE'S OWN TESTS take (`testenv.HasCGO()`
# adds cgo-built variants in debug/buildinfo, cgo-only subtests in debug/pe and go/internal/gcimporter,
# ENOTSUP skips in syscall), so a row's verdict COUNT carries the cgo state it was taken under. The
# Windows bank host has no C compiler and always ran cgo-off; the Linux bank host's default was
# cgo-ON, and the Linux annotations banked under it read HIGH by exactly the oracle's cgo-gated tests
# (measured on the oracle alone at 22d2bd9dc: buildinfo 200 vs 193 PASS, gcimporter 568 vs 567, pe
# three extra windows-only skip/skip subtests). cgo OFF is the state of record on every platform;
# pinning it here, once, is what makes every row's number comparable across hosts and OSes.
$priorSweepCgo = $env:CGO_ENABLED
$env:CGO_ENABLED = '0'
Write-Host "  CGO_ENABLED=0 pinned for the whole run (was '$priorSweepCgo') -- the corpus emission state and the annotations' state of record on every platform" -ForegroundColor DarkGray

$src = $SrcRoot
$repo = $RepoRoot
$table = Join-Path $repo 'docs/ValidatedTestPackages.md'
# The -Hop row population. A separate path rather than a re-pointed $table, because the two files are
# different KINDS: the roster is the record of what is banked, the skeleton the record of what is
# ELIGIBLE at the incoming release. -Hop reads the second and writes nothing; the roster stays the
# single source of truth for banked counts, and H10 banks into it from a hop run's record.
$hopSkeleton = Join-Path $repo 'docs/phase4/CENSUS-h10-eligibility-go124.md'
$exe = $Go2csExe
$goroot = (& go env GOROOT).Trim()

if (-not (Test-Path $table)) { throw "Cannot find the validated-package table at $table" }
if ($Hop -and -not (Test-Path $hopSkeleton)) { throw "Cannot find the H10 eligibility census at $hopSkeleton" }
if (-not $goroot) { throw 'Could not resolve GOROOT -- is the Go toolchain on PATH?' }

# ---- toolchain pin ------------------------------------------------------------------------------
# The sweep re-runs each package's Go tests from GOROOT's SOURCES and compares the verdicts against
# counts banked from one specific Go release, so the toolchain silently decides what is being
# measured. On the wrong release this compares that toolchain's tests against the pinned release's
# banked numbers, and nothing reports it, because each side stays internally consistent -- which is
# how a lane's gates once passed at banked counts on a toolchain the corpus was never pinned to.
# In the sweep's own terms such a run is NOT MEASURED, never a verdict, so it stops here rather than
# printing counts nobody can bank. Same comparison and same failure shape as the converter's own
# -stdlib/-tests guard (checkCorpusToolchainPin, src/go2cs/toolchainResolution.go).

# GOROOT's own VERSION file is preferred over `go env GOVERSION`, because the two can disagree and
# when they do it is the reported version that misdescribes what is being read: a GOROOT environment
# variable overrides the selected toolchain's own root, so a 1.23.1 go binary can report go1.23.1
# while GOROOT holds a 1.23.2 tree. The SOURCES are what the banked counts came from, so they win.
# Same precedence as the converter's convertingRelease.
$goversion = (& go env GOVERSION).Trim()
$gorootVersionFile = Join-Path $goroot 'VERSION'

if (Test-Path $gorootVersionFile) {
    # Go 1.21 added a `time <stamp>` line beneath the release; the release is the first line.
    $firstLine = Get-Content $gorootVersionFile -TotalCount 1

    if ($firstLine) { $goversion = $firstLine.Trim() }
}

$versionProps = Join-Path $src 'version.props'
$pinnedRelease = ''

if (Test-Path $versionProps) {
    $propsText = [System.IO.File]::ReadAllText($versionProps)

    if ($propsText -match '<GoStdLibVersion>([^<]+)</GoStdLibVersion>') {
        $pinnedRelease = $Matches[1].Trim()
    }
}

# An unreadable version on EITHER side is not a mismatch -- there is nothing to assert against, and
# refusing there would stop legitimate runs. Only a known disagreement fails.
if ($pinnedRelease -and $goversion) {
    $runningRelease = $goversion -replace '^go', ''

    if ($runningRelease -ne $pinnedRelease) {
        throw ("Toolchain pin: version.props pins the corpus to Go $pinnedRelease " +
            "(<GoStdLibVersion>$pinnedRelease</GoStdLibVersion>), but the Go tree this run would " +
            "read is $goversion, at GOROOT $goroot. The sweep re-runs each package's Go tests from " +
            "those sources, so it would measure go$runningRelease's tests against counts banked " +
            "from $pinnedRelease -- NOT MEASURED, never a verdict. Either run on go$pinnedRelease " +
            "with GOROOT pointing at that same tree, or, if the corpus is deliberately moving to " +
            "$runningRelease, bump <GoStdLibVersion> in version.props first. (If GOVERSION already " +
            "says go$pinnedRelease, check for a GOROOT environment variable overriding the selected " +
            "toolchain -- that mismatch is what this reads.)")
    }
}

# ---- roster: parse the table's rows -------------------------------------------------------------
# Row shape:  | [`net/http/internal/ascii`](https://...) | 13 | 1 | What it exercises. |
# Column 2 is the matching-verdict count, column 3 the disclosed count (blank when none), and the
# What-it-exercises cell may carry two annotations: HOST-CONDITIONAL verdicts (acceptance rules on
# Test-HostConditionalDelta below) and the PER-OS expectation this script honors below that.
#
# The parsing itself lives in src\_roster.ps1, dot-sourced above, for one reason: the per-OS
# annotation is a rule with an arithmetic consequence, and a rule with a consequence needs a guard
# that can exercise it without running this multi-hour gate. That guard is src\check-roster-format.ps1.
#
# Under -Hop the SOURCE switches and nothing else about the row pipeline does: Get-HopSkeletonRows
# returns the same property shape, so Filter/Exact, the sharding, the invocation and every verdict
# path below read a hop row exactly as they read a roster row. The roster is not consulted at all on
# a hop run -- it holds the OUTGOING release's counts, and comparing against those is the one thing
# this mode exists to stop doing.
$rows = if ($Hop) { Get-HopSkeletonRows -Path $hopSkeleton } else { Get-ValidatedRosterRows -Path $table }

if ($Filter) {
    $rows = if ($Exact) { $rows | Where-Object { $_.Package -eq $Filter } }
            else        { $rows | Where-Object { $_.Package -like "*$Filter*" } }
}
# @() so a single match stays an array -- PowerShell unwraps a one-element pipeline to a scalar,
# which has no .Count and would print a blank package count.
$rows = @($rows)
if (-not $rows) {
    $population = if ($Hop) { 'eligible packages (hop skeleton)' } else { 'banked packages' }
    throw "No $population matched$(if ($Filter) { " filter '$Filter'" })."
}

# ---- the OS dimension ----------------------------------------------------------------------------
# A verdict count is a fact about (package, OS): Go itself runs a different test set per GOOS, so
# crypto/rand offers 302 eligible verdicts on Linux against the banked Windows 298. Each row's
# EFFECTIVE expectation is therefore resolved once, here: its annotation for this run's target OS
# where one exists, the Windows columns otherwise.
#
# On Windows this is a no-op by construction -- Get-RosterRowExpectation returns the columns for
# every row, Source 'columns', and every line printed below is the line that was printed before the
# dimension existed. That is the invariant the Windows-unchanged proof rests on.
$targetGoos = Get-SweepTargetGoos

foreach ($row in $rows) {
    # A hop row has no expectation to resolve, in EITHER direction: no columns (the skeleton's counts
    # are blank by design) and no per-OS annotation (it carries none at the incoming release -- those
    # annotations are what a hop run's record lets H10 derive). So Get-RosterRowExpectation is not
    # consulted; the row is given an expectation that is honest about being absent, and Applicable is
    # TRUE because eligibility here is decided by what the run produces, never by a table cell.
    $effective = if ($Hop) {
        [PSCustomObject]@{ Expected = $null; Disclosed = $null; Applicable = $true; Source = 'hop' }
    }
    else {
        Get-RosterRowExpectation -Row $row -Goos $targetGoos
    }

    Add-Member -InputObject $row -NotePropertyName 'Effective' -Force -NotePropertyValue $effective
}

# A row annotated `<goos>: n/a` cannot exist on this OS (ruled 2026-08-29, the registry row):
# never pending, never validated -- reported by name and removed before any arithmetic, so the
# header sums and the exit code describe only what can be measured. Windows is untouched by
# construction: the n/a annotation never answers for the columns.
$notApplicableRows = @($rows | Where-Object { -not $_.Effective.Applicable })
foreach ($naRow in $notApplicableRows) {
    Write-Host ("  N/A   {0,-34} {1}: n/a -- platform-exclusive row; no expectation exists here, now or ever" -f $naRow.Package, $targetGoos) -ForegroundColor DarkGray
}
$rows = @($rows | Where-Object { $_.Effective.Applicable })

# Sharding, over the SAME final row set the sweep would otherwise process (post Filter/Exact/
# Applicable) -- roster order is the row order already established above, so "shard by the roster's
# own order" falls out of slicing this array as-is, no re-sort needed. Contiguous chunks, last shard
# absorbs any remainder from the ceiling division.
if ($ShardCount -gt 1) {
    $preShardTotal = $rows.Count
    $shardSize = [Math]::Ceiling($preShardTotal / $ShardCount)
    $startIdx = ($ShardIndex - 1) * $shardSize
    $endIdx = [Math]::Min($startIdx + $shardSize - 1, $preShardTotal - 1)
    $rows = if ($startIdx -gt $preShardTotal - 1) { @() } else { @($rows[$startIdx..$endIdx]) }
    Write-Host "  shard $ShardIndex/${ShardCount}: $($rows.Count) row(s) of $preShardTotal (roster order, size $shardSize)" -ForegroundColor Cyan
}

# Sweep-wide config override: an EXPLICITLY PASSED TestConfig or TestTiered means EVERY row runs
# under it, superseding any per-row `execution:` annotation for this run only (see the invocation
# site below).
#
# ⚠ This keys on whether the caller SPECIFIED the parameter, not on its VALUE, and that distinction
# became load-bearing when the default flipped to Release on 2026-09-02. The predicate used to read
# `($TestConfig -ne 'Debug') -or $TestTiered`, which was correct only while the default was Debug:
# carried forward past the flip it makes EVERY default run an override, and an override SUPERSEDES
# per-row `execution:` annotations -- so the three measured opt-out rows (internal/godebug,
# log/slog, net/http, all `release-tiered`) would silently run at TC0 and fail, and no bank would be
# eligible because every run would print the A/B warning. The bug would have looked like three
# regressions rather than one predicate.
#
# Keying on specification keeps both meanings intact and fails SAFE in the one ambiguous case: an
# explicit `-TestConfig Release` (same as the default) counts as an override, so it forces
# uniformity and marks the run non-bank-eligible. That can only ever cost a re-run; the opposite
# reading could silently bank a row under a config its own roster line does not declare, which is
# precisely what the override's own bank-eligibility rule exists to prevent.
$sweepConfigOverride = $PSBoundParameters.ContainsKey('TestConfig') -or
                       $PSBoundParameters.ContainsKey('TestTiered')
$sweepConfigLabel = if ($TestConfig -eq 'Release' -and $TestTiered) { 'Release (tiered)' } else { $TestConfig }

# Not computed on a hop run: every Effective.Expected is $null there, so the sum is meaningless, and
# summing nulls under $ErrorActionPreference='Stop' is a risk taken for a number the hop header does
# not print anyway.
$expectedTotal = if ($Hop) { 0 } else { ($rows | ForEach-Object { $_.Effective.Expected } | Measure-Object -Sum).Sum }
# test-config printed UNCONDITIONALLY, even at the Debug default -- the same reasoning the pipeline's
# own comparison-record field was given (not omitempty): a reader must never assume Debug by absence,
# and "no log can be read without knowing which configuration produced it" is the point of this row.
#
# On a hop run the expected total is 0 BY CONSTRUCTION, and printing "0 expected verdicts" beside a
# 227-row population would read like a roster that had lost its counts. The hop header says what the
# run is instead, including how many of its rows arrive carrying a predecessor.
if ($Hop) {
    $receivingRows = @($rows | Where-Object { $_.Receives })
    Write-Host ("HOP re-derivation: $($rows.Count) eligible package(s) from the census skeleton, " +
        "NO banked expectation in force, timeout $TestTimeout, test-config $sweepConfigLabel") -ForegroundColor Magenta
    Write-Host ('  every row records what it measures; nothing is compared against a floor. Everything ' +
        'that is not a count comparison still fails: build, host death, empty results, deadline, pin.') -ForegroundColor DarkGray
    if ($receivingRows.Count -gt 0) {
        Write-Host ("  $($receivingRows.Count) row(s) RECEIVE a relocated predecessor; its banked count travels " +
            'into the record as provenance and is never an expectation (the census: "no count is carried")') -ForegroundColor DarkGray
    }
}
else {
    Write-Host ("validated sweep: $($rows.Count) package(s), $expectedTotal expected verdicts, " +
        "timeout $TestTimeout, test-config $sweepConfigLabel") -ForegroundColor Cyan
}

# Announced only when the run actually carries one, so a sweep over default-path rows prints exactly
# what it always printed. The sweep-wide override is announced separately below because it is not a
# roster fact, and because it SUPERSEDES these per-row annotations rather than adding to them.
$executionRows = @($rows | Where-Object { $_.Execution })
if (-not $sweepConfigOverride -and $executionRows.Count -gt 0) {
    Write-Host ("  $($executionRows.Count) row(s) carry a per-row execution config: " +
        (($executionRows | ForEach-Object { "$($_.Package) [$($_.Execution)]" }) -join ', ')) -ForegroundColor Cyan
}
if ($sweepConfigOverride) {
    Write-Host ("  -TestConfig ${sweepConfigLabel}: EVERY row runs under it, annotated or not -- " +
        'an A/B measurement, not a bank-eligible sweep') -ForegroundColor Yellow
}

# Not printed on a hop run: every row's Source is 'hop', so this line would report "0 row(s) carry a
# <goos> expectation" and then promise comparison-validated-at-count, which is the OTHER reason for a
# missing expectation and not this one. The hop header above says which reason is in force.
if ($targetGoos -ne 'windows' -and -not $Hop) {
    $annotatedCount = @($rows | Where-Object { $_.Effective.Source -ne 'columns' }).Count
    Write-Host ("  target OS $targetGoos -- $annotatedCount row(s) carry a $targetGoos expectation; " +
        "$($rows.Count - $annotatedCount) fall back to the windows columns and report " +
        'comparison-validated-at-count when their count differs') -ForegroundColor Cyan
}

if (-not $SkipBuild) {
    Write-Host '==> building the converter' -ForegroundColor Cyan
    Push-Location $ConverterSrc
    try {
        # Absolute output path rather than the relative 'bin\go2cs.exe' this replaced: same file on
        # Windows, and it carries the host's own executable suffix instead of a hard-coded one.
        & go build -o $exe .
        if ($LASTEXITCODE -ne 0) { throw 'converter build failed' }
    }
    finally { Pop-Location }
}
if (-not (Test-Path $exe)) { throw "Converter not built: $exe" }

# ---- sweep --------------------------------------------------------------------------------------
# SERIAL by design: concurrent -tests runs share freshly-built dependency DLLs and collide on them
# (CS2012, "the process cannot access the file"), which reads as a package failure but is not one.
$env:MSBUILDDISABLENODEREUSE = '1'
$pass = 0; $fail = 0; $failed = @(); $started = Get-Date
# PER-ROW WALL TIMES, as a first-class machine-readable output rather than a log line to be scraped
# (coordinator ruling 2026-09-13, P4). The number itself is not new -- every verdict line has printed
# `[NNNs]` since 4e91a03e2 -- but a `[NNNs]` inside a coloured console line is only a cost proxy for
# whoever still has the log, and runbook §3.2 is explicit that this is unrecoverable afterward: "per-row
# log retention on the preceding consolidation sweep is a prerequisite of the next migration's shard
# map, and is unrecoverable afterward. Make it an obligation of that sweep, not of this step."
#
# Measured cost of not having done it: of the roster's rows only the 162 in the first fenced block of
# docs/phase4/DATA-sweep-row-walltimes.md carry a t_r at all, and the shard-map generator hard-asserts
# that 162 -- every row without a t_r is a row an LPT-greedy assignment cannot order. Written on EVERY
# run, not only hop runs, because the sweep that needs retaining is the ordinary consolidation sweep
# that precedes a hop, and a file only a hop writes would arrive one migration too late.
$rowTimings = New-Object System.Collections.Generic.List[object]
# The third bucket, and it exists only off Windows: a row whose comparison VALIDATED at a count this
# OS has no annotation for. Neither a pass (nothing is banked for it here) nor a silent failure --
# reported by name and summarized apart, the same shape BehavioralRunner's NOT MEASURED takes, and
# it still exits non-zero for the same reason: an unbanked count must never read as a green gate.
$cvac = 0; $cvacRows = @()
# ---- the -Hop buckets (coordinator ruling 2026-09-13) --------------------------------------------
# A DISTINCT word from CVAC with its OWN counter, ruled rather than shared, and the reason is that the
# two absences mean different things to whoever reads a mixed log or derives an annotation from it:
#
#   CVAC   no expectation for THIS OS       -- the roster HAS a row; this platform has no annotation
#   HOP    no expectation at THIS RELEASE   -- the corpus moved; nothing is banked for any platform
#
# Same shape, same retires-as-annotations-land semantics, and the totals line prints them SIDE BY SIDE
# rather than folded, so neither reason can be read as the other.
#
# ⚠ ONE DELIBERATE DIFFERENCE FROM CVAC, and it is the one place this cut departs from the ruling's
# literal wording -- flagged here, announced in the landing post, and trivially reversible by deleting
# the -not $Hop in the exit arm below. The ruling said "same non-failing semantics as CVAC", on my own
# report that CVAC was already non-failing. IT IS NOT: `if ($cvac) { exit 1 }` at the foot of this
# script, plus a red summary line, and its own comment states the reasoning -- "an unbanked count is
# not a green gate". That reasoning is exactly right for a GATE, which is what this script is when it
# sweeps the roster. A hop run is not a gate, it is a DERIVATION: every one of its rows is unbanked by
# construction, so inheriting CVAC's exit arm would make -Hop exit 1 unconditionally, and an exit code
# that cannot vary cannot tell a caller that the derivation broke. So $hop does NOT gate the exit,
# while $fail and $unstable still do -- honouring the ruling's INTENT (non-failing) rather than its
# wording (which describes a CVAC that does fail).
$hop = 0; $hopRows = @()
# The SECOND word the ruling requires, and the reason it is separate from $hop rather than a flavour of
# it: the incoming release's `n/a` annotations are DERIVED from this bucket, so "there are no eligible
# tests for this package here" must never be reachable by a row that merely failed to produce counts.
# It is decided by what the run PRODUCED -- the pipeline's own "No eligible Go tests for the requested
# target." line, cross-checked against the comparison record it writes beside it (status
# `not-applicable`) -- and a row whose two signals DISAGREE is a FAIL naming the disagreement, never a
# silent choice between them. Its sibling state, `infrastructure-blocked`, returns an ERROR from the
# pipeline and therefore stays a FAIL, which is the distinction that keeps a capability-blocked package
# out of this bucket.
$hopNoTests = 0; $hopNoTestsRows = @()
# Rows that passed as HOST-LIMITED: validated, and at a count smaller than the roster's because a
# committed `host-limit` disclosure accounts for a block this host provably cannot produce. They
# count as passes -- nothing regressed -- but a full sweep must not report their banked verdicts as
# re-validated when they were not, so they are named here as well as on their own verdict line.
$hostLimited = @()
$hostConditionalDisclosed = @()   # rows absorbed by the host-conditional-disclosure arm (Q31), summarized at the end
# The ORACLE-side three-run standard (coordinator ruling 2026-09-02; the rule is
# Test-OracleOnlyFailure in _roster.ps1). Two buckets, and they are opposites:
#   $oracleFlakedRows  rows whose FIRST run diverged only on the oracle side and whose SECOND run
#                      produced a verdict. Their verdict is run 2's -- $pass counts a clean one --
#                      and they are named here because a row that needed two runs must never read
#                      like a row that needed one.
#   $unstableRows      rows whose SECOND run was oracle-only as well. Their own verdict word,
#                      counted apart from $fail, and still a non-zero exit: two unreproducible
#                      oracle reds are a host to qualify, not a gate to pass.
$oracleFlakedRows = @()
$unstable = 0; $unstableRows = @()
# Where a failed row's evidence is PRESERVED before the re-run overwrites it (CLAUDE.md: a gate
# preserves a failed row's comparison record BEFORE any restore or cleanup -- a union battery once
# deleted the only evidence of which rows diverged). Under the repo's gitignored scratch root, so
# the preserved copies can never read as corpus dirt; stamped with the SWEEP's start so one run's
# rows land together.
$oracleEvidenceRoot = Join-Path $repo ('scratchpad/sweep-oracle-flake/' + $started.ToString('yyyyMMdd-HHmmss'))

# 'Continue' for the sweep itself: merging a native command's stderr with 2>&1 wraps each line in
# an ErrorRecord in PS 5.1, so under 'Stop' one benign converter warning (e.g. the unsafe.Sizeof
# notice from crypto/subtle) aborts the whole run. The verdict is judged from the output below,
# not from $?, so a non-terminating preference is the correct setting here.
$ErrorActionPreference = 'Continue'

# Go duration text -> TimeSpan, so two -test-timeout values can be COMPARED rather than merely
# swapped for one another. Returns $null for anything it cannot read -- the empty string, a bare
# number with no unit, an unknown unit -- all of which the converter would reject too; every caller
# treats $null as "the comparison cannot be made, keep the safer value".
function ConvertTo-GoDuration([string] $value) {
    if ([string]::IsNullOrWhiteSpace($value)) { return $null }

    $text = $value.Trim()
    $sign = 1
    if ($text.StartsWith('-')) { $sign = -1; $text = $text.Substring(1) }
    elseif ($text.StartsWith('+')) { $text = $text.Substring(1) }
    if ($text -eq '0') { return [TimeSpan]::Zero }

    # TICKS per unit, not milliseconds: [TimeSpan]::FromMilliseconds rounds to the nearest whole
    # millisecond on .NET Framework (PS 5.1), which silently flattens every sub-second value to zero.
    # Those units are here for completeness -- Go accepts them, so a value naming one must not be
    # misread as unparseable and demoted to the floor. The two micro signs Go accepts are spelled by
    # code point so this file needs no non-ASCII literal.
    $perUnit = @{
        'ns' = 0.01; 'us' = 10.0
        'ms' = 10000.0; 's' = 1e7; 'm' = 6e8; 'h' = 3.6e10
    }

    # The micro signs join OUTSIDE the literal, each behind a ContainsKey guard: hashtable keys
    # fold case-insensitively per the host's casing tables, and U+00B5 (micro sign) vs U+03BC
    # (Greek mu) are DISTINCT under Windows PowerShell's NLS but EQUAL under pwsh/ICU on Linux --
    # a literal carrying both dies there at evaluation with "Duplicate keys", killing the sweep
    # of any package whose deadline reaches this parser (measured 2026-08-21: crypto/tls on the
    # Linux lane). The guard admits whichever spellings the host's comparer keeps distinct;
    # lookups behave identically either way.
    foreach ($microUnit in @("$([char]0xB5)s", "$([char]0x3BC)s")) {
        if (-not $perUnit.ContainsKey($microUnit)) { $perUnit[$microUnit] = 10.0 }
    }

    $ticks = 0.0
    $consumed = 0

    foreach ($part in [regex]::Matches($text, '(\d+(?:\.\d*)?|\.\d+)([^\d.]+)')) {
        # Parts must tile the string end to end: a gap means it holds something that is not a
        # <number><unit> pair, which is a value this script must not pretend to understand.
        if ($part.Index -ne $consumed) { return $null }

        $unit = $part.Groups[2].Value
        if (-not $perUnit.ContainsKey($unit)) { return $null }

        # InvariantCulture explicitly: a fractional duration ('1.5h') must not read as 15 under a
        # comma-decimal culture.
        $ticks += [double]::Parse($part.Groups[1].Value, [Globalization.CultureInfo]::InvariantCulture) * $perUnit[$unit]
        $consumed = $part.Index + $part.Length
    }

    # Also catches "no parts matched at all", since $text is non-empty by here.
    if ($consumed -ne $text.Length) { return $null }
    # A duration too large for TimeSpan is unreadable rather than fatal -- Go rejects one too, and
    # $null routes it to the caller's safe branch instead of throwing mid-sweep.
    if ($ticks -gt [double][long]::MaxValue) { return $null }

    return [TimeSpan]::FromTicks([long][math]::Round($sign * $ticks))
}

# ---- host-conditional verdicts ------------------------------------------------------------------
# A banked count is normally exact, but a verdict COUNT can be legitimately host-dependent:
# path/filepath banks 61 on a host without symlink-creation privilege, where Go itself skips
# TestWalkSymlinkRoot -- and on a host WITH the privilege the test runs and spawns its six
# table-driven subtests, six verdict rows that simply do not exist on the banking host (the
# skip->pass flips of the other privilege-gated tests are count-NEUTRAL; only rows that appear or
# vanish move the count). Banking the larger number would false-red every unprivileged host, the
# larger population -- so the roster banks the FLOOR and names the conditional verdicts, and the
# sweep accepts floor+k ONLY when the k extra rows are exactly k of the named tests, agreeing on
# both runtimes, with no banked verdict missing. Anything outside the named set still fails
# loudly, exactly as before.
#
# The check needs the banked verdict NAME SET, not just the count -- count arithmetic alone would
# wave through a lost banked row canceling against a rogue new one. That set is the package's
# committed proof page (docs/validation/current/<pkg-dots>.md), read from HEAD deliberately: the
# sweep run that just validated at the larger count has already REWRITTEN the working-tree page to
# match itself, so only the committed copy still records what was banked.
function Test-HostConditionalDelta {
    param(
        [int] $Expected,           # banked matching-verdict count (roster column 2, the floor)
        [int] $Disclosed,          # banked disclosed count (roster column 3)
        [string[]] $Conditional,   # the named host-conditional verdicts (roster annotation)
        [int] $Got,                # the live run's validated count
        $Comparison,               # the run's go2cs_test_comparison.json via ConvertFrom-ComparisonRecord:
                                   # go/csharp are ORDINAL dictionaries (never PSObjects -- a PSObject
                                   # cannot hold the legal case-only verdict-name pairs; see _roster.ps1)
        [string[]] $BankedNames    # verdict names from the committed proof page's Verdicts table
    )

    # Local to this function -- nested definitions do not leak into the script scope.
    function New-HostConditionalVerdictResult([bool] $accepted, [string[]] $extras, [string] $reason) {
        return [PSCustomObject]@{ Accepted = $accepted; Extras = $extras; Reason = $reason }
    }

    $k = $Got - $Expected
    if ($k -lt 1) {
        return New-HostConditionalVerdictResult $false @() "count $Got is below the banked floor $Expected -- a lost verdict is never host-conditional"
    }
    if ($k -gt $Conditional.Count) {
        return New-HostConditionalVerdictResult $false @() "count $Got exceeds the floor by $k, more than the $($Conditional.Count) named host-conditional verdicts"
    }
    if ($null -eq $Comparison -or $null -eq $Comparison.go -or $null -eq $Comparison.csharp) {
        return New-HostConditionalVerdictResult $false @() 'comparison record carries no per-test verdict maps'
    }

    # A moved disclosed count shifts the same arithmetic this check relies on, and a disclosure
    # appearing or retiring is roster maintenance, never host capability.
    $liveDisclosed = if ($null -eq $Comparison.disclosed) { 0 } else { @($Comparison.disclosed).Count }
    if ($liveDisclosed -ne $Disclosed) {
        return New-HostConditionalVerdictResult $false @() "disclosed count moved ($liveDisclosed live vs $Disclosed banked) -- not a host-conditional shape"
    }

    # The proof page lists every compared verdict: the matched rows plus the disclosed-divergent
    # ones. If page and roster disagree the banked evidence is inconsistent -- absorb nothing.
    if ($BankedNames.Count -ne ($Expected + $Disclosed)) {
        return New-HostConditionalVerdictResult $false @() "committed proof page lists $($BankedNames.Count) verdicts where the roster banks $Expected matched + $Disclosed disclosed -- page and table disagree"
    }

    $goMap = $Comparison.go
    $csMap = $Comparison.csharp
    $liveNames = @($goMap.Keys)

    $missing = @($BankedNames | Where-Object { $liveNames -notcontains $_ })
    if ($missing.Count -gt 0) {
        return New-HostConditionalVerdictResult $false @() "banked verdicts missing from this run: $($missing -join ', ')"
    }

    $extras = @($liveNames | Where-Object { $BankedNames -notcontains $_ })
    $outside = @($extras | Where-Object { $Conditional -notcontains $_ })
    if ($outside.Count -gt 0) {
        return New-HostConditionalVerdictResult $false @() "extra verdicts outside the named host-conditional set: $($outside -join ', ')"
    }

    # With nothing missing this equals $k by construction; a mismatch means the count and the
    # verdict maps have come apart (a converter-side accounting change) -- refuse to absorb.
    if ($extras.Count -ne $k) {
        return New-HostConditionalVerdictResult $false @() "the $($extras.Count) extra verdict rows do not account for the count delta of $k"
    }

    foreach ($name in $extras) {
        $goVerdict = $goMap[$name]
        if (-not $csMap.ContainsKey($name) -or $csMap[$name] -ne $goVerdict) {
            $csVerdict = if (-not $csMap.ContainsKey($name)) { 'absent' } else { $csMap[$name] }
            return New-HostConditionalVerdictResult $false @() "conditional verdict ${name}: go '$goVerdict' vs C# '$csVerdict' -- the sides do not agree"
        }
    }

    return New-HostConditionalVerdictResult $true $extras $null
}

# ---- per-OS expectations -------------------------------------------------------------------------
# The disclosed count this run produced. The converter's "Validated N tests against go test" line
# carries it -- "(..., K disclosed-divergent (class), ...)" -- and omits the clause entirely when K
# is zero, exactly as the roster's Disclosed column is blank then. Read from the same output the
# matching count is read from, so the two numbers can never come from different runs.
function Get-DisclosedCount {
    param($Output)

    $match = ($Output | Select-String '(\d+) disclosed-divergent' | Select-Object -First 1)
    if ($match) { return [int]$match.Matches[0].Groups[1].Value }

    return 0
}

# THE SECOND HOP WORD's predicate: did this row run and find NO ELIGIBLE TESTS, as opposed to failing
# to produce counts? Only a positive answer from the INSTRUMENT counts, never a reading of a log tail,
# because the incoming release's `n/a` annotations are derived from this answer and a broken row
# annotating itself platform-exclusive would poison that derivation at its source.
#
# TWO required signals, both written by the pipeline on the same success path
# (testConversion.go: !manifestHasEligibleTests -> write the record, print the line, return nil):
#   * the line "No eligible Go tests for the requested target." on stdout
#   * a comparison record whose status is `not-applicable`
#
# ⚠ Their DISAGREEMENT is a refusal, not a coin toss, and it is reported rather than swallowed: one
# signal without the other means either a stale record from a previous run of this package or output
# that did not come from the path that writes it, and both are reasons to let the row fail loudly. The
# sibling state this predicate must never admit is `infrastructure-blocked`, which the pipeline returns
# as an ERROR with a record whose status differs -- so a capability-blocked package fails, by the
# instrument's own distinction and not by this script's judgement.
function Test-HopRowHasNoEligibleTests {
    param($Output, [string] $OutDir)

    $saidSo = @($Output | Select-String -SimpleMatch 'No eligible Go tests for the requested target.').Count -gt 0

    $recordSaysSo = $false
    $recordStatus = '<no record>'
    $comparisonPath = Join-Path $OutDir 'go2cs_test_comparison.json'

    if (Test-Path $comparisonPath) {
        try {
            $record = Get-Content -LiteralPath $comparisonPath -Raw | ConvertFrom-Json
            if ($record.PSObject.Properties['status']) { $recordStatus = [string]$record.status }
            $recordSaysSo = ($recordStatus -eq 'not-applicable')
        }
        catch {
            # An unreadable record is not a no-eligible-tests answer. Say which, and refuse.
            $recordStatus = "<unreadable: $($_.Exception.Message)>"
        }
    }

    if ($saidSo -and $recordSaysSo) { return $true }

    if ($saidSo -or $recordSaysSo) {
        Write-Host ('        no-eligible-tests check REFUSED -- the two signals disagree: output ' +
            "said-so=$saidSo, comparison record status='$recordStatus'. One without the other is a " +
            'stale or foreign record, so this row fails rather than deriving an n/a annotation.') -ForegroundColor DarkYellow
    }

    return $false
}

# Reads the two evidence artifacts the delta check needs -- the run's own comparison record and
# the committed proof page -- and applies Test-HostConditionalDelta. Every unreadable input is a
# rejection with its reason, never an acceptance: the mechanism only absorbs what it can prove.
function Get-HostConditionalVerdict {
    param([PSCustomObject] $Row, [int] $Got, [string] $OutDir)

    $comparisonPath = Join-Path $OutDir 'go2cs_test_comparison.json'
    if (-not (Test-Path $comparisonPath)) {
        return [PSCustomObject]@{ Accepted = $false; Extras = @(); Reason = "no comparison record at $comparisonPath" }
    }

    $comparison = $null
    $comparisonError = $null
    try { $comparison = ConvertFrom-ComparisonRecord -Path $comparisonPath } catch { $comparisonError = $_.Exception.Message }
    if ($null -eq $comparison) {
        return [PSCustomObject]@{ Accepted = $false; Extras = @(); Reason = "unreadable comparison record at ${comparisonPath}: $comparisonError" }
    }

    # 2>$null is safe here: the sweep loop runs under 'Continue', so a missing page's stderr lines
    # become discarded ErrorRecords rather than a terminating abort (the 'Stop' hazard the drift
    # section documents), and the rejection below already names the path loudly.
    $pageRel = 'docs/validation/current/' + ($Row.Package -replace '/', '.') + '.md'
    $pageLines = & git -C $repo show "HEAD:$pageRel" 2>$null
    if ($LASTEXITCODE -ne 0 -or -not $pageLines) {
        return [PSCustomObject]@{ Accepted = $false; Extras = @(); Reason = "no committed proof page at HEAD:$pageRel" }
    }

    # Verdict names come from the page's "## Verdicts" table alone -- a disclosed package repeats
    # its divergent tests' names in a later Disclosed-divergences table, which must not be counted.
    $bankedNames = New-Object System.Collections.Generic.List[string]
    $inVerdicts = $false
    foreach ($pageLine in @($pageLines)) {
        if ($pageLine -match '^##\s') { $inVerdicts = [bool]($pageLine -match '^##\s+Verdicts\b'); continue }
        if ($inVerdicts -and $pageLine -match '^\|\s*`([^`]+)`\s*\|') { [void]$bankedNames.Add($Matches[1]) }
    }

    # The floor is the expectation IN FORCE for this run's OS, not unconditionally the columns.
    # Note the evidence this absorption rests on is still Windows-shaped: the committed proof page
    # records the banking host's verdict names, so on an OS-annotated row the page-vs-roster
    # cross-check rejects rather than absorbs -- honestly, and by design. Proof pages gain the OS
    # dimension at the anchor release, per the per-OS ruling; until then a non-Windows host
    # exceeding an annotation is reported, never waved through.
    return Test-HostConditionalDelta -Expected $Row.Effective.Expected -Disclosed $Row.Effective.Disclosed -Conditional $Row.Conditional `
        -Got $Got -Comparison $comparison -BankedNames $bankedNames.ToArray()
}

# The FOURTH absorption's reader (Q31): a row naming host-conditional DISCLOSURES -- entries whose
# firing is a property of the host, so the same verdict sits in the matched column on one host and
# the disclosed column on another (os/exec's TestExtraFiles) -- gets one chance to prove that its
# moved disclosed count is exactly those entries firing here. The rule is Test-HostConditionalDisclosureDelta
# in _roster.ps1; this reads the run's own comparison record and hands it over. NO proof page: the
# evidence for a verdict changing column is the live record alone (the rule's comment says why the
# surplus arm's page cross-check must not be inherited). Every unreadable input is a rejection with
# its reason, never an acceptance.
function Get-HostConditionalDisclosureVerdict {
    param([PSCustomObject] $Row, [int] $Got, [int] $GotDisclosed, [string] $OutDir)

    $comparisonPath = Join-Path $OutDir 'go2cs_test_comparison.json'
    if (-not (Test-Path $comparisonPath)) {
        return [PSCustomObject]@{ Accepted = $false; Fired = @(); Reason = "no comparison record at $comparisonPath" }
    }

    $comparison = $null
    $comparisonError = $null
    try { $comparison = ConvertFrom-ComparisonRecord -Path $comparisonPath } catch { $comparisonError = $_.Exception.Message }
    if ($null -eq $comparison) {
        return [PSCustomObject]@{ Accepted = $false; Fired = @(); Reason = "unreadable comparison record at ${comparisonPath}: $comparisonError" }
    }

    return Test-HostConditionalDisclosureDelta -Expected $Row.Effective.Expected -Disclosed $Row.Effective.Disclosed `
        -Names $Row.ConditionalDisclosures -Got $Got -GotDisclosed $GotDisclosed -Comparison $comparison
}

# ---- capability-conditional verdicts (the MIRROR of the surplus mechanism above) -----------------
# The mechanism above assumes the roster banks a FLOOR and a more-capable host produces EXTRA
# verdicts. Some capability-bound test blocks run the opposite way: the roster banks the CEILING --
# every case the capability enables -- and a host lacking the prerequisite never spawns the case
# matrix at all, so Go's own top-level test collapses to ONE verdict. crypto/tls's TestBogoSuite is
# the first of these (the BoGo/BoringSSL shim runner): 3,243 sub-verdicts -- 1 parent + 861 pass +
# 2,381 skip -- collapse to exactly one, both runtimes agreeing, because Go's own oracle collapses
# identically absent the runner. That collapsed verdict is a FAIL, not a skip, and the disclosed
# count moves with it: see the measured note over Test-CapabilityAbsentDelta in _roster.ps1, which
# is where the rule and its evidence live. "A lost verdict is never host-conditional" above stays
# true for every OTHER shortfall: this path engages ONLY for a package registered here, and ONLY
# when the shortfall matches that package's registered block size exactly -- anything else still
# falls through to the same hard failure as before. In particular a host that HAS the capability but
# whose converted side misses the runner's own deadline produces the identical shortfall with Go
# PASSING, and this rule refuses it -- that shortfall is the converted side's, and it is absorbed
# only where the package's own COMMITTED manifest already discloses it: the THIRD host state, owned
# by Test-HostLimitDelta and Get-HostLimitVerdict below.
#
# Registered by package; BlockSize is the full-capability verdict count for Test (the top-level test
# itself plus every Go subtest under it) -- re-derive it from the committed proof page rather than
# trust this number cold if the suite's own case matrix ever changes. ONE registration serves BOTH
# shortfall rules on purpose: no package can reach either absorption without being named here.
$capabilityConditionalBlocks = @{
    'crypto/tls' = @{ Test = 'TestBogoSuite'; BlockSize = 3243 }
}

# Test-CapabilityAbsentDelta -- the pure decision rule -- lives in _roster.ps1 beside
# Get-SweepRowClassification, so check-roster-format.ps1 can fixture-test it the same way; this
# file only reads the evidence and calls it, mirroring Get-HostConditionalVerdict above.
#
# Reads the same two evidence artifacts Get-HostConditionalVerdict does and applies
# Test-CapabilityAbsentDelta instead. Every unreadable input is a rejection, never an acceptance.
function Get-CapabilityAbsentVerdict {
    param([PSCustomObject] $Row, [int] $Got, [string] $OutDir, [PSCustomObject] $Block)

    $comparisonPath = Join-Path $OutDir 'go2cs_test_comparison.json'
    if (-not (Test-Path $comparisonPath)) {
        return [PSCustomObject]@{ Accepted = $false; Reason = "no comparison record at $comparisonPath" }
    }
    $comparison = $null
    $comparisonError = $null
    try { $comparison = ConvertFrom-ComparisonRecord -Path $comparisonPath } catch { $comparisonError = $_.Exception.Message }
    if ($null -eq $comparison) {
        return [PSCustomObject]@{ Accepted = $false; Reason = "unreadable comparison record at ${comparisonPath}: $comparisonError" }
    }

    $pageRel = 'docs/validation/current/' + ($Row.Package -replace '/', '.') + '.md'
    $pageLines = & git -C $repo show "HEAD:$pageRel" 2>$null
    if ($LASTEXITCODE -ne 0 -or -not $pageLines) {
        return [PSCustomObject]@{ Accepted = $false; Reason = "no committed proof page at HEAD:$pageRel" }
    }

    $bankedNames = New-Object System.Collections.Generic.List[string]
    $inVerdicts = $false
    foreach ($pageLine in @($pageLines)) {
        if ($pageLine -match '^##\s') { $inVerdicts = [bool]($pageLine -match '^##\s+Verdicts\b'); continue }
        if ($inVerdicts -and $pageLine -match '^\|\s*`([^`]+)`\s*\|') { [void]$bankedNames.Add($Matches[1]) }
    }

    return Test-CapabilityAbsentDelta -Expected $Row.Effective.Expected -Disclosed $Row.Effective.Disclosed -Block $Block `
        -Got $Got -Comparison $comparison -BankedNames $bankedNames.ToArray()
}

# ---- host-limited verdicts (the THIRD host state) ------------------------------------------------
# Same registered block, same two evidence artifacts, plus a third that only this rule reads: the
# package's own COMMITTED disclosure manifest. The rule and its reasoning live in _roster.ps1
# (Test-HostLimitDelta); this reads the evidence and calls it, mirroring the two readers above.
#
# Reached ONLY when Get-CapabilityAbsentVerdict has already declined, so the two shortfall shapes
# can never both answer for one run -- and they are mutually exclusive on the evidence anyway (that
# rule needs an agreeing non-pass block root, this one needs a PASSING Go root).
function Get-HostLimitVerdict {
    param([PSCustomObject] $Row, [int] $Got, [string] $OutDir, [PSCustomObject] $Block)

    $comparisonPath = Join-Path $OutDir 'go2cs_test_comparison.json'
    if (-not (Test-Path $comparisonPath)) {
        return [PSCustomObject]@{ Accepted = $false; Reason = "no comparison record at $comparisonPath" }
    }
    $comparison = $null
    $comparisonError = $null
    try { $comparison = ConvertFrom-ComparisonRecord -Path $comparisonPath } catch { $comparisonError = $_.Exception.Message }
    if ($null -eq $comparison) {
        return [PSCustomObject]@{ Accepted = $false; Reason = "unreadable comparison record at ${comparisonPath}: $comparisonError" }
    }

    $pageRel = 'docs/validation/current/' + ($Row.Package -replace '/', '.') + '.md'
    $pageLines = & git -C $repo show "HEAD:$pageRel" 2>$null
    if ($LASTEXITCODE -ne 0 -or -not $pageLines) {
        return [PSCustomObject]@{ Accepted = $false; Reason = "no committed proof page at HEAD:$pageRel" }
    }

    $bankedNames = New-Object System.Collections.Generic.List[string]
    $inVerdicts = $false
    foreach ($pageLine in @($pageLines)) {
        if ($pageLine -match '^##\s') { $inVerdicts = [bool]($pageLine -match '^##\s+Verdicts\b'); continue }
        if ($inVerdicts -and $pageLine -match '^\|\s*`([^`]+)`\s*\|') { [void]$bankedNames.Add($Matches[1]) }
    }

    # The third artifact, and the absorption's admission gate. TWO properties are needed and they
    # are not the same one: the manifest must be the file the CONVERTER read (it is the run's own
    # input -- the disclosure in the record above exists because of it), and it must be COMMITTED,
    # so no uncommitted edit can mint an absorption out of a scratch file. Hence the on-disk read
    # for the first and the two git assertions for the second.
    #
    # ReadAllText rather than `git show`: PS 5.1 decodes a native command's bytes through the
    # console codepage and these manifests carry UTF-8 prose. Only ASCII fields are consumed here,
    # but reading the file correctly is cheaper than reasoning about which bytes survive.
    $manifestPath = Join-Path $OutDir 'go2cs_test_disclosures.json'
    $manifestRel = 'src/core/' + $Row.Package + '/go2cs_test_disclosures.json'
    if (-not (Test-Path $manifestPath)) {
        return [PSCustomObject]@{ Accepted = $false; Reason = "no disclosure manifest at $manifestPath -- nothing pins $($Block.Test)" }
    }
    if (-not (& git -C $repo ls-files -- $manifestRel)) {
        return [PSCustomObject]@{ Accepted = $false; Reason = "the disclosure manifest at $manifestRel is untracked -- only a COMMITTED pin can admit a shortfall" }
    }
    & git -C $repo -c core.safecrlf=false diff --quiet --ignore-cr-at-eol HEAD -- $manifestRel
    if ($LASTEXITCODE -ne 0) {
        return [PSCustomObject]@{ Accepted = $false; Reason = "the disclosure manifest at $manifestRel differs from HEAD -- only a COMMITTED pin can admit a shortfall" }
    }

    # An absent ENTRY is not an error here: it is left to the rule, which refuses a null pin by name
    # so the fixtures can exercise that refusal without a manifest on disk.
    $pin = $null
    try {
        $manifest = [System.IO.File]::ReadAllText($manifestPath) | ConvertFrom-Json
        $entry = @($manifest.disclosures | Where-Object { $_.name -eq $Block.Test })
        if ($entry.Count -eq 1) { $pin = [PSCustomObject]@{ Class = $entry[0].class; Signature = $entry[0].signature } }
    }
    catch {
        return [PSCustomObject]@{ Accepted = $false; Reason = "unreadable disclosure manifest at ${manifestPath}: $($_.Exception.Message)" }
    }

    return Test-HostLimitDelta -Expected $Row.Effective.Expected -Disclosed $Row.Effective.Disclosed -Block $Block `
        -Got $Got -Comparison $comparison -BankedNames $bankedNames.ToArray() -Pin $pin
}

# ---- the ORACLE-ONLY failure (the three-run standard, applied to the ORACLE side) ----------------
# The three rules above all answer "this row VALIDATED at the wrong count -- absorb the difference?".
# This one is reached one step earlier, on a row that produced no "Validated N" line at all, and it
# absorbs NOTHING: it decides only whether the sweep may ask a second time. The rule and its whole
# refusal ladder live in _roster.ps1 (Test-OracleOnlyFailure); this reads the evidence and calls it,
# mirroring the three readers above.
#
# TWO artifacts, and both must be THIS ATTEMPT'S OWN. That is the one thing this reader adds over
# its siblings, and it is not decoration: the pipeline's records are gitignored, `git clean -fd`
# skips ignored paths, and a restore cannot clear them -- so a warm tree carries the PREVIOUS run's
# comparison record and results file, byte-identical in shape to this run's (CLAUDE.md's three
# record-file rules, and the FAILED-BUILD case that re-reads an old record and reports old
# failures). A staleness test against the attempt's own start time is exact, costs one file stat,
# and is the difference between reading this run and reading the last one.
function Get-OracleOnlyVerdict {
    param([string] $OutDir, [datetime] $Since)

    function New-OracleReaderRejection([string] $reason) {
        return [PSCustomObject]@{ OracleOnly = $false; Flaked = @(); Reason = $reason }
    }

    $comparisonPath = Join-Path $OutDir 'go2cs_test_comparison.json'
    $resultsPath = Join-Path $OutDir 'go2cs_test_results.json'

    if (-not (Test-Path $comparisonPath)) {
        return New-OracleReaderRejection "no comparison record at $comparisonPath"
    }
    if ((Get-Item -LiteralPath $comparisonPath).LastWriteTime -lt $Since) {
        return New-OracleReaderRejection ("the comparison record at $comparisonPath predates this attempt " +
            "(started $Since) -- a warm tree's record from an earlier run is not this run's evidence")
    }
    if (-not (Test-Path $resultsPath)) {
        return New-OracleReaderRejection ("no converted-host results file at $resultsPath -- the tail rule has " +
            'nothing to read, so the deadline/crash question is answered NO')
    }
    if ((Get-Item -LiteralPath $resultsPath).LastWriteTime -lt $Since) {
        return New-OracleReaderRejection ("the results file at $resultsPath predates this attempt (started $Since) -- " +
            'a stale tail cannot answer the deadline/crash question for this run')
    }

    $comparison = $null
    $comparisonError = $null
    try { $comparison = ConvertFrom-ComparisonRecord -Path $comparisonPath } catch { $comparisonError = $_.Exception.Message }
    if ($null -eq $comparison) {
        return New-OracleReaderRejection "unreadable comparison record at ${comparisonPath}: $comparisonError"
    }

    $tail = $null
    try { $tail = Get-ResultsTailText -Path $resultsPath } catch { return New-OracleReaderRejection "unreadable results tail at ${resultsPath}: $($_.Exception.Message)" }

    return Test-OracleOnlyFailure -Comparison $comparison -TailText $tail
}

# Copies one attempt's record, results and JUnit files aside BEFORE the next attempt overwrites
# them. Missing files are skipped rather than reported: the caller has already established which
# artifacts exist, and a preserve step must never be the thing that fails a row.
function Save-OracleEvidence {
    param([string] $Package, [string] $OutDir, [int] $Attempt)

    $dest = Join-Path $oracleEvidenceRoot ('{0}/run{1}' -f ($Package -replace '/', '.'), $Attempt)
    [void](New-Item -ItemType Directory -Force -Path $dest)

    foreach ($name in @('go2cs_test_comparison.json', 'go2cs_test_results.json', 'go2cs_test_results.xml')) {
        $file = Join-Path $OutDir $name
        if (Test-Path $file) { Copy-Item -LiteralPath $file -Destination (Join-Path $dest $name) -Force }
    }

    return $dest
}

# ONE invocation path, called by the first attempt and by the oracle re-run alike. It is a function
# rather than two call sites for the reason the silent-duplication rule states: a re-run whose
# command line has drifted from the run it is repeating is not a re-run, and two independently
# maintained copies of an argument list is exactly the shape that drifts without a conflict.
# $exe and $src come from script scope, as they do for the three evidence readers above.
function Invoke-SweepRow {
    param(
        [Parameter(Mandatory)][string] $Package,
        [Parameter(Mandatory)][string] $GoDir,
        [Parameter(Mandatory)][string] $OutDir,
        [Parameter(Mandatory)][string] $PkgTimeout,
        [string[]] $ExecArgs = @()
    )

    # The cgo state is pinned ONCE for the whole run (see the pin above the row loop); the per-row
    # -CgoOff switch this function carried until 2026-09-03 retired with the per-package table.
    return & $exe -tests -test-action all -test-timeout $PkgTimeout @ExecArgs -go2cspath $src $GoDir $OutDir 2>&1
}

# Packages whose C# suite legitimately exceeds the default package deadline. hash/maphash's
# SMHasher matrix runs ~15 minutes in C# (7.6 s in Go — a performance gap, not a correctness one)
# and was BANKED under 30m; at the default it reports a timeout with every test up to the cut
# PASSING, which reads as a failure and costs an investigation every time. index/suffixarray is
# the same shape and worse: `TestNew{32,64}/exhaustive3` brute-forces every string up to length 8
# over a 3-letter alphabet, 12.4 s in Go and ~35 min in C#; under 10m it reports exactly the
# TestNew64/exhaustive3 tail as empty verdicts.
# crypto/dsa is the third member and the one that cost the most to misread: TestParameterGeneration
# is a probabilistic prime search over the converted math/big and takes 1,156.8 s (19.3 min) in C#
# against seconds in Go. The board recorded it as "no -test-timeout is enough" after measuring at
# 6m and again at 20m -- but 20m is just UNDER what the package needs end to end, so the deadline
# cut a run that was converging. It validates 4/4 at 30m.
#
# archive/zip is the mildest of the four and the one whose number MOVED. TestZip64LargeDirectory
# builds a 4 GiB central directory out of ~128 KB records; before r57c it never completed at all
# (>45 m) because @string's range indexer copied, making the rune walk over each 65,535-byte name
# quadratic. With @string carrying a real window the whole suite runs 20 s in Release against Go's
# 11.3 s -- honest. The deadline is here for the harness's DEBUG build, which the pipeline uses and
# which pays ~22x for non-inlined golib accessors: 391 s measured solo on the reference desktop,
# 774 s on an i7-5820K -- which leaves r57c's original 20m only ~35% headroom on slow hardware, so
# the entry is 30m: a deadline is a safety net against a hung run, never a performance assumption.
#
# PRECEDENCE -- the table is a FLOOR, not an override (2026-08-10). The effective budget is the
# LONGER of the entry and an explicitly-passed -TestTimeout, so a larger -TestTimeout raises these
# four like it raises everything else. It used to win unconditionally, which meant -TestTimeout was
# silently ignored for exactly the packages that need a long budget: an i7-5820K desktop reported
# hash/maphash and crypto/dsa as FAIL "package timeout after 00:30:00", and `-TestTimeout 60m` died
# at 30:00 again -- while driving the same package's pipeline by hand at 60m validated its banked
# 22/22. A SMALLER -TestTimeout still loses to the table: under-budgeting these four is the exact
# false red the table exists to prevent, and the usage line above advertises the flag for slow boxes.
# The values are SLOW-HOST-CALIBRATED per the safety-net doctrine (a deadline is a net against a
# hung run, never a performance assumption): a floor sized to the fastest box false-reds every bare
# sweep on a slower one, while a raised floor costs a fast box only how long a RARE genuine hang
# takes to be declared. Calibration evidence, i7-5820K 2026-08-10: maphash validated 22/22 in
# **2,406 s (40.1 min)** -- past the old i9-sized 30m floor, hence 60m; crypto/dsa's banked
# TestParameterGeneration was 1,156.8 s (19.3 min) on the i9, extrapolating to ~40-60 min here,
# hence 60m; index/suffixarray's ~35 min on the i9 extrapolates to 70-105 min, hence 120m;
# archive/zip measured 774 s here, so its 30m stands. -TestTimeout raises any of these further on
# a still-slower box; re-measure and move a floor when a slower legitimate host proves one short.
#
# crypto/dsa's floor is sized to its VARIANCE, not its typical run: TestParameterGeneration is a
# randomized prime search, and two same-machine samples an hour apart measured 1,496 s (25 min)
# and >3,600 s (blew the then-60m floor mid-run) -- the heavy tail, not load, is the variable, so
# the deadline must clear the tail. 120m holds both observed extremes with 2x headroom on the
# fast sample (2026-08-11, i7-5820K).
#
# go/parser's floor covers its deliberate deep-recursion suite (TestParseDepthLimit walks
# hundreds of thousands of converted frames per variant): 17 min measured solo on SSD
# (2026-08-14, i7-5820K), and the 10m default reported a passing suite as NOT MEASURED the
# first time the full sweep ran it under load. 40m clears the solo figure with 2x headroom.
#
# crypto/internal/mlkem768's floor is owed by ONE test: TestPQCrystalsAccumulated runs 10,000 full
# ML-KEM key-gen/encapsulate/decapsulate rounds and accumulates them into a SHAKE-128 digest, and
# measured 417.3 s of the package's 434.7 s total (2026-08-16, i7-5820K, solo). That clears the 10m
# default by only 1.4x -- inside the run-to-run spread a loaded sweep produces -- so the package
# would report NOT MEASURED on a bad day without a floor. 30m gives the measured figure 4x headroom.
#
# crypto/tls's floor is owed by ONE row that always spends its budget: the disclosed (host-limit)
# TestBogoSuite burns exactly 600 s — its child BoGo runner's own `go test` deadline, which fires
# before the converted shims can finish the 5,481-case matrix — before failing with the pinned
# signature, and the other 183 tests measure ~45 s around it. The whole C# suite measured 644.8 s
# solo (2026-08-18, laptop R, Ryzen 7 PRO 6850U), which the 10m default kills mid-BoGo and reports
# as NOT MEASURED. 30m clears the measured figure with ~2.8x headroom for a loaded sweep.
#
# archive/zip and go/parser carry FULL-SWEEP-LOAD multipliers their earlier floors did not: on the
# 151-row sweep of 2026-08-19 (laptop R, 340 min) zip blew 30m and parser blew 40m — both as
# one-sided-row truncations reading like divergences — while the SAME DAY, SOLO on the same
# machine, zip measured 850 s and parser 836 s (~14 min each, comfortably inside those floors).
# The sweep's accumulated disk/cache pressure is a ~2.5-3x multiplier on exactly the two suites
# that hammer storage (zip streams 4 GiB; parser walks hundreds of thousands of converted frames),
# so their floors are sized to the LOADED case: 60m and 90m clear the observed loaded shortfalls
# with ~2x headroom.
# sync/atomic's floor rose 60m -> 90m on 2026-09-03 (coordinator ruling, mailbox 82ec6654c): the
# Linux leveling re-sweep read the row NOT MEASURED at 60m on the G-LAPTOP WSL host -- the results
# tail stating `package timeout after 01:00:00` and the comparison truncated at 108 Go rows against
# 89 C# -- which is a host BUDGET question, never a verdict, so the row's `linux: 108` annotation
# stands and the floor is what moves. A floor, not an override: a larger -TestTimeout still wins.
$longTimeouts = @{ 'hash/maphash' = '60m'; 'index/suffixarray' = '120m'; 'crypto/dsa' = '120m'; 'archive/zip' = '60m'; 'go/parser' = '90m'; 'crypto/internal/mlkem768' = '30m'; 'time' = '40m'; 'crypto/tls' = '30m'; 'sync/atomic' = '90m'; 'net' = '40m'; 'net/http' = '60m' }
# 'net' joined 2026-09-02: at the 10m default the C# host dies an EXPLICIT results-tail deadline kill on
# the i7 class (the mass-empty shape), and at 40m the same tree validates 472/472 in ~1,480 s -- deadline
# sizing, not divergence (measured twice: the MakeFunc canary gate 2026-08-29 and the A2a gate 2026-09-02).
#
# 'net/http' joined 2026-09-02, and its floor is sized to a TRUNCATED measurement -- read the arithmetic
# below before moving it. Two arms of the SAME row, i7 class, Debug (the configuration the pipeline
# publishes today), bracket the 30m the train passes: one arm finished in 1,836 s, the other took
# 2,171 s and its results tail states `package timeout after 00:30:00` outright. The killed arm's
# signature is the documented one and nearly reads as a regression: 18 EXTRA empty verdicts on top of
# the row's four real h2 deadline failures (a separate, known disclosure-class item -- not what this
# floor is about) -- a contiguous alphabetical tail with the parked parallel batch interleaved through
# it, which reads SCATTERED and is not. Measured by lane claude/sub-os-row; board entry 2026-09-02 in
# docs/phase4/BOARD-next-validation-candidates.md.
#
# ARITHMETIC. 30m = 1,800 s is demonstrably short and the 10m default more so. The board does not
# decompose the two figures into convert/build/run legs, so take them as row walls: either way the
# killed arm proves the RUN leg alone exceeds 1,800 s, by however long the 18 unreported verdicts
# want -- 2,171 s is a LOWER BOUND on this row, not its cost. That rules out the next bracket up:
# 40m = 2,400 s clears the observed top by 1.11x, which is INSIDE the 335 s (18%) spread the two arms
# show on one host on one day, and BELOW every headroom this table already carries (hash/maphash 1.50x
# over its 2,406 s, net 1.62x over its ~1,480 s, crypto/tls 2.79x over its 644.8 s, mlkem768 4.14x over
# its 434.7 s, zip and parser ~2x on their LOADED figures -- and a full sweep's disk/cache pressure is
# the 2.5-3x multiplier the zip/parser entry above measures). 60m = 3,600 s is the next bracket the
# table already uses and it lands inside that band: 2.00x the deadline that demonstrably cut the run,
# 1.66x the observed top, 1.96x the completed arm. Applying net's own 1.62x to 2,171 s gives 3,517 s --
# 58.6 min, i.e. this same bracket -- so 60m is what this table's sizing habit produces, not a generous
# round-up; nothing measured argues for 90m/120m, which are the storage-hammering and heavy-tail rows.
# Re-measure when the Release + tiering-off configuration of record lands -- it re-times every row, and
# this is a Debug figure taken on one machine class.

# ---- per-package cgo state ------------------------------------------------------------------------
# A package whose Go FILE SELECTION is cgo-conditional must be converted in the same cgo state the
# committed corpus was emitted in. That state is CGO_ENABLED=0 (CLAUDE.md's emission-state rule), so
# both sides of the comparison see ONE selection; converting cgo-ON against a cgo-OFF corpus changes
# which files exist and the build dies on declarations that migrated.
#
# 'os/user' joined 2026-09-02, measured on a cloud Linux host as a one-variable A/B on the same row:
#   CGO_ENABLED=1 -> FAIL in 12 s, zero verdicts, the closure build dying; the run leaves
#                    cgo_unix_test.cs / cgo_user_test.cs behind, artifacts with no Windows counterpart
#   CGO_ENABLED=0 -> validated at 12, all 12 agreeing, 0 disclosed, 0 withdrawn, a strict superset of
#                    the 5 banked Windows names (the 7 extra are lookup_unix_test.go's, which
#                    `unix && !android && !cgo && !darwin` selects only when cgo is off)
#
# PER-PACKAGE, never session-wide: the three rows whose Linux annotations were derived cgo-ON
# (debug/buildinfo, go/internal/gcimporter, go/internal/srcimporter) keep their state, and a
# session-wide zero would bring them back short. Harmless on the other two targets by construction:
# Windows os/user carries no cgo constraint at all, and darwin selects the cgo_* files through the
# `(cgo || darwin)` disjunct whatever CGO_ENABLED says.
# 'net' and 'plugin' joined 2026-09-02 by the same one-variable A/B on the same host:
#   net     CGO_ENABLED=1 -> FAIL in 183 s, zero verdicts, the build dying with cgo_stub.cs absent
#                            from the run's own drift list (not re-emitted because not selected);
#                            the committed tree holds only that cgo-OFF arm.
#           CGO_ENABLED=0 -> the suite BUILDS and runs 503 verdicts. The row still does not
#                            validate on this host -- it is the Linux frontier lane R mapped, and
#                            it hit its own 40m deadline here -- but the pin is what makes the
#                            comparison legitimate at all, which is a separate question from
#                            whether the row passes.
#   plugin  CGO_ENABLED=1 -> FAIL in 126 s.
#           CGO_ENABLED=0 -> PASS 1. plugin_dlopen.go ((linux && cgo) || ...) is literal C
#                            (import "C", #include <dlfcn.h>); plugin_stubs.go (... || !cgo) is
#                            pure Go and is the arm the corpus holds.
# 'reflect' joined 2026-09-02 under a THIRD predicate the coordinator ruled from C2's cgo-ON
# reflect -tests build failure: a file that is TEST-conditional on cgo AND imports a package the
# corpus does not carry. reflect/nih_test.go is `//go:build cgo` and imports `runtime/cgo`, which
# has no src/core counterpart -- so under cgo-ON the test variant cannot build at all. This is the
# exception to "test-only conditionality is a count question, never a build one".
#
# The predicate is CENSUSED and bounded, not assumed: walking every //go:build line in the whole
# 1.23.12 stdlib for `cgo` and checking each such file's imports against src/core, reflect's
# nih_test.go is the ONLY member -- one file, one import. Every other cgo-gated test file imports
# only packages the corpus carries, so debug/pe, os/exec and os/signal stay count-only and unpinned.
#
# 'syscall' joined 2026-09-02 under a FOURTH predicate, and the coordinator WIDENED the table's
# rule to state it: cgo changes which FILES are converted OR which BRANCH the ORACLE'S OWN TESTS
# TAKE. syscall's file selection is not cgo-conditional at all -- the three AllThreadsSyscall tests
# open with `if err == ENOTSUP { t.Skip("AllThreadsSyscall disabled with cgo") }`, and real Go
# returns ENOTSUP there ONLY when cgo is linked. Measured on one host, one release, one variable:
# CGO_ENABLED=0 -> pass/pass/pass, CGO_ENABLED=1 -> skip/skip/skip.
#
# Why that had to be pinned rather than left to the ambient state: the CONVERTED side skips those
# three too, with the same text -- but for an unrelated reason. Our banked runtime_doAllThreadsSyscall
# hand-own returns ENOTSUP because a managed runtime cannot run a raw syscall on every OS thread, and
# the test reads that errno as "cgo is linked". So at ambient cgo-ON the row AGREES by a coincidence
# of errno, with the oracle configured into the same limitation we have for a different cause -- the
# instrument-built-out-of-the-thing-under-test family, and not something to bank an agreement on.
# Pinned cgo-OFF the three read go=pass / cs=skip, which is the honest shape: a disclosed HOST LIMIT
# (minted separately), never a match. The doctrine already decided it -- both comparison sides share
# ONE cgo state and the converted side can only be the corpus's.
# RETIRED 2026-09-03 (coordinator ruling a29200e47): the per-package table
#   @{ 'os/user'; 'net'; 'plugin'; 'reflect'; 'syscall' }
# became a WHOLE-RUN pin (see the cgo-pin block above the row loop). The measurements above are
# kept as the record of WHY those five rows needed it first -- each was a one-variable A/B on one
# host -- and of the fourth predicate that widened the rule to the oracle's own branches, which is
# the predicate the whole-run pin now applies to every row rather than to the five that had been
# measured by hand.

foreach ($row in $rows) {
    $pkg = $row.Package
    # The import path is already forward-slash-separated, which is exactly the form Join-Path
    # normalizes for us -- so the -replace that hand-built a Windows path is not just unnecessary,
    # it was the thing that made this mapping wrong off Windows.
    $outDir = Join-Path $src "core/$pkg"
    $goDir = Join-Path $goroot "src/$pkg"
    $label = '{0,-34}' -f $pkg
    # The longer of the two, passed VERBATIM -- whichever string wins reaches the converter exactly
    # as it was written, never re-formatted through the TimeSpan the comparison went by.
    $pkgTimeout = $TestTimeout

    if ($longTimeouts.ContainsKey($pkg)) {
        $floor = ConvertTo-GoDuration $longTimeouts[$pkg]
        $asked = ConvertTo-GoDuration $TestTimeout
        $raisesTheFloor = ($null -ne $asked) -and ($null -ne $floor) -and ($asked -gt $floor)
        if (-not $raisesTheFloor) { $pkgTimeout = $longTimeouts[$pkg] }
    }

    # -go2cspath is pinned to $src (this script's own directory) rather than inherited from the
    # ambient GO2CSPATH. A -tests run already self-locates the root from its output path, which lands
    # under src\core, so on a healthy box this is the same value -- but only when the ambient root is
    # INVALID does that recovery run: a GO2CSPATH pointing at some other real go2cs tree (a
    # deploy-core staging root, say) would be honored instead, and the suite would be built against
    # one tree's metadata while compiling the other's sources.
    # The per-row EXECUTION config (owner ruling 2026-08-30, Option A). A row that annotates itself
    # `execution: release-tc0` runs ITS leg under that config; every other row's invocation is
    # character-for-character the one it has always produced, because Get-RosterExecutionArgs
    # contributes an EMPTY array for the absent config. The sweep-wide -TestConfig/-TestTiered
    # override (2026-09-02, generalized from -ReleaseTC0) is expressed as "every row, annotated or
    # not, behaves as if annotated with THIS config" -- one rule, not two code paths, and it takes
    # the -test-config/-test-tiered flags directly rather than through the roster's config-name
    # indirection, since a sweep-wide A/B is not limited to the one config the roster vocabulary
    # names.
    if ($sweepConfigOverride) {
        $execArgs = @('-test-config', $TestConfig)
        if ($TestTiered) { $execArgs += '-test-tiered' }
        $execSuffix = " [test-config=$TestConfig$(if ($TestTiered) { ' tiered' })]"
    }
    else {
        $execArgs = @(Get-RosterExecutionArgs $row.Execution)
        # Printed on the verdict line so an opted-in row's evidence says so on its face; empty, and
        # therefore invisible, for every default-path row.
        $execSuffix = if ($row.Execution) { " [$($row.Execution)]" } else { '' }
    }

    $rowStarted = Get-Date
    $out = Invoke-SweepRow -Package $pkg -GoDir $goDir -OutDir $outDir -PkgTimeout $pkgTimeout -ExecArgs $execArgs
    # Per-row wall time, printed on every verdict line. This is the SWEEP's wall clock for the row
    # (convert + build + both test hosts + compare), which is the number shard planning needs --
    # the go test -json stream's own "Time" fields measure only the Go side and invert exactly the
    # rows that dominate a shard (hash/maphash: 7.6s in Go, ~40min in C#). After an oracle re-run it
    # is the SECOND attempt's wall, which is the run the verdict below describes.
    $rowSecs = [int]((Get-Date) - $rowStarted).TotalSeconds
    $verdict = ($out | Select-String 'Validated (\d+) tests against go test' | Select-Object -First 1)

    # ---- the ORACLE RE-RUN ARM (coordinator ruling 2026-09-02) -----------------------------------
    # Reached only when the row produced NO "Validated N" line, which is where an oracle flake always
    # lands: any mismatch clears Matched, so the converter returns an error and never prints it.
    # Test-OracleOnlyFailure (_roster.ps1) then reads the run's own evidence and answers one
    # question -- was every divergence Go=fail with C#=pass, on a run that otherwise completed? If
    # so the row is re-run ONCE and the SECOND run is what the classification below describes; if
    # the second run is oracle-only as well the row takes its own verdict word rather than FAIL.
    # A row with any converted-side failure never enters here: the rule refuses it by name, its
    # reason is printed under the FAIL line, and nothing about that row's handling has moved.
    $oracleFlakedNames = @()
    $oracleUnstable = $false
    $oracleReason = $null

    if (-not $verdict) {
        $oracle = Get-OracleOnlyVerdict -OutDir $outDir -Since $rowStarted
        $oracleReason = $oracle.Reason

        if ($oracle.OracleOnly) {
            $oracleFlakedNames = @($oracle.Flaked)
            # PRESERVE BEFORE THE RE-RUN. The second attempt overwrites the record and the results
            # file in place, so this is the only moment run 1's evidence exists.
            $preserved = Save-OracleEvidence -Package $pkg -OutDir $outDir -Attempt 1
            Write-Host ("  RERUN $label oracle-only failure set: $($oracleFlakedNames.Count) case(s), every one Go=fail/C#=pass, " +
                "zero converted-side failures -- re-running once [${rowSecs}s]") -ForegroundColor Magenta
            Write-Host "        run 1 evidence preserved at $preserved" -ForegroundColor DarkGray

            $rowStarted = Get-Date
            $out = Invoke-SweepRow -Package $pkg -GoDir $goDir -OutDir $outDir -PkgTimeout $pkgTimeout -ExecArgs $execArgs
            $rowSecs = [int]((Get-Date) - $rowStarted).TotalSeconds
            $verdict = ($out | Select-String 'Validated (\d+) tests against go test' | Select-Object -First 1)

            if (-not $verdict) {
                $oracleSecond = Get-OracleOnlyVerdict -OutDir $outDir -Since $rowStarted
                $oracleReason = $oracleSecond.Reason
                $oracleUnstable = $oracleSecond.OracleOnly
                [void](Save-OracleEvidence -Package $pkg -OutDir $outDir -Attempt 2)
            }
        }
    }

    if ($verdict) {
        $got = [int]$verdict.Matches[0].Groups[1].Value
        $gotDisclosed = Get-DisclosedCount -Output $out
        # 'columns' on Windows for every row, so the suffix is empty and the string is unchanged;
        # off Windows an annotated row says which OS's expectation it just met.
        $osSuffix = if ($row.Effective.Source -eq 'columns') { '' } else { " ($($row.Effective.Source))" }

        # The rule itself is a pure function in _roster.ps1, guarded by check-roster-format.ps1;
        # what stays here is the evidence-gathering and the reporting. A row that declares
        # host-conditional verdicts gets one chance to PROVE a surplus is exactly those named rows
        # materializing on a more-capable host -- consulted only when the plain classification is
        # not already a pass, since it costs a comparison-record read and a git show. Anything
        # unprovable falls through to the same failure as before, with the rejection reason attached.
        #
        # ⚠ On a hop run the classification is not CONSULTED, and that is a deliberate bypass rather
        # than a new arm inside the pure rule. Two reasons. (1) Correctness: with Expected $null the
        # rule would fall through `$Got -eq $Expectation.Expected` and answer 'unbanked-count' off
        # Windows and 'count' on Windows -- CVAC's word for the wrong reason on one platform and a
        # false FAIL on the other, for every row. (2) Scope: Get-SweepRowClassification is guarded by
        # check-roster-format.ps1, and a new arm there is a rule that needs its own fixtures; the hop
        # answer needs no rule, because there is nothing to compare. The three absorption arms below
        # are skipped with it -- each exists to excuse a count DELTA against a floor, and a row with
        # no floor has no delta to excuse. They also cost a comparison-record read and a git show per
        # row, which on 227 rows would buy nothing.
        $class = if ($Hop) { 'hop' }
                 else { Get-SweepRowClassification -Expectation $row.Effective -Got $got -GotDisclosed $gotDisclosed -TargetGoos $targetGoos }
        $hostConditional = $null

        if (-not $Hop -and $class -ne 'pass' -and $row.Conditional.Count -gt 0) {
            $hostConditional = Get-HostConditionalVerdict -Row $row -Got $got -OutDir $outDir

            if ($hostConditional.Accepted) {
                $class = Get-SweepRowClassification -Expectation $row.Effective -Got $got -GotDisclosed $gotDisclosed `
                    -TargetGoos $targetGoos -HostConditionalAccepted
            }
        }

        # The FOURTH absorption (Q31): a row naming host-conditional DISCLOSURES gets one chance to
        # prove that its moved disclosed count is exactly those entries firing on this host with the
        # same verdicts leaving the matched column -- consulted only on the disclosed-moved answer,
        # from the live record alone. Anything unprovable falls through with its reason attached.
        $hostConditionalDisclosure = $null
        if ($class -eq 'disclosed-moved' -and $row.ConditionalDisclosures.Count -gt 0) {
            $hostConditionalDisclosure = Get-HostConditionalDisclosureVerdict -Row $row -Got $got -GotDisclosed $gotDisclosed -OutDir $outDir

            if ($hostConditionalDisclosure.Accepted) {
                $class = Get-SweepRowClassification -Expectation $row.Effective -Got $got -GotDisclosed $gotDisclosed `
                    -TargetGoos $targetGoos -HostConditionalDisclosureAccepted
            }
        }

        # The mirror check: a package registered in $capabilityConditionalBlocks whose shortfall
        # matches its block exactly gets the same one chance to PROVE the shape, via evidence alone
        # -- no host probe, the comparison record and proof page either show the collapse or they
        # don't. Consulted only when the plain classification already failed, same cost discipline
        # as the surplus check above.
        $capabilityAbsent = $null
        $hostLimit = $null
        # -not $Hop, explicitly: this arm's guard is `$class -ne 'pass'`, and 'hop' is not 'pass', so a
        # registered package (crypto/tls) would otherwise reach the capability-absent and host-limit
        # probes on a hop run and try to excuse a shortfall against a floor that does not exist. The
        # disclosed-moved arm above needs no such guard -- its own `$class -eq 'disclosed-moved'` is
        # already unreachable from 'hop'.
        if (-not $Hop -and $class -ne 'pass' -and $capabilityConditionalBlocks.ContainsKey($pkg)) {
            $rowBlock = $capabilityConditionalBlocks[$pkg]
            $capabilityAbsent = Get-CapabilityAbsentVerdict -Row $row -Got $got -OutDir $outDir -Block $rowBlock

            if ($capabilityAbsent.Accepted) {
                $class = Get-SweepRowClassification -Expectation $row.Effective -Got $got -GotDisclosed $gotDisclosed `
                    -TargetGoos $targetGoos -CapabilityAbsentAccepted
            }
            else {
                # The THIRD host state: the capability was PRESENT and the converted side could not
                # produce the block inside the deadline the test itself carries. Absorbed only where
                # the package's COMMITTED manifest already discloses the block root as `host-limit`
                # AND the withdrawn Go-side rows ARE that block's banked sub-verdicts, name for name.
                $hostLimit = Get-HostLimitVerdict -Row $row -Got $got -OutDir $outDir -Block $rowBlock

                if ($hostLimit.Accepted) {
                    $class = Get-SweepRowClassification -Expectation $row.Effective -Got $got -GotDisclosed $gotDisclosed `
                        -TargetGoos $targetGoos -HostLimitAccepted
                }
            }
        }

        switch ($class) {
            'pass' {
                $pass++
                Write-Host "  PASS  $label $got$osSuffix$execSuffix [${rowSecs}s]" -ForegroundColor Green
            }
            'host-conditional' {
                $pass++
                Write-Host "  PASS  $label $got = $($row.Effective.Expected) banked + $($hostConditional.Extras.Count) host-conditional [${rowSecs}s]" -ForegroundColor Green
            }
            'host-conditional-disclosure' {
                # A pass, and NOT a silent one: the line names the entry that fired and that its
                # verdict changed COLUMN on this host; summarized again at the end for a full sweep.
                $pass++
                $firedList = ($hostConditionalDisclosure.Fired -join ', ')
                $hostConditionalDisclosed += "$pkg ($got matched + $gotDisclosed disclosed, banked $($row.Effective.Expected) + $($row.Effective.Disclosed); $firedList fired on this host)"
                Write-Host "  PASS  $label $got = $($row.Effective.Expected) banked - $($hostConditionalDisclosure.Fired.Count) host-conditional disclosure ($firedList fired on this host)$osSuffix [${rowSecs}s]" -ForegroundColor Green
            }
            'capability-absent' {
                $pass++
                $block = $capabilityConditionalBlocks[$pkg]
                Write-Host "  PASS  $label $got = $($row.Effective.Expected) banked - $($block.BlockSize) ($($block.Test) capability absent) [${rowSecs}s]" -ForegroundColor Green
            }
            'host-limit' {
                # A pass, and NOT a silent one: the line states the shortfall, the block that
                # accounts for it, and that the capability was PRESENT -- so this can never read as
                # the row having met its banked count. Summarized again at the end for a full sweep.
                $pass++
                $block = $capabilityConditionalBlocks[$pkg]
                $hostLimited += "$pkg ($got matched + $gotDisclosed disclosed, banked $($row.Effective.Expected) + $($row.Effective.Disclosed); $($block.Test) host-limit disclosed)"
                Write-Host "  PASS  $label $got = $($row.Effective.Expected) banked - $($block.BlockSize) ($($block.Test) host-limit disclosed; capability PRESENT, converted side over the deadline) [${rowSecs}s]" -ForegroundColor Green
            }
            'unbanked-count' {
                # COMPARISON-VALIDATED-AT-COUNT, the honest interim the per-OS ruling names. The
                # comparison reached "Validated N" -- the converter prints that line only after
                # matching every verdict -- but N was measured against the WINDOWS columns, and this
                # is not Windows. Nothing is banked for this OS, so it is not a pass; nothing is
                # wrong either, so it is not the silent-drift failure below. It is its own report,
                # and it retires row by row as annotations land.
                $cvac++; $cvacRows += "$pkg (count $got, windows column $($row.Expected))"
                Write-Host "  CVAC  $label $got (validated; no $targetGoos expectation, windows column $($row.Expected)) [${rowSecs}s]" -ForegroundColor Cyan
            }
            'hop' {
                # HOP: the comparison reached "Validated N" -- the converter prints that line only
                # after matching every verdict, so the two runtimes agreed row for row -- and there is
                # no expectation at this release to weigh N against. The record IS the deliverable:
                # this line, and the per-row TSV below, are what H10 banks into the roster.
                #
                # Its own counter, never folded into $cvac: CVAC means "no expectation for this OS",
                # this means "no expectation at this release", and a reader of a mixed log must be able
                # to tell which absence applied per row. It does not gate the exit (see $hop's
                # declaration for the reasoning, and the exit arms at the foot).
                #
                # A RECEIVED predecessor is printed as provenance and nothing is compared against it:
                # the census's own ruling is that a relocated row re-banks from zero. Printing it is
                # what stops the record reading as though 20 verdicts for these tests had never been
                # banked under another name.
                $hop++
                if ($row.Receives) {
                    $hopRows += "$pkg (count $got, disclosed $gotDisclosed; receives $($row.Receives))"
                    Write-Host "  HOP   $label $got (measured; no expectation at this release; receives $($row.Receives)) [${rowSecs}s]" -ForegroundColor Magenta
                }
                else {
                    $hopRows += "$pkg (count $got, disclosed $gotDisclosed)"
                    Write-Host "  HOP   $label $got (measured; no expectation at this release) [${rowSecs}s]" -ForegroundColor Magenta
                }
            }
            'disclosed-moved' {
                # An annotated row whose matching count agreed and whose DISCLOSED count did not.
                # Named as itself rather than mis-reported as a count failure.
                $fail++
                $failed += "$pkg (disclosed $gotDisclosed, $($row.Effective.Source) expectation $($row.Effective.Disclosed))"
                Write-Host "  DISC  $label $got, disclosed $gotDisclosed vs the $($row.Effective.Source) expectation $($row.Effective.Disclosed) [${rowSecs}s]" -ForegroundColor Yellow
                if ($null -ne $hostConditionalDisclosure -and $hostConditionalDisclosure.Reason) {
                    Write-Host "        host-conditional-disclosure check: $($hostConditionalDisclosure.Reason)" -ForegroundColor Yellow
                }
            }
            default {
                # Validated, but NOT at the expectation in force -- normally a silent change in what
                # the suite asserts, and a failure: the table and reality must agree, one of them is
                # now wrong.
                $fail++; $failed += "$pkg (count $got, banked $($row.Effective.Expected))$execSuffix"
                Write-Host "  COUNT $label $got, banked $($row.Effective.Expected)$execSuffix [${rowSecs}s]" -ForegroundColor Yellow
                if ($null -ne $hostConditional -and $hostConditional.Reason) {
                    Write-Host "        host-conditional check: $($hostConditional.Reason)" -ForegroundColor Yellow
                }
                if ($null -ne $capabilityAbsent -and $capabilityAbsent.Reason) {
                    Write-Host "        capability-absent check: $($capabilityAbsent.Reason)" -ForegroundColor Yellow
                }
                if ($null -ne $hostLimit -and $hostLimit.Reason) {
                    Write-Host "        host-limit check: $($hostLimit.Reason)" -ForegroundColor Yellow
                }
            }
        }

        # THE LEDGER NOTE. A row that needed two runs must never read like a row that needed one, so
        # the flake is stated on the row's own face, next to the verdict it produced -- and the
        # verdict above describes RUN 2, whatever it was. The names are capped at a dozen so this
        # stays the one loud line the ruling asks for; the full set is in the preserved run-1 record,
        # whose path was printed when the re-run was announced.
        if ($oracleFlakedNames.Count -gt 0) {
            $shown = @($oracleFlakedNames | Select-Object -First 12)
            $moreNote = if ($oracleFlakedNames.Count -gt $shown.Count) { " (+$($oracleFlakedNames.Count - $shown.Count) more in the preserved run-1 record)" } else { '' }
            Write-Host ("        ORACLE FLAKED ONCE -- run 1 diverged on the oracle side only " +
                "($($oracleFlakedNames.Count) case(s) Go=fail/C#=pass); run 2 is the verdict above. " +
                "Flaked: $($shown -join ', ')$moreNote") -ForegroundColor Magenta
            $oracleFlakedRows += "$pkg ($($oracleFlakedNames.Count) oracle-only case(s) on run 1, re-run classified '$class')"
        }
    }
    elseif ($oracleUnstable) {
        # BOTH runs oracle-only. Its own verdict word, counted apart from FAIL -- the converted side
        # passed every one of these cases twice, so calling it a corpus failure would be false, and
        # calling it a pass would be a green gate over an unmeasurable host. Non-zero exit either way.
        $unstable++
        $unstableRows += "$pkg ($($oracleFlakedNames.Count) oracle-only case(s) on run 1; run 2 oracle-only as well)"
        Write-Host "  ORACLE $label oracle unstable on this host -- two oracle-only runs, no converted-side failure in either [${rowSecs}s]" -ForegroundColor Red
        Write-Host "        both runs' evidence preserved under $oracleEvidenceRoot" -ForegroundColor DarkGray
    }
    elseif ($Hop -and (Test-HopRowHasNoEligibleTests -Output $out -OutDir $outDir)) {
        # THE SECOND HOP WORD. The row ran, the pipeline reported that this package has no eligible Go
        # tests for the target, and it said so the way it says so on SUCCESS: the line below plus a
        # comparison record whose status is `not-applicable`. That pair is the derivation of an
        # `<goos>: n/a` annotation at the incoming release -- which is why it may never be reachable by
        # a row that merely produced no counts. Both signals are required and their disagreement is a
        # FAIL, inside the predicate.
        #
        # NOT a pass: nothing was measured here, and it must never read as a re-validated row. Not a
        # failure either, and this is the one place where saying so is a positive measurement rather
        # than an absence: the pipeline's own capability-blocked sibling state returns an ERROR and
        # lands in the FAIL arm below, so "no eligible tests" is distinguished from "blocked" by the
        # instrument, not by this script's reading of a log tail.
        #
        # ⚠ REACHABILITY, because it depends on another rule and is not obvious from here: a row with
        # no verdict line passes through the ORACLE RE-RUN ARM above first, and that arm would double
        # this row's cost -- and then take it away with an 'oracle unstable' verdict on the second
        # empty run -- if it accepted an empty divergence set as "every divergence was Go=fail/C#=pass"
        # (vacuously true). It does not: Test-OracleOnlyFailure refuses any record whose status is not
        # 'failing', and a no-eligible-tests record's status is 'not-applicable', so the rule answers
        # NO with that reason and no re-run happens. This arm is reachable BECAUSE of that guard; if it
        # is ever relaxed, this arm must move ahead of the re-run rather than behind it.
        $hopNoTests++
        $hopNoTestsRows += "$pkg (ran; no eligible tests for $targetGoos)"
        Write-Host "  HOPNONE $label ran here; NO eligible tests for $targetGoos -- derives a '${targetGoos}: n/a' annotation [${rowSecs}s]" -ForegroundColor DarkCyan
    }
    else {
        $fail++; $failed += $pkg
        Write-Host "  FAIL  $label [${rowSecs}s]" -ForegroundColor Red
        $out | Select-Object -Last 3 | ForEach-Object { Write-Host "        $_" -ForegroundColor DarkGray }
        # WHY the oracle re-run did not fire. Printed always, because the interesting case is the one
        # where a reader expected it to: a stale record, a deadline in the tail, or a converted-side
        # divergence each say something different about what this row just did.
        if ($oracleReason) { Write-Host "        oracle-only check: $oracleReason" -ForegroundColor DarkGray }
    }

    # One record per row, taken here rather than inside each verdict arm so no arm can be added later
    # that forgets to contribute -- the loop body cannot reach its own end without passing this line.
    # $rowSecs is the SWEEP's wall clock for the row (convert + build + both hosts + compare), and
    # after an oracle re-run it is attempt 2's, matching the verdict that was printed.
    $rowCountForTiming = $null
    if ($verdict) { $rowCountForTiming = [int]$verdict.Matches[0].Groups[1].Value }
    [void]$rowTimings.Add([PSCustomObject]@{
        Package = $pkg
        Seconds = $rowSecs
        Count   = $rowCountForTiming
    })
}

$elapsed = [int]((Get-Date) - $started).TotalSeconds
Write-Host ''
# The comparison-validated-at-count segment appears only when there is one to report -- which on
# Windows is never, so the summary line is byte-for-byte the line it has always printed there.
$summary = "sweep: $pass pass"
if ($hostLimited.Count) { $summary += " ($($hostLimited.Count) host-limited)" }
if ($hostConditionalDisclosed.Count) { $summary += " ($($hostConditionalDisclosed.Count) host-conditional disclosure fired)" }
if ($oracleFlakedRows.Count) { $summary += " ($($oracleFlakedRows.Count) after an oracle re-run)" }
$summary += " / $fail fail"
if ($unstable) { $summary += " / $unstable oracle-unstable" }
if ($cvac) { $summary += " / $cvac comparison-validated-at-count" }
# SIDE BY SIDE, never folded into $cvac's segment, as ruled: `hop=` and `cvac=` name two different
# reasons for the same missing expectation, and a derivation that read one as the other would annotate
# a release-wide absence as a platform-specific one. Printed with an explicit `hop=` key rather than a
# prose phrase because this is the segment a driver greps.
if ($hop) { $summary += " / hop=$hop measured-at-count" }
if ($hopNoTests) { $summary += " / hop-no-tests=$hopNoTests" }
$summary += "  (${elapsed}s)"
# The colour follows the EXIT, so the line cannot look like one thing and exit as another: a hop run
# whose rows all recorded is green, because recording is what it was asked to do. $fail and $unstable
# still redden it, on a hop run exactly as on a sweep.
Write-Host $summary -ForegroundColor $(if ($fail -or $unstable -or ($cvac -and -not $Hop)) { 'Red' } else { 'Green' })

# The pipeline regenerates each package's committed test artifacts in place. Content drift is a
# real signal; CRLF-only churn is not (autocrlf smudges LF fixtures on checkout) -- so report by
# CONTENT, using the same --ignore-cr-at-eol discriminator the rebank doctrine uses.
# Do NOT redirect git's stderr: in PS 5.1 redirecting a native command's stderr wraps each line
# in an ErrorRecord, which under $ErrorActionPreference='Stop' aborts the script. Silence the
# autocrlf notice at its source instead, and relax the preference across the call.
$prevEap = $ErrorActionPreference
$ErrorActionPreference = 'Continue'
$drift = & git -C $repo -c core.safecrlf=false diff --numstat --ignore-cr-at-eol -- src/core

if ($drift) {
    # A couple of dozen production files drift on EVERY sweep, and none of them is a problem. A
    # `-tests` run converts the package in its TEST closure, which imports more than the production
    # closure, and a wider closure legitimately changes three things in the production emission:
    #
    #   Delta-io alias      `using io = io_package;` becomes a shadow-renamed alias, because the
    #                       test closure pulls in a `go.io` CHILD namespace that `io` would collide
    #                       with (bufio, bytes, strings, regexp, crypto, hash, image).
    #   root qualification  `@internal.x` / `go.math` become root-qualified, because the test
    #                       closure imports a package whose own namespace shadows the root
    #                       (crypto/md5's byteorder; math/rand/v2's `go/format` via regress_test.go).
    #   init-tests hook     production `package_init.cs` gains the partial-method hook the test
    #                       variant's relocated initializers implement -- and the compiler erases
    #                       it again when that half implements nothing, which is why NO committed
    #                       package_init.cs carries the hook and the drift is always the hook
    #                       APPEARING, never changing or vanishing.
    #                       ⚠ THIS CLASS IS DERIVED, AND A BANK OWES IT NOTHING. It used to be six
    #                       hand-maintained rows in the list below under the standing warning "THIS
    #                       LIST IS OWED BY EVERY BANK" -- a debt that went unpaid twice, and each
    #                       time false-redded EVERY full sweep against a healthy package until
    #                       someone re-derived the missing row from the failure (internal/profile,
    #                       2026-08-09; syscall, 2026-08-10). Nothing is listed now. The candidates
    #                       are every package_init.cs in the corpus, recognized by their PATH shape
    #                       ($initHookPathShape), and membership is decided entirely by CONTENT in
    #                       Test-InitTestsHookDrift -- which is also STRICTLY TIGHTER than the list
    #                       it replaces: it requires the hook to be the thing that appeared and
    #                       rejects any removed line, where a listed file was judged on its added
    #                       lines alone.
    #                       ⚠ THE PATH SHAPE COUNTS NO DIRECTORY SEGMENTS, deliberately. Under
    #                       layout L3 a platform-varying package keeps its copy at
    #                       `<pkg>/<goos>/package_init.cs` while every other package keeps it flat
    #                       at `<pkg>/package_init.cs`; one depth-agnostic pattern matches both, so
    #                       the flat-shape assumption that hid syscall's row is not even available
    #                       to make. Spelling the GOOS names instead would have re-created exactly
    #                       the maintenance this retires, at the first new port.
    #
    # Both emissions are correct for their own closure -- only the pipeline pairs them -- so this is
    # owed to whoever owns the next whole-corpus rebank, not to the person running a sweep today.
    # See the charter's `math/rand/v2` worked example and DESIGN-named-interface-wrappers.md section 7.
    #
    # Listing them under the same warning as real drift trains the reader to skip the warning, which
    # is how a genuine regression gets waved through. They get their own section instead -- and only
    # if their content still MATCHES the class, so a stale entry cannot hide a real change.
    #
    # What stays a NAME LIST is the alias/qualification class alone. Those are ordinary production
    # sources with no structural signature to enumerate by, and -- unlike the hook class -- they do
    # not gain a member every time a package banks: the set moves only when the converter's aliasing
    # or qualification changes, which is a converter arc with its own review, not a bank's paperwork.
    $closureFiles = @(
        'src/core/bufio/bufio.cs'
        'src/core/bufio/scan.cs'
        'src/core/bytes/buffer.cs'
        'src/core/bytes/reader.cs'
        'src/core/crypto/crypto.cs'
        'src/core/crypto/md5/md5.cs'
        'src/core/crypto/md5/md5block.cs'
        'src/core/hash/hash.cs'
        'src/core/image/format.cs'
        'src/core/math/rand/v2/pcg.cs'
        'src/core/math/rand/v2/rand.cs'
        'src/core/regexp/backtrack.cs'
        'src/core/regexp/exec.cs'
        'src/core/regexp/regexp.cs'
        'src/core/strings/reader.cs'
        'src/core/strings/replace.cs'
    )

    # The DERIVED class's candidate set: any package_init.cs anywhere under the corpus. `.+` spans
    # any number of directory segments, so `syscall/windows/package_init.cs` (layout L3) and
    # `unicode/package_init.cs` (flat) are the same pattern and no GOOS name appears anywhere.
    # Being a candidate decides nothing on its own -- Test-InitTestsHookDrift below still has to
    # recognize the content.
    $initHookPathShape = '^src/core/.+/package_init\.cs$'

    # Marker glyphs come from the canonical symbol table, never spelled here -- the standing rule
    # for every consumer of the converter's naming constants.
    $symbols = (Get-Content (Join-Path $src 'core/go2cs/symbols.json') -Raw | ConvertFrom-Json).symbols
    $symbolValue = { param($name) ($symbols | Where-Object { $_.name -eq $name }).value }
    $shadow = & $symbolValue 'ShadowVarMarker'
    $temp = & $symbolValue 'TempVarMarker'
    $root = & $symbolValue 'RootNamespace'

    # What an ADDED line in a closure-class diff may look like. Anything else means the file changed
    # for some OTHER reason as well, and it goes back to the warning block where it belongs.
    $closureShapes = @(
        [regex]::Escape("$shadow" + 'io')                     # the shadow-renamed io alias
        [regex]::Escape('global::' + $root + '.')             # root-qualified reference
        [regex]::Escape($root + '.@internal')                 # root-qualified internal package
        [regex]::Escape('init' + $temp + $temp + 'tests')     # the -tests init hook (see below)
        '^\s*//'                                              # a comment carried by the reorder
        '^\s*$'                                               # blank separator
    )

    # Judges the NAME-LISTED alias/qualification class only. (The hook shape stays in the list
    # above because this predicate is generic, but no listed file can gain one: the hook is emitted
    # into package_init.cs alone, and those are routed to Test-InitTestsHookDrift instead.)
    function Test-ClosureClassDrift([string] $path) {
        $added = & git -C $repo -c core.safecrlf=false diff --ignore-cr-at-eol -U0 -- $path |
            Where-Object { $_ -match '^\+' -and $_ -notmatch '^\+\+\+' } |
            ForEach-Object { $_.Substring(1) }

        foreach ($line in $added) {
            if (-not ($closureShapes | Where-Object { $line -match $_ })) { return $false }
        }

        return $true
    }

    # Judges the DERIVED init-tests hook class, and it is the whole safety argument for having no
    # name list: a candidate is absorbed only when its diff IS the hook, exactly as
    # writePackageInitFile emits it (initOrderOperations.go) -- the call inside the static ctor, a
    # blank line, the four-line explanation, and the erasable declaration. Seven added lines, none
    # removed. Three conditions, each closing a way a real change could pass for the class:
    #
    #   nothing removed   The committed corpus holds the production emission and the -tests
    #                     emission is that PLUS the hook, so this class only ever adds. A
    #                     package_init.cs that LOSES a line has lost a relocated initializer --
    #                     a real regression, and the one the name list waved through, since it
    #                     inspected added lines only.
    #   the hook appears  Without this anchor the comment and blank shapes below would absorb any
    #                     comment-only edit to any package_init.cs in the corpus.
    #   nothing else      Every added line must be the hook, a comment, or blank. One added line
    #                     of real code -- an extra relocated init call, say -- and the file goes
    #                     back to the warning block, hook or no hook.
    function Test-InitTestsHookDrift([string] $path) {
        $hook = [regex]::Escape('init' + $temp + $temp + 'tests')
        $changed = & git -C $repo -c core.safecrlf=false diff --ignore-cr-at-eol -U0 -- $path

        $added = @($changed | Where-Object { $_ -match '^\+' -and $_ -notmatch '^\+\+\+' } | ForEach-Object { $_.Substring(1) })
        $removed = @($changed | Where-Object { $_ -match '^-' -and $_ -notmatch '^---' })

        if ($removed.Count -gt 0) { return $false }
        if (-not ($added | Where-Object { $_ -match $hook })) { return $false }

        foreach ($line in $added) {
            if ($line -notmatch $hook -and $line -notmatch '^\s*//' -and $line -notmatch '^\s*$') { return $false }
        }

        return $true
    }

    $known = @()
    $real = @()

    foreach ($entry in $drift) {
        # numstat row: added <tab> removed <tab> path
        $path = ($entry -split "`t")[-1]

        # A package_init.cs is judged by the derived check ALONE -- the name list has no say over
        # it, in either direction, which is what makes a bank owe this section nothing.
        $isKnown = if ($path -match $initHookPathShape) {
            Test-InitTestsHookDrift $path
        }
        else {
            ($closureFiles -contains $path) -and (Test-ClosureClassDrift $path)
        }

        if ($isKnown) { $known += $entry } else { $real += $entry }
    }

    if ($known) {
        Write-Host ''
        Write-Host "known -tests-closure emission class ($($known.Count) files, documented, not drift):" -ForegroundColor DarkGray
        $known | ForEach-Object { Write-Host "  $_" -ForegroundColor DarkGray }
    }

    if ($real) {
        Write-Host ''
        Write-Host 'CONTENT drift in the corpus after the sweep -- inspect before banking or restoring:' -ForegroundColor Yellow
        $real | ForEach-Object { Write-Host "  $_" -ForegroundColor Yellow }
    }
}

$ErrorActionPreference = $prevEap

if ($hostLimited.Count) {
    Write-Host ''
    Write-Host 'host-limited -- validated, at a count a committed host-limit disclosure accounts for on THIS host:' -ForegroundColor DarkYellow
    $hostLimited | ForEach-Object { Write-Host "  $_" -ForegroundColor DarkYellow }
    Write-Host '  (the banked count stands; it is what a host that can produce the block scores -- see the manifest entry for the retirement condition)' -ForegroundColor DarkGray
}
if ($hostConditionalDisclosed.Count) {
    Write-Host ''
    Write-Host 'host-conditional disclosure fired -- validated, with a named entry moving its verdict from the matched column to the disclosed column on THIS host:' -ForegroundColor DarkYellow
    $hostConditionalDisclosed | ForEach-Object { Write-Host "  $_" -ForegroundColor DarkYellow }
    Write-Host '  (the banked counts stand; they are what the banking host scores -- the roster annotation names the condition, the manifest entry the mechanism)' -ForegroundColor DarkGray
}

if ($oracleFlakedRows.Count) {
    Write-Host ''
    Write-Host 'oracle flaked once -- run 1 diverged ONLY on the oracle side (Go=fail / C#=pass, no converted-side failure); run 2 is what the verdict describes:' -ForegroundColor Magenta
    $oracleFlakedRows | ForEach-Object { Write-Host "  $_" -ForegroundColor Magenta }
    Write-Host "  (run 1's comparison record and results file are preserved under $oracleEvidenceRoot)" -ForegroundColor DarkGray
}

if ($unstable) {
    Write-Host ''
    Write-Host 'oracle unstable on this host -- BOTH runs diverged only on the oracle side; the converted rows passed each time:' -ForegroundColor Red
    $unstableRows | ForEach-Object { Write-Host "  $_" -ForegroundColor Red }
    Write-Host '  (the three-run standard on the oracle side: fail-with, pass-clean, pass-again. Two unreproducible oracle reds' -ForegroundColor DarkGray
    Write-Host '   are a host to qualify -- go test -count=1 <pkg> on this box -- never a corpus regression, and never a green gate.)' -ForegroundColor DarkGray
}

if ($cvac) {
    Write-Host ''
    Write-Host "comparison-validated-at-count -- validated, with no $targetGoos expectation in the roster to bank against:" -ForegroundColor Cyan
    $cvacRows | ForEach-Object { Write-Host "  $_" -ForegroundColor Cyan }
    Write-Host "  (record a row's measured count as a '${targetGoos}: N + D' annotation in docs/ValidatedTestPackages.md to bank it)" -ForegroundColor DarkGray
}

# ---- the hop record ------------------------------------------------------------------------------
# The two hop buckets, reported apart because they are the two halves of what H10 banks: the measured
# counts, and the rows whose measurement is "there is nothing here to measure". Printed before the
# failure list so a reader of a mixed run sees the derivation and then what broke.
if ($hop) {
    Write-Host ''
    Write-Host "hop measured-at-count -- validated, with no expectation at this release to bank against ($targetGoos):" -ForegroundColor Magenta
    $hopRows | ForEach-Object { Write-Host "  $_" -ForegroundColor Magenta }
    Write-Host "  (these counts are H10's input: bank them into docs/ValidatedTestPackages.md as this release's rows)" -ForegroundColor DarkGray
    Write-Host '  (a `receives` note is the relocated predecessor and its 1.23.12 count -- provenance, never a floor)' -ForegroundColor DarkGray
}

if ($hopNoTests) {
    Write-Host ''
    # ${targetGoos}: not $targetGoos: -- a ':' after a BARE variable name is parsed as a scope
    # qualifier, and both editions refuse it ("':' was not followed by a valid variable name
    # character"). The line three below already spelled it correctly, which is what made the defect
    # easy to write and invisible to re-read.
    Write-Host "hop no-eligible-tests -- ran on this host and declared no eligible Go tests for ${targetGoos}:" -ForegroundColor DarkCyan
    $hopNoTestsRows | ForEach-Object { Write-Host "  $_" -ForegroundColor DarkCyan }
    Write-Host "  (each derives a '${targetGoos}: n/a' annotation. Both signals were required: the pipeline's own" -ForegroundColor DarkGray
    Write-Host '   line AND a not-applicable comparison record -- a row that merely produced no counts FAILED instead)' -ForegroundColor DarkGray
}

# ---- the per-row timing file ---------------------------------------------------------------------
# TSV, under the ignored scratchpad, stamped with this run's own start so successive runs accumulate
# rather than overwrite. Written before the exit arms below so a FAILING run still leaves its timings:
# a row's cost is a fact about the row, not about the verdict, and the shard map that needs it is
# usually planned from a run that had failures in it.
$timingDir = Join-Path $repo 'scratchpad/sweep-row-walltimes'
[void](New-Item -ItemType Directory -Force -Path $timingDir)
$modeWord = if ($Hop) { 'hop' } else { 'sweep' }
$timingPath = Join-Path $timingDir ($started.ToString('yyyyMMdd-HHmmss') + '-' + $modeWord + '.tsv')
$timingLines = New-Object System.Collections.Generic.List[string]
[void]$timingLines.Add("# run-validated-sweep per-row wall times")
[void]$timingLines.Add("# started`t$($started.ToString('o'))")
[void]$timingLines.Add("# goos`t$targetGoos")
[void]$timingLines.Add("# mode`t$modeWord")
[void]$timingLines.Add("# test-config`t$sweepConfigLabel")
[void]$timingLines.Add("# rows`t$($rowTimings.Count)")
[void]$timingLines.Add("package`tseconds`tcount")
foreach ($t in $rowTimings) {
    $countField = ''
    if ($null -ne $t.Count) { $countField = [string]$t.Count }
    [void]$timingLines.Add("$($t.Package)`t$($t.Seconds)`t$countField")
}
# UTF8 without a BOM, LF-terminated: the consumer is a generator script, and a BOM would land inside
# its first field. Out-File -Encoding utf8 writes a BOM under 5.1, so the bytes are written directly.
[System.IO.File]::WriteAllText($timingPath, (($timingLines -join "`n") + "`n"), (New-Object System.Text.UTF8Encoding($false)))
Write-Host ''
Write-Host "per-row wall times: $($rowTimings.Count) row(s) -> $timingPath" -ForegroundColor DarkGray

if ($fail) {
    Write-Host ''
    Write-Host 'failed:' -ForegroundColor Red
    $failed | ForEach-Object { Write-Host "  $_" -ForegroundColor Red }
    exit 1
}

# An oracle-unstable row is not a green gate either, for the same reason and by the same shape: the
# row was NOT measured on this host, and an unmeasured row must never read as a pass. It is counted
# and reported apart from $fail because what it names is different -- the reference implementation
# on this box, not the corpus.
if ($unstable) { exit 1 }

# An unbanked count is not a green gate: it exits non-zero exactly as it did before this dimension
# existed, only now it is reported as itself rather than as a count failure.
#
# ⚠ EXCEPT on a hop run, and this is the cut's one departure from the ruling's literal wording -- see
# $hop's declaration for the full reasoning, and the landing post, which states it rather than burying
# it. In short: that sentence is right because this script is a GATE, and every row of a hop run is
# unbanked BY CONSTRUCTION, so inheriting the arm would make -Hop exit 1 on every run including a
# perfect one -- an exit code that cannot vary cannot report anything. A hop run is a derivation, and
# its failure conditions are $fail and $unstable above, both of which still exit 1 here. Reverting this
# is deleting `-and -not $Hop`.
if ($cvac -and -not $Hop) { exit 1 }

# $hop and $hopNoTests deliberately do NOT appear in any exit arm: recording a count where nothing is
# banked, and recording that a package has no eligible tests here, are what -Hop was asked to do. A
# mode whose successful completion exits non-zero would train its caller to ignore the code.
exit 0
