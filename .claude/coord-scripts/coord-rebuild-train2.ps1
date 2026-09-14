# coord-rebuild-train2.ps1 -- reset the coordinator worktree to origin/master and re-merge train 2 in order
# on the lanes' CURRENT tips (R's rebased branch carrying the guard fix), then run the converter suite leg
# and the roster format guard at the rebuilt result. Docs merges are -m inline; the two big ones use files.
param(
    [string] $Worktree = 'C:\Projects\go2cs\.claude\worktrees\musing-moser-d4552c',
    [string] $Scratch  = 'C:\Projects\go2cs\.claude\coord-scripts',
    [string] $RTip     = ''   # expected sha of origin/claude/reflect-tail-lane-r-a20163 (abort if it differs)
)
$ErrorActionPreference = 'Continue'
$env:DOTNET_ROOT = '$env:USERPROFILE\dotnet10'
$env:GOROOT = '$env:USERPROFILE\sdk\go1.23.12'
$env:PATH = "$env:DOTNET_ROOT;$env:GOROOT\bin;$env:PATH"
$env:MSBUILDDISABLENODEREUSE = '1'
function Stamp([string] $m) { Write-Output ("[{0}] {1}" -f (Get-Date -Format 'yyyy-MM-dd HH:mm:ss'), $m) }
function Gitx { param([Parameter(ValueFromRemainingArguments)] $a) & git.exe -C $Worktree @a 2>&1 }

Stamp "REBUILD TRAIN 2 START"
$dirty = @(& git.exe -C $Worktree status --porcelain 2>$null)
if ($dirty.Count -gt 0) { Stamp "worktree dirty ($($dirty.Count)) -- ABORT"; $dirty | ForEach-Object { Stamp ("  " + $_) }; exit 1 }
Gitx fetch -q origin master claude/reflect-tail-lane-r-a20163 claude/g-typed-nil-unparked claude/c1-sweep-cgo-off-list claude/c1-board-cgo-class claude/laneR-board-unwrap-arm claude/c2-recon-go124 | Out-Null
$r = (Gitx rev-parse origin/claude/reflect-tail-lane-r-a20163) -join ''
if ($RTip -ne '' -and -not $r.StartsWith($RTip)) { Stamp "R tip is $r, expected $RTip -- ABORT"; exit 1 }
Stamp ("master=" + ((Gitx rev-parse --short origin/master) -join '') + " R=" + $r.Substring(0,9))
Gitx reset -q --hard origin/master | Out-Null
Stamp ("reset to " + ((Gitx rev-parse --short HEAD) -join ''))

$steps = @(
    @{ ref='origin/claude/reflect-tail-lane-r-a20163'; file=(Join-Path $Scratch 'coord-merge-r-42.txt') },
    @{ ref='origin/claude/g-typed-nil-unparked';       file=(Join-Path $Scratch 'coord-merge-g-unpark.txt') },
    @{ ref='origin/claude/c1-sweep-cgo-off-list';      file=(Join-Path $Scratch 'coord-merge-c1-cgo.txt') },
    @{ ref='origin/claude/c1-board-cgo-class';         file=(Join-Path $Scratch 'coord-merge-c1-board.txt') },
    @{ ref='origin/claude/laneR-board-unwrap-arm';     file=(Join-Path $Scratch 'coord-merge-r-board.txt') },
    @{ ref='origin/claude/c2-recon-go124';             file=(Join-Path $Scratch 'coord-merge-c2-recon3.txt') }
)
foreach ($s in $steps) {
    if (-not (Test-Path $s.file)) { Stamp ("missing message file " + $s.file + " -- ABORT"); exit 1 }
    $out = Gitx merge --no-ff -S -F $s.file $s.ref
    if ($LASTEXITCODE -ne 0) { Stamp ("MERGE FAILED for " + $s.ref); $out | ForEach-Object { Stamp ("  " + $_) }; exit 1 }
    Stamp ("merged " + $s.ref + " -> " + ((Gitx rev-parse --short HEAD) -join ''))
}
$dirty = @(& git.exe -C $Worktree status --porcelain 2>$null)
Stamp ("post-merge git status entries: " + $dirty.Count)

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
Stamp ("REBUILD TRAIN 2 END head=" + ((Gitx rev-parse --short HEAD) -join ''))
exit $suite
