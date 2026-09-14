# coord-finish-train2.ps1 -- the legs a rebuilt train owes at its FINAL head: converter suite, roster guard.
param([string] $Worktree = 'C:\Projects\go2cs\.claude\worktrees\musing-moser-d4552c')
$ErrorActionPreference = 'Continue'
$env:DOTNET_ROOT = '$env:USERPROFILE\dotnet10'
$env:GOROOT = '$env:USERPROFILE\sdk\go1.23.12'
$env:PATH = "$env:DOTNET_ROOT;$env:GOROOT\bin;$env:PATH"
$env:MSBUILDDISABLENODEREUSE = '1'
function Stamp([string] $m) { Write-Output ("[{0}] {1}" -f (Get-Date -Format 'yyyy-MM-dd HH:mm:ss'), $m) }
Stamp ("FINISH TRAIN 2 START head=" + ((& git.exe -C $Worktree rev-parse --short HEAD 2>$null) -join ''))
$dirty = @(& git.exe -C $Worktree status --porcelain 2>$null)
if ($dirty.Count -gt 0) { Stamp "worktree dirty ($($dirty.Count)) -- ABORT"; $dirty | ForEach-Object { Stamp ("  " + $_) }; exit 1 }
Stamp "SUITE LEG START"
$t0 = Get-Date
Set-Location (Join-Path $Worktree 'src\go2cs')
cmd /c "go test -count=1 -timeout 30m ./... 2>&1" | Select-String -Pattern '^(--- FAIL|FAIL|ok |panic:)' | ForEach-Object { Stamp ("  " + $_.Line) }
$suite = $LASTEXITCODE
Stamp ("SUITE LEG END exit=$suite elapsed=" + [int]((Get-Date) - $t0).TotalSeconds + "s")
Stamp "ROSTER GUARD"
Set-Location $Worktree
$g = cmd /c "powershell -NoProfile -ExecutionPolicy Bypass -File src\check-roster-format.ps1 2>&1"
$g | Select-Object -Last 1 | ForEach-Object { Stamp ("  " + $_) }
Stamp ("ROSTER GUARD exit=$LASTEXITCODE")
Stamp ("FINISH TRAIN 2 END head=" + ((& git.exe -C $Worktree rev-parse --short HEAD 2>$null) -join ''))
exit $suite
