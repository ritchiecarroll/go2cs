# Train 33 canary battery: the five reflect-IMPORTER canaries, recomputed 2026-09-07.
# Toolchain pin ABORTS on mismatch (it does not merely print).
# -TestConfig is deliberately NOT passed: the post-flip default is Release+TC0 and an
# EXPLICIT flag forces uniformity, superseding per-row annotations and voiding bank eligibility.
$ErrorActionPreference = 'Continue'
$env:DOTNET_ROOT = '$env:USERPROFILE\dotnet10'
$env:GOROOT      = '$env:USERPROFILE\sdk\go1.18'
$env:PATH        = "$env:GOROOT\bin;$env:DOTNET_ROOT;$env:PATH"
$env:MSBUILDDISABLENODEREUSE = '1'

$want = 'go1.23.12'
$got  = (& go version) 2>&1 | Out-String
if ($got -notmatch [regex]::Escape($want)) {
    Write-Host "ABORT: toolchain is not $want -- got: $($got.Trim())"
    exit 90
}
# The version string alone is NOT enough: if $env:GOROOTin holds no go.exe, PATH falls
# THROUGH to an ambient toolchain and the version check passes describing a different
# install. Assert the resolved binary IS the pinned root's.
$resolved = (Get-Command go -ErrorAction SilentlyContinue).Source
$rootOK   = ((& go env GOROOT) 2>&1 | Out-String).Trim()
if ($resolved -notlike "$env:GOROOT*" -or $rootOK -ne $env:GOROOT) {
    Write-Host "ABORT: go resolves to '$resolved' (GOROOT reports '$rootOK'), not the pinned $env:GOROOT"
    exit 91
}
Write-Host "PIN OK: $($got.Trim())  [resolved from the pinned root]"
Write-Host "SDK   : $((& dotnet --version) 2>&1)"

$wt = 'C:\Projects\go2cs\.claude\worktrees\musing-moser-d4552c'
Set-Location $wt
Write-Host "TREE  : $(git rev-parse --short HEAD) on $(git rev-parse --abbrev-ref HEAD)"

# canary set, recomputed at gate time with a strict import-SPEC predicate
$canaries = @('crypto/tls','net/http','os','go/types','encoding/json')
$fail = 0
$summary = @()
foreach ($p in $canaries) {
    Write-Host ""
    Write-Host "=== CANARY $p ============================================"
    $t0 = Get-Date
    # NO Tee here: Start-Process already redirects this stream, and a Tee onto the same
    # path throws FileOpenFailure, aborts the pipeline, and leaves $LASTEXITCODE as
    # TEE'S status -- which reports a leg that never ran as exit=0. Per-leg output goes
    # to its OWN file; the real exit is captured BEFORE anything else can overwrite it.
    $leg = $env:CANLOG + "." + $p.Replace("/","_")
    & "$wt\src\run-validated-sweep.ps1" -Filter $p -Exact *> $leg
    $rc = $LASTEXITCODE
    $el = [int]((Get-Date) - $t0).TotalSeconds

    # A leg is MEASURED only if the sweep actually emitted a verdict line. An exit code
    # alone cannot distinguish "passed" from "never ran".
    $txt = if (Test-Path $leg) { Get-Content $leg -Raw } else { '' }
    $measured = $txt -match '(?m)^\s*(PASS|FAIL|NO REGRESSION|\d+\s*/\s*\d+)'
    $verdict = if (-not $measured) { 'NOT MEASURED' } elseif ($rc -ne 0) { 'FAIL' } else { 'PASS' }
    if ($verdict -ne 'PASS') { $fail++ }

    Write-Host "=== $p  $verdict  exit=$rc  ${el}s  log=$leg"
    $summary += "  {0,-16} {1,-12} exit={2,-4} {3}s" -f $p, $verdict, $rc, $el
}
Write-Host ""
Write-Host "CANARY BATTERY: $($canaries.Count) legs, $fail not-PASS"
$summary | ForEach-Object { Write-Host $_ }
exit $fail
