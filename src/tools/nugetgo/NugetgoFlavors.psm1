# NugetgoFlavors.psm1 - Gbtc
# Copyright (c) 2026 The go2cs Authors. All rights reserved.
#
# SPDX-License-Identifier: AGPL-3.0-only
# Use of this source code is governed by the GNU Affero General Public License
# version 3 only, which can be found in the LICENSE file.

# PER-OS PROMOTION FOR A NUGETGO PACKAGE (COORD ruling 2026-10-12, for mousetrap and cobra).
#
# A converted module can differ by platform: mousetrap's StartedByExplorer has a Windows body and an "others" body, and
# cobra's command_win.go is Windows-only. A -recurse conversion emits ONE target, so nugetgo-pack.ps1 converts and
# builds the module once per flavor -- a target of the converted standard library's universe, the pairs the release's
# RID map ships (windows/amd64, linux/amd64, darwin/amd64, linux/arm64, darwin/arm64) -- and asks the go.* release's own question of
# them: do the flavors' assemblies differ in MEANING? The answer is a measurement, made with the release's material check
# (src/tools/pushnuget/GoAssemblyMaterial.psm1: each assembly hashed with Roslyn's deterministic identity fields zeroed),
# never a list and never the module author's word.
#
#   * They do not: the package is the one neutral assembly set it has always been, packed from the proof's flavor, so
#     a neutral package is byte for byte what the single-flavor pack produced.
#   * They do: the package takes push-nuget.ps1's RID-specific layout exactly. lib/<tfm>/ carries the REFERENCE flavor
#     (windows, the release's first RID) as the compile-time asset, and runtimes/<rid>/lib/<tfm>/ carries each shipped
#     RID's flavor (win-x64, linux-x64, osx-x64, linux-arm64 and osx-arm64; each arm64 RID its own flavor, linux/arm64
#     and, since D6 of DESIGN-darwin-arm64-flavour.md, darwin/arm64, never its x64 twin's). The release's
#     own comment block above $ridFlavors says why lib/ and not ref/, and why both Mac folders ship explicitly.
#
# A pack that ships a subset of the RIDs (a -Cgo pack leaves out the RIDs NugetgoCgo.psm1 excludes; none since the cgo
# arm, when osx-arm64 joined) passes its own
# map as -RidFlavors to every function here, so it converts, measures and describes exactly the flavors it ships.
#
# The RID map is the release's, and Test-NugetgoFlavors.ps1 reads push-nuget.ps1 to hold it so.

Import-Module (Join-Path $PSScriptRoot '../pushnuget/GoAssemblyMaterial.psm1') -Force

# RID -> the flavor it ships ('goos/goarch', a universe target), in push-nuget.ps1's order: the first entry is the
# reference flavor. TestRidFlavorMapsShipTheUniverse pins the distinct flavors, in order, to the universe.
function Get-NugetgoRidFlavors {
    [ordered]@{
        'win-x64'     = 'windows/amd64'
        'linux-x64'   = 'linux/amd64'
        'osx-x64'     = 'darwin/amd64'
        'linux-arm64' = 'linux/arm64'
        'osx-arm64'   = 'darwin/arm64'
    }
}

function Get-NugetgoReferenceFlavor { @((Get-NugetgoRidFlavors).Values)[0] }

# The distinct flavors of a RID map (the release's by default), in map order: one conversion and one build each.
function Get-NugetgoFlavors { param($RidFlavors = (Get-NugetgoRidFlavors)) @($RidFlavors.Values | Select-Object -Unique) }

# The GOOS of a flavor, and the target a RID's own host converts for by default (x64 is amd64). Since D6 every RID's
# host target is its flavor; before it, osx-arm64 shipped the darwin/amd64 flavor while a Mac arm64 host converted for
# darwin/arm64.
function Get-NugetgoFlavorGoos([Parameter(Mandatory)][string]$Flavor) { ($Flavor -split '/', 2)[0] }
function Get-NugetgoRidTarget([Parameter(Mandatory)][string]$Rid) {
    "$(Get-NugetgoFlavorGoos (Get-NugetgoRidFlavors)[$Rid])/$(if ($Rid -like '*-arm64') { 'arm64' } else { 'amd64' })"
}

# Where a package's imports are read: each shipped RID's own target, in map order, which covers every flavor and adds
# darwin/arm64, so a package a Mac arm64 host's build imports is never read as test-only.
function Get-NugetgoFlavorPlatforms { @((Get-NugetgoRidFlavors).Keys | ForEach-Object { Get-NugetgoRidTarget $_ } | Select-Object -Unique) }

# A flavor as a folder name: 'linux/arm64' is 'linux-arm64', the layout's own pair spelling.
function Get-NugetgoFlavorFolder([Parameter(Mandatory)][string]$Flavor) { $Flavor -replace '/', '-' }

# Does any flavor's assembly set differ in meaning from the proof's flavor? -Assemblies maps each flavor of
# Get-NugetgoFlavors to a hashtable of assembly file name -> bytes. A flavor that is missing is refused rather than
# read as neutral: the decision is only a measurement when every flavor was measured.
function Get-NugetgoFlavorDecision {
    [CmdletBinding()]
    param([Parameter(Mandatory)]$Assemblies, [Parameter(Mandatory)][string]$ProofFlavor, $RidFlavors = (Get-NugetgoRidFlavors))

    $flavors = @(Get-NugetgoFlavors -RidFlavors $RidFlavors)
    foreach ($each in $flavors) {
        if (-not $Assemblies.Contains($each)) { throw "no $each flavor was built: every flavor ($($flavors -join ', ')) is measured before a package is called neutral" }
    }
    if ($flavors -cnotcontains $ProofFlavor) { throw "the proof's flavor $ProofFlavor is not one of $($flavors -join ', ')" }

    $proof = $Assemblies[$ProofFlavor]
    $reasons = New-Object System.Collections.Generic.List[string]
    foreach ($each in $flavors) {
        if ($each -ceq $ProofFlavor) { continue }
        $flavor = $Assemblies[$each]
        $onlyProof = @($proof.Keys | Where-Object { -not $flavor.ContainsKey($_) } | Sort-Object)
        $onlyFlavor = @($flavor.Keys | Where-Object { -not $proof.ContainsKey($_) } | Sort-Object)
        if ($onlyProof.Count -or $onlyFlavor.Count) {
            $reasons.Add("[$each] the assembly set differs from ${ProofFlavor}'s (only in ${ProofFlavor}: $($onlyProof -join ', '); only in ${each}: $($onlyFlavor -join ', '))")
            continue
        }
        foreach ($name in @($proof.Keys | Sort-Object)) {
            $why = Get-GoLibEntryDifference -Name $name -A $proof[$name] -B $flavor[$name]
            if ($why) { $reasons.Add("[$each] $why (first: $ProofFlavor)") }
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
# set; anything else is refused by name rather than packed half-promoted. A -Cgo pack (NugetgoCgo.psm1) passes its own
# -RidFlavors, and -LibAssemblies for lib/: the reference flavor's REFERENCE assemblies, never an implementation.
function Set-NugetgoRidAssets {
    [CmdletBinding()]
    param([Parameter(Mandatory)][string]$Package, [Parameter(Mandatory)]$Assemblies, [Parameter(Mandatory)][string]$TargetFramework,
        $RidFlavors = (Get-NugetgoRidFlavors), $LibAssemblies = $null)

    Add-Type -AssemblyName System.IO.Compression, System.IO.Compression.FileSystem
    $rids = $RidFlavors
    $reference = if ($null -ne $LibAssemblies) { $LibAssemblies } else { $Assemblies[(Get-NugetgoReferenceFlavor)] }
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
# set of its flavor and nothing else, and RIDs that share a flavor carry the same bytes. No reason means accepted.
# -RidFlavors and -LibAssemblies as Set-NugetgoRidAssets takes them.
function Test-NugetgoRidLayout {
    [CmdletBinding()]
    param([Parameter(Mandatory)][string]$Package, [Parameter(Mandatory)]$Assemblies, [Parameter(Mandatory)][string]$TargetFramework,
        $RidFlavors = (Get-NugetgoRidFlavors), $LibAssemblies = $null)

    Add-Type -AssemblyName System.IO.Compression, System.IO.Compression.FileSystem
    $rids = $RidFlavors
    $libSet = if ($null -ne $LibAssemblies) { $LibAssemblies } else { $Assemblies[(Get-NugetgoReferenceFlavor)] }
    $reasons = New-Object System.Collections.Generic.List[string]
    $zip = [System.IO.Compression.ZipFile]::OpenRead($Package)
    try {
        $entries = @{}
        foreach ($e in $zip.Entries) { if ($e.FullName -like 'lib/*' -or $e.FullName -like 'runtimes/*') { $entries[$e.FullName] = Read-NugetgoZipEntryBytes $e } }
    }
    finally { $zip.Dispose() }

    $names = @($Assemblies[(Get-NugetgoReferenceFlavor)].Keys | Sort-Object)
    $same = { param($a, $b) $null -ne $a -and $null -ne $b -and [System.Linq.Enumerable]::SequenceEqual([byte[]]$a, [byte[]]$b) }
    foreach ($name in $names) {
        if (-not (& $same $entries["lib/$TargetFramework/$name"] $libSet[$name])) { $reasons.Add("lib/$TargetFramework/$name is not the reference ($(Get-NugetgoReferenceFlavor)) flavor's $(if ($null -ne $LibAssemblies) { 'reference assembly' } else { 'assembly' })") }
    }
    foreach ($rid in $rids.Keys) {
        $folder = "runtimes/$rid/lib/$TargetFramework/"
        $present = @($entries.Keys | Where-Object { $_.StartsWith($folder, [StringComparison]::Ordinal) } | ForEach-Object { $_.Substring($folder.Length) } | Sort-Object)
        if (($present -join '|') -cne ($names -join '|')) { $reasons.Add("$folder holds [$($present -join ', ')], not [$($names -join ', ')]"); continue }
        foreach ($name in $names) {
            if (-not (& $same $entries["$folder$name"] $Assemblies[$rids[$rid]][$name])) { $reasons.Add("$folder$name is not the $($rids[$rid]) flavor") }
        }
    }
    foreach ($each in Get-NugetgoFlavors -RidFlavors $rids) {
        $twins = @($rids.Keys | Where-Object { $rids[$_] -ceq $each })
        foreach ($rid in $twins | Select-Object -Skip 1) {
            foreach ($name in $names) {
                if (-not (& $same $entries["runtimes/$rid/lib/$TargetFramework/$name"] $entries["runtimes/$($twins[0])/lib/$TargetFramework/$name"])) { $reasons.Add("runtimes/$rid and runtimes/$($twins[0]) share $each but their $name differs") }
            }
        }
    }
    $stray = @($entries.Keys | Where-Object { $_ -match '^runtimes/([^/]+)/' -and -not $rids.Contains($Matches[1]) })
    if ($stray.Count) { $reasons.Add("runtimes/ holds a RID this layout does not ship: $($stray -join ', ')") }
    $reasons.ToArray()
}

# The read-back's layout decision, for every package nugetgo-pack.ps1 packs: a RID-specific package is read by
# Test-NugetgoRidLayout (-RidFlavors and -LibAssemblies passed through), and a neutral package carries no runtimes/
# entry at all. One call, so no branch of the pack can skip it. No reason means accepted.
function Test-NugetgoPackedLayout {
    [CmdletBinding()]
    param([Parameter(Mandatory)][string]$Package, [Parameter(Mandatory)]$Assemblies, [Parameter(Mandatory)][string]$TargetFramework,
        [switch]$RidSpecific, $RidFlavors = (Get-NugetgoRidFlavors), $LibAssemblies = $null)

    if ($RidSpecific) { return @(Test-NugetgoRidLayout -Package $Package -Assemblies $Assemblies -TargetFramework $TargetFramework -RidFlavors $RidFlavors -LibAssemblies $LibAssemblies) }
    Add-Type -AssemblyName System.IO.Compression, System.IO.Compression.FileSystem
    $zip = [System.IO.Compression.ZipFile]::OpenRead($Package)
    try { $runtimes = @($zip.Entries | Where-Object { $_.FullName -like 'runtimes/*' } | ForEach-Object { $_.FullName }) }
    finally { $zip.Dispose() }
    if ($runtimes.Count) { return @("a neutral package carries runtimes/ entries ($($runtimes[0])$(if ($runtimes.Count -gt 1) { " and $($runtimes.Count - 1) more" }))") }
    @()
}

# The package's third-party requires: the UNION of its flavors' (COORD ruling 2026-10-12, for cobra, whose command_win.go
# imports mousetrap on windows only). NuGet declares dependencies per target framework, never per RID, so -- as the
# go.* release does for its own dependencies -- every flavor's require is declared, and only the RID-matched assemblies
# load. The self-description's require lines take the same union. One module at two versions is refused: one go.mod
# resolves one version, so a disagreement means the flavors were not converted from the same module graph. And each
# -ThirdPartyPackage must name a module SOME flavor references (each flavor was asked with -AllowUnreferenced).
function Merge-NugetgoFlavorRequires {
    [CmdletBinding()]
    param([Parameter(Mandatory)]$Requires, [string[]]$ThirdPartyPackage = @(), $RidFlavors = (Get-NugetgoRidFlavors))

    $byModule = [ordered]@{}
    foreach ($each in Get-NugetgoFlavors -RidFlavors $RidFlavors) {
        foreach ($r in @($Requires[$each])) {
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
    param([Parameter(Mandatory)][string]$ProofFlavor, [switch]$RidSpecific, $RidFlavors = (Get-NugetgoRidFlavors))

    if (-not $RidSpecific) { return $null }
    $others = @(Get-NugetgoFlavors -RidFlavors $RidFlavors | Where-Object { $_ -cne $ProofFlavor })
    $named = if ($others.Count -gt 1) { ($others[0..($others.Count - 2)] -join ', ') + ' and ' + $others[-1] } else { $others[0] }
    "Flavors: this package carries RID-specific assemblies; its go2cs/source-metadata.txt describes the $ProofFlavor flavor's package metadata, and the $named flavors' metadata is not described."
}

Export-ModuleMember -Function Get-NugetgoRidFlavors, Get-NugetgoReferenceFlavor, Get-NugetgoFlavors, Get-NugetgoFlavorPlatforms, Get-NugetgoFlavorGoos, Get-NugetgoRidTarget, Get-NugetgoFlavorFolder, Get-NugetgoFlavorDecision, Set-NugetgoRidAssets, Test-NugetgoRidLayout, Test-NugetgoPackedLayout, Merge-NugetgoFlavorRequires, Get-NugetgoFlavorDisclosure
