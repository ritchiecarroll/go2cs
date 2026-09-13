---
name: validation-bank
description: Bank a validated package row, mint or judge a disclosure, or read the roster arithmetic. Includes the exclusion bar E1-E4, the alloc-assert meter rules, and the banked-row merge protections.
---

# Validation Bank

<!-- EXTRACTED VERBATIM from CLAUDE.md at 1800b04f8, lines 2899-3114, 4579-5388.
     Phase 1 of docs/PLAN-context-diet.md: RELOCATION ONLY, no compression, no edits.
     The byte-identical original is docs/doctrine/JOURNAL-2026-09-12.md.
     Phase 2 distills this file. Keep every dated derivation, but move it into an HTML
     comment beside the rule it justifies: comments are stripped before this file enters
     context, so provenance kept this way costs ZERO tokens.

     PHASE 2 DONE 2026-09-12: 1036 effective lines -> ~250, visible bytes 100,522 -> ~28,000.
     Every dated narrative lives in a comment attached to the rule it justifies; a comment block
     opens at the end of its rule's last line and closes at the end of its own last line, so a
     stripped read costs no residual line. Nothing was deleted -- the pre-distillation text is
     this file at 56ff452a5 and, byte identical, in the journal. Roughly 120 distinct normative
     rules survived; the normative structures (the canary predicate, the three displacement
     mechanisms, the alloc-meter labels, the E1-E4 bar, the four post-sweep dirt classes) are
     kept visible and intact, and only narrative around them was compressed.

     BATCH19 MERGED 2026-09-12: the doctrine items routed here from
     origin/claude/coord-doctrine-batch19 (commits 24bfc8304 / c5e17217b / e9e56b657, items
     1154-1321, anchored at CLAUDE.md@44f858717 lines 2975/2983/3000/3102/4669/4747/4824/4864/
     5214/5279/5373) were folded in: most as further evidence inside an existing rule's comment,
     several as amendments to a visible rule, and the genuinely new traps as new rules. That
     branch was never merged -- it was written as pure insertions into the pre-split CLAUDE.md,
     whose shape no longer exists. -->

## Banking a row, and protecting it at merge time
- **A lane's sweep proof binds its OWN tree, never the merge result** — each side green alone, RED the moment the
  merge lands; a banking merge owes `run-validated-sweep.ps1 -Filter <pkg>` at the merge RESULT. <!--
  Paid for by the crypto/tls regression, found 2026-08-19, rooted and fixed by lane claude/tls-regression. The
  flagship row banked green on its lane tip and was RED at master the moment its merge landed: the guilty change
  d1ed1f7c1 (local-iface-cast) had merged to master AFTER the lane forked. Each side green alone, the union never
  swept. The lane-tip proof is necessary, never sufficient. -->
- **A reflect-bridge-touching change's canaries are the FIVE largest banked reflect consumers BY VERDICT COUNT,
  recomputed from [`docs/ValidatedTestPackages.md`](docs/ValidatedTestPackages.md) at gate time and never carried
  forward.** PREDICATE: the package's own Go source — production OR `_test.go` — imports `reflect`, read from
  PARSED import declarations; positive-control every time (`encoding/json` IN, `cmp` OUT, `go/doc/comment` OUT).
  **The worked example is RETIRED — the derivation IS the rule.** <!--
  Test usage counts because the canary protects VERDICTS and verdicts are produced by test code; a suite leaning
  on reflect.DeepEqual is exactly what a bridge regression breaks (clause explicit 2026-08-29). A fresh top five
  is DATED data for the BOARD, never for this file. 2026-08-29 derivation: crypto/tls 3,643 (bogo-capable hosts
  only; the collapsed-verdict path otherwise), go/types 557, encoding/json 491, encoding/xml 386, crypto/x509 341.
  DRIFT 1: the example included go/internal/gcimporter (583) -- ZERO reflect touchpoints in prod or test --
  carried across several merge windows with only the counts re-read, until a lane's fresh grep caught it while
  holding an expensive sweep. The example had substituted for the derivation, this time performed by the
  coordinator.
  DRIFT 2 (2026-09-01): crypto/internal/nistec (2,195) also imports reflect nowhere; it had travelled beside
  gcimporter and was carried again the day before a fresh derivation dropped both.
  DRIFT 3 (2026-09-03), which retired the example: crypto/tls 3,643, net/http 1,343, go/types 557, encoding/json
  491, net 472 -- net/http and net outranking the two rows the example named.
  Same day the derivation sharpened: a line-anchored grep admitted go/doc/comment and go/internal/gccgoimporter
  while BOTH standing controls passed -- both vary "imports it or not", neither varies "mentions it as DATA"
  (go/doc/comment's std.go carries the name as data). Add the control that pins the axis the counterexample
  names. -->
- **"Reflect-bridge-touching" reads broadly** — `src/core/reflect/*_impl.cs`, `src/core/internal/reflectlite`,
  golib `GoReflect.*`/adapter/equality, go2cs-gen adapter/shell templates — **and the gate list carries the FULL
  behavioral suite**: a byte-identical `.cs` is invisible to CNR and the `reflect` `-tests` build alike. <!--
  Route #7's behavioral twin. -->
- **The canary rule is SPLIT**: a BRIDGE change takes the importer canaries; an `abi.synthType` / golib
  descriptor-synthesis change takes a **COST canary too** (`crypto/internal/nistec`), judged on **WALL TIME**. <!--
  R's proposal, ratified 2026-09-01. Synthesis runs on every interface boxing corpus-wide, a blast radius the
  importer predicate cannot see at all. An unmemoized GoPtrBytesOf pushed nistec from 354s past its 600s deadline;
  the memoized re-measure is 384s. Compare against the recorded baseline, not just the verdict. -->
- **Choose a canary by MECHANISM, not rank, when a row keys on the thing the change alters**: green WITH the
  defect present, breakable BY THE REPAIR. <!--
  Measured 2026-09-03: encoding/gob keys its type caches DIRECTLY on reflect.Type identity and is banked at 106
  green WITH a [][6]uint8/[][8]uint8 identity collapse present -- its suite never exercises the collapsing shapes,
  so green is not evidence the model is sound. Generalises the promoted-forwarder / net/http precedent. -->
- **A datum the MARKER loses rides as a SIBLING attribute, never in the marker's SPELLING**; synthesis fills the
  cargo AHEAD of the interning key, and a measured cargo disagreeing with the STAMP is refused BY NAME. <!--
  2026-09-05. Where a marker's TEXT is what go2cs-gen dispatches on ([GoType("chan …")], [GoType("ж<…>")]), a
  dropped datum -- a defined channel type's DIRECTION, a named pointer-to-array's LENGTH -- is stamped beside the
  UNTOUCHED marker and read once per type (the GoArrayDims/GoMapKeyDims family, extended from values to a defined
  type). Changing the spelling instead teaches the generator a new prefix and buys a test-only population a
  route-#7 ladder. Filling ahead of the key makes every route intern ONE descriptor. Descriptor synthesis takes
  the cost canary above. -->

## Reading refs, clones and trees
- **Derive any count in a clone whose refs you VERIFIED, and reconcile it against an independent derivation.** <!--
  2026-09-02: a derivation run with the mailbox clone as cwd read origin/master 15 rows behind and produced the
  SAME top five by luck (every dropped row was smaller); only a row-count reconciliation, 178 against the guard's
  193, caught it. -->
- **The mailbox clone is TRANSPORT — its refspec carries `claude/mailbox` ALONE, so `git fetch origin master`
  there moves NOTHING**; read content from a work tree, let `ls-remote` arbitrate, and assert the refspec in the
  lane's SCRIPT before any fetch-based arm. <!--
  Mechanism stated 2026-09-02 after a second instance; its origin/master stays pinned wherever the clone was made,
  and one such read was 15 rows behind and nearly escalated as "fifteen banked Linux rows lost". Met again
  2026-09-03 as a DEAD MONITOR ARM: a watcher's MASTER-CHANGED leg ran in the mailbox clone, so the arm was
  structurally dead the whole time it reported healthy. -->
- **Name the REF you read** (`git fetch origin master && git show origin/master:<path>`); **after a fetch that
  PRINTED AN ERROR verify the ref MOVED before reading off it**; **an ANCESTRY question goes to a clone that HAS
  the ancestry**; **and a SHALLOW clone DISQUALIFIES its host for any test that seeds an origin — charge that red
  to the CLONE, not to the tree.** <!--
  All three, 2026-09-02. A branch's base is a snapshot of master at fork time and ages out from under every claim
  made through it -- the stale-base illusion applied to a single FILE rather than a diff. A fetch dying on a
  clone's object corruption left origin/master unmoved, `git show origin/master:<path>` answered about the past
  while looking like the present, and "master is RED on the roster guard" was reported -- falsely; "benign for
  pushes" (they verify the remote moved) is not "benign for reads". A depth-200 shallow clone answered "NOT on
  the remote" for a ref the full-history repo showed contained, and it is the clone a lane reaches for by habit.
  SHALLOW-CLONE fourth clause added 2026-09-08 (C2), a different failure mode from the ancestry one: converter-suite
  TestSafePushSelfTest ABORTS on `shallow update not allowed` while seeding its hermetic origin -- identical at the
  announced base, green on a full clone in 187 s -- so a cloud-container lane attributes that ONE converter-suite red
  to its clone before hunting a tree defect. A host-QUALIFICATION fact, not a tree fact. Two companions from the same
  lane: `go test` launched from a worktree ROOT with no go.mod exits 1, and an empty grep of THAT output reads as an
  answer when it is an instrument miss; and a background task's reported exit code is the WRAPPER's last pipe stage,
  not the suite's -- the safety floor's capture-the-exit-code-before-any-pipe rule met from the task-runner side. -->

## Arms, loads and confounds
- **THE ARMS MUST MATCH ON LOAD — AND ON WARMTH, WHICH IS THE SAME RULE: a battery's FIRST LEG and a SOLO re-run
  are not two arms of one experiment, and a COLD base arm against WARM cut arms is not one either** — the deciding
  arm runs ALONE, matching the CLEAN arm's load, and **a brief that orders `base ×1, cut ×3` is DEVIATED from (add
  the missing base runs) and the deviation STATED, not hidden.** <!--
  Fourth clause, measured 2026-09-06; two arms NEARLY convicted a sound seat. All three arms of one row:
  seat-present under battery load FAIL 1325/0/20 in 890 s; seat-absent ALONE PASS 1345/0/0 in 351 s; seat-present
  ALONE PASS 1345/0/0 in 348 s. 348 against 351 is the same host doing the same work; 890 is a different machine
  in every way that matters to a cancellation-timing suite, and sixteen of the twenty diverging rows were
  cancellation and retry TIMING.
  WARMTH clause added 2026-09-08, the same rule met on a second axis: a coordinator brief specifying "base x1, cut x3"
  left the BASE arm COLD against warm cut arms, and warmth DOMINATES a sweep wall -- 100 s cold against 61 s warm on
  the SAME row, larger than any effect being measured. The agent added base r2/r3 UNASKED and reported the
  matched-warmth pair (61/61 against 61/62 s) with six byte-identical comparison records. The coordinator's brief was
  wrong; the deviation was STATED rather than silently followed or silently ignored. -->
- **A WALL-TIME GAP BETWEEN ARMS IS A TELL, NOT A CURIOSITY**; and **declining a regression call you are entitled
  to make is harder than making it, because the call looks like rigour** — name the confound BEFORE the deciding
  arm, state both outcomes in advance, refuse to predict. <!--
  890 vs 351 seconds was the only thing separating a real regression from a timing suite flaking under battery
  load, and it is exactly the kind of number that gets noted and skipped -- especially when one candidate mechanism
  is constructible and the other is not, and the confound is sitting in the wall clock. The cost of the call not
  made would have been a sound seat pulled, a phantom defect handed to somebody to chase, and an unconstructible
  mechanism pursued until found to be nothing. -->

## When a reading expires
- **A GATE READING HAS A TREE; when the tree moves the reading expires whether or not the change does** —
  re-gate, stating expected numbers as a PREDICTION first. **A ROSTER figure names a tree and a DAY; a sentence
  about "this tree" names a RUN** — citing a banked verdict count as a property of the tree under discussion is an
  unmeasured premise, and it is corrected in the ruling's REASON before the premise is tested, not after. <!--
  2026-09-06: a seat measured 323/58/7 against a manifest a later landing rewrote still MERGES clean -- verified by
  3-way -- but the reading describes no tree that exists. The stale-base illusion applied to MEASUREMENTS rather
  than diffs, arriving from both directions: a base stale BEFORE the train, or a train that makes the reading
  stale after. Predicting first stops the new reading being a rationalisation.
  ROSTER-FIGURE clause 2026-09-08 (coordinator): a ruling argued "inside a banked row at 3,643 verdicts PASS on that
  same tree" -- which quoted the ROSTER, i.e. another day and another host, and on the i7 a standing "not measurable
  here" -- while the lane's own run on the tree in question read FAIL with the census ON and the OFF control
  unfinished. The lane caught it before the control landed and named the three possible outcomes; the CONCLUSION
  survived because it rested on other evidence, and the REASON was corrected before the premise was tested. -->
- **The overlap test is against what the reading is a PROPERTY OF, not the seat's files: a file-scoped reading
  transfers on zero file overlap; an AGGREGATE expires at ANY landing** — ask it of the seat's PAYLOAD too. <!--
  2026-09-06: a seat whose files did not intersect a landing at all still carried an expired gate line, because
  that line quoted a SUITE TOTAL: 728 declared at the seat's base, 738 at the landed master after a train added
  ten GolibTests methods, 745 predicted before the re-gate and hit by both legs. A byte table IS the claim its
  commit exists to make, and a lane checking gate lines alone re-gated one aggregate of four and reported the
  seat re-gated. -->
- **The expiry class that expires SILENTLY is an alloc or byte reading** — the row still passes while the figure
  is no longer true of any tree. Check `git diff --numstat <base>..<master> -- src/core/golib/`; RE-MEASURE
  before re-stamping. <!--
  2026-09-06. A suite total and a manifest composition both fail LOUDLY; a byte figure does not. Verified at the
  train-31 landing: src/core/golib/ moved +301/-6 across 5 files with ж.cs among them, and the Architecture map
  already rules instance state on ж<T> a CORPUS-WIDE byte cost -- so any B/op, allocs/op or want-N figure taken
  before that landing expired regardless of which files its seat touched. A re-stamp is the act that makes a wrong
  figure look freshly verified, and a reading's loudness is independent of its importance. -->
- **A RATIO expires more readily than an absolute** — two tree-dependencies plus a silent claim that its halves
  share one box, one tree, one configuration. **Publish the absolute with its BOX and its TREE**, and **per ROW with
  THAT row's own base named**; correct by MEASUREMENT, never a third estimate. **Where the single-process spread
  exceeds the delta, only a FLOOR over processes separates signal from drift.** <!--
  A cost published as "+26.075s = +12.1%" survived a landing only in its wall-clock half (the converter-suite
  baseline moved 215s -> 344s, making the fraction 7.6%), and "+26.075s on a laptop / 344.012s on the i7 = 7.6%"
  replaced a stale number with an INCOMPARABLE one: the two machine classes differ 3-4x, so the 215->344 gap may
  be entirely the boxes, entirely 53 commits of tree growth, or any mixture, and nothing in either post can
  apportion it. A fraction whose halves cross either boundary is not a weaker figure, it is not a figure. A number
  defended is worth less than a number re-measured, even when defending it would have been vindicated.
  PER-ROW/FLOOR clauses 2026-09-08, from the warmth-pair measurement above: per-row door cost came out at
  +4.3..+13.7 ns against per-row bases of 151-200 ns, and the SINGLE-PROCESS spread often exceeded the delta itself,
  so a per-row absolute with its own named base plus a floor taken over processes is the only reading that survives
  -- a pooled percentage across rows with different bases would have manufactured a figure out of the spread. -->
- **A micro-bench can INVERT between hosts on the tiering axis, and a percentage against different anchors is not
  a comparison** — materiality is decided by a real kernel transition on the BINDING host. <!--
  2026-09-08: a door cost read within the noise floor at tiering OFF on both hosts, and clearly positive on one
  host under DEFAULT tiering where the other read slightly negative -- the sign FLIPPED. Half of it is rooted
  (every arm compiles as tier-1 on-stack replacement with synthesized PGO, since a ten-million-iteration loop
  inside a method called a handful of times is promoted by OSR; forcing full optimization removes OSR and halves
  the gap); the residual is labelled UNROOTED with the un-varied axes named. The probe's "percentage of the lower
  bound" line is an ANCHOR ARTIFACT -- one host's anchor is a user-mode read an order of magnitude cheaper than
  the other's real kernel transition, so cross-host percentages from one probe are invalid. -->
- **Correcting an estimate against each objection in turn is NOT deriving it**: on a third correction in the same
  direction the defect is the DERIVATION — re-derive or say UNKNOWN. **After amending a figure in a multi-section
  record, grep the WHOLE file for the old value.** <!--
  A census headline went 93 -> 92 -> 87 -> UNKNOWN across one evening, every correction sound against the objection
  that prompted it while inheriting the untested assumptions nobody had raised yet: "87 = 93 - 6" subtracted the
  measured departures and silently assumed every other package lost nothing -- false on its face, since one package
  held 32 of the 93 and its emission had changed. The third correction is where the author declared the headline
  UNKNOWN, the first sound statement in the sequence; the settling three-target build then returned 87. The
  author's own verdict: "87 was RIGHT and publishing it earlier was still WRONG." A correct number reached by an
  unsound method is not evidence, and being vindicated later does not retroactively make the earlier claim a
  measurement. Filing companion: twice in one day a headline's correct value sat two sections above the paragraph
  being written, both written by the person who had just derived the right number -- neither author was careless,
  they were LOCAL, and the contradiction is never in the section you just wrote. -->
- **"STALE" implies a recoverable provenance; a figure OFF THE LINE has none** — SUPERSEDE it with an attributable
  re-run (tree, flavour, host, configuration, base). Tell: **sibling seats' figures that do not COMPOSE are two
  different trees** — name both arms' SHAs. <!--
  A claimed "689 declared" sat under merge-base 721, the sibling seat's 725, its own blob's 731 and master's 732 --
  a tree with that count predates the branch entirely. A stale reading is corrected in a sentence; an
  unattributable one HOLDS the seat. Recovering the provenance of a figure nobody can place is unbounded work;
  replacing it is one run.
  Second instance 2026-09-08, the arithmetic doing the same work in the other direction: a lane carrying GolibTests
  readings for a seat with no log behind them FOUND logs reading 709 and nearly published them as a correction --
  they were ANOTHER arc's tree. 709 is not an admissible total for this tree (739 is), and that is what settled it;
  both legs were then re-run on the seat tree and landed where the carried figures said. The log's EXISTENCE proved
  nothing about which tree wrote it; the tree NAME did. Corollary for any figure quoted from a scratch log: a matching
  number is still unevidenced until the tree it was taken on is named. -->
- **A "does X exist" predicate answers about the tree it RUNS ON — asked about a MERGE RESULT, run it there**;
  "nothing was ever aimed at them" is a claim about INTENT no base-tree measurement supports. <!--
  A census counted //go:linkname push entries AT MASTER and read ZERO for four symbols, concluding they were
  "genuine frontier" -- when the seat under discussion is what ADDS those pushes (all four are bodied at master and
  the seat's diff adds 6, 6, 6 and 12 lines naming them, while the symbol the census called the one true candidate
  gets ZERO from that seat). The classification was INVERTED. -->
- **Three tells that a disagreement is a LAYER problem**: "ABSORBED" is not evidence it LANDED; the RIGHT answer
  from the WRONG instrument invites no second look; a line number off by a large fraction of a file is a CHECKOUT
  announcing itself. <!--
  All 2026-09-06. A coordinator carried "the E4 entries absorbed" for a day while master's manifest held 59 entries
  with all three tests ABSENT, the lane's own gate cleanup having run a checkout over src/core -- one count of the
  committed artifact separates "reported" from "landed". A finding measured out of a tree two trains stale came
  out identical although all three files HAD changed. A citation given as line 2357 sat at ~4155 at master: a
  disputed citation is a LAYER question, not a typo. -->
- **A MEASUREMENT is tree-specific; the ARGUMENT generalises. Both belong, argument BEHIND the measurement.** <!--
  A byte-identical CNR is a fact anyone re-checks in one command ABOUT THAT TREE; the key argument (the registry is
  keyed by the consumer's own import path, which no behavioral package can hold) is a property of the REGISTRY.
  Drop the measurement and you have a persuasive story about an unmeasured tree; drop the argument and you have a
  fact with no reach past the day it was taken. -->
- **A re-gate that lives only in a MAILBOX POST is not a re-stamp — the COMMIT BODY is the durable record**, landed
  as an APPENDED commit rather than an amend. Re-gating is cheap when the committed artifact is already the BEFORE:
  re-emit in place and let git be the differ. <!--
  A seat re-measured at 326/59/3 whose body still read 323/58/7 was measured and not re-stamped: transport carried
  the truth, the artifact carried the dead number, and the artifact is what a reader finds later. History rewrite
  is lane-scoped, so a rule requiring it is silently not followed -- one lane amended two bodies within the hour
  while another was refused outright; when a lane reports it cannot comply, suspect the rule before the lane. The
  cheap re-gate's verdict is one `git status`: both differing files CR-strip byte-identical to HEAD, comparator
  positive-controlled on a known-different pair. -->
- **BASELINE every non-green against master instead of EXPLAINING it — OWED EVEN WHEN THE DOCTRINE ALREADY NAMES
  THE CAUSE** — a baselined red travels across a host disagreement, an explained one does not; record an unexplained
  one as UNRECONCILED. Score the claim AS WORDED and post it BEFORE the run: *the failure SET at the cut EQUALS the
  set at master*. <!--
  A lane's -tests gate read RED on its host and GREEN on the coordinator's AT THE SAME COMMIT -- same OS family,
  opposite verdicts -- and the lane's cut survived unaffected because it had run MASTER and found the identical
  error there, rather than writing "this is red and here is why that is fine".
  ALREADY-NAMED-CAUSE clause 2026-09-08: three GolibTests reds at a seat's tip (link-staged fixture arms) failed with
  the SAME three names at master on the same box, for want of the symlink-creation privilege. "That is the known
  symlink gap" was available, true, and would have been an explanation portable only to the host that formed it --
  where a baseline is a DIFFERENCE anyone re-checks. Doctrine naming the cause is exactly when the baseline feels
  redundant and is not; the claim was worded as a SET equality and posted before the run. -->

## Standing facts
- **Compiling is the milestone, NOT operational**: Phase 3 (stdlib compiles clean) is a different claim from Phase
  4 (Go's own tests run and agree). [`docs/Roadmap.md`](docs/Roadmap.md), [`docs/README.md`](docs/README.md),
  [`docs/TestingInfrastructureRequirements.md`](docs/TestingInfrastructureRequirements.md). <!--
  Phase 3 complete 2026-07-10, commit 51ba5d9cf, tag stdlib-green-2026-07-10: all 302 packages of the full
  conversion (Go 1.23.1) compile clean -- zero errors, zero exclusions (runtime, reflect, net/http, go/types,
  crypto/tls, database/sql ... all included). Campaign detail in Roadmap's Phase 3 iteration log and README's NEWS
  section; Phase 4 design in TestingInfrastructureRequirements.md and Roadmap Phase 4. -->
- **The pipeline**: `go2cs -tests -test-action all <goroot-pkg> <src/core-pkg>` converts the `_test.go` variants,
  builds the hand-owned `go.testing` host (`src/core/testing`), runs it isolated and diffs terminal results
  against `go test -json`. It **always forces `-comments`** (test conversions are derivative works — the per-file
  Go copyright header must survive) and **self-locates `$(go2csPath)`** by walking the output dir up to the tree
  root, so the two-arg command works from a bare clone with no env. <!--
  First validated package: unicode/utf8, 2026-07-17, tag utf8-tests-green-2026-07-17. -->
- **Promotion happened once, wholesale**: hand-owned `[module: GoManualConversion]` / `*_impl.cs` files LIVE in the
  one tree — no canonical copy, no overlay-back step, no two-tree exceptions. <!--
  2026-08-01, superseding the 2026-07-01 defer. That ruling said not to promote package-by-package on a clean
  compile, and it was right: the chicken-and-egg it guarded against was real while the corpus was unproven. Phase 3
  plus 69 operationally-validated packages dissolved it, so the whole tree moved at once. -->
- **The S1/CS0030 "architectural wall" was a FORK, not a wall, and the fork held to 302/302**: native-type
  pointer/unsafe ops convert faithfully; **managed-referent** cases (`guintptr`/`muintptr`/… hiding a managed
  pointer in a `uintptr`) hold the `ж<T>`/`object` DIRECTLY, like `core/sync/atomic` `atomic.Pointer<T>`, never a
  `nuint` round-trip; raw-metal on non-native types is stubbed with `[module: GoManualConversion]`. <!--
  Ruled 2026-07-01. Native-type ops have identical memory semantics in both GC languages, so the faithful
  conversion lives in the converter/golib. Genuine raw metal is memory-layout math, type-descriptor walking, *.asm;
  a stub that compiles is an acceptable milestone solution. -->

## Hand-owns: the three mechanisms and their traps
- **There are THREE displacement mechanisms, and a guard enumerating two reports the third as a defect.** (1) a
  **registry entry** displaces a BODIED converted function — a converter change, two-seeded emission diff,
  hunk-only footprint; (2) a **body written into a bodyless `public static partial`** displaces BY CONSTRUCTION
  (`PartialStubGenerator`: `IsPartialDefinition && PartialImplementationPart is null`) — no `manualConversionFuncs`
  entry, no converter change, no two-seeded diff; (3) a **whole-file `[module: GoManualConversion]` marker**, where
  the converter never emits the file, so no registration is owed. <!--
  Pricing stated 2026-09-02; a change that removes generated stubs still owes a behavioral COMPILE (route #7's
  neighbourhood). Three-mechanism count measured 2026-09-06: three reds, zero real.
  TestManualConversionRegistrationsHaveBodies EXEMPTED only the first two until train 31; perGOOSReplaced
  (manualConversionDestination_test.go:206) added the third. -->
- **Marking a whole file to optimise a few functions freezes the rest and creates a permanent hand-merge obligation
  — rejected by the minimal-footprint rule even where the file is stable.** A whole-file rewrite that DOES replace
  `<name>.cs` **must carry `[module: go.GoManualConversion]`** after the `using`s, **with the `go.` qualifier or
  CS0246**. **CENSUS the csproj's `<Using Include= Alias= />` items before adding a file-level `using X = Y;` to a
  hand-own** — above the file-scoped namespace it duplicates a csproj global at the SAME scope (**CS1537**). <!--
  Placed before the file-scoped namespace. main.go's containsManualConversionMarker drops marked files from the
  convert set; without the marker a -stdlib reconvert regenerates the Go version over the rewrite. The go.
  qualifier was measured 2026-09-03. Further hand-own detail: docs/ConversionStrategies-Reference.md; two-tree
  history archived at src/archived/Baseline-vs-FullConversion.md.
  CS1537 clause 2026-09-08: syscall.csproj already carries one such global alias and NONE of its eight sibling
  _impl.cs companions declare it at file level -- the absence is the convention, and re-declaring is the error. Run
  the UNMASKING control (delete the blocker locally, rebuild) before believing "one defect": here it was genuinely
  one, with zero behind it, which is a measurement rather than a hope. -->
- **A `<name>_impl.cs` companion SUPPLEMENTS declarations** (bodyless `partial` plus the converter's comment
  placeholder) where the literal conversion compiles but cannot work. <!--
  e.g. sync's Mutex/RWMutex/WaitGroup (2026-07-11), whose Go runtime sleeping semaphore cannot be emulated,
  hand-rewritten on SemaphoreSlim/monitors. -->
- **Size hand-own versus converter change by whether the SITES TERMINATE**, and **check whether the class already
  HAS a name and a home before calling a finding its first member.** <!--
  A converter change is justified when the population is large enough that per-site work does not finish, or when
  the sites cannot be enumerated at all. Sixteen functions enumerated BY NAME from Go's own sources, eight already
  remediated in an existing hand-own file, eight open across two platforms -- that is a hand-own EXTENSION and the
  converter stays untouched. The class already had a name (ptrout), with its mechanism stated in its own file
  header. Tooling twin, same week: a lane sized a repo-wide identifier census and then found
  fleetIdentifierCensus_test.go already existed and was better -- the silent-duplication class pointed at TOOLING
  rather than at code, and cheaper to catch, because one grep answers it before anything is built. -->
- **Accessibility follows the tree's pattern** (a `Go`-prefixed PUBLIC helper per operation, native mirrors PRIVATE
  to the seam file), and **a hand-own's "deliberately not covered" scope header is corrected in the SAME commit
  that changes the scope** — a scope header that lies reads as the census. <!--
  So no consumer assembly sees a native type. One header still named two functions that had been hand-owned three
  days and one hour earlier. -->
- **A hand-own swap (backup restore, A/B neutering) can leave a STALE dll winning the build**: `Copy-Item` and
  `cp -p` preserve `LastWriteTime`, so **the defect "reproduces" against clean, HEAD-matching source with a clean
  `git status`**. Touch the file or build `--no-incremental`. <!--
  Measured 2026-08-16, cost one invalid run; cp -p twin found 2026-09-05. The restored source is OLDER than the
  assembly built from the neutered version, so incremental MSBuild keeps the wrong dll. -->
- **A hand-APPLIED edit to a generated file must be proven BYTE-IDENTICAL to the converter's own emission before it
  banks** — regenerate into a seeded root and byte-compare. <!--
  Standing bar, ruled 2026-08-31. The first measured hand-application was ONE BLANK LINE short of what the converter
  emits; without the check the next regen reports that cosmetic delta as drift and bills a phantom investigation to
  whoever runs it. Meaningful only when the emission actually landed in the compared root (the single-package
  output-positional trap) and only after the gate's negative control has been made to fail once. -->
- **The DIFFERENTIAL control (emission WITH the change vs WITHOUT) is the one that carries information**; run the
  five-minute control (revert, re-emit) BEFORE reporting any violated control, and **name the converter a control
  assumes**. <!--
  Both measured 2026-09-02. An ABSOLUTE "byte-identical to the committed file" control is unsatisfiable under
  standing corpus drift: three banked rows' -tests emissions changed WITH a cut and WITHOUT it (closure drift plus
  relocation debt), so a no-op would have failed the gate. "The landed hunk must reproduce with zero diff" assumed
  a binary carrying the merge while the measurement's binary was built pre-merge -- it failed for its premise, not
  for the instrument. Say which form the control took. -->
- **A same-package type RELOCATION is invisible to Go and collides with a marker-protected file that still declares
  the type** — iff the hand-own RE-DECLARES Go's members (whole-file rewrite → CS0102; an `_impl.cs` companion
  carrying go2cs-invented members MERGES) — **and the predicate covers every declaration kind that can duplicate:
  type, func, const, var.** **Absence from a census DOCUMENT is not absence from its POPULATION**: check the
  DERIVATION, not the index. <!--
  2026-09-07, a lane correcting its own rehearsal record; the safest class in a Go-shaped census is the most
  dangerous one here. The hop population was sound and the classifier named the moved type MEMBERS-REMOVED; the
  scope rule then filed MOVED-WITHIN-PACKAGE (8 rows) as MECHANICAL -- right for Go, INVERTED for go2cs. Measured
  over 142 marked files x 3 targets, comments stripped, filtered to Go-selected files: TWO type-level relocations,
  ONE collides (runtime2.cs::note), one merges (reflect/value_impl.cs::MapIter). The type-level predicate missed
  sync/mutex.cs::fatal, a relocated FUNCTION, found by auditing the instrument against the
  bare-name-across-a-namespace shape. A census document that names only the rows needing a human is not its
  population. -->
- **WHEN A HOP MOVES A FACT INTO THE CORPUS, ASK WHETHER ITS CONSUMER IS CONVERTED OR HAND-OWNED — the first updates
  itself, the second NEVER does, and the guard fails on the new tree exactly as it did on the old.** A hop census
  lists, PER MOVED FACT, the hand-owns that CONSUME it. <!--
  2026-09-08 (R): Go 1.24's runtime carries waitReasonSyncWaitGroupWait = 24 and its string arrives free with the
  corpus (runtime2.cs:881/927), but mapWaitReason lives in runtime/stubs_impl.cs under [module: GoManualConversion],
  so without a hand-written line the member still falls to waitReasonZero and the runtime-DERIVED guard fails on the
  1.24 tree exactly as it does at 1.23.12 -- a hop that looks like it fixed the row and did not. The tell is that the
  moved fact and its consumer sit in the same package, so nothing about the diff says one regenerated and one did
  not. -->
- **A guard's RED is a claim like any other: ask what observable it predicts and go look before treating it as a
  blocker** — a guard that cannot be wrong is the false-green trap wearing the other mask. <!--
  "The package fails CS0111 on that platform" is directly testable: the windows arm was already disproven by a
  307-project solution build at 0 errors, the linux arm by a `--no-incremental -p:GoTargetOS=linux` build of that
  one package, also 0. -->
- **A DEAD CALLER gets nothing** — record it MEASURED DEAD with a census assertion whose whole value is the day it
  goes RED. <!--
  2026-09-06: two of three callers of a defective wrapper had no call site anywhere, test emission included, so
  displacing them would have cost a registration, a placeholder and a body each to change behaviour nothing
  observes. The assertion fires the moment a forwarding property makes one live. -->
- **An EMPTY body is not a no-op when the throwing stub was a BRAKE** — a cut touching `execve` runs under a process
  ceiling, withdraws first and analyses second, and the marshalling fix precedes any body. <!--
  Measured 2026-09-02: empty runtime_BeforeExec/AfterExec bodies were argued correctly from execLock's readers and
  FORK-BOMBED the syscall row (96 children in 7 minutes), because Exec hands execve MANAGED argv/envp, the exec'd
  image comes up with garbage argv and an empty environ, loses its -test.run/helper-process markers and re-runs the
  whole suite. A CHAIN of child processes is itself proof execve did not replace the image (it keeps the pid).
  "Semantically sound" and "safe" are two claims. -->
- **Which FORMATTER runs decides what a converted test PRINTS**, so a hand-own's private reimplementation of a
  stdlib contract diverges silently — once the converted package banks, the hand-own DELEGATES to it. **A function
  the path never enters is falsified by its own silence.** <!--
  Measured 2026-09-02: the hand-owned test host carries its own verb dispatch (TestFormat.cs) with a SMALLER
  contract than fmt's -- # parsed and dropped, %T of nil as nil -- so every PRODUCTION-dimension control was green
  by construction, because production calls the converted fmt and the test dimension never does. The settling
  measurement was a probe printing ZERO lines where the failure reproduced. -->
- **A failure mode `recover()` CANNOT SEE is infrastructure, not a Go-semantics divergence — FIX IT, NEVER DISCLOSE
  IT**; a disclosure freezes the port's plumbing into a manifest as though it were a property of the language. Tell:
  **a refusal thrown inside `MethodInfo.Invoke` arrives wrapped in `TargetInvocationException` and is invisible to a
  converted `recover()` — pass `BindingFlags.DoNotWrapExceptions`**, and an arm nothing reaches TODAY is the
  booby-trap condition, not an exemption. <!--
  2026-09-06: the host raised a .NET InvalidOperationException where Go panics. The panic is recoverable in
  principle and reported Go-style; the exception is invisible to recover() and lands in the infrastructure bucket
  by the host's own classifier. The arithmetic settles it: a fix recovers the verdicts and spends nothing, a
  disclosure recovers them by spending one AND freezing a host defect -- and a non-row-shaped fix makes the
  recovered count a FLOOR.
  TargetInvocationException clause measured 2026-09-08: two of SEVEN refusal arms of a callback body surfaced wrapped,
  because the checking helpers live inside a Bind reached REFLECTIVELY; with BindingFlags.DoNotWrapExceptions on the
  invoke all seven print as a bare panic carrying Go's own text. Nothing reaches the two deepest arms today -- which
  is why they were wrong and nobody saw it. Same class as the raw NotImplementedException clause under
  "Host-killers and disclosability": a wrapper between the throw and the converted recover(). -->
- **`golib.panic(object)` NORMALISES a C# `string` to `@string` at the ONE boxing boundary, so a hand-own's bare
  `throw panic("…")` literal IS the right form** and the emitted `recover()` type switch matches it. **Open the
  CALLEE before claiming a literal surfaces wrong.** <!--
  2026-09-08. A hand-own author worried the bare literal would surface as a non-string-panic marker and found the
  answer in the callee, opened BEFORE the run rather than after a wrong claim -- the third time in one week the
  answer sat in an unopened callee. The comment at the site names the row that paid for it: sync's once-panic row
  reporting `want panic x, got x`. -->
- **A reported divergence can be THREE divergences with different reachability, and only source-reading separates
  them: a guard for a multi-part divergence asserts the HARD part.** <!--
  Reading Go against the port for one host Fail found (1) ORDER -- Go propagates to the ancestors BEFORE its own
  done check, so a late Fail marks the whole chain failed and only then panics, while the port checks first,
  throws, and NEVER propagates, so the same failure fails NOBODY; (2) KIND -- panic versus an exception invisible
  to recover(); (3) a missing done check on the propagation path. (1) is verdict-visible, (2) is the class
  boundary, (3) is unreachable; fixing the KIND alone leaves the verdict-changing half in place. The kind is the
  half a reader notices, the order is the half that changes a result, so the guard asserts that a late failure
  marks the PARENT failed. -->
- **Do not add a branch on the strength of SYMMETRY with the reference implementation if it is unreachable**, and
  **before calling a ported guard "Go's contract", find the STRUCTURE that keeps Go's copy silent and check the
  port has it** — a faithfully transcribed guard fires where Go's cannot. **Where NO guard can be written because
  the question is unanswerable today, the RETIREMENT TRIGGER is a comment AT THE SITE naming the stubs that make it
  so: the obligation sits in the FILE, never in a board row.** <!--
  runTests makes every top-level test a t.Run on a root T, so mid-run there is ALWAYS a live ancestor -- the same
  structural fact that makes Go's own logDepth panic unreachable; adding the check "because Go has it" puts an
  unexercisable branch into a host that already has one too many, so record the reason at the site against the
  structure that makes it unreachable. 2026-09-07: Go's logDepth panics only when the parent walk EXHAUSTS; our
  host implements the walk correctly (train 32) and starts every top-level test with parent: null, so the walk has
  nothing to walk. The guard is right; the CHAIN is missing -- a divergence wearing fidelity's clothes.
  RETIREMENT-TRIGGER clause 2026-09-08 (R): runtimeNow's non-bubble body is a delegation to the hand-owned now() of
  identical signature, and the bubble refusal is real BY CONSTRUCTION -- synctest.Run throws first, since
  internal/synctest is five throwing stubs -- while no guard could be written, because inBubble is one of those five
  (unreachable code asking an unanswerable question). The site's comment therefore NAMES all five stubs as the reason
  and states that the moment synctest is implemented this delegation hands a bubbled goroutine the REAL wall clock
  WITHOUT FAILING LOUDLY, so whoever lands synctest owns revisiting it. Stub census moved 243/22 -> 242/21 on
  windows, with `time` leaving the list. -->
- **A `-tests` run RE-CONVERTS every non-marked file, so a hand-own PROTOTYPE cannot be measured through the
  pipeline without its registry displacement** — only a marker-carrying `_impl.cs` survives, so packaging is
  decided by MEASUREMENT, not convenience. <!-- Measured 2026-09-03. -->
- **A `-tests` CONVERSION INTO A SCRATCH OUTPUT ROOT DROPS EVERY HAND-OWNED `*_impl.cs` THE PACKAGE DEPENDS ON, and
  the build then fails naming exactly the missing bodies — which READS AS THE PACKAGE'S STATE.** The sweep converts
  IN PLACE (`core/<pkg>` as the output directory) precisely because the hand-owns live there, so **a measurement of
  a row hand-rolls the SWEEP'S OWN invocation line, or runs the sweep.** <!--
  2026-09-08 (i9): the tell was `abiSeq` reported without `regAssign`, which abi_impl.cs defines -- indistinguishable
  from an unimplemented package unless you know the hand-own was never copied in. A lane published "neither named row
  can complete" off this reading and retracted within the hour; the census finding sitting beside it stood untouched
  because it had been run THROUGH the sweep. Note the tension with the testing-refusal rule above, whose census
  escape DEMANDS a scratch root: that escape answers an emission question, not a build one. -->
- **Displacing a `[GoRecv]` method whose receiver is `ref T` makes the converter emit the BOX-form call `Ꮡa.m(…)`
  at call sites inside `ref` bodies where no box exists — CS0103.** <!--
  2026-09-03: every prior displacement on that seam took a value receiver, so the shape was never exercised; the
  cut whose value had dropped below the fix's cost was PARKED and the defect routed as its own cut with the parked
  code as acceptance. -->
- **A hand-own protected only by a skip-list in ONE driver is unprotected in the OTHER** — the marker saves the
  FILE, not the ASSEMBLY, and **the `-tests` REFUSAL on `testing` is the real guard**. Beside it: **a host defect
  Go's OWN suite finds is FIXED, never disclosed**, and **a "race" test with `race.Enabled == false` on BOTH sides
  is a count-of-zero assertion, not an exclusion.** <!--
  Measured 2026-09-03: src/core/testing/*.cs carried ZERO [module: GoManualConversion] markers and -tests was
  unguarded on testing, so a mistyped `go2cs -tests … testing` would regenerate over the Phase-4 host and mint the
  F15b two-testing-packages collision -- not hypothetical: marker-saved file SHA-identical beside a .cs.auto, but
  56 errors, 25x CS0111, the collision measured. The refusal carries an explicit census escape that demands a
  scratch output root. The testing row's EXTERNAL variant cannot be measured by the canonical -tests command at
  all: the conversion's natural output path IS the hand-owned host's directory, so the run rewrites testing.cs and
  dies in the publish with only a manifest written -- unmeasured because the instrument clobbers its subject. The
  host defect was Setenv/Parallel ordering raised as a .NET exception, text truncated at the semicolon, no reverse
  guard; disclosing launders a bug into a class. -->
- **The host must print NOTHING a RE-EXECUTED helper's reader can see that Go's binary would not: measure any
  host-side change to process exit output on the RE-EXEC rows (`os/exec`, `syscall`, `flag`) before it banks** — a
  CONTROL row that fails after a fix is the control doing its job. <!--
  Measured 2026-09-04: a results-file flush reporting through the PRINTING reporter wrote "PASS … exit status 0"
  onto the stdout of every re-executed helper -- the one stream os/exec's tests read back -- and 22 helper-stdout
  readers went RED while the target row read clean. Go prints nothing on os.Exit, the PASS line is M.Run's own, and
  the fail action a non-zero status implies is the PARENT's (go test's) to append. -->
- **A refusal at a class-B/C site is a PANIC, not a plain exception** (a non-panic exception is classed as
  infrastructure error — unbankable AND a lie), **a guard for a RESOLUTION change asserts RESOLUTION**, and **a
  ruling that names a DATA SOURCE states HOW it is reached from the input at hand.** <!--
  2026-09-03. Resolution guard: token -> its own function; neighbour -> not; a caller-space token still routed to
  the caller table, with the name as confirmation only. Companions: synthetic PCs are minted from the canonical
  HIGH half of the address space so a dereference FAULTS rather than reading a stranger's memory, and each function
  owns a 4 KiB span because the corpus does arithmetic on PCs (+ sys.PCQuantum, + 1) -- a one-value map resolves
  neither expression. Reading the tree first turned a symbolizer increment into a RECONCILIATION:
  runtime/managed_impl.cs already carried the whole traceback surface, and a second symbolizer was one afternoon
  away. Three independently minted token spaces (caller frames, a 32-bit managed-pointer hash, synthetic PCs in the
  high half) are disjoint on 64-bit and COLLIDE on 32-bit -- a latent defect in a just-landed increment, found by
  its own follow-up census and remedied by throwing at mint time on 32-bit so it cannot rot. The data-source rule:
  "the file comes from the map record" was falsified by one measurement -- every route into the GoPositionMap
  records starts from a live frame's PDB file name and the records are keyed by C# FILE, so a synthetic PC, which
  has no frame, cannot read a file with what the tree has. -->
- **A registration SPLIT from its corpus footprint is refused by
  `TestManualConversionRegistrationsDisplaceSomething`**, and **a branch that looks seatable and is RED at the
  converter suite is posted as HOLD by its author.** <!--
  The guard's witness is the on-disk placeholder, so "registration and footprint are ONE commit" is enforced by a
  guard rather than by memory. The syscall.Uname silent subtraction was caught at the converter suite instead of
  days later at a red corpus. -->
- **The hand-own SET is decided by the PROTOCOL SPAN, not by the box census**: a census answers where an ALLOCATION
  happens, never who else must AGREE about the word. <!--
  2026-09-04: rwlock/rwunlock hand-owned onto an inline gate while increfAndClose still released through the side
  table lost every close-time wakeup, caught by Go's OWN internal/poll/fd_mutex_test.go (TestMutexCloseUnblock,
  Go=pass C#=fail at its own 10 s deadline) and fixed by the THIRD displacement. When a hand-own changes a
  synchronisation MECHANISM the unit is every function that touches it. -->
- **A hand-own inside a BANKED package has its guard already written — run that row BEFORE believing the cut and
  read the row's DESCRIPTION first**; **a guard Go SHIPS for the seam beats a hand-written probe**; **a CONDITIONED
  prediction is resolved by READING the row before the run.** <!--
  The row's own validated suite is the standing gate, and a synchronisation word's protocol span is readable off
  the banked row's test SET, not only off the source: the named test's blocked-reader wakeup was readable before
  anything ran. Red-before / green-after on a guard NEITHER lane wrote is the strongest form of the red-first bar.
  Reading the row first stops the branch being chosen to fit the measurement afterwards. -->
- **A ROUTE re-scores every box the body FORMS, not only the ones the design targeted** (a `ref` receiver forms no
  field-address box at all; `[GoRecv]` on it GENERATING the `ж` overload makes the hand-own ADDITIVE with zero
  call-site edits); **and a registry serves TWO ROLES of which only one is gated — gate the ROLE, not the entry.** <!--
  Both 2026-09-04. A ref-receiver hand-own collects the state word's atomics boxes along with the semaphore boxes it
  was cut for; a prediction was corrected a THIRD time BEFORE the run by the lane's own falsifier firing on its own
  change -- retracted-and-restated is the only honest form. Other face of the box-form call trap above. REGISTERING
  an unexported ref-primary is correct and useful (same-package callers consult it) while PUBLISHING it is a promise
  to nobody, since no foreign assembly can name the type; the hand-own registration path had never applied the
  exported bar the converter's own selections always had, which did not matter until an increment registered
  something unexported. -->
- **A registration that changes how a registered method's CALL SITES emit owes a `-tests` emission census of every
  BANKED row whose test reaches the method through an embed — the census, not the prediction, is the instrument**;
  the manual box-receiver arm is gated to DIRECT selections (`len(sel.Index()) == 1`), and **a stated remedy is
  checked against the LANGUAGE at the emission before the cut.** <!--
  2026-09-05. A displaced BOX-receiver method is re-declared as a [GoRecv] REF receiver, so the generators mint both
  the promoted forwarder and the ж<T> twin and every call form binds -- but the manual arm spelled
  Ꮡ<X>.<method>() for ANY registered method with a box in scope, a PROMOTED selection through an embedded field
  included, which no receiver form can bind and whose forwarder the name-collision skip refuses. A promoted
  selection now falls through to the hop machinery, and the guard asserts the displaced and undisplaced emissions of
  a promoted call AGREE. One such registration had master's converter emitting the box form for internal/poll's
  banked test (compiling only because the generators minted a forwarder there), so the committed test source and
  master's emission disagreed for a day with every standing gate green; the production two-seeded diff is
  structurally blind to it, and the prediction scored against that census missed in BOTH directions (a premise error
  on the base arm; a real move). Language check: a `ref` returned from a property cannot observe the last of three
  header stores, so a header WRITE is a displacement, never a materializing box. -->

## What a bank COMMITS, and what a roster row must SAY
- **When a package's Go test suite validates, COMMIT its converted C# test sources into `src/core/<pkg>`** —
  `*_test.cs`, `package_test_info.cs`, `go2cs_test_host.cs`, `<pkg>.tests.csproj` — reproducible via the
  [README "Try it yourself"](docs/README.md#try-it-yourself--validate-a-converted-test-suite) instructions. **The
  production `<pkg>.csproj` also updates (the IP-4 `<Compile Remove>` exclusion): intended, not drift.** <!--
  User ruling 2026-07-17. Committing them makes the passing suite visible and reviewable on GitHub. The pipeline's
  regenerated inputs/outputs are git-ignored by src/core/.gitignore (the staged *.go source copies,
  go2cs_test_manifest.json -- a machine-specific exe-hash digest -- go2cs_test_comparison.json,
  go2cs_test_results.json/.xml). Refresh the committed test sources at each milestone rebank alongside the
  production tree. -->
- **The committed test sources, proof pages and README badges ARE the WINDOWS record; a Linux-axis bank lives in
  the ROSTER ANNOTATION only** — a Linux sweep's README badge rewrite is RESTORED, not banked. <!--
  Measured 2026-09-03: 22 *_windows_test.cs, zero *_linux_test.cs corpus-wide. -->
- **A banked row's DENOMINATOR is a coordinator question answered by MEASUREMENT: write it, and the per-file
  dispositions, INTO the roster** — a row that states what it COVERS beats one that reads finished. <!--
  2026-09-06: a hand-owned package banked 37 tests against an upstream suite whose files it carries six of eleven,
  and the discriminator turned out MECHANICAL -- every carried file is the EXTERNAL test package and three of the
  five absent are the INTERNAL one, a principled boundary rather than an accident of effort (four of the five
  cannot convert or contribute no verdicts; the fifth is a real bounded gap). -->
- **A row stating a RATIO states its UNIVERSE, and a row carrying TWO denominators names what each ranges over IN
  THE SENTENCE** — conflating them yields a number wrong in the direction that FLATTERS us. <!--
  Verdicts-within-the-files-we-carry is not files-within-the-upstream-suite, neither is derivable from the other,
  and the absent files' verdicts belong to NEITHER count because the oracle emits only over files present. Position
  and context had been doing that work and stop being safe the moment a row carries two. Flattering is the one
  direction an honesty claim cannot afford. -->
- **Deepening a banked row is not banking a new one**: the objective is a ROW metric, so a bounded increment on a
  banked row queues BEHIND the still-unbanked rows however cheap it is, said explicitly when queued. <!--
  2026-09-06; up to seven tests. Otherwise it competes on its size rather than on its place. -->
- **A green STATES ITS LIMIT in the same post as the green** — the roster guard checks a row's structure and
  arithmetic and CANNOT check that its prose is true. <!--
  611 passing checks at the 2026-09-06 reading. The prose rests on the per-file reads behind it, each naming what
  it read; a green allowed to imply more than it measured is how a roster stops being trustworthy. -->
- **At a corpus HOP the roster is keyed by the NEW release's package identity** (`go list std` at the pin): no row
  follows production or tests across a split, outgoing counts are a FROZEN ANCHOR not a target, and a cost canary
  whose suite no longer exists is RE-BASELINED by measuring every candidate once. <!--
  Ruled 2026-09-07. A slowed-mechanism control decides which candidate carries the cost, before the rule's name and
  number are edited, dated. -->
- **A moved TEST destination is traced by TEST-FUNCTION NAME with the arithmetic closing to zero residue, never by
  file name**, and **a production-file successor derivation is BLIND to where a row's VERDICTS go.** <!--
  2026-09-07: crypto/internal/mlkem768 closes as 9 + 6 + 1 dropped = 16. A file-name match can agree with a moved
  file that was hollowed out, and `find -name | head -1` picks the first of several -- a first pass done that way
  would have reported 4 splits and sent a lane into cmd/. 3 of 10 banked rows split at 1.24.13. -->
- **A converter-side capability ALLOW-LIST can silently exclude tests**: `supportedTestCapabilities()`
  (`testConversion.go`) keys on the receiver's NAMED TYPE, so a test calling a member not on it is **converted,
  never registered, never run** — no red row, no empty verdict, no signature to grep, just a smaller total nobody
  questions. **Measure roster impact BEFORE any widening.** <!--
  A hand-owned host can implement the member PERFECTLY and every affected test stays SILENTLY EXCLUDED -- a row
  reporting a smaller denominator rather than a failure is the quietest way to lose verdicts there is. The
  function's own comments record the price twice: T.Deadline excluded SIX of context's cancellation tests, and the
  missing TB.* spellings gated out 26 of os/exec's tests, every process-spawn shape the package has, which had
  NEVER RUN. Measured 2026-09-07 at 2,425 verdicts across the three Go 1.24 members (Chdir 53 sites / 6 rows,
  Context 5 / 2, Loop 15 / 2). Widening is not plumbing -- it is the trustworthiness of every number the roster
  reports. -->

## Minting and judging a disclosure
- **Phrase a retirement condition in terms of the MEASUREMENT, not a MECHANISM** — "the reading reaches its want",
  never "when the object count reaches zero". <!--
  The os deferred entry survived a correction four hours after banking precisely because its condition was
  indifferent to WHICH branch of the helper reports; the mechanism-phrased version would have been dead on arrival.
  That property is normally invisible because it is normally not tested. -->
- **A LOOSER PIN is not a more forgiving disclosure — it is a WIDER HOLE**: it silently adopts a future UNKNOWN
  failure of the same shape and nothing ever fires. Tighter wins. <!--
  Two versions of the same three entries differed only in signature tightness -- one missing a **uintptr type
  prefix, the other the trailing colon that ends the stable prefix. The laundering hazard in its quietest form: it
  does not launder a KNOWN bug. Beyond tightness the tie-breaker is which version was gated at both configurations
  at current master with entry honesty asserted. -->
- **A signature pinned to a test's ONLY failure text carries no discriminating information — COUNT the assertions
  in the Go test before pinning**; a single-assertion test needs a different anchor. <!--
  2026-09-07: unique's TestMakeClonesStrings has exactly ONE t.Fatal in the whole function, on the time.After arm,
  so a strings.Contains pin on its message degenerates to "this test timed out" and will absorb any future
  regression silently; the admission path is the generic Go=pass/C#=fail arm and codegen-liveness is free text with
  no class-specific tightening, so nothing narrows it. A different anchor: the measured mechanism, an arm-pair, or
  a class the matcher tightens. -->
- **A measurement's HYGIENE says nothing about whether the disclosure it rests on is HONEST — verify the disclosure
  separately, adversarially, by someone trying to break it.** <!--
  A bank workflow measured unique at 19 matched / 1 disclosed / 0 undisclosed / 0 empty with exemplary hygiene --
  tail read in every spelling including escaped, freshness proven by mtime inside the run window, encoding checked
  before believing a zero, cold start, namespace misroute ruled out -- and the row still did not bank, because two
  of three adversarial lenses refuted the disclosure on independent grounds. -->
- **A disclosure with no gate that can RETIRE it is a permanent claim, not a measurement: the orphan check that
  closes it keys on a TERMINAL PASS**, with no-verdict, infrastructure-error and deadline-killed rows excluded BY
  CONSTRUCTION — **and because ONE manifest serves every platform, the check's first increment REPORTS (record key
  plus stderr line) and REFUSES NOTHING**; the refusal waits on platform-scoped entries. <!--
  No check of any class currently verifies that an entry names a test actually FAILING in the run, so a stale entry
  over a row that now passes is accepted silently everywhere. A "does this entry name a test that no longer fails?"
  predicate would fire on every row behind a host-killer (797 on one package in one afternoon, 221 on another the
  night before) and report a wall of stale disclosures on exactly the rows where the entry is most likely still
  correct and merely unreachable -- a total inversion. Positive evidence, the same clause the neighbouring mint rule
  draws. The first framing -- "the mint check is scoped too narrowly, widen it" -- was WRONG and was adopted by a
  coordinator without reading the function: that scoping is deliberate, it reads committed PROOF PAGES for a
  cross-platform hazard, and widening it would point a proof-page instrument at a within-run question.
  INCREMENT 1 BUILT AND MEASURED 2026-09-08, over the WINDOWS record: 7 pass/pass of 267 entries -- six in `sync`
  (five alloc-profile rows, one codegen-liveness) plus crypto/tls's bogo row, which is a HOST LIMIT and plausibly
  that row's third environmental outcome, so it rules nothing. 157 Go-pass/C#-fail entries are LIVE; 75 are
  UNEXAMINED for want of a proof page (reflect 62, runtime 6, runtime/pprof 6, unique 1 -- all unbanked rows). It is
  REPORT-ONLY because the manifest is SHARED across platforms (manifest doctrine rule 1: any removal is a
  cross-platform edit), so an automatic refusal here would retire another platform's live entry; increments 2
  (platform-scoped entries) and 3 (the refusal) are SPECIFIED in the record, not built. The unicode/utf8 positive
  control read 0 clean and exactly 1 planted, with predictions written before either run, 8 of 8 HELD. -->
- **GATE PLACEMENT: the cheap gate should catch the merge, the expensive one should be the backstop** — a gate's
  value is not only WHAT it catches but HOW EARLY and HOW CLOSE TO THE CAUSE. <!--
  A duplicate disclosure entry is REFUSED by the loader -- a dead row rather than a silently wrong one -- but the
  loader runs only when somebody sweeps the package, days after the merge, and presents as a CONVERSION ERROR ON A
  ROW rather than a merge defect. The format guard that runs on every roster change walks all 45 manifests and
  every entry and does not assert name uniqueness: a gate standing next to the answer and not asking. -->
- **Ruling #1 (owner): a Go=pass / C#=skip whose skip reason is OUR OWN missing feature is a FEATURE GAP, never a
  disclosure**; **a Go=pass / C#=HANG is the same ruling, and classing a hang is earned by reading the LIVE stacks
  (`dotnet-stack` on the running host).** <!--
  Re-read 2026-09-03/04; hang clause 2026-09-05, TestMutexWaitTimeMetric -- it stays on the host-deadline gate until
  the feature lands. An annotation banked where the ORACLE skipped for a HOST reason is host-conditional and says
  so, since on a capable host the gap shows and the row reads red. What read as the lock protocol's contended path
  was two threads, one spinning in Go's own status predicate and one parked holding nothing, with no runtime lock
  anywhere on the path -- so the root is a PREDICATE (the managed g never leaves _Grunning because the converted
  sync parks without gopark's accounting), not a lock. A hook in the seam every converted wait passes through is a
  golib API change whatever its line count. -->
- **A coordinator ruling citing a PRECEDENT re-reads the ruling that governs it before posting; an OWNER ruling
  outranks a coordinator's inference from artifacts**; **and a count gap is read from the row's OWN Go gate first**
  — a `cgroup2` permission gate is not the cgo axis. <!--
  The alloc-profile want-zero disclosures in bytes/bufio PREDATE ruling #1 (a want-zero alloc assert is satisfiable
  in principle and is never a disclosure) and stand as LEGACY to be re-examined, not as precedent to extend -- a
  "measured floor is disclosable" sentence built on them was retracted the same hour. -->

## Disclosure-manifest doctrine
<!-- Rules 1-7 measured 2026-09-03/04; 8-12 2026-09-06; 13-14 2026-09-07. -->
1. **One manifest file serves every platform, so any REMOVAL is a cross-platform edit** (additions are safe): read
   the OTHER platform's preserved record first, and RESTORE an entry that still fires elsewhere. <!--
   The first Linux annotation refresh to remove an entry (a row passing on Linux under Release+TC0) turned the
   WINDOWS row red on the next union battery with exactly that one unabsorbed mismatch. An entry present but not
   firing is not counted and changes nothing on the platform that retired it. The durable form is platform-scoped
   entries (schema plus reader), so a per-platform retirement is expressible without touching the other platform's
   absorption. -->
2. **An entry is a multi-line JSON object, and a manifest that does not PARSE reads as NO disclosures** — worse
   than the bug being fixed. PARSE manifests, never match them with a spacing-sensitive TEXT pattern; a restore is
   whole-file, numstat-checked and parsed before commit. <!--
   Valid only when the seat's sole delta to the file is the removal, asserted by counting the commits that touch the
   path. A non-parsing manifest strips every other entry's absorption too. An existence guard ("a disclosed count
   has a committed manifest") cannot see entry- or platform-level correctness and must not be read as if it did.
   Sharpened 2026-09-05: one manifest spelling a key with TWO SPACES silently dropped 54 of 176 disclosure entries
   from a census, and the only thing that caught it was a second derivation being on the table to disagree with. A
   count that disagrees with another derivation is an INSTRUMENT BUG until proven otherwise. -->
3. **A disclosure CLASS the comparison cannot ADMIT is a guard that cannot go green** — make the pipeline ADMIT the
   class, never re-label, with a positive control that a MISSPELLED class stays unabsorbed. **A configuration
   ruling exercises manifest entries the bank never did.** <!--
   matchTerminalStatuses unlocked a Go=pass / C#=skip pair only for platform-skip, so three committed
   cgo-configuration entries written for exactly that shape could never fire -- invisible while the bank host ran
   cgo ON, where the oracle skipped those tests too and they matched skip/skip. Ruling cgo OFF made the class LIVE
   for the first time. The entry's name carries WHY -- the axis -- and re-labelling throws that away. -->
4. **Two seats that are only honest TOGETHER are one train**: gate the first on the presence of the second rather
   than flipping a class in the interim, which leaves a landed master with a mislabelled entry.
5. **A row that cannot say WHY it skipped is not disclosed, it is unmeasured** — a Go=pass / C#=skip whose host
   wrote NO results file has an unreadable converted-side reason, so its class waits on a direct-host read. <!--
   The comparison record carries verdicts only. -->
6. **A SAME-FAMILY disclosure is written only from the row's OWN results-file signature, never from the family
   name. Fix, then disclose.** A `%T` / `TypeOf().String()` name change is reflect-bridge-touching AND owes the
   behavioral OUTPUT phase. <!--
   "Its 37 siblings are alloc-profile" would have hidden a corpus-visible NAME defect: reflect's Type().String()
   dropping an array's LENGTH at an element position, a production wrong answer. -->
7. **A capability-registry KEY is PINNED PER ENTRY with the package clause its GOROOT file declares** (internal
   tests bare, external suffixed), and **a codegen-liveness-shaped test-liveness finding takes that class's
   one-axis Debug A/B as its ADMISSION before an entry is minted.** <!--
   2026-09-04: a guard premised on "every gated test lives in an external package" REJECTED a correct internal key,
   and accepting BOTH spellings would have discarded its silent-mis-key protection -- three negative controls, each
   mis-spelling rejected, are what make the widened guard a guard. The liveness shape is a finalizer a test blocks
   on forever; this family's most convincing story was measured FALSE once. -->
8. **A disclosure that cannot MATCH is not inert — it is a NEW RED. Never add an entry whose signature cannot be
   satisfied, and measure BOTH states before assuming an unmatched entry is harmless.** <!--
   A parent test whose fail event carries a null output emits nothing of its own, absorption is a substring match
   against the entry's signature, and an empty signature is refused at load for every class but one, so no signature
   can ever match it. Measured both ways: WITH the entry, seven undisclosed rows and the parent reported as not
   matching its own disclosed signature; WITHOUT it, six and the parent ABSENT. -->
9. **A parent rides its children only when ALL its subtests are disclosed — the remedy for a failing parent is to
   enter the missing LEAF, never a parent bookkeeping entry.** <!--
   Named in the converter's own source, where another package pins twenty-five leaves and lets two parents ride it;
   the finding shrank its own queued re-bank list from two entries to one. -->
10. **A stale SIGNATURE fails safe; a stale REASON does not** — re-derive the REASON against a fresh measurement
    before re-pinning the signature, even when it looks obviously still true. <!--
    A pin that no longer matches stops absorbing and the row goes honestly RED, which is how two stale entries were
    found at all; a drifted reason with a signature that still FITS keeps absorbing while explaining the wrong thing
    -- a row reading green for a cause that is no longer its cause. The entry a lane called the SAFE half was
    carrying exactly the hazard it had described one post earlier, already authorised on that reading. -->
11. **The strongest disclosure is one whose own control proves the underlying defect ABSENT** — that is what makes
    it a disclosure rather than a hiding place. <!--
    A reflect row exists to catch a zero-sized return aliasing the next result's storage, and the cleared-slot arm
    COLLECTED -- which could not have happened had the alias existed, since the value under test still holds the
    first result across the collection -- so the entry documents a frame-slot pin AND measures the defect the test
    was written to detect as absent. -->
12. **Check the SCHEMA before writing into it, and do not invent a field to carry a ruling's wording.** <!--
    An execution annotation is a ROSTER ROW field parsed by the sweep's module, not a manifest field, so it lands at
    BANK time -- and the class in question needed no allow-list entry at all, i.e. no converter change. Written the
    same evening an inexpressible entry was measured to CREATE a red. -->
13. **`platform-skip` REQUIRES a platform condition in Go's OWN source — a CAPABILITY self-check is not one,
    however exactly its text matches. Read WHICH CONDITION the skip tests.** <!--
    Owner ruling 2026-09-07. net/http/pprof's TestDeltaProfile carries BOTH shapes in one function: one skip is
    conditioned on strings.HasPrefix(runtime.GOARCH, "arm") -- genuinely platform-conditioned -- while the other
    fires on `if !seen(p, "mutexHog1")` with the text "mutex profile is not working", which fires on ANY platform
    whenever the profiler yields nothing. Go passes because its profiler works; the converted side skips because
    ours returns a well-formed profile with zero samples. It has every mechanical marking of an admissible
    disclosure -- Go=pass/C#=skip, the skip text matching Go's source exactly, so it sails through the signature
    test -- and is none of one; admitting it would make any unimplemented feature Go guards with a self-check
    disclosable, turning the class from *cannot* into *haven't*. -->
14. **A ruling naming a class the SCHEMA DOES NOT DISPATCH ON is a silent no-op wearing a ruling's authority**: the
    class FIELD is the mechanism, the classification goes in `reason`, and a ruling that writes into a manifest is
    checked against the LOADER that reads it before it is posted. <!--
    Measured 2026-09-07; accepted, landed-looking, inert -- the worst outcome, and the loader's non-emptiness
    validation is what makes it so. The coordinator ruled two HANGING tests admitted as "capability entries, class
    REPRESENTATIONAL"; at testConversion.go only host-fatal reaches hostFatalSkipExpression (:6531) and only
    host-fatal may carry an empty signature (:6853), while class validation is non-emptiness ONLY (:6850). A hanging
    test has no failure text, so no other class can both load and function. The lane read the code, HELD the cut for
    the word, and proposed the precedented shape (class host-fatal, empty signature, the classification and the
    lifting condition in reason). Same family as (12), met by its author. -->

## The alloc-assert meter
- **The alloc asserts are denominated in BYTES, not object counts, and the two wants behave OPPOSITELY.**
  `AllocsPerRun` reports `max(1, allocated/runs)` with INTEGER division and falls back to the BYTE-derived figure
  when the object count is zero but bytes are not. **A want-ZERO assert passes iff allocated BYTES are zero** (a
  24-byte box cannot hide in it); **a want-ONE assert over 100 runs passes on any total under 200 bytes.** "Want 1
  got 152" needs 152 B/run down to ≤1, not to zero. **State BYTES where a row implies counts.** <!--
  Measured 2026-09-06 against AllocsPerRun's own documented three-case rule. Two orders of magnitude of headroom,
  invisible unless you read the arithmetic; reading every alloc row through the want-zero lens writes off a much
  softer population. -->
- **A THRESHOLD belongs to a TEST, not to a ROW: re-read the assertion in the test under discussion before
  comparing anything to it**, and NAME the test a second-hand bound came from. <!--
  2026-09-07: TestMapAlloc asserts <= 10; TestDeepEqualAllocs asserts ZERO. Both live in reflect, both are alloc
  asserts, and a coordinator holding "Go's <= 10" from the first read a measured 9 on the second as crossing the
  threshold -- concluding a retirement where the row still fails outright. The arithmetic (9 <= 10) was correct and
  irrelevant; a reading error wearing arithmetic's clothes. -->
- **Driving one unit to zero can move a failure to the OTHER unit's branch rather than fixing it** (so object-count
  work alone can NEVER retire a want-zero entry), and **a ROW is not an ASSERTION: size a row from ALL its
  assertion blocks before calling it cheap.** <!--
  Taking golib's counted objects to zero while bytes remain nonzero moves a want-zero assert from the count branch
  to the byte branch, where it still fails; at ~106 bytes per counted object, well over a hundred bytes per run sit
  outside the counter entirely, and the arc needs a BYTE-denominated measurement before it needs a fix. A "cheapest
  want-zero row in the package" claim was wrong by construction because one of the row's TWO AllocsPerRun blocks
  was measured and called the row -- the other was per-insert map allocation, 1502/run against a want of 10. -->
- **Read the instrument's own documentation before asking the fleet or reasoning from a model.** <!--
  "Can an alloc assert pass while the code genuinely allocates, since the counter is blind to boxing?" is answered
  by name in AllocsPerRun's documented rule; the lane that answered said it CHECKED rather than reasoned, and that
  from first principles it would have got it wrong. -->
- **The allocation-disclosure LABEL is decided by the METER, not by the BOUND**; a want of ZERO and a want of ONE
  are the same question: <!-- Owner-delegated ruling, 2026-09-05. -->
  - an **incomparable unit** (a byte-derived shim where Go counts OBJECTS) → **alloc-count-semantics**, nothing to
    retire;
  - the same meter with a **named mechanism** → **DEFERRED** plus a plan;
  - the same meter with a **stated proof** → **STRUCTURAL**;
  - **none of the three** → a reading OWED, and no label at all.
- **Verify the meter CLAIM in an entry's text PER ENTRY** (the unit can be a per-entry RUNTIME property), and **a
  FLOOR is a labelling hazard: an entry whose measured value equals BOTH its want and the floor takes NO label
  until the raw numbers are read.** <!--
  The shim reports Go's own meter (a COUNT) when its counter saw the allocations and a byte-derived figure when it
  saw NONE, since reporting the zero would be a FALSE PASS, and it NOTES which, with both numbers, on every nonzero
  result -- so "verify the unit once per path and let entries inherit it" is unsatisfiable by construction. A
  nonzero-byte result reports AT LEAST 1 deliberately, so an assert wanting 1 that reads 1 may be sitting ON the
  floor rather than agreeing. -->
- **An ABSENCE is a measurement too**, and for a DEFERRED disclosure the question is never "does a record exist" but
  **"does a stage REMOVE the allocation the assert counts"** — reducing the cost is not a retirement. <!--
  "No record exists for this family" was asserted inside an otherwise three-ways-derived census without opening the
  file the entries themselves cite BY NAME and section number; it existed, at 979 lines, with a staged plan.
  Reducing the cost is an addendum's justification. -->
- **"AMORTIZES" and "REMOVES" are the same to a counter under ONE condition, a property of the TEST** — a
  per-referent cache reaches steady-state zero exactly when the asserted closure REUSES its referents — **but the
  amortizing plan's own ELIGIBILITY RULES decide first.** <!--
  So the structural-vs-deferred call is MEASURABLE (read the assert's closure) and a coordinator hands back the AXIS
  rather than a verdict. One family's closures reused their referents perfectly (which by itself said "deferred")
  while the record's MANDATORY rule -- a shell over a VALUE type may not be cached, since its constructor copies the
  struct out of the box and a cached instance would freeze a snapshot -- makes the cache refuse exactly the
  referents being reused, so the family SPLIT: value-type rows STRUCTURAL on a three-part proof inside the record;
  no-boxed-value rows DEFERRED against the one proposal that REMOVES an allocation rather than amortizing it. A
  cache cannot help when each iteration boxes fresh values. -->
- **The METER belongs to the QUESTION** — an `AllocsPerRun` ASSERT is in BYTES, a ratified deferral's FLOOR is in
  OBJECTS: name the question (move the assert, or reach the floor?) then take its meter. **`deferred` →
  `structural` OUTRANKS moving the assert**, and **ZERO OBJECTS IS NOT ZERO BYTES** — the floor is a CLASS
  condition, the retirement a separate PER-ENTRY question. <!--
  2026-09-07: a lane spent a shift proving the byte meter was the one that mattered, then judged an arm worth "88 B
  of 12,808, 0.7% -- not worth a hand-own" and dropped it, when that arm was the ONLY one that moved the scalar rows
  3 objects -> 2, i.e. onto the FLOOR, converting deferred into structural, which never re-opens. A deferred entry
  is re-measured every sweep, a regression fails the row, and its plan must stay executable, where a structural
  entry is a closed proof. Reaching the object floor retires nothing on its own: TestDeepEqualAllocs asserts on the
  byte-derived result and all 38 subtests still failed. -->
- **An alloc harness that SWITCHES UNITS when a count reaches zero manufactures a PHANTOM REGRESSION — read the
  PER-ENTRY unit note, never the bare number**; and **a FLOOR and the instrument that reads it must SHARE A UNIT**,
  or "the reading EQUALS the floor" is not evaluable. <!--
  Two rows read "master 1 -> seat 240" and "master 1 -> seat 496", a 240x regression in the seat's own footprint. It
  was neither: those rows' golib OBJECT counts had reached zero, so the harness fell back to reporting BYTES, and
  the two runs' own notes say so in different words -- the direction that looks like catastrophe is the direction
  improvement produces. The ratified floor is "2 boxes at the any seam" while the host reads a golib OBJECT count
  that the same ruling records as blind to CLR boxing, and two rows measured 1 object at master -- BELOW a
  structural lower bound of 2, impossible if reading and bound shared a unit. So a counter reading of 2 establishes
  nothing either, sixteen rows meeting the condition AS WRITTEN do not meet it AS MEANT, and `structural` -- the
  label that never re-opens -- is the last place to accept a unit mismatch. -->
- **A census that REFUSES to bucket its unclassifiable rows is what makes a ruling possible: a third bucket named
  "cannot be assessed" carries more information than a forced binary.** <!--
  Those two rows were reported as "I am not classifying them in either direction"; had they been quietly placed
  either way the unit divergence would have been invisible and sixteen entries reclassified on it. A census's
  refusals are as load-bearing as its counts. -->

## The exclusion bar
**The roster's exclusion classes are the OWNER's; the bar refuses "merely hard, unimplemented, or expensive"; A
COORDINATOR RULING CANNOT MINT AN EXCLUSION CLASS. READ the bar, never RECITE it** — a dispatch naming the classes
from memory named THREE where the roster carried FOUR, the day after the fourth was minted.
- **E1** — no eligible tests.
- **E2** — broken oracle.
- **E3** — the subject IS the replaced representation.
- **E4** — the comparison is SOUND and validates nothing.
<!-- E1/E2/E3 read 2026-09-04; E4 minted by owner ruling 2026-09-07. "Untestable by capability" was a phrase doing
     work the ledger does not license, held by the lane against the parser and the format guard before it was ever
     written. -->
- **THE DENOMINATOR HAS THREE AXES AND THE AXIS IS NAMED BEFORE ANY SUBTRACTION**: (A) raw `_test.go` on disk, (B)
  constraint-surviving, (C) declares `func Test*`. **Fix the anchor's axis by DERIVATION — reproduce the roster's
  own anchor denominator — never by prose**, and **check whether an exclusion is ALREADY outside the axis you are
  subtracting from, or you double-count.** <!--
  Measured 2026-09-08: 227 / 217 / 215 at the corpus pin and 245 / 234 / 229 at go1.24.13. Axis C at the corpus pin
  REPRODUCES the roster's anchor denominator of 215, which is what fixes the anchor's axis; a dispatch quoting 234
  was quoting axis B while describing axis A. Four of the six E1 exclusions are ALREADY outside axis C, so
  subtracting all six double-counts. The new release's implementable denominator is 229 - E3 - E4 = 227 pending the
  E2 sweep. The lane's own range prediction (205-220) MISSED and is scored missed. -->
- **E2 IS DECIDABLE ONLY ON THE HOST THAT RUNS THE REFERENCE `go test`** (windows/amd64), so **a census run
  elsewhere marks candidates NOT DECIDABLE rather than producing a clean zero** — the hole can move the denominator
  DOWN and never up. <!--
  2026-09-08, with the axis measurement above. A zero produced off-host is not the absence of broken oracles, it is
  the absence of the instrument; and because E2 only ever subtracts, an off-host zero flatters. -->
- **A row whose tests an unbuilt implementation WOULD satisfy stays IN the denominator as unimplemented** — its
  recon is the disposition, and widening E3 for it is the precedent every later frontier row cites.
- **E1/E2/E3 sit on the *provably meaningless* limb** — no test, no trustworthy baseline, or a pass that would be
  fabrication; the comparison cannot produce information. **An E4 row's comparison works perfectly and tells the
  truth; what it cannot produce is a PASS**, and the E3 boundary is the REASON a pass is unavailable (fabrication
  versus legitimate-implementation-nobody-wrote). **`matched == 0` is NECESSARY, NOT SUFFICIENT**, but a row LEAVES
  E4 by ARITHMETIC the moment one verdict matches. <!--
  E3's own internal/unsafeheader is itself matched-0, which is why matched==0 cannot be sufficient. E4 members are
  the only exclusions admitted on a MEASURED comparison rather than an argument about why one cannot happen, and
  each carries an explicit revisit condition. The E3 boundary is a judgment exactly as E2's and E3's are. -->
- **"The blocker now has a NAME" is not "the row is impossible"** — a function refusing BY NAME because a capability
  was never built is the clearest evidence of UNIMPLEMENTED. **Representational impossibility QUALIFIES where
  unbuilt does not.** <!--
  2026-09-06: a coordinator read a retired blocker as a class change and was refused by the schema's owner,
  correctly, and withdrew without escalation. The same evening, from the opposite direction: a set of disclosures
  qualifies because the pointer model is a deliberate identity scheme, so the numeric comparison the assert
  performs has nothing to compare. The pair states the class boundary better than either case alone. -->
- **A precedent transfers only if its PREMISE does, and a MECHANISM does not transfer by resemblance either** —
  reaching for "the same kind of reason" on a resemblance is the same move as reading a registration as
  remediation. <!--
  The race row is excluded because Go declares NO ELIGIBLE TESTS outside an instrumented build, so the comparison is
  vacuous by Go's own definition; a row with two eligible tests that RUN and produce verdicts is the NEGATION of
  that premise, not an instance of it. The mechanism rule cut both ways in one evening: it refused an exclusion the
  coordinator wanted AND a disclosure a lane would have benefited from -- the second half of a billed increment
  stands as WORK until it has its own measurement. -->
- **Before ruling on a row's class, read the row's own RECON and any design record that exists BECAUSE of a prior
  ruling. A prior ruling stands until a NEW measurement reopens it; a measurement CONFIRMING its premise is not a
  reopening.** <!--
  The same question had been ruled the other way one day earlier -- "unimplemented, not untestable; expensive rather
  than impossible" -- and the design record for the missing capability exists precisely BECAUSE the row stayed in
  the denominator. -->
- **Which side of the bar an entry sits on is a MEASURED discriminator, never a wording choice: ask whether the
  CONCEPT exists for ANYTHING** (can the host produce a comparable code pointer for ANY function?) before writing
  the new reason, and **split a probe's population when the answer might differ across it.** <!--
  2026-09-06. "No managed body exists" reads as unimplemented and may be representational; if the concept exists,
  the block is one missing body and the entry is WORK that no rewording converts into a disclosure, and if it cannot
  for any function the argument stands on the pointer-identity footing. One probe answered three questions because
  its properties were reported SEPARATELY rather than averaged: plain functions non-zero, stable, distinct and
  resolvable (concept EXISTS, missing body is unimplemented work) while method values fail STABILITY alone (a fresh
  identity per read) -- which disposed of two entries in OPPOSITE directions and named the smaller, more valuable
  fix. -->
- **A test that only asks for SELF-CONSISTENCY can never be excused representationally: ask what the assertion
  REFERENCES** — the foreign system's values (a representational argument is available and must be argued) or only
  ours (the entry is a DEFECT wearing a disclosure's clothes). <!--
  If the assertion compares two of OUR OWN values and requires them equal -- as a method-value pointer test does,
  since Go's two method values share one trampoline whatever the receiver -- then nothing about the foreign system
  is being asked of us and a host with the missing property passes it UNCHANGED. -->

## Vacuous passes: when a green measures nothing
- **A banked row can be a VACUOUS PASS, and a census over the roster is what says how many: a pass whose two arms
  are equal for OPPOSITE reasons is a false green.** <!--
  2026-09-03: internal/abi's TestFuncPC compares FuncPCABI0(fn) against a value _test.s writes in Go -- assembly
  never converts, so the C# side reads 0 == 0 while Go compares two real addresses, and the verdicts MATCHED. Under
  every honest answer the row stops passing, so it was RULED to spend the verdict (2 -> 1 + 1 disclosed,
  runtime-capability, permanent by construction). The census over 202 banked rows found that class has exactly ONE
  member. The class stays small because bodyless partials THROW and the corpus converts under purego (17 silent
  hand-own bodies exist, one read by a banked assert), and nine banked passes are tautological by Go's OWN
  construction (both arms call the *Generic twin) -- honest, since that IS the production path. -->
- **A hand-own that changes a package's INIT STATE re-sweeps every banked row whose asserts read that state**;
  **vacuous-if-stubbed tests are NAMED before any stub lands**; **and a banked row that dies on a corpus defect is
  UNREADABLE, so it cannot gate anything.** <!--
  internal/cpu's four `if HasX && !HasY` implications could not fail while doinit never ran (all flags false) --
  remediated by the [ModuleInitializer] hand-own, but the BANKED page stayed vacuous until reswept; the second
  vacuous-as-recorded page the 202-row census found. Stub example: no content assertion around StartCPUProfile. An
  increment whose canary the dead row was declares final on the remaining canaries and STATES the row's
  unreadability, rather than waiting for a row nobody can run. -->
- **A MATCHED COUNT is not evidence until someone asks whether the converted side COULD HAVE FAILED** — the
  question is *"is there a state the converted implementation could have been in that makes this assertion
  fire?"*, never *"did the verdicts agree"*. Remedy: **make the silent zero a LOUD REFUSAL, an honest disclosed
  fail — never delete the test.** <!--
  runtime/pprof's 120 matched rows audited 2026-09-07: 13 REAL, 103 WEAK, 4 VACUOUS, with 100 of them resting on an
  assertion that CANNOT EXECUTE (pprof_impl.cs:110 opens `_ = labels;`, so counts stays empty, max = len(counts)-1
  = -1, and the ordering loop never runs) -- a bank quoting 120 is quoting one smoke test 100 times. The bar is the
  TestFuncPC set above. -->
- **A DOOR/BILLING census cannot see a dead assertion** (row billing and assertion-level reachability are two
  audits), and **a DERIVED membership is quoted only with a DOUBLE CLOSURE behind it** — a single-run census of
  crash/deadlock canaries owes a second UNGATED run. <!--
  104 of 120 rows billed to ONE declaration read as concentration in that declaration; the concentration was in
  that declaration's dead assertion, one layer down. The 14 declarations carrying matched rows summed to 183 across
  all 44 declarations (the census's own Go-side count) and to 120 across the 14. -->
- **A pass that NO HOST DEFECT COULD EVER MOVE is EXCLUDED with the reason stated, never counted**; **a test failing
  deterministically for a divergence the project CHOSE is run and DISCLOSED, not excluded**; **a failure path that
  loops forever is a HAZARD needing a capability entry**; **a row's SIZE is its VERDICT count, not its top-level
  NAME count.** <!--
  2026-09-04, all from one row: the vacuous pass asserted count(<a Go output literal the host never writes>) == 0,
  passing on BOTH sides -- the anti-laundering clause read from the other direction. A chosen divergence (the
  host-identity class) belongs where it can be seen. The never-ending failure path was a timeout-dump scrape that
  doubles and retries forever; admitting it turns the row into a deadline kill with a contiguous alphabetical tail.
  The row measured 156 verdicts against 59 top-level names, reconciled before sizing. -->
- **A design PREMISE about an output path is checked against an existing BANKED instance before options are
  costed** — colocation is the pipeline's norm, which one dispatch's premise had backwards.

## Host-killers and disclosability
- **A disclosure pins a failing NAMED row, so a HOST-KILLER cannot be disclosed at all** — a goroutine panic
  escaping the process, or an access violation, produces no row to pin; the arc's gate is making the killers produce
  rows. <!--
  2026-09-03. "Make the host-killer produce a row" was ALREADY implemented and buys nothing for the escaping-panic
  case: the host emits a fail row and a package fail, faithful to Go's own death, and surviving the panic would be a
  false green twice. -->
- **A converted `recover()` does NOT catch a raw `NotImplementedException` from an unimplemented-external stub** —
  only a converted panic — so such a stub reached from an http handler TERMINATES the process where Go answers
  500. <!--
  A regex-shaped test assertion is sized against the WHOLE regex (another goroutine's header AND its frames), not
  against the state word. -->
- **Two bugs of the same SHAPE can have opposite SIGNS — ask what a fix does to the row's DISCLOSABILITY, not only
  to its failure count.** <!--
  2026-09-07: runtime/pprof's state leak and runtime's host-lifecycle leak are both "an exception escapes and
  corrupts subsequent state". Fixing pprof's ALONE makes its row WORSE -- twelve disclosable fails become twelve
  UNdisclosable infrastructure-errors. Fixing runtime's is STRICTLY BETTER -- an infra-error reverts to a genuine
  disclosable fail and 799 unmeasured rows unblock. Neither sign was predictable from the bug's shape; both were
  measured before anyone acted. -->

## After a sweep: classify the dirt, never bank it
**After an operational SWEEP `git status` is dirty and it is (almost always) NOTHING — and the dirt is NOT confined
to `src/core`**: the sweep also REWRITES each swept package's proof page under `docs/validation/current/`, so
**restore BOTH roots** or the next diff reads as proof-page drift. **A gate's own DIRT PREFLIGHT catches the half a
corpus-scoped restore leaves; refusing to measure a tree it cannot vouch for is the shape every gate should have.** <!--
The proof-page rewrite is by design -- it is why the host-conditional check reads the COMMITTED page from HEAD. 56
stranded proof pages measured on one lane's shift, 2026-08-29. 2026-09-06: a solution leg aborted "ABORT dirty"
after a canary run whose restore had been scoped to src/core while the pipeline had also rewritten
docs/validation/current/<row>.md -- met by the gate rather than by the reader. -->
1. **CRLF phantoms — most of a healthy sweep's dirt.** The converter preserves the Go source's **LF** inside
   multi-line string literals while emitting CRLF everywhere else, `core.autocrlf` smudges those to CRLF on
   checkout, and a `-tests` run re-emits them as LF — so every banked file holding a multi-line literal shows
   **modified with no diff hunks at all**, absent even from `--numstat`. **Do not memorize a file list.**
   *Positive control:* `git diff --numstat HEAD~1` must be non-empty, or your check is broken, not clean. **But
   "numstat must be empty" is FALSE for `-text` paths** (`src/core/compress/testdata/*`), where a pure CRLF flip
   shows a real non-empty numstat that reads exactly like content drift: for verbatim `testdata`/`*.s` copies,
   **test CR-stripped equality against `HEAD` directly.** <!--
   The count tracks how many banked packages hold multi-line literals and grows with every bank: 15 at the
   47-package roster, 16 once strconv's testdata/testfp.txt joined, 5 + 10 at the 73-package roster. The -text
   exception found r40, 2026-08-04: gettysburg.txt 29/29. -->
2. **`-tests`-CLOSURE production files.** A handful of production `.cs` differ between the two emissions because
   the `-tests` closure imports more: the `Δio` alias, the `global::go.*` root escape, the using-block REORDER the
   alias causes, the `initᴛᴛtests()` hook in `package_init.cs` (+7 REAL lines that SURVIVE a numstat filter), and a
   `GoPositionMap` funcLit/range argument. **Classify-and-restore per the side the tree rests on** — a STANDING
   restore until the two emissions agree on one alias per import. **AMENDED: for a row whose test sources are
   REBANKED at or after the init-order arc the `initᴛᴛtests()` hook is BANKED, not restored** (a re-derived suite
   does not compile without it). **Staging corollary: never `git add -A` / `git add .` on a tree that has had a
   sweep or `-tests` run — name the paths.** **ONE-WAY emission changes are NOT closure shapes**: the `-tests`
   init-forcing hook (+7 lines in `package_test_info.cs`) appears at a row's next test-source REGENERATION and
   stays, no standing restore. <!--
   Fourth shape named 2026-08-17; amendment 2026-08-26, ratified at the leveling-rebank floor; staging corollary
   paid for 2026-08-26 -- the hook shape survives numstat filters and lands in the commit silently. Fifth shape
   (GoPositionMap funcLit/range) 2026-08-29/30, rooted independently by two lanes the same day, evidenced by banked
   cookiejar carrying it. Under the amendment those packages' package_init.cs rests on the -tests side and the
   -stdlib overlay ritual must classify-and-KEEP it; the restore rule stands only for rows still carrying pre-arc
   sources. The package_test_info.cs init-forcing hook landed 2026-08-30; 193 reference-model banked test infos are
   stale-until-rebank by design, and the rebank wave that levels them owes the full-roster sweep, the only place the
   throwing-production-init regression shape can materialize. Whether a sweep SHOWS closure shapes depends on which
   side the committed tree rests on, so neither state is the invariant: resting on the -tests side they are
   invisible and surface only under an -stdlib reconvert control; resting on the -stdlib side -- where r40 left it
   -- every sweep flips them and they must be RESTORED. Measured at r40: 13 files, wider than the six recorded in
   docs/phase4/DESIGN-named-interface-wrappers.md section 7 -- also bufio/{bufio,scan}.cs,
   crypto/md5/{md5,md5block}.cs, regexp/{regexp,exec,backtrack}.cs. Both emissions are correct for their own
   closure; only the pipeline pairs them. -->
   - **An amendment with a CONDITION is checked against the condition, not applied by name** — the rebank amendment
     covers rows whose test variant relocates into the PRODUCTION class. <!--
     2026-09-07: os's relocates into its own *_internal_test_package with its own static constructor, nothing
     implements the partial, so the compiler erases declaration and call and the hook is INERT -- restored,
     correctly, and the difference was one `git grep`. -->
3. **`.cs.auto` review siblings.** Tracked, refreshed by a `-tests` run but NOT by an `-stdlib` overlay (which
   excludes them to protect the hand-owned `.cs` beside them). Restore them in a sweep; re-measure the whole set at
   each rebank head, one seeded reconvert per target, rather than banking a count. <!-- CleanupBacklog 18. -->
4. **Deduplicated same-shape anonymous structs — LEVELED, so a reappearance is NEWS.** The converter binds a second
   anonymous `[GoType("dyn")]` struct of identical shape to the FIRST declaration's type; the diff reads as the
   duplicate block vanishing while slice and element types rename onto the original's `ᴛ1`, with a knock-on in
   `package_test_info.cs`. **Unlike classes 1–3 this one does NOT stand: meeting it again means a NEW unbanked
   converter change — find that commit rather than restoring the file.** <!--
   Commit e61758549, the reflectlite arc; package_test_info.cs's witness list sheds the declarations that no longer
   exist. A suite banked before that commit keeps the old shape until its own pipeline rerun; the 2026-08-24
   post-merge rebank ran the last five (math/cmplx, go/build/constraint, regexp, strings, time). -->

**Anything that is none of these — a non-empty `numstat` on a production `.cs` that is not a closure re-flip, or ANY
change to a production `.csproj` — is REAL DRIFT: stop and root-cause it before landing. But CONTROL THE DIRT AT
MASTER before charging it to the chain that ran the sweep** — the same sweep at landed master leaving the same files
with the same numstat is MASTER'S standing relocation debt: flag it for the regen, do not fix it under a
measurement. <!--
A production-.csproj change specifically meant the validation-pack block had been stripped; fixed in ce82093b0 and
proved clean across the full r40 sweep.
MASTER-CONTROL clause 2026-09-08 (i9): a `sync` sweep after a fatal-path chain left 24 files dirty (+199/-250,
[GoInit] hooks dropped from cond.cs, a new package_info_internal_test.cs) and read exactly like the chain's own
damage. The SAME sweep at landed master left the same 24 files with the same numstat, the two drift diffs differing
only by a blob index and the proof page's own converter stamp. Beside it, two instrument notes from the same run: the
probe was run as the BUILT BINARY because its own README had measured that `go run` MASKS the exit code (1 against
the binary's 2), and both pins were asserted on version AND GOROOT and ABORTED on, since printing a pin is not
checking it. Companion to the four-class list above: the residual here was forced-init relocation debt, NONE of the
four standing classes, which is what sent it to the master control rather than to a restore. -->
- **COMMIT THE CHANGE BEFORE SWEEPING IT: a row swept with the change in place but UNCOMMITTED is the wrong ORDER,
  because a restore cannot tell uncommitted work from the sweep's own dirt.** Restore BY NAME, verify by
  POST-CONDITION, re-sweep against the COMMITTED tree, then classify the residual. <!--
  2026-09-08, the sibling finding to the tree-naming rule under "When a reading expires": the file was restored by
  name, the restore verified by post-condition rather than by its own status line, the row re-swept against the
  committed tree, and the residual dirt then classified -- forced-init relocation debt, none of the four standing
  post-sweep classes, and controlled at master (above). -->
- **A BARE `git checkout <ref> -- .` OVER A SEAT'S OWN WORKTREE IS A REVERT WITH NO MARKER: it STAGES master content
  over every seat file, `git status` calls it modified exactly as it would real work, HEAD stays at the SEAT's SHA,
  and any gate run there measures LANDED MASTER while reporting as the seat — green because master is green, the
  purest false green.** **Read the dirty COUNT against what you EXPECT before any gate**; prefer ref reads (`git
  show`, `git grep <ref>`) when only CONTENT is needed, since they cannot touch the tree; restore with a checkout of
  HEAD over the directory (narrower than a hard reset) and **verify content BY GREP, never by the SHA.** <!--
  2026-09-08 (G). The SHA proves nothing here precisely because HEAD never moved -- the one identifier a reader
  checks is the one the accident leaves correct. Neighbour of the index-restore trap below and of the
  reflect-directory checkout that reverted a lane's own guard edit: same command family, worse outcome, because this
  one produces a GREEN rather than a lost edit. -->
- **After a `-tests` run a package directory holds THREE populations — tracked corpus files, tracked hand-owns,
  untracked generated emission — so any glob- or directory-wide operation hits the wrong one. Restore by FILENAME;
  clear emission with `git clean -nd` then `-fd`.** <!--
  Paid twice, 2026-09-02: `rm -f src/core/reflect/*_test.cs` deleted the TRACKED export_impl_test.cs hand-own (the
  glob encoded "test files under a converted package are generated" -- true for 13 of 14), and `git checkout --
  src/core/reflect` reverted the lane's own guard edit in value_impl.cs. The primitive that reads the tree's state
  beats the pattern encoding a belief about it. -->
- **`git checkout -- <path>` restores from the INDEX, so a control-arm restore DESTROYS an unstaged change in the
  same file** while "restore clean: 0" TRUTHFULLY reports a match with HEAD. **Stage the cut FIRST, restore control
  arms from the index.** <!--
  2026-09-04; the sweep-restore trap met from the control-arm direction. Only then does a diff-against-index check
  mean what it says. -->
- Open converter items: `src/go2cs/ToDo.md` (`visitMapType` completion, remaining dynamic-struct implicit-cast
  checks, optional recursive dependent-package conversion, comment conversion, cgo/asm targets).
