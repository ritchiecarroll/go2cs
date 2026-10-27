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
    the ruled description (no "PROOF:" prefix: owner review 2026-10-10), RepositoryUrl = the per-module conversion-source repo. D7: the module's MODULE.md ships as
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
    generated in the go.* standard library packages' style (NugetgoReadme.psm1) -- the package ID, the description
    with its module path linked to the Go module's source at its version (owner review 2026-10-10), a badge row, the Go package's synopsis (internal/gensynopsis, read from the module's own source) and a license line
    naming -LicenseSpdx -- and VALIDATION.md is packed beside it as a file the README links. Its links into the
    conversion repository name the tag nuget-<release version> (COORD ruling 2026-10-10), which COORD mints on the
    conversion commit before the package is signed; a rehearsal names its release's tag, since a rehearsal version
    gets none. The icon is the go2cs.png
    the conversion carries, or -Icon. The read-back refuses a README with inline code in a heading or a table wider
    than three columns or a link naming HEAD, and a package with no icon.

    The author's package page (COORD ruling 2026-10-10): the README lives in the conversion, at nuget/README.md in the
    converted module's directory. The first pack SEEDS it (the title, the fenced generated block, the synopsis) and
    says to commit it; from then on it is the author's, and every pack rewrites only the block between
    [//]: # (nugetgo:generated:begin) and [//]: # (nugetgo:generated:end), says so with the version and tag it wrote,
    and packs the file byte for byte. A missing, duplicated, misordered or malformed fence is refused by name, and so is
    a link before it. nuget/icon.png beside it replaces the go2cs icon; -Icon with that file present is refused.

    The conversion repository's root README (COORD ruling 2026-10-11): -ConversionReadme names it, and the pack refuses
    one that does not fit the description's form. A third-party conversion's README must END with the offer to the
    module's maintainers (New-NugetgoMaintainersSection); an author's must not carry it. The parameter is OPTIONAL for
    now. It becomes mandatory for release packs, never rehearsal packs, when the next sheet is cut.

    Flavors (COORD ruling 2026-10-12): the pack adopts the go.* release's per-OS promotion (push-nuget.ps1). A -recurse
    conversion emits one GOOS, so the pack converts the module's own source once per GOOS of the release's RID map
    (windows, linux, darwin) with this tree's converter, requires its conversion of -RecurseRoot's GOOS to reproduce every
    packed project's GoInputDigest, builds every flavor, and compares the flavors' assemblies with the release's material
    check (GoAssemblyMaterial.psm1). Equal in meaning: one neutral assembly set in lib/<tfm>/, packed from -RecurseRoot as
    before. Different: the release's RID-specific layout (NugetgoFlavors.psm1), lib/<tfm>/ the windows flavor and
    runtimes/<rid>/lib/<tfm>/ each RID's, with every flavor's go.* dependencies. Third-party requires are the union of
    every flavor's, in the nuspec and the self-description, and a RID-specific package's VALIDATION.md names, in one
    line, the flavors whose package metadata the self-description does not describe. The decision is a measurement,
    never a list and never the module author's word.

    Behavioral proof (COORD ruling 2026-10-12): a module with no Go tests has no comparison for `go2cs -recurse -tests`
    to make, so it ships -BehaviorProofDir instead of -ValidationDir: one row per shipped RID (<rid>.md), each written
    on a host of that RID by Invoke-NugetgoBehaviorRow.ps1, usually the os-matrix workflow's nugetgo-behavior stage
    (NugetgoBehaviorProof.psm1). VALIDATION.md is then the behavioral page, which says it is not a test comparison, and
    the Tests badge says there are no Go tests and how many RIDs ran. After the pack, each row's assembly must be the
    bytes the package carries for its RID, and each row must have been recorded on its file's RID and GOOS; anything
    else is refused by name and the package removed. A module with Go tests never takes this proof in place of its test
    comparison; it may carry the same rows BESIDE it (-BehaviorProofDir with -ValidationDir, COORD ruling 2026-10-12),
    as an additional row set: VALIDATION.md gains a "Sample behavior" section under the comparison, the README a
    Sample behavior badge after the Tests badge, and the rows are bound to the packed bytes by the same check.

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
    # A module with no Go tests: the directory of its behavioral proof rows, <rid>.md for every shipped RID.
    [string]$BehaviorProofDir,
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
    # The holder line for a license that carries no Copyright line (cobra's Apache LICENSE.txt): one of the module's own
    # source-header lines, refused when the license carries one (Resolve-NugetgoCopyright).
    [string]$Copyright,
    # The package icon, PNG or JPEG up to 1 MB; the go2cs icon the conversion carries when not given. A module author
    # publishing their own module sets their own.
    [string]$Icon,
    # The conversion repository's root README.md, checked against the description's form (Test-NugetgoConversionReadme).
    [string]$ConversionReadme,
    # The go2cs that converted -RecurseRoot (the runbook's G). Each flavor is converted with it, so the flavors share its
    # GoInputDigest revision; when it is not given, the pack builds one from this tree.
    [string]$Converter,
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
Import-Module (Join-Path $PSScriptRoot 'NugetgoFlavors.psm1') -Force
Import-Module (Join-Path $PSScriptRoot 'NugetgoBehaviorProof.psm1') -Force
# The list parameters arrive comma-joined from the rebuild driver (Split-NugetgoListArgument).
$ThirdPartyPackage = @(Split-NugetgoListArgument $ThirdPartyPackage)
$ExcludePackage = @(Split-NugetgoListArgument $ExcludePackage)
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
# The module's own source in the module cache, where the test-only packages, the license file and the package synopsis
# are read. The cache spells each uppercase letter of the path as '!' and its lowercase (golang.org/x/mod/module.EscapePath).
$moduleCacheDir = Join-Path (& go env GOMODCACHE).Trim() ([regex]::Replace("$ModulePath@$GoVersion", '[A-Z]', { '!' + $args[0].Value.ToLowerInvariant() }))
if (-not (Test-Path -LiteralPath $moduleCacheDir -PathType Container)) { Refuse "no module source at $moduleCacheDir, where the pack reads which packages are test-only (go mod download $ModulePath@$GoVersion)" }
# A test-only internal package -- imported only from _test.go files, or by nothing -- is left out on every platform the
# go.* release ships; one with Go tests of its own is packed, its proof row kept (COORD ruling 2026-10-13).
$testOnly = Get-NugetgoTestOnlyPackages -ModuleDir $moduleCacheDir -ModulePath $ModulePath -Platforms (Get-NugetgoFlavorPlatforms)
foreach ($path in @($testOnly.Kept)) { Write-Host "  kept: validated (test-only, with Go tests of its own): $path" }
# -ExcludePackage leaves a library out (Select-NugetgoPackedLibraries refuses an exclusion a packed package references).
$selected = @(Select-NugetgoPackedLibraries -RecurseRoot $RecurseRoot -Libraries $candidates.ToArray() -ExcludePackage $ExcludePackage -TestOnlyPackage @($testOnly.Excluded))
$candidatePaths = @(Get-NugetgoPackedPackages -RecurseRoot $RecurseRoot -Libraries $candidates.ToArray() | ForEach-Object { $_.ImportPath })
$libraries = New-Object System.Collections.Generic.List[object]
$goRefs = New-Object 'System.Collections.Generic.SortedSet[string]' ([StringComparer]::OrdinalIgnoreCase)
for ($ci = 0; $ci -lt $candidates.Count; $ci++) {
    $p = $candidates[$ci]
    if ($selected -notcontains $p) {
        if ($ExcludePackage -notcontains $candidatePaths[$ci] -and @($testOnly.Excluded) -contains $candidatePaths[$ci]) {
            Write-Host "  excluded (test-only: imported only from _test.go or by nothing): $($p.FullName.Substring($moduleSrc.Length))"
        }
        else { Write-Host "  excluded (-ExcludePackage): $($p.FullName.Substring($moduleSrc.Length))" }
        continue
    }
    $libraries.Add($p)
    [xml]$x = Get-Content -Raw -LiteralPath $p.FullName
    foreach ($ref in @($x.Project.ItemGroup | ForEach-Object { $_.PackageReference } | Where-Object { $_ })) {
        if ($ref.Include -like 'go.*' -and $ref.Version -eq '$(GoStdLibVersion)' -and $ref.PrivateAssets -ne 'all') { [void]$goRefs.Add($ref.Include) }
    }
}
if ($libraries.Count -eq 0) { Refuse "no library package under $moduleSrc" }

# ---- S2: what the self-description lists ------------------------------------------------------------------------------
$packed = @(Get-NugetgoPackedPackages -RecurseRoot $RecurseRoot -Libraries $libraries.ToArray())
# This flavor's requires; the package's are the union of every flavor's, taken after the flavors are converted.
$requires = @(Get-NugetgoThirdPartyRequires -RecurseRoot $RecurseRoot -Libraries $libraries.ToArray() -ThirdPartyPackage $ThirdPartyPackage -AllowUnreferenced)

# ---- B6 metadata -----------------------------------------------------------------------------------------------------
# $moduleCacheDir (above, before the selection) is where the license file and the package synopsis are read.
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
$resolvedCopyright = Resolve-NugetgoCopyright -LicenseFile $LicenseFile -Stated $Copyright -ModuleDir $moduleCacheDir
if ($resolvedCopyright.Reason) { Refuse $resolvedCopyright.Reason }
if ($resolvedCopyright.Source -ceq 'stated') { Write-Host "  copyright: stated with -Copyright (the license carries none); evidence: $($resolvedCopyright.Evidence)" }
$copyright = ($resolvedCopyright.Lines -join '; ')
# Each packed assembly's own copyright attribute names the UPSTREAM holder too (owner ruling 2026-10-04): the same
# Copyright lines, then the go2cs scaffolding line. It is set by a Directory.Build.targets in the converted module's
# own directory, so the conversion repository pushed from <RecurseRoot> rebuilds the same attribute. Escaped -p:
# globals would not: they reach every project the build touches (a third-party pkg/ project reference included),
# split at ',' and ';', and live in no file a rebuild from the pushed sources reads.
$assemblyCopyright = Get-NugetgoAssemblyCopyright -LicenseFile $LicenseFile -Lines $resolvedCopyright.Lines
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
if ($description.Contains('PROOF:')) { Refuse 'package description: it carries "PROOF:" (owner review 2026-10-10: the registry tier and the Tests badge carry the proof)' }
# The conversion repository's root README (COORD ruling 2026-10-11): a third-party conversion's ends with the offer to
# the module's maintainers, and an author's carries none.
if ($ConversionReadme) {
    if (-not (Test-Path -LiteralPath $ConversionReadme -PathType Leaf)) { Refuse "-ConversionReadme: no file at $ConversionReadme" }
    $conversionReasons = @(Test-NugetgoConversionReadme -Text ([System.IO.File]::ReadAllText($ConversionReadme)) -ModulePath $ModulePath -Author:$described.Author)
    if ($conversionReasons.Count) { Refuse "the conversion repository's README ${ConversionReadme}: $($conversionReasons -join '; ')" }
    Write-Host "conversion README: $(if ($described.Author) { 'the author form, with no maintainers section' } else { 'ends with the offer to the module''s maintainers' })"
}

# D7: a validated module ships its MODULE.md as VALIDATION.md, the per-package pages beside it. A module with no Go
# tests ships its behavioral proof instead (COORD ruling 2026-10-12). A module that cannot validate yet packs ONLY as a
# rehearsal of its shape (-UnvalidatedReason), whose VALIDATION.md says so; such a package is never a release.
$pages = @()
$proofSummary = $null
$behaviorRows = @{}
if (-not (Test-Path -LiteralPath $moduleCacheDir -PathType Container)) { Refuse "no module source at $moduleCacheDir, where the pack reads whether the module has Go tests (go mod download $ModulePath@$GoVersion)" }
$proofMode = Get-NugetgoProofMode -ValidationDir $ValidationDir -UnvalidatedReason $UnvalidatedReason -BehaviorProofDir $BehaviorProofDir -Release:$Release `
    -ModuleHasGoTests (Test-NugetgoModuleHasGoTests -ModuleDir $moduleCacheDir)
if ($proofMode.Reason) { Refuse $proofMode.Reason }
# The rows, the behavioral proof's or a tested module's additional sample rows: read now, bound after the pack against
# the bytes the package carries for each RID.
if ($BehaviorProofDir) {
    if (-not (Test-Path -LiteralPath $BehaviorProofDir -PathType Container)) { Refuse "-BehaviorProofDir: no directory at $BehaviorProofDir" }
    foreach ($f in @(Get-ChildItem -LiteralPath $BehaviorProofDir -Filter '*.md' -File)) { $behaviorRows[$f.BaseName] = Read-NugetgoBehaviorRow -Path $f.FullName }
    if ($proofMode.SampleRows) { Write-Host "  proof: sample rows beside the test comparison, $($behaviorRows.Count) row(s) from $BehaviorProofDir ($(@($behaviorRows.Keys | Sort-Object) -join ', '))" }
}
if ($proofMode.Mode -ceq 'unvalidated') {
    $validationPage = Join-Path $Scratch 'UNVALIDATED.md'
    New-Item -ItemType Directory -Force $Scratch | Out-Null
    [System.IO.File]::WriteAllText($validationPage, ("# ``$ModulePath`` -- NOT VALIDATED`n`n" +
        "This package is a pack-shape rehearsal of the go2cs conversion of ``$ModulePath`` $GoVersion. Its Go tests have " +
        "not been converted and compared, so it carries no validation proof, and it is not published.`n`n$UnvalidatedReason`n"),
        (New-Object System.Text.UTF8Encoding($false)))
}
elseif ($proofMode.Mode -ceq 'behavioral') {
    $validationPage = Join-Path $Scratch 'BEHAVIOR.md'
    New-Item -ItemType Directory -Force $Scratch | Out-Null
    [System.IO.File]::WriteAllText($validationPage, (New-NugetgoBehaviorPage -ModulePath $ModulePath -GoVersion $GoVersion -Rows $behaviorRows), (New-Object System.Text.UTF8Encoding($false)))
    Write-Host "  proof: behavioral, $($behaviorRows.Count) row(s) from $BehaviorProofDir ($(@($behaviorRows.Keys | Sort-Object) -join ', '))"
}
else {
    $validationPage = Join-Path $ValidationDir 'MODULE.md'
    if (-not (Test-Path -LiteralPath $validationPage)) { Refuse "no MODULE.md in $ValidationDir (a validated module is required; D7)" }
    # VALIDATION.md counts the packed packages only (COORD ruling 2026-10-26): a proven package -ExcludePackage leaves
    # out (cobra/doc) is named in one line, and its row and proof page are not shipped.
    $proofSelection = Select-NugetgoPackedProof -ModuleSummary ([System.IO.File]::ReadAllText($validationPage)) -Excluded $ExcludePackage
    $proofSummary = $proofSelection.Text
    $validationRoot = $ValidationDir.TrimEnd('\', '/')
    $pages = @(Get-ChildItem -LiteralPath $ValidationDir -Filter '*.md' -Recurse | Where-Object {
        $_.Name -ne 'MODULE.md' -and @($proofSelection.DroppedPages) -notcontains ($_.FullName.Substring($validationRoot.Length + 1) -replace '\\', '/') })
    foreach ($page in @($proofSelection.DroppedPages)) { Write-Host "  proof: $page left out with its -ExcludePackage'd package (VALIDATION.md names it)" }
    $totals = Get-NugetgoProofTotals -ModuleSummary $proofSummary
    if ($totals.Packages -eq 0) { Refuse "$validationPage has no package row: the README's Tests badge has no totals to state" }
    # The proof must be OF the packed bytes (the nugetgo rehearsal, 2026-10-09, gap 4): every MODULE.md row's input
    # digest is the GoInputDigest its packed project records. $packed lists the libraries in $libraries' order.
    $packedDigests = @{}
    for ($i = 0; $i -lt $libraries.Count; $i++) { $packedDigests[$packed[$i].ImportPath] = Get-NugetgoProjectInputDigest -ProjectFile $libraries[$i].FullName }
    $proofDigests = Get-NugetgoProofInputDigests -ModuleSummary ([System.IO.File]::ReadAllText($validationPage))
    # A packed package with Go tests must have a row: one without is a package the -tests run did not validate.
    $testedPaths = @(Get-NugetgoTestedImportPaths -ModuleDir $moduleCacheDir -ModulePath $ModulePath -ImportPaths @($packedDigests.Keys))
    $unbound = @(Test-NugetgoValidationBinding -Proof $proofDigests -Packed $packedDigests -Tested $testedPaths -Excluded $ExcludePackage)
    if ($unbound.Count) { Refuse "the validation proof is not a proof of the packed tree -- $($unbound -join '; ')" }
    foreach ($path in @($packedDigests.Keys | Sort-Object)) {
        Write-Host "  proof: $path $(if ($proofDigests.ContainsKey($path)) { "bound at $($packedDigests[$path])" } else { 'has no proof row (no Go tests)' })"
    }
}

# The unvalidated and behavioral pages are packed as written; a test proof is the selection above.
if ($null -eq $proofSummary) { $proofSummary = [System.IO.File]::ReadAllText($validationPage) }

# ---- the package README and icon ---------------------------------------------------------------------------------------
# The author's package page (COORD ruling 2026-10-10) lives in the converted module's own directory, the one tree the
# pack owns and every conversion repository carries: nuget/README.md and nuget/icon.png. In a flat conversion repository
# that is nuget/ at its root; in a nested one, <project dir>/nuget/.
$pageDir = Join-Path $moduleSrc 'nuget'
$authorReadme = Join-Path $pageDir 'README.md'
$authorIconFile = Join-Path $pageDir 'icon.png'
$iconSource = Select-NugetgoIconSource -Icon $Icon -AuthorIcon $(if (Test-Path -LiteralPath $authorIconFile -PathType Leaf) { $authorIconFile } else { '' })
if ($iconSource.Reason) { Refuse $iconSource.Reason }
$packageIcon = Resolve-NugetgoPackageIcon -Icon $iconSource.Path -ConvertedDirectories @($libraries | ForEach-Object { $_.DirectoryName })
if ($packageIcon.Reason) { Refuse $packageIcon.Reason }
# The conversion repository's tag the README links: the RELEASE version's, a rehearsal included (its own version is
# never tagged).
$repositoryTag = "nuget-$($ver.Version)"
$blockArgs = @{
    Description = $description; ModulePath = $ModulePath; GoVersion = $GoVersion; PackageVersion = $packageVersion
    ClosureVersion = $ClosureVersion; RepositoryUrl = $RepositoryUrl; RepositoryTag = $repositoryTag; LicenseName = $licenseName; LicenseSpdx = $LicenseSpdx
    DefaultIcon = $packageIcon.Default
}
if ($proofMode.Mode -ceq 'unvalidated') { $blockArgs.Unvalidated = $true }
elseif ($proofMode.Mode -ceq 'behavioral') { $blockArgs.BehaviorRids = $behaviorRows.Count }
else {
    $blockArgs.Matched = $totals.Matched; $blockArgs.Disclosed = $totals.Disclosed
    if ($proofMode.SampleRows) { $blockArgs.SampleRids = $behaviorRows.Count }
}
# The facts the block states that change from pack to pack, printed on every write so that a rehearsal's rewrite is
# never taken for the release's.
$facts = "package version $packageVersion, tag $repositoryTag"
$utf8 = New-Object System.Text.UTF8Encoding($false)
if (-not (Test-Path -LiteralPath $authorReadme -PathType Leaf)) {
    # SEED: the synopsis of the module's root package, from its Go source. A module whose root package is not packed
    # has no one package to speak for it, and its README has no synopsis.
    $synopsis = ''
    if (@($packed | Where-Object { $_.ImportPath -ceq $ModulePath }).Count) {
        if (-not (Test-Path -LiteralPath $moduleCacheDir -PathType Container)) { Refuse "no module source at $moduleCacheDir for the README's synopsis (go mod download $ModulePath@$GoVersion)" }
        Push-Location (Join-Path (Split-Path (Split-Path $PSScriptRoot)) 'go2cs')
        try { $synopsis = (& go run ./internal/gensynopsis -dir $moduleCacheDir | Out-String).Trim(); if ($LASTEXITCODE -ne 0) { Refuse "gensynopsis could not read $moduleCacheDir ($LASTEXITCODE)" } }
        finally { Pop-Location }
    }
    $readme = New-NugetgoPackageReadme -Id $id.Id -Synopsis $synopsis @blockArgs
    New-Item -ItemType Directory -Force $pageDir | Out-Null
    [System.IO.File]::WriteAllText($authorReadme, $readme, $utf8)
    Write-Host "  readme: SEEDED $authorReadme ($facts). Commit it: from now on it is yours, and every pack rewrites only the block between the nugetgo:generated markers."
}
else {
    $raw = [System.IO.File]::ReadAllBytes($authorReadme)
    $bom = $raw.Length -ge 3 -and $raw[0] -eq 0xEF -and $raw[1] -eq 0xBB -and $raw[2] -eq 0xBF
    $merged = Merge-NugetgoReadme ([System.IO.File]::ReadAllText($authorReadme)) (New-NugetgoGeneratedBlock @blockArgs)
    if ($merged.Reasons.Count) { Refuse "the package README $authorReadme -- $($merged.Reasons -join '; ')" }
    $readme = $merged.Text
    if ($merged.Rewritten) {
        [System.IO.File]::WriteAllText($authorReadme, $readme, (New-Object System.Text.UTF8Encoding($bom)))
        Write-Host "  readme: the generated block in $authorReadme was stale: REWRITTEN with $facts. Commit it."
    }
    else { Write-Host "  readme: $authorReadme, its generated block current ($facts)" }
}
# The README's first link is the Go module's source at its version (owner review 2026-10-10), so no link may come
# before the fence.
$moduleSource = Get-NugetgoModuleSourceUrl $ModulePath $GoVersion
$readmeReasons = @(Test-NugetgoReadme $readme $moduleSource) + @(Test-NugetgoReadmeFence $readme)
if ($readmeReasons.Count) { Refuse "the package README $authorReadme is not a nuget.org README -- $($readmeReasons -join '; ')" }
Write-Host "  readme: $(@($readme -split "`n").Count) lines; links the conversion tag $repositoryTag"
Write-Host "  icon: $($packageIcon.Name) $(if ($packageIcon.Default) { '(the go2cs icon the conversion carries)' } elseif ($iconSource.Path -eq $authorIconFile) { "(the author's $authorIconFile)" } else { "(-Icon $($packageIcon.Path))" })"
foreach ($page in $pages) {
    $rel = $page.FullName.Substring($ValidationDir.TrimEnd('\', '/').Length + 1).Replace('\', '/')
    if ($rel -ieq 'README.md' -or $rel -ieq $packageIcon.Name) { Refuse "the validation page $rel would be packed over the package's $rel" }
}

# ---- flavors: the go.* release's per-OS promotion (COORD ruling 2026-10-12) ---------------------------------------------
# A -recurse conversion emits ONE GOOS, and a module can differ by GOOS (mousetrap's StartedByExplorer, cobra's
# command_win.go). So the module is converted and built once per GOOS of the release's RID map, with this tree's
# converter and the module's own source, and the flavors' assemblies are compared with the release's material check
# (NugetgoFlavors.psm1). Equal in meaning: one neutral assembly set, packed from -RecurseRoot as always. Different: the
# release's RID-specific layout, after the pack. The proof's flavor is -RecurseRoot itself; the pack's own conversion of
# that GOOS must reproduce every packed project's GoInputDigest, so the other flavors are known to be the same input,
# options and converter.
$flavorDir = Join-Path $Scratch 'flavors'
if (Test-Path $flavorDir) { Remove-Item -Recurse -Force $flavorDir }
New-Item -ItemType Directory -Force $flavorDir | Out-Null
$rootProps = [System.IO.File]::ReadAllText((Join-Path $RecurseRoot 'Directory.Build.props'))
if ($rootProps -notmatch '<GoCompileRuntimeIdentifier>(?<rid>[^<]+)</GoCompileRuntimeIdentifier>') { Refuse "$RecurseRoot/Directory.Build.props names no GoCompileRuntimeIdentifier: the GOOS -RecurseRoot was converted for cannot be read" }
$proofRid = $Matches['rid']
$ridFlavors = Get-NugetgoRidFlavors
if (-not $ridFlavors.Contains($proofRid)) { Refuse "-RecurseRoot was converted for $proofRid, a RID the go.* release does not ship ($(@($ridFlavors.Keys) -join ', '))" }
$proofGoos = $ridFlavors[$proofRid]
$flavorArch = if ($proofRid -like '*-arm64') { 'arm64' } else { 'amd64' }
if (-not (Test-Path -LiteralPath $moduleCacheDir -PathType Container)) { Refuse "no module source at $moduleCacheDir to convert each flavor from (go mod download $ModulePath@$GoVersion)" }
$flavorConverterDir = Join-Path (Split-Path (Split-Path $PSScriptRoot)) 'go2cs'
if ($Converter) {
    if (-not (Test-Path -LiteralPath $Converter -PathType Leaf)) { Refuse "-Converter: no file at $Converter" }
    $converter = [System.IO.Path]::GetFullPath($Converter)
}
else {
    $converter = Join-Path $flavorDir $(if ($env:OS -eq 'Windows_NT') { 'go2cs.exe' } else { 'go2cs' })
    Push-Location $flavorConverterDir
    try { & go build -o $converter .; if ($LASTEXITCODE -ne 0) { Refuse "the converter at $flavorConverterDir did not build ($LASTEXITCODE)" } }
    finally { Pop-Location }
}
$flavorProjects = @{}
foreach ($goos in Get-NugetgoFlavorGoos) {
    $root = Join-Path $flavorDir $goos
    $log = Join-Path $flavorDir "$goos.log"
    & $converter -recurse=nuget -platforms "$goos/$flavorArch" -go2cspath (Split-Path $flavorConverterDir) $moduleCacheDir $root *> $log
    if ($LASTEXITCODE -ne 0) { Refuse "the $goos flavor of $ModulePath did not convert ($LASTEXITCODE): $log" }
    $flavorSrc = Join-Path (Join-Path $root 'src') $ModulePath
    $flavorProjects[$goos] = @(foreach ($p in $libraries) {
        $rel = $p.FullName.Substring($moduleSrc.Length).TrimStart('\', '/')
        $fp = Join-Path $flavorSrc $rel
        if (-not (Test-Path -LiteralPath $fp -PathType Leaf)) { Refuse "the $goos flavor has no ${rel}: a package missing on one GOOS is not packed" }
        Get-Item -LiteralPath $fp
    })
    # The assemblies' copyright targets the pack wrote beside -RecurseRoot's module, so every flavor's assemblies carry it.
    if (Test-Path -LiteralPath $moduleTargets) { Copy-Item -LiteralPath $moduleTargets (Join-Path $flavorSrc 'Directory.Build.targets') -Force }
}
for ($i = 0; $i -lt $libraries.Count; $i++) {
    $packedDigest = Get-NugetgoProjectInputDigest -ProjectFile $libraries[$i].FullName
    $flavorDigest = Get-NugetgoProjectInputDigest -ProjectFile $flavorProjects[$proofGoos][$i].FullName
    if ($packedDigest -cne $flavorDigest) { Refuse "the pack's $proofGoos conversion does not reproduce -RecurseRoot: $($libraries[$i].Name) records $packedDigest there and $flavorDigest here. -RecurseRoot was converted from other sources, options or converter than $moduleCacheDir and $converter (pass the go2cs that converted it as -Converter)" }
}
$flavorProjects[$proofGoos] = $libraries.ToArray()
$flavorConfig = Join-Path $flavorDir 'nuget.config'
[System.IO.File]::WriteAllText($flavorConfig, @"
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <clear />
    <add key="nugetgo-closure" value="$([System.Security.SecurityElement]::Escape($Feed))" />
  </packageSources>
</configuration>
"@, (New-Object System.Text.UTF8Encoding($false)))
$flavorAssemblies = @{}
$flavorPdbs = New-Object System.Collections.Generic.List[string]
$isolated = Join-Path $Scratch 'nuget-packages'
New-Item -ItemType Directory -Force $isolated | Out-Null
$saved = @{ NUGET_PACKAGES = $env:NUGET_PACKAGES; GoStdLibVersion = $env:GoStdLibVersion; ContinuousIntegrationBuild = $env:ContinuousIntegrationBuild; PathMap = $env:PathMap }
try {
    # The pack's own build environment (below), each flavor's root mapped to /_/ as -RecurseRoot is, so the flavors
    # differ only where their sources do.
    $env:NUGET_PACKAGES = $isolated; $env:GoStdLibVersion = $stdlibRange; $env:ContinuousIntegrationBuild = 'true'
    foreach ($goos in Get-NugetgoFlavorGoos) {
        $root = if ($goos -ceq $proofGoos) { $RecurseRoot } else { Join-Path $flavorDir $goos }
        $env:PathMap = "$([System.IO.Path]::GetFullPath($root).TrimEnd('\', '/'))=/_/,$([System.IO.Path]::GetFullPath($Scratch).TrimEnd('\', '/'))=/_scratch/"
        $flavorAssemblies[$goos] = @{}
        foreach ($p in $flavorProjects[$goos]) {
            & dotnet build $p.FullName -c Release "-p:RestoreConfigFile=$flavorConfig" --nologo -v q
            if ($LASTEXITCODE -ne 0) { throw "the $goos flavor of $($p.Name) did not build ($LASTEXITCODE)" }
            $targetPath = (& dotnet msbuild $p.FullName -getProperty:TargetPath -p:Configuration=Release | Out-String).Trim()
            if (-not (Test-Path -LiteralPath $targetPath -PathType Leaf)) { throw "the $goos flavor of $($p.Name) built no assembly at '$targetPath'" }
            $flavorAssemblies[$goos][(Split-Path -Leaf $targetPath)] = [System.IO.File]::ReadAllBytes($targetPath)
            $flavorPdbs.Add([System.IO.Path]::ChangeExtension($targetPath, '.pdb'))
        }
    }
}
finally { foreach ($k in $saved.Keys) { Set-Item "env:$k" $saved[$k] -ErrorAction SilentlyContinue; if ($null -eq $saved[$k]) { Remove-Item "env:$k" -ErrorAction SilentlyContinue } } }
$flavorDecision = Get-NugetgoFlavorDecision -Assemblies $flavorAssemblies -ProofGoos $proofGoos
# Third-party requires, per TFM: the union of every flavor's (COORD ruling 2026-10-12). A neutral package's flavors agree,
# so for one the union is -RecurseRoot's own list.
$flavorRequires = @{}
foreach ($goos in Get-NugetgoFlavorGoos) {
    $root = if ($goos -ceq $proofGoos) { $RecurseRoot } else { Join-Path $flavorDir $goos }
    $flavorRequires[$goos] = @(Get-NugetgoThirdPartyRequires -RecurseRoot $root -Libraries $flavorProjects[$goos] -ThirdPartyPackage $ThirdPartyPackage -AllowUnreferenced)
}
try { $requires = @(Merge-NugetgoFlavorRequires -Requires $flavorRequires -ThirdPartyPackage $ThirdPartyPackage) }
catch { Refuse ($_.Exception.Message -replace '^REFUSED: ', '') }
if ($flavorDecision.RidSpecific) {
    Write-Host "  flavors: $((Get-NugetgoFlavorGoos) -join ', ') differ in meaning: RID-specific assets ($(@($ridFlavors.Keys) -join ', ')), lib/ the $(Get-NugetgoReferenceGoos) flavor, as the go.* release packs them"
    foreach ($why in $flavorDecision.Reasons) { Write-Host "    $why" }
    # Dependencies are per target framework, never per RID (push-nuget.ps1): the package declares every flavor's.
    foreach ($goos in Get-NugetgoFlavorGoos) {
        foreach ($p in $flavorProjects[$goos]) {
            [xml]$x = Get-Content -Raw -LiteralPath $p.FullName
            foreach ($ref in @($x.Project.ItemGroup | ForEach-Object { $_.PackageReference } | Where-Object { $_ })) {
                if ($ref.Include -like 'go.*' -and $ref.Version -eq '$(GoStdLibVersion)' -and $ref.PrivateAssets -ne 'all') { [void]$goRefs.Add($ref.Include) }
            }
        }
    }
}
else { Write-Host "  flavors: $((Get-NugetgoFlavorGoos) -join ', ') agree in meaning: one neutral assembly set, packed from -RecurseRoot ($proofGoos)" }

# ---- the generated module pack project --------------------------------------------------------------------------------
$packDir = Join-Path (Join-Path $Scratch 'pack') $id.Id
if (Test-Path $packDir) { Remove-Item -Recurse -Force $packDir }
New-Item -ItemType Directory -Force $packDir | Out-Null
Copy-Item -LiteralPath $LicenseFile (Join-Path $packDir $licenseName)
Copy-Item -LiteralPath $packageIcon.Path (Join-Path $packDir $packageIcon.Name)
[System.IO.File]::WriteAllText((Join-Path $packDir 'README.md'), $readme, (New-Object System.Text.UTF8Encoding($false)))
# VALIDATION.md = the ruled two sentences as its header, then the proof page itself, unchanged below them.
# A RID-specific package's page discloses, in one line, the flavors its self-description does not describe.
$flavorDisclosure = Get-NugetgoFlavorDisclosure -ProofGoos $proofGoos -RidSpecific:$flavorDecision.RidSpecific
[System.IO.File]::WriteAllText((Join-Path $packDir 'VALIDATION.md'),
    ("> $description`n`n" + $(if ($flavorDisclosure) { "> $flavorDisclosure`n`n" } else { '' }) + $proofSummary +
        $(if ($proofMode.SampleRows) { "`n" + (New-NugetgoSampleBehaviorSection -ModulePath $ModulePath -GoVersion $GoVersion -Rows $behaviorRows) } else { '' })), (New-Object System.Text.UTF8Encoding($false)))
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
# Each packed package's own usings (build/<PackageId>.props): this tree's go.lib.pack.targets reads every packed package
# directory's package_info.cs, so the one props names every package's namespace and, opt-in, alias. Product=go2cs is what
# the targets recognise a converted pack by.
$packageDirs = ($libraries | ForEach-Object { "    <GoPackageDirectory Include=`"$(& $esc $_.DirectoryName)`" />" }) -join "`n"
$packTargets = Join-Path (Split-Path (Split-Path $PSScriptRoot)) 'core/golib/buildTransitive/go.lib.pack.targets'
if (-not (Test-Path -LiteralPath $packTargets -PathType Leaf)) { Refuse "no go.lib.pack.targets at $packTargets" }
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
    <!-- lib/ is the module's own assemblies, each referenced directly below. A library's pkg/ reference (a third-party
         module, a NuGet dependency of this package) must not resolve through it transitively into lib/. -->
    <DisableTransitiveProjectReferences>true</DisableTransitiveProjectReferences>
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
    <PackageTags>go2cs;golang;go</PackageTags>
    <Product>go2cs</Product>
  </PropertyGroup>
  <ItemGroup>
$projRefs
  </ItemGroup>
  <ItemGroup>
$packageDirs
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
  <Import Project="$(& $esc $packTargets)" Condition="'`$(GoLibPackTargetsImported)' != 'true'" />
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
if ($flavorDecision.RidSpecific) { Set-NugetgoRidAssets -Package $nupkg -Assemblies $flavorAssemblies -TargetFramework 'net10.0' }
# The behavioral proof must be OF the packed bytes: each RID's row ran the assembly this package carries for that RID,
# on that RID and its GOOS, and printed what go run printed.
if ($proofMode.Mode -ceq 'behavioral' -or $proofMode.SampleRows) {
    $unproven = @(Test-NugetgoBehaviorRows -Rows $behaviorRows -Assemblies (Get-NugetgoPackedRidAssemblies -Package $nupkg -TargetFramework 'net10.0') -ModulePath $ModulePath -GoVersion $GoVersion)
    if ($unproven.Count) { Remove-Item -LiteralPath $nupkg; Refuse "the $(if ($proofMode.SampleRows) { 'sample rows are' } else { 'behavioral proof is' }) not a proof of the packed bytes -- $($unproven -join '; ')" }
    foreach ($rid in @($behaviorRows.Keys | Sort-Object)) { Write-Host "  proof: $rid ran $($behaviorRows[$rid].assembly) on $($behaviorRows[$rid].host): bound" }
}
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
    $runtimeFiles = @($zip.Entries | Where-Object { $_.FullName -like 'runtimes/*.dll' } | ForEach-Object {
        $out = Join-Path $libDir ($_.FullName -replace '[\\/]', '_'); [System.IO.Compression.ZipFileExtensions]::ExtractToFile($_, $out, $true); $out })
}
finally { $zip.Dispose() }
$md = $nuspec.package.metadata
$deps = @($md.dependencies.group | ForEach-Object { $_.dependency } | Where-Object { $_ })
$badDeps = @($deps | Where-Object { $_.id -like 'go.*' -and -not ([string]$_.version).StartsWith("[$ClosureVersion, ", [StringComparison]::Ordinal) })
Write-Host "==> read back: $($md.id) $($md.version)"
Write-Host "    lib: $((@($entries | Where-Object { $_ -like 'lib/*' })) -join ', ')"
Write-Host "    runtimes: $(if ($flavorDecision.RidSpecific) { (@($entries | Where-Object { $_ -like 'runtimes/*' })) -join ', ' } else { 'none (one neutral assembly set)' })"
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
elseif ($packedReadme -cne $readme) { $presentation.Add("the packed README.md differs from $authorReadme as composed") }
else { foreach ($why in @(Test-NugetgoReadme $packedReadme $moduleSource) + @(Test-NugetgoReadmeFence $packedReadme)) { $presentation.Add("README.md $why") } }
if ($md.description.Contains('PROOF:')) { $presentation.Add('the description carries "PROOF:"') }
# nuget.org shows each tag as a chip; PROOF means nothing to a reader there (COORD ruling C, 2026-10-10).
if ($md.tags -match '\bPROOF\b') { $presentation.Add("the tags '$($md.tags)' carry PROOF") }
if (-not $md.icon) { $presentation.Add('the package has no icon') }
elseif ($md.icon -cne $packageIcon.Name -or $null -eq $packedIcon -or -not [System.Linq.Enumerable]::SequenceEqual($packedIcon, [System.IO.File]::ReadAllBytes($packageIcon.Path))) { $presentation.Add("the package icon '$($md.icon)' is not $($packageIcon.Path), byte for byte") }
if (@($entries | Where-Object { $_ -ceq 'VALIDATION.md' }).Count -ne 1) { $presentation.Add('VALIDATION.md is not packed as a file') }
Write-Host "    readme and icon: $(if ($presentation.Count) { "$($presentation.Count) refusal(s)" } else { 'README.md generated and clean; icon packed' })"
if ($presentation.Count) {
    Remove-Item -LiteralPath $nupkg
    throw "REFUSED: the package's nuget.org page -- $($presentation -join '; ')"
}
if ($badDeps.Count) { throw "go.* dependencies not at the B4 range: $(($badDeps | ForEach-Object { "$($_.id) $($_.version)" }) -join ', ')" }
if ($flavorDecision.RidSpecific) {
    $layout = @(Test-NugetgoRidLayout -Package $nupkg -Assemblies $flavorAssemblies -TargetFramework 'net10.0')
    if ($layout.Count) { Remove-Item -LiteralPath $nupkg; throw "REFUSED: the RID-specific layout -- $($layout -join '; ')" }
}
elseif (@($entries | Where-Object { $_ -like 'runtimes/*' }).Count) { Remove-Item -LiteralPath $nupkg; throw 'REFUSED: a neutral package carries runtimes/ entries' }
if (@($entries | Where-Object { $_ -like 'lib/*.dll' }).Count -ne $libraries.Count) { throw "lib/ carries $(@($entries | Where-Object { $_ -like 'lib/*.dll' }).Count) assemblies for $($libraries.Count) packages" }
# Each assembly's copyright and company, read from the dll taken back OUT of the nupkg: its version resource, the
# values a file's properties show.
foreach ($dll in @($libFiles) + @($runtimeFiles)) {
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
foreach ($pdb in $flavorPdbs) {
    if (-not (Test-Path -LiteralPath $pdb -PathType Leaf)) { throw "no $pdb beside a flavor's assembly" }
    foreach ($root in @(Find-NugetgoHostPaths -Bytes ([System.IO.File]::ReadAllBytes($pdb)) -Roots $hostRoots)) { $hostPathHits.Add("$(Split-Path -Leaf $pdb) ($pdb) names $root") }
}
Write-Host "    host paths: $($hostPathHits.Count) in $($entries.Count) packed entries, $($libFiles.Count) pdb(s) and the $($flavorPdbs.Count) flavor pdb(s)"
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
