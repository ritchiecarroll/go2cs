<#
.SYNOPSIS
    Interactive-paste check of every PowerShell block in an owner sheet: the blocks are fed line by line, as the
    owner's paste reaches an interactive pwsh. With no arguments, the suite for the check itself.

.DESCRIPTION
    COORD found this live in block 3 of the hashset revision-2 sheet (2026-10-10). An interactive pwsh runs each line as
    soon as it completes a statement. A block that closes an `if` with `}` and starts `else {` on the NEXT line runs the
    `if` alone, then fails on "The term 'else' is not recognized". A script parse accepts the same text, so a parse
    check cannot see it.

    The model is PSReadLine's AcceptLine: lines are appended to a buffer one at a time, and after each the buffer is
    parsed. While ANY parse error is incomplete input (an open brace or paren, a here-string, a trailing pipe or
    backtick), the next line is appended; otherwise the buffer is what the console submits. Each submission must parse
    without errors, and must not START with else, elseif, catch or finally: such a statement is its own command once
    the one before it has already run. A second arm holds the house rule at any depth: no line in a block starts with
    else, elseif, catch or finally. A block that ends inside an unfinished statement is a finding too.

    A PowerShell block is a fence opened with ```powershell, ```pwsh or ```ps1; other fences are not read.

    Check a sheet before it goes to the owner (docs/NugetgoPublish.md, Traps):
        pwsh -NoProfile -File src/tools/nugetgo/Test-SheetPaste.ps1 <sheet.md> [<sheet.md> ...]
    Exit code = the number of findings.

    With no sheet it runs its suite (TestNugetgoSuitesPass runs it so under `go test ./...`) over
    testdata/sheet-paste: good.md must read no finding with every submission counted, and bad.md must read exactly
    its five findings, each at its block and line. Exit code = the number of failed cases.
#>
# The sheets are the positional arguments, or one -Path value comma-joined. Under pwsh -File a plain [string[]]
# parameter binds ONE argument, so `-Path a.md b.md` read a.md and dropped b.md without a word; here it is refused.
param([Parameter(Position = 0, ValueFromRemainingArguments = $true)][string[]]$Path)
$ErrorActionPreference = 'Stop'
$Path = @($Path | ForEach-Object { $_ -split ',' } | Where-Object { $_ })

# The findings for one sheet, in file order, each with its 1-based line and block, and the submissions counted.
function Get-SheetPasteFindings([string]$File) {
    $lines = @((Get-Content -Raw -LiteralPath $File) -split "`r?`n")
    $findings = [System.Collections.Generic.List[object]]::new()
    $submissions = 0
    $block = 0
    $inBlock = $false
    $buffer = [System.Collections.Generic.List[string]]::new()
    $bufferStart = 0
    function Add-Finding([int]$line, [string]$kind, [string]$message) {
        $findings.Add([pscustomobject]@{ Line = $line; Block = $block; Kind = $kind; Message = $message })
    }
    for ($i = 0; $i -lt $lines.Count; $i++) {
        $line = $lines[$i]
        if (-not $inBlock) {
            if ($line -match '^```(powershell|pwsh|ps1)\s*$') { $inBlock = $true; $block++; $buffer.Clear() }
            continue
        }
        if ($line -match '^```\s*$') {
            if ($buffer.Count -and ($buffer -join '').Trim()) {
                Add-Finding ($bufferStart + 1) 'unfinished' 'the block ends inside an unfinished statement'
            }
            $inBlock = $false
            continue
        }
        # The house rule (COORD, 2026-10-10), stricter than the paste model: an else, elseif, catch or finally always
        # sits on its closing brace's line, at any depth, so no edit can move one to the top level unnoticed.
        if ($line -match '^\s*(else|elseif|catch|finally)\b') {
            Add-Finding ($i + 1) 'house-rule' "a line starts with '$($Matches[1])'; keep it on the closing brace's line ('} $($Matches[1])')"
        }
        if (-not $buffer.Count) { $bufferStart = $i }
        $buffer.Add($line)
        $text = $buffer -join "`n"
        $tokens = $null; $errors = $null
        [void][System.Management.Automation.Language.Parser]::ParseInput($text, [ref]$tokens, [ref]$errors)
        # PSReadLine's AcceptLine keeps reading while ANY parse error is incomplete input (an open brace or paren, a
        # here-string, a trailing pipe or backtick), and submits otherwise.
        if (@($errors | Where-Object { $_.IncompleteInput }).Count) { continue }
        # The console submits the buffer here.
        $buffer.Clear()
        if (-not $text.Trim()) { continue }
        $submissions++
        if ($errors.Count) {
            Add-Finding ($bufferStart + 1) 'parse' "does not parse as pasted: $($errors[0].Message)"
            continue
        }
        $first = @($tokens | Where-Object { $_.Kind -notin 'NewLine', 'Comment', 'EndOfInput' })[0]
        if ($first -and $first.Text -match '^(else|elseif|catch|finally)$') {
            Add-Finding ($bufferStart + 1) 'starts-with' "a pasted statement starts with '$($first.Text)': the statement before it has already run on its own; keep '} $($first.Text)' on the closing brace's line"
        }
    }
    [pscustomobject]@{ Findings = $findings.ToArray(); Submissions = $submissions; Blocks = $block }
}

if ($Path) {
    $total = 0
    $submitted = 0
    foreach ($file in $Path) {
        $result = Get-SheetPasteFindings $file
        $submitted += $result.Submissions
        foreach ($f in $result.Findings) {
            $total++
            Write-Host "  FAIL  ${file}:$($f.Line) (block $($f.Block)): $($f.Message)" -ForegroundColor Red
        }
    }
    Write-Host "$($Path.Count) file(s), $submitted submission(s), findings $total"
    exit $total
}

$failed = 0
$ran = 0
function Check([string]$name, [bool]$ok, [string]$detail) {
    $script:ran++
    if ($ok) { Write-Host "  ok    $name" } else { $script:failed++; Write-Host "  FAIL  $name -- $detail" -ForegroundColor Red }
}
$fixtures = Join-Path $PSScriptRoot 'testdata/sheet-paste'
$good = Join-Path $fixtures 'good.md'
$bad = Join-Path $fixtures 'bad.md'

Write-Host 'the good sheet'
$result = Get-SheetPasteFindings $good
$read = @($result.Findings | ForEach-Object { "$($_.Block):$($_.Line):$($_.Kind)" }) -join ', '
Check 'no finding' ($result.Findings.Count -eq 0) "read [$read]"
# Every statement is counted, so a check that stopped reading early cannot pass: block 1 is one dot-sourced statement;
# block 2 is an assignment, a pipeline continued by its open brace, one continued by a trailing pipe, one continued
# by a backtick, a try/catch/finally on one line and a here-string. The text fence between them is not a block.
Check 'two PowerShell blocks, seven submissions' ($result.Blocks -eq 2 -and $result.Submissions -eq 7) "read $($result.Blocks) block(s), $($result.Submissions) submission(s)"

Write-Host 'the bad sheet: each finding at its block and line'
$result = Get-SheetPasteFindings $bad
$read = @($result.Findings | ForEach-Object { "$($_.Block):$($_.Line):$($_.Kind)" }) -join ', '
$want = @{
    'a top-level else on the line after its brace breaks the rule and runs as its own command' = '2:15:house-rule, 2:15:starts-with'
    'an else inside a dot-sourced block breaks the house rule only'                            = '3:27:house-rule'
    'a block that ends inside an open brace'                                                   = '4:36:unfinished'
    'a statement that does not parse'                                                          = '5:43:parse'
}
foreach ($case in $want.GetEnumerator() | Sort-Object Value) {
    $block = $case.Value.Split(':')[0]
    $got = @($result.Findings | Where-Object { "$($_.Block)" -eq $block } | ForEach-Object { "$($_.Block):$($_.Line):$($_.Kind)" }) -join ', '
    Check "block ${block}: $($case.Key)" ($got -ceq $case.Value) "want [$($case.Value)], read [$got]"
}
Check 'block 1 is clean and nothing else is found' ($result.Findings.Count -eq 5 -and $result.Blocks -eq 5) "read $($result.Blocks) block(s): [$read]"

Write-Host 'the check as an owner-sheet tool (-Path): exit code = findings'
# A failed case quotes the run's summary line only: its own FAIL lines would read as this suite's.
$out = @(& $PSCommandPath -Path $bad 6>&1 | ForEach-Object { "$_" })
$rc = $LASTEXITCODE
Check 'bad.md exits 5 and names its blocks' ($rc -eq 5 -and ($out -join "`n").Contains('bad.md:15 (block 2)') -and $out[-1] -ceq '1 file(s), 5 submission(s), findings 5') "rc $rc, $($out[-1])"
$out = @(& $PSCommandPath -Path $good 6>&1 | ForEach-Object { "$_" })
$rc = $LASTEXITCODE
Check 'good.md exits 0' ($rc -eq 0 -and $out[-1] -ceq '1 file(s), 7 submission(s), findings 0') "rc $rc, $($out[-1])"
$pwsh = (Get-Command pwsh -CommandType Application -ErrorAction SilentlyContinue | Select-Object -First 1).Source
foreach ($form in @(@($good, $bad), @('-Path', "$good,$bad"))) {
    $out = @(& $pwsh -NoProfile -NonInteractive -File $PSCommandPath @form 6>&1 | ForEach-Object { "$_" })
    $rc = $LASTEXITCODE
    $shown = if ($form[0] -eq '-Path') { '-Path good.md,bad.md' } else { 'good.md bad.md' }
    Check "under pwsh -File, $shown reads both sheets" ($rc -eq 5 -and $out[-1] -ceq '2 file(s), 12 submission(s), findings 5') "rc $rc, $($out[-1])"
}

Write-Host "ran $ran, failed $failed"
exit $failed
