# ts-profile.ps1 -Tag <name> [-Profiles <dotnet-trace profiles>] [-Env k=v,...] [-GcDumpAtMs <ms>]
# Launch checkdemo the runner's way (.NET Process, stdout redirected to a pipe this script drains), with the 101 kit
# inputs as arguments, working dir = an empty folder; attach dotnet-trace by PID right after start (so runtime start-up
# is not traced); optionally take one gcdump at a fixed offset. Writes <tag>.nettrace, <tag>.stdout, <tag>.meta.
param([Parameter(Mandatory)][string]$Tag, [string]$Profiles = 'dotnet-sampled-thread-time,gc-verbose', [string[]]$Env = @(), [int]$GcDumpAtMs = 0, [switch]$NoTrace)
$O = 'C:\go2cs-tmp\ts-prof\runs'; New-Item -ItemType Directory -Force -Path $O | Out-Null
$exe = 'C:\go2cs-tmp\ts-prof\typescript-go-cs\.artifacts\bin\538abe619327\Release\net10.0\checkdemo.exe'
$argList = Get-Content 'C:\go2cs-tmp\ts-prof\units-101.local.args' | Where-Object { $_ }
$psi = [System.Diagnostics.ProcessStartInfo]::new($exe); $psi.UseShellExecute = $false; $psi.RedirectStandardOutput = $true; $psi.WorkingDirectory = 'C:\go2cs-tmp\ts-prof\min'
foreach ($a in $argList) { $psi.ArgumentList.Add($a) }
foreach ($k in 'DOTNET_ROLL_FORWARD','GOFLAGS','DOTNET_gcServer') { $psi.Environment.Remove($k) | Out-Null }
$psi.Environment['DOTNET_ROOT'] = '%USERPROFILE%\dotnet10'
foreach ($kv in $Env) { $k, $v = $kv -split '=', 2; $psi.Environment[$k] = $v }
$sw = [System.Diagnostics.Stopwatch]::StartNew()
$p = [System.Diagnostics.Process]::Start($psi)
$outTask = $p.StandardOutput.ReadToEndAsync()
$tr = $null
if (-not $NoTrace) {
  $tr = Start-Process -FilePath '%USERPROFILE%\.dotnet\tools\dotnet-trace.exe' -ArgumentList @('collect', '-p', "$($p.Id)", '--profile', $Profiles, '-o', "$O\$Tag.nettrace") -NoNewWindow -PassThru -RedirectStandardOutput "$O\$Tag.trace.log" -RedirectStandardError "$O\$Tag.trace.err"
}
if ($GcDumpAtMs -gt 0) {
  while ($sw.ElapsedMilliseconds -lt $GcDumpAtMs -and -not $p.HasExited) { Start-Sleep -Milliseconds 200 }
  if (-not $p.HasExited) { & '%USERPROFILE%\.dotnet\tools\dotnet-gcdump.exe' collect -p $p.Id -o "$O\$Tag.gcdump" *> "$O\$Tag.gcdump.log" }
}
$p.WaitForExit(); $wall = $sw.ElapsedMilliseconds
$out = $outTask.Result; [System.IO.File]::WriteAllText("$O\$Tag.stdout", $out)
if ($tr) { $tr.WaitForExit() }
$hash = [System.BitConverter]::ToString([System.Security.Cryptography.SHA256]::HashData([System.Text.Encoding]::UTF8.GetBytes($out))).Replace('-','').Substring(0,16)
"tag=$Tag rc=$($p.ExitCode) wall_ms=$wall cpu_ms=$([int]$p.TotalProcessorTime.TotalMilliseconds) stdout_sha16=$hash lines=$(($out -split "`n").Count) env=$($Env -join ';') trace=$(-not $NoTrace)" | Tee-Object "$O\$Tag.meta"
