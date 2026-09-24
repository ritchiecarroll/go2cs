$ErrorActionPreference = 'Stop'
. 'C:\p2g\src\_roster.ps1'
$rows = @(Get-ValidatedRosterRows -Path 'C:\p2g\docs\ValidatedTestPackages.md')
"rows: $($rows.Count)"
foreach ($i in 1,51,127,128,218) { "row {0}: {1}" -f $i, $rows[$i-1].Package }
foreach ($p in 'crypto/tls','net','os','internal/trace','path/filepath','os/exec') { $r = $rows | Where-Object Package -eq $p; "cell {0}: {1} | {2}  conditional={3} condDisclosures={4}" -f $p, $r.Expected, $r.Disclosed, (@($r.Conditional) -join ','), (@($r.ConditionalDisclosures) -join ',') }
"execution pins: " + (($rows | Where-Object { $_.Execution } | ForEach-Object { "$($_.Package)=$($_.Execution)" }) -join ', ')
$text = [IO.File]::ReadAllText('C:\p2g\src\version.props'); if ($text -match '<GoStdLibVersion>([^<]+)</GoStdLibVersion>') { "GoStdLibVersion: $($Matches[1])" }
$sw = [IO.File]::ReadAllText('C:\p2g\src\run-validated-sweep.ps1')
if ($sw -match "(?m)^\`$longTimeouts = (@\{[^\r\n]*\})") { $t = Invoke-Expression $Matches[1]; "longTimeouts: $($t.Count) entries: " + (($t.GetEnumerator() | Sort-Object Name | ForEach-Object { "$($_.Name)=$($_.Value)" }) -join ' ') }
if ($sw -match "'crypto/tls' = @\{ Test = '([^']+)'; BlockSize = (\d+) \}") { "capability block crypto/tls: $($Matches[1]) / $($Matches[2])" }
$hdr = [IO.File]::ReadAllText('C:\p2g\docs\ValidatedTestPackages.md'); $m = [regex]::Match($hdr, '([0-9,]+) matching verdicts\s*' + [char]0x00B7 + '\s*([0-9,]+) disclosed'); "header (first match): $($m.Groups[1].Value) / $($m.Groups[2].Value)"
