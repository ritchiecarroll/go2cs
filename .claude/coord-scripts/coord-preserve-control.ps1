# coord-preserve-control.ps1 -- the controls for the sweeps leg's PRESERVE step (CLAUDE.md rule 4:
# a gate preserves a failed row's comparison record to a distinct path BEFORE any restore or
# cleanup). Exercises the REAL code path in coord-record-keep.ps1 -- nothing here re-implements the
# decision, so a control cannot pass against a function that no longer matches it.
#
# Arms, and what each one would let through if it were the only one:
#   A  POSITIVE (decision)  non-PASS row + record files present  -> directory exists, non-empty,
#                           holds the comparison record, byte-identical to the source.
#   B  NEGATIVE (decision)  PASS row + the SAME record files     -> nothing preserved, no directory.
#                           The ONLY axis varied against arm A is the row's exit code.
#   C  RED CONTROL          non-PASS row + NO record files       -> arm A's assertions must FAIL
#                           (this is the arm that proves A can go red; without it A's green says
#                           only that the script ran).
#   D  NAMING               the preserved path matches the glob coord-reflect-run.ps1's SET DIFF
#                           uses, and neither caller carries its own copy of the name string.
#   E  LIVE (end to end)    coord-union-battery.ps1's sweeps leg, real sweep invocation, a row that
#                           genuinely does not pass, record files planted in the worktree: the
#                           directory is there afterwards AND the worktree copies are gone -- which
#                           is the ORDERING (preserve, then delete), the property train 21 lost.
#
# Exit 0 only when every armed arm reported as expected. -SkipLive drops arm E (the only arm that
# touches a worktree and runs another process). NEVER point -Worktree at the coordinator's worktree:
# arm E plants files in src/core and lets the leg's own hygiene delete them.
param(
    [string] $Worktree = 'C:\Projects\go2cs\.claude\worktrees\sub-q13',
    [switch] $SkipLive
)

$ErrorActionPreference = 'Continue'
$SP = 'C:\Projects\go2cs\.claude\coord-scripts'
. (Join-Path $SP 'coord-record-keep.ps1')

# A counter, never a return value: a PowerShell function that writes its progress AND returns a
# value returns the progress lines as part of that value (measured 2026-09-02). Check writes; the
# arms read the counter's delta.
$script:fails = 0
function Say([string] $m) { Write-Output ("[{0}] {1}" -f (Get-Date -Format 'HH:mm:ss'), $m) }
function Check([string] $what, [bool] $ok) {
    if ($ok) { Say ("  ok    " + $what) } else { Say ("  FAIL  " + $what); $script:fails++ }
}

# A scratch root that is NOT a worktree: arms A-D must not need one, and must not be able to
# accidentally measure the real corpus.
$root = Join-Path $env:TEMP ('coord-preserve-control-' + (Get-Date -Format 'yyyyMMdd-HHmmss'))
$keepRoot = Join-Path $root 'kept'
$srcRoot = Join-Path $root 'src'
$pkg = 'net/http'
$pkgDir = Join-Path $srcRoot 'core\net\http'
New-Item -ItemType Directory -Force -Path $pkgDir, $keepRoot | Out-Null

$recordNames = @('go2cs_test_comparison.json', 'go2cs_test_results.json', 'go2cs_test_results.xml')
$cmpText = '{"package":"net/http","status":"fail","go":{"TestQ13":"pass"},"csharp":{"TestQ13":"fail"}}'
$resText = '{"test":"","action":"timeout","output":"package timeout after 00:30:00"}'
$xmlText = '<testsuite name="q13" tests="1" failures="1" />'
function Plant([string] $dir) {
    Set-Content -LiteralPath (Join-Path $dir 'go2cs_test_comparison.json') -Value $cmpText -Encoding ascii -NoNewline
    Set-Content -LiteralPath (Join-Path $dir 'go2cs_test_results.json')    -Value $resText -Encoding ascii -NoNewline
    Set-Content -LiteralPath (Join-Path $dir 'go2cs_test_results.xml')     -Value $xmlText -Encoding ascii -NoNewline
}

Say "PRESERVE CONTROL START  scratch=$root  worktree=$Worktree"

# ---- arm A: POSITIVE -- a non-PASS row's record is preserved --------------------------------------
Say 'ARM A (positive): non-PASS row, record files present'
$fA = $script:fails
Plant $pkgDir
$keptA = Invoke-CoordRowRecordKeep -Package $pkg -ExitCode 1 -SrcRoot $srcRoot -Label 'q13ctlA' -Root $keepRoot
Check 'a record object is returned' ($null -ne $keptA)
if ($null -ne $keptA) {
    Check "the directory exists: $($keptA.Path)" (Test-Path $keptA.Path)
    $files = @(Get-ChildItem -LiteralPath $keptA.Path -Recurse -File -ErrorAction SilentlyContinue)
    Check "the directory is NON-EMPTY (3 expected, got $($files.Count))" ($files.Count -eq 3)
    Check 'the COMPARISON record is among them' ([bool] $keptA.HasComparison)
    $srcHash = (Get-FileHash -LiteralPath (Join-Path $pkgDir 'go2cs_test_comparison.json') -Algorithm SHA256).Hash
    $dstFile = Join-Path $keptA.Path 'go2cs_test_comparison.json'
    $dstHash = if (Test-Path $dstFile) { (Get-FileHash -LiteralPath $dstFile -Algorithm SHA256).Hash } else { 'ABSENT' }
    Check 'the preserved comparison is BYTE-IDENTICAL to the source' ($srcHash -eq $dstHash)
}
Say ("ARM A: " + $(if ($script:fails -eq $fA) { 'PASS' } else { 'RED' }))

# ---- arm B: NEGATIVE -- a PASS row leaves no directory --------------------------------------------
Say 'ARM B (negative): PASS row, the SAME record files present -- only the exit code differs'
$fB = $script:fails
$before = @(Get-ChildItem -LiteralPath $keepRoot -Directory -ErrorAction SilentlyContinue).Count
$keptB = Invoke-CoordRowRecordKeep -Package $pkg -ExitCode 0 -SrcRoot $srcRoot -Label 'q13ctlB' -Root $keepRoot
$after = @(Get-ChildItem -LiteralPath $keepRoot -Directory -ErrorAction SilentlyContinue).Count
Check 'nothing is returned for a PASS row' ($null -eq $keptB)
Check "no directory is created (count $before -> $after)" ($before -eq $after)
Check 'no q13ctlB-labelled directory exists' (@(Get-ChildItem -LiteralPath $keepRoot -Directory -Filter '*q13ctlB*' -ErrorAction SilentlyContinue).Count -eq 0)
Say ("ARM B: " + $(if ($script:fails -eq $fB) { 'PASS' } else { 'RED' }))

# ---- arm C: THE RED CONTROL -- arm A's assertions must fail when there is nothing to copy ---------
# Neutered by removing the SOURCE, not by a switch in the production path: a gate with a lie-lever in
# it is one more thing that can be left on.
Say "ARM C (red control): non-PASS row, NO record files -- arm A's assertions MUST fail"
$fC = $script:fails
$emptyDir = Join-Path $srcRoot 'core\empty\pkg'
New-Item -ItemType Directory -Force -Path $emptyDir | Out-Null
$keptC = Invoke-CoordRowRecordKeep -Package 'empty/pkg' -ExitCode 1 -SrcRoot $srcRoot -Label 'q13ctlC' -Root $keepRoot
$aWouldPass = ($null -ne $keptC) -and (Test-Path $keptC.Path)
Check "arm A's first assertion goes RED here (it did NOT pass)" (-not $aWouldPass)
Check 'no EMPTY directory is left behind to read as evidence' (@(Get-ChildItem -LiteralPath $keepRoot -Directory -Filter '*q13ctlC*' -ErrorAction SilentlyContinue).Count -eq 0)
Say ("ARM C: " + $(if ($script:fails -eq $fC) { 'PASS (the positive arm can go red)' } else { 'RED (the positive arm CANNOT go red -- arm A proves nothing)' }))

# ---- arm D: the NAMING is shared, in one place ----------------------------------------------------
Say "ARM D (naming): one spelling, found by the reflect leg's SET DIFF glob"
$fD = $script:fails
$p = Get-CoordRecordKeepPath -Package 'net/http' -Label 'train23' -Stamp '20260904-000000' -Root $keepRoot
$leaf = Split-Path $p -Leaf
Check "the leaf is coord-pkg-run-record-net.http-train23-20260904-000000 (got $leaf)" ($leaf -eq 'coord-pkg-run-record-net.http-train23-20260904-000000')
# The SET DIFF in coord-reflect-run.ps1 globs '<root>/coord-pkg-run-record-<pkg.dots>-*'.
Check 'it matches that glob for the package' ($leaf -like ('coord-pkg-run-record-' + ('net/http' -replace '/', '.') + '-*'))
Check "it does NOT match a sibling package's glob (net vs net.http)" (-not ($leaf -like 'coord-pkg-run-record-net-*'))
$battery = Get-Content -LiteralPath (Join-Path $SP 'coord-union-battery.ps1') -Raw
$reflect = Get-Content -LiteralPath (Join-Path $SP 'coord-reflect-run.ps1') -Raw
Check 'coord-union-battery.ps1 dot-sources coord-record-keep.ps1' ($battery -match '\.\s+\$recordKeep')
Check 'coord-reflect-run.ps1 dot-sources coord-record-keep.ps1' ($reflect -match '\.\s+\$recordKeep')
# The silent-duplication rule: a second copy of the name string would drift without a conflict.
$dupBattery = ([regex]::Matches($battery, "coord-pkg-run-record-' \+")).Count
$dupReflect = ([regex]::Matches($reflect, "coord-pkg-run-record-' \+")).Count
Check "neither caller builds the name itself (battery $dupBattery, reflect $dupReflect)" (($dupBattery + $dupReflect) -eq 0)
# The reflect leg's contract with the module: it writes pipeline-output.txt and summ.py INTO the
# directory, so it needs one even for a run that produced no record files at all. That is the ONE
# behaviour difference between the two callers, and it is a switch, not a second code path.
$noneDir = Join-Path $srcRoot 'core\nothing\here'
New-Item -ItemType Directory -Force -Path $noneDir | Out-Null
$keptAlways = Save-CoordPkgRecord -Package 'nothing/here' -OutDir $noneDir -Label 'q13ctlD' -Root $keepRoot -AlwaysCreate
Check '-AlwaysCreate gives the reflect leg a directory even with zero record files' (($null -ne $keptAlways) -and (Test-Path $keptAlways.Path) -and ($keptAlways.Files -eq 0) -and (-not $keptAlways.HasComparison))
Say ("ARM D: " + $(if ($script:fails -eq $fD) { 'PASS' } else { 'RED' }))

# ---- arm E: LIVE -- the leg itself, and the ORDERING ----------------------------------------------
if ($SkipLive) { Say 'ARM E: SKIPPED by -SkipLive' }
else {
    Say 'ARM E (live): coord-union-battery.ps1 sweeps leg, a row that does not pass, records planted in the worktree'
    $fE = $script:fails
    $liveDir = Join-Path $Worktree 'src\core\reflect'
    if (-not (Test-Path $liveDir)) { Check "the worktree package dir exists: $liveDir" $false }
    else {
        Plant $liveDir
        Check 'the records are planted in the worktree' ((@($recordNames | Where-Object { Test-Path (Join-Path $liveDir $_) })).Count -eq 3)
        $log = Join-Path $root 'arm-e-battery.log'
        # 'reflect' is UNBANKED, so the sweep's roster filter matches nothing and the row cannot pass:
        # a NOT MEASURED row, which is exactly one of the shapes rule 4 names, reached in seconds
        # instead of the hour a genuinely failing banked row costs.
        cmd /c "powershell -NoProfile -ExecutionPolicy Bypass -File `"$SP\coord-union-battery.ps1`" -Worktree `"$Worktree`" -SkipSuite -SkipCNR -Sweeps reflect -Label q13ctlE > `"$log`" 2>&1"
        $text = if (Test-Path $log) { Get-Content -LiteralPath $log -Raw } else { '' }
        Check 'the leg ran and logged (log is non-empty)' ($text.Length -gt 0)
        Check 'the row did NOT pass (leg logged a non-zero row exit)' ($text -match 'LEG3\.1 END exit=[1-9]')
        Check 'the leg printed RECORD PRESERVED with a path' ($text -match 'RECORD PRESERVED \(verdict not PASS')
        $dir = @(Get-ChildItem -LiteralPath $SP -Directory -Filter 'coord-pkg-run-record-reflect-q13ctlE-*' -ErrorAction SilentlyContinue)
        Check "exactly one preserved directory exists (got $($dir.Count))" ($dir.Count -eq 1)
        if ($dir.Count -eq 1) {
            $kf = @(Get-ChildItem -LiteralPath $dir[0].FullName -File)
            Check "it is NON-EMPTY (3 expected, got $($kf.Count)): $($kf.Name -join ', ')" ($kf.Count -eq 3)
            Check 'it holds the COMPARISON record' (Test-Path (Join-Path $dir[0].FullName 'go2cs_test_comparison.json'))
            Check 'the preserved comparison is the planted content' ((Get-Content -LiteralPath (Join-Path $dir[0].FullName 'go2cs_test_comparison.json') -Raw) -eq $cmpText)
        }
        # THE ORDERING, which is the whole point: the leg's hygiene delete ran, and the copy survived it.
        $left = @($recordNames | Where-Object { Test-Path (Join-Path $liveDir $_) })
        Check "the worktree copies are GONE (hygiene ran AFTER the preserve; $($left.Count) left)" ($left.Count -eq 0)
        Check 'the worktree is clean after the leg' ((@(& git.exe -C $Worktree status --porcelain 2>$null)).Count -eq 0)
        Say ("  battery log: $log")
        # CLEAN UP THE ARM'S OWN ARTIFACT. coord-reflect-run.ps1's SET DIFF takes the NEWEST
        # coord-pkg-run-record-reflect-* directory as "the previous run", so a control record left
        # here would become the next train's reflect baseline and its synthetic one-row comparison
        # would print a whole-suite FIXED/BROKEN set. The assertions above are the evidence; the
        # directory is not kept.
        $dir | ForEach-Object { Remove-Item -LiteralPath $_.FullName -Recurse -Force -ErrorAction SilentlyContinue }
        Check 'the control removed its own reflect record (it must not become the reflect SET DIFF baseline)' (@(Get-ChildItem -LiteralPath $SP -Directory -Filter 'coord-pkg-run-record-reflect-q13ctlE-*' -ErrorAction SilentlyContinue).Count -eq 0)
    }
    Say ("ARM E: " + $(if ($script:fails -eq $fE) { 'PASS' } else { 'RED' }))
}

Say ("PRESERVE CONTROL END  failed assertions: $script:fails")
if ($script:fails -eq 0) { Say 'CONTROL RESULT: PASS (all armed arms as expected)'; exit 0 }
Say 'CONTROL RESULT: RED'
exit 1
