$log = '<evidence-root>\canary.log'
for ($i = 0; $i -lt 30; $i++) {
    Add-Content -Path $log -Value ("{0} pid={1} tick={2}" -f (Get-Date).ToUniversalTime().ToString('o'), $PID, $i)
    Start-Sleep -Seconds 60
}
Add-Content -Path $log -Value ("{0} pid={1} DONE" -f (Get-Date).ToUniversalTime().ToString('o'), $PID)
