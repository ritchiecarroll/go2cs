#!/usr/bin/env python3
# ============================================================================================
# t47-derive.py -- DERIVE THE TRAIN 47 TEMPLATE FROM THE TRAIN 46 FILES.
#
# Run it with the Windows CPython on this box:   python t47-derive.py
# (`python3` here resolves to the WindowsApps stub and does nothing.)
#
# ⚠ EVERY BLOCK REPLACEMENT, EVERY LINE SUBSTITUTION AND EVERY GLOBAL SUBSTITUTION **ASSERTS IT
#   CHANGED SOMETHING** AND PRINTS WHAT IT DID.  A sed that matches nothing and a sed that does the
#   work exit the same way, and a derivation chain built on silent no-ops compounds across
#   generations -- this project has already paid for five generations of a mailbox tool derived that
#   way, each replacing a payload the previous one had never written.
#
# ⚠ IT DERIVES FIVE FILES, NOT ONE.  A defect fixed in the assembly and left standing in the land
#   script is a defect that survives the derive; the run records behind train 46 carry two shapes
#   (`case "${VAR:-0}" in ''` and `| grep -q`) that live in the smaller files.
#
# ⚠ IT WRITES ONLY `coord-train47-*` AND NEVER TOUCHES A TRAIN-46 FILE.  The train-46 assembly is a
#   RUNNING artifact (run 7 was live when this derive was written), and a derive that writes the
#   running train's scripts is the door that opens by itself: bash reads a script incrementally BY
#   BYTE OFFSET, so an insertion into a file a live chain is executing reparses its next command from
#   the middle of a line.  The refusal below is mechanical, not remembered.
# ============================================================================================
import io
import os
import re
import sys

SP = os.path.dirname(os.path.abspath(__file__))
NL = chr(10)

# ⚠ THE WRITE GUARD.  Named destinations only; anything else is refused before a byte is written.
ALLOWED_DST = {
    'coord-train47-assemble.sh',
    'coord-train47-rehearse.sh',
    'coord-train47-derive-selfcheck.sh',
    'coord-train47-land.sh',
    'coord-train47-land-dryread.sh',
}


class Doc(object):
    """One file under derivation.  Every operation asserts and reports."""

    def __init__(self, src):
        self.src = src
        self.path = os.path.join(SP, src)
        with io.open(self.path, encoding='utf-8', newline='') as f:
            text = f.read()
        self.lines = text.split(NL)
        if self.lines and self.lines[-1] == '':
            self.lines.pop()
        self.orig_n = len(self.lines)
        self.report = []
        self.ops = 0

    # ---- resolution -------------------------------------------------------------------------
    def find(self, pat, start=0, label=''):
        rx = re.compile(pat)
        for i in range(start, len(self.lines)):
            if rx.search(self.lines[i]):
                return i
        raise SystemExit('   ANCHOR NOT FOUND [%s / %s]: %s' % (self.src, label, pat))

    # ---- operations -------------------------------------------------------------------------
    def block(self, startpat, endpat, new, label, start_off=0, end_off=0, end_index=None):
        """Replace lines[a..b] INCLUSIVE with `new` (a string).
        ⚠ THE END ANCHOR IS SEARCHED FROM THE START ANCHOR + 1, NEVER FROM `a`.  With a negative
        start_off `a` sits BEFORE the anchor, and an end pattern that also matches the line at `a`
        resolves to a zero-length range -- a silent no-op wearing a replacement's clothes."""
        anchor = self.find(startpat, 0, label + '/start')
        a = anchor + start_off
        if end_index is not None:
            b = end_index + end_off
        else:
            b = self.find(endpat, anchor + 1, label + '/end') + end_off
        if b < a:
            raise SystemExit('   BAD RANGE [%s / %s]: %d..%d' % (self.src, label, a + 1, b + 1))
        newl = new.split(NL)
        if newl and newl[-1] == '':
            newl.pop()
        old = self.lines[a:b + 1]
        if old == newl:
            raise SystemExit('   NO-OP BLOCK [%s / %s]: the replacement equals what was there'
                             % (self.src, label))
        old_n = b - a + 1
        self.lines[a:b + 1] = newl
        self.ops += 1
        self.report.append('BLOCK   %-22s lines %5d..%-5d (%4d) -> %4d' % (label, a + 1, b + 1, old_n, len(newl)))
        return a

    def block_to_eof(self, startpat, new, label, start_off=0):
        a = self.find(startpat, 0, label + '/start') + start_off
        newl = new.split(NL)
        if newl and newl[-1] == '':
            newl.pop()
        old_n = len(self.lines) - a
        self.lines[a:] = newl
        self.ops += 1
        self.report.append('BLOCK   %-22s lines %5d..EOF  (%4d) -> %4d' % (label, a + 1, old_n, len(newl)))

    def subline(self, pat, newtext, label, want=1):
        """Replace WHOLE lines matching pat.  The replacement may carry embedded newlines."""
        rx = re.compile(pat)
        n = 0
        for i in range(len(self.lines)):
            if rx.search(self.lines[i]):
                if self.lines[i] == newtext:
                    raise SystemExit('   NO-OP SUBLINE [%s / %s]: line %d already reads the replacement'
                                     % (self.src, label, i + 1))
                self.lines[i] = newtext
                n += 1
        if n != want:
            raise SystemExit('   SUBLINE [%s / %s] matched %d line(s), wanted %d'
                             % (self.src, label, n, want))
        self.ops += 1
        self.report.append('LINE    %-22s %d line(s) replaced' % (label, n))

    def subline_contains(self, sub, newtext, label, want=1):
        """Replace WHOLE lines CONTAINING a literal substring.  Used wherever the target line carries
        shell/regex punctuation: building a Python regex to match a `grep -qE '^[[:space:]]*\\['`
        line is exactly the kind of escaping that collapses silently, and a substring cannot."""
        n = 0
        for i in range(len(self.lines)):
            if sub in self.lines[i]:
                if self.lines[i] == newtext:
                    raise SystemExit('   NO-OP SUBLINE-CONTAINS [%s / %s]: line %d already reads it'
                                     % (self.src, label, i + 1))
                self.lines[i] = newtext
                n += 1
        if n != want:
            raise SystemExit('   SUBLINE-CONTAINS [%s / %s] matched %d line(s), wanted %d'
                             % (self.src, label, n, want))
        self.ops += 1
        self.report.append('LINEC   %-22s %d line(s) replaced' % (label, n))

    def subline_next(self, anchorpat, pat, newtext, label, window=8):
        """Replace the FIRST line matching `pat` at or after a UNIQUE anchor, and REFUSE when it is
        further than `window` lines away.  ⚠ THAT DISTANCE CHECK IS THE POINT: several of the silent
        `FAILED=1` setters are byte-identical lines, so an ordinal or a bare pattern would retarget
        silently the moment anything above them moved.  An anchor plus a bounded window cannot."""
        i = self.find(anchorpat, 0, label + '/anchor')
        j = self.find(pat, i + 1, label + '/target')
        if j - i > window:
            raise SystemExit('   SUBLINE-NEXT [%s / %s]: the target is %d line(s) past its anchor '
                             '(window %d) -- the anchor drifted and this would retarget silently'
                             % (self.src, label, j - i, window))
        if self.lines[j] == newtext:
            raise SystemExit('   NO-OP SUBLINE-NEXT [%s / %s]' % (self.src, label))
        self.lines[j] = newtext
        self.ops += 1
        self.report.append('LINEN   %-22s line %5d (%d past its anchor)' % (label, j + 1, j - i))

    def gsub(self, old, new, label, minimum=1):
        n = 0
        for i in range(len(self.lines)):
            if old in self.lines[i]:
                n += self.lines[i].count(old)
                self.lines[i] = self.lines[i].replace(old, new)
        if n < minimum:
            raise SystemExit('   GSUB [%s / %s] replaced %d occurrence(s), wanted >= %d -- a '
                             'substitution that matches nothing is a SILENT NO-OP and a derivation '
                             'chain compounds it' % (self.src, label, n, minimum))
        self.ops += 1
        self.report.append('GSUB    %-22s %d occurrence(s)  [%s -> %s]' % (label, n, old[:34], new[:34]))

    def absent(self, tok, label):
        n = sum(l.count(tok) for l in self.lines)
        if n:
            for i, l in enumerate(self.lines):
                if tok in l:
                    print('     survivor line %d: %s' % (i + 1, l.strip()[:140]))
            raise SystemExit('   ABSENCE [%s / %s]: %d occurrence(s) of [%s] survive'
                             % (self.src, label, n, tok))
        self.ops += 1
        self.report.append('ABSENT  %-22s 0 occurrence(s) of [%s]' % (label, tok))

    def absent_in_code(self, tok, label):
        """The token must not survive on a NON-COMMENT line.  ⚠ A rule this file states in its own
        header necessarily SPELLS the token it forbids; a blanket absence check would then refuse the
        very sentence that documents the rule -- the route-#8 text-grepping family, met from the
        derive's side.  Comment hits are COUNTED AND PRINTED, because 'zero in code' and 'none
        anywhere' are different claims and only one of them is true."""
        code, cmt = [], 0
        for i, l in enumerate(self.lines):
            if tok in l:
                if l.lstrip().startswith('#'):
                    cmt += 1
                else:
                    code.append((i + 1, l.strip()[:130]))
        if code:
            for n, s in code:
                print('     survivor line %d: %s' % (n, s))
            raise SystemExit('   ABSENCE-IN-CODE [%s / %s]: %d live occurrence(s) of [%s]'
                             % (self.src, label, len(code), tok))
        self.ops += 1
        self.report.append('ABSENT  %-22s 0 in CODE, %d in comments  [%s]' % (label, cmt, tok))

    def count(self, pat):
        rx = re.compile(pat)
        return sum(1 for l in self.lines if rx.search(l))

    # ---- write ------------------------------------------------------------------------------
    def write(self, dst):
        if dst not in ALLOWED_DST:
            raise SystemExit('   WRITE REFUSED: [%s] is not a train-47 destination.  A derive that '
                             'can write the RUNNING train\'s scripts is the door that opens by '
                             'itself.' % dst)
        out = NL.join(self.lines) + NL
        path = os.path.join(SP, dst)
        with io.open(path, 'w', encoding='utf-8', newline='') as f:
            f.write(out)
        # ⚠ **THE DERIVE `bash -n`s WHAT IT WROTE, AND REFUSES.**  A replacement block whose text ends
        #   with a shell `"` immediately before the Python terminator loses that quote to the
        #   terminator -- the shell line then runs on into the next statement and the file does not
        #   parse.  It happened TWICE while this derive was written, in two different blocks, and
        #   neither is visible by reading either language on its own.  A derive that can emit an
        #   unparseable script is a derive whose next generation inherits it, so the check lives HERE
        #   rather than in whatever the operator remembers to run afterwards.
        rc = os.system('bash -n "%s"' % path.replace('\\', '/'))
        if rc != 0:
            raise SystemExit('   WROTE AN UNPARSEABLE SCRIPT: %s -- see the bash -n output above.  '
                             '(The usual cause is a block whose text ends in a shell quote directly '
                             'against the Python triple-quote terminator.)' % dst)
        return out.count(NL)


# ============================================================================================
# SHARED TEXT
# ============================================================================================
SETU_REASON = r"""# ⚠ `set -u` ONLY -- **NOT** `set -o pipefail`, AND THE REASON IS A MEASUREMENT.  Under pipefail a
#   `grep -q` that exits on its first match SIGPIPEs its producer, the pipeline's status becomes the
#   producer's death, and a TRUE match reads as a failure.  R measured that on this fleet.  This file
#   therefore carries ZERO `| grep -q` pipes: every one is a `grep -c` (or `grep -ac`) plus an
#   INTEGER TEST, which reads the same on a streaming producer and on a bounded one, and the
#   self-check greps for `| grep -q` and for `pipefail` and requires BOTH to read zero.
"""

STAMP_LIST = r"""# ⚠ THE EXACT STAMPS A CHAIN OR A WATCHER MAY KEY ON.  They are STAMPS, never WORDS: a waiter keyed
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
"""

LAUNCH_RULES = r"""# ⚠ LAUNCH SHAPE, AND THE THREE MECHANICS THE TRAIN-46 RUNS PAID FOR.
#   (1) **THE LAUNCH WRAPPER MUST END IN `exit $rc`.**  A wrapper whose last statement is a `tail`
#       (or any pipe) reports the LAST command's status, so a script that exited 1 is announced as 0.
#       Capture the real status as the FIRST statement after the run and exit on it:
#           bash coord-train47-assemble-run1.sh > <log> 2>&1; rc=$?; tail -40 <log>; exit $rc
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
"""

SECURITY_NOTE = r"""# ⚠ NO PROFILE PATH, ACCOUNT NAME, MACHINE NAME OR UNC PATH APPEARS IN THIS FILE.  The toolchain and
#   runtime roots are DERIVED from $HOME (overridable by name) and ASSERTED to exist; a literal would
#   be both a security-convention breach on any pushed surface and wrong on every other box.  The
#   assembly worktree is a FILL POINT like the base and the seat table, for the same reason plus a
#   sharper one: a train-47 script carrying train 46's worktree path would run inside a tree another
#   battery may be using.
"""


# ============================================================================================
# THE ASSEMBLY
# ============================================================================================
A = Doc('coord-train46-assemble.sh')

HEADER = r"""#!/bin/bash
# ============================================================================================
# TRAIN 47 -- A **TEMPLATE**.  SIX PLACEHOLDER SEAT ROWS AND A PLACEHOLDER BASE, DERIVED FROM
# coord-train46-assemble.sh WHILE TRAIN 46 WAS STILL IN FLIGHT.  Nothing here names a seat, because
# no seat is named yet: train 46 has not landed and this train's base is whatever master becomes.
#
# ⚠⚠ **THERE ARE THREE FILL POINTS AND EVERY ONE OF THEM REFUSES UNTIL IT IS FILLED.**  A template
#   that runs with placeholders in it is a battery measuring a tree nobody named, so each is a
#   REFUSAL rather than a default:
#     (F1) `WT=` -- the assembly worktree.  Reads `PENDING`; the run ABORTS (exit 2) until the
#          coordinator names the worktree this train assembles in.  ⚠ IT IS NOT INHERITED FROM THE
#          PREVIOUS TRAIN: a script pointing at a neighbour's worktree would run inside a tree
#          another battery may be holding, and "verify-only" describes a GIT effect, never an effect
#          on a neighbour.
#     (F2) `EXPECT_46=` -- the base.  Reads `PENDING`; the BASE ASSERTION ABORTS until it names the
#          SHA train 46 landed.  Every per-leg expectation, every class shape and LEG D's whole
#          prediction is derived FOR A BASE, and a gate reading has a tree: when the tree moves the
#          reading expires whether or not the change does.
#     (F3) `SEAT_TABLE` -- six rows reading `N|claude/PENDING-seatN|PENDING|<class>|tip`.  A PENDING
#          row is SKIPPED WITH A STAMP by default and ABORTS under TRAIN47_REQUIRE_ALL=1, which is
#          the shape a LANDING run takes.  The CLASS column is a placeholder too: the coordinator
#          overwrites ref, SHA and class together, and every class named below has a merge_seat arm.
#
# ⚠⚠ **THE SEVENTH COLUMN IS NEW AND IT IS OPTIONAL: `allowed=<ERE>`.**  A7 refuses any modification
#   of a committed behavioral file; a seat that legitimately RE-BASELINES a landed guard declares the
#   files it may modify in its own row, under a stated coordinator ruling, and A7 then admits EXACTLY
#   those -- and admits them only when the union's BLOB for each equals that seat's own blob, so the
#   exemption carries nothing another seat rides in on.  Train 46 carried the same idea as a
#   HARD-CODED path set and a hard-coded seat variable; here it is a property of the TABLE, so a
#   ruling is expressed where the seat lives and no code changes to express it.
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
#        `CLAUDE.md is UNTOUCHED ... seat 1 IS the doctrine batch` -- train 45's premise, carried by
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
""" + SECURITY_NOTE + r"""#
""" + SETU_REASON + r"""#
""" + STAMP_LIST + r"""#
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
#               a concurrent run holds the lock / an UNFILLED fill point / a PENDING seat under
#               TRAIN47_REQUIRE_ALL=1 / a filled seat SHA that does not resolve
#               3 a PREFLIGHT CONTROL failed (broken instrument -- nothing was assembled)
#               90 toolchain pin mismatch | 93 disk floor
#
""" + LAUNCH_RULES + r"""#     bash coord-train47-assemble.sh > <a log> 2>&1                       (PENDING rows SKIPPED)
#     TRAIN47_REQUIRE_ALL=1 bash coord-train47-assemble.sh > <a log> 2>&1 (a PENDING row ABORTS)
# ⚠ LAUNCH A **PER-RUN COPY** (coord-train47-assemble-run1.sh).  bash reads a script incrementally BY
# BYTE OFFSET, so an insertion above the running position reparses the next command from the middle of
# a line -- a train assembly died thirty minutes in on exactly that.  The labels derive from the
# basename, so a copy relabels itself and nothing is written out.
# ============================================================================================
"""
A.block(r'^#!/bin/bash$', r'^set -u$', HEADER, 'header', end_off=-1)

# ---- the pin roots, DERIVED and ASSERTED ---------------------------------------------------
PINS = r"""# --- THE TOOLCHAIN AND RUNTIME ROOTS, **DERIVED AND ASSERTED**, NEVER SPELLED. ----------------
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
"""
A.block(r"^PIN_GO_1_23_ROOT='", r"^PIN_DOTNET='", PINS, 'pin-roots')

A.subline(r"^export DOTNET_ROOT='",
          '# ⚠ DOTNET_ROOT is the WINDOWS form of the SAME derived root -- `cygpath -w` knows the mapping\n'
          '#   and the hand rule is only the fallback for a shell without it.  Two spellings of one root, never\n'
          '#   two roots: MSBuild materialises environment variables as properties and resolves property names\n'
          '#   case-INSENSITIVELY, which is how one POSIX block once carried two entries that folded into one.\n'
          'if command -v cygpath >/dev/null 2>&1; then DOTNET_ROOT_WIN=$(cygpath -w "$PIN_DOTNET"); else DOTNET_ROOT_WIN="$PIN_DOTNET"; fi\n'
          'export DOTNET_ROOT="$DOTNET_ROOT_WIN" MSBUILDDISABLENODEREUSE=1 CGO_ENABLED=0',
          'dotnet-root')

# ---- pin_go's WINDOWS-FORM root: derived, never spelled (security + one-copy-of-a-fact) -------
PINCASE = r"""  case "$want" in
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
  else case "$rootp" in /?/*) rootw="$(printf '%s' "$rootp" | cut -c2 | tr 'a-z' 'A-Z'):$(printf '%s' "$rootp" | cut -c3- | tr '/' '\\')" ;; *) rootw="$rootp" ;; esac; fi"""
A.block(r'^  case "\$want" in$', r'^  esac$', PINCASE, 'pin-windows-form')

# the PAIRING helper carried the SAME Windows-form literal a second time.  ONE derivation, two
# consumers: a profile path with an account name in it must not appear in this file, and the POSIX
# root already holds the fact -- a second spelling of one root is the thing that drifts.
PAIRROOT = r"""  rootp="$PIN_GO_1_23_ROOT"
  # ⚠ THE WINDOWS FORM IS DERIVED HERE TOO, for the two reasons pin_go states: a profile path
  #   carrying an account name must not appear in this file, and the POSIX root already holds the
  #   fact.  cygpath knows the mapping; the hand rule is the fallback for a shell without it.
  if command -v cygpath >/dev/null 2>&1; then rootw="$(cygpath -w "$rootp")"
  else case "$rootp" in /?/*) rootw="$(printf '%s' "$rootp" | cut -c2 | tr 'a-z' 'A-Z'):$(printf '%s' "$rootp" | cut -c3- | tr '/' '\\')" ;; *) rootw="$rootp" ;; esac; fi"""
A.subline_contains("rootw='C:", PAIRROOT, 'pairing-windows-form')

# ---- the worktree: FILL POINT F1 ------------------------------------------------------------
WTBLOCK = r"""# --- worktree, named in the first lines and no other accepted.  ⚠ **FILL POINT F1.** ----------
#     It reads PENDING and the run ABORTS until the coordinator names the worktree THIS train
#     assembles in.  It is deliberately NOT inherited from the previous train: a train-47 script
#     carrying train 46's worktree would run inside a tree another battery may be holding, and the
#     freeze binds the WORKTREE rather than the branch.  TRAIN47_WT overrides it for a control run,
#     and such a run says so in the stamp -- its verdicts are about this script, never about a train.
WT="${TRAIN47_WT:-PENDING}"
case "$WT" in
  PENDING|''|*PENDING*) echo "WORKTREE UNFILLED :: WT reads [$WT].  This is FILL POINT F1: name the assembly worktree for THIS train (or export TRAIN47_WT for a control run).  A template that runs with a placeholder in it is a battery measuring a tree nobody named -- ABORT"; exit 2 ;;
esac
[ -d "$WT" ] || { echo "WORKTREE [$WT] does not exist -- ABORT"; exit 2; }
WT_TOKEN="$(basename "$WT")"
cd "$WT" || { echo "cannot cd to the assembly worktree [$WT] -- ABORT"; exit 2; }
# ⚠ THE TOPLEVEL EXPECTATION IS DERIVED FROM $WT BY ONE RULE, NEVER WRITTEN TWICE.  msys spells a
#   drive `/c/...` while git answers `C:/...`; cygpath is the tool that knows the mapping.
if command -v cygpath >/dev/null 2>&1; then WT_TOP="$(cygpath -m "$WT" 2>/dev/null)"
else case "$WT" in /?/*) WT_TOP="$(printf '%s' "$WT" | cut -c2 | tr 'a-z' 'A-Z'):$(printf '%s' "$WT" | cut -c3-)" ;; *) WT_TOP="$WT" ;; esac; fi
[ "$(git rev-parse --show-toplevel | tr -d '\r')" = "$WT_TOP" ] || { echo "WRONG WORKTREE: git answers [$(git rev-parse --show-toplevel)] where the derivation from \$WT wants [$WT_TOP]"; exit 2; }
"""
A.block(r"^# --- worktree, named in the first lines", r'^\[ "\$\(cd "\$\(git rev-parse --show-toplevel\)"', WTBLOCK, 'worktree')

# ---- the ancestry constants: FILL POINT F2 ---------------------------------------------------
ANC = r"""# The landing this train's base is REQUIRED to contain, and the base itself.
# ⚠ **FILL POINT F2.**  EXPECT_46 is train 46's landing SHA and it is NOT KNOWN at derive time: train
#   46 was still in flight when this template was written.  It reads PENDING and the BASE ASSERTION
#   below ABORTS until it is filled.  EXPECT_45 is the ORDER pin -- a LANDED SHA that was already an
#   ancestor of origin/master when this was derived, so it cannot false-red and it pins the ordering.
EXPECT_45='44f858717'           # train 45's landing -- the ORDER pin (a landed, known ancestor)
EXPECT_46='PENDING'             # ⚠ FILL: the SHA train 46 landed; this train's BASE"""
A.block(r"^EXPECT_44='", r"^EXPECT_45='", ANC, 'ancestry')

A.subline(r'^for anc in "\$EXPECT_44:',
          'for anc in "$EXPECT_45:the ORDER pin (train 45\'s landing, a SHA that was already an ancestor of origin/master when this template was derived, so it cannot false-red)"; do',
          'ancestry-loop')

BASEASSERT = r"""BASE=$(git rev-parse --short HEAD)
# --- ⚠ THE BASE IS **ASSERTED**, NOT MERELY DERIVED, AND BOTH SHAS ARE PRINTED.  HEAD == origin/master
#     above says the tree is AT master; it does NOT say master is the master this script was derived
#     against.  A gate reading has a TREE, and when the tree moves the reading expires whether or not
#     the change does -- so every per-leg expectation, every class shape and LEG D's whole prediction
#     were derived for EXACTLY one base, and a different one means re-deriving rather than adjusting.
stamp "BASE ASSERTION :: this template's declared base EXPECT_46=$EXPECT_46 :: this run's BASE (== HEAD == origin/master) = $BASE :: origin/master right now reads $(git ls-remote origin refs/heads/master | cut -c1-9)"
case "$EXPECT_46" in
  PENDING|''|*PENDING*)
    stamp "  ^ BASE REFUSED: EXPECT_46 reads [$EXPECT_46].  This is FILL POINT F2 -- the base is train 46's LANDING SHA and it was not known when this template was derived.  Fill it, and RE-READ every expectation against it: a template that runs with a placeholder base measures a tree nobody named -- ABORT"
    exit 2 ;;
esac
case "$BASE" in
  "$EXPECT_46"*) : ;;
  *) stamp "  ^ BASE REFUSED: this script declares $EXPECT_46 and the tree is at $BASE.  Every expectation, class shape and prediction in it was derived for that base -- RE-DERIVE, do not adjust -- ABORT"; exit 2 ;;
esac"""
A.block(r'^BASE=\$\(git rev-parse --short HEAD\)$', r'^esac$', BASEASSERT, 'base-assertion')

# ---- fail_gate, the named-setter helper ------------------------------------------------------
A.subline(r'^FAILED=0$',
          'FAILED=0\n'
          '# --- ⚠ EVERY `FAILED=1` NAMES ITS GATE.  Train 46 carried FIFTEEN setters with no stamp on the\n'
          '#     same or the previous line, and a reader scanning for the flag could not tell which gate had\n'
          '#     set it; one of them cost a run its attribution.  A setter that names its gate costs one word.\n'
          'fail_gate(){ FAILED=1; stamp "  ^ FAILED=1 set by gate [$1]"; }',
          'fail-gate-helper')

# ============================================================================================
# THE SEATS BLOCK -- table, preflight, notes, and the alias data the later legs read
# ============================================================================================
SEATS = r"""# --- THERE IS NO ENV SEAT IN THIS TRAIN.  A mandatory-env slot for a seat nobody names is a refusal
#     waiting to fire on the WRONG REASON, so the mechanism is absent rather than unset.  Every row
#     the coordinator has not yet named is a PENDING row in the table, which is a different and
#     VISIBLE mechanism.
#
# --- A PENDING ROW IS **SKIPPED WITH A STAMP** BY DEFAULT, AND THAT IS THE TEMPLATE'S SHAPE.
#     This file is derived with SIX placeholder rows, so the default has to be the one that lets a
#     partial run be internally consistent rather than silently wrong.  The refusal lives where it
#     belongs:
#         TRAIN47_REQUIRE_ALL=1   a PENDING row ABORTS by name -- what a LANDING run of the full
#                                 table passes, and the only shape in which the assembled tree is
#                                 the train the coordinator announced.
#     ⚠ Declaredness and emptiness are tested SEPARATELY: a DECLARED-BUT-EMPTY TRAIN47_REQUIRE_ALL is
#     REFUSED, because ${X:-0} reads an empty override as unset and a blank authorisation would
#     silently become "no".
REQUIRE_ALL=0
if [ "${TRAIN47_REQUIRE_ALL+x}" = "x" ]; then
  if [ -z "${TRAIN47_REQUIRE_ALL:-}" ]; then
    stamp "TRAIN47_REQUIRE_ALL is DECLARED but EMPTY.  An empty authorisation is not 'unset' and must not be read as either answer -- name it or do not declare it -- ABORT"
    exit 2
  fi
  case "$TRAIN47_REQUIRE_ALL" in
    1) REQUIRE_ALL=1
       stamp "TRAIN47_REQUIRE_ALL=1 :: any row whose SHA reads PENDING will refuse by name.  This is the LANDING shape: it asserts that every row the coordinator announced has been filled, and it is the only shape in which the assembled tree is the full train." ;;
    0) REQUIRE_ALL=0
       stamp "TRAIN47_REQUIRE_ALL=0 :: stated explicitly; an unfilled row is SKIPPED WITH A STAMP and this run is a PARTIAL train that says so in every count" ;;
    *) stamp "TRAIN47_REQUIRE_ALL reads [$TRAIN47_REQUIRE_ALL], which is neither 0 nor 1 -- an authorisation this script cannot read is not an authorisation -- ABORT"; exit 2 ;;
  esac
else
  stamp "TRAIN47_REQUIRE_ALL is not in the environment :: an unfilled row is SKIPPED WITH A STAMP (the default, because this template was derived with SIX placeholders).  ⚠ A LANDING RUN SETS TRAIN47_REQUIRE_ALL=1, and the land script's own seat-count check is the second half of that."
fi

# ============================================================================================
# THE SEAT TABLE -- ⚠ **FILL POINT F3**.
# ONE place holds the MERGE ORDER, the seat NUMBER, the ref, the required SHA, the CLASS, the TIP
# MODE and -- optionally -- the ALLOWED set.  The loop below asserts processed == listed, so a child
# that eats the loop's stdin cannot swallow a seat and report a clean run (which is why the table is
# fed on FD 3 and every child reads /dev/null).
#
#   FIELDS:  N | ref | SHA | class | tipmode [| allowed=<ERE>]
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
#     allowed=  OPTIONAL.  An ERE naming committed behavioral files this seat may MODIFY, under a
#               coordinator ruling stated at the row.  A7 admits EXACTLY those paths and only when
#               the union's BLOB for each equals this seat's own blob -- so the exemption carries
#               nothing another seat rides in on.  Absent means A7's strict form: NO committed
#               behavioral file may be modified or deleted except the four MSTest classes.
#               ⚠ IT IS A RULING, NOT A CONVENIENCE.  Write the ruling in the seat's own note.
SEAT_TABLE="1|claude/PENDING-seat1|PENDING|converter|tip
2|claude/PENDING-seat2|PENDING|converter-test+docs|tip
3|claude/PENDING-seat3|PENDING|golib|tip
4|claude/PENDING-seat4|PENDING|golib-corpus-handown|tip
5|claude/PENDING-seat5|PENDING|manifest|tip
6|claude/PENDING-seat6|PENDING|doctrine|tip"

# The SHAs and refs later legs need BY NAME are DERIVED FROM THE TABLE, never retyped: the table is
# the one place that holds a seat's ref and pin, and a second copy of a SHA is the thing that drifts
# when a seat is re-pinned.
seat_field(){ printf '%s\n' "$SEAT_TABLE" | awk -F'|' -v n="$1" -v c="$2" '$1==n{print $c}'; }
SEAT1_SHA=$(seat_field 1 3);  SEAT2_SHA=$(seat_field 2 3);  SEAT3_SHA=$(seat_field 3 3)
SEAT4_SHA=$(seat_field 4 3);  SEAT5_SHA=$(seat_field 5 3);  SEAT6_SHA=$(seat_field 6 3)
SEAT1_REF=$(seat_field 1 2);  SEAT2_REF=$(seat_field 2 2);  SEAT3_REF=$(seat_field 3 2)
SEAT4_REF=$(seat_field 4 2);  SEAT5_REF=$(seat_field 5 2);  SEAT6_REF=$(seat_field 6 2)
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
[ "$TBL_BAD" = "0" ] || { stamp "SEAT TABLE ROWS ARE STRUCTURALLY UNSOUND -- NOTHING has been merged -- ABORT"; exit 2; }
stamp "SEAT TABLE ROWS ARE STRUCTURALLY SOUND :: numbers contiguous, refs unique, filled SHAs unique, every row five fields or more"

# --- THE ALLOWED SET, READ OUT OF THE TABLE'S OPTIONAL SIXTH FIELD.  A7 consumes it; nothing else
#     does.  It is stamped HERE, before a single merge, so a reader meets the ruling before the
#     assembly acts on it -- an exemption nobody can see is an exemption nobody re-reads.
ALLOWED_ROWS=$(printf '%s\n' "$SEAT_TABLE" | awk -F'|' '$6 ~ /^allowed=/ {printf "%s ", $1}')
ALLOWED_N=$(printf '%s\n' "$SEAT_TABLE" | awk -F'|' '$6 ~ /^allowed=/' | grep -ac . || true)
if [ "${ALLOWED_N:-0}" = "0" ]; then
  stamp "A7 ALLOWED SET :: NO row declares an \`allowed=\` set, so A7 runs in its STRICT form -- no committed behavioral file may be MODIFIED or DELETED except the four BehavioralTests classes.  That is the default and it is the state a reader should expect."
else
  stamp "A7 ALLOWED SET :: $ALLOWED_N row(s) [$ALLOWED_ROWS] declare an \`allowed=\` set.  ⚠ EACH IS A COORDINATOR RULING: a re-baselined golden is a RULING and never a merge.  A7 admits EXACTLY those paths AND only when the union's blob for each equals that seat's OWN blob, so the exemption cannot carry another seat's change."
  printf '%s\n' "$SEAT_TABLE" | awk -F'|' '$6 ~ /^allowed=/ {printf "      row %s (%s) allows: %s\n", $1, $2, substr($6, 9)}' | tee -a "$ASMLOG"
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
  case "${pa:-}" in
    allowed=*) [ "$ps" != "PENDING" ] || { stamp "  ^ SEAT PREFLIGHT REFUSED: row $pn declares an \`allowed=\` set while its SHA reads PENDING -- a ruling about a seat that does not exist yet."; PLACEBAD=1; } ;;
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

if [ "$PENDING_N" != "0" ]; then
  if [ "$REQUIRE_ALL" = "1" ]; then
    stamp "SEAT TABLE REFUSED :: $PENDING_N row(s) read PENDING [$PENDING_ROWS] and TRAIN47_REQUIRE_ALL=1 demands the full train.  Fill the table, or drop the switch and accept a PARTIAL train that says so -- ABORT"
    exit 2
  fi
  stamp "⚠⚠ SEAT TABLE :: $PENDING_N row(s) read PENDING [$PENDING_ROWS] and will be SKIPPED WITH A STAMP.  THIS RUN IS A PARTIAL TRAIN.  Every count below -- SEATS_DONE, the OWED vector, the new behavioral projects, LEG 1's registration delta, LEG 5's enumeration, LEG D's prediction and LEG K's row set -- is DERIVED at run time, so this run's numbers are internally consistent; they are simply not the announced train's."
fi

# --- THE SEAT NOTES ARE **DERIVED**, NOT WRITTEN.  Train 46's notes named each seat's deliverable in
#     prose, which is a fact about that train and the stale-figure class the moment a row is re-pinned.
#     What a template can honestly stamp is what the TABLE says, plus what the CLASS implies.
printf '%s\n' "$SEAT_TABLE" | awk -F'|' '{printf "SEAT %s NOTE :: %s @%s :: class %s, tip mode %s%s\n", $1, $2, $3, $4, $5, ($6 ~ /^allowed=/ ? " :: carries an A7 ALLOWED ruling" : "")}' | while IFS= read -r ln; do stamp "$ln"; done

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
"""
A.block(r"^# --- THERE IS NO ENV SEAT IN THIS TRAIN\.", r'^ALIAS_NEW=', SEATS, 'seats')

# ---- the class case block --------------------------------------------------------------------
CLASSES = r"""  case "$cls" in
    # A CONVERTER + BEHAVIORAL GUARD seat: converter source, the solution file, a behavioral guard
    # project (which may carry NESTED sub-libraries), the four MSTest classes, and a finding record.
    # ⚠ THE BEHAVIORAL ALTERNATIVE IS `[A-Za-z0-9]+/.*` RATHER THAN A NAMED PROJECT, because a guard
    #   may carry sub-libraries under its own directory and a per-project literal would have to be
    #   re-derived for every future seat -- while `BehavioralTests/[^/]+\.cs` stays a SEPARATE
    #   alternative so the four MSTest classes are admitted by name and nothing else under that
    #   directory is.  The forbidden list keeps src/core and src/gen: a converter-guard seat must
    #   never touch the corpus or the generator.
    converter-guard)      forbidden='src/core src/gen src/utilities'; shapepat='^(src/go2cs/[^/]+|src/go2cs\.slnx|src/tests/Behavioral/[A-Za-z0-9]+/.*|src/tests/Behavioral/BehavioralTests/[^/]+\.cs|docs/phase4/[^/]+\.md)$' ;;
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
    *)           stamp "seat $n: unknown class '$cls' -- ABORT"; return 1 ;;
  esac"""
A.block(r'^  case "\$cls" in$', r'^  esac$', CLASSES, 'classes')

# ---- the seat loop, the OWED vector, the optional union fix, the counts ------------------------
LOOP = r"""SEATS_LISTED=$(printf '%s\n' "$SEAT_TABLE" | grep -c .)
SEATS_DONE=0
SEATS_SKIPPED=0
SKIPPED_NAMES=""
MERGED_CLASSES=""
while IFS='|' read -r sn sb ss sc sm sa <&3; do
  [ -n "$sn" ] || continue
  [ -n "$sm" ] || { stamp "seat $sn: the seat table row has no TIP MODE field -- the table and the loop have drifted apart -- ABORT"; exit 1; }
  if [ "$ss" = "PENDING" ]; then
    if [ "$REQUIRE_ALL" = "1" ]; then
      stamp "seat $sn ($sc) $sb :: **PENDING** and TRAIN47_REQUIRE_ALL=1 -- the coordinator has not named this seat's tip, so this is not the announced train.  Fill the table row, or drop the switch -- ABORT"
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
OWED_GOLIB=0; OWED_GEN=0; OWED_CONV=0; OWED_BEH=0; OWED_DOCS=0; OWED_CLAUDEMD=0; OWED_MANIFEST=0; OWED_CORPUS=0; OWED_GT=0
for c in $MERGED_CLASSES; do
  case "$c" in golib|golib-gen|golib-corpus-handown|golib-converter-docs) OWED_GOLIB=1; OWED_GT=1 ;; esac
  case "$c" in golib-gen) OWED_GEN=1 ;; esac
  case "$c" in converter|converter-guard|converter-test+docs|golib-gen|golib-corpus-handown|golib-converter-docs) OWED_CONV=1 ;; esac
  case "$c" in converter-guard|golib-gen) OWED_BEH=1 ;; esac
  case "$c" in doctrine) OWED_CLAUDEMD=1 ;; esac
  case "$c" in manifest) OWED_MANIFEST=1 ;; esac
  case "$c" in converter|golib-corpus-handown|manifest) OWED_CORPUS=1 ;; esac
  case "$c" in docs|converter|converter-guard|converter-test+docs|golib|golib-corpus-handown|golib-converter-docs) OWED_DOCS=1 ;; esac
done
stamp "OWED VECTOR :: golib=$OWED_GOLIB gen=$OWED_GEN converter=$OWED_CONV behavioral=$OWED_BEH corpus=$OWED_CORPUS golibtests=$OWED_GT docs=$OWED_DOCS claudemd=$OWED_CLAUDEMD manifest=$OWED_MANIFEST (DERIVED from the CLASSES of the $SEATS_DONE merged row(s): [$MERGED_CLASSES]) -- every justification arm below reads THIS vector, and so does the land script, so the two cannot drift"
# ⚠ `docs` IS A **SOFT** OWE AND IS STAMPED AS ONE.  A class may or may not carry a record, so the
#   docs arm below reads the vector as "a record is EXPECTED" rather than "a record is REQUIRED", and
#   a zero is checked against the skip count instead of refusing outright.

# --- THE UNION FIX -- **OPTIONAL**, AND ITS ABSENCE IS STATED RATHER THAN SILENT. --------------
#     ⚠ WHY IT EXISTS AT ALL.  Train 46 run 4 died forty minutes into its battery at LEG C: two edits
#     to ONE `_test.go` -- one seat's new call sites, another train's widened signature -- merged
#     CLEAN BY CONTENT and the package failed to compile ONLY AT THE UNION.  Such a fix cannot live
#     on a seated branch (neither side is wrong alone), so it is an ASSEMBLY commit stated as the
#     train's own.  ⚠ AND THE REHEARSAL IS NOW WHERE IT IS FOUND: coord-train47-rehearse.sh runs
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
"""
A.block(r'^SEATS_LISTED=\$\(printf', r'^stamp "NEW behavioral projects in this train', LOOP, 'loop')

# ---- A1..A4 become the SEAT-CONTENT ASSERTION SLOT ----------------------------------------------
SEATASSERT = r"""# --- ⚠ **SEAT-CONTENT ASSERTIONS -- A FILL POINT WITH A GATE.** -------------------------------
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
# === SEAT-CONTENT ASSERTIONS END ===
stamp "SEAT-CONTENT ASSERTIONS :: arms=$SEATASSERT_N :: seats merged=$SEATS_DONE (an arm per merged seat is the bar; the arms read each seat's own content out of the FILES, which is what catches a merge that resolved a hunk the wrong way -- ancestry cannot)"
if [ "$SEATASSERT_N" -lt "$SEATS_DONE" ]; then
  stamp "  ^ SEAT-CONTENT ASSERTIONS REFUSED: $SEATASSERT_N arm(s) for $SEATS_DONE merged seat(s).  A battery over a tree whose seats nobody asserted measures a tree nobody ruled on -- write the arms in the fenced block above, one per seat, each gated on its own \$SEATn_SHA != PENDING."
  fail_gate SEAT-CONTENT-ASSERTIONS
fi

"""
A.block(r"^# --- A1 SEAT 1's CONVERTER FIX AND ITS GUARD",
        r'^# --- A5 THE NEW BEHAVIORAL GUARD PROJECTS', SEATASSERT, 'seat-assert-slot', end_off=-1)

# ---- A7, generalised onto the table's sixth field ------------------------------------------------
A7 = r"""# --- A7 NO SEAT MAY MOVE A COMMITTED BEHAVIORAL GOLDEN THAT ALREADY EXISTED --------------------
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
  case "${aa:-}" in allowed=*) : ;; *) continue ;; esac
  [ "$as" != "PENDING" ] || continue
  apat="${aa#allowed=}"
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
"""
A.block(r"^# --- A7 NO SEAT MAY MOVE A COMMITTED BEHAVIORAL GOLDEN",
        r'^\[ "\$FAILED" = "0" \] \|\| \{ stamp "=== A-ASSERTIONS FAILED', A7, 'A7', end_off=-1)

# ---- the DOCS-DELTA zero branch ------------------------------------------------------------------
DOCSZERO = r"""  stamp "DOCS-DELTA :: 0 docs/*.md files in this train's delta.  ⚠ WHETHER THAT IS A FINDING IS DECIDED BY THE **OWED VECTOR**, NOT BY A CARRIED PREMISE: OWED_DOCS=$OWED_DOCS (1 means at least one merged seat's CLASS is one that normally carries a record).  Train 45's arm refused unconditionally because four of its seats were docs seats -- a fact about that train -- and train 46 checked it against the skip count.  Here it is checked against the vector AND the skip count, which is the honest pair: a class may legitimately carry no record, so this is a FINDING to reconcile rather than a refusal."
  if [ "$OWED_DOCS" = "1" ] && [ "$SEATS_SKIPPED" = "0" ]; then
    stamp "  ^ DOCS-DELTA FINDING: no seat was skipped, at least one merged class normally carries a record, and yet ZERO docs/*.md are in the delta.  Read the seat notes: either a seat did not land its record, or a class that does not owe one is doing the work -- and the second is a note to make, not a silence to keep."
    fail_gate DOCS-DELTA
  fi"""
A.block(r"^  stamp \"DOCS-DELTA :: 0 docs/\*\.md files in this train's delta",
        r'^  \[ "\$SEATS_SKIPPED" -ge 1 \]', DOCSZERO, 'docs-delta-zero')

A.subline(r'^      docs/GoCorpusMigration\.md\|docs/DotNetMigration\.md\) lim=30;',
          '      docs/GoCorpusMigration.md|docs/DotNetMigration.md) lim=30; kind="RUNBOOK (living procedure, amended in-stage; a step rewrite IS an amendment there, which is why its allowance is larger)" ;;',
          'docs-runbook-kind')

# ---- G4, derived from the OWED vector -------------------------------------------------------------
G4 = r"""# --- G4 CLAUDE.md STRUCTURAL CHECK -- ⚠ ITS EXPECTATION IS **DERIVED**, NOT CARRIED. ----------
#     Train 46 run 3 refused here with "CLAUDE.md is UNTOUCHED ... seat 1 IS the doctrine batch" --
#     train 45's premise, carried into a train that had no doctrine seat.  A premise about which seat
#     is the doctrine batch is a fact about ONE train; what a template can know is whether any MERGED
#     row carries the class `doctrine`.  So:
#         OWED_CLAUDEMD=1  CLAUDE.md MUST be in the delta, and the structural arms run with teeth.
#         OWED_CLAUDEMD=0  CLAUDE.md must NOT be in the delta.
#     BOTH readings are stamped and only the MISMATCH refuses -- an expectation that cannot be wrong
#     in one direction is half a gate.
G4TOUCHED=$(git diff --name-only "$BASE" HEAD -- CLAUDE.md | grep -ac . || true)
stamp "G4 expectation DERIVED :: OWED_CLAUDEMD=$OWED_CLAUDEMD (a merged row carries class \`doctrine\`) :: CLAUDE.md in this train's delta=$G4TOUCHED :: they must AGREE"
if [ "$OWED_CLAUDEMD" = "1" ] && [ "${G4TOUCHED:-0}" = "0" ]; then
  stamp "  ^ G4 REFUSED: a merged seat carries class \`doctrine\` and CLAUDE.md is UNTOUCHED -- that seat did not land what its class says it is."
  fail_gate G4-expectation
elif [ "$OWED_CLAUDEMD" = "0" ] && [ "${G4TOUCHED:-0}" != "0" ]; then
  stamp "  ^ G4 REFUSED: CLAUDE.md is in the delta and NO merged seat carries class \`doctrine\`.  A doctrine edit riding in on another class is an edit nobody ruled on, and the class shapes are what should have refused it."
  fail_gate G4-expectation
fi
if [ "${G4TOUCHED:-0}" != "0" ]; then
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
"""
A.block(r'^# --- G4 CLAUDE\.md STRUCTURAL CHECK', r'^# --- G6 coarse census', G4, 'G4', end_off=-2)

A.subline(r'^stamp "G6 coarse census ::',
          'stamp "G6 coarse census :: detectorHits=$RAWH (STAMPED, never asserted -- ⚠ NO derive-time reading is carried in this TEMPLATE because no seat was named when it was written, so this number is a READING to reconcile against the seats and never a verdict) templateExcluded=$G6TPL (PRINTED: Sprintf-placeholder and detector-literal lines, the TEMPLATE class ruled 2026-09-08 16:15 after a run read them as residual) residual=$HITS (must be 0)"',
          'g6-stamp')

A.subline(r"^# --- G10a SEAT 10's BOARD, ASSERTED ON ITS \*\*STRUCTURAL GUARD\*\*",
          "# --- G10a THE BOARD'S OWN STRUCTURAL GUARD (it runs whether or not a seat touches the file) ----",
          'g10a-title')

# ---- G11(a), derived from the OWED vector ---------------------------------------------------------
G11A = r"""# --- G11(a) ⚠ **THE JUSTIFICATION IS DERIVED FROM THE OWED VECTOR, NEVER FROM A DIRECTORY LITERAL.**
#     Train 46 run 6 read `G11(a) JUSTIFICATION FALSE` on a healthy tree because the arm required
#     `src/core/syscall/windows` in the delta -- train 45's premise, and a fact about a train that
#     happened to have a syscall seat.  The vector above says what THIS train's merged CLASSES owe;
#     every arm below reads it, and every directory count is printed as a READING beside it.
G11ADIRS='src/core src/core/golib src/core/syscall/windows src/tests src/tests/GolibTests src/go2cs src/gen src/utilities docs CLAUDE.md'
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
g11a_arm "$OWED_CLAUDEMD" "$(git diff --name-only "$BASE" HEAD -- CLAUDE.md | grep -ac . || true)" 'CLAUDE.md' 'G4 runs with teeth: a batch is a PURE INSERTION, so zero deletions, zero table lines and the BOM state preserved'
stamp "G11(a) syscall/windows=$G11SYS -- a READING ONLY, and named here because a previous train REQUIRED it and refused a healthy tree for it.  A train with no syscall seat legitimately reads 0."
[ "$G11ABAD" = "0" ] || fail_gate G11a
"""
A.block(r"^G11ADIRS='", r'^fi$', G11A, 'G11a')

# ---- chain_stop messages, generalised ---------------------------------------------------------
A.subline(r'^  chain_stop "LEG 2 \(go2cs-stdlib\.slnx\)"',
          '  chain_stop "LEG 2 (go2cs-stdlib.slnx)" "the corpus does not compile at this train\'s union (OWED vector golib=$OWED_GOLIB corpus=$OWED_CORPUS), so nothing below can be read as a statement about this train." "$LEGS_ALL"',
          'leg2-chainstop')
A.subline(r'^  chain_stop "LEG 2b \(go2cs\.slnx\)"',
          '  chain_stop "LEG 2b (go2cs.slnx)" "the non-generated solution members do not compile at this train\'s union.  ⚠ THIS IS ROUTE #7\'s NAMED GATE: a src/gen change (OWED gen=$OWED_GEN) is invisible to CNR (transpile-only) and to the stdlib solution (one assembly at a time), so this build and LEG 5\'s COMPILE phase are the only legs that compile a CROSS-ASSEMBLY consumer of the generated shell." "LEG D two-seeded three-target -stdlib emission diff | LEG R -tests convert-then-build | LEG 3 GolibTests x2 | LEG 4 CNR | LEG 5 FULL behavioral suite | LEG K derived canary set + sync + nistec"',
          'leg2b-chainstop')

# ---- LEG 3's train-specific tail --------------------------------------------------------------
SKIPDELTA = ('[ "$(( ${SD:-0} - ${SR:-0} ))" = "3" ] || { stamp "  ^ SKIP DELTA != 3 -- one '
             'configuration under-measured the GC/pin-liveness class, which RUNS at Release and '
             'SELF-SKIPS at Debug (a non-optimizing frame would root its temporaries and make the '
             'assertion unfalsifiable).  ⚠ ADJUDICATE against THIS TRAIN\'S OWN NEW ARMS before '
             'reading it as a regression: a NEW GolibTests method that self-skips at one '
             'configuration MOVES this delta, and the files this train adds or modifies are listed '
             'by the delta arm below -- a TEMPLATE cannot name them.  3 is the number measured on a '
             'tree with new methods in it; the raw skip counts are stamped above and the '
             'DECLARED-count reconciliation says how many arms this train added."; '
             'fail_gate LEG-3-skip-delta; }')
A.subline_contains('SKIP DELTA != 3', SKIPDELTA, 'leg3-skipdelta')

# ---- the LEG 3 prose that still names train 46's own seats (the stale-figure class: a sentence
#      naming a seat describes nothing the moment the seat is re-pinned, and a TEMPLATE has none) --
A.subline_contains("#   file grows.  THIS train has TWO seats adding arms (seat 2 extends ArrayRangeAllocationTests.cs,",
                   "#   file grows.  A TEMPLATE cannot name its seats at all, so what is asserted instead is a RELATION",
                   'leg3-hdr-1')
A.subline_contains("#   seat 3 adds FatalReportTests.cs), so what is asserted instead is a RELATION whose two sides are",
                   "#   whose two sides are not both this tree; the arms this train adds are DERIVED from its own delta:",
                   'leg3-hdr-2')
A.subline_contains("#     stale-figure class the moment either file grows.  Train 46 has TWO seats adding GolibTests",
                   "#     stale-figure class the moment either file grows.  Whether THIS train adds GolibTests arms at",
                   'leg3-hdr-3')
A.subline_contains("#     content (seat 2 extends ArrayRangeAllocationTests.cs, seat 3 adds FatalReportTests.cs), so the",
                   "#     all is the OWED vector's to say and the delta's to measure, so the",
                   'leg3-hdr-4')

A.subline(r'^\[ "\$\{GT_DELTA_TOTAL:-0\}" -ge 1 \]',
          '[ "${GT_DELTA_TOTAL:-0}" -ge 1 ] || { stamp "  ^ LEG 3 NOTE: this train\'s delta adds ZERO GolibTests arms.  ⚠ WHETHER THAT IS A FINDING IS THE **OWED VECTOR\'s** TO SAY: OWED_GT=$OWED_GT.  A zero with OWED_GT=1 and nothing skipped means a golib-class seat did not land its arms; a zero with OWED_GT=0 is simply what a train without a golib seat reads. skippedPending=$SEATS_SKIPPED"; { [ "$OWED_GT" = "0" ] || [ "$SEATS_SKIPPED" -ge 1 ]; } || fail_gate LEG-3-arms; }',
          'leg3-arms-tail')
A.subline(r'^for arm in FatalReport ArrayRangeAllocation GoValueClone StorageKind; do',
          '# ⚠ THE ARM NAMES ARE **DERIVED FROM THE DELTA**, never listed.  A carried list names the previous\n'
          '#   train\'s arms and reads zero forever; the files this train added or modified are what a reader\n'
          '#   wants pointed at.  A passing MSTest run names only FAILURES, so ZEROS here are normal.\n'
          'for arm in $(printf \'%s\\n%s\\n\' "$GT_NEWFILES" "$GT_MODFILES" | grep -a . | sed -E \'s#.*/##; s#\\.cs$##\' | sort -u); do',
          'leg3-arm-mentions')

# ---- LEG 5 / LEG 0 stamps that name train-46 seats ---------------------------------------------
A.subline(r'^  stamp "  \^ LEG 5 FINDING: \$L5N project\(s\) failed',
          '  stamp "  ^ LEG 5 FINDING: $L5N project(s) failed, where E2\' expects NONE.  EVERY member is a finding BY NAME -- there is no absorbed set.  ⚠ WHERE TO LOOK, DERIVED FROM THE OWED VECTOR RATHER THAN FROM A CARRIED SEAT ORDER: gen=$OWED_GEN first (a generator template is invisible to CNR and to the stdlib solution alike, and THIS phase is route #7\'s named gate), then golib=$OWED_GOLIB, then converter=$OWED_CONV (its guard projects and any nested sub-libraries, which route #3 says no other gate enumerates).  The members:"',
          'l5-failing-note')

A.subline(r'^stamp "LEG 5 CALIBRATION OF RECORD ::',
          'stamp "LEG 5 CALIBRATION OF RECORD :: ⚠ THE CALIBRATION IS **DERIVED AT RUN TIME AND SCORED**, NEVER CARRIED.  This is a TEMPLATE: no base and no seat were named when it was written, so there is no derive-time enumeration to predict from and NONE is invented here.  What is ASSERTED is the RELATION -- Output pass == marked, Output skip == unmarked, and the first three phases\' pass == N -- read against the RUNNER\'s OWN --list enumeration and its OWN MatchConsoleOutput predicate.  ⚠ THE NEW PROJECTS ARE NOT ALL TOP-LEVEL: a guard may carry sub-libraries and the runner enumerates DEEPEST-FIRST and RECURSIVELY (route #3), so the enumeration grows by MORE than the top-level count -- derived here as newTopLevel=$NEWBEH newAllDepths=$NEWBEH_ALL.  A wrong prediction is a stamped miss and never a false red."',
          'l5-calibration')

A.subline(r'^      stamp "      \$nm \[\$ph\] -- one of the EIGHT',
          '      stamp "      $nm [$ph] -- one of the EIGHT alias-carrying projects.  ⚠ THIS IS A FINDING ABOUT A SEAT LIKE ANY OTHER: the alias fold makes the emission PIN-INDEPENDENT, so a Compile+Target red here is NOT a pairing diagnosis, and the seat that changes converter source is where to look"',
          'l5-eight-note')

# ---- every `| grep -q` becomes a `grep -c` plus an integer test (lesson 9) -----------------------
# ⚠ NOT ONLY THE STREAMING ONES.  Under `set -o pipefail` a `grep -q` SIGPIPEs its producer and a TRUE
#   match reads as a failure; this file does not set pipefail, but a later derive that does would
#   inherit a landmine per site.  A rule with no judgement in it -- ZERO `| grep -q`, asserted -- is
#   cheaper to keep than a rule that asks each site whether its producer streams.
A.subline_contains("legk_oracle_ok(){ printf '%s' \"$1\" | grep -qE",
                   'legk_oracle_ok(){ [ "$(printf \'%s\' "$1" | grep -acE "$LEGK_ORACLE_RE" || true)" != "0" ]; }',
                   'grepq-legk-oracle')
A.subline_contains('if git diff --name-only --diff-filter=U | grep -q .; then',
                   '    if [ "$(git diff --name-only --diff-filter=U | grep -ac . || true)" != "0" ]; then',
                   'grepq-unmerged')
A.subline_contains("| grep -qE '^[[:space:]]*\\[module:[[:space:]]*(go\\.)?GoManualConversion\\]' && printf",
                   '        [ "$(tr -d \'\\r\' < "$f" 2>/dev/null | grep -acE \'^[[:space:]]*\\[module:[[:space:]]*(go\\.)?GoManualConversion\\]\' || true)" != "0" ] && printf \'%s\\n\' "$f"',
                   'grepq-marker-emit')
A.subline_contains("| grep -qE '^[[:space:]]*\\[module:[[:space:]]*(go\\.)?GoManualConversion\\]' && blind=1",
                   '      [ "$(tr -d \'\\r\' < "$f" 2>/dev/null | grep -acE \'^[[:space:]]*\\[module:[[:space:]]*(go\\.)?GoManualConversion\\]\' || true)" != "0" ] && blind=1',
                   'grepq-marker-blind')
A.subline_contains('elif tr -d \'\\r\' < "$pi" | grep -qaE',
                   '  elif [ "$(tr -d \'\\r\' < "$pi" | grep -acE \'^[[:space:]]*\\[GoTestMatchingConsoleOutput\\][[:space:]]*$\' || true)" != "0" ]; then',
                   'grepq-outputattr')
A.subline_contains('| grep -qx "$nm"; then',
                   '    if [ "$ph" = "Compile,Target" ] && [ "$(printf \'%s\\n\' "$GOLDEN_PROJECTS" | grep -acx "$nm" || true)" != "0" ]; then',
                   'grepq-golden-member')
A.subline_contains("| grep -qx 'reflect'",
                   '      [ "$(go list -e -f \'{{join .Imports "\\n"}}{{"\\n"}}{{join .TestImports "\\n"}}{{"\\n"}}{{join .XTestImports "\\n"}}\' "$1" 2>/dev/null | tr -d \'\\r\' | grep -acx \'reflect\' || true)" != "0" ]',
                   'grepq-legk-imports')
A.subline_contains('grep -qx "$pj" "/tmp/${TAG}-l5-failnames.txt"',
                   '  [ "$(grep -acx "$pj" "/tmp/${TAG}-l5-failnames.txt" 2>/dev/null || true)" != "0" ] && L5EIGHT_RED="$L5EIGHT_RED $pj"',
                   'grepq-l5-failname')
A.subline_contains('&& grep -qF -- "$ALIAS_OLD"',
                   '  if [ -f "src/tests/Behavioral/$pj/main.cs" ] && [ "$(grep -acF -- "$ALIAS_OLD" "src/tests/Behavioral/$pj/main.cs" || true)" != "0" ]; then',
                   'grepq-l5-alias')
A.absent_in_code('grep -q', 'no-grep-q')
A.absent_in_code('pipefail', 'no-pipefail')

# ---- the fifteen silent FAILED=1 setters, each replaced by a NAMED fail_gate call ----------------
# ⚠ EACH IS ANCHORED ON A UNIQUE NEARBY LINE WITH A BOUNDED WINDOW.  Seven of these setters are the
#   byte-identical line `  FAILED=1`; an ordinal or a bare pattern would retarget silently the moment
#   anything above them moved, which is the whole class this derive exists to avoid.
A.subline_contains('[ "$A5BAD" = "0" ] || FAILED=1', '  [ "$A5BAD" = "0" ] || fail_gate A5-nested', 'silent-A5')
A.subline_next(r'census_residual_lines \| census_kinds$', r'^  FAILED=1$', '  fail_gate G6-residual', 'silent-G6', window=3)
A.subline_next(r'^if \[ "\$legcrc" != "0" \]; then$', r'^  FAILED=1$', '  fail_gate LEG-C', 'silent-legc', window=3)
A.subline_next(r"error \(MSB\|NETSDK\)\[0-9\]\+' \"\$L2\"", r'^  FAILED=1$', '  fail_gate LEG-2', 'silent-leg2', window=6)
A.subline_next(r"error \(MSB\|NETSDK\)\[0-9\]\+' \"\$L2B\"", r'^  FAILED=1$', '  fail_gate LEG-2b', 'silent-leg2b', window=6)
A.subline_next(r'head -10 "\$LEGD_MISSING"', r'^    FAILED=1; LEGD_OK=0; \}$', '    fail_gate LEG-D-seeding-control; LEGD_OK=0; }', 'silent-legd-seed', window=3)
A.subline_next(r'LEG D \$arm/\$goos UNMEASURED: the conversion exited non-zero', r'^        FAILED=1; LEGD_OK=0$', '        fail_gate LEG-D-conversion; LEGD_OK=0', 'silent-legd-conv', window=4)
A.subline_next(r'^elif \[ "\$LEGD_MET_ALL" != "1" \]; then$', r'^  FAILED=1$', '  fail_gate LEG-D-prediction', 'silent-legd-pred', window=2)
A.subline_next(r'LEG R \$row CONVERT failed', r'^      FAILED=1$', '      fail_gate LEG-R-convert', 'silent-legr-conv', window=4)
A.subline_next(r'"\$LR2" \| sed -E', r'^      FAILED=1$', '      fail_gate LEG-R-build', 'silent-legr-build', window=5)
A.subline_next(r'LEG 3 golib-\$cfg BUILD exit=\$brc', r'^    FAILED=1$', '    fail_gate LEG-3-build', 'silent-leg3-build', window=4)
A.subline_next(r'LEG 4 UNMEASURED \(after-guard\)', r'^  FAILED=1$', '  fail_gate LEG-4-after-guard', 'silent-leg4-guard', window=4)
A.subline_next(r'^  done 3< "/tmp/\$\{TAG\}-l5-fails\.txt"$', r'^  FAILED=1$', '  fail_gate LEG-5-failing-set', 'silent-leg5-set', window=3)
A.subline_next(r'^  if \[ "\$L5RECFAIL" = "0" \]; then$', r'^    FAILED=1$', '    fail_gate LEG-5-reconciliation', 'silent-leg5-recon', window=5)
A.subline_next(r'LEG K \$pkg is NOT a PASS at its banked count', r'^      \[ "\$rc" = "0" \] \|\| FAILED=1$', '      [ "$rc" = "0" ] || fail_gate LEG-K-row-exit', 'silent-legk-row', window=4)

# ---- the closing DONE stamp --------------------------------------------------------------------
A.subline(r'^stamp "=== \$\{LABEL\} ASSEMBLE DONE head=\$\(git rev-parse --short HEAD\) base=\$BASE route=\$ROUTE',
          'stamp "=== ${LABEL} ASSEMBLE DONE head=$(git rev-parse --short HEAD) base=$BASE route=$ROUTE seats=$SEATS_DONE (of $SEATS_LISTED listed; skippedPending=$SEATS_SKIPPED) requireAll=$REQUIRE_ALL seatShas=[$SEAT1_SHA $SEAT2_SHA $SEAT3_SHA $SEAT4_SHA $SEAT5_SHA $SEAT6_SHA] owed=[golib=$OWED_GOLIB gen=$OWED_GEN converter=$OWED_CONV behavioral=$OWED_BEH corpus=$OWED_CORPUS golibtests=$OWED_GT docs=$OWED_DOCS claudemd=$OWED_CLAUDEMD manifest=$OWED_MANIFEST] seatAssertArms=$SEATASSERT_N legD=$LEGD_VERDICTS legKrows=[$LEGK_ROWS] newBehavioralProjects=$NEWBEH newBehavioralProjectsAllDepths=$NEWBEH_ALL overallFailed=$FAILED ==="',
          'done-stamp')

A.gsub('coord-train46-assemble', 'coord-train47-assemble', 'self-name')
A.gsub('coord-train46-rehearse', 'coord-train47-rehearse', 'rehearse-name')
# ⚠ NO RENAME IS NEEDED HERE AND THAT IS ASSERTED RATHER THAN ASSUMED: the header, the seats block
#   and the loop block each carried TRAIN46_REQUIRE_ALL and each was REPLACED WHOLE, so a surviving
#   occurrence would mean one of those replacements did not cover what it was supposed to.
A.absent('TRAIN46', 'no-train46-token')
# ⚠ THE PROSE 'train 46' IS **KEPT AND NOT ASSERTED AWAY**.  It names the base this train sits on and
#   the run records each mechanised lesson came from; a derive that scrubbed it would delete the
#   record of WHY every one of these arms is shaped the way it is.

ASM_LINES = A.write('coord-train47-assemble.sh')


# ============================================================================================
# THE REHEARSAL
# ============================================================================================
R = Doc('coord-train46-rehearse.sh')

RHEADER = r"""#!/bin/bash
# ============================================================================================
# TRAIN 47 REHEARSAL -- SEQUENTIAL REAL MERGES in a THROWAWAY WORKTREE, plus a **PER-SEAT TYPE
# CHECK**, removed at the end.
#
# ⚠ IT RUNS THE REAL COMMAND.  A TEMP-INDEX dry run (`read-tree -m --aggressive` plus `merge-file`)
# answers "would the 3-way CONTENT merge conflict"; it does NOT answer "does `git merge` do what the
# ASSEMBLY script will do", and a pairwise three-way against master cannot see SEAT-VERSUS-SEAT
# collisions at all.  The accumulation is run for real and the accumulation is the real accumulation.
#
# ⚠⚠ **NEW IN TRAIN 47: `go vet ./...` IN src/go2cs AFTER EVERY SEAT MERGE, AND IT NAMES THE FIRST
#   FAILING SEAT.**  Train 46 run 4 died forty minutes into its battery at LEG C with
#   `FAIL go2cs [build failed]`: two edits to ONE `_test.go` -- one seat's new call sites, another
#   train's widened signature -- that git merged CLEAN BY CONTENT, so the package failed to compile
#   ONLY AT THE UNION and no per-seat gate could have seen it.  A clean merge is not a compiling
#   tree; this leg is the cheapest instrument that knows the difference, and finding it HERE costs
#   seconds where finding it in the battery costs a leg.
#   ⚠ CONSEQUENCE, STATED RATHER THAN GLOSSED: **THIS SCRIPT IS NO LONGER "git only".**  `go vet`
#   type-checks the package, which means it reads the Go toolchain and writes into the build cache.
#   It writes nothing into the repository, runs no converter and runs no dotnet, and it still creates
#   its OWN worktree -- but it is CPU, and it must not be run inside a worktree a battery is using.
#   ⚠ IT RUNS UNDER THE **CONVERTER BUILD PIN** (the 1.24.13 root, GOTOOLCHAIN=local), because the
#   converter module declares `go 1.24.13`: under the corpus pin it would refuse for the RIGHT reason
#   and read as a seat defect.  The pin RUNS `go version` and ABORTS on a mismatch -- printing a pin
#   is a decoration, and this repository has already paid for an instrument that printed one and
#   carried on.  A vet leg that could not establish its pin is reported UNMEASURED and is never a
#   pass: an instrument that cannot run has not found nothing, it has found nothing out.
#
# ⚠ IT RESOLVES NOTHING, AND IT **SAVES A RESOLUTION SLOT** FOR EACH CONFLICTED PATH.  The
# resolutions are made HERE, where a mistake costs nothing, VERIFIED here, and then applied
# MECHANICALLY at assembly and stamped PRE-RESOLVED.  What this script writes is the SLOT and the
# evidence -- the three versions plus the conflicted working file -- and what it never writes is an
# answer.  A conflict is the coordinator's RULING.
#     $SELFDIR/coord-train47-resolutions/seat<N>/
#         MANIFEST.txt          seat=<N> pin=<sha> branch=<ref> base=<sha> paths=<n>
#         conflicted/<path>     the merged file WITH its markers, exactly as git left it
#         base/<path>  ours/<path>  theirs/<path>     the three stages, for reading
#         files/<path>          ⚠ **EMPTY UNTIL THE COORDINATOR WRITES THE RESOLVED FILE HERE.**
#                               merge_seat applies EVERY path from `files/` or REFUSES; a partial set
#                               is a tree nobody ruled on.
# ⚠ AND IT VERIFIES WHAT IT IS GIVEN.  `--verify` re-reads a saved set and asserts: zero conflict
# markers; the file is non-empty; and -- for a `.go`, `.cs` or `.csproj` -- that BOTH sides' distinct
# top-level symbols survive.  A both-kept resolution proven only by the ABSENCE of markers is how one
# lane split a function in half behind a green `gofmt`, and a symbol COUNT is what caught it.
#
# ⚠ A CLEAN REHEARSAL IS NOT A CLEAN ASSEMBLY, AND THE DIFFERENCE IS PERMANENT.  This script runs
# `git merge` and `go vet`; the assembly ALSO applies each seat's CLASS SHAPE and its FORBIDDEN path
# set, which only the assembly does.  Two instruments, two questions.
#
# ⚠ `set -u` ONLY -- **NOT** `set -o pipefail`.  Under pipefail a `grep -q` that exits on its first
# match SIGPIPEs its producer and a TRUE match reads as a failure; R measured that on this fleet.
# This file carries ZERO `| grep -q` pipes for the same reason.
#
# ⚠ THE SEAT TABLE AND THE BASE ARE **READ OUT OF THE ASSEMBLY SCRIPT**, NEVER RETYPED.  A second
# copy of a seat list is the thing that drifts, and a rehearsal of a different list than the assembly
# will merge is a rehearsal of a train nobody is assembling.  That includes the BASE: train 46's
# rehearsal carried `BASE_EXPECT=<sha>` as a literal beside the assembly's own constant, which is two
# copies of one fact.
#
#   usage:  bash coord-train47-rehearse.sh                 # base = the CURRENT origin/master
#           M=<sha> bash coord-train47-rehearse.sh         # rehearse onto a named master
#           KEEP=1 bash coord-train47-rehearse.sh          # leave the worktree standing for inspection
#           NOVET=1 bash coord-train47-rehearse.sh         # skip the per-seat vet, STATED in the summary
#           bash coord-train47-rehearse.sh --verify        # verify the SAVED resolutions, merge nothing
#   exit 0  every seat merged clean AND type-checked (or every saved resolution verified)
#        1  a seat conflicted, a seat's merge broke the type check, or a saved resolution failed
#        2  a precondition refused
# ============================================================================================
"""
R.block(r'^#!/bin/bash$', r'^set -u$', RHEADER, 'rehearse-header', end_off=-1)

RSETUP = r"""G="${GO2CS_REPO:-/c/Projects/go2cs}"
WT="$G/.claude/worktrees/coord-t47-rehearse"
TAG=t47rehearse
say(){ printf '%s\n' "$*"; }

# --- THE CONVERTER BUILD PIN, DERIVED FROM $HOME AND ASSERTED.  ⚠ No profile path is spelled here:
#     a literal would breach the security convention on any pushed surface and be wrong on every
#     other box.  A DERIVED root that is not there is refused BY NAME rather than left to produce a
#     confusing toolchain error three steps later.
GO_SDK_ROOT="${GO2CS_SDK_ROOT:-$HOME/sdk}"
PIN_GO_1_24_ROOT="$GO_SDK_ROOT/go1.24.13"
VETPIN=0
pin_for_vet(){
  [ -d "$PIN_GO_1_24_ROOT" ] || { say "VET PIN UNAVAILABLE :: [$PIN_GO_1_24_ROOT] does not exist (override with GO2CS_SDK_ROOT)"; return 1; }
  export GOROOT="$PIN_GO_1_24_ROOT" GOTOOLCHAIN=local
  export PATH="$PIN_GO_1_24_ROOT/bin:$PATH"
  gv=$(go version 2>/dev/null | tr -d '\r')
  gr=$(go env GOROOT 2>/dev/null | tr -d '\r')
  case "$gv" in *go1.24.13*) : ;; *) say "VET PIN MISMATCH :: \`go version\` reads [$gv] where go1.24.13 is required -- printing a pin is a decoration, so this REFUSES"; return 1 ;; esac
  say "VET PIN OK :: $gv :: GOROOT=$gr GOTOOLCHAIN=local -- the converter module declares go 1.24.13, so the vet runs under the BUILD pin and not the corpus one"
  return 0
}
"""
R.block(r'^G=/c/Projects/go2cs$', r'^say\(\)\{ printf ', RSETUP, 'rehearse-setup')

R.subline(r'^ASM="\$SELFDIR/coord-train46-assemble\.sh"$',
          'ASM="$SELFDIR/coord-train47-assemble.sh"', 'rehearse-asm')
R.subline(r'^RESROOT="\$SELFDIR/coord-train46-resolutions"$',
          'RESROOT="$SELFDIR/coord-train47-resolutions"', 'rehearse-resroot')
R.subline(r'^BASE_EXPECT=44f858717$',
          '# --- THE BASE, READ OUT OF THE ASSEMBLY SCRIPT rather than retyped beside it.  ⚠ It may read\n'
          '#     PENDING: this is a TEMPLATE and the base is one of its three fill points.  A PENDING base\n'
          '#     is a READING here (the rehearsal onto the CURRENT origin/master is still meaningful and is\n'
          '#     what a coordinator wants before the seats are pinned), and it is STAMPED as such.\n'
          'BASE_EXPECT=$(grep -aE "^EXPECT_46=\'" "$ASM" 2>/dev/null | head -1 | sed -E "s/^EXPECT_46=\'([^\']*)\'.*/\\1/")\n'
          '[ -n "$BASE_EXPECT" ] || BASE_EXPECT=UNREADABLE',
          'rehearse-base')

R.subline(r'^\[ "\$ROWS" = "6" \]',
          '# ⚠ THE ROW COUNT IS **DERIVED AND STRUCTURALLY CHECKED**, NEVER COMPARED TO A LITERAL.  Train\n'
          '#   46\'s rehearsal carried `[ "$ROWS" = "6" ]`, which is a fact about that derive; what a\n'
          '#   rehearsal can honestly assert is what a hand-edited table breaks -- contiguous numbers and\n'
          '#   unique refs -- and the assembly asserts the same thing over the same table.\n'
          'RNUMS=$(printf \'%s\\n\' "$SEAT_TABLE" | awk -F\'|\' \'{print $1}\')\n'
          'REXP=$(awk -v n="$ROWS" \'BEGIN{for(i=1;i<=n;i++) print i}\')\n'
          'RDUP=$(printf \'%s\\n\' "$SEAT_TABLE" | awk -F\'|\' \'{print $2}\' | sort | uniq -d | grep -ac . || true)\n'
          'say "REHEARSAL table structure :: numbers [$(printf \'%s\' "$RNUMS" | tr \'\\n\' \' \')] (must be 1..$ROWS contiguous) :: duplicate refs=$RDUP (must be 0)"\n'
          '{ [ "$(printf \'%s\\n\' "$RNUMS")" = "$(printf \'%s\\n\' "$REXP")" ] && [ "${RDUP:-0}" = "0" ]; } || { say "REFUSED: the seat table is structurally unsound -- the ROW ORDER IS THE MERGE ORDER, so a duplicated or misnumbered row merges in an order nobody ruled on"; exit 2; }',
          'rehearse-rows')

# every row read gains the optional sixth field
R.subline(r'^while IFS=\x27\|\x27 read -r n b s c m; do$',
          "while IFS='|' read -r n b s c m a; do", 'rehearse-read-1', want=1)
R.subline(r'^while IFS=\x27\|\x27 read -r n b s c m <&3; do$',
          "while IFS='|' read -r n b s c m a <&3; do", 'rehearse-read-2', want=1)

# the per-seat vet, spliced in after each successful merge
RVET = ('''    MERGED=$(( MERGED + 1 ))
    say "seat $n ($c) $b @$short :: CLEAN  (tree -> $(git rev-parse --short 'HEAD^{tree}'), files in the accumulation $(git -c core.quotepath=false diff --name-only "$M" HEAD | grep -c .))"
    # --- ⚠ THE PER-SEAT TYPE CHECK.  A CLEAN MERGE IS NOT A COMPILING TREE: two edits to one file
    #     that git merges by CONTENT can leave a package that compiles on neither side's terms, and
    #     the only instrument that sees it is one that type-checks the ACCUMULATION.  The FIRST seat
    #     whose accumulation fails is the one named, because every later failure is downstream of it.
    if [ "${NOVET:-0}" = "1" ]; then
      say "      vet SKIPPED (NOVET=1) -- ⚠ STATED: this rehearsal says the seats MERGE and says NOTHING about whether the union COMPILES"
    elif [ "$VETPIN" != "1" ]; then
      say "      vet UNMEASURED -- the converter build pin could not be established, and an instrument that cannot run has not found nothing"
      VETUNMEASURED=$(( VETUNMEASURED + 1 ))
    else
      ( cd "$WT/src/go2cs" && go vet ./... > "/tmp/${TAG}-vet-$n.log" 2>&1 ); vrc=$?
      if [ "$vrc" = "0" ]; then
        say "      go vet ./... in src/go2cs :: exit=0 -- the accumulation THROUGH THIS SEAT type-checks"
      else
        VETFAIL=$(( VETFAIL + 1 ))
        [ -n "$VETFIRST" ] || VETFIRST="seat $n ($c) $b @$short"
        say "      ** go vet ./... in src/go2cs :: exit=$vrc -- THE ACCUMULATION THROUGH THIS SEAT DOES NOT TYPE-CHECK **"
        say "      ⚠ THIS IS A **UNION-ONLY** FAILURE IF THE SEAT IS GREEN ALONE, which is the class that once"
        say "        died forty minutes into a battery at LEG C.  Its remedy is an ASSEMBLY commit (the union"
        say "        fix), not an edit to a seated branch: neither side is wrong on its own."
        head -12 "/tmp/${TAG}-vet-$n.log" | sed 's/^/          /'
      fi
    fi''')
R.block(r'^    MERGED=\$\(\( MERGED \+ 1 \)\)$', r'^    say "seat \$n \(\$c\) \$b @\$short :: CLEAN', RVET, 'rehearse-vet')

R.subline(r'^CONFLICTS=0; MERGED=0; SKIPPED=0; SKIPNAMES=""$',
          'CONFLICTS=0; MERGED=0; SKIPPED=0; SKIPNAMES=""; VETFAIL=0; VETUNMEASURED=0; VETFIRST=""\n'
          'if [ "${NOVET:-0}" = "1" ]; then say "⚠ NOVET=1 :: the per-seat type check is OFF for this run, and the summary says so.  A rehearsal without it answers only \'do the seats MERGE\'."; else pin_for_vet && VETPIN=1; fi',
          'rehearse-vet-init')

# the seat-specific summary readings become DERIVED ones
RSUM = r"""# --- WHAT THE ACCUMULATION MOVED, DERIVED FROM THE DELTA RATHER THAN NAMED.  Train 46's rehearsal
#     printed three hand-written numstat lines naming that train's own files, which is the
#     stale-figure class: the moment a seat is re-pinned they describe nothing.  What a template can
#     print is the delta's own shape, grouped by the directories every class in the table can reach.
for d in src/go2cs src/go2cs.slnx src/gen src/core/golib src/core src/tests/GolibTests src/tests/Behavioral docs CLAUDE.md; do
  nd=$(git -c core.quotepath=false diff --name-only "$M" HEAD -- "$d" | grep -ac . || true)
  ad=$(git -c core.quotepath=false diff --numstat "$M" HEAD -- "$d" | awk '{a+=$1;r+=$2} END{printf "+%d/-%d", a+0, r+0}')
  say "delta $d :: files=$nd $ad"
done
# --- and the DISPLACEMENT SHAPE, asked of the delta rather than of a named file.  A converter
#     REGISTRY entry whose corpus footprint did not land is the syscall.Uname silent-subtraction
#     class: both diffs are pure additions and removals, git merges them without a conflict, and the
#     result compiles nowhere.  The DELETION COLUMN is what tells the two apart, so any corpus file
#     the accumulation SHRINKS is printed by name -- a reader can then check it against the seat that
#     claims to displace it.
say "corpus files whose emission SHRANK in this accumulation (a displacement's own signature; read them against the seat that claims it):"
git -c core.quotepath=false diff --numstat "$M" HEAD -- src/core | awk '$2+0 > $1+0 {printf "      %s  +%s/-%s\n", $3, $1, $2}'"""
R.block(r"^# --- seat 3's DISPLACEMENT, read as a numstat\.", r'^say "seat 2 generator half ::', RSUM, 'rehearse-summary')

RTAIL = r"""say ""
say "⚠ WHAT A CLEAN REHEARSAL DOES AND DOES NOT SAY.  It says the seats MERGE, in this order, onto"
say "  this base, and -- unless NOVET=1 -- that the accumulation TYPE-CHECKS through every seat.  It"
say "  says NOTHING about whether the seats are CORRECT together (that is the assembly's A-assertions"
say "  and its battery) and NOTHING about the CLASS SHAPES, which the assembly applies and this script"
say "  does not.  It also says nothing about any seat whose change EMITS NOTHING -- a hand-own"
say "  retarget, a manifest entry -- for which the instrument is the banked row's own sweep."
say "per-seat type check :: failures=$VETFAIL unmeasured=$VETUNMEASURED first failing seat=[${VETFIRST:-none}] (NOVET=${NOVET:-0})"
RC=0
[ "$CONFLICTS" = "0" ] || RC=1
[ "$MK" = "0" ] || RC=1
[ "$VETFAIL" = "0" ] || RC=1
[ "$VETUNMEASURED" = "0" ] || RC=1
say "=== REHEARSAL DONE conflicts=$CONFLICTS markers=$MK vetFailures=$VETFAIL vetUnmeasured=$VETUNMEASURED exit=$RC ==="
exit $RC"""
R.block(r'^say "⚠ WHAT A CLEAN REHEARSAL DOES AND DOES NOT SAY\.', r'^exit \$RC$', RTAIL, 'rehearse-tail', start_off=-1)

# ⚠ NO SELF-NAME RENAME IS OWED: the header block carried every mention and was replaced WHOLE.
#   The absence below is what says so, rather than a gsub that would report a silent zero.
R.gsub('TRAIN 46 RESOLUTION VERIFY', 'TRAIN 47 RESOLUTION VERIFY', 'rehearse-verify-title')
R.absent_in_code('grep -q', 'rehearse-no-grep-q')
R.absent_in_code('pipefail', 'rehearse-no-pipefail')
R.absent('coord-train46', 'rehearse-no-46-path')
REH_LINES = R.write('coord-train47-rehearse.sh')


# ============================================================================================
# THE DERIVE-TIME SELF-CHECK
# ============================================================================================
S = Doc('coord-train46-derive-selfcheck.sh')

SHEADER = r"""#!/bin/bash
# DERIVE-TIME SELF-CHECK for coord-train47-assemble.sh (throwaway, read-only).
#
# It EXTRACTS blocks from the assembly script BY GREP ANCHOR and runs them, so what is exercised is
# the assembly script's OWN definitions rather than a retyped copy -- a retyped copy is the thing that
# drifts.  It NEVER runs the assembly script: no cd into the assembly worktree, no fetch, no merge,
# no leg, no build.
#
#   usage:  bash coord-train47-derive-selfcheck.sh [<path to coord-train47-assemble.sh>]
#   exit 0  every arm passed
#   exit 1  an arm failed
#   exit 3  a BLOCK COULD NOT BE RESOLVED or does not parse -- the checker itself is broken, and that
#           is reported as its own state rather than as a finding about the script
#
# ⚠ `set -u` ONLY -- **NOT** `set -o pipefail`, for the reason the assembly's own header states: under
#   pipefail a `grep -q` SIGPIPEs its producer and a TRUE match reads as a failure.
#
# ⚠ WHAT THIS CHECKS, AND WHY EACH ARM EXISTS:
#   0  the script parses, carries no CR bytes and no BOM.
#   1  the BASE: EXPECT_46 is declared, the script ASSERTS it, and the assertion REFUSES a PENDING
#      base.  ⚠ ON A TEMPLATE THE BASE **IS** PENDING and that is the expected reading; what is
#      asserted is that the refusal EXISTS, because a template whose placeholder base can run is a
#      battery measuring a tree nobody named.
#   2  the SEAT TABLE -- its STRUCTURE (contiguous numbers, unique refs, unique filled SHAs, five
#      fields or more, an optional `allowed=` sixth), every SHA nine hex digits or PENDING, the
#      PENDING rows PRINTED BY NAME, and every class known to merge_seat.  ⚠ NO ROW-COUNT LITERAL,
#      here or in the assembly: train 46's `[ "$SEATS_LISTED" = "5" ]` refused a correct six-row
#      table AFTER merging all six.
#   3  every FILLED seat SHA RESOLVES, and a grown branch is REPORTED with its ancestry diagnosis
#      rather than silently re-pinned.
#   4  NO train-46 SHA survives outside a dated comment -- and the residual comment hits are PRINTED,
#      because "zero in code" is a different claim from "none anywhere" and only one of them is true.
#   5  the WORKTREE is a FILL POINT that REFUSES a placeholder, and SCRIPT_PATH is ABSOLUTE and
#      precedes the cd.
#   6  G11(c)'s NINE launch lines present, WITH negative controls -- the arm must go RED on a copy
#      with one removed, or its green is an assertion wearing a measurement's clothes.
#   7  the LEG K oracleGoVersion predicate, run FROM THE SCRIPT'S OWN DEFINITION over six values.
#   8  the per-seat CLASS SHAPES against the REAL seat diffs (for FILLED rows) and against decoys.
#   9  the PER-RUN-COPY discipline and the MID-BATTERY FREEZE announcement lines.
#  10  the G10d `-text` arm consults the ATTRIBUTE, with a decoy.
#  11  blk()'s own negative control: a broken anchor must be reported as the CHECKER being broken.
#  12  ⚠ **THE FIVE MECHANISED LESSONS, EACH WITH ITS OWN NEGATIVE CONTROL AGAINST THE TRAIN-46
#      ORIGINALS.**  Every one of these is a defect a train-46 launch actually hit; each arm must
#      read ZERO on the train-47 files AND NON-ZERO on the train-46 ones, because an arm that has
#      never been made to fire is an assertion rather than a measurement.  A control that cannot RUN
#      (the train-46 originals absent) is reported UNMEASURED and FAILS: an instrument that could not
#      run has not found nothing.
set -u
F="${1:-$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)/coord-train47-assemble.sh}"
SELFDIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
T46DIR="${T46DIR:-$SELFDIR}"
TAG=t47selfcheck
FAIL=0
say(){ printf '%s\n' "$*"; }
[ -f "$F" ] || { say "   ABORT: $F does not exist"; exit 3; }
# the BASE this checker measures against is READ FROM THE SCRIPT, never retyped beside it.
BASE_EXPECT=$(grep -aE "^EXPECT_46='" "$F" | head -1 | sed -E "s/^EXPECT_46='([^']*)'.*/\1/")
[ -n "$BASE_EXPECT" ] || BASE_EXPECT=UNREADABLE
"""
S.block(r'^#!/bin/bash$', r'^ex\(\)\{ sed -n ', SHEADER, 'sc-header', end_off=-2)

SC1 = r"""say "== 1. the BASE: EXPECT_46 is declared, ASSERTED, and a PENDING base REFUSES =="
E47=$(grep -aE "^EXPECT_46='" "$F" | head -1 | sed -E "s/^EXPECT_46='([^']*)'.*/\1/")
E46=$(grep -aE "^EXPECT_45='" "$F" | head -1 | sed -E "s/^EXPECT_45='([^']*)'.*/\1/")
say "   the script declares EXPECT_46=[$E47] (this train's BASE) and EXPECT_45=[$E46] (the ORDER pin)"
BA=$(grep -ac '^stamp "BASE ASSERTION ::' "$F")
BR=$(grep -ac 'BASE REFUSED: this script declares' "$F")
BP=$(grep -ac 'BASE REFUSED: EXPECT_46 reads' "$F")
say "   the script's OWN base assertion :: 'BASE ASSERTION ::' stamps=$BA (want 1) :: wrong-base refusal=$BR (want 1) :: PENDING-base refusal=$BP (want 1)"
{ [ "$BA" = "1" ] && [ "$BR" = "1" ] && [ "$BP" = "1" ]; } && say "   PASS: the script ASSERTS its base, REFUSES a different one, and REFUSES an UNFILLED one" || { say "   FAIL: the base is derived but not asserted in all three directions -- HEAD == origin/master says the tree is AT master, not that master is the master this was derived against, and a PENDING base that RUNS is a battery measuring a tree nobody named"; FAIL=1; }
case "$E47" in
  PENDING|'')
    say "   ⚠ EXPECT_46 reads PENDING, which is the EXPECTED state of a TEMPLATE.  It is FILL POINT F2."
    say "     Arms 3 and 8 are therefore UNMEASURABLE against real seat diffs and say so; they are not passes."
    ;;
  *)
    if [ -d "$G/.git" ]; then
      git -C "$G" fetch -q origin master 2>/dev/null
      RM=$(git -C "$G" ls-remote origin refs/heads/master 2>/dev/null | cut -c1-9)
      OM=$(git -C "$G" rev-parse --short=9 origin/master 2>/dev/null)
      say "   origin/master :: ls-remote reads [$RM] :: the fetched ref reads [$OM] :: the script's base is [$E47]"
      say "   ⚠ BOTH ARE PRINTED ON PURPOSE.  A base declared and a base MEASURED are two claims, and this arm exists to compare them rather than to restate one of them."
      { [ "$RM" = "$OM" ] && [ "$RM" = "$E47" ]; } && say "   PASS: origin/master IS $E47 and the two readings agree" || { say "   FAIL: origin/master reads [$RM]/[$OM] where this script declares $E47 -- the tree moved, so RE-DERIVE rather than adjust"; FAIL=1; }
    else
      say "   SKIPPED: $G is not a git repository from here -- the base could not be MEASURED, only read from the script"
      FAIL=1
    fi ;;
esac"""
S.block(r'^say "== 1\. the BASE:', r'^BA=\$\(grep -c ', SC1, 'sc-arm1', end_index=None, end_off=4)

SC2 = r"""say "== 2. the seat table: STRUCTURE (no row-count literal anywhere), nine-hex or PENDING, the classes =="
eval "$(blk '^SEAT_TABLE="1\|' '\|tip"$' 'SEAT_TABLE')" || exit 3
ROWS=$(printf '%s\n' "$SEAT_TABLE" | grep -c .)
NUMS=$(printf '%s\n' "$SEAT_TABLE" | awk -F'|' '{print $1}')
EXPN=$(awk -v n="$ROWS" 'BEGIN{for(i=1;i<=n;i++) print i}')
DUPREF=$(printf '%s\n' "$SEAT_TABLE" | awk -F'|' '{print $2}' | sort | uniq -d | grep -ac . || true)
DUPSHA=$(printf '%s\n' "$SEAT_TABLE" | awk -F'|' '$3!="PENDING"{print $3}' | sort | uniq -d | grep -ac . || true)
SHORTR=$(printf '%s\n' "$SEAT_TABLE" | awk -F'|' 'NF<5' | grep -ac . || true)
ALLOWR=$(printf '%s\n' "$SEAT_TABLE" | awk -F'|' '$6 ~ /^allowed=/' | grep -ac . || true)
say "   rows=$ROWS :: merge order [$(printf '%s' "$NUMS" | tr '\n' ' ')] :: duplicate refs=$DUPREF :: duplicate filled SHAs=$DUPSHA :: rows with <5 fields=$SHORTR :: rows carrying an allowed= ruling=$ALLOWR"
say "   ⚠ THERE IS NO ROW-COUNT LITERAL HERE, AND ARM 12 REFUSES ONE IN THE ASSEMBLY.  A count compared"
say "     against itself is a check that cannot fail; what a hand-edited table actually breaks is the"
say "     STRUCTURE, and that is what both instruments assert."
{ [ "$(printf '%s\n' "$NUMS")" = "$(printf '%s\n' "$EXPN")" ] && [ "${DUPREF:-0}" = "0" ] && [ "${DUPSHA:-0}" = "0" ] && [ "${SHORTR:-0}" = "0" ]; } && say "   PASS: numbers 1..$ROWS contiguous, refs unique, filled SHAs unique, every row five fields or more" || { say "   FAIL: the seat table is structurally unsound -- the ROW ORDER IS THE MERGE ORDER"; FAIL=1; }
BADSHA=0; PEND=0; PENDNAMES=""
while IFS='|' read -r n b s c m a; do
  [ -n "$n" ] || continue
  if [ "$s" = "PENDING" ]; then PEND=$(( PEND + 1 )); PENDNAMES="$PENDNAMES $n($b)"; continue; fi
  if [ "$(printf '%s' "$s" | grep -acE '^[0-9a-f]{9}$' || true)" = "0" ]; then
    BADSHA=$(( BADSHA + 1 )); say "   FAIL: seat $n's SHA [$s] is neither nine hex digits nor PENDING"
  fi
done <<< "$SEAT_TABLE"
say "   PENDING rows=$PEND [$PENDNAMES]"
say "   ⚠ PRINTED BY NAME, not merely counted.  On a TEMPLATE every row is a placeholder and that is the"
say "     EXPECTED reading; the assembly SKIPS them with a stamp by default, and TRAIN47_REQUIRE_ALL=1"
say "     is what makes them refuse -- the shape a LANDING run of the full table takes."
say "   malformed SHAs=$BADSHA (want 0)"
[ "$BADSHA" = "0" ] && say "   PASS: every non-PENDING SHA is nine hex digits" || FAIL=1
CLSOK=1
for c in $(printf '%s\n' "$SEAT_TABLE" | awk -F'|' '{print $4}' | sort -u); do
  # ⚠ A **FIXED-STRING** LOOKUP, NEVER AN ERE.  A class name may carry a regex metacharacter
  #   (`converter-test+docs` does), and an ERE built from it silently matches nothing -- which would
  #   report a correctly implemented class as missing.  The glyph-substring family, one layer over.
  if [ "$(grep -acF -- "    ${c})" "$F" || true)" != "0" ]; then say "   class '$c' has a merge_seat arm"; else say "   FAIL: class '$c' has NO merge_seat arm"; CLSOK=0; fi
done
[ "$CLSOK" = "1" ] && say "   PASS: every class in the table is a class merge_seat implements" || FAIL=1
for m in $(printf '%s\n' "$SEAT_TABLE" | awk -F'|' '{print $5}' | sort -u); do
  case "$m" in tip|sha) say "   tip mode '$m' is known" ;; *) say "   FAIL: unknown tip mode '$m'"; FAIL=1 ;; esac
done
# the PLACEHOLDER REF discipline: a row whose SHA is filled must not still name a placeholder ref,
# and a row whose SHA is PENDING must not carry an allowed= ruling.
PHBAD=0
while IFS='|' read -r n b s c m a; do
  [ -n "$n" ] || continue
  case "$b" in claude/PENDING-*) [ "$s" = "PENDING" ] || { say "   FAIL: row $n has a FILLED SHA ($s) against the PLACEHOLDER ref [$b]"; PHBAD=1; } ;; esac
  case "${a:-}" in allowed=*) [ "$s" != "PENDING" ] || { say "   FAIL: row $n declares an allowed= ruling while its SHA reads PENDING -- a ruling about a seat that does not exist"; PHBAD=1; } ;; esac
done <<< "$SEAT_TABLE"
[ "$PHBAD" = "0" ] && say "   PASS: no row carries a filled SHA against a placeholder ref, and no PENDING row carries a ruling (the assembly's own preflight asserts the same before it touches the worktree)" || FAIL=1"""
S.block(r'^say "== 2\. the seat table:', r'^\[ "\$PHBAD" = "0" \]', SC2, 'sc-arm2')

S.subline(r"^T45SHAS='",
          "# ⚠ THE RESIDUAL SET IS TRAIN 46's SEAT SHAS **AND** TRAIN 45's.  A template derived from train\n"
          "#   46 inherits its seat pins wherever a block was not replaced whole, and a live SHA from a\n"
          "#   landed train is a pin that resolves and means nothing.\n"
          "T45SHAS='234cf8e8d|1922e3ec1|31668f43e|787159ed7|aa7abc006|3e5ead2d1|7d377e27b|a29253a2b|044116000|9092ab8b7|4a8642e7e|c08cb29c5|acab60084|13908a888|d839cb1d7|19bb74012|f613d5cfa|35105cdcd|193af90f5|489c5553c|dddd46493|8d7c348bb|34cf4ad02|05b50de63|9893b70e1|8adf8875a|5c5ef371d|e7201a405|18cb44b19|238dfefea'",
          'sc-residual-shas')
S.gsub('train-45 seat SHAs', 'train-45/46 seat SHAs', 'sc-residual-prose', minimum=1)
S.gsub('train-45 SHA ', 'train-45/46 seat SHA ', 'sc-residual-prose2', minimum=1)

SC5 = r"""say "== 5. the worktree is a FILL POINT that REFUSES a placeholder; SCRIPT_PATH is ABSOLUTE and precedes the cd =="
WTLINE=$(grep -n '^WT="\${TRAIN47_WT:-' "$F" | head -1)
CDLINE=$(grep -n '^cd "\$WT"' "$F" | head -1)
REFUSE=$(grep -ac 'WRONG WORKTREE' "$F")
UNFILLED=$(grep -ac 'WORKTREE UNFILLED ::' "$F")
say "   $WTLINE"
say "   $CDLINE"
say "   'WRONG WORKTREE' refusals=$REFUSE (want >= 1) :: 'WORKTREE UNFILLED' refusals=$UNFILLED (want >= 1)"
{ [ -n "$WTLINE" ] && [ -n "$CDLINE" ] && [ "$REFUSE" -ge 1 ] && [ "$UNFILLED" -ge 1 ]; } && say "   PASS: the worktree is a named FILL POINT, an unfilled one ABORTS, and any other tree is refused.  ⚠ IT IS NOT INHERITED FROM THE PREVIOUS TRAIN: a script pointing at a neighbour's worktree would run inside a tree another battery may be holding" || { say "   FAIL: the worktree is not fill-point-and-refused"; FAIL=1; }
SPLINE=$(grep -n '^SCRIPT_PATH=' "$F" | head -1)
say "   $SPLINE"
if [ "$(printf '%s' "$SPLINE" | grep -acF 'cd "$(dirname "${BASH_SOURCE[0]}")" && pwd' || true)" != "0" ]; then
  say "   PASS: SCRIPT_PATH is resolved through cd+pwd BEFORE the script cd's into the assembly worktree, so G11(c)'s grep can open the file"
else
  say "   FAIL: SCRIPT_PATH is not the absolute form.  A relative BASH_SOURCE grepped after the cd once read '0 of 6 launch lines' -- the instrument's own blindness reported as a finding about the script."
  FAIL=1
fi
SPN=$(grep -ac '^SCRIPT_PATH=' "$F"); CDN=$(printf '%s' "$CDLINE" | cut -d: -f1); SPL=$(printf '%s' "$SPLINE" | cut -d: -f1)
say "   SCRIPT_PATH assignments=$SPN (want 1) at line $SPL; the cd is at line ${CDN:-none} (SCRIPT_PATH must come FIRST)"
{ [ "$SPN" = "1" ] && [ -n "${CDN:-}" ] && [ "$SPL" -lt "$CDN" ]; } && say "   PASS: the assignment precedes the cd" || { say "   FAIL: the ordering does not hold, so the absolute form buys nothing"; FAIL=1; }"""
S.block(r'^say "== 5\. the worktree is NAMED', r'^\{ \[ "\$SPN" = "1" \] && \[ -n "\$\{CDN:-\}" \]', SC5, 'sc-arm5')

# arm 8's extractors become fixed-prefix awk reads (a class name may carry a regex metacharacter)
S.subline_contains('  shape_of(){ grep -E "^ *$1\\)" "$F"',
                   '''  # ⚠ THE EXTRACTORS READ THE ARM BY A **FIXED PREFIX**, NEVER BY AN ERE BUILT FROM THE CLASS NAME.
  #   `converter-test+docs)` as an ERE means "converter-tes" then one-or-more "t" then "docs)", which
  #   matches nothing -- and the arm would then report a correctly implemented class as missing while
  #   every path read "outside shape".  The live values are still read out of THIS case block, which
  #   is the point: a retyped copy of a pattern is the thing that drifts.
  arm_of(){ awk -v c="$1)" '{ s=$0; sub(/^[ \\t]+/,"",s); if (substr(s,1,length(c))==c) { print; exit } }' "$F"; }
  shape_of(){ arm_of "$1" | sed -E "s/.*shapepat='([^']*)'.*/\\1/"; }''',
                   'sc-shape-of')
S.subline_contains('  forb_of(){  grep -E "^ *$1\\)" "$F"',
                   '  forb_of(){  arm_of "$1" | sed -E "s/.*forbidden=\'([^\']*)\'.*/\\1/"; }',
                   'sc-forb-of')
S.subline_contains("while IFS='|' read -r n b s c m; do\n", None, 'unused-placeholder', want=0) if False else None
S.gsub("while IFS='|' read -r n b s c m; do", "while IFS='|' read -r n b s c m a; do", 'sc-row-reads', minimum=1)

# the three decoys keep their meaning; the `| grep -q` forms become counts
S.gsub("if printf '%s\\n' 'src/tests/Behavioral/Anything/main.cs' | grep -qE \"$D1\"; then",
       "if [ \"$(printf '%s\\n' 'src/tests/Behavioral/Anything/main.cs' | grep -acE \"$D1\" || true)\" != \"0\" ]; then", 'sc-decoy1')
S.gsub("if printf '%s\\n' 'src/core/golib/slice.cs' | grep -qE \"$D2\"; then",
       "if [ \"$(printf '%s\\n' 'src/core/golib/slice.cs' | grep -acE \"$D2\" || true)\" != \"0\" ]; then", 'sc-decoy2')
S.gsub("if printf '%s\\n' 'src/gen/go2cs-gen/Generators/ImplementGenerator.cs' | grep -qE \"$D3\"; then",
       "if [ \"$(printf '%s\\n' 'src/gen/go2cs-gen/Generators/ImplementGenerator.cs' | grep -acE \"$D3\" || true)\" != \"0\" ]; then", 'sc-decoy3')
S.subline_contains("if printf '%s' \"$s\" | grep -qE '^[0-9a-f]{9}$'; then : ; else",
                   "  if [ \"$(printf '%s' \"$s\" | grep -acE '^[0-9a-f]{9}$' || true)\" = \"0\" ]; then",
                   'sc-sha-shape', want=0) if False else None
S.subline_contains("eval \"$(blk \"^LEGK_ORACLE_RE=\" '^legk_oracle_ok\\(\\)' 'LEGK oracle predicate')\" || exit 3",
                   '''eval "$(blk "^LEGK_ORACLE_RE=" '^legk_oracle_ok\\(\\)' 'LEGK oracle predicate')" || exit 3
# ⚠ THE PREDICATE'S OWN BODY IS EXTRACTED TOO, so what runs below is the assembly's live definition
#   and not a copy: the block above stops one line short of it, and this pulls the function itself.
eval "$(grep -a '^legk_oracle_ok()' "$F" | head -1)"''',
                   'sc-oracle-body')

# ---- ARM 12: the five mechanised lessons, each with its negative control -------------------------
SC12 = r"""
say ""
say "== 12. THE FIVE MECHANISED LESSONS, each with a NEGATIVE CONTROL against the train-46 originals =="
say "   ⚠ EVERY ONE OF THESE IS A DEFECT A TRAIN-46 LAUNCH ACTUALLY HIT.  Each arm must read ZERO on"
say "     the train-47 file AND NON-ZERO on the train-46 one: an arm that has never been made to fire"
say "     is an assertion wearing a measurement's clothes, and this project has already shipped a"
say "     positive control that stayed green because the thing it neutered was subsumed elsewhere."
say "   ⚠ A CONTROL THAT CANNOT RUN IS **UNMEASURED AND FAILS**.  If the train-46 originals are not"
say "     beside this script (override with T46DIR=<dir>), the arm reports that and does not pass:"
say "     an instrument that could not run has not found nothing, it has found nothing out."

# --- the five predicates.  ONE definition each, run over whatever file it is handed, so the arm and
#     its control are literally the same code -- which is what makes the control a control.
#     ⚠ EVERY ONE OF THEM READS **CODE ONLY**.  A file that documents the defect it forbids
#     necessarily SPELLS it, and a predicate that counted the header prose would refuse the very
#     sentence explaining the rule -- route #8's text-grepping family, met from the checker's side.
#     The comment hits are a different claim and are printed separately below.
codeonly(){ grep -av '^[[:space:]]*#' "$1" 2>/dev/null; }
l1_count(){ codeonly "$1" | grep -ac 'SEATS_LISTED" = "[0-9]' | tr -d '\r'; }
l3_count(){ if [ "$(codeonly "$1" | grep -ac 'OWED_CLAUDEMD')" = "0" ]; then echo 1; else echo 0; fi; }
l4_count(){ codeonly "$1" | grep -acE 'case "\$\{[A-Za-z_][A-Za-z0-9_]*:-[^}]*\}" in .{0,4}'"''" | tr -d '\r'; }
l9_count(){ codeonly "$1" | grep -ac -- '| grep -q' | tr -d '\r'; }
l11_count(){ codeonly "$1" | awk '/(^|[^A-Za-z0-9_])FAILED=1/ { if ($0 !~ /stamp/ && prev !~ /stamp/) c++ } { prev=$0 } END{ print c+0 }'; }

lesson(){ # $1 label  $2 predicate  $3 the train-47 file  $4 the train-46 original  $5 what it measures
  local lab="$1" fn="$2" new="$3" old="$4" what="$5" n o
  n=$("$fn" "$new"); n="${n:-0}"
  if [ ! -f "$old" ]; then
    say "   $lab :: train47=$n (want 0) :: ⚠ **CONTROL UNMEASURED** -- [$old] is not present, so the arm has not been shown able to fire.  $what"
    FAIL=1; return
  fi
  o=$("$fn" "$old"); o="${o:-0}"
  if [ "$n" = "0" ] && [ "${o:-0}" -ge 1 ]; then
    say "   PASS $lab :: train47=$n train46=$o -- the arm reads ZERO on the derived file and FIRES on the original.  $what"
  elif [ "$n" != "0" ]; then
    say "   FAIL $lab :: train47=$n (want 0) train46=$o -- the defect SURVIVED the derive.  $what"
    FAIL=1
  else
    say "   FAIL $lab (CONTROL) :: train47=$n train46=$o (want >= 1) -- the arm did NOT fire on the file that carries the defect, so its zero on the derived file says nothing.  $what"
    FAIL=1
  fi
}

# ⚠ THE CONTROL FILENAMES ARE **COMPOSED**, NOT SPELLED.  The derive that writes this file runs a
#   global `coord-train46-* -> coord-train47-*` rename over it, and a spelled name here would be
#   rewritten to point the control at the file it is supposed to be controlling AGAINST -- which is
#   exactly what happened on this arm's first run: L3 and L11 read their own output as the original
#   and reported the control as dead.  An instrument built out of the thing under test cannot
#   independently measure it.
T46NUM=46
T46ASM="$T46DIR/coord-train${T46NUM}-assemble.sh"
T46LAND="$T46DIR/coord-train${T46NUM}-land.sh"
LAND47="$SELFDIR/coord-train47-land.sh"
say "   the originals this arm controls against :: $T46ASM :: $T46LAND"
lesson 'L1 no literal seat count' l1_count "$F" "$T46ASM" \
  "Train 46 run 1 merged all six seats and then refused on \`[ \"\$SEATS_LISTED\" = \"5\" ]\` -- a fact about the derive, not about the train.  The count is now DERIVED and what is asserted is the table's STRUCTURE."
lesson 'L3 G4 derived from the classes' l3_count "$F" "$T46ASM" \
  "Train 46 run 3 refused with 'CLAUDE.md is UNTOUCHED ... seat 1 IS the doctrine batch' -- train 45's premise carried into a train with no doctrine seat.  G4 now reads OWED_CLAUDEMD and refuses only the MISMATCH, in either direction."
# X L4's CONTROL IS POINTED AT THE **LAND** AND **DRY-READ** FILES, NOT THE ASSEMBLY, AND THAT IS A
#   MEASUREMENT RATHER THAN A CHOICE: train 46's G10a site was CORRECTED IN FLIGHT after run 3
#   refused on it, so the train-46 assembly no longer carries the shape and a control against it
#   reads a dead ZERO.  The identical shape DID survive in the land script and in its dry-read,
#   where nothing on that train was looking -- a defect fixed in one file and left standing in
#   another is a defect that survives the derive.  The control therefore fires where the defect
#   lives, and the assembly's own zero is reported beside it as an ASSERTION carried by the SAME
#   predicate -- which is the only reason that zero is worth anything.
say "   reading  L4 (assembly) :: train47=$(l4_count "$F") train46=$(l4_count "$T46ASM") -- BOTH zero is EXPECTED here: the train-46 site was corrected in flight at run 3, so this predicate's control lives on the two files below"
[ "$(l4_count "$F")" = "0" ] || { say "   FAIL L4 (assembly) :: the case-default shape is present in the derived assembly"; FAIL=1; }
lesson 'L4 no case-default (land)' l4_count "$LAND47" "$T46LAND" \
  "The shape survived in the LAND script, where nothing in train 46 looked for it.  ${VAR:-0} substitutes before the '' arm can be reached, so the empty case is unreachable and the value is never normalised -- which is how G10a printed 'deletions=' EMPTY and refused."
lesson 'L4 no case-default (dry-read)' l4_count "$SELFDIR/coord-train47-land-dryread.sh" "$T46DIR/coord-train${T46NUM}-land-dryread.sh" \
  "And in the DRY-READ, whose whole job is to check the land script -- a checker carrying the very defect it checks for is the shape this arm exists to catch."
lesson 'L9 zero | grep -q' l9_count "$F" "$T46ASM" \
  "Under \`set -o pipefail\` a \`grep -q\` SIGPIPEs its producer and a TRUE match reads as a failure.  This tree does not set pipefail; carrying the shape anyway leaves a landmine per site for the derive that does."
lesson 'L11 every FAILED=1 names its gate' l11_count "$F" "$T46ASM" \
  "Train 46 carried FIFTEEN setters with no stamp on the same or the previous line; a reader scanning for the flag could not tell which gate set it, and one of them cost a run its attribution.  Each is now fail_gate <GATE>."

# --- and the two arms that are ASSERTIONS about the derived file alone (no control is possible: they
#     assert the PRESENCE of a mechanism the original does not have at all, and 'absent from the
#     original' is what every other arm here already measures).
for tok in 'OWED VECTOR ::' 'SEAT-CONTENT ASSERTIONS ::' 'fail_gate(){' 'A7 ALLOWED SET ::' 'CENSUS_TEMPLATE=' 'CONTROL C1/G6T template class'; do
  n=$(grep -acF -- "$tok" "$F" || true)
  if [ "${n:-0}" -ge 1 ]; then say "   PASS mechanism present :: [$tok] x$n"
  else say "   FAIL mechanism ABSENT :: [$tok] -- the derive did not land it"; FAIL=1; fi
done
say "   ⚠ CENSUS_TEMPLATE AND THE C1/G6T SIX-ARM CONTROL ARE CARRIED **BYTE-FOR-BYTE** FROM TRAIN 46."
say "     They were ruled after run 5 read residual=11 on the security guard's own Sprintf fixtures;"
say "     they are proven, and a derive that retyped them would be re-deriving a measured answer."
say "   ⚠ AND THE set-u/pipefail RATIONALE MUST BE **STATED IN EVERY HEADER**, because a rule whose reason is"
say "     not written down is a rule the next derive silently 'improves'."
for f in "$F" "$SELFDIR/coord-train47-rehearse.sh" "$LAND47" "$SELFDIR/coord-train47-land-dryread.sh" "${BASH_SOURCE[0]}"; do
  [ -f "$f" ] || { say "   FAIL: [$f] is not present, so its header could not be read"; FAIL=1; continue; }
  n=$(grep -ac 'NOT.*pipefail\|not.*pipefail' "$f" || true)
  p=$(grep -ac -- '| grep -q' "$f" || true)
  if [ "${n:-0}" -ge 1 ]; then say "   ok   $(basename "$f") :: states the set -u/pipefail rationale ($n line(s)) :: '| grep -q' sites=$p"
  else say "   FAIL $(basename "$f") :: does NOT state why it is \`set -u\` and not pipefail"; FAIL=1; fi
done
"""
# ⚠ THE ARM IS APPENDED AT THE **TAIL**, ANCHORED ON THE LINE THAT CLOSES ARM 11.  A substitution on
#   the DONE line alone would have matched TWICE -- the second occurrence is inside arm 11's
#   nested-invocation early exit -- and arm 12 would then have run inside its own negative control,
#   which is the shape of an instrument measuring itself.
S.block_to_eof(r'^rm -f "\$BRK"$',
               SC12 + '\nsay ""\nsay "=== SELF-CHECK DONE overallFail=$FAIL ==="\nexit $FAIL\n',
               'sc-arm12', start_off=1)
# ⚠ NOTHING TO RENAME, AND THAT IS ASSERTED.  The header block carried the only mention, and arm 12
#   COMPOSES the train-46 control filenames from a number rather than spelling them -- because a
#   rename here would otherwise point the control at the very file it controls against, which is
#   what it did on this arm's first run.
S.absent('coord-train46-assemble', 'sc-selfname')
# ⚠ the self-check's own name lived only in its header usage line, which was replaced WHOLE; the
#   absence assertion below is what says so rather than a gsub reporting a silent zero.
S.absent('coord-train46-derive-selfcheck', 'sc-selfname2')
# ⚠ TAG lived only in the header block, replaced whole; asserted rather than re-substituted.
S.absent('t46selfcheck', 'sc-tag')
S.absent('coord-train46-assemble', 'sc-no-46-asm')
SC_LINES = S.write('coord-train47-derive-selfcheck.sh')


# ============================================================================================
# THE LAND SCRIPT
# ============================================================================================
L = Doc('coord-train46-land.sh')

LHEADER = r"""#!/bin/bash
# ============================================================================================
# TRAIN 47 LAND.  DERIVED from coord-train46-land.sh: same skeleton -- read the assembly's own stdout
# for its DONE stamp and its NAMED gate stamps, do the tree arithmetic, and push NOTHING unless every
# read agrees.  LAND_VERIFY_ONLY=1 runs every gate and pushes nothing.
#
# ⚠⚠ **THIS FILE CARRIES NO NAMED ACCEPTANCE PATH, AND THAT IS THE POINT.**  Train 45's land grew
#   FIVE of them -- G10D-TEXTATTR, LEGD-STANDALONE, LEGD-LINUX-LINEFORM, LEG4-DRIFT-REBASELINED and
#   BEHAVIORAL-FIXUP -- each for a real fault measured on that train, and every one of those faults
#   was fixed IN THE ASSEMBLY SCRIPT rather than accepted here.  Train 46's land carried none, and
#   this one carries none.  ⚠ A FRESH TRAIN STARTS WITH ZERO ACCEPTED CLASSES.  An acceptance path
#   that outlives its fault is a LIE-LEVER: it accepts a SHAPE, and the next thing that produces that
#   shape may be a real defect.  If this train needs one, it is written HERE, against a MEASUREMENT,
#   with its own control -- and the next derive deletes it again.  The dry-read asserts the absence.
#
# ⚠ THE **ONE** EXCLUSION THAT SURVIVES IS THE WRAPPER'S TRAILING `assembly exit=N` LINE, and it is
#   not an acceptance path.  The battery is launched by a shell that echoes the assembly script's
#   exit status after it ends; that echo lands in the record as its LAST line.  It is NOT a leg stamp
#   -- no leg printed it, none can be attributed to it -- and it carries the SAME FACT the DONE stamp
#   already carries as overallFailed=N.  Its three conditions are ALL MEASURED and it fires ONLY when
#   the composition accepted.  ⚠ THE WRAPPER MUST **END IN `exit $rc`**: a wrapper whose last
#   statement is a pipe reports the LAST command's status, so a script that exited 1 is announced 0.
#
# ⚠⚠ **THE DIRECTORY ARITHMETIC IS DRIVEN BY THE ASSEMBLY'S OWN `OWED VECTOR` STAMP.**  Train 46's
#   land required src/gen, golib, GolibTests, Behavioral and docs to move -- correct for that train
#   and a premise about it.  The assembly derives what is OWED from the CLASSES of the rows that
#   merged and STAMPS the vector; this file PARSES that line rather than re-deriving it, because two
#   instruments deriving one fact independently is how they drift.  A record with no such line is a
#   record this gate cannot read, and it says so rather than defaulting to "nothing is owed".
#
# ⚠ EVERY GATE HERE IS A **READ** OF A RECORD PLUS TREE ARITHMETIC.  Nothing is re-run: the assembly
#   is the measurement and this is the reconciliation.  A stamp that is absent is a leg that did not
#   report it, which is not the same as a leg that failed -- and it is never a pass either.
#
# ⚠ `set -u` ONLY -- **NOT** `set -o pipefail`: under pipefail a `grep -q` SIGPIPEs its producer and a
#   TRUE match reads as a failure.  This file carries ZERO `| grep -q` pipes for the same reason.
#
#   usage:  bash coord-train47-land.sh                     # gates, then LEASE-PUSH master
#           LAND_VERIFY_ONLY=1 bash coord-train47-land.sh  # gates only, nothing pushed
#           LAND_ASM=<path> bash coord-train47-land.sh     # a differently-named assembly record
#           LAND_WT=<dir>   bash coord-train47-land.sh     # the assembly worktree (⚠ REQUIRED: it is
#                                                          #   a FILL POINT, exactly as in the assembly)
#   exit 0 landed (or verify-only clean) | 2 wrong tree / dirty / head mismatch / an unfilled fill point |
#        3 the assembly record is missing, empty, or did not finish | 4 a gate failed (nothing pushed) |
#        5 the push did not land
# ============================================================================================
set -u
# ⚠ **FILL POINT.**  The worktree is NOT inherited from the previous train: a land script pointing at
#   a neighbour's worktree would read a tree another battery may be holding.  LAND_WT names it.
WT="${LAND_WT:-PENDING}"
case "$WT" in
  PENDING|''|*PENDING*) echo "LAND WORKTREE UNFILLED :: set LAND_WT=<the assembly worktree>.  A land script that guesses its tree is a landing nobody can bound -- ABORT"; exit 2 ;;
esac
WT_OVERRIDE=YES"""
L.block(r'^#!/bin/bash$', r'^WT_OVERRIDE=\$\( \[ "\$WT" = "\$WT_DEFAULT" \]', LHEADER, 'land-header')

L.subline(r'^BASE_EXPECT=44f858717 ',
          '# --- THE BASE, READ OUT OF THE ASSEMBLY SCRIPT rather than retyped here.  Two copies of one\n'
          '#     SHA is the thing that drifts, and this file already reads the assembly for everything else.\n'
          'LAND_SELFDIR=$(cd "$(dirname "${LAND_SELF_ABS:-$0}")" 2>/dev/null && pwd) || LAND_SELFDIR=.\n'
          'BASE_EXPECT=$(grep -aE "^EXPECT_46=\'" "$LAND_SELFDIR/coord-train47-assemble.sh" 2>/dev/null | head -1 | sed -E "s/^EXPECT_46=\'([^\']*)\'.*/\\1/")\n'
          '[ -n "$BASE_EXPECT" ] && [ "$BASE_EXPECT" != "PENDING" ] || { echo "LAND BASE UNREADABLE/PENDING :: the assembly script declares EXPECT_46=[${BASE_EXPECT:-absent}].  A landing cannot be gated against a placeholder base -- ABORT"; exit 2; }',
          'land-base')

L.gsub("ASM=\"${LAND_ASM:-$SELFDIR/coord-train46-assemble-run1.stdout}\"",
       "ASM=\"${LAND_ASM:-$SELFDIR/coord-train47-assemble-run1.stdout}\"", 'land-asm-default')
L.gsub("=== train46 ASSEMBLE DONE", "=== train47 ASSEMBLE DONE", 'land-done-anchor')
L.gsub('coord-train46-assemble', 'coord-train47-assemble', 'land-asm-name')
L.gsub("TRAIN46_REQUIRE_ALL", "TRAIN47_REQUIRE_ALL", 'land-requireall')
L.gsub("coord-train46-land", "coord-train47-land", 'land-selfname')
L.gsub("TRAIN 46 LAND", "TRAIN 47 LAND", 'land-title')
# ⚠ THE BASE-ASSERTION ANCHOR IS RE-POINTED, NOT CARRIED.  Train 46's stamp said "derived-time base";
#   this template's says "this template's declared base", and an anchor whose words no longer appear
#   anywhere in the assembly can NEVER be satisfied -- the landing would then refuse a HEALTHY
#   battery, and the operator reaches for the switch that makes it stop refusing.  The dry-read's
#   ARM A is what found this one.
LAND_BASE_REQ = (
    "req 'BASE ASSERTION ::' 1 \"the assembly ASSERTED its own base rather than deriving BASE from "
    "HEAD and carrying on -- a gate reading has a TREE, and when the tree moves the reading expires "
    "whether or not the change does.  ⚠ It also REFUSES a PENDING base: this train was derived as "
    "a TEMPLATE, and a placeholder base that RUNS is a battery measuring a tree nobody named\"")
L.subline_contains("req 'BASE ASSERTION :: derived-time base' 1", LAND_BASE_REQ, 'land-base-anchor')

# the OWED VECTOR read
L.subline_contains('HEAD=$(git rev-parse --short HEAD)',
                   '''HEAD=$(git rev-parse --short HEAD)

# --- ⚠ THE OWED VECTOR, PARSED FROM THE ASSEMBLY'S OWN STAMP.  It is the assembly that knows which
#     CLASSES merged, so it derives the vector and stamps it; this file READS that line.  A record
#     with no such line is a record this gate cannot read -- reported as its own state, never
#     defaulted to "nothing is owed", because a default there would silently turn every directory
#     requirement below into a pass.
OWEDLINE=$(grep -a 'OWED VECTOR ::' "$ASM" | tail -1 | tr -d '\\r')
if [ -z "$OWEDLINE" ]; then
  stamp "OWED VECTOR :: **ABSENT from the record** -- this landing cannot know what the train's classes owed, and a missing vector must not read as 'nothing owed'.  ABORT"
  exit 3
fi
owed_of(){ printf '%s' "$OWEDLINE" | grep -oE "$1=[01]" | head -1 | cut -d= -f2; }
O_GOLIB=$(owed_of golib); O_GEN=$(owed_of gen); O_CONV=$(owed_of converter); O_BEH=$(owed_of behavioral)
O_CORPUS=$(owed_of corpus); O_GT=$(owed_of golibtests); O_DOCS=$(owed_of docs); O_CLAUDEMD=$(owed_of claudemd); O_MANIFEST=$(owed_of manifest)
for v in O_GOLIB O_GEN O_CONV O_BEH O_CORPUS O_GT O_DOCS O_CLAUDEMD O_MANIFEST; do eval "case \\"\\${$v}\\" in ''|*[!01]*) $v=0 ;; esac"; done
stamp "OWED VECTOR read from the record :: golib=$O_GOLIB gen=$O_GEN converter=$O_CONV behavioral=$O_BEH corpus=$O_CORPUS golibtests=$O_GT docs=$O_DOCS claudemd=$O_CLAUDEMD manifest=$O_MANIFEST -- the directory arithmetic below is driven by THIS, not by a premise about which seat was which"''',
                   'land-owed-vector')

# the seat-specific req anchors are replaced by the generic ones
LREQ = r"""# --- the A-assertions this train's SHAPE guarantees, whatever its seats are.
#     ⚠ TRAIN 46's SEAT-SPECIFIC ANCHORS (A2's ISlice.IsNil, A3's registry-and-displacement, A5 at a
#     count of two) ARE **GONE**, NOT RE-POINTED.  They named that train's seats; an anchor whose
#     words no longer appear anywhere in the assembly script can never be satisfied, so the landing
#     would refuse a HEALTHY battery -- and the operator then reaches for the switch that makes it
#     stop refusing.  What survives is what a TEMPLATE can guarantee: that the per-seat assertions
#     RAN (one arm per merged seat, asserted by the assembly itself), that the table was
#     structurally sound, and that the OWED vector was derived and stamped.
req 'SEAT TABLE ROWS ARE STRUCTURALLY SOUND ::' 1 'the seat table was structurally checked -- contiguous numbers, unique refs, unique filled SHAs -- rather than compared against a row-count LITERAL, which is what refused a correct six-row table on train 46 AFTER merging all six'
req 'OWED VECTOR ::' 1 'the assembly DERIVED what this train owes from the CLASSES of the rows that merged, and stamped it.  ⚠ That stamp is what THIS file reads: two instruments deriving one fact independently is how they drift'
req 'SEAT-CONTENT ASSERTIONS :: arms=' 1 'the per-seat content assertions reported their count.  ⚠ A battery over a tree whose seats nobody asserted measures a tree nobody ruled on, and the assembly refuses when the arm count is below the merged-seat count'
req 'A7 ALLOWED SET ::' 1 "A7 stated its allowed set BEFORE any merge -- either the STRICT form (no committed behavioral file may be modified) or the rows whose own \`allowed=\` ruling admits one.  An exemption nobody can see is an exemption nobody re-reads"
req 'A5 TOP-LEVEL [A-Za-z0-9_]+ :: missingFiles=\[ none\]|A5 :: this train adds NO new behavioral' 1 'A5 either verified every new TOP-LEVEL guard project is COMPLETE, or stated that this train adds none'
req 'A6 pins UNMOVED ::' 1 'A6 the toolchain declaration and the corpus pin are unmoved and no seat touched a pin file'"""
L.block(r"^req 'A5 TOP-LEVEL \[A-Za-z0-9_\]\+ :: missingFiles", r"^req 'A6 pins UNMOVED ::'", LREQ, 'land-req-seatspecific')

# the directory arithmetic, driven by the vector
LARITH = r"""stamp "delta by directory :: src/go2cs=$D_CONV src/go2cs.slnx=$D_SLNX src/core/golib=$D_GOLIB src/gen=$D_GEN src/tests/GolibTests=$D_GT src/tests/Behavioral=$D_BEH src/core=$D_CORE docs=$D_DOCS :: behavioral files MODIFIED or DELETED other than the four MSTest classes=$D_BEHMOD"
stamp "⚠ WHICH OF THOSE ARE **REQUIRED** IS THE OWED VECTOR'S TO SAY, NOT THIS FILE'S.  Train 46's land required src/gen to be non-zero and train 45's required it to be ZERO -- two opposite assertions, each correct about its own train and neither about the next.  The vector is derived from the CLASSES the assembly merged."
LARBAD=0
land_arm(){ # $1 owed  $2 measured  $3 name  $4 why
  if [ "$1" = "1" ]; then
    if [ "${2:-0}" -ge 1 ]; then stamp "  owed and present :: $3=$2 -- $4"
    else stamp "  ^ OWED AND ABSENT :: $3=$2 -- a seat did not land what its CLASS says it is, and every heavy leg was then measuring nothing new in $3"; LARBAD=1; fi
  else
    stamp "  not owed :: $3=$2 (a READING; no merged class owes it)"
  fi
}
land_arm "$O_CONV"     "$D_CONV"   'src/go2cs'            'LEG C, LEG Cg, LEG R and LEG D are the converter obligations'
land_arm "$O_GOLIB"    "$D_GOLIB"  'src/core/golib'       'LEG 2, LEG 2b, LEG 3 at both configurations and LEG 5'
land_arm "$O_GEN"      "$D_GEN"    'src/gen'              'ROUTE #7: invisible to CNR and to the stdlib solution alike; LEG 2b and LEG 5 COMPILE are its gates'
land_arm "$O_GT"       "$D_GT"     'src/tests/GolibTests' 'LEG 3 runs them at BOTH configurations'
land_arm "$O_BEH"      "$D_BEH"    'src/tests/Behavioral' 'LEG 5 compiles and runs the new guards, nested sub-libraries included'
land_arm "$O_CORPUS"   "$D_CORE"   'src/core'             'LEG 2 compiles the corpus and LEG D measures its emission'
land_arm "$O_CLAUDEMD" "$(git -c core.quotepath=false diff --name-only $BASE_EXPECT..HEAD -- CLAUDE.md | grep -c . || true)" 'CLAUDE.md' 'G4 runs with teeth on a doctrine batch'
[ "$LARBAD" = "0" ] || FAIL=1
# --- src/go2cs.slnx is its own arm: a solution registration is owed by the PROJECT COUNT, not by a
#     class, and the registration arithmetic below is what checks it.
stamp "  reading :: src/go2cs.slnx=$D_SLNX (the registration arithmetic below is what makes this a check rather than a number)"
[ "$D_BEHMOD" = "0" ] || { stamp "  ^ a committed behavioral file other than the four MSTest classes was MODIFIED or DELETED.  A CHANGED golden is a RE-BASELINE, and a re-baseline is a ruling rather than a merge -- the assembly's A7 admits one only when the seat's own table row declares it.  Files:"; git -c core.quotepath=false diff --name-only --diff-filter=MD $BASE_EXPECT..HEAD -- src/tests/Behavioral | grep -avE '^src/tests/Behavioral/BehavioralTests/[^/]+Tests\.cs$' | sed 's/^/    /' | tee -a "$LOG"; FAIL=1; }"""
L.block(r'^stamp "delta by directory ::', r'^\[ "\$D_BEHMOD" = "0" \]', LARITH, 'land-arithmetic')

# the case-default shape
L.subline_contains("case \"${c:-0}\" in ''|0) SCANDEAD=",
                   '''  # ⚠ THE **RAW** VALUE IS TESTED.  `${c:-0}` substitutes before the '' arm can be reached, so the
  #   empty case is unreachable and an empty count silently becomes "0" for the wrong reason -- the
  #   shape that made G10a print `deletions=` EMPTY and refuse on train 46 run 3.
  case "$c" in ''|0) SCANDEAD=$((SCANDEAD+1)); stamp "  ^ REFUSAL-SCAN CONTROL: pattern [$bad] found NOTHING on a planted line of its own shape -- that pattern is DEAD" ;; esac''',
                   'land-case-raw')
L.subline_contains('printf \'%s\' "$WX_LAST" | grep -qE -- "$WRAPEXIT_PAT" && WX_ISLAST=1 || WX_ISLAST=0',
                   'if [ "$(printf \'%s\' "$WX_LAST" | grep -acE -- "$WRAPEXIT_PAT" || true)" != "0" ]; then WX_ISLAST=1; else WX_ISLAST=0; fi',
                   'land-grepq')

# the landing notes become the template's own
LNOTES = r"""stamp "LANDING NOTES :: (1) **THIS LANDING EXCLUDED NOTHING FROM ITS REFUSAL SCAN.**  Train 45's land carried FIVE named acceptance paths, each for a real fault; every one of those faults was fixed IN THE INSTRUMENT rather than accepted, and a fresh train starts with zero accepted classes.  The dry-read asserts that as CODE, with the record of WHY kept as prose."
stamp "LANDING NOTES :: (2) THE DIRECTORY ARITHMETIC ABOVE WAS DRIVEN BY THE ASSEMBLY'S OWN **OWED VECTOR**, derived from the CLASSES of the rows that merged.  Train 45's land required src/gen to be ZERO and train 46's required it to be NON-ZERO -- two opposite assertions, each correct about its own train.  A premise about which seat is which does not survive a derive; a vector derived at run time does."
stamp "LANDING NOTES :: (3) THE SEAT TABLE WAS CHECKED **STRUCTURALLY**, NOT AGAINST A ROW-COUNT LITERAL.  Train 46 run 1 merged every seat and then refused because the literal said five.  What a hand-edited table actually breaks -- contiguous numbering, unique refs, unique pins -- is what both the assembly and its self-check assert."
stamp "LANDING NOTES :: (4) EVERY MERGED SEAT HAD A **CONTENT ASSERTION**.  The assembly refuses when the arm count is below the merged-seat count: a merge that resolved a hunk the wrong way is caught by CONTENT, and ancestry cannot see it."
stamp "LANDING NOTES :: (5) ANY RE-BASELINED GOLDEN CAME IN THROUGH ITS OWN SEAT'S \`allowed=\` RULING, and A7 admitted it only when the union's BLOB equalled that seat's own -- so the exemption carried nothing another seat rode in on."
stamp "LANDING NOTES :: (6) the converter's toolchain stays go1.24.13 and the corpus pin stays 1.23.12 -- A6 asserted both on the assembled tree and the delta carries zero pin files."
"""
L.block(r'^stamp "LANDING NOTES :: \(1\)', r'^stamp "LANDING NOTES :: \(6\)', LNOTES, 'land-notes')

L.absent_in_code('grep -q', 'land-no-grep-q')
L.absent_in_code('pipefail', 'land-no-pipefail')
L.absent('coord-train46', 'land-no-46-path')
LAND_LINES = L.write('coord-train47-land.sh')


# ============================================================================================
# THE LAND DRY-READ
# ============================================================================================
D = Doc('coord-train46-land-dryread.sh')

D.gsub('coord-train46-land-dryread', 'coord-train47-land-dryread', 'dr-selfname')
DR_SETU = r"""# ⚠ `set -u` ONLY -- **NOT** `set -o pipefail`: under pipefail a `grep -q` SIGPIPEs its producer and
#   a TRUE match reads as a failure.  This file carries ZERO `| grep -q` pipes for the same reason,
#   and its sibling self-check asserts that every one of the five scripts states this.
#
#   usage:  bash coord-train47-land-dryread.sh"""
# ⚠ THE ANCHOR IS THE **RENAMED** LINE: the self-name gsub above has already run, so anchoring on
#   the train-46 spelling would match nothing and report a silent zero.
D.subline_contains('#   usage:  bash coord-train47-land-dryread.sh', DR_SETU, 'dr-setu-rationale')
D.gsub('coord-train46-land.sh', 'coord-train47-land.sh', 'dr-land')
D.gsub('coord-train46-assemble.sh', 'coord-train47-assemble.sh', 'dr-asm')
D.gsub('TRAIN 46 LAND -- DRY READ', 'TRAIN 47 LAND -- DRY READ', 'dr-title')
D.gsub('t46-dryread', 't47-dryread', 'dr-tmp')
D.gsub('TRAIN 46', 'TRAIN 47', 'dr-train', minimum=1)
D.gsub('train-46', 'train-47', 'dr-train2', minimum=1)
# ⚠ the dry-read carries no bare 'train 46' prose (its train references are hyphenated or upper);
#   asserted rather than substituted, so a zero is a statement instead of a silent no-op.
D.absent('train 46', 'dr-train3')

D.subline_contains("case \"${c:-0}\" in ''|0) DEAD=",
                   '''  # ⚠ THE **RAW** VALUE IS TESTED: `${c:-0}` substitutes before the '' arm can be reached, so the
  #   empty case is unreachable -- the shape that made G10a print `deletions=` EMPTY on train 46 run 3.
  case "$c" in ''|0) DEAD=$(( DEAD + 1 )); say "   DEAD pattern [$bad] found NOTHING on a planted line of its own shape" ;; esac''',
                   'dr-case-raw')

# ARM C's control words must be words THIS train's assembly does not have, even as prose
D.subline_contains("for bad in 'FinalizerBindingTests' 'coord-doctrine-batch18'",
                   "for bad in 'FinalizerBindingTests' 'coord-doctrine-batch18' 'AliasNamespaceShadow' 'SliceTypeParamNil' 'RefLoweredDeferChain' 'FatalReportTests' 'Q44RegistryCensus' 'WindowsNewCallback' 'laneR-golib-sema'; do",
                   'dr-armc-words')

DARMF = r"""
say ""
say "== ARM F: the LAND script's own template properties =="
say "   ⚠ THREE THINGS A TEMPLATE'S LAND MUST NOT CARRY, each measured rather than claimed."
FBAD=0
# (1) NO row-count literal, and no hardcoded seat SHA.
n=$(grep -acE 'SEATS?N?[A-Z]*" = "[0-9]+"' "$LAND" || true)
say "   seat-count literals in the land script=$n (want 0 -- the record's own DONE stamp and the assembly's structural check are what bound the table)"
[ "${n:-0}" = "0" ] || { say "   FAIL: a seat-count literal survives"; FBAD=1; }
n=$(grep -acE '\b[0-9a-f]{9}\b' "$LAND" || true)
say "   nine-hex tokens anywhere in the land script=$n (a READING: the BASE is read out of the assembly, so a literal here would be a second copy of a SHA)"
# (2) the OWED VECTOR is READ, and its absence ABORTS rather than defaulting.
for tok in 'OWED VECTOR ::' 'ABSENT from the record' 'owed_of()'; do
  c=$(grep -acF -- "$tok" "$LAND" || true)
  if [ "${c:-0}" -ge 1 ]; then say "   ok   the land script reads [$tok]"
  else say "   FAIL: [$tok] is absent -- the directory arithmetic would then rest on a premise about which seat was which"; FBAD=1; fi
done
# (3) the WORKTREE is a FILL POINT that refuses.
c=$(grep -acF -- 'LAND WORKTREE UNFILLED' "$LAND" || true)
if [ "${c:-0}" -ge 1 ]; then say "   ok   an unfilled LAND_WT ABORTS"
else say "   FAIL: the land script would guess its worktree"; FBAD=1; fi
[ "$FBAD" = "0" ] && say "   PASS: the land script carries no seat-count literal, reads the OWED vector, and refuses an unfilled worktree" || FAIL=1
"""
D.block_to_eof(r'PASS: the refusal-scan exclusion is a constant nothing can change',
               DARMF + '\nsay ""\nsay "=== DRY READ DONE overallFail=$FAIL ==="\nexit $FAIL\n',
               'dr-armf', start_off=1)
D.absent_in_code('grep -q', 'dr-no-grep-q')
D.absent('coord-train46', 'dr-no-46-path')
DR_LINES = D.write('coord-train47-land-dryread.sh')


# ============================================================================================
# THE REPORT
# ============================================================================================
print('=== TRAIN 47 TEMPLATE DERIVE ===')
print('interpreter: %s' % sys.version.split()[0])
TOTAL = 0
for doc, dst, out_lines in ((A, 'coord-train47-assemble.sh', ASM_LINES),
                            (R, 'coord-train47-rehearse.sh', REH_LINES),
                            (S, 'coord-train47-derive-selfcheck.sh', SC_LINES),
                            (L, 'coord-train47-land.sh', LAND_LINES),
                            (D, 'coord-train47-land-dryread.sh', DR_LINES)):
    print('')
    print('%s  <- %s   %d line(s) -> %d line(s)   %d asserted operation(s)'
          % (dst, doc.src, doc.orig_n, out_lines, doc.ops))
    for r in doc.report:
        print('    ' + r)
    TOTAL += doc.ops
print('')
print('TOTAL asserted operations: %d' % TOTAL)
print('=== DERIVE DONE ===')
