# ts-phase.ps1 -Tag <t> -Exe <path> [-Repeat <input id>] -- run a phase-timed checkdemo the runner's way (.NET Process,
# stdout AND stderr redirected and drained), over the 101 kit inputs, or over ONE input repeated 101 times.
param([Parameter(Mandatory)][string]$Tag, [Parameter(Mandatory)][string]$Exe, [string]$Repeat = '')
$O = 'C:\go2cs-tmp\ts-prof\phase'; New-Item -ItemType Directory -Force -Path $O | Out-Null
$all = Get-Content 'C:\go2cs-tmp\ts-prof\units-101.local.args' | Where-Object { $_ }
$argList = if ($Repeat) { $one = $all | Where-Object { $_ -like "*/$Repeat.txt" } | Select-Object -First 1; 1..101 | ForEach-Object { $one } } else { $all }
$psi = [System.Diagnostics.ProcessStartInfo]::new($Exe); $psi.UseShellExecute = $false; $psi.RedirectStandardOutput = $true; $psi.RedirectStandardError = $true
$psi.WorkingDirectory = 'C:\go2cs-tmp\ts-prof\min'
foreach ($a in $argList) { $psi.ArgumentList.Add($a) }
foreach ($k in 'DOTNET_ROLL_FORWARD','GOFLAGS','DOTNET_gcServer','GOGC','GOMAXPROCS') { $psi.Environment.Remove($k) | Out-Null }
$psi.Environment['DOTNET_ROOT'] = '%USERPROFILE%\dotnet10'
$sw = [System.Diagnostics.Stopwatch]::StartNew()
$p = [System.Diagnostics.Process]::Start($psi)
$outTask = $p.StandardOutput.ReadToEndAsync(); $errTask = $p.StandardError.ReadToEndAsync()
$p.WaitForExit(); $wall = $sw.ElapsedMilliseconds
[System.IO.File]::WriteAllText("$O\$Tag.stdout", $outTask.Result); [System.IO.File]::WriteAllText("$O\$Tag.stderr", $errTask.Result)
$hash = [System.BitConverter]::ToString([System.Security.Cryptography.SHA256]::HashData([System.Text.Encoding]::UTF8.GetBytes($outTask.Result))).Replace('-','').Substring(0,16)
$total = ($errTask.Result -split "`n" | Where-Object { $_ -like 'TOTAL*' }) -join ''
"tag=$Tag rc=$($p.ExitCode) wall_ms=$wall cpu_ms=$([int]$p.TotalProcessorTime.TotalMilliseconds) stdout_sha16=$hash inputs=$($argList.Count) repeat=$Repeat $($total.Trim())"
