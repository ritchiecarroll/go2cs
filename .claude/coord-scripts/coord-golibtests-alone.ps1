# coord-golibtests-alone.ps1 -- the each-class-ALONE ordering control for the TestHost.Run driver classes.
# Runs each named class as its own MSTest invocation (--no-build, after the slnx leg); a class that aborts
# alone is the abort the host fix exists for. Exit = number of classes that did not complete cleanly.
param(
    [string] $Worktree = 'C:\Projects\go2cs\.claude\worktrees\musing-moser-d4552c',
    [string] $Classes = 'AllocationCounterTests,FixtureStagingLoudSkipTests,HostEnvironmentVisibilityTests,HostTestMainParseOrderTests,HostUnknownFlagPassThroughTests'
)
$ErrorActionPreference = 'Continue'
$env:DOTNET_ROOT = '$env:USERPROFILE\dotnet10'
$env:GOROOT = '$env:USERPROFILE\sdk\go1.23.12'
$env:PATH = "$env:DOTNET_ROOT;$env:GOROOT\bin;$env:PATH"
$env:MSBUILDDISABLENODEREUSE = '1'
function Stamp([string] $m) { Write-Output ("[{0}] {1}" -f (Get-Date -Format 'yyyy-MM-dd HH:mm:ss'), $m) }
$proj = Get-ChildItem -Path (Join-Path $Worktree 'src\tests\GolibTests') -Filter '*.csproj' -File | Select-Object -First 1
if (-not $proj) { Stamp "MISSING GolibTests csproj"; exit 1 }
Stamp ("GOLIBTESTS EACH-CLASS-ALONE START head=" + ((& git.exe -C $Worktree rev-parse --short HEAD 2>$null) -join ''))
$failed = 0
foreach ($c in ($Classes -split ',')) {
    $c = $c.Trim(); if ($c -eq '') { continue }
    $t0 = Get-Date
    $out = cmd /c "dotnet test `"$($proj.FullName)`" -c Debug --no-build --filter `"FullyQualifiedName~$c`" -p:go2csPath=$($Worktree -replace '\\','/')/src/ 2>&1"
    $code = $LASTEXITCODE
    $line = ($out | Select-String -Pattern 'Passed!|Failed!|Test Run Aborted|No test is available|crashed' | Select-Object -First 2 | ForEach-Object { $_.Line.Trim() }) -join ' | '
    $aborted = ($out | Select-String -Pattern 'Test Run Aborted|crashed' | Measure-Object).Count -gt 0
    Stamp ("  $c alone: exit=$code aborted=$aborted elapsed=" + [int]((Get-Date) - $t0).TotalSeconds + "s :: $line")
    if ($aborted -or $code -ne 0) { $failed++ }
}
Stamp "GOLIBTESTS EACH-CLASS-ALONE END failed=$failed"
exit $failed
