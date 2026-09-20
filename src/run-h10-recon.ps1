# run-h10-recon.ps1 - the H10 RECON LEG: run one worker's rows through the per-package pipeline and
# bank the per-row TSV that shardmap.py --timings reads.
#
# Ruled at COORD c7f68b53e / the pipeline post: the recon leg runs `go2cs -tests -test-action all` per
# row, NOT run-validated-sweep.ps1. The reason is enumeration, not preference: the sweep selects among
# BANKED rows (`-Filter ROW -Exact` throws "No banked packages matched" for anything else), so it cannot
# reach the twelve successors or the unbanked rows at all. A basis measured on two instruments is not a
# basis.
#
# ---------------------------------------------------------------------------------------------------
# WHAT THIS WRITES, AND WHY EVERY COLUMN IS WHERE IT IS
#
# ⚠ CITED BY MESSAGE TEXT, NOT BY LINE NUMBER. An earlier cut named four line numbers in
# shardmap.py; C2's own commit then inserted thirty lines into that file and TWO of the four went
# stale, pointing at unrelated code. A cross-file line citation goes stale when EITHER file moves and
# NO test in either repository can see it -- the wrapper's census does not parse Python and the
# generator's tests do not read this file. The refusal's own text cannot drift without the refusal
# itself changing, which is the property a citation needs. (C2's finding and C2's remedy.)
#
# shardmap.py reads FOUR columns BY NAME -- `need = ("row", "word", "verdicts", "sweep_s")` -- and
# refuses a header lacking any of them:
#     row · word · verdicts · sweep_s
# It also refuses the file outright on ANY CR byte ("carries ... CR byte(s) -- the banked TSVs are
# LF"), refuses a non-integer sweep_s by name ("a row with no measured cost is UNSCHEDULED, never
# nominal"), and refuses the whole basis if the hand-stopped drop never fired ("none of the
# hand-stopped rows ... appear in") -- so `net` MUST be measured and emitted even though its
# cost is discarded. The extras below are additive; the parser reads by name and ignores them.
#
# One extra is NOT decoration: `wall_s` is the observed integer wall for EVERY row whatever its
# word. `sweep_s` answers "may this row be scheduled on this number" and is deliberately
# non-integer when the row earned no cost; `wall_s` answers "how long did it take", which stays
# a fact for a row that timed out. The concatenation banks the hand-stopped row `net` with
# `sweep_s := wall_s`, and `net` is EXPECTED to TIMEOUT -- so without this column there is nothing
# to substitute from and the basis is refused either way.
#
# `post_s` is the second extra, and it exists because `sweep_s` CLOSES TOO EARLY to see the cost that
# matters most to a schedule: the seconds this script spends after the converter returns, reading and
# comparing the row's artifacts. Measured 2026-09-20: 540 s on `go/doc/comment` against a 23 s
# conversion here, and ~52:1 on `crypto/cipher` on G's box. Without this column a reader can only
# subtract two numbers that do not span the gap, and a leg budgeted from `sweep_s` is out by more than
# an order of magnitude. shardmap.py reads FOUR columns by name and ignores every other, which
# is why `wall_s` and `post_s` can be carried without touching the basis.
#
# ---------------------------------------------------------------------------------------------------
# THE ONE-ATTEMPT CLOCK
#
# sweep_s is this script's clock around the ONE CONVERTER INVOCATION for the row, and it CLOSES THE
# MOMENT THAT INVOCATION RETURNS -- before this script reads a single artifact. It is therefore the
# CONVERTER's cost, not the ROW's: this script's own post-processing lies entirely outside it.
#
# An earlier wording of this paragraph said "the ONE pipeline invocation for the row", which reads as
# though sweep_s covered the row end to end. It does not, and the difference is not a rounding error:
# `go/doc/comment` reported sweep_s=23 and then spent 540 s in this script, and G measured ~59 minutes
# of wrapper phase against a 66 s conversion. `post_s` is the column that closes that gap.
#
# What the single attempt DOES buy is unchanged: the recon leg makes a
# single attempt per row, so the oracle re-run inflation measured in run-validated-sweep.ps1 (it resets
# $rowStarted at :1105 and re-takes $rowSecs at :1107, so an EXTERNAL clock there spans both attempts)
# cannot arise here by construction. A row whose Go oracle is unstable is a READING -- its word says so
# -- not a re-run.
#
# ---------------------------------------------------------------------------------------------------
# TWO FORMAT STRINGS, NOT ONE  (C2's line, verified here at the source)
#
# ⚠ NAMED BY BRANCH RATHER THAN BY LINE, for the reason in the header block above: an earlier
# cut cited these at `testConversion.go:8271` and `:8274`, and at the version tip those two lines
# are now `agreedFailure := false` and `agreedFailure = true`. THREE SEATS ARE LANDING IN THAT FILE,
# so its line numbers are the least stable citation in this campaign; the format strings themselves
# are what this wrapper actually matches on and they cannot drift without the match breaking.
#
# The converter prints the summary from TWO sites, chosen by whether the row carries disclosures:
#   the DISCLOSED branch     "...(%d skipped..., %d disclosed-divergent (%s), %d ...excluded).\n"
#   the no-disclosure branch "...(%d skipped..., %d disclosed-unsupported declarations excluded).\n"
# A wrapper asserting ONE literal refuses every row of the other kind. The assertion below is on the
# COMMON PREFIX and the captured integer; the tail is deliberately variable.
#
# ---------------------------------------------------------------------------------------------------
# ENVIRONMENT, FOR A WORKER RUNNING THIS UNDER POWERSHELL CORE
#
# A `pwsh` installed as a DOTNET TOOL carries a net8 apphost and DIES AT LAUNCH if DOTNET_ROOT already
# points at the .NET 10 root (measured on the gate box). So DOTNET_ROOT is set INSIDE the child pwsh
# and never in the environment that launches it. This script does not set it for you: it is the
# caller's, because only the caller knows which shell it is starting.
#
# The pins this script asserts are GOROOT, GOTOOLCHAIN=local, CGO_ENABLED=0 and PATH -- see the block
# above the preflight for why PATH is one of them.
[CmdletBinding()]
param(
    # One package path per line; blank lines and #-comments ignored. This worker's rows, hand-listed.
    [Parameter(Mandatory)][string] $NameList,

    # The worktree at the version tip. Its src/core is the SEED for every row -- C2 measured that a
    # BARE output root fails the hand-own gate (sync rc 1 bare, rc 0 seeded), so the root is never a
    # scratch directory.
    [Parameter(Mandatory)][string] $Tree,

    # The pinned GOROOT. Asserted BY OUTPUT below, never by this string.
    [Parameter(Mandatory)][string] $GoRoot,

    # The TSV to write: a FILE PATH, never a directory. The write is this script's LAST
    # statement ([System.IO.File]::WriteAllText below), so a directory throws AFTER every row
    # has run and the whole leg is lost at its final line. Its parent must already exist.
    [Parameter(Mandatory)][string] $Out,

    # The commit -Tree must be detached at. Required: the leg's readings are only comparable if every
    # worker ran the same tree, and "the tip" is not a thing a script may assume.
    [Parameter(Mandatory)][string] $ExpectTip,

    # Where the per-row evidence and the TSV land. MUST be outside any git work tree -- it is the only
    # thing that survives the throwaway worktree being removed.
    [Parameter(Mandatory)][string] $Scratch,

    [string] $TestConfig   = 'Release',
    [string] $TestTimeout  = '30m',
    [string] $Platform     = '',

    # Run ONE row, print the matched summary line beside the emitted TSV row, write nothing. The
    # acceptance vehicle: decidable without a 228-row leg.
    [switch] $DryRun,

    # Exercise every guard and the format assertion, run no row, write nothing.
    [switch] $SelfTest
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

function Deny([string] $m) { Write-Host ''; Write-Host "RECON REFUSED: $m" -ForegroundColor Red; exit 2 }

# ⚠ NATIVE STDERR IS A TERMINATING ERROR IN 5.1 UNDER ErrorActionPreference=Stop, AND THE GOOD PATH IS
# THE ONE THAT WRITES IT. `git rev-parse --is-inside-work-tree` on a directory OUTSIDE a repository --
# which is exactly what the scratch must be -- prints "fatal: not a git repository" and PowerShell
# turns that into a NativeCommandError that kills the script. Measured: the guard refused its own
# success case. So every native git call goes through here, where stderr is discarded and the VERDICT
# is the exit code, read immediately.
function GitTry([string[]] $gitArgs) {
    $old = $ErrorActionPreference
    $ErrorActionPreference = 'Continue'
    try {
        $o = & git @gitArgs 2>$null
        $c = $LASTEXITCODE
        return [pscustomobject]@{ Out = (($o | Out-String).Trim()); Code = $c }
    } finally { $ErrorActionPreference = $old }
}

# ⚠ THE SAME FAULT GitTry EXISTS FOR, AT THE CALLS WHOSE TEXT IS THE MEASUREMENT.
# GitTry can discard stderr because its verdict is the EXIT CODE. The preflight's `go version` calls
# cannot: their OUTPUT is what the pin is asserted on, and the message the pin exists to catch --
# "go: ..\go.mod requires go >= 1.24 (running go 1.23.1; GOTOOLCHAIN=local)" -- arrives on stderr.
# Under Stop that message KILLS the preflight four lines before the Deny that would have named it,
# which is exactly what :192's comment warns about: a guard that dies is not a guard that refused.
# (C2 1e2adb3d, finding B. My own commit's comment stated the rule and left two call sites under it.)
function NativeFirstLine([scriptblock] $sb) {
    $old = $ErrorActionPreference
    $ErrorActionPreference = 'Continue'
    try {
        $o = & $sb 2>&1
        $ls = @($o | ForEach-Object { [string] $_ })
        if ($ls.Count -eq 0) { return '' }
        return $ls[0]
    } finally { $ErrorActionPreference = $old }
}

# For the calls whose verdict IS the exit code and whose stderr is noise -- GitTry's shape, generalised.
function NativeQuiet([scriptblock] $sb) {
    $old = $ErrorActionPreference
    $ErrorActionPreference = 'Continue'
    try {
        $o = & $sb 2>$null
        return (($o | Out-String).Trim())
    } finally { $ErrorActionPreference = $old }
}

# ---------------------------------------------------------------- the summary-line contract
# ONE definition, consulted by the assertion AND by the per-row parse, so the check cannot drift from
# the thing it checks (R's barmatch() shape; C1 paid for the replica lesson at a7c20e7cb).
$SummaryRe = '^Validated (\d+) tests against go test \('

function Get-VerdictCount([string[]] $lines) {
    $hits = @($lines | Where-Object { $_ -match $SummaryRe })
    # EXACTLY ONE. Zero is a row that produced no summary; more than one means the emission changed
    # shape and a first-match read would silently pick one.
    if ($hits.Count -ne 1) { return [pscustomobject]@{ Count = $null; Line = $null; Hits = $hits.Count } }
    $null = $hits[0] -match $SummaryRe
    return [pscustomobject]@{ Count = [int] $Matches[1]; Line = $hits[0]; Hits = 1 }
}

# ---------------------------------------------------------------- the format assertion (ruled)
# "the wrapper asserts the pipeline's summary-line format it parses (a planted line that fails to parse
# must refuse, not read 0)". Both real shapes must PARSE; a planted near-miss must NOT.
function Test-SummaryContract {
    $withDisclosures    = 'Validated 41 tests against go test (3 skipped identically on both sides, 2 disclosed-divergent (alloc-profile), 1 disclosed-unsupported declarations excluded).'
    $withoutDisclosures = 'Validated 302 tests against go test (7 skipped identically on both sides, 0 disclosed-unsupported declarations excluded).'
    $planted            = 'Validated some tests against go test (malformed).'
    $alsoPlanted        = 'Verified 41 tests against go test (3 skipped).'

    $a = Get-VerdictCount @($withDisclosures)
    $b = Get-VerdictCount @($withoutDisclosures)
    $c = Get-VerdictCount @($planted)
    $d = Get-VerdictCount @($alsoPlanted)
    $e = Get-VerdictCount @($withDisclosures, $withoutDisclosures)   # two lines: must refuse, not pick

    $ok = ($a.Count -eq 41) -and ($b.Count -eq 302) -and ($null -eq $c.Count) -and ($null -eq $d.Count) -and ($null -eq $e.Count)

    Write-Host '  summary-line contract:'
    Write-Host ("    with disclosures      -> {0,-6}  (want 41)"  -f $(if ($null -eq $a.Count) { 'REFUSE' } else { $a.Count }))
    Write-Host ("    without disclosures   -> {0,-6}  (want 302)" -f $(if ($null -eq $b.Count) { 'REFUSE' } else { $b.Count }))
    Write-Host ("    planted non-numeric   -> {0,-6}  (want REFUSE)" -f $(if ($null -eq $c.Count) { 'REFUSE' } else { $c.Count }))
    Write-Host ("    planted wrong verb    -> {0,-6}  (want REFUSE)" -f $(if ($null -eq $d.Count) { 'REFUSE' } else { $d.Count }))
    Write-Host ("    TWO matching lines    -> {0,-6}  (want REFUSE)" -f $(if ($null -eq $e.Count) { 'REFUSE' } else { $e.Count }))
    return $ok
}

# ---------------------------------------------------------------- preflight
Write-Host ''
Write-Host 'H10 recon leg -- preflight'

# ⚠⚠ -SelfTest COVERS THE SUMMARY-LINE CONTRACT ONLY, AND ITS OWN COMMENT ONCE CLAIMED MORE.
# It exits HERE, before `Assert-OrdinalJsonReader` runs in the preflight below, so the guard that
# REFUSES THE LEG is untested by the one flag that exists to test the guards.
#
# ⚠ I TRIED THE OBVIOUS FIX AND IT DOES NOT WORK: calling the canary from this block fails with
# CommandNotFoundException, because the function is defined ~130 lines BELOW here and a script's
# top-level flow cannot forward-reference it. The parse is clean either way, so only RUNNING
# `-SelfTest` catches it -- which is how this note came to be written rather than a broken switch
# shipped.
#
# THE REAL FIX IS A PREFLIGHT RESTRUCTURE, deliberately not in this commit: the block must move
# BELOW the JSON reader definitions, AND the eight tree/scratch validations between here and there
# must stop denying on the dummy values `-SelfTest` is forced to supply -- see the next paragraph.
#
# ⚠ BECAUSE IT CANNOT BE INVOKED AS DOCUMENTED EITHER. Six parameters are [Parameter(Mandatory)]
# with no parameter set of their own, so `-SelfTest` alone prompts for all six and under
# -NonInteractive is rc 1 with nothing run; it is reachable only by supplying six values it never
# uses, and those values then fail the tree validation below. The two defects are one restructure.
#
# THE CANARY IS NOT UNTESTED MEANWHILE: it runs in the preflight of every real leg, and its
# red-first arm drives it against a deliberately folding reader and requires the refusal.
if ($SelfTest) {
    Write-Host ''
    if (Test-SummaryContract) { Write-Host '  SELF-TEST PASSED -- the summary-line contract only; see the note above'; exit 0 }
    Deny 'the summary-line contract FAILED its self-test -- the parse and its controls disagree'
}

if (-not (Test-Path -LiteralPath $NameList)) { Deny "no name list at '$NameList'" }
if (-not (Test-Path -LiteralPath $Tree))     { Deny "no tree at '$Tree'" }
if (-not (Test-Path -LiteralPath (Join-Path $Tree 'src/core'))) { Deny "'$Tree' has no src/core -- the output root must be the worktree, whose src/core is the seed" }

# ---------------------------------------------------------------- the throwaway-worktree guard (ruled)
# `-test-action all` PUBLISHES the proof page, the index row and the README badge -- measured on the
# one-row dry run, where a SINGLE row re-banked bufio's proof page to 1.24.13 and removed 25 lines from
# the shared index. Those are the ROW ACT's artifacts; the recon leg produces READINGS. So the leg runs
# in a LINKED WORKTREE that is discarded whole afterwards, and this script refuses to run anywhere else.
#
# ⚠ The wrapper NEVER repairs the tree itself: no `git checkout --`, no `git clean`. A tool that cleans
# up after itself inside a work tree is one bad path away from discarding someone's work, and the
# discard is the caller's act on a tree it created for this.
$gd  = GitTry @('-C', $Tree, 'rev-parse', '--git-dir')
$gcd = GitTry @('-C', $Tree, 'rev-parse', '--git-common-dir')
if ($gd.Code -ne 0 -or $gcd.Code -ne 0) { Deny "'$Tree' is not a git work tree" }
# A LINKED worktree's --git-dir is <common>/worktrees/<name>; the MAIN checkout's two are equal.
if ($gd.Out -eq $gcd.Out) {
    Deny "'$Tree' is a MAIN checkout (--git-dir == --git-common-dir), not a linked worktree. The leg publishes validation artifacts and its tree is discarded afterwards -- run it in a linked worktree created for this list."
}
$sym = GitTry @('-C', $Tree, 'symbolic-ref', '-q', 'HEAD')
if ($sym.Code -eq 0 -and $sym.Out) { Deny "'$Tree' HEAD is on branch '$($sym.Out)' -- a recon tree is DETACHED, so nothing can be committed from it by habit" }
$head = GitTry @('-C', $Tree, 'rev-parse', 'HEAD')
if ($head.Out -ne $ExpectTip) { Deny "'$Tree' is at $($head.Out), not the expected tip $ExpectTip -- the leg's readings are comparable only across one tree" }

# The scratch must OUTLIVE the tree, so it must not be inside one.
# ⚠ The SUCCESS case here is git FAILING (not a repository). Read the code, never the stream.
if (-not (Test-Path -LiteralPath $Scratch)) { New-Item -ItemType Directory -Path $Scratch -Force | Out-Null }
$sc = GitTry @('-C', $Scratch, 'rev-parse', '--is-inside-work-tree')
if ($sc.Code -eq 0 -and $sc.Out -eq 'true') { Deny "the scratch '$Scratch' is inside a git work tree -- it holds the only copy of this leg's evidence and must survive the tree's removal" }

# The pin BY OUTPUT, never by the string handed in. GOTOOLCHAIN=local is load-bearing on this fleet:
# R measured, and i9 reproduced, that a 1.24.13 SDK's own go.exe answers the MACHINE pin without it.
$env:GOROOT       = $GoRoot
$env:GOTOOLCHAIN  = 'local'
$env:CGO_ENABLED  = '0'

# ⚠ THE SHAPE CHECKS COME FIRST, AND EACH REFUSES BY NAME. Without them an absent GOROOT throws out of
# the `&` call under ErrorActionPreference=Stop and the run dies rc 1 with NO refusal line -- measured,
# and it made the pin control look like it had fired when it had only crashed. A guard that dies is not
# a guard that refused.
# ⚠ THE EXECUTABLE SUFFIX IS NOT ALWAYS `.exe`, AND HARDCODING IT MISNAMES THE REFUSAL. The first cut
# joined 'bin/go.exe': on a non-Windows worker that path does not exist while 'bin/go' does, so the
# preflight refused a perfectly valid GOROOT with "not a GOROOT" -- a refusal naming a cause that is
# not the cause, which is worse than no refusal because it sends the reader to the wrong place.
# The runner's OS need not match a row's marker (ruled), so a linux worker taking bulk rows is a
# legitimate configuration, not a hypothetical.
function Find-Exe([string] $dir, [string] $stem) {
    foreach ($c in @((Join-Path $dir "$stem.exe"), (Join-Path $dir $stem))) {
        if (Test-Path -LiteralPath $c -PathType Leaf) { return $c }
    }
    return $null
}

# ---------------------------------------------------------------------------------------------------
# ONE PARSE PER ROW, AND A DICTIONARY INSTEAD OF A PROPERTY GRAPH
#
# ⚠⚠ THIS IS NOT WHERE THE WRAPPER PHASE WENT, AND AN EARLIER DRAFT OF THIS COMMENT SAID IT WAS.
# G measured ~59 minutes of wrapper phase against a 66 s conversion on `crypto/cipher` (~52:1), and
# this leg measured 540 s against a 23 s conversion on `go/doc/comment` (~23:1). I wrote that down
# here as a parse cost. It is not: measured in this commit's own arm, the OLD parse of that row's
# 10,059-member document takes 0.1 SECONDS. The phase is the results-file read -- see the block above
# Test-ResultsTimedOut, where the 547 s is measured and the remedy is.
#
# What this change is actually worth, and it is worth doing on its own terms:
#     ONE parse per row      the sixth cut parsed the same document at the classifier AND again at
#                            the verdicts cross-check, so whatever it costs was paid twice
#     a dictionary           2.5x on the 10,059-member fixture, and no PSObject property graph for a
#                            document this script only ever reads by key
#     one set of consumers   the members are read the same way in both editions
#
# Two editions, two readers, neither of them `ConvertFrom-Json`:
#     Core (>= 6)   System.Text.Json JsonDocument          ordinal; keeps case-differing properties
#     5.1           JavaScriptSerializer.DeserializeObject a Dictionary[string,object]; likewise
# ⚠ AN EARLIER CUT OF THIS BLOCK SAID CORE USED `ConvertFrom-Json -AsHashtable`, and the eighth
# replaced that with System.Text.Json for the reason stated thirteen lines below -- PowerShell's
# hashtable is case-INSENSITIVE and folds the very names this reader exists to preserve. The line
# survived the change and contradicted the code under it, which is worse than saying nothing.
# Both answer .Keys / an indexer through System.Collections.IDictionary, so ONE set of consumers below
# serves both editions and there is no per-edition branch outside this function.
# ⚠⚠ AND THE NAMES MUST SURVIVE THE READ, ORDINALLY, ON BOTH EDITIONS.
#
# Legal Go test names differ only by case, and the corpus has instances: `math/rand` carries FOUR
# collision pairs (`TestUniformFactorial/n=3/Int31n` vs `.../int31n`) and `mime/multipart` one.
# Measured here against those two documents:
#
#     ConvertFrom-Json (5.1)          REFUSES outright -- "contains the duplicated keys"
#     ConvertFrom-Json -AsHashtable   PowerShell's hashtable is CASE-INSENSITIVE, so on Core the
#                                     same names collapse -- 47 arrive as 43 with NO error at all
#     JavaScriptSerializer (5.1)      Dictionary[string,object], ORDINAL: 47 and 52 preserved
#     System.Text.Json (Core)         ordinal, and it keeps properties differing only by case
#
# COORD `454195a30` (3): "a case-folding hashtable that keeps 43 is the same defect silently."
# A refusal is loud and a fold is not, which is why -AsHashtable is the worse of the two and is
# not used here despite being the shorter spelling.
function ConvertFrom-JsonElementOrdinal($el) {
    switch ($el.ValueKind.ToString()) {
        'Object' {
            $d = New-Object 'System.Collections.Generic.Dictionary[string,object]' ([System.StringComparer]::Ordinal)
            foreach ($p in $el.EnumerateObject()) { $d[$p.Name] = (ConvertFrom-JsonElementOrdinal $p.Value) }
            return $d
        }
        'Array'  {
            $l = New-Object 'System.Collections.Generic.List[object]'
            foreach ($i in $el.EnumerateArray()) { $l.Add((ConvertFrom-JsonElementOrdinal $i)) }
            return $l.ToArray()
        }
        'String' { return $el.GetString() }
        'Number' { $n = 0L; if ($el.TryGetInt64([ref] $n)) { return $n } else { return $el.GetDouble() } }
        'True'   { return $true }
        'False'  { return $false }
        default  { return $null }
    }
}

# ⚠⚠ THE READER IS PROVED ON A LITERAL, BEFORE ANY ROW IS READ.
#
# The count check beside the maps cannot see a folding READER -- both of its sides come from the
# document the reader produced. This one does not read a row's document at all: it parses a
# TWENTY-SIX byte literal carrying ONE case collision and requires BOTH names back. The expectation
# is 2, written here, and nothing the reader does can produce it. (An earlier comment said
# twenty-four; C2 counted the bytes and I had not.)
#
# It refuses the whole leg rather than a row, deliberately: a reader that folds would mis-score every
# row with a case collision and score the rest correctly, which is the shape that gets banked before
# anyone notices. Two names are cheaper to check than 107 rows are to re-run.
function Assert-OrdinalJsonReader {
    $probe = Join-Path ([System.IO.Path]::GetTempPath()) ("recon-canary-" + [guid]::NewGuid().ToString('N') + ".json")
    try {
        [System.IO.File]::WriteAllText($probe, '{"go":{"Aa":"1","aA":"2"}}')
        $doc = $null
        try { $doc = Read-JsonDocument $probe } catch {
            Deny "the JSON reader REFUSED a document carrying two names that differ only by case: $($_.Exception.Message). Windows PowerShell's ConvertFrom-Json does exactly this, which is why this wrapper does not use it."
        }
        $n = @(Get-DocKeys $doc 'go').Count
        if ($n -ne 2) {
            Deny "the JSON reader FOLDS names differing only by case -- $n of 2 survived a literal probe. Every row whose Go test names differ only by case would be scored on a short verdict count, silently. (math/rand carries four such pairs, mime/multipart one.)"
        }
        Write-Host "  ordinal-reader canary : 2 of 2 names survived a case collision"
    } finally { Remove-Item -LiteralPath $probe -Force -ErrorAction SilentlyContinue }
}

function Read-JsonDocument([string] $Path) {
    $raw = [System.IO.File]::ReadAllText($Path)
    if ($PSVersionTable.PSVersion.Major -ge 6) {
        $doc = [System.Text.Json.JsonDocument]::Parse($raw)
        try { return (ConvertFrom-JsonElementOrdinal $doc.RootElement) } finally { $doc.Dispose() }
    }
    Add-Type -AssemblyName System.Web.Extensions
    $ser = New-Object System.Web.Script.Serialization.JavaScriptSerializer
    # The default MaxJsonLength is ~4 MB and a row's comparison document has already been measured at
    # 4.77 MB. Left at the default this would throw on exactly the rows the change exists to serve.
    $ser.MaxJsonLength  = [int]::MaxValue
    $ser.RecursionLimit = 1024
    return $ser.DeserializeObject($raw)
}

# ⚠ A MISSING MEMBER IS AN EMPTY SET, NOT A THROW. A comparison document with no `csharp` object at
# all is a reading this classifier must survive -- it is what a row that failed to build looks like.
# The cast to IDictionary is not decoration: Dictionary[string,object] implements the non-generic
# Contains EXPLICITLY, so without the cast PowerShell does not find the method.
function Get-DocMember($doc, [string] $member) {
    if ($null -eq $doc -or -not ($doc -is [System.Collections.IDictionary])) { return $null }
    $d = [System.Collections.IDictionary] $doc
    # ⚠ NOT .Contains(), AND THE CAST DOES NOT RESCUE IT. Dictionary[string,object] -- what the
    # 5.1 deserializer returns -- implements the non-generic IDictionary.Contains EXPLICITLY, so it is
    # absent from the public method table PowerShell's adapter binds against, and the call throws
    # "Cannot find an overload for Contains". Measured by this commit's own arm on the 5.1 path,
    # which is the edition the leg runs.
    #
    # Membership here is over the TOP-LEVEL keys only (go, csharp, disclosed, matched, status), so a
    # linear test is a handful of comparisons and never touches the per-entry walk this commit exists
    # to remove.
    #
    # ⚠ THE TWO HALVES DISAGREED ABOUT CASE AND NOW DO NOT (C2's note, named so it is never found
    # as new). `-contains` is case-INSENSITIVE while `$d[$member]` on an ordinal dictionary is
    # case-SENSITIVE, so a document spelling `GO` would pass the membership test and then fetch
    # nothing -- an absent member reported as a present-but-empty one. `-ccontains` makes the two
    # halves ask the same question. The schema's members are lower-case and no corpus record spells
    # them otherwise, so this changes no reading; it removes a disagreement, not a bug.
    if (@($d.Keys) -cnotcontains $member) { return $null }
    return $d[$member]
}

function Get-DocKeys($doc, [string] $member) {
    $sub = Get-DocMember $doc $member
    if ($null -eq $sub -or -not ($sub -is [System.Collections.IDictionary])) { return @() }
    return @(([System.Collections.IDictionary] $sub).Keys)
}

# ⚠⚠ THIS IS THE WRAPPER PHASE. `Get-Content -Tail 400` OVER A ONE-LINE FILE IS ~QUADRATIC.
#
# Measured on all 14 rows of this leg: `go2cs_test_results.json` contains a SINGLE line -- 2.9 MB of
# it on `go/doc/comment`, 1.74 MB on `crypto/tls`. So `Get-Content -Tail 400` bounded NOTHING: the
# "tail" was always the entire document. Timed directly on this box, returning ONE line:
#
#     1.74 MB (crypto/tls)        191 s     <- from that row's evidence timestamps
#     2.90 MB (go/doc/comment)    547 s     <- timed directly; the row's own phase measured 540 s
#
# 1.67x the bytes for 2.86x the time, on two independent rows: the reader is about quadratic in file
# size, and it is the whole of the phase G reported at ~52:1. The replacement is 0.06 s on the same
# 2.9 MB fixture -- about 9,500x -- and the arm states its budget in MINUTES because at these sizes a
# budget in seconds is one the old path could miss without anyone noticing it had.
#
# ⚠ AND A BYTE BOUND CANNOT SIMPLY REPLACE IT. With one line, keeping "the last 256 KB" hands the
# TIMEOUT predicate a FRAGMENT of that line, and a deadline record earlier in the document is then
# invisible -- the arm would narrow silently and the row would be classified PASS. No row in this leg
# carried a timeout marker, so there is no positive sample here with which to show any particular
# window would have caught it, and a bound that cannot be red-tested is not one to ship.
#
# So the two readers are separated by what they actually need:
#     the PREDICATE  Test-ResultsTimedOut  streams the WHOLE file in overlapped chunks. Complete
#                    coverage, bounded MEMORY, and no per-line object at all.
#     the EVIDENCE   Get-TailLines         keeps a bounded tail for a human to read. Truncation is
#                    acceptable here and is stated in the file it writes.
# ⚠ THE OVERLAP IS NOT OPTIONAL. A marker straddling a chunk boundary is invisible to a per-chunk
# search, and it would fail exactly once in a while -- the worst failure rate there is. Each chunk
# carries the previous chunk's last ($marker length - 1) characters, so no boundary can hide one.
function Test-ResultsTimedOut([string] $Path) {
    $fi = New-Object System.IO.FileInfo $Path
    if (-not $fi.Exists -or $fi.Length -le 0) { return $false }
    # The same pattern the pipeline arm used, kept verbatim so this is a change of READER, not of
    # PREDICATE: a deadline kill states itself as `"action":"timeout"` with optional whitespace.
    $rx    = [regex] '"action"\s*:\s*"timeout"'
    $keep  = 64
    $chunk = New-Object char[] 1048576
    $sr = New-Object System.IO.StreamReader($Path, [System.Text.Encoding]::UTF8, $true)
    try {
        $carry = ''
        while ($true) {
            $n = $sr.Read($chunk, 0, $chunk.Length)
            if ($n -le 0) { break }
            $text = $carry + (New-Object string($chunk, 0, $n))
            if ($rx.IsMatch($text)) { return $true }
            if ($text.Length -gt $keep) { $carry = $text.Substring($text.Length - $keep) }
            else                        { $carry = $text }
        }
    } finally { $sr.Dispose() }
    return $false
}

function Get-TailLines([string] $Path, [int] $MaxLines = 400, [int] $MaxBytes = 262144) {
    $fi = New-Object System.IO.FileInfo $Path
    if (-not $fi.Exists) { return @() }
    $len = $fi.Length
    if ($len -le 0) { return @() }
    $take = [long] [Math]::Min([long] $MaxBytes, $len)
    $buf  = New-Object byte[] $take
    $read = 0
    # ReadWrite share: the converter may still hold the file open on a row that is being torn down.
    $fs = [System.IO.File]::Open($Path, [System.IO.FileMode]::Open, [System.IO.FileAccess]::Read, [System.IO.FileShare]::ReadWrite)
    try {
        $null = $fs.Seek($len - $take, [System.IO.SeekOrigin]::Begin)
        $read = $fs.Read($buf, 0, [int] $take)
    } finally { $fs.Dispose() }
    if ($read -le 0) { return @() }
    $lines = @([System.Text.Encoding]::UTF8.GetString($buf, 0, $read) -split "`r?`n")
    # ⚠ THE FIRST LINE OF A MID-FILE SEEK IS A FRAGMENT, and a half-line matched against the timeout
    # predicate is a verdict read off a broken string. Dropping it costs one line of a 400-line window.
    if ($take -lt $len -and $lines.Count -gt 1) { $lines = $lines[1..($lines.Count - 1)] }
    if ($lines.Count -gt $MaxLines) { $lines = $lines[($lines.Count - $MaxLines)..($lines.Count - 1)] }
    return @($lines)
}

$goVer = Join-Path $GoRoot 'VERSION'
if (-not (Test-Path -LiteralPath $GoRoot)) { Deny "no GOROOT at '$GoRoot'" }
$goExe = Find-Exe (Join-Path $GoRoot 'bin') 'go'
if (-not $goExe) { Deny "'$GoRoot/bin' holds neither 'go' nor 'go.exe' -- not a GOROOT" }
if (-not (Test-Path -LiteralPath $goVer))  { Deny "'$GoRoot' has no VERSION file -- the second derivation of the pin is unavailable" }

# ⚠⚠ PATH IS PART OF THE PIN, AND ASSERTING THE BINARY BY ABSOLUTE PATH DOES NOT ASSERT IT.
# The converter SPAWNS `go` from PATH (it shells out to parse the package). Measured on the one-row
# dry run: with GOROOT and GOTOOLCHAIN set but PATH untouched, the converter reached this box's
# ambient go1.23.1 and every row died
#     "go: ..\go.mod requires go >= 1.24 (running go 1.23.1; GOTOOLCHAIN=local)"
# -- while the preflight below reported the pin MET, because it invoked $goExe by ABSOLUTE PATH.
# A gate that proves the SDK at $GoRoot is the right release proves nothing about the `go` the
# converter will actually run. So: prepend, and then assert THROUGH PATH as well, which is the
# resolution the converter performs.
$env:PATH = (Join-Path $GoRoot 'bin') + [System.IO.Path]::PathSeparator + $env:PATH

Assert-OrdinalJsonReader

$goVersion = NativeFirstLine { & $goExe version }
$pathGo    = (Get-Command go -ErrorAction SilentlyContinue)
if (-not $pathGo) { Deny "no 'go' resolvable on PATH after prepending '$GoRoot\bin'" }
$pathGoVer = NativeFirstLine { & go version }
$verFile   = (Get-Content -LiteralPath $goVer -TotalCount 1)
Write-Host "  go version OUTPUT : $goVersion   (the SDK at -GoRoot, by absolute path)"
Write-Host "  go ON PATH        : $pathGoVer   (what the converter will SPAWN)"
Write-Host "  go resolved from  : $($pathGo.Source)"
Write-Host "  GOROOT VERSION    : $verFile   (the second derivation)"
# A refusal that does not NAME the value it refused on sends the reader back to reproduce it.
if ($goVersion  -notmatch 'go1\.24\.13') { Deny "the pin did not take -- 'go version' OUTPUT is '$goVersion', not the corpus pin go1.24.13" }
if ($pathGoVer  -notmatch 'go1\.24\.13') { Deny "PATH's go is '$pathGoVer' -- the converter spawns THIS one, not the SDK at -GoRoot" }
if ($verFile    -notmatch 'go1\.24\.13') { Deny "GOROOT/VERSION is '$verFile', not the corpus pin go1.24.13" }
# THREE derivations of one pin, and they must agree: the binary, the binary PATH resolves, and the file.
if ($pathGo.Source -notlike (Join-Path $GoRoot '*')) { Deny "PATH's go resolves to '$($pathGo.Source)', which is not under the pinned GOROOT '$GoRoot'" }

$freeGb = [int] ((Get-PSDrive -Name ($Tree.Substring(0,1))).Free / 1GB)
Write-Host "  disk free         : $freeGb GB (floor 25)"
if ($freeGb -lt 25) { Deny "$freeGb GB free is under the ruled 25 GB floor" }

$converter = Find-Exe (Join-Path $Tree 'src/go2cs') 'go2cs'
if (-not $converter) { $converter = Find-Exe (Join-Path $Tree 'src/go2cs/bin/Release/net10.0') 'go2cs' }
if (-not $converter) { Deny "no converter binary ('go2cs' or 'go2cs.exe') under '$Tree/src/go2cs' -- build it at this tree first" }
Write-Host "  converter         : $converter"

Write-Host ''
if (-not (Test-SummaryContract)) { Deny 'the summary-line contract FAILED -- refusing to parse rows with an unchecked predicate' }

# ---------------------------------------------------------------- the rows
$rows = @(Get-Content -LiteralPath $NameList |
          ForEach-Object { $_.Trim() } |
          Where-Object { $_ -and -not $_.StartsWith('#') })
if ($rows.Count -eq 0) { Deny "the name list holds no rows -- a basis over no rows is clean by construction" }

# ---------------------------------------------------------------- the per-row deadline floors
# ⚠ DERIVED FROM THE SWEEP, NEVER COPIED. shardmap.py derives the reserved set from this same table
# for a stated reason -- "a copied list drifted twice in the map's short life (crypto/tls joined the
# table, two floors moved)" -- and a second copy here would be the third drift waiting to happen.
# A row without a floor gets the sweep's own default, which is this script's -TestTimeout.
$sweepPath = Join-Path $Tree 'src/run-validated-sweep.ps1'
if (-not (Test-Path -LiteralPath $sweepPath)) { Deny "no run-validated-sweep.ps1 at '$sweepPath' -- the deadline floors are derived from it, not carried here" }
$sweepSrc = [System.IO.File]::ReadAllText($sweepPath)
$anchor = [regex]::Match($sweepSrc, '\$longTimeouts\s*=\s*@\{')
if (-not $anchor.Success) { Deny "cannot derive the deadline floors: no `$longTimeouts table in run-validated-sweep.ps1" }
$close = $sweepSrc.IndexOf('}', $anchor.Index + $anchor.Length)
if ($close -lt 0) { Deny "the `$longTimeouts table is unterminated" }
$tableText = $sweepSrc.Substring($anchor.Index + $anchor.Length, $close - ($anchor.Index + $anchor.Length))
$Floors = @{}
foreach ($m in [regex]::Matches($tableText, "'([^']+)'\s*=\s*'([^']+)'")) { $Floors[$m.Groups[1].Value] = $m.Groups[2].Value }
# ⚠ A derivation that reads ZERO entries is the silent-empty shape the generator documents (a
# non-greedy match yielding 6 of 11, then 1, then 0). An empty table would silently give every row
# the default floor and under-run the long ones.
if ($Floors.Count -lt 5) { Deny "derived only $($Floors.Count) deadline floor(s) from run-validated-sweep.ps1 -- the table is 11 rows; refusing to run on a floor set that did not parse" }

# ⚠ THE FAN-OUT: a relocated row's floor is inherited by its SUCCESSOR(S) (e0d5121e2 section 1).
# `crypto/internal/mlkem768` is a floor row AND one of the ten relocated paths, and it fans out to
# two successors -- so its floor must reach both or they run at the default and are killed short.
# ⚠⚠ THIS IS A COPY, AND IT IS LABELLED AS ONE. Owner: C1. Source of record: C1's relocation table,
# ruled the map of record at `350a301a` and seated at `4de76ded06`. The first cut of this file carried
# a ten-entry copy that C2 measured already wrong in four ways (one row absent, one target wrong, two
# missing their second half); the second cut deleted nine of them and kept only the floored one. Both
# were wrong answers to the same question, and COORD ruled the third: carry the WHOLE map here, as a
# copy that says whose it is, and make the DURABLE form a data file.
#
# ⚠ THE DURABLE FORM IS NOT THIS. `docs/phase4/hopA-inputs/relocations.tsv` (one line per arc) lands
# with C1's roster seat after the leg, and the sweep's own $longTimeouts re-path is derived from it in
# that same commit. This map and C2's reserved-set derivation switch to reading that file in commits
# that land WITH the seat -- the correction and the act that would expose it are one landing. Until
# then a copy is what exists, so it is named rather than disguised.
#
# ⚠ SOURCE -> TARGETS, NEVER 1:1. Three of the ten rows SPLIT, and `crypto/internal/fips140test`
# receives arcs from three different rows -- so a table keyed either way drops arcs silently. Every
# arm of a split inherits the floor (e0d5121e2 section 1): a budget copied is an over-estimate, which
# is the safe direction; a budget split is a guess.
#
# ⚠ COUNTS, MEASURED FROM THE SET RATHER THAN CARRIED: 10 rows, 13 arcs, 11 distinct targets, 3 splits.
# C1's prose and COORD's ruling both say 14 arcs (and C1 "four of the ten split"); the table they
# publish lists 13 across 3 splits, and their own 11-distinct-targets figure is consistent only with
# 13 (13 arcs - 3 into fips140test + 1 = 11; 14 would give 12). 10 rows + 4 splits = 14 is arithmetic
# that closes on itself. The SET below is C1's table verbatim; only the count is corrected.
$FlooredSuccessors = @{
    'crypto/internal/alias'              = @('crypto/internal/fips140test')
    'crypto/internal/bigmod'             = @('crypto/internal/fips140/bigmod')
    'crypto/internal/edwards25519'       = @('crypto/internal/fips140/edwards25519', 'crypto/internal/fips140test')
    'crypto/internal/edwards25519/field' = @('crypto/internal/fips140/edwards25519/field')
    'crypto/internal/mlkem768'           = @('crypto/internal/fips140/mlkem', 'crypto/mlkem')
    'crypto/internal/nistec'             = @('crypto/internal/fips140/nistec', 'crypto/internal/fips140test')
    'internal/concurrent'                = @('internal/sync')
    'internal/weak'                      = @('weak')
    'runtime/internal/math'              = @('internal/runtime/math')
    'runtime/internal/sys'               = @('internal/runtime/sys')
}
$inherited = 0
foreach ($old in $FlooredSuccessors.Keys) {
    if ($Floors.ContainsKey($old)) {
        foreach ($new in $FlooredSuccessors[$old]) {
            if (-not $Floors.ContainsKey($new)) { $Floors[$new] = $Floors[$old]; $inherited++ }
        }
    }
}
# ⚠ THE ASSERTION STAYS, and it is what makes a stale copy loud instead of silent. A floored row that
# neither exists at the tree nor has a mapping REFUSES and is named, rather than running at the default
# floor and being killed short. That is exactly the transition ahead: when the roster seat re-paths the
# sweep's table, any floored row this copy does not cover stops the leg instead of under-running it.
$orphanFloors = @()
foreach ($k in @($Floors.Keys)) {
    if ($FlooredSuccessors.ContainsKey($k)) { continue }          # mapped; its successors carry it
    if (-not (Test-Path -LiteralPath (Join-Path $Tree ("src/core/" + $k)))) { $orphanFloors += $k }
}
if ($orphanFloors.Count -gt 0) {
    Deny ("floored row(s) absent from the tree with no successor mapping: " + ($orphanFloors -join ', ') +
          " -- each would run at the DEFAULT floor and be killed short. Map them here, or re-path the sweep's table, before running the leg.")
}
Write-Host "  deadline floors   : $($Floors.Count) derived from the sweep ($inherited inherited by a floored relocation)"

Write-Host ''
Write-Host "  rows in list      : $($rows.Count)"
Write-Host "  output            : $Out"
if ($DryRun) { Write-Host '  MODE              : DRY RUN -- one row, nothing written' -ForegroundColor Cyan }

$emit = New-Object System.Collections.Generic.List[string]
$emit.Add("row`tword`tverdicts`tsweep_s`tfirst_in_list`trc`tdiverged`tplatform`ttree`twall_s`tpost_s")

$treeSha = (GitTry @('-C', $Tree, 'rev-parse', 'HEAD')).Out
# ⚠ THE PLATFORM IS GOOS/GOARCH BY `go env` OUTPUT, NOT .NET's OSVersion.Platform. The first cut
# defaulted to the latter and emitted `Win32NT` -- caught on the i7's Core-edition run, and masked in
# my own testing because I passed -Platform explicitly every time, so the default was never exercised.
# `windows/amd64` is the roster's own spelling and the one every other reading in this campaign uses.
$plat = $Platform
if (-not $plat) {
    $goos   = NativeQuiet { & go env GOOS }
    $goarch = NativeQuiet { & go env GOARCH }
    if ($goos -and $goarch) { $plat = "$goos/$goarch" }
}
if (-not $plat) { Deny "cannot determine the platform -- `go env GOOS/GOARCH` produced nothing, and a row's reading is not comparable without it" }

$i = 0
foreach ($row in $rows) {
    $i++
    $first = 0
    if ($i -eq 1) { $first = 1 }

    $goDir  = Join-Path $GoRoot  ("src/" + $row)
    $outDir = Join-Path $Tree    ("src/core/" + $row)

    # ⚠⚠ NO LIST RUNNER PASSES -test-allow-handown. COORD `576` (a), on C1's sizing: this line
    # is what destroyed R's `testing` row and the seven rows after it. The converter ALREADY has the
    # right construct -- `testTargetHandOwnHost`, owner-ruled 2026-08-30 -- which converts the external
    # test variant only and emits NO production file, and all three of its evidence clauses hold for
    # `testing` at the leg tree. It never fired because `requireConvertibleTestTarget` consults
    # `testAllowHandOwn` BEFORE the host mode, so this flag short-circuited past it and the row
    # converted production in place, over 10 `[module: GoManualConversion]` files, with no restore
    # between rows. THE ROW NEVER NEEDED THE FLAG; THE FLAG IS WHAT DESTROYED IT.
    #
    # The earlier comment here read "the pipeline refuses that row by design, and the refusal text
    # prescribes the flag". The refusal text prescribes it for a SCRATCH root, which is the census use;
    # aimed at the tree's own src/core it is the destructive case that flag's own text forbids.
    #
    # Never -tags either (the corpus axis comes by doing nothing: resolveBuildTags applies
    # purego,math_big_pure_go to every -tests run) and never -test-filter (its own warning says a
    # filtered run publishes NO validation artifacts and is DIAGNOSTIC ONLY -- it would not be a row).
    $extra = @()

    Write-Host ''
    Write-Host ("  -> {0,-44} [{1} of {2}]" -f $row, $i, $rows.Count)

    if ($DryRun -and $i -gt 1) { Write-Host '     (dry run: stopping after one row)'; break }

    # The row's own deadline floor where it has one, the sweep's default otherwise.
    $rowTimeout = $TestTimeout
    if ($Floors.ContainsKey($row)) { $rowTimeout = $Floors[$row]; Write-Host "     floor: $rowTimeout (derived)" }

    # ⚠⚠ THE CONVERTER'S STDERR IS A TERMINATING ERROR UNDER Stop, AND FAILING IS THE MEASUREMENT.
    # `GitTry` above carries this exact diagnosis for git: in 5.1 a native command's stderr becomes a
    # NativeCommandError, which ErrorActionPreference='Stop' raises. I applied it to every git call and
    # NOT here -- to the one native call in this file whose FAILURE is a reading rather than a fault.
    #
    # Measured 2026-09-20, twice, at row 4 of 16: `crypto/mlkem` fails its `dotnet publish` (the ruled
    # CS0311 row), the converter says so on stderr, and the SCRIPT DIED -- mid-list, before the exit
    # code was read and before the classifier ran.
    #
    # ⚠ So the CONVERT and BUILD arms twenty lines below were UNREACHABLE BY CONSTRUCTION: a row cannot
    # be classified as failing, because failing is what stopped the run. Every control I had ever run
    # against the classifier used a row that SUCCEEDED, which is why nothing caught it. That is the
    # mirror of this file's own banked lesson -- there, a guard whose SUCCESS case wrote to stderr
    # passed all its refusal controls; here, a classifier whose FAILURE cases write to stderr can never
    # reach its failure arms.
    #
    # ⚠ `2>&1` alone does NOT fix it: the merged record is still an ErrorRecord and Stop raises it. The
    # preference must be lowered around the call. It is lowered ONLY around the call, and restored in a
    # finally, so every other statement keeps Stop.
    #
    # ⚠ And unlike GitTry, stderr is KEPT rather than discarded (`2>&1`, not `2>$null`): the classifier
    # reads the converter's stderr TEXT to separate CONVERT from BUILD.
    $started = Get-Date
    # ⚠ RESET PER ROW. $rc was assigned ONLY inside the try, so an invocation that threw before the
    # $LASTEXITCODE read -- or that never launched a process -- handed the classifier and the TSV the
    # PREVIOUS row's exit code, silently, and the row was classified from a number it did not produce.
    # Lowering the preference makes that narrow; narrow is not closed. (C2 1e2adb3d, finding A.)
    $rc = $null
    $prevEap = $ErrorActionPreference
    $ErrorActionPreference = 'Continue'
    try {
        $output = & $converter -tests -test-action all -test-config $TestConfig -test-timeout $rowTimeout `
                               -go2cspath (Join-Path $Tree 'src') @extra $goDir $outDir 2>&1
        # CAPTURED ON THE VERY NEXT LINE, before anything touches $? or a pipe. Floor 7, and the fault
        # five lanes hit in one night.
        $rc = $LASTEXITCODE
    } catch {
        # ⚠ WITHOUT THIS CATCH THE GUARD BELOW IS UNREACHABLE FOR THE CASE IT NAMES. A throw out of
        # the invocation propagates past `finally` and out of the loop, so the run dies rather than
        # refusing -- the very shape this file calls "a guard that dies is not a guard that refused".
        # The error text is kept as the row's output so the evidence capture still has something to
        # write, and $rc is left $null so the classifier below reads the row as NOVERDICT and NAMES it.
        #
        # ⚠ THIS ASSIGNMENT WAS DEAD WHEN IT WAS WRITTEN, AND IS LIVE ONLY NOW. Under the sixth cut the
        # very next guard called Deny, and Deny exits -- so nothing on this path ever read $output and
        # the evidence capture it was written for could not be reached. Remedy (ii) below is what gives
        # it a reader: the row now runs on to the capture, and this text is what lands in that row's
        # output-no-summary.txt. A comment claiming a purpose the control flow denied is the shape this
        # file keeps catching elsewhere; it was in here too.
        $output = @("RECON: the converter invocation threw: $($_.Exception.Message)")
    } finally { $ErrorActionPreference = $prevEap }
    # ⚠⚠ A THROWN ROW IS CLASSIFIED; IT IS NOT A REASON TO END THE LEG. The sixth cut called Deny
    # here, and Deny EXITS -- so one row that threw discarded every row after it, which on a 105-row
    # list is hours of measurement thrown away to report one failure. COORD ruled remedy (ii): the row
    # is banked with the honest word and the loop carries on. THIS LOOP NOW KEEPS NO EXIT PATH AT ALL.
    #
    # The row still carries nothing it did not produce, which is what the Deny was protecting: `rc`
    # reads n/a, `diverged` reads n/a, `sweep_s` reads UNMEASURED, and the only number on the line is
    # `wall_s`, which this script observed itself.
    # ⚠ RESET PER ROW, for the same reason $rc is: a value carried from the previous row is the
    # defect C2 measured at finding A, and a DERIVED verdict count is exactly the kind of value that
    # would survive silently and read as this row's.
    $derivedVerdicts = $null
    $rowThrew = ($rc -isnot [int])
    if ($rowThrew) {
        Write-Host "     !! the invocation produced no exit code -- banking NOVERDICT, the leg carries on" -ForegroundColor Yellow
    }
    $elapsed = [int] ((Get-Date) - $started).TotalSeconds
    # post_s starts where sweep_s stops: everything from here to the emit is THIS SCRIPT's cost.
    $postStarted = Get-Date

    $lines = @($output | ForEach-Object { [string] $_ })
    $v = Get-VerdictCount $lines

    # ---- the artifacts, read BEFORE the classifier, because two of them DECIDE it
    $cmpSrc = Join-Path $outDir 'go2cs_test_comparison.json'
    $resSrc = Join-Path $outDir 'go2cs_test_results.json'
    $resTail = @(Get-TailLines $resSrc)

    # ⚠ ONE PARSE PER ROW, NOT TWO. The sixth cut parsed this same document twice -- once for
    # `diverged` in the classifier and again for the verdicts cross-check below -- so every second of
    # the phase G measured was paid TWICE on every row that reached both. Parsed here, once, and the
    # two readers share it. `$cmpUnreadable` is kept apart from a null document because "no artifact"
    # and "an artifact I could not read" are different facts and the classifier separates them.
    #
    # ⚠⚠ AND IT IS READ ONLY IF THIS ROW WROTE IT. COORD `989`, on C1's finding: the comparison
    # record is GITIGNORED (`src/core/.gitignore:19` names it) and `git clean -fd` skips it, so a PRIOR
    # run's document survives in the tree and is indistinguishable from this run's. Deriving a word
    # from it would report a previous run's verdicts as this row's -- R's contamination class one layer
    # over, with arithmetic the only tell.
    #
    # ⚠ LastWriteTime, NOT CreationTime, AND THE DIFFERENCE IS LOAD-BEARING HERE. The evidence spec
    # (`989` (a)) names CreationTime because its question is "was this file COPIED in", where a copy
    # inherits its source's write time. THIS question is the other one -- "did THIS row write it" --
    # and an OVERWRITE leaves CreationTime at the original creation (NTFS also tunnels it back through
    # a delete-and-recreate within seconds). So a fresh record overwritten in place would read STALE
    # under CreationTime. LastWriteTime is correct whether the converter overwrites or recreates, which
    # is why it is the predicate here and CreationTime is the predicate there.
    $cmpDoc        = $null
    $cmpUnreadable = $false
    $cmpStale      = $false
    if (Test-Path -LiteralPath $cmpSrc) {
        $cmpWrite = (Get-Item -LiteralPath $cmpSrc -Force).LastWriteTime
        if ($cmpWrite -lt $started) {
            $cmpStale = $true
            Write-Host ("     !! comparison record predates this row (written {0}, row started {1}) -- STALE, not read" -f `
                $cmpWrite.ToString('HH:mm:ss.fff'), $started.ToString('HH:mm:ss.fff')) -ForegroundColor Yellow
        } else {
            try { $cmpDoc = Read-JsonDocument $cmpSrc } catch { $cmpUnreadable = $true }
        }
    }

    # ---- the word: the outcome class this cost was measured under (COORD's vocabulary)
    #
    # ⚠⚠ BUILD AND TIMEOUT ARE rc-GUARDED, and TIMEOUT is decided from the RESULTS TAIL, never from a
    # substring of the output. C2 measured the defect in the first cut: the two arms carried no rc
    # guard, so a row that PASSES whose output merely CONTAINS "timed out" was classed TIMEOUT -- and
    # since TIMEOUT now forces a non-integer sweep_s, that row leaves the plan. Eight rows in the
    # 207-row set carry the literal in their test sources, including `net` and `net/http`, which are
    # two the basis most needs. A deadline kill STATES ITSELF in the results file
    # (`"action":"timeout"`), so the authoritative signal is the artifact, not the console.
    $diverged = ''
    # ⚠ THE WHOLE DOCUMENT, NOT THE KEPT TAIL. $resTail is bounded for the evidence copy; deciding a
    # row's WORD from a bounded window would narrow this arm by exactly the amount that was trimmed.
    $timedOut = Test-ResultsTimedOut $resSrc
    # ⚠ THE THROWN ARM COMES FIRST, AND ITS ORDER IS LOAD-BEARING. With $rc left $null, `$rc -ne 0`
    # is TRUE, so every rc-guarded arm below is reachable on a row that never produced an exit code --
    # a thrown row whose error text happened to contain "error CS1234" would be classified BUILD.
    if     ($rowThrew)                                                             { $word = 'NOVERDICT' }
    elseif ($rc -ne 0 -and ($lines -match 'Conversion failed|unresolved dynamic')) { $word = 'CONVERT' }
    elseif ($rc -ne 0 -and ($lines -match 'error CS[0-9]+'))                       { $word = 'BUILD' }
    elseif ($timedOut)                                                             { $word = 'TIMEOUT' }
    # ⚠⚠ AN ABSENT SUMMARY IS NOT AN ABSENT RESULT. COORD `f45a3643d` (1), on R's `unicode/utf8`:
    # the converter prints its `Validated N tests` line ONLY on a MATCHED comparison, so a row with one
    # undisclosed divergence prints nothing, `verdicts` read NOMATCH, the word fell through to
    # NOVERDICT and `sweep_s` to UNMEASURED -- EVERY DIVERGED READING ON ALL THREE LISTS WAS HIDDEN
    # INSIDE NOVERDICT AND ITS COST DROPPED FROM THE BASIS. R's DIVERGED count of 0 over 105 rows is
    # that defect, not a fact about the corpus.
    #
    # So NOVERDICT is now reserved for a row with NO USABLE COMPARISON DOCUMENT -- none written, one
    # that will not parse, one that predates this row, or a thrown invocation. A row whose converter
    # ran all the way to a comparison is never UNMEASURED: it falls through to the derivation below,
    # which reads the same net-undisclosed set whether or not the summary happened to print.
    #
    # ⚠⚠ A STALE RECORD IS ITS OWN ARM AND IT COMES FIRST, because the arm below is not enough.
    # MEASURED on the tenth: the same row twice in one tree, the second run printing "STALE, not read"
    # and then emitting PASS 61 / diverged 0 -- a verdict computed from NO DOCUMENT AT ALL. The arm
    # below requires BOTH no-summary AND no-document; a stale record leaves `$cmpDoc` null but leaves
    # `$cmpUnreadable` FALSE, because nothing ever attempted a parse, so a row WITH a summary fell
    # through, read an empty map, and reached `$d = 0`.
    #
    # `$cmpStale` and `$cmpUnreadable` are two different facts and only one of them had a consequence.
    # This is the file's own "AN UNREADABLE ARTIFACT IS NOT A PASS" rule, which the staleness gate I
    # added to satisfy COORD `989` walked straight past: the gate fired, printed, and was ignored.
    elseif ($cmpStale)                                                             { $word = 'NOVERDICT' }
    elseif ($null -eq $v.Count -and $null -eq $cmpDoc)                             { $word = 'NOVERDICT' }
    else {
        # ⚠ DISTINCT DIVERGING TEST NAMES from the comparison JSON -- not output lines, which count
        # mentions rather than tests. Ordinal/case-sensitive: legal Go test names differ only by case.
        $diverged = 0
        if (Test-Path -LiteralPath $cmpSrc) {
            try {
                if ($cmpUnreadable) { throw 'the comparison document could not be parsed' }
                $jj = $cmpDoc
                # ⚠⚠ ORDINAL MAPS, AND AN EARLIER CUT OF THIS FILE SAID OTHERWISE. It carried
                # `$goMap = @{}` with a comment calling that a real defect deliberately left alone as
                # out of scope. It is not out of scope: COORD `454195a30` (3) requires all 47 of
                # `math/rand`'s names to survive in BOTH editions, and a `@{}` here reduced them to 43
                # with no error -- measured on R's committed fixture, 4 names lost, and 1 on
                # `mime/multipart`. A PowerShell hashtable's default comparer is case-INSENSITIVE;
                # the $names set beside it was already Ordinal, so the two disagreed about what a
                # name is. Whatever the deserializer preserves, these must not throw away.
                $goSub = Get-DocMember $cmpDoc 'go'
                $csSub = Get-DocMember $cmpDoc 'csharp'
                $goMap = New-Object 'System.Collections.Generic.Dictionary[string,string]' ([System.StringComparer]::Ordinal)
                if ($goSub -is [System.Collections.IDictionary]) {
                    foreach ($k in @(([System.Collections.IDictionary] $goSub).Keys)) { $goMap[[string] $k] = [string] $goSub[$k] }
                }
                $csMap = New-Object 'System.Collections.Generic.Dictionary[string,string]' ([System.StringComparer]::Ordinal)
                if ($csSub -is [System.Collections.IDictionary]) {
                    foreach ($k in @(([System.Collections.IDictionary] $csSub).Keys)) { $csMap[[string] $k] = [string] $csSub[$k] }
                }
                # ⚠ THIS CATCHES A MAP-CONSTRUCTION REGRESSION AND NOTHING WIDER, AND AN EARLIER
                # CUT CLAIMED MORE. It said "if a future edition's reader folds anyway, the row says
                # so" -- which is the ONE case it cannot see, because BOTH sides of the comparison
                # come from the same parsed document: a folding reader yields a folded document AND a
                # folded map, the two agree, and this stays silent. (C2's finding; and this lane's
                # own rule that a gate must not compare a value to its own variable.)
                #
                # What it DOES catch is the regression that actually happened -- ordinal keys read
                # correctly and then dropped into a case-folding container on the way to the map --
                # which is worth keeping. The READER is proved separately, on a literal, by the
                # canary in the preflight, whose expectation the reader did not produce.
                $goKeyN = @(Get-DocKeys $cmpDoc 'go').Count
                if ($goMap.Count -ne $goKeyN) {
                    Write-Host ("     !! ORDINAL NAMES LOST between the parsed document and the map: the document carries $goKeyN `go` names and the map holds $($goMap.Count). This is a MAP-CONSTRUCTION fault; a folding READER is invisible here and is the canary's job.") -ForegroundColor Red
                    throw "the comparison document's names did not survive the read ordinally ($goKeyN -> $($goMap.Count))"
                }
                # ⚠ THE DISCLOSED ONES ARE SUBTRACTED, AND THEY ARE PROSE, NOT NAMES. `disclosed` is a
                # list of SENTENCES -- "TestReadStringAllocs (alloc-profile): at-most-one AllocsPerRun
                # assert: ..." -- so a membership test against the whole string never matches and every
                # disclosed row reads DIVERGED. Measured on bufio: 1 diverging name, and it IS the
                # disclosed one, so the NET is 0 and the row is a PASS. The name is the leading token.
                $disclosedNames = New-Object System.Collections.Generic.HashSet[string] ([System.StringComparer]::Ordinal)
                foreach ($s in @(Get-DocMember $cmpDoc 'disclosed')) {
                    if ($s -and ($s -match '^(\S+)')) { $null = $disclosedNames.Add($Matches[1]) }
                }
                $names = New-Object System.Collections.Generic.HashSet[string] ([System.StringComparer]::Ordinal)
                foreach ($k in $goMap.Keys) { $null = $names.Add($k) }
                foreach ($k in $csMap.Keys) { $null = $names.Add($k) }
                $d = 0
                foreach ($n in $names) {
                    if ($disclosedNames.Contains($n)) { continue }
                    $g = ''; if ($goMap.ContainsKey($n)) { $g = $goMap[$n] }
                    $c = ''; if ($csMap.ContainsKey($n)) { $c = $csMap[$n] }
                    if (-not [string]::Equals($g, $c, [System.StringComparison]::Ordinal)) { $d++ }
                }
                # ⚠ THE DERIVED COUNT, WHEN THE SUMMARY DID NOT PRINT. The arithmetic is the
                # CONVERTER'S OWN -- the expression `len(goResults)-len(disclosed)` inside the
                # `Validated %d tests against go test` summary -- pinned by C1 so that three lanes and one
                # assembler do not each invent one:
                #     verdicts = len(go) - len(disclosed)
                # `withdrawn` is already absent from the Go map and is NOT subtracted again; `gated` is
                # published beside the count and is NOT subtracted; `matched` is a BOOL in the schema,
                # never a count. len(go) is taken from the DOCUMENT's key list, not from $goMap, whose
                # 5.1 comparer would collapse two names differing only by case.
                if ($null -eq $v.Count) {
                    $goKeyCount     = @(Get-DocKeys $cmpDoc 'go').Count
                    $disclosedCount = @(Get-DocMember $cmpDoc 'disclosed').Count
                    $derivedVerdicts = $goKeyCount - $disclosedCount
                    Write-Host ("     .. no summary line, but a comparison record: verdicts derived as {0} - {1} = {2}" -f `
                        $goKeyCount, $disclosedCount, $derivedVerdicts) -ForegroundColor Cyan
                }

                # `diverged` is the NET, UNDISCLOSED count: a disclosed divergence is accounted for by
                # the roster and is not a finding. The artifact's own `matched`/`status` corroborate it,
                # and a disagreement is reported rather than resolved silently.
                $diverged = $d
                $mMatched = Get-DocMember $cmpDoc 'matched'
                if ($d -eq 0 -and $mMatched -ne $true) {
                    Write-Host "     !! net diverged 0 but the artifact says matched=$mMatched status=$(Get-DocMember $cmpDoc 'status')" -ForegroundColor Yellow
                }
            } catch { $diverged = 'UNREAD' }
        }
        # ⚠ AN UNREADABLE ARTIFACT IS NOT A PASS. The first cut fell to PASS whenever $diverged was not
        # an int, so a comparison JSON nobody could read produced the basis's strongest verdict --
        # while `verdicts`, twenty lines down, refuses to guess for exactly the stated reason. `word`
        # is the only record of WHICH verdict a cost was measured under, and a reader filtering on it
        # would have seen a pass over an artifact that was never read. NOVERDICT is the honest class:
        # the row ran, and this instrument cannot say what it did.
        if     ($diverged -isnot [int]) { $word = 'NOVERDICT' }
        elseif ($diverged -gt 0)        { $word = 'DIVERGED' }
        else                            { $word = 'PASS' }
    }

    # ---- verdicts: cross-checked, and NEVER 0 on a failure to read.
    # shardmap reads a non-integer as None and still schedules the row; an APPROXIMATION poisons every
    # derived figure. So a disagreement or a miss emits the word NOMATCH, not a number.
    $verdicts = 'NOMATCH'
    # ⚠ A STALE ROW BANKS NO COUNT EITHER. The summary line is this run's output and its number is
    # real, but it cannot be cross-checked against a document this run wrote, and a count banked under
    # NOVERDICT is the shape this file refuses everywhere else. NOMATCH, by the same rule remedy (ii)
    # applies to a thrown row.
    #
    # ⚠⚠ THIS LINE IS A RESTATEMENT OF INTENT AND IS PROVABLY INERT: the initialiser three lines
    # above already holds 'NOMATCH' and nothing between them touches $verdicts. IT IS NOT THE
    # MECHANISM. The mechanism is the two `-not $cmpStale` guards below, which are what stop
    # $v.Count from over-writing NOMATCH afterwards -- so deleting either of them because this
    # explicit line "already covers the case" would reinstate the defect the eleventh fixed. Kept
    # because an explicit statement beside a guard is this file's habit; labelled because C1 named
    # the inverse risk, which is the one that actually bites.
    if ($cmpStale) { $verdicts = 'NOMATCH' }
    # ⚠ A DERIVED COUNT IS A MEASUREMENT, NOT A GUESS, AND IT IS WHY THIS ROW IS NOT NOMATCH. It is
    # the converter's own expression over this row's own record; the cross-check below is skipped for
    # it only because the cross-check compares the SUMMARY against the map, and there is no summary.
    if (-not $cmpStale -and $null -eq $v.Count -and $null -ne $derivedVerdicts) { $verdicts = $derivedVerdicts }
    if (-not $cmpStale -and $null -ne $v.Count) {
        $verdicts = $v.Count
        if (Test-Path -LiteralPath $cmpSrc) {
            try {
                if ($cmpUnreadable) { throw 'the comparison document could not be parsed' }
                # THE SAME DOCUMENT THE CLASSIFIER READ, not a second parse of the same file.
                # ORDINAL by the sweep's own comment: legal Go verdict names differ ONLY BY CASE, so a
                # case-insensitive count COLLAPSES those pairs and undercounts with a plausible integer.
                $goNames = @(Get-DocKeys $cmpDoc 'go')
                $mapCount = ($goNames | Sort-Object -CaseSensitive -Unique).Count
                # ⚠⚠ THE TWO NUMBERS DIFFER BY THE DISCLOSURES, BY CONSTRUCTION -- measured on the
                # one-row dry run, where bufio read "summary 80 vs map 81" and my first cross-check
                # called that a disagreement. the converter's disclosed branch passes
                # `len(goResults) - len(disclosed)` as the headline count while the map holds ALL of
                # goResults, so a bare equality test fires on EVERY row carrying a disclosure and
                # throws away a perfectly good cost. The relation is:
                #     map == summary + disclosed-divergent
                # The no-disclosure branch prints no such group, and 0 is then correct.
                $disclosed = 0
                if ($v.Line -match '(\d+) disclosed-divergent') { $disclosed = [int] $Matches[1] }
                if ($mapCount -ne ($v.Count + $disclosed)) {
                    Write-Host ("     !! verdicts DISAGREE: map $mapCount != summary $($v.Count) + disclosed $disclosed -- emitting NOMATCH") -ForegroundColor Yellow
                    $verdicts = 'NOMATCH'
                }
            } catch {
                Write-Host '     !! comparison JSON unreadable -- the count cannot be cross-checked' -ForegroundColor Yellow
            }
        }
    }

    # ⚠⚠ NO COUNT IS BANKED UNDER NOVERDICT, WHATEVER PUT IT THERE -- AND THE STALE ARM ABOVE WAS
    # THE ONLY FACT THAT GOT THAT RULE. MEASURED, not reasoned: a FRESH but UNREADABLE record reads
    # word=NOVERDICT and reached here holding verdicts=61 -- a count taken from the summary line of a
    # run whose comparison document could not be parsed at all. That is verbatim the shape the stale
    # arm refuses thirty lines above, by the same sentence: "a count banked under NOVERDICT is the
    # shape this file refuses everywhere else". The file refused it in one place and produced it in
    # another, by a different route to the same word.
    #
    # ⚠ DERIVED FROM THE WORD RATHER THAN ADDED AS A FIFTH SPECIAL CASE, which is C1's own reading
    # of why the ruled quad holds: `sweep_s` and `diverged` FOLLOW from the word instead of being
    # assigned in parallel, so they cannot drift out of sync with it. One line here covers all FOUR
    # facts in the list below -- stale, unreadable, thrown, and no-summary-no-document -- where four
    # parallel assignments would leave the next fact uncovered exactly as the third one was.
    #
    # NOTHING BANKED MOVES: all three NOVERDICT rows of this lane's leg already read NOMATCH, so this
    # closes a reachable hole rather than restating a figure.
    if ($word -eq 'NOVERDICT') { $verdicts = 'NOMATCH' }

    # ⚠ wall_s IS THE OBSERVED WALL FOR EVERY ROW, WHATEVER ITS WORD, AND IT IS ALWAYS AN INTEGER.
    # `sweep_s` answers "what may this row be SCHEDULED on" and is deliberately non-integer when the
    # row earned no cost. `wall_s` answers the different question "how long did this actually take",
    # which is still a fact for a row that timed out or produced no verdict -- and it is the only
    # number the hand-stopped row `net` can ever supply, because `net` is expected to TIMEOUT and its
    # cost in both 1.23 passes was a lower bound produced by stopping it. The concatenation banks
    # `net` with `sweep_s := wall_s`; without this column there is nothing to substitute FROM, and the
    # basis is refused either way (C2 `7c71a87f`, COORD's ruling).
    $wallS  = $elapsed
    $sweepS = $elapsed
    if ($word -eq 'TIMEOUT') {
        # A deadline kill is not a cost: the row did not finish, so its wall is a floor the operator
        # imposed, not a measurement of the row. NON-INTEGER -- which the generator REFUSES by name
        # and by value ("a row with no measured cost is UNSCHEDULED, never nominal"); the banked basis
        # excludes such rows at the concatenation.
        $sweepS = 'UNMEASURED'
    }
    if ($word -eq 'NOVERDICT') {
        # ⚠ FOUR DIFFERENT FACTS REACH THIS ONE WORD, AND THIS COMMENT HAS NOW BEEN INCOMPLETE
        # TWICE (C2 queued the first correction since 765aba82; C1 found the second in the eleventh's
        # delta read). They are not interchangeable to anyone reading the TSV:
        #     (a) NO SUMMARY LINE        the row produced no verdict at all
        #     (b) AN UNREADABLE ARTIFACT a comparison JSON existed and this instrument could not read
        #                                it. The row may well have PASSED; NOVERDICT says only that
        #                                nothing here can tell -- which is why it is not a PASS.
        #     (c) A THROWN INVOCATION    remedy (ii): the converter returned no exit code at all.
        #     (d) A STALE RECORD         a comparison JSON existed, was readable, and PREDATES this
        #                                row -- so it describes an EARLIER run. ⚠⚠ THIS IS THE ONE
        #                                CAUSE THAT LEAVES NO TRACE IN THE ROW'S OWN OUTPUT, because
        #                                the converter ran fine. A reader holding the old three-item
        #                                list and a clean-looking row had no reason to go looking for
        #                                `noverdict-cause.txt`, which is written for exactly this
        #                                case. (C1's finding, and the omission bit hardest here.)
        # They share the word because they share ONE consequence: the wall is real and is emitted as
        # wall_s, but a cost banked under no verdict is a number with no evidence behind it, so the
        # generator REFUSES it and the concatenation excludes the row.
        $sweepS = 'UNMEASURED'
    }

    # ⚠ AN EMPTY FIELD READS AS A ZERO. A row that produced no comparison artifact at all -- BUILD,
    # CONVERT, TIMEOUT, NOVERDICT -- left `diverged` at its initial empty string, and an empty column
    # in a file three lanes are concatenated into is the empty-counter class this fleet banked twice
    # tonight. `n/a` and not `UNREAD`: UNREAD means an artifact EXISTED and could not be read, which
    # is a different and worse fact than never having produced one.
    # ⚠⚠ `-eq ''` COERCES, AND IT WAS DESTROYING THE COLUMN. PowerShell converts the RIGHT operand
    # to the LEFT operand's type, so `0 -eq ''` is TRUE: a row with a REAL net-undisclosed count of
    # ZERO had it rewritten to `n/a`. Measured on this leg -- all TEN PASS rows emitted `diverged=n/a`
    # while their comparison records read `matched=true` -- so the column asserted "this row never
    # produced an artifact", which is this comment's own definition of n/a, of ten rows that did.
    #
    # This is inside the ruled set rather than beside it: COORD `f45a3643d` (1) specifies `diverged` =
    # the net undisclosed distinct names for exactly the rows the DIVERGED derivation now reaches, and
    # a PASS row's net count is 0. Left as it was, the column cannot express the value the same ruling
    # requires it to carry, and the assembler could not tell "no divergences" from "no artifact".
    #
    # The type test first is the whole fix: only a STRING that is empty means "never set".
    if ($diverged -is [string] -and $diverged -eq '') { $diverged = 'n/a' }

    # ⚠ AND THE SAME RULE FOR rc. A row that threw has no exit code; emitting an empty cell would read
    # as a zero, which is the SUCCESS value -- the worst possible default for the one row that failed
    # hardest. n/a, for the same reason `diverged` uses it one line above.
    $rcCell = $rc
    if ($rowThrew) { $rcCell = 'n/a' }

    # ---- CAPTURE THE EVIDENCE BEFORE THE TREE IS DISCARDED (ruled)
    # The worktree is removed when the list is done, and with it every artifact this row produced.
    # The diverged sets are the leg's readings; losing them would make the run unrepeatable without
    # re-running it. Keyed by row, under the scratch, which the guard proved is outside any work tree.
    $rowKey = ($row -replace '[\\/]', '__')
    $rowDir = Join-Path $Scratch $rowKey
    if (-not (Test-Path -LiteralPath $rowDir)) { New-Item -ItemType Directory -Path $rowDir -Force | Out-Null }

    # $cmpSrc / $resSrc / $resTail are read ABOVE, before the classifier, because the tail DECIDES
    # the TIMEOUT arm. One definition, used twice: the classifier reads it, and this copies it out.
    if (Test-Path -LiteralPath $cmpSrc) { Copy-Item -LiteralPath $cmpSrc -Destination (Join-Path $rowDir 'go2cs_test_comparison.json') -Force }
    if ($resTail.Count -gt 0) {
        # ⚠ THIS COPY IS BOUNDED AND SAYS SO. The results file is one line of up to a few MB; a human
        # reading the evidence wants its end, not all of it, and the row's WORD was decided by
        # Test-ResultsTimedOut over the WHOLE file rather than from this. Without the banner a later
        # reader could search this file, find no timeout marker, and conclude the row did not time out
        # -- a truncation reading as a fact, which is this fleet's most repeated failure.
        $banner = "# BOUNDED EVIDENCE COPY -- at most $($resTail.Count) line(s) / 256 KB from the END of"
        $banner += " $resSrc. The TIMEOUT arm did NOT read this file; it streamed the whole document."
        [System.IO.File]::WriteAllText((Join-Path $rowDir 'results-tail.txt'), ($banner + "`n" + ($resTail -join "`n") + "`n"))
    }

    # ⚠ THE CAUSE OF A NOVERDICT, WRITTEN WHERE A READER WILL LOOK. COORD `989` asked for the cause
    # and then ruled at `267113705a` that there is NO tail column and none is added -- it lives in the
    # row's evidence directory and the completion post. A stale record is the one NOVERDICT cause that
    # leaves no trace in the row's own output, because the converter ran fine; without this file the
    # row is indistinguishable from one whose comparison simply did not match.
    if ($cmpStale) {
        [System.IO.File]::WriteAllText((Join-Path $rowDir 'noverdict-cause.txt'),
            "stale record: the comparison document at $cmpSrc predates this row's start, so it " +
            "was not read and no word was derived from it. The record is a survivor of an earlier " +
            'run in this tree (it is gitignored, and `git clean -fd` skips it). Re-run in a tree ' +
            "whose pre-row-1 residue census removed prior records by name." + [char] 10)
    }

    # The summary line itself, and the whole stdout when there ISN'T one -- a row with no summary is
    # exactly the row whose output someone will want to read, and it is the row whose tree is discarded.
    if ($v.Line) { [System.IO.File]::WriteAllText((Join-Path $rowDir 'summary.txt'), $v.Line + "`n") }
    else         { [System.IO.File]::WriteAllText((Join-Path $rowDir 'output-no-summary.txt'), (($lines -join "`n") + "`n")) }

    Write-Host ("     {0,-10} verdicts={1,-8} {2}s  rc={3}   evidence -> {4}" -f $word, $verdicts, $elapsed, $rcCell, $rowKey)
    if ($DryRun -and $v.Line) {
        Write-Host ''
        Write-Host '     the MATCHED summary line, beside the row it produced:' -ForegroundColor Cyan
        Write-Host "       $($v.Line)"
    }

    # Closed as late as possible: everything after the converter returned is this script's own cost,
    # and the evidence capture above is part of it.
    $postS = [int] ((Get-Date) - $postStarted).TotalSeconds
    $emit.Add("$row`t$word`t$verdicts`t$sweepS`t$first`t$rcCell`t$diverged`t$plat`t$treeSha`t$wallS`t$postS")

    if ($DryRun) { break }
}

# ---------------------------------------------------------------- write, LF only
$text = ($emit -join "`n") + "`n"
$crs  = ([regex]::Matches($text, "`r")).Count
Write-Host ''
Write-Host "  CR bytes in the emission: $crs   (shardmap.py refuses the basis on ANY CR)"
if ($crs -ne 0) { Deny "the emission carries $crs CR byte(s); the generator refuses a basis on any CR" }

if ($DryRun) {
    Write-Host ''
    Write-Host '  DRY RUN -- the row that WOULD be emitted:'
    Write-Host "    $($emit[$emit.Count - 1])"
    Write-Host ''
    Write-Host '  nothing written.'
    exit 0
}

[System.IO.File]::WriteAllText($Out, $text)
Write-Host ''
Write-Host "  written: $Out ($($emit.Count - 1) row(s), LF only)"

# The hand-stopped drop must be able to fire, or shardmap refuses the whole basis by name.
$hasNet = @($emit | Where-Object { $_ -match '^net\t' }).Count
if ($hasNet -eq 0) {
    Write-Host ''
    Write-Host '  !! NOTE: no `net` row in this emission. shardmap.py refuses a basis in which the' -ForegroundColor Yellow
    Write-Host '     hand-stopped drop never fired, so the COMBINED basis must carry it from some worker.' -ForegroundColor Yellow
}
exit 0
