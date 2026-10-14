# NugetgoFlavors.psm1 - Gbtc
# Copyright (c) 2026 The go2cs Authors. All rights reserved.
#
# SPDX-License-Identifier: AGPL-3.0-only
# Use of this source code is governed by the GNU Affero General Public License
# version 3 only, which can be found in the LICENSE file.

# PER-OS PROMOTION FOR A NUGETGO PACKAGE (COORD ruling 2026-10-12, for mousetrap and cobra).
#
# A converted module can differ by GOOS: mousetrap's StartedByExplorer has a Windows body and an "others" body, and
# cobra's command_win.go is Windows-only. A -recurse conversion emits ONE target, so nugetgo-pack.ps1 converts and
# builds the module once per GOOS (its flavors) and asks the go.* release's own question of them: do the flavors'
# assemblies differ in MEANING? The answer is a measurement, made with the release's material check
# (src/tools/pushnuget/GoAssemblyMaterial.psm1: each assembly hashed with Roslyn's deterministic identity fields zeroed),
# never a list and never the module author's word.
#
#   * They do not: the package is the one neutral assembly set it has always been, packed from the proof's flavor, so
#     a neutral package is byte for byte what the single-flavor pack produced.
#   * They do: the package takes push-nuget.ps1's RID-specific layout exactly. lib/<tfm>/ carries the REFERENCE flavor
#     (windows, the release's first RID) as the compile-time asset, and runtimes/<rid>/lib/<tfm>/ carries each shipped
#     RID's flavor (win-x64, linux-x64, osx-x64 and osx-arm64; the two Mac RIDs the same darwin bytes). The release's
#     own comment block above $ridFlavors says why lib/ and not ref/, and why both Mac folders ship explicitly.
#
# The RID map is the release's, and Test-NugetgoFlavors.ps1 reads push-nuget.ps1 to hold it so.

Import-Module (Join-Path $PSScriptRoot '../pushnuget/GoAssemblyMaterial.psm1') -Force

# RID -> GOOS, in push-nuget.ps1's order: the first entry is the reference flavor.
function Get-NugetgoRidFlavors {
    [ordered]@{
        'win-x64'   = 'windows'
        'linux-x64' = 'linux'
        'osx-x64'   = 'darwin'
        'osx-arm64' = 'darwin'
    }
}

function Get-NugetgoReferenceGoos { @((Get-NugetgoRidFlavors).Values)[0] }

# The distinct GOOS values, in map order: one conversion and one build each.
function Get-NugetgoFlavorGoos { @((Get-NugetgoRidFlavors).Values | Select-Object -Unique) }

# The distinct 'goos/goarch' pairs the RID map ships, in map order (x64 is amd64): where a package's imports are read.
function Get-NugetgoFlavorPlatforms {
    @((Get-NugetgoRidFlavors).GetEnumerator() | ForEach-Object { "$($_.Value)/$(if ($_.Key -like '*-arm64') { 'arm64' } else { 'amd64' })" } | Select-Object -Unique)
}

# Does any flavor's assembly set differ in meaning from the proof's flavor? -Assemblies maps each GOOS of
# Get-NugetgoFlavorGoos to a hashtable of assembly file name -> bytes. A flavor that is missing is refused rather than
# read as neutral: the decision is only a measurement when every flavor was measured.
function Get-NugetgoFlavorDecision {
    [CmdletBinding()]
    param([Parameter(Mandatory)]$Assemblies, [Parameter(Mandatory)][string]$ProofGoos)

    foreach ($goos in Get-NugetgoFlavorGoos) {
        if (-not $Assemblies.Contains($goos)) { throw "no $goos flavor was built: every flavor ($((Get-NugetgoFlavorGoos) -join ', ')) is measured before a package is called neutral" }
    }
    if (-not $Assemblies.Contains($ProofGoos)) { throw "the proof's flavor $ProofGoos is not one of $((Get-NugetgoFlavorGoos) -join ', ')" }

    $proof = $Assemblies[$ProofGoos]
    $reasons = New-Object System.Collections.Generic.List[string]
    foreach ($goos in Get-NugetgoFlavorGoos) {
        if ($goos -ceq $ProofGoos) { continue }
        $flavor = $Assemblies[$goos]
        $onlyProof = @($proof.Keys | Where-Object { -not $flavor.ContainsKey($_) } | Sort-Object)
        $onlyFlavor = @($flavor.Keys | Where-Object { -not $proof.ContainsKey($_) } | Sort-Object)
        if ($onlyProof.Count -or $onlyFlavor.Count) {
            $reasons.Add("[$goos] the assembly set differs from ${ProofGoos}'s (only in ${ProofGoos}: $($onlyProof -join ', '); only in ${goos}: $($onlyFlavor -join ', '))")
            continue
        }
        foreach ($name in @($proof.Keys | Sort-Object)) {
            $why = Get-GoLibEntryDifference -Name $name -A $proof[$name] -B $flavor[$name]
            if ($why) { $reasons.Add("[$goos] $why (first: $ProofGoos)") }
        }
    }
    [pscustomobject]@{ RidSpecific = $reasons.Count -gt 0; Reasons = $reasons.ToArray() }
}

function Read-NugetgoZipEntryBytes([System.IO.Compression.ZipArchiveEntry]$Entry) {
    $ms = New-Object System.IO.MemoryStream
    $s = $Entry.Open()
    try { $s.CopyTo($ms) } finally { $s.Dispose() }
    , $ms.ToArray()
}

function Write-NugetgoZipEntry([System.IO.Compression.ZipArchive]$Zip, [string]$Name, [byte[]]$Bytes) {
    $old = $Zip.GetEntry($Name)
    if ($old) { $old.Delete() }
    $s = $Zip.CreateEntry($Name, [System.IO.Compression.CompressionLevel]::Optimal).Open()
    try { $s.Write($Bytes, 0, $Bytes.Length) } finally { $s.Dispose() }
}

# Rewrite a packed package into the RID-specific layout: lib/<tfm>/ takes the reference flavor's assemblies, and each
# RID's runtimes/<rid>/lib/<tfm>/ its own flavor's. The package's lib/<tfm>/*.dll set must be the flavors' assembly
# set; anything else is refused by name rather than packed half-promoted.
function Set-NugetgoRidAssets {
    [CmdletBinding()]
    param([Parameter(Mandatory)][string]$Package, [Parameter(Mandatory)]$Assemblies, [Parameter(Mandatory)][string]$TargetFramework)

    Add-Type -AssemblyName System.IO.Compression, System.IO.Compression.FileSystem
    $rids = Get-NugetgoRidFlavors
    $reference = $Assemblies[(Get-NugetgoReferenceGoos)]
    $zip = [System.IO.Compression.ZipFile]::Open($Package, 'Update')
    try {
        $lib = @($zip.Entries | Where-Object { $_.FullName -like "lib/$TargetFramework/*.dll" } | ForEach-Object { $_.Name } | Sort-Object)
        $want = @($reference.Keys | Sort-Object)
        if (($lib -join '|') -cne ($want -join '|')) { throw "$Package lib/$TargetFramework/ holds $($lib -join ', '), not the flavors' assemblies $($want -join ', ')" }
        foreach ($name in $want) { Write-NugetgoZipEntry $zip "lib/$TargetFramework/$name" $reference[$name] }
        foreach ($rid in $rids.Keys) {
            $flavor = $Assemblies[$rids[$rid]]
            foreach ($name in $want) { Write-NugetgoZipEntry $zip "runtimes/$rid/lib/$TargetFramework/$name" $flavor[$name] }
        }
    }
    finally { $zip.Dispose() }
}

# The read-back of a RID-specific package: lib/ is the reference flavor, every shipped RID carries the full assembly
# set of its flavor and nothing else, and RIDs that share a GOOS carry the same bytes. No reason means accepted.
function Test-NugetgoRidLayout {
    [CmdletBinding()]
    param([Parameter(Mandatory)][string]$Package, [Parameter(Mandatory)]$Assemblies, [Parameter(Mandatory)][string]$TargetFramework)

    Add-Type -AssemblyName System.IO.Compression, System.IO.Compression.FileSystem
    $rids = Get-NugetgoRidFlavors
    $reasons = New-Object System.Collections.Generic.List[string]
    $zip = [System.IO.Compression.ZipFile]::OpenRead($Package)
    try {
        $entries = @{}
        foreach ($e in $zip.Entries) { if ($e.FullName -like 'lib/*' -or $e.FullName -like 'runtimes/*') { $entries[$e.FullName] = Read-NugetgoZipEntryBytes $e } }
    }
    finally { $zip.Dispose() }

    $names = @($Assemblies[(Get-NugetgoReferenceGoos)].Keys | Sort-Object)
    $same = { param($a, $b) $null -ne $a -and $null -ne $b -and [System.Linq.Enumerable]::SequenceEqual([byte[]]$a, [byte[]]$b) }
    foreach ($name in $names) {
        if (-not (& $same $entries["lib/$TargetFramework/$name"] $Assemblies[(Get-NugetgoReferenceGoos)][$name])) { $reasons.Add("lib/$TargetFramework/$name is not the reference ($(Get-NugetgoReferenceGoos)) flavor") }
    }
    foreach ($rid in $rids.Keys) {
        $folder = "runtimes/$rid/lib/$TargetFramework/"
        $present = @($entries.Keys | Where-Object { $_.StartsWith($folder, [StringComparison]::Ordinal) } | ForEach-Object { $_.Substring($folder.Length) } | Sort-Object)
        if (($present -join '|') -cne ($names -join '|')) { $reasons.Add("$folder holds [$($present -join ', ')], not [$($names -join ', ')]"); continue }
        foreach ($name in $names) {
            if (-not (& $same $entries["$folder$name"] $Assemblies[$rids[$rid]][$name])) { $reasons.Add("$folder$name is not the $($rids[$rid]) flavor") }
        }
    }
    foreach ($goos in Get-NugetgoFlavorGoos) {
        $twins = @($rids.Keys | Where-Object { $rids[$_] -ceq $goos })
        foreach ($rid in $twins | Select-Object -Skip 1) {
            foreach ($name in $names) {
                if (-not (& $same $entries["runtimes/$rid/lib/$TargetFramework/$name"] $entries["runtimes/$($twins[0])/lib/$TargetFramework/$name"])) { $reasons.Add("runtimes/$rid and runtimes/$($twins[0]) share $goos but their $name differs") }
            }
        }
    }
    $stray = @($entries.Keys | Where-Object { $_ -match '^runtimes/([^/]+)/' -and -not $rids.Contains($Matches[1]) })
    if ($stray.Count) { $reasons.Add("runtimes/ holds a RID this layout does not ship: $($stray -join ', ')") }
    $reasons.ToArray()
}

# The package's third-party requires: the UNION of its flavors' (COORD ruling 2026-10-12, for cobra, whose command_win.go
# imports mousetrap on windows only). NuGet declares dependencies per target framework, never per RID, so -- as the
# go.* release does for its own dependencies -- every flavor's require is declared, and only the RID-matched assemblies
# load. The self-description's require lines take the same union. One module at two versions is refused: one go.mod
# resolves one version, so a disagreement means the flavors were not converted from the same module graph. And each
# -ThirdPartyPackage must name a module SOME flavor references (each flavor was asked with -AllowUnreferenced).
function Merge-NugetgoFlavorRequires {
    [CmdletBinding()]
    param([Parameter(Mandatory)]$Requires, [string[]]$ThirdPartyPackage = @())

    $byModule = [ordered]@{}
    foreach ($goos in Get-NugetgoFlavorGoos) {
        foreach ($r in @($Requires[$goos])) {
            if ($null -eq $r) { continue }
            if (-not $byModule.Contains($r.Module)) { $byModule[$r.Module] = $r; continue }
            $seen = $byModule[$r.Module]
            if ($seen.Version -cne $r.Version -or $seen.NuGetId -cne $r.NuGetId -or $seen.PackageVersion -cne $r.PackageVersion) {
                throw "REFUSED: the flavors require $($r.Module) at $($seen.Version) ($($seen.NuGetId) $($seen.PackageVersion)) and at $($r.Version) ($($r.NuGetId) $($r.PackageVersion))"
            }
        }
    }
    foreach ($entry in $ThirdPartyPackage) {
        $module = ($entry -split '=', 2)[0]
        if (-not $byModule.Contains($module)) { throw "REFUSED: -ThirdPartyPackage names $module, which no flavor's packed package references" }
    }
    @($byModule.Keys | Sort-Object | ForEach-Object { $byModule[$_] })
}

# The one line a RID-specific package's proof page carries (COORD ruling 2026-10-12, item 4): its self-description
# describes the proof flavor's package_info metadata, and the other flavors' are not described. Nothing for a neutral
# package, whose flavors agree.
function Get-NugetgoFlavorDisclosure {
    [CmdletBinding()]
    param([Parameter(Mandatory)][string]$ProofGoos, [switch]$RidSpecific)

    if (-not $RidSpecific) { return $null }
    $others = @(Get-NugetgoFlavorGoos | Where-Object { $_ -cne $ProofGoos })
    "Flavors: this package carries RID-specific assemblies; its go2cs/source-metadata.txt describes the $ProofGoos flavor's package metadata, and the $($others -join ' and ') flavors' metadata is not described."
}

Export-ModuleMember -Function Get-NugetgoRidFlavors, Get-NugetgoReferenceGoos, Get-NugetgoFlavorGoos, Get-NugetgoFlavorPlatforms, Get-NugetgoFlavorDecision, Set-NugetgoRidAssets, Test-NugetgoRidLayout, Merge-NugetgoFlavorRequires, Get-NugetgoFlavorDisclosure
