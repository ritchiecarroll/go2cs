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
     context, so provenance kept this way costs ZERO tokens. -->

- **⚠ MID-BATTERY SOURCE FREEZE — while any gate battery is running, converter/gen/golib source is
  untouchable, on ANY branch (ruled 2026-08-30).** The behavioral runners rebuild `go2cs.exe` from
  DISK source the moment a `.go` file is newer than the binary, and golib/gen compile into every
  project the battery builds — so an edit mid-run makes the remaining legs measure a MIX of committed
  and uncommitted state (route #1's stale-binary trap inverted: a too-FRESH binary). Lanes queue
  their cuts until the battery's summary prints; the coordinator announces battery start/close on the
  mailbox for exactly this reason.
  ⚠ Scope, stated 2026-09-02 after a lane held a cut it never needed to: the freeze binds **the
  worktree the battery runs in, on any branch checked out THERE** — the runners rebuild `go2cs.exe`
  from that tree's disk and golib/gen compile into the projects that battery builds, so a lane
  editing its own clone on its own machine cannot reach a battery leg elsewhere on the fleet.
  ⚠ **An UNPINNED SHELL violates the freeze without failing anything** (2026-09-04): `IsConverterStale`
  compares the exe's embedded release against the AMBIENT toolchain, so an unpinned shell makes every
  harness invocation REBUILD `go2cs.exe` — in a worktree carrying a battery that is the freeze broken
  by a shell rather than by an edit, and the only reason one such run was benign is `go.mod`'s own
  `go 1.23.12` line steering the toolchain switch, not the pin. Pin the shell (route #4's stamp is
  what it is compared against), and read a surprise converter rebuild as this before anything else.
  ⚠ **AND THE FREEZE BINDS THE HARNESS SCRIPTS** (2026-09-04, the coordinator against itself):
  **bash reads a script incrementally BY BYTE OFFSET**, so an insertion above the current position
  shifts the bytes and the next command is parsed from the middle of a line — a train assembly died
  thirty minutes after a seat slot was inserted into that same file for the NEXT train, on
  `syntax error near unexpected token '('` at a leg with nothing to do with the edit. `bash -n` passed
  before AND after; the running process never re-parses. **Launch every long run from a PER-RUN COPY
  of its script** (`coord-<train>-assemble-runN.sh`) and edit only the original; the relaunch then
  resumes on the intact head behind a skip flag, since the seats, the guards and the completed leg's
  own log holding its verdict are not in doubt.
  ⚠ **The door that opens by ITSELF: a DERIVE for the NEXT train must never write the RUNNING train's
  scripts** (2026-09-05). One derive's copied writer pairs carried the previous train's names as BOTH
  source and destination and rewrote the running assembly script IN PLACE while its chain was executing
  it — saved only by the chain being blocked inside a leg's command and by the derive being
  deterministic, so a re-derive restored the launched bytes. **A derive's writer REFUSES any path
  belonging to the running train**, and the freeze above binds coordinator scripts exactly as it binds
  converter source.
- **⚠ SEATING MECHANICS, five rules measured 2026-09-03.** **A SEATED branch takes NO commits at
  all — not even a doc-only one**: the seat's arithmetic is a claim about a SHA, "harmless" is the
  seat owner's judgment, and a train assembling in the two-minute window would have merged something
  unverified; a lane's follow-on work goes on its OWN branch off the same base (the corollary of
  announce-then-push). **A seating instruction gets its OWN LINE at the top of a post** — one embedded
  in a post about something else was read past for forty minutes — and **the slot takes the REMOTE
  TIP** while the assembly log prints the merged PARENT SHA, which is what the lane checks: a stale
  SHA quoted in a train listing would have seated a PARENT and dropped the row that made the tree
  honest, undetectable by the format guard because the table would be consistent and merely untrue.
  **A measurement tree must CONTAIN every seat the cut depends on** — `git merge-base --is-ancestor
  <seat> HEAD` before a single gate runs — because "the fix is in master" says nothing about a branch
  that forked before it: a stacked tree lacking a train's seam fix reported the ORIGINAL bug as a
  fresh "twin" of the fix, retracted within ten minutes once the seat tree was measured. And the
  coordinator's half: **a dispatch that names a MECHANISM nobody has measured is a "measure why first"
  order, never a cut.**
  ⚠ **DERIVE A TRAIN'S SEAT LIST FROM MERGE PARENTS, NEVER FROM MERGE MESSAGES.**
  `git log --merges --format='%P' <base>..<tip> | awk '{print $2}'` yields exactly one second parent per
  merge — the seat tip — and it is exact. A regex over merge MESSAGES over-counts, because a merge
  message legitimately MENTIONS branches it did not merge: measured 2026-09-06, message-scraping
  reported 21 seats for a train with 20 merge commits, the extra being a branch named inside another
  seat's message. **The over-match was harmless only because the gate consuming the list asks an
  ancestry question that is right regardless of why a name is on it** — a list built for a checker that
  trusted it would have been wrong. Resolving each second parent back to a branch still pointing at that
  sha is a free bonus check: a name that no longer resolves is a seat amended AFTER it landed.
  ⚠ **THREE MORE DISPATCH RULES, 2026-09-04.** **A dispatch's PREMISE is re-derived against the roster
  at DISPATCH time**, never carried from the census's read date: a remaining-rows record read on the
  2nd named a row as an unowned stub, the row BANKED on the 3rd, and the dispatch went out on the 4th
  with the stale clause in every line — the lane's first act was to MEASURE the premise and post the
  table, which is the right first act. **A dispatch gated "after the landing" idles a lane for as long
  as the battery runs, and the gate is usually unnecessary**: when the lane's own seat lands
  UNCHANGED, a branch cut on that seat's tip merges onto the landed master with no seam the seat does
  not already own — so **gate on the SEAM** (a file both sides touch, a registry both register into),
  never on the SHA; a lane silent past the watch's threshold with a landing-gated item is the
  coordinator's idle, not the lane's. And **a cloud lane cannot read the coordinator's scripts
  directory**, so a queue file it is dispatched from is pasted to the mailbox VERBATIM.
  ⚠ **A DISPATCH BUILT FROM A MEMORY FILE IS A CLAIM ABOUT THE CODE, AND A CLAIM ABOUT THE CODE IS READ
  AT THE TREE — a stale record quoted as an INSTRUCTION is worse than one quoted as a description.** A
  coordinator note recorded a package's x86 feature detection as unowned; that was true at its dispatch
  and the work LANDED the same day. Quoted five days later it sent a lane to build what already existed;
  the lane's *"STOP BEFORE I BUILD IT"* cost minutes instead of a day (2026-09-07). **Verify at the tree
  before dispatching, and give a resolved record a DO-NOT-DISPATCH banner rather than leaving it phrased
  as a task.** ⚠ **And a coordinator TRAP handed to a lane is a claim, and a wrong one propagates to
  every lane that inherits it**: a dispatch stated "the host is hand-owned, so it is yours to edit
  directly — there is no converter change here", and the bill was **one hand-own edit plus TWO
  converter-side changes**, found BY BUILDING IT. The lane's framing is the rule — *"I would rather
  correct it here than have the next lane inherit it"* — so **a trap list is doctrine in miniature and is
  owed the same standard as doctrine**: read at the tree before it is written, retired loudly when a lane
  refutes it.
  ⚠ **TWO MORE, 2026-09-05, both from a queue file that did not carry its own state.** **A queue file
  carries its LANDED state or the item is dispatched TWICE** — an item landed on train 23 was
  re-dispatched from a file that never got its mark, and the tell that caught it was
  `git worktree add -b` REFUSING an existing branch name; every queue file opens with a `# STATUS`
  header, and the coordinator stamps it AT THE LANDING, never from memory. **And a dispatch preflight
  probe scoped to where the FIX would live is structurally BLIND to a seat that banked a NEGATIVE
  result** — it touched no converter file precisely because it correctly wrote no fix — while
  `ls-remote --heads` is EMPTY for a merged branch because the remote ref is pruned at merge. The
  durable pair is `git ls-tree -r origin/master -- <the deliverable's path>` (the guard project, the
  doc, the manifest) plus `git merge-base --is-ancestor <local branch> origin/master`, with the
  surviving LOCAL branch as the tell that fires when both of those read new.
  ⚠ **A LOCAL BRANCH WITH ZERO COMMITS OVER MASTER WHOSE TIP IS AN ANCESTOR OF MASTER HAS LANDED, NOT
  "NOT STARTED"** (2026-09-07): the coordinator read `commits over master: 0` on a placement branch as
  an empty placement and RE-DISPATCHED it, while the very same check printed `is-ancestor-of-master:
  yes` — the tell named just above, read past inside one instrument's own output. Before re-dispatching
  anything, run `git log --oneline origin/master -- <deliverable path>` AND `git merge-base
  --is-ancestor <local tip> origin/master`. **And a memory note saying "dispatched, reviewed before
  landing" is a description of a MOMENT: it gets its LANDED SHA the day the train lands.**
  ⚠ **And the lane's half of the same idle: a lane whose ORDERED items have all landed STARTS the next
  item on its list without waiting for a coordinator prompt** (2026-09-05, three quiet hours a lane
  named honestly). **The mailbox is read BETWEEN steps, not INSTEAD of them.**
  ⚠ **TWO MORE, 2026-09-06, both about what a SEATED branch may carry.** The no-commits rule has a
  consequence worth spelling: **a correction to a seated record rides in the CUT's OWN commit as a
  DATED amendment, leaving the original sentence visible above it**, because a prediction is never
  edited after its result — including a prediction that turns out to be an inverted statement of fact
  — and **the coordinator corrects the SEAT MESSAGE too when that message repeated the error**, since
  the seat message is the train's own record of what it merged and is where a future reader looks
  first. And **a repair is BASED on the commit that INTRODUCED the defect**, not on a local assembly
  head that has no remote ref: based that way it merges cleanly into any assembly containing that seat
  and it reviews as a repair of a named thing — a train assembles on the coordinator's machine and
  lands by pushing master, so a base nobody can fetch is a base nobody can check.
  ⚠ **A GENERALISATION FROM ONE MEASURED ROW TO ITS SIBLING IS A HYPOTHESIS, NOT A ROUTING**
  (2026-09-06). A confident dispatch said two remaining rows sat behind one unimplemented stub; the
  sibling MEASURED an hour later RUNS — 122 of 160 matching, 38 differing on an entirely different
  axis — so the generalisation was wrong in the direction that costs a lane time, by naming a large
  row as cheap. **The measurement took four minutes: measure the sibling BEFORE naming it in a
  dispatch.**
  ⚠ **A MEASUREMENT IS NOT AUTOMATICALLY THE EARLIEST EVIDENCE — ask the row's OWNER before dispatching
  a diagnosis of it.** A coordinator measured a crash on another lane's row, read it as the first
  sighting, and dispatched a second lane to root it; the owner had already named the failing string
  **character for character** before the measurement existed, with the fix written, guarded, gated and
  pushed, and called STOP before the duplicate work landed (2026-09-07). **The ordering matters for the
  record as much as for the effort: the fix was not a guess at the measurement; the measurement was a
  confirmation of the diagnosis.** A coordinator holding a fresh number is the person most likely to
  mistake it for the first one.
- **⚠ REBASE AND RE-LANDING, two shapes with explicit acceptances (2026-09-03).** Two of three
  "conflicts" against a new master were ONE DUPLICATE COMMIT (the same patch as the landed seat,
  differing only in blob ids and one hunk offset) — **dropped by `rebase --onto`, not resolved by
  editing** — and a rebase is verified by ARITHMETIC (commit count minus the duplicate, the doc
  byte-equal, the applied delta identical to the original over its files, a temp-index 3-way reporting
  zero unmerged paths first) and lands as a **NEW branch** so the posted SHA stays untouched.
  Re-landing work that master MERGED and then content-REVERTED is the other shape: a direct merge of
  the original branch fights the revert everywhere it touched (58 conflicts, a modify/delete on a test
  the revert removed) and a rebase may silently drop the already-upstream body, so **the admissible
  form is revert-the-revert then cherry-pick the fix**. Its acceptance had to be rewritten too: "the
  re-landed tree equals the branch tip over `src/…`" CANNOT be empty once master has moved, so the
  acceptance is **PATCH-EQUIVALENCE** — the re-landing's delta against current master has the same
  file set and per-file numstat as the branch's own delta (`base^..tip`), the stable patch-id as the
  strong form, the displaced construct's occurrence count at the branch's number, and zero markers.
  **State an acceptance as a relation between two DELTAS, never between two trees, whenever the base
  has moved.**
  ⚠ **NAMING A BRANCH AS A CONVENIENT HOME FOR AN EDIT IS A REF CLAIM AND CARRIES THE REF RULES — check
  it is CURRENT and that its content is not a SUBSET of master, BEFORE recommending it.** A coordinator
  told a lane *"you have that branch open on that very file, so the edit is one line in a tree you
  already hold"* without checking: the branch was **138 behind** and a strict subset of master, so
  editing and merging it would have silently deleted a manifest note AND an entire disclosure entry —
  the silent-subtraction shape, in a manifest, where a dropped entry stops absorbing and surfaces as a
  phantom regression in someone's later sweep (2026-09-07). **A stale branch offered as a shortcut is a
  silent-subtraction delivery mechanism**, and the lane was right to check rather than take the
  instruction. ⚠ **And a REBASE across a file the predecessor also touches is where the silent
  subtraction lives — verify BOTH changes are present in the rebased tree, BY NAME.** A seat's gates
  stamped at the old master expired when the train landed that seat's own predecessor, which edits the
  same file; the rebase is then not cosmetic, and taking one side of the shared file drops the other
  with no conflict and no marker — the `syscall.Uname` class arriving through a rebase instead of a
  merge.
- **⚠ A SEAT'S GATE LIST AND GOLDEN SCOPE ARE BOTH DERIVED AT THE UNION, not at the seat's base
  (2026-09-03).** A seat's golden re-baseline covers only the projects that EXISTED at its base, so
  two guards born on master AFTER the seat drifted by exactly the seat's intended line and surfaced
  at the union CNR — classify by the diff's CONTENT (the seat's own intended line, nothing else) and
  re-baseline at the train's tip with the runner's four phases as the check: a STATED fixup, never a
  silent one, and never read as a regression. And **a seat whose corpus footprint lands in BANKED
  packages owes those rows' sweeps as its OWN gate**: an array-range-copy seat regressed a banked row
  at the union sweep (224 → 222 + 2 infra) where its full behavioral suite and GolibTests could not
  see an alloc assert in a banked row at all. Its companion rule: **a semantics fix that changes the
  COST of a Go construct is measured against the alloc-assert rows before it is called cheap** — an
  alloc mirror that maps to `GC.GetTotalAllocatedBytes` cannot be exempted by a counter, so the copy
  must genuinely not allocate (a rented snapshot returned on dispose, measured 0 objects / 0 bytes
  against a 1,000-object control). Remedy shape: unseat, fix forward on the branch (an uncounted site
  with the reason, never a weakened instrument), reseat with the sweeps.
  ⚠ **Its inverse, 2026-09-05: an emission change whose ONLY corpus movement is TEST-side in BANKED
  rows lands those hunks WITH the cut** — attribute lines only, numstat taken against the PRE-RUN
  tree. The lane's filtered sweeps re-derive exactly that emission, so **the tree the sweeps
  validated is the tree that lands**, and a banked row's committed test source never disagrees with
  the converter that just swept it. Distinct from the stale-until-rebank class, which is for changes
  NO sweep in the train re-derives.
  ⚠ **A UNION-ONLY EMISSION CHANGE — a golden the union CNR moves that NEITHER seat's own CNR could**
  (2026-09-05). Two converter cuts each byte-identical under their OWN CNR together stamped a named
  NESTED array (`type nn [2][3]int` → `[GoArrayDims(2, 3)]`) that neither stamped alone. ⚠ **Corrected
  by the same coordinator the same day, because the first reading of WHY was wrong**: it is not "two
  rules composing" — it is ONE rule meeting NEW SOURCE. The named-array stamp had nothing to stamp at
  its own base (`nn` was declared and unused, so no wrapper was emitted, and that seat's CNR was
  honestly byte-identical — MEASURED by rebuilding its exact converter in scratch: 0 diff lines),
  while a SIBLING seat added +93 rows of `main.go` exercising `nn`, so at the union the struct exists
  and the rule fires. **A seat's CNR is scoped to the SOURCES AT ITS BASE; a sibling seat landing new
  source rows creates the shape another seat's rule fires on; only the union CNR sees it.** Handling:
  before re-baselining, MEASURE the union emission (the filtered runner at the union — Compile plus
  Output against `go run`) so the golden change is a proven-intended emission rather than a
  papered-over regression; the re-baseline is an ASSEMBLY commit NAMING the composition, since no lane
  owns a golden; and because the CNR leg's restore step destroys the evidence, reproduce the emission
  into SCRATCH with the union converter. (The coordinator's first inference — "possible false-green CNR
  at a sub-agent tree" — was retracted in the open: **when an inference about a lane's instrument can
  be measured in minutes, measure before naming it.**)
  ⚠ **A GATE LEG WHOSE INVOCATION WAS DERIVED FROM THE INSTRUMENT *BEFORE* THE SEAT MEASURES THE SEATED
  INSTRUMENT WRONG** (2026-09-07): a train's deletion-pass leg called the instrument without the
  parameter the seat had made mandatory, the leg read exit 1 with an EMPTY table, and only its own
  vacuity control (KEEP-SELECTED = 0 → "the classifier answered nothing") stopped that zero from
  reading as clean. **A leg that exercises a seat is derived from the SEAT tip, and every census leg
  carries a positive control that the classifier answered at all.** Companion: **a docs-delta limit
  distinguishes the two RUNBOOKS from the RECORDS** — a runbook is living procedure, a step rewrite is
  an amendment, it is read whole at landing and takes a wider limit, while a record takes dated blocks
  and a tight one. A flat limit of 10 refused a legitimate procedure amendment.
- **⚠ THE UNION IS THE GATE OF RECORD, and a landing HOLDS on a broken banked set (2026-09-03).** A
  lane's own CNR is evidence for a seat REQUEST; the union CNR at assembly is the gate of record — so
  a lane-side freeze slip that cannot reach a transpile verdict is REPORTED, which is the remedy, and
  does not re-owe the lane's run. ⚠ A finality table's CNR row read "(read the log)" because the union
  CNR had ended WITHOUT a verdict line and the finalize step's placeholder fill had a FALLBACK that
  printed a string where the verdict belonged — route #6's shape inside a lane's own finalize, an
  absence MASKED instead of stopping; the lane retracted FINAL the same minute, the coordinator
  honoured the retraction with a stated hold window and independently preflighted the branch **from
  its MERGE BASE** (a two-point diff against a moved master had read 92 files / −5,700 where the
  merge-base diff read 30 / −5). And **a pass→fail on a BANKED row at a train head is a BROKEN SET
  that holds the landing until an arm names the seat** — master plus one seat at a time, the
  three-run standard, the attributed seat unseated at the tip.
  ⚠ **A banked row reading RED at a union is attributed against the PREVIOUS unions' PRESERVED RECORDS
  before it is called a regression** (2026-09-04): one train's `net/http` FAIL was byte-for-byte the
  previous three trains' shape — every verdict matched, the leak check exiting 1 — a STANDING red that
  the crypto/tls merge rule does not reach, read in ONE command because the preservation rule had put
  each union's record at a distinct path. **A union red with no prior record to compare against is the
  case that costs a bisect: keep preserving.** And **a filtered `-tests` run on a branch trains behind
  master measures the OLD closure**, so a gated re-measure runs on the MERGE RESULT.
  ⚠ **FOUR ATTRIBUTION RULES FROM ONE UNION (2026-09-05).** **A tree that SHOWS a failure often cannot
  say whether the failure PREDATES it**: the cut's red control crashed BEFORE reaching the dial, so
  only an arm with the FIX APPLIED and the suspect seats ABSENT could separate "the union broke it"
  from "the fix made a dead path REACHABLE" — build that arm (landed master plus the same three fix
  files PASSED the guard completely; the assembly head with the same files failed every dial; one axis,
  same machine, four minutes apart). **Probe the PRIME SUSPECT first and a bisect can end in ONE arm** —
  the pointer-token seat was the FIRST merge on the branch, so testing it cost one probe and settled the
  attribution outright — and **copy only the CORPUS files of a fix into a bisect arm, never the
  converter registry**, since the behavioral runner transpiles only the TEST tree, so the corpus files
  are what the guard compiles against and leaving the converter alone keeps other seats' rows out of the
  arm. **An IDENTICAL verdict count across two runs is NOT evidence that nothing changed**: the same row
  stopped at the same test both times because it is the first one that DIALS, while the mechanism
  changed completely — before, an access violation and NO results file; after, a results file stating
  the process ended before the host completed, with the sweep log carrying the refused socket option
  verbatim one line up. **The difference lived in the TAIL, never in the count.** And **clearing a
  MASKING fault is progress even when the row stays red**: an access violation with the stdout
  comparison never reached became a stdout MISMATCH with both sides exiting 0 — **report a guard's
  movement by FAILURE KIND** before anyone reads "still failing" as "nothing changed".
  ⚠ **A seat that lands ONE SIDE of a two-route identity NAMES the assertion it breaks — in its seat
  message, derived from the DESIGN before any battery is asked** (2026-09-04): a
  value-versus-constructed identity assertion goes red BY CONSTRUCTION when only one route moves, and
  a row that had been passing because BOTH routes were equally wrong is not a regression — but it is
  still named, because the union will report it. The coordinator's half: **a union set-diff's BROKEN
  entry is attributed by MECHANISM — from the failure text and the seats touching that code — before
  it is called transient.**
  ⚠ **THE LEG THAT READS BY SET DIFFERENCE CATCHES WHAT PASS-OR-FAIL LEGS CANNOT** (2026-09-06).
  Thirteen legs were green — the converter suite, a byte-identical corpus across 721 packages,
  three-platform compiles, a 684-project behavioural suite with zero failures, 25 of 25 sweep rows —
  and the FOURTEENTH found that a row's caught nil-dereference had become an uncatchable ACCESS
  VIOLATION killing the host and leaving 221 rows unmeasurable. No banked row regressed and no roster
  number would have shown it. **Trading a MEASURABLE row for an UNMEASURABLE one is a REGRESSION even
  when no banked number moves**: master measured that package 388 of 388, the union kills its host
  partway and leaves 221 rows unanswered, in a row a lane is actively working — so the train HELD. The
  four one-axis readings that attributed it (previous master, the suspect seat's own merge, the bare
  assembly head, the repaired head) also proved the repairs neither caused nor cured it, which is what
  a set-difference finding owes before it names a seat.
  ⚠ **THREE RULES FROM ONE HELD TRAIN, 2026-09-06.** **A REGRESSION AGAINST A BANKED ROW IS NOT
  ELIGIBLE FOR ACCEPT-AND-NAME** — landing with a defect named and open is for an OPEN defect; a
  change that takes banked verdicts DOWN is either fixed or its seat comes out of the train. The
  coordinator offered accept-and-name for a `reflect` regression on the reasoning that its root was
  a model question too large to answer under a landing deadline, and withdrew it: **388 verdicts
  falling to 167 reported is the roster going backwards, and no elegance of root makes that
  landable.** **A SEAT THAT CHANGES A FAILURE'S MODE FROM CAUGHT TO UNCATCHABLE IS A REGRESSION even
  though it created no new defect** — pre-seat the same operation already failed while the package
  still reported every verdict; post-seat the host dies and 221 rows go EMPTY — and reading the two
  trees honestly is what SHRINKS the fix: the ask is not "make the operation work" (the model
  question) but "make it fail the way it failed before", refuse by name, catchable, the defect left
  open and LOUD. **Compare the failure MODES across the two trees before sizing a fix against the
  failure itself.** And the arm that names the seat is cheaper than it looks: **bisect the TRAIN
  before reasoning about its seats.** Building an alternative assembly to cost a seat-drop
  incidentally produced every intermediate SHA, so the ladder over the known merge order — master,
  ten seats, eleven, thirteen, fifteen, sixteen — was a checkout and a run per rung and converged
  exactly. The measurement most needed was the cheapest available and it was run LAST, after an hour
  spent building an alternative to a misdiagnosed problem; **the intermediates already exist.**
  ⚠ **A SEAT WHOSE ACCEPTANCE IS A MEASUREMENT DOES NOT LAND ON A SUBSTITUTE GATE BECAUSE THE
  SUBSTITUTE IS GREEN** (coordinator, 2026-09-07 20:03). Three host-fatal entries were landed on the
  converter suite while the seat's own STATED acceptance — the runtime row's reading — was still
  pending, on the reasoning that skip entries "can only make a row run further". They made it REFUSE:
  `hostFatalMintViolations` (`testConversion.go`) globs EVERY proof page and matches disclosed names by
  BARE NAME with no package scoping, so runtime's `TestEmptyString` is refused because
  `encoding/json`'s passes — the row went from "runs to index 104" to "refuses at mint in 0.16 s",
  measurable → unmeasurable, the regression class this file names. The lane bounded it (19 of runtime's
  525 names collide, 12 of them the `TestSmhasher*` family; one of the four entries) and refused to
  report a count off a run that never ran. **When the acceptance is NAMED, the landing waits for it;
  and a guard that keys on a NAME across a corpus of 28,145 names needs the SCOPE that makes the name
  unique** — here the disclosing package's own page.
- **⚠ TRAIN-ASSEMBLY MECHANICS, four traps in one assembly (2026-09-03).** A seat cut off an OLDER
  base conflicted on SIX roster blocks, not the four a `head -12` grep showed — the filtered-status
  trap in a grep costume — and a resolver asserting `len == 4` bailed BEFORE writing while the
  `git add` chained after it with `;` staged marker-bearing files (the `;`-vs-`&&` rule again, caught
  before commit by the roster guard reading the HEAD side). The resolution rule that worked: **take
  the RULED side's prose for every conflicted block and re-derive only the numbers, then let the
  guard-as-calculator confirm.** A script trap beside it: `${X:-default}` treats an EMPTY env override
  as UNSET, so blanking a seat by env cannot skip it — a seat script carries an explicit
  already-seated list.
  ⚠ **A TRAIN'S MERGES ARE REHEARSED IN A THROWAWAY WORKTREE AT THE LANDED MASTER BEFORE THE ASSEMBLY
  RUNS** (2026-09-04). A pairwise three-way against master cannot see SEAT-VERSUS-SEAT collisions —
  two seats appending to one test file, two to one record — and the old-form `merge-tree` PREFIXES its
  markers, so a line-anchored marker count reads ZERO: the sequential rehearsal named FIVE conflicts
  where one was expected. **Each resolution is VERIFIED before it is saved** — a both-kept resolution
  of a CODE block is proven by `gofmt`, a build, and a COUNT of the symbols both sides own, never by
  the absence of markers: one both-kept resolution SPLIT a function, and a failed three-way apply left
  a clean file MISSING one side's function behind a green `gofmt`, caught only by the 2-of-3 symbol
  count (and the exit printed for `vet` was a pipe's). The saved resolutions are then applied
  mechanically at assembly and stamped PRE-RESOLVED, so the assembly meets no surprise and the hand
  work happened where a mistake cost nothing.
  ⚠ **A REHEARSAL PINNED AT A SHA EXPIRES THE MOMENT A TRAIN LANDS** (measured 2026-09-07): a rehearse
  copy carried its master SHA as a literal and, run after the next train had landed, reported a seat as
  ONE REAL CONFLICT on a record file — a STALE REF, not a conflict, since the pairwise 3-way against
  the landed master merged byte-identical to the seat. **A rehearsal PRINTS the master it rehearsed
  onto in its first line, and a per-run copy re-derives that SHA from `ls-remote` rather than
  inheriting the previous copy's literal.** ⚠ **And a CHAINED SEAT'S BASE IS ITS PREDECESSOR, NOT
  MASTER**: a rehearsal taking every seat's merge-base against master reads a seat cut on top of an
  earlier seat as a conflict on the earlier seat's own appends (seat 4 on seat 2, one false hunk). The
  base is the NEAREST common ancestor of the seat with master AND every seat already folded — the
  maximal candidate, with NO SINGLE MAXIMAL BASE stamped rather than silently resolved — and the stamp
  says CHAINED, so the reader knows which base the verdict used.
  ⚠ **A guard that pins entries BY EXACT KEY is a seam every later seat's entries cross**
  (2026-09-04): one train's amendment made a registry guard per-entry — an unpinned key fails outright
  — while a sibling seat's twelve entries had been cut before it under the older suffix rule, so each
  branch was green ALONE and the union RED on the first unpinned key by name: the crypto/tls merge
  shape in a converter test rather than in a corpus row. The rehearsal caught it; the fix is a UNION
  commit (the seats' entries pinned under the stricter rule, stamped as the train's own), and **a seat
  that adds keyed entries checks the UNION's rule for that map, not its base's.** Two instrument notes
  from the same hour: a resolution that replaces a WHOLE FILE with one branch's version silently drops
  every OTHER seat's change to it (master plus each seat's patch is the shape), and a bare `grep -c`
  of a symbol that must read N is the check that caught a clean `gofmt` hiding a missing function.
  ⚠ **A SEATED-BUT-UNLANDED BRANCH IS INVISIBLE TO EVERY CHECK A LANE CAN RUN, and that is the
  coordinator's debt, not the lane's** (2026-09-06). The assembly is local to the coordinator's machine
  BY DESIGN, so a lane censusing DELIVERABLE PRESENCE at master — the correct instrument, since a merged
  branch's ref is pruned and a surviving ref is the tell — correctly reads five of its own branches as
  ABSENT while they are ancestors of a live assembly head. The lane's three right moves were to census
  rather than guess, ask rather than re-offer, and NAME what it could not see; the coordinator's
  obligations are three. **Put the SEAT LEDGER's contents into status posts BY NAME** rather than
  leaving them in private notes. **"Specified and queued" means nothing if the QUEUE IS LOCAL** — an
  instrument was queued with predictions on record, in a file on the coordinator's machine that no lane
  can read, and the lane that could not find the specification derived its own, which cost it time and
  produced a BETTER instrument: send the artifact, or say it does not exist where they stand. And
  **when a lane asks for a REF instead of a description, PUBLISH one** — a read-only, transient
  reference branch with nothing based on it, nothing merged into it, deleted at landing — rather than
  answering with a caveat the lane cannot check; then answer "which commits touch my subject" by
  MEASURING it (exactly one commit of twenty-one touched the row's own trees, it was the one the
  dispatch was about, and the nearest reachable approximation would have been contaminated in precisely
  that place). The lane-side consequence: **a row's current NUMBER can depend on an UNLANDED seat, so a
  measurement at master answers about a DIFFERENT row** — assert the seat is an ANCESTOR of the
  measurement tree before running, build the converter from that tree, and check the binary's mtime
  MOVED rather than merely existing, three separate ways of not fooling yourself. Such a row's
  improvement ARRIVES WITH ITS TRAIN and belongs in the objective's arithmetic, not in a lane's memory.
  ⚠ **AN ALTERNATIVE THAT MUST BE COSTED IS BUILT, NOT DESCRIBED — and a coordinator's OWN assembly
  commits are ungated by construction** (2026-09-06, one held train). **A file-overlap census
  predicts where conflicts are POSSIBLE; only an assembly says where they ARE.** A seat-drop was
  sized by diffing each commit's file list against the dropped seat's two named collision points (a
  registry and a golden) and NEITHER conflicted, while a file the census had attributed to a
  different seat did — a conflict is a property of what the three-way BASE looks like once a seat is
  absent, not of which files a commit touches. The real assembly came in CHEAPER than the estimate
  (16 merges, ONE trivial resolution where the seat's side was a strict superset, zero markers) and
  it is the only thing that could have found the actual collision. The other half is the
  coordinator's own: a hand-own cut by a sub-agent and merged as an ASSEMBLY commit carried a
  CONVERTER registry change, so its blast radius was never the two corpus files it edited and it
  emptied 221 verdicts in a package two subsystems away — while four hours were spent blaming a
  lane's seat. A lane's seat arrives with its own gate lines; an assembly commit arrives with none
  and the battery that follows attributes to the whole train, so **an assembly commit carrying a
  converter change owes the same measured blast radius as a seat, taken BEFORE it is seated** — or
  it is not an assembly commit, it is an unmeasured seat wearing the coordinator's authority.
  ⚠ **THREE LAND-SCRIPT MECHANICS FROM ONE DERIVE (2026-09-07).** (a) **A guard that greps ITSELF
  through a RELATIVE `${BASH_SOURCE[0]}` after the script has `cd`-ed elsewhere reads EMPTY, and
  reporting that empty as "drift" is a guard mis-naming its own fault**: a land script refused a clean
  landing with "the landing census is not the assembly census" when all four of its own patterns read
  blank (22:17). Resolve the self path ABSOLUTE before any `cd`, and make an empty self-read refuse
  under its TRUE name — instrument fault — never under the finding's name. (b) **Merge arithmetic is
  FIRST-PARENT**: `rev-list --merges` reads 4 for a seat whose own history carries 3 merges, so the
  gate is `--merges --first-parent == 1`, with `all == first-parent + internal` closed as the second
  derivation. (c) **A LIVE gate keyed on the converter BINARY reads FREE hundreds of times during a CNR
  that holds the slot** (one short-lived `go2cs.exe` per package) — take THREE readings (the binary
  token; the harness HOSTS by command line, age-filtered so the census cannot self-match; battery-log
  freshness while the DONE stamp is absent) and refuse on any. And **a prune keyed on a branch name
  GIVEN rather than READ from `ls-remote` reports "already pruned" over a LIVE branch** — one word of
  difference in the name was enough.
  ⚠ **(d) THE SAME SELF-PATH DEFECT, MET A SECOND TIME, AND TWO NEW HALVES** (2026-09-08): a battery
  leg asking "are the six legs wired in this script" grepped its own RELATIVE path from inside the
  assembly worktree, reported 0 of 6, and set the whole battery's failure flag over legs that were
  unaffected — **an instrument fault, not a wiring fault** — re-measured standalone by ABSOLUTE path
  (6 of 6, control 5 of 6) and recorded beside the log. Beyond "resolve the self-path before any
  `cd`": **a self-check that runs from the LAUNCH cwd certifies the LAUNCH cwd**, so it must run the
  check from the cwd the real run uses AFTER its own `cd`. And **the landing takes ONE NARROWLY
  CONDITIONED path** — exactly that refusal, the standalone file present with its control, everything
  else intact — never a general override. Beside it: **a lane's accepted zero is stated WITH ITS
  POPULATION**, so a train does not assemble on a broader reading than the arms support.
