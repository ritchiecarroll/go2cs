<#
.SYNOPSIS
  Folds Q44 census files into a row total, the ONE way that is correct.

.DESCRIPTION
  A census file can hold SEVERAL blocks, because the partial flush writes one every N conversions
  and the final dump appends another. Those blocks are CUMULATIVE SNAPSHOTS of one running total,
  not increments. Two folds are therefore wrong and one is right:

    summing every block          DOUBLE-COUNTS. i9 measured a row 1.96x high this way
                                 (mailbox c62ca28686), and the output said nothing to prevent it.
    taking the FINAL block       DROPS any process killed before it finished -- which is exactly
                                 the case the partial flush exists for. Two of seven files on the
                                 measured row carried a PARTIAL and no final.
    LAST BLOCK PER FILE,         correct for both, and what this script does.
    then sum across FILES

  A file with no parseable block is REFUSED rather than counted as zero: an unrun census must never
  read as a measured near-zero. That is the same falsifier the `reflect` row is recorded under.

.PARAMETER Path
  Directory holding census files, or a single file.

.PARAMETER Pattern
  Filename filter (default *q44*census*.txt).

.PARAMETER Detail
  Also print each file's per-arm numbers.
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)] [string] $Path,
    [string] $Pattern = '*q44*census*.txt',
    [switch] $Detail
)

Set-StrictMode -Version 2.0
$ErrorActionPreference = 'Stop'

if (Test-Path -LiteralPath $Path -PathType Leaf) {
    $files = @(Get-Item -LiteralPath $Path)
} elseif (Test-Path -LiteralPath $Path -PathType Container) {
    $files = @(Get-ChildItem -LiteralPath $Path -Filter $Pattern -File -Recurse)
} else {
    Write-Error "no such path: $Path"; exit 2
}

if ($files.Count -eq 0) {
    # An empty enumeration is not a zero row: say so and refuse, rather than printing a total of 0.
    Write-Host "Q44READ REFUSED: no files matched '$Pattern' under $Path -- that is an unmeasured row, not a zero one"
    exit 1
}

# The totals line, in either the final or the partial spelling. Named groups so a reordering of the
# emitted fields cannot silently shift a column.
$rx = [regex]'^Q44CENSUS(?<partial>-PARTIAL)?\s+mints=(?<mints>\d+)\s+conversions=(?<conv>\d+)\s+arm1=(?<a1>\d+)\s+arm2a=(?<a2a>\d+)\s+arm2b=(?<a2b>\d+)\s+arm3=(?<a3>\d+)\s+arm4=(?<a4>\d+)'

# The START marker, written into the block the census emits at module init BEFORE any conversion.
# It is what separates a MEASURED zero (the process armed and converted nothing) from the four
# things "no file" used to mean: gate unset, golib never loaded, the host died first, or a failed
# write. It carries the PARTIAL header on purpose, so the fold above needs no special case.
$rxStart = [regex]'^Q44CENSUS-START\s'

$tot = [ordered]@{ mints = [long]0; conv = [long]0; a1 = [long]0; a2a = [long]0; a2b = [long]0; a3 = [long]0; a4 = [long]0 }
$refused = @()
$rows = @()

$armedZero = 0

foreach ($f in $files) {
    $last = $null; $blocks = 0; $i = -1; $lastTotalsAt = -1; $startAt = -1
    foreach ($line in (Get-Content -LiteralPath $f.FullName)) {
        $i++
        $m = $rx.Match($line)
        if ($m.Success) { $last = $m; $blocks++ ; $lastTotalsAt = $i; continue }
        if ($rxStart.IsMatch($line)) { $startAt = $i }
    }
    if ($null -eq $last) { $refused += $f.FullName; continue }

    $kind = if ($last.Groups['partial'].Success) { 'PARTIAL-ONLY' } else { 'final' }
    if ($kind -eq 'PARTIAL-ONLY' -and $blocks -gt 1) { $kind = 'PARTIAL (no final)' }
    # ARMED-ZERO is "the START block SURVIVED the fold", NOT "there is only one block" -- and the
    # difference is the whole case. A process that exits CLEANLY having converted nothing runs its
    # exit hook and writes a FINAL zero block, which was never ambiguous. The case that left NO
    # FILE is the one that DIED having converted nothing, and there the start block is the last
    # thing in the file. The START marker sits after its own totals line inside its block, so
    # "after the last totals line" is exactly "no later block superseded it".
    if ($startAt -gt $lastTotalsAt) { $kind = 'ARMED-ZERO'; $armedZero++ }

    $rows += [pscustomobject]@{
        File   = $f.Name
        Blocks = $blocks
        Kind   = $kind
        Conv   = [long]$last.Groups['conv'].Value
        Arm1   = [long]$last.Groups['a1'].Value
        Arm2a  = [long]$last.Groups['a2a'].Value
        Arm2b  = [long]$last.Groups['a2b'].Value
        Arm3   = [long]$last.Groups['a3'].Value
        Arm4   = [long]$last.Groups['a4'].Value
    }
    $tot.mints += [long]$last.Groups['mints'].Value
    $tot.conv  += [long]$last.Groups['conv'].Value
    $tot.a1    += [long]$last.Groups['a1'].Value
    $tot.a2a   += [long]$last.Groups['a2a'].Value
    $tot.a2b   += [long]$last.Groups['a2b'].Value
    $tot.a3    += [long]$last.Groups['a3'].Value
    $tot.a4    += [long]$last.Groups['a4'].Value
}

Write-Host "Q44READ fold: LAST block per file, summed across files (blocks are cumulative snapshots)"
foreach ($r in $rows) {
    if ($Detail) {
        Write-Host ("  {0,-40} blocks={1,-3} {2,-18} conv={3,-12} arm1={4} arm2a={5} arm2b={6} arm3={7} arm4={8}" -f `
            $r.File, $r.Blocks, $r.Kind, $r.Conv, $r.Arm1, $r.Arm2a, $r.Arm2b, $r.Arm3, $r.Arm4)
    } else {
        Write-Host ("  {0,-40} blocks={1,-3} {2,-18} conv={3}" -f $r.File, $r.Blocks, $r.Kind, $r.Conv)
    }
}

Write-Host ""
Write-Host ("Q44READ ROW TOTAL  files={0}  mints={1}  conversions={2}" -f $rows.Count, $tot.mints, $tot.conv)
Write-Host ("Q44READ ROW ARMS   arm1={0} arm2a={1} arm2b={2} arm3={3} arm4={4}" -f $tot.a1, $tot.a2a, $tot.a2b, $tot.a3, $tot.a4)

if ($armedZero -gt 0) {
    Write-Host ("Q44READ ARMED-ZERO {0} process(es) armed the census, converted NOTHING and DIED before their exit hook -- a MEASURED zero, not an unmeasured row" -f $armedZero)
}

$armSum = $tot.a1 + $tot.a2a + $tot.a2b + $tot.a3 + $tot.a4
if ($armSum -ne $tot.conv) {
    Write-Host ("Q44READ SKEW       arms sum to {0} against conversions {1}, delta {2} -- expected when any file's last block is a PARTIAL" -f $armSum, $tot.conv, ($tot.conv - $armSum))
} else {
    Write-Host "Q44READ RECONCILES arms sum to conversions exactly"
}

if ($refused.Count -gt 0) {
    Write-Host ""
    Write-Host ("Q44READ REFUSED {0} file(s) with NO parseable block -- an unrun census is not a zero:" -f $refused.Count)
    foreach ($p in $refused) { Write-Host "  $p" }
    exit 1
}
exit 0
