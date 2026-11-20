# i9 .4 -tests regen (COORD 2026-11-18 13:57Z), the .3 method (docs/phase4/data/dot3-tests-regen/testsregen.ps1) with
# this box's paths and rules: for EVERY roster row (Get-ValidatedRosterRows, never a hand list) the convert-only action
# `go2cs -tests -test-action convert -go2cspath <tree>/src <GOROOT>/src/<pkg> <tree>/src/core/<pkg>`, in place on the
# worktree at master b63418fafc. Serial, one conversion on the box, no test runs. Affinity 0xF, GOMAXPROCS=4.
param([Parameter(Mandatory)][string] $Run, [int] $Limit = 0)
$ErrorActionPreference = 'Continue'
[Diagnostics.Process]::GetCurrentProcess().ProcessorAffinity = [IntPtr]0xF
$env:PATH = "$env:USERPROFILE\sdk\go1.26.11\bin;" + $env:PATH
$env:GOTOOLCHAIN = 'local'; $env:GOMAXPROCS = '4'; $env:GOFLAGS = '-p=4'; $env:CGO_ENABLED = '0'
$env:TEMP = 'D:\go2cs-build\i9-b4-tmp'; $env:TMP = $env:TEMP
$repo = 'D:\go2cs-build\i9-dot4'
$src = Join-Path $repo 'src'
$log = "D:\go2cs-build\i9-b4-work\logs\dot4-testsregen-$Run"
New-Item -ItemType Directory -Force -Path $log | Out-Null
function Whea { @(Get-WinEvent -ProviderName Microsoft-Windows-WHEA-Logger -ErrorAction SilentlyContinue).Count }
function LastCrash { $e = Get-WinEvent -FilterHashtable @{LogName='System'; Id=41,1001} -MaxEvents 1 -ErrorAction SilentlyContinue; if ($e) { $e.TimeCreated.ToUniversalTime().ToString('yyyy-MM-dd HH:mm:ssZ') } else { 'none' } }
. (Join-Path $src '_roster.ps1')
$rows = @(Get-ValidatedRosterRows -Path (Join-Path $repo 'docs\ValidatedTestPackages.md'))
$goroot = (& go env GOROOT).Trim()
$gov = (& go version)
if ($gov -notmatch 'go1\.26\.11 ') { "ABORT: $gov"; exit 2 }
$exe = Join-Path $src 'go2cs\bin\go2cs.exe'
"rows=$($rows.Count) $gov exe-sha256-prefix=$((Get-FileHash $exe -Algorithm SHA256).Hash.Substring(0,16).ToLower()) head=$(git -C $repo rev-parse HEAD) dirty=$(@(git -C $repo status --porcelain 2>$null).Count) whea=$(Whea) lastcrash=$(LastCrash) start=$((Get-Date).ToUniversalTime().ToString('s'))Z"
$i = 0; $fail = 0
foreach ($r in $rows) {
    $i++
    if ($Limit -gt 0 -and $i -gt $Limit) { break }
    $pkg = $r.Package
    $goDir = Join-Path $goroot ('src\' + ($pkg -replace '/', '\'))
    $outDir = Join-Path $src ('core\' + ($pkg -replace '/', '\'))
    $t0 = Get-Date
    $out = & $exe -tests -test-action convert -go2cspath $src $goDir $outDir 2>&1
    $rc = $LASTEXITCODE
    $secs = [int]((Get-Date) - $t0).TotalSeconds
    [System.IO.File]::WriteAllLines((Join-Path $log (($pkg -replace '/', '.') + '.txt')), [string[]]@($out | ForEach-Object { "$_" }))
    if ($rc -ne 0) { $fail++ }
    "{0,3}/{1} {2,-40} rc={3} {4}s" -f $i, $rows.Count, $pkg, $rc, $secs
}
"done rows=$i failed=$fail whea=$(Whea) lastcrash=$(LastCrash) end=$((Get-Date).ToUniversalTime().ToString('s'))Z"
