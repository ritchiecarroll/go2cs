<#
.SYNOPSIS
    Guards the validated-package roster's machine-parsed format and its arithmetic.

.DESCRIPTION
    Three things this checks, all cheap enough to run at any time (pure text, no build, no gate):

      1. THE PARSER'S CONTRACT, against fixture rows -- the columns, the host-conditional
         annotation, and the per-OS annotation ruled on 2026-08-22, including the shapes that must
         NOT parse as one. The parser lives in `_roster.ps1` and is what `run-validated-sweep.ps1`
         reads the roster with, so a defect here moves a gate's verdict; it is guarded where the
         parsing lives rather than where the sweep runs.

      2. THE ROSTER'S OWN ARITHMETIC, derived from the table every time -- the progress header's
         package count, verdict sum and disclosed sum against the columns, the Linux progress
         line against the per-OS annotations, and the implementable-set line against the exclusion
         ledger (excluded count, denominator and percentage all recomputed, every ledger class one
         of the ruled ones). Nothing is hand-listed here: a hand-maintained roster
         mirror is the exact debt the sweep's own drift section records going unpaid twice, so
         every number this asserts is computed from the table it is asserting about.

      3. THE FRONT PAGE AGREES WITH THE ROSTER -- docs/README.md's featured NEWS block restates
         five of the header's figures in prose, and until 2026-09-07 nothing compared them, so it
         sat stale across at least three banks. Section 2e asserts each against the header, which
         stays the authority; the roster is never re-derived from the README.

        ./check-roster-format.ps1            # the guard
        ./check-roster-format.ps1 -List      # also print every per-OS annotation the roster carries

.NOTES
    Requires PowerShell 5.1 (Windows) or PowerShell 7+ (any platform). Exit 0 clean, 1 on any
    violation. No non-ASCII literal: the roster's separator glyphs are spelled by code point.
#>
[CmdletBinding()]
param(
    [switch] $List
)

$ErrorActionPreference = 'Stop'

. (Join-Path $PSScriptRoot '_roster.ps1')

$repo = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
$table = Join-Path $repo 'docs/ValidatedTestPackages.md'

$dot = [string][char]0x00B7      # U+00B7 MIDDLE DOT -- the cell-segment separator
$dash = [string][char]0x2014     # U+2014 EM DASH -- the header's clause separator
$minus = [string][char]0x2212    # U+2212 MINUS SIGN -- the implementable line's subtraction sign

$failures = New-Object System.Collections.Generic.List[string]
$checks = 0

function Assert-Equal {
    param([string] $What, $Expected, $Actual)

    $script:checks++
    if ("$Expected" -ne "$Actual") {
        [void]$script:failures.Add("$What -- expected '$Expected', got '$Actual'")
    }
}

function Assert-Throws {
    param([string] $What, [scriptblock] $Body, [string] $Fragment)

    $script:checks++
    try {
        & $Body | Out-Null
        [void]$script:failures.Add("$What -- expected a throw naming '$Fragment', nothing was thrown")
    }
    catch {
        if ("$_" -notmatch [regex]::Escape($Fragment)) {
            [void]$script:failures.Add("$What -- expected a throw naming '$Fragment', got '$_'")
        }
    }
}

# Writes fixture rows to a uniquely-named temp roster and parses it. Unique per call so two
# concurrent runs (a lane and a sibling worktree) cannot collide on one another's fixture.
function Read-FixtureRoster {
    param([string[]] $Rows)

    $header = @(
        '| Package | Tests | Disclosed | What it exercises |'
        '|:--|:--:|:--:|:--|'
    )
    $path = Join-Path ([System.IO.Path]::GetTempPath()) ('go2cs-roster-fixture-' + [guid]::NewGuid().ToString('n') + '.md')

    try {
        [System.IO.File]::WriteAllText($path, (($header + $Rows) -join "`r`n"), (New-Object System.Text.UTF8Encoding($false)))
        return @(Get-ValidatedRosterRows -Path $path)
    }
    finally {
        if (Test-Path $path) { Remove-Item $path -Force }
    }
}

# The exclusion-ledger sibling of Read-FixtureRoster: same unique temp file, the ledger's header.
# The SECTION HEADING is part of the fixture because it is part of the contract -- the parser is
# scoped to the ledger's own section, so a fixture without one is not a ledger. $Trailing appends
# lines AFTER the table, which is how the out-of-section arms plant a row somewhere else.
function Read-FixtureLedger {
    param([string[]] $Rows, [string[]] $Trailing = @(), [string[]] $Prefix = @('## Excluded packages', ''))

    $header = $Prefix + @(
        '| Package | Verdicts | Class | Mechanism | Rooting |'
        '|:--|:--:|:--:|:--|:--:|'
    )
    $path = Join-Path ([System.IO.Path]::GetTempPath()) ('go2cs-ledger-fixture-' + [guid]::NewGuid().ToString('n') + '.md')

    try {
        [System.IO.File]::WriteAllText($path, (($header + $Rows + $Trailing) -join "`r`n"), (New-Object System.Text.UTF8Encoding($false)))
        return @(Get-ExclusionLedgerRows -Path $path)
    }
    finally {
        if (Test-Path $path) { Remove-Item $path -Force }
    }
}

Write-Host 'roster format guard' -ForegroundColor Cyan

# ---- 1. the parser's contract, against fixtures --------------------------------------------------
$fixtureRows = @(
    "| [``plain/pkg``](https://x/plain) | 12 |  | Nothing special. $dot [proof](p.md) |"
    "| [``disc/pkg``](https://x/disc) | 12 | 3 | Has disclosures. $dot [proof](p.md) |"
    "| [``cond/pkg``](https://x/cond) | 61 |  | Path algebra $dot host-conditional (privilege $dash colon-free): ``TestA/one``, ``TestB/two`` $dot linux: 54 $dot [proof](p.md) |"
    "| [``ann/pkg``](https://x/ann) | 298 |  | Random ints. $dot linux: 302 $dot [proof](p.md) |"
    "| [``annd/pkg``](https://x/annd) | 17 | 1 | Mime tables. $dot linux: 18 + 1 $dot [proof](p.md) |"
    "| [``hcd/pkg``](https://x/hcd) | 88 | 1 | Processes. $dot host-conditional-disclosure (published-host descriptor count): ``TestExtraFiles`` $dot linux: 87 + 1 $dot [proof](p.md) |"
    "| [``dar/pkg``](https://x/dar) | 5 |  | Mac things. $dot darwin: 7 $dot [proof](p.md) |"
    "| [``prose/pkg``](https://x/prose) | 9 |  | Behavior on linux: 5 subtests skip. $dot [proof](p.md) |"
    "| [``segment/pkg``](https://x/segment) | 9 |  | Counted here $dot linux: 5 subtests skip $dot [proof](p.md) |"
    "| [``tail/pkg``](https://x/tail) | 4 |  | Ends on the annotation $dot linux: 6 |"
    "| [``winonly/pkg``](https://x/winonly) | 21 |  | Registry things. $dot linux: n/a $dot [proof](p.md) |"
    "| [``naprose/pkg``](https://x/naprose) | 3 |  | Not applicable prose here $dot linux: n/a maybe someday $dot [proof](p.md) |"
    "| [``exec/pkg``](https://x/exec) | 4 |  | Weak pointers. $dot execution: release-tc0 $dot [proof](p.md) |"
    "| [``execann/pkg``](https://x/execann) | 4 | 2 | Both annotations. $dot execution: release-tc0 $dot linux: 5 + 1 $dot [proof](p.md) |"
    "| [``execprose/pkg``](https://x/execprose) | 8 |  | Says the word $dot execution: release-tc0 is what it needs $dot [proof](p.md) |"
)

$fixture = Read-FixtureRoster $fixtureRows
$byName = @{}
foreach ($row in $fixture) { $byName[$row.Package] = $row }

Assert-Equal 'fixture: every row parses' 15 $fixture.Count

Assert-Equal 'columns: matched count' 12 $byName['plain/pkg'].Expected
Assert-Equal 'columns: blank disclosed reads 0' 0 $byName['plain/pkg'].Disclosed
Assert-Equal 'columns: disclosed count' 3 $byName['disc/pkg'].Disclosed
Assert-Equal 'columns: a plain row carries no annotation' 0 $byName['plain/pkg'].OS.Count

# The host-conditional annotation must survive an OS annotation following it in the same cell --
# its capture stops at the last backticked name, and the per-OS segment starts after that.
Assert-Equal 'host-conditional: names still parse beside an OS annotation' 'TestA/one,TestB/two' ($byName['cond/pkg'].Conditional -join ',')
Assert-Equal 'host-conditional: the row also carries its OS annotation' 54 $byName['cond/pkg'].OS['linux'].Expected

# The host-conditional DISCLOSURE annotation (Q31) parses into its own list, and the two annotations
# never cross-match: `host-conditional-disclosure` is not a `host-conditional` surplus list and a
# surplus list is not a disclosure list.
Assert-Equal 'host-conditional-disclosure: names parse' 'TestExtraFiles' ($byName['hcd/pkg'].ConditionalDisclosures -join ',')
Assert-Equal 'host-conditional-disclosure: does not read as a surplus annotation' 0 @($byName['hcd/pkg'].Conditional).Count
Assert-Equal 'host-conditional-disclosure: the surplus row carries no disclosure list' 0 @($byName['cond/pkg'].ConditionalDisclosures).Count
Assert-Equal 'host-conditional-disclosure: the OS annotation beside it still parses (floor)' 87 $byName['hcd/pkg'].OS['linux'].Expected
Assert-Equal 'host-conditional-disclosure: the OS annotation beside it still parses (disclosed)' 1 $byName['hcd/pkg'].OS['linux'].Disclosed

Assert-Equal 'annotation: N alone' 302 $byName['ann/pkg'].OS['linux'].Expected
Assert-Equal 'annotation: N alone means zero disclosed' 0 $byName['ann/pkg'].OS['linux'].Disclosed
Assert-Equal 'annotation: N + D matched' 18 $byName['annd/pkg'].OS['linux'].Expected
Assert-Equal 'annotation: N + D disclosed' 1 $byName['annd/pkg'].OS['linux'].Disclosed
Assert-Equal 'annotation: darwin is a valid key' 7 $byName['dar/pkg'].OS['darwin'].Expected
Assert-Equal 'annotation: it is the last segment, terminating pipe included' 6 $byName['tail/pkg'].OS['linux'].Expected

# The two ways prose must NOT read as an annotation: no separator before it, and a segment that
# continues into words after the number.
Assert-Equal 'prose: unseparated "linux: 5" is not an annotation' 0 $byName['prose/pkg'].OS.Count
Assert-Equal 'prose: a segment continuing past the number is not an annotation' 0 $byName['segment/pkg'].OS.Count

# The permanently-inapplicable form (ruled 2026-08-29): `linux: n/a` parses as Applicable=$false
# with null counts, and its prose-immunity mirrors the numeric form's.
Assert-Equal 'n/a: the annotation parses' $true $byName['winonly/pkg'].OS.ContainsKey('linux')
Assert-Equal 'n/a: it is inapplicable, not a count' $false $byName['winonly/pkg'].OS['linux'].Applicable
Assert-Equal 'n/a: expected is null, never a number' $true ($null -eq $byName['winonly/pkg'].OS['linux'].Expected)
Assert-Equal 'n/a: a numeric annotation is applicable' $true $byName['ann/pkg'].OS['linux'].Applicable
Assert-Equal 'n/a prose: a segment continuing past n/a is not an annotation' 0 $byName['naprose/pkg'].OS.Count

# The columns ARE the Windows expectation, so a windows-keyed annotation is a contradiction, and an
# unknown key is a typo the sweep must not silently drop.
Assert-Throws 'annotation: a windows key is refused by name' {
    Read-FixtureRoster @("| [``w/pkg``](https://x/w) | 3 |  | Two Windows answers. $dot windows: 4 $dot [proof](p.md) |")
} "carries a 'windows:' per-OS annotation"
Assert-Throws 'annotation: an unknown key is refused by name' {
    Read-FixtureRoster @("| [``p/pkg``](https://x/p) | 3 |  | Not a corpus flavor. $dot plan9: 4 $dot [proof](p.md) |")
} 'unknown per-OS annotation key'
Assert-Throws 'annotation: a repeated key is refused' {
    Read-FixtureRoster @("| [``r/pkg``](https://x/r) | 3 |  | Twice. $dot linux: 4 $dot linux: 5 $dot [proof](p.md) |")
} 'more than one'
Assert-Throws 'n/a: windows: n/a is refused by name (no back door)' {
    Read-FixtureRoster @("| [``wna/pkg``](https://x/wna) | 3 |  | Contradiction. $dot windows: n/a $dot [proof](p.md) |")
} "carries a 'windows:' per-OS annotation"
Assert-Throws 'n/a: a numeric and an n/a annotation for one key is refused' {
    Read-FixtureRoster @("| [``rna/pkg``](https://x/rna) | 3 |  | Two answers. $dot linux: 4 $dot linux: n/a $dot [proof](p.md) |")
} 'more than one'

# Expectation resolution: the annotation answers on its own OS, the columns everywhere else --
# including on Windows, where an annotation must never displace the banked columns.
$annotated = $byName['annd/pkg']
$plain = $byName['plain/pkg']

Assert-Equal 'expectation: annotated row under its own OS' 18 (Get-RosterRowExpectation -Row $annotated -Goos 'linux').Expected
Assert-Equal 'expectation: annotated row under its own OS, disclosed' 1 (Get-RosterRowExpectation -Row $annotated -Goos 'linux').Disclosed
Assert-Equal 'expectation: annotated row names its source' 'linux' (Get-RosterRowExpectation -Row $annotated -Goos 'linux').Source
Assert-Equal 'expectation: annotated row on Windows still reads the columns' 17 (Get-RosterRowExpectation -Row $annotated -Goos 'windows').Expected
Assert-Equal 'expectation: annotated row on Windows names the columns' 'columns' (Get-RosterRowExpectation -Row $annotated -Goos 'windows').Source
Assert-Equal 'expectation: unannotated row falls back to the columns' 12 (Get-RosterRowExpectation -Row $plain -Goos 'linux').Expected
Assert-Equal 'expectation: unannotated row names the columns' 'columns' (Get-RosterRowExpectation -Row $plain -Goos 'linux').Source
Assert-Equal 'expectation: a different OS does not read another OS annotation' 'columns' (Get-RosterRowExpectation -Row $annotated -Goos 'darwin').Source

# ---- 1a. the per-row EXECUTION annotation (owner ruling 2026-08-30, Option A) ---------------------
# The annotation names the local execution CONFIG a row's pipeline leg runs under -- an execution
# property, never a platform one. The load-bearing assertions here are the NEGATIVE ones: an
# unannotated row must carry no config and produce an EMPTY argument list, because "nothing changes
# for a row that did not opt in" is the whole ruling and this is where it is provable without
# running the gate.
Assert-Equal 'execution: the annotation parses' 'release-tc0' $byName['exec/pkg'].Execution
Assert-Equal 'execution: an unannotated row carries none' $true ($null -eq $byName['plain/pkg'].Execution)
Assert-Equal 'execution: it coexists with a per-OS annotation' 'release-tc0' $byName['execann/pkg'].Execution
Assert-Equal 'execution: the per-OS annotation beside it still parses' 5 $byName['execann/pkg'].OS['linux'].Expected
Assert-Equal 'execution: the per-OS disclosed half beside it still parses' 1 $byName['execann/pkg'].OS['linux'].Disclosed
Assert-Equal 'execution: it is not a per-OS annotation and mints no OS key' 0 $byName['exec/pkg'].OS.Count
Assert-Equal 'execution: the columns are untouched by it' 4 $byName['exec/pkg'].Expected

# Prose immunity, the same both-ends anchoring the per-OS forms rely on: a segment that continues
# into words after the config name is a sentence, not an annotation.
Assert-Equal 'execution prose: a segment continuing past the config is not an annotation' $true `
    ($null -eq $byName['execprose/pkg'].Execution)

# A config the mapping does not know is refused BY NAME rather than degraded to the default path --
# a silently-ignored config reads to its author as opted in while running the default, which is the
# exact failure the annotation exists to prevent.
Assert-Throws 'execution: an unknown config is refused by name' {
    Read-FixtureRoster @("| [``x/pkg``](https://x/x) | 3 |  | Typo. $dot execution: release $dot [proof](p.md) |")
} 'unknown execution annotation'
Assert-Throws 'execution: a repeated annotation is refused' {
    Read-FixtureRoster @("| [``y/pkg``](https://x/y) | 3 |  | Twice. $dot execution: release-tc0 $dot execution: release-tc0 $dot [proof](p.md) |")
} 'more than one execution annotation'

# The config -> converter-argument mapping, which is what actually moves a gate's invocation. The
# empty case is asserted first and hardest: it is the "nothing changes" guarantee in one line.
Assert-Equal 'execution args: no config contributes nothing' 0 (@(Get-RosterExecutionArgs $null)).Count
Assert-Equal 'execution args: an empty config contributes nothing' 0 (@(Get-RosterExecutionArgs '')).Count
# These assert the CURRENT converter flags. They were briefly wrong -- asserting the retired
# `-test-release-tc0` while _roster.ps1 already emitted `-test-config Release` -- and were corrected
# in train 12; the lesson kept here is that a mapping guard has to name the flags the converter
# actually parses, or it guards the wrong thing in the one direction that matters.
Assert-Equal 'execution args: release-tc0 maps to the converter flag' '-test-config Release' `
    ((@(Get-RosterExecutionArgs 'release-tc0')) -join ' ')
Assert-Equal 'execution args: release-tc0 contributes exactly two arguments' 2 (@(Get-RosterExecutionArgs 'release-tc0')).Count
# The opt-OUT mirror (2026-09-02, the Release+TC0 default flip). It states its WHOLE configuration --
# -test-config Release AND -test-tiered -- rather than leaning on the converter's default being
# Release, so the annotation cannot change meaning if a default moves again.
Assert-Equal 'execution args: release-tiered maps to the converter flags' '-test-config Release -test-tiered' `
    ((@(Get-RosterExecutionArgs 'release-tiered')) -join ' ')
Assert-Equal 'execution args: release-tiered contributes exactly three arguments' 3 (@(Get-RosterExecutionArgs 'release-tiered')).Count
Assert-Equal 'execution values: both configs are known to the roster vocabulary' 'release-tc0, release-tiered' `
    (($RosterExecutionValues | Sort-Object) -join ', ')
Assert-Throws 'execution args: an unknown config throws rather than running the default path' {
    Get-RosterExecutionArgs 'no-such-config'
} 'Unknown execution config'

# ---- 1b. the sweep's classification rule ---------------------------------------------------------
# The rule the sweep reports from, exercised without running the gate. The WINDOWS rows come first
# and matter most: they are the proof that the reachable classes on Windows are exactly the three
# that existed before the OS dimension did.
$winPlain = Get-RosterRowExpectation -Row $plain -Goos 'windows'          # columns 12 / 0
$winAnnotated = Get-RosterRowExpectation -Row $annotated -Goos 'windows'  # columns 17 / 1
$linAnnotated = Get-RosterRowExpectation -Row $annotated -Goos 'linux'    # annotation 18 / 1
$linPlain = Get-RosterRowExpectation -Row $plain -Goos 'linux'            # columns 12 / 0

Assert-Equal 'windows: a row at its banked count passes' 'pass' `
    (Get-SweepRowClassification -Expectation $winPlain -Got 12 -GotDisclosed 0 -TargetGoos 'windows')
Assert-Equal 'windows: a row off its banked count is a count failure' 'count' `
    (Get-SweepRowClassification -Expectation $winPlain -Got 13 -GotDisclosed 0 -TargetGoos 'windows')
Assert-Equal 'windows: a proven host-conditional surplus passes' 'host-conditional' `
    (Get-SweepRowClassification -Expectation $winPlain -Got 18 -GotDisclosed 0 -TargetGoos 'windows' -HostConditionalAccepted)
Assert-Equal 'windows: the disclosed count is NOT re-enforced on the columns path' 'pass' `
    (Get-SweepRowClassification -Expectation $winAnnotated -Got 17 -GotDisclosed 4 -TargetGoos 'windows')
Assert-Equal 'windows: an annotated row is still judged by its columns' 'count' `
    (Get-SweepRowClassification -Expectation $winAnnotated -Got 18 -GotDisclosed 1 -TargetGoos 'windows')
Assert-Equal 'windows: comparison-validated-at-count is unreachable' 'count' `
    (Get-SweepRowClassification -Expectation $winPlain -Got 99 -GotDisclosed 0 -TargetGoos 'windows')

Assert-Equal 'linux: an annotated row passes at its linux count' 'pass' `
    (Get-SweepRowClassification -Expectation $linAnnotated -Got 18 -GotDisclosed 1 -TargetGoos 'linux')
Assert-Equal 'linux: an annotated row off its linux count is a count failure' 'count' `
    (Get-SweepRowClassification -Expectation $linAnnotated -Got 17 -GotDisclosed 1 -TargetGoos 'linux')
Assert-Equal 'linux: an annotated row whose disclosures moved is named as that' 'disclosed-moved' `
    (Get-SweepRowClassification -Expectation $linAnnotated -Got 18 -GotDisclosed 0 -TargetGoos 'linux')
Assert-Equal 'linux: a moved disclosure is never absorbed as host-conditional' 'disclosed-moved' `
    (Get-SweepRowClassification -Expectation $linAnnotated -Got 19 -GotDisclosed 0 -TargetGoos 'linux' -HostConditionalAccepted)
$linHcd = Get-RosterRowExpectation -Row $byName['hcd/pkg'] -Goos 'linux'          # annotation 87 / 1
Assert-Equal 'linux: the host-conditional-disclosure row passes at its banking-host reading' 'pass' `
    (Get-SweepRowClassification -Expectation $linHcd -Got 87 -GotDisclosed 1 -TargetGoos 'linux')
Assert-Equal 'linux: the fired reading is disclosed-moved until PROVEN' 'disclosed-moved' `
    (Get-SweepRowClassification -Expectation $linHcd -Got 86 -GotDisclosed 2 -TargetGoos 'linux')
Assert-Equal 'linux: the PROVEN fired reading is its own class' 'host-conditional-disclosure' `
    (Get-SweepRowClassification -Expectation $linHcd -Got 86 -GotDisclosed 2 -TargetGoos 'linux' -HostConditionalDisclosureAccepted)
Assert-Equal 'linux: an unannotated row still passes at the windows count' 'pass' `
    (Get-SweepRowClassification -Expectation $linPlain -Got 12 -GotDisclosed 0 -TargetGoos 'linux')
Assert-Equal 'linux: an unannotated row off the windows count is comparison-validated-at-count' 'unbanked-count' `
    (Get-SweepRowClassification -Expectation $linPlain -Got 14 -GotDisclosed 0 -TargetGoos 'linux')
Assert-Equal 'linux: a lost verdict on an unannotated row is also unbanked, never a silent pass' 'unbanked-count' `
    (Get-SweepRowClassification -Expectation $linPlain -Got 1 -GotDisclosed 0 -TargetGoos 'linux')

# The n/a row end to end: inapplicable on its annotated OS at ANY count, columns as ever on Windows.
$linNa = Get-RosterRowExpectation -Row $byName['winonly/pkg'] -Goos 'linux'
Assert-Equal 'n/a expectation: inapplicable and named' $false $linNa.Applicable
Assert-Equal 'n/a expectation: source is the annotation' 'linux' $linNa.Source
Assert-Equal 'n/a classification: not-applicable at any count' 'not-applicable' `
    (Get-SweepRowClassification -Expectation $linNa -Got 0 -GotDisclosed 0 -TargetGoos 'linux')
Assert-Equal 'n/a classification: not-applicable even at a plausible count' 'not-applicable' `
    (Get-SweepRowClassification -Expectation $linNa -Got 21 -GotDisclosed 0 -TargetGoos 'linux')
Assert-Equal 'n/a on Windows: the columns answer exactly as before' 'pass' `
    (Get-SweepRowClassification -Expectation (Get-RosterRowExpectation -Row $byName['winonly/pkg'] -Goos 'windows') -Got 21 -GotDisclosed 0 -TargetGoos 'windows')

Assert-Equal 'windows: a proven capability-absent shortfall passes' 'capability-absent' `
    (Get-SweepRowClassification -Expectation $winPlain -Got 6 -GotDisclosed 0 -TargetGoos 'windows' -CapabilityAbsentAccepted)
Assert-Equal 'linux: a moved disclosure is never absorbed as capability-absent either' 'disclosed-moved' `
    (Get-SweepRowClassification -Expectation $linAnnotated -Got 12 -GotDisclosed 0 -TargetGoos 'linux' -CapabilityAbsentAccepted)

Assert-Equal 'windows: a proven host-limited shortfall passes as its own class' 'host-limit' `
    (Get-SweepRowClassification -Expectation $winPlain -Got 6 -GotDisclosed 0 -TargetGoos 'windows' -HostLimitAccepted)
Assert-Equal 'linux: a moved disclosure is never absorbed as host-limited either' 'disclosed-moved' `
    (Get-SweepRowClassification -Expectation $linAnnotated -Got 12 -GotDisclosed 0 -TargetGoos 'linux' -HostLimitAccepted)
# It is a THIRD bucket, never a re-spelling of the second: a caller that proved neither still gets
# the same hard count failure, and the two switches never collapse into one another.
Assert-Equal 'windows: an unproven shortfall is still a count failure with the new bucket present' 'count' `
    (Get-SweepRowClassification -Expectation $winPlain -Got 6 -GotDisclosed 0 -TargetGoos 'windows')

# ---- 1b2. the capability-absent mirror check, exercised end to end ------------------------------
# Test-CapabilityAbsentDelta/Get-CapabilityAbsentVerdict have no roster-row fixture of their own
# (they read a comparison record and a committed proof page, not a table row), so this proves the
# PURE function directly with a synthetic block and verdict maps -- the same evidence shape a real
# sweep run hands it, built by hand instead of by a pipeline. Six sub-tests spawn under a
# three-verdict block for a small, readable fixture; the real crypto/tls block is 3,243.
$block = [PSCustomObject]@{ Test = 'TestFakeSuite'; BlockSize = 3 }
$fullBankedNames = @('TestOther', 'TestFakeSuite', 'TestFakeSuite/case1', 'TestFakeSuite/case2')
# Fixture verdict maps are built in the shape ConvertFrom-ComparisonRecord actually produces --
# ordinal dictionaries, not PSCustomObjects (a PSObject cannot even hold the case-only verdict-name
# pairs a legal record may carry; see the reader's own fixture below).
function New-VerdictMap([hashtable] $Verdicts) {
    $map = New-Object 'System.Collections.Generic.Dictionary[string,string]' ([System.StringComparer]::Ordinal)
    foreach ($name in $Verdicts.Keys) { $map.Add([string]$name, [string]$Verdicts[$name]) }
    return , $map
}
$fullComparison = [PSCustomObject]@{
    go = New-VerdictMap @{ TestOther = 'pass'; TestFakeSuite = 'pass'; 'TestFakeSuite/case1' = 'pass'; 'TestFakeSuite/case2' = 'skip' }
    csharp = New-VerdictMap @{ TestOther = 'pass'; TestFakeSuite = 'pass'; 'TestFakeSuite/case1' = 'pass'; 'TestFakeSuite/case2' = 'skip' }
}
$absentComparison = [PSCustomObject]@{
    go = New-VerdictMap @{ TestOther = 'pass'; TestFakeSuite = 'skip' }
    csharp = New-VerdictMap @{ TestOther = 'pass'; TestFakeSuite = 'skip' }
}
# The shape a REAL capability-less host produces, measured 2026-08-28: the block root FAILS on both
# runtimes (Go's own oracle t.Fatal's -- crypto/tls's TestBogoSuite has no capability-absent skip
# branch at all), and the converter accounts a host-conditionally annotated root as DISCLOSED in
# exactly that shape, so the live disclosed count is the banked one PLUS the root.
$absentFailComparison = [PSCustomObject]@{
    go = New-VerdictMap @{ TestOther = 'pass'; TestFakeSuite = 'fail' }
    csharp = New-VerdictMap @{ TestOther = 'pass'; TestFakeSuite = 'fail' }
    disclosed = @('TestFakeSuite (host-limit): the runner outruns its own deadline')
}

# A SKIP root is an agreed match and STAYS in the matched count, so its collapse loses BlockSize - 1
# verdicts (Got 2 here), where a FAIL root is disclosed and loses the whole block (Got 1). Until
# 2026-09-26 the SKIP fixtures below used Got 1 -- a shape no real run produces (os measured it:
# 1103 = 1105 - 2 against a three-verdict block) -- which is why the rule never fired for os.
Assert-Equal 'capability-absent: the clean SKIP collapse (root stays matched, shortfall BlockSize - 1) is accepted' $true `
    (Test-CapabilityAbsentDelta -Expected 4 -Disclosed 0 -Block $block -Got 2 -Comparison $absentComparison -BankedNames $fullBankedNames).Accepted
Assert-Equal 'capability-absent: a SKIP root at the FAIL shortfall (BlockSize) is refused -- keyed, never loosened' $false `
    (Test-CapabilityAbsentDelta -Expected 4 -Disclosed 0 -Block $block -Got 1 -Comparison $absentComparison -BankedNames $fullBankedNames).Accepted
Assert-Equal 'capability-absent: a FAIL root at the SKIP shortfall (BlockSize - 1) is refused -- keyed, never loosened' $false `
    (Test-CapabilityAbsentDelta -Expected 4 -Disclosed 0 -Block $block -Got 2 -Comparison $absentFailComparison -BankedNames $fullBankedNames).Accepted
Assert-Equal 'capability-absent: the MEASURED collapse -- agreeing FAIL with the root disclosed -- is accepted' $true `
    (Test-CapabilityAbsentDelta -Expected 4 -Disclosed 0 -Block $block -Got 1 -Comparison $absentFailComparison -BankedNames $fullBankedNames).Accepted
Assert-Equal 'capability-absent: an agreeing FAIL whose extra disclosure is some OTHER row is refused' $false `
    (Test-CapabilityAbsentDelta -Expected 4 -Disclosed 0 -Block $block -Got 1 -Comparison ([PSCustomObject]@{
        go = New-VerdictMap @{ TestOther = 'pass'; TestFakeSuite = 'fail' }
        csharp = New-VerdictMap @{ TestOther = 'pass'; TestFakeSuite = 'fail' }
        disclosed = @('TestSomethingElse (alloc-profile): unrelated')
    }) -BankedNames $fullBankedNames).Accepted
Assert-Equal 'capability-absent: an agreeing FAIL that discloses nothing at all is refused' $false `
    (Test-CapabilityAbsentDelta -Expected 4 -Disclosed 0 -Block $block -Got 1 -Comparison ([PSCustomObject]@{
        go = New-VerdictMap @{ TestOther = 'pass'; TestFakeSuite = 'fail' }
        csharp = New-VerdictMap @{ TestOther = 'pass'; TestFakeSuite = 'fail' }
    }) -BankedNames $fullBankedNames).Accepted
Assert-Equal 'capability-absent: an agreeing SKIP that nonetheless discloses the root is refused' $false `
    (Test-CapabilityAbsentDelta -Expected 4 -Disclosed 0 -Block $block -Got 2 -Comparison ([PSCustomObject]@{
        go = New-VerdictMap @{ TestOther = 'pass'; TestFakeSuite = 'skip' }
        csharp = New-VerdictMap @{ TestOther = 'pass'; TestFakeSuite = 'skip' }
        disclosed = @('TestFakeSuite (host-limit): the runner outruns its own deadline')
    }) -BankedNames $fullBankedNames).Accepted
# THE control that keeps a capable-but-slow host red. Identical shortfall, identical 1 matched,
# identical absent fan-out -- and Go PASSED, so the matrix was established and the loss is the
# converted side's alone. This is the i7-5820K's real crypto/tls shape; absorbing it would convert a
# measured divergence into a green.
Assert-Equal 'capability-absent: Go pass / C# fail (capability PRESENT, converted side missed it) is refused' $false `
    (Test-CapabilityAbsentDelta -Expected 4 -Disclosed 0 -Block $block -Got 1 -Comparison ([PSCustomObject]@{
        go = New-VerdictMap @{ TestOther = 'pass'; TestFakeSuite = 'pass' }
        csharp = New-VerdictMap @{ TestOther = 'pass'; TestFakeSuite = 'fail' }
        disclosed = @('TestFakeSuite (host-limit): the runner outruns its own deadline')
    }) -BankedNames $fullBankedNames).Accepted
# The control the i7-5820K's real crypto/tls run produced on 2026-08-28, and the one every count
# above fails to tell apart: fail/fail, one collapsed root, the disclosed count exactly where an
# absent capability would put it -- and the capability was PRESENT, Go's flaky handful of failures
# inside a matrix it fully fanned out. The withdrawn rows are the only evidence that says so.
Assert-Equal 'capability-absent: an agreeing FAIL whose Go side DID fan out (rows withdrawn) is refused' $false `
    (Test-CapabilityAbsentDelta -Expected 4 -Disclosed 0 -Block $block -Got 1 -Comparison ([PSCustomObject]@{
        go = New-VerdictMap @{ TestOther = 'pass'; TestFakeSuite = 'fail' }
        csharp = New-VerdictMap @{ TestOther = 'pass'; TestFakeSuite = 'fail' }
        disclosed = @('TestFakeSuite (host-limit): the runner outruns its own deadline')
        withdrawn = @('TestFakeSuite/case1', 'TestFakeSuite/case2')
    }) -BankedNames $fullBankedNames).Accepted
# ...and a withdrawal that belongs to some OTHER disclosed root says nothing about this block.
Assert-Equal 'capability-absent: a withdrawal outside the block does not disqualify the collapse' $true `
    (Test-CapabilityAbsentDelta -Expected 4 -Disclosed 0 -Block $block -Got 1 -Comparison ([PSCustomObject]@{
        go = New-VerdictMap @{ TestOther = 'pass'; TestFakeSuite = 'fail' }
        csharp = New-VerdictMap @{ TestOther = 'pass'; TestFakeSuite = 'fail' }
        disclosed = @('TestFakeSuite (host-limit): the runner outruns its own deadline')
        withdrawn = @('TestSomethingElse/case1')
    }) -BankedNames $fullBankedNames).Accepted
Assert-Equal 'capability-absent: a shortfall that is not the registered block size is refused' $false `
    (Test-CapabilityAbsentDelta -Expected 4 -Disclosed 0 -Block $block -Got 3 -Comparison $absentComparison -BankedNames $fullBankedNames).Accepted
Assert-Equal 'capability-absent: a surplus (the surplus mechanism''s job, not this one''s) is refused' $false `
    (Test-CapabilityAbsentDelta -Expected 4 -Disclosed 0 -Block $block -Got 5 -Comparison $fullComparison -BankedNames $fullBankedNames).Accepted
Assert-Equal 'capability-absent: a subtest surviving alongside the collapse is refused, not absorbed' $false `
    (Test-CapabilityAbsentDelta -Expected 4 -Disclosed 0 -Block $block -Got 2 -Comparison ([PSCustomObject]@{
        go = New-VerdictMap @{ TestOther = 'pass'; TestFakeSuite = 'skip'; 'TestFakeSuite/case1' = 'skip' }
        csharp = New-VerdictMap @{ TestOther = 'pass'; TestFakeSuite = 'skip'; 'TestFakeSuite/case1' = 'skip' }
    }) -BankedNames $fullBankedNames).Accepted
Assert-Equal 'capability-absent: the top-level test agreeing on PASS instead of SKIP is refused' $false `
    (Test-CapabilityAbsentDelta -Expected 4 -Disclosed 0 -Block $block -Got 1 -Comparison ([PSCustomObject]@{
        go = New-VerdictMap @{ TestOther = 'pass'; TestFakeSuite = 'pass' }
        csharp = New-VerdictMap @{ TestOther = 'pass'; TestFakeSuite = 'pass' }
    }) -BankedNames $fullBankedNames).Accepted
# A disclosed count that moved for an unrelated reason. The banked shape here is 4 matched + 1
# disclosed (TestPinned), the collapse is the clean skip -- so the expected live count is that same
# 1, and a second disclosure means something OTHER than the capability moved.
Assert-Equal 'capability-absent: a moved disclosed count is refused, not a capability shape' $false `
    (Test-CapabilityAbsentDelta -Expected 4 -Disclosed 1 -Block $block -Got 2 -Comparison ([PSCustomObject]@{
        go = New-VerdictMap @{ TestOther = 'pass'; TestPinned = 'pass'; TestFakeSuite = 'skip' }
        csharp = New-VerdictMap @{ TestOther = 'pass'; TestPinned = 'fail'; TestFakeSuite = 'skip' }
        disclosed = @('TestPinned (alloc-profile): x', 'TestOther (alloc-profile): y')
    }) -BankedNames @('TestOther', 'TestPinned', 'TestFakeSuite', 'TestFakeSuite/case1', 'TestFakeSuite/case2')).Accepted
Assert-Equal 'capability-absent: an unaccounted extra live verdict is refused' $false `
    (Test-CapabilityAbsentDelta -Expected 4 -Disclosed 0 -Block $block -Got 2 -Comparison ([PSCustomObject]@{
        go = New-VerdictMap @{ TestOther = 'pass'; TestFakeSuite = 'skip'; TestRogue = 'pass' }
        csharp = New-VerdictMap @{ TestOther = 'pass'; TestFakeSuite = 'skip'; TestRogue = 'pass' }
    }) -BankedNames $fullBankedNames).Accepted

# os's measured shape in miniature (2026-09-26, the i9, no symlink privilege): the block root SKIPS on
# both runtimes at testenv.MustHaveSymlink, its two subtests are never spawned, and the row's two
# disclosures (both alloc tests) stand unchanged -- banked 4 + 2 (root, InRoot, NoRoot, TestOther),
# read 2 + 2. The privileged host is the second arm: it reads the banked count, which classifies as a
# plain pass BEFORE any absorption is consulted (the sweep calls the rule only when the class is not
# 'pass') -- a scratch stand-in for a privileged run, not one.
$osBlock = [PSCustomObject]@{ Test = 'TestOpenFileCreateExclDanglingSymlink'; BlockSize = 3 }
$osBankedNames = @('TestOther', 'TestUTF16Alloc', 'TestWriteStringAlloc', 'TestOpenFileCreateExclDanglingSymlink',
    'TestOpenFileCreateExclDanglingSymlink/InRoot', 'TestOpenFileCreateExclDanglingSymlink/NoRoot')
$osUnprivileged = [PSCustomObject]@{
    go = New-VerdictMap @{ TestOther = 'pass'; TestUTF16Alloc = 'pass'; TestWriteStringAlloc = 'pass'; TestOpenFileCreateExclDanglingSymlink = 'skip' }
    csharp = New-VerdictMap @{ TestOther = 'pass'; TestUTF16Alloc = 'fail'; TestWriteStringAlloc = 'fail'; TestOpenFileCreateExclDanglingSymlink = 'skip' }
    disclosed = @('TestUTF16Alloc (deferred): x', 'TestWriteStringAlloc (deferred): y')
}
Assert-Equal 'capability-absent: os unprivileged -- SKIP root, InRoot/NoRoot absent -- is accepted at banked - 2' $true `
    (Test-CapabilityAbsentDelta -Expected 4 -Disclosed 2 -Block $osBlock -Got 2 -Comparison $osUnprivileged -BankedNames $osBankedNames).Accepted
Assert-Equal 'capability-absent: os privileged reads the banked count, a plain pass that never consults the rule' 'pass' `
    (Get-SweepRowClassification -Expectation $winPlain -Got $winPlain.Expected -GotDisclosed 0 -TargetGoos 'windows')

# ---- 1b2b. the host-conditional DISCLOSURE arm (Q31), exercised end to end ------------------------
# Test-HostConditionalDisclosureDelta reads the live record alone. The fixture is os/exec's measured
# shape: banked 87 + 1 on the Linux bank host (TestExtraFiles runs there, Go=pass / C#=pass), and
# 86 + 2 on a container whose single-file published host holds 97 descriptors in 3..100, where the
# platform-skip entry fires (Go=pass / C#=skip) -- one verdict changing column, nothing else.
$hcdNames = @('TestExtraFiles')
$hcdFired = [PSCustomObject]@{
    go = New-VerdictMap @{ TestOther = 'pass'; TestExtraFiles = 'pass'; TestCredentialNoSetGroups = 'pass' }
    csharp = New-VerdictMap @{ TestOther = 'pass'; TestExtraFiles = 'skip'; TestCredentialNoSetGroups = 'fail' }
    disclosed = @('TestCredentialNoSetGroups (host-limit): the seam names the field', 'TestExtraFiles (platform-skip): source-defined platform skip')
}
$hcdResult = Test-HostConditionalDisclosureDelta -Expected 87 -Disclosed 1 -Names $hcdNames -Got 86 -GotDisclosed 2 -Comparison $hcdFired
Assert-Equal 'host-conditional-disclosure: the fired reading is accepted' $true $hcdResult.Accepted
Assert-Equal 'host-conditional-disclosure: the accepted result names what fired' 'TestExtraFiles' ($hcdResult.Fired -join ',')
Assert-Equal 'host-conditional-disclosure: the banking-host reading has nothing to absorb' $false `
    (Test-HostConditionalDisclosureDelta -Expected 87 -Disclosed 1 -Names $hcdNames -Got 87 -GotDisclosed 1 -Comparison $hcdFired).Accepted
Assert-Equal 'host-conditional-disclosure: a lost verdict beside the fired one is refused (85 + 2)' $false `
    (Test-HostConditionalDisclosureDelta -Expected 87 -Disclosed 1 -Names $hcdNames -Got 85 -GotDisclosed 2 -Comparison $hcdFired).Accepted
Assert-Equal 'host-conditional-disclosure: a second, unnamed disclosure is refused (86 + 3)' $false `
    (Test-HostConditionalDisclosureDelta -Expected 87 -Disclosed 1 -Names $hcdNames -Got 86 -GotDisclosed 3 -Comparison $hcdFired).Accepted
Assert-Equal 'host-conditional-disclosure: the moved verdict being some OTHER name is refused' $false `
    (Test-HostConditionalDisclosureDelta -Expected 87 -Disclosed 1 -Names $hcdNames -Got 86 -GotDisclosed 2 -Comparison ([PSCustomObject]@{
        go = New-VerdictMap @{ TestOther = 'pass'; TestExtraFiles = 'pass'; TestSomethingElse = 'pass' }
        csharp = New-VerdictMap @{ TestOther = 'pass'; TestExtraFiles = 'pass'; TestSomethingElse = 'skip' }
        disclosed = @('TestCredentialNoSetGroups (host-limit): the seam names the field', 'TestSomethingElse (platform-skip): unrelated')
    })).Accepted
Assert-Equal 'host-conditional-disclosure: the named entry firing in any other shape is refused (Go=pass / C#=fail)' $false `
    (Test-HostConditionalDisclosureDelta -Expected 87 -Disclosed 1 -Names $hcdNames -Got 86 -GotDisclosed 2 -Comparison ([PSCustomObject]@{
        go = New-VerdictMap @{ TestOther = 'pass'; TestExtraFiles = 'pass' }
        csharp = New-VerdictMap @{ TestOther = 'pass'; TestExtraFiles = 'fail' }
        disclosed = @('TestCredentialNoSetGroups (host-limit): the seam names the field', 'TestExtraFiles (platform-skip): source-defined platform skip')
    })).Accepted
Assert-Equal 'host-conditional-disclosure: a row naming nothing absorbs nothing' $false `
    (Test-HostConditionalDisclosureDelta -Expected 87 -Disclosed 1 -Names @() -Got 86 -GotDisclosed 2 -Comparison $hcdFired).Accepted
Assert-Equal 'host-conditional-disclosure: a record without verdict maps is refused' $false `
    (Test-HostConditionalDisclosureDelta -Expected 87 -Disclosed 1 -Names $hcdNames -Got 86 -GotDisclosed 2 -Comparison ([PSCustomObject]@{ disclosed = @('TestExtraFiles (platform-skip): x') })).Accepted

# ---- 1b2a. the host-limited mirror -- the THIRD host state, exercised end to end ------------------
# The shape the capability-absent rule refuses on its LAST check, and must go on refusing: the
# capability was PRESENT, Go fanned the whole matrix out, and the converted side could not produce it
# inside the deadline the test itself carries. What makes it absorbable is not a weaker check, it is
# a THIRD evidence artifact the other rule never reads -- the package's COMMITTED disclosure manifest
# pinning the block root as `host-limit` -- plus the strongest identity evidence available anywhere
# in this family: the converter WITHDRAWS every Go-side row beneath a signature-matched disclosed
# root and publishes the list, so the record enumerates the lost verdicts by name and this rule
# requires that enumeration to BE the block's banked sub-verdicts, both directions, nothing else.
#
# ⚠ THE DISCRIMINATOR IS THE FAN-OUT, NOT THE ROOT PAIR, and both arms below are real. The tempting
# reading -- state 3 is Go-pass/C#-fail, state 2 is the agreeing non-pass -- is WRONG and would build
# a rule that refuses the very host it exists for: the i7 coordinator's measured crypto/tls run
# (2026-09-01) reports `TestBogoSuite` go='fail' C#='fail' with 3,242 rows withdrawn, because Go's
# oracle fans out every case in under a minute and its root still fails on a handful of them. Both
# arms are pinned here so neither can be lost to the other.
#
# Same three-verdict miniature as the block above: root + two cases, banked 4 matched + 0 disclosed,
# of which 3 are the block, so a host-limited run scores 1 matched + 1 disclosed (the root).
$hostLimitPin = [PSCustomObject]@{ Class = 'host-limit'; Signature = 'runner failed: exit status 1' }
$hostLimitComparison = [PSCustomObject]@{
    go = New-VerdictMap @{ TestOther = 'pass'; TestFakeSuite = 'pass' }
    csharp = New-VerdictMap @{ TestOther = 'pass'; TestFakeSuite = 'fail' }
    withdrawn = @('TestFakeSuite/case1', 'TestFakeSuite/case2')
    disclosed = @('TestFakeSuite (host-limit): the runner outruns its own deadline')
}
# The MEASURED arm: the same run with Go's own root red too, which is what the sweep host produces.
$hostLimitFailComparison = [PSCustomObject]@{
    go = New-VerdictMap @{ TestOther = 'pass'; TestFakeSuite = 'fail' }
    csharp = New-VerdictMap @{ TestOther = 'pass'; TestFakeSuite = 'fail' }
    withdrawn = @('TestFakeSuite/case1', 'TestFakeSuite/case2')
    disclosed = @('TestFakeSuite (host-limit): the runner outruns its own deadline')
}

Assert-Equal 'host-limit: the pinned arm -- Go pass, C# fail, block withdrawn, root disclosed -- is accepted' $true `
    (Test-HostLimitDelta -Expected 4 -Disclosed 0 -Block $block -Got 1 -Comparison $hostLimitComparison `
        -BankedNames $fullBankedNames -Pin $hostLimitPin).Accepted
Assert-Equal 'host-limit: the MEASURED arm -- Go fail, C# fail, block withdrawn, root disclosed -- is accepted' $true `
    (Test-HostLimitDelta -Expected 4 -Disclosed 0 -Block $block -Got 1 -Comparison $hostLimitFailComparison `
        -BankedNames $fullBankedNames -Pin $hostLimitPin).Accepted

# THE ADMISSION GATE, in both of its failure directions. Without a committed pin this rule would be
# a general "accept a block-sized shortfall", which is the change it exists not to be.
Assert-Equal 'host-limit: no committed pin refuses the identical evidence' $false `
    (Test-HostLimitDelta -Expected 4 -Disclosed 0 -Block $block -Got 1 -Comparison $hostLimitComparison `
        -BankedNames $fullBankedNames -Pin $null).Accepted
Assert-Equal 'host-limit: a pin of some OTHER class refuses the identical evidence' $false `
    (Test-HostLimitDelta -Expected 4 -Disclosed 0 -Block $block -Got 1 -Comparison $hostLimitComparison `
        -BankedNames $fullBankedNames -Pin ([PSCustomObject]@{ Class = 'alloc-profile'; Signature = 'x' })).Accepted

# THE BINDING PROPERTY: the shortfall must BE the block's own sub-verdicts. A right-SIZED loss whose
# withdrawn names are not the banked ones is exactly the cancellation this refuses to be fooled by --
# asserted in both directions, since a rogue loss and a rogue withdrawal cancel in the count.
Assert-Equal 'host-limit: a banked sub-verdict that was NOT withdrawn is refused' $false `
    (Test-HostLimitDelta -Expected 4 -Disclosed 0 -Block $block -Got 1 -Comparison ([PSCustomObject]@{
        go = New-VerdictMap @{ TestOther = 'pass'; TestFakeSuite = 'pass' }
        csharp = New-VerdictMap @{ TestOther = 'pass'; TestFakeSuite = 'fail' }
        withdrawn = @('TestFakeSuite/case1')
        disclosed = @('TestFakeSuite (host-limit): the runner outruns its own deadline')
    }) -BankedNames $fullBankedNames -Pin $hostLimitPin).Accepted
Assert-Equal 'host-limit: a withdrawn name the proof page does not bank is refused' $false `
    (Test-HostLimitDelta -Expected 4 -Disclosed 0 -Block $block -Got 1 -Comparison ([PSCustomObject]@{
        go = New-VerdictMap @{ TestOther = 'pass'; TestFakeSuite = 'pass' }
        csharp = New-VerdictMap @{ TestOther = 'pass'; TestFakeSuite = 'fail' }
        withdrawn = @('TestFakeSuite/case1', 'TestFakeSuite/case3')
        disclosed = @('TestFakeSuite (host-limit): the runner outruns its own deadline')
    }) -BankedNames $fullBankedNames -Pin $hostLimitPin).Accepted
Assert-Equal 'host-limit: no fan-out at all is refused -- nothing proves WHICH verdicts were lost' $false `
    (Test-HostLimitDelta -Expected 4 -Disclosed 0 -Block $block -Got 1 -Comparison ([PSCustomObject]@{
        go = New-VerdictMap @{ TestOther = 'pass'; TestFakeSuite = 'pass' }
        csharp = New-VerdictMap @{ TestOther = 'pass'; TestFakeSuite = 'fail' }
        disclosed = @('TestFakeSuite (host-limit): the runner outruns its own deadline')
    }) -BankedNames $fullBankedNames -Pin $hostLimitPin).Accepted

# THE PARTITION, asserted in BOTH directions on the two evidence shapes that differ ONLY in whether
# the Go side fanned out. $absentFailComparison is byte-identical to $hostLimitFailComparison but for
# the missing `withdrawn` rows -- same roots, same verdicts, same disclosure -- and the two rules
# answer oppositely on the pair. That is the whole design: neither rule is a relaxation of the other,
# and no run can be read both ways.
Assert-Equal 'partition: a fail/fail collapse with NO fan-out is capability-absent, refused here' $false `
    (Test-HostLimitDelta -Expected 4 -Disclosed 0 -Block $block -Got 1 -Comparison $absentFailComparison `
        -BankedNames $fullBankedNames -Pin $hostLimitPin).Accepted
Assert-Equal 'partition: ...and the capability-absent rule ACCEPTS that same no-fan-out evidence' $true `
    (Test-CapabilityAbsentDelta -Expected 4 -Disclosed 0 -Block $block -Got 1 -Comparison $absentFailComparison `
        -BankedNames $fullBankedNames).Accepted
Assert-Equal 'partition: a fail/fail collapse WITH the fan-out is host-limited, accepted here' $true `
    (Test-HostLimitDelta -Expected 4 -Disclosed 0 -Block $block -Got 1 -Comparison $hostLimitFailComparison `
        -BankedNames $fullBankedNames -Pin $hostLimitPin).Accepted
Assert-Equal 'partition: ...and the capability-absent rule REFUSES that same fanned-out evidence' $false `
    (Test-CapabilityAbsentDelta -Expected 4 -Disclosed 0 -Block $block -Got 1 -Comparison $hostLimitFailComparison `
        -BankedNames $fullBankedNames).Accepted
Assert-Equal 'partition: the capability-absent rule also refuses the Go-pass arm this rule accepts' $false `
    (Test-CapabilityAbsentDelta -Expected 4 -Disclosed 0 -Block $block -Got 1 -Comparison $hostLimitComparison `
        -BankedNames $fullBankedNames).Accepted

# The verdict pair. The CONVERTED side must be the half that failed, and a SKIPPED Go root is refused
# even with a full fan-out -- Go has no capability-absent skip branch here, and a skipping root
# cannot have fanned anything out, so such a record is incoherent rather than absorbable.
Assert-Equal 'host-limit: an agreeing SKIP root is refused' $false `
    (Test-HostLimitDelta -Expected 4 -Disclosed 0 -Block $block -Got 1 -Comparison $absentComparison `
        -BankedNames $fullBankedNames -Pin $hostLimitPin).Accepted
Assert-Equal 'host-limit: a SKIPPED Go root is refused even WITH the full fan-out' $false `
    (Test-HostLimitDelta -Expected 4 -Disclosed 0 -Block $block -Got 1 -Comparison ([PSCustomObject]@{
        go = New-VerdictMap @{ TestOther = 'pass'; TestFakeSuite = 'skip' }
        csharp = New-VerdictMap @{ TestOther = 'pass'; TestFakeSuite = 'fail' }
        withdrawn = @('TestFakeSuite/case1', 'TestFakeSuite/case2')
        disclosed = @('TestFakeSuite (host-limit): the runner outruns its own deadline')
    }) -BankedNames $fullBankedNames -Pin $hostLimitPin).Accepted
Assert-Equal 'host-limit: a C# side that did NOT fail is refused' $false `
    (Test-HostLimitDelta -Expected 4 -Disclosed 0 -Block $block -Got 1 -Comparison ([PSCustomObject]@{
        go = New-VerdictMap @{ TestOther = 'pass'; TestFakeSuite = 'pass' }
        csharp = New-VerdictMap @{ TestOther = 'pass'; TestFakeSuite = 'pass' }
        withdrawn = @('TestFakeSuite/case1', 'TestFakeSuite/case2')
        disclosed = @('TestFakeSuite (host-limit): the runner outruns its own deadline')
    }) -BankedNames $fullBankedNames -Pin $hostLimitPin).Accepted

# The remaining shapes, each closing one way a real change could pass for this one.
Assert-Equal 'host-limit: a shortfall that is not the registered block size is refused' $false `
    (Test-HostLimitDelta -Expected 4 -Disclosed 0 -Block $block -Got 2 -Comparison $hostLimitComparison `
        -BankedNames $fullBankedNames -Pin $hostLimitPin).Accepted
Assert-Equal 'host-limit: a surplus is refused' $false `
    (Test-HostLimitDelta -Expected 4 -Disclosed 0 -Block $block -Got 5 -Comparison $hostLimitComparison `
        -BankedNames $fullBankedNames -Pin $hostLimitPin).Accepted
Assert-Equal 'host-limit: a subtest surviving in the compared set is refused, not absorbed' $false `
    (Test-HostLimitDelta -Expected 4 -Disclosed 0 -Block $block -Got 1 -Comparison ([PSCustomObject]@{
        go = New-VerdictMap @{ TestOther = 'pass'; TestFakeSuite = 'pass'; 'TestFakeSuite/case1' = 'pass' }
        csharp = New-VerdictMap @{ TestOther = 'pass'; TestFakeSuite = 'fail'; 'TestFakeSuite/case1' = 'pass' }
        withdrawn = @('TestFakeSuite/case1', 'TestFakeSuite/case2')
        disclosed = @('TestFakeSuite (host-limit): the runner outruns its own deadline')
    }) -BankedNames $fullBankedNames -Pin $hostLimitPin).Accepted
Assert-Equal 'host-limit: a banked verdict OUTSIDE the block going missing is refused' $false `
    (Test-HostLimitDelta -Expected 4 -Disclosed 0 -Block $block -Got 1 -Comparison ([PSCustomObject]@{
        go = New-VerdictMap @{ TestFakeSuite = 'pass' }
        csharp = New-VerdictMap @{ TestFakeSuite = 'fail' }
        withdrawn = @('TestFakeSuite/case1', 'TestFakeSuite/case2')
        disclosed = @('TestFakeSuite (host-limit): the runner outruns its own deadline')
    }) -BankedNames $fullBankedNames -Pin $hostLimitPin).Accepted
Assert-Equal 'host-limit: an unaccounted extra live verdict is refused' $false `
    (Test-HostLimitDelta -Expected 4 -Disclosed 0 -Block $block -Got 1 -Comparison ([PSCustomObject]@{
        go = New-VerdictMap @{ TestOther = 'pass'; TestFakeSuite = 'pass'; TestRogue = 'pass' }
        csharp = New-VerdictMap @{ TestOther = 'pass'; TestFakeSuite = 'fail'; TestRogue = 'pass' }
        withdrawn = @('TestFakeSuite/case1', 'TestFakeSuite/case2')
        disclosed = @('TestFakeSuite (host-limit): the runner outruns its own deadline')
    }) -BankedNames $fullBankedNames -Pin $hostLimitPin).Accepted
Assert-Equal 'host-limit: a disclosed count that moved beyond the root is refused' $false `
    (Test-HostLimitDelta -Expected 4 -Disclosed 0 -Block $block -Got 1 -Comparison ([PSCustomObject]@{
        go = New-VerdictMap @{ TestOther = 'pass'; TestFakeSuite = 'pass' }
        csharp = New-VerdictMap @{ TestOther = 'pass'; TestFakeSuite = 'fail' }
        withdrawn = @('TestFakeSuite/case1', 'TestFakeSuite/case2')
        disclosed = @('TestFakeSuite (host-limit): x', 'TestOther (alloc-profile): y')
    }) -BankedNames $fullBankedNames -Pin $hostLimitPin).Accepted
Assert-Equal 'host-limit: an extra disclosure that is some OTHER row is refused' $false `
    (Test-HostLimitDelta -Expected 4 -Disclosed 0 -Block $block -Got 1 -Comparison ([PSCustomObject]@{
        go = New-VerdictMap @{ TestOther = 'pass'; TestFakeSuite = 'pass' }
        csharp = New-VerdictMap @{ TestOther = 'pass'; TestFakeSuite = 'fail' }
        withdrawn = @('TestFakeSuite/case1', 'TestFakeSuite/case2')
        disclosed = @('TestSomethingElse (host-limit): unrelated')
    }) -BankedNames $fullBankedNames -Pin $hostLimitPin).Accepted
# The record's own class must agree with the pin: the compare oracle spells the class it actually
# applied into the entry, so pin and record describing different divergences is caught here.
Assert-Equal 'host-limit: a root disclosed under a class other than the pinned one is refused' $false `
    (Test-HostLimitDelta -Expected 4 -Disclosed 0 -Block $block -Got 1 -Comparison ([PSCustomObject]@{
        go = New-VerdictMap @{ TestOther = 'pass'; TestFakeSuite = 'pass' }
        csharp = New-VerdictMap @{ TestOther = 'pass'; TestFakeSuite = 'fail' }
        withdrawn = @('TestFakeSuite/case1', 'TestFakeSuite/case2')
        disclosed = @('TestFakeSuite (performance-margin): the runner outruns its own deadline')
    }) -BankedNames $fullBankedNames -Pin $hostLimitPin).Accepted
# A row whose proof page and roster columns disagree carries inconsistent banked evidence -- absorb
# nothing, exactly as the sibling rule refuses there.
Assert-Equal 'host-limit: a proof page that disagrees with the roster columns is refused' $false `
    (Test-HostLimitDelta -Expected 5 -Disclosed 0 -Block $block -Got 2 -Comparison $hostLimitComparison `
        -BankedNames $fullBankedNames -Pin $hostLimitPin).Accepted

# ---- 1b3. the comparison-record reader's contract ------------------------------------------------
# The trap this pins (measured 2026-08-29, G's net/http pre-staging): a Go suite may legally hold
# verdict names differing ONLY by case (net/http's .../GZIP and .../gzip pairs), which 5.1's
# ConvertFrom-Json throws on and a PSObject cannot represent at all. The reader must carry the pair
# DISTINCTLY -- two keys, two different values -- because a folding parser can at best keep one, so
# distinct values are the fold-detector, not just the count.
$readerFixturePath = Join-Path ([System.IO.Path]::GetTempPath()) ('go2cs-comparison-fixture-' + [guid]::NewGuid().ToString('n') + '.json')
try {
    [System.IO.File]::WriteAllText($readerFixturePath,
        '{"package":"fake","go":{"TestCase/GZIP":"pass","TestCase/gzip":"fail"},"csharp":{"TestCase/GZIP":"pass"},"withdrawn":["TestW"],"disclosed":["TestD (alloc-profile): x"]}',
        (New-Object System.Text.UTF8Encoding($false)))
    $readerRecord = ConvertFrom-ComparisonRecord -Path $readerFixturePath
    Assert-Equal 'reader: case-only verdict-name pair carried as TWO keys' 2 $readerRecord.go.Count
    Assert-Equal 'reader: upper-cased member keeps its own verdict' 'pass' $readerRecord.go['TestCase/GZIP']
    Assert-Equal 'reader: lower-cased member keeps its own verdict' 'fail' $readerRecord.go['TestCase/gzip']
    Assert-Equal 'reader: lookup is case-sensitive (absent case-variant is absent)' $false $readerRecord.csharp.ContainsKey('TestCase/gzip')
    Assert-Equal 'reader: withdrawn survives as an array' 'TestW' (@($readerRecord.withdrawn) -join ',')
    Assert-Equal 'reader: disclosed survives as an array' 1 (@($readerRecord.disclosed).Count)
}
finally {
    if (Test-Path $readerFixturePath) { Remove-Item $readerFixturePath -Force }
}
$readerAbsentPath = Join-Path ([System.IO.Path]::GetTempPath()) ('go2cs-comparison-fixture-' + [guid]::NewGuid().ToString('n') + '.json')
try {
    [System.IO.File]::WriteAllText($readerAbsentPath, '{"package":"fake"}', (New-Object System.Text.UTF8Encoding($false)))
    $readerAbsent = ConvertFrom-ComparisonRecord -Path $readerAbsentPath
    Assert-Equal 'reader: an absent go map is null (the delta rules'' no-maps rejection still fires)' $true ($null -eq $readerAbsent.go)
    Assert-Equal 'reader: absent withdrawn/disclosed are null' $true (($null -eq $readerAbsent.withdrawn) -and ($null -eq $readerAbsent.disclosed))
}
finally {
    if (Test-Path $readerAbsentPath) { Remove-Item $readerAbsentPath -Force }
}

# ---- 1c. the exclusion-ledger parser's contract --------------------------------------------------
# The ledger row's first cell is a PLAIN code span and the roster row's is a LINKED one -- the shape
# difference is the only thing keeping two tables in one document apart, so it is pinned in BOTH
# directions: a roster-shaped row must not read as a ledger row, and a ledger row must not read as
# a roster row.
$ledgerFixture = Read-FixtureLedger @(
    "| ``ex/one`` | 0 | E1 | Nothing eligible on this target. | [ruling][r] |"
    "| ``ex/two`` | $dash | E2 | The oracle fails. | [ruling][r] |"
    "| ``ex/three`` | 6 | E3 | The subject is the replaced representation. | [ruling][r] |"
    "| [``ros/row``](https://x/ros) | 12 |  | A roster-shaped row. $dot [proof](p.md) |"
)

Assert-Equal 'ledger fixture: plain-code-span rows parse, the roster-shaped row does not' 3 $ledgerFixture.Count
Assert-Equal 'ledger columns: package' 'ex/one' $ledgerFixture[0].Package
Assert-Equal 'ledger columns: verdicts is the raw cell text' '0' $ledgerFixture[0].Verdicts
Assert-Equal 'ledger columns: class' 'E1' $ledgerFixture[0].Class
Assert-Equal 'ledger columns: a dashed verdicts cell still carries its class' 'E2' $ledgerFixture[1].Class
Assert-Equal 'ledger row does not read as a roster row' 0 `
    (@(Read-FixtureRoster @("| ``ex/one`` | 0 | E1 | Nothing eligible on this target. | [ruling][r] |")).Count)

# The ledger's ADDRESS, which is the half the shape above could never supply. Added 2026-09-22 after
# the H10 relocation seat put six more plain-code-span tables in this one document and 51 of this
# guard's 70 failures at master 3b48e0c8e0 were the ledger parser reading them. Each arm below is
# written so that REMOVING the scoping makes it fail:
#
#   out-of-section  a ledger-shaped row under a LATER sibling heading is not a ledger row, however
#                   perfectly it matches -- this is the relocation tables' exact position.
#   subsection      a row under the ledger's OWN deeper heading IS one; the section runs to the next
#                   heading of equal or higher level, so `### The 215, derived` belongs to it.
#   no heading      refuses, rather than reverting to the whole-document scan being removed here.
#   two headings    refuses; two addresses is an ambiguity, not a choice to make silently.
$ledgerScopeFixture = Read-FixtureLedger -Rows @(
    "| ``ex/one`` | 0 | E1 | Nothing eligible on this target. | [ruling][r] |"
) -Trailing @(
    ''
    '## The relocation map'
    ''
    '| banked row | decls | successor |'
    '|:--|--:|:--|'
    "| ``ex/moved`` | 7 | ``ex/successor`` 7 |"
)

Assert-Equal 'ledger scope: a ledger-shaped row under a LATER sibling heading is not a ledger row' 1 `
    $ledgerScopeFixture.Count
Assert-Equal 'ledger scope: the row that parsed is the in-section one' 'ex/one' $ledgerScopeFixture[0].Package

$ledgerSubsectionFixture = Read-FixtureLedger -Rows @(
    "| ``ex/one`` | 0 | E1 | Nothing eligible on this target. | [ruling][r] |"
) -Trailing @(
    ''
    '### The derivation, dated'
    ''
    "| ``ex/two`` | 6 | E3 | Still the ledger's own section. | [ruling][r] |"
)

Assert-Equal 'ledger scope: the section runs through its OWN subsections' 2 $ledgerSubsectionFixture.Count

Assert-Throws 'ledger scope: no heading REFUSES rather than scanning the whole document' {
    Read-FixtureLedger -Rows @("| ``ex/one`` | 0 | E1 | Nothing. | [ruling][r] |") -Prefix @('# Some other page', '')
} 'no markdown heading reads'

Assert-Throws 'ledger scope: a DUPLICATED heading REFUSES rather than picking one' {
    Read-FixtureLedger -Rows @("| ``ex/one`` | 0 | E1 | Nothing. | [ruling][r] |") `
        -Trailing @('', '## Excluded packages', '')
} 'appears 2 times'

# ---- 2. the roster's own arithmetic --------------------------------------------------------------
$rows = @(Get-ValidatedRosterRows -Path $table)
$lines = [System.IO.File]::ReadAllLines($table)

function Get-HeaderNumber {
    param([string[]] $Lines, [string] $Select, [string] $Pattern, [int] $Group = 1)

    foreach ($line in $Lines) {
        if ($line -match [regex]::Escape($Select) -and $line -match $Pattern) {
            return [int](($Matches[$Group]) -replace ',', '')
        }
    }

    return -1
}

$columnTotal = ($rows | Measure-Object -Property Expected -Sum).Sum
$columnDisclosed = ($rows | Measure-Object -Property Disclosed -Sum).Sum

Assert-Equal 'header: validated package count equals the table row count' $rows.Count `
    (Get-HeaderNumber $lines 'Phase 4 progress' '(\d+)\s*/\s*(\d+)\s+testable packages validated')
Assert-Equal 'header: matching verdicts equal the Tests column sum' $columnTotal `
    (Get-HeaderNumber $lines 'matching test verdicts' '([\d,]+)\s+matching test verdicts')
Assert-Equal 'header: disclosed equals the Disclosed column sum' $columnDisclosed `
    (Get-HeaderNumber $lines 'matching test verdicts' '([\d,]+)\s+disclosed')

$testable = Get-HeaderNumber $lines 'Phase 4 progress' '(\d+)\s*/\s*(\d+)\s+testable packages validated' 2
$percentText = ''
foreach ($line in $lines) {
    if ($line -match 'Phase 4 progress' -and $line -match '([\d.]+)%') { $percentText = $Matches[1]; break }
}
if ($testable -gt 0) {
    $expectedPercent = [math]::Round(($rows.Count / [double]$testable) * 100, 1, [MidpointRounding]::AwayFromZero)
    Assert-Equal 'header: the percentage follows from the two counts' ('{0:0.0}' -f $expectedPercent) $percentText
}

# The implementable-set line, derived the same way as everything above it: the excluded count from
# the ledger table, the denominator as the subtraction it states, the percentage recomputed. This
# line was the header's one hand-computed exception when it landed; now it can go stale as silently
# as its siblings can -- which is to say, not at all.
$ledger = @(Get-ExclusionLedgerRows -Path $table)

# Every ledger row's class must be one of the ruled exclusion classes -- an unruled class name is a
# row admitted outside the admission bar, not a new class this guard should learn silently.
foreach ($row in $ledger) {
    Assert-Equal "ledger: $($row.Package) carries a ruled class ($($ExclusionLedgerClasses -join '/'), got '$($row.Class)')" `
        $true ($ExclusionLedgerClasses -contains $row.Class)
}

# Excluded and validated are disjoint by construction: a package that validates has rejoined the
# denominator, and a row counted on both sides of the subtraction counts itself twice.
$rosterPackages = @($rows | ForEach-Object { $_.Package })
$excludedAndValidated = @($ledger | Where-Object { $rosterPackages -contains $_.Package } | ForEach-Object { $_.Package })
Assert-Equal 'ledger: no excluded package is also a roster row' '' ($excludedAndValidated -join ', ')

# The subtraction sign admits an ASCII hyphen beside U+2212 -- the arithmetic, not the glyph, is
# what this guards.
$minusClass = '[\-' + $minus + ']'
$honestSetPattern = '\((\d+)\s*' + $minusClass + '\s*(\d+)\s+excluded\s*=\s*(\d+)\)'
$honestRatioPattern = ':\s*(\d+)\s*/\s*(\d+)'
$implementable = $testable - $ledger.Count

Assert-Equal 'honest header: restates the naive denominator' $testable `
    (Get-HeaderNumber $lines 'Against the implementable set' $honestSetPattern)
Assert-Equal 'honest header: excluded count equals the ledger row count' $ledger.Count `
    (Get-HeaderNumber $lines 'Against the implementable set' $honestSetPattern 2)
Assert-Equal 'honest header: stated difference equals naive minus excluded' $implementable `
    (Get-HeaderNumber $lines 'Against the implementable set' $honestSetPattern 3)
Assert-Equal 'honest header: numerator equals the table row count' $rows.Count `
    (Get-HeaderNumber $lines 'Against the implementable set' $honestRatioPattern)
Assert-Equal 'honest header: denominator equals the implementable set' $implementable `
    (Get-HeaderNumber $lines 'Against the implementable set' $honestRatioPattern 2)

$honestPercentText = ''
foreach ($line in $lines) {
    if ($line -match 'Against the implementable set' -and $line -match '([\d.]+)%') { $honestPercentText = $Matches[1]; break }
}
if ($implementable -gt 0) {
    $expectedHonestPercent = [math]::Round(($rows.Count / [double]$implementable) * 100, 1, [MidpointRounding]::AwayFromZero)
    Assert-Equal 'honest header: the percentage follows from the two counts' ('{0:0.0}' -f $expectedHonestPercent) $honestPercentText
}

# ---- 2b. the header's N is a POPULATION FILE, not a sentence (ruled 2026-09-22) ------------------
# Everything above this point derives the header from the TABLE, which is the right authority for
# the numerator and for both column sums -- and is structurally incapable of saying anything about
# the DENOMINATOR. `$testable` is parsed from the header and compared to nothing: the table cannot
# know how many testable packages exist, so until now that one number was the header's unguarded
# exception, asserted by prose and reproducible only by whoever last re-derived it. That is the same
# shape as the phantom ledger row the 2026-09-02 ruling struck -- an arithmetic that "comes out
# right" over a membership nobody can check.
#
# It is checkable now because the population is ENUMERATED:
# docs/phase4/hopA-inputs/recon-lists/population-go1.24.13.txt holds all N import paths. Four
# assertions follow from having it, and none of them can be satisfied by editing the header alone:
#   1. |population| == the header's N.
#   2. every BANKED row is a population member -- a row validated outside the denominator it is
#      counted in is a numerator that cannot be reached from the denominator.
#   3. every EXCLUSION row is a population member -- this is the struck-phantom rule, in code. Four
#      more rows were struck on 2026-09-22 for exactly this reason.
#   4. implementable == N - (exclusion rows), which 2's and 3's memberships make exact.
# The two percentages are asserted above and follow from these.
#
# READ ENDING-INSENSITIVELY, deliberately. The sibling instrument (shardmap.py) REFUSES on a CR byte
# in this file, because a CR there rides into a package name and every figure derived from it. This
# one trims instead: it is the roster's guard and runs on any checkout, and a lane whose clone
# predates the .gitattributes pin should get the arithmetic it asked for, not a line-endings lecture
# from the wrong instrument. Two guards, two questions.
$populationPath = Join-Path $repo 'docs/phase4/hopA-inputs/recon-lists/population-go1.24.13.txt'

# Get-PopulationRows, the reader, lives in _roster.ps1 (2026-09-23): push-nuget.ps1's release census
# reads the rowless-candidate class through it too, and one reader is what keeps the two agreeing.

# The arithmetic itself, as a function over its four inputs rather than inline against the real
# ones. That is what lets the fixture arms below drive it with a POPULATION THAT DISAGREES -- an
# inline version could only ever be red-tested by editing the committed file, which is the exact
# "regress the tree and hope you restore it" shape the floor forbids relying on. Returns one string
# per violation, each NAMING what it found; an empty array is a clean reading.
function Test-PopulationArithmetic {
    param(
        [Parameter(Mandatory)][AllowEmptyCollection()][string[]] $Population,
        [Parameter(Mandatory)][AllowEmptyCollection()][string[]] $BankedPackages,
        [Parameter(Mandatory)][AllowEmptyCollection()][string[]] $ExcludedPackages,
        [Parameter(Mandatory)][int] $Testable,
        [Parameter(Mandatory)][int] $Implementable
    )

    $violations = New-Object System.Collections.Generic.List[string]
    $member = @{}
    foreach ($name in $Population) { $member[$name] = $true }

    if ($Population.Count -ne $Testable) {
        [void]$violations.Add("the header's N ($Testable) is not the population's size ($($Population.Count))")
    }

    $strayBanked = @($BankedPackages | Where-Object { -not $member.ContainsKey($_) })
    if ($strayBanked.Count -gt 0) {
        [void]$violations.Add("$($strayBanked.Count) banked row(s) outside the population: $($strayBanked -join ', ')")
    }

    $strayExcluded = @($ExcludedPackages | Where-Object { -not $member.ContainsKey($_) })
    if ($strayExcluded.Count -gt 0) {
        [void]$violations.Add("$($strayExcluded.Count) exclusion row(s) outside the population: $($strayExcluded -join ', ')")
    }

    $expectedImplementable = $Testable - $ExcludedPackages.Count
    if ($Implementable -ne $expectedImplementable) {
        [void]$violations.Add("implementable ($Implementable) is not N minus the exclusion rows ($Testable - $($ExcludedPackages.Count) = $expectedImplementable)")
    }

    return @($violations)
}

# RED FIRST. Each arm below was run BEFORE the live reading and each one FAILED BY NAME on the defect
# it plants; the whole set was then re-run green against a fixture that closes. The fixture universe
# is four names so the arms are readable, and every arm changes exactly ONE axis away from it.
$popFixture = @('ex/one', 'ex/two', 'ex/three', 'ex/four')

Assert-Equal 'population fixture: a closing arithmetic reports nothing' '' `
    ((Test-PopulationArithmetic -Population $popFixture -BankedPackages @('ex/one', 'ex/two') `
        -ExcludedPackages @('ex/three') -Testable 4 -Implementable 3) -join '; ')

Assert-Equal 'population arm: a header N that is not the population size fails, naming both counts' `
    "the header's N (5) is not the population's size (4)" `
    ((Test-PopulationArithmetic -Population $popFixture -BankedPackages @('ex/one', 'ex/two') `
        -ExcludedPackages @('ex/three') -Testable 5 -Implementable 4) -join '; ')

Assert-Equal 'population arm: an EXCLUSION row outside the population fails, by name' `
    '1 exclusion row(s) outside the population: ex/phantom' `
    ((Test-PopulationArithmetic -Population $popFixture -BankedPackages @('ex/one', 'ex/two') `
        -ExcludedPackages @('ex/phantom') -Testable 4 -Implementable 3) -join '; ')

Assert-Equal 'population arm: a BANKED row outside the population fails, by name' `
    '1 banked row(s) outside the population: ex/stray' `
    ((Test-PopulationArithmetic -Population $popFixture -BankedPackages @('ex/one', 'ex/stray') `
        -ExcludedPackages @('ex/three') -Testable 4 -Implementable 3) -join '; ')

# The subtraction's own arm. Every arm here is written so it fires ALONE -- each fixture moves ONE
# axis and the expectation is the WHOLE joined violation list, so an arm that dragged a sibling in
# would fail on the extra text rather than pass on a superset. That is not theory: the N arm above
# was first written expecting TWO violations, and it failed by name showing it had produced one,
# which is how its fixture (N=5 against implementable=4, where 5-1=4 still closes) was found to move
# a single axis after all. An arm that can only be seen firing beside another is not an arm.
Assert-Equal 'population arm: an implementable that is not N minus the exclusions fails alone' `
    'implementable (2) is not N minus the exclusion rows (4 - 1 = 3)' `
    ((Test-PopulationArithmetic -Population $popFixture -BankedPackages @('ex/one', 'ex/two') `
        -ExcludedPackages @('ex/three') -Testable 4 -Implementable 2) -join '; ')

# Vacuity controls on the READER, which the arms above cannot reach: a file of comments and a file
# with a repeat both read as a perfectly closed arithmetic over the wrong universe.
Assert-Throws 'population reader: a comment-only file REFUSES rather than reading zero' {
    $p = Join-Path ([System.IO.Path]::GetTempPath()) ('go2cs-pop-fixture-' + [guid]::NewGuid().ToString('n') + '.txt')
    try {
        [System.IO.File]::WriteAllText($p, "# only a header`n`n", (New-Object System.Text.UTF8Encoding($false)))
        Get-PopulationRows -Path $p
    }
    finally { if (Test-Path $p) { Remove-Item $p -Force } }
} 'parsed to ZERO rows'

Assert-Throws 'population reader: a repeated name REFUSES rather than double-booking' {
    $p = Join-Path ([System.IO.Path]::GetTempPath()) ('go2cs-pop-fixture-' + [guid]::NewGuid().ToString('n') + '.txt')
    try {
        [System.IO.File]::WriteAllText($p, "ex/one`nex/two`nex/one`n", (New-Object System.Text.UTF8Encoding($false)))
        Get-PopulationRows -Path $p
    }
    finally { if (Test-Path $p) { Remove-Item $p -Force } }
} 'repeats 1 name(s): ex/one'

# ...and now the live reading, through the same function the arms just exercised.
$population = @(Get-PopulationRows -Path $populationPath)
$ledgerPackages = @($ledger | ForEach-Object { $_.Package })
$populationViolations = @(Test-PopulationArithmetic -Population $population `
    -BankedPackages $rosterPackages -ExcludedPackages $ledgerPackages `
    -Testable $testable -Implementable $implementable)

Write-Host ''
Write-Host 'population of record vs the roster header:' -ForegroundColor Cyan
Write-Host ('  {0,-32} {1}' -f 'file', (Resolve-Path $populationPath).Path.Substring($repo.Length + 1))
Write-Host ('  {0,-32} population {1,-6} header N {2}' -f 'testable packages', $population.Count, $testable)
Write-Host ('  {0,-32} banked {1,-10} excluded {2}' -f 'members accounted for', $rosterPackages.Count, $ledgerPackages.Count)
Write-Host ('  {0,-32} {1} - {2} = {3}' -f 'implementable', $testable, $ledgerPackages.Count, $implementable)

Assert-Equal 'population of record: the header closes against the enumeration' '' ($populationViolations -join '; ')

# ---- 2b2. every tracked TEST PROJECT is accounted for BY NAME (ruled 2026-09-22) -----------------
# The runbook's cross-check, restated as an IDENTITY a reader can re-add: the tracked *.tests.csproj
# set is exactly the banked rows, plus the exclusion rows that kept their artifacts, plus the
# population's rowless CANDIDATES that have them. At the batch-7 stamp that is 225 = 219 + 4 + 2
# (the two candidates reflect and runtime). It was arithmetic in prose until now, and "225 vs 219"
# was read as "the nine rowless" on the day a row banked -- a count that happens to close can hide a
# stray and a missing project that cancel.
#
# The candidate side is NOT derived from the project files, which would make the identity circular:
# a candidate is a POPULATION member that is neither a banked row nor an exclusion row, read from the
# population of record section 2b already loads. So the two ways this can fail are both real:
#   - a banked row with no tracked test project (a row whose artifacts are gone);
#   - a tracked test project for a package that is none of row, exclusion row or candidate (a stray,
#     e.g. a package outside N, or one whose row was struck without its artifacts).
# The TRACKED set comes from git, never the working tree: a build or an unfinished -tests run leaves
# untracked projects behind, and a guard that counted them would red on the lane's own debris.
#
# Get-TestProjectIdentityViolations lives in _roster.ps1 (2026-09-23), shared with push-nuget.ps1's
# release census, which holds the same identity before a publish; the fixtures below still drive it.

# The function's contract, both directions, against fixtures.
$idPop = @('ex/row', 'ex/gone', 'ex/cand', 'ex/quiet')
Assert-Equal 'test-project identity: a closed set reports nothing' 0 `
    @(Get-TestProjectIdentityViolations -Projects @('ex/row', 'ex/gone', 'ex/cand') -Banked @('ex/row') -Excluded @('ex/gone') -Population $idPop).Count
Assert-Equal 'test-project identity: a candidate WITHOUT artifacts is legal' 0 `
    @(Get-TestProjectIdentityViolations -Projects @('ex/row') -Banked @('ex/row') -Excluded @('ex/gone') -Population $idPop).Count
Assert-Equal 'test-project identity: a banked row with no project is named' 'banked row has no tracked tests.csproj: ex/row' `
    (@(Get-TestProjectIdentityViolations -Projects @('ex/cand') -Banked @('ex/row') -Excluded @() -Population $idPop) -join '; ')
Assert-Equal 'test-project identity: a stray project is named' 'tracked tests.csproj belongs to no row, exclusion row or population candidate: ex/stray' `
    (@(Get-TestProjectIdentityViolations -Projects @('ex/row', 'ex/stray') -Banked @('ex/row') -Excluded @() -Population $idPop) -join '; ')

$trackedCore = @(& git -C $repo ls-files -- 'src/core')
$gitExit = $LASTEXITCODE
Assert-Equal 'test-project identity: git ls-files read the tracked tree (exit 0)' 0 $gitExit
$testProjects = @($trackedCore |
    Where-Object { $_ -like 'src/core/*.tests.csproj' } |
    ForEach-Object { $_.Substring('src/core/'.Length, $_.LastIndexOf('/') - 'src/core/'.Length) } |
    Sort-Object -Unique)
$projectViolations = @(Get-TestProjectIdentityViolations -Projects $testProjects -Banked $rosterPackages `
    -Excluded $ledgerPackages -Population $population)

$excludedWithArtifacts = @($ledgerPackages | Where-Object { $testProjects -contains $_ } | Sort-Object)
$candidatesWithArtifacts = @($population | Where-Object {
        ($rosterPackages -notcontains $_) -and ($ledgerPackages -notcontains $_) -and ($testProjects -contains $_) } | Sort-Object)

# Printed UNCONDITIONALLY, both named sides, so the identity is re-addable from the output alone.
Write-Host ''
Write-Host 'tracked test projects vs the roster:' -ForegroundColor Cyan
Write-Host ('  {0} = {1} banked + {2} exclusion rows with artifacts + {3} rowless candidates' -f `
    $testProjects.Count, $rosterPackages.Count, $excludedWithArtifacts.Count, $candidatesWithArtifacts.Count)
Write-Host ('  exclusion rows with artifacts: {0}' -f ($excludedWithArtifacts -join ', '))
Write-Host ('  rowless candidates:            {0}' -f ($candidatesWithArtifacts -join ', '))

Assert-Equal 'test-project identity: the vacuity guard (a zero means git or the pathspec read nothing)' $true ($testProjects.Count -gt 0)
Assert-Equal 'test-project identity: every tracked tests.csproj is accounted for by name' '' ($projectViolations -join '; ')
Assert-Equal 'test-project identity: the named sides add up' $testProjects.Count `
    ($rosterPackages.Count + $excludedWithArtifacts.Count + $candidatesWithArtifacts.Count)

# ---- 2b3. a validated Tests badge sits on EXACTLY the roster rows that have a README (ruled 2026-09-22)
# A package's converted README is the page a reader lands on, so its Tests badge is a claim about the
# roster, and it must agree with the roster in BOTH directions:
#   - a banked row whose README carries no validated badge understates a validation (at the batch-7
#     stamp crypto/internal/fips140/aes, /ecdsa and /nistec were banked and still showed orange
#     not_yet_validated -- a badge that did not regenerate with its row);
#   - a validated badge on a package that is NOT a roster row claims a validation the roster does not
#     hold (crypto/internal/fips140deps, an EXCLUSION row, showed a green 1/1 linking a 1.23.12.3
#     snapshot that same day).
# The badge vocabulary measured over every README that day: `N/N_validated` (brightgreen),
# `not_yet_validated` (orange), `none_to_validate` (lightgrey) -- only the first claims validation.
#
# OUTSIDE THE CHECK BY CONSTRUCTION, named rather than failed: a banked row with no README at all has
# no badge to disagree with. At the stamp those were four test-only packages:
# crypto/internal/fips140test, embed/internal/embedtest, go/ast/internal/tests,
# internal/coverage/test. The README set is the TRACKED one (from git, like 2b2), so a lane's
# untracked scratch README cannot red it.
#
# Test-ReadmeAdvertisesValidated and Get-BadgeRosterViolations live in _roster.ps1 (2026-09-23),
# shared with push-nuget.ps1's release census; the fixtures below still drive them.

Assert-Equal 'badge: a validated Tests badge is recognised' $true `
    (Test-ReadmeAdvertisesValidated '[![Tests](https://img.shields.io/badge/Tests-1%2F1_validated-brightgreen?logo=go)](x)')
Assert-Equal 'badge: not_yet_validated is not a validation claim' $false `
    (Test-ReadmeAdvertisesValidated '[![Tests](https://img.shields.io/badge/Tests-not_yet_validated-orange?logo=go)](x)')
Assert-Equal 'badge: none_to_validate is not a validation claim' $false `
    (Test-ReadmeAdvertisesValidated '[![Tests](https://img.shields.io/badge/Tests-none_to_validate-lightgrey?logo=go)](x)')

# The set rule's contract, both directions, against fixtures.
Assert-Equal 'badge vs roster: agreement reports nothing' 0 `
    @(Get-BadgeRosterViolations -WithReadme @('ex/row', 'ex/cand') -Validated @('ex/row') -Banked @('ex/row', 'ex/noreadme') -Excluded @()).Count
Assert-Equal 'badge vs roster: a banked row without the badge is named' "banked row's README carries no validated Tests badge: ex/row" `
    (@(Get-BadgeRosterViolations -WithReadme @('ex/row') -Validated @() -Banked @('ex/row') -Excluded @()) -join '; ')
Assert-Equal 'badge vs roster: an exclusion row with the badge is named as one' 'validated Tests badge on an EXCLUSION row: ex/gone' `
    (@(Get-BadgeRosterViolations -WithReadme @('ex/gone') -Validated @('ex/gone') -Banked @() -Excluded @('ex/gone')) -join '; ')
Assert-Equal 'badge vs roster: a non-row with the badge is named' 'validated Tests badge on a non-row: ex/cand' `
    (@(Get-BadgeRosterViolations -WithReadme @('ex/cand') -Validated @('ex/cand') -Banked @() -Excluded @()) -join '; ')

$readmePackages = @($trackedCore |
    Where-Object { $_ -like 'src/core/*/README.md' } |
    ForEach-Object { $_.Substring('src/core/'.Length, $_.LastIndexOf('/') - 'src/core/'.Length) } |
    Sort-Object -Unique)
$validatedPackages = @($readmePackages | Where-Object {
        Test-ReadmeAdvertisesValidated ([System.IO.File]::ReadAllText((Join-Path (Join-Path $PSScriptRoot 'core') ($_ + '/README.md')))) })
$badgeViolations = @(Get-BadgeRosterViolations -WithReadme $readmePackages -Validated $validatedPackages `
    -Banked $rosterPackages -Excluded $ledgerPackages)
$rowsWithoutReadme = @($rosterPackages | Where-Object { $readmePackages -notcontains $_ } | Sort-Object)

Write-Host ''
Write-Host 'README Tests badges vs the roster:' -ForegroundColor Cyan
Write-Host ('  {0} tracked READMEs, {1} validated badges, {2} banked rows with a README' -f `
    $readmePackages.Count, $validatedPackages.Count, ($rosterPackages.Count - $rowsWithoutReadme.Count))
Write-Host ('  banked rows with NO README (outside the check): {0}' -f ($rowsWithoutReadme -join ', '))

Assert-Equal 'badge vs roster: the vacuity guard (a zero means the README walk read nothing)' $true ($readmePackages.Count -gt 0)
Assert-Equal 'badge vs roster: every README Tests badge agrees with the roster' '' ($badgeViolations -join '; ')

# ---- 2b4. the PROOF-PAGE identity's contract, against fixtures (2026-09-23, COORD ruling RN-6) ----
# current proof pages = rows by name + relocation anchors by link + exclusion rows by exclusion (the
# runbook's H10 close amendment, "THE RELEASE CENSUS, CORRECTED"). push-nuget.ps1's release
# pre-flight HOLDS it over docs/validation/current, and its fifth-number check holds it over the
# snapshot the freeze writes; the functions live in _roster.ps1, and this file is where _roster.ps1's
# contracts are pinned, so the arms are here. No live reading is taken HERE on purpose: the living
# pages already have two readers (the index tool and the release), and a third reader of one set is
# the drift the shared functions exist to remove.
$pageFixture = Get-ProofPageIdentity -Pages @('ex.row', 'ex.gone', 'ex.old') -Banked @('ex.row') `
    -Linked @('ex.row', 'ex.old') -Excluded @('ex.gone')
Assert-Equal 'page identity: a closed set reports nothing' '' ($pageFixture.Violations -join '; ')
Assert-Equal 'page identity: each page in ONE class, name before exclusion before link' 'ex.row|ex.gone|ex.old' `
    (($pageFixture.ByName -join ',') + '|' + ($pageFixture.ByExclusion -join ',') + '|' + ($pageFixture.ByLink -join ','))
Assert-Equal 'page identity: an ORPHAN page is named' `
    "proof page backed by nothing (no roster row by name, no exclusion row, no row's [proof] link): ex.stray" `
    ((Get-ProofPageIdentity -Pages @('ex.row', 'ex.stray') -Banked @('ex.row') -Linked @('ex.row') -Excluded @()).Violations -join '; ')
Assert-Equal 'page identity: an anchor never excuses a banked row with no page of its own' `
    'banked row has no proof page of its own: ex.row' `
    ((Get-ProofPageIdentity -Pages @('ex.old') -Banked @('ex.row') -Linked @('ex.old') -Excluded @()).Violations -join '; ')
Assert-Equal "page identity: a row's [proof] link that resolves to no page is named" `
    "a roster row's [proof] link resolves to no page: ex.gone" `
    ((Get-ProofPageIdentity -Pages @('ex.row') -Banked @('ex.row') -Linked @('ex.row', 'ex.gone') -Excluded @()).Violations -join '; ')

# The "by link" reader, in both spellings, and ROW lines only: prose and a placeholder admit nothing.
$ellipsis = [string][char]0x2026
$linkFixturePath = Join-Path ([System.IO.Path]::GetTempPath()) ('go2cs-link-fixture-' + [guid]::NewGuid().ToString('n') + '.md')
try {
    [System.IO.File]::WriteAllText($linkFixturePath, (@(
        '| Package | Tests | Disclosed | What it exercises |'
        '|:--|:--:|:--:|:--|'
        "| [``ex/row``](https://x/row) | 3 |  | Own page first. $dot [proof](validation/current/ex.row.md) $dot [proof](validation/current/ex.old.md) |"
        "| [``ex/two``](https://x/two) | 1 |  | Frozen spelling. $dot [proof](ex.two.md) $dot [proof]($ellipsis) |"
        ''
        'Prose is not a row: [proof](validation/current/ex.prose.md) and [proof](ex.prose.md).'
    ) -join "`r`n"), (New-Object System.Text.UTF8Encoding($false)))

    Assert-Equal 'row proof links: the LIVING spelling, from row lines only' 'ex.old,ex.row' `
        ((Get-RosterRowProofLinks -Path $linkFixturePath) -join ',')
    Assert-Equal 'row proof links: the FROZEN sibling spelling, from row lines only' 'ex.two' `
        ((Get-RosterRowProofLinks -Path $linkFixturePath -Frozen) -join ',')
}
finally {
    if (Test-Path $linkFixturePath) { Remove-Item $linkFixturePath -Force }
}

# ---- 2b5. the FROZEN roster's relative links, against fixtures (2026-09-24, COORD at the census seat's accept)
# ConvertTo-FrozenRosterText relocates EVERY relative, path-shaped link by '../../' -- any file type or
# a directory, where it used to relocate .md and .ps1 only and three 1.24.13 links (a directory, a .txt
# and a .py) would have published dangling in 1.24.13.1 behind a warning. Get-UnresolvedRelativeLinks
# names a relocated link that resolves to no tracked path; push-nuget.ps1 REFUSES both by name, in its
# pre-flight and again before it writes the frozen roster. Both functions are _roster.ps1's.
function Get-OrdinalJoin([string[]] $Items) {
    $list = New-Object System.Collections.Generic.List[string]
    foreach ($i in @($Items)) { if ($null -ne $i) { $list.Add($i) } }
    $list.Sort([System.StringComparer]::Ordinal)
    return ($list -join ',')
}

$relocFixture = ConvertTo-FrozenRosterText -Version '1.24.13.1' -Commit 'abc1234' -FrozenOn '2026-01-01' -RosterText ((@(
    '# Fixture roster'
    ''
    'A directory [d](phase4/evidence/), a text file [t](phase4/list.txt), a script [s](../src/tool.ps1), a page [p](validation/current/ex.row.md),'
    'a placeholder [x](url), an in-page anchor [a](#here), a site link [w](https://example.invalid/x.py).'
    ''
    '[ref]: phase4/data.py'
) -join "`n"))
Assert-Equal 'frozen roster: every relative path-shaped link relocates -- any file type, or a directory' `
    '../src/tool.ps1,phase4/data.py,phase4/evidence/,phase4/list.txt' (Get-OrdinalJoin $relocFixture.Relocated)
Assert-Equal 'frozen roster: a DIRECTORY link is relocated two levels up' $true ($relocFixture.Text.Contains('[d](../../phase4/evidence/)'))
Assert-Equal 'frozen roster: a .txt link is relocated two levels up' $true ($relocFixture.Text.Contains('[t](../../phase4/list.txt)'))
Assert-Equal 'frozen roster: a reference definition of any type is relocated' $true ($relocFixture.Text.Contains('[ref]: ../../phase4/data.py'))
Assert-Equal 'frozen roster: a placeholder, an in-page anchor and a URL are left exactly as written' $true (
    $relocFixture.Text.Contains('[x](url)') -and $relocFixture.Text.Contains('[a](#here)') -and
    $relocFixture.Text.Contains('[w](https://example.invalid/x.py)'))
Assert-Equal 'frozen roster: the proof link becomes its sibling page' $true ($relocFixture.Text.Contains('[p](ex.row.md)'))
Assert-Equal 'frozen roster: nothing path-shaped is left unrelocated' '' (Get-OrdinalJoin $relocFixture.Unrelocated)

$trackedFixture = @('docs/phase4/evidence/a.json', 'docs/phase4/list.txt', 'docs/README.md', 'src/tool.ps1')
Assert-Equal 'link resolution: a tracked file, a tracked directory either spelling, a fragment and an ../ walk resolve' '' `
    (@(Get-UnresolvedRelativeLinks -Targets @('phase4/evidence/', 'phase4/evidence', 'phase4/list.txt', '../src/tool.ps1', 'README.md#try') `
        -TrackedPaths $trackedFixture) -join '; ')
Assert-Equal 'link resolution: a missing path, a file spelled as a directory and a walk above the root are each NAMED' `
    '../../x.md (resolves to above the repository root); phase4/gone.txt (resolves to docs/phase4/gone.txt); phase4/list.txt/ (resolves to docs/phase4/list.txt)' `
    (@(Get-UnresolvedRelativeLinks -Targets @('phase4/gone.txt', 'phase4/list.txt/', '../../x.md') -TrackedPaths $trackedFixture) -join '; ')
Assert-Equal 'link resolution: an EMPTY tracked list resolves nothing (the check fails closed)' 1 `
    @(Get-UnresolvedRelativeLinks -Targets @('phase4/list.txt') -TrackedPaths @()).Count

# The Linux progress line is summed from the annotations exactly as the header above it is summed
# from the columns -- derived on both sides, so neither can drift from the table it describes.
# Three populations since the 2026-08-29 n/a ruling: validated-at-count (numeric annotation),
# permanently inapplicable (`linux: n/a` -- the package cannot exist there), and pending (no
# annotation). The header's honest denominator is the APPLICABLE rows -- the whole table minus the
# n/a set -- because a denominator silently containing rows no Linux can ever measure makes 100%
# unreachable and the line quietly dishonest against the parity goal.
$linuxRows = @($rows | Where-Object { $_.OS.ContainsKey('linux') -and $_.OS['linux'].Applicable })
$linuxNaRows = @($rows | Where-Object { $_.OS.ContainsKey('linux') -and -not $_.OS['linux'].Applicable })
$linuxTotal = 0
$linuxDisclosed = 0
foreach ($row in $linuxRows) {
    $linuxTotal += $row.OS['linux'].Expected
    $linuxDisclosed += $row.OS['linux'].Disclosed
}

Assert-Equal 'linux header: annotated row count' $linuxRows.Count `
    (Get-HeaderNumber $lines 'Linux:' 'Linux:\s*\*{0,2}(\d+)\s+of\s+(\d+)\s+applicable rows')
Assert-Equal 'linux header: denominator is the applicable table (whole minus n/a)' ($rows.Count - $linuxNaRows.Count) `
    (Get-HeaderNumber $lines 'Linux:' 'Linux:\s*\*{0,2}(\d+)\s+of\s+(\d+)\s+applicable rows' 2)
Assert-Equal 'linux header: matching verdicts equal the annotation sum' $linuxTotal `
    (Get-HeaderNumber $lines 'Linux:' '([\d,]+)\s+matching verdicts')
Assert-Equal 'linux header: disclosed equals the annotation sum' $linuxDisclosed `
    (Get-HeaderNumber $lines 'Linux:' '([\d,]+)\s+disclosed')
if ($linuxNaRows.Count -gt 0) {
    Assert-Equal 'linux header: the n/a count is stated, derived from the annotations' $linuxNaRows.Count `
        (Get-HeaderNumber $lines 'Linux:' '(\d+)\s+row(?:s)?\s+platform-exclusive')
}

# Every applicable annotation must be a real expectation, not a placeholder: a zero-count row would
# read as "validated at nothing" in the header's numerator. (The n/a form is the ONLY legal
# non-count annotation, and it is excluded above by construction.)
foreach ($row in $linuxRows) {
    Assert-Equal "annotation is a real count: $($row.Package)" $true ($row.OS['linux'].Expected -gt 0)
}

# ---- 2b. a disclosed count is backed by a COMMITTED manifest ----------------------------------------
# A row that banks `+ D` on any platform is making a claim about a file: the sweep can only absorb a
# pinned divergence through the package's go2cs_test_disclosures.json, read from the tree at run
# time. The hole this closes was measured 2026-09-03: debug/gosym banked `linux: 9 + 1` on 2026-08-29
# with its signature captured from the run and NO manifest committed beside the package (the bank
# commit changed this table alone), so the +1 could not reproduce on any host -- the Linux leveling
# re-sweep at 22d2bd9dc read the row 9 matched + 1 unabsorbed. Guard-as-calculator: the roster
# declares the count, the tree must hold the artifact the count rests on, and this line is where the
# two are compared. The Windows Disclosed column and every applicable per-OS `+ D` are checked alike,
# because the manifest is per package, not per platform.
#
# ⚠⚠ AND EXISTENCE WAS THE WHOLE OF IT UNTIL 2026-09-22, WHICH IS A GUARD-AS-CALCULATOR THAT NEVER
# READ THE CALCULATION. MEASURED on `database/sql` (Disclosed 2, two pins committed): DELETING ONE
# PIN from the manifest left this gate SILENT -- byte-identical output, still `2 of 638` -- and only
# REMOVING THE FILE fired it. A row may therefore claim any number it likes as long as some file
# exists beside the package, which is the shape of the hole debug/gosym opened, one level in.
#
# ⚠⚠ THE RELATION IS A CEILING, NOT AN EQUALITY, AND THE TREE IS WHAT SAYS SO. An equality check
# was the obvious form and it is FALSIFIED BY 15 OF THE 42 CLAIMING ROWS -- measured before it was
# written, which is the only reason it is not in this file. Both directions are legitimate:
#
#   pins < Disclosed   a PARENT of pinned SUBTESTS is itself a diverging verdict and is not pinned
#                      separately. `sync` pins TestOnceXGC/{OnceFunc,OnceValue,OnceValues} -- three
#                      pins, four disclosed. `testing` 14/15, `strconv` 10/11, `encoding/binary`
#                      8/9, `log/slog` 18/19, `net/netip` 54/57 (nine unpinned parents).
#   pins > Disclosed   a pin absorbs NOTHING on the platform the column describes -- `crypto/tls`
#                      commits 2 and banks 1, `os/signal` 3 and 2, `runtime/debug` 6 and 5. Whether
#                      such a pin is stale is the ORPHAN check's question, not this one's.
#
# So what binds is: the manifest must be able to ACCOUNT FOR the claim. The ceiling is the distinct
# pinned names plus the ancestors those names imply and do not themselves pin, and the roster's
# largest per-platform claim may not exceed it. Measured over the tree the day this landed: 40 rows
# with a manifest, ZERO over the ceiling, and the ceiling EXACTLY EQUAL on 34 of them -- so deleting
# a single pin from any of those 34 reds this guard by name, which is what the measurement above
# asked for. The two rows with no manifest at all are the fips140 relocation orphans, which this
# guard already named before this change and still names.
#
# Compared against the MAXIMUM across applicable platforms rather than per platform, because the
# manifest is per package and a pin absorbing on one platform is committed for all of them: the
# largest claim is the one the file has to cover, and the smaller ones follow.
function Get-DisclosureCeiling {
    param([string] $Path)

    $names = @(([System.IO.File]::ReadAllText($Path) | ConvertFrom-Json).disclosures |
               Where-Object { $null -ne $_ } |
               ForEach-Object { [string]$_.name } |
               Where-Object { -not [string]::IsNullOrWhiteSpace($_) })

    # ORDINAL, for the reason section 2c states about `platforms`: Go test names differ only by case
    # legally, and a case-folding set would merge two distinct pins into one and UNDERSTATE the
    # ceiling -- a guard failing in the direction that accuses a correct roster.
    $pinned = New-Object 'System.Collections.Generic.HashSet[string]' ([System.StringComparer]::Ordinal)
    foreach ($name in $names) { [void]$pinned.Add($name) }

    $implied = New-Object 'System.Collections.Generic.HashSet[string]' ([System.StringComparer]::Ordinal)
    foreach ($name in $names) {
        $parts = $name.Split([char]47)
        for ($i = 1; $i -lt $parts.Count; $i++) {
            $ancestor = ($parts[0..($i - 1)] -join '/')
            if (-not $pinned.Contains($ancestor)) { [void]$implied.Add($ancestor) }
        }
    }

    return @{ Pins = $pinned.Count; Implied = $implied.Count; Ceiling = ($pinned.Count + $implied.Count) }
}

foreach ($row in $rows) {
    $claims = @()
    if ($row.Disclosed -gt 0) { $claims += @{ Platform = 'windows'; Count = $row.Disclosed } }

    foreach ($key in $row.OS.Keys) {
        if ($row.OS[$key].Applicable -and $row.OS[$key].Disclosed -gt 0) {
            $claims += @{ Platform = $key; Count = $row.OS[$key].Disclosed }
        }
    }

    if ($claims.Count -eq 0) { continue }

    $manifest = Join-Path (Join-Path $PSScriptRoot 'core') ($row.Package + '/go2cs_test_disclosures.json')
    $present = Test-Path -LiteralPath $manifest
    Assert-Equal "disclosed count is backed by a committed go2cs_test_disclosures.json: $($row.Package)" $true $present

    # A ceiling cannot be read out of a file that is not there, and the assertion above has already
    # said so. Asserting a second time on the same absence would double-count one fault.
    if (-not $present) { continue }

    $largest = ($claims | Sort-Object { $_.Count } -Descending)[0]

    $ceiling = $null
    try { $ceiling = Get-DisclosureCeiling -Path $manifest }
    catch {
        Assert-Equal "go2cs_test_disclosures.json is readable for the ceiling: $($row.Package) ($($_.Exception.Message))" $true $false
        continue
    }

    Assert-Equal ("the manifest can account for the disclosed claim: $($row.Package) claims $($largest.Count) on $($largest.Platform), " +
                  "the manifest carries $($ceiling.Pins) pin(s) + $($ceiling.Implied) implied parent(s) = $($ceiling.Ceiling)") `
        $true ($largest.Count -le $ceiling.Ceiling)
}


# ---- 2c. every committed manifest's ENTRIES obey the deferred/structural contract ------------------
# The checks above ask whether the FILE exists and whether it holds ENOUGH pins to account for the
# row's claim; this asks whether what is in it is legal, which is a third question and the one the
# deferred class needs (coordinator ruling 2026-09-05, owner-ratified). The converter's loader enforces the same contract at compare time, so a broken
# entry fails a sweep -- but only for the row being swept, and only once someone sweeps it. This is
# the arm that reads all 168-odd committed entries across every row at once, so a mislabelled entry
# is caught the day it lands rather than at that row's next rebank.
#
# Read with ConvertFrom-Json rather than the ordinal readers _roster.ps1 uses for a COMPARISON
# record: those exist because a comparison record keys maps by TEST NAME, where two names differing
# only in case are distinct and a case-folding reader silently merges them. A manifest has no such
# map -- its entries are an array and the field names are fixed -- so the plain reader is correct on
# both editions here, and this comment is why the two differ rather than one having drifted.
$deferredClass = 'deferred'
$structuralClass = 'structural'
$manifestsChecked = 0
$entriesChecked = 0
$manifestParseFailures = 0

# Enumerated from the FILESYSTEM rather than from $rows, and that is the whole of this change.
# Reading the roster made this arm blind to a package that has no row -- which is the state EVERY
# package is in at the moment its manifest is first written, and therefore the moment that manifest
# is most likely to be wrong. Measured when this landed: 45 committed manifests, 3 of them
# (reflect, runtime, runtime/pprof) belonging to packages with no roster row. `os` was a fourth
# until it banked hours earlier, and its manifest went in unread by this arm in both directions --
# caught at the bank rather than after it, which is the only reason this is a fix and not a defect.
#
# The package name comes from the PATH by Split-Path and Substring rather than by regex: a package
# name is a path fragment, and deriving it with a pattern invites exactly the escaping mistakes that
# a guard file cannot afford.
# The PLATFORM SCOPE (increment 2 of the orphan-disclosure check). An entry may carry an optional
# `platforms` list scoping it to the targets it describes, so a per-platform retirement is
# expressible without touching another platform's absorption -- the durable fix doctrine rule (1)
# names. Absent or empty means every platform, which is why all 46 committed manifests are unaffected.
#
# Mirrored here from the Go loader for the same reason the floor rules are: the loader refuses a bad
# entry when its package is SWEPT, which is days after the merge and presents as a conversion error
# on a row rather than as a manifest defect. This walks the whole tree on every roster change.
#
# ORDINAL COMPARISON, deliberately. PowerShell's -contains and its hashtables are case-INSENSITIVE
# by default while the Go loader compares exactly, so a `platforms: ["Windows"]` entry would pass
# here and be refused there -- two guards disagreeing about one manifest, which is worse than either
# one being absent. -cnotcontains and an ordinal HashSet are what keep the two readings identical.
$disclosurePlatformTargets = @('windows', 'linux', 'darwin')

function Get-DisclosurePlatformViolations {
    param([string] $Name, $Platforms)

    if ($null -eq $Platforms) { return @() }

    $violations = New-Object System.Collections.Generic.List[string]
    $seen = New-Object 'System.Collections.Generic.HashSet[string]' ([System.StringComparer]::Ordinal)

    foreach ($platform in @($Platforms)) {
        $text = [string]$platform

        if ($disclosurePlatformTargets -cnotcontains $text) {
            [void]$violations.Add("$Name names platform '$text', which is not one of $($disclosurePlatformTargets -join ', ')")
            continue
        }
        if (-not $seen.Add($text)) {
            [void]$violations.Add("$Name names platform '$text' twice")
        }
    }

    return $violations.ToArray()
}

# Controlled BOTH ways against fixtures, because the tree cannot control it: no committed manifest
# carries a scope today, so the loop below fires zero assertions and a broken predicate would read
# exactly like a clean one. These arms are the only thing standing between that and a decoration.
Assert-Equal 'platforms: absent is legal, and is what every committed manifest carries' 0 `
    @(Get-DisclosurePlatformViolations -Name 'T' -Platforms $null).Count
Assert-Equal 'platforms: an empty list means every platform, not no platform' 0 `
    @(Get-DisclosurePlatformViolations -Name 'T' -Platforms @()).Count
Assert-Equal 'platforms: one corpus target is accepted' 0 `
    @(Get-DisclosurePlatformViolations -Name 'T' -Platforms @('linux')).Count
Assert-Equal 'platforms: all three corpus targets are accepted' 0 `
    @(Get-DisclosurePlatformViolations -Name 'T' -Platforms @('windows', 'linux', 'darwin')).Count
Assert-Equal 'platforms: a misspelled GOOS is refused (the likely mistake)' 1 `
    @(Get-DisclosurePlatformViolations -Name 'T' -Platforms @('windwos')).Count
Assert-Equal 'platforms: a real GOOS this corpus does not build is refused' 1 `
    @(Get-DisclosurePlatformViolations -Name 'T' -Platforms @('freebsd')).Count
Assert-Equal 'platforms: a duplicate is refused (evidence the list was edited unread)' 1 `
    @(Get-DisclosurePlatformViolations -Name 'T' -Platforms @('linux', 'linux')).Count
Assert-Equal 'platforms: case matters, because the Go loader compares ordinally' 1 `
    @(Get-DisclosurePlatformViolations -Name 'T' -Platforms @('Linux')).Count
Assert-Equal 'platforms: the refusal names the offending value, not merely that the entry is invalid' $true `
    ((@(Get-DisclosurePlatformViolations -Name 'T' -Platforms @('windwos')))[0] -like "*windwos*")

$coreRoot = Join-Path $PSScriptRoot 'core'
$manifestFiles = @(Get-ChildItem -LiteralPath $coreRoot -Recurse -File -Filter 'go2cs_test_disclosures.json' -ErrorAction SilentlyContinue | Sort-Object FullName)

foreach ($manifestFile in $manifestFiles) {
    $manifest = $manifestFile.FullName
    $pkg = (Split-Path $manifest -Parent).Substring($coreRoot.Length).TrimStart([char]92, [char]47).Replace([char]92, [char]47)

    $parsed = $null
    try { $parsed = [System.IO.File]::ReadAllText($manifest) | ConvertFrom-Json }
    catch { $manifestParseFailures++; Assert-Equal "go2cs_test_disclosures.json parses: $pkg" $true $false; continue }

    $manifestsChecked++

    foreach ($entry in @($parsed.disclosures)) {
        if ($null -eq $entry) { continue }
        $entriesChecked++
        $name = [string]$entry.name
        $class = [string]$entry.class

        if ($class -eq $deferredClass) {
            # Each field separately, so the failure NAMES the missing one.
            foreach ($field in 'want', 'reading', 'plan') {
                $value = [string]$entry.$field
                Assert-Equal "deferred entry names its $($field): $pkg/$name" $true (-not [string]::IsNullOrWhiteSpace($value))
            }
        }


        # The FLOOR (ruling 2026-09-05): a deferred entry may carry an object count GREATER than its
        # want, with its own proof sketch, naming the part of the reading no plan can remove. Same
        # three refusals as the loader, mirrored here so the whole tree is checked in one pass.
        $floor = 0
        if ($null -ne $entry.floor) { $floor = [int]$entry.floor }

        if ($floor -ne 0) {
            Assert-Equal "a floor belongs to a deferred entry, never a structural one: $pkg/$name" $false ($class -eq $structuralClass)
            Assert-Equal "floor is a positive object count: $pkg/$name" $true ($floor -gt 0)
            Assert-Equal "a floor names its proof (a claim the census can falsify): $pkg/$name" $true (-not [string]::IsNullOrWhiteSpace([string]$entry.proof))

            # The want must LEAD with the number the floor is compared against; refusing an
            # uncheckable pairing is the difference between a guard and a decoration.
            $wantText = ([string]$entry.want).Trim()
            $wantMatch = [regex]::Match($wantText, '^\d+')
            Assert-Equal "a floored entry's want leads with its number: $pkg/$name" $true $wantMatch.Success
            if ($wantMatch.Success) {
                Assert-Equal "floor exceeds the want (else nothing is deferred and the entry is structural): $pkg/$name" $true ($floor -gt [int]$wantMatch.Value)
            }
        }
        if ($class -eq $structuralClass) {
            Assert-Equal "structural entry names NO retirement plan (its claim is the assertion cannot be met): $pkg/$name" $true ([string]::IsNullOrWhiteSpace([string]$entry.plan))
        }

        # The PLATFORM SCOPE, conditional like the arms above: an entry with no `platforms` key fires
        # nothing, which is every entry in the tree today. That is why the fixture arms exist -- this
        # loop currently adds coverage without adding checks, exactly as the plain alloc-profile
        # entries do, and a scope arriving tomorrow is validated the day it lands.
        if ($null -ne $entry.platforms) {
            $platformViolations = @(Get-DisclosurePlatformViolations -Name "$pkg/$name" -Platforms $entry.platforms)
            Assert-Equal "platforms names only corpus targets, without duplicates: $pkg/$name ($($platformViolations -join '; '))" 0 $platformViolations.Count
        }
    }
}

# COVERAGE, and this is the assertion the row-enumerated form could not make. The vacuity guard below
# only asks whether the arm read SOMETHING; this asks whether it read EVERYTHING committed. They are
# different questions and the old form passed the first while failing the second silently, because a
# manifest belonging to an unbanked package produced no assertion and no absence anyone could see.
#
# It is also why the check count did not move when this landed: a plain `alloc-profile` entry fires
# no assertion at all, so reaching three more manifests adds coverage without adding checks. Without
# this line there would be no evidence in the output that the arm's reach had changed.
#
# ⚠ The reference count is enumerated INDEPENDENTLY, and the first version of this line was not.
# Comparing $manifestsChecked against $manifestFiles.Count compares the loop against its own input:
# it holds under ANY enumeration, including the row-based one this change replaces, so it passed a
# neutering control that should have reddened it. An assertion whose reference is derived from the
# thing under test measures nothing -- the corrective is a second derivation, and here that is a
# separate walk of the same tree.
$manifestsOnDisk = @(Get-ChildItem -LiteralPath $coreRoot -Recurse -File -Filter 'go2cs_test_disclosures.json' -ErrorAction SilentlyContinue).Count
Assert-Equal 'the manifest arm reaches every committed manifest, not just the banked rows' $manifestsOnDisk ($manifestsChecked + $manifestParseFailures)

# Vacuity guard: this arm is worthless if it silently checks nothing, which is exactly what a wrong
# path or a changed schema would produce. The corpus has committed manifests today, so a zero here is
# an instrument fault rather than a clean bill.
Assert-Equal "the manifest-entry arm actually read manifests (a zero would mean it checked nothing)" $true ($manifestsChecked -gt 0)
Assert-Equal "the manifest-entry arm actually read entries" $true ($entriesChecked -gt 0)
# ---- 2d. a prose figure never silently claims to be the live one --------------------------------
# COORD ruling 2026-09-06, made from its own misreading. This roster carried BOTH 202 and 203 in
# different voices -- the guard-recomputed header, and a dated derivation that was correct on the
# day it was written -- and nothing on the page said which was which. The stale one was quoted twice
# in rulings, and it is easy to see why: the sentence carrying it ended "recomputed by the format
# guard ... not hand-set", which is the most authoritative-sounding claim in the file, and it was
# false.
#
# The rule COORD asked for is a CHECK rather than a label, because a rule that lives in a mailbox
# post is one the next reader has to find first. A line stating a RATIO -- `N / M` with a percentage
# on the same line -- must EITHER match one of the two live figures computed above, OR carry an
# explicit `as of YYYY-MM-DD` on that same line. Nothing else about the line matters. A dated record
# therefore keeps its numbers at their own date, which is the whole point of a record: rewriting
# them to agree with today would destroy it rather than repair it. A figure that means to be live is
# recomputed like every other figure in the header, and cannot go stale in silence.
#
# Why the marker sits on the RATIO'S OWN LINE and not on its section heading: COORD reached the
# stale figure by SEARCH, not by reading downward, so a heading three paragraphs above it was never
# in the frame. A reader who lands on the number reads its date in the same sentence, or it is live.
#
# The escape is deliberately cheap -- four words -- because the alternative to a cheap escape is a
# guard that gets routed around. What it will not let you do is state a stale figure with no date.
$naiveLivePct = if ($testable -gt 0) { '{0:0.0}' -f [math]::Round(($rows.Count / [double]$testable) * 100, 1, [MidpointRounding]::AwayFromZero) } else { '' }
$honestLivePct = if ($implementable -gt 0) { '{0:0.0}' -f [math]::Round(($rows.Count / [double]$implementable) * 100, 1, [MidpointRounding]::AwayFromZero) } else { '' }
$liveRatios = @(
    @{ Num = $rows.Count; Den = $testable;      Pct = $naiveLivePct }
    @{ Num = $rows.Count; Den = $implementable; Pct = $honestLivePct }
)

$undatedStaleRatios = New-Object System.Collections.Generic.List[string]
$liveRatioLines = 0
foreach ($line in $lines) {
    if ($line -notmatch '(\d+)\s*/\s*(\d+)') { continue }
    $rNum = [int]$Matches[1]
    $rDen = [int]$Matches[2]
    if ($line -notmatch '([\d.]+)\s*%') { continue }
    $rPct = $Matches[1]

    $isLive = $false
    foreach ($live in $liveRatios) {
        if ($rNum -eq $live.Num -and $rDen -eq $live.Den -and $rPct -eq $live.Pct) { $isLive = $true; break }
    }
    if ($isLive) { $liveRatioLines++; continue }

    # The dated-record escape. A real ISO date is required rather than any prose carrying the words,
    # so "as of the last bank" buys no exemption.
    if ($line -match 'as of \d{4}-\d{2}-\d{2}') { continue }

    [void]$undatedStaleRatios.Add("$rNum / $rDen -- $($rPct)% :: $($line.Trim())")
}

Assert-Equal 'every prose ratio is either the live figure or carries `as of YYYY-MM-DD`' '' ($undatedStaleRatios -join '  |  ')

# Vacuity control, and it is the load-bearing half. A scan whose pattern stopped matching would
# report a clean sweep over a file it never read -- the false-empty census this project keeps paying
# for. The header states BOTH denominators, on two separate lines, so two live hits is the floor: if
# this ever reads under two, the check above proved nothing regardless of what it printed.
Assert-Equal 'ratio scan reached the header (vacuity control: both live figures found)' $true ($liveRatioLines -ge 2)

# ---- 2e. the README's featured NEWS block against the roster header ------------------------------
# 2d's rule stops a STALE figure inside this file. The same figures also live on the front page, in
# docs/README.md's featured NEWS block, and nothing compared the two: that block sat at 201/215,
# 27,734 matching and 154 disclosed across at least three banks, because it is hand-written prose in
# a file no arithmetic guard read. It is the first thing a visitor sees, so it is the worst place in
# the project for a stale number, and it is exactly the class 2d was minted for -- one document over.
#
# The roster header is the authority and this arm never re-derives it: every roster-side value below
# is the one section 2 already computed from the table (row count, column sums, the implementable
# subtraction, the Linux annotation sums) or parsed from the header ($testable, which the table
# cannot know). Section 2 has already asserted the header equals those, so a wrong header fails
# there, by its own name, rather than being propagated into this comparison.
#
# WHY THE BLOCK IS JOINED BEFORE IT IS PARSED, and it is the whole reason a per-line scan was not
# written: the block is hard-wrapped, and the wraps fall INSIDE the figures. At this tree "28,459"
# ends one line while "matching verdicts" begins the next, and "167 divergences" is split from
# "disclosed" the same way -- so a line-anchored pattern reads a well-formed ZERO on a file that
# plainly contains both. The lines are joined and their whitespace collapsed first; the patterns
# then match prose, not layout, and survive a re-wrap.
$readmeLines = [System.IO.File]::ReadAllLines((Join-Path $repo 'docs/README.md'))

$newsStart = -1
$newsEnd = -1
for ($i = 0; $i -lt $readmeLines.Count; $i++) {
    if ($newsStart -lt 0) {
        if ($readmeLines[$i] -match '^##\s' -and $readmeLines[$i] -match 'NEWS') { $newsStart = $i }
        continue
    }
    if ($readmeLines[$i] -match 'All announcements can be found') { $newsEnd = $i; break }
}

# Vacuity control, same shape as 2d's and for the same reason: if the heading is renamed or the
# archive line moves, the required headline below reads '(not found)' and fails by name (and every
# optional figure reads absent) -- but this states it in one line, so the report names the CAUSE
# rather than a symptom of it. Kept unchanged by the 2026-09-24 short-block change: an unlocated block
# must still fail, or every figure would read absent and pass.
Assert-Equal 'README featured NEWS block located (vacuity control: heading through archive line)' $true `
    (($newsStart -ge 0) -and ($newsEnd -gt $newsStart))

$newsText = ''
if ($newsStart -ge 0 -and $newsEnd -gt $newsStart) {
    $newsText = (($readmeLines[$newsStart..$newsEnd]) -join ' ') -replace '\s+', ' '
}

# ONE FIGURE REQUIRED, THE REST CHECKED IF PRESENT (owner order 2026-09-24). The featured block becomes
# a SHORT high-level summary with fewer statistics, and the full detail lives in docs/NEWS.md. Until
# then this arm REQUIRED all five figures, so an absent one read '(not found)' and failed, and a short
# block could not pass. What the guard still promises is unchanged: NO STALE FIGURE ON THE FRONT PAGE.
#   - the headline `N of the M testable` is REQUIRED: it is the one figure the owner keeps on the
#     front page, and an absent one fails as '(not found)', exactly as before;
#   - every OTHER figure is checked IF ITS PHRASING APPEARS: stated, it must equal the roster, whole
#     and by name, as before; unstated, the arm prints `(absent -- not stated in the block)` and
#     passes, because a figure the block does not state cannot be stale there.
# A figure with two parts (the implementable denominator and its percentage) is compared on the
# parts the block states; a part it does not state reads `(absent)` in the printout.
# The price is stated rather than hidden: a figure whose PHRASING drifts reads as absent, not stale.
# The phrasings below are the block's own; a re-worded figure needs its pattern re-worded with it,
# and the unconditional printout below is where a reader sees an expected figure reading absent.
function Get-NewsFigurePart {
    param([string] $Text, [string] $Pattern, [int] $Group = 1)

    # $null for a phrasing the block does not contain -- which the caller turns into '(not found)'
    # for the REQUIRED figure (a token that can never equal a roster figure, so it fails loudly) and
    # into an absent reading for the others.
    if ($Text -match $Pattern) { return (($Matches[$Group]) -replace ',', '') }
    return $null
}

$bankedPattern = '(\d+)\s+of\s+the\s+(\d+)\s+testable'
$linuxPattern = '(\d+)\s+of\s+the\s+(\d+)\s+applicable\s+rows'

# THE HEADLINE'S OWN PERCENTAGE (owner, 2026-09-24, ledger 1f226c6912). The block may state it in
# parentheses right after the headline counts -- "218 of the 230 testable standard-library packages
# (94.8%) pass ..." -- and no pattern read that position, so a stale one would have sat on the front
# page unwatched. Only a few words (letters, hyphens, spaces and bold asterisks; nothing that ends a
# clause) may sit between `testable` and the parenthesis, so a percentage later in the block is never
# read as this one. Its roster side is the HEADER's own figure (`N / M testable packages validated --
# X%`, parsed as $percentText in section 2, which asserts it follows from the counts); it is never
# recomputed here. Checked if present, like every figure but the headline counts.
$bankedPctPattern = '(\d+)\s+of\s+the\s+(\d+)\s+testable[A-Za-z\s*-]{0,60}?\(\s*([\d.]+)\s*%\s*\)'

$newsFigures = @(
    @{ Name = 'banked / testable packages'; Required = $true
       Parts = @(@{ Pattern = $bankedPattern; Group = 1; Roster = "$($rows.Count)" }
                 @{ Pattern = $bankedPattern; Group = 2; Roster = "$testable" }) }
    @{ Name = 'headline testable percentage'; Required = $false
       Parts = @(@{ Pattern = $bankedPctPattern; Group = 3; Roster = "$percentText" }) }
    @{ Name = 'matching verdicts'; Required = $false
       Parts = @(@{ Pattern = '([\d,]+)\s+matching\s+verdicts'; Group = 1; Roster = "$columnTotal" }) }
    @{ Name = 'disclosed divergences'; Required = $false
       Parts = @(@{ Pattern = '([\d,]+)\s+divergences\s+disclosed'; Group = 1; Roster = "$columnDisclosed" }) }
    @{ Name = 'implementable denominator / pct'; Required = $false
       Parts = @(@{ Pattern = 'denominator\s+is\s+\*{0,2}(\d+)\b'; Group = 1; Roster = "$implementable" }
                 @{ Pattern = 'roster\s+at\s+\*{0,2}([\d.]+)\s*%'; Group = 1; Roster = "$honestLivePct" }) }
    @{ Name = 'linux rows'; Required = $false
       Parts = @(@{ Pattern = $linuxPattern; Group = 1; Roster = "$($linuxRows.Count)" }
                 @{ Pattern = $linuxPattern; Group = 2; Roster = "$($rows.Count - $linuxNaRows.Count)" }) }
)

# Printed UNCONDITIONALLY, pass or fail, every figure including the absent ones. A comparison whose
# inputs are never shown is one nobody can tell apart from a comparison that did not happen.
Write-Host ''
Write-Host 'README featured NEWS block vs the roster header:' -ForegroundColor Cyan
foreach ($figure in $newsFigures) {
    # A List, not a pipeline: a pipeline drops the $null that marks an absent part and shifts the rest.
    $readings = New-Object System.Collections.Generic.List[object]
    foreach ($part in $figure.Parts) { $readings.Add((Get-NewsFigurePart $newsText $part.Pattern $part.Group)) }
    $rosterShown = (@($figure.Parts | ForEach-Object { $_.Roster }) -join '/')
    $stated = @($readings | Where-Object { $null -ne $_ }).Count

    if ($stated -eq 0 -and -not $figure.Required) {
        Write-Host ('  {0,-32} README {1,-14} roster {2}' -f $figure.Name, '(absent -- not stated in the block)', $rosterShown)
        continue
    }

    $readmeShown = (@(for ($p = 0; $p -lt $readings.Count; $p++) {
                if ($null -ne $readings[$p]) { $readings[$p] } elseif ($figure.Required) { '(not found)' } else { '(absent)' } }) -join '/')
    Write-Host ('  {0,-32} README {1,-14} roster {2}' -f $figure.Name, $readmeShown, $rosterShown)

    # Compared on the parts that bind: every part of the required figure, the stated parts of the rest.
    $expected = @()
    $actual = @()
    for ($p = 0; $p -lt $readings.Count; $p++) {
        if ($null -eq $readings[$p] -and -not $figure.Required) { continue }
        $expected += $figure.Parts[$p].Roster
        $actual += $(if ($null -ne $readings[$p]) { $readings[$p] } else { '(not found)' })
    }
    Assert-Equal "README featured NEWS block: $($figure.Name) matches the roster header" ($expected -join '/') ($actual -join '/')
}

# ---- 2f. a row's cells agree with its OWN page at the pin (ruled 2026-09-22) ----------------------
# The Tests column is the page's MATCHED count and the Disclosed column is the page's disclosed
# count -- the same pair the per-OS annotation spells "N + D". Nothing compared a row's cells with the
# page its [proof] link names, and at the 1.24.13 batch-7 stamp seven rows disagreed with their own
# pages in two ways:
#   - three rows put matched + disclosed in Tests (crypto/internal/fips140test 2267 for 2260,
#     crypto/sha3 23 for 18, internal/runtime/maps 111 for 3), so the header's "matching test
#     verdicts" silently counted 120 DISCLOSED verdicts as matching -- and section 2's header
#     assertion could not see it, because it sums the same wrong cells it is compared against;
#   - four rows carried a Disclosed count the re-banked page no longer reports (encoding/binary 8
#     for 6, net/netip 54 for 57, runtime/debug 6 for 5, strconv 10 for 11).
#
# WHICH PAGE: the row's FIRST [proof] link, which is the row's own evidence at the hop (an
# inheritance row keeps its retired anchor links AFTER its own page, as provenance). WHICH ROWS: only
# those whose first page was generated at the Go pin, read from src/version.props' GoStdLibVersion --
# the pin's source of truth -- so the arm moves with the next hop instead of naming this one. A row
# whose first page is older is COUNTED and printed, never silently skipped: it is a row the hop has not
# yet re-measured, and a gate cannot compare it against a page from a different Go.
#
# The headline parser returns NOTHING for a malformed headline, never zero: a page whose shape moved
# must fail by name here, not read as "0 matched" and fail as a count mismatch that points at the row.
function Get-ProofPageHeadline {
    param([string] $Text)

    $pattern = '(?m)^\*\*([\d,]+) matched ' + $dot + ' ([\d,]+) disclosed\*\* ' + $dash + ' Go (\S+?), '
    $m = [regex]::Match($Text, $pattern)
    if (-not $m.Success) { return $null }

    return [PSCustomObject]@{
        Matched   = [int]($m.Groups[1].Value -replace ',', '')
        Disclosed = [int]($m.Groups[2].Value -replace ',', '')
        GoVersion = $m.Groups[3].Value
    }
}

# The parser's contract, against fixture text, both ways -- the page shape is generated, so the one
# place it can be pinned down independently of today's pages is here.
$headlineFixture = "*Validated 2026-09-22*`n`n**2,260 matched $dot 7 disclosed** $dash Go 1.24.13, ``windows/amd64``, converted package"
$parsedHeadline = Get-ProofPageHeadline $headlineFixture
Assert-Equal 'page headline: matched, read through its thousands separator' 2260 $parsedHeadline.Matched
Assert-Equal 'page headline: disclosed' 7 $parsedHeadline.Disclosed
Assert-Equal 'page headline: the Go version the page was generated at' '1.24.13' $parsedHeadline.GoVersion
Assert-Equal 'page headline: a malformed headline reads as NOTHING, never as zero' $true `
    ($null -eq (Get-ProofPageHeadline "**7 matched** $dash Go 1.24.13, ``windows/amd64``"))

$versionProps = Join-Path $PSScriptRoot 'version.props'
$pinMatch = [regex]::Match([System.IO.File]::ReadAllText($versionProps), '<GoStdLibVersion>\s*([^<\s]+)\s*</GoStdLibVersion>')
Assert-Equal 'the Go pin reads from version.props (GoStdLibVersion)' $true $pinMatch.Success
$goPin = $pinMatch.Groups[1].Value

$pagesRoot = Join-Path $repo 'docs/validation'
$ownPageChecked = 0
$ownPageOlder = 0
$ownPageUnread = 0

foreach ($line in $lines) {
    if ($line -notmatch $RosterRowPattern) { continue }

    # Row fields FIRST, read by the parser's own pattern so the cells are the ones section 2 sums.
    $ownPkg = $Matches[1]
    $ownTests = [int]$Matches[2]
    $ownDisclosed = if ($Matches[3]) { [int]$Matches[3] } else { 0 }

    $link = [regex]::Match($line, '\[proof\]\((?:validation/)?(current/[^)\s]+)\)')
    if (-not $link.Success) {
        $ownPageUnread++
        Assert-Equal "own page: $ownPkg carries a [proof] link" $true $false
        continue
    }

    $rel = $link.Groups[1].Value
    $pagePath = Join-Path $pagesRoot $rel
    if (-not (Test-Path -LiteralPath $pagePath)) {
        $ownPageUnread++
        Assert-Equal "own page: the first [proof] page of $ownPkg exists ($rel)" $true $false
        continue
    }

    $headline = Get-ProofPageHeadline ([System.IO.File]::ReadAllText($pagePath))
    if ($null -eq $headline) {
        $ownPageUnread++
        Assert-Equal "own page: the first [proof] page of $ownPkg has a parseable headline ($rel)" $true $false
        continue
    }

    if ($headline.GoVersion -ne $goPin) { $ownPageOlder++; continue }

    $ownPageChecked++
    Assert-Equal "own page: $ownPkg Tests equals its page's MATCHED count ($rel)" $headline.Matched $ownTests
    Assert-Equal "own page: $ownPkg Disclosed equals its page's disclosed count ($rel)" $headline.Disclosed $ownDisclosed
}

# Printed UNCONDITIONALLY, like the README comparison: a gate whose reach is never shown cannot be
# told apart from one that reached nothing.
Write-Host ''
Write-Host ('rows against their own page at go{0}: {1} checked, {2} still link an older page first (not gated), {3} unread' -f $goPin, $ownPageChecked, $ownPageOlder, $ownPageUnread) -ForegroundColor Cyan

# Two derivations of one population: the rows section 2 parsed, and the rows this loop classified.
Assert-Equal 'own-page arm: every roster row is classified (checked + older + unread)' $rows.Count ($ownPageChecked + $ownPageOlder + $ownPageUnread)
Assert-Equal 'own-page arm: at least one row links a page generated at the pin (a zero means it checked nothing)' $true ($ownPageChecked -gt 0)

# ---- 2g. no LEGACY alloc-profile label survives in a row banked at the pin (ruled 2026-09-23) -----
# The H10 relabel (ledger 2026-09-23 03:37, X(1)-X(8) and O1-O5) retires the bare `alloc-profile`
# label: every allocation disclosure resolves to `deferred`, `structural` or `alloc-count-semantics`
# (docs/ConversionStrategies-Reference.md, "deferred and structural"). The loader still ACCEPTS the
# legacy label, so a row that has not re-swept keeps comparing; this arm is what makes the retirement
# hold for the rows that HAVE re-banked at the hop.
#
# WHICH ROWS: 2f's predicate -- a roster row whose FIRST [proof] page was generated at the Go pin read
# from src/version.props' GoStdLibVersion -- so the arm moves with the next hop instead of naming this
# one. A row whose own page is older than the pin is not gated here and is counted, as in 2f.
# WHICH ENTRIES: those IN SCOPE on the platform that page was banked on (the headline's `goos/arch`).
# An entry scoped away from that platform describes another leg's absorption -- encoding/binary's
# three value subtests and math/big TestNewIntAllocs are [linux, darwin] and are labelled or retired
# from the Linux leg (X(6)) -- and a Windows page cannot have re-measured it.
#
# Authored UNEXECUTED by C1 (a lane with no PowerShell), 2026-09-23. The i7 makes it red-first in
# batch 8d by plant: flip one in-scope entry of a pin-banked row back to alloc-profile, confirm this
# arm names that row and entry, then restore byte-identical. PREDICTED at the relabel's manifests
# (claude/c1-alloc-relabel): refused 0, unless `net` has banked at the pin before its two entries are
# labelled -- then it names exactly those two, which is X(6)'s close condition, not a defect here.
# reflect's 42 alloc-profile entries are not gated while reflect has no roster row; they will be
# refused here the day reflect banks at the pin, which is when they relabel (ledger 16:39). And while
# every row's headline page is a Windows page, the [linux, darwin]-scoped entries are never in scope
# here, so the Linux leg's obligation to label or retire them needs a gate of its own.
# CLOSED 2026-09-23 (G, on COORD's ruling of the Linux leg): all four RETIRED from the Linux reading at
# bb54ff0920 (claude/g-linux-leg-evidence b84e5d5bfa) -- the three TestSizeAllocs value subtests read
# pass/pass and TestNewIntAllocs fail/fail (Go fails it too), so none absorbed anything on any platform.
# They were the corpus's whole [linux, darwin]-scoped alloc-profile population, so no such gate is owed.
function Test-EntryInPlatformScope {
    param($Platforms, [string] $Goos)

    # Absent or empty means every platform: the loader's rule, which 2c's fixture arms pin.
    if ($null -eq $Platforms) { return $true }
    $list = @($Platforms)
    if ($list.Count -eq 0) { return $true }

    # Ordinal, like 2c, because the Go loader compares exactly.
    return ($list -ccontains $Goos)
}

function Get-LegacyAllocLabels {
    param($Entries, [string] $Goos)

    $names = New-Object System.Collections.Generic.List[string]
    foreach ($entry in @($Entries)) {
        if ($null -eq $entry) { continue }
        if ([string]$entry.class -cne 'alloc-profile') { continue }
        if (-not (Test-EntryInPlatformScope -Platforms $entry.platforms -Goos $Goos)) { continue }
        [void]$names.Add([string]$entry.name)
    }

    return $names.ToArray()
}

# The predicate's contract, against fixtures, both ways: at the relabel's manifests the tree refuses
# nothing, so the loop below fires no refusal and a broken predicate would read exactly like a clean one.
$legacyFixture = @(
    [PSCustomObject]@{ name = 'TestUnscoped'; class = 'alloc-profile' },
    [PSCustomObject]@{ name = 'TestEmptyScope'; class = 'alloc-profile'; platforms = @() },
    [PSCustomObject]@{ name = 'TestWindowsOnly'; class = 'alloc-profile'; platforms = @('windows') },
    [PSCustomObject]@{ name = 'TestLinuxDarwin'; class = 'alloc-profile'; platforms = @('linux', 'darwin') },
    [PSCustomObject]@{ name = 'TestDeferred'; class = 'deferred' },
    [PSCustomObject]@{ name = 'TestCountSemantics'; class = 'alloc-count-semantics' }
)
$legacyOnWindows = @(Get-LegacyAllocLabels -Entries $legacyFixture -Goos 'windows')
Assert-Equal 'legacy label: unscoped, empty-scoped and windows-scoped alloc-profile entries are refused on a windows page' 3 $legacyOnWindows.Count
Assert-Equal 'legacy label: the refusal NAMES the entry' $true ($legacyOnWindows -ccontains 'TestUnscoped')
Assert-Equal 'legacy label: an entry scoped away from the page platform is not refused' $false ($legacyOnWindows -ccontains 'TestLinuxDarwin')
Assert-Equal 'legacy label: a live label is never refused' $false (($legacyOnWindows -ccontains 'TestDeferred') -or ($legacyOnWindows -ccontains 'TestCountSemantics'))
$legacyOnLinux = @(Get-LegacyAllocLabels -Entries $legacyFixture -Goos 'linux')
Assert-Equal 'legacy label: on a linux page the linux-scoped entry IS refused and the windows-only one is not' $true `
    (($legacyOnLinux.Count -eq 3) -and ($legacyOnLinux -ccontains 'TestLinuxDarwin') -and -not ($legacyOnLinux -ccontains 'TestWindowsOnly'))

$legacyRowsRead = 0
$legacyRowsOlder = 0
$legacyEntriesRead = 0
$legacyRefused = 0

foreach ($line in $lines) {
    if ($line -notmatch $RosterRowPattern) { continue }
    $legacyPkg = $Matches[1]

    # A row with no [proof] link, a missing page or an unparseable headline is failed by name in 2f;
    # it is skipped here rather than failed twice.
    $link = [regex]::Match($line, '\[proof\]\((?:validation/)?(current/[^)\s]+)\)')
    if (-not $link.Success) { continue }
    $pagePath = Join-Path $pagesRoot $link.Groups[1].Value
    if (-not (Test-Path -LiteralPath $pagePath)) { continue }
    $pageText = [System.IO.File]::ReadAllText($pagePath)
    $headline = Get-ProofPageHeadline $pageText
    if ($null -eq $headline) { continue }
    if ($headline.GoVersion -ne $goPin) { $legacyRowsOlder++; continue }

    $goosMatch = [regex]::Match($pageText, '(?m)^\*\*[\d,]+ matched .*? Go \S+?, `([a-z0-9]+)/')
    Assert-Equal "legacy label: the pin-banked page of $legacyPkg names its platform" $true $goosMatch.Success
    if (-not $goosMatch.Success) { continue }
    $pageGoos = $goosMatch.Groups[1].Value
    $legacyRowsRead++

    # A row with no manifest carries no label to refuse; 2b owns whether it should have one.
    $manifest = Join-Path $coreRoot ($legacyPkg + '/go2cs_test_disclosures.json')
    if (-not (Test-Path -LiteralPath $manifest)) { continue }
    $parsed = $null
    try { $parsed = [System.IO.File]::ReadAllText($manifest) | ConvertFrom-Json }
    catch { continue }   # 2c fails an unparseable manifest by name

    $legacyEntries = @($parsed.disclosures)
    $legacyEntriesRead += $legacyEntries.Count
    foreach ($legacyName in @(Get-LegacyAllocLabels -Entries $legacyEntries -Goos $pageGoos)) {
        $legacyRefused++
        Assert-Equal "legacy label: $legacyPkg/$legacyName is banked at go$goPin on $pageGoos and still carries alloc-profile (relabel it deferred, structural or alloc-count-semantics)" $true $false
    }
}

# Printed UNCONDITIONALLY, like 2f: a gate whose reach is never shown cannot be told apart from one
# that reached nothing.
Write-Host ('legacy alloc-profile in rows banked at go{0}: {1} rows read ({2} entries), {3} rows older than the pin (not gated), {4} refused' -f $goPin, $legacyRowsRead, $legacyEntriesRead, $legacyRowsOlder, $legacyRefused) -ForegroundColor Cyan
Assert-Equal 'legacy-label arm: it read at least one row banked at the pin (a zero means it checked nothing)' $true ($legacyRowsRead -gt 0)
Assert-Equal 'legacy-label arm: it read at least one manifest entry (a zero means the manifests were never opened)' $true ($legacyEntriesRead -gt 0)

# ---- 3. the RENDERED table's column integrity -----------------------------------------------------
# Everything above guards what the roster MEANS to the parser. This guards what it LOOKS LIKE to a
# reader, which nothing else does -- and the two can disagree silently.
#
# A literal '|' inside a cell ends that cell early: GFM splits a table row on '|' BEFORE inline code
# spans are resolved, so backticks do not protect it. The `log` row carried (`63`|`65`) -- two
# alternative line numbers -- and spilled its description into a phantom fifth column on the
# published page (owner-reported 2026-08-25, escaped in the same change that added this check).
#
# Why no existing gate could catch it: `_roster.ps1` anchors on the LEADING cells, so the broken row
# parses correctly, every arithmetic assertion above it passes, and the sweep's verdicts are right.
# The damage is confined to the rendered page, which is why it survived until a human looked at one.
# A well-formed four-column row has exactly five UNESCAPED pipes; the lookbehind keeps a deliberate
# \| in prose legal, which is also the fix.
foreach ($line in $lines) {
    if ($line -notmatch $RosterRowPattern) { continue }

    $rowPackage = $Matches[1]
    $pipes = [regex]::Matches($line, '(?<!\\)\|').Count

    Assert-Equal "row renders four columns: $rowPackage" 5 $pipes
}

# The exclusion ledger is the same kind of visitor-facing table with the same hazard, five columns
# wide -- its Mechanism cells are exactly the prose an unescaped '|' would one day land in.
#
# Scoped through Get-ExclusionLedgerSectionLines exactly as the parse above it is, and for the same
# reason: unscoped, this loop asserted a FIVE-column shape on the H10 relocation map's three- and
# four-column tables and reported 14 failures that were all one defect in this loop's own address.
foreach ($line in (Get-ExclusionLedgerSectionLines -Lines $lines)) {
    if ($line -notmatch $ExclusionLedgerRowPattern) { continue }

    $rowPackage = $Matches[1]
    $pipes = [regex]::Matches($line, '(?<!\\)\|').Count

    Assert-Equal "ledger row renders five columns: $rowPackage" 6 $pipes
}

if ($List) {
    Write-Host ''
    Write-Host 'per-OS annotations in the roster:' -ForegroundColor Cyan
    foreach ($row in ($rows | Where-Object { $_.OS.Count -gt 0 } | Sort-Object Package)) {
        foreach ($key in ($row.OS.Keys | Sort-Object)) {
            $windows = "windows $($row.Expected)" + $(if ($row.Disclosed) { " + $($row.Disclosed)" } else { '' })
            $osText = "$key $($row.OS[$key].Expected)" + $(if ($row.OS[$key].Disclosed) { " + $($row.OS[$key].Disclosed)" } else { '' })
            Write-Host ('  {0,-34} {1,-16} {2}' -f $row.Package, $windows, $osText)
        }
    }

    Write-Host ''
    Write-Host 'per-row execution configs in the roster:' -ForegroundColor Cyan
    foreach ($row in ($rows | Where-Object { $_.Execution } | Sort-Object Package)) {
        Write-Host ('  {0,-34} {1,-16} {2}' -f $row.Package, $row.Execution, ((@(Get-RosterExecutionArgs $row.Execution)) -join ' '))
    }
}

Write-Host ''
if ($failures.Count -gt 0) {
    Write-Host "roster format guard: $($failures.Count) of $checks checks FAILED" -ForegroundColor Red
    $failures | ForEach-Object { Write-Host "  $_" -ForegroundColor Red }
    exit 1
}

$executionRows = @($rows | Where-Object { $_.Execution })
Write-Host "roster format guard: $checks checks pass ($($rows.Count) rows, $($linuxRows.Count) with a linux annotation, $($executionRows.Count) with an execution config, $($ledger.Count) excluded)" -ForegroundColor Green
exit 0
