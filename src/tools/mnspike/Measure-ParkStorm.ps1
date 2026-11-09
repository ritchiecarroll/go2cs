<#
.SYNOPSIS
    The M:N spike's ParkStorm harness (docs/phase4/DESIGN-mn-scheduler.md §8 item 5, gate G3; the sizing note's
    Appendix A protocol). Starts one ParkStorm process, samples it every 500 ms (private bytes, working set, OS
    threads), snapshots the sample at the moment it prints PARKED, and stops it past -MaxPrivateGB or -TimeoutSeconds.
    Prints one RESULT line: the verdict (PARKED / STOPPED-MEMORY / STOPPED-TIME / EXITED), the snapshot, the peak, and
    the last progress line the program printed.

.DESCRIPTION
    The process is stopped by its own PID, from the Process object this script started, never by name.
#>
param(
    [Parameter(Mandatory)][string]$Exe,
    [Parameter(Mandatory)][int]$Count,
    [int]$HoldSeconds = 5,
    [double]$MaxPrivateGB = 20,
    [int]$TimeoutSeconds = 600,
    [string]$Label = 'parkstorm'
)
$ErrorActionPreference = 'Stop'

# The child's stdout goes to a file the loop reads: an event handler would run on a pool thread with no runspace.
$out = Join-Path ([System.IO.Path]::GetTempPath()) "parkstorm-$Label-$PID.out"
$sw = [System.Diagnostics.Stopwatch]::StartNew()
$proc = Start-Process -FilePath $Exe -ArgumentList "$Count", "$HoldSeconds" -RedirectStandardOutput $out -NoNewWindow -PassThru

$verdict = $null; $snapshot = $null; $last = ''
$peakPrivate = 0; $peakThreads = 0
$seen = [System.Collections.Generic.List[string]]::new()

function Read-NewLines {
    $all = @()
    try { $all = @([System.IO.File]::ReadAllLines($out)) } catch { return }
    for ($i = $seen.Count; $i -lt $all.Count; $i++) {
        $seen.Add($all[$i]); $script:last = $all[$i]; Write-Host "  [$([int]$sw.Elapsed.TotalSeconds)s] $($all[$i])"
    }
}

while ($true) {
    Read-NewLines

    if ($proc.HasExited) { if (-not $verdict) { $verdict = if ($snapshot) { 'PARKED' } else { 'EXITED' } }; break }

    try {
        $proc.Refresh()
        $private = $proc.PrivateMemorySize64; $working = $proc.WorkingSet64; $threads = $proc.Threads.Count
    } catch { Start-Sleep -Milliseconds 500; continue }

    if ($private -gt $peakPrivate) { $peakPrivate = $private }
    if ($threads -gt $peakThreads) { $peakThreads = $threads }

    if (-not $snapshot -and ($seen | Where-Object { $_ -like 'PARKED *' })) {
        $snapshot = "private=$([math]::Round($private / 1MB)) MB working=$([math]::Round($working / 1MB)) MB threads=$threads at $([math]::Round($sw.Elapsed.TotalSeconds, 1))s"
    }

    if ($private -gt $MaxPrivateGB * 1GB) { $verdict = 'STOPPED-MEMORY'; break }
    if ($sw.Elapsed.TotalSeconds -gt $TimeoutSeconds) { $verdict = 'STOPPED-TIME'; break }
    Start-Sleep -Milliseconds 500
}

if (-not $proc.HasExited) {
    $proc.Kill($true)   # this script's own child, by its Process object
    $proc.WaitForExit(30000) | Out-Null
}
Read-NewLines
Remove-Item -LiteralPath $out -ErrorAction SilentlyContinue

# The converted program's stdout is block-buffered when redirected to a file, so its lines arrive at exit and a run
# the watchdog stops loses them. The verdict therefore reads the PARKED line at the end, and a stopped run's progress
# is its peak thread count (one OS thread per started goroutine under O0).
if ($verdict -eq 'EXITED' -and ($seen | Where-Object { $_ -like 'PARKED *' })) { $verdict = 'PARKED' }

"RESULT $Label n=$Count verdict=$verdict wall=$([math]::Round($sw.Elapsed.TotalSeconds, 1))s snapshot=[$snapshot] peak-private=$([math]::Round($peakPrivate / 1MB)) MB peak-threads=$peakThreads last=[$last] exit=$(if ($proc.HasExited) { $proc.ExitCode } else { 'running' })"
