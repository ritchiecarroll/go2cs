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
     context, so provenance kept this way costs ZERO tokens. -->

- **⚠ Banked-row protection at MERGE time — two rules, both paid for by the crypto/tls regression
  (found 2026-08-19; rooted and fixed by lane `claude/tls-regression`).** The flagship row banked
  green on its lane tip and was RED at master the moment its merge landed, because the guilty change
  (`d1ed1f7c1`, local-iface-cast) had merged to master AFTER the lane forked — each side green
  alone, the union never swept. A lane's sweep proof binds its OWN tree, never the merge result.
  (1) **A BANKING merge owes a post-merge filtered sweep of its own row at the merge RESULT** —
  `run-validated-sweep.ps1 -Filter <pkg>` on the merged master, not the lane tip; the lane-tip proof
  is necessary but not sufficient. (2) **Any reflect-bridge-touching change's canary set is the FIVE
  largest banked reflect consumers BY VERDICT COUNT — recomputed from
  `docs/ValidatedTestPackages.md` at gate time, never carried forward.** The consumer PREDICATE,
  explicit since 2026-08-29: **the package's OWN Go source — production OR `_test.go` — imports
  `reflect`** (test usage counts because the canary protects VERDICTS, and verdicts are produced by
  test code; a suite leaning on `reflect.DeepEqual` is exactly what a bridge regression breaks).
  Derivation is a grep of the GOROOT sources for `reflect` as an **IMPORT** — a name-LIST match
  over-matches (`go/doc/comment`'s `std.go` carries the name as data), so positive-control the
  predicate before using it (`encoding/json` in, `cmp` out) — plus the roster's counts, both at
  gate time. As of
  2026-08-29 it yields: `crypto/tls` 3,643 (bogo-capable hosts only; the collapsed-verdict path
  otherwise), `go/types` 557, `encoding/json` 491, `encoding/xml` 386, `crypto/x509` 341. ⚠ The
  PREVIOUS worked example here included `go/internal/gcimporter` (583) — a package with **zero
  reflect touchpoints in prod or test** — and that membership was then carried across several merge
  windows with only the counts re-read, until a lane's fresh grep caught it while holding an
  expensive sweep: the example had substituted for the derivation, which is precisely what this
  rule forbids, this time performed by the coordinator. Worked examples date and drift; the grep is
  the rule. ⚠ SECOND carried-membership catch (2026-09-01): `crypto/internal/nistec` (2,195) ALSO
  imports reflect nowhere — it had been travelling in the set beside gcimporter and was carried
  again the day before a lane's fresh derivation dropped both. "Reflect-bridge-touching" reads
  broadly: `src/core/reflect/*_impl.cs`, `src/core/internal/reflectlite`, golib's
  `GoReflect.*`/adapter/equality machinery, and the go2cs-gen adapter/shell templates all qualify.
  **And the canary RULE is now split (R's proposal, ratified 2026-09-01):** a change to the reflect
  BRIDGE takes the reflect-importer canaries above; a change to `abi.synthType`/golib **descriptor
  synthesis** takes a **COST canary as well**, because synthesis runs on every interface boxing
  corpus-wide — a blast radius the importer predicate cannot see at all (an unmemoized
  `GoPtrBytesOf` pushed nistec from 354s past its 600s deadline; the memoized re-measure is 384s).
  `crypto/internal/nistec` re-enters as exactly that cost canary: run it and compare its WALL TIME
  against the recorded baseline, not just its verdict.
  ⚠ **How a datum the MARKER loses is carried, and why it is never carried in the marker** (2026-09-05):
  where a marker's TEXT is what `go2cs-gen` dispatches on (`[GoType("chan …")]`, `[GoType("ж<…>")]`),
  a datum the marker drops — a defined channel type's DIRECTION, a named pointer-to-array's LENGTH —
  rides as a SIBLING attribute the converter stamps beside the UNTOUCHED marker and golib reads once
  per type (the `GoArrayDims`/`GoMapKeyDims` family extended from values to a defined type). Changing
  the marker's SPELLING instead teaches the generator a new prefix and buys a test-only population a
  route-#7 ladder. The descriptor synthesizer fills the stamped cargo AHEAD of the interning key so
  every route interns ONE descriptor; where a MEASURED cargo and the type's STAMP disagree the stamp
  decides and the disagreement is refused BY NAME; and descriptor synthesis takes the cost canary
  above.
  ⚠ **THE WORKED EXAMPLE IS RETIRED — the derivation IS the rule (2026-09-03, its THIRD drift).** A
  fresh gate-time derivation over the roster read `crypto/tls` 3,643, `net/http` 1,343, `go/types`
  557, `encoding/json` 491, `net` 472 — `net/http` and `net` outranking the two rows the example named
  — which is the third time a dated top-five carried past its date. Fresh top fives belong on the
  board as DATED data; this file carries only the derivation and its controls. ⚠ And the derivation
  itself sharpened the same day: derive from **PARSED import declarations**, not a line-anchored grep,
  which admitted `go/doc/comment` and `go/internal/gccgoimporter` while BOTH standing controls passed
  — both vary "imports it or not" and neither varies "mentions it as DATA", so add the control that
  pins the axis the counterexample names (`go/doc/comment` must be OUT).
  ⚠ **A canary can also be chosen by MECHANISM rather than by rank, and sometimes must be** (measured
  2026-09-03). `encoding/gob` keys its type caches DIRECTLY on `reflect.Type` identity and is banked
  at 106 green WITH a `[][6]uint8`/`[][8]uint8` identity collapse present: a banked consumer green
  with the defect **cannot detect it** (its suite never exercises the collapsing shapes — green is not
  evidence the model is sound) but **can be broken by the repair**. Such a row enters a change's gate
  list because it keys on the thing the change alters, not because a largest-importer rank would
  select it — the promoted-forwarder / `net/http` precedent generalised. **And the reflect-bridge gate
  list now carries the FULL behavioral suite** (route #7's behavioral twin, above): a bridge change
  that emits byte-identical `.cs` is invisible to CNR and to the `reflect` `-tests` build alike.
  ⚠ **Derive the canary set in a clone whose refs you have verified.** A derivation run with the
  mailbox clone as cwd read `origin/master` **15 rows behind** and produced the SAME top five by luck
  (every dropped row was smaller); only a row-count reconciliation — 178 against the guard's 193 —
  caught it (2026-09-02). The mailbox clone's non-mailbox refs are stale BY DESIGN and are never read
  for repo content; reconcile any derived count against an independent one before using it.
  ⚠ The MECHANISM, stated 2026-09-02 after a second instance: the mailbox clone is TRANSPORT, and its
  refspec carries `claude/mailbox` ALONE — so `git fetch origin master` there moves nothing and its
  `origin/master` stays pinned wherever the clone was made (one such read was 15 rows behind and nearly
  escalated as "fifteen banked Linux rows lost"). Repo content is read only from a work tree, and
  `ls-remote` arbitrates when two refs disagree. ⚠ Met again 2026-09-03 as a DEAD MONITOR ARM: a
  watcher's MASTER-CHANGED leg ran in the mailbox clone, where `git fetch origin master` moves
  nothing, so the arm was structurally dead the whole time it reported healthy. **Assert the clone's
  refspec before any fetch-based arm**, and put the assertion in the lane's own script rather than in
  its memory.
  ⚠ **Three ref-reading rules from one night, 2026-09-02.** **Name the REF you read**: a claim that
  says "at master" is read from `origin/master` AFTER a fetch, never from a branch's working tree — a
  branch's base is a snapshot of master at fork time and ages out from under every claim made through
  it (`git fetch origin master && git show origin/master:<path>`), the stale-base illusion applied to
  a single FILE rather than a diff. **After a fetch that PRINTED AN ERROR, verify the ref actually
  MOVED before reading anything off it**: a fetch dying on a clone's object corruption left
  `origin/master` unmoved, `git show origin/master:<path>` answered about the past while looking like
  the present, and "master is RED on the roster guard" was reported — falsely. "Benign for pushes"
  (they verified the remote moved) is not "benign for reads". **And an ANCESTRY question goes to a
  clone that HAS the ancestry**: a depth-200 shallow clone answered "NOT on the remote" for a ref the
  full-history repo showed contained, and it is the clone a lane reaches for by habit.
  ⚠ **FOURTH CLAUSE, MEASURED 2026-09-06: THE ARMS MUST MATCH ON LOAD.** The standard exists so that two
  arms cannot convict — and two arms NEARLY DID, because as written it does not say the arms must be the
  same experiment. All three arms of one row: **seat-present under battery load FAIL 1325/0/20 in 890 s;
  seat-absent ALONE PASS 1345/0/0 in 351 s; seat-present ALONE PASS 1345/0/0 in 348 s.** 348 against 351 is
  the same host doing the same work; **890 is a different machine in every way that matters to a
  cancellation-timing suite**, and sixteen of the twenty diverging rows were cancellation and retry TIMING.
  **A battery's FIRST LEG and a SOLO re-run are not two arms of one experiment** — the deciding arm runs
  ALONE, matching the CLEAN arm's load rather than the failing arm's.
  ⚠ **A WALL-TIME GAP BETWEEN TWO ARMS IS A TELL, NOT A CURIOSITY**: 890 vs 351 seconds was the only thing
  separating a real regression from a timing suite flaking under battery load, and it is exactly the kind of
  number that gets noted and skipped. **Compare the arms' WALL CLOCKS before attributing their verdict
  difference to the change**; when one candidate mechanism is constructible and the other is not, and a
  confound is sitting in the wall clock, the wall clock is where to look.
  ⚠ **DECLINING TO CALL A REGRESSION YOU ARE ENTITLED TO CALL IS HARDER THAN CALLING ONE, BECAUSE THE CALL
  LOOKS LIKE RIGOUR.** The lane holding two arms against its OWN seat named the load confound BEFORE running
  the third arm, stated both outcomes in advance so neither could be rationalised, and explicitly refused to
  predict — **which is what makes the pass believable rather than convenient.** The cost of the call not
  made would have been a sound seat pulled, a phantom defect handed to somebody to chase, and an
  unconstructible mechanism pursued until found to be nothing.
  ⚠ **A GATE READING HAS A TREE, AND WHEN THE TREE MOVES THE READING EXPIRES WHETHER OR NOT THE CHANGE
  DOES** (2026-09-06). A seat measured `323/58/7` against a manifest a later landing rewrote still MERGES
  clean — verified by 3-way — **but the reading describes no tree that exists.** This is the stale-base
  illusion applied to MEASUREMENTS rather than diffs, and it arrives from both directions: a base stale
  BEFORE the train, or a train that makes the reading stale after. **Re-gate, and state the expected
  numbers as a PREDICTION before re-running so the new reading cannot be a rationalisation.**
  ⚠ **A RATIO EXPIRES MORE READILY THAN AN ABSOLUTE, because it has TWO tree-dependencies — and it
  silently asserts that its two halves share a context: one machine, one tree, one configuration.** A cost
  published as "+26.075s = +12.1%" survived a landing only in its wall-clock half (the converter-suite
  baseline moved 215s → 344s, making the fraction 7.6%), and "+26.075s on a laptop ÷ 344.012s on the i7 =
  7.6%" then replaced a stale number with an **INCOMPARABLE** one: the two machine classes differ 3–4x, so
  the 215→344 gap may be entirely the boxes, entirely 53 commits of tree growth, or any mixture, and
  nothing in either post can apportion it. **Publish the absolute with its BOX and its TREE; a fraction
  whose halves cross either boundary is not a weaker figure, it is not a figure** — and the corrective is
  a MEASUREMENT, never a third estimate: **a number defended is worth less than a number re-measured, even
  when defending it would have been vindicated.**
  ⚠ **A MICRO-BENCH RESULT CAN INVERT BETWEEN HOSTS ON THE TIERING AXIS, AND A PERCENTAGE AGAINST
  DIFFERENT ANCHORS IS NOT A COMPARISON** (2026-09-08): a door cost read within the noise floor at
  tiering OFF on both hosts, and clearly positive on one host under DEFAULT tiering where the other
  read slightly negative — **the sign FLIPPED.** Half of it is rooted (every arm compiles as tier-1
  on-stack replacement with synthesized PGO, since a ten-million-iteration loop inside a method called
  a handful of times is promoted by OSR, and forcing full optimization removes OSR and halves the
  gap); the residual is labelled UNROOTED with the un-varied axes named. The probe's "percentage of
  the lower bound" line is an ANCHOR ARTIFACT — one host's anchor is a user-mode read an order of
  magnitude cheaper than the other's real kernel transition — so **cross-host percentages from one
  probe are invalid, and the row that decides materiality is a real kernel transition on the BINDING
  host.**
  ⚠ **THE EXPIRY CLASS THAT EXPIRES SILENTLY IS AN ALLOC OR BYTE READING — the row still passes, the
  assert still holds, and the figure in the commit body is simply no longer true of any tree**
  (2026-09-06). A suite total and a manifest composition both fail LOUDLY; a byte figure does not.
  Verified at the train-31 landing: `src/core/golib/` moved +301/−6 across 5 files with `ж.cs` among
  them, and the Architecture map already rules instance state on `ж<T>` a CORPUS-WIDE byte cost — so
  any `B/op`, `allocs/op` or want-N figure taken before that landing expired regardless of which files
  its seat touched. The mechanical check is `git diff --numstat <base>..<master> -- src/core/golib/`.
  ⚠ **And the re-stamp rule has a trap here: a silently-expiring reading is RE-MEASURED before it is
  re-stamped, never re-stamped from the old value** — a re-stamp is the act that makes a wrong figure
  look freshly verified. **A reading's loudness is independent of its importance.**
  ⚠ **CORRECTING AN ESTIMATE AGAINST EACH OBJECTION IN TURN IS NOT DERIVING IT, and it produces a
  sequence of numbers each wrong in a NEW way.** A census headline went 93 → 92 → 87 → UNKNOWN across
  one evening, every correction sound against the objection that prompted it while inheriting the
  untested assumptions nobody had raised yet: `87 = 93 − 6` subtracted the measured departures and
  silently assumed every other package lost nothing — false on its face, since one package held 32 of
  the 93 and its emission had changed. **The third correction is where the author stopped correcting and
  declared the headline UNKNOWN, which was the first sound statement in the sequence.** The settling
  three-target build then returned **87**, and the author's own verdict is the durable form: *"87 was
  RIGHT and publishing it earlier was still WRONG."* **A correct number reached by an unsound method is
  not evidence, and being vindicated by a later measurement does not retroactively make the earlier
  claim a measurement.** When a figure needs a third correction in the same direction, the defect is the
  DERIVATION: stop patching, re-derive, or say unknown. ⚠ Its filing companion: **a long record
  contradicts itself, and the author writing in one section does not see the other** — twice in one day
  a headline's correct value sat two sections above the paragraph being written, **both written by the
  person who had just derived the right number.** Neither author was careless; they were LOCAL. **After
  amending any figure in a multi-section record, grep the WHOLE file for the old value** — re-reading
  the section you just wrote cannot catch it, because the contradiction is never in that section.
  ⚠ **"STALE" IMPLIES A RECOVERABLE PROVENANCE; A FIGURE OFF THE LINE HAS NONE.** A claimed "689 declared"
  sat under merge-base 721, the sibling seat's 725, its own blob's 731 and master's 732 — **a tree with
  that count predates the branch entirely.** A stale reading is corrected in a sentence; **an
  unattributable one HOLDS the seat**, and it is not corrected but **SUPERSEDED** by an attributable one
  (re-run with tree, flavour, host, configuration and base all stated). Recovering the provenance of a
  figure nobody can place is unbounded work; replacing it is one run. Its tell: **two sibling seats'
  figures that do not COMPOSE are two different trees, never an inconsistency to reconcile by argument** —
  name both arms' SHAs in the record, always.
  ⚠ **THE OVERLAP TEST IS AGAINST WHAT THE READING IS A PROPERTY OF, NOT AGAINST THE SEAT'S FILES —
  and the three expiry rules are ONE rule: NAME THE BOUNDARY beside the number.** A seat whose files
  did not intersect a landing at all still carried an expired gate line, because that line quoted a
  SUITE TOTAL: 728 declared at the seat's base, 738 at the landed master after a train added ten
  GolibTests methods, 745 predicted before the re-gate and hit by both legs (2026-09-06). **A
  file-scoped reading transfers on zero file overlap; an AGGREGATE expires at ANY landing.** Ask what
  SET the number depends on, then check THAT set — and ask it of the seat's PAYLOAD, not only its gate
  lines: a byte table IS the claim its commit exists to make, and a lane checking gate lines alone
  re-gated one aggregate of four and reported the seat re-gated.
  ⚠ **A "DOES X EXIST" PREDICATE ANSWERS ABOUT THE TREE IT RUNS ON — asked about a MERGE RESULT, it must
  run on the merge result.** A census counted `//go:linkname` push entries **at master** and read ZERO for
  four symbols, concluding they were "genuine frontier, nothing was ever aimed at them" — **when the seat
  under discussion is what ADDS those pushes** (all four are bodied at master and the seat's diff adds 6,
  6, 6 and 12 lines naming them, while the symbol the census called the one true candidate gets ZERO from
  that seat). The classification was INVERTED, and **"nothing was ever aimed at them" is a claim about
  INTENT that a base-tree measurement cannot support.**
  ⚠ Three cheap tells for the LAYER, all 2026-09-06. **A lane's report that something ABSORBED is not
  evidence it LANDED** — a coordinator carried "the E4 entries absorbed" for a day while master's manifest
  held 59 entries with all three tests ABSENT, the lane's own gate cleanup having run a checkout over
  `src/core`; one count of the committed artifact separates "reported" from "landed". **The RIGHT answer
  from the WRONG instrument is the most dangerous error, because nothing about the output invites a second
  look** — a finding measured out of a tree two trains stale came out identical although all three files HAD
  changed. And **a line number off by a large fraction of a file is a CHECKOUT announcing itself** (a
  citation given as 2357 sat at ~4155 at master): a disputed citation is a LAYER question, not a typo.
  ⚠ **A MEASUREMENT is tree-specific; the ARGUMENT is what generalises — both belong, argument BEHIND the
  measurement.** A byte-identical CNR is a fact anyone re-checks in one command **about THAT TREE**; a key
  argument (the registry is keyed by the consumer's own import path, which no behavioral package can hold)
  is a property of the REGISTRY. **Drop the measurement and you have a persuasive story about an unmeasured
  tree; drop the argument and you have a fact with no reach past the day it was taken.**
  ⚠ **A RE-GATE THAT LIVES ONLY IN A MAILBOX POST IS NOT A RE-STAMP — the COMMIT BODY is the durable
  record.** A seat re-measured at 326/59/3 whose body still read 323/58/7 was measured and not
  re-stamped: transport carried the truth, the artifact carried the dead number, and the artifact is
  what a reader finds later. ⚠ **But a rule requiring a capability some lanes lack will silently not
  be followed** — history rewrite is lane-scoped, and one lane amended two bodies within the hour
  while another was refused outright — so prefer **the form that needs no permission any participant
  might lack** (an APPENDED commit over an amend), and when a lane reports it cannot comply, suspect
  the rule before the lane. ⚠ And **the re-gate is cheap when the committed artifact is already the
  BEFORE**: re-emit in place, let git be the differ, and the verdict is one `git status` — both
  differing files CR-strip byte-identical to HEAD, comparator positive-controlled on a known-different pair.
  ⚠ **BASELINE EVERY NON-GREEN AGAINST MASTER INSTEAD OF EXPLAINING IT — a BASELINED red is portable across
  a host disagreement and an EXPLAINED one is not.** A lane's `-tests` gate read RED on its host and GREEN
  on the coordinator's **at the same commit** — same OS family, opposite verdicts — and the lane's cut
  survived unaffected because it had run MASTER and found the identical error there, rather than writing
  "this is red and here is why that is fine". **The explanation is only as portable as the host it was
  formed on; the baseline is a DIFFERENCE and it travels.** Record such a disagreement as UNRECONCILED
  rather than inventing a mechanism for it.
- **Phase 3 complete (2026-07-10 — commit `51ba5d9cf`, tag `stdlib-green-2026-07-10`):** all **302**
  packages of the full conversion (Go 1.23.1) compile clean — zero errors, zero
  exclusions (`runtime`, `reflect`, `net/http`, `go/types`, `crypto/tls`, `database/sql`, … all included).
  **Compiling is the milestone, NOT operational** — operational validation is Phase 4 (running Go's own
  package tests). Campaign detail: [`docs/Roadmap.md`](docs/Roadmap.md) (Phase 3 iteration log) and the
  [`docs/README.md`](docs/README.md) NEWS section.
  - **Promotion happened, once, wholesale (2026-08-01) — superseding the 2026-07-01 defer.** That ruling
    said not to promote package-by-package on a clean compile, and it was right: the chicken-and-egg it
    guarded against was real while the corpus was unproven. Phase 3 plus 69 operationally-validated
    packages dissolved it, so the whole tree moved at once instead. The hand-owned
    `[module: GoManualConversion]` / `*_impl.cs` files now simply LIVE in the one tree — no canonical copy,
    no overlay-back step, no two-tree exceptions.
  - **⚠ Swapping a hand-own's file contents (backup restore, A/B neutering) can leave a STALE dll
    winning the build**: `Copy-Item` preserves `LastWriteTime`, so the restored source is OLDER than
    the assembly built from the neutered version and incremental MSBuild keeps the wrong dll — a
    defect then "reproduces" against clean, HEAD-matching source with a clean `git status` (measured
    2026-08-16, cost one invalid run). After any hand-own swap: touch the file or build
    `--no-incremental` before believing a repro.
    ⚠ `cp -p` is that trap's bash costume (2026-09-05) — it preserves the timestamp exactly as
    `Copy-Item` does, so the restored source is again OLDER than the assembly built from the neutered
    one.
  - **⚠ A hand-APPLIED edit to a generated file must be proven BYTE-IDENTICAL to the converter's
    own emission before it banks** (standing bar, ruled 2026-08-31). Regenerate into a seeded root
    and byte-compare the hand-applied file against the emission: the first measured
    hand-application was ONE BLANK LINE short of what the converter emits, and without the check
    the next regen reports that cosmetic delta as drift and bills a phantom investigation to
    whoever runs it. The comparison is only meaningful when the emission actually landed in the
    compared root (see the single-package output-positional trap above) and only after the gate's
    negative control has been made to fail once.
    ⚠ **Two control-FORM rules, both measured 2026-09-02.** An ABSOLUTE "byte-identical to the committed
    file" control is unsatisfiable under standing corpus drift — three banked rows' `-tests` emissions
    changed WITH a cut and WITHOUT it (closure drift plus relocation debt), so a no-op would have failed
    the gate; the DIFFERENTIAL form (emission with the change vs without) is the one that carries
    information, and the five-minute control (revert, re-emit) runs BEFORE any violated control is
    reported. And **a positive control's premise must hold at the CONVERTER the measurement used**: "the
    landed hunk must reproduce with zero diff" assumed a binary carrying the merge while the
    measurement's binary was built pre-merge — it failed for its premise, not for the instrument. Name
    the converter a control assumes, and say which form the control took.
  - **⚠ Phase-4 operational: two hand-owned patterns, and a WHOLE-FILE rewrite MUST carry the marker.** Making a
    package *run* (not just compile) often needs a native reimplementation where the literal conversion compiles
    but cannot work — e.g. `sync`'s Mutex/RWMutex/WaitGroup (2026-07-11), whose Go runtime sleeping semaphore
    cannot be emulated, are hand-rewritten on `SemaphoreSlim`/monitors. A `<name>_impl.cs` companion
    *supplements* some declarations (bodyless `partial` + a comment placeholder the converter emits); a
    **SIZE HAND-OWN VERSUS CONVERTER CHANGE BY WHETHER THE SITES TERMINATE.** A converter change is
    justified when the population is large enough that per-site work does not finish, or when the sites
    cannot be enumerated at all. Sixteen functions enumerated BY NAME from Go's own sources, eight already
    remediated in an existing hand-own file, eight open across two platforms — **that is a hand-own
    EXTENSION and the converter stays untouched**. ⚠ **And check whether the class already HAS a name and a
    home before calling a finding its first member**: this one did (`ptrout`), with its mechanism stated in
    its own file header. The tooling twin, same week: a lane sized a repo-wide identifier census and then
    found `fleetIdentifierCensus_test.go` already existed and was better — **the silent-duplication class
    pointed at TOOLING rather than at code, and cheaper to catch, because one grep answers it before
    anything is built.**
    ⚠ **There are THREE, and a displacement guard that enumerates two reports the third as a defect
    (measured 2026-09-06, three reds, zero real).** Beside the registry entry (a BODIED converted
    function) and the body written into a bodyless `partial` (self-displacing by construction) sits the
    **whole-file `[module: GoManualConversion]` marker** — where the converter never emits the file at
    all, so there is no competing body and **no registration is owed**.
    `TestManualConversionRegistrationsHaveBodies` EXEMPTED only the first two until train 31;
    `perGOOSReplaced` (`manualConversionDestination_test.go:206`) added the third, so the three reds
    it produced on 2026-09-06 are history and the count — THREE mechanisms — is what stands.
    ⚠ **THE SAFEST CLASS IN A GO-SHAPED CENSUS IS THE MOST DANGEROUS CLASS FOR A WHOLE-FILE HAND-OWN:
    a same-package type RELOCATION is invisible to Go and GUARANTEED to collide with a marker-protected
    file that still declares the type** (2026-09-07, a lane correcting its own rehearsal record). The
    hop population was sound and the classifier named the moved type MEMBERS-REMOVED; the scope rule
    then filed `MOVED-WITHIN-PACKAGE` (8 rows) as MECHANICAL — right for Go, INVERTED for go2cs. The
    refinement bounds the bill: **a relocation collides iff the hand-own RE-DECLARES Go's members** —
    a whole-file rewrite gives CS0102, while an `_impl.cs` companion carrying go2cs-invented members
    lets the partials MERGE. Measured over 142 marked files × 3 targets, comments stripped, filtered to
    Go-selected files: TWO type-level relocations, ONE collides (`runtime2.cs::note`), one merges
    (`reflect/value_impl.cs::MapIter`). ⚠ **And a collision predicate covers every DECLARATION KIND
    that can duplicate — type, func, const, var — not only the kind that motivated it**: the type-level
    predicate missed `sync/mutex.cs::fatal`, a relocated FUNCTION, found by auditing the instrument
    against the bare-name-across-a-namespace shape. Companion: **absence from a census DOCUMENT is not
    absence from its POPULATION** when the document names only the rows needing a human — check the
    artifact's DERIVATION, not its index.
    ⚠ **And a guard's RED is a claim like any other, falsifiable by the very thing it claims**: "the
    package fails CS0111 on that platform" is directly testable — the windows arm was already disproven
    by a 307-project solution build at 0 errors, the linux arm by a `--no-incremental
    -p:GoTargetOS=linux` build of that one package, also 0. **Before treating a red as a blocker, ask
    what observable it predicts and go look** — a guard that cannot be wrong is the false-green trap
    wearing the other mask.
    **whole-file** hand rewrite *replaces* the converted `<name>.cs` and **must carry `[module:
    go.GoManualConversion]`** — else a `-stdlib` reconvert regenerates the Go version over it (`main.go`'s
    `containsManualConversionMarker` drops marked files from the convert set; place it after the `using`s,
    before the file-scoped namespace). Further hand-own detail:
    [`docs/ConversionStrategies-Reference.md`](docs/ConversionStrategies-Reference.md) (the two-tree history
    is archived at `src/archived/Baseline-vs-FullConversion.md`).
    ⚠ **Two displacement mechanisms, priced differently (2026-09-02) — and read the NEIGHBOURING
    rulings before sizing one.** A BODYLESS `public static partial` (a linkname-declared destination)
    is displaced simply by WRITING a body: `PartialStubGenerator`'s predicate is
    `IsPartialDefinition && PartialImplementationPart is null`, so the throwing stub steps aside BY
    CONSTRUCTION — no `manualConversionFuncs` entry, no converter change, no two-seeded diff (though a
    change that removes generated stubs still owes a behavioral COMPILE, route #7's neighbourhood). A
    BODIED converted function is displaced ONLY through that registry — a converter change, with a
    two-seeded emission diff and a hunk-only corpus footprint. The cheap-looking third option (mark the
    whole file `GoManualConversion` and edit in place) freezes every function in the file to optimise a
    few and creates a permanent hand-merge obligation: rejected by the minimal-footprint rule even
    where the file is stable. Accessibility follows the pattern already in the tree — a `Go`-prefixed
    PUBLIC helper per operation, native mirrors PRIVATE to the seam file, so no consumer assembly sees
    a native type — and a hand-own's own "deliberately not covered" scope header is re-read and
    corrected in the SAME commit that changes the scope (one still named two functions that had been
    hand-owned three days and one hour earlier; a scope header that lies reads as the census).
    ⚠ **A DEAD CALLER GETS NOTHING, and the residue is a census assertion whose whole value is the day
    it goes RED** (2026-09-06): two of three callers of a defective wrapper had no call site anywhere,
    test emission included, so displacing them would have cost a registration, a placeholder and a body
    each to change behaviour nothing observes. Record them as MEASURED DEAD, with the assertion that
    fires the moment a forwarding property makes one live.
    ⚠ **And an EMPTY body is not a no-op when the throwing stub was a BRAKE** (measured 2026-09-02):
    empty `runtime_BeforeExec`/`AfterExec` bodies were argued correctly from `execLock`'s readers and
    FORK-BOMBED the `syscall` row (96 children in 7 minutes), because `Exec` hands `execve` MANAGED
    argv/envp, the exec'd image comes up with garbage argv and an empty environ, loses its
    `-test.run`/helper-process markers and re-runs the whole suite — and a CHAIN of child processes is
    itself proof `execve` did not replace the image (it keeps the pid). "Semantically sound" and "safe"
    are two claims: a cut touching `execve` runs under a process ceiling, withdraws first and analyses
    second, and the marshalling fix precedes any body.
    ⚠ **Which FORMATTER runs decides what a converted test PRINTS, and a hand-own's private
    reimplementation of a stdlib contract diverges silently** (measured 2026-09-02): the hand-owned
    test host carries its own verb dispatch (`TestFormat.cs`) with a SMALLER contract than `fmt`'s —
    `#` parsed and dropped, `%T` of nil as `nil` — so every PRODUCTION-dimension control was green by
    construction, because production calls the converted `fmt` and the test dimension never does. Once
    the converted package banks, the hand-own DELEGATES to it rather than carrying a second
    implementation. The measurement that settled it was a probe printing ZERO lines where the failure
    reproduced: a function the path never enters is falsified by its own silence.
    ⚠ **A FAILURE MODE `recover()` CANNOT SEE IS INFRASTRUCTURE, NOT A GO-SEMANTICS DIVERGENCE — FIX IT,
    NEVER DISCLOSE IT** (2026-09-06). The host raised a .NET `InvalidOperationException` where Go panics:
    the panic is recoverable in principle and reported Go-style, the exception is invisible to `recover()`
    and lands in the infrastructure bucket by the host's own classifier. **A disclosure would freeze the
    port's plumbing into a manifest as though it were a property of the language.** The arithmetic
    settles it: a fix recovers the verdicts and spends nothing, a disclosure recovers them by spending one
    AND freezing a host defect — and a non-row-shaped fix makes the recovered count a FLOOR.
    ⚠ **A REPORTED DIVERGENCE CAN BE THREE DIVERGENCES WITH DIFFERENT REACHABILITY, AND ONLY
    SOURCE-READING SEPARATES THEM.** Reading Go against the port for one host `Fail` found (1) **ORDER** —
    Go propagates to the ancestors BEFORE its own done check, so a late `Fail` marks the whole chain
    failed and only then panics, while the port checks first, throws, and NEVER propagates, so the same
    failure fails NOBODY; (2) **KIND** — panic versus an exception invisible to `recover()`; (3) a missing
    done check on the propagation path. **(1) is verdict-visible, (2) is the class boundary, (3) is
    unreachable — and fixing the KIND alone leaves the verdict-changing half in place.** A guard for a
    multi-part divergence therefore **asserts the HARD part** (that a late failure marks the PARENT
    failed), not the easy one: the kind is the half a reader notices, the order is the half that changes a
    result.
    ⚠ **And do not add a branch on the strength of SYMMETRY with the reference implementation if the
    branch is unreachable**: `runTests` makes every top-level test a `t.Run` on a root `T`, so mid-run
    there is ALWAYS a live ancestor — the same structural fact that makes Go's own `logDepth` panic
    unreachable. Adding the check "because Go has it" puts an unexercisable branch into a host that
    already has one too many; record the reason at the site, against the structure that makes it
    unreachable.
    ⚠ **A GUARD CAN BE FAITHFULLY TRANSCRIBED AND STILL DIVERGE, because what keeps Go's version quiet
    is a STRUCTURE the port omitted.** Go's `logDepth` panics only when the parent walk EXHAUSTS, and
    `runTests` parents every top-level test to a live root `T`, so mid-run the walk never exhausts and
    Go never panics. Our host implements the walk correctly (train 32) and starts every top-level test
    with `parent: null`, so the walk has nothing to walk and the guard fires where Go's cannot
    (2026-09-07). **The guard is right; the CHAIN is missing.** Transcribing a guard without the
    invariant that makes it unreachable is a divergence wearing fidelity's clothes: **before calling a
    ported guard "Go's contract", find the structure that keeps Go's copy silent and check the port has
    it.**
    ⚠ **A `-tests` run RE-CONVERTS every non-marked file, so a hand-own PROTOTYPE cannot be measured
    through the pipeline without its registry displacement** (measured 2026-09-03; only a
    marker-carrying `_impl.cs` survives). Packaging is therefore decided by MEASUREMENT, not by
    convenience. Two neighbours from the same arc: **displacing a `[GoRecv]` method whose receiver is
    `ref T`** (not a value) makes the converter emit the BOX-form call `Ꮡa.m(…)` at call sites inside
    `ref` bodies where no box exists (CS0103) — every prior displacement on that seam took a value
    receiver, so the shape was never exercised; the cut whose value had dropped below the fix's cost
    was PARKED and the defect routed as its own cut with the parked code as acceptance. And
    **`[module: GoManualConversion]` needs the `go.` qualifier when it precedes the namespace
    declaration** (CS0246 otherwise).
    ⚠ **A hand-own protected only by a skip-list in ONE driver is unprotected in the OTHER** (measured
    2026-09-03): the hand-owned test host `src/core/testing/*.cs` carried **zero** `[module:
    GoManualConversion]` markers and `-tests` was unguarded on `testing`, so a mistyped `go2cs -tests
    … testing` would regenerate over the Phase-4 host and mint the F15b two-testing-packages collision
    — and it is not hypothetical: the marker saves the FILE (SHA-identical beside a `.cs.auto`) but
    not the ASSEMBLY (56 errors, 25× CS0111, the collision measured). The `testing` row's EXTERNAL
    variant therefore cannot be measured by the canonical `-tests` command at all — the conversion's
    natural output path IS the hand-owned host's directory, so the run rewrites `testing.cs` and dies
    in the publish with only a manifest written: *unmeasured because the instrument clobbers its
    subject*. The `-tests` REFUSAL on `testing` is the real guard, with an explicit census escape that
    demands a scratch output root. Two rules beside it: a host defect that Go's OWN suite finds
    (`Setenv`/`Parallel` ordering raised as a .NET exception, text truncated at the semicolon, no
    reverse guard) is **FIXED, never disclosed** — disclosing launders a bug into a class; and a
    "race" test running with `race.Enabled == false` on BOTH sides collapses to a count-of-zero
    assertion and is not a genuine exclusion.
    ⚠ **The host must print NOTHING a RE-EXECUTED helper's reader can see that Go's binary would not
    print** (measured 2026-09-04): a results-file flush reporting through the PRINTING reporter wrote
    `PASS … exit status 0` onto the stdout of every re-executed helper — the one stream `os/exec`'s
    tests read back — and 22 helper-stdout readers went RED while the target row read clean. Go prints
    nothing on `os.Exit`, the `PASS` line is `M.Run`'s own, and the fail action a non-zero status
    implies is the PARENT's (`go test`'s) to append. **A host-side change to what the process emits on
    exit is measured on the RE-EXEC rows (`os/exec`, `syscall`, `flag`) before it banks**, not only on
    the row that motivated it — and a CONTROL row that fails after a fix is the control doing its job:
    the reading is the mechanism it quotes.
    ⚠ **A refusal at a class-B/C site is a PANIC, not a plain exception** (2026-09-03): the host
    classifies a non-panic exception as an infrastructure error, which is unbankable AND a lie (the
    host is fine). Two companions from that increment: synthetic PCs are minted from the canonical
    HIGH half of the address space so a dereference FAULTS rather than reading a stranger's memory,
    and each function owns a 4 KiB span because the corpus does arithmetic on PCs (`+ sys.PCQuantum`,
    `+ 1`) — a one-value map resolves neither expression. **Reading the tree first turned a symbolizer
    increment into a RECONCILIATION**: `runtime/managed_impl.cs` already carried the whole traceback
    surface, and a second symbolizer was one afternoon away. Three independently minted token spaces
    (caller frames, a 32-bit managed-pointer hash, synthetic PCs in the high half) are disjoint on
    64-bit and COLLIDE on 32-bit — a latent defect in a just-landed increment, found by its own
    follow-up census and remedied by throwing at mint time on 32-bit so it cannot rot. **A guard for a
    RESOLUTION change asserts RESOLUTION** (token → its own function; neighbour → not; a caller-space
    token still routed to the caller table), with the name as confirmation only. ⚠ And **a ruling
    that names a DATA SOURCE states HOW that source is reached from the input at hand, or it is a
    guess**: "the file comes from the map record" was falsified by one measurement — every route into
    the `GoPositionMap` records starts from a live frame's PDB file name and the records are keyed by
    C# FILE, so a synthetic PC, which has no frame, cannot read a file with what the tree has.
    ⚠ **A registration SPLIT from its corpus footprint is refused by the converter suite itself**
    (`TestManualConversionRegistrationsDisplaceSomething`, whose witness is the on-disk placeholder),
    so "registration and footprint are ONE commit" is enforced by a guard rather than by memory — the
    `syscall.Uname` silent subtraction caught at the converter suite instead of days later at a red
    corpus. **A branch that looks seatable and is RED at the converter suite is posted as HOLD by its
    author before anyone assembles it.**
    ⚠ **THE HAND-OWN SET IS DECIDED BY THE PROTOCOL SPAN, NOT BY THE BOX CENSUS** (2026-09-04). A
    census answers where an ALLOCATION happens; it never answers who else must AGREE about the word —
    so when a hand-own changes a synchronisation MECHANISM the unit is every function that touches it:
    `rwlock`/`rwunlock` hand-owned onto an inline gate while `increfAndClose` still released through
    the side table lost every close-time wakeup, caught by Go's OWN `internal/poll/fd_mutex_test.go`
    (`TestMutexCloseUnblock`, Go=pass C#=fail at its own 10 s deadline) and fixed by the THIRD
    displacement. ⚠ **A hand-own inside a BANKED package has its guard already written**: the row's
    own validated suite is the standing gate, so run that row BEFORE believing the cut, and **read the
    row's DESCRIPTION first** — the named test's blocked-reader wakeup was readable before anything
    ran, and a synchronisation word's protocol span is readable off the banked row's test SET, not
    only off the source. **A guard Go SHIPS for the seam** — oracle-compared, authored upstream —
    beats a hand-written probe on every axis, so look for one before writing one: red-before /
    green-after on a guard NEITHER lane wrote is the strongest form of the red-first bar. And a
    CONDITIONED prediction is resolved by READING the row before the run wherever the condition is
    checkable there, so the branch cannot be chosen to fit the measurement afterwards.
    ⚠ **Two registry rules from the same arc** (2026-09-04). **A ROUTE re-scores every box the body
    FORMS, not only the ones the design targeted** — a `ref` receiver cannot form ANY field-address
    box, so a ref-receiver hand-own collects the state word's atomics boxes along with the semaphore
    boxes it was cut for (a prediction corrected a THIRD time BEFORE the run, by the lane's own
    falsifier firing on its own change; retracted-and-restated is the only honest form) — and
    `[GoRecv]` on a ref receiver GENERATING the `ж` overload is what lets such a hand-own be ADDITIVE
    with zero call-site edits, the other face of the box-form call trap above. **And a registry can
    serve TWO ROLES of which only one is gated**: REGISTERING an unexported ref-primary is correct and
    useful (same-package callers consult it), while PUBLISHING it is a promise to nobody, since no
    foreign assembly can name the type — the hand-own registration path had never applied the exported
    bar the converter's own selections always had, which did not matter until an increment registered
    something unexported. **Gate the ROLE, not the entry.**
    ⚠ **And a THIRD, on the CALL SITES a registration re-emits** (2026-09-05). A displaced
    BOX-receiver method is re-declared as a `[GoRecv]` REF receiver, so the generators mint both the
    promoted forwarder and the `ж<T>` twin and every call form binds — but the converter's manual
    box-receiver arm spells `Ꮡ<X>.<method>()` for ANY registered method with a box in scope, a
    PROMOTED selection through an embedded field included, which no receiver form can bind and whose
    forwarder the name-collision skip refuses. **The arm is gated to DIRECT selections**
    (`len(sel.Index()) == 1`), a promoted selection falls through to the hop machinery, and the guard
    asserts the displaced and undisplaced emissions of a promoted call AGREE. Two rules with it: **a
    stated remedy is checked against the LANGUAGE at the emission before the cut** — a `ref` returned
    from a property cannot observe the last of three header stores, so a header WRITE is a
    displacement, never a materializing box — and **a registration that changes how a registered
    method's CALL SITES emit owes a `-tests` emission census of every BANKED row whose test reaches
    the method through an embed**: one such registration had master's converter emitting the box form
    for `internal/poll`'s banked test (compiling only because the generators minted a forwarder
    there), so the committed test source and master's emission disagreed for a day with every standing
    gate green, and the production two-seeded diff is structurally blind to it. The prediction scored
    against that census missed in BOTH directions (a premise error on the base arm; a real move):
    **the census, not the prediction, is the instrument.**
  - **⚠ The S1/CS0030 "architectural wall" was a FORK, not a wall (2026-07-01) — and the fork held to 302/302.**
    **Native-type** pointer/unsafe ops (identical memory semantics in both GC languages) get a faithful
    conversion in the converter/`golib`. **Managed-referent** cases (`guintptr`/`muintptr`/… hiding a managed
    pointer in a `uintptr`) hold the `ж<T>`/`object` **directly** (like `core/sync/atomic` `atomic.Pointer<T>`),
    never a `nuint` round-trip. Genuine **raw-metal on non-native types** (memory-layout math, type-descriptor
    walking, `*.asm`) is stubbed with `[module: GoManualConversion]` (a stub that compiles is an acceptable
    milestone solution).
  - **Next — Phase 4 (operational):** convert and run Go's own `_test.go` suites against the compiling
    packages; design in [`docs/TestingInfrastructureRequirements.md`](docs/TestingInfrastructureRequirements.md)
    and Phase 4 of [`docs/Roadmap.md`](docs/Roadmap.md). The `-tests` pipeline is live (`go2cs -tests
    -test-action all <goroot-pkg> <src/core-pkg>`): converts `_test.go` variants, builds a
    hand-owned `go.testing` host (`src/core/testing`), runs it isolated, and diffs terminal results
    against `go test -json`. `-tests` **always forces `-comments`** (test conversions are derivative
    works — the per-file Go copyright header must survive) and **self-locates `$(go2csPath)`** by walking
    the output dir up to the tree root (so the two-arg command works from a bare clone, no env). First
    validated package: `unicode/utf8` (2026-07-17, tag `utf8-tests-green-2026-07-17`).
  - **⚠ Validated-package commit policy (2026-07-17 user ruling):** when a package's Go test suite
  ⚠ **PHRASE A DISCLOSURE'S RETIREMENT CONDITION IN TERMS OF THE MEASUREMENT, NOT A MECHANISM** —
  "the reading reaches its want", never "when the object count reaches zero". The `os` deferred entry
  survived a correction four hours after banking precisely because its condition was indifferent to
  WHICH branch of the helper reports; the mechanism-phrased version would have been dead on arrival.
  **That property is normally invisible because it is normally not tested.**
  ⚠ **A LOOSER PIN IS NOT A MORE FORGIVING DISCLOSURE — IT IS A WIDER HOLE.** Two versions of the same
  three entries differed only in signature tightness (one missing a `**uintptr` type prefix, the other
  the trailing colon that ends the stable prefix). The looser pin can absorb a DIFFERENT failure of the
  same shape — **the laundering hazard in its quietest form: it does not launder a KNOWN bug, it
  silently adopts a future UNKNOWN one, and nothing ever fires.** Tighter wins; the tie-breaker beyond
  that is which version was gated at both configurations at current master with entry honesty asserted.
  ⚠ **A DISCLOSURE WITH NO GATE THAT CAN RETIRE IT IS A PERMANENT CLAIM, NOT A MEASUREMENT** — no check
  of any class currently verifies that an entry names a test that is actually FAILING in the run, so a
  stale entry over a row that now passes is accepted silently everywhere. **The orphan check that
  closes it keys on a TERMINAL PASS**, with no-verdict, infrastructure-error and deadline-killed rows
  excluded BY CONSTRUCTION: a "does this entry name a test that no longer fails?" predicate would fire
  on every row behind a host-killer (797 on one package in one afternoon, 221 on another the night
  before) and report a wall of stale disclosures on **exactly the rows where the entry is most likely
  still correct and merely unreachable** — a total inversion. Positive evidence, the same clause the
  neighbouring mint rule draws. (⚠ And the first framing of this gap — "the mint check is scoped too
  narrowly, widen it" — was WRONG and was adopted by a coordinator without reading the function: that
  scoping is deliberate, it reads committed PROOF PAGES for a cross-platform hazard, and widening it
  would point a proof-page instrument at a within-run question.)
  ⚠ **A DISCLOSURE SIGNATURE PINNED TO A TEST'S ONLY FAILURE TEXT CARRIES NO DISCRIMINATING INFORMATION
  — it means "this test failed", and it will absorb any future regression silently.** `unique`'s
  `TestMakeClonesStrings` has exactly ONE `t.Fatal` in the whole function, on the `time.After` arm, so a
  `strings.Contains` pin on its message degenerates to *"this test timed out"*; the admission path is
  the generic Go=pass/C#=fail arm and `codegen-liveness` is free text with no class-specific tightening,
  so nothing narrows it (2026-09-07). **Before pinning a signature, COUNT the assertions in the Go test:
  a single-assertion test cannot produce a discriminating pin**, and the disclosure needs a different
  anchor — the measured mechanism, an arm-pair, or a class the matcher tightens. ⚠ **And a
  measurement's HYGIENE says whether the numbers are real; it says nothing about whether the disclosure
  those numbers rest on is HONEST.** A bank workflow measured `unique` at 19 matched / 1 disclosed / 0
  undisclosed / 0 empty with exemplary hygiene — tail read in every spelling including escaped,
  freshness proven by mtime inside the run window, encoding checked before believing a zero, cold start,
  namespace misroute ruled out — **and the row still did not bank**, because two of three adversarial
  lenses refuted the disclosure on independent grounds. Verify the disclosure separately, adversarially,
  and by someone trying to break it.
  ⚠ **GATE PLACEMENT: the cheap gate should catch the merge and the expensive one should be the
  backstop, and we had it backwards.** A duplicate disclosure entry is REFUSED by the loader — so a
  dead row rather than a silently wrong one — but the loader runs only when somebody sweeps the
  package, days after the merge, and presents as a CONVERSION ERROR ON A ROW rather than a merge
  defect. The format guard that runs on every roster change walks all 45 manifests and every entry and
  **does not assert name uniqueness**: a gate standing next to the answer and not asking. **A gate's
  value is not only WHAT it catches but HOW EARLY and HOW CLOSE TO THE CAUSE.**
  ⚠ **THE ALLOC ASSERTS ARE DENOMINATED IN BYTES, NOT OBJECT COUNTS, AND THE TWO WANTS BEHAVE
  OPPOSITELY** (measured 2026-09-06 against `AllocsPerRun`'s own documented three-case rule).
  `AllocsPerRun` reports `max(1, allocated/runs)` with INTEGER division and falls back to the
  BYTE-derived figure when the object count is zero but bytes are not — so **a want-ZERO assert passes
  iff allocated BYTES are zero**: a 24-byte box cannot hide in it, and the deferred class's meter is
  sound. **A want-ONE assert, over 100 runs, passes on any total under 200 bytes** — two orders of
  magnitude of headroom, invisible unless you read the arithmetic, so a row reading "want 1 got 152"
  needs 152 B/run down to ≤1 and not down to zero. **Reading every alloc row through the want-zero lens
  writes off a much softer population**; state BYTES wherever a banked row's wording implies counts.
  ⚠ **A THRESHOLD BELONGS TO A TEST, NOT TO A ROW — carrying one test's bound onto another is a READING
  error wearing arithmetic's clothes.** `TestMapAlloc` asserts ≤ 10; `TestDeepEqualAllocs` asserts
  **ZERO**. Both live in `reflect`, both are alloc asserts, and a coordinator holding "Go's ≤ 10" from
  the first read a measured **9** on the second as *crossing the threshold* — concluding a retirement
  where the row still fails outright (2026-09-07). The arithmetic (`9 ≤ 10`) was correct and irrelevant.
  **Re-read the assertion in the test under discussion before comparing anything to it**, and when a
  bound is quoted second-hand, NAME the test it came from so the substitution is visible.
  ⚠ Two consequences. **Driving one unit to zero can move a failure to the OTHER unit's branch rather
  than fixing it** — taking golib's counted objects to zero while bytes remain nonzero moves a want-zero
  assert from the count branch to the byte branch, where it still fails, so object-count work alone can
  NEVER retire such an entry; at ~106 bytes per counted object, well over a hundred bytes per run sit
  outside the counter entirely, and the arc needs a BYTE-denominated measurement before it needs a fix.
  **And a ROW IS NOT AN ASSERTION**: a "cheapest want-zero row in the package" claim was wrong by
  construction because one of the row's TWO `AllocsPerRun` blocks was measured and called the row — the
  other was per-insert map allocation, 1502/run against a want of 10. **Size a row from all its
  assertion blocks before calling it cheap.**
  ⚠ Method note worth keeping: **read the instrument's own documentation before asking the fleet or
  reasoning from a model.** "Can an alloc assert pass while the code genuinely allocates, since the
  counter is blind to boxing?" is answered by name in `AllocsPerRun`'s documented rule; the lane that
  answered said it CHECKED rather than reasoned, and that from first principles it would have got it
  wrong.
    **validates** through the pipeline, COMMIT its converted C# test sources into
    `src/core/<pkg>` beside the production code — `*_test.cs`, `package_test_info.cs`,
    `go2cs_test_host.cs`, `<pkg>.tests.csproj` — so the passing suite is **visible and reviewable on
    GitHub**, and reproducible via the [README "Try it yourself"](docs/README.md#try-it-yourself--validate-a-converted-test-suite)
    instructions. The pipeline's regenerated inputs/outputs are **git-ignored** by
    `src/core/.gitignore` (the staged `*.go` source copies + `go2cs_test_manifest.json`
    [machine-specific exe-hash digest] + `go2cs_test_comparison.json` +
    `go2cs_test_results.json`/`.xml`). The production
    `<pkg>.csproj` also updates on this run (the IP-4 test-artifact `<Compile Remove>` exclusion) — that
    change is intended, not drift. Refresh the committed test sources at each milestone rebank alongside
    the production tree.
    ⚠ **The committed test sources, proof pages and README badges ARE the WINDOWS record; a Linux-axis
    bank lives in the ROSTER ANNOTATION only** (measured 2026-09-03: 22 `*_windows_test.cs`, zero
    `*_linux_test.cs` corpus-wide). A Linux sweep rewrites the README badge and that rewrite is
    RESTORED, not banked.
    ⚠ **What a roster ROW must SAY — four rules measured 2026-09-06.** **A banked row's DENOMINATOR is
    a coordinator question, answered by MEASUREMENT**: a hand-owned package banked 37 tests against an
    upstream suite whose files it carries six of eleven, and the discriminator turned out MECHANICAL —
    every carried file is the EXTERNAL test package and three of the five absent are the INTERNAL one,
    a principled boundary rather than an accident of effort (four of the five cannot convert or
    contribute no verdicts; the fifth is a real bounded gap). Write the denominator and the per-file
    dispositions INTO the roster beside the row: **a banked row that states what it COVERS is worth
    more than one that merely reads finished.** **A row that states a RATIO states its UNIVERSE, and a
    row carrying TWO denominators names what each ranges over IN THE SENTENCE** — verdicts within the
    files we carry is not files within the upstream suite, neither is derivable from the other, and the
    absent files' verdicts belong to NEITHER count because the oracle emits only over files present; a
    reader who conflates them gets a number wrong in the direction that FLATTERS us, which is the one
    direction an honesty claim cannot afford (position and context had been doing that work, and stop
    being safe the moment a row carries two). **Deepening a banked row is not banking a new one** — the
    objective is a ROW metric, so a bounded increment adding up to seven tests to an already-banked row
    queues BEHIND the rows still unbanked however cheap it is, and that is said explicitly when it is
    queued or it competes for attention on its size rather than on its place. And **a green STATES ITS
    LIMIT in the same post as the green**: the roster guard's 611 passing checks cover a row's
    structure and its arithmetic and CANNOT check that its prose is true — that rests on the per-file
    reads behind it, each naming what it read — because a green allowed to imply more than it measured
    is how a roster stops being trustworthy.
    ⚠ **AT A CORPUS HOP THE ROSTER IS KEYED BY THE NEW RELEASE'S PACKAGE IDENTITY** (`go list std` at
    the pin; ruled 2026-09-07). No row follows production or tests across a split, the outgoing counts
    are the FROZEN ANCHOR and not a target, and a cost canary whose suite no longer exists is
    RE-BASELINED by measuring every candidate package once — a slowed-mechanism control deciding which
    one carries the cost — before the rule's name and number are edited, dated. ⚠ **And a moved TEST
    destination is traced by TEST-FUNCTION NAME, with the arithmetic closing to zero residue
    (`crypto/internal/mlkem768`: 9 + 6 + 1 dropped = 16), never by file name**: a file-name match can
    agree with a moved file that was hollowed out, and `find -name | head -1` picks the first of
    several — a first pass done that way would have reported 4 splits and sent a lane into `cmd/`.
    **A production-file successor derivation is BLIND to where a row's VERDICTS go**: 3 of 10 banked
    rows split at 1.24.13 (2026-09-07).
    ⚠ **A CONVERTER-SIDE CAPABILITY ALLOW-LIST CAN SILENTLY EXCLUDE TESTS, AND A ROW THAT REPORTS A
    SMALLER DENOMINATOR RATHER THAN A FAILURE IS THE QUIETEST WAY TO LOSE VERDICTS THERE IS.**
    `supportedTestCapabilities()` (`testConversion.go`) is keyed on the receiver's NAMED TYPE, and a
    test calling a member not on it is **converted, never registered, never run** — so a hand-owned host
    can implement a member PERFECTLY and every affected test stays SILENTLY EXCLUDED: no red row, no
    empty verdict, no signature to grep, just a smaller total nobody questions. **The function's own
    comments already record the price twice** — `T.Deadline` excluded SIX of `context`'s cancellation
    tests, and the missing `TB.*` spellings gated out **26 of `os/exec`'s tests, every process-spawn
    shape the package has, which had NEVER RUN.** Measured 2026-09-07 at 2,425 verdicts across the three
    Go 1.24 members (`Chdir` 53 sites / 6 rows, `Context` 5 / 2, `Loop` 15 / 2). **Widening the
    allow-list is not plumbing — it is the trustworthiness of every number the roster reports**, and
    roster impact is measured BEFORE any widening.
  - **⚠ Disclosure-manifest doctrine, seven rules measured 2026-09-03/04.** (1) **A per-package
    disclosure manifest is ONE file shared by every platform, so any REMOVAL is a cross-platform
    edit** whatever evidence motivated it, while additions are safe: the first Linux annotation
    refresh to remove an entry (a row passing on Linux under Release+TC0) turned the WINDOWS row red
    on the next union battery with exactly that one unabsorbed mismatch. Read the OTHER platform's
    preserved record for the row before removing an entry, and RESTORE an entry that still fires
    elsewhere — an entry that is present but does not fire is not counted and changes nothing on the
    platform that retired it. The durable form is platform-scoped entries (schema plus reader) so a
    per-platform retirement is expressible without touching the other platform's absorption. (2) The
    fix has its own trap: **an entry is a multi-line JSON object, not the two lines a string-occurrence
    grep counts, and a manifest that does not PARSE reads as NO disclosures** — worse than the failure
    being fixed, since it strips every other entry's absorption too. A manifest restore is whole-file
    (valid only when the seat's sole delta to the file is the removal, asserted by counting the commits
    that touch the path), numstat-checked, and PARSED before it is committed; an existence guard ("a
    disclosed count has a committed manifest") cannot see entry- or platform-level correctness and
    must not be read as if it did. (3) **A disclosure CLASS the comparison cannot ADMIT is a guard
    that cannot go green, one layer down**: `matchTerminalStatuses` unlocked a Go=pass / C#=skip pair
    only for `platform-skip`, so three committed `cgo-configuration` entries written for exactly that
    shape could never fire — invisible while the bank host ran cgo ON, where the oracle skipped those
    tests too and they matched skip/skip. Ruling cgo OFF made the class LIVE for the first time, which
    is how a dormant class surfaces: **a configuration ruling exercises manifest entries the bank never
    did.** The durable fix is to make the pipeline ADMIT the class (the entry's name carries WHY — the
    axis — and re-labelling to an admitted class would throw that away), with a positive control that
    a MISSPELLED class stays unabsorbed. (4) **Two seats that are only honest TOGETHER are one train**:
    a re-annotation whose entry carries a class the pipeline cannot yet admit lands its row failing by
    construction, so the assembly gates the first on the presence of the second rather than flipping
    the class in the interim (a flip-and-flip-back leaves a landed master with a mislabelled entry).
    (5) **A row that cannot say WHY it skipped is not disclosed, it is unmeasured** — a Go=pass /
    C#=skip on a row whose host wrote NO results file has an unreadable converted-side reason (the
    comparison record carries verdicts only), so the entry's class waits on a direct-host read.
    (6) **A SAME-FAMILY disclosure is written only from the row's OWN results-file signature, never
    from the family name** — "its 37 siblings are alloc-profile" would have hidden a corpus-visible
    NAME defect (`reflect`'s `Type().String()` dropping an array's LENGTH at an element position, a
    production wrong answer). **Fix, then disclose**; and a `%T`/`TypeOf().String()` name change is
    reflect-bridge-touching AND owes the behavioral OUTPUT phase.
    (7) **A capability-registry KEY is PINNED PER ENTRY with the package clause its GOROOT file
    declares** — internal tests bare, external tests suffixed (2026-09-04). A guard premised on "every
    gated test lives in an external package" REJECTED a correct internal key, and accepting BOTH
    spellings would have discarded its silent-mis-key protection: three negative controls, each
    mis-spelling rejected, are what make the widened guard a guard. Its admission bar: **a
    test-liveness finding in the codegen-liveness shape** (a finalizer a test blocks on forever)
    **takes that class's one-axis Debug A/B as its ADMISSION before an entry is minted**, since this
    family's most convincing story was measured FALSE once.
    ⚠ **A manifest is a STRUCTURED file: it is PARSED, never matched with a spacing-sensitive TEXT
    pattern** (2026-09-05, sharpening rule (2) above) — one manifest spelling a key with TWO SPACES
    silently dropped 54 of 176 disclosure entries from a census, and the only thing that caught it was
    a second derivation being on the table to disagree with. **A count that disagrees with another
    derivation is an INSTRUMENT BUG until proven otherwise.**
    ⚠ **THE ALLOCATION-DISCLOSURE LABEL IS DECIDED BY THE METER, NOT BY THE BOUND** (owner-delegated
    ruling, 2026-09-05). An **incomparable unit** — a byte-derived shim where Go counts OBJECTS — is
    alloc-count-semantics with nothing to retire; **the same meter with a named mechanism** is DEFERRED
    plus a plan; **the same meter with a stated proof** is STRUCTURAL; **none of the three** is a
    reading OWED and no label at all. A want of ZERO and a want of ONE are the same question. Four
    mechanics the ruling carries. The meter CLAIM inside an entry's text is VERIFIED before any entry
    rests on it — ⚠ and per-ENTRY, because **a measurement unit can be a per-entry RUNTIME property
    rather than a per-instrument static one**: the shim reports Go's own meter (a COUNT) when its
    counter saw the allocations and a byte-derived figure when it saw NONE, since reporting the zero
    would be a FALSE PASS, and it NOTES which, with both numbers, on every nonzero result — so "verify
    the unit once per path and let entries inherit it" is unsatisfiable by construction. **A FLOOR is a
    labelling hazard**: a nonzero-byte result reports AT LEAST 1 deliberately, so an assert wanting 1
    that reads 1 may be sitting ON the floor rather than agreeing — an entry whose measured value
    equals BOTH its want and the floor takes NO label until the raw numbers behind the note are read.
    **AN ABSENCE IS A MEASUREMENT TOO**: "no record exists for this family" was asserted inside an
    otherwise three-ways-derived census without opening the file the entries themselves cite BY NAME
    and section number (it existed, at 979 lines, with a staged plan), and for a DEFERRED disclosure
    the question is never "does a record exist" but **"does a stage REMOVE the allocation the assert
    counts"** — reducing the cost is an addendum's justification, not a retirement. And **"AMORTIZES"
    and "REMOVES" are the same thing to an allocation counter under ONE condition, which is a property
    of the TEST**: a per-referent cache drives the steady-state average to zero exactly when the
    asserted closure REUSES its referents across iterations, and cannot help when each iteration boxes
    fresh values — so a structural-vs-deferred call on an amortizing plan is MEASURABLE (read the
    assert's closure) and a coordinator hands back the AXIS rather than a verdict. ⚠ But **the
    amortizing plan's own ELIGIBILITY RULES decide before the test's shape does**: one family's
    closures reused their referents perfectly (which by itself said "deferred") while the record's
    MANDATORY rule — a shell over a VALUE type may not be cached, since its constructor copies the
    struct out of the box and a cached instance would freeze a snapshot — makes the cache refuse
    exactly the referents being reused, so the family SPLIT (value-type rows STRUCTURAL on a three-part
    proof inside the record; no-boxed-value rows DEFERRED against the one proposal that REMOVES an
    allocation rather than amortizing it).
    ⚠ **THE METER BELONGS TO THE QUESTION, and establishing which meter matters for one question does
    not license carrying it to the next.** An `AllocsPerRun` ASSERT is denominated in BYTES; a ratified
    deferral's FLOOR is denominated in OBJECTS. A lane spent a shift proving the byte meter was the one
    that mattered, then judged an arm worth "88 B of 12,808, 0.7% — not worth a hand-own" and dropped it
    — when that arm was the ONLY one that moved the scalar rows 3 objects → 2, i.e. onto the FLOOR,
    converting `deferred` into `structural`, which never re-opens (2026-09-07). **Name the question
    first — does this move the assert, or does this reach the floor? — then take the meter that question
    owns.** ⚠ **A change that converts a `deferred` disclosure to `structural` OUTRANKS one that moves
    the assert**: a deferred entry carries a standing obligation (re-measured every sweep, regression
    fails the row, a plan that must stay executable) where a structural entry is a closed proof. ⚠ And
    **ZERO OBJECTS IS NOT ZERO BYTES**: reaching the object floor retires nothing on its own, because
    `TestDeepEqualAllocs` asserts on the byte-derived result and all 38 subtests still failed — **the
    floor is a CLASS condition and the retirement is a separate PER-ENTRY question.**
    ⚠ **AN ALLOC HARNESS THAT SWITCHES UNITS WHEN A COUNT REACHES ZERO MANUFACTURES A PHANTOM REGRESSION
    — read the PER-ENTRY unit note, never the bare number.** Two rows read `master 1 -> seat 240` and
    `master 1 -> seat 496`, a 240x regression in the seat's own footprint. It was neither: those rows'
    golib OBJECT counts had reached zero, so the harness fell back to reporting **BYTES**, and the two
    runs' own notes say so in different words. **A number is comparable only to another number in the
    same UNIT, and a harness may change the unit silently on the SUCCESS path** — the direction that
    looks like catastrophe is the direction improvement produces. ⚠ **And a FLOOR and the instrument
    that reads it must SHARE A UNIT, or "the reading EQUALS the floor" is not evaluable**: the ratified
    floor is *2 boxes at the `any` seam* while the host reads a golib OBJECT count that the same ruling
    records as blind to CLR boxing, and two rows measured **1 object at master — BELOW a structural
    lower bound of 2**, impossible if reading and bound shared a unit. So a counter reading of 2
    establishes nothing either, sixteen rows meeting the condition AS WRITTEN do not meet it AS MEANT,
    and `structural` — the label that never re-opens — is the last place to accept a unit mismatch.
    ⚠ **A CENSUS THAT REFUSES TO BUCKET ITS UNCLASSIFIABLE ROWS IS WHAT MAKES A RULING POSSIBLE**: those
    two rows were reported as *"I am not classifying them in either direction"*, and had they been
    quietly placed either way the unit divergence would have been invisible and sixteen entries
    reclassified on it. **A third bucket named "cannot be assessed" carries more information than a
    forced binary**, and a census's refusals are as load-bearing as its counts.
    ⚠ **Five more, 2026-09-06, all about the ENTRY rather than the file.** (8) **A disclosure that
    cannot MATCH is not inert — it is a NEW RED**: a parent test whose fail event carries a null output
    emits nothing of its own, absorption is a substring match against the entry's signature, and an
    empty signature is refused at load for every class but one, so no signature can ever match it —
    measured in BOTH directions (WITH the entry, seven undisclosed rows and the parent reported as not
    matching its own disclosed signature; WITHOUT it, six and the parent ABSENT). **Never add an entry
    whose signature cannot be satisfied**, and measure both states before assuming an unmatched entry
    is harmless. (9) **The DISCLOSED-PARENT AGGREGATION is the mechanism behind that**: a parent rides
    its children when ALL of its subtests are disclosed and does NOT when even one leaf remains
    undisclosed, so **the remedy for a failing parent is to enter the missing LEAF, never a parent
    bookkeeping entry** — named in the converter's own source, where another package pins twenty-five
    leaves and lets two parents ride it (the finding shrank its own queued re-bank list from two
    entries to one). (10) **A stale SIGNATURE fails safe; a stale REASON does not.** A disclosure whose
    pin no longer matches stops absorbing and the row goes honestly RED, which is how two stale entries
    were found at all; a disclosure whose REASON has drifted off its own failure, with a signature that
    still FITS, keeps absorbing while explaining the wrong thing — a row reading green for a cause that
    is no longer its cause. **Re-derive the REASON against a fresh measurement before re-pinning the
    signature, even when the reason looks obviously still true** — the entry a lane called the SAFE
    half was carrying exactly the hazard it had described one post earlier, already authorised on that
    reading. (11) **The strongest disclosure is one whose own control proves the underlying defect
    ABSENT**: a `reflect` row exists to catch a zero-sized return aliasing the next result's storage,
    and the cleared-slot arm COLLECTED — which could not have happened had the alias existed, since the
    value under test still holds the first result across the collection — so the entry documents a
    frame-slot pin AND measures the defect the test was written to detect as absent. That is what makes
    it a disclosure rather than a hiding place. (12) **Check the SCHEMA before writing into it, and do
    not invent a field to carry a ruling's wording**: an execution annotation is a ROSTER ROW field
    parsed by the sweep's module, not a manifest field, so it lands at BANK time — and the class in
    question needed no allow-list entry at all, i.e. no converter change. Written the same evening an
    inexpressible entry was measured to CREATE a red.
    (13) **`platform-skip` REQUIRES A PLATFORM CONDITION IN GO'S OWN SOURCE — a CAPABILITY self-check is
    not one, however exactly its text matches** (owner ruling, 2026-09-07). `net/http/pprof`'s
    `TestDeltaProfile` carries BOTH shapes in one function: one skip is conditioned on
    `strings.HasPrefix(runtime.GOARCH, "arm")` — genuinely platform-conditioned — while the other fires
    on `if !seen(p, "mutexHog1")` with the text *"mutex profile is not working"*, which fires on ANY
    platform whenever the profiler yields nothing. Go passes because its profiler works; the converted
    side skips because ours returns a well-formed profile with zero samples. **It has every mechanical
    marking of an admissible disclosure — Go=pass/C#=skip, the skip text matching Go's source exactly,
    so it sails through the signature test — and is none of one.** Admitting it would make any
    unimplemented feature Go happens to guard with a self-check disclosable, turning the class from
    *cannot* into *haven't*. **Read WHICH CONDITION the skip tests, not merely that Go's source
    defines it.**
    (14) **A RULING THAT NAMES A CLASS THE SCHEMA DOES NOT DISPATCH ON IS A SILENT NO-OP WEARING A
    RULING'S AUTHORITY** — and the loader's non-emptiness validation is what makes it the WORST
    outcome: accepted, landed-looking, inert (measured 2026-09-07). The coordinator ruled two HANGING
    tests admitted as "capability entries, class REPRESENTATIONAL"; at `testConversion.go` only
    `host-fatal` reaches `hostFatalSkipExpression` (:6531) and only `host-fatal` may carry an empty
    signature (:6853), while class validation is non-emptiness ONLY (:6850). A hanging test has no
    failure text, so **no other class can both load and function.** The lane read the code, HELD the
    cut for the word, and proposed the precedented shape (class `host-fatal`, empty signature, the
    classification and the lifting condition in `reason`). **The class FIELD is the mechanism; the
    classification goes in `reason`; and a ruling that writes into a manifest is checked against the
    LOADER that reads it before it is posted.** Same family as (12) above, met by its author.
  - **⚠ Ruling #1 and its boundary (owner, re-read 2026-09-03/04).** A Go=pass / C#=skip whose skip
    reason is OUR OWN missing feature is a **FEATURE GAP**, never a disclosure; and an annotation
    banked where the ORACLE skipped for a HOST reason is host-conditional and says so, since on a
    capable host the gap shows and the row reads red. ⚠ **A Go=pass / C#=HANG is the same ruling**
    (2026-09-05, `TestMutexWaitTimeMetric`) — it stays on the host-deadline gate until the feature
    lands — and the classification is earned by **reading the LIVE stacks (`dotnet-stack` on the
    running host) before classing a hang**: what read as the lock protocol's contended path was two
    threads, one spinning in Go's own status predicate and one parked holding nothing, with no runtime
    lock anywhere on the path — so the root is a PREDICATE (the managed g never leaves `_Grunning`
    because the converted `sync` parks without `gopark`'s accounting), not a lock. A hook in the seam
    every converted wait passes through is a golib API change whatever its line count.
    ⚠ **A coordinator ruling that cites a PRECEDENT
    re-reads the ruling that governs it before it is posted**: the `alloc-profile` want-zero
    disclosures in `bytes`/`bufio` PREDATE ruling #1 (a want-zero alloc assert is satisfiable in
    principle and is never a disclosure) and stand as LEGACY to be re-examined, not as precedent to
    extend — a "measured floor is disclosable" sentence built on them was retracted the same hour. **An
    owner ruling outranks a coordinator's inference from artifacts.** ⚠ And **a count gap is read from
    the row's OWN Go gate before it is attributed to the axis under discussion** (a `cgroup2`
    permission gate is not the cgo axis).
    ⚠ **A COORDINATOR RULING CANNOT MINT AN EXCLUSION CLASS** (2026-09-04). The roster's classes are
    the OWNER's — E1 no eligible tests, E2 broken oracle, E3 the subject IS the replaced
    representation — and the bar refuses "merely hard, unimplemented, or expensive", so "untestable by
    capability" was a phrase doing work the ledger does not license, held by the lane against the
    parser and the format guard before it was ever written. **A row whose tests an unbuilt
    implementation WOULD satisfy stays IN the denominator as unimplemented** — its recon is the
    disposition and the implementation is the queued hard thing; widening E3 for it would be the
    precedent every later frontier row cites.
    ⚠ **E4 — "the comparison is SOUND and validates nothing" — is a fourth limb of the exclusion bar,
    minted by owner ruling 2026-09-07.** E1/E2/E3 all sit on the *provably meaningless* limb: no test to
    run, no trustworthy baseline, or a pass that would be fabrication — the comparison **cannot produce
    information**. An E4 row's comparison works perfectly and tells the truth; what it cannot produce is
    a PASS. **The boundary against E3 is the REASON a pass is unavailable — fabrication versus
    legitimate-implementation-nobody-wrote — and that is a judgment exactly as E2's and E3's are.**
    `matched == 0` is the guardrail and it is NECESSARY, NOT SUFFICIENT (E3's own `internal/unsafeheader`
    is matched-0), but it makes a row LEAVE E4 by ARITHMETIC the moment one verdict matches. E4 members
    are the only exclusions admitted on a MEASURED comparison rather than an argument about why one
    cannot happen, and each carries an explicit revisit condition.
    ⚠ **The bar read from BOTH sides, one evening's five rules (2026-09-06).** **"The blocker now has a
    NAME" is not "the row is impossible"**: a hand-owned function that refuses BY NAME because a
    capability was never built is the clearest possible evidence of UNIMPLEMENTED, which the bar names
    as a reason to REFUSE exclusion — a coordinator read a retired blocker as a class change and was
    refused by the schema's owner, correctly, and withdrew without escalation. **Representational
    impossibility QUALIFIES where unbuilt does not**, demonstrated the same evening from the opposite
    direction: a set of disclosures qualifies because the pointer model is a deliberate identity scheme,
    so the numeric comparison the assert performs has nothing to compare — the pair states the class
    boundary better than either case alone. **A precedent transfers only if its PREMISE does**: the race
    row is excluded because Go declares NO ELIGIBLE TESTS outside an instrumented build, so the
    comparison is vacuous by Go's own definition, and a row with two eligible tests that RUN and produce
    verdicts is the NEGATION of that premise, not an instance of it — reaching for "the same kind of
    reason" on a resemblance rather than reading the class is the same move as reading a registration as
    remediation. **A MECHANISM does not transfer by resemblance either**, and that cut both ways in one
    evening: it refused an exclusion the coordinator wanted AND a disclosure a lane would have benefited
    from (the second half of a billed increment stands as WORK until it has its own measurement). And
    **before ruling on a row's class, read the row's own RECON and any design record that exists BECAUSE
    of a prior ruling**: the same question had been ruled the other way one day earlier — "unimplemented,
    not untestable; expensive rather than impossible" — and the design record for the missing capability
    exists precisely BECAUSE the row stayed in the denominator. **A prior ruling stands until a NEW
    measurement reopens it, and a measurement CONFIRMING the prior ruling's premise is not a reopening.**
    ⚠ **Which side of that bar an entry sits on is a MEASURED discriminator, never a wording choice**
    (2026-09-06). **"No managed body exists" reads as unimplemented and may be representational** — the
    discriminator is whether the CONCEPT exists for anything: can the host produce a comparable code
    pointer for ANY function? If it can, the block is one missing body and the entry is WORK that no
    rewording converts into a disclosure; if it cannot for any function, the concept is absent from the
    model and the argument stands on the same footing as the pointer-identity set. Measure that BEFORE
    writing the new reason. **One probe answered three questions because its properties were reported
    SEPARATELY rather than averaged**: plain functions non-zero, stable, distinct and resolvable (so the
    concept EXISTS and a missing body is unimplemented work) while method values fail STABILITY alone (a
    fresh identity per read) — which disposed of two entries in OPPOSITE directions and named the
    smaller, more valuable fix. **Split a probe's population when the answer might differ across it.**
    And the floor under all of it: **a test that only asks for SELF-CONSISTENCY can never be excused
    representationally** — if the assertion compares two of OUR OWN values and requires them equal (as a
    method-value pointer test does, since Go's two method values share one trampoline whatever the
    receiver), then nothing about the foreign system is being asked of us and a host with the missing
    property passes it UNCHANGED. **Ask what the assertion REFERENCES before writing or repairing any
    disclosure**: the foreign system's values (a representational argument is available and must be
    argued) or only ours (no such argument exists, and the entry is a DEFECT wearing a disclosure's
    clothes).
  - **⚠ A BANKED ROW CAN BE A VACUOUS PASS, and a census over the roster is what says how many**
    (2026-09-03). `internal/abi`'s `TestFuncPC` compares `FuncPCABI0(fn)` against a value `_test.s`
    writes in Go — assembly never converts, so the C# side reads `0 == 0` while Go compares two real
    addresses, and the verdicts MATCHED. **A pass whose two arms are equal for OPPOSITE reasons is a
    false green**; under every honest answer the row stops passing, so it was RULED to spend the
    verdict (2 → 1 + 1 disclosed, runtime-capability, permanent by construction). The census over 202
    banked rows found that class has exactly ONE member, and a SECOND vacuous-as-recorded page beside
    it: `internal/cpu`'s four `if HasX && !HasY` implications could not fail while `doinit` never ran
    (all flags false) — remediated by the `[ModuleInitializer]` hand-own, but the BANKED page stayed
    vacuous until reswept. **A hand-own that changes a package's INIT STATE re-sweeps every banked row
    whose asserts read that state.** The class stays small because bodyless partials THROW and the
    corpus converts under purego (17 silent hand-own bodies exist, one read by a banked assert), and
    nine banked passes are tautological by Go's OWN construction (both arms call the `*Generic` twin) —
    honest, since that IS the production path. ⚠ Two neighbours: **vacuous-if-stubbed tests are NAMED
    before any stub lands** (no content assertion around `StartCPUProfile`); and **a banked row that
    dies on a corpus defect is UNREADABLE, so it cannot gate anything** — an increment whose canary it
    was declares final on the remaining canaries and STATES the row's unreadability, rather than
    waiting for a row nobody can run.
    ⚠ **A MATCHED COUNT IS NOT EVIDENCE UNTIL SOMEONE ASKS WHETHER THE CONVERTED SIDE COULD HAVE
    FAILED.** `runtime/pprof`'s 120 matched rows audited 2026-09-07: **13 REAL, 103 WEAK, 4 VACUOUS**,
    with 100 of them resting on an assertion that CANNOT EXECUTE (`pprof_impl.cs:110` opens
    `_ = labels;`, so `counts` stays empty, `max = len(counts)-1 = -1`, and the ordering loop never
    runs) — **a bank quoting 120 is quoting one smoke test 100 times.** The bar is the `TestFuncPC`
    set above and the question it poses is *"is there a state the converted implementation could have
    been in that makes this assertion fire?"*, never *"did the verdicts agree"*; the remedy is the
    tree's own — make the silent zero a LOUD REFUSAL, an honest disclosed fail, never delete the test.
    ⚠ **A DOOR/BILLING census cannot see a dead assertion**, because it bills rows by the door they
    enter: 104 of 120 rows billed to ONE declaration read as concentration in that declaration, and the
    concentration was in **that declaration's dead assertion**, one layer down. Row-level billing and
    assertion-level reachability are two audits and the first is not evidence for the second. ⚠ And
    **a DERIVED membership is quoted only with a DOUBLE CLOSURE behind it** — the 14 declarations
    carrying matched rows summed to 183 across all 44 declarations (the census's own Go-side count) and
    to 120 across the 14 — stating its limits: a single-run census of crash/deadlock canaries owes a
    second UNGATED run before those rows are called stable.
    ⚠ **A pass that NO HOST DEFECT COULD EVER MOVE is not a measurement either** (2026-09-04): a test
    asserting `count(<a Go output literal the host never writes>) == 0` passes vacuously on BOTH
    sides — the anti-laundering clause read from the other direction — so it is EXCLUDED with the
    reason stated, never counted. Three neighbours from one row. A test that fails deterministically
    for a divergence the project CHOSE (the host-identity class) is run and DISCLOSED, not excluded,
    because a chosen divergence belongs where it can be seen. A test whose failure path is a loop that
    never ends (a timeout-dump scrape that doubles and retries forever) is a HAZARD needing a
    capability entry, since admitting it turns the row into a deadline kill with a contiguous
    alphabetical tail. And **a row's SIZE is its VERDICT count (156), not its top-level NAME count
    (59)** — the two are reconciled before a row is sized. ⚠ Beside them: **a design PREMISE about an
    output path is checked against an existing BANKED instance before options are costed** —
    colocation is the pipeline's norm, which one dispatch's premise had backwards, falsified by
    reading a banked row's directory.
  - **⚠ A disclosure pins a failing NAMED row, so a HOST-KILLER cannot be disclosed at all**
    (2026-09-03): a goroutine panic escaping the process, or an access violation, produces no row to
    pin — the arc's gate is making the killers produce rows. Two measured limits on that. "Make the
    host-killer produce a row" was ALREADY implemented and buys nothing for the escaping-panic case:
    the host emits a fail row and a package fail, faithful to Go's own death, and surviving the panic
    would be a false green twice. And **a converted `recover()` does NOT catch a raw
    `NotImplementedException` from an unimplemented-external stub** — only a converted panic — so any
    unimplemented stub reached from an http handler TERMINATES the process where Go answers 500. A
    regex-shaped test assertion is sized against the WHOLE regex (another goroutine's header AND its
    frames), not against the state word.
    ⚠ **TWO BUGS OF THE SAME SHAPE CAN HAVE OPPOSITE SIGNS, and only measurement tells you which you
    have — so ask what a fix does to the row's DISCLOSABILITY, not only to its failure count.**
    `runtime/pprof`'s state leak and `runtime`'s host-lifecycle leak are both "an exception escapes and
    corrupts subsequent state" (2026-09-07). Fixing pprof's ALONE makes its row WORSE — twelve
    disclosable fails become twelve UNdisclosable infrastructure-errors. Fixing runtime's is STRICTLY
    BETTER — an infra-error reverts to a genuine disclosable `fail` and 799 unmeasured rows unblock.
    **Neither sign was predictable from the bug's shape; both were measured before anyone acted, which
    is the only reason the difference is known.**
  - **⚠ After an operational SWEEP, `git status` is dirty and it is (almost always) NOTHING — classify,
    don't chase, and never bank it.** Two *different* phenomena get conflated here; a sweep produces only
    the first. ⚠ **The dirt is NOT confined to `src/core`: the sweep REWRITES each swept package's
    proof page under `docs/validation/current/`** (that rewrite is by design — it is why the
    host-conditional check reads the COMMITTED page from HEAD), so a restore scoped to the corpus
    leaves the pages behind — 56 of them measured on one lane's shift (2026-08-29). Restore both
    roots, or the next diff reads as proof-page drift. ⚠ **A gate's own DIRT PREFLIGHT is what
    catches the half a corpus-scoped restore leaves** (2026-09-06): a solution leg aborted
    `ABORT dirty` after a canary run whose restore had been scoped to `src/core` while the pipeline
    had also rewritten `docs/validation/current/<row>.md` — the class this paragraph documents,
    met by the gate rather than by the reader. The preflight earned its keep by REFUSING to measure
    a tree it could not vouch for, **which is the shape every gate should have: refuse over an
    unclean tree rather than measure it and report a number.**
    1. **CRLF phantoms — most of a healthy sweep's dirt.** The converter preserves the Go source's
       **LF** inside multi-line string literals while emitting CRLF everywhere else, and `core.autocrlf`
       smudges those in-string LFs to CRLF on checkout. A `-tests` run re-emits them as LF, so every
       banked file containing a multi-line literal shows **modified with no diff hunks at all** — it
       does not even appear in `--numstat`. Do **not** memorize a file list; the count tracks how many
       banked packages hold multi-line literals and grows with every bank (15 at the 47-package roster,
       16 once strconv's `testdata/testfp.txt` joined, **5 + 10 at the 73-package roster** — see the
       split below). *Positive control:* `git diff --numstat HEAD~1` must be non-empty, or your check
       is broken, not clean.
       ⚠ **The "numstat must be empty" rule is FALSE for `-text` paths (found r40, 2026-08-04).**
       `src/core/compress/testdata/*` is marked `-text`, so git does **not** normalize it and a pure
       CRLF flip shows a **real, non-empty numstat** (`gettysburg.txt` 29/29) that reads exactly like
       content drift. Verbatim `testdata`/`*.s` copies are therefore a SECOND phantom shape: test
       CR-stripped equality against `HEAD` directly rather than trusting `--numstat`.
    2. **`-tests`-CLOSURE production files.** A handful of production `.cs` differ between the two
       emissions (`Δio` alias, `global::go.*` root escape, the using-block REORDER the alias
       causes, and — the FOURTH shape, named 2026-08-17 — the `initᴛᴛtests()` hook a `-tests` run
       adds to a package's `package_init.cs` as +7 REAL lines `-stdlib` omits, which survives a
       numstat check that filters phantoms; same class, restore it — **AMENDED 2026-08-26,
       ratified at the leveling-rebank floor: for a row whose test sources are REBANKED at or
       after the init-order arc, the `initᴛᴛtests()` hook is BANKED, not restored** — a
       re-derived suite does not compile without it, so those packages' `package_init.cs` rests
       on the `-tests` side and it is the `-stdlib` overlay ritual that must classify-and-KEEP
       it; the restore rule stands only for rows still carrying pre-arc sources) because the
       `-tests` closure imports more. **Staging corollary (paid for 2026-08-26): never
       `git add -A`/`git add .` on a tree that has had a sweep or `-tests` run against it — name
       the paths; the hook shape survives numstat filters and lands in the commit silently.**
       Two additions, 2026-08-29/30: the **FIFTH shape** — a `GoPositionMap` funcLit/range
       argument the `-tests` emission adds and `-stdlib` omits (rooted independently by two
       lanes the same day; survives numstat filters; evidenced by banked `cookiejar` carrying
       it) — same class, classify-and-restore per the side the tree rests on. And ONE-WAY
       emission changes are NOT closure shapes: the `-tests` init-forcing hook (+7 lines in
       `package_test_info.cs`, landed 2026-08-30) appears at a row's next test-source
       REGENERATION and stays — 193 reference-model banked test infos are stale-until-rebank by
       design, no standing restore, and the rebank wave that levels them owes the full-roster
       sweep (the throwing-production-init regression shape can only materialize there).
       Whether a sweep SHOWS them depends on which
       side the committed tree currently rests on, so do not treat either state as the invariant:
       when the tree rests on the `-tests` side they are invisible to a sweep and surface only under
       an `-stdlib` reconvert control; when it rests on the `-stdlib` side — **where r40 left it** —
       every sweep flips them and they must be **RESTORED**. Measured at r40: **13 files**, wider than
       the six recorded in [`docs/phase4/DESIGN-named-interface-wrappers.md`](docs/phase4/DESIGN-named-interface-wrappers.md)
       §7 — also `bufio/{bufio,scan}.cs`, `crypto/md5/{md5,md5block}.cs`, `regexp/{regexp,exec,backtrack}.cs`.
       Both emissions are correct for their own closure — only the pipeline pairs them — so this is a
       STANDING restore, not a one-off cleanup, until the two agree on one alias per import.
       ⚠ **AN AMENDMENT WITH A CONDITION IS CHECKED AGAINST THE CONDITION, NOT APPLIED BY NAME.** The
       rule "for a row rebanking test sources, the `initᴛᴛtests()` hook is BANKED not restored" covers
       rows whose test variant relocates into the PRODUCTION class. `os`'s relocates into its own
       `*_internal_test_package` with its own static constructor, **nothing implements the partial**, so
       the compiler erases declaration and call and the hook is INERT — restored, correctly, and the
       difference was one `git grep` (2026-09-07).
    3. **`.cs.auto` review siblings.** Tracked, and refreshed by a `-tests` run but NOT by an
       `-stdlib` overlay (which excludes them to protect the hand-owned `.cs` beside them). Restore
       them in a sweep; re-measure the whole set at each rebank head, one seeded reconvert per
       target, rather than banking a count (CleanupBacklog item 18).
    4. **Deduplicated same-shape anonymous structs — LEVELED, so a reappearance is news.** The
       converter binds a second anonymous `[GoType("dyn")]` struct of identical shape to the FIRST
       declaration's type instead of minting its own (`e61758549`, the reflectlite arc). In a diff
       that reads as the duplicate `[GoType("dyn")]` block vanishing while the slice and element
       types rename onto the original's `ᴛ1`, with a knock-on in `package_test_info.cs`, whose
       witness list sheds the declarations that no longer exist. A suite banked before that commit
       keeps the old shape until its own pipeline rerun; the 2026-08-24 post-merge rebank ran the
       last five (`math/cmplx`, `go/build/constraint`, `regexp`, `strings`, `time`). Unlike classes
       1–3 this one does NOT stand: it is banked, so meeting it again means a NEW unbanked converter
       change — find that commit rather than restoring the file.
    Anything that is none of these — a non-empty `numstat` on a production `.cs` that is not a closure
    re-flip, or any change to a production `.csproj` — is **real drift**: stop and root-cause it before
    landing. (A production-`.csproj` change specifically meant the validation-pack block had been
    stripped; fixed in `ce82093b0` and proved clean across the full r40 sweep.)
    ⚠ **After a `-tests` run a package directory holds THREE populations — tracked corpus files,
    tracked hand-owns, and untracked generated emission — so any glob- or directory-wide operation hits
    the wrong one** (paid twice, 2026-09-02). `rm -f src/core/reflect/*_test.cs` deleted the TRACKED
    `export_impl_test.cs` hand-own (the glob encoded "test files under a converted package are
    generated" — true for 13 of 14), and `git checkout -- src/core/reflect` reverted the lane's own
    guard edit in `value_impl.cs`. Restore by FILENAME, clear emission with `git clean -nd` then `-fd`
    — the primitive that reads the tree's state beats the pattern encoding a belief about it.
    ⚠ **`git checkout -- <path>` restores from the INDEX, so a control-arm restore DESTROYS an
    unstaged change in the same file** while "restore clean: 0" TRUTHFULLY reports a match with HEAD
    (2026-09-04 — the sweep-restore trap above, met from the control-arm direction). **Stage the cut
    FIRST, restore control arms from the index**, and only then does a diff-against-index check mean
    what it says.
- Open converter items: `src/go2cs/ToDo.md` (e.g. `visitMapType` completion, remaining dynamic-struct
  implicit-cast checks, optional recursive dependent-package conversion, comment conversion, cgo/asm targets).

