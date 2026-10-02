# COORD queue item: clear the warnings out of the standard-library build

STATUS: QUEUED (owner order 2026-10-01, evening, during the 1.24.13.3 release). Not dispatched. Not started.
Written during the P9 quiet window: this file is local only. Commit it to claude/coord-handover after the
release's last push. Nothing in this item runs on the i7 before that push.

## The order
"Help clear warnings out of the std lib build: real clear where possible, suppression where not."

## What already exists (read at master 172d437e66; re-read at the dispatch tree)
- A ruled suppression policy, one decision applied to two templates: `src/go2cs/csproj-template.xml:27` and
  `src/go2cs/test-csproj-template.xml:36` carry the same `<NoWarn>` list (14 codes: CS0162 CS0164 CS0282 CS0660
  CS0661 CS1717 CS1718 CS8618 CS8860 CS8974 CS8981 IDE0060 IDE1006 CA2255), plus `<Nullable>annotations</Nullable>`.
  Guard: `TestBothCsprojTemplatesCarryTheSameSuppressionPolicy` (`src/go2cs/csprojTemplate_test.go:335-370`,
  `structuralNoWarnCodes`). Any new structural code is added to that list, to both templates, and to the guard.
- golib has its own list: `src/core/golib/golib.csproj:36` (CS0660 CS0661 CS8500 CS8981 IDE1006 CA2255).
- The trim analyzer (IL####) was already cleared by scoping the publish properties off Library projects
  (comment beside the guard at `csprojTemplate_test.go:372`).

## Step 1: the census (mechanical; P1-class work)
1. One full stdlib build at the dispatch tree (master after TRAIN L lands), for BOTH release targets (linux-x64
   and win-x64, Release), with a binlog or `-flp:warningsonly`. First check whether the 1.24.13.3 release's
   Phase 1 left build logs: that is a census of the exact shipped tree for free.
2. Table: warning code x count x projects, and for each code the ORIGIN class:
   (a) converter emission (fixed in `src/go2cs`, then the corpus regenerates),
   (b) golib (`src/core/golib`, hand-owned),
   (c) source-generator output (`src/gen/go2cs-gen`),
   (d) hand-owned corpus files (`[module: GoManualConversion]` replacements, `*_impl.cs` companions),
   (e) analyzers (IDE/CA),
   (f) NuGet/MSBuild (NU/MSB).
   Two sample sites per code, file:line.
3. Distinct count per code, never only the total. One code at thousands of sites is one decision; fifty codes
   at a handful each are fifty.

## Step 2: dispositions (COORD rules, per code)
- CLEAR FOR REAL when the fix keeps behaviour AND keeps the visible emission Go-like (reads-like-Go is the
  escalation line to the owner). Converter fixes ride a train: red first, goldens, CNR, two-seeded 3-target
  footprint. golib/gen/hand-own fixes are direct, and they owe GolibTests at Debug AND Release plus a
  `go2cs.slnx` build.
- SUPPRESS when the warning is structural to Go semantics (Go permits what C# flags), at the NARROWEST honest
  scope:
  - per code, in the ruled `structuralNoWarnCodes` list, with a one-line rationale per code;
  - a site-scoped `#pragma warning disable/restore` only where the code is real elsewhere.
  Never a blanket `<WarningLevel>`, `-nowarn`, or `TreatWarningsAsErrors=false`-style switch.
- DEFECT: a warning that may flag a real bug becomes a BOARD finding with a repro, never a suppression.
  Watch: CS0252/CS0253 (unintended reference comparison), CS0108/CS0114 (hiding), CS0649 (never assigned),
  CS4014 (un-awaited task), CS0659, CS8602-class, if any appear.
- Owner ruling 2026-09-28 applies by analogy: never change visible emission merely to satisfy a diagnostic.
  When clearing would cost readability, suppress instead.

## Step 3: keep it clean
- A ratchet: a guard that the stdlib build's warning count per code does not GROW, read from a data file the
  census process re-measures (never hand-set, no figures in durable docs). It runs where the stdlib build
  already runs.
- README/docs: none needed unless the NoWarn policy text changes (then `docs/` follows the design).

## Routing (proposed; COORD confirms at dispatch)
- Census: P1 (Sonnet: mechanical, pre-specified) after its current queue, or the i9 (heavy build).
- Converter-side clears: G or P2, after TRAIN M's real-module batch.
- golib/gen clears: i9 or C1.
- Sequencing: after the release and the L landing. Fixes ride TRAIN M or N. The census itself touches no master
  state and can run on any box.

## Note
The owner first posted this to C2 by mistake (2026-10-01 evening). C2's current seat is S3a sizing (-nuget-map);
the owner told C2 to stand down on it himself (same evening). COORD owes C2 nothing on this item, beyond
confirming S3a is still its seat if C2 asks.
