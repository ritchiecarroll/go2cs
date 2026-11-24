# NugetgoCgo.psm1 - Gbtc
# Copyright (c) 2026 The go2cs Authors. All rights reserved.
#
# SPDX-License-Identifier: AGPL-3.0-only
# Use of this source code is governed by the GNU Affero General Public License
# version 3 only, which can be found in the LICENSE file.

# THE -Cgo PACK OF A MODULE WITH A CGO PACKAGE (the cgo-against-go.* item, INC 2b; COORD's ruling (a), 2026-11-21, and
# its condition 2 as reworded the same day).
#
# A cgo package's C binding is per target: its emission names the target it was converted for, folds C's type sizes
# from that target's compiler, and refuses to call C anywhere else. So nugetgo-pack.ps1 -Cgo converts every flavor with
# `-recurse=nuget -cgo`, each with the pinned zig for that flavor's target as CC, and the package is always RID-specific:
#
#   * runtimes/<rid>/lib/<tfm>/ carries each packed RID's own flavor, for the RIDs whose go.* bytes are TRUE for it.
#     osx-arm64 is OUT BY NAME: the go.* packages ship it the darwin/amd64 flavor, so its cgo flavor would bind a C
#     layout to go.* bytes built for another architecture. It joins when true darwin/arm64 go.* assets ship; the guard
#     below needs no change then.
#   * lib/<tfm>/ carries REFERENCE assemblies only (the reference flavor's), never an implementation another RID could
#     fall back to: NuGet takes lib/ for any RID with no runtimes/ folder of its own, and a reference assembly refuses to
#     load (BadImageFormatException), where an implementation would bind another target's C.
#   * build/<id>.targets fails a consumer build by name (NUGETGOCGO001) when the build's RID -- $(RuntimeIdentifier),
#     or for a portable build the SDK's own $(NETCoreSdkRuntimeIdentifier) -- has no runtimes/<rid>/lib/ folder in the
#     package. It reads the set from the package's own folders, never from a list.
#
# Measured before this was written (C2's probe, 2026-11-21, dotnet 10.0.404): with a runtimes/ asset for one RID and a
# reference-only lib/, a build for another RID SUCCEEDS and binds the reference assembly; NuGet has no "no matching
# asset" error for a lib/ asset. A portable or RID build then fails at load with BadImageFormatException. With the
# guard, the same builds fail with its error, naming the RID; a build for a packed RID still builds.

Import-Module (Join-Path $PSScriptRoot 'NugetgoFlavors.psm1')
Import-Module (Join-Path $PSScriptRoot '../pushnuget/GoApiCompat.psm1')

# The pinned C toolchain, .github/cgo-toolchain.ps1's (Test-NugetgoCgo.ps1 holds the two equal). Every flavor
# cross-compiles with it: zig carries each target's C library headers and the darwin SDK stubs.
$script:ZigVersion = '0.17.0'

# Each flavor's C target, as zig names it. The converter's R3 reads the triple back from `CC -dumpmachine` and refuses
# one that is not the flavor's (cgoRoute.go cgoHostTarget): gnu for linux and windows, never musl, msvc or android.
$script:Triples = [ordered]@{
    'windows/amd64' = 'x86_64-windows-gnu'
    'linux/amd64'   = 'x86_64-linux-gnu'
    'darwin/amd64'  = 'x86_64-macos'
    'linux/arm64'   = 'aarch64-linux-gnu'
}

# The RIDs a -Cgo pack leaves out, each with the reason its README, VALIDATION.md and self-description state.
function Get-NugetgoCgoExcludedRids {
    [ordered]@{
        'osx-arm64' = 'the go.* packages ship osx-arm64 their darwin/amd64 flavor, and a cgo package''s C binding is per target, so osx-arm64 waits for true darwin/arm64 go.* assets'
    }
}

# The release's RID map less the exclusions, in its order: the RIDs a -Cgo pack carries.
function Get-NugetgoCgoRidFlavors {
    $excluded = Get-NugetgoCgoExcludedRids
    $all = Get-NugetgoRidFlavors
    $map = [ordered]@{}
    foreach ($rid in $all.Keys) { if (-not $excluded.Contains($rid)) { $map[$rid] = $all[$rid] } }
    $map
}

function Get-NugetgoCgoZigVersion { $script:ZigVersion }

function Get-NugetgoCgoTriple([Parameter(Mandatory)][string]$Flavor) {
    if (-not $script:Triples.Contains($Flavor)) { throw "REFUSED: no C target for the flavor $Flavor (NugetgoCgo.psm1 knows $(@($script:Triples.Keys) -join ', '))" }
    $script:Triples[$Flavor]
}

# CC for a flavor, as go reads it: split on spaces, with quotes kept together (cmd/internal/quoted).
function Get-NugetgoCgoCompiler([Parameter(Mandatory)][string]$Zig, [Parameter(Mandatory)][string]$Flavor) {
    $path = if ($Zig -match '\s') { "`"$Zig`"" } else { $Zig }
    "$path cc -target $(Get-NugetgoCgoTriple $Flavor)"
}

# Is -Zig the pinned zig? Returns the reason it is not, or $null.
function Test-NugetgoCgoZig([Parameter(Mandatory)][string]$Zig) {
    if (-not (Test-Path -LiteralPath $Zig -PathType Leaf)) { return "-Zig: no file at $Zig" }
    $version = "$(& $Zig version 2>&1 | Select-Object -First 1)".Trim()
    if ($LASTEXITCODE -ne 0) { return "-Zig: $Zig version exited $LASTEXITCODE" }
    if ($version -cne $script:ZigVersion) { return "-Zig: $Zig is zig $version, not the pinned $($script:ZigVersion) (.github/cgo-toolchain.ps1)" }
    $null
}

function Join-NugetgoCgoList([string[]]$Items) {
    if ($Items.Count -le 1) { return ($Items -join '') }
    ($Items[0..($Items.Count - 2)] -join ', ') + ' and ' + $Items[-1]
}

# What the pack states about its RIDs: VALIDATION.md carries it as a quoted paragraph, and the self-description as
# comment lines.
function Get-NugetgoCgoStatement([Parameter(Mandatory)][string]$PackageId) {
    $excluded = Get-NugetgoCgoExcludedRids
    $packed = @((Get-NugetgoCgoRidFlavors).Keys)
    $out = "Cgo: $PackageId converts a Go cgo package, whose C binding is per target. It carries assemblies for $(Join-NugetgoCgoList $packed) only."
    foreach ($rid in $excluded.Keys) { $out += " $rid is not packed: $($excluded[$rid])." }
    $out += " A build for any other RID fails with NUGETGOCGO001. A portable build (no -r) is checked against the .NET SDK's own RID, so on a host the package does not carry it is refused even when it will run elsewhere: build with -r and one of the packed RIDs."
    $out += " lib/ holds reference assemblies only, so a portable build deployed to a RID the package does not carry fails at load, never binding another target's code."
    $out += " A portable build made on a packed host and run on linux-musl (Alpine) takes runtimes/linux-x64 by .NET's RID fallback, whose C binding is glibc's: there the package's go.lib refuses those bindings at load, before any C call (its run-time target check). A build for linux-musl itself, -r or portable on such a host, is refused by NUGETGOCGO001."
    $out
}

# build/<id>.targets. Every name is suffixed with the package id, so two -Cgo packages in one build keep their own target.
function New-NugetgoCgoRidGuard([Parameter(Mandatory)][string]$PackageId) {
    $key = $PackageId -replace '[^A-Za-z0-9_]', '_'
    $excluded = Get-NugetgoCgoExcludedRids
    $note = (@($excluded.Keys | ForEach-Object { "$_ is not packed: $($excluded[$_])." }) -join ' ')
    # MSBuild's escapes first, then XML's: an XML entity's ';' must reach the parser as written.
    $esc = { param($s) [System.Security.SecurityElement]::Escape(($s -replace '%', '%25' -replace '\$', '%24' -replace '@', '%40' -replace ';', '%3B')) }
    @"
<Project>
  <!-- Generated by nugetgo-pack.ps1 -Cgo (src/tools/nugetgo/NugetgoCgo.psm1). $(& $esc $PackageId) converts a Go cgo package,
       whose C binding is per target: a build for a RID with no runtimes/<rid>/lib/ folder in this package is refused
       here, by name. The set is read from the package's own folders. -->
  <Target Name="_NugetgoCgoRidGuard_$key" BeforeTargets="CoreCompile">
    <PropertyGroup>
      <_NugetgoCgoRuntimes_$key>`$([System.IO.Path]::GetFullPath('`$(MSBuildThisFileDirectory)../runtimes'))</_NugetgoCgoRuntimes_$key>
      <_NugetgoCgoRid_$key>`$(RuntimeIdentifier)</_NugetgoCgoRid_$key>
      <_NugetgoCgoRid_$key Condition="'`$(_NugetgoCgoRid_$key)' == ''">`$(NETCoreSdkRuntimeIdentifier)</_NugetgoCgoRid_$key>
    </PropertyGroup>
    <ItemGroup>
      <_NugetgoCgoPackedRid_$key Include="`$([System.IO.Directory]::GetDirectories('`$(_NugetgoCgoRuntimes_$key)'))" />
    </ItemGroup>
    <Error Condition="!Exists('`$(_NugetgoCgoRuntimes_$key)/`$(_NugetgoCgoRid_$key)/lib')" Code="NUGETGOCGO001"
           Text="$(& $esc $PackageId) converts a Go cgo package, whose C binding is per target. It carries assemblies for @(_NugetgoCgoPackedRid_$key->'%(Filename)%(Extension)', ', ') only, and this build is for `$(_NugetgoCgoRid_$key)$(if ($note) { '. ' + (& $esc $note) } else { '.' }) Build with -r and one of the packed RIDs (a portable build takes the .NET SDK's own RID)." />
  </Target>
</Project>
"@
}

# Is the assembly a REFERENCE assembly? Read from its metadata: the compiler marks one with
# System.Runtime.CompilerServices.ReferenceAssemblyAttribute, and the runtime refuses to load it for execution.
function Test-NugetgoReferenceAssembly([Parameter(Mandatory)][byte[]]$Bytes) {
    $stream = New-Object System.IO.MemoryStream(, $Bytes)
    $pe = New-Object System.Reflection.PortableExecutable.PEReader($stream)
    try {
        if (-not $pe.HasMetadata) { return $false }
        $md = [System.Reflection.Metadata.PEReaderExtensions]::GetMetadataReader($pe)
        foreach ($handle in $md.GetAssemblyDefinition().GetCustomAttributes()) {
            $ctor = $md.GetCustomAttribute($handle).Constructor
            if ($ctor.Kind -ne [System.Reflection.Metadata.HandleKind]::MemberReference) { continue }
            $parent = $md.GetMemberReference([System.Reflection.Metadata.MemberReferenceHandle]$ctor).Parent
            if ($parent.Kind -ne [System.Reflection.Metadata.HandleKind]::TypeReference) { continue }
            $type = $md.GetTypeReference([System.Reflection.Metadata.TypeReferenceHandle]$parent)
            if ($md.GetString($type.Namespace) -ceq 'System.Runtime.CompilerServices' -and $md.GetString($type.Name) -ceq 'ReferenceAssemblyAttribute') { return $true }
        }
        $false
    }
    finally { $pe.Dispose(); $stream.Dispose() }
}

# A6's floor (COORD's push-lift ruling (a)): the restored go.lib's go.lib.targets declares GoCgoRunTimeTargetCheck, so its
# GoCgoLibraries.Register refuses bindings at load in a process they were not converted for. Returns the reason it does
# not, or $null.
function Test-NugetgoCgoGoLibFloor([AllowEmptyString()][string]$GoLibTargets, [string]$Version) {
    # Parsed, never matched as text: a commented-out element must not pass (P3's read of INC 3, NIT 4).
    $declared = $false
    try {
        [xml]$xml = $GoLibTargets
        $declared = @($xml.SelectNodes("//*[local-name()='GoCgoRunTimeTargetCheck']") | Where-Object { $_.InnerText.Trim() -ceq 'true' }).Count -gt 0
    }
    catch { $declared = $false }
    if ($declared) { return $null }
    "the go.lib $Version this pack restores does not declare GoCgoRunTimeTargetCheck: its GoCgoLibraries.Register does not refuse C bindings at load in a process they were not converted for (A6), so a -Cgo pack built on it is never pushed. Pack against a go.* release that carries A6"
}

# The self-description's record that the pack's go.lib floor carries A6; src/_nugetpush.ps1 reads its "a6: " prefix.
function Get-NugetgoCgoA6Note([Parameter(Mandatory)][string]$Version) {
    "a6: the go.lib this package was packed against, $Version, the floor of its go.lib range, declares GoCgoRunTimeTargetCheck: its bindings are refused at load in a process they were not converted for"
}

# R7: every packed RID's assembly has exactly the public surface of lib/'s reference assembly of the same name, read by
# the pinned ApiCompat (src/tools/pushnuget/GoApiCompat.psm1's engine) in strict mode, so a member one side has and the
# other lacks is named either way. -Reference maps assembly name -> path; -Runtimes maps RID -> (assembly name -> path).
# Returns Compared (the comparisons that printed a verdict) and Reasons (none means accepted).
function Test-NugetgoCgoSurface {
    [CmdletBinding()]
    param([Parameter(Mandatory)][string]$RepoRoot, [Parameter(Mandatory)][string]$WorkDir, [Parameter(Mandatory)]$Reference, [Parameter(Mandatory)]$Runtimes)

    # The pinned tool, restored and its version checked, as the pack's own binary-compatibility guard does: a hosted leg
    # has no restored tool until this runs (the first hosted read, converter-cgo 44954804095: "Run dotnet tool restore").
    [void](Initialize-GoApiCompatTool -RepoRoot $RepoRoot)
    $clean = 'APICompat ran successfully without finding any breaking changes.'
    $breaks = 'API breaking changes found.'
    $reasons = New-Object System.Collections.Generic.List[string]
    $compared = 0
    New-Item -ItemType Directory -Force $WorkDir | Out-Null
    foreach ($rid in @($Runtimes.Keys)) {
        $set = $Runtimes[$rid]
        foreach ($extra in @($set.Keys | Where-Object { -not $Reference.Contains($_) } | Sort-Object)) { $reasons.Add("[$rid] $extra has no reference assembly in lib/") }
        foreach ($name in @($Reference.Keys | Sort-Object)) {
            if (-not $set.Contains($name)) { $reasons.Add("[$rid] runtimes/$rid carries no $name"); continue }
            # Each side in a folder of its own, under the RID's and lib/'s names, so a diagnostic reads as the package path.
            $left = Join-Path $WorkDir "lib/$name"
            $right = Join-Path $WorkDir "runtimes/$rid/$name"
            New-Item -ItemType Directory -Force (Split-Path $left), (Split-Path $right) | Out-Null
            Copy-Item -LiteralPath $Reference[$name] $left -Force
            Copy-Item -LiteralPath $set[$name] $right -Force
            $ErrorActionPreference = 'Continue'
            Push-Location $RepoRoot
            try { $lines = @(& dotnet tool run apicompat -- -l ($left -replace '\\', '/') -r ($right -replace '\\', '/') --strict-mode 2>&1 | ForEach-Object { "$_" }); $exit = $LASTEXITCODE }
            finally { Pop-Location; $ErrorActionPreference = 'Stop' }
            $isClean = @($lines | Where-Object { $_.Trim() -ceq $clean }).Count
            $isBreak = @($lines | Where-Object { $_.Trim().StartsWith($breaks, [StringComparison]::Ordinal) }).Count
            $diagnostics = @($lines | Where-Object { $_ -match '^CP\d{4}: ' })
            if ($isClean + $isBreak -ne 1) { $reasons.Add("[$rid] $name`: the tool reported $($isClean + $isBreak) verdict(s) (exit $exit): $(($lines | Select-Object -Last 3) -join ' | ')"); continue }
            $compared++
            if ($isBreak) {
                if (-not $diagnostics.Count) { $reasons.Add("[$rid] $name`: breaking changes reported and none listed (exit $exit)"); continue }
                foreach ($d in $diagnostics) { $reasons.Add("[$rid] $name`: $d") }
            }
            elseif ($exit -ne 0) { $reasons.Add("[$rid] $name`: no breaking change reported and the tool exited $exit") }
        }
    }
    [pscustomobject]@{ Compared = $compared; Reasons = $reasons.ToArray() }
}

Export-ModuleMember -Function Get-NugetgoCgoExcludedRids, Get-NugetgoCgoRidFlavors, Get-NugetgoCgoZigVersion, Get-NugetgoCgoTriple,
    Get-NugetgoCgoCompiler, Test-NugetgoCgoZig, Get-NugetgoCgoStatement, New-NugetgoCgoRidGuard, Test-NugetgoReferenceAssembly, Test-NugetgoCgoSurface, Test-NugetgoCgoGoLibFloor, Get-NugetgoCgoA6Note
