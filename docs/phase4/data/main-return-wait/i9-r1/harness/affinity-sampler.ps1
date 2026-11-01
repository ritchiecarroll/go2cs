# affinity-sampler.ps1 <out> <stopfile> -- every 2 s, one line per live spinner or R1-R3 test process: time, name, pid,
# affinity mask, priority class, and the folder the exe ran from (jit-*, aot-*, go-*: which build; "?" if unreadable).
# Runs until <stopfile> exists. Unpinned and tiny: it is not part of the load.
param([string]$Out, [string]$StopFile)
while (-not (Test-Path $StopFile)) {
  $t = Get-Date -Format 'HH:mm:ss'
  foreach ($p in @(Get-Process -Name spin, GoroutinePanicAfterDeferredDone, GoroutineRecoveringWhenMainReturns, syscallbench -ErrorAction SilentlyContinue)) {
    $dir = '?'
    try { $dir = Split-Path -Leaf (Split-Path -Parent $p.Path) } catch { }
    try { "{0}`t{1}`t{2}`t0x{3:X}`t{4}`t{5}" -f $t, $p.ProcessName, $p.Id, [int64]$p.ProcessorAffinity, $p.PriorityClass, $dir | Add-Content -Path $Out -Encoding ascii } catch { }
  }
  Start-Sleep -Seconds 2
}
