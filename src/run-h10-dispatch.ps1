# run-h10-dispatch.ps1 - the H10 per-row dispatch driver.
#
# Reads the MACHINE-READABLE plan emitted by docs/phase4/hopA-inputs/shardmap.py --emit-plan and runs
# one worker's rows, in order, slice by slice, with the ruled cooldown gap between slices. Coordinator
# ruling e0d5121e2 section 7: "an H10 dispatch script that reads the emitted map and runs a worker's
# rows in order with the cooldown gaps of section 4 -- C2 designs it, i9 accepts it on one slice."
#
# Design record: docs/phase4/DESIGN-h10-dispatch-driver.md. Read that before changing anything here;
# the three refusals below are the whole point of the script and each one is there because the
# alternative fails SILENTLY.
#
# ---------------------------------------------------------------------------------------------------
# WHY THIS READS A TSV AND NOT THE MAP REPORT
#
# The generator's stdout is a report for a human and it defeats a parser three ways, each silently
# (measured 2026-09-13):
#
#   1. the row lists WRAP at column 118, so a line is not a record;
#   2. worker names carry SPACES and PARENTHESES ("i9-13900K (sweeper)"), so whitespace is not a
#      delimiter;
#   3. the SAME worker appears in EVERY `W` section with a DIFFERENT row set -- R-LAPTOP holds 85 rows
#      at W=3 and 60 at W=4 -- so a driver grepping its own worker name takes whichever section comes
#      first and dispatches 25 rows it was not assigned, with nothing anywhere reading wrong.
#
# So: `W` is a required parameter and a COLUMN in the plan. A driver that does not say which fleet size
# it is dispatching gets NO rows and a refusal, never the wrong ones.
#
# ---------------------------------------------------------------------------------------------------
# THE COOLDOWN IS BETWEEN SLICES, NOT BETWEEN ROWS
#
# Ruled at 4327ab7e1 section 2: slice cap 40 minutes, ten-minute cooldown, two slices and one gap for
# the i9's reserved leg. The cap is a BLAST-RADIUS limit on a box with one recorded thermal death, not
# a performance tuning knob -- and `-ShardCount`'s own comment says the gap belongs to the caller,
# which is this script. Both numbers are READ FROM THE PLAN rather than written here, so the ruling
# lives in one place and a plan generated under a different cap dispatches under that cap.
#
# A row is INDIVISIBLE: the sweep's unit of dispatch is a package. crypto/dsa alone is 1,317 s, which
# is why no cap below 21.95 minutes can exist, and why a slice holding one over-cap row is legal.
[CmdletBinding()]
param(
    # Every one of these three is MANDATORY and none has a default, deliberately. A default fleet size
    # or a guessed worker is the silent-wrong-rows failure above wearing a convenience.
    [Parameter(Mandatory)][string] $Plan,
    [Parameter(Mandatory)][string] $Worker,
    [Parameter(Mandatory)][int]    $FleetSize,

    # 0 = every slice this worker holds. i9's acceptance runs ONE slice, which is what this is for.
    [int] $OnlySlice = 0,

    # Print the dispatch -- every row, every slice boundary, every cooldown -- and run nothing. This is
    # the acceptance vehicle: it is decidable without a sweep, so a reader can check the driver
    # dispatches the rows the plan assigns before spending an hour finding out.
    [switch] $DryRun,

    # Per-row timings, TSV. Defaults beside the plan so a run is self-documenting.
    [string] $TimingOut,

    # The sweep. Defaulted RELATIVE TO THIS SCRIPT rather than to the caller's working directory,
    # because a driver resolved from $PWD runs a different tree's sweep than the one it was read from.
    [string] $SweepScript,

    # Escape hatch for the ramp experiment inside the recon leg (C2's (iii), ruled a slot there and not
    # before). -1 means "use the plan's". It is NOT a way to skip the discipline: 0 is accepted and
    # LOGGED AS A DEPARTURE on every slice boundary, so a run without gaps cannot be quietly mistaken
    # for a run with them.
    [int] $CooldownSecondsOverride = -1,

    # ---------------------------------------------------------------------------------------------
    # ⚠⚠ THE MODE. `sweep` is what this script has always done and stays the DEFAULT, so every
    # existing invocation is unchanged; `rebank` is the H10 re-bank act.
    #
    # THEY ARE NOT INTERCHANGEABLE AND THE RUNBOOK SAYS SO. H10 forbids the sweep wrapper for a
    # re-bank in its own words -- it is "the steady-state gate, enforcing the exact banked count and a
    # drift-clean corpus -- both of which this step invalidates BY DESIGN" -- and the sweep "selects
    # among BANKED rows", so it cannot reach the relocation successors or the unbanked candidates at
    # all. A driver that dispatched the sweep for a re-bank would run, report, and re-bank nothing.
    #
    # ONE SCRIPT, TWO MODES, NOT TWO SCRIPTS (COORD's ruling): the plan reader, the digest gate, the
    # mandatory-parameter refusals, the slice packing and the cooldown are the SAME in both, and they
    # are the half of this file that took the measurements to get right.
    [ValidateSet('sweep', 'rebank')][string] $Mode = 'sweep',

    # ⚠ THE WRAPPER IS TAKEN BY BLOB, NOT BY PATH-IN-A-TREE (the brief's B.8): the caller
    # materialises `src/run-h10-recon.ps1` from a named ref into scratch and passes it here, and this
    # script states its sha256 so the ACK can carry it. The wrapper is NOT on master -- it lives on
    # its own lane ref -- so a driver that looked for it beside itself would find nothing.
    [string] $RebankWrapper,

    # The wrapper's own mandatory inputs, passed straight through. They are NOT defaulted here: a
    # guessed tree or a guessed GOROOT is the silent-wrong-thing failure this file exists to refuse.
    [string] $Tree,
    [string] $GoRoot,
    [string] $Scratch,
    [string] $ExpectTip,

    # ⚠ THE RESUME LEDGER, append-only and idempotent (ruling (10)). A worker that dies mid-shard
    # re-runs this script; rows already recorded for THIS tree state are skipped rather than re-banked.
    [string] $Ledger,

    # Passed through to every row's sweep (`sweep`) or recon wrapper (`rebank`), and ONLY when given:
    # unset, the row script's own default applies and each invocation is what it was before these
    # existed. -TestDeadline is the converted host's per-test deadline (each row still takes the longer
    # of it and its $testDeadlineFloors floor); -TestBuildMaxCpu caps each row's test-host publish at N
    # MSBuild nodes (the converter's -test-build-maxcpu).
    [string] $TestDeadline,
    [ValidateRange(0, [int]::MaxValue)]
    [int] $TestBuildMaxCpu
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

function Deny([string] $message) {
    Write-Host ''
    Write-Host "DISPATCH REFUSED: $message" -ForegroundColor Red
    exit 2
}

$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
if (-not $SweepScript) {
    $SweepScript = Join-Path $scriptDir 'run-validated-sweep.ps1'
}

if (-not (Test-Path -LiteralPath $Plan)) { Deny "no plan at '$Plan'" }

# The row scripts' pass-through arguments, splatted into BOTH calls below and printed in the header,
# so the dry run shows exactly what every row would be handed.
$rowForward = [ordered]@{}
foreach ($name in @('TestDeadline', 'TestBuildMaxCpu')) {
    if ($PSBoundParameters.ContainsKey($name)) { $rowForward[$name] = $PSBoundParameters[$name] }
}

# ⚠⚠ THE EXECUTOR IS RESOLVED WHERE IT IS USED, NOT AT THE TOP. Measured 2026-09-20: a
# `-DryRun` on a box with no tree checked out REFUSED for want of a sweep script it never invokes --
# the loop `continue`s before the call. The arm whose whole purpose is to validate the plan reading,
# the digest and the packing WITHOUT executing anything could not run at all. The refusal was not
# wrong, it was in the wrong place.
# In `rebank` the sweep script is not used at ALL, so requiring it there would be the same mistake
# with a second cause.
if (-not $DryRun -and $Mode -eq 'sweep') {
    if (-not (Test-Path -LiteralPath $SweepScript)) {
        Deny "no sweep script at '$SweepScript' -- pass -SweepScript, or run this from a tree that has one"
    }
}

# ---------------------------------------------------------------- the rebank mode's own inputs
# Each of these refuses BY NAME. A mode that half-configures itself and then fails inside a row is
# the failure this whole file is written against.
if ($Mode -eq 'rebank') {
    if (-not $RebankWrapper) { Deny "-Mode rebank needs -RebankWrapper: the recon wrapper materialised BY BLOB from its lane ref (the brief's B.8). It is not on master and this script does not look for it beside itself." }
    if (-not (Test-Path -LiteralPath $RebankWrapper)) { Deny "no rebank wrapper at '$RebankWrapper'" }
    foreach ($pair in @(@('-Tree', $Tree), @('-GoRoot', $GoRoot), @('-Scratch', $Scratch), @('-ExpectTip', $ExpectTip))) {
        if (-not $pair[1]) { Deny "-Mode rebank needs $($pair[0]) -- it is passed straight through to the wrapper, and a guessed one is the silent-wrong-tree failure this driver refuses" }
    }

    # ⚠⚠ THE TREE GUARD IS THE INVERSE OF THE RECON WRAPPER'S, AND THAT IS THE POINT.
    # A recon tree is DETACHED and thrown away. The driver's tree is a LINKED worktree ON A BRANCH
    # (the brief's B.4 / OQ4) because its artifacts are BANKED and committed from it. The wrapper
    # refuses a branch by default and takes -AllowBranch from here; this end asserts the same fact
    # from the other side, so neither script is trusting the other to have checked.
    # ⚠⚠ GIT'S STDERR IS A TERMINATING ERROR UNDER `Stop`, so these run through a helper that
    # captures the exit code instead of dying on the message. MEASURED on the first cut of this
    # guard: `-Tree x` produced `fatal: cannot change to 'x'` and a NativeCommandError, rc 1, with NO
    # refusal line -- the guard threw four lines before the Deny that would have named the cause.
    # The recon wrapper states the rule and carries the same helper: a guard that DIES is not a guard
    # that REFUSED, and the difference is invisible to anything reading only the exit code.
    function GitQuiet([string[]] $gitArgs) {
        $prev = $ErrorActionPreference
        $ErrorActionPreference = 'Continue'
        try {
            # ⚠ Out-String, NOT a [string] cast: an empty pipeline casts to something whose .Trim()
            # throws InvokeMethodOnNull, which is how the first cut of this helper DIED on a bad tree
            # instead of refusing. This is the recon wrapper's GitTry body verbatim.
            $o = & git @gitArgs 2>$null
            $code = $LASTEXITCODE
            return [pscustomobject]@{ Code = $code; Out = (($o | Out-String).Trim()) }
        } finally { $ErrorActionPreference = $prev }
    }

    # ⚠⚠ THE TREE GUARD RUNS IN A DRY RUN TOO, AND THE FIRST CUT OF THIS SEAT GOT IT BACKWARDS.
    # MEASURED on the i7's Core-edition arm: with this block behind `-not $DryRun`,
    # `-Mode rebank -DryRun -Tree <a MAIN checkout>` returned rc 0, printed "28 rows would run" and
    # wrote a timings file -- so a green dry run read as evidence about a banking tree nobody had
    # checked. C2's design read found the same thing from the other side: an arm that exits above
    # the line under test says nothing about it.
    #
    # THE DISTINCTION IS BETWEEN AN EXECUTOR AND A PRECONDITION, and this seat now makes it in both
    # directions. The SWEEP SCRIPT is an executor a dry run never invokes, so requiring it there
    # refused an arm that executes nothing -- that check is rightly behind `-DryRun`. The TREE is
    # the thing the dry run REPORTS ABOUT: it says these rows would run HERE. A dry run that cannot
    # name a real banking tree is not cheaper, it is unfounded.
    # The guard executes nothing itself -- three `git rev-parse`/`symbolic-ref` reads -- and runs
    # before any plan row, so it costs a dry run only the honesty of naming a real tree.
    $gd  = GitQuiet @('-C', $Tree, 'rev-parse', '--git-dir')
    $gcd = GitQuiet @('-C', $Tree, 'rev-parse', '--git-common-dir')
    if ($gd.Code -ne 0 -or $gcd.Code -ne 0 -or -not $gd.Out -or -not $gcd.Out) {
        Deny "'$Tree' is not a git work tree"
    }
    if ($gd.Out -eq $gcd.Out) {
        Deny "'$Tree' is a MAIN checkout (--git-dir == --git-common-dir), not a linked worktree -- floor 11 is one worktree per shard"
    }
    # ⚠ THE SUCCESS CASE HERE IS git SUCCEEDING: a detached HEAD makes `symbolic-ref -q` exit
    # non-zero with no output, which is the RECON leg's shape and the driver's refusal.
    $sym = GitQuiet @('-C', $Tree, 'symbolic-ref', '-q', 'HEAD')
    if ($sym.Code -ne 0 -or -not $sym.Out) {
        Deny "'$Tree' HEAD is DETACHED -- the driver BANKS, so its tree is on a branch (brief B.4). A detached tree is the RECON leg's shape."
    }
    Write-Host "  tree            : $Tree on $($sym.Out) (linked worktree, banking)"
}
if ($FleetSize -lt 1) { Deny "-FleetSize must be 1 or more (got $FleetSize)" }
if ($OnlySlice -lt 0) { Deny "-OnlySlice must be 0 (all slices) or a slice number (got $OnlySlice)" }

# ---------------------------------------------------------------- read the plan
# ReadAllText, not Get-Content: PS 5.1's Get-Content splits on line endings and hands back an array
# whose CR is already gone, so the CR REPORT below could never be anything but zero. Reading the bytes
# as text keeps the question answerable.
$planText = [System.IO.File]::ReadAllText($Plan)
$crCount = ([regex]::Matches($planText, "`r")).Count

# CRLF is NORMALISED and then REPORTED, rather than refused. The plan is a generated artifact and a
# Windows checkout may legitimately hand it back with CRLF; the digest is computed over LF-joined
# records so it is checkout-invariant. Reporting the count keeps the normalisation from hiding
# anything -- this lane has already paid twice for a CR that a reader silently translated away.
$planLines = $planText -replace "`r`n", "`n" -replace "`r", "`n"
$planLines = $planLines -split "`n"

$meta = @{}
$dataLines = New-Object System.Collections.Generic.List[string]
foreach ($line in $planLines) {
    if ($line.Length -eq 0) { continue }
    if ($line.StartsWith('#')) {
        $parts = $line.Substring(1) -split "`t"
        if ($parts.Count -ge 2) { $meta[$parts[0]] = $parts[1..($parts.Count - 1)] }
        continue
    }
    if ($line.StartsWith("W`tworker`t")) { continue }
    $dataLines.Add($line)
}

foreach ($required in @('version', 'digest', 'rows', 'slice_cap_seconds', 'cooldown_seconds')) {
    if (-not $meta.ContainsKey($required)) {
        Deny "the plan carries no #$required line -- this is not a plan emitted by shardmap.py --emit-plan"
    }
}
if ($meta['version'][0] -ne '1') {
    Deny "plan version '$($meta['version'][0])' is not 1 -- this driver reads version 1"
}

# ---------------------------------------------------------------- the DIGEST gate
# The same principle the coordinator ruled for the generator's INPUT (e0d5121e2 section 5: refuse when
# the parse does not reproduce the declared digest), applied to its OUTPUT. It runs BEFORE any row is
# selected, so a truncated or hand-edited plan cannot dispatch even one package.
$sha = [System.Security.Cryptography.SHA256]::Create()
try {
    $builder = New-Object System.Text.StringBuilder
    foreach ($line in $dataLines) { [void] $builder.Append($line); [void] $builder.Append("`n") }
    $bytes = [System.Text.Encoding]::UTF8.GetBytes($builder.ToString())
    $computed = ($sha.ComputeHash($bytes) | ForEach-Object { $_.ToString('x2') }) -join ''
}
finally {
    $sha.Dispose()
}

$declaredDigest = $meta['digest'][0]
if ($computed -ne $declaredDigest) {
    Deny ("the plan's #digest does not reproduce over its own rows." + [Environment]::NewLine +
          "       declared $declaredDigest" + [Environment]::NewLine +
          "       computed $computed" + [Environment]::NewLine +
          "       $($dataLines.Count) row(s) parsed, #rows says $($meta['rows'][0]). A plan whose digest does not" + [Environment]::NewLine +
          "       reproduce has been truncated or edited, and dispatching from it would run a set nobody derived.")
}

$declaredRows = [int] $meta['rows'][0]
if ($dataLines.Count -ne $declaredRows) {
    Deny "#rows says $declaredRows and $($dataLines.Count) row(s) parsed (the digest matched, so this is a header/body disagreement in the generator, not a damaged file)"
}

# ---------------------------------------------------------------- parse the rows
$allRows = New-Object System.Collections.Generic.List[PSCustomObject]
foreach ($line in $dataLines) {
    $f = $line -split "`t"
    if ($f.Count -ne 7) {
        Deny "a plan row has $($f.Count) tab-separated field(s), expected 7: '$line'"
    }
    $allRows.Add([PSCustomObject]@{
        W        = [int] $f[0]
        Worker   = $f[1]
        Slice    = [int] $f[2]
        Seq      = [int] $f[3]
        Package  = $f[4]
        Cost     = [int] $f[5]
        Reserved = ($f[6] -eq '1')
    })
}

# ---------------------------------------------------------------- select, and refuse informatively
# Each refusal NAMES WHAT IS AVAILABLE. A bare "no rows" sends the reader to re-read the plan by eye,
# which is exactly the manual step the digest and the required -FleetSize exist to remove.
$atFleet = @($allRows | Where-Object { $_.W -eq $FleetSize })
if ($atFleet.Count -eq 0) {
    $sizes = (($allRows | ForEach-Object { $_.W } | Sort-Object -Unique) -join ', ')
    Deny "the plan carries no rows at -FleetSize $FleetSize. Fleet sizes present: $sizes"
}

$mine = @($atFleet | Where-Object { $_.Worker -eq $Worker })
if ($mine.Count -eq 0) {
    $workers = (($atFleet | ForEach-Object { $_.Worker } | Sort-Object -Unique) | ForEach-Object { "'$_'" }) -join ', '
    Deny ("no worker '$Worker' at -FleetSize $FleetSize. Workers present at that size: $workers" +
          [Environment]::NewLine +
          "       (the name must match the plan EXACTLY, parentheses and spaces included -- quote it)")
}

$sliceNumbers = @($mine | ForEach-Object { $_.Slice } | Sort-Object -Unique)
if ($OnlySlice -gt 0) {
    if ($sliceNumbers -notcontains $OnlySlice) {
        Deny "worker '$Worker' has no slice $OnlySlice at -FleetSize $FleetSize. Slices present: $($sliceNumbers -join ', ')"
    }
    $sliceNumbers = @($OnlySlice)
}

$cooldownSeconds = [int] $meta['cooldown_seconds'][0]
$cooldownSource = 'the plan'
if ($CooldownSecondsOverride -ge 0) {
    $cooldownSeconds = $CooldownSecondsOverride
    $cooldownSource = 'the -CooldownSecondsOverride flag -- A DEPARTURE FROM THE RULED GAP'
}
$sliceCapSeconds = [int] $meta['slice_cap_seconds'][0]

if (-not $TimingOut) {
    $TimingOut = Join-Path (Split-Path -Parent (Resolve-Path -LiteralPath $Plan)) 'h10-dispatch-timings.tsv'
}

# ---------------------------------------------------------------- announce what will happen
$modeWord = 'DISPATCH'
if ($DryRun) { $modeWord = 'DRY RUN -- nothing is executed' }

Write-Host ''
Write-Host "H10 dispatch driver -- $modeWord"
Write-Host "  plan            : $Plan"
if ($meta.ContainsKey('block')) {
    Write-Host "  measurement     : $($meta['block'] -join ' / ')"
}
if ($meta.ContainsKey('projection')) {
    # Printed on EVERY run, not once in a doc: section 3.2's rule is "say which it is, and gate
    # dispatch on it", and an operator reading a wall clock against this plan must know the costs are
    # a lower bound before they conclude the box is slow.
    Write-Host "  !! projection   : $($meta['projection'] -join ' -- ')"
}
Write-Host "  digest          : $declaredDigest (reproduced over $($dataLines.Count) row(s))"
Write-Host "  CR bytes in plan: $crCount (normalised before digesting; reported so the normalisation hides nothing)"
Write-Host "  worker          : $Worker   at W = $FleetSize"
Write-Host "  slice cap       : $sliceCapSeconds s ($([math]::Round($sliceCapSeconds / 60.0, 1)) min), from the plan"
Write-Host "  cooldown        : $cooldownSeconds s ($([math]::Round($cooldownSeconds / 60.0, 1)) min) BETWEEN slices, from $cooldownSource"
Write-Host "  sweep           : $SweepScript"
if ($rowForward.Count -gt 0) {
    Write-Host "  row arguments   : $(@($rowForward.Keys | ForEach-Object { "-$_ $($rowForward[$_])" }) -join ' ')"
} else {
    Write-Host '  row arguments   : none forwarded -- each row script uses its own defaults'
}
Write-Host "  timings         : $TimingOut"

$plannedCost = ($mine | Where-Object { $sliceNumbers -contains $_.Slice } | Measure-Object -Property Cost -Sum).Sum
$plannedCount = @($mine | Where-Object { $sliceNumbers -contains $_.Slice }).Count
$gapCount = [math]::Max(0, $sliceNumbers.Count - 1)
Write-Host ''
Write-Host ("  $plannedCount row(s) over $($sliceNumbers.Count) slice(s) [$($sliceNumbers -join ', ')], " +
            "$plannedCost i9-s of measured cost, $gapCount cooldown gap(s) = " +
            "$([math]::Round(($plannedCost + $gapCount * $cooldownSeconds) / 60.0, 1)) min including gaps")

# ---------------------------------------------------------------- the rebank preamble
# Everything here is computed ONCE per run, not per row: the wrapper's identity for the ACK, the
# tree state the ledger keys on, and the ledger's own already-done set.
$rebankSha = ''
$corpusCommit = ''
$converterStamp = ''
$ledgerDone = New-Object 'System.Collections.Generic.HashSet[string]' ([System.StringComparer]::Ordinal)

if ($Mode -eq 'rebank' -and -not $DryRun) {
    # ⚠ THE WRAPPER'S sha256 IS STATED, because the brief's B.8 says the ACK carries it and because
    # "the wrapper" is not a stable name -- it is whatever blob the caller materialised.
    # ⚠⚠ TWO IDENTITIES, EACH LABELLED WITH WHAT IT HASHED (COORD 3e3d57a497).
    # MEASURED: the recon wrapper reads sha256 55466ef4d8... as the repository stores it (LF) and
    # aa4664c275... as a Windows checkout holds it (CRLF) -- 1,229 line endings apart, one file.
    # An ACK carrying only the second would disagree with any worker that materialised the wrapper
    # by `git show`, and the disagreement looks exactly like a tampered blob.
    #
    # ⚠ `git hash-object` IS NOT THE ANSWER ON ITS OWN: outside a repository no attributes apply,
    # so it returns the id of whatever bytes it is handed -- the CRLF ones for a checkout copy. The
    # blob id below is computed over the LF-NORMALISED content with git's own formula, so it is the
    # REPOSITORY's id for either representation, which is the whole point of printing it.
    $wrapperBytes = [System.IO.File]::ReadAllBytes($RebankWrapper)
    $rebankSha = ([System.BitConverter]::ToString(
        [System.Security.Cryptography.SHA256]::Create().ComputeHash($wrapperBytes))
        ).Replace('-', '').ToLowerInvariant()

    # LF-normalise: drop every CR that immediately precedes an LF, and nothing else.
    $lf = New-Object 'System.Collections.Generic.List[byte]'
    for ($bi = 0; $bi -lt $wrapperBytes.Length; $bi++) {
        if ($wrapperBytes[$bi] -eq 13 -and ($bi + 1) -lt $wrapperBytes.Length -and $wrapperBytes[$bi + 1] -eq 10) { continue }
        $lf.Add($wrapperBytes[$bi])
    }
    $lfBytes = $lf.ToArray()
    $header  = [System.Text.Encoding]::ASCII.GetBytes("blob $($lfBytes.Length)" + [char] 0)
    $blobIn  = New-Object 'byte[]' ($header.Length + $lfBytes.Length)
    [System.Array]::Copy($header, 0, $blobIn, 0, $header.Length)
    [System.Array]::Copy($lfBytes, 0, $blobIn, $header.Length, $lfBytes.Length)
    $rebankBlob = ([System.BitConverter]::ToString(
        [System.Security.Cryptography.SHA1]::Create().ComputeHash($blobIn))
        ).Replace('-', '').ToLowerInvariant()

    $wrapperCR = @($wrapperBytes | Where-Object { $_ -eq 13 }).Count
    Write-Host "  wrapper         : $RebankWrapper"
    Write-Host "  wrapper blob    : $rebankBlob   (git blob id of the LF-normalised content -- the ACK's number)"
    Write-Host "  wrapper sha256  : $rebankSha   (of the file AS READ; $wrapperCR CR byte(s))"

    $hc = GitQuiet @('-C', $Tree, 'rev-parse', 'HEAD')
    if ($hc.Code -ne 0 -or -not $hc.Out) { Deny "could not read HEAD in '$Tree'" }
    $corpusCommit = $hc.Out
    if ($corpusCommit -ne $ExpectTip) {
        Deny "'$Tree' is at $corpusCommit, not the -ExpectTip $ExpectTip -- a shard's rows are comparable only across one tree"
    }

    # ⚠⚠ THE CONVERTER IS PART OF THE LEDGER KEY, and its MTIME is the honest predicate.
    # A row banked by one converter is not the same act as the same row banked by another, so a
    # resume that skipped rows across a rebuild would be claiming work it did not do. mtime means
    # "was this written" for a build output, which is the question here.
    $conv = $null
    foreach ($c in @((Join-Path $Tree 'src/go2cs/go2cs.exe'), (Join-Path $Tree 'src/go2cs/go2cs'))) {
        if (Test-Path -LiteralPath $c) { $conv = Get-Item -LiteralPath $c; break }
    }
    if (-not $conv) { Deny "no converter binary under '$Tree/src/go2cs' -- build it at this tree before dispatching" }
    # ⚠ AND ITS FAILURE DIRECTION IS THE SAFE ONE, which the sentence above does not say (C2).
    # This is mtime AND size, not content: a rebuild, a touch or a checkout changes mtime, so the
    # row RE-RUNS when it need not. A wrong SKIP needs two different converters agreeing on both
    # mtime-to-the-tick and byte length -- reachable by a timestamp-preserving copy (`cp -p`, a
    # restore from archive), never by a build.
    $converterStamp = "$($conv.LastWriteTimeUtc.ToString('o'))/$($conv.Length)"
    Write-Host "  converter       : $($conv.FullName) ($($conv.Length) bytes, $($conv.LastWriteTimeUtc.ToString('o')))"

    # ⚠ THE LEDGER IS APPEND-ONLY AND IDEMPOTENT (ruling (10)). A worker that dies mid-shard
    # re-runs this script; a row already recorded UNDER THIS EXACT TREE STATE is skipped rather than
    # re-banked. The key carries the corpus commit AND the converter stamp precisely so a resume
    # after a rebuild or a tip move re-runs everything instead of silently trusting a stale row.
    if ($Ledger) {
        if (Test-Path -LiteralPath $Ledger) {
            foreach ($line in [System.IO.File]::ReadAllLines($Ledger)) {
                if (-not $line -or $line.StartsWith('#')) { continue }
                $f = $line -split "`t"
                if ($f.Count -ge 4) { $null = $ledgerDone.Add(($f[1] + '|' + $f[2] + '|' + $f[3])) }
            }
            Write-Host "  ledger          : $Ledger ($($ledgerDone.Count) row(s) already recorded for some tree state)"
        } else {
            [System.IO.File]::WriteAllText($Ledger, "# h10 rebank ledger -- APPEND ONLY. utc`trow`tcorpus_commit`tconverter_stamp`tword`tbanked`n")
            Write-Host "  ledger          : $Ledger (created)"
        }
    }
}

# ---------------------------------------------------------------- run
$timings = New-Object System.Collections.Generic.List[string]
# ⚠ TWO HEADERS, BY MODE, and each is read BY NAME downstream rather than by position.
# The rebank header is the recon leg's eleven columns (the floor) plus the four the dispatcher knows
# and the two the ruling adds -- `banked` and `manifest_pins`.
if ($Mode -eq 'rebank') {
    $timings.Add("w`tworker`tslice`tseq`trow`tword`tverdicts`tsweep_s`tfirst_in_list`trc`tdiverged`tplatform`ttree`twall_s`tpost_s`tbanked`tmanifest_pins`tcost_i9_s`treserved")
} else {
    $timings.Add("w`tworker`tslice`tseq`tpackage`tcost_i9_s`treserved`twall_s`texit")
}
$failures = New-Object System.Collections.Generic.List[string]
$rowsRun = 0
$sliceOrdinal = 0

foreach ($sliceNumber in $sliceNumbers) {
    $sliceOrdinal++
    $sliceRows = @($mine | Where-Object { $_.Slice -eq $sliceNumber } | Sort-Object Seq)
    $sliceCost = ($sliceRows | Measure-Object -Property Cost -Sum).Sum

    Write-Host ''
    Write-Host ("=== slice $sliceNumber of [$($sliceNumbers -join ', ')] :: $($sliceRows.Count) row(s), " +
                "$sliceCost i9-s projected ===")

    foreach ($row in $sliceRows) {
        $marker = ''
        if ($row.Reserved) { $marker = ' [reserved]' }
        Write-Host ("  -> {0,-40} {1,6} i9-s{2}" -f $row.Package, $row.Cost, $marker)

        if ($DryRun) {
            # ⚠ THE DRY ROW MUST CARRY THE SAME COLUMN COUNT AS ITS HEADER. A short row under a
            # wide header is read BY NAME downstream and silently yields empty fields, which is the
            # empty-column class this fleet has banked twice.
            if ($Mode -eq 'rebank') {
                $timings.Add("$($row.W)`t$($row.Worker)`t$($row.Slice)`t$($row.Seq)`t$($row.Package)`tDRYRUN`t`t`t`t`t`t`t`t`t`t`t`t$($row.Cost)`t$(if ($row.Reserved) { 1 } else { 0 })")
            } else {
                $timings.Add("$($row.W)`t$($row.Worker)`t$($row.Slice)`t$($row.Seq)`t$($row.Package)`t$($row.Cost)`t$(if ($row.Reserved) { 1 } else { 0 })`t`tDRYRUN")
            }
            continue
        }

        $reservedFlag = 0
        if ($row.Reserved) { $reservedFlag = 1 }
        $safe = ($row.Package -replace '[\\/]', '__')

        if ($Mode -eq 'rebank') {
            # ⚠⚠ THE LEDGER SKIP IS KEYED ON THE TREE STATE, NOT ON THE ROW NAME.
            # "This row is done" is only true of the corpus commit and the converter that did it, so
            # a resume after a rebuild or a tip move re-runs rather than trusting a stale record.
            # ⚠ THIS LINE IS UNREACHABLE IN A DRY RUN BY ORDERING, NOT BY A GUARD (C2's read).
            # The preamble that fills $corpusCommit and $converterStamp is itself behind
            # `-not $DryRun`, so in a dry run they are EMPTY and this key would be "Package||" --
            # harmless only because the dry-run branch `continue`s above here. An edit that moved
            # the dry-run emission below this line would collide every dry-run row on one key.
            $ledgerKey = "$($row.Package)|$corpusCommit|$converterStamp"
            if ($Ledger -and $ledgerDone.Contains($ledgerKey)) {
                Write-Host "     already recorded for this tree state -- SKIPPED (resume)" -ForegroundColor DarkGray
                $timings.Add("$($row.W)`t$($row.Worker)`t$($row.Slice)`t$($row.Seq)`t$($row.Package)`tRESUMED`t`t`t`t`t`t`t`t`t`t`t`t$($row.Cost)`t$reservedFlag")
                continue
            }

            $rowList = Join-Path $Scratch "rebank-$safe.namelist"
            $rowOut  = Join-Path $Scratch "rebank-$safe.tsv"
            # LF, and one row: the wrapper reads a name list, and this driver's unit of dispatch is
            # a package (a row is INDIVISIBLE, as this file's own header says).
            [System.IO.File]::WriteAllText($rowList, $row.Package + "`n")

            $started = Get-Date
            # ⚠ ONE IMPLEMENTATION OF THE PIPELINE, INVOKED -- not a copy. The wrapper carries the
            # go2cs invocation, the word classifier, the ordinal readers, the staleness gate and the
            # evidence capture; a second spelling of those 507 lines here would drift from it.
            # -AllowBranch: the driver's tree is a BANKING tree and the wrapper refuses a branch by
            # default. Both ends assert that fact rather than trusting the other to have checked.
            & $RebankWrapper -NameList $rowList -Tree $Tree -GoRoot $GoRoot -Out $rowOut `
                             -ExpectTip $ExpectTip -Scratch $Scratch -AllowBranch @rowForward
            # CAPTURED IMMEDIATELY, before anything touches $? or a pipe. Floor 7.
            $rowExit = $LASTEXITCODE
            $wall = [int] ((Get-Date) - $started).TotalSeconds
            $rowsRun++

            # ⚠ THE WRAPPER'S OWN ROW IS READ BY NAME, NEVER BY POSITION -- the ruled rule for this
            # TSV, and the reason the eleven columns are a floor rather than a layout.
            $w = @{}
            if (Test-Path -LiteralPath $rowOut) {
                $wl = @([System.IO.File]::ReadAllLines($rowOut) | Where-Object { $_ })
                if ($wl.Count -ge 2) {
                    $wh = $wl[0] -split "`t"
                    $wv = $wl[1] -split "`t"
                    for ($k = 0; $k -lt $wh.Count; $k++) {
                        if ($k -lt $wv.Count) { $w[$wh[$k]] = $wv[$k] } else { $w[$wh[$k]] = '' }
                    }
                }
            }
            # ⚠ A MISSING EMISSION IS ITS OWN WORD, not a blank row. The wrapper writes LF-only and
            # refuses on CR; if nothing arrived, the driver says so rather than banking empty fields.
            $word = 'NOEMIT'
            if ($w.ContainsKey('word') -and $w['word']) { $word = $w['word'] }

            # ⚠ `banked` IS DERIVED FROM THE WORD AND SAYS ONLY WHAT THIS DRIVER CAN KNOW.
            # PASS and DIVERGED are the two words a row reaches by producing a comparison, which is
            # the act that writes the five artifacts. Every other word means no artifacts to bank.
            # `debt` is NOT derived here: a row whose artifacts exist but whose format gate refuses is
            # a classification made at BANKING, by the gate, not by the dispatcher -- and deriving it
            # from a word this script never checks would be a guess wearing a column.
            $banked = 'no'
            if ($word -eq 'PASS' -or $word -eq 'DIVERGED') { $banked = 'yes' }

            # ⚠ THE PIN COUNT IS READ FROM THE MANIFEST THE ROW JUST RE-SIGNED, not from the word.
            # An absent manifest is 0 and an unreadable one is `n/a` -- never 0, because "no pins" and
            # "could not tell" are different facts and a 0 for the second is the empty-counter class.
            $pins = '0'
            $manifest = Join-Path $Tree "src/core/$($row.Package)/go2cs_test_disclosures.json"
            if (Test-Path -LiteralPath $manifest) {
                try {
                    $mj = Get-Content -LiteralPath $manifest -Raw | ConvertFrom-Json
                    if ($mj.PSObject.Properties.Name -contains 'disclosures') { $pins = [string] @($mj.disclosures).Count }
                    else { $pins = 'n/a' }
                } catch { $pins = 'n/a' }
            }

            $timings.Add("$($row.W)`t$($row.Worker)`t$($row.Slice)`t$($row.Seq)`t$($row.Package)`t$word`t$($w['verdicts'])`t$($w['sweep_s'])`t$($w['first_in_list'])`t$($w['rc'])`t$($w['diverged'])`t$($w['platform'])`t$($w['tree'])`t$wall`t$($w['post_s'])`t$banked`t$pins`t$($row.Cost)`t$reservedFlag")

            if ($Ledger) {
                # APPEND ONLY. The ledger is a record of what was done, so it is never rewritten and
                # never pruned by this script.
                $stamp = (Get-Date).ToUniversalTime().ToString('o')
                [System.IO.File]::AppendAllText($Ledger, "$stamp`t$($row.Package)`t$corpusCommit`t$converterStamp`t$word`t$banked`n")
            }
            Write-Host ("     {0}  verdicts={1}  banked={2}  pins={3}  {4}s  rc={5}" -f `
                $word, $(if ($w['verdicts']) { $w['verdicts'] } else { '-' }), $banked, $pins, $wall, $rowExit)
        }
        else {
            $started = Get-Date
            # -Filter <pkg> -Exact is the sweep's own documented per-package campaign interface: exact path
            # match, because substring 'io' sweeps bufio and io/fs alongside io and a per-row driver would
            # re-sweep large rows repeatedly.
            & $SweepScript -Filter $row.Package -Exact -SkipBuild:($rowsRun -gt 0) @rowForward
            # CAPTURED IMMEDIATELY, before anything touches $? or a pipe. Floor 7, and the reason
            # safe-push.sh exists.
            $rowExit = $LASTEXITCODE
            $wall = [int] ((Get-Date) - $started).TotalSeconds
            $rowsRun++

            $timings.Add("$($row.W)`t$($row.Worker)`t$($row.Slice)`t$($row.Seq)`t$($row.Package)`t$($row.Cost)`t$reservedFlag`t$wall`t$rowExit")
        }

        if ($rowExit -ne 0) {
            $failures.Add("$($row.Package) (slice $($row.Slice), exit $rowExit)")
            Write-Host "     row exit $rowExit after $wall s" -ForegroundColor Yellow
        }
        else {
            Write-Host "     ok, $wall s"
        }
    }

    # The gap goes AFTER a slice and never after the last one -- a trailing sleep delays a result and
    # protects nothing. Written as an explicit ordinal test rather than "if not last", because the
    # off-by-one here costs ten minutes of every run and would never look wrong.
    if ($sliceOrdinal -lt $sliceNumbers.Count) {
        if ($cooldownSeconds -le 0) {
            Write-Host ''
            Write-Host ("  !! NO COOLDOWN between slice $sliceNumber and the next: the gap is $cooldownSeconds s from " +
                        "$cooldownSource. The ruled discipline is a ten-minute gap; this run is a DEPARTURE and " +
                        "its readings are not comparable with a gapped run.") -ForegroundColor Yellow
        }
        elseif ($DryRun) {
            Write-Host ''
            Write-Host "  (cooldown $cooldownSeconds s would run here)"
        }
        else {
            Write-Host ''
            Write-Host "  cooldown $cooldownSeconds s before slice $($sliceNumbers[$sliceOrdinal])..."
            Start-Sleep -Seconds $cooldownSeconds
        }
    }
}

# ---------------------------------------------------------------- the verdict, derived
[System.IO.File]::WriteAllText($TimingOut, (($timings -join "`n") + "`n"))
Write-Host ''
Write-Host "timings written: $TimingOut ($($timings.Count - 1) row(s))"

if ($DryRun) {
    Write-Host ''
    Write-Host "DRY RUN COMPLETE -- $plannedCount row(s) would run over $($sliceNumbers.Count) slice(s) with $gapCount gap(s). Nothing was executed."
    exit 0
}

# Derived from the count and exited on -- never a hardcoded verdict line. A confident parenthetical
# over a count that falsifies it is a check that cannot go red, and this repository has written one.
if ($failures.Count -gt 0) {
    Write-Host ''
    Write-Host "DISPATCH FAILED: $($failures.Count) of $rowsRun row(s) exited non-zero" -ForegroundColor Red
    foreach ($f in $failures) { Write-Host "  $f" }
    exit 1
}

Write-Host ''
Write-Host "DISPATCH CLEAN: $rowsRun of $rowsRun row(s) exit 0 over $($sliceNumbers.Count) slice(s)" -ForegroundColor Green
exit 0
