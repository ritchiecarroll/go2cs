# coord-train-battery.ps1 -- gate the migration train at its tip inside the coordinator worktree.
# Legs: (1) converter suite -count=1, (2) full CNR, (3) os/user filtered sweep (-Exact).
# Runs at 'Continue' so a native stderr line cannot kill the wrapper; every runner is existence-asserted first.
param([string] $Worktree = 'C:\Projects\go2cs\.claude\worktrees\musing-moser-d4552c')

$ErrorActionPreference = 'Continue'
$env:DOTNET_ROOT = '$env:USERPROFILE\dotnet10'
$env:GOROOT = '$env:USERPROFILE\sdk\go1.23.12'
$env:PATH = "$env:DOTNET_ROOT;$env:GOROOT\bin;$env:PATH"
$env:MSBUILDDISABLENODEREUSE = '1'

function Stamp([string] $m) { Write-Output ("[{0}] {1}" -f (Get-Date -Format 'yyyy-MM-dd HH:mm:ss'), $m) }
function Elapsed([datetime] $t0) { return ([int] ((Get-Date) - $t0).TotalSeconds).ToString() + 's' }

Stamp "BATTERY START worktree=$Worktree"
Stamp ("go version: " + ((& go version 2>&1) -join ' '))
Stamp ("go env GOROOT: " + ((& go env GOROOT 2>&1) -join ' '))
Stamp ("dotnet --version: " + ((& dotnet --version 2>&1) -join ' '))
Stamp ("HEAD: " + ((& git -C $Worktree rev-parse HEAD 2>&1) -join ' '))
$dirty = @(& git -C $Worktree status --short 2>&1)
Stamp ("pre-battery git status entries: " + $dirty.Count)

$cnr    = Join-Path $Worktree 'src\tests\Behavioral\check-no-regression.ps1'
$sweep  = Join-Path $Worktree 'src\run-validated-sweep.ps1'
$goDir  = Join-Path $Worktree 'src\go2cs'
$goExe  = Join-Path $env:GOROOT 'bin\go.exe'
$dnExe  = Join-Path $env:DOTNET_ROOT 'dotnet.exe'
foreach ($p in @($cnr, $sweep, $goDir, $goExe, $dnExe)) {
    if (-not (Test-Path $p)) { Stamp "MISSING (existence assertion failed): $p"; Stamp "BATTERY ABORT"; exit 1 }
}
Stamp "existence assertions: all 5 present"

# ---- Leg 1: converter suite ----
Stamp "LEG1 START converter suite: go test -count=1 -timeout 30m ./..."
$t0 = Get-Date
Set-Location $goDir
if ((Get-Location).Path -ne $goDir) { Stamp "LEG1 cd FAILED -> $((Get-Location).Path)"; Stamp "BATTERY ABORT"; exit 1 }
cmd /c "go test -count=1 -timeout 30m ./... 2>&1"
Stamp ("LEG1 END exit=$LASTEXITCODE elapsed=" + (Elapsed $t0))

# ---- Leg 2: CNR ----
Stamp "LEG2 START CNR (check-no-regression.ps1, re-transpiles unconditionally)"
$t0 = Get-Date
$behDir = Join-Path $Worktree 'src\tests\Behavioral'
Set-Location $behDir
if ((Get-Location).Path -ne $behDir) { Stamp "LEG2 cd FAILED -> $((Get-Location).Path)"; Stamp "BATTERY ABORT"; exit 1 }
cmd /c "powershell -NoProfile -ExecutionPolicy Bypass -File $cnr 2>&1"
Stamp ("LEG2 END exit=$LASTEXITCODE elapsed=" + (Elapsed $t0))
$dirty = @(& git -C $Worktree status --short 2>&1)
Stamp ("post-CNR git status entries: " + $dirty.Count)
if ($dirty.Count -gt 0) { $dirty | Select-Object -First 40 | ForEach-Object { Stamp ("  dirty: " + $_) } }

# ---- Leg 3: os/user filtered sweep ----
Stamp "LEG3 START sweep: run-validated-sweep.ps1 -Filter os/user -Exact -TestTimeout 10m"
$t0 = Get-Date
$srcDir = Join-Path $Worktree 'src'
Set-Location $srcDir
if ((Get-Location).Path -ne $srcDir) { Stamp "LEG3 cd FAILED -> $((Get-Location).Path)"; Stamp "BATTERY ABORT"; exit 1 }
cmd /c "powershell -NoProfile -ExecutionPolicy Bypass -File $sweep -Filter os/user -Exact -TestTimeout 10m 2>&1"
Stamp ("LEG3 END exit=$LASTEXITCODE elapsed=" + (Elapsed $t0))
$dirty = @(& git -C $Worktree status --short 2>&1)
Stamp ("post-sweep git status entries: " + $dirty.Count)
if ($dirty.Count -gt 0) { $dirty | Select-Object -First 60 | ForEach-Object { Stamp ("  dirty: " + $_) } }

Stamp "BATTERY END"
exit 0
