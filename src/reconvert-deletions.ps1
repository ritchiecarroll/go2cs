<#
.SYNOPSIS
    The H5 DELETION PASS: classify, and optionally delete, the files a SEEDED reconvert root still
    holds because the converter has STOPPED emitting them at the target release.

.DESCRIPTION
    THE HOLE THIS CLOSES. The corpus reconvert ritual (CLAUDE.md, "Corpus mechanics") seeds a scratch
    root from src\core before converting, and that seeding is non-negotiable -- an unseeded root gives
    the [module: GoManualConversion] detector nothing to detect and every hand-owned file is clobbered.
    But the seeding buys that protection at a price the ritual never paid: A SEEDED ROOT CANNOT REVEAL
    A FILE THE CONVERTER HAS STOPPED EMITTING. The seed put it there; the conversion simply did not
    rewrite it; the overlay copies it back; and nothing anywhere performs a deletion.

    At an ordinary regen that costs nothing, because the emitted set does not move. AT A RELEASE HOP
    IT IS FATAL. Lane R's 1.24.13 rehearsal (docs/phase4/REHEARSAL-h5-go124.md, section 3) measured 25
    such files, and the FIRST build died on the smallest of them in 116 seconds having measured
    nothing: internal/goexperiment/exp_aliastypeparams_off.cs (seeded, 1.23.12) and
    exp_aliastypeparams_on.cs (emitted, 1.24.13) BOTH declare AliasTypeParams, the package csproj
    globs *.cs so both compile, and the result is CS0102 x2 in a leaf that essentially the whole
    corpus depends on. Three GOEXPERIMENT flips default ON at 1.24.13 (aliastypeparams, swissmap,
    synchashtriemap) plus the FIPS reorganization move or deselect the other 24.

    WHAT A DELETION CANDIDATE IS -- the question this instrument got WRONG on its first real run, and
    the reason the NOT-A-CONVERSION-TARGET class exists. The pass as first landed asked exactly one
    question of every seeded file: "does Go still select this file's principal at the target?" That
    question is only MEANINGFUL for a file the converter EMITS. R's first dry run against a
    three-target scratch (mailbox 1f5e8f276, dry run, NOT applied) produced a 205-row delete set of
    which 117 rows were src\core\golib\*.cs (116) and src\core\go2cs\Symbols.cs (1) -- THE
    HAND-WRITTEN RUNTIME AND THE SYMBOLS SHARED PROJECT -- classified DELETE-ABSENT because
    "package not in std at target" is TRUE and IRRELEVANT for a directory that was never a Go package
    in the first place. The marker-based PROTECTED arm cannot see them: golib correctly carries no
    [module: GoManualConversion] marker, because nothing ever CONVERTS into golib and there is no
    generated body for a marker to displace. THE ONE DIRECTORY THAT NEEDS NO MARKER IS THE ONE THE
    MARKER GUARD DOES NOT PROTECT.

    So the candidate test is now POSITIVE and comes FIRST: a file is a deletion candidate only if it
    belongs to a package the converter actually emits, i.e. its resolved import path is in
    `go list std` AT THE SOURCE RELEASE (the outgoing corpus's own release, -SourceGoRoot) and is not
    skip-listed by the converter's own isNonConvertedStdLibPackage. Everything else under core\ --
    golib\, go2cs\, the skip-listed hand-owned packages (unsafe, testing), and any .cs with no package
    directory at all -- is NOT-A-CONVERSION-TARGET and can never reach a DELETE row. Asking the SOURCE
    rather than the target is what makes a REMOVED package still a candidate: internal/weak is in std
    at 1.23.12 and gone at 1.24.13, which is precisely the DELETE-ABSENT the pass exists to find.

    WHY MODIFICATION TIME ALONE CANNOT DECIDE A DELETION -- the load-bearing caveat. The converter's
    writePackageFile path goes through needToWriteFile (projectFileWriter.go), which SKIPS a write
    whose bytes are identical. So a file whose emission did not CHANGE between the two releases keeps
    its seed timestamp and reads SEEDED, exactly like a file that stopped being emitted. R measured
    1292 seeded-not-rewritten production .cs against 25 real deletions: the seeded set is ~50x the
    deletion set, and a timestamp-only deletion pass would destroy the corpus. The timestamp answers
    only "is this file a CANDIDATE"; GO ITSELF answers "should it exist", via

        go list -tags <the converter's set> -f '{{.GoFiles}} {{.CgoFiles}}' <importpath>

    run against the TARGET GOROOT with the corpus's own emission state (CGO_ENABLED=0) and the file's
    own flavour (GOOS). A file whose Go principal is still SELECTED there is kept, whatever its
    timestamp says.

    ⚠ THE -tags ARGUMENT IS LOAD-BEARING AND THIS PARAGRAPH USED TO OMIT IT. Without it the call asks
    about a DIFFERENT corpus than the one on disk: `-stdlib` emits under `purego, math_big_pure_go` by
    default (resolveBuildTags, commandLineOptions.go), and at go1.24.13 that disagreement moves NINETEEN
    principals per flavour -- so every purego variant read "not selected" and five of them were deleted
    from a live corpus. $GoEnvBase also empties GOFLAGS, so the tags cannot arrive by any other route and
    the omission was total. See -BuildTags, -TagLine and DELETE-DESELECTED below: there is now ONE tag
    resolution in the pipeline, it is the converter's, and a control proves it reaches `go list` before
    any row is classified.

    HOW EMITTED-VS-SEEDED IS DECIDED, and how it differs from the platform census. platformCensus.go
    stamps every seeded file to a fixed sentinel instant (censusSeedSentinel, 2000-01-01Z) and then
    tests emitted := !ModTime.Equal(sentinel) -- an exact, content-independent equality it can afford
    because it did the stamping. This instrument runs AFTER somebody else's reconvert and did not
    stamp anything, so it takes the reconvert's START stamp and mirrors the same rule as a threshold:
    a file modified BEFORE the sentinel was seeded, one modified at or after it was emitted. Pass the
    stamp as -SentinelTime, or as -Sentinel <path> naming a file created immediately before the
    conversion started (whose mtime is then the stamp). Either way the instrument REFUSES to run
    without one -- an absent sentinel would make every file look seeded.

    CLASSES, and what each means. Every candidate lands in exactly one, and every count is printed
    whether or not it is zero (a class that prints only when non-empty cannot be told from a class
    whose predicate never fired):

        NOT-A-CONVERSION-TARGET
                            the converter does not emit into this file's directory at all, so no Go
                            question about it is meaningful. Three disjoint reasons, each printed:
                            a HAND-WRITTEN repository root (golib\, go2cs\); a std package the
                            converter SKIP-LISTS (isNonConvertedStdLibPackage: unsafe, builtin,
                            testing, cmd and cmd/...); or an import path that is not in std at the
                            SOURCE release -- which includes a .cs sitting directly in core\ with no
                            package directory at all. NEVER deleted. Tested FIRST, before the marker
                            scan and before any target lookup.
        PROTECTED           carries the line-anchored [module: GoManualConversion] marker, or is an
                            *_impl.cs companion, INSIDE a package the converter does emit. NEVER
                            deleted, whatever Go says about its principal. A hand-own is the corpus's
                            own code; it is a reconciliation item for a human (R's runtime2.cs /
                            mfinal.cs), never a deletion.
        KEEP-SELECTED       Go still selects the principal at the target for this flavour. This is
                            the dominant class by construction (the needToWriteFile caveat above) and
                            it is the instrument's own negative control: a pass that cannot answer
                            "keep" is a pass that would delete the corpus.
        DELETE-ABSENT       the principal is gone at the target -- the file was removed, or its whole
                            package was (H3 removals: internal/weak, runtime/internal/sys, ...).
        DELETE-DESELECTED   the principal still EXISTS on disk at the target but Go does not select it
                            for this flavour -- a build-tag or GOEXPERIMENT flip -- AND the SOURCE
                            release DID select it, under the same tag set. This is the class that killed
                            R's build: exp_aliastypeparams_off.go is present at 1.24.13 and simply not
                            chosen.

                            ⚠ THE SECOND HALF OF THAT PREDICATE IS NOT DECORATION -- it is what the class
                            MEANS, and without it the class deleted five live files. The pass asked `go
                            list` with NO -tags while the converter had emitted the corpus under
                            `purego, math_big_pure_go` (its -stdlib default), so every purego variant read
                            "not selected" and was removed: crypto/md5/md5block_generic.cs,
                            crypto/sha1/sha1block_generic.cs, hash/maphash/maphash_purego.cs,
                            vendor/.../alias/alias_purego.cs, vendor/.../poly1305/mac_noasm.cs. The
                            modification-time arm did not save them because needToWriteFile skipped the
                            write for an unchanged body, so all five kept the seed stamp (two defects in
                            series; coordinator ruling 9c07f494f, i9's measurement bb3a1a747). Asking the
                            SOURCE release the same question turns "not selected here" into "the release
                            stopped selecting it", which is the only form that justifies a deletion.
        UNEXPLAINED-        a DELETE-DESELECTED candidate that NEITHER release selects under the
        DESELECTION         converter's tag set, while its principal is present at the target. Nothing
                            about the release hop explains it, so it is not a deselection: NEVER deleted,
                            always listed with all three readings, and the run EXITS NON-ZERO -- with or
                            without -Apply, because the incoherence is in the instrument's view of the
                            release pair and every other row in the same run shares that view. A wrong
                            -BuildTags or a wrong -Goarch is the likely cause and is named first in the
                            refusal; a converter defect is the third.
        UNRESOLVED          no Go principal is derivable INSIDE a package that SURVIVES at the target,
                            and the file is not generated metadata -- a stem that does not map to a .go
                            file name. NEVER deleted, always listed, and the run EXITS NON-ZERO so a
                            human reads them. Silently dropping one would be the silent-subtraction
                            failure this repository has already paid for.
        KEEP-METADATA       generated metadata (package_info.cs, package_init.cs,
                            package_info_internal_test.cs) in a package that SURVIVES at the target.
                            Kept, listed BY NAME, and NOT blocking.

                            THIS CLASS EXISTS BECAUSE THE ORDER WAS WRONG, not because a new
                            question was asked. Metadata used to return UNRESOLVED before the
                            absent-package test ran, so a package_info.cs whose PACKAGE was deleted
                            between the releases -- the stale residue this pass exists to remove -- read
                            as "needs a human" rather than DELETE-ABSENT. i9 measured the consequence on
                            a real three-target tree: 42 such rows, H5c refusing, the rung stopped
                            (mailbox 8f2eafdc8). Asking the package question FIRST splits them: gone at
                            the target is DELETE-ABSENT, survives is this class.

                            AND MEMBERSHIP DECIDES IT, NOT EMISSION. A surviving-package row that no
                            staging root carries is a package Go has and the run did not emit; deleting
                            it on that evidence is the mtime mistake in another coat (see the caveat
                            above, which the withdrawn third clause violated one screen below itself).
                            R's crypto/ecdh/package_init.cs is a genuine stale row and lands in
                            DELETE-ABSENT by its own package's absence, which is the right reason.

    Files the conversion emitted this run are not candidates at all. Neither are the test-host
    artifacts (package_test_info.cs, go2cs_test_host.cs) or any *_test.cs / *.cs.auto / *.g.cs: the
    package csproj <Compile Remove>s them, so a stale one cannot produce the CS0102 this pass exists to
    prevent, and admitting them would bury the real rows under hundreds of UNRESOLVED lines (R
    subtracted 384 test-host artifacts from the same arithmetic). They are counted, not listed.

    EVERY REFUSAL RUNS BEFORE THE DELETION LOOP. The first landing put the UNRESOLVED check AFTER the
    Remove-Item loop, so a run that exited 2 had already deleted -- a report wearing a refusal's exit
    code. Exit 2 now means, without exception, that NOTHING WAS REMOVED: the UNRESOLVED check, the
    delete-set decomposition and the trespass assertion (no delete row may sit under a hand-written
    root or a skip-listed package directory -- a PATH test, derived independently of the `go list`
    that classified it) all run first, and the loop itself re-checks every row it is about to delete.

.PARAMETER Root
    The seeded-and-reconverted scratch src root -- the directory holding core\. NOT the repository's
    own src\ (this instrument deletes files; point it at a scratch root).

.PARAMETER GoRoot
    The TARGET release's GOROOT -- the release the reconvert ran against, and the one whose file
    selection decides every DELETE row.

.PARAMETER ExpectGo
    The release -GoRoot must report, e.g. go1.24.13. The run REFUSES before printing any table when
    `go version` under -GoRoot says anything else. A deletion pass aimed at the wrong release deletes
    the wrong files, so this is a refusal and not a warning.

.PARAMETER SourceGoRoot
    The SOURCE release's GOROOT -- the release the OUTGOING corpus was converted from. It answers the
    only question that makes a file a candidate at all: is this directory something the converter
    EMITS? Mandatory, because a pass that cannot ask it offers the hand-written runtime for deletion
    (see the NOT-A-CONVERSION-TARGET note above). Named -SourceGoRoot against -GoRoot rather than
    renaming the pair -From/-To (handown-census.ps1's spelling) so the target parameter that already
    ships keeps its name.

.PARAMETER ExpectSourceGo
    The release -SourceGoRoot must report. DERIVED when omitted, from <GoStdLibVersion> in
    src\version.props -- the property of record for the release the committed corpus was converted
    from -- so the expectation cannot go stale independently of the corpus. Pass it explicitly when
    the pin has already moved ahead of the staging root you are classifying.

.PARAMETER SentinelTime
    The reconvert's start instant. Files modified before it were seeded; files modified at or after it
    were emitted by the run. Mutually exclusive with -Sentinel.

.PARAMETER Sentinel
    A file whose modification time IS the reconvert's start instant (create it immediately before the
    conversion). Mutually exclusive with -SentinelTime. A missing sentinel file is a refusal.

.PARAMETER Goos
    The GOOS the reconvert targeted, for files that sit FLAT in a package directory. Files inside an
    L3 per-GOOS folder (core\<pkg>\{windows,linux,darwin}\) are asked under THAT folder's flavour
    regardless. Defaults to this host's flavour.

.PARAMETER Goarch
    The GOARCH to ask Go under. Defaults to this host's architecture. Stated explicitly rather than
    inherited so two runs on two boxes cannot disagree silently.

.PARAMETER BuildTags
    THE CONVERTER'S build tags, because the converter is the authority on what it selected (coordinator
    ruling 9c07f494f §1). Defaults to the converter's own -stdlib default, purego and math_big_pure_go.
    An empty set is REFUSED, not honoured: the corpus is emitted under those tags, and resolving its file
    selection under none is the defect this parameter exists to close -- it deleted five live files
    before it was here. The value is checked three ways: non-empty, equal to -TagLine's parsed set when
    one is given, and PROVED to reach `go list` by a control that requires a known tag-sensitive
    package's selection to differ with and without it.

.PARAMETER TagLine
    The converter's own printed tag line, verbatim, e.g.
        Applying build tags: purego,math_big_pure_go (default; pass -tags to override)
    Optional. When supplied, the tags parsed from it must EQUAL -BuildTags or the run refuses --
    preferring neither, because a silent preference is how two tag resolutions got into the pipeline.
    The runbook's H5 amendment makes this line the step's record; this is where the record is compared
    against the instrument by a machine instead of by a reader.

.PARAMETER EmissionRoot
    The RAW per-target output the converter just wrote -- NOT the seeded scratch. Optional, and a
    strengthening rather than a prerequisite: the deselection gate decides correctly without it. When
    supplied, presence in it is direct evidence the converter emitted a file, which modification time
    cannot give (needToWriteFile skips an identical-bytes write, so an emitted-unchanged file keeps its
    seed stamp and reads SEEDED exactly like an abandoned one -- that is why the mtime arm did not save
    the five). REFUSED if it looks seeded (golib, or any hand-own marker), because in a seeded tree every
    committed file is present by the seed, "absent from the emission" can never be true, and
    DELETE-DESELECTED would report a VACUOUS zero that reads exactly like the fix working.

.PARAMETER Apply
    Perform the deletions. WITHOUT it this is a DRY RUN: it prints the table and the counts and
    deletes nothing.

.OUTPUTS
    Exit 0  -- classified, no UNRESOLVED and no UNEXPLAINED-DESELECTION rows (and, with -Apply, the
               DELETE rows are gone).
    Exit 2  -- classified, and something needs a human: UNRESOLVED rows, UNEXPLAINED-DESELECTION rows,
               or the trespass assertion fired. NOTHING WAS DELETED -- every check that can produce this
               code runs before the deletion loop. UNEXPLAINED-DESELECTION produces it WITHOUT -Apply
               too, unlike UNRESOLVED: that class says the instrument's view of the release pair is
               incoherent, and every other row in the run rests on the same view, so the dry run's own
               counts are not quotable.
    Exit 3  -- refused before classifying anything (bad root, missing/ambiguous sentinel, wrong
               release, unusable toolchain), or a deletion aborted part-way (which says so, and says
               how many files had already been removed).

    Explicit exit codes rather than `throw`, because the exit CODE is the property a caller gates on
    and a throw leaves it to the host (CLAUDE.md, false-green route #6).

.EXAMPLE
    # Dry run, the normal first invocation.
    .\reconvert-deletions.ps1 -Root D:\scratch\h5\src -GoRoot C:\sdk\go1.24.13 `
                              -ExpectGo go1.24.13 -SourceGoRoot C:\sdk\go1.23.12 `
                              -Sentinel D:\scratch\h5\run.stamp

.EXAMPLE
    # Same classification, then delete exactly the DELETE-* rows.
    .\reconvert-deletions.ps1 -Root D:\scratch\h5\src -GoRoot C:\sdk\go1.24.13 `
                              -ExpectGo go1.24.13 -SourceGoRoot C:\sdk\go1.23.12 `
                              -Sentinel D:\scratch\h5\run.stamp -Apply

.NOTES
    Requires PowerShell 5.1 (Windows) or PowerShell 7+ (any platform). Deliberately ASCII-only: a
    BOM-less .ps1 carrying a non-ASCII literal is re-decoded by 5.1's parser under the system codepage
    and the literal silently mojibakes at PARSE time (CLAUDE.md). Keeping the source ASCII removes the
    trap rather than papering it with a BOM.

    Written for docs/GoCorpusMigration.md H5. See docs/phase4/REHEARSAL-h5-go124.md section 3 for the
    measurement that motivated it.
#>

[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)][string]   $Root,
    [Parameter(Mandatory = $true)][string]   $GoRoot,
    [Parameter(Mandatory = $true)][string]   $ExpectGo,
    [Parameter(Mandatory = $true)][string]   $SourceGoRoot,
    [string]                                 $ExpectSourceGo,
    [datetime]                               $SentinelTime,
    [string]                                 $Sentinel,
    [string]                                 $Goos,
    [string]                                 $Goarch,
    [string[]]                               $Orphan = @(),
    # THE CONVERTER'S tag set, because the converter is the authority on what it selected (coordinator
    # ruling 9c07f494f §1). The default MIRRORS defaultStdLibBuildTags (commandLineOptions.go), which a
    # mirrored constant can always drift from -- so it is not trusted on its own: -TagLine cross-checks
    # it against what the converter PRINTED, and Assert-TagsReachedGoList refuses if the set never
    # reached `go list` at all. An EMPTY set is refused rather than defaulted, because "no tags" is the
    # exact defect this parameter exists to close.
    [string[]]                               $BuildTags = @('purego', 'math_big_pure_go'),
    # The converter's own printed line, verbatim, e.g.
    #   "Applying build tags: purego,math_big_pure_go (default; pass -tags to override)"
    # Optional; when given, the tags parsed out of it must EQUAL -BuildTags or the run refuses. The
    # runbook's H5 amendment makes this line the step's record, so this is where the record is checked
    # against the instrument instead of against a reader's memory.
    [string]                                 $TagLine,
    # The RAW per-target emission the converter just wrote -- NOT the seeded scratch. Optional, and a
    # STRENGTHENING rather than a prerequisite: when supplied and proven unseeded, presence in it is
    # direct evidence the converter emitted the file, which the modification-time arm cannot give
    # (needToWriteFile, projectFileWriter.go:663, skips an identical-bytes write, so an
    # emitted-unchanged file keeps its seed stamp and reads SEEDED exactly like an abandoned one).
    [string]                                 $EmissionRoot,
    [switch]                                 $Apply
)

Set-StrictMode -Version 2.0
$ErrorActionPreference = 'Stop'

. (Join-Path $PSScriptRoot '_paths.ps1')

# ---------------------------------------------------------------------------------------------
# Refusals. Every one of these runs BEFORE a single file is classified, so a refused run cannot be
# mistaken for a clean one: it prints no table at all.
# ---------------------------------------------------------------------------------------------

function Deny {
    param([Parameter(Mandatory = $true)][string] $Message)

    Write-Host ''
    Write-Host "REFUSED: $Message" -ForegroundColor Red
    Write-Host 'Nothing was classified and nothing was deleted.'
    exit 3
}

# Post-classification refusal. Distinct from Deny because the two make DIFFERENT promises: Deny says
# nothing was read, this says nothing was DELETED. Both are checked before the Remove-Item loop.
function Stop-ForReview {
    param([Parameter(Mandatory = $true)][string] $Message)

    Write-Host ''
    Write-Host "STOPPED: $Message" -ForegroundColor Yellow
    Write-Host 'NOTHING WAS DELETED. Dispose of the rows above, then re-run.'
    exit 2
}

if (-not (Test-Path -LiteralPath $Root -PathType Container)) {
    Deny "-Root does not exist or is not a directory: $Root"
}

$RootFull = (Resolve-Path -LiteralPath $Root).Path
$CoreDir  = Join-Path $RootFull 'core'

if (-not (Test-Path -LiteralPath $CoreDir -PathType Container)) {
    Deny "-Root has no core\ directory: $CoreDir -- pass the scratch SRC root (the one holding core\), not the core dir itself"
}

# The repository's own corpus is never a deletion target. This instrument exists to run against a
# throwaway reconvert root; pointing it at src\core would delete tracked files that a seeded root's
# arithmetic says are stale, on a tree where they are not.
$RepoCore = Join-Path $SrcRoot 'core'

if ($CoreDir.TrimEnd('\', '/') -ieq $RepoCore.TrimEnd('\', '/')) {
    Deny "-Root resolves to the REPOSITORY corpus ($RepoCore). This pass runs against a scratch reconvert root only."
}

# Exactly one sentinel form. Both, or neither, is ambiguous -- and an absent sentinel would make
# every file on disk look seeded, i.e. would offer the whole corpus for deletion.
$haveTime = $PSBoundParameters.ContainsKey('SentinelTime')
$haveFile = -not [string]::IsNullOrWhiteSpace($Sentinel)

if ($haveTime -and $haveFile) {
    Deny 'pass exactly one of -SentinelTime or -Sentinel, not both'
}

if (-not $haveTime -and -not $haveFile) {
    Deny 'pass -SentinelTime <datetime> or -Sentinel <path> -- the reconvert start stamp. Without it every file reads SEEDED.'
}

if ($haveFile) {
    if (-not (Test-Path -LiteralPath $Sentinel -PathType Leaf)) {
        Deny "-Sentinel file is missing: $Sentinel"
    }

    $SentinelStamp = (Get-Item -LiteralPath $Sentinel).LastWriteTimeUtc
}
else {
    $SentinelStamp = $SentinelTime.ToUniversalTime()
}

# The SOURCE release expectation, DERIVED rather than restated. <GoStdLibVersion> in version.props is
# the property of record for the release the committed corpus was converted from, and the reconvert
# being classified started from THAT corpus -- so deriving it here means the expectation cannot drift
# from the tree independently. Comments are stripped first because version.props' own prose names the
# element while explaining it (the _paths.ps1 $NetVersion precedent, same trap, same remedy). There is
# deliberately no fallback: an instrument that cannot know the source release must say so.
if ([string]::IsNullOrWhiteSpace($ExpectSourceGo)) {
    $VersionProps = Join-Path $SrcRoot 'version.props'

    if (-not (Test-Path -LiteralPath $VersionProps -PathType Leaf)) {
        Deny "cannot derive -ExpectSourceGo: the corpus release's property of record is missing at $VersionProps -- pass -ExpectSourceGo explicitly"
    }

    $stdLibVersionMatch = [regex]::Match(
        ([System.IO.File]::ReadAllText($VersionProps) -replace '(?s)<!--.*?-->', ''),
        '<GoStdLibVersion(?:\s[^>]*)?>\s*([^<\s]+)\s*</GoStdLibVersion>')

    if (-not $stdLibVersionMatch.Success) {
        Deny "cannot derive -ExpectSourceGo from $VersionProps -- expected a <GoStdLibVersion>...</GoStdLibVersion> element. Pass -ExpectSourceGo explicitly."
    }

    $ExpectSourceGo       = 'go' + $stdLibVersionMatch.Groups[1].Value
    $ExpectSourceGoOrigin = "derived from <GoStdLibVersion> in $VersionProps"
}
else {
    $ExpectSourceGoOrigin = 'passed as -ExpectSourceGo'
}

if ([string]::IsNullOrWhiteSpace($Goos))   { $Goos   = $HostGoos }
if ([string]::IsNullOrWhiteSpace($Goarch)) { $Goarch = $HostGoarch }

if ([string]::IsNullOrWhiteSpace($Goos)) {
    Deny 'cannot derive the target GOOS on this host -- pass -Goos explicitly'
}

# The environment every `go` child runs under. GOROOT and GOTOOLCHAIN=local pin the release (an
# `auto` toolchain would silently switch and answer about a release nobody named); CGO_ENABLED=0 is
# the corpus's own emission state, and it CHANGES the selected file set for cgo-conditional packages;
# GOWORK=off and an empty GOFLAGS stop an ambient workspace or flag from moving the answer.
$GoEnvBase = @{
    'GOTOOLCHAIN'  = 'local'
    'CGO_ENABLED'  = '0'
    'GOWORK'       = 'off'
    'GOFLAGS'      = ''
    'GO111MODULE'  = ''
}

# A toolchain is a VALUE here rather than a pair of script-scope variables, because there are now two
# of them (source and target) and every `go` child must be unambiguous about which one answered. A
# bare `go` off PATH is never used: it resolves to whatever the ambient release happens to be and
# would answer the file-selection question about the wrong corpus.
function New-Toolchain {
    param(
        [Parameter(Mandatory = $true)][string] $GoRootPath,
        [Parameter(Mandatory = $true)][string] $Label,
        [Parameter(Mandatory = $true)][string] $ParameterName
    )

    if (-not (Test-Path -LiteralPath $GoRootPath -PathType Container)) {
        Deny "$Label GOROOT does not exist or is not a directory: $GoRootPath (pass $ParameterName)"
    }

    $full = (Resolve-Path -LiteralPath $GoRootPath).Path
    $exe  = Join-Path $full "bin/go$ExeSuffix"

    if (-not (Test-Path -LiteralPath $exe -PathType Leaf)) {
        Deny "no Go toolchain at $exe (pass $ParameterName <the $Label release's GOROOT>)"
    }

    $srcDir = Join-Path $full 'src'

    if (-not (Test-Path -LiteralPath $srcDir -PathType Container)) {
        Deny "$Label GOROOT has no src\ directory: $srcDir"
    }

    return [pscustomobject]@{ Label = $Label; Root = $full; Exe = $exe; SrcDir = $srcDir }
}

$TargetGo = New-Toolchain -GoRootPath $GoRoot       -Label 'target' -ParameterName '-GoRoot'
$SourceGo = New-Toolchain -GoRootPath $SourceGoRoot -Label 'source' -ParameterName '-SourceGoRoot'

function Invoke-Go {
    param(
        [Parameter(Mandatory = $true)][object]   $Toolchain,
        [Parameter(Mandatory = $true)][string[]] $Arguments,
        [Parameter(Mandatory = $true)][string]   $ForGoos
    )

    $saved = @{}
    $vars  = @{}

    foreach ($key in $GoEnvBase.Keys) { $vars[$key] = $GoEnvBase[$key] }

    $vars['GOROOT'] = $Toolchain.Root
    $vars['GOOS']   = $ForGoos
    $vars['GOARCH'] = $Goarch

    foreach ($key in $vars.Keys) {
        $saved[$key] = [System.Environment]::GetEnvironmentVariable($key)
        [System.Environment]::SetEnvironmentVariable($key, $vars[$key])
    }

    $previousLocation = (Get-Location).Path

    # A FAILING `go list` is not an error here -- it is the EVIDENCE that decides DELETE-ABSENT and
    # that tells an L3 per-GOOS folder from a real package whose leaf is a GOOS name. Under the
    # script's `Stop` preference a native command's stderr line becomes a terminating
    # NativeCommandError (the r41 trap CLAUDE.md documents for the converter's own WARNINGs), so the
    # preference is scoped to `Continue` across the invocation and the EXIT CODE is read instead.
    $previousPreference = $ErrorActionPreference
    $ErrorActionPreference = 'Continue'

    try {
        # From the toolchain's own GOROOT\src so a std import path resolves with no module context
        # of the caller's.
        Set-Location -LiteralPath $Toolchain.SrcDir

        $exe    = $Toolchain.Exe
        $output = & $exe @Arguments 2>&1
        # Captured BEFORE anything else touches $LASTEXITCODE (CLAUDE.md: a pipe reports the pipe's).
        $code   = $LASTEXITCODE
    }
    finally {
        $ErrorActionPreference = $previousPreference

        Set-Location -LiteralPath $previousLocation

        foreach ($key in $saved.Keys) {
            [System.Environment]::SetEnvironmentVariable($key, $saved[$key])
        }
    }

    return [pscustomobject]@{
        ExitCode = $code
        Text     = ($output | Out-String)
    }
}

# The release gates, one per toolchain. PRINTING a pin is not CHECKING it (CLAUDE.md); this compares
# and refuses. Both are gated: a deletion pass whose SOURCE release is wrong misjudges which
# directories the converter emits into, which is the class of error the NOT-A-CONVERSION-TARGET arm
# exists to prevent.
function Assert-Release {
    param(
        [Parameter(Mandatory = $true)][object] $Toolchain,
        [Parameter(Mandatory = $true)][string] $Expected
    )

    $probe = Invoke-Go -Toolchain $Toolchain -Arguments @('version') -ForGoos $Goos

    if ($probe.ExitCode -ne 0) {
        Deny "``go version`` under $($Toolchain.Root) failed (exit $($probe.ExitCode)): $($probe.Text.Trim())"
    }

    $text = $probe.Text.Trim()

    if ($text -notmatch ('(^|\s)' + [regex]::Escape($Expected) + '(\s|$)')) {
        Deny "$($Toolchain.Label) toolchain mismatch -- expected '$Expected' and $($Toolchain.Exe) reports '$text'"
    }

    return $text
}

$versionText       = Assert-Release -Toolchain $TargetGo -Expected $ExpectGo
$sourceVersionText = Assert-Release -Toolchain $SourceGo -Expected $ExpectSourceGo

# ---------------------------------------------------------------------------------------------
# The tag set: ONE resolution, the converter's, checked three ways before it is used.
# ---------------------------------------------------------------------------------------------

$BuildTags = @($BuildTags | Where-Object { -not [string]::IsNullOrWhiteSpace($_) } | ForEach-Object { $_.Trim() })

# An EMPTY set is the defect, not a default. Refused here rather than silently meaning "no tags",
# because "no tags" is exactly what this instrument did while the converter used two.
if ($BuildTags.Count -eq 0) {
    Deny "-BuildTags resolved to an EMPTY set. The corpus is emitted under the converter's -stdlib default (purego, math_big_pure_go); classifying it under no tags is the defect this parameter closes. Pass the tag set the converter PRINTED."
}

$BuildTagsLabel = ($BuildTags -join ',')

# CROSS-CHECK against the converter's own printed line when one was supplied. The runbook's H5 amendment
# makes that line the step's record; this is the one place the record and the instrument are compared by
# a machine rather than by a reader. Parsed rather than matched whole, because the line carries a
# parenthetical whose wording is not ours to depend on.
if (-not [string]::IsNullOrWhiteSpace($TagLine)) {
    $tagLineMatch = [regex]::Match($TagLine, 'Applying build tags:\s*([^\s(]+)')

    if (-not $tagLineMatch.Success) {
        Deny "-TagLine does not contain a parsable ``Applying build tags: <set>`` -- pass the converter's printed line verbatim, or omit -TagLine"
    }

    $printedTags   = @($tagLineMatch.Groups[1].Value -split ',' | Where-Object { $_ -ne '' } | ForEach-Object { $_.Trim() })
    $printedSorted = (($printedTags | Sort-Object) -join ',')
    $passedSorted  = (($BuildTags   | Sort-Object) -join ',')

    if ($printedSorted -ne $passedSorted) {
        Deny "TAG SET DISAGREEMENT -- the converter printed [$printedSorted] and this run was given [$passedSorted]. The converter is the authority; re-run with its set. Refusing rather than preferring either, because a silent preference is how two tag resolutions got into the pipeline in the first place."
    }

    Write-Host "  build tags        [$BuildTagsLabel]   CONFIRMED against the converter's printed line"
}
else {
    Write-Host "  build tags        [$BuildTagsLabel]   (no -TagLine supplied, so NOT confirmed against the converter's own output)"
}

# ⚠ THE FALSIFYING CONTROL, and the one that would have caught tonight's defect on its first run. Every
# assertion above is about the VALUE of the tag set; none of them proves the value ever reached `go list`.
# It did not before: the argument was simply absent, and $GoEnvBase empties GOFLAGS so the tags could not
# have arrived by any other route either -- every reading was silently tagless while a banner would have
# said "purego". So ask a package whose selection is KNOWN to turn on these tags, both ways, and require
# the two answers to DIFFER. A gate that cannot go red proves nothing; this one reddens on precisely the
# failure it exists for.
function Assert-TagsReachedGoList {
    # crypto/md5 rather than a purego-only leaf: its split is over a decade old, the package exists and
    # is in `go list std` at every release in scope, so a refusal here means the tags did not arrive
    # rather than that the canary itself moved.
    $canaryPackage = 'crypto/md5'
    $canaryFlavour = $(if ([string]::IsNullOrWhiteSpace($Goos)) { 'linux' } else { ($Goos -split ',')[0].Trim() })

    $withTags = Invoke-Go -Toolchain $TargetGo -Arguments @('list', '-tags', $BuildTagsLabel, '-f', '{{.GoFiles}}', $canaryPackage) -ForGoos $canaryFlavour
    $without  = Invoke-Go -Toolchain $TargetGo -Arguments @('list', '-f', '{{.GoFiles}}', $canaryPackage) -ForGoos $canaryFlavour

    if ($withTags.ExitCode -ne 0 -or $without.ExitCode -ne 0) {
        Deny "the tag control could not run: ``go list $canaryPackage`` failed at $ExpectGo (GOOS=$canaryFlavour) with exit $($withTags.ExitCode)/$($without.ExitCode). A control that did not execute is not a control."
    }

    $taggedText   = $withTags.Text.Trim()
    $untaggedText = $without.Text.Trim()

    if ([string]::IsNullOrWhiteSpace($taggedText) -or [string]::IsNullOrWhiteSpace($untaggedText)) {
        Deny "the tag control read an EMPTY selection for $canaryPackage on one or both arms -- a comparison against nothing reports agreement for the wrong reason."
    }

    if ($taggedText -eq $untaggedText) {
        Deny "TAG CONTROL FAILED: ``go list`` answered IDENTICALLY for $canaryPackage with tags [$BuildTagsLabel] and with none, so the tag set is NOT reaching ``go list`` and every selection below would be resolved under the wrong tags. This is the exact defect coordinator ruling 9c07f494f was cut for; refusing rather than classifying."
    }

    Write-Host "  tag control       $canaryPackage selection DIFFERS with and without [$BuildTagsLabel] -- the tags reach ``go list``"
}

Assert-TagsReachedGoList


# ---------------------------------------------------------------------------------------------
# Predicates.
# ---------------------------------------------------------------------------------------------

# The LINE-ANCHORED marker scan CLAUDE.md's corpus mechanics mandate, matching the converter's own
# two accepted spellings (with or without the `go.` qualifier, with or without the Attribute suffix,
# per containsModuleMarker in directiveOperations.go). Unanchored, this over-counts by every bodyless
# partial placeholder comment that merely MENTIONS the marker (~63 against the real 40-odd).
$HandOwnMarkerPattern = '(?m)^\s*\[\s*module\s*:\s*(go\.)?\s*GoManualConversion(Attribute)?\s*\]'

# MSBuild's directories, not the conversion's -- mirrors isBuildOutputDirectory (platformCensus.go).
$BuildOutputDirs = @('bin', 'obj', 'Generated')

$KnownGoos = @('windows', 'linux', 'darwin')

# The converter's OWN skip-list, MIRRORED. `go list std` names unsafe and testing like any other
# package, so no Go question can exclude them -- the converter's isNonConvertedStdLibPackage
# (src\go2cs\stdLibConverter.go) is the only authority, and this is a copy of it. A copy DRIFTS, so
# reconvertDeletionsSkipList_test.go extracts the literal below out of this file and compares it to
# the converter's set under the plain `go test ./...` in src\go2cs. Keep the assignment on ONE line
# and in this spelling; the guard's regex reads it.
$NonConvertedStdPackages = @('unsafe', 'builtin', 'testing', 'cmd')
$NonConvertedStdPrefixes = @('cmd/')

# The two roots under core\ that are the REPOSITORY's own C#, not converter output: golib (the
# hand-written runtime) and go2cs (the Symbols shared project). These are LITERALS and cannot be
# anything else -- they correspond to no Go import path at any release, so no `go list` at either end
# can name them, and they carry no [module: GoManualConversion] marker because nothing ever converts
# into them and there is no generated body for a marker to displace. That is exactly why the marker
# guard could not see them and why R's first dry run offered all 117 of their files for deletion.
$HandWrittenRoots = @('golib', 'go2cs')

function Test-HandOwnMarker {
    param([Parameter(Mandatory = $true)][string] $Path)

    # ReadAllText, never Get-Content: 5.1's Get-Content decodes BOM-less UTF-8 as ANSI, and this file
    # is only ever read (never written), so the round-trip damage would be silent.
    $text = [System.IO.File]::ReadAllText($Path)

    return [regex]::IsMatch($text, $HandOwnMarkerPattern)
}

# ---------------------------------------------------------------------------------------------
# The emission index: direct evidence of what the converter SELECTED, when a raw emission root was
# supplied. $null when it was not, and every consumer tests for $null -- the deselection gate stands on
# its own, so this is a STRENGTHENING and never a prerequisite.
# ---------------------------------------------------------------------------------------------

$EmissionIndex = $null

if (-not [string]::IsNullOrWhiteSpace($EmissionRoot)) {
    if (-not (Test-Path -LiteralPath $EmissionRoot -PathType Container)) {
        Deny "-EmissionRoot does not exist or is not a directory: $EmissionRoot"
    }

    $emissionFull = (Resolve-Path -LiteralPath $EmissionRoot).Path
    $emissionCs   = @(Get-ChildItem -LiteralPath $emissionFull -Recurse -File -Filter '*.cs' -ErrorAction SilentlyContinue)

    # Population first: an index built from nothing would mark every corpus file "not emitted" and hand
    # the gate a corpus-wide candidate set.
    if ($emissionCs.Count -lt 100) {
        Deny "-EmissionRoot holds only $($emissionCs.Count) .cs file(s) -- a stdlib emission is thousands. Refusing rather than reading an empty or wrong root as 'the converter emitted nothing'."
    }

    # ⚠ AND THE PRECONDITION THAT MAKES THE WHOLE ARM MEAN ANYTHING: the root must be a RAW EMISSION and
    # not a SEEDED tree. CLAUDE.md's floor 2 requires the reconvert scratch to be seeded from src\core,
    # and in a seeded tree every committed file is present BY THE SEED -- so "absent from the emission" is
    # false for everything, arm 1 saves every row including the abandoned ones, and DELETE-DESELECTED
    # reports a clean ZERO because its predicate can no longer fire. That is a vacuous green of exactly
    # the kind this package keeps finding, and it would read as the fix working.
    #
    # The discriminator is what a seed carries and an emission cannot: golib (hand-written, never
    # converted into) and the [module: GoManualConversion] whole-file replacements.
    $seedTell = @()

    if (Test-Path -LiteralPath (Join-Path $emissionFull 'core/golib') -PathType Container) { $seedTell += 'core/golib' }
    if (Test-Path -LiteralPath (Join-Path $emissionFull 'golib')      -PathType Container) { $seedTell += 'golib' }

    $markedInEmission = @($emissionCs | Where-Object { Test-HandOwnMarker -Path $_.FullName } | Select-Object -First 1)

    if ($markedInEmission.Count -gt 0) { $seedTell += '[module: GoManualConversion] file(s)' }

    # ⚠ THE BASE IS FOUND, NEVER COMPOSED -- i9's arm-3 trap, banked by the coordinator at 9c07f494f §2:
    # a path that does not exist answers ABSENT, which is indistinguishable from a real absence and points
    # the same way the hypothesis does. A staging root is `<stage>/<target>/src/core/...`, so composing
    # `$EmissionRoot/core` would silently miss by one segment and make every lookup fail.
    $coreCandidates = @(Get-ChildItem -LiteralPath $emissionFull -Recurse -Directory -Filter 'core' -ErrorAction SilentlyContinue |
                        Where-Object { $_.Parent.Name -eq 'src' -or $_.Parent.FullName -eq $emissionFull })

    if ($coreCandidates.Count -gt 1) {
        Deny ("-EmissionRoot holds {0} directories named 'core' ({1}) and this instrument will not guess which one the corpus corresponds to. Pass the root whose immediate child is 'src' or 'core'." -f $coreCandidates.Count, (($coreCandidates | ForEach-Object { $_.FullName }) -join ', '))
    }

    $emissionBase = $(if ($coreCandidates.Count -eq 1) { $coreCandidates[0].FullName } else { $emissionFull })

    # ⚠ AND THE SPELLING IS BORROWED, NOT REINVENTED. Row identity is Get-RelativeDisplayPath's output --
    # FORWARD slashes, relative to core\ -- and an index keyed any other way makes arm 1 a DEAD ARM that
    # reports nothing and refuses nothing. Calling the same helper is what keeps the two spellings from
    # drifting; the first cut of this block built the keys with DirectorySeparatorChar and would have
    # matched nothing at all on Windows, with no symptom beyond the arm quietly never firing.
    $EmissionIndex = New-Object 'System.Collections.Generic.HashSet[string]' ([System.StringComparer]::OrdinalIgnoreCase)

    foreach ($emitted in $emissionCs) {
        if (-not $emitted.FullName.StartsWith($emissionBase, [System.StringComparison]::OrdinalIgnoreCase)) { continue }

        $null = $EmissionIndex.Add((Get-RelativeDisplayPath -Path $emitted.FullName -Root $emissionBase))
    }

    # ⚠ THE ANTI-VACUITY CONTROL, and the one that catches a base or spelling mismatch AT RUNTIME rather
    # than by reading the code. Two trees that describe the same corpus must SHARE PATHS; an overlap of
    # zero means the join is broken, and a broken join makes arm 1 unable to save anything while looking
    # exactly like "the converter emitted none of these". Scored against the corpus's own .cs, in the
    # corpus's own spelling, before a single row is classified.
    $corpusCs = @(Get-ChildItem -LiteralPath $CoreDir -Recurse -File -Filter '*.cs' -ErrorAction SilentlyContinue)
    $overlap  = 0

    foreach ($corpusFile in $corpusCs) {
        if ($EmissionIndex.Contains((Get-RelativeDisplayPath -Path $corpusFile.FullName -Root $CoreDir))) { $overlap++ }
    }

    if ($corpusCs.Count -eq 0) {
        Deny "the emission-overlap control found NO .cs under $CoreDir, so it could not be scored. A control that ran over an empty population is not a control."
    }

    if ($overlap -eq 0) {
        Deny ("EMISSION JOIN BROKEN: none of the {0} corpus .cs under {1} is present in the {2}-entry emission index built from {3}. The two trees share no path, so the emission arm could never fire and every row would fall through to the selection arms unprotected. The base is almost certainly at the wrong level -- pass the root whose child is 'src' or 'core'." -f $corpusCs.Count, $CoreDir, $EmissionIndex.Count, $emissionBase)
    }

    # ⚠ THE SEED-TELL REFUSAL IS HELD UNTIL HERE, DELIBERATELY (ruling R4, on C2's offer 2d2d74b471 §4).
    # It used to fire immediately after $seedTell was built, ABOVE the base/index/overlap block -- which
    # made EMISSION JOIN BROKEN unreachable on any seeded root, so plant 5 could only ever be measured
    # with a raw-emitted root that joins nothing, and no lane has one. Both boxes that planted it hit
    # that shadow.
    #
    # Holding the refusal until the overlap is scored costs nothing -- it still exits before a single row
    # is classified -- and buys two things. The join reading is carried in the refusal's OWN text, so ONE
    # seeded run yields BOTH readings. And the two more fundamental refusals above now fire FIRST: an
    # empty corpus population and a zero overlap both make the seed-tell reading itself meaningless,
    # because you cannot say a root "looks seeded" from an index that shares no path with the corpus. A
    # check asserts its input population is non-empty before its verdict means anything.
    #
    # The two Write-Host lines below stay BELOW this refusal, so "(proved raw: no golib, no hand-own
    # marker)" is still reached only when $seedTell is EMPTY. That claim's truth is unchanged.
    if ($seedTell.Count -gt 0) {
        Deny ("-EmissionRoot looks SEEDED rather than raw-emitted (found: {0}). In a seeded tree every committed file is present by the seed, so 'absent from the emission' can never be true and DELETE-DESELECTED would report a VACUOUS zero. Pass the converter's own per-target output root, or omit -EmissionRoot and let the two-release selection gate decide alone. EMISSION JOIN, read BEFORE this refusal: {1} of {2} corpus .cs found in the {3}-entry emission index built from {4}." -f ($seedTell -join ', '), $overlap, $corpusCs.Count, $EmissionIndex.Count, $emissionBase)
    }

    Write-Host "  emission index    $($EmissionIndex.Count) emitted .cs, base $emissionBase (proved raw: no golib, no hand-own marker)"
    Write-Host ("  emission join     {0} of {1} corpus .cs found in the emission index -- the arm is LIVE" -f $overlap, $corpusCs.Count)
}
else {
    Write-Host '  emission index    NOT SUPPLIED -- the deselection gate decides alone (pass -EmissionRoot to add the converter''s own answer)'
}

# The SOURCE release's std package set, one `go list std` per flavour, cached. This is the set the
# converter's -stdlib driver walks, so membership in it IS "the converter emits into this directory".
# It is fetched per GOOS because std membership is flavour-dependent: crypto/x509/internal/macos is in
# std under darwin and absent under windows, internal/runtime/syscall is in std under linux and absent
# under windows -- and each of those has an L3 per-GOOS folder in the corpus that a windows-only set
# would misclassify.
$SourceStdCache = @{}

function Get-SourceStdSet {
    param([Parameter(Mandatory = $true)][string] $ForGoos)

    if ($SourceStdCache.ContainsKey($ForGoos)) {
        return $SourceStdCache[$ForGoos]
    }

    $result = Invoke-Go -Toolchain $SourceGo -Arguments @('list', 'std') -ForGoos $ForGoos

    if ($result.ExitCode -ne 0) {
        Deny "``go list std`` under the source release ($($SourceGo.Root), GOOS=$ForGoos) failed (exit $($result.ExitCode)): $($result.Text.Trim())"
    }

    $set = New-Object 'System.Collections.Generic.HashSet[string]'

    foreach ($line in ($result.Text -split "`r?`n")) {
        $trimmed = $line.Trim()
        if ($trimmed -ne '') { $null = $set.Add($trimmed) }
    }

    # An EMPTY set would make every file NOT-A-CONVERSION-TARGET -- the safe direction for a deletion
    # pass, and therefore exactly the way this instrument could go silently vacuous: it would delete
    # nothing and report a clean run. A std list is ~300 packages; anything under 100 is a broken
    # probe, not a small standard library.
    if ($set.Count -lt 100) {
        Deny "``go list std`` under the source release (GOOS=$ForGoos) returned only $($set.Count) package(s) -- a std list is ~300. Refusing rather than classifying every file as not-a-conversion-target."
    }

    $SourceStdCache[$ForGoos] = $set

    return $set
}

# Cache: one `go list` per (release, importpath, goos). A full corpus is ~350 packages x 1 flavour;
# without this it would be one process per FILE. The RELEASE is part of the key because the
# deselection gate below asks the same question at BOTH GOROOTs, and a cache keyed only on
# (importpath, goos) would answer the second question with the first release's reading.
$SelectionCache = @{}

# ⚠ THE TAG ARGUMENT IS THE WHOLE POINT OF THIS FUNCTION'S SIGNATURE (coordinator ruling 9c07f494f,
# i9's measurement bb3a1a747). It previously ran `go list` with NO -tags while $GoEnvBase empties
# GOFLAGS, so the instrument resolved the corpus's file selection under the EMPTY tag set while the
# converter had emitted it under `purego, math_big_pure_go` -- two components resolving tags twice, and
# the deletion pass was the one that did not know. Measured at go1.24.13, that disagreement moves
# NINETEEN principals per flavour (identical list on windows, linux and darwin), of which the corpus
# carried seven at their exact path; five reached the deletion loop and were removed. There is now ONE
# tag set in the instrument, it is the converter's, and it is the same set at both releases.
function Get-GoSelection {
    param(
        [Parameter(Mandatory = $true)][string] $ImportPath,
        [Parameter(Mandatory = $true)][string] $ForGoos,
        # Which release to ask. Defaults to the target, which is every caller except the deselection
        # gate -- so an un-updated caller keeps its old meaning rather than silently changing release.
        [Parameter(Mandatory = $false)][object] $Toolchain
    )

    if ($null -eq $Toolchain) { $Toolchain = $TargetGo }

    $key = "$($Toolchain.Label)|$ForGoos|$ImportPath"

    if ($SelectionCache.ContainsKey($key)) {
        return $SelectionCache[$key]
    }

    $listArgs = @('list')

    # Never emit a bare `-tags` with an empty value: `-tags=` is an EXPLICIT empty set to the go tool,
    # which is the defect wearing a different spelling. $BuildTags is asserted non-empty at startup, so
    # this is belt-and-braces at the one place the argument is built.
    if ($BuildTags.Count -gt 0) { $listArgs += @('-tags', ($BuildTags -join ',')) }

    $listArgs += @('-f', '{{.GoFiles}} {{.CgoFiles}}', $ImportPath)

    $result = Invoke-Go -Toolchain $Toolchain -Arguments $listArgs -ForGoos $ForGoos

    $selection = [pscustomobject]@{
        PackageExists = ($result.ExitCode -eq 0)
        Files         = New-Object 'System.Collections.Generic.HashSet[string]'
        Diagnostic    = $result.Text.Trim()
    }

    if ($selection.PackageExists) {
        foreach ($token in ($result.Text -replace '[\[\]]', ' ' -split '\s+')) {
            if ($token -ne '') { $null = $selection.Files.Add($token) }
        }
    }

    $SelectionCache[$key] = $selection

    return $selection
}

# Resolve a candidate's package import path and flavour from its location under core\.
#
# THE AMBIGUITY THIS HANDLES, and why it is not a name test. Layout L3 puts a package's
# platform-varying files in core\<pkg>\{windows,linux,darwin}\ -- so `crypto\rand\windows\` is a
# per-GOOS FOLDER of package crypto/rand. But `internal\syscall\windows\` is a REAL Go package whose
# own last segment happens to be a GOOS name (measured at 1.24.13: `go list internal/syscall/windows`
# succeeds and lists twelve files, while `go list crypto/rand/windows` says "is not in std"). A
# name-only rule gets one of those two backwards. So the rule is: ask Go about the full directory
# first; only if NEITHER release knows it AND the last segment is a GOOS name is it an L3 folder.
#
# The SOURCE is consulted for that second question as well as the target, because a real package that
# was REMOVED at the target is still a real package: without it, a removed `<pkg>/<goos>` package
# would be read as an L3 folder of its parent and reported under the parent's name. Its files are a
# DELETE either way, so this sharpens the row's principal rather than changing its class -- but a row
# that names the wrong package is a row a human misreads.
function Resolve-Principal {
    param([Parameter(Mandatory = $true)][string] $RelativePath)

    $parts = $RelativePath -split '/'

    # A .cs sitting directly in core\ belongs to no package. Tested on the PART COUNT, not on the
    # length of a computed directory list: PowerShell's `0..($n - 2)` with $n = 1 is the range 0..-1,
    # which counts DOWNWARDS and yields @(0, -1) -- two indices, both resolving to the file name. The
    # landed instrument computed the list first and tested its Count, so core\GlobalUsings.cs became
    # the two-segment import path "GlobalUsings.cs/GlobalUsings.cs", which no `go list` knows, and the
    # file was reported for deletion.
    if ($parts.Count -lt 2) {
        return $null
    }

    $stem = [System.IO.Path]::GetFileNameWithoutExtension($parts[-1])
    $dirs = @($parts[0..($parts.Count - 2)])

    $fullDir = ($dirs -join '/')

    $asPackage = Get-GoSelection -ImportPath $fullDir -ForGoos $Goos

    if ($asPackage.PackageExists) {
        return [pscustomobject]@{ ImportPath = $fullDir; Goos = $Goos; Principal = "$stem.go"; Selection = $asPackage }
    }

    $leaf = $dirs[-1]

    if ($dirs.Count -ge 2 -and ($KnownGoos -contains $leaf) -and
        -not (Get-SourceStdSet -ForGoos $Goos).Contains($fullDir)) {

        $parentPath = ($dirs[0..($dirs.Count - 2)] -join '/')
        $selection  = Get-GoSelection -ImportPath $parentPath -ForGoos $leaf

        return [pscustomobject]@{ ImportPath = $parentPath; Goos = $leaf; Principal = "$stem.go"; Selection = $selection }
    }

    # Neither a package Go knows at the target nor an L3 folder: either the package is gone at the
    # target (a DELETE row) or the directory was never converter output at all (a
    # NOT-A-CONVERSION-TARGET row). The source-std test in Test-ConversionTarget tells those apart;
    # this reports it under its own path so the row reads honestly either way, with the failed lookup
    # as its selection.
    return [pscustomobject]@{ ImportPath = $fullDir; Goos = $Goos; Principal = "$stem.go"; Selection = $asPackage }
}

# Does the converter EMIT into this file's directory? Asked FIRST, of every candidate, because a
# "does Go still select the principal" answer is meaningless where the answer here is no -- which is
# how 116 golib files and Symbols.cs came to be classified DELETE-ABSENT on this instrument's first
# real run. Returns a reason and a grouping key when the answer is no.
function Test-ConversionTarget {
    param(
        [Parameter(Mandatory = $true)][string] $RelativePath,
        [Parameter(Mandatory = $false)]        $Resolved
    )

    $root = ($RelativePath -split '/')[0]

    # A PATH test, deliberately independent of anything `go list` says -- see the $HandWrittenRoots
    # note. It is also the assertion the deletion loop re-checks, so the two derivations that must
    # agree before a file is removed do not share an instrument.
    if ($HandWrittenRoots -contains $root) {
        return [pscustomobject]@{
            IsTarget = $false; Group = "$root/"
            Reason   = "hand-written repository code under $root\ -- never converter output"
        }
    }

    if ($null -eq $Resolved) {
        return [pscustomobject]@{
            IsTarget = $false; Group = '<core root>'
            Reason   = 'no package directory -- a .cs directly under core\ is not converter output'
        }
    }

    $importPath = $Resolved.ImportPath

    if ($NonConvertedStdPackages -contains $importPath) {
        return [pscustomobject]@{
            IsTarget = $false; Group = $importPath
            Reason   = "std package skip-listed by the converter (isNonConvertedStdLibPackage): $importPath"
        }
    }

    foreach ($prefix in $NonConvertedStdPrefixes) {
        if ($importPath.StartsWith($prefix, [System.StringComparison]::Ordinal)) {
            return [pscustomobject]@{
                IsTarget = $false; Group = $prefix
                Reason   = "std path skip-listed by the converter (isNonConvertedStdLibPackage): $importPath"
            }
        }
    }

    if (-not (Get-SourceStdSet -ForGoos $Resolved.Goos).Contains($importPath)) {
        return [pscustomobject]@{
            IsTarget = $false; Group = $importPath
            Reason   = "not a std package at the source release ($ExpectSourceGo, GOOS=$($Resolved.Goos)): $importPath"
        }
    }

    return [pscustomobject]@{ IsTarget = $true; Group = ''; Reason = '' }
}

# ---------------------------------------------------------------------------------------------
# Walk and classify.
# ---------------------------------------------------------------------------------------------

Write-Host ''
Write-Host '=== reconvert deletion pass ===============================================' -ForegroundColor Cyan
Write-Host "  root              $RootFull"
Write-Host "  target toolchain  $versionText"
Write-Host "  target GOROOT     $($TargetGo.Root)"
Write-Host "  source toolchain  $sourceVersionText   ($ExpectSourceGoOrigin)"
Write-Host "  source GOROOT     $($SourceGo.Root)"
Write-Host "  flavour asked     GOOS=$Goos GOARCH=$Goarch CGO_ENABLED=0"
Write-Host ("  seed sentinel     {0:yyyy-MM-dd HH:mm:ss}Z  (modified before this = seeded)" -f $SentinelStamp)
Write-Host ("  mode              {0}" -f $(if ($Apply) { 'APPLY -- deletions will be performed' } else { 'DRY RUN -- nothing will be deleted' }))
Write-Host ''

$rows = New-Object System.Collections.ArrayList

$totalCs        = 0
$emittedCount   = 0
$excludedCount  = 0

$allCs = Get-ChildItem -LiteralPath $CoreDir -Recurse -File -Filter '*.cs' -ErrorAction SilentlyContinue

foreach ($file in $allCs) {
    $relative = Get-RelativeDisplayPath -Path $file.FullName -Root $CoreDir

    # MSBuild's own output, never the conversion's.
    $segments = $relative -split '/'
    $inBuildOutput = $false

    foreach ($segment in $segments) {
        if ($BuildOutputDirs -contains $segment) { $inBuildOutput = $true; break }
    }

    if ($inBuildOutput) { continue }

    $totalCs++

    # Emitted by THIS run -> not a candidate. Mirrors platformCensus's modification-time rule, as a
    # threshold rather than an equality because this instrument did not stamp the seed itself.
    #
    # ⚠ THIS TEST HAS A KNOWN FALSE-NEGATIVE AND IT IS NO LONGER LOAD-BEARING (G, mailbox d496727c8).
    # needToWriteFile (projectFileWriter.go:663) skips an identical-bytes write, so a file the converter
    # DID emit unchanged keeps its seed stamp and reads SEEDED here -- and that is the largest class in a
    # hop, every row whose principal did not change between the releases. It is safe anyway, because a
    # file that falls through is then CLASSIFIED rather than deleted: arm 1 saves it if the emission
    # carries it, arm 2 saves it if the target still selects its principal, and only arm 3's two-release
    # gate can condemn it. So this line is now an OPTIMISATION and a statistic ($emittedCount), not a
    # correctness gate -- which is precisely what it was NOT before the selection fix, when a
    # false SEEDED reading fed straight into a tagless deselection and deleted five live files.
    # G's principal-existence proposal would tighten the statistic; it is not needed for safety.
    if ($file.LastWriteTimeUtc -ge $SentinelStamp) {
        $emittedCount++
        continue
    }

    $name = $file.Name

    # <Compile Remove>d artifacts: a stale one cannot collide, so it is counted and not listed.
    if ($name -eq 'package_test_info.cs' -or $name -eq 'go2cs_test_host.cs' -or
        $name -like '*_test.cs' -or $name -like '*.cs.auto' -or $name -like '*.g.cs') {
        $excludedCount++
        continue
    }

    # FIRST question, before the marker scan and before any target lookup: does the converter emit
    # into this directory at all? Everything downstream presumes it does.
    $resolved = Resolve-Principal -RelativePath $relative
    $target   = Test-ConversionTarget -RelativePath $relative -Resolved $resolved

    if (-not $target.IsTarget) {
        $null = $rows.Add([pscustomobject]@{
            Path = $relative; Principal = ''; Class = 'NOT-A-CONVERSION-TARGET'
            Reason = $target.Reason; Group = $target.Group; Full = $file.FullName
        })

        continue
    }

    $isImpl   = ($name -like '*_impl.cs')
    $isMarked = $false

    if (-not $isImpl) { $isMarked = Test-HandOwnMarker -Path $file.FullName }

    if ($isImpl -or $isMarked) {
        $reason = $(if ($isImpl) { '*_impl.cs companion' } else { '[module: GoManualConversion]' })

        $null = $rows.Add([pscustomobject]@{
            Path = $relative; Principal = ''; Class = 'PROTECTED'; Reason = $reason; Group = ''; Full = $file.FullName
        })

        continue
    }

    # Generated metadata has no Go principal by construction -- BUT THAT IS A STATEMENT ABOUT THE
    # PRINCIPAL, NOT ABOUT THE PACKAGE, and the package is a question Go can answer. So the metadata
    # test no longer short-circuits here; it runs AFTER the absent-package test below.
    #
    # ⚠ WHY THE ORDER WAS THE WHOLE DEFECT (coordinator rulings 66e2b64d9 §2(b), 49d0b9ea1 §1; i9
    # measured it at 8f2eafdc8). Returning UNRESOLVED here made EVERY metadata row unresolved
    # unconditionally, whatever its package -- so a package_info.cs whose package was DELETED between
    # the two releases, which is exactly the stale residue this pass exists to remove, read as "needs a
    # human" instead of DELETE-ABSENT, and H5c refused on a real three-target tree with 42 such rows.
    # $resolved was already computed above and then discarded: the answer was in hand and unused.
    #
    # ⚠ AND `Resolve-Principal` ALREADY STRIPS A TRAILING GOOS SEGMENT (its L3 arm: leaf in $KnownGoos
    # and the full directory not a source-std package -> the import path is the PARENT and the leaf is
    # the flavour). So a layout-L3 per-GOOS metadata row -- os/windows/package_info.cs and its nine
    # siblings -- resolves to a LIVE package and falls to the KEEP arm below on its own. No derivation
    # change is needed and none is made here: the derivation was never the defect, the ORDER was.
    # DESIGN-multiplatform-corpus.md §8 confirms per-GOOS metadata is by design, not residue:
    # "identical ones stay flat, varying ones land in the per-GOOS folder (27 and 4 respectively)".
    $isMetadata = ($name -eq 'package_info.cs' -or $name -eq 'package_init.cs' -or $name -eq 'package_info_internal_test.cs')

    # A metadata row has no `.go` principal, so naming one would print a file that never existed
    # (internal/weak/package_info.go). The label says what the row IS.
    $principalLabel = $(if ($isMetadata) { "$($resolved.ImportPath)/<generated metadata>" } else { "$($resolved.ImportPath)/$($resolved.Principal)" })

    if (-not $resolved.Selection.PackageExists) {
        $null = $rows.Add([pscustomobject]@{
            Path = $relative; Principal = $principalLabel; Class = 'DELETE-ABSENT'
            Reason = "package not in std at target"; Group = ''; Full = $file.FullName
        })

        continue
    }

    # The package SURVIVES at the target, so this metadata belongs to something Go still has. It is
    # KEPT and reported BY NAME -- never deleted and never blocking.
    #
    # ⚠ WHY KEPT RATHER THAN DECIDED BY WHETHER THE RUN EMITTED IT (ruling 49d0b9ea1 §1). A row in a
    # surviving package that no staging root carries is a package Go has and the emission did not
    # produce -- deselected, or a converter defect -- and deleting it on that evidence is the timestamp
    # mistake in another coat. Keeping it is safe; it is a finding, not a deletion. THE DECISION USES NO
    # TIMESTAMP AND NO EMISSION EVIDENCE AT ALL: membership at the target decides, and membership is a
    # property of the release pair rather than of what one run happened to rewrite.
    #
    # ⚠ WHY THE RULING'S FOURTH ARM IS NOT WRITTEN HERE -- stated, not silently omitted. Ruling
    # 990f3ba1b §1 splits this case two ways: present at the target AND among the packages THIS RUN
    # converted -> admit; present and NOT converted this run -> UNRESOLVED, a human by name. With this
    # instrument's inputs the second is UNREACHABLE, and writing a branch that cannot be entered is the
    # unfalsifiable-guard shape this package keeps finding. Test-ConversionTarget has already run above,
    # so a row only reaches here with IsTarget = $true -- its package IS in `go list std` at the SOURCE
    # release and is not skip-listed. What the script cannot know is what one particular RUN converted: it
    # classifies a seeded tree on disk and never observes the conversion, so "converted this run" has no
    # input to read. The ruling's own note agrees on the population: "none here".
    #
    # THE INPUT THAT WOULD MAKE THE ARM REAL is COORD's train-48 converter seat -- a per-file emission
    # manifest (new / changed / reproduced-seed / line-endings-only) written beside the emission. When
    # that exists this arm takes it as a parameter and the fourth case becomes measurable instead of
    # assumed. Until then the honest shape is three arms, not four with one that cannot fire.
    if ($isMetadata) {
        $null = $rows.Add([pscustomobject]@{
            Path = $relative; Principal = $principalLabel; Class = 'KEEP-METADATA'
            Reason = "generated metadata in a package that survives at the target (no Go principal to select)"
            Group = ''; Full = $file.FullName
        })

        continue
    }

    # ⚠ THE ORDER OF THE NEXT THREE ARMS IS ITSELF THE SAFETY PROPERTY: every arm that can SAVE a file
    # runs before the one arm that can condemn it, and the condemning arm must then satisfy a gate of
    # its own. That is what keeps coordinator ruling 9c07f494f (derive the class from the EMISSION)
    # compatible with ruling 49d0b9ea1 §1 (absence from a staging root must not by itself delete): here
    # emission evidence only ever KEEPS a file, and a deletion additionally needs the release to have
    # changed its mind. Absence alone condemns nothing, so a converter that stops emitting a file by
    # DEFECT produces a refusal rather than a deletion.

    # ARM 1 -- the emission itself, when a raw emission root was supplied and PROVED unseeded. This is
    # the only evidence that separates "emitted with identical bytes" from "no longer emitted at all":
    # the modification-time arm at the top of this loop cannot, because needToWriteFile
    # (projectFileWriter.go:663) returns false for an unchanged body, so nothing is written and the
    # seed stamp survives. THAT IS THE FIRST HALF OF TONIGHT'S DEFECT -- the five live purego files were
    # emitted UNCHANGED, kept their seed stamp, read SEEDED, and fell through to the selection arms.
    if ($null -ne $EmissionIndex -and $EmissionIndex.Contains($relative)) {
        $null = $rows.Add([pscustomobject]@{
            Path = $relative; Principal = $principalLabel; Class = 'KEEP-SELECTED'
            Reason = 'the emission carries it (the converter selected it; no re-derivation needed)'
            Group = ''; Full = $file.FullName
        })

        continue
    }

    # ARM 2 -- the target release's own selection, under THE CONVERTER'S tag set. This is where the
    # five read SELECTED once the tags reached `go list`, and it is the second half of the defect: the
    # same call with no tags read DESELECTED for all five.
    if ($resolved.Selection.Files.Contains($resolved.Principal)) {
        $null = $rows.Add([pscustomobject]@{
            Path = $relative; Principal = $principalLabel; Class = 'KEEP-SELECTED'
            Reason = "selected for $($resolved.Goos) at $ExpectGo under tags [$BuildTagsLabel]"
            Group = ''; Full = $file.FullName
        })

        continue
    }

    $onDisk = Join-Path $TargetGo.SrcDir (($resolved.ImportPath -replace '/', [System.IO.Path]::DirectorySeparatorChar) + [System.IO.Path]::DirectorySeparatorChar + $resolved.Principal)

    if (-not (Test-Path -LiteralPath $onDisk -PathType Leaf)) {
        $null = $rows.Add([pscustomobject]@{
            Path = $relative; Principal = $principalLabel; Class = 'DELETE-ABSENT'
            Reason = 'principal removed at target'; Group = ''; Full = $file.FullName
        })

        continue
    }

    # ARM 3 -- THE DESELECTION GATE, and the only arm that can condemn. The class MEANS "Go stopped
    # selecting this file at the target", so that sentence is now asserted rather than assumed: the same
    # question, the same tag set, asked at the SOURCE release too.
    #
    # ⚠ WHY NOT THE RUNBOOK AMENDMENT'S TEXT COMPARISON (dd8dd5700c §2: grep '^//go:build' at both SDK
    # trees, same text -> refuse). Measured at the two GOROOTs, that predicate gets four of six of
    # tonight's rows wrong, and three of them in the direction that DELETES A LIVE FILE:
    #
    #   crypto/md5/md5block_generic.go   1.23.12  (!amd64 && !386 && !arm && !ppc64le && !ppc64
    #                                              && !s390x && !arm64) || purego
    #                                    1.24.13  (!386 && !amd64 && !arm && !arm64 && !loong64
    #                                              && !ppc64 && !ppc64le && !riscv64 && !s390x) || purego
    #   -> the TEXT differs (Go grew the negated arch list; the `|| purego` disjunct is untouched), so a
    #      text predicate reads "the release changed its mind" and deletes a file both releases select.
    #      sha1block_generic.go and poly1305/mac_noasm.go differ the same way, for the same reason.
    #
    #   internal/goexperiment/exp_aliastypeparams_off.go  `//go:build !goexperiment.aliastypeparams`
    #                                                     IDENTICAL at both releases
    #   -> the TEXT is unchanged because the 1.24.13 flip is in the DEFAULT GOEXPERIMENT SET, not in the
    #      constraint. A text predicate refuses the row this whole instrument was built to delete -- the
    #      file the DESCRIPTION block above opens with, which killed the first build in 116 seconds.
    #
    # CONSTRAINT TEXT IS NOT SELECTION. Selection is that text EVALUATED against an environment, and
    # between two releases both inputs move: the text, the architecture list, and the GOEXPERIMENT
    # defaults. Asking `go list` asks about all three at once and read 7 of 7 correctly on the rows
    # above. And it is still ONE tag resolution -- the converter's set, at both ends -- which is what
    # ruling 9c07f494f forbade doing twice with two different sets.
    $sourceSelection = Get-GoSelection -ImportPath $resolved.ImportPath -ForGoos $resolved.Goos -Toolchain $SourceGo
    $selectedAtSource = ($sourceSelection.PackageExists -and $sourceSelection.Files.Contains($resolved.Principal))

    if ($selectedAtSource) {
        $null = $rows.Add([pscustomobject]@{
            Path = $relative; Principal = $principalLabel; Class = 'DELETE-DESELECTED'
            Reason = "selected at $ExpectSourceGo, NOT selected at $ExpectGo, same tags [$BuildTagsLabel] -- the release stopped selecting it"
            Group = ''; Full = $file.FullName
        })
    }
    else {
        # Neither release selects this principal under the converter's tag set, yet the corpus carries
        # the file and the principal is on disk at the target. NOTHING ABOUT THE RELEASE HOP EXPLAINS
        # THAT, so it is not a deselection and it is not deleted. It is the shape a wrong tag set, a
        # wrong GOARCH, or a converter defect makes, and all three want a human rather than a deletion.
        $null = $rows.Add([pscustomobject]@{
            Path = $relative; Principal = $principalLabel; Class = 'UNEXPLAINED-DESELECTION'
            Reason = "principal present at $ExpectGo but selected at NEITHER release under tags [$BuildTagsLabel] -- no release change explains this row"
            Group = ''; Full = $file.FullName
        })
    }
}

# ---------------------------------------------------------------------------------------------
# Report. Counts print unconditionally -- a class that appears only when non-empty cannot be told
# from a class whose predicate never fired.
# ---------------------------------------------------------------------------------------------

# KEEP-METADATA sits beside UNRESOLVED deliberately: it is the class rows MOVED to when the metadata
# test stopped short-circuiting the absent-package test, and a reader comparing this run against a
# pre-amendment log needs both counts adjacent to see where the 42 went. A class absent from this list
# is counted nowhere and listed nowhere, so adding a class means adding it here.
#
# UNEXPLAINED-DESELECTION sits immediately after DELETE-DESELECTED because that is where a reader looks
# for it: it is the class a would-be deselection falls to when the release hop does not explain it, and
# the two counts only mean anything read together.
$classOrder = @('DELETE-ABSENT', 'DELETE-DESELECTED', 'UNEXPLAINED-DESELECTION', 'UNRESOLVED', 'KEEP-METADATA', 'PROTECTED', 'NOT-A-CONVERSION-TARGET', 'KEEP-SELECTED')

foreach ($class in $classOrder) {
    $inClass = @($rows | Where-Object { $_.Class -eq $class })

    Write-Host ("--- {0} ({1}) " -f $class, $inClass.Count).PadRight(75, '-')

    if ($class -eq 'KEEP-SELECTED') {
        # The dominant class by construction; listing it whole buries everything else. Its COUNT is
        # the negative control that matters (an instrument that cannot answer "keep" deletes a corpus),
        # and -Verbose prints the rows for anyone who wants them.
        foreach ($row in $inClass) { Write-Verbose ("  {0}  <- {1}" -f $row.Path, $row.Principal) }
        continue
    }

    if ($class -eq 'NOT-A-CONVERSION-TARGET') {
        # Rolled up by GROUP rather than listed: golib alone is over a hundred rows and would bury
        # every actionable line, and the group IS the finding (which directory, and why). Each group
        # prints its count and one representative reason; -Verbose prints the rows.
        foreach ($group in ($inClass | Group-Object Group | Sort-Object Name)) {
            Write-Host ("  {0,-46} {1,4}  {2}" -f $group.Name, $group.Count, $group.Group[0].Reason)
        }

        foreach ($row in $inClass) { Write-Verbose ("  {0}  ({1})" -f $row.Path, $row.Reason) }
        continue
    }

    foreach ($row in ($inClass | Sort-Object Path)) {
        if ($row.Principal -eq '') {
            Write-Host ("  {0}`n      {1}" -f $row.Path, $row.Reason)
        }
        else {
            Write-Host ("  {0}`n      <- {1}  ({2})" -f $row.Path, $row.Principal, $row.Reason)
        }
    }
}

$deleteRows = @($rows | Where-Object { $_.Class -like 'DELETE-*' })
$unresolved = @($rows | Where-Object { $_.Class -eq 'UNRESOLVED' })

# ⚠ The delete set is selected by a NAME PATTERN, so which classes are deletable is currently a property
# of their spelling rather than of a decision. That is fine until someone adds a class -- and a class
# named DELETE-UNEXPLAINED, or a rename of UNEXPLAINED-DESELECTION to match its siblings, would join the
# delete set SILENTLY and without a diff anywhere near the deletion loop. So the intended set is named
# EXACTLY, once, and disagreement is an instrument defect rather than a corpus finding.
$deletableClasses = @('DELETE-ABSENT', 'DELETE-DESELECTED')
$unexpectedDeletable = @($deleteRows | Where-Object { $deletableClasses -notcontains $_.Class } | ForEach-Object { $_.Class } | Sort-Object -Unique)

if ($unexpectedDeletable.Count -gt 0) {
    Deny ("the delete set contains class(es) not on the deletable list: {0}. The deletable classes are exactly [{1}]. A new class matched the DELETE-* pattern without being ruled deletable." -f ($unexpectedDeletable -join ', '), ($deletableClasses -join ', '))
}

# ---------------------------------------------------------------------------------------------
# A DELETE-ABSENT PACKAGE IS A DIRECTORY, NOT A LIST OF .cs -- and the full delete set it implies.
#
# Coordinator rulings `894a761f6` §1 ("the H5c amendment removes a DELETE-ABSENT package as a
# DIRECTORY, residue asserted") and `bf2fd7da0` §3 (ONE `git rm` commit removes exactly the ruled
# delete set, and it REFUSES unless `absent-in-stage.txt` cmp-equals "H5c's delete set at 100 rows
# UNION the residue files of every DELETE-ABSENT package"; a divergence is a posted finding, never
# absorbed). C1's applier hit the same thing from the other side: its precondition keyed on a
# DIRECTORY that H5c leaves behind, so `apply` refused on a real post-H5c root (`3029f08ff1`).
#
# The residue is what the classification loop never looked at: a package directory holds its
# `.csproj`, `.tests.csproj`, `README.md`, icons and test `.cs` beside the production `.cs` this
# instrument classifies. Delete the production files only and the directory survives with a csproj
# the solution generator will still enumerate.
#
# ⚠ WHY THIS REMOVES ENUMERATED FILES AND THEN AN EMPTY DIRECTORY, rather than deleting a directory
# recursively. This instrument runs ONE flavour per invocation (`-Goos`), so "the package is absent at
# the target" is known for THAT flavour only -- R's interim delete guards exactly this by requiring the
# three flavours' DELETE sets to be identical before it removes anything, because a flat file under a
# package directory can be live for a flavour this run never asked about. A recursive directory delete
# would act on that uncertainty; enumerating the residue, removing exactly those files, and then
# dropping the directory ONLY IF IT IS NOW EMPTY turns the uncertain case into a LOUD one: a leftover
# is reported by name and the directory stays. Never `Remove-Item -Recurse` here.
$absentPackageDirs = @{}

foreach ($row in @($deleteRows | Where-Object { $_.Class -eq 'DELETE-ABSENT' -and $_.Reason -eq 'package not in std at target' })) {
    $segments = $row.Path -split '/'

    if ($segments.Count -lt 2) { continue }

    $dir = ($segments[0..($segments.Count - 2)] -join '/')
    $absentPackageDirs[$dir] = $true
}

$residueRows = @()

foreach ($dir in @($absentPackageDirs.Keys | Sort-Object)) {
    $onDisk = Join-Path $CoreDir ($dir -replace '/', [System.IO.Path]::DirectorySeparatorChar)

    if (-not (Test-Path -LiteralPath $onDisk -PathType Container)) { continue }

    foreach ($file in @(Get-ChildItem -LiteralPath $onDisk -File -ErrorAction SilentlyContinue)) {
        $relative = Get-RelativeDisplayPath -Path $file.FullName -Root $CoreDir

        # Anything already carrying a DELETE row is accounted for; the residue is the remainder, and a
        # PROTECTED file in here is NOT residue -- a hand-own is never swept by a package's removal.
        if (@($rows | Where-Object { $_.Path -eq $relative }).Count -gt 0) { continue }

        $residueRows += [pscustomobject]@{ Path = $relative; Dir = $dir; Full = $file.FullName }
    }
}

# ONE HOME for the residue .cs term. It is printed in the deletion summary and subtracted by the
# post-condition, and those were two copies of the same expression until this line existed -- the
# drift shape this file keeps finding in other instruments. Computed from the rows the sweep
# ENUMERATED, never from a listing (ruling cee96ffad §2).
$residueCs = @($residueRows | Where-Object { $_.Path -like '*.cs' }).Count

Write-Host ''
Write-Host '  DELETE-ABSENT packages (whole-package removals)' -ForegroundColor Cyan
Write-Host ("    package directories            {0}" -f $absentPackageDirs.Count)
Write-Host ("    residue files beside the rows  {0}" -f $residueRows.Count)

foreach ($dir in @($absentPackageDirs.Keys | Sort-Object)) {
    $mine = @($residueRows | Where-Object { $_.Dir -eq $dir })
    Write-Host ("      {0,-52} {1} residue file(s)" -f $dir, $mine.Count)

    foreach ($r in $mine) { Write-Host ("          {0}" -f $r.Path) }
}

# ---------------------------------------------------------------------------------------------
# ORPHANED-HAND-OWN: a PROTECTED file whose PACKAGE is DELETE-ABSENT at the target.
#
# Coordinator ruling `485d7387d` §2, on i9's measurement at `6d5696dcd`: H5c's invariant *never deletes
# a hand-own* held (147 -> 147) and the CONSEQUENCE at a release hop is a hand-own protected into a
# directory whose project is gone -- it survives and nothing can build it. i9 measured 5 such files
# across 4 packages (crypto/internal/alias/alias_impl.cs, internal/concurrent/hashtriemap{,_whitebox}.cs,
# internal/weak/pointer.cs, vendor/golang.org/x/crypto/sha3/xor.cs), 0 .csproj surviving.
#
# ⚠ WHY THIS IS A DERIVED CLASS AND NOT ONE THE CLASSIFIER ASSIGNS. `$classOrder` is decided per FILE by
# the classification loop; "its package is DELETE-ABSENT" is a property of the PACKAGE and is only known
# after `$absentPackageDirs` is built, which is after classification. Assigning it in the loop would mean
# asking a question the loop cannot yet answer. The row's own class stays PROTECTED -- the invariant is
# about PROTECTED and is not weakened here.
#
# ⚠ AND WHY THE REFUSAL ALONE FIXES THE ORPHANING, with no change to the residue rule. A package's
# `.csproj` carries no row (it is not a `.cs`, so the classification loop never saw it), so it is residue
# and is removed. That is correct when the package is gone and is exactly what orphans a protected file
# when one survives. Because every refusal runs BEFORE the deletion loop, a run with an undisposed orphan
# removes NOTHING -- so the csproj cannot be deleted out from under a hand-own. The residue rule is
# untouched; the ordering is what makes it safe.
#
# ⚠ ONLY `delete` IS A DISPOSITION THIS INSTRUMENT CAN COMPLETE. Ruling `f0837eea1`, on C2's own report at
# `cb1e4aaf6`: **a move lives in git by the file's owner and carries the file's identity; an instrument
# names orphans and deletes what has no principal, and refuses the rest by name.**
#
# `relocate:` was offered here and is withdrawn, because a `Move-Item` is not the move. Ruling `43ce0c8e6`
# §1 settled that a hand-own's C# identity is a function of its path, so the `namespace` and class lines ARE
# part of the move -- and this file contains no C# parser and should not grow one. i9 then MEASURED the
# failure mode at `0cfc5f33c2` §4-§5: the three mismatched files produced **zero build errors**, and
# `internal/sync/hashtriemap.cs` compiles its moved-in file into `concurrent_package` among `sync_package`
# siblings, which no compiler can object to. A partial move is therefore SILENT, not loud -- the worst
# shape this instrument can produce -- and one of the three needed only the CLASS line changed and not the
# namespace, so even a rewrite would have to be right about two lines in three different shapes.
#
# A `delete` is complete by construction: nothing survives to carry a wrong address. It still owes a
# REMOVED registry entry in Go (`manualTypeOperations.go` names `crypto/internal/alias/alias_impl.cs` as
# holding the displaced `AnyOverlap` body), or `TestManualConversionRegistrationsDisplaceSomething` goes
# red on a registration whose destination vanished; that is a separate seat and is not inferred from here.
$orphanRows = @()

foreach ($row in @($rows | Where-Object { $_.Class -eq 'PROTECTED' })) {
    $segments = $row.Path -split '/'

    if ($segments.Count -lt 2) { continue }

    $dir = ($segments[0..($segments.Count - 2)] -join '/')

    if ($absentPackageDirs.ContainsKey($dir)) {
        $orphanRows += [pscustomobject]@{ Path = $row.Path; Dir = $dir; Full = $row.Full }
    }
}

# Dispositions. `-Orphan <path>=delete` is the only one performed; a `relocate:<new-package>` spelling is
# still PARSED so a stale invocation is answered with the pointer rather than a syntax complaint, and is
# then refused by name (ruling `f0837eea1`). A human's ruling carried by the instrument, never inferred --
# the UNRESOLVED shape (ruling `485d7387d` §2).
$orphanDisposition = @{}

foreach ($entry in @($Orphan)) {
    $split = $entry.IndexOf('=')

    if ($split -lt 1) {
        Stop-ForReview ("-Orphan entry '{0}' is not <path>=delete (the only disposition this instrument performs; relocate: is parsed and refused by name)." -f $entry)
    }

    # Separator-normalised with .NET char Replace rather than a regex: a doubled backslash in a
    # single-quoted PowerShell pattern is an escaping puzzle AND a hit for the fleet's UNC census class.
    $oPath = $entry.Substring(0, $split).Trim().Replace([char]92, [char]47)
    $oWhat = $entry.Substring($split + 1).Trim()

    # `relocate:` is still PARSED, deliberately: a stale invocation carrying one should be answered with
    # the pointer below, not with a syntax complaint that hides why the verb went away.
    if ($oWhat -ne 'delete' -and $oWhat -notlike 'relocate:?*') {
        Stop-ForReview ("-Orphan disposition '{0}' for {1} is not 'delete' (and 'relocate:' is withdrawn -- see the refusal below)." -f $oWhat, $oPath)
    }

    if ($orphanDisposition.ContainsKey($oPath)) {
        Stop-ForReview ("-Orphan names {0} twice. One disposition per path." -f $oPath)
    }

    $orphanDisposition[$oPath] = $oWhat
}

# ⚠ PRINTED UNCONDITIONALLY, including the zero. i9 `7ae5355bb` §3: this block was guarded on a non-zero
# count, so a clean run emitted NOTHING and i9 had to derive the zero by entailment from DELETE-ABSENT 0 --
# correct reasoning, but an absent section is indistinguishable from a check that never ran. **A zero that
# can only be observed as silence is not a measurement.** Ruling `d2ad84bdb` §3.
Write-Host ''
Write-Host '  ORPHANED-HAND-OWN (PROTECTED file whose package is DELETE-ABSENT)' -ForegroundColor Cyan
Write-Host ("    orphaned hand-owns             {0}" -f $orphanRows.Count)
Write-Host ("    dispositions supplied          {0}" -f $orphanDisposition.Count)

if ($orphanRows.Count -gt 0 -or $orphanDisposition.Count -gt 0) {

    foreach ($o in ($orphanRows | Sort-Object Path)) {
        $d = if ($orphanDisposition.ContainsKey($o.Path)) { $orphanDisposition[$o.Path] } else { 'NO DISPOSITION' }
        Write-Host ("      {0,-58} {1}" -f $o.Path, $d)
    }
}

# The FULL delete set, emitted whether or not -Apply was passed: it is the artifact the ruled `git rm`
# step compares against, so a dry run has to be able to produce it. LF-joined and sorted with an
# ordinal comparer so the file is byte-comparable by `cmp` across the boxes that write and read it --
# the CR and culture-sort classes this fleet has already paid for twice.
# ⚠ THE WRAP GOES OUTSIDE THE PIPELINE, and the earlier spelling put it inside. This read
#
#     $deleteSetFull = @( @($deleteRows | ...) + @($residueRows | ...) ) | Sort-Object -Unique
#
# where the `@()` closes BEFORE the pipe, so it constrains the operand and not the RESULT: `Sort-Object`
# over an empty operand emits NOTHING, the assignment lands `$null`, and `$deleteSetFull.Count` on the
# next screen throws PropertyNotFoundException under `Set-StrictMode -Version 2.0`. The run exits 1 from
# a REPORTING line, after classifying correctly and deleting nothing.
#
# ⚠ AND THE SYMMETRY IS THE CRUEL PART, which is why this comment is longer than the fix: THE EMPTY
# DELETE SET IS THE SUCCESS CONDITION. The defect is as old as this block (present verbatim at
# 088f8778f6) and was UNREACHABLE the whole time, because the pass never produced an empty delete set --
# it was still wrongly deleting five live purego files. Correcting the selection is what made the empty
# case reachable, so the crash arrived as a consequence of the instrument becoming right, and i9 met it
# on the very run whose control (DELETE-DESELECTED 0) had just PASSED (mailbox 2337e10e8a). G found the
# same mechanism in check-handown-audit.ps1 the same hour (cc07363b8): an arm that checks the fixture is
# correct and crashes exactly when the fixture IS correct.
#
# Split into two statements rather than re-nested, because the one-line form is what hid it: the reader
# has to notice WHICH paren the pipe is outside of, and nobody does.
$deleteSetUnion = @($deleteRows | ForEach-Object { $_.Path }) + @($residueRows | ForEach-Object { $_.Path })
$deleteSetFull  = @($deleteSetUnion | Sort-Object -Unique -CaseSensitive)

$deleteSetPath = Join-Path $Root 'h5c-delete-set-full.txt'
[System.IO.File]::WriteAllText($deleteSetPath, (($deleteSetFull -join "`n") + "`n"))

Write-Host ''
Write-Host ("  delete set written  {0}" -f $deleteSetPath)
Write-Host ("    {0} path(s) = {1} classified row(s) + {2} residue file(s)" -f $deleteSetFull.Count, $deleteRows.Count, $residueRows.Count)

# Derived, not asserted as a constant: the union can be SMALLER than the sum when a residue file also
# carries a row, and printing the arithmetic from the same variables the file was built from is what
# keeps a future edit from stating a total the file does not have.
if ($deleteSetFull.Count -ne ($deleteRows.Count + $residueRows.Count)) {
    Write-Host ("    NOTE {0} path(s) appear in both halves of the union" -f (($deleteRows.Count + $residueRows.Count) - $deleteSetFull.Count))
}

function Write-Counts {
    Write-Host ''
    Write-Host '  counts' -ForegroundColor Cyan
    Write-Host ("    production .cs under core        {0}" -f $totalCs)
    Write-Host ("      emitted by this run            {0}" -f $emittedCount)
    Write-Host ("      <Compile Remove>d artifacts    {0}" -f $excludedCount)
    Write-Host ("      seeded candidates              {0}" -f $rows.Count)

    foreach ($class in $classOrder) {
        Write-Host ("        {0,-24} {1}" -f $class, @($rows | Where-Object { $_.Class -eq $class }).Count)
    }

    # The source std sets the candidate test consulted, one line per flavour actually asked. A count
    # beside a set claim (CLAUDE.md): a set nobody counted is a set nobody checked.
    Write-Host ''
    Write-Host '  source std sets consulted' -ForegroundColor Cyan

    foreach ($flavour in ($SourceStdCache.Keys | Sort-Object)) {
        Write-Host ("    go list std ({0}, GOOS={1,-8}) {2} package(s)" -f $ExpectSourceGo, $flavour, $SourceStdCache[$flavour].Count)
    }
}

Write-Counts

# ---------------------------------------------------------------------------------------------
# Pre-deletion. EVERYTHING that can refuse runs here, before a single Remove-Item: the first landing
# of this instrument put the UNRESOLVED check after the loop, so exit 2 meant "deleted, then
# complained". Exit 2 now means zero files removed.
# ---------------------------------------------------------------------------------------------

Write-Host ''
Write-Host '  delete set' -ForegroundColor Cyan

foreach ($class in @('DELETE-ABSENT', 'DELETE-DESELECTED')) {
    Write-Host ("    {0,-24} {1}" -f $class, @($rows | Where-Object { $_.Class -eq $class }).Count)
}

Write-Host ("    {0,-24} {1}" -f 'total', $deleteRows.Count)

# The trespass assertion. By construction no NOT-A-CONVERSION-TARGET row can be in $deleteRows -- the
# candidate test runs first and `continue`s. This re-derives the same property from the PATH alone, so
# a future edit that reorders the classification, or a skip-list that drifts from the converter's,
# fails LOUDLY here instead of deleting the hand-written runtime. Two derivations, one instrument each.
function Test-ProtectedPath {
    param([Parameter(Mandatory = $true)][string] $RelativePath)

    $segments = $RelativePath -split '/'
    $root     = $segments[0]

    if ($HandWrittenRoots -contains $root) { return "under the hand-written root $root\" }

    # A skip-listed PACKAGE protects its own directory, not its subtree: testing\ is hand-owned and
    # testing\fstest\ is an ordinary converted package. Guarded on the segment count before the range
    # is formed, for the `0..-1` reason spelled out in Resolve-Principal.
    if ($segments.Count -gt 1) {
        $dir = ($segments[0..($segments.Count - 2)] -join '/')

        if ($NonConvertedStdPackages -contains $dir) {
            return "in the converter-skip-listed package $dir"
        }
    }

    foreach ($prefix in $NonConvertedStdPrefixes) {
        if ($RelativePath.StartsWith($prefix, [System.StringComparison]::Ordinal)) {
            return "under the converter-skip-listed path $prefix"
        }
    }

    return $null
}

$trespassers = @()

foreach ($row in $deleteRows) {
    $why = Test-ProtectedPath -RelativePath $row.Path
    if ($null -ne $why) { $trespassers += [pscustomobject]@{ Path = $row.Path; Why = $why } }
}

if ($trespassers.Count -gt 0) {
    Write-Host ''
    Write-Host ("TRESPASS ASSERTION FIRED -- {0} delete row(s) sit where the converter never emits:" -f $trespassers.Count) -ForegroundColor Red

    foreach ($row in ($trespassers | Sort-Object Path)) {
        Write-Host ("    {0}`n        {1}" -f $row.Path, $row.Why) -ForegroundColor Red
    }

    Stop-ForReview 'the classification and the path test disagree. This is an instrument defect, not a corpus finding.'
}

if ($Apply -and $unresolved.Count -gt 0) {
    Write-Host ''
    Write-Host ("UNRESOLVED: {0} seeded file(s) have no derivable Go principal." -f $unresolved.Count) -ForegroundColor Yellow
    Stop-ForReview '-Apply refuses while UNRESOLVED rows stand. Dispose of each (they are listed above), then re-run with -Apply.'
}

# ⚠ THE EXPLANATION REFUSAL -- the runbook H5 amendment's line, and the one that would have stopped
# tonight's five from being deleted (coordinator ruling 9c07f494f; the surviving runbook line is "every
# DELETE-DESELECTED row is explained at the two GOROOTs or the run is refused").
#
# Unconditional rather than gated on -Apply, unlike UNRESOLVED above, and the asymmetry is deliberate: an
# UNRESOLVED row is a question about a file, and a dry run listing questions is useful. An
# UNEXPLAINED-DESELECTION row says the INSTRUMENT'S OWN VIEW of the release pair is incoherent -- the
# corpus holds a file whose principal exists at the target and which neither release selects -- and every
# reading in the same run rests on that same view. So the report is not to be trusted row by row, and
# exiting 2 without -Apply is what stops a dry run's DELETE-DESELECTED count from being quoted as a
# measurement. A wrong tag set is the first thing this shape means, which is why the banner prints the set
# and its provenance.
$unexplained = @($rows | Where-Object { $_.Class -eq 'UNEXPLAINED-DESELECTION' })

if ($unexplained.Count -gt 0) {
    Write-Host ''
    Write-Host ("UNEXPLAINED-DESELECTION: {0} row(s) are not selected at EITHER release under tags [{1}]." -f $unexplained.Count, $BuildTagsLabel) -ForegroundColor Yellow
    Write-Host '  Nothing about the release hop explains these, so they are not deselections and none was deleted.'
    Write-Host '  Read in this order -- the first two are far more likely than the third:'
    Write-Host ("    1. the tag set. It is [{0}] here; is that what the converter PRINTED for this emission?" -f $BuildTagsLabel)
    Write-Host ("    2. -Goarch. It is '{0}'; a corpus emitted for another architecture reads exactly like this." -f $Goarch)
    Write-Host '    3. a converter defect -- the corpus holds a file the converter should no longer be emitting.'

    foreach ($row in $unexplained) {
        Write-Host ("    {0}`n        {1}" -f $row.Path, $row.Reason) -ForegroundColor Yellow
    }

    Stop-ForReview ("{0} DELETE-DESELECTED candidate(s) have no explanation at the two GOROOTs. The class means 'Go stopped selecting this file at the target'; a row neither release selects does not say that, and deleting it deletes a live file -- which is exactly what happened to five of them before this gate existed." -f $unexplained.Count)
}

# ⚠ BEFORE the deletion loop, with UNRESOLVED, because that ordering is what keeps a hand-own's `.csproj`
# from being swept while the hand-own itself is protected: a refused run removes NOTHING.
$orphanUndisposed = @($orphanRows | Where-Object { -not $orphanDisposition.ContainsKey($_.Path) })
$orphanStale      = @($orphanDisposition.Keys | Where-Object { $p = $_; @($orphanRows | Where-Object { $_.Path -eq $p }).Count -eq 0 })

if ($Apply -and $orphanUndisposed.Count -gt 0) {
    Write-Host ''
    Write-Host ("ORPHANED-HAND-OWN: {0} PROTECTED file(s) sit in a DELETE-ABSENT package and have no disposition:" -f $orphanUndisposed.Count) -ForegroundColor Yellow

    foreach ($o in ($orphanUndisposed | Sort-Object Path)) {
        Write-Host ("    {0}`n        package {1} is absent at the target; the file survives with no project to build it." -f $o.Path, $o.Dir) -ForegroundColor Yellow
    }

    Stop-ForReview '-Apply refuses while an orphaned hand-own has no disposition. For a file whose principal is GONE at the target: -Orphan "<path>=delete". For one whose principal MOVED: do the move in git (path plus the namespace and class lines, ruling 43ce0c8e6 section 1) and re-run -- it is then no longer an orphan and needs no disposition here.'
}

# ⚠ `relocate:` REFUSED BY NAME (ruling `f0837eea1`). Placed BEFORE the stale-disposition check, because a
# relocate names a REAL orphan and would otherwise pass that check and reach the deletion loop.
$orphanRelocates = @($orphanDisposition.Keys | Where-Object { $orphanDisposition[$_] -like 'relocate:?*' })

if ($orphanRelocates.Count -gt 0) {
    Write-Host ''
    Write-Host ("ORPHANED-HAND-OWN: {0} disposition(s) ask for a relocate, which this instrument no longer performs:" -f $orphanRelocates.Count) -ForegroundColor Yellow

    foreach ($p in ($orphanRelocates | Sort-Object)) {
        Write-Host ("    {0}  ->  {1}" -f $p, $orphanDisposition[$p]) -ForegroundColor Yellow
    }

    Stop-ForReview ('a relocate is a git move PLUS the namespace and class lines (ruling 43ce0c8e6 section 1: a hand-own C# identity is a function of its path), and it belongs in a commit by the file owner -- not in a deletion instrument with no C# parser. A partial move is SILENT: i9 measured the three mismatched files at 0cfc5f33c2 producing ZERO build errors. Do the move in git, then re-run -- the file is no longer inside a DELETE-ABSENT package, so it is no longer an orphan and there is nothing here to dispose of. -Orphan <path>=delete is unchanged and remains complete.')
}

# A disposition naming a path that is NOT an orphan is a stale ruling, and passing it silently would let a
# corrected package list keep an instruction nobody re-read. Same reason the delete set is compared rather
# than absorbed.
if ($orphanStale.Count -gt 0) {
    Write-Host ''
    Write-Host ("ORPHANED-HAND-OWN: {0} disposition(s) name a path that is not an orphaned hand-own in this run:" -f $orphanStale.Count) -ForegroundColor Yellow

    foreach ($p in ($orphanStale | Sort-Object)) { Write-Host ("    {0}" -f $p) -ForegroundColor Yellow }

    Stop-ForReview 'an -Orphan disposition does not match any orphaned hand-own here. Re-read the list above and drop or correct it.'
}

if (-not $Apply) {
    Write-Host ''
    Write-Host ("DRY RUN -- {0} file(s) would be deleted, {1} unresolved, {2} orphaned hand-own(s). Re-run with -Apply to delete." -f $deleteRows.Count, $unresolved.Count, $orphanRows.Count) -ForegroundColor Yellow
}
else {
    Write-Host ''
    Write-Host ("APPLY -- deleting {0} file(s)" -f $deleteRows.Count) -ForegroundColor Yellow

    $deleted = 0

    foreach ($row in ($deleteRows | Sort-Object Path)) {
        # Belt and braces: the same path test the assertion above ran, re-asked immediately before the
        # irreversible act. Unreachable while the assertion stands, which is the point -- if it ever
        # fires the run stops HERE rather than continuing through the rest of the list.
        $why = Test-ProtectedPath -RelativePath $row.Path

        if ($null -ne $why) {
            Write-Host ''
            Write-Host ("DELETION ABORTED MID-LOOP at {0} ({1})" -f $row.Path, $why) -ForegroundColor Red
            Write-Host ("{0} file(s) had already been removed. The tree is PART-DELETED; discard the staging root." -f $deleted) -ForegroundColor Red
            exit 3
        }

        Remove-Item -LiteralPath $row.Full -Force
        $deleted++
        Write-Host ("    deleted  {0}" -f $row.Path)
    }

    # The residue, then the directory -- and the directory ONLY if removing the enumerated files
    # emptied it. See the block where $residueRows is built for why this is never -Recurse.
    $residueDeleted = 0
    $dirsRemoved    = 0
    $dirsKept       = @()
    $slnxRemoved    = 0

    foreach ($r in ($residueRows | Sort-Object Path)) {
        $why = Test-ProtectedPath -RelativePath $r.Path

        if ($null -ne $why) {
            Write-Host ''
            Write-Host ("RESIDUE DELETION ABORTED at {0} ({1})" -f $r.Path, $why) -ForegroundColor Red
            Write-Host 'The tree is PART-DELETED; discard the staging root.' -ForegroundColor Red
            exit 3
        }

        Remove-Item -LiteralPath $r.Full -Force
        $residueDeleted++
        Write-Host ("    deleted  {0}  (residue)" -f $r.Path)
    }

    # Orphan dispositions run HERE -- after the residue sweep, before the directory-empty test -- so a
    # deleted orphan lets its now-empty package directory be removed in the loop below instead of being
    # reported as KEPT. Every one was named and refused-on above; nothing is inferred.
    $orphansDeleted = 0

    foreach ($o in ($orphanRows | Sort-Object Path)) {
        $what = $orphanDisposition[$o.Path]

        if ($what -eq 'delete') {
            # ⚠ THE ONLY Remove-Item IN THIS FILE THAT DELETES A PROTECTED FILE, and it is reachable only
            # for a path that appears in $orphanRows (so: classified PROTECTED, in a DELETE-ABSENT package)
            # AND carries an explicit human disposition. Every other deletion loop calls Test-ProtectedPath
            # immediately before the irreversible act; this one cannot use it as a veto, because deleting a
            # protected file is exactly what was ruled. So it is used as a FLOOR instead: the skip-listed
            # packages and golib are never DELETE-ABSENT, so an orphan that also matches the path guard
            # means the ruling and the instrument's floor disagree, and that is not a thing to resolve by
            # deleting. Ruling `485d7387d` §2 authorises the disposition, not a floor breach.
            $floor = Test-ProtectedPath -RelativePath $o.Path

            if ($null -ne $floor) {
                Write-Host ''
                Write-Host ("ORPHAN DELETE REFUSED: {0} is floor-protected ({1}); a ruling cannot reach it." -f $o.Path, $floor) -ForegroundColor Red
                Write-Host ("{0} file(s) had already been removed. The tree is PART-DELETED; discard the staging root." -f ($deleted + $residueDeleted)) -ForegroundColor Red
                exit 3
            }

            Remove-Item -LiteralPath $o.Full -Force
            $orphansDeleted++
            Write-Host ("    deleted  {0}  (orphaned hand-own, ruled delete)" -f $o.Path) -ForegroundColor Yellow
            continue
        }

        # UNREACHABLE: every relocate is refused before this loop. Kept as an assertion rather than as a
        # move, so a future edit that re-admits the verb fails loudly here instead of half-moving a file.
        Stop-ForReview ("unreachable: disposition '{0}' for {1} is not 'delete' and should have been refused before the deletion loop. This is an instrument defect, not a corpus finding." -f $what, $o.Path)
    }

    if ($orphanRows.Count -gt 0) {
        Write-Host ("  orphaned hand-owns: {0} deleted, of {1}  (relocate is REFUSED; a move lives in git)" -f $orphansDeleted, $orphanRows.Count)
    }

    foreach ($dir in @($absentPackageDirs.Keys | Sort-Object)) {
        $onDisk = Join-Path $CoreDir ($dir -replace '/', [System.IO.Path]::DirectorySeparatorChar)

        if (-not (Test-Path -LiteralPath $onDisk -PathType Container)) { continue }

        $left = @(Get-ChildItem -LiteralPath $onDisk -Force -ErrorAction SilentlyContinue)

        if ($left.Count -eq 0) {
            Remove-Item -LiteralPath $onDisk -Force
            $dirsRemoved++
            Write-Host ("    removed  {0}/  (empty after its files)" -f $dir)
        }
        else {
            # NOT a failure of the run, and NOT swept: a flavour this invocation never asked about can
            # legitimately keep a file here. Named so the reader decides, which is the whole reason the
            # removal is not recursive.
            $dirsKept += $dir
            Write-Host ("    KEPT     {0}/  -- {1} entr(y/ies) remain, not enumerated as residue:" -f $dir, $left.Count) -ForegroundColor Yellow

            foreach ($e in $left) { Write-Host ("          {0}" -f $e.Name) -ForegroundColor Yellow }
        }
    }

    # ---------------------------------------------------------------------------------------------
    # THE SOLUTION ENTRY IS PART OF THE PACKAGE (coordinator ruling `485d7387d` §2).
    #
    # Train 47 learned "a DELETE-ABSENT package is a DIRECTORY, not a list of .cs" (`894a761f6` §1); this
    # is that lesson one level up. i9 measured the consequence at `6d5696dcd`: the reconvert emits
    # go2cs-stdlib.slnx listing all 358 converted packages, H5c then removes 14 of them, nothing tells
    # the solution, and `dotnet build src/go2cs-stdlib.slnx` fails with 14 x MSB3202 in 1.01 s -- it never
    # compiles anything. The corpus was right and the manifest was a step behind.
    #
    # ⚠ WHY THIS EDITS THE FILE RATHER THAN RE-RUNNING THE GENERATOR. `GenerateSolutionFile` derives its
    # project list from `collectConvertedProjects`, and `parseCoreProjectRefs`'s own comment says a package
    # is recovered from its DEPENDENTS' <ProjectReference> entries -- "its dependents reference it, so it
    # surfaces here even though its own .csproj is absent from the output tree" (`unsafe` is the canonical
    # case). So a regeneration would re-add any removed package still referenced by a survivor, putting
    # the dangling entry back. Removing the lines H5c's own package list names cannot do that.
    #
    # Matched on the package path this instrument ALREADY HOLDS, after normalising the line's separators,
    # with a trailing slash so `internal/weak/` cannot match `internal/weakmap/...`, and the file is
    # rewritten preserving its CRLF terminators (the generator emits CR LF; this repo has paid for a
    # line-ending flip twice).
    $slnxPath = Join-Path $RootFull 'go2cs-stdlib.slnx'

    # ⚠ THE PREDICATE IS "ITS PROJECT IS GONE", NOT "ITS DIRECTORY IS GONE", and the two differ in a case
    # this instrument already has a code path for. Ruling `485d7387d` §2 words the post-condition as
    # `slnx entries removed == DELETE-ABSENT packages whose DIRECTORY was removed`. For the expected run
    # the two are identical (all fourteen directories empty out, so 14 == 14 either way). They part
    # company on a KEPT directory: a single-flavour run legitimately keeps a package directory when a file
    # for a flavour it never asked about remains -- that is what `$dirsKept` is for -- and the package's
    # `.csproj` has still been removed as residue. Under the ruled predicate that entry is NOT removed, so
    # it dangles, and MSB3202 comes back while the post-condition reads clean. Keying on the project file
    # closes that and agrees with the ruling everywhere the ruling is right. Reported to COORD as a
    # divergence rather than applied silently.
    $slnxDirs = @()

    foreach ($dir in @($absentPackageDirs.Keys | Sort-Object)) {
        $onDisk = Join-Path $CoreDir ($dir -replace '/', [System.IO.Path]::DirectorySeparatorChar)

        # No directory at all, or a directory with no surviving .csproj: either way nothing can build it
        # and its <Project> entry is dangling.
        $csprojLeft = @()

        if (Test-Path -LiteralPath $onDisk -PathType Container) {
            $csprojLeft = @(Get-ChildItem -LiteralPath $onDisk -File -Filter '*.csproj' -ErrorAction SilentlyContinue)
        }

        if ($csprojLeft.Count -eq 0) { $slnxDirs += $dir }
    }

    if ($slnxDirs.Count -gt 0) {
        if (-not (Test-Path -LiteralPath $slnxPath -PathType Leaf)) {
            Write-Host ''
            Write-Host ("SOLUTION FILE NOT FOUND at {0} -- {1} package(s) lost their project file and their <Project> entries cannot be removed." -f $slnxPath, $slnxDirs.Count) -ForegroundColor Red
            Write-Host 'The corpus is correct and the solution would be left stale; that is the MSB3202 wall. Refusing to report success.' -ForegroundColor Red
            exit 3
        }

        $slnxText  = [System.IO.File]::ReadAllText($slnxPath)
        $slnxCrLf  = $slnxText.Contains("`r`n")
        $slnxLines = $slnxText -split "`r?`n"
        $keptLines = @()

        foreach ($line in $slnxLines) {
            $drop = $false

            foreach ($dir in $slnxDirs) {
                # The emitted attribute is solution-relative with forward slashes: core/<pkg>/<name>.csproj
                # Both separators, for the reason `coreProjectRefRE` in solutionGenerator.go gives: a
                # corpus emitted by a pre-F5 binary or a deployed tree can carry backslashes, and a
                # forward-slash-only matcher would keep the entry while its directory is gone.
                # NORMALISE THEN MATCH, rather than a both-separators character class: same coverage
                # (solutionGenerator.go's own reason -- a pre-F5 binary or a deployed tree can emit
                # backslashes), one spelling of the path, and no escaped backslash in the pattern.
                $normalized = $line.Replace([char]92, [char]47)
                $pattern    = 'Path="core/' + (($dir -split '/' | ForEach-Object { [regex]::Escape($_) }) -join '/') + '/'

                if ($normalized -match $pattern) { $drop = $true; break }
            }

            if ($drop) {
                $slnxRemoved++
                Write-Host ("    slnx     removed entry for {0}/" -f ($line.Trim()))
            }
            else { $keptLines += $line }
        }

        $joiner = if ($slnxCrLf) { "`r`n" } else { "`n" }
        [System.IO.File]::WriteAllText($slnxPath, ($keptLines -join $joiner))
    }

    $survivors = @($deleteRows | Where-Object { Test-Path -LiteralPath $_.Full })
    $residueSurvivors = @($residueRows | Where-Object { Test-Path -LiteralPath $_.Full })

    Write-Host ''
    Write-Host ("  deleted {0} of {1}; {2} survived" -f $deleted, $deleteRows.Count, $survivors.Count)
    Write-Host ("  residue deleted {0} of {1}; {2} survived" -f $residueDeleted, $residueRows.Count, $residueSurvivors.Count)

    # Printed HERE, beside the two deletion counts, because this is where a reader reconciling the
    # arithmetic looks -- and because the alternative is parsing it out of the residue listing, which is
    # what produced 42-against-37 (ruling cee96ffad §2: "the instrument states the number it used").
    # The after-block below subtracts this same variable, so the two can never disagree.
    Write-Host ("  residue .cs {0}   (the term the post-condition subtracts; the rest are .csproj, icons and test hosts)" -f $residueCs)
    Write-Host ("  package directories removed {0} of {1}; {2} kept with entries remaining" -f $dirsRemoved, $absentPackageDirs.Count, $dirsKept.Count)
    Write-Host ("  slnx <Project> entries removed {0}  (packages with no project file {1}; directories removed {2})" -f $slnxRemoved, $slnxDirs.Count, $dirsRemoved)

    # ⚠ THIS ROW IS A GUARD ON THE GENERATED SOLUTION, NOT A REPAIR OF IT -- ruling `d2ad84bdb` §3, which
    # supplies the mechanism my own report at `1f8da6540` did not have:
    #
    #   **the stdlib solution heals by regeneration, and the step-5 wall was the regenerated file not being
    #   carried from the staging root.**
    #
    # So the 358-vs-344 gap i9 measured at `7ae5355bb` §2 was never stale CONTENT: the converter rewrites
    # this file from the packages it converted, and the committed copy stayed behind because nothing carried
    # the regenerated one over. That is why this pass removed 0 -- correctly -- and why it must stay: it is
    # the check that the solution about to be carried has no dangling entry, not the thing that fixes one.
    # The runbook gains the carry step (COORD, next docs commit).
    #
    # ⚠ AND THE POST-CONDITION OWES A POPULATION ASSERTION, which it did not have on its first real run:
    # `0 -ne 0` is false, so it PASSED over an empty population in a file that refuses an empty file list,
    # an empty pattern file and an empty range elsewhere. A bare equality of two zeroes is an arm that
    # cannot go red. It now states which case it is in.
    #
    # A mismatch is the MSB3202 wall either forming (fewer entries removed than projects gone -> dangling
    # references) or over-reaching (more -> a live project dropped out of the solution). Both are exit 3
    # rather than a note, because both produce a solution wrong in a way `dotnet build` reports as
    # somebody else's bug. If it fires, the two things to look at are a package listed TWICE in the slnx
    # (its own .csproj plus a reference recovered from a dependent) and an entry written under a path
    # this matcher does not recognise; both are visible in the removed-entry lines printed above.
    if ($slnxDirs.Count -eq 0) {
        # VACUOUS, and said so rather than passing quietly: no DELETE-ABSENT package lost its project file,
        # so there was nothing for the removal to act on and the equality below would compare 0 with 0.
        Write-Host '  slnx post-condition: VACUOUS on this run -- no DELETE-ABSENT package lost its project file, so nothing was removable. The check did not run over a population.'
    }
    elseif ($slnxRemoved -ne $slnxDirs.Count) {
        Write-Host ''
        Write-Host ("SLNX POST-CONDITION FAILED: {0} <Project> entr(y/ies) removed against {1} package(s) with no project file ({2} director(y/ies) removed)." -f $slnxRemoved, $slnxDirs.Count, $dirsRemoved) -ForegroundColor Red
        Write-Host 'The corpus and its solution manifest disagree. Discard the staging root.' -ForegroundColor Red
        exit 3
    }
    else {
        Write-Host ("  slnx post-condition: MET over {0} package(s) with no project file -- {1} <Project> entr(y/ies) removed." -f $slnxDirs.Count, $slnxRemoved)
    }

    if ($residueSurvivors.Count -gt 0) {
        foreach ($r in $residueSurvivors) { Write-Host ("    SURVIVED  {0}  (residue)" -f $r.Path) -ForegroundColor Red }
        Write-Host 'RESIDUE DELETION INCOMPLETE' -ForegroundColor Red
        exit 3
    }

    if ($survivors.Count -gt 0) {
        foreach ($row in $survivors) { Write-Host ("    SURVIVED  {0}" -f $row.Path) -ForegroundColor Red }
        Write-Host 'DELETION INCOMPLETE' -ForegroundColor Red
        exit 3
    }

    # Re-count by RE-WALKING the tree, not by re-printing the classification. The counts above
    # describe what was PLANNED; this one is measured off disk afterwards, so the two can disagree
    # and a disagreement is visible. (An earlier draft re-called the same counter here and printed
    # byte-identical numbers under a comment claiming they described the result -- a comment that
    # claims a behaviour the code lacks reads as the census.)
    $remaining = @(Get-ChildItem -LiteralPath $CoreDir -Recurse -File -Filter '*.cs' -ErrorAction SilentlyContinue |
        Where-Object {
            $rel = Get-RelativeDisplayPath -Path $_.FullName -Root $CoreDir
            $keep = $true
            foreach ($segment in ($rel -split '/')) { if ($BuildOutputDirs -contains $segment) { $keep = $false; break } }
            $keep
        })

    # ⚠ THE EXPECTED COUNT OWES A RESIDUE TERM, and its absence was a defect of mine that only a tree
    # with residue .cs could surface. i9 measured it at 846cbd849 running be9668d56: the classified set
    # deleted exactly (102 of 102, 0 survived), then the residue sweep removed 108 more files of which
    # 37 were production .cs, and this post-condition refused a count it could not reconcile -- exit 3,
    # CORRECTLY. The residue sweep landed in 01caa02a0 and this arithmetic was not updated with it, so
    # the check has been unsatisfiable on any tree with residue .cs ever since.
    #
    # DERIVED WITH THE SAME PREDICATE THE WALK USES, never a fresh glob: $remaining counts every .cs
    # under core outside $BuildOutputDirs, so the term is the .cs among $residueRows. Two reasons that is
    # exactly the deleted set rather than an approximation of it: the residue enumeration is
    # NON-RECURSIVE over a package directory, so no build-output path can enter it; and residue SURVIVORS
    # already exited 3 above, so every residue row reaching this line was removed.
    #
    # ⚠ WHY THE FIX IS THE ARITHMETIC AND NOT "CLASSIFY THE RESIDUE INSTEAD". A single-flavour run cannot
    # classify a file whose principal it cannot resolve for the flavours it never asked about -- that is
    # why the sweep exists and why the directory removal is not recursive (coordinator ruling 894a761f6
    # §1 accepted the departure). And residue is NOT deleted unnamed: every residue file appears in
    # h5c-delete-set-full.txt and on its own `deleted <path> (residue)` line. So the gap was bookkeeping,
    # not disclosure.
    #
    # THE DECOMPOSITION IS PRINTED rather than left to be reconstructed. i9's parse of the log counted 42
    # residue .cs against the 37 the arithmetic implies, because the KEPT-directory block below prints
    # the names of entries that were NOT residue and a listing-parse cannot tell the two blocks apart.
    # A number the instrument derived beats a number a reader parsed out of its prose.
    $expectedRemaining = $totalCs - $deleted - $residueCs

    Write-Host ''
    Write-Host '  after (re-walked from disk)' -ForegroundColor Cyan
    Write-Host ("    production .cs under core        {0}   (was {1}, minus {2} classified, minus {3} residue .cs)" -f $remaining.Count, $totalCs, $deleted, $residueCs)

    if ($remaining.Count -ne $expectedRemaining) {
        Write-Host ("    ARITHMETIC MISMATCH -- expected {0} = {1} - {2} classified - {3} residue .cs" -f $expectedRemaining, $totalCs, $deleted, $residueCs) -ForegroundColor Red
        exit 3
    }
}

Write-Host ''

if ($unresolved.Count -gt 0) {
    Write-Host ("UNRESOLVED: {0} seeded file(s) have no derivable Go principal and were NOT deleted." -f $unresolved.Count) -ForegroundColor Yellow
    Write-Host 'A human must dispose of each before the overlay. Exiting non-zero so this is not passed over.'
    exit 2
}

Write-Host 'No unresolved rows.' -ForegroundColor Green
exit 0
