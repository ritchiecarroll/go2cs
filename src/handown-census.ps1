<#
.SYNOPSIS
    The H6 hand-own differential census (GoCorpusMigration.md H6): for every hand-owned file in the
    corpus, diff the UPSTREAM Go source it replaces between two Go releases and classify. Read-only.

.DESCRIPTION
    H6 is the hand-own differential review -- until this instrument it was a person reading diffs
    over ALL hand-owns. This census reduces the review burden to the substantive list:

      untouched            upstream source identical between the releases -> no review owed
      touched-trivial      only comments/blank lines/whitespace moved     -> no review owed
      touched-substantive  code moved -> the H6 REVIEW LIST (human judgment starts here)
      no-upstream-counterpart  no Go file exists at the mapped path in either release
                           (hand-ADDITIONS like *_impl companions with no upstream twin,
                            and platform mirrors) -> H6 reviews these only via their principal

    CLASSIFICATION, NOT JUDGMENT: the instrument never says a substantive change is safe or unsafe.
    The comparison is deliberately conservative in one direction: a file is trivial ONLY if the
    comment/whitespace-stripped forms match byte-for-byte; any doubt (unbalanced block comment,
    stripper bailout) classifies as SUBSTANTIVE, because over-reporting sends a human to look and
    under-reporting hides a behavior change.

.PARAMETER FromGoRoot
    GOROOT of the CURRENT pinned release (e.g. C:\Users\<u>\sdk\go1.23.1). Mandatory -- the
    instrument never guesses toolchains; a guessed toolchain is this week's GOTOOLCHAIN trap.

.PARAMETER ToGoRoot
    GOROOT of the TARGET release (e.g. C:\Users\<u>\sdk\go1.23.12). Mandatory.

.NOTES
    Self-verifying: class counts must sum to the marker census, which is re-measured from the tree
    every run (the census moves in both directions; never carry a number). Marker scan is
    line-anchored and whole-file, per the documented false-count traps.
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)] [string] $FromGoRoot,
    [Parameter(Mandatory = $true)] [string] $ToGoRoot,
    [switch] $ListUntouched
)

$ErrorActionPreference = 'Stop'

# Paths from the shared primitives, never re-derived (repo rule).
. (Join-Path $PSScriptRoot '_paths.ps1')
$core = Join-Path $PSScriptRoot 'core'

foreach ($root in @($FromGoRoot, $ToGoRoot)) {
    if (-not (Test-Path (Join-Path $root 'src'))) {
        throw "not a GOROOT (no src/ beneath it): $root"
    }
}

function Read-Text([string]$Path) { [System.IO.File]::ReadAllText($Path) }

# Strip Go comments and normalize whitespace, conservatively. Returns $null on any bailout --
# and $null classifies as SUBSTANTIVE, the safe direction.
function Get-StrippedGo([string]$Path) {
    $text = Read-Text $Path
    $sb = New-Object System.Text.StringBuilder
    $i = 0; $n = $text.Length
    $state = 'code'   # code | line-comment | block-comment | dquote | bquote | squote
    while ($i -lt $n) {
        $c = $text[$i]
        $c2 = if ($i + 1 -lt $n) { $text[$i + 1] } else { [char]0 }
        switch ($state) {
            'code' {
                if ($c -eq '/' -and $c2 -eq '/') {
                    # Go DIRECTIVES (//go:noescape, //go:build, //go:linkname ...) are lexically
                    # comments but operationally load-bearing -- a one-directive diff is exactly
                    # what dll_windows.go's go1.23.12 change is. Keep them as code.
                    if ($i + 5 -le $n -and $text.Substring($i, 5) -eq '//go:') {
                        while ($i -lt $n -and $text[$i] -ne "`n") { [void]$sb.Append($text[$i]); $i++ }
                        continue
                    }
                    $state = 'line-comment'; $i += 2; continue
                }
                if ($c -eq '/' -and $c2 -eq '*') { $state = 'block-comment'; $i += 2; continue }
                if ($c -eq '"')  { $state = 'dquote'; [void]$sb.Append($c); $i++; continue }
                if ($c -eq '`')  { $state = 'bquote'; [void]$sb.Append($c); $i++; continue }
                if ($c -eq "'")  { $state = 'squote'; [void]$sb.Append($c); $i++; continue }
                [void]$sb.Append($c); $i++
            }
            'line-comment' {
                if ($c -eq "`n") { $state = 'code'; [void]$sb.Append($c) }
                $i++
            }
            'block-comment' {
                if ($c -eq '*' -and $c2 -eq '/') { $state = 'code'; $i += 2; continue }
                $i++
            }
            'dquote' {
                [void]$sb.Append($c)
                if ($c -eq '\') { if ($i + 1 -lt $n) { [void]$sb.Append($c2); $i += 2; continue } }
                elseif ($c -eq '"') { $state = 'code' }
                $i++
            }
            'bquote' {
                [void]$sb.Append($c)
                if ($c -eq '`') { $state = 'code' }
                $i++
            }
            'squote' {
                [void]$sb.Append($c)
                if ($c -eq '\') { if ($i + 1 -lt $n) { [void]$sb.Append($c2); $i += 2; continue } }
                elseif ($c -eq "'") { $state = 'code' }
                $i++
            }
        }
    }
    if ($state -eq 'block-comment' -or $state -eq 'dquote' -or $state -eq 'bquote') {
        return $null   # unbalanced at EOF -- bail out, classify substantive
    }
    # Normalize: strip trailing space, drop blank lines, collapse interior runs of spaces/tabs.
    $lines = $sb.ToString() -split "`r?`n" | ForEach-Object { ($_ -replace '[ \t]+', ' ').Trim() } | Where-Object { $_ -ne '' }
    return ($lines -join "`n")
}

# --- 1. Marker census: line-anchored, whole files, tracked files only -----------------------------
# The predicate itself lives in _paths.ps1 as Get-HandOwnMarkedPath, with the BOM tolerance and the
# reason it is -P and not an ERE escape. ONE definition: check-handown-audit.ps1 re-measures this exact
# population at H6, and a second spelling of it here is a second predicate that will eventually differ.
$marked = @(Get-HandOwnMarkedPath -CoreRoot $core | ForEach-Object { $_ -replace '/', '\' })
if ($marked.Count -eq 0) { throw 'marker census returned zero -- wrong directory or broken git grep' }

# --- 2. Map each hand-own to the upstream Go source it replaces -----------------------------------
# src/core/<pkg-path>/[<goos>/]<name>[_impl].cs  ->  <GOROOT>/src/<pkg-path>/<name>.go
$goosDirs = @('windows', 'linux', 'darwin')
$results = New-Object System.Collections.Generic.List[object]
foreach ($rel in $marked) {
    $parts = $rel -split '\\'
    $file = $parts[-1]
    $dirParts = @($parts[0..($parts.Count - 2)])
    if ($dirParts.Count -gt 0 -and $goosDirs -contains $dirParts[-1]) {
        $dirParts = @($dirParts[0..($dirParts.Count - 2)])   # layout L3: per-GOOS folder is routing, not package path
    }
    $goName = ([System.IO.Path]::GetFileNameWithoutExtension($file) -replace '_impl$', '') + '.go'
    $upRel = (@($dirParts) + $goName) -join '/'
    $fromPath = Join-Path (Join-Path $FromGoRoot 'src') $upRel
    $toPath   = Join-Path (Join-Path $ToGoRoot 'src') $upRel

    $fromExists = Test-Path $fromPath
    $toExists   = Test-Path $toPath
    if (-not $fromExists -and -not $toExists) {
        $class = 'no-upstream-counterpart'
    }
    elseif (-not $fromExists -or -not $toExists) {
        # appeared or vanished across the range -- always a human look
        $class = 'touched-substantive'
    }
    elseif ((Read-Text $fromPath) -ceq (Read-Text $toPath)) {
        $class = 'untouched'
    }
    else {
        $sFrom = Get-StrippedGo $fromPath
        $sTo   = Get-StrippedGo $toPath
        if ($null -ne $sFrom -and $null -ne $sTo -and $sFrom -ceq $sTo) { $class = 'touched-trivial' }
        else { $class = 'touched-substantive' }
    }
    $results.Add([pscustomobject]@{ HandOwn = "src\core\$rel"; Upstream = $upRel; Class = $class })
}

# --- 3. Report, self-verified ---------------------------------------------------------------------
$byClass = $results | Group-Object Class
$total = ($byClass | Measure-Object -Property Count -Sum).Sum
Write-Host "handown-census: $($marked.Count) marked files  |  $FromGoRoot -> $ToGoRoot"
Write-Host ''
foreach ($g in $byClass | Sort-Object Name) {
    Write-Host ("{0,-24} {1,4}" -f $g.Name, $g.Count)
}
Write-Host ("{0,-24} {1,4}" -f 'TOTAL', $total)
if ($total -ne $marked.Count) {
    Write-Error "self-verify FAILED: classified $total of $($marked.Count) marked files"
    exit 1
}
Write-Host ''
Write-Host '=== THE H6 REVIEW LIST (touched-substantive) ==='
$sub = @($results | Where-Object { $_.Class -eq 'touched-substantive' })
if ($sub.Count -eq 0) { Write-Host '  (empty -- no hand-own''s upstream source moved substantively)' }
foreach ($r in $sub) { Write-Host ("  {0}`n      <- {1}" -f $r.HandOwn, $r.Upstream) }
Write-Host ''
Write-Host '=== touched-trivial (no review owed; listed for the record) ==='
foreach ($r in @($results | Where-Object { $_.Class -eq 'touched-trivial' })) {
    Write-Host ("  {0}  <- {1}" -f $r.HandOwn, $r.Upstream)
}
if ($ListUntouched) {
    Write-Host ''
    Write-Host '=== untouched ==='
    foreach ($r in @($results | Where-Object { $_.Class -eq 'untouched' })) { Write-Host ("  {0}" -f $r.HandOwn) }
}
Write-Host ''
Write-Host 'Classification only -- the judgment on every substantive row stays human (H6).'
