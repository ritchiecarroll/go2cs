# Launch train 30's re-battery DETACHED, so it survives the session's turn boundary (a Bash background task is reaped
# with the session process tree -- measured again 2026-09-06, the chain died three minutes in with an empty task log).
$ErrorActionPreference = 'Continue'
$S = 'C:\Projects\go2cs\.claude\coord-scripts'
$log = Join-Path $S 'coord-train30-rebattery-detached.log'
$args = @('-lc', "bash $S/coord-train30-rebattery.sh")
$p = Start-Process -FilePath 'C:\Program Files\Git\bin\bash.exe' -ArgumentList $args -WindowStyle Hidden -RedirectStandardOutput $log -RedirectStandardError "$log.err" -PassThru
"DETACHED pid=$($p.Id) log=$log"
