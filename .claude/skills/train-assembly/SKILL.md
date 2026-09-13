---
name: train-assembly
description: Assemble, rehearse, gate or land a train. Seats and seating mechanics, union gates, rehearsal, the mid-battery source freeze, landing scripts and attribution when a leg reds.
---

# Train Assembly

<!-- EXTRACTED VERBATIM from CLAUDE.md at 1800b04f8, lines 1444-1476, 6116-6471.
     Phase 1 of docs/PLAN-context-diet.md: RELOCATION ONLY, no compression, no edits.
     The byte-identical original is docs/doctrine/JOURNAL-2026-09-12.md.
     Phase 2 distills this file. Keep every dated derivation, but move it into an HTML
     comment beside the rule it justifies: comments are stripped before this file enters
     context, so provenance kept this way costs ZERO tokens.

     PHASE 2 APPLIED 2026-09-12. Visible half = the procedure in assembly order, each rule led by its
     TELL. Every dated narrative, SHA, measured number, package name and named incident from the
     Phase 1 text is preserved in the comment block beside the rule it justifies. Nothing was deleted;
     the byte-identical original remains docs/doctrine/JOURNAL-2026-09-12.md.

     BATCH19 MERGED 2026-09-12: the nine hunks the extraction map routes here from
     origin/claude/coord-doctrine-batch19 (24bfc8304 / c5e17217b / e9e56b657, items 1154-1321, anchors
     old 6118/6162/6172/6182/6188/6226/6357/6388/6459) were folded in — most as further evidence inside
     the existing rule's comment, a handful as new rules at the step where they apply. That branch was
     never merged; these comments are now its record in this file. -->

## 1. Freeze, while any battery runs
- Converter/gen/golib source AND harness/coordinator scripts are frozen in **the worktree the battery runs in, on any branch checked out there** — the runners rebuild `go2cs.exe` from that tree's disk and golib/gen compile into every project built, so a mid-run edit makes the remaining legs measure a MIX of committed and uncommitted state. Queue your cut until the summary prints. The freeze does NOT cross machines.
- **Tell — `go2cs.exe` rebuilds unexpectedly mid-run:** an UNPINNED SHELL, not an edit. `IsConverterStale` compares the exe's embedded release against the AMBIENT toolchain. Pin the shell (route #4's stamp).
- **Tell — `syntax error near unexpected token '('` at a leg unrelated to any edit:** bash reads a script BY BYTE OFFSET. Launch every long run from a per-run copy (`coord-<train>-assemble-runN.sh`), edit only the original, relaunch behind a skip flag; `bash -n` passes either way and proves nothing. A DERIVE for the NEXT train REFUSES any path belonging to the RUNNING train.

<!-- Ruled 2026-08-30 (freeze on ANY branch; route #1's stale-binary trap inverted — a too-FRESH binary).
     Lanes queue their cuts until the battery's summary prints; the coordinator announces battery
     start/close on the mailbox for exactly this reason.
     Scope stated 2026-09-02 after a lane held a cut it never needed to: the freeze binds the worktree the
     battery runs in, on any branch checked out THERE — the runners rebuild go2cs.exe from that tree's disk
     and golib/gen compile into the projects that battery builds, so a lane editing its own clone on its own
     machine cannot reach a battery leg elsewhere on the fleet.
     2026-09-04 unpinned shell: "An UNPINNED SHELL violates the freeze without failing anything" — an
     unpinned shell makes EVERY harness invocation rebuild go2cs.exe; in a worktree carrying a battery that
     is the freeze broken by a shell rather than by an edit, and the only reason one such run was benign is
     go.mod's own `go 1.23.12` line steering the toolchain switch, not the pin. Read a surprise converter
     rebuild as this before anything else.
     2026-09-04, the coordinator against itself: a train assembly died thirty minutes after a seat slot was
     inserted into that same file for the NEXT train, on `syntax error near unexpected token '('` at a leg
     with nothing to do with the edit. bash -n passed before AND after; the running process never re-parses.
     The relaunch resumes on the intact head behind a skip flag, since the seats, the guards and the
     completed leg's own log holding its verdict are not in doubt.
     2026-09-05, "the door that opens by ITSELF": one derive's copied writer pairs carried the previous
     train's names as BOTH source and destination and rewrote the running assembly script IN PLACE while its
     chain was executing it — saved only by the chain being blocked inside a leg's command and by the derive
     being deterministic, so a re-derive restored the launched bytes. The freeze binds coordinator scripts
     exactly as it binds converter source. -->

## 2. Seating
- **A SEATED branch takes NO commits at all — not even a doc-only one.** Follow-on work goes on its OWN branch off the same base. A correction to a seated record rides in the CUT's OWN commit as a DATED amendment leaving the original sentence visible; the coordinator corrects the SEAT MESSAGE too. **A seated record's ERRATUM is POSTED, not amended** — until the train lands the POST *is* the record's erratum, and the correction lands afterwards as ONE dated block.
- **A seating instruction gets its OWN LINE at the top of a post**, and **the slot takes the REMOTE TIP** — the assembly log prints the merged PARENT SHA, which is what the lane checks; a stale SHA seats a PARENT and drops a row, consistent in the table and merely untrue.
- **Assert `git merge-base --is-ancestor <seat> HEAD` before a single gate runs** — "the fix is in master" says nothing about a branch that forked before it.
- **Derive a seat list from MERGE PARENTS, never merge messages:** `git log --merges --format='%P' <base>..<tip> | awk '{print $2}'`. A message regex over-counts; merges legitimately MENTION branches they did not merge. A second parent no longer resolving to a branch is a seat amended AFTER it landed.
- **A repair is BASED on the commit that INTRODUCED the defect**, never on a local assembly head — a base nobody can fetch is a base nobody can check. **A dispatch naming a MECHANISM nobody has measured is a "measure why first" order, never a cut.**

<!-- Seating mechanics, five rules measured 2026-09-03. No-commits: the seat's arithmetic is a claim about a
     SHA, "harmless" is the seat owner's judgment, and a train assembling in the two-minute window would have
     merged something unverified (the corollary of announce-then-push). A seating instruction embedded in a
     post about something else was read past for forty minutes. is-ancestor: a stacked tree lacking a train's
     seam fix reported the ORIGINAL bug as a fresh "twin" of the fix, retracted within ten minutes once the
     seat tree was measured. A stale SHA quoted in a train listing would have seated a PARENT and dropped the
     row that made the tree honest, undetectable by the format guard because the table would be consistent
     and merely untrue.
     2026-09-06, seat lists: message-scraping reported 21 seats for a train with 20 merge commits, the extra
     being a branch named inside another seat's message. The over-match was harmless ONLY because the gate
     consuming the list asks an ancestry question that is right regardless of why a name is on it — a list
     built for a checker that trusted it would have been wrong. Resolving each second parent back to a branch
     still pointing at that sha is a free bonus check.
     2026-09-06, what a seated branch may carry: a prediction is never edited after its result — including a
     prediction that turns out to be an inverted statement of fact — and the seat message is the train's own
     record of what it merged, where a future reader looks first. A repair based on the introducing commit
     merges cleanly into any assembly containing that seat and reviews as a repair of a named thing; a train
     assembles on the coordinator's machine and lands by pushing master.
     2026-09-08 (C1), erratum posted not amended — batch19, anchor old 6182: THREE defects were found in an
     already-seated record. (i) A non-greedy extractor took the modifier `static` as the function NAME on
     TUPLE-returning declarations, collapsing five per flavour into one phantom function; fixed by enumerating
     every `name(` and discarding keywords, controlled on seven shapes in both directions. (ii) Two of the
     109 "sites" were the DECLARATIONS themselves, so the call-site population is 107 and 109 counts token
     occurrences. (iii) A falsifier used the fatal path's own site count as its "reachable" baseline and so
     COULD NOT FIRE; measured reach is 47/46/50 sites per flavour, with MUST-BE-ZERO and MUST-BE-NONZERO
     controls run before any bucket prints. The headline population and the hop reading stood, so the seat was
     not disturbed: the post carried the erratum and one dated block landed after the train. -->

## 3. Dispatching
- **Re-derive a dispatch's PREMISE against the roster AT DISPATCH TIME**, never from the census's read date; a lane's first act on receiving one is to MEASURE the premise and post the table. **A premise about the LANE'S OWN HOLDINGS — "the baseline you hold at `<sha>`" — is a claim like any other:** take the NAMED baseline yourself (one extra build) rather than comparing against whichever one you happen to hold, or the previous train's movement is charged to the files under test.
- **A dispatch built from a MEMORY FILE is a claim about the code, and a claim about the code is read at the TREE** — a stale record quoted as an INSTRUCTION is worse than one quoted as a description. Give a resolved record a DO-NOT-DISPATCH banner. **A coordinator TRAP handed to a lane is doctrine in miniature:** read at the tree before it is written, retired loudly when a lane refutes it. **A WAKE PROMPT asserting a REFUTED finding is worse than a stale one** — it has a future self act on what the fleet has already measured false — and is corrected the moment the transport allows.
- **A generalisation from one measured row to its sibling is a HYPOTHESIS, not a routing** — measure the sibling first. **A measurement is not automatically the earliest evidence:** ask the row's OWNER before dispatching a diagnosis; a coordinator holding a fresh number is likeliest to mistake it for the first one. **Tell — a conclusion carried to the very member your OWN enumeration named as the exception** (the scalar cases' answer applied to the struct case, where there is no operator to invoke at all): the exception was in the list you wrote, and one addendum later a mechanism is built on the carried half.
- **Gate on the SEAM (a shared file, a shared registry), never on the SHA** — an "after the landing" gate idles a lane for the whole battery and is usually unnecessary when the seat lands UNCHANGED. **A cloud lane cannot read the coordinator's scripts directory:** paste a queue file to the mailbox VERBATIM.
- **A queue file carries its LANDED state or the item is dispatched TWICE** — it opens with a `# STATUS` header stamped AT THE LANDING, never from memory; "dispatched, reviewed before landing" describes a MOMENT and gets its LANDED SHA the day the train lands. Lane's half: **when your ORDERED items have all landed, START the next one** — the mailbox is read BETWEEN steps, not INSTEAD of them — but **"my lane is idle" is not "this gap is unclaimed": read `anchor..tip` for the ROUTING before spending a run on one.** Two hosts agreeing on a gate its author cannot run is CORROBORATION, recorded as such, not a second finding.
- **Before re-dispatching run `git ls-tree -r origin/master -- <deliverable path>`, `git log --oneline origin/master -- <deliverable path>` AND `git merge-base --is-ancestor <local tip> origin/master`. Tell — `commits over master: 0` PLUS `is-ancestor-of-master: yes` means LANDED, not "not started". Tell — `git worktree add -b` REFUSING an existing branch name.** A preflight scoped to where the FIX would live is BLIND to a seat that banked a NEGATIVE result, and `ls-remote --heads` is EMPTY for a merged branch because the ref is pruned at merge — the surviving LOCAL branch is the tell.
- **A RULING derives a deliverable's ABSENCE from the TREE, never from a post** — one `git show origin/master:<path>` — and **a RULING that names WORK greps the IMPLEMENTING half first** (`partial … <symbol>(…) {` against the declaration's `;`, plus the registry entry, raw mentions labelled as neither). Reading the DECLARATION file or a filename keyed on the SYMBOL cannot find a body that lives in a file named for its CONCERN: **an instrument's evidence base is a property of its KEY.** A lane's "I have minted nothing" about its own pushed surface is a claim like any other, and "I wrote that rule an hour ago" is not the check. **Read a citation for a corpus row AT THE CORPUS PIN** — a line number past EOF is the cheap tell.

<!-- Three dispatch rules, 2026-09-04: a remaining-rows record read on the 2nd named a row as an unowned stub,
     the row BANKED on the 3rd, and the dispatch went out on the 4th with the stale clause in every line — the
     lane's first act was to MEASURE the premise and post the table, which is the right first act. The
     landing-gate idle (a lane silent past the watch's threshold with a landing-gated item is the
     coordinator's idle, not the lane's; a branch cut on the seat's tip merges onto landed master with no seam
     the seat does not already own) and the cloud lane's unreadable scripts directory are the other two.
     2026-09-07, dispatch from a memory file: a coordinator note recorded a package's x86 feature detection as
     unowned; true at its dispatch, and the work LANDED the same day. Quoted five days later it sent a lane to
     build what already existed; the lane's "STOP BEFORE I BUILD IT" cost minutes instead of a day. Companion:
     a dispatch stated "the host is hand-owned, so it is yours to edit directly — there is no converter change
     here", and the bill was ONE hand-own edit plus TWO converter-side changes, found BY BUILDING IT. The
     lane's framing is the rule — "I would rather correct it here than have the next lane inherit it".
     Two more, 2026-09-05, both from a queue file that did not carry its own state: an item landed on train 23
     was re-dispatched from a file that never got its mark, and the tell that caught it was `git worktree
     add -b` REFUSING an existing branch name. The durable preflight pair is `git ls-tree -r origin/master --
     <the deliverable's path>` (the guard project, the doc, the manifest) plus `git merge-base --is-ancestor
     <local branch> origin/master`, with the surviving LOCAL branch as the tell that fires when both read new.
     2026-09-07: the coordinator read `commits over master: 0` on a placement branch as an empty placement and
     RE-DISPATCHED it, while the very same check printed `is-ancestor-of-master: yes` — the tell read past
     inside one instrument's own output.
     2026-09-06, generalisation: a confident dispatch said two remaining rows sat behind one unimplemented
     stub; the sibling MEASURED an hour later RUNS — 122 of 160 matching, 38 differing on an entirely
     different axis — wrong in the direction that costs a lane time, by naming a large row as cheap. The
     measurement took four minutes.
     2026-09-07, earliest evidence: a coordinator measured a crash on another lane's row, read it as the first
     sighting, and dispatched a second lane to root it; the owner had already named the failing string
     character for character before the measurement existed, with the fix written, guarded, gated and pushed,
     and called STOP before the duplicate work landed. The ordering matters for the record as much as for the
     effort: the fix was not a guess at the measurement; the measurement was a confirmation of the diagnosis.
     2026-09-05: a lane named three quiet hours honestly — its ordered items had all landed and it waited.
     2026-09-08 (coordinator/C1), rulings — batch19, anchor old 6162, TWICE IN ONE HOUR. (a) A ruling told a
     lane to mint a host-fatal manifest entry on the strength of that lane's "I have minted nothing"; the entry
     had been at master SIXTEEN HOURS. One `git show origin/master:<path>` is the whole check — the
     dispatch-preflight rule above, applied to rulings. (b) The next ruling scheduled managed `getg`, which had
     LANDED WITH TRAIN 27, on a lane's "bodyless on all three flavours" — a claim produced by two instruments
     structurally INCAPABLE of finding a partial's implementing half: reading the DECLARATION file (`stubs.cs`)
     and a filename pattern keyed on the SYMBOL, while the body lives in `stubs_impl.cs:102`, a file named for
     its CONCERN. The narrow check is the lane's own is-implemented script. Companion from the same post:
     citations for a corpus row are read AT THE CORPUS PIN — line numbers taken at 1.24.13 (the converter pin,
     because the ambient `go` switches UP inside `src/go2cs` under `GOTOOLCHAIN=auto`) cited `exec.go:225-231`,
     PAST EOF at 1.23.12's 222 lines.
     2026-09-08 (R), idle lane — batch19, anchor old 6172: a lane ran a peer guard's Compile+Output (196.8 s)
     that another lane had been routed twenty minutes earlier; `anchor..tip` carried the routing.
     2026-09-08, the named baseline — same anchor: a dispatch said "you hold at <sha> (878/0/0)"; the lane held
     baselines at two OTHER commits only, 31 behind the landed master, so comparing the chain against them
     would have charged the previous train's movement to the four files under test — the
     empty-baseline-reads-as-total-disagreement class in a new costume. The lane took the NAMED baseline first
     (one extra solution build) so its comparison leg read ONE axis, and verified both trees from the remote
     before anything ran.
     2026-09-08, generalising over your own enumeration — batch19, anchor old 6188: a design section listed the
     callback parameter types and named the struct case EXPLICITLY, then carried the SCALAR conclusion ("the
     binder will not invoke the operator") to the struct case, where there IS no operator to invoke; one
     addendum later a mechanism was built on the carried half. The ruled reinterpret is UNIFORM (one rule for
     the struct, the two integer widths and a named handle) and FAITHFUL (byte-for-byte what Go's own
     descriptor copy does), and the reference implementation's mechanism was in the file already read to build
     the enumeration. Companion from the same entry: a wake prompt asserting a REFUTED finding is worse than a
     stale one. -->

## 4. Rebase and re-landing
- **Tell — "conflicts" against a new master that are really ONE DUPLICATE COMMIT** (the same patch as the landed seat, differing only in blob ids and one hunk offset): **drop it with `rebase --onto`, never resolve by editing.** Verify by ARITHMETIC (commit count minus the duplicate, the doc byte-equal, the applied delta identical over its files, a temp-index 3-way reporting zero unmerged paths first) and land as a NEW branch so the posted SHA stays untouched.
- **Re-landing work master MERGED then content-REVERTED: the admissible form is revert-the-revert then cherry-pick the fix** — a direct merge fights the revert everywhere it touched, and a rebase may silently drop the already-upstream body. **State an acceptance as a relation between two DELTAS, never two trees, whenever the base has moved**: same file set and per-file numstat as `base^..tip`, the stable patch-id as the strong form, the displaced construct's occurrence count at the branch's number, zero markers.
- **Naming a branch as a convenient home for an edit is a REF CLAIM: check it is CURRENT and not a SUBSET of master BEFORE recommending it** — a stale branch offered as a shortcut is a silent-subtraction delivery mechanism.
- **A REBASE across a file the predecessor also touches is where silent subtraction lives — verify BOTH changes are in the rebased tree, BY NAME.** One side drops with no conflict and no marker.
- **A re-base's acceptance is NEVER "0 conflicts", and it is written BEFORE acting on the ruling** — the named conflicts with the side each resolves to, PLUS a positive assertion that FAILS on silent subtraction (the displaced construct present TWICE afterwards, by name). Where two cuts are each right for their OWN release the union is NEITHER: take theirs' LOCATION with ours' BODIES re-pointed. Taking the obviously-correct side WHOLE restores the pre-seat state with no marker and no gate that builds the newer corpus.

<!-- Two shapes with explicit acceptances, 2026-09-03. Two of three "conflicts" against a new master were ONE
     DUPLICATE COMMIT. The re-landing shape: a direct merge of the original branch fought the revert
     everywhere it touched — 58 conflicts, a modify/delete on a test the revert removed. The acceptance "the
     re-landed tree equals the branch tip over src/..." CANNOT be empty once master has moved, so it became
     PATCH-EQUIVALENCE.
     2026-09-07, branch-as-shortcut: a coordinator told a lane "you have that branch open on that very file,
     so the edit is one line in a tree you already hold" without checking — the branch was 138 BEHIND and a
     strict subset of master, so editing and merging it would have silently deleted a manifest note AND an
     entire disclosure entry; a dropped manifest entry stops absorbing and surfaces as a phantom regression in
     someone's later sweep. The lane was right to check rather than take the instruction.
     Same entry, rebase half: a seat's gates stamped at the old master expired when the train landed that
     seat's own predecessor, which edits the same file; the rebase is then not cosmetic — the syscall.Uname
     class arriving through a rebase instead of a merge.
     2026-09-08 (C1), the re-base acceptance — batch19, anchor old 6226: a hop re-base touching five files
     collided inside ONE LANE'S OWN TWO CUTS — a seat severs two fatal primitives onto a report type at the
     CURRENT release, while the hop cut REMOVES them because the newer Go moved them to another file. Both are
     right for their release and the union is neither. The stated acceptance was "0 conflicts on the runtime
     files, ONE named conflict resolved to theirs, AND the report type present TWICE afterwards" — the third
     clause is the one that fails on the silent restore of the pre-seat catchable exception, and it was written
     BEFORE acting on the ruling, not after. -->

## 5. Gate list and golden scope are derived AT THE UNION
- **A seat's golden re-baseline covers only the projects that EXISTED at its base**, so guards born on master AFTER the seat drift by exactly the seat's intended line and surface at the union CNR. Classify by the diff's CONTENT (the seat's own intended line, nothing else) and re-baseline at the train's tip with the runner's four phases as the check: a STATED fixup, never silent, never read as a regression.
- **A seat whose corpus footprint lands in BANKED packages owes those rows' sweeps as its OWN gate** — a full behavioral suite and GolibTests cannot see an alloc assert in a banked row at all. **A semantics fix that changes the COST of a Go construct is measured against the alloc-assert rows before it is called cheap** (an alloc mirror mapping to `GC.GetTotalAllocatedBytes` cannot be exempted by a counter). Remedy: unseat, fix forward on the branch (an uncounted site WITH the reason, never a weakened instrument), reseat with the sweeps.
- **Inverse — an emission change whose ONLY corpus movement is TEST-side in BANKED rows lands those hunks WITH the cut** (attribute lines only, numstat against the PRE-RUN tree): the lane's filtered sweeps re-derive exactly that emission, so the tree the sweeps validated is the tree that lands. Distinct from the stale-until-rebank class, for changes NO sweep in the train re-derives.
- **Tell — a golden the UNION CNR moves that NEITHER seat's own CNR could.** Not two rules composing: ONE rule meeting NEW SOURCE — **a seat's CNR is scoped to the SOURCES AT ITS BASE**, and a sibling seat's new source rows create the shape another seat's rule fires on. Before re-baselining, MEASURE the union emission (the filtered runner at the union — Compile plus Output against `go run`); re-baseline as an ASSEMBLY commit NAMING the composition, since no lane owns a golden; reproduce into SCRATCH, because the CNR leg's restore destroys the evidence. **When an inference about a lane's instrument can be measured in minutes, measure before naming it.**
- **A gate leg whose invocation was derived from the instrument BEFORE the seat measures the seated instrument wrong. Tell — exit 1 with an EMPTY table.** Derive a leg that exercises a seat from the SEAT tip, and give **every census leg a positive control that the classifier answered at all**. **A docs-delta limit distinguishes the two RUNBOOKS from the RECORDS**: a runbook is living procedure, read whole at landing, wider limit; a record takes dated blocks and a tight one.

<!-- 2026-09-03: two guards born on master AFTER the seat drifted by exactly the seat's intended line and
     surfaced at the union CNR. An array-range-copy seat regressed a banked row at the union sweep
     (224 -> 222 + 2 infra) where its full behavioral suite and GolibTests could not see an alloc assert in a
     banked row at all. The copy must genuinely not allocate: a rented snapshot returned on dispose, measured
     0 objects / 0 bytes against a 1,000-object control.
     Inverse measured 2026-09-05 (test-side hunks land with the cut); a banked row's committed test source
     never disagrees with the converter that just swept it.
     Union-only emission change, 2026-09-05: two converter cuts each byte-identical under their OWN CNR
     together stamped a named NESTED array (`type nn [2][3]int` -> `[GoArrayDims(2, 3)]`) that neither stamped
     alone. CORRECTED by the same coordinator the same day because the first reading of WHY was wrong: `nn`
     was declared and unused at that seat's base, so no wrapper was emitted and that seat's CNR was honestly
     byte-identical — MEASURED by rebuilding its exact converter in scratch: 0 diff lines — while a SIBLING
     seat added +93 rows of main.go exercising `nn`, so at the union the struct exists and the rule fires. The
     union emission is measured before re-baselining so the golden change is a proven-intended emission rather
     than a papered-over regression. The coordinator's first inference, "possible false-green CNR at a
     sub-agent tree", was retracted in the open.
     2026-09-07, gate leg derived from the pre-seat instrument: a train's deletion-pass leg called the
     instrument without the parameter the seat had made mandatory, the leg read exit 1 with an EMPTY table,
     and only its own vacuity control (KEEP-SELECTED = 0 -> "the classifier answered nothing") stopped that
     zero from reading as clean. Same entry: a flat docs-delta limit of 10 refused a legitimate procedure
     amendment. -->

## 6. The union is the gate of record
- A lane's own CNR is evidence for a seat REQUEST; **the union CNR at assembly is the gate of record.** A lane-side freeze slip that cannot reach a transpile verdict is REPORTED — that is the remedy — and does not re-owe the lane's run. **A branch-alone GREEN and a branch-alone RED are EQUALLY SILENT about the union**, and neither instrument can see the other's finding.
- **Tell — a finality table's CNR row reading "(read the log)":** the union CNR ended WITHOUT a verdict line and finalize's placeholder fill had a FALLBACK printing a string where the verdict belonged (route #6 inside a lane's own finalize — an absence MASKED instead of stopping). Retract FINAL, hold on a stated window, preflight the branch **from its MERGE BASE**.
- **A pass→fail on a BANKED row at a train head is a BROKEN SET that holds the landing until an arm names the seat** — master plus one seat at a time, the three-run standard, the attributed seat unseated at the tip.
- **A banked row reading RED at a union is attributed against the PREVIOUS unions' PRESERVED RECORDS before it is called a regression** — keep preserving, each union's record at a distinct path. **A filtered `-tests` run on a branch trains behind master measures the OLD closure**; a gated re-measure runs on the MERGE RESULT.
- **Tell — a CNR red on a SEAT BRANCH whose base PREDATES a landed emission change:** the drop set is the BASE's, not the seat's, and a ruled "a red stops the battery" must not be applied to a BASE artifact — say so inside the minute. Attribute it without a base arm from two facts: the branch changes ZERO non-test converter files, and landed master reads 0 at the same pin on the same box. The two-minute arm for any seat whose diff touches no non-test converter source is **`go build ./src/go2cs` at the base and at the seat tip giving the SAME sha256 and size under the pinned toolchain** — a CNR verdict is a function of the binary and the Go sources it reads, and the seat changes neither; the merge-base CNR stays the better arm for the POSITIVE half (that the drops are present on the bare base). **A count running a CONSTANT amount lower than master across readings is the projects master GAINED since the base** — explained, not a shortfall.

**Attribution when a leg reds**, in order:
1. **Bisect the TRAIN before reasoning about its seats — the intermediates already exist** (building an alternative assembly produces every intermediate SHA; the ladder is a checkout and a run per rung). **Probe the PRIME SUSPECT first — a bisect can end in ONE arm.**
2. **A tree that SHOWS a failure often cannot say whether the failure PREDATES it.** Build the arm with the FIX APPLIED and the suspect seats ABSENT — the only arm separating "the union broke it" from "the fix made a dead path REACHABLE".
3. **Copy only the CORPUS files of a fix into a bisect arm, never the converter registry** — the behavioral runner transpiles only the TEST tree, so leaving the converter alone keeps other seats' rows out of the arm.
4. **An IDENTICAL verdict count across two runs is NOT evidence nothing changed — the difference lives in the TAIL, never in the count.** **Report a guard's movement by FAILURE KIND** before "still failing" is read as "nothing changed": clearing a MASKING fault is progress even when the row stays red.
5. **A union set-diff's BROKEN entry is attributed by MECHANISM** — from the failure text and the seats touching that code — before it is called transient.

**What holds a train:**
- **Trading a MEASURABLE row for an UNMEASURABLE one is a REGRESSION even when no banked number moves** — the leg that reads by SET DIFFERENCE catches what pass-or-fail legs cannot.
- **A seat that changes a failure's mode from CAUGHT to UNCATCHABLE is a regression though it created no new defect. Compare failure MODES across the two trees before sizing a fix against the failure itself** — the ask is not "make the operation work" but "make it fail the way it failed before", refuse by name, catchable, the defect left open and LOUD.
- **A regression against a BANKED row is NOT eligible for accept-and-name** — that is for an OPEN defect; a change taking banked verdicts DOWN is either fixed or its seat comes out of the train.
- **A seat whose acceptance is a MEASUREMENT does not land on a SUBSTITUTE gate because the substitute is green** — when the acceptance is NAMED, the landing waits for it. **A guard keying on a NAME across the corpus needs the SCOPE that makes the name unique** (the disclosing package's own page).
- **A seat landing ONE SIDE of a two-route identity NAMES the assertion it breaks, in its seat message, derived from the DESIGN before any battery is asked** — a row passing because BOTH routes were equally wrong is not a regression, but the union will report it.

<!-- 2026-09-03: the union is the gate of record; the finalize FALLBACK printed a string where the verdict
     belonged and the lane retracted FINAL the same minute. The coordinator honoured the retraction with a
     stated hold window and independently preflighted the branch from its MERGE BASE — a two-point diff
     against a moved master read 92 files / -5,700 where the merge-base diff read 30 / -5.
     2026-09-04, standing red: one train's net/http FAIL was byte-for-byte the previous three trains' shape —
     every verdict matched, the leak check exiting 1 — a STANDING red that the crypto/tls merge rule does not
     reach, read in ONE command because the preservation rule had put each union's record at a distinct path.
     A union red with no prior record to compare against is the case that costs a bisect: keep preserving.
     Four attribution rules from one union, 2026-09-05: the cut's red control crashed BEFORE reaching the
     dial, so only an arm with the fix applied and the suspect seats absent could separate "the union broke
     it" from "the fix made a dead path REACHABLE" — landed master plus the same three fix files PASSED the
     guard completely; the assembly head with the same files failed every dial; one axis, same machine, four
     minutes apart. The pointer-token seat was the FIRST merge on the branch, so testing it cost one probe and
     settled the attribution outright. Identical verdict counts: the same row stopped at the same test both
     times because it is the first one that DIALS, while the mechanism changed completely — before, an access
     violation and NO results file; after, a results file stating the process ended before the host completed,
     with the sweep log carrying the refused socket option verbatim one line up. Failure-kind movement: an
     access violation with the stdout comparison never reached became a stdout MISMATCH with both sides
     exiting 0.
     2026-09-04: a value-versus-constructed identity assertion goes red BY CONSTRUCTION when only one route
     moves. The coordinator's half: a union set-diff's BROKEN entry is attributed by MECHANISM before it is
     called transient.
     2026-09-06, set difference: thirteen legs were green — the converter suite, a byte-identical corpus
     across 721 packages, three-platform compiles, a 684-project behavioural suite with zero failures, 25 of
     25 sweep rows — and the FOURTEENTH found that a row's caught nil-dereference had become an uncatchable
     ACCESS VIOLATION killing the host and leaving 221 rows unmeasurable. No banked row regressed and no
     roster number would have shown it: master measured that package 388 of 388, the union kills its host
     partway and leaves 221 rows unanswered, in a row a lane is actively working — so the train HELD. The four
     one-axis readings that attributed it (previous master, the suspect seat's own merge, the bare assembly
     head, the repaired head) also proved the repairs neither caused nor cured it, which is what a
     set-difference finding owes before it names a seat.
     Three rules from one held train, 2026-09-06: the coordinator offered accept-and-name for a reflect
     regression on the reasoning that its root was a model question too large to answer under a landing
     deadline, and withdrew it — 388 verdicts falling to 167 reported is the roster going backwards, and no
     elegance of root makes that landable. Caught-to-uncatchable: pre-seat the same operation already failed
     while the package still reported every verdict; post-seat the host dies and 221 rows go EMPTY.
     Bisect-the-train: the ladder over the known merge order — master, ten seats, eleven, thirteen, fifteen,
     sixteen — was a checkout and a run per rung and converged exactly; the measurement most needed was the
     cheapest available and it was run LAST, after an hour spent building an alternative to a misdiagnosed
     problem.
     2026-09-07 20:03, substitute gate: three host-fatal entries were landed on the converter suite while the
     seat's own STATED acceptance — the runtime row's reading — was still pending, on the reasoning that skip
     entries "can only make a row run further". They made it REFUSE: hostFatalMintViolations
     (testConversion.go) globs EVERY proof page and matches disclosed names by BARE NAME with no package
     scoping, so runtime's TestEmptyString is refused because encoding/json's passes — the row went from "runs
     to index 104" to "refuses at mint in 0.16 s", measurable -> unmeasurable, the regression class this file
     names. The lane bounded it (19 of runtime's 525 names collide, 12 of them the TestSmhasher* family; one
     of the four entries) and refused to report a count off a run that never ran. The corpus is 28,145 names.
     2026-09-08 (i9), a base's red — batch19, anchor old 6357: `CHANGED = 8` at the oracle pin on a seat's
     branch was EXACTLY the drop set the previous train's landing post had PRE-STATED as "seat 1 did not reach
     its sites". Proven without the base arm by the two facts above and posted inside the minute. Beside it,
     counts a constant 2 lower than master across three readings were the two projects master gained since the
     base. The one unmeasured arm (CNR on the bare merge-base) was NAMED by its author and declined by the
     coordinator as not owed.
     2026-09-08 (C2), same red, two-minute arm — same anchor: `go build ./src/go2cs` at the base and at the
     seat tip under the pinned toolchain gave the SAME sha256 and size, making that `CHANGED = 8` the base's BY
     MEASUREMENT and retiring the "which file kinds matter" argument.
     2026-09-08 (C2 and i9), branch-alone silence — same anchor, measured from OPPOSITE SIDES in the same hour:
     one lane's oracle pin read 8 on a pre-seat base that reads 0 at the union, while the other's split-token
     arms COMPILED on the branch and NOT at the union, where a train's six-parameter widening reaches them.
     Both instruments were run correctly and neither could see the other's finding — which is why seat gates
     are "the seat's own lines" while the union battery is the gate of record. Method note from the same
     exchange: the hazard (a real clobber, 19 blocks destroyed) and the cause of the open row were kept as TWO
     claims, and the finding survived because its author asked a question whose answer it could not control. -->

## 7. Rehearse the merges before the assembly runs
- Rehearse a train's merges **in a THROWAWAY WORKTREE at the landed master, SEQUENTIALLY** — a pairwise three-way against master cannot see SEAT-VERSUS-SEAT collisions. **Tell — a line-anchored marker count reads ZERO on a real conflict:** the old-form `merge-tree` PREFIXES its markers. **A rehearsal that MERGES without BUILDING is blind to the seam only the union reaches** — arms that compile on each branch alone fail at the union where a train's widened arity reaches them.
- **Verify each resolution BEFORE it is saved.** A both-kept CODE block is proven by `gofmt`, a build, and a bare `grep -c` COUNT of the symbols both sides own — never by the absence of markers; a failed three-way apply leaves a clean file MISSING one side's function behind a green `gofmt`. Check the exit code you print is the command's, not a pipe's. **A resolution replacing a WHOLE FILE with one branch's version silently drops every OTHER seat's change to it** — master plus each seat's patch is the shape.
- Apply the saved resolutions mechanically at assembly, stamped PRE-RESOLVED, so the assembly meets no surprise and the hand work happened where a mistake cost nothing.
- **Tell — a seat reported as ONE REAL CONFLICT on a record file:** a rehearsal pinned at a SHA expires the moment a train lands; that is a STALE REF. A rehearsal **PRINTS the master it rehearsed onto in its first line**, and a per-run copy **re-derives that SHA from `ls-remote`** instead of inheriting a literal. **A CHAINED seat's base is its PREDECESSOR, not master** — the NEAREST common ancestor of the seat with master AND every seat already folded, the maximal candidate, NO SINGLE MAXIMAL BASE stamped rather than silently resolved, and the stamp says CHAINED. **Tell — a hand `merge-file` chain reporting +1 line per class while the SET shows three new names and zero duplicates:** the wrong base turned the other side's ADDITIONS into DELETIONS at rc 0. **A count that disagrees with a SET is the INSTRUMENT, not the union** — the `git merge` rehearsal at each seat's TRUE base is the reading of record; the hand chain is a cross-check only.
- **A seat that adds keyed entries checks the UNION's rule for that map, not its base's.** A guard pinning entries BY EXACT KEY is a seam every later seat's entries cross: each branch green ALONE, the union RED on the first unpinned key by name. The fix is a UNION commit stamped as the train's own.

<!-- 2026-09-04, rehearsal: two seats appending to one test file, two to one record; the sequential rehearsal
     named FIVE conflicts where one was expected. One both-kept resolution SPLIT a function, and a failed
     three-way apply left a clean file MISSING one side's function behind a green gofmt, caught only by the
     2-of-3 symbol count (and the exit printed for vet was a pipe's).
     2026-09-07, stale rehearsal SHA: a rehearse copy carried its master SHA as a literal and, run after the
     next train had landed, reported a seat as ONE REAL CONFLICT on a record file — the pairwise 3-way against
     the landed master merged byte-identical to the seat. Same entry, chained seats: a rehearsal taking every
     seat's merge-base against master read a seat cut on top of an earlier seat as a conflict on the earlier
     seat's own appends (seat 4 on seat 2, one false hunk).
     2026-09-04, keyed-entry guard: one train's amendment made a registry guard per-entry — an unpinned key
     fails outright — while a sibling seat's twelve entries had been cut before it under the older suffix
     rule; the crypto/tls merge shape in a converter test rather than in a corpus row. The rehearsal caught it.
     Two instrument notes from the same hour: whole-file resolution drops other seats' changes, and a bare
     `grep -c` of a symbol that must read N is the check that caught a clean gofmt hiding a missing function.
     2026-09-08 (coordinator), `merge-file` chained on the wrong base — batch19, anchor old 6388: a union check
     over the four MSTest classes read +1 Check line per class with THREE new names and zero duplicates, so two
     master lines had VANISHED. One seat had been chained against the landed master where its true
     `git merge-base` was an earlier commit: the seat's unchanged copy then LACKED the previous train's two
     Check lines and `merge-file` honoured that as a removal, at rc 0. Re-run with each seat's OWN merge base
     the chain read 693 -> 696, one line per step, nothing removed. -->

## 8. Assembly and landing
- **Tell — a conflict count that disagrees with what you grepped:** a `head -12` grep showed FOUR conflicted roster blocks where there were SIX (the filtered-status trap in a grep costume), a resolver asserting `len == 4` bailed BEFORE writing, and the `git add` chained after it with `;` staged marker-bearing files. **Chain with `&&`, never `;`.** Resolution rule that worked: **take the RULED side's prose for every conflicted block and re-derive only the numbers**, then let the guard-as-calculator confirm. **`${X:-default}` treats an EMPTY env override as UNSET**, so blanking a seat by env cannot skip it — a seat script carries an explicit already-seated list.
- **An alternative that must be COSTED is BUILT, not described.** A file-overlap census predicts where conflicts are POSSIBLE; only an assembly says where they ARE — a conflict is a property of the three-way BASE once a seat is absent, not of which files a commit touches.
- **An assembly commit carrying a converter change owes the same measured blast radius as a seat, taken BEFORE it is seated** — otherwise it is an unmeasured seat wearing the coordinator's authority. A lane's seat arrives with its own gate lines; an assembly commit arrives with none, and the battery that follows attributes to the whole train.
- **Every gate and assertion CARRIED from the previous train is re-read against the NEW seat set AT DERIVE TIME — a derive that keeps them verbatim has re-asserted the previous train.** Tell — a carried pure-append PREFIX test reddening a CORRECT docs seat: a board and a RECON amendment are in-place INSERTIONS, not appends. When a correct seat breaks a carried assertion the remedy is a STRONGER arm, never an exemption: admit exactly the ruled file set AND require the union's change on each admitted file to EQUAL that seat's own. **A GOLDEN IS ITS EMISSION, so the arm is BLOB IDENTITY** — `main.cs` and its `.cs.target` are the SAME blob — which beats a numstat (a different fifteen lines match a count, not a sha) and beats a line-set hash; take the reference blobs WITH the seat owner's enumeration ("fifteen paths, no others"), so a sixteenth file or a different blob is provably a rider. **A derive that FIXES an instrument fault at the source carries NO acceptance path for it** — an acceptance path that outlives its fault is a lie-lever waiting — and an IMPLAUSIBLE count is the derive's own dry-read control.
- **A seated-but-unlanded branch is INVISIBLE to every check a lane can run, and that is the coordinator's debt.** Put the SEAT LEDGER's contents into status posts BY NAME; **"specified and queued" means nothing if the QUEUE IS LOCAL** — send the artifact or say it does not exist where they stand; **when a lane asks for a REF instead of a description, PUBLISH one** (read-only, transient, nothing based on or merged into it, deleted at landing) and answer "which commits touch my subject" by MEASURING it.
- Lane-side: **a row's current NUMBER can depend on an UNLANDED seat, so a measurement at master answers about a DIFFERENT row** — assert the seat is an ANCESTOR of the measurement tree, build the converter from that tree, and check the binary's mtime MOVED rather than merely existing. Such a row's improvement ARRIVES WITH ITS TRAIN and belongs in the objective's arithmetic, not in a lane's memory. **A lane's accepted zero is stated WITH ITS POPULATION**, so a train does not assemble on a broader reading than the arms support. **Tell — CS0246 on a seat's OWN type inside a measurement tree:** the type lives only on an UNLANDED peer's branches, so that tree CANNOT carry the seat's shape — keep the pre-seat pair BY NECESSITY and SAY SO in the reading. **Never patch a CONTESTED seat into a measurement tree to get its shape** — resolving that contest inside a measurement tree chooses an order that is its owner's to choose — and restate the prediction WITH its condition rather than carrying it.

**Land-script mechanics:**
- **Resolve `${BASH_SOURCE[0]}` ABSOLUTE before any `cd`, and with it every RECORD and LOG path derived from it** — one built relative and read AFTER the `cd` exits on a well-formed EMPTY off a file it could not open ("no ASSEMBLE DONE stamp", exit 3), which is why the fix is measured BOTH ways (readable after `cd`: old no, new yes). **Tell — a refusal naming a finding ("the landing census is not the assembly census", "0 of 6 legs wired") while all of the guard's own patterns read blank.** That is an INSTRUMENT fault: make an empty self-read refuse under its true name, never under the finding's, and never let it set a whole battery's failure flag. **A self-check running from the LAUNCH cwd certifies the LAUNCH cwd** — run it from the cwd the real run uses AFTER its own `cd`.
- **Merge arithmetic is FIRST-PARENT:** `rev-list --merges` reads 4 for a seat whose own history carries 3 merges, so the gate is `--merges --first-parent == 1`, with `all == first-parent + internal` as the second derivation.
- **A LIVE gate keyed on the converter BINARY reads FREE hundreds of times during a CNR that holds the slot** (one short-lived `go2cs.exe` per package). Take THREE readings — the binary token; the harness HOSTS by command line, age-filtered so the census cannot self-match; battery-log freshness while the DONE stamp is absent — and refuse on any.
- **A prune keyed on a branch name GIVEN rather than READ from `ls-remote` reports "already pruned" over a LIVE branch** — one word of difference in the name is enough. **Past a proven instrument fault the landing takes ONE NARROWLY CONDITIONED path** — exactly that refusal, the standalone file present with its control, everything else intact — never a general override.
- **Define a refusal-pattern list ONCE as an array and read it from EVERY consumer** — the scan, the scan's control and the acceptance set are three copies about to drift. **STAMP an exclusion after the loop whether or not it is in force**, so it can never be silent, while the scan's positive control applies NONE; and compute a narrow acceptance path's refusal SET from that SAME array, so it can only accept the exact state the scan would otherwise have refused on.

<!-- Four traps in one assembly, 2026-09-03: a seat cut off an OLDER base conflicted on SIX roster blocks, not
     the four a `head -12` grep showed; the `;`-vs-`&&` rule again, caught before commit by the roster guard
     reading the HEAD side; the ${X:-default} empty-override trap.
     2026-09-06, one held train: a seat-drop was sized by diffing each commit's file list against the dropped
     seat's two named collision points (a registry and a golden) and NEITHER conflicted, while a file the
     census had attributed to a different seat did. The real assembly came in CHEAPER than the estimate — 16
     merges, ONE trivial resolution where the seat's side was a strict superset, zero markers — and it is the
     only thing that could have found the actual collision. The other half: a hand-own cut by a sub-agent and
     merged as an ASSEMBLY commit carried a CONVERTER registry change, so its blast radius was never the two
     corpus files it edited and it emptied 221 verdicts in a package two subsystems away — while four hours
     were spent blaming a lane's seat.
     2026-09-06, seated-but-unlanded: the assembly is local to the coordinator's machine BY DESIGN, so a lane
     censusing DELIVERABLE PRESENCE at master — the correct instrument, since a merged branch's ref is pruned
     and a surviving ref is the tell — correctly read five of its own branches as ABSENT while they were
     ancestors of a live assembly head. The lane's three right moves: census rather than guess, ask rather
     than re-offer, NAME what it could not see. An instrument was queued with predictions on record, in a file
     on the coordinator's machine that no lane can read, and the lane that could not find the specification
     derived its own, which cost it time and produced a BETTER instrument. Measuring the published reference
     branch: exactly one commit of twenty-one touched the row's own trees, it was the one the dispatch was
     about, and the nearest reachable approximation would have been contaminated in precisely that place.
     Three land-script mechanics from one derive, 2026-09-07: (a) a land script refused a clean landing with
     "the landing census is not the assembly census" when all four of its own patterns read blank (22:17);
     (b) first-parent merge arithmetic; (c) the three-reading LIVE gate, and the prune keyed on a given rather
     than read branch name.
     (d) 2026-09-08, the SAME self-path defect met a second time: a battery leg asking "are the six legs wired
     in this script" grepped its own RELATIVE path from inside the assembly worktree, reported 0 of 6, and set
     the whole battery's failure flag over legs that were unaffected — an instrument fault, not a wiring fault
     — re-measured standalone by ABSOLUTE path (6 of 6, control 5 of 6) and recorded beside the log. Beyond
     "resolve the self-path before any cd": a self-check that runs from the LAUNCH cwd certifies the LAUNCH
     cwd. And the landing takes ONE NARROWLY CONDITIONED path — exactly that refusal, the standalone file
     present with its control, everything else intact — never a general override. Beside it: a lane's accepted
     zero is stated WITH ITS POPULATION.
     (e) 2026-09-08 — batch19, anchor old 6459: the SAME self-path defect had a SECOND LATENT INSTANCE one gate
     over. The land script built its RECORD and LOG paths from a relative `$(dirname "$SELF")` and read them
     AFTER the `cd` into the worktree, so launched by a bare relative name it would have exited 3 with "no
     ASSEMBLE DONE stamp" — a well-formed empty off a file it could not open — measured both ways (readable
     after `cd`: OLD no, NEW absolute yes). Three instrument mechanics from the same amendment: the
     refusal-pattern array read by every consumer (the scan, the scan's control and the acceptance set were
     three copies about to drift); the exclusion STAMPED after the loop whether or not in force, with the
     positive control applying none; and the narrow acceptance path computing the record's refusal SET from
     that same array.
     2026-09-08, carried gates — same anchor: one train's pure-append PREFIX test would have reddened TWO
     CORRECT docs seats on the next train, because a board and a RECON amendment are in-place INSERTIONS; found
     by a census of the derived script's own output and replaced by the board's structural raw/endraw guard.
     Beside it, a derive that fixes an instrument fault at the source carries NO acceptance path for it — two
     named acceptance paths were deliberately not carried forward and the assemble script's predicate was
     corrected with a six-arm control. The derive's own dry-read took FOUR corrections, each caught by an
     IMPLAUSIBLE count (19/37/16/7 of 43 anchors "wrong"), and its rehearsal reproduced the previous train's
     relative-self-path fault on its first run.
     2026-09-08 (coordinator with G), a carried A-assertion — same anchor: "no seat may move a committed
     behavioral golden" was TRUE of the train it was written for and FALSE of the next, whose seat modifies a
     LANDED guard by an ANNOUNCED and ACCEPTED golden change; the assembly merged six seats clean and stopped
     at that assertion before any leg, which is the right shape. The replacement arm admits exactly the ruled
     file set and requires the union's change on each admitted file to EQUAL the seat's own, by BLOB IDENTITY
     (`main.cs` and its `.cs.target` are the same blob), beating both a numstat and a line-set hash; the seat
     owner supplied the reference values WITH the enumeration ("fifteen paths, no others").
     2026-09-08 (R), a measurement tree that cannot carry a seat's shape — batch19, anchor old 6118: a ruled
     re-pointing could not be done in a hop ladder because the type exists on a PEER's UNLANDED branches and
     nowhere in master or the ladder (CS0246), so the ladder kept the pre-seat exception pair BY NECESSITY and
     said so; the lane REFUSED to patch the contested seat in, because resolving that contest inside a
     measurement tree would be choosing an order that is its owner's to choose, and restated the prediction
     WITH its condition rather than carrying it. The same tree's `sync/runtime_impl.cs` 3-way (base 320 / ours
     239 / theirs 344 -> 263, rc 0) kept the lane's own NINE references where a verbatim take would have
     dropped them silently — the subtraction class of step 4, one file over. -->
