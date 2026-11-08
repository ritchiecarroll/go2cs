# .3 -tests regen (COORD 03:25Z): for EVERY roster row (read through _roster.ps1's reader, never a hand list), the
# convert-only action `go2cs -tests -test-action convert`, in place on the worktree at master (the seeded tree; production
# is a fixed point, so the committed tree IS E(final)). Serial, one conversion at a time. Nothing runs a test here.
param([int] $Limit = 0)
$ErrorActionPreference = 'Continue'
$repo = 'C:\go2cs-tmp\r-testsregen'
$src = Join-Path $repo 'src'
$log = 'C:\go2cs-tmp\r-kit\testsregen'
New-Item -ItemType Directory -Force -Path $log | Out-Null
. (Join-Path $src '_roster.ps1')
$rows = @(Get-ValidatedRosterRows -Path (Join-Path $repo 'docs\ValidatedTestPackages.md'))
$goroot = (& go env GOROOT).Trim()
$exe = Join-Path $src 'go2cs\bin\go2cs.exe'
"rows=$($rows.Count) goroot=$goroot exe=$exe head=$(git -C $repo rev-parse HEAD) start=$((Get-Date).ToUniversalTime().ToString('s'))Z"
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
"done rows=$i failed=$fail end=$((Get-Date).ToUniversalTime().ToString('s'))Z"
