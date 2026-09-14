# coord-runtime-emission-control.ps1 -- the fresh-emission control for a per-package regen (i9's method):
# seed src/core into a temp root, run the three-target L3 emission for ONE package with the CURRENT converter,
# and report which files the run WROTE that differ from the worktree's committed tree (CR-stripped compare).
# A regen that is fully banked reports 0 real differences on all three targets.
param(
    [string] $Worktree = 'C:\Projects\go2cs\.claude\worktrees\musing-moser-d4552c',
    [string] $Package = 'runtime',
    [string] $Stage = 'D:\go2cs-scratch\coord-emission-control'
)
$ErrorActionPreference = 'Continue'
$env:DOTNET_ROOT = '$env:USERPROFILE\dotnet10'
$env:GOROOT = '$env:USERPROFILE\sdk\go1.23.12'
$env:PATH = "$env:DOTNET_ROOT;$env:GOROOT\bin;$env:PATH"
function Stamp([string] $m) { Write-Output ("[{0}] {1}" -f (Get-Date -Format 'yyyy-MM-dd HH:mm:ss'), $m) }
$exe = Join-Path $Worktree 'src\go2cs\bin\go2cs.exe'
Stamp ("EMISSION CONTROL START package=$Package head=" + ((& git.exe -C $Worktree rev-parse --short HEAD 2>$null) -join ''))
if (Test-Path $Stage) { Remove-Item -Recurse -Force $Stage -ErrorAction SilentlyContinue }
$root = Join-Path $Stage 'src'
New-Item -ItemType Directory -Force -Path $root | Out-Null
# seed: src/core WITHOUT build output, plus version.props and docs/validation (the README badges read them)
$t0 = Get-Date
& robocopy (Join-Path $Worktree 'src\core') (Join-Path $root 'core') /E /NFL /NDL /NJH /NJS /NC /NS /NP /XD bin obj Generated | Out-Null
Copy-Item (Join-Path $Worktree 'src\version.props') $root -Force
New-Item -ItemType Directory -Force -Path (Join-Path $Stage 'docs') | Out-Null
& robocopy (Join-Path $Worktree 'docs\validation') (Join-Path $Stage 'docs\validation') /E /NFL /NDL /NJH /NJS /NC /NS /NP | Out-Null
$seeded = (Get-ChildItem -Path (Join-Path $root 'core') -Recurse -Filter '*.cs' -File | Measure-Object).Count
$repo = (Get-ChildItem -Path (Join-Path $Worktree 'src\core') -Recurse -Filter '*.cs' -File | Where-Object { $_.FullName -notmatch '\\(bin|obj|Generated)\\' } | Measure-Object).Count
Stamp "seeded .cs: $seeded (repo: $repo) in $([int]((Get-Date) - $t0).TotalSeconds)s"
if ($seeded -ne $repo) { Stamp "SEED COUNT MISMATCH -- ABORT"; exit 1 }
Set-Location (Join-Path $Worktree 'src\go2cs')
cmd /c "go build -o `"$exe`" . 2>&1" | Out-Null
$sentinel = Get-Date
$t0 = Get-Date
$out = cmd /c "`"$exe`" -stdlib $Package -comments -platforms windows/amd64,linux/amd64,darwin/amd64 -platform-stage `"$Stage\stage`" -go2cspath `"$root`" 2>&1"
$code = $LASTEXITCODE
Stamp ("emission exit=$code elapsed=" + [int]((Get-Date) - $t0).TotalSeconds + "s; converter WARNINGs: " + (@($out | Select-String -Pattern 'WARNING') | Measure-Object).Count)
$out | Select-String -Pattern 'marker-gate|violation|did not fully type-check|panic:' | Select-Object -First 6 | ForEach-Object { Stamp ("  " + $_.Line.Trim()) }
# classify: files the run WROTE (mtime >= sentinel) under the package, compared CR-stripped against the worktree
$pkgRel = 'core\' + ($Package -replace '/', '\')
$written = Get-ChildItem -Path (Join-Path $root $pkgRel) -Recurse -File -Include '*.cs','*.csproj','*.cs.auto' | Where-Object { $_.LastWriteTime -ge $sentinel }
$differ = 0; $identical = 0; $absent = 0
foreach ($f in $written) {
    $rel = $f.FullName.Substring((Join-Path $root '').Length)
    $wt = Join-Path (Join-Path $Worktree 'src') $rel
    if (-not (Test-Path $wt)) { $absent++; Stamp ("  ABSENT in worktree: " + $rel); continue }
    $a = ([System.IO.File]::ReadAllText($f.FullName)) -replace "`r", ''
    $b = ([System.IO.File]::ReadAllText($wt)) -replace "`r", ''
    if ($a -eq $b) { $identical++ } else { $differ++; Stamp ("  DIFFERS: " + $rel) }
}
Stamp ("written=" + $written.Count + " identical=$identical differ=$differ absent-in-worktree=$absent")
Stamp "EMISSION CONTROL END"
if ($code -ne 0 -or $differ -gt 0 -or $absent -gt 0) { exit 1 } else { exit 0 }
