# coord-nistec-pair.ps1 -- PAIRED re-measure of the crypto/internal/nistec COST canary: a control worktree pinned at
# a base SHA versus the coordinator worktree's head, alternated (A B A B) on the same machine in the same hour, SOLO.
# The sweep's "[Ns]" wall for the row is the figure of record (the same instrument the 269/274 baselines used).
# One discarded warm-up sweep in the control tree first: the main tree is warm from its battery, the control is cold.
param(
    [switch] $AllowBusy,
    [string] $Main = 'C:\Projects\go2cs\.claude\worktrees\musing-moser-d4552c',
    [string] $Control = 'C:\Projects\go2cs\.claude\worktrees\coord-nistec-ctrl',
    [string] $ControlSha = '092329148',
    [int] $Rounds = 2,
    [switch] $SkipWarmup,
    [string] $Package = 'crypto/internal/nistec',
    [string] $SweepTimeout = '10m'
)
$ErrorActionPreference = 'Continue'
$env:DOTNET_ROOT = '$env:USERPROFILE\dotnet10'
$env:GOROOT = '$env:USERPROFILE\sdk\go1.23.12'
$env:PATH = "$env:DOTNET_ROOT;$env:GOROOT\bin;$env:PATH"
$env:MSBUILDDISABLENODEREUSE = '1'
function Stamp([string] $m) { Write-Output ("[{0}] {1}" -f (Get-Date -Format 'yyyy-MM-dd HH:mm:ss'), $m) }
Stamp "NISTEC PAIR START main=$Main control=$Control@$ControlSha rounds=$Rounds"

# solo precondition: no converter, runner or test host alive anywhere on the box
$busy = Get-CimInstance Win32_Process | Where-Object { $_.Name -match '^(go2cs|BehavioralRunner|testhost|vstest\.console)\.exe$' }
if ($busy) { $busy | ForEach-Object { Stamp ("  BUSY: " + $_.Name + " pid=" + $_.ProcessId + " since " + $_.CreationDate) }; if ($AllowBusy) { Stamp "box is not solo -- CONTINUING under -AllowBusy (pair taken under load; both arms interleaved)" } else { Stamp "box is not solo -- ABORT"; exit 1 } }

# control worktree: create detached at the base SHA if absent; verify HEAD; build its converter at the exact path
if (-not (Test-Path (Join-Path $Control '.git'))) {
    Stamp "creating control worktree"
    & git.exe -C $Main worktree add --detach $Control $ControlSha 2>&1 | ForEach-Object { Stamp ("  " + $_) }
}
$ctlHead = (& git.exe -C $Control rev-parse --short HEAD 2>$null) -join ''
$ctlWant = (& git.exe -C $Main rev-parse --short $ControlSha 2>$null) -join ''
if ($ctlHead -ne $ctlWant) { Stamp "control HEAD $ctlHead != $ctlWant -- ABORT"; exit 1 }
$mainHead = (& git.exe -C $Main rev-parse --short HEAD 2>$null) -join ''
Stamp "control HEAD=$ctlHead main HEAD=$mainHead"
$ctlExe = Join-Path $Control 'src\go2cs\bin\go2cs.exe'
if (-not (Test-Path $ctlExe)) {
    Set-Location (Join-Path $Control 'src\go2cs')
    cmd /c "go build -o `"$ctlExe`" . 2>&1" | ForEach-Object { Stamp ("  " + $_) }
    if (-not (Test-Path $ctlExe)) { Stamp "control converter not at $ctlExe -- ABORT"; exit 1 }
}
$mainExe = Join-Path $Main 'src\go2cs\bin\go2cs.exe'
if (-not (Test-Path $mainExe)) { Stamp "main converter not at $mainExe -- ABORT"; exit 1 }
Stamp ("control exe: " + (Get-Item $ctlExe).Length + " B " + (Get-Item $ctlExe).LastWriteTime + "; main exe: " + (Get-Item $mainExe).Length + " B " + (Get-Item $mainExe).LastWriteTime)

function Invoke-Row([string] $tree, [string] $label) {
    $sweep = Join-Path $tree 'src\run-validated-sweep.ps1'
    Set-Location (Join-Path $tree 'src')
    $t0 = Get-Date
    $out = cmd /c "powershell -NoProfile -ExecutionPolicy Bypass -File $sweep -Filter $Package -Exact -TestTimeout $SweepTimeout -SkipBuild 2>&1"
    $line = ($out | Select-String -Pattern ('^\s*(PASS|FAIL)\s+' + [regex]::Escape($Package) + '\s')) | Select-Object -First 1
    $txt = if ($line) { $line.Line.Trim() } else { '(no verdict line)' }
    $secs = if ($txt -match '\[(\d+)s\]') { [int]$Matches[1] } else { -1 }
    Write-Host ("[{0}]   {1} : {2}  (leg wall {3}s)" -f (Get-Date -Format 'yyyy-MM-dd HH:mm:ss'), $label, $txt, [int]((Get-Date) - $t0).TotalSeconds)
    # restore sweep dirt + the pipeline's ignored record files
    & git.exe -C $tree checkout HEAD -- src/core docs/validation/current 2>$null
    & git.exe -C $tree clean -fdq -- src/core 2>$null
    $pkgDir = Join-Path $tree ('src\core\' + ($Package -replace '/', '\'))
    foreach ($r in 'go2cs_test_manifest.json','go2cs_test_comparison.json','go2cs_test_results.json','go2cs_test_results.xml') { $p = Join-Path $pkgDir $r; if (Test-Path $p) { Remove-Item $p -Force } }
    $cmp = Join-Path $pkgDir 'go2cs_test_comparison'; if (Test-Path $cmp) { Remove-Item $cmp -Recurse -Force }
    return [int]$secs
}

if (-not $SkipWarmup) { Stamp "warm-up (control, discarded)"; $null = Invoke-Row $Control 'warm-up control' } else { Stamp "warm-up skipped (both trees warm)" }
$ctlSecs = @(); $mainSecs = @()
for ($i = 1; $i -le $Rounds; $i++) {
    $ctlSecs += Invoke-Row $Control  ("round $i control $ctlHead")
    $mainSecs += Invoke-Row $Main    ("round $i main    $mainHead")
}
Stamp ("control $ctlHead : " + ($ctlSecs -join ', ') + " s")
Stamp ("main    $mainHead : " + ($mainSecs -join ', ') + " s")
$ca = ($ctlSecs | Measure-Object -Average).Average; $ma = ($mainSecs | Measure-Object -Average).Average
if ($ca -gt 0) { Stamp ("mean control {0:N0} s, mean main {1:N0} s, delta {2:+0.0;-0.0}%" -f $ca, $ma, (($ma - $ca) / $ca * 100)) }
Stamp ("post-run git status: main " + ((& git.exe -C $Main status --porcelain 2>$null) | Measure-Object).Count + " entries, control " + ((& git.exe -C $Control status --porcelain 2>$null) | Measure-Object).Count + " entries")
Stamp "NISTEC PAIR END"
