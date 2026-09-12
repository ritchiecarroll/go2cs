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
     context, so provenance kept this way costs ZERO tokens. -->

⚠ **NOTHING routinely builds `go2cs.slnx` end to end, so a broken solution member rots invisibly.**
Every harness — `BehavioralRunner`, MSTest, `check-no-regression.ps1`, `run-validated-sweep.ps1` —
builds each `.csproj` **by path**, never through the solution (the same by-path habit
`check-solution-integrity.ps1` polices from the other direction). A solution member that no gate
compiles can therefore break while every gate stays green. That is what happened to the
`utilities/QuickTest` scratch project: the r41 GoFrame arc (`6adab2909`, 2026-08-05) retired golib's
`func`/`Defer`/`Recover` execution-context API and carried the corpus with it, but QuickTest was
hand-written scratch nobody builds, so it took `go2cs.slnx` down for two days unnoticed. It was
**retired** on 2026-08-07 rather than hand-fixed again: it was the solution's only hand-written,
un-gated member, so it would have rotted at the next golib change, and the experiments it held
(struct/interface promotion hand-simulated *before* `go2cs-gen` existed) are covered by real
behavioral tests now. Git keeps it at `d3223d252` if a shape is ever wanted back. **After changing a
golib/runtime API, build `src/go2cs.slnx` once before banking** — ~90 s, and no other gate covers it.
⚠ Two golib cost rules, both measured 2026-09-01: **a golib change adding INSTANCE state to `ж<T>`
(or any per-box base class) is a corpus-wide byte-cost change** — +8 B lands on EVERY pointer box,
proportional to boxes allocated per path (measured 14/1/0 boxes across three alloc rows), so the
commit states the cost even when correctness demands the field (the element-aliasing publish gate
did; its unfavorable direction shipped unmeasured and later burned an attribution run). And **an
alloc row's B/op is only comparable against a figure taken at the same suite scope** — filtered vs
unfiltered differed by +167.04 B/op on ONE tree (AllocsPerRun's single warmup doesn't cover
one-time costs a full run has already paid), so a filtered census never compares its bytes against
a full-run record: the alloc-instrument sibling of the gated-census stream rule.
⚠ **THE ALLOC INSTRUMENT'S OWN CONVERGENCE — six rules from one arc (2026-09-04), because a byte
endpoint quoted off an unconverged instrument is a false measurement.** **A prediction's BASELINE is
measured at the same scope in the SAME RUN, never quoted from a record** — a 1,457.8 B/run baseline
taken out of a design record read 1,510.8 under the acceptance's own filter (the comparability rule
above, met on the prediction side) — and a reduction LARGER than predicted does not make the
prediction less wrong: both corrections go in the commit message, not only in the post. **A reduction
claim quotes the deterministic FLOOR** (the minimum over reps) **or a high-runs figure, and names its
UNIT and its CONFIGURATION**: the measured os row is 1,320.00 B/run in every configuration except
Release+tiered (1,256, tier-1 escape analysis stack-allocating one non-escaping box), and a 100-run
`AllocsPerRun` sample carries a fixed 0–800 B/run per-window accounting term, so it cannot resolve a
change under ~150 B/run. golib's object COUNT is charged at the `new` while the BYTE cost diverges
under a tiered JIT — a box the JIT stack-allocates still counts 1.00 — so no JIT improvement banks a
count-conditioned row; only not constructing the boxes does. **A COUNT-based byte prediction is a
LOWER bound whenever the cut also UN-ESCAPES surviving boxes**: six deleted boxes at 64 B predicted
384 and the converged floors read 512 (1,320.00 → 808.00), the two surviving receiver boxes having
stopped escaping. **A minimum over 40 draws of a 100-run window is unconverged by a near-constant
offset (+42.5/+44.4 here) that CANCELS in a DIFFERENCE of two such minima** — so a difference of
unconverged minima may be right while both absolutes are wrong; record absolutes from the converged
instrument, and when the count is exact and the bytes do not close, SEGMENT (a per-frame byte probe
with literal tags, an exact segment sum and a one-row positive control) before naming a mechanism.
**An instrument that reads HIGHER on strictly FEWER objects is telling you it is NOT CONVERGED** — a
40-rep minimum read 789.8 after a cut removed two boxes from a tree reading 785.0, impossible for a
true floor — and that reading is REPORTED as non-convergence, never smoothed or explained. Finally,
**a static census of "boxes" is bounded by the INSTRUMENT's population**: golib's counter counts
golib allocation sites only, so a defer's delegate, a params array and an interface box are not among
the counted eight, and a capability can have TWO populations of different sizes (239 sites minting a
box AND a delegate, 207 a delegate alone) of which only the first is visible — census BOTH, segment
the row with the converged instrument BEFORE sizing an increment against it, and size a lowering by a
`go/ast` census with each exclusion counted separately, because the exclusions are the residue the
null owns. A count is the unit that carried information at every step of that arc; the bytes needed
the instrument.
⚠ **Four more from the same ladder's next increment (2026-09-04), each a way to quote a wrong
endpoint.** **A cut that removes boxes an EARLIER cut already UN-ESCAPED saves ZERO bytes** — deleting
two receiver boxes read 744.25 → 744.25 B/run with the count 10 → 8, because an earlier increment had
already priced those two at 0.00 B: 64 B is a property of a box that ESCAPES, the converse of the
count-based LOWER-bound rule above, so the unit is re-read from the CURRENT tree's segment table
before every prediction and never carried down the ladder (that prediction was exact on the count and
wrong by the whole 128 B). **A reading is comparable only under the SAME WINDOW PROTOCOL as the record
it is read against**: the converged 744.25 B / 8 obj is the FLOOR of windows 2 and 3 of three
1,000,000-run windows in one process, while a 100-run single window read 898 / 916 / 895 — the count
exact, the bytes ~150 B high and varying, because a short window cannot dilute the amortised slack —
so stop-and-reconcile before reading anything against the record (a GC-configuration hypothesis, gen0
0 against 99, was tested with ONE variable and dropped when it moved nothing but the noise). Three
ladder mechanics beside them: window 1 of a 1,000,000-run `AllocsPerRun` reading always sits ~1.3
B/run above the integer floor while windows 2–3 land on it, so **TWO windows is the minimum
protocol**; `-p:BaseOutputPath` and `-p:BaseIntermediateOutputPath` on the command line are GLOBAL
properties that propagate into every referenced corpus project and mint parallel `obj-*` trees under
`src/core` which then collide on the SDK's default compile glob (CS0579/CS1537), so an
outside-the-repo probe project sets `EnableDefaultCompileItems=false` with one explicit `Compile`
item; and a segment an instrument could not enter is reported COMBINED with its split marked DERIVED,
cross-checked against an identical construction measured directly in the same run, so nobody quotes a
derivation as a reading. ⚠ And **a LAYOUT read (`Unsafe.SizeOf<T>`) is decided by the field set at JIT
time and cannot move with load**, so the loaded-versus-solo rule does not apply to it — but a size row
is an ASSERTION only after a control grows the struct by one word and reads RED; before that it is a
baseline that passes either way.
⚠ **A REPRESENTATION choice is measured on the AXIS THAT SEPARATES ITS CANDIDATES** (2026-09-05):
three arms of a box-slot design read their predicted bytes to the byte on the alloc row — which
separates only the slot's +8 B/box — and were told apart ONLY by a synthetic 10M-call hot loop
(18.0 / 20.2 / 37.0 ns against a 33.8 baseline), because a syscall-dominated row cannot resolve a
lookup at all (the TLS handshake sat in a 4% spread in no consistent order). **Name the instrument
that CAN see the difference before the arms are built**, and state a corpus-wide cost as a PER-ROW
FORMULA (+8 B × consumer-type boxes per op), never as a single verdict.
⚠ **Three more from the same ladder's tail (2026-09-05).** **An alloc census table carries ns/op
BESIDE obj/op and B/op** — it is the only column that separates a real zero from an ELIDED call
(a 9 ns control against a 2,516 ns `DeepEqual(slice,slice)`) — and, per the instrument-population rule
above, **an allocation counter charging golib's own sites is structurally BLIND to CLR boxing**, so a
"floor of N boxes" stays a PROOF CLAIM until a byte-arithmetic instrument against a hand-boxed control
measures it; two instruments in two PROCESSES agreeing to the object (probe 3 obj against suite 3 obj)
is what makes a census the reading of record. **Two INDEPENDENT derivations agreeing to the byte AND
the object LOCATE a residue without a third instrument** — a per-frame byte probe taken at an earlier
base and a later cut's own measured total closing at 120+88+120+56+104 = 488 B and 2+2+2 = 6 objects —
so the re-probe is OFFERED, not owed. And **a count-based bank condition (objects = 0) is served ONLY
by the candidates that remove OBJECTS**: a bytes-only residue (pins) is named as its own increment and
never stands between the row and its condition.
⚠ **A SATURATED COLUMN CARRIES NO CROSS-HOST INFORMATION** (2026-09-08). The CLR's identity hash was
measured 26 bits wide on one flavour — the top six bits never set over a million simultaneously-live
objects, uniformly distributed, collisions matching the birthday expectation — so a packing that
drops the top two bits costs EXACTLY nothing, while a 16-bit packing collides in nearly every draw.
But at that population a 16-bit column's collisions are FORCED to the population minus its bucket
count, and an 8-bit control's likewise, **so their exact agreement across two hosts is ARITHMETIC and
not corroboration**: "all four counts identical" would have read as strong support while two of the
four were never in a position to dissent, and the only FREE column DID differ. Three mechanics ride
with it: the prediction was written into the probe's own source header before it ran and scored as
worded; objects are held LIVE for the whole run, because a collected object's reused hash reads as a
collision; and the hash SEQUENCE is deterministic per host across processes, so such a number is a
per-host CONSTANT rather than a sample — a guard built on it is stable and samples nothing, and a
residual is measured against the EXACT birthday expectation, never its square-law approximation.

- **⚠ An UNBANKED package's `-tests` assembly is in NO standing gate — route #7's shape, one
  assembly over (found 2026-09-01 by a lane's own sweep, by no gate).** CNR is transpile-only and
  the stdlib solution compiles PRODUCTION assemblies, so nothing at master ever builds the test
  emission of a package that has not banked: `reflect`'s `-tests` assembly sat compile-broken at
  master after a widened lift dedup bound a PUBLIC lifted struct's member shape to an INTERNAL
  prior lift — the dedup crossing ACCESSIBILITY tiers, CS0050/51/52 — with every standing gate
  green. **Standing amendment: any converter change touching lift identity, dedup registries or
  anonymous-type naming owes a `-tests` CONVERT-then-BUILD of `reflect` at the MERGE RESULT, beside
  CNR.** ⚠ **CONVERT then BUILD — a bare `-test-action build` measures NOTHING on a box that has not
  converted the row** (measured 2026-09-04): `build` consumes an EXISTING digest-validated manifest,
  and `go2cs_test_manifest.json` is machine-specific and git-ignored, so on a clean box the run exits
  in **0 s** with `test manifest is missing` — which an error-pattern filter reads as a bare `exit=1`
  and a green-word grep reads as nothing at all. The gate is `-test-action convert` (or `all`)
  followed by the build, on every box that has not already emitted that row. The same hole has a
  second door: **the production-only two-seeded diff is blind to
  TEST-side emission** — a carrier stamp that dangled two banked rows lived in `x509_test.cs`,
  which `-stdlib` never writes, so the diff matched its prediction exactly and said nothing about
  the footprint that broke them. A converter change that emits CROSS-PACKAGE references therefore
  (a) lands its corpus footprint in the SAME train — the two-seeded diff applied verbatim,
  byte-identity asserted, exactly as a hand-own registration lands with its body — and (b) owes a
  `-tests` emission census of the banked rows it can reach, beside the `-stdlib` diff.
  ⚠ **"NOTHING REACHES IT" IS NOT A REASON TO IGNORE A LATENT DEFECT — IT IS THE PRECISE CONDITION THAT
  MAKES IT A BOOBY TRAP.** A 28-member cluster of throwing destinations was shown unreached by its
  package's own suite, which deflated the claim that connecting them would move the row; it
  **STRENGTHENED** the original hazard, because unreached is exactly why no gate sees them and exactly
  why the first cut that finally reaches one gets billed for a wall it did not build. **A KNOWN-RED ROW
  THAT NO STANDING GATE REDDENS IS THE WORST KIND OF RED, NOT A CHEAP ONE** — verifying that a schedule
  cannot see the red is the argument AGAINST boarding it, not for it, and **a fact that points somewhere
  is not an argument for going there**: supply it accurately, label what it is, and let the conclusion be
  argued.
  ⚠ **A LATENT-MISATTRIBUTION HAZARD AND AN OBJECTIVE ACCELERATOR ARE DIFFERENT CLAIMS NEEDING DIFFERENT
  EVIDENCE, and they are easy to conflate when the population is the same.** The evidence for "this will
  bill the wrong commit someday" is that the class exists and no gate sees it; the evidence for
  "connecting these advances the objective" is that something currently REACHES them — which turned out
  to be false for the largest family. **Promoting the first claim into the second is a silent upgrade;
  state which claim a population is being offered for.**
  ⚠ **A GENERATED ARTIFACT'S BOILERPLATE DIAGNOSTIC IS NOT A FINDING ABOUT THE SPECIFIC SYMBOL — IT
  DESCRIBES THE SHAPE THE GENERATOR FOUND.** All seven pprof stubs say "external (assembly or cgo)
  function is not implemented", **including two with demonstrable bodies at known lines**; three
  participants read that text as a statement about the function and produced three separate
  misclassifications from it. Frontier-vs-wiring is decided by whether a body and a push exist upstream —
  two greps — never by the stub's own words. **Generalizes: any message a tool emits from a template is
  evidence about the TEMPLATE'S TRIGGER CONDITION, not about the instance.** The remedy is not "read
  stubs more carefully": it is to make the stub say what it knows ("a linkname names this symbol and the
  body was not linked"), because **a tool that has the information and emits boilerplate instead is
  spending its users' attention to save its own.**
  ⚠ **AMENDS THE ABOVE ON THE REMEDY: THE FIX FOR A MISLEADING DIAGNOSTIC IS USUALLY TO CLAIM LESS, NOT
  TO COMPUTE MORE.** The proposal was to teach the generator to distinguish wiring from frontier, which
  required a converter-emitted marker for it to read. **A source generator sees ONE compilation and
  structurally cannot answer "is this implemented somewhere else"; what it knows precisely is "nothing
  in THIS compilation implements it", and its own attribute doc already states that equivalence and
  defends it against the two proxies that fail** (2026-09-07). So the defect was never missing
  capability: **the PROSE asserted a CAUSE the equivalence does not establish.** Delete the unwarranted
  half and the message becomes true, the marker machinery is unnecessary, and the fix shrinks. **Before
  building an oracle a tool cannot be, check whether it is merely SAYING more than it knows.**
  ⚠ **And a diagnostic that reads its evidence from a COMMENT inherits that comment's conditionality**:
  the linkname sits in leading trivia and survives only under `-comments`, so a generator keyed on it
  would go quietly generic on any emission without them. **Where a signal must be durable it belongs in
  an attribute the converter emits unconditionally** — the comment is the cheap read, not the reliable
  one.
  ⚠ **A SUPPRESSED LINKNAME PUSH LEAVES A THROWING DESTINATION THAT NO GATE FAMILY CAN SEE UNTIL
  SOMETHING CALLS IT** — CNR is transpile-only, the census compiles without running, and the behavioral
  suite reaches only what a program actually calls, so the corpus compiles, every gate is green, and the
  row that finally arrives looks like a regression in whatever cut landed last. **The cost is not the
  wall, it is that the wall BILLS THE WRONG COMMIT.** The class **splits three ways and only one is a
  defect**: (1) displaced by a registry entry or an `_impl.cs` — not a member, the body exists; (2) the
  generator fills it and **Go has no implementation either** (asm, cgo) — an honest refuse-by-name,
  documented and correct; (3) the generator fills it, **Go HAS an implementation, and a push exists that
  did not arrive** — lost functionality wearing the costume of a deliberate refusal, indistinguishable
  from bucket 2 from outside. **A census returns bucket 3 BY NAME; an undifferentiated count tells
  nobody whether to worry**, and the sound oracle is each built package's generated stub file, not a
  text predicate over declarations — the text bounds the CONTAINER, not the population.
  ⚠ **A STUB THAT THROWS IS DIAGNOSABLE WHERE A BODY RETURNING ZERO IS NOT** (2026-09-08): the
  runtime's fatal path reaches the caller-PC and caller-SP intrinsics, whose PC is LOAD-BEARING in Go
  (the traceback's starting frame, consumed unconditionally on a runtime throw) and DEAD here (an
  always-empty program-counter table) — so a `return 0` body would dereference address zero inside the
  traceback's own initialisation and turn a NAMED refusal into a wild read; the tree already refused
  it in writing, under "not implemented here, on purpose". The remedy is the precedent one function
  over: SEVER the consumers onto the managed walk by registry displacement, plus a write primitive on
  the flavour where every runtime throw is otherwise MUTE. **And the cheapest falsifier runs before
  any line is written**: a user program reaching a converted runtime fatal, its stdout, stderr and
  exit code read per flavour.
  ⚠ **"Recovers catchability, not capability" — keep those two apart in any option list.** A loud
  refusal a lane can find and attribute is worth a great deal and **is not the same as the thing
  working**. This class existed for three weeks precisely because a board entry said *"no managed body"*
  (capability) where the truth was *"body exists, unreachable across the assembly boundary"* (wiring) —
  two readings that send a lane to completely different work. **"Wired on paper and still throwing" is a
  third state**: `readProfile` carries both a body and its linkname directive and still throws, because a
  directive existing is not the push ARRIVING, so a remedy sized for the missing-directive case does not
  touch it. **Name the state, not the symptom — all three present as the same throw.**
  ⚠ **AN EXIT CODE CANNOT DISCRIMINATE A WORKING RUNTIME FATAL PATH FROM A DEAD ONE ON WINDOWS**
  (2026-09-08): golib's unhandled-exception backstop writes the exception text and exits 2 for ANY
  unhandled managed exception, so "the exit becomes 2" is satisfied VACUOUSLY today by the
  not-implemented throw at the fatal path's first statement — the increment's acceptance therefore
  keys on the stderr SHAPE (Go's own goroutine header and frame order) and on the exception's
  ABSENCE. The probe's structural half held exactly (the text once, death at the caller-PC intrinsic),
  and **the falsifier that would have retired the increment — exit 2 WITH a Go-shaped traceback — did
  not fire.**
  ⚠ **A MAP KEYED BY A DIRECTIVE'S DESTINATION IS STRUCTURALLY BLIND TO THE OPPOSITE WIRING DIRECTION.**
  A PUSH is the producer naming its consumer; a PULL is the consumer naming its producer. Measured
  2026-09-06: **45 push / 53 pull / 0 both, fully disjoint** — so "wired" was 98 where a
  destination-keyed census said 45. **The blind spot is a property of the KEY, not an oversight, and no
  amount of re-running finds it: when a census counts RELATIONSHIPS, enumerate the DIRECTIONS the
  relationship can take before trusting the key.**
  ⚠ **`go run` REPORTS A DIFFERENT EXIT CODE FROM THE BUILT BINARY ON A RUNTIME FATAL** (1 against 2,
  2026-09-08), so **a probe whose PRIMARY READING is the exit code BUILDS and runs the binary**,
  capturing the code as the FIRST statement after it. Two companions from the same probe: **a probe's
  own markers go through the formatting package, never the builtin print**, whose lowering enters the
  very runtime print path the probe predicted dead; and **a proposed TRIGGER is MEASURED before it is
  used** — an unlock-of-unlocked-mutex fatal never enters the runtime's fatal path at all, because the
  hand-owned mutex declares its own local hook.
  ⚠ **A REGISTRY CHECK CANNOT SEE A DEPARTURE THAT DOES NOT GO THROUGH THE REGISTRY — enumerate the
  ways a member can LEAVE a population before trusting any census of it.** A whole-file
  `GoManualConversion` replacement and a bodyless-partial completion each remove a member from the
  unimplemented-stub population **without touching the push registry**, so a registry-keyed census
  could not observe those departures even in principle, however carefully it ran (2026-09-07) — the
  destination-keyed map's blind spot one layer over. ⚠ The settling instrument is the GENERATOR'S OWN
  OUTPUT, which observes the population directly rather than one route into it: **a structural
  after-state is a second, independent way to measure a seat, and it reads off ARTIFACTS rather than
  verdicts** — `runtime/pprof`'s generated stub directory held 7 files before a landing and 1 after,
  six bodyless partials having gained real implementing parts, with no comparison record and no
  confound about which of 53 commits did it.
  ⚠ **A PACKAGE-DELTA CENSUS KEYED REMOVED→SUCCESSOR IS BLIND TO A PACKAGE THAT GAINS FILES FROM ONE
  THAT IS NOT REMOVED** (2026-09-08): at 1.24.13 `sync/mutex.go` shrinks 261 → 66 lines — a delegating
  wrapper — and its 234-line spin-and-park implementation moves to the NEW `internal/sync`, the exact
  mechanism `sync/mutex.cs` is hand-owned for; the committed census row `internal/concurrent →
  internal/sync | 3/3` is TRUE while saying nothing about it, because `sync` is not a REMOVED package.
  The destination-keyed blind spot in a new costume: **when a census counts RELATIONSHIPS, enumerate
  the directions before trusting the key, and a hand-own census at a hop compares each hand-own's
  PRINCIPAL line count across BOTH releases** (ten `sync` hand-owns, one moved principal).
  ⚠ **The instrument that DOES walk every banked row's TEST emission is a roster-wide `-tests`
  reconversion, and it found what nothing else could (2026-09-02):** a BANKED row (`errors`, 61
  verdicts) whose test assembly no longer built at master — a production-registry dedup arm whose
  accessibility guard reasons within ONE assembly (`v.inFunction` short-circuits) bound the EXTERNAL
  test variant's function-local lift to the production assembly's INTERNAL lift (CS0122), bisected to
  `5442b402e`, whose own blast-radius census was a `-stdlib` two-seed diff and therefore structurally
  blind to test emission. Rules: a cross-assembly reuse is admissible only if REACHABLE — an internal
  variant, a PUBLIC candidate, **or an EXTERNAL variant whose test-project MODEL puts production's
  internals in sight** (whitebox-reference or recompile; only the plain reference model, chosen exactly
  when the package has NO internal test file and therefore gets no `InternalsVisibleTo` grant, cannot
  see them). ⚠ **The axis is the test ASSEMBLY, not the Go VARIANT — corrected 2026-09-04 by
  measurement, after this line's first form said "an internal variant, or a PUBLIC candidate" and the
  converter's predicate agreed with it.** `f38c2ae01` keyed reachability on `testExternalVariant`,
  which is right for `errors` (external-only suite, no grant) and wrong for every package that HAS an
  internal test file: there BOTH variants emit into the ONE `.tests` project, and the same fact that
  selects the whitebox model (`selectTestProjectModel`) is what makes the production csproj emit
  `InternalsVisibleTo $(AssemblyName).tests` (`insertFriendAssemblyAccess`) — so the grant is a
  CONSEQUENCE of the model and decidable without reading the csproj back. runtime paid it on EVERY
  target: `hash_test.go`'s `IfaceKey.i interface{ F() }` calls production `ifaceHash` through the
  `export_test.go` bridge, so the refusal minted a second `IfaceKey_i` and the call could not bind
  (`hash_test.cs(540,52) CS1503`), byte-identically on windows and linux. The two rules had been
  written by two arcs: `liftNameNeedsPublicType`, directly above the refusal, documents that same
  reuse as one hash_test.go "needs to compile at all". A documented invariant that a later seeding
  violates is a bug the doc cannot catch; **a lift/dedup change owes a `-tests` convert-then-build
  of a row with an EXTERNAL test variant — `errors`, the cheapest — beside `reflect`**; and a delta
  table carries build failures as their own REGRESSION column, distinct from movers. (A bisect
  converging on adjacent commits with BOTH controls valid is an attribution; the named suspects were
  exonerated by measurement, not by argument. And a bisect is not always OWED: where the green-to-red
  window holds exactly ONE commit touching the predicate, an instrumented reading names the SEAM as
  well as the commit for the price of one convert — 2026-09-04, the CS1503 above.)
  ⚠ **The `-tests` driver MIRRORS `processConversion`'s analysis sequence BY HAND** (measured
  2026-09-03), so a new converter pass wired into the `-stdlib` driver binds NOTHING under `-tests`
  until it is wired there too — self-caught by a darwin conversion emitting 0 records where 28 were
  predicted. Two hand-mirrored sequences drift: wire a new pass into ONE sequence both drivers call,
  or make its guard run under BOTH drivers.
  ⚠ **One assembly FURTHER over: a BANKED row whose TEST side carries a hand-owned `*_impl_test.cs`
  companion is compiled by NO standing gate either** (2026-09-05, `internal/reflectlite`). CNR is
  transpile-only, the stdlib solution compiles PRODUCTION assemblies, and the union sweep's roster is a
  fixed list — so an abi/golib API retirement broke that row at master for TWO trains with every gate
  green, found only by a lane's own banked-row sweep. **The class is named by the FILE SUFFIX** (one
  `git ls-tree`, re-derived per train — 2 packages at `9c44a6d6a`) **and the guard is a BUILD of each
  such row's test host at assembly**, never a grep over bridge internals, which rots with renames and
  cannot see a type-level break.
- **⚠ FALSE-GREEN route #7's BEHAVIORAL twin — a golib/reflect/gen change that alters RUNTIME
  behavior while emitting byte-identical `.cs` is invisible to CNR *and* to the `reflect` `-tests`
  build (found 2026-09-03; it shipped through TWO trains).** CNR is transpile-only and byte-identical
  by construction here; the `-tests` gate is compile-only; and a FILTERED behavioral runner never
  exercises the affected row — so nothing sees it. `ReflectArrayOf`'s identity assertion
  (`reflect.SliceOf(reflect.ArrayOf(…))` == the declared slice-of-array type) went red at the
  descriptor-cargo increment and was caught only when a darwin census flagged it as an outside-model
  movement and the coordinator reproduced it on windows. **Rule: the train battery runs the FULL
  behavioral suite — all phases, Output included — when a seat touches `golib`/`reflect`/`src/gen`;
  it is the only leg that sees a behavioral (non-compile, non-emission) regression.** Corollary: a
  full-suite PASS NUMBER quoted in a seat message is trustworthy only if the run ENUMERATED the
  affected row and was not stale-green (route #2), so a seat claiming NNN/0 Output over a tree with a
  known-red Output-compared row is reconciled before the number is believed.
  ⚠ **AN INTERFACE MEMBER ADDED TO THE HAND-OWNED TESTING HOST BREAKS EVERY CROSS-ASSEMBLY ADAPTER, AND
  THE HOST COMPILING GREEN IS EXACTLY WHY IT IS DANGEROUS** — route #7 in the testing host's clothes
  (2026-09-07). `TB.Context()` at Go 1.24 breaks **all 57** assemblies carrying
  `[assembly: GoImplement<…, testing_package.TB>]`: go2cs-gen mints each adapter's forwarders from the
  interface's member set AT COMPILE TIME, so the forwarder names `go.context_package.Context` in a
  consuming assembly that does not reference `context` (CS0234 / CS0012 / CS9334). **It cannot fix
  itself, and the reason is deliberate**: the emitted csprojs set
  `DisableTransitiveProjectReferences=true`, so a project's reference set is exactly its Go imports —
  and Go's own test files do not import `context` merely to call `t.Context()`, so the converter can
  never derive it. The reference must be INJECTED the way `testing` already is; **the existing
  injection is the specification.** Only a cross-assembly CONSUMER compile catches it — `archive/zip`,
  which adapts both `B→TB` and `T→TB`, is the natural canary.
  ⚠ **Its narrowest member: a banked SINGLE-VERDICT row whose mechanism lives in `golib` is guarded
  NOWHERE but that row** (2026-09-04), and where the load-bearing half is a WIRING line in a hand-own
  (a `runtime.GC()` call into a cache clear), its DELETION stays green on every standing gate. Such a
  mechanism takes a **GolibTests guard whose WIRING arm is the one that pays for the file** — the arm
  that fails when the line is removed, not the arm that restates what the library does.
  ⚠ **And a behavioral project's PASS is PER PHASE** (2026-09-04): a project without
  `[GoTestMatchingConsoleOutput]` passes Target ONLY — its emitted text byte-compared, its printed
  output never diffed against `go run` — so a golib RENDERER change guarded by a Target-only project
  has no guard at all: a battery reported `ChanElemDims PASS` with the value row genuinely RED
  (`chan [3]int` rendering as `chan []int`). **A post quoting a guard's PASS says which PHASE compared
  what**; the reader's own pre-read carried the tell (`Output: 0 compared`) and read it as vacuity
  rather than as an unmeasured red. And **a guard's header is a CLAIM that ages** — this one said an
  increment would close the row while that increment landed slices-only: correct the claim at the
  site, name the owning increment, and never turn an arm on for the strength of a comment. ⚠ One
  guard-author mechanic from the same family: in a file carrying `using static go.runtime_package` the
  bare name `GC` binds Go's `runtime.GC()` and SHADOWS `System.GC` (CS0119) — qualify it.
  ⚠ **And the tell has a CAUSE worth knowing: an Output line reading `0 compared, 0 failed, skip 1`
  is NOT a pass** (2026-09-05) — a FRESHLY transpiled `package_info.cs` lacks the hand-added
  `[GoTestMatchingConsoleOutput]` (the converter preserves it by reading the EXISTING file at the
  output path, so there is nothing to preserve where the file was regenerated from scratch), the
  Go-vs-C# comparison never runs, and Transpile/Compile still read true: route #6 in a costume. **Read
  the comparison COUNT, not the banner.**
  ⚠ **A design RULING is not reversed inside a FIX commit** (2026-09-05, the reinterpret remedy): a
  fix that made a reference-bearing box's reinterpret a GC-safe VIEW over the slot contradicted the
  seated design's loud-failure choice, and the design's own retention guards went RED on it (2 of
  651) — the guard working, the fix withdrawn. Its companion, when the row was reconsidered honestly:
  **a row whose runtime pass was a PUN surviving by luck** (a pointer-to-pointer reinterpret through a
  transient slot address) **is DEMOTED to the compile-shape guard its own comment already claimed**,
  with the demotion stated in the commit as a SCOPE change of that row and the golden re-baselined from
  the REBUILT binary.
- **⚠ Route #7's ATTRIBUTION mirror: a crash INSIDE a generated shell is usually the shell being
  faithful** (measured 2026-09-02, runtime's `textAddr`). The `RecvGenerator` shell's
  DerefOrNull → NullRef → NRE on the first field touch IS Go's nil-receiver semantics; the nil came
  from `funcInfo()`'s module search, which can never succeed because the package's sole moduledata is
  a permanent empty stub (`len(pclntable)==0` skips it every time). A structurally guaranteed nil is
  not a race and not goroutine-specific — which tests crash is decided only by which ones reach the
  call at all. **Trace to the ASSIGNMENT, not the frame**, before billing `src/gen/`.
  ⚠ **And the CHEAPEST instrument for reaching that assignment is the BUILT GUARD BINARY run by hand**
  under the right runtime root (`DOTNET_ROOT`), 2026-09-05: the runner captures only the FIRST stderr
  line (`Fatal error.`) while the binary itself prints the whole managed stack, and the frame names the
  assignment. Two runs, five seconds, in place of a four-arm bisect that had already been priced.
- **Windows local time works (fixed 2026-08-01).** Binding the converted `time` exposed a pre-existing
  crash the stub had hidden: `time.Now().Weekday()` → `initLocal()` → `syscall.GetTimeZoneInformation`
  access-violated, because the wrapper hands the kernel the address of a managed `Timezoneinformation`
  whose `array<uint16>` name fields are managed references where Windows expects inline `WCHAR[32]`. That
  wrapper is now hand-owned against a blittable mirror (`core/syscall/windows/zsyscall_windows_impl.cs` — per-GOOS since r50a), guarded
  by the `LocalTimeZone` behavioral test — which compares real zone abbreviations and offsets against
  `go run`, not merely the absence of a fault. **The CLASS is still open, and it is now TWO classes**
  — wrappers passing a non-blittable struct by ADDRESS (the layout defect above), and wrappers taking
  a `**T` OUT-parameter, which arrive as NULL because `ж<T> → uintptr` answers 0 for a heap-boxed
  pointer that is still nil. The running census, the per-member remedy and why they are deliberately
  NOT fixed speculatively live on
  [`docs/phase4/BOARD-next-validation-candidates.md`](docs/phase4/BOARD-next-validation-candidates.md);
  re-measure there rather than carrying a count. The old note said "nothing exercises them today;
  `net` and `crypto/x509` will" — both now do: `net`'s DNS path forced `GetAddrInfoW`/`FreeAddrInfoW`
  (fixed 2026-08-16, guarded by `LookupServicePort`), and `crypto/x509`'s Windows system verifier is
  the measured consumer of the OUT-parameter class. Two walls stood behind them, both `net` /
  `crypto/x509` arcs rather than syscall ones — a **third** fork, where the kernel memory is a byte
  buffer the CALLER reinterprets, so no wrapper is at fault and no mirror-the-wrapper remedy applies.
  The first is CLOSED: `net.adapterAddresses` walked a native `IP_ADAPTER_ADDRESSES` chain out of a
  managed byte buffer and killed the process on the loop's own nil test; it is hand-owned since
  2026-08-17 (`core/net/windows/interface_windows_impl.cs` transcribes the whole chain — every
  record, its six nested lists and every sockaddr — into managed boxes), guarded by the
  `IpAdapterAddresses` behavioral test, and it is what unblocked Windows name resolution at all,
  since `dnsReadConfig` is `getSystemDNSConfig`'s only source of DNS servers. The second is still
  open: the CryptoAPI chain walk reads `CertContext` / `CertChainContext` back through raw addresses.
  ⚠ **The class has its ROOT, and it is not "one word where four bytes belong" (measured 2026-09-02):
  the CLR gives AUTO layout to any struct holding a reference-typed field and REORDERS it, so the
  KERNEL READS THE WRONG FIELD.** Converted `Msghdr`'s `Namelen` sits at managed offset 40 while the
  kernel reads `msg_namelen` at 8 — where it finds `Iov`, an object reference and therefore never zero:
  EISCONN on a connected stream, EINVAL on a datagram; `RawSockaddrInet4`'s `Addr` at managed offset 8
  is the heap pointer earlier instrumentation dumped. A correctly laid-out struct (`Iovec`) can still
  hand the kernel managed addresses, so the remedy is to ENCODE into a native buffer (a
  `writeNativeSockaddr`) or an explicit-layout blittable mirror — never a managed struct passed by
  address. A three-arm A/B (hand-own / generated body restored / hand-own back, `--no-incremental`) is
  what turns a "does not reproduce" into an attribution.
  ⚠ **That remedy has a BOUNDARY, and an identity seam sits on the far side of it** (2026-09-06):
  **marshalling answers a LAYOUT problem; a pointer the OS issued and REFERENCE-COUNTS is an IDENTITY
  problem, and no correct marshalling can answer one.** A fallback handing the kernel a marshalled
  native image of a managed view hands back an address the OS never issued — fatal where the site FREES
  memory the OS owns — so the remedy for a MISS on an identity seam is to **REFUSE BY NAME**, never to
  rebuild; the hit path already knows this, since it REMEMBERS an address rather than reconstructing
  one. Offering marshal-on-miss and refuse-by-name as two options on ONE axis is the sizing error, and
  they answer different problems: the certificate-context helper censused five producers — three
  native-backed, where the fallback is right, and two managed views that both remember — leaving the
  reaching set for the fallback EMPTY.
  ⚠ **The class's FIFTH member, and the reading rule it re-proves (2026-09-03):** `readMapping` handed
  `Module32FirstW` a managed `ModuleEntry32` — auto-layout ~64 B against the native 1080, with
  `module.Size` folded to 1080 — overwriting a kilobyte of heap, faulting in one test and resolving as
  an unrelated cctor crash in another. The routed description named two other functions entirely; the
  RECORD named neither. **A finding's prose is re-derived from its record before a cut is aimed.** The
  remedy is the class's: a `fixed`-buffer mirror plus a registry displacement, and its positive
  evidence is a downstream test getting PAST the API that validates the record size — an UNGATED row
  can read "unchanged" after a real fix when an earlier host-killer runs first, so only a gated run
  measures it.
  ⚠ **A TOKEN CUT'S ACCEPTANCE NAMES THE PLATFORM WHERE THE NATIVE SIDE DOES NOT PROBE** (2026-09-05,
  a union battery): "EFAULT rather than reordered memory" holds where libc merely READS the pointer;
  on Windows `ntdll` (`RtlGetVersion`) **WRITES** through it and the process faults 0xC0000005 — so the
  class such a cut makes LOUD (reference-bearing structs passed by address) is a PER-PLATFORM census
  question, and every REACHED member is a hand-own of the `GetTimeZoneInformation` shape. Two behavioral
  guards went red on `rtlGetVersion`, reached on every Windows TCP dial, where master had read silent
  zeros.
  ⚠ **A SECOND native-boundary class, measured 2026-09-03/04: LIFETIME, not layout.** golib's
  `uintptr` operator pins DURABLY but only for the BOX's lifetime, so a call site passing
  `(uintptr)Ꮡ(a, 0)` without HOLDING the box lets the backing array move during the native call (an
  unpinned array shown to MOVE first, then ARM 2 reading 0 and ARM 3 stable with the box held) —
  "durable" is not "unconditional". Sharper: the pin's holder is FINALIZABLE and sits on a box that is
  garbage the instant the take returns, so the pin is released on the finalizer's schedule and "the
  four takes are atomic" is true only while nothing runs between them. **The population is the
  PREDICATE (an address reaches a native call with no holder alive across it), never the idiom** — 43
  `(uintptr)Ꮡ(` sites are the idiom's count, not the hazard's. Its worst form compiles, emits
  byte-identical C# and passes every SERIAL gate: the `syscall` read/write wrappers mint their buffer
  pointer through the NON-RETAINING door (an implicit box-to-`uintptr` conversion), so only
  concurrency inside the kernel window sees it — 77 sites, `KeepAlive` **zero** corpus-wide, an
  absence turned into a measurement by a GOROOT-side derivation of the shape. **A pointer type with a
  retaining and a non-retaining constructor is a trap wherever an implicit conversion can select the
  non-retaining one**; mint through the retaining door, and note that a door taking its address inside
  `fixed` retains the BOX but not the PIN — retention and pinning are two properties a kernel-bound
  pointer needs both of.
  ⚠ **THREE CLR FUNCTION-POINTER FACTS, MEASURED, for any hand-own that hands a Go func value to
  native code** (i7, 2026-09-08): `Marshal.GetFunctionPointerForDelegate` REFUSES a GENERIC delegate
  TYPE whatever the target kind or overload, so a Go func value needs a NON-GENERIC
  unmanaged-function-pointer shim per arity forwarding by a TYPED call; `DynamicInvoke` never applies
  a user-defined implicit conversion, so a shim's forward is TYPED or it silently narrows the
  contract; and the native pointer's identity is per delegate INSTANCE, so a stable Go-style numeric
  handle per func value needs the shim instance CACHED — **and that cache is also the rooting the
  pointer needs for process life.**
  ⚠ **THE REFERENCE-BEARING PIN MISS IS STRUCTURAL, NOT ARC-BLOCKED — and that is a STRONGER result
  than "blocked".** For a reference-bearing `T`, `StandardBox` allocates no `m_slot`, so
  `PinnableStorage` is null, `EnsureStableAddress` never calls `PinnedBuffer.PinOnly`, and `m_pin` stays
  null: **no `PinnedBuffer` is ever CONSTRUCTED**, so "the pin was released by its finalizer" cannot be
  the mechanism. The address is REGISTERED anyway and validate-on-read refuses it (`IsPinnedAt` false
  when `m_pin` is null), so the recovery MISSES and the consumer holds a native alias of an address
  nothing was asked to keep still. **A `labelMap` carries references, so it can NEVER pin** — the
  pointer-token arc's arrival does not change that on its own. Measured and guarded at
  `PinnedBoxStalenessWitnessTests.cs`: 5/5 Debug, 5/5 Release+TC0, `Skipped: 0` load-bearing, and
  independently reproduced by a second lane (2026-09-07). ⚠ **A probe using `ж<long>` measures the
  CONTROL, not the case** — `long` is reference-FREE, so it allocates `m_slot`, pins and resolves; the
  witness file names that shape as the control in its own words.
  ⚠ **A B/op census cannot SEE what a pin COSTS** (2026-09-04): the `GCHandle`'s handle-table entry
  never lands on the GC heap at all, and a FINALIZABLE holder whose finalizer is suppressed only on a
  `Dispose` the contract never calls rides the finalization queue, is promoted at least a generation,
  and frees its handle on the finalizer thread — GC pressure proportional to pinned-box COUNT,
  reported as ZERO bytes. **Price a pin from the TYPE** (header, fields, padding, the handle, the
  finalizer) **and from the QUEUE it joins**, never from the bytes a run reports. And a remedy that
  changes WHERE storage is allocated (pinned-object-heap boxes) is its OWN increment with its own
  footprint and cost pair — never folded under the correctness cut whose evidence it would otherwise
  borrow.
  ⚠ **TRANSCRIBE the reference compiler's predicate rather than paraphrase the rule** (2026-09-04, the
  root of that gap): the converter's KeepAlive analysis carried the sentence "never through an
  intermediate variable", but `cmd/compile`'s `escape.rewriteArgument` tests the OPERAND TYPE
  (`IsUnsafePtr` into `IsUintptr`), so Go's guarantee covers the two-step form its own generated
  wrappers use 16 times per platform — **one wrong sentence in a comment was the whole gap**, and the
  fix is the compiler's predicate in the converter. Its census sibling: **a closed arc's set can be
  NARROWER than the Go contract it says it reproduces** — Go marks FOUR functions
  `//go:uintptrkeepalive` (`RawSyscall`, `RawSyscall6`, `Syscall`, `Syscall6`) and the converter's
  keep-alive funnel set covered only the `Syscall` family, leaving eleven generated Linux wrappers
  handing the kernel addresses with no keep-alive; re-derive such a set from Go's own annotations.
  ⚠ **Two more members of that class, measured 2026-09-05.** It has a MANAGED-CALLEE member: a
  `(uintptr)` bridge strips retention from a PINNABLE same-frame box exactly as it does from a syscall
  buffer, and a CONVERTED callee that resolves the number through validate-on-read refuses after any
  GC between the mint and the resolve — the landed `KeepAlive` fix's predicate was FUNNEL-shaped and
  never modelled a converted callee taking `unsafe.Pointer`, and a weak token does not retain either.
  **A retention fix is keyed on the ARGUMENT's shape (a bridged same-frame box), never on the callee's
  KIND.** And **a NAME-keyed predicate is per-platform BY CONSTRUCTION**: that fix's funnel set was
  the uppercase `Syscall*` family while darwin funnels through lowercase libc trampolines, so it
  covered ZERO darwin sites while its census guard read green on every train — **a guard reading ZERO
  on a target is either "no sites" or "blind", and only a PER-TARGET positive control (the guard RED
  on a tree known to carry unheld sites there) tells them apart.** Route #8's shape, one target over.
  ⚠ **A REMEDIED member of a class can still be EXPOSED when the class's RULE changes underneath it**
  (2026-09-05): a hand-own written BEFORE the managed-pointer token cut can be correct in SHAPE and
  broken in FACT if its own body takes the `uintptr` of a reference-bearing box — so **census the
  COMPANIONS' INTERNALS, not only the undisplaced wrappers.** What closed that question is the shape
  of the answer rather than its emptiness: **a NEGATIVE census is worth what its SHAPE is worth** —
  "no hand-owned companion is exposed" carried because 72 of 96 conversions were MEASURED to take a
  native pointer, positive evidence that the remedy's shape had been applied consistently, where an
  empty grep would have closed nothing. **State what the population DOES, not only what it lacks**,
  and name the limits (here an unflagged struct and the caller-side variant) that make the closure
  honest.
  ⚠ **A box cannot OBSERVE a write performed through a `ref` it has already handed out** (2026-09-05):
  the converted `*(*uintptr)(unsafe.Pointer(&slot)) = addr` writes through `ж<T>.Value`'s ref over the
  same managed storage, so a golib-side hook at the reinterpret seam runs when the VIEW is taken and
  not when the value is STORED — the remedy for a pointer-typed destination is a converter EMISSION
  change, and **an increment whose halves are converter + golib owes BOTH gate families and lands
  together.** Its record-first companion: `NativeBox<array<T>>` over Go's headerless `*[N]T`
  reinterprets element bytes as a managed `T[]` HEADER — it IS the prestub null read it was meant to
  fix — and a design refuting its own shorthand on paper is the argument for writing the record first.
  ⚠ **A NATIVE CALLBACK'S ARGUMENT BRIDGE IS A REINTERPRET, NOT A CONVERSION** (ruled 2026-09-08):
  Go's own callback wrapper copies each argument's SIZE BYTES from the native word at its ABI offset —
  raw bits — so the shim's typed forward reads the LOW bytes of each word into the converted parameter
  type, refusing BY NAME where the type is wider than a word, in Go's own words. A record's PROPOSED
  mechanism — an expression-tree compile, chosen precisely because its convert node resolves
  user-defined operators — would have failed at compile time on the first struct-parameter arity,
  because the converter mints NO such operator for those types: **the premise "a conversion the binder
  refuses" was the wrong reading of what the forward must DO.** The ahead-of-time caveat survives only
  in the generic instantiation, bounded to test-host reach with the falsifier stated; and the decision
  was RAISED FROM THE BODY INTO THE RECORD before a line was written, which is the third time that
  order paid in one arc.
  ⚠ **A RULE KEYED ON THE WRONG ATTRIBUTE IS A LOWER BOUND, NOT A RULE — and the ruled remedy is a
  THIRD ANSWER declared ABSTRACT on the base** (2026-09-06). The pointer operator asked a PINNABILITY
  property ("can this be held still?") the question "is there an address here at all?", and those
  differ for every field or element reference rooted in a reference-bearing CONTAINER whose pointee is
  reference-FREE — the class that regressed every Windows dial. The narrow remedy, testing the
  POINTEE's type, restates the same confusion one level down and leaves a human obligation with no
  compiler behind it; declaring the answer ABSTRACT makes every box kind STATE it or the assembly does
  not compile. Two things that bought, neither party having them in mind. **A lane's BASE can lack a
  type the UNION contains**: the repair implemented the new member for all SIX box kinds its base
  carried and the assembly — which also carries a SEVENTH kind from another lane's increment on the
  intervening train — failed at the first build with CS0534, where the pointee-typed predicate would
  have COMPILED at the union and given the seventh kind an answer to the wrong question. And **a
  comment recording WHY a member is abstract, at the site, turns the next author's surprise into an
  instruction** — the eighth kind's author meets the same build error, and the difference between a
  wall and a signpost is one paragraph naming the error code and what it prevented, paired with the
  argument from the other side (ordering forwards to the SOURCE's token, so the identity is the source
  variable's and never the materialized copy's location). **A REPAIR CHANGES ONE THING and says which
  thing it is NOT changing**: this one reopens the standing pin-unheld hole for the restored class,
  exactly as the pre-merge code did and no wider, and closing that hole is its own arc with its own
  population and guard — a scope statement of that shape, made BEFORE the cut, is what keeps both
  changes reviewable.
  ⚠ **A BOX'S POINTEE TYPE IS ITS TYPE ARGUMENT, NEVER ITS STORAGE** (2026-09-08): a finalizer
  registration check that resolved the pointee from the STORAGE OBJECT named the CONTAINER in its
  refusal message for a field-reference box whose dynamic type is the field's own pointer type,
  refusing at iteration 0 a shape Go ACCEPTS — a loud refusal at the wrong index, which is better than
  a five-minute silent hang and still wrong. **For every box kind the pointee is the type argument**,
  and a predicate keyed on any other field carries a RED-FIRST arm per box kind — field-reference and
  element-reference included — or it is guarded only on the standard box.
  ⚠ **Ordering managed pointers by a token derived from `RuntimeHelpers.GetHashCode` is unsound BY
  CONSTRUCTION** (2026-09-03): identity hashes collide (26 bits on CoreCLR), so two distinct
  allocations can share a span and every address-ordering predicate over them (`alias.AnyOverlap`,
  `slices.overlaps`) can answer true for disjoint buffers — a birthday event that shows only on a path
  minting millions of pairs. Two rules beside it: **a seat that moves the corpus onto the path Go
  takes is CORRECT even when that path has a latent defect the old path never reached** — the seat
  stays and the defect roots; and a crash a QUARTER of the way into a stream is a different signature
  from a two-row divergence at the end of a complete one, so a standing "NOT MEASURABLE on this box"
  ruling covers the latter only.
  ⚠ **A SATURATING INDEX FIELD IN A POINTER-ORDER TOKEN IS A CORRECTNESS BREAK, NOT A PRECISION LOSS**
  (ruled 2026-09-08): pointer equality is token-based and the managed-pointer registry is KEYED by the
  token, so two distinct elements whose indices SATURATE to the same value compare EQUAL and collide
  in the registry, which then resolves the wrong box. Found by WRITING the candidate out as a DESIGN
  rather than as a sentence — which is what turned "the narrow split is arguable" into "its only sound
  form holes the door where the biggest arrays are". Ruled: the variant in which every index carries
  the tag is taken unless a hot-loop bench measures it worse, and any fallback that would replace it
  lands with its hole COUNTED, never as a refusal.
  ⚠ **A shared syscall dispatcher that mis-calls the WRITE primitive MUTES the entire platform's
  runtime error output** (measured 2026-09-03 on darwin): the keystone walked the pointee of an ABI0
  `&fd` — which names the first of three contiguous stack args — and passed fd + junk, breaking
  `read`/`write1`; `runtime`'s `throw`/`print` reach `writeErr` → `write1` on the same shape, so every
  death on that platform is exit-138/SIGBUS with a **blank stderr** and nothing can reach it. For a
  stderr instrument, a blank-stderr death on such a platform is the mis-call SIGNAL — a symptom to
  attribute, never an instrument gap to chase — so the mute baseline is recorded BEFORE the primitive
  is seamed. Its neighbour, and the reason the instrument came first: **a STACK before a design.** A
  full-stderr instrument read two deaths whose predicted sites — both with falsifiers named — were
  FALSIFIED on site in twenty minutes of runner time, retiring two designs; an instrument's COST is
  quoted with its result so the next reader can afford to measure before designing.
  ⚠ **READ THE DISPATCHER'S SOURCE before naming the address a native write went through**
  (2026-09-05): "through the interior address of an unpinned box" was the fitting story, and the code
  said the box was never handed to libc at all — 35 of darwin's 50 `libcCall` sites box the FIRST
  parameter ALONE where Go hands a contiguous `cgo_unsafe_args` block. The REMEDY did not move; the
  MECHANISM sentence did, and a "slot" instrument that would have cost a day was answered by
  construction.
  ⚠ **A stdout LINE COUNT places a mute death without a byte of stderr** (2026-09-04): a guard that
  prints six lines printed exactly TWO on both mac legs, so both died inside the third statement — and
  the leg that did produce a stack named the same frame, two independent derivations agreeing. **A
  capture stage's NULL is evidence only beside a POSITIVE CONTROL in the same capture** (the leg that
  printed through that same stage), and **a ranked hypothesis the run does not support is WITHDRAWN**
  rather than kept alive as "still possible": an alignment story explained one leg's death MODE and
  nothing about WHERE, which the line count settled.
  ⚠ **A CORRUPTION THAT DOES NOT CRASH ON ONE PLATFORM IS THE SAME CORRUPTION** (2026-09-05): x64 and
  arm64 perform the identical 16-byte native write through a STALE REGISTER — the libc dispatcher
  places the first parameter's one field and leaves the trampoline's second and third registers as
  the caller-saved state left them, while Go's `cgo_unsafe_args` block is unpacked by offset — and
  only the leg whose stack walker reads those bytes dies, so **the MUTE leg was the honest one.** Two
  reading rules with it: **a death inside a panic's own REPORT (a stack-trace capture) is scored at
  the door the report was ABOUT**, and **a mute death's exit code says nothing until the crash
  report's frames and registers place it** (pc/far/esr — a data read of a value that is nothing's
  address is a fingerprint to decode, here `sigaction`'s mask/flags word).
  ⚠ **A DUMP, NOT A STACK AND NOT A STORY, NAMES THE WALL** (2026-09-05): a blank-stderr **exit 139**
  from a managed host can be the CLR's own PRESTUB dispatching a generic method on a CORRUPTED
  reference — a raw native address landed in a managed slot by an unsafe store — and the SINGLE-FILE
  host writes no dump and no stderr at all. The framework-dependent host under
  `DOTNET_DbgEnableMiniDump` plus `dotnet-dump … clrstack -all` (and `gdb` for the faulting
  instruction) reads it; nothing cheaper does.
  ⚠ **A SIGNAL DISPOSITION IS A KERNEL FACT, and a bridge that models it in managed code satisfies
  neither of its consumers** (2026-09-05, met on linux and darwin in one week). Go's `signal.Ignore`
  is a kernel `SIG_IGN` — the KERNEL consults it (`tty_check_change` lets a background-pgrp process
  `tcsetpgrp` only if SIGTTOU is ignored or blocked) and it is INHERITED ACROSS EXEC — so a bridge
  modelling Ignore as swallow-in-handler leaves `SIG_DFL`, and a host in a background process group
  under a tty STOPS FOREVER on SIGTTOU: the mute class in a T-state costume, no handler, no deadline,
  no results file. Attributed by four arms varying ONE axis each, the last two differing only in the
  kernel disposition. The TERMINAL-FREE instrument is an exec'd child PRINTING each disposition (Go
  reads 1 on every line): it needs no pty, cannot stop, and separates the two models where a canary
  under `script` can only hang — **read at the code first, then measure, then cut.** Two companions.
  **A CORRECT change exposing a latent gap is a FINDING, never a regression** (the roster is untouched
  because no fleet sweep has a controlling terminal). And **the CLR-free signal class is PER FLAVOUR**:
  darwin's differs from linux's by SIGUSR1, coreclr's activation-injection signal where no `SIGRTMIN`
  exists (`pal` `signal.cpp`), so a kernel `SIG_IGN` there discards GC-suspension activations — read
  the RUNTIME's SOURCE for such a boundary, never memory, and state the class per flavour in the
  bridge's own header.
  ⚠ **A cross-package COPY of a hand-owned type with lazily-shared state is correct BY ACCIDENT after
  the owner's first use and fatal before it** (2026-09-03): the converter's box wrap over a
  package-qualified selector boxes a COPY of an exported package var, and a hand-owned `RWMutex`'s
  state is created on first use and shared — so a copy taken after the owner touches the var lands on
  the real lock while a copy taken before gets its own. That is why no linux/windows row ever saw the
  class and why one darwin arrangement fatals. **A guard for such a fix must hold the sub-library var
  UNTOUCHED before the cross-package pair, or it is green by the mask** — measuring the mechanism on a
  scratch module before cutting the guard is what caught it, the corollary to "a stack before a
  design". The SAME-PACKAGE control rooted the defect: the converter emits the right form (the
  exported box) for a local var and loses it on the qualified selector, so the defect is in the
  selector path, not the receiver machinery.
  ⚠ **Two companion-file rules from the same seam** (2026-09-06). **A companion may deliberately
  COMPENSATE for a caller belief that is factually FALSE, and it must say so IN ITS OWN WORDS or the
  compensation is invisible**: a DNS list-free is a NO-OP because the caller keeps Go's deferred free
  where Go put it while the native chain was already freed and the caller holds a managed
  transcription; and the accept path IGNORES the caller's buffer because under our representation that
  buffer is unusable both as data and as a table key. A caller-side reading cannot see either without
  the companion's own statement. And **a handoff keyed on the CURRENT GOROUTINE turns a cross-package
  sequencing property into a load-bearing invariant that no type, signature or gate expresses**: the
  accept staging is parked in a weak table keyed on the goroutine and consumed by an entry point whose
  signature carries no handle, no overlapped and no identity at all, with the premise — one goroutine,
  no interleaved accept — stated only in a comment. Both violation modes throw by name, so it fails
  loudly rather than silently; but the remedy that REMOVES the invariant keys on something structural
  (the descriptor, or the overlapped that already identifies the operation at submit and harvest),
  while a guard only documents it.
