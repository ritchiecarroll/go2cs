<#
.SYNOPSIS
    Table-driven arms for GoApiCompat.psm1, the binary-compatibility guard push-nuget.ps1 and nugetgo-pack.ps1 run: a
    member removed from the public surface must read red and be named, the same surface restored must read green, and
    a comparison the guard did not actually make must never read green. Exit code = the number of failed cases.

.DESCRIPTION
    The negative control the guard's ruling requires (COORD, 2026-11-07, condition (b)): the sizing's M2/M3 shape.
    GoTypeOps<T>.Instance made internal read red and named that one member; restored, it read green. Here the member
    is Fixture.Widget.Instance in testdata/apicompat (built by build-apicompat-fixtures.ps1), packed into throwaway
    .nupkg files with one lib/ asset and one runtimes/ asset, so every arm runs the guard as push-nuget.ps1 does: the
    pinned tool from the repository's manifest, one process per asset.
#>
$ErrorActionPreference = 'Stop'
Import-Module (Join-Path $PSScriptRoot 'GoApiCompat.psm1') -Force
Add-Type -AssemblyName System.IO.Compression
Add-Type -AssemblyName System.IO.Compression.FileSystem
$failed = 0
$ran = 0
function Check([string]$name, [bool]$ok, [string]$detail) {
    $script:ran++
    if ($ok) { Write-Host "  ok    $name" } else { $script:failed++; Write-Host "  FAIL  $name -- $detail" -ForegroundColor Red }
}

$repoRoot = [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../../..'))
$fixtures = Join-Path $PSScriptRoot 'testdata/apicompat'
$work = Join-Path ([System.IO.Path]::GetTempPath()) ("go2cs-apicompat-suite-" + [guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory $work | Out-Null

$lib = 'lib/net10.0/Widget.dll'
$linux = 'runtimes/linux-x64/lib/net10.0/Widget.dll'

# A throwaway package: each asset path maps to a fixture file, or to raw bytes.
function New-Package([string]$Name, [hashtable]$Assets) {
    $path = Join-Path $work "$Name.nupkg"
    $zip = [System.IO.Compression.ZipFile]::Open($path, 'Create')
    try {
        foreach ($asset in $Assets.Keys) {
            $entry = $zip.CreateEntry($asset)
            $stream = $entry.Open()
            try {
                $value = $Assets[$asset]
                if ($value -is [byte[]]) { $bytes = $value } else { $bytes = [System.IO.File]::ReadAllBytes((Join-Path $fixtures $value)) }
                $stream.Write($bytes, 0, $bytes.Length)
            }
            finally { $stream.Dispose() }
        }
    }
    finally { $zip.Dispose() }
    $path
}

function New-Suppressions([string]$Name, [string[]]$Entries) {
    $path = Join-Path $work "$Name.xml"
    $body = ($Entries | ForEach-Object { $_ }) -join "`n"
    Set-Content -LiteralPath $path -Encoding utf8 -Value "<?xml version=`"1.0`" encoding=`"utf-8`"?>`n<Suppressions>`n$body`n</Suppressions>"
    $path
}

function Entry([string]$Id, [string]$Target, [string]$Asset, [string]$Extra = '') {
    "  <Suppression><DiagnosticId>$Id</DiagnosticId><Target>$Target</Target><Left>$Asset</Left><Right>$Asset</Right>$Extra</Suppression>"
}

function Compare-Fixture([string]$Baseline, [string]$Current, [string]$Suppressions) {
    Test-GoApiCompatPackage -Id 'fixture.widget' -Baseline $Baseline -Current $Current -SuppressionFile $Suppressions -RepoRoot $repoRoot -WorkDir $work
}

try {
    Write-Host 'the pinned tool restores from the manifest'
    $pinned = $null
    try { $pinned = Initialize-GoApiCompatTool -RepoRoot $repoRoot } catch { $pinned = $null; $why = $_.Exception.Message }
    Check 'the tool restores and answers the version the manifest pins' ([bool]$pinned) "$why"
    if (-not $pinned) { throw 'the tool did not restore: no later arm can run' }

    $base = New-Package 'base' @{ $lib = 'widget-base.dll'; $linux = 'widget-base.dll' }
    $internal = New-Package 'internal' @{ $lib = 'widget-internal.dll'; $linux = 'widget-base.dll' }
    $added = New-Package 'added' @{ $lib = 'widget-added.dll'; $linux = 'widget-added.dll' }
    $noLinux = New-Package 'nolinux' @{ $lib = 'widget-base.dll' }
    $garbage = New-Package 'garbage' @{ $lib = [System.Text.Encoding]::ASCII.GetBytes('not an assembly'); $linux = 'widget-base.dll' }

    Write-Host 'the same surface is green, and the guard says it compared every asset'
    $r = Compare-Fixture $base $base $null
    Check 'identical packages read green' ($r.Failures.Count -eq 0) ($r.Failures -join '; ')
    Check 'both assets were compared (lib/ and runtimes/)' ($r.Compared -eq 2 -and $r.Assets.Count -eq 2) "compared $($r.Compared) of $($r.Assets.Count)"
    $r = Compare-Fixture $base $added $null
    Check 'an added member is compatible' ($r.Failures.Count -eq 0 -and $r.Compared -eq 2) "compared $($r.Compared); $($r.Failures -join '; ')"

    Write-Host 'THE NEGATIVE CONTROL: a member made internal reads red and is named; restored, it reads green'
    $r = Compare-Fixture $base $internal $null
    Check 'the internalized member reads red' ($r.Failures.Count -eq 1) "failures: $($r.Failures.Count): $($r.Failures -join '; ')"
    Check 'the failure names the member, its rule and its asset' (
        $r.Failures.Count -eq 1 -and $r.Failures[0].Contains('Fixture.Widget.Instance') -and $r.Failures[0].Contains('CP0002') -and $r.Failures[0].Contains($lib)) "$($r.Failures -join '; ')"
    Check 'the red run still compared both assets' ($r.Compared -eq 2) "compared $($r.Compared)"
    $r = Compare-Fixture $base $base $null
    Check 'restored, it reads green again' ($r.Failures.Count -eq 0 -and $r.Compared -eq 2) "compared $($r.Compared); $($r.Failures -join '; ')"

    Write-Host 'a suppression covers its announced break, and nothing else'
    $suppressed = New-Suppressions 'suppressed' @(Entry 'CP0002' 'F:Fixture.Widget.Instance' $lib)
    $r = Compare-Fixture $base $internal $suppressed
    Check 'the suppressed break reads green' ($r.Failures.Count -eq 0 -and $r.Compared -eq 2) "compared $($r.Compared); $($r.Failures -join '; ')"
    $r = Compare-Fixture $base $base $suppressed
    Check 'a suppression that matches nothing reads red (the file empties itself)' (
        $r.Failures.Count -eq 1 -and $r.Failures[0].Contains('unnecessary') -and $r.Failures[0].Contains('Fixture.Widget.Instance')) "$($r.Failures -join '; ')"
    $elsewhere = New-Suppressions 'elsewhere' @(Entry 'CP0002' 'F:Fixture.Widget.Instance' 'runtimes/osx-arm64/lib/net10.0/Widget.dll')
    $r = Compare-Fixture $base $base $elsewhere
    Check 'a suppression naming an asset the baseline lacks reads red' (
        @($r.Failures | Where-Object { $_.Contains('osx-arm64') -and $_.Contains('does not carry') }).Count -eq 1) "$($r.Failures -join '; ')"
    $linuxOnly = New-Suppressions 'linuxonly' @(Entry 'CP0002' 'F:Fixture.Widget.Instance' $linux)
    $r = Compare-Fixture $base $internal $linuxOnly
    Check 'a suppression for another asset does not cover this one' (
        @($r.Failures | Where-Object { $_.Contains('CP0002') -and $_.Contains($lib) -and -not $_.Contains('unnecessary') }).Count -eq 1 -and
        @($r.Failures | Where-Object { $_.Contains('unnecessary') -and $_.Contains($linux) }).Count -eq 1) "$($r.Failures -join '; ')"

    Write-Host 'what the guard cannot see is red, never green'
    $r = Compare-Fixture $base $noLinux $null
    Check 'an asset missing from the current package reads red and is named' (
        @($r.Failures | Where-Object { $_.Contains($linux) -and $_.Contains('missing') }).Count -eq 1) "$($r.Failures -join '; ')"
    $r = Compare-Fixture $base $garbage $null
    Check 'a comparison the tool could not make reads red' (
        @($r.Failures | Where-Object { $_.Contains($lib) -and $_.Contains('verdict') }).Count -eq 1) "$($r.Failures -join '; ')"
    Check 'and is not counted as compared' ($r.Compared -eq 1) "compared $($r.Compared)"

    # go.gen is an analyzer: its one assembly is under analyzers/, so it has no lib/ or runtimes/ asset. The first
    # whole-surface census (2026-11-07) stopped push-nuget.ps1 there: nothing was extracted, so the scratch directory the
    # guard removes at the end did not exist, and the removal threw.
    # The shapes the 1.26.10.3 census packed (362 packages): lib/ only; lib/ plus build/ (318); lib/ plus buildTransitive/
    # (go.lib); lib/ plus the four RID folders plus build/ (41, the L3 packages); analyzers/ only (go.gen). Each assembly
    # has its .pdb beside it, and the two Mac RIDs carry the same darwin bytes.
    Write-Host 'the release''s own package shapes: a .pdb beside each assembly, build/ and buildTransitive/, all four RID folders'
    $pdb = [System.Text.Encoding]::ASCII.GetBytes('not compared')
    $release = @{ 'lib/net10.0/Widget.dll' = 'widget-base.dll'; 'lib/net10.0/Widget.pdb' = $pdb; 'build/go.widget.props' = $pdb; 'buildTransitive/go.widget.targets' = $pdb }
    foreach ($rid in 'win-x64', 'linux-x64', 'osx-x64', 'osx-arm64') { $release["runtimes/$rid/lib/net10.0/Widget.dll"] = 'widget-base.dll'; $release["runtimes/$rid/lib/net10.0/Widget.pdb"] = $pdb }
    $releaseBase = New-Package 'release-base' $release
    $darwinBreak = $release.Clone(); $darwinBreak['runtimes/osx-arm64/lib/net10.0/Widget.dll'] = 'widget-internal.dll'
    $releaseRed = New-Package 'release-red' $darwinBreak
    $r = Compare-Fixture $releaseBase $releaseBase $null
    Check 'the release shape compares its five assemblies and nothing else (no .pdb, no build/)' (
        $r.Failures.Count -eq 0 -and $r.Compared -eq 5 -and $r.Assets.Count -eq 5) "compared $($r.Compared) of $($r.Assets.Count); $($r.Failures -join '; ')"
    $r = Compare-Fixture $releaseBase $releaseRed $null
    Check 'a break in one RID''s flavor alone reads red and names that RID' (
        $r.Failures.Count -eq 1 -and $r.Failures[0].Contains('runtimes/osx-arm64/') -and $r.Failures[0].Contains('Fixture.Widget.Instance') -and $r.Compared -eq 5) "compared $($r.Compared); $($r.Failures -join '; ')"

    Write-Host 'a package with no lib/ or runtimes/ asset (go.gen, an analyzer) has no surface to compare, and does not stop the guard'
    $analyzer = New-Package 'analyzer' @{ 'analyzers/dotnet/cs/Widget.dll' = 'widget-base.dll' }
    $r = $null; $threw = $null
    try { $r = Compare-Fixture $analyzer $analyzer $null } catch { $threw = $_.Exception.Message }
    Check 'an analyzer-only package does not throw' ($null -eq $threw) "$threw"
    Check 'it reads green with nothing compared' ($null -ne $r -and $r.Failures.Count -eq 0 -and $r.Compared -eq 0 -and $r.Assets.Count -eq 0) $(if ($r) { "compared $($r.Compared) of $($r.Assets.Count); $($r.Failures -join '; ')" } else { 'no result' })
    $r = $null; $threw = $null
    try { $r = Compare-Fixture $analyzer $analyzer $suppressed } catch { $threw = $_.Exception.Message }
    Check 'a suppression for an analyzer-only package names an asset it does not carry, and reads red' (
        $null -eq $threw -and @($r.Failures | Where-Object { $_.Contains('does not carry') }).Count -eq 1) $(if ($threw) { $threw } else { "$($r.Failures -join '; ')" })

    Write-Host 'the suppression file format'
    $pkgMode = New-Suppressions 'pkgmode' @(Entry 'CP0002' 'F:Fixture.Widget.Instance' $lib '<IsBaselineSuppression>true</IsBaselineSuppression>')
    $threw = $false; try { Read-GoApiCompatSuppressions $pkgMode | Out-Null } catch { $threw = $_.Exception.Message.Contains('package-mode') }
    Check 'a package-mode baseline entry is refused by name' $threw 'read without a refusal'
    $split = New-Suppressions 'split' @("  <Suppression><DiagnosticId>CP0002</DiagnosticId><Target>F:Fixture.Widget.Instance</Target><Left>$lib</Left><Right>$linux</Right></Suppression>")
    $threw = $false; try { Read-GoApiCompatSuppressions $split | Out-Null } catch { $threw = $_.Exception.Message.Contains('same asset') }
    Check 'an entry naming two assets is refused' $threw 'read without a refusal'

    # Nearly every converted member's Target names zhe (U+0436, the heap box), and the tree's suppression files carry no
    # BOM. Windows PowerShell 5.1 -- the shell release-nuget.ps1 drives push-nuget.ps1 through -- read them as the ANSI
    # code page, so each such Target came back with zhe as two cp1252 characters (U+00D0 U+00B6), matched nothing, and
    # the guard printed the break beside an "unnecessary" entry for the same member (release-smoke 42533214613,
    # 2026-11-08). TestPushnugetSuitesPassUnderWindowsPowerShell runs this suite under 5.1 as well. This file has no BOM
    # either, so the strings below spell zhe as [char]0x0436, never literally.
    Write-Host 'a suppression file is read as UTF-8, BOM or not, under either PowerShell'
    $zhe = [string][char]0x0436
    $rangeTarget = 'M:go.builtin.range``1(go.' + $zhe + '{``0})'
    $noBom = Join-Path $work 'nobom.xml'
    [System.IO.File]::WriteAllText($noBom, "<?xml version=`"1.0`" encoding=`"utf-8`"?>`n<Suppressions>`n$(Entry 'CP0002' $rangeTarget $lib)`n</Suppressions>", (New-Object System.Text.UTF8Encoding($false)))
    $read = @(Read-GoApiCompatSuppressions $noBom)
    Check "a BOM-less file's non-ASCII Target reads as written (PowerShell $($PSVersionTable.PSVersion))" (
        $read.Count -eq 1 -and $read[0].Target -ceq $rangeTarget) "read '$(@($read | ForEach-Object { $_.Target }) -join ', ')'"
    foreach ($file in @((Get-GoApiCompatSuppressionMap (Join-Path $repoRoot 'src/core')).Values)) {
        $written = @([regex]::Matches([System.IO.File]::ReadAllText($file, [System.Text.Encoding]::UTF8), '<Target>([^<]*)</Target>') | ForEach-Object { [System.Net.WebUtility]::HtmlDecode($_.Groups[1].Value) })
        $readBack = @(Read-GoApiCompatSuppressions $file | ForEach-Object { $_.Target })
        $differ = @(for ($i = 0; $i -lt [Math]::Max($written.Count, $readBack.Count); $i++) { if ($i -ge $written.Count -or $i -ge $readBack.Count -or $written[$i] -cne $readBack[$i]) { $i } })
        $name = Split-Path -Leaf (Split-Path -Parent $file)
        Check "every Target of $name's suppression file reads as written ($(@($written | Where-Object { $_.Contains($zhe) }).Count) of $($written.Count) name zhe)" (
            $differ.Count -eq 0 -and $written.Count -gt 0) "$($differ.Count) of $($written.Count) differ; the first read back as '$(if ($differ.Count -and $differ[0] -lt $readBack.Count) { $readBack[$differ[0]] })'"
    }

    Write-Host 'the baseline is the last published revision on the same line'
    $published = @('1.26.10', '1.26.10.1', '1.26.10.2', '1.26.10.3', '1.26.10.2-local.1', '1.27.0.1', '1.24.13.7')
    Check 'a release takes the revision before it' ((Get-GoApiCompatBaselineVersion $published '1.26.10.3') -ceq '1.26.10.2') "$(Get-GoApiCompatBaselineVersion $published '1.26.10.3')"
    Check 'a local rehearsal compares with the published release below it' ((Get-GoApiCompatBaselineVersion $published '1.26.10.3-local.1') -ceq '1.26.10.2') "$(Get-GoApiCompatBaselineVersion $published '1.26.10.3-local.1')"
    # But push-nuget.ps1 -VersionSuffix packs at version.props' build, which is the LAST-PUBLISHED one: the .3
    # rehearsal packed 1.26.10.2-ci.<run>, whose own baseline is .1, so it compared with a revision the release does not
    # follow and read .2's own changes as breaks (release-smoke 42533214613, 2026-11-08). The guard therefore takes the
    # release version the run stands for, and push-nuget.ps1 passes the would-be version on a pack-only run. (A
    # -VersionSuffix rehearsal now packs a prerelease of the would-be release, release-smoke 43000314806; an unsuffixed
    # pack-only run still packs at the last-published build, so the arm stays.)
    Check 'a pack at the last-published build, by its own version, would compare with the revision before that' ((Get-GoApiCompatBaselineVersion $published '1.26.10.2-ci.1') -ceq '1.26.10.1') "$(Get-GoApiCompatBaselineVersion $published '1.26.10.2-ci.1')"
    $guardParams = (Get-Command Invoke-GoApiCompatGuard).Parameters
    Check 'the guard takes the release version a run stands for' ($guardParams.ContainsKey('ReleaseVersion')) 'Invoke-GoApiCompatGuard has no -ReleaseVersion'
    Check 'the first release of a Go minor has no baseline' ($null -eq (Get-GoApiCompatBaselineVersion $published '1.27.0.0')) "$(Get-GoApiCompatBaselineVersion $published '1.27.0.0')"
    Check 'a new package has no baseline' ($null -eq (Get-GoApiCompatBaselineVersion @() '1.26.10.3')) 'a baseline from nothing'
    Check 'a module revision takes its own module version''s previous revision' ((Get-GoApiCompatBaselineVersion @('1.0.300', '1.0.300.1', '1.0.300.2', '1.0.301') '1.0.300.3') -ceq '1.0.300.2') "$(Get-GoApiCompatBaselineVersion @('1.0.300', '1.0.300.1', '1.0.300.2', '1.0.301') '1.0.300.3')"

    # Any release after 1.26.10.3 compares with 1.26.10.3, which already carries B3's break, so its entries match
    # nothing there and the guard refuses them (REVIEW-go-break-floor.md, item 4). They were deleted when 1.26.10.3 was
    # published (C2, 2026-11-10: every pre-publish pack on master refused on them, release-smoke 42720953059), with the
    # file, as its header said; the record copy stays at docs/phase4/data/binary-compat/. A later announced break adds
    # the file back, so the bound stays: at most B3's 12, citing B3 while it holds any.
    Write-Host 'golib carries at most B3''s announced break, and nothing unannounced'
    $golib = Join-Path $repoRoot 'src/core/golib/CompatibilitySuppressions.xml'
    $entries = @()
    if (Test-Path -LiteralPath $golib) { $entries = @(Read-GoApiCompatSuppressions $golib) }
    Check 'golib''s suppression file carries at most the 12 B3 entries' ($entries.Count -le 12) "$($entries.Count) entries"
    Check 'every entry names golib''s lib/ asset' (@($entries | Where-Object { $_.Asset -cne 'lib/net10.0/golib.dll' }).Count -eq 0) 'an entry names another asset'
    $text = ''
    if (Test-Path -LiteralPath $golib) { $text = [System.IO.File]::ReadAllText($golib) }
    Check 'the file cites B3 and batch 231 while it carries any entry' ($entries.Count -eq 0 -or ($text.Contains('B3') -and $text.Contains('batch 231'))) 'no citation'

    # A package id cannot be turned back into its directory: go.vendor.golang.org.x.crypto.cryptobyte lives in
    # vendor/golang.org/x/crypto/cryptobyte, and the reverse mapping looked in vendor/golang/org/... (the whole-surface
    # census, 2026-11-07). The map is built from the directories.
    Write-Host 'a suppression file is found by its directory''s package id, dotted path segments included'
    $core = Join-Path $work 'core'
    foreach ($dir in 'golib', 'fmt', 'vendor/golang.org/x/crypto/cryptobyte') {
        New-Item -ItemType Directory -Force (Join-Path $core $dir) | Out-Null
        Set-Content -LiteralPath (Join-Path (Join-Path $core $dir) 'CompatibilitySuppressions.xml') -Value '<Suppressions />'
    }
    $map = Get-GoApiCompatSuppressionMap $core
    Check 'golib maps to go.lib' ("$($map['go.lib'])".EndsWith('golib' + [System.IO.Path]::DirectorySeparatorChar + 'CompatibilitySuppressions.xml')) "$($map['go.lib'])"
    Check 'a directory with dotted segments maps to its package id' (
        "$($map['go.vendor.golang.org.x.crypto.cryptobyte'])".Contains('golang.org')) "keys: $(@($map.Keys | Sort-Object) -join ', ')"
    Check 'every file is mapped, and nothing else' ($map.Count -eq 3 -and $map.ContainsKey('go.fmt')) "keys: $(@($map.Keys | Sort-Object) -join ', ')"
    # The tree's own files: every one is mapped, none twice. With no announced break the tree carries none at all.
    $real = Get-GoApiCompatSuppressionMap (Join-Path $repoRoot 'src/core')
    $onDisk = @(Get-ChildItem -LiteralPath (Join-Path $repoRoot 'src/core') -Recurse -File -Filter 'CompatibilitySuppressions.xml')
    Check 'every suppression file in the tree is mapped by its package id' ($real.Count -eq $onDisk.Count) "$($real.Count) mapped of $($onDisk.Count) on disk; keys: $(@($real.Keys | Sort-Object) -join ', ')"
    # Deleted with golib's when 1.26.10.3 was published; a later break re-adds it under the same bound.
    $cb = @()
    if ($real.ContainsKey('go.vendor.golang.org.x.crypto.cryptobyte')) { $cb = @(Read-GoApiCompatSuppressions $real['go.vendor.golang.org.x.crypto.cryptobyte']) }
    Check 'vendored cryptobyte carries at most batch 235''s 5 entries, and cites the batch while it carries any' (
        $cb.Count -le 5 -and ($cb.Count -eq 0 -or [System.IO.File]::ReadAllText($real['go.vendor.golang.org.x.crypto.cryptobyte']).Contains('batch 235'))) "$($cb.Count) entries"

    # The break floor (DESIGN-go-break-floor.md section 4): go.lib's targets fail a consumer's build for a package built
    # below $(GoBreakFloor), and push-nuget refuses a release whose floor disagrees with its suppression files. Once the
    # guard is green every suppression entry left is a break against the baseline, so a reachable one means this
    # release breaks.
    Write-Host 'the break floor moves with a reachable break and only then'
    $floorCmds = @('Test-GoBreakFloor', 'Test-GoBreakFloorShape', 'Test-GoBreakFloorReachable', 'Get-GoBreakFloor', 'Get-GoBreakFloorFromPackage')
    $missing = @($floorCmds | Where-Object { -not (Get-Command $_ -ErrorAction SilentlyContinue) })
    Check 'the module exports the break-floor functions' ($missing.Count -eq 0) "missing: $($missing -join ', ')"
    if ($missing.Count -eq 0) {
        function FloorFailures([string]$R, [string]$F, [string]$BV, [string]$BF, [string[]]$Breaks) {
            @(Test-GoBreakFloor -ReleaseVersion $R -Floor $F -BaselineVersion $BV -BaselineFloor $BF -ReachableBreaks $Breaks)
        }
        Check 'a reachable break with the floor at the release reads green' ((FloorFailures '1.26.10.5' '1.26.10.5' '1.26.10.4' '1.26.10.3' @('go.lib')).Count -eq 0) ((FloorFailures '1.26.10.5' '1.26.10.5' '1.26.10.4' '1.26.10.3' @('go.lib')) -join '; ')
        $f = @(FloorFailures '1.26.10.5' '1.26.10.3' '1.26.10.4' '1.26.10.3' @('go.lib'))
        Check 'a reachable break that leaves the floor behind reads red, naming the package and both versions' (
            $f.Count -eq 1 -and $f[0].Contains('go.lib') -and $f[0].Contains('1.26.10.3') -and $f[0].Contains('1.26.10.5')) ($f -join '; ')
        Check 'a rehearsal of a breaking release takes its numeric version' ((FloorFailures '1.26.10.5-ci.7' '1.26.10.5' '1.26.10.4' '1.26.10.3' @('go.lib')).Count -eq 0) ((FloorFailures '1.26.10.5-ci.7' '1.26.10.5' '1.26.10.4' '1.26.10.3' @('go.lib')) -join '; ')
        Check 'no break keeps the baseline''s floor: green' ((FloorFailures '1.26.10.5' '1.26.10.3' '1.26.10.4' '1.26.10.3' @()).Count -eq 0) ((FloorFailures '1.26.10.5' '1.26.10.3' '1.26.10.4' '1.26.10.3' @()) -join '; ')
        $f = @(FloorFailures '1.26.10.5' '1.26.10.5' '1.26.10.4' '1.26.10.3' @())
        Check 'no break, and the floor moved anyway: red (it would force a rebuild wave for nothing)' ($f.Count -eq 1 -and $f[0].Contains('1.26.10.3')) ($f -join '; ')
        Check 'the first release with the floor may name a published release' ((FloorFailures '1.26.10.4' '1.26.10.3' '1.26.10.3' '' @()).Count -eq 0) ((FloorFailures '1.26.10.4' '1.26.10.3' '1.26.10.3' '' @()) -join '; ')
        Check 'and not one above its baseline' ((FloorFailures '1.26.10.4' '1.26.10.4' '1.26.10.3' '' @()).Count -eq 1) ((FloorFailures '1.26.10.4' '1.26.10.4' '1.26.10.3' '' @()) -join '; ')
        Check 'nor one on another Go line' ((FloorFailures '1.26.10.4' '1.24.13.3' '1.26.10.3' '' @()).Count -eq 1) ((FloorFailures '1.26.10.4' '1.24.13.3' '1.26.10.3' '' @()) -join '; ')
        Check 'the first release of a Go minor sets the floor to itself' ((FloorFailures '1.27.0.1' '1.27.0.1' '' '' @()).Count -eq 0) ((FloorFailures '1.27.0.1' '1.27.0.1' '' '' @()) -join '; ')
        Check 'and reads red when it keeps the old minor''s floor' ((FloorFailures '1.27.0.1' '1.26.10.3' '' '' @()).Count -eq 1) ((FloorFailures '1.27.0.1' '1.26.10.3' '' '' @()) -join '; ')
        Check 'a missing floor reads red' ((FloorFailures '1.26.10.5' '' '1.26.10.4' '1.26.10.3' @()).Count -eq 1) ((FloorFailures '1.26.10.5' '' '1.26.10.4' '1.26.10.3' @()) -join '; ')
        Check 'golib is reachable' (Test-GoBreakFloorReachable 'go.lib') 'unreachable'
        Check 'a public package is reachable' (Test-GoBreakFloorReachable 'go.encoding.json') 'unreachable'
        Check 'a std-vendored package is not' (-not (Test-GoBreakFloorReachable 'go.vendor.golang.org.x.crypto.cryptobyte')) 'reachable'
        Check 'an internal package is not' (-not (Test-GoBreakFloorReachable 'go.crypto.internal.fips140.v1.0.0')) 'reachable'
        Check 'a package whose path only contains the word is reachable' (Test-GoBreakFloorReachable 'go.example.internals') 'unreachable'
        Check 'the floor is read from a targets text' ((Get-GoBreakFloor '<P><GoBreakFloor>1.26.10.3</GoBreakFloor></P>') -ceq '1.26.10.3') "$(Get-GoBreakFloor '<P><GoBreakFloor>1.26.10.3</GoBreakFloor></P>')"
        Check 'a targets text without one reads as none' ($null -eq (Get-GoBreakFloor '<Project />')) 'a floor from nothing'
        $treeFloor = Get-GoBreakFloor ([System.IO.File]::ReadAllText((Join-Path $repoRoot 'src/core/golib/buildTransitive/go.lib.targets')))
        Check 'the tree''s go.lib.targets states a floor' ($treeFloor -match '^\d+\.\d+\.\d+\.\d+$') "$treeFloor"

        # What holds in every one of the four cases needs no baseline: the floor parses, sits on the release's Go line
        # and is at most the release. -VerifyOnly packs nothing and compares with nothing, so this is the part it runs.
        Write-Host 'the floor''s baseline-free cases'
        function ShapeFailures([string]$R, [string]$F) { @(Test-GoBreakFloorShape -ReleaseVersion $R -Floor $F) }
        Check 'a floor at the release reads green' ((ShapeFailures '1.26.10.4' '1.26.10.4').Count -eq 0) ((ShapeFailures '1.26.10.4' '1.26.10.4') -join '; ')
        Check 'a floor below the release on its line reads green' ((ShapeFailures '1.26.10.4' '1.26.10.3').Count -eq 0) ((ShapeFailures '1.26.10.4' '1.26.10.3') -join '; ')
        Check 'a rehearsal takes its numeric version' ((ShapeFailures '1.26.10.4-ci.3' '1.26.10.4').Count -eq 0) ((ShapeFailures '1.26.10.4-ci.3' '1.26.10.4') -join '; ')
        $f = @(ShapeFailures '1.26.10.4' '1.26.10.5')
        Check 'a floor above the release reads red, naming both' ($f.Count -eq 1 -and $f[0].Contains('1.26.10.5') -and $f[0].Contains('1.26.10.4')) ($f -join '; ')
        $f = @(ShapeFailures '1.27.0.1' '1.26.10.3')
        Check 'a floor on another Go line reads red, naming both' ($f.Count -eq 1 -and $f[0].Contains('1.26.10.3') -and $f[0].Contains('1.27.0.1')) ($f -join '; ')
        Check 'a missing floor reads red' ((ShapeFailures '1.26.10.4' '').Count -eq 1) ((ShapeFailures '1.26.10.4' '') -join '; ')
        Check 'a three-part floor reads red' ((ShapeFailures '1.26.10.4' '1.26.10').Count -eq 1) ((ShapeFailures '1.26.10.4' '1.26.10') -join '; ')
        Check 'the full check refuses a floor above the release too' ((FloorFailures '1.26.10.4' '1.26.10.5' '1.26.10.3' '1.26.10.3' @()).Count -eq 1) ((FloorFailures '1.26.10.4' '1.26.10.5' '1.26.10.3' '1.26.10.3' @()) -join '; ')

        # push-nuget reads the floor from the go.lib it packed, which is what ships.
        Write-Host 'the floor is read from a packed go.lib'
        Add-Type -AssemblyName System.IO.Compression.FileSystem
        $nupkg = Join-Path $work 'go.lib.1.26.10.9.nupkg'
        $zip = [System.IO.Compression.ZipFile]::Open($nupkg, 'Create')
        try {
            $writer = New-Object System.IO.StreamWriter($zip.CreateEntry('buildTransitive/go.lib.targets').Open())
            try { $writer.Write('<Project><PropertyGroup><GoBreakFloor>1.26.10.7</GoBreakFloor></PropertyGroup></Project>') } finally { $writer.Dispose() }
        }
        finally { $zip.Dispose() }
        Check 'a packed go.lib states its floor' ((Get-GoBreakFloorFromPackage $nupkg) -ceq '1.26.10.7') "$(Get-GoBreakFloorFromPackage $nupkg)"
    }

    # The floor read from the consumer's side (Test-GoPackFloor): a pack is consumed with the go.lib it ships, whose
    # targets fail the build for a go.* package below its floor, a prerelease counting as its release. Release-smoke
    # 43000314806 packed 1.26.10.3-ci.<run> beside a floor of 1.26.10.4 and every consumer build failed GO2CS3002.
    Write-Host 'a pack passes the floor of the go.lib it ships'
    $packFloorExported = [bool](Get-Command Test-GoPackFloor -ErrorAction SilentlyContinue)
    Check 'the module exports Test-GoPackFloor' $packFloorExported 'not exported'
    if ($packFloorExported) {
        function Pack([string]$Version, [string[]]$Ids) { @($Ids | ForEach-Object { [pscustomobject]@{ Id = $_; Version = $Version } }) }
        function PackFloorFailures([object[]]$Packages, [string]$Floor) { @(Test-GoPackFloor -Packages $Packages -Floor $Floor) }
        $rehearsal = Pack '1.26.10.4-ci.7' @('go.lib', 'go.cmp')
        Check 'a rehearsal of the release the floor names passes it' ((PackFloorFailures $rehearsal '1.26.10.4').Count -eq 0) ((PackFloorFailures $rehearsal '1.26.10.4') -join '; ')
        Check 'a release at its floor passes it' ((PackFloorFailures (Pack '1.26.10.4' @('go.lib')) '1.26.10.4').Count -eq 0) ((PackFloorFailures (Pack '1.26.10.4' @('go.lib')) '1.26.10.4') -join '; ')
        Check 'a pack above the floor passes it' ((PackFloorFailures (Pack '1.26.10.5-ci.1' @('go.lib')) '1.26.10.4').Count -eq 0) ((PackFloorFailures (Pack '1.26.10.5-ci.1' @('go.lib')) '1.26.10.4') -join '; ')
        $f = @(PackFloorFailures (Pack '1.26.10.3-ci.43000314806' @('go.lib', 'go.cmp')) '1.26.10.4')
        Check 'a pack labelled with the last-published build reads red, naming its packages, its version, the release it reads as and the floor' (
            $f.Count -eq 1 -and $f[0].Contains('go.cmp, go.lib') -and $f[0].Contains('packed at 1.26.10.3-ci.43000314806') -and
            $f[0].Contains('reads as 1.26.10.3,') -and $f[0].Contains('<GoBreakFloor> 1.26.10.4') -and $f[0].Contains('GO2CS3002')) ($f -join '; ')
        $f = @(PackFloorFailures (Pack '1.26.10.3' @('go.a', 'go.b', 'go.c', 'go.d', 'go.e')) '1.26.10.4')
        Check 'past three packages the rest are counted' ($f.Count -eq 1 -and $f[0].Contains('go.a, go.b, go.c and 2 more are packed at 1.26.10.3')) ($f -join '; ')
        $f = @(PackFloorFailures (@(Pack '1.26.10.4-ci.1' @('go.lib')) + @(Pack '1.26.10.3' @('go.bufio'))) '1.26.10.4')
        Check 'only the versions below the floor are named' ($f.Count -eq 1 -and $f[0].StartsWith('pack floor: go.bufio is packed at 1.26.10.3,')) ($f -join '; ')
        Check 'a package outside go.* is not read' ((PackFloorFailures (Pack '1.0.0' @('nugetgo.example')) '1.26.10.4').Count -eq 0) ((PackFloorFailures (Pack '1.0.0' @('nugetgo.example')) '1.26.10.4') -join '; ')
        Check 'go.* is matched as go.lib.targets matches it, ignoring case' ((PackFloorFailures (Pack '1.26.10.3' @('Go.Cmp')) '1.26.10.4').Count -eq 1) 'Go.Cmp was not read'
        Check 'a version that does not parse counts as below' ((PackFloorFailures (Pack 'unknown' @('go.lib')) '1.26.10.4').Count -eq 1) 'it passed'
        Check 'a floor that does not parse reads red' ((PackFloorFailures $rehearsal '').Count -eq 1 -and (PackFloorFailures $rehearsal 'x').Count -eq 1) 'it passed'
    }

    Write-Host 'the release path runs it'
    $push = [System.IO.File]::ReadAllText((Join-Path $repoRoot 'src/push-nuget.ps1'))
    Check 'push-nuget.ps1 restores the pinned tool' ($push.Contains('Initialize-GoApiCompatTool')) 'no restore call'
    Check 'push-nuget.ps1 runs the guard over the merged packages' ($push.Contains('Invoke-GoApiCompatGuard')) 'no guard call'
    Check 'push-nuget.ps1 finds suppression files through the directory map' ($push.Contains('Get-GoApiCompatSuppressionMap')) 'no map call'
    Check 'push-nuget.ps1 refuses a suppression file that names no packed package' ($push.Contains('names no packed package')) 'no orphan check'
    Check 'push-nuget.ps1 checks the break floor after the guard' ($push.Contains('Test-GoBreakFloor ')) 'no floor check'
    $shapeAt = $push.IndexOf('Test-GoBreakFloorShape')
    $verifyExitAt = $push.IndexOf('if ($VerifyOnly) {')
    Check 'push-nuget.ps1 checks the floor''s baseline-free cases before -VerifyOnly exits' ($shapeAt -ge 0 -and $verifyExitAt -ge 0 -and $shapeAt -lt $verifyExitAt) "shape check at $shapeAt, -VerifyOnly exit at $verifyExitAt"
    Check 'push-nuget.ps1 reads the floor from the packed go.lib' ($push -match '\$breakFloor = Get-GoBreakFloorFromPackage') 'the floor is not read from the packed go.lib'
    Check 'push-nuget.ps1 compares a pack-only run with what its release would follow' ($push -match '-ReleaseVersion \$guardReleaseVersion' -and $push -match '\$guardReleaseVersion = if \(\$dryRun\) \{ \$wouldBeVersion \} else \{ \$fullVersion \}') 'the guard is not given the would-be version'
    $packFloorAt = $push.IndexOf('Test-GoPackFloor -Packages')
    $floorReadAt = $push.IndexOf('$breakFloor = Get-GoBreakFloorFromPackage')
    $pushGateAt = $push.IndexOf('# --- Push gate')
    Check 'push-nuget.ps1 checks every packed package against the floor of the go.lib it ships, before the push gate' (
        $floorReadAt -ge 0 -and $packFloorAt -gt $floorReadAt -and $packFloorAt -lt $pushGateAt -and $push -match 'Test-GoPackFloor -Packages .*-Floor \$breakFloor') "floor read at $floorReadAt, pack floor at $packFloorAt, push gate at $pushGateAt"
    Check 'push-nuget.ps1 packs a -VersionSuffix rehearsal as a prerelease of the release it rehearses' (
        $push -match '\$packVersion = if \(\$VersionSuffix\) \{ "\$wouldBeVersion-\$VersionSuffix" \} else \{ \$fullVersion \}') 'a rehearsal is not packed at the would-be version'
    $pack = [System.IO.File]::ReadAllText((Join-Path $repoRoot 'src/tools/nugetgo/nugetgo-pack.ps1'))
    Check 'nugetgo-pack.ps1 compares a module with its previous revision' ($pack.Contains('Invoke-GoApiCompatGuard')) 'no guard call'
}
finally {
    Remove-Item -LiteralPath $work -Recurse -Force -ErrorAction SilentlyContinue
}

Write-Host "ran $ran, failed $failed"
exit $failed
