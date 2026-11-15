# run-gate.ps1 <go2cs ref> <local|nuget> [GoStdLibVersion] : the typescript-go-cs gate on WINDOWS, end to end, at one ref.
# The PowerShell twin of run-gate.sh: the same steps, in the same order, with the same inputs, outputs and skip rules.
# Only the host-specific parts differ: GoTargetOS=windows, .exe names, robocopy instead of tar, apphost paths
# recorded in <name>-cs.path files instead of symlinks. Keep the two scripts step-for-step identical.
# RESUMABLE: every step writes into $W\gate-<sha>-<mode>\ and is skipped when its output exists, so a run cut off by
# a time limit is continued by running the same command again. Every step is gated on the previous one's exit code.
#   1. a detached worktree of the go2cs clone at <ref>, and its go2cs binary
#   2. the module: TypeScript ba29a56299 tsc\ minus its top-level testdata\, plus go\cmd\* from this kit; NATIVE demos
#      built from it unmodified; the corpus extracted; then a copy with the xxh3 fork's replace + go mod tidy for the conversion
#   3. go2cs -recurse (local) or -recurse=nuget; the five refused packages are skipped (rc 1 by design)
#   4. dotnet build of the four demos, Release, GoTargetOS=windows
#   5. compare.py (parse demo + corpus), demo_compare.py x3, typecheck_slice.py x2 (type-check, emit: all 7,404)
#   6. layout.py stage + fill + census into $W\gate-<sha>-<mode>\stage
# Env: GO2CS (a go2cs clone), TS (a TypeScript checkout at ba29a56299), W (work root), KIT (this directory),
#      DENY (a local file of name regexes for the census; never committed),
#      OWNER_README (optional: the repository's README.md; fill then writes README.patch beside the staged tree),
#      PYTHON (optional: the Python 3 launcher; default "python").
# Run with: pwsh -NoProfile -File run-gate.ps1 <ref> <local|nuget> [GoStdLibVersion]
param(
    [Parameter(Mandatory = $true)][string]$Ref,
    [Parameter(Mandatory = $true)][ValidateSet("local", "nuget")][string]$Mode,
    [string]$GoStdLibVersion = ""
)
$ErrorActionPreference = "Stop"
$PSNativeCommandUseErrorActionPreference = $false  # native exit codes are read from $LASTEXITCODE, never thrown
foreach ($v in "GO2CS", "TS", "W") { if (-not [Environment]::GetEnvironmentVariable($v)) { throw "$v is not set" } }
$GO2CS = $env:GO2CS; $TSR = $env:TS; $WR = $env:W
$KIT = if ($env:KIT) { $env:KIT } else { $PSScriptRoot }
$PY = if ($env:PYTHON) { $env:PYTHON } else { "python" }
$env:GOTOOLCHAIN = "local"; $env:CGO_ENABLED = "0"; $env:GOWORK = "off"
$env:GOSUMDB = "sum.golang.org"; $env:GOPROXY = "https://proxy.golang.org"
$env:GoTargetOS = "windows"; $env:MSBUILDDISABLENODEREUSE = "1"; $env:DOTNET_CLI_TELEMETRY_OPTOUT = "1"

git -C $GO2CS fetch -q origin; if ($LASTEXITCODE -ne 0) { Write-Output "fetch failed"; exit 1 }
$sha = git -C $GO2CS rev-parse --short=10 "$Ref^{commit}"; if ($LASTEXITCODE -ne 0) { Write-Output "bad ref $Ref"; exit 1 }
$T = Join-Path $WR "gate-$sha-$Mode"; New-Item -ItemType Directory -Force $T | Out-Null; $log = Join-Path $T "gate.log"
function Step([string]$m) { $l = "== $([DateTime]::UtcNow.ToString('HH:mm:ss'))Z $m"; Write-Output $l; Add-Content -Path $log -Value $l }
function Die([string]$m) { $l = "FAILED: $m"; Write-Output $l; Add-Content -Path $log -Value $l; exit 1 }
# Runs a native command with its output appended to a log; returns its exit code (never the pipe's).
function Run([string]$logFile, [string]$exe, [string[]]$argv, [string]$cwd = $null) {
    if ($cwd) { Push-Location $cwd }
    try { & $exe @argv *>> $logFile; return $LASTEXITCODE } finally { if ($cwd) { Pop-Location } }
}
$WT = Join-Path $T "go2cs"; $MODN = Join-Path $T "tsc-native"; $MODC = Join-Path $T "tsc-convert"
$OUT = Join-Path $T "out"; $R = Join-Path $T "results"; $B = Join-Path $T "bin"
New-Item -ItemType Directory -Force $B, $R | Out-Null
$go2csExe = Join-Path $B "go2cs.exe"

if (-not (Test-Path $go2csExe)) {
    Step "worktree + go2cs at $sha"
    if (-not (Test-Path $WT)) { git -C $GO2CS worktree add -q --detach $WT $sha; if ($LASTEXITCODE -ne 0) { Die worktree } }
    if ((Run $log "go" @("build", "-o", $go2csExe, ".") (Join-Path $WT "src\go2cs")) -ne 0) { Die "go2cs build" }
}

if (-not (Test-Path (Join-Path $B ".native-ok"))) {
    Step "native module, demos, corpus"
    if (Test-Path $MODN) { Remove-Item -Recurse -Force $MODN }
    # tar --exclude=./testdata excludes only the TOP-LEVEL testdata; internal\parser\testdata is kept. robocopy /XD with
    # the full path does the same. robocopy exit codes below 8 are success.
    robocopy (Join-Path $TSR "tsc") $MODN /E /XD (Join-Path $TSR "tsc\testdata") /NFL /NDL /NJH /NJS /NP *>> $log
    if ($LASTEXITCODE -ge 8) { Die copy }
    Copy-Item -Recurse -Force (Join-Path $KIT "go\cmd\*") (Join-Path $MODN "cmd")
    if ((Run $log "go" @("mod", "download") $MODN) -ne 0) { Die "mod download" }
    if ((Run $log "go" @("mod", "verify") $MODN) -ne 0) { Die "mod verify" }
    foreach ($d in "parsedemo", "checkdemo", "emitdemo", "mfdemo", "parsecorpus-extract") {
        if ((Run $log "go" @("build", "-o", (Join-Path $B "$d-go.exe"), "./cmd/$d") $MODN) -ne 0) { Die "native $d" }
    }
    if (-not (Test-Path (Join-Path $T "corpus"))) {
        if ((Run $log (Join-Path $B "parsecorpus-extract-go.exe") @((Join-Path $TSR "tsc"), (Join-Path $T "corpus"))) -ne 0) { Die extract }
    }
    New-Item -ItemType File -Force (Join-Path $B ".native-ok") | Out-Null
}

if (-not (Test-Path (Join-Path $OUT ".convert-done"))) {
    Step "convert ($Mode)"
    foreach ($p in $MODC, $OUT) { if (Test-Path $p) { Remove-Item -Recurse -Force $p } }
    Copy-Item -Recurse $MODN $MODC
    $fork = Join-Path $WT "src\forks\github.com\zeebo\xxh3@v1.1.0"
    if ((Run $log "go" @("mod", "edit", "-replace", "github.com/zeebo/xxh3=$fork") $MODC) -ne 0) { Die replace }
    if ((Run $log "go" @("mod", "tidy") $MODC) -ne 0) { Die tidy }
    $flag = if ($Mode -eq "nuget") { "-recurse=nuget" } else { "-recurse" }
    $convLog = Join-Path $T "convert.log"
    & $go2csExe $flag -go2cspath (Join-Path $WT "src") $MODC $OUT *> $convLog; $rc = $LASTEXITCODE
    $l = "convert rc=$rc (1 expected: refused packages skipped)"; Write-Output $l; Add-Content -Path $log -Value $l
    if (-not (Select-String -Path $convLog -Pattern "packages converted" -Quiet)) { Die "convert did not finish" }
    go clean -cache; New-Item -ItemType File -Force (Join-Path $OUT ".convert-done") | Out-Null
}

$C = Join-Path $OUT "src\github.com\microsoft\TypeScript\tsc\cmd"
$P = @(); if ($GoStdLibVersion) { $P = @("-p:GoStdLibVersion=$GoStdLibVersion") }
foreach ($d in "checkdemo", "parsedemo", "emitdemo", "mfdemo") {
    if (Test-Path (Join-Path $B ".built-$d")) { continue }
    Step "build $d"
    $bl = Join-Path $T "build-$d.log"
    dotnet build (Join-Path $C "$d\$d.csproj") -c Release -m:2 -p:UseSharedCompilation=false -nodeReuse:false @P *> $bl
    if ($LASTEXITCODE -ne 0) { Die "build $d" }
    $m = Select-String -Path $bl -Pattern "$d -> (.*\\$d)\.dll" | Select-Object -Last 1
    if (-not $m) { Die "no output line for $d" }
    $exe = $m.Matches[0].Groups[1].Value + ".exe"
    if (-not (Test-Path $exe)) { Die "no apphost for $d" }
    Set-Content -Path (Join-Path $B "$d-cs.path") -Value $exe -NoNewline
    New-Item -ItemType File -Force (Join-Path $B ".built-$d") | Out-Null
}
function CsExe([string]$d) { Get-Content -Raw (Join-Path $B "$d-cs.path") }
function GoExe([string]$d) { Join-Path $B "$d-go.exe" }

$S = Join-Path $KIT "samples"
if (-not (Test-Path (Join-Path $R "parse\results.json"))) {
    Step "parse demo + corpus"
    Run $log $PY @("-I", (Join-Path $KIT "compare.py"), "--native", (GoExe parsedemo), "--converted", (CsExe parsedemo), "--samples", $S, "--corpus", (Join-Path $T "corpus"), "--out", (Join-Path $R "parse")) | Out-Null
}
if (-not (Test-Path (Join-Path $R "checkdemo.json"))) {
    Step "check demo"
    Run $log $PY @("-I", (Join-Path $KIT "demo_compare.py"), "--name", "checkdemo", "--native", (GoExe checkdemo), "--converted", (CsExe checkdemo), "--cwd", $S, "--out", $R, "--", "check.ts", "good.ts") | Out-Null
}
if (-not (Test-Path (Join-Path $R "emitdemo.json"))) {
    Step "emit demo"
    Run $log $PY @("-I", (Join-Path $KIT "demo_compare.py"), "--name", "emitdemo", "--native", (GoExe emitdemo), "--converted", (CsExe emitdemo), "--cwd", $S, "--out", $R, "--", "good.ts", "bad.ts", "emit.ts") | Out-Null
}
if (-not (Test-Path (Join-Path $R "mfdemo.json"))) {
    Step "multi-file"
    Run $log $PY @("-I", (Join-Path $KIT "demo_compare.py"), "--name", "mfdemo", "--native", (GoExe mfdemo), "--converted", (CsExe mfdemo), "--cwd", (Join-Path $S "projects"), "--out", $R, "--", "shop", "legacy") | Out-Null
}
foreach ($p in "emit", "check") {
    if (Test-Path (Join-Path $R "$p-full\slice.json")) { continue }
    Step "full $p pass (resumable)"
    Run (Join-Path $T "$p-full.log") $PY @("-I", (Join-Path $KIT "typecheck_slice.py"), "--native", (GoExe "${p}demo"), "--converted", (CsExe "${p}demo"), "--corpus", (Join-Path $T "corpus"), "--out", (Join-Path $R "$p-full"), "--every", "1", "--timeout", "3600") | Out-Null
    if (-not (Test-Path (Join-Path $R "$p-full\slice.json"))) { Die "$p pass incomplete" }
}
if (-not (Test-Path (Join-Path $KIT "controls.tsv"))) { Die "controls.tsv (measured by hand on this box) missing from the kit" }
Copy-Item (Join-Path $KIT "controls.tsv") (Join-Path $R "controls.tsv")

Step "stage + fill + census"
foreach ($p in (Join-Path $T "stage"), (Join-Path $T "proof-report")) { if (Test-Path $p) { Remove-Item -Recurse -Force $p } }
New-Item -ItemType Directory -Force (Join-Path $T "proof-report") | Out-Null
if ((Run $log $PY @("-I", (Join-Path $KIT "layout.py"), "stage", "--tree", $OUT, "--tsc", $MODC, "--ts-root", $TSR, "--proof", $KIT, "--dest", (Join-Path $T "stage"))) -ne 0) { Die stage }
$sdk = (dotnet --version); $plat = "$(go env GOOS)/$(go env GOARCH)"; $gov = ((go version) -split " ")[2]
$fill = @("-I", (Join-Path $KIT "layout.py"), "fill", "--dest", (Join-Path $T "stage"), "--proof", $KIT, "--results", $R, "--converter", $sha,
    "--gostdlib", $(if ($GoStdLibVersion) { $GoStdLibVersion } else { "local" }), "--sdk", $sdk, "--platform", $plat, "--goversion", $gov)
if ($env:OWNER_README) { $fill += @("--owner-readme", $env:OWNER_README) }
if ((Run $log $PY $fill) -ne 0) { Die fill }
$census = @("-I", (Join-Path $KIT "layout.py"), "census", "--dest", (Join-Path $T "stage"), "--mode", $Mode)
if ($env:DENY) { $census += @("--deny", $env:DENY) }
& $PY @census 2>&1 | Tee-Object -FilePath $log -Append; $crc = $LASTEXITCODE
Step "done; census rc=$crc"; exit $crc
