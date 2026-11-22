<#
.SYNOPSIS
    The M:N spike's phase 2 sampler (docs/phase4/DATA-mn-spike-2.md). Starts one program, samples it every 250 ms
    (OS threads, private bytes), keeps its standard output in -OutFile, and stops it past -MaxPrivateGB or
    -TimeoutSeconds. Prints one READING line: the verdict (EXITED / STOPPED-MEMORY / STOPPED-TIME), the wall time,
    the peaks, the exit code and the SHA-256 of the output with carriage returns removed.

.DESCRIPTION
    The program reads its settings from the environment the caller set (MN_STORM_N, MN_STORM_HOLD_MS), so the same
    line measures the Go build and the converted build. A converted program's standard output is block-buffered
    when redirected, so no reading depends on when a line arrives: the peak thread count is the reading, and a
    program that holds its goroutines parked (MN_STORM_HOLD_MS) is sampled while it holds them.

    The process is stopped by its own Process object, never by name.
#>
param(
    [Parameter(Mandatory)][string]$Exe,
    [Parameter(Mandatory)][string]$OutFile,
    [double]$MaxPrivateGB = 12,
    [int]$TimeoutSeconds = 600,
    [string]$Label = 'shape'
)
$ErrorActionPreference = 'Stop'

$sw = [System.Diagnostics.Stopwatch]::StartNew()
$proc = Start-Process -FilePath $Exe -RedirectStandardOutput $OutFile -NoNewWindow -PassThru
$handle = $proc.Handle   # held so ExitCode stays readable after the process ends

$verdict = 'EXITED'
$peakPrivate = 0; $peakThreads = 0; $samples = 0

while (-not $proc.HasExited) {
    try {
        $proc.Refresh()
        $private = $proc.PrivateMemorySize64; $threads = $proc.Threads.Count
    } catch { Start-Sleep -Milliseconds 250; continue }

    $samples++
    if ($private -gt $peakPrivate) { $peakPrivate = $private }
    if ($threads -gt $peakThreads) { $peakThreads = $threads }

    if ($private -gt $MaxPrivateGB * 1GB) { $verdict = 'STOPPED-MEMORY'; break }
    if ($sw.Elapsed.TotalSeconds -gt $TimeoutSeconds) { $verdict = 'STOPPED-TIME'; break }
    Start-Sleep -Milliseconds 250
}

if (-not $proc.HasExited) {
    $proc.Kill($true)   # this script's own child, by its Process object
}
$proc.WaitForExit(30000) | Out-Null
$wall = [math]::Round($sw.Elapsed.TotalSeconds, 1)

$text = ''
try { $text = [System.IO.File]::ReadAllText($OutFile).Replace("`r", '') } catch { }
$sha = [System.BitConverter]::ToString([System.Security.Cryptography.SHA256]::HashData([System.Text.Encoding]::UTF8.GetBytes($text))).Replace('-', '').ToLowerInvariant().Substring(0, 16)
$lines = @($text.Split("`n") | Where-Object { $_ -ne '' }).Count

"READING $Label verdict=$verdict wall=${wall}s peak-threads=$peakThreads peak-private=$([math]::Round($peakPrivate / 1MB)) MB samples=$samples exit=$($proc.ExitCode) out-lines=$lines out-sha=$sha"
