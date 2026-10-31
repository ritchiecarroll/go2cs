# affinity-sampler.ps1 <out> <stopfile> -- every 2 s, one line per live spinner or R0 test process: time, name, pid,
# affinity mask, priority class. Runs until <stopfile> exists. Unpinned and tiny: it is not part of the load.
param([string]$Out, [string]$StopFile)
while (-not (Test-Path $StopFile)) {
  $t = Get-Date -Format 'HH:mm:ss'
  foreach ($p in @(Get-Process -Name spin, GoroutinePanicAfterDeferredDone, GoroutineRecoveringWhenMainReturns -ErrorAction SilentlyContinue)) {
    try { "{0}`t{1}`t{2}`t0x{3:X}`t{4}" -f $t, $p.ProcessName, $p.Id, [int64]$p.ProcessorAffinity, $p.PriorityClass | Add-Content -Path $Out -Encoding ascii } catch { }
  }
  Start-Sleep -Seconds 2
}
