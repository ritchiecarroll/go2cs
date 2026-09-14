$ErrorActionPreference = 'Continue'
$env:DOTNET_ROOT = '$env:USERPROFILE\dotnet10'
$env:GOROOT      = '$env:USERPROFILE\sdk\go1.23.12'
$env:PATH        = "$env:GOROOT\bin;$env:DOTNET_ROOT;$env:PATH"
$env:MSBUILDDISABLENODEREUSE = '1'

$want = 'go1.23.12'
$got  = (& go version) 2>&1 | Out-String
if ($got -notmatch [regex]::Escape($want)) { Write-Host "ABORT: toolchain is not $want -- $($got.Trim())"; exit 90 }
$resolved = (Get-Command go -ErrorAction SilentlyContinue).Source
$rootOK   = ((& go env GOROOT) 2>&1 | Out-String).Trim()
if ($resolved -notlike "$env:GOROOT*" -or $rootOK -ne $env:GOROOT) {
    Write-Host "ABORT: go resolves to '$resolved' (GOROOT '$rootOK'), not the pinned $env:GOROOT"; exit 91 }
Write-Host "PIN OK: $($got.Trim())  [resolved from the pinned root]"

$wt = 'C:\Projects\go2cs\.claude\worktrees\musing-moser-d4552c'
Set-Location $wt
Write-Host "TREE  : $(git rev-parse --short HEAD) on $(git rev-parse --abbrev-ref HEAD)"
Write-Host ""

$fail = 0
$summary = @()
function Leg($name, $verdictPattern, $block) {
    Write-Host "=== LEG $name ============================================"
    $t0 = Get-Date
    $log = $env:UNIONLOG + "." + $name
    & $block *> $log
    $rc = $LASTEXITCODE
    $el = [int]((Get-Date) - $t0).TotalSeconds
    $txt = if (Test-Path $log) { Get-Content $log -Raw } else { '' }
    $measured = $txt -match $verdictPattern
    $verdict = if (-not $measured) { 'NOT MEASURED' } elseif ($rc -ne 0) { 'FAIL' } else { 'PASS' }
    Write-Host "=== $name  $verdict  exit=$rc  ${el}s"
    $script:summary += "  {0,-22} {1,-12} exit={2,-4} {3}s" -f $name, $verdict, $rc, $el
    if ($verdict -ne 'PASS') { $script:fail++ }
}

Leg 'solution-integrity' '(?m)(0 cycles|registered|OK|PASS)' { & "$wt\src\tests\Behavioral\check-solution-integrity.ps1" }
Leg 'cnr'                '(?m)(BYTE-IDENTICAL|NO REGRESSION|byte-identical|NOT MEASURED|DRIFT)' { & "$wt\src\tests\Behavioral\check-no-regression.ps1" }
Leg 'stdlib-build'       '(?m)(Build succeeded|error|Warning\(s\))' { & dotnet build "$wt\src\go2cs-stdlib.slnx" -c Debug --no-incremental -m -p:UseSharedCompilation=false -p:GoTargetOS=windows }

Write-Host ""
Write-Host "UNION BATTERY: $fail not-PASS"
$summary | ForEach-Object { Write-Host $_ }
exit $fail
