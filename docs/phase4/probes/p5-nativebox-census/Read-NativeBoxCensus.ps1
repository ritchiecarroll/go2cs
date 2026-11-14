<#
.SYNOPSIS
  Folds native-box dereference census files (NBCENSUS) into one total: the LAST block of each file, summed across files.

.DESCRIPTION
  One process writes several blocks into its own file, and each block is a cumulative snapshot. Summing blocks
  double-counts, and reading only a final block drops a process that died before its exit hook. See README.md.

.PARAMETER Path
  The directory the census files were written to.

.PARAMETER Pattern
  The file name pattern. Default 'nb-*.txt'.

.PARAMETER Detail
  Also print one line per file.
#>
param(
    [Parameter(Mandatory = $true)][string] $Path,
    [string] $Pattern = 'nb-*.txt',
    [switch] $Detail
)

$ErrorActionPreference = 'Stop'

$files = @(Get-ChildItem -LiteralPath $Path -Filter $Pattern -File | Sort-Object Name)

if ($files.Count -eq 0) {
    Write-Output "NOT MEASURED: no file matches '$Pattern' under '$Path'. No file means the census never armed."
    exit 1
}

$total = @{ dereferences = [long]0; b2a = [long]0; b2b = [long]0; unpinnable = [long]0 }
$rows = @{}
$startOnly = 0
$partialLast = 0
$bad = @()

foreach ($file in $files) {
    $lines = @(Get-Content -LiteralPath $file.FullName)
    $last = -1

    for ($i = 0; $i -lt $lines.Count; $i++) {
        if ($lines[$i] -match '^NBCENSUS(-PARTIAL)? dereferences=') { $last = $i }
    }

    if ($last -lt 0) { $bad += $file.Name; continue }

    if ($lines[$last] -notmatch '^NBCENSUS(-PARTIAL)? dereferences=(\d+) b2a=(\d+) b2b=(\d+) unpinnable=(\d+)$') { $bad += $file.Name; continue }

    $isPartial = $Matches[1] -eq '-PARTIAL'
    $d = [long]$Matches[2]; $a = [long]$Matches[3]; $b = [long]$Matches[4]; $u = [long]$Matches[5]
    $total.dereferences += $d; $total.b2a += $a; $total.b2b += $b; $total.unpinnable += $u

    $isStart = $false

    for ($i = $last + 1; $i -lt $lines.Count; $i++) {
        if ($lines[$i] -match '^NBCENSUS-START ') { $isStart = $true }
        elseif ($lines[$i] -match '^NBCENSUS-BROKEN ') { $bad += "$($file.Name) (BROKEN block)" }
        elseif ($lines[$i] -match '^NBCENSUS-ROW\s+(\d+)  (.+)$') {
            $key = $Matches[2]
            if (-not $rows.ContainsKey($key)) { $rows[$key] = [long]0 }
            $rows[$key] += [long]$Matches[1]
        }
    }

    if ($isStart) { $startOnly++ } elseif ($isPartial) { $partialLast++ }

    if ($Detail) { Write-Output ("FILE {0}  dereferences={1} b2a={2} b2b={3} unpinnable={4}{5}" -f $file.Name, $d, $a, $b, $u, $(if ($isStart) { '  (start block only)' } elseif ($isPartial) { '  (last block is a partial)' } else { '' })) }
}

if ($bad.Count -gt 0) {
    Write-Output ("NOT MEASURED: no usable block in: " + ($bad -join ', '))
    exit 1
}

Write-Output ("NBCENSUS-TOTAL files={0} dereferences={1} b2a={2} b2b={3} unpinnable={4}" -f $files.Count, $total.dereferences, $total.b2a, $total.b2b, $total.unpinnable)
Write-Output ("NBCENSUS-FILES start-only={0} last-block-partial={1} (a partial last block is a process that did not reach its exit hook; its rows may trail its counters)" -f $startOnly, $partialLast)

foreach ($key in ($rows.Keys | Sort-Object)) {
    Write-Output ("NBCENSUS-ROW {0,10}  {1}" -f $rows[$key], $key)
}

exit 0
