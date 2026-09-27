# MILESTONE — the 75% crossing, prepared
<!-- {% raw %} — Jekyll/Liquid guard: this doc quotes Go/Liquid-ambiguous syntax and shell braces; keep the matching endraw as the final line. -->

> **✅ EXECUTED (2026-08-22) — this is now a RECORD of a ritual that ran.** The crossing happened and
> the package below was used as written: tag **`stdlib-tests-75pct-2026-08-22`**, branch
> **`release/go1.23`**, published **`nuget-1.23.1.7`** — all three present in the repository. The
> roster header it waited on reads **162 / 215 (75.3 %)**, so the frame's precondition
> (*"Complete 1.23.1"*) is satisfied, which is what opened the hop era. The `<PLACEHOLDER>` tokens
> below were filled from the roster header at execution, exactly as the preparation required.
>
> Amended, never rewritten: everything under the original status line stands as prepared, including
> the parts the execution changed — the superseded two-machine release flow is recorded as superseded
> **in place** rather than deleted. One lesson this ritual earned is doctrine elsewhere now: the
> **announcement text lands BEFORE the tag mints**, because the tag anchors the shipped tree and
> cannot be moved afterward — see [`../GoCorpusMigration.md`](../GoCorpusMigration.md) H12, where the
> release ritual is defined in five ordered elements.

> **Status AS PREPARED: PREPARED, not executed.** This is the ritual package for Go 1.23.1's **terminal**
> validation marker — 75 % of the testable standard library — assembled ahead of the crossing so the
> crossing itself is review-and-execute rather than compose-under-pressure. Nothing here has been
> run: no tag exists, no branch exists, no package is published, and no user-facing document has
> been touched. Every mechanic below was read out of the script or doc that owns it, and each claim
> names its source. Figures the crossing supplies are left as `<PLACEHOLDER>` tokens; they are
> filled from the roster header at execution, never predicted here.
>
> **Why this milestone is tagged at all.** The charter's remaining cadence is 50 %, then **75 % only
> if it coincides with something genuinely hard landing**, then 100 % unconditionally
> ([`Phase4-Autonomous-Loop-Charter.md`](Phase4-Autonomous-Loop-Charter.md) §4). The position-map
> arc is that landing: converted frames report Go `file:line` positions derived from conversion-time
> facts, which is what three of the last five rows were waiting on. The condition is met on its
> merits, not by arithmetic.
>
> **What the crossing triggers**, per [`../PLAN-corpus-upgrade.md`](../PLAN-corpus-upgrade.md) §0:
> the annotated signed tag, the long-lived `release/go1.23` branch, a full-roster consolidation
> sweep, and a user-owned pre-hop .NET 9 anchor NuGet release with its README/NEWS updates. §0 also
> fixes what the marker *means*: 75 % is 1.23.1's **terminal** marker, not a waypoint — validation
> spent on `.1` past it is spent twice, because every roster row re-derives from the new release's
> test sources at the next hop (H10). The campaign continues on 1.23.12.

**Section map**

| § | Contents | Owner |
|:--|:--|:--|
| [1](#1-the-tag-annotation) | The tag annotation text, final draft | coordinator |
| [2](#2-the-crossing-command-sequence) | Tag, branch, sweep dispatch — in order, with preconditions | coordinator |
| [3](#3-the-anchor-release-checklist--user-executed) | Anchor NuGet release checklist | **USER** |
| [4](#4-draft--the-readmenews-milestone-text--user-owned) | README/NEWS milestone text | **USER** (draft only) |
| [5](#5-consistency-preflight--run-before-tagging) | What to verify before tagging | coordinator |
| [6](#6-what-this-milestone-does-not-include) | Deferred items riding behind the milestone | — |

---

## 1. The tag annotation

The charter fixes the shape: annotated, gpg-signed, named `stdlib-tests-<pct>pct-<date>`, its
annotation carrying **the figures, what was banked in the arc, and the correctness classes the arc
flushed out** — "the record a future reader wants" (§4). The precedent is
`stdlib-tests-25pct-2026-07-26`, whose annotation runs subject line + five body paragraphs wrapped
near 72 columns; this draft follows it.

Save the block below to a scratch file and pass it with `git tag -s … -F <file>` (§2, step 2) — the
first line becomes the subject. Fill every `<PLACEHOLDER>` from the roster header
(`docs/ValidatedTestPackages.md`) **after** the final banking commit lands, never before.

```text
Three quarters of Go's standard-library test suites validate in C#

<ROWS> of the 215 testable standard-library packages (<PCT>%) now have their
own Go 1.23.1 test suites converted, built against the converted standard
library, run under the Go-semantics test host, and compared verdict-for-
verdict against a clean `go test -json` baseline: <VERDICTS> matching
verdicts, <DISCLOSED> signature-pinned disclosed divergences. A row appears
only when EVERY eligible Test function agrees, which is what keeps the
denominator honest.

This is Go 1.23.1's TERMINAL marker, not a waypoint. Every roster row
re-derives from the release's own test sources at a version hop, so
validation spent here past the credibility milestone would be spent twice;
the campaign continues on 1.23.12, the release users would actually choose.

Banked in this arc, and the reason 75% is tagged at all: the POSITION MAP.
A converted frame now reports a Go file:line pair, derived from conversion-
time facts rather than composed -- the file identity falls out of the same
import-path derivation the frame's function name already used, and file and
line ship together because either alone mints a position that exists in
neither tree. The identity is build-shape-faithful, exactly as Go bakes it:
the published standard library reports its trimpath/import-path form, a
-recurse user module reports what Go would have baked for that build. Its
consumer set -- runtime/debug, log, log/slog, flag -- is three of the last
five rows, which is what made it the campaign's highest-leverage single arc.

Four more arcs landed behind it. CHANNEL DIRECTION became descriptor cargo
riding on the channel value, carried the way array dimensions already were,
and reflect.Value.Recv/Send landed with it -- retiring the chan-direction
disclosure class on its own recorded remedy and banking both template
packages. MAP KEY AND ELEMENT DIMENSIONS became cargo on the same carrier,
reaching a struct field's dims through the pointer and map hops a decode
target cannot measure, and retiring the array-length class whole. A NATIVE
POINTER SLOT now holds the pointer's VALUE rather than a managed reference,
closing the uintptr -> unsafe.Pointer -> uintptr round-trip that took
sync/atomic to 108 of 108 and a GC-invisible dangling-reference hazard with
it; alongside it the alignment identity token became layout-truthful,
answering Go's align64 guarantee from the same metadata that answers
StructField.Offset. And a READMEMSTATS MEASUREMENT SURFACE -- an always-on
GC pause recorder -- supplies real pause and release facts with the read
path measuring 0.0 B per call.

The campaign flushed out correctness classes no compile could see, and its
discipline is visible in what it REFUSED as much as what it banked. A
disclosure names what the managed runtime provably cannot satisfy, never
what is merely unimplemented: encoding/gob stands at 105 of 106 and does not
bank, because reflect.ArrayOf/StructOf is an arc with a price, not a
divergence; net/netip's structural wall fell from 0 to 210 of 267 and it
does not bank either, because all 57 residuals are one root -- allocation
behavior -- and none is disclosable; crypto/internal/edwards25519 was
measured against a signed-off design, found to have no constituency on its
path, and moved off the terminal route rather than forced. The classes as
they stand: alloc-profile, alloc-count-semantics, codegen-liveness and
host-limit carry the manifests today; runtime-capability is minted and joins
the roster preamble with the first banking commit that uses it; and
chan-direction is GONE -- it was written to retire itself on its own remedy,
and it did.

Corpus: <PKGS> converted packages compiling, the behavioral suite green, one
tree emitting windows, linux and darwin, and every proof page regenerated
from the comparison record that decides the rows.
```

**Composition notes** (for the reviewer, not for the annotation):

- The figures line uses the roster header's own three numbers. The header is recomputed from its
  own table; §5 verifies the table sums to it before the tag is minted.
- The disclosure-class paragraph states four live classes plus `runtime-capability` as minted.
  Verified: the committed manifests use exactly `alloc-profile`, `alloc-count-semantics`,
  `codegen-liveness`, `host-limit` today; `runtime-capability` was ruled into existence on
  2026-08-20 with `WriteHeapDump` as its only member and "joins the roster preamble WITH the first
  banking commit that uses it, not before" (BOARD, *RULING x2*). **If `runtime/debug` has banked by
  the time the tag is minted, the class is live and the sentence is already true; if it has not,
  change "is minted and joins" to "is ruled and lands with `runtime/debug`."**
- The `host-limit` line is deliberately not expanded here; the position-map ruling adds one entry to
  that class (`TestStack`'s fifth-frame assert) whose retirement path is **structural and permanent**
  — unlike the `os/exec` relocatability entries, it does **not** retire at the .NET 10 single-file
  host, and the entry's own text must say so. That belongs in the roster preamble and the proof
  page, not in the tag.
- Keep the annotation free of anything that changes after the tag is cut. Package counts and roster
  figures are pinned by the tag; machine-dependent timings and in-flight arc status are not, and do
  not belong in it.

---

## 2. The crossing command sequence

Run from a clean checkout of **master** at the commit that banks the final row. Every command below
is coordinator-owned. Preconditions are stated per step; a failed precondition stops the sequence
rather than being worked around.

### Step 0 — preconditions for the whole sequence

```bash
git -C <repo> status --porcelain          # must be EMPTY
git -C <repo> rev-parse --abbrev-ref HEAD # must be: master
git -C <repo> log --oneline -1            # note this SHA; it is the tree the tag names
```

- The final banking merge has had its **post-merge filtered sweep at the merge result** —
  `run-validated-sweep.ps1 -Filter <pkg>` on merged master, not on the lane tip (CLAUDE.md's
  banked-row protection rule; a lane's proof binds its own tree, never the merge result).
- §5's consistency preflight passes in full.
- No sibling lane is mid-merge into master.

### Step 1 — write the annotation to a scratch file

```bash
# Use YOUR lane-prefixed scratch path; the scratchpad is shared across concurrent lanes.
# Fill every <PLACEHOLDER> from docs/ValidatedTestPackages.md's header first.
$ann = "<scratchpad>/coordinator-75pct-tag-annotation.txt"
```

Precondition: no `<PLACEHOLDER>` token survives in the file. Grep it before proceeding.

### Step 2 — mint the annotated, signed tag

```bash
git -C <repo> tag -s stdlib-tests-75pct-<YYYY-MM-DD> -F "$ann"
git -C <repo> tag -v stdlib-tests-75pct-<YYYY-MM-DD>   # verify the signature
git -C <repo> tag -n60 stdlib-tests-75pct-<YYYY-MM-DD> # read back the annotation as stored
```

Preconditions: gpg agent available (a signing failure must stop the sequence, not be downgraded to
an unsigned tag); the tag name does not already exist. Date format is `YYYY-MM-DD`, matching
`stdlib-tests-25pct-2026-07-26` and `stdlib-half-validated-2026-08-08`.

⚠ **A pushed tag is effectively immutable.** Read the annotation back (`-n60`) and accept it before
step 4 pushes anything.

### Step 3 — create the `release/go1.23` branch

```bash
git -C <repo> branch release/go1.23 stdlib-tests-75pct-<YYYY-MM-DD>
git -C <repo> log --oneline -1 release/go1.23   # must equal the SHA from step 0
```

Precondition: `release/go1.23` does not exist locally or on the remote (verified at preparation
time: the remote carries only `master`, `claude/netpoll-arc`, `claude/scheduler-arc`).

Shape, per PLAN §0.6: this is the long-lived version branch for the 1.23 story. Master cuts over to
a hop only at the five-gate parity check (P1–P5, PLAN §4); the branch may carry a red P2 for a long
time — that is what it is for.

### Step 4 — push the tag and the branch

```bash
git -C <repo> push origin stdlib-tests-75pct-<YYYY-MM-DD>
git -C <repo> push origin release/go1.23
```

Precondition: master itself is pushed and current (the charter's push policy authorizes
`git push origin master` at gated clean points; a tag pointing at an unpushed commit publishes a
dangling reference).

### Step 5 — dispatch the full-roster consolidation sweep

The consolidation sweep is the milestone's own proof that every banked row still validates on one
tree, at one commit, in one run. It runs on the sweeper machine, backgrounded, from a coordinator
session — **never parked by a lane**, whose process tree is reaped at the turn boundary (CLAUDE.md's
sweep caveat; two sweeps were lost to exactly this at 106/110 and 98/110).

The dispatch is a **mailbox entry**, appended on branch `claude/mailbox`, in the mailbox's own fixed
entry format. Append, never edit; pull before appending, push immediately after.

```markdown
## <YYYY-MM-DD HH:MM> · FROM coordinator · TO <sweeper machine>

JOB — 75% consolidation sweep, full roster, at the tag.

Tree:      stdlib-tests-75pct-<YYYY-MM-DD>  (= master @ <SHA>)
Command:   ./src/run-validated-sweep.ps1 -TestTimeout <value>
Run it:    BACKGROUNDED, detached (Start-Process -WindowStyle Hidden, output
           redirected to a lane-prefixed log). Do NOT use -NoNewWindow +
           Wait-Process: the wait re-parents the run onto the turn and the
           turn boundary kills it.
Budget:    measure it; the last full-roster reading is stale and the roster
           has grown. Budget well above it and re-measure — the four
           $longTimeouts floors (hash/maphash, index/suffixarray, crypto/dsa,
           archive/zip) are FLOORS, and a LARGER -TestTimeout raises them.
Expected:  <ROWS>/<ROWS> packages, <VERDICTS> matching verdicts,
           <DISCLOSED> disclosed — the roster header's own arithmetic. Any
           count movement other than a declared host-conditional row's named
           floor+k is a FAILURE, not a variance.
Report:    reply here with the summary line, the elapsed time, the machine,
           and `git status` classified per CLAUDE.md's post-sweep rules
           (CRLF phantoms, -tests-closure re-emissions, .cs.auto siblings =
           restore; anything else = real drift, stop and root-cause).
           Re-measure and post the wall time so the budget table can be
           updated — the current row is a stale reading.
```

⚠ **Two notes on the mailbox itself.** Its protocol defines one entry format and **no `JOB`
schema** — the block above conforms to the entry format with `JOB` as the body's first word, which
is the smallest thing that reads as a dispatch without inventing a protocol. And the mailbox is
transport, not record: the sweep's *result* belongs on the board, not here.

### Step 6 — decision point the coordinator owns at execution

**If the consolidation sweep is not green, what happens to the tag?** No doctrine covers this, so it
is named rather than assumed. The mechanics that constrain the choice: a pushed tag cannot be moved
without breaking every README badge link minted against it and every reader who fetched it; the
milestone's public surface (§4's README/NEWS text, the anchor release) is separately gated and has
not shipped at this point. The shape that follows from those two facts — **the tag stays, naming the
tree it named; the fix lands as a follow-up commit; the announcement and the anchor release wait on
a green sweep** — is offered as a recommendation, not a ruling.

---

## 3. The anchor-release checklist — USER-EXECUTED

> **TIMING (user + coordinator, 2026-08-22): the anchor release WAITS for parity-close.** The
> anchor's hard constraint is only "before the .NET 10 hop", and Linux parity precedes the hop on
> the ladder -- so the release ships ONE combined story ("over 75% validated, on Windows and
> Linux") instead of two pushes. **Trigger, concretely:** (a) the readiness poller landed and
> measured (it flips the socket family incl. crypto/tls's Linux leg); (b) the remaining Linux
> seams closed or classified-final; (c) the per-OS verdict-arithmetic ruling delivered (its
> constituency: crypto/rand, mime, path/filepath, debug/buildinfo); (d) one full dual-OS
> consolidation sweep green (both legs on the sweeper). Until then: the tag flies, the site is
> live, and nothing downstream blocks on the unpublished packages.
>
> **✅ ALL FOUR CONDITIONS CLOSED (coordinator, 2026-08-23) — the release is GO.** (a) the poller
> merged at `18770d083` measured through S3 (crypto/tls 400/402 on Linux, the Windows banked
> count); (b) seams classified-final (every Linux FAIL row attributed, nothing unexplained);
> (c) the ruling delivered and LIVE (the consolidation sweep validated all four constituency
> rows against their annotations; the COUNT class is empty); (d) the dual-OS consolidation
> sweep at `18770d083`: **Windows leg 162/162 PASS (JOB-007, 18,569 verdicts, the exact roster
> total)** and **Linux leg 152/162 GREEN (149 PASS / 10 FAIL / 3 CVAC, zero regressions, all
> ten FAILs attributed to classified seams)** — the quadruple + attribution reading per the
> board's condition-(d) arithmetic entry. Per-row wall tables for both legs are banked in
> [`DATA-sweep-row-walltimes.md`](DATA-sweep-row-walltimes.md).

> **⚠ SUPERSEDED FOR FUTURE RELEASES (2026-08-24) — the ritual is now ONE MACHINE, and the only
> human act is the card PIN.** The owner's code-signing certificate now lives on the release
> machine, so `release-nuget.bat` runs pack → sign → push → record as four phases of one
> command: Phase 0 proves every precondition (clean tree, API key, certificate reachable) before
> anything moves, Phase 2 signs the whole set in ONE process so the card is unlocked ONCE, and
> Phase 3 gates the irreversible push behind an explicit confirmation. The owner delegated the
> ritual on that basis. Two facts make the delegation safe, and both are enforced in the script
> rather than remembered: a published version can be unlisted but never deleted (hence the
> gate), and **signing is now MANDATORY** — the certificate is registered with nuget.org, which
> rejects any unsigned package pushed under the account. The text below records the two-machine
> flow this supersedes; `-OfflineSigning` still executes it.
>
> **⚠ USER-EXECUTED IN FULL** (the superseded flow). Publishing to nuget.org is an irreversible act — a version can be
> unlisted, never deleted — and no lane and no coordinator session runs `push-nuget.ps1 -Push`. The
> checklist below is what the user reads while running it. Every mechanic is read out of
> [`../../src/push-nuget.ps1`](../../src/push-nuget.ps1) at the line noted; nothing is inferred from
> the plan documents.
>
> The **pre-hop .NET 9 anchor** framing: this release is the last publication of the 1.23.1 corpus on
> .NET 9, cut *before* the .NET 10 hop and the 1.23.12 hop so the ladder has a fixed, published
> reference point on the runtime the campaign was measured on.

### 3.1 Version arithmetic — what this release will be

Read from [`../../src/version.props`](../../src/version.props): `GoStdLibVersion` **1.23.1**,
`GoBuildNumber` **6**. `GoBuildNumber` is the LAST-PUBLISHED build number, and `-Push` increments it
*before* publishing (script lines 20–23, 139–160). Therefore:

| | Value |
|:--|:--|
| Current published version | `1.23.1.6` |
| This release publishes | `1.23.1.7` |
| Release tag it mints | `nuget-1.23.1.7` |
| Write-once proof snapshot | `docs/validation/1.23.1.7/` |

Confirmed consistent with the tree: tags `nuget-1.23.1.3` … `nuget-1.23.1.6` exist, and
`docs/validation/` holds `1.23.1.2` … `1.23.1.6` plus `current/`.

`-BumpBuild` defaults **on** with `-Push` and **off** for a pack-only run; `-BumpBuild:$false`
re-publishes the current version (line 59–63, 143) — used only to finish a partially-failed push.

### 3.2 Preconditions the script enforces itself (it fails fast; know why)

| # | Precondition | Where | Failure shape |
|:--|:--|:--|:--|
| 1 | `src/go2cs-stdlib.slnx` and `src/version.props` exist | 105–106 | throw |
| 2 | **Go toolchain on PATH** | 119–121 | throw — the metadata gate needs it |
| 3 | **`TestStdLibMetadataInSync` passes** — `go test -count=1 -run TestStdLibMetadataInSync .` in `src/go2cs` | 123–137 | throw. Remedy named in the message: `go generate .` from `src/go2cs`, commit the regenerated `stdlib-metadata.txt`, re-run. This runs **before anything is built**, deliberately, so the run fails at second zero rather than after a full Release build |
| 4 | `git` available | 192–196 | **warning, not a throw** — the run continues and every package README's C# Source badge links a tag that does not exist. Treat this as fatal by hand |
| 5 | **gpg signing works** — the release tag is `git tag -s` | 200–208 | throw, on purpose: "publishing 300 packages whose READMEs all link a tag that does not exist is worse than stopping" |
| 6 | The write-once snapshot for the newly bumped version does **not** already exist | 229–235 | throw — "the version counter and the docs tree disagree" |
| 7 | `-SkipBuild` **not** passed | 547–551 | throw — a multiplatform release has no single on-disk build to pack |
| 8 | `NUGET_API_KEY` (or `-ApiKey`) set — only checked at the push gate | 817 | throw, after packing |

⚠ **Precondition 3 is the one that bites after corpus work.** `stdlib-metadata.txt` is generated
*from* the corpus and is what the converter reads under `-recurse=nuget` instead of the on-disk
`package_info.cs`. A regen that moved records without regenerating it leaves this gate red — and the
damage of publishing anyway lands in end users' builds, not here.

### 3.3 Dry run — the pack-only default, **not** `-WhatIf`

The script's own documented inspection path is the **default pack-only run** (`.EXAMPLE`, lines
68–70): *"Pack every package to `src\artifacts\nupkg` (no push, no bump). Inspect the output, then
push."* It ends at the push gate with `Pack-only (default)` and `exit 0` (lines 910–914).

```powershell
# From src\ — USER, on the release machine, solo. Two full Release builds dominate; budget ~20 min
# warm and ~50 min on a cold tree, both measured on the fleet's slowest box (§3.6.4, §3.6.8).
.\push-nuget.ps1
```

**Budget: tens of minutes, not hours — but budget from the top of the range.** Two measurements exist,
both on the i7-5820K, the slowest machine on the fleet:

| Tree state | Whole fifteen-phase run | |
|:--|--:|:--|
| Warm (`bin`/`obj` already populated) | **1,095 s — 18.3 min** | §3.6.4 |
| **Cold** (a fresh worktree, nothing built) | **2,811 s — 46.8 min** | §3.6.8 |

The two Release builds are the whole story — ~510 s each warm, roughly 2.5× that cold — while the
metadata gate, the freeze, both retargets, both verifiers, the flavor comparison, the merge and both
packs total well under two minutes either way. **Budget ~50 minutes** unless the release machine has
already built this tree; a release machine usually has not. (This paragraph read *"budget hours on a
slow host"* before the rehearsal executed it — an over-estimate, but only by ~2× against the cold
number, not the ~6× §3.6.6 D3 inferred from the warm run alone.)

What a pack-only run does and does not do, verified:

- **Does not bump** `GoBuildNumber` (line 143: `$doBump = [bool]$Push`).
- **Does not mint the release tag** — the mint is gated on the bump, and prints
  `No build-number bump this run -- not tagging (the run that bumps mints nuget-1.23.1.7)`, naming
  the **would-be** tag (lines 213–225).
- **Does** run the full metadata gate, both RID build+pack passes, the flavor comparison, and the
  merge — i.e. it exercises everything that can fail on build or packaging grounds.
- **Does** exercise the freeze, both badge retargets and both verifiers, against a **would-be**
  snapshot: a pack-only run freezes `docs/validation/current/` into a *temporary* directory named
  for the version a bumping run would publish, verifies every green badge against those pages, and
  deletes it (lines 256–330). So a pack-only run **is** a real badge-consistency check, and phase 5's
  `Froze N` count is measured rather than skipped. It writes **nothing** into `docs/validation/`.

⚠ **This is the fixed behavior; the original could not run at all.** Until 2026-08-22 a pack-only run
reused the LAST-PUBLISHED version for the snapshot path, found that directory already frozen, skipped
the freeze, and then verified today's badges against a stale snapshot — so it threw within eight
seconds on the first package validated since the last release, which is the normal state of this
campaign. §3.6.2 has the mechanism and §3.6.6 D1 the defect; the fix moves only the proof-page
*location* for a pack-only run and leaves every `-Push` path byte-identical.

- **Does not** pack a `VALIDATION.md` for packages validated since the last release. The `.csproj`
  `Exists()` guard reads the repository's `docs/validation/<packed-version>/`, which the dry run's
  temporary snapshot deliberately does not move — so a dry run packs the last release's page count,
  silently and with `exit 0` (§3.6.5). That gap is a **property of the dry run only**: on release
  morning the freeze writes every page into the tree before the build and every guard is satisfied.

⚠ **`-WhatIf` is not a whole-script rehearsal, and should not be used as one.** The script declares
`SupportsShouldProcess`, and its writes and pushes are individually gated — but the two
`dotnet build` and two `dotnet pack` invocations are native calls that are **not** ShouldProcess-
gated (lines 614, 620), so a `-WhatIf` run still performs both full Release builds and both packs.
Meanwhile the merge's `Copy-Item` (line 759) *is* a ShouldProcess cmdlet and inherits the WhatIf
preference, so the copy is suppressed while the zip surgery that immediately follows it is not.
Reading the code, a `-WhatIf -Push` run is therefore expected to do the expensive work and then fail
in the merge rather than report a clean rehearsal. **This has not been executed** — the statement is
from source, and it is exactly why the pack-only run is the recommended dry run. (See the flag in
§6's contradiction note: PLAN §4's P5 gate names "`push-nuget.ps1` dry run" as the instrument that
exercises the *tag mint*, which no dry run does.)

### 3.4 The release run

```powershell
# USER, solo on the release machine. NUGET_API_KEY set in the environment.
.\push-nuget.ps1 -Push
```

Phase order, and the invariant to check at each (the script prints a `==>` line per phase):

| # | Phase | Verify |
|:--|:--|:--|
| 1 | `Verifying stdlib-metadata.txt matches src\core` | passes; else regenerate and restart |
| 2 | `Bumped GoBuildNumber 6 -> 7` | the number is what §3.1 predicts |
| 3 | `Package version: 1.23.1.7   (solution: go2cs-stdlib.slnx)` | matches |
| 4 | `Created signed release tag nuget-1.23.1.7 at <sha>` | tag is signed and at the intended commit. It is minted **before** the build because every README bakes a link to it (lines 168–186) |
| 5 | `Froze N validation proof page(s) at docs\validation\1.23.1.7` | **N == the roster row count** — the snapshot is `docs/validation/current/*.md` copied verbatim |
| 6 | `Retargeted N README badge link(s)` / `Verified N green badge(s) against the frozen 1.23.1.7 proof pages` | **N == the roster row count**. The verifier re-derives each green badge from the frozen proof page's `**X matched · Y disclosed**` totals line and throws on any mismatch (lines 282–312) |
| 7 | `Retargeted N C# Source badge(s)` / `Verified N C# Source badge(s) pin 1.23.1.7 and its release tag` | this verifier throws on **any** package README lacking the badge — that vacuous-pass hole was closed after the `1.23.1.5` run shipped through it (lines 328–332, 366) |
| 8 | `Layout L3: N package(s) carry per-GOOS sources -> RID-specific assemblies` | derived from the corpus itself (a GOOS-named directory holding no project file), never a hardcoded list (lines 556–583) |
| 9 | `[linux-x64] Building Release at -p:GoTargetOS=linux` then `[win-x64] …windows` | **two full Release builds**, `--no-incremental`, `-p:UseSharedCompilation=false`. Order is reversed deliberately so the *reference* (windows) flavor is the last pass and the tree is left in the state a plain build produces (lines 589–594) |
| 10 | `[<rid>] N package(s)` for each RID | **the two counts must be equal**; the script throws if the package-ID **sets** differ (lines 676–682) |
| 11 | `Flavor comparison … / of N platform-neutral package(s): P differ materially, I differ only in the deterministic-identity fields` | **P must be 0.** A non-zero P prints a loud warning: a package with no per-GOOS sources whose assembly *length* moves between flavors means the corpus gained a platform axis the L3 derivation cannot see — investigate before publishing (lines 742–748) |
| 12 | `Merged M RID-specific package(s); copied C platform-neutral package(s) verbatim` | M == the L3 count from phase 8 (plus any promoted); a neutral package is copied byte-for-byte from the reference flavor, so the Windows lane cannot regress through the merge |
| 13 | `Packed N package(s)` | N == the per-RID count from phase 10 |
| 14 | `Pushing N package(s) to https://api.nuget.org/v3/index.json` | `go.lib` and `go.gen` go first (every stdlib package depends on them), then the rest, each with `--skip-duplicate` so a re-run is idempotent (lines 819–833) |
| 15 | `Done. Pushed N package(s) at version 1.23.1.7.` | — |

⚠ **Phase 9 is the release's real risk, and it is not a Windows risk.** The linux pass is a full
`-p:GoTargetOS=linux` build of the stdlib solution and the script **throws** if it fails — and
because the build order is reversed, the linux pass runs *first*, so a broken linux build costs the
whole run before any Windows artifact exists. Both CLAUDE.md and
[`CENSUS-linux-compile-wall.md`](CENSUS-linux-compile-wall.md) §10 record that build as clean
(307/307, 0 errors, 475 s, 2026-08-14, after the regen wave). **The reason to re-check it anyway is
the census's own root cause: the wall was corpus debt, created by Windows-only regens leaving a
package's shared files and its per-GOOS files emitted by two different converter eras** — 112 errors,
one package, one class. That mechanism recurs with every Windows-only regen since, so the clean
reading is a measurement with a date, not a standing property. Re-measure at the release tree, in
minutes rather than hours:

```powershell
dotnet build src\go2cs-stdlib.slnx -c Debug -p:GoTargetOS=linux --no-incremental -p:UseSharedCompilation=false -clp:ErrorsOnly
```

(`--no-incremental` is mandatory across a `GoTargetOS` switch: what differs between targets is the
`<Compile>` item set, not any timestamp, and `obj/` is poisoned otherwise. Purge `bin`/`obj`/
`Generated` between target switches.)

⚠ **Disk, before either of the two Release builds.** A release run writes two full builds, two pack
trees and a merge, and consumes **~7 GB** on the repository volume (§3.6.1, measured). A nearly-full
repo drive makes writes fail *mid-run* and surface as false build failures or a truncated tracked
file — which is why `run-validated-sweep.ps1` refuses below a 25 GB floor. **Check free space.**

The slow-`D:` warning this paragraph used to carry — the census's **1,606 ms per 4 KB file**, a
2,140× penalty over `C:` — **was a measured state of the disk at the census, not a property of it**,
and it does not reproduce: re-measured 2026-08-22 at **0.65 ms per 4 KB file on `D:`** against
0.82 ms on `C:`, i.e. no penalty at all, and the rehearsal ran the whole release path from `D:` in
18.3 minutes (§3.6.4, §3.6.6 D4). Do not route the release off the repository's own volume on the
strength of the old number. If a phase looks hung, **re-measure** the volume rather than assuming
either reading.

### 3.5 After the push

1. **Commit together**, as one release commit: `src/version.props` (the bump), the frozen
   `docs/validation/1.23.1.7/` snapshot, and every retargeted `src/core/**/README.md`. The script
   says so at three separate points; they are one atomic record of what was published.
2. **Push the `nuget-1.23.1.7` tag with the release commit.** The tag is minted at HEAD *before* the
   release commit, deliberately: the two differ only by version.props, the snapshot and the badge
   links — no converted C# moves between them — so the tree a badge reaches is the C# in the package
   (lines 176–179).
3. **Spot-check one published package's README on nuget.org**: the Tests badge, its proof link
   (`go2cs.net/validation/1.23.1.7/<dot-id>.html`) and the packed `VALIDATION.md` must all describe
   the binary just pushed. Each validated package's `.csproj` packs
   `$(go2csPath)../docs/validation/$(GoStdLibVersion).$(GoBuildNumber)/<dot-id>.md` as
   `VALIDATION.md` under an `Exists()` guard — so a missing snapshot page is **silent**, which is
   what phase 5's count check exists to catch.
4. **Only then** apply §4's README/NEWS text (still user-owned).
5. **Record whether the run printed `"repairing with a direct project build"`** — one line in the
   release notes/commit is enough. This is the pack-race alternation discriminator (ledger #5,
   closed measured-and-hardened): the script now disables MSBuild node reuse, and the
   assert-and-repair stays as the instrument — if the repair never fires again after the flag,
   node reuse is confirmed as the race's root by alternation, at zero repro cost; if it fires
   WITH the flag set, the attribution is wrong and the forensics reopen with a binlog to catch it.

### 3.6 REHEARSAL — the dry run, executed (2026-08-22)

§3.3 names the pack-only run as the checklist's own dry-run-first step. This is that step, performed,
so release morning has no first-time surprises. Machine: **i7-5820K** (6C/12T, 32 GB, Windows 11),
.NET SDK 9.0.317, Go 1.23.1 windows/amd64, repository on `D:`, run solo. Tree: master `71a95c8ff`,
`git status` clean, `GoBuildNumber` **6**.

Everything below is measured. Two runs were made: the documented command exactly as §3.3 writes it,
and — after that run proved unable to reach the build phases — an **instrumented copy** whose only
delta is described in §3.6.3.

#### 3.6.1 Preconditions, live state

| # | Precondition | Live state |
|:--|:--|:--|
| 1 | slnx + version.props exist | **PASS** |
| 2 | Go toolchain on PATH | **PASS** — `go1.23.1 windows/amd64`, `C:\Program Files\Go\bin\go.exe` |
| 3 | `TestStdLibMetadataInSync` | **PASS** — `ok go2cs 0.161s`, **2.1 s** of wall clock including toolchain start. The "fails at second zero rather than after a full Release build" design works exactly as intended, and the gate is cheap enough to re-run at will |
| 4 | `git` available | **PASS** — `C:\Program Files\Git\cmd\git.exe`. Its absence-is-a-warning path was not exercised; §3.2's "treat as fatal by hand" stands unmeasured |
| 5 | gpg signing | `gpg.exe` present at `C:\Program Files (x86)\GnuPG\bin\gpg.exe`, but **NOT exercised — no dry run can exercise it**, because the tag mint is gated on the bump. See D6 |
| 6 | `docs/validation/1.23.1.7` absent | **PASS** — verified absent. `docs/validation/` holds `1.23.1.2`…`1.23.1.6` + `current/`. Do not pre-create it |
| 7 | `-SkipBuild` not passed | n/a |
| 8 | `NUGET_API_KEY` | not reached (push gate only) |

**Linux-runs-first: CONFIRMED by observation**, not by reading — the first `==>` build line names
`[linux-x64]`. And the release's flagged risk is green **today**: the full `-p:GoTargetOS=linux`
Release build of `go2cs-stdlib.slnx` completed with **0 errors in 511.5 s**.

**Disk.** `C:` 97.9 GB free, `D:` 439.6 GB free. The run consumed **6.5 GB** of Release build output
under `src/core/**/{bin,obj}` plus **0.16 GB** of packages — budget ~7 GB on the repository volume.

#### 3.6.2 ⚠ THE DOCUMENTED DRY RUN CANNOT RUN — it dies at ~8 s, before it builds anything

`.\push-nuget.ps1`, exactly as §3.3 writes it, **throws at line 300 after 7.9 s**:

```
==> Validation snapshot 1.23.1.6 already exists (write-once) -- keeping it
==> Retargeted 0 README badge link(s) to 1.23.1.6
Green badge in src\core\archive\tar\README.md links a proof page that was not snapshotted:
docs\validation\1.23.1.6\archive.tar.md
```

The mechanism is entirely deterministic, and it is a property of the CALENDAR, not of the tree:

1. a pack-only run does not bump, so `$fullVersion` is **1.23.1.6** — the LAST-PUBLISHED version;
2. `$versionProofs` is therefore `docs/validation/1.23.1.6`, which **already exists**, so the freeze
   takes the "keeping it" branch and the snapshot is never refreshed;
3. the green-badge verifier then checks all **162** green badges against that **126-page** frozen
   snapshot — and **36 packages have banked since the 1.23.1.6 release**, so 36 badges link a page
   that directory will never contain. `archive/tar` is merely the alphabetically first.

162 − 126 = 36. The 36 are correct badges: `archive/tar`, `crypto/tls`, `encoding/json`, `fmt`,
`go/types`, `html/template`, `os/exec`, `text/template`, and — the five most recently banked —
`runtime/debug`, `log`, `flag`, `net/mail`, `sync/atomic`, among the rest.

**Nothing is broken, and the release run is unaffected**: with `-Push` the bump makes `$fullVersion`
1.23.1.7, the freeze copies all 162 `current/*.md` into a directory that does not yet exist, and every
badge then verifies against a page that is there. The *dry run* is the only casualty — and it is a
casualty every time at least one row banks between a release and the next dry run, which is the normal
state of this campaign.

⚠ **Correction to §3.3.** Its closing claim — *"Both badge retargets are no-ops at the current
version, and both verifiers still run, so a pack-only run **is** a real badge-consistency check"* — is
true only while the roster has not moved since the last publication. Once it has, the green verifier is
**guaranteed** to throw and the pack-only run measures nothing past its first eight seconds. The
statement was reasoned from source and never executed; this is what executing it says.

> **RESOLVED 2026-08-22.** The script now gives a pack-only run its own would-be snapshot (a temporary
> directory, never the tree), so the freeze branch executes and the verifier checks today's badges
> against today's pages. §3.3 has been rewritten to the fixed behavior; the claim it makes is now true
> by construction rather than by calendar luck. The eight-second failure above is the **pre-fix**
> record and is kept as the mechanism it documents.

#### 3.6.3 The instrumented rehearsal — the release-morning shape

To exercise phases 5–13 the run needs the FREEZE branch, which only a not-yet-existing version
directory reaches. The rehearsal therefore used a copy of `push-nuget.ps1` with **exactly one changed
line** — `$validationDir` pointed at a scratch root on `C:` seeded with `current/` alone (verified by
diff: 1 file changed, 1 deletion). Nothing else differs, and the copy was deleted afterward. Consequence:
the freeze, both retargets and both verifiers execute precisely as they will on release morning.

Its one artifact is the sharpest result of the rehearsal, so it is stated plainly: the `.csproj`
`Exists()` guard reads the **repository's** `docs/validation/<version>/`, which the redirect does not
move — so the packed `VALIDATION.md` count reflects the real 126-page `1.23.1.6` directory. §3.6.5
turns that into the demonstration.

#### 3.6.4 The fifteen phases, measured

| # | Phase | §3.4 expectation | Observed | |
|:--|:--|:--|:--|:--|
| 1 | metadata gate | passes | `ok go2cs 0.161s` — **2.1 s** | PASS |
| 2 | bump | n/a for pack-only | correctly skipped | gated off |
| 3 | version | `1.23.1.6 (solution: go2cs-stdlib.slnx)` | exact | PASS |
| 4 | tag | minted pre-build | correctly skipped — ⚠ **wrong tag named**, see D2 | gated off |
| 5 | `Froze N` | **N == roster rows** | **Froze 162**; roster is 162 | **PASS** |
| 6 | green badges | **N == roster rows** | Retargeted **0** (no-op at current version), **Verified 162** | **PASS** |
| 7 | C# Source badges | throws on any README lacking one | Retargeted **0**, **Verified 305** | **PASS** |
| 8 | L3 derivation | from the corpus | **37** packages | PASS |
| 9 | two Release builds, linux first | both clean | linux **511.5 s**, windows **508.1 s**, order reversed as documented | **PASS** |
| 10 | per-RID counts | **must be equal** | **307 / 307**, id sets identical | **PASS** |
| 11 | flavor comparison | **P must be 0** | 37 of 37 L3 differ; of 270 neutral **0 differ materially**, 216 identity-only | **PASS** |
| 12 | merge | M == L3 count | **Merged 37**, copied **270** verbatim | **PASS** |
| 13 | `Packed N` | == per-RID count | **307** | **PASS** |
| 14 | push | n/a | gate reached: `Pack-only (default)` | correct |
| 15 | done | n/a | **exit 0 at 1,095.2 s** | — |

The 216-of-270 identity-only figure reproduces the design's own increment-4 measurement quoted in the
script's comment, on the artifacts actually packed.

**Timing table for this machine** — the number §3.3 most needs:

| Segment | Wall |
|:--|--:|
| preflight, phases 1–8 (gate, freeze, both retargets, both verifiers, L3) | **5.5 s** |
| `[linux-x64]` Release build (307 projects, `--no-incremental`) | **511.5 s** |
| `[linux-x64]` pack | 32.0 s |
| `[win-x64]` Release build | **508.1 s** |
| `[win-x64]` pack | 32.7 s |
| read flavors + compare + merge | 5.3 s |
| **total** | **1,095.2 s — 18.3 min** |

#### 3.6.5 Outputs verified

* **307 merged `.nupkg`**, 307 per flavor under `_flavors/`, 0.16 GB total — matching the solution's
  307 projects.
* **Multi-RID shape**, `go.os`: `lib/net9.0/os.dll` **463,360 B** (the Windows reference flavor),
  `runtimes/win-x64/lib/net9.0/os.dll` 463,360 B, `runtimes/linux-x64/lib/net9.0/os.dll` **464,896 B**.
  The reference flavor is duplicated into its own RID folder rather than left to fall back, exactly as
  the script's comment states.
* **Neutral packages are byte-identical to the reference pack** — `go.sort` merged and `_flavors/win-x64`
  hash to the same SHA-256. "The Windows lane cannot regress through the merge" is verified, not assumed.
* **Dependency union is real and correct.** `go.os`: 19 (win) / 21 (linux) / **23 merged = 23 union**,
  win-only `go.internal.godebug`, `go.internal.syscall.windows`; linux-only `go.internal.byteorder`,
  `go.internal.goarch`, `go.internal.stringslite`, `go.internal.syscall.unix`. `go.syscall`: 15/15/**18**.
  `go.net`: 25/25/**26**. Every merged set equals the union of its flavors.
* **The tree is left holding the Windows flavor** (`os.dll` on disk = 463,360 B). The reversed build
  order does what §3.4 phase 9 says it does.
* **No side effects**: no tag created, `version.props` unchanged, zero READMEs modified.

**⚠ The proof-snapshot silent failure, reproduced end to end.** Of the 307 merged packages, exactly
**126 carry a `VALIDATION.md`** — precisely the 126 pages in `docs/validation/1.23.1.6`. The other
**36 validated packages pack nothing at all**, with no warning, no error, and **exit 0**. That is the
`Exists()`-guard silence §3.5 point 3 names, and it is why phase 5's count check exists. On release
morning the freeze writes all 162 pages *before* the build, so all 162 guards are satisfied — but the
arithmetic is worth checking rather than trusting:

```powershell
# after the run: the count that must equal the "Froze N" line
(Get-ChildItem src\artifacts\nupkg -Filter *.nupkg | Where-Object {
    $z=[IO.Compression.ZipFile]::OpenRead($_.FullName)
    $hit=[bool]($z.Entries | Where-Object FullName -eq 'VALIDATION.md'); $z.Dispose(); $hit }).Count
```

Set arithmetic confirmed on this tree: roster rows == `current/*.md` == green-badge READMEs == **162**,
with **zero** set difference in either direction. Release morning's freeze will satisfy every badge.

#### 3.6.6 Deviations found

**D1–D4 were fixed on 2026-08-22** (§3.6.8); D5–D7 are residue and confirmations, not defects.

* **D1 — §3.3's dry run is unrunnable whenever a row has banked since the last release.** §3.6.2. The
  checklist's recommended inspection path is, today, an eight-second failure. A dry run that reaches the
  build phases needs the script to be able to freeze a *proposed* version; no such affordance exists.
  **FIXED** — the affordance now exists: a pack-only run freezes the would-be version into a temporary
  directory and verifies against it.
* **D2 — the pack-only skip message names the wrong tag.** It prints `the run that bumps mints
  nuget-1.23.1.6`, because `$releaseTag` (line 187) is composed from the **un-bumped** `$fullVersion`.
  The run that bumps mints `nuget-1.23.1.7`. Cosmetic, but it misinforms at exactly the moment someone
  is checking §3.1's version arithmetic. **FIXED** — a dry run names the would-be tag. The
  `-Push -BumpBuild:$false` branch keeps the old wording deliberately (release path, left byte-identical) -- RULED CORRECT AS-IS (coordinator, 2026-08-22): a no-bump re-push re-publishes the existing version, so the existing tag is precisely the right name;
  it carries the same imprecision and is flagged in §3.6.8 as an open, cosmetic residue.
* **D3 — §3.3 over-budgets by ~6x.** "Budget hours on a slow host" measures **18.3 minutes** on the
  fleet's slowest machine. Two Release builds dominate at ~510 s each; everything else is seconds.
  **FIXED, and then re-scoped** — §3.3 now carries BOTH measurements and budgets from the top: this
  section's 18.3 min is the *warm* figure, and §3.6.8 measures **46.8 min** on a cold tree. Against the
  cold number the original "hours" was an over-estimate by ~2×, not ~6×.
* **D4 — §3.3's `D:` disk warning does not reproduce.** Re-measured 2026-08-22: **0.65 ms per 4 KB file
  on `D:`** against 0.82 ms on `C:` — no penalty at all, versus the 1,606 ms / 2,140× the census
  recorded. That was a state of the disk, not a property of it; the release may run from the repository's
  own volume. Keep the free-space check, which is a different concern. **FIXED** — §3.4's disk paragraph
  now states it as a measured state rather than a property, and says to re-measure.
* **D5 — precondition 5 (gpg) cannot be rehearsed by any dry run**, since the mint is bump-gated. §6's
  contradiction note is now confirmed by execution rather than by reading. Residue, not a defect.
* **D6 — 303 of 306 library `.csproj` carry the `VALIDATION.md` pack block.** The three without are
  `golib`, `testing` and `unsafe` — all hand-owned, none on the roster. Recorded so a future audit does
  not chase the 303-vs-306 gap.
* **D7 — phase 11's wording.** §3.4 frames the line as "P differ materially" with P required to be 0;
  the emitted line reads `0 differ materially, 216 differ only in the deterministic-identity fields`.
  No deviation — recorded as a confirmation that the expected reading is the observed one.

#### 3.6.7 Go/no-go residue for release morning

1. **Prove gpg signing before starting**, since nothing else can: `git tag -s rehearsal-gpg -m x` then
   `git tag -d rehearsal-gpg`. Seconds, and it removes the one precondition this rehearsal could not touch.
2. **Re-run the metadata gate after any corpus work** that lands between now and the release. It is 2 s,
   and it is the only gate that fails before a build is spent.
3. **Re-derive phase 5's count on the day.** It is 162/162/162 now and moves with every bank; the
   invariant is roster rows == `current/*.md` == green-badge READMEs == the `Froze N` line.
4. **Do not pre-create `docs/validation/1.23.1.7`** — precondition 6 throws on it, and it is absent today.
5. **~7 GB free on the repository volume**, and expect ~20 minutes on a machine of this class if the
   tree is already built — **~50 minutes if it is not** (§3.6.8 measured 46.8 min cold). Budget cold.
6. **Unassessed by this lane**: §3's timing ruling (all four parity-close triggers) and §5's consistency
   preflight. Both remain owed before the tag flies.

#### 3.6.8 D1–D4 fixed, and the dry run re-executed green (2026-08-22)

D1 and D2 were script defects and D3/D4 were prose defects; all four are fixed. The dry run was then
executed again, **unmodified and uninstrumented**, on the same tree state that broke it (162 roster
rows, a 126-page last-published snapshot) — the exact condition §3.6.2 says is guaranteed to throw.

**What changed in `push-nuget.ps1`.** A pack-only run now computes the version a bumping run *would*
publish and freezes `docs/validation/current/` into a **temporary** directory named for it, verifies
every green badge against those pages, and deletes the directory. `$fullVersion` is unchanged — it is
still the version the run packs and still what the README retargets compare against — so **only the
proof-page location moves**, and nothing is written into the tree. The would-be version also corrects
the tag the skip message names (D2).

**The release path is untouched, by construction.** The affordance is gated on
`$dryRun = (-not $Push) -and (-not $doBump)`, which is `$false` on every `-Push` run whatever its bump
setting — so a re-push of the current version (`-Push -BumpBuild:$false`) still freezes and verifies
against the real tree, and a `-BumpBuild`-without-push still writes the real write-once snapshot. Each
of the six touched sites resolves to its pre-existing form when `$dryRun` is `$false`, including the
console strings; the script carries the site-by-site proof as a comment beside the predicate.

**Failing-first, then green.** The unfixed script was re-run first to reproduce the defect: it threw at
**7.3 s** on `archive/tar`, exactly as §3.6.2 records. The fixed script then ran all fifteen phases to
`exit 0`.

| # | Phase | Observed on the fixed run | |
|:--|:--|:--|:--|
| 1 | metadata gate | `ok go2cs` | PASS |
| 2 | bump | correctly skipped | gated off |
| 3 | version | `1.23.1.6` — the version packed, unchanged by the fix | PASS |
| 4 | tag | correctly skipped, naming **`nuget-1.23.1.7`** (D2 fixed) | gated off |
| 5 | `Froze N` | **162** into the temporary would-be `1.23.1.7` directory; roster is 162 | **PASS** |
| 6 | green badges | Retargeted 0, **Verified 162** — was an 8-second throw before the fix | **PASS** |
| 7 | C# Source badges | Retargeted 0, **Verified 305** | **PASS** |
| 8 | L3 derivation | **37** packages | PASS |
| 9 | two Release builds, linux first | both **0 errors**; order reversed as documented | **PASS** |
| 10 | per-RID counts | **307 / 307** | **PASS** |
| 11 | flavor comparison | 37 of 37 L3 differ; of 270 neutral **0 differ materially**, 216 identity-only | **PASS** |
| 12 | merge | **Merged 37**, copied **270** verbatim | **PASS** |
| 13 | `Packed N` | **307** | **PASS** |
| 14 | push | gate reached: `Pack-only (default)` | correct |
| 15 | done | **exit 0 at 2,810.8 s** | — |

Every count reproduces §3.6.4's instrumented rehearsal exactly — 162/305/37/307/307/37/270/216 — which
is the point: the fix changed *where* a dry run's proof pages live and nothing else. The wall time did
not reproduce (2,811 s against 1,095 s) because this run was made in a **fresh worktree with nothing
built**; that cold number is now the one §3.3 budgets from.

**Side effects, checked after the run:** `docs/validation/1.23.1.7` **absent**; `GoBuildNumber` still
**6**; zero READMEs and zero `version.props` modifications; no `go2cs-dryrun-proofs-*` directory left
under the system temp path.

**Residue, deliberately not fixed.**

* The `-Push -BumpBuild:$false` branch still prints the old skip wording, which carries D2's same
  imprecision. It is a release path and this fix was scoped to leave every release path byte-identical,
  so correcting it is a separate, cosmetic change for the user to rule on.
* A dry run still packs only the last release's `VALIDATION.md` count (§3.6.5). Closing that would mean
  writing the would-be snapshot into the tree, which is exactly what the write-once rule forbids —
  §3.3 states it as a dry-run-only property instead.
* The `-WhatIf` improvement noted in the script's comment (a pack-only `-WhatIf` no longer declines the
  temporary freeze and then fails verification) is **reasoned from source, not executed** — the same
  caveat §3.3 attaches to its other `-WhatIf` claims.

---

## 4. DRAFT — the README/NEWS milestone text — USER-OWNED

> **⚠ DRAFT FOR REVIEW. No lane applies this.** The main README update is explicitly user-owned
> ritual (charter §10, push policy: *"The user's ritual moves to the README.md update"*; the
> doc-update-cadence rule batches NEWS/README rather than touching them per package). This section
> exists so the text is ready when the user wants it, not so it can be landed.
>
> Style, per the docs convention: **present tense for visitors — state what IS.** History lives in
> the news archive, the board and the tag annotation. No changing-corpus figures beyond the
> milestone's own numbers, which stay as placeholders until the crossing supplies them.

### 4.1 `docs/README.md` — replace the NEWS block

The block is the file's `## 📰 NEWS — <headline>` section, ending with the standing
`**➡ All announcements can be found in the [go2cs News Archive](NEWS.md).**` line, which stays.

```markdown
## 📰 NEWS — Over 75% of Go's standard library validates in C#

**<ROWS> of the 215 testable standard-library packages pass their own Go test suites in C#** —
<VERDICTS> matching verdicts against `go test -json`, compared verdict for verdict, with
<DISCLOSED> divergences disclosed by exact failure signature and nothing else waived. A package
appears on the [roster](ValidatedTestPackages.md) only when *every* eligible test agrees, and
every row links a [proof page](validation/index.md) listing Go's verdict beside go2cs's, test by
test. Converted frames now report **Go file and line positions** — `runtime/debug.Stack()`,
`log`'s file prefixes and `flag`'s error output name the Go source they came from, not the
generated C#. Channel direction and array dimensions ride the type descriptor, so `reflect`
answers about them the way Go does. And the numbers are the honest ones: packages that come
close and do not fully agree are not on the roster, because a milestone you can trust is worth
more than one you can round up to. Full detail in the
[news archive entry](NEWS.md#<anchor>).

**➡ All announcements can be found in the [go2cs News Archive](NEWS.md).**
```

### 4.2 `docs/NEWS.md` — new archive entry, newest first

```markdown
## <Month D, YYYY> — Over 75% of the standard library's test suites pass in C#

**<ROWS> of the 215 testable standard-library packages (<PCT>%) validate their own Go 1.23.1 test
suites in C#** — <VERDICTS> matching verdicts against a clean `go test -json` baseline, with
<DISCLOSED> signature-pinned disclosed divergences. Each suite is converted from Go's own
`_test.go` sources, built against the converted standard library, run under the Go-semantics test
host, and compared verdict for verdict; a package joins the roster only when every eligible test
agrees.

The milestone is tagged because something genuinely hard landed with it. **A converted stack frame
now reports a Go file and line.** The position is derived from conversion-time facts rather than
composed — file and line ship together, because either alone names a position that exists in
neither tree — and the identity is build-shape-faithful: the published standard library reports
the same trimpath form Go bakes into a published binary, while a converted user module reports the
source path Go would have baked for that build. `runtime/debug`, `log`, `log/slog` and `flag` all
rest on it.

Three more capabilities landed alongside. **Channel direction and map key/element dimensions ride
the type descriptor**, so `reflect` distinguishes `<-chan int` from `chan int` and a decode target
can measure an array it has never populated — which retired an entire disclosure class on its own
recorded remedy and banked both template packages. **A native pointer slot holds the pointer's
value**, not a managed reference, closing a round-trip that had also hidden a GC-invisible
dangling-reference hazard. And **an always-on GC recorder** supplies real pause and release facts
to `runtime/debug`'s statistics surface at zero cost per read.

What the campaign declined is part of the record. A disclosure names something the managed runtime
provably cannot satisfy — never something merely unimplemented — so packages that reach 105 of 106
or 210 of 267 stay off the roster with their remaining root named and priced. Four disclosure
classes carry the campaign's divergences today; a fifth retired itself the day its remedy landed,
exactly as it was written to.

This is Go 1.23.1's terminal validation marker. Every roster row re-derives from a release's own
test sources at a version hop, so the campaign continues on Go 1.23.12 — the release users would
actually choose — with the 1.23 story living on its own branch.
```

### 4.3 `docs/README.md` — Milestones table row

Append in date order, matching the existing columns
(`| date | linked headline | commit · tag | one-sentence summary |`):

```markdown
| <YYYY-MM-DD> | [**Over 75% of the standard library's test suites pass in C#**](NEWS.md#<anchor>) | [`stdlib-tests-75pct-<YYYY-MM-DD>`](https://github.com/ritchiecarroll/go2cs/releases/tag/stdlib-tests-75pct-<YYYY-MM-DD>) | **<ROWS>/215** packages, <VERDICTS> matching verdicts, <DISCLOSED> disclosed; converted frames report Go file:line positions; Go 1.23.1's terminal validation marker, with `release/go1.23` cut. |
```

---

## 5. Consistency preflight — run before tagging

Six checks, each with the exact command or file. All six pass on the tree this package was prepared
against; the commands are the ones that were used to establish that, not approximations of them.

### 5.1 Roster table sums equal the roster header

The header's three figures are recomputed from the table itself, so a banking commit that updates
one and not the other is invisible until a sweep disagrees.

```bash
awk -F'|' '/^\| \[`/ {n++; gsub(/ /,"",$3); t+=$3; gsub(/ /,"",$4); if($4!="") d+=$4} \
  END {print "rows="n, "tests="t, "disclosed="d}' docs/ValidatedTestPackages.md
```

Compare against the `> ### Phase 4 progress:` header block. **Verified at preparation time:**
`rows=159 tests=18533 disclosed=79`, matching the header's *159 / 215 — 74.0% · 18,533 matching test
verdicts · 79 disclosed* exactly. Also confirm the percentage: `rows / 215`.

### 5.2 Proof-page index completeness

Three counts must agree — roster rows, living proof pages, and index rows:

```bash
ls docs/validation/current/*.md | wc -l            # living proof pages
grep -c '^| `' docs/validation/index.md            # rows in the proof index
```

**Verified at preparation time:** 159 and 159, against 159 roster rows. A row banked without its
index entry publishes a package whose badge links a page the index does not list; a page without a
row is a stale proof.

### 5.3 Badge states

Every validated package's README carries a green Tests badge; the count must equal the roster:

```bash
git grep -l 'badge/Tests-[0-9]\+%2F[0-9]\+_validated-brightgreen' -- 'src/core/**/README.md' | wc -l
```

**Verified at preparation time:** 159, equal to the roster row count. Note the badge's *link* still
points at the last published version (`1.23.1.6`) — that is correct and expected; `push-nuget.ps1`
retargets it at release and then re-derives and verifies every badge against the frozen snapshot
(§3.4 phases 6–7). The preflight's job is only that the badge **exists** for every banked row.

⚠ Census with `git grep` or a filesystem walk, never a bare `rg` over `src/core` — a default ripgrep
honors `src/core/.gitignore` and under-counts.

### 5.4 The endraw guard on both append-only docs

The Jekyll/Liquid raw guard must remain the first line and the endraw tag the **final** line
of both the board and the roster. (The tags are deliberately NOT spelled with their brace syntax
here: inside a raw-guarded doc, a quoted endraw tag TERMINATES the guard — the defect that took
this very file's Pages build down on 2026-08-22.) An append landing after the closer takes the
Pages build down —
which has happened once, and the board's own last line says so.

```bash
head -1 docs/phase4/BOARD-next-validation-candidates.md; tail -1 docs/phase4/BOARD-next-validation-candidates.md
head -1 docs/ValidatedTestPackages.md;                   tail -1 docs/ValidatedTestPackages.md
```

**Verified at preparation time:** both files open with the raw guard and close with the endraw.

### 5.5 Pages build green

There is **no local Pages instrument and no CI workflow** in this repository (`.github/workflows/`
does not exist; `docs/_config.yml` configures a remote-theme Jekyll build that GitHub runs on push).
So the honest check is two-part:

1. §5.4's guard pairing on every Liquid-guarded doc — the only failure mode that has actually broken
   the build (`git grep -l endraw -- docs/` lists them: the board, the roster,
   `ConversionStrategies-Reference.md`, `Glossary.md`, `DESIGN-multiplatform-corpus.md`,
   `DESIGN-package-ancestry-view.md`).
2. After pushing, confirm the Pages deployment succeeded on GitHub before the announcement in §4
   goes out. The tag does not depend on it; the *announcement* does, because every link in §4 points
   into the built site.

### 5.6 Working tree and banked-row protection

```bash
git status --porcelain     # EMPTY
```

And confirm the final banking merge had its **post-merge filtered sweep at the merge result** (§2
step 0). The `crypto/tls` regression is the recorded precedent: a row green on its lane tip was red
at master the moment its merge landed, because the guilty change had merged after the lane forked —
each side green alone, the union never swept.

---

## 6. What this milestone does **not** include

Named here so the tag is not held hostage to any of them. None is a precondition for the crossing;
each rides just behind it, and each has a home already.

| Item | What it is | Where it lives | Why it can wait |
|:--|:--|:--|:--|
| **The time-class leveling rebank** | `time`'s banked test sources are born-stale and have surfaced in three separate lanes; the same class holds `encoding/base64`'s directional-channel initializer (the `cargo-recv` emission postdates that bank) and `database/sql/driver`'s `package_test_info.cs` alias block, plus the seven `runtime` `unsafe.Pointer` box-compare sites flagged for the next leveling regen | BOARD, standing "born-stale, restore rather than level" rule; the ReadMemStats harvest's small queue | Born-stale rows **re-validate at their banked counts** — the staleness is emission drift, not a verdict change, so the sweep is unaffected. Each levels at its own rebank |
| **Map-coverage completion behind the dims-cargo arc** | The key/elem dims cargo landed with four measured positions; `encoding/gob` reaches 105 of 106 and the residual (`reflect.ArrayOf`/`StructOf`) is an arc with a price, explicitly *not* a disclosure | BOARD, *MAP KEY/ELEM DIMS ARE DESCRIPTOR CARGO* (2026-08-20) | It buys a row that is already off the terminal path; the cargo carrier it would extend is banked and guarded |
| **The `.slnx` registrations** | `core/math/big` and `core/runtime/debug` are in `go2cs.slnx`'s build closure via `GolibTests` but unregistered in the `.slnx` — one line each, deferred twice for lane conflict-avoidance | BOARD, ReadMemStats harvest queue item (1) | An unregistered member breaks only the Visual Studio solution build; every gate builds by path. Take it at a quiet point |
| **The `GO2CSPATH` durable fix** | The converter's child-environment `$(go2csPath)` case-insensitive race is root-caused and **harness-pinned** on Linux; the converter-side fix LANDED at `24797074c` (2026-08-22); the harness pin awaits its Linux re-measure retirement | BOARD (2026-08-21, lane `claude/linux-measure-1`); the harness pin is committed | The pin holds the measurement honest today. A converter change owes its own CNR, which is a gate cycle the crossing does not need to spend |
| **HashSet extraction** | `src/go2cs/HashSet.go` extracted to `github.com/ritchiecarroll/hashset` as the nugetgo proof of concept, both directions | [`../PLAN-nugetgo.md`](../PLAN-nugetgo.md) §6 | It is a converter-tooling proof of concept on a different plan's timeline; it touches no corpus row and no published package |

**Also deliberately outside this milestone**, for the same reason: the `.NET 10` hop (which owns
`host-limit`'s retirement via the single-file test host), the `1.23.12` hop, and the ж-box B′ design
increment — all commissioned for the 1.23.12 era by ruling, none of them a 1.23.1 obligation.

<!-- {% endraw %} — keep this the FINAL line: the Liquid guard opened on line 1 must close here. -->
