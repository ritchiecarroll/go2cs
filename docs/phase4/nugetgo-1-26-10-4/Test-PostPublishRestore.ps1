<#
.SYNOPSIS
    Post-publish smoke for the nugetgo wave on go.* 1.26.10.4: a consumer restore AND build of every package the
    four PIN sheets publish, from nuget.org, next to go.fmt at the release, plus the break-floor control. One
    command, run after S4's last push:

        pwsh -NoProfile -File docs/phase4/nugetgo-1-26-10-4/Test-PostPublishRestore.ps1

.DESCRIPTION
    The packages, their versions and the release are read from the sheets beside this script (each sheet's block 1
    `$Release` line and its `$Kits` rows), never listed here. For each package it restores two throwaway net10.0
    consumer projects under a fresh temp folder, each with a nuget.config that clears every source but -Source and
    an empty, private packages folder:
      [alone]   the package at its exact version and nothing else. NuGet resolves the lowest go.* versions the
                package's own ranges allow, so this reads the package's floors: built on the release, they ARE it.
      [go.fmt]  the package beside go.fmt at exactly the release -- the consumer shape of P2's audit findings #1
                and #2, where a uuid package built on go.* 1.26.10.2 beside go.fmt 1.26.10.3 fails with NU1107.

    A row PASSES when both restores exit 0 and, in each, the package resolves at its sheet version, every go.*
    package in the restored graph resolves at exactly the release, and every other nugetgo package in the graph
    (cobra's pflag and mousetrap) resolves at its own sheet version; and the [go.fmt] consumer then BUILDS, exit 0
    and no GO2CS3002. The build is the half a restore cannot stand in for: go.lib's break-floor check
    (GoCheckBreakFloor) runs before CoreCompile, so a package built below the floor restores cleanly and is refused
    only at build. A version nuget.org does not list yet reads NOT-PUBLISHED, which is not a pass: nuget.org can take
    minutes to list a pushed version, so a NOT-PUBLISHED row soon after a push is re-run, and one an hour later is a
    finding.

    The floor control (the last row, '<kit>-floor') is the build's red arm. It takes the -FloorControl kit's PREVIOUS
    revision (the sheet's Rev less one; uuid 1.6.0.3 for this wave), built on the previous release (the release less
    one in its last part; go.* 1.26.10.3), and references it beside go.fmt at the release. It PASSES only when all of
    these hold, so a floor that stopped refusing turns the smoke red:
      [restore] exits 0 (the floor is not a restore check);
      [build]   fails, with exactly one distinct per-package `error GO2CS3002` line, and that line names the package,
                its version and the previous release; and the consumer's assembly is not written;
      [opt-out] the same build with -p:GoAllowBelowBreakFloor=true exits 0, with `warning GO2CS3002` naming it.

    Exit code = the number of rows that did not pass, the control's included.

.PARAMETER FloorControl
    The kit whose previous revision is the floor control. Defaults to uuid.
.PARAMETER NoFloorControl
    Skips the floor control (a rehearsal against a local feed that holds no earlier revision, for one).
.PARAMETER Version
    Overrides a package's version, as name=version (the sheet's kit name). The red control is the previous wave's
    revision of uuid, which reproduces the audit finding while the release's go.* packages are live:
        -Version uuid=<the revision nuget.org lists before this wave's>
.PARAMETER Only
    Runs only the named kits (the sheet's kit names).
.PARAMETER Source
    The package sources, every other source cleared. Defaults to nuget.org alone. A local folder feed of the kits'
    release packages placed before nuget.org rehearses the smoke before anything is pushed:
        -Source <folder>, https://api.nuget.org/v3/index.json
    The NOT-PUBLISHED pre-check runs only when nuget.org is the one source.
.PARAMETER Keep
    Keeps the temp folder (the projects, their restore logs and assets files) and prints its path.
#>
[CmdletBinding()]
param(
    [string[]]$Version = @(),
    [string[]]$Only = @(),
    [string[]]$Source = @('https://api.nuget.org/v3/index.json'),
    [string]$TargetFramework = 'net10.0',
    [string]$CoReference = 'go.fmt',
    [string]$FloorControl = 'uuid',
    [switch]$NoFloorControl,
    [switch]$Keep
)
$ErrorActionPreference = 'Stop'
# `pwsh -File` hands a comma list to a [string[]] parameter as ONE string, so each list is split here.
$Version = @($Version | ForEach-Object { $_ -split ',' } | Where-Object { $_ })
$Only    = @($Only    | ForEach-Object { $_ -split ',' } | Where-Object { $_ })
$Source  = @($Source  | ForEach-Object { $_ -split ',' } | Where-Object { $_ })

if ((dotnet --version) -notlike '10.*') { throw 'dotnet is not .NET 10: point DOTNET_ROOT and PATH at a .NET 10 SDK' }

# ---- the packages, from the sheets -------------------------------------------------------------------------------
$sheets = @(Get-ChildItem -LiteralPath $PSScriptRoot -Filter 'S?-PIN-SHEET.md' | Sort-Object Name)
if ($sheets.Count -ne 4) { throw "expected the four PIN sheets beside this script, found $($sheets.Count)" }
$releases = @()
$kits = [ordered]@{}
$kitRe = "Name = '(?<name>[^']+)';\s*Base = '(?<base>[^']+)';\s*Rev = '(?<rev>[0-9]+)';\s*Id = '(?<id>[^']+)'"
foreach ($s in $sheets) {
    $text = [IO.File]::ReadAllText($s.FullName)
    $rel = [regex]::Matches($text, "\`$Release\s*=\s*'([^']+)'") | ForEach-Object { $_.Groups[1].Value } | Select-Object -Unique
    if (@($rel).Count -ne 1) { throw "$($s.Name): expected one `$Release value, read [$(@($rel) -join ', ')]" }
    $releases += $rel
    $rows = [regex]::Matches($text, $kitRe)
    if ($rows.Count -lt 1) { throw "$($s.Name): no kit rows read" }
    foreach ($m in $rows) {
        $name = $m.Groups['name'].Value
        if ($kits.Contains($name)) { throw "kit '$name' appears on two sheets" }
        $pv = if ([int]$m.Groups['rev'].Value) { "$($m.Groups['base'].Value).$($m.Groups['rev'].Value)" } else { $m.Groups['base'].Value }
        $kits[$name] = [ordered]@{ Name = $name; Id = $m.Groups['id'].Value; Version = $pv; Sheet = $s.BaseName.Substring(0, 2)
                           Base = $m.Groups['base'].Value; Rev = [int]$m.Groups['rev'].Value }
    }
}
$Release = @($releases | Select-Object -Unique)
if ($Release.Count -ne 1) { throw "the sheets disagree on the release: $($Release -join ', ')" }
$Release = $Release[0]

$sheetVersionOf = @{}
foreach ($k in $kits.Values) { $sheetVersionOf[$k.Id.ToLowerInvariant()] = $k.Version }
foreach ($o in $Version) {
    if ($o -notmatch '^([^=]+)=(.+)$') { throw "-Version '$o' is not name=version" }
    if (-not $kits.Contains($Matches[1])) { throw "-Version names '$($Matches[1])', which no sheet carries" }
    $kits[$Matches[1]].Version = $Matches[2]
    $kits[$Matches[1]].Overridden = $true
}
foreach ($n in $Only) { if (-not $kits.Contains($n)) { throw "-Only names '$n', which no sheet carries" } }
$run = @($kits.Values | Where-Object { $Only.Count -eq 0 -or $Only -contains $_.Name })

# The floor control: the kit's previous revision, built on the previous release (the release less one in its last part).
$control = $null
if (-not $NoFloorControl) {
    if (-not $kits.Contains($FloorControl)) { throw "-FloorControl names '$FloorControl', which no sheet carries" }
    $c = $kits[$FloorControl]
    if ($c.Rev -lt 1) { throw "-FloorControl $FloorControl is at its first revision ($($c.Version)): it has no previous revision" }
    $parts = $Release.Split('.')
    $parts[-1] = [string]([int]$parts[-1] - 1)
    $control = [ordered]@{ Name = "$($c.Name)-floor"; Id = $c.Id; Sheet = $c.Sheet; BuiltOn = $parts -join '.'
                           Version = if ($c.Rev -gt 1) { "$($c.Base).$($c.Rev - 1)" } else { $c.Base } }
}

Write-Host "release $Release, $($run.Count) of $($kits.Count) package(s) from $(($sheets | ForEach-Object BaseName) -join ', '), source $($Source -join ' + ')"

# ---- one consumer restore per package ----------------------------------------------------------------------------
$work = Join-Path ([IO.Path]::GetTempPath()) ("nugetgo-smoke-$Release-$PID-" + [DateTime]::UtcNow.ToString('yyyyMMddHHmmss'))
New-Item -ItemType Directory $work | Out-Null
# Empty MSBuild and central-package files stop the walk up the folder tree at this folder.
foreach ($f in 'Directory.Build.props', 'Directory.Build.targets') { Set-Content -LiteralPath (Join-Path $work $f) -Value '<Project />' }
Set-Content -LiteralPath (Join-Path $work 'Directory.Packages.props') -Value '<Project><PropertyGroup><ManagePackageVersionsCentrally>false</ManagePackageVersionsCentrally></PropertyGroup></Project>'
$isNugetOrg = $Source.Count -eq 1 -and $Source[0] -like 'https://api.nuget.org/*'
$results = @()

# One consumer project in one SHAPE: 'alone' references only the package, so NuGet resolves the lowest version its
# own go.* ranges allow -- the release, if the package was built on it; any other shape names a go.* package
# referenced beside it at exactly the release, the audit's shape.
function Invoke-ConsumerRestore($k, [string]$shape, [switch]$Lenient) {
    $dir = Join-Path $work "$($k.Name)-$shape"
    $packages = Join-Path $dir 'packages'
    New-Item -ItemType Directory $dir, $packages | Out-Null
    @"
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <clear />
$(($Source | ForEach-Object -Begin { $i = 0 } -Process { $i++; "    <add key=`"smoke$i`" value=`"$_`" />" }) -join "`n")
  </packageSources>
</configuration>
"@ | Set-Content -LiteralPath (Join-Path $dir 'nuget.config')
    $beside = if ($shape -ne 'alone') { "`n    <PackageReference Include=`"$shape`" Version=`"[$Release]`" />" } else { '' }
    @"
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>$TargetFramework</TargetFramework>
    <OutputType>Exe</OutputType>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="$($k.Id)" Version="[$($k.Version)]" />$beside
  </ItemGroup>
</Project>
"@ | Set-Content -LiteralPath (Join-Path $dir 'consumer.csproj')
    Set-Content -LiteralPath (Join-Path $dir 'Program.cs') -Value 'System.Console.WriteLine("consumer");'

    $log = Join-Path $dir 'restore.log'
    $env:NUGET_PACKAGES = $packages
    dotnet restore (Join-Path $dir 'consumer.csproj') --configfile (Join-Path $dir 'nuget.config') --packages $packages --no-cache *> $log
    $rc = $LASTEXITCODE
    Remove-Item Env:NUGET_PACKAGES
    $codes = @([regex]::Matches((Get-Content -Raw -LiteralPath $log), '\b(NU1\d{3})\b') | ForEach-Object { $_.Value } | Sort-Object -Unique)
    if ($rc -ne 0) {
        $detail = "restore exited $rc$(if ($codes) { ': ' + ($codes -join ', ') })"
        $first = Get-Content -LiteralPath $log | Where-Object { $_ -match 'error NU1' } | Select-Object -First 1
        if ($first) { $detail += ' -- ' + ($first -replace '^.*?error ', '' -replace '\s*\[.*$', '') }
        return [pscustomobject]@{ Shape = $shape; Ok = $false; Detail = $detail; Dir = $dir }
    }

    $assets = Get-Content -Raw -LiteralPath (Join-Path $dir 'obj/project.assets.json') | ConvertFrom-Json
    $libs = @($assets.libraries.PSObject.Properties | Where-Object { $_.Value.type -eq 'package' } | ForEach-Object {
        $p = $_.Name -split '/', 2; [pscustomobject]@{ Id = $p[0].ToLowerInvariant(); Version = $p[1] } })
    $wrong = @()
    $self = @($libs | Where-Object Id -eq $k.Id.ToLowerInvariant())
    if ($self.Count -ne 1 -or $self[0].Version -ne $k.Version) { $wrong += "$($k.Id) resolved [$(($self | ForEach-Object Version) -join ', ')]" }
    $goPkgs = @($libs | Where-Object { $_.Id -like 'go.*' })
    $off = @($goPkgs | Where-Object Version -ne $Release)
    if ($off.Count) { $wrong += "$($off.Count) go.* off $Release (e.g. $($off[0].Id) $($off[0].Version))" }
    $wrong += @($libs | Where-Object { $_.Id -like 'nugetgo.*' -and $_.Id -ne $k.Id.ToLowerInvariant() -and $sheetVersionOf.ContainsKey($_.Id) -and $sheetVersionOf[$_.Id] -ne $_.Version } |
        ForEach-Object { "$($_.Id) $($_.Version), sheet $($sheetVersionOf[$_.Id])" })
    if ($goPkgs.Count -eq 0) { $wrong += 'no go.* package in the restored graph' }
    if ($Lenient) {
        # The floor control reads the graph rather than judging it: the build decides.
        $note = if ($wrong.Count) { 'graph: ' + ($wrong -join '; ') } else { "$($goPkgs.Count) go.* at $Release" }
        return [pscustomobject]@{ Shape = $shape; Ok = $true; Detail = $note; Dir = $dir }
    }
    if ($wrong.Count) { return [pscustomobject]@{ Shape = $shape; Ok = $false; Detail = 'off the release: ' + ($wrong -join '; '); Dir = $dir } }
    return [pscustomobject]@{ Shape = $shape; Ok = $true; Detail = "$($goPkgs.Count) go.* at $Release$(if ($codes) { '; warnings ' + ($codes -join ', ') })"; Dir = $dir }
}

# A build of a restored consumer, with its GO2CS3002 lines: each distinct per-package line once (MSBuild prints an
# error inline and again in its summary), the closing "The build stops here" line left out.
function Invoke-ConsumerBuild([string]$dir, [string]$label, [string[]]$extra = @()) {
    $log = Join-Path $dir "build-$label.log"
    $env:NUGET_PACKAGES = Join-Path $dir 'packages'
    dotnet build (Join-Path $dir 'consumer.csproj') --no-restore -c Release -nologo -nodeReuse:false @extra *> $log
    $rc = $LASTEXITCODE
    Remove-Item Env:NUGET_PACKAGES
    $floor = @([regex]::Matches((Get-Content -Raw -LiteralPath $log), '(?m)\b(error|warning) GO2CS3002: (.+?)(?:\s+\[[^\]\r\n]*\])?\r?$') |
        Where-Object { $_.Groups[2].Value -notlike 'The build stops here*' } |
        ForEach-Object { [pscustomobject]@{ Kind = $_.Groups[1].Value; Text = $_.Groups[2].Value.Trim() } } |
        Sort-Object Kind, Text -Unique)
    $assembly = @(Get-ChildItem -LiteralPath (Join-Path $dir 'bin') -Recurse -File -Filter 'consumer.dll' -ErrorAction SilentlyContinue).Count -gt 0
    $first = Get-Content -LiteralPath $log | Where-Object { $_ -match ': error ' } | Select-Object -First 1
    [pscustomobject]@{ Rc = $rc; Floor = $floor; Assembly = $assembly
                       FirstError = if ($first) { $first -replace '^.*?: error ', '' -replace '\s*\[[^\]]*\]\s*$', '' } else { '' } }
}

# The floor control's two builds of its restored consumer, judged: [build] refused with exactly one per-package
# GO2CS3002 naming the package, its version and the release it was built on, and no assembly written; [opt-out]
# built, with that diagnostic as a warning.
function Test-FloorControl($k, [string]$dir) {
    $named = "$($k.Id) $($k.Version) was built on go.* $($k.BuiltOn)"
    $parts = @()
    $b = Invoke-ConsumerBuild $dir 'floor'
    $errs = @($b.Floor | Where-Object Kind -eq 'error')
    $refused = $b.Rc -ne 0 -and $errs.Count -eq 1 -and $errs[0].Text.StartsWith($named, [StringComparison]::OrdinalIgnoreCase) -and -not $b.Assembly
    $parts += if ($refused) { "[build] refused: one GO2CS3002, $named" }
              else { "[build] NOT refused as expected: exit $($b.Rc), $($errs.Count) per-package GO2CS3002 error(s)$(if ($errs) { ' (' + (($errs | Select-Object -First 3 | ForEach-Object { $_.Text -replace ',.*$', '' }) -join '; ') + $(if ($errs.Count -gt 3) { '; ...' }) + ')' } elseif ($b.FirstError) { ', first error: ' + $b.FirstError })$(if ($b.Assembly) { ', assembly written' })" }
    $o = Invoke-ConsumerBuild $dir 'optout' @('-p:GoAllowBelowBreakFloor=true')
    $warned = @($o.Floor | Where-Object { $_.Kind -eq 'warning' -and $_.Text.StartsWith($named, [StringComparison]::OrdinalIgnoreCase) }).Count -gt 0
    $built = $o.Rc -eq 0 -and $warned
    $parts += if ($built) { '[opt-out] built, GO2CS3002 as a warning' }
              else { "[opt-out] exit $($o.Rc), $(if ($warned) { 'warned' } else { 'no GO2CS3002 warning naming it' })$(if ($o.FirstError) { ', first error: ' + $o.FirstError })" }
    [pscustomobject]@{ Ok = $refused -and $built; Parts = $parts }
}

foreach ($k in $run) {
    $row = [ordered]@{ Kit = $k.Name; Sheet = $k.Sheet; Package = $k.Id; Version = $k.Version; Verdict = ''; Detail = '' }
    if ($k.Overridden) { $row.Version = "$($k.Version) (override)" }

    if ($isNugetOrg) {
        $listed = @()
        try { $listed = @((Invoke-RestMethod "https://api.nuget.org/v3-flatcontainer/$($k.Id.ToLowerInvariant())/index.json").versions) } catch { $listed = @() }
        if ($listed -notcontains $k.Version) {
            $row.Verdict = 'NOT-PUBLISHED'
            $row.Detail = if ($listed.Count) { "nuget.org lists $($listed[-1]) last" } else { 'nuget.org lists no version of this id' }
            $results += [pscustomobject]$row; continue
        }
    }

    $verdicts = @(foreach ($shape in 'alone', $CoReference) { Invoke-ConsumerRestore $k $shape })
    $beside = $verdicts[-1]
    if ($beside.Ok) {
        $b = Invoke-ConsumerBuild $beside.Dir 'release'
        $verdicts += if ($b.Rc -eq 0 -and $b.Floor.Count -eq 0) { [pscustomobject]@{ Shape = 'build'; Ok = $true; Detail = 'built, no GO2CS3002' } }
                     elseif ($b.Floor.Count) { [pscustomobject]@{ Shape = 'build'; Ok = $false; Detail = "exit $($b.Rc): " + (($b.Floor | ForEach-Object { "$($_.Kind) GO2CS3002 $($_.Text -replace ',.*$', '')" }) -join '; ') } }
                     else { [pscustomobject]@{ Shape = 'build'; Ok = $false; Detail = "exit $($b.Rc)$(if ($b.FirstError) { ': ' + $b.FirstError })" } }
    }
    $failed = @($verdicts | Where-Object { -not $_.Ok })
    $row.Verdict = if ($failed.Count) { 'FAIL' } else { 'PASS' }
    $row.Detail = ($verdicts | ForEach-Object { "[$($_.Shape)] $($_.Detail)" }) -join '  '
    $results += [pscustomobject]$row
}

if ($control) {
    $k = $control
    $row = [ordered]@{ Kit = $k.Name; Sheet = $k.Sheet; Package = $k.Id; Version = "$($k.Version) (go.* $($k.BuiltOn))"; Verdict = ''; Detail = '' }
    $listed = @()
    if ($isNugetOrg) { try { $listed = @((Invoke-RestMethod "https://api.nuget.org/v3-flatcontainer/$($k.Id.ToLowerInvariant())/index.json").versions) } catch { $listed = @() } }
    if ($isNugetOrg -and $listed -notcontains $k.Version) {
        $row.Verdict = 'NOT-PUBLISHED'; $row.Detail = "nuget.org does not list $($k.Version), the control's revision"
    } else {
        $r = Invoke-ConsumerRestore $k $CoReference -Lenient
        $j = if ($r.Ok) { Test-FloorControl $k $r.Dir } else { [pscustomobject]@{ Ok = $false; Parts = @() } }
        $ok = $r.Ok -and $j.Ok
        $parts = @("[restore] $($r.Detail)") + $j.Parts
        $row.Verdict = if ($ok) { 'PASS' } else { 'FAIL' }
        $row.Detail = $parts -join '  '
    }
    $results += [pscustomobject]$row
}

$results | Format-Table Kit, Sheet, Package, Version, Verdict, Detail -AutoSize -Wrap | Out-String -Width 220 | Write-Host
$notPass = @($results | Where-Object Verdict -ne 'PASS').Count
Write-Host "$($results.Count - $notPass) of $($results.Count) PASS"
if ($Keep) { Write-Host "kept: $work" } else { Remove-Item -Recurse -Force -LiteralPath $work }
exit $notPass
