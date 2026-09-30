<#
.SYNOPSIS
    Packs the go2cs converted Go standard library (plus the go.lib runtime and go.gen source
    generator) as NuGet packages from a fresh Release build, and optionally pushes them to a feed.

.DESCRIPTION
    Targets ONLY src\go2cs-stdlib.slnx -- the generated standard-library solution, which contains
    the ~301 converted stdlib libraries, the hand-owned ones (unsafe, testing) AND the two shared
    infrastructure projects core\golib (go.lib) and gen\go2cs-gen (go.gen). It deliberately never
    packs src\go2cs.slnx, whose behavioral-test and example projects are not publishable.

    MULTIPLATFORM (docs\phase4\DESIGN-multiplatform-corpus.md section 9(a), increment 4). The converted corpus
    is one tree whose platform-varying packages keep per-GOOS sources selected by $(GoTargetOS), so the
    solution is built and packed ONCE PER RID and the flavors are merged into a single nupkg per
    package: lib\<tfm> carries the reference (Windows) flavor as the compile-time asset, and
    runtimes\<rid>\lib\<tfm> carries each shipped RID's runtime assembly. Package IDs, and everything a
    consumer writes, are unchanged. Platform-neutral packages -- the large majority -- are copied
    verbatim from the reference pass and are byte-for-byte what a single-pass release produced.

    All packages share one version, sourced from src\version.props: <GoStdLibVersion>.<GoBuildNumber>
    (base tracks the converted Go release, e.g. 1.23.1; the 4th part is the build/publish counter).
    -Push increments GoBuildNumber by default so every publish is a new version (commit version.props
    afterward to record the release). See -BumpBuild to force or suppress the bump.

    PUBLICATION ALSO FREEZES THE PROOF. Before anything is built, docs\validation\current\ is copied
    to docs\validation\<version>\ (write-once) and the version-pinned validation badge links in every
    src\core\*\README.md are retargeted at it, so a published package's green badge, its proof link
    and the VALIDATION.md it packs all describe the exact binary being pushed. The ROSTER PAGE is
    frozen with them (since 2026-09-07) as docs\validation\<version>\ValidatedTestPackages.md, its
    proof links retargeted onto the snapshot's own siblings, so the snapshot is a self-contained
    record of how the campaign stood at publication rather than 200-odd pages with no page around
    them. Commit the snapshot, the retargeted READMEs and version.props together.

    IT ALSO MINTS THE RELEASE TAG, at that same pre-build moment rather than after the push. Every
    README's C# Source badge links github.com/ritchiecarroll/go2cs/tree/nuget-<version>/src/core/<pkg>,
    so the tag has to exist by the time those READMEs are baked into packages -- tagging afterward
    published a README full of links to a tag that did not exist yet. Creation is idempotent
    (check-then-skip) so a re-run after a failed later phase does not die on "tag already exists".

    SAFETY: pushing to a public feed is an irreversible publish (a version can be unlisted, never
    deleted). This script therefore PACKS ONLY by default; it pushes nothing unless -Push is given,
    and -WhatIf reports each push without performing it. The API key is read from the NUGET_API_KEY
    environment variable (or -ApiKey) and is never written to disk.

.PARAMETER ApiKey
    NuGet API key for -Push. Defaults to the NUGET_API_KEY environment variable.

.PARAMETER Source
    NuGet push source. Defaults to https://api.nuget.org/v3/index.json. May be a local folder feed.

.PARAMETER Configuration
    Build configuration to pack. Defaults to Release.

.PARAMETER OutDir
    Directory to collect the merged .nupkg files. Defaults to src\artifacts\nupkg. Each RID's
    unmerged pack is kept beside them under _flavors\<rid>\ for inspection.

.PARAMETER SkipBuild
    NO LONGER SUPPORTED, and rejected with an explanation. A multiplatform release is built once per
    RID with a different $(GoTargetOS) each time, so there is no single on-disk build to pack.

.PARAMETER BumpBuild
    Force or suppress the GoBuildNumber increment in src\version.props (then commit it). Defaults to
    ON with -Push (each publish is a new version) and OFF for a pack-only run. Pass -BumpBuild:$false
    to publish the CURRENT version without bumping -- e.g. finishing a partially-failed push (with
    --skip-duplicate) or serial automation that manages the version itself.

.PARAMETER Push
    Actually push the packed .nupkg to the feed. Without this switch the script only packs.

.PARAMETER VerifyOnly
    Run the pre-flight verification and the release census, print the result, and exit -- 0 when
    clean, non-zero naming every mismatch. Nothing is bumped, tagged, frozen, built, packed or
    pushed, and the Go toolchain is not required. This is the cheap check to run after the last
    roster-moving sweep, so a release morning cannot discover a stale badge with a signed tag and a
    write-once snapshot already on disk. Mutually exclusive with -Push and -BumpBuild.

    The census is the runbook's NAMED IDENTITIES (tests.csproj, proof pages, green badges and index
    rows, each term named), and the pre-flight also composes the version the next publish would mint
    and refuses it if it already exists, or is not numerically newer than every recorded
    docs\validation\<version>\ snapshot and nuget-* tag, and it refuses by name any relative link the
    frozen roster would carry that cannot be relocated or resolves to no tracked path. Those halves
    read git (`git tag --list`, `git ls-remote --tags origin`, `git ls-files`, all read-only), so git
    must be on PATH; an origin that cannot be reached is reported by name and the tag comparison runs
    on the local tags.

.PARAMETER VersionSuffix
    A LOCAL REHEARSAL's prerelease label (e.g. local.1): every package is packed as
    <GoStdLibVersion>.<GoBuildNumber>-<VersionSuffix>, and every go.* dependency between them carries that
    same version. PACK-ONLY: refused with -Push, -BumpBuild and -VerifyOnly, so a suffixed package can never
    reach a feed through this script, and version.props, the tags and docs\validation are never touched.
    Only the PACKAGE version moves; assembly versions are unchanged. It exists for local-feed rehearsals
    (the J0 pilot) whose packages must not share an id+version with a published release in a NuGet cache.

.EXAMPLE
    .\push-nuget.ps1
    Pack every package to src\artifacts\nupkg (no push, no bump). Inspect the output, then push.

.EXAMPLE
    .\push-nuget.ps1 -VersionSuffix local.1 -OutDir D:\feeds\go2cs-local
    A local rehearsal: pack every package as <version>-local.1 into a folder feed. Nothing is bumped,
    tagged, frozen or pushed.

.EXAMPLE
    .\push-nuget.ps1 -VerifyOnly
    Verify the tree is releasable -- every green badge's arithmetic against its proof page, every
    package README's C# Source badge, the release census's named identities, and the next release's
    existence and monotonicity -- in seconds, changing nothing.

.EXAMPLE
    .\push-nuget.ps1 -Push
    The normal release: bump the build number, build + pack Release, and push the NEXT version to
    nuget.org (NUGET_API_KEY set).

.EXAMPLE
    .\push-nuget.ps1 -Push -BumpBuild:$false
    Re-push the CURRENT version without bumping -- e.g. to finish a partially-failed publish
    (--skip-duplicate skips the packages already on the feed).
#>
#Requires -Version 5.1
[CmdletBinding(SupportsShouldProcess = $true)]
param(
    [string]$ApiKey = $env:NUGET_API_KEY,
    [string]$Source = 'https://api.nuget.org/v3/index.json',
    [string]$Configuration = 'Release',
    [string]$OutDir,
    [switch]$SkipBuild,
    [switch]$BumpBuild,
    [switch]$Push,
    [switch]$VerifyOnly,
    [string]$VersionSuffix
)

$ErrorActionPreference = 'Stop'

$src = $PSScriptRoot
$slnx = Join-Path $src 'go2cs-stdlib.slnx'
$versionProps = Join-Path $src 'version.props'
if (-not $OutDir) { $OutDir = Join-Path $src 'artifacts\nupkg' }

$utf8NoBom = New-Object System.Text.UTF8Encoding($false)

function Write-Step($msg) { Write-Host "==> $msg" -ForegroundColor Cyan }

if (-not (Test-Path $slnx)) { throw "Solution not found: $slnx" }
if (-not (Test-Path $versionProps)) { throw "version.props not found: $versionProps" }

$repoRoot = Split-Path $src -Parent

if ($VerifyOnly -and ($Push -or $BumpBuild)) {
    throw ("-VerifyOnly is mutually exclusive with -Push and -BumpBuild: it checks the tree and exits " +
           "before anything is bumped, tagged, frozen, packed or published.")
}

# A local rehearsal's label is PACK-ONLY by construction: a suffixed package must never be pushed, and a
# run that bumps is a release. -VerifyOnly packs nothing, so a suffix there is a mistake worth naming.
if ($VersionSuffix) {
    if ($Push -or $PSBoundParameters.ContainsKey('BumpBuild') -or $VerifyOnly) {
        throw ("-VersionSuffix is a local-rehearsal pack and is refused with -Push, -BumpBuild and -VerifyOnly: " +
               "a suffixed package must never reach a feed through this script.")
    }

    if ($VersionSuffix -notmatch '^[0-9A-Za-z-]+(\.[0-9A-Za-z-]+)*$') {
        throw "-VersionSuffix '$VersionSuffix' is not a NuGet prerelease label (dot-separated alphanumerics and hyphens)."
    }
}

# --- PRE-FLIGHT VERIFICATION: EVERY CHECKABLE THING, BEFORE ANY IRREVERSIBLE ONE ------------------
# THE ORDER DEFECT THIS FIXES (measured 2026-09-07). The run's order used to be
#
#     bump version.props -> mint the SIGNED tag -> freeze docs\validation\<version>\ -> VERIFY
#
# so all three irreversible acts preceded the only check that can stop a release. Each is awkward to
# undo -- a bumped counter, a signed tag, and a write-once proof snapshot the NEXT release's own
# precondition check refuses to overwrite -- and the verifier at the end of that chain throws on a
# green badge whose arithmetic disagrees with its proof page, which is a state the tree reaches
# NORMALLY: retiring a disclosure moves a package's matched/disclosed split, and the README keeps
# claiming the old one until a reconvert re-emits the badge. The tree was in exactly that state on
# three packages when this was written, so a release run would have minted a signed tag and frozen a
# snapshot and THEN died, leaving both behind for a release that never happened.
#
# WHAT MOVED, AND WHAT CANNOT. Everything below is the version-INDEPENDENT half of the two verifiers
# that used to run at the end, asked here against docs\validation\current -- which is precisely the
# set the freeze is about to copy, so it is the same question asked earlier (the same argument the
# dry-run snapshot below already rests on). What cannot move is the half comparing a badge's pinned
# version against $fullVersion: before the retarget every README legitimately still names the
# PREVIOUS release, so checking it here would fail every run. Those pins stay where they are and are
# verified against the FROZEN snapshot after the retarget -- but through the same two functions, so
# there is ONE predicate rather than two that can drift apart.
#
# BEHAVIOUR DELTA ON THE RELEASE PATH. The late calls are the same checks in the same order against
# the same inputs, with one difference: a problem is COLLECTED and the caller throws, where the
# inline loops threw on the first. Fatality is identical -- any problem still stops the run -- and a
# release that is going to stop now names every package instead of the alphabetically first.
. (Join-Path $PSScriptRoot '_roster.ps1')

# The green Tests badge, verified against a directory of proof pages. An empty $PinnedVersion means
# "do not check the badge's link version", which is what makes this callable before the retarget.
function Get-GoGreenBadgeVerification {
    param(
        [Parameter(Mandatory)][string]$CoreDir,
        [Parameter(Mandatory)][string]$ProofRoot,
        [string]$PinnedVersion = ''
    )

    $problems = New-Object System.Collections.Generic.List[string]
    $verified = 0

    foreach ($readme in Get-ChildItem $CoreDir -Filter 'README.md' -Recurse -File) {
        $text = [System.IO.File]::ReadAllText($readme.FullName)
        if ($text -notmatch 'badge/Tests-(\d+)%2F(\d+)_validated-brightgreen') { continue }

        $badgeMatched = [int]$Matches[1]
        $badgeTotal = [int]$Matches[2]

        # The dot-id itself contains dots (path.filepath), so its capture excludes only "/" and ")".
        if ($text -notmatch 'https://go2cs\.net/validation/([^/]+)/([^)/]+)\.html') {
            $problems.Add("Green badge without a proof link in $($readme.FullName)")
            continue
        }

        $linkVersion = $Matches[1]
        $dotId = $Matches[2]

        if ($PinnedVersion -and $linkVersion -ne $PinnedVersion) {
            $problems.Add("Green badge in $($readme.FullName) still links $linkVersion, not $PinnedVersion")
            continue
        }

        $proofPage = Join-Path $ProofRoot "$dotId.md"
        if (-not (Test-Path $proofPage)) {
            $problems.Add("Green badge in $($readme.FullName) links a proof page missing from $ProofRoot -- $proofPage")
            continue
        }

        $proofText = [System.IO.File]::ReadAllText($proofPage)
        if ($proofText -notmatch '\*\*(\d+) matched \S+ (\d+) disclosed\*\*') {
            $problems.Add("No totals line in $proofPage")
            continue
        }

        if ($badgeMatched -ne [int]$Matches[1] -or $badgeTotal -ne ([int]$Matches[1] + [int]$Matches[2])) {
            $problems.Add("Badge in $($readme.FullName) claims $badgeMatched/$badgeTotal but $proofPage records $($Matches[1]) matched + $($Matches[2]) disclosed")
            continue
        }

        $verified++
    }

    return [pscustomobject]@{ Verified = $verified; Problems = $problems.ToArray() }
}

# The C# Source badge. Its two version pins are checked only when $PinnedVersion is given; the
# STRUCTURAL half -- that every package README carries the badge and its release-tag link at all --
# runs on both calls, because that is the check whose absence let the 1.23.1.5 run ship a badge whose
# form had drifted (a retarget that silently no-opped past a verifier that skipped what it could not
# match).
function Get-GoSourceBadgeVerification {
    param(
        [Parameter(Mandatory)][string]$CoreDir,
        [string]$PinnedVersion = ''
    )

    $problems = New-Object System.Collections.Generic.List[string]
    $verified = 0

    foreach ($readme in Get-ChildItem $CoreDir -Filter 'README.md' -Recurse -File) {
        $text = [System.IO.File]::ReadAllText($readme.FullName)
        # Non-package READMEs legitimately carry no badges: the root attribution file, golib's
        # hand-written runtime README, and testdata corpora (plus anything under build output).
        # The separator class admits '/' as well as '\' so the exclusion still holds when this runs
        # on a non-Windows host, which -VerifyOnly makes reachable; on Windows Get-ChildItem yields
        # '\' and the added alternative can never match, so the release path is unmoved.
        if ($readme.Directory.FullName -eq $CoreDir) { continue }
        if ($readme.FullName -match '[\\/](testdata|bin|obj)[\\/]' -or $readme.Directory.Name -eq 'golib') { continue }
        if ($text -notmatch 'badge/Source-@([^-\s)]+)-512BD4') {
            $problems.Add("README without a C# Source badge: $($readme.FullName) -- every package README carries one; a no-match here means the badge form drifted and this retarget is no-opping (the vacuous pass that shipped on the 1.23.1.5 run)")
            continue
        }

        if ($PinnedVersion -and $Matches[1] -ne $PinnedVersion) {
            $problems.Add("C# Source badge in $($readme.FullName) states version $($Matches[1]), not $PinnedVersion")
            continue
        }

        if ($text -notmatch 'https://github\.com/ritchiecarroll/go2cs/tree/nuget-([^/\s)]+)/src/core/') {
            $problems.Add("C# Source badge without a release-tag link in $($readme.FullName)")
            continue
        }

        if ($PinnedVersion -and $Matches[1] -ne $PinnedVersion) {
            $problems.Add("C# Source badge in $($readme.FullName) links tag nuget-$($Matches[1]), not nuget-$PinnedVersion")
            continue
        }

        $verified++
    }

    return [pscustomobject]@{ Verified = $verified; Problems = $problems.ToArray() }
}

# THE RELEASE CENSUS, AS NAMED IDENTITIES. Four derivations of the campaign's state are read from the
# tree, BY NAME:
#
#   green badges    src\core\**\README.md carrying a green Tests badge
#   proof pages     docs\validation\current\*.md
#   roster rows     the DATA ROWS of docs\ValidatedTestPackages.md's table, parsed by _roster.ps1 --
#                   the sweep's own reader, never the document's prose, which is a dated derivation
#                   that goes stale independently of the table beside it
#   test projects   src\core\**\*.tests.csproj
#
# plus the four the identities need to NAME their other terms: the roster's exclusion ledger, the
# pages its ROWS' [proof] links resolve to, the population of record for the pinned Go release, and
# docs\validation\index.md's rows. (The README-less rows come from the same README walk as the badges.)
#
# THIS WAS A SET EQUALITY until 2026-09-23 -- "four independent derivations of ONE set, the banked
# packages" -- and the H10 close amendment REFUTED that by design (docs\GoCorpusMigration.md, "THE
# RELEASE CENSUS, CORRECTED"; ledger 2026-09-22 16:31, 3469154a95). Exclusion rows keep their
# artifacts, rowless candidates keep theirs, relocation anchors keep their source pages, and a
# test-only package banks with no README at all. At the 1.24.13 close stamp fa18863b94 the equality
# read 214 / 232 / 218 / 225 and refused 21 packages that are every one of them correct, which left
# the pre-flight able to say only "red" -- a gate that is always red stops nothing. The census is now
# the ruled NAMED IDENTITIES, every term named (COORD ruling RN-6, ledger c695b56071):
#
#   tests.csproj    =  banked rows + exclusion rows keeping artifacts + rowless candidates keeping artifacts
#   proof pages     =  rows by name + relocation anchors by a row's [proof] link + exclusion rows by exclusion
#   green badges    =  banked rows - banked rows with no README
#   index rows      =  banked rows
#
# Anything that fits NO named class is refused BY NAME, and so is a named class whose members do not
# add up to the count beside it. The functions that hold each identity are _roster.ps1's -- the same
# ones check-roster-format.ps1 sections 2b2/2b3/2b4 drive with fixtures -- so the release and the
# roster guard cannot disagree about what "accounted for" means. What a violation must NEVER be fixed
# by is deleting a page or inventing a badge to make a set close: every term is a fact about the tree.
#
# A disagreement is a release defect either way: the green badge is what a package advertises on
# nuget.org, and the proof snapshot is what it packs as VALIDATION.md. Reported by NAME as well as by
# count, because "200 against 204" sends the reader on a hunt that a named class answers outright.
function Get-GoReleaseCensus {
    param(
        [Parameter(Mandatory)][string]$CoreDir,
        [Parameter(Mandatory)][string]$CurrentProofs,
        [Parameter(Mandatory)][string]$RosterPath,
        [Parameter(Mandatory)][string]$IndexPath,
        [Parameter(Mandatory)][string]$PopulationPath
    )

    # ONE walk for both README sets: the green badges, and the READMEs that exist at all, which is
    # what names the banked rows the green-badge identity subtracts (a test-only package has none).
    $greenIds = New-Object System.Collections.Generic.List[string]
    $readmeIds = New-Object System.Collections.Generic.List[string]
    foreach ($readme in Get-ChildItem $CoreDir -Filter 'README.md' -Recurse -File) {
        if ($readme.Directory.FullName -eq $CoreDir) { continue }
        if ($readme.FullName -match '[\\/](testdata|bin|obj)[\\/]') { continue }
        $readmeId = $readme.Directory.FullName.Substring($CoreDir.Length).TrimStart('\', '/') -replace '[\\/]', '.'
        $readmeIds.Add($readmeId)
        $text = [System.IO.File]::ReadAllText($readme.FullName)
        if ($text -notmatch 'badge/Tests-\d+%2F\d+_validated-brightgreen') { continue }
        $greenIds.Add($readmeId)
    }

    $pageIds = New-Object System.Collections.Generic.List[string]
    if (Test-Path $CurrentProofs) {
        foreach ($page in Get-ChildItem $CurrentProofs -Filter '*.md' -File) { $pageIds.Add($page.BaseName) }
    }

    # The roster's import paths ('net/http') and every other derivation's dot-id ('net.http') are the
    # same identifier in two spellings; the proof page's own file name is what fixes the mapping, so
    # every list below is converted to the dot-id before any identity compares it.
    $rosterIds = New-Object System.Collections.Generic.List[string]
    foreach ($row in Get-ValidatedRosterRows -Path $RosterPath) { $rosterIds.Add(($row.Package -replace '/', '.')) }

    $excludedIds = New-Object System.Collections.Generic.List[string]
    foreach ($row in Get-ExclusionLedgerRows -Path $RosterPath) { $excludedIds.Add(($row.Package -replace '/', '.')) }

    $linkedIds = @(Get-RosterRowProofLinks -Path $RosterPath)

    # Unique, as check-roster-format's 2b2 reads them: a directory is one package however many test
    # projects it holds.
    $testIds = @(Get-ChildItem $CoreDir -Filter '*.tests.csproj' -Recurse -File |
        ForEach-Object { $_.Directory.FullName.Substring($CoreDir.Length).TrimStart('\', '/') -replace '[\\/]', '.' } |
        Sort-Object -Unique)

    # The two inputs that can be ABSENT without the tree being wrong in any other way are read under a
    # catch, so a missing one is a NAMED problem beside every other rather than a bare throw that hides
    # the rest of the census.
    $inputProblems = New-Object System.Collections.Generic.List[string]

    $populationIds = @()
    try { $populationIds = @(Get-PopulationRows -Path $PopulationPath | ForEach-Object { $_ -replace '/', '.' }) }
    catch { $inputProblems.Add("the rowless-candidate class cannot be derived: $($_.Exception.Message)") }

    $indexIds = @()
    try { $indexIds = @(Get-GoValidationIndexRows -Path $IndexPath | ForEach-Object { $_ -replace '/', '.' }) }
    catch { $inputProblems.Add("the index-row identity cannot be read: $($_.Exception.Message)") }

    return [pscustomobject]@{
        GreenIds = $greenIds.ToArray(); PageIds = $pageIds.ToArray()
        RosterIds = $rosterIds.ToArray(); TestIds = $testIds
        ReadmeIds = $readmeIds.ToArray(); ExcludedIds = $excludedIds.ToArray()
        LinkedIds = $linkedIds; PopulationIds = $populationIds; IndexIds = $indexIds
        InputProblems = $inputProblems.ToArray()
    }
}

# docs\validation\index.md's CURRENT table, one row per banked package. The header and the row shape
# are docs\phase4\hopA-inputs\regen-validation-index.py's own (TABLE_HEAD, INDEX_ROW) -- the tool that
# writes this table -- and the read is SCOPED to that table, so the Frozen snapshots table above it
# (whose rows open with a version, not a code span) can never be read as package rows. Refuses a
# missing header and a header with zero rows: a reader that finds nothing reports a clean identity.
function Get-GoValidationIndexRows {
    param([Parameter(Mandatory)][string]$Path)

    if (-not (Test-Path $Path)) { throw "no validation index at $Path" }

    $lines = [System.IO.File]::ReadAllLines($Path)
    $head = [Array]::IndexOf($lines, '| Package | Proof | Converted package |')
    if ($head -lt 0) { throw "the validation index at $Path carries no CURRENT table header ('| Package | Proof | Converted package |')" }

    $rows = New-Object System.Collections.Generic.List[string]
    for ($i = $head + 2; $i -lt $lines.Count; $i++) {
        if ($lines[$i] -notmatch '^\| `([^`]+)`') { break }
        $rows.Add($Matches[1])
    }

    if ($rows.Count -eq 0) { throw "the validation index at $Path carries its CURRENT table header and ZERO rows" }
    return $rows.ToArray()
}

# THE NEXT RELEASE: EXISTENCE PLUS MONOTONICITY, before anything is bumped (H11; COORD ruling RN-9,
# ledger c695b56071). The version a publish WOULD mint is composed here exactly as the bump below
# composes it -- version.props' base plus (counter + 1) -- and refused if
#
#   it EXISTS      a docs\validation\<version>\ snapshot or a nuget-<version> tag already names it, or
#   it is not NEWER than every recorded release, snapshot and tag alike.
#
# Both halves, because each is blind to what the other sees. Monotonicity alone is TRUE AND
# INSUFFICIENT: a counter carried across a base bump is perfectly monotonic (0f97dcc8db composed
# 1.24.13.3, which was never published). Existence alone cannot see a counter that fell behind a
# release recorded later. The existence half's tree precedent is repoguard's
# TestPublishedCounterMatchesTheRecordedReleases, which reads the snapshots; this reads the tags too,
# because a tag minted by a run that died before its freeze is a release name the feed may already
# hold. Its git calls are `git tag --list` and `git ls-remote --tags origin`, both read-only.
#
# NUMERIC PER COMPONENT, NEVER LEXICAL. 1.23.12.10 sorts below 1.23.12.9 as a string and 1.24.13.1
# below 1.24.9.1. The rule is releasestamp.Compare's (src\go2cs\internal\releasestamp\stamp.go: a
# missing component reads 0), which for the four numeric components a release carries is identical to
# [System.Version]'s ordering and to NuGetVersion's (PLAN OQ-11). [bigint] per component, so no
# component width can overflow into a wrong order.
#
# What is compared is admitted by SHAPE and everything else is printed EXCLUDED, by name, never
# parsed and never dropped silently: a snapshot is a directory named like releasestamp.IsStamp
# (docs\validation\current is the working proof set, not a release), and a release tag is
# nuget-N.N.N.N (nuget-stdlib-2026-07-14 is not one).
function Compare-GoReleaseStamp {
    param([Parameter(Mandatory)][string]$Left, [Parameter(Mandatory)][string]$Right)

    $l = $Left.Split('.')
    $r = $Right.Split('.')

    for ($i = 0; $i -lt [Math]::Max($l.Count, $r.Count); $i++) {
        $lv = if ($i -lt $l.Count) { [bigint]::Parse($l[$i]) } else { [bigint]::Zero }
        $rv = if ($i -lt $r.Count) { [bigint]::Parse($r[$i]) } else { [bigint]::Zero }
        $order = $lv.CompareTo($rv)
        if ($order -ne 0) { return [Math]::Sign($order) }
    }

    return 0
}

#
# THE TAGS ARE READ FROM ORIGIN TOO (2026-09-24, ledger 620ba7a2b8). -Tags is the LOCAL list and
# -RemoteTags origin's (`git ls-remote --tags origin`, read-only); the comparison runs over their UNION.
# A release tag that exists only on origin -- minted and pushed from another clone, never fetched here
# -- is invisible to `git tag --list`, and push-nuget checks for its tag LOCALLY before minting, so a
# local-only read would let this run mint a SECOND nuget-<version> at a different commit for a
# version the feed may already hold. An origin-only name is labelled 'origin tag' wherever it is
# reported. The caller prints, by name, when origin could not be read and the union is local only.
function Get-GoNextReleaseVerification {
    param(
        [Parameter(Mandatory)][string]$VersionPropsText,
        [Parameter(Mandatory)][string]$SnapshotsDir,
        [Parameter(Mandatory)][AllowEmptyCollection()][string[]]$Tags,
        [AllowEmptyCollection()][string[]]$RemoteTags = @()
    )

    $problems = New-Object System.Collections.Generic.List[string]
    $excluded = New-Object System.Collections.Generic.List[string]

    $base = if ($VersionPropsText -match '<GoStdLibVersion>([^<]+)</GoStdLibVersion>') { $Matches[1].Trim() } else { '' }
    $counter = if ($VersionPropsText -match '<GoBuildNumber>([^<]+)</GoBuildNumber>') { $Matches[1].Trim() } else { '' }

    if ($base -notmatch '^[0-9]+(\.[0-9]+)*$' -or $counter -notmatch '^[0-9]+$') {
        $problems.Add("the next release cannot be composed: version.props reads GoStdLibVersion '$base' and GoBuildNumber '$counter'")
        return [pscustomobject]@{ Next = ''; Base = $base; Counter = $counter; Newest = ''; NewestFrom = ''
                                  Snapshots = 0; ReleaseTags = 0; Excluded = @(); Problems = $problems.ToArray() }
    }

    $next = "$base.$([bigint]::Parse($counter) + 1)"

    # version -> where it is recorded ('snapshot', 'tag'); ordinal keys, so 1.24.13.01 and 1.24.13.1
    # are two NAMES that the numeric comparison below then finds equal.
    $recorded = New-Object 'System.Collections.Generic.Dictionary[string,System.Collections.Generic.List[string]]' ([System.StringComparer]::Ordinal)
    $snapshotCount = 0
    $tagCount = 0

    if (-not (Test-Path $SnapshotsDir -PathType Container)) {
        $problems.Add("VACUOUS: the snapshot root $SnapshotsDir is not a directory, so no recorded release can be read -- every comparison below would pass on an empty listing")
    }
    else {
        foreach ($dir in Get-ChildItem $SnapshotsDir -Directory) {
            if ($dir.Name -notmatch '^[0-9]+(\.[0-9]+)+$') {
                $excluded.Add("docs\validation\$($dir.Name)\")
                continue
            }
            if (-not $recorded.ContainsKey($dir.Name)) { $recorded[$dir.Name] = New-Object System.Collections.Generic.List[string] }
            $recorded[$dir.Name].Add('snapshot')
            $snapshotCount++
        }
    }

    # Local tags first, then origin's names that are not local (ordinal), so each name is labelled once.
    $localTagSet = New-Object 'System.Collections.Generic.HashSet[string]' ([System.StringComparer]::Ordinal)
    foreach ($tag in @($Tags | Where-Object { $_ })) { [void]$localTagSet.Add($tag) }
    $originOnly = @($RemoteTags | Where-Object { $_ -and -not $localTagSet.Contains($_) } | Sort-Object -Unique)
    $originOnlyCount = 0

    $tagSources = @(@($localTagSet | Sort-Object) | ForEach-Object { [pscustomobject]@{ Name = $_; Label = 'tag' } }) +
                  @($originOnly | ForEach-Object { [pscustomobject]@{ Name = $_; Label = 'origin tag' } })
    foreach ($source in $tagSources) {
        if ($source.Name -notmatch '^nuget-([0-9]+(\.[0-9]+){3})$') {
            $excluded.Add("$($source.Label) $($source.Name)")
            continue
        }
        $tagVersion = $Matches[1]
        if (-not $recorded.ContainsKey($tagVersion)) { $recorded[$tagVersion] = New-Object System.Collections.Generic.List[string] }
        $recorded[$tagVersion].Add($source.Label)
        if ($source.Label -eq 'tag') { $tagCount++ } else { $originOnlyCount++ }
    }

    if ($recorded.Count -eq 0 -and (Test-Path $SnapshotsDir -PathType Container)) {
        $problems.Add("VACUOUS: no release is recorded at all -- no docs\validation\<version>\ snapshot and no nuget-<version> tag -- which this repository's history contradicts; an instrument reading the wrong place would report every version as new")
    }

    # EXISTENCE, by exact name.
    if ($recorded.ContainsKey($next)) {
        foreach ($where in $recorded[$next]) {
            if ($where -eq 'snapshot') {
                $problems.Add("the next release $next already EXISTS: docs\validation\$next\ is a recorded snapshot. Snapshots are write-once, and version.props' counter must name the LATEST release on its base")
            }
            elseif ($where -eq 'origin tag') {
                $problems.Add("the next release $next already EXISTS: tag nuget-$next is on ORIGIN and not in the local tags. push-nuget checks for its tag locally, so this run would mint a second nuget-$next at another commit for a version the feed may already hold -- fetch the tags and reconcile version.props first")
            }
            else {
                $problems.Add("the next release $next already EXISTS: tag nuget-$next is minted. push-nuget KEEPS an existing tag rather than re-minting it, so the release would publish under a tag naming another tree")
            }
        }
    }

    # MONOTONICITY, numerically, against every OTHER recorded name (an exact name is existence's).
    $newest = ''
    foreach ($version in @($recorded.Keys)) {
        if (-not $newest -or (Compare-GoReleaseStamp $version $newest) -gt 0) { $newest = $version }
        if ($version -ceq $next) { continue }
        if ((Compare-GoReleaseStamp $next $version) -le 0) {
            $problems.Add("the next release $next is not strictly newer than recorded release $version ($($recorded[$version] -join ' + ')) -- compared numerically per component")
        }
    }

    $newestFrom = if ($newest) { $recorded[$newest] -join ' + ' } else { '' }

    return [pscustomobject]@{
        Next = $next; Base = $base; Counter = $counter; Newest = $newest; NewestFrom = $newestFrom
        Snapshots = $snapshotCount; ReleaseTags = $tagCount; OriginOnlyTags = $originOnlyCount
        Excluded = $excluded.ToArray(); Problems = $problems.ToArray()
    }
}

# Origin's nuget-* tag NAMES, read-only (`git ls-remote --tags origin 'refs/tags/nuget-*'`). Returns
# the names and, when origin could not be read, the reason -- never an empty list that reads as "origin
# has none". 'Continue' in this function only: under the script's 'Stop' a stderr line from git would
# end the pre-flight, and an unreachable origin must be REPORTED and passed over, not fatal.
# GIT_TERMINAL_PROMPT=0 so a remote that wants credentials fails at once instead of waiting on a
# console nobody is watching. The peeled `^{}` rows of annotated tags are the same names and dropped.
function Get-GoOriginReleaseTags {
    param([Parameter(Mandatory)][string]$RepoRoot)

    $ErrorActionPreference = 'Continue'
    $savedPrompt = $env:GIT_TERMINAL_PROMPT
    $env:GIT_TERMINAL_PROMPT = '0'
    try {
        $stdout = New-Object System.Collections.Generic.List[string]
        $stderr = New-Object System.Collections.Generic.List[string]
        & git -C $RepoRoot ls-remote --tags origin 'refs/tags/nuget-*' 2>&1 | ForEach-Object {
            if ($_ -is [System.Management.Automation.ErrorRecord]) { $stderr.Add($_.ToString()) } else { $stdout.Add("$_") }
        }
        $code = $LASTEXITCODE
    }
    catch {
        return [pscustomobject]@{ Names = @(); Problem = "git ls-remote could not run ($($_.Exception.Message))" }
    }
    finally {
        $env:GIT_TERMINAL_PROMPT = $savedPrompt
    }

    if ($code -ne 0) {
        $reason = (@($stderr | Where-Object { $_.Trim() }) | Select-Object -First 2) -join ' / '
        return [pscustomobject]@{ Names = @(); Problem = "git ls-remote --tags origin exited $code ($reason)" }
    }

    $names = @($stdout | ForEach-Object { if ($_ -match '\srefs/tags/(\S+)$') { $Matches[1] } } |
               Where-Object { $_ -notlike '*^{}' } | Sort-Object -Unique)
    return [pscustomobject]@{ Names = $names; Problem = $null }
}

# THE FROZEN ROSTER'S RELATIVE LINKS. ConvertTo-FrozenRosterText relocates every relative,
# path-shaped link by '../../' so it names the same path from two directories deeper; this names the
# two ways one can still dangle in the published snapshot, each REFUSED by name: a link the transform
# could not relocate at all (its Unrelocated audit arm), and a relocated link whose target is not a
# tracked path (Get-UnresolvedRelativeLinks, against `git ls-files`). One definition, called by the
# pre-flight on a dry transform of the living roster and again by the freeze before it writes.
function Get-GoFrozenRosterLinkProblems {
    param(
        [Parameter(Mandatory)]$Transform,
        [Parameter(Mandatory)][AllowEmptyCollection()][string[]]$TrackedPaths
    )

    $problems = New-Object System.Collections.Generic.List[string]
    foreach ($link in @($Transform.Unrelocated)) {
        $problems.Add("roster link the freeze cannot relocate (it would dangle in the frozen snapshot): $link")
    }
    foreach ($link in @(Get-UnresolvedRelativeLinks -Targets @($Transform.Relocated) -TrackedPaths $TrackedPaths)) {
        $problems.Add("roster link resolves to no tracked path (it would dangle in the frozen snapshot): $link")
    }
    return $problems.ToArray()
}

$coreReadmeRoot = Join-Path $src 'core'
$preflightProofs = Join-Path $repoRoot 'docs\validation\current'
$rosterPath = Join-Path $repoRoot 'docs\ValidatedTestPackages.md'
$validationIndexPath = Join-Path $repoRoot 'docs\validation\index.md'
$snapshotsRoot = Join-Path $repoRoot 'docs\validation'

# The population of record for the Go release version.props pins -- the file check-roster-format.ps1
# section 2b reads. Composed from the pin rather than spelled with it, so the next hop's release reads
# ITS population or refuses by name for want of one, instead of silently reading this hop's.
$preflightPropsText = [System.IO.File]::ReadAllText($versionProps)
$preflightGoPin = if ($preflightPropsText -match '<GoStdLibVersion>([^<]+)</GoStdLibVersion>') { $Matches[1].Trim() } else { '?' }
$populationPath = Join-Path $repoRoot "docs\phase4\hopA-inputs\recon-lists\population-go$preflightGoPin.txt"

Write-Step "Pre-flight: verifying badges and the release census BEFORE anything is bumped, tagged or frozen"

$census = Get-GoReleaseCensus -CoreDir $coreReadmeRoot -CurrentProofs $preflightProofs -RosterPath $rosterPath `
                              -IndexPath $validationIndexPath -PopulationPath $populationPath

# Printed UNCONDITIONALLY, pass or fail: the four numbers are the cheapest statement of where the
# campaign stands, and a reader who only ever sees them when they disagree cannot tell a healthy run
# from one whose census never executed. The line's shape is unchanged; the identities that account
# for the four numbers, by name, follow it.
Write-Step ("Release census: {0} green badge(s) / {1} proof page(s) / {2} roster row(s) / {3} .tests.csproj" -f `
            $census.GreenIds.Count, $census.PageIds.Count, $census.RosterIds.Count, $census.TestIds.Count)

$censusProblems = New-Object System.Collections.Generic.List[string]
foreach ($problem in $census.InputProblems) { $censusProblems.Add($problem) }

# Every set arithmetic below is ordinal and over dot-ids (see Get-GoReleaseCensus).
function Get-GoOrdinalIntersection([string[]]$Of, [string[]]$With) {
    $withSet = New-Object 'System.Collections.Generic.HashSet[string]' ([System.StringComparer]::Ordinal)
    foreach ($p in @($With)) { [void]$withSet.Add($p) }
    return @(@($Of) | Where-Object { $withSet.Contains($_) } | Sort-Object -Unique)
}

$bankedSet = New-Object 'System.Collections.Generic.HashSet[string]' ([System.StringComparer]::Ordinal)
foreach ($p in $census.RosterIds) { [void]$bankedSet.Add($p) }
$excludedSet = New-Object 'System.Collections.Generic.HashSet[string]' ([System.StringComparer]::Ordinal)
foreach ($p in $census.ExcludedIds) { [void]$excludedSet.Add($p) }

# 1. tests.csproj = banked rows + exclusion rows keeping artifacts + rowless candidates keeping them.
#    Membership is check-roster-format 2b2's function; the two named classes are what it printed.
$projectViolations = @(Get-TestProjectIdentityViolations -Projects $census.TestIds -Banked $census.RosterIds `
                                                         -Excluded $census.ExcludedIds -Population $census.PopulationIds)
$excludedWithArtifacts = Get-GoOrdinalIntersection -Of $census.ExcludedIds -With $census.TestIds
$candidatesWithArtifacts = Get-GoOrdinalIntersection -With $census.TestIds -Of @($census.PopulationIds |
    Where-Object { -not $bankedSet.Contains($_) -and -not $excludedSet.Contains($_) })
foreach ($problem in $projectViolations) { $censusProblems.Add($problem) }
$projectSum = $census.RosterIds.Count + $excludedWithArtifacts.Count + $candidatesWithArtifacts.Count
if (-not $projectViolations.Count -and $census.TestIds.Count -ne $projectSum) {
    $censusProblems.Add("the tests.csproj identity does not add up: $($census.TestIds.Count) project(s) against $($census.RosterIds.Count) banked + $($excludedWithArtifacts.Count) exclusion + $($candidatesWithArtifacts.Count) candidate = $projectSum (a repeated roster row?)")
}

# 2. proof pages = rows by name + relocation anchors by a row's [proof] link + exclusion rows.
$pageIdentity = Get-ProofPageIdentity -Pages $census.PageIds -Banked $census.RosterIds `
                                      -Linked $census.LinkedIds -Excluded $census.ExcludedIds
foreach ($problem in $pageIdentity.Violations) { $censusProblems.Add($problem) }
if (-not $pageIdentity.Violations.Count -and $pageIdentity.ByName.Count -ne $census.RosterIds.Count) {
    $censusProblems.Add("the proof-page identity does not add up: $($pageIdentity.ByName.Count) page(s) by name against $($census.RosterIds.Count) roster row(s) (a repeated roster row?)")
}

# 3. green badges = banked rows - banked rows with no README. Membership, both directions, is
#    check-roster-format 2b3's function; the subtracted class is named here, as 2b3 names it.
$badgeViolations = @(Get-BadgeRosterViolations -WithReadme $census.ReadmeIds -Validated $census.GreenIds `
                                               -Banked $census.RosterIds -Excluded $census.ExcludedIds)
$readmeSet = New-Object 'System.Collections.Generic.HashSet[string]' ([System.StringComparer]::Ordinal)
foreach ($p in $census.ReadmeIds) { [void]$readmeSet.Add($p) }
$rowsWithoutReadme = @($census.RosterIds | Where-Object { -not $readmeSet.Contains($_) } | Sort-Object -Unique)
foreach ($problem in $badgeViolations) { $censusProblems.Add($problem) }
if (-not $badgeViolations.Count -and $census.GreenIds.Count -ne ($census.RosterIds.Count - $rowsWithoutReadme.Count)) {
    $censusProblems.Add("the green-badge identity does not add up: $($census.GreenIds.Count) green badge(s) against $($census.RosterIds.Count) banked - $($rowsWithoutReadme.Count) README-less = $($census.RosterIds.Count - $rowsWithoutReadme.Count)")
}

# 4. index rows = banked rows, by name in both directions. Skipped only when the index could not be
#    read at all, which is already a named problem above.
$indexViolations = New-Object System.Collections.Generic.List[string]
if ($census.IndexIds.Count) {
    $indexSet = New-Object 'System.Collections.Generic.HashSet[string]' ([System.StringComparer]::Ordinal)
    foreach ($p in $census.IndexIds) { [void]$indexSet.Add($p) }
    foreach ($p in @($census.RosterIds | Sort-Object -Unique)) {
        if (-not $indexSet.Contains($p)) { $indexViolations.Add("banked row has no docs\validation\index.md row: $p") }
    }
    foreach ($p in @($census.IndexIds | Sort-Object -Unique)) {
        if (-not $bankedSet.Contains($p)) { $indexViolations.Add("docs\validation\index.md row names no banked row: $p") }
    }
    if (-not $indexViolations.Count -and $census.IndexIds.Count -ne $census.RosterIds.Count) {
        $indexViolations.Add("the index-row identity does not add up: $($census.IndexIds.Count) index row(s) against $($census.RosterIds.Count) roster row(s) (a repeated row?)")
    }
}
foreach ($problem in $indexViolations) { $censusProblems.Add($problem) }

# Printed UNCONDITIONALLY, like the census line: each identity with its terms, and every class other
# than the banked rows named in full, so the arithmetic is re-addable from the output alone.
Write-Step "Named identities (docs\GoCorpusMigration.md, THE RELEASE CENSUS, CORRECTED):"
Write-Host ("      {0} .tests.csproj = {1} banked row(s) + {2} exclusion row(s) keeping artifacts + {3} rowless candidate(s) keeping artifacts" -f `
            $census.TestIds.Count, $census.RosterIds.Count, $excludedWithArtifacts.Count, $candidatesWithArtifacts.Count)
Write-Host ("          exclusion rows keeping artifacts : {0}" -f ($excludedWithArtifacts -join ', '))
Write-Host ("          rowless candidates               : {0}" -f ($candidatesWithArtifacts -join ', '))
Write-Host ("      {0} proof page(s) = {1} roster row(s) by name + {2} relocation anchor(s) by a row's [proof] link + {3} exclusion row(s) by exclusion" -f `
            $census.PageIds.Count, $pageIdentity.ByName.Count, $pageIdentity.ByLink.Count, $pageIdentity.ByExclusion.Count)
Write-Host ("          relocation anchors               : {0}" -f ($pageIdentity.ByLink -join ', '))
Write-Host ("          exclusion pages                  : {0}" -f ($pageIdentity.ByExclusion -join ', '))
Write-Host ("      {0} green badge(s) = {1} banked row(s) - {2} banked row(s) with no README" -f `
            $census.GreenIds.Count, $census.RosterIds.Count, $rowsWithoutReadme.Count)
Write-Host ("          banked rows with no README       : {0}" -f ($rowsWithoutReadme -join ', '))
Write-Host ("      {0} index row(s) = {1} banked row(s)" -f $census.IndexIds.Count, $census.RosterIds.Count)

# THE NEXT RELEASE, existence plus monotonicity (see Get-GoNextReleaseVerification). A tag list that
# cannot be read is a named problem, never an empty list: an empty list would pass the tag half.
$releaseTagNames = @()
$tagReadProblem = $null
if (-not (Get-Command git -ErrorAction SilentlyContinue)) {
    $tagReadProblem = "git is not available, so the nuget-* tags cannot be read and the next release's existence and monotonicity cannot be checked against them"
}
else {
    try {
        $releaseTagNames = @(& git -C $repoRoot tag --list 'nuget-*')
        if ($LASTEXITCODE -ne 0) { $tagReadProblem = "git tag --list 'nuget-*' exited $LASTEXITCODE, so the tag half of the next release's check read nothing" }
    }
    catch { $tagReadProblem = "git tag --list 'nuget-*' failed ($($_.Exception.Message)), so the tag half of the next release's check read nothing" }
}

# ...and origin's, beside them (see Get-GoNextReleaseVerification). An unreadable origin is NOT a
# problem -- a release morning off the network still has its local tags -- but it is printed by name
# below, every time, so a local-only comparison can never pass for the union.
$originTags = if ($tagReadProblem) { [pscustomobject]@{ Names = @(); Problem = 'not read (the local tag read failed first)' } }
              else { Get-GoOriginReleaseTags -RepoRoot $repoRoot }

$nextRelease = Get-GoNextReleaseVerification -VersionPropsText $preflightPropsText -SnapshotsDir $snapshotsRoot `
                                             -Tags $releaseTagNames -RemoteTags $originTags.Names
if ($tagReadProblem) { $censusProblems.Add($tagReadProblem) }
foreach ($problem in $nextRelease.Problems) { $censusProblems.Add($problem) }

$nextVerdict = if ($nextRelease.Problems.Count -or $tagReadProblem) { 'REFUSED' } else { 'unrecorded, and strictly newer than every recorded release' }
Write-Step ("Next release: {0} (base {1} + counter {2} + 1): {3} -- newest recorded {4} ({5}), across {6} snapshot(s), {7} local release tag(s) and {8} on origin only, compared numerically per component" -f `
            $nextRelease.Next, $nextRelease.Base, $nextRelease.Counter, $nextVerdict, $nextRelease.Newest, $nextRelease.NewestFrom,
            $nextRelease.Snapshots, $nextRelease.ReleaseTags, $nextRelease.OriginOnlyTags)
if ($originTags.Problem) {
    Write-Host ("      origin tags: UNREADABLE -- {0}; the comparison ran on the LOCAL tags only" -f $originTags.Problem) -ForegroundColor Yellow
}
else {
    $originOnlyNames = @($originTags.Names | Where-Object { $releaseTagNames -cnotcontains $_ })
    Write-Host ("      origin tags: {0} nuget-* tag(s) read by git ls-remote; {1} not in the local tags{2}" -f `
                $originTags.Names.Count, $originOnlyNames.Count, $(if ($originOnlyNames.Count) { ': ' + ($originOnlyNames -join ', ') } else { '' }))
}
if ($nextRelease.Excluded.Count) {
    Write-Host ("      excluded by name, not a release: {0}" -f ($nextRelease.Excluded -join ', '))
}

# THE FROZEN ROSTER'S LINKS, asked here rather than discovered after the freeze (see
# Get-GoFrozenRosterLinkProblems). The transform is run on the living roster with the would-be
# version and its text discarded: only its link accounting is read. The tracked-path list is kept
# for the freeze's own backstop, so both read one list. A git read that fails is a named problem,
# and an empty list resolves nothing, so the check fails closed.
$trackedRepoPaths = @()
try {
    $trackedRepoPaths = @(& git -C $repoRoot -c core.quotepath=off ls-files)
    if ($LASTEXITCODE -ne 0) { $censusProblems.Add("git ls-files exited $LASTEXITCODE, so the frozen roster's links cannot be resolved against the tracked tree") }
}
catch { $censusProblems.Add("git ls-files failed ($($_.Exception.Message)), so the frozen roster's links cannot be resolved against the tracked tree") }

$auditVersion = if ($nextRelease.Next) { $nextRelease.Next } else { 'preflight' }
$rosterLinkAudit = ConvertTo-FrozenRosterText -RosterText ([System.IO.File]::ReadAllText($rosterPath)) -Version $auditVersion -Commit 'preflight'
$rosterLinkProblems = @(Get-GoFrozenRosterLinkProblems -Transform $rosterLinkAudit -TrackedPaths $trackedRepoPaths)
foreach ($problem in $rosterLinkProblems) { $censusProblems.Add($problem) }
$rosterLinkVerdict = if ($rosterLinkProblems.Count) { "$($rosterLinkProblems.Count) would dangle" } else { 'every one resolves to a tracked path' }
Write-Step ("Frozen-roster links: {0} relative link(s) relocated ({1} distinct): {2}; {3} proof link(s) onto sibling pages" -f `
            @($rosterLinkAudit.Relocated).Count, @($rosterLinkAudit.Relocated | Sort-Object -Unique).Count, $rosterLinkVerdict, $rosterLinkAudit.ProofLinks)

# NOT $green/$source. PowerShell resolves variable names case-INSENSITIVELY, so `$source = <object>`
# binds the script's own [string]$Source PARAMETER -- which keeps its type constraint and COERCES the
# result to "@{Verified=305; Problems=System.String[]}". Both members then read $null (so the count
# printed blank and @($null) contributed one nameless "problem"), and -- the damage that matters --
# $Source is the nuget PUSH URL, so a release run would have pushed at a garbage source. Measured and
# fixed here; the names below cannot collide with any parameter of this script.
$greenBadge = Get-GoGreenBadgeVerification -CoreDir $coreReadmeRoot -ProofRoot $preflightProofs
$sourceBadge = Get-GoSourceBadgeVerification -CoreDir $coreReadmeRoot

Write-Step "Pre-flight verified $($greenBadge.Verified) green badge(s) against docs\validation\current and $($sourceBadge.Verified) C# Source badge(s) structurally"

$preflightProblems = @($greenBadge.Problems) + @($sourceBadge.Problems) + @($censusProblems.ToArray())

if ($preflightProblems.Count) {
    Write-Host ''
    Write-Host "PRE-FLIGHT FAILED -- $($preflightProblems.Count) problem(s). Nothing was bumped, tagged or frozen." -ForegroundColor Red
    foreach ($problem in $preflightProblems) { Write-Host "    $problem" -ForegroundColor Red }
    Write-Host ''
    throw ("Release pre-flight found $($preflightProblems.Count) problem(s) -- census was " +
           "$($census.GreenIds.Count) green badge(s) / $($census.PageIds.Count) proof page(s) / " +
           "$($census.RosterIds.Count) roster row(s) / $($census.TestIds.Count) .tests.csproj. " +
           "A badge whose arithmetic disagrees with its proof page is fixed by RECONVERTING the " +
           "package (the badge is generated from the proof page's totals line by " +
           "src\go2cs\readmeValidationBadge.go), never by hand-editing the README. An item no " +
           "named identity accounts for is fixed by accounting for it BY NAME -- a roster row, an " +
           "exclusion row or a row's [proof] link -- never by deleting a page or inventing a badge.")
}

if ($VerifyOnly) {
    Write-Step "Pre-flight clean. -VerifyOnly: nothing was bumped, tagged, frozen, packed or pushed."
    exit 0
}

# --- Embedded stdlib-metadata staleness gate ------------------------------------------------------
# go2cs/stdlib-metadata.txt is the converter's embedded record of what every converted stdlib package
# EXPORTS across assemblies (its GoTypeAlias aliases and GoImplement records). Under -recurse=nuget the
# converter reads it INSTEAD of the package_info.cs it can no longer find on disk, so it must describe
# the very tree this script is about to pack. If the converted stdlib's exported surface changed and the
# asset was not regenerated, the published packages and the converter disagree -- and the damage lands
# in END USERS' builds (missing `global using` aliases, or a duplicate/absent interface adapter), not
# here. Verify BEFORE anything is built, so the run fails at second zero rather than after a full
# Release build. Regenerate with `go generate .` from src\go2cs and commit the result.
# MSBuild worker nodes PERSIST after a build and are re-entered by the next one. This script runs
# back-to-back solution builds (one per RID, different $(GoTargetOS) each), which is exactly the
# shape the repo's standing rule prescribes this flag for -- and the 1.23.1.7 release's pack race
# (gen's bin empty at pack time after a SUCCEEDED build, 3/3 in the full script, 0/3 isolated,
# 0/2 in binlog-armed repro) fits stale node state around the clean/copy file ops better than
# anything else measured. The healthy binlog shows 16 nodes; fresh nodes per pass cost seconds.
# The assert-and-repair below STAYS: if it never fires again after this flag, node reuse is
# confirmed by alternation at zero repro cost (ledger #5, closed measured-and-hardened).
$env:MSBUILDDISABLENODEREUSE = '1'

$converterDir = Join-Path $src 'go2cs'

if (-not (Get-Command go -ErrorAction SilentlyContinue)) {
    throw "The Go toolchain is required to verify go2cs\stdlib-metadata.txt is in sync with src\core before publishing."
}

Write-Step "Verifying stdlib-metadata.txt matches src\core"
Push-Location $converterDir

try {
    & go test -count=1 -run TestStdLibMetadataInSync . | Out-Host

    if ($LASTEXITCODE -ne 0) {
        throw "STALE EMBEDDED METADATA: go2cs\stdlib-metadata.txt does not match src\core. " +
              "Run ``go generate .`` from src\go2cs, commit the regenerated asset, then re-run this script. " +
              "Publishing now would ship packages the converter's -recurse=nuget mode describes incorrectly."
    }
}
finally {
    Pop-Location
}

# --- Build-number bump (raw-text edit preserves comments/formatting) -----------------------------
# Each publish should be a NEW version, so the build number is bumped by default when -Push is given
# and left alone for a pack-only run. An explicit -BumpBuild / -BumpBuild:$false always wins -- use
# -BumpBuild:$false to re-push the CURRENT version (e.g. finishing a partially-failed push).
if ($PSBoundParameters.ContainsKey('BumpBuild')) { $doBump = [bool]$BumpBuild } else { $doBump = [bool]$Push }

$propsText = [System.IO.File]::ReadAllText($versionProps)
if ($propsText -notmatch '<GoBuildNumber>(\d+)</GoBuildNumber>') { throw "GoBuildNumber not found in $versionProps" }
$build = [int]$Matches[1]

$bumped = $false

if ($doBump) {
    $newBuild = $build + 1
    if ($PSCmdlet.ShouldProcess($versionProps, "bump GoBuildNumber $build -> $newBuild")) {
        $propsText = $propsText -replace '<GoBuildNumber>\d+</GoBuildNumber>', "<GoBuildNumber>$newBuild</GoBuildNumber>"
        [System.IO.File]::WriteAllText($versionProps, $propsText, $utf8NoBom)
        Write-Step "Bumped GoBuildNumber $build -> $newBuild (commit version.props to record the release)"
        $build = $newBuild
        $bumped = $true
    }
}

if ($propsText -match '<GoStdLibVersion>([^<]+)</GoStdLibVersion>') { $baseVersion = $Matches[1] } else { $baseVersion = '?' }
$fullVersion = "$baseVersion.$build"
Write-Step "Package version: $fullVersion   (solution: go2cs-stdlib.slnx)"

# The version the PACK writes. Unchanged unless a local rehearsal passed -VersionSuffix, which is refused on
# every release path above, so on a release $packVersion IS $fullVersion and nothing below moves.
$packVersion = if ($VersionSuffix) { "$fullVersion-$VersionSuffix" } else { $fullVersion }
if ($VersionSuffix) { Write-Step "LOCAL REHEARSAL: packing as $packVersion (pack-only; nothing is bumped, tagged or pushed)" }

# The version a BUMPING run would publish. Only meaningful when this run did not bump: $build is then
# still the last-published number, so +1 names the next release. After a bump $build already IS that
# number, so the value would be one release too far ahead -- it is set to $null rather than computed
# wrongly, and every consumer below sits on a -not $doBump path where that cannot happen.
if ($doBump) { $wouldBeVersion = $null } else { $wouldBeVersion = "$baseVersion.$($build + 1)" }

# A pack-only INSPECTION run -- the dry run docs\phase4\MILESTONE-75pct-prep.md section 3.3 recommends, and the
# only shape in which the dry-run affordances below engage. Deliberately NARROWER than -not $doBump:
#
#   .\push-nuget.ps1                     $dryRun = $true    the section 3.3 dry run
#   .\push-nuget.ps1 -BumpBuild:$false   $dryRun = $true    same shape, bump explicitly declined
#   .\push-nuget.ps1 -Push               $dryRun = $false   the release
#   .\push-nuget.ps1 -Push -BumpBuild:$false
#                                        $dryRun = $false   a RELEASE (re-push of the current version
#                                                           finishing a partially-failed publish); it
#                                                           does not bump, but it publishes, so it must
#                                                           freeze and verify against the REAL tree
#   .\push-nuget.ps1 -BumpBuild          $dryRun = $false   prepare-the-release-commit; it bumps, so it
#                                                           writes the real write-once snapshot
#
# Excluding -Push whatever its bump setting is what keeps the release path untouched by everything
# below: with -Push, $dryRun is $false by construction and every branch guarded on it is dead code.
#
# CODE-PATH PROOF that -Push behaves exactly as it did before this affordance existed. The dry-run fix
# touches six executable sites, and with -Push every one of them resolves to its pre-existing form:
#
#   1. $wouldBeVersion  -- a new variable. $null when $doBump; read ONLY inside `if ($dryRun)` branches,
#                          so on any -Push run it is either $null or computed-and-never-read.
#   2. $dryRun          -- $false whenever $Push, regardless of $doBump. This is the keystone: it makes
#                          sites 3-6 unreachable on every release path.
#   3. the tag skip message, 4. the "Froze N" message, 6. the "Verified N" message
#                       -- each `if ($dryRun) { new } else { original }`; the else branch reproduces the
#                          previous string literal verbatim, so console output is byte-identical too.
#   5. the snapshot redirect and the ShouldProcess short-circuit
#                       -- `if ($dryRun)` is not taken, so $versionProofs keeps docs\validation\<version>,
#                          and `$dryRun -or $PSCmdlet.ShouldProcess(...)` evaluates ShouldProcess exactly
#                          as the bare call did ($false -or X is X, including its -WhatIf side effect).
#   + the try/finally  -- adds no catch, so exceptions propagate unchanged, and the finally is a no-op
#                          because $dryRunProofRoot is $null on every release path. No `exit` is enclosed.
#
# Net effect with -Push: two variable assignments that nothing reads. Nothing else in the script's
# behaviour, output or side effects moves.
$dryRun = (-not $Push) -and (-not $doBump)

# ($repoRoot is computed with the pre-flight block above, which needs it.)

# --- Release tag ----------------------------------------------------------------------------------
# The tag is minted HERE, before anything is packed, because every package's README BAKES A LINK TO
# IT: the C# Source badge points at github.com/ritchiecarroll/go2cs/tree/nuget-<version>/src/core/<pkg>
# so a reader lands on the exact C# that shipped in the package they hold. Minting the tag after the
# push -- where it used to live, as a Phase-3 instruction -- meant every README on nuget.org linked a
# 404 for however long it took to get around to tagging. Created before the first .nupkg is built,
# the link resolves the moment the package is published.
#
# The tag names the tree this release was built FROM. HEAD here is the last commit before the release
# commit, and the two differ only by version.props, the proof snapshot and the retargeted README
# links -- no converted C# moves between them -- so the tree the badge reaches IS the C# in the
# package. (Committing the release before running this flow would collapse the two; nothing needs it.)
#
# It runs BEFORE the write-once proof snapshot deliberately: a signing failure then costs nothing,
# where the reverse order would leave a frozen directory behind for a release that never happened.
#
# Gated on the bump, because the bump is what makes a run a release -- a pack-only inspection run
# must not mint a release tag. Idempotent by check-then-skip, loudly, so a re-run after a failed
# later phase carries on instead of dying on "tag already exists".
$releaseTag = "nuget-$fullVersion"

if (-not $doBump) {
    # $releaseTag is composed from the UN-bumped $fullVersion, so on a non-bumping run it names the tag
    # of the release already published -- not the one "the run that bumps" would mint. Naming it here
    # misinforms at exactly the moment someone is checking the version arithmetic. A dry run therefore
    # names the would-be tag instead. The -Push -BumpBuild:$false branch keeps today's wording verbatim:
    # it is a release path, and this fix is scoped to leave every release path byte-identical.
    if ($dryRun) {
        Write-Step "No build-number bump this run -- not tagging (the run that bumps mints nuget-$wouldBeVersion)"
    }
    else {
        Write-Step "No build-number bump this run -- not tagging (the run that bumps mints $releaseTag)"
    }
}
elseif (-not (Get-Command git -ErrorAction SilentlyContinue)) {
    Write-Warning ("git is not available, so release tag $releaseTag was NOT created. Every package README's C# " +
                   "Source badge links it and will 404 until it exists: create it by hand before publishing " +
                   "(git tag -s $releaseTag -m ""NuGet publication $fullVersion"").")
}
elseif (& git -C $repoRoot tag --list $releaseTag) {
    Write-Step "Release tag $releaseTag already exists -- keeping it (re-run of a partially completed release)"
}
elseif ($PSCmdlet.ShouldProcess($releaseTag, 'create signed release tag at HEAD')) {
    # Signed, per repository convention. A failure here is fatal on purpose: publishing 300 packages
    # whose READMEs all link a tag that does not exist is worse than stopping. If GPG is the problem,
    # the agent must be launched via Gpg4win's gpgconf.
    & git -C $repoRoot tag -s $releaseTag -m "NuGet publication $fullVersion"

    if ($LASTEXITCODE -ne 0) {
        throw "Failed to create signed release tag $releaseTag ($LASTEXITCODE). Every package README's C# Source badge links this tag -- resolve the signing failure and re-run before publishing."
    }

    Write-Step "Created signed release tag $releaseTag at $(& git -C $repoRoot rev-parse --short HEAD) (push it with the release commit)"
}

# --- Validation proof snapshot + badge retarget ---------------------------------------------------
# Every validated package's README carries a green badge whose link is VERSION-PINNED, and every
# validated package packs that same page as VALIDATION.md. Both point at docs\validation\<version>\,
# which is written ONCE here, at publication, and never rewritten: the proof shown for go.io 1.23.1.3
# stays forever the proof as of that binary, while docs\validation\current\ keeps moving.
#
# Order matters. The snapshot is taken BEFORE the build so the .csproj Exists() guards see the files
# they are about to pack, and the READMEs are retargeted in the same breath so the badge, the link
# and the packed sheet are the one version being published.
#
# THE SNAPSHOT ALSO FREEZES THE ROSTER PAGE (added 2026-09-07). Until then it froze the per-package
# proofs and nothing around them, so the published site had 204 proof pages from publication day and
# no view of how the campaign stood behind them: the only "as it shipped" roster was the signed tag
# nuget-<version>, a git object that reaches a reader on GitHub and nobody on go2cs.net. The copy is
# a TRANSFORM, not a file copy -- ConvertTo-FrozenRosterText in _roster.ps1 retargets the roster's
# 200-odd proof links onto this snapshot's own sibling pages and relocates the handful that pointed
# out of docs\, so the snapshot reads without walking back into the LIVING directory. It lands
# inside the write-once branch below, so a frozen directory that already exists is still never
# rewritten.
$validationDir = Join-Path $repoRoot 'docs\validation'
$currentProofs = Join-Path $validationDir 'current'
$versionProofs = Join-Path $validationDir $fullVersion
$dryRunProofRoot = $null
$frozenRosterName = 'ValidatedTestPackages.md'

# Hoisted from the README retarget below, which had the only copy: the roster freeze writes a text
# file through the same door and for the same reason, and two definitions of one encoding is the
# thing that drifts. Its rationale is stated at the retarget, where the mojibake it prevents was
# first paid for.
$utf8NoBomText = New-Object System.Text.UTF8Encoding($false)

# WHY A DRY RUN NEEDS ITS OWN SNAPSHOT DIRECTORY (found by the section 3.6 rehearsal, defect D1).
#
# A pack-only run does not bump, so $fullVersion is the LAST-PUBLISHED version and $versionProofs is a
# directory that already exists. The freeze then takes the "keeping it" branch and is never exercised,
# and -- the real damage -- the green-badge verifier below checks TODAY's badges against a snapshot
# frozen at the last release. Every package validated since then links a page that frozen directory
# will never contain, so the verifier throws on the alphabetically first one and the run dies in eight
# seconds having measured nothing. That is not a defect in the tree: it is guaranteed the moment one
# row banks after a release, which is the normal state of this campaign (at the rehearsal: 162 green
# badges against a 126-page snapshot). The documented dry run was simply unrunnable.
#
# The fix mirrors what release morning actually does, without writing to the tree. A dry run freezes
# docs\validation\current into a TEMPORARY directory named for the version a bumping run would publish,
# and the verifier checks the corpus's badges against those pages -- which is a coherent check, because
# current\ is exactly the set the next release will freeze. The freeze branch runs for real (phase 5's
# "Froze N" count is measured, not skipped), and the directory is removed in the finally below.
#
# It is NOT written to docs\validation\<would-be>\ in the tree: that path is write-once and belongs to
# the release that bumps. Pre-creating it would make the release's own precondition check throw.
#
# $fullVersion is deliberately NOT moved to the would-be version. It is the version this run packs, and
# it is what the README badge retarget and verification below compare against; moving it would rewrite
# every README in the tree to advertise a version that was never published, and bake that wrong version
# into the packed READMEs. Only the PROOF-PAGE location moves.
if ($dryRun) {
    $dryRunProofRoot = Join-Path ([System.IO.Path]::GetTempPath()) "go2cs-dryrun-proofs-$PID-$([System.IO.Path]::GetRandomFileName())"
    $versionProofs = Join-Path $dryRunProofRoot $wouldBeVersion
    Write-Step "Dry run -- freezing the would-be $wouldBeVersion snapshot to a temporary directory (the tree is not written)"
}

try {

if (-not (Test-Path $currentProofs)) {
    Write-Warning "No validation proof pages at $currentProofs -- skipping the snapshot and badge retarget."
} else {
    if (Test-Path $versionProofs) {
        # A frozen directory is write-once. Re-publishing the CURRENT version (-BumpBuild:$false, e.g.
        # finishing a partially-failed push -- and any -WhatIf run, which declines the bump)
        # legitimately finds its own snapshot already there; a version that was ACTUALLY bumped a
        # moment ago finding one means the version counter and the docs tree disagree.
        if ($bumped) { throw "Validation snapshot $versionProofs already exists for the newly bumped version $fullVersion. Frozen snapshots are write-once -- reconcile src\version.props with docs\validation before publishing." }
        Write-Step "Validation snapshot $fullVersion already exists (write-once) -- keeping it"
    }
    # ShouldProcess gates writes the USER's tree keeps; a dry run's snapshot is a temporary directory
    # this script deletes itself, so there is nothing to approve or to decline. Short-circuiting on
    # $dryRun therefore also lets a pack-only -WhatIf reach the phases past this block instead of
    # declining the freeze and then failing verification against a directory it just refused to fill.
    # On every release path $dryRun is $false and ShouldProcess is consulted exactly as before.
    elseif ($dryRun -or $PSCmdlet.ShouldProcess($versionProofs, "snapshot docs\validation\current")) {
        New-Item -ItemType Directory -Force $versionProofs | Out-Null
        Copy-Item (Join-Path $currentProofs '*.md') $versionProofs -Force
        # Excludes the frozen roster by NAME rather than by counting before it is written. The
        # phase-5 invariant this number carries is "one page per banked package"; an order-dependent
        # count would read 205 the day somebody moves the roster freeze two lines up, and the
        # invariant would then be wrong in a comment that still said it was right.
        $frozenCount = @(Get-ChildItem $versionProofs -Filter *.md -File |
                         Where-Object { $_.Name -ne $frozenRosterName }).Count

        # --- each copied page, retargeted onto the release it now belongs to -----------------------
        # The copy above is verbatim, and a verbatim copy of a LIVING page is not a frozen one: its
        # roster link walks up to docs\ValidatedTestPackages.md and its source link names tree/master,
        # so a page whose whole job is to say "this is the evidence for the binary we shipped" reads
        # its row out of a roster that has banked packages since, against source from a branch that
        # has moved on. Both replacements have a target that already exists at this point in the run:
        # the snapshot's own roster copy is written a few lines below, and the signed tag was minted
        # before the build. The rule and its reasoning are Update-FrozenProofPage in _roster.ps1 --
        # one definition, shared with the one-off that retargeted the 1.23.12.3 pages already frozen,
        # so a committed snapshot cannot drift from the code that produces its successors.
        #
        # ONLY the copied proof pages. The frozen ROSTER is deliberately excluded and would be
        # CORRUPTED by this transform: its single '](../../ValidatedTestPackages.md)' is the note's
        # pointer at the LIVING roster -- relocated on purpose by ConvertTo-FrozenRosterText -- and
        # retargeting it would leave the page linking itself while calling itself the living one. The
        # roster is not in $versionProofs yet either way; the name filter says so rather than relying
        # on that ordering, which is the same reason $frozenCount above excludes it by name.
        #
        # The THIRD substitution rides the same pass, and this loop is unchanged for it. A page whose
        # package discloses also points once at that package's hand-owned go2cs_test_disclosures.json,
        # spelled blob/master rather than tree/master, so the source substitution cannot see it and it
        # would publish naming a branch that keeps moving -- the same defect, one link over, and the
        # one the residual of d4c88e765 measured at 36 of these 204 pages. Update-FrozenProofPage
        # CALLS Update-FrozenProofPageDisclosureLink and returns its count in the same object, so the
        # page is still retargeted by one call and there is no second pass. What that count is NOT
        # folded into is either assertion below. See the third aggregate after them.
        $retargetedRoster = 0
        $retargetedSource = 0
        $retargetedDisclosure = 0
        $unretargeted = New-Object System.Collections.Generic.List[string]

        foreach ($page in @(Get-ChildItem $versionProofs -Filter *.md -File |
                            Where-Object { $_.Name -ne $frozenRosterName })) {
            try {
                $retargeted = Update-FrozenProofPage -Path $page.FullName -Version $fullVersion
                $retargetedRoster += $retargeted.RosterLinks
                $retargetedSource += $retargeted.SourceLinks
                $retargetedDisclosure += $retargeted.DisclosureLinks
            }
            catch { $unretargeted.Add("$($page.Name): $($_.Exception.Message)") }
        }

        # The counts are per-PAGE invariants, and they are checked here as well as inside the
        # transform because the two answer different questions: the throw in Update-FrozenProofPage
        # names a page whose template drifted, while these name a disagreement between the set that
        # was copied and the set that was retargeted -- which no per-page check can see. Every page
        # the converter generates carries exactly one ROSTER link and one SOURCE link, so those two
        # sums must equal the page count; anything else is published-site breakage nobody is watching
        # for. The DISCLOSURE sum is deliberately NOT in this assertion: see below.
        if ($unretargeted.Count) {
            throw ("The $fullVersion snapshot has $($unretargeted.Count) proof page(s) whose frozen links could " +
                   "not be retargeted -- they would publish still pointing at the living roster, tree/master or " +
                   "blob/master:`n    " +
                   ($unretargeted -join "`n    "))
        }

        if ($retargetedRoster -ne $frozenCount -or $retargetedSource -ne $frozenCount) {
            throw ("The $fullVersion snapshot retargeted $retargetedRoster roster link(s) and $retargetedSource " +
                   "source link(s) across $frozenCount proof page(s); each page carries exactly one of each, so " +
                   "both counts must equal the page count. Reconcile the proof-page template with " +
                   "Update-FrozenProofPage before publishing.")
        }

        # The third aggregate is REPORTED, not asserted against the page count, and the reason is the
        # count's shape rather than any weakness of nerve. A page carries a disclosure pointer only if
        # its package has a manifest to name -- 36 of 204 at 1.23.12.3, with 168 carrying none -- so
        # this sum is the size of a SUBSET nothing structural predicts: it moves the day a package
        # banks a first disclosure or retires its last, with no template change to notice. Comparing
        # it to $frozenCount would fail every release; comparing it to a number written here would go
        # stale on exactly that day. What IS asserted lives per page in
        # Update-FrozenProofPageDisclosureLink -- more than one throws, and no blob/master may survive
        # the pin -- and the aggregate that matters is measurable after the fact on the snapshot
        # itself: pages carrying blob/master before equals pages carrying blob/nuget-<version> after,
        # and pages still carrying blob/master after is zero.
        Write-Step ("Retargeted the frozen proof pages -- $retargetedRoster roster link(s) onto this snapshot's " +
                    "own $frozenRosterName, $retargetedSource source link(s) onto tag $releaseTag, " +
                    "$retargetedDisclosure disclosure-manifest pointer(s) pinned across the pages that carry one " +
                    "(optional per page, so this one is not the page count)")

        # --- the roster page, transformed into this snapshot's own copy of itself ------------------
        # Which commit the note names: the TAG's, when the tag exists and this is a real release --
        # the tag is the authority on the tree a release was built from, and a re-run finishing a
        # partially-failed publish must not stamp a HEAD that has since moved. On the dry-run path
        # $releaseTag is composed from the UN-bumped version, so it names the PREVIOUS release; HEAD
        # is the honest answer there, being what the run that bumps would tag.
        #
        # ⚠ try/catch, not $LASTEXITCODE alone. This script runs at $ErrorActionPreference = 'Stop'
        # (line 110), and under 'Stop' a native command whose stderr is REDIRECTED raises a
        # terminating NativeCommandError -- so a bare `& git ... 2>$null` followed by an exit-code
        # test is a fallback that can never be reached: git's "fatal: ..." kills the release before
        # the test runs. Measured on the freeze dry run, where the first form of this block threw on
        # a rev-parse that was SUPPOSED to fall through to the warn-and-skip path below.
        $rosterSource = Join-Path $repoRoot 'docs\ValidatedTestPackages.md'
        $frozenCommit = ''
        if (Get-Command git -ErrorAction SilentlyContinue) {
            if (-not $dryRun) {
                try {
                    $sha = & git -C $repoRoot rev-parse --short "$releaseTag^{commit}" 2>$null
                    if ($LASTEXITCODE -eq 0 -and $sha) { $frozenCommit = [string]$sha }
                }
                catch { $frozenCommit = '' }
            }
            if (-not $frozenCommit) {
                try {
                    $sha = & git -C $repoRoot rev-parse --short HEAD 2>$null
                    if ($LASTEXITCODE -eq 0 -and $sha) { $frozenCommit = [string]$sha }
                }
                catch { $frozenCommit = '' }
            }
        }

        # Both misses WARN and skip rather than throw: freezing the proofs is what this block did
        # before the roster joined it, and no release path that works today starts failing because
        # the addition could not name its commit. A snapshot without the roster is the pre-2026-09-07
        # shape, which the fifth-number check below reports rather than treats as a defect.
        if (-not (Test-Path $rosterSource)) {
            Write-Warning "No roster page at $rosterSource -- the $fullVersion snapshot will carry proof pages only."
        }
        elseif (-not $frozenCommit) {
            Write-Warning ("Could not resolve the commit this release is built from (git unavailable or " +
                           "rev-parse failed), so the $fullVersion snapshot will carry proof pages only. " +
                           "The frozen roster's whole value is naming that commit; a snapshot that cannot " +
                           "is not written.")
        }
        else {
            $frozenRoster = ConvertTo-FrozenRosterText `
                -RosterText ([System.IO.File]::ReadAllText($rosterSource)) `
                -Version $fullVersion -Commit $frozenCommit

            # The audit arm, REFUSING before the roster is written (it WARNED after writing it until
            # 2026-09-24, and three links would have published dangling in 1.24.13.1). The pre-flight
            # already ran this same check against the same roster and tracked-path list, so reaching a
            # throw here means the tree moved between the two; it is a backstop, not the gate.
            $frozenLinkProblems = @(Get-GoFrozenRosterLinkProblems -Transform $frozenRoster -TrackedPaths $trackedRepoPaths)
            if ($frozenLinkProblems.Count) {
                throw ("The frozen $fullVersion roster would carry $($frozenLinkProblems.Count) relative link(s) that " +
                       "dangle in docs\validation\$($fullVersion):`n    " + ($frozenLinkProblems -join "`n    "))
            }

            [System.IO.File]::WriteAllText((Join-Path $versionProofs $frozenRosterName), $frozenRoster.Text, $utf8NoBomText)

            # --- and that roster's package column, pinned onto the tag ------------------------------
            # ConvertTo-FrozenRosterText retargets the roster's links as a DOCUMENT -- proof links onto
            # the sibling pages, out-of-docs links onto the deeper path -- and leaves the PACKAGE COLUMN
            # naming tree/master, one link per row. That is the same defect the pages beside it were
            # retargeted out of a few lines above: a frozen roster whose package column names a moving
            # branch is not frozen either. The target already exists here too -- $releaseTag was minted
            # before the build -- and the rule is Update-FrozenRosterSourceLinks in _roster.ps1.
            #
            # A SIBLING call rather than a third substitution inside the transform above, for the reason
            # that function's own header states: it turns a LIVING roster into a frozen one and cannot be
            # re-run on a roster it has already transformed, while the one-off that pinned the already
            # frozen 1.23.12.3 roster had to run this ALONE on a file -- the same shape, and the same
            # one-definition-two-callers reason, as Update-FrozenProofPage.
            #
            # It runs INSIDE the branch that wrote the roster. The two arms above warn and skip when
            # there is no roster source or no commit to name, and a snapshot that carries no roster has
            # no package column to pin; calling it out here would throw on a path that is deliberately
            # the pre-2026-09-07 shape rather than a defect.
            #
            # ONE assertion, inside the function, unlike the per-page/aggregate pair above. That pair
            # answers two questions -- a page whose template drifted, and the set copied against the set
            # retargeted -- and the second question does not exist for a single file: the function counts
            # the links and the rows out of the same roster, which is the whole of what can be asked.
            $pinnedRoster = Update-FrozenRosterSourceLinks `
                -Path (Join-Path $versionProofs $frozenRosterName) -Version $fullVersion

            # --- and the roster's one PROSE pointer at a disclosure manifest -------------------------
            # The package column is not the roster's only absolute link at a moving branch. Its prose
            # points once at a package's hand-owned go2cs_test_disclosures.json, spelled blob/master
            # rather than tree/master, so the substitution above cannot see it and it would publish
            # naming a branch that keeps moving -- the same defect, one link over.
            #
            # A THIRD call, not a second phase inside the one above, because that function's count IS
            # the roster's row count: a prose pointer is not a package-column link, so folding it in
            # would make the substitutions 205 against 204 rows or force a second count shape into the
            # one function whose count describes itself. Its own count is a FIXED one (a frozen roster
            # carries exactly one such pointer, which is a census of the living roster rather than a
            # derivation) and it is asserted in both directions inside Update-FrozenRosterDisclosureLink.
            $pinnedDisclosure = Update-FrozenRosterDisclosureLink `
                -Path (Join-Path $versionProofs $frozenRosterName) -Version $fullVersion

            Write-Step ("Froze the roster page at $frozenRosterName -- $($frozenRoster.ProofLinks) proof link(s) " +
                        "retargeted onto this snapshot, $($frozenRoster.Relocated.Count) link(s) relocated, " +
                        "$($pinnedRoster.SourceLinks) package-column source link(s) pinned onto tag $releaseTag " +
                        "across $($pinnedRoster.Rows) row(s), $($pinnedDisclosure.DisclosureLinks) disclosure-manifest " +
                        "pointer(s) pinned, commit $frozenCommit")
        }
        # The count is EVERY page under docs\validation\current -- rows' own pages, relocation anchors
        # and exclusion pages alike (COORD ruling RN-6) -- so no green badge, no roster [proof] link and
        # no index row can link a page this snapshot lacks, the way 21 green badges link pages absent
        # from 1.23.12.3. It is NOT the roster's row count and never was by design: the fifth-number
        # check below holds the named identity that relates the two. A dry run names the would-be
        # version and the temporary location so the line cannot be misread as a write into the tree;
        # the release wording is unchanged.
        if ($dryRun) {
            Write-Step "Froze $frozenCount validation proof page(s) for would-be version $wouldBeVersion at $versionProofs (temporary)"
        }
        else {
            Write-Step "Froze $frozenCount validation proof page(s) at docs\validation\$fullVersion"
        }
    }

    # THE FIFTH NUMBER, and the only one that is a statement about the SNAPSHOT rather than about the
    # working tree. The pre-flight census above holds the named identities over the tree as it stands
    # TODAY; this asks whether the thing about to be published is internally coherent -- is every page
    # frozen into this directory accounted for by the roster frozen beside it, and does every row of
    # that roster have its page here.
    #
    # A NAMED IDENTITY, not an equality (COORD ruling RN-6, ledger c695b56071). This compared the
    # frozen roster's row count with the frozen page count until 2026-09-23, and the two are unequal by
    # design: the snapshot freezes EVERY current page, and relocation anchors and exclusion pages have
    # no row of their own. At the 1.24.13 close it would have read 218 rows against 232 pages and
    # thrown -- after the bump, the signed tag and the write-once snapshot, the exact order defect the
    # pre-flight exists to prevent. The identity is the pre-flight's page identity, over the frozen
    # directory, through the same _roster.ps1 function:
    #
    #   frozen pages = frozen rows by name + relocation anchors by a frozen row's [proof] link
    #                  + exclusion rows by the frozen roster's own exclusion ledger
    #
    # and a frozen row's [proof] link that resolves to no sibling page is refused by name too -- a link
    # that would dangle inside the published snapshot.
    #
    # Rows, not the prose header. The roster's own "204 / 215" line is a DATED derivation that goes
    # stale independently of the table beside it (the same reason the census parses rows through
    # _roster.ps1 rather than reading the document's sentences), and check-roster-format.ps1 already
    # holds header-against-rows for the living page.
    #
    # OUTSIDE the write-once branch on purpose: a re-run that finds its snapshot already there gets
    # the check too, so a directory frozen by an earlier attempt is verified rather than assumed. A
    # snapshot with no frozen roster is the pre-2026-09-07 shape and is REPORTED, not failed -- the
    # eight published before this existed are correct as they stand and are never rewritten.
    $frozenRosterPath = Join-Path $versionProofs $frozenRosterName
    if (Test-Path $frozenRosterPath) {
        $frozenPageIds = @(Get-ChildItem $versionProofs -Filter *.md -File |
                           Where-Object { $_.Name -ne $frozenRosterName } | ForEach-Object { $_.BaseName })
        $frozenPageCount = $frozenPageIds.Count
        $frozenRowIds = @(Get-ValidatedRosterRows -Path $frozenRosterPath | ForEach-Object { $_.Package -replace '/', '.' })
        $frozenRowCount = $frozenRowIds.Count
        $frozenExcludedIds = @(Get-ExclusionLedgerRows -Path $frozenRosterPath | ForEach-Object { $_.Package -replace '/', '.' })
        $frozenLinkedIds = @(Get-RosterRowProofLinks -Path $frozenRosterPath -Frozen)

        $frozenIdentity = Get-ProofPageIdentity -Pages $frozenPageIds -Banked $frozenRowIds `
                                                -Linked $frozenLinkedIds -Excluded $frozenExcludedIds
        $frozenProblems = @($frozenIdentity.Violations)
        if (-not $frozenProblems.Count -and $frozenIdentity.ByName.Count -ne $frozenRowCount) {
            $frozenProblems += "$($frozenIdentity.ByName.Count) page(s) by name against $frozenRowCount frozen roster row(s) (a repeated row?)"
        }

        if ($frozenProblems.Count) {
            throw ("The frozen $fullVersion snapshot is not self-consistent: its roster carries " +
                   "$frozenRowCount row(s) against $frozenPageCount proof page(s) in the same directory, and " +
                   "$($frozenProblems.Count) item(s) fit no named class:`n    " + ($frozenProblems -join "`n    ") + "`n" +
                   "A published snapshot whose roster and proofs disagree advertises rows it cannot " +
                   "show the evidence for -- reconcile docs\ValidatedTestPackages.md with " +
                   "docs\validation\current before publishing.")
        }

        Write-Step ("Frozen snapshot self-consistent: $frozenRowCount roster row(s) / $frozenPageCount proof page(s) = " +
                    "$($frozenIdentity.ByName.Count) by name + $($frozenIdentity.ByLink.Count) relocation anchor(s) by a " +
                    "row's [proof] link + $($frozenIdentity.ByExclusion.Count) exclusion page(s)")
    }
    else {
        Write-Step "Frozen snapshot $fullVersion carries no roster page (snapshots before 1.23.12.3 did not) -- proof pages only"
    }

    # Retarget the version segment of every green badge link in the converted stdlib's READMEs. Read
    # AND write through [System.IO.File] with UTF-8/no-BOM: PS 5.1's Get-Content reads the converter's
    # BOM-less UTF-8 as ANSI and Out-File re-encodes the damage, which is what mojibake'd the corpus's
    # (c) signs once already. ReadAllText/WriteAllText round-trips the CRLF the converter emitted.
    # ($utf8NoBomText is created above the try, where the roster freeze needs the same encoding.)
    # The segment class excludes whitespace and ')' as well as '/': a hand-owned README's PROSE link
    # (testing's `validation/index.html) was produced...`) has no second '/', and a bare [^/]+ ate
    # everything up to the next stray slash -- collapsing four lines of prose into a broken URL on
    # the 1.23.1.3 release run. A green badge's versioned link always terminates its segment with
    # '/', so the tightened class changes nothing for the links this retarget exists to move.
    #
    # That same class is what lets the Tests badge share its LINE with the Docs badge (added
    # 2026-08-08): the space between the two badges terminates the segment, so a retarget can never
    # run past the proof link into the pkg.go.dev link beside it. The Docs badge is otherwise
    # invisible to every pattern in this block -- it is anchored on 'go2cs.net/validation/', and the
    # verification below on 'badge/Tests-', neither of which a 'badge/Docs-' / 'pkg.go.dev' badge
    # can satisfy. Verified against both a green and a vendored README before the badge landed.
    $badgeLinkPattern = '(https://go2cs\.net/validation/)[^/\s)]+(/)'
    $retargeted = 0

    foreach ($readme in Get-ChildItem (Join-Path $src 'core') -Filter 'README.md' -Recurse -File) {
        $text = [System.IO.File]::ReadAllText($readme.FullName)
        if ($text -notmatch $badgeLinkPattern) { continue }

        $updated = [regex]::Replace($text, $badgeLinkPattern, "`${1}$fullVersion`${2}")
        if ($updated -eq $text) { continue }

        if ($PSCmdlet.ShouldProcess($readme.FullName, "retarget validation badge link to $fullVersion")) {
            [System.IO.File]::WriteAllText($readme.FullName, $updated, $utf8NoBomText)
            $retargeted++
        }
    }

    Write-Step "Retargeted $retargeted README badge link(s) to $fullVersion (commit them with version.props)"

    # Consistency by construction: a converter README re-emission must now be a no-op. The badge line
    # is composed from exactly two inputs -- the published version and the proof page's totals line --
    # so re-deriving it here from the FROZEN snapshot and comparing byte for byte is that re-emission,
    # without needing the Go toolchain or a 4-minute reconvert mid-release.
    #
    # This is the arm the pre-flight above CANNOT replace, and it is kept for exactly that reason: it
    # reads the FROZEN snapshot rather than docs\validation\current, and it is the only place the
    # badge's version PIN is checkable, because the retarget that sets it runs a few lines up.
    $greenVerify = Get-GoGreenBadgeVerification -CoreDir (Join-Path $src 'core') -ProofRoot $versionProofs -PinnedVersion $fullVersion
    $verified = $greenVerify.Verified
    if ($greenVerify.Problems.Count) { throw ("Frozen-snapshot verification failed with $($greenVerify.Problems.Count) problem(s):`n    " + ($greenVerify.Problems -join "`n    ")) }

    if ($dryRun) {
        Write-Step "Verified $verified green badge(s) against the would-be $wouldBeVersion proof pages"
    }
    else {
        Write-Step "Verified $verified green badge(s) against the frozen $fullVersion proof pages"
    }
}

}
finally {
    # Only ever removes a directory this run created under the system temp path; $dryRunProofRoot is
    # $null on every release path, so this is a no-op there. In the finally so a throw anywhere in the
    # snapshot/verify block above still cleans up. (The try's body is left at its original indentation
    # to keep this fix's diff readable -- PowerShell does not care, and `git diff` shows the change
    # rather than a re-indent of ninety unchanged lines.)
    if ($dryRunProofRoot -and (Test-Path $dryRunProofRoot)) {
        Remove-Item $dryRunProofRoot -Recurse -Force -ErrorAction SilentlyContinue
        Write-Step "Removed the dry run's temporary proof snapshot"
    }
}

# --- C# Source badge retarget ---------------------------------------------------------------------
# The C# Source badge (2026-08-08) is version-pinned TWICE -- in its message and in the release tag
# its link resolves against -- and both must move to the version this run is publishing, or a
# published package's README sends the reader to the PREVIOUS release's C#.
#
# Its own block, deliberately outside the proof-snapshot branch above: this badge is on EVERY package
# README, validated or not, and has nothing to do with docs\validation. Gating it on the proof pages
# existing would silently ship stale source links on the one run where they had gone missing.
#
# Both patterns are anchored on literals only this badge carries -- 'badge/Source-@' paired with the
# .NET purple '-512BD4' (the Go Source badge beside it is '-00ADD8', so the colour field is what
# tells the twins apart since the r51d tidy dropped the language text from the message). The version
# class excludes '-' (it terminates at the badge's colour field) and whitespace/')' (so it can never
# run past the badge, the lesson the Tests-badge pattern learned the hard way on the 1.23.1.3 run).
# ⚠ The 1.23.1.5 run shipped with this pattern still spelling r51c's 'Source-C%23_@' form: the text
# retarget silently no-opped against r51d's renamed badge while the link retarget matched, and the
# verifier below SKIPPED files without the stale form instead of failing them -- a vacuous pass.
# Both are corrected here; the verifier now throws on a README with no C# Source badge at all.
$sourceBadgeVersionPattern = '(badge/Source-@)[^-\s)]+(-512BD4)'
$sourceBadgeTagPattern = '(https://github\.com/ritchiecarroll/go2cs/tree/nuget-)[^/\s)]+(/src/core/)'
$sourceRetargeted = 0

foreach ($readme in Get-ChildItem (Join-Path $src 'core') -Filter 'README.md' -Recurse -File) {
    $text = [System.IO.File]::ReadAllText($readme.FullName)
    $updated = $text

    foreach ($pattern in @($sourceBadgeVersionPattern, $sourceBadgeTagPattern)) {
        $updated = [regex]::Replace($updated, $pattern, "`${1}$fullVersion`${2}")
    }

    if ($updated -eq $text) { continue }

    if ($PSCmdlet.ShouldProcess($readme.FullName, "retarget C# Source badge to $fullVersion")) {
        [System.IO.File]::WriteAllText($readme.FullName, $updated, $utf8NoBom)
        $sourceRetargeted++
    }
}

Write-Step "Retargeted $sourceRetargeted C# Source badge(s) to $fullVersion (commit them with version.props)"

# Same consistency-by-construction check the green badges get: the badge is composed from
# version.props and nothing else, so re-deriving it here IS the converter re-emission, and both of
# its pins must name the version being published.
#
# Deliberately still OUTSIDE the proof-snapshot branch above, for the reason stated there: this badge
# is on EVERY package README, validated or not. The pre-flight ran this function's STRUCTURAL half;
# only the two version pins are new here, and they are only checkable after the retarget above.
$sourceVerify = Get-GoSourceBadgeVerification -CoreDir (Join-Path $src 'core') -PinnedVersion $fullVersion
$sourceVerified = $sourceVerify.Verified
if ($sourceVerify.Problems.Count) { throw ("C# Source badge verification failed with $($sourceVerify.Problems.Count) problem(s):`n    " + ($sourceVerify.Problems -join "`n    ")) }

Write-Step "Verified $sourceVerified C# Source badge(s) pin $fullVersion and its release tag"

# --- Multiplatform pack: ONE nupkg per package, carrying RID-specific assemblies ------------------
# docs\phase4\DESIGN-multiplatform-corpus.md section 9 option (a), staged as section 12 increment 4.
#
# The converted corpus is ONE tree in layout L3: a package whose C# varies by GOOS keeps its
# platform-selected sources in per-GOOS subfolders and $(GoTargetOS) admits exactly one of them to the
# compilation. A PUBLISHED package must nevertheless work on every supported platform without the
# consumer choosing anything, so the solution is built once per RID and the flavors are merged into a
# single nupkg per package:
#
#   go.os/
#     lib/<tfm>/os.dll                        compile-time asset + RID-agnostic runtime fallback
#     runtimes/win-x64/lib/<tfm>/os.dll       runtime asset, selected on win-x64
#     runtimes/linux-x64/lib/<tfm>/os.dll     runtime asset, selected on linux-x64
#
# WHY lib/ RATHER THAN ref/, against NuGet's documented asset selection. NuGet gives `lib/{tfm}/` both
# the `compile` and the `runtime` asset roles; `ref/{tfm}/` gives only `compile`; and
# `runtimes/{rid}/lib/{tfm}/` gives only `runtime`, RID-selected, and REQUIRES a compile asset to exist
# elsewhere in the package. A RID-specific managed assembly is therefore always a two-part shape, and
# the only question is what the compile half is:
#
#   * lib/ carrying the REFERENCE flavor (chosen). Compile-time binding is the reference flavor's
#     surface; at run time the host reads the `runtimeTargets` entries NuGet writes into deps.json for
#     the runtimes/ assets and, when one matches the running RID, uses it INSTEAD of the RID-agnostic
#     lib/ assembly of the same name -- so a portable framework-dependent app resolves the right
#     flavor with no RuntimeIdentifier anywhere in the consumer's project. On a RID this release does
#     not ship, the lib/ assembly is what loads: a Linux-arm64 consumer would silently get Windows
#     behaviour. That is the honest cost of this choice and it is why the shipped RID set is stated in
#     the design rather than inferred.
#   * ref/ carrying a neutral surface (not chosen). It would turn that silent wrong-flavor fallback
#     into a loud missing-assembly failure, which is better -- but there IS no neutral surface to put
#     there: section 6 measures `syscall` at 270 names in common out of 992/2,186/1,899 and `log/syslog` as
#     exporting nothing at all on Windows, so a ref/ assembly would have to be either a synthesised
#     intersection (a build artifact nothing in this repository produces) or one flavor again, which
#     is what lib/ already is, minus the fallback. section 11 records that seam as open.
#
# The reference flavor is the FIRST entry below, and it is Windows: that keeps the compile surface a
# consumer binds against exactly the one today's single-platform packages present, and it matches section 11's
# "let lib/<tfm>/syscall.dll carry the host-of-record flavor".
#
# DEPENDENCIES ARE PER TARGET FRAMEWORK, NEVER PER RID (section 9). A package whose imports differ by GOOS --
# 21 of them carry conditioned <ProjectReference> groups -- therefore declares the UNION of its
# flavors' dependencies, and the merge below computes that union. Both sets restore everywhere; only
# the RID-matched assemblies ever load.
Add-Type -AssemblyName System.IO.Compression.FileSystem

# --- nupkg surgery helpers ------------------------------------------------------------------------
# A .nupkg is an ordinary zip, so the merge below is entry-level: no repack, no re-sign, no NuGet
# authoring API. It runs at PACK time, before the offline signing step a release performs, so it can
# never invalidate a signature.
function Add-GoZipEntry([System.IO.Compression.ZipArchive]$Zip, [string]$Name, [byte[]]$Bytes) {
    if ($Zip.GetEntry($Name)) { throw "Package already contains an entry named $Name" }
    $entry = $Zip.CreateEntry($Name, [System.IO.Compression.CompressionLevel]::Optimal)
    $s = $entry.Open()
    try { $s.Write($Bytes, 0, $Bytes.Length) } finally { $s.Dispose() }
}

function Get-GoZipEntryText([System.IO.Compression.ZipArchiveEntry]$Entry) {
    $ms = New-Object System.IO.MemoryStream
    $s = $Entry.Open()
    try { $s.CopyTo($ms) } finally { $s.Dispose() }
    # Decoding with a BOM-less UTF8Encoding leaves any byte-order mark in the string as a leading
    # U+FEFF, so re-encoding with the same object reproduces the original preamble exactly rather
    # than silently adding or dropping one.
    return (New-Object System.Text.UTF8Encoding($false)).GetString($ms.ToArray())
}

function Set-GoZipEntryText([System.IO.Compression.ZipArchive]$Zip, [string]$Name, [string]$Text) {
    $existing = $Zip.GetEntry($Name)
    if ($existing) { $existing.Delete() }
    Add-GoZipEntry $Zip $Name ((New-Object System.Text.UTF8Encoding($false)).GetBytes($Text))
}

function Test-GoXmlWhitespace($Node) {
    if (-not $Node) { return $false }
    if (@('Whitespace', 'SignificantWhitespace') -contains [string]$Node.NodeType) { return $true }
    return ([string]$Node.NodeType -eq 'Text' -and $Node.Value -match '^\s*$')
}

# Union the flavor's <dependencies> into the base .nuspec's. NuGet declares dependencies per target
# framework only -- never per RID (section 9) -- so a package whose imports differ by GOOS must declare every
# flavor's, and let the RID decide which assemblies actually load. Matching is by id: the version is
# one value across the whole release, so two flavors can only ever disagree about PRESENCE.
function Merge-GoNuspecDependencies([string]$BaseText, [string]$FlavorText, [string]$Id, [string]$Rid) {
    # `dotnet pack` writes the .nuspec with a UTF-8 BOM, which Get-GoZipEntryText deliberately keeps
    # as a leading U+FEFF so the rewrite reproduces it. XmlDocument.LoadXml takes a STRING, where a
    # U+FEFF is an ordinary character and not a legal document prefix ("Data at the root level is
    # invalid. Line 1, position 1."), so it is peeled off here and put back on the way out.
    $bom = ''
    if ($BaseText.Length -and $BaseText[0] -eq [char]0xFEFF) { $bom = [string][char]0xFEFF; $BaseText = $BaseText.Substring(1) }
    if ($FlavorText.Length -and $FlavorText[0] -eq [char]0xFEFF) { $FlavorText = $FlavorText.Substring(1) }

    $baseDoc = New-Object System.Xml.XmlDocument
    $baseDoc.PreserveWhitespace = $true
    $baseDoc.LoadXml($BaseText)

    $flavorDoc = New-Object System.Xml.XmlDocument
    $flavorDoc.PreserveWhitespace = $true
    $flavorDoc.LoadXml($FlavorText)

    # local-name() throughout: the .nuspec's default namespace varies by schema revision and none of
    # this cares which one it is.
    $flavorDeps = $flavorDoc.SelectSingleNode("//*[local-name()='dependencies']")
    if (-not $flavorDeps) { return ($bom + $BaseText) }

    $baseDeps = $baseDoc.SelectSingleNode("//*[local-name()='dependencies']")
    if (-not $baseDeps) {
        # The reference flavor declares nothing and this one does. Import the whole block rather than
        # dropping it -- a dependency that exists on only one platform is exactly the case (a) exists for.
        $metadata = $baseDoc.SelectSingleNode("//*[local-name()='metadata']")
        if (-not $metadata) { throw "No <metadata> in the .nuspec of $Id" }
        [void]$metadata.AppendChild($baseDoc.ImportNode($flavorDeps, $true))
        return ($bom + $baseDoc.OuterXml)
    }

    # Modern `dotnet pack` always emits <group targetFramework=...>; the flat form is handled so this
    # cannot quietly become a no-op if that ever changes.
    $flavorGroups = @($flavorDeps.SelectNodes("*[local-name()='group']"))
    if ($flavorGroups.Count -eq 0) { $flavorGroups = @($flavorDeps) }

    foreach ($fg in $flavorGroups) {
        $tfm = ''
        if ($fg.Attributes -and $fg.Attributes['targetFramework']) { $tfm = $fg.Attributes['targetFramework'].Value }

        $baseGroups = @($baseDeps.SelectNodes("*[local-name()='group']"))
        if ($baseGroups.Count -eq 0) { $baseGroups = @($baseDeps) }

        $bg = $null
        foreach ($g in $baseGroups) {
            $gTfm = ''
            if ($g.Attributes -and $g.Attributes['targetFramework']) { $gTfm = $g.Attributes['targetFramework'].Value }
            if ($gTfm -eq $tfm) { $bg = $g; break }
        }

        if (-not $bg) { [void]$baseDeps.AppendChild($baseDoc.ImportNode($fg, $true)); continue }

        $existing = @($bg.SelectNodes("*[local-name()='dependency']"))
        $have = @($existing | ForEach-Object { $_.Attributes['id'].Value })
        $anchor = $null
        if ($existing.Count) { $anchor = $existing[$existing.Count - 1] }

        foreach ($fd in @($fg.SelectNodes("*[local-name()='dependency']"))) {
            if ($have -contains $fd.Attributes['id'].Value) { continue }

            $imported = $baseDoc.ImportNode($fd, $true)
            if ($anchor -and (Test-GoXmlWhitespace $anchor.PreviousSibling)) {
                # Reproduce the indentation of the line above, so the merged .nuspec stays readable.
                $ws = $anchor.PreviousSibling.CloneNode($true)
                [void]$bg.InsertAfter($ws, $anchor)
                [void]$bg.InsertAfter($imported, $ws)
            }
            else {
                [void]$bg.AppendChild($imported)
            }
            $anchor = $imported
        }
    }

    return ($bom + $baseDoc.OuterXml)
}

# RID -> $(GoTargetOS). ORDER IS SIGNIFICANT: the first entry is the reference flavor (see above).
# Increment 5 adds macOS here, and only here.
$ridFlavors = [ordered]@{
    'win-x64'   = 'windows'
    'linux-x64' = 'linux'
}
$referenceRid = @($ridFlavors.Keys)[0]

if ($SkipBuild) {
    throw ("-SkipBuild cannot produce a multiplatform release. The RID-specific assemblies come from one " +
           "build pass per RID ($(@($ridFlavors.Keys) -join ', ')), each with a different `$(GoTargetOS), " +
           "so no single on-disk build holds them all. Re-run without -SkipBuild.")
}

New-Item -ItemType Directory -Force $OutDir | Out-Null
Get-ChildItem $OutDir -Filter *.nupkg -ErrorAction SilentlyContinue | Remove-Item -Force -ErrorAction SilentlyContinue

# --- Which packages need RID-specific assets, derived from the corpus itself ----------------------
# Never a hardcoded list. Layout L3's own rule, read back off the tree: a directory named after a GOOS
# that holds NO project file is a per-GOOS SOURCE folder, and its parent is a platform-varying package.
# The "no project file" test is what keeps `internal/syscall/windows` -- a real package whose own name
# is a GOOS -- from being read as `internal/syscall`'s Windows variants, exactly as section 8 specifies for
# the converter's layout-adoption rule.
#
# Every such package ships RID-specific assets whether or not this RID PAIR happens to change its
# emission (a package varying only on darwin, say, produces identical win/linux assemblies). Shipping
# by structure rather than by a byte compare keeps the package shape a predictable function of the
# corpus; the byte compare below is the verification, not the decision.
$coreDir = Join-Path $src 'core'
$goosNames = @('windows', 'linux', 'darwin')
$ridSplitIds = New-Object 'System.Collections.Generic.HashSet[string]' ([StringComparer]::OrdinalIgnoreCase)

foreach ($dir in Get-ChildItem $coreDir -Recurse -Directory) {
    if ($goosNames -notcontains $dir.Name) { continue }
    if (@(Get-ChildItem $dir.FullName -Filter *.csproj -File).Count -gt 0) { continue }

    $projs = @(Get-ChildItem $dir.Parent.FullName -Filter *.csproj -File | Where-Object { $_.Name -notlike '*.tests.csproj' })
    if ($projs.Count -ne 1) { throw "Expected exactly one library project beside per-GOOS folder $($dir.FullName); found $($projs.Count)" }

    $projText = [System.IO.File]::ReadAllText($projs[0].FullName)
    if ($projText -notmatch '<AssemblyName>([^<]+)</AssemblyName>') { throw "No <AssemblyName> in $($projs[0].FullName)" }
    [void]$ridSplitIds.Add("go.$($Matches[1])")
}

Write-Step "Layout L3: $($ridSplitIds.Count) package(s) carry per-GOOS sources -> RID-specific assemblies"

# --- One build + pack pass per RID ---------------------------------------------------------------
$flavorRoot = Join-Path $OutDir '_flavors'
if (Test-Path $flavorRoot) { Remove-Item $flavorRoot -Recurse -Force }

# REVERSED deliberately, so the REFERENCE flavor is the last pass. Every pass writes the same
# bin\/obj\, so whichever runs last is what a developer's tree is left holding; ending on the
# reference flavor leaves it in exactly the state a plain property-absent build produces, instead of
# silently leaving Linux assemblies behind for the next local run to pick up.
$buildOrder = @($ridFlavors.Keys)
[array]::Reverse($buildOrder)

foreach ($rid in $buildOrder) {
    $goos = $ridFlavors[$rid]
    $flavorOut = Join-Path $flavorRoot $rid
    New-Item -ItemType Directory -Force $flavorOut | Out-Null

    # --no-incremental on EVERY pass, for two independent reasons. (1) The passes share one obj\/bin\,
    # and what differs between them is the <Compile> ITEM SET, not any source timestamp -- a clean
    # compile is the cheap way to be certain pass N never inherits pass N-1's assembly. (2) The flag is
    # not byte-neutral (section 12 increment 3.5's second measurement trap), so a byte comparison between the
    # flavors -- which this script performs below -- is only an instrument if it is held constant.
    #
    # UseSharedCompilation=false is a MEASURED necessity here, not the usual concurrency hygiene.
    # go2cs-gen runs inside the compiler, and with the shared VBCSCompiler every project's generator
    # work funnels through one server process: the same clean Release build of this solution measures
    # ~160 s with csc per project and had not finished after 14 minutes without it (one core busy, 24
    # MSBuild nodes idle). Two RID passes make that the difference between a 6-minute release and an
    # hour-long one.
    Write-Step "[$rid] Building $Configuration at -p:GoTargetOS=$goos (compiles the whole stdlib; several minutes)"
    & dotnet build $slnx -c $Configuration -p:GoTargetOS=$goos -p:GeneratePackageOnBuild=false --no-incremental -p:UseSharedCompilation=false --nologo -v m
    if ($LASTEXITCODE -ne 0) { throw "[$rid] dotnet build failed ($LASTEXITCODE) at -p:GoTargetOS=$goos -- fix build errors before packing" }

    # go2cs-gen is GoTargetOS-neutral, yet under the FULL script's solution build its output copy has
    # been observed missing at pack time (3 of 3 release runs, R 2026-08-23) while the identical build
    # invoked in isolation produces it (3 of 3 probes) -- an unrooted solution-build race. Assert and
    # repair deterministically before the --no-build pack: the direct build is a cheap no-op when the
    # output already exists, and pack cannot proceed without it. Root-cause is boarded (the
    # release-machinery hardening item); this is its assert half landed on the release's critical path.
    $genOut = Join-Path $PSScriptRoot 'gen/go2cs-gen/bin' | Join-Path -ChildPath $Configuration | Join-Path -ChildPath 'netstandard2.0'
    if (-not (Test-Path (Join-Path $genOut 'go2cs-gen.dll'))) {
        Write-Step "[$rid] go2cs-gen output missing after the solution build -- repairing with a direct project build"
        & dotnet build (Join-Path $PSScriptRoot 'gen/go2cs-gen/go2cs-gen.csproj') -c $Configuration -p:UseSharedCompilation=false --nologo -v m
        if ($LASTEXITCODE -ne 0) { throw "[$rid] go2cs-gen repair build failed ($LASTEXITCODE)" }
        if (-not (Test-Path (Join-Path $genOut 'go2cs-gen.dll'))) { throw "[$rid] go2cs-gen output still missing after a direct build -- investigate before packing" }
    }

    # --no-build packs exactly what the pass above produced; the same -p:GoTargetOS is required here
    # too, because it selects the conditioned <ProjectReference> groups the .nuspec is derived from.
    Write-Step "[$rid] Packing -> $flavorOut"
    # A local rehearsal overrides the PACKAGE version only (a global property, so every go.* dependency
    # between the packages carries it too); a release passes nothing extra. The @( ) must wrap the whole `if`:
    # an `if` that yields a one-element array unrolls it to a bare string, and @ splats a string by character.
    $packVersionArgs = @(if ($VersionSuffix) { "-p:PackageVersion=$packVersion" })
    & dotnet pack $slnx -c $Configuration -o $flavorOut -p:GoTargetOS=$goos -p:GeneratePackageOnBuild=false --no-build --nologo -v m @packVersionArgs
    if ($LASTEXITCODE -ne 0) { throw "[$rid] dotnet pack failed ($LASTEXITCODE)" }
}

# --- Asset merge ----------------------------------------------------------------------------------
function Read-GoPackageFacts([string]$Path) {
    $zip = [System.IO.Compression.ZipFile]::OpenRead($Path)
    try {
        $nuspec = @($zip.Entries | Where-Object { $_.FullName -notlike '*/*' -and $_.FullName -like '*.nuspec' })
        if ($nuspec.Count -ne 1) { throw "Expected exactly one root .nuspec in $Path; found $($nuspec.Count)" }

        $reader = New-Object System.IO.StreamReader($nuspec[0].Open())
        try { $text = $reader.ReadToEnd() } finally { $reader.Dispose() }
        if ($text -notmatch '<id>([^<]+)</id>') { throw "No <id> in the .nuspec of $Path" }
        $id = $Matches[1]

        # Hash the compile/runtime payload only. README.md, VALIDATION.md, the icons and the .nuspec
        # are flavor-independent by construction, and the OPC bookkeeping parts (.psmdcp) carry a
        # freshly minted identifier on every pack, so including them would make every comparison differ.
        $sha = [System.Security.Cryptography.SHA256]::Create()
        $lib = @{}
        $size = @{}
        try {
            foreach ($e in $zip.Entries) {
                if ($e.FullName -notlike 'lib/*') { continue }
                $size[$e.FullName] = $e.Length
                $s = $e.Open()
                try { $lib[$e.FullName] = [BitConverter]::ToString($sha.ComputeHash($s)) } finally { $s.Dispose() }
            }
        } finally { $sha.Dispose() }

        return [pscustomobject]@{ Id = $id; Path = $Path; Lib = $lib; Size = $size }
    }
    finally { $zip.Dispose() }
}

Write-Step "Reading packed flavors"
$facts = @{}
foreach ($rid in $ridFlavors.Keys) {
    $byId = @{}
    foreach ($f in Get-ChildItem (Join-Path $flavorRoot $rid) -Filter *.nupkg) {
        $fact = Read-GoPackageFacts $f.FullName
        if ($byId.ContainsKey($fact.Id)) { throw "[$rid] Two .nupkg claim package id $($fact.Id)" }
        $byId[$fact.Id] = $fact
    }
    $facts[$rid] = $byId
    Write-Step "  [$rid] $($byId.Count) package(s)"
}

$refFacts = $facts[$referenceRid]
$otherRids = @($ridFlavors.Keys | Where-Object { $_ -ne $referenceRid })

# The flavors must agree on the package-ID SET. They do today because every project is in the union
# solution and a platform-exclusive package still builds (to an assembly with no types) everywhere --
# but that is a property of the corpus, not a guarantee, and a package that appeared in only one
# flavor would otherwise be published as a silently single-platform package.
foreach ($rid in $otherRids) {
    $missing = @($refFacts.Keys | Where-Object { -not $facts[$rid].ContainsKey($_) })
    $extra = @($facts[$rid].Keys | Where-Object { -not $refFacts.ContainsKey($_) })
    if ($missing.Count -or $extra.Count) {
        throw "[$rid] package-id set differs from [$referenceRid]: $($missing.Count) missing ($($missing -join ', ')), $($extra.Count) extra ($($extra -join ', '))"
    }
}

# Verification. A package the corpus says is platform-neutral should produce an EQUIVALENT assembly
# under every flavor -- that is section 13.1's finding, re-checked here against the artifacts actually
# being shipped rather than against a build tree. One that does not is promoted to RID-specific
# (correctness first) and reported loudly. The converse -- an L3 package whose flavors happen to match
# for this RID pair -- is only a measurement, and is reported as one.
#
# WHAT "EQUIVALENT" HAS TO MEAN HERE, and why a byte compare cannot be the test. Roslyn's
# deterministic identity fields -- the PE timestamp, the MVID, the PDB id and the PDB content
# checksum -- are hashes OF THE COMPILATION, and they CASCADE: an assembly whose identity moved shifts
# every dependent's identity too, even when every source file and every semantic byte is unchanged.
# Measured on this corpus (increment 4): a byte compare called 216 of 270 platform-neutral packages
# "different", and the difference was ~72 bytes of a ~340 KB assembly in four runs, with the assembly
# LENGTH unchanged -- the same signature section 12's increment-3 proof recorded. Meanwhile the same
# comparison between two SAME-flavor packs reports every assembly byte-identical, so the build is
# reproducible and the cascade is the whole explanation.
#
# So the discriminator is the entry set plus the assembly LENGTH. Identity fields are fixed-size, so a
# pure identity difference can never move it: this test has no false positives, which is what a
# release gate needs. It is a smoke alarm, not a semantic digest -- section 13.1's digest remains the
# instrument that answers the IL question in full, and it is a per-increment measurement rather than a
# per-release one.
$promoted = @()
$variesOnThisPair = 0
$identityOnly = 0
$l3Count = $ridSplitIds.Count   # captured before any promotion below can inflate it

foreach ($id in @($refFacts.Keys | Sort-Object)) {
    $material = $false      # entry set or assembly length differs: a real difference
    $anyByte = $false       # any byte differs at all: includes the identity cascade above

    foreach ($rid in $otherRids) {
        $a = $refFacts[$id]
        $b = $facts[$rid][$id]
        if ($a.Lib.Count -ne $b.Lib.Count) { $material = $true; $anyByte = $true; break }

        foreach ($k in $a.Lib.Keys) {
            if (-not $b.Lib.ContainsKey($k)) { $material = $true; $anyByte = $true; break }
            if ($a.Size[$k] -ne $b.Size[$k]) { $material = $true }
            if ($a.Lib[$k] -ne $b.Lib[$k]) { $anyByte = $true }
        }
        if ($material) { break }
    }

    if ($ridSplitIds.Contains($id)) {
        if ($anyByte) { $variesOnThisPair++ }
    }
    elseif ($material) {
        $promoted += $id
        [void]$ridSplitIds.Add($id)
    }
    elseif ($anyByte) { $identityOnly++ }
}

Write-Step ("Flavor comparison across {0}: {1} of {2} L3 package(s) differ" -f `
            ($ridFlavors.Keys -join '/'), $variesOnThisPair, $l3Count)
Write-Step ("  of {0} platform-neutral package(s): {1} differ materially, {2} differ only in the deterministic-identity fields (expected -- see the note above)" -f `
            ($refFacts.Count - $l3Count), $promoted.Count, $identityOnly)

if ($promoted.Count) {
    Write-Warning ("These packages carry NO per-GOOS sources yet their assemblies differ in LENGTH between flavors, " +
                   "which the identity cascade cannot explain, so they are being shipped with RID-specific assets: " +
                   "$($promoted -join ', '). Investigate before publishing -- the corpus has most likely gained a " +
                   "platform axis this script's L3 derivation cannot see " +
                   "(see docs\phase4\DESIGN-multiplatform-corpus.md section 13.1).")
}

# The merge itself. A neutral package is copied VERBATIM from the reference flavor -- byte for byte the
# package today's single-pass release produced -- so the Windows lane cannot regress through this
# script. Only a RID-specific package is rewritten.
Write-Step "Merging RID assets -> $OutDir"
$merged = 0
$copied = 0

foreach ($id in @($refFacts.Keys | Sort-Object)) {
    $refPath = $refFacts[$id].Path
    $target = Join-Path $OutDir (Split-Path $refPath -Leaf)
    Copy-Item $refPath $target -Force

    if (-not $ridSplitIds.Contains($id)) { $copied++; continue }

    $zip = [System.IO.Compression.ZipFile]::Open($target, 'Update')
    try {
        # The reference flavor's own assets move from lib/ to its RID folder as well as staying in
        # lib/. Duplicating rather than relying on "no RID matched, fall back to lib/" is deliberate:
        # it states each shipped RID's flavor explicitly in the package, so which assembly a RID gets
        # never depends on which flavor lib/ happens to carry.
        $libEntries = @($zip.Entries | Where-Object { $_.FullName -like 'lib/*' })
        foreach ($e in $libEntries) {
            $ms = New-Object System.IO.MemoryStream
            $s = $e.Open()
            try { $s.CopyTo($ms) } finally { $s.Dispose() }
            Add-GoZipEntry $zip ("runtimes/$referenceRid/" + $e.FullName) $ms.ToArray()
        }

        $nuspecEntry = @($zip.Entries | Where-Object { $_.FullName -notlike '*/*' -and $_.FullName -like '*.nuspec' })[0]
        $nuspecText = Get-GoZipEntryText $nuspecEntry

        foreach ($rid in $otherRids) {
            $src2 = [System.IO.Compression.ZipFile]::OpenRead($facts[$rid][$id].Path)
            try {
                foreach ($e in @($src2.Entries | Where-Object { $_.FullName -like 'lib/*' })) {
                    $ms = New-Object System.IO.MemoryStream
                    $s = $e.Open()
                    try { $s.CopyTo($ms) } finally { $s.Dispose() }
                    Add-GoZipEntry $zip ("runtimes/$rid/" + $e.FullName) $ms.ToArray()
                }

                $flavorNuspec = @($src2.Entries | Where-Object { $_.FullName -notlike '*/*' -and $_.FullName -like '*.nuspec' })[0]
                $nuspecText = Merge-GoNuspecDependencies $nuspecText (Get-GoZipEntryText $flavorNuspec) $id $rid
            }
            finally { $src2.Dispose() }
        }

        Set-GoZipEntryText $zip $nuspecEntry.FullName $nuspecText
    }
    finally { $zip.Dispose() }

    $merged++
}

Write-Step "Merged $merged RID-specific package(s); copied $copied platform-neutral package(s) verbatim"

$pkgs = @(Get-ChildItem $OutDir -Filter *.nupkg)
Write-Step "Packed $($pkgs.Count) package(s)"
if ($pkgs.Count -eq 0) { throw "No .nupkg produced in $OutDir" }

# --- Push gate ----------------------------------------------------------------------------------
if (-not $Push) {
    Write-Host ""
    Write-Host "Pack-only (default). Inspect $OutDir, then re-run with -Push to publish." -ForegroundColor Yellow
    exit 0
}

if (-not $ApiKey) { throw "-Push requires an API key: pass -ApiKey or set `$env:NUGET_API_KEY." }

# Publish go.lib and go.gen first (dependencies of every stdlib package). --skip-duplicate makes a
# re-run idempotent; nuget.org indexes asynchronously so strict ordering is a nicety, not required.
$deps = @($pkgs | Where-Object { $_.Name -match '^go\.(lib|gen)\.' })
$rest = @($pkgs | Where-Object { $_.Name -notmatch '^go\.(lib|gen)\.' })
$ordered = $deps + $rest

Write-Step "Pushing $($ordered.Count) package(s) to $Source"
$pushed = 0
foreach ($p in $ordered) {
    if ($PSCmdlet.ShouldProcess($p.Name, "nuget push -> $Source")) {
        & dotnet nuget push $p.FullName --source $Source --api-key $ApiKey --skip-duplicate
        if ($LASTEXITCODE -ne 0) { throw "push failed for $($p.Name) ($LASTEXITCODE)" }
        $pushed++
    }
}
Write-Step "Done. Pushed $pushed package(s) at version $fullVersion."
