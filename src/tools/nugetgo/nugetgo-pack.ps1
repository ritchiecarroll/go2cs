<#
.SYNOPSIS
    Pack one converted Go module as ONE nugetgo.* package (docs/PLAN-nugetgo.md section 8, B2-B6; the multi-package
    module design's D6/D7).

.DESCRIPTION
    Input is a `go2cs -recurse=nuget` output root. Every main-module LIBRARY package under <RecurseRoot>/src/<module>
    (one assembly per Go package; a main package is excluded, and so is each package -ExcludePackage names) is built
    through its own project, whose go.* references are PackageReferences at $(GoStdLibVersion). A generated module
    pack project then carries each package's assembly
    under lib/<tfm>/, and declares the UNION of the packages' public go.* PackageReferences at the same
    $(GoStdLibVersion). The pack passes GoStdLibVersion as B4's range, [<ClosureVersion>, <next Go minor>), so NuGet
    writes the range from the PackageReference route itself, with no rewrite of any edge. The lower bound is the
    closure version packed against: a PARAMETER, never a literal. NuGet NORMALIZES the range as it writes the nuspec
    (the upper bound 1.25 reads 1.25.0): the same range, ACCEPTED as such (COORD, 2026-09-30: B4 rules the range, not
    its bytes).

    Identity (B2, B3) comes from NugetgoIdentity.psm1. A rehearsal carries -RehearsalSuffix (e.g. local.1), so a
    rehearsal can never mint a real release version: NuGet caches by id+version immutably. A version with no suffix is
    packed only under -Release.

    Metadata (B6): upstream's Copyright lines and license file verbatim, under its own name (NugetgoLicense.psm1),
    the PROOF description, RepositoryUrl = the per-module conversion-source repo. D7: the module's MODULE.md ships as
    VALIDATION.md, with every per-package proof page beside it. The proof must be OF the packed bytes: each MODULE.md
    row's input digest must be the GoInputDigest its packed project records (NugetgoValidationBinding.psm1), so a proof
    from other sources, other options, another target or another converter is refused by name.

    Assembly copyright (owner ruling 2026-10-04): each packed assembly's own copyright attribute names the UPSTREAM
    holder -- the same Copyright lines, then one go2cs scaffolding line -- and its company and authors are -Authors.
    The pack writes them into <RecurseRoot>/src/<module path>/Directory.Build.targets, which the conversion repository
    pushed from that tree carries, so a rebuild from it produces the same attribute. A module whose every Copyright
    line already names The go2cs Authors keeps the csproj template's values and gets no such file. The read-back
    checks each dll taken out of the nupkg.

    Self-description (docs/PLAN-nugetgo.md section 5, stage S2, format v1 as COORD ruled it on 2026-10-02): the
    package carries go2cs/source-metadata.txt -- the module path and version, the go2cs corpus release it was built
    against (-ClosureVersion), one `package` line per packed Go package, one `require` line per third-party module
    the packed packages reference, and each package's package_info.cs metadata -- so a consuming converter can map
    it without a converted tree on disk. The Go side writes it (src/go2cs/internal/gensourcemeta, the same package
    that parses it), and the read-back runs that parser on the copy taken back OUT of the nupkg. A third-party
    module the packed packages reference becomes a NuGet dependency on the package -ThirdPartyPackage names (owner
    ruling B4: the first revision built for the same corpus); one the caller does not name is refused by name.

    README and icon (owner feedback on the hashset 1.0.0 nuget.org page, 2026-10-10): the package README is
    generated in the go.* standard library packages' style (NugetgoReadme.psm1) -- the package ID, the PROOF text, a
    badge row, the Go package's synopsis (internal/gensynopsis, read from the module's own source) and a license line
    naming -LicenseSpdx -- and VALIDATION.md is packed beside it as a file the README links. Its links into the
    conversion repository name the tag nuget-<release version> (COORD ruling 2026-10-10), which COORD mints on the
    conversion commit before the package is signed; a rehearsal names its release's tag, since a rehearsal version
    gets none. The icon is the go2cs.png
    the conversion carries, or -Icon. The read-back refuses a README with inline code in a heading or a table wider
    than three columns or a link naming HEAD, and a package with no icon.

    The restore is isolated: a nuget.config with <clear/> and only -Feed as a source, and a private NUGET_PACKAGES.
    The user's global packages folder is censused for nugetgo.* and go.* before and after, and the run fails if it
    grew. Nothing is pushed anywhere, and nothing is signed: signing is the release's last step.
#>
#Requires -Version 5.1
[CmdletBinding()]
param(
    [Parameter(Mandatory)][string]$ModulePath,
    [Parameter(Mandatory)][string]$GoVersion,
    [int]$Revision = 0,
    [Parameter(Mandatory)][string]$RecurseRoot,
    [string]$ValidationDir,
    [string]$UnvalidatedReason,
    [Parameter(Mandatory)][string]$ClosureVersion,
    [Parameter(Mandatory)][string]$Feed,
    [Parameter(Mandatory)][string]$OutDir,
    [Parameter(Mandatory)][string]$Scratch,
    [Parameter(Mandatory)][string]$RepositoryUrl,
    [Parameter(Mandatory)][string]$Upstream,
    # The module's own author publishes: the description takes the author's form, and -RepositoryUrl must sit under the
    # module path's own host/org (Get-NugetgoDescription, owner ruling 2026-10-02).
    [switch]$UpstreamPublishes,
    [string]$LicenseFile,
    # The upstream license as an SPDX expression, for the README's license line. It is stated, never inferred from the
    # license file (NugetgoLicense.psm1).
    [string]$LicenseSpdx,
    # The package icon, PNG or JPEG up to 1 MB; the go2cs icon the conversion carries when not given. A module author
    # publishing their own module sets their own.
    [string]$Icon,
    [string]$RehearsalSuffix,
    [switch]$Release,
    [string[]]$ExistingIds = @(),
    [string[]]$ThirdPartyPackage = @(),
    # Library packages of the module left out of the pack, by import path (a package holding the module's own test fixtures).
    [string[]]$ExcludePackage = @(),
    [string[]]$VList,
    [string]$Authors = 'go2cs conversion',
    [string]$Gpf = $(if ($env:NUGET_PACKAGES) { $env:NUGET_PACKAGES } else { Join-Path ([Environment]::GetFolderPath('UserProfile')) '.nuget/packages' })
)

$ErrorActionPreference = 'Stop'
Import-Module (Join-Path $PSScriptRoot 'NugetgoClosure.psm1') -Force
Import-Module (Join-Path $PSScriptRoot 'NugetgoIdentity.psm1') -Force
Import-Module (Join-Path $PSScriptRoot 'NugetgoSelfDescription.psm1') -Force
Import-Module (Join-Path $PSScriptRoot 'NugetgoValidationBinding.psm1') -Force
Import-Module (Join-Path $PSScriptRoot 'NugetgoLicense.psm1') -Force
Import-Module (Join-Path $PSScriptRoot 'NugetgoHostPaths.psm1') -Force
Import-Module (Join-Path $PSScriptRoot 'NugetgoReadme.psm1') -Force
Add-Type -AssemblyName System.IO.Compression, System.IO.Compression.FileSystem

function Refuse([string]$why) { throw "REFUSED: $why" }

# ---- identity ----------------------------------------------------------------------------------------------------
if (-not $Release -and -not $RehearsalSuffix) { Refuse 'a pack is a rehearsal (-RehearsalSuffix) unless -Release says otherwise: a real version is never minted for rehearsal content' }
if ($Release -and $RehearsalSuffix) { Refuse '-Release and -RehearsalSuffix are exclusive' }
if ($RehearsalSuffix -and $RehearsalSuffix -notmatch '^[0-9a-z-]+(\.[0-9a-z-]+)*$') { Refuse "-RehearsalSuffix '$RehearsalSuffix' is not a lowercase NuGet prerelease label" }

$id = Get-NugetgoPackageId -ModulePath $ModulePath -ExistingIds $ExistingIds
if (-not $id.Id) { Refuse "package ID: $($id.Reason)" }
$ver = Get-NugetgoVersion -GoVersion $GoVersion -Revision $Revision -VList $VList
if ($ver.Refused) { Refuse "package version: $($ver.Reason)" }
$packageVersion = if ($RehearsalSuffix) {
    if ($ver.Version.Contains('-')) { "$($ver.Version).$RehearsalSuffix" } else { "$($ver.Version)-$RehearsalSuffix" }
} else { $ver.Version }

# ---- B4: the stdlib range, from the closure version ------------------------------------------------------------------
if ($ClosureVersion -notmatch '^(?<maj>\d+)\.(?<min>\d+)\.\d+(\.\d+)?(-[0-9A-Za-z.-]+)?$') { Refuse "-ClosureVersion '$ClosureVersion' is not a stdlib package version" }
$goMajor = [int]$Matches['maj']; $goMinor = [int]$Matches['min']   # captured now: a later -match overwrites $Matches
$stdlibRange = "[$ClosureVersion, $goMajor.$($goMinor + 1))"

# ---- the module's library packages ---------------------------------------------------------------------------------
$moduleSrc = Join-Path (Join-Path $RecurseRoot 'src') $ModulePath
if (-not (Test-Path $moduleSrc)) { Refuse "no converted module at $moduleSrc (a -recurse=nuget output root is expected)" }
$projects = @(Get-ChildItem -Path $moduleSrc -Recurse -Filter '*.csproj' | Where-Object { $_.Name -notlike '*.tests.csproj' } | Sort-Object FullName)
$candidates = New-Object System.Collections.Generic.List[object]
foreach ($p in $projects) {
    [xml]$x = Get-Content -Raw -LiteralPath $p.FullName
    $outputType = @($x.Project.PropertyGroup | ForEach-Object { $_.OutputType } | Where-Object { $_ })[0]
    if ($outputType -ne 'Library') { Write-Host "  excluded (OutputType $outputType, a Go main): $($p.FullName.Substring($moduleSrc.Length))"; continue }
    $candidates.Add($p)
}
# -ExcludePackage leaves a library out (Select-NugetgoPackedLibraries refuses an exclusion a packed package references).
$selected = @(Select-NugetgoPackedLibraries -RecurseRoot $RecurseRoot -Libraries $candidates.ToArray() -ExcludePackage $ExcludePackage)
$libraries = New-Object System.Collections.Generic.List[object]
$goRefs = New-Object 'System.Collections.Generic.SortedSet[string]' ([StringComparer]::OrdinalIgnoreCase)
foreach ($p in $candidates) {
    if ($selected -notcontains $p) { Write-Host "  excluded (-ExcludePackage): $($p.FullName.Substring($moduleSrc.Length))"; continue }
    $libraries.Add($p)
    [xml]$x = Get-Content -Raw -LiteralPath $p.FullName
    foreach ($ref in @($x.Project.ItemGroup | ForEach-Object { $_.PackageReference } | Where-Object { $_ })) {
        if ($ref.Include -like 'go.*' -and $ref.Version -eq '$(GoStdLibVersion)' -and $ref.PrivateAssets -ne 'all') { [void]$goRefs.Add($ref.Include) }
    }
}
if ($libraries.Count -eq 0) { Refuse "no library package under $moduleSrc" }

# ---- S2: what the self-description lists ------------------------------------------------------------------------------
$packed = @(Get-NugetgoPackedPackages -RecurseRoot $RecurseRoot -Libraries $libraries.ToArray())
$requires = @(Get-NugetgoThirdPartyRequires -RecurseRoot $RecurseRoot -Libraries $libraries.ToArray() -ThirdPartyPackage $ThirdPartyPackage)

# ---- B6 metadata -----------------------------------------------------------------------------------------------------
# The module's own source in the module cache, where the license file and the package synopsis are read. The cache
# spells each uppercase letter of the path as '!' and its lowercase (golang.org/x/mod/module.EscapePath).
$moduleCacheDir = Join-Path (& go env GOMODCACHE).Trim() ([regex]::Replace("$ModulePath@$GoVersion", '[A-Z]', { '!' + $args[0].Value.ToLowerInvariant() }))
# The upstream license file under its own name: the converter's moduleLicenseNames, in its order (NugetgoLicense.psm1).
if (-not $LicenseFile) {
    $found = Find-NugetgoModuleLicense -ModuleDir $moduleCacheDir
    if (-not $found.Name) { Refuse "no upstream license file: $($found.Reason)" }
    $LicenseFile = $found.Path
}
if (-not (Test-Path -LiteralPath $LicenseFile -PathType Leaf)) { Refuse "no upstream license file at $LicenseFile" }
$licenseName = Split-Path -Leaf $LicenseFile
if (-not $LicenseSpdx) { Refuse '-LicenseSpdx is required: the README names the upstream license as an SPDX expression (for example MIT or BSD-3-Clause), stated by the caller, never inferred' }
if (-not (Test-NugetgoSpdx $LicenseSpdx)) { Refuse "-LicenseSpdx '$LicenseSpdx' is not an SPDX expression (identifiers joined by AND, OR or WITH)" }
$copyright = (@(Get-NugetgoCopyrightLines -LicenseFile $LicenseFile) -join '; ')
if (-not $copyright) { Refuse "the upstream license file carries no Copyright line: $LicenseFile" }
# Each packed assembly's own copyright attribute names the UPSTREAM holder too (owner ruling 2026-10-04): the same
# Copyright lines, then the go2cs scaffolding line. It is set by a Directory.Build.targets in the converted module's
# own directory, so the conversion repository pushed from <RecurseRoot> rebuilds the same attribute. Escaped -p:
# globals would not: they reach every project the build touches (a third-party pkg/ project reference included),
# split at ',' and ';', and live in no file a rebuild from the pushed sources reads.
$assemblyCopyright = Get-NugetgoAssemblyCopyright -LicenseFile $LicenseFile
$moduleTargets = Join-Path $moduleSrc 'Directory.Build.targets'
$ownTargets = (Test-Path -LiteralPath $moduleTargets) -and ([System.IO.File]::ReadAllText($moduleTargets)).Contains((Get-NugetgoTargetsMarker))
if ((Test-Path -LiteralPath $moduleTargets) -and -not $ownTargets) { Refuse "$moduleTargets exists and was not written by nugetgo-pack.ps1: the packed assemblies' copyright cannot be set beside it" }
if ($assemblyCopyright.Skip) {
    if ($ownTargets) { Remove-Item -LiteralPath $moduleTargets }
    Write-Host "  assembly copyright: the csproj template's ($($assemblyCopyright.Reason))"
}
else {
    [System.IO.File]::WriteAllText($moduleTargets, (New-NugetgoAssemblyMetadataTargets -ModulePath $ModulePath -Copyright $assemblyCopyright.Copyright -Company $Authors -Authors $Authors),
        (New-Object System.Text.UTF8Encoding($false)))
    Write-Host "  assembly copyright: $($assemblyCopyright.Copyright)  (set by $moduleTargets)"
}
# The description, exactly as ruled (COORD, within B6, 2026-09-30). The Go release is the closure's own (its first three
# components), never a literal, so the text cannot outlive the corpus it describes. The same two sentences head
# VALIDATION.md and are the release notes.
$goRelease = ($ClosureVersion -split '[.-]')[0..2] -join '.'
$described = Get-NugetgoDescription -ModulePath $ModulePath -GoVersion $GoVersion -GoRelease $goRelease -Upstream $Upstream `
    -RepositoryUrl $RepositoryUrl -UpstreamPublishes:$UpstreamPublishes
if ($described.Refused) { Refuse "package description: $($described.Reason)" }
$description = $described.Description

# D7: a validated module ships its MODULE.md as VALIDATION.md, the per-package pages beside it. A module that cannot
# validate yet packs ONLY as a rehearsal of its shape (-UnvalidatedReason), whose VALIDATION.md says so; such a
# package is never a release.
$pages = @()
if ($UnvalidatedReason) {
    if ($Release) { Refuse 'an unvalidated module is never packed as a release (-UnvalidatedReason with -Release)' }
    if ($ValidationDir) { Refuse '-UnvalidatedReason and -ValidationDir are exclusive' }
    $validationPage = Join-Path $Scratch 'UNVALIDATED.md'
    New-Item -ItemType Directory -Force $Scratch | Out-Null
    [System.IO.File]::WriteAllText($validationPage, ("# ``$ModulePath`` -- NOT VALIDATED`n`n" +
        "This package is a pack-shape rehearsal of the go2cs conversion of ``$ModulePath`` $GoVersion. Its Go tests have " +
        "not been converted and compared, so it carries no validation proof, and it is not published.`n`n$UnvalidatedReason`n"),
        (New-Object System.Text.UTF8Encoding($false)))
}
else {
    if (-not $ValidationDir) { Refuse 'a validated module needs -ValidationDir (D7), or -UnvalidatedReason for a pack-shape rehearsal' }
    $validationPage = Join-Path $ValidationDir 'MODULE.md'
    if (-not (Test-Path -LiteralPath $validationPage)) { Refuse "no MODULE.md in $ValidationDir (a validated module is required; D7)" }
    $pages = @(Get-ChildItem -LiteralPath $ValidationDir -Filter '*.md' -Recurse | Where-Object { $_.Name -ne 'MODULE.md' })
    $totals = Get-NugetgoProofTotals -ModuleSummary ([System.IO.File]::ReadAllText($validationPage))
    if ($totals.Packages -eq 0) { Refuse "$validationPage has no package row: the README's Tests badge has no totals to state" }
    # The proof must be OF the packed bytes (the nugetgo rehearsal, 2026-10-09, gap 4): every MODULE.md row's input
    # digest is the GoInputDigest its packed project records. $packed lists the libraries in $libraries' order.
    $packedDigests = @{}
    for ($i = 0; $i -lt $libraries.Count; $i++) { $packedDigests[$packed[$i].ImportPath] = Get-NugetgoProjectInputDigest -ProjectFile $libraries[$i].FullName }
    $proofDigests = Get-NugetgoProofInputDigests -ModuleSummary ([System.IO.File]::ReadAllText($validationPage))
    $unbound = @(Test-NugetgoValidationBinding -Proof $proofDigests -Packed $packedDigests)
    if ($unbound.Count) { Refuse "the validation proof is not a proof of the packed tree -- $($unbound -join '; ')" }
    foreach ($path in @($packedDigests.Keys | Sort-Object)) {
        Write-Host "  proof: $path $(if ($proofDigests.ContainsKey($path)) { "bound at $($packedDigests[$path])" } else { 'has no proof row (no Go tests)' })"
    }
}

# ---- the package README and icon ---------------------------------------------------------------------------------------
$packageIcon = Resolve-NugetgoPackageIcon -Icon $Icon -ConvertedDirectories @($libraries | ForEach-Object { $_.DirectoryName })
if ($packageIcon.Reason) { Refuse $packageIcon.Reason }
# The synopsis of the module's root package, from its Go source. A module whose root package is not packed has no one
# package to speak for it, and its README has no synopsis.
$synopsis = ''
if (@($packed | Where-Object { $_.ImportPath -ceq $ModulePath }).Count) {
    if (-not (Test-Path -LiteralPath $moduleCacheDir -PathType Container)) { Refuse "no module source at $moduleCacheDir for the README's synopsis (go mod download $ModulePath@$GoVersion)" }
    Push-Location (Join-Path (Split-Path (Split-Path $PSScriptRoot)) 'go2cs')
    try { $synopsis = (& go run ./internal/gensynopsis -dir $moduleCacheDir | Out-String).Trim(); if ($LASTEXITCODE -ne 0) { Refuse "gensynopsis could not read $moduleCacheDir ($LASTEXITCODE)" } }
    finally { Pop-Location }
}
# The conversion repository's tag the README links: the RELEASE version's, a rehearsal included (its own version is
# never tagged).
$repositoryTag = "nuget-$($ver.Version)"
$readmeArgs = @{
    Id = $id.Id; Description = $description; ModulePath = $ModulePath; GoVersion = $GoVersion; PackageVersion = $packageVersion
    ClosureVersion = $ClosureVersion; RepositoryUrl = $RepositoryUrl; RepositoryTag = $repositoryTag; LicenseName = $licenseName; LicenseSpdx = $LicenseSpdx
    Synopsis = $synopsis; DefaultIcon = $packageIcon.Default
}
if ($UnvalidatedReason) { $readmeArgs.Unvalidated = $true } else { $readmeArgs.Matched = $totals.Matched; $readmeArgs.Disclosed = $totals.Disclosed }
$readme = New-NugetgoPackageReadme @readmeArgs
$readmeReasons = @(Test-NugetgoReadme $readme)
if ($readmeReasons.Count) { Refuse "the generated README is not a nuget.org README -- $($readmeReasons -join '; ')" }
Write-Host "  readme: generated ($(@($readme -split "`n").Count) lines; links the conversion tag $repositoryTag; synopsis: $(if ($synopsis) { $synopsis } else { 'none' }))"
Write-Host "  icon: $($packageIcon.Name) $(if ($packageIcon.Default) { '(the go2cs icon the conversion carries)' } else { "(-Icon $($packageIcon.Path))" })"
foreach ($page in $pages) {
    $rel = $page.FullName.Substring($ValidationDir.TrimEnd('\', '/').Length + 1).Replace('\', '/')
    if ($rel -ieq 'README.md' -or $rel -ieq $packageIcon.Name) { Refuse "the validation page $rel would be packed over the package's $rel" }
}

# ---- the generated module pack project --------------------------------------------------------------------------------
$packDir = Join-Path (Join-Path $Scratch 'pack') $id.Id
if (Test-Path $packDir) { Remove-Item -Recurse -Force $packDir }
New-Item -ItemType Directory -Force $packDir | Out-Null
Copy-Item -LiteralPath $LicenseFile (Join-Path $packDir $licenseName)
Copy-Item -LiteralPath $packageIcon.Path (Join-Path $packDir $packageIcon.Name)
[System.IO.File]::WriteAllText((Join-Path $packDir 'README.md'), $readme, (New-Object System.Text.UTF8Encoding($false)))
# VALIDATION.md = the ruled two sentences as its header, then the proof page itself, unchanged below them.
[System.IO.File]::WriteAllText((Join-Path $packDir 'VALIDATION.md'),
    ("> $description`n`n" + [System.IO.File]::ReadAllText($validationPage)), (New-Object System.Text.UTF8Encoding($false)))
$esc = { param($s) [System.Security.SecurityElement]::Escape($s) }

# S2: go2cs/source-metadata.txt, written by the converter's own Go package so its consumer is known to read it.
$converterDir = Join-Path (Split-Path (Split-Path $PSScriptRoot)) 'go2cs'
$selfDescription = Join-Path (Join-Path $packDir 'go2cs') 'source-metadata.txt'
New-Item -ItemType Directory -Force (Split-Path $selfDescription) | Out-Null
$genArgs = @('run', './internal/gensourcemeta', '-src', (Join-Path $RecurseRoot 'src'), '-module', $ModulePath, '-module-version', $GoVersion,
    '-go2cs-release', $ClosureVersion, '-out', $selfDescription)
foreach ($p in $packed) { $genArgs += @('-package', "$($p.ImportPath)=$($p.Assembly)") }
foreach ($r in $requires) { $genArgs += @('-require', "$($r.Module)@$($r.Version)=$($r.NuGetId)") }
Push-Location $converterDir
try { & go @genArgs; if ($LASTEXITCODE -ne 0) { Refuse "gensourcemeta could not write the self-description ($LASTEXITCODE)" } }
finally { Pop-Location }
$projRefs = ($libraries | ForEach-Object { "    <ProjectReference Include=`"$(& $esc $_.FullName)`" PrivateAssets=`"all`" />" }) -join "`n"
$pkgRefs = ((@($goRefs | ForEach-Object { "    <PackageReference Include=`"$_`" Version=`"`$(GoStdLibVersion)`" />" }) +
    @($requires | ForEach-Object { "    <PackageReference Include=`"$(& $esc $_.NuGetId)`" Version=`"$(& $esc $_.PackageVersion)`" />" })) -join "`n")
$pageItems = ($pages | ForEach-Object {
    $rel = $_.FullName.Substring($ValidationDir.TrimEnd('\', '/').Length + 1)
    "    <None Include=`"$(& $esc $_.FullName)`" Pack=`"true`" PackagePath=`"$(& $esc ([System.IO.Path]::GetDirectoryName($rel)))`" />"
}) -join "`n"
$csproj = @"
<Project Sdk="Microsoft.NET.Sdk">
  <!-- Generated by nugetgo-pack.ps1: ONE nupkg for the Go module $ModulePath, one assembly per Go package (D6). -->
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <IncludeBuildOutput>false</IncludeBuildOutput>
    <EnableDefaultItems>false</EnableDefaultItems>
    <GenerateDocumentationFile>false</GenerateDocumentationFile>
    <TargetsForTfmSpecificContentInPackage>`$(TargetsForTfmSpecificContentInPackage);NugetgoModuleAssemblies</TargetsForTfmSpecificContentInPackage>
    <PackageId>$(& $esc $id.Id)</PackageId>
    <Version>$(& $esc $packageVersion)</Version>
    <Authors>$(ConvertTo-NugetgoMSBuildLiteral $Authors)</Authors>
    <Description>$(ConvertTo-NugetgoMSBuildLiteral $description)</Description>
    <PackageReleaseNotes>$(ConvertTo-NugetgoMSBuildLiteral $description)</PackageReleaseNotes>
    <Copyright>$(ConvertTo-NugetgoMSBuildLiteral $copyright)</Copyright>
    <PackageLicenseFile>$(& $esc $licenseName)</PackageLicenseFile>
    <PackageReadmeFile>README.md</PackageReadmeFile>
    <PackageIcon>$(& $esc $packageIcon.Name)</PackageIcon>
    <RepositoryUrl>$(ConvertTo-NugetgoMSBuildLiteral $RepositoryUrl)</RepositoryUrl>
    <RepositoryType>git</RepositoryType>
    <PackageProjectUrl>$(ConvertTo-NugetgoMSBuildLiteral $RepositoryUrl)</PackageProjectUrl>
    <PackageTags>go2cs;golang;go;PROOF</PackageTags>
  </PropertyGroup>
  <ItemGroup>
$projRefs
  </ItemGroup>
  <ItemGroup>
$pkgRefs
  </ItemGroup>
  <ItemGroup>
    <None Include="$(& $esc $licenseName)" Pack="true" PackagePath="" />
    <None Include="README.md" Pack="true" PackagePath="" />
    <None Include="$(& $esc $packageIcon.Name)" Pack="true" PackagePath="" />
    <None Include="VALIDATION.md" Pack="true" PackagePath="" />
    <None Include="go2cs/source-metadata.txt" Pack="true" PackagePath="go2cs" />
$pageItems
  </ItemGroup>
  <!-- Each package project's own assembly, as its project reference resolved it, under lib/<tfm>/. -->
  <Target Name="NugetgoModuleAssemblies" DependsOnTargets="ResolveProjectReferences">
    <ItemGroup>
      <TfmSpecificPackageFile Include="@(_ResolvedProjectReferencePaths)" PackagePath="lib/`$(TargetFramework)" />
    </ItemGroup>
  </Target>
</Project>
"@
$packProject = Join-Path $packDir "$($id.Id).csproj"
[System.IO.File]::WriteAllText($packProject, $csproj, (New-Object System.Text.UTF8Encoding($false)))
$nugetConfig = Join-Path $packDir 'nuget.config'
[System.IO.File]::WriteAllText($nugetConfig, @"
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <clear />
    <add key="nugetgo-closure" value="$(& $esc $Feed)" />
  </packageSources>
</configuration>
"@, (New-Object System.Text.UTF8Encoding($false)))

# ---- pack, isolated, with the global packages folder gated -------------------------------------------------------------
function Get-GpfPairs { if (-not (Test-Path $Gpf)) { return @() }; Get-ChildItem -Directory $Gpf | Where-Object { $_.Name -like 'go.*' -or $_.Name -like 'nugetgo.*' } | ForEach-Object { $n = $_.Name; Get-ChildItem -Directory $_.FullName | ForEach-Object { "$n/$($_.Name)" } } | Sort-Object }
$gpfBefore = @(Get-GpfPairs)
$isolated = Join-Path $Scratch 'nuget-packages'
New-Item -ItemType Directory -Force $isolated, $OutDir | Out-Null
$savedNp = $env:NUGET_PACKAGES
$savedStd = $env:GoStdLibVersion
$savedCi = $env:ContinuousIntegrationBuild
$savedPathMap = $env:PathMap
$env:NUGET_PACKAGES = $isolated
# No host path in a packed assembly (the nugetgo rehearsal's gap 3): a deterministic CI build, with the recurse root
# and the scratch mapped, so the pdb path an assembly's debug directory names and every source path in its pdb read
# /_/... . Through the environment, as the range below, so a path holding ',' or ';' is not split as a -p: global.
$env:ContinuousIntegrationBuild = 'true'
$env:PathMap = "$([System.IO.Path]::GetFullPath($RecurseRoot).TrimEnd('\', '/'))=/_/,$([System.IO.Path]::GetFullPath($Scratch).TrimEnd('\', '/'))=/_scratch/"
# The range reaches every project through the ENVIRONMENT, which MSBuild reads as properties: a -p: global would
# split it at its comma. The recurse root's generated props defaults GoStdLibVersion only when it is empty.
$env:GoStdLibVersion = $stdlibRange
try {
    Write-Host "==> $($id.Id) $packageVersion  ($($libraries.Count) package assemblies; stdlib range $stdlibRange)"
    # Restore first, and pack only against a PUBLISHED release (COORD, 2026-10-09): every go.* package restored is exactly
    # -ClosureVersion, and nuget.org lists it. A package built against anything else names a go.lib it was not compiled
    # against, or one nobody can restore.
    & dotnet restore $packProject --configfile $nugetConfig --nologo -v m
    if ($LASTEXITCODE -ne 0) { throw "dotnet restore failed ($LASTEXITCODE)" }
    $closureReasons = @(Test-NugetgoPublishedClosure -Restored (Get-NugetgoRestoredClosure -PackagesFolder $isolated) -ClosureVersion $ClosureVersion `
        -PublishedVersions { param($packageId) Get-NugetgoPublishedVersions -Id $packageId })
    if ($closureReasons.Count) { Refuse "the pack's go.* closure is not the published release ${ClosureVersion}: $($closureReasons -join '; ')" }
    Write-Host "    closure: every go.* package restored is the published $ClosureVersion"
    & dotnet pack $packProject -c Release -o $OutDir --no-restore --nologo -v m
    if ($LASTEXITCODE -ne 0) { throw "dotnet pack failed ($LASTEXITCODE)" }
}
finally { $env:NUGET_PACKAGES = $savedNp; $env:GoStdLibVersion = $savedStd; $env:ContinuousIntegrationBuild = $savedCi; $env:PathMap = $savedPathMap }
$gpfAfter = @(Get-GpfPairs)
$grown = @($gpfAfter | Where-Object { $gpfBefore -notcontains $_ })
if ($grown.Count) { throw "the global packages folder GREW: $($grown -join ', ')" }

# ---- read the package back ---------------------------------------------------------------------------------------------
$nupkg = Join-Path $OutDir "$($id.Id).$packageVersion.nupkg"
if (-not (Test-Path $nupkg)) { $nupkg = @(Get-ChildItem $OutDir -Filter "$($id.Id).*.nupkg" | Sort-Object LastWriteTime -Descending)[0].FullName }
# The roots no packed byte may name: the build root, the pack's scratch and output, the module cache, the user profile.
$hostRoots = @($RecurseRoot, $Scratch, $OutDir, (& go env GOMODCACHE).Trim(), [Environment]::GetFolderPath('UserProfile')) |
    Where-Object { $_ } | ForEach-Object { [System.IO.Path]::GetFullPath($_) } | Select-Object -Unique
$hostPathHits = New-Object System.Collections.Generic.List[string]
$zip = [System.IO.Compression.ZipFile]::OpenRead($nupkg)
try {
    $entries = @($zip.Entries | ForEach-Object { $_.FullName })
    foreach ($entry in $zip.Entries) {
        $stream = $entry.Open(); $buffer = New-Object System.IO.MemoryStream
        try { $stream.CopyTo($buffer) } finally { $stream.Dispose() }
        foreach ($root in @(Find-NugetgoHostPaths -Bytes $buffer.ToArray() -Roots $hostRoots)) { $hostPathHits.Add("$($entry.FullName) names $root") }
    }
    $nuspecEntry = $zip.Entries | Where-Object { $_.FullName -like '*.nuspec' } | Select-Object -First 1
    $reader = New-Object System.IO.StreamReader($nuspecEntry.Open())
    [xml]$nuspec = $reader.ReadToEnd(); $reader.Dispose()
    $readmeEntry = $zip.Entries | Where-Object { $_.FullName -eq 'README.md' } | Select-Object -First 1
    $packedReadme = $null
    if ($readmeEntry) { $reader = New-Object System.IO.StreamReader($readmeEntry.Open()); $packedReadme = $reader.ReadToEnd(); $reader.Dispose() }
    $iconEntry = $zip.Entries | Where-Object { $_.FullName -ceq $packageIcon.Name } | Select-Object -First 1
    $packedIcon = $null
    if ($iconEntry) {
        $stream = $iconEntry.Open(); $buffer = New-Object System.IO.MemoryStream
        try { $stream.CopyTo($buffer) } finally { $stream.Dispose() }
        $packedIcon = $buffer.ToArray()
    }
    $validationEntry = $zip.Entries | Where-Object { $_.FullName -eq 'VALIDATION.md' } | Select-Object -First 1
    $reader = New-Object System.IO.StreamReader($validationEntry.Open())
    $validationHead = $reader.ReadToEnd(); $reader.Dispose()
    $selfEntry = $zip.Entries | Where-Object { $_.FullName -eq 'go2cs/source-metadata.txt' } | Select-Object -First 1
    $packedSelfDescription = Join-Path $Scratch 'read-back-source-metadata.txt'
    if ($selfEntry) { [System.IO.Compression.ZipFileExtensions]::ExtractToFile($selfEntry, $packedSelfDescription, $true) }
    $libDir = Join-Path $Scratch 'read-back-lib'
    if (Test-Path $libDir) { Remove-Item -Recurse -Force $libDir }
    New-Item -ItemType Directory -Force $libDir | Out-Null
    $libFiles = @($zip.Entries | Where-Object { $_.FullName -like 'lib/*.dll' } | ForEach-Object {
        $out = Join-Path $libDir $_.Name; [System.IO.Compression.ZipFileExtensions]::ExtractToFile($_, $out, $true); $out })
}
finally { $zip.Dispose() }
$md = $nuspec.package.metadata
$deps = @($md.dependencies.group | ForEach-Object { $_.dependency } | Where-Object { $_ })
$badDeps = @($deps | Where-Object { $_.id -like 'go.*' -and -not ([string]$_.version).StartsWith("[$ClosureVersion, ", [StringComparison]::Ordinal) })
Write-Host "==> read back: $($md.id) $($md.version)"
Write-Host "    lib: $((@($entries | Where-Object { $_ -like 'lib/*' })) -join ', ')"
Write-Host "    dependencies: $($deps.Count), each go.* at $(@($deps | ForEach-Object { $_.version } | Sort-Object -Unique) -join ' | ')"
Write-Host "    license file: $($md.license.'#text') ($($md.license.type)); readme: $($md.readme); repository: $($md.repository.url)"
Write-Host "    copyright: $($md.copyright)"
Write-Host "    description: $($md.description)"
Write-Host "    pages: $((@($entries | Where-Object { $_ -like '*.md' })) -join ', ')"
Write-Host "    icon: $($md.icon)"
if ($md.id -ne $id.Id -or $md.version -ne $packageVersion) { throw "read-back identity $($md.id) $($md.version) is not $($id.Id) $packageVersion" }
if ($md.description -cne $description -or $md.releaseNotes -cne $description) { throw 'the read-back description or release notes are not the ruled text' }
if ($md.copyright -cne $copyright) { throw "the read-back copyright '$($md.copyright)' is not the upstream license file's Copyright lines '$copyright'" }
# Every free-text value reaches the nuspec intact: the generated project MSBuild-escapes each, so a '$(...)', '@(...)' or
# '%XX' inside one is text, never an expansion or an escape MSBuild would decode.
if ($md.authors -cne $Authors) { throw "the read-back authors '$($md.authors)' are not -Authors '$Authors'" }
# NuGet writes projectUrl through System.Uri, which decodes an escape such as '%20' as it renders: that field is
# compared as a URI, the repository url (written as given) as text.
if ($md.repository.url -cne $RepositoryUrl -or ([Uri]$md.projectUrl).AbsoluteUri -cne ([Uri]$RepositoryUrl).AbsoluteUri) { throw "the read-back repository url '$($md.repository.url)' or project url '$($md.projectUrl)' is not -RepositoryUrl '$RepositoryUrl'" }
if (-not $validationHead.StartsWith("> $description", [StringComparison]::Ordinal)) { throw 'the packed VALIDATION.md does not open with the ruled text' }
# The README and the icon, as nuget.org will show them (owner, 2026-10-10). A refused package is removed, as a host
# path's is, so no push can pick it up.
$presentation = New-Object System.Collections.Generic.List[string]
if ($md.readme -cne 'README.md') { $presentation.Add("the package README is '$($md.readme)', not the generated README.md") }
if ($null -eq $packedReadme) { $presentation.Add('the package carries no README.md') }
elseif ($packedReadme -cne $readme) { $presentation.Add('the packed README.md differs from the one generated') }
else { foreach ($why in @(Test-NugetgoReadme $packedReadme)) { $presentation.Add("README.md $why") } }
if (-not $md.icon) { $presentation.Add('the package has no icon') }
elseif ($md.icon -cne $packageIcon.Name -or $null -eq $packedIcon -or -not [System.Linq.Enumerable]::SequenceEqual($packedIcon, [System.IO.File]::ReadAllBytes($packageIcon.Path))) { $presentation.Add("the package icon '$($md.icon)' is not $($packageIcon.Path), byte for byte") }
if (@($entries | Where-Object { $_ -ceq 'VALIDATION.md' }).Count -ne 1) { $presentation.Add('VALIDATION.md is not packed as a file') }
Write-Host "    readme and icon: $(if ($presentation.Count) { "$($presentation.Count) refusal(s)" } else { 'README.md generated and clean; icon packed' })"
if ($presentation.Count) {
    Remove-Item -LiteralPath $nupkg
    throw "REFUSED: the package's nuget.org page -- $($presentation -join '; ')"
}
if ($badDeps.Count) { throw "go.* dependencies not at the B4 range: $(($badDeps | ForEach-Object { "$($_.id) $($_.version)" }) -join ', ')" }
if (@($entries | Where-Object { $_ -like 'lib/*.dll' }).Count -ne $libraries.Count) { throw "lib/ carries $(@($entries | Where-Object { $_ -like 'lib/*.dll' }).Count) assemblies for $($libraries.Count) packages" }
# Each assembly's copyright and company, read from the dll taken back OUT of the nupkg: its version resource, the
# values a file's properties show.
foreach ($dll in $libFiles) {
    $info = [System.Diagnostics.FileVersionInfo]::GetVersionInfo($dll)
    Write-Host "    assembly $(Split-Path -Leaf $dll): copyright '$($info.LegalCopyright)', company '$($info.CompanyName)'"
    if ($assemblyCopyright.Skip) {
        if (-not ([string]$info.LegalCopyright).Contains('The go2cs Authors')) { throw "$(Split-Path -Leaf $dll) does not keep the csproj template's copyright: '$($info.LegalCopyright)'" }
    }
    elseif ($info.LegalCopyright -cne $assemblyCopyright.Copyright -or $info.CompanyName -cne $Authors) {
        throw "$(Split-Path -Leaf $dll) carries copyright '$($info.LegalCopyright)' and company '$($info.CompanyName)', not '$($assemblyCopyright.Copyright)' and '$Authors'"
    }
}
# No host path: every packed entry (scanned above), and the pdb each packed assembly was built with, which a symbols
# package would carry.
foreach ($dll in $libFiles) {
    $pdbName = [System.IO.Path]::ChangeExtension((Split-Path -Leaf $dll), '.pdb')
    $pdb = @(Get-ChildItem -LiteralPath $RecurseRoot -Recurse -Force -Filter $pdbName -File | Sort-Object LastWriteTime -Descending)[0]
    if (-not $pdb) { throw "no $pdbName under $RecurseRoot for the packed $(Split-Path -Leaf $dll)" }
    foreach ($root in @(Find-NugetgoHostPaths -Bytes ([System.IO.File]::ReadAllBytes($pdb.FullName)) -Roots $hostRoots)) { $hostPathHits.Add("$pdbName names $root") }
}
Write-Host "    host paths: $($hostPathHits.Count) in $($entries.Count) packed entries and $($libFiles.Count) pdb(s)"
if ($hostPathHits.Count) {
    Remove-Item -LiteralPath $nupkg   # a refused package is never left where a push could pick it up
    throw "REFUSED: the package would publish a host path -- $($hostPathHits -join '; ')"
}
# S2: the self-description is in the package, byte for byte what was written, and the CONVERTER'S OWN parser reads it.
if (-not $selfEntry) { throw 'the package carries no go2cs/source-metadata.txt' }
if ([System.IO.File]::ReadAllText($packedSelfDescription) -cne [System.IO.File]::ReadAllText($selfDescription)) { throw 'the packed go2cs/source-metadata.txt differs from the one written' }
Push-Location $converterDir
try { $verified = (& go run ./internal/gensourcemeta -verify $packedSelfDescription 2>&1 | Out-String).Trim(); $verifyCode = $LASTEXITCODE }
finally { Pop-Location }
$expectedSummary = "$ModulePath $GoVersion (go2cs-release $ClosureVersion): $($libraries.Count) package(s), $($requires.Count) require(s)"
Write-Host "    self-description: $verified"
if ($verifyCode -ne 0 -or -not $verified.StartsWith($expectedSummary, [StringComparison]::Ordinal)) { throw "the packed self-description does not read back as '$expectedSummary': $verified" }
foreach ($r in $requires) {
    if (@($deps | Where-Object { $_.id -ceq $r.NuGetId -and ([string]$_.version).StartsWith($r.PackageVersion, [StringComparison]::Ordinal) }).Count -ne 1) {
        throw "the third-party dependency $($r.NuGetId) $($r.PackageVersion) (for $($r.Module)) is not in the nuspec"
    }
}
Write-Host "==> packed $nupkg (global packages folder unchanged: $($gpfAfter.Count) go.*/nugetgo.* pairs)"
