# coord-union-battery.ps1 -- union gate for a merge result in the coordinator worktree.
# Legs: (1) converter suite -count=1, (2) full CNR, (3) filtered -Exact sweeps for each package in -Sweeps.
# Runs at 'Continue' so a native stderr line cannot kill the wrapper; every runner is existence-asserted first.
param(
    [string] $Worktree = 'C:\Projects\go2cs\.claude\worktrees\musing-moser-d4552c',
    [string] $Sweeps = '',
    [string] $SweepTimeout = '10m',
    [switch] $SkipSuite,
    [switch] $SkipCNR,
    [switch] $SweepBuildsConverter,
    # The label a non-PASS row's PRESERVED record carries (CLAUDE.md rule 4; see the sweeps leg).
    # The train chain derives it from its own script name so a copied-forward assemble script cannot
    # label train 24's evidence 'train23'; a caller that forgets still preserves, under 'battery'.
    [string] $Label = 'battery'
)

$ErrorActionPreference = 'Continue'
$env:DOTNET_ROOT = '$env:USERPROFILE\dotnet10'
$env:GOROOT = '$env:USERPROFILE\sdk\go1.23.12'
$env:PATH = "$env:DOTNET_ROOT;$env:GOROOT\bin;$env:PATH"
$env:MSBUILDDISABLENODEREUSE = '1'

function Stamp([string] $m) { Write-Output ("[{0}] {1}" -f (Get-Date -Format 'yyyy-MM-dd HH:mm:ss'), $m) }
function Elapsed([datetime] $t0) { return ([int] ((Get-Date) - $t0).TotalSeconds).ToString() + 's' }

# The preserved-record naming and copy are SHARED with coord-reflect-run.ps1 -- one definition, so
# that leg's SET DIFF finds a record this leg preserved without hand-copying. An instrument that
# cannot find its own helper must be loud, never silently skip the preservation (route #6).
$SP = 'C:\Projects\go2cs\.claude\coord-scripts'
$recordKeep = Join-Path $SP 'coord-record-keep.ps1'
if (-not (Test-Path $recordKeep)) { Stamp "MISSING (existence assertion failed): $recordKeep"; Stamp "BATTERY ABORT"; exit 1 }
. $recordKeep
# Every row preserved by ONE battery run shares this stamp, so a listing groups the run's evidence.
$recordStamp = (Get-Date -Format 'yyyyMMdd-HHmmss')

$worst = 0
Stamp "UNION BATTERY START worktree=$Worktree sweeps=[$Sweeps] sweepTimeout=$SweepTimeout"
Stamp ("go version: " + ((& go version 2>&1) -join ' '))
Stamp ("dotnet --version: " + ((& dotnet --version 2>&1) -join ' '))
Stamp ("HEAD: " + ((& git -C $Worktree rev-parse HEAD 2>&1) -join ' '))
$dirty = @(& git.exe -C $Worktree status --porcelain 2>$null)
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

if (-not $SkipSuite) {
    Stamp "LEG1 START converter suite: go test -count=1 -timeout 30m ./..."
    $t0 = Get-Date
    Set-Location $goDir
    if ((Get-Location).Path -ne $goDir) { Stamp "LEG1 cd FAILED"; Stamp "BATTERY ABORT"; exit 1 }
    cmd /c "go test -count=1 -timeout 30m ./... 2>&1"
    Stamp ("LEG1 END exit=$LASTEXITCODE elapsed=" + (Elapsed $t0)); if ($LASTEXITCODE -ne 0) { $worst = 1 }
} else { Stamp "LEG1 SKIPPED by flag" }

if (-not $SkipCNR) {
    Stamp "LEG2 START CNR"
    $t0 = Get-Date
    $behDir = Join-Path $Worktree 'src\tests\Behavioral'
    Set-Location $behDir
    if ((Get-Location).Path -ne $behDir) { Stamp "LEG2 cd FAILED"; Stamp "BATTERY ABORT"; exit 1 }
    cmd /c "powershell -NoProfile -ExecutionPolicy Bypass -File $cnr 2>&1"
    Stamp ("LEG2 END exit=$LASTEXITCODE elapsed=" + (Elapsed $t0)); if ($LASTEXITCODE -ne 0) { $worst = 1 }
    $dirty = @(& git.exe -C $Worktree status --porcelain 2>$null)
    Stamp ("post-CNR git status entries: " + $dirty.Count)
    if ($dirty.Count -gt 0) { $dirty | Select-Object -First 40 | ForEach-Object { Stamp ("  dirty: " + $_) } }
} else { Stamp "LEG2 SKIPPED by flag" }

if ($Sweeps -ne '') {
    $srcDir = Join-Path $Worktree 'src'
    $n = 0
    $preserved = @()
    foreach ($pkg in ($Sweeps -split ',')) {
        $pkg = $pkg.Trim(); if ($pkg -eq '') { continue }
        $n++
        Stamp "LEG3.$n START sweep: -Filter $pkg -Exact -TestTimeout $SweepTimeout"
        $t0 = Get-Date
        Set-Location $srcDir
        if ((Get-Location).Path -ne $srcDir) { Stamp "LEG3 cd FAILED"; Stamp "BATTERY ABORT"; exit 1 }
        $skip = if ($SweepBuildsConverter) { '' } else { '-SkipBuild' }
        cmd /c "powershell -NoProfile -ExecutionPolicy Bypass -File $sweep -Filter $pkg -Exact -TestTimeout $SweepTimeout $skip 2>&1"
        # Captured BEFORE anything else runs: an exit code read after another command is that
        # command's (the pipe/`|| true` family of false readings), and this one decides whether the
        # row's evidence survives.
        $rowExit = $LASTEXITCODE
        Stamp ("LEG3.$n END exit=$rowExit elapsed=" + (Elapsed $t0)); if ($rowExit -ne 0) { $worst = 1 }

        # RULE 4 (CLAUDE.md): a gate PRESERVES a failed row's comparison record to a distinct path
        # BEFORE any restore or cleanup -- deletion is for hygiene, never for evidence. This is that
        # step, and it is HERE, inside the loop, because the restore + record delete below run once
        # after every row: by then the failed row's go2cs_test_comparison.json is gone and the only
        # way back to "which verdicts diverged" is to re-measure the row (paid on train 21, 268 s).
        # A PASS row is preserved NOT AT ALL and falls through to that delete, unchanged.
        # The predicate is the row's EXIT CODE, so FAIL, COUNT/DISC, oracle-unstable, CVAC and every
        # NOT MEASURED shape are all covered -- see Invoke-CoordRowRecordKeep for why that is wider,
        # and stricter, than reading the sweep's printed word.
        $kept = Invoke-CoordRowRecordKeep -Package $pkg -ExitCode $rowExit -SrcRoot $srcDir -Label $Label -Stamp $recordStamp -Root $SP
        if ($rowExit -ne 0) {
            if ($null -eq $kept) {
                Stamp ("LEG3.$n RECORD NOT PRESERVED: the row left no record files under " +
                    (Join-Path $srcDir ('core\' + ($pkg -replace '/', '\'))) +
                    " -- it produced none (a preflight refusal, an unbanked filter, or a build that died before the run). Nothing to preserve; nothing lost.")
            }
            else {
                $preserved += $kept.Path
                Stamp ("LEG3.$n RECORD PRESERVED (verdict not PASS, exit=$rowExit) -> " + $kept.Path)
                Stamp ("LEG3.$n   " + $kept.Files + " file(s): " + ($kept.Copied -join ', ') + " | comparison record present: " + $kept.HasComparison)
                if (-not $kept.HasComparison) {
                    Stamp ("LEG3.$n   WARNING: no go2cs_test_comparison.json among them -- the WHICH-ROWS-DIVERGED evidence is NOT in this directory; read the results tail for a deadline kill or a torn publish tree.")
                }
            }
        }
    }
    $dirty = @(& git.exe -C $Worktree status --porcelain 2>$null)
    Stamp ("post-sweep git status entries: " + $dirty.Count)
    if ($dirty.Count -gt 0) { $dirty | Select-Object -First 80 | ForEach-Object { Stamp ("  dirty: " + $_) } }
    # The evidence ledger, stated once so a reader of the log tail knows what survived the cleanup
    # below without scrolling back through the rows.
    Stamp ("preserved records this leg: " + $preserved.Count + " (non-PASS rows; PASS rows are deleted below as hygiene)")
    $preserved | ForEach-Object { Stamp ("  kept: " + $_) }
    # Sweep dirt is NEVER banked: restore the corpus and the proof pages so later legs (slnx, GolibTests) and the
    # push measure the merge result, not the -tests emission the sweep left behind (classified above for the record).
    & git.exe -C $Worktree checkout HEAD -- src/core docs/validation/current 2>$null | Out-Null
    & git.exe -C $Worktree clean -fdq -- src/core 2>$null | Out-Null
    # The pipeline's own state is git-IGNORED and survives checkout+clean: remove the record files so the next run's
    # freshness check cannot read a previous run's comparison as its own (bin/obj stay; cold builds are not the point).
    # HYGIENE ONLY -- every non-PASS row's copy was taken above, before this line. Do not move this delete earlier,
    # and do not move the preservation later: the ordering IS the rule.
    Get-ChildItem -Path (Join-Path $Worktree 'src\core') -Recurse -Force -Include 'go2cs_test_manifest.json','go2cs_test_comparison.json','go2cs_test_results.json','go2cs_test_results.xml' -ErrorAction SilentlyContinue | Remove-Item -Force -ErrorAction SilentlyContinue
    Get-ChildItem -Path (Join-Path $Worktree 'src\core') -Recurse -Force -Directory -Filter 'go2cs_test_comparison' -ErrorAction SilentlyContinue | Remove-Item -Recurse -Force -ErrorAction SilentlyContinue
    $dirty = @(& git.exe -C $Worktree status --porcelain 2>$null)
    Stamp ("after sweep-dirt restore: " + $dirty.Count + " entries")
}

Stamp "UNION BATTERY END"
Stamp ("UNION BATTERY EXIT " + $worst + " (0 only when every leg exited 0)")
exit $worst
