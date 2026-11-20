# sample-ilc.ps1 STOPFILE OUT : every 5 s, record the max PrivateMemorySize64 and WorkingSet64 of any ilc.exe whose
# command line names the scout tree (g-ms), until STOPFILE exists. Writes "peak_private_gb peak_ws_gb samples".
param([string]$StopFile, [string]$Out)
$peakPriv = 0; $peakWs = 0; $n = 0; $seen = @{}
while (-not (Test-Path $StopFile)) {
  # ILC is started with a RELATIVE response-file path, so its command line need not name the tree (A0 sampled 0).
  # Any ilc.exe counts; this box runs one AOT publish at a time, and the seen-list records which ran.
  $ilc = Get-CimInstance Win32_Process -Filter "Name='ilc.exe'"
  foreach ($p in $ilc) { $seen["$($p.ProcessId) $($p.ExecutablePath)"] = 1 }
  foreach ($p in $ilc) {
    $proc = Get-Process -Id $p.ProcessId -ErrorAction SilentlyContinue
    if ($proc) { $n++; if ($proc.PrivateMemorySize64 -gt $peakPriv) { $peakPriv = $proc.PrivateMemorySize64 }; if ($proc.WorkingSet64 -gt $peakWs) { $peakWs = $proc.WorkingSet64 } }
  }
  Start-Sleep -Seconds 5
}
$lines = @("{0:N2} {1:N2} {2}" -f ($peakPriv / 1GB), ($peakWs / 1GB), $n) + @($seen.Keys | ForEach-Object { "seen $_" })
$lines | Set-Content $Out
