<#
.SYNOPSIS
    Guards the sweep's ORACLE-ONLY failure rule -- the classifier that decides whether
    run-validated-sweep.ps1 may re-run a failed row once before failing it.

.DESCRIPTION
    The rule (Test-OracleOnlyFailure, src\_roster.ps1) and the tail reader beside it
    (Get-ResultsTailText) decide a GATE's verdict: get them wrong in the permissive direction and a
    genuine converted-side regression is re-run and waved through; get them wrong in the strict
    direction and a green corpus keeps failing on the reference implementation's own flake. Neither
    can be exercised by running the sweep -- that is a multi-hour gate that needs a flaking oracle to
    reproduce at all -- so it is exercised HERE, against FABRICATED records, exactly as
    check-roster-format.ps1 exercises the three count-absorption rules beside it.

    Records are fabricated as FILES and read back through ConvertFrom-ComparisonRecord rather than
    handed to the rule as hashtables. That is deliberate: the reader has one branch per PowerShell
    edition (JavaScriptSerializer on 5.1, System.Text.Json on 7+, neither type present on the other
    host), and the members this rule reads -- `status`, `errors`, `testFilter` -- are new on both
    branches. A fixture that skipped the file would test the rule and leave half the shipped code
    unexecuted on whichever edition the lane happened not to run.

        ./sweep-oracle-rerun-selftest.ps1

.NOTES
    Requires PowerShell 5.1 (Windows) or PowerShell 7+ (any platform); the point is that it runs
    identically on both. Exit 0 clean, 1 on any violation. No build, no converter, no corpus: pure
    text and a handful of files under the host's temp directory, uniquely named per process so two
    lanes on one machine cannot collide.

    No non-ASCII literal appears in this file -- Windows PowerShell 5.1 parses a BOM-less .ps1
    through the system codepage, so a non-ASCII glyph in a pattern would silently decode to
    mojibake at PARSE time and the guard would report on a regex that can never match.
#>
[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'

. (Join-Path $PSScriptRoot '../_roster.ps1')

$failures = New-Object System.Collections.Generic.List[string]
$checks = 0

function Assert-Equal {
    param([string] $What, $Expected, $Actual)

    $script:checks++
    if ("$Expected" -ne "$Actual") {
        [void]$script:failures.Add("$What -- expected '$Expected', got '$Actual'")
    }
}

# The reason a refusal gives is part of its contract: a rule that refuses for the wrong stated
# reason is a rule whose next reader will chase the wrong artifact.
function Assert-ReasonNames {
    param([string] $What, $Result, [string] $Fragment)

    $script:checks++
    if ("$($Result.Reason)" -notmatch [regex]::Escape($Fragment)) {
        [void]$script:failures.Add("$What -- expected a reason naming '$Fragment', got '$($Result.Reason)'")
    }
}

# ---- fixture construction -------------------------------------------------------------------------
# JSON is built by hand rather than through ConvertTo-Json for one reason that matters: one fixture
# must carry a BACKSLASH-ESCAPED event inside a string value (the shape a host stream takes when it
# is embedded in another JSON document), and getting that escaping exactly right is the thing under
# test. Hand-building makes the bytes on disk explicit. Only quote and backslash are escaped -- no
# fixture here contains a control character.
function Get-JsonString([string] $value) {
    return '"' + $value.Replace('\', '\\').Replace('"', '\"') + '"'
}

function New-RecordJson {
    param(
        [hashtable] $Go = @{},
        [hashtable] $CSharp = @{},
        [string[]] $Errors = @(),
        [string] $Status = 'failing',
        [string] $TestFilter = ''
    )

    $goPairs = @($Go.Keys | Sort-Object | ForEach-Object { (Get-JsonString $_) + ':' + (Get-JsonString $Go[$_]) })
    $csPairs = @($CSharp.Keys | Sort-Object | ForEach-Object { (Get-JsonString $_) + ':' + (Get-JsonString $CSharp[$_]) })
    $errorItems = @($Errors | ForEach-Object { Get-JsonString $_ })

    $parts = @(
        '"schemaVersion":1'
        '"package":"fixture"'
        '"status":' + (Get-JsonString $Status)
        '"go":{' + ($goPairs -join ',') + '}'
        '"csharp":{' + ($csPairs -join ',') + '}'
        '"matched":false'
        '"skipped":[]'
        '"disclosed":[]'
        '"excluded":[]'
        '"errors":[' + ($errorItems -join ',') + ']'
    )

    if ($TestFilter) { $parts += '"testFilter":' + (Get-JsonString $TestFilter) }

    $parts += '"environment":{"configuration":"Debug","tiered":true}'

    return '{' + ($parts -join ',') + '}'
}

$fixtureRoot = Join-Path ([System.IO.Path]::GetTempPath()) ("sweep-oracle-selftest-$PID-" + [DateTime]::UtcNow.Ticks)
[void](New-Item -ItemType Directory -Force -Path $fixtureRoot)

$fixtureSeq = 0

# Writes one fabricated record and reads it back the way the sweep does.
function Read-Fixture {
    param(
        [hashtable] $Go = @{},
        [hashtable] $CSharp = @{},
        [string[]] $Errors = @(),
        [string] $Status = 'failing',
        [string] $TestFilter = ''
    )

    $script:fixtureSeq++
    $path = Join-Path $fixtureRoot ("record-$script:fixtureSeq.json")
    [System.IO.File]::WriteAllText($path, (New-RecordJson -Go $Go -CSharp $CSharp -Errors $Errors -Status $Status -TestFilter $TestFilter))

    return ConvertFrom-ComparisonRecord -Path $path
}

# The measured shape, four rows wide: the converted side passes everything, Go's own binary fails
# one case it passed on the banking host. (crypto/tls run 3 was this at 3,644 rows and eight
# flaked cases; the arithmetic is identical at four.)
$goFlaked = @{ 'T1' = 'pass'; 'T2' = 'pass'; 'T3' = 'fail'; 'T4' = 'pass' }
$csClean = @{ 'T1' = 'pass'; 'T2' = 'pass'; 'T3' = 'pass'; 'T4' = 'pass' }
$oracleDivergence = 'T3: Go="fail" C#="pass"'
$goExit = 'go test: exit status 1'
$csExit = 'converted tests: exit status 1'

# A clean tail: the host's own results file, ending on an ordinary terminal event.
$cleanTail = '{"schemaVersion":1,"package":"fixture","events":[{"test":"T4","action":"pass","elapsed":0.01}]}'

# ---- 1. the shape the ruling was written from ------------------------------------------------------

$result = Test-OracleOnlyFailure -Comparison (Read-Fixture -Go $goFlaked -CSharp $csClean -Errors @($oracleDivergence, $goExit)) -TailText $cleanTail
Assert-Equal 'oracle-only: the measured shape is oracle-only' $true $result.OracleOnly
Assert-Equal 'oracle-only: the flaked case is named' 'T3' ($result.Flaked -join ',')
Assert-Equal 'oracle-only: an accepted shape carries no reason' $true ([string]::IsNullOrEmpty($result.Reason))

# BOTH exit-status lines, which is what crypto/tls actually produces: its fixtures expired
# 2025-01-01 and fail identically on either runtime, so the converted host exits non-zero too and
# the compare oracle's exit-code forgiveness is blocked the moment any mismatch exists. Refusing
# these lines would make the rule unfireable for the only case it was written from.
$result = Test-OracleOnlyFailure -Comparison (Read-Fixture -Go $goFlaked -CSharp $csClean -Errors @($oracleDivergence, $goExit, $csExit)) -TailText $cleanTail
Assert-Equal 'oracle-only: both exit-status lines are tolerated' $true $result.OracleOnly

# More than one flaked case, in the order the record lists them.
$goTwo = @{ 'T1' = 'pass'; 'T2' = 'fail'; 'T3' = 'fail'; 'T4' = 'pass' }
$result = Test-OracleOnlyFailure -Comparison (Read-Fixture -Go $goTwo -CSharp $csClean -Errors @('T2: Go="fail" C#="pass"', $oracleDivergence, $goExit)) -TailText $cleanTail
Assert-Equal 'oracle-only: every flaked case is named' 'T2,T3' ($result.Flaked -join ',')

# ---- 2. a converted-side failure is never the oracle's ---------------------------------------------

# MIXED: one oracle-side divergence and one converted-side. The row is this corpus's own failure.
$csMixed = @{ 'T1' = 'pass'; 'T2' = 'fail'; 'T3' = 'pass'; 'T4' = 'pass' }
$result = Test-OracleOnlyFailure -Comparison (Read-Fixture -Go $goFlaked -CSharp $csMixed -Errors @('T2: Go="pass" C#="fail"', $oracleDivergence, $goExit)) -TailText $cleanTail
Assert-Equal 'mixed: a mixed failure set is not oracle-only' $false $result.OracleOnly
Assert-ReasonNames 'mixed: the reason names the converted-side row' $result 'T2'

# CONVERTED-ONLY: the plain regression this rule must never touch.
$result = Test-OracleOnlyFailure -Comparison (Read-Fixture -Go $csClean -CSharp $csMixed -Errors @('T2: Go="pass" C#="fail"', $csExit)) -TailText $cleanTail
Assert-Equal 'converted-only: a converted-side failure set is not oracle-only' $false $result.OracleOnly
Assert-ReasonNames 'converted-only: the reason names the verdict pair' $result "Go='pass' C#='fail'"

# The AXIS a "Go=fail" test alone would not vary: the converted side may fail to produce the row at
# all. Go=fail / C#=skip is not the oracle flaking, it is the converted side not running the case.
$csSkip = @{ 'T1' = 'pass'; 'T2' = 'pass'; 'T3' = 'skip'; 'T4' = 'pass' }
$result = Test-OracleOnlyFailure -Comparison (Read-Fixture -Go $goFlaked -CSharp $csSkip -Errors @('T3: Go="fail" C#="skip"', $goExit)) -TailText $cleanTail
Assert-Equal 'skip: Go=fail with C#=skip is not oracle-only' $false $result.OracleOnly

# A disclosed row whose pinned signature did not match carries a trailing parenthetical. The pair
# still decides, and fail/fail is not this shape.
$result = Test-OracleOnlyFailure -Comparison (Read-Fixture -Go $goFlaked -CSharp $csMixed `
        -Errors @('T3: Go="fail" C#="fail" (failure does not match the disclosed host-limit signature "boom")', $goExit)) -TailText $cleanTail
Assert-Equal 'signature: an agreeing failure is not oracle-only' $false $result.OracleOnly

# ---- 3. a killed or crashed run is never a flake ---------------------------------------------------

# THE TIMEOUT EVENT, PLAIN -- the host's own tail states the package deadline kill outright.
$timeoutTail = '{"schemaVersion":1,"package":"fixture","events":[{"test":"T4","action":"pass"},' +
    '{"test":"","action":"timeout","output":"package timeout after 00:40:00"}]}'
$result = Test-OracleOnlyFailure -Comparison (Read-Fixture -Go $goFlaked -CSharp $csClean -Errors @($oracleDivergence, $goExit)) -TailText $timeoutTail
Assert-Equal 'timeout (plain): a deadline kill is not oracle-only' $false $result.OracleOnly
Assert-ReasonNames 'timeout (plain): the reason names the signature class' $result 'deadline/crash signature'

# THE TIMEOUT EVENT, JSON-ESCAPED -- carried inside the record's own error text, where the event's
# quotes are backslash-escaped by the enclosing document. A substring count of the plain spelling
# returns ZERO here (CLAUDE.md, 2026-09-02), which is why the pattern admits both. The tail is
# deliberately CLEAN in this case, so a match can only have come from the escaped spelling.
$escapedEvent = 'converted tests: the host stream ended {\"test\":\"\",\"action\":\"timeout\"}'
$result = Test-OracleOnlyFailure -Comparison (Read-Fixture -Go $goFlaked -CSharp $csClean -Errors @($oracleDivergence, $escapedEvent)) -TailText $cleanTail
Assert-Equal 'timeout (escaped): the escaped spelling is read too' $false $result.OracleOnly
Assert-ReasonNames 'timeout (escaped): the reason names the signature class' $result 'deadline/crash signature'

# A CRASH SIGNATURE in the tail.
$crashTail = '{"schemaVersion":1,"package":"fixture","events":[{"test":"T4","action":"fail","output":"Unhandled exception. System.NotImplementedException"}]}'
$result = Test-OracleOnlyFailure -Comparison (Read-Fixture -Go $goFlaked -CSharp $csClean -Errors @($oracleDivergence, $goExit)) -TailText $crashTail
Assert-Equal 'crash: a crash signature in the tail is not oracle-only' $false $result.OracleOnly
Assert-ReasonNames 'crash: the reason names the signature class' $result 'deadline/crash signature'

# The torn-publish-tree exit code, which is a cleanup and never a finding.
$tornTail = '{"schemaVersion":1,"package":"fixture","events":[]} exit status 0xc0000142'
$result = Test-OracleOnlyFailure -Comparison (Read-Fixture -Go $goFlaked -CSharp $csClean -Errors @($oracleDivergence, $goExit)) -TailText $tornTail
Assert-Equal 'torn tree: 0xc0000142 in the tail is not oracle-only' $false $result.OracleOnly

# NO TAIL AT ALL. The deadline question cannot be answered, so it is answered NO -- the same
# discipline every absorption rule in the sweep uses: an unreadable input is a rejection.
#
# This case found a real defect on its first run: with $TailText declared `[string]`, PowerShell
# COERCED the $null the reader returns for a missing file into the empty string, so the refusal
# below could never fire and an unchecked run would have been re-run as a flake. Both spellings are
# asserted now, and the parameter is untyped for exactly this reason.
$result = Test-OracleOnlyFailure -Comparison (Read-Fixture -Go $goFlaked -CSharp $csClean -Errors @($oracleDivergence, $goExit)) -TailText $null
Assert-Equal 'no tail: a $null tail is not oracle-only' $false $result.OracleOnly
Assert-ReasonNames 'no tail: the reason says the question was answered NO' $result 'answered NO'

$result = Test-OracleOnlyFailure -Comparison (Read-Fixture -Go $goFlaked -CSharp $csClean -Errors @($oracleDivergence, $goExit)) -TailText ''
Assert-Equal 'no tail: an EMPTY tail is not oracle-only either' $false $result.OracleOnly

# ---- 4. a run that did not complete -----------------------------------------------------------------

# ENTRY-COUNT MISMATCH: a truncated side. Its missing rows would read as one-sided divergences whose
# C# half is absent, not passing -- but the count says so first and more cheaply.
$csShort = @{ 'T1' = 'pass'; 'T2' = 'pass'; 'T3' = 'pass' }
$result = Test-OracleOnlyFailure -Comparison (Read-Fixture -Go $goFlaked -CSharp $csShort -Errors @($oracleDivergence, $goExit)) -TailText $cleanTail
Assert-Equal 'counts: an entry-count mismatch is not oracle-only' $false $result.OracleOnly
Assert-ReasonNames 'counts: the reason names both counts' $result '4 Go row(s) against 3 C# row(s)'

# A side with NO verdicts at all -- the mass-empty family.
$result = Test-OracleOnlyFailure -Comparison (Read-Fixture -Go $goFlaked -CSharp @{} -Errors @($oracleDivergence, $csExit)) -TailText $cleanTail
Assert-Equal 'empty: a side with no verdicts is not oracle-only' $false $result.OracleOnly

# A record with no verdict maps at all.
Assert-Equal 'maps: a record carrying no verdict maps is not oracle-only' $false `
    (Test-OracleOnlyFailure -Comparison ([PSCustomObject]@{ status = 'failing'; errors = @() }) -TailText $cleanTail).OracleOnly

# ---- 5. records that do not describe this row ------------------------------------------------------

# STATUS: only a run that COMPLETED and diverged can be a flake.
foreach ($status in @('conversion-blocked', 'infrastructure-blocked', 'not-applicable', 'validated')) {
    $result = Test-OracleOnlyFailure -Comparison (Read-Fixture -Go $goFlaked -CSharp $csClean -Errors @($oracleDivergence, $goExit) -Status $status) -TailText $cleanTail
    Assert-Equal "status: '$status' is not oracle-only" $false $result.OracleOnly
}

# A GATED record answers for its filter's survivors and rewrites the same file a full run writes.
$result = Test-OracleOnlyFailure -Comparison (Read-Fixture -Go $goFlaked -CSharp $csClean -Errors @($oracleDivergence, $goExit) -TestFilter 'TestBogo') -TailText $cleanTail
Assert-Equal 'gated: a filtered record is not this row evidence' $false $result.OracleOnly
Assert-ReasonNames 'gated: the reason names the filter' $result 'TestBogo'

# AN INFRASTRUCTURE ERROR ENTRY, refused BY NAME rather than skipped -- a new error shape must never
# be waved through by a rule that only knows how to recognize divergences.
foreach ($entry in @(
        'census: go test reported tests the manifest does not declare: TestX',
        'test manifest: test manifest is stale: input digest changed (run -tests -test-action convert)',
        'unsupported testing capabilities: subprocess',
        'test disclosures: unreadable manifest')) {
    $result = Test-OracleOnlyFailure -Comparison (Read-Fixture -Go $goFlaked -CSharp $csClean -Errors @($oracleDivergence, $entry)) -TailText $cleanTail
    Assert-Equal "infrastructure: '$(($entry -split ':')[0])' is not oracle-only" $false $result.OracleOnly
    Assert-ReasonNames 'infrastructure: the reason quotes the entry' $result 'non-divergence error'
}

# NOTHING TO ATTRIBUTE: exit-status lines with no per-test divergence behind them.
$result = Test-OracleOnlyFailure -Comparison (Read-Fixture -Go $goFlaked -CSharp $csClean -Errors @($goExit, $csExit)) -TailText $cleanTail
Assert-Equal 'no divergence: exit lines alone are not oracle-only' $false $result.OracleOnly

# An empty error list on a failing record.
$result = Test-OracleOnlyFailure -Comparison (Read-Fixture -Go $goFlaked -CSharp $csClean -Errors @()) -TailText $cleanTail
Assert-Equal 'no errors: an empty error list is not oracle-only' $false $result.OracleOnly

# ---- 6. the tail reader ------------------------------------------------------------------------------
# Get-ResultsTailText is the other half of the shipped code and has its own failure mode: a results
# file for a 3,600-verdict package is megabytes, so it reads a WINDOW. The window must still contain
# the event the rule looks for, which it does by construction (TestHost.WriteResults appends the
# package-level event last) -- asserted here rather than assumed.

Assert-Equal 'tail: a missing file reads as $null' $true ($null -eq (Get-ResultsTailText -Path (Join-Path $fixtureRoot 'no-such-file.json')))

$bigPath = Join-Path $fixtureRoot 'big-results.json'
$padding = '{"test":"Filler","action":"pass","output":"' + ('x' * 1000) + '"},'
[System.IO.File]::WriteAllText($bigPath, '{"schemaVersion":1,"package":"fixture","events":[' + ($padding * 200) +
    '{"test":"","action":"timeout","output":"package timeout after 00:40:00"}]}')

$tail = Get-ResultsTailText -Path $bigPath
Assert-Equal 'tail: the window is bounded' $true ($tail.Length -le 65536)
Assert-Equal 'tail: the file is genuinely larger than the window' $true ((Get-Item -LiteralPath $bigPath).Length -gt 65536)
Assert-Equal 'tail: the last event is inside the window' $true ($tail -match '"action"\s*:\s*"timeout"')

# And end to end through the tail reader, which is how the sweep calls it.
$result = Test-OracleOnlyFailure -Comparison (Read-Fixture -Go $goFlaked -CSharp $csClean -Errors @($oracleDivergence, $goExit)) -TailText $tail
Assert-Equal 'tail: a windowed deadline kill still refuses' $false $result.OracleOnly

$smallPath = Join-Path $fixtureRoot 'small-results.json'
[System.IO.File]::WriteAllText($smallPath, $cleanTail)
Assert-Equal 'tail: a file smaller than the window reads whole' $cleanTail (Get-ResultsTailText -Path $smallPath)

# ---- the go2cs-gen output-missing class (Test-GeneratedTypeMissingFailure) ------------------------------
# The sweep's second re-run arm. Its fixtures are the shapes a failed row's OUTPUT takes; the i9's D5 reflect
# row is the positive case (three CS0246 on generated types, nothing else). Every refusal names why.

$i9Shape = @(
    "walk.cs(406,25): error CS0246: The type or namespace name 'FileNode' could not be found [go.ast.csproj]",
    "walk.cs(422,20): error CS0246: The type or namespace name 'inspectorVisitor' could not be found [go.ast.csproj]",
    "walk.cs(432,14): error CS0246: The type or namespace name 'inspectorVisitor' could not be found [go.ast.csproj]"
) -join "`n"
$result = Test-GeneratedTypeMissingFailure -OutputText $i9Shape
Assert-Equal 'generated: the i9 D5 shape is the class' $true $result.GeneratedTypeMissing
Assert-Equal 'generated: the i9 D5 shape names CS0246 once' 'CS0246' ($result.Codes -join ',')

$result = Test-GeneratedTypeMissingFailure -OutputText "CSC : error CS8034: Unable to load Analyzer assembly go2cs-gen.dll"
Assert-Equal 'generated: a guarded load failure (error CS8034) is the class' $true $result.GeneratedTypeMissing

$result = Test-GeneratedTypeMissingFailure -OutputText ($i9Shape + "`nx.cs(1,1): error CS1503: Argument 1: cannot convert")
Assert-Equal 'generated: a real compile error alongside refuses' $false $result.GeneratedTypeMissing
Assert-ReasonNames 'generated: the refusal names the outside code' $result 'CS1503'

$result = Test-GeneratedTypeMissingFailure -OutputText "CSC : warning CS8034: Unable to load Analyzer assembly go2cs-gen.dll"
Assert-Equal 'generated: a WARNING is not a failed build' $false $result.GeneratedTypeMissing
Assert-ReasonNames 'generated: the warning refusal says no compiler error' $result 'no compiler error'

$result = Test-GeneratedTypeMissingFailure -OutputText "--- FAIL: TestSomething`nConverted test action failed: mismatch"
Assert-Equal 'generated: a failure after the build refuses' $false $result.GeneratedTypeMissing

$result = Test-GeneratedTypeMissingFailure -OutputText $null
Assert-Equal 'generated: no output refuses' $false $result.GeneratedTypeMissing
Assert-ReasonNames 'generated: the no-output refusal says so' $result 'no output'

# ---- verdict -----------------------------------------------------------------------------------------

Remove-Item -LiteralPath $fixtureRoot -Recurse -Force -ErrorAction SilentlyContinue

Write-Host ''
if ($failures.Count -eq 0) {
    Write-Host "sweep-oracle-rerun-selftest: $checks checks, 0 violations ($($PSVersionTable.PSEdition) $($PSVersionTable.PSVersion))" -ForegroundColor Green
    exit 0
}

Write-Host "sweep-oracle-rerun-selftest: $checks checks, $($failures.Count) VIOLATION(S) ($($PSVersionTable.PSEdition) $($PSVersionTable.PSVersion))" -ForegroundColor Red
$failures | ForEach-Object { Write-Host "  $_" -ForegroundColor Red }
exit 1
