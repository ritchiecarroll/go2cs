# coord-reflect-run.ps1 -- RUN an UNBANKED package's Go test suite through the -tests pipeline (convert + build + run +
# compare) in the coordinator worktree, preserve the comparison record to the scratchpad BEFORE any restore, and print the
# verdict shape: go/cs row counts, mismatches, C#-EMPTY rows (mass-empty = alphabetical tail), the results tail, and the
# un-disclosed divergence count. The validated sweep cannot do this (it walks the ROSTER; an unbanked row matches nothing
# and exits 0 -- a vacuous green, measured 2026-09-02). Restores src/core + docs/validation/current and deletes the
# pipeline's ignored record files afterwards.
param(
    [string] $Worktree = 'C:\Projects\go2cs\.claude\worktrees\musing-moser-d4552c',
    [string] $Package = 'reflect',
    [string] $Timeout = '30m',
    [string] $Label = 'run',
    [switch] $AllowBusy,
    [string] $TestConfig = ''
)
$ErrorActionPreference = 'Continue'
$env:DOTNET_ROOT = '$env:USERPROFILE\dotnet10'
$env:GOROOT = '$env:USERPROFILE\sdk\go1.23.12'
$env:PATH = "$env:DOTNET_ROOT;$env:GOROOT\bin;$env:PATH"
$env:MSBUILDDISABLENODEREUSE = '1'
$SP = 'C:\Projects\go2cs\.claude\coord-scripts'
function Stamp([string] $m) { Write-Output ("[{0}] {1}" -f (Get-Date -Format 'yyyy-MM-dd HH:mm:ss'), $m) }
# The preserved-record naming and copy live in ONE file, shared with coord-union-battery.ps1's sweeps
# leg (CLAUDE.md rule 4). That is what lets the SET DIFF below find a record the SWEEPS leg preserved
# as this package's previous run, with no hand-copying.
$recordKeep = Join-Path $SP 'coord-record-keep.ps1'
if (-not (Test-Path $recordKeep)) { Stamp "MISSING (existence assertion failed): $recordKeep"; Stamp "PKG RUN ABORT"; exit 1 }
. $recordKeep
$head = (& git.exe -C $Worktree rev-parse --short HEAD 2>$null) -join ''
$goVer = (& go version) -join ''
Stamp "PKG RUN START package=$Package head=$head timeout=$Timeout go=[$goVer]"
if ($goVer -notmatch 'go1\.23\.12') { Stamp "toolchain is not go1.23.12 -- ABORT"; exit 1 }
$busy = Get-CimInstance Win32_Process | Where-Object { $_.Name -match '^(go2cs|BehavioralRunner|testhost|vstest\.console)\.exe$' }
if ($busy) { $busy | ForEach-Object { Stamp ("  BUSY: " + $_.Name + " pid=" + $_.ProcessId) }; if ($AllowBusy) { Stamp "box is not solo -- CONTINUING (AllowBusy; a verdict comparison, not a wall-time gate)" } else { Stamp "box is not solo -- ABORT"; exit 1 } }
$exe = Join-Path $Worktree 'src\go2cs\bin\go2cs.exe'
$cfgArg = if ($TestConfig) { "-test-config $TestConfig" } else { '' }
$src = Join-Path $env:GOROOT ('src\' + ($Package -replace '/', '\'))
$dst = Join-Path $Worktree ('src\core\' + ($Package -replace '/', '\'))
Set-Location (Join-Path $Worktree 'src\go2cs')
$t0 = Get-Date
$out = cmd /c "`"$exe`" -tests -test-action all $cfgArg -test-timeout $Timeout -go2cspath `"$(Join-Path $Worktree 'src')`" `"$src`" `"$dst`" 2>&1"
$code = $LASTEXITCODE
Stamp ("pipeline exit=$code elapsed=" + [int]((Get-Date) - $t0).TotalSeconds + "s")
# -AlwaysCreate: this leg writes its own pipeline-output.txt and summ.py into the directory, so it
# wants one even for a run that produced no record files at all (a build that died, a torn tree).
$keepInfo = Save-CoordPkgRecord -Package $Package -OutDir $dst -Label $Label -Root $SP -AlwaysCreate
$keep = $keepInfo.Path
$out | Out-File -FilePath (Join-Path $keep 'pipeline-output.txt') -Encoding utf8
Stamp "record preserved at $keep : $((Get-ChildItem $keep -Recurse -File | Measure-Object).Count) files"
$cmp = Join-Path $keep 'go2cs_test_comparison\results.json'; if (-not (Test-Path $cmp)) { $cmp = Join-Path $keep 'go2cs_test_comparison.json' }
if (Test-Path $cmp) {
    $py = @"
import json,sys
d=json.load(open(sys.argv[1],encoding='utf-8-sig'))
go=d.get('go') or {}; cs=d.get('csharp') or {}
disc=set(str(x).split(' (')[0].strip() for x in (d.get('disclosed') or []))
names=sorted(set(go)|set(cs))
mis=[(n,go.get(n,''),cs.get(n,'')) for n in names if go.get(n,'')!=cs.get(n,'') and n not in disc]
empty=[n for n,g_,c_ in mis if c_=='']
print('package:',d.get('package'),'status:',d.get('status'),'go rows:',len(go),'cs rows:',len(cs),'disclosed:',len(disc),'excluded:',len(d.get('excluded') or []),'errors:',len(d.get('errors') or []))
print('UN-DISCLOSED mismatches:',len(mis),'| C#-empty:',len(empty), ('| empty span: '+sorted(empty)[0]+' .. '+sorted(empty)[-1]) if empty else '')
from collections import Counter
print('cs verdicts:',dict(Counter(cs.values())))
for n,g_,c_ in mis[:40]: print('   ',n,'go=',g_,'cs=',c_ or '""')
# SET diff against the most recent PRESERVED record for this package (the moved-set is the verdict; counts are a currency)
import glob,os
prev=[q for q in sorted(glob.glob(os.path.join(os.path.dirname(os.path.dirname(sys.argv[1])),'coord-pkg-run-record-'+sys.argv[2]+'-*'))) if os.path.dirname(sys.argv[1])!=q and not os.path.dirname(sys.argv[1]).startswith(q)]
prev.sort(key=os.path.getmtime)
if prev:
    pq=prev[-1]; pc=os.path.join(pq,'go2cs_test_comparison','results.json')
    if not os.path.exists(pc): pc=os.path.join(pq,'go2cs_test_comparison.json')
    if os.path.exists(pc):
        pd=json.load(open(pc,encoding='utf-8-sig')); pgo=pd.get('go') or {}; pcs=pd.get('csharp') or {}
        pmis=set(n for n in set(pgo)|set(pcs) if pgo.get(n,'')!=pcs.get(n,''))
        cmis=set(n for n in names if go.get(n,'')!=cs.get(n,''))
        print('SET DIFF vs',os.path.basename(pq),'(config',(pd.get('environment') or {}).get('configuration') or pd.get('configuration'),'->',(d.get('environment') or {}).get('configuration') or d.get('configuration'),')')
        print('  FIXED  (diverged before, agrees now):',sorted(pmis-cmis))
        print('  BROKEN (agreed before, diverges now):',sorted(cmis-pmis))
        print('  rows before/now:',len(set(pgo)|set(pcs)),'/',len(names))
    else: print('SET DIFF: previous record has no comparison file:',pq)
else: print('SET DIFF: no previous preserved record for this package')
"@
    $py | Out-File -FilePath (Join-Path $keep 'summ.py') -Encoding ascii
    python (Join-Path $keep 'summ.py') $cmp ($Package -replace '/', '.') 2>&1 | ForEach-Object { Stamp ("  " + $_) }
} else { Stamp "no comparison record -- read pipeline-output.txt"; $out | Select-Object -Last 8 | ForEach-Object { Stamp ("  " + $_) } }
$res = Join-Path $keep 'go2cs_test_results.json'
if (Test-Path $res) { Stamp "results tail:"; Get-Content $res -Tail 3 | ForEach-Object { Stamp ("  " + $_.Substring(0, [Math]::Min(180, $_.Length))) } }
& git.exe -C $Worktree checkout HEAD -- src/core docs/validation/current 2>$null
& git.exe -C $Worktree clean -fdq -- src/core 2>$null
foreach ($r in 'go2cs_test_manifest.json','go2cs_test_comparison.json','go2cs_test_results.json','go2cs_test_results.xml') { $p = Join-Path $dst $r; if (Test-Path $p) { Remove-Item $p -Force } }
$c = Join-Path $dst 'go2cs_test_comparison'; if (Test-Path $c) { Remove-Item $c -Recurse -Force }
Stamp ("post-restore git status entries: " + ((& git.exe -C $Worktree status --porcelain 2>$null) | Measure-Object).Count)
Stamp "PKG RUN END"
exit $code
