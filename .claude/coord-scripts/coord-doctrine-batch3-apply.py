#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
coord-doctrine-batch3-apply.py — apply doctrine batch 3 to the go2cs worktree.

Performs every edit in sections (B) and (C) of coord-doctrine-batch3-draft.md as EXACT-STRING
replacements. Read/write is io.open(..., encoding='utf-8', newline='') so line endings and BOM
state survive byte-for-byte; anchors and inserted text are authored with '\\n' here and converted
to each file's own line ending before matching, so the inserted text carries the same endings as
the file it lands in.

Safety model
  * every anchor must occur EXACTLY once (0 or >1 is a hard abort, naming file + edit + anchor);
  * edits are applied to an in-memory copy in list order, so an edit anchored on a previous edit's
    OUTPUT (INS-G2 depends on AMD-G1) validates correctly in --dry-run as well as --apply;
  * NOTHING is written unless every edit in every file resolved — a single miss aborts the run
    with no file touched.

Usage
  python coord-doctrine-batch3-apply.py             # dry run (default): report only, write nothing
  python coord-doctrine-batch3-apply.py --dry-run   # same, explicit
  python coord-doctrine-batch3-apply.py --apply     # write the files
  python coord-doctrine-batch3-apply.py --root <dir>
"""

import argparse
import io
import os
import sys

DEFAULT_ROOT = r"C:\Projects\go2cs\.claude\worktrees\musing-moser-d4552c"

CLAUDE = "CLAUDE.md"
HOP = os.path.join("docs", "GoCorpusMigration.md")
CI = os.path.join("docs", "CIMatrix.md")
ROSTER = os.path.join("docs", "ValidatedTestPackages.md")
TRACKER = os.path.join("docs", "phase4", "TRACKER-100-percent.md")
DARWIN = os.path.join("docs", "phase4", "FINDING-darwin-run-layer.md")
BOARD = os.path.join("docs", "phase4", "BOARD-next-validation-candidates.md")


def rep(old, new):
    """Replace `old` with `new`."""
    return (old, new)


def ins(anchor, text):
    """Insert `text` on the line(s) immediately after `anchor`."""
    return (anchor, anchor + "\n" + text)


# ---------------------------------------------------------------------------------------------
# (B) CLAUDE.md — 19 edits, in file order except that AMD-G1 precedes INS-G2 (INS-G2 is anchored
#     on the tail of the bullet AMD-G1 rewrites).
# ---------------------------------------------------------------------------------------------

EDITS = []

EDITS.append(("AMD-J", CLAUDE, rep(
r"""> hops: they **lead**, amended in-stage from lessons learned, and no plan or record overrides them on
> procedure. `docs/PLAN-*.md` hold ruled strategy and instance campaigns; their OQ rulings are
> settled, and only a new ruling reopens one.""",
r"""> hops: they **lead**, amended in-stage from lessons learned, and no plan or record overrides them on
> procedure. `docs/PLAN-*.md` hold ruled strategy and instance campaigns; their OQ rulings are
> settled, and only a new ruling reopens one — but a ruling's **SCOPE** is a claim about *who reaches
> the seam*, not a permanent property: the netpoll ruling's "zero runtime edits" covered
> `internal/poll`'s consumers and never runtime's own suite reaching `netpollGenericInit` through an
> `export_test` re-export, so a NEW consumer re-opens the scope question without re-opening the
> ruling (2026-09-01; the remedy was one honest no-op equivalence, both halves measured).""")))

EDITS.append(("OPT-1", CLAUDE, rep(
r"""> the go2cs-specific discipline: root-cause against the real emitted `.cs`/`.cs.target` (the golden is the
> authoritative record), keep the A/B footprint minimal, change *only* the goldens a fix must, and prove no""",
r"""> the go2cs-specific discipline: root-cause against the real emitted `.cs`/`.cs.target` (the golden is the
> authoritative record) and **read the emission BEFORE spending a gate battery — it is the cheapest layer
> and it keeps paying**, keep the A/B footprint minimal, change *only* the goldens a fix must, and prove no""")))

EDITS.append(("INS-Q", CLAUDE, ins(
r"""    heuristics above remain for the cases the tail cannot settle (a crash leaves no timeout
    event), but the tail is checked FIRST and quoted in any census that reports empty verdicts.""",
r"""    ⚠ Not every crash is tail-silent: a **module-init** death states itself there too — a
    `NotImplementedException` thrown from the host's static constructor is written into the tail
    verbatim (2026-09-01) — so a mass-empty on a flavor with no run layer is the same one-line
    read, not an inference.""")))

EDITS.append(("INS-B", CLAUDE, ins(
r"""  `MSBUILDDISABLENODEREUSE=1` was set; set it for any back-to-back `-tests` queue before believing a
  diagnostic-free build failure.""",
r"""  **Five more launch/instrument traps, each paid 2026-09-01/02.** (1) **Git Bash rewrites `cmd /c`
  into `cmd C:\`** — MSYS path conversion eats the `/c` — so the command never runs: `cmd` opens
  interactively, reads EOF, exits **0**. A "runner gate" passed that way with a log holding only the
  cmd banner; the EMPTY grep for its verdict line, not the exit code, is what caught it. Drive
  `cmd /c` from PowerShell (a `.ps1` launched by `powershell -File`) or set `MSYS_NO_PATHCONV=1`,
  and **grep a gate's log for its verdict line before believing "exit 0"** — route #6's shape,
  hand-typed. (2) **`robocopy` from Git Bash with forward-slash paths copies NOTHING and exits 1**,
  which is robocopy's SUCCESS code — a silent no-op that reads as a completed stage. (3) **The
  behavioral runner shells out to a BARE `dotnet`**, so the SDK must be on PATH: `DOTNET_ROOT`
  alone does not prevent NETSDK1045. (4) **Never locate a comparison binary by a recursive glob's
  first hit** — `Get-ChildItem -Recurse -Filter <name>.exe | Select -First 1` returned
  `bin\Release\Go\…` ahead of `bin\Release\net10.0\…` (G sorts before n), so a byte-identical
  289/289 "C# matches Go exactly" reading was ONE binary printed twice, and the runner reporting a
  real gap was right; name the TFM path, and positive-control an output comparison by making the C#
  side differ once. (5) **The LF-anchor trap is not converter-only** — harness C# is CRLF under the
  same `eol=crlf` pin, so an LF-anchored patch to `BehavioralRunner/Program.cs` matches zero times
  and the build that follows reports exit 0 with 0 errors *because the file was never changed*.
  (`strings` also cannot see a .NET UTF-16 literal: use `strings -el`, and check the checker against
  a literal known to be present.)""")))

EDITS.append(("INS-D", CLAUDE, ins(
r"""  attributes everywhere) — measured: exactly 1 Release assembly written corpus-wide vs a clean
  78-project filtered batch. "651 suspects" means "one project is red", not "the corpus is broken".""",
r"""- **⚠ An UNBANKED package's `-tests` assembly is in NO standing gate — route #7's shape, one
  assembly over (found 2026-09-01 by a lane's own sweep, by no gate).** CNR is transpile-only and
  the stdlib solution compiles PRODUCTION assemblies, so nothing at master ever builds the test
  emission of a package that has not banked: `reflect`'s `-tests` assembly sat compile-broken at
  master after a widened lift dedup bound a PUBLIC lifted struct's member shape to an INTERNAL
  prior lift — the dedup crossing ACCESSIBILITY tiers, CS0050/51/52 — with every standing gate
  green. **Standing amendment: any converter change touching lift identity, dedup registries or
  anonymous-type naming owes a `-tests -test-action build` of `reflect` at the MERGE RESULT, beside
  CNR.** The same hole has a second door: **the production-only two-seeded diff is blind to
  TEST-side emission** — a carrier stamp that dangled two banked rows lived in `x509_test.cs`,
  which `-stdlib` never writes, so the diff matched its prediction exactly and said nothing about
  the footprint that broke them. A converter change that emits CROSS-PACKAGE references therefore
  (a) lands its corpus footprint in the SAME train — the two-seeded diff applied verbatim,
  byte-identity asserted, exactly as a hand-own registration lands with its body — and (b) owes a
  `-tests` emission census of the banked rows it can reach, beside the `-stdlib` diff.""")))

EDITS.append(("INS-A", CLAUDE, ins(
r"""  reading trustworthy). Same family as the `grep -P` and bare-`rg` notes: an instrument that
  cannot fail reports success over a hole.""",
r"""  **⚠ The general form, named after five instances in ONE day (2026-09-01): an instrument built out
  of the thing under test cannot independently measure it — the corrective is a SECOND
  DERIVATION.** The sharpest instance is structural: **a `-stdlib` census answers "how much does the
  corpus change" and is BLIND to "does the fix reach the row" whenever the motivating site is in a
  `_test.go` file** — `-stdlib` never writes test emission, so those are two questions and they cost
  two runs. The others rhyme: a probe keyed on its own incomplete predicate reports the predicate;
  a probe blind to unwired slots reports the wiring; a type-name census was believable only once a
  `go/parser` derivation reproduced it, and a classifier's 66 only once an independent predicate
  reached the same number. Four corollaries, all measured 2026-09-01/02. **Name the LAYER a census
  is attached to**: a `claude/g-*` lookup over bare `g-*` refs returned a confident EMPTY, and a
  working-tree line-ending count under the `eol=crlf` pin was reported as a COMMITTED fact (the blob
  is LF, the checkout makes it CRLF, and `git checkout --` "cured" a state that was never in the
  index) — two retractions in one night, both from an unnamed layer. **An empty enumeration inside a
  redirected log is not evidence of absence** until a second instrument agrees. **An EMPTY diff
  after a "fix" is the fix saying it was not needed**, never the gate agreeing. And **count errors
  with the strict `error (CS|MSB|NETSDK)[0-9]+` pattern only** — a loose `grep -cE 'error '` scored
  1 error on a clean 831-assembly build by matching `internal.oserror ->`, and matched the word
  inside Go type names (`(…, error)`) where a case-sensitive `ERROR` read 0 against the loose
  grep's 140. ⚠ Finally, **a census attaches to the DEFECT's boundary — defined by the EXISTING
  marker set — not to the boundary the dispatch named**: a call-argument census missed two
  composite-literal sites the pointer twin's `anyBoxedPtrArgs` already marks, one of them a live
  defect. Grep the marker set first, and attach at every site it covers.""")))

EDITS.append(("INS-P", CLAUDE, ins(
r"""  any regex-bearing PowerShell instrument that embeds a converter glyph literally: run it against a
  known-populated target and confirm it finds a nonzero count before trusting a zero anywhere else.""",
r"""  **⚠ A SHARED PowerShell instrument owes a run on BOTH editions before it banks (measured
  2026-09-02).** `_roster.ps1`'s comparison reader took `Add-Type -AssemblyName
  System.Web.Extensions` — a genuine PS 5.1 case-folding fix, smoke-proven on Windows only, and
  .NET-Framework-ONLY. Under pwsh 7 the script died at its second block, so the sweep's three
  absorption arms (host-conditional, capability-absent, host-limit) silently **DECLINED on every
  Linux host**, and the catch's own message named the missing assembly rather than the missing
  capability — pointing at the wrong artifact, in the file that decides whether a row banks. The
  fix is an edition-conditional reader (Desktop keeps `JavaScriptSerializer`; Core uses
  `System.Text.Json.JsonDocument`, explicit, never `-AsHashtable` behaviour inherited from a newer
  host), and the guard exercises both. **Rule: 5.1 on a Windows lane AND 7 on a Linux lane — or the
  OS-matrix linux leg — before a shared `.ps1` change merges.**""")))

EDITS.append(("AMD-E", CLAUDE, rep(
r"""  Derivation is a grep of the GOROOT sources plus the roster's counts, both at gate time. As of""",
r"""  Derivation is a grep of the GOROOT sources for `reflect` as an **IMPORT** — a name-LIST match
  over-matches (`go/doc/comment`'s `std.go` carries the name as data), so positive-control the
  predicate before using it (`encoding/json` in, `cmp` out) — plus the roster's counts, both at
  gate time. As of""")))

EDITS.append(("INS-F", CLAUDE, ins(
r"""  `crypto/internal/nistec` re-enters as exactly that cost canary: run it and compare its WALL TIME
  against the recorded baseline, not just its verdict.""",
r"""- **⚠ The three-run flake standard, and the A/B a re-converting SWEEP silently invalidates
  (2026-09-01/02).** A row that fails once is not a finding: the standard is **fail-WITH the change,
  pass CLEAN, pass again WITH the change restored** — three runs, in that order, before anything is
  attributed to a commit. The strong form is what costs lanes: **reverting the `.cs` is not an A/B
  when the instrument re-converts.** `run-validated-sweep.ps1` re-emits from the LIVE binary, so a
  hand-reverted corpus file is overwritten before the row runs and both arms measure the same
  converter — which is exactly what happened on the h2 deadline rows, identical signatures on both
  sides reading as "the change is innocent". Swap the **PRESERVED pre-change `go2cs.exe`** into the
  sweep path instead, and state which binary each arm ran.""")))

# Coordinator correction 1: the script's $longTimeouts holds exactly TEN entries.
EDITS.append(("AMD-L", CLAUDE, rep(
r"""⚠ **EIGHT** packages carry per-package deadline FLOORS in the script's `$longTimeouts`""",
r"""⚠ **TEN** packages carry per-package deadline FLOORS in the script's `$longTimeouts` (re-counted 2026-09-02; `sync/atomic` 60m and `net` 40m joined since the eight)""")))

# Coordinator correction 2: no unverified build-constraint string.
EDITS.append(("AMD-K", CLAUDE, ins(
r"""  like a converter defect; it is an environment mismatch. On any Linux host with gcc (where cgo-on
  is the default), set `CGO_ENABLED=0` before converting against or regenerating the corpus.""",
r"""  ⚠ **It bites from the TEST side too, and there the state is PER-PACKAGE** (measured 2026-09-02 on
  a Linux lane as a one-variable A/B on `os/user`, whose Go file selection is cgo-conditional): a
  sweep converts under the session's `CGO_ENABLED`, so a cgo-ON run selects `_test.go` files the
  cgo-OFF corpus never carried, leaves untracked `cgo_*_test.cs` artifacts behind, and dies in the
  closure build in ~12 s with **zero verdicts** — a build failure that reads like a conversion
  defect. Both comparison sides must share ONE cgo state, and the converted side can only be the
  corpus's. `run-validated-sweep.ps1` pins it per package (`$cgoOffPackages`, beside
  `$longTimeouts`); a row whose file selection is cgo-conditional joins that table rather than
  depending on the session it happened to run in.""")))

EDITS.append(("INS-M", CLAUDE, ins(
r"""  reconvert and an untouched seed returns a normal-looking result with nothing marking it invalid
  (the emitted-before-seeded family, build-step edition).""",
r"""- **⚠ The bank unit for a converter change's corpus footprint is the two-seeded diff's HUNKS, never
  its FILE set (measured 2026-09-02).** Applying the A/B's ten whole files onto a corpus that is
  stale in OTHER families carries those families in with them: the whole-file application landed
  six relocation hooks into one `package_info.cs` while the file that declares them — byte-identical
  between the two binaries, so never flagged — still declared three, and the result was CS0111 ×3.
  Byte-identity to the new emission PASSED and an exact path-set assertion PASSED; neither can see
  a file the diff never named. The tell was arithmetic: **279 applied diff lines against 32
  measured**. Apply the change's OWN lines (the re-done application was 9 hunks / 24 lines, zero
  `GoPositionMap` and zero import-hook lines in the delta, with one untouched package as the direct
  control, and built clean everywhere); position maps and relocation hooks belong to the deliberate
  regen, not to a converter train. ⚠ **And COMMIT the corpus edit BEFORE any sweep** (paid twice in
  one day): a sweep wrapper's restore step (`git checkout HEAD -- src/core`) cannot distinguish your
  uncommitted work from the sweep's own dirt, so hand-applied hunks vanished between two rows and
  the second row failed **invalidly** — a phantom red. Ordering is the fix, not the script; re-run
  the application's own assertions after any restore.""")))

EDITS.append(("INS-C", CLAUDE, ins(
r"""  querying process: a `Where-Object { $_.CommandLine -like '*check-no-regression*' }` sweep matches
  the very command line performing the sweep, so it reports a phantom survivor and, if you kill it,
  kills your own shell.""",
r"""  **Three probe-hygiene rules from the same family (2026-09-01/02).** **Process AGE is read from
  `CreationDate` against `Get-Date`**, never against an assumed clock — a healthy three-minute-old
  run was killed in the belief it had been hanging for hours. **`pgrep -f <name>` matches its own
  wrapper's command line** — the bash edition of the self-match above — so a `while pgrep -f` wait
  loop spins forever on its own reflection after the child has exited; match on `/proc/*/exe`, i.e.
  the executable, never on a pattern that can match the process running the check. And **completion
  inferred from a SIDE EFFECT is not completion**: a file reverted because a running CNR had already
  transpiled it is a footprint, not an exit code — check the run.""")))

EDITS.append(("INS-O", CLAUDE, ins(
r"""to verify. The other src PowerShell utilities `clean-bin.ps1` (remove bin/obj/Generated) and
`set-version.ps1` each also have a `.bat` launcher.""",
r"""⚠ Purge with that instrument, or with an explicitly depth-UNLIMITED walk: an ad-hoc
`find … -maxdepth 3` purge missed 274 of 388 output directories and drove a lane's disk into the
harness's own free-space floor (2026-09-02).""")))

# AMD-G1 MUST precede INS-G2: INS-G2 is anchored on AMD-G1's replacement tail.
EDITS.append(("AMD-G1", CLAUDE, rep(
r"""- **Re-fetch immediately before any merge in a live campaign.** Refs move under you; arithmetic against
  a SHA you read ten minutes ago is arithmetic against a tree nobody has.""",
r"""- **Re-fetch immediately before any merge in a live campaign.** Refs move under you; arithmetic against
  a SHA you read ten minutes ago is arithmetic against a tree nobody has. ⚠ A rebase REWRITES a SHA
  someone else has already been handed: **never force-push a tip whose SHA has been posted — post
  the fresh SHA first** (paid twice in one day, 2026-09-01, the second time crossing a coordinator
  merge that was reading the old one).""")))

EDITS.append(("INS-G2", CLAUDE, ins(
r"""  the fresh SHA first** (paid twice in one day, 2026-09-01, the second time crossing a coordinator
  merge that was reading the old one).""",
r"""- **Three merge mechanics, measured 2026-09-01/02.** **Union CNR is never skipped on composition
  reasoning** — "both sides are transpile-clean, so the union is" is not a verdict, and the case it
  cannot see is exactly the one that bit: a merge carrying a NEW behavioral test cut from an older
  base went red at the union (the `CollidingPackageNames` red). **A conflict dry-run does not need
  `git merge-tree --write-tree`** — that subcommand is unavailable on this box's git; the form that
  works is a temporary-index `read-tree -m --aggressive -i <base> <ours> <theirs>` plus
  `git merge-file -p` over each unmerged path, a 3-way CONTENT check that never touches the
  worktree, so it is legal under the mid-battery source freeze. And **rebase equivalence is checked
  by TREE, not by commit list**: `git diff <merge-of-old-tip> <rebased-tip>` coming back EMPTY is
  what proves a running battery's verdicts transfer to a train rebuilt on new SHAs.""")))

EDITS.append(("AMD-N", CLAUDE, ins(
r"""  `go test ./...`, beside `projitemsIntegrity_test`) so the class turns into a red converter suite at
  the merge rather than a red corpus later.""",
r"""  ⚠ **And check what the check's WITNESS is made of.** A displacement guard whose witness is
  ON-DISK placeholders is ENVIRONMENT-dependent for TEST-side hand-owns: it passes on a tree that
  has run that package's `-tests` and fails on every clean clone, because an unbanked row has no
  committed test emission (2026-09-02). Ruled remedy: a GOROOT `_test.go` witness arm, matched by
  CLASS rather than by name and counted separately as the weaker witness; the production arm is
  unchanged.""")))

EDITS.append(("AMD-H", CLAUDE, ins(
r"""  description ("go pass vs C# fail") was wrong on the load-bearing detail and a rule built as
  described would have refused the very host it existed for.""",
r"""  Two more, 2026-09-02. **A control only tests the AXIS YOU VARIED**: eight plausible, well-formed,
  entirely wrong findings passed BOTH of a census's controls because every repro varied box-ref and
  none varied RECEIVER KIND — so list the axes the predicate actually reads, and vary each one in a
  control. **And a control that does not use the CALLER's input shape is not a control for the
  caller**: a helper's self-test passed lines with content while every real call site passes BLANK
  lines, and a `Mandatory [string[]]` parameter rejects an empty ELEMENT (`[AllowEmptyCollection]`
  does not cover it), so the helper threw on every real invocation while the step's verdict and its
  artifact both looked normal — a guard that could never go green, caught only because a dispatch's
  annotations arrived from something else. Test with the exact type and shape the call sites pass.""")))

EDITS.append(("AMD-I", CLAUDE, ins(
r"""  unexercisable branch in a guard is a false-green seed; deleting it with its evidence is the
  deliverable, not a loss.""",
r"""  **Its positive twin: a negative result is BANKED — in CODE at the gate, or in the RECORD**
  (both 2026-09-01/02). A measured-wrong next step recorded where the next reader will stand — the
  `MapIndex` follow-up marked *measured wrong: 0 fixed, 1 broken*, in the code at the site it would
  be attempted from — and a commissioned fix **cancelled with its measurement attached** each cost
  one line and save the next lane the whole attempt. The cancellation carries its own rule:
  **a predicate the converter already holds beats a metadata field nothing reads** (the proposed
  flag was dropped because an existing classifier already answers the same question, counts and all).""")))

# ---------------------------------------------------------------------------------------------
# (C) Runbook / board / other authority docs — 7 edits.
# ---------------------------------------------------------------------------------------------

EDITS.append(("DOC-HOP", HOP, rep(
r"""- **Classify emitted-vs-seeded by a sentinel modification time**, not by content: seeding puts every
  repository file in the staging root, so an overlay can never reveal a file the converter has
  *stopped* emitting unless the classification is time-based.""",
r"""- **Classify emitted-vs-seeded by a sentinel modification time**, not by content: seeding puts every
  repository file in the staging root, so an overlay can never reveal a file the converter has
  *stopped* emitting unless the classification is time-based. **A hop's corpus-side DELETION bill is
  a first-class number, and this classification is the only thing that can see it.** The 1.24 trial
  measured **31 files** — 28 whose principal Go file is gone, 2 build-tag flips (`sync/map.cs` among
  them), 1 other — and an unclassified stale sibling is not a diff but a COMPILE ERROR: the
  `aliastypeparams` baseline flip emits the `_on` file while the seed still holds the `_off` one,
  i.e. CS0102. State the bill with the emission census; do not discover it at the build.""")))

EDITS.append(("DOC-CI-1", CI, ins(
r"""separate, which is what makes a wall readable. Dispatching all three is three cheap clicks.""",
r"""
**A restricted-egress lane can dispatch — but not always by the same tool.** Dispatch capability is
per-TOOL, not per-account: on a container whose egress policy blocks the Actions blob domain,
`gh workflow run` returned **403** while the GitHub MCP `actions_run_trigger` returned **204** for
the same workflow, so a lane that cannot trigger one way tries the other before reporting the
workflow unavailable. The same policy blocks `go.dev` and every .NET CDN, so such a lane provisions
its own toolchains by other routes — the Go release as a **module** from `proxy.golang.org`, and
.NET from the distribution's own package rather than a Microsoft CDN. Those are facts about the
lane, never about this workflow: what it measures is unchanged, and the readable channel for the
results is the annotations route below.""")))

EDITS.append(("DOC-CI-2", CI, ins(
r"""result that stays in a GitHub run page is a result nobody has.""",
r"""
Relay a sweep row from its **comparison RECORD**, never from its pass line: the disclosed count is
not on the pass line, so a row summarized from it under-reports by exactly the disclosures — and a
disclosure count is half of what makes a row honest.""")))

EDITS.append(("DOC-ROSTER", ROSTER, rep(
r"""- **E2 — broken oracle.** Go's own suite fails on the reference side, so no clean differential
  baseline exists to compare the conversion against.""",
r"""- **E2 — broken oracle.** Go's own suite fails on the reference side, so no clean differential
  baseline exists to compare the conversion against. ⚠ **An E2 exclusion is only as durable as the
  HOST that measured it** — a failing oracle can be a property of one machine rather than of the
  target — so a **fleet-wide re-probe comes before any machinery is built on the exclusion**
  (`os/user`, 2026-09-01: the row banked from a host whose oracle probed clean; had no host been
  clean, the honest form was an oracle-side host-limit disclosure, not a standing exclusion).""")))

EDITS.append(("DOC-TRACKER", TRACKER, rep(
r"""> authority) and the mailbox record at update time — never carried forward. If this file and
> the roster disagree, the roster is right and this file is stale.""",
r"""> authority) and the mailbox record at update time — never carried forward. If this file and
> the roster disagree, the roster is right and this file is stale. ⚠ That applies to the **row
> tables**, not only to the header: a distance table carried forward while its rows banked elsewhere
> named two already-banked packages as unattempted and nearly sent a lane at them (2026-09-02).
> Re-derive every table from the roster at each touch.""")))

EDITS.append(("DOC-DARWIN", DARWIN, ins(
r"""Option 2 is the smaller surface and the better fit for the corpus's shape; it is recorded as an
observation, not a ruling.""",
r"""
**Amendment 2026-09-02 — the first casualty is pinned, and it sizes the keystone.** Read from
frames the runner now carries (through check-run annotations alone, no artifact download), a
converted program dies in **`syscall.init()` → `Getrlimit` → `rawSyscall`** — one package EARLIER
than this finding predicted, which named `os`'s static constructor. The minimum keystone to reach
`Main` is therefore **`rawSyscall` plus the `libc_getrlimit` trampoline**, and the consequence for
scoping is the useful half: **neither an `os`-only nor an `fmt`-only scope is the right unit** —
the entry point is reached before either package's own initialization runs. Option 2 above is the
shape this sizing favors; it remains an observation pending the owner's read.""")))

# Coordinator correction 3: the board is append-only and every append lands INSIDE the raw guard,
# so the entry goes at the TAIL — anchor is the file's final line, which is put back after it.
BOARD_GUARD = (
    r"""<!-- {% endraw %} — keep this the FINAL line: the board is append-only and every append must """
    r"""land INSIDE the raw guard, or Jekyll's Liquid chokes on quoted Go composite-literal syntax """
    r"""(this exact failure took the Pages build down at f37ba28ef). -->"""
)

EDITS.append(("BOARD", BOARD, rep(
BOARD_GUARD,
r"""## FINDINGS (2026-09-02) — two shapes worth meeting before the next bridge or method-value arc

**One rule, TWO minters — a bridge change can be green on the probe and wrong on the row.**
`reflect.rtype.Field(i)` mints its struct-field descriptor through its own `structFieldDescriptor`,
*beside* `abi.synthesizeStructType`, whose header states the one-rule invariant both are supposed to
honor. A change that substitutes at one minter and not the other passes a probe aimed at the other
and fails the verdict — so a descriptor/bridge change enumerates the minters before it measures,
and either substitutes at both or states why one is out of scope.

**The method-value family's fourth face has TWO mechanisms, not one** (4 of 17 sites red-first).
**M1** — the receiver EXPRESSION is deferred into the wrapper lambda, so any non-trivial expression
at a lambda site re-executes per call; this is **kind-independent**. **M2** — the root-ident
snapshot aliases through a REFERENCE-semantics base (the value-receiver lambda path), which is a
third axis the M1 predicate never reads. The pairing is the lesson: one commit needed two axes in
its control where it varied one, and the next needed two mechanisms where it saw one — the same
error in both directions. Both are covered by the evaluate-once ruling; recorded here so a future
census names both before it counts.

""" + BOARD_GUARD)))


# ---------------------------------------------------------------------------------------------
# Machinery
# ---------------------------------------------------------------------------------------------

def read_text(path):
    with io.open(path, encoding="utf-8", newline="") as handle:
        return handle.read()


def write_text(path, text):
    with io.open(path, "w", encoding="utf-8", newline="") as handle:
        handle.write(text)


def file_eol(text):
    return "\r\n" if "\r\n" in text else "\n"


def to_eol(text, eol):
    # Authored strings use '\n' only; convert to the target file's ending.
    return text.replace("\n", eol) if eol != "\n" else text


def snippet(text, width=72):
    first = text.split("\n", 1)[0]
    if len(first) > width:
        first = first[:width] + "..."
    return repr(first)  # repr keeps console output ASCII-safe


def main():
    parser = argparse.ArgumentParser(
        description="Apply doctrine batch 3 (dry run by default; --apply to write).")
    parser.add_argument("--apply", action="store_true", help="write the files")
    parser.add_argument("--dry-run", action="store_true",
                        help="report only, write nothing (the default)")
    parser.add_argument("--root", default=DEFAULT_ROOT, help="worktree root")
    args = parser.parse_args()

    if args.apply and args.dry_run:
        print("ERROR: --apply and --dry-run are mutually exclusive.")
        return 2
    writing = bool(args.apply)

    try:
        sys.stdout.reconfigure(encoding="utf-8", errors="replace")
    except Exception:
        pass

    mode = "APPLY" if writing else "DRY RUN"
    print("doctrine batch 3 -- %s" % mode)
    print("root: %s" % args.root)
    print("")

    # Preserve file order of first appearance, and edit order within each file.
    order = []
    grouped = {}
    for edit_id, rel, (old, new) in EDITS:
        if rel not in grouped:
            grouped[rel] = []
            order.append(rel)
        grouped[rel].append((edit_id, old, new))

    results = []   # (rel, original_text, new_text, edits_applied)
    misses = []    # (rel, edit_id, anchor, count)

    for rel in order:
        path = os.path.join(args.root, rel)
        if not os.path.isfile(path):
            misses.append((rel, "-", "FILE NOT FOUND: %s" % path, -1))
            print("  [MISSING FILE] %s" % rel)
            continue

        original = read_text(path)
        eol = file_eol(original)
        current = original
        applied = 0

        print("%s  (eol=%s)" % (rel, "CRLF" if eol == "\r\n" else "LF"))
        for edit_id, old, new in grouped[rel]:
            anchor = to_eol(old, eol)
            count = current.count(anchor)
            if count == 1:
                current = current.replace(anchor, to_eol(new, eol), 1)
                applied += 1
                print("  [OK]   %-10s anchor found once" % edit_id)
            else:
                misses.append((rel, edit_id, old, count))
                print("  [MISS] %-10s anchor occurs %d times -- %s"
                      % (edit_id, count, snippet(old)))
        results.append((rel, original, current, applied))
        print("")

    if misses:
        print("ABORTED -- %d anchor(s) did not resolve exactly once. NOTHING was written." % len(misses))
        for rel, edit_id, anchor, count in misses:
            print("  %s :: %s :: count=%s" % (rel, edit_id, count))
            print("      %s" % snippet(anchor, 100))
        return 1

    total_edits = 0
    total_lines = 0
    print("SUMMARY")
    for rel, original, current, applied in results:
        eol = file_eol(original)
        added = current.count(eol) - original.count(eol)
        total_edits += applied
        total_lines += added
        print("  %-46s edits=%-3d lines+=%d" % (rel, applied, added))
    print("  %-46s edits=%-3d lines+=%d" % ("TOTAL (%d files)" % len(results), total_edits, total_lines))
    print("")

    if not writing:
        print("DRY RUN -- no file was written. Re-run with --apply to write.")
        return 0

    for rel, original, current, _applied in results:
        if current != original:
            write_text(os.path.join(args.root, rel), current)
            print("  written: %s" % rel)
    print("")
    print("APPLIED -- %d edits across %d files, %d lines added." % (total_edits, len(results), total_lines))
    return 0


if __name__ == "__main__":
    sys.exit(main())
