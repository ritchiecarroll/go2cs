<#
.SYNOPSIS
    M:N row 16c: the sqlite-cs and TypeScript-go-cs legs of threshold T3 (docs/phase4/DESIGN-mn-stdlib-twins.md
    section 4). The Performance suite's leg is run-row16c-t3.ps1; this script follows its shape.

.DESCRIPTION
    T3 asks whether a consumer that does NOT opt in pays for a standard library built with twins. This script takes
    ONE consumer, already converted with `go2cs -recurse` against THIS tree (local project references into src/core),
    and times its own workload three times on ONE tree:

        plainA   the standard library plain                       with plainB, the A/A pair: the box's noise floor
        plainB   the standard library plain, again
        twins    the standard library twinned, by the environment variable the caller names (-TwinArm)

    The consumer stays plain in every arm: no GoTwinsFor, and GO2CS_MN unset, so the scheduler is off.

    Each arm, in order:
      1. checks the disk floor and deletes bin, obj, Generated and .artifacts under this tree's src/core and under the
         consumer's converted root (a twinned build rewrites src/core's outputs in place);
      2. JIT leg: `dotnet build <project> -c Release -p:GoTargetOS=<os>`, then reads the built assembly's path from
         MSBuild and runs it with `dotnet <dll> <args>`: one discarded warm-up run, then -Runs measured runs;
      3. AOT leg (unless -NoAot): `dotnet publish <project> -c Release -r <rid> -p:PublishAot=true -p:GoTargetOS=<os>
         -o <arm>/aot`, sampling the publish tree's CPU every -SampleSeconds so a quiet ILC is shown working, not hung;
         then the same warm-up and -Runs measured runs of the native executable, and its size in bytes.
    Every run is a .NET Process with stdout and stderr redirected and drained (never a shell pipe). Every run's stdout
    is hashed; every run in every arm must print the same bytes as the first, or the arm stops (a twinned stdlib that
    changes a consumer's output is not a timing question). -ExpectedStdout compares against a native Go oracle too.
    Every build log is counted for GO2CS0017, GO2CS0018 and GO2CS0021, which must be zero in every arm.

    Each arm writes <arm>.json. The readings, row16c-<leg>-readings.md, give per leg: plainA, plainB (medians, in
    seconds), the A/A band, twins, the ratio and T3's verdict, and the AOT executable's size in all three arms. The
    template it fills is docs/phase4/DATA-mn-row16c-consumers-template.md. -ScoreFrom re-scores saved arm files
    without building anything.

    T3, scored as section 4 writes it (the same arithmetic as run-row16c-t3.ps1): a leg MISSES when twins is more
    than 3% over the plain mean BEYOND the A/A band ((twins / mean) - 1 - band > 0.03); the AOT executable MISSES when
    it is more than 1% larger than the plain mean. A miss stops row 10 for the owner: the script names it and exits 2.

    Why -TwinArm is an argument: the standard library twin switch, spelled turned on, may appear in no tracked script
    (repoguard TestStdlibTwinsDefaultOff), so the command line that turns it on is the caller's, from the template.

.PARAMETER Leg
    A label for the readings: 'sqlite-cs' or 'typescript-go-cs' (or anything else, for a rehearsal).

.PARAMETER Project
    The consumer's program .csproj inside its converted root (for sqlite-cs:
    <out>/src/sqlitecs/demo/sqlitecs.demo.csproj).

.PARAMETER ConsumerRoot
    The converted root the project lives under (the `<out>` of `go2cs -recurse ... <src> <out>`). Its build output is
    purged before every arm. It must have been converted against THIS tree; the script refuses a root whose generated
    Directory.Build.props does not name this tree's src.

.PARAMETER ArgsFile
    Optional: a file with one program argument per line (blank lines skipped), e.g. TypeScript's units-101 list.

.PARAMETER WorkDir
    Optional: the working directory of every run (default: the project's directory).

.PARAMETER ExpectedStdout
    Optional: a file holding native Go's stdout for the same workload. Every run must print exactly these bytes.

.PARAMETER TwinArm
    NAME=VALUE: the environment variable set for the twins arm only (builds and runs), and removed after it. The value
    to pass is in docs/phase4/DATA-mn-row16c-consumers-template.md.

.PARAMETER Runs
    Measured runs per leg per arm, after one discarded warm-up (default 5). The reading is the median.

.PARAMETER NoAot
    The JIT leg alone. T3 needs both legs; the readings say so.

.PARAMETER GoTargetOS
    Default: the host's (windows, linux or darwin). Always passed explicitly, since without it on Linux the Windows
    flavour of the standard library links.

.PARAMETER OutDir
    Where logs, arm files and readings go. Default: a new directory under the system temp directory.

.PARAMETER DiskFloorGB
    Free space required before every arm (default 30).

.PARAMETER SampleSeconds
    The AOT publish's CPU is sampled this often into the arm's aot-cpu.log (default 60).

.PARAMETER PublishArgs
    Extra arguments appended to every arm's AOT `dotnet publish`, the same in all three arms, so T3 still compares like
    with like. TypeScript-go-cs needs '-p:TrimMode=full','-p:NativeDebugSymbols=false' (COORD's ruling 2026-11-27: its
    default TrimMode=partial failed in ILC with "Stream was too long" after about 3 hours; full with no native symbols
    is the one known-good publish). The readings print them.

.PARAMETER DryRun
    Print every command each arm would run, in order, and exit 0. Nothing is deleted, built or run.

.PARAMETER ScoreFrom
    A directory holding plainA.json, plainB.json and twins.json from an earlier run: score them and exit.

.EXAMPLE
    ./run-row16c-consumers.ps1 -Leg sqlite-cs -ConsumerRoot <out> -Project <out>/src/sqlitecs/demo/sqlitecs.demo.csproj `
        -ExpectedStdout <native.txt> -TwinArm '<from DATA-mn-row16c-consumers-template.md>' -Runs 21 -OutDir <dir>
#>
[CmdletBinding()]
param(
    [string] $Leg = '',
    [string] $Project = '',
    [string] $ConsumerRoot = '',
    [string] $ArgsFile = '',
    [string] $WorkDir = '',
    [string] $ExpectedStdout = '',
    [string] $TwinArm = '',
    [int]    $Runs = 5,
    [switch] $NoAot,
    [string] $GoTargetOS = '',
    [string] $OutDir = '',
    [int]    $DiskFloorGB = 30,
    [int]    $SampleSeconds = 60,
    [string[]] $PublishArgs = @(),
    [switch] $DryRun,
    [string] $ScoreFrom = ''
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest
. (Join-Path $PSScriptRoot '../../_paths.ps1')

$coreDir = Join-Path $SrcRoot 'core'
$inv = [Globalization.CultureInfo]::InvariantCulture
$armNames = @('plainA', 'plainB', 'twins')
$warningIds = @('GO2CS0017', 'GO2CS0018', 'GO2CS0021')

function Fail([string] $message)
{
    Write-Host "ROW16C FAILURE $message" -ForegroundColor Red
    exit 1
}

function Get-Median([double[]] $values)
{
    if ($null -eq $values -or $values.Count -eq 0) { return $null }
    $s = @($values | Sort-Object)
    $n = $s.Count
    if ($n % 2 -eq 1) { return [double]$s[($n - 1) / 2] }
    return ([double]$s[$n / 2 - 1] + [double]$s[$n / 2]) / 2.0
}

# ---- scoring: shared by a full run and -ScoreFrom ----
function Write-Readings([hashtable] $arms, [string] $dir, [string] $headLine)
{
    function F3($v) { if ($null -eq $v) { return 'n/a' } return ([double]$v).ToString('N3', $inv) }
    function Pct($v) { if ($null -eq $v) { return 'n/a' } return (100.0 * [double]$v).ToString('N2', $inv) + '%' }

    $legName = $arms['plainA'].leg
    $md = New-Object System.Collections.Generic.List[string]
    $md.Add("# Row 16c readings (T3, $legName)")
    $md.Add('')
    $md.Add("    $headLine")
    foreach ($a in $armNames)
    {
        $x = $arms[$a]
        $tn = if ($x.PSObject.Properties.Name -contains 'twinNames') { "$($x.twinNames) in $($x.twinAssemblies) go.*.dll" } else { 'not counted' }
        $pa = if ($x.PSObject.Properties.Name -contains 'publishArgs' -and $null -ne $x.publishArgs -and @($x.publishArgs).Count -gt 0) { @($x.publishArgs) -join ' ' } else { '(none)' }
        $md.Add("    arm ${a}: twin names=$tn; extra publish args: $pa")
        $md.Add("    arm ${a}: sha=$($x.sha) twinned=$($x.twinned) runs=$($x.runs) jit-build=$($x.jitBuildSeconds)s aot-publish=$(if ($null -eq $x.aotPublishSeconds) { 'n/a' } else { "$($x.aotPublishSeconds)s" }) stdout=$($x.stdoutSha16) warnings $($warningIds -join '/')=$($x.warnings -join '/')")
    }
    $rehearsal = @($armNames | Where-Object { $arms[$_].noAot -or $arms[$_].runs -lt 3 })
    if ($rehearsal.Count -gt 0) { $md.Add('    REHEARSAL: -NoAot or fewer than 3 runs in an arm, so this is not row 16c; T3 needs both legs.') }
    $md.Add('')
    $md.Add('Workload wall, seconds (median of the measured runs after one discarded warm-up; process start to exit).')
    $md.Add('')
    $md.Add('| leg | plainA | plainB | A/A band | twins | twins / plain mean | beyond A/A | T3 (+3%) |')
    $md.Add('|---|---:|---:|---:|---:|---:|---:|---|')

    $misses = New-Object System.Collections.Generic.List[string]
    $notMeasured = New-Object System.Collections.Generic.List[string]
    foreach ($leg in @('jit', 'aot'))
    {
        $a = $arms['plainA'].$leg; $b = $arms['plainB'].$leg; $t = $arms['twins'].$leg
        $band = $null; $ratio = $null; $excess = $null; $verdict = 'NOT MEASURED'
        if ($null -ne $a -and $null -ne $b -and $null -ne $t)
        {
            $mean   = ($a + $b) / 2.0
            $band   = [math]::Abs($a - $b) / $mean
            $ratio  = $t / $mean
            $excess = $ratio - 1.0 - $band
            if ($excess -gt 0.03) { $verdict = 'MISSED'; $misses.Add("$legName $($leg.ToUpper())") } else { $verdict = 'MET' }
        }
        else { $notMeasured.Add($leg.ToUpper()) }
        $md.Add("| $($leg.ToUpper()) | $(F3 $a) | $(F3 $b) | $(Pct $band) | $(F3 $t) | $(if ($null -eq $ratio) { 'n/a' } else { $ratio.ToString('N4', $inv) }) | $(Pct $excess) | $verdict |")
    }

    $md.Add('')
    $md.Add('AOT executable bytes (T3: twins at most +1% over the plain mean).')
    $md.Add('')
    $md.Add('| plainA | plainB | twins | twins / plain mean | T3 (+1%) |')
    $md.Add('|---:|---:|---:|---:|---|')
    $sa = $arms['plainA'].aotBytes; $sb = $arms['plainB'].aotBytes; $st = $arms['twins'].aotBytes
    $sizeVerdict = 'NOT MEASURED'; $sizeRatio = 'n/a'
    if ($null -ne $sa -and $null -ne $sb -and $null -ne $st)
    {
        $r = $st / (($sa + $sb) / 2.0)
        $sizeRatio = $r.ToString('N4', $inv)
        if ($r - 1.0 -gt 0.01) { $sizeVerdict = 'MISSED'; $misses.Add("$legName AOT size") } else { $sizeVerdict = 'MET' }
    }
    else { $notMeasured.Add('AOT size') }
    $md.Add("| $(if ($null -eq $sa) { 'n/a' } else { $sa }) | $(if ($null -eq $sb) { 'n/a' } else { $sb }) | $(if ($null -eq $st) { 'n/a' } else { $st }) | $sizeRatio | $sizeVerdict |")

    $md.Add('')
    $warned = @($armNames | Where-Object { ($arms[$_].warnings | Measure-Object -Sum).Sum -gt 0 })
    if ($warned.Count -gt 0)
    {
        $md.Add("**GATE FAILED: $($warningIds -join '/') fired in arm(s) $($warned -join ', ')** (must be 0 in every arm; a separate gate, not a T3 reading).")
    }
    else { $md.Add("Gate: $($warningIds -join '/') = 0 in every arm.") }
    if ($misses.Count -gt 0)
    {
        $md.Add("**T3 MISSED: $($misses -join '; ').** Section 4: a T3 miss stops row 10 for the owner. Report it and stop.")
    }
    if ($notMeasured.Count -gt 0)
    {
        $md.Add("**T3 NOT MEASURED on: $($notMeasured -join ', ').** That is not a pass; name it.")
    }
    if ($misses.Count -eq 0 -and $notMeasured.Count -eq 0)
    {
        $md.Add('**T3 MET on both legs and on the AOT executable size.**')
    }
    $md.Add('')
    $md.Add('Exit: 0 met, 2 a T3 miss or the gate failed, 3 something not measured (not a pass).')

    $file = Join-Path $dir "row16c-$($legName -replace '[^A-Za-z0-9_.-]', '_')-readings.md"
    [IO.File]::WriteAllLines($file, $md)
    Write-Host ''
    $md | ForEach-Object { Write-Host $_ }
    Write-Host ''
    Write-Host "row16c: readings in $file"
    if ($misses.Count -gt 0 -or $warned.Count -gt 0) { return 2 }
    if ($notMeasured.Count -gt 0) { return 3 }
    return 0
}

if ($ScoreFrom -ne '')
{
    $arms = @{}
    foreach ($a in $armNames)
    {
        $f = Join-Path $ScoreFrom "$a.json"
        if (-not (Test-Path -LiteralPath $f)) { Fail "-ScoreFrom: $f is missing" }
        $arms[$a] = Get-Content -Raw -LiteralPath $f | ConvertFrom-Json
    }
    exit (Write-Readings $arms $ScoreFrom "re-scored from $ScoreFrom")
}

# ---- a run: validate ----
if ($Leg -eq '') { Fail '-Leg is required' }
if ($Project -eq '' -or -not (Test-Path -LiteralPath $Project)) { Fail "-Project '$Project' does not exist" }
if ($ConsumerRoot -eq '' -or -not (Test-Path -LiteralPath $ConsumerRoot)) { Fail "-ConsumerRoot '$ConsumerRoot' does not exist" }
$Project = (Resolve-Path -LiteralPath $Project).Path
$ConsumerRoot = (Resolve-Path -LiteralPath $ConsumerRoot).Path
if (-not $Project.StartsWith($ConsumerRoot)) { Fail "-Project is not under -ConsumerRoot" }
if ($TwinArm -notmatch '^([A-Za-z_][A-Za-z0-9_]*)=(.+)$') { Fail "-TwinArm '$TwinArm' is not NAME=VALUE" }
$twinName  = $Matches[1]
$twinValue = $Matches[2]
if ($Runs -lt 1) { Fail '-Runs must be at least 1' }
# The consumer must not opt in and the scheduler must be off; and a GC or roll-forward override would time a different
# runtime than the consumer's own runtimeconfig (the TypeScript tables were taken with these unset).
foreach ($name in @('GO2CS_MN', 'GoTwinsFor', 'GOMAXPROCS', 'GOGC', 'DOTNET_gcServer', 'DOTNET_GCgen0size', 'DOTNET_ROLL_FORWARD', $twinName))
{
    if ($null -ne [Environment]::GetEnvironmentVariable($name)) { Fail "$name is set in the environment; every arm must start without it" }
}

# The consumer must reference THIS tree's src/core: its generated Directory.Build.props pins $(go2csPath).
if ($null -ne [Environment]::GetEnvironmentVariable('go2csPath')) { Fail 'go2csPath is set in the environment; it would override the consumer''s pin to this tree' }
$pin = Join-Path $ConsumerRoot 'Directory.Build.props'
if (-not (Test-Path -LiteralPath $pin)) { Fail "no ${pin}: not a go2cs -recurse root (the template's convert step writes it)" }
$pinText = [IO.File]::ReadAllText($pin) -replace '(?s)<!--.*?-->', ''
if ($pinText -notmatch '<go2csPath>([^<]+)</go2csPath>') { Fail "$pin sets no go2csPath: a -recurse=nuget root builds against published packages, not this tree" }
$pinValue = $Matches[1].Trim().Replace('$(MSBuildThisFileDirectory)', $ConsumerRoot + [IO.Path]::DirectorySeparatorChar)
$pinFull = [IO.Path]::GetFullPath($pinValue.Replace('\', [IO.Path]::DirectorySeparatorChar).Replace('/', [IO.Path]::DirectorySeparatorChar)).TrimEnd([IO.Path]::DirectorySeparatorChar)
$srcFull = [IO.Path]::GetFullPath($SrcRoot).TrimEnd([IO.Path]::DirectorySeparatorChar)
if (-not [string]::Equals($pinFull, $srcFull, $(if ($IsWindowsHost) { [StringComparison]::OrdinalIgnoreCase } else { [StringComparison]::Ordinal })))
{
    Fail "$pin pins go2csPath to $pinFull, not this tree's src ($srcFull); reconvert the consumer against this tree (the template's convert step)"
}

$progArgs = @()
if ($ArgsFile -ne '')
{
    if (-not (Test-Path -LiteralPath $ArgsFile)) { Fail "-ArgsFile '$ArgsFile' does not exist" }
    $progArgs = @(Get-Content -LiteralPath $ArgsFile | Where-Object { $_.Trim() -ne '' })
}
if ($WorkDir -eq '') { $WorkDir = Split-Path -Parent $Project }
if (-not (Test-Path -LiteralPath $WorkDir)) { Fail "-WorkDir '$WorkDir' does not exist" }
$expectedBytes = $null
if ($ExpectedStdout -ne '')
{
    if (-not (Test-Path -LiteralPath $ExpectedStdout)) { Fail "-ExpectedStdout '$ExpectedStdout' does not exist" }
    $expectedBytes = [IO.File]::ReadAllBytes((Resolve-Path -LiteralPath $ExpectedStdout).Path)
}

if ($GoTargetOS -eq '')
{
    $GoTargetOS = if ($IsWindowsHost) { 'windows' } elseif ($IsMacOS) { 'darwin' } else { 'linux' }
}
$arch = [Runtime.InteropServices.RuntimeInformation]::OSArchitecture.ToString().ToLowerInvariant()
$ridOs = switch ($GoTargetOS) { 'windows' { 'win' } 'darwin' { 'osx' } default { 'linux' } }
$rid = "$ridOs-$arch"

if ($OutDir -eq '') { $OutDir = Join-Path ([IO.Path]::GetTempPath()) ("row16c-$Leg-" + (Get-Date -Format 'yyyyMMdd-HHmmss')) }

$buildArgs   = @('build', $Project, '-c', 'Release', "-p:GoTargetOS=$GoTargetOS", '-p:UseSharedCompilation=false', '-nodeReuse:false')
$pathArgs    = @('msbuild', $Project, '-getProperty:TargetPath', '-p:Configuration=Release', "-p:GoTargetOS=$GoTargetOS")
# PowerShell names are case-insensitive: this must not be called $publishArgs, or it overwrites -PublishArgs.
$aotCommandArgs = @('publish', $Project, '-c', 'Release', '-r', $rid, '-p:PublishAot=true', "-p:GoTargetOS=$GoTargetOS", '-p:UseSharedCompilation=false', '-nodeReuse:false') + $PublishArgs

function Format-Cmd([string] $exe, [string[]] $a) { return ($exe + ' ' + (($a | ForEach-Object { if ($_ -match '\s') { "`"$_`"" } else { $_ } }) -join ' ')) }

if ($DryRun)
{
    Write-Host "row16c DRY RUN: leg=$Leg tree=$RepoRoot GoTargetOS=$GoTargetOS rid=$rid runs=$Runs (+1 warm-up) aot=$(-not $NoAot) out=$OutDir"
    Write-Host "  program args: $($progArgs.Count) (from '$ArgsFile'); workdir: $WorkDir; expected stdout: '$ExpectedStdout'"
    foreach ($a in $armNames)
    {
        $tw = ($a -eq 'twins')
        Write-Host ""
        Write-Host "== arm $a$(if ($tw) { " (env $twinName=$twinValue for every command below, removed after)" })"
        Write-Host "  check free disk >= $DiskFloorGB GB"
        Write-Host "  delete bin/obj/Generated/.artifacts under $coreDir and $ConsumerRoot"
        Write-Host "  $(Format-Cmd 'dotnet' $buildArgs)   > $a.jit-build.log"
        Write-Host "  $(Format-Cmd 'dotnet' $pathArgs)   -> <dll>"
        Write-Host "  count twin method names in <dll dir>/go.*.dll: $(if ($tw) { 'must exceed plainA''s' } elseif ($a -eq 'plainA') { 'the plain baseline' } else { 'must equal plainA''s' })"
        Write-Host "  dotnet <dll> <$($progArgs.Count) args>   x$($Runs + 1) (first discarded), cwd $WorkDir"
        if (-not $NoAot)
        {
            Write-Host "  $(Format-Cmd 'dotnet' ($aotCommandArgs + @('-o', (Join-Path $OutDir "$a/aot"))))   > $a.aot-publish.log, CPU sampled every ${SampleSeconds}s"
            Write-Host "  <aot exe> <$($progArgs.Count) args>   x$($Runs + 1) (first discarded); size in bytes"
        }
        Write-Host "  count $($warningIds -join '/') in both logs; write $a.json"
    }
    Write-Host ""
    Write-Host "then: score the three arm files against T3 -> row16c-$Leg-readings.md (exit 2 on a miss)"
    exit 0
}

New-Item -ItemType Directory -Force -Path $OutDir | Out-Null
$OutDir = (Resolve-Path $OutDir).Path

function Get-FreeGB([string] $path)
{
    $full = [IO.Path]::GetFullPath($path)
    # On Windows, query ONLY the drive that holds the path. Enumerating every drive and asking IsReady blocks on a mapped
    # network drive that is reconnecting: 90 minutes on the i7 before the first arm (2026-11-27, COORD's f7494cabe5).
    if ($IsWindowsHost)
    {
        try { return [int][math]::Floor([IO.DriveInfo]::new([IO.Path]::GetPathRoot($full)).AvailableFreeSpace / 1GB) }
        catch { return -1 }
    }
    # Elsewhere every path's root is '/', so take the longest mount point that holds the path.
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
$tracked = @(& git -C $RepoRoot ls-files -- 'src/core' | Where-Object { $_ -match '/(bin|obj|Generated|\.artifacts)/' })
if ($LASTEXITCODE -ne 0) { Fail "git ls-files failed in $RepoRoot" }
if ($tracked.Count -gt 0) { Fail "$($tracked.Count) tracked file(s) under bin/obj/Generated/.artifacts, e.g. $($tracked[0]); the purge would delete them" }
if ($OutDir.StartsWith($ConsumerRoot) -or $OutDir.StartsWith($coreDir)) { Fail "-OutDir must not be under the purged trees" }

function Clear-BuildOutput
{
    foreach ($base in @($coreDir, $ConsumerRoot))
    {
        $dirs = @(Get-ChildItem -LiteralPath $base -Directory -Recurse -Force -ErrorAction SilentlyContinue |
            Where-Object { $_.Name -in @('bin', 'obj', 'Generated', '.artifacts') } |
            Sort-Object { $_.FullName.Length })
        foreach ($d in $dirs)
        {
            if (Test-Path -LiteralPath $d.FullName) { Remove-Item -LiteralPath $d.FullName -Recurse -Force }
        }
    }
}

# A child process with stdout and stderr redirected and drained; returns exit code, wall seconds and stdout bytes.
function Invoke-Child([string] $exe, [string[]] $a, [string] $cwd, [string] $logPath, [string] $cpuLog)
{
    $psi = [Diagnostics.ProcessStartInfo]::new($exe)
    $psi.UseShellExecute = $false
    $psi.RedirectStandardOutput = $true
    $psi.RedirectStandardError = $true
    $psi.WorkingDirectory = $cwd
    foreach ($x in $a) { $psi.ArgumentList.Add($x) }
    $psi.Environment['MSBUILDDISABLENODEREUSE'] = '1'
    $sw = [Diagnostics.Stopwatch]::StartNew()
    $p = [Diagnostics.Process]::Start($psi)
    $ms = [IO.MemoryStream]::new()
    $outTask = $p.StandardOutput.BaseStream.CopyToAsync($ms)
    $errTask = $p.StandardError.ReadToEndAsync()
    if ($cpuLog -ne '')
    {
        # The publish tree's CPU, so a quiet ILC reads as working: dotnet and ilc processes started since this one.
        $t0 = Get-Date
        while (-not $p.WaitForExit($SampleSeconds * 1000))
        {
            $cpu = 0.0
            foreach ($q in @(Get-Process -ErrorAction SilentlyContinue | Where-Object { $_.ProcessName -in @('dotnet', 'ilc') }))
            {
                try { if ($q.StartTime -ge $t0.AddSeconds(-5)) { $cpu += $q.TotalProcessorTime.TotalSeconds } } catch { }
            }
            Add-Content -LiteralPath $cpuLog -Value ("{0:HH:mm:ss} elapsed={1}s cpu-total={2:N0}s" -f (Get-Date), [int]$sw.Elapsed.TotalSeconds, $cpu)
        }
    }
    $p.WaitForExit()
    $outTask.Wait()
    $sw.Stop()
    $bytes = $ms.ToArray()
    if ($logPath -ne '')
    {
        [IO.File]::WriteAllBytes($logPath, $bytes)
        [IO.File]::AppendAllText($logPath, $errTask.Result)
    }
    return @{ Code = $p.ExitCode; Seconds = $sw.Elapsed.TotalSeconds; Stdout = $bytes; Stderr = $errTask.Result }
}

# Twin methods are named with TwinGenerator.Suffix (U+A78F then 'twin'). Count that name's UTF-8 bytes in the metadata
# of every go.*.dll beside the consumer: the converted standard library's assemblies. golib is excluded by rule from
# twinning but carries the name in its own support code (6 occurrences at 317b273e58), so it is not counted.
$script:twinNeedle = [Text.Encoding]::Latin1.GetString([Text.Encoding]::UTF8.GetBytes([string][char]0xA78F + 'twin'))
function Count-TwinNames([string] $dir)
{
    $names = 0; $assemblies = 0
    foreach ($f in @(Get-ChildItem -LiteralPath $dir -Filter 'go.*.dll' -File))
    {
        $assemblies++
        $text = [Text.Encoding]::Latin1.GetString([IO.File]::ReadAllBytes($f.FullName))
        $i = 0
        while (($i = $text.IndexOf($script:twinNeedle, $i, [StringComparison]::Ordinal)) -ge 0) { $names++; $i += $script:twinNeedle.Length }
    }
    return @{ Names = $names; Assemblies = $assemblies }
}

function Get-Sha16([byte[]] $b) { return [BitConverter]::ToString([Security.Cryptography.SHA256]::HashData($b)).Replace('-', '').Substring(0, 16) }

$script:firstStdout = $null
function Measure-Workload([string] $arm, [string] $legName, [string] $exe, [string[]] $prefix)
{
    $walls = New-Object System.Collections.Generic.List[double]
    for ($i = 0; $i -le $Runs; $i++)
    {
        $r = Invoke-Child $exe ($prefix + $progArgs) $WorkDir '' ''
        if ($r.Code -ne 0)
        {
            [IO.File]::WriteAllText((Join-Path $OutDir "$arm.$legName.failed-run.stderr.txt"), $r.Stderr)
            Fail "arm $arm ${legName} run $i exit $($r.Code); stderr in $arm.$legName.failed-run.stderr.txt"
        }
        if ($null -eq $script:firstStdout)
        {
            $script:firstStdout = $r.Stdout
            [IO.File]::WriteAllBytes((Join-Path $OutDir 'first-stdout.txt'), $r.Stdout)
            if ($null -ne $expectedBytes -and -not [Linq.Enumerable]::SequenceEqual($r.Stdout, $expectedBytes)) { Fail "arm $arm ${legName}: stdout differs from -ExpectedStdout (saved as first-stdout.txt)" }
        }
        elseif (-not [Linq.Enumerable]::SequenceEqual($r.Stdout, $script:firstStdout))
        {
            [IO.File]::WriteAllBytes((Join-Path $OutDir "$arm.$legName.diverged-stdout.txt"), $r.Stdout)
            Fail "arm $arm $legName run ${i}: stdout differs from the first run's (saved beside first-stdout.txt)"
        }
        if ($i -gt 0) { $walls.Add($r.Seconds) }
    }
    $med = Get-Median $walls.ToArray()
    Write-Host ("   {0} {1}: median {2:N3}s over {3} runs ({4})" -f $arm, $legName, $med, $walls.Count, (($walls | ForEach-Object { $_.ToString('N3', $inv) }) -join ' '))
    return @{ Median = $med; Walls = $walls.ToArray() }
}

function Count-Warnings([string[]] $logs)
{
    $counts = @()
    foreach ($id in $warningIds)
    {
        $n = 0
        foreach ($l in $logs) { if (Test-Path -LiteralPath $l) { $n += @(Select-String -LiteralPath $l -Pattern "\b$id\b" -AllMatches | ForEach-Object { $_.Matches.Count } | Measure-Object -Sum).Sum } }
        $counts += [int]$n
    }
    return ,$counts
}

$sha   = (& git -C $RepoRoot rev-parse HEAD).Trim()
$dirty = @(& git -C $RepoRoot status --porcelain --untracked-files=no).Count
$sdk   = @(& dotnet --version 2>$null)
$sdk   = if ($sdk.Count -gt 0) { $sdk[0] } else { '?' }
$head  = "row16c: leg=$Leg tree=$RepoRoot sha=$sha tracked-changes=$dirty consumer=$ConsumerRoot project=$Project cores=$([Environment]::ProcessorCount) runs=$Runs aot=$(-not $NoAot) GoTargetOS=$GoTargetOS rid=$rid host=$([Runtime.InteropServices.RuntimeInformation]::OSDescription.Trim()) sdk=$sdk"
Write-Host $head
if ($dirty -ne 0) { Write-Host "row16c: WARNING the tree has $dirty tracked change(s): the readings name a SHA the tree does not match" -ForegroundColor Yellow }

function Invoke-Arm([string] $arm, [bool] $twinned)
{
    $free = Get-FreeGB $RepoRoot
    if ($free -lt $DiskFloorGB) { Fail "arm ${arm}: $free GB free, the floor is $DiskFloorGB GB" }
    Write-Host ''
    Write-Host "== arm ${arm}: twinned=$twinned free=${free}GB $(Get-Date -Format 'HH:mm:ss')"
    Clear-BuildOutput

    if ($twinned) { [Environment]::SetEnvironmentVariable($twinName, $twinValue, 'Process') }
    try
    {
        $jitLog = Join-Path $OutDir "$arm.jit-build.log"
        $b = Invoke-Child 'dotnet' $buildArgs $WorkDir $jitLog ''
        Write-Host ("   {0} JIT build: exit {1} in {2:N0}s" -f $arm, $b.Code, $b.Seconds)
        if ($b.Code -ne 0) { Fail "arm ${arm}: JIT build exit $($b.Code); read $jitLog" }
        $tp = Invoke-Child 'dotnet' $pathArgs $WorkDir '' ''
        $dll = [Text.Encoding]::UTF8.GetString($tp.Stdout).Trim()
        if ($tp.Code -ne 0 -or -not (Test-Path -LiteralPath $dll)) { Fail "arm ${arm}: cannot read TargetPath ('$dll')" }
        # The arm's standard library is what the flag says it is: twin methods present in the twins arm, absent in
        # the plain arms. Without this the twins arm rests on the switch alone (R's point, 2026-11-27).
        $twinNames = Count-TwinNames (Split-Path -Parent $dll)
        Write-Host "   $arm twin method names in go.*.dll: $($twinNames.Names) across $($twinNames.Assemblies) assemblies"
        if ($twinNames.Assemblies -eq 0) { Fail "arm ${arm}: no go.*.dll beside $dll to check for twins" }
        # A plain standard library is not twin-free: its hand-written park points (sync's Lock, time's Sleep, and the
        # other packages' references to them) carry the name, 11 in the hello closure at 317b273e58. So plainA sets the
        # baseline, plainB must match it, and the twins arm must exceed it.
        if ($arm -eq 'plainA') { $script:plainTwinNames = $twinNames.Names }
        elseif (-not $twinned -and $twinNames.Names -ne $script:plainTwinNames) { Fail "arm ${arm}: $($twinNames.Names) twin names, plainA built $($script:plainTwinNames); two plain builds of one tree differ" }
        elseif ($twinned -and $twinNames.Names -le $script:plainTwinNames) { Fail "arm ${arm}: the twins arm built $($twinNames.Names) twin names, no more than plain's $($script:plainTwinNames); -TwinArm '$TwinArm' did not reach the standard library build" }
        $jit = Measure-Workload $arm 'JIT' 'dotnet' @($dll)

        $aot = $null; $aotBytes = $null; $aotSecs = $null; $logs = @($jitLog)
        if (-not $NoAot)
        {
            $aotDir = Join-Path $OutDir "$arm/aot"
            $aotLog = Join-Path $OutDir "$arm.aot-publish.log"
            $logs += $aotLog
            $pb = Invoke-Child 'dotnet' ($aotCommandArgs + @('-o', $aotDir)) $WorkDir $aotLog (Join-Path $OutDir "$arm.aot-cpu.log")
            $aotSecs = [int]$pb.Seconds
            Write-Host ("   {0} AOT publish: exit {1} in {2:N0}s" -f $arm, $pb.Code, $pb.Seconds)
            if ($pb.Code -ne 0) { Fail "arm ${arm}: AOT publish exit $($pb.Code); read $aotLog" }
            $exe = Join-Path $aotDir ([IO.Path]::GetFileNameWithoutExtension($dll) + $ExeSuffix)
            if (-not (Test-Path -LiteralPath $exe)) { Fail "arm ${arm}: no AOT executable at $exe" }
            $aotBytes = (Get-Item -LiteralPath $exe).Length
            $aot = Measure-Workload $arm 'AOT' $exe @()
        }
    }
    finally
    {
        if ($twinned) { [Environment]::SetEnvironmentVariable($twinName, $null, 'Process') }
    }

    $x = [ordered]@{
        leg = $Leg; arm = $arm; sha = $sha; twinned = $twinned; runs = $Runs; noAot = [bool]$NoAot
        jit = $jit.Median; jitWalls = $jit.Walls; jitBuildSeconds = [int]$b.Seconds
        twinNames = $twinNames.Names; twinAssemblies = $twinNames.Assemblies; publishArgs = $PublishArgs
        aot = if ($null -eq $aot) { $null } else { $aot.Median }; aotWalls = if ($null -eq $aot) { @() } else { $aot.Walls }
        aotPublishSeconds = $aotSecs; aotBytes = $aotBytes
        stdoutSha16 = (Get-Sha16 $script:firstStdout); warnings = (Count-Warnings $logs)
    }
    $x | ConvertTo-Json -Depth 4 | Set-Content -LiteralPath (Join-Path $OutDir "$arm.json")
    return $x
}

$arms = @{}
$arms['plainA'] = Invoke-Arm 'plainA' $false
$arms['plainB'] = Invoke-Arm 'plainB' $false
$arms['twins']  = Invoke-Arm 'twins'  $true
# Round-trip through the arm files, so a run and -ScoreFrom score exactly the same data.
foreach ($a in $armNames) { $arms[$a] = Get-Content -Raw -LiteralPath (Join-Path $OutDir "$a.json") | ConvertFrom-Json }
exit (Write-Readings $arms $OutDir $head)
