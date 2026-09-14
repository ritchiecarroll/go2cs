$log = '$env:USERPROFILE\AppData\Local\Temp\claude\C--Projects-go2cs--claude-worktrees-go2cs-fleet-train-19-7d1cc7\388822cf-bade-49e8-96a9-7ab378101ad4\scratchpad\coord-t33-canary-0907-0103.log'
$env:CANLOG = $log
$p = Start-Process -FilePath 'powershell' -ArgumentList @('-NoProfile','-File','C:\Projects\go2cs\.claude\coord-scripts\coord-t33-canaries.ps1') -WindowStyle Hidden -PassThru -RedirectStandardOutput $log -RedirectStandardError ($log + '.err')
Write-Host ('PID=' + $p.Id); Write-Host ('LOG=' + $log)
