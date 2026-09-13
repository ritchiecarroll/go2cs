<#
.SYNOPSIS
    The H6 completeness gate: no migration's corpus is adopted until every hand-own in the RE-MEASURED
    census has a classified delta record in this migration's audit file.

.DESCRIPTION
    The runbook (docs/GoCorpusMigration.md, H6) states the gate; the audit file's section 3 states it
    mechanically in six numbered assertions, and this script implements those six LITERALLY:

      1. re-measure the line-anchored census over src/core;
      2. assert every marked path appears EXACTLY ONCE in the audit's row table;
      3. assert every row's class is one of unchanged / a / b / c;
      4. assert every 'b' carries a non-empty reason and every 'c' a work-item reference;
      5. assert ZERO rows in the "no .auto emitted" state;
      6. exit non-zero on any violation.

    Its only inputs are the audit file and the tree. It decides whether the audit is COMPLETE; it does
    not decide whether any row's class is CORRECT, which is a reading no script can take.

    RE-MEASURE, NEVER CARRY. Assertion 1 runs the predicate over the tree being adopted rather than
    trusting the count the audit file records. A population carried from an earlier measurement is the
    failure this gate exists to prevent: the audit would then be complete with respect to a census
    nobody re-ran, which reads exactly like completeness.

    A SKELETON REFUSES, AND THAT IS CORRECT. Every class in a skeleton is blank, so assertion 3 refuses
    it by name. The gate runs at FILL time; a skeleton passing would mean the gate cannot tell an
    unfilled audit from a finished one.

    PURE ASCII on purpose: Windows PowerShell 5.1 decodes a UTF-8 file without a BOM as the system
    codepage, so a non-ASCII character in this file can change what a pattern matches depending on the
    edition that runs it. The em dash the audit file uses for a blank cell is written as a code point
    here rather than as a character, for the same reason.

.PARAMETER AuditFile
    The migration's audit file. Default: docs/phase4/AUDIT-h6-handown-go124.md

.PARAMETER CorePath
    The corpus root the census is re-measured over. Default: src/core

.PARAMETER SelfTest
    Run the self-test instead of the gate: a hermetic repository in a temp directory, every arm
    RED-FIRST, including the floor-13 control the ruling names -- one marked path regressed out of the
    audit, the gate refusing and NAMING it.

.OUTPUTS
    Exit 0 = the audit is complete for the re-measured census.
    Exit 1 = at least one assertion violated; every violation is printed with the path it is about.
    Exit 2 = misuse or instrument failure (a census that returns nothing, an unreadable audit file, a
             row table that cannot be found). Never confused with a clean pass.
#>
[CmdletBinding()]
param(
    [string] $AuditFile = 'docs/phase4/AUDIT-h6-handown-go124.md',
    [string] $CorePath = 'src/core',
    [switch] $SelfTest
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

# The four classes, from the audit file's own section 2. Named once.
$script:ValidClasses = @('unchanged', 'a', 'b', 'c')

# The "no .auto emitted" sentinel (assertion 5). The audit file states the STATE -- "a row whose
# hand-own got no .auto emitted is a DEFECT in the audit, not a pass" -- but the skeleton fixes no
# SPELLING for it, because no row has been filled yet. This is the spelling this gate reads, declared
# here rather than buried in a regex; it is matched in either sha256 column or in the reason.
$script:NoAutoSentinel = 'no .auto emitted'

# What counts as a WORK-ITEM REFERENCE for a 'c' row (assertion 4). A 'c' is REWRITE OWED, and the
# audit requires "a named work item, gating the migration or deferred with owner and reason" -- so a
# non-empty cell is not enough, because "TODO" is non-empty. These are the reference shapes this
# repository actually uses. Refusing is the safe direction: a reason this gate does not recognise costs
# one rewrite, while a bare TODO that passes costs the migration the very row it was meant to gate.
$script:WorkItemPatterns = @(
    'OQ-\d+',                       # an open question in a docs record
    'BOARD',                        # a BOARD entry
    'train\s+\d+',                  # a train seat
    'claude/[A-Za-z0-9._-]+',       # a lane branch
    '#\d+',                         # an issue or PR
    '\b[0-9a-f]{9,40}\b',           # a commit SHA
    '\bH\d+[a-z]?\b'                # a ladder rung: a c row's work item may be the hop step that retires it
)

# Everything the gate prints goes here as well as to the host, so an arm can assert on the REASON a
# refusal gave rather than only on its exit code. An arm that checks the code alone cannot tell a
# refusal for the right reason from one for the wrong reason, and the two look identical in a log.
$script:GateLog = New-Object System.Collections.Generic.List[string]

function Write-Gate {
    param([string] $Message)
    $script:GateLog.Add($Message) | Out-Null
    Write-Host $Message
}

function Add-Violation {
    param([string] $Assertion, [string] $Subject, [string] $Detail)
    $script:Violations.Add([pscustomobject]@{ Assertion = $Assertion; Subject = $Subject; Detail = $Detail }) | Out-Null
    Write-Gate ("  VIOLATION [{0}] {1} -- {2}" -f $Assertion, $Subject, $Detail)
}

# --- 1. The census, RE-MEASURED --------------------------------------------------------------------
# The predicate is NOT spelled here. It lives in _paths.ps1 as Get-HandOwnMarkedPath -- the same
# function handown-census.ps1 defines the audit POPULATION with -- because two predicates that are
# meant to agree and are written out twice are two predicates that will differ, and if these two ever
# differed this gate would report an audit complete with respect to a population the census never had.
# The BOM tolerance and the reason it is -P rather than an ERE escape are documented at that function.
. (Join-Path $PSScriptRoot '_paths.ps1')

function Get-MarkedPath {
    param([string] $Core)

    if (-not (Test-Path -LiteralPath $Core)) {
        throw "corpus root not found: $Core"
    }
    $marked = @(Get-HandOwnMarkedPath -CoreRoot $Core)
    # A census that returns nothing has not found a clean tree; it has failed to LOOK. Exit 2, never 0.
    if ($marked.Count -eq 0) {
        throw "marker census returned ZERO over $Core -- wrong directory, untracked tree, or broken git grep. A census that scanned nothing passes everything."
    }
    return $marked
}

# --- 2. The audit's row table ----------------------------------------------------------------------
# Rows are the lines that open with a row NUMBER, so the table header, its separator and every other
# table in the file are excluded by SHAPE rather than by line position -- a gate keyed on line numbers
# goes wrong silently the first time the record gains a paragraph.
function Get-AuditRow {
    param([string] $Path)

    if (-not (Test-Path -LiteralPath $Path)) {
        throw "audit file not found: $Path"
    }
    $lines = @(Get-Content -LiteralPath $Path -Encoding UTF8)
    $rows = New-Object System.Collections.Generic.List[object]
    foreach ($line in $lines) {
        if ($line -notmatch '^\s*\|\s*\d+\s*\|') { continue }
        $cells = $line.Split('|')
        # '| a | b |' splits to ['', ' a ', ' b ', ''], so ten data cells means twelve elements.
        if ($cells.Count -lt 12) { continue }
        $rows.Add([pscustomobject]@{
            Number   = $cells[1].Trim()
            Path     = $cells[2].Trim().Trim([char]0x60).Trim()
            AutoFrom = $cells[7].Trim()
            AutoTo   = $cells[8].Trim()
            Class    = $cells[9].Trim().Trim([char]0x60).Trim()
            Reason   = $cells[10].Trim()
        }) | Out-Null
    }
    if ($rows.Count -eq 0) {
        throw "no rows parsed from $Path -- the row table was not found, and a gate over zero rows passes everything."
    }
    return $rows
}

function Test-BlankCell {
    param([string] $Value)
    if ([string]::IsNullOrWhiteSpace($Value)) { return $true }
    $v = $Value.Trim()
    if ($v -eq '-') { return $true }
    if ($v -eq ([string][char]0x2014)) { return $true }   # the em dash the audit uses for a blank cell
    return $false
}

function Invoke-Gate {
    param([string] $Audit, [string] $Core)

    $script:Violations = New-Object System.Collections.Generic.List[object]
    $script:GateLog.Clear()

    Write-Gate '== H6 completeness gate'
    Write-Gate ("   audit : {0}" -f $Audit)
    Write-Gate ("   corpus: {0}" -f $Core)

    # @( ) on BOTH, and not decoration: PowerShell unrolls a returned collection into the pipeline, so
    # a ONE-row audit arrives as a single object and `.Count` throws under StrictMode. A one-row audit
    # is not a corner case -- it is the shape of the floor-13 control below, which is what caught this.
    $marked = @(Get-MarkedPath -Core $Core)
    $rows = @(Get-AuditRow -Path $Audit)
    Write-Gate ("   census RE-MEASURED: {0} marked hand-own(s); audit carries {1} row(s)" -f $marked.Count, $rows.Count)

    # ASSERTION 2 -- every marked path appears EXACTLY ONCE.
    $byPath = @{}
    foreach ($r in $rows) {
        if (-not $byPath.ContainsKey($r.Path)) { $byPath[$r.Path] = 0 }
        $byPath[$r.Path] = $byPath[$r.Path] + 1
    }
    foreach ($p in $marked) {
        if (-not $byPath.ContainsKey($p)) {
            Add-Violation -Assertion 'A2-missing' -Subject $p -Detail 'marked hand-own has NO row in the audit'
        } elseif ($byPath[$p] -ne 1) {
            Add-Violation -Assertion 'A2-duplicate' -Subject $p -Detail ("appears {0} times in the audit, want exactly once" -f $byPath[$p])
        }
    }

    # REPORTED, NOT ASSERTED, and printed so the silence is not read as an assertion: a row naming a
    # path the re-measured census no longer marks. The ruled gate is the six assertions implemented
    # literally, and "every marked path has a row" does not say "every row has a marked path" -- a
    # stale row is a record question rather than a completeness one. A reader who saw no line here
    # would otherwise conclude the gate had checked it.
    $orphans = @($rows | Where-Object { $marked -notcontains $_.Path })
    if ($orphans.Count -gt 0) {
        Write-Gate ("  NOTE: {0} audit row(s) name a path the re-measured census does not mark (REPORTED, not one of the six):" -f $orphans.Count)
        foreach ($o in $orphans) { Write-Gate ("        {0}" -f $o.Path) }
    }

    foreach ($r in $rows) {
        # ASSERTION 3 -- the class is one of the four.
        if ($script:ValidClasses -notcontains $r.Class) {
            $shown = $r.Class
            if (Test-BlankCell -Value $shown) { $shown = '(blank)' }
            Add-Violation -Assertion 'A3-class' -Subject $r.Path -Detail ("class is {0}, want one of {1}" -f $shown, ($script:ValidClasses -join '/'))
        }

        # ASSERTION 4 -- 'b' needs its reason written out; 'c' needs a NAMED work item.
        if ($r.Class -eq 'b') {
            if (Test-BlankCell -Value $r.Reason) {
                Add-Violation -Assertion 'A4-b-reason' -Subject $r.Path -Detail 'class b carries no reason; the audit requires the reason written out, never the bare letter'
            }
        }
        if ($r.Class -eq 'c') {
            $matched = $false
            foreach ($pat in $script:WorkItemPatterns) {
                if ($r.Reason -match $pat) { $matched = $true; break }
            }
            if (-not $matched) {
                Add-Violation -Assertion 'A4-c-workitem' -Subject $r.Path -Detail 'class c carries no recognisable work-item reference (OQ-n, BOARD, train n, a lane branch, #n, or a commit SHA)'
            }
        }

        # ASSERTION 5 -- zero rows in the "no .auto emitted" state.
        $blob = ($r.AutoFrom + ' ' + $r.AutoTo + ' ' + $r.Reason)
        if ($blob -match [regex]::Escape($script:NoAutoSentinel)) {
            Add-Violation -Assertion 'A5-no-auto' -Subject $r.Path -Detail 'row records the "no .auto emitted" state, which the audit calls a DEFECT in the audit, never a pass'
        }
    }

    # ASSERTION 6 -- the verdict is DERIVED from the count and the exit follows it. A verdict string
    # printed beside a count it does not read is a check that cannot go red.
    $n = $script:Violations.Count
    if ($n -eq 0) {
        Write-Gate ("==> H6 AUDIT COMPLETE: {0} marked hand-own(s), {1} row(s), 0 violations" -f $marked.Count, $rows.Count)
        return 0
    }
    Write-Gate ("==> H6 AUDIT INCOMPLETE: {0} violation(s) over {1} marked hand-own(s) and {2} row(s)" -f $n, $marked.Count, $rows.Count)
    return 1
}

# --- The self-test ---------------------------------------------------------------------------------
# UTF-8 with NO byte-order mark, on both editions. See the note at the fixture for what a BOM does to a
# line-anchored predicate.
function Write-NoBom {
    param([string] $Path, [string[]] $Text)
    $enc = New-Object System.Text.UTF8Encoding($false)
    [System.IO.File]::WriteAllLines($Path, [string[]]$Text, $enc)
}

# UTF-8 WITH a byte-order mark, for the one fixture that needs to be hidden from a BOM-blind predicate.
function Write-WithBom {
    param([string] $Path, [string[]] $Text)
    $enc = New-Object System.Text.UTF8Encoding($true)
    [System.IO.File]::WriteAllLines($Path, [string[]]$Text, $enc)
}

function Invoke-Arm {
    param([string] $Name, [string] $Audit, [string] $Core, [int] $WantExit, [string] $WantText)

    $rc = 0
    try {
        $rc = Invoke-Gate -Audit $Audit -Core $Core
    } catch {
        $script:GateLog.Add($_.Exception.Message) | Out-Null
        $rc = 2
    }
    $out = ($script:GateLog -join "`n")

    $ok = ($rc -eq $WantExit)
    if ($ok -and $WantText -ne '') { $ok = ($out -match [regex]::Escape($WantText)) }
    if ($ok) {
        $script:ArmsPassed++
        Write-Host ("  ok   {0}" -f $Name)
    } else {
        $script:ArmsFailed++
        Write-Host ("  FAIL {0}: wanted exit {1} and text '{2}', got exit {3}" -f $Name, $WantExit, $WantText, $rc)
        Write-Host $out
    }
}

function Invoke-SelfTest {
    $tmp = Join-Path ([System.IO.Path]::GetTempPath()) ("h6gate-" + [guid]::NewGuid().ToString('N'))
    $script:ArmsPassed = 0
    $script:ArmsFailed = 0
    try {
        $core = Join-Path $tmp 'src/core'
        New-Item -ItemType Directory -Path (Join-Path $core 'pkga') -Force | Out-Null
        New-Item -ItemType Directory -Path (Join-Path $core 'pkgb') -Force | Out-Null
        # NO BOM, and measured rather than assumed. Windows PowerShell 5.1's `Set-Content -Encoding
        # UTF8` writes a byte-order mark, and the census predicate is LINE-ANCHORED: with a BOM the
        # first line begins EF BB BF rather than '[', the anchor does not match, and the fixture reads
        # ZERO marked files. That is what the first run of this self-test did -- the fixture's own
        # precondition caught it, which is the whole reason the precondition is asserted here instead
        # of trusted. A BOM-bearing hand-own would be invisible to the census for the same reason.
        Write-NoBom -Path (Join-Path $core 'pkga/one.cs') -Text '[module: GoManualConversion]'
        Write-NoBom -Path (Join-Path $core 'pkgb/two.cs') -Text '[module: go.GoManualConversion]'
        Write-NoBom -Path (Join-Path $core 'pkgb/plain.cs') -Text '// not a hand-own'
        # The BOM-hidden hand-own: a byte-order mark, then the marker on LINE 1. Invisible to a
        # line-anchored predicate that does not tolerate the mark, because the line begins EF BB BF
        # rather than '['. Arm 12 is what this file exists for after the 2026-09-13 ruling, and it is
        # the one arm here that reads the corpus-facing behaviour rather than the audit-facing one.
        New-Item -ItemType Directory -Path (Join-Path $core 'pkgc') -Force | Out-Null
        Write-WithBom -Path (Join-Path $core 'pkgc/bom.cs') -Text '[module: GoManualConversion]'

        Push-Location $tmp
        try {
            git init -q 2>&1 | Out-Null
            git config user.name 'self test' 2>&1 | Out-Null
            git config user.email 'self@test' 2>&1 | Out-Null
            git add -A 2>&1 | Out-Null
            git -c commit.gpgsign=false commit -q -m 'fixture' 2>&1 | Out-Null
        } finally { Pop-Location }

        # The fixture's own precondition, asserted rather than assumed: two marked files and one
        # unmarked one. Without the unmarked file a predicate matching every .cs would pass every arm.
        $probe = Get-MarkedPath -Core $core
        # THREE now: two plain markers and one hidden behind a BOM. If this reads 2 the predicate has
        # lost its BOM tolerance, and arm 12 below would be measuring the fixture rather than the gate.
        if ($probe.Count -ne 3) {
            Write-Host ("SELF-TEST UNMEASURED: fixture census reads {0} marked file(s), want 3 (one of them BOM-hidden) -- every arm below would be about the fixture" -f $probe.Count)
            return 1
        }

        $hdr = @(
            '# FIXTURE',
            '',
            '## 4. The rows',
            '',
            '| # | path | upstream | instrument class | evidence class | dossier | auto@from | auto@to | class | reason / work item |',
            '|--:|:--|:--|:--|:--|:--|:--|:--|:--|:--|'
        )
        $bt = [string][char]0x60
        $rowOne = '| 1 | ' + $bt + 'pkga/one.cs' + $bt + ' | pkga/one.go | untouched | .auto differential | - | aaaa | aaaa | unchanged | both hashes filled |'
        $rowTwoTail = '| a | carried at 1234abcd9 with a gate that observes it |'
        $rowTwo = '| 2 | ' + $bt + 'pkgb/two.cs' + $bt + ' | pkgb/two.go | touched-substantive | .auto differential | - | bbbb | cccc ' + $rowTwoTail

        # The BOM-hidden hand-own's row. Every fixture carries it by DEFAULT, so the arms above stay
        # about what they are named for; arm 12 is the one that omits it, and omitting it is the whole
        # measurement -- a BOM-blind census would not see pkgc/bom.cs either, so the audit without its
        # row would read COMPLETE. With the tolerance the census sees three and the missing row refuses.
        $rowThree = '| 3 | ' + $bt + 'pkgc/bom.cs' + $bt + ' | pkgc/bom.go | untouched | .auto differential | - | dddd | dddd | unchanged | both hashes filled |'

        $script:fixtureIndex = 0
        function New-Fixture {
            param([string[]] $Rows, [switch] $OmitBomRow)
            $script:fixtureIndex++
            $path = Join-Path $tmp ("audit-{0}.md" -f $script:fixtureIndex)
            $all = $Rows
            if (-not $OmitBomRow) { $all = @($Rows) + @($rowThree) }
            Write-NoBom -Path $path -Text ($hdr + $all)
            return $path
        }

        Write-Host 'check-handown-audit self-test -- hermetic repo, real git, red-first'

        # ARM 1 (GREEN) -- a complete audit passes. Without it, a gate that refused everything would
        # pass every red arm below and no arm would say so.
        Invoke-Arm -Name 'a COMPLETE audit PASSES' -Core $core -WantExit 0 -WantText 'H6 AUDIT COMPLETE' `
            -Audit (New-Fixture -Rows @($rowOne, $rowTwo))

        # ARM 2 (RED) -- the floor-13 control the ruling names: one marked path regressed OUT of the
        # audit, and the gate must NAME it rather than merely refuse.
        Invoke-Arm -Name 'a marked path with NO row REFUSES and NAMES it' -Core $core -WantExit 1 -WantText 'pkgb/two.cs' `
            -Audit (New-Fixture -Rows @($rowOne))

        # ARM 3 (RED) -- the same path twice.
        Invoke-Arm -Name 'a path listed TWICE REFUSES' -Core $core -WantExit 1 -WantText 'A2-duplicate' `
            -Audit (New-Fixture -Rows @($rowOne, $rowTwo, ($rowTwo -replace '^\| 2 ', '| 3 ')))

        # ARM 4 (RED) -- a class outside the four.
        Invoke-Arm -Name 'a class outside unchanged/a/b/c REFUSES' -Core $core -WantExit 1 -WantText 'A3-class' `
            -Audit (New-Fixture -Rows @($rowOne, ($rowTwo -replace '\| a \|', '| x |')))

        # ARM 5 (RED) -- a BLANK class, which is exactly what a SKELETON carries.
        Invoke-Arm -Name 'a BLANK class (the skeleton state) REFUSES' -Core $core -WantExit 1 -WantText '(blank)' `
            -Audit (New-Fixture -Rows @($rowOne, ($rowTwo -replace '\| a \|', '|  |')))

        # ARM 6 (RED) -- class b with no reason.
        Invoke-Arm -Name 'class b with NO reason REFUSES' -Core $core -WantExit 1 -WantText 'A4-b-reason' `
            -Audit (New-Fixture -Rows @($rowOne, ($rowTwo -replace [regex]::Escape($rowTwoTail), '| b |  |')))

        # ARM 7 (RED) -- class c whose reason is non-empty but names nothing. "TODO" is non-empty.
        Invoke-Arm -Name 'class c whose reason names NO work item REFUSES' -Core $core -WantExit 1 -WantText 'A4-c-workitem' `
            -Audit (New-Fixture -Rows @($rowOne, ($rowTwo -replace [regex]::Escape($rowTwoTail), '| c | TODO |')))

        # ARM 8 (GREEN, the BOUND on arm 7) -- class c WITH a work item passes, so arm 7 narrows the
        # gate rather than refusing every c. Without this, A4-c could reject everything and arm 7 would
        # still be green.
        Invoke-Arm -Name 'class c WITH a work item PASSES' -Core $core -WantExit 0 -WantText 'H6 AUDIT COMPLETE' `
            -Audit (New-Fixture -Rows @($rowOne, ($rowTwo -replace [regex]::Escape($rowTwoTail), '| c | deferred, OQ-12, owner G |')))

        # ARM 9 (RED) -- the "no .auto emitted" state.
        Invoke-Arm -Name 'a row in the "no .auto emitted" state REFUSES' -Core $core -WantExit 1 -WantText 'A5-no-auto' `
            -Audit (New-Fixture -Rows @($rowOne, ($rowTwo -replace '\| bbbb \| cccc ', '| no .auto emitted | no .auto emitted ')))

        # ARM 10 -- a LADDER RUNG is a work item. Added with the pattern rather than after it: an
        # accepted pattern with no arm is an addition nobody has seen fire, which is the state this
        # repository audited its own scrub census out of the same day.
        Invoke-Arm -Name 'class c whose work item is a LADDER RUNG passes' -Core $core -WantExit 0 -WantText 'H6 AUDIT COMPLETE' `
            -Audit (New-Fixture -Rows @($rowOne, ($rowTwo -replace [regex]::Escape($rowTwoTail), '| c | retired by H5c, owner G |')))

        # ARM 12 -- THE BOM ARM, and the reason the predicate moved to _paths.ps1. The audit omits the
        # BOM-hidden hand-own's row. A census that cannot see past a byte-order mark does not see the
        # file either, so it counts two, matches two rows, and reports COMPLETE -- a green earned by
        # the instrument's blindness. With the tolerance it counts three and REFUSES, naming the file.
        #
        # Red-proved by reverting the predicate to the anchored ERE: this arm reads exit 0 (COMPLETE)
        # before the tolerance and exit 1 naming pkgc/bom.cs after it. It is the only arm here whose
        # subject is the CORPUS-facing predicate rather than the audit-facing assertions.
        Invoke-Arm -Name 'a BOM-hidden hand-own is SEEN, and its missing row REFUSES' -Core $core -WantExit 1 -WantText 'pkgc/bom.cs' `
            -Audit (New-Fixture -Rows @($rowOne, $rowTwo) -OmitBomRow)

        # ARM 11 (MISUSE) -- a census that finds NOTHING exits 2. It must never report a clean audit,
        # which is the whole reason exit 2 is separate from exit 0.
        $emptyCore = Join-Path $tmp 'empty'
        New-Item -ItemType Directory -Path $emptyCore -Force | Out-Null
        Invoke-Arm -Name 'a census that finds NOTHING exits 2, not 0' -Core $emptyCore -WantExit 2 -WantText 'returned ZERO' `
            -Audit (New-Fixture -Rows @($rowOne, $rowTwo))

        if ($script:ArmsFailed -ne 0) {
            Write-Host ("SELF-TEST FAILED: {0} arm(s) failed" -f $script:ArmsFailed)
            return 1
        }
        if ($script:ArmsPassed -ne 12) {
            Write-Host ("SELF-TEST FAILED: {0} arm(s) ran, expected 12 -- an arm that quietly stops running is what this count exists to catch" -f $script:ArmsPassed)
            return 1
        }
        Write-Host 'SELF-TEST CLEAN -- 12 arms'
        return 0
    } finally {
        if (Test-Path -LiteralPath $tmp) { Remove-Item -LiteralPath $tmp -Recurse -Force -ErrorAction SilentlyContinue }
    }
}

if ($SelfTest) {
    exit (Invoke-SelfTest)
}

exit (Invoke-Gate -Audit $AuditFile -Core $CorePath)
