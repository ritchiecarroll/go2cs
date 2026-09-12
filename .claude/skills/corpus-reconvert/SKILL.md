---
name: corpus-reconvert
description: Measure a converter change's corpus footprint. The seeded two-seeded diff, the hunk rule, marker gates, per-target emission and the reconvert-overlay-build-bucket loop.
---

# Corpus Reconvert

<!-- EXTRACTED VERBATIM from CLAUDE.md at 1800b04f8, lines 3774-4103.
     Phase 1 of docs/PLAN-context-diet.md: RELOCATION ONLY, no compression, no edits.
     The byte-identical original is docs/doctrine/JOURNAL-2026-09-12.md.
     Phase 2 distills this file. Keep every dated derivation, but move it into an HTML
     comment beside the rule it justifies: comments are stripped before this file enters
     context, so provenance kept this way costs ZERO tokens. -->

- **⚠ The bank unit for a converter change's corpus footprint is the two-seeded diff's HUNKS, never
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
  the application's own assertions after any restore.
  ⚠ Two corollaries measured 2026-09-02. **"Byte-identical to the emission" is a property of the FILE, not
  of the CHANGE** — copying the footprint files wholesale out of the NEW seeded root is byte-identical BY
  CONSTRUCTION and still wrong, carrying every arc not yet regen'd into the corpus (numstat read 3/9,
  13/31, 3/15, 5/6, 1/7 against a change that owns six lines); numstat is the cheaper instrument, and the
  strongest-looking provenance check cannot see the difference. And **a footprint hunk is the fresh
  emission's STATEMENT even when the committed statement carries another arc's unbanked drift**: carry the
  one inert, byte-verified foreign line and SAY in the commit which line belongs to which arc — a
  hand-written shape no converter emits is worse than one foreign line named. Read the COMMITTED bytes,
  never the seed, before cutting a footprint against a base.
  ⚠ **The hunk rule binds METADATA files too, and the tell is the diff's line KINDS** (measured
  2026-09-02): re-emitting a `package_info.cs` from a `-tests` run imports closure family #2 wholesale
  — 139 `[assembly: go.GoPositionMap` lines became `global::go.GoPositionMap` while the other two GOOS
  folders kept the `go.` form — where exactly ONE map line was owed; counting the diff's KINDS
  (139 −/139 + of a single attribute) caught at the seat what reading it did not. Three companions.
  After ANY pipeline run, every file in the commit is diffed against the PRE-run tree and its line
  kinds counted — an INTENTIONAL file gets the same check as an unintentional one (one lane paid this
  twice in a session, past its own written lesson, because a `-tests` measurement run hands back the
  whole-file form of the metadata file a surgical hunk had touched). A "differs" is CR-STRIPPED before
  it is named (eight runtime files reported as additional drift at one regen were the in-comment-LF
  phantom under a raw-byte `diff -rq`; every one CR-strips identical). And the BEFORE arm of a
  before/after is taken at the CUT'S OWN BASE, never at an earlier landing on the reasoning that "none
  of the intervening merges touch it". A file that is not gofmt-clean at master leaves foreign
  whitespace lines in the next cut's diff: verify they are pure whitespace and NAME them.
  ⚠ **THE HUNK RULE'S INSTRUMENT (adopted 2026-09-03).** `git merge-file -p <committed>
  <base-emission> <cut-emission>` — a 3-way merge with the BASE seeded emission as merge base —
  COMPUTES a converter change's own lines instead of leaving them to be picked out by hand. It is
  PROVED by a pair: **applied delta == emission delta (CR-normalised)**, and **the residual drift
  against the emission is the IDENTICAL SET before and after** (plus 0 `GoPositionMap` and 0
  import-hook lines in an additive delta). Both are needed — a hand span-replacement that obeyed the
  rule still dropped six comment lines the converter HOISTS out of displaced bodies and re-emits
  standalone: it compiled, the placeholders were present and the path set passed, and the tell was
  arithmetic (93 emission-delta lines against 65 applied). ⚠ `diff(base emission, committed)` coming
  back NON-EMPTY is the HEALTHY control reading, not a fault — the corpus carries standing families —
  and position-map hash changes stay with the deliberate regen.
  ⚠ **`patch` applies some hunks and REJECTS others while exiting non-zero** (2026-09-04), leaving a
  file PARTIALLY patched with a `.rej` beside it: an exit-code check calls the run "failed" while
  `git diff --numstat` shows the file half-changed (+9/−9). **Read the TREE, not the exit code** — and
  prove a hunk application IS exactly the change by the emission's added-line count equalling the
  applied added-line count on EVERY file, with no `.rej`/`.orig` surviving.
  ⚠ **Count a footprint with `git diff --numstat`, and take the third reading from a DIFFERENT
  instrument** (measured 2026-09-03): `grep -cE '^[-+][^-+]'` drops every removed BLANK line (a bare
  `-`), so an emission count and an applied count taken the same way agreed with each other (78 = 78)
  while numstat said 82. Two blind instruments agreeing is not a closed arithmetic.
  ⚠ **Line KINDS are read against what the change DOES, never against a remembered number** (three
  measurements, 2026-09-03). An ADDITIVE change's delta carries **zero** `GoPositionMap` lines, so any
  map line there is foreign drift. A change that **REMOVES** emitted code re-encodes the file's map
  and carries its OWN map lines — their ABSENCE would mean the emission had not shrunk — and it kills
  any lift whose only source was the removed body (`nanotime1_r`), so the class-B "zero map lines in
  the delta" bar is for additions only. Refinement: when a displacement removes a file's **LAST**
  mapped content the converter RETIRES the record rather than re-encoding it, so a removal can own a
  map line's ABSENCE; the prediction states WHICH, per file, and names "a map line removed rather than
  re-encoded" as its own falsifier. A prediction's COUNT can hold while its KIND does not — state
  both, and leave a wrong prediction VISIBLE above the measurement that refuted it (appended, never
  rewritten), because a prediction is only worth having if it cannot be edited after the result.
  ⚠ **A prediction is SCORED in the record whether or not it HELD, and its MECHANISM and its SPECIFICS
  are scored SEPARATELY** (2026-09-05): one prediction's mechanism was right and all THREE of its
  specifics wrong — not those sites, not those tests, and on Windows not an errno but a fault. **A
  record that cites its predictions only when they land is an advertisement.**
  ⚠ **A displacement's map line is invalidated by ANY later change to the same source file, and the
  check runs ONCE at the ASSEMBLED tip** (2026-09-03, the seam cut into four times in one day). A
  train's comment drain shifted positions after a keystone's map was derived, so the source file
  merged byte-identical to the fresh emission while the map carried a value NEITHER converter emits
  (three hashes: drain-only, displacement-only, both). Sharper: **the seat that invalidates a position
  map need not write a map line at all** — a co-seat following the hunk rule CORRECTLY (zero map
  lines) moves the positions the value describes and is structurally invisible to a per-seat union
  check; the more correctly it follows the rule, the more invisible. So the fresh-emission check
  covers `package_info.cs` as well as the source, and runs at the assembled tip by the coordinator,
  never per seat; a knowingly wrong map line is fixed there from the emission as one stated fixup
  line, never pasted, and pre-existing stale siblings stay with the regen.
  ⚠ **And a footprint hunk that would re-encode a position map against a fresh emission SHORTER than
  the committed file is a WRONG map on that file — never applied in a converter train** (2026-09-04):
  an unbanked relocation sits between the two, so the value would describe NEITHER tree. The
  deliberate regen levels the map and the relocation together, with the other arc's delta NAMED in the
  commit; and an L3 package is measured on EVERY target, each into its own seed.
  ⚠ **A converter change's corpus footprint is measured on ALL THREE TARGETS or it is not measured**
  (2026-09-04): an A/B run single-target on the host default posted "over the whole corpus" while its
  linux and darwin lines were never applied — the L3 blind spot this file names, walked into while
  quoting the rule. The tell is the emission COUNT PER TARGET stated in the post (1,656 windows
  against 1,724 and 1,727). ⚠ **And the hunk-application arithmetic is NOT sufficient by itself**:
  line counts, the foreign-line grep and `git apply`'s exit are all SET properties, and a hunk landed
  in the WRONG FUNCTION has the identical set — a blanket `-C1` put a `bool` flag declaration 55 lines
  early, inside a neighbouring function, while its set and its read stayed in the intended one, caught
  only by the three-target build (CS0103 ×2 on every target, MSB 0). The instrument is THREE checks:
  the counts (whole-file), the foreign-line grep (imported arcs), and a POSITIONAL per-function
  balance — every flag set or read declared in that function, every declared flag used, both function
  names printed on a mismatch. Two checker rules from that checker's own first failure: **print TOTALS
  unconditionally**, so "found nothing" is distinguishable from "the predicate never fired" (an `awk`
  version treating a function as starting at `internal|public|private` merged two `[GoRecv] public
  static` functions into one self-satisfying block and read 2 declarations where the file has 3); and
  **a control whose INPUT is wrong is thrown out and rebuilt, never read** (a two-hex-digit `printf`
  trap spelled the wrong glyph).
  ⚠ **THE COPIED-SCRIPT TRAP, TWO COSTUMES IN ONE FOOTPRINT RUN** (2026-09-08): a derived
  two-seeded-diff instrument kept the previous arc's HARDCODED prediction label — so the raw log
  scores the REAL prediction, posted separately, as a miss — and kept the previous arc's POSITIVE
  CONTROL, which read zero for the honest reason that no such shape was in play and therefore proved
  nothing about the new arms. **A derived instrument re-derives its LABEL and its CONTROL for the fix
  it now measures, or the run STATES that it has none**; a non-zero predicted reading is what shows
  the instrument detected the change, and its author SAYS the control was absent rather than claiming
  one it did not have.
  ⚠ **A footprint PREDICTION is derived from the same KIND of run that will measure it — the
  two-seeded `-stdlib` emission — never from a single-package probe** (2026-09-03), whose numstat
  carries the import-init hook closure family the driver keeps: a probe predicted −86 where the
  emission read −77 plus the placeholder. Post the predicted number AS READ, never rounded, and score
  a miss against master line by line rather than explaining it away. Its companion: **a footprint's
  SHAPE can be predicted while its LOCATION cannot** — a defect measured only in the probes built to
  find it landed on one production field, and a footprint smaller than sized and on REAL code is
  better evidence than a large one on guards ("every instance so far is in my own probes" is a
  statement about the census, not about the corpus).
  ⚠ **A FAILED FOOTPRINT PREDICTION GOES OUT ON ITS OWN, WITH BOTH READINGS AND THE MEASUREMENT THAT
  SETTLES THEM NAMED — never buried under a success and never re-scoped to fit** (2026-09-08): an
  alias cut's three-target diff moved ONE corpus file on a flavour the prediction had called empty,
  and the author stated both readings — (a) the rename is correct and master carries a latent compile
  error there, or (b) the fold over-approximates — noted that (a) CONTRADICTS that flavour's census
  compiling clean, refused to narrow the fold "to satisfy a prediction I got wrong", and named the
  discriminator as a compile of that ONE package before and after. ⚠ **RUN THE ARM THAT CAN
  DISCRIMINATE AND NAME IT AS ONE ARM**: the AFTER arm cannot discriminate here — a spurious alias
  rename compiles BY CONSTRUCTION, which is the asymmetry the design rests on — so only the BEFORE arm
  was run, and it read the package clean at master in minutes, a second instrument agreeing with the
  project-condition read while the per-flavour compile item set independently refuted "the census does
  not reach the file". Its near-miss: **a mis-named path made `git show` return NOTHING and `grep -c`
  report a well-formed 0 over empty input** — a missing file reads exactly like a clean census — so
  `git cat-file -e` before believing any count taken from `git show`.
  ⚠ **A BASELINE NUMBER IS DERIVED FROM THE TREE BEING MEASURED, AT RUN TIME — never remembered from
  another tree** (two instances, 2026-09-04). A count prediction that SPANS A TRAIN carries the
  train's own additions: a census predicted `665 − 1 = 664` holding the PREVIOUS train's project count
  fixed while the train being measured seated four new guards, and both legs read `668 = 665 + 4 − 1`
  — so **predict the DIFFERENCE the change makes** (cross-leg: 668/15 against 669/14, exactly one, and
  that one the marked project) and derive totals from the tree at run time, never the reverse. Its
  one-scale-smaller sibling: a length remembered from a DIFFERENT worktree is not a baseline — an
  appended record read as "+372 lines" against a length measured in a pin tree, while the cut sat on a
  landed master the file had already grown under, where `git diff --numstat` against the cut's OWN
  base read 55/0. **A number that surprises you is checked against the instrument before it is
  reported.**
  ⚠ **A PREDICTION IS CORRECTED BEFORE ITS MEASUREMENT OR NOT AT ALL, and one was wrong THREE
  separate ways** (2026-09-08): a PER-FLAVOUR number published as universal (the stamps live in
  per-GOOS metadata files and differ across the three); a MASTER baseline applied to a hopped tree
  that carries a different one, since **the release itself moved the counts independently of the
  change under test**; and the increment added to the wrong baseline. Corrected per flavour with the
  falsifier stated — anything else is a finding, and a source-versus-assembly disagreement is
  REPORTED, never resolved by picking the match. Two mechanics: **the reader counts distinct stamped
  TYPES, never attribute APPLICATIONS** (one flavour read one more application than types, from a
  comment hit — exactly the trap); and **a SEATED record takes no commits**, so a one-line correction
  rides the MERGE MESSAGE when the seat lands first.
  ⚠ **A census's SCOPE must match the EMISSION it predicts** (2026-09-04): a `-stdlib` footprint is
  scored by the PRODUCTION-only census, because `-stdlib` never emits test packages, while the
  with-tests figure answers the DEMAND question — one increment's "~130 predicted sites" was the
  with-tests number against a measured production-only **12**, exactly the six packages the production
  census named. Two censuses, two questions: a prediction quoted from the wrong one is corrected in
  the commit, and the ruling made from the right one stands.
  ⚠ **A CENSUS PREDICATE AND AN EMISSION PREDICATE ANSWER DIFFERENT QUESTIONS, and a gate carried
  from one into the other unexamined refuses what the emission already makes correct** (the
  defer-to-`finally` lowering arc, 2026-09-04). "Direct child of the body" was a cheap conservative
  proxy for a POPULATION census and was carried into the cut unchanged, while the per-site reached
  flag the cut ITSELF emits makes a conditional defer correct by construction — so the gate refused a
  real site and the arc's acceptance row did not move under it, found by READING Go's source after the
  acceptance had been re-sized from a segment table. **Every gate carried from a census into an
  emission is re-derived from the emission's own mechanism**; a correct cut whose acceptance row does
  not move lands with that null STATED in its commit; and the widenings that WOULD move the row are a
  sized successor with their own census and ordering proof, never a re-cut of a measured increment.
  ⚠ Worse than conservative: **a gate that is CORRECT as a census heuristic can be UNSOUND as an
  emission rule, and the two read identically in code.** A prefix gate scanning preceding statements
  with `ast.Inspect` walks INTO an `if`, a `switch`, a loop and a func literal, so a dereference on a
  branch that may never run witnessed one that must have — over-counting a population by four in a
  census, and silently moving a nil-receiver panic past an already-run body in an emission (four of
  170 qualifying sites leaned on exactly that). Neither CNR nor a build can see it: both arms compile
  and emit fine. What found it was re-reading the predicate against the sentence that justifies it and
  noticing the code does not establish it. **Where a gate's justification is "this provably ran
  before", the gate COMPUTES that relation and never approximates it with syntax** — three gates in
  one arc were each a syntactic proxy narrower than the real thing (a witness required at body level;
  an `if`'s INIT and COND, which run unconditionally once the statement is reached, skipped with its
  body — a third of one population, qualify 182 → 226, hidden behind it; and an own-block-only scan
  blind to an outer statement preceding the enclosing construct), each refusing sites the widening
  existed to admit, the last caught by a NAMED falsifier rather than by a compile error. So: walk OUT
  through the ancestor chain collecting each level's earlier siblings plus every enclosing
  `if`/`switch` init and condition; scan an `IfStmt`'s Init and Cond, a `SwitchStmt`'s Init and Tag, a
  `TypeSwitchStmt`'s Init; and refuse bodies, `select`, loops and labels — an `ast` walk that must
  prove "always executed" skips every branching node and refuses to descend into a `FuncLit`.
  ⚠ **The census is the INSTRUMENT, not the CONTRACT**: when the census predicate and the converter
  predicate — two independent implementations of one rule — disagree, read the DIRECTION before
  relaxing anything. Two predicted paths that did not move rooted to the converter requiring a
  function's OTHER defers to be LOWERABLE where the census required only TOP-LEVEL: the stricter side
  cost population, not correctness, so the census is recorded as over-counting by exactly that class
  and the converter is left as it is. A falsifier phrased as "any path OUTSIDE the prediction" catches
  the dangerous direction; absences INSIDE it are sized, rooted and stated. And **a census that must
  agree with a converter gate ports the converter's predicate VERBATIM** (the `provablyBefore` pair
  copied into the census), so the two run the same code and the only difference left is SCOPE — a
  different and stated question: one arc's "166 against 129 reached" was predicate drift, not a class
  to live with.
  ⚠ Two prediction rules from the same arc. **Predict the quantity the INSTRUMENT measures**: an A/B
  whose PRE arm is the PREVIOUS increment's tip measures only the new increment's delta, so a file
  whose qualifying sites the previous increment already lowered shows NO change — a 54-file census
  population was predicted against an arm that read 21, all inside the set, the falsifier silent and
  the number wrong for a reason the data already held. And **a population prediction names the SHAPE
  it counts** — "deferred method calls" (everywhere, mostly on locals) is not "a method on the
  enclosing method's own receiver" (332), and a wide band around the wrong population still misses by
  a factor. **A source-level census cannot see EMISSION properties**: 10 of 65 lowered calls kept
  their box because the deferred method is a PROMOTED method on an EMBEDDED field, reached through a
  field-reference box the lowering moves into the `finally` rather than removes (correct, LIFO holds,
  the delegate still saved) — so the receiver-method bucket has two byte profiles it does not split,
  stated rather than chased.
  ⚠ And the lowering's own three-part obligation, kept as doctrine because it documents a HAZARD
  rather than a heuristic: lowering a Go `defer` into a C# `finally` moves THREE things from
  registration time to exit time, each a silent wrong answer — the RECEIVER's evaluation, the
  ARGUMENTS' evaluation, and the REGISTRATION itself. So a receiver, or any PREFIX of its path,
  reassigned afterwards disqualifies the site, as does an argument changed afterwards; and because the
  call is registered only if control REACHES the defer statement, the lowered call is guarded by a
  local flag set at the defer's own source position unless the defer is the body's first statement. **A
  gate that reads ZERO on today's corpus is kept anyway when it documents a hazard rather than a
  heuristic**, and a predicate built from a census is re-read AT THE EMISSION SITE before the cut —
  which is where this one was found.
  ⚠ **A ZERO corpus footprint is a FALSIFIABLE CLAIM, and is banked only when it is EXPLAINED**
  (2026-09-04). The claim is "these shapes cannot occur in a compiling corpus", settled by the
  two-seeded diff with any non-empty diff posted as HUNKS before anything is applied — and what turns
  a zero three-target diff (18,720 files a side, only the run's own timestamped reports differing)
  from luck into a statement is a POSITIVE-CONTROLLED census of the pinned GOROOT, production AND
  `_test.go`, finding zero occurrences of the trigger shapes: the fix is for end-user Go reached
  through `-recurse`. **A reviewer wanting a non-trivial footprint should be suspicious of the FIX,
  not reassured by the zero.**
  ⚠ **Amended 2026-09-04: a predicted-ZERO production footprint turns the two-seeded `-stdlib` diff
  into the NEGATIVE ARM, not into a gate to drop.** "Zero movement" is a prediction that can FAIL, so
  the diff runs with its positive control beside the `-tests` emission census that measures the change
  — "empty by construction" applies only where nothing PREDICTED it empty.
  ⚠ **And a std census answers "how many sites in std" — it is BLIND to the BEHAVIORAL corpus, a
  SECOND population with its own shapes** (2026-09-05): the chan-of-array `make` the std census found
  ZERO of in production existed once in the behavioral tree, and CNR is what censused it (CHANGED =
  one golden). **A footprint prediction names BOTH populations, and CNR's CHANGED set IS the
  behavioral census.**
  ⚠ **A DISPLACED BODY TAKES ITS CONVERSION SITES WITH IT, and no guard sees it** (measured
  2026-09-03): registering a function in `manualConversionFuncs` drops every `GoImplement` record the
  converted body minted (two endian-order records vanished from `package_info.cs`, in a seeded run
  too, since a package conversion does not merge existing records). The CLOSURE COMPILE is what
  caught it — the assignment compiles only through the operator the record generates — so the
  companion DECLARES the records it performs, spelled as the file spells its own, and the closure
  build is the witness. A hand-own displacing an `init` declares `[GoInit]` itself, since a displaced
  init emits only the placeholder.
  ⚠ **THE RE-DERIVE DISCRIMINATOR HAS THREE OUTCOMES, NOT TWO** (2026-09-07): RESIDUE (the declaration
  survives, the stamp is absent → drop), **MISSING GENERATED** (the emission declares a construct the
  frozen file NEVER had → RESTORE — 39 of these were `[GoInit]` import-init hooks absent wholesale from
  three whole-file hand-owns, the forced-init class over the `.cs` files themselves, since a hand-own
  carrying none of its hooks is not forcing its imports' inits), and BY DESIGN (the hand rewrite
  deleted it → nothing owed). **A two-way split files the middle class as "gone by design" silently.**
  Its instrument lesson: an anchored pattern REQUIRING a trailing character cannot match a declaration
  that ENDS its line (7/1, not 6/2), and a scripted check disagreeing with a hand check is where the
  finding is. ⚠ **And that middle class then closed EMPTY, on a control its own population could not
  contain: a census whose population is HAND-OWNS ONLY cannot see a corpus-wide CONVERTER change, and
  no re-run of it ever will** (2026-09-08). The 39 members were the 2026-09-01 import-init-hook
  RELOCATION read as hand-own residue, because every member of the population was a hand-own and the
  control that breaks the reading — an ORDINARY production file with no hand-own near it
  (`sync/cond.cs`: committed 1 hook, fresh 0) — was outside the population BY CONSTRUCTION; it was
  reproduced on a linux target before it was accepted. **A freshness screen is keyed to the CONVERTER
  CHANGE the question depends on, never to a generic rebank date; and a residue census carries at least
  one ordinary-file control per claimed class.** The one apparent survivor (godebug's real `init()`)
  has a Go principal and is BY DESIGN; RESIDUE 12 / BY DESIGN 11 stand.
  ⚠ **A CONVERTER STAMP WITH TWO EMISSION PATHS CAN DISAGREE WITH ITSELF, AND A FROZEN HAND-OWN HIDES
  IT UNTIL THE STAMP IS RESTORED** (2026-09-08): the value-clone stamp's FIELD-LIST path
  collision-mangles a field name that the DECLARATION path spells plainly, in BOTH releases'
  emissions — latent for as long as the file was a frozen hand-own carrying no stamp, and surfacing as
  a generated-shell compile error the moment a residue drop restored the stamp VERBATIM with every
  per-line assertion holding. **The emission was internally inconsistent; the cut that REACHED it is
  not the cut that BUILT it.** A stamp census over the emission compares each stamped name against its
  DECLARED member before a re-derive restores it.
  ⚠ **Two census-scope rules for a footprint over converted C#.** Anchor on the alias family for
  **package** qualifiers, not only type names — `Ꮡ((Δ)?pkg.Var)` — because the converter mints a
  Δ-prefixed alias for an imported PACKAGE as readily as for a type: a name-keyed census of one fix's
  sites counted 5 where the `(Δ)?`-anchored one counted 10 in 5 files (2026-09-03). And **a corpus
  compare includes `*.cs.auto`** whenever the change can reach a hand-owned file's declarations: a
  `*.cs`-only walk is blind to the OWNER-arm emission of a whole-file `[module: GoManualConversion]`
  hand-own, whose converter output lands in the `.cs.auto` review sibling while the committed `.cs`
  stays hand-written by design. The sibling's drift is the standing `.cs.auto` class — named, not
  overlaid — but the compare must still SEE it to confirm the owner arm reached no other hand-own.
  ⚠ **A WHOLE-FILE HAND-OWN'S DELTA AGAINST ITS TRACKED `.cs.auto` HOLDS TWO POPULATIONS WITH OPPOSITE
  OBLIGATIONS — HAND EDITS (re-apply) and FREEZE RESIDUE (the converter improved after the freeze:
  DROP) — and a 3-way re-derive cannot tell them apart**, because both are BASE→OURS changes: it
  re-applies the residue unexamined and the gap reproduces at every future re-derive (`runtime2.cs`:
  16 hunks, 4 `[GoValueClone]` stamps absent at master AND at the re-derive). The discriminator
  (2026-09-07): a shape present ONLY in hand-owned files is a HAND EDIT; a shape in the `.cs.auto` AND
  in N corpus files but absent from this `.cs` is a residue CANDIDATE; then the declaration confound
  check — **a declaration GONE means by design, not residue** (8 absences split 6/2). Every whole-file
  re-derive at a hop STATES its hand/residue split, and candidates that skipped the declaration check
  are never quoted as residue. Companion: **a hand-own HEADER that under-documents its own delta
  (2 listed, 4 found) is caught only when something merges against it** — list every hunk.
  ⚠ **And the tracked `.cs.auto` is a valid 3-way BASE only where it POST-DATES the hand file's last
  commit** — one `git log` per file as the screen, and 3 of 30 fail it (`sync/mutex.cs`,
  `syscall/linux/exec_unix.cs`, `time/tick.cs`). A 3-way rooted on an OLDER sibling measures a delta
  against a tree nobody has, so **regenerate-and-compare is the form of record** and the date screen
  only says who owes it. Companion: restoring a converter-decision attribute the hand file lacks
  (`[GoValueClone]`) is a BEHAVIOUR CHANGE consumers read, and rides as its OWN cut, never inside a
  re-derive seat.
  ⚠ **A COMMIT-DATE SCREEN FOR A STALE 3-WAY BASE IS STRUCTURALLY WRONG** (2026-09-08; 2 of 10
  caught, 8 missed, 1 false positive): a date answers "was the sibling written before the hand file",
  the question is "is the sibling's content THE EMISSION", and only a TARGET-MATCHED regen answers it
  — the committed siblings are ONE flavour's emissions, and a regen for another flavour read a file's
  stamp count differently. **And CONCORDANT staleness is HARMLESS**: freeze residue needs the base to
  HAVE the block, ours to LACK it and theirs to HAVE it, so where THEIRS also lacks it — the relocated
  import hooks — both sides delete and a 3-way rooted on the stale base is still RIGHT; "differs from
  today's emission" OVERSTATES "bad base" (of 28 measured, a minority is genuinely stale). ⚠ **A THIRD
  invalid-base class surfaced the same hour: NOT-AN-EMISSION** — one tracked review sibling was
  HAND-AUTHORED, created whole in a two-file hand cut and carrying no generated-file header at any
  commit, so a 3-way rooted on it resolves hunks against content the converter NEVER WROTE; **a date
  screen cannot see it, only a content compare against a target-matched emission can**, and nothing
  enforced the sibling's own do-not-edit banner. The refreshed siblings land as a TRACKED seat, so
  base and record are one artifact.
  ⚠ **A converter fix that ADDS a branch is proven SURGICAL by measuring that the OTHER branch
  reproduces existing emission byte-for-byte AT THE BASE** (2026-09-03) — the surgical claim is a
  measurement, not an argument.
  ⚠ **Scope DELETES by TRACKEDNESS as well as by lane prefix — `git ls-files` decides** (paid three
  times, 2026-09-03). A cleanup glob `rm -f …/go2cs_test_*.json` deleted the TRACKED disclosures
  manifest `go2cs_test_disclosures.json` (pipeline artifacts and a committed manifest share the
  prefix), and `rm -f *_test.cs` deleted TRACKED test sources twice in one night because the reflex
  was carried from an UNBANKED row (test emission untracked, glob correct) to a BANKED one (test
  emission committed) — the two rows differ in exactly the property that matters. The durable form is
  a STEP, not care: **`git status --porcelain | grep '^ D'` after any cleanup, asserting
  `deleted-tracked: 0`, before the next command**; clear emission with `git clean -nd` then `-fd`.
  Companion: a bank-check's own `-tests` pass OVERWRITES a merged non-marked file — restore from the
  preserved artifact and re-run every footprint invariant after any pipeline pass.
