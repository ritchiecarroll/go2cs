#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
coord-doctrine-batch4-apply.py — apply doctrine batch 4 to the go2cs worktree's CLAUDE.md.

Batch 4 = accumulator items 47-91 (items 1-46 landed as batch 3, commit 2ba31d3a3). Every edit is
an EXACT-STRING replacement: old = a verbatim anchor already in the file, new = anchor + inserted
text. Read/write is io.open(..., encoding='utf-8', newline='') so line endings and BOM state survive
byte-for-byte; anchors and inserted text are authored with '\\n' here and converted to the file's own
line ending before matching, so inserted text carries the same endings as the file it lands in.

Safety model
  * every anchor must occur EXACTLY once (0 or >1 is a hard abort, naming the edit and the anchor);
  * edits apply to an in-memory copy in list order, so an edit anchored on a previous edit's OUTPUT
    would still validate in --dry-run (batch 4 has no such dependency, but the machinery is kept);
  * NOTHING is written unless every edit resolved — a single miss aborts with no file touched.

Usage
  python coord-doctrine-batch4-apply.py                  # dry run (default): report only
  python coord-doctrine-batch4-apply.py --dry-run        # same, explicit
  python coord-doctrine-batch4-apply.py --apply          # write the file
  python coord-doctrine-batch4-apply.py <path> --apply   # explicit target path
"""

import argparse
import io
import os
import sys

DEFAULT_PATH = r"C:\Projects\go2cs\.claude\worktrees\musing-moser-d4552c\CLAUDE.md"


def ins(anchor, text):
    """Insert `text` on the line(s) immediately after `anchor`."""
    return (anchor, anchor + "\n" + text)


EDITS = []

# ---------------------------------------------------------------------------------------------
# Build / test workflow — converter flags
# ---------------------------------------------------------------------------------------------

# E01 — items 54a, 66 : TOOLCHAIN RESOLUTION bullet (wrong-release, and the boot-armed form)
EDITS.append(("E01-release", ins(
r"""  dependency set, minting phantom CS0426s that read as Linux defects — net-family Linux work routes
  through the SWEEP, always.""",
r"""  ⚠ **Fourth member — the RIGHT SPELLING of the WRONG RELEASE, and every existing GOROOT check is
  blind to it** (measured 2026-09-02, a cloud lane): on a box carrying side-by-side SDKs bare `go`
  resolved 1.24.7 while the corpus pins 1.23.12 — `go env GOROOT` stays self-consistent, the conversion
  succeeds and exits 0, and the spelling/namespace guards pass because nothing about the PATH is wrong.
  **A conversion against the corpus prints `go version` AND GOROOT before it runs.** It is armed at
  BOOT too: a stale `/etc/profile.d` lane script exporting an older `/usr/local/go/bin` beat the newer
  fleet file (profile.d sources alphabetically — a `zz-` prefix fixes it), and `wsl.exe -- bash -lc` does
  not source profile.d like a real login: verify by bare `go version` in a real login shell.""")))

# E02 — items 54b, 83 : the emit-beside-input trap has a SILENT repo-root form
EDITS.append(("E02-beside", ins(
r"""    blank line → the gate must go red → the restore must be byte-identical) — a gate diffing a
    seeded copy against its own source is a gate that cannot go red.""",
r"""    ⚠ **Paid twice more on 2026-09-02, and the REPO-ROOT form is the silent one.** A lane omitting the
    positional wrote 167 `.cs` into a GOROOT — loud, because GOROOT is not supposed to hold `.cs`; the
    same omission with the repo root as cwd left **41 untracked byte-identical copies of
    `src/core/strconv` in the REPOSITORY ROOT for eight hours**, invisible to every `| head`/`| grep`
    status check shaped around expected files. A FILTERED `git status` answers "did my change land";
    only an UNFILTERED `git status --porcelain`, read whole, answers "is the tree clean".""")))

# E03 — item 70 : a deadline raised with a byte-identical result is a BLOCK
EDITS.append(("E03-block", ins(
r"""    set before reading scattered as genuine divergence** — a set equality is one grep, and it is the
    difference between one root and 43 phantom findings.""",
r"""    ⚠ **A deadline RAISED with a byte-identical result is a BLOCK, not slowness** (measured 2026-09-02,
    `net` at 40m vs 60m: 501 terminal verdicts / 27 orphans on BOTH runs, both ending at the same test)
    — more budget cannot move a hang, and the stream's last `run` event is what places it. Compare the
    terminal and orphan SETS across the two deadlines before budgeting a longer one: equal sets mean the
    unreported names are one hang plus its serial tail and the parked parallel batch — the re-pricing
    shape (one test worth N verdicts), not N divergences.""")))

# E04 — item 76 : the timeout event can be an ESCAPED JSON string
EDITS.append(("E04-escaped", ins(
r"""    `NotImplementedException` thrown from the host's static constructor is written into the tail
    verbatim (2026-09-01) — so a mass-empty on a flavor with no run layer is the same one-line
    read, not an inference.""",
r"""    ⚠ And the tail read has its own false-empty: the event can be carried as an ESCAPED JSON
    string, so a substring count of `"action":"timeout"` returned **0** on a record whose tail states
    the kill (2026-09-02) — match the escaped form too, or parse the field.""")))

# E05 — items 52, 88, 69, 77 : the pipeline's record files
EDITS.append(("E05-record", ins(
r"""    durable half of the rule stands regardless: a stale results.json next to a fresh comparison
    is NOT a deadline kill; a gated/filtered census gates on the CAPTURED STREAM; and the cheap
    check is the results file's timestamp against the comparison's.""",
r"""    ⚠ **Three record-file rules, all measured 2026-09-02.** (1) A **gated** (`-test-filter`) run
    REWRITES the package's comparison record with nothing marking it gated — a harvest read
    `runtime/debug` as bankable off a filtered control's record, the only tell being 9 go entries where
    the full run had 10 — so after any gated diagnostic that record is poisoned for banking until an
    UNGATED run overwrites it. (2) A paired before/after measurement needs two FILES, not two runs: the
    record is git-ignored, so a branch restore cannot bring the "after" back and the baseline overwrites
    it in place — the diff then compares a file with itself and reads "zero moved"; copy each side's
    `results.json` to a distinct path first. (3) `git checkout HEAD -- src/core` + `git clean -fd` clears
    NONE of the pipeline's git-ignored state (`bin/`, `obj/`, the manifest, the comparison and results
    files), so a "restored" tree is WARM and a filtered run's record travels into the next one: delete
    the record files after every sweep, and state cold-vs-warm when comparing two runs.""")))

# ---------------------------------------------------------------------------------------------
# Test-harness mechanics
# ---------------------------------------------------------------------------------------------

# E06 — items 49, 91, 65 : an MSTest verdict WORD is not a verdict
EDITS.append(("E06-aborted", ins(
r"""  (clears stale hosts *before* the build — the lock manifests at build time — and runs with
  `--blame-hang`) over a bare `dotnet test`.""",
r"""  **⚠ An MSTest verdict WORD is not a verdict — an ABORTED run prints one anyway** (measured 2026-09-02,
  GolibTests on a Linux lane): the second-to-last line reads `Passed! - Failed: 0, Passed: 82` and the
  LAST reads `Test Run Aborted.`, against a declared count near 470 — the exit code is honestly 1, but a
  verdict-word grep reads green, and `$?` after a pipe is the LAST command's status (grep's), so a piped
  invocation captures the raw exit first. **A GolibTests gate greps for `Test Run Aborted` AND compares
  the run's Total against the DECLARED count (`grep -c '\[TestMethod\]'`)**; an abort is an UNMEASURED
  suite, never a pass — the tell was adding 7 tests and watching the total stay 82. Run it `--no-build`
  behind the solution leg, too: a `dotnet test` that BUILDS raced twice in one night on a spurious
  CS0234/CS0246 that was gone on `--no-build` against the build just completed.""")))

# E07 — items 58, 63 : route #8's sharper form (class order) and the fix shape
EDITS.append(("E07-order", ins(
r"""  case), never the artifact's text, and re-check a guard's negative arm whenever the construct it
  greps for legitimately relocates.""",
r"""  ⚠ **Route #8's sharper form KILLS the suite instead of going vacuous, and its verdict rides CLASS
  ORDER** (measured 2026-09-02, GolibTests): the guard's premise — "GolibTests does not reference
  converted `flag`" — was disarmed by a later `ProjectReference`, after which the converted `flag.Parse()`
  parsed MSTest's OWN command line through the process-global `flag.CommandLine` (`ExitOnError` →
  `os.Exit(2)`) unless a sibling class that replaced it with `ContinueOnError` happened to run first: one
  host's 460/460 was a lucky ordering, another's 82-then-abort the same defect. The fix SHAPE matters as
  much — the host parses its OWN args and **never mutates a process-global that converted tests read**
  (`os.Args` feeds `sync`'s `TestMutexMisuse`, `flag`'s `TestExitCode` and every self-re-exec): a
  divergence STATED against the ruling is how to diverge.""")))

# E08 — item 81 : route #7's attribution mirror (a faithful generated shell)
EDITS.append(("E08-nilrecv", ins(
r"""  (a) lands its corpus footprint in the SAME train — the two-seeded diff applied verbatim,
  byte-identity asserted, exactly as a hand-own registration lands with its body — and (b) owes a
  `-tests` emission census of the banked rows it can reach, beside the `-stdlib` diff.""",
r"""- **⚠ Route #7's ATTRIBUTION mirror: a crash INSIDE a generated shell is usually the shell being
  faithful** (measured 2026-09-02, runtime's `textAddr`). The `RecvGenerator` shell's
  DerefOrNull → NullRef → NRE on the first field touch IS Go's nil-receiver semantics; the nil came
  from `funcInfo()`'s module search, which can never succeed because the package's sole moduledata is
  a permanent empty stub (`len(pclntable)==0` skips it every time). A structurally guaranteed nil is
  not a race and not goroutine-specific — which tests crash is decided only by which ones reach the
  call at all. **Trace to the ASSIGNMENT, not the frame**, before billing `src/gen/`.""")))

# E09 — item 47 : a PowerShell function named `Git` shadows git.exe
EDITS.append(("E09-gitshadow", ins(
r"""  host), and the guard exercises both. **Rule: 5.1 on a Windows lane AND 7 on a Linux lane — or the
  OS-matrix linux leg — before a shared `.ps1` change merges.**""",
r"""  **⚠ And a PowerShell FUNCTION named `Git` shadows `git.exe`** — command names resolve
  case-insensitively, so `& git` inside it recurses until "call depth overflow" (measured 2026-09-02,
  coordinator). The overflow line, captured through `2>&1`, then counted as ONE dirty entry in a
  `status --porcelain` check and aborted a rebuild twice with a message that read like real tree
  dirt. Name wrappers distinctly, invoke `git.exe` explicitly, and take `status --porcelain` with
  stderr dropped.""")))

# E10 — items 48, 75b : relaunch overlap, and a slow status is not a hang
EDITS.append(("E10-relaunch", ins(
r"""  inferred from a SIDE EFFECT is not completion**: a file reverted because a running CNR had already
  transpiled it is a footprint, not an exit code — check the run.""",
r"""  **⚠ Two more, 2026-09-02.** **Relaunching a chain while its predecessor's TAIL leg is still alive
  puts two runs in one worktree** — a third rebuild attempt met the second chain's in-flight `reflect`
  `-tests` convert as untracked `*_test.cs` and aborted on its dirt gate, the r41 overlap hazard
  caught only because that gate existed: census live processes (and wait for the task notification)
  before relaunching anything into a worktree. And **the harness's own
  `git status --untracked-files=all` over a worktree full of `bin`/`obj` can run for an HOUR** —
  slow, not hung, and not evidence of anything else.""")))

# E11 — items 87, 90 : the -tests dimension, and re-deriving a census population
EDITS.append(("E11-census", ins(
r"""  marker set — not to the boundary the dispatch named**: a call-argument census missed two
  composite-literal sites the pointer twin's `anyBoxedPtrArgs` already marks, one of them a live
  defect. Grep the marker set first, and attach at every site it covers.""",
r"""  ⚠ **Two 2026-09-02 refinements from one census that read ZERO against thirteen real sites.** Every nil
  construction of pointer-to-array type in Go 1.23.12 lives in a `_test.go` (reflect 10, runtime/arena 2,
  encoding/binary 1), so the production census of 64 nil-to-pointer conversions found none: **ask the
  `-tests` dimension whenever the motivating site is a test.** Where three derivations disagreed (grep 6,
  an instrument pointed at the grep-NOMINATED packages 11, an independent `go/packages` pass over all std
  packages 13) the disagreement was SCOPE, not predicate: scoping a census with the tool just shown to
  under-report reproduces its blind spot. Then **re-derive the population before any design is cut against
  it**: the 13 split into three tiers (6, 3, 4) and the "most interesting" members were the tier that
  needs nothing — a summary restating a lane's conclusion inherits its unvaried axis, so state what was
  MEASURED, not what was concluded.""")))

# E12 — item 84 : derive the canary set in a clone whose refs you have verified
EDITS.append(("E12-canary", ins(
r"""  `crypto/internal/nistec` re-enters as exactly that cost canary: run it and compare its WALL TIME
  against the recorded baseline, not just its verdict.""",
r"""  ⚠ **Derive the canary set in a clone whose refs you have verified.** A derivation run with the
  mailbox clone as cwd read `origin/master` **15 rows behind** and produced the SAME top five by luck
  (every dropped row was smaller); only a row-count reconciliation — 178 against the guard's 193 —
  caught it (2026-09-02). The mailbox clone's non-mailbox refs are stale BY DESIGN and are never read
  for repo content; reconcile any derived count against an independent one before using it.""")))

# E13 — item 86 : the mid-battery freeze's SCOPE
EDITS.append(("E13-freeze", ins(
r"""  their cuts until the battery's summary prints; the coordinator announces battery start/close on the
  mailbox for exactly this reason.""",
r"""  ⚠ Scope, stated 2026-09-02 after a lane held a cut it never needed to: the freeze binds **the
  worktree the battery runs in, on any branch checked out THERE** — the runners rebuild `go2cs.exe`
  from that tree's disk and golib/gen compile into the projects that battery builds, so a lane
  editing its own clone on its own machine cannot reach a battery leg elsewhere on the fleet.""")))

# ---------------------------------------------------------------------------------------------
# Corpus mechanics
# ---------------------------------------------------------------------------------------------

# E14 — item 60 : IDENTICAL means nothing when the side was not written either
EDITS.append(("E14-identical", ins(
r"""    as differences (a confounded census nearly banked 60 false hits, 2026-09-01) — compare only
    paths BOTH conversions write, or classify by write-evidence first.""",
r"""    ⚠ Its MIRROR, measured 2026-09-02: **IDENTICAL means nothing when the side was not WRITTEN
    either.** A windows-default single-target reconvert reported ZERO diff on an L3 package's
    `linux/` files — the very files another lane had measured, under a linux-target conversion, as
    carrying four missing forced-init hooks. Classify by write-evidence PER TARGET, and measure an L3
    package with the three-target `-platforms` emission rather than the host default.""")))

# E15 — items 51, 56 : byte-identity is a property of the FILE; a hunk is the emission's STATEMENT
EDITS.append(("E15-hunk", ins(
r"""  one day): a sweep wrapper's restore step (`git checkout HEAD -- src/core`) cannot distinguish your
  uncommitted work from the sweep's own dirt, so hand-applied hunks vanished between two rows and
  the second row failed **invalidly** — a phantom red. Ordering is the fix, not the script; re-run
  the application's own assertions after any restore.""",
r"""  ⚠ Two corollaries measured 2026-09-02. **"Byte-identical to the emission" is a property of the FILE, not
  of the CHANGE** — copying the footprint files wholesale out of the NEW seeded root is byte-identical BY
  CONSTRUCTION and still wrong, carrying every arc not yet regen'd into the corpus (numstat read 3/9,
  13/31, 3/15, 5/6, 1/7 against a change that owns six lines); numstat is the cheaper instrument, and the
  strongest-looking provenance check cannot see the difference. And **a footprint hunk is the fresh
  emission's STATEMENT even when the committed statement carries another arc's unbanked drift**: carry the
  one inert, byte-verified foreign line and SAY in the commit which line belongs to which arc — a
  hand-written shape no converter emits is worse than one foreign line named. Read the COMMITTED bytes,
  never the seed, before cutting a footprint against a base.""")))

# E16 — items 57, 61 : control FORM (differential, and whose converter it assumes)
EDITS.append(("E16-controlform", ins(
r"""    compared root (see the single-package output-positional trap above) and only after the gate's
    negative control has been made to fail once.""",
r"""    ⚠ **Two control-FORM rules, both measured 2026-09-02.** An ABSOLUTE "byte-identical to the committed
    file" control is unsatisfiable under standing corpus drift — three banked rows' `-tests` emissions
    changed WITH a cut and WITHOUT it (closure drift plus relocation debt), so a no-op would have failed
    the gate; the DIFFERENTIAL form (emission with the change vs without) is the one that carries
    information, and the five-minute control (revert, re-emit) runs BEFORE any violated control is
    reported. And **a positive control's premise must hold at the CONVERTER the measurement used**: "the
    landed hunk must reproduce with zero diff" assumed a binary carrying the merge while the
    measurement's binary was built pre-merge — it failed for its premise, not for the instrument. Name
    the converter a control assumes, and say which form the control took.""")))

# ---------------------------------------------------------------------------------------------
# Performance suite
# ---------------------------------------------------------------------------------------------

# E17 — items 82, 73 : a swallowed package init, and the native control for a latency row
EDITS.append(("E17-perf", ins(
r"""  watchdog. `--no-aot` drops the whole column and stays fast. Keep each
  benchmark ≥50 ms and output deterministic (inline xorshift, no `math/rand`).""",
r"""- **⚠ Two measured 2026-09-02, both from the TLS-handshake row.** Verify found a **SEMANTIC** divergence
  before anything was timed: the converted `crypto/tls` negotiates ChaCha20-Poly1305 where Go negotiates
  AES-128-GCM on the same host, because `internal/cpu`'s `doinit()` calls `cpuid` — x86 assembly, a
  throwing generated stub — and the throw is SWALLOWED, so x86 feature detection is all-false corpus-wide
  and every AES-NI/AVX fast path runs its software fallback. **A silently-ignored package init is a
  corpus-wide false green**; trace the swallow before pricing anything above it. And for a
  near-threshold SERIAL-latency row, **core count is the wrong lever — a NATIVE control on the same host
  is what exonerates the stack**: Go passed at 250 ms where the managed side failed at 250/500/1000 ms in
  the same run, leaving managed-vs-native handshake latency as the residual.""")))

# ---------------------------------------------------------------------------------------------
# Environment / child-process pins
# ---------------------------------------------------------------------------------------------

# E18 — item 80 : a pin the converted side needs goes in the SHARED child-env base
EDITS.append(("E18-envbase", ins(
r"""  The Linux harness pin (`_paths.ps1`) STAYS until a Linux lane re-measures without it.""",
r"""  ⚠ **A pin the CONVERTED side needs goes in the SHARED child-env base, never in one side's env**
  (measured 2026-09-02, the TZ pin): `runtime.envs` is filled by a `[ModuleInitializer]` before
  `Main`, so no host code precedes the snapshot and `TestHost.Run` cannot pin `TZ` from inside the
  process — and making the snapshot live would break Go's own set-at-process-start semantics. The fix
  is the process environment at LAUNCH, beside GOROOT/PATH, applied to BOTH sides of the comparison:
  a cross-SIDE divergence is worse than the cross-platform one it was meant to cure.""")))

# ---------------------------------------------------------------------------------------------
# Integrating concurrent lanes
# ---------------------------------------------------------------------------------------------

# E19 — items 62, 89, 74, 79 : isolate by relation; both binder paths; a disclosure's prose
EDITS.append(("E19-controls", ins(
r"""  artifact both looked normal — a guard that could never go green, caught only because a dispatch's
  annotations arrived from something else. Test with the exact type and shape the call sites pass.""",
r"""  Three more, 2026-09-02. **Isolate by the RELATION the defect travels on, not by textual mention**:
  "classes that mention `flag`/`testing`" found four, "classes that drive `TestHost.Run`" five, and two
  single-cause fixes each passed a green full suite while an each-class-ALONE control still aborted — run
  every member alone. **A probe green on one binder path says nothing about the other**:
  `Delegate.CreateDelegate`'s static overload refuses a `DynamicMethod`, so a row came back
  infrastructure-error where the bound-path probe was green — exercise BOTH paths or NAME the one you
  skipped. And a committed disclosure quoted a 125/250/500 ms ladder its source does not contain (the
  rungs are 250/500/1000): re-derive a disclosure's mechanism from the line it cites, and post the RAW
  numbers beside any reading, since a measurement outlives the interpretation attached to it.""")))

# E20 — item 64 : a checker whose own binding threw still printed clean
EDITS.append(("E20-detector", ins(
r"""  dead. Any regex-bearing guard on PS 5.1 gets a BOM if it carries non-ASCII, and gets its
  detection deliberately regressed once before its verdicts are believed.""",
r"""  ⚠ Same species one layer up (2026-09-02, met independently by two lanes): a checker printed
  **PARSES CLEAN** while its own `[ref]` binding had thrown on an undeclared variable — the `else`
  branch prints clean regardless. Declare a checker's ref targets, and run it once against a
  deliberately BROKEN copy before believing any "clean".""")))

# E21 — items 71, 72 : a non-reproducible motivating failure HOLDS the cut; a flag is not behaviour
EDITS.append(("E21-holdcut", ins(
r"""  **a predicate the converter already holds beats a metadata field nothing reads** (the proposed
  flag was dropped because an existing classifier already answers the same question, counts and all).""",
r"""  **And a cut whose only demonstrated motivating failure is NON-REPRODUCIBLE is HELD** (2026-09-02,
  an L3 alias cut withdrawn): the mechanism read from the code and the emission actually measured
  disagreed — `mergeExisting=true` at the write sites READ as "preserves a windows alias into a linux
  run", while the merge is seeded per flavour and re-derives the whole imported-alias section — so a
  275-line filter nothing can exercise shipped on a static census with no dynamic measurement.
  **Measure the path once before building on a flag**, and withdraw the predicate with its census
  kept, which is the warm-design rule paid forward.""")))

# E22 — items 55, 59 : the ledger's reverse arm, and a linkname destination in a _test.go
EDITS.append(("E22-seam", ins(
r"""  committed test emission (2026-09-02). Ruled remedy: a GOROOT `_test.go` witness arm, matched by
  CLASS rather than by name and counted separately as the weaker witness; the production arm is
  unchanged.""",
r"""  ⚠ **The ledger's REVERSE arm earns its keep on hoisted literals** (2026-09-02): the converter hoists a
  body's string literals WITH the body, so a displaced body's `…ˢ` literals cease to exist and any
  hand-own referencing one dangles — the reverse side found it before a compile did; a hand-own spells
  its own panic text and depends on no hoist the displacement removes. ⚠ And a **linkname destination
  declared in a `_test.go` lands in the INTERNAL-test class, where a production-side push cannot reach
  it** (`reflect`'s `gcbits`, provided by runtime via linkname: emitted bodyless into the internal-test
  package and picked up by the throwing partial stub) — completion is the reflectlite pattern,
  registration plus a body in `export_impl_test.cs`, witnessed by the guard's test-side arm.""")))

# E23 — item 50 : a resolver that fails must stop the commit
EDITS.append(("E23-resolver", ins(
r"""- **Check the diffstat against the claim BEFORE the push, never after.** A merge whose file list does
  not match what the commit says it does is stopped at that point, not explained afterwards.""",
r"""- **⚠ A resolver that FAILS must stop the commit** (2026-09-02): a conflict-resolver script's
  assertion failed and the `git add; git commit` chained after it with `;` rather than `&&` committed
  a board carrying three conflict markers — caught only by the marker count printed beside the
  commit, and amended before the push. Chain `python … && git add … && git commit`, and grep every
  merge commit's blobs for `^<<<<<<<` before pushing.""")))


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
        description="Apply doctrine batch 4 (dry run by default; --apply to write).")
    parser.add_argument("path", nargs="?", default=DEFAULT_PATH, help="target CLAUDE.md")
    parser.add_argument("--apply", action="store_true", help="write the file")
    parser.add_argument("--dry-run", action="store_true",
                        help="report only, write nothing (the default)")
    args = parser.parse_args()

    if args.apply and args.dry_run:
        print("ERROR: --apply and --dry-run are mutually exclusive.")
        return 2
    writing = bool(args.apply)

    try:
        sys.stdout.reconfigure(encoding="utf-8", errors="replace")
    except Exception:
        pass

    print("doctrine batch 4 -- %s" % ("APPLY" if writing else "DRY RUN"))
    print("target: %s" % args.path)
    print("")

    if not os.path.isfile(args.path):
        print("ABORTED -- file not found: %s" % args.path)
        return 1

    original = read_text(args.path)
    eol = file_eol(original)
    bom = original.startswith("\ufeff")
    print("eol=%s  bom=%s  lines=%d" % ("CRLF" if eol == "\r\n" else "LF", bom,
                                        original.count(eol) + 1))
    print("")

    current = original
    misses = []
    applied = 0

    for edit_id, (old, new) in EDITS:
        anchor = to_eol(old, eol)
        count = current.count(anchor)
        if count == 1:
            current = current.replace(anchor, to_eol(new, eol), 1)
            applied += 1
            print("  [OK]   %-16s anchor found once" % edit_id)
        else:
            misses.append((edit_id, old, count))
            print("  [MISS] %-16s anchor occurs %d times -- %s" % (edit_id, count, snippet(old)))

    print("")
    if misses:
        print("ABORTED -- %d anchor(s) did not resolve exactly once. NOTHING was written." % len(misses))
        for edit_id, anchor, count in misses:
            print("  %s :: count=%s" % (edit_id, count))
            print("      %s" % snippet(anchor, 100))
        return 1

    added = current.count(eol) - original.count(eol)
    print("SUMMARY: %d edits resolved, %d lines added." % (applied, added))

    if not writing:
        print("DRY RUN -- nothing written. Re-run with --apply to write.")
        return 0

    write_text(args.path, current)
    print("APPLIED -- %s written (%d edits, +%d lines)." % (args.path, applied, added))
    return 0


if __name__ == "__main__":
    sys.exit(main())
