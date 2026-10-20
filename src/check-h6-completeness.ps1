<#
.SYNOPSIS
    The H6 completeness gate (docs/GoCorpusMigration.md, H6, "The completeness gate"): no migration's
    corpus is adopted until every hand-own in the RE-MEASURED census has exactly one classified row in
    that migration's audit file. Read-only; pure text over the tree this script sits in.

.DESCRIPTION
    POPULATION -- the census's own, never restated here. src/handown-census.ps1 is dot-sourced inside a
    function scope and this gate reads the marked set it computed ($marked). Its predicate is not
    copied: a literal grep undercounts, because the go.-qualified marker spelling is most of the
    population. The census classifies as well as counts, so it needs the hop's two GOROOTs; they are
    passed straight through. Its own guards stay armed: a zero marker count throws and a classification
    that does not sum to the census fails its self-verify. Either one REFUSES this gate (exit 2).

    ROWS -- the audit's row tables are found by their HEADER: every Markdown table whose header cells
    include '#', a cell starting 'path', a cell exactly 'class', a cell starting 'reason' and two cells
    naming an '.auto' sha256. All of them are gated as ONE population, under one identical header. Every
    '|' line under a delimiter row, up to the first that is not one, is a row. Cells split on unescaped
    '|' (GFM). A path cell is read without its backticks, relative to src/core. Prose is never a row.

    ASSERTIONS -- each violation is printed on one line, prefixed by its kind:
      MISSING    a census path with no row
      DUPLICATE  a census path with more than one row (every line named)
      EXTRA      a row path the census lacks
      CLASS      a class cell that is not exactly one of: unchanged a b c
      B-REASON   a 'b' whose reason cell is empty, a dash, or a bare class letter
      C-ITEM     a 'c' carrying neither a work-item reference ('work item: <referent>') nor an explicit
                 deferral (the word 'deferred' with a parenthesised reason, or 'reason: ...', AND
                 'owner <name>')
      NO-AUTO    a row in the "no .auto emitted" state: a cell recording it ('no .auto emitted',
                 'MISSING-AUTO'), or an '.auto' sha256 cell that is blank or a dash
      MALFORMED  a row whose cell count differs from the header's (its other checks are skipped, so
                 its path also reads MISSING)

    VACUITY -- refused (exit 2), never passed: a census that throws, a census of zero paths, a census
    that exposed no population, an audit with no row table, tables whose headers differ, a zero-row table.
    Both counts are printed on every run, before any verdict.

    NAMED BLIND SPOTS -- what a clean exit does NOT say:
      - The relocation blind spot (docs/phase4/CENSUS-h6-handown-package-aliases.md): a frozen hand-own
        present in both trees passes any set comparison, this one included, while carrying
        declarations the target release moved. The package-alias census is read beside every
        substantive row; this gate cannot see that class.
      - The gate reads what a row RECORDS, not whether it is true. A cited commit, hash, owner or work
        item is checked for presence, never resolved; a (c) whose rewrite has landed still reads (c).
      - Unmarked *_impl.cs companions are outside the census predicate, so outside this gate
        (the audit's OQ-1).

        powershell -NoProfile -ExecutionPolicy Bypass -File src/check-h6-completeness.ps1 `
            -FromGoRoot <outgoing GOROOT> -ToGoRoot <incoming GOROOT>

.PARAMETER FromGoRoot
    GOROOT of the outgoing release, passed through to handown-census.ps1 (mandatory there too).

.PARAMETER ToGoRoot
    GOROOT of the incoming release, passed through to handown-census.ps1. Its VERSION must name the
    release src/version.props pins (bumped at H2, so at H6 it is the incoming one), or the gate refuses.

.PARAMETER AuditFile
    The migration's audit file. Default: docs/phase4/AUDIT-h6-handown-go<GoStdLibVersion>.md in this
    tree, the release read from src/version.props, never a literal (NOTES-next-hop A8). A default that
    names no file REFUSES by name rather than reading an earlier hop's audit; an earlier hop's audit
    (hop B's is AUDIT-h6-handown-go124.md) is passed explicitly.

.NOTES
    Requires PowerShell 5.1 (Windows) or PowerShell 7+. Exit 0 clean, 1 on any violation, 2 when the
    gate refuses to measure. No non-ASCII literal: the audit's dash glyphs are spelled by code point,
    and every cell excerpt printed escapes non-ASCII as <U+XXXX>.
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)] [string] $FromGoRoot,
    [Parameter(Mandatory = $true)] [string] $ToGoRoot,
    [string] $AuditFile
)

$ErrorActionPreference = 'Stop'

$repo = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
$censusScript = Join-Path $PSScriptRoot 'handown-census.ps1'

$emDash = [string][char]0x2014
$enDash = [string][char]0x2013
$classes = @('unchanged', 'a', 'b', 'c')

function Stop-Refused([string] $Why) {
    Write-Host "h6-completeness: REFUSED -- $Why"
    exit 2
}

# One-line, ASCII-only excerpt of a cell: non-ASCII shown as <U+XXXX>, long text elided.
function Show-Cell([string] $Text, [int] $Max = 160) {
    $t = ($Text -replace '\s+', ' ').Trim()
    if ($t.Length -gt $Max) { $t = $t.Substring(0, $Max) + '...' }
    $sb = New-Object System.Text.StringBuilder
    foreach ($ch in $t.ToCharArray()) {
        if ([int]$ch -lt 128) { [void]$sb.Append($ch) } else { [void]$sb.Append(('<U+{0:X4}>' -f [int]$ch)) }
    }
    return $sb.ToString()
}

function Get-CellText([string] $Cell) {
    return ($Cell.Trim() -replace '^`+', '' -replace '`+$', '').Trim()
}

function Test-Blank([string] $Text) {
    $t = ($Text -replace '[`*]', '').Trim()
    return ($t -eq '' -or $t -eq $emDash -or $t -eq $enDash -or $t -eq '-')
}

# --- 1. Population: handown-census.ps1's own marked set, re-measured over this tree ----------------
# Dot-sourced in THIS function's scope, so its variables land here and nowhere else; its Write-Host
# lines are captured (stream 6) and the count table is echoed.
function Get-CensusPopulation([string] $Census, [string] $From, [string] $To) {
    $info = @(. $Census -FromGoRoot $From -ToGoRoot $To 6>&1)
    foreach ($line in $info) {
        $s = "$line"
        Write-Host "  census | $s"
        if ($s -match '^TOTAL\s') { break }
    }
    $v = Get-Variable -Name 'marked' -Scope Local -ErrorAction SilentlyContinue
    if ($null -eq $v) { return $null }
    return , @($v.Value | ForEach-Object { "$_" -replace '\\', '/' })
}

if (-not (Test-Path $censusScript)) { Stop-Refused "the census instrument is absent: src/handown-census.ps1" }

# --- 0. The release: src/version.props <GoStdLibVersion>, the runbook's one source of record -------
# A literal default here read the PREVIOUS hop's audit when run without -AuditFile, and passed or
# failed on rows nobody wrote for this hop (NOTES-next-hop A8).
$propsPath = Join-Path $repo 'src/version.props'
$pinMatch = if (Test-Path -LiteralPath $propsPath) {
    [regex]::Match([System.IO.File]::ReadAllText($propsPath), '<GoStdLibVersion>\s*([^<\s]+)\s*</GoStdLibVersion>')
}
if (-not $pinMatch -or -not $pinMatch.Success) { Stop-Refused "src/version.props carries no <GoStdLibVersion>, so the release under audit has no source of record" }
$goPin = $pinMatch.Groups[1].Value
if (-not $AuditFile) { $AuditFile = Join-Path $repo "docs/phase4/AUDIT-h6-handown-go$goPin.md" }
if (-not (Test-Path $AuditFile)) { Stop-Refused "the audit file is absent: $AuditFile (the release pinned in src/version.props is go$goPin)" }

# The census classifies against -ToGoRoot while src/core is the pinned release's, so the two must name
# one release. Matched whole: go1.24.13 is not met by go1.24.130.
$toVersionPath = Join-Path $ToGoRoot 'VERSION'
if (-not (Test-Path -LiteralPath $toVersionPath)) { Stop-Refused "-ToGoRoot has no VERSION file, so its release cannot be checked against the pin go$goPin" }
$toVersion = (Get-Content -LiteralPath $toVersionPath -TotalCount 1)
if (-not [regex]::IsMatch($toVersion, '(?<![0-9A-Za-z.])go' + [regex]::Escape($goPin) + '(?![0-9.])')) {
    Stop-Refused "-ToGoRoot's VERSION is '$toVersion', not the release src/version.props pins (go$goPin)"
}
Write-Host "h6-completeness: release go$goPin (src/version.props); audit $AuditFile"

Write-Host "h6-completeness: population by src/handown-census.ps1 over src/core"
try {
    $population = Get-CensusPopulation $censusScript $FromGoRoot $ToGoRoot
}
catch {
    Stop-Refused "the census failed, so there is no population to gate: $_"
}
if ($null -eq $population) { Stop-Refused 'the census exposed no marked population ($marked is not set after it ran)' }
$censusCount = $population.Count
Write-Host "h6-completeness: census = $censusCount marked paths"
if ($censusCount -eq 0) { Stop-Refused 'VACUOUS: the census returned 0 marked paths' }

# --- 2. Rows: the audit table found by its header ---------------------------------------------------
$lines = [System.IO.File]::ReadAllLines($AuditFile, [System.Text.Encoding]::UTF8)

function Split-Row([string] $Line) {
    $t = $Line.Trim()
    if ($t.StartsWith('|')) { $t = $t.Substring(1) }
    if ($t.EndsWith('|') -and -not $t.EndsWith('\|')) { $t = $t.Substring(0, $t.Length - 1) }
    return , @([regex]::Split($t, '(?<!\\)\|') | ForEach-Object { $_.Trim() })
}

$headers = @()
for ($i = 0; $i -lt $lines.Count; $i++) {
    if (-not $lines[$i].TrimStart().StartsWith('|')) { continue }
    $cells = Split-Row $lines[$i]
    $plain = @($cells | ForEach-Object { ($_ -replace '[`*]', '').Trim() })
    $hasNum = $plain -contains '#'
    $hasPath = @($plain | Where-Object { $_ -match '^path\b' }).Count -eq 1
    $hasClass = @($plain | Where-Object { $_ -ceq 'class' }).Count -eq 1
    $hasReason = @($plain | Where-Object { $_ -match '^reason\b' }).Count -eq 1
    $hashCols = @($plain | Where-Object { $_ -match '^\.auto\b.*\bsha256\b' }).Count
    if ($hasNum -and $hasPath -and $hasClass -and $hasReason -and $hashCols -eq 2) {
        $headers += , @($i, $plain)
    }
}
if ($headers.Count -eq 0) { Stop-Refused "VACUOUS: no row table in the audit (no header with #, path, class, reason and two .auto sha256 cells)" }
# Every table carrying the row header is gated, as ONE population (an audit with a section per lane carries
# one table per lane). Each table's rows are read under its own header, and every header must match the
# first cell for cell: a column that meant different things in different tables would be gated as one.
$hdr = $headers[0][1]
foreach ($h in $headers) {
    if (($h[1] -join '|') -cne ($hdr -join '|')) {
        Stop-Refused ("the row table at line $($h[0] + 1) has a different header from the one at line $($headers[0][0] + 1); every gated table must carry the same columns")
    }
}
$colNum = [array]::IndexOf($hdr, '#')
$colPath = @(0..($hdr.Count - 1) | Where-Object { $hdr[$_] -match '^path\b' })[0]
$colClass = @(0..($hdr.Count - 1) | Where-Object { $hdr[$_] -ceq 'class' })[0]
$colReason = @(0..($hdr.Count - 1) | Where-Object { $hdr[$_] -match '^reason\b' })[0]
$colHash = @(0..($hdr.Count - 1) | Where-Object { $hdr[$_] -match '^\.auto\b.*\bsha256\b' })

$rows = New-Object System.Collections.Generic.List[object]
$violations = New-Object System.Collections.Generic.List[string]
$auditRel = (Resolve-Path $AuditFile).Path.Replace($repo, '').TrimStart('\', '/')
foreach ($h in $headers) {
    $hdrLine = $h[0]
    $delim = if ($hdrLine + 1 -lt $lines.Count) { $lines[$hdrLine + 1].Trim() } else { '' }
    if ($delim -notmatch '^\|(\s*:?-+:?\s*\|)+$') { Stop-Refused "the row table's header (line $($hdrLine + 1)) has no delimiter row under it" }

    $tableRows = 0
    $lastLine = $hdrLine + 2
    for ($i = $hdrLine + 2; $i -lt $lines.Count; $i++) {
        if (-not $lines[$i].TrimStart().StartsWith('|')) { break }
        $cells = Split-Row $lines[$i]
        $ln = $i + 1
        if ($cells.Count -ne $hdr.Count) {
            [void]$violations.Add("MALFORMED line ${ln}: $($cells.Count) cells, the header has $($hdr.Count): $(Show-Cell $lines[$i] 120)")
            continue
        }
        $rows.Add([pscustomobject]@{
            Line   = $ln
            Num    = Get-CellText $cells[$colNum]
            Path   = Get-CellText $cells[$colPath]
            Class  = Get-CellText $cells[$colClass]
            Reason = $cells[$colReason]
            Hashes = @($colHash | ForEach-Object { $cells[$_] })
            Cells  = $cells
        })
        $tableRows++
        $lastLine = $ln
    }
    if ($headers.Count -gt 1) {
        Write-Host "h6-completeness: table  = $tableRows rows (header line $($hdrLine + 1), rows through line $lastLine)"
        if ($tableRows -eq 0) { Stop-Refused "VACUOUS: the row table at line $($hdrLine + 1) parsed 0 rows" }
    }
}
if ($headers.Count -eq 1) {
    Write-Host "h6-completeness: audit  = $($rows.Count) rows parsed ($auditRel, header line $($headers[0][0] + 1), rows through line $lastLine)"
}
else {
    Write-Host "h6-completeness: audit  = $($rows.Count) rows parsed ($auditRel, $($headers.Count) tables gated as one population)"
}
if ($rows.Count -eq 0) { Stop-Refused 'VACUOUS: the audit row table parsed 0 rows' }

# --- 3. Assertions ------------------------------------------------------------------------------------
function Get-RowId($r) { return "row $($r.Num) (line $($r.Line)) $($r.Path)" }

# 3a. every census path exactly once; no row path the census lacks
$byPath = @{}
foreach ($r in $rows) {
    if (-not $byPath.ContainsKey($r.Path)) { $byPath[$r.Path] = New-Object System.Collections.Generic.List[object] }
    $byPath[$r.Path].Add($r)
}
$censusSet = New-Object 'System.Collections.Generic.HashSet[string]' ([System.StringComparer]::Ordinal)
foreach ($p in $population) { [void]$censusSet.Add($p) }
foreach ($p in ($population | Sort-Object)) {
    if (-not $byPath.ContainsKey($p)) { [void]$violations.Add("MISSING $p -- marked in the census, no row in the audit") }
    elseif ($byPath[$p].Count -gt 1) {
        [void]$violations.Add("DUPLICATE $p -- $($byPath[$p].Count) rows: " + (($byPath[$p] | ForEach-Object { "row $($_.Num) line $($_.Line)" }) -join ', '))
    }
}
foreach ($r in $rows) {
    if (-not $censusSet.Contains($r.Path)) { [void]$violations.Add("EXTRA $(Get-RowId $r) -- a row the census lacks") }
}

# 3b. class, b reason, c work item or deferral, no-.auto state
$tally = @{}
$cReport = New-Object System.Collections.Generic.List[string]
foreach ($r in $rows) {
    $id = Get-RowId $r
    $key = if ($classes -ccontains $r.Class) { $r.Class } else { 'OTHER' }
    $tally[$key] = 1 + [int]$tally[$key]

    if ($classes -cnotcontains $r.Class) {
        [void]$violations.Add("CLASS $id -- class '$(Show-Cell $r.Class 40)' is not one of: $($classes -join ' ')")
    }

    $reasonPlain = ($r.Reason -replace '[`*]', '').Trim()
    if ($r.Class -ceq 'b' -and ((Test-Blank $r.Reason) -or $reasonPlain -match '^(?i)(b|\(b\)|n/?a)\.?$')) {
        [void]$violations.Add("B-REASON $id -- a 'b' with no written reason: '$(Show-Cell $r.Reason 40)'")
    }

    if ($r.Class -ceq 'c') {
        $item = ''
        $m = [regex]::Match($r.Reason, '(?i)\bwork[\s-]*item\b\s*[:=]\s*(?<ref>[^;]*)')
        if ($m.Success) {
            $item = ($m.Groups['ref'].Value -replace '[`*]', '').Trim().TrimEnd('.', ',').Trim()
            if ((Test-Blank $item) -or $item -match '^(?i)(tbd|todo|none|owed|n/?a|\?+)$') { $item = '' }
        }
        $owner = ''; $why = ''
        if ($r.Reason -match '(?i)\bdeferred\b') {
            $mo = [regex]::Match($r.Reason, '(?i)\bowner\b\s*[:=]?\s+(?<o>[A-Za-z0-9][\w.-]*)')
            if ($mo.Success) { $owner = $mo.Groups['o'].Value }
            $mw = [regex]::Match($r.Reason, '(?i)\bdeferred\b\s*\((?<w>[^)]*)\)')
            if (-not $mw.Success) { $mw = [regex]::Match($r.Reason, '(?i)\breason\s*[:=]\s*(?<w>[^;]*)') }
            if ($mw.Success) { $why = $mw.Groups['w'].Value.Trim() }
        }
        $deferred = ($owner -ne '' -and $why -ne '')
        $arms = @()
        if ($item -ne '') { $arms += "work item '$(Show-Cell $item 90)'" }
        if ($deferred) { $arms += "deferred, owner '$owner', reason '$(Show-Cell $why 60)'" }
        if ($arms.Count -gt 0) { [void]$cReport.Add("  c $id -- " + ($arms -join '; ')) }
        else {
            [void]$violations.Add("C-ITEM $id -- a 'c' with neither a work-item reference nor an explicit deferral with owner and reason")
        }
    }

    $noAuto = @()
    foreach ($c in $r.Cells) {
        if ($c -match '(?i)\bno\s+`?\.auto`?\s+emitted\b' -or $c -match '(?i)\bMISSING-AUTO\b') { $noAuto += "a cell records it: '$(Show-Cell $c 80)'"; break }
    }
    $blankHashes = @($r.Hashes | Where-Object { Test-Blank $_ })
    if ($blankHashes.Count -gt 0) {
        $noAuto += ("$($blankHashes.Count) of 2 .auto sha256 cells blank ('" + (($r.Hashes | ForEach-Object { Show-Cell $_ 20 }) -join "', '") + "')")
    }
    if ($noAuto.Count -gt 0) { [void]$violations.Add("NO-AUTO $id -- " + ($noAuto -join '; ')) }
}

# --- 4. Verdict ---------------------------------------------------------------------------------------
Write-Host ("h6-completeness: classes = " + (($classes + 'OTHER') | ForEach-Object { "$_ $([int]$tally[$_])" }) -join '  ')
if ($cReport.Count -gt 0) {
    Write-Host "h6-completeness: open (c) rows and what each carries:"
    $cReport | ForEach-Object { Write-Host $_ }
}
if ($violations.Count -gt 0) {
    Write-Host "h6-completeness: FAIL -- $($violations.Count) violation(s), census $censusCount, rows $($rows.Count)"
    $violations | ForEach-Object { Write-Host "  $_" }
    exit 1
}
Write-Host "h6-completeness: PASS -- census $censusCount marked paths, $($rows.Count) rows, every one classified and on record"
Write-Host "  (a set comparison: the relocation blind spot is not seen here -- read the package-alias census beside every substantive row)"
exit 0
