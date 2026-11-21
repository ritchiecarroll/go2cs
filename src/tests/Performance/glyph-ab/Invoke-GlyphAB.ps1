<#
.SYNOPSIS
    SCRATCH (never for master): the global ᴍ cost test's runtime-performance A/B kit (COORD 20261120T202904Z item 3).

.DESCRIPTION
    Times two checkouts of go2cs on ONE box, interleaved: -Base (master) and -Glyph (claude/c1-scratch-glyph-global).
    Each checkout converts the benchmark programs with ITS OWN converter and builds them against ITS OWN src/core, so the
    only difference between the arms is the standard library's method form. The benchmark programs are package main,
    which the glyph converter's default (libraries only, the owner's steer of 11-21) leaves in today's form.

    Three legs, each interleaved arm by arm (A B, then B A on the next round), so drift on the box lands on both arms:

      1. HEADLINE  each checkout's own run-performance.ps1: the JIT and Native AOT columns, Release, as published.
                   Round 1 runs every phase; later rounds run --phase measure against the round-1 builds.
      2. DEBUG     the uninlined-forwarder worst case: -DebugBenchmarks built -c Debug (the go.* references build
                   Debug too, so no forwarder is inlined), each run -Runs times per arm, workload time (elapsed_ns).
      3. R2R       ReadyToRun with the standard library in its own version bubbles: the converted project's own
                   publish (PublishReadyToRun, not composite; the user assembly excluded as the project file sets),
                   so a call from one go.* assembly into another's forwarder stays a call until tier 1. Wall time
                   (start-up) and workload time, -Runs times per arm.

    A/A first (COORD): run once with -Glyph pointing at a SECOND checkout of the same master SHA. The A/A deltas are the
    noise floor the A/B deltas are read against. The kit refuses a run whose two arms are the same directory.

.PARAMETER Base
    Root of the master checkout (the directory holding src/).
.PARAMETER Glyph
    Root of the glyph checkout, or of a second master checkout for the A/A pass.
.PARAMETER Out
    A NEW directory for the results. Refused if it exists.
.PARAMETER Rounds
    Headline rounds. Default 3: round 1 builds, rounds 2..N re-measure, alternating arm order.
.PARAMETER Runs
    Timed runs per arm per benchmark for the Debug and R2R legs. Default 11, after 1 discarded warm-up per arm.
.PARAMETER NoAot
    Skip the Native AOT column in the headline leg (much faster; the AOT column is then NOT MEASURED, and says so).
.PARAMETER Legs
    Any of Headline, Debug, R2R. Default all three.

.EXAMPLE
    ./Invoke-GlyphAB.ps1 -Base C:\g2cs\master -Glyph C:\g2cs\master2 -Out C:\g2cs\ab-AA      # A/A first
    ./Invoke-GlyphAB.ps1 -Base C:\g2cs\master -Glyph C:\g2cs\glyph   -Out C:\g2cs\ab-AB      # then A/B
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory)] [string] $Base,
    [Parameter(Mandatory)] [string] $Glyph,
    [Parameter(Mandatory)] [string] $Out,
    [int] $Rounds = 3,
    [int] $Runs = 11,
    [switch] $NoAot,
    [ValidateSet('Headline', 'Debug', 'R2R')] [string[]] $Legs = @('Headline', 'Debug', 'R2R'),
    [string[]] $DebugBenchmarks = @('PerfSort', 'PerfStringMatch', 'PerfTlsHandshake'),
    [string[]] $R2RBenchmarks = @('PerfStartup', 'PerfSort', 'PerfStringMatch', 'PerfIface', 'PerfTlsHandshake')
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

# ---- preflight: every refusal names what it found ----

$Base = (Resolve-Path $Base).Path.TrimEnd('\', '/')
$Glyph = (Resolve-Path $Glyph).Path.TrimEnd('\', '/')

if ($Base -eq $Glyph) { throw "REFUSED: -Base and -Glyph are one directory ($Base). For A/A use a SECOND checkout of the same SHA." }
if (Test-Path $Out) { throw "REFUSED: -Out exists ($Out). Each run writes into a new directory." }
if ($env:GO2CS_GLYPH_METHODS) { throw "REFUSED: GO2CS_GLYPH_METHODS is set ('$($env:GO2CS_GLYPH_METHODS)'). The kit measures the converter's default (libraries only)." }

$rid = if ($IsWindows) { 'win-x64' } elseif ($IsMacOS) { if ([Runtime.InteropServices.RuntimeInformation]::OSArchitecture -eq 'Arm64') { 'osx-arm64' } else { 'osx-x64' } } else { if ([Runtime.InteropServices.RuntimeInformation]::OSArchitecture -eq 'Arm64') { 'linux-arm64' } else { 'linux-x64' } }
$exeSuffix = if ($IsWindows) { '.exe' } else { '' }

$arms = [ordered]@{ base = $Base; glyph = $Glyph }
$record = [ordered]@{}

foreach ($name in $arms.Keys) {
    $root = $arms[$name]
    if (-not (Test-Path (Join-Path $root 'src/tests/Performance/run-performance.ps1'))) { throw "REFUSED: $name ($root) holds no src/tests/Performance/run-performance.ps1." }
    $sha = (git -C $root rev-parse HEAD).Trim()
    # The headline leg re-transpiles the benchmark programs into their TRACKED .cs files, so a modification under
    # src/tests/Performance is the kit's own output. Anything else modified means the arm is not its commit.
    $dirty = @(git -C $root status --porcelain --untracked-files=no | Where-Object { $_ -notmatch '^.. src/tests/Performance/' })
    if ($dirty.Count -gt 0) { throw "REFUSED: $name ($root) has modified tracked file(s) outside src/tests/Performance: $($dirty -join '; ')" }
    $record[$name] = $sha
}

$sdk = (dotnet --version).Trim()
if (-not $sdk.StartsWith('10.')) { throw "REFUSED: dotnet --version is $sdk; the corpus builds with the 10.0 band (global.json's latestMajor takes the highest SDK installed: pin it for this shell)." }

New-Item -ItemType Directory -Path $Out | Out-Null
$aa = $record.base -eq $record.glyph
$header = @(
    "# glyph A/B kit run, $(Get-Date -Format o)",
    "base  $($record.base)  $Base",
    "glyph $($record.glyph)  $Glyph",
    "mode  $(if ($aa) { 'A/A (one SHA in both arms)' } else { 'A/B' })",
    "host  $([Environment]::MachineName)  $([Runtime.InteropServices.RuntimeInformation]::OSDescription)  $rid  cores=$([Environment]::ProcessorCount)",
    "sdk   $sdk",
    "legs  $($Legs -join ',')  rounds=$Rounds  runs=$Runs  aot=$(if ($NoAot) { 'NOT MEASURED (-NoAot)' } else { 'yes' })"
)
$header | Set-Content (Join-Path $Out 'run.txt') -Encoding utf8
$header | Write-Host

# ---- helpers ----

function Get-ElapsedNs([string[]] $lines) {
    foreach ($line in $lines) {
        if ($line -match '^elapsed_ns:\s*(\d+)') { return [long]$Matches[1] }
    }
    return $null
}

function Invoke-Timed([string] $exe, [string] $workDir) {
    $sw = [Diagnostics.Stopwatch]::StartNew()
    $output = & $exe 2>&1 | ForEach-Object { "$_" }
    $sw.Stop()
    if ($LASTEXITCODE -ne 0) { throw "$exe exited $LASTEXITCODE" }
    [pscustomobject]@{ WallMs = $sw.Elapsed.TotalMilliseconds; ElapsedNs = (Get-ElapsedNs $output); Checksum = ($output | Where-Object { $_ -notmatch '^elapsed_ns:' }) -join "`n" }
}

function Get-Median([double[]] $values) {
    $sorted = $values | Sort-Object
    $n = $sorted.Count
    if ($n -eq 0) { return [double]::NaN }
    if ($n % 2 -eq 1) { return $sorted[($n - 1) / 2] }
    return ($sorted[$n / 2 - 1] + $sorted[$n / 2]) / 2
}

# One interleaved timing leg over already-built executables: warm-up once per arm, then $Runs rounds alternating arm
# order. The checksum must agree across the arms and every run, or the leg refuses (the programs must compute the same).
function Measure-Leg([string] $leg, [hashtable] $exes) {
    $rows = [Collections.Generic.List[object]]::new()

    foreach ($bench in $exes.Keys) {
        $pair = $exes[$bench]
        $checksum = $null

        foreach ($name in $arms.Keys) { $null = Invoke-Timed $pair[$name] (Split-Path $pair[$name]) }

        for ($i = 1; $i -le $Runs; $i++) {
            $order = if ($i % 2 -eq 1) { @('base', 'glyph') } else { @('glyph', 'base') }

            foreach ($name in $order) {
                $r = Invoke-Timed $pair[$name] (Split-Path $pair[$name])
                if ($null -eq $checksum) { $checksum = $r.Checksum }
                elseif ($r.Checksum -ne $checksum) { throw "REFUSED [$leg] $bench ${name} run ${i}: output differs from the first run's." }
                $rows.Add([pscustomobject]@{ Leg = $leg; Benchmark = $bench; Arm = $name; Run = $i; WallMs = $r.WallMs; ElapsedNs = $r.ElapsedNs })
            }
        }
    }

    $csv = Join-Path $Out "$leg.csv"
    $rows | Export-Csv $csv -NoTypeInformation -Encoding utf8

    $summary = foreach ($bench in $exes.Keys) {
        $b = $rows | Where-Object { $_.Benchmark -eq $bench -and $_.Arm -eq 'base' }
        $g = $rows | Where-Object { $_.Benchmark -eq $bench -and $_.Arm -eq 'glyph' }
        $bw = Get-Median ($b.ElapsedNs | ForEach-Object { [double]$_ }); $gw = Get-Median ($g.ElapsedNs | ForEach-Object { [double]$_ })
        $bt = Get-Median ($b.WallMs); $gt = Get-Median ($g.WallMs)
        [pscustomobject]@{
            Leg = $leg; Benchmark = $bench
            BaseWorkMs = [math]::Round($bw / 1e6, 2); GlyphWorkMs = [math]::Round($gw / 1e6, 2); WorkDeltaPct = [math]::Round(($gw - $bw) / $bw * 100, 2)
            BaseWallMs = [math]::Round($bt, 1); GlyphWallMs = [math]::Round($gt, 1); WallDeltaPct = [math]::Round(($gt - $bt) / $bt * 100, 2)
        }
    }

    $summary | Export-Csv (Join-Path $Out "$leg-summary.csv") -NoTypeInformation -Encoding utf8
    $summary | Format-Table -AutoSize | Out-String | Tee-Object -FilePath (Join-Path $Out "$leg-summary.txt") | Write-Host
}

# ---- leg 1: the headline, each checkout's own runner ----

if ($Legs -contains 'Headline') {
    for ($r = 1; $r -le $Rounds; $r++) {
        $order = if ($r % 2 -eq 1) { @('base', 'glyph') } else { @('glyph', 'base') }

        foreach ($name in $order) {
            $script = Join-Path $arms[$name] 'src/tests/Performance/run-performance.ps1'
            $runnerArgs = @('--runs', '5')
            if ($NoAot) { $runnerArgs += '--no-aot' }
            # The runner times a variant only once Verify has set it verified, so a re-measure round runs verify too
            # (G's rehearsal, 2026-11-21: `--phase measure` alone printed n/a for every row).
            if ($r -gt 1) { $runnerArgs += @('--phase', 'verify,measure') }
            $log = Join-Path $Out "headline-r$r-$name.md"

            Write-Host "==> headline round $r, $name"
            & pwsh -NoProfile -File $script @runnerArgs *>&1 | Tee-Object -FilePath $log | Out-Null
            if ($LASTEXITCODE -ne 0) { throw "headline round $r ${name}: run-performance.ps1 exited $LASTEXITCODE (log $log)" }

            # run-performance.ps1 mirrors README.md over docs/Performance.md on success (the two differ on master), so that
            # file is restored to the commit's bytes before the check (G's rehearsal, 2026-11-21). The run may also rewrite
            # the benchmarks' tracked .cs (a re-transpile); it must touch nothing else.
            git -C $arms[$name] checkout -- docs/Performance.md
            $dirty = @(git -C $arms[$name] status --porcelain --untracked-files=no | Where-Object { $_ -notmatch '^.. src/tests/Performance/' })
            if ($dirty.Count -gt 0) { throw "headline round $r ${name}: the run modified tracked files outside src/tests/Performance: $($dirty -join '; ')" }
        }

        if ($r -eq 1) {
            # The benchmarks are package main, which the glyph converter's default leaves in today's form, so the two
            # arms' transpiled programs should be identical. A difference is a FINDING (recorded, not refused): it
            # means the arms differ in more than the standard library.
            $compare = foreach ($dir in Get-ChildItem (Join-Path $Base 'src/tests/Performance') -Directory -Filter 'Perf*') {
                foreach ($file in Get-ChildItem $dir.FullName -Filter '*.cs') {
                    $other = Join-Path $Glyph "src/tests/Performance/$($dir.Name)/$($file.Name)"
                    $same = (Test-Path $other) -and ((Get-Content $file.FullName -Raw) -replace "`r`n", "`n") -ceq ((Get-Content $other -Raw) -replace "`r`n", "`n")
                    "{0,-6} {1}/{2}" -f $(if ($same) { 'same' } else { 'DIFFER' }), $dir.Name, $file.Name
                }
            }
            $compare | Set-Content (Join-Path $Out 'transpile-compare.txt') -Encoding utf8
            $differ = @($compare | Where-Object { $_ -like 'DIFFER*' })
            Write-Host "==> transpiled programs: $($compare.Count - $differ.Count) same, $($differ.Count) differ (transpile-compare.txt)"
        }
    }
}

# ---- legs 2 and 3: build per arm, then time interleaved ----

function Build-Arm([string] $leg, [string[]] $benchmarks, [scriptblock] $command) {
    $exes = @{}

    foreach ($bench in $benchmarks) {
        $pair = @{}

        foreach ($name in $arms.Keys) {
            $root = $arms[$name]
            $csproj = Join-Path $root "src/tests/Performance/$bench/$bench.csproj"
            if (-not (Test-Path $csproj)) { throw "REFUSED [$leg] $bench ${name}: no $csproj (run the headline leg first: it transpiles the programs)." }
            $outDir = Join-Path $Out "$leg/$name/$bench"
            $go2csPath = ($root -replace '\\', '/') + '/src/'
            $argv = & $command $csproj $go2csPath $outDir

            Write-Host "==> $leg build $bench, $name"
            & dotnet @argv 2>&1 | Tee-Object -FilePath (Join-Path $Out "$leg-build-$name-$bench.log") | Out-Null
            if ($LASTEXITCODE -ne 0) { throw "[$leg] $bench ${name}: dotnet exited $LASTEXITCODE" }

            $exe = Join-Path $outDir "$bench$exeSuffix"
            if (-not (Test-Path $exe)) { throw "[$leg] $bench ${name}: no executable at $exe" }
            $pair[$name] = $exe

            # A Debug build or a RID-specific self-contained publish leaves the project's own obj/ (and bin/) restored for
            # that shape, which the next JIT build of the same project then trips over (G's rehearsal, 2026-11-21: the
            # runner refused, correctly). The timed executable is under -Out, so the project's own build output goes.
            foreach ($dir in 'bin', 'obj') {
                $stale = Join-Path (Split-Path $csproj) $dir
                if (Test-Path $stale) { Remove-Item -LiteralPath $stale -Recurse -Force }
            }
        }

        $exes[$bench] = $pair
    }

    return $exes
}

if ($Legs -contains 'Debug') {
    $exes = Build-Arm 'debug' $DebugBenchmarks {
        param($csproj, $go2csPath, $outDir)
        @('build', $csproj, '-nologo', '-clp:ErrorsOnly', '-c', 'Debug', "-p:go2csPath=$go2csPath", "-p:OutDir=$outDir/")
    }
    Measure-Leg 'debug' $exes
}

if ($Legs -contains 'R2R') {
    $exes = Build-Arm 'r2r' $R2RBenchmarks {
        param($csproj, $go2csPath, $outDir)
        @('publish', $csproj, '-nologo', '-clp:ErrorsOnly', '-c', 'Release', '-r', $rid, "-p:go2csPath=$go2csPath", '-o', $outDir)
    }
    Measure-Leg 'r2r' $exes
}

Write-Host "==> done: $Out"
