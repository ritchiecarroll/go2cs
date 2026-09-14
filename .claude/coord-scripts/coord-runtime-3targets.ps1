# coord-runtime-3targets.ps1 -- build one core package (default runtime) on all three GoTargetOS values, purging
# bin/obj/Generated between switches (a GoTargetOS switch changes the Compile item set while timestamps do not move).
param(
    [string] $Worktree = 'C:\Projects\go2cs\.claude\worktrees\musing-moser-d4552c',
    [string] $Package = 'runtime'
)
$ErrorActionPreference = 'Continue'
$env:DOTNET_ROOT = '$env:USERPROFILE\dotnet10'
$env:GOROOT = '$env:USERPROFILE\sdk\go1.23.12'
$env:PATH = "$env:DOTNET_ROOT;$env:GOROOT\bin;$env:PATH"
$env:MSBUILDDISABLENODEREUSE = '1'
function Stamp([string] $m) { Write-Output ("[{0}] {1}" -f (Get-Date -Format 'yyyy-MM-dd HH:mm:ss'), $m) }
$dir = Join-Path $Worktree ('src\core\' + ($Package -replace '/', '\'))
$proj = Get-ChildItem -Path $dir -Filter '*.csproj' -File | Where-Object { $_.Name -notlike '*.tests.csproj' } | Select-Object -First 1
if (-not $proj) { Stamp "MISSING csproj under $dir"; exit 1 }
Stamp ("3-TARGET BUILD START package=$Package head=" + ((& git.exe -C $Worktree rev-parse --short HEAD 2>$null) -join ''))
$failed = 0
foreach ($goos in @('windows', 'linux', 'darwin')) {
    foreach ($d in @('bin', 'obj', 'Generated')) { $p = Join-Path $dir $d; if (Test-Path $p) { Remove-Item -Recurse -Force $p -ErrorAction SilentlyContinue } }
    $t0 = Get-Date
    $out = cmd /c "dotnet build `"$($proj.FullName)`" -c Debug --no-incremental -m -p:UseSharedCompilation=false -p:GoTargetOS=$goos 2>&1"
    $code = $LASTEXITCODE
    $errs = @($out | Select-String -Pattern 'error (CS|MSB|NETSDK)[0-9]+' | Select-Object -First 8)
    Stamp ("  GoTargetOS=$goos exit=$code strictErrors=" + $errs.Count + " elapsed=" + [int]((Get-Date) - $t0).TotalSeconds + "s")
    $errs | ForEach-Object { Stamp ("    " + $_.Line.Trim()) }
    if ($code -ne 0 -or $errs.Count -gt 0) { $failed++ }
}
# leave the tree on the default target so later legs are not poisoned by the last switch
foreach ($d in @('bin', 'obj', 'Generated')) { $p = Join-Path $dir $d; if (Test-Path $p) { Remove-Item -Recurse -Force $p -ErrorAction SilentlyContinue } }
Stamp "3-TARGET BUILD END failed=$failed (bin/obj/Generated purged; default target restored on next build)"
exit $failed
