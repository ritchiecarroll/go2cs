# coord-build-packages.ps1 -- build a list of src/core packages by csproj path with a STRICT error count.
param(
    [string] $Worktree = 'C:\Projects\go2cs\.claude\worktrees\musing-moser-d4552c',
    [string] $Packages = 'crypto,crypto/tls,database/sql/driver,encoding/json,encoding/xml,plugin'
)
$ErrorActionPreference = 'Continue'
$env:DOTNET_ROOT = '$env:USERPROFILE\dotnet10'
$env:GOROOT = '$env:USERPROFILE\sdk\go1.23.12'
$env:PATH = "$env:DOTNET_ROOT;$env:GOROOT\bin;$env:PATH"
$env:MSBUILDDISABLENODEREUSE = '1'
function Stamp([string] $m) { Write-Output ("[{0}] {1}" -f (Get-Date -Format 'yyyy-MM-dd HH:mm:ss'), $m) }
Stamp ("PACKAGE BUILDS START head=" + ((& git.exe -C $Worktree rev-parse --short HEAD 2>$null) -join '') + " packages=[$Packages]")
$failed = 0
foreach ($pkg in ($Packages -split ',')) {
    $pkg = $pkg.Trim(); if ($pkg -eq '') { continue }
    $dir = Join-Path $Worktree ('src\core\' + ($pkg -replace '/', '\'))
    $proj = Get-ChildItem -Path $dir -Filter '*.csproj' -File | Where-Object { $_.Name -notlike '*.tests.csproj' } | Select-Object -First 1
    if (-not $proj) { Stamp "MISSING csproj under $dir"; $failed++; continue }
    $t0 = Get-Date
    $out = cmd /c "dotnet build `"$($proj.FullName)`" -c Debug -m -p:UseSharedCompilation=false 2>&1"
    $code = $LASTEXITCODE
    $errs = @($out | Select-String -Pattern 'error (CS|MSB|NETSDK)[0-9]+' | Select-Object -First 10)
    Stamp ("  $pkg exit=$code strictErrors=" + $errs.Count + " elapsed=" + [int]((Get-Date) - $t0).TotalSeconds + "s")
    $errs | ForEach-Object { Stamp ("    " + $_.Line.Trim()) }
    if ($code -ne 0 -or $errs.Count -gt 0) { $failed++ }
}
Stamp "PACKAGE BUILDS END failed=$failed"
exit $failed
