# p2-driver.ps1 -- G-LAPTOP's S-G shard of the P2 full validated-roster sweep (Windows PowerShell 5.1).
# Brief: claude/coord-handover f223c19182, docs/phase4/briefs/full-roster-sweep-brief.md (COORD rulings SW-1..SW-19).
# ASCII-only on purpose: a BOM-less 5.1 script decodes as ANSI, so U+00B7 is spelled [char]0x00B7.
param(
    [switch] $SelfTest,
    [switch] $AppendNetOs   # SW-3 amendment: os then net after the 126, net behind Q5's pre-row hook
)

$ErrorActionPreference = 'Continue'

# ---- fills ------------------------------------------------------------------------------------
$HostNick  = 'G-LAPTOP'
$Wt        = 'C:\p2g'
$Ev        = 'C:\h10\p2\G-LAPTOP'
$SweepCopy = 'C:\p2g\src\run-validated-sweep.p2-G-LAPTOP.ps1'
$Base      = '61724860b49a84c373f1c0c6f76d8df3cbad9804'
$Tracked0  = 15394
$GoRootWin = '<goroot>'
$DotnetWin = '<dotnet10>'
$ConverterExe = 'C:\p2g\src\go2cs\bin\go2cs.exe'
$Shard     = 'S-G'
$AskMinutes = 10
# sw :937 at the sweep base, asserted in S0 (12 entries).
$LongTimeouts = @{ 'hash/maphash' = 60; 'index/suffixarray' = 120; 'crypto/dsa' = 120; 'archive/zip' = 60; 'go/parser' = 90;
    'crypto/internal/fips140/mlkem' = 30; 'crypto/mlkem' = 30; 'time' = 40; 'crypto/tls' = 60; 'sync/atomic' = 150; 'net' = 120; 'net/http' = 60 }
$TerminalWords = @('PASS','COUNT','DISC','CVAC','FAIL','ORACLE','N/A','NOT MEASURED','TIMEOUT')
$RefusalMarkers = @('DISK PREFLIGHT','Toolchain pin','No banked packages matched','Converter not built','converter build failed')
$ToleratedNetFails = @('TestLookupCNAME')

$Ledger = Join-Path $Ev 'ledger.tsv'
$LedgerHeader = "package`tshard`thost`tkind`tattempt`tstart`tend`touter_s`tsweep_s`tdeadline_min`tword`tgot`tgotDisclosed`tgotDisclosedArray`tbankedTests`tbankedDisclosed`tabsorption`tpage`tdrift`tlog`tcorpus_commit`tconverter_commit`tconverter_sha256`tconverter_mtime`tctoolchain`trc"

function Log([string] $m) { [Console]::Out.WriteLine(("{0} {1}" -f (Get-Date -Format "s"), $m)); [Console]::Out.Flush() }
function Utf8NoBom { New-Object System.Text.UTF8Encoding($false) }
function Append-Lf([string] $path, [string] $line) { [System.IO.File]::AppendAllText($path, $line + "`n", (Utf8NoBom)) }
function Read-Text([string] $path) { if (Test-Path -LiteralPath $path) { [System.IO.File]::ReadAllText($path) } else { '' } }
function Row-Dir([string] $row) { $row -replace '/', '__' }

# ---- pins, inside the process -------------------------------------------------------------------
function Set-Pins {
    $env:GOROOT = $GoRootWin
    $env:PATH = "$GoRootWin\bin;$DotnetWin;" + $env:PATH
    $env:GOTOOLCHAIN = 'local'; $env:CGO_ENABLED = '0'; $env:DOTNET_ROOT = $DotnetWin
    foreach ($n in 'GOFLAGS','GoTargetOS','NUGET_API_KEY','NuGetCertFingerprint') { Remove-Item "Env:$n" -ErrorAction SilentlyContinue }
}
function Assert-Pins {
    $gv = (& go version 2>&1 | Out-String).Trim()
    $gr = (& go env GOROOT 2>&1 | Out-String).Trim()
    $gf = (& go env GOFLAGS 2>&1 | Out-String).Trim()
    $dv = (& dotnet --version 2>&1 | Out-String).Trim()
    $bad = @()
    if ($gv -notmatch 'go1\.24\.13 windows/amd64') { $bad += "go version '$gv'" }
    if ($gr -ne $GoRootWin) { $bad += 'go env GOROOT mismatch' }
    if ($gf) { $bad += 'go env GOFLAGS non-empty (env file)' }
    if ($dv -notmatch '^10\.') { $bad += "dotnet --version '$dv'" }
    foreach ($n in 'NUGET_API_KEY','NuGetCertFingerprint','GoTargetOS') { if (Test-Path "Env:$n") { $bad += "$n present" } }
    $amb = @(Get-ChildItem Env: | Where-Object { $_.Name -match '^(DOTNET_Tiered|DOTNET_TC_|COMPlus_)' } | ForEach-Object Name)
    if ($amb.Count) { $bad += "ambient names: $($amb -join ',')" }
    Log ("PINS go='{0}' GOROOT={1} GOFLAGS-empty={2} dotnet={3} creds-absent={4}" -f $gv, ($gr -eq $GoRootWin), (-not $gf), $dv, (-not (Test-Path Env:NUGET_API_KEY) -and -not (Test-Path Env:NuGetCertFingerprint)))
    return $bad
}

# ---- verdict parsing (the SAME functions the live path calls) ------------------------------------
function Get-Verdict([string] $logText, [string] $errText, [string] $row, [string] $execution) {
    $all = ($logText + "`n" + $errText) -split "`r?`n"
    $esc = [regex]::Escape($row)
    $vre = '^  (PASS|COUNT|DISC|CVAC|FAIL|ORACLE|N/A) +(\S+) '
    $lines = @($all | Where-Object { $_ -match $vre -and $Matches[2] -eq $row })
    $rerun = @($all | Where-Object { $_ -match ('^  RERUN +' + $esc + ' ') }).Count
    $flaked = @($all | Where-Object { $_ -match '^        ORACLE FLAKED ONCE' }).Count
    $refusals = @($RefusalMarkers | Where-Object { $all -match [regex]::Escape($_) })
    $summary = @($all | Where-Object { $_ -match '^sweep: ' }) | Select-Object -Last 1
    $r = [ordered]@{ Word = $null; Got = $null; Wall = $null; Line = $null; Rerun = $rerun; Flaked = $flaked; Refusals = $refusals; Summary = $summary; Problem = $null; Absorption = 'none'; Delta = 0 }
    if ($lines.Count -eq 0) { $r.Problem = 'NO VERDICT LINE'; return $r }
    if ($lines.Count -gt 1) { $r.Problem = 'DOUBLED verdict line'; return $r }
    $l = $lines[0]; $r.Line = $l
    $null = $l -match $vre; $r.Word = $Matches[1]
    if ($l -match '\[(\d+)s\]\s*$') { $r.Wall = [int]$Matches[1] }
    $after = $l.Substring(8).Trim()
    $after = $after.Substring($row.Length).Trim()
    if ($after -match '^(\d+)') { $r.Got = [int]$Matches[1] }
    if ($rerun -gt 1) { $r.Problem = "RERUN x$rerun" }
    elseif ($rerun -eq 1 -and $r.Word -ne 'ORACLE' -and $flaked -ne 1) { $r.Problem = 'RERUN without ORACLE or FLAKED ONCE' }
    if ($r.Word -eq 'PASS') {
        if ($l -match 'banked \+ (\d+) host-conditional') { $r.Absorption = 'host-conditional'; $r.Delta = [int]$Matches[1] }
        elseif ($l -match 'banked - (\d+) host-conditional disclosure') { $r.Absorption = 'host-conditional-disclosure'; $r.Delta = [int]$Matches[1] }
        elseif ($l -match 'banked - (\d+) \((\S+) capability absent\)') { $r.Absorption = 'capability-absent'; $r.Delta = [int]$Matches[1] }
        elseif ($l -match 'banked - (\d+) \((\S+) host-limit disclosed') { $r.Absorption = 'host-limit'; $r.Delta = [int]$Matches[1] }
        elseif ($flaked -eq 1) { $r.Absorption = 'oracle-reran' }
        $hasSuffix = $l -match ' \[release-tiered\] \[\d+s\]\s*$'
        if ($r.Absorption -eq 'none' -or $r.Absorption -eq 'oracle-reran') {
            if ($execution -and -not $hasSuffix) { $r.Problem = "execution pin '$execution' missing its suffix" }
            if (-not $execution -and $hasSuffix) { $r.Problem = 'unexpected execution suffix' }
        }
    }
    return $r
}

function Test-Doubled([string] $t) { }

# ---- page reading (the sweep's own reader shapes, sw :583-584) ------------------------------------
function Read-Page([string] $text) {
    $rows = [ordered]@{}; $inV = $false; $n = $null; $d = $null
    foreach ($line in ($text -split "`r?`n")) {
        if ($line -match '^##\s+Verdicts\b') { $inV = $true; continue }
        if ($inV -and $line -match '^##\s') { $inV = $false }
        if ($inV -and $line -match '^\|\s*`([^`]+)`\s*\|(.*)$') { $rows[$Matches[1]] = ($Matches[2] -replace '\s+', ' ').Trim() }
        if ($null -eq $n -and $line -match '\*\*(\d+) matched') { $n = [int]$Matches[1]; if ($line -match '(\d+) disclosed') { $d = [int]$Matches[1] } }
    }
    return [ordered]@{ Rows = $rows; N = $n; D = $d }
}
function Compare-Page([string] $headText, [string] $newPath, [datetime] $rowStart) {
    if (-not (Test-Path -LiteralPath $newPath)) { return [ordered]@{ Reading = 'ABSENT'; Moved = @(); N = $null; D = $null } }
    if ((Get-Item -LiteralPath $newPath).LastWriteTime -lt $rowStart) { return [ordered]@{ Reading = 'NOT REWRITTEN'; Moved = @(); N = $null; D = $null } }
    $h = Read-Page $headText; $w = Read-Page (Read-Text $newPath)
    $moved = @()
    foreach ($k in $h.Rows.Keys) { if (-not $w.Rows.Contains($k) -or $w.Rows[$k] -ne $h.Rows[$k]) { $moved += $k } }
    foreach ($k in $w.Rows.Keys) { if (-not $h.Rows.Contains($k)) { $moved += $k } }
    $countsMoved = ($h.N -ne $w.N) -or ($h.D -ne $w.D)
    $reading = if ($moved.Count -eq 0 -and -not $countsMoved) { 'EQUAL' } else { 'MOVED' }
    return [ordered]@{ Reading = $reading; Moved = $moved; N = $w.N; D = $w.D; HeadN = $h.N; HeadD = $h.D; CountsMoved = $countsMoved }
}

# ---- Q5: the net qualifier criterion (sourced by the hook and the self-test alike) ----------------
function Test-NetQualifier([string] $logText, [int] $rc) {
    $lines = $logText -split "`r?`n"
    $tailOk = (($rc -eq 0) -and ($lines -match "^ok\s+net\s")) -or (($rc -eq 1) -and ($lines -match "^FAIL\s+net\s"))
    $panic = @($lines | Where-Object { $_ -match 'panic:|test timed out' })
    $fails = @($lines | Where-Object { $_ -match '^\s*--- FAIL: (\S+)' } | ForEach-Object { $null = $_ -match '^\s*--- FAIL: (\S+)'; $Matches[1] } | Sort-Object -Unique)
    $extra = @($fails | Where-Object { $ToleratedNetFails -notcontains $_ })
    $ok = $tailOk -and ($panic.Count -eq 0) -and ($extra.Count -eq 0)
    $absent = @($ToleratedNetFails | Where-Object { $fails -notcontains $_ })
    return [ordered]@{ Pass = [bool]$ok; FailingSet = $fails; Extra = $extra; TailOk = [bool]$tailOk; Panic = $panic.Count; AbsentTolerated = $absent }
}

# ---- process census (image path / command line; never killed) ------------------------------------
function Get-StrayProcesses {
    $self = $PID
    $parents = @(); $p = Get-CimInstance Win32_Process -Filter "ProcessId=$self"
    while ($p -and $p.ParentProcessId -and $parents.Count -lt 8) { $parents += $p.ParentProcessId; $p = Get-CimInstance Win32_Process -Filter "ProcessId=$($p.ParentProcessId)" }
    @(Get-CimInstance Win32_Process | Where-Object {
        $_.ProcessId -ne $self -and $parents -notcontains $_.ProcessId -and (
            ($_.ExecutablePath -and $_.ExecutablePath.StartsWith($Wt, [StringComparison]::OrdinalIgnoreCase)) -or
            ($_.CommandLine -and $_.CommandLine -match 'run-validated-sweep\.p2-G-LAPTOP\.ps1')) })
}
function Get-Go2csCount { @(Get-CimInstance Win32_Process -Filter "Name='go2cs.exe'").Count }

# ---- git helpers ---------------------------------------------------------------------------------
function Invoke-WtGit([string[]] $a) { & git.exe -C $Wt @a 2>&1 }
function Get-TrackedCount {
    $z = Join-Path $Ev 'lsN.z'
    & cmd /c "git.exe -C $Wt ls-files -z > `"$z`""
    if ($LASTEXITCODE -ne 0) { return -1 }
    $b = [System.IO.File]::ReadAllBytes($z); $c = 0; foreach ($x in $b) { if ($x -eq 0) { $c++ } }; return $c
}
function Assert-Restored {
    $por = @(Invoke-WtGit @('status','--porcelain')) | Where-Object { $_ -and $_ -ne '?? src/run-validated-sweep.p2-G-LAPTOP.ps1' }
    $del = @($por | Where-Object { $_ -match '^ D' })
    $tc = Get-TrackedCount
    return [ordered]@{ Ok = ($por.Count -eq 0 -and $tc -eq $Tracked0); Porcelain = $por; Deleted = $del.Count; Tracked = $tc }
}
function Restore-Roots {
    $null = & git.exe -C $Wt checkout -- src/core docs/validation 2>&1
    if ($LASTEXITCODE -ne 0) { return "checkout rc=$LASTEXITCODE" }
    $uz = Join-Path $Ev 'untracked.z'
    & cmd /c "git.exe -C $Wt ls-files --others --exclude-standard -z -- src/core docs/validation > `"$uz`""
    if ($LASTEXITCODE -ne 0) { return "ls-files others rc=$LASTEXITCODE" }
    $names = ([System.IO.File]::ReadAllText($uz)) -split "`0" | Where-Object { $_ }
    foreach ($n in $names) { $p = Join-Path $Wt ($n -replace '/', '\'); Remove-Item -LiteralPath $p -Force -Recurse -ErrorAction SilentlyContinue }
    return $null
}
function Invoke-Purge {
    $pl = Join-Path $Ev ("purge-{0}.log" -f (Get-Date -Format 'yyyyMMdd-HHmmss'))
    $p = Start-Process powershell -ArgumentList '-NoProfile','-ExecutionPolicy','Bypass','-File',"$Wt\src\clean-bin.ps1",'-Root',"$Wt\src\core",'-Force' -NoNewWindow -PassThru -RedirectStandardOutput $pl -RedirectStandardError "$pl.err"
    $null = $p.Handle; $null = $p.WaitForExit(); $rc = $p.ExitCode
    $exeOk = Test-Path -LiteralPath $ConverterExe
    $size = 0; Get-ChildItem -LiteralPath "$Wt\src\core" -Recurse -Directory -Force -ErrorAction SilentlyContinue | Where-Object { $_.Name -in 'bin','obj','Generated' } | ForEach-Object { $size += (Get-ChildItem -LiteralPath $_.FullName -Recurse -File -Force -ErrorAction SilentlyContinue | Measure-Object Length -Sum).Sum }
    Log ("PURGE rc={0} converter-present={1} bin/obj/Generated={2:N1} MB log={3}" -f $rc, $exeOk, ($size/1MB), (Split-Path $pl -Leaf))
    return ($rc -eq 0 -and $exeOk)
}
function Free-GB { [math]::Round((Get-PSDrive C).Free / 1GB, 1) }

# ---- drift (git's numstat with the sweep's own CR discriminator) ---------------------------------
function Get-Drift([string] $numstatPath) {
    $lines = @(Get-Content -LiteralPath $numstatPath -ErrorAction SilentlyContinue | Where-Object { $_ })
    if ($lines.Count -eq 0) { return 'clean' }
    $auto = 0; $test = 0; $other = @()
    foreach ($l in $lines) {
        $path = ($l -split "`t")[2]
        if ($path -like '*.cs.auto') { $auto++ }
        elseif ($path -match '(_test\.cs|/package_test_info\.cs|/go2cs_test_host\.cs|\.tests\.csproj)$') { $test++ }
        else { $other += $path }
    }
    return ("auto={0} test-artifacts={1} production={2}" -f $auto, $test, $other.Count)
}

# ---- the per-row budget ---------------------------------------------------------------------------
function Get-PkgTimeoutMin([string] $row) { if ($LongTimeouts.ContainsKey($row)) { [math]::Max($AskMinutes, $LongTimeouts[$row]) } else { $AskMinutes } }

# ---- ledger ---------------------------------------------------------------------------------------
function Read-LedgerWords {
    $w = @{}; if (-not (Test-Path -LiteralPath $Ledger)) { return $w }
    foreach ($l in (Get-Content -LiteralPath $Ledger | Select-Object -Skip 1)) { $f = $l -split "`t"; if ($f.Count -gt 10 -and $f[3] -eq 'verdict') { $w[$f[0]] = @($w[$f[0]]) + $f[10] } }
    return $w
}

# ==================================================================================================
function Invoke-SelfTest {
    $fails = @(); $n = 0
    function Check([string] $name, [bool] $cond) { $script:stN++; if ($cond) { Log "  ok   $name" } else { Log "  FAIL $name"; $script:stFails += $name } }
    $script:stN = 0; $script:stFails = @()
    $pad = { param($r) '{0,-34}' -f $r }
    $row = 'archive/tar'; $L = & $pad $row
    $v = Get-Verdict "  PASS  $L 98 [99s]`nsweep: 1 pass / 0 fail  (100s)" '' $row ''; Check 'plain PASS' ($v.Word -eq 'PASS' -and $v.Got -eq 98 -and $v.Wall -eq 99 -and -not $v.Problem -and $v.Absorption -eq 'none')
    $r2 = 'internal/godebug'; $L2 = & $pad $r2
    $v = Get-Verdict "  PASS  $L2 5 [release-tiered] [120s]" '' $r2 'release-tiered'; Check 'PASS [release-tiered]' ($v.Word -eq 'PASS' -and $v.Got -eq 5 -and -not $v.Problem)
    $v = Get-Verdict "  PASS  $L2 5 [120s]" '' $r2 'release-tiered'; Check 'pinned row WITHOUT suffix reds' ([bool]$v.Problem)
    $r3 = 'path/filepath'; $L3 = & $pad $r3
    $v = Get-Verdict "  PASS  $L3 67 = 61 banked + 6 host-conditional [40s]" '' $r3 ''; Check 'host-conditional PASS' ($v.Absorption -eq 'host-conditional' -and $v.Delta -eq 6 -and $v.Got -eq 67)
    $r4 = 'os/exec'; $L4 = & $pad $r4
    $v = Get-Verdict "  PASS  $L4 117 = 116 banked - 1 host-conditional disclosure (TestExtraFiles fired on this host) [200s]" '' $r4 ''; Check 'host-conditional-disclosure PASS' ($v.Absorption -eq 'host-conditional-disclosure' -and $v.Delta -eq 1)
    $r5 = 'crypto/tls'; $L5 = & $pad $r5
    $v = Get-Verdict "  PASS  $L5 1340 = 4759 banked - 3419 (TestBogoSuite capability absent) [700s]" '' $r5 ''; Check 'capability-absent PASS' ($v.Absorption -eq 'capability-absent' -and $v.Delta -eq 3419)
    $v = Get-Verdict "  PASS  $L5 1340 = 4759 banked - 3419 (TestBogoSuite host-limit disclosed; capability PRESENT, converted side over the deadline) [885s]" '' $r5 ''; Check 'host-limit PASS' ($v.Absorption -eq 'host-limit' -and $v.Delta -eq 3419)
    $v = Get-Verdict "  COUNT $L 97, banked 98 [99s]" '' $row ''; Check 'COUNT' ($v.Word -eq 'COUNT' -and $v.Got -eq 97)
    $v = Get-Verdict "  DISC  $L 98, disclosed 1 vs the columns expectation 0 [99s]" '' $row ''; Check 'DISC' ($v.Word -eq 'DISC' -and $v.Got -eq 98)
    $v = Get-Verdict "  CVAC  $L 98 (validated; no windows expectation, windows column 97) [99s]" '' $row ''; Check 'CVAC' ($v.Word -eq 'CVAC')
    $v = Get-Verdict "  FAIL  $L [99s]`n        some tail" '' $row ''; Check 'FAIL' ($v.Word -eq 'FAIL' -and -not $v.Problem)
    $v = Get-Verdict "  RERUN $L oracle-only failure set: 1 case(s)`n  ORACLE $L oracle unstable on this host -- two oracle-only runs [50s]" '' $row ''; Check 'RERUN then ORACLE' ($v.Word -eq 'ORACLE' -and $v.Rerun -eq 1 -and -not $v.Problem)
    $v = Get-Verdict "  RERUN $L oracle-only failure set: 1 case(s)`n  PASS  $L 98 [99s]`n        ORACLE FLAKED ONCE -- run 1 diverged" '' $row ''; Check 'RERUN, PASS, FLAKED ONCE (named discharge)' ($v.Word -eq 'PASS' -and $v.Absorption -eq 'oracle-reran' -and -not $v.Problem)
    $v = Get-Verdict "  RERUN $L oracle-only failure set: 1 case(s)`n  PASS  $L 98 [99s]" '' $row ''; Check 'RERUN then bare PASS reds' ([bool]$v.Problem)
    $v = Get-Verdict "PASS  $L 98 [99s]" '' $row ''; Check 'column-0 look-alike is no verdict' ($v.Problem -eq 'NO VERDICT LINE')
    $v = Get-Verdict "        PASS  $L 98 [99s]" '' $row ''; Check '8-space tail line is no verdict' ($v.Problem -eq 'NO VERDICT LINE')
    $v = Get-Verdict "no verdict at all" '' $row ''; Check 'no-verdict log' ($v.Problem -eq 'NO VERDICT LINE')
    $v = Get-Verdict "  PASS  $L 98 [99s]`n  FAIL  $L [99s]" '' $row ''; Check 'two final verdict lines red DOUBLED' ($v.Problem -eq 'DOUBLED verdict line')
    foreach ($m in $RefusalMarkers) { $v = Get-Verdict '' "x`n$m here`n" $row ''; Check "refusal marker in .err: $m" ($v.Refusals -contains $m); $v = Get-Verdict "x`n$m here" '' $row ''; Check "refusal marker in .log: $m" ($v.Refusals -contains $m) }
    $pr = Read-Text (Join-Path $Ev 'plant-refusal.log'); $v = Get-Verdict $pr '' 'net/http' ''; Check 'S0.4 plant-refusal.log names its marker' ($v.Refusals -contains 'No banked packages matched')
    # page reader
    $tmp = Join-Path $Ev 'selftest'; New-Item -ItemType Directory -Force $tmp | Out-Null
    $dot = [char]0x00B7
    $head = "*Validated 2026-09-22 $dot converter ``abc``*`n`n**3 matched $dot 1 disclosed** -- x`n`n## Verdicts`n`n| Test | go | cs |`n|:--|:--:|:--:|`n| ``TestA`` | pass | pass |`n| ``TestB`` | pass | pass |`n| ``TestC`` | pass | fail |`n"
    $p1 = Join-Path $tmp 'eq.md'; [IO.File]::WriteAllText($p1, ($head -replace '2026-09-22', '2026-09-24' -replace 'abc', 'def'), (Utf8NoBom))
    $c = Compare-Page $head $p1 ((Get-Date).AddMinutes(-5)); Check 'page EQUAL (volatile date/converter lines ignored)' ($c.Reading -eq 'EQUAL' -and $c.N -eq 3 -and $c.D -eq 1)
    $bt = [string][char]96
    $p2 = Join-Path $tmp 'mv.md'; [IO.File]::WriteAllText($p2, $head.Replace('| ' + $bt + 'TestB' + $bt + ' | pass | pass |', '| ' + $bt + 'TestB' + $bt + ' | pass | fail |'), (Utf8NoBom))
    $c = Compare-Page $head $p2 ((Get-Date).AddMinutes(-5)); Check 'page MOVED names TestB' ($c.Reading -eq 'MOVED' -and $c.Moved -contains 'TestB' -and $c.Moved.Count -eq 1)
    $p3 = Join-Path $tmp 'nr.md'; [IO.File]::WriteAllText($p3, $head, (Utf8NoBom)); (Get-Item $p3).LastWriteTime = (Get-Date).AddHours(-2)
    $c = Compare-Page $head $p3 ((Get-Date).AddMinutes(-5)); Check 'page NOT REWRITTEN (mtime before row start)' ($c.Reading -eq 'NOT REWRITTEN')
    # Q5 criterion arms
    $okLog = "=== RUN TestX`n--- FAIL: TestLookupCNAME (0.2s)`nFAIL`nFAIL`tnet`t300.1s"
    $q = Test-NetQualifier $okLog 1; Check 'Q5 tolerated set alone passes' $q.Pass
    $q = Test-NetQualifier ($okLog -replace 'FAIL`n', "--- FAIL: TestPlant (0.1s)`nFAIL`n") 1; $q = Test-NetQualifier ("--- FAIL: TestLookupCNAME (0.2s)`n    --- FAIL: TestPlant (0.1s)`nFAIL`tnet`t1s") 1; Check 'Q5 tolerated + TestPlant aborts naming TestPlant' (-not $q.Pass -and $q.Extra -contains 'TestPlant')
    $q = Test-NetQualifier "--- FAIL: TestPlant (0.1s)`nFAIL`tnet`t1s" 1; Check 'Q5 TestPlant alone aborts' (-not $q.Pass -and $q.Extra -contains 'TestPlant')
    $q = Test-NetQualifier "PASS`nok  `tnet`t300s" 0; Check 'Q5 empty set passes and prints the absent tolerated leaf' ($q.Pass -and $q.AbsentTolerated -contains 'TestLookupCNAME')
    $q = Test-NetQualifier "--- FAIL: TestLookupCNAME (0.2s)`npanic: test timed out after 40m0s`nFAIL`tnet`t2400s" 1; Check 'Q5 a test-timed-out panic reds' (-not $q.Pass -and $q.Panic -ge 1)
    # NUL-count tell over a log produced by the driver's own Start-Process redirection
    $nl = Join-Path $tmp 'redir-tell.log'; $p = Start-Process powershell -ArgumentList '-NoProfile','-Command','Write-Output hello' -NoNewWindow -PassThru -RedirectStandardOutput $nl -RedirectStandardError "$nl.err"; $null = $p.Handle; $null = $p.WaitForExit()
    $nuls = 0; foreach ($x in [IO.File]::ReadAllBytes($nl)) { if ($x -eq 0) { $nuls++ } }; $nt = Read-Text $nl; Log "  (nul tell: nuls=$nuls text=[$($nt.Trim())] rc=$($p.ExitCode) err=[$((Read-Text "$nl.err").Trim())] bytes=$((Get-Item $nl).Length))"; Check 'NUL-count tell over a Start-Process-redirected log reads 0' (($nuls -eq 0) -and ($nt -match 'hello'))
    # the live git helpers (the self-test once missed a function-name recursion here)
    $hd = (& git.exe -C $Wt rev-parse HEAD 2>&1 | Out-String).Trim(); Check 'git.exe HEAD == base' ($hd -eq $Base)
    Check 'Get-TrackedCount == S0 count' ((Get-TrackedCount) -eq $Tracked0)
    $ar = Assert-Restored; Check 'Assert-Restored on the clean tree' ($ar.Ok -and $ar.Deleted -eq 0)
    Check 'no function shadows git' (-not (Get-Command git -CommandType Function -ErrorAction SilentlyContinue))
    # budget arithmetic
    Check 'budget archive/tar = 4x10+30 = 70 min' ((4 * (Get-PkgTimeoutMin 'archive/tar') + 30) -eq 70)
    Check 'budget index/suffixarray = 4x120+30 = 510 min' ((4 * (Get-PkgTimeoutMin 'index/suffixarray') + 30) -eq 510)
    Log ("SELFTEST {0} checks, {1} failed{2}" -f $script:stN, $script:stFails.Count, $(if ($script:stFails.Count) { ': ' + ($script:stFails -join '; ') } else { '' }))
    return $script:stFails.Count
}

# ==================================================================================================
$exit = 1
try {
    Set-Location -LiteralPath $Ev
    Set-Pins
    Log "p2-driver start: shard $Shard host $HostNick base $Base pid $PID selftest=$SelfTest appendNetOs=$AppendNetOs"
    . "$Wt\src\_roster.ps1"
    if ($SelfTest) { $f = Invoke-SelfTest; $exit = if ($f -eq 0) { 0 } else { 1 }; return }

    $bad = Assert-Pins
    if ($bad.Count) { Log "RUN-LEVEL STOP: pins: $($bad -join '; ')"; $exit = 2; return }
    $os = Get-CimInstance Win32_OperatingSystem
    Log ("uptime {0:N2} h (last boot {1})" -f ((Get-Date) - $os.LastBootUpTime).TotalHours, $os.LastBootUpTime)
    $head = (& git.exe -C $Wt rev-parse HEAD 2>&1 | Out-String).Trim(); if ($head -ne $Base) { Log "RUN-LEVEL STOP: HEAD $head != base"; $exit = 2; return }
    $convSha = (Get-FileHash -LiteralPath $ConverterExe -Algorithm SHA256).Hash.ToLower(); $convMtime = (Get-Item -LiteralPath $ConverterExe).LastWriteTime.ToString('s')

    # ---- the list, derived by the SHARD PLAN's rule ----
    $rosterRows = @(Get-ValidatedRosterRows -Path "$Wt\docs\ValidatedTestPackages.md")
    if ($rosterRows.Count -ne 218) { Log "RUN-LEVEL STOP: roster rows $($rosterRows.Count) != 218"; $exit = 2; return }
    $list = @($rosterRows[0..126] | Where-Object { $_.Package -ne 'crypto/tls' })
    if ($list.Count -ne 126 -or $list[0].Package -ne 'archive/tar' -or $list[-1].Package -ne 'internal/godebugs' -or $rosterRows[127].Package -ne 'internal/gover') { Log "RUN-LEVEL STOP: list shape"; $exit = 2; return }
    if ($AppendNetOs) { $list += @($rosterRows | Where-Object { $_.Package -eq 'os' }); $list += @($rosterRows | Where-Object { $_.Package -eq 'net' }) }
    Log ("LIST {0} rows: first {1}, last {2}" -f $list.Count, $list[0].Package, $list[-1].Package)
    $i = 0; foreach ($r in $list) { $i++; Log ("  list {0,3} {1}" -f $i, $r.Package) }

    # ---- START GATE ----
    if (-not (Test-Path -LiteralPath $Ledger)) { [IO.File]::WriteAllText($Ledger, $LedgerHeader + "`n", (Utf8NoBom)) }
    $inflight = @(Get-ChildItem -LiteralPath (Join-Path $Ev 'rows') -Recurse -Filter INFLIGHT -File -ErrorAction SilentlyContinue)
    $dirty = @(Invoke-WtGit @('status','--porcelain')) | Where-Object { $_ -and $_ -ne '?? src/run-validated-sweep.p2-G-LAPTOP.ps1' }
    if ($inflight.Count -or $dirty.Count) {
        Log ("START GATE: resume after an interrupted run (inflight={0}, dirty={1}); classify per rb 3.6: uptime above; sibling bare-name kill / harness reap / build-server shutdown / reboot -- read uptime vs the INFLIGHT start" -f $inflight.Count, $dirty.Count)
        foreach ($f in $inflight) {
            $rd = $f.DirectoryName; $rowName = (Split-Path $rd -Leaf) -replace '__', '/'
            $k = 1; while (Test-Path -LiteralPath (Join-Path $rd "aborted-$k")) { $k++ }
            $dest = Join-Path $rd "aborted-$k"; New-Item -ItemType Directory -Force $dest | Out-Null
            Get-ChildItem -LiteralPath $rd -File | Move-Item -Destination $dest
            Append-Lf $Ledger (@($rowName,$Shard,$HostNick,'verdict','-',(Get-Content -LiteralPath (Join-Path $dest 'INFLIGHT') -ErrorAction SilentlyContinue),(Get-Date -Format 's'),'','','','ABORTED','','','','','','none','','','',$Base,$Base,$convSha,$convMtime,'none','') -join "`t")
            Log "START GATE: $rowName ABORTED (records -> aborted-$k)"
        }
        $e = Restore-Roots; if ($e) { Log "RUN-LEVEL STOP: restore: $e"; $exit = 2; return }
        if (-not (Invoke-Purge)) { Log 'RUN-LEVEL STOP: start-gate purge'; $exit = 2; return }
        $a = Assert-Restored; if (-not $a.Ok -or $a.Deleted) { Log "RUN-LEVEL STOP: start-gate assert: porcelain=$($a.Porcelain -join ' | ') tracked=$($a.Tracked)"; $exit = 2; return }
        if ((Get-Go2csCount) -ne 0) { Log 'RUN-LEVEL STOP: a go2cs.exe survived the dead driver'; $exit = 2; return }
    }
    $words = Read-LedgerWords

    $processed = 0; $sincePurge = 0; $consec = @(); $stop = $null
    foreach ($r in $list) {
        $row = $r.Package; $rd0 = Join-Path $Ev ('rows\' + (Row-Dir $row))
        $prior = @($words[$row] | Where-Object { $_ })
        if ($prior.Count -and ($TerminalWords -contains $prior[-1])) { Log "SKIP $row (terminal: $($prior[-1]))"; $processed++; continue }
        $attempt = $prior.Count + 1
        $rd = if ($attempt -eq 1) { $rd0 } else { Join-Path $rd0 "attempt-$attempt" }
        New-Item -ItemType Directory -Force $rd | Out-Null

        # 1. PRE-ROW
        if (Test-Path -LiteralPath (Join-Path $Ev 'STOP')) { Log 'STOP file present: restoring and exiting'; $null = Restore-Roots; $exit = 3; return }
        $free = Free-GB
        if ($free -lt 35 -or $sincePurge -ge 30) {
            if (-not (Invoke-Purge)) { $stop = 'purge failed'; break }
            $sincePurge = 0; $free = Free-GB
            if ($free -lt 25) { $stop = "free disk $free GB after a purge"; break }
        }
        if ((Get-Go2csCount) -ne 0) { $stop = 'stray go2cs.exe at row start'; break }
        if ($row -eq 'net') {
            $bind = @(Get-NetAdapterBinding -ComponentID ms_tcpip6 -ErrorAction SilentlyContinue | ForEach-Object { "$($_.Name)=$($_.Enabled)" })
            Log "Q5 IPv6 bindings: $($bind -join '; ')"
            if (@(Get-NetAdapterBinding -Name 'Wi-Fi' -ComponentID ms_tcpip6 -ErrorAction SilentlyContinue | Where-Object Enabled).Count) { $stop = 'Q5: Wi-Fi IPv6 is BOUND (net banked with IPv6 unbound)'; break }
            foreach ($q in 1,2) {
                $ql = Join-Path $rd "netqual-$q.log"
                $qp = Start-Process go -ArgumentList 'test','-count=1','-timeout','40m','net' -NoNewWindow -PassThru -RedirectStandardOutput $ql -RedirectStandardError "$ql.err" -WorkingDirectory $env:TEMP
                $null = $qp.Handle; $null = $qp.WaitForExit(); $qrc = $qp.ExitCode
                $qr = Test-NetQualifier ((Read-Text $ql) + "`n" + (Read-Text "$ql.err")) $qrc
                Log ("Q5 netqual-{0}: rc={1} pass={2} failing=[{3}] extra=[{4}] panic={5}" -f $q, $qrc, $qr.Pass, ($qr.FailingSet -join ','), ($qr.Extra -join ','), $qr.Panic)
                if (-not $qr.Pass) { $stop = "Q5 netqual-$q failed: $($qr.Extra -join ',')"; break }
            }
            if ($stop) { break }
        }

        # 2. RUN
        $start = Get-Date; [IO.File]::WriteAllText((Join-Path $rd 'INFLIGHT'), $start.ToString('s'), (Utf8NoBom))
        $pkgMin = Get-PkgTimeoutMin $row; $budgetMin = 4 * $pkgMin + 30
        $log = Join-Path $rd 'sweep.log'; $err = Join-Path $rd 'sweep.err'
        Log ("ROW {0} start (attempt {1}, pkgTimeout {2}m, budget {3}m, free {4} GB)" -f $row, $attempt, $pkgMin, $budgetMin, $free)
        $p = Start-Process powershell -ArgumentList '-NoProfile','-ExecutionPolicy','Bypass','-File',$SweepCopy,'-Filter',$row,'-Exact','-SkipBuild' -NoNewWindow -PassThru -RedirectStandardOutput $log -RedirectStandardError $err
        $null = $p.Handle
        $done = $p.WaitForExit($budgetMin * 60 * 1000)
        $word = $null; $rc = $null; $v = $null
        if (-not $done) {
            $word = 'TIMEOUT'
            $tail = (Get-Content -LiteralPath "$Wt\src\core\$row\go2cs_test_results.json" -Tail 400 -ErrorAction SilentlyContinue) -join "`n"
            [IO.File]::WriteAllText((Join-Path $rd 'results-tail.txt'), "=== RESULTS TAIL (read first; budget kill) ===`n$tail", (Utf8NoBom))
            & taskkill /T /F /PID $p.Id 2>&1 | Out-Null
            Start-Sleep -Seconds 3
            $alive = [bool](Get-Process -Id $p.Id -ErrorAction SilentlyContinue)
            $surv = Get-StrayProcesses
            if (-not $alive) { Log "BUDGET KILL: $row child tree pid $($p.Id) confirmed dead" }
            if ($alive -or $surv.Count) { $stop = "survivor after the budget kill of $row ($($surv.Count) by path/cmdline)"; }
        } else { $rc = $p.ExitCode }
        $end = Get-Date

        # 3. PARSE
        $logText = Read-Text $log; $errText = Read-Text $err
        if ($word -ne 'TIMEOUT') {
            $v = Get-Verdict $logText $errText $row $r.Execution
            if ($v.Refusals.Count) { $word = 'REFUSED'; $stop = "refusal marker(s) in $row : $($v.Refusals -join ', ')" }
            elseif ($v.Problem) { $word = $(if ($v.Word) { $v.Word } else { 'NOT MEASURED' }); $stop = "$row verdict problem: $($v.Problem)" }
            else { $word = $v.Word }
        }

        # 4. EVIDENCE, before any restore
        $gotD = ''; $gotDArr = ''
        foreach ($name in 'go2cs_test_comparison.json','go2cs_test_results.json','go2cs_test_results.xml') {
            $src = "$Wt\src\core\$row\$name"
            if (Test-Path -LiteralPath $src) {
                $mt = (Get-Item -LiteralPath $src).LastWriteTime
                if ($mt -lt $start) { Log "  evidence $name STALE ($($mt.ToString('s')) < row start)"; Copy-Item -LiteralPath $src -Destination (Join-Path $rd "STALE-$name") }
                else { Copy-Item -LiteralPath $src -Destination (Join-Path $rd $name) }
            } else { Log "  evidence $name ABSENT" }
        }
        $cmp = Join-Path $rd 'go2cs_test_comparison.json'
        if (Test-Path -LiteralPath $cmp) {
            try { $rec = ConvertFrom-ComparisonRecord -Path $cmp; $arr = @($rec.disclosed); $gotDArr = $arr.Count
                  $gotD = @($arr | Where-Object { $rec.go -and $rec.go.ContainsKey((($_ -split ' \(')[0])) }).Count } catch { Log "  comparison record unreadable: $($_.Exception.Message)" }
        }
        if ($word -ne 'PASS') {
            $tail = (Get-Content -LiteralPath "$Wt\src\core\$row\go2cs_test_results.json" -Tail 400 -ErrorAction SilentlyContinue) -join "`n"
            if (-not (Test-Path -LiteralPath (Join-Path $rd 'results-tail.txt'))) { [IO.File]::WriteAllText((Join-Path $rd 'results-tail.txt'), "=== RESULTS TAIL (read first) ===`n$tail", (Utf8NoBom)) }
        }
        & cmd /c "git.exe -C $Wt -c core.safecrlf=false diff --numstat --ignore-cr-at-eol -- src/core > `"$rd\numstat.txt`" 2>nul"
        & cmd /c "git.exe -C $Wt diff --numstat -- src/core > `"$rd\numstat-raw.txt`" 2>nul"
        & cmd /c "git.exe -C $Wt status --porcelain > `"$rd\porcelain.txt`" 2>nul"
        $dotted = $row -replace '/', '.'
        $pagePath = "$Wt\docs\validation\current\$dotted.md"
        if (Test-Path -LiteralPath $pagePath) { Copy-Item -LiteralPath $pagePath -Destination (Join-Path $rd "page-$dotted.md") }
        & cmd /c "git.exe -C $Wt diff -- docs/validation > `"$rd\docs-validation.diff`" 2>nul"
        $headPage = (& git.exe -C $Wt show "HEAD:docs/validation/current/$dotted.md" 2>$null | Out-String)
        $pg = Compare-Page $headPage $pagePath $start
        $pageReading = $pg.Reading; if ($pg.Moved.Count) { $pageReading += ' [' + ($pg.Moved -join ',') + ']' }
        if ($pg.N -ne $null) { $pageReading += " N=$($pg.N) D=$($pg.D)" }
        $drift = Get-Drift (Join-Path $rd 'numstat.txt')
        if ($row -eq 'crypto/rsa') {
            $gd = Join-Path $rd 'generated'; New-Item -ItemType Directory -Force $gd | Out-Null
            Get-ChildItem -LiteralPath "$Wt\src\core\crypto\rsa" -Recurse -Directory -Force -ErrorAction SilentlyContinue | Where-Object { $_.Name -eq 'Generated' -or ($_.Name -eq 'generated' -and $_.FullName -match '\x5Cobj\x5C') } | ForEach-Object { $t = Join-Path $gd (($_.FullName.Substring("$Wt\src\core\crypto\rsa".Length)).TrimStart('\') -replace '\\','__'); Copy-Item -LiteralPath $_.FullName -Destination $t -Recurse -Force }
            if ($word -ne 'PASS') {
                $hits = @(Get-ChildItem -LiteralPath $rd -Recurse -File | Select-String -Pattern 'CS8785|CS9248' -ErrorAction SilentlyContinue)
                if ($hits.Count) { [IO.File]::WriteAllText((Join-Path $rd 'cs8785.txt'), (($hits | Select-Object -First 20 | ForEach-Object { $_.Line }) -join "`n"), (Utf8NoBom)) } else { [IO.File]::WriteAllText((Join-Path $rd 'cs8785.txt'), 'CS8785 text not captured', (Utf8NoBom)) }
            }
        }

        # 5. RESTORE BOTH ROOTS
        $e = Restore-Roots
        $a = Assert-Restored
        if ($e -or -not $a.Ok -or $a.Deleted) { $stop = "restore after $row : $e porcelain=$($a.Porcelain -join ' | ') tracked=$($a.Tracked)" }

        # 6. LEDGER
        $absorption = if ($v) { $v.Absorption } else { 'none' }
        $sweepWall = if ($v -and $v.Wall) { $v.Wall } else { '' }
        $got = if ($v -and $null -ne $v.Got) { $v.Got } else { '' }
        $relLog = ('rows\' + (Row-Dir $row) + $(if ($attempt -gt 1) { "\attempt-$attempt" } else { '' }) + '\sweep.log')
        Append-Lf $Ledger (@($row,$Shard,$HostNick,'verdict',$attempt,$start.ToString('s'),$end.ToString('s'),[int]($end-$start).TotalSeconds,$sweepWall,$pkgMin,$word,$got,$gotD,$gotDArr,$r.Expected,$r.Disclosed,$absorption,$pageReading,$drift,$relLog,$Base,$Base,$convSha,$convMtime,'none',$rc) -join "`t")
        Remove-Item -LiteralPath (Join-Path $rd 'INFLIGHT') -ErrorAction SilentlyContinue
        $processed++; $sincePurge++
        Log ("ROW {0} {1} got={2} D={3}/{4} banked={5}|{6} wall={7}s outer={8}s page={9} drift={10} rc={11}" -f $row, $word, $got, $gotD, $gotDArr, $r.Expected, $r.Disclosed, $sweepWall, [int]($end-$start).TotalSeconds, $pageReading, $drift, $rc)

        # three consecutive non-PASS with one first-error class = host breakage
        if ($word -eq 'PASS') { $consec = @() } else {
            $cls = $word + ':' + $(if ($logText -match '(error [A-Z]+\d+)') { $Matches[1] } elseif ($logText -match 'oracle-only check: ([^\r\n]{0,40})') { $Matches[1] } else { 'none' })
            $consec += $cls
            if ($consec.Count -ge 3 -and (@($consec[-3..-1] | Sort-Object -Unique).Count -eq 1)) { $stop = "three consecutive non-PASS rows of one class: $cls" }
        }
        if ($stop) { break }
    }
    if ($stop) { Log "RUN-LEVEL STOP: $stop"; $null = Restore-Roots; $exit = 2; return }
    Log ("END processed={0} listed={1}" -f $processed, $list.Count)
    $exit = if ($processed -eq $list.Count) { 0 } else { 1 }
}
catch { Log "DRIVER EXCEPTION: $($_.Exception.Message)"; $exit = 1 }
finally { [Console]::Out.WriteLine("DRIVER_EXIT=$exit"); [Console]::Out.Flush() }
