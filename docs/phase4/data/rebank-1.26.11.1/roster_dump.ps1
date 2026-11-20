# Dumps the roster through its reader, one line per banked row, for deal.py (PLAN-rebank-1-26-11-1.md).
# Usage: pwsh -NoProfile -File roster_dump.ps1 -Tree <worktree> > roster.tsv
# Columns: package, windows expected, windows disclosed, execution pin, then one cell each for linux, linux-arm64 and
# darwin. A cell is "E+D", "E+D@RID|OTHER-RID=E+D" when the annotation carries a RID note, "n/a" when the row
# says n/a, and "-" when the row carries no annotation. The arm64 reading lives in the linux cell's RID note.
param([Parameter(Mandatory)][string] $Tree)
. (Join-Path $Tree 'src/_roster.ps1')
$rows = Get-ValidatedRosterRows -Path (Join-Path $Tree 'docs/ValidatedTestPackages.md')
foreach ($r in $rows) {
    $cells = @($r.Package, $r.Expected, $r.Disclosed, ($r.Execution -join ','))
    foreach ($k in 'linux', 'linux-arm64', 'darwin') {
        if (-not $r.OS.ContainsKey($k)) { $cells += '-'; continue }
        $o = $r.OS[$k]
        if (-not $o.Applicable) { $cells += 'n/a'; continue }
        $note = ''
        if ($o.Other) {
            $note = if ($o.Other.Pending) { "|$($o.Other.Rid)=pending" } else { "|$($o.Other.Rid)=$($o.Other.Expected)+$($o.Other.Disclosed)" }
        }
        $rid = if ($o.Rid) { "@$($o.Rid)" } else { '' }
        $cells += "$($o.Expected)+$($o.Disclosed)$rid$note"
    }
    $cells -join "`t"
}
