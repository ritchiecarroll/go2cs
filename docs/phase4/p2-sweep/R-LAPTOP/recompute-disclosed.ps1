# recompute-disclosed.ps1 -- OFFLINE recompute of gotDisclosed and orphanedDisclosures for S-R (COORD
# ruling 2026-09-24): the live driver read them through `$doc['disclosed']`/`.Contains`, which fail on
# the PSCustomObject _roster's ConvertFrom-ComparisonRecord returns under Windows PowerShell 5.1. This
# reads the SAME preserved per-row records (rows/<row>/go2cs_test_comparison.json) through the same
# reader, by PROPERTY. Windows PowerShell 5.1; ASCII only.
param([switch] $SelfTest)
$ErrorActionPreference = 'Continue'
$Er = '<evidence-root>'
. '<wt>\src\_roster.ps1'

function Get-DisclosureCounts([string] $Path) {
    $doc = ConvertFrom-ComparisonRecord -Path $Path
    $names = @(@($doc.PSObject.Properties | ForEach-Object Name))
    $disc = if ($names -contains 'disclosed') { @($doc.disclosed | Where-Object { $_ -ne $null }) } else { @() }
    $orph = if ($names -contains 'orphanedDisclosures') { @($doc.orphanedDisclosures | Where-Object { $_ -ne $null }) } else { $null }
    return @{ Disclosed = $disc.Count; Orphaned = $(if ($null -eq $orph) { 'absent' } else { $orph.Count }); OrphanedNames = $(if ($orph) { ($orph | ForEach-Object { "$_".Split(':')[0] }) -join ';' } else { '' }); Status = $doc.status }
}

if ($SelfTest) {
    $src = Join-Path $Er 'rows\crypto__tls\go2cs_test_comparison.json'
    $r = Get-DisclosureCounts $src
    "control crypto/tls: disclosed=$($r.Disclosed) orphaned=$($r.Orphaned) status=$($r.Status)   (want disclosed=1)"
    # PLANT: the same record with one extra disclosed entry injected -> must read 2.
    $text = [IO.File]::ReadAllText($src)
    $i = $text.IndexOf('"disclosed"')
    if ($i -lt 0) { 'PLANT REFUSED: no "disclosed" member'; exit 1 }
    $j = $text.IndexOf('[', $i)
    $planted = $text.Substring(0, $j + 1) + '"TestPlanted (planted): control entry",' + $text.Substring($j + 1)
    $pp = Join-Path $Er 'plant-comparison.json'
    [IO.File]::WriteAllText($pp, $planted, (New-Object Text.UTF8Encoding($false)))
    $p = Get-DisclosureCounts $pp
    "plant: disclosed=$($p.Disclosed)   (want 2)"
    Remove-Item -LiteralPath $pp -Force
    if ($r.Disclosed -eq 1 -and $p.Disclosed -eq 2) { 'RECOMPUTE SELF-TEST PASSED'; exit 0 } else { 'RECOMPUTE SELF-TEST FAILED'; exit 1 }
}

"row`tgot_disclosed`torphaned`torphaned_names`tstatus`trecord"
foreach ($d in (Get-ChildItem -LiteralPath (Join-Path $Er 'rows') -Directory | Sort-Object Name)) {
    $pkg = $d.Name -replace '__','/'
    $rec = Join-Path $d.FullName 'go2cs_test_comparison.json'
    if (-not (Test-Path -LiteralPath $rec)) { "$pkg`t`t`t`t`tABSENT"; continue }
    $r = Get-DisclosureCounts $rec
    "$pkg`t$($r.Disclosed)`t$($r.Orphaned)`t$($r.OrphanedNames)`t$($r.Status)`trows/$($d.Name)/go2cs_test_comparison.json"
}
