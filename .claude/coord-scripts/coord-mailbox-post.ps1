# coord-mailbox-post.ps1 -- delivery-verified append to the fleet mailbox (claude/mailbox).
# Usage: powershell -NoProfile -ExecutionPolicy Bypass -File coord-mailbox-post.ps1 -EntryFile <path> -Message <commit subject> -LastRead <sha>
# Prints the ABSORBED range (LastRead..pre-append tip) so the own-push blind window announces itself: READ it if non-empty.
# 2026-09-03: the commit message goes through -F (a temp file) -- PS 5.1 splits a native argument on embedded double quotes, which turned
# one message into a bad pathspec, failed the commit, and still printed DELIVERED=True (local == remote when nothing was committed).
# The commit is now asserted to MOVE HEAD (exit 3 otherwise), so a failed commit can no longer read as a delivery.
# 2026-09-13: bounded 3-attempt retry -- fetch+reset+append+guard+commit+push per attempt; interleaved commits printed per attempt; -ClonePath for hermetic tests.
# 2026-09-13 (review): delivery is judged by CONTAINMENT, not equality -- a remote that moved between our accepted push and the
# ls-remote used to read as NOT DELIVERED, and under the retry that re-appended and posted the entry TWICE at exit 0. Also:
# the read anchor is keyed to the clone it describes, an unresolvable read range refuses instead of reading as empty, the
# entry bytes the guards scanned are the bytes appended, the exit-3 rollback actually rolls back, and -MainCheckout is plumbed.
param(
    [Parameter(Mandatory)] [string] $EntryFile,
    [Parameter(Mandatory)] [string] $Message,
    [Parameter(Mandatory)] [string] $LastRead,
    [switch] $DryRun,
    [string] $ClonePath = 'C:\Projects\go2cs-mailbox-coord',
    [string] $MainCheckout = 'C:\Projects\go2cs'
)
# KEEP IN SYNC with the -ClonePath default above. A param default cannot reference a variable (param must be the
# first statement), and the anchor rule below has to be able to ask "is this the coordinator's own clone?".
$defaultClonePath = 'C:\Projects\go2cs-mailbox-coord'
# ENTRY-FILE EXISTENCE (2026-09-07): moved ABOVE every guard. It used to sit BELOW them all, after the
# dry-run exit, and the consequence was measured before this move: `-DryRun` on a TYPO'D path exited 0
# printing "guards passed, nothing posted". The census reported "0 arm(s) fired / 5 checked" with a
# SKIPPED line, and the placeholder guard's Select-String errored ObjectNotFound and FELL THROUGH as
# clean -- a guard that cannot read its input PASSES it, which is CLAUDE.md false-green route #6 sitting
# inside the tool whose whole job is to refuse. The exit code and the wording are unchanged; only the
# ORDER moved, and it now refuses in dry-run and real mode alike, before a single guard runs.
if (-not (Test-Path $EntryFile)) { "ENTRY FILE MISSING: $EntryFile"; exit 1 }
# THE ONE READ. Every guard below scans THIS variable and none performs its own I/O, because an I/O
# failure inside a guard is indistinguishable from a clean scan -- exactly how the placeholder arm
# failed open above, and how the branch guard's own `if (Test-Path ...)` would have censused the
# SUBJECT ALONE while reporting normally. A read that throws here (locked file, ACL) terminates
# non-zero: fail CLOSED. It also pins what the guards scanned to one snapshot of the file.
$entryText = [System.IO.File]::ReadAllText($EntryFile)
# PLACEHOLDER-ABORT (2026-09-05, after C2's GATELINES post): an entry still carrying an <ANGLE_TOKEN> placeholder is refused before any git step.
if ($entryText -cmatch '<[A-Z0-9_]{3,}>') { Write-Host 'REFUSED: entry file carries an unfilled <PLACEHOLDER> token'; exit 3 }
$ErrorActionPreference = 'Continue'  # moved ABOVE the guards (2026-09-07): they run native git, and a caller at 'Stop' dies on the first stderr line
# IDENTIFIER CENSUS (2026-09-07) -- EXIT-GATED at exit 8, BEFORE any git step.
#
# The owner's standing security order (CLAUDE.md, "Conventions -- SECURITY", 2026-09-01) keeps real
# hostnames, UNC/share names, account/user names, profile paths and domains off EVERY pushed surface
# -- commits, mailbox entries and branch names alike; fleet machines are named ONLY by nickname.
# Doctrine: "The pre-post census runs before EVERY push -- diff-scoped and EXIT-GATED, or it is
# decoration." A census whose exit code does not gate the push is a guard built and not armed.
#
# BOTH SURFACES, censused SEPARATELY. This tool writes the entry file into MAILBOX.md *and* writes
# -Message as the commit SUBJECT (the -F temp file below), and on 2026-09-07 a lane found its own
# census read the BODY and never the COMMIT MESSAGE. A refusal therefore NAMES which surface carried
# the hit, because "the post is clean" and "the subject is clean" are two different claims.
#
# It never echoes an identifier. A guard that spelled what it forbids would put it on the pushed
# surface itself, which is the thing being prevented, so every reported line is MASKED -- the
# offending token first, then a defensive second pass over any profile segment, home segment or UNC
# host still standing.
#
# Arm (d) re-implements the DENIED-TOKEN pass of src/go2cs/fleetIdentifierCensus_test.go faithfully:
# salt-free SHA-256 of the LOWERCASED token, hex, indexed by token LENGTH (length carried WITH the
# hash in one record, as that file insists, so half an entry cannot be added), tested against the
# whole identifier run AND against each dot/hyphen component -- which is what makes a machine name
# and the account name inside it both match. Reproduction was verified two ways before this landed:
# against the NIST sha256("abc") vector, and by hashing this box's own $env:USERNAME and watching it
# equal that file's 7-length "fleet account name" entry.
#
# Helper names are Get-Alias-checked. A function named H or LP is SHADOWED by a built-in
# (Get-History, Out-Printer) and is never called -- the trap that once made a "long-path-safe"
# comparer return $null for every path so null-vs-null read as a clean True. The first draft of this
# arm used `H` and died on exactly that. idcHashTok / idcMaskLine / idcIsPlaceholder are alias-free.
function idcHashTok([string]$s) {
    $sha = [System.Security.Cryptography.SHA256]::Create()
    try { ($sha.ComputeHash([System.Text.Encoding]::UTF8.GetBytes($s.ToLowerInvariant())) | ForEach-Object { $_.ToString('x2') }) -join '' }
    finally { $sha.Dispose() }
}
# A profile/home segment that is a redaction or a generic stand-in is NOT an identifier. The sigil
# test covers <user>, %USERPROFILE%, $HOME, {user} and (user); the word list mirrors the tree's own
# fleetPlaceholderSegments so both guards answer the same question the same way.
function idcIsPlaceholder([string]$seg) {
    if ($null -eq $seg -or $seg.Length -lt 2) { return $true }
    if ('<%${[('.Contains($seg.Substring(0,1))) { return $true }
    return (@('user','users','username','user-name','youruser','profile','profile-root','home','root',
              'name','account','host','hostname','server','share','machine','public','default',
              'programdata','userprofile','unc','...','foo','bar','baz','example','redacted',
              'placeholder','go','gopher','runner','agent','ubuntu','vagrant','administrator',
              'admin','ci','build','dev') -contains $seg.ToLowerInvariant())
}
# [regex]::Replace with an IgnoreCase flag, never String.Replace(s,s,StringComparison): that overload
# is .NET Core only and PS 5.1 -- the edition that RUNS this script -- would not bind it.
function idcMaskLine([string]$line, [string[]]$lits) {
    $m = $line
    # LONGEST LITERAL FIRST. Masking the account before the machine name leaves the machine's SUFFIX
    # standing -- a 12-character hostname rendered as "<*REDACTED-7*>-HOME", which discloses exactly
    # the infrastructure detail the order forbids. Measured on this arm's own (e) control before it
    # landed: a contained literal must never be masked ahead of the literal that contains it.
    foreach ($l in ($lits | Where-Object { $_ } | Sort-Object -Property Length -Descending)) {
        if ($l -and $l.Length -ge 2) { $m = [regex]::Replace($m, [regex]::Escape($l), ('<*REDACTED-' + $l.Length + '*>'), 'IgnoreCase') }
    }
    $m = [regex]::Replace($m, '(?i)(users[\\/]+)([^\\/\s"''`,;:)\]}>*|]{2,})', '$1<*REDACTED*>')
    $m = [regex]::Replace($m, '(?i)(/home/)([^\\/\s"''`,;:)\]}>*|]{2,})', '$1<*REDACTED*>')
    $m = [regex]::Replace($m, '([\\][\\])([A-Za-z][A-Za-z0-9._-]+)', '$1<*REDACTED*>')
    if ($m.Length -gt 160) { $m = $m.Substring(0,160) + ' ...' }
    return ($m.Trim())
}
# The denylist, copied hash-for-hash from src/go2cs/fleetIdentifierCensus_test.go. Plaintext appears
# nowhere: adding one spells nothing --  echo -n "<token>" | tr A-Z a-z | sha256sum
$idcDenied = @(
    @{ Len =  7; Hash = '20befabea93592064aad4d07e1af70c5d6859667e1edffdb591accf60e2993ee'; What = 'fleet account name' },
    @{ Len =  8; Hash = 'deff430814c33ac000dbdf4bd1061321b8387df004594375c947fabf73d3acc1'; What = 'fleet account name' },
    @{ Len = 13; Hash = '1070b0f89514d6852350c53ac7682edcb2d41f38d66fb95c340cdec08802c74e'; What = 'fleet machine name' }
)
$idcDenIdx = @{}
foreach ($d in $idcDenied) { if (-not $idcDenIdx.ContainsKey($d.Len)) { $idcDenIdx[$d.Len] = @{} }; $idcDenIdx[$d.Len][$d.Hash] = $d.What }
# DERIVED at run time, never spelled: the script must not carry the literals it forbids.
$idcAcct = $env:USERNAME
$idcMach = $env:COMPUTERNAME
# The ONE exception to the account arm: the PUBLIC repository URL. The owner's GitHub handle is
# published attribution, not infrastructure, and it is legitimately present at master -- the tree's
# own guard clears it by file for the same reason. Removed as an exact substring BEFORE arm (c) runs,
# so a deep blob link carrying it is cleared too while a bare handle elsewhere is still refused.
$idcPublicUrl = 'github.com/ritchiecarroll/go2cs'
$idcProfileRe = '(?i)(?:users[\\/]+|/home/)([^\\/\s"''`,;:)\]}>*|]+)'
# A hand-rolled lookbehind, as the tree's guard uses: a UNC prefix BEGINS a path token, so it must sit
# at line start or after whitespace, a quote or an opening delimiter. Without it every ESCAPED
# backslash in prose or in a C#/Go literal reads as a UNC prefix. Group 3 is the host. The class is
# bracketed ([\\]) rather than written as a run of escapes, because a backslash run is exactly what
# collapses on the way through a shell, a heredoc or a -replace pattern.
$idcUncRe = '(^|[\s"''`(\[=,])([\\][\\])([A-Za-z][A-Za-z0-9._-]+)[\\]'
$idcSurfaces = @()
$idcSurfaces += ,@('subject', [string]$Message)
# The ONE read above, not a second one here. The entry file is GUARANTEED present -- the existence check
# now precedes every guard -- so this surface can no longer be skipped, and the SKIPPED line that used to
# announce it ("file unreadable; the ENTRY FILE MISSING check below refuses it") is gone with the case it
# described. That line was the census honestly reporting that it had censused half its surface; the
# refusal it deferred to ran AFTER the dry-run exit and therefore never ran at all under -DryRun.
$idcSurfaces += ,@('entry file', $entryText)
$idcHits = @(); $idcChecked = 0; $idcSkipped = @()
if (-not $idcAcct -or $idcAcct.Length -lt 4) { $idcSkipped += 'class (c) account literal -- $env:USERNAME is empty or under 4 chars, too short to match on without refusing everything' }
if (-not $idcMach -or $idcMach.Length -lt 4) { $idcSkipped += 'class (e) machine name -- $env:COMPUTERNAME is empty or under 4 chars, too short to match on without refusing everything' }
foreach ($idcS in $idcSurfaces) {
    $idcName = $idcS[0]; $idcText = [string]$idcS[1]
    $idcLines = $idcText -split '\r?\n'
    # Five classes, five INDEPENDENT arms per surface, each counted whether or not it fires.
    $idcFired = @{ a = $false; b = $false; c = $false; d = $false; e = $false }
    for ($i = 0; $i -lt $idcLines.Count; $i++) {
        $idcLine = $idcLines[$i]; $idcNo = $i + 1
        if ($idcLine.Length -eq 0) { continue }
        # (a) profile-root and home paths, in every spelling: C:\Users\x, C:/Users/x, /c/Users/x, /home/x
        foreach ($m in [regex]::Matches($idcLine, $idcProfileRe)) {
            $seg = $m.Groups[1].Value
            if (idcIsPlaceholder $seg) { continue }
            $idcFired.a = $true
            $idcHits += [pscustomobject]@{ Class = '(a) profile/home path'; Surface = $idcName; Line = $idcNo; Masked = (idcMaskLine $idcLine @($seg, $idcAcct, $idcMach)) }
        }
        # (b) UNC / share paths
        foreach ($m in [regex]::Matches($idcLine, $idcUncRe)) {
            $idcFired.b = $true
            $idcHits += [pscustomobject]@{ Class = '(b) UNC/share path'; Surface = $idcName; Line = $idcNo; Masked = (idcMaskLine $idcLine @($m.Groups[3].Value, $idcAcct, $idcMach)) }
        }
        # (c) the account literal on THIS box, derived, with the public repo URL removed first
        if ($idcAcct -and $idcAcct.Length -ge 4) {
            $idcCLine = [regex]::Replace($idcLine, [regex]::Escape($idcPublicUrl), '', 'IgnoreCase')
            if ($idcCLine.ToLowerInvariant().Contains($idcAcct.ToLowerInvariant())) {
                $idcFired.c = $true
                $idcHits += [pscustomobject]@{ Class = '(c) account literal'; Surface = $idcName; Line = $idcNo; Masked = (idcMaskLine $idcLine @($idcAcct, $idcMach)) }
            }
        }
        # (d) the fleet's HASHED denylist -- whole run and each dot/hyphen component
        foreach ($m in [regex]::Matches($idcLine, '[A-Za-z0-9._-]+')) {
            $run = $m.Value
            $cands = @($run) + @($run -split '[-.]' | Where-Object { $_.Length -gt 0 })
            foreach ($t in $cands) {
                if (-not $idcDenIdx.ContainsKey($t.Length)) { continue }
                $what = $idcDenIdx[$t.Length][(idcHashTok $t)]
                if ($what) {
                    $idcFired.d = $true
                    $idcHits += [pscustomobject]@{ Class = "(d) denied token [$what]"; Surface = $idcName; Line = $idcNo; Masked = (idcMaskLine $idcLine @($t, $idcAcct, $idcMach)) }
                    break
                }
            }
        }
        # (e) THIS box's machine name, case-insensitive. Additive to (d): the fleet denylist carries a
        # 13-character machine name that is NOT this box, so without this arm the coordinator's own
        # hostname would be the one identifier the guard could not see.
        if ($idcMach -and $idcMach.Length -ge 4 -and $idcLine.ToLowerInvariant().Contains($idcMach.ToLowerInvariant())) {
            $idcFired.e = $true
            $idcHits += [pscustomobject]@{ Class = '(e) machine name'; Surface = $idcName; Line = $idcNo; Masked = (idcMaskLine $idcLine @($idcMach, $idcAcct)) }
        }
    }
    $idcChecked += 5
    if (-not $idcAcct -or $idcAcct.Length -lt 4) { $idcChecked-- }
    if (-not $idcMach -or $idcMach.Length -lt 4) { $idcChecked-- }
}
# Printed UNCONDITIONALLY, so "found nothing" is never confusable with "never fired": a checked count
# of 0 is itself a refusal-worthy reading, and the SKIPPED lines name any arm that could not run.
$idcClasses = @($idcHits | ForEach-Object { $_.Class + ' in the ' + $_.Surface } | Sort-Object -Unique)
"IDENTIFIER CENSUS: $($idcClasses.Count) arm(s) fired / $idcChecked checked (5 classes x $($idcSurfaces.Count) surface(s))"
$idcSkipped | ForEach-Object { "  SKIPPED: $_" }
if ($idcHits.Count -gt 0) {
    "REFUSED: the entry or the subject carries an infrastructure identifier -- the owner's standing security order keeps these off every pushed surface:"
    foreach ($c in $idcClasses) { "  $c" }
    foreach ($h in ($idcHits | Group-Object { $_.Class + '|' + $_.Surface } | ForEach-Object { $_.Group[0] })) {
        "  " + $h.Class + " -- " + $h.Surface + " line " + $h.Line + ": " + $h.Masked
    }
    "The identifier itself is NOT printed. Nothing was committed and nothing was pushed; the mailbox clone is untouched."
    exit 8
}
# BRANCH-EXISTS-ABORT (2026-09-07): every claude/<branch> token in the entry or the subject must RESOLVE, or the post is refused
# before any git step. An entry naming a branch that exists nowhere sends its reader to a ref they cannot fetch, and is
# indistinguishable from a fabricated one -- the resolve-before-acting rule applied at the WRITE side.
# The mailbox clone CANNOT answer this: its refspec carries claude/mailbox ALONE, so it has no origin/master and no other heads.
# Every query below is read-only in the MAIN checkout (ls-remote / log / merge-base -- never a checkout, never a build).
# -MainCheckout, not a hard-wired path (2026-09-13): -ClonePath made the mailbox clone a variable while this
# stayed nailed to the coordinator's own checkout, so a "hermetic" run whose entry named any claude/* branch
# would have fetched from GitHub out of the real repository. The default is unchanged.
$bgRepo = $MainCheckout
# The ONE read above. The old form was `if (Test-Path $EntryFile) { $bgText = ReadAllText(...) + $Message }`,
# which on an unreadable file silently censused the SUBJECT ALONE and still printed a normal verdict line.
$bgText = $entryText + "`n" + $Message
# The lookbehind rejects a preceding word char or DOT so ".claude/worktrees/..." and ".claude/settings.json" -- the repo's own
# directory, 14 hits in the live mailbox -- are not read as branches, while ALLOWING '/' so "origin/claude/x" and
# "refs/heads/claude/x" still are. Measured over the whole live mailbox 2026-09-07: 545 raw tokens -> 530, the 15 dropped all
# noise (glob stems, .claude/ paths), 0 real refs lost. Both exclusions below were found by running this on real traffic.
$bgTokens = @()
foreach ($bgM in [regex]::Matches($bgText, '(?<![A-Za-z0-9._-])claude/[A-Za-z0-9._/-]+\*?')) {
    $bgT = $bgM.Value
    if ($bgT.EndsWith('*')) { continue }
    if ($bgText.Substring($bgM.Index + $bgM.Length) -match '^[\s`]*\[NEW\]') { continue }   # a dispatch naming a branch a lane is ASKED TO CREATE: marked [NEW] explicitly, so the guard cannot mistake it for a claim                   # a glob PATTERN in prose ("the claude/g-* lanes"), not a branch name
    $bgT = $bgT -replace '[.,)`]+$', ''
    if ($bgT -match '[-._/]$') { continue }                # a truncated stem: SKIP it, never repair it into a plausible name
    if ($bgT -ne 'claude/mailbox') { $bgTokens += $bgT }   # the channel itself is exempt
}
$bgTokens = @($bgTokens | Sort-Object -Unique)
if ($bgTokens.Count -gt 0) {
    if (-not (Test-Path (Join-Path $bgRepo '.git'))) { "REFUSED: branch guard cannot reach the main checkout at $bgRepo -- it cannot know, so it does not pass"; exit 9 }
    git -C $bgRepo fetch origin master 2>&1 | Out-Null    # the ancestor arms answer against a CURRENT origin/master
    git -C $bgRepo rev-parse --verify -q origin/master 2>&1 | Out-Null
    if ($LASTEXITCODE -ne 0) { "REFUSED: branch guard has no origin/master in $bgRepo -- the landed arms cannot be evaluated at all"; exit 9 }
    $bgRemote = @{}
    foreach ($bgL in (git -C $bgRepo ls-remote --heads origin 2>$null)) {   # ONE ls-remote for every token
        if ($bgL -match 'refs/heads/(.+)$') { $bgRemote[$Matches[1]] = $true }
    }
    if ($bgRemote.Count -eq 0) { "REFUSED: branch guard could not list remote heads (ls-remote returned nothing) -- an empty listing is not an answer"; exit 9 }
    # ARM 2b: name -> merged tip out of master's OWN landing record, "Merge <branch> (<LANE, ><tip>, off <base>) -- ...".
    # A landed branch is pruned BY DESIGN, so it has no tracking ref either; measured 2026-09-07 over 120 master merge
    # subjects, the tracking-ref arm (2a) resolved 0 of 57 real tokens while 16 were pruned. This arm is what admits them.
    $bgLanded = @{}
    foreach ($bgS in (git -C $bgRepo log --format='%s' --merges origin/master 2>$null)) {
        if ($bgS -match '^Merge\s+(claude/[A-Za-z0-9._/-]+)\s+\(([^)]*)\)') {
            $bgN = $Matches[1]; $bgIn = $Matches[2]
            if (-not $bgLanded.ContainsKey($bgN) -and $bgIn -match '\b([0-9a-f]{7,40})\b') { $bgLanded[$bgN] = $Matches[1] }
        }
    }
    $bgPass = @(); $bgFail = @()
    foreach ($bgT in $bgTokens) {
        $bgArm = ''; $bgWhy = @()
        if ($bgRemote.ContainsKey($bgT)) { $bgArm = 'on the remote' }
        if (-not $bgArm) {                                                   # ARM 2a: local tracking ref whose tip landed
            $bgSha = (git -C $bgRepo rev-parse --verify -q ('origin/' + $bgT + '^{commit}') 2>$null)
            if ($bgSha) {
                git -C $bgRepo merge-base --is-ancestor $bgSha origin/master 2>&1 | Out-Null
                if ($LASTEXITCODE -eq 0) { $bgArm = 'tracking ref, tip is an ancestor of origin/master' }
                else { $bgWhy += 'a tracking ref exists but its tip is NOT an ancestor of origin/master' }
            } else { $bgWhy += 'no local tracking ref, so the tracking-ref arm is NOT EVALUABLE for it' }
        }
        if (-not $bgArm) {
            if ($bgLanded.ContainsKey($bgT)) {
                git -C $bgRepo merge-base --is-ancestor $bgLanded[$bgT] origin/master 2>&1 | Out-Null
                if ($LASTEXITCODE -eq 0) { $bgArm = 'landed-and-pruned: master merges it at ' + $bgLanded[$bgT] }
                else { $bgWhy += ('master records a merge at ' + $bgLanded[$bgT] + ' but that is NOT an ancestor of origin/master') }
            } else { $bgWhy += 'no "Merge <branch> (<tip>)" record anywhere in origin/master' }
        }
        # the coordinator's transient train-head refs are deleted at landing, so ABSENT is as good as present for them
        if (-not $bgArm -and $bgT -match '^claude/coord-train[0-9]+-head$') { $bgArm = 'transient train-head ref; absent = deleted at landing' }
        if ($bgArm) { $bgPass += ('  ' + $bgT + ' [' + $bgArm + ']') } else { $bgFail += ('  ' + $bgT + ' -- ' + ($bgWhy -join '; ')) }
    }
    "BRANCH GUARD: $($bgTokens.Count) claude/* token(s), $($bgPass.Count) resolved, $($bgFail.Count) unresolved"
    $bgPass | ForEach-Object { $_ }
    if ($bgFail.Count -gt 0) {
        "REFUSED: the entry or subject names branch(es) that resolve NOWHERE -- a reader cannot fetch them:"
        $bgFail | ForEach-Object { $_ }
        "Nothing was committed and nothing was pushed; the mailbox clone is untouched."
        exit 9
    }
} else { "BRANCH GUARD: 0 claude/* token(s) to check (claude/mailbox is exempt)" }  # print the total even at zero, so "found nothing" is not "never fired"
if ($DryRun) { Write-Host 'DRY-RUN: guards passed, nothing posted'; exit 0 }  # DRY-RUN (2026-09-05): an admit-arm control must never publish
$mb = $ClonePath
if (-not (Test-Path "$mb\docs\phase4\MAILBOX.md")) { "MAILBOX CLONE MISSING at $mb"; exit 1 }
# (the ENTRY FILE MISSING check that stood here MOVED to the top of the file, above every guard -- 2026-09-07)
# ANCHOR ON STATE THE TOOL REMEMBERS, not on what the caller passes (2026-09-06, C1s finding one tool over):
# a read-confirmation COMPUTED from the live tip makes the comparison tip == tip and is vacuous by construction, so the
# absorbed range is derived from the anchor THIS SCRIPT wrote after its own last post; -LastRead is cross-checked against it.
#
# AND KEYED TO THE CLONE IT DESCRIBES (2026-09-13). The anchor records what THIS TOOL last posted to THIS
# channel. Before -ClonePath existed the two could not disagree; now they can, and a successful run against
# any other clone -- a hermetic fixture, a second checkout -- wrote that clone's SHA into the coordinator's
# own anchor file, after which the absorbed range is derived from a SHA the real clone has never heard of.
# The DEFAULT clone keeps the historical file name and location exactly (the live anchor is not migrated);
# any other clone anchors inside its OWN .git, which no `reset --hard` and no `checkout` can reach.
if ($ClonePath -eq $defaultClonePath) {
    $anchorFile = Join-Path $PSScriptRoot "coord-mailbox-anchor.txt"
} else {
    if (-not (Test-Path (Join-Path $mb '.git') -PathType Container)) {
        "REFUSED: -ClonePath $mb has no .git DIRECTORY to hold its own read anchor, and this tool will not write a non-default clone's SHA into the coordinator's anchor file"
        exit 1
    }
    $anchorFile = Join-Path $mb '.git\coord-mailbox-anchor.txt'
}
$anchor = if (Test-Path $anchorFile) { (Get-Content $anchorFile -Raw).Trim() } else { "" }
# THE ONE READ of the entry body, hoisted above the retry loop (2026-09-13): every attempt appends the SAME bytes,
# so a file edited mid-run cannot make attempt 3 post something attempt 1's guards never saw. It is the SAME READ
# the guards scanned (THE ONE READ, $entryText) and not a second ReadAllText -- there are two network calls in the branch
# guard between the two, and a file rewritten in that window would have been appended and pushed UNCENSUSED.
$entry = $entryText -replace "`r?`n", "`r`n"
# BOUNDED RETRY (2026-09-13). Lanes post about once a minute, so the window between the fetch and the push is
# routinely lost to an interleaving post and the push is rejected non-fast-forward -- a RACE, not a refusal.
# Each attempt re-fetches, re-resets, re-appends, RE-RUNS THE FLEET-TREE GUARD (the standing tree now carries the
# interleaved entries, so the guard is not skippable on a later attempt), re-commits and re-pushes. Refusals are
# NOT retried: a guard that refuses would refuse again, and exits immediately with its own code.
$fgScript = Join-Path $PSScriptRoot 'coord-mailbox-fleetguard.ps1'
$maxAttempts = 3
$prevPre = ''
$prevPush = @()
for ($attempt = 1; $attempt -le $maxAttempts; $attempt++) {
git -C $mb fetch origin claude/mailbox 2>&1 | Out-Null
git -C $mb reset --hard origin/claude/mailbox | Out-Null
$pre = (git -C $mb rev-parse HEAD)
"pre-append tip (attempt $attempt of $maxAttempts): $pre"
if ($attempt -eq 1) {
    # A RANGE THAT DOES NOT RESOLVE IS NOT AN EMPTY RANGE (2026-09-13). `git log X..HEAD` on an unknown revision
    # writes `fatal: ambiguous argument` to STDERR, returns nothing on stdout, and (at $ErrorActionPreference =
    # 'Continue') the run went on to commit and push under an ABSORBED block that reads exactly like "nothing to
    # absorb". The absorbed range IS the read discipline this tool exists to enforce, so an endpoint that cannot
    # be resolved refuses rather than reporting a measurement it never made.
    git -C $mb rev-parse --verify -q "$LastRead^{commit}" 2>&1 | Out-Null
    if ($LASTEXITCODE -ne 0) {
        "REFUSED: -LastRead $LastRead does not resolve to a commit in the mailbox clone -- the absorbed range cannot be measured, and an unmeasurable range is not an empty one."
        "Nothing was committed and nothing was pushed; the mailbox clone is untouched."
        exit 4
    }
    if ($anchor -and $anchor -ne $LastRead) {
        git -C $mb rev-parse --verify -q "$anchor^{commit}" 2>&1 | Out-Null
        if ($LASTEXITCODE -ne 0) {
            "REFUSED: this tool's own anchor $anchor does not resolve in the mailbox clone at $mb -- the authoritative absorbed range cannot be measured. Check that the anchor belongs to THIS clone before posting again."
            "Nothing was committed and nothing was pushed; the mailbox clone is untouched."
            exit 4
        }
        "READ-ANCHOR MISMATCH: you claim to have read to $LastRead; this tool last posted at $anchor."
        "ABSORBED from the TOOLS OWN anchor (authoritative, read every line):"
        git -C $mb log --oneline "$anchor..HEAD"
    }
    "ABSORBED since last-read $LastRead (read every line if non-empty):"
    git -C $mb log --oneline "$LastRead..HEAD"
    if (-not $anchor) { "(no tool anchor yet -- this run establishes one)" }
} else {
    # What arrived while the previous attempt was in flight. NEVER suppressed and never summarised: this is the
    # range the operator is obliged to read, and it is the only evidence that the previous failure WAS a race.
    $interleaved = @(git -C $mb log --oneline "$prevPre..HEAD")
    $ilRc = $LASTEXITCODE          # captured immediately: stdout-only capture makes a FAILED query look like an EMPTY one,
    "INTERLEAVED during attempt $($attempt - 1) (read every line): "
    if ($ilRc -ne 0) {             # and "nothing interleaved" is a positive claim that must not come from a failed measurement
        "  (THE QUERY FAILED, exit $ilRc -- $prevPre could not be resolved against the fetched tip. NOTHING is claimed about interleaving.)"
        "  The previous attempt's push output was:"
        $prevPush | ForEach-Object { "  $_" }
        "PUSH NOT DELIVERED -- the race test could not be evaluated. Fetch, diagnose the clone, re-append, re-push"
        exit 2
    }
    if ($interleaved.Count -eq 0) {
        "  (EMPTY -- nothing interleaved between $prevPre and $pre, so the previous failure was NOT a race.)"
        "  The previous attempt's push output was:"
        $prevPush | ForEach-Object { "  $_" }
        "PUSH NOT DELIVERED -- not a race; retrying would repeat it. Fetch, diagnose the push itself, re-append, re-push"
        exit 2
    }
    $interleaved | ForEach-Object { $_ }
}
[System.IO.File]::AppendAllText("$mb\docs\phase4\MAILBOX.md", $entry, [System.Text.UTF8Encoding]::new($false))
# FLEET-TREE GUARD (2026-09-08) -- EXIT-GATED at 10, AFTER the append and BEFORE the commit and the push.
#
# It does NOT duplicate the IDENTIFIER CENSUS above. That census is DIFF-SCOPED -- the entry file and the
# subject, i.e. the delta -- which is the shape doctrine prescribes and it is genuinely armed (measured
# 2026-09-08: a planted profile path, a planted UNC and the account literal in the subject all refuse at 8).
# What it cannot see is the mailbox branch's STANDING TREE: src/go2cs/fleetIdentifierCensus_test.go guards
# MASTER's tree only, the file does not exist on claude/mailbox, and on 2026-09-08 master's guard run against
# the mailbox tree read SEVEN hits that had stood since before the census landed -- pre-census residue that
# was never in anybody's delta. This arm scans the tracked TREE; the census above scans the DELTA.
#
# Placed AFTER the append deliberately: the guard reads WORKING-TREE content of tracked paths, so the entry
# just appended to the tracked MAILBOX.md is inside what it scans (verified with a planted line, which it
# reported at the appended line number). A refusal rolls the append back from the index, which matches
# origin here because of the reset above, so the clone is left exactly as the other refusal paths leave it.
if (-not (Test-Path $fgScript)) {
    "REFUSED: coord-mailbox-fleetguard.ps1 is missing beside this tool -- the tree guard cannot run, and a gate that cannot measure does not pass"
    git -C $mb checkout -q -- docs/phase4/MAILBOX.md
    exit 10
}
& powershell -NoProfile -ExecutionPolicy Bypass -File $fgScript -ClonePath $mb -MainCheckout $MainCheckout
$fgRc = $LASTEXITCODE          # captured immediately, before anything else can touch $LASTEXITCODE or $?
if ($fgRc -ne 0) {
    "REFUSED: the mailbox TREE carries a fleet identifier, or the tree guard could not measure (exit $fgRc) -- the line numbers and classes are printed above; the identifier itself is not."
    "Nothing was committed and nothing was pushed; the appended entry has been rolled back."
    git -C $mb checkout -q -- docs/phase4/MAILBOX.md
    exit 10          # a REFUSAL is not a race: it is not retried, on this attempt or any other
}
git -C $mb add docs/phase4/MAILBOX.md
$msgFile = [System.IO.Path]::Combine([System.IO.Path]::GetTempPath(), "coord-mailbox-msg-$PID.txt")
[System.IO.File]::WriteAllText($msgFile, $Message, [System.Text.UTF8Encoding]::new($false))
git -C $mb -c commit.gpgsign=false commit -q -F $msgFile
Remove-Item $msgFile -ErrorAction SilentlyContinue
$post = (git -C $mb rev-parse HEAD)
# The rollback is `checkout -q <tree-ish> -- <path>`, NOT `checkout -q -- <path> <tree-ish>` (2026-09-13): after `--`
# BOTH operands are pathspecs, 'origin/claude/mailbox' matched no tracked file, and git aborted the WHOLE checkout --
# measured rc=1 with the appended MAILBOX.md still staged and still dirty. The one refusal path that claimed to
# discard the entry was the one that did not. The other two sites (above) already use the correct form.
if ($post -eq $pre) { "COMMIT DID NOT LAND (HEAD unchanged at $pre) -- NOT DELIVERED; entry discarded by the next reset"; git -C $mb checkout -q origin/claude/mailbox -- docs/phase4/MAILBOX.md; exit 3 }
$pushOut = @(git -C $mb push origin claude/mailbox 2>&1 | ForEach-Object { "$_" })
$pushOut | ForEach-Object { $_ }
$local = (git -C $mb rev-parse HEAD)
$remote = ((git -C $mb ls-remote origin refs/heads/claude/mailbox) -split "`t")[0]
"local=$local"
"remote=$remote"
# DELIVERY IS CONTAINMENT, NOT EQUALITY (2026-09-13, from the adversarial review and reproduced end to end).
# `$local -eq $remote` compares our HEAD against the remote tip AT THE MOMENT OF THE ls-remote. Lanes post about
# once a minute, so a lane landing in the window between our ACCEPTED push and that ls-remote leaves $local -ne
# $remote while our commit is ON the remote: the tool printed "PUSH NOT DELIVERED", the retry reset onto a tip that
# ALREADY CARRIED our entry, re-appended, pushed again -- and reported DELIVERED=True at exit 0 with the entry in
# the mailbox TWICE. The EMPTY-interleave detector cannot catch it (the range is non-empty; it contains our own
# commit). So a non-equal reading is not a verdict: fetch, and ask whether our commit is CONTAINED in the remote.
$delivered = ($local -eq $remote)
if (-not $delivered) {
    git -C $mb fetch origin claude/mailbox 2>&1 | Out-Null
    git -C $mb merge-base --is-ancestor $local origin/claude/mailbox 2>&1 | Out-Null
    if ($LASTEXITCODE -eq 0) {
        $delivered = $true
        $rtip = (git -C $mb rev-parse origin/claude/mailbox)
        "DELIVERED-LATE: the remote moved between our push and the ls-remote. $local IS contained in origin/claude/mailbox (now $rtip), so the push was ACCEPTED -- re-appending would post this entry a second time."
    }
}
"DELIVERED=$delivered"
if ($delivered) {
    [System.IO.File]::WriteAllText($anchorFile, $local, [System.Text.UTF8Encoding]::new($false))
    "READ-ANCHOR now $($local.Substring(0,9)) -- the tools own record; the next run derives the absorbed range from it whatever -LastRead says"
    exit 0
}
"PUSH NOT DELIVERED on attempt $attempt of $maxAttempts"
$prevPre = $pre                # the next attempt reports what arrived between THIS tip and the one it fetches
$prevPush = $pushOut
git -C $mb fetch origin claude/mailbox 2>&1 | Out-Null
git -C $mb reset --hard origin/claude/mailbox | Out-Null      # roll this attempt's commit off; the next attempt re-appends
}
"PUSH NOT DELIVERED -- fetch, READ the interleaved commits, re-append, re-push"
exit 2
