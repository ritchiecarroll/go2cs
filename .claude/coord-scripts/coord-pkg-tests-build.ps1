# coord-pkg-tests-build.ps1 -- fresh -tests convert + build of ONE stdlib package at the worktree head, then restore.
# Generalizes coord-reflect-tests-build.ps1: -Package runtime|reflect|... (import path with forward slashes).
param(
    [string] $Worktree = 'C:\Projects\go2cs\.claude\worktrees\musing-moser-d4552c',
    [string] $Package  = 'runtime',
    [string] $Timeout  = '10m'
)
$ErrorActionPreference = 'Continue'
$env:DOTNET_ROOT = '$env:USERPROFILE\dotnet10'
$env:GOROOT = '$env:USERPROFILE\sdk\go1.23.12'
$env:PATH = "$env:DOTNET_ROOT;$env:GOROOT\bin;$env:PATH"
$env:MSBUILDDISABLENODEREUSE = '1'
function Stamp([string] $m) { Write-Output ("[{0}] {1}" -f (Get-Date -Format 'yyyy-MM-dd HH:mm:ss'), $m) }
$rel = $Package -replace '/', '\'
$goDir = Join-Path $env:GOROOT ('src\' + $rel)
$outDir = Join-Path $Worktree ('src\core\' + $rel)
$exe = Join-Path $Worktree 'src\go2cs\bin\go2cs.exe'
$src = Join-Path $Worktree 'src'
Stamp ("PKG -TESTS BUILD START package=$Package head=" + ((& git.exe -C $Worktree rev-parse --short HEAD 2>$null) -join '') + " GOROOT=$env:GOROOT")
foreach ($p in @($goDir, $outDir)) { if (-not (Test-Path $p)) { Stamp "MISSING $p -- ABORT"; exit 1 } }
Set-Location (Join-Path $Worktree 'src\go2cs')
cmd /c "go build -o `"$exe`" . 2>&1" | Out-Null
if (-not (Test-Path $exe)) { Stamp "converter build FAILED ($exe missing) -- ABORT"; exit 1 }
Stamp ("converter built: " + (Get-Item $exe).Length + " bytes")
$t0 = Get-Date
$conv = cmd /c "`"$exe`" -tests -test-action convert -test-timeout $Timeout -go2cspath `"$src`" `"$goDir`" `"$outDir`" 2>&1"
$cc = $LASTEXITCODE
Stamp ("convert exit=$cc elapsed=" + [int]((Get-Date) - $t0).TotalSeconds + "s")
$conv | Select-String -Pattern 'did not fully type-check|visit file error|panic:' | Select-Object -First 5 | ForEach-Object { Stamp ("  " + $_.Line.Trim()) }
$t0 = Get-Date
$bld = cmd /c "`"$exe`" -tests -test-action build -test-timeout $Timeout -go2cspath `"$src`" `"$goDir`" `"$outDir`" 2>&1"
$bc = $LASTEXITCODE
$errs = @($bld | Select-String -Pattern 'error (CS|MSB|NETSDK)[0-9]+' | Select-Object -First 15)
Stamp ("build exit=$bc strictErrors=" + $errs.Count + " elapsed=" + [int]((Get-Date) - $t0).TotalSeconds + "s")
$errs | ForEach-Object { Stamp ("  " + $_.Line.Trim()) }
$dirt = @(& git.exe -C $Worktree status --porcelain -- ('src/core/' + $Package) 2>$null)
Stamp ("post-run $Package dirt entries: " + $dirt.Count)
& git.exe -C $Worktree checkout HEAD -- ('src/core/' + $Package) 2>$null | Out-Null
& git.exe -C $Worktree clean -fdq -- ('src/core/' + $Package) 2>$null | Out-Null
Get-ChildItem -Path $outDir -Recurse -Force -Include 'go2cs_test_manifest.json','go2cs_test_comparison.json','go2cs_test_results.json','go2cs_test_results.xml' -ErrorAction SilentlyContinue | Remove-Item -Force -ErrorAction SilentlyContinue
Get-ChildItem -Path $outDir -Recurse -Force -Directory -Filter 'go2cs_test_comparison' -ErrorAction SilentlyContinue | Remove-Item -Recurse -Force -ErrorAction SilentlyContinue
$dirt = @(& git.exe -C $Worktree status --porcelain -- ('src/core/' + $Package) 2>$null)
Stamp ("after restore $Package dirt entries: " + $dirt.Count)
Stamp "PKG -TESTS BUILD END"
if ($cc -ne 0 -or $bc -ne 0 -or $errs.Count -gt 0) { exit 1 } else { exit 0 }
