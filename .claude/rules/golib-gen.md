---
paths:
  - "src/core/golib/**"
  - "src/gen/**"
---

# golib and go2cs-gen: byte costs, the alloc instrument, route #7 and the native-boundary classes
<!-- EXTRACTED VERBATIM from CLAUDE.md at 1800b04f8, lines 71-180, 1129-1367, 4297-4578.
     Phase 1 of docs/PLAN-context-diet.md: RELOCATION ONLY, no compression, no edits.
     The byte-identical original is docs/doctrine/JOURNAL-2026-09-12.md.
     Phase 2 distills this file. Keep every dated derivation, but move it into an HTML
     comment beside the rule it justifies: comments are stripped before this file enters
     context, so provenance kept this way costs ZERO tokens.

     PHASE 2 DONE 2026-09-12. Every dated narrative from the Phase-1 text is preserved below,
     verbatim or near-verbatim, inside the comment block beside the durable rule it justifies.
     Nothing was deleted. The visible half is the manual; the comments are the chronicle. If a
     rule reads thin, its derivation is in the comment under it, and its byte-identical
     pre-split original is in docs/doctrine/JOURNAL-2026-09-12.md.

     BATCH19 MERGED 2026-09-12: the doctrine items from claude/coord-doctrine-batch19
     (24bfc8304 / c5e17217b / e9e56b657) that the extraction map routes to this file were
     integrated here — most as further evidence inside an existing rule's comment, a few as new
     visible rules where the trap was genuinely new. Nothing from that branch was dropped. -->
## Build the solution after any golib/runtime API change
**Nothing routinely builds `src/go2cs.slnx` end to end, so a broken member rots invisibly** — every harness (`BehavioralRunner`, MSTest, `check-no-regression.ps1`, `run-validated-sweep.ps1`) builds each `.csproj` **by path** (the habit `check-solution-integrity.ps1` polices from the other direction). **After a golib/runtime API change, build `src/go2cs.slnx` once before banking** — ~90 s, no other gate covers it.
<!-- The same by-path habit `check-solution-integrity.ps1` polices from the other direction. The
     victim was the `utilities/QuickTest` scratch project: the r41 GoFrame arc (`6adab2909`,
     2026-08-05) retired golib's `func`/`Defer`/`Recover` execution-context API and carried the
     corpus with it, but QuickTest was hand-written scratch nobody builds, so it took `go2cs.slnx`
     down for two days unnoticed. RETIRED 2026-08-07 rather than hand-fixed again: it was the
     solution's only hand-written, un-gated member, so it would have rotted at the next golib
     change, and the experiments it held (struct/interface promotion hand-simulated *before*
     `go2cs-gen` existed) are covered by real behavioral tests now. Git keeps it at `d3223d252`. -->
## Byte costs — the reference numbers
| Quantity | Measured |
|---|---|
| INSTANCE state on `ж<T>` or any per-box base class | **+8 B on EVERY pointer box**, corpus-wide (14/1/0 boxes across three alloc rows) |
| One pointer box that ESCAPES | **64 B**; a box an earlier cut already UN-ESCAPED is **0.00 B** |
| Same row, filtered vs unfiltered suite scope | **+167.04 B/op** on one tree |
| `os` alloc row | **1,320.00 B/run** in every configuration **except Release+tiered (1,256)** — tier-1 escape analysis stack-allocates one non-escaping box |
| 100-run `AllocsPerRun` sample | fixed **0–800 B/run** accounting term; **cannot resolve a change under ~150 B/run** |
| Window 1 of a 1,000,000-run `AllocsPerRun` | ~**1.3 B/run** above the integer floor; windows 2–3 land on it |
| Converged floor after the receiver-box cut | **744.25 B / 8 obj**; a 100-run single window read **898/916/895** (count exact, bytes ~150 B high) |
| 40-draw minimum of a 100-run window | unconverged by **+42.5/+44.4**, an offset that CANCELS in a difference of two such minima |
| Box-slot representation arms, 10M-call hot loop | **18.0 / 20.2 / 37.0 ns** against a **33.8** baseline |
| CLR identity hash (CoreCLR) | **26 bits** — top six never set over a million simultaneously-live objects |

**A golib change adding instance state to a per-box base class states its corpus-wide cost in the commit even when correctness demands the field, and states it as a PER-ROW FORMULA** (+8 B × consumer-type boxes per op), never as a single verdict. **An alloc row's B/op is comparable only against a figure taken at the SAME SUITE SCOPE.**
<!-- Both cost rules measured 2026-09-01. +8 B: proportional to boxes allocated per path (14/1/0
     across three alloc rows), so the commit states the cost even when correctness demands the field
     — the element-aliasing publish gate did; its unfavorable direction shipped unmeasured and later
     burned an attribution run. Scope: filtered vs unfiltered differed by +167.04 B/op on ONE tree
     (AllocsPerRun's single warmup doesn't cover one-time costs a full run has already paid), so a
     filtered census never compares its bytes against a full-run record: the alloc-instrument sibling
     of the gated-census stream rule. The per-row-formula rule is from the 2026-09-05 representation
     arc: three arms of a box-slot design read their predicted bytes to the byte on the alloc row,
     which separates only the slot's +8 B/box. -->
## The alloc instrument
A byte endpoint quoted off an unconverged instrument is a false measurement.
- **A prediction's BASELINE is measured at the same scope in the SAME RUN, never quoted from a record.** A reduction LARGER than predicted is still a wrong prediction; both corrections go in the **commit message**, not only the post.
- **Quote the deterministic FLOOR** (minimum over reps) **or a high-runs figure, and name the UNIT and the CONFIGURATION.**
- **TWO windows is the minimum protocol**, and **a reading is comparable only under the SAME WINDOW PROTOCOL as the record it is read against** — stop and reconcile first.
- **An instrument reading HIGHER on strictly FEWER objects is NOT CONVERGED.** Report that; never smooth or explain it. **A difference of unconverged minima may be right while both absolutes are wrong** — record absolutes only from the converged instrument.
- **When the count is exact and the bytes do not close, SEGMENT before naming a mechanism**: per-frame byte probe with literal tags, exact segment sum, one-row positive control. **A segment the instrument could not enter is reported COMBINED with its split marked DERIVED**, cross-checked against an identical construction measured directly in the same run.
- **Object COUNT is charged at the `new` while BYTES diverge under a tiered JIT** — a stack-allocated box still counts 1.00, so **no JIT improvement banks a count-conditioned row; only not constructing the boxes does.**
- **A COUNT-based byte prediction is a LOWER bound when the cut also UN-ESCAPES surviving boxes**, and its converse: **removing boxes an earlier cut already un-escaped saves ZERO bytes.** **Re-read the per-box unit from the CURRENT tree's segment table before every prediction.**
- **A LAYOUT read (`Unsafe.SizeOf<T>`) is fixed at JIT time and cannot move with load** — but a size row is an ASSERTION only after a control grows the struct by one word and reads RED.
- **`-p:BaseOutputPath` / `-p:BaseIntermediateOutputPath` are GLOBAL properties**: they propagate into every referenced corpus project, mint parallel `obj-*` trees under `src/core` and collide on the default compile glob (**CS0579/CS1537**). An outside-the-repo probe sets `EnableDefaultCompileItems=false` with one explicit `Compile` item.
- **The counter charges golib's own allocation sites only — it is structurally BLIND to CLR boxing**, a defer's delegate, a params array, an interface box. **A "floor of N boxes" is a PROOF CLAIM until byte arithmetic against a hand-boxed control measures it.**
- **A capability can have TWO populations of different sizes** (239 sites minting a box AND a delegate, 207 a delegate alone), only the first visible — **census BOTH**, and size a lowering by a `go/ast` census with **each exclusion counted separately**.
- **An alloc census table carries ns/op BESIDE obj/op and B/op** — the only column separating a real zero from an ELIDED call (9 ns control vs 2,516 ns `DeepEqual(slice,slice)`).
- **Two instruments in two PROCESSES agreeing to the object make a census the reading of record**; **two INDEPENDENT derivations agreeing to the byte AND the object LOCATE a residue** — the re-probe is OFFERED, not owed.
- **A count-based bank condition (objects = 0) is served ONLY by candidates that remove OBJECTS**; a bytes-only residue (pins) is its own increment.
- **Measure a REPRESENTATION choice on the axis that SEPARATES its candidates, and name the instrument that can see the difference before the arms are built** — a syscall-dominated row cannot resolve a lookup at all.
- **A SATURATED COLUMN CARRIES NO CROSS-HOST INFORMATION**: where a value is FORCED by the population, two hosts agreeing is arithmetic, not corroboration — read the FREE columns. Hold probe objects LIVE for the whole run (a collected object's reused hash reads as a collision), write the prediction into the probe's source header before it runs, and measure a residual against the EXACT birthday expectation.
<!-- THE ALLOC INSTRUMENT'S OWN CONVERGENCE — six rules from one arc (2026-09-04), because a byte
     endpoint quoted off an unconverged instrument is a false measurement. A prediction's BASELINE is
     measured at the same scope in the SAME RUN, never quoted from a record — a 1,457.8 B/run
     baseline taken out of a design record read 1,510.8 under the acceptance's own filter (the
     comparability rule, met on the prediction side) — and a reduction LARGER than predicted does not
     make the prediction less wrong: both corrections go in the commit message, not only in the post.
     A reduction claim quotes the deterministic FLOOR (the minimum over reps) or a high-runs figure,
     and names its UNIT and its CONFIGURATION: the measured os row is 1,320.00 B/run in every
     configuration except Release+tiered (1,256, tier-1 escape analysis stack-allocating one
     non-escaping box), and a 100-run `AllocsPerRun` sample carries a fixed 0–800 B/run per-window
     accounting term, so it cannot resolve a change under ~150 B/run. golib's object COUNT is charged
     at the `new` while the BYTE cost diverges under a tiered JIT — a box the JIT stack-allocates
     still counts 1.00 — so no JIT improvement banks a count-conditioned row; only not constructing
     the boxes does. A COUNT-based byte prediction is a LOWER bound whenever the cut also UN-ESCAPES
     surviving boxes: six deleted boxes at 64 B predicted 384 and the converged floors read 512
     (1,320.00 → 808.00), the two surviving receiver boxes having stopped escaping. A minimum over 40
     draws of a 100-run window is unconverged by a near-constant offset (+42.5/+44.4 here) that
     CANCELS in a DIFFERENCE of two such minima — so a difference of unconverged minima may be right
     while both absolutes are wrong; record absolutes from the converged instrument, and when the
     count is exact and the bytes do not close, SEGMENT (a per-frame byte probe with literal tags, an
     exact segment sum and a one-row positive control) before naming a mechanism. An instrument that
     reads HIGHER on strictly FEWER objects is telling you it is NOT CONVERGED — a 40-rep minimum
     read 789.8 after a cut removed two boxes from a tree reading 785.0, impossible for a true floor
     — and that reading is REPORTED as non-convergence, never smoothed or explained. Finally, a
     static census of "boxes" is bounded by the INSTRUMENT's population: golib's counter counts golib
     allocation sites only, so a defer's delegate, a params array and an interface box are not among
     the counted eight, and a capability can have TWO populations of different sizes (239 sites
     minting a box AND a delegate, 207 a delegate alone) of which only the first is visible — census
     BOTH, segment the row with the converged instrument BEFORE sizing an increment against it, and
     size a lowering by a `go/ast` census with each exclusion counted separately, because the
     exclusions are the residue the null owns. A count is the unit that carried information at every
     step of that arc; the bytes needed the instrument.

     Four more from the same ladder's next increment (2026-09-04), each a way to quote a wrong
     endpoint. A cut that removes boxes an EARLIER cut already UN-ESCAPED saves ZERO bytes — deleting
     two receiver boxes read 744.25 → 744.25 B/run with the count 10 → 8, because an earlier
     increment had already priced those two at 0.00 B: 64 B is a property of a box that ESCAPES, the
     converse of the count-based LOWER-bound rule, so the unit is re-read from the CURRENT tree's
     segment table before every prediction and never carried down the ladder (that prediction was
     exact on the count and wrong by the whole 128 B). A reading is comparable only under the SAME
     WINDOW PROTOCOL as the record it is read against: the converged 744.25 B / 8 obj is the FLOOR of
     windows 2 and 3 of three 1,000,000-run windows in one process, while a 100-run single window
     read 898 / 916 / 895 — the count exact, the bytes ~150 B high and varying, because a short
     window cannot dilute the amortised slack — so stop-and-reconcile before reading anything against
     the record (a GC-configuration hypothesis, gen0 0 against 99, was tested with ONE variable and
     dropped when it moved nothing but the noise). Three ladder mechanics beside them: window 1 of a
     1,000,000-run `AllocsPerRun` reading always sits ~1.3 B/run above the integer floor while
     windows 2–3 land on it, so TWO windows is the minimum protocol; `-p:BaseOutputPath` and
     `-p:BaseIntermediateOutputPath` on the command line are GLOBAL properties that propagate into
     every referenced corpus project and mint parallel `obj-*` trees under `src/core` which then
     collide on the SDK's default compile glob (CS0579/CS1537), so an outside-the-repo probe project
     sets `EnableDefaultCompileItems=false` with one explicit `Compile` item; and a segment an
     instrument could not enter is reported COMBINED with its split marked DERIVED, cross-checked
     against an identical construction measured directly in the same run, so nobody quotes a
     derivation as a reading. And a LAYOUT read (`Unsafe.SizeOf<T>`) is decided by the field set at
     JIT time and cannot move with load, so the loaded-versus-solo rule does not apply to it — but a
     size row is an ASSERTION only after a control grows the struct by one word and reads RED; before
     that it is a baseline that passes either way.

     A REPRESENTATION choice is measured on the AXIS THAT SEPARATES ITS CANDIDATES (2026-09-05):
     three arms of a box-slot design read their predicted bytes to the byte on the alloc row — which
     separates only the slot's +8 B/box — and were told apart ONLY by a synthetic 10M-call hot loop
     (18.0 / 20.2 / 37.0 ns against a 33.8 baseline), because a syscall-dominated row cannot resolve
     a lookup at all (the TLS handshake sat in a 4% spread in no consistent order). Name the
     instrument that CAN see the difference before the arms are built, and state a corpus-wide cost
     as a PER-ROW FORMULA (+8 B × consumer-type boxes per op), never as a single verdict.

     Three more from the same ladder's tail (2026-09-05). An alloc census table carries ns/op BESIDE
     obj/op and B/op — it is the only column that separates a real zero from an ELIDED call (a 9 ns
     control against a 2,516 ns `DeepEqual(slice,slice)`) — and, per the instrument-population rule
     above, an allocation counter charging golib's own sites is structurally BLIND to CLR boxing, so
     a "floor of N boxes" stays a PROOF CLAIM until a byte-arithmetic instrument against a hand-boxed
     control measures it; two instruments in two PROCESSES agreeing to the object (probe 3 obj
     against suite 3 obj) is what makes a census the reading of record. Two INDEPENDENT derivations
     agreeing to the byte AND the object LOCATE a residue without a third instrument — a per-frame
     byte probe taken at an earlier base and a later cut's own measured total closing at
     120+88+120+56+104 = 488 B and 2+2+2 = 6 objects — so the re-probe is OFFERED, not owed. And a
     count-based bank condition (objects = 0) is served ONLY by the candidates that remove OBJECTS: a
     bytes-only residue (pins) is named as its own increment and never stands between the row and its
     condition.

     A SATURATED COLUMN CARRIES NO CROSS-HOST INFORMATION (2026-09-08). The CLR's identity hash was
     measured 26 bits wide on one flavour — the top six bits never set over a million
     simultaneously-live objects, uniformly distributed, collisions matching the birthday expectation
     — so a packing that drops the top two bits costs EXACTLY nothing, while a 16-bit packing
     collides in nearly every draw. But at that population a 16-bit column's collisions are FORCED to
     the population minus its bucket count, and an 8-bit control's likewise, so their exact agreement
     across two hosts is ARITHMETIC and not corroboration: "all four counts identical" would have
     read as strong support while two of the four were never in a position to dissent, and the only
     FREE column DID differ. Three mechanics ride with it: the prediction was written into the
     probe's own source header before it ran and scored as worded; objects are held LIVE for the
     whole run, because a collected object's reused hash reads as a collision; and the hash SEQUENCE
     is deterministic per host across processes, so such a number is a per-host CONSTANT rather than
     a sample — a guard built on it is stable and samples nothing, and a residual is measured against
     the EXACT birthday expectation, never its square-law approximation. -->
## False-green route #7 and its twins — the gate holes around golib/gen
**Route #7 = a change no standing gate compiles or runs.** CNR is transpile-only; the stdlib solution compiles PRODUCTION assemblies; the union sweep's roster is a fixed list of BANKED rows; a filtered behavioral runner never exercises the affected row. Four assemblies sit outside all of it:
1. **An UNBANKED package's `-tests` assembly is in NO standing gate.** Any converter change touching **lift identity, dedup registries or anonymous-type naming** owes a `-tests` CONVERT-then-BUILD of **`reflect`** at the **MERGE RESULT**, beside CNR (CS0050/51/52 shipped green here).
2. **A BANKED row's `-tests` assembly is in no gate either** — only a roster-wide `-tests` reconversion walks them. The same change owes a convert-then-build of a row with an EXTERNAL test variant — **`errors`**, the cheapest — beside `reflect` (CS0122, CS1503).
3. **A BANKED row whose TEST side carries a hand-owned `*_impl_test.cs` companion is compiled by no standing gate.** The class is named by the **FILE SUFFIX** (one `git ls-tree`, re-derived per train) and **the guard is a BUILD of each such row's test host at assembly** — never a grep over bridge internals, which rots with renames and cannot see a type-level break.
4. **A golib/reflect/gen change altering RUNTIME behavior while emitting byte-identical `.cs` is invisible to CNR *and* to the `-tests` build.** **The train battery runs the FULL behavioral suite — all phases, Output included — when a seat touches `golib`/`reflect`/`src/gen`**; it is the only leg that sees a behavioral regression. A full-suite PASS NUMBER is trustworthy only if the run ENUMERATED the affected row and was not stale-green (**route #2**).
- **CONVERT then BUILD.** A bare `-test-action build` consumes an EXISTING digest-validated manifest, and `go2cs_test_manifest.json` is machine-specific and git-ignored — on a clean box the run exits in **0 s** with `test manifest is missing`, which an error-pattern filter reads as a bare `exit=1` and a green-word grep reads as nothing. Use `-test-action convert` (or `all`) then the build.
- **The production-only two-seeded diff is blind to TEST-side emission.** A converter change emitting CROSS-PACKAGE references lands its corpus footprint in the SAME train (two-seeded diff applied verbatim, byte-identity asserted) **and** owes a `-tests` emission census of the banked rows it can reach.
- **The `-tests` driver MIRRORS `processConversion`'s analysis sequence BY HAND**, so a pass wired into the `-stdlib` driver binds NOTHING under `-tests`. **Wire a new pass into ONE sequence both drivers call, or make its guard run under BOTH.**
- **Cross-assembly lift reuse is admissible only if REACHABLE, and the axis is the test ASSEMBLY, not the Go VARIANT**: an internal variant, a PUBLIC candidate, **or an EXTERNAL variant whose test-project MODEL puts production's internals in sight** (whitebox-reference or recompile). Only the plain reference model — chosen exactly when the package has NO internal test file and so gets no `InternalsVisibleTo` grant — cannot see them. The grant is a CONSEQUENCE of the model (`selectTestProjectModel` → `insertFriendAssemblyAccess`), decidable without reading the csproj back.
- **A delta table carries build failures as their own REGRESSION column, distinct from movers.**
<!-- An UNBANKED package's `-tests` assembly is in NO standing gate — route #7's shape, one assembly
     over (found 2026-09-01 by a lane's own sweep, by no gate). CNR is transpile-only and the stdlib
     solution compiles PRODUCTION assemblies, so nothing at master ever builds the test emission of a
     package that has not banked: `reflect`'s `-tests` assembly sat compile-broken at master after a
     widened lift dedup bound a PUBLIC lifted struct's member shape to an INTERNAL prior lift — the
     dedup crossing ACCESSIBILITY tiers, CS0050/51/52 — with every standing gate green. Standing
     amendment: any converter change touching lift identity, dedup registries or anonymous-type
     naming owes a `-tests` CONVERT-then-BUILD of `reflect` at the MERGE RESULT, beside CNR.

     CONVERT then BUILD — a bare `-test-action build` measures NOTHING on a box that has not
     converted the row (measured 2026-09-04).

     The same hole has a second door: the production-only two-seeded diff is blind to TEST-side
     emission — a carrier stamp that dangled two banked rows lived in `x509_test.cs`, which `-stdlib`
     never writes, so the diff matched its prediction exactly and said nothing about the footprint
     that broke them. A converter change that emits CROSS-PACKAGE references therefore (a) lands its
     corpus footprint in the SAME train — the two-seeded diff applied verbatim, byte-identity
     asserted, exactly as a hand-own registration lands with its body — and (b) owes a `-tests`
     emission census of the banked rows it can reach, beside the `-stdlib` diff.

     The instrument that DOES walk every banked row's TEST emission is a roster-wide `-tests`
     reconversion, and it found what nothing else could (2026-09-02): a BANKED row (`errors`, 61
     verdicts) whose test assembly no longer built at master — a production-registry dedup arm whose
     accessibility guard reasons within ONE assembly (`v.inFunction` short-circuits) bound the
     EXTERNAL test variant's function-local lift to the production assembly's INTERNAL lift (CS0122),
     bisected to `5442b402e`, whose own blast-radius census was a `-stdlib` two-seed diff and
     therefore structurally blind to test emission.

     The axis is the test ASSEMBLY, not the Go VARIANT — corrected 2026-09-04 by measurement, after
     this line's first form said "an internal variant, or a PUBLIC candidate" and the converter's
     predicate agreed with it. `f38c2ae01` keyed reachability on `testExternalVariant`, which is
     right for `errors` (external-only suite, no grant) and wrong for every package that HAS an
     internal test file: there BOTH variants emit into the ONE `.tests` project, and the same fact
     that selects the whitebox model (`selectTestProjectModel`) is what makes the production csproj
     emit `InternalsVisibleTo $(AssemblyName).tests` (`insertFriendAssemblyAccess`) — so the grant is
     a CONSEQUENCE of the model and decidable without reading the csproj back. runtime paid it on
     EVERY target: `hash_test.go`'s `IfaceKey.i interface{ F() }` calls production `ifaceHash`
     through the `export_test.go` bridge, so the refusal minted a second `IfaceKey_i` and the call
     could not bind (`hash_test.cs(540,52) CS1503`), byte-identically on windows and linux. The two
     rules had been written by two arcs: `liftNameNeedsPublicType`, directly above the refusal,
     documents that same reuse as one hash_test.go "needs to compile at all". A documented invariant
     that a later seeding violates is a bug the doc cannot catch; a lift/dedup change owes a `-tests`
     convert-then-build of a row with an EXTERNAL test variant — `errors`, the cheapest — beside
     `reflect`; and a delta table carries build failures as their own REGRESSION column, distinct
     from movers. (A bisect converging on adjacent commits with BOTH controls valid is an
     attribution; the named suspects were exonerated by measurement, not by argument. And a bisect is
     not always OWED: where the green-to-red window holds exactly ONE commit touching the predicate,
     an instrumented reading names the SEAM as well as the commit for the price of one convert —
     2026-09-04, the CS1503 above.)

     The `-tests` driver MIRRORS `processConversion`'s analysis sequence BY HAND (measured
     2026-09-03), so a new converter pass wired into the `-stdlib` driver binds NOTHING under
     `-tests` until it is wired there too — self-caught by a darwin conversion emitting 0 records
     where 28 were predicted. Two hand-mirrored sequences drift.

     One assembly FURTHER over: a BANKED row whose TEST side carries a hand-owned `*_impl_test.cs`
     companion is compiled by NO standing gate either (2026-09-05, `internal/reflectlite`). CNR is
     transpile-only, the stdlib solution compiles PRODUCTION assemblies, and the union sweep's roster
     is a fixed list — so an abi/golib API retirement broke that row at master for TWO trains with
     every gate green, found only by a lane's own banked-row sweep. The class is named by the FILE
     SUFFIX (one `git ls-tree`, re-derived per train — 2 packages at `9c44a6d6a`) and the guard is a
     BUILD of each such row's test host at assembly.

     FALSE-GREEN route #7's BEHAVIORAL twin — a golib/reflect/gen change that alters RUNTIME behavior
     while emitting byte-identical `.cs` is invisible to CNR *and* to the `reflect` `-tests` build
     (found 2026-09-03; it shipped through TWO trains). CNR is transpile-only and byte-identical by
     construction here; the `-tests` gate is compile-only; and a FILTERED behavioral runner never
     exercises the affected row — so nothing sees it. `ReflectArrayOf`'s identity assertion
     (`reflect.SliceOf(reflect.ArrayOf(…))` == the declared slice-of-array type) went red at the
     descriptor-cargo increment and was caught only when a darwin census flagged it as an
     outside-model movement and the coordinator reproduced it on windows. Corollary: a full-suite
     PASS NUMBER quoted in a seat message is trustworthy only if the run ENUMERATED the affected row
     and was not stale-green (route #2), so a seat claiming NNN/0 Output over a tree with a known-red
     Output-compared row is reconciled before the number is believed. -->
## Reading a behavioral PASS; guards that pay for themselves
- **A behavioral project's PASS is PER PHASE.** Without `[GoTestMatchingConsoleOutput]` a project passes **Target ONLY** — emitted text byte-compared, printed output never diffed against `go run`. **A post quoting a guard's PASS says which PHASE compared what.**
- **`0 compared, 0 failed, skip 1` on the Output line is NOT a pass** — **route #6** in a costume. **Read the comparison COUNT, not the banner.** Cause: a FRESHLY transpiled `package_info.cs` lacks the hand-added `[GoTestMatchingConsoleOutput]`, because the converter preserves it by reading the EXISTING file at the output path.
- **A banked SINGLE-VERDICT row whose mechanism lives in `golib` is guarded NOWHERE but that row**, and where the load-bearing half is a WIRING line in a hand-own (a `runtime.GC()` into a cache clear) its DELETION stays green everywhere. It takes a **GolibTests guard whose WIRING arm pays for the file** — the arm that fails when the line is removed, not the arm restating what the library does.
- **A guard's header is a CLAIM that ages** — correct it at the site, name the owning increment, and **never turn an arm on for the strength of a comment**.
- In a file carrying `using static go.runtime_package`, the bare name `GC` binds Go's `runtime.GC()` and **SHADOWS `System.GC` (CS0119)** — qualify it.
- **A design RULING is not reversed inside a FIX commit**, and **a row whose runtime pass was a PUN surviving by luck is DEMOTED to the compile-shape guard its own comment already claimed** — demotion stated in the commit as a SCOPE change, golden re-baselined from the REBUILT binary.
- **EACH FIX BUYS THE NEXT PHASE'S MEASUREMENT, AND THE NEXT PHASE UNMASKS THE NEXT DEFECT** — budget a new guard file as a LADDER (Transpile → Compile → Output), not as one cut, and expect the row added for one defect to surface the next.
- **A GOLDEN IS HELD ON A COMPILE RED WHATEVER CAUSED IT**: hold it, MEASURE that the landed fixes' converter delta cannot reach the new site, and name the new row as NEW in that commit. **A defect too deep for the seat is NAMED, not absorbed** — locate its class, NARROW the guard's rows to the property the seat actually delivers, bank the runtime half as DEBT in the guard's header, and file the design with a re-census before any cut.
<!-- Its narrowest member: a banked SINGLE-VERDICT row whose mechanism lives in `golib` is guarded
     NOWHERE but that row (2026-09-04).

     And a behavioral project's PASS is PER PHASE (2026-09-04): a project without
     `[GoTestMatchingConsoleOutput]` passes Target ONLY — its emitted text byte-compared, its printed
     output never diffed against `go run` — so a golib RENDERER change guarded by a Target-only
     project has no guard at all: a battery reported `ChanElemDims PASS` with the value row genuinely
     RED (`chan [3]int` rendering as `chan []int`). The reader's own pre-read carried the tell
     (`Output: 0 compared`) and read it as vacuity rather than as an unmeasured red. And a guard's
     header is a CLAIM that ages — this one said an increment would close the row while that
     increment landed slices-only. One guard-author mechanic from the same family: in a file carrying
     `using static go.runtime_package` the bare name `GC` binds Go's `runtime.GC()` and SHADOWS
     `System.GC` (CS0119).

     And the tell has a CAUSE worth knowing: an Output line reading `0 compared, 0 failed, skip 1` is
     NOT a pass (2026-09-05) — a FRESHLY transpiled `package_info.cs` lacks the hand-added
     `[GoTestMatchingConsoleOutput]` (the converter preserves it by reading the EXISTING file at the
     output path, so there is nothing to preserve where the file was regenerated from scratch), the
     Go-vs-C# comparison never runs, and Transpile/Compile still read true: route #6 in a costume.

     A design RULING is not reversed inside a FIX commit (2026-09-05, the reinterpret remedy): a fix
     that made a reference-bearing box's reinterpret a GC-safe VIEW over the slot contradicted the
     seated design's loud-failure choice, and the design's own retention guards went RED on it (2 of
     651) — the guard working, the fix withdrawn. Its companion, when the row was reconsidered
     honestly: a row whose runtime pass was a PUN surviving by luck (a pointer-to-pointer reinterpret
     through a transient slot address) is DEMOTED to the compile-shape guard its own comment already
     claimed, with the demotion stated in the commit as a SCOPE change of that row and the golden
     re-baselined from the REBUILT binary.

     EACH FIX BUYS THE NEXT PHASE'S MEASUREMENT (2026-09-08): ONE guard file surfaced three defects in
     a day, each by a row added for the previous one — it carried a second defect in its own emission,
     written for the first, and the rows added for the third unmasked a fourth. Two fixes let the
     project reach Compile and unmasked a literal-emission drift (a `uintptr` initialised from an
     above-`uint32` constant); that fix let it reach Output and unmasked a `*[N]T` conversion over a
     MANAGED scalar box, emitted as a `uintptr` token round trip whose array view reads the pointee as
     an array HEADER — IndexOutOfRange, deterministic (the `NativeBox<array<T>>` shape named under the
     LIFETIME class below, one symptom over). The golden was HELD on that Compile red, it was MEASURED
     that the two landed fixes' converter delta cannot reach a variable-declaration initializer, and
     the new row was named as NEW in that commit: a pre-seat reading, nothing at master affected. The
     defect too deep for the seat was NAMED rather than absorbed — its class located as the
     reinterpret-VIEW half of the `[GoValueClone]` population, the guard's rows narrowed to the
     property the seat delivers, the runtime half banked as DEBT in the guard's header, and the design
     filed with a re-census before any cut. -->
## The testing host is a cross-assembly contract
**An interface member added to the hand-owned testing host breaks EVERY cross-assembly adapter, and the host compiling green is exactly why it is dangerous** — route #7 in the testing host's clothes. `TB.Context()` at Go 1.24 breaks **all 57** assemblies carrying `[assembly: GoImplement<…, testing_package.TB>]`: go2cs-gen mints each adapter's forwarders from the interface's member set AT COMPILE TIME, so the forwarder names `go.context_package.Context` in a consuming assembly that does not reference `context` (**CS0234 / CS0012 / CS9334**). **It cannot fix itself, deliberately**: the emitted csprojs set `DisableTransitiveProjectReferences=true`, so a project's reference set is exactly its Go imports, and Go's test files do not import `context` to call `t.Context()`. **The reference must be INJECTED the way `testing` already is; the existing injection is the specification.** Only a cross-assembly CONSUMER compile catches it — **`archive/zip`, which adapts both `B→TB` and `T→TB`, is the natural canary.**
<!-- Measured 2026-09-07. -->
## Route #7's ATTRIBUTION mirror
**A crash INSIDE a generated shell is usually the shell being faithful — trace to the ASSIGNMENT, not the frame, before billing `src/gen/`.** **The cheapest instrument for reaching that assignment is the BUILT GUARD BINARY run by hand** under the right runtime root (`DOTNET_ROOT`): the runner captures only the FIRST stderr line (`Fatal error.`) while the binary prints the whole managed stack, and the frame names the assignment.

**A CORRECT GENERAL RULE APPLIED TO AN UNVERIFIED PARTICULAR IS STILL A GUESS: the rule names the SHAPE, the derivation names the MEMBER — run the derivation BEFORE the claim.** For skipped `GolibTests` project references the derivation is one grep of the csproj `ProjectReference` graph; "dependents are skipped, not errored" is TRUE and still does not name WHICH root, so picking the failure already in mind can hide two more blockers.
<!-- Measured 2026-09-02, runtime's `textAddr`. The `RecvGenerator` shell's DerefOrNull → NullRef →
     NRE on the first field touch IS Go's nil-receiver semantics; the nil came from `funcInfo()`'s
     module search, which can never succeed because the package's sole moduledata is a permanent
     empty stub (`len(pclntable)==0` skips it every time). A structurally guaranteed nil is not a
     race and not goroutine-specific — which tests crash is decided only by which ones reach the call
     at all. The built-binary instrument: 2026-09-05 — two runs, five seconds, in place of a four-arm
     bisect that had already been priced.

     A CORRECT GENERAL RULE APPLIED TO AN UNVERIFIED PARTICULAR IS STILL A GUESS (2026-09-08, lane R,
     twice in one day): "dependents are skipped, not errored" correctly said ONE root sat behind five
     skipped GolibTests references — and the root was then PICKED, the failure already in mind, rather
     than DERIVED. One grep of the csproj `ProjectReference` graph, run only to audit an
     already-published claim, found the real root (`fmt` → `slices`, CS8761, 99 dependents) and moved
     that seat's acceptance from one blocker to three. -->
## Stubs, diagnostics and censuses
- **A GENERATED ARTIFACT'S BOILERPLATE DIAGNOSTIC IS NOT A FINDING ABOUT THE SPECIFIC SYMBOL — it describes the SHAPE THE GENERATOR FOUND.** Generalizes: **any templated message is evidence about the TEMPLATE'S TRIGGER CONDITION, not the instance.** Frontier-vs-wiring is decided by whether a body and a push exist upstream — two greps — never by the stub's own words.
- **The fix for a misleading diagnostic is usually to CLAIM LESS, not COMPUTE MORE.** A source generator sees ONE compilation and cannot answer "is this implemented somewhere else"; what it knows is **"nothing in THIS compilation implements it"**. **Before building an oracle a tool cannot be, check whether it is merely SAYING more than it knows.**
- **A diagnostic reading its evidence from a COMMENT inherits that comment's conditionality** — a linkname sits in leading trivia and survives only under `-comments`. **A signal that must be durable belongs in an attribute the converter emits unconditionally.**
- **A SUPPRESSED LINKNAME PUSH LEAVES A THROWING DESTINATION NO GATE FAMILY CAN SEE UNTIL SOMETHING CALLS IT** — CNR is transpile-only, the census compiles without running, the behavioral suite reaches only what a program calls. **The cost is not the wall, it is that the wall BILLS THE WRONG COMMIT.** The class **splits three ways, only one a defect**: (1) **displaced** by a registry entry or `_impl.cs` — not a member, the body exists; (2) generator fills it and **Go has no implementation either** (asm, cgo) — an honest refuse-by-name; (3) generator fills it, **Go HAS an implementation, and a push exists that did not arrive** — lost functionality in the costume of a deliberate refusal. **A census returns bucket 3 BY NAME; an undifferentiated count tells nobody whether to worry**, and the sound oracle is **each built package's generated stub file**, not a text predicate over declarations — the text bounds the CONTAINER, not the population.
- **"Recovers catchability, not capability" — keep those apart in any option list.** **"Wired on paper and still throwing" is a third state**: `readProfile` carries a body AND its linkname directive and still throws, because a directive existing is not the push ARRIVING. **Name the state, not the symptom — all three present as the same throw.**
- **A STUB THAT THROWS IS DIAGNOSABLE WHERE A BODY RETURNING ZERO IS NOT.** For a load-bearing intrinsic the remedy is to **SEVER the consumers onto the managed walk by registry displacement**, plus a write primitive on the flavour where every runtime throw is otherwise MUTE. **The cheapest falsifier runs before any line is written**: a user program reaching a converted runtime fatal, its stdout, stderr and exit code read per flavour.
- **AN EXIT CODE CANNOT DISCRIMINATE A WORKING RUNTIME FATAL PATH FROM A DEAD ONE ON WINDOWS**: golib's unhandled-exception backstop writes the exception text and exits **2** for ANY unhandled managed exception, so "the exit becomes 2" is satisfied VACUOUSLY by the not-implemented throw. **Key such an acceptance on the stderr SHAPE** (Go's goroutine header and frame order) **and on the exception's ABSENCE.**
- **`go run` REPORTS A DIFFERENT EXIT CODE FROM THE BUILT BINARY ON A RUNTIME FATAL** (1 against 2) — **a probe whose PRIMARY READING is the exit code BUILDS and runs the binary**, capturing the code as the FIRST statement after it. **A probe's own markers go through the formatting package, never the builtin print** (whose lowering enters the very runtime print path the probe predicted dead), and **a proposed TRIGGER is MEASURED before it is used** — an unlock-of-unlocked-mutex fatal never enters the runtime's fatal path, because the hand-owned mutex declares its own local hook.
- **"NOTHING REACHES IT" IS NOT A REASON TO IGNORE A LATENT DEFECT — IT IS THE PRECISE CONDITION THAT MAKES IT A BOOBY TRAP. A KNOWN-RED ROW THAT NO STANDING GATE REDDENS IS THE WORST KIND OF RED**: verifying that a schedule cannot see the red argues AGAINST boarding it. **A fact that points somewhere is not an argument for going there.**
- **A LATENT-MISATTRIBUTION HAZARD AND AN OBJECTIVE ACCELERATOR ARE DIFFERENT CLAIMS NEEDING DIFFERENT EVIDENCE** and are easy to conflate when the population is the same — "this will bill the wrong commit someday" needs only the class and the gate gap; "connecting these advances the objective" needs something that currently REACHES them. **State which claim a population is offered for.**
- **A MAP KEYED BY A DIRECTIVE'S DESTINATION IS BLIND TO THE OPPOSITE WIRING DIRECTION.** A PUSH is the producer naming its consumer; a PULL the consumer naming its producer — **45 push / 53 pull / 0 both, fully disjoint**, so "wired" was 98 where a destination-keyed census said 45. **The blind spot is a property of the KEY; no amount of re-running finds it.**
- **A REGISTRY CHECK CANNOT SEE A DEPARTURE THAT DOES NOT GO THROUGH THE REGISTRY — enumerate the ways a member can LEAVE a population before trusting a census of it.** A whole-file `GoManualConversion` replacement and a bodyless-partial completion each remove a member from the unimplemented-stub population without touching the push registry. **The settling instrument is the GENERATOR'S OWN OUTPUT** — a structural after-state reads off ARTIFACTS rather than verdicts.
- **A MARKER-KEYED CENSUS CANNOT OBSERVE A FILE THAT CARRIES NO MARKER BY DESIGN.** The frozen-metadata class — a package whose every production file is hand-owned, so the driver never re-emits its `package_info.cs`, csproj or README — is invisible to any census walking MARKED files. **Corollary: the marker that protects a hand-own from being CLOBBERED protects an ORPHANED hand-own from being CLEANED UP** (the deletion pass tests PROTECTED first), so orphan removal at a hop is explicit work.
- **The `-stdlib` QUEUE IS `go list std` AT THE SOURCE RELEASE, so a package RENAMED at the target never enters the target's queue and no emission re-mints its metadata** — un-freezing frozen metadata cannot clear a straggler (**CS0426**) there: the RELOCATION clears it, and the un-freeze only makes the destination self-minting rather than re-frozen at the NEXT hop. **Read the queue's POPULATION before assigning a remedy a leg.**
- **A PACKAGE-DELTA CENSUS KEYED REMOVED→SUCCESSOR IS BLIND TO A PACKAGE THAT GAINS FILES FROM ONE THAT IS NOT REMOVED.** **A hand-own census at a hop compares each hand-own's PRINCIPAL line count across BOTH releases.**
<!-- All seven pprof stubs say "external (assembly or cgo) function is not implemented", including
     two with demonstrable bodies at known lines; three participants read that text as a statement
     about the function and produced three separate misclassifications from it. The remedy is not
     "read stubs more carefully": it is to make the stub say what it knows ("a linkname names this
     symbol and the body was not linked"), because a tool that has the information and emits
     boilerplate instead is spending its users' attention to save its own. AMENDS THE ABOVE ON THE
     REMEDY (2026-09-07): the proposal was to teach the generator to distinguish wiring from
     frontier, which required a converter-emitted marker for it to read. A source generator sees ONE
     compilation and structurally cannot answer "is this implemented somewhere else"; its own
     attribute doc already states that equivalence and defends it against the two proxies that fail.
     So the defect was never missing capability: the PROSE asserted a CAUSE the equivalence does not
     establish. Delete the unwarranted half and the message becomes true, the marker machinery is
     unnecessary, and the fix shrinks. And a diagnostic that reads its evidence from a COMMENT
     inherits that comment's conditionality: the linkname sits in leading trivia and survives only
     under `-comments`, so a generator keyed on it would go quietly generic on any emission without
     them — the comment is the cheap read, not the reliable one.

     A SUPPRESSED LINKNAME PUSH: this class existed for three weeks precisely because a board entry
     said "no managed body" (capability) where the truth was "body exists, unreachable across the
     assembly boundary" (wiring) — two readings that send a lane to completely different work. A loud
     refusal a lane can find and attribute is worth a great deal and is not the same as the thing
     working. A remedy sized for the missing-directive case does not touch `readProfile`.

     A STUB THAT THROWS IS DIAGNOSABLE WHERE A BODY RETURNING ZERO IS NOT (2026-09-08): the runtime's
     fatal path reaches the caller-PC and caller-SP intrinsics, whose PC is LOAD-BEARING in Go (the
     traceback's starting frame, consumed unconditionally on a runtime throw) and DEAD here (an
     always-empty program-counter table) — so a `return 0` body would dereference address zero inside
     the traceback's own initialisation and turn a NAMED refusal into a wild read; the tree already
     refused it in writing, under "not implemented here, on purpose". The remedy is the precedent one
     function over: SEVER the consumers onto the managed walk by registry displacement, plus a write
     primitive on the flavour where every runtime throw is otherwise MUTE.

     EXIT CODE on Windows (2026-09-08): the increment's acceptance keys on the stderr SHAPE and on
     the exception's ABSENCE. The probe's structural half held exactly (the text once, death at the
     caller-PC intrinsic), and the falsifier that would have retired the increment — exit 2 WITH a
     Go-shaped traceback — did not fire. `go run` exit code measured 2026-09-08.

     LATENT DEFECT REASONING: a 28-member cluster of throwing destinations was shown unreached by its
     package's own suite, which deflated the claim that connecting them would move the row; it
     STRENGTHENED the original hazard, because unreached is exactly why no gate sees them and exactly
     why the first cut that finally reaches one gets billed for a wall it did not build. Supply a
     pointing fact accurately, label what it is, and let the conclusion be argued. Promoting the
     misattribution-hazard claim into the accelerator claim is a silent upgrade; the accelerator
     claim turned out to be FALSE for the largest family.

     Push/pull measured 2026-09-06: 45 push / 53 pull / 0 both, fully disjoint. Registry-departure
     blind spot (2026-09-07): the destination-keyed map's blind spot one layer over — a
     registry-keyed census could not observe those departures even in principle, however carefully it
     ran. The settling instrument: `runtime/pprof`'s generated stub directory held 7 files before a
     landing and 1 after, six bodyless partials having gained real implementing parts, with no
     comparison record and no confound about which of 53 commits did it. PACKAGE-DELTA CENSUS
     (2026-09-08): at 1.24.13 `sync/mutex.go` shrinks 261 → 66 lines — a delegating wrapper — and its
     234-line spin-and-park implementation moves to the NEW `internal/sync`, the exact mechanism
     `sync/mutex.cs` is hand-owned for; the committed census row `internal/concurrent →
     internal/sync | 3/3` is TRUE while saying nothing about it, because `sync` is not a REMOVED
     package. Ten `sync` hand-owns, one moved principal.

     A MARKER-KEYED CENSUS CANNOT OBSERVE A FILE THAT CARRIES NO MARKER BY DESIGN (2026-09-08, lane
     G) — the key-blindness family again, one file class over from the destination-keyed map and the
     registry-keyed census above. A hop alias census walked MARKED files and so could not see the
     frozen-metadata class: the four packages whose every production file is hand-owned, which is why
     the driver never re-emits their `package_info.cs`, csproj or README. Re-derived at master, 2 of
     the 4 are affected and BOTH RELOCATE at the next release as renames, so an alias hand-edit would
     have patched metadata for a package that will not exist. The sizing ruled the relocation COMPOSED
     with an un-freeze: un-freezing the metadata is a converter seat (its footprint at the current
     release is those four packages' metadata re-minted, the eight missing forced-init hooks
     arriving), and the relocation rides the hop's hand-own branch. Its corollary is the protection
     marker read from the other side: the deletion pass tests PROTECTED first, so the marker that
     keeps a hand-own from being clobbered also keeps an ORPHANED hand-own from being cleaned up.

     UN-FREEZING THE FROZEN METADATA CANNOT CLEAR A STRAGGLER IN A PACKAGE THE QUEUE NEVER VISITS
     (2026-09-08, lane G) — this SUPERSEDES the implied half of the ruling directly above, and both
     are kept. The coordinator's "relocation composed with un-freeze" ruling STOOD; its implied "the
     un-freeze levels the straggler" did NOT: the `-stdlib` queue is `go list std` at the SOURCE
     release, so a package RENAMED at the target release never enters the target's queue and no
     emission re-mints its metadata. The RELOCATION is what clears the CS0426; the un-freeze is only
     what makes the relocation's destination self-minting rather than re-frozen at the NEXT hop. -->
## The native boundary, class 1: LAYOUT
**ROOT: the CLR gives AUTO layout to any struct holding a reference-typed field and REORDERS it, so the KERNEL READS THE WRONG FIELD** — not "one word where four bytes belong". **Never pass a managed struct to native code by address**: ENCODE into a native buffer (a `writeNativeSockaddr`) or an explicit-layout / `fixed`-buffer blittable mirror, usually with a registry displacement. **A correctly laid-out struct (`Iovec`) can still hand the kernel managed addresses**, so correct layout is not sufficient.

| Site | Symptom |
|---|---|
| `syscall.GetTimeZoneInformation` wrapper | `Timezoneinformation`'s `array<uint16>` name fields are managed references where Windows expects inline `WCHAR[32]` → access violation via `time.Now().Weekday()` → `initLocal()` |
| `Msghdr` | `Namelen` at managed offset 40; kernel reads `msg_namelen` at 8, finds `Iov` (an object reference, never zero) → **EISCONN** on a connected stream, **EINVAL** on a datagram |
| `RawSockaddrInet4` | `Addr` at managed offset 8 is the heap pointer earlier instrumentation dumped |
| `readMapping` → `Module32FirstW` | managed `ModuleEntry32` auto-layout ~64 B against the native **1080**, `module.Size` folded to 1080 — overwrites a kilobyte of heap; faulted in one test, surfaced as an unrelated cctor crash in another |
| `rtlGetVersion` (`ntdll`) | `RtlGetVersion` **WRITES** through the pointer → **0xC0000005**, where libc would only have returned EFAULT |

- **Sibling class:** a wrapper taking a **`**T` OUT-parameter** arrives as **NULL**, because `ж<T> → uintptr` answers 0 for a heap-boxed pointer that is still nil. `crypto/x509`'s Windows system verifier is the measured consumer.
- **Third fork — no wrapper is at fault:** where the kernel memory is a byte buffer the CALLER reinterprets, no mirror-the-wrapper remedy applies. `net.adapterAddresses` is **CLOSED** (hand-owned `core/net/windows/interface_windows_impl.cs` transcribes the whole `IP_ADAPTER_ADDRESSES` chain into managed boxes; guarded by `IpAdapterAddresses`). The CryptoAPI chain walk reading `CertContext` / `CertChainContext` back through raw addresses is **still open**.
- Hand-owns in place: `core/syscall/windows/zsyscall_windows_impl.cs` (per-GOOS since r50a), guarded by `LocalTimeZone` — which compares real zone abbreviations and offsets against `go run`, not merely the absence of a fault; `GetAddrInfoW`/`FreeAddrInfoW` guarded by `LookupServicePort`. **The running census and per-member remedy live on [`docs/phase4/BOARD-next-validation-candidates.md`](docs/phase4/BOARD-next-validation-candidates.md) — re-measure there rather than carrying a count.**
- **A three-arm A/B (hand-own / generated body restored / hand-own back, `--no-incremental`) turns a "does not reproduce" into an attribution.**
- **A finding's prose is re-derived from its record before a cut is aimed** — a routed description named two other functions entirely and the RECORD named neither.
- **An UNGATED row can read "unchanged" after a real fix when an earlier host-killer runs first** — only a gated run measures it.
- **The class a token cut makes LOUD is a PER-PLATFORM census question, and such a cut's acceptance names the platform where the native side does not probe**: "EFAULT rather than reordered memory" holds only where libc merely READS the pointer.
- **Marshalling answers a LAYOUT problem; a pointer the OS issued and REFERENCE-COUNTS is an IDENTITY problem, and no correct marshalling can answer one.** The remedy for a MISS on an identity seam is to **REFUSE BY NAME**, never to rebuild — the hit path REMEMBERS an address rather than reconstructing one. **Offering marshal-on-miss and refuse-by-name as two options on ONE axis is the sizing error.**
<!-- Windows local time works (fixed 2026-08-01). Binding the converted `time` exposed a pre-existing
     crash the stub had hidden. The CLASS is still open, and it is now TWO classes — wrappers passing
     a non-blittable struct by ADDRESS (the layout defect) and wrappers taking a `**T`
     OUT-parameter. The old note said "nothing exercises them today; `net` and `crypto/x509` will" —
     both now do: `net`'s DNS path forced `GetAddrInfoW`/`FreeAddrInfoW` (fixed 2026-08-16, guarded
     by `LookupServicePort`), and `crypto/x509`'s Windows system verifier is the measured consumer of
     the OUT-parameter class. Two walls stood behind them, both `net` / `crypto/x509` arcs rather
     than syscall ones — a third fork, where the kernel memory is a byte buffer the CALLER
     reinterprets, so no wrapper is at fault and no mirror-the-wrapper remedy applies. The first is
     CLOSED: `net.adapterAddresses` walked a native `IP_ADAPTER_ADDRESSES` chain out of a managed
     byte buffer and killed the process on the loop's own nil test; hand-owned since 2026-08-17
     (`core/net/windows/interface_windows_impl.cs` transcribes the whole chain — every record, its
     six nested lists and every sockaddr — into managed boxes), guarded by the `IpAdapterAddresses`
     behavioral test, and it is what unblocked Windows name resolution at all, since `dnsReadConfig`
     is `getSystemDNSConfig`'s only source of DNS servers. The second is still open: the CryptoAPI
     chain walk reads `CertContext` / `CertChainContext` back through raw addresses.

     The ROOT measured 2026-09-02. Converted `Msghdr`'s `Namelen` sits at managed offset 40 while the
     kernel reads `msg_namelen` at 8 — where it finds `Iov`, an object reference and therefore never
     zero: EISCONN on a connected stream, EINVAL on a datagram; `RawSockaddrInet4`'s `Addr` at
     managed offset 8 is the heap pointer earlier instrumentation dumped.

     The remedy's BOUNDARY (2026-09-06): a fallback handing the kernel a marshalled native image of a
     managed view hands back an address the OS never issued — fatal where the site FREES memory the
     OS owns. The certificate-context helper censused five producers — three native-backed, where the
     fallback is right, and two managed views that both remember — leaving the reaching set for the
     fallback EMPTY.

     The class's FIFTH member (2026-09-03): `readMapping`'s remedy is the class's — a `fixed`-buffer
     mirror plus a registry displacement — and its positive evidence is a downstream test getting
     PAST the API that validates the record size.

     A TOKEN CUT'S ACCEPTANCE NAMES THE PLATFORM (2026-09-05, a union battery): two behavioral guards
     went red on `rtlGetVersion`, reached on every Windows TCP dial, where master had read silent
     zeros. Every REACHED member is a hand-own of the `GetTimeZoneInformation` shape. -->
## The native boundary, class 2: LIFETIME — pins, retention, function pointers
- **THE POPULATION IS THE PREDICATE — an address reaches a native call with no holder alive across it — NEVER THE IDIOM.** 43 `(uintptr)Ꮡ(` sites are the idiom's count, not the hazard's.
- **golib's `uintptr` operator pins DURABLY but only for the BOX's lifetime**: `(uintptr)Ꮡ(a, 0)` without HOLDING the box lets the backing array move during the native call. Sharper — **the pin's holder is FINALIZABLE and sits on a box that is garbage the instant the take returns**, so the pin is released on the finalizer's schedule and "the four takes are atomic" holds only while nothing runs between them.
- **A pointer type with a retaining and a non-retaining constructor is a trap wherever an implicit conversion can select the non-retaining one — mint through the retaining door.** The `syscall` read/write wrappers mint through the NON-RETAINING door (implicit box-to-`uintptr`), so only concurrency inside the kernel window sees it: **77 sites, `KeepAlive` zero corpus-wide**. It compiles, emits byte-identical C# and passes every SERIAL gate.
- **Retention and pinning are two properties a kernel-bound pointer needs BOTH of** — taking an address inside `fixed` retains the BOX but not the PIN.
- **A retention fix is keyed on the ARGUMENT's shape (a bridged same-frame box), never on the callee's KIND** — a `(uintptr)` bridge strips retention from a PINNABLE same-frame box exactly as from a syscall buffer, and a CONVERTED callee resolving the number through validate-on-read refuses after any GC between mint and resolve. A weak token does not retain either.
- **A NAME-keyed predicate is per-platform BY CONSTRUCTION** — route #8's shape, one target over. **A guard reading ZERO on a target is either "no sites" or "blind", and only a PER-TARGET positive control (the guard RED on a tree known to carry unheld sites there) tells them apart.**
- **TRANSCRIBE the reference compiler's predicate rather than paraphrase the rule.** `cmd/compile`'s `escape.rewriteArgument` tests the OPERAND TYPE (`IsUnsafePtr` into `IsUintptr`), so Go's guarantee covers the two-step form its own generated wrappers use 16 times per platform — one wrong sentence ("never through an intermediate variable") was the whole gap. **A closed arc's set can be NARROWER than the Go contract it reproduces**: Go marks FOUR functions `//go:uintptrkeepalive` — `RawSyscall`, `RawSyscall6`, `Syscall`, `Syscall6` — and the converter's funnel set covered only the `Syscall` family, leaving eleven generated Linux wrappers with no keep-alive. **Re-derive such a set from Go's own annotations.**
- **THE REFERENCE-BEARING PIN MISS IS STRUCTURAL, NOT ARC-BLOCKED.** For a reference-bearing `T`, `StandardBox` allocates no `m_slot`, so `PinnableStorage` is null, `EnsureStableAddress` never calls `PinnedBuffer.PinOnly`, `m_pin` stays null and **no `PinnedBuffer` is ever CONSTRUCTED** — "the pin was released by its finalizer" cannot be the mechanism. The address is REGISTERED anyway and validate-on-read refuses it (`IsPinnedAt` false when `m_pin` is null), so the recovery MISSES and the consumer holds a native alias of an address nothing was asked to keep still. **A `labelMap` carries references, so it can NEVER pin.** Guarded at `PinnedBoxStalenessWitnessTests.cs` (5/5 Debug, 5/5 Release+TC0, `Skipped: 0` load-bearing). **A probe using `ж<long>` measures the CONTROL, not the case** — `long` is reference-FREE, so it allocates `m_slot`, pins and resolves.
- **A B/op census cannot SEE what a pin COSTS**: the `GCHandle`'s handle-table entry never lands on the GC heap, and a FINALIZABLE holder whose finalizer is suppressed only on a `Dispose` the contract never calls rides the finalization queue, is promoted at least a generation and frees its handle on the finalizer thread — **GC pressure proportional to pinned-box COUNT, reported as ZERO bytes**. **Price a pin from the TYPE** (header, fields, padding, handle, finalizer) **and the QUEUE it joins.** A remedy moving WHERE storage is allocated (pinned-object-heap boxes) **is its OWN increment** with its own footprint and cost pair.
- **CLR function pointers, for any hand-own handing a Go func value to native code:** `Marshal.GetFunctionPointerForDelegate` **REFUSES a GENERIC delegate TYPE** whatever the target kind or overload — so a Go func value needs a **NON-GENERIC unmanaged-function-pointer shim per arity** forwarding by a TYPED call; **`DynamicInvoke` never applies a user-defined implicit conversion**, so the forward is TYPED or it silently narrows the contract; and **the native pointer's identity is per delegate INSTANCE**, so a stable Go-style numeric handle needs the shim instance **CACHED — and that cache is also the rooting the pointer needs for process life.**
- **A NATIVE CALLBACK'S ARGUMENT BRIDGE IS A REINTERPRET, NOT A CONVERSION.** Go's own callback wrapper copies each argument's SIZE BYTES from the native word at its ABI offset — raw bits — so the shim's typed forward reads the LOW bytes of each word into the converted parameter type, **refusing BY NAME where the type is wider than a word**. An expression-tree compile, chosen because its convert node resolves user-defined operators, would have failed at compile time on the first struct-parameter arity: **the premise "a conversion the binder refuses" was the wrong reading of what the forward must DO.**
- **A DOOR CAN BE RIGHT ON CLASS AND WRONG ON ITS OWN STATED REASON — audit a refusal's CLASS and its stated PREMISE separately.** `runtime`'s six managed-pointer-token refusals are 6 of 6 the reference-bearing-pointee class, refused by name, and none of them the pin-unheld hole — yet 5 of the 6 refuse on a premise ("native code would read or write memory that is not the caller's") that is FALSE for them: the five callback rows pass a funcval pointer at argument 3 as an **OPAQUE PASS-THROUGH COOKIE** the OS carries back to Go's own `callback`, which reinterprets and calls through it IN GO — native code never dereferences it. Those five rows are ONE funnel with SIX callers. **No remedy is owed from a results file**: what is owed is the emission route MEASURED, the inbound half (can `callback` recover its box) measured on the SAME row, the sixth caller run, and a falsifier — a native callee that STORES and DEREFERENCES the cookie.
- **A NAMED COMPLETENESS GAP CAN BE A DIFFERENT FACT ALREADY IN THE ARTIFACT — resolve it from the PRESERVED RECORD before dispatching a solo run.** That funnel's sixth caller is absent from the refusal list because it NEVER REACHES the token door: Go=pass / C#=fail in 1 ms at its own line on `runtime.LockOSThread didn't`, before any syscall wrapper — so the funnel reads FIVE of six with the sixth **UNMEASURED with respect to the door**, neither counter-example nor confirmation. **A member missing from a population is UNMEASURED until its reason is read.**
- **A REMEDIED member of a class can still be EXPOSED when the class's RULE changes underneath it** — a hand-own written BEFORE the managed-pointer token cut can be correct in SHAPE and broken in FACT if its own body takes the `uintptr` of a reference-bearing box. **Census the COMPANIONS' INTERNALS, not only the undisplaced wrappers.**
- **A NEGATIVE census is worth what its SHAPE is worth**: "no hand-owned companion is exposed" carried because **72 of 96 conversions were MEASURED to take a native pointer**, where an empty grep would have closed nothing. **State what the population DOES, not only what it lacks**, and name the limits that make the closure honest.
- **A box cannot OBSERVE a write performed through a `ref` it has already handed out** — the converted `*(*uintptr)(unsafe.Pointer(&slot)) = addr` writes through `ж<T>.Value`'s ref over the same managed storage, so a golib-side hook at the reinterpret seam runs when the VIEW is taken, not when the value is STORED. **The remedy for a pointer-typed destination is a converter EMISSION change, and an increment whose halves are converter + golib owes BOTH gate families and lands together.** `NativeBox<array<T>>` over Go's headerless `*[N]T` reinterprets element bytes as a managed `T[]` HEADER — **it IS the prestub null read it was meant to fix**; a design refuting its own shorthand on paper is the argument for **writing the record first**.
<!-- A SECOND native-boundary class, measured 2026-09-03/04: LIFETIME, not layout. An unpinned array
     shown to MOVE first, then ARM 2 reading 0 and ARM 3 stable with the box held — "durable" is not
     "unconditional". 43 `(uintptr)Ꮡ(` sites are the idiom's count, not the hazard's. 77 syscall
     read/write sites, `KeepAlive` zero corpus-wide, an absence turned into a measurement by a
     GOROOT-side derivation of the shape.

     THREE CLR FUNCTION-POINTER FACTS, MEASURED (i7, 2026-09-08).

     THE REFERENCE-BEARING PIN MISS IS STRUCTURAL, NOT ARC-BLOCKED — and that is a STRONGER result
     than "blocked". The pointer-token arc's arrival does not change on its own that a `labelMap`
     carries references and can never pin. Measured and guarded at
     `PinnedBoxStalenessWitnessTests.cs`: 5/5 Debug, 5/5 Release+TC0, `Skipped: 0` load-bearing, and
     independently reproduced by a second lane (2026-09-07). A probe using `ж<long>` measures the
     CONTROL, not the case — the witness file names that shape as the control in its own words.

     A B/op census cannot SEE what a pin COSTS (2026-09-04). A remedy that changes WHERE storage is
     allocated is its OWN increment, never folded under the correctness cut whose evidence it would
     otherwise borrow.

     TRANSCRIBE the reference compiler's predicate rather than paraphrase the rule (2026-09-04, the
     root of that gap): the converter's KeepAlive analysis carried the sentence "never through an
     intermediate variable" — one wrong sentence in a comment was the whole gap, and the fix is the
     compiler's predicate in the converter.

     Two more members of that class, measured 2026-09-05. The MANAGED-CALLEE member: the landed
     `KeepAlive` fix's predicate was FUNNEL-shaped and never modelled a converted callee taking
     `unsafe.Pointer`. And a NAME-keyed predicate is per-platform BY CONSTRUCTION: that fix's funnel
     set was the uppercase `Syscall*` family while darwin funnels through lowercase libc trampolines,
     so it covered ZERO darwin sites while its census guard read green on every train. Route #8's
     shape, one target over.

     A REMEDIED member of a class can still be EXPOSED when the class's RULE changes underneath it
     (2026-09-05): what closed that question is the shape of the answer rather than its emptiness —
     72 of 96 conversions were MEASURED to take a native pointer, positive evidence that the remedy's
     shape had been applied consistently. Name the limits (here an unflagged struct and the
     caller-side variant) that make the closure honest.

     A box cannot OBSERVE a write performed through a `ref` it has already handed out (2026-09-05).

     A NATIVE CALLBACK'S ARGUMENT BRIDGE IS A REINTERPRET, NOT A CONVERSION (ruled 2026-09-08): in
     Go's own words. The ahead-of-time caveat survives only in the generic instantiation, bounded to
     test-host reach with the falsifier stated; and the decision was RAISED FROM THE BODY INTO THE
     RECORD before a line was written, which is the third time that order paid in one arc.

     A DOOR CAN BE RIGHT ON CLASS AND WRONG ON ITS OWN STATED REASON (2026-09-08, lane C2): every call
     shape was re-derived from the pinned GOROOT source, and the zero-based index 3 matching the
     door's own report is two derivations of ONE number. This is the CLAIM-LESS rule of the stubs
     section met at the native boundary — the class was right and the prose asserted a cause the
     evidence does not establish, so the fix is to delete the unwarranted half, not to compute more.

     A NAMED COMPLETENESS GAP CAN BE A DIFFERENT FACT ALREADY IN THE ARTIFACT (2026-09-08, i9): the
     hand-owned `LockOSThread` no-ops cost a SECOND test that the funnel's own name had filed under
     the callback question. Read from the PRESERVED RECORD rather than held for the dispatched solo
     run — a solo run is a different experiment and is still owed — it hands the other lane's root a
     falsifiable prediction on a test it was not derived from: the fix should move the sixth caller
     WITHOUT anyone touching the door. -->
## Pointer identity, box kinds and ordering tokens
- **A BOX'S POINTEE TYPE IS ITS TYPE ARGUMENT, NEVER ITS STORAGE.** A finalizer-registration check resolving the pointee from the STORAGE OBJECT named the CONTAINER in its refusal for a field-reference box whose dynamic type is the field's own pointer type, **refusing at iteration 0 a shape Go ACCEPTS**. **A predicate keyed on any other field carries a RED-FIRST arm per box kind** — field-reference and element-reference included — or it is guarded only on the standard box.
- **A RULE KEYED ON THE WRONG ATTRIBUTE IS A LOWER BOUND, NOT A RULE — and the ruled remedy is a THIRD ANSWER declared ABSTRACT on the base.** The pointer operator asked a PINNABILITY property ("can this be held still?") the question "is there an address here at all?"; those differ for every field or element reference rooted in a reference-bearing CONTAINER whose pointee is reference-FREE — the class that regressed every Windows dial. Testing the POINTEE's type restates the confusion one level down; **declaring the answer ABSTRACT makes every box kind STATE it or the assembly does not compile.**
- **A lane's BASE can lack a type the UNION contains**: a repair implementing the new member for all SIX box kinds its base carried failed at the first build with **CS0534** against a SEVENTH kind from another lane's increment — where a pointee-typed predicate would have COMPILED at the union and answered the wrong question.
- **A comment recording WHY a member is abstract, at the site, turns the next author's surprise into an instruction** — name the error code and what it prevented, paired with the argument from the other side (ordering forwards to the SOURCE's token, so identity is the source variable's and never the materialized copy's location).
- **A REPAIR CHANGES ONE THING and says which thing it is NOT changing** — a scope statement of that shape, made BEFORE the cut, keeps both changes reviewable.
- **Ordering managed pointers by a token derived from `RuntimeHelpers.GetHashCode` is unsound BY CONSTRUCTION**: identity hashes collide (26 bits on CoreCLR), so two distinct allocations can share a span and every address-ordering predicate over them (`alias.AnyOverlap`, `slices.overlaps`) can answer true for disjoint buffers — a birthday event visible only on a path minting millions of pairs.
- **A SATURATING INDEX FIELD IN A POINTER-ORDER TOKEN IS A CORRECTNESS BREAK, NOT A PRECISION LOSS**: pointer equality is token-based and the managed-pointer registry is KEYED by the token, so two distinct elements whose indices SATURATE compare EQUAL and collide in the registry, which then resolves the wrong box. **Ruled: the variant in which every index carries the tag is taken unless a hot-loop bench measures it worse, and any fallback that would replace it lands with its hole COUNTED, never as a refusal.**
- **A seat that moves the corpus onto the path Go takes is CORRECT even when that path has a latent defect the old path never reached** — the seat stays and the defect roots.
- **A crash a QUARTER of the way into a stream is a different signature from a two-row divergence at the end of a complete one**, so a standing "NOT MEASURABLE on this box" ruling covers the latter only.
<!-- A BOX'S POINTEE TYPE IS ITS TYPE ARGUMENT, NEVER ITS STORAGE (2026-09-08): a loud refusal at the
     wrong index, which is better than a five-minute silent hang and still wrong.

     A RULE KEYED ON THE WRONG ATTRIBUTE IS A LOWER BOUND, NOT A RULE (2026-09-06). The narrow
     remedy, testing the POINTEE's type, leaves a human obligation with no compiler behind it. Two
     things that bought, neither party having them in mind: the CS0534 seventh-kind build failure at
     the union, and the at-site comment that turns the eighth kind's author's wall into a signpost.
     The repair reopens the standing pin-unheld hole for the restored class, exactly as the pre-merge
     code did and no wider, and closing that hole is its own arc with its own population and guard.

     Ordering by `GetHashCode` unsound (2026-09-03). Saturating index field ruled 2026-09-08 — found
     by WRITING the candidate out as a DESIGN rather than as a sentence, which turned "the narrow
     split is arguable" into "its only sound form holes the door where the biggest arrays are". -->
## Mute deaths, dumps and signal dispositions
- **A shared syscall dispatcher that mis-calls the WRITE primitive MUTES the entire platform's runtime error output** — `runtime`'s `throw`/`print` reach `writeErr` → `write1`, so every death is **exit-138/SIGBUS with a blank stderr**. **For a stderr instrument, a blank-stderr death on such a platform is the mis-call SIGNAL — a symptom to attribute, never an instrument gap to chase — so the mute baseline is recorded BEFORE the primitive is seamed.**
- **A STACK BEFORE A DESIGN**, and **an instrument's COST is quoted with its result** so the next reader can afford to measure before designing.
- **READ THE DISPATCHER'S SOURCE before naming the address a native write went through**: 35 of darwin's 50 `libcCall` sites box the FIRST parameter ALONE where Go hands a contiguous `cgo_unsafe_args` block — "through the interior address of an unpinned box" was the fitting story and the box was never handed to libc at all.
- **A CORRUPTION THAT DOES NOT CRASH ON ONE PLATFORM IS THE SAME CORRUPTION**: x64 and arm64 perform the identical 16-byte native write through a **STALE REGISTER** (the libc dispatcher places the first parameter's one field and leaves the trampoline's second and third registers as the caller-saved state left them), and only the leg whose stack walker reads those bytes dies — **the MUTE leg was the honest one.**
- **A death inside a panic's own REPORT (a stack-trace capture) is scored at the door the report was ABOUT**, and **a mute death's exit code says nothing until the crash report's frames and registers place it** (pc/far/esr — a data read of a value that is nothing's address is a fingerprint to decode).
- **A stdout LINE COUNT places a mute death without a byte of stderr** — a guard printing six lines printed exactly TWO on both mac legs, so both died inside the third statement. **A capture stage's NULL is evidence only beside a POSITIVE CONTROL in the same capture**, and **a ranked hypothesis the run does not support is WITHDRAWN**, not kept alive as "still possible".
- **A DUMP, NOT A STACK AND NOT A STORY, NAMES THE WALL**: a blank-stderr **exit 139** from a managed host can be the CLR's own PRESTUB dispatching a generic method on a CORRUPTED reference — a raw native address landed in a managed slot by an unsafe store — and **the SINGLE-FILE host writes no dump and no stderr at all.** The framework-dependent host under `DOTNET_DbgEnableMiniDump` plus `dotnet-dump … clrstack -all` (and `gdb` for the faulting instruction) reads it; **nothing cheaper does.**
- **A SIGNAL DISPOSITION IS A KERNEL FACT, and a bridge modelling it in managed code satisfies neither of its consumers.** Go's `signal.Ignore` is a kernel `SIG_IGN` — the KERNEL consults it (`tty_check_change` lets a background-pgrp process `tcsetpgrp` only if SIGTTOU is ignored or blocked) and it is **INHERITED ACROSS EXEC** — so modelling Ignore as swallow-in-handler leaves `SIG_DFL`, and a host in a background process group under a tty **STOPS FOREVER** on SIGTTOU: the mute class in a T-state costume, no handler, no deadline, no results file. **The TERMINAL-FREE instrument is an exec'd child PRINTING each disposition** (Go reads 1 on every line): no pty, cannot stop, and it separates the two models where a canary under `script` can only hang. **Read at the code first, then measure, then cut.**
- **The CLR-free signal class is PER FLAVOUR**: darwin's differs from linux's by **SIGUSR1**, coreclr's activation-injection signal where no `SIGRTMIN` exists (`pal` `signal.cpp`), so a kernel `SIG_IGN` there discards GC-suspension activations. **Read the RUNTIME's SOURCE for such a boundary, never memory, and state the class per flavour in the bridge's own header.**
- **A CORRECT change exposing a latent gap is a FINDING, never a regression.**
<!-- Mute platform measured 2026-09-03 on darwin: the keystone walked the pointee of an ABI0 `&fd` —
     which names the first of three contiguous stack args — and passed fd + junk, breaking
     `read`/`write1`; `runtime`'s `throw`/`print` reach `writeErr` → `write1` on the same shape, so
     every death on that platform is exit-138/SIGBUS with a blank stderr and nothing can reach it.
     Its neighbour, and the reason the instrument came first: a STACK before a design. A full-stderr
     instrument read two deaths whose predicted sites — both with falsifiers named — were FALSIFIED
     on site in twenty minutes of runner time, retiring two designs.

     READ THE DISPATCHER'S SOURCE (2026-09-05): the REMEDY did not move; the MECHANISM sentence did,
     and a "slot" instrument that would have cost a day was answered by construction.

     A stdout LINE COUNT places a mute death (2026-09-04): the leg that did produce a stack named the
     same frame, two independent derivations agreeing; the positive control was the leg that printed
     through that same stage; an alignment story explained one leg's death MODE and nothing about
     WHERE, which the line count settled.

     A CORRUPTION THAT DOES NOT CRASH ON ONE PLATFORM IS THE SAME CORRUPTION (2026-09-05): Go's
     `cgo_unsafe_args` block is unpacked by offset. The register fingerprint here was `sigaction`'s
     mask/flags word. A DUMP, NOT A STACK AND NOT A STORY, NAMES THE WALL (2026-09-05).

     A SIGNAL DISPOSITION IS A KERNEL FACT (2026-09-05, met on linux and darwin in one week).
     Attributed by four arms varying ONE axis each, the last two differing only in the kernel
     disposition. The roster is untouched because no fleet sweep has a controlling terminal. -->
## Hand-owned types and companion files
- **A cross-package COPY of a hand-owned type with lazily-shared state is correct BY ACCIDENT after the owner's first use and fatal before it.** The converter's box wrap over a package-qualified selector boxes a COPY of an exported package var, and a hand-owned `RWMutex`'s state is created on first use and shared — so a copy taken **after** the owner touches the var lands on the real lock while one taken **before** gets its own. **A guard for such a fix must hold the sub-library var UNTOUCHED before the cross-package pair, or it is green by the mask.** The SAME-PACKAGE control roots it: the converter emits the right form (the exported box) for a local var and loses it on the qualified selector, so the defect is in the **selector path**, not the receiver machinery. **Measure the mechanism on a scratch module before cutting the guard** — the corollary to "a stack before a design".
- **A companion may deliberately COMPENSATE for a caller belief that is factually FALSE, and it must say so IN ITS OWN WORDS or the compensation is invisible** — a caller-side reading cannot see it otherwise.
- **A handoff keyed on the CURRENT GOROUTINE turns a cross-package sequencing property into a load-bearing invariant that no type, signature or gate expresses.** Both violation modes throw by name, so it fails loudly; but **the remedy that REMOVES the invariant keys on something structural** (the descriptor, or the overlapped that already identifies the operation at submit and harvest) — **a guard only documents it.**
<!-- Cross-package copy measured 2026-09-03. That is why no linux/windows row ever saw the class and
     why one darwin arrangement fatals.

     Two companion-file rules from the same seam (2026-09-06). The compensation examples: a DNS
     list-free is a NO-OP because the caller keeps Go's deferred free where Go put it while the
     native chain was already freed and the caller holds a managed transcription; and the accept path
     IGNORES the caller's buffer because under our representation that buffer is unusable both as
     data and as a table key. The goroutine handoff: the accept staging is parked in a weak table
     keyed on the goroutine and consumed by an entry point whose signature carries no handle, no
     overlapped and no identity at all, with the premise — one goroutine, no interleaved accept —
     stated only in a comment. -->
