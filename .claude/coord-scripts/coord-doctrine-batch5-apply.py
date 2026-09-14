#!/usr/bin/env python3
# coord-doctrine-batch5-apply.py -- apply doctrine batch 5 (accumulator items 95-169) to CLAUDE.md.
#
#   python coord-doctrine-batch5-apply.py <path-to-CLAUDE.md> [--dry-run]
#
# Contract:
#   * reads and writes with io.open(..., encoding='utf-8', newline='') so CRLF is preserved
#     byte-for-byte and nothing else in the file is touched;
#   * every insertion is anchored on an EXACT existing line (matched WITH its trailing \r\n);
#   * all anchors are asserted to occur exactly once BEFORE anything is written (all-or-nothing);
#   * idempotent: a second run detects its own insertions and makes no change ("already applied");
#   * prints a summary of anchors and inserted line counts;
#   * performs NO git operations.

import io
import os
import sys

NL = "\r\n"


def block(text):
    """Triple-quoted LF text -> list of lines, leading/trailing blank line stripped."""
    return text.strip("\n").split("\n")


# --------------------------------------------------------------------------------------------
# (name, anchor line WITHOUT line ending, inserted lines)
# --------------------------------------------------------------------------------------------

INSERTIONS = [

("1. long runs / detachment (items 111, 169)",
 "  fate onto the child and the turn boundary kills it exactly as if it had been spawned inline.",
 block(r"""
  ⚠ **The two detachment stories are measured and point OPPOSITE ways (2026-09-02).** A
  `Start-Process -WindowStyle Hidden` from INSIDE a PowerShell TOOL call died silently ~15 s in (the
  documented pattern covers a BASH-launched child surviving the turn boundary, not a tool call's own
  job scope), while a Bash `run_in_background` task is reaped with the SESSION's process tree — a
  2-hour solo sweep died ~13 min in and sat UNDETECTED for 76, with no completion notification.
  Anything longer than a turn runs DETACHED, env-pinned in the SAME command, logged unique-per-run
  and polled POSITIVELY by PID;
  clean-death evidence before a restore is modified files with ZERO untracked.
""")),

("2. preserve a failed row's record; roster-walking leg (items 96, 126)",
 "    the record files after every sweep, and state cold-vs-warm when comparing two runs.",
 block(r"""
    ⚠ **Two more, 2026-09-02.** (4) A gate PRESERVES a failed row's comparison record to a distinct
    path BEFORE any restore or cleanup — a union battery deleted the records after a `net/http` sweep
    FAILED, discarding the only evidence of which rows diverged; deletion is for hygiene, never for
    evidence. (5) `run-validated-sweep.ps1` walks the ROSTER, so `-Filter <pkg> -Exact` on an UNBANKED
    row throws "No banked packages matched" while the battery leg wrapping it exits 0 over the hole —
    route #6 in a coordinator instrument: run an unbanked row through the pipeline DIRECTLY, and carry
    every leg's failure in the wrapper's exit code.
""")),

("3. 0xc0000142 in the results tail (item 165)",
 "    the kill (2026-09-02) — match the escaped form too, or parse the field.",
 block(r"""
    ⚠ A new tail-stated member (2026-09-02, the `chanDir` arm): 388 divergences, every verdict `C#=""`,
    stream 0/0/0 — reading like a corpus-wide regression from the lane's own cut — with the tail saying
    `exit status 0xc0000142` (STATUS_DLL_INIT_FAILED), a TORN `bin`/publish tree from an interrupted
    run. Delete the publish dir and re-run; `0xc0000142` in the tail is a cleanup, never a finding.
""")),

("4. measurement configuration + host qualification (items 116/133/134/135/137/138/142/145/153/156/162b)",
 "  sweep path instead, and state which binary each arm ran.",
 block(r"""
- **⚠ THE MEASUREMENT CONFIGURATION IS PART OF THE VERDICT — the `-tests` pipeline publishes DEBUG
  (measured 2026-09-02, the net/http h2 pair).** The generated `<pkg>.tests.csproj` pins no
  Configuration, so every roster verdict to date was taken at an optimization level no user ships: one
  published artifact flips `TestWriteDeadlineEnforcedPerStream/h2` fail→pass under Release (43.7 ms vs
  500–1000 ms per handshake), and default tiering flips it BOTH ways across consecutive runs of that
  same binary — a validation-integrity defect, the flake class arriving through the JIT. Ruled
  contract: **Release + `DOTNET_TieredCompilation=0`, both RECORDED** in two places that cannot
  silently drift — the comparison record (`testEnvironmentRecord{Configuration,Tiered}`, never
  `omitempty`: absence must not read as Debug) and the host's own `results.json` — plus the proof
  pages. The experiment already existed as the converter's `-test-release-tc0` (a bare `dotnet build
  -c Release` on that csproj is the trap it avoids — the template's `go2csPath` is Debug-conditional):
  **grep the converter's flags before building an instrument**, and NAME both sides' configuration.
  ⚠ **Owner ruling (2026-09-02 11:44): the validation configuration of RECORD is Release with tiering
  off; Debug stays available by flag; the pipeline and sweep defaults flip after the Release census.**
- **⚠ HOST QUALIFICATION for a network row: preflight `go test -count=1 net` BEFORE any net-family run
  (2026-09-02).** A host whose Go's OWN suite fails is disqualified as a bank host (a container
  answering `TestLookupCNAME` with the CDN CNAME and no IPv6; a WSL host failing that AND all 18
  `TestLookupNoSuchHost` leaves), and on an unqualified host the two arms of an A/B run different
  oracles — evidence, never a bank. A test asserting a live PUBLIC DNS record is UNIVERSAL drift once
  three independent resolvers agree (disclose it on the host-qualification ledger, not any one
  host's); and **a lane does not change a host's system configuration on its own initiative** — relay
  the commands to the owner, and RE-qualify afterwards (G-LAPTOP's WSL did, the same day: the 18
  leaves pass, wall 707 s → 35 s, and it is the fleet's Linux `net` bank host).
""")),

("5. attribution + control form (items 97/113/119/120/121/132/158/160/161/164)",
 "  numbers beside any reading, since a measurement outlives the interpretation attached to it.",
 block(r"""
  Four ATTRIBUTION rules from one night of probe work, 2026-09-02. **A variant table names what each
  variant REMOVES and the attribution line is DERIVED from that column** — a swapped label on a correct
  measurement survives review by looking self-consistent. **An attribution is a ONE-AXIS pair**: a pair
  differing on two axes (container AND assembly) read 2.7x where the one-axis pair read 4.17x, and the
  design is cut against the one-axis number. **A gap between two arms of the SAME code with the SAME
  attribute is a CONFOUND TELL, never a boundary cost** — identical IL inlined from two assemblies
  yields identical machine code, so 4.0 vs 11.1 ns/word means an unoptimized callee or a declined
  inline: read `DebuggableAttribute.IsJITOptimizerDisabled` INSIDE the probe process, and on a release
  runtime read inlining from `DOTNET_JitDisasmSummary=1` (an inlined callee is absent from the list),
  since `DOTNET_JitPrintInlinedMethods` prints nothing there. **A hand-transcribed proxy is diffed
  against the emission before its number is quoted** — one token moved a 2.75x reading — and a
  retraction's positive claim owes the same measurement as the claim it retracts. Three control-FORM
  rules beside them: **a gate is ruled only after its BEFORE shows it can MOVE** (the TZ-pin gate row
  was green before the pin existed; calibrate with the variable genuinely ABSENT, since `TZ=` empty
  means UTC in Go and reads exactly like the pin); **a body's own failure is earned by a control in a
  SEPARATE worktree at the same SHA**, never by splitting the cut into commits; and **count a guard's
  DISCRIMINATING lines, not its lines** — a loopback receiver on 127.0.0.1 was GREEN against the body
  it guarded, a destination zeroed to 0.0.0.0 arriving anyway (bind 127.0.0.2 so arrival depends on
  the octets, and exercise the OLD path in the control).
""")),

("6. property inferred from an artifact; a hook that fires (items 108, 110, 155, 167)",
 "  MEASURED, not what was concluded.",
 block(r"""
  ⚠ **Three more, 2026-09-02, one shape: a property INFERRED from an artifact instead of measured.** A
  census can be exactly right about what EXISTS and exactly wrong about what it MEANS — thirteen
  typed-nil sites counted correctly by two derivations, then classified off the emissions and wrong
  twice (what the named spelling preserves is C# TYPE IDENTITY, not the dimension). **A converter hook
  that FIRES is not a hook that CHANGES the emission**: `getExprContext` returns the FIRST matching
  context, so cargo APPENDED as a second one is unreachable while the instrumentation reads healthy —
  instrument, then DISBELIEVE the instrument's agreement with the emission. And **a utility that exits
  0 with NO output is indistinguishable from one that never found its input**: its zero is a result
  only after a positive control (delete a known line, re-run unchanged, require it byte-identical).
""")),

("7. platform-exclusive guards, F8, poisoned csproj (items 101/123/144/163/166)",
 "  enumeration, a Linux CNR run therefore reports `FindFirstFileData` as NOT MEASURED by design).",
 block(r"""
  ⚠ **The class bites in BOTH directions now (2026-09-02): a behavioral guard written against ONE
  platform's syscall API cannot type-check on the other and turns THAT host's CNR red by name.** A
  lane's own-platform CNR green says nothing about the other host's gate — the union battery there is
  where it surfaces. F8 landed with train 11 (2026-09-02): a converter-preserved
  `[GoPlatformExclusive("<goos>")]` marker in `package_info.cs` naming the native platform(s), plus a
  LOUD skip-by-name BEFORE transpile in every enumerator (CNR, `BehavioralRunner`, MSTest as
  `Inconclusive`), its gating set DERIVED from the other platform's NOT MEASURED list (six
  windows-native, `ScmRightsSeam` linux) and positive-controlled both ways; commit markers before any
  CNR `-Revert`, which destroys uncommitted ones. Worse, a best-effort conversion on a
  NON-native host REWRITES the package's csproj and `package_info.cs` (the stdlib ProjectReferences and
  import aliases drop when the type-check that supplies them fails), so a Windows CNR POISONS a
  Linux-only behavioral package and every later leg of the chain measures the poisoned file — 5
  CS0246/CS0234 reading as a missing-reference regression. A chain therefore RESTORES behavioral dirt
  (`git checkout HEAD -- src/tests/Behavioral`) between CNR and any build leg, and F8's skip must
  precede the converter. Such a guard also carries a `runtime.GOOS` early-out as `main`'s first
  statement (raw `syscall.Socket` panics on Windows without the WSAStartup `net` performs), goldens
  stay WINDOWS-generated, and a Linux CNR-EQUIVALENT's DRIFT column is noisy by construction — the NOT
  MEASURED column is the honest one there.
""")),

("8. the QUIET wrong-release shape; oracleGoVersion (items 100, 122, 149, 157)",
 "  not source profile.d like a real login: verify by bare `go version` in a real login shell.",
 block(r"""
  ⚠ **And its QUIET shape, with the seatbelt that is not one (2026-09-02, the container class):** where
  the loud form misroutes the namespace and exits 0, an oracle run under an ambient 1.24.7 against a
  1.23.12 corpus answers NORMALLY — no empties, no errors, a real comparison against a corpus the tree
  does not have. `GOROOT="$(go env GOROOT)"` is the trap wearing a seatbelt: pin explicitly, put its
  `bin` FIRST on PATH, ABORT unless bare `go version` reports the pinned release, and re-measure
  anything banked under an ambient one. The container class is NOT uniform (no bare `go` on one host,
  1.24.7 on another, 1.25.1 off PATH on a third) and a persistent USER-scope GOROOT can pin an old
  release on a laptop lane, so no lane assumes another's toolchain number — and pin `-go2cspath
  <worktree>/src` on every hand-invoked `-tests` run, whose generated csproj otherwise falls back to
  the machine-global deploy root (MSB4006 loud; a plausible verdict from uncompiled bits quiet).
  Because nothing recorded WHICH release ran the oracle, `oracleGoVersion` now goes into the comparison
  record, captured as OBSERVED — a `go version` through the same call, directory and environment the
  `go test -json` child inherits, `omitempty` so a late probe failure cannot invalidate a comparison.
""")),

("9. what the both-editions check IS (item 168)",
 "  OS-matrix linux leg — before a shared `.ps1` change merges.**",
 block(r"""
  ⚠ **What that check IS, stated 2026-09-02: the PARSE of every shared script under pwsh 7 Core, plus
  one row actually run.** A cloud container may carry NO PowerShell at all (`dotnet tool install
  --global PowerShell` lands one on the user's tool path) and its writable allowance may sit under the
  sweep's own disk-preflight floor — such a host runs the edition and gate checks with
  `-IgnoreDiskPreflight` STATED, and never banks a Linux row.
""")),

("10. src/core/<pkg> holds three populations (items 95, 107)",
 "    stripped; fixed in `ce82093b0` and proved clean across the full r40 sweep.)",
 block(r"""
    ⚠ **After a `-tests` run a package directory holds THREE populations — tracked corpus files,
    tracked hand-owns, and untracked generated emission — so any glob- or directory-wide operation hits
    the wrong one** (paid twice, 2026-09-02). `rm -f src/core/reflect/*_test.cs` deleted the TRACKED
    `export_impl_test.cs` hand-own (the glob encoded "test files under a converted package are
    generated" — true for 13 of 14), and `git checkout -- src/core/reflect` reverted the lane's own
    guard edit in `value_impl.cs`. Restore by FILENAME, clear emission with `git clean -nd` then `-fd`
    — the primitive that reads the tree's state beats the pattern encoding a belief about it.
""")),

("11. \"armed\" is a claim about a running task (items 115, 149b, 159)",
 "  way gates are positive-controlled).",
 block(r"""
  ⚠ **"Armed" is a claim about a task verifiably STILL RUNNING** (2026-09-02): a task id that has
  EXITED is evidence of a PAST arming, and a lane went silent for hours with BOTH legs down — its
  exit-on-change watcher had fired on the lane's own post and was never re-armed, while the backstop
  that exists to catch exactly that first failure was itself gone. A protocol step that must be
  remembered at the end of the busiest turn, and whose failure is silent, fails on a schedule: DELETE
  the step (a persistent monitor needs no re-arm on a local lane; on the cloud-container class it is
  hard-capped at ~30 min, so there the relaunch leg is load-bearing) rather than reminding harder, and
  back it with a leg that verifies LIVENESS, not existence, and checks its own existence on every
  firing. Its reading
  half: a filter built from expectations can be simply where you stopped reading — read every numbered
  item of a post addressed to you, and read anchor..tip before starting the next one.
""")),

("12. cpuid root corrected; handshake attribution falsified (items 124, 129, 135)",
 "  the same run, leaving managed-vs-native handshake latency as the residual.",
 block(r"""
- **⚠ Both halves of that row are CORRECTED by later measurement (2026-09-02) — read them together.**
  There is no swallow: `schedinit` never runs, so `cpuinit`/`cpu.Initialize`/`doinit`/`cpuid` are
  UNREACHABLE and every `X86.Has*` is simply its zero value; the fix is a `[ModuleInitializer]`
  stand-in (the `goenvs`/`goargs` precedent) hand-owning `internal/cpu` over
  `System.Runtime.Intrinsics.X86`, 14 of Go's 20 flags mapped and 5 left false as the conservative
  direction. **A silently-UNREACHED package init is the same corpus-wide false green as a swallowed
  one** — trace the CALL CHAIN, not a `catch`. And the handshake residual was FALSIFIED as the h2
  pair's cause: a clean negative A/B moved 0 rows with AES-GCM negotiated, an isolated handshake is
  ~44 ms (which cannot blow a 250 ms rung), and the pair is a build-CONFIGURATION artifact — see the
  Debug-publish rule above; a cut's justification stays what it MEASURED.
""")),

("13. a WSL reconfiguration can change which USER a lane runs as (item 162a)",
 "  a literal known to be present.)",
 block(r"""
  **A sixth, 2026-09-02:** a WSL reconfiguration can silently change which USER a lane's automation
  runs as — after a resolver change the default user flipped, the lane's scripts became unreadable, and
  the wrapper EXITED 0 over a permission error in its log: route #6's shape again, a runner that cannot
  reach its own work reporting success. The LOG caught it, not the exit code; `wsl -u root` is the fix.
""")),

]


def main(argv):
    if len(argv) < 2:
        sys.stderr.write("usage: coord-doctrine-batch5-apply.py <path-to-CLAUDE.md> [--dry-run]\n")
        return 2

    path = argv[1]
    dry_run = "--dry-run" in argv[2:]

    if not os.path.isfile(path):
        sys.stderr.write("ERROR: not a file: %s\n" % path)
        return 2

    with io.open(path, encoding="utf-8", newline="") as f:
        text = f.read()

    # ---- idempotency: has this batch already landed? ------------------------------------------
    present, absent = [], []
    for name, anchor, lines in INSERTIONS:
        marker = NL.join(lines) + NL
        (present if marker in text else absent).append(name)

    if not absent:
        print("already applied - all %d batch-5 blocks are present; no change written."
              % len(INSERTIONS))
        return 0
    if present:
        sys.stderr.write("ERROR: partially applied - refusing to write.\n")
        for n in present:
            sys.stderr.write("  present: %s\n" % n)
        for n in absent:
            sys.stderr.write("  absent : %s\n" % n)
        return 1

    # ---- all-or-nothing anchor validation BEFORE any mutation ---------------------------------
    print("anchor validation (each must occur exactly once, CRLF-terminated):")
    problems = []
    for name, anchor, lines in INSERTIONS:
        count = text.count(anchor + NL)
        if count != 1:
            problems.append((name, anchor, count))
        short = anchor.strip()
        if len(short) > 70:
            short = short[:70] + "..."
        print("  [%-4s] x%d  %s" % ("ok" if count == 1 else "FAIL", count, short))
    if problems:
        sys.stderr.write("\nERROR: %d anchor(s) not unique - nothing written.\n" % len(problems))
        for name, anchor, count in problems:
            sys.stderr.write("  %s: %d occurrences\n    anchor: %s\n" % (name, count, anchor))
        return 1

    # ---- apply --------------------------------------------------------------------------------
    total_lines = 0
    summary = []
    out = text
    for name, anchor, lines in INSERTIONS:
        old = anchor + NL
        if out.count(old) != 1:
            sys.stderr.write("ERROR: anchor no longer unique mid-apply (%s) - nothing written.\n"
                             % name)
            return 1
        out = out.replace(old, old + NL.join(lines) + NL, 1)
        total_lines += len(lines)
        summary.append((name, anchor, len(lines)))

    if dry_run:
        print("\n--dry-run: NOT writing. Would insert:")
    else:
        with io.open(path, "w", encoding="utf-8", newline="") as f:
            f.write(out)
        print("\nwrote %s" % path)

    print("\n%-4s %-6s %s" % ("#", "lines", "block"))
    for i, (name, anchor, n) in enumerate(summary, 1):
        short = anchor.strip()
        if len(short) > 62:
            short = short[:62] + "..."
        print("%-4d %-6d %s" % (i, n, name))
        print("            after: %s" % short)

    print("\n%d blocks, %d lines inserted." % (len(summary), total_lines))
    print("bytes: %d -> %d (delta %d)" % (len(text.encode("utf-8")),
                                          len(out.encode("utf-8")),
                                          len(out.encode("utf-8")) - len(text.encode("utf-8"))))
    crlf_in, lf_in = text.count("\r\n"), text.count("\n")
    crlf_out, lf_out = out.count("\r\n"), out.count("\n")
    print("line endings: in CRLF=%d bare-LF=%d | out CRLF=%d bare-LF=%d"
          % (crlf_in, lf_in - crlf_in, crlf_out, lf_out - crlf_out))
    return 0


if __name__ == "__main__":
    sys.exit(main(sys.argv))
