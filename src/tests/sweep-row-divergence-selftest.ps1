<#
.SYNOPSIS
    Guards the sweep-shard leg's per-row divergence lines (Get-ComparisonDivergenceLines, src\_roster.ps1).

.DESCRIPTION
    The os-matrix sweep-shard leg prints, for each FAILING row, its comparison record as summary lines:
    status, matched, disclosed, the undisclosed divergence count, and the first undisclosed divergences
    with both verdicts. Those lines reach the check-runs annotations, which is the only part of a leg a
    cloud lane can read (the job summary and the artifact are blob-served). Before them, a skip-shaped
    divergence on a failing row could not be read from such a lane at all (COORD ruling (a), 2026-11-06,
    after the osx-arm64 runtime row's AES trio went unread in run 42214977499).

    The rows here are PLANTED: fabricated comparison records written as files and read back through
    ConvertFrom-ComparisonRecord, exactly as the leg reads a real one, so both reader branches (5.1 and 7+)
    are exercised under whichever edition runs this.

        ./sweep-row-divergence-selftest.ps1

.NOTES
    Requires PowerShell 5.1 (Windows) or PowerShell 7+ (any platform). Exit 0 clean, 1 on any violation.
    No build, no converter, no corpus. No non-ASCII literal appears in this file (see
    sweep-oracle-rerun-selftest.ps1 for why).
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

function Get-JsonString([string] $value) {
    return '"' + $value.Replace('\', '\\').Replace('"', '\"') + '"'
}

function Get-JsonMap([hashtable] $map) {
    return '{' + ((@($map.Keys | Sort-Object | ForEach-Object { (Get-JsonString $_) + ':' + (Get-JsonString $map[$_]) })) -join ',') + '}'
}

$fixtureRoot = Join-Path ([System.IO.Path]::GetTempPath()) ('go2cs-divergence-selftest-' + [guid]::NewGuid().ToString('n'))
New-Item -ItemType Directory -Force -Path $fixtureRoot | Out-Null

# Writes a planted record and reads it back through the shipped reader.
function Read-PlantedRecord {
    param(
        [string] $Name,
        [hashtable] $Go = @{},
        [hashtable] $CSharp = @{},
        [string[]] $Disclosed = @(),
        [string[]] $Errors = @(),
        [string] $Status = 'failing'
    )

    $parts = @(
        '"package":"fixture"'
        '"status":' + (Get-JsonString $Status)
        '"go":' + (Get-JsonMap $Go)
        '"csharp":' + (Get-JsonMap $CSharp)
        '"disclosed":[' + ((@($Disclosed | ForEach-Object { Get-JsonString $_ })) -join ',') + ']'
        '"errors":[' + ((@($Errors | ForEach-Object { Get-JsonString $_ })) -join ',') + ']'
    )

    $path = Join-Path $fixtureRoot "$Name.json"
    [System.IO.File]::WriteAllText($path, '{' + ($parts -join ',') + '}', (New-Object System.Text.UTF8Encoding($false)))
    return ConvertFrom-ComparisonRecord -Path $path
}

# ---- 1. a planted failing row: every shape of undisclosed divergence, a disclosure, and a non-divergence error ----
# Go pass / C# skip and Go skip / C# pass are the two shapes the C# FAIL listing above it can never show
# (the AES trio's shapes); Go fail / C# "" is a one-sided row; the parenthetical is a disclosure whose
# signature did not match, which is still undisclosed and keeps its detail.
$planted = Read-PlantedRecord -Name 'planted' `
    -Go @{ TestA = 'pass'; TestB = 'pass'; TestC = 'skip'; TestD = 'pass'; TestE = 'fail'; TestF = 'pass' } `
    -CSharp @{ TestA = 'pass'; TestB = 'skip'; TestC = 'pass'; TestD = 'fail'; TestF = 'fail' } `
    -Disclosed @('TestD') `
    -Errors @(
        'TestB: Go="pass" C#="skip"'
        'TestC: Go="skip" C#="pass"'
        'TestE: Go="fail" C#=""'
        'TestF: Go="pass" C#="fail" (failure does not match the disclosed host-limit signature "x")'
        'go test: exit status 1'
    )

$lines = Get-ComparisonDivergenceLines -Row 'fix/planted' -Comparison $planted

Assert-Equal 'planted: the headline names status, matched, disclosed, undisclosed and other errors' `
    'fix/planted: status failing; matched 1 of 6 Go / 5 C# verdicts; disclosed 1; undisclosed divergences 4; other errors 1' $lines[0]
Assert-Equal 'planted: Go pass / C# skip is listed with both verdicts' '    TestB: Go="pass" C#="skip"' $lines[1]
Assert-Equal 'planted: Go skip / C# pass is listed with both verdicts' '    TestC: Go="skip" C#="pass"' $lines[2]
Assert-Equal 'planted: a one-sided row is listed' '    TestE: Go="fail" C#=""' $lines[3]
Assert-Equal 'planted: a signature mismatch keeps its detail' `
    '    TestF: Go="pass" C#="fail" (failure does not match the disclosed host-limit signature "x")' $lines[4]
Assert-Equal 'planted: a non-divergence error is printed apart' '    error: go test: exit status 1' $lines[5]
Assert-Equal 'planted: nothing else is printed' 6 $lines.Count

# ---- 2. the cap: the first $First divergences, then the remainder counted ----
$many = @(1..45 | ForEach-Object { 'TestN{0:D2}: Go="pass" C#="skip"' -f $_ })
$capped = Read-PlantedRecord -Name 'capped' -Errors $many
$cappedLines = Get-ComparisonDivergenceLines -Row 'fix/capped' -Comparison $capped -First 40

Assert-Equal 'cap: the count is the whole set' $true ($cappedLines[0] -match 'undisclosed divergences 45;')
Assert-Equal 'cap: the 40th is printed' '    TestN40: Go="pass" C#="skip"' $cappedLines[40]
Assert-Equal 'cap: the remainder is counted, not dropped' '    ... and 5 more undisclosed divergence(s)' $cappedLines[41]
Assert-Equal 'cap: header + 40 + remainder' 42 $cappedLines.Count

# ---- 3. a failing row with nothing undisclosed (a disclosed-only red against a banked count) ----
$disclosedOnly = Read-PlantedRecord -Name 'disclosed-only' -Status 'passing' `
    -Go @{ TestA = 'pass'; TestB = 'pass' } -CSharp @{ TestA = 'pass'; TestB = 'skip' } -Disclosed @('TestB')
$disclosedLines = Get-ComparisonDivergenceLines -Row 'fix/disclosed' -Comparison $disclosedOnly

Assert-Equal 'disclosed-only: the headline says zero undisclosed' `
    'fix/disclosed: status passing; matched 1 of 2 Go / 2 C# verdicts; disclosed 1; undisclosed divergences 0; other errors 0' $disclosedLines[0]
Assert-Equal 'disclosed-only: one line' 1 $disclosedLines.Count

# ---- 4. a row that wrote no record ----
$absent = Get-ComparisonDivergenceLines -Row 'fix/absent' -Comparison $null
Assert-Equal 'absent: named, not silent' 'fix/absent: no comparison record' $absent[0]

# ---- 5. a long detail is clipped, and says so ----
$long = 'TestL: Go="pass" C#="fail" (' + ('y' * 400) + ')'
$clipped = Get-ComparisonDivergenceLines -Row 'fix/long' -Comparison (Read-PlantedRecord -Name 'long' -Errors @($long)) -DetailWidth 60
Assert-Equal 'clip: the line is cut at the width and marked' ('    ' + $long.Substring(0, 60) + ' ...') $clipped[1]

# ---- 6. the C# side's own failure text under each undisclosed divergence (COORD's TestUnsafePoint ruling) ----
# The planted row again, with failure text for two of its divergences: TestF's (blank lines skipped, capped at
# -TextLines) and TestE's; TestB and TestC have none and print none. Without -FailureText the lines are exactly those
# of section 1, which its own asserts already pin.
$failureText = @{
    TestF = "    unsafepoint_test.go:21: can't objdump exit status 1`n`n    second line`n    third line`n    fourth line"
    TestE = 'one line only'
}
$withText = Get-ComparisonDivergenceLines -Row 'fix/planted' -Comparison $planted -FailureText $failureText -TextLines 3

Assert-Equal 'text: the headline is unchanged' $lines[0] $withText[0]
Assert-Equal 'text: a divergence with no text prints no body' '    TestC: Go="skip" C#="pass"' $withText[2]
Assert-Equal 'text: a divergence with text is followed by it' '    TestE: Go="fail" C#=""' $withText[3]
Assert-Equal 'text: its one line, marked' '      | one line only' $withText[4]
Assert-Equal 'text: the signature-mismatch divergence, then its first line' '      |     unsafepoint_test.go:21: can''t objdump exit status 1' $withText[6]
Assert-Equal 'text: a blank line is skipped' '      |     second line' $withText[7]
Assert-Equal 'text: capped at -TextLines' '      |     third line' $withText[8]
Assert-Equal 'text: the line after the cap is the next entry' '    error: go test: exit status 1' $withText[9]
Assert-Equal 'text: header + 4 divergences + 4 text lines + 1 error' 10 $withText.Count
$clippedText = Get-ComparisonDivergenceLines -Row 'fix/long' -Comparison (Read-PlantedRecord -Name 'long2' -Errors @('TestL: Go="pass" C#="fail"')) -FailureText @{ TestL = ('z' * 300) } -DetailWidth 40
Assert-Equal 'text: a long text line is clipped like a divergence line' ('      | ' + ('z' * 40) + ' ...') $clippedText[2]

# ---- verdict -----------------------------------------------------------------------------------------

Remove-Item -LiteralPath $fixtureRoot -Recurse -Force -ErrorAction SilentlyContinue

Write-Host ''
if ($failures.Count -eq 0) {
    Write-Host "sweep-row-divergence-selftest: $checks checks, 0 violations ($($PSVersionTable.PSEdition) $($PSVersionTable.PSVersion))" -ForegroundColor Green
    exit 0
}

Write-Host "sweep-row-divergence-selftest: $checks checks, $($failures.Count) VIOLATION(S) ($($PSVersionTable.PSEdition) $($PSVersionTable.PSVersion))" -ForegroundColor Red
$failures | ForEach-Object { Write-Host "  $_" -ForegroundColor Red }
exit 1
