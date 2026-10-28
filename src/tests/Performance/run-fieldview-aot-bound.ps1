<#
.SYNOPSIS
    Bounds PerfFieldView's Native AOT time against its JIT time, on one box in one session.
.DESCRIPTION
    golib's field-view cache matches a field's accessor against every view the box already holds
    (golib's Views.cs partial of the pointer type, sameAccessor). A match that reads Delegate.Method costs about 1 us per
    read under Native AOT, where the JIT caches it: with that match, PerfFieldView read about 95 s
    under AOT against 3.4 s under the JIT on R-LAPTOP (2026-10-28), where 1.24.13.7 read 5.35 s under
    AOT. This gate keeps that from coming back unnoticed.
    It builds PerfFieldView twice from this tree: the JIT arm (a Release build) and the AOT arm (a
    Native AOT publish, -p:PerfAot=true, as PerformanceRunner publishes it). It then runs both arms
    alternately, -Runs processes each, checks that every process prints the same checksum, and
    compares medians:
        workload time (the row's elapsed_ns)   red when AOT is above -Bound x JIT (default 3.0)
    A bound is a ratio between two arms measured here; never compare its numbers with a table from
    another machine. The AOT publish runs ILC over the whole closure and takes many minutes.
.PARAMETER Runs
    Processes per arm (default 5). The first process of each arm is a discarded warmup.
.PARAMETER Bound
    Largest accepted AOT/JIT ratio of median workload time.
.EXAMPLE
    ./run-fieldview-aot-bound.ps1
#>
[CmdletBinding()]
param(
    [int]    $Runs  = 5,
    [double] $Bound = 3.0
)

. (Join-Path $PSScriptRoot '../../_paths.ps1')

Set-StrictMode -Version Latest

# A native tool writing to stderr must not become a terminating error under 'Stop'.
$ErrorActionPreference = 'Continue'

# Windows PowerShell 5.1 is the only shell on the perf-canon host, so this script stays inside its
# language surface: no ternary, no Join-String, no Process.Kill(bool).

$row     = 'PerfFieldView'
$rowDir  = Join-Path $PSScriptRoot $row
$csproj  = Join-Path $rowDir "$row.csproj"
$jitOut  = Join-Path $rowDir "bin/Release/$NetVersion"
$armRoot = Join-Path $rowDir 'bin/aot-bound'
$srcRoot = (Resolve-Path (Join-Path $PSScriptRoot '../..')).Path

if ($Runs -lt 2) {
    Write-Host "-Runs must be at least 2: the first process of each arm is discarded." -ForegroundColor Red
    exit 2
}

# The Native AOT link step finds the C++ linker through vswhere.exe on PATH. A box without the Visual
# Studio installer directory on PATH fails at link with "'vswhere.exe' is not recognized".
if (-not (Get-Command 'vswhere.exe' -ErrorAction SilentlyContinue)) {
    $installer = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio\Installer'

    if (Test-Path -LiteralPath (Join-Path $installer 'vswhere.exe')) {
        $env:PATH = "$installer;$env:PATH"
    }
}

function Build-JitArm {
    Write-Host "==> building the JIT arm..." -ForegroundColor Cyan

    $buildArgs = @('build', $csproj, '-c', 'Release', '-nologo', '-clp:ErrorsOnly', "-p:go2csPath=$srcRoot/")
    # To the host, not the pipeline: anything a function writes to the pipeline joins its return value.
    & dotnet @buildArgs | Out-Host

    if ($LASTEXITCODE -ne 0) {
        Write-Host "*** the JIT arm did not build ($LASTEXITCODE) ***" -ForegroundColor Red
        exit 1
    }

    # The row's csproj ignores -o, so the arm is copied out of the one output directory it writes.
    $armDir = Join-Path $armRoot 'jit'

    if (Test-Path -LiteralPath $armDir) { Remove-Item -LiteralPath $armDir -Recurse -Force }

    New-Item -ItemType Directory -Path $armDir -Force | Out-Null
    Copy-Item -Path (Join-Path $jitOut '*') -Destination $armDir -Recurse -Force

    $dll = Join-Path $armDir "$row.dll"

    if (-not (Test-Path -LiteralPath $dll)) {
        Write-Host "*** the JIT arm built, but there is no $dll ***" -ForegroundColor Red
        exit 1
    }

    return $dll
}

function Build-AotArm {
    Write-Host "==> publishing the Native AOT arm (ILC over the whole closure; many minutes)..." -ForegroundColor Cyan

    $armDir = Join-Path $armRoot 'aot'

    if (Test-Path -LiteralPath $armDir) { Remove-Item -LiteralPath $armDir -Recurse -Force }

    $publishArgs = @('publish', $csproj, '-c', 'Release', '-nologo', '-clp:ErrorsOnly', '-p:PerfAot=true',
        "-p:go2csPath=$srcRoot/", '-o', $armDir)
    & dotnet @publishArgs | Out-Host

    if ($LASTEXITCODE -ne 0) {
        Write-Host "*** the AOT arm did not publish ($LASTEXITCODE) ***" -ForegroundColor Red
        exit 1
    }

    $exe = Join-Path $armDir "$row.exe"

    if (-not (Test-Path -LiteralPath $exe)) {
        Write-Host "*** the AOT arm published, but there is no $exe ***" -ForegroundColor Red
        exit 1
    }

    return $exe
}

# One process: its workload time (elapsed_ns) and its checksum line.
function Measure-Run([string]$fileName, [string]$arguments) {
    $psi = New-Object System.Diagnostics.ProcessStartInfo
    $psi.FileName = $fileName
    $psi.Arguments = $arguments
    $psi.UseShellExecute = $false
    $psi.RedirectStandardOutput = $true
    $psi.RedirectStandardError = $true

    $process = [System.Diagnostics.Process]::Start($psi)
    $stdout = $process.StandardOutput.ReadToEndAsync()
    $stderr = $process.StandardError.ReadToEndAsync()
    $process.WaitForExit()
    $text = $stdout.Result
    [void]$stderr.Result

    if ($process.ExitCode -ne 0) {
        Write-Host "*** $fileName $arguments exited $($process.ExitCode) ***" -ForegroundColor Red
        exit 1
    }

    $ns = [regex]::Match($text, '(?m)^elapsed_ns:\s*(\d+)')
    $sum = [regex]::Match($text, '(?m)^checksum:\s*(\d+)')

    if (-not $ns.Success -or -not $sum.Success) {
        Write-Host "*** $fileName printed no elapsed_ns or checksum line ***" -ForegroundColor Red
        exit 1
    }

    return [pscustomobject]@{ Ns = [double]$ns.Groups[1].Value; Sum = $sum.Groups[1].Value }
}

function Get-Median([double[]]$values) {
    $sorted = $values | Sort-Object
    $n = $sorted.Count

    if ($n % 2 -eq 1) { return $sorted[($n - 1) / 2] }

    return ($sorted[$n / 2 - 1] + $sorted[$n / 2]) / 2
}

$jitDll = Build-JitArm
$aotExe = Build-AotArm

$samples = @{ jit = @(); aot = @() }
$checksum = $null

for ($i = 0; $i -lt $Runs; $i++) {
    # Alternate which arm goes first, so neither always runs on a warmer machine.
    $order = @('jit', 'aot')
    if ($i % 2 -eq 1) { $order = @('aot', 'jit') }

    foreach ($arm in $order) {
        if ($arm -eq 'jit') { $sample = Measure-Run 'dotnet' ('"' + $jitDll + '"') }
        else { $sample = Measure-Run $aotExe '' }

        if ($null -eq $checksum) { $checksum = $sample.Sum }

        if ($sample.Sum -ne $checksum) {
            Write-Host "FAIL: the $arm arm printed checksum $($sample.Sum), another run printed $checksum" -ForegroundColor Red
            exit 1
        }

        Write-Host ("  run {0} {1,-4} elapsed {2,16:N0} ns" -f $i, $arm, $sample.Ns)

        if ($i -gt 0) { $samples[$arm] += $sample }
    }
}

$jitNs = Get-Median ($samples.jit | ForEach-Object { $_.Ns })
$aotNs = Get-Median ($samples.aot | ForEach-Object { $_.Ns })
$ratio = $aotNs / $jitNs

Write-Host ""
Write-Host ("time: JIT {0,16:N0} ns   AOT {1,16:N0} ns   ratio {2:N2}  (bound {3:N2}; checksum {4})" -f $jitNs, $aotNs, $ratio, $Bound, $checksum)

if ($ratio -gt $Bound) {
    Write-Host "FAIL: PerfFieldView under Native AOT is $('{0:N1}' -f $ratio)x its JIT time" -ForegroundColor Red
    exit 1
}

Write-Host "PASS" -ForegroundColor Green
exit 0
