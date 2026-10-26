<#
.SYNOPSIS
    Runs the ParkStorm demonstrator (parkstorm/) once and samples it while its goroutines are parked.

.DESCRIPTION
    Starts <Exe> <N> <Hold>, with GO2CS_GOROUTINE_STACK set to -Reserve when one is given, and samples the
    process's private memory, working set and OS thread count every 500 ms. It records a snapshot when the
    program prints PARKED, and stops the run past -MaxPrivateGB of private memory or -TimeoutSeconds. Prints
    one tab-separated result line:

        reserve  n  parked_s  private_mb  working_set_mb  threads  released_s  outcome

    Windows (it reads the process through System.Diagnostics.Process); park-storm.sh gives the same columns on
    linux.

.EXAMPLE
    ./park-storm.ps1 -Exe ./bin/Release/net10.0/parkstorm.exe -N 100000 -Reserve 1MiB
#>
param(
    [Parameter(Mandatory)][string] $Exe,
    [Parameter(Mandatory)][int] $N,
    [int] $Hold = 5,
    [string] $Reserve = '',
    [double] $MaxPrivateGB = 20,
    [int] $TimeoutSeconds = 600
)

$ErrorActionPreference = 'Stop'

$out = [IO.Path]::GetTempFileName()
$saved = $env:GO2CS_GOROUTINE_STACK
if ($Reserve) { $env:GO2CS_GOROUTINE_STACK = $Reserve } else { Remove-Item Env:GO2CS_GOROUTINE_STACK -ErrorAction SilentlyContinue }

try {
    $process = Start-Process -FilePath (Resolve-Path $Exe).Path -ArgumentList "$N $Hold" -RedirectStandardOutput $out -NoNewWindow -PassThru
}
finally {
    if ($null -ne $saved) { $env:GO2CS_GOROUTINE_STACK = $saved } else { Remove-Item Env:GO2CS_GOROUTINE_STACK -ErrorAction SilentlyContinue }
}

$clock = [Diagnostics.Stopwatch]::StartNew()
$snapshot = $null; $outcome = 'ok'

while (-not $process.HasExited) {
    Start-Sleep -Milliseconds 500
    $process.Refresh()

    if ($null -eq $snapshot -and (Select-String -Path $out -Pattern '^PARKED ' -Quiet)) {
        $snapshot = [pscustomobject]@{
            Private = [math]::Round($process.PrivateMemorySize64 / 1MB)
            Working = [math]::Round($process.WorkingSet64 / 1MB)
            Threads = $process.Threads.Count
        }
    }

    if (-not $process.HasExited -and $process.PrivateMemorySize64 -gt $MaxPrivateGB * 1GB) { $outcome = 'stopped-memory'; $process.Kill(); break }
    if (-not $process.HasExited -and $clock.Elapsed.TotalSeconds -gt $TimeoutSeconds) { $outcome = 'stopped-time'; $process.Kill(); break }
}

$process.WaitForExit()
$text = Get-Content $out
Remove-Item $out -ErrorAction SilentlyContinue

$parked = ($text | Select-String '^PARKED \d+ goroutines=\d+ in ([0-9.]+)s' | Select-Object -First 1).Matches.Groups[1].Value
$released = ($text | Select-String '^RELEASED \d+ in ([0-9.]+)s' | Select-Object -First 1).Matches.Groups[1].Value
if ($outcome -eq 'ok' -and $process.ExitCode -ne 0) { $outcome = "exit-$($process.ExitCode)" }

$reserveLabel = if ($Reserve) { $Reserve } else { 'default' }
"$reserveLabel`t$N`t$parked`t$($snapshot.Private)`t$($snapshot.Working)`t$($snapshot.Threads)`t$released`t$outcome"
