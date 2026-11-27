<#
.SYNOPSIS
    M:N row 16c: the Performance suite's leg of threshold T3 (docs/phase4/DESIGN-mn-stdlib-twins.md section 4).

.DESCRIPTION
    T3 asks whether a consumer that does NOT opt in pays for a standard library built with twins. This script times
    the whole Performance suite (run-performance.ps1, JIT and Native AOT, every row) three times on ONE tree:

        plainA   the standard library plain                       with plainB, the A/A pair: the box's noise floor
        plainB   the standard library plain, again
        twins    the standard library twinned, by the environment variable the caller names (-TwinArm)

    The consumers stay plain in every arm: no GoTwinsFor, and GO2CS_MN unset, so the scheduler is off.

    Before every arm it deletes bin, obj and Generated under THIS tree's src/core and src/tests/Performance (a twinned
    build rewrites src/core in place, and the AOT publish-reuse stamp lives in bin), checks the disk floor, and starts
    run-performance.ps1 in its own PowerShell process with its output in the arm's log. It never passes
    --update-readme. AOT publishes run one at a time inside the runner; nothing else may build on the box meanwhile.

    The readings file, row16c-readings.md, gives per row and per leg: plainA, plainB, the A/A band, twins, the ratio
    and T3's verdict, and each AOT executable's size in all three arms. The template it fills is
    docs/phase4/DATA-mn-row16c-template.md.

    T3, scored as section 4 writes it: a row MISSES when twins is more than 3% over the plain mean BEYOND the A/A band
    ((twins / mean) - 1 - band > 0.03), on JIT or AOT; an AOT executable MISSES when it is more than 1% larger than the
    plain mean. A miss stops row 10 for the owner: the script names every missing row and exits 2.

    Why -TwinArm is an argument: the standard library twin switch, spelled turned on, may appear in no tracked script
    (repoguard TestStdlibTwinsDefaultOff), so the command line that turns it on is the caller's, from the template.

.PARAMETER TwinArm
    NAME=VALUE: the environment variable set for the twins arm only, and removed after it. The value to pass is in
    docs/phase4/DATA-mn-row16c-template.md.

.PARAMETER Runs
    Measured runs per variant, forwarded as --runs (default 5; the runner's own per-row overrides still apply).

.PARAMETER Filter
    Forwarded as --filter. Row 16c is every row; a filter is for a rehearsal only, and the readings say so.

.PARAMETER NoAot
    Forwarded as --no-aot: the JIT leg alone. T3 needs both legs; the readings say so.

.PARAMETER OutDir
    Where the logs and readings go. Default: a new directory under the system temp directory.

.PARAMETER DiskFloorGB
    Free space required before every arm (default 30).

.EXAMPLE
    ./run-row16c-t3.ps1 -TwinArm '<from DATA-mn-row16c-template.md>'
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)][string] $TwinArm,
    [int]    $Runs = 5,
    [string] $Filter = '',
    [switch] $NoAot,
    [string] $OutDir = '',
    [int]    $DiskFloorGB = 30
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest
. (Join-Path $PSScriptRoot '../../_paths.ps1')

$perfDir = $PSScriptRoot
$coreDir = Join-Path $SrcRoot 'core'
$runner  = Join-Path $PSScriptRoot 'run-performance.ps1'

function Fail([string] $message)
{
    Write-Host "ROW16C FAILURE $message" -ForegroundColor Red
    exit 1
}

if ($TwinArm -notmatch '^([A-Za-z_][A-Za-z0-9_]*)=(.+)$') { Fail "-TwinArm '$TwinArm' is not NAME=VALUE" }
$twinName  = $Matches[1]
$twinValue = $Matches[2]

foreach ($name in @('GO2CS_MN', 'GoTwinsFor', 'GOMAXPROCS', $twinName))
{
    if ($null -ne [Environment]::GetEnvironmentVariable($name)) { Fail "$name is set in the environment; every arm must start without it" }
}

if ($OutDir -eq '') { $OutDir = Join-Path ([IO.Path]::GetTempPath()) ("row16c-" + (Get-Date -Format 'yyyyMMdd-HHmmss')) }
New-Item -ItemType Directory -Force -Path $OutDir | Out-Null
$OutDir = (Resolve-Path $OutDir).Path
$pwshExe = (Get-Process -Id $PID).Path

function Get-FreeGB([string] $path)
{
    $full = [IO.Path]::GetFullPath($path)
    $best = $null
    foreach ($d in [IO.DriveInfo]::GetDrives())
    {
        if (-not $d.IsReady) { continue }
        $rootPath = $d.RootDirectory.FullName
        if ($full.StartsWith($rootPath, [StringComparison]::OrdinalIgnoreCase) -and ($null -eq $best -or $rootPath.Length -gt $best.RootDirectory.FullName.Length)) { $best = $d }
    }
    if ($null -eq $best) { return -1 }
    return [int][math]::Floor($best.AvailableFreeSpace / 1GB)
}

# Nothing tracked may live under the directories the purge deletes.
$tracked = @(& git -C $RepoRoot ls-files -- 'src/core' 'src/tests/Performance' | Where-Object { $_ -match '/(bin|obj|Generated)/' })
if ($LASTEXITCODE -ne 0) { Fail "git ls-files failed in $RepoRoot" }
if ($tracked.Count -gt 0) { Fail "$($tracked.Count) tracked file(s) under bin/obj/Generated, e.g. $($tracked[0]); the purge would delete them" }

function Clear-BuildOutput
{
    foreach ($base in @($coreDir, $perfDir))
    {
        $dirs = @(Get-ChildItem -LiteralPath $base -Directory -Recurse -Force -ErrorAction SilentlyContinue |
            Where-Object { $_.Name -eq 'bin' -or $_.Name -eq 'obj' -or $_.Name -eq 'Generated' } |
            Sort-Object { $_.FullName.Length })
        foreach ($d in $dirs)
        {
            if (Test-Path -LiteralPath $d.FullName) { Remove-Item -LiteralPath $d.FullName -Recurse -Force }
        }
    }
}

$sha   = (& git -C $RepoRoot rev-parse HEAD).Trim()
$dirty = @(& git -C $RepoRoot status --porcelain --untracked-files=no).Count
$sdk   = @(& dotnet --version 2>$null)
if ($sdk.Count -gt 0) { $sdk = $sdk[0] } else { $sdk = '?' }
$head  = "row16c: tree=$RepoRoot sha=$sha tracked-changes=$dirty cores=$([Environment]::ProcessorCount) runs=$Runs filter='$Filter' aot=$(-not $NoAot) host=$([Runtime.InteropServices.RuntimeInformation]::OSDescription.Trim()) sdk=$sdk"
Write-Host $head
if ($dirty -ne 0) { Write-Host "row16c: WARNING the tree has $dirty tracked change(s): the readings name a SHA the tree does not match" -ForegroundColor Yellow }

$runnerArgs = @('--runs', "$Runs")
if ($NoAot) { $runnerArgs += '--no-aot' }
if ($Filter -ne '') { $runnerArgs += @('--filter', $Filter) }

function Invoke-Arm([string] $arm, [bool] $twinned)
{
    $free = Get-FreeGB $RepoRoot
    if ($free -lt $DiskFloorGB) { Fail "arm ${arm}: $free GB free, the floor is $DiskFloorGB GB" }
    Write-Host ""
    Write-Host "== arm ${arm}: twinned=$twinned free=${free}GB"
    Clear-BuildOutput

    $log = Join-Path $OutDir "$arm.log"
    $err = Join-Path $OutDir "$arm.stderr.log"
    # One pre-quoted argument string: the array form does not quote a path with a space in it.
    $argLine = "-NoProfile -File `"$runner`" " + (($runnerArgs | ForEach-Object { if ($_ -match '\s') { "`"$_`"" } else { $_ } }) -join ' ')

    if ($twinned) { [Environment]::SetEnvironmentVariable($twinName, $twinValue, 'Process') }
    $start = Get-Date
    try
    {
        $p = Start-Process -FilePath $pwshExe -ArgumentList $argLine -RedirectStandardOutput $log -RedirectStandardError $err -NoNewWindow -Wait -PassThru
    }
    finally
    {
        if ($twinned) { [Environment]::SetEnvironmentVariable($twinName, $null, 'Process') }
    }
    $rc = $p.ExitCode
    $secs = [int]((Get-Date) - $start).TotalSeconds
    Write-Host "   arm ${arm}: run-performance.ps1 exit $rc in ${secs}s (log $log)"
    if ($rc -ne 0) { Fail "arm ${arm}: run-performance.ps1 exit $rc; read $log and $err" }

    # The suite's own mirror rewrites docs/Performance.md from README.md; with no --update-readme it must not move.
    $moved = @(& git -C $RepoRoot status --porcelain --untracked-files=no)
    if ($moved.Count -gt 0)
    {
        $moved | Set-Content -LiteralPath (Join-Path $OutDir "$arm.status.txt")
        Write-Host "   arm ${arm}: WARNING $($moved.Count) tracked file(s) changed during the arm (listed in $arm.status.txt); the next arm runs on them" -ForegroundColor Yellow
    }

    # The execution-time table: the block after the '**Execution time**' line.
    $rows = [ordered]@{}
    $inTable = $false
    foreach ($line in [IO.File]::ReadAllLines($log))
    {
        if ($line -match '^\*\*Execution time\*\*') { $inTable = $true; continue }
        if (-not $inTable) { continue }
        if ($line -match '^\|\s*Benchmark\s*\|' -or $line -match '^\|-') { continue }
        if ($line -match '^\|')
        {
            $cells = @($line.Trim().Trim('|').Split('|') | ForEach-Object { $_.Trim() })
            if ($cells.Count -lt 4) { continue }
            $rows[$cells[0]] = @{ Jit = (ConvertTo-Ms $cells[2]); Aot = (ConvertTo-Ms $cells[3]) }
            continue
        }
        if ($rows.Count -gt 0) { break }
    }
    if ($rows.Count -eq 0) { Fail "arm ${arm}: no execution-time table in $log" }

    # Each AOT executable's size, read before the next arm's purge deletes it.
    $sizes = @{}
    foreach ($proj in @(Get-ChildItem -LiteralPath $perfDir -Directory -Filter 'Perf*' | Where-Object { $_.Name -notlike 'PerformanceRunner*' }))
    {
        $exe = Join-Path $proj.FullName ("bin/Release/aot/" + $proj.Name + $ExeSuffix)
        if (Test-Path -LiteralPath $exe) { $sizes[$proj.Name.Substring(4)] = (Get-Item -LiteralPath $exe).Length }
    }

    return @{ Rows = $rows; Sizes = $sizes; Seconds = $secs }
}

function ConvertTo-Ms([string] $cell)
{
    if ($cell -match '^([0-9][0-9,]*(\.[0-9]+)?)') { return [double]::Parse($Matches[1].Replace(',', ''), [Globalization.CultureInfo]::InvariantCulture) }
    return $null
}

$arms = [ordered]@{}
$arms['plainA'] = Invoke-Arm 'plainA' $false
$arms['plainB'] = Invoke-Arm 'plainB' $false
$arms['twins']  = Invoke-Arm 'twins'  $true

# ---- the readings and T3 ----
$inv = [Globalization.CultureInfo]::InvariantCulture
function F1($v) { if ($null -eq $v) { return 'n/a' } return ([double]$v).ToString('N1', $inv) }
function Pct($v) { if ($null -eq $v) { return 'n/a' } return (100.0 * [double]$v).ToString('N2', $inv) + '%' }

$md = New-Object System.Collections.Generic.List[string]
$md.Add('# Row 16c readings (T3, the Performance suite)')
$md.Add('')
$md.Add("    $head")
$md.Add("    twins arm: $twinName=$twinValue in the environment; arm walls: plainA $($arms['plainA'].Seconds)s, plainB $($arms['plainB'].Seconds)s, twins $($arms['twins'].Seconds)s")
if ($Filter -ne '' -or $NoAot) { $md.Add('    REHEARSAL: a filter or --no-aot was passed, so this is not row 16c; T3 needs every row on both legs.') }
$md.Add('')
$md.Add('Execution time, milliseconds (the suite''s own unit; median of the runner''s runs).')
$md.Add('')
$md.Add('| Benchmark | leg | plainA | plainB | A/A band | twins | twins / plain mean | beyond A/A | T3 (+3%) |')
$md.Add('|---|---|---:|---:|---:|---:|---:|---:|---|')

$misses = New-Object System.Collections.Generic.List[string]
foreach ($name in $arms['plainA'].Rows.Keys)
{
    foreach ($leg in @('Jit', 'Aot'))
    {
        $a = $arms['plainA'].Rows[$name][$leg]
        $b = if ($arms['plainB'].Rows.Contains($name)) { $arms['plainB'].Rows[$name][$leg] } else { $null }
        $t = if ($arms['twins'].Rows.Contains($name)) { $arms['twins'].Rows[$name][$leg] } else { $null }
        if ($null -eq $a -and $null -eq $b -and $null -eq $t) { continue }
        $band = $null; $ratio = $null; $excess = $null; $verdict = 'NOT MEASURED'
        if ($null -ne $a -and $null -ne $b -and $null -ne $t)
        {
            $mean   = ($a + $b) / 2.0
            $band   = [math]::Abs($a - $b) / $mean
            $ratio  = $t / $mean
            $excess = $ratio - 1.0 - $band
            if ($excess -gt 0.03) { $verdict = 'MISSED'; $misses.Add("$name $leg") } else { $verdict = 'MET' }
        }
        $legName = if ($leg -eq 'Jit') { 'JIT' } else { 'AOT' }
        $md.Add("| $name | $legName | $(F1 $a) | $(F1 $b) | $(Pct $band) | $(F1 $t) | $(if ($null -eq $ratio) { 'n/a' } else { $ratio.ToString('N4', $inv) }) | $(Pct $excess) | $verdict |")
    }
}

$md.Add('')
$md.Add('AOT executable bytes (T3: twins at most +1% over the plain mean).')
$md.Add('')
$md.Add('| Benchmark | plainA | plainB | twins | twins / plain mean | T3 (+1%) |')
$md.Add('|---|---:|---:|---:|---:|---|')
foreach ($name in ($arms['plainA'].Sizes.Keys | Sort-Object))
{
    $a = $arms['plainA'].Sizes[$name]
    $b = if ($arms['plainB'].Sizes.Contains($name)) { $arms['plainB'].Sizes[$name] } else { $null }
    $t = if ($arms['twins'].Sizes.Contains($name)) { $arms['twins'].Sizes[$name] } else { $null }
    $verdict = 'NOT MEASURED'; $ratioText = 'n/a'
    if ($null -ne $b -and $null -ne $t)
    {
        $ratio = $t / (($a + $b) / 2.0)
        $ratioText = $ratio.ToString('N4', $inv)
        if ($ratio - 1.0 -gt 0.01) { $verdict = 'MISSED'; $misses.Add("$name AOT size") } else { $verdict = 'MET' }
    }
    $md.Add("| $name | $a | $(if ($null -eq $b) { 'n/a' } else { $b }) | $(if ($null -eq $t) { 'n/a' } else { $t }) | $ratioText | $verdict |")
}

$md.Add('')
if ($misses.Count -gt 0)
{
    $md.Add("**T3 MISSED on $($misses.Count): $($misses -join '; ').** Section 4: a T3 miss stops row 10 for the owner. Report these rows and stop.")
}
else
{
    $md.Add('**T3 MET on every row read** (NOT MEASURED rows, if any, are listed above and are not a pass).')
}

$readings = Join-Path $OutDir 'row16c-readings.md'
[IO.File]::WriteAllLines($readings, $md)
Write-Host ""
$md | ForEach-Object { Write-Host $_ }
Write-Host ""
Write-Host "row16c: readings in $readings"
if ($misses.Count -gt 0) { exit 2 }
exit 0
