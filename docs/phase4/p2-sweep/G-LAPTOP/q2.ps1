$names = foreach ($s in 'Machine','User') { [Environment]::GetEnvironmentVariables($s).Keys | Where-Object { $_ -match '^(GoTargetOS|GOFLAGS|DOTNET_Tiered|DOTNET_TC_|COMPlus_|NUGET_API_KEY|NuGetCertFingerprint)' } | ForEach-Object { "${s}:$_" } }
"Q1 Machine/User-scope names: [" + ($names -join ' ') + "]"
$cs = Get-CimInstance Win32_ComputerSystem; $os = Get-CimInstance Win32_OperatingSystem
"Q2 logical CPUs: $($cs.NumberOfLogicalProcessors); RAM: {0:N1} GB; last boot: {1}; uptime: {2:N1} h" -f ($cs.TotalPhysicalMemory/1GB), $os.LastBootUpTime, ((Get-Date) - $os.LastBootUpTime).TotalHours
"Q2 domain-joined: $($cs.PartOfDomain)"
"Q2 free C: (wt drive = profile drive): {0:N1} GB" -f ((Get-PSDrive C).Free/1GB)
"Q2 LongPathsEnabled: " + (Get-ItemProperty 'HKLM:\SYSTEM\CurrentControlSet\Control\FileSystem').LongPathsEnabled
$g = @(Get-CimInstance Win32_Process -Filter "Name='go2cs.exe'"); "Q2 go2cs.exe processes (by image path): $($g.Count)"; $g | ForEach-Object { "   " + $_.ExecutablePath }
$busy = @(Get-CimInstance Win32_Process | Where-Object { $_.CommandLine -match 'run-validated-sweep|run-h10-|go2cs(\.exe)? -tests|\.tests\.exe|go test ' }); "Q2 battery-like processes: $($busy.Count)"; $busy | ForEach-Object { "   pid $($_.ProcessId) $($_.Name)" }
foreach ($c in 'gcc','cc','clang') { "Q2 C-toolchain $c on PATH: " + [bool](Get-Command $c -CommandType Application -ErrorAction SilentlyContinue) }
$e = Get-Command echo -CommandType Application -ErrorAction SilentlyContinue | Select-Object -First 1; "Q4 echo (Application) on the sweep's PATH: " + $(if ($e) { 'present (' + ($e.Source -replace 'Users\[^\]+','Users\<u>') + ')' } else { 'absent' })
"Q2 powershell edition: $($PSVersionTable.PSEdition) $($PSVersionTable.PSVersion)"
