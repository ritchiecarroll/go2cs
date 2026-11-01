# stop-r3.ps1 [-Kill] -- list (or, with -Kill, stop) the R3 driver's processes: the bash running r3-run.sh, the bash
# running race3.sh, the spinner (pado\go\spin\spin.exe) and the red test exes (r1\jit-red-*). Exact PIDs after a
# command-line or image-path check; the driver first, so it cannot start the next step.
param([switch]$Kill)
$all = @(Get-CimInstance Win32_Process)
$drv = @($all | Where-Object { $_.Name -eq 'bash.exe' -and $_.CommandLine -match 'r1/r3-run\.sh' })
$race = @($all | Where-Object { $_.Name -eq 'bash.exe' -and $_.CommandLine -match 'pado/race3\.sh' })
$spin = @($all | Where-Object { $_.Name -eq 'spin.exe' -and "$($_.ExecutablePath)" -like '*\pado\go\spin\spin.exe' })
$red = @($all | Where-Object { "$($_.ExecutablePath)" -like '*\pado\r1\jit-red-*' })
foreach ($g in @(@('driver', $drv), @('race3', $race), @('spin', $spin), @('red-exe', $red))) {
  foreach ($p in $g[1]) {
    "{0} {1} {2}" -f $g[0], $p.ProcessId, $p.Name
    if ($Kill) { Stop-Process -Id $p.ProcessId -Force -ErrorAction SilentlyContinue }
  }
}
