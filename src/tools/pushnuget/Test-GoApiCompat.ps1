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

    Write-Host 'the suppression file format'
    $pkgMode = New-Suppressions 'pkgmode' @(Entry 'CP0002' 'F:Fixture.Widget.Instance' $lib '<IsBaselineSuppression>true</IsBaselineSuppression>')
    $threw = $false; try { Read-GoApiCompatSuppressions $pkgMode | Out-Null } catch { $threw = $_.Exception.Message.Contains('package-mode') }
    Check 'a package-mode baseline entry is refused by name' $threw 'read without a refusal'
    $split = New-Suppressions 'split' @("  <Suppression><DiagnosticId>CP0002</DiagnosticId><Target>F:Fixture.Widget.Instance</Target><Left>$lib</Left><Right>$linux</Right></Suppression>")
    $threw = $false; try { Read-GoApiCompatSuppressions $split | Out-Null } catch { $threw = $_.Exception.Message.Contains('same asset') }
    Check 'an entry naming two assets is refused' $threw 'read without a refusal'

    Write-Host 'the baseline is the last published revision on the same line'
    $published = @('1.26.10', '1.26.10.1', '1.26.10.2', '1.26.10.3', '1.26.10.2-local.1', '1.27.0.1', '1.24.13.7')
    Check 'a release takes the revision before it' ((Get-GoApiCompatBaselineVersion $published '1.26.10.3') -ceq '1.26.10.2') "$(Get-GoApiCompatBaselineVersion $published '1.26.10.3')"
    Check 'a local rehearsal compares with the published release below it' ((Get-GoApiCompatBaselineVersion $published '1.26.10.3-local.1') -ceq '1.26.10.2') "$(Get-GoApiCompatBaselineVersion $published '1.26.10.3-local.1')"
    Check 'the first release of a Go minor has no baseline' ($null -eq (Get-GoApiCompatBaselineVersion $published '1.27.0.0')) "$(Get-GoApiCompatBaselineVersion $published '1.27.0.0')"
    Check 'a new package has no baseline' ($null -eq (Get-GoApiCompatBaselineVersion @() '1.26.10.3')) 'a baseline from nothing'
    Check 'a module revision takes its own module version''s previous revision' ((Get-GoApiCompatBaselineVersion @('1.0.300', '1.0.300.1', '1.0.300.2', '1.0.301') '1.0.300.3') -ceq '1.0.300.2') "$(Get-GoApiCompatBaselineVersion @('1.0.300', '1.0.300.1', '1.0.300.2', '1.0.301') '1.0.300.3')"

    Write-Host 'golib carries B3''s announced break, and nothing unannounced'
    $golib = Join-Path $repoRoot 'src/core/golib/CompatibilitySuppressions.xml'
    $entries = @(Read-GoApiCompatSuppressions $golib)
    Check 'golib''s suppression file carries the 12 B3 entries' ($entries.Count -eq 12) "$($entries.Count) entries"
    Check 'every entry names golib''s lib/ asset' (@($entries | Where-Object { $_.Asset -cne 'lib/net10.0/golib.dll' }).Count -eq 0) 'an entry names another asset'
    $text = [System.IO.File]::ReadAllText($golib)
    Check 'the file cites B3 and batch 231' ($text.Contains('B3') -and $text.Contains('batch 231')) 'no citation'

    Write-Host 'the release path runs it'
    $push = [System.IO.File]::ReadAllText((Join-Path $repoRoot 'src/push-nuget.ps1'))
    Check 'push-nuget.ps1 restores the pinned tool' ($push.Contains('Initialize-GoApiCompatTool')) 'no restore call'
    Check 'push-nuget.ps1 runs the guard over the merged packages' ($push.Contains('Invoke-GoApiCompatGuard')) 'no guard call'
    $pack = [System.IO.File]::ReadAllText((Join-Path $repoRoot 'src/tools/nugetgo/nugetgo-pack.ps1'))
    Check 'nugetgo-pack.ps1 compares a module with its previous revision' ($pack.Contains('Invoke-GoApiCompatGuard')) 'no guard call'
}
finally {
    Remove-Item -LiteralPath $work -Recurse -Force -ErrorAction SilentlyContinue
}

Write-Host "ran $ran, failed $failed"
exit $failed
