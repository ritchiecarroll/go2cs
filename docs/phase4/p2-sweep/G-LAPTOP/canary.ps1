# survival sleeper: same launch shape as the driver; appends a timestamp every 60 s for 30 min
$log = 'C:\h10\p2\G-LAPTOP\canary.log'
for ($i = 0; $i -lt 30; $i++) { [IO.File]::AppendAllText($log, ((Get-Date -Format 's') + " pid $PID tick $i`n")); Start-Sleep -Seconds 60 }
[IO.File]::AppendAllText($log, ((Get-Date -Format 's') + " pid $PID done`n"))
