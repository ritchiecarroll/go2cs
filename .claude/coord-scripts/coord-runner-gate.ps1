# coord-runner-gate.ps1 -- a filtered behavioral run (all four phases) in the coordinator worktree,
# driven through cmd so the wrapper's Stop preference cannot die on the runner's stderr.
param(
    [string] $Worktree = 'C:\Projects\go2cs\.claude\worktrees\musing-moser-d4552c',
    [string] $Filter = 'Defer'
)
$ErrorActionPreference = 'Continue'
$env:DOTNET_ROOT = '$env:USERPROFILE\dotnet10'
$env:GOROOT = '$env:USERPROFILE\sdk\go1.23.12'
$env:PATH = "$env:DOTNET_ROOT;$env:GOROOT\bin;$env:PATH"
$env:MSBUILDDISABLENODEREUSE = '1'
function Stamp([string] $m) { Write-Output ("[{0}] {1}" -f (Get-Date -Format 'yyyy-MM-dd HH:mm:ss'), $m) }
$beh = Join-Path $Worktree 'src\tests\Behavioral'
$wrapper = Join-Path $beh 'run-behavioral.ps1'
if (-not (Test-Path $wrapper)) { Stamp "MISSING: $wrapper"; exit 1 }
Stamp "RUNNER GATE START head=$((& git -C $Worktree rev-parse HEAD 2>&1) -join ' ') filter=$Filter"
Stamp ("go: " + ((& go version 2>&1) -join ' ') + "  dotnet: " + ((& dotnet --version 2>&1) -join ' '))
Set-Location $beh
if ((Get-Location).Path -ne $beh) { Stamp "cd FAILED"; exit 1 }
$t0 = Get-Date
cmd /c "powershell -NoProfile -ExecutionPolicy Bypass -File $wrapper --filter $Filter 2>&1"
Stamp ("RUNNER GATE END exit=$LASTEXITCODE elapsed=" + [int]((Get-Date) - $t0).TotalSeconds + "s")
$dirty = @(& git -C $Worktree status --short 2>&1)
Stamp ("post-run git status entries: " + $dirty.Count)
exit 0
