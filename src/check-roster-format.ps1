<#
.SYNOPSIS
    Guards the validated-package roster's machine-parsed format and its arithmetic.

.DESCRIPTION
    Two things this checks, both cheap enough to run at any time (pure text, no build, no gate):

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
function Read-FixtureLedger {
    param([string[]] $Rows)

    $header = @(
        '| Package | Verdicts | Class | Mechanism | Rooting |'
        '|:--|:--:|:--:|:--|:--:|'
    )
    $path = Join-Path ([System.IO.Path]::GetTempPath()) ('go2cs-ledger-fixture-' + [guid]::NewGuid().ToString('n') + '.md')

    try {
        [System.IO.File]::WriteAllText($path, (($header + $Rows) -join "`r`n"), (New-Object System.Text.UTF8Encoding($false)))
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

Assert-Equal 'capability-absent: the clean collapse is accepted' $true `
    (Test-CapabilityAbsentDelta -Expected 4 -Disclosed 0 -Block $block -Got 1 -Comparison $absentComparison -BankedNames $fullBankedNames).Accepted
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
    (Test-CapabilityAbsentDelta -Expected 4 -Disclosed 0 -Block $block -Got 1 -Comparison ([PSCustomObject]@{
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
    (Test-CapabilityAbsentDelta -Expected 4 -Disclosed 0 -Block $block -Got 2 -Comparison $absentComparison -BankedNames $fullBankedNames).Accepted
Assert-Equal 'capability-absent: a surplus (the surplus mechanism''s job, not this one''s) is refused' $false `
    (Test-CapabilityAbsentDelta -Expected 4 -Disclosed 0 -Block $block -Got 5 -Comparison $fullComparison -BankedNames $fullBankedNames).Accepted
Assert-Equal 'capability-absent: a subtest surviving alongside the collapse is refused, not absorbed' $false `
    (Test-CapabilityAbsentDelta -Expected 4 -Disclosed 0 -Block $block -Got 1 -Comparison ([PSCustomObject]@{
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
    (Test-CapabilityAbsentDelta -Expected 4 -Disclosed 1 -Block $block -Got 1 -Comparison ([PSCustomObject]@{
        go = New-VerdictMap @{ TestOther = 'pass'; TestPinned = 'pass'; TestFakeSuite = 'skip' }
        csharp = New-VerdictMap @{ TestOther = 'pass'; TestPinned = 'fail'; TestFakeSuite = 'skip' }
        disclosed = @('TestPinned (alloc-profile): x', 'TestOther (alloc-profile): y')
    }) -BankedNames @('TestOther', 'TestPinned', 'TestFakeSuite', 'TestFakeSuite/case1', 'TestFakeSuite/case2')).Accepted
Assert-Equal 'capability-absent: an unaccounted extra live verdict is refused' $false `
    (Test-CapabilityAbsentDelta -Expected 4 -Disclosed 0 -Block $block -Got 1 -Comparison ([PSCustomObject]@{
        go = New-VerdictMap @{ TestOther = 'pass'; TestFakeSuite = 'skip'; TestRogue = 'pass' }
        csharp = New-VerdictMap @{ TestOther = 'pass'; TestFakeSuite = 'skip'; TestRogue = 'pass' }
    }) -BankedNames $fullBankedNames).Accepted

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
foreach ($row in $rows) {
    $needsManifest = ($row.Disclosed -gt 0)

    foreach ($key in $row.OS.Keys) {
        if ($row.OS[$key].Applicable -and $row.OS[$key].Disclosed -gt 0) { $needsManifest = $true }
    }

    if (-not $needsManifest) { continue }

    $manifest = Join-Path (Join-Path $PSScriptRoot 'core') ($row.Package + '/go2cs_test_disclosures.json')
    Assert-Equal "disclosed count is backed by a committed go2cs_test_disclosures.json: $($row.Package)" $true (Test-Path -LiteralPath $manifest)
}

# ---- 2c. a prose figure never silently claims to be the live one --------------------------------
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
foreach ($line in $lines) {
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
