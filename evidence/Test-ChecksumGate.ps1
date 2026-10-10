<#
.SYNOPSIS
    Parse-and-run test of OWNER-SHEET.md's checksum gate (block 2), against this kit's own packages.

.DESCRIPTION
    COORD found a false red live in block 2 of a sheet (2026-10-10): with a ONE-LINE SHA256SUMS, Get-Content returns a
    scalar string, so `(Get-Content X) -match ...` returns a BOOLEAN, `$sums[0]` is $true, `$want` becomes "True", and
    the gate throws "checksum mismatch" on a good package. This test takes the gate's lines out of the sheet as written
    (from `$file = ` to the "checksum mismatch" line), parses them, and runs them in a scratch $Pub against three
    SHA256SUMS files:
      1. the kit's own SHA256SUMS                       -> passes
      2. a ONE-LINE SHA256SUMS holding only that file    -> passes (the case that failed live)
      3. the one line with a wrong hash                  -> throws "checksum mismatch" (the gate still bites)
    $Id and $PV are the sheet's own: the last assignment of each before the gate. Exit code = the number of failed
    cases. Run with PowerShell 7 from the kit's root: pwsh -NoProfile -File evidence/Test-ChecksumGate.ps1
#>
$ErrorActionPreference = 'Stop'
$kit = Split-Path $PSScriptRoot
$sheet = Get-Content -Raw -LiteralPath (Join-Path $kit 'OWNER-SHEET.md')
$lines = @($sheet -split "`r?`n")
$start = [Array]::FindIndex([string[]]$lines, [Predicate[string]] { param($l) $l -match '^\$file = ' })
$end = if ($start -ge 0) { [Array]::FindIndex([string[]]$lines, $start, [Predicate[string]] { param($l) $l -match 'checksum mismatch' }) } else { -1 }
if ($start -lt 0 -or $end -le $start) { Write-Host 'FAIL  the sheet has no checksum gate to test'; exit 1 }
$gate = ($lines[$start..$end] | Where-Object { $_ -notmatch '^\$SignDir = |^New-Item |^Copy-Item ' }) -join "`n"
$gate = $gate.Replace('"$SignDir\$file"', '$packed')
$tokens = $null; $errors = $null
[void][System.Management.Automation.Language.Parser]::ParseInput($gate, [ref]$tokens, [ref]$errors)
$failed = 0
if ($errors.Count) { $failed++; Write-Host "FAIL  the gate does not parse: $($errors[0].Message)" } else { Write-Host '  ok    the gate parses' }
$before = $lines[0..$start]
$Id = ($before | Where-Object { $_ -match "^\`$Id\s*=\s*'" } | Select-Object -Last 1) -replace "^\`$Id\s*=\s*'([^']+)'.*", '$1'
$PV = ($before | Where-Object { $_ -match "^\`$PV\s*=\s*'" } | Select-Object -Last 1) -replace "^\`$PV\s*=\s*'([^']+)'.*", '$1'

$Pub = Join-Path ([IO.Path]::GetTempPath()) ("checksum-gate-$PID-" + [DateTime]::UtcNow.Ticks)
New-Item -ItemType Directory -Force (Join-Path $Pub 'kit') | Out-Null
Copy-Item -Recurse (Join-Path $kit 'nupkg') (Join-Path $Pub 'kit')
$sumsPath = Join-Path $Pub 'kit/nupkg/SHA256SUMS'
$all = @(Get-Content $sumsPath)
try {
    # The packed file the gate reads: its own $file, under the nupkg folder its SHA256SUMS line names.
    $file = Invoke-Expression ($lines[$start] -replace '^\$file = ', '')
    # The line the gate itself selects: its own -match pattern, evaluated with its own $file.
    $pattern = Invoke-Expression (($lines[$start..$end] | Where-Object { $_ -match '^\$sums = ' } | Select-Object -First 1) -replace '^.*-match ', '')
    $line = @($all | Where-Object { $_ -match $pattern })
    if ($line.Count -ne 1) { throw "the kit's SHA256SUMS has $($line.Count) line(s) for $file" }
    $packed = Join-Path (Join-Path $Pub 'kit/nupkg') (($line[0] -split '\s+', 2)[1])
    $cases = @(
        @('the kit''s own SHA256SUMS', $all, $null),
        @('a ONE-LINE SHA256SUMS (the live false red)', @($line[0]), $null),
        @('the one line with a wrong hash', @(('0' * 64) + '  ' + ($line[0] -split '\s+', 2)[1]), 'checksum mismatch'))
    foreach ($c in $cases) {
        [System.IO.File]::WriteAllText($sumsPath, (($c[1] -join "`n") + "`n"))
        $threw = $null
        try { & ([scriptblock]::Create($gate)) | Out-Null } catch { $threw = $_.Exception.Message }
        $ok = if ($c[2]) { $threw -and $threw.Contains($c[2]) } else { -not $threw }
        if ($ok) { Write-Host "  ok    $($c[0])$(if ($threw) { " -- threw: $threw" })" }
        else { $failed++; Write-Host "  FAIL  $($c[0]) -- $(if ($threw) { "threw: $threw" } else { 'did not throw' })" -ForegroundColor Red }
    }
}
finally { Remove-Item -Recurse -Force $Pub }
Write-Host "$Id $PV  failed $failed"
exit $failed
