# coord-reflect-tests-build.ps1 -- the reflect -tests CONVERT + BUILD leg (the standing gate for
# lift/dedup/anonymous-type converter changes), run in the coordinator worktree against the head's
# converter, then the reflect tree restored (unbanked test artifacts + class-2 production shapes).
param([string] $Worktree = 'C:\Projects\go2cs\.claude\worktrees\musing-moser-d4552c', [string] $Package = 'reflect')
$ErrorActionPreference = 'Continue'
$env:DOTNET_ROOT = '$env:USERPROFILE\dotnet10'
$env:GOROOT = '$env:USERPROFILE\sdk\go1.23.12'
$env:PATH = "$env:DOTNET_ROOT;$env:GOROOT\bin;$env:PATH"
$env:MSBUILDDISABLENODEREUSE = '1'
function Stamp([string] $m) { Write-Output ("[{0}] {1}" -f (Get-Date -Format 'yyyy-MM-dd HH:mm:ss'), $m) }
$goDir = Join-Path $Worktree 'src\go2cs'
$exe = Join-Path $goDir 'bin\go2cs.exe'
$goroot = (& go env GOROOT 2>&1) -join ''
$pkgGo = Join-Path $goroot 'src\reflect'
$pkgOut = Join-Path $Worktree 'src\core\reflect'
Stamp "$Package -TESTS BUILD START head=$((& git -C $Worktree rev-parse HEAD 2>&1) -join ' ') GOROOT=$goroot"
Set-Location $goDir
if ((Get-Location).Path -ne $goDir) { Stamp "cd FAILED"; exit 1 }
cmd /c "go build -o bin\go2cs.exe . 2>&1"
if (-not (Test-Path $exe)) { Stamp "MISSING converter at $exe"; exit 1 }
Stamp ("converter built: " + (Get-Item $exe).Length + " bytes")
$t0 = Get-Date
cmd /c "`"$exe`" -tests -test-action convert -test-timeout 20m `"$pkgGo`" `"$pkgOut`" 2>&1"
Stamp ("convert exit=$LASTEXITCODE elapsed=" + [int]((Get-Date) - $t0).TotalSeconds + "s")
$t0 = Get-Date
cmd /c "`"$exe`" -tests -test-action build -test-timeout 20m `"$pkgGo`" `"$pkgOut`" 2>&1"
Stamp ("build exit=$LASTEXITCODE elapsed=" + [int]((Get-Date) - $t0).TotalSeconds + "s")
$dirty = @(& git -C $Worktree status --short -- src/core/$Package 2>&1)
Stamp ("post-run $Package dirt entries: " + $dirty.Count)
& git -C $Worktree checkout HEAD -- src/core/$Package 2>&1 | Out-Null
Get-ChildItem -Path (Join-Path $Worktree 'src\coreeflect') -Recurse -Force -Include 'go2cs_test_manifest.json','go2cs_test_comparison.json','go2cs_test_results.json','go2cs_test_results.xml' -ErrorAction SilentlyContinue | Remove-Item -Force -ErrorAction SilentlyContinue
Get-ChildItem -Path (Join-Path $Worktree 'src\coreeflect') -Recurse -Force -Directory -Filter 'go2cs_test_comparison' -ErrorAction SilentlyContinue | Remove-Item -Recurse -Force -ErrorAction SilentlyContinue
& git -C $Worktree clean -fdq -- src/core/$Package 2>&1 | Out-Null
Stamp ("after restore $Package dirt entries: " + @(& git -C $Worktree status --short -- src/core/$Package 2>&1).Count)
Stamp "REFLECT -TESTS BUILD END"
exit 0
