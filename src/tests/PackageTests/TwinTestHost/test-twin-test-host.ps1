<#
.SYNOPSIS
    The test host's twin path, end to end (docs/phase4/DESIGN-mn-test-twin.md): a converted test package built with
    twins and run with GO2CS_MN=1 runs a parking test as its twin, multiplexed.

.DESCRIPTION
    Converts this fixture with `go2cs -tests`, builds its test host with twins (GoTwinsFor names the production and the
    test project), and runs the published host once per test, reading the scheduler's READING line from stderr
    (GO2CS_MN_READING=1). With no twin path the scheduler never starts and no READING line is printed: the row reads
    that as zero parks and says so.

    Arms (each must hold):
      A  TestParks, GO2CS_MN=1         passes, and parks > 0 (it ran as its twin)
      B  TestParks, GO2CS_MN unset     passes, and the scheduler never starts (the default path is untouched)
      C  TestPlainOnly, GO2CS_MN=1     passes, and the scheduler never starts (no twin: it runs plain)
      D  TestFatalInTwin, GO2CS_MN=1   FAILS as a test ("twin fatal"), not as an infrastructure error
      E  TestFatalFromGoroutine, MN=1  the host refuses it: "called from a goroutine other than the test goroutine"
      F  TestParallelParks, MN=1       passes; its parks and sensed counts are reported (the t.Parallel gate)

.PARAMETER Go2cs
    The converter to use. Built from src/go2cs into a temporary directory when omitted.
#>
param(
    [string] $Go2cs = ''
)

$ErrorActionPreference = 'Stop'
$fixture = $PSScriptRoot
$src = (Resolve-Path (Join-Path $PSScriptRoot '..\..\..')).Path

if ($Go2cs -eq '') {
    $Go2cs = Join-Path ([System.IO.Path]::GetTempPath()) "go2cs-twin-test-host-$PID.exe"
    Push-Location (Join-Path $src 'go2cs')
    try {
        & go build -o $Go2cs .
        if ($LASTEXITCODE -ne 0) { throw "converter build failed ($LASTEXITCODE)" }
    }
    finally { Pop-Location }
}

& $Go2cs -tests -test-action convert -go2cspath $src $fixture | Out-Null
if ($LASTEXITCODE -ne 0) { throw "go2cs -tests convert failed ($LASTEXITCODE)" }

$testProject = Get-ChildItem -LiteralPath $fixture -Filter '*.tests.csproj' | Select-Object -First 1
$productionProject = $testProject.BaseName -replace '\.tests$', ''

$previousTwinsFor = $env:GoTwinsFor
$env:GoTwinsFor = "$productionProject;$($testProject.BaseName)"
try {
    & $Go2cs -tests -test-action build -go2cspath $src $fixture | Out-Null
    if ($LASTEXITCODE -ne 0) { throw "go2cs -tests build (twinned) failed ($LASTEXITCODE)" }
}
finally { $env:GoTwinsFor = $previousTwinsFor }

$hostExe = Join-Path $fixture "bin\tests\publish\$($testProject.BaseName).exe"
if (-not (Test-Path -LiteralPath $hostExe)) { throw "the published test host is missing: $hostExe" }

function Invoke-Host([string] $test, [bool] $multiplexed) {
    $saved = @{ MN = $env:GO2CS_MN; READING = $env:GO2CS_MN_READING; ROOT = $env:GO2CS_MODULE_ROOT }
    $env:GO2CS_MN = if ($multiplexed) { '1' } else { $null }
    $env:GO2CS_MN_READING = '1'
    $env:GO2CS_MODULE_ROOT = $fixture
    $outFile = [System.IO.Path]::GetTempFileName()
    $errFile = [System.IO.Path]::GetTempFileName()
    try {
        $process = Start-Process -FilePath $hostExe -ArgumentList @("-test.run=^$test`$", '-test.v') -WorkingDirectory $fixture `
            -NoNewWindow -Wait -PassThru -RedirectStandardOutput $outFile -RedirectStandardError $errFile
        $out = Get-Content -LiteralPath $outFile -Raw
        $err = Get-Content -LiteralPath $errFile -Raw
    }
    finally {
        $env:GO2CS_MN = $saved.MN; $env:GO2CS_MN_READING = $saved.READING; $env:GO2CS_MODULE_ROOT = $saved.ROOT
        Remove-Item -LiteralPath $outFile, $errFile -ErrorAction SilentlyContinue
    }

    $reading = if ($err -match '(?m)^READING go-scheduler .*$') { $Matches[0] } else { $null }
    $parks = if ($reading -and $reading -match '\bparks=(\d+)') { [long]$Matches[1] } else { 0 }
    # sensed= arrives with N5's step 3a (a managed wait retakes its P); a READING without it reports n/a, never 0.
    $sensed = if ($reading -and $reading -match '\bsensed=(\d+)') { $Matches[1] } else { 'n/a' }

    [pscustomobject]@{
        Test = $test; Multiplexed = $multiplexed; ExitCode = $process.ExitCode
        Output = "$out`n$err"; Reading = $reading; Parks = $parks; Sensed = $sensed
        Started = $null -ne $reading
    }
}

$failures = 0
function Assert-Arm([string] $arm, [bool] $holds, [string] $what, $result) {
    $state = if ($result.Started) { "parks=$($result.Parks) sensed=$($result.Sensed)" } else { 'scheduler never started (parks 0)' }
    if ($holds) {
        Write-Host "PASS $arm $what [$state]"
    }
    else {
        Write-Host "FAIL $arm $what [$state, exit $($result.ExitCode)]"
        Write-Host ($result.Output.Trim() -split "`n" | Select-Object -Last 12 | Out-String)
        $script:failures++
    }
}

$a = Invoke-Host 'TestParks' $true
Assert-Arm 'A' ($a.ExitCode -eq 0 -and $a.Output -match '--- PASS: TestParks' -and $a.Parks -gt 0) 'TestParks under GO2CS_MN=1 runs as its twin' $a

$b = Invoke-Host 'TestParks' $false
Assert-Arm 'B' ($b.ExitCode -eq 0 -and $b.Output -match '--- PASS: TestParks' -and -not $b.Started) 'TestParks with GO2CS_MN unset stays plain' $b

$c = Invoke-Host 'TestPlainOnly' $true
Assert-Arm 'C' ($c.ExitCode -eq 0 -and $c.Output -match '--- PASS: TestPlainOnly' -and -not $c.Started) 'TestPlainOnly (no twin) runs plain under GO2CS_MN=1' $c

$d = Invoke-Host 'TestFatalInTwin' $true
Assert-Arm 'D' ($d.ExitCode -ne 0 -and $d.Output -match '--- FAIL: TestFatalInTwin' -and $d.Output -match 'twin fatal' -and $d.Output -notmatch 'other than the test goroutine') 't.Fatal inside the twin fails the test, not the host' $d

$e = Invoke-Host 'TestFatalFromGoroutine' $true
Assert-Arm 'E' ($e.ExitCode -ne 0 -and $e.Output -match 'called from a goroutine other than the test goroutine') 't.Fatal from a started goroutine is still refused' $e

$f = Invoke-Host 'TestParallelParks' $true
Assert-Arm 'F' ($f.ExitCode -eq 0 -and $f.Output -match '--- PASS: TestParallelParks' -and $f.Parks -gt 0) 'TestParallelParks under GO2CS_MN=1 runs as its twin' $f

if ($failures -gt 0) {
    Write-Host "TWIN TEST HOST: $failures arm(s) failed"
    exit 1
}

Write-Host 'TWIN TEST HOST: every arm holds'
exit 0
