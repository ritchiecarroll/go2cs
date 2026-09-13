---
name: measurement-discipline
description: Design or judge a measurement. Controls, one-axis A/B arms, positive and negative controls, census predicates, second derivations, predictions scored as worded, and the configuration axes that flip a verdict. Use when a number is about to be quoted or a claim banked.
---

# Measurement Discipline

<!-- EXTRACTED VERBATIM from CLAUDE.md at 1800b04f8, lines 3152-3352, 6598-7270, 7271-7573.
     Phase 1 of docs/PLAN-context-diet.md: RELOCATION ONLY, no compression, no edits.
     The byte-identical original is docs/doctrine/JOURNAL-2026-09-12.md.
     Phase 2 distills this file. Keep every dated derivation, but move it into an HTML
     comment beside the rule it justifies: comments are stripped before this file enters
     context, so provenance kept this way costs ZERO tokens. -->

<!-- PHASE 2 DONE 2026-09-12. The visible half states each trap ONCE, as an imperative. Every
     dated narrative, SHA, file:line, package name, incident, magnitude and mechanism that
     justified a rule -- and every SECONDARY VARIANT of a rule stated above only once -- is
     preserved VERBATIM in the DERIVATIONS comment closing the section that carries it. Nothing
     was deleted: those blocks, sorted by source line, reproduce lines 15-1191 of the Phase 1
     body byte for byte, no gap and no overlap (asserted when this file was built). So when a
     rule reads thin, when you need the incident or the number behind it, or when you are about
     to design an instrument in one of these areas, READ THE DERIVATION BLOCK under its section.
     It costs nothing until you open it, and it is where the variants, the retractions and the
     second derivations live.
     BATCH19 MERGED 2026-09-12: the fifteen doctrine hunks the extraction map routes here from
     origin/claude/coord-doctrine-batch19 (commits 24bfc8304 / c5e17217b / e9e56b657, items
     1154-1321, all PURE INSERTIONS into the pre-split CLAUDE.md) are folded in -- most as NEW
     INSTANCES of rules already stated, whose dated narratives sit at the END of the relevant
     DERIVATIONS block under a "BATCH19" banner; where an instance widened, completed or corrected
     a rule the visible line says so and BOTH narratives are kept. -->

## Configuration is part of the verdict

- **Name the configuration in every verdict and comparison**; `<pkg>.tests.csproj` pins none, so an unnamed
  verdict sits at an optimization level no user ships. Of RECORD: **Release, tiered JIT off**, in
  `testEnvironmentRecord{Configuration,Tiered}` (never `omitempty`), `results.json`, and the proof pages.
  Build instruments with `-test-config Debug|Release` and `-test-tiered`, never a bare `dotnet build -c
  Release`; `-test-release-tc0` is RETIRED. Both defaults are Release, with `internal/godebug`
  `TestCmdBisect`, `log/slog` `TestCallDepth` and `net/http` `TestRegisterErr` opting out via
  `execution: release-tiered`. **An override predicate keyed on a default's VALUE becomes always-on when the
  default flips, and an override SUPERSEDES per-row annotations** — key on `$PSBoundParameters.ContainsKey`.
- **Run the configuration A/B before suspecting any commit**; identity inferred from a STACK is
  configuration-fragile. **Two runs agreeing prove DETERMINISM, not causation**; a colour asserted as a
  configuration property needs two draws per configuration, and a host baseline is stable-failures PLUS
  flaky-rows. **Re-measure at Release any GC-liveness, codegen-liveness or pin-lifetime reading taken at
  Debug** — a non-optimizing frame roots temporaries, so every pin holds.
- **Release+TC0 escape analysis can make an ALLOCATION probe read zero**: self-controls going red says every
  guard on it would pass VACUOUSLY, so make the self-control body ESCAPE, never skip-with-reason. **A
  stack-reading guard loses lambda frames to inlining** — pin `NoInlining`, keep edits OUT of the `#line`
  region it measures, type the sink GENERIC. **A TIERING-class guard WARMS UP (thirty-plus calls, past
  tier-1) or it is vacuous**, and a missing frame is read as a SEQUENCE, never a sample. **The one-variable
  matrix — base vs cut × tiering ON/OFF, same box, same build — separates a configuration class from a
  regression**; identical at both tiers means no tier axis.
- **A Go finalizer run INLINE on the CLR finalizer thread deadlocks against `runtime.GC()`'s wait whenever
  the body blocks**, leaving the thread dead for every LATER test in that host; `mfinal.cs` now carries Go's
  shape. **The default binder NEVER applies a user-defined conversion, and a DEFINED type is a WRAPPER with
  implicit operators, not a subclass**, so `DynamicInvoke` on a Go-typed argument binds NEVER: dispatch by
  ASSIGNABILITY, enforce at REGISTRATION, never swallow a BINDING failure.
- **An agreeing failure on an ABSENT HOST CAPABILITY masks a question; agreement on shared semantics answers
  one** — name the capability and tests on the row and proof page; a second host in the same state gives
  reproducibility, never coverage, while a host that DIFFERS on that axis retires the caveat and earns a
  DATED coverage line: the row's COMPOSITION changes where its matched COUNT stands. **Never write a host's
  capability into a record from a count that is INVARIANT across the axis it describes** — every number in
  the clause can be right while the clause is wrong, because rows that move with the capability move on BOTH
  sides; read the SET, probe the capability directly, and state an unreconciled contradiction AS
  unreconciled. **Read the RECORD, not your own SUMMARY**, and **read the test's OWN SOURCE before framing
  its failure mode.**

<!-- DERIVATIONS (configuration, finalizers, agreement vs coverage) — Phase 1 text, verbatim:
- **⚠ THE MEASUREMENT CONFIGURATION IS PART OF THE VERDICT — the `-tests` pipeline publishes DEBUG
  (measured 2026-09-02, the net/http h2 pair).** The generated `<pkg>.tests.csproj` pins no
  Configuration, so every roster verdict to date was taken at an optimization level no user ships: one
  published artifact flips `TestWriteDeadlineEnforcedPerStream/h2` fail→pass under Release (43.7 ms vs
  500–1000 ms per handshake), and default tiering flips it BOTH ways across consecutive runs of that
  same binary — a validation-integrity defect, the flake class arriving through the JIT. Ruled
  contract: **Release + `DOTNET_TieredCompilation=0`, both RECORDED** in two places that cannot
  silently drift — the comparison record (`testEnvironmentRecord{Configuration,Tiered}`, never
  `omitempty`: absence must not read as Debug) and the host's own `results.json` — plus the proof
  pages. The converter carries it as TWO flags since the tiering census — **`-test-config
  Debug|Release`** (Release publishes with an explicit `-p:go2csPath`, replacing the csproj template's
  Debug-conditional default, and disables the CLR's tiered JIT by default) and **`-test-tiered`** (the
  explicit opt back IN to tiered JIT, meaningless under Debug); the earlier `-test-release-tc0`
  spelling is RETIRED and survives only in one comment in `testConversion.go`. A bare `dotnet build
  -c Release` on the generated csproj is the trap they avoid: **grep the converter's flags before
  building an instrument**, and NAME both sides' configuration.
  ⚠ **Owner ruling (2026-09-02 11:44): the validation configuration of RECORD is Release with tiering
  off; Debug stays available by flag; the pipeline and sweep defaults flip after the Release census.**
  ⚠ **Falsify at the CHEAPEST layer, and separate a gate's PREMISE from its CONSEQUENCE** (2026-09-02,
  the Release census's own blocker): a `beforefieldinit` lazy-static-init hypothesis for a shim's
  Release-only flag rejection died to one grep of the pinned GOROOT — the flag does not exist in Go
  1.23.12 and the converted shim registers 45 = 45, i.e. an external runner/shim version skew — while
  the real event (an access violation at Release in a published single-file host, since unreproduced
  in two further runs and carried OPEN) stands unexplained. **A gate whose premise was wrong keeps its
  CONSEQUENCE when the consequence stands on its own**: a default flip that would UNMEASURE a
  3,643-verdict row is not taken on a corrected premise.
  ⚠ **THE FLIP LANDED 2026-09-02**, the census complete (`docs/phase4/CENSUS-release-tc0-delta.md`:
  195 of 201 rows unchanged, six disclosures retiring, nothing owed a root). `-test-config` defaults
  to **Release** and `run-validated-sweep.ps1`'s `-TestConfig` to **Release**; Debug is a flag away.
  **THREE rows opt back OUT via a new `execution: release-tiered` annotation** — `internal/godebug`
  (`TestCmdBisect`), `log/slog` (`TestCallDepth`) and `net/http` (`TestRegisterErr`) — all three
  PC/line-attribution assertions that tiering's presence supplies, each measured as a one-axis A/B,
  never inferred. `release-tc0` is retained though redundant. ⚠ **The sweep's override predicate had
  to change WITH the default and it is the trap in this flip:** it was `($TestConfig -ne 'Debug') -or
  $TestTiered`, which carried past the flip makes EVERY default run an override — and an override
  SUPERSEDES per-row annotations, so all three opt-outs would silently run at TC0 and fail while no
  run stayed bank-eligible. It now keys on whether the caller SPECIFIED the parameter
  (`$PSBoundParameters.ContainsKey`), so the default respects annotations and is bank-eligible while
  any EXPLICIT flag — the default's own value included — forces uniformity and is not. A default's
  value and a default's *explicitness* are different questions, and a predicate written when they
  coincided answers the wrong one afterwards. Proof pages and comparison records written before the
  flip still say Debug and are stale-until-reswept BY DESIGN; a rebank wave levels them.
  ⚠ **The stack-walk tiering class has a member in our OWN hand-own** (measured 2026-09-02, a one-axis
  A/B at `01a7fdefe`): `reflect`'s `valueMethodName` walks `StackTrace(2)` for a `_package` frame and
  LOSES the Recv frame under Release+TC0 inlining, so `TestValuePanic` passes at Debug and fails at
  Release on the SAME head. **A row that appears under the new default is attributed by the
  configuration A/B BEFORE any commit is suspected**; the remedy is the method name reaching `mustBe`
  explicitly with the walk retired, because a hand-own that infers identity from a STACK is
  configuration-fragile by construction.
  ⚠ **After the flip, every comparison NAMES its configuration beside the tree** (2026-09-02), and a
  set diff whose arms were taken at different times reads the configuration back from each RECORD
  rather than assuming it: a morning control at Debug against an evening pair at the new Release+TC0
  default made a row "appear" that had merely flipped on the configuration axis. Two runs agreeing
  prove DETERMINISM, not causation, when both sit on the same side of an unnoticed axis.
  ⚠ **ONE SAMPLE MISTAKEN FOR A PROPERTY** (2026-09-07): a GolibTests row RED at Release+TC0 and GREEN
  at Debug was reported as configuration-dependent; at the next SHA — a +59/−0 test-only commit that
  could not have touched it — the row was GREEN at both, so the row is NON-DETERMINISTIC on that box
  and the earlier reading was ONE DRAW. **A colour asserted as a configuration property needs two draws
  per configuration before it is one; and a host baseline is stated as stable-failures PLUS
  flaky-rows, never as one number.**
  ⚠ **A codegen-liveness disclosure whose measurement PREDATES the flip is re-measured at Release
  before it is quoted** (measured 2026-09-03): `unique` read 7/20 at Debug and **16/20** at the
  Release default with ZERO flake over eleven runs, so a board blocker resting on a Debug
  frame-residency A/B (RETAINED 4/4) dissolved — eight of its ten GC rows pass at Release, exactly as
  the class's own text predicted, because tier-0 frame liveness is a JIT artifact. ⚠ **Both figures
  are superseded by the 2026-09-05 re-read on the same head: `unique` is 19 of 20 at Release+TC0 and
  8 of 20 at Debug**, the ten `checkMapsFor` rows CONFIRMED as codegen-liveness by a one-axis tier
  A/B — and the twentieth is the one that matters, because `TestMakeClonesStrings` fails IDENTICALLY
  at BOTH tiers, so **frame residency is FALSIFIED as its mechanism** (five candidates eliminated:
  clone retention, `strings.Clone` delegation, CWT keying, the StringData pin, the tier). Per the
  four-candidates rule below, the next step there is an INSTRUMENT — a heap root-path read — never a
  sixth hypothesis.
  ⚠ **A pin-lifetime or GC-liveness PROBE runs at Release with tiering off, or it is not a
  measurement** (2026-09-03): the SAME probe read 6 million clean calls at Debug and went red in 9 s
  at Release+TC0, because a non-optimizing frame roots its temporaries for the method's life so every
  pin holds. This is the MIRROR of the `internal/poll` finding where the same hypothesis was measured
  FALSE — **both stand**: the configuration is part of the measurement, and which way it cuts is
  decided per case by running it, never by precedent.
  ⚠ **And the flip can make an ALLOCATION probe read zero** (measured 2026-09-04): under
  Release+TC0, .NET's escape analysis stack-allocates a small object from the first call, so a probe
  reads zero for a body that allocates at Debug — the probe's own SELF-CONTROLS go red, and that red
  is the instrument telling you every allocation guard resting on it would otherwise PASS VACUOUSLY.
  Keep the self-controls; the durable fix is a self-control body that ESCAPES (a static store, an
  interface return) so it allocates under full optimization too — never a skip-with-reason, which
  would leave the whole allocation-guard family unmeasured at exactly the configuration the roster's
  alloc verdicts are taken under. Its sibling in the same run: **literal-frame NAMING guards that read
  a stack lose their lambda frames to inlining** at the same flag — pin the named bodies
  `NoInlining` for the guard, and RECORD that the feature itself (`runtime.Stack` frame sets,
  recorded-literal-frame names) is inlining-dependent at the configuration of record, which any
  stack-counting host logic must account for. **The one-variable matrix — base vs cut × tiering ON vs
  OFF, same box, same build — separates a configuration class from a cut's regression in one read.**
  ⚠ **A guard for a TIERING class WARMS UP, or it is vacuous by construction** (2026-09-04):
  `net/http`'s leak check lost its first frame from the **174th** call of a hot method on, in ONE
  process — tier-1 promotion with PGO raising the inline budget at a hot call site — while a
  single-test arm (five calls) and a one-call guard both rendered the frame PRESENT at both tiers and
  "refuted" the mechanism. **A missing frame is read as a SEQUENCE** (dump every call, find the
  boundary), never as a sample; a guard for the class makes thirty-plus calls, waits out the tier-1
  delay, then asserts, and its control goes RED on the old code under tiered+PGO. "Refuted at one
  call" is a statement about one call.
  ⚠ **Frame-pinning mechanics from the same week** (2026-09-04): an attribute on a lambda EXPRESSION
  reaches its synthesized backing method, so pinning a frame is ONE attribute and not a shape change;
  an edit INSIDE a `#line` region moves the very position a guard measures, so attributes go inline on
  the mapped line, explanations OUTSIDE the region, and the mapped lines are re-read afterwards; and a
  `NoInlining` sink is GENERIC, never `object`-typed, because boxing a value type would hand a byte
  invariant bytes it did not earn. The byte-cost invariant's DIRECTION decides what a blind probe can
  hide: a stack-allocated object only makes `objects*24 > bytes` MORE likely, so an unescaped table
  produces phantom violations and can never hide a real over-charge — every shape is made to escape so
  no future one goes quietly stack-allocated.
  ⚠ One adjacent hazard the same census surfaced: the finalizer sentinel runs the Go finalizer
  INLINE on the .NET finalizer thread, so a finalizer doing an unbuffered channel send DEADLOCKS if
  the object ever becomes collectible during `runtime.GC()`'s `WaitForPendingFinalizers`.
  ⚠ **That hazard was ROOTED 2026-09-04, and it has NO JIT-tier axis** — which is what a hang
  identical at Debug and at Release+TC0 was saying (four candidates measured out, then an INSTRUMENT:
  five arms with the prediction committed first, the unfixed tree the guard's own red). A Go finalizer
  body run INLINE on the CLR finalizer thread deadlocks against a `runtime.GC()` that waits for
  pending finalizers whenever the body waits on its CALLER — a test that blocks inside its finalizer
  until the test ends does exactly that, and a deadlock has no tier to vary. Go's model is ONE
  goroutine running all finalizers sequentially, a parked finalizer parking only itself, and
  `runtime.GC()` waiting for no body; the fix is that shape (a dedicated runner, the sentinel handing
  off) with the converted GC's stronger-than-Go drain KEPT for well-behaved finalizers and BOUNDED as
  a safety net, the divergence stated.
  ⚠ **Two 2026-09-06 amendments — one WIDENS the hazard, one NARROWS its motivating row.** It is a
  CORPUS-WIDE latent defect rather than a row's problem: any Go finalizer that blocks — an unbuffered
  send, a mutex, a receive — parks the host's finalizer thread FOREVER, so the converted collection
  call never returns for its caller AND the thread is disabled for every LATER test in that host. The
  evidence it is real is a census rather than a hypothesis: **EVERY working finalizer-notification
  channel in the corpus is BUFFERED**, across two packages, and the only unbuffered one is the single
  test that does not pass — a property of Go's tests, not of our runtime. But **read the test's OWN
  SOURCE before framing its failure mode**: the deadlock framing for that row was refuted from its own
  lines — its `select` IS a concurrent receiver for a full second and the collection call is made
  BEFORE the wait, so a call that never returned would produce a package deadline rather than the
  measured one-second failure. The hazard survives with a NARROWER trigger, the finalizer running LATE
  after the receiver gives up, from which point the send has nobody; and the row that raised it cannot
  answer whether that happened, being the last row in its package and the only finalizer in it. The
  faithful shape is still a real finalizer goroutine, and it stays a DESIGN increment because other
  rows rely on finalizer bodies having RUN by the time the collection call returns — the drain
  semantics need a ruling before anything moves.
  ⚠ **A RULING PREMISE QUOTED FROM THIS FILE IS CHECKED AT THE TREE BEFORE THE RULING POSTS**
  (2026-09-07): the coordinator ruled "adopt the Go finalizer shape" from the sentence above that the
  sentinel runs the Go finalizer INLINE — which describes master BEFORE 2026-09-04 — while `mfinal.cs`
  (`d17103497`) already carries the ruled shape point for point, found by a lane OPENING the file. The
  wall attribution built on that premise was withdrawn, and **the paragraph above is amended by this
  measurement**: the INLINE description is history, not the current tree. Companion from the same read:
  **a finalizer registry that validates only `is Delegate`, beside a catch that swallows EVERY
  exception, makes a mismatched finalizer register silently and never run** — infrastructure failing
  silently, distinct from the documented user-panic divergence — and a guard whose five arms all match
  the parameter type BY CONSTRUCTION has never varied the type axis.
  ⚠ **A DELEGATE INVOKED THROUGH THE DEFAULT BINDER NEVER APPLIES A USER-DEFINED CONVERSION, and the
  converter emits a DEFINED type as a WRAPPER with an implicit-operator pair, never as a subclass**
  (`a55450206`, ruled `7ec93b2b5`, 2026-09-08). So any golib seam that dispatches a stored `Delegate`
  on a Go-typed argument — `SetFinalizer`'s runner, and anything else that `DynamicInvoke`s — binds a
  defined-pointer-typed object to an underlying-typed parameter NEVER, while a runner that swallows the
  exception presents it as "dequeued, never delivered". **Dispatch by Go's ASSIGNABILITY** (the
  generated `op_Implicit` for defined↔underlying, the adapter shell for an interface parameter), with
  the same predicate enforced at REGISTRATION, and **never swallow a BINDING failure — it is
  infrastructure, not a Go divergence.**
  ⚠ **AN AGREEING FAILURE ON AN ABSENT HOST CAPABILITY IS NOT THE SAME KIND OF AGREEMENT AS ONE ON
  SHARED SEMANTICS — the first MASKS a question, the second answers one** (measured 2026-09-06). The
  `os` row's Go ORACLE failed eight tests on a box lacking the Windows symbolic-link privilege; they
  agreed name for name on both sides, counted as matched, and the arithmetic closed exactly (685 = 645
  pass + 32 skip + 8 agreeing-fail, in-Go-not-C# EMPTY). Nothing was wrong — **but neither side ever RAN
  those eight, so the row learned nothing about them and must SAY SO, naming the capability and the
  tests, on the row and the proof page.**
  ⚠ **A reproduction on a SECOND HOST IN THE SAME STATE is a reproducibility result, not a coverage
  result.** The same row reproduced digit for digit on another box — but both hosts lacked the
  privilege, so what retires is "one host only" and NOT the coverage caveat; a privileged host's reading
  remains unmeasured by anyone. **Two hosts agreeing is evidence about determinism, and evidence about
  coverage only if they differ on the axis in question.**
  ⚠ **And the way it surfaced is the durable half: read the RECORD, not your own SUMMARY.** A summary of
  a comparison reports the DIFFERENCES and silently drops the agreements — precisely where a host
  condition hides. The hardest direction to point that rule is at your own prose: a lane re-deriving a
  funnel "from the artifacts rather than from my summary of them" found a key-mangled table, a
  mis-framed artifact and a headline figure that should have been 29 rather than 28 — **a number it and
  the coordinator had quoted four times.** The next person to quote a figure quotes the writers, not the
  artifact.

  ===== BATCH19 (24bfc8304 / c5e17217b / e9e56b657, merged 2026-09-12) — verbatim =====
  These two COMPLETE the agreement-vs-coverage pair above: the first supplies the case the rule above
  could only describe in the negative (a second host that DOES differ on the axis), the second is a new
  trap in its own right and is now stated visibly.
  ⚠ **A SECOND HOST HOLDING THE PRIVILEGE CHANGES A ROW'S COMPOSITION WHILE ITS COUNT STANDS**
  (2026-09-08): `os` on the i7 read Go 665 pass + 20 skip with ZERO failures, against the bank
  host's 645 + 32 + 8 agreeing symlink-privilege failures — **683 matched either way**, but the
  eight tests RAN and PASSED on both sides here, so the roster's "no second host has read this row"
  caveat becomes describable and gains a dated coverage line. Two hosts agreeing is evidence about
  coverage only when they DIFFER on the axis in question; these did.
  ⚠ **A PROVENANCE CLAUSE INFERRED FROM A COUNT THAT IS INVARIANT ACROSS THE AXIS IT DESCRIBES CAN
  BE WRONG WHILE EVERY NUMBER IN IT IS RIGHT** (2026-09-08): the `os` proof page's clause said the
  i7 "also lacks" the symlink privilege because its `683 matched / 2 disclosed` matched the bank
  host's — but all twenty rows that move with the privilege move on BOTH sides (8 agreeing fails
  become passes, 12 skips become passes), so 683 CANNOT discriminate the hosts and the SET was never
  read. A fresh reading contradicted it, and a two-line probe (the symlink call succeeds in the
  sweep's own shell, though the token lists no such privilege and the developer-mode value is
  absent) SUPERSEDED the clause for that host, stated as a dated reconciliation with "whether the
  state differed then is not recoverable". **Read the SET before writing a host's capability into a
  record**; the seat that found the contradiction stated it UNRECONCILED rather than resolving it by
  inference.
-->

## Host qualification

- **Preflight `go test -count=1 net` before any net-family run**; on an unqualified host the two A/B arms run
  different oracles. **The gate's criterion is the FAILING SET, a LEDGER not a threshold**: a NAMED,
  EVIDENCED universally-drifted leaf is tolerated with evidence at the site, ANY other failing leaf ABORTS by
  name, names print either way; Go 1.23.12's `TestLookupCNAME` is such a leaf. `.`-source the criterion block
  and control it in four arms against the LIVE block — **a warn-and-continue switch is the lie-lever shape.**
- **A live PUBLIC DNS assertion is universal drift once three independent resolvers agree.** **A lane never
  changes a host's system configuration on its own initiative**: relay, then RE-qualify — and **an
  unqualified authorization given in ONE context does not self-extend to a DISTINCT ENVIRONMENT on the same
  box** (a WSL root is not the Windows host): ask for the one line. **A count borrowed
  across boxes is re-measured on the box that will score it** — a number from ANOTHER box is not a baseline
  at all; **a first-run failure that does not recur is a HOST ARTIFACT**, never a disclosure; **counts
  differing between hosts are read against the record's HOST-CONDITIONAL ENTRY.**
- **An E2 sweep reports its failures AS THE HOST with the cause quoted, and calls them UNDECIDED rather than
  CLEARED** — an E2 reading binds to the host that measured it, a test that asks the live internet measures
  the internet, and a roster name absent from `go list std` ERRORS instead of failing as an oracle, so
  resolve all of them BEFORE sweeping. **A one-run oracle verdict on a networked, DOMAIN-JOINED host is wrong
  in EITHER direction and the ELAPSED TIMES are the tell** (1.05 s pass vs 21.2 s fail, same host, forty
  minutes apart): an E2 candidate owes a showing that its failure is NOT the environment, and one red is not
  that showing. **A `skip` is recorded as NO TEST RAN, never as a pass**, and a failing line carrying a
  domain, account, profile path or SID is NOT quoted.

<!-- DERIVATIONS (host qualification) — Phase 1 text, verbatim:
- **⚠ HOST QUALIFICATION for a network row: preflight `go test -count=1 net` BEFORE any net-family run
  (2026-09-02).** A host whose Go's OWN suite fails is disqualified as a bank host (a container
  answering `TestLookupCNAME` with the CDN CNAME and no IPv6; a WSL host failing that AND all 18
  `TestLookupNoSuchHost` leaves), and on an unqualified host the two arms of an A/B run different
  oracles — evidence, never a bank. A test asserting a live PUBLIC DNS record is UNIVERSAL drift once
  three independent resolvers agree (disclose it on the host-qualification ledger, not any one
  host's); and **a lane does not change a host's system configuration on its own initiative** — relay
  the commands to the owner, and RE-qualify afterwards (G-LAPTOP's WSL did, the same day: the 18
  leaves pass, wall 707 s → 35 s, and it is the fleet's Linux `net` bank host).
  ⚠ **The gate's criterion is the FAILING SET, and it is a LEDGER rather than a threshold** (stated
  2026-09-05): a NAMED, EVIDENCED universally-drifted leaf is tolerated with its evidence at the site;
  ANY other failing leaf ABORTS by name; the leaf names are printed either way. Go 1.23.12's
  `TestLookupCNAME` is such a leaf as of that date — `www.iana.org`'s CNAME now resolves through a CDN
  and three independent resolvers plus the host resolver agree — so it fails on every host until Go's
  own source changes and is NOT a re-qualification item. The criterion block is `.`-sourced and
  controlled in four arms against the LIVE block, never a retyped copy; and **a gate with a
  warn-and-continue switch is the lie-lever shape** — it does not get one.
  ⚠ **A prediction CARRIED from another lane's box predicts a HOST property as a property of the CODE**
  (2026-09-04): "the standing symlink-privilege trio" failed on one laptop and passed on the i7, so a
  baseline read 6 where the prediction said 9 — a miss in the FAVOURABLE direction, owned rather than
  absorbed. A count borrowed across boxes is re-measured on the box that will score it. Its neighbour:
  **a first-run failure that does not recur on a second identical run is a HOST ARTIFACT, named as
  such** (a COM-port semaphore timeout was one), never a disclosure. ⚠ And **a row whose counts differ
  between two hosts is read against the record's own HOST-CONDITIONAL ENTRY before either reading is
  called a regression** (2026-09-05): `os/exec` reading 86+2 on a single-file container host IS that
  entry — `TestExtraFiles` fires where fds 3..100 are held — against 87+1 on the fleet's bank host.

  ===== BATCH19 (24bfc8304 / c5e17217b / e9e56b657, merged 2026-09-12) — verbatim =====
  The authorization-scope half WIDENS the "never change a host's configuration on your own initiative"
  rule above (the same reasoning, one environment further out) and is now carried in it; the E2 sweep
  entries are new operational rules, stated visibly. The branch-retention twin below is mailbox/merge
  material kept here because it arrived in this hunk and evidence is never dropped.
  ⚠ **AN UNQUALIFIED AUTHORIZATION GIVEN IN ONE CONTEXT IS NOT SELF-EXTENDED TO A DISTINCT ENVIRONMENT ON
  THE SAME BOX** (2026-09-08, G): "you are authorized to install .NET 10" arrived in a Windows-blocked
  context, and a WSL root is a distinct environment — so the lane asked for the one-line confirmation
  rather than assuming. **Its branch twin: a pruned-by-ruling branch is KEPT when its content exists
  nowhere else** — the mailbox is transport, a SHA in a post is one `gc` from unfetchable, and the branch
  is what makes the finding checkable.
  ⚠ **AN E2 SWEEP REPORTS ITS FAILURES AS THE HOST, WITH THE CAUSE QUOTED, AND CALLS THEM UNDECIDED
  RATHER THAN CLEARED** (2026-09-08): 227 packages at go1.24.13 on windows/amd64, 225 pass; `os`
  fails 161 symlink leaves (the privilege measured read-only — not elevated, no privilege, no
  developer mode, and nothing changed silently) and `net` fails 27 DNS leaves from THREE host causes
  (a resolver mis-answering NXDOMAIN, a container's extra PTR name, the CDN-drifted CNAME). Both
  sets reproduced identically on a second run with a planted-difference control. Three rules: **an
  E2 reading binds to the host that measured it**; a roster name absent from `go list std` ERRORS,
  which is not an oracle that fails, so resolve all 227 BEFORE sweeping; and a test that asks the
  live internet measures the internet. The i7, which holds symlink creation, settled `os` the same
  hour (PASS 1096/0/24) — two hosts differing on exactly the axis in question.
  ⚠ **A HOST-QUALIFICATION PROBE RUN ON A SECOND HOST NARROWS THE CAUSE SET** (2026-09-08):
  R-LAPTOP's `net` at go1.24.13 read 26 failing leaves, deterministic (two full runs, set difference
  0 both ways, the compare positive-controlled by planting one leaf), collapsing to TWO roots — the
  ledgered CDN-drifted `TestLookupCNAME` and 25 `TestLookupNoSuchHost` leaves from a resolver that
  SYNTHESISES instead of answering NXDOMAIN. The other host's third cause (`TestLookupLocalPTR`)
  PASSES here, so it travels with the container, not with Windows or the release. Zero E2 members;
  **`net` stays UNDECIDED because no fleet host has a conforming resolver**, and changing one is the
  OWNER's system-settings call, recorded as an ask rather than done under a measurement. A figure's
  provenance offered by its author changes nothing about a same-box rule: **a number from ANOTHER
  box is not a baseline at all.**
  ⚠ **A ONE-RUN ORACLE VERDICT ON A NETWORKED, DOMAIN-JOINED HOST CAN BE WRONG IN EITHER DIRECTION**
  (2026-09-08): `os/user` PASSED at go1.24.13 (1.05 s) and FAILED at go1.23.12 (21.2 s) on ONE host
  forty minutes apart — one test on a domain-trust timeout, the ELAPSED TIMES the tell — so an E2
  candidate on such a host needs its failure shown NOT to be the environment, and a single red is
  not that showing (an unprompted companion sweep at the corpus pin read 200 of 204 sound, three
  host-attributable). The failing line was NOT quoted, because it carried the domain, the account, a
  profile path and a SID — the security order applied by the lane on its own. And `internal/pkgbits`
  reporting `skip` is recorded as **no-test-ran**, never as a pass.
-->

## Controls: a gate that has never been made to fail proves nothing

- **Before trusting a census or self-verify that reports zero, regress one site deliberately, confirm it names
  exactly that site, restore, verify byte-identical.** **A positive control neuters the MECHANISM — never a
  switch every arm resets — and a check no OTHER check subsumes**; its red must name the RIGHT assertion. **A
  control whose modification cannot be EXHIBITED proves nothing**: an unapplied patch reads as a PASSED
  control over unmodified code, so the load-bearing row must fail ALONE with neighbours green.
- **A control only tests the AXIS YOU VARIED** — vary every axis the PREDICATE reads, not the ones the change
  targets (`ISlice<T> : IArray<T>` makes every array test a trap unless it excludes slices). **Five more ways
  a control does not control what it names**: no CALLER's input shape; no arm THROUGH THE CALLER, hiding a
  defect in the predicate's ARGUMENT; varying the COMPARISON rather than the PROBE; varying something BESIDES
  what its prose claims; never reaching the dangerous path. **State a control's STRENGTH, not only its result.**
- **An arm asserts the reason it failed, or it is not a control — a red firing for the WRONG reason is worse
  than one that never fires, because it fires.** **The strongest adjudication is an arm built so it COULD
  confirm the challenger**, varying only the disputed element, one axis measured twice in opposite
  directions. **A positive control's target is chosen where ONLY the mechanism under test can produce the
  signal**; for a merge invariant that is often a REAL REF whose value genuinely differs, in MIRROR arms.
- **Isolate by the RELATION the defect travels on, not by textual mention, and run every member ALONE.**
  **Calibrate every new assertion against a known-good ref BEFORE adding it.** **A control's FLOOR is DERIVED
  from a text bound, never set from feel** — a mis-sized control fires on the POPULATION, not the instrument;
  resolve by a SECOND derivation and **name falsifiers BOTH ways.** **When one collector feeds two arms, an
  exemption belongs at the REASONING point, never the COLLECTION point**: **the fix for a false RED plants a
  false GREEN.**
- **Instrument mechanics that fail open**: an APPLIER asserts its own SITE COUNTS; a glyph-prefixed identifier
  guard compares WHOLE TOKENS (`Ꮡs.assign(` contains `s.assign(`); an emission FLOOR is derived, never
  guessed; a `[string]` PowerShell parameter coerces `$null` to `''` so a "no readable tail" refusal never
  triggers; `Mandatory [string[]]` rejects an empty ELEMENT past `[AllowEmptyCollection]`. **A BEFORE arm
  producing NO output makes every arm read DIFFERS** — control that it prints at all, THEN positive-control
  the arm that must go red; **a gate is ruled only after its BEFORE shows it can MOVE**, calibrated with the
  variable genuinely ABSENT (`TZ=` empty means UTC in Go).
- **Count a guard's DISCRIMINATING lines and ARMS**: arms where old and new behaviour COINCIDE are
  must-not-regress arms. **A body's own failure is earned by a control in a SEPARATE worktree at the same
  SHA**, and **a ruling's load-bearing assumption is MEASURED before any code exists, with a negative control
  that fires.** **When every synthetic axis comes back clean, the differentiator is INSIDE the row** — a
  `t.Logf` before the `Fatalf`, and a reduction is trusted only once its assertion string appears VERBATIM in
  the real row.
- **A vacuous TRUE inside an auto arm is a false green that reads as coverage** (an arm iterating an EMPTY
  `Fields` returned success while five assertions failed), so such arms are LOUD, never successful — but **an
  insurance arm is measured against the LEGAL values it must pass before it throws** (`struct{}` has zero
  fields, `[0]T` is legal Go: discriminate by SIZE). **A PASS THAT CANNOT FAIL IS NOT A PASS, and the honest
  answer names WHICH**: "this row structurally CANNOT see it" beats "no", recorded beside the CAPABILITY.
  **Lift a capability gate only where the HOST'S IMPLEMENTATION makes the assertion CAPABLE OF FAILING,
  never because the host DECLARES a member of that name** — red versus vacuously green is exactly what a
  name-keyed widening cannot see.
  **Displacement of a generated stub is proven by WRITE-EVIDENCE, never by absence**, and **a fixpoint needs
  its demotion STICKY**, with a pass-count/oscillation guard.
- **Three ways a guard is green without measuring anything.** It tests the COMMENT instead of the CONDITION —
  when every arm matches the prose, ask what ELSE satisfies the CODE. Its POPULATION IS ZERO, making "no row
  moves" its PREDICTION not its hedge: it ships with a TWO-ARMED control (FIRE on a planted instance, SILENT
  across every real producer) and board debt carrying the producer TABLE. Or its red control asserts a FAULT
  where the pre-fix behaviour is a REFUSED CALL. **A guard written alongside its fix shares the fix's model
  and can only confirm it** — only an instrument its author did not write caught either of two wrong models —
  and **the mtime-moved assertion separates a rebuilt binary from a leftover.**
- **A PLANT THAT FIRES PROVES THE GATE FIRES, NOT THAT IT DISCRIMINATES** — a refusal proves the gate caught
  the TOKEN only if an IDENTICALLY SHAPED plant carrying a HARMLESS token reads CLEAN; without that paired
  arm an over-fusing joiner refuses every shape and every "it fires" still reads PASS, so more shapes measure
  the same one thing. **Measure a pass's redundancy by BUILDING THE GATE WITHOUT IT and running the plants
  through both**: a pass that is the only catcher for a class owes a SELF-PROOF on every run, not more arms.
- **ONE ARM ASSERTING TWO HALVES PASSES WHEN EITHER HALF IS TRUE** — split a guard into "is the PREDICATE
  right" and "does the caller CONSULT it", negative controls firing DISJOINT arm sets; **a door proven
  CORRECT on one platform is not proven WIRED there**, so host-gate the wiring arms and say they are owed.
  **A guard line printing a BOOLEAN against a HARDCODED copy of the oracle's text is a vacuous pass in
  waiting**: reword the oracle and both sides print `false` and MATCH, equal for OPPOSITE reasons, and the
  golden banks it — print the RECOVERED VALUE, let the two sides compare strings, give each non-string
  outcome its own marker, and give an identity arm its DISCRIMINATING complement (different function,
  different pointer) or a constant-pointer implementation passes it.
- **A NARROWING COMMIT CENSUSES WHICH ASSERTIONS THE REMOVED LINES CARRIED, NOT ONLY WHICH ROWS** — narrowing
  a guard's rows for one defect silently deletes another defect's ONLY coverage, the silent-subtraction class
  inside a commit whose message is about something else, and the nearest-looking substitute exercises the
  OTHER band. **A census output that says "empty means no guard" is read for its RESULT, not its label.**

<!-- DERIVATIONS (controls, guards, vacuous greens) — Phase 1 text, verbatim:
- **A gate that has never been made to fail proves nothing.** Before trusting a census/self-verify that
  reports zero, regress one site deliberately, confirm it reports exactly that site, then fix and
  re-verify — and confirm the restore is byte-identical. The same principle as the positive controls
  the corpus loop uses: a green that cannot go red is not a measurement. Two refinements, both
  measured 2026-09-01: **a positive control must neuter a check no OTHER check subsumes** — under
  defense-in-depth, a broken control still reads green because a downstream check catches the
  regression it injected, so the control proves nothing about the check it targets (verify the
  control's red names the RIGHT assertion); and **a finding's PROSE is not its record** — a routed
  finding's description (a chip, a board line, a relayed diagnosis) is re-derived from the captured
  comparison/measurement record before anything is built on it, because the sweep-encoding chip's own
  description ("go pass vs C# fail") was wrong on the load-bearing detail and a rule built as
  described would have refused the very host it existed for.
  Two more, 2026-09-02. **A control only tests the AXIS YOU VARIED**: eight plausible, well-formed,
  entirely wrong findings passed BOTH of a census's controls because every repro varied box-ref and
  none varied RECEIVER KIND — so list the axes the predicate actually reads, and vary each one in a
  control. **And a control that does not use the CALLER's input shape is not a control for the
  caller**: a helper's self-test passed lines with content while every real call site passes BLANK
  lines, and a `Mandatory [string[]]` parameter rejects an empty ELEMENT (`[AllowEmptyCollection]`
  does not cover it), so the helper threw on every real invocation while the step's verdict and its
  artifact both looked normal — a guard that could never go green, caught only because a dispatch's
  annotations arrived from something else. Test with the exact type and shape the call sites pass.
  Three more, 2026-09-02. **Isolate by the RELATION the defect travels on, not by textual mention**:
  "classes that mention `flag`/`testing`" found four, "classes that drive `TestHost.Run`" five, and two
  single-cause fixes each passed a green full suite while an each-class-ALONE control still aborted — run
  every member alone. **A probe green on one binder path says nothing about the other**:
  `Delegate.CreateDelegate`'s static overload refuses a `DynamicMethod`, so a row came back
  infrastructure-error where the bound-path probe was green — exercise BOTH paths or NAME the one you
  skipped. And a committed disclosure quoted a 125/250/500 ms ladder its source does not contain (the
  rungs are 250/500/1000): re-derive a disclosure's mechanism from the line it cites, and post the RAW
  numbers beside any reading, since a measurement outlives the interpretation attached to it.
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
  ⚠ **Count a guard's DISCRIMINATING ARMS the same way** (2026-09-04): a control forcing the old
  behaviour reddened 3 of 7, not the 4 claimed, because two of the arms are cases where the old and
  the new behaviour COINCIDE — must-not-regress arms, not evidence about the mechanism.
  Four more control-design rules, 2026-09-02. **A ruling's load-bearing assumption is MEASURED before
  any code exists, with a negative control that fires** — libc `setegid` reaches an already-parked .NET
  thread (glibc's setxid broadcast) where the raw `setresgid` syscall does not, so the design records
  what a plain `DllImport` buys and what it does not, and the keystone's reason stays the structural
  one; a probe that changes PROCESS CREDENTIALS lands as a guard only with a privilege check and a LOUD
  unprivileged skip. **A probe proving a fix's SHAPE includes the case the NAIVE fix would OVER-CLAIM**
  — minting `Promoted=true` flipped the 35 target verdicts and made a VALUE store assert true where Go
  says false — and a generator that emits NOTHING for a type under the alternative flag is route #7's
  neighbour and gets a guard note. **A control that needs a 25-minute CNR to run is a control nobody
  runs** — give it a check-only switch — and its arms must reach EVERY step: two positive arms never
  reached a strip step, so a `[char]` `Replace` overload bug lived there until a negative arm (drift
  PLUS one unrelated hunk) threw. **And when every synthetic axis comes back clean, the differentiator
  is INSIDE the row**: the next measurement goes inside the failing test (a `t.Logf` before the
  `Fatalf`, one gated run on a host that has the package), not beside it — the same reason an
  IN-CONTEXT ratio understates an isolated one (2.7x against 7.5–11x, the surrounding loop diluting the
  cost), and the reason a reduction is trusted only once its assertion string appears VERBATIM in the
  real row's output.
  Two more, 2026-09-02, both guards that could never FIRE. **A `[string]`-typed PowerShell parameter
  coerces `$null` to `''`**, so a refusal written as "no readable tail" could not trigger on any input
  — an unreadable deadline tail would have read as clean. Untype the parameter and assert BOTH
  spellings, absent and empty. **And a BEFORE arm that produces NO output makes every arm read
  DIFFERS** — an instrument failure wearing a finding's clothes (the extracted copy was correctly
  throwing on a missing `Directory.Build.props`). A comparison that cannot report IDENTICAL on a
  known-identical arm proves nothing: control that the BEFORE arm prints at all, THEN positive-control
  the arm that must go red.
  **Seven more, 2026-09-06.** ⚠ **CALIBRATE EVERY NEW ASSERTION AGAINST A KNOWN-GOOD REF BEFORE ADDING
  IT** — a merge-invariant checker calibrated on four assertions against master had a fifth added
  uncalibrated and failed on master immediately, because it counted the board's own PROSE describing the
  very trap it looks for. Deleted rather than special-cased: the alternative is a checker that blocks every
  merge or, worse, gets suppressed and takes the sound assertions with it.
  ⚠ **THE POSITIVE CONTROL FOR A MERGE INVARIANT IS OFTEN A REAL REF WHOSE VALUE GENUINELY DIFFERS, NOT A
  SYNTHETIC VIOLATION** — a pin-count check read GREEN on master (105) and RED on a branch predating those
  pins (0), the hazard demonstrated on real data with no scratch commit, worktree or injected defect. **And
  prefer MIRROR arms**: a registry-completeness check controlled on master (all 8 owed entries LOST) and on
  each of two seats ALONE (its own entries OK, every other seat's LOST) — exact mirrors are what show the
  check reads each contributor's OWN contribution rather than some shared property both arms satisfy.
  **Look for the ref that already disagrees before manufacturing one.**
  ⚠ **A POSITIVE CONTROL'S TARGET IS CHOSEN WHERE ONLY THE MECHANISM UNDER TEST CAN PRODUCE THE
  SIGNAL** (2026-09-08): one package has ZERO subdirectories in the pinned GOROOT, so a PLANTED child
  project referenced from its own project file makes that namespace something the CORPUS FOLD ALONE
  can see — one input, one planted corpus, two converters, base emitting the bare alias and the cut
  emitting the prefixed one — **and that is what turns a zero on the real corpus into a measurement.**
  Building it CORRECTED a claim one step from posting ("the fix is structurally unreachable at this
  pin, so its zero is vacuous"): TRUE of the package whose loader closure already holds its child,
  FALSE as a general statement, since the fold fires wherever the CORPUS closure EXCEEDS the LOADER's.
  A run already under way with the weaker control was KILLED — by parentage, survivors censused —
  rather than allowed to produce a provisional reading its author had said would not bank.
  ⚠ **AN ARM ASSERTS THE REASON IT FAILED, OR IT IS NOT A CONTROL — and a red that goes red for the WRONG
  reason is worse than a control that fails to fire, because it fires.** Three self-test arms targeting SHA
  validation died on an unrelated branch-existence check BEFORE reaching it, so "it refused" would have
  read as proof with that check never executing, and a suite asserting `exit != 0` prints three greens.
  **An arm that cannot reach its target is a SCRIPT-ORDERING defect wearing a test's clothes** — the fix
  belongs in the script (input validation before the network read), not in the test.
  ⚠ **A CONTROL THAT VARIES THE COMPARISON RATHER THAN THE PROBE TESTS THE WRONG AXIS.** A toolchain-pin
  guard was controlled by setting the PIN to an impossible value and watching it refuse: that exercises the
  equality test and **leaves the axis the guard exists to measure held fixed**, so it shipped able to pass
  in its own motivating case. **Name what the guard is FOR, then vary THAT** — and **STATE A CONTROL'S
  STRENGTH, NOT ONLY ITS RESULT** (the redone version was a probe-level measurement plus a source read,
  **not an end-to-end run**; the sentence saying which is what stops the next reader quoting it as one).
  ⚠ **A CONTROL THAT READS LIKE ONE AND ISN'T — and its author is the last person who will notice.** A
  disclosure cited *"a plainly dead byte slice, sharing none of the string machinery, reads COLLECTED"*
  as a representation control. It is not one: that arm allocates inside a callee that has **RETURNED**
  before the collection, so it varies the FRAME as well as the wrapper and can isolate neither
  (2026-09-06). **The existing instrument could not have answered the question it appeared to answer**,
  and the author would have defended the claim with evidence that did not reach it. **When a control's
  prose describes what it varies, check what it varies BESIDES.** ⚠ **And the strongest adjudication is
  an arm built so it COULD confirm the challenger**: faced with a lens claiming a retention was caused
  by our own `@string` representation, the row's owner built an arm with the original's frame shape
  EXACTLY, varying ONLY the wrapper — a bare byte slice where the original had `@string` — and it read
  RETAINED, so the representation is not the cause; paired with the overwritten-slot arm reading
  COLLECTED, the discriminator is the frame slot, **measured twice in opposite directions on one axis.**
  The prediction went on record first with the caveat that makes it credible: *"I am the author of the
  sentence under test, so this prediction is the one to distrust."*
  ⚠ **A GUARD CAN TEST EVERYTHING EXCEPT THE OPERATION IT EXISTS TO PROTECT.** A safe-push composition's
  eight-arm self-test reached only `--dry-run`: **no arm exercised an actual push**, so every arm fired, the
  suite read clean, and the dangerous path was untouched. **A cheaper guard that omits the dangerous path is
  not a cheaper guard; it is a different and weaker one wearing the same name.**
  ⚠ **WHEN ONE COLLECTOR FEEDS TWO ARMS, AN EXEMPTION BELONGS AT THE REASONING POINT, NEVER THE COLLECTION
  POINT.** A `continue` in the collector is the cheap fix for a false positive — and it also empties that
  population out of the *other* arm, so **the fix for a false RED plants a false GREEN: the
  silent-subtraction class arriving through the remedy.** Done correctly the collector's population is
  unchanged and a separate set is consulted only by the arm that needs the exemption, pinned by an assertion
  that fails on the collector-level implementation and passes on this one.
  ⚠ **A control's FLOOR is DERIVED from a text bound BEFORE it is committed to, never set from feel**
  (2026-09-04): a mis-sized control fires on the POPULATION rather than on the instrument, and the
  number alone cannot tell the reader which — the resolution is a SECOND derivation (the instrument's
  arm against a build-tag-blind text upper bound) plus the known-count guard sources. Corollary:
  **name falsifiers in BOTH directions** — the two that fired on that census resolved opposite ways,
  one on the population and one on the prediction.
  ⚠ **Three more, 2026-09-05.** **A cost canary's WALL that sits in a band ~100 s above the same
  box's reading hours earlier is ATTRIBUTED BEFORE IT IS QUOTED**, and the attribution instrument is
  a FOURTH ARM that re-runs an OLDER known tip TODAY: cut ×3, its own base, and the earlier tip all
  landing in one band means the move is the HOST'S DAY (load, disk, session age), not the code — **a
  canary compared against a number taken on a different day compares two hosts.** **A negative
  control whose red lands at a DIFFERENT ARM than the defect's symptom STATES the offset rather than
  smoothing it**: a count assertion firing one step before the poll that would have hung still names
  its own assertion, but the reader must know it did not reproduce the symptom's SHAPE — say which
  arm went red and why it precedes the symptom. And **a CAS on a managed slot holding boxes compares
  box IDENTITY** — the comparand is the exact INSTANCE observed in the same iteration — guarded by a
  racing-push arm and a distinct-box-same-address arm, never left unstated as "compared by address".
  ⚠ **The publish shape that CAS belongs to, and the control that proves it** (2026-09-05, the
  field-view cache): **a lock-free publish by compare-exchange reads the head ONCE, scans THAT exact
  list, and CASes against THAT same head** — scanning for a racer's entry only AFTER a failed CAS
  admits a racer whose publish lands between the caller's miss and its head read, and the result is
  two views of one field. The guard's concurrency arm (`Barrier` + `Parallel.For`, 50 × 16) was RED on
  its first run in BOTH configurations and it was exactly that defect. Its control half: **a positive
  control neuters the MECHANISM, never a switch every arm resets** — neutering the Disabled default was
  measured VACUOUS first, while the mechanism neuter read 5 RED / 2 GREEN with each red naming its own
  assertion.
  **⚠ A CONTROL WHOSE MODIFICATION CANNOT BE EXHIBITED PROVES NOTHING** (2026-09-04): an LF-anchored
  patch against CRLF source did not apply, the script's own assertion fired, and the run that followed
  read as a PASSED control while testing unmodified code — the census-instrument-that-never-compiled-in
  species, met inside a guard. The corrective is structural, not attentional: **the load-bearing row
  must fail ALONE when its mechanism is removed while every neighbour stays green** (so no other check
  subsumes it), and the restore must be byte-identical. Three mechanical siblings: an APPLIER gated by
  LINE RANGES silently applied 2 of 3 sentinels when the emission shifted — an applier asserts its own
  SITE COUNTS; a rewrite gated on registry MEMBERSHIP assumed every registered method takes the box
  (true of the members it was written from — an assumption, not a derivation), so a gate asks the
  question the emission depends on ("is a receiver box in scope"), and a guard over glyph-prefixed
  identifiers compares WHOLE TOKENS (`Ꮡs.assign(` contains `s.assign(`); an emission FLOOR that is a
  GUESS rejects healthy emissions — derive the population. ⚠ And **a predicted CNR drift is SPENT
  before the run** (re-baseline the predicted project first, then require byte-identical) — a stronger
  gate than predicting the drift and then explaining a dirty verdict; while **a count matching a
  documented failure count is not a matching CAUSE** until the exception itself is read (three
  failures, all in one fixture, all wanting a Windows symlink privilege: count-matched AND
  identity-matched).
  **⚠ VACUOUS ARMS, INSURANCE ARMS AND FIXPOINTS.** A **vacuous TRUE inside an auto arm** is a false
  green that reads as coverage: an arm iterating a descriptor's `Fields` (EMPTY on a synthesized
  descriptor) returned success, so a struct was "assigned to registers" in zero steps and five
  assertions failed from one silent yes. The interim remedy is that **empty-means-cannot-see arms
  become LOUD, never successful** — but ⚠ **an insurance arm is measured against the LEGAL values it
  must pass BEFORE it is written to throw**: `struct{}` legitimately has zero fields and `[0]T` is
  legal Go, so the discriminator is SIZE (nonzero size with zero fields is unseeable; 0/0 is
  `struct{}`), and where no discriminator exists **a half-rule that says why its other half is absent
  beats a whole one that throws on `struct{}`.** ⚠ Before writing a claim about what a model CANNOT
  express, **read the DECLARATION of the field the claim concerns** — a model was asserted unable to
  distinguish unknown from zero while its own declaration said `null = unknown`, and the
  self-correction came from a comment rather than a probe, meaning the inference was the weak link.
  ⚠ **A row's worth is stated at its REACH**: a function reachable only from `export_test` exercises
  its arm only through one test and leaves the sibling arm LATENT, so fixing those rows makes the row
  honest and is not a production behaviour — the record says so in those words rather than letting
  test-only rows borrow a production finding's justification. ⚠ And a **fixpoint over a two-step
  admit/demote rule needs the demotion to be STICKY or the iteration is not monotone**: one
  classification fixpoint OSCILLATED for 18 passes (admit, demote, re-admit) and was caught by the
  monotonicity guard the ruling had asked for. Put a pass-count/oscillation guard in every fixpoint and
  read a firing as a bug, not slow convergence — and note that the prediction ("2–3 passes") missed
  because the cascade was filtered one LAYER earlier than modelled: **predictions name the layer they
  model.** ⚠ Finally, a prediction made from a test's ERROR STRING without reading what FEEDS the
  assertion aimed a check at a branch that cannot fire on any platform, and the pass that arrived was
  VACUOUS — every assertion iterating an empty set. **A vacuous pass is never a match**; where the
  pipeline cannot make the test fail honestly, the per-declaration capability GATE already carried in
  the comparison record lists it with its capability and its lifting condition. (Displacement of a
  generated stub is proven by WRITE-EVIDENCE — the fresh generator output lacks it, the old stub file
  is stale — never by absence.) ⚠ **A PASS THAT CANNOT FAIL IS NOT A PASS, and the honest answer
  names WHICH** (2026-09-06): asked whether a silent degradation showed up in a row, the honest answer
  was not "no" but "this row structurally CANNOT see it" — the two tests that exercise the call are
  capability-GATED, absent from both verdict maps and never compared, so the passing neighbours never
  reach it. Say which, because the next reader will see the green rows and conclude coverage, and
  **record it beside the CAPABILITY, not beside the row.**
  ⚠ **THREE WAYS A GUARD IS GREEN WITHOUT MEASURING ANYTHING, all 2026-09-06.** **A guard can test the
  COMMENT instead of the CONDITION**: all three arms of a pointer-token guard built their box as a heap
  box of a POINTER-to-struct — a reference-bearing POINTEE, which is what the arm's comment describes —
  while the arm's actual condition is "no pinnable storage", which is ALSO true of every field or
  element reference rooted in a reference-bearing CONTAINER whose pointee is reference-FREE. That
  second class was unguarded, and it is the class that regressed every Windows dial. **When a guard's
  arms all match the prose, ask what ELSE satisfies the CODE.** **A guard whose POPULATION IS ZERO has
  "no row moves" as its PREDICTION, not its hedge** — and it is exactly the guard that can be green
  because it is BROKEN: its payoff is a future defect's failure mode, never a moving row, so its
  acceptance criterion is stated in the only direction it can be measured (a row that DID move
  falsifies the population census), and it ships with a TWO-ARMED control — it must FIRE on a planted
  instance of the shape and stay SILENT across every real producer — or it asserts the population
  instead of measuring it. Board debt for such a guard carries the producer TABLE and the control
  requirement, not just the title, so a later lane inherits the measurement rather than trusting it.
  And **when the PRE-FIX behaviour is a REFUSED CALL rather than a crash, a red control asserting a
  fault asserts something FALSE**: under the token arm three of four boxes are order tokens, so the
  control asserts a nil error and a byte count instead. **Put that reasoning in the GUARD's header, not
  only in the design record** — the next reader meets the guard.
  ⚠ **A GUARD WRITTEN ALONGSIDE ITS FIX SHARES THE FIX'S MODEL AND CAN ONLY CONFIRM IT** (2026-09-08):
  a fold cut's first attempt keyed on the CONVERTED package's own project file — which a behavioral
  project does not have — and its fixture encoded THE SAME WRONG MODEL, so the guard PASSED while CNR
  still read the eight; the second attempt globbed every project file and admitted the TEST-project
  SIBLING, a far wider closure that moved hundreds of files, and NONE of the four guard arms had put
  such a sibling on disk. **Only CNR — an instrument the author did not write and could not align —
  caught either**, and both defects now carry SEPARATELY neutered arms. Beside it: a `go build -o`
  from the wrong cwd fails, and the alias read from the STALE binary the previous broken run left was
  nearly banked as a pass — **the mtime-moved assertion is what separates a rebuilt binary from a
  leftover.**

  ===== BATCH19 (24bfc8304 / c5e17217b / e9e56b657, merged 2026-09-12) — verbatim =====
  The first four are NEW VISIBLE RULES above (the paired plant, the pass-redundancy build, the split
  conjunctive guard, the boolean-against-hardcoded-oracle line, the narrowing census); the capability-gate
  entry AMENDS the "A PASS THAT CANNOT FAIL IS NOT A PASS" rule with the criterion for LIFTING a gate.
  ⚠ **A REFUSAL PROVES A GATE CAUGHT THE TOKEN ONLY IF AN IDENTICALLY SHAPED PLANT CARRYING A HARMLESS
  TOKEN READS CLEAN** (2026-09-08, i9, the split-token gate day): four lanes probed their gates with
  eight, six, seven and six shapes and every probe measured the same ONE thing — that the plant FIRES —
  and none measured WHY; a joiner that fused text too aggressively would refuse everything split across
  lines and every "it fires" would still read PASS. **The PAIRED control (protected token → REFUSED;
  harmless token in the same split shape → CLEAN) is what makes a shape probe a measurement of the
  DETECTOR rather than of the joiner.** Companions from the same day: a two-line joiner passes a
  THREE-WAY split while refusing the two-line case, and the blank-line paragraph break is the commonest
  break in prose and none of four lanes had named it until one did.
  ⚠ **A PASS ADDED TO A GATE FOR ONE CLASS IS A SINGLE POINT OF FAILURE FOR THAT CLASS, AND THE WAY TO
  SEE IT IS TO BUILD THE GATE WITHOUT THE PASS AND RUN THE PLANTS THROUGH BOTH** (2026-09-08, i9/R): a
  line-pass-only variant — the join block removed, the variant ASSERTED to differ from the subject before
  any verdict — took eleven plants: the split identifier in four shapes was caught by the JOINED pass
  ALONE, while wrapped PATHS were caught both ways, because a real path carries several triggering
  substrings and one line break separates only one pair. So the redundancy offered as the difference
  between two gates was real for paths and ABSENT for the class the join exists for: had the join broken
  silently, as a sibling's had that same morning, four shapes would have passed clean. **The remedy is a
  SELF-PROOF of the join — a known split plant that must refuse on every run — rather than more arms.**
  ⚠ **A CHOKE POINT IS DERIVED TWO WAYS BEFORE A DOOR IS PLACED AT IT, AND THE DISPATCHED SITE IS
  CORRECTED RATHER THAN OBEYED** (2026-09-08): a token door dispatched at one of EIGHT entry points
  was placed one frame lower at the private dispatcher — exactly ONE call site in the file, through
  which every native invocation routes — so one loop covers all eight entries and the call TARGET is
  checked too. The guard SPLITS into "is the PREDICATE right" (9 arms) and "does the trampoline
  CONSULT it" (4 arms), because one arm asserting both PASSES when either half is true, with four
  negative controls firing DISJOINT arm sets. **And a door proven CORRECT on one platform is not
  proven WIRED there**: reaching the dispatcher runs the windows module initializer, so the four
  wiring arms are host-gated and owed on Windows.
  ⚠ **A STDOUT-COMPARED GUARD LINE THAT PRINTS A BOOLEAN AGAINST A HARDCODED COPY OF THE ORACLE'S
  TEXT IS A VACUOUS PASS IN WAITING** (2026-09-08): if the oracle rewords the panic, the Go side
  prints `false`; a body raising anything else prints `false` too; the two lines MATCH and the
  golden banks the vacuous `false` as the contract — two arms equal for OPPOSITE reasons. **Print
  the RECOVERED VALUE and let the two sides compare strings**, so the file carries no assumption
  about the pin's wording, and give the non-string outcomes (failed to refuse; refused with a
  managed exception) DISTINCT markers, since they are different defects. And an identity arm (same
  function, same pointer) needs its DISCRIMINATING complement (different function, different
  pointer), or a constant-pointer implementation passes it. Both found by reading the guard against
  the tree's own rules before any run.
  ⚠ **THE GATE FOR LISTING A TEST CAPABILITY IS "DOES THE HOST'S IMPLEMENTATION MAKE THE ASSERTION
  CAPABLE OF FAILING", NEVER "DOES THE HOST DECLARE A MEMBER OF THAT NAME"** (2026-09-08): censused
  over the 204 committed proof pages, the allow-list gates 44 declarations (testing 38, os 4,
  net/http 1, math/big 1) and **ZERO are liftable** — 19 reach unexported internals of a
  hand-written host, 17 are free-text capability reasons, and the 8 exported-member candidates split
  into 2 that would not COMPILE and 6 that would pass VACUOUSLY (a parallel-benchmark body never
  invoked, its iterator always false, a sub-benchmark returning true without running the body, a
  calibration returning at its first line). The lane's own prediction that one row would go RED was
  refuted by Go's source: **red versus vacuously green is exactly the distinction a name-keyed
  widening cannot see.**
  ⚠ **NARROWING A GUARD'S ROWS FOR ONE DEFECT CAN SILENTLY DELETE ANOTHER DEFECT'S ONLY COVERAGE — the
  silent-subtraction class inside a commit whose message is about something else** (2026-09-08, G): a
  narrowing of `SwitchPointerSentinelCase`'s A rows to compile shape removed the two lines that were
  another defect's ONLY assertion (a `uintptr` declared with a SIGNED-band literal, printed through the
  element-address helpers), while the commit that fixed that defect had done so through source that
  already existed and added no row. The nearest-looking substitute exercises the UNSIGNED-parse band,
  which always carried the rule — a plausible neighbour covering the WRONG band. Two rules: **a narrowing
  commit censuses which ASSERTIONS the removed lines carried, not only which ROWS**; and **a census
  output that says "empty means no guard" is read for its RESULT, not its label** (the lane's own line
  said empty while the output was not).
-->

## Arms and attribution

- **An attribution is a ONE-AXIS pair, and the dispatch says which arm carries the change BEFORE either arm
  runs.** **A variant table names what each variant REMOVES and the attribution line is DERIVED from that
  column.** **A gap between two arms of the SAME code with the SAME attribute is a CONFOUND TELL, never a
  boundary cost**: read `DebuggableAttribute.IsJITOptimizerDisabled` INSIDE the probe process, and inlining
  from `DOTNET_JitDisasmSummary=1` (an inlined callee is absent) — `DOTNET_JitPrintInlinedMethods` prints
  nothing there. **A hand-transcribed proxy is diffed against the emission before its number is quoted.**
- **Name what each arm HOLDS, and when an arm is "the tree before X" say which OTHER commits it also lacks**:
  "pre-existing at MY BASE" is not "pre-existing at MASTER". **VERIFY EACH ARM BY ANCESTRY (`git merge-base
  --is-ancestor <accused> <arm>`), printed per arm, never by the merge order you intended**, and **put the
  check IN the probe script** — a banked lesson that is not MECHANISED is paid for again one rung later. **A
  result that does not fit the mechanism is a reason to re-examine the ARM**, and **an AGREEING confounded
  arm is the most dangerous kind**: score it VOID.
- **Decompose the operation before attributing its failure** — an arm measuring a SEQUENCE attributes the
  whole sequence's failure to its first step. **An acceptance criterion is derived from a measured BASELINE,
  never from the shape of the failure**, and **a criterion derived on ONE HOST is that host's until a second
  host reads it: print the host.** **Two failures narrowing to one commit are not necessarily the same
  failure** — a common CAUSE is not a common DEATH.
- **Before ruling, name the axis every reading SHARES and ask what sits on the axis nobody varied**, and
  **rule at the speed of the EVIDENCE, not of the conversation**: every refutation came from somebody
  BUILDING the thing. **An attribution read out of the SOURCE beats a before/after with a confound in it.**
  **Where a run STOPS is a property of the RUN whenever the death is asynchronous**; the only honest movement
  signal is the MOVED SET. **A COUNT that matches its prediction is not a SET that matches**, and **a
  falsified EXPLANATION does not falsify the MEASUREMENT it was invented for.**
- **A defect REPORT is measured at the REPORTING BRANCH'S OWN BASE converter as well as at master before
  anything is built for it**: `CS1010` beside `CS1003` is the signature of a TEXT-CORRUPTED file, never of an
  emission decision. **The differential control has an ARITHMETIC form** — under standing corpus drift an
  ABSOLUTE byte-identity leg fails BY CONSTRUCTION, and the cut is exonerated when `D(master, emission) =
  D(cut, emission) + the cut's own lines` closes FILE BY FILE. **Each arm runs its OWN binary, stated**: the
  CUT's converter against the BASE tree emits bodiless placeholders, a red reading as "the baseline is broken".
- **A canary row NOT MEASURABLE on the box by standing ruling is not an A/B instrument there**; **a two-point
  comparison states its WITHIN-ARM spread before its direction**; **a cost canary's wall is ATTRIBUTED by a
  FOURTH ARM re-running an older known tip TODAY.** **A control for a suite's row keeps the SUITE's
  configuration**, and **a suite RED at master names the FIVE-MINUTE CONTROL — the suite without its file —
  before attributing.** **A control arm measuring a DIFFERENCE over a WINDOW can pass for the wrong reason**:
  assert the RELATION at a MOMENT, and read WHICH arms went red and whether each names its OWN assertion.
- **Read the failing assertion's PRINTED OPERANDS before classifying a row** — a bill read off the stack can
  be exactly the wrong way round. **Ask of every acceptance: what does the system do INSTEAD when the seam
  fails? If it "falls back silently", the probe needs an arm with NO fallback** — a probe whose only
  distinguisher is "does it crash" reads BEFORE as a PASS; twin: **reaching a consumer defect is the producer
  working.** **A silent-success failure in the CODE is the same species as a vacuous green in a gate**:
  `net.LookupHost` on darwin returned no addresses, no error, exit 0.
- **One-arm attribution from the intermediate that already exists**: master GREEN, master+X RED, X-absent
  seats ruled OUT by ancestry PRINTED PER BRANCH, survivors RE-GATED at the tree that lands; **the transfer
  argument for legs NOT re-run is a FILE-SET check asserted in the land script.** **A falsification is BANKED
  AS A LIVE ASSERTION and a finding SURVIVES its mechanism.** **A finding is rooted to a CLASS by repetition
  with variation, not by hypothesis.** **To tell a root from a cascade, run the suspected DOWNSTREAM member
  alone**: a cascade shows as a fixed ORDER rather than a set, so count the ROOTS before sizing the number.

<!-- DERIVATIONS (arms, attribution, roots vs cascades) — Phase 1 text, verbatim:
  ⚠ **TO TELL A ROOT FROM A CASCADE, RUN THE SUSPECTED DOWNSTREAM MEMBER ALONE — not just the
  suspected root** (2026-09-06). A 38-verdict cluster looked like one failing test leaving a resource
  open with every later test reporting `already in use`; a single-test gated run of the DOWNSTREAM
  test failed on the SAME unimplemented primitive as the head test, so the `already in use` text was
  a symptom of the START PATH throwing, not of a predecessor's leak. One deep root, not a cascade —
  which is better news, because it makes the row answerable by one piece of work and it relocates
  that work out of the package and into the runtime. Two gated runs at ~90 s each replaced a
  plausible story. **A cascade shows as a fixed ORDER rather than a set: count the ROOTS before
  sizing the number.**
  ⚠ **A BILL LINE READ FROM THE TRACE CAN BE THE WRONG WAY ROUND** (2026-09-05): "every
  `StructField.Offset` reads 0 — offsets are never synthesized" was billed off a stack, while the
  failing line's own printed operands (`mismatched offsets: 8 0`, i.e. `f.Offset` then `offs`) said
  the SYNTHESIZED offsets were Go's and the TEST's expectation — raw address arithmetic over managed
  storage — was the zero. **Read the failing assertion's PRINTED OPERANDS before classifying a row**;
  a row re-billed by its own line moves classes without a cut.
  ⚠ **AN ARM THAT CALLS THE PREDICATE CANNOT SEE A DEFECT IN THE PREDICATE'S ARGUMENT** (2026-09-08):
  a registration check was CORRECT and was handed the WRONG OBJECT — the container a lifetime key
  resolves a field reference to, where Go validates the interface's DYNAMIC type, the rule stated
  twelve lines below in the same file — so the two arms that exercise the predicate DIRECTLY were
  green while the row refused at iteration 0, and the arm that sees it calls the ENTRY POINT the row
  calls. **Every predicate arm set carries at least one arm THROUGH THE CALLER.** And the denial that
  preceded the fix, from four correct reads of the OBJECT and none of the CALL SITE, was corrected in
  public BY SHA the hour it was measured false.
  ⚠ **A CLAIM OF WHAT A CHANGE BUYS IS CHECKED AGAINST THE SIBLING CASES THAT ALSO REACH THE SHAPE**
  (2026-09-08): "the fix for the box family" OVERSTATED one case's contribution, because a sibling
  case reaches every box-family arm through a base-chain walk, so under the neuter exactly ONE arm
  goes red — that case's UNIQUE contribution — where the author's own comment predicted two. The table
  re-derived AT THE CODE reproduced the coordinator's measurement, and the wrong neuter comment was
  replaced by a COMMENT-ONLY commit **proven so by a byte-identical hash after whole-line comments
  were stripped from both sides, the stripper positive-controlled.**
  ⚠ **ASK OF EVERY ACCEPTANCE: WHAT DOES THE SYSTEM DO *INSTEAD* WHEN THE SEAM FAILS? IF THE ANSWER IS
  "FALLS BACK SILENTLY", THE PROBE NEEDS AN ARM WITH NO FALLBACK.** A port-lookup arm was masked by an
  `/etc/services` fallback exactly as predicted, and the fallback-free arm was the only reason the BEFORE
  failure was visible at all — *"a probe whose only distinguisher had been 'does it crash' would have read
  BEFORE as a PASS"*. Its positive twin, **"REACHING A CONSUMER DEFECT IS THE PRODUCER WORKING"**: you
  cannot die one package over without a chain to walk, so a NEW failure further along is evidence FOR the
  fix. And **a SILENT-SUCCESS FAILURE IN THE CODE is the same species as a vacuous green in a gate** —
  before one increment `net.LookupHost` on darwin returned **no addresses, no error, exit 0**, the SHAPE of
  success with none of the content, so anything reporting darwin name resolution as working was reporting
  the fallback. **A gate that cannot fail; a FUNCTION that cannot fail — record it where the consuming lane
  will stand.**
  ⚠ And the dispatch-side form of the agreeing-confounded-arm rule already beside this one: **say which arm
  carries the change, in the dispatch, BEFORE either arm runs** — an acceptance dispatch returned an
  IDENTICAL before/after pair because the probe was the constant and the increment never varied, so the
  one-axis A/B had no axis at all.
  **⚠ SIX MORE, 2026-09-04, four of them retractions.** **The neuter rule met from the ARM's own
  side**: a wiring arm asserting "the cache is empty after `runtime.GC()`" stayed GREEN with the
  synchronous clear DELETED, because `GC()`'s own tail drains finalizers and the registry's sentinel
  clears the cache by the ASYNCHRONOUS route — emptiness cannot discriminate the two paths, so the arm
  guarded neither, and only making it fail exposed that. The discriminating property is WHERE and WHEN
  the clear ran (caller thread at the head of `GC()` versus the finalizer thread; the gen2 count at
  the first clear), timing-free — and the refuted control also taught something TRUE about the line it
  guards: it is a GUARANTEE of Go's contract (`clearpools` at `gcStart`, synchronous), not the only
  route to the outcome. Beside it, **a lane measuring a suite RED at master names the FIVE-MINUTE
  CONTROL (the suite without its file) before attributing**, and checks whether a SEATED cut already
  owns the reds. **A control arm that measures a DIFFERENCE over a WINDOW can pass for the wrong
  reason**: a before/after `NumGoroutine` delta read GREEN against a neutered predicate because a
  sibling test's goroutine exited inside the window and cancelled the +1 — assert the RELATION at a
  MOMENT (the count while the goroutine is registered, against the total that sees it); and when a
  control goes red, read WHICH arms went red and whether each names its OWN assertion, because "the
  control failed" is not the reading and "these arms failed on these assertions" is. **A COUNT that
  matches its prediction is not a SET that matches**: 19 admitted declarations equalled the predicted
  19 while two MEMBERS differed — one in that should have been out, one out that should have been
  in — and only a build failure on the first exposed the cancellation, so **a prediction names
  MEMBERS and its scorecard compares the SET**, and a "to the digit" claim on a count is retracted the
  moment the membership is read. **A falsified EXPLANATION does not falsify the MEASUREMENT it was
  invented for**: a delta measured at 510.1 B was explained by a side table, the explanation was
  refuted by segmentation, and BOTH the lane and the coordinator then retired the NUMBER with it —
  while the number was right (512 = 384 + 128, two surviving boxes un-escaped). The measurement and
  the story are independent claims: when a mechanism is refuted, re-derive what the measurement
  OBLIGES and leave the number standing as an unexplained residue (a ladder to which no story was
  attached — the count, 17/11/10 — survived every revision in that arc). **A defect REPORT is measured
  at the REPORTING BRANCH'S OWN BASE converter as well as at master before anything is built for it**:
  a routed emission-mangling chip reproduced at NEITHER (six conversions, byte-identical), both of its
  diagnoses fell on rows written for each, and the standing population — thousands of compiling
  formats of the same shape — had said so at one grep; `CS1010` beside `CS1003` is the signature of a
  TEXT-CORRUPTED file (the r41 overlap family), never of an emission decision, an elimination
  comparing two calls that differ by file POSITION and line FORM has isolated nothing, and the
  negative result banks as a GUARD pinning the emitted form plus a dated record, never as a fix that
  cannot be made to fail. And **the differential control has an ARITHMETIC form**: under standing
  corpus drift an ABSOLUTE byte-identity leg (committed cut against fresh emission) fails BY
  CONSTRUCTION, and the cut is exonerated when `D(master, emission) = D(cut, emission) + the cut's own
  lines` closes FILE BY FILE, the residue being the standing forced-init/relocation debt named per
  file — a chain's FAIL flag for such a leg is READ with that meaning rather than re-run green, and
  the result post states which FORM each leg took.
  **⚠ ATTRIBUTION AND ARMS, five rules from 2026-09-03.** **A prediction is stated in a currency the
  predictor has MEASURED**: a row-level triple was arithmetic on another lane's accounting and the
  measured decomposition reached neither triple — the MOVED SET (FIXED/BROKEN derived from both
  records under one accounting) is the verdict. **Each arm runs its OWN binary, stated** — the mirror
  of the re-converting-sweep rule: the CUT's converter run against the BASE tree emits placeholders
  with no bodies, a guaranteed red that reads as "the baseline is broken". **A canary row that is NOT
  MEASURABLE on the box by standing ruling is not an A/B instrument there** — red on both arms with
  two different signatures under load says nothing about the cut. **A two-point comparison states its
  WITHIN-ARM spread before it states a direction** (a spread of 90 s and 154 s inside one arm exceeded
  the 145 s arm-versus-control gap, so "faster on both readings" was variance read as merit; the
  verdict was unchanged and only the wording was over-read, which is the cheap kind of correction).
  And **a control for a suite's row keeps the SUITE's configuration** — a stack-walk hand-own's motive
  was mis-attributed by BOTH the lane ("identical at both tierings" — tiering was never the axis) and
  the coordinator ("the call shape, not a lambda" — the shape was the same) while the stack trace named
  the real axis: list the axes (configuration, tiering, shape, tree) and vary each.
  ⚠ **AN ATTRIBUTION READ OUT OF THE SOURCE BEATS A BEFORE/AFTER WITH A CONFOUND IN IT.** A lane
  correctly declined to attribute a +49-verdict move to one seat because 53 commits sat between the
  arms; the attribution was then found in `visitFuncDecl.go` — a converter comment naming that very row
  and the five symbols its push targeted — which is EXACT where the measurement could only be suggestive
  (2026-09-07). **When an A/B is confounded, look for the change that STATES ITS OWN INTENT before
  designing a cleaner A/B.** ⚠ And **where a run STOPS is a property of the RUN, not of the tree,
  whenever the death is triggered ASYNCHRONOUSLY — a moved stop-index is not movement.** Two arms
  reported max reported index 100 and 101, adjacent names, +1 in the flattering direction, and the row's
  own author DECLINED to claim it because the killing goroutine is async and the stop point is a race.
  **The only honest movement signal is the MOVED SET** — both were EMPTY, so the fix changed the death
  MODE and nothing else.
  ⚠ **An AGREEING confounded arm is the most dangerous kind, because nothing about the result invites
  the question** (2026-09-06): an arm varied the callee axis while the DOMINANT axis — the caller's
  slot, already proven to pin — stayed fixed, so it could not have informed the question either way,
  and it AGREED with its author's prediction. Scored VOID rather than as a hit, by the author, citing
  their own rule from one day earlier that a control only tests the axis you varied.
  ⚠ **AN ARM ADDED TO TEST ONE'S OWN MECHANISM CAN REFUTE IT AND CALIBRATE THE INSTRUMENT IN ONE RUN**
  (2026-09-08): a ready explanation for a door cost — that one arm's constants cannot be encoded as
  immediates — was put ON THE BENCH as a third arm spelled to avoid both, and read several times WORSE
  than either door in every configuration, refuting the mechanism. **And that third arm's STABILITY —
  the same sign and magnitude in every order and both tiering modes — is what a REAL difference looks
  like**, against which the measured pair (sign flipping with tiering, magnitude moving when unrelated
  arms are added, inside the noise floor in nearly half the runs) reads as AT OR BELOW RESOLUTION,
  reported with its weak lean rather than explained away. Two companions: **a per-TEST figure quoted
  as per-CALL understates by the arity** — by more than an order of magnitude on the widest call — so
  the probe prints the arity rows itself; and a prediction's REASON is scored SEPARATELY from its
  CONCLUSION, each as worded.
  **⚠ WHAT AN ARM HOLDS, AND THE ONE COMMAND THAT ANSWERS IT — five rules from one night, 2026-09-06.**
  **A CONTROL'S BASE DECIDES WHAT IT CAN DISTINGUISH.** A lane proved by SET comparison — name lists
  kept, only-at-mine EMPTY, gone-now EMPTY, the arithmetic closing across three arms with every
  addition accounted for — that neither of its later commits added a single failure, and it was
  exactly right about what it claimed; but its base was its OWN first seat's tip, so the 42 standing
  failures could equally be the corpus's OR that seat's. **"Pre-existing at MY BASE" is not
  "pre-existing at MASTER" when the base already contains the commit under suspicion**, the tell was
  a sibling lane reading a very different failure count on a different tree, and when the question
  widens past a control the base moves with it — the extra arm is one run. **A "PRE-CHANGE" CONTROL
  TREE MUST DIFFER FROM THE TEST TREE ON ONE AXIS, AND A TREE CARRYING SOME OTHER SEAT IS NOT THAT**:
  a package regression was attributed to a train's first seat from four readings whose "without it"
  arm was master plus ONE UNRELATED seat rather than master plus the other fourteen — **a one-axis
  control in appearance and a two-axis comparison in fact** — refuted by BUILDING the alternative, an
  assembly with that seat and its repairs removed reproducing the failure EXACTLY (same 221 empty
  verdicts, same first and last name in the span), so the seat was never the cause. Name what each arm
  holds, and when an arm is "the tree before X", say which OTHER commits it also lacks. **VERIFY EACH
  ARM'S CONTENTS BY ANCESTRY (`git merge-base --is-ancestor <accused> <arm>`), never by the merge
  order you intended**: a branch cut from INSIDE a train's chain drags the whole chain in, so a rung
  labelled "add these three files" added 111 files and 6,666 lines with the accused seat among them,
  and "the cause is mine" was published off it — one command per arm. The tell that forced the check
  was an emission diff coming back EMPTY, and **a result that does not fit the mechanism is a reason
  to re-examine the ARM, not to invent a mechanism.** Then the pair that closes the class: **a banked
  lesson that is not MECHANISED is a lesson that will be paid for again** — the same coordinator
  retracted a two-axis-labelled-one-axis attribution, banked "name what each arm holds", and committed
  the identical error one bisect rung later, because the lesson was written down and the one command
  that applies it was not run. Put the check IN the probe script (the arm prints its own ancestry
  answer before it runs) so applying it costs nothing and skipping it is visible: three lines, and it
  **printed unasked in the very next unrelated run**, on a probe whose author was not thinking about
  it. That is the difference between a lesson in a document and a lesson in a tool.
  **⚠ THE AXIS NOBODY VARIED — six hours of fleet reasoning, and what each correction cost
  (2026-09-06).** **AN ARM THAT MEASURES A SEQUENCE ATTRIBUTES THE WHOLE SEQUENCE'S FAILURE TO ITS
  FIRST STEP**: a table reading array/pointer/func as "caught panic at master" carried the whole
  investigation until its own author built an arm separating the WRITE from the WALK that follows and
  measured that **every write lands and reads back Go's answer on all eight kinds** — the panics
  belonged to the walk. The consequence was total, because a refuse-by-name ruling stood entirely on
  "those kinds are silently writing to the wrong field", which was false, and the accused commit is a
  REGRESSION against measured-correct behaviour rather than a fix exposing a latent fault.
  **Decompose the operation before attributing its failure**, and note who caught it: the instrument's
  author, still testing after everyone else had accepted the reading. Two earlier corrections to that
  same table, each by measurement rather than by care. **An acceptance criterion is derived from a
  measured BASELINE, never from the shape of the failure** — an eight-kind arm found all seven
  reference kinds dying at a seat and published a criterion requiring all seven to become CATCHABLE,
  while the properly measured PRE-SEAT baseline had only THREE failing (catchably) and the other FOUR
  SURVIVING, so building to the published criterion would have made a fix REFUSE four shapes that
  previously worked — and no gate could show it, because the row would still report and the package
  would still pass. **And a criterion derived on ONE HOST is a criterion for that host until a second
  host reads it**: on Linux master all eight kinds SURVIVE where Windows fails three, so "make it fail
  the way it failed before" is per-PLATFORM — Go itself is platform-independent here and we are not —
  and neither the author nor the coordinator who RATIFIED it as *the* acceptance test asked which host
  it came from. **Print the host in the instrument's output.** Beside them: **two failures narrowing
  to one commit are not necessarily the same failure** — a common CAUSE is not a common DEATH — so the
  fix's acceptance reports BOTH readings from the SAME tree (both recover together, or the arm
  recovers and the row does not, the row's cause still inside that commit but not what the arm
  measures), asked BEFORE the fix runs. The mechanical corrective for all of it: **before ruling on a
  defect, name the axis every reading in front of you SHARES, and ask what sits on the axis nobody
  varied.** Here survived / caught / died are all LIVENESS, not one measurement asked whether the
  surviving write produced GO'S ANSWER, and one added assertion settled in minutes what six hours of
  argument could not — it was available the whole time. **Rule at the speed of the EVIDENCE, not the
  speed of the conversation**: five coordinator attributions or rulings in one night, four corrected,
  none wrong for want of care in the argument, and every refutation came from somebody BUILDING the
  thing — an assembly, a baseline run, a real merge — rather than reasoning about it. The build was
  cheaper than the argument.
  ⚠ **ONE-ARM ATTRIBUTION FROM THE INTERMEDIATE THAT ALREADY EXISTS: master GREEN, master+X RED,
  X-absent seats ruled OUT by `merge-base --is-ancestor` PRINTED PER BRANCH — and the survivors are then
  RE-GATED at the tree that lands, never exonerated by argument.** Measured 2026-09-07: an arm carrying
  master plus one seat trio reproduced both dial-guard reds SOLO, and the re-assembly without the trio
  read both guards Output 1/0 before landing. **The transfer argument for the legs NOT re-run is stated
  as a FILE-SET check** — the seat diff touches 0 converter/golib/gen files, so the CNR, solution and
  GolibTests readings transfer from the previous master — **asserted in the land script, never assumed.**
  **⚠ A GUARD'S ROWS ARE ENUMERATED OVER THE AXES ITS PREDICATE READS, not the axes the change
  targets** (measured 2026-09-03, twice on one golib predicate). A fix's identity guard held only slice
  rows with ARRAY elements, so `ISlice<T> : IArray<T>` let a slice element's runtime LENGTH be stamped
  as an array dimension on a map's descriptor and two equal `http.Header` values with different
  insertion order interned as different `reflect.Type`s — `DeepEqual` false on textually identical
  values, three banked verdicts pass→fail at a train head. The same latent predicate deep-cloned a
  named-slice element and threw an `InvalidCastException` in the array-range copy the same week:
  **`ISlice : IArray` makes every "is this an array" test a trap unless it excludes slices
  explicitly.** The fix goes at the PREDICATE's door so no caller can reach the hole again.
-->

## Censuses and predicates

- **Every count in one table is derived under ONE stated predicate, exclusions named by file and line.**
  "Followed by `(`" as a proxy for "is code" is a predicate of its own and needs its own control, since a
  comment can QUOTE a call: a figure can be RIGHT BY ARITHMETIC and WRONG BY DERIVATION, and **a
  reconciliation that merely FITS is refused as a disclosure on resemblance is.** **Key a census on the
  CONSTRAINT, not the name** — a name-keyed count misses every member the name does not cover, a PACKAGE
  claim read from ONE FILE is not a census, and the count is PRINTED BEFORE a ruling quotes the number.
  **State a gate as the PROPERTY the emission needs, never a spelling**: a SYNTACTIC enumeration of operand
  shapes misses the one nobody listed (an address-of label is a unary expression), where screening on the
  `go/types` TYPE clears every shape with ONE predicate. **Re-read the CITED LINE's own notation before
  claiming to falsify it.** **A census number travels with its
  UNIT, and a reconciliation RE-DERIVES the other instrument's number rather than POSITIONING it.**
- **When a helper documents the N renderers that must spell a thing ONE way, census all N**: the renderer that
  never got the spelling IS the defect. **A switch arm returning for ONE pointee kind lets every other kind
  FALL OUT to a generic fallback that cannot compile** (CS0144 against an abstract box type), and two arms of
  one switch take controls on SEPARATE assertions when one half's compile failure MASKS the other.
- **An observation set aside as "not the root" is re-read against the COMPILER'S OWN WORDS first** — "a
  constant value is expected" IS the compiler's words for a pattern whose operand is not constant — and **ONE
  DEFECT CAN WEAR TWO DIAGNOSTICS**: the same lowering says that for a bare identifier and "type or namespace
  not found" for a member CALL. **A converter reproducer that exits 0 has measured the EMISSION, not the
  COMPILE**, and **a C# cast binds LOOSER than member access.**
- **A NULL is a result only after the instrument is shown to have FIRED**, and **a pass's documented SCOPE is
  read before predicting what it does to a site.** **A COMPILE BLOCKER masks a whole flag-on emission** —
  census a blocker post across the WHOLE build output, not the file its first error names. **"The stamp is
  there" is not "the stamp is read"**: give the sites that share an implicit rule ONE named predicate and
  assert the OUTPUT. **A fix can silently do NOTHING when an accessor materializes a DETACHED COPY.**
- **READ THE FALL-THROUGH BEFORE MEASURING THE ARMS** — where the predicate is FALSE the value does not stop,
  it lands in the next branch and can be answered WRONGLY AND SILENTLY there, so the arms are designed from
  the code's own fall-through, not from the arm list. **Predict PER ARM with the SCOPE named** (a zero on one
  arm is a statement about the harness's population, not about the corpus), and **NAME the arm that carries a
  built-in positive control** — if nothing drives that arm above zero the census never ran at all.

<!-- DERIVATIONS (census predicates, renderers, nulls and blockers) — Phase 1 text, verbatim:
  **⚠ PREDICATE DISCIPLINE FOR A CENSUS — every count in one table is derived under ONE stated
  predicate with its exclusions named by file and line** (four instances, 2026-09-03). "Followed by
  `(`" as a proxy for "is code" is a predicate of its own and needs its own control: a comment can
  QUOTE a call, so one symbol read 93 under the paren proxy against 95 raw / 91 code / 89 call sites,
  and a "91 actionable" figure was nearly published RIGHT BY ARITHMETIC AND WRONG BY DERIVATION. A
  reconciliation that merely FITS ("subtract the bookkeeping") is refused exactly as a disclosure on
  resemblance is. A name-keyed census read 14 sites in 2 files where the CONSTRAINT-keyed derivation
  read 23 in 4 — two of the missed files banked rows. **A gate is stated as the PROPERTY the emission
  needs ("names a library"), never as a spelling** — a literal `.dylib` gate would have excluded the
  28 framework records the ruling counted IN — and a shipped comment claiming an invariant is
  falsified by the same census and fixed in the SAME cut. **A sizing asserts a population by a
  predicate the emission gates on, names the excluded shapes, and still runs the diff that would have
  caught a wrong assertion** ("windows and linux ZERO by construction" was split by a count: the
  pragma is absent on linux and present 51 times on windows in a DIFFERENT SHAPE) — correct the
  sizing, not the commit. ⚠ And **re-read the CITED LINE's own notation before claiming to falsify
  it**: a "0 of 345" headline measured a proposition the design never asserted (it compared against a
  remembered PARAPHRASE; the design's own notation holds 344 of 345, and what was wrong was the SCOPE).
  ⚠ **A CENSUS NUMBER TRAVELS WITH ITS UNIT, and a reconciliation RE-DERIVES the other instrument's
  number rather than POSITIONING it** (2026-09-05). One lane's 46 — a whole-flavour count of
  address-taken scalar VARIABLES, deduplicated by variable — was placed into another lane's SITE chain
  (27 < 39 < 48 < 61) where it cannot sit; re-derived from the instrument's own file, 10 of the 46 are
  lift-shaped and 0 are `&args` structs, so the operative conclusion survived and the MAPPING did not.
  A number quoted without its unit reads as a member of whatever series it is placed in.
  **⚠ ELIDED-vs-TYPED SIBLING DRIFT: when a helper documents the N renderers that must spell a thing
  ONE way, census all N** (2026-09-04). A converter fix landing on the TYPED renderer of a construct
  never reached its ELIDED twin — the arm that renders the same shape when the literal's type is
  INFERRED — so the elided form kept the pre-fix emission and a keyed sibling kept the same hole: the
  renderer that never got the spelling IS the defect. Its neighbour: **a switch arm that returns only
  for ONE pointee kind lets every other kind FALL OUT to a generic fallback that cannot compile**
  (CS0144 against an abstract box type), so the class is every kind the arm does not name, reached
  through every literal shape that routes an elided element there. Two defects that are two ARMS of
  one switch take controls on SEPARATE assertions when one half's compile failure MASKS the other (a
  third binary carrying only one half isolates it); the fix's own assertion is stronger than "it
  compiles" when the elided spelling emits BYTE-IDENTICALLY to the explicit one; and a residual
  deliberately NOT fixed is recorded at the call site, in the reference doc AND in the guard's
  comment, with its honest fix named as its own item.
  ⚠ **AN OBSERVATION SET ASIDE AS "NOT THE ROOT" IS RE-READ AGAINST THE COMPILER'S OWN WORDS FOR THE
  ERROR BEFORE IT IS SET ASIDE** — twice in one evening (2026-09-08): a census node reading "not found
  here" was filed as a footnote while Go's chain was traced anyway, and a lowered comparison spelling
  a PATTERN MATCH where Go means pointer equality was filed as "not that diagnostic and not that
  root", when that diagnostic IS "a constant value is expected", the compiler's words for a pattern
  whose operand is not constant. The ARTIFACT settled it in one read: the two arms of ONE lowered
  chain DISAGREE — the pattern form for the address-of case and the equality form for nil one line
  down — so the converter already knew the right form, and the screening that stopped the C# switch
  closed the right half while leaving the pattern spelling in the chain it produced. **A converter
  reproducer that exits 0 has measured the EMISSION, not the COMPILE; a compiler-error claim is a
  compile-time claim.** ⚠ **ONE DEFECT CAN WEAR TWO DIAGNOSTICS**: the same lowering reads "a constant
  value is expected" when the operand is a bare identifier and "type or namespace not found" when it
  is a member CALL, since C# then reads a POSITIONAL PATTERN whose type would be the call's receiver —
  so one root's second site was the other root in a different costume, and the rung prediction MOVED
  before the rung, with falsifiers both ways. Its neighbour, a genuinely separate defect: **a C# cast
  binds LOOSER than member access**, so a pointer-to-array index shape needs the cast PARENTHESISED
  before the member access, or it indexes the operand and casts the result.
  **⚠ A NULL IS A RESULT ONLY AFTER THE INSTRUMENT IS SHOWN TO HAVE FIRED** (three shapes,
  2026-09-03/04). A spike's null at the CALL SITES was an instrument artifact — the DECLARATION had
  never lowered, so nothing had fired — and the real blocker was the pass's own stated SCOPE, which
  neither hypothesis had read: **read a pass's documented scope before predicting what it will do to a
  site.** A COMPILE BLOCKER masks a whole flag-on emission (once the package compiled the census read
  ZERO reduction, 98 = 98, because one predicate pinned the leaves and a chain is pinned by its
  leaves) — so a blocker post is censused across the WHOLE build output, not the file its first error
  names (four errors reported, 99 CS0103 missed in the sibling). And **"the stamp is there" is not
  "the stamp is read"**: after three landed halves a positive control was STILL red with the stamp
  visibly present in the emitted C#, because a FOURTH site discarded it — four sites carried the same
  implicit membership rule and each was widened separately; the remedy is ONE named predicate they all
  call, and the control that finds it asserts the OUTPUT, not the artifact. Its golib twin: **a fix
  can silently do NOTHING** when an accessor materializes a DETACHED COPY of the storage — every write
  lands on a throwaway object and every read misses, with no error anywhere.

  ===== BATCH19 (24bfc8304 / c5e17217b / e9e56b657, merged 2026-09-12) — verbatim =====
  The first is a NEW INSTANCE of "key a census on the CONSTRAINT, not the name" and sharpens it with two
  clauses now carried visibly (one file is not a package claim; print the count BEFORE the ruling quotes
  it). The second is a new instance of "state a gate as the PROPERTY the emission needs, never a
  spelling", with the go/types screen named visibly; its two companions are kept here in full. The third
  is the new visible "READ THE FALL-THROUGH BEFORE MEASURING THE ARMS" rule.
  ⚠ **A NAME-KEYED CENSUS MISSES THE MEMBERS THE NAME DOES NOT COVER, AND ONE FILE WAS READ FOR A
  PACKAGE CLAIM** (2026-09-08): `internal/sync` has EIGHT bodyless partials, not six — two carry no
  `runtime_` prefix, and a third lives in a different file from the one read. **7 + 1 is visibly not
  6**, and the count was printed only AFTER the ruling had quoted the number; the two missed members
  belong to another lane's fatal-path class with a SECOND SITE at the new release, and were NAMED
  rather than silently adopted into the hand-own.
  ⚠ **A TYPE-BASED SCREEN COVERS EVERY OPERAND SHAPE WHERE A SYNTACTIC ENUMERATION MISSED ONE**
  (2026-09-08): a switch-lowering screen listed identifier, selector and index expressions, and an
  ADDRESS-OF label is a unary expression, so it fell THROUGH; screening on the label's `go/types`
  TYPE (a pointer can never be a constant pattern) clears both diagnostics with ONE predicate, and
  the guard GREW to carry both operand shapes — because the field-address form, the one the real
  source has, would have kept emitting the wrong lowering under a bare-identifier-only guard. Two
  companions: **a behavioral guard's `go.mod` must not trigger a toolchain DOWNLOAD** (a newer `go`
  directive made the oracle fetch; pinned like its siblings, and under `GOTOOLCHAIN=local` it would
  have failed outright); and the corpus's ProjectReference CONDITION SET is CLOSED — 2,651
  unconditioned, 56 linux, 45 darwin, 23 windows, no negations, no AND/OR, and no output-type group
  enclosing a reference — which makes a fold's GOOS conditioning PROVABLE rather than heuristic.
  ⚠ **READ THE FALL-THROUGH BEFORE MEASURING THE ARMS** (2026-09-08, C2): `IsTokenArithmetic` masks the
  low 32 bits and requires `allocationBase != number`, so it is FALSE when the number IS the base (offset
  0) — a cross-type resolve at offset 0 therefore falls PAST the arithmetic refusal to `new
  NativeBox<T>((nuint)value.Value)`, a native box over a TOKEN, answering the write case wrongly today
  and silently. That was read out of `ж.cs:700` BEFORE the four-arm census was wired at the registry
  (caller-supplied values, no stack walk), predicted per arm with the SCOPE named — a GolibTests zero on
  one arm is a scope statement, the corpus population owed to a Windows box — and the arm carrying a
  built-in positive control was NAMED, since the refusal tests must drive arm 3 above zero or the census
  never ran.
-->

## Predictions

- **Every reading gets a prediction to disagree with** — that, not the fix, is what makes an arc fast. State
  it BEFORE the run, score it BY NAME as worded with REASON separate from CONCLUSION, and **state it
  falsifiably**: "if X is the cause this arm CHANGES the errno; an arm leaving it unchanged has FALSIFIED its
  own candidate", not pass/fail. **Before probing two candidate mechanisms, ask which ARGUMENTS the failing
  call READS** — a struct the callee only WRITES cannot produce an errno about BUFFER SIZE (`getpwuid_r`'s
  ERANGE), so the DISCRIMINATING arm is on a READ argument.
- **A prediction is stated in a currency the predictor has MEASURED** and **names the LAYER it models.** **"A
  REPRODUCTION, NOT A PREDICTION" is the honest label when the expectation was informed by the other lane's
  result**, and **a non-fresh reading is REPRODUCED in a fresh worktree rather than argued sound.** **An arm
  added to test one's OWN mechanism can refute it and calibrate the instrument in one run**: its STABILITY —
  same sign and magnitude in every order and both tiering modes — is what a REAL difference looks like, while
  a pair whose sign flips with tiering is AT OR BELOW RESOLUTION. **A per-TEST figure quoted as per-CALL
  understates by the arity.**

<!-- DERIVATIONS (predictions) — Phase 1 text, verbatim:
  ⚠ **AN ARC AS A RECORD SHAPE: four predictions, each stated BEFORE its run and scored BY NAME**
  (2026-09-08). A row that ate a five-minute deadline with ZERO converted verdicts became a
  three-and-a-half-second PASS through a blocker that moved one deeper BY SYMBOL, an iteration-index
  probe that named the failing shape, a dispatch increment whose adapter arm bound while registration
  refused at index 0 on the container, and the referent fix that delivered every case — **with one
  wrong denial corrected by SHA within the hour, and one superseded SHA MEASURED to confirm the
  retraction rather than taken on report. What made it fast was never the fix: it was that every
  reading had a prediction to disagree with.**
  ⚠ **BEFORE sending a probe at two candidate mechanisms, ask which ARGUMENTS the failing call READS**
  (2026-09-05): a struct the callee only WRITES cannot produce an errno about BUFFER SIZE
  (`getpwuid_r`'s ERANGE), so a wrong layout there corrupts or faults rather than explaining the
  symptom — the DISCRIMINATING arm is the one on a READ argument, and a candidate settled STATICALLY
  (a managed struct with reference fields handed by address) is owed without a measurement at all.
  State the prediction in the sharper form for the same cost: **"if X is the cause this arm CHANGES the
  errno; an arm leaving it unchanged has FALSIFIED its own candidate"**, rather than pass/fail.
  ⚠ **"A REPRODUCTION, NOT A PREDICTION" IS THE HONEST LABEL WHEN THE EXPECTATION WAS INFORMED BY THE
  OTHER LANE'S RESULT** (2026-09-08): a second host's zero adds INDEPENDENCE — a second box, a second
  operator, the same instrument — while the property that makes the zero a MEASUREMENT (one
  instrument, eight before, zero after) belongs to the first lane, and the "eight before" reading is
  the second lane's own earlier step, so the pair is not circular. Two companions: the pairing arm was
  LOAD-BEARING rather than a formality, because a planted control had just shown the widened predicate
  CAN stamp at the older convert pin, so a NEW stamp in the real corpus surfacing as CHANGED was a
  live risk; and a non-fresh reading, taken in a worktree carrying build output from a full suite, was
  REPRODUCED in a fresh worktree rather than argued sound — "because you banked a stale-binary
  near-miss in the same hour".
-->

## Read the artifact before the sentence

- **Read the artifact before writing the sentence that is about it.** The forms: a design's notation, a
  shipped comment's claim, a test file's existence, a claimed dependency on another lane's branch; an
  "unverified" written WITHOUT LOOKING, which hides better; a CALL SITE cited without reading the CALLEE (the
  site PANICS on its first line, the callee returning an INERT NIL); a finding routed from a lane's PROSE
  instead of the record; a check quoted without its COUNTING METHOD; a ruling premise quoted from doctrine
  instead of read AT THE TREE. **A ruled measurement can be satisfied by a READ** — a width claim in a
  comment costs ONE `GOOS=windows GOARCH=386 go build` and stops being an argument.
- **TWO TRUE FACTS AND AN INVENTED RELATION: CO-OCCURRENCE IS NOT A RELATION.** A missing hand-own and a
  throwing stub in the same package are not "the hand-own bodies the stub" until one `git show` says which
  symbol the file actually bodies — and the correction runs in BOTH directions, since the absence can cost
  nothing while the real gap is a frontier item rather than staleness. **READ THE GREP: a count that
  contradicts its own evidence line is the false-empty family's arithmetic member.** **A hazard named from
  ONE LANE'S SOURCE READ can already be DISCHARGED in the tree the seat will be cut against** — read the
  TREE before sizing the remedy, report what the tree cannot exhibit as UNMEASURED rather than guessing,
  and LABEL an attribution inferred from a tree's state as inferred.
- **A COMMENT THAT CLAIMS A BEHAVIOUR THE CODE LACKS READS AS THE CENSUS, and a faithful PORT propagates the
  claim** — the darwin twin inherited a missing `Wait4`/EINTR loop, one zombie per failed transfer. **A
  DELIBERATE NO-OP is the shape MOST likely to carry a documented reason**: `pprof_impl.cs`'s `_ = labels;`
  sits over a recorded HOST-KILLING OOM. **A record's MEASUREMENT and its MECHANISM are separable claims**,
  and **a refuted row takes a DATED amendment the day it is refuted, naming the file and line.**
- **THE ORACLE'S CAPTURED STRINGS ARE THE SPECIFICATION — its FALLBACKS included**: our climb failing where
  Go's SUCCEEDS is the defect, where Go's own fails is contract. **An expectation read off the thing under
  test is not a test of it** — expected strings come FROM GO under the pinned toolchain, PINNED and not one
  answer (a C# `int` is Go's `int32`). **The TIDY-LOOKING change is measured against Go's own output BEFORE
  it is written**: Go sorts interface method names by the BARE name, so the obvious sort would REVERSE Go's
  order — leave the evidence in a comment AT THE SITE.
- **A COORDINATOR'S LEAD IS A HYPOTHESIS, and the coordinator's share of a bad rule is the larger one** —
  retract IN PUBLIC the moment it is measured false, with the real site in the same message: **a lane sent to
  the WRONG file loses more time than one sent nowhere.** **A lane's observation becomes fleet DOCTRINE only
  after surviving an attempt to break it**: two runs agreeing to three decimals is a SUGGESTIVE NUMBER, not a
  mechanism. **A DEFERRAL IS A READ OF THE RECORD, AND A READ HAS A TREE** — "settled" is a claim about the
  TIP; resolve crossed messages by quoting the SHA SEQUENCE.
- **AN ERROR THAT SAYS "DO NOT BOTHER" IS WORSE THAN ONE THAT SAYS "TRY THIS" — nobody measures a road they
  have been told is closed**: a wrong positive gets measured and dies, while a wrong negative REMOVES the
  measurement that would have killed it. **Read the code before predicting from it; bank the negative you
  MEASURED, not the one you reasoned to.** **A file's SELF-DECLARED LABEL carries no weight**, and **printing
  `n/a (failure path not entered)` is honest for a PROBE and disqualifying for a GUARD.** **A hand-owned host
  that must hand a converted package an INSTANCE of an interface it may not reference has a supported answer
  already: `golib.AdapterBinder.TryCreate`** — methods on the BOX receiver, interface type via
  `Type.GetType`; no assembly, no project reference, no dynamic codegen.

<!-- DERIVATIONS (reading artifacts, oracles, leads and deferrals) — Phase 1 text, verbatim:
  ⚠ **A REFUSAL IS NOT RETIRED BY FINDING ITS EXPLANATION DATED — but the reason it happened may no
  longer be the reason it would happen, which changes WHERE the fix lives.** `pprof_impl.cs`'s refusal
  records BOTH an observation (a map reading `len == 1` at store and a garbage length at read-back
  across two GCs, ending in an `OutOfMemoryException`) and an explanation naming an API that no longer
  exists — today's `SetProfileLabels(object?)` stores a managed reference twice and mints nothing
  (2026-09-07). The observation stands; the explanation is stale. **A record's MEASUREMENT and its
  MECHANISM are separable claims**, and here the surviving `uintptr` hop sits at a GENERATED line, so a
  remedy could live at the CONVERTER rather than behind the pointer-token arc.
  ⚠ **A RULED FIX'S CAVEAT IS NARROWED FROM "I DO NOT KNOW" TO A NAMED RESIDUAL BY READING THE
  CONTRACT, then discharged by an ARM rather than by a doc comment** (2026-09-08): equality on the box
  type is pointer identity BY CONSTRUCTION — the operator delegates to the per-kind equality, and the
  order token is documented per box kind as producing equal tokens for equal pointers — so the fix has
  the right shape, and the ONE place it could compile and still diverge is a CROSS-KIND comparison (a
  box recovered from a numeric handle against a heap box). That is NAMED, PRICED (a sentinel
  comparison silently false makes the loop SPIN rather than fail loudly — the class of red no gate
  reads) and folded into the guard as an output-compared round-trip row, with the unequal outcome
  routed BY NAME to the token registry rather than to the fix. **Two posts crossing by under a minute
  and converging independently — one from the diagnostic text, one from the artifact — is worth more
  than either alone.**
  **⚠ THE ORACLE'S CAPTURED STRINGS ARE THE SPECIFICATION — its FALLBACKS included** (2026-09-03).
  Go's own `valueMethodName` climb fails on the package-level `reflect.Append` path and Go itself
  prints `call of unknown method on int Value`, so threading the public name there would have "fixed"
  a string Go DELIBERATELY prints and broken the byte-compare gate. **Our climb failing where Go's
  SUCCEEDS is the defect; where Go's own fails is contract.** Capture the oracle's text through the
  PUBLIC entry point before choosing what to thread — and prefer a SOLE-CALLER proof to a capture (a
  panic you fail to provoke proves nothing about reachability), with a composer whose own test on the
  threaded name preserves the fallback BY CONSTRUCTION rather than by a special case. ⚠ Its
  construction rule: **an expectation read off the thing under test is not a test of it** — a
  formatter-delegation guard's eight expected strings were taken FROM GO under the pinned toolchain,
  and it went red on its first run for a REAL reason (a C# `int` is Go's `int32`, not `int`): the
  mapping is PINNED, not one answer. ⚠ **The TIDY-LOOKING change is measured against Go's own output
  BEFORE it is written** (2026-09-04): once interface method names were package-qualified, sorting the
  RENDERED (qualified) strings looked like the obvious completion — and Go sorts by the BARE name
  (`interface { zlib.aaa(); main.zzz() }`), so the tidy sort would have REVERSED Go's order. The sort
  stays untouched with the evidence in a comment AT THE SITE, because the next reader will see the
  qualification and reach for it; and a row that cannot be built (it needs a sibling package) is
  recorded in the guard's comment rather than left unmentioned.
  **⚠ A FALSIFICATION IS BANKED AS A LIVE ASSERTION, and a finding SURVIVES its mechanism**
  (2026-09-03). A candidate root posted by the coordinator was measured FALSE twice over (the predicate
  never reaches the token; the fallback race produced 0 wrong answers in 200k+ takes under 200k+ forced
  compacting collections), so the FINDING stands — the row dies — while its MECHANISM is retracted the
  moment the falsification lands, the compiled remedy is REVERTED under the non-reproducible-motivating-
  failure rule with its design kept, and the falsification carries a positive control that its premise
  is real (the hash does collide) before its consequence is asserted false. A "found in passing, read
  from code, not measured" item is LABELLED exactly that. ⚠ Its constructive twin: **a finding is
  rooted to a CLASS by repetition with variation, not by hypothesis** — four deaths, two host classes,
  two different racy tests first, one message and one call chain, the death point moving
  286/373/656/2124 s — which establishes "not host-conditional, not test-specific, a race" without
  asserting any mechanism, and a frame-by-frame VERIFICATION post follows the ones that merely CLAIMED
  the chain.
  **⚠ ASSERTING AN ARTIFACT'S CONTENT WITHOUT READING IT — four instances on one arc, one of them the
  coordinator's** (2026-09-03): a design section's notation, a shipped comment's claim, a test file's
  existence (the guard a design "proposed" already existed and covered its whole tier), and a
  "dependency on another lane's branch" for a class that exists at master. **The rule already written
  for declarations extends to every artifact a claim is about: read it before the sentence.** Its
  routing twin: a finding relayed from a lane's PROSE ("four guards carry no marker") was routed as a
  cut without re-deriving it from the record — all four already carried the marker and the "red" was a
  ruled criterion working as designed; three derivations plus a positive control, empty branch deleted,
  no SHA, is the correct shape of a stop-and-post. ⚠ And **a check is quoted WITH its counting
  method**: two lanes quoting different numbers for one named check (38 against 51 — parenthesised call
  sites against the bare token) both satisfied the relation the check asserts, so a discrepancy in a
  PASSING check is stated the moment it is seen rather than left to become the next carried figure.
  ⚠ **A COMMENT THAT CLAIMS A BEHAVIOUR THE CODE LACKS READS AS THE CENSUS to the next reader, and a
  faithful PORT propagates the claim** (2026-09-05): the linux exec seam's Foreground-failure path
  SIGKILLs the child and returns without Go's `Wait4`/EINTR loop — one zombie per failed transfer —
  and the darwin twin is identical, because the twin was written from the comment. **A failure path
  that discards a LIVE child owes its reap even when no roster row reaches it**; the roster's silence
  is exactly why it survived review. Routing: the design-of-record's OWNER fixes a LANDED seam as its
  own seat with a guard, while a twin inside an UNANNOUNCED cut is fixed inside that cut.
  ⚠ **Its constructive twin, 2026-09-04:** a hand-owned host that must hand a converted package an
  INSTANCE of an interface it may not reference had a supported answer already in the tree —
  `golib.AdapterBinder.TryCreate` (public, its own header stating exactly that case: a dynamic type
  may live in an assembly converted AFTER the interface's own) builds the duck-typing shell over a
  Go-shaped box whose methods are written on the BOX receiver (the one form the binder takes;
  `ResolveReceiverMethods` skips a by-ref receiver), with the interface type obtained by
  `Type.GetType` — no assembly, no project reference, no dynamic codegen. **`Reflection.Emit` and a
  satellite assembly were cancelled on the tree's own TEXT before either was built.**
  ⚠ **The read-it-before-the-sentence rule met from two more directions, 2026-09-06.** **An
  "unverified" written WITHOUT LOOKING is the same failure as an unread anchor, and it hides better**:
  a design record's section said a registry gate's composition with a per-GOOS body was unverified
  "since every existing member is a flat file", and FIVE of the six members emit per-GOOS with the
  accessibility flip, one is flat, and the exact analogue sits in the same package — **a ruled
  measurement can be satisfied by a READ**, and the production mechanism already working is stronger
  evidence than a synthetic probe (the author's own correction, before the train landed it). And **the
  CORPUS can refute a prediction before it is made**: a row failed to move for a reason its own
  hand-own HEADER already recorded — those rows sit behind a host-killer first, so bodies there move
  nothing measurable. Third instance in one evening of a claim asserted without reading the file it was
  about, and the first where the file was OURS rather than Go's.
  ⚠ **A CALL SITE CITED WITHOUT READING THE CALLEE is the same failure with a callee clause**
  (2026-09-08): "the corpus already does that conversion at this file and line" named a REAL call site
  whose callee is a hand-own returning an INERT NIL interface value — its own comment says why,
  raw-metal on a non-native type — so the cited site PANICS on the first line of the function it was
  offered to support, and there was no working conversion on either side. Twice in one arc, both files
  one command away, the pattern named by its author — and the correction re-framed the item from
  WIRING to a bucket-3 FRONTIER, the body's own dependencies being themselves stubs, which moved the
  remedy from a push to a managed callback minted on the syscall side.
  ⚠ **A COORDINATOR'S LEAD IS A HYPOTHESIS, and the coordinator's share of a bad rule is the larger
  one** (2026-09-05/06). A lead is **RETRACTED IN PUBLIC the moment it is measured false, with the real
  site in the same message** — the integer-setter chain a lane was dispatched to was measured SAFE (a
  reference-free pointee gets the pinnable slot, the reinterpret takes the aliasing arm, the new token
  arm never fires) and the option named was not even on that path: **a lane sent to the WRONG file
  loses more time than one sent nowhere.** Before routing at all, **measure a claim's EXTENSION**:
  seventeen alleged allocation entries whose reason asserts pointer semantics were refuted by measuring
  exactly that set — EMPTY — alongside three others, two built to fail if the refuter were wrong, while
  a bucketing that treats the capability CLASS as an allocation family reproduces the reported number
  exactly, seventeen included. **A lane's observation becomes fleet DOCTRINE only after surviving an
  attempt to break it**: a lane reported its mechanism as explicitly UNESTABLISHED and its correlate as
  a place to look, and the coordinator published it as a reading rule with an imperative — because
  **two runs agreeing to three decimals is a SUGGESTIVE NUMBER, not a mechanism** (a hosted-runner leg
  died at 47.017 minutes twice; the THIRD dispatch of the same stage completed in 3.2 minutes with its
  compile genuinely running, and the measurement survived as one open transient while "deterministic,
  reproducible on this stage" did not). Two agreeing runs is not the attempt; the third run was, and
  nobody had run it. The other direction is safe by a recognisable tell: **when a lane asks a
  coordinator to overturn a ruling in the LANE'S OWN FAVOUR, the tell that it is safe is that the lane
  NAMES the asymmetry and hands over ARMS rather than a conclusion — and that the control which would
  have caught a self-serving reading is the one the lane RAN** (the coordinator was right about the
  fact and wrong about the cause, and one one-axis pair settled it).
  ⚠ **A DEFERRAL IS A READ OF THE RECORD, AND A READ HAS A TREE.** A lane proposed a better option, a
  coordinator adopted it, and twelve minutes later the same lane stood down with "it is settled, take
  the original" — deferring to a ruling that had already been superseded *on that lane's own argument*
  (2026-09-07). Not reopening a settled ruling is right conduct and the right default; **but "settled"
  is a claim about the TIP, and the tip had moved.** Before standing down, check that the thing you are
  standing down TO is still current, and **resolve crossed messages by quoting the SHA SEQUENCE, never
  by restating positions.**
  ⚠ **AN ERROR THAT SAYS "DO NOT BOTHER" IS WORSE THAN ONE THAT SAYS "TRY THIS" — nobody measures a road they
  have been told is closed.** A lane published *"memoizing these two entry points would move the rows by ZERO
  — state it loudest"*, then read the code three hours later and found they cost real bytes. **A wrong
  NEGATIVE propagates further than a wrong positive, because a wrong positive gets measured and dies while a
  wrong negative REMOVES the measurement that would have killed it** — and "state it loudest" is exactly how
  it spreads. **Read the code before predicting from it.** This is the counterweight to the
  negative-result-is-banked rule beside it: bank the negative you MEASURED, never the one you reasoned to.
  ⚠ Its artifact-level twin: **A FILE'S SELF-DECLARED LABEL GOES IN THE LIST AND CARRIES NO WEIGHT — a probe
  can outgrow its label.** A disposition that opened with a file's own `// never for merge` comment listed it
  FIRST and leaned on it LEAST, because the comment is evidence about its author's intent at the time and not
  about its fitness now; the other three grounds were measurements. **And printing `n/a (failure path not
  entered)` rather than a green it did not measure is honest for a PROBE and disqualifying for a GUARD — a
  guard has no `n/a`.**

  ===== BATCH19 (24bfc8304 / c5e17217b / e9e56b657, merged 2026-09-12) — verbatim =====
  The first is a NEW INSTANCE of "bank the negative you MEASURED, not the one you reasoned to" (its
  branch-standing companion is merge material, kept here because it arrived in this hunk). The second is
  the new visible CO-OCCURRENCE rule. The third is a new instance of "a ruling premise quoted from
  doctrine instead of read AT THE TREE", now carried visibly with the width-claim one-liner and the
  read-the-TREE-before-sizing clause.
  ⚠ **A MECHANISM MEASURED FALSE BEFORE POSTING** (2026-09-08, G): "the namespace empties" was one edit
  from being posted as the cause of a peer's CS0246 on a bare `using` — 403 such usings classified across
  the two releases name ZERO lost directories (`runtime/internal` keeps two packages at the newer
  release, both in `go list std`), so the mechanism is the EMISSION half and was stated as its owner's
  rather than guessed. Companion: **standing blocks drift** — 16 of one lane's 24 remote branches were
  landed remnants by ancestry and its standing showed 5 live where 8 are; **classify by ancestry plus
  `ls-remote`, never by memory.**
  ⚠ **TWO TRUE FACTS AND AN INVENTED RELATION** (2026-09-08, R): a hand-own missing from a tree and a
  throwing stub in the same package were linked by CO-OCCURRENCE — "the hand-own bodies the stub" — when
  one `git show` would have read that the file bodies `syncTimer` while the stub (`runtimeNow`) is a Go
  1.24 ADDITION no release of the corpus has ever bodied. The correction ran in BOTH directions: the
  absence costs zero, and the real gap is a frontier item rather than staleness. Companion instrument
  note: a per-release count expression printed 0 for BOTH releases beside a grep that plainly showed the
  1.24 declaration — **read the grep; a count that contradicts its own evidence line is the false-empty
  family's arithmetic member.**
  ⚠ **A WIDTH CLAIM IN A COMMENT IS CHECKED, NOT ARGUED** (2026-09-08): a parameter's comment
  claimed 32-bit representability, and `GOOS=windows GOARCH=386 go build` (rc=0) cost ONE command
  and turned the claim into a measurement.
  ⚠ **A HAZARD NAMED FROM ONE LANE'S SOURCE READ CAN ALREADY BE DISCHARGED IN THE TREE THE SEAT WILL
  BE CUT AGAINST — read the TREE before sizing the remedy** (2026-09-08): one lane warned that
  deleting a package's two fatal shims without bodying them in its companion would silently re-arm
  the generated throwing stub at the new release; the other MEASURED that the ladder tree ALREADY
  carries them as partial implementations in that companion, so the seat's action is a RETARGET of
  two existing bodies, not a delete-plus-add. **The GENERATED STUB DIRECTORY is a one-listing oracle
  for which partials are bodied** (`sync` holds exactly one stub file, `internal/sync` eight). A
  question the tree cannot exhibit — which diagnostic a collision WOULD raise — is reported
  UNMEASURED rather than guessed, and an attribution inferred from a tree's state is LABELLED as
  inferred.
-->

## Negative results, withdrawals and sizing

- **The warm-design trap: the speculative branch is easiest to write while the design is still warm.** **RUN
  THE FALSIFIER YOU YOURSELF NAMED BEFORE BUILDING THE REMEDY IT QUALIFIES** — a remedy sentence published
  with its own falsifier three paragraphs below it is retired by that falsifier, at the cost of one filtered
  run, and the design is WITHDRAWN UNBUILT. **PRICE THE OBVIOUS REPAIR BEFORE WRITING IT**: a repair can be a
  TRUE corpus-wide structural finding and still not SAVE THE ROW, because the death one line later is
  FAITHFUL — say which increment actually moves the verdict, and split a census PER COUNTER rather than per
  family. Machinery you cannot make FAIL under its own control is DELETED with the measurement in a comment
  at the site. **A negative result is BANKED — in CODE at the gate, or in the RECORD — where the next reader will
  stand**: a follow-up marked *measured wrong: 0 fixed, 1 broken*, or a fix CANCELLED WITH ITS MEASUREMENT
  ATTACHED. **A guard asserting MORE than its increment delivers is narrowed to the DELIVERED reach and the
  removed assertion banked as a NEGATIVE**, and **a cut whose only motivating failure is NON-REPRODUCIBLE is
  HELD.**
- **A CORRECT cut with zero measured payoff is WITHDRAWN, not banked on fidelity** — keep the census, record
  the nulls in the file's own header, leave the elimination chain on the board. **But a PREREQUISITE with a
  measured null LANDS** when it sits on the measured critical path of a NAMED blocker. **AN ARC IS WITHDRAWN
  ON MEASUREMENT EXACTLY AS A COMMIT IS**, its product becoming the correctness fix plus a DESIGN RECORD of
  the measured wall. **A design record's increment ORDER is a prediction like any other** — falsify it by
  READING the mechanism before spending a battery, and correct the arithmetic in a DATED block. **A scoping
  with no case earns no instrument.**
- **A SIZING PROBE THAT STOPS AT THE DOOR IT IS SIZING CANNOT PREDICT WHAT LIES BEHIND IT** — predict the
  next WALL unless the probe ran THROUGH, and score against the CUT's stubs, not the probe's; **a probe that
  dies BEFORE its first measurement proves only the wall it died on**, leaving its prediction UNMEASURED,
  never "met". **An acceptance table that enumerates WHERE a dispatch dies must first ask whether the row
  dies BEFORE dispatch** — a mute exit code is the ABSENCE of evidence (138 is 128 + SIGBUS, never reaching
  the managed throw path), so dispatch the stage that keeps whole stderr before arguing a mechanism.
- **Enumerate outcomes per FAILURE, not per row**; **a row can be BLOCKED TWICE, so state the acceptance PER
  BLOCKER**, said before an arc lands. **A cut owes its OWN behavioral guard**, pinning both acceptance
  directions including the one no consumer exercises. **An audit can measure what a change BUYS and miss what
  it COSTS, which is half a datum that reads whole**: **a benefit figure with no cost figure is not a
  sizing.** **When a design's precondition is a runtime PROPERTY, measure the property STANDALONE before
  anyone writes the design — and positive-control it** with an arm you EXPECT to move.

<!-- DERIVATIONS (negative results, withdrawals, sizing) — Phase 1 text, verbatim:
- **The warm-design trap:** the speculative branch is easiest to write while the design is still warm
  — and twice in one day (2026-09-01) a lane built guard/fix machinery, could not make it FAIL under
  its own control, and deleted it with the measurement recorded in a comment at the site. An
  unexercisable branch in a guard is a false-green seed; deleting it with its evidence is the
  deliverable, not a loss.
  **Its positive twin: a negative result is BANKED — in CODE at the gate, or in the RECORD**
  (both 2026-09-01/02). A measured-wrong next step recorded where the next reader will stand — the
  `MapIndex` follow-up marked *measured wrong: 0 fixed, 1 broken*, in the code at the site it would
  be attempted from — and a commissioned fix **cancelled with its measurement attached** each cost
  one line and save the next lane the whole attempt. The cancellation carries its own rule:
  **a predicate the converter already holds beats a metadata field nothing reads** (the proposed
  flag was dropped because an existing classifier already answers the same question, counts and all).
  ⚠ **Its narrowing form: a guard asserting MORE than its increment delivers is narrowed to the
  DELIVERED reach, and the removed assertion is banked as a NEGATIVE** (2026-09-05) — never left red
  on a landed seat, and never quietly deleted.
  **And a cut whose only demonstrated motivating failure is NON-REPRODUCIBLE is HELD** (2026-09-02,
  an L3 alias cut withdrawn): the mechanism read from the code and the emission actually measured
  disagreed — `mergeExisting=true` at the write sites READ as "preserves a windows alias into a linux
  run", while the merge is seeded per flavour and re-derives the whole imported-alias section — so a
  275-line filter nothing can exercise shipped on a static census with no dynamic measurement.
  **Measure the path once before building on a flag**, and withdraw the predicate with its census
  kept, which is the warm-design rule paid forward.
  ⚠ **A BOARD ROW THAT SURVIVES ITS OWN REFUTATION costs a fleet a night** (2026-09-06): a row dated
  three weeks earlier said a set of profile bodies does not exist; they exist, they are internal, and
  the row is an un-performed push — measured line by line. **A refuted row takes a DATED amendment the
  day it is refuted, naming the file and line that refutes it.** Its twin: **a RETIRED blocker is not a
  banked row** — the tracer row's stated blocker was retired by an equivalence that DOES reach it, and
  what stands behind it is an honest refusal by name, which is a capability EXCLUSION with a proof
  rather than remaining work.
  **And a CORRECT cut with zero measured payoff is WITHDRAWN, not banked on fidelity** (2026-09-02, the
  `math/bits` hand-own over the BCL intrinsics): three nulls in one arc — the RSA-2048 signature moved
  0.1% (64.59 → 64.65 ms, one variable, the after-assembly proven to carry the intrinsics),
  `hash/maphash` −3.7%, the handshake by construction — against sixteen hand-owned functions that are a
  permanent maintenance obligation. The primitives ARE faster per call (Mul64 5.76 → 3.03 ns, OnesCount
  5.18 → 2.91, RotateLeft 4.18 → 2.62, and Add64 TIED at 4.88 → 4.81 because `UInt128` is not lowered
  to `adc`), yet even the fast form is 6.4x Go's 0.47 ns for what is ONE instruction on both sides — so
  the residual is the emission's call/return, tuple-return and value plumbing, a golib/emission
  question no leaf hand-own can reach. Post the PREDICTION before the numbers and note which kind it
  was: the op-count prediction (2–4x) failed publicly, the one made after a measured mechanism held.
  The withdrawal keeps the census, records the nulls in the file's own header, and leaves the chain on
  the board so nobody re-walks the eliminations — and the arc's LATER, narrower cut (word-size
  Mul/Add/Sub plus one inlining attribute, RSA-2048 66.4 → 20.2 ms measured) is the one that banked,
  because it was cut at the seam the nulls located: the emitted body, not the leaf.
  ⚠ **That withdrawal rule is about a LEAF, and a PREREQUISITE with a measured null is a different
  thing** (2026-09-06, the profile linkname push): the rule was written for a leaf optimisation —
  correct, faster per call, three measured nulls, nothing behind it, a permanent maintenance obligation.
  **A cut that moves ZERO verdicts but sits on the measured critical path of a NAMED remaining blocker
  LANDS instead**, because withdrawing it makes whoever takes that blocker redo it first. State the
  distinction at the ruling, or it reads as a precedent for landing null cuts.
  **Where a rule is PLACED decides which cases can reach it** (measured 2026-09-02, both directions):
  the same rule at a HELPER's arms — after the identity arm has already returned — cannot break an
  identity case, while as a CALLER-side gate it runs ahead of identity and did (0 fixed, 1 broken).
  Put a rule where the cases that must not reach it have already returned. Its retirement half: when a
  rule is spelled once at a helper, a caller-side copy is retired only if it duplicated the RULE — a
  copy that enforces ORDER (`SetMapIndex` checking the key BEFORE its nil-map panic, Go's own sequence)
  is load-bearing and STAYS, with the reason at the site, because the helper answers the question but
  cannot express when the caller must ask it; and each retirement carries the row its copy used to
  catch as a positive control. Mechanically: a REBASE leaves `go2cs.exe` stale (route #1) — rebuild
  before re-transpiling a golden.
  ⚠ **An equality rule that answers by REFERENT when both sides resolve and by NUMBER otherwise has
  an `Equals`/`GetHashCode` contract hole in the ASYMMETRIC case** (2026-09-05, the `MapIndex`
  identity rule): one side resolves, the other does not, and the numbers are equal. **Name the hole
  in the code with the reason it is accepted, guard the symmetric cases, and census the population
  that could see it** (`map[unsafe.Pointer]` keys) — **a hash rule is stated WITH its equality rule,
  never after it.**
  ⚠ **And a pointer-identity rule that answers by REFERENT compares ORDER TOKENS — the allocation
  base plus the Go field offset — never box OBJECTS** (2026-09-05, the same arc's root 4): a field or
  element reference is MINTED AFRESH at every `&l.p`, so two boxes over one field are two objects
  carrying one token, and a `ReferenceEquals`-only rule read `fp == unsafe.Pointer(&l.p)` FALSE where
  Go reads true. Equality is `ReferenceEquals` OR equal tokens, and `GetHashCode` hashes the token —
  one fact, stated together, per the rule above. The `reflect` acceptance could not see it; the FULL
  behavioral suite did, at row 8 of `ManagedAtomicPointer` (650/651) — **the cross-assembly consumer
  gate a golib EQUALITY change owes** (route #7's behavioral twin, above).
  **A cut owes its OWN behavioral guard, and an acceptance table for a row with TWO independent
  failures cannot be built from one of them** (measured 2026-09-02). Three outcomes were enumerated
  for a row that carried a second, unrelated failure and none of them admitted the one that happened —
  "the named failure resolves and the other remains"; enumerate outcomes per FAILURE, not per row.
  Borrowing a lane's roster row as a cut's acceptance test also couples the cut's evidence to that
  row's other defects: it cost a 55-minute run on a restarting host and proved nothing about the cut.
  Pin both acceptance directions in a guard the cut owns — including the direction no consumer
  exercises. ⚠ The same arithmetic runs forward: **a row can be gated on TWO independent defects, and
  that is SAID before an arc lands** — otherwise the row "should have moved" and the arc reads as
  having failed. ⚠ And it runs forward once more (2026-09-06): **a row can be BLOCKED TWICE, and
  clearing one blocker is progress even when the other stands.** A profile row's four capability rungs
  — what the runtime can OBSERVE — and its eight cross-assembly-unreachable linkname destinations are
  different blockers on one row, so "leave that row alone, its rungs are a real frontier" does not
  exclude the mechanical half. **State the acceptance PER BLOCKER**: the downstream row may BANK on the
  push, the blocked row's failure mode must MOVE to the rungs, and if it banks instead then the rung
  reading was wrong.
  ⚠ **An acceptance table that enumerates WHERE a dispatch dies must first ask whether the row dies
  BEFORE dispatch** (2026-09-04): an increment predicted to move a guard from a mute exit 138 to a
  speaking failure read byte-identical on one leg — 138 is 128 + SIGBUS, a signal death that never
  reaches the managed throw path, so no downstream fix can make it print (on the other leg the
  prediction HELD, which is the two-leg scoring rule paid forward). **A mute exit code is the ABSENCE
  of evidence**: the stage that keeps whole stderr is where the evidence is, and it is dispatched
  before a mechanism is argued. The lane posted the falsification FIRST, its hypothesis labelled as
  such and the scope of what it had and had not re-read stated — which is what let the second leg's
  correction land the same day.
  **⚠ AN ARC IS WITHDRAWN ON MEASUREMENT EXACTLY AS A COMMIT IS** (2026-09-03/04). Two increments
  measured zero reduction (the second also uncompilable), the wall was NAMED (a chain pinned by methods
  taking aliasing field addresses, which the correctness fix rightly excludes), and the arc's real
  product became the correctness fix plus a DESIGN RECORD of the measured wall — so the next lane
  starts from a wall that was measured rather than a hypothesis, and a performance arc yields the lane
  to the stated objective when it has no acceptance case left. ⚠ **A design record's increment ORDER is
  a prediction like any other**: a "cheapest-looking first" ordering was falsified by READING the
  mechanism before spending a battery — the target was excluded at the SELECTION stage, its body pinned
  an identity-keyed leaf, the selection fixpoint cascaded that wall upward, and the acceptance chain
  crossed a package boundary in the emission: **zero reachable population on both named targets.** The
  increment is RETIRED with its measurement attached, the record's arithmetic corrected in a DATED
  block, and a boundary the record filed as "awaiting a case" is recognised as HAVING its case the
  moment a row's own bank condition depends on it. **A scoping with no case earns no instrument** — the
  census that would narrow a retired scoping is not built. ⚠ Same shape, three more: three would-be
  hardware-free increments were measured OUT OF EXISTENCE (a dormant caller, an empty class after its
  one member was taken, a remedy whose declarations exist only under another GOOS), each cancelled with
  its measurement attached so no lane re-walks them — and **"a guard that can only run on <platform>
  never runs" is true of STANDING gates and understates a DISPATCH**: a dispatch can MEASURE a
  keystone's payoff even though nothing gates on it, so **measurable-but-not-gated is a real position.**
  A stop-and-post against one's OWN sizing record (a section's "0 new markers" falsified by a function
  being bodied) is the record doing its job.
  ⚠ **A SIZING PROBE THAT STOPS AT THE DOOR IT IS SIZING CANNOT PREDICT WHAT LIES BEHIND IT**
  (2026-09-05, two increments in one week). A "+3 rows PASS" prediction was made from a probe that died
  at its first wall, and every row moved exactly ONE door onto walls the probe could not see: **predict
  the next WALL unless the probe ran THROUGH**, and score a prediction against the CUT's stubs rather
  than the probe's — a synthetic `getcallerpc` in the probe made "past `usleep`" true there and false
  in the cut. Its sibling: **a probe that dies BEFORE its first measurement proves only the wall it
  died on** — the prediction it was to score stays a prediction (UNMEASURED, never "met"), the
  increment lands on its build gates with the acceptance stated as OWED to the NAMED increment that
  opens the wall, and the new wall is sized as its OWN door and ordered by what it opens, ahead of a
  correctness increment that moves no row. And the zero-payoff withdrawal rule above bites an objective
  that is a NUMBER: **a DOOR increment on a capability frontier is banked on the DOOR measurement**
  (the site retired on every row, the next wall named), with the failed pass prediction recorded as
  failed.
  ⚠ **VERIFYING THAT A SITE EXISTS IS NOT VERIFYING THAT A CHANGE IS SAFE — when a comment points at
  its own measurement, READ THE MEASUREMENT.** A coordinator checked that `_ = labels;` was still at
  `pprof_impl.cs:110`, quoted the comment beside it — *"the block beneath this function records the
  measurement that decided it"* — and dispatched the one-line fill without following that sentence
  (2026-09-07). The block records a HOST-KILLING OOM: a finalizer-set label whose map read `len == 1` at
  store and a garbage length at read-back across two collections, sizing a slice in `printCountProfile`
  into an `OutOfMemoryException`; the refusal is recorded in FIVE places at master including a GolibTests
  guard. **A deliberate no-op is the shape MOST likely to carry a documented reason — someone had to
  defend leaving it there.** ⚠ **And an audit can measure what a change BUYS and miss what it COSTS,
  which is half a datum that reads whole**: the sizing correctly moved three census buckets and missed
  that the fill re-arms a measured process-killer, because it read the FUNCTION and not the block
  beneath it — **trading 100 WEAK rows for an intermittent `infrastructure-error` moves the row from
  weak-but-passing to NOT-A-VERDICT-AT-ALL, nondeterministically.** Attach the cost to the sizing
  wherever the sizing is quoted; a benefit figure with no cost figure is not a sizing. ⚠ **When a
  design's precondition is a runtime PROPERTY, measure the property STANDALONE before anyone writes the
  design** — thirty lines minting the suspect pointer, forcing the collections and reading the value
  back decides admissibility with no corpus change and no host risk — and **positive-control it**: the
  arm that must go red is a pointer you EXPECT to move, so a "stable" verdict cannot come from a probe
  that could never observe movement.

  ===== BATCH19 (24bfc8304 / c5e17217b / e9e56b657, merged 2026-09-12) — verbatim =====
  Both are NEW INSTANCES of the warm-design trap, and both SHARPEN it: the first adds "run the falsifier
  you named yourself, first"; the second adds "a true structural finding that does not save the row is
  priced before it is written", plus the per-counter census split. Both clauses are now carried visibly
  in the warm-design bullet.
  ⚠ **A REMEDY SENTENCE PUBLISHED WITH ITS OWN FALSIFIER NAMED IS RETIRED BY THAT FALSIFIER BEFORE
  ANYTHING IS BUILT** (2026-09-08, C2): a lane wrote "the 2a remedy is arm 3's refusal extended to offset
  0" and, three paragraphs later, "falsifier (a): per site, does the test that reaches it pass" — ran it,
  and it fired 8 of 8. Every 2a site PASSES at master, the project had RULED that shape three days
  earlier as the same census's own loud form, so the arm was never an unremedied case and the design was
  WITHDRAWN UNBUILT. Two mechanics: the verdicts were taken CENSUS-OFF (the instrument answers only WHICH
  sites a filter reaches; the verdict of record is the uninstrumented tree's, and the perturbation's
  DIRECTION — an extra `Resolve` cannot manufacture a PASS — is stated rather than assumed), and two
  guessed site-owners were wrong twice with the owner found by measurement. **The warm-design trap
  avoided by the falsifier-first habit, at the cost of one filtered run.**
  ⚠ **THE OBVIOUS REPAIR WAS PRICED BEFORE IT WAS WRITTEN AND IT BUYS NOTHING** (2026-09-08, C1):
  `TestLockOSThreadNesting`'s root is the hand-owned `LockOSThread` family as NO-OPS — one counter has
  ZERO increment sites corpus-wide and the other three, all on the cgo extra-M path we emit OFF (the
  census split PER COUNTER after catching its author's own draft claim) — so Go reads 1,0 where we read
  0,0, and the goroutine's late `Log` then panics through a top-level test constructed with a NULL
  parent. The natural repair (give top-level tests Go's root T so the Log is absorbed) is a TRUE
  corpus-wide structural finding and **does NOT save the row**: Go's own `Fail` panics with no walk, ours
  reproduces it, the host dies one line later and that death is FAITHFUL — only the counter saves the
  row. Prediction on record before any run: accounting the two counters in the four hand-owned bodies
  makes the test a matched pass and moves the wall past 185 WITHOUT clearing it. Two companion
  corrections: the first two stderr lines are ONE guard (a fixed-size allocator asserting a size that was
  NEVER initialised — the unreached-`schedinit` class, not a teardown), and a shipped comment claiming
  honesty "by persistence" is FALSIFIED because the hand-own displaced the incrementing code. NOT
  measured: no build, no run.
-->

## Where a fix goes, and what an increment is worth

- **AN INCREMENT'S ORDER IS A PREDICTION WITH TWO AXES — the reduction AND the FOOTPRINT — and "smallest
  first" reasoned from the reduction alone INVERTS once the footprint is measured.** Measure the footprint
  BEFORE the battery; a large one is AFFORDABLE exactly when a mis-bound site fails LOUDLY at compile;
  REDUCTION and REBIND COUNT are two numbers; **a bound is scored by the MEASURED number AND the REASON.**
  **WHERE A BOX IS FORMED DECIDES WHICH INCREMENT REMOVES IT** — callee body, call site and a package away
  behind a promotion cascade are three increments; name the falsifier ("any THIRD box moving on the row").
- **A prediction is checked against the PREDICTOR'S OWN exclusion clauses before it is posted**, and **every
  emission finding a prediction did not carry becomes an acceptance ROW** — a minted deref-or-null alias must
  not move a nil-receiver panic earlier than Go's, and a lock through a ref-returning `.Value` must contend
  on the SAME mutex, since a by-value copy-lock compiles and never contends. **"RETIRED FOR NO POPULATION" IS
  A CLAIM ABOUT A TREE, NEVER A PERMANENT PROPERTY.** **A design record's STATED remedy is checked against
  the LANGUAGE before an increment is cut against it** (a ref-capturing local function cannot become a
  delegate, CS8175), and **a record's PROJECTED count is re-measured before it is quoted.**
- **SEPARATE THE NAMING SURFACE FROM THE IDENTITY SURFACE BEFORE SIZING A TYPE-ERASURE REMEDY** — threading
  identity changes a PUBLIC generic's SIGNATURE and risks a FALSE GREEN, since a lookup keyed on the carrier
  while the store is keyed on the object returns early through the test's own `if !ok { return }`. **A
  RECOMMENDATION BUILT ON A SHAPE ARGUMENT IS MEASURED ON ITS POPULATION — busiest shape first**, since a
  heuristic can be right BY LUCK on the shape most likely to be asked; **recover a fact the converter DROPPED
  at the LAST site where it is still statically known.** **A zero-cost "never worse" heuristic that the
  durable fix retires the day it lands is THROWAWAY and is declined.**
- **A PER-VALUE CORPUS-WIDE COST IS WEIGHED AGAINST THE POPULATION IT SERVES, by a second derivation**: a
  dims field on `slice<T>` was DEMANDED by its creation sites and PAID FOR by every slice value the corpus
  holds, so **"always right for a caseless row" does not outweigh a permanent tax on the most common type.**
  **A `ConditionalWeakTable` keyed on a slice's BACKING ARRAY is sound only while empty backings are DISTINCT
  objects**: `new T[0]` allocates fresh, `Array.Empty<T>()` is the shared singleton, `make(x, 0)` is where
  golib hands it out, so enforce by SUBSTITUTION in the write path. **A predicate SOUND until a new backing
  existed becomes a trap the day the backing arrives** — `ElemRefBox` read `m_array is not null` and took a
  NATIVE-backed slice's EMPTY managed array as the backing: **use the type's OWN predicate (`IsNativeBacked`),
  treat a new-kind guard going RED on OLD code as the guard's second job, and SPLIT a live master defect into
  its own seat.** **An "expected-today" guard row lives only in a harness that can STATE an expected value
  (GolibTests)**, since a stdout-compared behavioral project reds whole on any row differing from Go.
- **CANDIDATES ARE RE-SCORED AFTER THE ROUTE IS CHOSEN.** Beside it: **a table with `GetOrAdd` and NO removal
  path is a per-process accumulation defect a redesign should RETIRE, never re-key**; **a per-value cost
  bounded by an EXTERNAL population is a different rule from the corpus-wide per-box byte rule**; **a sizing
  that omits the LOAD-BEARING step is a HOLD, not a footnote**; a capability serving a handful of sites is a
  CANDIDATE, not built. **Before dissolving a wall, measure what the wall is MADE OF** — an identity boundary
  can be a property of the PORT's representation rather than the source semantics, measured by the falsifier
  that would retire it and cut with a concurrency guard proven RED first.
- **THE ORDER OF A FIX IS LOAD-BEARING, and a fix at the wrong layer reads as "the fix does not work"** —
  populate first, then thread. **Read the site's own comment before editing the line beneath it**: the line
  may be a documented REFUSAL, and skipping that passes a nine-shape guard while breaking a banked consumer's
  IDENTITY. **A guard that prints NAMES must also assert IDENTITY where identity is the contract**; **a model
  fix is checked for its EMISSION TWIN before it is scoped as golib-only**, the twin's gates (converter
  suite, two-seeded diff by hunk, CNR with predicted golden drift) joining the increment; and **a consumer's
  exposure is READ from its first substantive line, never guessed from its name.**
- **READ THE FLAG-OFF EMISSION OF THE SAME SITE before cutting a fix at a call-site ARM** — an arm-level fix
  compiled while making the site WORSE than flag-off. **A wrong classification is corrected where it is MADE,
  never papered at the arm.** **A "perf" item is re-measured for CORRECTNESS against `go run` before it is
  priced** — an allocation-hygiene item was a Go-SEMANTICS divergence, `for i, v := range a` over an array
  VALUE observing the body's own writes. **A three-row filtered acceptance cannot falsify "no OTHER row
  moved"**, and **a predicted-then-confirmed baseline is still MEASURED once.**
- **Where a rule is PLACED decides which cases can reach it** — at a HELPER's arms, after the identity arm
  returned, it cannot break an identity case; as a CALLER-side gate it runs ahead of identity and did. **A
  caller-side copy that enforces ORDER** (`SetMapIndex` checking the key BEFORE its nil-map panic) **is
  load-bearing and STAYS.** **A hash rule is stated WITH its equality rule, never after it**, and **a
  pointer-identity rule that answers by REFERENT compares ORDER TOKENS — allocation base plus Go field offset
  — never box OBJECTS**, since a field reference is MINTED AFRESH at every `&l.p`: equality is
  `ReferenceEquals` OR equal tokens, `GetHashCode` hashes the token. **A golib equality change owes a
  CROSS-ASSEMBLY CONSUMER GATE.** **A RULED FIX'S CAVEAT IS NARROWED TO A NAMED RESIDUAL BY READING THE
  CONTRACT, then discharged by an ARM rather than a doc comment.**
- **WHEN SIZING A CORPUS-WIDE BEHAVIOUR CHANGE, PICK THE DEFAULT THAT MAKES THE ARC MONOTONIC** —
  default-FATAL for unimplemented stubs means every increment can only convert a host death into a reported
  verdict and never the reverse: landable one package at a time, safe to stop between any two. **A sizing
  whose author picks the harder default against their own convenience does not need second-guessing.** **PUT
  THE DECISION WHERE THE KNOWLEDGE IS**: a stub's kind is SEMANTIC and a structural predicate provably cannot
  recover it — **the converter knows what the symbol IS, the generator knows only what it LOOKS LIKE.** **For
  a change that makes failures RECOVERABLE the acceptance criterion is the FALSE-GREEN direction, named**: a
  test that stops dying and starts FAILING is the point, while one that starts PASSING may be passing on a
  RECOVERED missing capability. **A REBASE leaves `go2cs.exe` stale (route #1) — rebuild before
  re-transpiling a golden.**

<!-- DERIVATIONS (placement, ordering, increments, sizing defaults) — Phase 1 text, verbatim:
  **⚠ AN INCREMENT'S ORDER IS A PREDICTION WITH TWO AXES — the reduction AND the FOOTPRINT — and
  "smallest first" reasoned from the reduction alone INVERTS the moment the footprint is measured**
  (2026-09-04, sharpening the ordering rule above). "One box on the os row" was a property of the
  measured ROW, while the RULE the increment needed — publishing a ref primary for the corpus's
  most-used lock — rebinds every boxed call on a ref-addressable base: ~667 sites in 73 files, ~500
  even scoped to the increment's own name. The cut chosen as the cheap first exercise of a new
  contract was the EXPENSIVE one, and the CONTAINED cut went first. **Measure the footprint BEFORE the
  battery, not after.** A large footprint is AFFORDABLE exactly when a mis-bound site fails LOUDLY at
  compile (the multi-target build becomes the load-bearing gate and the predicted path set the
  falsifier); a cut's REDUCTION and its REBIND COUNT are two different numbers, stated separately —
  one is an alloc-row acceptance, the other a count of call sites; and **a bound stated as a bound is
  scored by the MEASURED number AND the measured REASON** (667 became 365 because the binding
  condition — a ref-lvalue base and the specific callee — decides, not the callee count).
  **⚠ WHERE A BOX IS FORMED DECIDES WHICH INCREMENT REMOVES IT, and the prediction is corrected on
  that reading BEFORE the cut** (2026-09-04): of five seam boxes behind one boundary, two are formed
  inside the callee's OWN body (a ref-receiver hand-own of the callee removes them), two at the
  CALLERS' call sites (a converter call-site rule removes them), and one a package away behind a
  promotion cascade — so the hand-own increment's honest deliverable is two boxes PLUS a PRECONDITION
  (the callee made promotable), and the general increment collects the rest for the whole corpus. The
  tempting extension — hand-own the six tiny callers to reach four of five — takes a file from two
  hand-owned functions to eight of eleven: a whole-file hand-own in disguise, declined by the
  minimal-footprint rule because the general rule collects those same boxes across 73 files at once.
  The falsifier for the split is **"any THIRD box moving on the row"**. Two companions: a prediction is
  checked against the PREDICTOR'S OWN exclusion clauses before it is posted (one lane wrote both
  constraints and then predicted across them; the count falsifier fired exactly as written, and the
  miss LOCATED the remaining boxes instead of leaving them unexplained), and **every emission finding a
  prediction did not carry becomes an acceptance ROW** (a minted deref-or-null entry alias must not
  move a nil-receiver panic earlier than Go's; a lock through a ref-returning `.Value` on a raw box
  must contend on the SAME mutex, since a by-value copy-lock compiles and never contends).
  **⚠ "RETIRED FOR NO POPULATION" IS A CLAIM ABOUT A TREE, NEVER A PERMANENT PROPERTY** — the
  ruling-SCOPE rule's twin (2026-09-04): an increment retired on the measurement that a lock's method
  could never take a `ref` receiver was re-opened by the LATER increment that MADE it one and created
  exactly the population the retirement said could not exist. **The increment that changes a
  retirement's premise re-opens it, with a dated amendment rather than a rewrite.** Beside it: **a
  design record's STATED remedy is checked against the LANGUAGE before an increment is cut against
  it** — "the deferred call emitted as a local function taking `ref` to the frame's state" had been
  carried since the record was cut and is not expressible (a ref-capturing local function cannot
  become a delegate, CS8175, and the frame stores a delegate), so the obstruction is the FRAME'S
  STORAGE and the candidate is retired IN the record with that sentence rather than quietly rewritten.
  ⚠ **SEPARATE THE NAMING SURFACE FROM THE IDENTITY SURFACE BEFORE SIZING A TYPE-ERASURE REMEDY**
  (2026-09-05, the `unique` blocker): a descriptor READ FOR ITS NAME and a descriptor USED AS A MAP KEY
  or compared are two populations, and a Stage-B sizing (a call-graph fixed point plus generic-
  signature churn) collapsed to ONE increment once five bare-type-parameter `TypeFor[T]` sites split
  into two naming reads and three identity uses. Threading identity would have changed a PUBLIC
  generic's SIGNATURE — moving banked consumers — AND risked a FALSE GREEN, since a lookup keyed on
  the carrier while the store is keyed on the object finds nothing and returns early through the test's
  own `if !ok { return }`, turning two subtests green for no reason. And **a record's PROJECTED count
  is re-measured before it is quoted**: intervening arcs had already closed the larger half of a
  "7 of 20".
  **⚠ A RECOMMENDATION BUILT ON A SHAPE ARGUMENT IS MEASURED ON ITS POPULATION — busiest shape first**
  (2026-09-03): a per-package registry looked right ("a package declares one `[][N]T` per element
  type") until the stdlib census showed `byte`/`uint8` carrying three lengths, so the heuristic was
  right BY LUCK on exactly the shape most likely to be asked; withdrawn by its author before the
  ruling. Its constructive half: **recover a fact the converter DROPPED at the LAST site where it is
  still statically known**, rather than by observation downstream — and a boundary with no REACHING
  case is RECORDED in the design record, never built ahead of its case. (A "complementary piece" that
  shares the same reach gap is DOMINATED, not complementary; sizing it was the warm-design trap and was
  retracted.) ⚠ **A zero-cost "never worse" heuristic that the durable fix retires the day it lands is
  THROWAWAY and is declined even though it would buy the row back sooner** — the row is carried instead
  as a NAMED known red on the union battery until the durable cut seats.
  **⚠ A PER-VALUE CORPUS-WIDE COST IS WEIGHED AGAINST THE POPULATION IT SERVES, by a second
  derivation** (2026-09-04, the `ж<T>` byte rule's slice sibling): a dims field on `slice<T>`
  (40 → 48 B, +20% on every slice value the corpus holds) was DEMANDED by 130 creation sites and PAID
  FOR by 27,143, and over a zero-byte side table it bought exactly one row class — the nil slice — for
  which no reaching case exists in the corpus or the roster. **"Always right for a caseless row" does
  not outweigh a permanent tax on the most common type**; the ruling took the zero-byte form with the
  boundary RECORDED and the field named as its remedy, and the guard rows kept as *expected-today* rows
  that flip when a case earns the field. This is DISTINCT from declining a heuristic that was right by
  LUCK: the side table is right by construction on every measured site, so the deciding criterion —
  correctness on what exists — is met at zero cost. ⚠ Two mechanics from the same cut. **A
  `ConditionalWeakTable` keyed on a slice's BACKING ARRAY is sound only while empty backings are
  DISTINCT objects** — `new T[0]` allocates fresh (measured), `Array.Empty<T>()` is the shared
  singleton, and `make(x, 0)` is exactly where golib hands out the singleton — so the rule is enforced
  by SUBSTITUTION in the write path, never by an assertion that would turn a legal Go program into a
  runtime throw (the assertion is a test-time guard with a positive control that feeds the singleton,
  plus a census that the emission never spells `Array.Empty` at a creation site).
  ⚠ **The same singleton in a NEW costume, and the rule that generalises it** (2026-09-05): a
  predicate SOUND until a new backing existed becomes a trap the day the backing arrives — `ElemRefBox`
  read `m_array is not null` and took a NATIVE-backed slice's EMPTY managed array as the backing
  (`IndexOutOfRange` on every element), and because `[]` is the shared `Array.Empty<T>()` singleton,
  `Canonical()` would have given EVERY native block in the process ONE identity: the CWT
  pointer-identity hazard again. **Use the type's OWN predicate** (`IsNativeBacked`), **treat a
  new-kind guard going RED on OLD code as the guard's second job**, and SPLIT a live master defect
  found that way into its own seat with its own control rather than riding the increment that found it.
  And **an "expected-today" guard row lives only in a harness that can STATE an expected value
  (GolibTests)** —
  in a stdout-compared behavioral project any row that differs from Go reds the whole project, straight
  into the full-behavioral leg built to catch red projects; the behavioral guard carries the rows that
  must be green plus a documented non-printing block for the boundary.
  **⚠ CANDIDATES ARE RE-SCORED AFTER THE ROUTE IS CHOSEN**, because a route changes the premises the
  others were scored on (2026-09-04): a candidate scored 0 "because the callee receives only the
  primitive, never the containing struct" revived and won once the chosen route displaced the CALLER as
  a hand-own that holds the struct. Two axes from it generalise: **a table with `GetOrAdd` and NO
  removal path is a per-process accumulation defect a redesign should RETIRE, never re-key** (a gate
  that lives and dies with its struct has no table); and **a per-value cost bounded by an EXTERNAL
  population is a different rule from the corpus-wide per-box byte rule** and is stated in its own
  terms. Companions from the same hold: a sizing that omits the LOAD-BEARING step is a HOLD, not a
  footnote; a converter capability serving 16 sites is recorded as a CANDIDATE, not built; and a "twin"
  fix with zero production population is dropped as a reduction and named as uniformity work. ⚠ And
  **before dissolving a wall, measure what the wall is MADE OF**: an identity boundary can be a property
  of the PORT's representation rather than of the source semantics — a table keyed on a box because the
  box carried "same field of same object" dissolved once the underlying word was measured to be DEAD
  STORAGE in the port, free to BECOME the identity as a lazily CAS-assigned handle. That is measured by
  the falsifier that would retire it (any read of the word as a VALUE, anywhere in source or corpus)
  and cut with a concurrency guard proven RED first, its one semantic divergence stated in the record.
  **⚠ THE ORDER OF A FIX IS LOAD-BEARING, and a fix at the wrong layer reads as "the fix does not
  work"** (2026-09-03): a root moved from the renderer to the container CONSTRUCTOR, and a renderer
  fixed FIRST threads a null, changes nothing observable, and sends the next reader into the layer that
  is already correct — populate first, then thread. It moved a THIRD time, into a DELIBERATE, documented
  REFUSAL, which is why **you read the site's own comment before editing the line beneath it**: an
  increment that skipped that step would have passed a nine-shape guard and broken a banked consumer's
  IDENTITY that nothing in the gate list measures. Three companions. **A guard that prints NAMES must
  also assert IDENTITY where identity is the contract.** **A model fix is checked for its EMISSION TWIN
  before it is scoped as golib-only** — the same element positions the runtime model dropped were also
  unwalked by the converter's own stamping pass, and the twin's gates (converter suite, two-seeded diff
  by hunk, CNR with predicted golden drift) join the increment. And an increment can be **three halves
  in a FORCED ORDER**, any one or two of which produce nothing observable — so **re-reason a tested
  decision line by line** rather than deleting a line that was right when written (an existing row
  asserting "slice of arrays emits nothing" was DELIBERATE and correct under the old accessor). ⚠ **A
  consumer's exposure is READ from its first substantive line, never guessed from its name** — a "probably
  not a consumer" guess was wrong twice in one arc.
  **⚠ READ THE FLAG-OFF EMISSION OF THE SAME SITE before cutting a fix at a call-site ARM**
  (2026-09-03): a classification pass had demoted a local's box by weighing its RECEIVER-use and missing
  its RESULT-use, and the arm-level fix compiled while making the site WORSE than flag-off (7 boxes
  against 1). **A wrong classification is corrected where it is MADE, never papered at the arm**, and
  the rejected form is written at the site as measured-wrong-by-reading. ⚠ Two neighbours: **a
  predicted-then-confirmed baseline is still MEASURED once**, because it is the before-arm of the next
  increment's delta; and **a "perf" item is re-measured for CORRECTNESS against `go run` before it is
  priced** — a backlog item filed as allocation hygiene was a Go-SEMANTICS divergence (`for i, v := range a`
  over an array VALUE observed the body's own writes, seven of seven shapes), which is why no sweep
  caught it: the copy belongs to the range EXPRESSION where `gc` puts it, scoped exactly as `gc` scopes
  it, and that is what then lets the enumerator be cheap. ⚠ And **a three-row filtered acceptance cannot
  falsify "no OTHER row moved"** — say so, and let the union gate carry it.
  **⚠ WHEN SIZING A CORPUS-WIDE BEHAVIOUR CHANGE, PICK THE DEFAULT THAT MAKES THE ARC MONOTONIC**
  (2026-09-06) — the default changes the arc's SHAPE, not just its safety. Default-FATAL for
  unimplemented stubs (list the CAPABILITY ones, leave everything else exactly as today) means every
  increment can only convert a host death into a reported verdict and never the reverse: no
  full-roster blast-radius gate before the first landing, landable one package at a time with a
  per-increment row measurement, and safe to stop between any two — where the opposite default would
  have shipped 24 unclassified stubs with a kind nobody decided. **A sizing whose author picks the
  harder default against their own convenience is one that does not need second-guessing.** Two rules
  ride with it. **PUT THE DECISION WHERE THE KNOWLEDGE IS**: a stub's kind (capability versus
  memory-moving / address-returning / atomic) is SEMANTIC and a structural predicate provably cannot
  recover it — measured, not argued, since the unsafe thirteen return void, bool and a pointer so no
  return-type rule spans them, while "takes an unsafe pointer" sweeps in the 140 capability stubs that
  must stay recoverable — and a curated symbol table inside a Roslyn analyzer is the OTHER wrong home:
  **the converter knows what the symbol IS, the generator knows only what it LOOKS LIKE, so the
  converter stamps an attribute and the generator reads it.** And **for a change that makes failures
  RECOVERABLE the acceptance criterion is the FALSE-GREEN direction, named**: a test that stops dying
  and starts FAILING is the point, while a test that stops dying and starts PASSING may be passing on
  a RECOVERED missing capability — the only way such an arc can do damage — so each increment's
  measurement rules that out explicitly rather than reporting a net verdict improvement.
-->
