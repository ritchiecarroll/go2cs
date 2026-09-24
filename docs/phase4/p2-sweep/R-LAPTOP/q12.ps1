$ErrorActionPreference = 'Continue'
"PS=$($PSVersionTable.PSVersion)"
"GoTargetOS present=" + [bool](Test-Path Env:GoTargetOS)
"DOTNET_Tiered*/DOTNET_TC_*/COMPlus_* names: [" + ((Get-ChildItem Env: | Where-Object { $_.Name -like 'DOTNET_Tiered*' -or $_.Name -like 'DOTNET_TC_*' -or $_.Name -like 'COMPlus_*' } | ForEach-Object Name) -join ',') + "]"
"NUGET_API_KEY present=" + [bool](Test-Path Env:NUGET_API_KEY) + " NuGetCertFingerprint present=" + [bool](Test-Path Env:NuGetCertFingerprint)
"GOFLAGS env present=" + [bool](Test-Path Env:GOFLAGS)
$cs = Get-CimInstance Win32_ComputerSystem; $os = Get-CimInstance Win32_OperatingSystem
"logicalCPUs=$([Environment]::ProcessorCount) RAM_GB=$([math]::Round($cs.TotalPhysicalMemory/1GB,1)) lastBoot=$($os.LastBootUpTime.ToUniversalTime().ToString('o')) uptime_h=$([math]::Round(((Get-Date)-$os.LastBootUpTime).TotalHours,1))"
"git=$(& git --version)"
"freeGB C: (wt and profile drive)=$([math]::Round((Get-PSDrive C).Free/1GB,1)) profileDrive=$($env:USERPROFILE.Substring(0,2)) tempDrive=$($env:TEMP.Substring(0,2))"
"go2cs.exe by image path=" + @(Get-CimInstance Win32_Process | Where-Object { $_.ExecutablePath -and ([IO.Path]::GetFileName($_.ExecutablePath) -ieq 'go2cs.exe') }).Count
"test/battery processes: " + ((Get-CimInstance Win32_Process | Where-Object { $_.Name -match '\.tests\.exe$|^go2cs.*\.exe$|BehavioralRunner|testhost' } | ForEach-Object { $_.Name }) -join ',')
"dotnet processes=" + @(Get-Process dotnet -ErrorAction SilentlyContinue).Count + " go processes=" + @(Get-Process go -ErrorAction SilentlyContinue).Count
"LongPathsEnabled=" + (Get-ItemProperty 'HKLM:\SYSTEM\CurrentControlSet\Control\FileSystem' -Name LongPathsEnabled -ErrorAction SilentlyContinue).LongPathsEnabled
"PartOfDomain=" + $cs.PartOfDomain
foreach ($c in 'gcc','cc','clang') { "$c resolves=" + [bool](Get-Command $c -CommandType Application -ErrorAction SilentlyContinue) }
"echo executable on PATH=" + [bool](Get-Command echo -CommandType Application -ErrorAction SilentlyContinue) + " (" + ((Get-Command echo -CommandType Application -ErrorAction SilentlyContinue | Select-Object -First 1).Source -replace '^.*\(Git|msys64|usr)\','<...>\$1\') + ")"
"NET: " + ((Get-NetConnectionProfile | ForEach-Object { "$($_.InterfaceAlias) [$($_.NetworkCategory), $($_.IPv4Connectivity)]" }) -join '; ')
"WSL distros running=" + @((& wsl.exe -l --running 2>$null) | Where-Object { $_ -match '\S' -and $_ -notmatch 'Windows Subsystem|no running|There are no' }).Count
