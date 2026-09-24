# COORD verification: string-literal design, revision 4 (2026-09-24)

**What was verified.** C2's revision 4 of `docs/phase4/DESIGN-string-literal-allocation.md` §8:
- branch `claude/c2-literal-cache-draft`, tip `c555d91c55` (revision 3 was `d382b60677`);
- C2's announcement, `claude/mailbox` `docs/phase4/inbox/COORD/20260924T125751Z-C2.md`.

**Method.** A read-only workflow with three verifier dimensions (the fixes, the evidence, the owner's decision),
two adversarial refuters per finding, and a completeness critic. Nothing was built or run. Every delta, ratio and
count that the committed raw outputs can support was recomputed from them. The owner's decision brief is derived
from this note and delivered separately.

## Verdict

Revision 4 is **sound where it counts**:
- **Revision 3's order defect is real.** Registering after the import hooks served 5% of lookups.
- **The fixed order serves the start-up lookups:** hello 1,389/1,389, net/http 2,949/2,951.
- **Hybrid lazy registration is correctly rejected.** It registers 78-79% of literals at start anyway, and its
  saving sits inside the spread. It is worse on net/http at TC=0 and in the single-file shape.
- **The helper-thread variant deadlocks**, and the page-step scan faults.
- **All 12 of the revision-3 note's fixes are mapped:** 8 APPLIED, 3 PARTIAL, 1 mixed.

Three things change what the owner reads. None of them is a defect in C2's measurements.

1. **Tier 2 as designed does not reach TEST-assembly literals.** The shipped mechanism is a first-position hook in
   `package_info.cs`. A test assembly never compiles `package_info.cs` (`test-csproj-template.xml:99-102`), and
   the probe skipped test modules deliberately (`genreg.py:22,33`). Every §8 member literal lives in a test
   assembly: log TestDiscard's `"%s"u8` is at `log_test.cs:239`, and slog's F6 keys are in `logger_test.cs`. So
   Tier 2's stated payoff, count parity on the member rows, is not reachable by the design as written. (The
   critic's G1; the evidence is closed and the design item is open.)
2. **sstring-first's reach is missing from the owner-facing text.**
   - §8.2R2's literal-to-parameter row quotes only the non-format subset, and the O1 ranking gives O1's reach as
     the 111 `string(b)` arguments.
   - Go's own escape analysis puts O1 at **up to 4,835 production literal arguments**: 60% of the 8,083 joined
     literal arguments, and **97% of the format-position population** (2,491 of 2,556), which is arm B's.
   - No revision measured sstring-first on its own. Its demotion to "a performance campaign after Tier 2" assumes
     Tier 2 lands.
   - The O1 survival census that §8.2.3 named "the first census to run" was never run. (The decision
     dimension's F1 and F2; both survive their refuters, at minor.)
3. **The start-up tax is per PROCESS, and it is mostly registration CODE.**
   - In the fixed-order rows the table's own work (`registerMs`) is 6-13% of B−A. Boff−A (the registration code
     with a no-op Register) is +25 to +74 ms.
   - The variants that remove per-literal registration code were never probed: revision 3's range-only lazy form
     and reflection over RVA fields.
   - Banked suites start the converted test host many times: crypto/tls's BoGo shim runs 3,418 cases, and os/exec
     and runtime also spawn. At the test host's TC=0 tax that adds hundreds of seconds of process time (an
     inference).

## Findings for the design record (C2), by severity after the refuters

**Carry into any further revision:**

- **(fixes F1) The range-only lazy variant.** Revision 3's range-only lazy variant is still live text, is never
  measured, and is neither withdrawn nor sized. Arm L is a different mechanism. Its stated blocker (the order) is
  now cleared. Size it, or withdraw it with a reason. The critic's G5 adds the reflection-over-RVA blob: the two
  variants with no per-literal code are the only levers against the dominant cost.
- **(fixes F8) G8's rewrite over-corrects.**
  - Go's `concatstrings` also returns a single non-empty operand that ALIASES (runtime/string.go:49), including
    `unsafe.String`.
  - The new precondition that "an alias operand stays on the copying path" therefore counts where Go does not.
    As written, `unsafe.String(...) + ""` would copy and count one object where Go allocates nothing.
  - Restate G8 as Go's own rule. Any residual hazard should be stated as a golib-internal lifetime concern, not
    a count divergence.
- **(decision F1/F2) Put O1's literal reach in the owner-facing row and ranking,** including the 4,835 and the
  97% format-position share, and state that sstring-first was not evaluated without Tier 2.
- **(critic G1) State how test assemblies would register,** or state that Tier 2 does not reach them. (Test
  compile lists are alphabetical, e.g. `log.tests.csproj:104-107`, so a "first-position" file would have to be
  forced.)
- **(critic G3) Frame the tax per process,** and name the process-spawning suites.
- **(critic G6) The shipped registration generator's BUILD-time cost** (a Roslyn generator reading constants from
  the semantic model, run on every build of every converted project) is neither measured nor named.
- **(critic G8) "Uncounted and outside every window" holds only for modules an import hook forces.** A module
  with no hook registers at first touch, and a table doubling can land inside a single-shot measurement window.
- **(critic G9) R1(iv)'s `NoUncountedBackingAllocations` guard** would flag the table's deliberately uncounted
  arrays (`LiteralTable.cs:31,228`). Name an exemption, or a non-counting door.
- **(fixes F2, evidence F6) Statistics.**
  - The recorded p25-p75 spreads are never used in a conclusion.
  - Quoted ranges mix medians and mins, and the net/http mins come from one fast A run.
  - Quote ranges where the median and the min disagree: "L costs +23 to +49 ms more at TC=0", and "net/http
    +63 to +78 ms tiered".
- **(evidence F1/F2) Reproducibility.**
  - The per-run timings behind the medians of 31 are not committed, so no significance test can run on the
    evidence.
  - The code behind the timed L arms is not committed: the committed `LiteralTable.cs` is a later version with a
    different range discovery.
  - The R2R rows cannot be regenerated: `time.py` dropped the r2r regime, and `driver.sh` has no R2R publish.
  - The error runs in L's favour, so the verdict stands, but the reproducibility statement must say so.
- **(evidence F4) No leg, in two places the Tier 2 ruling depends on.**
  - NativeAOT's module-initializer order against the first literal use, with the design's real partial-method
    hook rather than the probe's Compile-item order.
  - Every absolute start-up figure off linux. The Windows hello closure carries about 10% more literals (the
    critic's G10: 3,419 is a windows-closure count, with linux at 3,095).
- **(evidence F5) "Per-literal cost is a RATIO, not a slope"** rests on one revision-3 cell; the other cells scale
  near-proportionally. The critic's inference for the net/http test process is about 130 ms at TC=0.
- **(evidence F8) The miss-tax ns figures come from a 6-literal table that fits in L1,** a best case. The
  "inside run-to-run spread" claim holds only for tiered runs.
- **(fixes F3, F4, F5, F7, F9) Text hygiene.**
  - The B−Boff "did not repeat" comparison uses the wrong arm.
  - Revision-4 text cites bare revision-3 line numbers.
  - One superseded figure sits under a revision-4 pointer.
  - One sentence contradicts the results header on range discovery.
  - The L-excess inference compares module counts on unequal bases: the measured jitMethods are 2,982 against
    2,881 at TC=0.
- **(decision F3, F4) Terms.**
  - State what happens to arms A and B. Revision 4 dropped "not needed once Tier 2 lands" and revived arm A for
    the UTF-16 carve-out, which is a different population.
  - Pin terms by mechanism, because "arm B" and the Tier numbering collide with the landed Tiers A/A′/B/C.

**Closed by the critic (no action):**
- The 3,419 count (a windows-closure count).
- The memory headline (0.3-0.9 MB, recomputed).
- There is no interaction with G's len/cap overloads, since `@string` is not ISlice.
- The wiring points are unchanged between the draft's base and master.
- The slog member rows are already attributed on master. No slog row banks from a literal tier alone; the only
  member row a literal tier banks outright is log TestDiscard (2 → 1).

## What must be measured before any Tier 2 ruling is final

- **Windows:** start-up A/B with the fixed order in the test-host shape (single-file, TC=0) and under `dotnet run`,
  for hello and net/http.
- **The net/http TEST process,** which does not build for linux at this tree.
- **The Roslyn u8 dedup pin** on Windows and for a generator-emitted literal.
- **One AllocsPerRun member row read on a Tier 2 build,** after the test-assembly registration gap is resolved.
- **A ReadyToRun publish of the FIXED-order arms.** The 1-5 ms figure came from revision 3's order.
- **A NativeAOT start-up arm,** plus NativeAOT initializer order under the real hook.

## Provenance

- The workflow run was `wf_b583cac2-0bd` (68 agents, all completed). The per-agent outputs are in COORD's session
  scratch; this file is the record.
- The owner's decision brief was delivered in the COORD session on the same day and is recorded on the ledger
  with the ruling.
