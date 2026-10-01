<#
.SYNOPSIS
    Pack one converted Go module as ONE nugetgo.* package (docs/PLAN-nugetgo.md section 8, B2-B6; the multi-package
    module design's D6/D7).

.DESCRIPTION
    Input is a `go2cs -recurse=nuget` output root. Every main-module LIBRARY package under <RecurseRoot>/src/<module>
    (one assembly per Go package; a main package is excluded) is built through its own project, whose go.* references
    are PackageReferences at $(GoStdLibVersion). A generated module pack project then carries each package's assembly
    under lib/<tfm>/, and declares the UNION of the packages' public go.* PackageReferences at the same
    $(GoStdLibVersion). The pack passes GoStdLibVersion as B4's range, [<ClosureVersion>, <next Go minor>), so NuGet
    writes the range from the PackageReference route itself, with no rewrite of any edge. The lower bound is the
    closure version packed against: a PARAMETER, never a literal. NuGet NORMALIZES the range as it writes the nuspec
    (the upper bound 1.25 reads 1.25.0): the same range, ACCEPTED as such (COORD, 2026-09-30: B4 rules the range, not
    its bytes).

    Identity (B2, B3) comes from NugetgoIdentity.psm1. A rehearsal carries -RehearsalSuffix (e.g. local.1), so a
    rehearsal can never mint a real release version: NuGet caches by id+version immutably. A version with no suffix is
    packed only under -Release.

    Metadata (B6): upstream's Copyright lines and LICENSE verbatim, the PROOF description, RepositoryUrl = the
    per-module conversion-source repo. D7: the module's MODULE.md ships as VALIDATION.md, with every per-package
    proof page beside it.

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
    [string]$LicenseFile,
    [string]$RehearsalSuffix,
    [switch]$Release,
    [string[]]$ExistingIds = @(),
    [string[]]$VList,
    [string]$Authors = 'go2cs conversion',
    [string]$Gpf = $(if ($env:NUGET_PACKAGES) { $env:NUGET_PACKAGES } else { Join-Path $env:USERPROFILE '.nuget\packages' })
)

$ErrorActionPreference = 'Stop'
Import-Module (Join-Path $PSScriptRoot 'NugetgoIdentity.psm1') -Force
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
$moduleSrc = Join-Path $RecurseRoot ('src\' + $ModulePath.Replace('/', '\'))
if (-not (Test-Path $moduleSrc)) { Refuse "no converted module at $moduleSrc (a -recurse=nuget output root is expected)" }
$projects = @(Get-ChildItem -Path $moduleSrc -Recurse -Filter '*.csproj' | Where-Object { $_.Name -notlike '*.tests.csproj' } | Sort-Object FullName)
$libraries = New-Object System.Collections.Generic.List[object]
$goRefs = New-Object 'System.Collections.Generic.SortedSet[string]' ([StringComparer]::OrdinalIgnoreCase)
foreach ($p in $projects) {
    [xml]$x = Get-Content -Raw -LiteralPath $p.FullName
    $outputType = @($x.Project.PropertyGroup | ForEach-Object { $_.OutputType } | Where-Object { $_ })[0]
    if ($outputType -ne 'Library') { Write-Host "  excluded (OutputType $outputType, a Go main): $($p.FullName.Substring($moduleSrc.Length))"; continue }
    $libraries.Add($p)
    foreach ($ref in @($x.Project.ItemGroup | ForEach-Object { $_.PackageReference } | Where-Object { $_ })) {
        if ($ref.Include -like 'go.*' -and $ref.Version -eq '$(GoStdLibVersion)' -and $ref.PrivateAssets -ne 'all') { [void]$goRefs.Add($ref.Include) }
    }
}
if ($libraries.Count -eq 0) { Refuse "no library package under $moduleSrc" }

# ---- B6 metadata -----------------------------------------------------------------------------------------------------
if (-not $LicenseFile) { $LicenseFile = Join-Path (& go env GOMODCACHE).Trim() ("$ModulePath@$GoVersion" -replace '/', '\' ) | Join-Path -ChildPath 'LICENSE' }
if (-not (Test-Path -LiteralPath $LicenseFile)) { Refuse "no upstream LICENSE at $LicenseFile" }
$copyright = (@(Get-Content -LiteralPath $LicenseFile | Where-Object { $_ -cmatch '^\s*Copyright\b' } | ForEach-Object { $_.Trim() }) -join '; ')
if (-not $copyright) { Refuse "the upstream LICENSE carries no Copyright line: $LicenseFile" }
# The description, exactly as ruled (COORD, within B6, 2026-09-30). The Go release is the closure's own (its first three
# components), never a literal, so the text cannot outlive the corpus it describes. The same two sentences head
# VALIDATION.md and are the release notes.
$goRelease = ($ClosureVersion -split '[.-]')[0..2] -join '.'
$description = "PROOF: unofficial go2cs C# conversion of $ModulePath $GoVersion, built on the Go $goRelease standard library, " +
    "not affiliated with or endorsed by $Upstream or the Go project. " +
    "Security: that standard library carries no Go security fixes issued after Go $goRelease; review before any production use."

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
}

# ---- the generated module pack project --------------------------------------------------------------------------------
$packDir = Join-Path $Scratch "pack\$($id.Id)"
if (Test-Path $packDir) { Remove-Item -Recurse -Force $packDir }
New-Item -ItemType Directory -Force $packDir | Out-Null
Copy-Item -LiteralPath $LicenseFile (Join-Path $packDir 'LICENSE')
# VALIDATION.md = the ruled two sentences as its header, then the proof page itself, unchanged below them.
[System.IO.File]::WriteAllText((Join-Path $packDir 'VALIDATION.md'),
    ("> $description`n`n" + [System.IO.File]::ReadAllText($validationPage)), (New-Object System.Text.UTF8Encoding($false)))
$esc = { param($s) [System.Security.SecurityElement]::Escape($s) }
$projRefs = ($libraries | ForEach-Object { "    <ProjectReference Include=`"$(& $esc $_.FullName)`" PrivateAssets=`"all`" />" }) -join "`n"
$pkgRefs = ($goRefs | ForEach-Object { "    <PackageReference Include=`"$_`" Version=`"`$(GoStdLibVersion)`" />" }) -join "`n"
$pageItems = ($pages | ForEach-Object {
    $rel = $_.FullName.Substring($ValidationDir.TrimEnd('\').Length + 1)
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
    <Authors>$(& $esc $Authors)</Authors>
    <Description>$(& $esc $description)</Description>
    <PackageReleaseNotes>$(& $esc $description)</PackageReleaseNotes>
    <Copyright>$(& $esc $copyright)</Copyright>
    <PackageLicenseFile>LICENSE</PackageLicenseFile>
    <PackageReadmeFile>VALIDATION.md</PackageReadmeFile>
    <RepositoryUrl>$(& $esc $RepositoryUrl)</RepositoryUrl>
    <RepositoryType>git</RepositoryType>
    <PackageProjectUrl>$(& $esc $RepositoryUrl)</PackageProjectUrl>
    <PackageTags>go2cs;golang;go;PROOF</PackageTags>
  </PropertyGroup>
  <ItemGroup>
$projRefs
  </ItemGroup>
  <ItemGroup>
$pkgRefs
  </ItemGroup>
  <ItemGroup>
    <None Include="LICENSE" Pack="true" PackagePath="" />
    <None Include="VALIDATION.md" Pack="true" PackagePath="" />
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
$env:NUGET_PACKAGES = $isolated
# The range reaches every project through the ENVIRONMENT, which MSBuild reads as properties: a -p: global would
# split it at its comma. The recurse root's generated props defaults GoStdLibVersion only when it is empty.
$env:GoStdLibVersion = $stdlibRange
try {
    Write-Host "==> $($id.Id) $packageVersion  ($($libraries.Count) package assemblies; stdlib range $stdlibRange)"
    & dotnet pack $packProject -c Release -o $OutDir --configfile $nugetConfig --nologo -v m
    if ($LASTEXITCODE -ne 0) { throw "dotnet pack failed ($LASTEXITCODE)" }
}
finally { $env:NUGET_PACKAGES = $savedNp; $env:GoStdLibVersion = $savedStd }
$gpfAfter = @(Get-GpfPairs)
$grown = @($gpfAfter | Where-Object { $gpfBefore -notcontains $_ })
if ($grown.Count) { throw "the global packages folder GREW: $($grown -join ', ')" }

# ---- read the package back ---------------------------------------------------------------------------------------------
$nupkg = Join-Path $OutDir "$($id.Id).$packageVersion.nupkg"
if (-not (Test-Path $nupkg)) { $nupkg = @(Get-ChildItem $OutDir -Filter "$($id.Id).*.nupkg" | Sort-Object LastWriteTime -Descending)[0].FullName }
$zip = [System.IO.Compression.ZipFile]::OpenRead($nupkg)
try {
    $entries = @($zip.Entries | ForEach-Object { $_.FullName })
    $nuspecEntry = $zip.Entries | Where-Object { $_.FullName -like '*.nuspec' } | Select-Object -First 1
    $reader = New-Object System.IO.StreamReader($nuspecEntry.Open())
    [xml]$nuspec = $reader.ReadToEnd(); $reader.Dispose()
    $validationEntry = $zip.Entries | Where-Object { $_.FullName -eq 'VALIDATION.md' } | Select-Object -First 1
    $reader = New-Object System.IO.StreamReader($validationEntry.Open())
    $validationHead = $reader.ReadToEnd(); $reader.Dispose()
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
if ($md.id -ne $id.Id -or $md.version -ne $packageVersion) { throw "read-back identity $($md.id) $($md.version) is not $($id.Id) $packageVersion" }
if ($md.description -cne $description -or $md.releaseNotes -cne $description) { throw 'the read-back description or release notes are not the ruled text' }
if (-not $validationHead.StartsWith("> $description", [StringComparison]::Ordinal)) { throw 'the packed VALIDATION.md does not open with the ruled text' }
if ($badDeps.Count) { throw "go.* dependencies not at the B4 range: $(($badDeps | ForEach-Object { "$($_.id) $($_.version)" }) -join ', ')" }
if (@($entries | Where-Object { $_ -like 'lib/*.dll' }).Count -ne $libraries.Count) { throw "lib/ carries $(@($entries | Where-Object { $_ -like 'lib/*.dll' }).Count) assemblies for $($libraries.Count) packages" }
Write-Host "==> packed $nupkg (global packages folder unchanged: $($gpfAfter.Count) go.*/nugetgo.* pairs)"
