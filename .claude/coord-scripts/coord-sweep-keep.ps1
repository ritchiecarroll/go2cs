# coord-sweep-keep.ps1 -- run ONE filtered -Exact sweep in a worktree SOLO, PRESERVE its comparison record to the
# scratchpad BEFORE restoring the tree, print the verdict line, the results-file tail, and the go-vs-C# mismatch list,
# then restore sweep dirt and delete the pipeline's ignored record files. The union battery deletes the record unread;
# this is the instrument for re-reading a failed row.
param(
    [string] $Worktree = 'C:\Projects\go2cs\.claude\worktrees\musing-moser-d4552c',
    [Parameter(Mandatory = $true)] [string] $Package,
    [string] $SweepTimeout = '40m',
    [string] $Label = 'rerun',
    [switch] $AllowBusy
)
$ErrorActionPreference = 'Continue'
$env:DOTNET_ROOT = '$env:USERPROFILE\dotnet10'
$env:GOROOT = '$env:USERPROFILE\sdk\go1.23.12'
$env:PATH = "$env:DOTNET_ROOT;$env:GOROOT\bin;$env:PATH"
$env:MSBUILDDISABLENODEREUSE = '1'
$SP = 'C:\Projects\go2cs\.claude\coord-scripts'
function Stamp([string] $m) { Write-Output ("[{0}] {1}" -f (Get-Date -Format 'yyyy-MM-dd HH:mm:ss'), $m) }
$head = (& git.exe -C $Worktree rev-parse --short HEAD 2>$null) -join ''
Stamp "SWEEP-KEEP START package=$Package head=$head label=$Label timeout=$SweepTimeout"
$busy = Get-CimInstance Win32_Process | Where-Object { $_.Name -match '^(go2cs|BehavioralRunner|testhost|vstest\.console)\.exe$' }
if ($busy -and -not $AllowBusy) { $busy | ForEach-Object { Stamp ("  BUSY: " + $_.Name + " pid=" + $_.ProcessId) }; Stamp "box is not solo -- ABORT"; exit 1 }
$pkgDir = Join-Path $Worktree ('src\core\' + ($Package -replace '/', '\'))
$keep = Join-Path $SP ('coord-sweep-record-' + ($Package -replace '/', '.') + '-' + $Label + '-' + (Get-Date -Format 'yyyyMMdd-HHmmss'))
Set-Location (Join-Path $Worktree 'src')
$t0 = Get-Date
$out = cmd /c "powershell -NoProfile -ExecutionPolicy Bypass -File .\run-validated-sweep.ps1 -Filter $Package -Exact -TestTimeout $SweepTimeout -SkipBuild 2>&1"
$code = $LASTEXITCODE
$verdict = ($out | Select-String -Pattern ('^\s*(PASS|FAIL)\s+' + [regex]::Escape($Package) + '\s')) | Select-Object -First 1
Stamp ("sweep exit=$code elapsed=" + [int]((Get-Date) - $t0).TotalSeconds + "s : " + $(if ($verdict) { $verdict.Line.Trim() } else { '(no verdict line)' }))
# preserve the record BEFORE any restore
New-Item -ItemType Directory -Force -Path $keep | Out-Null
foreach ($r in 'go2cs_test_comparison','go2cs_test_results.json','go2cs_test_results.xml','go2cs_test_comparison.json','go2cs_test_manifest.json') {
    $p = Join-Path $pkgDir $r; if (Test-Path $p) { Copy-Item $p -Destination $keep -Recurse -Force }
}
$out | Out-File -FilePath (Join-Path $keep 'sweep-output.txt') -Encoding utf8
Stamp "record preserved at $keep : $((Get-ChildItem $keep -Recurse -File | Measure-Object).Count) files"
# tail + mismatch summary from the preserved comparison
$cmp = Join-Path $keep 'go2cs_test_comparison\results.json'
if (-not (Test-Path $cmp)) { $cmp = Join-Path $keep 'go2cs_test_comparison.json' }   # the pipeline writes the flat form at the package root
if (Test-Path $cmp) {
    $py = @"
import json,sys
p=sys.argv[1]
d=json.load(open(p,encoding='utf-8-sig'))
# schema (testConversion.go testComparison): package, status, go{test:verdict}, csharp{test:verdict}, matched,
# skipped[], disclosed[], excluded[], errors[]
go=d.get('go') or {}; cs=d.get('csharp') or {}
print('package:',d.get('package'),'status:',d.get('status'),'matched:',d.get('matched'),
      'go rows:',len(go),'cs rows:',len(cs),'disclosed:',len(d.get('disclosed') or []),
      'excluded:',len(d.get('excluded') or []),'skipped:',len(d.get('skipped') or []),'errors:',len(d.get('errors') or []))
for e in (d.get('errors') or [])[:5]: print('  error:',str(e)[:200])
names=sorted(set(go)|set(cs))
# a disclosed row still shows go=pass/cs=fail in the maps; the manifest absorbs it, so it is not a mismatch
disc=set()
for x in (d.get('disclosed') or []):
    disc.add(str(x).split(' (')[0].strip())
mis=[(n,go.get(n,''),cs.get(n,'')) for n in names if go.get(n,'')!=cs.get(n,'') and n not in disc]
print('disclosed names:',sorted(disc))
empty=[n for n,g_,c_ in mis if c_=='']
print('mismatches:',len(mis),'of which C#-empty:',len(empty))
if empty:
    e=sorted(empty); print('  C#-empty first/last (alphabetical-tail check):',e[0],'..',e[-1])
for n,g_,c_ in mis[:60]: print('   ',n,'go=',g_,'cs=',c_ or '""')
"@
    $py | Out-File -FilePath (Join-Path $keep 'summ.py') -Encoding ascii
    python (Join-Path $keep 'summ.py') $cmp 2>&1 | ForEach-Object { Stamp ("  " + $_) }
} else { Stamp "no comparison record produced (build failure or deadline before the run) -- read sweep-output.txt" }
$res = Join-Path $keep 'go2cs_test_results.json'
if (Test-Path $res) { Stamp "results tail:"; Get-Content $res -Tail 3 | ForEach-Object { Stamp ("  " + $_.Substring(0, [Math]::Min(200, $_.Length))) } }
# restore + delete the ignored record files
& git.exe -C $Worktree checkout HEAD -- src/core docs/validation/current 2>$null
& git.exe -C $Worktree clean -fdq -- src/core 2>$null
foreach ($r in 'go2cs_test_manifest.json','go2cs_test_comparison.json','go2cs_test_results.json','go2cs_test_results.xml') { $p = Join-Path $pkgDir $r; if (Test-Path $p) { Remove-Item $p -Force } }
$c = Join-Path $pkgDir 'go2cs_test_comparison'; if (Test-Path $c) { Remove-Item $c -Recurse -Force }
Stamp ("post-restore git status entries: " + ((& git.exe -C $Worktree status --porcelain 2>$null) | Measure-Object).Count)
Stamp "SWEEP-KEEP END"
exit $code
