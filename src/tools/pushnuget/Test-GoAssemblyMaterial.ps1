<#
.SYNOPSIS
    Table-driven arms for GoAssemblyMaterial.psm1: push-nuget.ps1's platform-neutral check must promote a package whose
    flavors differ in MEANING and must not promote one whose flavors differ only in the deterministic identity fields.
    The 1.24.13.6 release rehearsal shipped go.internal.coverage.decodemeta with its Windows assembly in lib/ only: its
    linux and darwin flavors reference other per-OS types, but all three are 345,600 bytes long, so a length test
    called them equal. Exit code = the number of failed cases.
#>
$ErrorActionPreference = 'Stop'
Import-Module (Join-Path $PSScriptRoot 'GoAssemblyMaterial.psm1') -Force
$failed = 0
$ran = 0
function Check([string]$name, [bool]$ok, [string]$detail) {
    $script:ran++
    if ($ok) { Write-Host "  ok    $name" } else { $script:failed++; Write-Host "  FAIL  $name -- $detail" -ForegroundColor Red }
}
function Fixture([string]$name) { , [System.IO.File]::ReadAllBytes((Join-Path $PSScriptRoot "testdata/$name")) }
function Key([string]$name, [byte[]]$bytes) { Get-GoLibEntryKey -Name $name -Bytes $bytes }
function Ascii([string]$s) { , [System.Text.Encoding]::ASCII.GetBytes($s) }

# testdata/build-fixtures.ps1 builds these: alpha/bravo are one source with an element type of Dep.Alpha or Dep.Bravo
# (equal-length names, so equal-length assemblies); cascade-a/-b are one source built against two Deps that differ
# only in an internal constant, so they differ only in the identity fields.
$alpha = Fixture 'alpha.dll'
$bravo = Fixture 'bravo.dll'
$cascadeA = Fixture 'cascade-a.dll'
$cascadeB = Fixture 'cascade-b.dll'

Write-Host 'the fixtures are what they claim'
Check 'alpha and bravo have the same length' ($alpha.Length -eq $bravo.Length) "$($alpha.Length) vs $($bravo.Length)"
Check 'alpha and cascade-a are one build (identical bytes)' ([System.Linq.Enumerable]::SequenceEqual($alpha, $cascadeA)) 'the fixture build is not reproducible'
Check 'cascade-a and cascade-b have the same length and differ in bytes' (
    $cascadeA.Length -eq $cascadeB.Length -and -not [System.Linq.Enumerable]::SequenceEqual($cascadeA, $cascadeB)) 'not an identity-only pair'

Write-Host 'a material difference of the same length is material'
Check 'alpha and bravo have different keys' ((Key 'lib/net10.0/App.dll' $alpha) -ne (Key 'lib/net10.0/App.dll' $bravo)) 'equal keys: the length test passes it'
$why = Get-GoLibEntryDifference -Name 'lib/net10.0/App.dll' -A $alpha -B $bravo
Check 'the reason names the type references that differ' ("$why".Contains('Alpha') -and "$why".Contains('Bravo')) "reason [$why]"

Write-Host 'the identity cascade is not material'
Check 'cascade-a and cascade-b have equal keys' ((Key 'lib/net10.0/App.dll' $cascadeA) -eq (Key 'lib/net10.0/App.dll' $cascadeB)) 'different keys: binary noise promoted'
Check 'cascade-a and cascade-b have no reason' ($null -eq (Get-GoLibEntryDifference -Name 'lib/net10.0/App.dll' -A $cascadeA -B $cascadeB)) 'a reason for an identity-only pair'
Check 'identical bytes have equal keys' ((Key 'lib/net10.0/App.dll' $alpha) -eq (Key 'lib/net10.0/App.dll' $cascadeA)) 'alpha and cascade-a are the same build'

Write-Host 'everything else that differs is material'
$padded = [byte[]]($alpha + [byte[]]::new(512))
Check 'a different length is material' ((Key 'lib/net10.0/App.dll' $alpha) -ne (Key 'lib/net10.0/App.dll' $padded)) 'equal keys'
Check 'a non-assembly entry differing in content is material' (
    (Key 'lib/net10.0/App.xml' (Ascii '<doc>abc</doc>')) -ne (Key 'lib/net10.0/App.xml' (Ascii '<doc>abd</doc>'))) 'equal keys'
Check 'a .dll that is not a PE image is compared by content' (
    (Key 'lib/net10.0/Bad.dll' (Ascii 'not a pe image 1')) -ne (Key 'lib/net10.0/Bad.dll' (Ascii 'not a pe image 2'))) 'equal keys'
Check 'a difference that renames nothing still gives a reason' (
    [bool](Get-GoLibEntryDifference -Name 'lib/net10.0/App.dll' -A $alpha -B $padded)) 'no reason for a length difference'

Write-Host 'push-nuget uses it'
$push = [System.IO.File]::ReadAllText((Join-Path $PSScriptRoot '../../push-nuget.ps1'))
Check 'push-nuget.ps1 keys every lib/ entry with Get-GoLibEntryKey' ($push.Contains('Get-GoLibEntryKey')) 'no key call'
Check 'push-nuget.ps1 names each promoted package''s reason with Get-GoLibEntryDifference' ($push.Contains('Get-GoLibEntryDifference')) 'no reason call'

Write-Host "ran $ran, failed $failed"
exit $failed
