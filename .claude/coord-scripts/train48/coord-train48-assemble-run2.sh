#!/bin/bash
# ============================================================================================
# TRAIN 48 -- A **TEMPLATE**.  EIGHTEEN SEAT ROWS, ALL EIGHTEEN PINNED AT ORIGIN, A BASE THAT RESOLVES
# AT LAUNCH AND A CONTAINMENT PIN FILLED WITH TRAIN 47'S LANDING.  DERIVED 2026-09-13 BY t48-derive.py FROM
# coord-train47-assemble.sh (sha256 e03178b96d31d162588acd75fd4f42854b7f6f2c8b698a62e948a08d56e89734)
# AFTER TRAIN 47 LANDED (31fe4925d, run 8).  ⚠ IF THAT FILE CHANGES BEFORE TRAIN 48 ASSEMBLES, RE-RUN THE
# DERIVE FROM FRESH COPIES RATHER THAN PATCHING THIS ONE: a template hand-patched away from its
# parent is a template nobody can re-derive.
# ⚠ THE TRAIN-47 HEADER'S OWN PROVENANCE LINE READ "DERIVED FROM coord-train47-assemble.sh", which is
#   this file's ancestor naming ITSELF: that derive's global coord-train46->coord-train47 rename hit
#   its own provenance sentence.  It is fixed here rather than inherited, and named so the next derive
#   knows to look: a provenance line inside the scope of the rename that produced it cannot be trusted.
#
# ⚠⚠ **THERE ARE FOUR FILL POINTS AND EVERY ONE OF THEM REFUSES UNTIL IT IS FILLED.**  A template
#   that runs with placeholders in it is a battery measuring a tree nobody named, so each is a
#   REFUSAL rather than a default:
#     (F1) `WT=` -- the assembly worktree.  Reads `PENDING`; the run ABORTS (exit 2) until the
#          coordinator names the worktree this train assembles in.  ⚠ IT IS NOT INHERITED FROM THE
#          PREVIOUS TRAIN: a script pointing at a neighbour's worktree would run inside a tree
#          another battery may be holding, and "verify-only" describes a GIT effect, never an effect
#          on a neighbour.
#     (F2) `T48_CONTAIN_PIN=` -- the CONTAINMENT pin.  **FILLED BY THE DERIVE, 2026-09-13**: train 47
#          LANDED as 31fe4925d055537dbb48c343f726e027631f6aa1 (run 8, 15 seats, base a02ac3df3), so the
#          SHA that did not exist when this template was first written now does.  The FILL-POINT
#          PREFLIGHT still ABORTS (exit 2) on a `PENDING` here, and the fill lives in t48-derive.py
#          rather than in this file -- a pin hand-typed here is undone without trace by the next
#          re-derive.  ⚠ THE BASE ITSELF IS NOT A FILL POINT AND MUST NOT BECOME ONE:
#          `EXPECT_BASE='origin/master'` is the coordinator's standing rule -- the base is
#          origin/master RESOLVED AT LAUNCH, never a literal SHA baked into a gate, because a baked
#          base refuses a HEALTHY launch the moment any lane lands and the operator then reaches for
#          the switch that makes it stop refusing.  What the train actually depends on is
#          CONTAINMENT, and that is the thing asserted.
#     (F3) `SEAT_TABLE` -- eighteen rows.  ALL EIGHTEEN carry a resolved SHA re-read at origin row by
#          row with `git ls-remote` (rows 1-10 on 2026-09-13 after train 47 landed; rows 11-18 on
#          2026-09-13 EVENING from the coordinator's fixed table, ruling 817f98813 + C1 aaa41c087,
#          AS CORRECTED BY THE ADVERSARIAL VERIFY AND THE COORDINATOR'S 19:05 RULINGS D1-D4: row 17
#          re-pinned to its origin tip, rows 11 and 13 SWAPPED so the declared stack merges second,
#          row 4 moved to SHA mode at its existing pin, and the row that duplicated row 6 DROPPED --
#          which is why this table is EIGHTEEN rows and not nineteen);
#          ZERO read `PENDING`.  A PENDING row ABORTS by default and is skipped only under
#          TRAIN48_SKIP_PENDING=1 BY NAME; TRAIN48_REQUIRE_ALL=1 is the LANDING shape and refuses one
#          outright.  The CLASS column of a placeholder row may read `PENDING-class`, which is
#          admissible ONLY while that row's SHA is PENDING -- a filled row carrying it is REFUSED,
#          because a class is what decides the shape a seat is held to.
#     (F4) `EXPECT_G3=` -- G3's census expectation.  **FILLED BY THE DERIVE, 2026-09-13**: `204`, the
#          figure train 47's own G3 MEASURED at the tree that landed (run 8: all four census sets agree
#          at 204), and nothing the census counts -- a README badge, a docs/validation proof page, a
#          roster row, a *.tests.csproj -- has moved on master since.  The PREFLIGHT still ABORTS on a
#          `PENDING` here.  ⚠ IT IS A FILL POINT
#          RATHER THAN A LITERAL because roster rows move between trains: train 47 carried `204`
#          spelled into the arm, and a figure spelled into a gate is the stale-figure class one
#          landing away from refusing a healthy battery (or, worse, from agreeing by coincidence).
#
# ⚠⚠ **THE OPTIONAL FIELDS ARE `key=value` PAIRS FROM FIELD SIX ON, ORDER-FREE, AND THERE ARE TWO.**
#   `allowed=<ERE>`   **A PER-ROW RULING OVER THE WHOLE SEAT, NOT ONLY OVER A7 (COORD 2026-09-13
#                     19:05, extending the 18:20 ruling on G 7a947650f).**  It admits EXACTLY the
#                     named paths OUTSIDE the class's shape AND outside its forbidden list, and it
#                     admits them NOWHERE ELSE.  ⚠ TRAIN 47 CONSULTED IT IN A7 ALONE: merge_seat's
#                     forbidden arm and shape arm read the CLASS and nothing else, so a row whose
#                     ruled path was forbidden or out of shape ABORTED at assembly with a message
#                     about a class, and the ruling written at the row read as decoration.  Both
#                     readings now SUBTRACT the ruled paths, the subtraction is COUNTED AND LISTED in
#                     a stamp (`allowedByRuling=N` plus the paths) rather than hidden, a ruled path
#                     the seat does not actually touch is a STALE RULING and ABORTS BY NAME, and
#                     immediately after the merge every ruled path is asserted BLOB-IDENTICAL between
#                     the union and that seat's own tree -- so the exemption carries nothing another
#                     seat rides in on.  A7 keeps its own scope (src/tests/Behavioral,
#                     --diff-filter=MD) unchanged; arm A7b beside it asserts the same identity over
#                     every ruled path that is NOT behavioral, where the OWNER of a path ruled by two
#                     rows is the LAST row in merge order (which is why a shared ruling needs a
#                     declared `stack-on=`).
#   `stack-on=<row>`  **NEW IN TRAIN 48.**  This row is deliberately built ON another row of this
#                     table.  Git merges the shared commit ONCE, so the union carries the content
#                     once and the hazard is the RECORD rather than the tree -- but the patch-id
#                     census below cannot tell a declared stack from a cherry-pick CONTAMINATION by
#                     shape, so the declaration is what discriminates them.  The parser REFUSES a
#                     stack-on that names a MISSING row, a row that merges AFTER it, or ITSELF.
#   ⚠ AN UNKNOWN KEY IS A REFUSAL, NOT A WARNING, AND THAT IS WHAT CATCHES THE TABLE'S ONE REAL
#     ENCODING HAZARD: an `allowed=` ERE carrying a `|` ALTERNATION splits into extra fields here.
#     Refusing the unknown key names it; silently keeping field six would truncate the ruling and run
#     a narrower exemption than the coordinator wrote.
#
# ⚠⚠ WHAT THIS DERIVE MECHANISED, AND THE TRAIN-46 RUN RECORD THAT MOTIVATED EACH.  Every one is a
#   defect the seven train-46 launches exposed; six of those launches refused or were killed on an
#   INSTRUMENT fault rather than on a finding about a tree.
#   (L1) **THE SEAT COUNT IS DERIVED, NEVER A LITERAL.**  run 1 (15:38:47) merged all six seats and
#        then refused: `the table holds 6 row(s) where this train's derived seat set is FIVE`.  The
#        literal was a fact about the derive, not about the train.  Here the count comes from the
#        table at run time, and what is ASSERTED instead is STRUCTURAL and cannot be satisfied by
#        restating itself: the row numbers are 1..N contiguous with no duplicate, every ref and every
#        filled SHA is unique, the pre-loop and post-loop row counts agree, and the loop accounted
#        for every listed row.  The self-check refuses any surviving `[ "$SEATS_LISTED" = "<n>" ]`.
#   (L2) **A7's ALLOWED SET IS A TABLE FIELD.**  run 2 (15:42:03) refused on a seat that legitimately
#        modified a LANDED guard under a coordinator ruling; the fix was hand-written into A7 as a
#        named project and a named seat variable.  See the seventh column above.
#   (L3) **G4's EXPECTATION IS DERIVED FROM THE SEAT CLASSES.**  run 3 (15:45:15) refused with
#        `CLAUDE.md is UNTOUCHED ... the doctrine row IS seated` -- train 45's premise, carried by
#        the derive into a train with no doctrine seat.  Here G4 asks whether any MERGED row carries
#        class `doctrine`: if one does, CLAUDE.md must be in the delta; if none does, it must not be.
#        BOTH readings are stamped and only the MISMATCH refuses.
#   (L4) **NO `case "${VAR:-0}" in ''` ANYWHERE.**  run 3's G10a printed `deletions=` EMPTY and
#        refused: `${GADEL:-0}` substitutes before the `''` arm can be reached, so the empty case is
#        unreachable and the *value* was never normalised.  Every such site in all five scripts now
#        tests the RAW value; the self-check greps for the shape and requires zero.
#   (L5) **THE REHEARSAL TYPE-CHECKS AFTER EACH SEAT.**  run 4 (15:53) found a UNION-ONLY compile
#        failure at LEG C -- two edits to one `_test.go` that git merged clean by CONTENT -- forty
#        minutes into a battery.  The rehearsal now runs `go vet ./...` in src/go2cs under the
#        CONVERTER BUILD PIN after every seat merge and names the FIRST failing seat, which is where
#        that costs seconds instead.  The UNION FIX mechanism is kept and made **OPTIONAL**: absent
#        patch = no fix, STATED; present patch = its footprint asserted against a declared manifest,
#        `go vet` as its gate, committed as the train's own.
#   (L6) **THE CENSUS TEMPLATE CLASS IS KEPT VERBATIM.**  run 5 (16:04:23) read residual=11 on the
#        security guard's own Sprintf fixtures.  CENSUS_TEMPLATE and the C1/G6T six-arm control are
#        carried BYTE-FOR-BYTE; they are proven, and the self-check requires the placeholder-segment
#        arm to be present.
#   (L7) **EVERY G11 JUSTIFICATION IS DERIVED FROM THE SEAT CLASSES.**  run 6 (16:22:49) read
#        `G11(a) JUSTIFICATION FALSE` because it required `syscall/windows` in the delta -- train
#        45's premise, a directory literal in a train with no syscall seat.  Here the OWED VECTOR is
#        computed from the CLASSES of the rows that merged, stamped as its own line, and every
#        justification arm reads it.  The land script reads that same stamped vector, so the two
#        instruments cannot drift.
#   (L8) LAUNCH, KILL AND CENSUS MECHANICS -- see the LAUNCH SHAPE block below.
#   (L9) `set -u` ONLY, and ZERO `| grep -q` -- see the block below.
#  (L10) EXACT STAMPS, never words -- see the block below.
#  (L11) **EVERY `FAILED=1` NAMES ITS GATE.**  Train 46 carried fifteen setters with no stamp on the
#        same or the previous line; one of them cost a run its attribution.  Every such site is now
#        `fail_gate <GATE>`, which sets the flag AND stamps the gate name, and the self-check refuses
#        any bare `FAILED=1` without a stamp beside it.
#  (L12) THE LAND SCRIPT CARRIES **ZERO** ACCEPTANCE PATHS, and its dry-read asserts that.
#  (L13) **NO SEAT-NUMBER LITERAL MAY CARRY A CLAIM.**  Train 47's file made thirty-eight assertions of
#        the form "seat N is ..." or "seat N's ..." in stamps and in the prose beside gates -- each a
#        fact about THAT train's table, and each one a premise the next derive inherits silently
#        because it reads like documentation rather than like code.  One of them survived into train
#        47's own G11(b) as a QUOTED RETIREMENT -- the sentence that RETIRED a per-row src/gen premise
#        re-spelled that premise inside its own parenthetical, which is the same literal wearing a
#        correction's clothes and is why this rule forbids the SHAPE and not the claim.  Every such
#        premise is now
#        stated in terms of the OWED VECTOR or of the seat's CLASS, and the derive-time self-check's
#        ARM (a) greps for the shape and requires ZERO -- with the train-47 originals as its RED
#        control, where the same predicate reads thirty-eight.  ⚠ THE PREDICATE IS **CASE-INSENSITIVE**,
#        and that is a 2026-09-13 correction rather than a decoration: the case-sensitive form read 26
#        and MISSED eleven more, one of which was a LIVE STAMP inside LEG K claiming a row number for
#        the `sync` gate.  A predicate that cannot see SHOUTED prose is a predicate that reports the
#        loudest instance of the defect as clean.
#  (L14) **THE PATCH-ID ARM: A CHERRY-PICK DUPLICATION CENSUS, RUN BEFORE ANY MERGE.**  Ancestry
#        cannot see a cherry-pick: the same content under a new SHA makes `merge-base --is-ancestor`
#        read false and `git log <base>..<seat>` list a commit nobody recognises, right up until the
#        train lands the same diff twice.  The census tool is READ OUT OF ITS OWN SEAT's pinned blob
#        (so the arm and the seat cannot drift), its OWN `--self-test` is run FIRST as the gate on
#        whether its verdict is worth reading, and the declared stacks come from the table's
#        `stack-on=` fields.  ⚠ AN ARM THAT CANNOT RUN IS **UNMEASURED AND FAILS**: if the census
#        tool's seat is absent from the table or still PENDING, the arm sets FAILED rather than
#        passing over, because an instrument that could not run has not found nothing.
#  (L15) **EVERY VERDICT READ OUT OF A TOOL'S OUTPUT IS AN ANCHORED ROW MATCH.**  A phrase a report
#        prints as a ROW is also a phrase it can print inside a COUNT line, and a substring match
#        cannot tell them apart.  The patch-id arm reads `^==> CENSUS`, `^DUPLICATE patch-id `,
#        `^UNDECLARED STACK: ` and `^SELF-TEST CLEAN -- `; the two toolchain pins read
#        `^go version go<release> ` instead of matching `go<release> ` anywhere in the line.  The
#        self-check's ARM (b) counts what is LEFT unanchored against a written whitelist and refuses
#        a NEW one.
#
# ⚠⚠ WHAT THIS DERIVE **KEPT** FROM TRAIN 46, UNCHANGED, BECAUSE THE RUN RECORDS SHOW IT WORKING.
#   * The TWO-PIN PAIRING and its after-guard.  GOROOT re-exported to the 1.23.12 root with its bin
#     FIRST on PATH and GOTOOLCHAIN UNSET for the corpus/oracle legs (CNR, the behavioral suite, the
#     dial guard, LEG D's RUN half, LEG K); the explicit 1.24.13 root with GOTOOLCHAIN=local for the
#     converter's own build and suites.  Every pin RUNS `go version` AND `go env GOROOT` and ABORTS
#     on either mismatch, because printing a pin is a decoration.
#   * LEG D's CORRECTED seeding control (three arms, both binaries from `git archive`, six seeds from
#     one snapshot, write-evidence per arm per target, the prediction stamped BEFORE the diff and
#     derived with the THREE-DOT form per merged seat, the per-target comparison BY MECHANISM).
#   * LEG 5's OWN CONVERTER REBUILD immediately before the suite and its transpile-predicate stamps
#     before and after -- the zero-`.cs`-newer-than-the-binary assertion that closes route #2's door.
#   * LEG K's DERIVED reflect canary set (controlled both directions before the membership is used),
#     plus `sync` and plus `crypto/internal/nistec` as the descriptor-synthesis COST canary whose
#     WALL is recorded and not judged.
#   * The PRE-RESOLVED merge mechanism: the rehearsal saves a slot per conflicted path, the
#     coordinator fills and verifies it there, and merge_seat applies it MECHANICALLY or refuses.
#   * The six preflight controls C1..C6, the run lock, the disk-floor preflight, ROUTE B for a master
#     held in another worktree, `< /dev/null` on every loop-resident child, processed == listed on
#     every loop, and the freeze announcement stamps.
#
# ⚠ NO PROFILE PATH, ACCOUNT NAME, MACHINE NAME OR UNC PATH APPEARS IN THIS FILE.  The toolchain and
#   runtime roots are DERIVED from $HOME (overridable by name) and ASSERTED to exist; a literal would
#   be both a security-convention breach on any pushed surface and wrong on every other box.  The
#   assembly worktree is a FILL POINT like the base and the seat table, for the same reason plus a
#   sharper one: a train-48 script carrying train 46's worktree path would run inside a tree another
#   battery may be using.
#
# ⚠ `set -u` ONLY -- **NOT** `set -o pipefail`, AND THE REASON IS A MEASUREMENT.  Under pipefail a
#   `grep -q` that exits on its first match SIGPIPEs its producer, the pipeline's status becomes the
#   producer's death, and a TRUE match reads as a failure.  R measured that on this fleet.  This file
#   therefore carries ZERO `| grep -q` pipes: every one is a `grep -c` (or `grep -ac`) plus an
#   INTEGER TEST, which reads the same on a streaming producer and on a bounded one, and the
#   self-check greps for `| grep -q` and for `pipefail` and requires BOTH to read zero.
#
# ⚠ THE EXACT STAMPS A CHAIN OR A WATCHER MAY KEY ON.  They are STAMPS, never WORDS: a waiter keyed
#   on the word `ABORT` fired on an informational line that merely said a PENDING row will ABORT BY
#   NAME, which is prose about a refusal and not a refusal.  Key on these and nothing else:
#       === <label> ASSEMBLE START            the run began
#       === LIGHT GATES DONE                  every cheap gate has reported; the heavy legs follow
#       === A-ASSERTIONS FAILED               the assembled tree is not the tree that was gated
#       === CHAIN STOPPED                     a red COMPILE gate stopped the chain
#       === <label> ASSEMBLE DONE             the run ended, whatever its verdict
#   Every leg additionally stamps `exit=<code>` and a one-line verdict CARRYING ITS ROW COUNT, so a
#   monitor pattern of `exit=[1-9]|UNMEASURED|REFUSED|ABORT|CHAIN STOPPED` sees a red leg, and a leg
#   refused by its own preflight cannot read as a green one.
#
#   LEG / TOOLCHAIN TABLE  (the column that matters is the third)
#     PRE/POST-MERGE pin      1.24.13   master's src/go2cs/go.mod declares it
#     C5 pin control          BOTH      a scratch module declaring go 1.24.13, under GOTOOLCHAIN=local:
#                                       1.24.13 must SUCCEED, 1.23.12 must REFUSE
#     C6 LEG K format control none      pure text; the whole-line oracleGoVersion predicate, 6 arms
#     G1 roster guard         .NET only
#     G2 ps1 parse            .NET only
#     G3 push-nuget -VerifyOnly .NET only
#     G4/G6/G10*/G11/G12      none      text and git only
#     LEG C, LEG Cg, G5, G5b  1.24.13   the converter's own suites, GOTOOLCHAIN=local
#     G7b                     1.23.12   the OLD-PIN REFUSAL control on the real tree; its RED is the pass
#     LEG 0 dial guard        PAIRING
#     LEG 1 integrity x3      .NET only
#     LEG 2 stdlib slnx       .NET only
#     LEG 2b go2cs.slnx       .NET only
#     LEG D build half        1.24.13   both binaries, `go version <exe>` read back off each
#     LEG D run half          PAIRING   six seeded conversions
#     LEG R                   BOTH      converter BUILT at 1.24.13, pipeline RUN at 1.23.12
#     LEG 3 GolibTests x2     .NET only
#     LEG 4 CNR               PAIRING   + the AFTER-GUARD on the produced binary (must read go1.24.13)
#     LEG 5 full suite        PAIRING   + its OWN converter rebuild + the AFTER-GUARD again
#     LEG K canaries + rows   1.23.12   two-pin, -SkipBuild MANDATORY
#
#   THE CLASSES merge_seat IMPLEMENTS.  Each is the TIGHTEST shape its named deliverable needs.  A
#   seat that legitimately needs more means this script is RE-DERIVED, never loosened here: widening
#   a class to admit the seat that violates it is how a gate stops being a gate.
#     doctrine              CLAUDE.md alone (a doctrine batch is a PURE INSERTION; G4 has teeth)
#     docs                  docs/**.md alone
#     manifest              one src/core/<pkg>/go2cs_test_disclosures.json
#     converter-test+docs   converter source + its guards + the projitems + check-roster-format.ps1
#                           + a docs/phase4 record  (the orphan-disclosure check's own shape)
#     converter             converter source + the solution file + the corpus METADATA a converter
#                           change re-mints (package_info.cs, .csproj) + a record
#     converter-guard       converter source + the solution file + a behavioral guard project + the
#                           four MSTest classes + a record
#     golib                 src/core/golib + src/tests/GolibTests + a record
#     golib-gen             golib + the go2cs-gen Templates tree + converter + a behavioral guard
#     golib-corpus-handown  golib + corpus hand-owns + a converter registry + GolibTests + a record
#     golib-converter-docs  golib + converter + GolibTests + a record (+ a docs/phase4 probe tree)
#     converter-test        DERIVED 2026-09-13 for train 48: converter source and its `_test.go`
#                           guards and the projitems, and NOTHING else.  It is `converter-test+docs`
#                           WITHOUT the record and WITHOUT check-roster-format.ps1 -- the tightest
#                           shape a guard-only row needs.  ⚠ A row of this class whose real diff
#                           carries a tool script is RE-CLASSED to `converter-test+tooling` in the
#                           TABLE; widening this shape to admit it is how a gate stops being a gate.
#     converter-test+tooling DERIVED 2026-09-13 for train 48: converter source and its guards PLUS a
#                           repo tool script under src/ (`*.sh`, `*.ps1`, src/utilities) and a record.
#                           It OWES LEG C (the full converter suite) and G2 (the both-edition
#                           PowerShell parse), and the OWED VECTOR carries both.
#     tooling               DERIVED 2026-09-13 for train 48: a repo tool script and a record, and no
#                           converter source at all.  It owes G2 and nothing else -- a PowerShell
#                           change that no leg parses is a change no gate on this train would see.
#     converter-corpus-metadata  DERIVED 2026-09-13 for the row that was UNSEATED later the same day
#                           (seat 6, the former row 8): converter source + the corpus metadata a
#                           converter change re-mints, INCLUDING the package README.md.  `converter`
#                           itself is unchanged beside it -- a new arm, never a widened one.
#                           ⚠ NO ROW OF THIS TRAIN'S TABLE USES IT.  It is left implemented because an
#                           arm nobody rides is inert and removing one would be a second change.
#     docs-data             DERIVED 2026-09-13 for row 5: docs/**.{md,txt}.  `docs` is unchanged beside
#                           it; only the extension moves, and only for the row that was ruled to carry
#                           a data file.  The forbidden list (`src`) -- the real gate -- is identical.
#   ⚠ `src/go2cs` AS A PATHSPEC DOES NOT MATCH `src/go2cs.slnx`, which is what lets a class forbid the
#     converter DIRECTORY while a seat legitimately registers a project in the SOLUTION file.
#   ⚠ EVERY PATH READ IS TAKEN WITH `-c core.quotepath=false`: a path carrying a non-ASCII glyph comes
#     back octal-escaped and QUOTED otherwise, the shape pattern refuses a correct seat, and the
#     refusal reads like a seat defect rather than an instrument one.
#
#   ============================================================================================
#   THE EXPECTATIONS.  E1'/E2'/E3' carry from train 46 because the PAIRING is unchanged.  ED is LEG
#   D's.  ⚠ EVERY ONE OF THEM IS A RELATION DERIVED AT RUN TIME, NOT A LITERAL: a wrong prediction is
#   then a STAMPED MISS rather than a false red.
#   ============================================================================================
#   (E1')  LEG 4 -- CNR UNDER THE PAIRING READS **ZERO DRIFT**.  exit 0; CHANGED == 0 read TWICE (the
#       git numstat over src/tests/Behavioral AND CNR's own printed CHANGED list); NOT MEASURED == 0;
#       the skip line and the verdict line's own note BOTH naming the platform-exclusive guards this
#       Windows host cannot measure; advisory warnings == the named healthy baseline.
#       ⚠ A seat that carries a re-baselined golden (an `allowed=` row) moves the CHANGED set BY
#       DESIGN only if the golden did NOT travel with the seat.  If it did, CNR is predicted
#       byte-identical and a CHANGED reading is a finding.
#   (E2')  LEG 5 -- THE FULL SUITE IS **ALL GREEN**, with the Output pass/skip reconciled against the
#       RUNNER's OWN `--list` enumeration and its OWN MatchConsoleOutput predicate, and the TRANSPILE
#       PREDICATE asserted on the tree BEFORE and AFTER the suite.
#   (E3')  LEG 0 -- the dial guard is GREEN in all four phases.  It is the PAIRING's cheapest positive
#       control and it does NOT stop the chain: LEG 4 and LEG 5 are the set-level instruments.
#   (ED)   LEG D -- ON EACH OF THE THREE TARGETS THE BASE-vs-CUT EMISSION DELTA IS **EXACTLY THE
#       PREDICTED SET**, derived at run time from every MERGED seat's own `git diff <base>...<seat> --
#       src/core` (THREE dots) and compared PER FILE and BY MECHANISM.
#
#   Every refusal exits non-zero and names what it saw.  Every leg stamps its EXIT CODE and a
#   one-line verdict CARRYING ITS ROW COUNT into the assembly log.  A leg refused by its own
#   preflight stamps UNMEASURED.  The chain's exit is the OR of its legs.
#
#   Exit codes: 0 clean | 1 a refusal or a failing leg | 2 wrong worktree / underivable label /
#               a concurrent run holds the lock / an UNFILLED fill point / a PENDING seat without
#               TRAIN48_SKIP_PENDING=1 / a filled seat SHA that does not resolve
#               3 a PREFLIGHT CONTROL failed (broken instrument -- nothing was assembled)
#               90 toolchain pin mismatch | 93 disk floor
#
# ⚠ LAUNCH SHAPE, AND THE THREE MECHANICS THE TRAIN-46 RUNS PAID FOR.
#   (1) **THE LAUNCH WRAPPER MUST END IN `exit $rc`.**  A wrapper whose last statement is a `tail`
#       (or any pipe) reports the LAST command's status, so a script that exited 1 is announced as 0.
#       Capture the real status as the FIRST statement after the run and exit on it:
#           bash coord-train48-assemble-run1.sh > <log> 2>&1; rc=$?; tail -40 <log>; exit $rc
#   (2) **KILL A RUNNING ASSEMBLY BY THE PID THE LOCK FILE NAMES**, never by the harness task's shell
#       and never by executable NAME.  The lock file holds `pid=<n>`; that is an MSYS pid, so map it
#       to the Windows pid with `ps -l` (its WINPID column) before handing it to `taskkill /T /F`.
#       Stopping the task kills the task's SHELL and leaves the chain an orphan writing into the same
#       log -- two chains in one worktree, every reading discarded.
#   (3) **THE CENSUS AFTER A KILL INCLUDES `bash` BY COMMAND LINE AND EXCLUDES THE QUERYING SHELLS BY
#       AGE.**  A census that matches on the executable alone cannot see the chain (it is a bash
#       script), and one that matches on the command line matches ITSELF -- so filter by CreationDate:
#       a real long run is minutes old and the shell performing the check is seconds old.  READ THE
#       LOCK FILE before believing "no survivors": an absent lock and a live pid are different states.
#   (4) **THE SWITCH IS `TRAIN48_REQUIRE_ALL`, AND THE UN-PREFIXED `REQUIRE_ALL` IS REFUSED BY NAME.**
#       `REQUIRE_ALL` is this script's own internal variable and is assigned unconditionally, so a
#       launcher exporting it would configure NOTHING and the run would proceed as a PARTIAL train
#       while reading as a landing.  The preflight below refuses an inherited un-prefixed spelling
#       rather than overwriting it silently.  Write the wrapper as an EXPORT, not as a one-shot
#       prefix, so a `nohup`/`start` layer cannot drop it between the assignment and the child:
#           cp coord-train48-assemble.sh coord-train48-assemble-run1.sh
#           cat > launch-run1.sh <<'EOF'
#           export TRAIN48_REQUIRE_ALL=1
#           bash coord-train48-assemble-run1.sh > <log> 2>&1; rc=$?; tail -40 <log>; exit $rc
#           EOF
#     bash coord-train48-assemble-run1.sh > <a log> 2>&1                        (a PENDING row ABORTS)
#     TRAIN48_SKIP_PENDING=1 bash coord-train48-assemble-run1.sh > <a log> 2>&1  (PENDING rows SKIPPED -- a PARTIAL train)
#     TRAIN48_REQUIRE_ALL=1 bash coord-train48-assemble-run1.sh > <a log> 2>&1   (the LANDING shape)
# ⚠ LAUNCH A **PER-RUN COPY** (coord-train48-assemble-run1.sh).  bash reads a script incrementally BY
# BYTE OFFSET, so an insertion above the running position reparses the next command from the middle of
# a line -- a train assembly died thirty minutes in on exactly that.  The labels derive from the
# basename, so a copy relabels itself and nothing is written out.
# ============================================================================================
set -u

# --- derived identity.  A copy of this script relabels itself; nothing is written out. --------
# ⚠ ABSOLUTE before any cd: G11(c) greps this file AFTER the script has cd'd into the assembly worktree, and a
# relative ${BASH_SOURCE[0]} read "0 of 6 launch lines" on train 44 run 1 (2026-09-08) -- an instrument fault, not a wiring fault.
SCRIPT_PATH="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)/$(basename "${BASH_SOURCE[0]}")"
SCRIPT_BASE="$(basename "$SCRIPT_PATH")"
SCRIPT_DIR="$(cd "$(dirname "$SCRIPT_PATH")" && pwd)"
LABEL="$(printf '%s' "$SCRIPT_BASE" | sed -n 's/^coord-\(train[0-9][0-9]*[a-z]*\).*/\1/p')"
[ -n "$LABEL" ] || { echo "cannot derive a train label from basename '$SCRIPT_BASE' -- ABORT"; exit 2; }
TRAINNUM="$(printf '%s' "$LABEL" | sed 's/[a-z]*$//')"
TAG="t${LABEL#train}"                                   # train44 -> t44   (leg-log prefix)
HEADBR="coord-${TRAINNUM}-head"                         # train44 -> coord-train44-head
RUNID="$(date '+%Y%m%d-%H%M%S')"
ASMLOG="$SCRIPT_DIR/coord-${LABEL}-assemble-${RUNID}.log"

# --- worktree, named in the first lines and no other accepted.  ⚠ **FILL POINT F1.** ----------
#     It reads PENDING and the run ABORTS until the coordinator names the worktree THIS train
#     assembles in.  It is deliberately NOT inherited from the previous train: a train-48 script
#     carrying train 47's worktree would run inside a tree another battery may be holding, and the
#     freeze binds the WORKTREE rather than the branch.  TRAIN48_WT overrides it for a control run,
#     and such a run says so in the stamp -- its verdicts are about this script, never about a train.
WT="${TRAIN48_WT:-PENDING}"
case "$WT" in
  PENDING|''|*PENDING*) echo "WORKTREE UNFILLED :: WT reads [$WT].  This is FILL POINT F1: name the assembly worktree for THIS train (or export TRAIN48_WT for a control run).  A template that runs with a placeholder in it is a battery measuring a tree nobody named -- ABORT"; exit 2 ;;
esac
[ -d "$WT" ] || { echo "WORKTREE [$WT] does not exist -- ABORT"; exit 2; }
WT_TOKEN="$(basename "$WT")"
cd "$WT" || { echo "cannot cd to the assembly worktree [$WT] -- ABORT"; exit 2; }
# ⚠ THE TOPLEVEL EXPECTATION IS DERIVED FROM $WT BY ONE RULE, NEVER WRITTEN TWICE.  msys spells a
#   drive `/c/...` while git answers `C:/...`; cygpath is the tool that knows the mapping.
if command -v cygpath >/dev/null 2>&1; then WT_TOP="$(cygpath -m "$WT" 2>/dev/null)"
else case "$WT" in /?/*) WT_TOP="$(printf '%s' "$WT" | cut -c2 | tr 'a-z' 'A-Z'):$(printf '%s' "$WT" | cut -c3-)" ;; *) WT_TOP="$WT" ;; esac; fi
[ "$(git rev-parse --show-toplevel | tr -d '\r')" = "$WT_TOP" ] || { echo "WRONG WORKTREE: git answers [$(git rev-parse --show-toplevel)] where the derivation from \$WT wants [$WT_TOP]"; exit 2; }

# --- THE TOOLCHAIN AND RUNTIME ROOTS, **DERIVED AND ASSERTED**, NEVER SPELLED. ----------------
# ⚠ A LITERAL PROFILE PATH IS TWO FAULTS AT ONCE: it breaches the security convention the moment any
#   part of this file is quoted onto a pushed surface, and it is wrong on every box but one.  The
#   roots are derived from $HOME (each overridable BY NAME) and then ASSERTED to exist -- because a
#   DERIVED root that is not there is worse than a literal that is: every pin below would then fail
#   with a message about a toolchain rather than about a path.
GO_SDK_ROOT="${GO2CS_SDK_ROOT:-$HOME/sdk}"
PIN_GO_1_23_ROOT="$GO_SDK_ROOT/go1.23.12"
PIN_GO_1_24_ROOT="$GO_SDK_ROOT/go1.24.13"
PIN_DOTNET="${GO2CS_DOTNET_ROOT:-$HOME/dotnet10}"
for _r in "$PIN_GO_1_23_ROOT" "$PIN_GO_1_24_ROOT" "$PIN_DOTNET"; do
  [ -d "$_r" ] || { echo "PIN ROOT MISSING: [$_r] does not exist.  The roots are derived from \$HOME (override with GO2CS_SDK_ROOT / GO2CS_DOTNET_ROOT); a derived root that is not there would make every pin below fail with a message about a toolchain rather than about a path -- ABORT"; exit 90; }
done
[ -x "$PIN_GO_1_23_ROOT/bin/go" ] || [ -x "$PIN_GO_1_23_ROOT/bin/go.exe" ] || { echo "PIN ROOT [$PIN_GO_1_23_ROOT] has no bin/go -- ABORT"; exit 90; }
[ -x "$PIN_GO_1_24_ROOT/bin/go" ] || [ -x "$PIN_GO_1_24_ROOT/bin/go.exe" ] || { echo "PIN ROOT [$PIN_GO_1_24_ROOT] has no bin/go -- ABORT"; exit 90; }
LAUNCH_PATH="$PATH"
strip_pins(){ # $1 = a PATH string -> the same PATH with the three pin dirs removed
  local out="" p
  local OLDIFS="$IFS"; IFS=':'
  for p in $1; do
    case "$p" in
      "$PIN_GO_1_23_ROOT/bin"|"$PIN_GO_1_24_ROOT/bin"|"$PIN_DOTNET") continue ;;
    esac
    out="${out:+$out:}$p"
  done
  IFS="$OLDIFS"
  printf '%s' "$out"
}
ORIG_PATH="$(strip_pins "$LAUNCH_PATH")"
PIN_COMPONENTS_REMOVED=$(( $(printf '%s' "$LAUNCH_PATH" | tr ':' '\n' | grep -ac .) - $(printf '%s' "$ORIG_PATH" | tr ':' '\n' | grep -ac .) ))

# ⚠ DOTNET_ROOT is the WINDOWS form of the SAME derived root -- `cygpath -w` knows the mapping
#   and the hand rule is only the fallback for a shell without it.  Two spellings of one root, never
#   two roots: MSBuild materialises environment variables as properties and resolves property names
#   case-INSENSITIVELY, which is how one POSIX block once carried two entries that folded into one.
if command -v cygpath >/dev/null 2>&1; then DOTNET_ROOT_WIN=$(cygpath -w "$PIN_DOTNET"); else DOTNET_ROOT_WIN="$PIN_DOTNET"; fi
export DOTNET_ROOT="$DOTNET_ROOT_WIN" MSBUILDDISABLENODEREUSE=1 CGO_ENABLED=0
export PATH="$PIN_DOTNET:$ORIG_PATH"

# The base, the ORDER pin, the CONTAINMENT pin and G3's expectation.
# ⚠ **FILL POINTS F2 AND F4.**  T48_CONTAIN_PIN is the SHA train 47 lands and it does NOT EXIST at
#   derive time -- train 47 was still in flight when this template was written.  It reads PENDING and
#   the FILL-POINT PREFLIGHT below ABORTS until it is filled; once filled, the BASE ANCESTRY loop
#   asserts that origin/master CONTAINS it.
# ⚠ EXPECT_BASE IS **NOT** A FILL POINT AND MUST NOT BECOME ONE.  The base is origin/master RESOLVED
#   AT LAUNCH.  A baked base SHA refuses a HEALTHY launch the moment any lane lands, and the operator
#   then reaches for the switch that makes it stop refusing; what the train depends on is CONTAINMENT
#   of the pin above, and that is what is asserted.  The PENDING refusal in the BASE ASSERTION below
#   is kept intact for the case where a later coordinator does spell a SHA here.
# ⚠ EXPECT_ORDER is the ORDER pin: a LANDED SHA that was already an ancestor of origin/master when
#   this was derived, so it cannot false-red.  a02ac3df3 is train 47's own base -- measured, not
#   assumed: `git ls-remote origin refs/heads/master` read a02ac3df3 on 2026-09-13.
EXPECT_ORDER='a02ac3df3'        # train 47's base -- the ORDER pin (a landed, known ancestor)
EXPECT_BASE='origin/master'     # this train's BASE: origin/master AS RESOLVED AT LAUNCH
T48_CONTAIN_PIN='31fe4925d055537dbb48c343f726e027631f6aa1'   # (F2) FILLED 2026-09-13: the SHA train 47 LANDED (run 8, 15 seats, base a02ac3df3, tree 161af6c44)
EXPECT_G3='204'                 # (F4) FILLED 2026-09-13: MEASURED by train 47 run 8's own G3 at the tree that landed; nothing the census counts has moved on master since

stamp(){ echo "[$(date '+%F %T')] $*" | tee -a "$ASMLOG"; }
FAILED=0
# --- ⚠ EVERY `FAILED=1` NAMES ITS GATE.  Train 46 carried FIFTEEN setters with no stamp on the
#     same or the previous line, and a reader scanning for the flag could not tell which gate had
#     set it; one of them cost a run its attribution.  A setter that names its gate costs one word.
fail_gate(){ FAILED=1; stamp "  ^ FAILED=1 set by gate [$1]"; }

# --- ⚠ **THE FILL-POINT PREFLIGHT.**  F2 and F4 refuse HERE -- at launch, before the run lock is
#     taken and before a single seat is merged.  The BASE ASSERTION and G3 each read their own value
#     again later, but a template whose placeholder is only met three hundred lines and one lock into
#     the run has already cost the operator the thing a refusal is for.  ⚠ EACH REFUSAL NAMES ITS OWN
#     FILL POINT: "a gate refused" and "a gate could not be configured" are different states.
FILLBAD=0
case "$T48_CONTAIN_PIN" in
  PENDING|''|*PENDING*)
    stamp "  ^ FILL POINT F2 REFUSED: the containment pin is not filled (T48_CONTAIN_PIN reads [$T48_CONTAIN_PIN]).  It is the SHA TRAIN 47 LANDED, which did not exist when this template was derived.  Fill it: the base of this train is origin/master resolved at launch, and the CONTAINMENT of that SHA is the only statement this script makes about the base's CONTENT."
    FILLBAD=1 ;;
  *) : ;;
esac
case "$EXPECT_G3" in
  PENDING|''|*PENDING*)
    stamp "  ^ FILL POINT F4 REFUSED: G3's census expectation is not filled (EXPECT_G3 reads [$EXPECT_G3]).  Roster rows MOVE between trains, so the figure is named per train rather than spelled into the arm; a stale figure there refuses a healthy battery or agrees with it by coincidence, and the two are indistinguishable from the log."
    FILLBAD=1 ;;
  *) : ;;
esac
[ "$FILLBAD" = "0" ] || { stamp "FILL POINTS UNFILLED -- NOTHING has been locked, fetched or merged -- ABORT"; exit 2; }
stamp "FILL POINTS OK :: F1 worktree=[$WT] :: F2 containment pin=[$T48_CONTAIN_PIN] :: F4 G3 census expectation=[$EXPECT_G3] (F3, the seat table, is asserted at the SEAT TABLE block below)"

# --- run lock (see the header) ----------------------------------------------------------------
LOCK="/tmp/${TAG}-assemble.lock"
if ! ( set -o noclobber; echo "pid=$$ started=$(date '+%F %T') script=$SCRIPT_BASE" > "$LOCK" ) 2>/dev/null; then
  echo "LOCK HELD: $LOCK says [$(cat "$LOCK" 2>/dev/null)]"
  echo "another ${LABEL} assembly is running (or one was killed hard).  If it is genuinely dead:  rm -f $LOCK"
  exit 2
fi
trap 'rm -f "$LOCK"' EXIT

# ============================================================================================
# THE CENSUS PATTERN SET.  ⚠ COPIED BYTE-FOR-BYTE from coord-train43-assemble.sh and BYTE-COMPARED
# against it by coord-train44-derive-selfcheck.sh section 2.  Do NOT retype these through a shell
# command string OR a heredoc: the doubled backslashes collapse and the census reports every excluded
# line as a residual.  This derive measured that collapse in its own probe before it reached this file.
# ============================================================================================
CENSUS_DETECT='users[\\/]|/home/|\\\\\\\\|'"$(basename "$HOME")"''   # SCRUBBED: the account name is derived from the environment (see README.md)
CENSUS_EXCLUDE='\[\\+/\]|\[/\\+\]|github\\?\.com/ritchiecarroll/go2cs'
CENSUS_PLACEHOLDER='[Uu]sers[\\/]+(<[^>]{0,40}>|user)([\\/]|$)'
CENSUS_HARD='/home/|\\\\\\\\|'"$(basename "$HOME")"''   # SCRUBBED: the account name is derived from the environment (see README.md)
CENSUS_TMP="/tmp/${TAG}-census.tmp"
CENSUS_KIND_USERS='users[\\/]'
CENSUS_KIND_HOME='/home/'
CENSUS_KIND_UNC='\\\\\\\\'
CENSUS_KIND_NAME=''"$(basename "$HOME")"''   # SCRUBBED: the account name is derived from the environment (see README.md)
# TEMPLATE CLASS (coordinator, 2026-09-08 16:15; run 5 read residual=11 on it, all in the security guard's OWN
#   test file): a hit whose SEGMENT is a Sprintf placeholder (percent-s or percent-q, possibly after the source-text
#   escapes backslash-n / backslash-t or spaces that the guard's split-line fixtures carry) or the literal placeholder
#   account user/go, plus the detector's own quoted literals.  Keyed on the SEGMENT CLASS, never on the path -- the
#   guard's source is never exempted by name (doctrine), so a fixture carrying a REAL segment still counts.  Excluded
#   lines are PRINTED beside the residual (templateExcluded=N) and the class is controlled at C1/G6T (six arms).
CENSUS_TEMPLATE='(/home/|[Uu]sers(\\\\|/)|\\\\\\\\)(\\n|\\t| )*(%s|%q|user/go)|\[\]byte\("/home/"\)|"users"\)'

census_lines(){ grep -aE '^\+' | grep -aiE "$CENSUS_DETECT"; }
census_raw(){   census_lines | grep -ac . || true; }
census_residual_lines(){
  census_lines | grep -avE "$CENSUS_EXCLUDE" > "$CENSUS_TMP"
  grep -aiE  "$CENSUS_HARD" "$CENSUS_TMP" 2>/dev/null | grep -avE "$CENSUS_TEMPLATE"
  grep -aivE "$CENSUS_HARD" "$CENSUS_TMP" 2>/dev/null | grep -avE "$CENSUS_PLACEHOLDER" | grep -avE "$CENSUS_TEMPLATE"
  rm -f "$CENSUS_TMP"
}
census_template_lines(){ census_lines | grep -avE "$CENSUS_EXCLUDE" | grep -aE "$CENSUS_TEMPLATE"; }
census_template(){ census_template_lines | grep -ac . || true; }
census_residual(){ census_residual_lines | grep -ac . || true; }
census_kinds(){
  local k="/tmp/${TAG}-census-kinds.tmp"
  cat > "$k"
  local tot; tot=$(grep -ac . "$k" || true)
  stamp "      KIND HISTOGRAM over $tot residual line(s) -- which DETECTOR alternative fired:"
  stamp "        users-path        : $(grep -aicE "$CENSUS_KIND_USERS" "$k" || true)"
  stamp "        /home/            : $(grep -aicE "$CENSUS_KIND_HOME"  "$k" || true)"
  stamp "        four-backslash UNC: $(grep -aicE "$CENSUS_KIND_UNC"   "$k" || true)"
  stamp "        owner account name: $(grep -aicE "$CENSUS_KIND_NAME"  "$k" || true)"
  stamp "      (a line may fire more than one alternative, so these need not sum to $tot; a SUM OF"
  stamp "       ZERO with a non-zero residual means the histogram's four patterns have drifted from"
  stamp "       CENSUS_DETECT -- read that as an instrument fault, not as a finding)"
  rm -f "$k"
}

# --- CR/LF byte probe.  ONE definition; C4 controls it BOTH ways. -----------------------------
cr_bytes(){ tr -cd '\r' < "$1" | wc -c; }
lf_lines(){ wc -l < "$1"; }

# --- the ps1 parse form.  ONE definition, shared by the C2 control and G2 itself. -------------
parse_ps1(){ # $1 = edition, $2 = windows path, $3 = log to append
  local ed="$1" winpath="$2" log="$3" cmd
  cmd="\$t=\$null;\$e=\$null;[void][System.Management.Automation.Language.Parser]::ParseFile('${winpath}',[ref]\$t,[ref]\$e); if(\$e.Count){\$e|%{\$_.Message};exit 1}else{exit 0}"
  case "$ed" in
    pwsh) env -u DOTNET_ROOT PATH="$ORIG_PATH" pwsh -NoProfile -Command "$cmd" >> "$log" 2>&1 ;;
    *)    "$ed" -NoProfile -Command "$cmd" >> "$log" 2>&1 ;;
  esac
  return $?
}

# --- the live-process census.  BY EXECUTABLE PATH and COMMAND LINE, shells excluded. ----------
proc_count(){ # $1 = token
  powershell -NoProfile -Command "@(Get-CimInstance Win32_Process | Where-Object { \$_.ExecutablePath -like '*$1*' -or \$_.CommandLine -like '*$1*' } | Where-Object { \$_.Name -notin @('bash.exe','powershell.exe','pwsh.exe') }).Count" 2>/dev/null | tr -d '\r'
}

freegb(){ powershell -NoProfile -Command "[int]((Get-PSDrive C).Free/1GB)" 2>/dev/null | tr -d '\r'; }

# --- ONE definition of the behavioral Output verdict read (from coord-train40b-battery.sh).
#     It echoes:  <compared> <failed> <terminal> <summaryPass> <summaryFail>
#     NONE anywhere means the log did not carry that reading at all.  The verdict is read from the
#     COMPARISON COUNT, never from the word PASS: an Output line reading "0 compared, 0 failed" is a
#     project whose comparison never ran (a freshly transpiled package_info.cs loses the hand-added
#     [GoTestMatchingConsoleOutput]).
read_output_verdict(){ # $1 = runner log
  local log="$1" line cmp fail term spass sfail srow
  line=$(tr -d '\r' < "$log" 2>/dev/null | grep -aE '^\[Output\][[:space:]]+running C# vs Go' | tail -1)
  cmp=$( printf '%s' "$line" | grep -aoE '[0-9]+ compared' | grep -aoE '^[0-9]+')
  fail=$(printf '%s' "$line" | grep -aoE '[0-9]+ failed'   | grep -aoE '^[0-9]+')
  term=$(tr -d '\r' < "$log" 2>/dev/null | grep -aoE '^(PASS|FAIL)  \(' | tail -1 | cut -c1-4)
  srow=$(tr -d '\r' < "$log" 2>/dev/null | grep -aE '^[[:space:]]+Output[[:space:]]+pass' | tail -1)
  spass=$(printf '%s' "$srow" | grep -aoE 'pass[[:space:]]+[0-9]+' | grep -aoE '[0-9]+')
  sfail=$(printf '%s' "$srow" | grep -aoE 'fail[[:space:]]+[0-9]+' | grep -aoE '[0-9]+')
  printf '%s %s %s %s %s' "${cmp:-NONE}" "${fail:-NONE}" "${term:-NONE}" "${spass:-NONE}" "${sfail:-NONE}"
}

# --- ONE definition of the SUMMARY PHASE-ROW read.  The runner prints one row per phase:
#         "  <Phase>    pass NNNN   fail NNNN   skip NNNN   timeout NNNN"
#     (BehavioralRunner/Program.cs, the summary writer).  It echoes:  <pass> <fail> <skip> <timeout>
#     with NONE anywhere the row did not carry a reading.
#     ⚠ WHY THIS EXISTS AND WHY IT IS SEPARATE FROM read_output_verdict.  The Output RECONCILIATION
#     asserts pass AND skip, not `compared` alone: the runner moves a project whose COMPILE did not PASS
#     into Output SKIP, so a compile regression that a compared-only check would swallow shows here as
#     pass BELOW the marked count and skip ABOVE the unmarked count.  And the Transpile / Compile /
#     Target rows are what pin the ENUMERATION: each of their pass counts must equal N, so a phase that
#     silently processed fewer projects than the runner listed cannot read green.
#     ⚠ The phase name is matched with a WORD boundary and the row anchored at the leading indent, so a
#     prose line mentioning a phase cannot be read as its row.
read_phase_row(){ # $1 = runner log, $2 = phase name (Transpile|Compile|Target|Output)
  local log="$1" ph="$2" row p f s t
  row=$(tr -d '\r' < "$log" 2>/dev/null | grep -aE "^[[:space:]]+${ph}[[:space:]]+pass[[:space:]]+[0-9]+" | tail -1)
  p=$(printf '%s' "$row" | grep -aoE 'pass[[:space:]]+[0-9]+'    | grep -aoE '[0-9]+')
  f=$(printf '%s' "$row" | grep -aoE 'fail[[:space:]]+[0-9]+'    | grep -aoE '[0-9]+')
  s=$(printf '%s' "$row" | grep -aoE 'skip[[:space:]]+[0-9]+'    | grep -aoE '[0-9]+')
  t=$(printf '%s' "$row" | grep -aoE 'timeout[[:space:]]+[0-9]+' | grep -aoE '[0-9]+')
  printf '%s %s %s %s' "${p:-NONE}" "${f:-NONE}" "${s:-NONE}" "${t:-NONE}"
}

# --- ONE definition of the refusal-marker read.  A leg refused by its own preflight stamps a
#     plausible-looking verdict otherwise; the CLOCK is the other tell, so a wall is stamped beside
#     every leg.
refusal_markers(){ # $1 = log
  tr -d '\r' < "$1" 2>/dev/null | grep -aoE 'DISK PREFLIGHT|No banked packages matched|is not digitally signed|NETSDK1045|MSB4166|Test Run Aborted|test manifest is (stale|missing)|Toolchain pin|Converter not built' | sort -u | tr '\n' ' '
}

# --- ONE definition of the preserved-record naming.  A second copy of the string is the thing that
#     drifts, so every preservation call reads this function.
preserved_path(){ # $1 = package path (slashes), $2 = artifact basename
  printf '%s/coord-%s-preserved-%s-%s-%s' "$SCRIPT_DIR" "$TAG" "$(printf '%s' "$1" | tr '/' '.')" "$RUNID" "$2"
}

# --- THE PER-LEG TOOLCHAIN PIN.  It RUNS go version AND go env GOROOT and ABORTS on either mismatch.
#     GOTOOLCHAIN=local so `auto` cannot silently switch a 1.23.12 shell up to 1.24.13 on the strength
#     of the merged go.mod.  It EXPORTS GOROOT and re-orders PATH, which is i9's clause 1cf3af3635: the
#     -goroot FLAG does not isolate the package loader and the ambient GOROOT leaks in.  Returns
#     non-zero rather than exiting, so the caller decides.
pin_go(){ # $1 = 1.23.12 | 1.24.13   $2 = leg label
  local want="$1" lbl="$2" rootw rootp gv genv
  case "$want" in
    1.23.12) rootp="$PIN_GO_1_23_ROOT" ;;
    1.24.13) rootp="$PIN_GO_1_24_ROOT" ;;
    *) stamp "PIN[$lbl] unknown release '$want' -- ABORT"; return 1 ;;
  esac
  # ⚠ THE WINDOWS FORM IS **DERIVED FROM THE POSIX ROOT**, NEVER SPELLED.  Train 46 carried the two
  #   roots a second time as `C:\Users\<account>\sdk\go1.xx.yy` literals, which is a profile path with
  #   an account name in it -- a security-convention breach the moment any part of this file is quoted
  #   onto a pushed surface, AND a second copy of a fact the POSIX root already holds.  cygpath is the
  #   tool that knows the mapping; the hand rule is only the fallback for a shell without it.
  if command -v cygpath >/dev/null 2>&1; then rootw="$(cygpath -w "$rootp")"
  else case "$rootp" in /?/*) rootw="$(printf '%s' "$rootp" | cut -c2 | tr 'a-z' 'A-Z'):$(printf '%s' "$rootp" | cut -c3- | tr '/' '\\')" ;; *) rootw="$rootp" ;; esac; fi
  if [ ! -x "$rootp/bin/go.exe" ] && [ ! -x "$rootp/bin/go" ]; then
    stamp "PIN[$lbl] the go$want root has no go binary at $rootp/bin -- ABORT"; return 1
  fi
  export GOROOT="$rootw" GOTOOLCHAIN=local
  export PATH="$rootp/bin:$PIN_DOTNET:$ORIG_PATH"
  gv="$(go version 2>&1)"
  case "$gv" in
    # ⚠ AN **ANCHORED ROW MATCH**, not a substring of the line (L15).  `go version` prints one row
    #   beginning `go version go<release> <goos>/<goarch>`; a `*"go$want "*` test also accepts that
    #   token anywhere else on the line, which is the shape that cannot tell a verdict row from a
    #   sentence mentioning the release.
    go\ version\ go"$want"\ *) : ;;
    *) stamp "PIN[$lbl] TOOLCHAIN MISMATCH: want go$want, bare go version says [$gv] -- ABORT"; return 1 ;;
  esac
  # The RIGHT SPELLING OF THE WRONG RELEASE is a real failure mode, so GOROOT is read back and
  # compared as a path rather than trusted from the export.
  genv="$(go env GOROOT 2>&1 | tr -d '\r' | tr '\\' '/')"
  case "$genv" in
    */sdk/go$want) : ;;
    *) stamp "PIN[$lbl] GOROOT MISMATCH: go env GOROOT says [$genv], want a path ending /sdk/go$want -- ABORT"; return 1 ;;
  esac
  stamp "PIN[$lbl] OK :: $gv :: GOROOT=$genv GOTOOLCHAIN=$(go env GOTOOLCHAIN 2>/dev/null | tr -d '\r')"
  return 0
}

# --- THE TWO-PIN **PAIRING**, which is what the CNR and behavioral-suite legs are RUN under.
#     It is a DIFFERENT function from pin_go and not a parameter of it, because the two differ on the
#     one setting that decides whether the shape works at all: pin_go exports GOTOOLCHAIN=local so a
#     1.23.12 shell CANNOT silently switch up (which is what makes LEG K's pin falsifiable), while the
#     pairing must UNSET it so the converter's own module graph CAN switch the BUILD up to 1.24.13
#     while every corpus and behavioral package still LOADS at 1.23.12.  `local` breaks the pairing
#     outright; `auto` would break LEG K.  Two questions, two settings, and neither is a default to
#     inherit.
#     ⚠ THE ASSERTION IS TAKEN FROM A DIRECTORY WITH **NO go.mod**.  Inside src/go2cs both `go version`
#     and `go env GOROOT` report the SWITCHED toolchain under auto (i9 0eef5b66c), so an assertion
#     taken there reads 1.24.13 and is a decoration.  This function REFUSES if the cwd carries a
#     go.mod, rather than trusting that the caller is at the worktree root.
pin_go_pairing(){ # $1 = leg label
  local lbl="$1" rootw rootp gv genv gtc
  rootp="$PIN_GO_1_23_ROOT"
  # ⚠ THE WINDOWS FORM IS DERIVED HERE TOO, for the two reasons pin_go states: a profile path
  #   carrying an account name must not appear in this file, and the POSIX root already holds the
  #   fact.  cygpath knows the mapping; the hand rule is the fallback for a shell without it.
  if command -v cygpath >/dev/null 2>&1; then rootw="$(cygpath -w "$rootp")"
  else case "$rootp" in /?/*) rootw="$(printf '%s' "$rootp" | cut -c2 | tr 'a-z' 'A-Z'):$(printf '%s' "$rootp" | cut -c3- | tr '/' '\\')" ;; *) rootw="$rootp" ;; esac; fi
  if [ ! -x "$rootp/bin/go.exe" ] && [ ! -x "$rootp/bin/go" ]; then
    stamp "PAIRING[$lbl] the go1.23.12 root has no go binary at $rootp/bin -- ABORT"; return 1
  fi
  if [ -f "./go.mod" ]; then
    stamp "PAIRING[$lbl] the current directory carries a go.mod, so a \`go version\` taken here would report the SWITCHED toolchain under auto and the assertion would be a decoration -- ABORT (cwd=$(pwd))"
    return 1
  fi
  export GOROOT="$rootw"
  unset GOTOOLCHAIN
  export PATH="$rootp/bin:$PIN_DOTNET:$ORIG_PATH"
  gv="$(go version 2>&1)"
  case "$gv" in
    # ⚠ ANCHORED ROW MATCH, for the reason pin_go states (L15).
    go\ version\ go1.23.12\ *) : ;;
    *) stamp "PAIRING[$lbl] TOOLCHAIN MISMATCH: want go1.23.12 at a no-module cwd, bare go version says [$gv] -- ABORT"; return 1 ;;
  esac
  genv="$(go env GOROOT 2>&1 | tr -d '\r' | tr '\\' '/')"
  case "$genv" in
    */sdk/go1.23.12) : ;;
    *) stamp "PAIRING[$lbl] GOROOT MISMATCH: go env GOROOT says [$genv], want a path ending /sdk/go1.23.12 -- ABORT"; return 1 ;;
  esac
  gtc="$(go env GOTOOLCHAIN 2>/dev/null | tr -d '\r')"
  case "$gtc" in
    auto) : ;;
    *) stamp "PAIRING[$lbl] GOTOOLCHAIN reads [$gtc], want auto.  \`local\` BREAKS the single-root pairing outright -- the converter cannot be built under the run pin -- so this is a refusal, not a note -- ABORT"; return 1 ;;
  esac
  stamp "PAIRING[$lbl] OK :: $gv :: GOROOT=$genv GOTOOLCHAIN=$gtc cwd=$(pwd) (no go.mod here, which is what makes this assertion mean anything) :: the module graph switches ONLY the converter's BUILD up to 1.24.13; every behavioral and corpus package LOADS at 1.23.12"
  return 0
}

# --- THE PAIRING'S **AFTER-GUARD**.  `go version <exe>` reads the binary's own EMBEDDED release, which
#     no cwd and no GOTOOLCHAIN can switch, so it is the ONE reading that says the BUILD half of the
#     pairing happened.  A binary reading anything but go1.24.13 marks its leg UNMEASURED: the pairing's
#     whole claim is build-up / load-down, and half of that claim is only visible here.
#     Returns 0 when the binary reads go1.24.13; 1 otherwise.  It NEVER exits: the caller decides
#     whether the leg is UNMEASURED or merely stamped.
CONVEXE='src/go2cs/bin/go2cs.exe'
after_guard_converter(){ # $1 = leg label, $2 = "assert" | "stamp"
  local lbl="$1" mode="$2" ver stale
  if [ ! -f "$CONVEXE" ]; then
    stamp "AFTER-GUARD[$lbl] $CONVEXE is ABSENT -- the leg's own converter build did not land a binary where the harness puts it (src/_paths.ps1's Go2csExe)"
    return 1
  fi
  ver="$(go version "$CONVEXE" 2>&1 | tr -d '\r')"
  stale=$(find src/go2cs -name '*.go' -newer "$CONVEXE" 2>/dev/null | grep -ac . || true)
  case "$ver" in
    *go1.24.13*)
      stamp "AFTER-GUARD[$lbl] OK :: \`go version $CONVEXE\` reads [$ver] -- the BUILD half switched up as the module graph requires, while the pairing held the LOAD at 1.23.12.  .go inputs newer than the binary = $stale (the converter's own mtime predicate; 0 expected)"
      return 0 ;;
    *)
      stamp "AFTER-GUARD[$lbl] REFUSED :: \`go version $CONVEXE\` reads [$ver], not go1.24.13.  The binary's embedded release is the one reading no cwd can switch, so this says the BUILD half of the pairing did not happen -- the leg is UNMEASURED, never a pass ($mode)"
      return 1 ;;
  esac
}

stamp "=== ${LABEL} ASSEMBLE START (script=$SCRIPT_BASE run=$RUNID) ==="
stamp "log=$ASMLOG  worktree=$WT  headBranch=$HEADBR  legLogs=/tmp/${TAG}-*.log  legArtifacts=$SCRIPT_DIR/coord-${TAG}-*"
stamp "launch PATH carried a pin: $( [ "$PIN_COMPONENTS_REMOVED" -gt 0 ] && echo "YES ($PIN_COMPONENTS_REMOVED component(s) removed to build the pin-free ORIG_PATH)" || echo "no (0 components removed)" ) -- a READING, never a refusal: this script pins per leg and derives ORIG_PATH by removal, so both launch shapes are correct here"

# ============================================================================================
# PREFLIGHT CONTROLS -- BEFORE anything is fetched or merged.  A broken instrument stops the run;
# it never reports over a hole.
# ============================================================================================
CTLLOG="/tmp/${TAG}-control.log"; : > "$CTLLOG"

# --- C1: G6 planted-line census control -------------------------------------------------------
#     The plant is carried over from train 43 UNCHANGED, including the release-tag source-link row.
#     It is a control of the PATTERN SET, not of this train's delta, and every shape it excludes is a
#     shape the repository really carries; dropping rows because this train has no frozen-page seat
#     would weaken the control for no gain.
PLANT="/tmp/${TAG}-census-planted.txt"; : > "$PLANT"
# must COUNT: a REAL-looking account segment
printf '%s\n' '+C:\Users\plantedname\sdk\go1.23.12'                                         >> "$PLANT"
# must NOT count: regex classes at two backslash depths (prose, and shell source)
printf '%s\n' '+ prose quoting the census class users[\\/] beside a /home/ shape'            >> "$PLANT"
printf '%s\n' '+ shell source: grep -aiE "users[\\\\/]|/home/" over the delta'               >> "$PLANT"
# must NOT count: both spellings of the PUBLIC repo URL
printf '%s\n' '+see https://github.com/ritchiecarroll/go2cs for the public repo'             >> "$PLANT"
printf '%s\n' '+ regex-escaped: https://github\.com/ritchiecarroll/go2cs/tree/nuget-x/src/'  >> "$PLANT"
# must NOT count: the REDACTED placeholder forms
printf '%s\n' '+ redacted windows profile: C:\Users\<user>\sdk\go1.24.13'                    >> "$PLANT"
printf '%s\n' '+ redacted lowercase posix-style: /c/users/<user>/sdk/go1.24.13'              >> "$PLANT"
printf '%s\n' '+ redacted bare-word account: C:\Users\user\go\src'                           >> "$PLANT"
# must COUNT: a placeholder MUST NOT launder a HARD hit sharing its line
printf '%s\n' '+ MIXED: C:\Users\<user>\x beside /home/someone/y'                            >> "$PLANT"
# must NOT count: the roster-row-in-a-table-cell shape
printf '%s\n' '+| [`archive/tar`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/archive/tar) | 97 | | TAR archives · [proof](archive.tar.md) |' >> "$PLANT"
# must NOT count: a frozen proof page's source link, which names the release TAG rather than master
printf '%s\n' '+[`src/core/bytes`](https://github.com/ritchiecarroll/go2cs/tree/nuget-1.23.12.3/src/core/bytes).' >> "$PLANT"
C_RAW=$(census_raw      < "$PLANT")
C_RES=$(census_residual < "$PLANT")
UNC4=$(printf '%s\n' '+ source-quoted unc: "\\\\server\\share\\go2cs"' | census_residual)
UNC2=$(printf '%s\n' '+ bare unc prefix: \\server\share\go2cs'          | census_residual)
stamp "CONTROL C1/G6 planted: detectorHits=$C_RAW (want 11 -- every planted line is SEEN) residual=$C_RES (want 2: the real-segment line and the MIXED line.  The two regex classes, BOTH URL spellings, the URL in a table cell, the RELEASE-TAG source link and all three redacted placeholders must not)"
stamp "CONTROL C1/G6 sub-check: the four-backslash alternative fires on a source-quoted UNC line = $UNC4 (want 1)"
stamp "CONTROL C1/G6 REPORTED (not asserted): a BARE two-backslash UNC prefix reads $UNC2 -- the inherited detector's backslash alternative needs four, so bare UNC is a known census gap"
if [ "$C_RAW" != "11" ] || [ "$C_RES" != "2" ] || [ "$UNC4" != "1" ]; then
  stamp "CONTROL C1/G6 FAILED -- the census pattern set is not behaving; NOTHING assembled -- ABORT"
  stamp "  residual lines the control actually produced:"
  census_residual_lines < "$PLANT" | cut -c1-200 | sed 's/^/    /' | tee -a "$ASMLOG"
  exit 3
fi
# --- CONTROL C1/G6T: the TEMPLATE class, six arms, each a different answer ---------------------------
T_EXC=$(printf '%s\n' '+		{"posix home path", fmt.Sprintf("root at /home/%s/go\n", seg), "profile-path", false},' | census_residual)
T_EXC2=$(printf '%s\n' '+		{"split with indent", fmt.Sprintf("root at /home/\n    %s/go\n", seg), "profile-path-split", true},' | census_residual)
T_EXC3=$(printf '%s\n' '+			if fleetHasFold(joined, "users") || bytes.Contains(joined, []byte("/home/")) {' | census_residual)
T_EXC4=$(printf '%s\n' '+		{"unc host", fmt.Sprintf("share at \\\\%s\\public\\x\n", host), "network-path", false},' | census_residual)
T_SURV=$(printf '%s\n' '+		{"real segment", "root at /home/somebody/go\n", "profile-path", false},' | census_residual)
T_SURV2=$(printf '%s\n' '+	x := fmt.Sprintf("%s at /home/realname/go", kind)' | census_residual)
T_CNT=$(printf '%s\n' '+		{"posix home path", fmt.Sprintf("root at /home/%s/go\n", seg), "profile-path", false},' | census_template)
stamp "CONTROL C1/G6T template class :: fixture with a placeholder segment residual=$T_EXC (want 0) :: split fixture residual=$T_EXC2 (want 0) :: detector literal residual=$T_EXC3 (want 0) :: UNC fixture residual=$T_EXC4 (want 0) :: REAL segment residual=$T_SURV (want 1 -- the class must not launder a real name) :: placeholder ELSEWHERE with a real segment residual=$T_SURV2 (want 1 -- the placeholder must be the SEGMENT, not merely on the line) :: templateExcluded on the fixture=$T_CNT (want 1)"
if [ "$T_EXC" != "0" ] || [ "$T_EXC2" != "0" ] || [ "$T_EXC3" != "0" ] || [ "$T_EXC4" != "0" ] || [ "$T_SURV" != "1" ] || [ "$T_SURV2" != "1" ] || [ "$T_CNT" != "1" ]; then
  stamp "CONTROL C1/G6T FAILED -- the TEMPLATE class is either laundering a real segment or not excluding a fixture; NOTHING assembled -- ABORT"; exit 1
fi
rm -f "$PLANT"

# --- C2: G2 both-edition parser control -------------------------------------------------------
GOODPS="/tmp/${TAG}-ctl-good.ps1"; BADPS="/tmp/${TAG}-ctl-bad.ps1"
printf '%s\n' 'Write-Host "control"' > "$GOODPS"
printf '%s\n' 'function f {' 'Write-Host "unterminated' > "$BADPS"
G2_EDITIONS=""
for ed in powershell pwsh; do
  if ! command -v "$ed" >/dev/null 2>&1; then
    stamp "CONTROL C2 $ed not on PATH -- that edition is UNMEASURED for this run (state it in the landing post)"
    continue
  fi
  parse_ps1 "$ed" "$(cygpath -wa "$GOODPS")" "$CTLLOG"; grc=$?
  parse_ps1 "$ed" "$(cygpath -wa "$BADPS")"  "$CTLLOG"; brc=$?
  stamp "CONTROL C2 $ed :: good.ps1 exit=$grc (want 0)  broken.ps1 exit=$brc (want 1)"
  if [ "$grc" != "0" ] || [ "$brc" != "1" ]; then
    stamp "CONTROL C2 $ed FAILED -- that parser arm cannot tell a good file from a broken one; NOTHING assembled -- ABORT"
    exit 3
  fi
  G2_EDITIONS="$G2_EDITIONS $ed"
done
[ -n "$G2_EDITIONS" ] || { stamp "CONTROL C2: NO PowerShell edition available -- ABORT"; exit 3; }
stamp "CONTROL C2 live editions:$G2_EDITIONS"

# --- C3: POSITIVE CONTROL on the live-process census ------------------------------------------
PC_ALL=$(powershell -NoProfile -Command "@(Get-CimInstance Win32_Process).Count" 2>/dev/null | tr -d '\r')
PC_EXE=$(proc_count '.exe')
stamp "CONTROL C3 live-process census positive control :: totalProcesses=${PC_ALL:-unknown} (want >10) sameShapeQueryOn'.exe'=${PC_EXE:-unknown} (want >0)"
case "${PC_ALL}" in ''|*[!0-9]*) PC_ALL=0 ;; esac
case "${PC_EXE}" in ''|*[!0-9]*) PC_EXE=0 ;; esac
if [ "$PC_ALL" -le 10 ] || [ "$PC_EXE" -le 0 ]; then
  stamp "CONTROL C3 FAILED -- the process census cannot see processes it must see; its zero would be the INSTRUMENT's, not the tree's; NOTHING assembled -- ABORT"
  exit 3
fi

# --- C4: CR/LF byte-probe control, BOTH DIRECTIONS --------------------------------------------
CRP_LF="/tmp/${TAG}-crlf-lf.probe"; CRP_CRLF="/tmp/${TAG}-crlf-crlf.probe"
printf 'a\nb\n'     > "$CRP_LF"
printf 'a\r\nb\r\n' > "$CRP_CRLF"
C4_LF=$(cr_bytes "$CRP_LF"); C4_CRLF=$(cr_bytes "$CRP_CRLF"); C4_CRLF_N=$(lf_lines "$CRP_CRLF")
stamp "CONTROL C4 CR byte probe :: LF-only file reads $C4_LF (want 0)  CRLF file reads $C4_CRLF over $C4_CRLF_N line(s) (want 2 == 2)"
if [ "$C4_LF" != "0" ] || [ "$C4_CRLF" != "2" ] || [ "$C4_CRLF_N" != "2" ]; then
  stamp "CONTROL C4 FAILED -- the CR byte probe cannot tell CRLF from LF; every CR==LF assertion below would be meaningless; NOTHING assembled -- ABORT"
  exit 3
fi
rm -f "$CRP_LF" "$CRP_CRLF"

# --- C5: THE TOOLCHAIN PIN'S OWN CONTROL, BOTH DIRECTIONS -------------------------------------
#     A pin that cannot REFUSE is a decoration: under the box's ambient GOTOOLCHAIN=auto a 1.23.12
#     shell SWITCHES to 1.24.13 on the strength of a go.mod, so "I pinned 1.23.12" would be
#     unfalsifiable -- and LEG K's whole correctness rests on the 1.23.12 pin holding.
#       1.24.13 + local  must SUCCEED   (the root can serve the requirement)
#       1.23.12 + local  must REFUSE    (it cannot, and it must SAY so rather than switch)
PINCTL="$SCRIPT_DIR/coord-${TAG}-pinctl"
rm -rf "$PINCTL"; mkdir -p "$PINCTL"
printf 'module %spinctl\n\ngo 1.24.13\n' "$TAG" > "$PINCTL/go.mod"
C5OK=1
if pin_go 1.24.13 "C5-arm1"; then
  ( cd "$PINCTL" && go list -m > /dev/null 2>>"$CTLLOG" ) ; c5a=$?
else
  c5a=99
fi
if pin_go 1.23.12 "C5-arm2"; then
  ( cd "$PINCTL" && go list -m > /dev/null 2>>"$CTLLOG" ) ; c5b=$?
else
  c5b=99
fi
stamp "CONTROL C5 toolchain pin :: a module declaring go 1.24.13 :: under the 1.24.13 pin exit=$c5a (want 0) :: under the 1.23.12 pin exit=$c5b (want NON-ZERO -- the pin must REFUSE, not switch)"
[ "$c5a" = "0" ] || { stamp "  ^ C5 arm 1 FAILED: the 1.24.13 root could not serve a module that requires it"; C5OK=0; }
[ "$c5b" = "0" ] && { stamp "  ^ C5 arm 2 FAILED: the 1.23.12 pin ACCEPTED a module requiring 1.24.13 -- GOTOOLCHAIN is switching under the pin, so every per-leg toolchain reading below would be a decoration and LEG K's two-pin shape would be a fiction"; C5OK=0; }
[ "$c5b" = "99" ] && { stamp "  ^ C5 arm 2 UNMEASURED: the 1.23.12 pin itself could not be established -- LEG K needs that root, so this is a refusal rather than a note"; C5OK=0; }
rm -rf "$PINCTL"
[ "$C5OK" = "1" ] || { stamp "CONTROL C5 FAILED -- NOTHING assembled -- ABORT"; exit 3; }

# --- C6: THE LEG K oracleGoVersion PREDICATE'S OWN CONTROL, THREE ARMS ------------------------
#     ⚠ THIS CONTROL EXISTS BECAUSE TRAIN 44's RUN GOT THIS EXACTLY WRONG.  Its LEG K case-arm
#     expected the record's oracleGoVersion to read the bare release TOKEN `go1.23.12`, while the
#     converter records the bare `go version` OUTPUT (testConversion.go: "OracleGoVersion is the bare
#     `go version` output of the toolchain that actually ran the ..."), so BOTH canary rows -- each
#     PASSING with the oracle at 1.23.12 -- were stamped REFUSED by the instrument, and the landing
#     had to grow a named fault path to accept them.  The predicate is corrected here and the fault
#     path is GONE from train 45's land script; a control is what makes that a fix rather than a
#     different guess.
#     The predicate is ONE definition with TWO consumers (this control and LEG K itself), because a
#     second copy of a pattern is the thing that drifts.
LEGK_ORACLE_RE='^go version go1\.23\.12 [a-z0-9]+/[a-z0-9]+$'
legk_oracle_ok(){ [ "$(printf '%s' "$1" | grep -acE "$LEGK_ORACLE_RE" || true)" != "0" ]; }
C6A=0; C6B=0; C6C=0; C6D=0
legk_oracle_ok 'go version go1.23.12 windows/amd64' && C6A=1     # the REAL form -- must ACCEPT
legk_oracle_ok 'go version go1.24.13 windows/amd64' && C6B=1     # the wrong RELEASE -- must REFUSE
legk_oracle_ok 'go1.23.12'                           && C6C=1     # the bare TOKEN -- must REFUSE
legk_oracle_ok 'go version go1.23.1 windows/amd64'  && C6D=1     # a PREFIX of the right release -- must REFUSE
stamp "CONTROL C6 LEG K oracleGoVersion predicate [$LEGK_ORACLE_RE] :: real form accepted=$C6A (want 1) :: wrong release go1.24.13 accepted=$C6B (want 0) :: bare token accepted=$C6C (want 0) :: the PREFIX release go1.23.1 accepted=$C6D (want 0 -- an unanchored pattern would take it, which is the exact shape a 'right spelling of the wrong release' arrives in)"
if [ "$C6A" != "1" ] || [ "$C6B" != "0" ] || [ "$C6C" != "0" ] || [ "$C6D" != "0" ]; then
  stamp "CONTROL C6 FAILED -- the LEG K oracle predicate cannot tell the recorded form from a wrong one; LEG K's verdict would be a decoration either way; NOTHING assembled -- ABORT"
  exit 3
fi


# ============================================================================================
# TREE GATES
# ============================================================================================
FREE0=$(freegb)
stamp "disk preflight :: freeGB=${FREE0:-unknown} (floor 30 -- train 40b's number, kept because this train runs the same two disk-hungry legs: the FULL behavioral suite writes ~20 GB of per-project bin output, and LEG K's sweep then refuses at its OWN 25 GB floor if the post-suite purge does not recover it)"
case "${FREE0}" in ''|*[!0-9]*) FREE0=0 ;; esac
[ "$FREE0" -ge 30 ] || { stamp "ABORT disk ${FREE0}GB < 30 -- a battery is a disk INPUT and its floor is preflighted BEFORE its first leg"; exit 93; }

stamp "tree: head=$(git rev-parse --short HEAD) branch=$(git rev-parse --abbrev-ref HEAD) dirty=$(git status --porcelain | wc -l)"
[ "$(git status --porcelain | wc -l)" = "0" ] || { stamp "worktree dirty -- ABORT"; git status --porcelain | head -20 | sed 's/^/    /'; exit 1; }

LIVE=$(proc_count "$WT_TOKEN")
stamp "live processes by PATH/cmdline in this worktree (shells excluded; the C3 control proved this query answers): ${LIVE:-unknown}"
[ "${LIVE:-1}" = "0" ] || { stamp "a gate/build is live in this worktree -- ABORT (mid-battery freeze)"; exit 1; }

# ⚠ THE PRE-MERGE PIN IS 1.24.13 HERE, NOT 1.23.12 AS IN TRAIN 43.  master f4d2b981b's go.mod already
# declares 1.24.13, so the base itself needs the new root; a 1.23.12 pre-merge pin would be a
# statement about a tree that no longer exists.
pin_go 1.24.13 "PRE-MERGE" || exit 90

git fetch -q origin master || { stamp "fetch origin master FAILED -- ABORT"; exit 1; }

# --- the two-armed tree route.  Both arms are STAMPED; neither is silent. ---------------------
MYTOP="$(cd "$(git rev-parse --show-toplevel)" && pwd)"
MASTER_WT="$(git worktree list --porcelain | awk '/^worktree /{w=$2} /^branch refs\/heads\/master$/{print w; exit}')"
MASTER_WT_N=""
[ -n "$MASTER_WT" ] && MASTER_WT_N="$(cd "$MASTER_WT" 2>/dev/null && pwd)"
if [ -z "$MASTER_WT_N" ] || [ "$MASTER_WT_N" = "$MYTOP" ]; then
  ROUTE=A
  stamp "TREE ROUTE A :: master is free in this worktree -- checkout master + pull --ff-only"
  git checkout -q master || { stamp "ROUTE A: git checkout master FAILED -- ABORT"; exit 1; }
  git pull --ff-only origin master; prc=$?
  stamp "ROUTE A: git pull --ff-only origin master exit=$prc"
  [ "$prc" = "0" ] || { stamp "ROUTE A: the pull was NOT a fast-forward -- this worktree's master has diverged from origin; resolve by hand -- ABORT"; exit 1; }
else
  ROUTE=B
  stamp "TREE ROUTE B :: master is checked out in ANOTHER worktree ($MASTER_WT_N) -- pulling a branch another worktree holds would move ITS head out from under it, so this run DETACHES at origin/master instead.  Identical in tree terms; the holder is untouched."
  git checkout -q --detach origin/master || { stamp "ROUTE B: git checkout --detach origin/master FAILED -- ABORT"; exit 1; }
fi

if [ "$(git rev-parse HEAD)" = "$(git rev-parse origin/master)" ]; then
  if [ "$ROUTE" = "B" ]; then
    stamp "HEAD == origin/master OK ($(git rev-parse --short HEAD)) -- under ROUTE B this is a CONSISTENCY check, not an independent one: route B set HEAD to origin/master, so it cannot fail here.  Named rather than quoted as a gate with teeth."
  else
    stamp "HEAD == origin/master OK ($(git rev-parse --short HEAD)) -- ROUTE A, an independent check: the pull had to land it there."
  fi
else
  stamp "HEAD != origin/master ($(git rev-parse --short HEAD) vs $(git rev-parse --short origin/master)) -- ABORT"
  exit 1
fi
BASE=$(git rev-parse --short HEAD)
# --- ⚠ THE BASE IS **ASSERTED**, NOT MERELY DERIVED, AND BOTH SHAS ARE PRINTED.  HEAD == origin/master
#     above says the tree is AT master; it does NOT say master is the master this script was derived
#     against.  A gate reading has a TREE, and when the tree moves the reading expires whether or not
#     the change does -- so every per-leg expectation, every class shape and LEG D's whole prediction
#     were derived for EXACTLY one base, and a different one means re-deriving rather than adjusting.
stamp "BASE ASSERTION :: this template's declared base EXPECT_BASE=$EXPECT_BASE :: this run's BASE (== HEAD == origin/master) = $BASE :: origin/master right now reads $(git ls-remote origin refs/heads/master | cut -c1-9)"
case "$EXPECT_BASE" in
  PENDING|''|*PENDING*)
    stamp "  ^ BASE REFUSED: EXPECT_BASE reads [$EXPECT_BASE].  This is FILL POINT F2 -- the base was not known when this template was derived.  Fill it, and RE-READ every expectation against it: a template that runs with a placeholder base measures a tree nobody named -- ABORT"
    exit 2 ;;
  origin/master)
    # ⚠ THE DECLARED BASE IS A **REVSPEC**, NOT A SHA, AND THAT IS THIS TRAIN'S RULE.  The HEAD ==
    #   origin/master check above has already run; what makes this an assertion rather than a
    #   restatement is the CONTAINMENT pin below, which is a fact about the base's CONTENT.
    stamp "BASE :: EXPECT_BASE reads [origin/master] -- the base is origin/master RESOLVED AT LAUNCH ($BASE), never a literal SHA baked into a gate.  A baked SHA refuses a HEALTHY launch the moment any lane lands; the CONTAINMENT pin below is the assertion that has teeth."
    ;;
  *)
    case "$BASE" in
      "$EXPECT_BASE"*) : ;;
      *) stamp "  ^ BASE REFUSED: this script declares $EXPECT_BASE and the tree is at $BASE.  Every expectation, class shape and prediction in it was derived for that base -- RE-DERIVE, do not adjust -- ABORT"; exit 2 ;;
    esac ;;
esac

# --- BASE ANCESTRY, ASSERTED.  Both SHAs were already ancestors of origin/master at derive time, so a
#     red here means master went somewhere nobody expects -- not a legitimate reordering.
for anc in "$EXPECT_ORDER:the ORDER pin (train 47's base, a SHA that was already an ancestor of origin/master when this template was derived -- measured with git ls-remote on 2026-09-13 -- so it cannot false-red)" \
           "$T48_CONTAIN_PIN:the CONTAINMENT pin THIS train requires -- the SHA TRAIN 47 LANDED.  This train's seats were cut against a tree that carries it, and every class shape, every OWED reading and LEG D's whole prediction is derived FOR a base.  ⚠ This is the assertion that replaces a baked base SHA: it is a statement about the base's CONTENT and it survives every legitimate move of origin/master"; do
  a="${anc%%:*}"; adesc="${anc#*:}"
  if ! git cat-file -e "${a}^{commit}" 2>/dev/null; then
    stamp "BASE ANCESTRY REFUSED :: $a ($adesc) does not resolve here even after the master fetch -- an unresolved SHA and an invented one read the same, so this is a REFUSAL, not a 'no' -- ABORT"
    exit 1
  fi
  if git merge-base --is-ancestor "$a" HEAD 2>/dev/null; then
    stamp "BASE ANCESTRY OK :: base master=$BASE CONTAINS $a -- $adesc"
  else
    stamp "BASE ANCESTRY REFUSED :: base master=$BASE does NOT contain $a ($adesc).  That SHA was already an ancestor of origin/master when this script was derived, so this cannot be a legitimate reordering -- ABORT"
    exit 1
  fi
done

# --- the BASE's own toolchain declaration, read from the file.  If master's go.mod no longer says
#     1.24.13 then the window has CLOSED (H5 landed) and every pin decision in this script is stale.
BASEMOD='src/go2cs/go.mod'
if [ -f "$BASEMOD" ]; then
  B24=$(grep -acE '^go 1\.24\.13$' "$BASEMOD" || true)
  stamp "BASE toolchain declaration :: $BASEMOD declares 'go 1.24.13' x$B24 (must be 1 -- the H2->H5 window is OPEN)"
  [ "$B24" = "1" ] || { stamp "  ^ ABORT: the base does not declare go 1.24.13.  Either H5 landed and the window CLOSED, or the base is not what this script was derived against; every per-leg pin below would describe a tree nobody has."; exit 90; }
else
  stamp "BASE toolchain declaration UNMEASURED :: $BASEMOD absent -- ABORT"; exit 90
fi
BASEPROPS='src/version.props'
if [ -f "$BASEPROPS" ]; then
  BPIN=$(sed -nE 's#.*<GoStdLibVersion>([^<]+)</GoStdLibVersion>.*#\1#p' "$BASEPROPS" | tail -1)
  stamp "BASE corpus pin :: $BASEPROPS <GoStdLibVersion>=${BPIN:-unreadable} (must be 1.23.12 -- this is the release LEG K's sweep will REQUIRE, and its own guard throws on a disagreement)"
  [ "${BPIN:-}" = "1.23.12" ] || { stamp "  ^ ABORT: the corpus pin is not 1.23.12, so LEG K's two-pin shape names the wrong release.  Re-derive rather than adjust here."; exit 90; }
else
  stamp "BASE corpus pin UNMEASURED :: $BASEPROPS absent -- ABORT"; exit 90
fi

if git rev-parse --verify -q "refs/heads/$HEADBR" >/dev/null; then
  stamp "note: $HEADBR already exists at $(git rev-parse --short "$HEADBR") -- this is a RE-RUN; it is force-reset to $BASE"
fi
git checkout -q -B "$HEADBR" || { stamp "cannot create $HEADBR (is it checked out in another worktree? -- git worktree list) -- ABORT"; exit 1; }

# ============================================================================================
# SEATS
# ============================================================================================
MERGELOG="/tmp/${TAG}-merge.log"; : > "$MERGELOG"

# --- THERE IS NO ENV SEAT IN THIS TRAIN.  A mandatory-env slot for a seat nobody names is a refusal
#     waiting to fire on the WRONG REASON, so the mechanism is absent rather than unset.  Every row
#     the coordinator has not yet named is a PENDING row in the table, which is a different and
#     VISIBLE mechanism.
#
# --- ⚠ **THE DEFAULT IS INVERTED AT SEAT FILL (2026-09-13): A PENDING ROW NOW *REFUSES*.**
#     The TEMPLATE skipped a PENDING row with a stamp, which was right for a file whose every row was a
#     placeholder.  This train's table is FILLED -- FIFTEEN rows named (SIXTEEN until the 2026-09-13
#     unseating of the former row 8), ZERO awaiting an announced SHA --
#     and with a filled table the cheap mistake changes direction: the dangerous run is no longer "the
#     template refused", it is "a launch quietly assembled a subset of the fifteen rows and every count in the
#     record was internally consistent about a train nobody announced".  So the skip is now something a
#     run must OPT INTO BY NAME, and the two switches say different things:
#         TRAIN48_SKIP_PENDING=1  a PENDING row is SKIPPED WITH A STAMP and the run is a PARTIAL train
#                                 that says so in every count -- a REHEARSAL/DRESS shape.
#         TRAIN48_REQUIRE_ALL=1   the LANDING shape.  A PENDING row ABORTS (as it does by default) AND
#                                 the land script's own seat accounting requires requireAll=1 with
#                                 skippedPending=0, which is the second half of the same claim.
#         neither                 a PENDING row ABORTS BY NAME.  The safe default.
#     ⚠ THE TWO TOGETHER ARE A CONTRADICTION AND ARE REFUSED rather than ordered by precedence: one
#     demands every row and the other permits a gap, and a script that silently picked a winner would
#     be deciding what the operator meant.
#     ⚠ Declaredness and emptiness are tested SEPARATELY: a DECLARED-BUT-EMPTY TRAIN48_REQUIRE_ALL is
#     REFUSED, because ${X:-0} reads an empty override as unset and a blank authorisation would
#     silently become "no".
# --- ⚠ **THE UN-PREFIXED SPELLING IS A REFUSAL, NOT A SILENCE.**  (Carried template defect (a), fixed
#     2026-09-13 before first use.)  `REQUIRE_ALL` is this script's OWN internal variable and it is
#     assigned unconditionally on the next line, so a launcher that exports `REQUIRE_ALL=1` -- the
#     obvious spelling, and the one a wrapper written from memory reaches for -- is OVERWRITTEN here
#     and the run proceeds as a PARTIAL train while the operator believes they configured a LANDING.
#     A silently-ignored authorisation is worse than a missing one: it produces an internally
#     consistent record of a train nobody announced.  So the inherited spelling is detected BEFORE the
#     assignment and named.
if [ "${REQUIRE_ALL+x}" = "x" ]; then
  stamp "REQUIRE_ALL is in the ENVIRONMENT reading [${REQUIRE_ALL:-<empty>}].  That is NOT this script's switch -- the switch is TRAIN48_REQUIRE_ALL -- and this variable is about to be OVERWRITTEN by the script's own default, so an export of it would configure NOTHING while reading as a landing run.  Re-launch with TRAIN48_REQUIRE_ALL=<0|1> -- ABORT"
  exit 2
fi
REQUIRE_ALL=0
if [ "${TRAIN48_REQUIRE_ALL+x}" = "x" ]; then
  if [ -z "${TRAIN48_REQUIRE_ALL:-}" ]; then
    stamp "TRAIN48_REQUIRE_ALL is DECLARED but EMPTY.  An empty authorisation is not 'unset' and must not be read as either answer -- name it or do not declare it -- ABORT"
    exit 2
  fi
  case "$TRAIN48_REQUIRE_ALL" in
    1) REQUIRE_ALL=1
       stamp "TRAIN48_REQUIRE_ALL=1 :: any row whose SHA reads PENDING will refuse by name.  This is the LANDING shape: it asserts that every row the coordinator announced has been filled, and it is the only shape in which the assembled tree is the full train." ;;
    0) REQUIRE_ALL=0
       stamp "TRAIN48_REQUIRE_ALL=0 :: stated explicitly.  ⚠ THIS IS NOT A PERMISSION TO SKIP: a PENDING row still ABORTS unless TRAIN48_SKIP_PENDING=1 is ALSO given, because with a FILLED table the dangerous run is the one that quietly assembles a subset of the rows this table declares" ;;
    *) stamp "TRAIN48_REQUIRE_ALL reads [$TRAIN48_REQUIRE_ALL], which is neither 0 nor 1 -- an authorisation this script cannot read is not an authorisation -- ABORT"; exit 2 ;;
  esac
else
  stamp "TRAIN48_REQUIRE_ALL is not in the environment :: a PENDING row ABORTS BY NAME (the default at seat fill).  ⚠ A LANDING RUN SETS TRAIN48_REQUIRE_ALL=1, and the land script's own seat-count check -- requireAll=1 AND skippedPending=0 -- is the second half of that."
fi
# --- ⚠ THE SKIP IS AN **OPT-IN**, and the un-prefixed spelling is refused for the same reason as above.
if [ "${SKIP_PENDING+x}" = "x" ]; then
  stamp "SKIP_PENDING is in the ENVIRONMENT reading [${SKIP_PENDING:-<empty>}].  That is NOT this script's switch -- the switch is TRAIN48_SKIP_PENDING -- and this variable is about to be OVERWRITTEN by the script's own default, so an export of it would configure NOTHING while reading as an authorised partial run -- ABORT"
  exit 2
fi
SKIP_PENDING=0
if [ "${TRAIN48_SKIP_PENDING+x}" = "x" ]; then
  if [ -z "${TRAIN48_SKIP_PENDING:-}" ]; then
    stamp "TRAIN48_SKIP_PENDING is DECLARED but EMPTY.  An empty authorisation is not 'unset' and must not be read as either answer -- name it or do not declare it -- ABORT"
    exit 2
  fi
  case "$TRAIN48_SKIP_PENDING" in
    1) SKIP_PENDING=1
       stamp "TRAIN48_SKIP_PENDING=1 :: a PENDING row is SKIPPED WITH A STAMP and THIS RUN IS A PARTIAL TRAIN.  Every count below -- SEATS_DONE, the OWED vector, LEG 1's registration delta, LEG D's prediction, LEG 5's enumeration and LEG K's row set -- is DERIVED at run time, so this run's numbers are internally consistent; they are simply not the announced train's, and the land script REFUSES such a record as a landing." ;;
    0) SKIP_PENDING=0
       stamp "TRAIN48_SKIP_PENDING=0 :: stated explicitly; a PENDING row ABORTS BY NAME" ;;
    *) stamp "TRAIN48_SKIP_PENDING reads [$TRAIN48_SKIP_PENDING], which is neither 0 nor 1 -- an authorisation this script cannot read is not an authorisation -- ABORT"; exit 2 ;;
  esac
fi
if [ "$REQUIRE_ALL" = "1" ] && [ "$SKIP_PENDING" = "1" ]; then
  stamp "TRAIN48_REQUIRE_ALL=1 AND TRAIN48_SKIP_PENDING=1 are a CONTRADICTION: one demands every announced row and the other permits a gap.  A script that picked a winner here would be deciding what the operator meant -- name ONE of them -- ABORT"
  exit 2
fi

# ============================================================================================
# THE SEAT TABLE -- ⚠ **FILL POINT F3**.
# ONE place holds the MERGE ORDER, the seat NUMBER, the ref, the required SHA, the CLASS, the TIP
# MODE and -- optionally -- the ALLOWED set.  The loop below asserts processed == listed, so a child
# that eats the loop's stdin cannot swallow a seat and report a clean run (which is why the table is
# fed on FD 3 and every child reads /dev/null).
#
#   FIELDS:  N | ref | SHA | class | tipmode [| <key>=<value> ...]
#     ⚠ FIELDS SIX AND BEYOND ARE ORDER-FREE `key=value` PAIRS.  The known keys are `allowed=<ERE>`
#       and `stack-on=<row number>`; an UNKNOWN key is REFUSED by name rather than ignored, which is
#       also what catches an `allowed=` ERE carrying a `|` alternation (it splits into fields here,
#       and silently keeping field six alone would run a narrower exemption than was ruled).
#     N         the seat number.  ⚠ THE ROW ORDER IS THE MERGE ORDER, and the numbers must be
#               1..N contiguous with no duplicate -- asserted below, because a hand-edited table is
#               exactly what produces a duplicated or misnumbered row and nothing else would see it.
#     ref       the branch.  A row whose SHA is FILLED while its ref still reads `claude/PENDING-`
#               is REFUSED: a filled SHA against a branch nobody named is a seat nobody can check.
#     SHA       nine hex digits, or the literal PENDING.
#     class     one of the classes merge_seat implements (see the header).  The class column of a
#               placeholder row is a PLACEHOLDER TOO -- overwrite it with the ref and the SHA.
#     tipmode   `tip` the branch tip must EQUAL the pin | `sha` the pin must be an ANCESTOR of the
#               tip, with everything beyond it listed by name as deliberately not carried.
#     allowed=  OPTIONAL (field 6+).  An ERE naming committed behavioral files this seat may MODIFY, under a
#               coordinator ruling stated at the row.  A7 admits EXACTLY those paths and only when
#               the union's BLOB for each equals this seat's own blob -- so the exemption carries
#               nothing another seat rides in on.  Absent means A7's strict form: NO committed
#               behavioral file may be modified or deleted except the four MSTest classes.
#               ⚠ IT IS A RULING, NOT A CONVENIENCE.  Write the ruling in the seat's own note.
#               ⚠⚠ **EXTENDED 2026-09-13 19:05 (COORD, D5) FROM "A7's EXEMPTION" TO "A PER-ROW RULING
#               OVER THE WHOLE SEAT".**  The ERE admits EXACTLY the named paths OUTSIDE the class's
#               SHAPE and outside its FORBIDDEN list as well -- merge_seat's two readings subtract
#               them, COUNT them and LIST them (`allowedByRuling=N` plus the paths); a ruled path the
#               seat does not touch is a STALE RULING and ABORTS BY NAME; and the identity predicate
#               is asserted TWICE -- once per seat immediately after its merge (union blob == that
#               seat's blob) and once over the finished union in arm A7b, where a path ruled by two
#               rows is owned by the LAST of them in merge order.  ⚠ NOT ONE CLASS SHAPE OR FORBIDDEN
#               LIST WAS WIDENED to make this work; widening is what a ruling exists to avoid.
# ⚠ **FILLED FROM THE TRAIN-48 BOARD, 2026-09-13.**  EIGHTEEN ROWS, ALL EIGHTEEN PINNED.  Every SHA
#   was MEASURED against `git ls-remote origin` on 2026-09-13 EVENING, after train 47 landed, and
#   RE-READ at 19:05 for the adversarial verify; NONE reads PENDING.
#   ⚠⚠ **THE ADVERSARIAL VERIFY OF 2026-09-13 19:05 FOUND FOUR TABLE DEFECTS AND THE COORDINATOR RULED
#      ON ALL FOUR (D1-D4).  EVERY ONE IS APPLIED HERE, IN THE DERIVE, AND NAMED:**
#      (D1) ROW 17 WAS STALE.  claude/c2-h10-shardmap-projection had moved ONE COMMIT past 5129946000
#           -- ls-remote reads a633896bf69d554f33b19b7a574466175ece32db.  RE-PINNED TO THE ORIGIN TIP
#           (tip mode).  The C2 AMENDMENTS re-cut §20 recorded as "NOT at origin" IS at origin now.
#      (D2) ROW 11 DECLARED stack-on=13, WHICH NAMES A LATER ROW, and seat_opts_validate refuses it --
#           THE ROW ORDER IS THE MERGE ORDER.  THE TWO SEATS ARE SWAPPED: row 11 is now
#           claude/c2-census-goroot-fix-clean (converter-test) and row 13 is
#           claude/c1-token-door-census-stacked (converter-test+tooling) carrying `stack-on=11`.
#           NOTHING ELSE ABOUT EITHER ROW CHANGED -- each keeps its own class and its own allowed=.
#      (D3) THE NINETEENTH ROW DUPLICATED ROW 6 (claude/c2-safepush-shallow-skip @ fa2fdd30dc at both).
#           DROPPED.  This table is EIGHTEEN rows and every literal that said nineteen says eighteen.
#      (D4) ROW 4 (claude/laneR-docs-h6-skeleton) GREW PAST its pin.  It is now a **SHA MODE** row at
#           the SAME pin d18059950 -- the pin IS an ancestor of the tip (measured), and the excluded
#           commit(s) are STAMPED BY NAME at assembly.  ⚠ THE COUNT IS NOT WRITTEN DOWN ANYWHERE HERE:
#           D4 measured ONE excluded commit at 19:05 and E3 measured TWO at ~20:00, and the branch may
#           move again before the train runs.  SHA mode is exactly the mode that does not care.
#      (E1) ROW 13 DROPS THE `allowed=` RULING IT INHERITED FROM THE PRE-D2 TABLE.  merge_seat measures
#           a seat against merge-base(HEAD, want); after row 11 merges, row 13's base is row 11's tip
#           and its measured diff is its own four files, none of them under `.claude/`.  The inherited
#           ruling named two paths ABSENT from that diff, which merge_seat's own STALE-RULING arm (D5)
#           refuses BY NAME -- so the row would have ABORTED THE ASSEMBLY.  Row 11 keeps its ruling and
#           is the SOLE owner of the two `.claude/**` paths.
#   ⚠ TWO ROWS MOVED AT THAT RE-READ AND WERE RE-PINNED IN THE DERIVE (never by hand in the assembly):
#     row 1 -- the G handown branch was REBASED onto the train-47 landing exactly as the ruling said it
#             would be, and it rebased under a NEW NAME: claude/g-handown-metadata-t48-r47 @ 35fe4e016.
#             The old name claude/g-handown-metadata-t48 still exists at bb13897e6 and is SUPERSEDED --
#             pinning the old name would merge a branch whose three-way base is a pre-train-47 tree.
#     row 7 -- claude/c1-seat-duplication-census moved 77e41300a -> a4802675d (the finding recorded in
#             §3.1 of the NOTES, now discharged by re-pinning at the coordinator's instruction).
#             ⚠ THE PATCH-ID ARM READS ITS OWN TOOL OUT OF THIS ROW'S BLOB, so the census tool's
#             --self-test is owed AGAIN at a4802675d: the run §6 records was taken at 77e41300a.  ⚠ THE ROW ORDER **IS** THE MERGE ORDER, and two rows
#   are ordered by a DECLARED STACK rather than by preference: a `stack-on=` row must merge after the
#   row it is built on, and the parser refuses the other order.
#     row  ref                                        class                    note
#      1   claude/g-handown-metadata-t48-r47          converter-corpus-metadata  allowed= cache.cs (COORD ruling 2026-09-13 18:20 on G 7a947650f: the re-mint emits the import-init hook into package_info.cs, so the hand-carried copy in cache.cs is a duplicate [GoInit] the seat's own guard refuses; A7 admits it only when the union blob equals the seat's).  RE-PINNED after train 47 landed: the rebase the
#                                                                               ruling required happened, under a NEW NAME, and
#                                                                               ls-remote reads 35fe4e016 as this branch's tip.
#                                                                               The pre-rebase name (bb13897e6) is SUPERSEDED.
#      2   claude/c2-h6-crosscheck                    docs
#      3   claude/laneR-h6-alias-block                docs                       stack-on=2 -- a DECLARED H6 docs stack.  The
#                                                                               patch-id census reads a stack and a cherry-pick
#                                                                               contamination as the SAME SHAPE, so the
#                                                                               declaration is what discriminates them.
#      4   claude/laneR-docs-h6-skeleton              docs                       ⚠ **SHA MODE (D4, 2026-09-13 19:05).**  The tip
#                                                                               has moved BEYOND the pin d18059950 and the branch
#                                                                               is still moving: D4 read ONE excluded commit
#                                                                               (c2b699dafc, G's mcleanup.cs row), and the E3
#                                                                               re-read at ~20:00 reads TWO (c2b699dafc then
#                                                                               067302ea09, the H6 audit re-cut).  Both are the
#                                                                               same VERSION-BRANCH census business: they describe
#                                                                               a 147/145-row tree where master's census is 146.
#                                                                               A commit whose record describes a tree that is not
#                                                                               master must NOT ride a master-bound train, so the
#                                                                               pin stays where it is and the mode is `sha`: the
#                                                                               pin is asserted an ANCESTOR of the tip and **THE
#                                                                               EXCLUDED COMMITS ARE STAMPED BY NAME AT RUN
#                                                                               TIME** -- no count is written here, because a
#                                                                               count in a comment goes stale the next time the
#                                                                               lane pushes and the reader then trusts the wrong
#                                                                               one.  (mailbox 46198c1b9 §5.)
#      5   claude/laneR-prepin-baselines-recut        docs
#      6   claude/c2-safepush-shallow-skip            converter-test
#      7   claude/c1-seat-duplication-census          converter-test+tooling     ⚠ THIS ROW CARRIES THE PATCH-ID ARM'S OWN TOOL
#                                                                               (src/seat-duplication-census.sh).  The arm reads
#                                                                               the tool out of THIS row's pinned blob, so the
#                                                                               instrument and the seat cannot drift -- and the
#                                                                               arm REFUSES if this row is absent or PENDING.
#                                                                               ⚠ RE-PINNED 77e41300a -> a4802675d (ls-remote,
#                                                                               2026-09-13 evening).  THE TOOL'S --self-test IS
#                                                                               OWED AGAIN AT a4802675d: §6's recorded run was
#                                                                               taken at the superseded pin.
#      8   claude/g-fleet-patchid-census              converter-test+tooling
#      9   claude/i9-board-runtime-door-bisect        docs                       REF FILL POINT F3a, FILLED 2026-09-13 BY
#                                                                               MEASUREMENT, not by guess: `git ls-remote origin`
#                                                                               reads 68ad83c2c -- i9's BOARD entry, already the
#                                                                               pin in field three -- as the tip of THIS branch,
#                                                                               on two readings a day apart.  ⚠ THE NAME ROUTES
#                                                                               THE FETCH AND THE TIP CHECK; IT CANNOT CHANGE
#                                                                               WHICH COMMIT IS MERGED, because merge_seat merges
#                                                                               the SHA and tip mode refuses unless tip == pin.
#                                                                               The preflight's refusal for an unfilled ref stays
#                                                                               in the assembly for the next template that needs
#                                                                               it; this row no longer exercises it.
#     10   claude/i9-board-archive-tar                docs                       stack-on=9
#   ⚠ ROWS 11-19 FILLED 2026-09-13 EVENING from the coordinator's FIXED TABLE (ruling 817f98813 +
#     C1 aaa41c087).  Every 40-char SHA was RE-READ with `git ls-remote origin` -- not one was typed
#     from the ruling -- and every file set below was MEASURED as
#     `git diff --name-only $(git merge-base origin/master <tip>)..<tip>`.  The class on each row is
#     the TIGHTEST class whose shapepat admits that measured set; where NO class admits a path, the
#     class was NOT widened and an `allowed=` ruling names the exact path instead.
#     ⚠ **AND THE RULING IS NOW ENFORCED RATHER THAN DECORATIVE (COORD 2026-09-13 19:05, D5).**  A
#       row's `allowed=` set is subtracted from merge_seat's FORBIDDEN reading and from its SHAPE
#       reading -- counted and listed in a stamp, never hidden -- a ruled path the seat does not
#       touch ABORTS as a STALE RULING, every ruled path is asserted BLOB-IDENTICAL to that seat's own
#       blob immediately after the merge, and arm A7b re-asserts the same identity over the finished
#       union for every ruled path outside src/tests/Behavioral.  NOT ONE CLASS WAS WIDENED to get
#       there: the shapes and forbidden lists are byte-identical to train 47's.
#     row  ref                                        class                    measured note
#     11   claude/c2-census-goroot-fix-clean          converter-test              8 files @5cee80fbe, 4 commits over 271300cea.
#                                                                               Six src/go2cs/* (converter-test's shape is
#                                                                               EXACTLY `^src/go2cs/[^/]+$`, the tightest fit)
#                                                                               plus two `.claude/**` paths named by allowed=.
#                                                                               ⚠ **SWAPPED WITH ROW 13 BY D2** -- this is the
#                                                                               row the stack is built ON, so it merges FIRST.
#     12   claude/c1-gctestisreachable-clean          golib-corpus-handown        7 files @4a9ae8cbb, 2 commits over a02ac3df3.
#                                                                               src/core/runtime/{mgc,mgc_impl,panic_impl}.cs +
#                                                                               {darwin,linux,windows}/package_info.cs +
#                                                                               src/go2cs/manualTypeOperations.go -- ALL SEVEN
#                                                                               admitted by the class shape, no ruling needed.
#     13   claude/c1-token-door-census-stacked        converter-test+tooling     11 files @b4914e878, 6 commits over
#                                                                               origin/master 271300cea.  STACKED ON ROW 11:
#                                                                               11's 5cee80fbe IS AN ANCESTOR of this tip
#                                                                               (measured `git merge-base --is-ancestor` = yes),
#                                                                               and this row's OWN delta over row 11 is FOUR
#                                                                               files
#                                                                               (docs/phase4/CENSUS-token-door-live-wrappers.md,
#                                                                               src/go2cs/go2cs-src.projitems,
#                                                                               src/go2cs/tokenDoorCensusGuard_test.go,
#                                                                               src/token-door-census.sh).  ⚠ **SWAPPED WITH ROW
#                                                                               11 BY D2**: `stack-on=11` now names an EARLIER
#                                                                               row, which is the only order the parser admits
#                                                                               and the only order that merges the stack second.
#                                                                               ⚠⚠ **THIS ROW CARRIES NO `allowed=` RULING
#                                                                               (COORD E1, 2026-09-13 ~20:00).**  It INHERITED
#                                                                               one from the pre-D2 table and that ruling was
#                                                                               STALE: merge_seat measures a seat against
#                                                                               merge-base(HEAD, want), and after row 11 merges
#                                                                               HEAD carries 5cee80fbe, so this row's measured
#                                                                               diff is its OWN FOUR FILES and NO `.claude`
#                                                                               path is in it.  A ruling naming two paths absent
#                                                                               from the diff is exactly what merge_seat's own
#                                                                               STALE-RULING arm (D5) refuses BY NAME, so the
#                                                                               inherited ruling would have ABORTED THE
#                                                                               ASSEMBLY AT THIS SEAT.  Row 11 is the SOLE
#                                                                               owner of the two `.claude/**` paths and keeps
#                                                                               its ruling; A7b then reads ONE matching row and
#                                                                               compares the union's blob against ROW 11's.
#                                                                               ⚠ MEASURED IN THE LAB, not assumed: row 11's
#                                                                               pin IS an ancestor of this tip, this tip's blobs
#                                                                               for both paths EQUAL row 11's
#                                                                               (5d63166a55…, 127dd9fefe…), so the union after
#                                                                               THIS row merges still carries row 11's blob and
#                                                                               A7b's identity predicate holds.
#     14   claude/c1-mfinal-mint-door-clean           golib-corpus-handown        1 file @3f1612524: src/core/runtime/mfinal.cs.
#                                                                               Admitted whole, no ruling needed.
#     15   claude/c2-board-both-ordered               docs                        1 file @da5e83047, 5 commits: the BOARD.
#                                                                               Admitted whole, no ruling needed.
#     16   claude/c2-merge-probe-predicate            tooling                     2 files @4b7985c07, both under
#                                                                               `.claude/skills/merge-hazards/` (SKILL.md and
#                                                                               merge-probe.sh).  ⚠ NO CLASS ADMITS EITHER PATH.
#                                                                               `tooling` is chosen for its FORBIDDEN list --
#                                                                               the widest of the vocabulary (src/core src/gen
#                                                                               src/tests src/go2cs), none of which this row
#                                                                               touches -- and both paths are NAMED by allowed=.
#     17   claude/c2-h10-shardmap-projection          docs                        1 file @a633896bf, 3 commits over 271300cea:
#                                                                               docs/phase4/DATA-h10-shardmap-projection-go124.md.
#                                                                               ⚠ **RE-PINNED BY D1 (2026-09-13 19:05).**  §20's
#                                                                               reading that the tip had NOT moved past
#                                                                               5129946000 is SUPERSEDED: ls-remote now reads
#                                                                               a633896bf69d554f33b19b7a574466175ece32db, one
#                                                                               commit further on (a SECOND AMENDMENTS block,
#                                                                               superseding section 5a per row 18).  The pin is
#                                                                               the ORIGIN TIP, re-read by ls-remote and not
#                                                                               typed from any ruling, and the mode stays `tip`.
#                                                                               The file set is UNCHANGED at the new tip (one
#                                                                               docs/phase4/*.md), so the `docs` class holds.
#     18   claude/c2-h10-map-rederivation             tooling                     6 files @41c1d1d28, 7 commits over a02ac3df3.
#                                                                               ⚠ THE RULING'S CLASS HINT ("docs") IS FALSIFIED
#                                                                               BY MEASUREMENT: the row carries
#                                                                               src/run-h10-dispatch.ps1, and the `docs` class's
#                                                                               forbidden list is `src` -- it would ABORT on the
#                                                                               FORBIDDEN arm, not merely the shape arm.
#                                                                               `tooling` admits the root-level .ps1 and the
#                                                                               three docs/phase4/*.md; the two it does not are
#                                                                               NAMED by allowed= (.gitattributes and
#                                                                               docs/phase4/hopA-inputs/shardmap.py -- the
#                                                                               generator; ⚠ NO TSVs are present at this tip,
#                                                                               contrary to the ruling's description).
#   ⚠⚠ **THE TWO STRUCTURAL REFUSALS §20 LEFT STANDING FOR THE COORDINATOR ARE NOW **RULED** AND
#      APPLIED (2026-09-13 19:05).  THEY ARE RECORDED HERE RATHER THAN ERASED, BECAUSE A TABLE
#      QUIETLY CORRECTED AWAY FROM THE RULING THAT PRODUCED IT IS A TABLE NOBODY CAN AUDIT.**
#      (a) RULED -> **SWAPPED (D2)**.  The fixed table had row 11 declaring `stack-on=13`, which names
#          a LATER row; seat_opts_validate refused it verbatim, because THE ROW ORDER IS THE MERGE
#          ORDER and a row built on another must merge SECOND.  The coordinator SWAPPED the two seats
#          rather than unseating either: claude/c2-census-goroot-fix-clean is row 11 and
#          claude/c1-token-door-census-stacked is row 13 with `stack-on=11`.  Nothing else about
#          either row moved -- each kept its own class and its own allowed= ruling.
#      (b) RULED -> **DROPPED (D3)**.  The fixed table seated claude/c2-safepush-shallow-skip
#          @ fa2fdd30dc1f06eeeccbcdc792eeb896157c5792 at BOTH row 6 and row 19: same ref, same 40-char
#          SHA, and the preflight refuses on `duplicate refs` and on `duplicate filled SHAs` alike.
#          The NINETEENTH row is DROPPED and row 6 stands.  ⚠ THE COUNT IS CORRECTED IN EVERY LITERAL
#          THAT CARRIED IT -- the header title, the F3 fill-point narrative and this block -- by the
#          same derive ops that write them, because a dropped row and a stale count in a header is
#          exactly the pair that makes a reader trust the wrong one.
#   ⚠ THE SHAs IN THE NOTE COLUMN ARE **NOT PINS**.  They are what the board named; a pin lives in the
#     table's third field and nowhere else, and a SHA read out of a comment is a SHA nobody asserted.
#   ⚠ AN `allowed=` ERE HERE CANNOT CARRY A `|` ALTERNATION -- the table is `awk -F'|'` and an
#     alternation SPLITS INTO EXTRA FIELDS, which the parser refuses by name.  The four rulings below
#     that must admit TWO paths therefore use the alternation-free form `^(pathA)?(pathB)?$`, which
#     names each path exactly and matches nothing else that is a path.
#   ⚠ `PENDING-class` IS A CLASS PLACEHOLDER AND IT REFUSES THE MOMENT ITS ROW IS FILLED.  A class
#     decides the shape a seat is held to; filling a SHA without deciding the class would run the row
#     through `merge_seat`'s unknown-class arm, which aborts -- late, and with a message about a class
#     rather than about a fill point.
SEAT_TABLE="1|claude/g-handown-metadata-t48-r47|35fe4e016|converter-corpus-metadata|tip|allowed=^src/core/crypto/internal/boring/bcache/cache\.cs$
2|claude/c2-h6-crosscheck|191164e7a|docs|tip
3|claude/laneR-h6-alias-block|47592cb3f|docs|tip|stack-on=2
4|claude/laneR-docs-h6-skeleton|d18059950|docs|sha
5|claude/laneR-prepin-baselines-recut|becf28abc|docs|tip
6|claude/c2-safepush-shallow-skip|fa2fdd30d|converter-test|tip
7|claude/c1-seat-duplication-census|a4802675d|converter-test+tooling|tip
8|claude/g-fleet-patchid-census|9b78bfff6|converter-test+tooling|tip
9|claude/i9-board-runtime-door-bisect|68ad83c2c|docs|tip
10|claude/i9-board-archive-tar|314e699c6|docs|tip|stack-on=9
11|claude/c2-census-goroot-fix-clean|5cee80fbe|converter-test|tip|allowed=^(\.claude/rules/converter\.md)?(\.claude/skills/mailbox/SKILL\.md)?$
12|claude/c1-gctestisreachable-clean|4a9ae8cbb|golib-corpus-handown|tip
13|claude/c1-token-door-census-stacked|b4914e878|converter-test+tooling|tip|stack-on=11
14|claude/c1-mfinal-mint-door-clean|3f1612524|golib-corpus-handown|tip
15|claude/c2-board-both-ordered|da5e83047|docs|tip
16|claude/c2-merge-probe-predicate|4b7985c07|tooling|tip|allowed=^(\.claude/skills/merge-hazards/SKILL\.md)?(\.claude/skills/merge-hazards/merge-probe\.sh)?$
17|claude/c2-h10-shardmap-projection|a633896bf|docs|tip
18|claude/c2-h10-map-rederivation|41c1d1d28|tooling|tip|allowed=^(\.gitattributes)?(docs/phase4/hopA-inputs/shardmap\.py)?$"

# The SHAs and refs later legs need BY NAME are DERIVED FROM THE TABLE, never retyped: the table is
# the one place that holds a seat's ref and pin, and a second copy of a SHA is the thing that drifts
# when a seat is re-pinned.
seat_field(){ printf '%s\n' "$SEAT_TABLE" | awk -F'|' -v n="$1" -v c="$2" '$1==n{print $c}'; }
# ⚠ THE ROW COUNT IS WHATEVER THE TABLE ABOVE DECLARES AND IS NOT SPELLED HERE -- a spelled count is a
#   second copy of a fact the table already carries, and the two drift the first time a row is seated
#   or unseated.  The
#   variables are named by ROW NUMBER (== merge position), which
#   is what every arm below and the DONE stamp use.  The coordinator's own seat numbering is a
#   different sequence and is carried in the table's comment block, never in code.
SEAT1_SHA=$(seat_field 1 3); SEAT2_SHA=$(seat_field 2 3); SEAT3_SHA=$(seat_field 3 3);
SEAT4_SHA=$(seat_field 4 3); SEAT5_SHA=$(seat_field 5 3); SEAT6_SHA=$(seat_field 6 3);
SEAT7_SHA=$(seat_field 7 3); SEAT8_SHA=$(seat_field 8 3); SEAT9_SHA=$(seat_field 9 3);
SEAT10_SHA=$(seat_field 10 3); SEAT11_SHA=$(seat_field 11 3); SEAT12_SHA=$(seat_field 12 3);
SEAT13_SHA=$(seat_field 13 3); SEAT14_SHA=$(seat_field 14 3); SEAT15_SHA=$(seat_field 15 3);
SEAT16_SHA=$(seat_field 16 3); SEAT17_SHA=$(seat_field 17 3); SEAT18_SHA=$(seat_field 18 3)
SEAT1_REF=$(seat_field 1 2); SEAT2_REF=$(seat_field 2 2); SEAT3_REF=$(seat_field 3 2);
SEAT4_REF=$(seat_field 4 2); SEAT5_REF=$(seat_field 5 2); SEAT6_REF=$(seat_field 6 2);
SEAT7_REF=$(seat_field 7 2); SEAT8_REF=$(seat_field 8 2); SEAT9_REF=$(seat_field 9 2);
SEAT10_REF=$(seat_field 10 2); SEAT11_REF=$(seat_field 11 2); SEAT12_REF=$(seat_field 12 2);
SEAT13_REF=$(seat_field 13 2); SEAT14_REF=$(seat_field 14 2); SEAT15_REF=$(seat_field 15 2);
SEAT16_REF=$(seat_field 16 2); SEAT17_REF=$(seat_field 17 2); SEAT18_REF=$(seat_field 18 2)

# --- ⚠ **THE OPTIONAL FIELDS, PARSED AS ORDER-FREE `key=value` PAIRS FROM FIELD SIX ON.**
#     Train 47 read field SIX as `allowed=...` positionally.  With two optional keys that shape is a
#     silent truncation: `read -r n b s c m a` puts EVERY remaining field into `a` delimiters and all,
#     an awk test of `$6 ~ /^allowed=/` misses a ruling written in field seven, and both failures read
#     as "this row declares no ruling" -- which is the strict default and therefore invisible.
SEAT_OPT_KEYS='allowed stack-on'
seat_opt(){ # $1 = a table ROW, $2 = a key -> that key's value, or empty
  printf '%s' "$1" | cut -d'|' -f6- | tr '|' '\n' | sed -n "s/^$2=//p" | head -1
}
seat_row(){ printf '%s\n' "$SEAT_TABLE" | awk -F'|' -v n="$1" '$1==n'; }
seat_allowed_paths(){ # $1 = an allowed= ERE -> the LITERAL paths it names, one per line
  # ⚠ THE RULINGS ARE WRITTEN IN ONE SHAPE AND THAT SHAPE IS WHAT MAKES THEM ENUMERABLE.  An
  #   `allowed=` ERE in this table is either a single anchored path or the alternation-free form
  #   `^(pathA)?(pathB)?$` forced by the `awk -F'|'` parse.  Stripping the anchors and the parens and
  #   splitting on `?` therefore yields exactly the paths the ruling names -- which is what lets the
  #   STALE-RULING refusal below name a path the seat never touched, instead of reporting that some
  #   unnamed part of a regex matched nothing.
  printf '%s' "$1" | sed 's/^\^//; s/\$$//; s/[()]//g' | tr '?' '\n' | sed 's/\\//g' | grep -a . || true
}
alw_filter(){ # $1 = this seat's allowed= ERE (may be EMPTY).  stdin -> stdin MINUS the ruled paths.
  # ⚠ THE EMPTY CASE IS `cat`, NEVER `grep -avE ''`: an empty ERE matches EVERY line, so a seat with
  #   no ruling would have its ENTIRE diff subtracted and both gates below would read ZERO -- a gate
  #   that passes everything, reached by the one input that is overwhelmingly the common case.
  if [ -n "$1" ]; then grep -avE "$1" || true; else cat; fi
}
seat_allowed_rows(){ # the row NUMBERS that declare an allowed= ruling, scanning fields 6..NF
  printf '%s\n' "$SEAT_TABLE" | awk -F'|' '{for(i=6;i<=NF;i++) if ($i ~ /^allowed=/) {print $1; break}}'
}
# ⚠ ONE FUNCTION VALIDATES EVERY OPTIONAL FIELD, AND THE DERIVE-TIME SELF-CHECK EXTRACTS AND RUNS IT
#   OVER PLANTED TABLES.  A validator that is only ever run on the real table is a validator whose
#   refusals have never been seen to fire.
seat_opts_validate(){ # reads SEAT_TABLE -> 0 clean, 1 refused; every refusal STAMPED by row
  local bad=0 row n rest fld k v OLDIFS
  while IFS= read -r row <&4; do
    [ -n "$row" ] || continue
    n=${row%%|*}
    rest=$(printf '%s' "$row" | cut -d'|' -f6-)
    [ -n "$rest" ] || continue
    OLDIFS="$IFS"; IFS='|'; set -- $rest; IFS="$OLDIFS"
    for fld in "$@"; do
      [ -n "$fld" ] || continue
      case "$fld" in
        *=*) k=${fld%%=*}; v=${fld#*=} ;;
        *) stamp "  ^ SEAT TABLE REFUSED: row $n carries the optional field [$fld], which is not <key>=<value>.  Fields six and beyond are ORDER-FREE key=value pairs; a bare field here is most often an \`allowed=\` ERE carrying a \`|\` ALTERNATION, which splits into fields and would otherwise run a NARROWER exemption than the coordinator ruled."; bad=1; continue ;;
      esac
      case " $SEAT_OPT_KEYS " in
        *" $k "*) : ;;
        *) stamp "  ^ SEAT TABLE REFUSED: row $n declares the unknown option key [$k].  The known keys are [$SEAT_OPT_KEYS]; an unknown one is refused by NAME rather than ignored, because an option nobody reads is a ruling nobody applied."; bad=1; continue ;;
      esac
      if [ "$k" = "stack-on" ]; then
        case "$v" in
          ''|*[!0-9]*) stamp "  ^ SEAT TABLE REFUSED: row $n declares stack-on=[$v], which is not a row number.  A stack is declared against a ROW of this table, never against a SHA or a branch name."; bad=1 ;;
          *)
            if [ "$v" = "$n" ]; then
              stamp "  ^ SEAT TABLE REFUSED: row $n declares stack-on=$v -- a row cannot be stacked on ITSELF.  A self-reference would make the patch-id census declare every one of that row's own commits exempt from itself, which is an exemption that can never refuse."
              bad=1
            elif [ "$(seat_row "$v" | grep -ac . || true)" != "1" ]; then
              stamp "  ^ SEAT TABLE REFUSED: row $n declares stack-on=$v and NO row $v exists in this table.  A declaration naming a row nobody seated exempts nothing and hides the fact that it exempts nothing."
              bad=1
            elif [ "$v" -gt "$n" ]; then
              stamp "  ^ SEAT TABLE REFUSED: row $n declares stack-on=$v, which merges AFTER it.  THE ROW ORDER IS THE MERGE ORDER: a row built on another must merge second, or the union it was cut against is not the union it lands in."
              bad=1
            fi ;;
        esac
      fi
    done
  done 4<<< "$SEAT_TABLE"
  return $bad
}
# ⚠ AND A CLASS PLACEHOLDER IS ADMISSIBLE **ONLY WHILE ITS ROW IS PENDING**.  A class decides the
#   shape a seat is held to; a filled row carrying `PENDING-class` would reach merge_seat's
#   unknown-class arm seats deep, and the abort would name a class rather than a fill point.
seat_class_placeholders(){
  printf '%s\n' "$SEAT_TABLE" | awk -F'|' '$4=="PENDING-class" && $3!="PENDING"{print $1}'
}
PENDING_ROWS=$(printf '%s\n' "$SEAT_TABLE" | awk -F'|' '$3=="PENDING"{printf "%s(%s) ", $1, $2}')
PENDING_N=$(printf '%s\n' "$SEAT_TABLE" | awk -F'|' '$3=="PENDING"' | grep -ac . || true)
SEATS_LISTED_PRE=$(printf '%s\n' "$SEAT_TABLE" | grep -c .)

# --- ⚠ THE TABLE'S **STRUCTURE** IS ASSERTED, AND THE ROW COUNT IS NOT.  Train 46 carried
#     `[ "$SEATS_LISTED" = "5" ]` -- a literal that was a fact about the derive rather than about the
#     train, and run 1 refused on it AFTER merging all six seats.  Deriving the count from the table
#     and comparing it against itself would be a check that cannot fail (a self-consistent check
#     needs at least one input anchored to something else), so what is asserted here is what a
#     hand-edited table actually breaks and nothing else would see:
#       * the seat NUMBERS are 1..N contiguous, with no duplicate and no gap;
#       * every ref is unique, and every FILLED SHA is unique -- two rows naming one branch is a
#         table edited in place, and it would merge the same commit twice;
#       * every row has at least five fields (the loop refuses a row with no tip mode as well);
#       * and the count the loop accounts for equals the count read here (asserted after the loop).
#     The ANNOUNCEMENT match -- "is this the table the coordinator announced" -- is the coordinator's
#     at seat fill and the land script's afterwards; it is not a thing this file can know.
TBL_BAD=0
TBL_NUMS=$(printf '%s\n' "$SEAT_TABLE" | awk -F'|' '{print $1}')
TBL_EXPECT=$(seq 1 "$SEATS_LISTED_PRE" 2>/dev/null || awk -v n="$SEATS_LISTED_PRE" 'BEGIN{for(i=1;i<=n;i++) print i}')
if [ "$(printf '%s\n' "$TBL_NUMS")" != "$(printf '%s\n' "$TBL_EXPECT")" ]; then
  stamp "  ^ SEAT TABLE REFUSED: the seat numbers read [$(printf '%s' "$TBL_NUMS" | tr '\n' ' ')] where 1..$SEATS_LISTED_PRE contiguous is the only admissible sequence.  The ROW ORDER IS THE MERGE ORDER, so a duplicated or misnumbered row merges in an order nobody ruled on."
  TBL_BAD=1
fi
TBL_REFS=$(printf '%s\n' "$SEAT_TABLE" | awk -F'|' '{print $2}' | sort | uniq -d | grep -ac . || true)
TBL_SHAS=$(printf '%s\n' "$SEAT_TABLE" | awk -F'|' '$3!="PENDING"{print $3}' | sort | uniq -d | grep -ac . || true)
TBL_SHORT=$(printf '%s\n' "$SEAT_TABLE" | awk -F'|' 'NF<5' | grep -ac . || true)
stamp "SEAT TABLE :: $SEATS_LISTED_PRE row(s) in MERGE ORDER [$(printf '%s' "$TBL_NUMS" | tr '\n' ' ')] :: duplicate refs=$TBL_REFS (must be 0) :: duplicate filled SHAs=$TBL_SHAS (must be 0) :: rows with fewer than five fields=$TBL_SHORT (must be 0) :: PENDING rows=$PENDING_N [$PENDING_ROWS] requireAll=$REQUIRE_ALL"
{ [ "${TBL_REFS:-0}" = "0" ] && [ "${TBL_SHAS:-0}" = "0" ] && [ "${TBL_SHORT:-0}" = "0" ]; } || TBL_BAD=1
if [ "$(seat_class_placeholders | grep -ac . || true)" != "0" ]; then
  stamp "  ^ SEAT TABLE REFUSED: row(s) [$(seat_class_placeholders | tr '\n' ' ')] carry the class placeholder \`PENDING-class\` with a FILLED SHA.  The class is what decides the shape the seat is held to, so it is filled together with the ref and the SHA -- never after."
  TBL_BAD=1
fi
seat_opts_validate || TBL_BAD=1
[ "$TBL_BAD" = "0" ] || { stamp "SEAT TABLE ROWS ARE STRUCTURALLY UNSOUND -- NOTHING has been merged -- ABORT"; exit 2; }
stamp "SEAT TABLE ROWS ARE STRUCTURALLY SOUND :: numbers contiguous, refs unique, filled SHAs unique, every row five fields or more, every optional field a KNOWN key=value, every stack-on naming an EARLIER row that exists, and no filled row carrying a class placeholder"
STACKROWS=$(printf '%s\n' "$SEAT_TABLE" | awk -F'|' '{for(i=6;i<=NF;i++) if ($i ~ /^stack-on=/) {printf "%s(on %s) ", $1, substr($i,10); break}}')
stamp "SEAT TABLE DECLARED STACKS :: [${STACKROWS:-none}] -- a declared stack is a NARROWING of the patch-id census below and never a weakening: it can only exempt the shape git already collapses (one commit on two seats merges ONCE), and a cherry-pick DUPLICATE stays red whatever is declared."

# --- THE ALLOWED SET, READ OUT OF THE TABLE'S OPTIONAL SIXTH FIELD.  A7 consumes it; nothing else
#     does.  It is stamped HERE, before a single merge, so a reader meets the ruling before the
#     assembly acts on it -- an exemption nobody can see is an exemption nobody re-reads.
ALLOWED_ROWS=$(seat_allowed_rows | tr '\n' ' ')
ALLOWED_N=$(seat_allowed_rows | grep -ac . || true)
if [ "${ALLOWED_N:-0}" = "0" ]; then
  stamp "A7 ALLOWED SET :: NO row declares an \`allowed=\` set, so A7 runs in its STRICT form -- no committed behavioral file may be MODIFIED or DELETED except the four BehavioralTests classes.  That is the default and it is the state a reader should expect."
else
  stamp "A7 ALLOWED SET :: $ALLOWED_N row(s) [$ALLOWED_ROWS] declare an \`allowed=\` set.  ⚠ EACH IS A COORDINATOR RULING: a re-baselined golden is a RULING and never a merge.  A7 admits EXACTLY those paths AND only when the union's blob for each equals that seat's OWN blob, so the exemption cannot carry another seat's change."
  printf '%s\n' "$SEAT_TABLE" | awk -F'|' '{for(i=6;i<=NF;i++) if ($i ~ /^allowed=/) {printf "      row %s (%s) allows: %s\n", $1, $2, substr($i, 9); break}}' | tee -a "$ASMLOG"
fi

# ⚠ **THE PLACEHOLDER PREFLIGHT, RUN BEFORE THE WORKTREE IS TOUCHED.**  The seat loop already aborts
#   when it MEETS an unresolvable pin, but that refusal arrives seats deep and after merges have
#   landed in the worktree; this one arrives first.  Three arms, and they answer three questions:
#     (a) EVERY row whose SHA is not the literal `PENDING` must RESOLVE here.  A filled placeholder is
#         held to exactly the bar a named seat is held to, because an unresolved SHA and an INVENTED
#         one read the same.  ⚠ The fetch comes first: "unresolved" before a fetch is only a stale
#         clone, and after one it is a typo or an invention -- opposite severities.
#     (b) A row whose REF still carries the `claude/PENDING-` placeholder prefix while its SHA has
#         been FILLED is a REFUSAL: a filled SHA against a branch nobody named is a seat nobody can
#         check, and the shape/forbidden arms would then be measuring a ref that does not exist.
#     (c) An `allowed=` field on a row whose SHA is PENDING is a ruling about a seat that does not
#         exist.  It is REFUSED rather than ignored.
PLACEBAD=0
while IFS='|' read -r pn pb ps pc pm pa <&3; do
  [ -n "$pn" ] || continue
  # ⚠ READ THROUGH seat_opt, NEVER POSITIONALLY: `pa` came from a positional read and holds
  #   fields SIX AND BEYOND with their delimiters once a second optional key exists, so an
  #   `allowed=*` glob on it misses a ruling written in field seven -- and a missed ruling reads
  #   as the STRICT default, which is silence.
  case "$(seat_opt "$(seat_row "$pn")" allowed)" in
    ?*) [ "$ps" != "PENDING" ] || { stamp "  ^ SEAT PREFLIGHT REFUSED: row $pn declares an \`allowed=\` set while its SHA reads PENDING -- a ruling about a seat that does not exist yet."; PLACEBAD=1; } ;;
    ''|*) : ;;
  esac
  [ "$ps" = "PENDING" ] && continue
  git fetch -q origin "+refs/heads/$pb:refs/remotes/origin/$pb" < /dev/null 2>/dev/null
  if git cat-file -e "${ps}^{commit}" 2>/dev/null; then
    stamp "SEAT PREFLIGHT row $pn ($pb) :: pinned SHA $ps RESOLVES"
  else
    stamp "  ^ SEAT PREFLIGHT REFUSED: row $pn ($pb) is pinned at $ps, which does NOT resolve even after fetching that ref.  An unresolved SHA and an invented one read the same, so this is a refusal and never a 'no'."
    PLACEBAD=1
  fi
  case "$pb" in
    claude/PENDING-*) stamp "  ^ SEAT PREFLIGHT REFUSED: row $pn carries a FILLED SHA ($ps) against the PLACEHOLDER ref [$pb].  Fill the REF as well as the SHA -- a seat whose branch nobody named cannot have its class shape or its forbidden paths measured at all."; PLACEBAD=1 ;;
  esac
done 3<<< "$SEAT_TABLE"
[ "$PLACEBAD" = "0" ] || { stamp "SEAT TABLE REFUSED :: at least one filled row does not resolve, a filled row still names a placeholder ref, or a PENDING row carries a ruling.  NOTHING has been merged -- ABORT"; exit 2; }

# ============================================================================================
# THE **PATCH-ID ARM** -- A CHERRY-PICK DUPLICATION CENSUS OVER THIS TRAIN'S SEATS, RUN AFTER THE
# SEAT PREFLIGHT AND BEFORE ANY MERGE.
# ⚠ WHY ANCESTRY CANNOT DO THIS.  A lane that develops an item on its own branch and CHERRY-PICKS it
#   onto a seat gives the same content a NEW SHA.  Every ancestry check then reads clean --
#   `merge-base --is-ancestor` is false, `git log <base>..<seat>` lists a commit nobody recognises --
#   right up until the train lands the same diff twice.  `git patch-id --stable` hashes the DIFF, so
#   it is invariant across the cherry-pick, the rebase and the committer, which is exactly the axis
#   ancestry is blind to.
# ⚠ THE TOOL IS READ OUT OF ITS OWN SEAT'S PINNED BLOB.  It is not a copy kept beside this script and
#   it is not the working tree's version: both would let the instrument and the seat drift, and the
#   seat is the thing that defines the instrument.  ⚠ AND IF THAT SEAT IS NOT ON THIS TABLE THE ARM
#   IS **UNMEASURED AND FAILS** -- an instrument that could not run has not found nothing.
# ⚠ THE SELF-TEST RUNS FIRST AND GATES THE VERDICT.  A tool whose own controls fail measures nothing,
#   so its reading about this train's seats is not consulted at all in that case.
# ⚠ EVERY READ OF ITS OUTPUT IS AN **ANCHORED ROW MATCH** (L15): `^==> CENSUS`, `^DUPLICATE patch-id `,
#   `^UNDECLARED STACK: `, `^declared-stack SHA `, `^SELF-TEST CLEAN -- `.  The tool prints those
#   phrases as ROW headers and also inside its own count lines, and a substring match cannot tell a
#   verdict from a tally.
# ============================================================================================
PIDROW=$(printf '%s\n' "$SEAT_TABLE" | awk -F'|' '$2=="claude/c1-seat-duplication-census"{print $1}' | head -1)
PIDSHA=""
[ -n "$PIDROW" ] && PIDSHA=$(seat_field "$PIDROW" 3)
PIDDIR="$SCRIPT_DIR/coord-${TAG}-legD-patchid-$RUNID"
if [ -z "$PIDROW" ] || [ -z "${PIDSHA:-}" ] || [ "$PIDSHA" = "PENDING" ]; then
  stamp "PATCH-ID ARM UNMEASURED :: the census tool's seat is not on this table (row=[${PIDROW:-absent}] pin=[${PIDSHA:-absent}]).  The arm reads src/seat-duplication-census.sh out of that row's OWN pinned blob, so without the row there is no instrument -- and an UNMEASURED arm is NOT a pass."
  fail_gate PATCH-ID-ARM-UNMEASURED
else
  mkdir -p "$PIDDIR"
  PIDTOOL="$PIDDIR/seat-duplication-census.sh"
  if ! git show "${PIDSHA}:src/seat-duplication-census.sh" > "$PIDTOOL" 2>"$PIDDIR/show.err"; then
    stamp "PATCH-ID ARM UNMEASURED :: src/seat-duplication-census.sh could not be read out of row $PIDROW's pin $PIDSHA -- [$(head -2 "$PIDDIR/show.err" | tr '\n' ' ')].  The pin resolved at the SEAT PREFLIGHT, so this is the FILE being absent from that commit rather than the commit being absent."
    fail_gate PATCH-ID-ARM-UNMEASURED
  else
    stamp "PATCH-ID ARM tool :: src/seat-duplication-census.sh read from row $PIDROW's pinned $PIDSHA into $PIDTOOL ($(grep -ac . "$PIDTOOL" || true) line(s))"
    PIDST="$PIDDIR/self-test.log"
    stamp "PATCH-ID ARM command :: bash $PIDTOOL --self-test"
    bash "$PIDTOOL" --self-test > "$PIDST" 2>&1 < /dev/null; pidstrc=$?
    PIDARMLINE=$(grep -aE '^SELF-TEST CLEAN -- ' "$PIDST" | tail -1)
    PIDOKN=$(grep -acE '^  ok   ' "$PIDST" || true)
    stamp "PATCH-ID ARM self-test exit=$pidstrc :: verdict=[${PIDARMLINE:-NO ANCHORED VERDICT LINE}] :: arms that reported ok=$PIDOKN"
    if [ "$pidstrc" != "0" ] || [ -z "$PIDARMLINE" ]; then
      stamp "  ^ PATCH-ID ARM REFUSED: the census tool's OWN self-test did not pass (exit=$pidstrc, verdict line present=$( [ -n "$PIDARMLINE" ] && echo yes || echo no )).  A tool whose controls fail measures nothing, so its verdict about this train's seats is NOT consulted.  The head of its output:"
      sed -n '1,25p' "$PIDST" | cut -c1-200 | sed 's/^/    /' | tee -a "$ASMLOG"
      fail_gate PATCH-ID-ARM-SELFTEST
    else
      # --- the census itself.  PENDING rows carry no resolvable ref and are EXCLUDED BY NAME, because
      #     a census run over a set it could not resolve would refuse as MISUSE and read as a finding.
      PIDARGS=(--base "$BASE")
      PIDSTACKN=0; PIDSKIP=""
      while IFS= read -r prow <&6; do
        [ -n "$prow" ] || continue
        pnum=${prow%%|*}
        pso=$(seat_opt "$prow" stack-on)
        [ -n "$pso" ] || continue
        pcs=$(seat_field "$pnum" 3); pps=$(seat_field "$pso" 3)
        if [ "$pcs" = "PENDING" ] || [ "$pps" = "PENDING" ]; then
          stamp "PATCH-ID ARM :: row $pnum declares stack-on=$pso but one side is PENDING (child=$pcs parent=$pps) -- the declaration is NOT passed, so if both rows later fill without it the census refuses them as an UNDECLARED STACK rather than passing them silently."
          continue
        fi
        PIDARGS+=(--stack "$pcs:$pps"); PIDSTACKN=$(( PIDSTACKN + 1 ))
      done 6<<< "$SEAT_TABLE"
      PIDSEATN=0
      while IFS= read -r prow <&6; do
        [ -n "$prow" ] || continue
        pnum=${prow%%|*}; psha=$(seat_field "$pnum" 3)
        if [ "$psha" = "PENDING" ]; then PIDSKIP="$PIDSKIP $pnum"; continue; fi
        PIDARGS+=("$psha"); PIDSEATN=$(( PIDSEATN + 1 ))
      done 6<<< "$SEAT_TABLE"
      PIDCEN="$PIDDIR/census.log"
      stamp "PATCH-ID ARM coverage :: $PIDSEATN row(s) censused, $PIDSTACKN declared stack(s) passed, PENDING row(s) EXCLUDED=[${PIDSKIP:- none}] -- ⚠ this arm's verdict is about the rows it was GIVEN; a PENDING row that later fills has not been censused by this run."
      stamp "PATCH-ID ARM command :: bash $PIDTOOL ${PIDARGS[*]}"
      bash "$PIDTOOL" "${PIDARGS[@]}" > "$PIDCEN" 2>&1 < /dev/null; pidrc=$?
      PIDVERD=$(grep -aE '^==> CENSUS (CLEAN|RED)' "$PIDCEN" | tail -1)
      PIDDUP=$(grep -acE '^DUPLICATE patch-id ' "$PIDCEN" || true)
      PIDSTK=$(grep -acE '^UNDECLARED STACK: ' "$PIDCEN" || true)
      PIDDECL=$(grep -acE '^declared-stack SHA ' "$PIDCEN" || true)
      case "$pidrc" in
        0)
          stamp "PATCH-ID ARM CLEAN :: exit=0 :: [${PIDVERD:-NO ANCHORED VERDICT LINE}] :: declared-stack exemptions the tool PRINTED=$PIDDECL (an exemption nobody can see is an exemption nobody re-reads)"
          if [ -z "$PIDVERD" ]; then
            stamp "  ^ PATCH-ID ARM REFUSED: exit 0 with no anchored CENSUS verdict row.  A zero exit and a verdict are two claims, and a tool that exited clean without printing one has not said what it measured."
            fail_gate PATCH-ID-ARM-NOVERDICT
          fi ;;
        1)
          stamp "  ^ PATCH-ID ARM REFUSED :: exit=1 :: [${PIDVERD:-NO ANCHORED VERDICT LINE}] :: cherry-pick duplicate row(s)=$PIDDUP undeclared stack row(s)=$PIDSTK.  Every NAMED row, verbatim:"
          grep -aE '^(DUPLICATE patch-id |UNDECLARED STACK: )' "$PIDCEN" | cut -c1-200 | sed 's/^/    /' | tee -a "$ASMLOG"
          grep -aE '^   [^ ]' "$PIDCEN" | head -40 | cut -c1-200 | sed 's/^/      /' | tee -a "$ASMLOG"
          stamp "      ⚠ THE TWO FINDINGS HAVE TWO REMEDIES.  A DUPLICATE patch-id is the same content boarding twice under two names -- no declaration excuses it, and one of the two rows comes off the table.  An UNDECLARED STACK is one commit on two seats, which git merges once: declare it with stack-on=<row> in the child's table row, or split it."
          fail_gate PATCH-ID-ARM ;;
        2)
          stamp "  ^ PATCH-ID ARM MISUSE :: exit=2 -- the tool refused its own invocation, so NOTHING was censused.  That is a fault in this arm's arguments (fewer than two resolvable seats, or a ref the tool could not resolve), not a finding about the seats:"
          head -8 "$PIDCEN" | cut -c1-200 | sed 's/^/    /' | tee -a "$ASMLOG"
          fail_gate PATCH-ID-ARM-MISUSE ;;
        *)
          stamp "  ^ PATCH-ID ARM UNMEASURED :: exit=$pidrc, which the tool defines no meaning for.  An exit this arm cannot read is not a pass:"
          head -8 "$PIDCEN" | cut -c1-200 | sed 's/^/    /' | tee -a "$ASMLOG"
          fail_gate PATCH-ID-ARM-UNMEASURED ;;
      esac
    fi
  fi
fi

if [ "$PENDING_N" != "0" ]; then
  if [ "$SKIP_PENDING" != "1" ]; then
    stamp "SEAT TABLE REFUSED :: $PENDING_N row(s) read PENDING [$PENDING_ROWS] and TRAIN48_SKIP_PENDING is not 1.  With a FILLED table the default is to REFUSE: a run that quietly assembles some of the announced rows produces an internally consistent record of a train nobody announced.  Fill the rows, or opt into a PARTIAL train BY NAME with TRAIN48_SKIP_PENDING=1 -- ABORT"
    exit 2
  fi
  if [ "$REQUIRE_ALL" = "1" ]; then
    stamp "SEAT TABLE REFUSED :: $PENDING_N row(s) read PENDING [$PENDING_ROWS] and TRAIN48_REQUIRE_ALL=1 demands the full train.  Fill the table, or drop the switch and accept a PARTIAL train that says so -- ABORT"
    exit 2
  fi
  stamp "⚠⚠ SEAT TABLE :: $PENDING_N row(s) read PENDING [$PENDING_ROWS] and will be SKIPPED WITH A STAMP.  THIS RUN IS A PARTIAL TRAIN.  Every count below -- SEATS_DONE, the OWED vector, the new behavioral projects, LEG 1's registration delta, LEG 5's enumeration, LEG D's prediction and LEG K's row set -- is DERIVED at run time, so this run's numbers are internally consistent; they are simply not the announced train's."
fi

# --- THE SEAT NOTES ARE **DERIVED**, NOT WRITTEN.  Train 46's notes named each seat's deliverable in
#     prose, which is a fact about that train and the stale-figure class the moment a row is re-pinned.
#     What a template can honestly stamp is what the TABLE says, plus what the CLASS implies.
printf '%s\n' "$SEAT_TABLE" | awk -F'|' '{o=""; for(i=6;i<=NF;i++){ if ($i ~ /^allowed=/) o=o" :: carries an A7 ALLOWED ruling"; if ($i ~ /^stack-on=/) o=o" :: DECLARED STACK on row "substr($i,10) } printf "SEAT %s NOTE :: %s @%s :: class %s, tip mode %s%s\n", $1, $2, $3, $4, $5, o}' | while IFS= read -r ln; do stamp "$ln"; done

# THE BEHAVIORAL PROJECTS WHOSE EMISSION CARRIES THE RUNTIME PACKAGE'S Δ-PREFIXED ALIAS.
# ⚠ WHAT THE LIST IS: these eight emissions CARRY the alias, that is an invariant a converter seat can
#   break, and LEG 0 and LEG 5 both read it OUT OF THE EMISSION rather than inferring it from a green.
#   It CAN go red, so it is not a green that cannot fail.  It is NOT a diagnosis for a failed pairing:
#   the alias fold makes the emission PIN-INDEPENDENT, so a Compile+Target red on one of the eight is
#   a finding about a SEAT like any other, and the seat that changes converter source is where to look.
GOLDEN_PROJECTS='FuncForPCName
FuncLiteralCallerNames
GoexitDefers
GoroutineWaitState
IterPullRendezvous
RuntimeCallerFrames
SetFinalizerBridge
SyscallKeystonePulls'

# The alias pair itself, as the two literal emitted lines.  ⚠ WRITTEN AS DATA, not built in a command
# string: the glyph and the C# text both survive a Write/Edit but not necessarily a shell round trip.
ALIAS_OLD='using Δruntime = runtime_package;'
ALIAS_NEW='using runtime = runtime_package;'

merge_seat(){ # $1 branch, $2 required sha (prefix ok), $3 seat number, $4 class, $5 tipmode
  local b="$1" want="$2" n="$3" cls="$4" mode="$5" got gotshort tip m c f seen listed forbidden bad ahead mrc mb shapebad shapepat
  local alw alwn alwlist alwp alwbu alwbs dpaths
  git fetch -q origin "+refs/heads/$b:refs/remotes/origin/$b" < /dev/null || { stamp "seat $n fetch $b FAILED (does the ref exist on the remote? git ls-remote origin refs/heads/$b) -- ABORT"; return 1; }
  got=$(git rev-parse "origin/$b"); gotshort=$(git rev-parse --short "origin/$b")
  if ! git cat-file -e "${want}^{commit}" 2>/dev/null; then
    stamp "seat $n $b :: the pinned SHA $want is NOT PRESENT locally even after the fetch -- it was force-pushed away, or it was mistyped.  An unresolved SHA and an invented one read the same, so this is a REFUSAL -- ABORT"
    return 1
  fi
  case "$mode" in
    tip)
      case "$got" in
        "$want"*) : ;;
        *)
          stamp "seat $n $b tip is $got -- REQUIRED $want (tip mode) -- ABORT (the seat moved after it was pinned)"
          if git merge-base --is-ancestor "$want" "origin/$b" 2>/dev/null; then
            ahead=$(git rev-list --count "${want}..origin/$b")
            stamp "  DIAGNOSIS: the pinned SHA IS an ancestor of the tip -- the seat GREW by $ahead commit(s) after it was pinned.  RE-PIN the seat list; do not force.  The added commits:"
            git log --oneline "${want}..origin/$b" | sed 's/^/      /' | tee -a "$ASMLOG"
          else
            stamp "  DIAGNOSIS: the pinned SHA is NOT an ancestor of the tip -- the branch was REWRITTEN, or the pin names a different branch.  Ask the owner before re-pinning."
          fi
          return 1 ;;
      esac
      stamp "seat $n ($cls, tip mode) $b :: branch tip == pinned SHA $want"
      ;;
    sha)
      if ! git merge-base --is-ancestor "$want" "origin/$b" 2>/dev/null; then
        stamp "seat $n $b :: the pinned SHA $want is NOT an ancestor of the branch tip $gotshort -- the branch was REWRITTEN, or the pin names a different branch -- ABORT"
        return 1
      fi
      ahead=$(git rev-list --count "${want}..origin/$b")
      stamp "seat $n ($cls, SHA mode) $b :: pinned SHA $want is on the branch; tip is $gotshort, $ahead commit(s) BEYOND it, DELIBERATELY NOT CARRIED.  Excluded commits by name:"
      if [ "$ahead" = "0" ]; then
        stamp "      (none -- the branch tip EQUALS the pinned SHA.  A READING, not an error, but reconcile it: a SHA-mode seat whose tip equals its pin is indistinguishable from a tip-mode seat and the split it encodes may be gone.)"
      else
        git log --oneline "${want}..origin/$b" | sed 's/^/      /' | tee -a "$ASMLOG"
      fi
      ;;
    *) stamp "seat $n: unknown tip mode '$mode' -- ABORT"; return 1 ;;
  esac
  tip="$(git rev-parse --short "$want")"
  mb="$(git merge-base HEAD "$want")"

  # --- per-seat forbidden path set + the SHAPE assertion that actually pins the seat.  The forbidden
  #     list alone is too loose for every class here, which is why both run.
  #     ⚠ EVERY PATH READ IS TAKEN WITH `-c core.quotepath=false`.  A golib row's files can carry a
  #     non-ASCII glyph; with quotepath on, git returns an octal-escaped QUOTED path, the shape
  #     pattern refuses it, and the refusal reads like a seat defect rather than an instrument one.
  # ⚠ EACH ARM IS **ONE LINE**, `forbidden=` then `shapepat=`, and that layout is load-bearing rather
  #   than cosmetic: the derive-time self-check reads both values OUT OF THIS CASE BLOCK with a
  #   single-line sed, so a two-line arm makes its extractor return the line unchanged, every path
  #   then reads "outside shape", and the checker reports a correct seat as refused.  A retyped copy
  #   of a pattern is the thing that drifts, so the checker reads the live one -- and this is the
  #   layout it reads.
  case "$cls" in
    # A CONVERTER + BEHAVIORAL GUARD seat: converter source, the solution file, a behavioral guard
    # project (which may carry NESTED sub-libraries), the four MSTest classes, and a finding record.
    # ⚠ THE BEHAVIORAL ALTERNATIVE IS `[A-Za-z0-9]+/.*` RATHER THAN A NAMED PROJECT, because a guard
    #   may carry sub-libraries under its own directory and a per-project literal would have to be
    #   re-derived for every future seat -- while `BehavioralTests/[^/]+\.cs` stays a SEPARATE
    #   alternative so the four MSTest classes are admitted by name and nothing else under that
    #   directory is.  The forbidden list keeps src/core and src/gen: a converter-guard seat must
    #   never touch the corpus or the generator.
    converter-guard)      forbidden='src/core src/gen src/utilities'; shapepat='^(src/go2cs/[^/]+|src/go2cs\.slnx|src/tests/Behavioral/[A-Za-z0-9]+/.*|src/tests/Behavioral/BehavioralTests/[^/]+\.cs|docs/phase4/[^/]+\.md)$' ;;
    # ⚠ DERIVED FOR THIS TRAIN (2026-09-13), and `converter-guard` is UNTOUCHED beside it.  SAME SHAPE,
    # SAME FORBIDDEN LIST, ONE DIFFERENCE THAT IS NOT ABOUT SHAPE AT ALL: this class does **NOT** set
    # OWED_BEH.  A `converter-guard` seat ADDS a guard project, and A5 reads OWED_BEH=1 against the
    # count of NEW TOP-LEVEL behavioral projects -- so a converter seat that RE-EMITS AN EXISTING
    # guard (row 8: one converter source plus seven MODIFIED files under one existing project, zero
    # additions) is refused by A5 "owed and absent" on a HEALTHY tree.  That is precisely the carried-
    # premise class A5's own comment was rewritten to kill, so the fix is a SEPARATE arm rather than a
    # weakened A5: the shape stays exactly as tight, and G11(a) prints src/tests/Behavioral as a
    # not-owed READING (it can only refuse when owed=1 and measured=0, never the other way).
    # ⚠ IT DOES NOT AND CANNOT EXEMPT A7.  A row of this class modifies committed behavioral files by
    # definition, and A7's strict form refuses that; the exemption is an `allowed=` ERE in the ROW's
    # sixth field under a COORDINATOR RULING, which is where a re-baselined golden belongs.
    converter-guard-rebaseline) forbidden='src/core src/gen src/utilities'; shapepat='^(src/go2cs/[^/]+|src/go2cs\.slnx|src/tests/Behavioral/[A-Za-z0-9]+/.*|src/tests/Behavioral/BehavioralTests/[^/]+\.cs|docs/phase4/[^/]+\.md)$' ;;
    # A CONVERTER seat that RE-MINTS CORPUS METADATA.  A converter change that moves what
    # `package_info.cs` or a generated `.csproj` says lands its corpus footprint in the SAME train --
    # a registration split from its footprint is the syscall.Uname silent-subtraction class -- so the
    # shape admits the metadata files and NOTHING ELSE under src/core.  ⚠ THE FORBIDDEN LIST USES
    # `:(exclude)` PATHSPECS: the seat legitimately lives INSIDE src/core, so a bare `src/core` would
    # refuse it and omitting src/core would forbid nothing there at all.
    converter)            forbidden='src/gen src/tests src/utilities src/core :(exclude)src/core/**/package_info.cs :(exclude)src/core/**/*.csproj'; shapepat='^(src/go2cs/[^/]+|src/go2cs\.slnx|src/core/[A-Za-z0-9_./+-]+/package_info\.cs|src/core/[A-Za-z0-9_./+-]+\.csproj|docs/phase4/[^/]+\.md)$' ;;
    # A CONVERTER-SIDE INSTRUMENT with its own guards and a roster-side consumer -- the shape the
    # orphan-disclosure check takes: converter source and its `_test.go` guards, the VS shared-project
    # item list a new converter source must be registered in, the roster format guard, and a record.
    # ⚠ `src/go2cs/[^/]+` admits `go2cs-src.projitems` by construction; it is named here because a NEW
    #   converter .go file that is not registered there is invisible at the command line and bites
    #   only in Visual Studio.
    converter-test+docs)  forbidden='src/core src/gen src/tests src/utilities'; shapepat='^(src/go2cs/[^/]+|src/check-roster-format\.ps1|docs/phase4/[^/]+\.md)$' ;;
    # A DISCLOSURE MANIFEST seat: ONE package's committed manifest.  ⚠ A manifest is a STRUCTURED file
    # and a REMOVAL from it is a cross-platform edit whatever evidence motivated it, so the shape is
    # deliberately the tightest in this file: one path pattern, nothing else.
    manifest)             forbidden='src/go2cs src/gen src/tests src/utilities docs'; shapepat='^src/core/[A-Za-z0-9_./+-]+/go2cs_test_disclosures\.json$' ;;
    # A DOCTRINE BATCH: CLAUDE.md alone.  G4 has teeth for this class -- a batch is a PURE INSERTION,
    # so zero deletions, zero table lines, the BOM state preserved and CR == LF.
    doctrine)             forbidden='src docs'; shapepat='^CLAUDE\.md$' ;;
    # A GOLIB seat: the runtime library, its GolibTests arms and a record.  ⚠ EXCLUDE-PATHSPEC
    # forbidden list for the same reason as `converter`: the seat lives inside src/core.
    golib)                forbidden='src/go2cs src/gen src/tests/Behavioral src/utilities src/core :(exclude)src/core/golib'; shapepat='^(src/core/golib/.*\.cs|src/tests/GolibTests/[^/]+\.cs|docs/phase4/[^/]+\.md)$' ;;
    # GOLIB + the GENERATOR + the converter + a behavioral guard.  The widest class here, and every
    # alternative is one such a seat's measured diff needs.  ⚠ FALSE-GREEN ROUTE #7 IS LIVE FOR IT:
    # a src/gen change is invisible to CNR (transpile-only) and to the stdlib solution (one assembly
    # at a time), so LEG 5's FULL behavioral COMPILE and LEG 2b's go2cs.slnx build are its gates.
    golib-gen)            forbidden='docs src/utilities src/core :(exclude)src/core/golib src/gen :(exclude)src/gen/go2cs-gen/Templates'; shapepat='^(src/core/golib/.*\.cs|src/gen/go2cs-gen/Templates/.*\.cs|src/go2cs\.slnx|src/go2cs/[^/]+|src/tests/Behavioral/[A-Za-z0-9]+/.*|src/tests/Behavioral/BehavioralTests/[^/]+\.cs|src/tests/(GolibTests|GenericTests)/[^/]+\.(cs|csproj))$' ;;
    # A GOLIB PRIMITIVE with corpus consumers: hand-owns, the emitted file a registry displaces, the
    # per-GOOS package_info.cs a displacement re-stamps, the converter REGISTRY file, a GolibTests arm
    # and a record.  ⚠ `src/core/runtime/([a-z]+/)?[^/]+\.cs` admits BOTH the flat files and the
    # per-GOOS folders, which is layout L3's shape and not a widening.
    golib-corpus-handown) forbidden='src/gen src/tests/Behavioral src/utilities'; shapepat='^(docs/phase4/[^/]+\.md|src/core/golib/([a-z]+/)?[^/]+\.cs|src/core/runtime/([a-z]+/)?[^/]+\.cs|src/core/sync/[^/]+\.cs|src/go2cs/[^/]+|src/tests/GolibTests/[^/]+\.cs)$' ;;
    # GOLIB + CONVERTER + a record, with a docs/phase4 PROBE TREE admitted: `docs/phase4/probes/<name>/...`
    # is an EXISTING repository convention, so admitting it states a convention rather than a violation.
    golib-converter-docs) forbidden='src/gen src/tests/Behavioral src/utilities src/core/runtime'; shapepat='^(docs/phase4/[^/]+\.md|docs/phase4/probes/[^/]+/.*|src/core/golib/.*\.cs|src/go2cs/[^/]+|src/tests/GolibTests/[^/]+\.cs)$' ;;
    docs)                 forbidden='src';                                                 shapepat='^docs/.*\.md$' ;;
    # ⚠ DERIVED FOR THIS TRAIN (2026-09-13), and `converter` is UNTOUCHED beside it.  Row 8's measured
    # diff carries TWO `src/core/**/README.md` alongside the package_info.cs and .csproj files the
    # converter re-mints -- a README under src/core is converter-MINTED PACKAGE METADATA of exactly the
    # same kind, and it is not a corpus SOURCE.  Widening `converter` would have loosened the gate for
    # row 8 as well, which is the shape doctrine forbids, so the admission is a SEPARATE arm: this one
    # admits the three metadata spellings and NOTHING ELSE under src/core, and its forbidden list
    # excludes exactly those three and no more.
    converter-corpus-metadata) forbidden='src/gen src/tests src/utilities src/core :(exclude)src/core/**/package_info.cs :(exclude)src/core/**/*.csproj :(exclude)src/core/**/README.md'; shapepat='^(src/go2cs/[^/]+|src/go2cs\.slnx|src/core/[A-Za-z0-9_./+-]+/package_info\.cs|src/core/[A-Za-z0-9_./+-]+\.csproj|src/core/[A-Za-z0-9_./+-]+/README\.md|docs/phase4/[^/]+\.md)$' ;;
    # ⚠ DERIVED FOR THIS TRAIN (2026-09-13), and `docs` is UNTOUCHED beside it.  Row 5 lands
    # `docs/phase4/h5-removals.txt` -- a docs-only DATA file, which `^docs/.*\.md$` refuses.  The real
    # gate in the docs class is its forbidden list (`src`), and that is carried here unchanged; only
    # the extension is widened, and only for the row that was ruled to carry one.
    docs-data)            forbidden='src';                                                 shapepat='^docs/.*\.(md|txt)$' ;;
    # ⚠ DERIVED FOR TRAIN 48, and `converter-test+docs` is UNTOUCHED beside it.  A guard-only row:
    # converter source, its `_test.go` guards and the projitems a new converter source must be
    # registered in, and NOTHING else -- no record, no roster script.  It is the tightest shape a row
    # that adds only a guard needs, and a row of this class whose real diff carries a tool script is
    # RE-CLASSED in the TABLE rather than admitted by widening this arm.
    converter-test)       forbidden='src/core src/gen src/tests src/utilities docs'; shapepat='^src/go2cs/[^/]+$' ;;
    # ⚠ DERIVED FOR TRAIN 48.  Converter source and its guards PLUS a repo TOOL SCRIPT under src/ (a
    # `*.sh` or `*.ps1` at the src/ root, or anything under src/utilities) plus a record.  It owes LEG
    # C **and** G2: a PowerShell change that no leg parses is a change no gate on this train sees, and
    # G2 is the both-edition parse.  ⚠ `src/[^/]+\.(sh|ps1)` is deliberately ROOT-ONLY -- a script
    # buried under src/core or src/tests is a corpus or harness file wearing a tool's extension.
    converter-test+tooling) forbidden='src/core src/gen src/tests'; shapepat='^(src/go2cs/[^/]+|src/[A-Za-z0-9_.-]+\.(sh|ps1)|src/utilities/.*|docs/phase4/[^/]+\.md)$' ;;
    # ⚠ DERIVED FOR TRAIN 48.  A tool script and a record, and NO converter source at all: the OWED
    # vector therefore carries tooling=1 and converter=0 for this class, which is what keeps G11(a)'s
    # converter arm a READING rather than a false refusal on a train whose only tool row is this one.
    tooling)              forbidden='src/core src/gen src/tests src/go2cs'; shapepat='^(src/[A-Za-z0-9_.-]+\.(sh|ps1)|src/utilities/.*|docs/phase4/[^/]+\.md)$' ;;
    *)           stamp "seat $n: unknown class '$cls' -- ABORT"; return 1 ;;
  esac
  # shellcheck disable=SC2086
  # --- ⚠ **THE PER-ROW `allowed=` RULING, READ HERE AND SUBTRACTED FROM BOTH READINGS BELOW.**
  #     COORD ruling 2026-09-13 19:05 (D5), extending the 18:20 ruling on G 7a947650f.  It is read
  #     with the assembler's OWN reader -- seat_opt over the row's fields six and beyond -- and never
  #     positionally: with two optional keys a positional read welds `|stack-on=NN` onto the ERE, and
  #     an alternation is a WIDENING nobody wrote.
  #     ⚠ THE SUBTRACTION IS COUNTED AND LISTED IN A STAMP.  A path removed from a gate's reading
  #     without a stamp naming it is a gate no reader can audit, and the exemption would be invisible
  #     in exactly the log a later reader uses to decide the train was clean.
  #     ⚠ A RULED PATH THE SEAT DOES NOT TOUCH IS A **STALE RULING** AND ABORTS BY NAME.  An exemption
  #     for a path that is not in the diff exempts nothing and hides that it exempts nothing -- the
  #     same defect class as a `stack-on=` naming a row nobody seated, and it is refused the same way.
  alw="$(seat_opt "$(seat_row "$n")" allowed)"
  alwn=0; alwlist=""
  if [ -n "$alw" ]; then
    dpaths=$(git -c core.quotepath=false diff --name-only "$mb" "$want")
    while IFS= read -r alwp <&5; do
      [ -n "$alwp" ] || continue
      if [ "$(printf '%s\n' "$dpaths" | grep -acxF -- "$alwp" || true)" = "0" ]; then
        stamp "seat $n ($cls) $b :: **STALE RULING** -- allowed= names [$alwp] and this seat's REAL diff does not touch it.  An exemption for a path the seat never changes exempts nothing and hides that it exempts nothing.  Re-read the ruling against the seat, or drop the path from it -- ABORT"
        return 1
      fi
    done 5<<< "$(seat_allowed_paths "$alw")"
    alwn=$(printf '%s\n' "$dpaths" | grep -aE "$alw" | grep -ac . || true)
    alwlist=$(printf '%s\n' "$dpaths" | grep -aE "$alw" | tr '\n' ' ')
    stamp "seat $n ($cls) $b :: allowed= RULING IN FORCE [$alw] :: allowedByRuling=$alwn :: paths [$alwlist] -- SUBTRACTED from the forbidden reading and from the shape reading below, and each asserted BLOB-IDENTICAL to this seat's own blob after the merge."
  fi
  # shellcheck disable=SC2086   (⚠ REPEATED DELIBERATELY: the directive applies to the NEXT command,
  #   and the block above now sits between `bad=` and the copy train 47 left there.  Two directives
  #   is noise; a directive that has silently stopped covering the word-split it was written for is a
  #   lint nobody re-argued.)
  bad=$(git -c core.quotepath=false diff --name-only "$mb" "$want" -- $forbidden | alw_filter "$alw" | grep -ac . || true)
  if [ "$bad" != "0" ]; then
    stamp "seat $n ($cls) $b touches a FORBIDDEN path ($bad file(s) under: $forbidden) -- ABORT"
    # shellcheck disable=SC2086
    git -c core.quotepath=false diff --name-only "$mb" "$want" -- $forbidden | alw_filter "$alw" | sed 's/^/    /' | tee -a "$ASMLOG"
    return 1
  fi
  stamp "seat $n ($cls) $b @$tip :: forbidden-path files=0 (checked: $forbidden) :: allowedByRuling=$alwn [$alwlist]"

  if [ -n "$shapepat" ]; then
    shapebad=$(git -c core.quotepath=false diff --name-only "$mb" "$want" | alw_filter "$alw" | grep -avE "$shapepat" | grep -ac . || true)
    if [ "${shapebad:-0}" != "0" ]; then
      stamp "seat $n ($cls) $b changes $shapebad path(s) OUTSIDE its class shape [$shapepat] -- ABORT"
      git -c core.quotepath=false diff --name-only "$mb" "$want" | alw_filter "$alw" | grep -avE "$shapepat" | sed 's/^/    /' | tee -a "$ASMLOG"
      return 1
    fi
    stamp "seat $n ($cls) shape OK :: every changed path matches $shapepat :: allowedByRuling=$alwn [$alwlist] (ruled paths are admitted OUTSIDE the shape and nowhere else)"
  fi

  stamp "seat $n file list (numstat):"
  git -c core.quotepath=false diff --numstat "$mb" "$want" | sed 's/^/      /' | tee -a "$ASMLOG"

  # ⚠ NO SEAT IN THIS TRAIN CARRIES A MERGE-MESSAGE BODY.  Train 44 carried one (R ad83d04a3's
  #   correction to a seated record) and asserted it landed VERBATIM.  Nothing here owes one, so the
  #   mechanism is REMOVED rather than left as an empty branch that could never fire.

  # MERGE THE SHA, not the branch: a seat is a commit, and merging origin/$b would carry anything the
  # branch has grown since.  The tip-equality check above is what keeps the two spellings honest.
  git merge --no-ff --no-edit "$want" -m "Merge $b ($tip) -- ${LABEL} seat $n.

Co-Authored-By: Claude Fable 5.1 <noreply@anthropic.com>" >> "$MERGELOG" 2>&1 < /dev/null
  mrc=$?   # captured as the FIRST statement after the command: anything between -- a $(...) in the
           # test line included -- resets $?, which this repository has already paid for.
  if [ "$mrc" != 0 ]; then
    # ⚠ A FAILURE BRANCH PRINTS WHAT IT **OBSERVED**, NEVER WHAT IT ASSUMES.  A `git merge` can fail
    #   for reasons that are not conflicts at all -- a bad `-F` path whose last field carried a
    #   trailing CR produced a "CONFLICT" report with ZERO unmerged paths and cost a published
    #   estimate -- so the unmerged set is listed first and its EMPTINESS is called out by name.
    UNM=$(git diff --name-only --diff-filter=U | grep -ac . || true)
    stamp "seat $n MERGE FAILED for $b :: exit=$mrc unmergedPaths=$UNM -- ⚠ NONE means it was NOT a conflict (read $MERGELOG for what it actually was)"
    git diff --name-only --diff-filter=U | sed 's/^/    /' | tee -a "$ASMLOG"
    if [ "${UNM:-0}" = "0" ]; then
      stamp "seat $n :: ZERO unmerged paths, so this was NOT a conflict and no saved resolution could apply.  The merge log's tail:"
      tail -8 "$MERGELOG" | sed 's/^/    /' | tee -a "$ASMLOG"
      return 1
    fi
    # --- THE PRE-RESOLVED PATH.  ⚠ IT APPLIES SAVED RESOLUTIONS **MECHANICALLY** AND RESOLVES
    #     NOTHING ITSELF.  The rehearsal (coord-train48-rehearse.sh) runs the REAL merges in a
    #     throwaway worktree, reports every conflict BY PATH, and writes a resolution SLOT per
    #     conflicted path; the coordinator fills those slots and VERIFIES each one there, where a
    #     mistake costs nothing.  This block does the one thing an assembly should do with them:
    #     apply them, assert them, and stamp each by name.
    #     ⚠ EVERY CLAUSE IS A REFUSAL, NOT A CONVENIENCE:
    #       * ALL unmerged paths must have a saved resolution.  A PARTIAL set is refused, because a
    #         merge finished half by a saved file and half by whatever git left is a tree nobody ruled
    #         on.
    #       * Each applied file is scanned for CONFLICT MARKERS in the COMMITTED blob afterwards, by
    #         the same scan every seat gets.
    #       * The resolution's own MANIFEST must name THIS seat, THIS pin **AND THIS BASE**.  A
    #         resolution directory carried over from an earlier derive is a saved answer to a
    #         different question, and a SHA is the only thing that can tell the two apart.
    #         ⚠ THE BASE IS THE THIRD AXIS AND IT IS THE ONE THAT MOVES ON ITS OWN.  seat and pin are
    #         written by the coordinator and change only when the coordinator changes them; the BASE
    #         is origin/master, which moved DURING the train-48 derive (bd1d26faf -> ddd509c1e ->
    #         654343a5e).  A resolution ruled on against one master and applied over another is a
    #         hand-merge of two hunks nobody compared -- the same seat, the same pin, a DIFFERENT
    #         three-way base -- and both of the other two checks pass while it happens.  The manifest
    #         has carried `base=` since it was first written (coord-train48-rehearse.sh writes
    #         seat/pin/branch/base/paths/saved); until now the assembly read only two of the three.
    #         A manifest with NO `base=` line is REFUSED rather than admitted: an absent field and a
    #         matching one are not the same reading, and a resolution that cannot say which master it
    #         was ruled on is exactly the artefact this clause exists to catch.
    #       * The seat is stamped **PRE-RESOLVED** by name, so no reader can mistake it for a clean
    #         merge and no land script can count it as one.
    RESDIR="$SCRIPT_DIR/coord-${LABEL}-resolutions/seat$n"
    RESMAN="$RESDIR/MANIFEST.txt"
    if [ ! -f "$RESMAN" ]; then
      stamp "seat $n :: CONFLICTED and there is NO saved resolution set at $RESDIR.  A conflict is the coordinator's RULING, not this script's -- run coord-${LABEL}-rehearse.sh, resolve there, and re-run -- ABORT"
      return 1
    fi
    RESPIN=$(grep -aE '^pin=' "$RESMAN" | head -1 | cut -d= -f2 | tr -d '\r')
    RESSEAT=$(grep -aE '^seat=' "$RESMAN" | head -1 | cut -d= -f2 | tr -d '\r')
    RESBASE=$(grep -aE '^base=' "$RESMAN" | head -1 | cut -d= -f2 | tr -d '\r')
    stamp "seat $n :: a saved resolution set EXISTS at $RESDIR :: its manifest names seat=[$RESSEAT] pin=[$RESPIN] base=[$RESBASE] and this seat is $n @$want on base $BASE"
    if [ "$RESSEAT" != "$n" ] || [ "${RESPIN}" != "${want}" ]; then
      stamp "seat $n :: the saved resolution names seat [$RESSEAT] at pin [$RESPIN] where this seat is [$n] at [$want].  A resolution carried over from an earlier derive is a saved answer to a DIFFERENT question -- re-rehearse -- ABORT"
      return 1
    fi
    # --- THE THIRD AXIS: THE BASE.  seat and pin are the coordinator's; the BASE is origin/master and
    #     it MOVES WITHOUT ANYONE TOUCHING THE TRAIN.  A resolution ruled on over one master, applied
    #     over another, passes both checks above and produces a tree nobody compared.  Compared as
    #     RESOLVED COMMITS, not as strings, so a nine-hex manifest and a differently-abbreviated $BASE
    #     do not read as a mismatch -- and an UNRESOLVABLE manifest base is a refusal in its own right.
    if [ -z "${RESBASE:-}" ]; then
      stamp "seat $n :: the saved resolution's MANIFEST carries NO \`base=\` line, so it cannot say which master it was ruled on.  An absent field is not a matching one -- re-rehearse so the slot is written against THIS base ($BASE) -- ABORT"
      return 1
    fi
    RESBASE_FULL=$(git rev-parse --verify -q "${RESBASE}^{commit}" 2>/dev/null || true)
    BASE_FULL=$(git rev-parse --verify -q "${BASE}^{commit}" 2>/dev/null || true)
    if [ -z "$RESBASE_FULL" ]; then
      stamp "seat $n :: the saved resolution names base [$RESBASE], which does NOT RESOLVE in this clone.  An unresolved SHA and an invented one read the same -- re-rehearse -- ABORT"
      return 1
    fi
    if [ -z "$BASE_FULL" ] || [ "$RESBASE_FULL" != "$BASE_FULL" ]; then
      stamp "seat $n :: the saved resolution was ruled on over base [$RESBASE] ($RESBASE_FULL) and THIS RUN'S BASE IS [$BASE] ($BASE_FULL).  Same seat, same pin, DIFFERENT three-way base: the files in $RESDIR/files/ are a hand-merge of hunks that were never compared against this master, and applying them would commit a resolution nobody ruled on.  Re-run coord-${LABEL}-rehearse.sh at THIS base, re-resolve, --verify -- ABORT"
      return 1
    fi
    stamp "seat $n :: saved resolution BASE MATCHES :: ruled on over $RESBASE == this run's base $BASE ($BASE_FULL) -- the three-way base the coordinator resolved against is the one being merged over"
    RESAPPLIED=0; RESMISSING=0
    while IFS= read -r up; do
      [ -n "$up" ] || continue
      src="$RESDIR/files/$up"
      if [ -f "$src" ]; then
        mkdir -p "$(dirname "$up")"
        cp "$src" "$up" && git add -- "$up" && RESAPPLIED=$(( RESAPPLIED + 1 )) && stamp "    PRE-RESOLVED $up  <- $src ($(grep -ac . "$src" || true) line(s))"
      else
        RESMISSING=$(( RESMISSING + 1 )); stamp "    NO SAVED RESOLUTION for $up (expected at $src)"
      fi
    done < <(git diff --name-only --diff-filter=U)
    stamp "seat $n :: saved resolutions applied=$RESAPPLIED missing=$RESMISSING of $UNM unmerged path(s)"
    if [ "${RESMISSING:-1}" != "0" ] || [ "$RESAPPLIED" != "$UNM" ]; then
      stamp "seat $n :: the saved resolution set is INCOMPLETE ($RESAPPLIED of $UNM).  A merge finished half by a saved file and half by whatever git left is a tree nobody ruled on -- ABORT"
      git merge --abort 2>/dev/null || git reset --hard -q HEAD
      return 1
    fi
    if [ "$(git diff --name-only --diff-filter=U | grep -ac . || true)" != "0" ]; then
      stamp "seat $n :: unmerged paths REMAIN after applying every saved resolution -- ABORT"
      git merge --abort 2>/dev/null || git reset --hard -q HEAD
      return 1
    fi
    git commit --no-edit -m "Merge $b ($tip) -- ${LABEL} seat $n (PRE-RESOLVED from $RESDIR).

Co-Authored-By: Claude Fable 5.1 <noreply@anthropic.com>" >> "$MERGELOG" 2>&1 < /dev/null
    crc=$?
    if [ "$crc" != "0" ]; then
      stamp "seat $n :: the PRE-RESOLVED merge could not be committed (exit=$crc) -- ABORT"
      tail -8 "$MERGELOG" | sed 's/^/    /' | tee -a "$ASMLOG"
      return 1
    fi
    stamp "seat $n :: **PRE-RESOLVED MERGE COMMITTED** -- $RESAPPLIED path(s) came from the rehearsal's saved set and NOT from this script's judgment.  ⚠ Every later reading of this train describes a tree whose conflicts were resolved by the coordinator at rehearsal time, and the marker scan below is what says none of them left a marker behind."
  fi
  # --- ⚠ **ALLOWED-IDENTITY-AFTER-MERGE.**  COORD 2026-09-13 19:05 (D5), clause (ii).  Subtracting a
  #     path from a gate's reading is only honest if the union then carries THAT SEAT's blob for it: a
  #     ruling is an exemption for ONE SEAT's change, never a hole another seat's change can ride
  #     through.  A7 has asserted exactly this for the behavioral tree since train 47; every ruled
  #     path OUTSIDE that tree was unasserted until now, which is most of them.
  #     ⚠ IT RUNS HERE, RIGHT AFTER THE MERGE, AND AGAIN OVER THE FINISHED UNION IN A7b.  Both are
  #     needed and neither is redundant: this one says the merge did not mangle the seat's own blob,
  #     A7b says no LATER seat moved it.
  if [ -n "$alw" ]; then
    while IFS= read -r alwp <&5; do
      [ -n "$alwp" ] || continue
      alwbu=$(git rev-parse "HEAD:$alwp" 2>/dev/null | cut -c1-12)
      alwbs=$(git rev-parse "$want:$alwp" 2>/dev/null | cut -c1-12)
      if [ -z "$alwbu" ] || [ "$alwbu" != "$alwbs" ]; then
        stamp "seat $n ($cls) :: ⚠ allowed= IDENTITY REFUSED for $alwp -- the union's blob is [${alwbu:-ABSENT}] and this seat's is [${alwbs:-ABSENT}].  The ruling admits this path OUTSIDE the class shape, so the union must carry THIS SEAT's content for it; anything else is another seat riding the exemption -- ABORT"
        return 1
      fi
      stamp "seat $n allowed $alwp :: union BLOB == this seat's BLOB ($alwbu) -- identity"
    done 5<<< "$(seat_allowed_paths "$alw")"
  fi
  # marker scan over the committed blobs.  The loop asserts processed == listed.
  m=0; seen=0; listed=$(git -c core.quotepath=false diff --name-only HEAD~1 HEAD | grep -ac . || true)
  while IFS= read -r f; do
    seen=$(( seen + 1 ))
    c=$(git show "HEAD:$f" 2>/dev/null < /dev/null | grep -ac '^<<<<<<<')
    m=$(( m + ${c:-0} ))
  done < <(git -c core.quotepath=false diff --name-only HEAD~1 HEAD)
  stamp "seat $n merged $b @$tip :: files=$listed scanned=$seen conflictMarkersInBlobs=$m"
  [ "$seen" = "$listed" ] || { stamp "seat $n marker scan processed $seen of $listed files -- the scan is unreliable -- ABORT"; return 1; }
  [ "$m" = "0" ] || { stamp "seat $n left conflict markers in committed blobs -- ABORT"; return 1; }
  return 0
}

SEATS_LISTED=$(printf '%s\n' "$SEAT_TABLE" | grep -c .)
SEATS_DONE=0
SEATS_SKIPPED=0
SKIPPED_NAMES=""
MERGED_CLASSES=""
while IFS='|' read -r sn sb ss sc sm sa <&3; do
  [ -n "$sn" ] || continue
  [ -n "$sm" ] || { stamp "seat $sn: the seat table row has no TIP MODE field -- the table and the loop have drifted apart -- ABORT"; exit 1; }
  if [ "$ss" = "PENDING" ]; then
    if [ "$REQUIRE_ALL" = "1" ] || [ "$SKIP_PENDING" != "1" ]; then
      stamp "seat $sn ($sc) $sb :: **PENDING** and the run is not authorised to skip it (requireAll=$REQUIRE_ALL skipPending=$SKIP_PENDING) -- the coordinator has not named this row's tip, so this is not the announced train.  Fill the table row, or opt into a PARTIAL train with TRAIN48_SKIP_PENDING=1 -- ABORT"
      exit 2
    fi
    SEATS_SKIPPED=$(( SEATS_SKIPPED + 1 )); SKIPPED_NAMES="$SKIPPED_NAMES $sn($sb)"
    stamp "seat $sn ($sc) $sb :: **SKIPPED -- PENDING**.  This row is a PLACEHOLDER the coordinator has not filled; every count below is DERIVED at run time so this run's numbers are internally consistent, and they describe a $(( SEATS_LISTED - PENDING_N ))-seat train rather than the announced $SEATS_LISTED."
    continue
  fi
  merge_seat "$sb" "$ss" "$sn" "$sc" "$sm" < /dev/null || exit 1
  SEATS_DONE=$(( SEATS_DONE + 1 ))
  MERGED_CLASSES="$MERGED_CLASSES $sc"
done 3<<< "$SEAT_TABLE"
stamp "seats :: listed=$SEATS_LISTED merged=$SEATS_DONE skippedPending=$SEATS_SKIPPED [$SKIPPED_NAMES] requireAll=$REQUIRE_ALL mergedClasses=[$MERGED_CLASSES]"

# --- ⚠ THE SEAT-COUNT ASSERTIONS.  **NO LITERAL.**  Train 46 carried `[ "$SEATS_LISTED" = "6" ]` and
#     run 1 refused on it having already merged every seat -- the literal was a fact about the derive.
#     What is asserted instead is the pair of relations a hand-edited table or a swallowed row breaks,
#     and the structural arms above (contiguous numbers, unique refs, unique SHAs) are the other half.
[ "$SEATS_LISTED" = "$SEATS_LISTED_PRE" ] || { stamp "  ^ SEAT TABLE REFUSED: the table read $SEATS_LISTED_PRE row(s) before the loop and $SEATS_LISTED after it.  The table is a constant in this script, so a difference means the file changed under a running chain -- which is the byte-offset hazard the per-run-copy discipline exists for -- ABORT"; exit 1; }
[ "$(( SEATS_DONE + SEATS_SKIPPED ))" = "$SEATS_LISTED" ] || { stamp "the seat loop accounted for $(( SEATS_DONE + SEATS_SKIPPED )) of $SEATS_LISTED -- a seat was SWALLOWED (a child ate the loop's stdin) -- ABORT"; exit 1; }
[ "$SEATS_DONE" -ge 1 ] || { stamp "  ^ SEAT TABLE REFUSED: ZERO seats merged.  A battery over an unchanged tree measures master and reports it as a train -- ABORT"; exit 1; }
if [ "$SEATS_SKIPPED" != "0" ]; then
  stamp "⚠⚠ THIS RUN SKIPPED $SEATS_SKIPPED PENDING SEAT(S) AND IS A **PARTIAL TRAIN**.  Its readings describe the seats that merged and nothing else; the land script's own seat-count check is what refuses it as a landing, which is the intended shape."
fi

# ============================================================================================
# THE **OWED VECTOR** -- DERIVED FROM THE CLASSES OF THE ROWS THAT MERGED, AND READ BY EVERY
# JUSTIFICATION ARM BELOW **AND BY THE LAND SCRIPT**.
# ⚠ THIS IS THE FIX FOR THE FAULT THAT REFUSED RUN 6.  G11(a) required `src/core/syscall/windows` in
#   the delta -- a DIRECTORY LITERAL carried from a train that happened to have a syscall seat -- and
#   read JUSTIFICATION FALSE on a healthy tree.  A justification is a statement about what THIS
#   train's seats are, so it is derived from their CLASSES; a class is the one thing the table
#   declares about a seat before anything is measured.  ⚠ AND IT IS STAMPED AS ITS OWN LINE, because
#   the land script parses that line rather than re-deriving it: two instruments deriving one fact
#   independently is how they drift, and a stamped vector is a single derivation with two readers.
OWED_GOLIB=0; OWED_GEN=0; OWED_CONV=0; OWED_BEH=0; OWED_DOCS=0; OWED_CLAUDEMD=0; OWED_MANIFEST=0; OWED_CORPUS=0; OWED_GT=0; OWED_TOOLING=0
for c in $MERGED_CLASSES; do
  case "$c" in golib|golib-gen|golib-corpus-handown|golib-converter-docs) OWED_GOLIB=1; OWED_GT=1 ;; esac
  case "$c" in golib-gen) OWED_GEN=1 ;; esac
  case "$c" in converter|converter-corpus-metadata|converter-guard|converter-guard-rebaseline|converter-test|converter-test+docs|converter-test+tooling|golib-gen|golib-corpus-handown|golib-converter-docs) OWED_CONV=1 ;; esac
  case "$c" in converter-guard|golib-gen) OWED_BEH=1 ;; esac
  case "$c" in doctrine) OWED_CLAUDEMD=1 ;; esac
  case "$c" in manifest) OWED_MANIFEST=1 ;; esac
  case "$c" in converter|converter-corpus-metadata|golib-corpus-handown|manifest) OWED_CORPUS=1 ;; esac
  # ⚠ THE TOOLING OWE IS NEW IN TRAIN 48 AND IT IS A **G2** OWE.  `converter-test+tooling` owes LEG C
  #   (its converter half) AND G2 (its script half); `tooling` owes G2 ALONE, which is why it is not in
  #   the converter arm above -- a class that owed LEG C without touching src/go2cs would make G11(a)'s
  #   converter arm refuse a healthy tree, which is the exact fault (L7) was written to remove.
  case "$c" in tooling|converter-test+tooling) OWED_TOOLING=1 ;; esac
  case "$c" in docs|docs-data|converter|converter-guard|converter-guard-rebaseline|converter-test+docs|converter-test+tooling|tooling|golib|golib-corpus-handown|golib-converter-docs) OWED_DOCS=1 ;; esac
done
stamp "OWED VECTOR :: golib=$OWED_GOLIB gen=$OWED_GEN converter=$OWED_CONV behavioral=$OWED_BEH corpus=$OWED_CORPUS golibtests=$OWED_GT docs=$OWED_DOCS claudemd=$OWED_CLAUDEMD manifest=$OWED_MANIFEST tooling=$OWED_TOOLING (DERIVED from the CLASSES of the $SEATS_DONE merged row(s): [$MERGED_CLASSES]) -- every justification arm below reads THIS vector, and so does the land script, so the two cannot drift"
# ⚠ `docs` IS A **SOFT** OWE AND IS STAMPED AS ONE.  A class may or may not carry a record, so the
#   docs arm below reads the vector as "a record is EXPECTED" rather than "a record is REQUIRED", and
#   a zero is checked against the skip count instead of refusing outright.

# --- THE UNION FIX -- **OPTIONAL**, AND ITS ABSENCE IS STATED RATHER THAN SILENT. --------------
#     ⚠ WHY IT EXISTS AT ALL.  Train 46 run 4 died forty minutes into its battery at LEG C: two edits
#     to ONE `_test.go` -- one seat's new call sites, another train's widened signature -- merged
#     CLEAN BY CONTENT and the package failed to compile ONLY AT THE UNION.  Such a fix cannot live
#     on a seated branch (neither side is wrong alone), so it is an ASSEMBLY commit stated as the
#     train's own.  ⚠ AND THE REHEARSAL IS NOW WHERE IT IS FOUND: coord-train48-rehearse.sh runs
#     `go vet ./...` in src/go2cs under the converter build pin after EVERY seat merge and names the
#     FIRST failing seat, which costs seconds rather than a battery leg.
#     ⚠ THE PATCH IS OPTIONAL AND ITS FOOTPRINT IS DECLARED.  A patch present without a declared
#     footprint is a patch nobody sized, so `<patch>.expected` (one `path<TAB>add<TAB>del` line per
#     file, exactly as `git diff --numstat` prints it) is REQUIRED beside it and compared line for
#     line.  Absent patch == no fix, stated -- and a run with no patch asserts NOTHING by itself
#     about the union's compilability: LEG C is that assertion.
UFPATCH="$SCRIPT_DIR/coord-${LABEL}-unionfix.patch"
UFEXPECT="$SCRIPT_DIR/coord-${LABEL}-unionfix.expected"
if [ ! -s "$UFPATCH" ]; then
  stamp "UNION FIX :: NO patch at $UFPATCH, so NO union fix is applied in this run.  ⚠ STATED, NEVER SILENT: the rehearsal's per-seat \`go vet\` is what finds a union-only compile failure, and if it found one the patch belongs here.  A run with no patch says nothing by itself about whether the union compiles -- LEG C is that assertion."
else
  [ -s "$UFEXPECT" ] || { stamp "UNION FIX REFUSED :: a patch exists at $UFPATCH but its declared footprint $UFEXPECT is absent or empty.  A patch with no declared footprint is a patch nobody sized -- ABORT"; exit 1; }
  UFBEFORE=$(git rev-parse HEAD)
  git apply --check "$UFPATCH" 2>/dev/null || { stamp "UNION FIX REFUSED :: the patch does not apply cleanly to the merged tree (a seat moved under it) -- ABORT"; exit 1; }
  git apply "$UFPATCH" || { stamp "UNION FIX REFUSED :: git apply failed after --check passed -- ABORT"; exit 1; }
  UFGOT=$(git diff --numstat | sort)
  UFWANT=$(tr -d '\r' < "$UFEXPECT" | grep -a . | sort)
  if [ "$UFGOT" != "$UFWANT" ]; then
    stamp "UNION FIX REFUSED :: the applied footprint is not the declared one -- ABORT.  APPLIED:"
    printf '%s\n' "$UFGOT" | sed 's/^/    /' | tee -a "$ASMLOG"
    stamp "  DECLARED:"
    printf '%s\n' "$UFWANT" | sed 's/^/    /' | tee -a "$ASMLOG"
    git checkout -- . ; exit 1
  fi
  git add -A -- $(printf '%s\n' "$UFWANT" | cut -f3 | tr '\n' ' ')
  git -c user.name="$(git config user.name)" commit -q -F "$SCRIPT_DIR/coord-${LABEL}-unionfix.msg" 2>/dev/null || git -c user.name="$(git config user.name)" commit -q -m "${LABEL} UNION FIX (see $(basename "$UFPATCH"))

Applied at assembly because the defect exists ONLY at the union: each side is correct alone and
git merged them clean by content.  Its declared footprint is asserted against $(basename "$UFEXPECT")
line for line, and \`go vet ./...\` in src/go2cs is its gate.

Co-Authored-By: Claude Fable 5.1 <noreply@anthropic.com>" || { stamp "UNION FIX REFUSED :: commit failed -- ABORT"; exit 1; }
  UFAFTER=$(git rev-parse HEAD)
  if pin_go 1.24.13 "UNION-FIX-VET"; then
    ( cd src/go2cs && go vet ./... > "$SCRIPT_DIR/coord-${LABEL}-unionfix-vet-${RUNID}.log" 2>&1 ); ufvet=$?
  else
    ufvet=90
  fi
  stamp "UNION FIX applied :: $UFBEFORE -> $UFAFTER :: footprint matches $(basename "$UFEXPECT") ($(printf '%s\n' "$UFWANT" | grep -ac .) file(s)) :: go vet ./... in src/go2cs exit=$ufvet (must be 0)"
  [ "$ufvet" = "0" ] || { stamp "  ^ UNION FIX REFUSED: go vet still fails after the fix (or its pin could not be established) -- the union carries a SECOND compile defect; read the vet log -- ABORT"; head -12 "$SCRIPT_DIR/coord-${LABEL}-unionfix-vet-${RUNID}.log" 2>/dev/null | sed 's/^/    /' | tee -a "$ASMLOG"; exit 1; }
fi

HEADSHA=$(git rev-parse --short HEAD)
stamp "assembled head=$HEADSHA base=$BASE seatsMerged=$SEATS_DONE deltaFiles=$(git -c core.quotepath=false diff --name-only "$BASE" HEAD | grep -ac . || true)"
stamp "delta numstat: $(git -c core.quotepath=false diff --numstat "$BASE" HEAD | awk '{a+=$1;d+=$2} END{printf "+%d/-%d over %d file(s)", a+0, d+0, NR+0}')"
git -c core.quotepath=false diff --numstat "$BASE" HEAD | sed 's/^/    /' >> "$ASMLOG"
stamp "delta file list:"
git -c core.quotepath=false diff --name-only "$BASE" HEAD | sed 's/^/      /' | tee -a "$ASMLOG"

# --- THE NEW BEHAVIORAL PROJECTS, DERIVED **AT TWO DEPTHS**, and that is not bookkeeping.
#     ⚠ FALSE-GREEN ROUTE #3 IS EXACTLY THIS DISTINCTION.  A behavioral guard may carry NESTED
#     sub-library packages, each with its own .csproj that MUST be registered in go2cs.slnx and each
#     of which the runner enumerates (deepest-first, recursively) -- while only the TOP-LEVEL project
#     carries the four MSTest `Check<Name>()` registrations, because UpdateTestTargets is deliberately
#     top-level-only.  One number cannot answer both questions, and a train that used the top-level
#     count for the solution arithmetic would silently accept an unregistered sub-library -- which is
#     how `nsshadow` slipped through: it passes every harness gate and breaks only Visual Studio.
#     A project is NEW when its .csproj is in the delta as an ADDITION.
NEWBEH_ALL_LIST=$(git -c core.quotepath=false diff --name-only --diff-filter=A "$BASE" HEAD -- src/tests/Behavioral | grep -aE '^src/tests/Behavioral/.*\.csproj$' | sort -u || true)
NEWBEH_ALL=$(printf '%s\n' "$NEWBEH_ALL_LIST" | grep -ac . || true)
NEWBEH_LIST=$(git -c core.quotepath=false diff --name-only --diff-filter=A "$BASE" HEAD -- src/tests/Behavioral | grep -aE '^src/tests/Behavioral/[^/]+/[^/]+\.csproj$' | awk -F/ '{print $4}' | sort -u || true)
NEWBEH=$(printf '%s\n' "$NEWBEH_LIST" | grep -ac . || true)
NEWBEH_NESTED=$(( NEWBEH_ALL - NEWBEH ))
stamp "NEW behavioral projects in this train's delta (derived, not carried) :: TOP-LEVEL=$NEWBEH [$(printf '%s' "$NEWBEH_LIST" | tr '\n' ' ')] :: ALL DEPTHS=$NEWBEH_ALL of which NESTED sub-libraries=$NEWBEH_NESTED :: OWED_BEH=$OWED_BEH (a class that adds a guard was merged) -- ⚠ the two numbers answer different questions: MSTest registrations and goldens are TOP-LEVEL, solution registrations are ALL DEPTHS, and route #3 is what happens when one is used for the other"
stamp "NEW behavioral .csproj paths, ALL DEPTHS:"
printf '%s\n' "$NEWBEH_ALL_LIST" | sed 's/^/      /' | tee -a "$ASMLOG"
# ============================================================================================
# A-ASSERTIONS -- what the assembled tree must CONTAIN and must NOT.  Read from the FILES, not from
# the graph, so a merge that resolved a hunk the wrong way is caught by content rather than by
# ancestry.  Every one of these is a SECOND derivation of something a seat's numstat already implied.
# ⚠ TRAIN 44's A1/A2 (C1's TryBindFinalizerArgument referent fix and GoReflect.FinalizerBinding.cs)
#   ARE GONE, not re-pointed.  That seat LANDED with train 44 and no seat here touches it, so those
#   assertions could no longer fail -- and a green that cannot go red is what this file refuses
#   everywhere else.
# ⚠ EVERY ASSERTION THAT BELONGS TO A **PENDING** SEAT IS GATED ON THAT SEAT HAVING MERGED, and says
#   so.  An assertion that silently passes because its subject was skipped is the vacuous green this
#   train's PENDING mechanism exists to make visible.
# ============================================================================================

# --- ⚠ **SEAT-CONTENT ASSERTIONS -- A FILL POINT WITH A GATE.** -------------------------------
#     Train 46 carried A1..A4: four hand-written arms, each reading a NAMED seat's own content out of
#     the assembled FILES rather than out of the graph, so a merge that resolved a hunk the wrong way
#     was caught by content instead of by ancestry.  They are the strongest assertions in that file
#     and they are also the most train-specific: every one of them names a seat.
#     A TEMPLATE CANNOT WRITE THEM, so what it carries instead is the SLOT and the GATE that refuses
#     a run with none.  The coordinator writes ONE ARM PER NAMED SEAT here at seat fill; each arm
#     ends by incrementing SEATASSERT_N, and the count is compared against the number of seats that
#     MERGED.  ⚠ A GREEN HERE WITH ZERO ARMS WOULD BE A TREE NOBODY ASSERTED, which is exactly the
#     shape "the assembled tree is not the tree this train was gated for" is meant to catch.
#
#     WHAT AN ARM LOOKS LIKE (from train 46's A3, which is the sharpest of the four -- a REGISTRY
#     entry and the corpus body it DISPLACES, read as TWO halves, because a registration split from
#     its footprint is the syscall.Uname silent-subtraction class: both diffs are pure additions and
#     removals, git merges them without a conflict, and the result compiles nowhere):
#
#         if [ "$SEAT3_SHA" != "PENDING" ]; then
#           A3REG=$(git diff --numstat "$BASE" HEAD -- src/go2cs/manualTypeOperations.go | cut -f1)
#           A3DEL=$(git diff --numstat "$BASE" HEAD -- src/core/runtime/panic.cs | cut -f2)
#           stamp "A3 seat 3 registry AND displacement :: registry +${A3REG:-0} (must be >0) :: the displaced body's DELETION column=${A3DEL:-0} (must be >0 -- a +N/-0 there would mean the entry landed and the displacement did not)"
#           { [ "${A3REG:-0}" -ge 1 ] && [ "${A3DEL:-0}" -ge 1 ]; } || fail_gate A3
#           SEATASSERT_N=$(( SEATASSERT_N + 1 ))
#         fi
#
#     ⚠ EVERY ARM IS GATED ON ITS SEAT HAVING MERGED (the `$SEATn_SHA != PENDING` test above), so a
#     PARTIAL run does not refuse on an assertion about a seat that was never there.
# === SEAT-CONTENT ASSERTIONS BEGIN (filled at seat fill; one arm per named seat) ===
SEATASSERT_N=0
# ⚠ FILL POINT.  ONE ARM PER NAMED SEAT, written at seat fill, each gated on its own
#   `$SEATn_SHA != PENDING` and each ending in `SEATASSERT_N=$(( SEATASSERT_N + 1 ))`.
#   ⚠ TRAIN 47's FIFTEEN ARMS ARE **DELETED, NOT RENUMBERED**.  Every one of them read a named seat's
#   own content -- a registry entry and the corpus body it displaces, a guard file and its projitems
#   registration, a heading and the ladder under it -- and none of those predicates is about a row of
#   THIS table.  A renumbered arm is an assertion about content nobody seated, and it would refuse a
#   healthy battery under a train-47 seat's name.
#   ⚠ THE ARM-COUNT SHORTFALL BELOW IS A **POST-BATTERY GATE**, not the refusal that stops a
#   REQUIRE_ALL run with a PENDING row -- that one ABORTS with exit 2 in the SEAT TABLE's PENDING
#   block, before a single seat is merged.  Reaching the shortfall means a row MERGED whose arm nobody
#   wrote, which is a different mistake found at a different time.

# --- row 1 :: claude/g-handown-metadata-t48-r47 -- the preservation guards, their REGISTRATIONS, and
#     the RULED cache.cs, read as three different things.  The converter half is two new guard files
#     that are invisible in Visual Studio unless projitems names them; the metadata half is the count
#     of package_info.cs files this train moved; the hand-own half is BLOB IDENTITY against the seat's
#     own tree, which is the only reading that says the allowed= path came from THIS seat.
if [ "$SEAT1_SHA" != "PENDING" ]; then
  S1F=0
  for f in src/go2cs/handOwnReferences_test.go src/go2cs/handOwnTypeAccessibility_test.go; do
    [ -f "$f" ] || { stamp "  ^ A-row1 REFUSED: $f is not in the assembled tree"; S1F=1; }
    [ "$(grep -acF -- "$(basename "$f")" src/go2cs/go2cs-src.projitems || true)" = "1" ] || { stamp "  ^ A-row1 REFUSED: $(basename "$f") is not registered exactly once in go2cs-src.projitems -- a converter source that is not registered there is invisible in Visual Studio and bites nowhere else"; S1F=1; }
    [ "$(grep -acE '^func Test' "$f" 2>/dev/null || true)" -ge 1 ] || { stamp "  ^ A-row1 REFUSED: $f carries no top-level Test function -- a guard that merged as an empty shell is a green that cannot go red"; S1F=1; }
  done
  S1PKG=$(git diff --name-only "$BASE" HEAD -- src/core | grep -acE 'package_info\.cs$' || true)
  S1BH=$(git rev-parse "HEAD:src/core/crypto/internal/boring/bcache/cache.cs" 2>/dev/null || true)
  S1BS=$(git rev-parse "$SEAT1_SHA:src/core/crypto/internal/boring/bcache/cache.cs" 2>/dev/null || true)
  S1BLOB=0; { [ -n "$S1BH" ] && [ "$S1BH" = "$S1BS" ]; } && S1BLOB=1
  stamp "A-row1 hand-own preservation guards + the RULED cache.cs :: guard-file failures=$S1F (must be 0 -- both files present, each registered x1 in projitems, each carrying >= 1 top-level Test) :: package_info.cs files this train moved=$S1PKG (must be >= 4) :: the allowed= cache.cs blob at the union == this seat's own blob=$S1BLOB (must be 1; union=$S1BH seat=$S1BS)"
  { [ "$S1F" = "0" ] && [ "${S1PKG:-0}" -ge 4 ] && [ "$S1BLOB" = "1" ]; } || fail_gate A-row1
  SEATASSERT_N=$(( SEATASSERT_N + 1 ))
fi

# --- row 2 :: claude/c2-h6-crosscheck -- a DATED BLOCK APPENDED to a record that already existed at
#     the base, so presence proves nothing: the heading is the reading.
if [ "$SEAT2_SHA" != "PENDING" ]; then
  S2P='docs/phase4/CENSUS-h6-handown-package-aliases.md'
  S2OK=1; [ -f "$S2P" ] || { stamp "  ^ A-row2 REFUSED: $S2P is not in the assembled tree"; S2OK=0; }
  S2H=$(grep -acF -- 'Cross-check block -- C2, 2026-09-13' "$S2P" 2>/dev/null || true)
  S2D=$(git diff --name-only "$BASE" HEAD -- "$S2P" | grep -ac . || true)
  stamp "A-row2 C2's H6 alias cross-check block :: file present=$S2OK :: the block's own dated heading x$S2H (must be 1; the base reads 0) :: the record in this train's delta=$S2D (must be 1)"
  { [ "$S2OK" = "1" ] && [ "${S2H:-0}" = "1" ] && [ "${S2D:-0}" = "1" ]; } || fail_gate A-row2
  SEATASSERT_N=$(( SEATASSERT_N + 1 ))
fi

# --- row 3 :: claude/laneR-h6-alias-block -- DECLARED `stack-on=2`, TWO BLOCKS IN ONE FILE.  The arm
#     reads BOTH: this row's own heading AND row 2's, because two appends to one record are exactly the
#     pair a wrong-side resolution keeps one half of, and git reports that as a clean merge.
if [ "$SEAT3_SHA" != "PENDING" ]; then
  S3P='docs/phase4/CENSUS-h6-handown-package-aliases.md'
  S3OK=1; [ -f "$S3P" ] || { stamp "  ^ A-row3 REFUSED: $S3P is not in the assembled tree"; S3OK=0; }
  S3H=$(grep -acF -- 'ARM A withdrawn, ARM B, ARM C' "$S3P" 2>/dev/null || true)
  S3BASEH=$(grep -acF -- 'Cross-check block -- C2, 2026-09-13' "$S3P" 2>/dev/null || true)
  S3PAIR=1
  if [ "$SEAT2_SHA" != "PENDING" ]; then
    [ "${S3BASEH:-0}" = "1" ] || S3PAIR=0
  else
    S3BASEH="NOT MEASURED (row 2 is PENDING)"
  fi
  stamp "A-row3 lane R's emission-half block, STACKED ON ROW 2 :: file present=$S3OK :: this row's heading x$S3H (must be 1; the base reads 0) :: row 2's heading still x$S3BASEH (must be 1 whenever row 2 merged -- a stacked append that kept only the top block is silent subtraction)"
  { [ "$S3OK" = "1" ] && [ "${S3H:-0}" = "1" ] && [ "$S3PAIR" = "1" ]; } || fail_gate A-row3
  SEATASSERT_N=$(( SEATASSERT_N + 1 ))
fi

# --- row 4 :: claude/laneR-docs-h6-skeleton -- THE ONLY **SHA-MODE** ROW.  The pin is d18059950 and the
#     branch tip is 2 commits beyond it, so this arm asserts the PINNED skeleton's content and NOT the
#     tip's: the blob at the union must equal the blob at $SEAT4_SHA, and the tip-only re-cut must be
#     ABSENT.  MEASURED: mcleanup.cs reads 2 at the pin and 9 at the tip, so the count discriminates
#     the two trees by itself -- a SHA-mode row that silently carried its tip would read 9 here.
if [ "$SEAT4_SHA" != "PENDING" ]; then
  S4P='docs/phase4/AUDIT-h6-handown-go124.md'
  S4OK=1; [ -f "$S4P" ] || { stamp "  ^ A-row4 REFUSED: $S4P is not in the assembled tree"; S4OK=0; }
  S4NEW=$(git diff --name-only --diff-filter=A "$BASE" HEAD -- "$S4P" | grep -ac . || true)
  S4BH=$(git rev-parse "HEAD:$S4P" 2>/dev/null || true)
  S4BS=$(git rev-parse "$SEAT4_SHA:$S4P" 2>/dev/null || true)
  S4BLOB=0; { [ -n "$S4BH" ] && [ "$S4BH" = "$S4BS" ]; } && S4BLOB=1
  S4MC=$(grep -acF -- 'mcleanup.cs' "$S4P" 2>/dev/null || true)
  S4PRES=$(grep -acF -- 'item 21 RETIRED' docs/phase4/CENSUS-preservation-2026-09-12.md 2>/dev/null || true)
  stamp "A-row4 the H6 audit SKELETON at its PIN, not at its tip :: file present=$S4OK :: ADDED by this train=$S4NEW (must be 1) :: union blob == the PINNED blob=$S4BLOB (must be 1; union=$S4BH pin=$S4BS) :: mcleanup.cs x$S4MC (must be 2 -- the pin's reading; the branch tip reads 9, so this number alone says which tree boarded) :: the preservation census's item-21 retirement x$S4PRES (must be 1; the base reads 0)"
  { [ "$S4OK" = "1" ] && [ "${S4NEW:-0}" = "1" ] && [ "$S4BLOB" = "1" ] && [ "${S4MC:-0}" = "2" ] && [ "${S4PRES:-0}" = "1" ]; } || fail_gate A-row4
  SEATASSERT_N=$(( SEATASSERT_N + 1 ))
fi

# --- row 5 :: claude/laneR-prepin-baselines-recut -- a BOARD append.  FOUR rows of this train append to
#     this one file and two of them were PRE-RESOLVED, so each BOARD arm reads its OWN block's heading
#     and row 15 reads all three of the others.
if [ "$SEAT5_SHA" != "PENDING" ]; then
  S5P='docs/phase4/BOARD-next-validation-candidates.md'
  S5OK=1; [ -f "$S5P" ] || { stamp "  ^ A-row5 REFUSED: $S5P is not in the assembled tree"; S5OK=0; }
  S5H=$(grep -acF -- 'PRE-PIN BASELINE COMMIT (R)' "$S5P" 2>/dev/null || true)
  S5T=$(grep -acF -- 'next validation candidates, each rooted' "$S5P" 2>/dev/null || true)
  stamp "A-row5 the pre-pin reflect/unique baselines :: BOARD present=$S5OK :: this row's block heading x$S5H (must be 1; the base reads 0) :: the BOARD's own title still x$S5T (must be 1 -- a resolution that replaced the file rather than appending to it would move this)"
  { [ "$S5OK" = "1" ] && [ "${S5H:-0}" = "1" ] && [ "${S5T:-0}" = "1" ]; } || fail_gate A-row5
  SEATASSERT_N=$(( SEATASSERT_N + 1 ))
fi

# --- row 6 :: claude/c2-safepush-shallow-skip -- ⚠ THE GUARD FILE ALREADY EXISTED AT THE BASE, so
#     `-f` and a projitems registration are BOTH TRUE ON THE BASE TREE ALONE and neither can fail.
#     The readings that can are the NEW test by name and the Test-function COUNT: base 1, union 2.
if [ "$SEAT6_SHA" != "PENDING" ]; then
  S6P='src/go2cs/safePushGuard_test.go'
  S6OK=1; [ -f "$S6P" ] || { stamp "  ^ A-row6 REFUSED: $S6P is not in the assembled tree"; S6OK=0; }
  S6REG=$(grep -acF -- 'safePushGuard_test.go' src/go2cs/go2cs-src.projitems || true)
  S6FN=$(grep -acE '^func Test' "$S6P" 2>/dev/null || true)
  S6NEW=$(grep -acF -- 'TestSafePushShallowDetectionFiresOnAShallowCloneAndNotOnAFullOne' "$S6P" 2>/dev/null || true)
  S6HELP=$(grep -acF -- 'func safePushRepoIsShallowIn(' "$S6P" 2>/dev/null || true)
  stamp "A-row6 the safe-push SHALLOW-CLONE skip :: file present=$S6OK :: projitems registrations=$S6REG (must be 1) :: top-level Test functions=$S6FN (must be 2 -- the base reads 1, so the COUNT is what says this row boarded) :: the new test named x$S6NEW (must be >= 1; the base reads 0) :: its shallow-detection helper x$S6HELP (must be 1)"
  { [ "$S6OK" = "1" ] && [ "${S6REG:-0}" = "1" ] && [ "${S6FN:-0}" = "2" ] && [ "${S6NEW:-0}" -ge 1 ] && [ "${S6HELP:-0}" = "1" ]; } || fail_gate A-row6
  SEATASSERT_N=$(( SEATASSERT_N + 1 ))
fi

# --- row 7 :: claude/c1-seat-duplication-census -- an INSTRUMENT and its GUARD, and the guard's
#     registration.  A census script without its self-test is a measurement nobody can re-run.
if [ "$SEAT7_SHA" != "PENDING" ]; then
  S7OK=1
  for f in src/go2cs/seatDuplicationGuard_test.go src/seat-duplication-census.sh; do
    [ -f "$f" ] || { stamp "  ^ A-row7 REFUSED: $f is not in the assembled tree"; S7OK=0; }
  done
  S7REG=$(grep -acF -- 'seatDuplicationGuard_test.go' src/go2cs/go2cs-src.projitems || true)
  S7FN=$(grep -acF -- 'func TestSeatDuplicationCensusSelfTest' src/go2cs/seatDuplicationGuard_test.go 2>/dev/null || true)
  S7EX=$(grep -acEv '^[[:space:]]*(#|$)' src/seat-duplication-census.sh 2>/dev/null || true)
  stamp "A-row7 the seat-duplication census :: both files present=$S7OK :: guard registered in projitems x$S7REG (must be 1) :: its named self-test x$S7FN (must be 1) :: EXECUTABLE lines in the instrument=$S7EX (must be >= 1 -- a script that merged as comments alone runs nothing)"
  { [ "$S7OK" = "1" ] && [ "${S7REG:-0}" = "1" ] && [ "${S7FN:-0}" = "1" ] && [ "${S7EX:-0}" -ge 1 ]; } || fail_gate A-row7
  SEATASSERT_N=$(( SEATASSERT_N + 1 ))
fi

# --- row 8 :: claude/g-fleet-patchid-census -- same shape as row 7, a different instrument.
if [ "$SEAT8_SHA" != "PENDING" ]; then
  S8OK=1
  for f in src/go2cs/fleetPatchIdCensusGuard_test.go src/fleet-patchid-census.sh; do
    [ -f "$f" ] || { stamp "  ^ A-row8 REFUSED: $f is not in the assembled tree"; S8OK=0; }
  done
  S8REG=$(grep -acF -- 'fleetPatchIdCensusGuard_test.go' src/go2cs/go2cs-src.projitems || true)
  S8FN=$(grep -acF -- 'func TestFleetPatchIdCensusSelfTest' src/go2cs/fleetPatchIdCensusGuard_test.go 2>/dev/null || true)
  S8EX=$(grep -acEv '^[[:space:]]*(#|$)' src/fleet-patchid-census.sh 2>/dev/null || true)
  stamp "A-row8 the fleet patch-id census :: both files present=$S8OK :: guard registered in projitems x$S8REG (must be 1) :: its named self-test x$S8FN (must be 1) :: EXECUTABLE lines in the instrument=$S8EX (must be >= 1)"
  { [ "$S8OK" = "1" ] && [ "${S8REG:-0}" = "1" ] && [ "${S8FN:-0}" = "1" ] && [ "${S8EX:-0}" -ge 1 ]; } || fail_gate A-row8
  SEATASSERT_N=$(( SEATASSERT_N + 1 ))
fi

# --- row 9 :: claude/i9-board-runtime-door-bisect -- ⚠ A **PRE-RESOLVED** MERGE.  The union's BOARD
#     came from the coordinator's saved resolution set and NOT from git's judgment, so this row's own
#     block is the only thing that says the resolution kept it.
if [ "$SEAT9_SHA" != "PENDING" ]; then
  S9P='docs/phase4/BOARD-next-validation-candidates.md'
  S9OK=1; [ -f "$S9P" ] || { stamp "  ^ A-row9 REFUSED: $S9P is not in the assembled tree"; S9OK=0; }
  S9H=$(grep -acF -- 'door regression is EXACTLY' "$S9P" 2>/dev/null || true)
  S9T=$(grep -acF -- 'next validation candidates, each rooted' "$S9P" 2>/dev/null || true)
  stamp "A-row9 i9's runtime-door bisect block, through a PRE-RESOLVED merge :: BOARD present=$S9OK :: this row's block heading x$S9H (must be 1; the base reads 0) :: the BOARD's own title still x$S9T (must be 1)"
  { [ "$S9OK" = "1" ] && [ "${S9H:-0}" = "1" ] && [ "${S9T:-0}" = "1" ]; } || fail_gate A-row9
  SEATASSERT_N=$(( SEATASSERT_N + 1 ))
fi

# --- row 10 :: claude/i9-board-archive-tar -- DECLARED `stack-on=9`, a second BOARD append.
if [ "$SEAT10_SHA" != "PENDING" ]; then
  S10P='docs/phase4/BOARD-next-validation-candidates.md'
  S10OK=1; [ -f "$S10P" ] || { stamp "  ^ A-row10 REFUSED: $S10P is not in the assembled tree"; S10OK=0; }
  S10H=$(grep -acF -- 'a banked 97-verdict row killed the test host ONCE in 27 isolated runs' "$S10P" 2>/dev/null || true)
  stamp "A-row10 the archive/tar one-in-27 host kill :: BOARD present=$S10OK :: this row's block heading x$S10H (must be 1; the base reads 0)"
  { [ "$S10OK" = "1" ] && [ "${S10H:-0}" = "1" ]; } || fail_gate A-row10
  SEATASSERT_N=$(( SEATASSERT_N + 1 ))
fi

# --- row 11 :: claude/c2-census-goroot-fix-clean -- THREE HALVES.  A new converter source and its
#     registration, a new guard file, and TWO `.claude/` paths admitted by an allowed= ruling -- and the
#     ruled paths are read by BLOB IDENTITY against the seat, which is the only thing that says the doc
#     half came from THIS seat rather than from something else that touched the same file.
if [ "$SEAT11_SHA" != "PENDING" ]; then
  S11OK=1
  for f in src/go2cs/toolchainGoRootFix_test.go src/go2cs/toolchainResolution.go .claude/rules/converter.md .claude/skills/mailbox/SKILL.md; do
    [ -f "$f" ] || { stamp "  ^ A-row11 REFUSED: $f is not in the assembled tree"; S11OK=0; }
  done
  S11REG=$(grep -acF -- 'toolchainGoRootFix_test.go' src/go2cs/go2cs-src.projitems || true)
  S11FN=$(grep -acE '^func Test' src/go2cs/toolchainGoRootFix_test.go 2>/dev/null || true)
  S11PIN=$(grep -acF -- 'corpusPinnedReleaseOrError' src/go2cs/toolchainResolution.go 2>/dev/null || true)
  S11RB=0
  for p in .claude/rules/converter.md .claude/skills/mailbox/SKILL.md; do
    bh=$(git rev-parse "HEAD:$p" 2>/dev/null || true); bs=$(git rev-parse "$SEAT11_SHA:$p" 2>/dev/null || true)
    if [ -n "$bh" ] && [ "$bh" = "$bs" ]; then S11RB=$(( S11RB + 1 )); else stamp "  ^ A-row11 ruled path $p union blob [$bh] != seat blob [$bs]"; fi
  done
  stamp "A-row11 the GOROOT census fix :: the four files present=$S11OK :: guard registered in projitems x$S11REG (must be 1) :: top-level Test functions in the guard=$S11FN (must be >= 1; measured 10) :: the pinned-release resolver named in toolchainResolution.go x$S11PIN (must be >= 1; the base reads 0) :: allowed= ruled paths BLOB-IDENTICAL to the seat=$S11RB of 2 (must be 2)"
  { [ "$S11OK" = "1" ] && [ "${S11REG:-0}" = "1" ] && [ "${S11FN:-0}" -ge 1 ] && [ "${S11PIN:-0}" -ge 1 ] && [ "$S11RB" = "2" ]; } || fail_gate A-row11
  SEATASSERT_N=$(( SEATASSERT_N + 1 ))
fi

# --- row 12 :: claude/c1-gctestisreachable-clean -- ⚠ A REGISTRY ENTRY AND THE CORPUS BODY IT
#     DISPLACES, READ AS TWO HALVES.  Both diffs are pure additions and removals, git merges them
#     without a conflict, and a tree that kept one compiles nowhere: the registration says the
#     converter will displace gcTestIsReachable, the mgc.cs DELETION column and its dropped mention say
#     the displacement actually happened, and mgc_impl.cs is the body that replaced it.
if [ "$SEAT12_SHA" != "PENDING" ]; then
  S12OK=1
  for f in src/go2cs/manualTypeOperations.go src/core/runtime/mgc.cs src/core/runtime/mgc_impl.cs src/core/runtime/panic_impl.cs; do
    [ -f "$f" ] || { stamp "  ^ A-row12 REFUSED: $f is not in the assembled tree"; S12OK=0; }
  done
  S12REG=$(grep -acF -- '"gcTestIsReachable": goosAny,' src/go2cs/manualTypeOperations.go 2>/dev/null || true)
  S12IMPL=$(grep -acF -- 'gcTestIsReachable' src/core/runtime/mgc_impl.cs 2>/dev/null || true)
  S12LEFT=$(grep -acF -- 'gcTestIsReachable' src/core/runtime/mgc.cs 2>/dev/null || true)
  S12DEL=$(git diff --numstat "$BASE" HEAD -- src/core/runtime/mgc.cs | cut -f2)
  case "$S12DEL" in ''|*[!0-9]*) S12DEL=0 ;; esac
  S12ERR=$(grep -acF -- 'ERRATUM 2026-09-13' src/core/runtime/panic_impl.cs 2>/dev/null || true)
  stamp "A-row12 gcTestIsReachable REGISTRY and DISPLACEMENT :: the four files present=$S12OK :: the registry entry x$S12REG (must be 1; the base reads 0) :: the body in mgc_impl.cs x$S12IMPL (must be >= 1) :: mentions LEFT in mgc.cs=$S12LEFT (must be 1 -- the base reads 2, so a 2 here is the registration landing without its displacement) :: mgc.cs DELETION column=$S12DEL (must be > 0; a +N/-0 there is the same silent subtraction) :: the panic_impl erratum x$S12ERR (must be >= 1)"
  { [ "$S12OK" = "1" ] && [ "${S12REG:-0}" = "1" ] && [ "${S12IMPL:-0}" -ge 1 ] && [ "${S12LEFT:-0}" = "1" ] && [ "$S12DEL" -ge 1 ] && [ "${S12ERR:-0}" -ge 1 ]; } || fail_gate A-row12
  SEATASSERT_N=$(( SEATASSERT_N + 1 ))
fi

# --- row 13 :: claude/c1-token-door-census-stacked -- DECLARED `stack-on=11`.  Instrument, guard and
#     the census RECORD the instrument produced: a record without its instrument is a figure nobody can
#     re-derive, which is the reason this class exists.
if [ "$SEAT13_SHA" != "PENDING" ]; then
  S13OK=1
  for f in src/go2cs/tokenDoorCensusGuard_test.go src/token-door-census.sh docs/phase4/CENSUS-token-door-live-wrappers.md; do
    [ -f "$f" ] || { stamp "  ^ A-row13 REFUSED: $f is not in the assembled tree"; S13OK=0; }
  done
  S13REG=$(grep -acF -- 'tokenDoorCensusGuard_test.go' src/go2cs/go2cs-src.projitems || true)
  S13FN=$(grep -acF -- 'func TestTokenDoorCensusControls' src/go2cs/tokenDoorCensusGuard_test.go 2>/dev/null || true)
  S13EX=$(grep -acEv '^[[:space:]]*(#|$)' src/token-door-census.sh 2>/dev/null || true)
  S13NEW=$(git diff --name-only --diff-filter=A "$BASE" HEAD -- docs/phase4/CENSUS-token-door-live-wrappers.md | grep -ac . || true)
  stamp "A-row13 the token-door census :: the three files present=$S13OK :: guard registered in projitems x$S13REG (must be 1) :: its named control test x$S13FN (must be 1) :: EXECUTABLE lines in the instrument=$S13EX (must be >= 1) :: the census record ADDED by this train=$S13NEW (must be 1)"
  { [ "$S13OK" = "1" ] && [ "${S13REG:-0}" = "1" ] && [ "${S13FN:-0}" = "1" ] && [ "${S13EX:-0}" -ge 1 ] && [ "${S13NEW:-0}" = "1" ]; } || fail_gate A-row13
  SEATASSERT_N=$(( SEATASSERT_N + 1 ))
fi

# --- row 14 :: claude/c1-mfinal-mint-door-clean -- ⚠ A PURE ANNOTATION OF A HAND-OWN FILE: every added
#     line is a comment and NOT ONE LINE OF CODE MOVES.  So the arm asserts the annotation by name AND
#     asserts the deletion column is ZERO -- a body that changed here would be a different change
#     wearing this row's name.
if [ "$SEAT14_SHA" != "PENDING" ]; then
  S14P='src/core/runtime/mfinal.cs'
  S14OK=1; [ -f "$S14P" ] || { stamp "  ^ A-row14 REFUSED: $S14P is not in the assembled tree"; S14OK=0; }
  S14DOOR=$(grep -acF -- 'POINTER-MINT DOOR (2026-09-13, C1' "$S14P" 2>/dev/null || true)
  S14NS=$(git diff --numstat "$BASE" HEAD -- "$S14P")
  S14ADD=$(printf '%s' "$S14NS" | cut -f1); S14DEL=$(printf '%s' "$S14NS" | cut -f2)
  case "$S14ADD" in ''|*[!0-9]*) S14ADD=0 ;; esac
  case "$S14DEL" in ''|*[!0-9]*) S14DEL=0 ;; esac
  S14MARK=$(grep -acF -- 'GoManualConversion' "$S14P" 2>/dev/null || true)
  stamp "A-row14 the mfinal pointer-mint door note :: file present=$S14OK :: the door note x$S14DOOR (must be >= 1; the base reads 0) :: numstat +$S14ADD/-$S14DEL (additions must be > 0 and DELETIONS MUST BE 0 -- this row annotates, it does not edit) :: the hand-own marker still x$S14MARK (must be >= 1)"
  { [ "$S14OK" = "1" ] && [ "${S14DOOR:-0}" -ge 1 ] && [ "$S14ADD" -ge 1 ] && [ "$S14DEL" = "0" ] && [ "${S14MARK:-0}" -ge 1 ]; } || fail_gate A-row14
  SEATASSERT_N=$(( SEATASSERT_N + 1 ))
fi

# --- row 15 :: claude/c2-board-both-ordered -- ⚠ THE SECOND **PRE-RESOLVED** MERGE, and the row whose
#     branch name is a claim about ORDER.  This arm is the one that reads the whole BOARD: its own block
#     AND rows 9 and 10's, each exactly once, because a resolution that kept the last writer would leave
#     a BOARD that looks healthy and has lost two dated blocks.  Rows 9/10 are read only when they
#     merged; a partial run says NOT MEASURED rather than refusing on a seat that was never there.
if [ "$SEAT15_SHA" != "PENDING" ]; then
  S15P='docs/phase4/BOARD-next-validation-candidates.md'
  S15OK=1; [ -f "$S15P" ] || { stamp "  ^ A-row15 REFUSED: $S15P is not in the assembled tree"; S15OK=0; }
  S15H=$(grep -acF -- 'WOULD SILENTLY LOSE 5 OF 11 TIMEOUT FLOORS' "$S15P" 2>/dev/null || true)
  S15BOTH=1
  if [ "$SEAT9_SHA" != "PENDING" ] && [ "$SEAT10_SHA" != "PENDING" ]; then
    S15R9=$(grep -acF -- 'door regression is EXACTLY' "$S15P" 2>/dev/null || true)
    S15R10=$(grep -acF -- 'a banked 97-verdict row killed the test host ONCE in 27 isolated runs' "$S15P" 2>/dev/null || true)
    { [ "${S15R9:-0}" = "1" ] && [ "${S15R10:-0}" = "1" ]; } || S15BOTH=0
  else
    S15R9='NOT MEASURED'; S15R10='NOT MEASURED'
  fi
  stamp "A-row15 the shard-map timeout-floor entry, through a PRE-RESOLVED merge :: BOARD present=$S15OK :: this row's block heading x$S15H (must be 1; the base reads 0) :: row 9's block still x$S15R9 and row 10's still x$S15R10 (each must be 1 whenever that row merged -- four appends to one record and two hand resolutions is exactly where a block goes missing)"
  { [ "$S15OK" = "1" ] && [ "${S15H:-0}" = "1" ] && [ "$S15BOTH" = "1" ]; } || fail_gate A-row15
  SEATASSERT_N=$(( SEATASSERT_N + 1 ))
fi

# --- row 16 :: claude/c2-merge-probe-predicate -- a SKILL and the SCRIPT it names, both admitted by an
#     allowed= ruling.  The script is read for EXECUTABLE content and the skill by BLOB IDENTITY.
if [ "$SEAT16_SHA" != "PENDING" ]; then
  S16OK=1
  for f in .claude/skills/merge-hazards/SKILL.md .claude/skills/merge-hazards/merge-probe.sh; do
    [ -f "$f" ] || { stamp "  ^ A-row16 REFUSED: $f is not in the assembled tree"; S16OK=0; }
  done
  S16EX=$(grep -acEv '^[[:space:]]*(#|$)' .claude/skills/merge-hazards/merge-probe.sh 2>/dev/null || true)
  S16RULE=$(grep -acF -- 'refuses to call an all-vacuous run evidence' .claude/skills/merge-hazards/SKILL.md 2>/dev/null || true)
  S16RB=0
  for p in .claude/skills/merge-hazards/SKILL.md .claude/skills/merge-hazards/merge-probe.sh; do
    bh=$(git rev-parse "HEAD:$p" 2>/dev/null || true); bs=$(git rev-parse "$SEAT16_SHA:$p" 2>/dev/null || true)
    if [ -n "$bh" ] && [ "$bh" = "$bs" ]; then S16RB=$(( S16RB + 1 )); else stamp "  ^ A-row16 ruled path $p union blob [$bh] != seat blob [$bs]"; fi
  done
  stamp "A-row16 the merge-probe vacuity predicate :: both files present=$S16OK :: EXECUTABLE lines in merge-probe.sh=$S16EX (must be >= 1) :: the skill's own rule sentence x$S16RULE (must be 1; the base reads 0) :: allowed= ruled paths BLOB-IDENTICAL to the seat=$S16RB of 2 (must be 2 -- a skill that names a script it does not carry is the half-merge this row is exposed to)"
  { [ "$S16OK" = "1" ] && [ "${S16EX:-0}" -ge 1 ] && [ "${S16RULE:-0}" = "1" ] && [ "$S16RB" = "2" ]; } || fail_gate A-row16
  SEATASSERT_N=$(( SEATASSERT_N + 1 ))
fi

# --- row 17 :: claude/c2-h10-shardmap-projection -- a new DATA record, asserted as ADDED and by the
#     heading that states it is a PROJECTION, because that word is the whole ruling on the file.
if [ "$SEAT17_SHA" != "PENDING" ]; then
  S17P='docs/phase4/DATA-h10-shardmap-projection-go124.md'
  S17OK=1; [ -f "$S17P" ] || { stamp "  ^ A-row17 REFUSED: $S17P is not in the assembled tree"; S17OK=0; }
  S17NEW=$(git diff --name-only --diff-filter=A "$BASE" HEAD -- "$S17P" | grep -ac . || true)
  S17H=$(grep -acF -- 'H10 roster re-derivation, Go 1.24.13 hop (lane C2)' "$S17P" 2>/dev/null || true)
  S17N=$(grep -ac . "$S17P" 2>/dev/null || true)
  stamp "A-row17 the H10 shard-map PROJECTION :: file present=$S17OK :: ADDED by this train=$S17NEW (must be 1) :: its own title x$S17H (must be 1; the base reads 0) :: non-empty lines=$S17N (must be >= 1 -- a record that merged empty records nothing)"
  { [ "$S17OK" = "1" ] && [ "${S17NEW:-0}" = "1" ] && [ "${S17H:-0}" = "1" ] && [ "${S17N:-0}" -ge 1 ]; } || fail_gate A-row17
  SEATASSERT_N=$(( SEATASSERT_N + 1 ))
fi

# --- row 18 :: claude/c2-h10-map-rederivation -- THE WIDEST ROW: a generator, the driver that runs it,
#     three records, and TWO allowed= ruled paths.  The .gitattributes LF pin and shardmap.py are read
#     by BLOB IDENTITY because the pin exists PRECISELY so the generator's inputs are byte-stable -- a
#     generator that boarded without its pin is the defect the pin was added to prevent.
if [ "$SEAT18_SHA" != "PENDING" ]; then
  S18OK=1
  for f in .gitattributes docs/phase4/hopA-inputs/shardmap.py src/run-h10-dispatch.ps1 docs/phase4/DATA-h10-map-rederivation-2026-09-13.md docs/phase4/DESIGN-h10-dispatch-driver.md docs/phase4/DATA-sweep-row-walltimes.md; do
    [ -f "$f" ] || { stamp "  ^ A-row18 REFUSED: $f is not in the assembled tree"; S18OK=0; }
  done
  S18EX=$(grep -acEv '^[[:space:]]*(#|$)' src/run-h10-dispatch.ps1 2>/dev/null || true)
  S18NEW=$(git diff --name-only --diff-filter=A "$BASE" HEAD -- docs/phase4/DATA-h10-map-rederivation-2026-09-13.md docs/phase4/DESIGN-h10-dispatch-driver.md src/run-h10-dispatch.ps1 | grep -ac . || true)
  S18DIG=$(grep -acF -- 'a CONTENT key for every block above' docs/phase4/DATA-sweep-row-walltimes.md 2>/dev/null || true)
  S18RB=0
  for p in .gitattributes docs/phase4/hopA-inputs/shardmap.py; do
    bh=$(git rev-parse "HEAD:$p" 2>/dev/null || true); bs=$(git rev-parse "$SEAT18_SHA:$p" 2>/dev/null || true)
    if [ -n "$bh" ] && [ "$bh" = "$bs" ]; then S18RB=$(( S18RB + 1 )); else stamp "  ^ A-row18 ruled path $p union blob [$bh] != seat blob [$bs]"; fi
  done
  stamp "A-row18 the H10 map re-derivation :: the six files present=$S18OK :: EXECUTABLE lines in the dispatch driver=$S18EX (must be >= 1) :: files ADDED by this train among the driver and its two records=$S18NEW (must be 3) :: the walltimes digest section appended x$S18DIG (must be >= 1; the base reads 0) :: allowed= ruled paths BLOB-IDENTICAL to the seat=$S18RB of 2 (must be 2 -- the LF pin and the generator it pins are one change and cannot board separately)"
  { [ "$S18OK" = "1" ] && [ "${S18EX:-0}" -ge 1 ] && [ "${S18NEW:-0}" = "3" ] && [ "${S18DIG:-0}" -ge 1 ] && [ "$S18RB" = "2" ]; } || fail_gate A-row18
  SEATASSERT_N=$(( SEATASSERT_N + 1 ))
fi
# === SEAT-CONTENT ASSERTIONS END ===
stamp "SEAT-CONTENT ASSERTIONS :: arms=$SEATASSERT_N :: seats merged=$SEATS_DONE (an arm per merged seat is the bar; the arms read each seat's own content out of the FILES, which is what catches a merge that resolved a hunk the wrong way -- ancestry cannot)"
if [ "$SEATASSERT_N" -lt "$SEATS_DONE" ]; then
  stamp "  ^ SEAT-CONTENT ASSERTIONS REFUSED: $SEATASSERT_N arm(s) for $SEATS_DONE merged seat(s).  A battery over a tree whose seats nobody asserted measures a tree nobody ruled on -- write the arms in the fenced block above, one per seat, each gated on its own \$SEATn_SHA != PENDING."
  stamp "      ⚠ THIS IS A **POST-BATTERY GATE**, NOT A PRE-LEG REFUSAL.  fail_gate sets FAILED and the run CONTINUES through every remaining leg; the verdict is at the DONE stamp.  It is NOT the check that stops a TRAIN48_REQUIRE_ALL=1 run with a PENDING row -- that one ABORTS with exit 2 in the SEAT TABLE's PENDING block, before a single seat is merged, and a REQUIRE_ALL run therefore never reaches this line.  Reaching it means a row MERGED whose arm nobody wrote, which is a different mistake and is found at a different time."
  fail_gate SEAT-CONTENT-ASSERTIONS
fi

# --- A5 THE NEW BEHAVIORAL GUARD PROJECTS ARE **COMPLETE** --------------------------------------
#     A guard project boards only when it carries its Go source, its EMISSION, its package_info.cs,
#     its GOLDEN, its solution registration and its four MSTest registrations.
#     ⚠ TWO POPULATIONS, AND THE SECOND IS WHY ROUTE #3 EXISTS.  The TOP-LEVEL projects owe all six.
#     A NESTED SUB-LIBRARY owes its solution registration and its emission and NOTHING ELSE: it has no
#     `.cs.target` by design (UpdateTestTargets is top-level-only) and no MSTest registration, and its
#     drift is caught by CNR's `git status` while its CROSS-PACKAGE effect is caught by its parent's
#     golden.  Asserting a golden on a sub-library would be a red-by-construction arm; asserting NO
#     registration for it would re-open the hole that let an unregistered sub-library break only
#     Visual Studio.  So the two populations are asserted SEPARATELY and each against its own bar.
A5BAD=0
if [ "${NEWBEH:-0}" = "0" ]; then
  # ⚠ THE EXPECTATION IS THE **OWED VECTOR'S**, NOT A CARRIED PREMISE.  Train 46's arm said "with both
  #   named guard seats aboard this must be 2" and REFUSED a zero -- a fact about that train, and it
  #   would refuse every healthy launch of THIS one, whose fifteen rows add no behavioral project at all
  #   (row 8 RE-EMITS an existing guard -- seven MODIFIED files, zero additions -- which is why its class
  #   is converter-guard-rebaseline and does not set OWED_BEH).
  #   Owed-and-absent refuses; not-owed is a stated READING that can never refuse.
  if [ "$OWED_BEH" = "1" ]; then
    stamp "  ^ A5 REFUSED: a merged row's CLASS adds a behavioral guard project (OWED_BEH=1) and yet ZERO new TOP-LEVEL behavioral projects are in the delta -- that row did not land what its class says it is."
    fail_gate A5-owed-and-absent
  else
    stamp "A5 :: this train adds NO new behavioral guard project (new TOP-LEVEL=$NEWBEH, ALL DEPTHS=$NEWBEH_ALL) and OWED_BEH=$OWED_BEH says that is the EXPECTED state -- a READING, not a refusal, and not a gate that was skipped.  ⚠ It is STATED rather than silent because A5 is the arm route #3 lives in, and 'no projects' and 'the arm did not run' are the same output otherwise."
  fi
else
  while IFS= read -r pj <&3; do
    [ -n "$pj" ] || continue
    d="src/tests/Behavioral/$pj"
    miss=""
    for want in "$d/$pj.csproj" "$d/go.mod" "$d/main.go" "$d/main.cs" "$d/package_info.cs" "$d/main.cs.target"; do
      [ -f "$want" ] || miss="$miss $(basename "$want")"
    done
    reg=$(grep -ac "tests/Behavioral/$pj/$pj\.csproj" src/go2cs.slnx 2>/dev/null || true)
    mst=$(grep -alc "Check$pj\b" src/tests/Behavioral/BehavioralTests/*Tests.cs 2>/dev/null | grep -ac . || true)
    attr=$(tr -d '\r' < "$d/package_info.cs" 2>/dev/null | grep -acE '^[[:space:]]*\[GoTestMatchingConsoleOutput\][[:space:]]*$' || true)
    stamp "A5 TOP-LEVEL $pj :: missingFiles=[${miss:- none}] slnxRegistrations=$reg (must be 1) mstestClassesNamingIt=$mst (must be 4) outputAttr=$attr (1 makes it Output-COMPARED; 0 makes it Target-only, which is a WEAKER guard and is stated rather than assumed)"
    [ -z "$miss" ] || { stamp "  ^ A5 REFUSED: $pj is a HALF PROJECT.  A behavioral project without its golden cannot be Target-compared and one without a package_info.cs is counted UNMARKED by the runner; either way LEG 5 would measure a project nobody finished."; A5BAD=1; }
    [ "${reg:-0}" = "1" ] || { stamp "  ^ A5 REFUSED: $pj is registered $reg time(s) in go2cs.slnx (must be exactly 1)."; A5BAD=1; }
    [ "${mst:-0}" = "4" ] || { stamp "  ^ A5 REFUSED: $mst of the four MSTest classes name Check$pj."; A5BAD=1; }
  done 3<<< "$NEWBEH_LIST"
  # --- the NESTED half.  Every new .csproj at ANY depth must be registered EXACTLY ONCE, and the
  #     nested ones are asserted for REGISTRATION and EMISSION only.
  while IFS= read -r cp <&3; do
    [ -n "$cp" ] || continue
    # ⚠ TOP-LEVEL IS DECIDED BY **SEGMENT COUNT**, NEVER BY A CASE GLOB.  (Carried template defect (b),
    #   fixed 2026-09-13 before first use.)  `src/tests/Behavioral/[!/]*/[!/]*.csproj` reads as "one
    #   non-slash character then ANYTHING" -- `*` crosses `/` in a shell pattern -- so a NESTED
    #   sub-library at src/tests/Behavioral/P/sub/sub.csproj MATCHES it, is skipped here as
    #   "handled above", and is asserted by NOTHING at all.  That is route #3 re-opened by the very
    #   arm written to close it.  `src/tests/Behavioral/P/P.csproj` is FIVE segments; anything deeper
    #   is nested, and the count is unambiguous.
    cpseg=$(printf '%s' "$cp" | awk -F/ '{print NF}')
    [ "$cpseg" = "5" ] && continue                                            # top-level: handled above
    rel="${cp#src/tests/Behavioral/}"
    nreg=$(grep -ac "tests/Behavioral/$rel" src/go2cs.slnx 2>/dev/null || true)
    ndir=$(dirname "$cp")
    npi=$( [ -f "$ndir/package_info.cs" ] && echo 1 || echo 0 )
    ngo=$(ls "$ndir"/*.go 2>/dev/null | grep -ac . || true)
    ncs=$(ls "$ndir"/*.cs 2>/dev/null | grep -avc 'package_info\.cs$' || true)
    stamp "A5 NESTED $rel :: slnxRegistrations=$nreg (must be 1 -- an unregistered sub-library passes EVERY harness gate and breaks only Visual Studio) :: package_info.cs=$npi (must be 1 -- it is an INPUT to its parent's transpile) :: .go sources=$ngo emitted .cs=$ncs (both >=1) :: ⚠ NO golden and NO MSTest registration are owed here, by UpdateTestTargets' top-level-only design"
    { [ "${nreg:-0}" = "1" ] && [ "$npi" = "1" ] && [ "${ngo:-0}" -ge 1 ]; } || { stamp "  ^ A5 REFUSED: the nested sub-library $rel is not registered exactly once, or has no package_info.cs, or has no Go source.  Route #3: nested packages were transpiled by NO gate at all until the enumerators were made recursive, and a sub-library's package_info.cs is what its PARENT reads to decide whether to mint a value adapter -- so a stale or absent one silently disarms the parent's guard."; A5BAD=1; }
  done 3<<< "$NEWBEH_ALL_LIST"
  [ "$A5BAD" = "0" ] || fail_gate A5-nested
fi

# --- A6 NO SEAT MAY MOVE THE TOOLCHAIN DECLARATION OR THE CORPUS PIN ---------------------------
A6MOD=$(grep -acE '^go 1\.24\.13$' src/go2cs/go.mod 2>/dev/null || true)
A6PIN=$(sed -nE 's#.*<GoStdLibVersion>([^<]+)</GoStdLibVersion>.*#\1#p' src/version.props 2>/dev/null | tail -1)
A6DIFF=$(git diff --name-only "$BASE" HEAD -- src/go2cs/go.mod src/go2cs/go.sum src/version.props | grep -ac . || true)
stamp "A6 pins UNMOVED :: src/go2cs/go.mod declares go 1.24.13 x$A6MOD (must be 1) :: version.props <GoStdLibVersion>=${A6PIN:-unreadable} (must be 1.23.12) :: files among {go.mod, go.sum, version.props} in this train's delta=$A6DIFF (must be 0 -- no seat here has any business moving a pin)"
{ [ "$A6MOD" = "1" ] && [ "${A6PIN:-}" = "1.23.12" ] && [ "${A6DIFF:-1}" = "0" ]; } || { stamp "  ^ A6 REFUSED: a pin moved, or a seat touched a pin file.  Every per-leg toolchain decision in this script, and LEG D's and LEG K's two-pin shapes, was derived for go.mod=1.24.13 and version.props=1.23.12 -- neither is still true, so stop."; FAILED=1; }

# --- A7 NO SEAT MAY MOVE A COMMITTED BEHAVIORAL GOLDEN THAT ALREADY EXISTED --------------------
#     A changed golden is a RE-BASELINE, and a re-baseline is a RULING, not a merge.  The four
#     BehavioralTests/*Tests.cs classes ARE modifications and are the one structural exception (every
#     seat adding a guard project rewrites them).
#     ⚠ **THE RULED EXCEPTIONS COME FROM THE SEAT TABLE'S SIXTH FIELD, NOT FROM THIS BLOCK.**  Train
#     46 refused a legitimate re-baseline at run 2 and the fix was hand-written here as a named
#     project and a named seat variable -- correct for that train and a fact about it.  A seat that
#     rules a re-baseline declares `allowed=<ERE>` in its OWN ROW; A7 then admits exactly those paths
#     and ONLY when the union's BLOB for each equals THAT seat's own blob, so the exemption carries
#     nothing another seat rides in on.  With no `allowed=` row the strict form runs and nothing under
#     src/tests/Behavioral may be modified at all.
A7MOD=$(git -c core.quotepath=false diff --name-only --diff-filter=MD "$BASE" HEAD -- src/tests/Behavioral | grep -ac . || true)
# the allowed pattern, built from the rows that declare one.  ⚠ With NO such row the pattern is a
# SENTINEL that cannot match any path, so the strict form is the default BY CONSTRUCTION rather than
# by an `if` a later edit could invert.
A7ALLOW='^<<<NO-A7-ALLOWED-SET-DECLARED>>>$'
A7MAP="/tmp/${TAG}-a7-allowed.txt"; : > "$A7MAP"
while IFS='|' read -r an ab as ac am aa <&3; do
  [ -n "$an" ] || continue
  # ⚠ READ THROUGH seat_opt, NEVER POSITIONALLY.  With two optional keys, `read -r ... aa` puts every
  #   field from six on into `aa` delimiters and all, and a positional `allowed=*` test then misses a
  #   ruling written in field seven -- which reads as "no ruling declared", i.e. as the STRICT default.
  apat="$(seat_opt "$(seat_row "$an")" allowed)"
  [ -n "$apat" ] || continue
  [ "$as" != "PENDING" ] || continue
  # (the allowed= value is read through seat_opt above; nothing positional is left here)
  printf '%s\t%s\t%s\n' "$an" "$as" "$apat" >> "$A7MAP"
  case "$A7ALLOW" in
    '^<<<NO-A7-ALLOWED-SET-DECLARED>>>$') A7ALLOW="$apat" ;;
    *) A7ALLOW="$A7ALLOW|$apat" ;;
  esac
done 3<<< "$SEAT_TABLE"
A7ROWS=$(grep -ac . "$A7MAP" || true)
A7MODX=$(git -c core.quotepath=false diff --name-only --diff-filter=MD "$BASE" HEAD -- src/tests/Behavioral | grep -avE '^src/tests/Behavioral/BehavioralTests/[^/]+Tests\.cs$' | grep -avE "$A7ALLOW" | grep -ac . || true)
A7ALLOWED=$(git -c core.quotepath=false diff --name-only --diff-filter=MD "$BASE" HEAD -- src/tests/Behavioral | grep -aE "$A7ALLOW" | grep -ac . || true)
A7SEATONLY=0
for a7f in $(git -c core.quotepath=false diff --name-only --diff-filter=MD "$BASE" HEAD -- src/tests/Behavioral | grep -aE "$A7ALLOW"); do
  # WHICH seat owns this path?  The row whose own pattern matches it.  ⚠ If more than one does, the
  # rulings overlap and the comparison below is ambiguous -- that is REFUSED rather than resolved.
  a7owner=""; a7ownersha=""; a7n=0
  while IFS="$(printf '\t')" read -r mn ms mp; do
    [ -n "$mn" ] || continue
    if [ "$(printf '%s\n' "$a7f" | grep -acE "$mp" || true)" != "0" ]; then a7owner="$mn"; a7ownersha="$ms"; a7n=$(( a7n + 1 )); fi
  done < "$A7MAP"
  if [ "$a7n" != "1" ]; then
    stamp "  ^ A7 REFUSED: $a7f is matched by $a7n allowed= pattern(s).  A path ruled by two seats has no single owner to compare its blob against, and one ruled by none reached this loop through the alternation -- either way the exemption cannot be checked."
    A7SEATONLY=1; continue
  fi
  a7u=$(git diff "$BASE" HEAD -- "$a7f" | grep -aE '^[-+][^-+]' | sort | md5sum | cut -c1-12)
  a7s=$(git diff "$(git merge-base "$BASE" "$a7ownersha")" "$a7ownersha" -- "$a7f" | grep -aE '^[-+][^-+]' | sort | md5sum | cut -c1-12)
  a7bu=$(git rev-parse "HEAD:$a7f" 2>/dev/null | cut -c1-12); a7bs=$(git rev-parse "$a7ownersha:$a7f" 2>/dev/null | cut -c1-12)
  if [ "$a7u" = "$a7s" ] && [ -n "$a7bu" ] && [ "$a7bu" = "$a7bs" ]; then
    stamp "A7 allowed $a7f :: ruled by seat $a7owner :: union BLOB == that seat's BLOB ($a7bu) -- identity -- and the line-set agrees ($a7u)"
  else
    stamp "  ^ A7 REFUSED: $a7f is in seat $a7owner's allowed set but the union's blob ($a7bu) is NOT that seat's ($a7bs), or the line-set differs ($a7u vs $a7s) -- another seat rides on the exemption"
    A7SEATONLY=1
  fi
  # --- and the GOLDEN-IS-THE-EMISSION arm, DERIVED FROM THE PATH rather than from a project name.
  #     A `.cs` whose sibling `.cs.target` exists must be the SAME BLOB: a golden that is not its
  #     emission is a re-baseline that re-baselined nothing.
  case "$a7f" in
    *.cs)
      a7t="${a7f}.target"
      if git cat-file -e "HEAD:$a7t" 2>/dev/null; then
        a7gold=$(git rev-parse "HEAD:$a7t" 2>/dev/null | cut -c1-12)
        if [ "$a7bu" = "$a7gold" ]; then stamp "A7 golden IS the emission :: $a7f blob == $(basename "$a7t") blob ($a7bu)"
        else stamp "  ^ A7 REFUSED: $a7f ($a7bu) and $(basename "$a7t") ($a7gold) are DIFFERENT blobs in the union -- a golden that is not its emission"; A7SEATONLY=1; fi
      fi ;;
  esac
done
stamp "A7 behavioral tree :: files under src/tests/Behavioral MODIFIED or DELETED in this train's delta=$A7MOD :: rows declaring an allowed= ruling=$A7ROWS :: files admitted by those rulings=$A7ALLOWED :: NOT one of the four BehavioralTests classes and NOT allowed=$A7MODX (must be 0) :: allowed-set changes that are not their own seat's=$A7SEATONLY (must be 0)"
[ "$A7SEATONLY" = "0" ] || fail_gate A7-allowed-identity
[ "${A7MODX:-1}" = "0" ] || { stamp "  ^ A7 REFUSED: a committed behavioral file other than the four MSTest classes (and outside any ruled allowed= set) was MODIFIED or DELETED.  A re-baseline is a RULING: declare it in the seat's own table row as allowed=<ERE>, with the ruling written at the row.  Files:"; git -c core.quotepath=false diff --name-only --diff-filter=MD "$BASE" HEAD -- src/tests/Behavioral | grep -avE '^src/tests/Behavioral/BehavioralTests/[^/]+Tests\.cs$' | grep -avE "$A7ALLOW" | sed 's/^/    /' | tee -a "$ASMLOG"; fail_gate A7; }

# --- A7b THE `allowed=` RULINGS **OUTSIDE** THE BEHAVIORAL TREE -------------------------------------
#     ⚠ A7's SCOPE IS UNCHANGED AND DELIBERATELY SO: it reads src/tests/Behavioral with
#     --diff-filter=MD, which is the tree it was written for and the tree whose goldens a re-baseline
#     moves.  COORD's 2026-09-13 19:05 ruling makes `allowed=` a ruling over the WHOLE seat, so the
#     ruled paths that are NOT behavioral need the same identity predicate -- read ONCE over the
#     finished union, because the question this arm answers is what the UNION carries, and a later
#     seat can move a path an earlier seat's ruling admitted.
#     ⚠ **THE OWNER OF A PATH RULED BY TWO ROWS IS THE LAST ROW IN MERGE ORDER.**  A7's own loop
#     REFUSES a path matched by more than one pattern, because within one behavioral re-baseline two
#     owners is an ambiguity.  Here it is a RULE rather than an accident, and the rule is stated even
#     though THIS table no longer exercises it: COORD's E1 ruling (~20:00) dropped row 13's inherited
#     `allowed=` as STALE, so each ruled path outside the behavioral tree is now ruled by EXACTLY ONE
#     row and A7b's owner loop reads a7bcand=1 every time.  The owner rule stays because the SHAPE
#     recurs -- a declared stack whose child re-touches a ruled path would restore it -- and the
#     self-check's arm (g) keeps a PLANTED overlap as its red control so the rule is exercised by
#     something even when the table does not exercise it.  The decision is STAMPED with the count of
#     matching rows and the owner named, so a reader can see that a choice was made rather than that
#     one pattern happened to win.
A7BBAD=0; A7BN=0
A7BPATHS=$(while IFS="$(printf '\t')" read -r mn ms mp; do
  [ -n "$mn" ] || continue
  seat_allowed_paths "$mp"
done < "$A7MAP")
for a7bp in $(printf '%s\n' "$A7BPATHS" | grep -a . | sort -u); do
  case "$a7bp" in src/tests/Behavioral/*) continue ;; esac
  a7bowner=""; a7bownersha=""; a7bcand=0
  # ⚠ THE LOOP DOES NOT `break`: it reads A7MAP in MERGE ORDER and keeps the LAST match, which IS the
  #   owner rule.  A break here would silently make the FIRST row the owner and the assertion would
  #   then compare the union against a blob a later seat legitimately replaced.
  while IFS="$(printf '\t')" read -r mn ms mp; do
    [ -n "$mn" ] || continue
    if [ "$(printf '%s\n' "$a7bp" | grep -acE "$mp" || true)" != "0" ]; then
      a7bowner="$mn"; a7bownersha="$ms"; a7bcand=$(( a7bcand + 1 ))
    fi
  done < "$A7MAP"
  A7BN=$(( A7BN + 1 ))
  a7bbu=$(git rev-parse "HEAD:$a7bp" 2>/dev/null | cut -c1-12)
  a7bbs=$(git rev-parse "$a7bownersha:$a7bp" 2>/dev/null | cut -c1-12)
  if [ -n "$a7bbu" ] && [ "$a7bbu" = "$a7bbs" ]; then
    stamp "A7b allowed $a7bp :: ruled by $a7bcand row(s), OWNER=seat $a7bowner (the LAST in merge order) :: union BLOB == that seat's BLOB ($a7bbu) -- identity"
  else
    stamp "  ^ A7b REFUSED: $a7bp is ruled by seat $a7bowner (the last of $a7bcand matching row(s)) but the union's blob (${a7bbu:-ABSENT}) is NOT that seat's (${a7bbs:-ABSENT}) -- the exemption is carrying content that seat did not write."
    A7BBAD=1
  fi
done
stamp "A7b allowed= rulings OUTSIDE src/tests/Behavioral :: ruled paths asserted=$A7BN :: identity failures=$A7BBAD (must be 0).  A7's own scope (src/tests/Behavioral, --diff-filter=MD) is UNCHANGED; this arm sits BESIDE it and never inside it."
[ "$A7BBAD" = "0" ] || fail_gate A7b-allowed-identity
[ "$FAILED" = "0" ] || { stamp "=== A-ASSERTIONS FAILED -- the assembled tree is not the tree this train was gated for.  STOPPING before any leg: every reading below would describe a tree nobody ruled on. ==="; stamp "=== ${LABEL} ASSEMBLE DONE head=$HEADSHA base=$BASE overallFailed=1 (stopped at the A-assertions) ==="; exit 1; }
DOCFILES=$(git diff --name-only "$BASE" HEAD -- docs | grep -E '\.md$' || true)
if [ -z "$DOCFILES" ]; then
  stamp "DOCS-DELTA :: 0 docs/*.md files in this train's delta.  ⚠ WHETHER THAT IS A FINDING IS DECIDED BY THE **OWED VECTOR**, NOT BY A CARRIED PREMISE: OWED_DOCS=$OWED_DOCS (1 means at least one merged seat's CLASS is one that normally carries a record).  Train 45's arm refused unconditionally because four of its seats were docs seats -- a fact about that train -- and train 46 checked it against the skip count.  Here it is checked against the vector AND the skip count, which is the honest pair: a class may legitimately carry no record, so this is a FINDING to reconcile rather than a refusal."
  if [ "$OWED_DOCS" = "1" ] && [ "$SEATS_SKIPPED" = "0" ]; then
    stamp "  ^ DOCS-DELTA FINDING: no seat was skipped, at least one merged class normally carries a record, and yet ZERO docs/*.md are in the delta.  Read the seat notes: either a seat did not land its record, or a class that does not owe one is doing the work -- and the second is a note to make, not a silence to keep."
    fail_gate DOCS-DELTA
  fi
else
  stamp "DOCS-DELTA :: $(printf '%s\n' "$DOCFILES" | grep -c .) docs/*.md file(s) in the delta"
  while IFS= read -r f <&3; do
    [ -n "$f" ] || continue
    ns=$(git diff --numstat "$BASE" HEAD -- "$f")
    add=$(printf '%s' "$ns" | cut -f1); del=$(printf '%s' "$ns" | cut -f2)
    case "$del" in
      ''|*[!0-9]*) stamp "DOCS-DELTA $f :: numstat not numeric ('$ns') -- UNMEASURED"; FAILED=1; continue ;;
    esac
    case "$f" in
      docs/GoCorpusMigration.md|docs/DotNetMigration.md) lim=30; kind="RUNBOOK (living procedure, amended in-stage; a step rewrite IS an amendment there, which is why its allowance is larger)" ;;
      *) lim=10; kind="RECORD (dated blocks only)" ;;
    esac
    stamp "DOCS-DELTA $f :: numstat=+$add/-$del (deletions must be <= $lim for a $kind)"
    if [ "$del" -gt "$lim" ]; then
      stamp "  ^ REFUSED: $del deletions exceed $lim -- this is a REWRITE, not an amendment"
      FAILED=1
    fi
  done 3<<< "$DOCFILES"
fi

# ============================================================================================
# LIGHT GATES -- seconds to minutes, all before the heavy legs, so a refusal costs nothing.
# ============================================================================================

# --- G1 roster format guard --------------------------------------------------------------------
#     ⚠ IT IS A **NO-REGRESSION READING ON THIS TRAIN, NOT A SEAT GATE**, AND THAT IS CARRIED RATHER
#     THAN RE-MEASURED.  Train 47's derive measured ITS OWN seat tips at a02ac3df3 and found no row
#     editing docs/ValidatedTestPackages.md (the roster this guard recomputes).  ⚠ THIS TABLE HAS NOT
#     BEEN MEASURED FOR IT: a carried "which row edits the roster" premise is the stale-figure class
#     (L13) wearing a measurement's clothes, so nothing here names a row.  It stays ONE run: the guard
#     walks the whole roster and every committed manifest, so one invocation covers every row that
#     touches either and counting it twice would be two readings of one measurement.  ⚠ ITS VERDICT IS THE
#     `checks pass` LINE PLUS THE EXIT CODE, never a word grep: a guard that iterates its own input and
#     compares the count against that same input can never fail, which is the fault this very guard
#     carried at master until its manifest-coverage arm was given a SECOND, independent walk.
G1LOG="/tmp/${TAG}-g1-roster.log"; : > "$G1LOG"
if [ ! -f src/check-roster-format.ps1 ]; then
  stamp "G1 UNMEASURED :: src/check-roster-format.ps1 is absent from the assembled tree"
  FAILED=1
else
  G1WIRED=$(grep -acF 'check-roster-format.ps1' "$SCRIPT_PATH" || true)
  G1LAUNCH=$(grep -acE '^ *powershell .*-File src/check-roster-format\.ps1' "$SCRIPT_PATH" || true)
  stamp "G1 WIRING :: this script names check-roster-format.ps1 $G1WIRED time(s) and LAUNCHES it $G1LAUNCH time(s) (must be exactly 1 launch -- one run covers every roster cell this train's docs-class rows touch; a second launch would be one measurement reported twice), read from the ABSOLUTE self-path $SCRIPT_PATH"
  [ "${G1LAUNCH:-0}" = "1" ] || { stamp "  ^ G1 REFUSED: the roster guard is launched $G1LAUNCH time(s), not once"; FAILED=1; }
  powershell -NoProfile -ExecutionPolicy Bypass -File src/check-roster-format.ps1 > "$G1LOG" 2>&1 < /dev/null; rc=$?
  # ⚠ AN **ANCHORED ROW MATCH** PLUS ITS UNANCHORED SECOND READING (L15).  Measured from the
  #   producer: src/check-roster-format.ps1 prints `roster format guard: <n> checks pass (...)` as one
  #   line at column 0.  The bare `[0-9]+ checks pass` would also match that phrase inside any future
  #   summary line the guard grows, and the two readings are stamped together so a disagreement is a
  #   FINDING rather than a silent change of what this gate counts.
  G1PASS=$(tr -d '\r' < "$G1LOG" | grep -acE '^[[:space:]]*roster format guard: [0-9]+ checks pass' || true)
  G1PASS_ANY=$(tr -d '\r' < "$G1LOG" | grep -acE '[0-9]+ checks pass' || true)
  [ "${G1PASS:-0}" = "${G1PASS_ANY:-0}" ] || stamp "  G1 READING NOTE :: the anchored verdict rows ($G1PASS) and the bare phrase count ($G1PASS_ANY) DISAGREE -- the guard printed that phrase somewhere other than its own verdict row, and the anchored count is the one this gate uses" 
  stamp "G1 roster guard exit=$rc :: checks-pass line(s)=$G1PASS (must be >= 1 -- an exit 0 with NO such line is a guard that did not reach its verdict, which is a different state from a pass) :: $(tr -d '\r' < "$G1LOG" | grep -aE 'checks pass|FAIL' | tail -1 | cut -c1-140)"
  [ "${G1PASS:-0}" -ge 1 ] || { stamp "  ^ G1 REFUSED: the guard produced no 'N checks pass' line, so its exit code is describing something other than a completed run"; FAILED=1; }
  [ "$rc" = "0" ] || { FAILED=1; stamp "  ^ G1 FAIL lines:"; tr -d '\r' < "$G1LOG" | grep -aiE 'fail' | head -10 | sed 's/^/    /' | tee -a "$ASMLOG"; }
fi

# --- G2 both-edition parse of every changed .ps1 ----------------------------------------------
#     ⚠ THE EXPECTATION IS A **READING IN BOTH DIRECTIONS, DERIVED FROM THE OWED VECTOR**, never a
#     number carried from another train.  Train 43's leg REQUIRED two changed .ps1 and a zero meant a
#     seat had not landed; train 47's derive then wrote "no seat touches a .ps1 at all, so a zero is
#     the expected reading" -- a fact about THAT table.  This train carries a `tooling` class whose
#     whole deliverable can BE a .ps1, so G11(a)'s tooling arm is what says whether a non-zero is
#     OWED, and the stamps below report the count without announcing either value as a surprise.  An
#     expectation spelled here and an owe derived there is two answers to one question.
G2LOG="/tmp/${TAG}-g2-parse.log"; : > "$G2LOG"
PS1S=$(git diff --name-only "$BASE" HEAD -- '*.ps1' || true)
G2N=$(printf '%s\n' "$PS1S" | grep -c . || true)
if [ -z "$PS1S" ]; then
  stamp "G2 :: 0 changed .ps1 in this train's delta (a READING -- whether one is OWED is G11(a)'s tooling arm's to say, and it says so there).  The C2 preflight proved both editions live, so this zero is the tree's, not the instrument's."
else
  stamp "G2 :: $G2N changed .ps1 to parse under:$G2_EDITIONS (a READING -- whether a changed .ps1 is OWED is G11(a)'s tooling arm's to say).  Parsing every one of them, and the COUNT is reconciled against the OWED VECTOR before landing."
  while IFS= read -r f <&3; do
    [ -n "$f" ] || continue
    for ed in $G2_EDITIONS; do
      parse_ps1 "$ed" "$(cygpath -wa "$f")" "$G2LOG" < /dev/null; prc=$?
      stamp "G2 parse $f under $ed exit=$prc"
      [ "$prc" = "0" ] || FAILED=1
    done
  done 3<<< "$PS1S"
fi

# --- G3 push-nuget -VerifyOnly at the ASSEMBLED tree -------------------------------------------
#     -VerifyOnly runs the whole pre-flight and exits BEFORE anything is bumped, tagged, frozen, packed
#     or pushed, so it costs no disk.  No seat here touches the release path; this is the cheapest
#     whole-corpus consistency reading available and it is run for that.
#     REFUSED: exit != 0 | the census line absent or not carrying exactly four integers | the four not
#              all EQUAL | a PRE-FLIGHT FAILED line | no 'Pre-flight clean'
#     STAMPED: whether the four equal EXPECT_G3 (fill point F4).  A roster bank landing between the fill and the assembly
#              moves all four together and legitimately; refusing on the literal would false-red that.
G3LOG="/tmp/${TAG}-g3-verifyonly.log"; : > "$G3LOG"
if [ ! -f src/push-nuget.ps1 ]; then
  stamp "G3 UNMEASURED :: src/push-nuget.ps1 is absent from the assembled tree"
  FAILED=1
else
  G3_T0=$(date +%s)
  powershell -NoProfile -ExecutionPolicy Bypass -File src/push-nuget.ps1 -VerifyOnly > "$G3LOG" 2>&1 < /dev/null; g3rc=$?
  G3_WALL=$(( $(date +%s) - G3_T0 ))
  G3LINE=$(tr -d '\r' < "$G3LOG" | grep -aE 'Release census:' | tail -1)
  G3NUMS=$(printf '%s\n' "$G3LINE" | grep -aoE '[0-9]+' | tr '\n' ' ')
  G3CNT=$(printf '%s\n' "$G3LINE" | grep -aoE '[0-9]+' | grep -ac . || true)
  # ⚠ ANCHORED ROW MATCHES PLUS THEIR UNANCHORED SECOND READINGS (L15).  Measured from the producer:
  #   src/push-nuget.ps1 prints the clean verdict through `Write-Step`, which prefixes `==> `, and the
  #   failure verdict through `Write-Host` at column 0.  Leading whitespace is tolerated because a
  #   redirected PowerShell host may indent; what is EXCLUDED is the phrase appearing mid-line, which
  #   is the shape a tally produces.
  G3CLEAN=$(tr -d '\r' < "$G3LOG" | grep -acE '^[[:space:]]*==> Pre-flight clean' || true)
  G3CLEAN_ANY=$(tr -d '\r' < "$G3LOG" | grep -ac 'Pre-flight clean' || true)
  G3BAD=$(tr -d '\r' < "$G3LOG" | grep -acE '^[[:space:]]*PRE-FLIGHT FAILED' || true)
  G3BAD_ANY=$(tr -d '\r' < "$G3LOG" | grep -ac 'PRE-FLIGHT FAILED' || true)
  { [ "${G3CLEAN:-0}" = "${G3CLEAN_ANY:-0}" ] && [ "${G3BAD:-0}" = "${G3BAD_ANY:-0}" ]; } || stamp "  G3 READING NOTE :: an anchored verdict count and its bare phrase count DISAGREE (clean $G3CLEAN vs $G3CLEAN_ANY, failed $G3BAD vs $G3BAD_ANY) -- push-nuget printed a verdict phrase somewhere other than its own verdict row; the ANCHORED counts are the ones this gate uses" 
  G3PROB=$(tr -d '\r' < "$G3LOG" | sed -nE 's/.*PRE-FLIGHT FAILED -- ([0-9]+) problem.*/\1/p' | tail -1)
  stamp "G3 push-nuget -VerifyOnly exit=$g3rc wall=${G3_WALL}s :: census=[$G3NUMS] (green badges / proof pages / roster rows / .tests.csproj) integersOnTheLine=$G3CNT problems=${G3PROB:-0} preFlightCleanLines=$G3CLEAN preFlightFailedLines=$G3BAD"
  if [ "$G3CNT" != "4" ]; then
    stamp "  ^ G3 UNMEASURED: the 'Release census:' line is absent or does not carry exactly four integers -- the script's output FORMAT changed, or Write-Host output was not captured.  Read $G3LOG."
    FAILED=1
  else
    G3A=$(echo $G3NUMS | cut -d' ' -f1); G3B=$(echo $G3NUMS | cut -d' ' -f2)
    G3C=$(echo $G3NUMS | cut -d' ' -f3); G3D=$(echo $G3NUMS | cut -d' ' -f4)
    if [ "$G3A" = "$G3B" ] && [ "$G3B" = "$G3C" ] && [ "$G3C" = "$G3D" ]; then
      stamp "G3 census INVARIANT holds :: all four sets agree at $G3A"
      # ⚠ THE EXPECTATION IS **FILL POINT F4**, NOT A LITERAL.  Train 47 spelled `204` here; roster
      #   rows move between trains, and a figure spelled into a gate either refuses a healthy battery a
      #   landing later or agrees with it by coincidence -- and the log cannot tell those apart.
      if [ "$G3A" = "$EXPECT_G3" ]; then
        stamp "G3 EXPECTATION MET :: $G3A/$G3B/$G3C/$G3D, and EXPECT_G3=$EXPECT_G3 is the coordinator's stated figure for THIS train (fill point F4)"
      else
        stamp "G3 EXPECTATION DISAGREES (stamped, NOT refused) :: the four agree at $G3A where EXPECT_G3 (fill point F4) reads $EXPECT_G3.  A roster bank landing between the fill and the assembly moves all four together and is legitimate; a figure that moved for any OTHER reason is a finding.  Reconcile before landing."
      fi
    else
      stamp "  ^ G3 REFUSED: the four census sets DISAGREE ($G3A / $G3B / $G3C / $G3D).  push-nuget's own census requires every id to appear in all four, so a disagreement is a real hole -- read the problem lines in $G3LOG."
      FAILED=1
    fi
  fi
  [ "$g3rc" = "0" ] || { stamp "  ^ G3 exit was non-zero -- the pre-flight threw; problem lines:"; tr -d '\r' < "$G3LOG" | grep -aiE 'problem|FAILED|throw|Exception' | head -12 | sed 's/^/    /' | tee -a "$ASMLOG"; FAILED=1; }
  [ "$G3BAD" = "0" ]   || { stamp "  ^ G3 REFUSED: a PRE-FLIGHT FAILED line is present (${G3PROB:-?} problem(s))"; FAILED=1; }
  [ "$G3CLEAN" != "0" ] || { stamp "  ^ G3 REFUSED: the 'Pre-flight clean' line is ABSENT -- -VerifyOnly did not reach its own success path"; FAILED=1; }
fi

# --- G4 CLAUDE.md STRUCTURAL CHECK -- ⚠ ITS EXPECTATION IS **DERIVED**, NOT CARRIED. ----------
#     Train 46 run 3 refused here with "CLAUDE.md is UNTOUCHED ... the doctrine row IS seated" --
#     train 45's premise, carried into a train that had no doctrine seat.  A premise about which seat
#     is the doctrine batch is a fact about ONE train; what a template can know is whether any MERGED
#     row carries the class `doctrine`.  So:
#         OWED_CLAUDEMD=1  CLAUDE.md MUST be in the delta, and the structural arms run with teeth.
#         OWED_CLAUDEMD=0  CLAUDE.md must NOT be in the delta.
#     BOTH readings are stamped and only the MISMATCH refuses -- an expectation that cannot be wrong
#     in one direction is half a gate.
#     ⚠ THE DEFAULTS BELOW POINT AT THE **REFUSAL**, NOT AT THE GREEN.  The assignment keeps its
#     `grep -ac . || true` shape (the `|| true` is what stops a zero-match grep killing the step), so
#     an EMPTY G4TOUCHED can only come from the pipeline itself failing -- and `${G4TOUCHED:-0}` made
#     that instrument fault read as "CLAUDE.md untouched", which on THIS train (OWED_CLAUDEMD=0) is
#     the EXPECTED state: the gate would have gone green on a reading it never took.  Every consumer
#     now defaults to `1` -- the reading that puts the run in the refusing arm and into the structural
#     block -- exactly as LEG U's readings do.  Because `:-1` is the refusing direction only while
#     nothing is owed, the EMPTINESS ITSELF is refused by name immediately below, which is what makes
#     the direction of the default a belt rather than the whole gate.
G4TOUCHED=$(git diff --name-only "$BASE" HEAD -- CLAUDE.md | grep -ac . || true)
if [ -z "${G4TOUCHED:-}" ]; then
  stamp "  ^ G4 REFUSED (INSTRUMENT): the CLAUDE.md delta reading came back EMPTY.  \`grep -ac .\` prints a count or nothing at all, so an empty reading is the pipeline failing and not a file being untouched -- an unread gate is not a passed gate."
  fail_gate G4-unread
  G4TOUCHED=1
fi
stamp "G4 expectation DERIVED :: OWED_CLAUDEMD=$OWED_CLAUDEMD (a merged row carries class \`doctrine\`) :: CLAUDE.md in this train's delta=$G4TOUCHED :: they must AGREE"
if [ "$OWED_CLAUDEMD" = "1" ] && [ "${G4TOUCHED:-1}" = "0" ]; then
  stamp "  ^ G4 REFUSED: a merged seat carries class \`doctrine\` and CLAUDE.md is UNTOUCHED -- that seat did not land what its class says it is."
  fail_gate G4-expectation
elif [ "$OWED_CLAUDEMD" = "0" ] && [ "${G4TOUCHED:-1}" != "0" ]; then
  stamp "  ^ G4 REFUSED: CLAUDE.md is in the delta and NO merged seat carries class \`doctrine\`.  A doctrine edit riding in on another class is an edit nobody ruled on, and the class shapes are what should have refused it."
  fail_gate G4-expectation
fi
if [ "${G4TOUCHED:-1}" != "0" ]; then
  G4CR=$(cr_bytes CLAUDE.md); G4LF=$(lf_lines CLAUDE.md)
  G4TBL=$(git diff "$BASE" HEAD -- CLAUDE.md | grep -cE '^[-+][[:space:]]*\|')
  G4ADD=$(git diff --numstat "$BASE" HEAD -- CLAUDE.md | cut -f1); G4DEL=$(git diff --numstat "$BASE" HEAD -- CLAUDE.md | cut -f2)
  # ⚠ NORMALISE THE **RAW** VALUE.  `${G4DEL:-0}` inside a `case` substitutes before the '' arm can be
  #   reached, so the empty case is unreachable and the value is never normalised -- which is exactly
  #   how G10a printed `deletions=` EMPTY and refused on run 3.
  case "$G4ADD" in ''|*[!0-9]*) G4ADD=0 ;; esac
  case "$G4DEL" in ''|*[!0-9]*) G4DEL=0 ;; esac
  # the diff's OWN added-line count, as a SECOND derivation of the numstat.  Two readings of one
  # comparison, stated as such rather than claimed as two instruments.
  G4DIFFADD=$(git diff "$BASE" HEAD -- CLAUDE.md | grep -cE '^\+[^+]|^\+$')
  G4MARK=$(grep -ac '^<<<<<<<' CLAUDE.md || true)
  # BOM state, BOTH sides.  A doctrine batch must not add or remove one.
  G4BOM_BASE=$(git show "$BASE:CLAUDE.md" | head -c 3 | od -An -tx1 | tr -d ' \n')
  G4BOM_HEAD=$(head -c 3 CLAUDE.md | od -An -tx1 | tr -d ' \n')
  stamp "G4 CLAUDE.md structural :: CR=$G4CR LF=$G4LF (must be EQUAL under the eol=crlf pin) numstat=+${G4ADD}/-${G4DEL} diffAddedLines=$G4DIFFADD (a second reading of the same comparison; it may exceed the numstat by 0 and must not be less) tableLinesInDelta=$G4TBL (must be 0) conflictMarkers=$G4MARK (must be 0) BOM base=[$G4BOM_BASE] head=[$G4BOM_HEAD] (must be EQUAL)"
  [ "$G4CR" = "$G4LF" ] || { stamp "  ^ G4 REFUSED: CR bytes ($G4CR) != LF lines ($G4LF) -- the working-tree form is pinned CRLF by .gitattributes"; fail_gate G4-crlf; }
  [ "$G4TBL" = "0" ] || { stamp "  ^ G4 REFUSED: $G4TBL table line(s) move in the CLAUDE.md delta.  A doctrine batch inserts prose; a moved table row is an edit to a measured figure and needs a ruling."; fail_gate G4-table; }
  [ "$G4DEL" = "0" ] || { stamp "  ^ G4 REFUSED: ${G4DEL} line(s) REMOVED -- a batch is a PURE INSERTION"; fail_gate G4-deletions; }
  [ "$G4MARK" = "0" ] || { stamp "  ^ G4 REFUSED: conflict markers survive in CLAUDE.md"; fail_gate G4-markers; }
  [ "$G4BOM_BASE" = "$G4BOM_HEAD" ] || { stamp "  ^ G4 REFUSED: the BOM state changed across the merge ([$G4BOM_BASE] -> [$G4BOM_HEAD]).  Encoding damage lands on the FIRST line, which is where review attention is weakest."; fail_gate G4-bom; }
  if [ "${G4DIFFADD:-0}" -lt "${G4ADD}" ]; then
    stamp "  ^ G4 REFUSED: the diff shows $G4DIFFADD added line(s) where the numstat says ${G4ADD} -- the two readings of one comparison disagree, which means one of them is not reading what it claims"
    fail_gate G4-two-readings
  fi
else
  stamp "G4 CLAUDE.md UNTOUCHED in this train's delta, and OWED_CLAUDEMD=$OWED_CLAUDEMD says that is the EXPECTED state -- a READING, not a refusal."
fi

# --- G6 coarse census over the delta's added lines ---------------------------------------------
#     ⚠ DERIVE-TIME READING, RE-MEASURED FOR THE FINAL SEVEN-SEAT SET: detectorHits=0 AND residual=0
#     over EVERY ONE of the seven seats (1 363dbf285, 2 9b311a651, 3 9d3fa86ae, 4 6ebb567bb,
#     5 541b4fd7b, 6 bcc601d89, 7 b7dc47bc6 -- the probe at its RULED re-pin, re-censused
#     there), measured by running THIS SCRIPT'S OWN census functions --
#     extracted by anchor, not retyped -- over each seat's real diff against its merge base.  There is
#     no longer an unmeasured seat, so a NON-ZERO detectorHits here means a seat MOVED after this derive
#     and is a READING to reconcile rather than a refusal.  Only the RESIDUAL is asserted.
#     ⚠ A DOCS SEAT CARRYING A PROBE TREE IS THE ONE THIS ARM IS FOR: a probe record carrying a go.mod
#     and a main.go is exactly the shape that carries a profile path by accident.  Measured 0/0.
#     ⚠ The WITHDRAWN seat is out of this census with its row; its 0/0 reading was taken at the time
#     and is no longer a statement about this train.  The SHA is named ONCE in the NOT-BOARDING block
#     and ONCE in the run-time withdrawal stamp, and nowhere else in this file, so there is exactly one
#     place to correct if the coordinator re-seats it.
HITS=$(git diff "$BASE" HEAD | census_residual)
RAWH=$(git diff "$BASE" HEAD | census_raw)
G6TPL=$(git diff "$BASE" HEAD | census_template)
stamp "G6 coarse census :: detectorHits=$RAWH (STAMPED, never asserted -- ⚠ NO derive-time reading is carried in this TEMPLATE because no seat was named when it was written, so this number is a READING to reconcile against the seats and never a verdict) templateExcluded=$G6TPL (PRINTED: Sprintf-placeholder and detector-literal lines, the TEMPLATE class ruled 2026-09-08 16:15 after a run read them as residual) residual=$HITS (must be 0)"
[ "${G6TPL:-0}" = "0" ] || { stamp "  ^ G6 template-class lines EXCLUDED (each carries a placeholder SEGMENT or is the detector's own literal -- read them; a real segment here would be a residual, not an exclusion):"; git diff "$BASE" HEAD | census_template_lines | head -20 | cut -c1-200 | sed 's/^/    /' | tee -a "$ASMLOG"; }
if [ "${HITS:-1}" != "0" ]; then
  stamp "  ^ G6 residual lines follow (hard hits first, then the rest -- read before acting):"
  git diff "$BASE" HEAD | census_residual_lines | head -20 | cut -c1-200 | sed 's/^/    /' | tee -a "$ASMLOG"
  stamp "  ^ and the KIND HISTOGRAM.  READ IT BEFORE TOUCHING THE EXCLUSION: a widening is the"
  stamp "    direction that can HIDE a hit.  Take the reading to the coordinator."
  git diff "$BASE" HEAD | census_residual_lines | census_kinds
  fail_gate G6-residual
fi

# --- G10a THE BOARD'S OWN STRUCTURAL GUARD (it runs whether or not a seat touches the file) ----
#     ⚠ THIS IS NOT TRAIN 44's G10a, AND THE DIFFERENCE WAS MEASURED RATHER THAN ASSUMED.  Train 44's
#     seat-2 record was a PURE APPEND and its gate was a byte-identical-prefix test.  This train's two
#     append-shaped docs seats are NOT pure appends: measured on train 47 at ITS derive time, a docs row's BOARD grew
#     24192 -> 24275 lines with the base NOT a byte-identical prefix of the head, and another docs row's RECON
#     amendment grows 1021 -> 1028 the same way.  Both are CORRECT: the board's new sections go BEFORE
#     its closing guard line, and a RECON section-2 amendment is an in-place dated block.  A prefix
#     test carried over would have gone RED on two correct seats -- the stale-gate class -- so the
#     assertion is the one CLAUDE.md actually names for this file: ONE {% raw %}, ONE {% endraw %},
#     and the endraw LAST, because a board seat has already published a section INSIDE a comment by
#     splitting that guard.
G10AF='docs/phase4/BOARD-next-validation-candidates.md'
if [ ! -f "$G10AF" ]; then
  stamp "G10a UNMEASURED :: $G10AF is ABSENT from the assembled tree -- a docs row whose whole deliverable is that file landed nothing"
  FAILED=1
else
  GARAW=$(grep -ac '{% raw %}' "$G10AF" || true)
  GAEND=$(grep -ac '{% endraw %}' "$G10AF" || true)
  GALAST=$(tr -d '\r' < "$G10AF" | grep -n . | tail -1 | cut -d: -f1)
  GATOT=$(lf_lines "$G10AF")
  GAENDLN=$(tr -d '\r' < "$G10AF" | grep -n '{% endraw %}' | tail -1 | cut -d: -f1)
  GACR=$(cr_bytes "$G10AF")
  GADEL=$(git diff --numstat "$BASE" HEAD -- "$G10AF" | cut -f2)
  case "${GADEL}" in ''|*[!0-9]*) GADEL=0 ;; esac
  stamp "G10a BOARD structural :: {% raw %} x$GARAW (must be 1) {% endraw %} x$GAEND (must be 1) endrawOnLine=$GAENDLN lastNonEmptyLine=$GALAST (must be EQUAL -- the endraw is the FINAL content) totalLines=$GATOT CR=$GACR (must equal $GATOT under the eol=crlf pin) deletions=$GADEL (must be 0)"
  { [ "${GARAW:-0}" = "1" ] && [ "${GAEND:-0}" = "1" ] && [ "${GAENDLN:-0}" = "${GALAST:-x}" ] && [ "$GACR" = "$GATOT" ] && [ "$GADEL" = "0" ]; } || { stamp "  ^ G10a REFUSED: the board's own structural guard does not hold.  A split or non-final endraw publishes every later section INSIDE an HTML comment -- invisible, with the commit reading normally -- which is exactly the failure this arm exists for."; FAILED=1; }
fi

# --- G10b THE REVIEW-SIBLING .cs.auto -- shape AND provenance ----------------------------------
#     ⚠ NO GATE IN THIS BATTERY COMPILES A .cs.auto.  They are the converter's REVIEW SIBLINGS beside
#     hand-owned files; nothing builds them and nothing runs them.  What CAN be asserted is their
#     SHAPE (the path ends .cs.auto) and their PROVENANCE (a sibling .cs carrying a LINE-ANCHORED
#     [module: GoManualConversion] marker exists, because a .cs.auto exists only where a marked file
#     sits beside it -- so an orphan is either a wrong path or a retired hand-own).
G10BN=0; G10BORPH=0; G10BSHAPE=0
while IFS= read -r f <&3; do
  [ -n "$f" ] || continue
  G10BN=$(( G10BN + 1 ))
  case "$f" in
    *.cs.auto) : ;;
    *) G10BSHAPE=$(( G10BSHAPE + 1 )); stamp "      NOT a .cs.auto in the delta's auto set: $f" ; continue ;;
  esac
  sib="${f%.auto}"
  if [ ! -f "$sib" ]; then
    G10BORPH=$(( G10BORPH + 1 )); stamp "      ORPHAN: $f has NO sibling $sib in the assembled tree"
    continue
  fi
  mk=$(grep -acE '^[[:space:]]*\[module:[[:space:]]*(go\.)?GoManualConversion\]' "$sib" || true)
  if [ "${mk:-0}" -lt 1 ]; then
    G10BORPH=$(( G10BORPH + 1 )); stamp "      ORPHAN: $f -- its sibling $sib carries NO line-anchored GoManualConversion marker, so it is not a hand-own and has no business having a review sibling"
  fi
done 3<<< "$(git -c core.quotepath=false diff --name-only "$BASE" HEAD -- src/core | grep '\.cs\.auto$' || true)"
stamp "G10b .cs.auto metadata :: .cs.auto files in this train's delta=$G10BN shapeViolations=$G10BSHAPE (must be 0) orphans=$G10BORPH (must be 0)"
# ⚠ **THE COUNT IS A READING IN THIS TRAIN, NOT AN ASSERTION, AND THE CHANGE IS DELIBERATE.**  Train
#   45's arm REFUSED on `G10BN >= 1`, because that train's seat 2 carried exactly one review sibling
#   (src/core/runtime/runtime2.cs.auto) as the corpus half of its fix -- so a zero there meant the
#   converter change landed and its footprint did not.  NO SEAT IN TRAIN 46 CARRIES A `.cs.auto`, so
#   the same assertion here would be a FALSE RED on a correct train: an arm whose subject does not
#   exist must not refuse, and an arm kept only because it was in the previous script is the
#   stale-figure class wearing a gate's clothes.
#   ⚠ WHAT IS STILL ASSERTED IS THE PART THAT CAN GO RED: any `.cs.auto` that DOES appear must have
#   the right SHAPE and a real hand-owned sibling.  A zero population makes those two vacuously true
#   and it SAYS so rather than reporting a pass.
[ "${G10BSHAPE:-1}" = "0" ] || { stamp "  ^ G10b REFUSED: a path in the delta's auto set is not a .cs.auto"; FAILED=1; }
[ "${G10BORPH:-1}" = "0" ] || { stamp "  ^ G10b REFUSED: an orphaned review sibling -- either the path is wrong or the hand-own it belongs to has been retired, and a .cs.auto exists only where a marked file sits beside it"; FAILED=1; }
[ "${G10BN:-0}" -ge 1 ] || stamp "G10b :: ZERO .cs.auto in this train's delta, which is the EXPECTED reading -- no seat here carries a review sibling.  The shape and orphan arms above are therefore VACUOUSLY true and are reported as such rather than as a pass; ⚠ NOTHING IN THIS BATTERY COMPILES A .cs.auto in any case, which is stated rather than glossed."

# --- G10c THE EIGHT ARTIFACT GOLDENS ARE UNTOUCHED ---------------------------------------------
#     ⚠ ITS CONSUMER LIST CHANGED WITH THIS DERIVE AND IS RESTATED RATHER THAN LEFT STALE.  On train
#     45 this arm existed for THREE consumers: LEG 4's named Δ-drop diagnosis, LEG 4b's pre-fix
#     reading, and LEG 5's Δ assertion.  LEG 4b is RETIRED (it was an earlier train's own acceptance
#     and that seat LANDED), and LEG 4's NAMED diagnosis is retired with it -- the alias fold makes the
#     emission pin-INDEPENDENT, so the Δ-drop signature can no longer be produced by a failed pairing
#     and a branch that cannot be taken is the warm-design trap.  What REMAINS is real and is why this
#     arm stays: LEG 0 and LEG 5 both read the Δ OUT OF THE EMISSION, and a seat that moved one of
#     these goldens would invalidate those readings before either is taken.
#     ⚠ IT IS A SECOND DERIVATION OF A7, NOT A DUPLICATE OF IT.  A7 reads the DELTA (git's answer about
#     what changed); this reads the FILES (content against the committed blob).  Two derivations of one
#     property, and the one that disagrees is where to look.
G10CDRIFT=0
while IFS= read -r p <&3; do
  [ -n "$p" ] || continue
  t="src/tests/Behavioral/$p/main.cs.target"
  if [ ! -f "$t" ]; then
    stamp "      ⚠ $t is ABSENT from the assembled tree -- one of the eight has no golden at all"
    G10CDRIFT=$(( G10CDRIFT + 1 )); continue
  fi
  if git show "$BASE:$t" 2>/dev/null | tr -d '\r' | diff -q - <(tr -d '\r' < "$t") >/dev/null 2>&1; then
    :
  else
    stamp "      ⚠ $t DIFFERS from its committed blob at $BASE -- a seat moved a golden this train's Δ readings rest on"
    G10CDRIFT=$(( G10CDRIFT + 1 ))
  fi
done 3<<< "$GOLDEN_PROJECTS"
stamp "G10c the eight artifact goldens :: differing from their blob at $BASE = $G10CDRIFT (must be 0 -- LEG 0's and LEG 5's Δ readings rest on these being exactly as committed; LEG 4b and LEG 4's named diagnosis are RETIRED and are no longer among this arm's consumers)"
[ "${G10CDRIFT:-1}" = "0" ] || { stamp "  ^ G10c REFUSED: $G10CDRIFT of the eight is not the blob committed at $BASE."; FAILED=1; }

# --- G10d THE DOCS DELTA'S LINE ENDINGS -- **AND THE `-text` ATTRIBUTE IS NOW CONSULTED** ---------
#     ⚠ THIS IS THE FAULT TRAIN 45's LAND SCRIPT HAD TO ACCEPT, FIXED HERE IN THE INSTRUMENT.
#     Train 45's G10d asserted that every `docs/*.md` in the delta is CRLF in the working tree "under
#     the eol=crlf pin".  That premise is FALSE for docs/: the `.gitattributes` `text eol=crlf` block
#     covers the converter-emitted artifact types and `src/core/**/README.md`, NOT docs at large -- and
#     one seated file carried the `-text` attribute, the verbatim-bytes exemption this repository
#     already grants to `testdata` and to four behavioral projects.  A `-text` file's working tree is
#     whatever its blob is, BY DESIGN, so G10d refused a correct seat and the landing grew a named
#     acceptance path for it.  An acceptance path that outlives its fault is a lie-lever; the fix
#     belongs in the instrument, so the instrument now ASKS GIT what the attribute is.
#     THE PREDICATE, and both halves matter:
#       * a path whose `git check-attr text` answers `unset` -- the `-text` mark -- is EXEMPT, but ONLY
#         when its INDEX and WORKTREE layers AGREE (`i/lf w/lf` or `i/crlf w/crlf`) in
#         `git ls-files --eol`.  A `-text` file whose layers DISAGREE is not the verbatim-bytes case at
#         all; it is a file that has been smudged despite the exemption, and it still refuses.
#       * every OTHER docs file must be CRLF in the working tree.  A non-exempt LF docs file still
#         REFUSES, which is the half that keeps this arm a gate rather than a formality.
#     ⚠ THE EXEMPTION COUNT IS STAMPED BESIDE THE MISMATCH COUNT.  An exclusion that is not printed is
#     an exclusion that grows quietly, and a reader needs to see how many files left the population
#     before reading a zero as a clean sweep.
#     ⚠ POSITIVE CONTROL, and it is the evidence this predicate is the right one: train 45's own
#     standalone G10D measurement read **9 of 9 docs files accepted with exactly 1 text-exempt**, and a
#     PLANTED non-exempt LF docs file was REFUSED by the same code.  Both directions, on the real tree.
#     A future derive that widens this arm re-runs BOTH arms of that control before believing it.
G10DBAD=0
G10D_RECORDS=""
if [ "${A3IN_DELTA:-0}" != "0" ]; then G10D_RECORDS="docs/phase4/DESIGN-fatal-path.md"; fi
for nf in $G10D_RECORDS; do
  atbase=NO; git cat-file -e "$BASE:$nf" 2>/dev/null && atbase=YES
  athead=NO; [ -f "$nf" ] && athead=YES
  stamp "G10d NEW record :: $nf existsAtBase=$atbase (must be NO) existsAtHead=$athead (must be YES)"
  { [ "$atbase" = "NO" ] && [ "$athead" = "YES" ]; } || G10DBAD=$(( G10DBAD + 1 ))
done
[ -n "$G10D_RECORDS" ] || stamp "G10d NEW record :: no named record is owed by a MERGED seat in this run (seat 3 skipped or absent), so this arm asserts nothing and SAYS so rather than passing over a hole"
G10DCR=0; G10DCRN=0; G10DEX=0; G10DEXNAMES=""
while IFS= read -r f <&3; do
  [ -n "$f" ] || continue
  [ -f "$f" ] || continue
  G10DCRN=$(( G10DCRN + 1 ))
  # ⚠ ASK GIT, do not infer.  `check-attr` answers for a path whether or not it is tracked, and its
  #   third field is the RESOLVED value: `set`, `unset` (the `-text` mark) or `unspecified`.
  #   `ls-files --eol` is the second reading and carries the two LAYERS, which is what makes the
  #   exemption CONDITIONAL rather than blanket.
  attrv=$(git check-attr text -- "$f" 2>/dev/null | sed 's/.*: //' | tr -d '\r')
  eolrow=$(git ls-files --eol -- "$f" 2>/dev/null | tr -s ' ' | tr -d '\r')
  ilayer=$(printf '%s' "$eolrow" | grep -oE 'i/[a-z-]+' | head -1)
  wlayer=$(printf '%s' "$eolrow" | grep -oE 'w/[a-z-]+' | head -1)
  c=$(cr_bytes "$f"); l=$(lf_lines "$f")
  if [ "$attrv" = "unset" ]; then
    if [ -n "$ilayer" ] && [ "${ilayer#i/}" = "${wlayer#w/}" ]; then
      G10DEX=$(( G10DEX + 1 )); G10DEXNAMES="$G10DEXNAMES $f"
      stamp "      TEXT-EXEMPT (attr -text, layers AGREE $ilayer $wlayer): $f CR=$c LF=$l -- verbatim bytes are this file's contract, so a CRLF assertion over it would be the instrument over-asserting"
      continue
    fi
    G10DCR=$(( G10DCR + 1 ))
    stamp "      CR/LF MISMATCH (attr -text but the LAYERS DISAGREE $ilayer $wlayer): $f CR=$c LF=$l -- a -text file is supposed to be byte-identical between index and worktree, so a disagreement is NOT the verbatim-bytes case and is not exempt"
    continue
  fi
  [ "$c" = "$l" ] || { G10DCR=$(( G10DCR + 1 )); stamp "      CR/LF MISMATCH (attr=[${attrv:-unreadable}] $ilayer $wlayer): $f CR=$c LF=$l"; }
done 3<<< "$(git diff --name-only "$BASE" HEAD -- docs | grep -E '\.md$' || true)"
stamp "G10d docs CR/LF :: docs/*.md files in the delta checked=$G10DCRN mismatches=$G10DCR (must be 0) textExempt=$G10DEX [$G10DEXNAMES] (STAMPED beside the mismatch count, because an exclusion that is not printed is an exclusion that grows quietly) :: new-record shape violations=$G10DBAD (must be 0)"
[ "${G10DBAD:-1}" = "0" ] || { stamp "  ^ G10d REFUSED: a record this train introduces as a NEW file is not new (or did not arrive)"; FAILED=1; }
[ "${G10DCR:-1}" = "0" ] || { stamp "  ^ G10d REFUSED: a docs file that is NOT \`-text\`-exempt has a non-CRLF working-tree form, or a \`-text\` file's two layers disagree.  The exemption is by ATTRIBUTE and is conditional on the layers agreeing; it is not a blanket pass for docs/."; FAILED=1; }

# --- G12 THE GOLIB BOX-KIND CENSUS -------------------------------------------------------------
#     ⚠ OWED BY ANY SEAT THAT EDITS ж.cs -- the BASE of every box kind.  Seat 2 edits src/core/golib
#     (slice.cs) and seat 3 adds src/core/golib/runtime/FatalReport.cs.  If a seat ever adds an
#     ABSTRACT member there, every kind must answer it or the assembly is CS0534 at LEG 2/2b; this
#     leg names that in seconds instead.  Measured at derive time on the assembly worktree at master:
#     SEVEN direct kinds (ElemRefBox, FieldRefBox, HeaderSliceBox, NativeArrayBox, NativeBox,
#     SliceHeaderBox, StandardBox), every one carrying a StorageKind answer; no seat in THIS train
#     adds a box kind or an abstract member, so the floor is unchanged at 7.  A stale
#     helper class and NO new kind and NO new abstract member, so the floor is unchanged at 7.
G12DIR='src/core/golib'
if [ ! -d "$G12DIR" ]; then
  stamp "G12 UNMEASURED :: $G12DIR is absent"; FAILED=1
else
  G12KINDS=0; G12MISS=0; G12NAMES=""
  while IFS= read -r bf <&3; do
    [ -n "$bf" ] || continue
    [ -f "$bf" ] || continue
    direct=$(tr -d '\r' < "$bf" | grep -acE 'class[[:space:]]+[A-Za-z0-9_]+<[A-Za-z0-9_, ]*>[[:space:]]*:[[:space:]]*ж<' || true)
    [ "${direct:-0}" -ge 1 ] || continue
    G12KINDS=$(( G12KINDS + direct ))
    G12NAMES="$G12NAMES $(basename "$bf")"
    sk=$(tr -d '\r' < "$bf" | grep -ac 'StorageKind' || true)
    if [ "${sk:-0}" -lt 1 ]; then
      G12MISS=$(( G12MISS + 1 ))
      stamp "      box kind file with NO StorageKind answer: $bf"
    fi
  done 3<<< "$(git -c core.quotepath=false ls-files "$G12DIR" | grep '\.cs$' || true)"
  stamp "G12 golib box-kind census :: DIRECT box kinds found=$G12KINDS (7 at the derive-time master, re-measured on the assembly worktree rather than carried from train 44) withNoStorageKindAnswer=$G12MISS (must be 0) :: files [$G12NAMES ]"
  if [ "${G12KINDS:-0}" -lt 7 ]; then
    stamp "  ^ G12 REFUSED: only $G12KINDS direct box kind(s) enumerated where the derive measured 7.  Either a box kind was REMOVED (a finding), or the enumeration is broken -- most likely core.quotepath or the glyph in the filenames.  An assertion over an almost-empty set is the vacuity this catches."
    FAILED=1
  fi
  [ "${G12MISS:-1}" = "0" ] || { stamp "  ^ G12 REFUSED: a direct box kind states no StorageKind.  With the base member abstract that is CS0534 at LEG 2/2b; this leg names it in seconds instead."; FAILED=1; }
fi
pin_go 1.24.13 "POST-MERGE" || { stamp "the post-merge toolchain pin could not be established; every Go leg below would be UNMEASURED -- ABORT"; exit 90; }

# ============================================================================================
# LEG C -- THE FULL CONVERTER SUITE, AND IT IS A **REAL GATE** IN THIS TRAIN.
#   ⚠ THE INVERSION FROM TRAIN 44, STATED.  There the converter was UNTOUCHED and the suite was a
#   no-regression leg run because CNR rebuilds the binary LEG K reuses.  Here FIVE seats change
#   src/go2cs -- seat 1 (the alias-shadow qualify), seat 2 (the type-parameter nil test, converter half),
#   seat 5 (the ref-lowered defer/go box render) and seat 3 (the manualTypeOperations registry, +27),
#   plus the `fleetIdentifierCensus_test.go` split-token arm a converter-test row carries -- so this is the gate that measures
#   them, and G11(b) below REFUSES if converter files are present and this leg is not wired.
#   ⚠ THE PIN IS ASSERTED **HERE**, AT THIS LEG'S OWN cwd, WITH `go env GOVERSION`.  pin_go already
#   asserted `go version` and `go env GOROOT` from the worktree root; inside src/go2cs a go.mod
#   declaring `go 1.24.13` sits, and the question this leg needs answered is what the TOOLCHAIN
#   RESOLVES TO THERE.  Under GOTOOLCHAIN=local that is the pinned release or the build refuses; the
#   reading is taken rather than assumed, because a pin that is printed and not compared is a
#   decoration and this repository has paid for one.
# ============================================================================================
LEGCGV=$( cd src/go2cs && go env GOVERSION 2>&1 | tr -d '\r' )
stamp "LEG C toolchain, read AT THE SUITE'S OWN cwd :: (cd src/go2cs && go env GOVERSION) = [$LEGCGV] (must be exactly go1.24.13; the PAIRING's no-go.mod assertion answers a different question and neither substitutes for the other)"
if [ "$LEGCGV" != "go1.24.13" ]; then
  stamp "  ^ LEG C UNMEASURED: the converter module resolves to [$LEGCGV] where master's go.mod requires go1.24.13.  Every reading below would describe a toolchain nobody ruled on -- ABORT"
  exit 90
fi

LEGCLOG="/tmp/${TAG}-legc-converter.log"; : > "$LEGCLOG"
LEGC_T0=$(date +%s)
( cd src/go2cs && go test -count=1 -timeout 30m ./... > "$LEGCLOG" 2>&1 < /dev/null ); legcrc=$?
LEGC_WALL=$(( $(date +%s) - LEGC_T0 ))
stamp "LEG C FULL converter suite exit=$legcrc wall=${LEGC_WALL}s (200-330 s solo is this box's own range; a FAIL at ~600 s is go test's DEFAULT 10-minute wall and not the code, which is why -timeout 30m is passed) :: pkgsOK=$(grep -ac '^ok' "$LEGCLOG" || true) FAILlines=$(grep -ac '^FAIL' "$LEGCLOG" || true) :: $(grep -aE '^(ok|FAIL|--- FAIL)' "$LEGCLOG" | tail -2 | tr '\n' ' ' | cut -c1-160)"
if [ "$legcrc" != "0" ]; then
  fail_gate LEG-C
  stamp "  ^ LEG C failing test names:"
  grep -aE '^(--- FAIL|FAIL)' "$LEGCLOG" | head -20 | sed 's/^/    /' | tee -a "$ASMLOG"
fi

# ============================================================================================
# LEG Cg -- THE SEATS' OWN NAMED GUARDS, VERBOSE, WITH A DERIVED FLOOR.
#   ⚠ IT EXISTS BECAUSE A NON-VERBOSE `go test ./...` PRINTS PACKAGE LINES AND **NO TEST NAMES**, so
#   a guard count read off LEG C would be ZERO BY CONSTRUCTION -- the instrument, not the guards.
#   The `=== RUN` lines ARE this leg's positive control, and the FLOOR is DERIVED from the assembled
#   tree rather than carried: a filter that matches nothing prints a clean `ok` and exits 0.
#   ⚠ TRAIN 44 SPLIT THIS INTO G5 (the fleet identifier census) AND G5b (the registry ledgers) WITH
#   TWO FILTERS.  They are ONE leg here because four seats move the population and two filters over
#   one suite is two floors to keep in step; the names each seat contributes are stamped below, so a
#   floor that moves is still attributable to a seat.
#   Derive-time PREDICTION, to be SCORED against the run (RE-DERIVED by the coordinator at seat fill,
#   2026-09-08 15:40, from `git diff 44f858717...<seat> -- 'src/go2cs/*_test.go'` over all six seats):
#   master 44f858717's own reading under THIS filter is 23 matching top-level Test funcs (the six
#   train-46 terms NamespaceShadow|AliasNamespace|ImportSpec|TypeParamNil|SliceNil|ManualType match
#   NOTHING at master -- 23 under the train-45 filter too -- they are guesses at guard names and are
#   stated as dead); NO train-46 seat ADDS a `func Test` in src/go2cs/*_test.go except a converter-test row's own,
#   TestSplitRefusalIsAttributableToTheToken, which this filter does NOT match (it runs in the full
#   converter suite, LEG C).  So the union floor is predicted 23 = master's 23 + 0.  The floor below
#   is computed from the tree, so a disagreement with 23 is itself the reading and not a refusal.
# ============================================================================================
LEGCG_FILTER='ImportAliasRename|RootShadow|StripLocalTypeQualifier|SiblingClosureContributes|SiblingTestDeclarators|ValueCloneStamp|FleetIdentifier|Clearance|Denied|ConverterStalenessConsultsTheToolchain|EmbedDirectivesStayWithin|HarnessRebuildPredicates|LinknamePushRegistry|ManualConversionRegistrations|Projitems|RunActionHostArgv|NamespaceShadow|AliasNamespace|ImportSpec|TypeParamNil|SliceNil|ManualType'
LEGCG_NAMES=$(cat src/go2cs/*_test.go 2>/dev/null | tr -d '\r' | grep -aoE '^func Test[A-Za-z0-9_]+' | sed 's/^func //' | grep -aE "$LEGCG_FILTER" | sort | tr '\n' ' ')
LEGCG_FLOOR=$(printf '%s\n' "$LEGCG_NAMES" | tr ' ' '\n' | grep -ac . || true)
case "${LEGCG_FLOOR}" in ''|*[!0-9]*) LEGCG_FLOOR=0 ;; esac
LEGCGLOG="/tmp/${TAG}-legcg-guards.log"; : > "$LEGCGLOG"
( cd src/go2cs && go test -count=1 -timeout 30m -v -run "$LEGCG_FILTER" ./... > "$LEGCGLOG" 2>&1 < /dev/null ); legcgrc=$?
LEGCG_RUNS=$(grep -ac '^=== RUN' "$LEGCGLOG" || true)
stamp "LEG Cg named guards exit=$legcgrc runLines=$LEGCG_RUNS derivedFloor=$LEGCG_FLOOR :: matched names from the assembled tree [$LEGCG_NAMES]"
# ⚠ **NO PREDICTED FLOOR LITERAL IN THIS TRAIN, AND THE ABSENCE IS DELIBERATE.**  The template carried
#   `predicted 23 at the union: master 44f858717's own 23 ... no train-46 seat adds a matching name`,
#   which is a statement about a DIFFERENT base and a DIFFERENT seat set: this train's base is not
#   44f858717 and THREE of its rows add new `_test.go` guards under this same filter, so that literal
#   could only ever print a MISS on a healthy run and train the reader to ignore the line.  The floor
#   is DERIVED from the assembled tree's own test files above; what is asserted is the RELATION the
#   derivation makes checkable -- run lines must not fall below it -- and the number itself is a
#   reading to reconcile against the rows' test files, never an expectation to adjust after the fact.
[ "${LEGCG_FLOOR:-0}" -ge 1 ] || stamp "      ⚠ LEG Cg derived floor is ZERO -- the filter matched no Test function in the assembled tree at all, which is an instrument reading and not a statement about the guards."
[ "$legcgrc" = "0" ] || { FAILED=1; stamp "  ^ LEG Cg failing tests:"; grep -aE '^(--- FAIL|FAIL)' "$LEGCGLOG" | head -14 | sed 's/^/    /' | tee -a "$ASMLOG"; }
if [ "${LEGCG_RUNS:-0}" = "0" ]; then
  stamp "  ^ LEG Cg UNMEASURED: the -run filter matched NO test -- a guard subset that did not run is not a pass, and it is exactly what a filter typo produces"
  FAILED=1
elif [ "$LEGCG_FLOOR" -gt 0 ] && [ "${LEGCG_RUNS:-0}" -lt "$LEGCG_FLOOR" ]; then
  stamp "  ^ LEG Cg SHORT: $LEGCG_RUNS run lines against a derived floor of $LEGCG_FLOOR -- either an arm did not run, or the DERIVATION over-counted.  Both numbers are printed above so a red is diagnosable without a re-run."
  FAILED=1
fi
# --- the FOUR guards this train's seats are ABOUT, named individually.  A subset count can be met
#     while the one guard a seat exists for did not run, so each is read from the verbose log by name.
for g in TestImportAliasRenameReadsBothClosures TestImportAliasRenameIsTwoSided TestValueCloneStampSpellsFieldsAsDeclared TestFleetIdentifierNicknameHostsAreAdmitted TestConverterStalenessConsultsTheToolchain TestLinknamePushRegistryMatchesGoSource; do
  nr=$(grep -ac "^=== RUN[[:space:]]*$g\$" "$LEGCGLOG" || true)
  np=$(grep -ac "^--- PASS: $g " "$LEGCGLOG" || true)
  stamp "LEG Cg seat guard :: $g RUN=$nr PASS=$np"
  if [ "${nr:-0}" = "0" ]; then
    stamp "      ^ that guard did NOT RUN.  ⚠ For the four this train's seats ADD, a zero means the seat's own test file is not in the assembled tree; for the two it merely EXERCISES, a zero means the filter no longer reaches it.  Either way it is not a pass."
    FAILED=1
  fi
done

# --- G7b THE PIN IS LOAD-BEARING, proved on the REAL tree --------------------------------------
#     The assembled converter module MUST refuse under the OLD pin.  Its RED is the pass; a green
#     means GOTOOLCHAIN is switching under the pin, in which case every 1.24.13 reading in this run is
#     a decoration AND LEG K's 1.23.12 pin is not a pin either.
G7BLOG="/tmp/${TAG}-g7b-oldpin.log"; : > "$G7BLOG"
if pin_go 1.23.12 "G7b-oldpin"; then
  ( cd src/go2cs && go list -m > "$G7BLOG" 2>&1 < /dev/null ); g7brc=$?
  stamp "G7b old-pin refusal :: the assembled converter module under the go1.23.12 pin exits $g7brc (want NON-ZERO) :: $(tr -d '\r' < "$G7BLOG" | grep -aiE 'requires|toolchain' | head -1 | cut -c1-160)"
  [ "$g7brc" = "0" ] && { stamp "  ^ G7b REFUSED: the OLD toolchain accepted the assembled module.  Either the go directive is not what A5 read, or GOTOOLCHAIN is switching under the pin."; FAILED=1; }
else
  stamp "G7b UNMEASURED :: the 1.23.12 pin could not be established for the control -- and LEG K needs that same root, so this is a refusal rather than a note"
  FAILED=1
fi
pin_go 1.24.13 "POST-G7b" || { stamp "could not restore the 1.24.13 pin after the G7b control -- every leg below would be UNMEASURED -- ABORT"; exit 90; }
# ============================================================================================
# G11 -- THE JUSTIFICATIONS, CHECKED RATHER THAN CLAIMED.  ⚠ BOTH ARMS POINT THE OPPOSITE WAY FROM
# TRAIN 44's: there the converter was untouched and G11(b) ASSERTED that zero to justify not wiring
# the converter families; here FOUR seats change src/go2cs and G11(b) is what makes the converter
# obligations explicit and REFUSES if the gates they name are not wired.
# ============================================================================================
# --- G11(a) ⚠ **THE JUSTIFICATION IS DERIVED FROM THE OWED VECTOR, NEVER FROM A DIRECTORY LITERAL.**
#     Train 46 run 6 read `G11(a) JUSTIFICATION FALSE` on a healthy tree because the arm required
#     `src/core/syscall/windows` in the delta -- train 45's premise, and a fact about a train that
#     happened to have a syscall seat.  The vector above says what THIS train's merged CLASSES owe;
#     every arm below reads it, and every directory count is printed as a READING beside it.
G11ADIRS='src/core src/core/golib src/core/syscall/windows src/tests src/tests/GolibTests src/go2cs src/gen src/utilities docs CLAUDE.md'
# ⚠ THE TOOLING POPULATION IS A PATHSPEC SET, NOT A DIRECTORY, because a repo tool script lives at the
#   src/ ROOT (`src/*.sh`, `src/*.ps1`) as well as under src/utilities -- and `src/utilities` alone
#   would read ZERO on a row whose whole deliverable is a root-level script.
G11TOOLSPEC='src/*.sh src/*.ps1 src/utilities'
G11ACOUNTS=''
for d in $G11ADIRS; do
  n=$(git -c core.quotepath=false diff --name-only "$BASE" HEAD -- "$d" | grep -ac . || true)
  case "$n" in ''|*[!0-9]*) n=0 ;; esac
  G11ACOUNTS="$G11ACOUNTS $d=$n"
done
G11GOLIB=$(git -c core.quotepath=false diff --name-only "$BASE" HEAD -- src/core/golib | grep -ac . || true)
G11GT=$(git -c core.quotepath=false diff --name-only "$BASE" HEAD -- src/tests/GolibTests | grep -ac . || true)
G11SYS=$(git -c core.quotepath=false diff --name-only "$BASE" HEAD -- src/core/syscall/windows | grep -ac . || true)
G11CONV=$(git -c core.quotepath=false diff --name-only "$BASE" HEAD -- src/go2cs src/gen | grep -ac . || true)
G11GEN=$(git -c core.quotepath=false diff --name-only "$BASE" HEAD -- src/gen | grep -ac . || true)
G11BEH=$(git -c core.quotepath=false diff --name-only "$BASE" HEAD -- src/tests/Behavioral | grep -ac . || true)
G11CORE=$(git -c core.quotepath=false diff --name-only "$BASE" HEAD -- src/core | grep -ac . || true)
# shellcheck disable=SC2086
G11TOOL=$(git -c core.quotepath=false diff --name-only "$BASE" HEAD -- $G11TOOLSPEC | grep -ac . || true)
case "${G11TOOL}" in ''|*[!0-9]*) G11TOOL=0 ;; esac
for v in G11GOLIB G11GT G11SYS G11CONV G11GEN G11BEH G11CORE; do eval "case \"\${$v}\" in ''|*[!0-9]*) $v=0 ;; esac"; done
stamp "G11(a) per-directory counts ::$G11ACOUNTS -- printed SEPARATELY because one combined number cannot say WHICH directory it describes, and NEVER summed, because src/core/golib and src/core/syscall/windows are SUBSETS of src/core and src/tests/GolibTests is a subset of src/tests"
# ⚠ EACH ARM IS (owed, measured).  An arm whose OWED is 0 is a READING and can never refuse; an arm
#   whose OWED is 1 and whose measurement is 0 means a seat did not land what its CLASS says it is,
#   and the gate families named beside it would then be measuring a tree with nothing new in it.
G11ABAD=0
g11a_arm(){ # $1 owed  $2 measured  $3 directory  $4 what the gates are
  if [ "$1" = "1" ]; then
    if [ "${2:-0}" -ge 1 ]; then stamp "G11(a) OWED and PRESENT :: $3=$2 -- $4"
    else stamp "  ^ G11(a) JUSTIFICATION FALSE :: $3=$2 where the OWED vector says a merged class owes it -- $4"; G11ABAD=1; fi
  else
    stamp "G11(a) not owed :: $3=$2 (a READING; no merged class owes this directory, and a non-zero here is a seat doing work outside its class rather than a fault)"
  fi
}
g11a_arm "$OWED_GOLIB"    "$G11GOLIB" 'src/core/golib'      'the golib gate families are wired: go2cs-stdlib.slnx (LEG 2), go2cs.slnx (LEG 2b -- CLAUDE.md names it BY NAME for a golib/runtime API change), GolibTests at BOTH configurations (LEG 3), and the FULL behavioral suite (LEG 5, route #7 behavioral twin -- the only leg that sees a golib change emitting byte-identical .cs)'
g11a_arm "$OWED_GT"       "$G11GT"    'src/tests/GolibTests' 'LEG 3 runs those arms at BOTH configurations; three liveness tests RUN at Release and SELF-SKIP at Debug, so a Debug-only reading under-measures by three'
g11a_arm "$OWED_CONV"     "$G11CONV"  'src/go2cs + src/gen'  'LEG C (the FULL converter suite at 1.24.13), LEG Cg (the named guards), LEG R (the -tests convert-then-build) and LEG D (the two-seeded three-target emission diff)'
g11a_arm "$OWED_GEN"      "$G11GEN"   'src/gen'              'ROUTE #7: a generator change is invisible to CNR (transpile-only) and to the stdlib solution (one assembly at a time), so LEG 2b and LEG 5 COMPILE are its named gates'
g11a_arm "$OWED_BEH"      "$G11BEH"   'src/tests/Behavioral' 'LEG 5 compiles and runs the new guard projects, including any NESTED sub-libraries, which route #3 says are enumerated by no other gate'
g11a_arm "$OWED_CORPUS"   "$G11CORE"  'src/core'             'LEG 2 compiles the corpus and LEG D measures its emission'
g11a_arm "$OWED_TOOLING"  "$G11TOOL"  'src/*.sh + src/*.ps1 + src/utilities' 'G2 parses every CHANGED .ps1 under BOTH PowerShell editions -- the only gate on this train that reads a harness script at all, which is why a tooling class owes it BY NAME rather than by being swept up in some other leg'
g11a_arm "$OWED_CLAUDEMD" "$(git diff --name-only "$BASE" HEAD -- CLAUDE.md | grep -ac . || true)" 'CLAUDE.md' 'G4 runs with teeth: a batch is a PURE INSERTION, so zero deletions, zero table lines and the BOM state preserved'
stamp "G11(a) syscall/windows=$G11SYS -- a READING ONLY, and named here because a previous train REQUIRED it and refused a healthy tree for it.  A train with no syscall seat legitimately reads 0."
if [ "$G11ABAD" = "0" ]; then
  stamp "G11(a) justification CHECKED :: every directory the OWED vector names is PRESENT in this train's delta (golib=$G11GOLIB GolibTests=$G11GT converter=$G11CONV gen=$G11GEN behavioral=$G11BEH core=$G11CORE; syscall/windows=$G11SYS a reading) -- the golib gate families are owed AND wired.  Stamped in the same shape as G11(b)'s because the land reads it by that name (a req the assembly never satisfied before run 8)."
else
  fail_gate G11a
fi

# --- G11(b) THE CONVERTER OBLIGATIONS, ENUMERATED AND **ALL THREE WIRED** -----------------------
#     ⚠ THIS ARM NAMES NO GAP IN THIS TRAIN, AND THAT IS A CHANGE FROM EVERY DERIVE BEFORE 45.  The
#     doctrine names three converter obligations and this battery's answer to each is stated
#     separately, because a justification that says a leg is owed in a script that does not run it is
#     worse than no justification at all:
#       (i)   THE CONVERTER SUITE -- WIRED, as LEG C plus LEG Cg's named-guard subset.
#       (ii)  THE `-tests` CONVERT-THEN-BUILD OF `reflect` AND `errors` -- WIRED, as LEG R.  Its
#             trigger clause is "lift identity, dedup registries or anonymous-type naming", and the
#             FILE-level owner set is censused below.  ⚠ IT IS REACHED BY A SECOND ROUTE IN THIS TRAIN
#             AND THAT ROUTE IS THE STRONGER ONE WHEREVER A ROW TAKES IT: a change to
#             `src/go2cs/manualTypeOperations.go`, a MANUAL-CONVERSION REGISTRY, whose registration displaces a bodied function
#             re-emits that package's TEST side as well -- the class NO standing gate compiles, and
#             the one that took a banked row's test assembly down at master for two trains.
#       (iii) THE TWO-SEEDED `-stdlib` EMISSION DIFF BY HUNK -- WIRED, as LEG D, CORRECTED to the
#             standalone's shape (see that leg's header for the five corrections).
G11TRIGGER='src/go2cs/typeAccessibilityOperations.go src/go2cs/visitInterfaceType.go src/go2cs/visitStructType.go src/go2cs/dynamicTypeOperations.go src/go2cs/adapterNameCollisions.go src/go2cs/interfaceConversion.go src/go2cs/deferredMarkerOperations.go src/go2cs/manualTypeOperations.go'
# shellcheck disable=SC2086
G11TRIGHIT=$(git -c core.quotepath=false diff --name-only "$BASE" HEAD -- $G11TRIGGER | grep -ac . || true)
case "${G11TRIGHIT}" in ''|*[!0-9]*) G11TRIGHIT=0 ;; esac
G11GEN=$(git -c core.quotepath=false diff --name-only "$BASE" HEAD -- src/gen | grep -ac . || true)
case "${G11GEN}" in ''|*[!0-9]*) G11GEN=0 ;; esac
if [ "$G11CONV" -ge 1 ]; then
  stamp "G11(b) justification CHECKED :: $G11CONV file(s) under src/go2cs or src/gen are in this train's delta, so the CONVERTER obligations are OWED.  Files:"
  git -c core.quotepath=false diff --name-only "$BASE" HEAD -- src/go2cs src/gen | sed 's/^/      /' | tee -a "$ASMLOG"
  stamp "G11(b) lift/dedup/anonymous-naming/REGISTRY trigger census :: $G11TRIGHIT of the named owner set are in the delta (owner set: $G11TRIGGER).  ⚠ manualTypeOperations.go IS IN THAT SET IN THIS TRAIN, added because a manual-conversion registration DISPLACES a bodied function -- the displacement's blast radius on TEST emission is exactly what LEG R compiles and what no standing gate does.  The clause is REACHED at $G11TRIGHIT >= 1."
  [ "${G11TRIGHIT:-0}" -ge 1 ] && git -c core.quotepath=false diff --name-only "$BASE" HEAD -- $G11TRIGGER | sed 's/^/      trigger: /' | tee -a "$ASMLOG"
  stamp "G11(b) ALL THREE CONVERTER OBLIGATIONS ARE WIRED IN THIS BATTERY :: LEG C (the FULL converter suite at 1.24.13 plus LEG Cg's named guards), LEG R (the -tests convert-then-build of reflect AND errors at the MERGE RESULT), and LEG D (the two-seeded three-target -stdlib emission diff, CORRECTED to the standalone's shape).  ⚠ THE PER-SEAT MEASUREMENTS DO NOT SUBSTITUTE FOR LEG D: a per-SEAT footprint is not a UNION footprint, and one rule meeting source ANOTHER seat added is a UNION-only class that has already been met on this train's own predecessors.  LEG 4 and LEG 5 remain the emission instruments over the BEHAVIORAL corpus; LEG D is the one over the STDLIB corpus, and they are different populations."
  # --- THE src/gen HALF, ASSERTED SEPARATELY.  ⚠ FALSE-GREEN ROUTE #7 IS A DIFFERENT OBLIGATION FROM
  #     THE THREE ABOVE and it is checked here rather than folded into them: a `src/gen` change is
  #     invisible to CNR (transpile-only) and to the stdlib solution (which compiles one assembly at a
  #     time, so an accessibility demotion that breaks only CROSS-assembly consumers stays green), and
  #     the corpus + CNR ladder a converter arc normally runs therefore proves NOTHING about it.  The
  #     doctrine's rule is explicit: any change under src/gen owes a FULL behavioral COMPILE phase and
  #     at least one cross-assembly consumer gate.
  if [ "$G11GEN" -ge 1 ]; then
    G11W_SUITE=0; G11W_SLNX=0
    grep -aqF -- 'run-behavioral.ps1 --build-timeout' "$SCRIPT_PATH" && G11W_SUITE=1
    grep -aqF -- 'go2cs.slnx' "$SCRIPT_PATH" && G11W_SLNX=1
    stamp "G11(b) src/gen JUSTIFICATION :: $G11GEN file(s) under src/gen are in the delta (a merged row whose CLASS owes src/gen), so ROUTE #7's obligations are OWED :: the FULL behavioral suite is wired here=$G11W_SUITE (must be 1 -- LEG 5, whose COMPILE phase is the only leg that builds a cross-assembly consumer of the generated shell) :: the go2cs.slnx build is wired here=$G11W_SLNX (must be 1 -- LEG 2b, the cross-assembly consumer gate CLAUDE.md names BY NAME).  ⚠ It is ALSO why LEG K carries the DERIVED reflect canary set: a go2cs-gen shell template is reflect-bridge-touching by the canary rule's own wording."
    { [ "$G11W_SUITE" = "1" ] && [ "$G11W_SLNX" = "1" ]; } || { stamp "  ^ G11(b) JUSTIFICATION FALSE :: a src/gen change is in this train's delta and one of route #7's two named gates is NOT wired in this script.  That combination is how a promoted-forwarder demotion shipped green on a 307/0 + byte-identical-CNR ladder and was caught days later by a derived canary sweep."; FAILED=1; }
  else
    if [ "${OWED_GEN:-0}" = "0" ]; then
      stamp "G11(b) src/gen JUSTIFICATION :: ZERO files under src/gen in this train's delta, and the OWED VECTOR reads gen=$OWED_GEN (no merged row's class owes src/gen) -- the EXPECTED reading for THIS train, stated as a reading.  (Per (L7) every justification derives from the OWED vector rather than from a row's position; an earlier derive carried a per-row premise instead and refused a healthy tree for it.)"
    else
      stamp "G11(b) src/gen JUSTIFICATION :: ZERO files under src/gen in this train's delta while the OWED VECTOR reads gen=$OWED_GEN -- the PARTIAL-TRAIN reading when the src/gen seat is skipped, stated rather than passed over."
      [ "$SEATS_SKIPPED" -ge 1 ] || { stamp "  ^ G11(b) JUSTIFICATION FALSE :: no seat was skipped and yet src/gen is untouched, where a merged row's class OWES src/gen (OWED_GEN=$OWED_GEN)."; FAILED=1; }
    fi
  fi
else
  if [ "${OWED_CONV:-0}" = "1" ] || [ "${OWED_GEN:-0}" = "1" ]; then
    stamp "G11(b) JUSTIFICATION FALSE :: ZERO files under src/go2cs and src/gen, where the OWED VECTOR reads converter=$OWED_CONV gen=$OWED_GEN (merged rows' classes owe converter changes).  Either those seats did not land or this is not the train this script was derived for."
    FAILED=1
  else
    stamp "G11(b) :: ZERO files under src/go2cs and src/gen and the OWED VECTOR owes neither -- a READING (no converter seat on this train), not a refusal."
  fi
fi

# --- G11(c) THE BATTERY LEGS ARE ACTUALLY WIRED IN THIS SCRIPT ---------------------------------
#     ⚠ THIS ARM READS THE INSTRUMENT, NOT THE TREE, AND SAYS SO.  It exists because a justification
#     that says a leg is owed, in a script that does not run it, is worse than no justification.
#     ⚠ SCRIPT_PATH IS ABSOLUTE (resolved before the cd at the top of this file).  Train 44 run 1 read
#     `0 of 6` here from a RELATIVE ${BASH_SOURCE[0]} grepped after the cd -- grep could not open the
#     file at all, and the arm reported its own blindness as a finding about the script.  The absolute
#     form is carried here and the derive-time self-check asserts it.
G11WIRED=0; G11MISSING=""
for legpat in 'run-behavioral.ps1 --filter' 'go2cs-stdlib.slnx' 'go2cs.slnx' 'GolibTests/GolibTests.csproj' 'check-no-regression.ps1' 'run-validated-sweep.ps1' 'go test -count=1 -timeout 30m ./...' '-test-action' '-stdlib -comments'; do
  if grep -aqF -- "$legpat" "$SCRIPT_PATH"; then G11WIRED=$(( G11WIRED + 1 )); else G11MISSING="$G11MISSING [$legpat]"; fi
done
stamp "G11(c) battery legs WIRED IN THIS SCRIPT :: $G11WIRED of 9 launch lines present (run-behavioral --filter, go2cs-stdlib.slnx, go2cs.slnx, GolibTests.csproj, check-no-regression.ps1, run-validated-sweep.ps1, the converter suite's own go-test line, the -tests pipeline's -test-action, and LEG D's own \`-stdlib -comments\` arm) read from the ABSOLUTE self-path $SCRIPT_PATH"
[ "$G11WIRED" = "9" ] || { stamp "  ^ G11(c) REFUSED: missing launch line(s):$G11MISSING -- a gate family this train's justifications declare OWED is not wired"; FAILED=1; }

stamp "=== LIGHT GATES DONE :: overallFailed so far = $FAILED.  The battery legs follow; a red COMPILE gate STOPS the chain and names every unrun leg as UNMEASURED. ==="
LEGS_ALL="LEG 0 dial guard | LEG 1 integrity x3 GOOS | LEG 2 go2cs-stdlib.slnx | LEG 2b go2cs.slnx | LEG D two-seeded three-target -stdlib emission diff | LEG R -tests convert-then-build of reflect and errors | LEG U unicode/utf8 -tests platform-scope control (row 1's own seat gate) | LEG 3 GolibTests x2 configurations | LEG 4 CNR under the pairing | LEG 5 FULL behavioral suite | LEG K derived reflect canary set + sync + nistec"
chain_stop(){ # $1 = the leg that stopped it, $2 = the reason, $3 = the legs not run
  stamp "=== CHAIN STOPPED at $1 ==="
  stamp "Reason: $2"
  stamp "UNMEASURED, and NAMED so nobody reads their absence as a pass: $3"
  stamp "post-battery dirty=$(git status --porcelain | wc -l) freeGB=$(freegb) (was ${FREE0} at the preflight)"
  stamp "=== ${LABEL} ASSEMBLE DONE head=$(git rev-parse --short HEAD) base=$BASE route=$ROUTE seats=$SEATS_DONE overallFailed=1 (stopped at $1) ==="
  exit 1
}

# ============================================================================================
# LEG 0 -- THE PAIRING'S CHEAPEST POSITIVE CONTROL, FIRST AND CHEAP.  ⚠ EXPECTATION E3', AND IT IS
# **GREEN**.
#   ⚠ THIS LEG'S EXPECTATION HAS MOVED THREE TIMES NOW, AND EVERY MOVE IS STATED RATHER THAN QUIETLY
#   REWRITTEN.  Two forms ago it demanded a clean PASS and STOPPED THE CHAIN on a red.  One form ago
#   -- every behavioral leg at the 1.24.13 pin -- it expected the Δ-drop artifact's RED (Transpile
#   pass, Target fail, Compile fail with CS0576, Output skip).  Train 44's fifth arm retired that
#   configuration from batteries, so the leg ran under the PAIRING and its expectation went green
#   again.  THE THIRD MOVE IS THIS DERIVE'S (2026-09-08) AND IT IS A **REMOVAL**:
#       Transpile pass 1 / fail 0
#       Compile   pass 1 / fail 0
#       Target    pass 1 / fail 0
#       Output    1 compared, 0 failed   the comparison RUNS, which is the reading that matters -- an
#                                        Output line reading "0 compared" is a phase that never ran
#                                        and is never a pass
#       and its emitted main.cs still carries `using Δruntime = runtime_package;`
#   ⚠ THE **NAMED** PAIRING-FAILURE BRANCH IS RETIRED.  It read a Target FAIL + Compile FAIL + CS0576
#   and NAMED it "the pairing did not hold -- this leg ran at the 1.24.13 pin", on the evidence that
#   the 1.24.13 converter DROPPED the Δ off the runtime package alias and the bare form does not
#   compile against the 1.23.12 corpus.  The landed alias fold makes the emission
#   PIN-INDEPENDENT, so that signature can no longer be produced by a failed pairing: the branch
#   cannot be taken, and a branch that cannot be taken is the warm-design trap.  Any red here is now
#   a finding about the TREE, reported as one, with the PAIRING stamps read first.
#   ⚠ WHAT IS LOST BY THAT, SAID OUT LOUD: this leg no longer distinguishes an ENVIRONMENT failure
#   from a TREE failure by the red's shape.  The pairing's own stamps and the after-guard answer that
#   question now, and LEG 4 is the set-level instrument that attributes.
#   ⚠ THE Δ READ STAYS, AND IT IS NOT THE RETIRED BRANCH.  Reading the alias OUT OF THE EMISSION is an
#   invariant a converter seat can still break -- three seats in this train change converter source --
#   so it can go red and it is asserted.  What was retired is the DIAGNOSIS built on it, not the read.
#   ⚠ WHY IT IS KEPT WHEN LEG 5 SUBSUMES IT: it is E2' on ONE project, in minutes rather than hours,
#   and it is the cheapest possible statement that the pairing held before ~2-3 hours are spent on the
#   full suite.  Budgeted small.
#   ⚠ AND WHAT IT DOES **NOT** MEASURE, SAID OUT LOUD RATHER THAN LEFT TO BE INFERRED: no seat's own
#   mechanism is measured here.  A src/gen row's is LEG 2b's and LEG 5's (route #7); a golib-corpus
#   row's is LEG K's `sync` row and LEG 3's arms; a converter row's is LEG 1's registration
#   says the PAIRING held -- a statement about the environment every later leg is read in, and not a
#   statement about any seat.
#   ⚠ NO CHAIN STOP, unchanged: LEG 4 and LEG 5 are the SET-LEVEL readings that attribute, and stopping
#   would deny them.
#   ⚠ run-behavioral.ps1 runs at $ErrorActionPreference='Stop', so a native stderr line can kill the
#   WRAPPER and orphan the runner, leaving a TRUNCATED log.  That signature is DETECTED here rather
#   than read as a failure: a log with no terminal PASS/FAIL line is UNMEASURED, not red.
#   ⚠ THE REBUILD LINE IS **NO LONGER EXPECTED**, AND THAT SENTENCE USED TO SAY THE OPPOSITE.  Until
#   train 45's seat 4 the runner's staleness predicate compared `go env GOVERSION` at its own cwd
#   (1.23.12 under the pairing) against the binary's embedded 1.24.13 and was PERMANENTLY stale, so it
#   rebuilt on every invocation.  That seat compares the embedded release against the converter
#   MODULE's own `go` directive instead -- the right predicate -- so ZERO is now the expected reading
#   here and a NON-zero is the odd one.  It is STAMPED either way, never a fault.
pin_go_pairing "LEG-0" || { stamp "LEG 0 UNMEASURED :: the two-pin pairing could not be established, so the leg would measure an environment nobody ruled on"; FAILED=1; }
LEG0_FAILED=0
for guard in SetFinalizerBridge; do
  L0="$SCRIPT_DIR/coord-${TAG}-guard-$guard-$RUNID.log"; t0=$(date +%s)
  powershell -NoProfile -ExecutionPolicy Bypass -File src/tests/Behavioral/run-behavioral.ps1 --filter "$guard" < /dev/null > "$L0" 2>&1
  rc=$?; w=$(( $(date +%s) - t0 ))
  tr -d '\r' < "$L0" > "$L0.t" && mv "$L0.t" "$L0"
  read -r G_CMP G_FAIL G_TERM G_SPASS G_SFAIL <<< "$(read_output_verdict "$L0")"
  REF=$(refusal_markers "$L0")
  # The four phase verdicts, read from the runner's own per-phase summary rows rather than inferred
  # from the terminal word.  Each row reads like:  Transpile   pass 1  fail 0  skip 0
  ph_of(){ tr -d '\r' < "$L0" | grep -aiE "^[[:space:]]+$1[[:space:]]+pass" | tail -1 | tr -s ' '; }
  P_TR=$(ph_of Transpile); P_CO=$(ph_of Compile); P_TA=$(ph_of Target); P_OU=$(ph_of Output)
  fail_of(){ printf '%s' "$1" | grep -aoE 'fail[[:space:]]+[0-9]+' | grep -aoE '[0-9]+'; }
  pass_of(){ printf '%s' "$1" | grep -aoE 'pass[[:space:]]+[0-9]+' | grep -aoE '[0-9]+'; }
  F_TR=$(fail_of "$P_TR"); F_CO=$(fail_of "$P_CO"); F_TA=$(fail_of "$P_TA")
  A_TR=$(pass_of "$P_TR"); A_CO=$(pass_of "$P_CO"); A_TA=$(pass_of "$P_TA")
  CS0576=$(grep -ac 'CS0576' "$L0" || true)
  REBUILT=$(grep -ac 'Building go2cs.exe (converter sources changed)' "$L0" || true)
  # the Δ the pairing is supposed to KEEP, read from the emission itself rather than inferred from a
  # green.  ⚠ BEFORE the restore, or there is nothing left to read.
  L0DELTA=$(grep -acF -- "$ALIAS_OLD" "src/tests/Behavioral/$guard/main.cs" 2>/dev/null || true)
  stamp "LEG 0 guard=$guard exit=$rc wall=${w}s :: Output $G_CMP compared / $G_FAIL failed (summary pass=$G_SPASS fail=$G_SFAIL) terminal=$G_TERM CS0576mentions=$CS0576 converterRebuildLines=$REBUILT (>=1 EXPECTED under the pairing -- a READING, never a fault)${REF:+ refusalMarkers=[$REF]}"
  stamp "LEG 0 phase table :: [$P_TR] [$P_CO] [$P_TA] [$P_OU]"
  stamp "LEG 0 phase verdicts :: Transpile ${A_TR:-?}/${F_TR:-?} Compile ${A_CO:-?}/${F_CO:-?} Target ${A_TA:-?}/${F_TA:-?} (pass/fail; E3' expects 1/0 on all three)"
  stamp "LEG 0 emission Δ :: src/tests/Behavioral/$guard/main.cs carries [$ALIAS_OLD] x$L0DELTA (must be >= 1 -- the pairing's claim is that the Δ is KEPT, and a green alone does not say what is in the file)"
  if [ "$G_TERM" = "NONE" ]; then
    stamp "  ^ LEG 0 $guard UNMEASURED: no terminal PASS/FAIL line -- the Stop-preference wrapper died and orphaned the runner, or the log was truncated.  This is neither a red nor a green; it is an unmeasured leg, and it is not a verdict about anything."
    LEG0_FAILED=1
  elif [ "${F_TR:-1}" = "0" ] && [ "${F_CO:-1}" = "0" ] && [ "${F_TA:-1}" = "0" ] && [ "${G_CMP:-0}" = "1" ] && [ "${G_FAIL:-1}" = "0" ] && [ "${L0DELTA:-0}" -ge 1 ]; then
    stamp "LEG 0 E3' MET :: all four phases clean, Output 1 compared / 0 failed, and the emission KEEPS the Δ -- the two-pin pairing held, which is the cheapest statement available that LEG 4's and LEG 5's expectations describe the tree this run is producing."
  else
    # ⚠ THE **NAMED** PAIRING-FAILURE BRANCH IS RETIRED HERE (2026-09-08).  It matched Target FAIL +
    #   Compile FAIL + CS0576 and named it "the pairing did not hold", on i9's measurement of the
    #   Δ-drop signature.  The landed alias fold makes the emission PIN-INDEPENDENT, so that
    #   signature can no longer be produced by a failed pairing and the branch could not be taken.
    #   Its inputs are still READ and PRINTED below -- CS0576 in particular, because it remains the
    #   cheapest tell that a 1.23.12 corpus met a 1.24.13-shaped emission -- but they are stamped as a
    #   READING and no longer name a cause this leg cannot establish.
    stamp "  ^ LEG 0 FINDING -- E3' is not met.  This is a finding about the TREE, and it is NOT named: the pairing-failure diagnosis that once sat here rested on the Δ-drop signature, which the alias fold retired.  Read the PAIRING stamps above FIRST (they are what say the environment held), then the OWED VECTOR above (converter=$OWED_CONV gen=$OWED_GEN golib=$OWED_GOLIB corpus=$OWED_CORPUS) and the rows whose CLASSES set it -- named there, derived at run time, and never carried as a premise about which row is which.  Inputs, printed rather than interpreted :: CS0576mentions=$CS0576 (a NON-zero is the cheapest tell that a 1.23.12 corpus met a 1.24.13-shaped emission) :: emissionCarriesAlias=$L0DELTA.  Detail:"
    grep -aA3 -- '---- 1 failing project(s) ----' "$L0" | sed 's/^/      /' | tee -a "$ASMLOG" > /dev/null
    grep -aE 'exit code mismatch|stdout mismatch|Fatal error|error CS[0-9]+|\[Target\]|\[Output\]' "$L0" | head -8 | sed 's/^/      /' | tee -a "$ASMLOG"
    LEG0_FAILED=1
  fi
done
# the after-guard is a READING here, not an assertion: the fifth arm names LEG 4 and LEG 5 as the two
# legs that owe it, and adding an unasked refusal to LEG 0 would be a gate nobody ruled on.
after_guard_converter "LEG-0 (reading only)" stamp || stamp "  ^ LEG 0 after-guard did not read go1.24.13 -- STAMPED, not asserted, because the ruling puts this guard on LEG 4 and LEG 5.  If LEG 4's own after-guard refuses, this line is where it started."
git checkout -- src/tests/Behavioral 2>/dev/null; git clean -fdq -- src/tests/Behavioral 2>/dev/null
L0DEL=$(git status --porcelain | grep -c '^ D')
stamp "LEG 0 post-restore :: dirty=$(git status --porcelain | wc -l) deleted-tracked=$L0DEL (both must be 0)"
[ "$L0DEL" = "0" ] || { stamp "  ^ the restore DELETED TRACKED FILES -- restore them before anything else"; LEG0_FAILED=1; }
[ "$LEG0_FAILED" = "0" ] || FAILED=1
stamp "LEG 0 DONE (legFailed=$LEG0_FAILED) -- the chain CONTINUES either way: LEG 4 and LEG 5 are the SET-LEVEL instruments that attribute, and stopping here would deny them."

# ============================================================================================
# LEG 1: corpus/solution integrity per GOOS.  Cheap, and CNR runs it as its own preflight anyway --
# run explicitly so all three targets get their own stamped reading.
# ============================================================================================
for goos in windows linux darwin; do
  L="$SCRIPT_DIR/coord-${TAG}-integ-$goos-$RUNID.log"; t0=$(date +%s)
  ( cd src/tests/Behavioral && powershell -NoProfile -ExecutionPolicy Bypass -File ./check-solution-integrity.ps1 -TargetOS $goos < /dev/null ) > "$L" 2>&1
  rc=$?; w=$(( $(date +%s) - t0 ))
  tr -d '\r' < "$L" > "$L.t" && mv "$L.t" "$L"
  stamp "LEG 1 integrity-$goos exit=$rc wall=${w}s :: $(grep -aiE 'PASS|FAIL|cycle' "$L" | tail -1 | cut -c1-140)"
  [ $rc -eq 0 ] || FAILED=1
done

# --- THE REGISTRATION ARITHMETIC, DERIVED AT RUN TIME AND NEVER A LITERAL.  The gate is the DIFFERENCE
#     the change makes -- registrations added == projects added -- because a total carried from another
#     tree is the stale-figure class.  MEASURED 2026-09-13 at a02ac3df3 over all sixteen seat tips of the table AS IT THEN STOOD
#     (the former row 8 has since been UNSEATED, which can only REMOVE a candidate): NO
#     seat adds a src/tests/Behavioral/*.csproj (an earlier note here claimed seats 3 and 8 each add one;
#     neither touches src/tests at any pin), so the predicted head figure EQUALS master's registration
#     count and both sides of the assertion are read from the tree at run time.  ⚠ A registration without a
#     project, or a project without a registration, is the nsshadow shape: every harness gate stays
#     green and only the SOLUTION build in Visual Studio breaks.
L1REGH=$(grep -ac '<Project Path="tests/Behavioral/' src/go2cs.slnx 2>/dev/null || true)
L1REGB=$(git show "$BASE:src/go2cs.slnx" 2>/dev/null | grep -ac '<Project Path="tests/Behavioral/' || true)
case "${L1REGH}" in ''|*[!0-9]*) L1REGH=0 ;; esac
case "${L1REGB}" in ''|*[!0-9]*) L1REGB=0 ;; esac
L1REGD=$(( L1REGH - L1REGB ))
L1COUNTLINE=$(grep -ah 'behavioral projects are registered' "$SCRIPT_DIR/coord-${TAG}-integ-windows-$RUNID.log" 2>/dev/null | tail -1 | grep -aoE '[0-9]+' | head -1)
stamp "LEG 1 registration arithmetic (DERIVED from THIS tree at run time) :: behavioral <Project Path> lines in go2cs.slnx at BASE=$L1REGB, at the assembled HEAD=$L1REGH, delta=$L1REGD :: NEW behavioral projects TOP-LEVEL=$NEWBEH ALL DEPTHS=$NEWBEH_ALL (nested sub-libraries=$NEWBEH_NESTED) :: the integrity guard's own count line reads ${L1COUNTLINE:-UNREADABLE}"
# ⚠ **THE DELTA IS COMPARED AGAINST ALL DEPTHS, NOT AGAINST THE TOP-LEVEL COUNT, AND THAT IS THIS
#   TRAIN'S OWN CORRECTION.**  Train 45's arm compared it against $NEWBEH, which was right there
#   because both of its guard projects were FLAT.  Seat 1 of this train adds a guard carrying TWO
#   SUB-LIBRARIES, each with its own .csproj, so the solution grows by THREE registrations for ONE
#   top-level project -- and the carried comparison would have read 3 against 1 and REFUSED a correct
#   seat.  FALSE-GREEN ROUTE #3 is the mirror of that: a nested package that no solution entry names
#   passes EVERY harness gate and breaks the solution in Visual Studio only, which is exactly how
#   `nsshadow` slipped through and stayed unregistered for two commits.
#   ⚠ THE TWO NUMBERS ANSWER DIFFERENT QUESTIONS AND BOTH ARE PRINTED: solution registrations are owed
#   at ALL DEPTHS; goldens and the four MSTest `Check<Name>()` registrations are owed only at TOP LEVEL
#   (UpdateTestTargets is deliberately top-level-only), and A5 above is where that half is asserted.
[ "$L1REGD" = "${NEWBEH_ALL:-0}" ] || { stamp "  ^ LEG 1 REFUSED: $L1REGD registration(s) were added for $NEWBEH_ALL new behavioral .csproj at ALL DEPTHS ($NEWBEH top-level + $NEWBEH_NESTED nested).  A project on disk that no solution entry names passes every harness gate and breaks only Visual Studio; an entry for a project that is not there breaks the solution outright.  ⚠ Compare this against A5's per-project registration reads before attributing: the two derivations walk the same tree by different routes, and the one that disagrees is where to look."; FAILED=1; }
[ "$L1REGD" -ge 0 ] || { stamp "  ^ LEG 1 REFUSED: the registration count went DOWN ($L1REGB -> $L1REGH).  No seat in this train removes a behavioral project."; FAILED=1; }


# ============================================================================================
# LEG 2: go2cs-stdlib.slnx (windows flavour).  ⚠ OWED BY **SEAT 2's** src/core/golib/slice.cs and by
# **SEAT 3's** golib primitive, its runtime hand-owns, its per-GOOS package_info.cs and the panic.cs
# body its registration DISPLACES -- every one a CORPUS file, and the stdlib solution is the gate that
# compiles them.  (No seat in this train carries a .cs.auto, and nothing here or anywhere else in this
# battery compiles one, which G10b states rather than glosses.)
# --no-incremental because
# what differs between targets is the <Compile> ITEM SET and not any source timestamp.
# CS and MSB/NETSDK stay TWO numbers: folding them makes a contention-born MSB storm and a real
# regression indistinguishable, and an all-I/O histogram is ONE environmental failure wearing N codes.
# ============================================================================================
stamp "LEG 2 go2cs-stdlib.slnx (windows) starting -- budget ~516 s --no-incremental on this box class"
L2="$SCRIPT_DIR/coord-${TAG}-stdlibslnx-$RUNID.log"; t0=$(date +%s)
dotnet build src/go2cs-stdlib.slnx -c Debug -m -p:UseSharedCompilation=false -p:GoTargetOS=windows --no-incremental > "$L2" 2>&1
rc=$?; w=$(( $(date +%s) - t0 ))
CS=$(grep -acE 'error CS[0-9]+' "$L2"); MSB=$(grep -acE 'error (MSB|NETSDK)[0-9]+' "$L2")
stamp "LEG 2 stdlib slnx exit=$rc wall=${w}s :: CS=$CS MSB/NETSDK=$MSB (two numbers, never folded)"
[ "$CS" = "0" ] || grep -aE 'error CS[0-9]+' "$L2" | sed -E 's/.*(error CS[0-9]+)/\1/' | sort | uniq -c | sort -rn | head -5 | while read -r l; do stamp "    $l"; done
[ "$MSB" = "0" ] || grep -aE 'error (MSB|NETSDK)[0-9]+' "$L2" | sed -E 's/.*(error (MSB|NETSDK)[0-9]+)/\1/' | sort | uniq -c | sort -rn | head -5 | while read -r l; do stamp "    $l"; done
if [ $rc -ne 0 ]; then
  fail_gate LEG-2
  if [ "$CS" = "0" ] && [ "$MSB" != "0" ]; then
    stamp "  ^ LEG 2: CS=0 with MSB/NETSDK=$MSB -- read the code histogram above before believing a regression.  An all-I/O histogram (MSB3491/MSB3027/MSB3021/CS0016/CS8104, every message 'not enough space on the disk') is ONE environmental failure wearing N codes; a contention-born MSB storm clears on a solo re-run."
  fi
  chain_stop "LEG 2 (go2cs-stdlib.slnx)" "the corpus does not compile at this train's union (OWED vector golib=$OWED_GOLIB corpus=$OWED_CORPUS), so nothing below can be read as a statement about this train." "$LEGS_ALL"
fi

# ============================================================================================
# LEG 2b: go2cs.slnx.  ⚠ A SEPARATE LEG WITH A SEPARATE REASON, not a duplicate of LEG 2.  CLAUDE.md
# names this build BY NAME for a golib API change: "NOTHING routinely builds go2cs.slnx end to end, so
# a broken solution member rots invisibly ... After changing a golib/runtime API, build src/go2cs.slnx
# once before banking -- and no other gate covers it."  Seat 7 ADDS a golib API
# (a PointerTokens.cs beside the box base, plus new members on the base itself), so this is that
# gate -- and it is also the leg that would surface a CS0534 if a new abstract member had arrived
# on the box base without every box kind answering it, which G12 above names in seconds instead.
# ============================================================================================
stamp "LEG 2b go2cs.slnx starting -- budget ~845 s solo, ~3600 s under sibling-lane load"
L2B="$SCRIPT_DIR/coord-${TAG}-go2csslnx-$RUNID.log"; t0=$(date +%s)
dotnet build src/go2cs.slnx -c Debug -m -p:UseSharedCompilation=false --no-incremental > "$L2B" 2>&1
rc=$?; w=$(( $(date +%s) - t0 ))
CS=$(grep -acE 'error CS[0-9]+' "$L2B"); MSB=$(grep -acE 'error (MSB|NETSDK)[0-9]+' "$L2B")
stamp "LEG 2b go2cs.slnx exit=$rc wall=${w}s :: CS=$CS MSB/NETSDK=$MSB"
[ "$CS" = "0" ] || grep -aE 'error CS[0-9]+' "$L2B" | sed -E 's/.*(error CS[0-9]+)/\1/' | sort | uniq -c | sort -rn | head -5 | while read -r l; do stamp "    $l"; done
[ "$MSB" = "0" ] || grep -aE 'error (MSB|NETSDK)[0-9]+' "$L2B" | sed -E 's/.*(error (MSB|NETSDK)[0-9]+)/\1/' | sort | uniq -c | sort -rn | head -5 | while read -r l; do stamp "    $l"; done
if [ $rc -ne 0 ]; then
  fail_gate LEG-2b
  chain_stop "LEG 2b (go2cs.slnx)" "the non-generated solution members do not compile at this train's union.  ⚠ THIS IS ROUTE #7's NAMED GATE: a src/gen change (OWED gen=$OWED_GEN) is invisible to CNR (transpile-only) and to the stdlib solution (one assembly at a time), so this build and LEG 5's COMPILE phase are the only legs that compile a CROSS-ASSEMBLY consumer of the generated shell." "LEG D two-seeded three-target -stdlib emission diff | LEG R -tests convert-then-build | LEG 3 GolibTests x2 | LEG 4 CNR | LEG 5 FULL behavioral suite | LEG K derived canary set + sync + nistec"
fi

# ============================================================================================
# LEG D -- THE UNION TWO-SEEDED, THREE-TARGET `-stdlib` EMISSION DIFF.
#   ⚠⚠ THIS IS THE **CORRECTED** LEG, AND EVERY CORRECTION HAS A MEASUREMENT BEHIND IT.  Train 45
#   wired this leg for the first time and it REFUSED AT ITS OWN FIRST CONTROL, measuring nothing: the
#   control compared `git archive HEAD -- src/go2cs` byte-for-byte against the worktree checkout,
#   found ONE difference (`src/go2cs/manualConversionDestination_test.go`) and stamped UNMEASURED.
#   The refusal was the control working exactly as written; the CONTROL was written at the WRONG
#   LAYER, and a standalone re-run measured the mechanism rather than assuming it:
#     * `git ls-files --eol -- 'src/go2cs/*.go'` reads 264 files, ALL `i/lf` -- every blob is LF, so
#       "the other files have CRLF blobs" describes no tree.  263 read `w/crlf` and exactly ONE reads
#       `w/lf`.  `git archive` does NOT export the raw blob (the extracted main.go carries its 739
#       CRs, identical to the checkout), so the ARCHIVE is the conventional side and the WORKTREE is
#       the odd one out: that one file was written with LF by some tool and, because autocrlf
#       normalises LF->LF on add, `git status` reads CLEAN on it forever.
#     * The same `diff -qr` WITH `--strip-trailing-cr` over all 300 extracted files reports ZERO.
#     * The one differing file is a `_test.go`, which `go build` EXCLUDES from the binary -- the same
#       fact route #5's converter build-input set is built on.
#     * All FIFTEEN `//go:embed` targets are BYTE-IDENTICAL between the archive and the checkout.
#       Those are the real emission inputs the wired control's own comment was reaching for, and it
#       then tested a population twenty times wider than the hazard, at a layer the hazard does not
#       live on.
#
#   THE FIVE CORRECTIONS, EACH STATED AS WHAT IT NOW DOES:
#     (i)   THE SEEDING CONTROL IS **THREE ARMS**, not one.
#             ARM 1  NON-TEST converter source, CR-STRIP-identical, must be 0 -- with the RAW count
#                    stated beside it so an exclusion that swallowed everything is visible, and with
#                    the `_test.go` population COUNTED so the reader can interpret the number.
#             ARM 2  the raw-differing set is CLASSIFIED, never merely subtracted: EVERY raw-differing
#                    file -- `_test.go` INCLUDED -- must be CR-STRIP-IDENTICAL.  A difference that
#                    SURVIVES the strip is a real CONTENT difference and refuses BY NAME.  ⚠ THE
#                    PREDICATE IS THE HAZARD, NOT THE FILE NAME: an earlier form demanded that every
#                    raw-differing file BE a `_test.go`, which refuses a benign line-ending-only
#                    difference in a NON-test file, and its own control caught that (arm 1 green, arm
#                    2 red, one file).  The `_test.go` / non-test split is STATED and is not a refusal
#                    criterion.
#             ARM 3  the `//go:embed` targets, BYTE-exact (NOT CR-stripped: those bytes are embedded
#                    verbatim and a line-ending shift in one WOULD move the arm's whole emission).
#                    ⚠ THE LIST IS **DERIVED FROM THE DIRECTIVES THEMSELVES**, the way
#                    `src/tests/ConverterBuildInputs.cs` derives it -- never listed here -- so a
#                    directive added tomorrow is covered the day it is written.  The DERIVED COUNT is
#                    PRINTED and asserted > 0: a derivation that silently resolved nothing would make
#                    this arm a vacuous green, and it is the arm the whole seeding argument rests on.
#                    (Fifteen at the standalone's measurement; the number is a NOTE, the derivation is
#                    the rule.)
#     (ii)  **BOTH ARMS ARE BUILT FROM `git archive`** -- BASE from $BASE, CUT from the union HEAD --
#           where the wired leg built the CUT arm from the live worktree.  A deliberate TIGHTENING:
#           building both arms the same way makes the seeding question SYMMETRIC, and whatever the
#           archive does to one arm it does to the other.  Arm 3 above is what licenses it.  The two
#           binaries' hashes are ASSERTED TO DIFFER -- with three converter seats aboard, two
#           identical binaries would mean the base extraction silently produced the union's source.
#     (iii) ALL SIX SEEDS ARE CUT FROM **ONE FROZEN SNAPSHOT** BEFORE ANY ARM CONVERTS.  A script that
#           seeds each arm at the moment that arm starts seeds two different trees when anything moves
#           between them, and the diff then reports files NEITHER converter wrote.
#     (iv)  **THE PREDICTION IS DERIVED AT RUN TIME FROM EVERY SEAT, PER FILE, AND COMPARED BY
#           MECHANISM.**  Two halves, and the second is this derive's own deliverable:
#             * THE DERIVATION IS THE **THREE-DOT** FORM, over `src/core`, for EVERY MERGED SEAT --
#               not one hand-picked seat.  Two-dot against a base a seat forked before returns
#               MASTER'S OWN NEWER CONTENT REVERSED, which as an expectation reads MISSED on a correct
#               train and as an exclusion MASKS real footprint.
#             * THE PER-TARGET LINE COMPARISON IS **BY MECHANISM**, not by `cmp` against the committed
#               lines.  ⚠ THIS IS THE LINUX FINDING, MEASURED 2026-09-08 AND NOT PREDICTED: train 45's
#               LEG D compared the measured hunk against the COMMITTED hunk with `cmp`, and the
#               committed hunk was WINDOWS-derived.  On linux the same mechanism produced
#                   -  ...[GoValueClone("sigmask", "tls", "createstack", "Δtrace", ...)]
#                   +  ...[GoValueClone("sigmask", "tls", "createstack", "trace",  ...)]
#               -- the identical token change `"Δtrace"` -> `"trace"`, inside a field list that
#               carries `sigmask` at its head on that flavour and does not on windows.  `cmp` read
#               MISSED on a target that had MET perfectly.  So the comparison now derives the MINIMAL
#               DIFFERING TOKEN from the committed pair, applies it to the MEASURED removed line, and
#               requires the result to equal the MEASURED added line -- which is invariant under a
#               flavour-specific field list and still refuses any OTHER change.  Where a predicted
#               file's add and del counts differ (a DISPLACEMENT, e.g. a registry entry removing a
#               body: +2/-53), no token pair exists and the comparison falls back to EXACT and says so.
#               ⚠ **THE COMPARATOR WAS CONTROLLED AT DERIVE TIME, BOTH DIRECTIONS, ON THE REAL PAIR**
#               (2026-09-08, five arms, run before this leg was written into the script):
#                 1  the LINUX measured pair against the WINDOWS predicted pair  -> `MET mechanism
#                    [Δtrace -> trace]`   (the token DERIVED, not typed; the sigmask-carrying field
#                    list does not move the verdict)
#                 2  a DECOY -- the same linux pair with `chacha8` -> `chacha9` instead
#                                                                -> `MISS mechanism 1 of 1 pair(s)`
#                 3  `cmp`, i.e. train 45's own form, on the SAME inputs      -> DIFFERS, which is
#                    the FALSE MISS the linux target read and the whole reason this arm exists
#                 4  an identical pair                                        -> `MET mechanism`
#                 5  a count mismatch (+1/-2 against a predicted +1/-1)       -> `MISS counts`
#               A control that has never been made to fail is not a measurement, and arm 2 is the one
#               that says this comparison is still a GATE rather than a way of accepting anything.
#     (v)   A FAILED TARGET'S DIFF **CONTENT** (the `<` and `>` lines, not just the file names) is
#           written to a PRESERVED artifact BEFORE any purge, and the purge NEVER removes the
#           per-target diff files.  A gate whose cleanup destroys the artifact it measures reads as a
#           clean sweep.
#     (vi)  THE LEG'S RECORD IS WRITTEN THROUGH A PER-RUN TEMP AND **PUBLISHED ON EXIT**.  A run that
#           refuses at its preflight must not destroy a completed measurement sitting at that path --
#           measured on the standalone, where an injector control left a 430-byte stub where the
#           record goes.  Refusals publish to a distinct `-REFUSED` path.
#
#   WHAT IS **NOT** RELAXED, because the fault was in ONE control and a correction is not a licence:
#     * ONE CONVERTER PROCESS AT A TIME, censused BY NAME before each arm (the r41 DYNTYPE hazard).
#     * PER ARM PER TARGET, THE COUNT OF FILES **WRITTEN THIS RUN** IS ASSERTED > 0 AND STATED.  Two
#       untouched seeds compared against each other read 0/0/0 and are indistinguishable from a clean
#       gate -- the emitted-before-seeded trap, paid three times by earlier lanes.
#     * THE MARKER-FILE CONTENT CONTROL across all six seeds and the worktree, in a package THIS TRAIN
#       TOUCHES.  A file NEITHER converter writes must be byte-identical everywhere, and that is a
#       CONTENT test; mtimes are a HINT only.
#     * SINGLE-TARGET PER ARM, deliberately: `-platforms <goos>/amd64` with ONE value falls through to
#       the ordinary StdLibConverter, which HONOURS an existing L3 tree.  Both arms run the SAME target
#       into their OWN seed cut from ONE snapshot, so a file neither wrote is byte-identical by
#       construction and a file both wrote is compared like for like.
# ============================================================================================
LEGD_OK=1
LEGD_ROOT="$SCRIPT_DIR/coord-${TAG}-legD-$RUNID"
LEGD_REC="$SCRIPT_DIR/coord-${TAG}-legD-record-$RUNID.txt"
LEGD_TMPREC="$(mktemp "/tmp/${TAG}-legD.XXXXXX")"
LEGD_PUBLISH_AS=refusal
legd_publish(){
  [ -f "$LEGD_TMPREC" ] || return 0
  case "$LEGD_PUBLISH_AS" in
    measurement) cp "$LEGD_TMPREC" "$LEGD_REC" 2>/dev/null ;;
    *)           cp "$LEGD_TMPREC" "${LEGD_REC%.txt}-REFUSED.txt" 2>/dev/null ;;
  esac
  rm -f "$LEGD_TMPREC"
}
trap 'legd_publish; rm -f "$LOCK"' EXIT
legd_stamp(){ stamp "$@"; printf '[%s] %s\n' "$(date '+%F %T')" "$*" >> "$LEGD_TMPREC" 2>/dev/null; }
LEGD_FREE0=$(freegb)
legd_stamp "LEG D two-seeded three-target -stdlib emission diff STARTING :: root=$LEGD_ROOT record=$LEGD_REC freeGB=$LEGD_FREE0 (floor 12 for six seeds plus their emissions) -- budget ~9 min per three-target arm on this box"
if [ -e "$LEGD_ROOT" ]; then
  legd_stamp "LEG D UNMEASURED :: $LEGD_ROOT already exists.  A reused scratch root is how an arm ends up seeded from a tree nobody named -- refusing rather than reusing"
  FAILED=1; LEGD_OK=0
fi
if [ "$LEGD_OK" = "1" ] && [ "${LEGD_FREE0:-0}" -lt 12 ]; then
  legd_stamp "LEG D UNMEASURED :: freeGB=$LEGD_FREE0 is below the 12 GB floor this leg needs.  Disk is a gate INPUT and a leg refused by its own preflight must say so rather than stamp a plausible zero"
  FAILED=1; LEGD_OK=0
fi
LEGD_PROC0=$(proc_count 'go2cs.exe')
if [ "$LEGD_OK" = "1" ] && [ "${LEGD_PROC0:-0}" != "0" ]; then
  legd_stamp "LEG D UNMEASURED :: $LEGD_PROC0 go2cs.exe process(es) alive before the leg started.  ONE converter per box at a time"
  FAILED=1; LEGD_OK=0
fi
if [ "$LEGD_OK" = "1" ]; then
  mkdir -p "$LEGD_ROOT/bin" "$LEGD_ROOT/seed" "$LEGD_ROOT/ctl" || { legd_stamp "LEG D UNMEASURED :: could not create $LEGD_ROOT"; FAILED=1; LEGD_OK=0; }
fi

# --- the extractor.  ⚠ PYTHON, NOT `tar`.  A bare `tar` on this box resolves to TWO PROGRAMS
#     depending on PATH order and the MSYS one reads a Windows path as a REMOTE HOST.  ⚠ Every path
#     handed to it is `cygpath -w`'d: a bash `-f` test and a native tool's path ARGUMENT are different
#     namespaces, and `/c/...` resolves for one only.
legd_untar(){ # $1 = tar (posix)  $2 = destination dir (posix)
  local tw dw
  mkdir -p "$2" || return 1
  tw=$(cygpath -wa "$1") || return 1
  dw=$(cygpath -wa "$2") || return 1
  python -c "import sys,tarfile
t=tarfile.open(sys.argv[1])
t.extractall(sys.argv[2])
print(len([m for m in t.getmembers() if m.isfile()]))" "$tw" "$dw" 2>/dev/null | tr -d '\r'
}

# ============================================================================================
# D0 -- THE CORRECTED SEEDING CONTROL, THREE ARMS.  ⚠ FROM HERE THE RUN IS MEASURING, so its record
#   is published as a MEASUREMENT rather than as a refusal.
# ============================================================================================
if [ "$LEGD_OK" = "1" ]; then
  LEGD_PUBLISH_AS=measurement
  git archive --format=tar HEAD -- src/go2cs > "$LEGD_ROOT/ctl-head.tar" 2>/dev/null
  LEGD_CTLN=$(legd_untar "$LEGD_ROOT/ctl-head.tar" "$LEGD_ROOT/ctl/head")
  LEGD_CTLDIR="$LEGD_ROOT/ctl/head/src/go2cs"
  if [ ! -d "$LEGD_CTLDIR" ]; then
    legd_stamp "LEG D UNMEASURED :: the control archive extracted no src/go2cs"; FAILED=1; LEGD_OK=0
  fi
fi
if [ "$LEGD_OK" = "1" ]; then
  # The tracked file list comes from the ARCHIVE, not from a filesystem walk of the worktree: the
  # worktree carries bin/obj the archive cannot, and a walk would report those as "only in one side".
  ( cd "$LEGD_ROOT/ctl/head" && find src/go2cs -type f ) | sed 's|^src/go2cs/||' | sort > "$LEGD_ROOT/ctl/files.txt"
  LEGD_RAWDIFF="$LEGD_ROOT/ctl/raw-differ.txt"; : > "$LEGD_RAWDIFF"
  LEGD_STRDIFF="$LEGD_ROOT/ctl/str-differ.txt"; : > "$LEGD_STRDIFF"
  LEGD_MISSING="$LEGD_ROOT/ctl/missing.txt";    : > "$LEGD_MISSING"
  LEGD_NT=0; LEGD_NTRAW=0; LEGD_NTSTR=0; LEGD_TN=0; LEGD_TRAW=0
  while IFS= read -r rel; do
    [ -n "$rel" ] || continue
    a="$LEGD_CTLDIR/$rel"; b="$WT/src/go2cs/$rel"
    if [ ! -f "$b" ]; then printf '%s\n' "$rel" >> "$LEGD_MISSING"; continue; fi
    case "$rel" in
      *_test.go)
        LEGD_TN=$(( LEGD_TN + 1 ))
        cmp -s "$a" "$b" || { LEGD_TRAW=$(( LEGD_TRAW + 1 )); printf '%s\n' "$rel" >> "$LEGD_RAWDIFF"; }
        ;;
      *)
        LEGD_NT=$(( LEGD_NT + 1 ))
        if ! cmp -s "$a" "$b"; then
          LEGD_NTRAW=$(( LEGD_NTRAW + 1 )); printf '%s\n' "$rel" >> "$LEGD_RAWDIFF"
          diff -q --strip-trailing-cr "$a" "$b" >/dev/null 2>&1 || { LEGD_NTSTR=$(( LEGD_NTSTR + 1 )); printf '%s\n' "$rel" >> "$LEGD_STRDIFF"; }
        fi
        ;;
    esac
  done < "$LEGD_ROOT/ctl/files.txt"
  LEGD_MISSN=$(grep -ac . "$LEGD_MISSING" || true)
  legd_stamp "LEG D CONTROL arm 1 (the seeding method itself, CORRECTED) :: git archive of HEAD's src/go2cs extracted ${LEGD_CTLN:-0} file(s); over the $LEGD_NT NON-test converter source files the CR-STRIPPED byte differences against the WORKTREE checkout = $LEGD_NTSTR (must be 0), of which RAW-byte differences = $LEGD_NTRAW (stated beside it so an exclusion that swallowed everything is visible); the $LEGD_TN _test.go files are EXCLUDED FROM THE CRITERION because \`go build\` excludes them from the binary, and $LEGD_TRAW of them differ raw; files present in the archive but ABSENT from the checkout = $LEGD_MISSN (must be 0)"
  { [ "${LEGD_NTSTR:-1}" = "0" ] && [ "${LEGD_MISSN:-1}" = "0" ] && [ "${LEGD_NT:-0}" -gt 0 ]; } || {
    legd_stamp "  ^ LEG D UNMEASURED: the archive and the checkout disagree on NON-test converter source (or the population is empty).  CR-stripped differing files:"
    head -10 "$LEGD_STRDIFF" 2>/dev/null | sed 's/^/    /' | tee -a "$ASMLOG"
    head -10 "$LEGD_MISSING" 2>/dev/null | sed 's/^/    missing: /' | tee -a "$ASMLOG"
    fail_gate LEG-D-seeding-control; LEGD_OK=0; }
fi
if [ "$LEGD_OK" = "1" ]; then
  # ARM 2 -- CLASSIFY the raw-differing set; never merely subtract it.  ⚠ THE PREDICATE IS THE HAZARD,
  #   NOT THE FILE NAME.  A `_test.go` is excluded from arm 1's CRITERION and is NOT excluded here: a
  #   CONTENT change in one still refuses, because "not a go build input" licenses ignoring its LINE
  #   ENDINGS and nothing else.
  LEGD_RAWN=$(grep -ac . "$LEGD_RAWDIFF" || true)
  LEGD_BADCLASS=0; LEGD_RT=0; LEGD_RNT=0
  while IFS= read -r rel; do
    [ -n "$rel" ] || continue
    case "$rel" in *_test.go) LEGD_RT=$(( LEGD_RT + 1 )) ;; *) LEGD_RNT=$(( LEGD_RNT + 1 )) ;; esac
    diff -q --strip-trailing-cr "$LEGD_CTLDIR/$rel" "$WT/src/go2cs/$rel" >/dev/null 2>&1 || LEGD_BADCLASS=$(( LEGD_BADCLASS + 1 ))
  done < "$LEGD_RAWDIFF"
  legd_stamp "LEG D CONTROL arm 2 (the raw-differing set, CLASSIFIED) :: raw-differing files=$LEGD_RAWN (_test.go=$LEGD_RT non-test=$LEGD_RNT -- the split is STATED because the reader needs it, and is NOT a refusal criterion), of which NOT CR-strip-identical=$LEGD_BADCLASS (must be 0 -- a difference that SURVIVES the strip is a real CONTENT difference and refuses BY NAME rather than being subtracted, `_test.go` INCLUDED)"
  [ "${LEGD_BADCLASS:-1}" = "0" ] || {
    legd_stamp "  ^ LEG D UNMEASURED: the raw-differing set is not the benign line-ending class this control accepts.  The set:"
    head -10 "$LEGD_RAWDIFF" 2>/dev/null | sed 's/^/    /' | tee -a "$ASMLOG"; FAILED=1; LEGD_OK=0; }
fi
if [ "$LEGD_OK" = "1" ]; then
  # ARM 3 -- THE //go:embed TARGETS, BYTE-EXACT, DERIVED FROM THE DIRECTIVES.
  LEGD_EMB="$LEGD_ROOT/ctl/embed.txt"; : > "$LEGD_EMB"
  ( cd "$LEGD_CTLDIR" && grep -rn '^//go:embed' . --include='*.go' 2>/dev/null ) | while IFS=: read -r f l rest; do
      d=$(dirname "$f"); pat=$(printf '%s' "$rest" | sed 's|^//go:embed[[:space:]]*||')
      for p in $pat; do ( cd "$LEGD_CTLDIR/$d" 2>/dev/null && ls -d $p 2>/dev/null | while read -r x; do
          printf '%s\n' "$d/$x" | sed 's|^\./||'; done ); done
    done | sort -u > "$LEGD_EMB"
  LEGD_EMBN=$(grep -ac . "$LEGD_EMB" || true)
  LEGD_EMBBAD=0; LEGD_EMBBADL="$LEGD_ROOT/ctl/embed-bad.txt"; : > "$LEGD_EMBBADL"
  while IFS= read -r rel; do
    [ -n "$rel" ] || continue
    cmp -s "$LEGD_CTLDIR/$rel" "$WT/src/go2cs/$rel" || { LEGD_EMBBAD=$(( LEGD_EMBBAD + 1 )); printf '%s\n' "$rel" >> "$LEGD_EMBBADL"; }
  done < "$LEGD_EMB"
  legd_stamp "LEG D CONTROL arm 3 (the //go:embed EMISSION INPUTS, BYTE-exact) :: targets RESOLVED FROM THE DIRECTIVES THEMSELVES = $LEGD_EMBN (must be > 0 -- a derivation that silently resolved nothing would make this arm a VACUOUS GREEN, and it is the arm the whole seeding argument rests on; 15 at the standalone's measurement, and the DERIVATION is the rule rather than that number) :: BYTE differences against the WORKTREE checkout = $LEGD_EMBBAD (must be 0, NOT CR-stripped: these bytes are embedded verbatim into the binary and a line-ending shift in one WOULD move the arm's whole emission)"
  { [ "${LEGD_EMBN:-0}" -gt 0 ] && [ "${LEGD_EMBBAD:-1}" = "0" ]; } || {
    legd_stamp "  ^ LEG D UNMEASURED: an embedded asset differs between the archive and the checkout (or the derivation resolved NOTHING), so an archive-built binary is NOT the tree's converter:"
    head -10 "$LEGD_EMBBADL" 2>/dev/null | sed 's/^/    /' | tee -a "$ASMLOG"; FAILED=1; LEGD_OK=0; }
  sed 's/^/    embed target: /' "$LEGD_EMB" >> "$ASMLOG"
  rm -rf "$LEGD_ROOT/ctl/head" "$LEGD_ROOT/ctl-head.tar"
fi

# ============================================================================================
# D1 -- THE TWO BINARIES, **BOTH FROM ARCHIVES**, BOTH AT 1.24.13.
# ============================================================================================
LEGD_BASEBIN="$LEGD_ROOT/bin/go2cs-base.exe"
LEGD_CUTBIN="$LEGD_ROOT/bin/go2cs-cut.exe"
if [ "$LEGD_OK" = "1" ]; then
  if ! pin_go 1.24.13 "LEG-D-build"; then
    legd_stamp "LEG D UNMEASURED :: the 1.24.13 BUILD pin could not be established, so neither arm's binary can be built"
    FAILED=1; LEGD_OK=0
  fi
fi
if [ "$LEGD_OK" = "1" ]; then
  for arm in base cut; do
    case "$arm" in base) REF="$BASE" ;; *) REF="HEAD" ;; esac
    git archive --format=tar "$REF" -- src/go2cs > "$LEGD_ROOT/$arm.tar" 2>/dev/null
    n=$(legd_untar "$LEGD_ROOT/$arm.tar" "$LEGD_ROOT/${arm}src")
    legd_stamp "LEG D $arm converter source :: git archive of $REF's src/go2cs -> ${n:-0} file(s) at $LEGD_ROOT/${arm}src/src/go2cs (go.mod present: $( [ -f "$LEGD_ROOT/${arm}src/src/go2cs/go.mod" ] && echo yes || echo NO ))"
    [ -f "$LEGD_ROOT/${arm}src/src/go2cs/go.mod" ] || { legd_stamp "  ^ LEG D UNMEASURED: the extracted $arm source carries no go.mod, so nothing can be built from it"; FAILED=1; LEGD_OK=0; }
    rm -f "$LEGD_ROOT/$arm.tar"
  done
fi
if [ "$LEGD_OK" = "1" ]; then
  # ⚠ THE SENTINEL IS TAKEN BEFORE THE BUILDS.  `go build -o` exiting 0 with a STALE binary already at
  #   the invoked path is a measured failure mode: the existence-and-size check reads the stale file
  #   and passes.  The assertion is that the binary's MTIME MOVED past this instant.
  touch "$LEGD_ROOT/.build-sentinel"
  ( cd "$LEGD_ROOT/basesrc/src/go2cs" && go build -o "$LEGD_BASEBIN" . ) > "$SCRIPT_DIR/coord-${TAG}-legd-build-base-$RUNID.log" 2>&1 < /dev/null
  lbrc=$?
  ( cd "$LEGD_ROOT/cutsrc/src/go2cs" && go build -o "$LEGD_CUTBIN" . ) > "$SCRIPT_DIR/coord-${TAG}-legd-build-cut-$RUNID.log" 2>&1 < /dev/null
  lcrc=$?
  LEGD_BASEFRESH=$( [ -f "$LEGD_BASEBIN" ] && [ "$LEGD_BASEBIN" -nt "$LEGD_ROOT/.build-sentinel" ] && echo yes || echo NO )
  LEGD_CUTFRESH=$(  [ -f "$LEGD_CUTBIN"  ] && [ "$LEGD_CUTBIN"  -nt "$LEGD_ROOT/.build-sentinel" ] && echo yes || echo NO )
  LEGD_BASEVER="$(go version "$LEGD_BASEBIN" 2>&1 | tr -d '\r')"
  LEGD_CUTVER="$(go version "$LEGD_CUTBIN" 2>&1 | tr -d '\r')"
  LEGD_BASEH=$(sha256sum "$LEGD_BASEBIN" 2>/dev/null | cut -c1-16)
  LEGD_CUTH=$(sha256sum "$LEGD_CUTBIN" 2>/dev/null | cut -c1-16)
  legd_stamp "LEG D binaries :: BASE build exit=$lbrc at $LEGD_BASEBIN mtimeMoved=$LEGD_BASEFRESH sha=$LEGD_BASEH :: \`go version\` reads [$LEGD_BASEVER]"
  legd_stamp "LEG D binaries :: CUT  build exit=$lcrc at $LEGD_CUTBIN  mtimeMoved=$LEGD_CUTFRESH  sha=$LEGD_CUTH  :: \`go version\` reads [$LEGD_CUTVER]"
  legd_stamp "LEG D binaries :: the two hashes DIFFER=$( [ "$LEGD_BASEH" != "$LEGD_CUTH" ] && echo yes || echo NO ) -- the A/B's own positive control.  THREE seats change src/go2cs, so two IDENTICAL binaries would mean the base extraction silently produced the union's source and both arms are the same converter"
  legd_stamp "LEG D binaries :: BOTH arms are built from \`git archive\` (the wired train-45 leg built the CUT arm from the live worktree) -- a deliberate TIGHTENING that makes the seeding question SYMMETRIC, licensed by control arm 3 above"
  legd_stamp "LEG D binaries :: neither exe has a converter source tree ADJACENT to it, so the -stdlib staleness REFUSAL is silent by construction and -allow-stale-converter is deliberately NOT passed -- a pinned binary with no source beside it is the shape that predicate exempts"
  { [ "$lbrc" = "0" ] && [ "$lcrc" = "0" ]; } || { legd_stamp "  ^ LEG D UNMEASURED: a converter build failed; heads of the logs:"; head -8 "$SCRIPT_DIR/coord-${TAG}-legd-build-base-$RUNID.log" | sed 's/^/    base: /' | tee -a "$ASMLOG"; head -8 "$SCRIPT_DIR/coord-${TAG}-legd-build-cut-$RUNID.log" | sed 's/^/    cut:  /' | tee -a "$ASMLOG"; FAILED=1; LEGD_OK=0; }
  { [ "$LEGD_BASEFRESH" = "yes" ] && [ "$LEGD_CUTFRESH" = "yes" ]; } || { legd_stamp "  ^ LEG D UNMEASURED: a binary is absent or its mtime did NOT move past the sentinel -- an existence check alone reads a stale file at the right path and passes"; FAILED=1; LEGD_OK=0; }
  case "$LEGD_BASEVER" in *go1.24.13*) : ;; *) legd_stamp "  ^ LEG D UNMEASURED: the BASE binary does not embed go1.24.13"; FAILED=1; LEGD_OK=0 ;; esac
  case "$LEGD_CUTVER"  in *go1.24.13*) : ;; *) legd_stamp "  ^ LEG D UNMEASURED: the CUT binary does not embed go1.24.13"; FAILED=1; LEGD_OK=0 ;; esac
  [ "$LEGD_BASEH" != "$LEGD_CUTH" ] || { legd_stamp "  ^ LEG D UNMEASURED: the two binaries are BYTE-IDENTICAL.  With three converter seats aboard that cannot be true, so the diff below would compare a converter with itself"; FAILED=1; LEGD_OK=0; }
fi

# ============================================================================================
# D2 -- ALL SIX SEEDS, FROM ONE SNAPSHOT, BEFORE ANY ARM CONVERTS.
# ============================================================================================
LEGD_TARGETS='windows linux darwin'
if [ "$LEGD_OK" = "1" ]; then
  git archive --format=tar HEAD -- src/core src/version.props docs/validation > "$LEGD_ROOT/seed.tar" 2>/dev/null
  LEGD_SEEDN=""
  for arm in base cut; do
    for goos in $LEGD_TARGETS; do
      n=$(legd_untar "$LEGD_ROOT/seed.tar" "$LEGD_ROOT/seed/$arm-$goos")
      LEGD_SEEDN="$LEGD_SEEDN $arm-$goos=${n:-0}"
    done
  done
  legd_stamp "LEG D seeds :: SIX roots cut from ONE frozen snapshot (git archive HEAD -- src/core src/version.props docs/validation) BEFORE any arm converts, so no arm can be seeded from a tree that moved under it :: files per seed:$LEGD_SEEDN"
  legd_stamp "LEG D seeds :: bin/obj/Generated are excluded BY CONSTRUCTION (an archive carries only tracked content), which is also why an untracked stdlib_conversion_progress.txt in a checkout cannot travel into a seed and be read as a previous conversion"
  # --- the marker-file CONTENT control, in a package THIS TRAIN TOUCHES.  ⚠ DERIVED, not named: the
  #     first tracked `[module: GoManualConversion]` file under a package this train's delta reaches.
  #     A marker file in an UNTOUCHED package discriminates nothing.
  LEGD_MARK=$(git -c core.quotepath=false diff --name-only "$BASE" HEAD -- src/core | sed -n 's#^\(src/core/[^/]*\(/[^/]*\)\?\)/.*#\1#p' | sort -u | while IFS= read -r p; do
      git -c core.quotepath=false ls-files "$p" 2>/dev/null | grep '\.cs$' | while IFS= read -r f; do
        [ "$(tr -d '\r' < "$f" 2>/dev/null | grep -acE '^[[:space:]]*\[module:[[:space:]]*(go\.)?GoManualConversion\]' || true)" != "0" ] && printf '%s\n' "$f"
      done
    done | head -1)
  if [ -z "$LEGD_MARK" ]; then
    legd_stamp "LEG D seed CONTENT control :: NO [module: GoManualConversion] file was found inside a package this train's delta reaches, so the seed-identity control has no DISCRIMINATING subject and falls back to a WHOLE-TREE identity check over the six seeds"
    LEGD_MARK='src/version.props'
  fi
  LEGD_MH_WT=$(sha256sum "$WT/$LEGD_MARK" 2>/dev/null | cut -c1-16)
  LEGD_MARKBAD=0; LEGD_MHLIST=""
  for arm in base cut; do
    for goos in $LEGD_TARGETS; do
      h=$(sha256sum "$LEGD_ROOT/seed/$arm-$goos/$LEGD_MARK" 2>/dev/null | cut -c1-16)
      LEGD_MHLIST="$LEGD_MHLIST $arm-$goos=${h:-absent}"
      [ "$h" = "$LEGD_MH_WT" ] || LEGD_MARKBAD=1
    done
  done
  legd_stamp "LEG D seed CONTENT control :: $LEGD_MARK (DERIVED as a hand-own inside a package this train touches, so it discriminates) :: worktree=$LEGD_MH_WT seeds:$LEGD_MHLIST -- all seven must agree, because a file NEITHER converter writes must be byte-identical everywhere and that is a CONTENT test, not a timestamp one"
  { [ "$LEGD_MARKBAD" = "0" ] && [ -n "$LEGD_MH_WT" ]; } || { legd_stamp "  ^ LEG D UNMEASURED: a seed's control file does not match the worktree's, so the six seeds are not six copies of one tree"; FAILED=1; LEGD_OK=0; }
  rm -f "$LEGD_ROOT/seed.tar"
fi

# ============================================================================================
# D3 -- THE ARMS.  ONE CONVERTER AT A TIME, EACH WITH ITS OWN WRITE-EVIDENCE SENTINEL.
# ============================================================================================
if [ "$LEGD_OK" = "1" ]; then
  if ! pin_go_pairing "LEG-D-run"; then
    legd_stamp "LEG D UNMEASURED :: the two-pin PAIRING could not be established for the RUN.  The binaries embed 1.24.13 and the corpus and its oracle LOAD at 1.23.12 until H5; -goroot does not isolate the loader"
    FAILED=1; LEGD_OK=0
  else
    legd_stamp "LEG D environment :: GOROOT=[$(go env GOROOT 2>/dev/null | tr -d '\r')] (taken VERBATIM, backslashes and all -- a forward-slash spelling misroutes the whole emission into namespace go.std.* at exit 0)"
  fi
fi
LEGD_WRITTEN=""
if [ "$LEGD_OK" = "1" ]; then
  for arm in base cut; do
    case "$arm" in base) BIN="$LEGD_BASEBIN" ;; *) BIN="$LEGD_CUTBIN" ;; esac
    for goos in $LEGD_TARGETS; do
      SEED="$LEGD_ROOT/seed/$arm-$goos"
      LOG="$SCRIPT_DIR/coord-${TAG}-legd-$arm-$goos-$RUNID.log"
      pc=$(proc_count 'go2cs.exe')
      if [ "${pc:-0}" != "0" ]; then
        legd_stamp "LEG D $arm/$goos UNMEASURED :: $pc go2cs.exe alive before this arm -- one converter at a time, and two conversions in one instrument is the r41 hazard"
        FAILED=1; LEGD_OK=0; continue
      fi
      touch "$SEED/.legd-sentinel"
      t0=$(date +%s)
      "$BIN" -stdlib -comments -platforms "$goos/amd64" -go2cspath "$(cygpath -wa "$SEED/src")" -convert-timeout 90m > "$LOG" 2>&1 < /dev/null
      arc=$?
      w=$(( $(date +%s) - t0 ))
      WRC=$(find "$SEED/src/core" -type f -newer "$SEED/.legd-sentinel" 2>/dev/null | grep -ac . || true)
      if [ -f "$SEED/src/failed_packages.txt" ]; then FP=$(grep -ac . "$SEED/src/failed_packages.txt" || true); else FP=0; fi
      SLNX=$( [ -f "$SEED/src/go2cs-stdlib.slnx" ] && echo yes || echo NO )
      LEGD_WRITTEN="$LEGD_WRITTEN $arm/$goos=$WRC"
      legd_stamp "LEG D ARM $arm/$goos :: exit=$arc wall=${w}s filesWRITTEN-this-run=$WRC (must be > 0 -- two untouched seeds compared against each other read 0/0/0 and are indistinguishable from a clean gate) failedPackages=$FP (must be 0) slnxEmitted=$SLNX"
      if [ "$arc" != "0" ]; then
        legd_stamp "  ^ LEG D $arm/$goos UNMEASURED: the conversion exited non-zero.  Tail:"
        tail -12 "$LOG" | tr -d '\r' | sed 's/^/    /' | tee -a "$ASMLOG"
        fail_gate LEG-D-conversion; LEGD_OK=0
      fi
      [ "${WRC:-0}" -gt 0 ] || { legd_stamp "  ^ LEG D $arm/$goos UNMEASURED: ZERO files were written this run, so this arm emitted nothing and any diff against it is vacuous"; FAILED=1; LEGD_OK=0; }
      [ "${FP:-0}" = "0" ] || { legd_stamp "  ^ LEG D $arm/$goos: failed_packages.txt names $FP package(s) -- a killed-but-healthy conversion reads exactly like a converter defect, so this is stated rather than absorbed:"; head -10 "$SEED/src/failed_packages.txt" | sed 's/^/    /' | tee -a "$ASMLOG"; FAILED=1; LEGD_OK=0; }
    done
  done
  legd_stamp "LEG D write-evidence, all six arms ::$LEGD_WRITTEN -- stated per arm per target, because IDENTICAL means nothing when a side was not written either"
fi

# ============================================================================================
# D4 -- THE PREDICTION, DERIVED FROM **EVERY MERGED SEAT** AND STAMPED **BEFORE** THE DIFF.
#   ⚠ THREE DOTS, AND OVER EVERY SEAT.  A seat that forked before this base makes the two-dot form
#   return MASTER'S OWN NEWER CONTENT REVERSED; and a prediction derived from ONE hand-picked seat
#   cannot describe a union of three.
#   ⚠ THE SET IS SPLIT INTO **EMITTABLE** AND **BLIND**, AND THE SPLIT IS STATED.  LEG D compares two
#   CONVERTER EMISSIONS, so it can only see files the converter WRITES.  `src/core/golib/**` is the
#   hand-written runtime, an `*_impl.cs` is a hand-own companion, and a file carrying a line-anchored
#   `[module: GoManualConversion]` is a whole-file hand-own, and a `go2cs_test_disclosures.json` is the
#   HAND-OWNED -tests disclosure manifest (validationProofPages.go names it so; -stdlib never writes it)
#   -- the converter emits NONE of them, so a seat's changes there are STRUCTURALLY INVISIBLE to this
#   leg and are named as such rather than counted as a missed prediction.  (A `.cs.auto` IS emitted --
#   the marker lives in its sibling -- so it stays in the emittable set.)
#   ⚠ 2026-09-13 (train 47 run 4): the manifest class was MISSING here, so two manifests edited by two
#   MANIFEST seats were predicted as EMITTABLE and the leg read MISSED x3 on a diff that correctly read
#   ZERO -- a false red of the PREDICTION, the measurement side untouched (write-evidence all six arms).
# ============================================================================================
LEGD_PREDFILE="$SCRIPT_DIR/coord-${TAG}-legd-expected-$RUNID.txt"
LEGD_PRED="$LEGD_ROOT/pred"; LEGD_EXPSET="$LEGD_ROOT/expected-files.txt"; LEGD_BLINDSET="$LEGD_ROOT/blind-files.txt"
if [ "$LEGD_OK" = "1" ]; then
  mkdir -p "$LEGD_PRED"
  : > "$LEGD_EXPSET"; : > "$LEGD_BLINDSET"
  LEGD_ALLSEATFILES="$LEGD_ROOT/all-seat-core-files.txt"; : > "$LEGD_ALLSEATFILES"
  LEGD_TWODOT_TOT=0
  while IFS='|' read -r pn pb ps pc pm <&3; do
    [ -n "$pn" ] || continue
    [ "$ps" = "PENDING" ] && continue
    git -c core.quotepath=false diff --name-only "$BASE...$ps" -- src/core >> "$LEGD_ALLSEATFILES" 2>/dev/null
    td=$(git -c core.quotepath=false diff --name-only "$BASE..$ps" -- src/core < /dev/null 2>/dev/null | grep -ac . || true)
    LEGD_TWODOT_TOT=$(( LEGD_TWODOT_TOT + ${td:-0} ))
    anc=$(git merge-base --is-ancestor "$BASE" "$ps" 2>/dev/null && echo yes || echo NO)
    legd_stamp "LEG D prediction, seat $pn ($pb @$ps) :: src/core files in its THREE-dot delta = $(git -c core.quotepath=false diff --name-only "$BASE...$ps" -- src/core < /dev/null 2>/dev/null | grep -ac . || true) :: the TWO-dot form gives $td :: baseIsAncestorOfSeat=$anc"
  done 3<<< "$SEAT_TABLE"
  sort -u "$LEGD_ALLSEATFILES" -o "$LEGD_ALLSEATFILES"
  while IFS= read -r f; do
    [ -n "$f" ] || continue
    blind=0
    case "$f" in src/core/golib/*) blind=1 ;; esac
    case "$f" in *_impl.cs) blind=1 ;; esac
    case "$f" in */go2cs_test_disclosures.json) blind=1 ;; esac   # the hand-owned -tests manifest (2026-09-13, run 4's false red)
    if [ "$blind" = "0" ] && [ -f "$f" ]; then
      [ "$(tr -d '\r' < "$f" 2>/dev/null | grep -acE '^[[:space:]]*\[module:[[:space:]]*(go\.)?GoManualConversion\]' || true)" != "0" ] && blind=1
    fi
    if [ "$blind" = "1" ]; then printf '%s\n' "$f" >> "$LEGD_BLINDSET"; continue; fi
    printf '%s\n' "$f" >> "$LEGD_EXPSET"
    key=$(printf '%s' "$f" | tr '/' '_')
    git diff "$BASE...HEAD" -- "$f" < /dev/null 2>/dev/null | grep -aE '^\+[^+]' | sed 's/^+//' > "$LEGD_PRED/$key.add"
    git diff "$BASE...HEAD" -- "$f" < /dev/null 2>/dev/null | grep -aE '^-[^-]' | sed 's/^-//' > "$LEGD_PRED/$key.del"
  done < "$LEGD_ALLSEATFILES"
  LEGD_EXPN=$(grep -ac . "$LEGD_EXPSET" || true)
  LEGD_BLINDN=$(grep -ac . "$LEGD_BLINDSET" || true)
  {
    printf '%s LEG D -- THE PREDICTION, STAMPED BEFORE THE DIFF WAS TAKEN\n' "$(printf '%s' "$TAG" | tr '[:lower:]' '[:upper:]')"
    printf 'derived at run time from: git diff %s...<seat> -- src/core   (THREE dots), over EVERY MERGED seat\n' "$BASE"
    printf 'EMITTABLE files (this leg can see these): %s\n' "$LEGD_EXPN"
    sed 's/^/  expect: /' "$LEGD_EXPSET"
    printf 'BLIND files (golib / *_impl.cs / whole-file hand-owns / go2cs_test_disclosures.json manifests -- the converter emits none of them): %s\n' "$LEGD_BLINDN"
    sed 's/^/  blind:  /' "$LEGD_BLINDSET"
    printf 'the TWO-dot form of the same queries returns %s file(s) in total and is WRONG here (stale-base illusion)\n' "$LEGD_TWODOT_TOT"
    for k in "$LEGD_PRED"/*.add; do
      [ -f "$k" ] || continue
      b=$(basename "$k" .add)
      printf -- '--- %s : expected ADDED (the CUT arm emits these) ---\n' "$b"; cat "$k"
      printf -- '--- %s : expected REMOVED (the BASE arm emits these) ---\n' "$b"; cat "${k%.add}.del"
    done
  } > "$LEGD_PREDFILE" 2>&1
  legd_stamp "LEG D **PREDICTION** (stamped BEFORE the diff, derived at run time from EVERY merged seat, artifact=$LEGD_PREDFILE) :: on EACH of the three targets the base-vs-cut emission delta is EXACTLY the $LEGD_EXPN EMITTABLE file(s) [$(tr '\n' ' ' < "$LEGD_EXPSET")] and ZERO other corpus files.  $LEGD_BLINDN file(s) are STRUCTURALLY BLIND to this leg [$(tr '\n' ' ' < "$LEGD_BLINDSET")] -- golib, hand-own companions, whole-file hand-owns and the hand-owned disclosure manifests are not converter output, so their absence from the diff is a PROPERTY and never a miss"
  legd_stamp "LEG D PREDICTION derivation :: THREE-dot per seat; the TWO-dot forms total $LEGD_TWODOT_TOT file(s) and are the STALE-BASE ILLUSION where a seat forked before this base -- as an expectation that reads MISSED on a correct train, as an exclusion it MASKS real footprint"
  [ "${LEGD_EXPN:-0}" -ge 1 ] || { legd_stamp "  ^ LEG D NOTE: the EMITTABLE prediction set is EMPTY.  That is a legitimate reading -- a converter change may move no corpus byte at all -- and it is a FALSIFIABLE claim: the diff below must then be empty on all three targets.  It is stated rather than silently treated as 'nothing to check'"; }
fi

# ============================================================================================
# D5 -- THE DIFF, PER TARGET, CR-STRIPPED, COMPARED **BY MECHANISM**.
#   THE FLOOR (the run's OWN report artifacts, excluded BY NAME, never by a wildcard):
#     stdlib_conversion_progress.txt  written unconditionally; carries wall times, so it differs
#                                     between two healthy arms by design
#     failed_packages.txt             its PRESENCE is asserted ABOVE as a refusal
#     graphs/ reports/                unconditional generator output
#   NOT excluded, deliberately: go2cs-stdlib.slnx.  It is EMISSION -- a converter change that moved the
#   project set would show there -- and a floor list that swallowed it would hide a real finding.
# ============================================================================================
LEGD_MET_ALL=1; LEGD_VERDICTS=""
if [ "$LEGD_OK" = "1" ]; then
  for goos in $LEGD_TARGETS; do
    A="$LEGD_ROOT/seed/base-$goos"; B="$LEGD_ROOT/seed/cut-$goos"
    DL="$SCRIPT_DIR/coord-${TAG}-legd-diff-$goos-$RUNID.txt"
    DC="$SCRIPT_DIR/coord-${TAG}-legd-diffcontent-$goos-$RUNID.txt"
    diff -qr --strip-trailing-cr \
      -x 'stdlib_conversion_progress.txt' -x 'failed_packages.txt' -x 'graphs' -x 'reports' -x '.legd-sentinel' \
      "$A" "$B" > "$DL" 2>&1
    DIFFN=$(grep -acE '^Files .* differ$' "$DL" || true)
    ONLYN=$(grep -acE '^Only in ' "$DL" || true)
    DIFFSET=$(grep -aE '^Files .* differ$' "$DL" | awk '{print $2}' | sed "s#^$A/##" | sort | tr '\n' ' ')
    legd_stamp "LEG D DIFF $goos :: differingFiles=$DIFFN onlyInOneSide=$ONLYN :: set=[$DIFFSET] (artifact=$DL, CR-stripped, floor files excluded BY NAME)"
    # ⚠ (v) THE DIFF **CONTENT** IS WRITTEN OUT BEFORE ANY PURGE, for EVERY target, MET or MISSED.
    #   A failed arm whose `<`/`>` lines live only under the scratch root is a gate whose cleanup
    #   destroys the artifact it measures, and that reads as a clean sweep.
    : > "$DC"
    grep -aE '^Files .* differ$' "$DL" | awk '{print $2}' | sed "s#^$A/##" | while IFS= read -r rf; do
      [ -n "$rf" ] || continue
      printf '===== %s =====\n' "$rf" >> "$DC"
      diff --strip-trailing-cr "$A/$rf" "$B/$rf" >> "$DC" 2>&1
    done
    legd_stamp "LEG D DIFF $goos content artifact :: $DC ($(grep -ac . "$DC" || true) line(s)) -- written BEFORE the purge, for MET and MISSED alike"
    MET=1; WHY=""
    if [ "${ONLYN:-0}" != "0" ]; then
      MET=0; WHY="$WHY [${ONLYN} path(s) present on ONE side only]"
      legd_stamp "  ^ LEG D $goos: files present on one side only -- a converter that STOPPED or STARTED emitting a file is a footprint the line count cannot see:"
      grep -aE '^Only in ' "$DL" | head -10 | sed 's/^/    /' | tee -a "$ASMLOG"
    fi
    # the SET comparison, both directions
    GOTSET="$LEGD_ROOT/got-$goos.txt"
    grep -aE '^Files .* differ$' "$DL" | awk '{print $2}' | sed "s#^$A/##" | sort -u > "$GOTSET"
    # ⚠ (vi) THE PREDICTION IS FILTERED **PER TARGET**.  (Carried template defect (d), ported from the
    #   train-46 run-8 fix 2026-09-13; train 46 run 7 read a FALSE RED on all THREE targets without it.)
    #   The expected set is the UNION over every target, but a single-target conversion writes ONLY its
    #   own flavour's per-GOOS folder (layout L3), so the OTHER two flavours' `<pkg>/<goos>/...` files
    #   can never differ on this target and must not be counted as "predicted but did not differ".  A
    #   flat file (no per-GOOS segment) is expected on every target.  The unfiltered union count is
    #   STAMPED beside the filtered one, because a filter nobody can see is a filter nobody re-reads.
    EXPGOOS="$LEGD_ROOT/expected-$goos.txt"
    sort -u "$LEGD_EXPSET" | awk -v g="$goos" '{ if ($0 ~ /\/(windows|linux|darwin)\//) { if (index($0, "/" g "/") > 0) print } else print }' > "$EXPGOOS"
    legd_stamp "LEG D $goos prediction FILTERED per target :: $(grep -ac . "$EXPGOOS" || true) expected on this target of $(sort -u "$LEGD_EXPSET" | grep -ac . || true) in the union (files under another flavour's folder cannot be written by this target's conversion)"
    EXTRA=$(comm -13 <(sort -u "$EXPGOOS") "$GOTSET" | grep -ac . || true)
    MISSING=$(comm -23 <(sort -u "$EXPGOOS") "$GOTSET" | grep -ac . || true)
    if [ "${EXTRA:-0}" != "0" ]; then
      MET=0; WHY="$WHY [$EXTRA file(s) differ that the prediction does not name]"
      legd_stamp "  ^ LEG D $goos: files OUTSIDE the prediction:"; comm -13 <(sort -u "$EXPGOOS") "$GOTSET" | head -10 | sed 's/^/    /' | tee -a "$ASMLOG"
    fi
    if [ "${MISSING:-0}" != "0" ]; then
      MET=0; WHY="$WHY [$MISSING predicted file(s) did NOT differ]"
      legd_stamp "  ^ LEG D $goos: predicted files that did NOT move.  ⚠ On a PER-GOOS package this can be legitimate -- a file that exists only under another flavour cannot move here -- so each is NAMED rather than counted:"; comm -23 <(sort -u "$EXPGOOS") "$GOTSET" | head -10 | sed 's/^/    /' | tee -a "$ASMLOG"
    fi
    # --- the PER-FILE, BY-MECHANISM comparison.  ⚠ The embedded helper is quoted-heredoc'd so no
    #     converter glyph passes through a shell pattern, and it compares FILES rather than patterns.
    while IFS= read -r rf; do
      [ -n "$rf" ] || continue
      key=$(printf '%s' "$rf" | tr '/' '_')
      [ -f "$LEGD_PRED/$key.add" ] || continue
      diff --strip-trailing-cr "$A/$rf" "$B/$rf" > "$LEGD_ROOT/hunk-$goos-$key.txt" 2>&1
      grep -aE '^> ' "$LEGD_ROOT/hunk-$goos-$key.txt" | sed 's/^> //' > "$LEGD_ROOT/got-add-$goos-$key.txt"
      grep -aE '^< ' "$LEGD_ROOT/hunk-$goos-$key.txt" | sed 's/^< //' > "$LEGD_ROOT/got-del-$goos-$key.txt"
      MECH=$(python - "$LEGD_PRED/$key.del" "$LEGD_PRED/$key.add" "$LEGD_ROOT/got-del-$goos-$key.txt" "$LEGD_ROOT/got-add-$goos-$key.txt" <<'PYMECH' 2>&1 | tr -d '\r'
import sys
def rd(p):
    try:
        data = open(p, 'rb').read()
    except OSError:
        return None
    if not data:
        return []
    lines = data.decode('utf-8', errors='replace').split(chr(10))
    if lines and lines[-1] == '':
        lines.pop()
    return [l.rstrip(chr(13)) for l in lines]
pdel, padd, gdel, gadd = (rd(a) for a in sys.argv[1:5])
if pdel is None or padd is None or gdel is None or gadd is None:
    print("ERR unreadable"); raise SystemExit(0)
if len(gdel) != len(pdel) or len(gadd) != len(padd):
    print("MISS counts predicted +%d/-%d measured +%d/-%d" % (len(padd), len(pdel), len(gadd), len(gdel))); raise SystemExit(0)
DELIM = set(' \t"\',()[]{};:<>=&|!+*/#')
def token_span(s, i, j):
    while i > 0 and s[i-1] not in DELIM: i -= 1
    while j < len(s) and s[j] not in DELIM: j += 1
    return i, j
def subst(o, n):
    # the MINIMAL differing span, expanded to a TOKEN boundary on both sides
    p = 0
    while p < len(o) and p < len(n) and o[p] == n[p]: p += 1
    so = len(o); sn = len(n)
    while so > p and sn > p and o[so-1] == n[sn-1]: so -= 1; sn -= 1
    if p == so and p == sn: return None
    a, b = token_span(o, p, so)
    # expand n by the SAME left/right amounts so the two tokens correspond
    old_tok = o[a:b]; new_tok = n[a:sn + (b - so)]
    # ⚠ AN EMPTY OLD TOKEN WOULD MAKE str.replace EXPLODE INTO EVERY POSITION.  It cannot arise once
    #   the span is expanded to a token boundary (the boundary walk always yields at least the
    #   differing character), but the guard is cheap and a silent explosion is not.
    if not old_tok: return None
    return (old_tok, new_tok)
if len(pdel) == len(padd) and len(pdel) > 0:
    subs = []
    for o, n in zip(pdel, padd):
        s = subst(o, n)
        if s is None: subs.append(None)
        else: subs.append(s)
    bad = 0; shown = []
    for i, (go_, gn) in enumerate(zip(gdel, gadd)):
        s = subs[i]
        if s is None:
            if go_ != gn: bad += 1; shown.append(i)
            continue
        if go_.replace(s[0], s[1]) != gn:
            bad += 1; shown.append(i)
    if bad == 0:
        print("MET mechanism [" + "; ".join(("%s -> %s" % (s[0], s[1])) if s else "identical" for s in subs) + "]")
    else:
        print("MISS mechanism %d of %d pair(s) are not the predicted token change; first at index %s" % (bad, len(gdel), shown[:3]))
else:
    if gdel == pdel and gadd == padd:
        print("MET exact (unequal add/del counts -- a DISPLACEMENT, so no token pair exists and the comparison is exact)")
    else:
        print("MISS exact (unequal add/del counts; the measured lines are not the predicted lines)")
PYMECH
)
      legd_stamp "LEG D DIFF $goos file $rf :: measured +$(grep -ac . "$LEGD_ROOT/got-add-$goos-$key.txt" || true)/-$(grep -ac . "$LEGD_ROOT/got-del-$goos-$key.txt" || true) against predicted +$(grep -ac . "$LEGD_PRED/$key.add" || true)/-$(grep -ac . "$LEGD_PRED/$key.del" || true) :: $MECH"
      case "$MECH" in
        MET*) : ;;
        *) MET=0; WHY="$WHY [$rf: $MECH]"
           legd_stamp "  ^ measured lines for $rf:"; sed 's/^/    got+ /' "$LEGD_ROOT/got-add-$goos-$key.txt" | head -6 | tee -a "$ASMLOG"; sed 's/^/    got- /' "$LEGD_ROOT/got-del-$goos-$key.txt" | head -6 | tee -a "$ASMLOG" ;;
      esac
    done < "$GOTSET"
    if [ "$MET" = "1" ]; then
      legd_stamp "LEG D $goos **MET** :: the base-vs-cut emission delta is EXACTLY the predicted set, and every changed line is the predicted MECHANISM -- which is invariant under a flavour-specific field list and still refuses any other change"
      LEGD_VERDICTS="$LEGD_VERDICTS $goos=MET"
    else
      legd_stamp "LEG D $goos **MISSED** ::$WHY -- a footprint OUTSIDE the prediction is a HOLD, not a note.  The file list is $DL, the CONTENT is $DC, and the extra paths are named above"
      LEGD_VERDICTS="$LEGD_VERDICTS $goos=MISSED"
      LEGD_MET_ALL=0
    fi
  done
  legd_stamp "LEG D VERDICTS ::$LEGD_VERDICTS"
fi

# ============================================================================================
# D6 -- PURGE, AND THE DISK READING BOTH SIDES OF IT.  ⚠ THE PURGE TAKES THE **SEEDS** AND NOTHING
#   ELSE THAT MATTERS: the per-target file lists, the per-target diff CONTENT and the prediction all
#   live in $SCRIPT_DIR and are the evidence.  A cleanup that removes what the leg measured reads as a
#   clean sweep.
# ============================================================================================
LEGD_FREE1=$(freegb)
if [ -e "$LEGD_ROOT" ]; then
  rm -rf "$LEGD_ROOT" 2>/dev/null
  LEGD_LEFT=$( [ -e "$LEGD_ROOT" ] && echo PRESENT || echo gone )
else
  LEGD_LEFT=gone
fi
LEGD_FREE2=$(freegb)
LEGD_KEPT=$(ls "$SCRIPT_DIR"/coord-${TAG}-legd-diff-*-$RUNID.txt "$SCRIPT_DIR"/coord-${TAG}-legd-diffcontent-*-$RUNID.txt 2>/dev/null | grep -ac . || true)
legd_stamp "LEG D disk :: freeGB before=$LEGD_FREE0 after-the-arms=$LEGD_FREE1 after-the-purge=$LEGD_FREE2 :: scratch root is now $LEGD_LEFT :: per-target artifacts KEPT in \$SCRIPT_DIR = $LEGD_KEPT (must be 6 -- three file lists and three content files; the purge NEVER removes them)"
[ "${LEGD_KEPT:-0}" = "6" ] || { [ "$LEGD_OK" = "1" ] && { legd_stamp "  ^ LEG D FINDING: $LEGD_KEPT of the six per-target artifacts survive the purge.  A gate whose cleanup destroys the artifact it measures reads as a clean sweep, which is the fault this clause exists for"; FAILED=1; }; }
if [ "$LEGD_OK" != "1" ]; then
  legd_stamp "LEG D exit=1 :: **UNMEASURED**.  It does not stop the chain -- LEG 3, LEG 4 and LEG 5 are set-level instruments over a different population -- but this train's converter obligation is NOT discharged and the land script's LEG D requirements will refuse the record"
  FAILED=1
elif [ "$LEGD_MET_ALL" != "1" ]; then
  fail_gate LEG-D-prediction
  chain_stop "LEG D (two-seeded three-target -stdlib emission diff)" "the union's corpus footprint is NOT the predicted one on at least one target ($LEGD_VERDICTS).  A footprint outside the prediction is a HOLD: it means THREE converter seats TOGETHER emit something no seat measured ALONE, which is exactly the composition class the union gate exists to find, and no later leg in this battery can see it." "LEG 3 GolibTests x2 | LEG 4 CNR | LEG 5 FULL behavioral suite | LEG K canary set + sync + nistec | LEG R -tests convert-then-build"
else
  legd_stamp "LEG D exit=0 :: **MET on all three targets**.  G11(b)'s third obligation is DISCHARGED IN THIS BATTERY: the union of three converter seats moves EXACTLY the predicted emittable set on windows, linux and darwin, every changed line by the predicted MECHANISM, and nothing else"
fi
trap 'rm -f "$LOCK"' EXIT
legd_publish
pin_go 1.24.13 "POST-LEG-D" || { stamp "could not restore the 1.24.13 pin after LEG D -- ABORT"; exit 90; }

# ============================================================================================
# LEG R -- THE `-tests` CONVERT-THEN-BUILD OF `reflect` AND `errors` AT THE MERGE RESULT.
#   ⚠ IT IS OWED BECAUSE FOUR SEATS CHANGE THE CONVERTER AND G11(b)'s TRIGGER CENSUS IS NON-EMPTY.
#   The doctrine clause: a converter change touching lift identity, dedup registries or
#   anonymous-type naming owes a `-tests` convert-then-build of `reflect` at the MERGE RESULT beside
#   CNR, and a lift/dedup change owes the same for a row with an EXTERNAL test variant -- `errors`,
#   the cheapest -- because the accessibility half of the dedup guard reasons within ONE assembly and
#   is blind to the external test package.  ⚠ NEITHER ROW IS COMPILED BY ANY STANDING GATE: CNR is
#   transpile-only and the stdlib solution compiles PRODUCTION assemblies, so an unbanked-shaped break
#   in a banked row's TEST emission sits at master with every gate green.
#   ⚠ CONVERT **THEN** BUILD, NEVER A BARE BUILD.  `build` consumes an EXISTING digest-validated
#   manifest and `go2cs_test_manifest.json` is machine-specific and git-ignored, so on a box that has
#   not emitted the row a bare build exits in ~0 s with `test manifest is missing` -- which an
#   error-pattern filter reads as a bare exit=1 and a green-word grep reads as nothing at all.  The
#   SAME tail also says `test manifest is stale: input digest changed` after a converter rebuild, and
#   THAT failure carries an EMPTY error-code histogram, which is the tell: a build failure with no
#   `error (CS|MSB|NETSDK)[0-9]+` line is not a build failure.
#   ⚠ THE TWO-PIN SHAPE: the converter is BUILT at 1.24.13 (master's go.mod requires it) and the
#   PIPELINE RUNS under the 1.23.12 pin, because the corpus and its oracle are 1.23.12 until H5 and
#   `-goroot` does NOT isolate the loader -- the ambient GOROOT leaks into go/packages and 1.23
#   sources type-checked against 1.24's internal/abi produce ~150 errors that read exactly like a
#   corpus break.
#   ⚠ THE GOROOT PACKAGE ARGUMENT IS TAKEN FROM `go env GOROOT` VERBATIM.  A forward-slash spelling
#   fails getProjectName's prefix test, the walk-up finds $GOROOT/src/go.mod (`module std`), and the
#   whole emission lands in `namespace go.std.*` -- exiting 0, with the damage surfacing as CS0117 in
#   CONSUMER packages.  The visible tell is a `std.<pkg>.csproj` beside the committed one, and this
#   leg checks for exactly that afterwards.
#   ⚠ IT DOES NOT STOP THE CHAIN.  A red here does not invalidate LEG 3, LEG 4, LEG 4b or LEG 5 --
#   those are set-level instruments over a different population -- so it sets FAILED and continues.
# ============================================================================================
LEGR_OK=1
if ! pin_go 1.24.13 "LEG-R-build"; then
  stamp "LEG R UNMEASURED :: the 1.24.13 pin could not be established, so the converter cannot be built for it"; FAILED=1; LEGR_OK=0
else
  LEGRB="$SCRIPT_DIR/coord-${TAG}-legr-build-$RUNID.log"
  ( cd src/go2cs && go build -o bin/go2cs.exe . > "$LEGRB" 2>&1 < /dev/null ); lrbrc=$?
  LEGRVER="$(go version "$CONVEXE" 2>&1 | tr -d '\r')"
  stamp "LEG R converter build exit=$lrbrc :: \`go version $CONVEXE\` reads [$LEGRVER] (must carry go1.24.13 -- the binary's own EMBEDDED release is the one reading no cwd and no GOTOOLCHAIN can switch)"
  case "$LEGRVER" in
    *go1.24.13*) : ;;
    *) stamp "  ^ LEG R UNMEASURED: the built converter does not read go1.24.13"; FAILED=1; LEGR_OK=0 ;;
  esac
  [ "$lrbrc" = "0" ] || { stamp "  ^ LEG R UNMEASURED: the converter did not build; head of the log:"; head -12 "$LEGRB" | sed 's/^/    /' | tee -a "$ASMLOG"; FAILED=1; LEGR_OK=0; }
fi
if [ "$LEGR_OK" = "1" ]; then
  if ! pin_go 1.23.12 "LEG-R-run"; then
    stamp "LEG R UNMEASURED :: the 1.23.12 RUN pin could not be established, so the pipeline would type-check 1.23 sources against 1.24's internal/abi and produce ~150 errors that read exactly like a corpus break"
    FAILED=1; LEGR_OK=0
  fi
fi
if [ "$LEGR_OK" = "1" ]; then
  LEGR_GOROOT="$(go env GOROOT 2>/dev/null | tr -d '\r')"
  LEGR_SRCROOT="$(cygpath -wa src 2>/dev/null || printf '%s' "$WT/src")"
  stamp "LEG R environment :: GOROOT=[$LEGR_GOROOT] (taken from \`go env GOROOT\` VERBATIM, backslashes and all -- a forward-slash spelling misroutes the whole emission into namespace go.std.* at exit 0) :: -go2cspath [$LEGR_SRCROOT] (pinned, because a generated csproj otherwise falls back to the machine-global deploy root)"
  for row in reflect errors; do
    LR1="$SCRIPT_DIR/coord-${TAG}-legr-$row-convert-$RUNID.log"
    LR2="$SCRIPT_DIR/coord-${TAG}-legr-$row-build-$RUNID.log"
    t0=$(date +%s)
    "$CONVEXE" -tests -test-action convert -go2cspath "$LEGR_SRCROOT" "${LEGR_GOROOT}\\src\\${row}" "$(cygpath -wa "src/core/$row" 2>/dev/null || printf '%s' "$WT/src/core/$row")" > "$LR1" 2>&1 < /dev/null
    crc=$?
    "$CONVEXE" -tests -test-action build   -go2cspath "$LEGR_SRCROOT" "${LEGR_GOROOT}\\src\\${row}" "$(cygpath -wa "src/core/$row" 2>/dev/null || printf '%s' "$WT/src/core/$row")" > "$LR2" 2>&1 < /dev/null
    brc=$?
    w=$(( $(date +%s) - t0 ))
    tr -d '\r' < "$LR1" > "$LR1.t" && mv "$LR1.t" "$LR1"
    tr -d '\r' < "$LR2" > "$LR2.t" && mv "$LR2.t" "$LR2"
    RCS=$(grep -acE 'error CS[0-9]+' "$LR2" || true); RMSB=$(grep -acE 'error (MSB|NETSDK)[0-9]+' "$LR2" || true)
    RSTALE=$(grep -acE 'test manifest is (stale|missing)' "$LR2" || true)
    RSTD=$(find "src/core/$row" -maxdepth 1 -name 'std.*.csproj' 2>/dev/null | grep -ac . || true)
    stamp "LEG R $row :: convert exit=$crc  build exit=$brc  wall=${w}s :: CS=$RCS MSB/NETSDK=$RMSB (two numbers, NEVER folded) manifestStaleOrMissingLines=$RSTALE stdNamespaceArtifacts=$RSTD (must be 0 -- a std.<pkg>.csproj beside the committed one is the GOROOT-spelling misroute, and it exits 0)"
    if [ "$crc" != "0" ]; then
      stamp "  ^ LEG R $row CONVERT failed -- the converter could not emit this row's tests at the merge result.  Tail:"
      tail -12 "$LR1" | sed 's/^/    /' | tee -a "$ASMLOG"
      fail_gate LEG-R-convert
    fi
    if [ "$brc" != "0" ]; then
      if [ "$RCS" = "0" ] && [ "$RMSB" = "0" ]; then
        stamp "  ^ LEG R $row BUILD exited $brc with an EMPTY error-code histogram, which is the documented tell that it is NOT a build failure (manifestStaleOrMissing=$RSTALE).  Read the tail before calling it a regression:"
      else
        stamp "  ^ LEG R $row BUILD FAILED with CS=$RCS MSB/NETSDK=$RMSB -- this is the class no standing gate sees: a banked row's TEST assembly broken at the merge result by a converter change.  Codes:"
        grep -aE 'error (CS|MSB|NETSDK)[0-9]+' "$LR2" | sed -E 's/.*(error (CS|MSB|NETSDK)[0-9]+).*/\1/' | sort | uniq -c | sort -rn | head -6 | while read -r l; do stamp "      $l"; done
      fi
      tail -12 "$LR2" | sed 's/^/    /' | tee -a "$ASMLOG"
      fail_gate LEG-R-build
    else
      stamp "LEG R $row OK :: the row's test emission CONVERTS and its test assembly BUILDS at the merge result"
    fi
    [ "${RSTD:-0}" = "0" ] || { stamp "  ^ LEG R $row REFUSED: $RSTD std.*.csproj artifact(s) -- the GOROOT spelling misrouted the emission into namespace go.std.*"; FAILED=1; }
  done
  # --- the restore.  ⚠ BY `git checkout` + `git clean`, NEVER BY A GLOB.  `reflect` and `errors` are
  #     BANKED rows, so their *_test.cs are TRACKED; the reflex `rm -f *_test.cs` carried over from an
  #     UNBANKED row deletes committed sources, and the two rows differ in exactly that property.
  #     ⚠ BOTH ROOTS (2026-09-13, run 5): a -tests run also rewrites the row's committed proof page under
  #     docs/validation/current/ and the index -- a corpus-scoped restore leaves them behind and the
  #     unfiltered dirt gate reads them as drift (it did, at LEG U, run 5 09:46).
  git checkout -- src/core docs/validation 2>/dev/null
  git clean -fdq -- src/core 2>/dev/null
  LRDIRTY=$(git status --porcelain | wc -l); LRDEL=$(git status --porcelain | grep -c '^ D')
  stamp "LEG R post-restore :: dirty=$LRDIRTY deleted-tracked=$LRDEL (both must be 0)"
  [ "$LRDIRTY" = "0" ] || { stamp "  ^ LEG R restore did NOT clean the tree -- read the UNFILTERED status:"; git status --porcelain | head -20 | sed 's/^/    /' | tee -a "$ASMLOG"; FAILED=1; }
  [ "$LRDEL" = "0" ] || { stamp "  ^ the restore DELETED TRACKED FILES -- restore them before anything else"; FAILED=1; }
  # --- and RE-ASSERT **THIS TRAIN'S** CORPUS FOOTPRINT AFTER THE PIPELINE PASS.  A `-tests` run
  #     RE-CONVERTS every non-marked corpus file it reaches, so a footprint invariant is re-read after
  #     any pipeline pass rather than assumed to have survived it -- and the restore above is exactly
  #     the step that has silently reverted a hand-applied hunk before.
  #     ⚠ THE SUBJECT IS THE REGISTRY ROW'S DISPLACEMENT, NOT TRAIN 45's `.cs.auto` HUNK.  Train 45's arm re-read
  #     `src/core/runtime/runtime2.cs.auto` for the as-declared [GoValueClone] stamp; that seat LANDED,
  #     so the same read here would assert a fact about master and could not go red about THIS train.
  #     A REGISTRY row's registration DISPLACES a bodied function out of `src/core/runtime/panic.cs`, which is
  #     a NON-marked corpus file the pipeline re-converts -- so it is precisely the footprint this
  #     re-read exists for.
  if [ "${A3IN_DELTA:-0}" != "0" ]; then
    LRPH=$(grep -acE 'GoManualConversion|manual conversion' src/core/runtime/panic.cs 2>/dev/null || true)
    LRNS=$(git diff --numstat "$BASE" HEAD -- src/core/runtime/panic.cs | awk '{printf "+%s/-%s", $1, $2}')
    LRDEL2=$(git diff --numstat "$BASE" HEAD -- src/core/runtime/panic.cs | cut -f2)
    case "${LRDEL2}" in ''|*[!0-9]*) LRDEL2=0 ;; esac
    stamp "LEG R post-restore footprint re-read :: the manual-conversion registry row's displacement in src/core/runtime/panic.cs still reads numstat ${LRNS:-ABSENT} with a DELETION column of $LRDEL2 (must be > 0 -- the registration REMOVES a bodied function's emission, and a +N/-0 after a pipeline pass means the restore put the body back) :: the file mentions a manual-conversion placeholder x$LRPH (a READING)"
    [ "${LRDEL2:-0}" -gt 0 ] || { stamp "  ^ LEG R REFUSED: the pipeline pass moved the registry row's corpus footprint and the restore did not put it back.  Every later leg would be measuring a tree in which the registration landed and the displacement did not -- which is the syscall.Uname silent-subtraction class arriving through a gate's own cleanup."; FAILED=1; }
  else
    stamp "LEG R post-restore footprint re-read :: NOT ASSERTED -- NO merged row in this train carries a corpus DISPLACEMENT (a converter registry entry that removes a bodied function's emission), so there is no footprint for a pipeline pass to have moved and put back.  Stated rather than passed over: 'the arm found nothing' and 'the arm had nothing to look at' are different states."
  fi
fi
pin_go 1.24.13 "POST-LEG-R" || { stamp "could not restore the 1.24.13 pin after LEG R -- ABORT"; exit 90; }

# ============================================================================================
# LEG U -- THE `unicode/utf8` `-tests` ARM, AND IT IS ROW 1's OWN SEAT GATE.
#   ⚠ WHY IT IS A BATTERY LEG AND NOT A CONVERTER TEST.  Row 1 adds platform SCOPING to the disclosure
#   manifest: an entry carrying `"platforms": [...]` is INERT on every other target and LIVE on its
#   own.  `go vet`, LEG C and LEG Cg all type-check and unit-test that logic in isolation; NONE of them
#   runs the pipeline end to end against a real package with a real manifest on disk, and the scope
#   split happens at LOAD, inside the run.  A rule that is only ever exercised by its own unit test is
#   a rule nobody has watched work.
#   ⚠ IT IS A CONTROLLED ARM IN **BOTH DIRECTIONS**, which is the whole point: a foreign-GOOS entry
#   must NOT be absorbed and must be LISTED out of scope (the negative arm), and the SAME entry
#   re-scoped to THIS run's target must be absorbed (the positive arm).  Either arm alone is an
#   assertion; the pair is a measurement.  ⚠ AND THE DIFFERENCE BETWEEN "not absorbed" AND "listed out
#   of scope" IS THE ARM'S CONTENT: an entry silently dropped and an entry deliberately withheld
#   produce the same absorption count, and only the record tells them apart.
#   ⚠ THE TWO-PIN SHAPE IS LEG R's, VERBATIM: the converter was BUILT at 1.24.13 (LEG R did that and
#   read the release back off the binary) and the PIPELINE RUNS at 1.23.12, because the corpus and its
#   oracle are 1.23.12 until H5.
#   ⚠ THE PLANT IS UNTRACKED BY CONSTRUCTION: `src/core/unicode/utf8` carries NO committed disclosure
#   manifest (measured at derive time), so the arm CREATES one and DELETES it.  A plant that overwrote
#   a committed manifest could not be restored byte-identically by definition, so an existing file
#   there is a REFUSAL rather than a backup-and-restore.
#   ⚠ IT DOES NOT STOP THE CHAIN.
# ============================================================================================
LEGU_PKG='unicode/utf8'
LEGU_DIR="src/core/$LEGU_PKG"
LEGU_MAN="$LEGU_DIR/go2cs_test_disclosures.json"
LEGU_CMP="$LEGU_DIR/go2cs_test_comparison.json"
LEGU_OK=1
legu_oos(){ python - "$1" "$2" <<'PY' 2>/dev/null | tr -d '\r'
import json,sys
try: d=json.load(open(sys.argv[1],encoding='utf-8'))
except Exception: print(-1); raise SystemExit
print(sum(1 for e in (d.get('outOfScopeDisclosures') or []) if e.get('name')==sys.argv[2]))
PY
}
legu_run(){ # $1 = a label -> writes the log, echoes "exit=<rc> orphans=<n> named=<n> oos=<n>"
  local lbl="$1" lg rc orph named oos
  lg="$SCRIPT_DIR/coord-${TAG}-legu-$lbl-$RUNID.log"
  "$CONVEXE" -tests -test-action all -test-timeout 10m -go2cspath "$LEGU_SRCROOT" "${LEGU_GOROOT}\\src\\unicode\\utf8" "$(cygpath -wa "$LEGU_DIR" 2>/dev/null || printf '%s' "$WT/$LEGU_DIR")" > "$lg" 2>&1 < /dev/null
  rc=$?
  tr -d '\r' < "$lg" > "$lg.t" && mv "$lg.t" "$lg"
  orph=$(grep -acF -- 'ORPHANED DISCLOSURE' "$lg" || true)
  named=0; [ -n "${LEGU_NAME:-}" ] && named=$(grep -aF -- 'ORPHANED DISCLOSURE' "$lg" | grep -acF -- "$LEGU_NAME" || true)
  oos=$(legu_oos "$LEGU_CMP" "${LEGU_NAME:-<none>}")
  # ⚠ THE LOG PATH IS **DERIVABLE FROM THE LABEL** and is NOT returned through a variable: this
  #   function is called inside $( ), so anything it assigns dies with the subshell and a reader
  #   reaching for it afterwards would get an unset variable under `set -u` -- or, worse, a stale one.
  printf 'exit=%s orphans=%s named=%s oos=%s\n' "$rc" "${orph:-0}" "${named:-0}" "${oos:-x}"
}
legu_log(){ printf '%s/coord-%s-legu-%s-%s.log' "$SCRIPT_DIR" "$TAG" "$1" "$RUNID"; }
# ⚠ **LEG U'S RUN/SKIP KEY IS THE OWED VECTOR, NOT A ROW NUMBER.**  Train 47 keyed this leg to
#   `$SEAT1_SHA` -- that is, to "the first row is the orphan-disclosure / platform-scope seat" -- which
#   is (L13) surviving as EXECUTABLE positional coupling rather than as prose, where the self-check's
#   ARM (a) cannot see it at all.  Under the LANDING shape no row reads PENDING, so that key could
#   never skip: the leg ran on every train and stamped a claim about whichever row happened to merge
#   first.  What this leg actually measures is the CONVERTER's platform-scope rule AT THE MERGE RESULT,
#   so the CONVERTER OWE is what keys it and the stamps say only what was measured.  ⚠ THE LANDING'S
#   THREE LEG U ANCHORS ARE GUARDED BY THE SAME VECTOR FIELD -- an anchor that can never be satisfied
#   is what teaches an operator to reach for the override.
if [ "$OWED_CONV" != "1" ]; then
  stamp "LEG U :: NOT RUN -- no MERGED class owes the converter (OWED_CONV=$OWED_CONV), so the platform-scope rule this leg exercises is not in this train's delta.  STATED rather than skipped silently: a leg that did not run is not a leg that passed."
  LEGU_OK=0
elif [ ! -d "$LEGU_DIR" ]; then
  stamp "LEG U UNMEASURED :: $LEGU_DIR is not in the assembled tree"; fail_gate LEG-U-preflight; LEGU_OK=0
elif [ -e "$LEGU_MAN" ]; then
  stamp "LEG U UNMEASURED :: $LEGU_MAN ALREADY EXISTS.  This arm PLANTS a manifest there and deletes it again; a committed manifest at that path means the plant would overwrite a tracked file and 'restore byte-identical' would be a claim rather than a property -- REFUSING rather than backing it up"
  fail_gate LEG-U-preflight; LEGU_OK=0
elif ! pin_go 1.23.12 "LEG-U-run"; then
  stamp "LEG U UNMEASURED :: the 1.23.12 RUN pin could not be established, so the pipeline would type-check 1.23 sources against 1.24's internal/abi and produce errors that read exactly like a corpus break"
  fail_gate LEG-U-pin; LEGU_OK=0
fi
if [ "$LEGU_OK" = "1" ]; then
  LEGU_GOROOT="$(go env GOROOT 2>/dev/null | tr -d '\r')"
  LEGU_SRCROOT="$(cygpath -wa src 2>/dev/null || printf '%s' "$WT/src")"
  LEGU_THIS="$(go env GOOS 2>/dev/null | tr -d '\r')"
  # the FOREIGN target is DERIVED from this run's own, never spelled: a literal would be wrong on the
  # first box that is not this one, and the arm's whole content is "a target that is NOT this one".
  case "$LEGU_THIS" in
    windows) LEGU_FOREIGN=linux ;;
    linux)   LEGU_FOREIGN=darwin ;;
    darwin)  LEGU_FOREIGN=linux ;;
    *)       LEGU_FOREIGN='' ;;
  esac
  stamp "LEG U environment :: GOROOT=[$LEGU_GOROOT] (VERBATIM from \`go env GOROOT\`) :: -go2cspath [$LEGU_SRCROOT] :: this run's target=[$LEGU_THIS] :: the FOREIGN scope this arm plants=[$LEGU_FOREIGN] (DERIVED from the target, never spelled)"
  if [ -z "$LEGU_FOREIGN" ]; then
    stamp "  ^ LEG U UNMEASURED: \`go env GOOS\` read [$LEGU_THIS], which is none of windows/linux/darwin, so a FOREIGN scope cannot be derived and the negative arm has no meaning"
    fail_gate LEG-U-goos; LEGU_OK=0
  fi
fi
if [ "$LEGU_OK" = "1" ]; then
  LEGU_NAME=''
  # --- ARM 1: the CLEAN run.  No manifest, so zero orphan reports is the only admissible reading, and
  #     the arm also DERIVES the test name the next two arms plant against -- from THIS run's own
  #     comparison record, never from a name written here, because a name that has stopped passing
  #     would make both later arms vacuous in the same direction.
  t0=$(date +%s); LEGU_A1=$(legu_run clean); w=$(( $(date +%s) - t0 ))
  LEGU_NAME=$(python - "$LEGU_CMP" <<'PY' 2>/dev/null | tr -d '\r'
import json,sys
try: d=json.load(open(sys.argv[1],encoding='utf-8'))
except Exception: raise SystemExit
cs=d.get('csharp') or {}
for k in sorted(cs):
    if cs[k]=='pass' and '/' not in k:
        print(k); break
PY
)
  stamp "LEG U ARM 1 (CLEAN, no manifest) :: $LEGU_A1 wall=${w}s :: the passing test this arm DERIVED to plant against=[${LEGU_NAME:-NONE}] (from the run's OWN comparison record; an empty one makes both later arms UNMEASURED rather than green)"
  case "$LEGU_A1" in
    'exit=0 orphans=0'*) : ;;
    *) stamp "  ^ LEG U ARM 1 FINDING: the CLEAN run did not exit 0 with zero orphan reports.  Every later arm compares against this baseline, so a dirty baseline makes them unreadable.  Tail:"; tail -12 "$(legu_log clean)" | sed 's/^/    /' | tee -a "$ASMLOG"; fail_gate LEG-U-clean; LEGU_OK=0 ;;
  esac
  [ -n "$LEGU_NAME" ] || { stamp "  ^ LEG U UNMEASURED: no terminal-pass test name could be read out of $LEGU_CMP, so there is nothing to scope a plant to"; fail_gate LEG-U-name; LEGU_OK=0; }
fi
legu_plant(){ # $1 = the GOOS scope to write
  cat > "$LEGU_MAN" <<JSON
{
  "schemaVersion": 1,
  "disclosures": [
    {
      "name": "$LEGU_NAME",
      "class": "alloc-profile",
      "signature": "planted by LEG U -- go2cs train 48 assembly, deleted before this leg returns",
      "platforms": ["$1"],
      "reason": "PLANTED BY **THIS** ASSEMBLY'S LEG U as a two-direction control over the converter's platform-scoped disclosure entries. It names a test this very run records a terminal PASS for, so an IN-SCOPE copy of it is an orphan by that rule's own predicate and an OUT-OF-SCOPE copy is inert. It is written immediately before one conversion and deleted immediately after; if this file is ever found committed, the leg did not reach its restore."
    }
  ]
}
JSON
}
if [ "$LEGU_OK" = "1" ]; then
  # --- ARM 2 (NEGATIVE): a FOREIGN-GOOS entry must NOT be absorbed, and must be LISTED out of scope.
  legu_plant "$LEGU_FOREIGN"
  t0=$(date +%s); LEGU_A2=$(legu_run foreign); w=$(( $(date +%s) - t0 ))
  stamp "LEG U ARM 2 (NEGATIVE, the entry scoped to [$LEGU_FOREIGN] while this run targets [$LEGU_THIS]) :: $LEGU_A2 wall=${w}s :: EXPECTED named=0 (the entry is NOT absorbed) AND oos=1 (it is LISTED out of scope).  ⚠ THE TWO ARE DIFFERENT CLAIMS: an entry silently dropped by a bad scope test and an entry deliberately withheld produce the SAME absorption count, and only the record's outOfScopeDisclosures list tells them apart"
  case "$LEGU_A2" in
    *'named=0 oos=1') stamp "LEG U ARM 2 MET :: not absorbed, and listed out of scope BY NAME" ;;
    *) stamp "  ^ LEG U ARM 2 FINDING: [$LEGU_A2].  A non-zero named= means a foreign-scoped entry WAS applied on this target (the scope test is inert); an oos= other than 1 means the entry was dropped WITHOUT being published, which is the silently-smaller-manifest shape row 1's own design record forbids."; fail_gate LEG-U-foreign ;;
  esac
  # --- ARM 3 (POSITIVE): the SAME entry re-scoped to THIS target must be absorbed and reported.
  legu_plant "$LEGU_THIS"
  t0=$(date +%s); LEGU_A3=$(legu_run thisgoos); w=$(( $(date +%s) - t0 ))
  stamp "LEG U ARM 3 (POSITIVE, the SAME entry re-scoped to [$LEGU_THIS]) :: $LEGU_A3 wall=${w}s :: EXPECTED named=1 (absorbed, and reported as an ORPHAN because the converted side records a terminal pass for it in this run) AND oos=0 (nothing withheld).  ⚠ ONE AXIS MOVED BETWEEN ARMS 2 AND 3 -- the scope string -- so the difference is attributable to the scope rule and to nothing else"
  case "$LEGU_A3" in
    *'named=1 oos=0') stamp "LEG U ARM 3 MET :: absorbed on this target and reported by name" ;;
    *) stamp "  ^ LEG U ARM 3 FINDING: [$LEGU_A3].  A named=0 here with ARM 2 also reading 0 would mean the entry is inert on EVERY target -- an entry that can never apply, which is the failure mode row 1's loader validation exists to refuse."; fail_gate LEG-U-thisgoos ;;
  esac
  # --- THE RESTORE, AND IT IS ASSERTED RATHER THAN PERFORMED.  A `-tests` run rewrites this package's
  #     committed emission and its records, so the restore is `git checkout` + `git clean` over exactly
  #     that directory -- never a glob -- and BOTH the plant's removal and the tracked tree's byte
  #     identity are read afterwards.
  rm -f "$LEGU_MAN"
  # ⚠ BOTH ROOTS (2026-09-13, run 5's red): ARM 3's absorbed orphan rewrote docs/validation/current/<row>.md
  #   and docs/validation/index.md -- the pipeline's OTHER emission root -- and a package-scoped restore
  #   left the tree dirty=2 with the package byte-identical.  The proof pages are restored with the
  #   package; the unfiltered tree-dirty assertion below is unchanged and is what caught it.
  git checkout -- "$LEGU_DIR" docs/validation 2>/dev/null
  git clean -fdq -- "$LEGU_DIR" 2>/dev/null
  LEGU_PLANTLEFT=0; [ -e "$LEGU_MAN" ] && LEGU_PLANTLEFT=1
  LEGU_PKGDIRTY=$(git status --porcelain -- "$LEGU_DIR" | grep -c . || true)
  LEGU_TREEDIRTY=$(git status --porcelain | grep -c . || true)
  LEGU_DEL=$(git status --porcelain | grep -c '^ D')
  LEGU_BYTES=$(git diff --name-only HEAD -- "$LEGU_DIR" | grep -c . || true)
  stamp "LEG U post-restore :: the planted manifest still present=$LEGU_PLANTLEFT (must be 0) :: files differing from their committed blob under $LEGU_DIR=$LEGU_BYTES (must be 0 -- BYTE-IDENTICAL, which is the claim) :: package dirty=$LEGU_PKGDIRTY tree dirty=$LEGU_TREEDIRTY deleted-tracked=$LEGU_DEL (all must be 0)"
  if [ "$LEGU_PLANTLEFT" != "0" ] || [ "${LEGU_BYTES:-1}" != "0" ] || [ "${LEGU_TREEDIRTY:-1}" != "0" ] || [ "${LEGU_DEL:-1}" != "0" ]; then
    stamp "  ^ LEG U REFUSED: the restore did not return the tree.  Read the UNFILTERED status -- every leg after this one would be measuring a tree carrying this arm's plant or its emission:"
    git status --porcelain | head -20 | sed 's/^/    /' | tee -a "$ASMLOG"
    fail_gate LEG-U-restore
  fi
fi
pin_go 1.24.13 "POST-LEG-U" || { stamp "could not restore the 1.24.13 pin after LEG U -- ABORT"; exit 90; }

# ============================================================================================
# LEG 3: GolibTests at BOTH configurations -- and this is where THIS TRAIN'S OWN ARMS are read.
#   ⚠ THE DECLARED COUNT IS DERIVED FROM THIS TREE, AND THE ADMISSIBLE TOTALS ARE COMPUTED FROM THE
#   CSPROJ, because the cheap layer settles more than the expensive one: a run gives you a number, the
#   arithmetic tells you which numbers are POSSIBLE.  A reported total matching NONE of them is a
#   stronger statement than any single re-run can make.
#   ⚠ **NO LITERAL PREDICTION IS CARRIED INTO THIS TRAIN, AND THAT IS A CHANGE.**  Train 45's header
#   named `declared at master 780`, `seat 7 adds 13`, `declared at the union 793` and
#   `PREDICTED ADMISSIBLE unset 752 · windows 752 · linux 785 · darwin 744` -- every one of them
#   measured at ONE seat's pin, correct that day, and the stale-figure class the moment a GolibTests
#   file grows.  A TEMPLATE cannot name its seats at all, so what is asserted instead is a RELATION
#   whose two sides are not both this tree; the arms this train adds are DERIVED from its own delta:
#   not both this tree:
#       declared(BASE, read from the committed blobs at $BASE) + this train's DELTA == declared(UNION)
#   ⚠ THAT ANCHORING IS THE WHOLE POINT.  A Total-against-declared check once PASSED on a run 42
#   methods short -- not skipped, not fabricated, RUN -- because the declared count and the total were
#   both taken from the same stale checkout.  A self-consistent check needs at least one input
#   anchored to a NAMED REF, and $BASE is it.
#   ⚠ THE COMPILE SET, NOT A RAW GREP.  The csproj's conditional `Compile Remove` groups take files
#   out per `$(GoTargetOS)` flavour, so a raw `[TestMethod]` count over the directory is an UPPER
#   BOUND and a run reporting fewer is COUNT-MATCHED rather than truncated.  The evaluator below knows
#   exactly TWO condition shapes -- the two this csproj carries -- and a THIRD is a REFUSAL, never a
#   silent default: an evaluator that guessed would make the admissible totals wrong in an unknown
#   direction while still printing four of them.
#   ⚠ GolibTests sets NO $(GoTargetOS) (measured: -getProperty:GoTargetOS returns ""), so the ordinary
#   run is the UNSET column and that is the total this leg expects.
#   ⚠ THREE tests RUN at Release and SKIP at Debug -- the GC/pin-liveness class, which self-skips where
#   a non-optimizing frame would root its temporaries and make the assertion unfalsifiable.  A skip
#   delta of exactly 3 is the self-consistency check that BOTH legs ran what they should; a Debug-only
#   reading silently under-measures that class by three.  ⚠ A NEW test that self-skips at one
#   configuration MOVES that delta, so a miss is ADJUDICATED against this train's two new arm files
#   before it is read as a regression.
# ============================================================================================
GTCSPROJ='src/tests/GolibTests/GolibTests.csproj'
DECL=$(grep -rc '\[TestMethod\]' src/tests/GolibTests --include=*.cs | awk -F: '{s+=$2} END {print s+0}')
# --- the compile-set evaluator.  It knows EXACTLY TWO condition shapes, which are the two the csproj
#     carries; a THIRD conditional ItemGroup with Compile Removes is a REFUSAL, never a silent default.
GTMAP="/tmp/${TAG}-gt-removes.txt"
awk '
  /<ItemGroup[[:space:]]+Condition=/ { c=$0; sub(/.*Condition="/,"",c); sub(/".*/,"",c); cond=c; next }
  /<\/ItemGroup>/ { cond=""; next }
  /<Compile[[:space:]]+Remove="/ { f=$0; sub(/.*Remove="/,"",f); sub(/".*/,"",f); if (cond != "") print cond "\t" f }
' "$GTCSPROJ" | tr -d '\r' > "$GTMAP"
GTCONDS=$(cut -f1 "$GTMAP" | sort -u)
GTUNKNOWN=0
while IFS= read -r c <&3; do
  [ -n "$c" ] || continue
  case "$c" in
    *"!= 'linux'"*) : ;;
    *"!= ''"*"!= 'windows'"*) : ;;
    *) GTUNKNOWN=$(( GTUNKNOWN + 1 )); stamp "      UNKNOWN conditional ItemGroup shape: [$c]" ;;
  esac
done 3<<< "$GTCONDS"
gt_removed_methods(){ # $1 = flavour  -> the method count removed from the compile set
  local flav="$1" tot=0 c f n
  while IFS="$(printf '\t')" read -r c f; do
    [ -n "$f" ] || continue
    local applies=0
    case "$c" in
      *"!= 'linux'"*)                 [ "$flav" != "linux" ] && applies=1 ;;
      *"!= ''"*"!= 'windows'"*)       { [ -n "$flav" ] && [ "$flav" != "windows" ]; } && applies=1 ;;
    esac
    [ "$applies" = "1" ] || continue
    n=$(grep -ac '\[TestMethod\]' "src/tests/GolibTests/$f" 2>/dev/null || echo 0)
    tot=$(( tot + ${n:-0} ))
  done < "$GTMAP"
  printf '%s' "$tot"
}
GT_R_UNSET=$(gt_removed_methods '')
GT_R_WIN=$(gt_removed_methods 'windows')
GT_R_LNX=$(gt_removed_methods 'linux')
GT_R_DRW=$(gt_removed_methods 'darwin')
GT_T_UNSET=$(( DECL - GT_R_UNSET )); GT_T_WIN=$(( DECL - GT_R_WIN ))
GT_T_LNX=$(( DECL - GT_R_LNX ));     GT_T_DRW=$(( DECL - GT_R_DRW ))
stamp "LEG 3 GolibTests arithmetic (computed from THIS tree and THIS csproj, no build) :: declared=$DECL :: removed unset=$GT_R_UNSET windows=$GT_R_WIN linux=$GT_R_LNX darwin=$GT_R_DRW :: ADMISSIBLE TOTALS unset=$GT_T_UNSET windows=$GT_T_WIN linux=$GT_T_LNX darwin=$GT_T_DRW :: unknownConditionShapes=$GTUNKNOWN (must be 0).  ⚠ NO LITERAL PREDICTION IS CARRIED HERE.  Train 45's stamp named 793 declared and 752 admissible, both measured at ONE seat's pin -- correct that day and the stale-figure class the moment a GolibTests file grows.  This train has TWO seats adding arms, so the DECLARED count is reconciled against the BASE further down (declared(base) + this train's delta == declared(union)), which is the only form of that check whose two sides are not both this tree."
stamp "LEG 3 ⚠ the ordinary run is the UNSET column, so the expected Total is $GT_T_UNSET -- COMPUTED from this csproj at run time, never a literal.  GolibTests sets no \$(GoTargetOS), so the '!= linux' Remove group APPLIES and the '!= \"\" and != windows' group does NOT (its condition requires GoTargetOS to be non-empty), which is why the ordinary run compiles the windows-only files."
[ "$GTUNKNOWN" = "0" ] || { stamp "  ^ LEG 3 REFUSED: the csproj carries a conditional ItemGroup shape this evaluator does not know, so the admissible totals above are WRONG in an unknown direction.  Read the csproj and extend the evaluator; do not read the totals."; FAILED=1; }
[ "${DECL:-0}" -ge 1 ] || { stamp "  ^ LEG 3 REFUSED: the declared count is 0 -- the derivation, not the suite, is broken"; FAILED=1; }
for cfg in Release Debug; do
  L="$SCRIPT_DIR/coord-${TAG}-golib-$cfg-$RUNID.log"; t0=$(date +%s)
  if [ "$cfg" = "Release" ]; then export DOTNET_TieredCompilation=0; else unset DOTNET_TieredCompilation; fi
  # ⚠ BUILD FIRST, THEN TEST --no-build.  A `dotnet test` that BUILDS raced twice in one night on a
  # spurious CS0234/CS0246 that was gone on --no-build against the build just completed.  The build is
  # explicit here rather than inherited from LEG 2b, which builds DEBUG only: a --no-build Release run
  # behind a Debug solution leg would fail for a reason that has nothing to do with this train.
  dotnet build src/tests/GolibTests/GolibTests.csproj -c $cfg -p:UseSharedCompilation=false < /dev/null > "$L" 2>&1
  brc=$?
  if [ "$brc" != "0" ]; then
    stamp "LEG 3 golib-$cfg BUILD exit=$brc :: CS=$(grep -acE 'error CS[0-9]+' "$L") MSB/NETSDK=$(grep -acE 'error (MSB|NETSDK)[0-9]+' "$L") -- the suite is UNMEASURED at this configuration, never a pass"
    grep -aE 'error (CS|MSB|NETSDK)[0-9]+' "$L" | head -6 | sed 's/^/      /' | tee -a "$ASMLOG"
    fail_gate LEG-3-build
    continue
  fi
  dotnet test src/tests/GolibTests/GolibTests.csproj -c $cfg --no-build < /dev/null >> "$L" 2>&1
  rc=$?; w=$(( $(date +%s) - t0 ))
  tr -d '\r' < "$L" > "$L.t" && mv "$L.t" "$L"
  TOT=$(grep -aoE 'Passed:[[:space:]]*[0-9]+|Failed:[[:space:]]*[0-9]+|Skipped:[[:space:]]*[0-9]+|Total:[[:space:]]*[0-9]+' "$L" | tail -4 | tr '\n' ' ')
  ABORT=$(grep -ac 'Test Run Aborted' "$L")
  stamp "LEG 3 golib-$cfg exit=$rc wall=${w}s :: $TOT :: aborted=$ABORT :: config=${cfg} tiering=$( [ "$cfg" = "Release" ] && echo OFF || echo default )"
  [ "$ABORT" = "0" ] || { stamp "  ^ an ABORTED run is UNMEASURED, never a pass -- and a verdict WORD is not a verdict: an aborted run prints 'Passed!' on its second-to-last line"; FAILED=1; }
  [ $rc -eq 0 ] || FAILED=1
done
unset DOTNET_TieredCompilation
n_of(){ grep -aoE "$1:[[:space:]]*[0-9]+" "$2" | tail -1 | grep -oE '[0-9]+'; }
LR="$SCRIPT_DIR/coord-${TAG}-golib-Release-$RUNID.log"; LD="$SCRIPT_DIR/coord-${TAG}-golib-Debug-$RUNID.log"
SR=$(n_of Skipped "$LR"); SD=$(n_of Skipped "$LD"); TR=$(n_of Total "$LR"); TD=$(n_of Total "$LD"); FR=$(n_of Failed "$LR"); FD=$(n_of Failed "$LD")
stamp "LEG 3 ASSERT skipDelta=$(( ${SD:-0} - ${SR:-0} )) [asserted 3] skips R=${SR:-?} D=${SD:-?} totals R=${TR:-?} D=${TD:-?} [asserted equal, and asserted == an ADMISSIBLE total] failed R=${FR:-?} D=${FD:-?} [asserted 0]"
[ "$(( ${SD:-0} - ${SR:-0} ))" = "3" ] || { stamp "  ^ SKIP DELTA != 3 -- one configuration under-measured the GC/pin-liveness class, which RUNS at Release and SELF-SKIPS at Debug (a non-optimizing frame would root its temporaries and make the assertion unfalsifiable).  ⚠ ADJUDICATE against THIS TRAIN'S OWN NEW ARMS before reading it as a regression: a NEW GolibTests method that self-skips at one configuration MOVES this delta, and the files this train adds or modifies are listed by the delta arm below -- a TEMPLATE cannot name them.  3 is the number measured on a tree with new methods in it; the raw skip counts are stamped above and the DECLARED-count reconciliation says how many arms this train added."; fail_gate LEG-3-skip-delta; }
[ -n "${TR:-}" ] && [ "${TR:-x}" = "${TD:-y}" ] || { stamp "  ^ TOTALS DIFFER or unreadable across configurations"; FAILED=1; }
if [ -n "${TR:-}" ]; then
  case "$TR" in
    "$GT_T_UNSET"|"$GT_T_WIN") stamp "LEG 3 TOTAL is ADMISSIBLE :: $TR matches the unset/windows column ($GT_T_UNSET) -- which is the column GolibTests runs in" ;;
    "$GT_T_LNX") stamp "  ^ LEG 3 REFUSED: the total $TR matches the LINUX column, so this run compiled a flavour it should not have -- GoTargetOS leaked in from the environment"; FAILED=1 ;;
    "$GT_T_DRW") stamp "  ^ LEG 3 REFUSED: the total $TR matches the DARWIN column"; FAILED=1 ;;
    *) stamp "  ^ LEG 3 REFUSED: the total $TR matches NONE of the admissible totals ($GT_T_UNSET / $GT_T_WIN / $GT_T_LNX / $GT_T_DRW).  A self-consistent check needs at least one input anchored to a named ref: both of these come from THIS tree, so a total outside all four means the run measured something else -- a truncated suite, a stale build, or a filter."; FAILED=1 ;;
  esac
fi
[ "${FR:-1}" = "0" ] && [ "${FD:-1}" = "0" ] || { stamp "  ^ GolibTests FAILURES (or unreadable count) -- name them:"; grep -aE '^[[:space:]]*(Failed|Error Message)' "$LR" "$LD" | head -12 | sed 's/^/      /' | tee -a "$ASMLOG"; FAILED=1; }
# --- THIS TRAIN'S OWN GolibTests ARMS.  ⚠ THE COUNTS ARE **DERIVED FROM THE COMPILE SET AT THE
#     UNION**, NEVER FROM A LITERAL.  Train 45's arm here asserted `TokenValueTagRefusalTests=9 and
#     TokenDoorWiredTests=4`, two numbers measured at ONE seat's pin -- correct on the day, and the
#     stale-figure class the moment either file grows.  Whether THIS train adds GolibTests arms at
#     all is the OWED vector's to say and the delta's to measure, so the
#     assertion here is the RELATION between the declared count and the DELTA, both read from this
#     tree:  declared(union) == declared(base) + the methods this train's delta adds.
#     ⚠ AND THE DECLARED COUNT IS THE **COMPILE SET's**, not a raw grep's.  The csproj `Compile Remove`
#     groups take files out per `$(GoTargetOS)` flavour, so a raw `[TestMethod]` count over the
#     directory is an upper bound and a run reporting fewer is COUNT-MATCHED rather than truncated.
#     The evaluator above computed the admissible totals from THIS csproj; this arm reads the DELTA's
#     own contribution and reconciles it against them.
GT_NEWFILES=$(git -c core.quotepath=false diff --name-only --diff-filter=A "$BASE" HEAD -- src/tests/GolibTests | grep -aE '\.cs$' | sort || true)
GT_MODFILES=$(git -c core.quotepath=false diff --name-only --diff-filter=M "$BASE" HEAD -- src/tests/GolibTests | grep -aE '\.cs$' | sort || true)
GT_NEWN=0
for f in $GT_NEWFILES; do n=$(grep -ac '\[TestMethod\]' "$f" 2>/dev/null || echo 0); GT_NEWN=$(( GT_NEWN + ${n:-0} )); stamp "LEG 3 delta arm file (ADDED) :: $f declares $n [TestMethod] arm(s)"; done
GT_MODN=0
for f in $GT_MODFILES; do
  now=$(grep -ac '\[TestMethod\]' "$f" 2>/dev/null || echo 0)
  was=$(git show "$BASE:$f" 2>/dev/null | grep -ac '\[TestMethod\]' || echo 0)
  GT_MODN=$(( GT_MODN + now - was ))
  stamp "LEG 3 delta arm file (MODIFIED) :: $f declares $now [TestMethod] arm(s) where the base had $was (delta $(( now - was )))"
done
GT_DECL_BASE=$(git ls-tree -r --name-only "$BASE" -- src/tests/GolibTests | grep -aE '\.cs$' | while IFS= read -r f; do git show "$BASE:$f" 2>/dev/null | grep -ac '\[TestMethod\]'; done | awk '{s+=$1} END{print s+0}')
GT_DELTA_TOTAL=$(( GT_NEWN + GT_MODN ))
stamp "LEG 3 DECLARED-COUNT RECONCILIATION (derived at the UNION, both sides from this tree) :: declared at the BASE=$GT_DECL_BASE :: this train's delta adds $GT_DELTA_TOTAL arm(s) ($GT_NEWN in ADDED files, $GT_MODN net in MODIFIED ones) :: declared at the UNION=$DECL :: the relation base + delta == union reads $(( GT_DECL_BASE + GT_DELTA_TOTAL )) vs $DECL"
if [ "$(( GT_DECL_BASE + GT_DELTA_TOTAL ))" = "${DECL:-0}" ]; then
  stamp "LEG 3 DECLARED COUNT RECONCILED :: the union's declared count is exactly the base's plus this train's own arms.  ⚠ That is the check a self-consistent count cannot make on its own: the base side is anchored to a NAMED REF ($BASE) rather than to the same tree the union count came from, which is precisely how a Total-against-declared check once passed on a run 42 methods short."
else
  stamp "  ^ LEG 3 FINDING: declared(base $GT_DECL_BASE) + delta($GT_DELTA_TOTAL) = $(( GT_DECL_BASE + GT_DELTA_TOTAL )) but the union declares $DECL.  Either a GolibTests file moved outside this train's delta, or the derivation is broken -- and the ADMISSIBLE TOTALS above were computed from \$DECL, so they are wrong in an unknown direction until this closes."
  FAILED=1
fi
[ "${GT_DELTA_TOTAL:-0}" -ge 1 ] || { stamp "  ^ LEG 3 NOTE: this train's delta adds ZERO GolibTests arms.  ⚠ WHETHER THAT IS A FINDING IS THE **OWED VECTOR's** TO SAY: OWED_GT=$OWED_GT.  A zero with OWED_GT=1 and nothing skipped means a golib-class seat did not land its arms; a zero with OWED_GT=0 is simply what a train without a golib seat reads. skippedPending=$SEATS_SKIPPED"; { [ "$OWED_GT" = "0" ] || [ "$SEATS_SKIPPED" -ge 1 ]; } || fail_gate LEG-3-arms; }
# --- ARM MENTIONS, a READING and never an assertion.  A passing MSTest run names only FAILURES, so
#     ZEROS here are normal and NON-zeros are where to look.
# ⚠ THE ARM NAMES ARE **DERIVED FROM THE DELTA**, never listed.  A carried list names the previous
#   train's arms and reads zero forever; the files this train added or modified are what a reader
#   wants pointed at.  A passing MSTest run names only FAILURES, so ZEROS here are normal.
for arm in $(printf '%s\n%s\n' "$GT_NEWFILES" "$GT_MODFILES" | grep -a . | sed -E 's#.*/##; s#\.cs$##' | sort -u); do
  nr=$(grep -ac "$arm" "$LR" || true); nd=$(grep -ac "$arm" "$LD" || true)
  [ "${nr:-0}" = "0" ] && [ "${nd:-0}" = "0" ] && continue
  stamp "LEG 3 this train's arm MENTIONED IN A LOG :: $arm Release=$nr Debug=$nd -- a passing run names only failures, so this is where to look"
done
# ============================================================================================
# LEG 4 -- CNR UNDER THE PAIRING.  ⚠ EXPECTATION E1', AND IT IS **CHANGED == 0**.
#   ⚠ THIS LEG'S EXPECTATION HAS MOVED TWICE AND BOTH MOVES ARE ON THE RECORD.  Two forms ago it
#   expected exit 0 / CHANGED 0 / NOT MEASURED 0 on the premise that seat 4 was G's H9 re-baseline;
#   that premise died with COORD a388443ac's withdrawal of H9.  One form ago -- every behavioral leg at
#   the 1.24.13 pin per the FOURTH arm -- it expected exit 1 with CHANGED == exactly EIGHT projects,
#   35/35, every line the Δ-drop pair, because the converter drops the Δ off the `runtime` package
#   alias when the LOADER runs at 1.24.13 (i9 0858372b5).  THE ALIAS-DEFECT SEAT'S FIFTH ARM retires that from a
#   battery in as many words -- "the 1.24.13-pinned CNR stays as the alias defect's OWN instrument ...
#   NEVER AS A BATTERY LEG" -- so the leg runs under the PAIRING and expects ZERO DRIFT:
#       exit 0                        CNR exits 0 only when nothing changed AND nothing was unmeasured
#       CHANGED == 0                  read TWICE: the git numstat over src/tests/Behavioral (0 files)
#                                     and CNR's OWN printed CHANGED list (absent).  ⚠ Those are TWO
#                                     READINGS OF ONE COMPARISON, not two instruments -- CNR builds its
#                                     list from git status -- and that is stated rather than claimed as
#                                     corroboration.
#       NOT MEASURED == 0             never a pass in either direction; a best-effort conversion is the
#                                     route-#4 shape and is a FINDING
#       skip line names SIX           `==> SKIPPED (platform-exclusive, 6)`.  Derived at the tree, not
#                                     remembered: 15 [GoPlatformExclusive] markers, 9 windows + 6 linux,
#                                     so 728 - 6 = 722 measurable -- the figure i9's ef05467a3 read
#                                     under this same pairing.
#       advisory warnings == 2 + N    from the verdict line's own `(N advisory converter warnings)`
#                                     parenthetical, judged against an expectation DERIVED from the tree:
#                                     the two `unsafe.Sizeof` const-context lines (i9 e458b952f) plus one
#                                     line per measurable LIBRARY package without a license (run 6 -> 7).
#   ⚠ ANY CHANGED MEMBER IS A FINDING.  There is no absorbed set here, and that is the difference the
#   fifth arm makes.  ONE finding has its own NAME because it is the one a broken pairing produces:
#   **exit 1 with the CHANGED set == the EIGHT, 35/35, every line the Δ-drop pair** means THE PAIRING
#   DID NOT HOLD AND THIS LEG RAN AT THE 1.24.13 PIN.  Train 43's pair-check logic is KEPT below for
#   exactly that diagnosis -- the SET, the TOTALS, the PER-PROJECT PAIRS and the LINE PAIRS -- and is
#   NOT expected to fire.  A CHANGED set that is neither empty nor the eight is a finding about a SEAT.
#   THE EIGHT, SPELLED HERE FOR A READER WHO LANDS AT THIS LEG RATHER THAN AT THE HEADER -- the CODE
#   below reads the single GOLDEN_PROJECTS definition and never a retyped copy of it:
#       FuncForPCName  FuncLiteralCallerNames  GoexitDefers  GoroutineWaitState
#       IterPullRendezvous  RuntimeCallerFrames  SetFinalizerBridge  SyscallKeystonePulls
#   ⚠ THIS TRAIN'S CONVERTER OWE IS **DERIVED**, NOT ASSUMED ZERO.  The template's line here read
#   "NO SEAT TOUCHES src/go2cs OR src/gen", which was a fact about train 46; five of this train's rows
#   change src/go2cs and none changes src/gen, so any drift at all is
#   attributable by class rather than drift to absorb.
#   ⚠ AND THE AFTER-GUARD IS PART OF THE EXPECTATION, NOT A DECORATION.  CNR re-transpiles
#   UNCONDITIONALLY and rebuilds go2cs.exe from disk -- which is what makes it immune to false-green
#   routes #1, #2 and #4, and why LEG K can reuse the binary afterwards.  Under the pairing that
#   rebuild must SWITCH UP: `go version src/go2cs/bin/go2cs.exe` must read go1.24.13, because the
#   binary's embedded stamp is the one reading no cwd and no GOTOOLCHAIN can change.  A binary reading
#   anything else marks this leg UNMEASURED -- the pairing's claim is build-up / load-down, and this is
#   the only place the build half is visible.
# ============================================================================================
pin_go_pairing "LEG-4" || { stamp "LEG 4 UNMEASURED :: the two-pin pairing could not be established, so CNR would transpile under an environment nobody ruled on -- and a byte-identical verdict taken there would be a decoration"; FAILED=1; }
stamp "LEG 4 CNR starting under the PAIRING (budget ~1050-1750 s on this box class; EXPECTED exit 0 with CHANGED == 0, NOT MEASURED 0, the skip line naming 6 and the advisory warning count equal to the expectation DERIVED from the tree (2 + the library packages without a license) -- E1')"
L4="$SCRIPT_DIR/coord-${TAG}-cnr-$RUNID.log"; t0=$(date +%s)
powershell -NoProfile -ExecutionPolicy Bypass -File src/tests/Behavioral/check-no-regression.ps1 > "$L4" 2>&1
rc=$?; w=$(( $(date +%s) - t0 ))
tr -d '\r' < "$L4" > "$L4.t" && mv "$L4.t" "$L4"

# NOT MEASURED is read from CNR's own summary LINE, not from any occurrence of the phrase: the words
# appear in the script's own prose and would be found by a substring count.
L4NM=$(sed -nE 's/^==> NOT MEASURED \(([0-9]+)\).*/\1/p' "$L4" | tail -1)
[ -n "${L4NM:-}" ] || L4NM=0
# The CHANGED file set, read from GIT -- the SAME comparison CNR itself uses, so this is one
# derivation with two readings and NOT an independent corroboration.  Stated rather than claimed.
git diff --numstat -- src/tests/Behavioral > "/tmp/${TAG}-l4-numstat.txt"
L4FILES=$(grep -ac . "/tmp/${TAG}-l4-numstat.txt" || true)
L4ADD=$(awk '{a+=$1} END{print a+0}' "/tmp/${TAG}-l4-numstat.txt")
L4DEL=$(awk '{d+=$2} END{print d+0}' "/tmp/${TAG}-l4-numstat.txt")
L4UNBAL=$(awk '$1!=$2 {c++} END{print c+0}' "/tmp/${TAG}-l4-numstat.txt")
awk '{print $3}' "/tmp/${TAG}-l4-numstat.txt" | awk -F/ '{print $4}' | sort -u > "/tmp/${TAG}-l4-projects.txt"
printf '%s\n' "$GOLDEN_PROJECTS" | sort > "/tmp/${TAG}-l4-want.txt"
L4MISSING=$(comm -23 "/tmp/${TAG}-l4-want.txt" "/tmp/${TAG}-l4-projects.txt" | tr '\n' ' ')
L4EXTRA=$(comm -13 "/tmp/${TAG}-l4-want.txt" "/tmp/${TAG}-l4-projects.txt" | tr '\n' ' ')
L4PROJ=$(tr '\n' ' ' < "/tmp/${TAG}-l4-projects.txt")
L4CNRLIST=$(awk '/^==> CHANGED converter output/{f=1;next} /^==>/{f=0} f&&NF{c++} END{print c+0}' "$L4")
# --- THE VERDICT LINE'S OWN THREE NUMBERS.  They are read from the LINE, never from a phrase count:
#     "==> NO REGRESSION: ... byte-identical across all <N> behavioral packages (<A> advisory converter
#      warnings) (<K> platform-exclusive skipped: <names>)."
L4VERDLINE=$(grep -a '^==> NO REGRESSION:' "$L4" | tail -1)
L4MEAS=$(printf '%s' "$L4VERDLINE" | sed -nE 's/.*across all ([0-9]+) behavioral packages.*/\1/p')
L4ADV=$(printf '%s' "$L4VERDLINE"  | sed -nE 's/.*\(([0-9]+) advisory converter warnings\).*/\1/p')
L4SKIPNOTE=$(printf '%s' "$L4VERDLINE" | sed -nE 's/.*\(([0-9]+) platform-exclusive skipped:.*/\1/p')
L4SKIP=$(sed -nE 's/^==> SKIPPED \(platform-exclusive, ([0-9]+)\).*/\1/p' "$L4" | tail -1)
stamp "LEG 4 CNR exit=$rc wall=${w}s :: changedFiles(git)=$L4FILES changedLines(CNR's own printed list)=$L4CNRLIST notMeasured=$L4NM added=$L4ADD removed=$L4DEL filesWhereAdded!=Removed=$L4UNBAL"
stamp "LEG 4 CNR verdict line :: $(grep -aiE 'byte-identical|NO REGRESSION|CHANGED converter output|ALIAS-DRIFT CHECK' "$L4" | tail -2 | tr '\n' ' ' | cut -c1-220)"
stamp "LEG 4 verdict-line numbers :: measurablePackages=${L4MEAS:-UNREADABLE} (722 at derive time: 728 packages minus the 6 foreign platform-exclusives) advisoryConverterWarnings=${L4ADV:-UNREADABLE} (judged below against the expectation DERIVED from the tree, never against a carried baseline) skipNoteOnTheVerdictLine=${L4SKIPNOTE:-ABSENT} skipLine=${L4SKIP:-ABSENT} (both must read 6)"
stamp "LEG 4 changed projects (derived from the changed paths): [$L4PROJ]"
case "$rc" in
  0) stamp "LEG 4 exit 0 is the EXPECTED reading (E1') :: CNR exits 0 only when nothing changed AND nothing was unmeasured, which under the two-pin pairing is the fifth arm's ZERO DRIFT.  The COUNTS below are what decide whether it is only that." ;;
  1) stamp "  ^ LEG 4 FINDING: CNR exited 1, its DRIFT signal.  Under the pairing the expectation is ZERO drift, so something moved -- the DIAGNOSIS block below decides whether it is the pairing failing (the eight, 35/35, every line the Δ-drop pair) or a SEAT."; FAILED=1 ;;
  *) stamp "  ^ LEG 4 REFUSED: CNR exited $rc, which is neither clean (0) nor its drift signal (1)"; FAILED=1 ;;
esac
[ "${L4NM:-0}" = "0" ] || { stamp "  ^ LEG 4 REFUSED: NOT MEASURED is $L4NM, and NOT MEASURED is never a pass.  A best-effort conversion is the route-#4 shape and is a FINDING, not drift."; FAILED=1; }
# --- E1' ITSELF: CHANGED == 0, read twice; the skip line; the advisory count -------------------
[ "${L4FILES:-1}" = "0" ] || { stamp "  ^ LEG 4 FINDING: $L4FILES behavioral file(s) CHANGED where E1' expects ZERO.  Under the pairing the emission must be byte-identical to the committed tree (i9 ef05467a3 read 722 byte-identical / 0 NOT MEASURED).  The diagnosis below names which kind of finding this is."; FAILED=1; }
[ "${L4CNRLIST:-1}" = "0" ] || { stamp "  ^ LEG 4 FINDING: CNR's OWN printed CHANGED list carries $L4CNRLIST line(s).  This is the SECOND READING of the same comparison, not an independent one, and it is here so a disagreement between it and the git numstat is visible -- a disagreement would mean one of the two is not reading what it claims."; FAILED=1; }
if [ -z "$L4VERDLINE" ]; then
  stamp "  ^ LEG 4 UNMEASURED: the '==> NO REGRESSION:' verdict line is ABSENT, so the measurable count, the advisory count and the skip note cannot be read at all.  On a clean run CNR prints exactly that line; its absence is either drift (which the counts above name) or a truncated log, and neither is a pass."
  FAILED=1
else
  [ "${L4MEAS:-0}" -ge 1 ] 2>/dev/null || { stamp "  ^ LEG 4 UNMEASURED: the verdict line carries no measurable-package count"; FAILED=1; }
  # --- THE ADVISORY COUNT IS COMPARED AGAINST AN EXPECTATION DERIVED FROM THE TREE, NEVER A BARE BASELINE (2026-09-13, run 6 -> run 7).
  #     CNR prints only the TOTAL of the converter's WARNING lines; it does not retain them.  Run 6 read 52 against the
  #     train-46 baseline of 2, stamped the count 'counted, never fatal', and then set FAILED=1 -- a record reading
  #     overallFailed=1 with every leg green and no attributable refusal.  The lines were CAPTURED once, standalone, on
  #     this tree (2026-09-13, a standalone CNR on the run-6 worktree at union tree 161af6c44 with the WARNING lines retained by a planted Add-Content line, the script restored byte-identical after; scratch cnr-capture-20260913.log and cnr-advisory-lines.txt; the total CNR itself printed 52; 51 warned packages, every one already a library without a license on master bd1d26faf (the rule inputs, not the emission); the tree predictor below reads EXACT against the capture; i9 read the same total independently on the union tree at 7ede39d6).
  #     KINDS OF RECORD at that capture (kind|count|owed-by): unsafe-sizeof-const|2|base: two unsafe.Sizeof in const context in UnsafeOperations (i9 e458b952f), the train-46 baseline; license-unspecified|50|base: licensing landing 1800b04f8 (master 2026-09-11 14:34, after train 46 measured its baseline at 44f858717) -- licensing.go warnUnspecifiedLicense, once per behavioral module without a LICENSE; no train-48 seat touches licensing.go, a LICENSE, a csproj or a go.mod under src/tests/Behavioral
  #     -- total 52.
  #     MECHANISM (read at the writer, not inferred): projectFileWriter.go:497 resolves the license marker through
  #     licenseConvertedProject ONLY for a Library output; licensing.go:196-217 warns when no LICENSE sits beside the
  #     project and the module ships none; the once-per-module dedupe is a PROCESS-global sync.Map and CNR runs one
  #     converter process per project (check-no-regression.ps1:249), so the bound is per invocation -- one line per
  #     measurable LIBRARY package without a license.  The predictor below re-derives that set from the TREE and the
  #     CNR log's own skip list, so a train that adds or licenses a library package moves the expectation with it.
  #     On the run-6 tree the prediction (50) read EXACT against the capture, name for name; C2 confirmed the site
  #     from a second box (d0807ee31).  Retaining the lines by kind is CNR's job and is a train-48 seat.
  L4ADV_PRED="/tmp/${TAG}-l4-advisory-predicted.txt"
  L4ADV_LIB=$(python - "$WT/src/tests/Behavioral" "$L4" "$L4ADV_PRED" <<'PY' 2>/dev/null | tr -d '\r'
import os, re, io, sys
root, log, out = sys.argv[1], sys.argv[2], sys.argv[3]
txt = io.open(log, encoding='utf-8', errors='replace').read().replace('\r', '')
skipped = set()
m = re.search(r'^==> SKIPPED \(platform-exclusive, (\d+)\).*?\n((?:    .*\n)+)', txt, re.M)
if m:
    for l in m.group(2).split('\n'):
        l = l.strip()
        if l:
            skipped.add(l.split(' ')[0])
pkg = re.compile(r'^package\s+([A-Za-z_][A-Za-z0-9_]*)', re.M)
lic = {'license', 'license.txt', 'license.md', 'licence', 'copying', 'copying.txt'}
pred = []
for dp, dns, fns in os.walk(root):
    dns[:] = [d for d in dns if d not in ('bin', 'obj') and not d.startswith('.')]
    gos = [f for f in fns if f.endswith('.go') and not f.endswith('_test.go')]
    if not gos:
        continue
    rel = os.path.relpath(dp, root).replace('\\', '/')
    if rel == '.' or rel.split('/')[0] in skipped:
        continue
    names = set()
    for f in gos:
        mm = pkg.search(io.open(os.path.join(dp, f), encoding='utf-8-sig', errors='replace').read())
        if mm:
            names.add(mm.group(1))
    if 'main' in names or any(f.lower() in lic for f in fns):
        continue
    d = dp
    modlic = False
    while True:
        if os.path.isfile(os.path.join(d, 'go.mod')):
            modlic = any(f.lower() in lic for f in os.listdir(d))
            break
        q = os.path.dirname(d)
        if q == d or os.path.normcase(d) == os.path.normcase(root):
            break
        d = q
    if modlic:
        continue
    pred.append(rel)
io.open(out, 'w', encoding='utf-8', newline='\n').write('\n'.join(sorted(pred)) + '\n')
print(len(pred))
PY
)
  case "$L4ADV_LIB" in
    ''|*[!0-9]*) stamp "  ^ LEG 4 REFUSED: the advisory predictor is UNREADABLE [${L4ADV_LIB:-empty}] -- python or the CNR log's skip list is not where the arm expects"; fail_gate 'LEG-4-advisory-predictor'; L4ADV_LIB=0 ;;
  esac
  L4ADV_EXPECT=$((2 + L4ADV_LIB))
  stamp "LEG 4 ADVISORY EXPECTATION DERIVED FROM THE TREE :: 2 (unsafe.Sizeof in const context, UnsafeOperations -- the baseline CARRIED IN from the preceding train, first measured at train 46 by i9 e458b952f) + $L4ADV_LIB library package(s) without a license among the measurable set (Library outputs only, one converter process per project) = $L4ADV_EXPECT; the predicted list is at $L4ADV_PRED"
  if [ "${L4ADV:-x}" = "$L4ADV_EXPECT" ]; then
    stamp "LEG 4 ADVISORY WARNINGS :: $L4ADV == the DERIVED expectation $L4ADV_EXPECT.  Counted, never fatal at this reading."
  else
    stamp "  ^ LEG 4 REFUSED: the advisory converter warning count reads [${L4ADV:-UNREADABLE}] where the DERIVED expectation is $L4ADV_EXPECT -- a kind nobody has classified, or a classified kind whose count moved.  CNR retains no lines; capture them standalone (a planted retention line, restored byte-identical after) and classify before landing."
    fail_gate 'LEG-4-advisory'
  fi
fi
if [ "${L4SKIP:-x}" = "6" ] && [ "${L4SKIPNOTE:-x}" = "6" ]; then
  stamp "LEG 4 PLATFORM-EXCLUSIVE SKIP :: the skip line and the verdict line's own note BOTH read 6 -- the six linux-native guards this Windows host cannot measure.  CNR restates the count in two places for exactly this reason, so a disagreement between them would be the finding."
  stamp "LEG 4 skipped by name :: $(grep -aA7 '^==> SKIPPED (platform-exclusive' "$L4" | grep -aE '^    [A-Za-z]' | tr -s ' ' | tr '\n' ' ' | cut -c1-220)"
else
  stamp "  ^ LEG 4 FINDING: the platform-exclusive skip reads [${L4SKIP:-ABSENT}] on its own line and [${L4SKIPNOTE:-ABSENT}] in the verdict line's note, where BOTH must be 6.  A count that moved means the [GoPlatformExclusive] population moved, and F8's whole point is that the skipped names are stated rather than folded into a total."
  FAILED=1
fi
# ============================================================================================
# THE DIAGNOSIS -- TRAIN 43's PAIR-CHECK LOGIC, KEPT AND **NOT EXPECTED TO FIRE**.
#   It runs ONLY when something moved.  Its job is to separate the ONE finding this configuration
#   change can produce -- the pairing not holding, i.e. the leg running at the 1.24.13 pin -- from a
#   finding about a SEAT.  It is not an expectation and it absorbs nothing: whichever branch it takes,
#   FAILED is already 1 by the time it runs.
# ============================================================================================
if [ "${L4FILES:-0}" != "0" ]; then
  stamp "LEG 4 DIAGNOSIS (drift is present, so the diagnosis runs) :: changed projects=[$L4PROJ]"
  git diff -U0 -- src/tests/Behavioral | tr -d '\r' > "/tmp/${TAG}-l4-diff.txt"
  L4RM=$(grep -acE '^-[^-]' "/tmp/${TAG}-l4-diff.txt" || true)
  L4AD=$(grep -acE '^\+[^+]' "/tmp/${TAG}-l4-diff.txt" || true)
  L4RM_OK=$(grep -aE '^-[^-]' "/tmp/${TAG}-l4-diff.txt" | sed 's/^-//' | sed 's/^[[:space:]]*//; s/[[:space:]]*$//' | grep -acFx "$ALIAS_OLD" || true)
  L4AD_OK=$(grep -aE '^\+[^+]' "/tmp/${TAG}-l4-diff.txt" | sed 's/^+//' | sed 's/^[[:space:]]*//; s/[[:space:]]*$//' | grep -acFx "$ALIAS_NEW" || true)
  stamp "LEG 4 DIAGNOSIS totals :: added=$L4ADD removed=$L4DEL filesWhereAdded!=Removed=$L4UNBAL :: removedLines=$L4RM of which the OLD alias spelling=$L4RM_OK :: addedLines=$L4AD of which the NEW alias spelling=$L4AD_OK :: overlap with the eight alias-carrying projects -- missing=[$L4MISSING] extra=[$L4EXTRA] (a READING that locates the drift, NOT a named diagnosis)"
  # ⚠ TRAIN 45's **NAMED** DIAGNOSIS IS RETIRED HERE (2026-09-08), AND THE RETIREMENT IS THE POINT.
  #   That branch read a drift of "exactly the EIGHT, 35/35 balanced per file, the per-project pairs
  #   2/3/2/3/2/15/6/2, every changed line one of the two alias spellings" and NAMED it: **the pairing
  #   did not hold, the leg ran at the 1.24.13 pin**.  It cannot fire on this base.  Train 45 seat 1's
  #   alias fold (`claude/g-h5-alias-corpus-closure`) makes `computeImportAliasRenames` read the CORPUS
  #   csproj closure per GOOS, so the emission is PIN-INDEPENDENT and a failed pairing no longer drops
  #   the Δ.  A branch that cannot be taken is the warm-design trap, and the honest replacement is not
  #   a weaker branch but NO branch: every drift here is now a finding about a SEAT, and the per-seat
  #   converter counts below are what point at one.
  #   ⚠ WHAT IS LOST, SAID OUT LOUD: this leg no longer distinguishes an ENVIRONMENT failure from a
  #   SEAT failure by the drift's shape.  The PAIRING stamps and the after-guard are what answer that
  #   question now, and they are read first.
  # ⚠ THE PER-SEAT CONVERTER COUNT, DERIVED FROM THE SEAT TABLE AT RUN TIME.  Train 45's own stamp
  #   here once said "no seat in this train touches src/go2cs or src/gen", which was FALSE TWICE OVER
  #   and pointed the reader AWAY from the cause -- the drift was a seat's own converter rule reaching
  #   a golden its author had not re-baselined.  THREE seats change converter source in this train, so
  #   the stamp prints WHICH and BY HOW MUCH, with the THREE-DOT form (the two-dot form against a moved
  #   base reports master's own newer content REVERSED) and with `_test.go` excluded, because
  #   `go build` excludes it from the binary and it cannot change an emission.
  L4SEATCONV=""
  while IFS='|' read -r sn sref ssha scls smode <&3; do
    [ -n "$sn" ] || continue
    [ "$ssha" = "PENDING" ] && { L4SEATCONV="$L4SEATCONV seat$sn=PENDING"; continue; }
    c=$(git diff --name-only "$BASE...$ssha" -- 'src/go2cs/*.go' < /dev/null 2>/dev/null | grep -av '_test\.go' | grep -ac . || true)
    L4SEATCONV="$L4SEATCONV seat$sn=${c:-?}"
  done 3<<< "$SEAT_TABLE"
  stamp "  ^^ LEG 4 FINDING :: E1' expects ZERO drift under the pairing and this leg measured some, so it is a finding about a SEAT.  Converter files each seat touches (src/go2cs/*.go, _test.go excluded, three-dot from $BASE, DERIVED from the seat table at run time):$L4SEATCONV -- a seat with a NON-ZERO count is where to look first, and this train has three.  ⚠ Read the PAIRING stamps and the after-guard BEFORE attributing to a seat: an environment failure and a seat failure are different questions and this leg's shape no longer separates them.  The changed projects are [$L4PROJ]; the lines that are neither alias spelling:"
  { grep -aE '^-[^-]' "/tmp/${TAG}-l4-diff.txt" | sed 's/^-//' | sed 's/^[[:space:]]*//; s/[[:space:]]*$//' | grep -avFx "$ALIAS_OLD"
    grep -aE '^\+[^+]' "/tmp/${TAG}-l4-diff.txt" | sed 's/^+//' | sed 's/^[[:space:]]*//; s/[[:space:]]*$//' | grep -avFx "$ALIAS_NEW"; } | head -20 | cut -c1-180 | sed 's/^/    /' | tee -a "$ASMLOG"
  rm -f "/tmp/${TAG}-l4-diff.txt"
else
  stamp "LEG 4 DIAGNOSIS :: NOT RUN -- there is no drift to diagnose, which is E1'.  Train 43's pair-check logic is kept above the restore for the case where the pairing fails; it is not an expectation and it did not fire."
fi
if [ "$rc" = "0" ] && [ "${L4FILES:-1}" = "0" ] && [ "${L4CNRLIST:-1}" = "0" ] && [ "${L4NM:-1}" = "0" ] \
   && [ "${L4SKIP:-x}" = "6" ] && [ "${L4ADV:-x}" = "${L4ADV_EXPECT:-UNDERIVED}" ]; then
  stamp "LEG 4 E1' MET :: exit 0, CHANGED == 0 on BOTH readings, NOT MEASURED 0, the skip line naming 6, the advisory count ${L4ADV:-UNREADABLE} equal to the expectation DERIVED from the tree (${L4ADV_EXPECT:-UNDERIVED}).  The two-pin pairing produced a byte-identical emission across ${L4MEAS:-?} measurable behavioral packages -- the fifth arm's zero-drift expectation, measured here rather than quoted."
fi
rm -f "/tmp/${TAG}-l4-want.txt"

# RESTORE.  deleted-tracked must be 0: a cleanup that deletes tracked files is the class this
# repository has paid for three times, and after a -tests or transpile run a package directory holds
# THREE populations (tracked corpus files, tracked hand-owns, untracked emission).
git checkout -- src/tests/Behavioral 2>/dev/null
git clean -fdq -- src/tests/Behavioral 2>/dev/null
L4DIRTY=$(git status --porcelain | wc -l)
L4DELTRK=$(git status --porcelain | grep -c '^ D')
stamp "LEG 4 post-CNR restore :: dirty=$L4DIRTY deleted-tracked=$L4DELTRK (both must be 0)"
[ "$L4DIRTY" = "0" ] || { stamp "  ^ LEG 4 restore did NOT clean the tree -- read the UNFILTERED status before anything else:"; git status --porcelain | head -20 | sed 's/^/    /' | tee -a "$ASMLOG"; FAILED=1; }
[ "$L4DELTRK" = "0" ] || { stamp "  ^ the restore DELETED TRACKED FILES -- restore them before anything else"; FAILED=1; }
rm -f "/tmp/${TAG}-l4-numstat.txt" "/tmp/${TAG}-l4-projects.txt"

# --- THE AFTER-GUARD.  ⚠ IT IS AN ASSERTION HERE, NOT A STAMP, AND IT IS HALF OF E1'.
#     The pairing's claim is BUILD-UP / LOAD-DOWN: the module graph switches only the converter's build
#     to 1.24.13 while every behavioral package loads at 1.23.12.  The LOAD half is what CNR's
#     byte-identical verdict measures; the BUILD half is visible ONLY in the binary's own embedded
#     release, which no cwd and no GOTOOLCHAIN can switch.  A byte-identical CNR under a binary built
#     at 1.23.12 would be a different experiment wearing the same green.
#     It doubles as the record of the binary LEG K will reuse -- stated once, beside the leg that built
#     it, rather than re-derived in LEG K from a different comparison and called corroboration.
if after_guard_converter "LEG-4" assert; then
  CONVOK=1
else
  stamp "  ^ LEG 4 UNMEASURED (after-guard) :: the BUILD half of the pairing cannot be confirmed, so this leg's byte-identical reading is not the experiment the fifth arm names -- and LEG K, which reuses this exact binary under -SkipBuild, has nothing it can trust either."
  CONVOK=0
  fail_gate LEG-4-after-guard
fi

# ============================================================================================
# LEG 5 -- THE FULL BEHAVIORAL SUITE, ALL PHASES, UNDER THE PAIRING.  ⚠ EXPECTATION E2', AND IT IS
# **ALL GREEN**.
# ROUTE #7's BEHAVIORAL TWIN, and the ONLY leg that can see this train's central seat go wrong: a
# golib change that alters RUNTIME behaviour while emitting byte-identical .cs is invisible to every
# compile gate, and the Output phase is the point.
#   ⚠ THE EXPECTATION MOVED WITH THE ALIAS-DEFECT SEAT'S FIFTH ARM, AND THE PRIOR ONE IS NAMED RATHER THAN ERASED.
#   One form ago this leg ran at the 1.24.13 pin and expected the EIGHT to fail Target AND Compile,
#   with their Output rows unmeasured-by-artifact.  The fifth arm retires that configuration from
#   batteries; under the PAIRING the i7 measured all eight GREEN in all four phases with Δruntime
#   present in every emission and the goldens byte-matched, so:
#       THE LEG PASSES IFF   every project passes ALL FOUR phases -- 0 fail, 0 timeout / NOT MEASURED --
#                            the Output compared-count reconciles against the tree, the six
#                            platform-exclusives are skipped BY NAME, and the EIGHT are PASS in all
#                            four phases with `using Δruntime = runtime_package;` in each emission.
#       ANY RED IS A FINDING, BY NAME.  There is no expected failing set, so the failing set is printed
#                            whole; seat 2 (src/gen -- route #7's own population) is the first place
#                            to look, then seat 3 (golib and the corpus).
#   ⚠ THE Δ ASSERTION IS THE POSITIVE HALF, AND IT IS NOT THE SAME CLAIM AS "GREEN".  A green suite says
#   nothing about WHICH alias spelling is in the file; the pairing's claim is specifically that the Δ is
#   KEPT.  So the eight emissions are GREPPED for the old spelling after the leg and BEFORE the restore,
#   asserted 8 of 8.  Without it a green would be compatible with an emission nobody looked at.
#   ⚠ THE OUTPUT RECONCILIATION USES **THE RUNNER'S OWN ENUMERATION AND THE RUNNER'S OWN PREDICATE**,
#   both taken from the ASSEMBLED tree at run time.  It is not a formula over packages:
#         N        = the runner's `--list` enumeration -- its `(N projects)` footer, cross-checked
#                    against the parsed project-name count, and the two must AGREE
#         marked   = of those N, the projects whose package_info.cs holds a line that TRIMS to
#                    [GoTestMatchingConsoleOutput]   (MatchConsoleOutput, verbatim)
#         unmarked = N - marked
#       EXPECTED:  Output pass == marked, Output skip == unmarked, Output fail == 0, Output timeout == 0,
#                  and Transpile / Compile / Target pass == N.
#   ⚠ WHAT THIS REPLACED, AND WHY IT WAS WRONG THREE WAYS.  The previous form derived
#   "compared = behavioral packages (dirs holding *.go) - no-`package main` skips - foreign
#   platform-exclusives = 728 - 59 - 6 = 663".  (i) POPULATION: the runner enumerates TOP-LEVEL
#   directories holding BOTH a .csproj and a .go -- 685 -- while 728 is CNR's population, which recurses
#   into nested sub-library packages the runner never sees.  (ii) PREDICATE: the runner's Output gate is
#   an EXPLICIT OPT-IN attribute, and "no `package main`" is only the commonest REASON a project declines
#   it -- measured, 13 of the 26 unmarked are the no-`package main` library projects, a STRICT SUBSET, so
#   a `package main` derivation under-counts the skips by 13.  (iii) DOUBLE SUBTRACTION: F8 removes the
#   six foreign platform-exclusives from `projects` BEFORE `--list` prints, so they are already out of N.
#   ⚠ CALIBRATED BEFORE IT WAS WIRED.  Run in a throwaway worktree at f4d2b981b, this derivation read
#   enumerated=685  marked=659  unmarked=26 -- i9's measured full-suite reading (685 projects, Output 659
#   compared / 26 skipped) to the digit, with 659+26==685 closing and 0 of the 685 missing a
#   package_info.cs.  The old 663 matched nothing the runner reports.
#   ⚠ BOTH pass AND skip ARE ASSERTED, which is strictly stronger than reconciling `compared` alone: the
#   runner moves a project whose COMPILE did not PASS into Output SKIP, so a compile regression reads as
#   pass<marked AND skip>unmarked and cannot hide inside one number.  Every component is printed
#   unconditionally, so a disagreement is diagnosable without a re-run.
#   ⚠ A REBUILD LINE IS **NO LONGER EXPECTED**, AND THAT IS WHY THIS LEG REBUILDS THE CONVERTER
#   ITSELF.  The sentence that stood here said the opposite and was true until train 45: the runner's
#   staleness predicate read `go env GOVERSION` at its own cwd (src/tests/Behavioral, no module:
#   1.23.12) against the binary's embedded 1.24.13, was PERMANENTLY stale under the pairing, and
#   printed `Building go2cs.exe (converter sources changed)...` on EVERY invocation -- which had the
#   side effect that the binary came out newer than every .cs and the Transpile phase could not be
#   skipped.  Train 45's seat 4 (claude/i9-harness-twopin, src/tests/ConverterBuildInputs.cs)
#   correctly compares the embedded release against the CONVERTER MODULE'S OWN `go` directive, so the
#   accidental rebuild is gone -- and train 45 duly reported `Transpile pass 687` over a corpus where
#   LEG 4b's restore had just stamped every .cs newer than the binary, i.e. 687 SKIPPED transpiles
#   reported as passes.  The reading is kept because it is worth having; ZERO is now its expected
#   value, and the assertion moved to the TRANSPILE PREDICATE stamps around the suite.
#   ⚠ A FILTERED run would not do: a filtered runner never exercises a project it does not build, and
#   an affected row need not be one anybody predicted.
#   ⚠ The build budgets are raised explicitly.  BehavioralRunner has its OWN internal timeouts and no
#   caller budget can influence them; the stock 2400 s batch cap was sized at ~604 projects and this
#   corpus is larger, so a healthy run would report the whole corpus NOT MEASURED at the default.
#   ⚠ AND THE AFTER-GUARD RUNS HERE TOO: `go version src/go2cs/bin/go2cs.exe` must read go1.24.13
#   afterwards.  Its justification used to be that the runner rebuilt on every invocation; since that
#   is no longer true, the reason it still runs is THIS leg's own rebuild -- the binary it produced is
#   the one the suite transpiled with, and the after-guard is what says that binary embeds 1.24.13.
#   THE EIGHT, SPELLED HERE FOR A READER WHO LANDS AT THIS LEG -- the CODE below reads the single
#   GOLDEN_PROJECTS definition and never a retyped copy of it:
#       FuncForPCName  FuncLiteralCallerNames  GoexitDefers  GoroutineWaitState
#       IterPullRendezvous  RuntimeCallerFrames  SetFinalizerBridge  SyscallKeystonePulls
# ============================================================================================

# ============================================================================================
# LEG 4b IS **RETIRED** (2026-09-08), AND THE RETIREMENT IS STATED HERE RATHER THAN LEAVING A HOLE.
#   Train 45 ran CNR a SECOND time under the EXPLICIT 1.24.13 RUN pin, expecting CHANGED == 0 where
#   the pre-fix reading had been EIGHT projects (35/35, every line the `using Δruntime` -> `using
#   runtime` drop).  That leg was SEAT 1 OF TRAIN 45's OWN ACCEPTANCE -- the alias fold's defect
#   instrument, run once, as that seat's acceptance -- and it was explicitly NOT a battery
#   expectation about the pairing.  `claude/g-h5-alias-corpus-closure` @234cf8e8d LANDED with train
#   45, so on this base the leg would run a ~1,050-1,750 s CNR to re-measure a fix no seat in this
#   train touches, and its expectation could no longer fail.
#   ⚠ WHAT IS LOST BY REMOVING IT, STATED SO NOBODY HAS TO REDISCOVER IT.  The 1.24.13-pinned CNR is
#   still the ONLY instrument that would catch a REGRESSION of that alias closure, and this train has
#   none.  Train 44's seat-6 ruling stands unchanged -- "the 1.24.13-pinned CNR stays as the alias
#   defect's OWN instrument ... NEVER AS A BATTERY LEG" -- so re-adding it to a future train requires
#   a SEAT whose acceptance it is, not a preference for more coverage.
#   ⚠ AND ITS SIDE EFFECT IS GONE WITH IT, WHICH MATTERS FOR LEG 5.  LEG 4b re-transpiled every
#   behavioral `.cs` and then RESTORED the tree, and that restore stamped every one of those files
#   NEWER than go2cs.exe -- which is how train 45 reported `Transpile pass 687` over 687 SKIPPED
#   transpiles.  LEG 5 now rebuilds the converter itself and ASSERTS the transpile predicate directly
#   (K4 in this file's header), so that leg does not depend on this one's absence any more than it
#   depended on its presence.
# ============================================================================================
pin_go_pairing "LEG-5" || { stamp "LEG 5 UNMEASURED :: the two-pin pairing could not be established, so the suite would transpile and compile under an environment nobody ruled on"; FAILED=1; }

# --- THE RUNNER'S OWN ENUMERATION, TAKEN **BEFORE** THE SUITE ---------------------------------------
#     ⚠ THE EXPECTATION IS DERIVED FROM THE INSTRUMENT, NOT FROM A FORMULA OVER PACKAGES.  `--list`
#     prints one project name per line and a `(N projects)` footer, AFTER F8 has removed the foreign
#     platform-exclusives -- so N is already the MEASURABLE set and nothing further is subtracted from
#     it.  Both readings are taken and they must AGREE: the parsed name count and the footer's own
#     number.  A footer that disagrees with the names is the enumeration collapsing (route #3's shape),
#     which is exactly the failure a count-only read cannot see.
#     ⚠ It is invoked THE WAY THE SUITE IS -- run-behavioral.ps1, from the worktree root, `< /dev/null`
#     -- so the enumeration and the run come from ONE code path.  `--list` neither transpiles nor builds
#     any project; it builds the runner (which the suite would build anyway, seconds later) and exits.
L5LIST="$SCRIPT_DIR/coord-${TAG}-suite-list-$RUNID.log"
powershell -NoProfile -ExecutionPolicy Bypass -File src/tests/Behavioral/run-behavioral.ps1 --list < /dev/null > "$L5LIST" 2>&1
lrc=$?
tr -d '\r' < "$L5LIST" > "$L5LIST.t" && mv "$L5LIST.t" "$L5LIST"
L5FOOT=$(grep -aoE '^\([0-9]+ projects\)$' "$L5LIST" | tail -1 | tr -dc '0-9')
L5FOOTLN=$(grep -anE '^\([0-9]+ projects\)$' "$L5LIST" | tail -1 | cut -d: -f1)
if [ -n "$L5FOOTLN" ]; then
  sed -n "1,$(( L5FOOTLN - 1 ))p" "$L5LIST" | grep -aE '^[A-Za-z0-9_]+$' > "/tmp/${TAG}-l5-projects.txt"
else
  : > "/tmp/${TAG}-l5-projects.txt"
fi
L5ENUM=$(grep -ac . "/tmp/${TAG}-l5-projects.txt" || true)
L5LISTREF=$(refusal_markers "$L5LIST")
stamp "LEG 5 ENUMERATION (the runner's OWN --list, taken before the suite) :: exit=$lrc parsedProjectNames=$L5ENUM footerSays=${L5FOOT:-UNREADABLE}${L5LISTREF:+ refusalMarkers=[$L5LISTREF]}"
if [ "$lrc" != "0" ] || [ -z "${L5FOOT:-}" ] || [ "${L5ENUM:-0}" -lt 1 ] || [ "$L5ENUM" != "${L5FOOT:-x}" ]; then
  stamp "  ^ LEG 5 UNMEASURED (enumeration) :: the runner's --list did not produce an enumeration this leg can reconcile against -- exit=$lrc names=$L5ENUM footer=${L5FOOT:-UNREADABLE}.  Without N there is no expectation to assert, so the Output reconciliation below is a READING and not a gate, and it is stamped as such rather than defaulting to a pass."
  FAILED=1
fi
# --- and the MARKED count, using the runner's OWN predicate, character for character.
#     MatchConsoleOutput (BehavioralRunner/Program.cs) is
#         File.Exists(package_info.cs) && File.ReadLines(...).Any(l => l.Trim() == "[GoTestMatchingConsoleOutput]")
#     -- a whole LINE that TRIMS to the attribute, an EXPLICIT opt-in.  ⚠ NOT a substring grep for the
#     attribute name: a project that merely MENTIONS it in a comment would be counted as opted in, which
#     is the unanchored-marker trap this repository already carries for [module: GoManualConversion].
#     C#'s Trim() removes the CR too, so the CR is stripped before the anchored match.
L5MARK=0; L5UNMARK=0; L5NOPI=0; L5UNMARK_NAMES=""
while IFS= read -r pj <&3; do
  [ -n "$pj" ] || continue
  pi="src/tests/Behavioral/$pj/package_info.cs"
  if [ ! -f "$pi" ]; then
    L5NOPI=$(( L5NOPI + 1 )); L5UNMARK=$(( L5UNMARK + 1 )); L5UNMARK_NAMES="$L5UNMARK_NAMES $pj(no-package_info)"
  elif [ "$(tr -d '\r' < "$pi" | grep -acE '^[[:space:]]*\[GoTestMatchingConsoleOutput\][[:space:]]*$' || true)" != "0" ]; then
    L5MARK=$(( L5MARK + 1 ))
  else
    L5UNMARK=$(( L5UNMARK + 1 )); L5UNMARK_NAMES="$L5UNMARK_NAMES $pj"
  fi
done 3< "/tmp/${TAG}-l5-projects.txt"
stamp "LEG 5 OUTPUT EXPECTATION (derived from the runner's own enumeration and the runner's own predicate) :: enumerated N=$L5ENUM :: marked (MatchConsoleOutput true) = $L5MARK :: unmarked = $L5UNMARK (of which missing a package_info.cs = $L5NOPI, which must be 0) :: EXPECTED Output pass=$L5MARK skip=$L5UNMARK fail=0 timeout=0 :: EXPECTED Transpile/Compile/Target pass=$L5ENUM each"
stamp "LEG 5 unmarked projects (they opt OUT of the Output comparison; the no-\`package main\` library projects are a STRICT SUBSET of these, not the whole of them) :: [$L5UNMARK_NAMES ]"
[ "$(( L5MARK + L5UNMARK ))" = "$L5ENUM" ] || { stamp "  ^ LEG 5 UNMEASURED (expectation) :: marked+unmarked ($(( L5MARK + L5UNMARK ))) != enumerated ($L5ENUM) -- the derivation lost a project, so it cannot be an expectation for anything"; FAILED=1; }
[ "${L5NOPI:-0}" = "0" ] || { stamp "  ^ LEG 5 FINDING: $L5NOPI enumerated project(s) have no package_info.cs.  The runner counts those as UNMARKED (File.Exists is the first half of its predicate) and so does this derivation, but a behavioral project without one is a tree finding in its own right."; FAILED=1; }
stamp "LEG 5 CALIBRATION OF RECORD :: ⚠ THE CALIBRATION IS **DERIVED AT RUN TIME AND SCORED**, NEVER CARRIED.  This is a TEMPLATE: no base and no seat were named when it was written, so there is no derive-time enumeration to predict from and NONE is invented here.  What is ASSERTED is the RELATION -- Output pass == marked, Output skip == unmarked, and the first three phases' pass == N -- read against the RUNNER's OWN --list enumeration and its OWN MatchConsoleOutput predicate.  ⚠ THE NEW PROJECTS ARE NOT ALL TOP-LEVEL: a guard may carry sub-libraries and the runner enumerates DEEPEST-FIRST and RECURSIVELY (route #3), so the enumeration grows by MORE than the top-level count -- derived here as newTopLevel=$NEWBEH newAllDepths=$NEWBEH_ALL.  A wrong prediction is a stamped miss and never a false red."

# --- THE CONVERTER IS REBUILT HERE, EXPLICITLY, IMMEDIATELY BEFORE THE SUITE -----------------------
#     ⚠ WHY THIS LEG NOW OWNS ITS OWN REBUILD.  Train 45 measured `Transpile pass 687` with the
#     runner having printed NO rebuild line and having TRANSPILED NOTHING: LEG 4 and LEG 4b each
#     re-transpile every behavioral .cs and then RESTORE the tree, and a restore stamps every one of
#     those files with an mtime NEWER than go2cs.exe -- after which the runner's up-to-date predicate
#     (`.cs` newer than its `.go` AND newer than the binary) is satisfied by HEAD's OWN committed
#     emission for all 687 projects and the phase is skipped.  That is FALSE-GREEN ROUTE #2 through
#     the door no binary opens, and it reported a clean 687.
#     ⚠ WHAT USED TO HIDE IT, AND WHAT REMOVED THE COVER.  Until train 45 the runner rebuilt the
#     converter on EVERY invocation, because `IsConverterStale` compared the binary's embedded release
#     against the LIVE toolchain and the two-pin pairing holds those permanently different -- so the
#     binary came out newer than every .cs by accident and the predicate was stale for every project.
#     Seat 4 (claude/i9-harness-twopin, src/tests/ConverterBuildInputs.cs) CORRECTLY changed that
#     comparison to the converter module's own `go` directive, which is the right predicate and which
#     removed the accident this leg was silently resting on.  A leg that depends on another
#     component's incidental behaviour is a leg that goes vacuous when that component is fixed.
#     ⚠ THE BUILD FORM IS LEG R's, VERBATIM, and it runs under the PAIRING this leg already set:
#     GOTOOLCHAIN=auto plus src/go2cs/go.mod's own `go 1.24.13` is what switches the BUILD half up
#     while the LOAD half stays at 1.23.12, which is the whole two-pin shape.
#     ⚠ THE SENTINEL IS TAKEN BEFORE THE BUILD.  `go build -o` exiting 0 with a STALE binary already
#     at the invoked path is a measured failure mode; an existence-and-size check reads the stale file
#     and passes, so the assertion is that the mtime MOVED past this instant.
touch "/tmp/${TAG}-l5-build-sentinel"
L5B="$SCRIPT_DIR/coord-${TAG}-l5-convbuild-$RUNID.log"
( cd src/go2cs && go build -o bin/go2cs.exe . > "$L5B" 2>&1 < /dev/null ); l5brc=$?
L5BFRESH=$( [ -f "$CONVEXE" ] && [ "$CONVEXE" -nt "/tmp/${TAG}-l5-build-sentinel" ] && echo yes || echo NO )
L5BVER="$(go version "$CONVEXE" 2>&1 | tr -d '\r')"
stamp "LEG 5 CONVERTER REBUILD exit=$l5brc :: mtimeMoved=$L5BFRESH (must be yes) :: \`go version $CONVEXE\` reads [$L5BVER] (must carry go1.24.13 -- the binary's own EMBEDDED release, which no cwd and no GOTOOLCHAIN can switch afterwards)"
[ "$l5brc" = "0" ] || { stamp "  ^ LEG 5 UNMEASURED: the converter did not build, so the suite would transpile with whatever binary is on disk; head of the log:"; head -12 "$L5B" | sed 's/^/    /' | tee -a "$ASMLOG"; FAILED=1; }
[ "$L5BFRESH" = "yes" ] || { stamp "  ^ LEG 5 UNMEASURED: the converter binary's mtime did NOT move past the sentinel, so nothing here can say which binary the suite is about to use"; FAILED=1; }
case "$L5BVER" in *go1.24.13*) : ;; *) stamp "  ^ LEG 5 UNMEASURED: the rebuilt converter does not embed go1.24.13"; FAILED=1 ;; esac
# --- and the PREDICATE ITSELF, read from the tree the runner is about to walk.  ⚠ THIS IS THE
#     ASSERTION, not the rebuild: the rebuild is only the means.  `bin`, `obj` and `Generated` are
#     excluded because they hold build output whose mtimes say nothing about the emission.
L5CSNEW=$(find src/tests/Behavioral -type f -name '*.cs' -not -path '*/bin/*' -not -path '*/obj/*' -not -path '*/Generated/*' -newer "$CONVEXE" 2>/dev/null | grep -ac . || true)
L5CSALL=$(find src/tests/Behavioral -type f -name '*.cs' -not -path '*/bin/*' -not -path '*/obj/*' -not -path '*/Generated/*' 2>/dev/null | grep -ac . || true)
stamp "LEG 5 TRANSPILE PREDICATE (BEFORE the suite) :: behavioral .cs files NEWER than the rebuilt converter = ${L5CSNEW:-x} of ${L5CSALL:-0} (MUST be 0).  The runner skips a project's Transpile when its .cs is newer than BOTH its .go and go2cs.exe, so a zero here is what makes the phase measure an emission rather than re-validate the committed one"
[ "${L5CSNEW:-1}" = "0" ] || { stamp "  ^ LEG 5 FINDING: ${L5CSNEW} behavioral .cs file(s) are still NEWER than the converter, so the runner's up-to-date predicate is SATISFIED for them and their Transpile will be SKIPPED.  A pass count that includes a skipped transpile is a phase validating the committed emission against itself -- FALSE-GREEN ROUTE #2 -- and it reads exactly like a clean run."; FAILED=1; }
stamp "LEG 5 FULL SUITE starting under the PAIRING (budget ~1900-6600 s on this box class; EXPECTED: ALL GREEN -- every project passing all four phases, Output pass=$L5MARK / skip=$L5UNMARK / fail=0 / timeout=0 against the runner's own enumeration of $L5ENUM, the six platform-exclusives skipped by name, and the eight carrying Δruntime -- E2')"
L5="$SCRIPT_DIR/coord-${TAG}-suite-$RUNID.log"; t0=$(date +%s)
powershell -NoProfile -ExecutionPolicy Bypass -File src/tests/Behavioral/run-behavioral.ps1 --build-timeout 10800 --build-one-timeout 900 > "$L5" 2>&1
rc=$?; w=$(( $(date +%s) - t0 ))
tr -d '\r' < "$L5" > "$L5.t" && mv "$L5.t" "$L5"
read -r S_CMP S_FAIL S_TERM S_SPASS S_SFAIL <<< "$(read_output_verdict "$L5")"
S_REF=$(refusal_markers "$L5")
S_NM=$(grep -acEi 'NOT MEASURED' "$L5" || true)
S_CS0576=$(grep -ac 'CS0576' "$L5" || true)
S_REBUILT=$(grep -ac 'Building go2cs.exe (converter sources changed)' "$L5" || true)
S_SKIP=$(grep -acEi 'platform-exclusive|GoPlatformExclusive' "$L5" || true)
stamp "LEG 5 SUITE exit=$rc wall=${w}s :: Output $S_CMP compared / $S_FAIL failed (summary pass=$S_SPASS fail=$S_SFAIL) terminal=$S_TERM notMeasuredMentions=$S_NM CS0576mentions=$S_CS0576 converterRebuildLines=$S_REBUILT (a READING, never a fault -- and ⚠ ZERO IS NOW THE EXPECTED VALUE: train 45's seat 4 changed the runner's staleness comparison from the LIVE toolchain to the converter module's own go.mod directive, which retired the rebuild-on-every-invocation this leg used to rest on.  A NON-ZERO is now the odd one.  This leg REBUILDS the converter itself immediately before the suite, and the TRANSPILE PREDICATE stamps above and below are what say the phase measured anything) platformExclusiveMentions=$S_SKIP${S_REF:+ refusalMarkers=[$S_REF]}"
stamp "LEG 5 phase table :: $(grep -aiE '^[[:space:]]+(Transpile|Compile|Target|Output)[[:space:]]+pass' "$L5" | tr '\n' ' ' | tr -s ' ' | cut -c1-300)"
# --- THE TRANSPILE PREDICATE AGAIN, AFTER THE SUITE.  ⚠ A `Transpile pass N` LINE IS NOT EVIDENCE
#     THAT N PROJECTS WERE TRANSPILED -- the runner reports a SKIPPED project as a pass -- so the
#     evidence is the tree: after a real transpile every enumerated project's .cs is NEWER than the
#     binary that wrote it.  Taken BEFORE the restore, because the restore is what erases it.
#     ⚠ THE FLOOR IS DELIBERATELY CONSERVATIVE BY SIX AND SAYS SO.  F8 removes the six foreign
#     platform-exclusives from `projects` BEFORE `--list` prints, so they are ALREADY out of $L5ENUM
#     and the honest floor is $L5ENUM itself; the asserted floor subtracts them a second time so that
#     a future change to what F8 skips cannot turn this assertion red for the wrong reason.  Both
#     numbers are printed, and the stronger comparison is a READING beside the asserted one.
L5CSNEW2=$(find src/tests/Behavioral -type f -name '*.cs' -not -path '*/bin/*' -not -path '*/obj/*' -not -path '*/Generated/*' -newer "$CONVEXE" 2>/dev/null | grep -ac . || true)
L5CSFLOOR=$(( ${L5ENUM:-0} - 6 ))
stamp "LEG 5 TRANSPILE PREDICATE (AFTER the suite, before the restore) :: behavioral .cs files newer than the converter = ${L5CSNEW2:-0} :: ASSERTED floor = enumerated ${L5ENUM:-0} minus the six platform-exclusives = $L5CSFLOOR :: READING, not asserted -- against the enumeration itself ${L5ENUM:-0}, which is the honest floor since F8 already removed the six ($( [ "${L5CSNEW2:-0}" -ge "${L5ENUM:-0}" ] && echo MET || echo BELOW ))"
if [ "${L5CSNEW2:-0}" -ge "${L5CSFLOOR:-0}" ] && [ "${L5CSFLOOR:-0}" -ge 1 ]; then
  stamp "LEG 5 TRANSPILE RAN :: the suite rewrote at least $L5CSFLOOR behavioral .cs file(s), so the Transpile phase MEASURED an emission rather than reporting a skip as a pass"
else
  stamp "  ^ LEG 5 FINDING: only ${L5CSNEW2:-0} behavioral .cs file(s) are newer than the converter where the floor is $L5CSFLOOR.  Transpile was SKIPPED for most or all of the corpus and its pass count validates the COMMITTED emission against itself -- which is what train 45 measured (Transpile pass 687, zero rebuild lines, every .cs newer than the binary because LEG 4b's restore had just stamped them)."
  FAILED=1
fi
if [ "$S_TERM" = "NONE" ]; then
  stamp "  ^ LEG 5 UNMEASURED: no terminal PASS/FAIL line -- the Stop-preference wrapper died and orphaned the runner, or the log was truncated.  Census for the CHILD by executable path before believing anything else; this is not a red and it is not E2' either."
  FAILED=1
fi
if [ "${S_REBUILT:-0}" -ge 1 ]; then
  stamp "LEG 5 NOTE (a reading, not a refusal) :: the runner printed $S_REBUILT 'Building go2cs.exe (converter sources changed)' line(s).  ⚠ UNDER THE CURRENT HARNESS THAT IS THE **ODD** READING, not the expected one: since train 45's seat 4 the staleness predicate compares the binary's embedded release against the converter module's own go.mod directive, so a converter this leg has JUST REBUILT should not read stale.  It is not a fault -- an editor save or a branch switch moves a source mtime -- but it means the suite used a binary this leg did not build, and the after-guard below is where that gets reconciled."
else
  stamp "LEG 5 NOTE (a reading, not a refusal) :: the runner printed NO 'Building go2cs.exe (converter sources changed)' line, which since the runner's rebuild predicate was corrected is the EXPECTED reading and no longer says anything about the transpile.  It USED to: the old predicate compared a 1.23.12 GOVERSION against the binary's embedded 1.24.13 and was permanently stale under the pairing, so the runner rebuilt on every invocation and every .cs was thereby older than the binary.  That accident is gone, which is why this leg now rebuilds the converter itself and asserts the TRANSPILE PREDICATE directly."
fi

# --- THE FAILING SET.  ⚠ UNDER E2' IT MUST BE **EMPTY**, so this reads it whole rather than comparing
#     it against a predicted set.  The runner prints `  <Name> [<phase>,<phase>]`; the bracket is
#     PARSED and SORTED so a phase list is reported in a stable order rather than string-compared.
grep -aE '^  [A-Za-z0-9_]+ \[[A-Za-z,]+\]$' "$L5" > "/tmp/${TAG}-l5-fails.txt" || true
awk '{print $1}' "/tmp/${TAG}-l5-fails.txt" | sort -u > "/tmp/${TAG}-l5-failnames.txt"
L5N=$(grep -ac . "/tmp/${TAG}-l5-failnames.txt" || true)
stamp "LEG 5 FAILING SET :: $L5N project(s) (E2' expects 0) :: [$(tr '\n' ' ' < "/tmp/${TAG}-l5-failnames.txt")]"
if [ "${L5N:-1}" = "0" ]; then
  stamp "LEG 5 SET OK :: no project failed any phase, which is E2'."
else
  stamp "  ^ LEG 5 FINDING: $L5N project(s) failed, where E2' expects NONE.  EVERY member is a finding BY NAME -- there is no absorbed set.  ⚠ WHERE TO LOOK, DERIVED FROM THE OWED VECTOR RATHER THAN FROM A CARRIED SEAT ORDER: gen=$OWED_GEN first (a generator template is invisible to CNR and to the stdlib solution alike, and THIS phase is route #7's named gate), then golib=$OWED_GOLIB, then converter=$OWED_CONV (its guard projects and any nested sub-libraries, which route #3 says no other gate enumerates).  The members:"
  while IFS= read -r ln <&3; do
    [ -n "$ln" ] || continue
    nm=$(printf '%s' "$ln" | awk '{print $1}')
    ph=$(printf '%s' "$ln" | sed -E 's/.*\[([A-Za-z,]+)\].*/\1/' | tr ',' '\n' | sort | tr '\n' ',' | sed 's/,$//')
    if [ "$ph" = "Compile,Target" ] && [ "$(printf '%s\n' "$GOLDEN_PROJECTS" | grep -acx "$nm" || true)" != "0" ]; then
      stamp "      $nm [$ph] -- one of the EIGHT alias-carrying projects.  ⚠ THIS IS A FINDING ABOUT A SEAT LIKE ANY OTHER: the alias fold makes the emission PIN-INDEPENDENT, so a Compile+Target red here is NOT a pairing diagnosis, and the seat that changes converter source is where to look"
    else
      stamp "      $nm [$ph]"
    fi
  done 3< "/tmp/${TAG}-l5-fails.txt"
  fail_gate LEG-5-failing-set
fi
# --- THE EIGHT MUST BE GREEN **AND** MUST CARRY THE Δ.  Two different claims, and the second is the
#     one a green suite cannot make on its own.  ⚠ GREPPED BEFORE THE RESTORE, or there is nothing left
#     to read.  The literal comes from ALIAS_OLD, written once at the top of this script.
L5DELTA_OK=0; L5DELTA_BAD=""; L5EIGHT_RED=""
while IFS= read -r pj <&3; do
  [ -n "$pj" ] || continue
  [ "$(grep -acx "$pj" "/tmp/${TAG}-l5-failnames.txt" 2>/dev/null || true)" != "0" ] && L5EIGHT_RED="$L5EIGHT_RED $pj"
  if [ -f "src/tests/Behavioral/$pj/main.cs" ] && [ "$(grep -acF -- "$ALIAS_OLD" "src/tests/Behavioral/$pj/main.cs" || true)" != "0" ]; then
    L5DELTA_OK=$(( L5DELTA_OK + 1 ))
  else
    L5DELTA_BAD="$L5DELTA_BAD $pj"
  fi
done 3<<< "$GOLDEN_PROJECTS"
stamp "LEG 5 THE EIGHT :: red among them=[${L5EIGHT_RED:- none}] (must be none) :: emissions carrying [$ALIAS_OLD] = $L5DELTA_OK of 8 (must be 8 -- the pairing's claim is that the Δ is KEPT, and a green suite alone does not say what is in the file)"
[ -z "$L5EIGHT_RED" ] || { stamp "  ^ LEG 5 FINDING: one of the eight is RED.  If its phases are exactly [Compile,Target] with CS0576 this is the pairing failing, not a seat."; FAILED=1; }
[ "${L5DELTA_OK:-0}" = "8" ] || { stamp "  ^ LEG 5 FINDING: $L5DELTA_BAD -- emission(s) among the eight do NOT carry the Δ spelling, so the converter loaded a closure at the wrong release even if the suite happened to stay green.  This is the assertion that separates 'green' from 'green for the right reason'."; FAILED=1; }
# --- THE OUTPUT RECONCILIATION, AGAINST **THE RUNNER'S OWN ENUMERATION AND PREDICATE** -------------
#     The expectation was derived BEFORE the suite ran (L5ENUM / L5MARK / L5UNMARK above) from
#     `--list` and from MatchConsoleOutput; this reads what the run REPORTED and compares the two.
#     ⚠ FIVE ASSERTIONS, NOT ONE RECONCILED NUMBER.  `compared` alone cannot separate a compile
#     regression from a skip: the runner moves a project whose Compile did not PASS into Output SKIP,
#     so the pair (pass BELOW marked, skip ABOVE unmarked) is what names it.  And the three upstream
#     phases pin the ENUMERATION -- a Transpile/Compile/Target pass count below N is a phase that
#     processed fewer projects than the runner listed, which no Output reading can see.
#     ⚠ THE PLATFORM-EXCLUSIVES ARE **NOT** SUBTRACTED HERE.  F8 removes them from `projects` before
#     `--list` prints, so they are already out of N; the leg still asserts the skip LINE reads six,
#     because that is a different claim (F8 reporting them BY NAME) and it is checked below.
read -r P_TPASS P_TFAIL P_TSKIP P_TTMO <<< "$(read_phase_row "$L5" Transpile)"
read -r P_CPASS P_CFAIL P_CSKIP P_CTMO <<< "$(read_phase_row "$L5" Compile)"
read -r P_GPASS P_GFAIL P_GSKIP P_GTMO <<< "$(read_phase_row "$L5" Target)"
read -r P_OPASS P_OFAIL P_OSKIP P_OTMO <<< "$(read_phase_row "$L5" Output)"
L5GOOS=$(go env GOOS 2>/dev/null | tr -d '\r')
L5PEXLINE=$(grep -aoE '^SKIPPED \(platform-exclusive, [0-9]+\)' "$L5" | tail -1 | tr -dc '0-9')
L5PEXNAMES=$(sed -n '/^SKIPPED (platform-exclusive, [0-9]*)/,/^$/p' "$L5" | grep -aoE '^    [A-Za-z0-9_]+ \[[a-z0-9/,-]+\]' | sed 's/^ *//' | tr '\n' ' ')
stamp "LEG 5 OUTPUT RECONCILIATION (expected from the runner's own --list enumeration and MatchConsoleOutput; reported from the run) :: host GOOS=$L5GOOS :: enumerated N=$L5ENUM :: EXPECTED Output pass=$L5MARK skip=$L5UNMARK fail=0 timeout=0 :: REPORTED Output pass=$P_OPASS fail=$P_OFAIL skip=$P_OSKIP timeout=$P_OTMO (comparison line said compared=$S_CMP failed=$S_FAIL)"
stamp "LEG 5 PHASE RECONCILIATION :: EXPECTED Transpile/Compile/Target pass=$L5ENUM each :: REPORTED Transpile pass=$P_TPASS fail=$P_TFAIL skip=$P_TSKIP timeout=$P_TTMO | Compile pass=$P_CPASS fail=$P_CFAIL skip=$P_CSKIP timeout=$P_CTMO | Target pass=$P_GPASS fail=$P_GFAIL skip=$P_GSKIP timeout=$P_GTMO"
stamp "LEG 5 platform-exclusive skip line (F8 reports them BY NAME, and they are ALREADY out of N) :: count=${L5PEXLINE:-ABSENT} :: [$L5PEXNAMES]"
[ "${L5PEXLINE:-0}" = "6" ] || { stamp "  ^ LEG 5 FINDING: the runner's own skip line reads ${L5PEXLINE:-ABSENT} platform-exclusive(s) where LEG 4's skip line and the derive both read 6.  The two legs get this from the same tree by different routes, so a disagreement is an instrument question before it is a tree question."; FAILED=1; }
if [ "${L5ENUM:-0}" -lt 1 ]; then
  stamp "  ^ LEG 5 UNMEASURED (reconciliation): there is no enumeration to reconcile against, so the numbers above are a READING and nothing here is asserted.  The enumeration refusal upstream already named this."
elif [ "$P_OPASS" = "NONE" ] || [ "$P_TPASS" = "NONE" ] || [ "$P_CPASS" = "NONE" ] || [ "$P_GPASS" = "NONE" ]; then
  stamp "  ^ LEG 5 UNMEASURED (reconciliation): the summary phase table did not carry all four rows, so the reported side of this comparison does not exist.  A missing row is not a zero and it is not a pass."
  FAILED=1
else
  L5RECFAIL=0
  [ "$P_OPASS" = "$L5MARK" ]   || { stamp "  ^ LEG 5 FINDING: Output pass=$P_OPASS against an expected $L5MARK.  A DEFICIT here with a matching SURPLUS in skip is a COMPILE regression -- the runner buckets a project whose Compile did not PASS as an Output SKIP -- and a deficit with fail>0 is a real comparison divergence, which is exactly what route #7's behavioral twin produces."; L5RECFAIL=1; }
  [ "$P_OSKIP" = "$L5UNMARK" ] || { stamp "  ^ LEG 5 FINDING: Output skip=$P_OSKIP against an expected $L5UNMARK (the projects whose package_info.cs does NOT carry a line trimming to [GoTestMatchingConsoleOutput]).  A SURPLUS means projects fell out of the comparison -- either a compile that did not pass, or an emission that lost its hand-added attribute, which is the '0 compared, 0 failed, skip 1' shape one project at a time."; L5RECFAIL=1; }
  { [ "$P_OFAIL" = "0" ] && [ "$P_OTMO" = "0" ]; } || { stamp "  ^ LEG 5 FINDING: Output fail=$P_OFAIL timeout=$P_OTMO, and E2' expects 0 of each.  A FAILED comparison is a C#-vs-\`go run\` divergence and is where a golib, generator or golib-primitive row would show; a TIMEOUT is NOT MEASURED and is never a pass."; L5RECFAIL=1; }
  for pv in "Transpile:$P_TPASS:$P_TFAIL:$P_TSKIP:$P_TTMO" "Compile:$P_CPASS:$P_CFAIL:$P_CSKIP:$P_CTMO" "Target:$P_GPASS:$P_GFAIL:$P_GSKIP:$P_GTMO"; do
    phn="${pv%%:*}"; rest="${pv#*:}"; php="${rest%%:*}"; rest="${rest#*:}"; phf="${rest%%:*}"; rest="${rest#*:}"; phs="${rest%%:*}"; pht="${rest##*:}"
    [ "$php" = "$L5ENUM" ] || { stamp "  ^ LEG 5 FINDING: $phn pass=$php against the runner's own enumeration of $L5ENUM (fail=$phf skip=$phs timeout=$pht).  Under E2' every enumerated project passes every one of the first three phases, so a shortfall here is a project that did not get through -- and a TIMEOUT among them is NOT MEASURED, never a failure and never a pass."; L5RECFAIL=1; }
  done
  if [ "$L5RECFAIL" = "0" ]; then
    stamp "LEG 5 RECONCILED :: Output pass $P_OPASS == marked $L5MARK, skip $P_OSKIP == unmarked $L5UNMARK, fail 0, timeout 0; Transpile/Compile/Target pass == the enumerated $L5ENUM each.  The expectation came from the runner's OWN --list and its OWN MatchConsoleOutput predicate, so this is one instrument reconciled against itself rather than a formula over a different population."
  else
    fail_gate LEG-5-reconciliation
  fi
fi
# the comparison LINE is read too, as a second derivation of the same quantity: it is produced by a
# different counter inside the runner (`compared`), and where the two disagree the disagreement is
# itself the finding.
if [ "$S_CMP" = "NONE" ]; then
  stamp "  ^ LEG 5 UNMEASURED: the log carried no '[Output] running C# vs Go ... N compared' line at all."
  FAILED=1
elif [ "$P_OPASS" != "NONE" ] && [ "$S_CMP" != "$P_OPASS" ]; then
  stamp "  ^ LEG 5 FINDING (instrument): the comparison line says compared=$S_CMP while the summary row says Output pass=$P_OPASS.  These are two counters inside ONE runner and they must agree; a difference is a project that was compared and then scored as something other than pass, so read the failing set above before reading either number as the answer."
  FAILED=1
fi
stamp "LEG 5 detail (the failing lines and their messages, verbatim):"
sed -n '/---- [0-9]* failing project(s) ----/,/^$/p' "$L5" | head -60 | sed 's/^/      /' | tee -a "$ASMLOG"
rm -f "/tmp/${TAG}-l5-fails.txt" "/tmp/${TAG}-l5-failnames.txt" "/tmp/${TAG}-l5-projects.txt"
stamp "LEG 5 EXIT CODE :: $rc -- and under E2' it must be 0"
[ "$rc" = "0" ] || { stamp "  ^ LEG 5 FINDING: the suite exited $rc.  E2' expects an all-green run, so a non-zero exit is a finding; the FAILING SET above names which projects, and if that set is empty while the exit is non-zero the run died somewhere the phase table does not report."; FAILED=1; }
# --- THE AFTER-GUARD, ASSERTED.  ⚠ ITS OLD JUSTIFICATION IS RETIRED: the runner does NOT rebuild
#     on every invocation any more (train 45 seat 4).  What makes this guard mean something now is
#     that THIS LEG rebuilt the converter itself immediately before the suite, so the binary it reads
#     is the binary the suite transpiled with.
after_guard_converter "LEG-5" assert || { stamp "  ^ LEG 5 UNMEASURED (after-guard) :: the binary this suite transpiled with was not built at go1.24.13, so the BUILD half of the pairing did not hold for this leg and its green describes an experiment nobody named."; FAILED=1; CONVOK=0; }

git checkout -- src/tests/Behavioral 2>/dev/null; git clean -fdq -- src/tests/Behavioral 2>/dev/null
L5DELTRK=$(git status --porcelain | grep -c '^ D')
stamp "LEG 5 post-suite restore :: dirty=$(git status --porcelain | wc -l) deleted-tracked=$L5DELTRK (must be 0)"
[ "$L5DELTRK" = "0" ] || { stamp "  ^ a cleanup DELETED TRACKED FILES -- restore them before anything else"; FAILED=1; }

# --- the bin purge.  `git clean -fd` does NOT do it: [Bb]in/ and [Oo]bj/ are gitignored at the repo
#     root, so an unqualified clean leaves ~20 GB of behavioral build output standing -- and LEG K's
#     sweep has its own 25 GB floor it would then refuse at, stamping a plausible-looking empty verdict.
#     UNLIMITED depth (a -maxdepth purge once missed 274 of 388 directories) and SCOPED to
#     src/tests/Behavioral, so it cannot reach src/go2cs/bin/go2cs.exe -- which LEG K needs.
BEFORE=$(freegb)
find src/tests/Behavioral -type d \( -name bin -o -name obj -o -name Generated \) -prune -print0 2>/dev/null | xargs -0 -r rm -rf
AFTER=$(freegb)
LEFT=$(find src/tests/Behavioral -type d \( -name bin -o -name obj -o -name Generated \) -prune -print 2>/dev/null | grep -c .)
stamp "LEG 5 post-suite bin purge :: freeGB ${BEFORE:-?} -> ${AFTER:-?}  dirsLeftUnderBehavioral=$LEFT (must be 0)  converterStillPresent=$( [ -f src/go2cs/bin/go2cs.exe ] && echo yes || echo NO )"
[ "$LEFT" = "0" ] || { stamp "  ^ the purge did not remove everything it found"; FAILED=1; }
[ -f src/go2cs/bin/go2cs.exe ] || { stamp "  ^ the purge removed the CONVERTER -- LEG K cannot run -SkipBuild without it"; CONVOK=0; }

# ============================================================================================
# LEG K -- THE **DERIVED** REFLECT CANARY SET, PLUS THE `sync` SEAT-GATE ROW, PLUS THE COST CANARY.  TWO-PIN.
#   ⚠ THE CANARY SET IS **DERIVED AT RUN TIME AND NEVER CARRIED**, because a dated top-five has
#   drifted THREE times in this project's history and each time the stale membership travelled for
#   days: `go/internal/gcimporter` and `crypto/internal/nistec` both rode in the set while importing
#   reflect NOWHERE, and a worked example in the doctrine file substituted for the derivation until a
#   lane's fresh grep caught it while holding an expensive sweep.  So this leg carries the RULE and
#   the CONTROLS, and computes the members from THIS tree at THIS moment.
#
#   WHY IT IS OWED AT ALL.  Seat 2 changes `src/gen/go2cs-gen/Templates/InheritedType/
#   ISliceTypeTemplate.cs` -- a go2cs-gen TEMPLATE.  The canary rule's own wording is that
#   "reflect-bridge-touching reads broadly: src/core/reflect/*_impl.cs, src/core/internal/reflectlite,
#   golib's GoReflect.*/adapter/equality machinery, and the go2cs-gen adapter/shell templates all
#   qualify", and seat 2 ALSO changes `src/core/golib/slice.cs`.  A shell template is compiled into
#   every converted consumer of a named slice type, which is the cross-assembly population route #7
#   names and which NO standing gate compiles.
#
#   THE DERIVATION, AND EVERY CLAUSE HAS A REASON:
#     * THE PREDICATE IS "THE PACKAGE'S OWN GO SOURCE -- PRODUCTION **OR** `_test.go` -- IMPORTS
#       reflect".  Test usage counts because the canary protects VERDICTS, and verdicts are produced
#       by test code: a suite leaning on reflect.DeepEqual is exactly what a bridge regression breaks.
#     * IT IS READ FROM **PARSED IMPORT DECLARATIONS**, not a line-anchored grep.  A name-LIST match
#       over-matches -- `go/doc/comment` carries the string as DATA in a raw literal -- and a
#       line-anchored grep admitted `go/doc/comment` and `go/internal/gccgoimporter` while BOTH
#       standing controls passed.  Here the parser is `go list` under the 1.23.12 pin: the toolchain's
#       own import resolution, which cannot be a text heuristic.
#     * THE CONTROLS ARE RUN **BEFORE** THE SET IS BELIEVED, both directions: `encoding/json` must be
#       IN and `go/doc/comment` must be OUT.  A derivation that cannot reproduce a known-good answer
#       is not a derivation, and the second control pins the axis the counterexample names.
#     * THE RANKING IS BY **MATCHING VERDICT COUNT FROM THE ROSTER IN THIS TREE**, top five.
#   ⚠ IF THE DERIVATION REFUSES -- `go list` unavailable, a control failing, an unreadable roster --
#   THE LEG IS **UNMEASURED AND SAYS SO**.  It does not fall back to a carried list: a fallback is how
#   a stale membership becomes permanent.
#
#   THE OTHER TWO ROWS, and they are NOT canaries:
#     * `sync` is **A GOLIB-CORPUS HAND-OWN SEAT GATE, NOT A CANARY**.  A hand-own retarget under
#       `src/core/sync/` emits nothing a diff can see and is invisible to LEG D and to CNR alike; such
#       a row is BANKED, so only a RUN of the banked row can say whether the hand-own still works.
#       ⚠ THE ROW IS UNCONDITIONAL AND ITS OWE IS NOT: it is cheap, and a gate dropped because this
#       table happens to seat no hand-own row is a gate that comes back only if somebody remembers.
#       It is given `-TestTimeout 20m` because it is NOT in the sweep's `$longTimeouts` table and would
#       otherwise run at the 10m default; a LARGER -TestTimeout raises the floor, a smaller one still loses.
#     * `crypto/internal/nistec` is the **DESCRIPTOR-SYNTHESIS COST CANARY**.  Its WALL is RECORDED
#       BESIDE the recorded baseline FOR THE COORDINATOR TO COMPARE and is **NOT JUDGED** here: a wall
#       compared against a figure taken on another day compares two hosts, and only a fourth arm (an
#       older tip re-run TODAY) could attribute a move.  It carries NO extra argument, because an
#       argument it did not carry before would make this run's wall incomparable with the record.
#
#   ⚠ THE BUDGET IS LARGE AND IS STATED RATHER THAN DISCOVERED.  The derived canary set can include
#   `crypto/tls` (3,643 verdicts, bogo-capable hosts only) and `net/http` (1,343), each of which has a
#   per-package deadline FLOOR in the sweep's own `$longTimeouts` table.  This is the longest leg in
#   the battery by a wide margin.  It is NOT given a skip switch: a switch that empties a gate is the
#   lie-lever shape, and the coordinator can stop a run.
#
#   HOW EACH ROW IS READ: the VERDICT must be PASS at the row's BANKED count, DERIVED from the roster
#   in THIS tree at run time and never a literal here.
#   NO -TestConfig IS PASSED.  An EXPLICIT -TestConfig forces uniformity and OVERRIDES per-row
#   execution annotations, and a row run that way is not bank-eligible; the default is Release with
#   tiering off, which is the configuration of record.
#   ⚠ THE TWO-PIN SHAPE, and every clause of it has a source:
#     * the converter was BUILT under 1.24.13 by LEG 4 and LEG 5 (asserted by mtime AND by the
#       binary's own embedded release, not assumed)
#     * the sweep runs under the 1.23.12 pin, because its OWN guard compares version.props's
#       <GoStdLibVersion> against GOROOT's VERSION file and THROWS on a disagreement
#     * -SkipBuild is MANDATORY: without it the sweep runs `go build` under the 1.23.12 pin and dies
#       on `go.mod requires go >= 1.24.13`.  With it, the sweep only checks the binary EXISTS.
#     * pin_go EXPORTS GOROOT and re-orders PATH rather than relying on any flag, because `-goroot`
#       does NOT isolate the package loader
#     * the record's oracleGoVersion must read the WHOLE recorded `go version` line at go1.23.12.  It
#       is `omitempty`, so an ABSENT field is UNMEASURED and never a pass.
# ============================================================================================
NISTEC_BASELINE_S=384     # the recorded (memoized) re-measure quoted in CLAUDE.md; a REFERENCE, not a gate
FREE_K=$(freegb)
LEGK_ROWS=""; LEGK_DERIVED_OK=0; LEGK_CANARIES=""
# --- THE CANARY DERIVATION.  It runs under the 1.23.12 pin because `go list` must resolve the
#     CORPUS's release, and it is CONTROLLED both directions before its answer is used.
if pin_go 1.23.12 "LEG-K-derive"; then
  LEGK_ROSTER='docs/ValidatedTestPackages.md'
  if [ ! -f "$LEGK_ROSTER" ]; then
    stamp "LEG K CANARY DERIVATION UNMEASURED :: $LEGK_ROSTER is absent, so there is no banked-row population to rank"
  else
    # every banked row and its MATCHING verdict count, read from the roster's own table.  awk with
    # index(), never a built regex: escaping a package's slashes through a sed once collapsed a
    # doubled backslash and the pattern matched nothing, printing UNREADABLE for a row plainly there.
    LEGK_BANKED="/tmp/${TAG}-legk-banked.txt"
    awk -F'|' 'NF>4 { f=$2; gsub(/^ +| +$/,"",f); if (substr(f,1,1)=="[" ) { p=f; sub(/^\[`/,"",p); sub(/`\].*/,"",p); v=$3; gsub(/[^0-9]/,"",v); if (p != "" && v != "") print v "\t" p } }' "$LEGK_ROSTER" | sort -rn > "$LEGK_BANKED"
    LEGK_BANKN=$(grep -ac . "$LEGK_BANKED" || true)
    # the IMPORT predicate, from the toolchain's own resolution.  Production, internal tests and
    # external tests are all asked, because the canary protects VERDICTS.
    legk_imports_reflect(){ # $1 = std package path -> 0 when it imports reflect anywhere
      [ "$(go list -e -f '{{join .Imports "\n"}}{{"\n"}}{{join .TestImports "\n"}}{{"\n"}}{{join .XTestImports "\n"}}' "$1" 2>/dev/null | tr -d '\r' | grep -acx 'reflect' || true)" != "0" ]
    }
    LEGK_C1=NO; LEGK_C2=NO
    legk_imports_reflect 'encoding/json'   && LEGK_C1=YES
    legk_imports_reflect 'go/doc/comment'  || LEGK_C2=YES
    stamp "LEG K CANARY CONTROLS (run BEFORE the set is believed, both directions) :: encoding/json imports reflect = $LEGK_C1 (want YES) :: go/doc/comment does NOT = $LEGK_C2 (want YES -- it carries the NAME as DATA inside a raw literal, which is exactly what a line-anchored grep admits and a parsed derivation refuses) :: bankedRowsRead=$LEGK_BANKN"
    if [ "$LEGK_C1" = "YES" ] && [ "$LEGK_C2" = "YES" ] && [ "${LEGK_BANKN:-0}" -ge 1 ]; then
      LEGK_N=0
      while IFS="$(printf '\t')" read -r v p <&3; do
        [ -n "$p" ] || continue
        [ "$LEGK_N" -ge 5 ] && break
        if legk_imports_reflect "$p"; then
          LEGK_N=$(( LEGK_N + 1 ))
          LEGK_CANARIES="$LEGK_CANARIES $p"
          stamp "LEG K canary $LEGK_N :: $p (roster matching verdicts=$v) -- a BANKED row whose own Go source imports reflect, by the toolchain's parsed import declarations"
        fi
      done 3< "$LEGK_BANKED"
      if [ "$LEGK_N" -ge 1 ]; then
        LEGK_DERIVED_OK=1
        stamp "LEG K CANARY SET DERIVED :: $LEGK_N of a wanted 5 ::$LEGK_CANARIES -- computed from THIS tree's roster and THIS toolchain's import resolution.  ⚠ A DATED top-five is NOT carried in this script and never will be: that membership has drifted three times in this project's history"
      else
        stamp "LEG K CANARY DERIVATION UNMEASURED :: the predicate admitted ZERO banked rows, which cannot be true of this corpus -- the derivation is broken, not the tree"
      fi
    else
      stamp "LEG K CANARY DERIVATION UNMEASURED :: a control failed (json=$LEGK_C1 comment=$LEGK_C2) or the roster read nothing.  ⚠ NO FALLBACK LIST IS USED -- a fallback is how a stale membership becomes permanent"
    fi
  fi
else
  stamp "LEG K CANARY DERIVATION UNMEASURED :: the 1.23.12 pin could not be established for the derivation"
fi
if [ "$LEGK_DERIVED_OK" != "1" ]; then
  stamp "  ^ LEG K FINDING: the reflect canary set could not be derived, so this train's reflect-bridge obligation is NOT discharged.  The two NON-canary rows below still run, because they are seat gates and not canaries."
  FAILED=1
fi
# the row set: the DERIVED canaries, then the `sync` gate a golib-corpus row owes, then the cost canary.  Deduplicated, and
# each row's PROVENANCE is stamped so a reader never has to guess why a row is here.
for p in $LEGK_CANARIES; do LEGK_ROWS="$LEGK_ROWS $p"; done
case " $LEGK_ROWS " in *" sync "*) : ;; *) LEGK_ROWS="$LEGK_ROWS sync" ;; esac
case " $LEGK_ROWS " in *" crypto/internal/nistec "*) : ;; *) LEGK_ROWS="$LEGK_ROWS crypto/internal/nistec" ;; esac
LEGK_ROWN=$(printf '%s\n' $LEGK_ROWS | grep -ac . || true)
stamp "LEG K starting :: $LEGK_ROWN row(s) [$LEGK_ROWS] :: freeGB=${FREE_K:-?} (the sweep's own floor is 25 GB and it is NOT bypassed -- no -IgnoreDiskPreflight) converterUsable=$CONVOK"
stamp "LEG K ⚠ BUDGET :: this is the LONGEST leg in the battery.  The derived canary set can include rows with per-package deadline FLOORS in the sweep's own \$longTimeouts table (crypto/tls 30m, net/http 60m, time 40m among them), so a full run here is measured in HOURS.  It is stated rather than discovered, and it is NOT given a skip switch: a switch that empties a gate is the lie-lever shape"
if [ "${CONVOK:-0}" != "1" ]; then
  stamp "LEG K UNMEASURED :: the converter binary LEG K would reuse is absent, stale against its own sources, or not the 1.24.13 build.  -SkipBuild reuses whatever is at src/go2cs/bin/go2cs.exe and asserts only that it EXISTS, so running anyway would measure an unknown binary against the corpus.  Named as UNMEASURED rather than skipped."
  FAILED=1
elif [ "${FREE_K:-0}" -lt 25 ]; then
  stamp "LEG K UNMEASURED :: ${FREE_K}GB free is under the sweep's own 25 GB floor -- it would refuse at its DISK PREFLIGHT and stamp a plausible-looking empty verdict (22 rows in 24 seconds is what that looks like).  Free space and re-run LEG K alone."
  FAILED=1
elif [ "${LEGK_ROWN:-0}" -lt 2 ]; then
  stamp "LEG K UNMEASURED :: the row set holds $LEGK_ROWN row(s), which cannot be right -- the `sync` gate and the cost canary are unconditional"
  FAILED=1
elif ! pin_go 1.23.12 "LEG-K"; then
  stamp "LEG K UNMEASURED :: the 1.23.12 pin could not be established, so the sweep would read the 1.24.13 GOROOT and its own toolchain guard would throw"
  FAILED=1
else
  LEGK_PROCESSED=0
  for pkg in $LEGK_ROWS; do
    LEGK_PROCESSED=$(( LEGK_PROCESSED + 1 ))
    ROWDOTS=$(printf '%s' "$pkg" | tr '/' '.')
    LK="$SCRIPT_DIR/coord-${TAG}-sweep-$ROWDOTS-$RUNID.log"
    LKEXTRA=""; LKPROV="DERIVED reflect canary (a go2cs-gen template change is reflect-bridge-touching by the canary rule's own wording)"
    if [ "$pkg" = "sync" ]; then LKEXTRA="-TestTimeout 20m"; LKPROV="THE GOLIB-CORPUS HAND-OWN SEAT GATE -- a hand-own retarget under src/core/sync emits nothing and is invisible to LEG D and CNR alike"; fi
    if [ "$pkg" = "crypto/internal/nistec" ]; then LKPROV="the DESCRIPTOR-SYNTHESIS COST CANARY -- its WALL is RECORDED and NOT judged"; fi
    # The banked figures are DERIVED FROM THE ROSTER IN THIS TREE at run time, never a literal.
    RB=$(awk -F'|' -v k="[\`$pkg\`](" 'NF>4 { f=$2; gsub(/^ +| +$/,"",f); if (index(f,k)==1) { gsub(/ /,"",$3); gsub(/ /,"",$4); print $3"+"$4; exit } }' docs/ValidatedTestPackages.md)
    t0=$(date +%s)
    # shellcheck disable=SC2086
    powershell -NoProfile -ExecutionPolicy Bypass -File src/run-validated-sweep.ps1 -Filter "$pkg" -Exact -SkipBuild $LKEXTRA < /dev/null > "$LK" 2>&1
    rc=$?; w=$(( $(date +%s) - t0 ))
    tr -d '\r' < "$LK" > "$LK.t" && mv "$LK.t" "$LK"
    REF=$(refusal_markers "$LK")
    # ⚠ the verdict grep is INDENTED.  The sweep indents its row line and prints its DRIFT report
    # AFTER it, so a column-0 anchor reads every passing row as a fault and a fixed tail is not an
    # instrument.  Absent, this says INSTRUMENT FAULT rather than defaulting to a verdict.
    VERD=$(grep -aE '^  (PASS|DISC|FAIL) ' "$LK" | tail -1 | sed 's/^  //' | cut -c1-180)
    SUMM=$(grep -aE '^sweep: ' "$LK" | tail -1 | cut -c1-140)
    ROWS=$(grep -acE '^  (PASS|DISC|FAIL) ' "$LK")
    CREC="src/core/$pkg/go2cs_test_comparison.json"
    OGV="ABSENT"
    [ -f "$CREC" ] && OGV=$(tr -d '\r' < "$CREC" | grep -aoE '"oracleGoVersion"[[:space:]]*:[[:space:]]*"[^"]*"' | head -1 | sed -E 's/.*"([^"]*)"$/\1/')
    [ -n "$OGV" ] || OGV="ABSENT"
    BASE_NOTE=""
    [ "$pkg" = "crypto/internal/nistec" ] && BASE_NOTE=" recordedBaseline=${NISTEC_BASELINE_S}s (REFERENCE ONLY -- taken on another day; this script does not judge a wall)"
    [ -n "$LKEXTRA" ] && BASE_NOTE="$BASE_NOTE extraArgs=[$LKEXTRA]"
    stamp "LEG K $pkg exit=$rc wall=${w}s rows=$ROWS rosterBanked=${RB:-unreadable} oracleGoVersion=[$OGV] (must be go1.23.12) :: provenance=$LKPROV${BASE_NOTE}${REF:+ refusalMarkers=[$REF]}"
    if [ -n "$REF" ]; then
      stamp "  ^ LEG K $pkg UNMEASURED: the sweep refused at its own preflight (markers above) -- an empty leg is never a green one.  'Toolchain pin' there means the 1.23.12 pin did not hold; 'Converter not built' means -SkipBuild found no binary; a DISK PREFLIGHT line means it refused at its own 25 GB floor and stamped a plausible-looking empty verdict."
      FAILED=1
    elif [ -z "$VERD" ]; then
      stamp "  ^ LEG K $pkg :: NO VERDICT LINE -- instrument fault.  Not defaulted to a verdict."
      FAILED=1
    else
      stamp "LEG K $pkg VERDICT :: $VERD"
      stamp "LEG K $pkg SUMMARY :: ${SUMM:-<no summary line>}"
      case "$VERD" in
        PASS*) : ;;
        *) stamp "  ^ LEG K $pkg is NOT a PASS at its banked count"; FAILED=1 ;;
      esac
      [ "$rc" = "0" ] || fail_gate LEG-K-row-exit
    fi
    case "$OGV" in
      ABSENT) stamp "  ^ LEG K $pkg: the comparison record carries NO oracleGoVersion (the field is omitempty, so absence means the probe did not answer).  The oracle's release is therefore UNMEASURED for this row, and an unmeasured oracle is not a verdict."; FAILED=1 ;;
      *) if legk_oracle_ok "$OGV"; then
           stamp "LEG K $pkg oracle OK :: oracleGoVersion reads [$OGV], which matches the recorded whole-line form $LEGK_ORACLE_RE.  ⚠ THE FIELD IS THE BARE \`go version\` OUTPUT (testConversion.go), NOT the bare release token -- train 44's run stamped both PASSING canary rows REFUSED on exactly that misreading, and control C6 above is what makes this predicate a fix rather than a different guess."
         else
           stamp "  ^ LEG K $pkg REFUSED: oracleGoVersion reads [$OGV] where the corpus is pinned to 1.23.12 and the field is the bare \`go version\` output.  The row compared THAT toolchain's tests against counts banked from 1.23.12 -- NOT MEASURED, never a verdict."
           FAILED=1
         fi ;;
    esac
    # PRESERVE the row's records BEFORE any restore -- keyed on the EXIT CODE, never on the printed
    # word, because the row whose record is most needed is often the one that never RAN.
    if [ "$rc" != "0" ] || [ -n "$REF" ] || [ -z "$VERD" ]; then
      for art in go2cs_test_comparison.json go2cs_test_results.json; do
        SRC="src/core/$pkg/$art"
        if [ -f "$SRC" ]; then
          DST="$(preserved_path "$pkg" "$art")"
          cp "$SRC" "$DST" && stamp "  preserved $SRC -> $DST" || stamp "  PRESERVE FAILED for $SRC"
        else
          stamp "  no $art at $SRC to preserve (the row may never have produced one)"
        fi
      done
    fi
    # restore AFTER preservation, per row -- the sweep rewrites corpus files AND the row's committed
    # proof page under docs/validation/current/.  ⚠ Restore BOTH roots: a corpus-scoped restore leaves
    # the proof pages behind and the next leg's dirt gate reads them as drift.
    git checkout -- src/core docs/validation 2>/dev/null
    git clean -fdq -- src/core 2>/dev/null
    D2=$(git status --porcelain | grep -c '^ D')
    stamp "  post-row restore :: dirty=$(git status --porcelain | wc -l) deleted-tracked=$D2 (must be 0)"
    [ "$D2" = "0" ] || { stamp "  ^ the restore DELETED TRACKED FILES"; FAILED=1; }
  done
  stamp "LEG K row loop :: processed=$LEGK_PROCESSED of $LEGK_ROWN listed (a child that ate the loop's stdin would swallow a row silently, so the count is stated)"
  [ "$LEGK_PROCESSED" = "$LEGK_ROWN" ] || { stamp "  ^ LEG K FINDING: the loop processed $LEGK_PROCESSED of $LEGK_ROWN rows"; FAILED=1; }
  pin_go 1.24.13 "POST-LEG-K" || stamp "note: could not restore the 1.24.13 pin after LEG K -- no leg follows it, so this is a stamp rather than a refusal"
fi
# ============================================================================================
stamp "CLOSING NOTES :: **THIS TEMPLATE MAKES NO PER-ROW CLOSING CLAIM, AND THAT IS A DELETION RATHER THAN AN OMISSION.**  Train 47 closed with two row-numbered notes: one asserting that its first row landed platform SCOPING for disclosure manifests and naming LEG U as that row's own seat gate, one asserting that its ninth row was cut from an OLDER base and that a per-row A-row<N> arm therefore read it THREE-DOT.  Both were facts about THAT table, and the rename would have carried both onto whatever rows this train happens to seat; this train's SEAT-CONTENT block is an empty SLOT and no arm here emits a per-row content stamp, so the per-row anchor those words name does not exist either.  A closing note is written AT SEAT FILL, beside the SEAT-CONTENT arm that measures the thing it claims, or it is not written at all.  ⚠ WHAT IS LOST BY THE DELETION IS NAMED: a seat cut from a base older than this train's still needs the THREE-DOT form for every read of it -- its class shape, its forbidden set and LEG D's per-seat prediction alike -- because a two-dot query against a base a seat forked before returns MASTER's own newer content REVERSED, and the seat-fill note must say so again."
stamp "OWED AND RUN :: LEG C the FULL converter suite at go1.24.13 (a REAL GATE this train -- three seats change src/go2cs) with LEG Cg's named-guard subset and its DERIVED floor, LEG D the two-seeded three-target -stdlib emission diff CORRECTED to the standalone's shape, LEG R the -tests convert-then-build of reflect AND errors at the merge result (the class NO standing gate compiles, reached here by a manual-conversion REGISTRY change), LEG 0 the PAIRING's cheapest positive control (E3'), LEG 1 integrity x3 GOOS with the registration arithmetic DERIVED AT TWO DEPTHS, LEG 2 go2cs-stdlib.slnx and LEG 2b go2cs.slnx (route #7's cross-assembly consumer gate for the src/gen seat), LEG 3 GolibTests at BOTH configurations with the ADMISSIBLE totals computed from the csproj and the DECLARED count reconciled against the BASE, LEG 4 CNR under the PAIRING expecting E1', LEG 5 the FULL behavioral suite expecting E2' with its OWN converter rebuild and its TRANSPILE PREDICATE asserted, LEG K the DERIVED reflect canary set plus the \`sync\` gate plus the nistec cost canary under the TWO-PIN shape."
stamp "⚠ WHAT THIS DERIVE RETIRED RATHER THAN CARRIED :: LEG 4b (an earlier train's OWN acceptance -- that seat LANDED, so the leg could only pass); LEG 4's NAMED Δ-drop diagnosis and the GOLDEN_PAIRS table it scored against (the alias fold makes the emission PIN-INDEPENDENT, so that signature can no longer be produced and a branch that cannot be taken is the warm-design trap); and train 45's A1..A9, replaced by this train's own A1..A7 rather than re-pointed.  Each retirement is DATED at its site and states what is LOST by it, because an assertion that can no longer fail is a green that cannot go red and an acceptance path that outlives its fault is a lie-lever."
stamp "⚠ WHAT THIS DERIVE FIXED IN THE INSTRUMENT RATHER THAN ACCEPTING AT THE LANDING :: (1) G10d now CONSULTS the \`-text\` ATTRIBUTE -- train 45's arm asserted CRLF over every docs/*.md 'under the eol=crlf pin' when docs/ is OUTSIDE that pin, refused a correct seat carrying the verbatim-bytes exemption, and the landing grew a named acceptance path for it.  The exemption here is BY ATTRIBUTE and CONDITIONAL on the index and worktree layers agreeing, the exemption COUNT is stamped beside the mismatch count, and a non-exempt LF docs file still refuses.  (2) LEG D is corrected to the standalone's shape in five named ways, of which the per-target BY-MECHANISM line comparison is this derive's own deliverable: train 45's \`cmp\` against a WINDOWS-derived committed hunk read MISSED on a linux target that had MET perfectly, because the same token change sat inside a flavour-specific field list."
stamp "STATED RATHER THAN GLOSSED :: NO GATE IN THIS BATTERY COMPILES A .cs.auto.  A review sibling in this train's delta, if any, is asserted by SHAPE and PROVENANCE in G10b and by nothing else, because nothing builds it and nothing runs it."
stamp "post-assembly dirty=$(git status --porcelain | wc -l) freeGB=$(freegb) (was ${FREE0} at the preflight)"
stamp "=== ${LABEL} ASSEMBLE DONE head=$(git rev-parse --short HEAD) base=$BASE route=$ROUTE seats=$SEATS_DONE (of $SEATS_LISTED listed; skippedPending=$SEATS_SKIPPED) requireAll=$REQUIRE_ALL seatShas=[$SEAT1_SHA $SEAT2_SHA $SEAT3_SHA $SEAT4_SHA $SEAT5_SHA $SEAT6_SHA $SEAT7_SHA $SEAT8_SHA $SEAT9_SHA $SEAT10_SHA $SEAT11_SHA $SEAT12_SHA $SEAT13_SHA $SEAT14_SHA $SEAT15_SHA $SEAT16_SHA $SEAT17_SHA $SEAT18_SHA] owed=[golib=$OWED_GOLIB gen=$OWED_GEN converter=$OWED_CONV behavioral=$OWED_BEH corpus=$OWED_CORPUS golibtests=$OWED_GT docs=$OWED_DOCS claudemd=$OWED_CLAUDEMD manifest=$OWED_MANIFEST tooling=$OWED_TOOLING] seatAssertArms=$SEATASSERT_N legD=$LEGD_VERDICTS legKrows=[$LEGK_ROWS] newBehavioralProjects=$NEWBEH newBehavioralProjectsAllDepths=$NEWBEH_ALL overallFailed=$FAILED ==="
stamp "nothing has been pushed; land separately once the readings are on the record."
exit $FAILED
