<#
.SYNOPSIS
    Table-driven arms for NugetgoValidationBinding.psm1: the proof a module pack ships as VALIDATION.md must be a proof of
    the packed bytes (the nugetgo rehearsal, 2026-10-09, gap 4). The rehearsal packed a -recurse=nuget root beside a
    MODULE.md from a separate -recurse -tests root, and nothing compared them. Each converted module package's project now
    records GoInputDigest; MODULE.md records the same value per package; the pack refuses a proof whose digest is not the
    packed tree's. A case that should refuse names a fragment of its reason. Exit code = the number of failed cases.
#>
$ErrorActionPreference = 'Stop'
Import-Module (Join-Path $PSScriptRoot 'NugetgoValidationBinding.psm1') -Force
$failed = 0
$ran = 0
function Check([string]$name, [bool]$ok, [string]$detail) {
    $script:ran++
    if ($ok) { Write-Host "  ok    $name" } else { $script:failed++; Write-Host "  FAIL  $name -- $detail" -ForegroundColor Red }
}

$a = 'sha256-' + ('a' * 64)
$b = 'sha256-' + ('b' * 64)
$c = 'sha256-' + ('c' * 64)

Write-Host 'MODULE.md is read'
$summary = @"
# ``example.com/mod`` -- module validation summary

| Package | Matched | Disclosed | Input digest | Proof |
|:--|--:|--:|:--|:--|
| ``example.com/mod`` | 37 | 0 | ``$a`` | [index.md](index.md) |
| ``example.com/mod/sub`` | 3 | 1 | ``$b`` | [sub.md](sub.md) |

**Total: 40 matched, 1 disclosed** across 2 package(s).
"@
$proof = Get-NugetgoProofInputDigests -ModuleSummary $summary
Check 'each row''s import path and digest' ($proof.Count -eq 2 -and $proof['example.com/mod'] -eq $a -and $proof['example.com/mod/sub'] -eq $b) "read [$(($proof.GetEnumerator() | ForEach-Object { "$($_.Key)=$($_.Value)" }) -join ', ')]"
$old = "| Package | Matched | Disclosed | Proof |`n|:--|--:|--:|:--|`n| ``example.com/mod`` | 37 | 0 | [index.md](index.md) |`n"
$proofOld = Get-NugetgoProofInputDigests -ModuleSummary $old
Check 'a summary written before the binding reads as a row with no digest' ($proofOld.Count -eq 1 -and $proofOld['example.com/mod'] -eq '') "read [$(($proofOld.GetEnumerator() | ForEach-Object { "$($_.Key)=$($_.Value)" }) -join ', ')]"

Write-Host 'the packed project is read'
$dir = Join-Path ([System.IO.Path]::GetTempPath()) ("nugetgo-binding-$PID-" + [DateTime]::UtcNow.Ticks)
New-Item -ItemType Directory -Force $dir | Out-Null
try {
    $withDigest = Join-Path $dir 'with.csproj'
    [System.IO.File]::WriteAllText($withDigest, "<Project Sdk=`"Microsoft.NET.Sdk`">`r`n  <PropertyGroup>`r`n    <OutputType>Library</OutputType>`r`n    <GoInputDigest>$a</GoInputDigest>`r`n  </PropertyGroup>`r`n</Project>`r`n")
    $without = Join-Path $dir 'without.csproj'
    [System.IO.File]::WriteAllText($without, "<Project Sdk=`"Microsoft.NET.Sdk`">`r`n  <PropertyGroup>`r`n    <OutputType>Library</OutputType>`r`n  </PropertyGroup>`r`n</Project>`r`n")
    Check 'a project''s GoInputDigest' ((Get-NugetgoProjectInputDigest -ProjectFile $withDigest) -eq $a) "read '$(Get-NugetgoProjectInputDigest -ProjectFile $withDigest)'"
    Check 'a project with none reads empty' ((Get-NugetgoProjectInputDigest -ProjectFile $without) -eq '') "read '$(Get-NugetgoProjectInputDigest -ProjectFile $without)'"
}
finally { Remove-Item -Recurse -Force $dir }

Write-Host 'the proof is bound to the packed tree'
$cases = @(
    # name, proof, packed, expected reason fragment (or $null)
    @('a proof of exactly the packed inputs', @{ 'example.com/mod' = $a; 'example.com/mod/sub' = $b }, @{ 'example.com/mod' = $a; 'example.com/mod/sub' = $b }, $null),
    @('a packed package with no Go tests has no proof row', @{ 'example.com/mod' = $a }, @{ 'example.com/mod' = $a; 'example.com/mod/notests' = $c }, $null),
    @('a proof made from other inputs', @{ 'example.com/mod' = $a; 'example.com/mod/sub' = $c }, @{ 'example.com/mod' = $a; 'example.com/mod/sub' = $b }, "example.com/mod/sub: the proof was made from inputs $c, the packed tree from $b"),
    @('a proof with no digest', @{ 'example.com/mod' = '' }, @{ 'example.com/mod' = $a }, 'the proof records no input digest for example.com/mod'),
    @('a packed tree with no digest', @{ 'example.com/mod' = $a }, @{ 'example.com/mod' = '' }, 'example.com/mod is packed from a conversion that records no input digest'),
    @('a proof row for a package not packed', @{ 'example.com/mod' = $a; 'example.com/mod/gone' = $b }, @{ 'example.com/mod' = $a }, 'example.com/mod/gone has a proof row and is not packed'),
    @('an empty proof', @{}, @{ 'example.com/mod' = $a }, 'the proof lists no package')
)
foreach ($case in $cases) {
    $reasons = @(Test-NugetgoValidationBinding -Proof $case[1] -Packed $case[2])
    if ($case[3]) { Check $case[0] (@($reasons | Where-Object { $_.Contains($case[3]) }).Count -eq 1) "reasons [$($reasons -join ' | ')], want one naming '$($case[3])'" }
    else { Check $case[0] ($reasons.Count -eq 0) "reasons [$($reasons -join ' | ')], want none" }
}

Write-Host 'the pack uses it'
$pack = [System.IO.File]::ReadAllText((Join-Path $PSScriptRoot 'nugetgo-pack.ps1'))
Check 'nugetgo-pack.ps1 reads each packed project''s digest and the proof''s' (
    $pack.Contains('Get-NugetgoProjectInputDigest') -and $pack.Contains('Get-NugetgoProofInputDigests')) 'the pack does not read both digests'
$bound = $pack.IndexOf('Test-NugetgoValidationBinding')
Check 'nugetgo-pack.ps1 checks the binding before it packs' ($bound -ge 0 -and $bound -lt $pack.IndexOf('& dotnet pack')) 'no binding check ahead of dotnet pack'

Write-Host "ran $ran, failed $failed"
exit $failed
