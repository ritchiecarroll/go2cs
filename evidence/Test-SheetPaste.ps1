<#
.SYNOPSIS
    Interactive-paste test of every PowerShell block in OWNER-SHEET.md (or the Markdown files named on the command
    line): the blocks are fed line by line, as the owner's paste reaches an interactive pwsh.

.DESCRIPTION
    COORD found this live in block 3 of the hashset revision-2 sheet (2026-10-10). An interactive pwsh runs each line as
    soon as it completes a statement. A block that closes an `if` with `}` and starts `else {` on the NEXT line runs the
    `if` alone, then fails on "The term 'else' is not recognized". A script parse accepts the same text, so the earlier
    parse check could not see it.

    The model is PSReadLine's AcceptLine: lines are appended to a buffer one at a time, and after each the buffer is
    parsed. While ANY parse error is incomplete input (an open brace or paren, a here-string, a trailing pipe or
    backtick), the next line is appended; otherwise the buffer is what the console submits. Each submission must parse without errors, and
    must not START with else, elseif, catch or finally: such a statement is its own command once the one before it has
    already run. A second arm holds the house rule at any depth: no line in a block starts with else, elseif, catch or
    finally. Exit code = the number of findings.
    Run with PowerShell 7 from the kit's root: pwsh -NoProfile -File evidence/Test-SheetPaste.ps1 [file.md ...]
#>
param([string[]]$Path)
$ErrorActionPreference = 'Stop'
if (-not $Path) { $Path = @(Join-Path (Split-Path $PSScriptRoot) 'OWNER-SHEET.md') }
$findings = 0
$submissions = 0
foreach ($file in $Path) {
    $lines = @((Get-Content -Raw -LiteralPath $file) -split "`r?`n")
    $block = 0
    $inBlock = $false
    $buffer = New-Object System.Collections.Generic.List[string]
    $bufferStart = 0
    for ($i = 0; $i -lt $lines.Count; $i++) {
        $line = $lines[$i]
        if (-not $inBlock) {
            if ($line -match '^```(powershell|pwsh|ps1)\s*$') { $inBlock = $true; $block++; $buffer.Clear() }
            continue
        }
        if ($line -match '^```\s*$') {
            if ($buffer.Count -and ($buffer -join '').Trim()) {
                $findings++
                Write-Host "  FAIL  ${file}:$($bufferStart + 1) (block $block): the block ends inside an unfinished statement" -ForegroundColor Red
            }
            $inBlock = $false
            continue
        }
        # The house rule (COORD, 2026-10-10), stricter than the paste model: an else, elseif, catch or finally always
        # sits on its closing brace's line, at any depth, so no edit can move one to the top level unnoticed.
        if ($line -match '^\s*(else|elseif|catch|finally)\b') {
            $findings++
            Write-Host "  FAIL  ${file}:$($i + 1) (block $block): a line starts with '$($Matches[1])'; keep it on the closing brace's line ('} $($Matches[1])')" -ForegroundColor Red
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
        $where = "${file}:$($bufferStart + 1) (block $block)"
        if ($errors.Count) {
            $findings++
            Write-Host "  FAIL  ${where}: does not parse as pasted: $($errors[0].Message)" -ForegroundColor Red
            continue
        }
        $first = @($tokens | Where-Object { $_.Kind -notin 'NewLine', 'Comment', 'EndOfInput' })[0]
        if ($first -and $first.Text -match '^(else|elseif|catch|finally)$') {
            $findings++
            Write-Host "  FAIL  ${where}: a pasted statement starts with '$($first.Text)': the statement before it has already run on its own; keep '} $($first.Text)' on the closing brace's line" -ForegroundColor Red
        }
    }
}
Write-Host "$($Path.Count) file(s), $submissions submission(s), findings $findings"
exit $findings
