<#
.SYNOPSIS
    Bounds what the interface-shell cache costs a program that never re-asserts a value.

.DESCRIPTION
    AdapterBinder keeps the last runtime shell each shell factory built, so a re-assert of the same
    dynamic value reuses it (docs/phase4/DESIGN-iface-shell-caching.md, section 12). A cache can only
    lose on the opposite shape, a new value per assert, and the first cache shipped there lost badly:
    about 9x the time and 3.5x the working set, measured on the PerfIfaceShellChurn row. This gate
    keeps that from coming back unnoticed.

    It builds PerfIfaceShellChurn twice from this tree: the CONTROL arm with the cache compiled out
    (golib.csproj's Go2csNoShellCache) and the TREE arm as it stands. Only golib differs between the
    two. It then runs both arms alternately, -Runs processes each, and compares medians:

        workload time (the row's elapsed_ns)   red above -NsBound x the control (default 1.15)
        peak working set (polled while alive)  red above -WsBound x the control (default 1.25)

    Both arms run on this box in one session, in alternating order, so the ratio carries no host
    or warmth difference. A bound is a ratio between two arms measured here; never compare its
    numbers with a table from another machine.

    The control build replaces golib's compiled output; the tree arm is built last, so the tree's
    bin/ ends the run holding the tree's own golib.

.PARAMETER Runs
    Processes per arm (default 7). The first process of each arm is a discarded warmup.

.PARAMETER NsBound
    Largest accepted tree/control ratio of median workload time.

.PARAMETER WsBound
    Largest accepted tree/control ratio of median peak working set.

.EXAMPLE
    ./run-shell-churn-bound.ps1
#>
[CmdletBinding()]
param(
    [int]    $Runs    = 7,
    [double] $NsBound = 1.15,
    [double] $WsBound = 1.25
)

. (Join-Path $PSScriptRoot '../../_paths.ps1')

Set-StrictMode -Version Latest

# A native tool writing to stderr must not become a terminating error under 'Stop'.
$ErrorActionPreference = 'Continue'

# Windows PowerShell 5.1 is the only shell on the perf-canon host, so this script stays inside its
# language surface: no ternary, no Join-String, no Process.Kill(bool).

$row     = 'PerfIfaceShellChurn'
$rowDir  = Join-Path $PSScriptRoot $row
$csproj  = Join-Path $rowDir "$row.csproj"
$outDir  = Join-Path $rowDir "bin/Release/$NetVersion"
$armRoot = Join-Path $rowDir 'bin/churn-bound'
$srcRoot = (Resolve-Path (Join-Path $PSScriptRoot '../..')).Path

if ($Runs -lt 2) {
    Write-Host "-Runs must be at least 2: the first process of each arm is discarded." -ForegroundColor Red
    exit 2
}

function Build-Arm([string]$name, [string[]]$extra) {
    Write-Host "==> building the $name arm..." -ForegroundColor Cyan

    $buildArgs = @('build', $csproj, '-c', 'Release', '-nologo', '-clp:ErrorsOnly', "-p:go2csPath=$srcRoot/") + $extra
    # To the host, not the pipeline: anything a function writes to the pipeline joins its return value.
    & dotnet @buildArgs | Out-Host

    if ($LASTEXITCODE -ne 0) {
        Write-Host "*** the $name arm did not build ($LASTEXITCODE) ***" -ForegroundColor Red
        exit 1
    }

    $dll = Join-Path $outDir "$row.dll"

    if (-not (Test-Path -LiteralPath $dll)) {
        Write-Host "*** the $name arm built, but there is no $dll ***" -ForegroundColor Red
        exit 1
    }

    # The row's csproj ignores -o, so each arm is copied out of the one output directory it writes.
    $armDir = Join-Path $armRoot $name

    if (Test-Path -LiteralPath $armDir) { Remove-Item -LiteralPath $armDir -Recurse -Force }

    New-Item -ItemType Directory -Path $armDir -Force | Out-Null
    Copy-Item -Path (Join-Path $outDir '*') -Destination $armDir -Recurse -Force

    return (Join-Path $armDir "$row.dll")
}

# One process: its workload time (elapsed_ns) and its peak working set, polled while it is alive
# exactly as PerformanceRunner polls it.
function Measure-Run([string]$dll) {
    $psi = New-Object System.Diagnostics.ProcessStartInfo
    $psi.FileName = 'dotnet'
    $psi.Arguments = '"' + $dll + '"'
    $psi.UseShellExecute = $false
    $psi.RedirectStandardOutput = $true
    $psi.RedirectStandardError = $true

    $process = [System.Diagnostics.Process]::Start($psi)
    $stdout = $process.StandardOutput.ReadToEndAsync()
    $stderr = $process.StandardError.ReadToEndAsync()
    $peak = 0

    while (-not $process.WaitForExit(1)) {
        try {
            $process.Refresh()
            if ($process.PeakWorkingSet64 -gt $peak) { $peak = $process.PeakWorkingSet64 }
        }
        catch {
            # Exited between the wait and the query.
        }
    }

    $process.WaitForExit()
    $text = $stdout.Result
    [void]$stderr.Result

    if ($process.ExitCode -ne 0) {
        Write-Host "*** $dll exited $($process.ExitCode) ***" -ForegroundColor Red
        exit 1
    }

    $match = [regex]::Match($text, '(?m)^elapsed_ns:\s*(\d+)')

    if (-not $match.Success) {
        Write-Host "*** $dll printed no elapsed_ns line ***" -ForegroundColor Red
        exit 1
    }

    return [pscustomobject]@{ Ns = [double]$match.Groups[1].Value; Ws = [double]$peak }
}

function Get-Median([double[]]$values) {
    $sorted = $values | Sort-Object
    $n = $sorted.Count

    if ($n % 2 -eq 1) { return $sorted[($n - 1) / 2] }

    return ($sorted[$n / 2 - 1] + $sorted[$n / 2]) / 2
}

$controlDll = Build-Arm 'control' @('-p:Go2csNoShellCache=true')
$treeDll    = Build-Arm 'tree' @()

$samples = @{ control = @(); tree = @() }

for ($i = 0; $i -lt $Runs; $i++) {
    # Alternate which arm goes first, so neither always runs on a warmer machine.
    $order = @('control', 'tree')
    if ($i % 2 -eq 1) { $order = @('tree', 'control') }

    foreach ($arm in $order) {
        $dll = $controlDll
        if ($arm -eq 'tree') { $dll = $treeDll }

        $sample = Measure-Run $dll
        Write-Host ("  run {0} {1,-7} elapsed {2,12:N0} ns  peak {3,8:N1} MB" -f $i, $arm, $sample.Ns, ($sample.Ws / 1MB))

        if ($i -gt 0) { $samples[$arm] += $sample }
    }
}

$controlNs = Get-Median ($samples.control | ForEach-Object { $_.Ns })
$treeNs    = Get-Median ($samples.tree    | ForEach-Object { $_.Ns })
$controlWs = Get-Median ($samples.control | ForEach-Object { $_.Ws })
$treeWs    = Get-Median ($samples.tree    | ForEach-Object { $_.Ws })

$nsRatio = $treeNs / $controlNs
$wsRatio = $treeWs / $controlWs

Write-Host ""
Write-Host ("time:        control {0,12:N0} ns   tree {1,12:N0} ns   ratio {2:N3}  (bound {3:N2})" -f $controlNs, $treeNs, $nsRatio, $NsBound)
Write-Host ("working set: control {0,8:N1} MB   tree {1,8:N1} MB   ratio {2:N3}  (bound {3:N2})" -f ($controlWs / 1MB), ($treeWs / 1MB), $wsRatio, $WsBound)

$failed = $false

if ($nsRatio -gt $NsBound) {
    Write-Host "FAIL: the shell cache makes a new value per assert $('{0:N1}' -f (($nsRatio - 1) * 100))% slower than no cache" -ForegroundColor Red
    $failed = $true
}

if ($wsRatio -gt $WsBound) {
    Write-Host "FAIL: the shell cache grows the working set $('{0:N1}' -f (($wsRatio - 1) * 100))% over no cache" -ForegroundColor Red
    $failed = $true
}

if ($failed) { exit 1 }

Write-Host "PASS" -ForegroundColor Green
exit 0
