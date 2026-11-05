# census.ps1 -- the box census for run.sh (protocol v2, amended 2026-11-05 BEFORE run2): every build/toolchain process by
# executable name, EXCEPT a dotnet.exe whose command line runs pwsh.dll (an interactive PowerShell host, such as the
# owner's idle terminal). Each such shell is listed as ADMITTED with its CPU seconds, so run.sh can require it idle
# (CPU time growth under 1 s between box start and box end). Windows PowerShell 5 compatible.
$names = @('dotnet.exe', 'go.exe', 'MSBuild.exe', 'link.exe', 'VBCSCompiler.exe', 'go2cs.exe', 'cl.exe', 'ilc.exe', 'crossgen2.exe')
'CPU=' + (Get-CimInstance Win32_Processor | Select-Object -First 1).Name
$procs = @(Get-CimInstance Win32_Process | Where-Object { $names -contains $_.Name })
$shells = @($procs | Where-Object { $_.Name -eq 'dotnet.exe' -and $_.CommandLine -match 'pwsh\.dll' })
$builds = @($procs | Where-Object { -not ($_.Name -eq 'dotnet.exe' -and $_.CommandLine -match 'pwsh\.dll') })
'build/toolchain processes: ' + $builds.Count
$builds | ForEach-Object { 'proc ' + $_.ProcessId + ' ' + $_.ExecutablePath }
'admitted pwsh hosts: ' + $shells.Count
$shells | ForEach-Object {
    $cpu = try { [math]::Round((Get-Process -Id $_.ProcessId -ErrorAction Stop).TotalProcessorTime.TotalSeconds, 2) } catch { -1 }
    'shell ' + $_.ProcessId + ' cpu_s=' + $cpu
}
