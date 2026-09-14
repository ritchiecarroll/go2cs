param([string] $Worktree = 'C:\Projects\go2cs\.claude\worktrees\musing-moser-d4552c', [switch] $NoBuild)
$ErrorActionPreference = 'Continue'
$env:DOTNET_ROOT = '$env:USERPROFILE\dotnet10'
$env:GOROOT = '$env:USERPROFILE\sdk\go1.23.12'
$env:PATH = "$env:DOTNET_ROOT;$env:GOROOT\bin;$env:PATH"
$env:MSBUILDDISABLENODEREUSE = '1'
function Stamp([string] $m) { Write-Output ("[{0}] {1}" -f (Get-Date -Format 'yyyy-MM-dd HH:mm:ss'), $m) }
$proj = Get-ChildItem -Path (Join-Path $Worktree 'src\tests\GolibTests') -Filter '*.csproj' -File | Select-Object -First 1
if (-not $proj) { Stamp "MISSING GolibTests csproj"; exit 1 }
Stamp ("GOLIBTESTS START head=" + ((& git.exe -C $Worktree rev-parse --short HEAD 2>$null) -join '') + " proj=" + $proj.Name)
$declaredRaw = (Get-ChildItem -Path $proj.DirectoryName -Filter '*.cs' -Recurse | Where-Object { $_.FullName -notmatch '\\(bin|obj)\\' } | Select-String -Pattern '\[TestMethod\]' | Measure-Object).Count
# The compile set is the denominator, not the source tree: GolibTests.csproj `Compile Remove`s the other flavors'
# test files under GoTargetOS-conditioned ItemGroups (measured 2026-09-02: 479 in source, 474 compiled on Windows).
# Evaluate each conditioned group for this host's default GoTargetOS ('' -> windows) and subtract the removed files' methods.
$csproj = [System.IO.File]::ReadAllText($proj.FullName)
$removedMethods = 0
foreach ($grp in [regex]::Matches($csproj, '(?s)<ItemGroup([^>]*)>(.*?)</ItemGroup>')) {
    $cond = ([regex]::Match($grp.Groups[1].Value, 'Condition="([^"]*)"')).Groups[1].Value
    $holds = $true
    if ($cond) {
        $expr = $cond -replace '\$\(GoTargetOS\)', '' -replace '\s+and\s+', ' -and ' -replace '\s+or\s+', ' -or ' -replace '!=', ' -ne ' -replace '==', ' -eq '
        try { $holds = [bool](Invoke-Expression $expr) } catch { Stamp "  cannot evaluate csproj condition [$cond] -- treating as holds"; $holds = $true }
    }
    if (-not $holds) { continue }
    foreach ($rm in [regex]::Matches($grp.Groups[2].Value, '<Compile Remove="([^"]+)"')) {
        $f = Join-Path $proj.DirectoryName $rm.Groups[1].Value
        if (Test-Path $f) { $n = (Select-String -Path $f -Pattern '\[TestMethod\]' | Measure-Object).Count; $removedMethods += $n; Stamp ("  compile-removed on this host: " + $rm.Groups[1].Value + " ($n methods)") }
    }
}
$declared = $declaredRaw - $removedMethods
Stamp "declared [TestMethod]: $declared (source $declaredRaw minus $removedMethods removed from this host's compile set)"
$t0 = Get-Date
$nb = if ($NoBuild) { '--no-build' } else { '' }
Stamp ("mode: " + $(if ($NoBuild) { '--no-build (assemblies from the preceding slnx leg)' } else { 'build+test' }))
$out = cmd /c "dotnet test `"$($proj.FullName)`" -c Debug $nb -m -p:UseSharedCompilation=false -p:go2csPath=$($Worktree.Replace('\','/'))/src/ 2>&1"
$code = $LASTEXITCODE
# DETAIL (2026-09-05): the runner's whole output is written beside the summary so a 'Failed: 1' has a NAME; every 'Failed <test>' line is stamped.
$detail = Join-Path $PSScriptRoot ('coord-golibtests-detail-' + (Get-Date -Format 'yyyyMMdd-HHmmss') + '.log'); $out | Out-File -FilePath $detail -Encoding utf8; Stamp ('  detail: ' + $detail)
$out | Select-String -Pattern '^\s+Failed [A-Za-z_]' | ForEach-Object { Stamp ('  FAILED TEST: ' + $_.Line.Trim()) }
$out | Select-String -Pattern 'Passed!|Failed!|Test Run Aborted|Total tests|error (CS|MSB|NETSDK)[0-9]+' | Select-Object -First 12 | ForEach-Object { Stamp ("  " + $_.Line.Trim()) }
# the verdict WORD is never the reading: an aborted run prints "Passed!" above "Test Run Aborted."; the run's Total must equal the compile-set count
$aborted = @($out | Select-String -Pattern 'Test Run Aborted').Count -gt 0
$totalLine = $out | Select-String -Pattern 'Total:\s+(\d+)' | Select-Object -First 1
$total = if ($totalLine) { [int]$totalLine.Matches[0].Groups[1].Value } else { -1 }
if ($aborted) { Stamp "TEST RUN ABORTED -- unmeasured suite, not a pass"; if ($code -eq 0) { $code = 1 } }
if ($total -ne $declared) { Stamp "COUNT MISMATCH: run Total $total != declared $declared -- unmeasured suite"; if ($code -eq 0) { $code = 1 } } else { Stamp "count-matched: $total / $declared" }
Stamp ("GOLIBTESTS END exit=$code elapsed=" + [int]((Get-Date) - $t0).TotalSeconds + "s")
exit $code
