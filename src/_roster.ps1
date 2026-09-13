<#
.SYNOPSIS
    The validated-package roster reader: docs\ValidatedTestPackages.md parsed into row objects,
    including the per-OS expectation annotations. Dot-source it.

.DESCRIPTION
    The roster table is the single source of truth for which packages are banked and what counts
    they carry, and `run-validated-sweep.ps1` has always read it rather than hardcoding a list. The
    parsing lives HERE rather than inside the sweep for one reason: it now carries a rule with an
    arithmetic consequence -- the per-OS annotation ruled on 2026-08-22 -- and a rule with a
    consequence needs a guard that can exercise it without running a multi-hour gate.
    `check-roster-format.ps1` is that guard; it dot-sources this file exactly as the sweep does.

    Two things are parsed out of a row:

      COLUMNS      | [`pkg`](url) | <matched> | <disclosed> | What it exercises. |
                   Columns 2 and 3 are the WINDOWS record for the Go 1.23.1 era, per the per-OS
                   ruling. They are authoritative and never blended with any other OS's numbers.

      ANNOTATIONS  Inside the What-it-exercises cell, as its own middle-dot-separated segment
                   placed last, immediately before the ` <dot> [proof](...)` link:

                       <dot> linux: 302
                       <dot> linux: 18 + 1

                   `<goos>: <matched>` records that OS's matching-verdict count; the optional
                   `+ <disclosed>` records its disclosed count and is omitted when zero, mirroring
                   the blank Disclosed column. `windows` is deliberately NOT a valid key -- the
                   columns ARE the Windows expectation, and a row claiming otherwise is a
                   contradiction the parser refuses rather than silently prefers one half of.

                   A row may also carry an EXECUTION annotation, the same segment shape:

                       <dot> execution: release-tc0

                   That one is not a count and not a platform -- it is the local execution CONFIG
                   the row's pipeline leg must run under (owner ruling 2026-08-30, Option A). The
                   Applicable/Expected semantics are untouched by it: a row's banked numbers mean
                   exactly what they meant, and the annotation says only HOW the converted host is
                   published and run to produce them. Unannotated rows are the default path and
                   nothing about them moves.

    The same document also carries the EXCLUSION LEDGER (the "Excluded packages" table), read by
    Get-ExclusionLedgerRows. A ledger row's first cell is a PLAIN code span on purpose -- the
    roster row's linked [`pkg`](url) shape is what $RosterRowPattern anchors on, so the two tables
    in one document can never be confused by either parser; the document's own HTML comment
    beneath the ledger states the same rule from the other side.

    Nothing here has side effects; it defines pure functions and returns.

.NOTES
    Requires PowerShell 5.1 (Windows) or PowerShell 7+ (any platform).
    No non-ASCII literal appears in this file: the middle-dot separator is spelled by code point,
    the same discipline the sweep's Go-duration parser uses for the two micro signs.
#>

# The cell-segment separator the roster's What-it-exercises column uses (U+00B7 MIDDLE DOT). Spelled
# by code point so this file stays ASCII and cannot be mojibaked by a PowerShell argument pass.
$RosterSegmentSeparator = [string][char]0x00B7

# The OS keys an annotation may carry: the corpus's non-Windows platform flavors (layout L3 emits
# windows, linux and darwin). `windows` is absent ON PURPOSE -- see Get-ValidatedRosterRows.
$RosterOsKeys = @('linux', 'darwin')

# One row of the table. Row shape:
#   | [`net/http/internal/ascii`](https://...) | 13 | 1 | What it exercises. |
$RosterRowPattern = '^\|\s*\[`([^`]+)`\]\([^)]*\)\s*\|\s*(\d+)\s*\|\s*(\d*)\s*\|'

# The host-conditional annotation (predates the per-OS one, unchanged): the run of backticked,
# comma-separated names right after the colon, stopping at the first text that is not one.
$RosterConditionalPattern = 'host-conditional\s*(?:\([^)]*\))?\s*:\s*((?:`[^`]+`\s*,\s*)*`[^`]+`)'

# The host-conditional DISCLOSURE annotation (Q31, 2026-09-04): the names of disclosure-manifest
# entries whose FIRING is a property of the host, so the same verdict honestly sits in the matched
# column on one host and in the disclosed column on another (os/exec's TestExtraFiles: fires where the
# published test host holds a descriptor in 3..100 at exec_test.go's init() scan, 97 on a single-file
# container host, none on the fleet's Linux bank host). Same backticked-list grammar as the surplus
# annotation above; the pattern above cannot match this one -- its `host-conditional` must be followed
# by an optional paren group and a colon, and `-disclosure` is neither (asserted by fixture).
$RosterConditionalDisclosurePattern = 'host-conditional-disclosure\s*(?:\([^)]*\))?\s*:\s*((?:`[^`]+`\s*,\s*)*`[^`]+`)'

# The per-OS expectation annotation. Anchored on the cell separator at BOTH ends (the closing
# lookahead also admits the row's terminating pipe) so it can only match a segment of its own --
# prose that happens to say "on linux: five of them" is not an annotation and must not read as one.
$RosterOsPattern =
    [regex]::Escape($RosterSegmentSeparator) +
    '\s*([a-z][a-z0-9]*)\s*:\s*(\d+)\s*(?:\+\s*(\d+)\s*)?(?=' +
    [regex]::Escape($RosterSegmentSeparator) + '|\||$)'

# The permanently-inapplicable form of the same annotation (ruled 2026-08-29, from the registry
# row): a package that cannot exist on an OS carries `<goos>: n/a` -- never pending, never
# validated, counted in neither the Linux header's numerator nor its applicable denominator. Same
# both-ends anchoring as the numeric form, for the same prose-immunity reason.
$RosterOsNaPattern =
    [regex]::Escape($RosterSegmentSeparator) +
    '\s*([a-z][a-z0-9]*)\s*:\s*n/a\s*(?=' +
    [regex]::Escape($RosterSegmentSeparator) + '|\||$)'

# ---- the per-row EXECUTION annotation (owner ruling 2026-08-30, Option A) ------------------------
# Some Go tests assert a liveness property Go's compiler provides and the CLR's DEFAULT execution
# config does not: `codegen-liveness` names the class. The tier-0 A/B measured that the class is a
# CONFIG artifact rather than a structural one -- internal/weak's suite validates 4/4 under a Release
# publish with DOTNET_TieredCompilation=0, five consecutive runs -- but the flip is NOT globally
# safe: two residuals (internal/godebug's line attribution, log/slog's pc=0) are TC0-ONLY failures.
# So the ruling is per-row opt-in, and this annotation is how a row opts in.
#
# It is an EXECUTION property, never a platform one. Nothing about Applicable/Expected moves: the
# row's Tests and Disclosed columns still mean what they meant, the per-OS annotations still answer
# for their own OS, and an unannotated row's pipeline leg is byte-for-byte the leg it always was.
# The value names a config, not a flag list -- Get-RosterExecutionArgs owns that mapping, so the
# roster never has to know what the converter's command line looks like.
# 'release-tiered' joined 2026-09-02 with the Release+TC0 default flip, and it is the MIRROR of
# 'release-tc0': where that one opted a row INTO tiering-off while Debug was the default, this one
# opts a row back OUT of it now that Release+TC0 is. Three rows carry it, each measured as a
# one-axis A/B rather than inferred -- internal/godebug (TestCmdBisect), log/slog (TestCallDepth)
# and net/http (TestRegisterErr) -- all three PC/line-attribution assertions that tiering's presence
# is what supplies. 'release-tc0' is RETAINED though the flip makes it redundant: it still names
# exactly what it always named, and a row that opted in deliberately should keep saying so.
$RosterExecutionValues = @('release-tc0', 'release-tiered')

# Same both-ends separator anchoring as the per-OS forms, for the same prose-immunity reason: a
# sentence reading "the execution: it runs Release" is not an annotation and must not read as one.
# The value admits internal hyphens (release-tc0) where an OS key does not.
$RosterExecutionPattern =
    [regex]::Escape($RosterSegmentSeparator) +
    '\s*execution\s*:\s*([a-z][a-z0-9]*(?:-[a-z0-9]+)*)\s*(?=' +
    [regex]::Escape($RosterSegmentSeparator) + '|\||$)'

# One row of the exclusion ledger (the "Excluded packages" table in the same document). Its first
# cell is a PLAIN code span, never the roster row's linked [`pkg`](url) shape -- that difference is
# what keeps the two tables apart. Row shape:
#   | `os/user` | <verdicts> | E2 | Mechanism prose. | [ruling][exclusion-ruling] |
$ExclusionLedgerRowPattern = '^\|\s*`([^`]+)`\s*\|\s*([^|]*?)\s*\|\s*([^|]*?)\s*\|'

# The ruled exclusion classes (owner ruling 2026-08-25): E1 no eligible tests on the target
# platform, E2 broken oracle, E3 the test's subject is the replaced representation.
#
# E4 joined by owner ruling 2026-09-07, and it is a THIRD LIMB rather than a relaxation of the
# first two. E1/E2/E3 all sit on the bar's "provably meaningless" limb -- no test to run, no
# trustworthy oracle, or a pass that would be fabrication -- i.e. the comparison cannot produce
# information. E4 names the case where the comparison IS sound and produces information, and
# yields no VALIDATION: it ran, it was host-qualified, and every verdict says the capability is
# absent. `runtime/trace` is the founding member (0 matched / 2 diverged, measured, not argued).
#
# The distinction from E3 is the reason a pass is unavailable, and it is a judgment like E2's and
# E3's: an E3 pass would be FABRICATION (the subject is the replaced representation), an E4 pass
# would be LEGITIMATE IMPLEMENTATION nobody has written. `matched == 0` is the guardrail that
# keeps the class from drifting -- it is NECESSARY, not sufficient, since E3's own
# `internal/unsafeheader` is matched-0 too -- and the moment effort produces one matching verdict
# the row leaves E4 by arithmetic rather than by anyone's judgment. E4 members are therefore the
# rows most likely to exercise the standing rejoin clause, and each carries its revisit condition.
$ExclusionLedgerClasses = @('E1', 'E2', 'E3', 'E4')

<#
.SYNOPSIS
    The GOOS flavor this host validates -- the corpus flavor a build here actually binds.
.DESCRIPTION
    `_paths.ps1` pins $env:GoTargetOS to the HOST's own flavor on every non-Windows host, precisely
    because every L3 csproj defaults the property to `windows` when it is EMPTY. That default is the
    whole rule: the environment variable wins where one is set, and 'windows' is what an unset one
    means -- which is right on Windows and only there.

    2026-09-02: this paragraph used to end "...and on macOS because darwin's corpus does not build
    yet and keeps the status-quo default until its own lane earns one". That wall is CLOSED -- the
    darwin corpus compiles clean, census run 32649840220 at c003d32af, zero errors on osx-x64 and
    osx-arm64 -- so _paths.ps1 pins `darwin` on a macOS host and this function reports it. Darwin's
    remaining gap is the RUN layer (docs/phase4/FINDING-darwin-run-layer.md), which sits downstream
    of the flavor and is not a reason to bind the wrong one.

    The $IsMacOS/$IsLinux fallbacks cover a consumer that dot-sourced this file WITHOUT _paths.ps1;
    both are inert on Windows PowerShell 5.1, where neither variable exists ($null -> falsey).
#>
function Get-SweepTargetGoos {
    if (-not [string]::IsNullOrWhiteSpace($env:GoTargetOS)) {
        return $env:GoTargetOS.Trim().ToLowerInvariant()
    }

    if ($IsMacOS) { return 'darwin' }
    if ($IsLinux) { return 'linux' }

    return 'windows'
}

<#
.SYNOPSIS
    Parses the roster table into row objects.
.OUTPUTS
    One PSCustomObject per row: Package, Expected, Disclosed, Conditional (string[]), and OS
    (hashtable of goos -> @{ Expected; Disclosed }).
#>
function Get-ValidatedRosterRows {
    param([Parameter(Mandatory)][string] $Path)

    if (-not (Test-Path $Path)) { throw "Cannot find the validated-package table at $Path" }

    # ReadAllLines rather than Get-Content: PowerShell 5.1 reads a BOM-less UTF-8 file as ANSI,
    # which would split the separator's two bytes into two characters. The column captures are
    # ASCII either way, but the annotation anchors on that separator, so the encoding is now
    # load-bearing and is stated rather than inherited.
    $lines = [System.IO.File]::ReadAllLines($Path)

    $rows = New-Object System.Collections.Generic.List[object]

    foreach ($line in $lines) {
        if ($line -notmatch $RosterRowPattern) { continue }

        # Row fields FIRST: every -match below overwrites $Matches.
        $rowPackage = $Matches[1]
        $rowExpected = [int]$Matches[2]
        $rowDisclosed = if ($Matches[3]) { [int]$Matches[3] } else { 0 }

        $rowConditional = @()
        if ($line -match $RosterConditionalPattern) {
            $rowConditional = @([regex]::Matches($Matches[1], '`([^`]+)`') | ForEach-Object { $_.Groups[1].Value })
        }

        $rowConditionalDisclosures = @()
        if ($line -match $RosterConditionalDisclosurePattern) {
            $rowConditionalDisclosures = @([regex]::Matches($Matches[1], '`([^`]+)`') | ForEach-Object { $_.Groups[1].Value })
        }

        $rowOs = @{}
        foreach ($match in [regex]::Matches($line, $RosterOsPattern)) {
            $key = $match.Groups[1].Value

            # A `windows:` annotation is a contradiction, not a preference: columns 2 and 3 ARE the
            # Windows expectation, so a row carrying both would hold two Windows answers with no
            # rule for which wins. Refused by name rather than ignored, because an ignored one would
            # read to its author as recorded.
            if ($key -eq 'windows') {
                throw ("Roster row '$rowPackage' carries a 'windows:' per-OS annotation. The Tests " +
                    'and Disclosed COLUMNS are the Windows expectation -- record a Windows count ' +
                    'there, never as an annotation.')
            }

            if ($RosterOsKeys -notcontains $key) {
                throw ("Roster row '$rowPackage' carries an unknown per-OS annotation key '$key'. " +
                    "Known keys: $($RosterOsKeys -join ', ').")
            }

            if ($rowOs.ContainsKey($key)) {
                throw "Roster row '$rowPackage' carries more than one '$key' annotation."
            }

            $rowOs[$key] = [PSCustomObject]@{
                Expected   = [int]$match.Groups[2].Value
                Disclosed  = if ($match.Groups[3].Success) { [int]$match.Groups[3].Value } else { 0 }
                Applicable = $true
            }
        }

        # The n/a form, with the numeric form's refusals repeated verbatim: a key this loop admits
        # that the one above refuses would make `windows: n/a` a back door, and a row carrying both
        # `linux: N` and `linux: n/a` holds two answers with no rule for which wins.
        foreach ($match in [regex]::Matches($line, $RosterOsNaPattern)) {
            $key = $match.Groups[1].Value

            if ($key -eq 'windows') {
                throw ("Roster row '$rowPackage' carries a 'windows:' per-OS annotation. The Tests " +
                    'and Disclosed COLUMNS are the Windows expectation -- record a Windows count ' +
                    'there, never as an annotation.')
            }

            if ($RosterOsKeys -notcontains $key) {
                throw ("Roster row '$rowPackage' carries an unknown per-OS annotation key '$key'. " +
                    "Known keys: $($RosterOsKeys -join ', ').")
            }

            if ($rowOs.ContainsKey($key)) {
                throw "Roster row '$rowPackage' carries more than one '$key' annotation."
            }

            $rowOs[$key] = [PSCustomObject]@{
                Expected   = $null
                Disclosed  = $null
                Applicable = $false
            }
        }

        # The execution annotation. Refused by NAME on an unknown value rather than ignored: a row
        # whose author wrote `execution: release` would otherwise run the default path while reading,
        # to that author, as opted in -- which is the silent-config failure this whole annotation
        # exists to make impossible. Two of them is two answers with no rule for which wins.
        $rowExecution = $null
        foreach ($match in [regex]::Matches($line, $RosterExecutionPattern)) {
            $value = $match.Groups[1].Value

            if ($RosterExecutionValues -notcontains $value) {
                throw ("Roster row '$rowPackage' carries an unknown execution annotation " +
                    "'$value'. Known configs: $($RosterExecutionValues -join ', ').")
            }

            if ($null -ne $rowExecution) {
                throw "Roster row '$rowPackage' carries more than one execution annotation."
            }

            $rowExecution = $value
        }

        [void]$rows.Add([PSCustomObject]@{
            Package     = $rowPackage
            Expected    = $rowExpected
            Disclosed   = $rowDisclosed
            Conditional = $rowConditional
            ConditionalDisclosures = $rowConditionalDisclosures
            OS          = $rowOs
            Execution   = $rowExecution
        })
    }

    return $rows.ToArray()
}

<#
.SYNOPSIS
    Parses the H10 eligibility census's appendix skeleton into row objects, for -Hop runs.
.DESCRIPTION
    The hop's row population is NOT the banked roster. At a release hop the banked table holds the
    rows banked at the OUTGOING release, while the eligible set at the INCOMING one is the census's
    own appendix -- and the two populations genuinely differ. Measured at 2e6cf71e4 between
    docs/ValidatedTestPackages.md (204 rows) and this skeleton (227): 194 names in common, 10 roster
    rows absent from the skeleton, 33 skeleton rows absent from the roster.

    ⚠ THE TEN ARE RELOCATIONS, NOT DISAPPEARANCES, and that is why this function carries Receives.
    The skeleton's fifth column names, for ten of its rows, the 1.23.12 row whose tests moved into
    it and the count that row banked (`internal/sync` receives `internal/concurrent (20)`; `weak`
    receives `internal/weak (4)`; the six crypto/internal rows receive their pre-fips140 selves;
    `internal/runtime/math` and `.../sys` receive the old `runtime/internal/*`). Those ten cells name
    EXACTLY the ten roster rows absent from the skeleton, one for one, zero unmatched in either
    direction -- so nothing was lost at this hop, and a mode that reported the ten as "gone" or the
    ten arrivals as "new" would throw away the one fact that makes their counts reviewable.

    ⚠ A Receives count is PROVENANCE AND NEVER AN EXPECTATION. The census rules this itself, above
    its relocation table: "No count is carried: a relocated row re-banks from zero at 1.24, and the
    verdict figure below records only what the 1.23.12 anchor held." So Expected stays $null here
    for every row, relocated or not, and no caller may compare a measured count against Receives.

    Verdicts and disclosed are blank for all 227 rows BY DESIGN -- the hop MEASURES them; they are
    its output, not its input. (Recorded because the author of this function first reported the
    Receives column blank as well, having characterised a 227-row table from the rows he sampled:
    ten populated cells sit at indices 29-41, 138-142 and 227, and a sparse column is the one kind
    of emptiness that sampling confirms.)
.OUTPUTS
    One PSCustomObject per row, carrying every property run-validated-sweep.ps1 reads off a roster
    row so the sweep's row pipeline needs no second shape: Package, Expected ($null -- nothing is
    banked at the incoming release), Disclosed ($null), Conditional (empty), ConditionalDisclosures
    (empty), OS (empty), Execution ($null -- no row carries an annotation in the skeleton), plus
    Index and Receives, which exist only on a hop row.
#>
function Get-HopSkeletonRows {
    param([Parameter(Mandatory)][string] $Path)

    if (-not (Test-Path $Path)) { throw "Cannot find the H10 eligibility census at $Path" }

    # ReadAllLines for the same load-bearing reason Get-ValidatedRosterRows states: 5.1's Get-Content
    # reads a BOM-less UTF-8 file as ANSI, and this file carries non-ASCII prose around the table.
    $lines = [System.IO.File]::ReadAllLines($Path)

    # FIVE fields exactly, and the count is what separates the appendix from the census's other
    # tables: the admitted-list table above it opens with the same `| N | `pkg` |` shape and carries
    # THREE, and the relocation table's rows open with a backticked name rather than an index. A
    # looser pattern would silently sweep both in.
    $pattern = '^\|\s*(\d+)\s*\|\s*`([^`]+)`\s*\|([^|]*)\|([^|]*)\|([^|]*)\|\s*$'

    $rows = New-Object System.Collections.Generic.List[object]

    foreach ($line in $lines) {
        if ($line -notmatch $pattern) { continue }

        $rowIndex    = [int]$Matches[1]
        $rowPackage  = $Matches[2].Trim()
        $rowVerdicts = $Matches[3].Trim()
        $rowDisclosed = $Matches[4].Trim()
        $rowReceives = $Matches[5].Trim()

        if (-not $rowPackage) { throw "Hop skeleton row $rowIndex has an empty package name." }

        # The skeleton's counts are its OUTPUT. A populated one means this file has already been
        # banked into, and a hop run over it would compare against figures it is supposed to be
        # deriving -- refused by name rather than silently honoured or silently ignored.
        if ($rowVerdicts -or $rowDisclosed) {
            throw ("Hop skeleton row $rowIndex ($rowPackage) already carries a count " +
                "(verdicts '$rowVerdicts', disclosed '$rowDisclosed'). The skeleton is the hop's " +
                'output, not its input: re-derive into a blank skeleton, or bank this one into the ' +
                'roster and sweep against that instead.')
        }

        [void]$rows.Add([PSCustomObject]@{
            Package     = $rowPackage
            Expected    = $null
            Disclosed   = $null
            Conditional = @()
            ConditionalDisclosures = @()
            OS          = @{}
            Execution   = $null
            Index       = $rowIndex
            Receives    = $rowReceives
        })
    }

    $result = $rows.ToArray()

    # Three assertions rather than a count against a constant, because a constant goes stale at the
    # next hop and this population is meant to change: the table must be non-empty, its indices must
    # run 1..N with no gap (a mis-parse drops or doubles rows, and either shows up here), and its
    # names must be unique. Each names the offending row.
    if ($result.Count -eq 0) {
        throw ("$Path yielded no hop skeleton rows -- the 5-field appendix pattern matched nothing, " +
            'so this is a parse failure and not an empty census.')
    }

    for ($i = 0; $i -lt $result.Count; $i++) {
        if ($result[$i].Index -ne ($i + 1)) {
            throw ("Hop skeleton indices are not contiguous: row at position $($i + 1) carries " +
                "index $($result[$i].Index) ($($result[$i].Package)).")
        }
    }

    $duplicates = @($result | Group-Object -Property Package | Where-Object { $_.Count -gt 1 })
    if ($duplicates.Count -gt 0) {
        throw ("Hop skeleton names are not unique: " + (($duplicates | ForEach-Object { $_.Name }) -join ', '))
    }

    return $result
}

<#
.SYNOPSIS
    Parses the exclusion-ledger table ("Excluded packages") into row objects.
.OUTPUTS
    One PSCustomObject per row: Package, Verdicts (the raw cell text -- a naive count where one
    exists, an em dash where no baseline exists to count against), Class.
#>
function Get-ExclusionLedgerRows {
    param([Parameter(Mandatory)][string] $Path)

    if (-not (Test-Path $Path)) { throw "Cannot find the validated-package table at $Path" }

    # ReadAllLines for the same encoding reason Get-ValidatedRosterRows states.
    $lines = [System.IO.File]::ReadAllLines($Path)

    $rows = New-Object System.Collections.Generic.List[object]

    foreach ($line in $lines) {
        if ($line -notmatch $ExclusionLedgerRowPattern) { continue }

        [void]$rows.Add([PSCustomObject]@{
            Package  = $Matches[1]
            Verdicts = $Matches[2]
            Class    = $Matches[3]
        })
    }

    return $rows.ToArray()
}

<#
.SYNOPSIS
    The roster document rewritten as a release's FROZEN snapshot copy of itself.
.DESCRIPTION
    A release freezes docs\validation\current\ into docs\validation\<version>\ so the proof shown
    for a published binary stays the proof as of that binary. It did NOT freeze the roster PAGE, so
    the published site had every per-package proof from publication day and no "how things stood"
    view of the campaign around them: the only such view was the signed tag nuget-<version>, which
    is a git object and reaches nobody reading go2cs.net. This function is the missing half -- the
    roster text as it stood, retargeted so the snapshot links WITHIN ITSELF.

    Two substitutions, both counted and both returned, because a published document is the one
    artifact where a silent no-op and a silent over-match cost the same and look identical:

      RELOCATE   Every relative link that pointed OUT of docs\ (../src/..., README.md#..., a
                 phase4\ reference definition) is two directories shallower than the snapshot, so
                 it gains '../../' and resolves to exactly the file it named before. This is pure
                 relocation compensation: the target does not move, the path to it does. Runs
                 FIRST and excludes validation/current/ explicitly, so the proof links below are
                 still in their original spelling when the second substitution looks for them --
                 order is load-bearing, since a retargeted sibling link ('bytes.md') matches the
                 relocate pattern perfectly and would be sent to '../../bytes.md'.

      PROOF      validation/current/<id>.md -> <id>.md, the sibling frozen page. This is the whole
                 point: a snapshot whose 204 proof links walk back into the LIVING directory is a
                 snapshot of one page, not of a publication.

    UNRELOCATED is the audit arm. It names every path-shaped relative target in the source that
    NEITHER substitution consumed -- empty on today's roster, and the only way a future roster
    gaining a link shape nobody anticipated shows up as something other than a dangling link on
    the published site. It is reported, never silently passed.

    The roster's ABSOLUTE links are deliberately NOT touched here, and there are two kinds. The
    PACKAGE COLUMN is one tree/master URL per row, pinned onto the release tag by
    Update-FrozenRosterSourceLinks below; the roster's prose also carries one blob/master pointer at a
    package's disclosure manifest, pinned by Update-FrozenRosterDisclosureLink beside it. Both are
    separate functions because both have to be runnable ALONE on an already-frozen roster, which this
    one cannot be: re-running it would insert a second note and relocate the relocated links. The
    release calls all three, in that order. "Two substitutions" above counts this function's, not the
    frozen roster's absolute ones.

    The note is inserted after the H1 rather than before it: this page has no YAML front matter
    (nothing under docs\ does) and its first line is the Jekyll {% raw %} guard whose matching
    endraw must stay the file's last line, so both ends of the document are spoken for. Inside the
    raw guard is also where the note is safe by construction -- it can never be read as Liquid.

    Non-ASCII is composed from [char], never written as a literal: PS 5.1 parses a BOM-less UTF-8
    .ps1 through the system codepage, and a mojibake'd em dash in a COMMENT is cosmetic while one
    in this function's output ships to nuget.org and go2cs.net.
.OUTPUTS
    PSCustomObject: Text, ProofLinks (int), Relocated (string[]), Unrelocated (string[]).
#>
function ConvertTo-FrozenRosterText {
    param(
        [Parameter(Mandatory)][string] $RosterText,
        [Parameter(Mandatory)][string] $Version,
        [Parameter(Mandatory)][string] $Commit,
        [string] $FrozenOn = (Get-Date -Format 'yyyy-MM-dd')
    )

    if ([string]::IsNullOrWhiteSpace($RosterText)) { throw 'ConvertTo-FrozenRosterText: the roster text is empty.' }

    # Every link target the source carries, by both markdown forms, before anything is rewritten.
    # Path-shaped means "contains a slash, or ends in an extension" -- which admits every real link
    # and excludes the two placeholder targets the roster's own HTML comments carry when they
    # describe a link shape in prose.
    $inlineTargetPattern = '\]\(([^)\s]+)\)'
    $refTargetPattern = '(?m)^\[[^\]]+\]:[ \t]+(\S+)'
    $pathShaped = '^(?!https?://)(?!#)(?:[^\s)]*/[^\s)]*|[^\s)]+\.[A-Za-z0-9]{1,6}(?:#[^\s)]*)?)$'

    $sourceTargets = New-Object System.Collections.Generic.List[string]
    foreach ($pattern in @($inlineTargetPattern, $refTargetPattern)) {
        foreach ($match in [regex]::Matches($RosterText, $pattern)) {
            $target = $match.Groups[1].Value
            if ($target -match $pathShaped) { $sourceTargets.Add($target) }
        }
    }

    $handled = New-Object System.Collections.Generic.List[string]
    $relocated = New-Object System.Collections.Generic.List[string]
    $text = $RosterText

    # RELOCATE. The extension set is derived from a census of the roster this shipped against
    # (.md and .ps1 were its only relative non-proof targets); anything outside it is not silently
    # passed, it lands in Unrelocated below.
    $relocateInline = '\]\((?!https?://|#|validation/current/)((?:\.\./)*[A-Za-z0-9_][^)\s]*\.(?:md|ps1)(?:#[^)\s]*)?)\)'
    # The trailing class admits \r as well as space and tab. .NET's multiline '$' matches BEFORE the
    # \n of a CRLF line, so a '[ \t]*$' tail cannot reach the end of a line in this repo's CRLF
    # working tree -- measured: the one reference definition in the roster went UNMATCHED and landed
    # in Unrelocated, which is the audit arm doing its job rather than a link silently dangling.
    $relocateRef = '(?m)^(\[[^\]]+\]:[ \t]+)(?!https?://|#|validation/current/)((?:\.\./)*[A-Za-z0-9_][^\s]*\.(?:md|ps1)(?:#\S*)?)[ \t\r]*$'

    $text = [regex]::Replace($text, $relocateInline, {
        param($m)
        $relocated.Add($m.Groups[1].Value); $handled.Add($m.Groups[1].Value)
        '](../../' + $m.Groups[1].Value + ')'
    })

    $text = [regex]::Replace($text, $relocateRef, {
        param($m)
        $relocated.Add($m.Groups[2].Value); $handled.Add($m.Groups[2].Value)
        $m.Groups[1].Value + '../../' + $m.Groups[2].Value
    })

    # PROOF. Second, for the ordering reason stated above.
    $proofPattern = '\]\(validation/current/([^)\s]+)\)'
    $text = [regex]::Replace($text, $proofPattern, {
        param($m)
        $handled.Add('validation/current/' + $m.Groups[1].Value)
        '](' + $m.Groups[1].Value + ')'
    })
    $proofLinks = @($handled | Where-Object { $_ -like 'validation/current/*' }).Count

    $unrelocated = @($sourceTargets | Where-Object { $handled -notcontains $_ } | Sort-Object -Unique)

    # The note, as a SINGLE-QUOTED here-string with placeholders rather than an array of
    # concatenations. Two traps closed by that shape, the first of them measured here:
    #
    #   PRECEDENCE  PowerShell binds ',' TIGHTER than '+', so @( 'a' + $x + 'b', 'c' + $y ) is not a
    #               two-element array of joined strings -- it is 'a' + $x + ('b','c') + $y, an array
    #               of fragments. The first run of this function emitted an 8-line note as 22 lines,
    #               one per fragment, with the version and the date each alone on a line of a
    #               PUBLISHED document. A here-string has no operators to mis-bind.
    #   BACKTICK    A double-quoted string would need every markdown code-span backtick doubled;
    #               inside @'...'@ a backtick is a backtick.
    #
    # The one non-ASCII glyph is composed rather than typed (see the encoding paragraph above).
    $emDash = [string][char]0x2014
    $noteTemplate = @'

> **Frozen snapshot {DASH} go2cs {VERSION}.** This is the roster exactly as it stood at
> publication, copied on {DATE} from commit `{COMMIT}` {DASH} the tree the signed tag
> `nuget-{VERSION}` names. It is written once at release and never rewritten, so the view of how
> the campaign stood behind a published binary survives every later bank; the living roster, which
> keeps moving, is [`docs/ValidatedTestPackages.md`](../../ValidatedTestPackages.md). Every proof
> link below points at this snapshot's own sibling page rather than at the living
> `validation/current/`, so the snapshot reads without leaving itself.
'@

    $noteText = $noteTemplate.Replace('{DASH}', $emDash).Replace('{VERSION}', $Version).Replace('{COMMIT}', $Commit).Replace('{DATE}', $FrozenOn)
    $note = @($noteText -split "`r?`n")
    if ($note -join '' -match '\{(DASH|VERSION|COMMIT|DATE)\}') { throw 'ConvertTo-FrozenRosterText: an unfilled placeholder survived into the note.' }

    $newline = if ($RosterText -match "`r`n") { "`r`n" } else { "`n" }
    $lines = @($text -split "`r?`n")

    $h1 = -1
    for ($i = 0; $i -lt $lines.Count; $i++) {
        if ($lines[$i] -match '^#\s') { $h1 = $i; break }
    }
    if ($h1 -lt 0) { throw 'ConvertTo-FrozenRosterText: the roster has no H1 to place the frozen-snapshot note under.' }

    $out = New-Object System.Collections.Generic.List[string]
    for ($i = 0; $i -le $h1; $i++) { $out.Add($lines[$i]) }
    foreach ($noteLine in $note) { $out.Add($noteLine) }
    for ($i = $h1 + 1; $i -lt $lines.Count; $i++) { $out.Add($lines[$i]) }

    return [pscustomobject]@{
        Text        = ($out -join $newline)
        ProofLinks  = $proofLinks
        Relocated   = $relocated.ToArray()
        Unrelocated = $unrelocated
        NoteLines   = $note.Count
    }
}

<#
.SYNOPSIS
    Retarget one FROZEN proof page's MOVING links onto the release that page belongs to.
.DESCRIPTION
    A proof page under docs\validation\current\ is a LIVING document and its outbound links are
    right for that: the roster link walks up to docs\ValidatedTestPackages.md, the converted
    package's source link names tree/master, and -- on the pages whose package discloses -- a prose
    sentence points at that package's hand-owned go2cs_test_disclosures.json, spelled blob/master.
    Copied into docs\validation\<version>\ unchanged -- which is what the freeze did until
    2026-09-07 -- every one of them keeps pointing at a moving target, so a page whose whole purpose
    is to say "this is the evidence for the binary we shipped" reads its row out of a roster that
    has banked packages since, its source out of a branch that has moved on, and the manifest that
    licenses its disclosed rows out of that same branch. A frozen page that links the living roster,
    tree/master and blob/master is not frozen.

    Every target exists already and none has to be invented. The snapshot carries its OWN roster
    copy beside the page (ConvertTo-FrozenRosterText above), so the roster link becomes a sibling;
    and the release mints the signed tag nuget-<version> BEFORE the build, so both absolute links
    have an immutable ref to name. Ordinal string replacements throughout, no line splitting, so the
    page's line endings survive exactly as the converter emitted them.

    ZERO IS A THROW FOR THE TWO UNIVERSAL LINKS, not a skip. Every page the converter generates
    carries exactly one roster link and one source link, so a page carrying neither is template
    drift somebody must look at -- and a silent skip would publish that page still pointing at
    master while the count beside it read fine. It also makes those two arms non-idempotent BY
    DESIGN: a second run over an already-retargeted page finds nothing to do and says so, which is
    what makes the caller's count assertion a live check rather than a number that can only ever be
    right. That non-idempotency is also why the third substitution is a callable sibling rather than
    a third block in this body: see below.

    THE DISCLOSURE POINTER IS OPTIONAL PER PAGE, which is a THIRD COUNT SHAPE rather than a third
    instance of the one above, and it lives in Update-FrozenProofPageDisclosureLink below -- CALLED
    from here, so the caller still retargets a page with one call and makes no second pass, and the
    count comes back in this function's own object. It is a separate function for one reason: it has
    to be RUNNABLE ALONE on a page this function would throw on. A snapshot frozen before that rule
    existed carries pages whose roster and source links are pinned already, so the two arms above
    refuse them -- deliberately -- while their disclosure pointers still name master. Its header
    carries the count's shape, its census, and why the caller reports the sum rather than asserting
    it against the page count.

    The source-link pattern is anchored on the REPOSITORY, not on a bare '/tree/master/': a future
    page linking some other project's master must not be silently rewritten to name a go2cs tag. A
    page that spells this repository's URL some other way lands in the zero-substitution throw
    above, which is the reporting arm rather than a link quietly left behind.

    The encoding is built here rather than taken from the caller. Read AND write through
    [System.IO.File] with UTF-8/no-BOM for the reason stated at push-nuget.ps1's README retarget --
    PS 5.1's Get-Content reads BOM-less UTF-8 as ANSI and Out-File re-encodes the damage -- and a
    shared function that reached for a caller's variable would work in one script and be undefined
    in the next.
.OUTPUTS
    PSCustomObject: RosterLinks (int), SourceLinks (int), DisclosureLinks (int) -- substitutions
    made, per link kind. The first two are 1 on every page; the third is 0 or 1, and the caller
    sums it into an aggregate of its own rather than into either page-count assertion.
#>
function Update-FrozenProofPage {
    param(
        [Parameter(Mandatory)][string] $Path,
        [Parameter(Mandatory)][string] $Version
    )

    if (-not (Test-Path -LiteralPath $Path)) { throw "Update-FrozenProofPage: no page at $Path." }
    if ([string]::IsNullOrWhiteSpace($Version)) { throw 'Update-FrozenProofPage: the version is empty.' }

    # No trailing ')' on the roster pattern: the link may legitimately carry a '#anchor' suffix, and
    # the prefix is what moves.
    $rosterFrom = '](../../ValidatedTestPackages.md'
    $rosterTo = '](ValidatedTestPackages.md'
    $sourceFrom = 'github.com/ritchiecarroll/go2cs/tree/master/'
    $sourceTo = "github.com/ritchiecarroll/go2cs/tree/nuget-$Version/"

    $text = [System.IO.File]::ReadAllText($Path)

    $rosterLinks = ([regex]::Matches($text, [regex]::Escape($rosterFrom))).Count
    $sourceLinks = ([regex]::Matches($text, [regex]::Escape($sourceFrom))).Count

    if ($rosterLinks -eq 0) {
        throw ("Update-FrozenProofPage: $Path carries no roster link ('$rosterFrom') to retarget. A frozen " +
               "page that cannot be pointed at its snapshot's own roster is template drift -- reconcile the " +
               "proof-page template with this transform rather than publishing the page as it stands.")
    }

    if ($sourceLinks -eq 0) {
        throw ("Update-FrozenProofPage: $Path carries no converted-source link ('$sourceFrom') to pin. A frozen " +
               "page whose source link cannot be moved off master describes a branch rather than the binary that " +
               "was published -- reconcile the proof-page template with this transform.")
    }

    $text = $text.Replace($rosterFrom, $rosterTo).Replace($sourceFrom, $sourceTo)
    [System.IO.File]::WriteAllText($Path, $text, (New-Object System.Text.UTF8Encoding($false)))

    # The THIRD substitution, delegated to the sibling below rather than spelled again here. One
    # definition, two callers: this page loop, and the one-off that had to reach the 36 pages of an
    # ALREADY-FROZEN snapshot -- which this function cannot be run on, its two arms above throwing on
    # a page whose roster and source links are pinned already. That is the same constraint, and the
    # same resolution, as Update-FrozenRosterSourceLinks beside ConvertTo-FrozenRosterText.
    $disclosure = Update-FrozenProofPageDisclosureLink -Path $Path -Version $Version

    return [pscustomobject]@{
        RosterLinks = $rosterLinks
        SourceLinks = $sourceLinks
        DisclosureLinks = $disclosure.DisclosureLinks
    }
}

<#
.SYNOPSIS
    Pin one proof page's OPTIONAL disclosure-manifest pointer onto the release that page belongs to.
.DESCRIPTION
    The third of Update-FrozenProofPage's substitutions, in its own function because it is the only
    one of the three that has to be RUNNABLE ALONE. Update-FrozenProofPage throws on a page whose
    roster and source links are already pinned -- deliberately, that is what makes its counts a live
    check -- so it cannot be re-run over a snapshot that has already been frozen. The 1.23.12.3 pages
    were frozen before this rule existed and 36 of them carry a pointer still naming blob/master, so
    reaching them needed a transform that could run on a file by itself. Exactly the constraint, and
    exactly the one-definition-two-callers resolution, that put Update-FrozenRosterSourceLinks and
    Update-FrozenRosterDisclosureLink beside ConvertTo-FrozenRosterText rather than inside it: the
    committed snapshot cannot drift from the code that will produce its successors.

    Update-FrozenProofPage CALLS this, so the freeze step's page loop is unchanged and makes no
    second pass -- the page is retargeted once, by one call, and the count comes back in the same
    object as the other two.

    ZERO IS ORDINARY here, which is what makes this a THIRD COUNT SHAPE rather than a third instance
    of the arms above, and it is the whole reason the caller reports this number instead of asserting
    it. A page carries the pointer only if its package has a disclosure manifest to name: at
    1.23.12.3, 36 of the 204 pages carry exactly one and 168 carry none, measured on the frozen
    snapshot and independently on the living docs\validation\current\ tree, which agree page for page
    BY NAME. The sum is therefore the size of a SUBSET nothing structural predicts -- it moves the
    day a package banks a first disclosure or retires its last, with no template change to notice --
    so asserting it against the page count would fail every release, and asserting it against a
    number written here would go stale on exactly that day.

    What IS asserted is the pair a subset does support. MORE THAN ONE throws: a shape nobody has seen
    (no page in either tree carries two), and a second pointer on one page is a template change whose
    author should rule on it rather than have it pinned behind an arm written for one. And the after
    arm is a TRIPWIRE, not a measurement, inert today by construction: String's Replace moves every
    occurrence and 'blob/nuget-<version>/' cannot contain 'blob/master/', so a survivor means somebody
    has widened the pattern until the target contains the source. It exists to fail that day, which is
    the standing both roster siblings give their own after-arms.

    The pattern is anchored on the REPOSITORY, spelled as all three siblings spell it, so a page that
    ever links another project's blob/master is not silently rewritten to name a go2cs tag. Such a
    page lands in the ordinary zero, which -- unlike the source link's zero one arm up -- cannot be
    told from a page whose package simply discloses nothing. That is the price of an optional link and
    it is stated rather than papered over.

    Read AND write through [System.IO.File] at UTF-8/no-BOM, replacing rather than splitting lines,
    for the reasons stated at Update-FrozenProofPage above: PS 5.1's Get-Content reads a BOM-less
    UTF-8 file as ANSI and Out-File re-encodes the damage, and the page's line endings must survive
    exactly as the converter emitted them.
.OUTPUTS
    PSCustomObject: DisclosureLinks (int) -- substitutions made, 0 or 1.
#>
function Update-FrozenProofPageDisclosureLink {
    param(
        [Parameter(Mandatory)][string] $Path,
        [Parameter(Mandatory)][string] $Version
    )

    if (-not (Test-Path -LiteralPath $Path)) { throw "Update-FrozenProofPageDisclosureLink: no page at $Path." }
    if ([string]::IsNullOrWhiteSpace($Version)) { throw 'Update-FrozenProofPageDisclosureLink: the version is empty.' }

    $disclosureFrom = 'github.com/ritchiecarroll/go2cs/blob/master/'
    $disclosureTo = "github.com/ritchiecarroll/go2cs/blob/nuget-$Version/"

    $text = [System.IO.File]::ReadAllText($Path)

    $before = ([regex]::Matches($text, [regex]::Escape($disclosureFrom))).Count

    if ($before -gt 1) {
        throw ("Update-FrozenProofPageDisclosureLink: $Path carries $before disclosure-manifest pointer(s) " +
               "('$disclosureFrom') where a proof page carries at most one. A second pointer on one page is a " +
               "template change nobody has made yet -- rule on it rather than have it pinned behind an arm " +
               "written for one.")
    }

    # Nothing to do, and that is ORDINARY: 168 of the 204 pages at 1.23.12.3 name no manifest. The
    # file is left untouched rather than rewritten byte-identically, so a page that discloses nothing
    # cannot have its encoding or line endings changed by a transform that had no work to do.
    if ($before -eq 0) {
        return [pscustomobject]@{ DisclosureLinks = 0 }
    }

    $text = $text.Replace($disclosureFrom, $disclosureTo)

    $after = ([regex]::Matches($text, [regex]::Escape($disclosureFrom))).Count

    if ($after -ne 0) {
        throw ("Update-FrozenProofPageDisclosureLink: $Path still carries $after disclosure-manifest pointer(s) " +
               "('$disclosureFrom') after the pin. Replace moves every occurrence, so a survivor means the " +
               "target spelling now contains the source -- the pattern has been widened too far.")
    }

    [System.IO.File]::WriteAllText($Path, $text, (New-Object System.Text.UTF8Encoding($false)))

    return [pscustomobject]@{
        DisclosureLinks = $before
    }
}

<#
.SYNOPSIS
    Pin a FROZEN roster's package-column source links onto the release that roster belongs to.
.DESCRIPTION
    ConvertTo-FrozenRosterText above retargets the links a roster owns as a DOCUMENT -- its proof
    links onto the sibling frozen pages, its out-of-docs links onto the deeper path. It leaves the
    PACKAGE COLUMN alone, and that column is one link per row naming tree/master: the converted C#
    for that package, on a branch that keeps moving. A frozen roster whose package column names
    master is not frozen either, by exactly the argument Update-FrozenProofPage above makes for a
    frozen proof page -- so the snapshot's roster is pinned onto the same signed tag,
    nuget-<version>, that the pages beside it name.

    A SIBLING rather than a third substitution inside ConvertTo-FrozenRosterText. That function
    turns a LIVING roster into a frozen one and cannot be re-run on a roster it has already
    transformed: it would insert a second note and relocate the already-relocated links. The
    1.23.12.3 roster was frozen before this rule existed, so the one-off that pinned it needed a
    transform it could run ALONE, on a file -- exactly as Update-FrozenProofPage is run alone. One
    definition, two callers, so a committed snapshot cannot drift from the code that will produce
    its successors.

    THE COUNT IS THE ROSTER'S OWN ROW COUNT, and a disagreement is a throw. Every roster row is a
    package whose first cell links its converted source, so the substitutions and the rows are the
    same number by construction: a shortfall is rows whose link is spelled some other way, which
    would publish still naming master, and a surplus is the pattern reaching something that is not a
    package link. The rows are counted by Get-ValidatedRosterRows -- the roster parser of record,
    the one check-roster-format.ps1 counts with -- rather than by a number written here, so the
    assertion cannot go stale the day a package banks. Zero rows is its own throw: 0 -eq 0 is an
    assertion that cannot fail, and a roster this function cannot find rows in is not one it should
    be silently rewriting.

    THE NOTE'S LINK IS NOT A PACKAGE LINK and must survive untouched. A frozen roster carries
    exactly one '](../../ValidatedTestPackages.md)': the note's deliberate pointer at the LIVING
    roster, written on purpose by ConvertTo-FrozenRosterText and the reason push-nuget.ps1 excludes
    the roster from Update-FrozenProofPage. Both directions are asserted. A count that is not one is
    template drift -- zero means the note's pointer went missing, more than one means the snapshot
    holds a second walk back into the living tree that reads as the note's and is not. A count this
    transform CHANGED means the source pattern was widened until it reached the note; that arm is
    inert today by construction, the two strings having nothing in common, and it exists to fail the
    day somebody widens the pattern rather than to measure anything now.

    The pattern is anchored on the REPOSITORY, and spelled exactly as Update-FrozenProofPage spells
    it, for the same reason: a future roster linking some other project's master must not be
    silently rewritten to name a go2cs tag. The two spellings are deliberately duplicated rather
    than hoisted -- hoisting would put the landed, gated body of Update-FrozenProofPage into this
    change's diff -- so a repository move edits both, which is what the count assertions here and
    there are for.

    Read AND write through [System.IO.File] at UTF-8/no-BOM, for the reason stated at
    Update-FrozenProofPage: PS 5.1's Get-Content reads a BOM-less UTF-8 file as ANSI and Out-File
    re-encodes the damage, and this file ships to go2cs.net. Replace, not line splitting, so the
    roster's line endings survive exactly as ConvertTo-FrozenRosterText joined them.
.OUTPUTS
    PSCustomObject: SourceLinks (int), Rows (int), NoteLinks (int).
#>
function Update-FrozenRosterSourceLinks {
    param(
        [Parameter(Mandatory)][string] $Path,
        [Parameter(Mandatory)][string] $Version
    )

    if (-not (Test-Path -LiteralPath $Path)) { throw "Update-FrozenRosterSourceLinks: no roster at $Path." }
    if ([string]::IsNullOrWhiteSpace($Version)) { throw 'Update-FrozenRosterSourceLinks: the version is empty.' }

    $sourceFrom = 'github.com/ritchiecarroll/go2cs/tree/master/'
    $sourceTo = "github.com/ritchiecarroll/go2cs/tree/nuget-$Version/"
    # No trailing ')' on the note pattern: the link may legitimately carry a '#anchor' suffix, and
    # the prefix is what is being counted.
    $noteLink = '](../../ValidatedTestPackages.md'

    $text = [System.IO.File]::ReadAllText($Path)

    $rows = @(Get-ValidatedRosterRows -Path $Path).Count
    $sourceLinks = ([regex]::Matches($text, [regex]::Escape($sourceFrom))).Count
    $noteLinksBefore = ([regex]::Matches($text, [regex]::Escape($noteLink))).Count

    if ($rows -eq 0) {
        throw ("Update-FrozenRosterSourceLinks: $Path has no roster rows, so the count this transform is " +
               "asserted against is zero and could only ever agree with itself. A snapshot roster with no " +
               "rows is not one to rewrite -- reconcile the frozen page with the roster table's shape.")
    }

    if ($sourceLinks -ne $rows) {
        throw ("Update-FrozenRosterSourceLinks: $Path carries $sourceLinks package-column source link(s) " +
               "('$sourceFrom') across $rows roster row(s). Every row links its converted source, so the two " +
               "must be equal: fewer links than rows would publish rows still naming a moving branch, more " +
               "means the pattern reached something that is not a package link. Reconcile the roster's package " +
               "column with this transform rather than publishing the snapshot as it stands.")
    }

    if ($noteLinksBefore -ne 1) {
        throw ("Update-FrozenRosterSourceLinks: $Path carries $noteLinksBefore pointer(s) at the living roster " +
               "('$noteLink') where the frozen-snapshot note carries exactly one. Zero means the note's " +
               "deliberate pointer went missing; more than one means the snapshot holds another walk back into " +
               "the living tree that reads as the note's and is not.")
    }

    $text = $text.Replace($sourceFrom, $sourceTo)

    $noteLinksAfter = ([regex]::Matches($text, [regex]::Escape($noteLink))).Count

    if ($noteLinksAfter -ne $noteLinksBefore) {
        throw ("Update-FrozenRosterSourceLinks: pinning the source links changed the number of living-roster " +
               "pointer(s) in $Path from $noteLinksBefore to $noteLinksAfter. The note's link is deliberate and " +
               "this transform must not reach it -- the source pattern has been widened too far.")
    }

    [System.IO.File]::WriteAllText($Path, $text, (New-Object System.Text.UTF8Encoding($false)))

    return [pscustomobject]@{
        SourceLinks = $sourceLinks
        Rows        = $rows
        NoteLinks   = $noteLinksAfter
    }
}

<#
.SYNOPSIS
    Pin a FROZEN roster's disclosure-manifest prose pointer onto the release that roster belongs to.
.DESCRIPTION
    Update-FrozenRosterSourceLinks above pins the roster's PACKAGE COLUMN -- one tree/master link per
    row, asserted against the row count. The roster carries one more link at a moving target that is
    neither in that column nor in that count: a prose sentence pointing at a package's hand-owned
    go2cs_test_disclosures.json, spelled blob/master rather than tree/master. The same argument reaches
    it -- a frozen roster naming a moving branch is not frozen -- so it is pinned onto the same signed
    tag, nuget-<version>, that the package column and the pages beside it name.

    A SIBLING rather than a second phase inside Update-FrozenRosterSourceLinks, and the reason is that
    function's own count. Its whole strength is that the number it asserts IS the roster's row count,
    read from Get-ValidatedRosterRows, so it cannot go stale the day a package banks. This pointer is
    prose, not a package-column link: folding it in would make the substitutions 205 against 204 rows,
    or force a second count shape into the one function whose count describes itself. That is the
    reasoning the residual of 339d2fdc7 gives for leaving it out -- honoured here rather than reversed.

    THE COUNT IS A FIXED ONE, WHICH IS WEAKER THAN ITS SIBLING'S DERIVED COUNT -- said rather than
    dressed up. Nothing structural in a roster equals "number of disclosure-manifest pointers": the
    roster names one package's manifest because its prose happens to discuss one, so the expectation is
    a census of the LIVING roster (docs\ValidatedTestPackages.md carries exactly one) and not a
    derivation. Both directions are asserted anyway, and both say something. ZERO means either the
    prose pointer went missing or this transform has already run -- which makes it non-idempotent BY
    DESIGN, the property that makes the count a live check rather than a number that can only ever
    agree. MORE THAN ONE means the roster grew a second such pointer that a fixed count does not
    describe, and whoever added it should rule on it rather than have it pinned behind a number
    written for one.

    The after-count arm is a TRIPWIRE, not a measurement, and is inert today by construction: String's
    Replace moves every occurrence, and 'blob/nuget-<version>/' cannot contain 'blob/master/', so the
    count after is zero whenever the count before was one. It exists to fail the day somebody widens
    the pattern, which is exactly the standing the sibling above gives its note-link arm.

    The pattern is anchored on the REPOSITORY, spelled as both siblings spell it, so a roster that ever
    links another project's blob/master is not silently rewritten to name a go2cs tag. Read AND write
    through [System.IO.File] at UTF-8/no-BOM, replacing rather than splitting lines, for the reasons
    stated at Update-FrozenProofPage: PS 5.1's Get-Content reads a BOM-less UTF-8 file as ANSI and
    Out-File re-encodes the damage, and the roster's line endings must survive exactly as
    ConvertTo-FrozenRosterText joined them.
.OUTPUTS
    PSCustomObject: DisclosureLinks (int) -- substitutions made.
#>
function Update-FrozenRosterDisclosureLink {
    param(
        [Parameter(Mandatory)][string] $Path,
        [Parameter(Mandatory)][string] $Version
    )

    if (-not (Test-Path -LiteralPath $Path)) { throw "Update-FrozenRosterDisclosureLink: no roster at $Path." }
    if ([string]::IsNullOrWhiteSpace($Version)) { throw 'Update-FrozenRosterDisclosureLink: the version is empty.' }

    $disclosureFrom = 'github.com/ritchiecarroll/go2cs/blob/master/'
    $disclosureTo = "github.com/ritchiecarroll/go2cs/blob/nuget-$Version/"

    $text = [System.IO.File]::ReadAllText($Path)

    $before = ([regex]::Matches($text, [regex]::Escape($disclosureFrom))).Count

    if ($before -ne 1) {
        throw ("Update-FrozenRosterDisclosureLink: $Path carries $before disclosure-manifest pointer(s) " +
               "('$disclosureFrom') where a frozen roster carries exactly one. Zero means the prose pointer " +
               "went missing, or that this transform has already run; more than one means the roster grew a " +
               "pointer a fixed count does not describe. Reconcile the roster's prose with this transform " +
               "rather than publishing the snapshot as it stands.")
    }

    $text = $text.Replace($disclosureFrom, $disclosureTo)

    $after = ([regex]::Matches($text, [regex]::Escape($disclosureFrom))).Count

    if ($after -ne 0) {
        throw ("Update-FrozenRosterDisclosureLink: $Path still carries $after disclosure-manifest pointer(s) " +
               "('$disclosureFrom') after the pin. Replace moves every occurrence, so a survivor means the " +
               "target spelling now contains the source -- the pattern has been widened too far.")
    }

    [System.IO.File]::WriteAllText($Path, $text, (New-Object System.Text.UTF8Encoding($false)))

    return [pscustomobject]@{
        DisclosureLinks = $before
    }
}

<#
.SYNOPSIS
    The expectation a row must be validated against on a given GOOS.
.DESCRIPTION
    A verdict count is a fact about (package, OS). Under the row's own annotation for this GOOS,
    that annotation is the expectation; otherwise the Windows columns stand -- which is exactly
    right on Windows, and on another OS is the honest interim the ruling names: the row is compared
    against the Windows number and reported comparison-validated-at-count when it differs.

    Source says which of the two answered: 'columns' or the goos key.
#>
function Get-RosterRowExpectation {
    param(
        [Parameter(Mandatory)][PSCustomObject] $Row,
        [Parameter(Mandatory)][string] $Goos
    )

    $key = $Goos.Trim().ToLowerInvariant()

    if ($key -ne 'windows' -and $Row.OS -and $Row.OS.ContainsKey($key)) {
        return [PSCustomObject]@{
            Expected   = $Row.OS[$key].Expected
            Disclosed  = $Row.OS[$key].Disclosed
            Source     = $key
            Applicable = $Row.OS[$key].Applicable
        }
    }

    return [PSCustomObject]@{
        Expected   = $Row.Expected
        Disclosed  = $Row.Disclosed
        Source     = 'columns'
        Applicable = $true
    }
}

<#
.SYNOPSIS
    The converter arguments one execution config implies. Pure -- no I/O, no state.
.DESCRIPTION
    The single place that knows what a config NAME means on a command line, so the roster records an
    intent and the sweep spells no flags of its own. An empty/absent config is the default path and
    contributes NOTHING: the invocation an unannotated row produces is character-for-character the
    invocation it produced before this annotation existed, which is the guarantee the ruling rests on.

    `release-tc0` maps to the converter's `-test-config Release` (2026-09-02: generalized from the
    retired `-test-release-tc0` bool into `-test-config Debug|Release` + `-test-tiered`; Release's own
    DEFAULT is untiered, exactly matching this annotation's meaning, so no `-test-tiered` is added
    here -- this mapping is the config's ORIGINAL meaning, not a new one). The honest seam for BOTH
    halves of the config, still: the publish CONFIGURATION is decided inside the converter's own
    `dotnet publish` invocation (publishTestHost, testConversion.go), where Release passes an explicit
    `-p:go2csPath` -- the template's `Condition="'$(go2csPath)'==''"` guard is written to be
    overridden exactly that way, so a Release publish still binds THIS tree rather than the deployed
    `~/go2cs` root. The run half, `DOTNET_TieredCompilation=0` by default at Release, rides the same
    flag (testHostRunEnv), because a Release publish alone does not retire tier-0: a program can start
    at tier-0 and simply never run long enough to be promoted.

    An unknown config throws rather than degrading to the default -- see the parser's refusal above
    for why a silently-ignored config is the failure this design exists to prevent.
.OUTPUTS
    A string[] of converter arguments; empty for the default path.
#>
function Get-RosterExecutionArgs {
    param([string] $Execution)

    if ([string]::IsNullOrWhiteSpace($Execution)) { return @() }

    switch ($Execution) {
        'release-tc0' { return @('-test-config', 'Release') }
        # The opt-OUT. -test-tiered is meaningless without -test-config Release (the converter says
        # so on the flag itself), so both are passed rather than relying on the converter default
        # being Release -- an execution annotation states its whole configuration, so it cannot
        # silently change meaning if a default moves again.
        'release-tiered' { return @('-test-config', 'Release', '-test-tiered') }
        default {
            throw ("Unknown execution config '$Execution'. Known configs: " +
                "$($RosterExecutionValues -join ', ').")
        }
    }
}

<#
.SYNOPSIS
    Classifies one row's live result against the expectation in force. Pure -- no I/O, no state.
.DESCRIPTION
    The whole three-bucket rule in one place, so it can be exercised without running the gate:

      pass              the count met the expectation in force (and, for an annotated row, so did
                        the disclosed count)
      host-conditional  the surplus was PROVEN to be the row's named host-conditional verdicts
                        (that proof is evidence-based and lives in the sweep; this takes its answer)
      host-limit        a shortfall PROVEN to be a registered block the converted side could not
                        produce on this host, absorbed by the block root's own COMMITTED host-limit
                        disclosure. The third host state (Test-HostLimitDelta); same evidence
                        discipline as the two above -- this takes its answer
      host-conditional-disclosure
                        a named disclosure entry FIRED on this host and not on the banking host, so
                        exactly that verdict moved from the matched column to the disclosed column
                        -- PROVEN from the live record by Test-HostConditionalDisclosureDelta (the
                        sweep reads the evidence; this takes its answer). Checked before
                        disclosed-moved below, since here the disclosed count legitimately moved
      disclosed-moved   an annotated row's matching count agreed and its DISCLOSED count did not --
                        roster maintenance, never host capability, and never absorbed
      unbanked-count    comparison-validated-at-count: a validated run on an OS this row has no
                        expectation for. Neither a pass nor a drift failure
      not-applicable    the row is annotated `<goos>: n/a` -- the package cannot exist on this OS,
                        so there is nothing to measure, now or ever (ruled 2026-08-29). The sweep
                        removes such rows before running them; this answer exists so a caller that
                        classifies anyway gets the truth rather than a count comparison against null
      count             the banked expectation and reality disagree; one of them is now wrong

    On Windows the reachable set is exactly what it was before the OS dimension existed -- pass,
    host-conditional, count -- because Source is 'columns' for every row (no disclosed check, per
    below) and TargetGoos is 'windows' (no unbanked bucket).

    The COLUMNS path deliberately does not check the disclosed count: a banked Windows row's
    disclosures are enforced where they always were, against the committed proof page, and a second
    enforcement here would move a banked path's verdicts for no new information. An annotation's
    `+ D` half is new ground and is checked where it is written.
#>
function Get-SweepRowClassification {
    param(
        [Parameter(Mandatory)][PSCustomObject] $Expectation,
        [Parameter(Mandatory)][int] $Got,
        [int] $GotDisclosed = 0,
        [Parameter(Mandatory)][string] $TargetGoos,
        [switch] $HostConditionalAccepted,
        [switch] $CapabilityAbsentAccepted,
        [switch] $HostLimitAccepted,
        [switch] $HostConditionalDisclosureAccepted
    )

    # Before any count math: an inapplicable expectation has null counts, and comparing against
    # them would classify by accident.
    if ($Expectation.PSObject.Properties['Applicable'] -and -not $Expectation.Applicable) { return 'not-applicable' }

    $disclosedAgrees = ($Expectation.Source -eq 'columns') -or ($GotDisclosed -eq $Expectation.Disclosed)

    # The one absorption whose shape IS a moved disclosed count, so it is honoured before the
    # disclosed-moved answer below -- and only on the caller's proof (Test-HostConditionalDisclosureDelta).
    if ($HostConditionalDisclosureAccepted) { return 'host-conditional-disclosure' }
    if (-not $disclosedAgrees) { return 'disclosed-moved' }
    if ($HostConditionalAccepted) { return 'host-conditional' }
    if ($CapabilityAbsentAccepted) { return 'capability-absent' }
    if ($HostLimitAccepted) { return 'host-limit' }
    if ($Got -eq $Expectation.Expected) { return 'pass' }

    if ($TargetGoos.Trim().ToLowerInvariant() -ne 'windows' -and $Expectation.Source -eq 'columns') {
        return 'unbanked-count'
    }

    return 'count'
}

# ---- capability-conditional verdicts (the MIRROR of the host-conditional surplus above) ----------
# Get-SweepRowClassification above assumes a surplus is the only host-dependent shape: the roster
# banks a FLOOR and a more-capable host produces EXTRA verdicts (host-conditional). Some
# capability-bound test blocks run the opposite way -- the roster banks the CEILING, every case the
# capability enables, and a host lacking the prerequisite never spawns the case matrix at all, so
# the whole block collapses to the ONE top-level verdict. crypto/tls's TestBogoSuite is the first of
# these (the BoGo/BoringSSL shim runner): 3,243 sub-verdicts -- 1 parent + 861 pass + 2,381 skip --
# collapse to exactly one. "A lost verdict is never host-conditional" stays true for every OTHER
# shortfall: a caller engages this ONLY for a package it registers, and ONLY when the shortfall
# matches that package's block size exactly -- run-validated-sweep.ps1's
# $capabilityConditionalBlocks table is where that registration lives; this function is the pure
# rule, proven directly by check-roster-format.ps1's fixtures rather than through a roster row (the
# evidence is a comparison record and a proof page, not a table cell).
#
# ⚠ WHAT THE COLLAPSED VERDICT ACTUALLY IS, MEASURED (2026-08-28, the bogo skip-parity lane).
# This rule was first written expecting SKIP on both sides -- "Go's own oracle skips identically
# absent the runner". Go's oracle does no such thing, and the difference is the whole mechanism.
# `TestBogoSuite`'s only skip branches (`bogo_shim_test.go:337-347`) are short-mode, js/wasip1, no
# `go build`, no exec, and the builders' Windows-flake guard -- NONE of them is "the BoGo runner is
# absent". Absent the runner the test **FAILS**, at `bogo_shim_test.go:364`
# (`t.Fatalf("failed to download boringssl: %s", err)`) -- measured directly by pointing GOMODCACHE
# at an empty directory with GOPROXY=off: `--- FAIL: TestBogoSuite (0.35s)`, never a skip.
# So the shape a genuinely capability-less host produces is Go **fail** / C# **fail**, which is
# precisely the SECOND accepted shape of a host-conditional disclosure -- and the converter accounts
# that root as DISCLOSED rather than matched (`matchTerminalStatuses`, testConversion.go), so on
# such a host the live disclosed count is the banked one PLUS the root, by construction. Demanding
# skip-on-both AND an unmoved disclosed count therefore made this rule unfireable for its only
# registered member, in two independent ways at once.
#
# What is NOT capability-absent, and must stay red: the block root AGREEING is the load-bearing
# evidence, because a Go side that PASSED established the whole matrix -- the shortfall is then the
# converted side's alone (crypto/tls on a host whose managed shims miss the BoGo runner's own
# 600 s deadline reads exactly this way: Go pass / C# fail, same 3,243 shortfall, same 400 matched).
# Absorbing that would wave through a real, measured divergence, so the rule below accepts only an
# AGREEING NON-PASS root and refuses every other combination by name.
function Test-CapabilityAbsentDelta {
    param(
        [int] $Expected,           # banked matching-verdict count (roster column 2, the CEILING here)
        [int] $Disclosed,
        [PSCustomObject] $Block,   # @{ Test = <top-level name>; BlockSize = <int> }
        [int] $Got,
        $Comparison,
        [string[]] $BankedNames
    )

    function New-CapabilityAbsentResult([bool] $accepted, [string] $reason) {
        return [PSCustomObject]@{ Accepted = $accepted; Reason = $reason }
    }

    $shortfall = $Expected - $Got
    if ($shortfall -ne $Block.BlockSize) {
        return New-CapabilityAbsentResult $false "shortfall $shortfall does not match $($Block.Test)'s registered block size $($Block.BlockSize) -- a lost verdict outside the named block is never capability-conditional"
    }
    if ($null -eq $Comparison -or $null -eq $Comparison.go -or $null -eq $Comparison.csharp) {
        return New-CapabilityAbsentResult $false 'comparison record carries no per-test verdict maps'
    }

    if ($BankedNames.Count -ne ($Expected + $Disclosed)) {
        return New-CapabilityAbsentResult $false "committed proof page lists $($BankedNames.Count) verdicts where the roster banks $Expected matched + $Disclosed disclosed -- page and table disagree"
    }

    # The block is every banked name that IS the top-level test or one of its Go subtests (the '/'
    # naming go test -json itself uses). This must be exactly BlockSize names, or the committed
    # evidence disagrees with the registered size and nothing below can be trusted.
    $blockPrefix = "$($Block.Test)/"
    $blockNames = @($BankedNames | Where-Object { $_ -eq $Block.Test -or $_.StartsWith($blockPrefix) })
    if ($blockNames.Count -ne $Block.BlockSize) {
        return New-CapabilityAbsentResult $false "the committed proof page names $($blockNames.Count) verdicts under $($Block.Test), not the registered $($Block.BlockSize) -- re-derive the block size before trusting this row"
    }

    $goMap = $Comparison.go
    $csMap = $Comparison.csharp
    $liveNames = @($goMap.Keys)

    # Every banked name OUTSIDE the block must still be present live -- the mechanism absorbs the
    # named block collapsing, nothing else.
    $expectedOutside = @($BankedNames | Where-Object { $blockNames -notcontains $_ })
    $missingOutside = @($expectedOutside | Where-Object { $liveNames -notcontains $_ })
    if ($missingOutside.Count -gt 0) {
        return New-CapabilityAbsentResult $false "banked verdicts outside the block missing from this run: $($missingOutside -join ', ')"
    }

    # No block subtest may appear at all (they were never spawned), and the top-level name must be
    # present and AGREEING -- see the measured note above for why "agreeing" is the test and "skip"
    # is not.
    $liveBlockNames = @($liveNames | Where-Object { $_ -eq $Block.Test -or $_.StartsWith($blockPrefix) })
    if (@($liveBlockNames | Where-Object { $_ -ne $Block.Test }).Count -gt 0) {
        return New-CapabilityAbsentResult $false "subtests under $($Block.Test) appear in this run -- the capability is not cleanly absent, re-diagnose rather than absorb"
    }
    if ($liveBlockNames -notcontains $Block.Test) {
        return New-CapabilityAbsentResult $false "$($Block.Test) itself is missing from this run -- an absent capability must still report its one collapsed verdict"
    }

    $goVerdict = $goMap[$Block.Test]
    $csVerdict = if (-not $csMap.ContainsKey($Block.Test)) { 'absent' } else { $csMap[$Block.Test] }
    if ($goVerdict -ne $csVerdict) {
        return New-CapabilityAbsentResult $false "$($Block.Test): go '$goVerdict' vs C# '$csVerdict' -- an absent capability collapses IDENTICALLY on both runtimes; a disagreement here is a real divergence on a host that has the capability, not a missing one"
    }
    if ($goVerdict -ne 'skip' -and $goVerdict -ne 'fail') {
        return New-CapabilityAbsentResult $false "$($Block.Test): both runtimes report '$goVerdict' -- a PASSING oracle established the case matrix, so the shortfall is the converted side's and is never capability-absent"
    }

    # THE discriminator, and the reason agreement alone is not enough (measured 2026-08-28 on the
    # i7-5820K). A host that HAS the capability but whose converted side misses the runner's own
    # deadline can ALSO show fail/fail: Go fans out all 3,242 BoGo cases and fails a handful of them
    # flakily, the converted side fails at the wall, and every count above is bit-for-bit identical
    # to the capability-absent shape. What is not identical is the FAN-OUT: those 3,242 Go-side rows
    # exist, and `matchTerminalStatuses` withdraws them by name into the comparison record. An
    # absent capability produces NO such rows, because the test dies before its case matrix exists.
    # So a withdrawal under the block is proof the capability was present, and the shortfall is the
    # converted side's alone. (`withdrawn` is omitempty, so a record without one withdrew nothing.)
    $withdrawnUnderBlock = @()
    if ($null -ne $Comparison.withdrawn) {
        $withdrawnUnderBlock = @($Comparison.withdrawn | Where-Object { $_ -eq $Block.Test -or $_.StartsWith($blockPrefix) })
    }
    if ($withdrawnUnderBlock.Count -gt 0) {
        return New-CapabilityAbsentResult $false "$($withdrawnUnderBlock.Count) Go-side verdict(s) under $($Block.Test) were withdrawn -- the case matrix DID fan out, so the capability was present on this host and the lost verdicts are the converted side's, not the capability's"
    }

    # The disclosed accounting is decided by WHICH collapsed verdict this is, because the converter
    # accounts the two shapes differently and the roster's banked number was taken on a host that
    # had the capability:
    #   fail/fail -- the host-conditional shape. matchTerminalStatuses accounts the annotated root
    #                as DISCLOSED (never as an agreed match), so the live count is banked + 1 and
    #                the extra entry must BE this block's root.
    #   skip/skip -- no disclosure fires (a skip is not the pinned failure), so the count is banked
    #                exactly and the root must NOT appear among the disclosures.
    # Anything else is a disclosure that moved for some unrelated reason, which is what this check
    # has always existed to catch.
    $liveDisclosedEntries = @()
    if ($null -ne $Comparison.disclosed) { $liveDisclosedEntries = @($Comparison.disclosed) }
    $liveDisclosed = $liveDisclosedEntries.Count
    # A disclosure entry reads "<TestName> (<class>): <reason>"; the name is everything before the
    # first " (" and is compared whole, so a prefix can never pass for the root.
    $rootIsDisclosed = @($liveDisclosedEntries | Where-Object { ($_ -split ' \(', 2)[0] -eq $Block.Test }).Count -gt 0
    $expectedDisclosed = if ($goVerdict -eq 'fail') { $Disclosed + 1 } else { $Disclosed }
    if ($liveDisclosed -ne $expectedDisclosed) {
        return New-CapabilityAbsentResult $false "disclosed count moved ($liveDisclosed live vs $expectedDisclosed expected for an agreeing '$goVerdict' collapse of $($Block.Test), against $Disclosed banked) -- not a capability-conditional shape"
    }
    if ($goVerdict -eq 'fail' -and -not $rootIsDisclosed) {
        return New-CapabilityAbsentResult $false "$($Block.Test) fails on both runtimes but is not among this run's disclosures -- the extra disclosure is some other row, so this is not the collapse it looks like"
    }
    if ($goVerdict -eq 'skip' -and $rootIsDisclosed) {
        return New-CapabilityAbsentResult $false "$($Block.Test) skips on both runtimes yet is disclosed -- the pinned divergence fired on a shape it does not describe, re-diagnose rather than absorb"
    }

    # Nothing live may go unaccounted for: the outside names plus the one collapsed root, exactly.
    $accountedFor = @($expectedOutside) + @($Block.Test)
    $unaccounted = @($liveNames | Where-Object { $accountedFor -notcontains $_ })
    if ($unaccounted.Count -gt 0) {
        return New-CapabilityAbsentResult $false "live verdicts this row does not account for: $($unaccounted -join ', ')"
    }

    return New-CapabilityAbsentResult $true $null
}

# ---- host-conditional DISCLOSURES (the FOURTH absorption: one verdict changes COLUMN) ------------
# The three rules above absorb a count that MOVED -- a surplus the host's capabilities add, a
# block an absent capability collapses, a block a present-but-limited host cannot finish. This one
# absorbs a count that did not move at all: the SAME verdict sits in the matched column on the
# banking host and in the disclosed column on another, because the disclosure entry that names it
# fires on a property of the HOST. os/exec's TestExtraFiles is the first member (Q31, 2026-09-04):
# exec_test.go's init() scans descriptors 3..100 and the test t.Skips outright if any is open; the
# single-file published host on a container holds 97 of them (Go=pass / C#=skip, the platform-skip
# entry fires, the row reads 86 + 2), the fleet's Linux bank host holds none (Go=pass / C#=pass,
# matched, the row reads 87 + 1 -- 438728de0). Neither reading is wrong, and before this rule the
# sweep called the container's `disclosed-moved` because Get-SweepRowClassification answers that
# before any absorption is consulted. The roster names the members and the CONDITION
# (`host-conditional-disclosure (<condition>): `TestExtraFiles``) beside the floor it keeps.
#
# What it accepts, and only that: the matched count SHORT of the floor by k >= 1, the disclosed
# count OVER its expectation by exactly the same k, exactly k of the annotation's names present
# among the live record's disclosures, and each of those reading Go=pass / C#=skip in the verdict
# maps -- the platform-skip shape this member was measured in. A lost verdict beside a fired one
# (85 + 2), a second disclosure the annotation does not name (86 + 3, or 86 + 2 with another name),
# and a fired name in any other shape (Go=pass / C#=fail) are all refused by name. Evidence is the
# live comparison record ALONE: the surplus rule's committed-proof-page cross-check rejects
# OS-annotated rows by design (the page is the banking host's, Windows-shaped), and this rule must
# not inherit a rejection that has nothing to do with its own question. Pure, proven directly by
# check-roster-format.ps1's fixtures; run-validated-sweep.ps1's Get-HostConditionalDisclosureVerdict
# reads the evidence and calls it.
function Test-HostConditionalDisclosureDelta {
    param(
        [int] $Expected,           # banked matching-verdict count in force (the floor)
        [int] $Disclosed,          # banked disclosed count in force
        [string[]] $Names,         # the roster's host-conditional-disclosure names
        [int] $Got,                # the live run's validated count
        [int] $GotDisclosed,       # the live run's disclosed count
        $Comparison                # the run's go2cs_test_comparison.json via ConvertFrom-ComparisonRecord
    )

    function New-HostConditionalDisclosureResult([bool] $accepted, [string[]] $fired, [string] $reason) {
        return [PSCustomObject]@{ Accepted = $accepted; Fired = $fired; Reason = $reason }
    }

    if ($null -eq $Names -or @($Names).Count -eq 0) {
        return New-HostConditionalDisclosureResult $false @() 'the row names no host-conditional disclosures'
    }

    $k = $Expected - $Got
    if ($k -lt 1) {
        return New-HostConditionalDisclosureResult $false @() "count $Got is not below the floor $Expected -- nothing moved into the disclosed column"
    }
    if (($GotDisclosed - $Disclosed) -ne $k) {
        return New-HostConditionalDisclosureResult $false @() "matched fell by $k but disclosed moved by $($GotDisclosed - $Disclosed) ($GotDisclosed live vs $Disclosed banked) -- not one verdict changing column"
    }
    if ($null -eq $Comparison -or $null -eq $Comparison.go -or $null -eq $Comparison.csharp) {
        return New-HostConditionalDisclosureResult $false @() 'comparison record carries no per-test verdict maps'
    }

    # The live disclosures, by name: each entry is "Name (class): reason" as the converter writes it.
    $liveDisclosedNames = @()
    if ($null -ne $Comparison.disclosed) {
        $liveDisclosedNames = @(@($Comparison.disclosed) | ForEach-Object {
            if ("$_" -match '^(\S+)\s+\(') { $Matches[1] } else { "$_" }
        })
    }

    $fired = @($liveDisclosedNames | Where-Object { $Names -contains $_ })
    if ($fired.Count -ne $k) {
        return New-HostConditionalDisclosureResult $false @() "$($fired.Count) of the named host-conditional disclosures fired ($($fired -join ', ')) but the columns moved by $k -- the difference is some OTHER verdict"
    }

    $goMap = $Comparison.go
    $csMap = $Comparison.csharp
    foreach ($name in $fired) {
        if (-not $goMap.ContainsKey($name) -or -not $csMap.ContainsKey($name)) {
            return New-HostConditionalDisclosureResult $false @() "$name is disclosed but absent from the verdict maps"
        }
        if ($goMap[$name] -ne 'pass' -or $csMap[$name] -ne 'skip') {
            return New-HostConditionalDisclosureResult $false @() "$name fired in the shape Go=$($goMap[$name]) / C#=$($csMap[$name]); the host-conditional disclosure shape is Go=pass / C#=skip and nothing else is absorbed"
        }
    }

    return New-HostConditionalDisclosureResult $true $fired $null
}

# ---- host-limited verdicts (the THIRD host state, and the SECOND shortfall shape) -----------------
# The rule above owns the shortfall an ABSENT capability produces, and its LAST check refuses, by
# name, every shortfall whose Go side FANNED THE MATRIX OUT -- correctly, on the evidence IT reads:
# the case matrix existed, so the lost verdicts are the converted side's alone. What that rule cannot
# see is the one artifact that changes what such a loss MEANS: a COMMITTED `host-limit` disclosure
# pinning the block root. That class is the project's existing, reviewed vocabulary for exactly this
# shape -- a divergence the converted host's DEPLOYMENT SHAPE structurally cannot close, banked with
# a failure signature and a self-retiring condition rather than waved through -- and the converter's
# own compare oracle already accepts the row under it (matchTerminalStatuses), which is why the
# pipeline reports the very same run as `status: validated, matched: true` while only the sweep's
# COUNT gate refuses.
#
# So a capability-conditional block has THREE host states, not two:
#
#   runner present, host fast enough   the block fans out on both sides and every sub-verdict
#                                      MATCHES -- the roster's banked ceiling (crypto/tls 3643 + 1)
#   runner absent                      the matrix never exists: the block collapses to one verdict on
#                                      BOTH runtimes with NO fan-out, and the two counts fall
#                                      together -- Test-CapabilityAbsentDelta
#   runner present, host too slow      Go fans the matrix out, the converted side dies on the
#                                      runner's own fixed deadline, and the block root becomes a
#                                      DISCLOSED divergence -- this rule (crypto/tls 400 + 2)
#
# THE DISCRIMINATOR IS THE FAN-OUT, NOT THE ROOT'S VERDICT (measured 2026-09-01, i7 coordinator, the
# run this rule was written against). The tempting reading is that state 3 is the Go-pass/C#-fail
# pair and state 2 the agreeing non-pass one; it is NOT, and a rule built that way would refuse the
# very host it exists for. crypto/tls's own manifest documents both arms and the sweep measured the
# second: `TestBogoSuite` go='fail' C#='fail', 3,242 rows withdrawn, root disclosed `host-limit`,
# status validated. Go's oracle fans out all 3,242 cases in under a minute and its ROOT still fails
# on a handful of them; the converted side dies at the 600 s wall with the pinned signature; and the
# compare oracle admits either arm (hostConditionalFailureMatches takes the primary signature OR the
# host-conditional one). So the two rules partition the space on the WITHDRAWALS -- that rule refuses
# any, this one requires exactly the block's own -- and no run can be read both ways.
#
# The third state is NOT a weaker second: it carries strictly MORE evidence. A capability-absent
# collapse has no fan-out to point at, so its discriminator is the ABSENCE of withdrawn rows; this
# shape's discriminator is their PRESENCE, name for name. The converter withdraws every Go-side row
# beneath a signature-matched disclosed root and publishes the list, so the record itself ENUMERATES
# the lost verdicts -- and this rule requires that enumeration to BE the block's banked sub-verdicts
# exactly, in both directions, rather than merely to count to its size.
#
# What is NOT host-limited, and must stay red: any shortfall the committed manifest does not pin as
# `host-limit`; any shortfall whose withdrawn set is not exactly the block's banked sub-verdicts (a
# rogue loss cancelling against a rogue withdrawal is the arithmetic this refuses to be fooled by);
# any run whose converted side did not FAIL on the block root; a root that SKIPPED on the Go side (Go
# has no capability-absent skip branch here, and a skipping root cannot have fanned anything out);
# any surviving block subtest; any banked verdict outside the block going missing; any live verdict
# the row does not account for; and any disclosed movement other than the block root itself joining.
# The MISSING PIN is the load-bearing refusal -- this rule can only ever absorb what a reviewed,
# committed, self-retiring disclosure already describes, so it generalizes to no other shortfall and
# to no unpinned package. And note what the sweep never even reaches: a converted side that failed
# for some OTHER reason is a MISMATCH inside the converter's own oracle, so no "Validated N" line is
# printed and the row fails as FAIL, not COUNT. The signature pin does that work upstream of here.
$HostLimitDisclosureClass = 'host-limit'

# The Go-side root verdicts this rule admits, and the arms crypto/tls's manifest documents: `pass`
# (the primary pinned divergence -- Go clean, converted side over the wall) and `fail` (Go's own
# oracle red on a handful of the cases it fanned out). `skip` is absent ON PURPOSE, and so is an
# absent root: neither can coexist with the fan-out this rule demands.
$HostLimitGoRootVerdicts = @('pass', 'fail')

function Test-HostLimitDelta {
    param(
        [int] $Expected,           # banked matching-verdict count (roster column 2, the CEILING here)
        [int] $Disclosed,          # banked disclosed count (roster column 3)
        [PSCustomObject] $Block,   # @{ Test = <top-level name>; BlockSize = <int> }
        [int] $Got,
        $Comparison,
        [string[]] $BankedNames,
        $Pin                       # the COMMITTED manifest entry for $Block.Test: @{ Class; Signature }
    )

    function New-HostLimitResult([bool] $accepted, [string] $reason) {
        return [PSCustomObject]@{ Accepted = $accepted; Reason = $reason }
    }

    # Ordinal, for the same case-sensitivity reason ConvertFrom-ComparisonRecord states -- and a SET
    # rather than an array because this rule compares two three-thousand-name populations to one
    # another: the sibling's `-notcontains` scans are quadratic and would spend minutes here.
    function New-OrdinalSet([string[]] $names) {
        $set = New-Object 'System.Collections.Generic.HashSet[string]' ([System.StringComparer]::Ordinal)
        foreach ($name in $names) { [void]$set.Add($name) }
        return , $set
    }

    $shortfall = $Expected - $Got
    if ($shortfall -ne $Block.BlockSize) {
        return New-HostLimitResult $false "shortfall $shortfall does not match $($Block.Test)'s registered block size $($Block.BlockSize) -- a lost verdict outside the named block is never host-limited"
    }
    if ($null -eq $Comparison -or $null -eq $Comparison.go -or $null -eq $Comparison.csharp) {
        return New-HostLimitResult $false 'comparison record carries no per-test verdict maps'
    }

    # THE ADMISSION GATE. Everything below proves WHICH verdicts were lost; this proves the loss was
    # already disclosed, committed and reviewed. Without it the rule would be a general "accept a
    # block-sized shortfall", which is the change this design exists to not be.
    if ($null -eq $Pin) {
        return New-HostLimitResult $false "the committed disclosure manifest does not pin $($Block.Test) -- an undisclosed shortfall is a divergence, never a host state"
    }
    if ($Pin.Class -ne $HostLimitDisclosureClass) {
        return New-HostLimitResult $false "the committed manifest pins $($Block.Test) as '$($Pin.Class)', not '$HostLimitDisclosureClass' -- only that class names a deployment-shape ceiling this rule may absorb"
    }

    if ($BankedNames.Count -ne ($Expected + $Disclosed)) {
        return New-HostLimitResult $false "committed proof page lists $($BankedNames.Count) verdicts where the roster banks $Expected matched + $Disclosed disclosed -- page and table disagree"
    }

    $blockPrefix = "$($Block.Test)/"
    $blockNames = @($BankedNames | Where-Object { $_ -eq $Block.Test -or $_.StartsWith($blockPrefix) })
    if ($blockNames.Count -ne $Block.BlockSize) {
        return New-HostLimitResult $false "the committed proof page names $($blockNames.Count) verdicts under $($Block.Test), not the registered $($Block.BlockSize) -- re-derive the block size before trusting this row"
    }

    $blockSet = New-OrdinalSet $blockNames
    $goMap = $Comparison.go
    $csMap = $Comparison.csharp
    $liveNames = @($goMap.Keys)
    $liveSet = New-OrdinalSet $liveNames

    # Every banked name OUTSIDE the block must still be present live -- the mechanism absorbs the
    # named block, nothing else.
    $expectedOutside = @($BankedNames | Where-Object { -not $blockSet.Contains($_) })
    $missingOutside = @($expectedOutside | Where-Object { -not $liveSet.Contains($_) })
    if ($missingOutside.Count -gt 0) {
        return New-HostLimitResult $false "banked verdicts outside the block missing from this run: $($missingOutside -join ', ')"
    }

    # No block subtest may survive in the COMPARED set (each was withdrawn beneath the disclosed
    # root), and the root itself must be there to carry the disclosure.
    $liveBlockNames = @($liveNames | Where-Object { $_ -eq $Block.Test -or $_.StartsWith($blockPrefix) })
    if (@($liveBlockNames | Where-Object { $_ -ne $Block.Test }).Count -gt 0) {
        return New-HostLimitResult $false "subtests under $($Block.Test) survive in this run's compared set -- the block did not collapse, re-diagnose rather than absorb"
    }
    if ($liveBlockNames -notcontains $Block.Test) {
        return New-HostLimitResult $false "$($Block.Test) itself is missing from this run -- a host-limited block must still report its one collapsed root"
    }

    # The root's verdict pair. The CONVERTED side must be the half that failed -- that is the whole
    # claim of a host limit -- and the Go root must be one of the two arms the manifest documents.
    # Note this is NOT the discriminator against the capability-absent shape; the fan-out below is.
    $goVerdict = $goMap[$Block.Test]
    $csVerdict = if (-not $csMap.ContainsKey($Block.Test)) { 'absent' } else { $csMap[$Block.Test] }
    if ($csVerdict -ne 'fail') {
        return New-HostLimitResult $false "$($Block.Test): go '$goVerdict' vs C# '$csVerdict' -- a host-limited block's converted side FAILS on the pinned signature; anything else is not this shape"
    }
    if ($HostLimitGoRootVerdicts -notcontains $goVerdict) {
        return New-HostLimitResult $false "$($Block.Test): go '$goVerdict' -- a host-limited block's Go root is '$($HostLimitGoRootVerdicts -join "' or '")'; a skipped or absent root cannot have fanned out the matrix this rule requires"
    }

    # THE FAN-OUT, NAME FOR NAME -- the binding property AND the discriminator. The shortfall count
    # above says only "the right SIZE went missing"; this says the missing rows are the block's own
    # banked sub-verdicts and nothing else, in both directions. A rogue loss cancelling against a
    # rogue withdrawal is the exact arithmetic this refuses to be fooled by. It is also the evidence
    # the capability-absent rule cannot have -- there the matrix never existed, and that rule refuses
    # on ANY withdrawal under the block -- so requiring exactly the block's own partitions the two
    # rules cleanly whatever the roots report. Withdrawals OUTSIDE the block are left alone, as they
    # are there: one would move the shortfall and be caught by the first check.
    $withdrawn = @()
    if ($null -ne $Comparison.withdrawn) { $withdrawn = @($Comparison.withdrawn) }
    $withdrawnUnderBlock = @($withdrawn | Where-Object { $_ -eq $Block.Test -or $_.StartsWith($blockPrefix) })
    $withdrawnSet = New-OrdinalSet $withdrawnUnderBlock
    $expectedWithdrawn = @($blockNames | Where-Object { $_ -ne $Block.Test })
    $expectedWithdrawnSet = New-OrdinalSet $expectedWithdrawn

    $notWithdrawn = @($expectedWithdrawn | Where-Object { -not $withdrawnSet.Contains($_) })
    $unexpectedWithdrawn = @($withdrawnUnderBlock | Where-Object { -not $expectedWithdrawnSet.Contains($_) })
    if ($notWithdrawn.Count -gt 0 -or $unexpectedWithdrawn.Count -gt 0) {
        $detail = @()
        if ($notWithdrawn.Count -gt 0) { $detail += "$($notWithdrawn.Count) banked sub-verdict(s) were NOT withdrawn (first: $($notWithdrawn[0]))" }
        if ($unexpectedWithdrawn.Count -gt 0) { $detail += "$($unexpectedWithdrawn.Count) withdrawn name(s) the proof page does not bank (first: $($unexpectedWithdrawn[0]))" }
        return New-HostLimitResult $false ("the withdrawn Go-side rows are not exactly $($Block.Test)'s banked sub-verdicts -- " +
            ($detail -join '; ') + ' -- the shortfall must BE the block and nothing else')
    }

    # The disclosed accounting: the banked disclosures plus the block root, which the compare oracle
    # accounts as DISCLOSED (never as an agreed match) in this shape. Anything else is a disclosure
    # that moved for an unrelated reason, which is what this family of checks exists to catch.
    $liveDisclosedEntries = @()
    if ($null -ne $Comparison.disclosed) { $liveDisclosedEntries = @($Comparison.disclosed) }
    $expectedDisclosed = $Disclosed + 1
    if ($liveDisclosedEntries.Count -ne $expectedDisclosed) {
        return New-HostLimitResult $false "disclosed count moved ($($liveDisclosedEntries.Count) live vs $expectedDisclosed expected -- the $Disclosed banked plus $($Block.Test) itself) -- not a host-limited shape"
    }

    # A disclosure entry reads "<TestName> (<class>): <reason>", so the class the compare oracle
    # ACTUALLY applied to this run is readable here -- and is cross-checked against the committed pin
    # rather than assumed from it. Record and pin disagreeing means one of them is describing a
    # different divergence than the other.
    $rootEntry = @($liveDisclosedEntries | Where-Object { ($_ -split ' \(', 2)[0] -eq $Block.Test })
    if ($rootEntry.Count -eq 0) {
        return New-HostLimitResult $false "$($Block.Test) is not among this run's disclosures -- the extra disclosure is some other row, so this is not the collapse it looks like"
    }
    $liveClass = if ($rootEntry[0] -match ('^' + [regex]::Escape($Block.Test) + '\s\(([^)]+)\):')) { $Matches[1] } else { '' }
    if ($liveClass -ne $Pin.Class) {
        return New-HostLimitResult $false "$($Block.Test) is disclosed as '$liveClass' in this run but pinned as '$($Pin.Class)' in the committed manifest -- record and pin disagree"
    }

    # Nothing live may go unaccounted for: the outside names plus the one collapsed root, exactly.
    $accountedFor = New-OrdinalSet (@($expectedOutside) + @($Block.Test))
    $unaccounted = @($liveNames | Where-Object { -not $accountedFor.Contains($_) })
    if ($unaccounted.Count -gt 0) {
        return New-HostLimitResult $false "live verdicts this row does not account for: $($unaccounted -join ', ')"
    }

    return New-HostLimitResult $true $null
}

# ---- the ORACLE-ONLY failure (the three-run standard, applied to the ORACLE side) -----------------
# Every rule above answers the question "this row validated, but at the WRONG COUNT -- may the
# difference be absorbed?". This one answers a question none of them can reach, because it happens
# one step earlier: the row did not validate AT ALL, and the reason is that GO'S OWN TEST BINARY
# failed cases the converted side passed.
#
# MEASURED (2026-09-02, crypto/tls run 3): the row COMPLETED -- 3,644 Go rows against 3,644 C# rows,
# neither side truncated -- and the entire failure set was seven `TestBogoSuite/*` cases plus their
# parent, every one of them Go=fail / C#=pass, with ZERO Go=pass / C#=fail entries anywhere. The
# converted side was clean; the oracle flaked. The sweep reported `FAIL crypto/tls`, which is a
# green corpus failing a gate on the reference implementation's own flake.
#
# THE RULING (coordinator, 2026-09-02): the three-run flake standard -- fail-WITH, pass-CLEAN, pass
# again -- is not a property of the converted side. It is a property of MEASUREMENT, and it applies
# to whichever side moved. So a row whose failure set is oracle-only is RE-RUN ONCE before it fails;
# a clean re-run banks and says loudly that the oracle flaked once; a second oracle-only result is
# `oracle unstable on this host` -- its own verdict word, counted apart, still a non-zero exit,
# because two unreproducible oracle reds are a host to fix, never a green gate. A row with ANY
# converted-side failure is untouched: FAIL, first time, no re-run.
#
# WHAT THIS IS NOT. It is not a fourth roster absorption arm -- the roster grows no annotation, no
# count moves, and Get-SweepRowClassification is not extended. A re-run either produces this row's
# banked count or it does not; this rule decides only whether the sweep is entitled to ASK twice.
#
# The refusals below are the whole safety argument, and each one closes a shape that would otherwise
# read like an oracle flake:
#
#   status not `failing`   a conversion-blocked / infrastructure-blocked / not-applicable record
#                          describes a run that never compared anything
#   a GATED record         a `-test-filter` run answers for its filter's survivors and rewrites the
#                          SAME file a full run writes (testConversion.go's TestFilter field exists
#                          for exactly this) -- never this row's full evidence
#   entry counts unequal   a truncated side is a dead run, and its missing rows would read as
#                          one-sided divergences whose C# half is absent, not passing
#   a deadline or crash    the results-file TAIL states a package deadline kill or a crash outright
#     signature            (CLAUDE.md's "read the tail FIRST" rule); a killed run is never a flake
#   any converted-side     one Go=pass / C#=fail entry and the failure set is this corpus's own,
#     divergence           whatever else is in it
#   an unreadable error    a census gap, a stale manifest, a blocked capability or the zero-match
#     entry                guard is an infrastructure statement, not a divergence -- refused BY NAME
#                          rather than skipped, so a new error shape can never be waved through
#
# The two tolerated non-divergence entries are the EXIT-STATUS lines (`go test: ...` and
# `converted tests: ...`), and tolerating them is not a hole: an exit code here is a CONSEQUENCE of
# the divergence set, not independent evidence. Go's binary exits non-zero because the cases it
# flaked failed; the converted host exits non-zero because crypto/tls carries agreed fail/fail rows
# (its fixtures expired 2025-01-01) which block the compare oracle's own exit-code forgiveness the
# moment any mismatch exists. Every way those exits could mean something WORSE is caught
# independently and above: a build or publish death leaves zero verdict rows (the count check), a
# deadline kill states itself in the tail (the signature scan), and a mid-run crash leaves one-sided
# rows (the count check, then the C#-half check). Refusing the lines themselves would make this rule
# unfireable for the only case it was written from.

# The mismatch line matchTerminalStatuses writes: `fmt.Sprintf("%s: Go=%q C#=%q", ...)`, optionally
# followed by a parenthetical when a pinned disclosure signature did not match. Anchored at the
# start and non-greedy on the name so the SHORTEST name wins; the trailing parenthetical is ignored
# here because the go/cs pair it qualifies is refused by the fail/pass test anyway.
$OracleOnlyDivergencePattern = '^(?<name>.+?): Go="(?<go>[^"]*)" C#="(?<cs>[^"]*)"'

# The two error entries that are a CONSEQUENCE of the divergence set rather than evidence of their
# own -- see the paragraph above. Ordinal prefix match; anything else is refused by name.
$OracleOnlyToleratedErrorPrefixes = @('go test: ', 'converted tests: ')

# A run that was KILLED or CRASHED, in the words the artifacts actually use. Scanned over the
# results-file tail AND over the comparison record's own error text, because the same event reaches
# the two places in two spellings: plain in the host's own stream, BACKSLASH-ESCAPED when it is
# carried inside another JSON string. `\\?` before each quote admits both -- a substring count of
# the plain form alone returned 0 on a record whose tail states the kill (CLAUDE.md, 2026-09-02).
#
# Every entry here fails SAFE: a false match refuses the re-run and the row reports FAIL exactly as
# it does today, so a signature that is occasionally present in a test's captured output costs
# nothing but the arm not firing.
$OracleOnlyKillSignatures = @(
    '\\?"action\\?"\s*:\s*\\?"timeout\\?"'                  # the package-deadline event, both spellings
    'package timeout after'                                 # that event's own output text
    '\\?"action\\?"\s*:\s*\\?"infrastructure-error\\?"'     # the host's non-verdict terminal action
    '0xc0000142'                                            # STATUS_DLL_INIT_FAILED -- a torn publish tree
    'Unhandled exception'                                   # the CLR's crash banner
    'NotImplementedException'                               # the measured module-init death
    'Test Run Aborted'                                      # an aborted host run
    'fatal error: '                                         # Go's own runtime crash banner
)

<#
.SYNOPSIS
    Decides whether a failed row's failure set is ORACLE-ONLY -- every divergence Go=fail with
    C#=pass, on a run that otherwise completed. Pure -- no I/O, no state.
.OUTPUTS
    OracleOnly (bool), Flaked (the divergent test names, for the ledger note), Reason (why not).
#>
function Test-OracleOnlyFailure {
    param(
        $Comparison,            # go2cs_test_comparison.json via ConvertFrom-ComparisonRecord
        # The run's OWN go2cs_test_results.json tail, or $null when there was none to read.
        # DELIBERATELY UNTYPED. A `[string]` parameter COERCES $null to the empty string, so the
        # "no tail" refusal below could never fire through it -- an unreadable tail would have read
        # as a clean one and the re-run would have fired on a run whose deadline nobody checked.
        # Caught by this rule's own fixtures on the first run (sweep-oracle-rerun-selftest.ps1),
        # which is the whole argument for exercising a rule with the shapes its CALLER passes.
        $TailText
    )

    # Local to this function -- nested definitions do not leak into the script scope.
    function New-OracleOnlyResult([bool] $oracleOnly, [string[]] $flaked, [string] $reason) {
        return [PSCustomObject]@{ OracleOnly = $oracleOnly; Flaked = $flaked; Reason = $reason }
    }

    if ($null -eq $Comparison -or $null -eq $Comparison.go -or $null -eq $Comparison.csharp) {
        return New-OracleOnlyResult $false @() 'comparison record carries no per-test verdict maps'
    }

    if ($Comparison.status -ne 'failing') {
        return New-OracleOnlyResult $false @() ("comparison status is '$($Comparison.status)', not 'failing' -- " +
            'only a run that COMPLETED and diverged can be an oracle flake')
    }

    if (-not [string]::IsNullOrWhiteSpace($Comparison.testFilter)) {
        return New-OracleOnlyResult $false @() ("the record was produced under -test-filter '$($Comparison.testFilter)' -- " +
            'a gated record answers for its filter, never for the row')
    }

    $goCount = $Comparison.go.Count
    $csCount = $Comparison.csharp.Count

    if ($goCount -eq 0 -or $csCount -eq 0) {
        return New-OracleOnlyResult $false @() ("the run produced $goCount Go and $csCount C# verdict row(s) -- " +
            'a side with no verdicts is a run that did not happen, never a flake')
    }
    if ($goCount -ne $csCount) {
        return New-OracleOnlyResult $false @() ("entry counts incomplete: $goCount Go row(s) against $csCount C# row(s) -- " +
            'a truncated run, not an oracle flake')
    }

    # EMPTY as well as absent: a results file that exists and holds nothing is a host that wrote no
    # stream, which is a dead run and not a tail this can read either way.
    if ([string]::IsNullOrEmpty($TailText)) {
        return New-OracleOnlyResult $false @() ('no readable results-file tail -- the deadline/crash question ' +
            'cannot be answered, so it is answered NO')
    }

    $errorEntries = @()
    if ($null -ne $Comparison.errors) { $errorEntries = @($Comparison.errors | ForEach-Object { [string]$_ }) }

    # The tail AND the record's own error text, one scan: the escaped spelling is exactly what an
    # event embedded in the record's JSON looks like.
    $scanned = @($TailText) + $errorEntries
    foreach ($signature in $OracleOnlyKillSignatures) {
        foreach ($text in $scanned) {
            if ($text -match $signature) {
                return New-OracleOnlyResult $false @() ("a deadline/crash signature is present (matched '$signature') -- " +
                    'a killed or crashed run is never an oracle flake')
            }
        }
    }

    if ($errorEntries.Count -eq 0) {
        return New-OracleOnlyResult $false @() 'the record lists no divergence at all -- there is nothing to attribute to the oracle'
    }

    $flaked = New-Object System.Collections.Generic.List[string]

    foreach ($entry in $errorEntries) {
        if ($entry -match $OracleOnlyDivergencePattern) {
            $name = $Matches['name']
            $goVerdict = $Matches['go']
            $csVerdict = $Matches['cs']

            if ($goVerdict -ne 'fail' -or $csVerdict -ne 'pass') {
                return New-OracleOnlyResult $false @() ("${name}: Go='$goVerdict' C#='$csVerdict' -- " +
                    "an oracle flake is Go='fail' with C#='pass'; anything else is this corpus's own divergence")
            }

            [void]$flaked.Add($name)
            continue
        }

        $tolerated = $false
        foreach ($prefix in $OracleOnlyToleratedErrorPrefixes) {
            if ($entry.StartsWith($prefix, [System.StringComparison]::Ordinal)) { $tolerated = $true; break }
        }

        if (-not $tolerated) {
            return New-OracleOnlyResult $false @() "the record carries a non-divergence error this rule does not read: $entry"
        }
    }

    if ($flaked.Count -eq 0) {
        return New-OracleOnlyResult $false @() ('the record carries exit-status lines but no per-test divergence -- ' +
            'nothing to attribute to the oracle')
    }

    return New-OracleOnlyResult $true $flaked.ToArray() $null
}

<#
.SYNOPSIS
    The TAIL of the converted host's own results file, as text. $null when there is no file.
.DESCRIPTION
    The artifact CLAUDE.md's "read the results-file TAIL FIRST" rule names: TestHost.WriteResults
    appends its package-level event (the deadline kill among them) LAST and serializes the whole
    stream, so the answer to "was this run killed?" is always in the final bytes.

    The LAST $Bytes rather than the whole file, because a 3,600-verdict package's results file is
    megabytes of event log and this is called on a gate's hot path. FileShare ReadWrite so another
    handle cannot turn the read into an exception the caller would have to guess about; a tail that
    cuts a UTF-8 sequence mid-character costs one replacement glyph and no signature.
#>
function Get-ResultsTailText {
    param(
        [Parameter(Mandatory)][string] $Path,
        [int] $Bytes = 65536
    )

    if (-not (Test-Path -LiteralPath $Path)) { return $null }

    $stream = [System.IO.File]::Open($Path, [System.IO.FileMode]::Open, [System.IO.FileAccess]::Read, [System.IO.FileShare]::ReadWrite)
    try {
        $take = [int][Math]::Min([long]$Bytes, $stream.Length)
        if ($take -le 0) { return '' }

        [void]$stream.Seek(-[long]$take, [System.IO.SeekOrigin]::End)
        $buffer = New-Object byte[] $take
        $read = $stream.Read($buffer, 0, $take)

        return [System.Text.Encoding]::UTF8.GetString($buffer, 0, $read)
    }
    finally { $stream.Dispose() }
}

# ---- comparison-record reader --------------------------------------------------------------------
# Reads go2cs_test_comparison.json into the shape the two delta rules above consume: `go`/`csharp`
# as CASE-SENSITIVE (ordinal) dictionaries, `withdrawn`/`disclosed` as arrays or $null, plus the
# three scalar/array members the oracle-only rule reads: `status`, `errors` and `testFilter`.
#
# ⚠ Why not ConvertFrom-Json (measured 2026-08-29, G's net/http pre-staging): a Go test suite can
# legitimately hold verdict names differing ONLY by case -- net/http's
# TestTransportContentEncodingCaseInsensitive spawns .../GZIP and .../gzip pairs, which is exactly
# what a test with that name would do. Windows PowerShell 5.1's JSON->PSObject path folds member
# names case-insensitively and THROWS on such input ("contains the duplicated keys"), and a
# PSObject cannot hold the pair at all -- so both the old parser and the old property-bag shape are
# structurally unable to carry a legal record. The converter's own maps are Go map[string]string,
# case-sensitive by construction (all eleven case-folding sites in testConversion.go are on paths
# and env-var names, none on a test name), so the record is sound; only the PowerShell reader was
# not. JavaScriptSerializer deserializes into case-sensitive dictionaries; the explicit ordinal
# re-copy below makes that deliberate rather than inherited, and a true duplicate key (the same
# exact name twice) still throws loudly on Add.
function ConvertFrom-ComparisonRecord {
    param([string] $Path)

    $text = [System.IO.File]::ReadAllText($Path)

    # Two readers, one shape, because neither type exists on both editions: System.Web.Extensions is
    # .NET Framework only (it cannot load under PowerShell 7 -- every Linux host, and any Windows host
    # driving these instruments with pwsh rather than 5.1), and System.Text.Json is not present on 5.1.
    # Both must yield ORDINAL dictionaries for the case-sensitivity reason above. `ConvertFrom-Json
    # -AsHashtable` is deliberately NOT used on Core: it happens to preserve case-only pairs on 7.5,
    # but that is inherited behaviour rather than a stated contract, and the whole point of the
    # explicit re-copy below is that the ordinal choice is deliberate.
    $goMap = $null
    $csMap = $null
    $withdrawn = $null
    $disclosed = $null
    $status = $null
    $recordErrors = $null
    $testFilter = $null

    if ($PSVersionTable.PSEdition -eq 'Desktop') {
        if (-not $script:comparisonSerializer) {
            Add-Type -AssemblyName System.Web.Extensions
            $script:comparisonSerializer = New-Object System.Web.Script.Serialization.JavaScriptSerializer
            $script:comparisonSerializer.MaxJsonLength = [int]::MaxValue
        }

        $raw = $script:comparisonSerializer.DeserializeObject($text)
        if ($null -eq $raw) { throw "comparison record at $Path deserialized to nothing" }

        $toOrdinalMap = {
            param($member)
            if (-not $raw.ContainsKey($member)) { return $null }
            $map = New-Object 'System.Collections.Generic.Dictionary[string,string]' ([System.StringComparer]::Ordinal)
            foreach ($entry in $raw[$member].GetEnumerator()) { $map.Add([string]$entry.Key, [string]$entry.Value) }
            return , $map
        }

        $goMap = & $toOrdinalMap 'go'
        $csMap = & $toOrdinalMap 'csharp'
        $withdrawn = if ($raw.ContainsKey('withdrawn')) { @($raw['withdrawn']) } else { $null }
        $disclosed = if ($raw.ContainsKey('disclosed')) { @($raw['disclosed']) } else { $null }
        $status = if ($raw.ContainsKey('status')) { [string]$raw['status'] } else { $null }
        $recordErrors = if ($raw.ContainsKey('errors')) { @($raw['errors'] | ForEach-Object { [string]$_ }) } else { $null }
        $testFilter = if ($raw.ContainsKey('testFilter')) { [string]$raw['testFilter'] } else { $null }
    }
    else {
        $document = $null
        try {
            $document = [System.Text.Json.JsonDocument]::Parse($text)
            if ($null -eq $document) { throw "comparison record at $Path deserialized to nothing" }
            $root = $document.RootElement

            $toOrdinalMap = {
                param($member)
                $element = [System.Text.Json.JsonElement]::new()
                if (-not $root.TryGetProperty($member, [ref] $element)) { return $null }
                $map = New-Object 'System.Collections.Generic.Dictionary[string,string]' ([System.StringComparer]::Ordinal)
                foreach ($property in $element.EnumerateObject()) { $map.Add($property.Name, $property.Value.GetString()) }
                return , $map
            }

            $toArray = {
                param($member)
                $element = [System.Text.Json.JsonElement]::new()
                if (-not $root.TryGetProperty($member, [ref] $element)) { return $null }
                return , @($element.EnumerateArray() | ForEach-Object { $_.GetString() })
            }

            # The scalar sibling of $toArray, for the two string members. GetString() on an absent
            # property is not reachable -- TryGetProperty gates it exactly as it gates the two above.
            $toText = {
                param($member)
                $element = [System.Text.Json.JsonElement]::new()
                if (-not $root.TryGetProperty($member, [ref] $element)) { return $null }
                return $element.GetString()
            }

            $goMap = & $toOrdinalMap 'go'
            $csMap = & $toOrdinalMap 'csharp'
            $withdrawn = & $toArray 'withdrawn'
            $disclosed = & $toArray 'disclosed'
            $status = & $toText 'status'
            $recordErrors = & $toArray 'errors'
            $testFilter = & $toText 'testFilter'
        }
        finally {
            if ($null -ne $document) { $document.Dispose() }
        }
    }

    return [PSCustomObject]@{
        go         = $goMap
        csharp     = $csMap
        withdrawn  = $withdrawn
        disclosed  = $disclosed
        status     = $status
        errors     = $recordErrors
        testFilter = $testFilter
    }
}
