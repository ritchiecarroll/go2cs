<#
.SYNOPSIS
    M:N row 16b: thresholds T1 (size) and T2 (build) of docs/phase4/DESIGN-mn-stdlib-twins.md section 4, on one tree.

.DESCRIPTION
    T1 asks how much larger the standard library's assemblies are when built with twins; T2 how much longer the
    standard library takes to build. Both are read from the same three clean builds of src/go2cs-stdlib.slnx:

        plainA   the standard library plain                       with plainB, the A/A pair (wall band; byte band,
        plainB   the standard library plain, again                 which a deterministic build makes 0)
        twins    the standard library twinned, by the environment variable the caller names (-TwinArm)

    Each arm, in order:
      1. checks the disk floor, then deletes bin, obj and Generated under this tree's src/core and src/gen (the
         analyzer is built inside every arm too, the same in all three);
      2. `dotnet build <solution> -c Release -p:GoTargetOS=<os> --no-incremental -p:UseSharedCompilation=false
         -nodeReuse:false`, timed from process start to exit, output in <arm>.build.log;
      3. reads each solution member's OWN assembly: for every core/ project in src/go2cs-stdlib.slnx, the file
         <project dir>/bin/Release/<tfm>/<AssemblyName>.dll (never a copy in another project's bin). Its byte count,
         and the number of twin method names in it (the generator's suffix, U+A78F then 'twin');
      4. asserts the arm is what the flag says: plainA's twin-name count is the baseline (a plain standard library
         carries a handful of hand-written park-point twins), plainB must equal it, the twins arm must exceed it;
         and every arm must produce the same set of assemblies;
      5. counts GO2CS0017, GO2CS0018 and GO2CS0021 in the build log (a gate: 0 in every arm);
      6. writes <arm>.json, including every assembly's bytes.

    The readings, row16b-<os>-readings.md, give the walls and bytes per arm, the A/A bands, the ratios, T1's and T2's
    verdicts, and the ten assemblies that grew most. The template is docs/phase4/DATA-mn-row16b-template.md.
    -ScoreFrom re-scores saved arm files without building anything.

    T1 (size, section 4): MISSED when summed twins bytes / plain mean - 1 > 0.08. Size has no noise to subtract: a
    nonzero byte A/A band is itself reported as a finding (the build is not deterministic) before T1 is read.
    T2 (build, section 4, "every time threshold is beyond the A/A band"): MISSED when twins wall / plain mean wall,
    less the wall A/A band, exceeds 1.8.
    A miss stops for the owner: the script names it and exits 2.

    Why -TwinArm is an argument: the standard library twin switch, spelled turned on, may appear in no tracked script
    (repoguard TestStdlibTwinsDefaultOff), so the command line that turns it on is the caller's, from the template.

.PARAMETER TwinArm
    NAME=VALUE: the environment variable set for the twins arm only, and removed after it. The value is in
    docs/phase4/DATA-mn-row16b-template.md.

.PARAMETER GoTargetOS
    windows, linux or darwin. Default: the host's. Read windows first; each other target is a separate invocation
    (a GoTargetOS switch changes the compile item set, and every arm purges, so nothing carries across).

.PARAMETER Solution
    What each arm builds. Default src/go2cs-stdlib.slnx, which is row 16b. Any other value (one package's .csproj, for
    a rehearsal of the kit) is marked REHEARSAL in the readings. The assemblies measured are always the stdlib
    solution's members that the build produced.

.PARAMETER OutDir
    Where logs, arm files and readings go. Default: a new directory under the system temp directory.

.PARAMETER DiskFloorGB
    Free space required before every arm (default 25).

.PARAMETER DryRun
    Print every command each arm would run, in order, and exit 0. Nothing is deleted or built.

.PARAMETER ScoreFrom
    A directory holding plainA.json, plainB.json and twins.json from an earlier run: score them and exit.

.EXAMPLE
    ./run-row16b-stdlib.ps1 -TwinArm '<from DATA-mn-row16b-template.md>' -GoTargetOS windows -OutDir <dir>
#>
[CmdletBinding()]
param(
    [string] $TwinArm = '',
    [string] $GoTargetOS = '',
    [string] $Solution = '',
    [string] $OutDir = '',
    [int]    $DiskFloorGB = 25,
    [switch] $DryRun,
    [string] $ScoreFrom = ''
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest
. (Join-Path $PSScriptRoot '../../_paths.ps1')

$coreDir = Join-Path $SrcRoot 'core'
$genDir  = Join-Path $SrcRoot 'gen'
$stdlibSlnx = Join-Path $SrcRoot 'go2cs-stdlib.slnx'
$inv = [Globalization.CultureInfo]::InvariantCulture
$armNames = @('plainA', 'plainB', 'twins')
$warningIds = @('GO2CS0017', 'GO2CS0018', 'GO2CS0021')

function Fail([string] $message)
{
    Write-Host "ROW16B FAILURE $message" -ForegroundColor Red
    exit 1
}

# ---- scoring: shared by a full run and -ScoreFrom ----
function Write-Readings([hashtable] $arms, [string] $dir, [string] $headLine)
{
    function Pct($v) { if ($null -eq $v) { return 'n/a' } return (100.0 * [double]$v).ToString('N2', $inv) + '%' }
    function N0($v) { return ([double]$v).ToString('N0', $inv) }

    $os = $arms['plainA'].goTargetOS
    $md = New-Object System.Collections.Generic.List[string]
    $md.Add("# Row 16b readings (T1 size, T2 build), GoTargetOS=$os")
    $md.Add('')
    $md.Add("    $headLine")
    foreach ($a in $armNames)
    {
        $x = $arms[$a]
        $md.Add("    arm ${a}: sha=$($x.sha) twinned=$($x.twinned) build=$($x.buildSeconds)s assemblies=$($x.assemblies) bytes=$($x.bytes) twin-names=$($x.twinNames) golib=$($x.golibBytes) warnings $($warningIds -join '/')=$($x.warnings -join '/')")
    }
    $rehearsal = @($armNames | Where-Object { $arms[$_].rehearsal })
    if ($rehearsal.Count -gt 0) { $md.Add("    REHEARSAL: the arms built '$($arms['plainA'].solution)', not src/go2cs-stdlib.slnx; this is not row 16b.") }
    $md.Add('')

    $misses = New-Object System.Collections.Generic.List[string]
    $findings = New-Object System.Collections.Generic.List[string]

    # T1
    $ba = [double]$arms['plainA'].bytes; $bb = [double]$arms['plainB'].bytes; $bt = [double]$arms['twins'].bytes
    $bMean = ($ba + $bb) / 2.0
    $bBand = [math]::Abs($ba - $bb) / $bMean
    $bRatio = $bt / $bMean
    $t1 = if ($bRatio - 1.0 -gt 0.08) { 'MISSED' } else { 'MET' }
    if ($t1 -eq 'MISSED') { $misses.Add('T1 size') }
    if ($bBand -ne 0) { $findings.Add("the two plain builds' bytes differ by $(N0 ([math]::Abs($ba - $bb))) ($(Pct $bBand)): the build is not deterministic; read T1 with that in mind") }

    # T2
    $wa = [double]$arms['plainA'].buildSeconds; $wb = [double]$arms['plainB'].buildSeconds; $wt = [double]$arms['twins'].buildSeconds
    $wMean = ($wa + $wb) / 2.0
    $wBand = [math]::Abs($wa - $wb) / $wMean
    $wRatio = $wt / $wMean
    $t2 = if ($wRatio - $wBand -gt 1.8) { 'MISSED' } else { 'MET' }
    if ($t2 -eq 'MISSED') { $misses.Add('T2 build') }

    $md.Add('| threshold | plainA | plainB | A/A band | twins | twins / plain mean | bar | verdict |')
    $md.Add('|---|---:|---:|---:|---:|---:|---|---|')
    $md.Add("| T1 size, summed go.*.dll bytes ($($arms['plainA'].assemblies) assemblies) | $(N0 $ba) | $(N0 $bb) | $(Pct $bBand) | $(N0 $bt) | $($bRatio.ToString('N4', $inv)) (+$(Pct ($bRatio - 1.0))) | at most +8% | $t1 |")
    $md.Add("| T2 build wall, seconds | $(N0 $wa) | $(N0 $wb) | $(Pct $wBand) | $(N0 $wt) | $($wRatio.ToString('N3', $inv))x (less band: $(($wRatio - $wBand).ToString('N3', $inv))x) | at most 1.8x beyond A/A | $t2 |")
    $md.Add('')

    # The assemblies that grew most, twins against plainA.
    $pa = @{}; foreach ($p in $arms['plainA'].perAssembly.PSObject.Properties) { $pa[$p.Name] = [double]$p.Value }
    $rows = foreach ($p in $arms['twins'].perAssembly.PSObject.Properties)
    {
        if ($pa.ContainsKey($p.Name) -and $pa[$p.Name] -gt 0) { [pscustomobject]@{ Name = $p.Name; Plain = $pa[$p.Name]; Twins = [double]$p.Value; Delta = [double]$p.Value - $pa[$p.Name] } }
    }
    $md.Add('The ten assemblies that grew most (twins against plainA):')
    $md.Add('')
    $md.Add('| assembly | plainA bytes | twins bytes | delta | growth |')
    $md.Add('|---|---:|---:|---:|---:|')
    foreach ($r in @($rows | Sort-Object Delta -Descending | Select-Object -First 10))
    {
        $md.Add("| $($r.Name) | $(N0 $r.Plain) | $(N0 $r.Twins) | $(N0 $r.Delta) | $(Pct ($r.Twins / $r.Plain - 1.0)) |")
    }
    $md.Add('')

    $warned = @($armNames | Where-Object { ($arms[$_].warnings | Measure-Object -Sum).Sum -gt 0 })
    if ($warned.Count -gt 0) { $md.Add("**GATE FAILED: $($warningIds -join '/') fired in arm(s) $($warned -join ', ')** (must be 0 in every arm).") }
    else { $md.Add("Gate: $($warningIds -join '/') = 0 in every arm.") }
    foreach ($f in $findings) { $md.Add("**FINDING: $f.**") }
    if ($misses.Count -gt 0) { $md.Add("**MISSED: $($misses -join '; ').** Section 4: a miss on T1-T5 stops for the owner. Report it and stop.") }
    else { $md.Add('**T1 and T2 MET** on this target.') }
    $md.Add('')
    $md.Add('Exit: 0 met, 2 a miss or the gate failed.')

    $file = Join-Path $dir "row16b-$os-readings.md"
    [IO.File]::WriteAllLines($file, $md)
    Write-Host ''
    $md | ForEach-Object { Write-Host $_ }
    Write-Host ''
    Write-Host "row16b: readings in $file"
    if ($misses.Count -gt 0 -or $warned.Count -gt 0) { return 2 }
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
if ($TwinArm -notmatch '^([A-Za-z_][A-Za-z0-9_]*)=(.+)$') { Fail "-TwinArm '$TwinArm' is not NAME=VALUE" }
$twinName  = $Matches[1]
$twinValue = $Matches[2]
foreach ($name in @('GO2CS_MN', 'GoTwinsFor', 'go2csPath', $twinName))
{
    if ($null -ne [Environment]::GetEnvironmentVariable($name)) { Fail "$name is set in the environment; every arm must start without it" }
}
if ($GoTargetOS -eq '') { $GoTargetOS = if ($IsWindowsHost) { 'windows' } elseif ($IsMacOS) { 'darwin' } else { 'linux' } }
if ($GoTargetOS -notin @('windows', 'linux', 'darwin')) { Fail "-GoTargetOS '$GoTargetOS' is not windows, linux or darwin" }
if ($Solution -eq '') { $Solution = $stdlibSlnx }
if (-not (Test-Path -LiteralPath $Solution)) { Fail "-Solution '$Solution' does not exist" }
$Solution = (Resolve-Path -LiteralPath $Solution).Path
$isRehearsal = ($Solution -ne (Resolve-Path -LiteralPath $stdlibSlnx).Path)
if ($OutDir -eq '') { $OutDir = Join-Path ([IO.Path]::GetTempPath()) ("row16b-$GoTargetOS-" + (Get-Date -Format 'yyyyMMdd-HHmmss')) }

$buildArgs = @('build', $Solution, '-c', 'Release', "-p:GoTargetOS=$GoTargetOS", '--no-incremental', '-p:UseSharedCompilation=false', '-nodeReuse:false')

function Format-Cmd([string] $exe, [string[]] $a) { return ($exe + ' ' + (($a | ForEach-Object { if ($_ -match '\s') { "`"$_`"" } else { $_ } }) -join ' ')) }

# The measured set: every core/ member of the stdlib solution, by its project directory and AssemblyName.
$members = New-Object System.Collections.Generic.List[object]
foreach ($m in [regex]::Matches([IO.File]::ReadAllText($stdlibSlnx), 'Path="(core/[^"]+\.csproj)"'))
{
    $proj = Join-Path $SrcRoot $m.Groups[1].Value
    if (-not (Test-Path -LiteralPath $proj)) { continue }
    $text = [IO.File]::ReadAllText($proj)
    $asm = if ($text -match '<AssemblyName>([^<]+)</AssemblyName>') { $Matches[1].Trim() } else { [IO.Path]::GetFileNameWithoutExtension($proj) }
    if ($asm -like '*$(*') { continue }
    $members.Add([pscustomobject]@{ Dir = (Split-Path -Parent $proj); Assembly = $asm })
}
if ($members.Count -eq 0) { Fail "no core/ projects read from $stdlibSlnx" }

if ($DryRun)
{
    Write-Host "row16b DRY RUN: tree=$RepoRoot GoTargetOS=$GoTargetOS solution=$Solution$(if ($isRehearsal) { ' (REHEARSAL)' }) members=$($members.Count) out=$OutDir"
    foreach ($a in $armNames)
    {
        $tw = ($a -eq 'twins')
        Write-Host ''
        Write-Host "== arm $a$(if ($tw) { " (env $twinName=$twinValue for the build, removed after)" })"
        Write-Host "  check free disk >= $DiskFloorGB GB"
        Write-Host "  delete bin/obj/Generated under $coreDir and $genDir"
        Write-Host "  $(Format-Cmd 'dotnet' $buildArgs)   > $a.build.log, timed"
        Write-Host "  read <project dir>/bin/Release/<tfm>/<AssemblyName>.dll for $($members.Count) members: bytes and twin names"
        Write-Host "  twin names: $(if ($tw) { 'must exceed plainA''s' } elseif ($a -eq 'plainA') { 'the plain baseline' } else { 'must equal plainA''s' }); assembly set must equal plainA's"
        Write-Host "  count $($warningIds -join '/') in $a.build.log; write $a.json"
    }
    Write-Host ''
    Write-Host "then: score T1 and T2 -> row16b-$GoTargetOS-readings.md (exit 2 on a miss or the gate)"
    exit 0
}

New-Item -ItemType Directory -Force -Path $OutDir | Out-Null
$OutDir = (Resolve-Path $OutDir).Path
if ($OutDir.StartsWith($coreDir) -or $OutDir.StartsWith($genDir)) { Fail '-OutDir must not be under the purged trees' }

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

$tracked = @(& git -C $RepoRoot ls-files -- 'src/core' 'src/gen' | Where-Object { $_ -match '/(bin|obj|Generated)/' })
if ($LASTEXITCODE -ne 0) { Fail "git ls-files failed in $RepoRoot" }
if ($tracked.Count -gt 0) { Fail "$($tracked.Count) tracked file(s) under bin/obj/Generated, e.g. $($tracked[0]); the purge would delete them" }

function Clear-BuildOutput
{
    foreach ($base in @($coreDir, $genDir))
    {
        $dirs = @(Get-ChildItem -LiteralPath $base -Directory -Recurse -Force -ErrorAction SilentlyContinue |
            Where-Object { $_.Name -in @('bin', 'obj', 'Generated') } |
            Sort-Object { $_.FullName.Length })
        foreach ($d in $dirs)
        {
            if (Test-Path -LiteralPath $d.FullName) { Remove-Item -LiteralPath $d.FullName -Recurse -Force }
        }
    }
}

# Twin methods are named with TwinGenerator.Suffix (U+A78F then 'twin'); count that name's UTF-8 bytes in a file.
$script:twinNeedle = [Text.Encoding]::Latin1.GetString([Text.Encoding]::UTF8.GetBytes([string][char]0xA78F + 'twin'))
function Count-TwinNamesIn([string] $file)
{
    $text = [Text.Encoding]::Latin1.GetString([IO.File]::ReadAllBytes($file))
    $n = 0; $i = 0
    while (($i = $text.IndexOf($script:twinNeedle, $i, [StringComparison]::Ordinal)) -ge 0) { $n++; $i += $script:twinNeedle.Length }
    return $n
}

function Read-Assemblies
{
    $per = [ordered]@{}; $bytes = [long]0; $twins = 0
    foreach ($m in $members)
    {
        $hits = @(Get-ChildItem -Path (Join-Path $m.Dir 'bin/Release') -Filter "$($m.Assembly).dll" -Recurse -File -ErrorAction SilentlyContinue |
            Where-Object { $_.Directory.Parent.Name -eq 'Release' })
        if ($hits.Count -eq 0) { continue }
        if ($hits.Count -gt 1) { Fail "$($m.Assembly): $($hits.Count) builds under $($m.Dir)/bin/Release; expected one target framework" }
        $per[$m.Assembly] = $hits[0].Length
        $bytes += $hits[0].Length
        $twins += Count-TwinNamesIn $hits[0].FullName
    }
    return @{ Per = $per; Bytes = $bytes; Twins = $twins }
}

function Count-Warnings([string] $log)
{
    $counts = @()
    foreach ($id in $warningIds) { $counts += [int](@(Select-String -LiteralPath $log -Pattern "\b$id\b" -AllMatches | ForEach-Object { $_.Matches.Count } | Measure-Object -Sum).Sum) }
    return ,$counts
}

$sha   = (& git -C $RepoRoot rev-parse HEAD).Trim()
$dirty = @(& git -C $RepoRoot status --porcelain --untracked-files=no).Count
$sdk   = @(& dotnet --version 2>$null)
$sdk   = if ($sdk.Count -gt 0) { $sdk[0] } else { '?' }
$head  = "row16b: tree=$RepoRoot sha=$sha tracked-changes=$dirty solution=$Solution GoTargetOS=$GoTargetOS members=$($members.Count) cores=$([Environment]::ProcessorCount) host=$([Runtime.InteropServices.RuntimeInformation]::OSDescription.Trim()) sdk=$sdk"
Write-Host $head
if ($dirty -ne 0) { Write-Host "row16b: WARNING the tree has $dirty tracked change(s): the readings name a SHA the tree does not match" -ForegroundColor Yellow }

function Invoke-Arm([string] $arm, [bool] $twinned)
{
    $free = Get-FreeGB $RepoRoot
    if ($free -lt $DiskFloorGB) { Fail "arm ${arm}: $free GB free, the floor is $DiskFloorGB GB" }
    Write-Host ''
    Write-Host "== arm ${arm}: twinned=$twinned free=${free}GB $(Get-Date -Format 'HH:mm:ss')"
    Clear-BuildOutput

    $log = Join-Path $OutDir "$arm.build.log"
    $psi = [Diagnostics.ProcessStartInfo]::new('dotnet')
    $psi.UseShellExecute = $false
    $psi.RedirectStandardOutput = $true
    $psi.RedirectStandardError = $true
    $psi.WorkingDirectory = $SrcRoot
    foreach ($x in $buildArgs) { $psi.ArgumentList.Add($x) }
    $psi.Environment['MSBUILDDISABLENODEREUSE'] = '1'
    if ($twinned) { $psi.Environment[$twinName] = $twinValue }
    $sw = [Diagnostics.Stopwatch]::StartNew()
    $p = [Diagnostics.Process]::Start($psi)
    $outTask = $p.StandardOutput.ReadToEndAsync(); $errTask = $p.StandardError.ReadToEndAsync()
    $p.WaitForExit()
    $sw.Stop()
    [IO.File]::WriteAllText($log, $outTask.Result + $errTask.Result)
    $secs = [math]::Round($sw.Elapsed.TotalSeconds, 1)
    Write-Host "   $arm build: exit $($p.ExitCode) in ${secs}s (log $log)"
    if ($p.ExitCode -ne 0) { Fail "arm ${arm}: build exit $($p.ExitCode); read $log" }

    $r = Read-Assemblies
    Write-Host "   $arm assemblies: $($r.Per.Count), bytes $($r.Bytes), twin names $($r.Twins)"
    if ($r.Per.Count -eq 0) { Fail "arm ${arm}: no member assembly found under src/core/*/bin/Release" }
    if ($arm -eq 'plainA') { $script:plainNames = @($r.Per.Keys); $script:plainTwinNames = $r.Twins }
    else
    {
        $diff = @(Compare-Object $script:plainNames @($r.Per.Keys))
        if ($diff.Count -gt 0) { Fail "arm ${arm}: built a different assembly set from plainA's ($($diff.Count) differ, e.g. $($diff[0].InputObject))" }
        if (-not $twinned -and $r.Twins -ne $script:plainTwinNames) { Fail "arm ${arm}: $($r.Twins) twin names, plainA built $($script:plainTwinNames); two plain builds of one tree differ" }
        if ($twinned -and $r.Twins -le $script:plainTwinNames) { Fail "arm ${arm}: the twins arm built $($r.Twins) twin names, no more than plain's $($script:plainTwinNames); -TwinArm '$TwinArm' did not reach the build" }
    }
    $golib = @(Get-ChildItem -Path (Join-Path $coreDir 'golib/bin/Release') -Filter 'golib.dll' -Recurse -File -ErrorAction SilentlyContinue | Where-Object { $_.Directory.Parent.Name -eq 'Release' })

    $x = [ordered]@{
        arm = $arm; sha = $sha; twinned = $twinned; goTargetOS = $GoTargetOS; solution = $Solution; rehearsal = $isRehearsal
        buildSeconds = $secs; assemblies = $r.Per.Count; bytes = $r.Bytes; twinNames = $r.Twins
        golibBytes = if ($golib.Count -eq 1) { $golib[0].Length } else { $null }
        warnings = (Count-Warnings $log); perAssembly = $r.Per
    }
    $x | ConvertTo-Json -Depth 4 | Set-Content -LiteralPath (Join-Path $OutDir "$arm.json")
}

Invoke-Arm 'plainA' $false
Invoke-Arm 'plainB' $false
Invoke-Arm 'twins'  $true
$arms = @{}
foreach ($a in $armNames) { $arms[$a] = Get-Content -Raw -LiteralPath (Join-Path $OutDir "$a.json") | ConvertFrom-Json }
exit (Write-Readings $arms $OutDir $head)
