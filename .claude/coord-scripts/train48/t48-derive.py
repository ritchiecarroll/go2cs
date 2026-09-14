#!/usr/bin/env python3
# ============================================================================================
# t48-derive.py -- DERIVE THE TRAIN 48 TEMPLATE FROM THE TRAIN 47 FILES.
#
# Run it with the Windows CPython on this box:   python t48-derive.py
# (`python3` here resolves to the WindowsApps stub and does nothing.)
#
# ⚠ IT READS train47/ AND WRITES train48/, AND IT NEVER WRITES A TRAIN-47 FILE.  The train-47
#   instrument is LIVE (a battery was running in its worktree while this was written) and bash reads a
#   script incrementally BY BYTE OFFSET, so an insertion into a file a live chain is executing
#   reparses its next command from the middle of a line.  The write guard below is mechanical, not
#   remembered.
#
# ⚠ THE COPY **IS** THIS SCRIPT.  Step 1's "copy the five files and rename every train47 token" is
#   performed HERE as asserted gsub operations rather than by a separate `cp` pass, for the reason the
#   whole method exists: a copy step followed by an editing step is two steps that can drift, and only
#   one of them asserts.  Re-running this script from the train-47 template reproduces the train-48
#   files byte-for-byte; there is no intermediate state to keep in sync.
#
# ⚠ EVERY BLOCK REPLACEMENT, EVERY LINE SUBSTITUTION, EVERY INSERTION AND EVERY GLOBAL SUBSTITUTION
#   **ASSERTS IT CHANGED SOMETHING** AND PRINTS WHAT IT DID.  A sed that matches nothing and a sed
#   that does the work exit the same way, and a derivation chain built on silent no-ops compounds
#   across generations.
#
# ⚠ IT DERIVES FIVE FILES, NOT ONE.  A defect fixed in the assembly and left standing in the land
#   script is a defect that survives the derive.
# ============================================================================================
import hashlib
import io
import os
import re
import sys

HERE = os.path.dirname(os.path.abspath(__file__))
SRC = os.path.abspath(os.path.join(HERE, '..', 'train47'))
NL = chr(10)

# ⚠ THE WRITE GUARD.  Named destinations under train48/ only; anything else is refused before a byte
#   is written.  The SOURCE directory is asserted to be train47 and the DESTINATION train48, because a
#   derive that can write its own input is the door that opens by itself.
ALLOWED_DST = {
    'coord-train48-assemble.sh',
    'coord-train48-rehearse.sh',
    'coord-train48-derive-selfcheck.sh',
    'coord-train48-land.sh',
    'coord-train48-land-dryread.sh',
}
if os.path.basename(HERE) != 'train48':
    raise SystemExit('   REFUSED: this derive must live in train48/, not [%s]' % HERE)
if os.path.basename(SRC) != 'train47':
    raise SystemExit('   REFUSED: the template source must be train47/, not [%s]' % SRC)


def sha256_of(path):
    h = hashlib.sha256()
    with io.open(path, 'rb') as f:
        h.update(f.read())
    return h.hexdigest()


class Doc(object):
    """One file under derivation.  Every operation asserts and reports."""

    def __init__(self, src):
        self.src = src
        self.path = os.path.join(SRC, src)
        with io.open(self.path, encoding='utf-8', newline='') as f:
            text = f.read()
        # ⚠ THE LINE-ENDING STYLE IS MEASURED BEFORE AND AFTER.  Every train-47 file is LF-only; a CR
        #   introduced here would be a parse hazard in a file bash reads by byte offset, and "it looked
        #   fine" is not a measurement.
        self.cr_in = text.count(chr(13))
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

    def find_unique(self, pat, label=''):
        rx = re.compile(pat)
        hits = [i for i, l in enumerate(self.lines) if rx.search(l)]
        if len(hits) != 1:
            raise SystemExit('   ANCHOR NOT UNIQUE [%s / %s]: %s matched %d line(s) %s'
                             % (self.src, label, pat, len(hits), [h + 1 for h in hits[:6]]))
        return hits[0]

    # ---- operations -------------------------------------------------------------------------
    def block(self, startpat, endpat, new, label, start_off=0, end_off=0):
        """Replace lines[a..b] INCLUSIVE with `new`.
        ⚠ THE END ANCHOR IS SEARCHED FROM THE START ANCHOR + 1, NEVER FROM `a`: with a negative
        start_off `a` sits BEFORE the anchor, and an end pattern that also matches the line at `a`
        resolves to a zero-length range -- a silent no-op wearing a replacement's clothes."""
        anchor = self.find(startpat, 0, label + '/start')
        a = anchor + start_off
        b = self.find(endpat, anchor + 1, label + '/end') + end_off
        if b < a:
            raise SystemExit('   BAD RANGE [%s / %s]: %d..%d' % (self.src, label, a + 1, b + 1))
        newl = new.split(NL)
        if newl and newl[-1] == '':
            newl.pop()
        if self.lines[a:b + 1] == newl:
            raise SystemExit('   NO-OP BLOCK [%s / %s]' % (self.src, label))
        old_n = b - a + 1
        self.lines[a:b + 1] = newl
        self.ops += 1
        self.report.append('BLOCK   %-26s lines %5d..%-5d (%4d) -> %4d' % (label, a + 1, b + 1, old_n, len(newl)))
        return a

    def block_to_eof(self, startpat, new, label, start_off=0):
        """Replace from the line matching `startpat` (plus start_off) through EOF.  ⚠ THE ANCHOR MUST
        BE UNIQUE for the same reason insert_after's must: a tail replacement anchored on the first of
        several matches silently deletes everything after whichever copy the file spells first."""
        a = self.find_unique(startpat, label) + start_off
        newl = new.split(NL)
        if newl and newl[-1] == '':
            newl.pop()
        old_n = len(self.lines) - a
        if self.lines[a:] == newl:
            raise SystemExit('   NO-OP BLOCK-TO-EOF [%s / %s]' % (self.src, label))
        self.lines[a:] = newl
        self.ops += 1
        self.report.append('TAIL    %-26s lines %5d..EOF  (%4d) -> %4d' % (label, a + 1, old_n, len(newl)))

    def insert_after(self, pat, new, label):
        """Insert `new` immediately AFTER the line matching `pat`.  ⚠ THE ANCHOR MUST BE UNIQUE: an
        insertion at the first of several matches lands in whichever copy the file happens to spell
        first, and nothing downstream would ever say so."""
        i = self.find_unique(pat, label)
        newl = new.split(NL)
        if newl and newl[-1] == '':
            newl.pop()
        self.lines[i + 1:i + 1] = newl
        self.ops += 1
        self.report.append('INSERT  %-26s after line %5d (+%d)' % (label, i + 1, len(newl)))

    def subline(self, pat, newtext, label, want=1):
        rx = re.compile(pat)
        n = 0
        for i in range(len(self.lines)):
            if rx.search(self.lines[i]):
                if self.lines[i] == newtext:
                    raise SystemExit('   NO-OP SUBLINE [%s / %s] at line %d' % (self.src, label, i + 1))
                self.lines[i] = newtext
                n += 1
        if n != want:
            raise SystemExit('   SUBLINE [%s / %s] matched %d line(s), wanted %d' % (self.src, label, n, want))
        self.ops += 1
        self.report.append('LINE    %-26s %d line(s) replaced' % (label, n))

    def subline_contains(self, sub, newtext, label, want=1):
        """Replace WHOLE lines CONTAINING a literal substring.  Used wherever the target line carries
        shell/regex punctuation: building a Python regex for it is exactly the escaping that collapses
        silently, and a substring cannot."""
        n = 0
        for i in range(len(self.lines)):
            if sub in self.lines[i]:
                if self.lines[i] == newtext:
                    raise SystemExit('   NO-OP SUBLINE-CONTAINS [%s / %s] at line %d' % (self.src, label, i + 1))
                self.lines[i] = newtext
                n += 1
        if n != want:
            raise SystemExit('   SUBLINE-CONTAINS [%s / %s] matched %d line(s), wanted %d'
                             % (self.src, label, n, want))
        self.ops += 1
        self.report.append('LINEC   %-26s %d line(s) replaced' % (label, n))

    def gsub(self, old, new, label, minimum=1, maximum=None):
        n = 0
        for i in range(len(self.lines)):
            if old in self.lines[i]:
                n += self.lines[i].count(old)
                self.lines[i] = self.lines[i].replace(old, new)
        if n < minimum:
            raise SystemExit('   GSUB [%s / %s] replaced %d occurrence(s), wanted >= %d -- a '
                             'substitution that matches nothing is a SILENT NO-OP and a derivation '
                             'chain compounds it' % (self.src, label, n, minimum))
        if maximum is not None and n > maximum:
            raise SystemExit('   GSUB [%s / %s] replaced %d occurrence(s), wanted <= %d -- it reached '
                             'further than the edit was scoped for' % (self.src, label, n, maximum))
        self.ops += 1
        self.report.append('GSUB    %-26s %d occurrence(s)  [%s]' % (label, n, old[:44]))

    def require(self, tok, label, minimum=1):
        """⚠ AN ASSERTION THAT SOMETHING WAS **KEPT**.  A derive reports what it changed; what it
        deliberately did NOT change is invisible unless something says so, and a corpus pin that
        quietly vanished reads exactly like a corpus pin that was never there."""
        n = sum(l.count(tok) for l in self.lines)
        if n < minimum:
            raise SystemExit('   REQUIRE [%s / %s]: [%s] appears %d time(s), wanted >= %d'
                             % (self.src, label, tok, n, minimum))
        self.ops += 1
        self.report.append('KEPT    %-26s %d occurrence(s) of [%s]' % (label, n, tok[:40]))

    def absent(self, tok, label):
        code = [(i + 1, l.strip()[:140]) for i, l in enumerate(self.lines) if tok in l]
        if code:
            for n, s in code[:10]:
                print('     survivor line %d: %s' % (n, s))
            raise SystemExit('   ABSENCE [%s / %s]: %d occurrence(s) of [%s] survive'
                             % (self.src, label, len(code), tok))
        self.ops += 1
        self.report.append('ABSENT  %-26s 0 occurrence(s) of [%s]' % (label, tok[:40]))

    def absent_in_code(self, tok, label, allow=()):
        """The token must not survive on a NON-COMMENT line, except on lines carrying one of `allow`.
        ⚠ A file that states a rule necessarily SPELLS the token it forbids, so comment hits are
        COUNTED AND PRINTED rather than refused: 'zero in code' and 'none anywhere' are different
        claims and only one of them is true.  ⚠ AND THE `allow` LIST IS THE CONTROL WIRING: the
        self-check's negative controls read the train-47 ORIGINALS by name, and a blanket absence
        check would rewrite the control to point at the file it controls AGAINST."""
        code, cmt, allowed = [], 0, 0
        for i, l in enumerate(self.lines):
            if tok not in l:
                continue
            if l.lstrip().startswith('#'):
                cmt += 1
            elif any(a in l for a in allow):
                allowed += 1
            else:
                code.append((i + 1, l.strip()[:130]))
        if code:
            for n, s in code:
                print('     survivor line %d: %s' % (n, s))
            raise SystemExit('   ABSENCE-IN-CODE [%s / %s]: %d live occurrence(s) of [%s]'
                             % (self.src, label, len(code), tok))
        self.ops += 1
        self.report.append('ABSENT  %-26s 0 in CODE (%d comment, %d control-wiring)  [%s]'
                           % (label, cmt, allowed, tok[:30]))

    def count_re(self, pat):
        rx = re.compile(pat)
        return sum(1 for l in self.lines if rx.search(l))

    # ---- write ------------------------------------------------------------------------------
    def write(self, dst):
        if dst not in ALLOWED_DST:
            raise SystemExit('   WRITE REFUSED: [%s] is not a train-48 destination.  A derive that can '
                             'write the RUNNING train\'s scripts is the door that opens by itself.' % dst)
        out = NL.join(self.lines) + NL
        cr_out = out.count(chr(13))
        if cr_out != self.cr_in:
            raise SystemExit('   LINE-ENDING DRIFT [%s -> %s]: CR bytes %d -> %d.  The train-47 files '
                             'are LF-only and bash reads a script BY BYTE OFFSET.' % (self.src, dst, self.cr_in, cr_out))
        path = os.path.join(HERE, dst)
        with io.open(path, 'w', encoding='utf-8', newline='') as f:
            f.write(out)
        # ⚠ **THE DERIVE `bash -n`s WHAT IT WROTE, AND REFUSES.**  A replacement block whose text ends
        #   with a shell `"` immediately before the Python terminator loses that quote to the
        #   terminator; the shell line then runs on into the next statement and the file does not
        #   parse.  Neither language shows it on its own, so the check lives HERE rather than in
        #   whatever the operator remembers to run afterwards.
        rc = os.system('bash -n "%s"' % path.replace(chr(92), '/'))
        if rc != 0:
            raise SystemExit('   WROTE AN UNPARSEABLE SCRIPT: %s -- see the bash -n output above.' % dst)
        print('   wrote %-38s %5d lines  CR=%d' % (dst, out.count(NL), cr_out))
        return out.count(NL)


T47ASM_SHA = sha256_of(os.path.join(SRC, 'coord-train47-assemble.sh'))
print('template: coord-train47-assemble.sh sha256=%s' % T47ASM_SHA)


def rename_tokens(d, also_t47=True):
    """STEP 1's rename, as asserted operations.  ⚠ ORDER IS LOAD-BEARING: `TRAIN47` before `train47`
    (the second is not a substring of the first, but spelling them in one place keeps the order
    reviewable), and the train46 -> train47 CONTROL retarget must come AFTER the train47 -> train48
    pass or every name collapses onto train48 and the self-check's negative control would be pointed
    at the file it is supposed to be controlling AGAINST.

    ⚠ EACH SPELLING IS OPTIONAL **PER FILE** AND THE SET AS A WHOLE IS NOT.  The five files do not
    all carry all five spellings (the dry-read carries no bare `TRAIN47` at all), so a mandatory
    per-spelling gsub would refuse a correct file; a mandatory TOTAL is what actually catches the
    thing worth catching -- a rename pass that fired on nothing and left a file named for the
    previous train.  The per-file counts are REPORTED either way, so 'absent' and 'renamed' stay
    different readings rather than collapsing into one."""
    fired = 0
    for old, new, lab in (('TRAIN47', 'TRAIN48', 'rename-TRAIN47'),
                          ('train47', 'train48', 'rename-train47'),
                          ('TRAIN 47', 'TRAIN 48', 'rename-TRAIN-47-sp'),
                          ('Train 47', 'Train 48', 'rename-Train-47-sp'),
                          ('train 47', 'train 48', 'rename-train-47-sp'),
                          ('train-47', 'train-48', 'rename-train-47-hy')):
        if any(old in l for l in d.lines):
            d.gsub(old, new, lab)
            fired += 1
        else:
            d.report.append('RENAME  %-26s absent from this file (a READING, not a no-op)' % lab)
    if also_t47 and any('t47' in l for l in d.lines):
        d.gsub('t47', 't48', 'rename-t47')
        fired += 1
    if not fired:
        raise SystemExit('   RENAME [%s]: NOT ONE train-47 spelling was found, so the rename pass '
                         'fired on nothing.  A file that needs no rename is a file this derive is '
                         'not reading.' % d.src)


# ============================================================================================
# THE ASSEMBLY
# ============================================================================================
A = Doc('coord-train47-assemble.sh')
rename_tokens(A)

# ---- the header: provenance, the FILL POINTS, the new lessons, the new classes -----------------
HDR_TITLE = r"""# TRAIN 48 -- A **TEMPLATE**.  EIGHTEEN SEAT ROWS, ALL EIGHTEEN PINNED AT ORIGIN, A BASE THAT RESOLVES
# AT LAUNCH AND A CONTAINMENT PIN FILLED WITH TRAIN 47'S LANDING.  DERIVED 2026-09-13 BY t48-derive.py FROM
# coord-train47-assemble.sh (sha256 %s)
# AFTER TRAIN 47 LANDED (31fe4925d, run 8).  ⚠ IF THAT FILE CHANGES BEFORE TRAIN 48 ASSEMBLES, RE-RUN THE
# DERIVE FROM FRESH COPIES RATHER THAN PATCHING THIS ONE: a template hand-patched away from its
# parent is a template nobody can re-derive.
# ⚠ THE TRAIN-47 HEADER'S OWN PROVENANCE LINE READ "DERIVED FROM coord-train47-assemble.sh", which is
#   this file's ancestor naming ITSELF: that derive's global coord-train46->coord-train47 rename hit
#   its own provenance sentence.  It is fixed here rather than inherited, and named so the next derive
#   knows to look: a provenance line inside the scope of the rename that produced it cannot be trusted.""" % T47ASM_SHA
A.block(r'^# TRAIN 48 -- A \*\*TEMPLATE\*\*\.', r'^#$', HDR_TITLE, 'header-title', end_off=-1)

HDR_FILL = r"""# ⚠⚠ **THERE ARE FOUR FILL POINTS AND EVERY ONE OF THEM REFUSES UNTIL IT IS FILLED.**  A template
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
#     a narrower exemption than the coordinator wrote."""
A.block(r'^# ⚠⚠ \*\*THERE ARE THREE FILL POINTS', r'^# ⚠⚠ WHAT THIS DERIVE MECHANISED', HDR_FILL,
        'header-fillpoints', end_off=-2)

HDR_NEW = r"""#  (L13) **NO SEAT-NUMBER LITERAL MAY CARRY A CLAIM.**  Train 47's file made thirty-eight assertions of
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
#        a NEW one."""
A.insert_after(r'^#  \(L12\) THE LAND SCRIPT CARRIES \*\*ZERO\*\* ACCEPTANCE PATHS', HDR_NEW, 'header-L13-L15')

HDR_CLASSES = r"""#     converter-test        DERIVED 2026-09-13 for train 48: converter source and its `_test.go`
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
#                           change that no leg parses is a change no gate on this train would see."""
A.insert_after(r'^#     golib-converter-docs  golib \+ converter \+ GolibTests \+ a record', HDR_CLASSES, 'header-classes')

# ---- FILL POINT F1: the worktree goes back to PENDING -------------------------------------------
WTBLOCK = r"""# --- worktree, named in the first lines and no other accepted.  ⚠ **FILL POINT F1.** ----------
#     It reads PENDING and the run ABORTS until the coordinator names the worktree THIS train
#     assembles in.  It is deliberately NOT inherited from the previous train: a train-48 script
#     carrying train 47's worktree would run inside a tree another battery may be holding, and the
#     freeze binds the WORKTREE rather than the branch.  TRAIN48_WT overrides it for a control run,
#     and such a run says so in the stamp -- its verdicts are about this script, never about a train.
WT="${TRAIN48_WT:-PENDING}"
"""
A.block(r'^# --- worktree, named in the first lines', r'^WT="\$\{TRAIN48_WT:-', WTBLOCK, 'worktree-fillpoint')

# ---- FILL POINT F2/F4: the pins ------------------------------------------------------------------
ANC = r"""# The base, the ORDER pin, the CONTAINMENT pin and G3's expectation.
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
T48_CONTAIN_PIN='PENDING'       # ⚠ FILL (F2): the SHA train 47 landed
EXPECT_G3='PENDING'             # ⚠ FILL (F4): G3's expected roster census for THIS train"""
A.block(r"^# The landings this train's base is REQUIRED to contain", r"^EXPECT_CONTAINS='", ANC, 'pins')

# ---- (F10) THE TWO PINS, **FILLED BY THE DERIVE** ------------------------------------------------
#      Train 47 LANDED on 2026-09-13 as master 31fe4925d055537dbb48c343f726e027631f6aa1 (tree
#      161af6c441ae1d8fa44f10b44a9740ba2c20ecea, base a02ac3df3, 15 seats, run 8), so F2's "the SHA
#      train 47 lands" now EXISTS and F4's census figure has been MEASURED on the tree that landed.
#      ⚠ THEY ARE FILLED **HERE**, AS ASSERTED OPERATIONS, AND NEVER TYPED INTO THE ASSEMBLY.  A pin
#        hand-edited into a derived file is undone without trace by the next re-derive -- which is the
#        one thing this whole method exists to prevent.  Each fill is `minimum=1, maximum=1` (a fill
#        that matched nothing and a fill that reached further both refuse) and each is followed by the
#        `absent()` that makes a RE-RUN of the derive unable to re-introduce the placeholder.
#      ⚠ F4 IS A MEASUREMENT WITH A STATED DERIVATION, NOT A CARRIED LITERAL.  Train 47's run-8 record
#        reads `G3 census INVARIANT holds :: all four sets agree at 204` at its ASSEMBLED tree -- the
#        tree that landed.  The only two commits master has taken since (1885bce69 doctrine batch d,
#        271300cea docs) touch twelve files, none of them a README badge, a docs/validation proof page,
#        a docs/ValidatedTestPackages.md roster row or a *.tests.csproj -- which are the four sets
#        push-nuget's census counts.  So the figure that G3 will read at this train's base is 204, and
#        the arm STAMPS rather than refuses when a roster bank lands between this fill and the run.
A.subline_contains("T48_CONTAIN_PIN='PENDING'",
                   "T48_CONTAIN_PIN='31fe4925d055537dbb48c343f726e027631f6aa1'   # (F2) FILLED 2026-09-13: the SHA train 47 LANDED (run 8, 15 seats, base a02ac3df3, tree 161af6c44)",
                   'f10-fill-f2-containment-pin')
A.absent("T48_CONTAIN_PIN='PENDING'", 'f10-no-pending-containment-pin')
A.subline_contains("EXPECT_G3='PENDING'",
                   "EXPECT_G3='204'                 # (F4) FILLED 2026-09-13: MEASURED by train 47 run 8's own G3 at the tree that landed; nothing the census counts has moved on master since",
                   'f10-fill-f4-expect-g3')
A.absent("EXPECT_G3='PENDING'", 'f10-no-pending-g3')
A.gsub('EXPECT_46', 'EXPECT_BASE', 'rename-EXPECT_46')
A.gsub('EXPECT_45', 'EXPECT_ORDER', 'rename-EXPECT_45')
A.gsub('EXPECT_CONTAINS', 'T48_CONTAIN_PIN', 'rename-EXPECT_CONTAINS')

# ---- the FILL-POINT PREFLIGHT: F2 and F4 refuse AT LAUNCH, before the lock and before any merge ---
FILLPRE = r"""
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
"""
A.insert_after(r'^fail_gate\(\)\{ FAILED=1;', FILLPRE, 'fill-point-preflight')

# ---- the base ancestry descriptions ---------------------------------------------------------------
A.subline_contains('"$EXPECT_ORDER:the ORDER pin (train 45\'s landing',
                   '''for anc in "$EXPECT_ORDER:the ORDER pin (train 47's base, a SHA that was already an ancestor of origin/master when this template was derived -- measured with git ls-remote on 2026-09-13 -- so it cannot false-red)" \\''',
                   'anc-order')
A.subline_contains('"$T48_CONTAIN_PIN:the CONTAINMENT pin THIS train requires',
                   '''           "$T48_CONTAIN_PIN:the CONTAINMENT pin THIS train requires -- the SHA TRAIN 47 LANDED.  This train's seats were cut against a tree that carries it, and every class shape, every OWED reading and LEG D's whole prediction is derived FOR a base.  ⚠ This is the assertion that replaces a baked base SHA: it is a statement about the base's CONTENT and it survives every legitimate move of origin/master"; do''',
                   'anc-contain')

# ---- L15: the two toolchain pins read an ANCHORED ROW, not a substring ----------------------------
A.subline_contains('    *"go$want "*) : ;;',
                   '''    # ⚠ AN **ANCHORED ROW MATCH**, not a substring of the line (L15).  `go version` prints one row
    #   beginning `go version go<release> <goos>/<goarch>`; a `*"go$want "*` test also accepts that
    #   token anywhere else on the line, which is the shape that cannot tell a verdict row from a
    #   sentence mentioning the release.
    go\\ version\\ go"$want"\\ *) : ;;''',
                   'pin-anchored')
A.subline_contains('    *"go1.23.12 "*) : ;;',
                   '''    # ⚠ ANCHORED ROW MATCH, for the reason pin_go states (L15).
    go\\ version\\ go1.23.12\\ *) : ;;''',
                   'pairing-anchored')

# ============================================================================================
# STEP 5 -- EVERY SEAT-NUMBER LITERAL BECOMES A CLAIM DERIVED FROM THE CLASS OR THE OWED VECTOR.
# ⚠ Each of these is a surgical phrase substitution rather than a line rewrite, so the sentence that
#   RECORDS why an arm is shaped as it is survives while the premise it carried does not.
# ============================================================================================
for _sub in [
    ("which is what seat 6's FIFTH ARM rules for CNR and the behavioral suite.",
     "which is what the CNR and behavioral-suite legs are RUN under.", 'l5-pairing'),
    ("one run covers seat 12's roster cell AND the H10 docs seat",
     "one run covers every roster cell this train's docs-class rows touch", 'l5-g1'),
    ("measured at derive time, seat 10's BOARD grows",
     "measured on train 47 at ITS derive time, a docs row's BOARD grew", 'l5-g10a-1'),
    ("the head, and seat 11's RECON",
     "the head, and another docs row's RECON", 'l5-g10a-2'),
    ("-- seat 10's whole deliverable is that file",
     "-- a docs row whose whole deliverable is that file landed nothing", 'l5-g10a-3'),
    ("(it was train 45 seat 1's own acceptance",
     "(it was an earlier train's own acceptance", 'l5-leg4b'),
    ("plus seat 4's `fleetIdentifierCensus_test.go` (the split-token arm)",
     "plus the `fleetIdentifierCensus_test.go` split-token arm a converter-test row carries", 'l5-census'),
    ("except seat 4's one,", "except a converter-test row's own,", 'l5-functest'),
    ("added because seat 3's registration DISPLACES a bodied function",
     "added because a manual-conversion registration DISPLACES a bodied function", 'l5-g11b-1'),
    ("(seat 2's go2cs-gen inherited-type template)", "(a merged row whose CLASS owes src/gen)", 'l5-g11b-2'),
    ("(An earlier derive's 'seat 2 is a src/gen seat' premise was train 46's; per (L7) every justification derives from the OWED vector, never from a seat number.)",
     "(Per (L7) every justification derives from the OWED vector rather than from a row's position; an earlier derive carried a per-row premise instead and refused a healthy tree for it.)",
     'l5-g11b-3'),
    ("Train 45 seat 1's alias fold", "The landed alias fold", 'l5-aliasfold', 2),
    ("seat 3's displacement in src/core/runtime/panic.cs",
     "the manual-conversion registry row's displacement in src/core/runtime/panic.cs", 'l5-legr-1'),
    ("the pipeline pass moved seat 3's corpus footprint",
     "the pipeline pass moved the registry row's corpus footprint", 'l5-legr-2'),
    ("which since train 45's seat 4 is the EXPECTED reading",
     "which since the runner's rebuild predicate was corrected is the EXPECTED reading", 'l5-leg5-1'),
    ("is where seat 2's golib/generator change or seat 3's golib primitive would show",
     "is where a golib, generator or golib-primitive row would show", 'l5-leg5-2'),
    ("then seat 3's own gate, then the cost canary",
     "then the `sync` gate a golib-corpus row owes, then the cost canary", 'l5-legk-1'),
    ("-- seat 3's gate and the cost canary are unconditional",
     "-- the `sync` gate and the cost canary are unconditional", 'l5-legk-2'),
    ("(seat 2's go2cs-gen template change is reflect-bridge-touching)",
     "(a go2cs-gen template change is reflect-bridge-touching by the canary rule's own wording)", 'l5-legk-3'),
    ("reached here by seat 3's REGISTRY change", "reached here by a manual-conversion REGISTRY change", 'l5-done-1'),
    ("canary set plus seat 3's ", "canary set plus the ", 'l5-done-2'),
    ("LEG 4b (train 45 seat 1's OWN acceptance", "LEG 4b (an earlier train's OWN acceptance", 'l5-done-3'),
]:
    old, new, lab = _sub[0], _sub[1], _sub[2]
    A.gsub(old, new, lab, minimum=(_sub[3] if len(_sub) > 3 else 1))
A.subline_contains("#   mechanism is measured here.  Seat 2's is LEG 2b's and LEG 5's (route #7); seat 3's is LEG K's",
                   "#   mechanism is measured here.  A src/gen row's is LEG 2b's and LEG 5's (route #7); a golib-corpus",
                   'l5-mech-1')
A.subline_contains("#   `sync` row and LEG 3's arms; seat 1's is LEG 1's registration arithmetic and LEG 5.  A green here",
                   "#   row's is LEG K's `sync` row and LEG 3's arms; a converter row's is LEG 1's registration",
                   'l5-mech-2')

# ============================================================================================
# STEP 3 -- THE SEAT TABLE
# ============================================================================================
TABLE = r'''# ⚠ **FILLED FROM THE TRAIN-48 BOARD, 2026-09-13.**  EIGHTEEN ROWS, ALL EIGHTEEN PINNED.  Every SHA
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
#           claude/c1-token-door-census-recut (converter-test+tooling) carrying `stack-on=11`.
#           NOTHING ELSE ABOUT EITHER ROW CHANGED -- each keeps its own class and its own allowed=.
#      (G1) ROW 13 IS RE-PINNED TO C1'S RE-CUT (COORD b6c472397, 2026-09-13 ~22:15).  Run 2's LEG C
#           refused the previous cut: its census instrument carried a HARDCODED ABSOLUTE default path,
#           which refused on one box and censused the WRONG CHECKOUT on another.  That was a SEAT
#           defect and it was fixed IN THE SEAT -- one commit on top of the previous pin, one file
#           (`src/token-door-census.sh`, +10/-1), deriving CORE from the script's own location.  The
#           row now reads `claude/c1-token-door-census-recut @93bf34030`, same class, same
#           `stack-on=11`, same absence of an `allowed=` ruling.  ⚠ THE SUPERSEDED REF AND SHA ARE
#           NAMED IN THE TRAIN-48 NOTES (§25) AND NOWHERE IN THIS FILE, deliberately: an assembler
#           that spells two pins for one row is an assembler a reader can quote the wrong half of.
#           A-row13's arm is REWRITTEN for the re-cut's readings (§25) and still refuses at the base.
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
#     13   claude/c1-token-door-census-recut          converter-test+tooling     11 files @93bf34030, 7 commits over
#                                                                               origin/master 271300cea (RE-PINNED, G1 above:
#                                                                               the seventh commit is the one-file fix for the
#                                                                               hardcoded default path LEG C refused in run 2).
#                                                                               STACKED ON ROW 11:
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
#          claude/c1-token-door-census-recut is row 13 with `stack-on=11`.  Nothing else about
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
13|claude/c1-token-door-census-recut|93bf34030|converter-test+tooling|tip|stack-on=11
14|claude/c1-mfinal-mint-door-clean|3f1612524|golib-corpus-handown|tip
15|claude/c2-board-both-ordered|da5e83047|docs|tip
16|claude/c2-merge-probe-predicate|4b7985c07|tooling|tip|allowed=^(\.claude/skills/merge-hazards/SKILL\.md)?(\.claude/skills/merge-hazards/merge-probe\.sh)?$
17|claude/c2-h10-shardmap-projection|a633896bf|docs|tip
18|claude/c2-h10-map-rederivation|41c1d1d28|tooling|tip|allowed=^(\.gitattributes)?(docs/phase4/hopA-inputs/shardmap\.py)?$"
'''
A.block(r'^# ⚠ RE-FILLED 2026-09-13 \(SIXTEEN ROWS\)', r'^15\|claude/.*\|tip"$', TABLE, 'seat-table')

# the FIELDS documentation gains the key=value rule
A.subline_contains('#   FIELDS:  N | ref | SHA | class | tipmode [| allowed=<ERE>]',
                   '''#   FIELDS:  N | ref | SHA | class | tipmode [| <key>=<value> ...]
#     ⚠ FIELDS SIX AND BEYOND ARE ORDER-FREE `key=value` PAIRS.  The known keys are `allowed=<ERE>`
#       and `stack-on=<row number>`; an UNKNOWN key is REFUSED by name rather than ignored, which is
#       also what catches an `allowed=` ERE carrying a `|` alternation (it splits into fields here,
#       and silently keeping field six alone would run a narrower exemption than was ruled).''',
                   'fields-doc')
A.subline_contains("#     allowed=  OPTIONAL.  An ERE naming committed behavioral files this seat may MODIFY, under a",
                   "#     allowed=  OPTIONAL (field 6+).  An ERE naming committed behavioral files this seat may MODIFY, under a",
                   'fields-allowed')

SEATVARS = r"""SEAT1_SHA=$(seat_field 1 3); SEAT2_SHA=$(seat_field 2 3); SEAT3_SHA=$(seat_field 3 3);
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
}"""
A.block(r'^SEAT1_SHA=\$\(seat_field 1 3\);', r'SEAT15_REF=\$\(seat_field 15 2\)$', SEATVARS, 'seat-vars')

# the structural check consumes the two new validators
STRUCT = r"""if [ "$(seat_class_placeholders | grep -ac . || true)" != "0" ]; then
  stamp "  ^ SEAT TABLE REFUSED: row(s) [$(seat_class_placeholders | tr '\n' ' ')] carry the class placeholder \`PENDING-class\` with a FILLED SHA.  The class is what decides the shape the seat is held to, so it is filled together with the ref and the SHA -- never after."
  TBL_BAD=1
fi
seat_opts_validate || TBL_BAD=1
[ "$TBL_BAD" = "0" ] || { stamp "SEAT TABLE ROWS ARE STRUCTURALLY UNSOUND -- NOTHING has been merged -- ABORT"; exit 2; }
stamp "SEAT TABLE ROWS ARE STRUCTURALLY SOUND :: numbers contiguous, refs unique, filled SHAs unique, every row five fields or more, every optional field a KNOWN key=value, every stack-on naming an EARLIER row that exists, and no filled row carrying a class placeholder"
STACKROWS=$(printf '%s\n' "$SEAT_TABLE" | awk -F'|' '{for(i=6;i<=NF;i++) if ($i ~ /^stack-on=/) {printf "%s(on %s) ", $1, substr($i,10); break}}')
stamp "SEAT TABLE DECLARED STACKS :: [${STACKROWS:-none}] -- a declared stack is a NARROWING of the patch-id census below and never a weakening: it can only exempt the shape git already collapses (one commit on two seats merges ONCE), and a cherry-pick DUPLICATE stays red whatever is declared."
"""
A.block(r'^\[ "\$TBL_BAD" = "0" \] \|\| \{ stamp "SEAT TABLE ROWS ARE STRUCTURALLY UNSOUND',
        r'^stamp "SEAT TABLE ROWS ARE STRUCTURALLY SOUND ::', STRUCT, 'seat-struct')

# the allowed-set readout scans fields 6..NF
A.subline_contains("ALLOWED_ROWS=$(printf '%s\\n' \"$SEAT_TABLE\" | awk -F'|' '$6 ~ /^allowed=/ {printf \"%s \", $1}')",
                   'ALLOWED_ROWS=$(seat_allowed_rows | tr \'\\n\' \' \')', 'a7-allowed-rows')
A.subline_contains("ALLOWED_N=$(printf '%s\\n' \"$SEAT_TABLE\" | awk -F'|' '$6 ~ /^allowed=/' | grep -ac . || true)",
                   'ALLOWED_N=$(seat_allowed_rows | grep -ac . || true)', 'a7-allowed-n')
A.subline_contains("""  printf '%s\\n' "$SEAT_TABLE" | awk -F'|' '$6 ~ /^allowed=/ {printf "      row %s (%s) allows: %s\\n", $1, $2, substr($6, 9)}' | tee -a "$ASMLOG\"""",
                   """  printf '%s\\n' "$SEAT_TABLE" | awk -F'|' '{for(i=6;i<=NF;i++) if ($i ~ /^allowed=/) {printf "      row %s (%s) allows: %s\\n", $1, $2, substr($i, 9); break}}' | tee -a "$ASMLOG\"""",
                   'a7-allowed-print')
# ⚠ THE PREFLIGHT'S allowed= TEST READS THROUGH seat_opt TOO, AND ITS `case` HEAD IS REWRITTEN AS A
#   SEPARATE OP.  Replacing only the ARM line would leave the old `case "${pa:-}" in` head above a
#   second `case` head -- a file that does not parse, and the write guard's `bash -n` caught exactly
#   that here.  Two anchors, two ops, each asserting.
A.subline_contains('  case "${pa:-}" in',
                   '  # ⚠ READ THROUGH seat_opt, NEVER POSITIONALLY: `pa` came from a positional read and holds\n'
                   '  #   fields SIX AND BEYOND with their delimiters once a second optional key exists, so an\n'
                   '  #   `allowed=*` glob on it misses a ruling written in field seven -- and a missed ruling reads\n'
                   '  #   as the STRICT default, which is silence.\n'
                   '  case "$(seat_opt "$(seat_row "$pn")" allowed)" in',
                   'preflight-allowed-case')
A.subline_contains("""    allowed=*) [ "$ps" != "PENDING" ] || { stamp "  ^ SEAT PREFLIGHT REFUSED: row $pn declares an \\`allowed=\\` set while its SHA reads PENDING""",
                   """    ?*) [ "$ps" != "PENDING" ] || { stamp "  ^ SEAT PREFLIGHT REFUSED: row $pn declares an \\`allowed=\\` set while its SHA reads PENDING -- a ruling about a seat that does not exist yet."; PLACEBAD=1; } ;;""",
                   'preflight-allowed')
A.subline_contains("""printf '%s\\n' "$SEAT_TABLE" | awk -F'|' '{printf "SEAT %s NOTE :: %s @%s :: class %s, tip mode %s%s\\n", $1, $2, $3, $4, $5, ($6 ~ /^allowed=/ ? " :: carries an A7 ALLOWED ruling" : "")}' | while IFS= read -r ln; do stamp "$ln"; done""",
                   """printf '%s\\n' "$SEAT_TABLE" | awk -F'|' '{o=""; for(i=6;i<=NF;i++){ if ($i ~ /^allowed=/) o=o" :: carries an A7 ALLOWED ruling"; if ($i ~ /^stack-on=/) o=o" :: DECLARED STACK on row "substr($i,10) } printf "SEAT %s NOTE :: %s @%s :: class %s, tip mode %s%s\\n", $1, $2, $3, $4, $5, o}' | while IFS= read -r ln; do stamp "$ln"; done""",
                   'seat-note-awk')

# the A7 map loop reads its pattern through seat_opt rather than positionally
A.subline_contains("""  case "${aa:-}" in allowed=*) : ;; *) continue ;; esac""",
                   """  # ⚠ READ THROUGH seat_opt, NEVER POSITIONALLY.  With two optional keys, `read -r ... aa` puts every
  #   field from six on into `aa` delimiters and all, and a positional `allowed=*` test then misses a
  #   ruling written in field seven -- which reads as "no ruling declared", i.e. as the STRICT default.
  apat="$(seat_opt "$(seat_row "$an")" allowed)"
  [ -n "$apat" ] || continue""", 'a7-map-read')
#  ⚠ THE POSITIONAL READ IS **REPLACED BY A COMMENT, NOT BY A `:`**.  A bare no-op left where a read
#    used to be reads as a check somebody deleted and nobody re-argued; the comment says where the
#    read went, which is the one thing the next reader needs.
A.subline_contains('  apat="${aa#allowed=}"',
                   '  # (the allowed= value is read through seat_opt above; nothing positional is left here)',
                   'a7-map-apat')

# ============================================================================================
# STEP 3b -- **THE `allowed=` RULING IS HONOURED BY merge_seat, NOT ONLY BY A7** (COORD 2026-09-13
#            19:05, D5, extending the 18:20 ruling on G 7a947650f).
#
# ⚠ THE DEFECT, MEASURED RATHER THAN SUSPECTED.  Train 47's merge_seat consults the CLASS and nothing
#   else: its forbidden arm is `git diff --name-only $mb $want -- $forbidden` and its shape arm is
#   `... | grep -avE "$shapepat"`.  ONLY A7 read `allowed=`, and A7 scopes itself to
#   src/tests/Behavioral with --diff-filter=MD.  So every row of this table whose ruling names a path
#   outside that scope -- rows 1, 11, 16 and 18 -- would have ABORTED AT ASSEMBLY on its own
#   class, with a message about a shape, while the ruling written at the row read as decoration.
#   (Row 13 carried a FIFTH such ruling until COORD's E1 ruling at ~20:00 read it as STALE and dropped
#   it; the count here is FOUR and is the count of rows that carry a ruling, not of rows that ever did.)
#
# ⚠ AND THE REMEDY IS NOT A WIDER CLASS.  Not one shapepat and not one forbidden list is touched by
#   any op below; what changes is that the two readings SUBTRACT the paths a coordinator ruling names,
#   COUNT and LIST what they subtracted, REFUSE a ruled path the seat does not touch, and assert BLOB
#   IDENTITY for every ruled path once the merge has happened.
# ============================================================================================
ALWHELP = r"""seat_allowed_paths(){ # $1 = an allowed= ERE -> the LITERAL paths it names, one per line
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
}"""
A.insert_after(r'^seat_row\(\)\{', ALWHELP, 'seat-allowed-helpers')

# merge_seat's locals gain the ruling's four variables.  ⚠ THEY ARE **LOCAL**: a ruling that leaked
# from one seat into the next would exempt a path nobody ruled on for that seat, and every stamp below
# would still read clean.
A.subline_contains('  local b="$1" want="$2" n="$3" cls="$4" mode="$5" got gotshort tip m c f seen listed forbidden bad ahead mrc mb shapebad shapepat',
                   '  local b="$1" want="$2" n="$3" cls="$4" mode="$5" got gotshort tip m c f seen listed forbidden bad ahead mrc mb shapebad shapepat\n'
                   '  local alw alwn alwlist alwp alwbu alwbs dpaths',
                   'merge-seat-allowed-locals')

ALWREAD = r'''  # --- ⚠ **THE PER-ROW `allowed=` RULING, READ HERE AND SUBTRACTED FROM BOTH READINGS BELOW.**
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
  bad=$(git -c core.quotepath=false diff --name-only "$mb" "$want" -- $forbidden | alw_filter "$alw" | grep -ac . || true)'''
A.subline_contains('  bad=$(git -c core.quotepath=false diff --name-only "$mb" "$want" -- $forbidden | grep -ac . || true)',
                   ALWREAD, 'merge-seat-allowed-read')
A.subline_contains('    git -c core.quotepath=false diff --name-only "$mb" "$want" -- $forbidden | sed \'s/^/    /\' | tee -a "$ASMLOG"',
                   '    git -c core.quotepath=false diff --name-only "$mb" "$want" -- $forbidden | alw_filter "$alw" | sed \'s/^/    /\' | tee -a "$ASMLOG"',
                   'merge-seat-forbidden-list')
A.subline_contains('  stamp "seat $n ($cls) $b @$tip :: forbidden-path files=0 (checked: $forbidden)"',
                   '  stamp "seat $n ($cls) $b @$tip :: forbidden-path files=0 (checked: $forbidden) :: allowedByRuling=$alwn [$alwlist]"',
                   'merge-seat-forbidden-stamp')
A.subline_contains('    shapebad=$(git -c core.quotepath=false diff --name-only "$mb" "$want" | grep -avE "$shapepat" | grep -ac . || true)',
                   '    shapebad=$(git -c core.quotepath=false diff --name-only "$mb" "$want" | alw_filter "$alw" | grep -avE "$shapepat" | grep -ac . || true)',
                   'merge-seat-shape-filter')
A.subline_contains('      git -c core.quotepath=false diff --name-only "$mb" "$want" | grep -avE "$shapepat" | sed \'s/^/    /\' | tee -a "$ASMLOG"',
                   '      git -c core.quotepath=false diff --name-only "$mb" "$want" | alw_filter "$alw" | grep -avE "$shapepat" | sed \'s/^/    /\' | tee -a "$ASMLOG"',
                   'merge-seat-shape-list')
A.subline_contains('    stamp "seat $n ($cls) shape OK :: every changed path matches $shapepat"',
                   '    stamp "seat $n ($cls) shape OK :: every changed path matches $shapepat :: allowedByRuling=$alwn [$alwlist] (ruled paths are admitted OUTSIDE the shape and nowhere else)"',
                   'merge-seat-shape-stamp')

ALWIDENT = r'''  # --- ⚠ **ALLOWED-IDENTITY-AFTER-MERGE.**  COORD 2026-09-13 19:05 (D5), clause (ii).  Subtracting a
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
  # marker scan over the committed blobs.  The loop asserts processed == listed.'''
A.subline_contains('  # marker scan over the committed blobs.  The loop asserts processed == listed.',
                   ALWIDENT, 'merge-seat-allowed-identity')

A7B = r'''
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
[ "$A7BBAD" = "0" ] || fail_gate A7b-allowed-identity'''
A.insert_after(r'^\[ "\$\{A7MODX:-1\}" = "0" \] \|\|', A7B, 'a7b-arm')

# the FIELDS documentation states the EXTENDED meaning and the identity predicate
A.subline_contains("#               ⚠ IT IS A RULING, NOT A CONVENIENCE.  Write the ruling in the seat's own note.",
                   """#               ⚠ IT IS A RULING, NOT A CONVENIENCE.  Write the ruling in the seat's own note.
#               ⚠⚠ **EXTENDED 2026-09-13 19:05 (COORD, D5) FROM "A7's EXEMPTION" TO "A PER-ROW RULING
#               OVER THE WHOLE SEAT".**  The ERE admits EXACTLY the named paths OUTSIDE the class's
#               SHAPE and outside its FORBIDDEN list as well -- merge_seat's two readings subtract
#               them, COUNT them and LIST them (`allowedByRuling=N` plus the paths); a ruled path the
#               seat does not touch is a STALE RULING and ABORTS BY NAME; and the identity predicate
#               is asserted TWICE -- once per seat immediately after its merge (union blob == that
#               seat's blob) and once over the finished union in arm A7b, where a path ruled by two
#               rows is owned by the LAST of them in merge order.  ⚠ NOT ONE CLASS SHAPE OR FORBIDDEN
#               LIST WAS WIDENED to make this work; widening is what a ruling exists to avoid.""",
                   'fields-allowed-extended')

# ============================================================================================
# STEP 4 -- THE PATCH-ID ARM
# ============================================================================================
PIDARM = r'''
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
'''
A.insert_after(r'^\[ "\$PLACEBAD" = "0" \] \|\| \{ stamp "SEAT TABLE REFUSED :: at least one filled row',
               PIDARM, 'patch-id-arm')

# ---- the three new classes -----------------------------------------------------------------------
CLASSES = r"""    # ⚠ DERIVED FOR TRAIN 48, and `converter-test+docs` is UNTOUCHED beside it.  A guard-only row:
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
"""
A.insert_after(r'^    docs-data\)            forbidden=', CLASSES, 'classes-new')

# ---- the OWED vector knows the new classes --------------------------------------------------------
OWED = r"""OWED_GOLIB=0; OWED_GEN=0; OWED_CONV=0; OWED_BEH=0; OWED_DOCS=0; OWED_CLAUDEMD=0; OWED_MANIFEST=0; OWED_CORPUS=0; OWED_GT=0; OWED_TOOLING=0
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
stamp "OWED VECTOR :: golib=$OWED_GOLIB gen=$OWED_GEN converter=$OWED_CONV behavioral=$OWED_BEH corpus=$OWED_CORPUS golibtests=$OWED_GT docs=$OWED_DOCS claudemd=$OWED_CLAUDEMD manifest=$OWED_MANIFEST tooling=$OWED_TOOLING (DERIVED from the CLASSES of the $SEATS_DONE merged row(s): [$MERGED_CLASSES]) -- every justification arm below reads THIS vector, and so does the land script, so the two cannot drift\""""
A.block(r'^OWED_GOLIB=0; OWED_GEN=0;', r'^stamp "OWED VECTOR :: golib=', OWED, 'owed-vector')
A.subline_contains('so the two cannot drift\\"', 'stamp "OWED VECTOR :: golib=$OWED_GOLIB gen=$OWED_GEN converter=$OWED_CONV behavioral=$OWED_BEH corpus=$OWED_CORPUS golibtests=$OWED_GT docs=$OWED_DOCS claudemd=$OWED_CLAUDEMD manifest=$OWED_MANIFEST tooling=$OWED_TOOLING (DERIVED from the CLASSES of the $SEATS_DONE merged row(s): [$MERGED_CLASSES]) -- every justification arm below reads THIS vector, and so does the land script, so the two cannot drift"', 'owed-stamp-fix')

# ---- G11(a) gains the tooling arm -----------------------------------------------------------------
A.subline_contains("G11ADIRS='src/core src/core/golib src/core/syscall/windows src/tests src/tests/GolibTests src/go2cs src/gen src/utilities docs CLAUDE.md'",
                   "G11ADIRS='src/core src/core/golib src/core/syscall/windows src/tests src/tests/GolibTests src/go2cs src/gen src/utilities docs CLAUDE.md'\n# ⚠ THE TOOLING POPULATION IS A PATHSPEC SET, NOT A DIRECTORY, because a repo tool script lives at the\n#   src/ ROOT (`src/*.sh`, `src/*.ps1`) as well as under src/utilities -- and `src/utilities` alone\n#   would read ZERO on a row whose whole deliverable is a root-level script.\nG11TOOLSPEC='src/*.sh src/*.ps1 src/utilities'",
                   'g11a-toolspec')
A.subline_contains("G11CORE=$(git -c core.quotepath=false diff --name-only \"$BASE\" HEAD -- src/core | grep -ac . || true)",
                   "G11CORE=$(git -c core.quotepath=false diff --name-only \"$BASE\" HEAD -- src/core | grep -ac . || true)\n# shellcheck disable=SC2086\nG11TOOL=$(git -c core.quotepath=false diff --name-only \"$BASE\" HEAD -- $G11TOOLSPEC | grep -ac . || true)\ncase \"${G11TOOL}\" in ''|*[!0-9]*) G11TOOL=0 ;; esac",
                   'g11a-toolcount')
A.subline_contains("""g11a_arm "$OWED_CORPUS"   "$G11CORE"  'src/core'             'LEG 2 compiles the corpus and LEG D measures its emission'""",
                   """g11a_arm "$OWED_CORPUS"   "$G11CORE"  'src/core'             'LEG 2 compiles the corpus and LEG D measures its emission'
g11a_arm "$OWED_TOOLING"  "$G11TOOL"  'src/*.sh + src/*.ps1 + src/utilities' 'G2 parses every CHANGED .ps1 under BOTH PowerShell editions -- the only gate on this train that reads a harness script at all, which is why a tooling class owes it BY NAME rather than by being swept up in some other leg'""",
                   'g11a-toolarm')

# ---- G3 reads its expectation from the FILL POINT -------------------------------------------------
A.subline_contains('      if [ "$G3A" = "204" ]; then',
                   '      # ⚠ THE EXPECTATION IS **FILL POINT F4**, NOT A LITERAL.  Train 47 spelled `204` here; roster\n      #   rows move between trains, and a figure spelled into a gate either refuses a healthy battery a\n      #   landing later or agrees with it by coincidence -- and the log cannot tell those apart.\n      if [ "$G3A" = "$EXPECT_G3" ]; then',
                   'g3-expect-1')
A.subline_contains('        stamp "G3 EXPECTATION MET :: 204/204/204/204, the coordinator\'s stated figure for this train"',
                   '        stamp "G3 EXPECTATION MET :: $G3A/$G3B/$G3C/$G3D, and EXPECT_G3=$EXPECT_G3 is the coordinator\'s stated figure for THIS train (fill point F4)"',
                   'g3-expect-2')
A.subline_contains('        stamp "G3 EXPECTATION DISAGREES (stamped, NOT refused) :: the four agree at $G3A where the coordinator stated 204.',
                   '        stamp "G3 EXPECTATION DISAGREES (stamped, NOT refused) :: the four agree at $G3A where EXPECT_G3 (fill point F4) reads $EXPECT_G3.  A roster bank landing between the fill and the assembly moves all four together and is legitimate; a figure that moved for any OTHER reason is a finding.  Reconcile before landing."',
                   'g3-expect-3')

# ---- the SEAT-CONTENT ASSERTIONS go back to being a SLOT ------------------------------------------
SEATASSERT = r"""# ⚠ FILL POINT.  ONE ARM PER NAMED SEAT, written at seat fill, each gated on its own
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
"""
A.block(r'^# ⚠ FILLED 2026-09-13\.  An arm per FILLED row\.', r'^# === SEAT-CONTENT ASSERTIONS END ===$',
        SEATASSERT, 'seat-content-slot', end_off=-1)


# ---- the corpus pin is KEPT, and the derive says so ------------------------------------------------
A.require('version.props <GoStdLibVersion>=${A6PIN:-unreadable} (must be 1.23.12)', 'a6-corpus-pin')
A.require("[ \"${A6PIN:-}\" = \"1.23.12\" ]", 'a6-corpus-pin-test')
A.require("grep -acE '^go 1\\.24\\.13$'", 'a6-gomod-pin')

# ============================================================================================
# STEP 6 -- ROW-ANCHORED ASSERTIONS.
# ⚠ THE RULE AND ITS LIMIT, BOTH STATED.  A phrase a report prints as a ROW is also a phrase it can
#   print inside a COUNT line, and a substring match cannot tell them apart -- `0 NOT MEASURED` in a
#   tally reads exactly like a row that says NOT MEASURED.  The three sites below are converted
#   because the PRODUCER's own source was READ and its column-0 form measured; every other in-scope
#   site is LEFT and whitelisted in the self-check's ARM (b), because an anchor that does not match
#   what the producer actually prints is a gate that refuses a HEALTHY run -- which is strictly worse
#   than the substring it replaced.
# ⚠ EACH CONVERSION KEEPS **BOTH READINGS**.  The anchored count decides; the bare count is stamped
#   beside it and a DISAGREEMENT is printed as a finding.  A conversion that threw the old reading
#   away would be a silent change of what the gate measures.
# ============================================================================================
A.subline_contains("""  G1PASS=$(tr -d '\\r' < "$G1LOG" | grep -acE '[0-9]+ checks pass' || true)""",
                   """  # ⚠ AN **ANCHORED ROW MATCH** PLUS ITS UNANCHORED SECOND READING (L15).  Measured from the
  #   producer: src/check-roster-format.ps1 prints `roster format guard: <n> checks pass (...)` as one
  #   line at column 0.  The bare `[0-9]+ checks pass` would also match that phrase inside any future
  #   summary line the guard grows, and the two readings are stamped together so a disagreement is a
  #   FINDING rather than a silent change of what this gate counts.
  G1PASS=$(tr -d '\\r' < "$G1LOG" | grep -acE '^[[:space:]]*roster format guard: [0-9]+ checks pass' || true)
  G1PASS_ANY=$(tr -d '\\r' < "$G1LOG" | grep -acE '[0-9]+ checks pass' || true)
  [ "${G1PASS:-0}" = "${G1PASS_ANY:-0}" ] || stamp "  G1 READING NOTE :: the anchored verdict rows ($G1PASS) and the bare phrase count ($G1PASS_ANY) DISAGREE -- the guard printed that phrase somewhere other than its own verdict row, and the anchored count is the one this gate uses" """,
                   'step6-g1pass')
A.subline_contains("""  G3CLEAN=$(tr -d '\\r' < "$G3LOG" | grep -ac 'Pre-flight clean' || true)""",
                   """  # ⚠ ANCHORED ROW MATCHES PLUS THEIR UNANCHORED SECOND READINGS (L15).  Measured from the producer:
  #   src/push-nuget.ps1 prints the clean verdict through `Write-Step`, which prefixes `==> `, and the
  #   failure verdict through `Write-Host` at column 0.  Leading whitespace is tolerated because a
  #   redirected PowerShell host may indent; what is EXCLUDED is the phrase appearing mid-line, which
  #   is the shape a tally produces.
  G3CLEAN=$(tr -d '\\r' < "$G3LOG" | grep -acE '^[[:space:]]*==> Pre-flight clean' || true)
  G3CLEAN_ANY=$(tr -d '\\r' < "$G3LOG" | grep -ac 'Pre-flight clean' || true)""",
                   'step6-g3clean')
A.subline_contains("""  G3BAD=$(tr -d '\\r' < "$G3LOG" | grep -ac 'PRE-FLIGHT FAILED' || true)""",
                   """  G3BAD=$(tr -d '\\r' < "$G3LOG" | grep -acE '^[[:space:]]*PRE-FLIGHT FAILED' || true)
  G3BAD_ANY=$(tr -d '\\r' < "$G3LOG" | grep -ac 'PRE-FLIGHT FAILED' || true)
  { [ "${G3CLEAN:-0}" = "${G3CLEAN_ANY:-0}" ] && [ "${G3BAD:-0}" = "${G3BAD_ANY:-0}" ]; } || stamp "  G3 READING NOTE :: an anchored verdict count and its bare phrase count DISAGREE (clean $G3CLEAN vs $G3CLEAN_ANY, failed $G3BAD vs $G3BAD_ANY) -- push-nuget printed a verdict phrase somewhere other than its own verdict row; the ANCHORED counts are the ones this gate uses" """,
                   'step6-g3bad')

# ============================================================================================
# THE PRE-DERIVE REVIEW FIXES (2026-09-13).  ⚠ EVERY ONE OF THESE WAS EITHER A **CARRIED TRAIN-47
#   FACT THAT THE BLANKET RENAME LEFT WEARING A TRAIN-48 CLAIM'S CLOTHES**, or a ROW-NUMBER premise
#   that (L13) forbids in prose and this derive then left standing in **CODE**.  They are asserted
#   operations here rather than hand patches on the output, for the reason the whole method exists: a
#   template hand-patched away from its derive cannot be re-derived, and the next run of this script
#   from fresh copies would put every one of them back.
# ============================================================================================

# ---- (F7) THE RENAME CORRUPTED PROVENANCE.  `train 47 run 4` is a run that HAPPENED and has a log on
#      disk; `train 48 run 4` has not, because train 48 has never run.  A spelling rename cannot tell a
#      RECORD from a CLAIM, so each record is restored BY NAME and the invented spelling is asserted
#      ABSENT afterwards -- the absence is the guard that the rename cannot re-introduce it.
A.subline_contains('2026-09-13 (train 48 run 4): the manifest class was MISSING here',
                   '#   ⚠ 2026-09-13 (train 47 run 4): the manifest class was MISSING here, so two manifests edited by two',
                   'f7-legd-manifest-prov')
A.absent('train 48 run 4', 'f7-no-invented-run-record')

LEGU_REASON = ('      "reason": "PLANTED BY **THIS** ASSEMBLY\'S LEG U as a two-direction control over the converter\'s '
               'platform-scoped disclosure entries. It names a test this very run records a terminal PASS for, so an '
               'IN-SCOPE copy of it is an orphan by that rule\'s own predicate and an OUT-OF-SCOPE copy is inert. It is '
               'written immediately before one conversion and deleted immediately after; if this file is ever found '
               'committed, the leg did not reach its restore."')
A.subline_contains("PLANTED BY THE TRAIN-47 ASSEMBLY'S LEG U", LEGU_REASON, 'f7-legu-plant-reason')
A.absent('TRAIN-47 ASSEMBLY', 'f7-no-foreign-plant-author')

# ---- (F3) LEG U'S RUN/SKIP KEY WAS POSITIONAL ------------------------------------------------------
LEGU_KEY = r"""# ⚠ **LEG U'S RUN/SKIP KEY IS THE OWED VECTOR, NOT A ROW NUMBER.**  Train 47 keyed this leg to
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
  LEGU_OK=0"""
A.block(r'^if \[ "\$SEAT1_SHA" = "PENDING" \]; then$', r'^  LEGU_OK=0$', LEGU_KEY, 'f3-legu-key')

# ---- (F2) THE TWO ROW-NUMBERED CLOSING STAMPS ------------------------------------------------------
CLOSING_NEW = r"""stamp "CLOSING NOTES :: **THIS TEMPLATE MAKES NO PER-ROW CLOSING CLAIM, AND THAT IS A DELETION RATHER THAN AN OMISSION.**  Train 47 closed with two row-numbered notes: one asserting that its first row landed platform SCOPING for disclosure manifests and naming LEG U as that row's own seat gate, one asserting that its ninth row was cut from an OLDER base and that a per-row A-row<N> arm therefore read it THREE-DOT.  Both were facts about THAT table, and the rename would have carried both onto whatever rows this train happens to seat; this train's SEAT-CONTENT block is an empty SLOT and no arm here emits a per-row content stamp, so the per-row anchor those words name does not exist either.  A closing note is written AT SEAT FILL, beside the SEAT-CONTENT arm that measures the thing it claims, or it is not written at all.  ⚠ WHAT IS LOST BY THE DELETION IS NAMED: a seat cut from a base older than this train's still needs the THREE-DOT form for every read of it -- its class shape, its forbidden set and LEG D's per-seat prediction alike -- because a two-dot query against a base a seat forked before returns MASTER's own newer content REVERSED, and the seat-fill note must say so again."
"""
A.block(r'^stamp "ROW 1 CLOSING NOTE ::', r'^stamp "ROW 9 CLOSING NOTE ::', CLOSING_NEW, 'f2-row-closing-notes')
A.absent('A-row9', 'f2-no-arow9-anchor')

# ⚠ THE ARMS ARE FILLED **AFTER** THE TWO TRAIN-47 ANCHOR ASSERTIONS ABOVE, AND THE ORDER IS THE
#   POINT: `f2-no-arow9-anchor` asserts that TRAIN 47's own A-row9 anchor did not survive the
#   derive, and it must read a file this train has not yet written an A-row9 into, or it would be
#   asserting nothing.  The fill is a separate, later step for exactly that reason.
# ---- THE EIGHTEEN SEAT-CONTENT ARMS, one per FILLED row -------------------------------------------
# ⚠ WRITTEN AT SEAT FILL, AND THEREFORE HERE RATHER THAN BY HAND.  Every arm reads the named seat's own
#   content out of the ASSEMBLED FILES, is gated on its own $SEATn_SHA, stamps the value beside its
#   `must be`, and ends in the SEATASSERT_N increment the post-battery gate counts.  MEASURED against
#   the run-1 union 3e0e6023f8 over base 271300cea0: every arm carries at least one predicate that is
#   FALSE at the base and TRUE at the union, so none of them is a green that cannot go red.
ARMS = r"""
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

# --- row 13 :: claude/c1-token-door-census-recut -- DECLARED `stack-on=11`.  Instrument, guard and
#     the census RECORD the instrument produced: a record without its instrument is a figure nobody can
#     re-derive, which is the reason this class exists.
#     ⚠ RE-WRITTEN FOR THE RE-CUT (G1, COORD b6c472397).  The previous cut's instrument carried a
#     HARDCODED ABSOLUTE default path: the guard drives the script with NO argument, so that literal WAS
#     the path under test -- it refused on one box and censused the WRONG CHECKOUT on another, and it
#     was a username-style home path on a pushed surface besides.  The arm therefore reads the FIX and
#     not merely the file: the instrument must carry NO absolute path AT ALL, and its default must be
#     DERIVED from `dirname "$0"`.  Both readings are about the seat's own bytes, so a re-cut that
#     reverted the fix would board with its arm green only if this arm did not exist.
#     ⚠ CR IS STRIPPED BEFORE EVERY MATCH.  `*.sh` is pinned `text eol=lf` so the worktree form is LF
#     TODAY; an anchored pattern that silently stops matching the day a pin moves is the exact CRLF
#     trap this train has already met three times, and `tr -d` costs nothing.
if [ "$SEAT13_SHA" != "PENDING" ]; then
  S13OK=1
  for f in src/go2cs/tokenDoorCensusGuard_test.go src/token-door-census.sh docs/phase4/CENSUS-token-door-live-wrappers.md; do
    [ -f "$f" ] || { stamp "  ^ A-row13 REFUSED: $f is not in the assembled tree"; S13OK=0; }
  done
  S13REG=$(grep -acF -- 'tokenDoorCensusGuard_test.go' src/go2cs/go2cs-src.projitems || true)
  S13FN=$(grep -acF -- 'func TestTokenDoorCensusControls' src/go2cs/tokenDoorCensusGuard_test.go 2>/dev/null || true)
  S13EX=$(tr -d '\r' < src/token-door-census.sh 2>/dev/null | grep -acEv '^[[:space:]]*(#|$)' || true)
  S13ABS=$(tr -d '\r' < src/token-door-census.sh 2>/dev/null | grep -acE '(^|[^A-Za-z0-9])(/(c|C)/[Uu]sers/|/[Hh]ome/|/[Uu]sers/|[A-Za-z]:/)' || true)
  S13DIR=$(tr -d '\r' < src/token-door-census.sh 2>/dev/null | grep -acE '^CORE=\$\{1:-.*dirname "\$0".*\}$' || true)
  S13NEW=$(git diff --name-only --diff-filter=A "$BASE" HEAD -- docs/phase4/CENSUS-token-door-live-wrappers.md | grep -ac . || true)
  if [ "${S13ABS:-0}" != "0" ]; then
    stamp "  ^ A-row13 REFUSED: the instrument carries an ABSOLUTE path -- the guard drives it with no argument, so the literal IS the path under test, and it is a pushed-surface home path besides:"
    tr -d '\r' < src/token-door-census.sh | grep -anE '(^|[^A-Za-z0-9])(/(c|C)/[Uu]sers/|/[Hh]ome/|/[Uu]sers/|[A-Za-z]:/)' | while IFS= read -r ln; do stamp "    $ln"; done
  fi
  stamp "A-row13 the token-door census (RE-CUT) :: the three files present=$S13OK :: guard registered in projitems x$S13REG (must be 1) :: its named control test x$S13FN (must be 1) :: EXECUTABLE lines in the instrument=$S13EX (must be >= 1) :: ABSOLUTE paths in the instrument=$S13ABS (must be 0 -- the defect LEG C refused in run 2) :: the default DERIVED from dirname \$0 x$S13DIR (must be 1 -- the fix itself, and the base reads 0 because the file is not there) :: the census record ADDED by this train=$S13NEW (must be 1)"
  { [ "$S13OK" = "1" ] && [ "${S13REG:-0}" = "1" ] && [ "${S13FN:-0}" = "1" ] && [ "${S13EX:-0}" -ge 1 ] && [ "${S13ABS:-0}" = "0" ] && [ "${S13DIR:-0}" = "1" ] && [ "${S13NEW:-0}" = "1" ]; } || fail_gate A-row13
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
"""
A.insert_after(r'^#   wrote, which is a different mistake found at a different time\.$',
               ARMS, 'seat-content-arms-18')
# ⚠ THE ARM COUNT IS ASSERTED HERE TOO, IN THE DERIVE, and against the table the derive itself wrote:
#   a payload that lost an arm to an editing slip would still parse and would still be counted only by
#   the run-time gate, which reads it AFTER a battery.
_ARM_INC = 'SEATASSERT_N=$(( SEATASSERT_N + 1 ))'
_arms_written = sum(1 for _l in ARMS.split(NL) if _l.strip() == _ARM_INC)
if _arms_written != 18:
    raise SystemExit('   SEAT-CONTENT ARMS: payload carries %d increment(s), wanted 18' % _arms_written)
print('   seat-content arms: %d, one per FILLED row' % _arms_written)

# ---- (G1) ROW 13 RE-PINNED TO C1'S RE-CUT, AND THE RE-PIN ASSERTED RATHER THAN ANNOUNCED -----------
#      The re-pin itself is PAYLOAD, edited inside the seat-table op, the three header narrative ops
#      and the arms op (the §20.5 precedent).  What CANNOT be payload is the claim that the OLD
#      spelling is gone everywhere and the NEW one is everywhere it must be: a re-pin that updates the
#      table and leaves a header naming the superseded branch is a file whose two halves disagree, and
#      the half a reader quotes is the wrong one.  These six ops are that claim, made by the derive.
#      ⚠ THEY ARE PLACED **AFTER** THE ARMS OP ON PURPOSE.  The arms payload carries row 13's ref in
#      its own comment, so an absence check ordered before it would read a file the arms had not yet
#      been written into -- the same ordering fault §23.1 records for `f2-no-arow9-anchor`, and the
#      same fix.
A.absent('c1-token-door-census-stacked', 'g1-no-superseded-row13-ref')
A.absent('b4914e878', 'g1-no-superseded-row13-sha')
A.require('13|claude/c1-token-door-census-recut|93bf34030|converter-test+tooling|tip|stack-on=11',
          'g1-row13-recut-seated')
A.require('claude/c1-token-door-census-recut', 'g1-row13-recut-named', minimum=4)
#      And the two NEW readings inside A-row13's arm, asserted by their exact spelling: an arm that
#      lost its fix predicate to an editing slip would still parse, still increment, and still be
#      counted by the block-scoped arm count -- which asks HOW MANY arms, never WHICH READINGS.
A.require("S13ABS=$(tr -d '\\r' < src/token-door-census.sh", 'g1-row13-no-absolute-path-reading')
A.require('S13DIR=$(tr -d \'\\r\' < src/token-door-census.sh 2>/dev/null | grep -acE \'^CORE=\\$\\{1:-.*dirname "\\$0".*\\}$\'',
          'g1-row13-dirname-derivation-reading')

# ---- (F5) THE DONE STAMP UNDER-REPORTED AN EIGHTEEN-ROW TRAIN --------------------------------------
#      ⚠ SEAT16..SEAT18 WERE DEFINED AND REFERENCED NOWHERE.  A DONE stamp that lists fifteen of
#      eighteen pins is a record that reads complete, and the three it drops are the three a reader
#      would have to go back to the table for.  `tooling=` is added for the same reason: the OWED
#      VECTOR stamp already carries it, and two stamps of one fact that disagree is worse than one.
DONE_NEW = ('stamp "=== ${LABEL} ASSEMBLE DONE head=$(git rev-parse --short HEAD) base=$BASE route=$ROUTE '
            'seats=$SEATS_DONE (of $SEATS_LISTED listed; skippedPending=$SEATS_SKIPPED) requireAll=$REQUIRE_ALL '
            'seatShas=[$SEAT1_SHA $SEAT2_SHA $SEAT3_SHA $SEAT4_SHA $SEAT5_SHA $SEAT6_SHA $SEAT7_SHA $SEAT8_SHA '
            '$SEAT9_SHA $SEAT10_SHA $SEAT11_SHA $SEAT12_SHA $SEAT13_SHA $SEAT14_SHA $SEAT15_SHA $SEAT16_SHA '
            '$SEAT17_SHA $SEAT18_SHA] owed=[golib=$OWED_GOLIB gen=$OWED_GEN converter=$OWED_CONV '
            'behavioral=$OWED_BEH corpus=$OWED_CORPUS golibtests=$OWED_GT docs=$OWED_DOCS claudemd=$OWED_CLAUDEMD '
            'manifest=$OWED_MANIFEST tooling=$OWED_TOOLING] seatAssertArms=$SEATASSERT_N legD=$LEGD_VERDICTS '
            'legKrows=[$LEGK_ROWS] newBehavioralProjects=$NEWBEH newBehavioralProjectsAllDepths=$NEWBEH_ALL '
            'overallFailed=$FAILED ==="')
A.subline_contains('seatShas=[$SEAT1_SHA', DONE_NEW, 'f5-done-stamp')

# ---- (F9) STALE PREMISES BESIDE GATES.  Each of these is a fact about ANOTHER train's table that the
#      rename left reading as a fact about this one.  Two of them were LIVE STAMPS.
A.subline_contains('quietly assembles a subset of the fifteen rows',
                   '''       stamp "TRAIN48_REQUIRE_ALL=0 :: stated explicitly.  ⚠ THIS IS NOT A PERMISSION TO SKIP: a PENDING row still ABORTS unless TRAIN48_SKIP_PENDING=1 is ALSO given, because with a FILLED table the dangerous run is the one that quietly assembles a subset of the rows this table declares" ;;''',
                   'f9-subset-stamp')

ROWCOUNT_NEW = r"""# ⚠ THE ROW COUNT IS WHATEVER THE TABLE ABOVE DECLARES AND IS NOT SPELLED HERE -- a spelled count is a
#   second copy of a fact the table already carries, and the two drift the first time a row is seated
#   or unseated.  The
#   variables are named by ROW NUMBER (== merge position), which
#   is what every arm below and the DONE stamp use.  The coordinator's own seat numbering is a"""
A.block(r'^# ⚠ FIFTEEN ROWS IN THIS TRAIN ', r"^#   is what every arm below and the DONE stamp use\.",
        ROWCOUNT_NEW, 'f9-row-count-comment')

G1_NEW = r"""#     ⚠ IT IS A **NO-REGRESSION READING ON THIS TRAIN, NOT A SEAT GATE**, AND THAT IS CARRIED RATHER
#     THAN RE-MEASURED.  Train 47's derive measured ITS OWN seat tips at a02ac3df3 and found no row
#     editing docs/ValidatedTestPackages.md (the roster this guard recomputes).  ⚠ THIS TABLE HAS NOT
#     BEEN MEASURED FOR IT: a carried "which row edits the roster" premise is the stale-figure class
#     (L13) wearing a measurement's clothes, so nothing here names a row.  It stays ONE run: the guard
#     walks the whole roster and every committed manifest, so one invocation covers every row that
#     touches either and counting it twice would be two readings of one measurement.  ⚠ ITS VERDICT IS THE"""
A.block(r'^#     ⚠ IT IS A \*\*REAL SEAT GATE\*\* IN THIS TRAIN',
        r'^#     seat, and counting it twice would be two readings of one measurement\.  ⚠ ITS VERDICT IS THE$',
        G1_NEW, 'f9-g1-roster-premise')

G2_NEW = r"""#     ⚠ THE EXPECTATION IS A **READING IN BOTH DIRECTIONS, DERIVED FROM THE OWED VECTOR**, never a
#     number carried from another train.  Train 43's leg REQUIRED two changed .ps1 and a zero meant a
#     seat had not landed; train 47's derive then wrote "no seat touches a .ps1 at all, so a zero is
#     the expected reading" -- a fact about THAT table.  This train carries a `tooling` class whose
#     whole deliverable can BE a .ps1, so G11(a)'s tooling arm is what says whether a non-zero is
#     OWED, and the stamps below report the count without announcing either value as a surprise.  An
#     expectation spelled here and an owe derived there is two answers to one question."""
A.block(r"^#     ⚠ THE EXPECTATION FOR THIS TRAIN IS ZERO, WHICH INVERTS TRAIN 43's LEG\.",
        r'^#     BOTH directions rather than copied over with its assertion inverted by accident\.$',
        G2_NEW, 'f9-g2-expectation')
A.subline_contains("the EXPECTED reading (no seat here carries a harness script)",
                   '''  stamp "G2 :: 0 changed .ps1 in this train's delta (a READING -- whether one is OWED is G11(a)'s tooling arm's to say, and it says so there).  The C2 preflight proved both editions live, so this zero is the tree's, not the instrument's."''',
                   'f9-g2-zero-stamp')
A.subline_contains("⚠ UNEXPECTED for this train (the coordinator's expectation is 0)",
                   '''  stamp "G2 :: $G2N changed .ps1 to parse under:$G2_EDITIONS (a READING -- whether a changed .ps1 is OWED is G11(a)'s tooling arm's to say).  Parsing every one of them, and the COUNT is reconciled against the OWED VECTOR before landing."''',
                   'f9-g2-nonzero-stamp')
A.subline_contains('STAMPED: whether the four equal 204.',
                   '#     STAMPED: whether the four equal EXPECT_G3 (fill point F4).  A roster bank landing between the fill and the assembly',
                   'f9-g3-204-narration')

LEGR_NEW = r"""#             AND THAT ROUTE IS THE STRONGER ONE WHEREVER A ROW TAKES IT: a change to
#             `src/go2cs/manualTypeOperations.go`, a MANUAL-CONVERSION REGISTRY, whose registration displaces a bodied function"""
A.block(r'^#             AND THAT ROUTE IS THE STRONGER ONE: seat 3 changes ',
        r'^#             a MANUAL-CONVERSION REGISTRY, and a registration that displaces a bodied function$',
        LEGR_NEW, 'f9-legr-second-route')

# ---- (L13) THE ELEVEN SEAT-NUMBER LITERALS THE CASE-SENSITIVE PREDICATE MISSED ---------------------
#      ⚠ TEN ARE PROSE BESIDE A GATE AND THE ELEVENTH IS A **LIVE STAMP**: LEG K stamped
#      `provenance=SEAT 3's OWN GATE -- the src/core/sync/mutex.cs retarget` for a row this train does
#      not have.  The rule forbids the SHAPE, so each is restated by CLASS or by MECHANISM.
A.subline_contains('`CLAUDE.md is UNTOUCHED ... seat 1 IS the doctrine batch` -- train 45',
                   "#        `CLAUDE.md is UNTOUCHED ... the doctrine row IS seated` -- train 45's premise, carried by",
                   'l13-doctrine-quote-hdr')
A.subline_contains("`-c core.quotepath=false`.  Seat 7's golib files carry a",
                   "  #     ⚠ EVERY PATH READ IS TAKEN WITH `-c core.quotepath=false`.  A golib row's files can carry a",
                   'l13-quotepath')
A.subline_contains('refused here with "CLAUDE.md is UNTOUCHED ... seat 1 IS the doctrine batch"',
                   '#     Train 46 run 3 refused here with "CLAUDE.md is UNTOUCHED ... the doctrine row IS seated" --',
                   'l13-doctrine-quote-g4')
A.subline_contains("# --- G10b SEAT 2's ONE .cs.auto -- shape AND provenance",
                   '# --- G10b THE REVIEW-SIBLING .cs.auto -- shape AND provenance ----------------------------------',
                   'l13-g10b-header')
A.subline_contains("⚠ THE SUBJECT IS SEAT 3's DISPLACEMENT, NOT TRAIN 45's",
                   "  #     ⚠ THE SUBJECT IS THE REGISTRY ROW'S DISPLACEMENT, NOT TRAIN 45's `.cs.auto` HUNK.  Train 45's arm re-read",
                   'l13-legd-subject')
A.subline_contains("#     Seat 3's registration DISPLACES a bodied function out of",
                   "  #     A REGISTRY row's registration DISPLACES a bodied function out of `src/core/runtime/panic.cs`, which is",
                   'l13-legd-registration')
A.subline_contains("(i9 0858372b5).  Seat 6's FIFTH ARM retires that from a",
                   "#   alias when the LOADER runs at 1.24.13 (i9 0858372b5).  THE ALIAS-DEFECT SEAT'S FIFTH ARM retires that from a",
                   'l13-cnr-fifth-arm')
A.subline_contains("⚠ THE EXPECTATION MOVED WITH SEAT 6's FIFTH ARM,",
                   "#   ⚠ THE EXPECTATION MOVED WITH THE ALIAS-DEFECT SEAT'S FIFTH ARM, AND THE PRIOR ONE IS NAMED RATHER THAN ERASED.",
                   'l13-leg5-fifth-arm')
A.subline_contains("REFLECT CANARY SET, PLUS SEAT 3's OWN ROW, PLUS THE COST CANARY",
                   '# LEG K -- THE **DERIVED** REFLECT CANARY SET, PLUS THE `sync` SEAT-GATE ROW, PLUS THE COST CANARY.  TWO-PIN.',
                   'l13-legk-header')
LEGK_SYNC = r"""#     * `sync` is **A GOLIB-CORPUS HAND-OWN SEAT GATE, NOT A CANARY**.  A hand-own retarget under
#       `src/core/sync/` emits nothing a diff can see and is invisible to LEG D and to CNR alike; such
#       a row is BANKED, so only a RUN of the banked row can say whether the hand-own still works.
#       ⚠ THE ROW IS UNCONDITIONAL AND ITS OWE IS NOT: it is cheap, and a gate dropped because this
#       table happens to seat no hand-own row is a gate that comes back only if somebody remembers.
#       It is given `-TestTimeout 20m` because it is NOT in the sweep's `$longTimeouts` table and would
#       otherwise run at the 10m default; a LARGER -TestTimeout raises the floor, a smaller one still loses."""
A.block(r"^#     \* `sync` is \*\*SEAT 3's SEAT GATE\*\*\.",
        r'^#       run at the 10m default; a LARGER -TestTimeout raises the floor, a smaller one still loses\.$',
        LEGK_SYNC, 'l13-legk-sync-note')
A.subline_contains('LKPROV="SEAT 3\'s OWN GATE -- the src/core/sync/mutex.cs retarget',
                   '''    if [ "$pkg" = "sync" ]; then LKEXTRA="-TestTimeout 20m"; LKPROV="THE GOLIB-CORPUS HAND-OWN SEAT GATE -- a hand-own retarget under src/core/sync emits nothing and is invisible to LEG D and CNR alike"; fi''',
                   'l13-legk-lkprov')

# ---- (F8) THE LEG 4 ADVISORY ARM IS **DERIVED** IN TRAIN 47 NOW, AND ONE SURVIVING STAMP IS STILL
#      RELATIVE.  Train 47 was patched on 2026-09-13 (run 6 -> run 7) so the advisory converter
#      warning count is compared against L4ADV_EXPECT=$((2 + L4ADV_LIB)), where L4ADV_LIB is predicted
#      from the TREE by an embedded python heredoc, and a mismatch calls fail_gate 'LEG-4-advisory'
#      instead of setting a bare FAILED=1 under a 'never fatal' stamp.  The PREDICTOR, the expectation
#      and both fail_gate refusals carry into train 48 through the plain copy and need no op.
#      ⚠ 2026-09-13 ~14:12 -- RUN 7 WAS KILLED AT 14:06 AND RUN 8 LAUNCHED AT 14:12 FROM A TRAIN-47
#        THAT HAD BEEN PATCHED AGAIN, in its own directory.  That patch fixed THREE of the four places
#        this block used to rewrite.  Those three ops are RETIRED here rather than kept as no-ops: an
#        op whose anchor already stands in the shape it wants can only ever refuse or lie.
#          * f8-l4adv-e1-aggregate -- RETIRED, and this one REFUSED on the first re-derive run (the
#            anchor [ "${L4ADV:-x}" = "2" ] matched 0 lines).  E1's aggregate now reads
#            [ "${L4ADV:-x}" = "${L4ADV_EXPECT:-UNDERIVED}" ] in train 47 itself (assemble.sh:4538) and
#            train 48 INHERITS it.  ⚠ UNDERIVED IS THE BETTER DEFAULT AND THAT IS THE REASON TO TAKE
#            THE INHERITED LINE RATHER THAN RE-ASSERT THIS OP'S OWN :-2 -- a :-2 default says MET on a
#            tree whose verdict line was never emitted at all, which is the unmeasured-reads-as-healthy
#            shape; UNDERIVED can never equal a count, so an unread arm refuses instead of passing.
#          * f8-l4adv-e1-stamp -- RETIRED.  Train 47's E1' MET stamp now names the derived expectation
#            (${L4ADV_EXPECT:-UNDERIVED}) and its line is BYTE-IDENTICAL to the text this op wrote
#            (measured against assemble.sh:4539, whole line).  A no-op rewrite asserts nothing.
#          * f8-l4adv-verdict-stamp -- RETIRED.  The verdict-line stamp no longer says "must be 2";
#            train 47 words it "judged below against the expectation DERIVED from the tree, never
#            against a carried baseline".  The op's replacement differed from that only in
#            capitalisation and one word ("bare"), so it fixed nothing while pinning a wording train 47
#            may legitimately move again.
#      WHAT IS LEFT is (b) -- the one place that is still RELATIVE rather than wrong:
#        (b) the DERIVED-EXPECTATION stamp calls the carried 2 "the train-46 baseline".  In train 47's
#            file that reads "the train before this one"; renamed into a train-48 file it names a train
#            two hops back.  Generalised the way §16's F7 generalises -- the MEASUREMENT keeps its
#            provenance (train 46, i9 e458b952f) and the RELATIVE wording goes.
#      ⚠ COMMENT PROVENANCE IS LEFT ALONE ON PURPOSE.  The capture block above these lines records run
#        6, the 52 = 2 + 50 split, the kinds and the mechanism at projectFileWriter.go:497; that is a
#        record of how the arm was derived and it stays exactly as train 47 wrote it.
A.subline_contains(
    'stamp "LEG 4 ADVISORY EXPECTATION DERIVED FROM THE TREE ::',
    '''  stamp "LEG 4 ADVISORY EXPECTATION DERIVED FROM THE TREE :: 2 (unsafe.Sizeof in const context, UnsafeOperations -- the baseline CARRIED IN from the preceding train, first measured at train 46 by i9 e458b952f) + $L4ADV_LIB library package(s) without a license among the measurable set (Library outputs only, one converter process per project) = $L4ADV_EXPECT; the predicted list is at $L4ADV_PRED"''',
    'f8-l4adv-expect-stamp')
# ⚠ AND THE GUARD THAT MAKES A RE-RUN UNABLE TO RE-INTRODUCE IT.  The line above is a whole-line
#   replacement, so a train-47 edit that reworded it would refuse at the anchor rather than pass
#   silently; this reads the RESULT instead, on CODE lines only, so the capture comment's own
#   "the train-46 baseline" is counted and kept.
A.absent_in_code('the train-46 baseline', 'f8-no-relative-baseline-in-code')

# ---- (F1a) G10d TREATS AN `eol=lf` PIN AS AN EXEMPTION IN ITS OWN RIGHT ----------------------------
#      COORD ruling 2026-09-13 (defect F1a, MEASURED by run 2).  G10d refused
#      docs/phase4/DATA-sweep-row-walltimes.md -- attr=[set] i/lf w/lf, CR=0 LF=395 -- a CORRECT
#      `text eol=lf` pin written by a seat's own .gitattributes.  The gate read a deliberate
#      declaration as a defect: `-text` was the only exemption it knew, and `-text` is a DIFFERENT
#      statement ("these bytes are verbatim") from `eol=lf` ("this file IS text and its worktree form
#      is LF BY INSTRUCTION").  The pin exemption is added BESIDE the -text one, counted SEPARATELY
#      and stamped by name, and is conditional on the two layers agreeing exactly as -text is.
A.subline_contains('G10DCR=0; G10DCRN=0; G10DEX=0; G10DEXNAMES=""',
                   'G10DCR=0; G10DCRN=0; G10DEX=0; G10DEXNAMES=""; G10DEOL=0; G10DEOLNAMES=""',
                   'f1a-g10d-eol-counters')
A.subline_contains('attrv=$(git check-attr text -- "$f"',
                   r'''  attrv=$(git check-attr text -- "$f" 2>/dev/null | sed 's/.*: //' | tr -d '\r')
  # ⚠ (F1a) AND THE SECOND ATTRIBUTE, READ THE SAME WAY.  `eol` is what a `text eol=lf` pin resolves
  #   to, and asking for it is the only way to tell a pinned LF file from an unconverted one: both
  #   read CR=0 in the worktree and only the attribute says which is which.
  eolv=$(git check-attr eol -- "$f" 2>/dev/null | sed 's/.*: //' | tr -d '\r')''',
                   'f1a-g10d-eol-attr')
A.subline_contains('[ "$c" = "$l" ] || { G10DCR=',
                   r'''  # ⚠ (F1a) AN `eol=lf` PIN IS AN EXEMPTION, AND IT IS NOT THE `-text` ONE.  Train 48 run 2 refused
  #   docs/phase4/DATA-sweep-row-walltimes.md (attr=[set] i/lf w/lf, CR=0 LF=395) on a pin a seat's own
  #   .gitattributes had just written -- the instrument contradicting the repository's own declaration.
  #   ⚠ AND IT IS CONDITIONAL ON THE TWO LAYERS AGREEING, exactly as the -text exemption is: a pinned
  #   file whose index and worktree forms DIFFER is not the pinned case, and calling it exempt would be
  #   the exemption growing past the reason it was granted.
  if [ "$eolv" = "lf" ]; then
    if [ -n "$ilayer" ] && [ "${ilayer#i/}" = "${wlayer#w/}" ]; then
      G10DEOL=$(( G10DEOL + 1 )); G10DEOLNAMES="$G10DEOLNAMES $f"
      stamp "      EOL-PINNED (attr eol=lf, layers AGREE $ilayer $wlayer): $f CR=$c LF=$l -- an LF worktree form is what the pin INSTRUCTS, so a CRLF assertion over it would be the gate refusing a declaration the repository makes on purpose"
      continue
    fi
    G10DCR=$(( G10DCR + 1 ))
    stamp "      CR/LF MISMATCH (attr eol=lf but the LAYERS DISAGREE $ilayer $wlayer): $f CR=$c LF=$l -- a pinned file whose index and worktree forms differ is NOT the pinned case and is not exempt"
    continue
  fi
  [ "$c" = "$l" ] || { G10DCR=$(( G10DCR + 1 )); stamp "      CR/LF MISMATCH (attr=[${attrv:-unreadable}] $ilayer $wlayer): $f CR=$c LF=$l"; }''',
                   'f1a-g10d-eol-branch')
A.subline_contains('stamp "G10d docs CR/LF :: docs/*.md files in the delta checked=',
                   r'''stamp "G10d docs CR/LF :: docs/*.md files in the delta checked=$G10DCRN mismatches=$G10DCR (must be 0) textExempt=$G10DEX [$G10DEXNAMES] eolPinned=$G10DEOL [$G10DEOLNAMES] (BOTH exclusions STAMPED beside the mismatch count, because an exclusion that is not printed is an exclusion that grows quietly, and they are counted SEPARATELY because they are different statements: -text is verbatim bytes, eol=lf is an LF worktree form BY INSTRUCTION) :: new-record shape violations=$G10DBAD (must be 0)"''',
                   'f1a-g10d-eol-stamp')
A.subline_contains('^ G10d REFUSED: a docs file that is NOT',
                   r'''[ "${G10DCR:-1}" = "0" ] || { stamp "  ^ G10d REFUSED: a docs file that is NOT \`-text\`-exempt and NOT \`eol=lf\`-pinned has a non-CRLF working-tree form, or an exempt-or-pinned file's two layers disagree.  Both exemptions are by ATTRIBUTE and both are conditional on the layers agreeing; neither is a blanket pass for docs/."; FAILED=1; }''',
                   'f1a-g10d-eol-refusal')
A.require('EOL-PINNED (attr eol=lf', 'f1a-eol-pin-exemption-kept')

# ---- (F1b) THE PRE-RESOLVED APPLY RE-MATERIALISES EACH PATH THROUGH GIT ----------------------------
#      COORD ruling 2026-09-13 (defect F1b, MEASURED by run 2).  G10d refused
#      docs/phase4/BOARD-next-validation-candidates.md -- attr unspecified, i/lf w/lf, CR=0 LF=24779.
#      The path is not pinned at all: the slot apply `cp`s the LF union file into the worktree and
#      `git add`s it, so the WORKTREE form stays LF while autocrlf would have materialised CRLF, and
#      G10d read the apply's own artefact as a seat's defect.  The index already holds the normalised
#      blob after the add; `git checkout -- <path>` writes it back out under the WORKTREE'S OWN
#      attributes, so the resolved path ends in the form the repository would have produced for it.
A.subline_contains('cp "$src" "$up" && git add -- "$up"',
                   r'''        # ⚠ (F1b) THE RESOLVED PATH IS **RE-MATERIALISED THROUGH GIT** AFTER THE ADD, and that is a
        #   ruling rather than a tidy-up.  `cp` writes the slot's bytes verbatim; the saved union files
        #   are LF, so a bare cp leaves the worktree form LF while every other file in the tree carries
        #   whatever the worktree's own attributes say.  The add normalises INTO THE INDEX and the
        #   checkout writes that blob back OUT under those attributes.
        # ⚠ AND THE `rm -f` IS LOAD-BEARING -- MEASURED, not assumed.  A plain `git checkout -- <path>`
        #   over a path whose index entry is stat-clean (which it is, one command after the add) writes
        #   NOTHING: probed on this box, cp+add reads CR=0, checkout reads CR=0, `git checkout-index -f`
        #   reads CR=0, and only rm-then-checkout reads CR=3.  A no-op wearing a conversion's clothes is
        #   exactly the shape this fix exists to remove, so the file is removed and re-created.
        #   ⚠ THE WINDOW IS SAFE BY THE CHAIN: if the checkout fails, RESAPPLIED is not incremented, the
        #   INCOMPLETE check below refuses, and its `git merge --abort` / `git reset --hard` restores.
        cp "$src" "$up" && git add -- "$up" && rm -f -- "$up" && git checkout -- "$up" && RESAPPLIED=$(( RESAPPLIED + 1 )) && stamp "    PRE-RESOLVED $up  <- $src ($(grep -ac . "$src" || true) line(s), RE-MATERIALISED through git add + rm + git checkout so the worktree form is the one this worktree's attributes dictate and not the slot's)"''',
                   'f1b-preresolved-rematerialise')
A.require('git add -- "$up" && rm -f -- "$up" && git checkout -- "$up"', 'f1b-rematerialise-kept')

# ---- (F2) THE golibtests OWE IS DERIVED FROM THE MERGED DELTA, NOT FROM A CLASS --------------------
#      COORD ruling 2026-09-13 (defect F2, MEASURED by run 2): "JUSTIFICATION FALSE ::
#      src/tests/GolibTests=0 where the OWED vector says a merged class owes it" on a healthy tree --
#      the train's two golib-corpus-handown rows touch src/core/runtime and src/go2cs and nothing under
#      src/tests/GolibTests.  That is the SAME fault the syscall DIRECTORY LITERAL was written out of:
#      an arm refusing for a class's POTENTIAL rather than for a seat's omission.  OWED_GOLIB stays
#      class-derived -- LEG 3 still RUNS for the golib family, and what a leg RUNS is a different
#      question from what a justification arm may REFUSE on.
A.subline_contains('golib|golib-gen|golib-corpus-handown|golib-converter-docs) OWED_GOLIB=1; OWED_GT=1',
                   r'''  # ⚠ (F2) `golibtests` LEFT THIS LOOP ON 2026-09-13 and is derived from the DELTA below.  A class
  #   says what a seat MAY touch; it cannot say what a seat DID touch, and G11(a) refuses on the
  #   second question.  OWED_GOLIB stays here and stays class-derived, because it selects LEGS.
  case "$c" in golib|golib-gen|golib-corpus-handown|golib-converter-docs) OWED_GOLIB=1 ;; esac''',
                   'f2-owed-gt-off-the-class')
A.subline_contains('stamp "OWED VECTOR :: golib=$OWED_GOLIB',
                   r'''# ⚠ (F2) `golibtests` IS DERIVED FROM THE **MERGED DELTA**, and it is measured HERE so the stamped
#   vector and the arm that reads it describe the same tree.  A zero is then a READING -- a train
#   whose golib-class seats legitimately touch no GolibTests file -- and LEG 3's own NOTE is what says
#   whether that zero is a finding, which is where that judgment already lived.
OWED_GT_FILES=$(git -c core.quotepath=false diff --name-only "$BASE" HEAD -- src/tests/GolibTests | grep -ac . || true)
case "${OWED_GT_FILES}" in ''|*[!0-9]*) OWED_GT_FILES=0 ;; esac
OWED_GT=0
if [ "${OWED_GT_FILES:-0}" -ge 1 ]; then OWED_GT=1; fi
stamp "OWED VECTOR :: golib=$OWED_GOLIB gen=$OWED_GEN converter=$OWED_CONV behavioral=$OWED_BEH corpus=$OWED_CORPUS golibtests=$OWED_GT (from the DELTA: $OWED_GT_FILES file(s)) docs=$OWED_DOCS claudemd=$OWED_CLAUDEMD manifest=$OWED_MANIFEST tooling=$OWED_TOOLING (DERIVED from the CLASSES of the $SEATS_DONE merged row(s): [$MERGED_CLASSES] -- EXCEPT golibtests, which is derived from the merged DELTA because a class cannot say what a seat DID touch) -- every justification arm below reads THIS vector, and so does the land script, so the two cannot drift"''',
                   'f2-owed-gt-from-delta')
A.subline_contains('for v in G11GOLIB G11GT G11SYS G11CONV G11GEN G11BEH G11CORE; do',
                   r'''for v in G11GOLIB G11GT G11SYS G11CONV G11GEN G11BEH G11CORE; do eval "case \"\${$v}\" in ''|*[!0-9]*) $v=0 ;; esac"; done
# ⚠ (F2) THE OWED VECTOR'S golibtests MEASUREMENT AND THIS ONE ARE THE **SAME QUANTITY DERIVED
#   TWICE** -- the vector is stamped before these per-directory counts exist, so the reading is taken
#   in two places over one BASE..HEAD.  They are CHECKED against each other rather than assumed equal:
#   two instruments deriving one fact independently is exactly how two instruments drift.
[ "${OWED_GT_FILES:-x}" = "${G11GT:-y}" ] || { stamp "  ^ G11(a) INSTRUMENT FAULT: the OWED vector measured src/tests/GolibTests=${OWED_GT_FILES:-unset} and the per-directory census reads ${G11GT:-unset} over the same BASE..HEAD -- one quantity, two derivations, disagreeing, which makes BOTH readings unsafe to judge on"; fail_gate G11a-gt-derivation; }''',
                   'f2-owed-gt-cross-check')
A.absent_in_code('OWED_GOLIB=1; OWED_GT=1', 'f2-no-class-derived-golibtests-owe')

# ---- closing absence assertions -------------------------------------------------------------------
A.absent('TRAIN47', 'no-TRAIN47')
A.absent_in_code('train47', 'no-train47-in-code')
A.absent('EXPECT_46', 'no-EXPECT_46')
A.absent('EXPECT_CONTAINS', 'no-EXPECT_CONTAINS')
A.absent_in_code('grep -q', 'no-grep-q')
A.absent_in_code('pipefail', 'no-pipefail')
# ⚠ **CASE-INSENSITIVE, AND THAT IS A CORRECTION RATHER THAN A WIDENING.**  The case-sensitive form
#   read 26 on the train-47 template and ZERO on the derived assembly -- while ELEVEN of the shape
#   survived in it, ten as SHOUTED prose beside a gate and one as a LIVE LEG K STAMP.  A predicate
#   that cannot see upper case reports the loudest instance of its own defect as clean.
SEATLIT = r"(?i)seat [0-9]+ is |seat [0-9]+'s |row [0-9]+ is a"
n = A.count_re(SEATLIT)
if n:
    for i, l in enumerate(A.lines):
        if re.search(SEATLIT, l):
            print('     seat-number literal line %d: %s' % (i + 1, l.strip()[:160]))
    raise SystemExit('   STEP 5 FAILED [assemble]: %d seat-number literal(s) survive' % n)
A.ops += 1
A.report.append('STEP5   %-26s 0 seat-number literals (the train-47 original reads %d)'
                % ('no-seat-literals', Doc('coord-train47-assemble.sh').count_re(SEATLIT)))

ASM_LINES = A.write('coord-train48-assemble.sh')
for r in A.report:
    print('   ' + r)
print('   assembly ops=%d  lines %d -> %d' % (A.ops, A.orig_n, ASM_LINES))


# ============================================================================================
# THE REHEARSAL
# ============================================================================================
R = Doc('coord-train47-rehearse.sh')
rename_tokens(R)
R.gsub('EXPECT_46', 'EXPECT_BASE', 'rehearse-base-var', minimum=2)
R.gsub('EXPECT_CONTAINS', 'T48_CONTAIN_PIN', 'rehearse-contain-var', minimum=1)
REH_NOTE = r"""#
# ⚠⚠ **WHAT CHANGED FOR TRAIN 48, AND WHAT DID NOT.**  The mechanism is train 47's, unchanged: the
#   seat table and the base are READ OUT OF THE ASSEMBLY SCRIPT and never retyped, the merges are
#   real, the resolutions are SLOTS the coordinator fills, and `go vet ./...` type-checks the
#   accumulation after every seat.  Three readings moved with the assembly:
#     * the base variable is `EXPECT_BASE` (train 47 spelled it `EXPECT_46`, a name that carried the
#       previous train's NUMBER into the one after it -- which is how a derive inherits a premise);
#     * the containment pin is `T48_CONTAIN_PIN`, and it reads PENDING until train 47 lands.  A
#       rehearsal against a base that does not contain it is STATED, not refused: the rehearsal onto
#       the CURRENT origin/master is exactly what a coordinator wants before the pin is fillable;
#     * the seat table's optional fields are now ORDER-FREE `key=value` pairs (`allowed=`,
#       `stack-on=`), so the row read here puts fields six and beyond into one variable and the
#       rehearsal does not interpret them at all -- the ASSEMBLY validates them, and two instruments
#       parsing one field is how they drift.
# ⚠ THE REHEARSAL SAYS NOTHING ABOUT DUPLICATION.  A cherry-pick that puts one diff on two seats
#   MERGES CLEAN -- git applies the same content twice and the second application is a no-op or a
#   trivial conflict -- so a green rehearsal is not evidence against it.  The PATCH-ID ARM in the
#   assembly is that instrument, and it runs before any merge."""
R.insert_after(r'^# ⚠ A CLEAN REHEARSAL IS NOT A CLEAN ASSEMBLY, AND THE DIFFERENCE IS PERMANENT\.', REH_NOTE,
               'rehearse-t48-note')
R.absent_in_code('EXPECT_46', 'rehearse-no-expect46')
R.absent_in_code('train47', 'rehearse-no-train47')
# ---- (F7) THE RENAME INVENTED A NOVELTY.  `go vet ./...` after every seat merge was new in TRAIN 47
#      -- this file's own note below says "the mechanism is train 47's, unchanged" -- and the blanket
#      spelling rename turned the header into a claim about a train that has never run.  Two sentences
#      of one file contradicting each other is the shape a reader resolves by believing the wrong one.
R.subline_contains('**NEW IN TRAIN 48: `go vet ./...` IN src/go2cs AFTER EVERY SEAT MERGE',
                   '# ⚠⚠ **NEW IN TRAIN 47: `go vet ./...` IN src/go2cs AFTER EVERY SEAT MERGE, AND IT NAMES THE FIRST',
                   'f7-rehearse-govet-prov')
R.absent('NEW IN TRAIN 48', 'f7-rehearse-no-invented-novelty')

# ---- ⚠ **THE SEAT-TABLE END ANCHOR NO LONGER MATCHES `|tip"$` AND THAT IS A REFUSAL, NOT A DRIFT.**
#      The rehearsal extracts SEAT_TABLE out of the assembly by finding the line that CLOSES the
#      here-quoted table, and train 47's every row ended `|tip"`.  Train 48's LAST row (row 18) is
#      `...|tip|allowed=^(\.gitattributes)?(docs/phase4/hopA-inputs/shardmap\.py)?$"` -- the closing
#      quote follows the RULING, not the tip mode -- so the old anchor resolves to NOTHING and the
#      extraction would abort.  The anchor is retargeted to the CLOSING QUOTE ITSELF, which is what it
#      was always trying to name: the first line at or after the table's start whose last byte is `"`.
#      ⚠ IT IS A DERIVE OP RATHER THAN A HAND PATCH because the next re-derive would otherwise undo it
#      without trace, and the failure it would reproduce reads as "the table has no rows".
R.subline_contains("""hi=$(awk -v s="$lo" 'NR>=s' "$ASM" | grep -nE '\\|tip"$' | head -1 | cut -d: -f1)""",
                   """hi=$(awk -v s="$lo" 'NR>=s' "$ASM" | grep -nE '"$' | head -1 | cut -d: -f1)""",
                   'rehearse-table-end-anchor')
REH_LINES = R.write('coord-train48-rehearse.sh')
for r in R.report:
    print('   ' + r)
print('   rehearsal ops=%d  lines %d -> %d' % (R.ops, R.orig_n, REH_LINES))


# ============================================================================================
# THE LAND SCRIPT
# ============================================================================================
L = Doc('coord-train47-land.sh')
rename_tokens(L)
L.gsub('EXPECT_46', 'EXPECT_BASE', 'land-base-var', minimum=3)

# the OWED vector read gains `tooling`
L.subline_contains("O_CORPUS=$(owed_of corpus); O_GT=$(owed_of golibtests); O_DOCS=$(owed_of docs); O_CLAUDEMD=$(owed_of claudemd); O_MANIFEST=$(owed_of manifest)",
                   "O_CORPUS=$(owed_of corpus); O_GT=$(owed_of golibtests); O_DOCS=$(owed_of docs); O_CLAUDEMD=$(owed_of claudemd); O_MANIFEST=$(owed_of manifest)\n"
                   "# ⚠ `tooling` IS NEW IN TRAIN 48 AND IT IS PARSED THE SAME WAY: the assembly DERIVES the vector\n"
                   "#   from the merged CLASSES and STAMPS it, and this file READS that line.  A record whose vector\n"
                   "#   carries no `tooling=` field is an OLDER assembly's record, and owed_of returns empty for it --\n"
                   "#   which the normaliser below turns into 0, i.e. 'not owed', which is the correct reading of a\n"
                   "#   train that had no tooling class at all.\n"
                   "O_TOOLING=$(owed_of tooling)",
                   'land-owed-tooling')
L.subline_contains("for v in O_GOLIB O_GEN O_CONV O_BEH O_CORPUS O_GT O_DOCS O_CLAUDEMD O_MANIFEST; do",
                   "for v in O_GOLIB O_GEN O_CONV O_BEH O_CORPUS O_GT O_DOCS O_CLAUDEMD O_MANIFEST O_TOOLING; do eval \"case \\\"\\${$v}\\\" in ''|*[!01]*) $v=0 ;; esac\"; done",
                   'land-owed-normalise')
L.subline_contains('stamp "OWED VECTOR read from the record :: golib=$O_GOLIB gen=$O_GEN converter=$O_CONV behavioral=$O_BEH corpus=$O_CORPUS golibtests=$O_GT docs=$O_DOCS claudemd=$O_CLAUDEMD manifest=$O_MANIFEST',
                   'stamp "OWED VECTOR read from the record :: golib=$O_GOLIB gen=$O_GEN converter=$O_CONV behavioral=$O_BEH corpus=$O_CORPUS golibtests=$O_GT docs=$O_DOCS claudemd=$O_CLAUDEMD manifest=$O_MANIFEST tooling=$O_TOOLING -- the directory arithmetic below is driven by THIS, not by a premise about which row was which"',
                   'land-owed-stamp')

# the anchors a TRAIN-48 record must carry
LREQ = r"""req 'FILL POINTS OK ::' 1 'the assembly met its own FILL POINTS -- the containment pin and the G3 census expectation -- BEFORE it took the run lock.  A template that ran with a placeholder in it is a battery measuring a tree nobody named, and the refusal arrives at launch rather than three hundred lines in'
req 'SEAT TABLE DECLARED STACKS ::' 1 'the assembly stated which rows are DECLARED STACKS before it merged anything.  A declared stack NARROWS the patch-id census (git merges one commit once, so the union carries the content once); an exemption nobody can see is an exemption nobody re-reads'
req 'PATCH-ID ARM (CLEAN|REFUSED|MISUSE|UNMEASURED|coverage|command|self-test|tool)' 1 'the patch-id arm REPORTED.  ⚠ Its ABSENCE is the state this anchor exists for: ancestry cannot see a cherry-pick -- the same content under a new SHA makes is-ancestor read false right up until the train lands the same diff twice -- so a record with no patch-id line at all was produced by an assembly that never ran the census'
req 'A6 pins UNMOVED ::' 1 'A6 the toolchain declaration and the corpus pin are unmoved and no seat touched a pin file'"""
L.subline_contains("req 'A6 pins UNMOVED ::'", LREQ, 'land-req-t48')

LNOTES = r"""stamp "LANDING NOTES :: (7) THE **PATCH-ID ARM** RAN BEFORE ANY MERGE, and its verdict is in the record above.  Ancestry cannot see a cherry-pick: the same content under a new SHA makes \`merge-base --is-ancestor\` read false and \`git log <base>..<seat>\` list a commit nobody recognises, right up until the train lands the same diff twice.  A DECLARED STACK is exempt only because git merges one commit ONCE; a DUPLICATE patch-id is not exempt by any declaration."
stamp "LANDING NOTES :: (8) NO CLAIM IN THIS FILE IS KEYED TO A **ROW NUMBER**.  Train 47's assembly made thirty-eight assertions of the form 'seat N is ...'; each was a fact about that table, and a derive inherits them silently because they read like documentation.  Every claim here is stated in terms of the OWED VECTOR or of a row's CLASS."
"""
L.insert_after(r'^stamp "LANDING NOTES :: \(6\)', LNOTES, 'land-notes-t48')

LAND_ROW8 = r"""# ⚠ **A ROW-SPECIFIC LAND ANCHOR IS DELETED WITH ITS ARM, NEVER RE-POINTED.**  Train 47's land
#   required a stamp that its own row-8 SEAT-CONTENT arm emitted.  This template's SEAT-CONTENT block
#   is an empty SLOT -- one arm per seat, written at seat fill -- so nothing emits that stamp, the
#   anchor could NEVER be satisfied, and the landing would refuse a HEALTHY battery.  That is the
#   shape that teaches an operator to reach for the override.  The dry-read's own ARM A found it.
#   What survives are the anchors a TEMPLATE can guarantee: the structural seat-table check, the OWED
#   vector, the SEAT-CONTENT arm COUNT, the A7 allowed set, and this train's own FILL POINTS, DECLARED
#   STACKS and PATCH-ID ARM stamps.
"""
L.block(r"^#[^A-Za-z]*the former row 8's", r"^req 'A-row8 generic cross-package alias qualifier",
        LAND_ROW8, 'land-row8-anchor-deleted')
L.absent('A-row8', 'land-no-row8-anchor')

# ---- (F4) THE SEAT-COUNT FLOOR WAS CARRIED BYTE-FOR-BYTE WITH ITS TABLE'S NUMBER IN IT -------------
LAND_FLOOR = r"""# ⚠ **THERE IS NO SEAT-COUNT FLOOR HERE, AND THE DELETION IS THE FIX RATHER THAN AN OMISSION.**
#   Train 47 carried `[ "${SEATSN:-0}" -ge 15 ]` at this point under a comment saying the floor MOVES
#   WITH THE TABLE IN BOTH DIRECTIONS -- and a derive carries the NUMBER, not the comment.  Left as it
#   stood it was a fact about train 47's fifteen filled rows sitting in a file whose table declares a
#   different number, which is the stale-figure class holding a gate's pen.
#   ⚠ WHAT IS LOST BY DELETING IT: nothing this file was relying on.  The row-count EQUALITY on the
#   next line is STRICTLY STRONGER -- a partial run lists N rows and merges fewer, and the equality
#   refuses exactly that -- and requireAll=1 and skippedPending=0 are both asserted above.  What is
#   gained is that NO seat-count literal survives in this file at all, which is the thing the
#   dry-read's ARM F was written to assert and could not, because its own predicate saw only the
#   `"$SEATSN" = "N"` equality form and never the `-ge` form this floor was written in."""
L.block(r"^# ⚠ THE FLOOR IS \*\*FIFTEEN\*\*, AND IT IS A FACT ABOUT THIS TRAIN'S TABLE",
        r'^\[ "\$\{SEATSN:-0\}" -ge 15 \] \|\| \{ stamp ', LAND_FLOOR, 'f4-seat-floor-deleted')
#      ⚠ THE ABSENCE IS TAKEN **IN CODE ONLY**: the comment above necessarily spells the floor it
#        deleted, and "zero in code" and "none anywhere" are different claims.
L.absent_in_code('SEATSN:-0}" -ge', 'f4-no-seat-count-floor')

# ---- (F6) `tooling` WAS PARSED, STAMPED AND THEN NEVER ARMED --------------------------------------
#      The vector line already says the directory arithmetic below is driven by THIS; without an arm
#      that sentence was false for one field of it.  A train whose only tooling row lands nothing under
#      the tool pathspec was refused by the ASSEMBLY's G11(a) and passed by the LANDING -- two
#      instruments reading one owe and disagreeing, which is the shape that gets the weaker one quoted.
L.subline_contains('D_BEHMOD=$(git -c core.quotepath=false diff --name-only --diff-filter=MD',
                   '''D_BEHMOD=$(git -c core.quotepath=false diff --name-only --diff-filter=MD $BASE_EXPECT..HEAD -- src/tests/Behavioral | grep -avE '^src/tests/Behavioral/BehavioralTests/[^/]+Tests\\.cs$' | grep -c . || true)
# ⚠ THE TOOLING PATHSPEC IS A **SET**, NOT A DIRECTORY.  `src/utilities` alone reads ZERO on a row
#   whose whole deliverable is a root-level script, which is the reading that would make an OWED and
#   ABSENT tooling row look discharged.  It is the same set the assembly's G11(a) tooling arm uses.
D_TOOL=$(git -c core.quotepath=false diff --name-only $BASE_EXPECT..HEAD -- 'src/*.sh' 'src/*.ps1' src/utilities | grep -c . || true)''',
                   'f6-land-tooling-measure')
L.subline_contains("land_arm \"$O_CLAUDEMD\" \"$(git -c core.quotepath=false diff --name-only $BASE_EXPECT..HEAD -- CLAUDE.md",
                   '''land_arm "$O_TOOLING"  "$D_TOOL"   'src/*.sh + src/*.ps1 + src/utilities' 'G2 parses every CHANGED .ps1 under BOTH PowerShell editions -- the only gate on this train that reads a harness script at all, which is why a tooling class owes it BY NAME rather than by being swept up in some other leg'
land_arm "$O_CLAUDEMD" "$(git -c core.quotepath=false diff --name-only $BASE_EXPECT..HEAD -- CLAUDE.md | grep -c . || true)" 'CLAUDE.md' 'G4 runs with teeth on a doctrine batch\'''',
                   'f6-land-tooling-arm')
L.subline_contains('stamp "delta by directory :: src/go2cs=$D_CONV',
                   '''stamp "delta by directory :: src/go2cs=$D_CONV src/go2cs.slnx=$D_SLNX src/core/golib=$D_GOLIB src/gen=$D_GEN src/tests/GolibTests=$D_GT src/tests/Behavioral=$D_BEH src/core=$D_CORE docs=$D_DOCS tooling(src/*.sh,src/*.ps1,src/utilities)=$D_TOOL :: behavioral files MODIFIED or DELETED other than the four MSTest classes=$D_BEHMOD"''',
                   'f6-land-tooling-stamp')

# ---- (F3) THE LEG U ANCHORS ARE GUARDED BY THE SAME VECTOR FIELD THAT KEYS THE LEG -----------------
LAND_LEGU = r"""# --- LEG U.  ⚠ ITS THREE ARMS ARE REQUIRED SEPARATELY, because the pair is the
#     measurement: a CLEAN baseline, then the SAME entry refused on a foreign target AND published as
#     out of scope, then the SAME entry absorbed on this one.  Either direction alone is an assertion.
#     ⚠ AND THEY ARE REQUIRED **ONLY WHEN THE CONVERTER OWE SAYS THE LEG RAN**.  The assembly keys
#     LEG U to OWED_CONV rather than to a row number; requiring its stamps unconditionally would make
#     a train with no converter row refuse a HEALTHY landing, which is the fault the deleted per-row
#     anchor above was written up for.  When the leg did not run, that is STATED here, not skipped.
if [ "$O_CONV" = "1" ]; then
req 'LEG U ARM 1 \(CLEAN, no manifest\) :: exit=0 orphans=0' 1 'LEG U the CLEAN unicode/utf8 -tests run exited 0 with zero orphan reports -- the baseline both controlled arms are read against'
req 'LEG U ARM 2 MET :: not absorbed, and listed out of scope BY NAME' 1 'LEG U the NEGATIVE arm: a foreign-GOOS-scoped disclosure was NOT applied AND was PUBLISHED in outOfScopeDisclosures.  ⚠ Two different claims: an entry silently dropped and one deliberately withheld produce the same absorption count'
req 'LEG U ARM 3 MET :: absorbed on this target and reported by name' 1 "LEG U the POSITIVE arm: the SAME entry re-scoped to this run's own target WAS applied and reported.  One axis moved between the arms -- the scope string -- so the difference is attributable to the scope rule and nothing else"
req 'LEG U post-restore ::.*must be 0' 1 'LEG U restored the tree: the planted manifest deleted, every file under the package BYTE-IDENTICAL to its committed blob, and deleted-tracked zero'
else
stamp "LEG U anchors NOT REQUIRED :: the record's OWED VECTOR reads converter=$O_CONV, so the assembly stood LEG U down and its four stamps do not exist.  A READING, and the reason is named: an anchor that can never be satisfied is what teaches an operator to reach for the override."
fi"""
L.block(r"^# --- LEG U, row 1's own seat gate\.", r"^req 'LEG U post-restore ::", LAND_LEGU, 'f3-land-legu-guard')
L.absent_in_code('EXPECT_46', 'land-no-expect46')
L.absent_in_code('train47', 'land-no-train47')
L.absent_in_code('grep -q', 'land-no-grep-q')
L.absent_in_code('pipefail', 'land-no-pipefail')
LN = L.count_re(SEATLIT)
if LN:
    for i, l in enumerate(L.lines):
        if re.search(SEATLIT, l):
            print('     seat-number literal line %d: %s' % (i + 1, l.strip()[:160]))
    raise SystemExit('   STEP 5 FAILED [land]: %d seat-number literal(s) survive' % LN)
L.ops += 1
L.report.append('STEP5   %-26s 0 seat-number literals' % 'no-seat-literals')
LAND_LINES = L.write('coord-train48-land.sh')
for r in L.report:
    print('   ' + r)
print('   land ops=%d  lines %d -> %d' % (L.ops, L.orig_n, LAND_LINES))


# ============================================================================================
# THE LAND DRY-READ
# ============================================================================================
D = Doc('coord-train47-land-dryread.sh')
rename_tokens(D)

# ARM C's control words must be words THIS train's assembly does NOT have, even as prose.  A control
# built from words the file happens to contain proves nothing; one built from a PREVIOUS train's words
# is the shape that decays the moment a derive keeps the prose.
D.subline_contains("for bad in 'FinalizerBindingTests' 'coord-doctrine-batch18'",
                   "# ⚠ THE CONTROL WORDS ARE RE-CUT PER TRAIN, AND THEY ARE **MEASURED**, NOT ASSUMED.  Each must be\n"
                   "#   absent from THIS train's assembly; the arm below FAILS if any is found, which is what keeps the\n"
                   "#   control a control rather than a decoration.  Train 47's own seat words are used here precisely\n"
                   "#   because they are the words a derive would have carried in if it had carried anything.\n"
                   "for bad in 'CollidingPackageNames' 'coord-orphan-disclosure-check' 'laneR-armc-guard' 'c2-sync-disclosure-retire' 'laneR-h5-lastrung' 'coord-pprof-vacuous-audit' 'g-generic-alias-recut' 'c1-crashwhiletracing-marking' 'c1-lockosthread-body'; do",
                   'dr-armc-words-t48')

DARMG = r"""
say ""
say "== ARM G: the TRAIN-48 properties the land script must carry =="
say "   ⚠ EACH IS MEASURED ON THE LAND SCRIPT ITSELF, not claimed.  An anchor whose words no longer"
say "     appear anywhere in the assembly can never be satisfied, so the landing would refuse a HEALTHY"
say "     battery and the operator then reaches for the switch that makes it stop refusing."
GBAD=0
for tok in 'O_TOOLING' 'tooling=$O_TOOLING' 'PATCH-ID ARM' 'SEAT TABLE DECLARED STACKS ::' 'FILL POINTS OK ::'; do
  c=$(grep -acF -- "$tok" "$LAND" || true)
  if [ "${c:-0}" -ge 1 ]; then say "   ok   the land script carries [$tok] x$c"
  else say "   FAIL: [$tok] is absent from the land script -- a train-48 record would be read with a train-47 reader"; GBAD=1; fi
done
# and the NEGATIVE half: the land script must NOT have kept the previous train's base variable name.
for tok in 'EXPECT_46' 'TRAIN47_' 'coord-train47-'; do   # [t47-control]
  c=$(grep -acF -- "$tok" "$LAND" || true)
  if [ "${c:-0}" = "0" ]; then say "   ok   the land script carries NO [$tok]"
  else say "   FAIL: [$tok] survives in the land script x$c -- it would read a variable this train's assembly does not write"; GBAD=1; fi
done
[ "$GBAD" = "0" ] && say "   PASS: the land script reads the train-48 vector and anchors, and carries no train-47 spelling" || FAIL=1
"""
D.block_to_eof(r'PASS: the land script carries no seat-count literal, reads the OWED vector',
               DARMG + '\nsay ""\nsay "=== DRY READ DONE overallFail=$FAIL ==="\nexit $FAIL\n',
               'dr-armg', start_off=1)
# ⚠ THE ONE LIVE `train47` SPELLING IN THIS FILE IS **CONTROL WIRING**: ARM G asserts that the LAND
#   script carries no train-47 token, which it can only do by naming one.  A blanket absence check
#   would rewrite the control to look for the very token it was written to forbid -- the shape that
#   pointed train 47's own arm-12 controls at their own output on their first run.
# ---- (F7) THE RENAME MOVED A TRAIN-47 SEAT ONTO TRAIN 48'S TABLE ----------------------------------
#      `claude/c2-census-reader` is not on this train's table at all; the sentence records WHY a
#      control word was removed at train 47's seat fill, which is exactly the kind of provenance the
#      derive keeps -- so it is restored rather than deleted.
DR_CENSUSREADER = r"""#   the control WORKING rather than a control being weakened.  Train 47 seated `claude/c2-census-reader`,
#   whose deliverable IS `src/core/golib/Q44RegistryCensus.cs`, so THAT assembly's own per-row content"""
D.block(r'^#   the control WORKING rather than a control being weakened\.  Train 48 seats ',
        r"^#   whose deliverable IS `src/core/golib/Q44RegistryCensus\.cs`, so the assembly's own row-9 content$",
        DR_CENSUSREADER, 'f7-dr-censusreader-prov')

# ---- (F4b) ARM F's SEAT-COUNT PREDICATE COULD NOT SEE THE FORM THE LAND ACTUALLY CARRIED -----------
#      It matched only `"$SEATSN" = "15"` and printed `seat-count literals=0` over a file carrying
#      `[ "${SEATSN:-0}" -ge 15 ]`.  An arm that reports a clean zero over the defect it exists for is
#      worse than no arm: it is the defect plus a certificate.  The predicate now reads the whole
#      family of numeric comparisons, and it is SHOWN to fire on a planted one in the same run.
DR_ARMF = r"""# (1) NO row-count literal, and no hardcoded seat SHA.
#     ⚠ THE PREDICATE COVERS EVERY NUMERIC COMPARISON FORM, not just the `=` equality.  Train 48's
#       first cut of this arm matched `"$SEATSN" = "N"` alone and read ZERO over a land script that
#       carried `[ "${SEATSN:-0}" -ge 15 ]` three lines from here -- the arm's own subject, invisible
#       to it.  -ge/-gt/-le/-lt/-eq/-ne and = are all seat-count literals.
#       ⚠ THE GAP BETWEEN THE NAME AND THE OPERATOR IS `[^[:alpha:]]`, NOT `[^0-9]`: the floor was
#         written `[ "${SEATSN:-0}" -ge 15 ]` and a no-digits gap cannot cross its own `:-0` DEFAULT.
#         The first cut of this widening read ONE of the two planted forms and its control said so.
SEATLIT_LAND_RE='SEATS[A-Z]*[^[:alpha:]]{0,10}(-ge|-gt|-le|-lt|-eq|-ne|=)[[:space:]]*"?[0-9]+'
#     ⚠ COMMENT HITS ARE **COUNTED AND REPORTED, NOT REFUSED**.  A file that RETIRES a floor
#       necessarily spells the floor it retired, and "zero in code" and "none anywhere" are different
#       claims -- only one of which is true, and refusing the true one would delete the record of what
#       was removed.  The exemption is by COMMENT LINE and by nothing else.
seatcount_code(){ grep -anE "$SEATLIT_LAND_RE" "$1" 2>/dev/null | grep -avE '^[0-9]+:[[:space:]]*#' || true; }
n=$(seatcount_code "$LAND" | grep -ac . || true)
nall=$(grep -acE "$SEATLIT_LAND_RE" "$LAND" || true)
say "   seat-count literals in the land script :: IN CODE=$n (want 0 -- the record's own DONE stamp and the assembly's structural check are what bound the table) :: in COMMENTS=$(( ${nall:-0} - ${n:-0} )) (a READING)"
[ "${n:-0}" = "0" ] || { say "   FAIL: a seat-count literal survives IN CODE:"; seatcount_code "$LAND" | head -4 | cut -c1-160 | sed 's/^/        /'; FBAD=1; }
# --- THE RED CONTROL, IN-RUN.  ⚠ THREE PLANTED LINES: the `-ge` floor, the `=` equality and a
#     COMMENTED floor.  The first two must be SEEN and the third must NOT -- a control that only
#     proved the arm fires would not prove the comment exemption is narrow.
PLANTF=$(mktemp "/tmp/dryread-seatcount-control.XXXXXX")
{ printf '%s\n' '[ "${SEATSN:-0}" -ge 15 ] || exit 4'
  printf '%s\n' '[ "$SEATSN" = "15" ] || exit 4'
  printf '%s\n' '#   the floor was [ "${SEATSN:-0}" -ge 15 ] and it was deleted'; } > "$PLANTF"
c=$(seatcount_code "$PLANTF" | grep -ac . || true)
call=$(grep -acE "$SEATLIT_LAND_RE" "$PLANTF" || true)
if [ "${c:-0}" = "2" ] && [ "${call:-0}" = "3" ]; then say "   ok   CONTROL :: the predicate reads $c IN CODE on a planted file carrying the -ge floor AND the = equality, and $call in all -- it fires on either form and the comment exemption takes exactly the one comment"
else say "   FAIL CONTROL: the predicate reads ${c:-0} in code and ${call:-0} in all on a plant built to read 2 and 3, so its zero above says nothing"; FBAD=1; fi
rm -f "$PLANTF"
"""
D.block(r'^# \(1\) NO row-count literal, and no hardcoded seat SHA\.$',
        r'^\[ "\$\{n:-0\}" = "0" \] \|\| \{ say "   FAIL: a seat-count literal survives"; FBAD=1; \}$',
        DR_ARMF, 'f4b-dr-armf-predicate')

D.absent_in_code('train47', 'dr-no-train47', allow=("for tok in 'EXPECT_46'",))
D.absent_in_code('grep -q', 'dr-no-grep-q')
DN = D.count_re(SEATLIT)
if DN:
    raise SystemExit('   STEP 5 FAILED [dryread]: %d seat-number literal(s) survive' % DN)
D.ops += 1
D.report.append('STEP5   %-26s 0 seat-number literals' % 'no-seat-literals')
DR_LINES = D.write('coord-train48-land-dryread.sh')
for r in D.report:
    print('   ' + r)
print('   dry-read ops=%d  lines %d -> %d' % (D.ops, D.orig_n, DR_LINES))


# ============================================================================================
# THE DERIVE-TIME SELF-CHECK
# ============================================================================================
S = Doc('coord-train47-derive-selfcheck.sh')
rename_tokens(S)
S.gsub('EXPECT_46', 'EXPECT_BASE', 'sc-base-var', minimum=5)
S.gsub('EXPECT_45', 'EXPECT_ORDER', 'sc-order-var', minimum=2)
S.gsub('EXPECT_CONTAINS', 'T48_CONTAIN_PIN', 'sc-contain-var', minimum=2)

SC_SETUP = r"""
# --- ⚠ **THE OFFLINE SWITCH, AND WHY IT IS NOT A LOOSENING.**  Arms 1, 3 and 8 FETCH each seat's ref
#     before deciding whether its pin resolves, because "unresolved" before a fetch is a stale clone
#     and after one is a typo or an invention -- opposite severities.  A box under a FROZEN battery
#     may forbid a fetch outright, and an instrument that silently skipped the fetch would turn the
#     second reading into the first.  So the switch is NAMED, it is OFF by default, and when it is ON
#     an unresolvable pin is reported UNMEASURED **and still FAILS**: an instrument that could not run
#     has not found nothing, it has found nothing out.  The count is carried separately so a reader
#     can tell a fault from a blindness.
T48_SELFCHECK_OFFLINE="${T48_SELFCHECK_OFFLINE:-0}"
OFFLINE_UNMEASURED=0
sc_fetch(){ # $1 = a refspec (or the word master)
  if [ "$T48_SELFCHECK_OFFLINE" = "1" ]; then return 0; fi
  git -C "$G" fetch -q origin "$1" 2>/dev/null || true
}
[ "$T48_SELFCHECK_OFFLINE" = "1" ] && say "⚠ T48_SELFCHECK_OFFLINE=1 :: NO fetch is performed.  Every pin that does not resolve in this clone is reported UNMEASURED and FAILS -- it is NOT reported as an invention, because without a fetch those two states are indistinguishable."

# --- the TRAIN-47 originals, for the NEW arms' negative controls.  ⚠ COMPOSED FROM A NUMBER, NEVER
#     SPELLED: this file is written by a derive that runs a global coord-train47-* -> coord-train48-*
#     rename over it, and a spelled name here would be rewritten to point each control at the very
#     file it controls AGAINST.  Train 47's own arm 12 did exactly that on its first run.
T47NUM=47
if [ -z "${T47DIR:-}" ] && [ -d "$SELFDIR/../train${T47NUM}" ]; then T47DIR="$(cd "$SELFDIR/../train${T47NUM}" && pwd)"; fi
T47DIR="${T47DIR:-$SELFDIR}"
T47ASM="$T47DIR/coord-train${T47NUM}-assemble.sh"
T47LAND="$T47DIR/coord-train${T47NUM}-land.sh"
T47DRY="$T47DIR/coord-train${T47NUM}-land-dryread.sh"
"""
S.insert_after(r'^\[ -n "\$BASE_EXPECT" \] \|\| BASE_EXPECT=UNREADABLE$', SC_SETUP, 'sc-setup')

S.gsub('git -C "$G" fetch -q origin master 2>/dev/null', 'sc_fetch master', 'sc-fetch-master', minimum=2)
S.gsub('git -C "$G" fetch -q origin "+refs/heads/$b:refs/remotes/origin/$b" 2>/dev/null',
       'sc_fetch "+refs/heads/$b:refs/remotes/origin/$b"', 'sc-fetch-seat')

# ---- arm 1: a PENDING containment pin is FILL POINT F2, not a failed containment --------------
S.subline_contains('    if [ -d "$G/.git" ] && [ -n "${ECON:-}" ]; then',
                   r"""    # ⚠ A **PENDING** CONTAINMENT PIN IS THE EXPECTED STATE OF THIS TEMPLATE AND IS NOT A FAILED
    #   CONTAINMENT.  The SHA train 47 lands did not exist when this was derived.  What is asserted
    #   here instead is that the assembly REFUSES the placeholder at launch -- an unfilled pin that
    #   RUNS is a battery measuring a tree nobody named -- and containment itself is left UNMEASURED
    #   and SAID to be, rather than claimed either way.
    case "${ECON:-}" in
      PENDING|*PENDING*)
        say "   ⚠ T48_CONTAIN_PIN reads [$ECON] -- FILL POINT F2, and the EXPECTED state of a template derived before train 47 landed.  CONTAINMENT IS UNMEASURABLE until it is filled and is NOT claimed here."
        FP2=$(grep -ac 'FILL POINT F2 REFUSED' "$F" || true)
        say "   the assembly's own FILL-POINT F2 refusal is present x${FP2:-0} (want >= 1)"
        [ "${FP2:-0}" -ge 1 ] || { say "   FAIL: the containment pin is a placeholder AND nothing refuses it -- the run would proceed against a base nobody asserted"; B1OK=0; }
        ;;
    esac
    if [ -d "$G/.git" ] && [ -n "${ECON:-}" ] && [ "${ECON#*PENDING}" = "$ECON" ]; then""",
                   'sc-arm1-pending-pin')

# ---- arm 2: `PENDING-class` is a CLASS FILL POINT, and it must NOT have a merge_seat arm --------
S.subline_contains("""  if [ "$(grep -acF -- "    ${c})" "$F" || true)" != "0" ]; then say "   class '$c' has a merge_seat arm"; else say "   FAIL: class '$c' has NO merge_seat arm"; CLSOK=0; fi""",
                   r"""  # ⚠ `PENDING-class` IS A **CLASS FILL POINT**, AND THE CORRECT READING IS THAT IT HAS **NO**
  #   merge_seat ARM.  A class decides the shape a seat is held to; implementing an arm for the
  #   placeholder would let a row whose class nobody decided merge under a shape nobody ruled.  What
  #   is asserted instead is the pair of refusals that make the placeholder safe: the assembly's
  #   structural check refuses a FILLED row carrying it, and merge_seat's unknown-class arm refuses it
  #   if one ever reached the loop.
  if [ "$c" = "PENDING-class" ]; then
    PCF=$(grep -acF -- 'seat_class_placeholders' "$F" || true)
    PCA=$(grep -acF -- "    ${c})" "$F" || true)
    say "   class '$c' is a FILL POINT :: the structural placeholder refusal is wired x${PCF:-0} (want >= 2 -- the reader and its consumer) :: it has a merge_seat arm x${PCA:-0} (want 0 -- a placeholder with an arm is a shape nobody ruled)"
    { [ "${PCF:-0}" -ge 2 ] && [ "${PCA:-0}" = "0" ]; } || { say "   FAIL: the class placeholder is not refused structurally, or an arm was written for it"; CLSOK=0; }
  elif [ "$(grep -acF -- "    ${c})" "$F" || true)" != "0" ]; then say "   class '$c' has a merge_seat arm"; else say "   FAIL: class '$c' has NO merge_seat arm"; CLSOK=0; fi""",
                   'sc-arm2-pending-class')

# ---- arm 3: offline-aware resolution -----------------------------------------------------------
S.subline_contains("""      say "   FAIL: seat $n's SHA $s ($b) does not resolve even after the fetch -- an unresolved SHA and an INVENTED one read the same\"""",
                   r"""      if [ "$T48_SELFCHECK_OFFLINE" = "1" ]; then
        say "   FAIL (UNMEASURED, offline): row $n's SHA $s ($b) is not in this clone and T48_SELFCHECK_OFFLINE=1 forbade the fetch.  ⚠ THIS IS NOT THE CLAIM THAT THE SHA IS INVENTED -- without a fetch a stale clone and an invention are indistinguishable, and an arm that could not run is not a pass.  Re-run WITHOUT the switch to turn this into a measurement."
        OFFLINE_UNMEASURED=$(( OFFLINE_UNMEASURED + 1 ))
      else
        say "   FAIL: row $n's SHA $s ($b) does not resolve even after the fetch -- an unresolved SHA and an INVENTED one read the same"
      fi""",
                   'sc-arm3-offline')


# --- \u26a0 ARM 8 SPLITS **UNMEASURED** FROM **REFUSED BY ITS CLASS**.  Train 47's arm set ONE flag for
#     both, so a ref this clone does not carry printed "a seat would be REFUSED by its own class" --
#     an instrument's blindness reported as a finding about a seat, which is a false red by the
#     definition this project uses.  Two counters, two sentences, and the UNMEASURED one still FAILS.
S.subline_contains('  SH_BAD=0', '  SH_BAD=0; SH_UNM=0', 'sc-arm8-counters')
S.subline_contains('      say "   seat $n ($c) $b :: the remote ref is not present locally -- fetch it before believing this arm; UNMEASURED"',
                   '      say "   row $n ($c) $b :: the remote ref is not present locally -- fetch it before believing this arm; UNMEASURED"\n'
                   '      SH_UNM=$(( SH_UNM + 1 ))\n'
                   '      [ "$T48_SELFCHECK_OFFLINE" = "1" ] && OFFLINE_UNMEASURED=$(( OFFLINE_UNMEASURED + 1 ))',
                   'sc-arm8-unmeasured')
SC_ARM8_VERDICT = (
    '  say "   arm 8 coverage :: rows read=$(( SHN - SH_UNM )) of $SHN :: rows UNMEASURED because their ref is absent from this clone=$SH_UNM"\n'
    '  if [ "$SH_BAD" = "0" ]; then\n'
    '    say "   PASS: every FILLED row\'s REAL diff is admitted by its own class shape and touches none of its forbidden paths"\n'
    '  elif [ "$SH_UNM" -ge 1 ]; then\n'
    '    say "   FAIL (UNMEASURED): $SH_UNM row(s) could not be read at all because their ref is not in this clone.  \u26a0 THIS IS NOT THE CLAIM THAT A ROW VIOLATES ITS CLASS -- an instrument that could not run has not found nothing.  Any row that DID read outside its shape is NAMED above; if none is, this arm is blind rather than red."\n'
    '    FAIL=1\n'
    '  else\n'
    '    say "   FAIL: a row would be REFUSED by its own class -- and this is a RULING THE COORDINATOR OWNS, not a shape to widen here."\n'
    '    FAIL=1\n'
    '  fi')
S.subline_contains('  [ "$SH_BAD" = "0" ] && say "   PASS: every FILLED seat', SC_ARM8_VERDICT, 'sc-arm8-verdict')
S.subline_contains('  SH_BAD=0; SH_UNM=0', '  SH_BAD=0; SH_UNM=0; SHN=$(printf \'%s\\n\' "$SEAT_TABLE" | grep -c .)', 'sc-arm8-rowcount')

# --- ⚠⚠ **ARM 8 MEASURED A STACKED ROW AGAINST THE WRONG BASE** (COORD E2, 2026-09-13 ~20:00).
#     merge_seat measures a seat against `merge-base HEAD "$want"`, and by the time a row declaring
#     `stack-on=N` merges, HEAD ALREADY CARRIES ROW N -- so the assembly reads the child's OWN delta
#     while this arm, anchored on the train's base, credited the child with every file its PARENT
#     changed.  That is not a harmless over-read: it is what made row 13's INHERITED `allowed=` ruling
#     read LIVE here and STALE at assembly, i.e. the one shape these arms exist to catch, surviving a
#     green self-check.  The base rule is now the ASSEMBLER'S, read from the table's own pinned value
#     and never from master.  ⚠ AND A STACKED ROW WHOSE PARENT PIN DOES NOT RESOLVE IS **UNMEASURED**
#     rather than silently re-based on master: falling back would restore exactly the over-read.
S.subline_contains('    mb=$(git -C "$G" merge-base "$BASE_EXPECT" "$s" 2>/dev/null)',
                   r"""    sob=$(printf '%s\n' "$SEAT_TABLE" | awk -F'|' -v r="$n" '$1==r{for(i=6;i<=NF;i++) if ($i ~ /^stack-on=/) {print substr($i,10); exit}}')
    mbref="$BASE_EXPECT"; mbwhy="the train's base [$BASE_EXPECT]"
    if [ -n "$sob" ]; then
      sobsha=$(printf '%s\n' "$SEAT_TABLE" | awk -F'|' -v r="$sob" '$1==r{print $3}')
      if [ -n "$sobsha" ] && [ "$sobsha" != "PENDING" ] && git -C "$G" cat-file -e "${sobsha}^{commit}" 2>/dev/null; then
        mbref="$sobsha"; mbwhy="row $sob's PINNED SHA [$sobsha] (this row DECLARES stack-on=$sob, and merge_seat will see row $sob in HEAD before it reaches this seat)"
      else
        say "   row $n ($c) $b :: declares stack-on=$sob and row $sob pins [${sobsha:-ABSENT}], which does not resolve in this clone -- the STACKED BASE is UNMEASURABLE, and NO reading is taken.  ⚠ Reading it against the train's base instead would credit this row with every file its PARENT changed, which is the over-read this rule exists to remove."
        SH_UNM=$(( SH_UNM + 1 ))
        [ "$T48_SELFCHECK_OFFLINE" = "1" ] && OFFLINE_UNMEASURED=$(( OFFLINE_UNMEASURED + 1 ))
        SH_BAD=1; continue
      fi
    fi
    mb=$(git -C "$G" merge-base "$mbref" "$s" 2>/dev/null)""",
                   'sc-arm8-stacked-base')

# --- and the per-row stamp NAMES the base it measured against.  A count measured against a base the
#     reader cannot see is a count the reader has to guess the meaning of, and the whole point of E2
#     is that TWO bases were in play and only one of them was the assembler's.
S.subline_contains('    say "   seat $n ($c) $b @$s :: files=$nfiles outsideShape=$out forbiddenHits=$bad allowedByRuling=$nalw"',
                   '    say "   seat $n ($c) $b @$s :: files=$nfiles outsideShape=$out forbiddenHits=$bad allowedByRuling=$nalw :: measured against $mbwhy -> merge-base $mb"',
                   'sc-arm8-stamp-base')

# --- \u26a0 ARM 13 CARRIED TWO EXPECTATIONS THAT WERE FACTS ABOUT A **FILLED** TRAIN.  A template's
#     worktree is PENDING and its SEAT-CONTENT block is EMPTY BY CONSTRUCTION; asserting the filled
#     state here would refuse every template and pass only a file nobody can re-derive.  What is
#     asserted instead is that each is a FILL POINT WITH A REFUSAL BEHIND IT.
S.subline_contains('"F1 the assembly worktree is FILLED :: [$WTVAL]"',
                   'case "$WTVAL" in\n'
                   '  \'\'|*PENDING*) a13 "$(grep -ac \'WORKTREE UNFILLED ::\' "$F" | tr -d \'\\r\')" "F1 the assembly worktree is a FILL POINT reading [$WTVAL], and the run ABORTS until it is named -- what is asserted on a TEMPLATE is that the REFUSAL exists" ;;\n'
                   '  *) a13 1 "F1 the assembly worktree is FILLED :: [$WTVAL]" ;;\n'
                   'esac',
                   'sc-arm13-worktree')
S.subline_contains('a13 "$( [ "${ARMN:-0}" -ge "${FILLEDN:-0}" ] && echo 1 || echo 0 )" "every FILLED row has a content assertion"',
                   '# \u26a0 ON A **TEMPLATE** THE SLOT IS EMPTY AND THAT IS THE CORRECT STATE.  What is asserted is the\n'
                   '#   SLOT and the GATE: the fenced block exists and the assembly REFUSES when the arm count is\n'
                   '#   below the merged-seat count.  A template shipping arms would ship assertions about content\n'
                   '#   nobody seated, and a landing would then refuse a healthy battery under a dead row\'s name.\n'
                   'if [ "${ARMN:-0}" = "0" ]; then\n'
                   '  SLOTB=$(grep -ac \'SEAT-CONTENT ASSERTIONS BEGIN\' "$F" | tr -d \'\\r\')\n'
                   '  SLOTG=$(grep -ac \'SEAT-CONTENT ASSERTIONS REFUSED\' "$F" | tr -d \'\\r\')\n'
                   '  a13 "$( [ "${SLOTB:-0}" -ge 1 ] && [ "${SLOTG:-0}" -ge 1 ] && echo 1 || echo 0 )" "the SEAT-CONTENT block is an EMPTY SLOT carrying its GATE (fenced block x${SLOTB:-0}, shortfall refusal x${SLOTG:-0}); the arms are written at seat fill, one per named row"\n'
                   'else\n'
                   '  a13 "$( [ "${ARMN:-0}" -ge "${FILLEDN:-0}" ] && echo 1 || echo 0 )" "every FILLED row has a content assertion"\n'
                   'fi',
                   'sc-arm13-seatcontent')

# --- ⚠ THE **BLOCK-SCOPED** ARM COUNT, WITH ITS REGRESSED-COPY CONTROL.  `$ARMN` above is a FILE-WIDE
#     count over non-comment lines: an increment written anywhere in the assembly would satisfy it
#     while asserting nothing about the SLOT the run-time gate reads.  This arm counts increments
#     INSIDE the fence only and requires EXACT EQUALITY with the FILLED row count -- one arm too few is
#     a seat nobody asserted, one too many is an assertion about a row nobody seated, and the run-time
#     gate (`-lt`) can only ever see the first of those.
S.insert_after(r'^say "   SEAT-CONTENT arms written=\$ARMN :: filled rows=\$FILLEDN ',
               'SCBLK=$(awk \'/=== SEAT-CONTENT ASSERTIONS BEGIN/{inb=1; next} /=== SEAT-CONTENT ASSERTIONS END ===/{inb=0} inb\' "$F" | grep -av \'^[[:space:]]*#\')\n'
               'SCARM=$(printf \'%s\\n\' "$SCBLK" | grep -acF \'SEATASSERT_N=$(( SEATASSERT_N + 1 ))\' || true)\n'
               '# ⚠ THE REGRESSED-COPY CONTROL: the SAME predicate over a copy of the block with ONE increment\n'
               '#   deleted must read one lower AND must MISS the filled-row count.  A count that agreed either way\n'
               '#   would be a green that cannot go red, which is the safety floor\'s item 13 verbatim.\n'
               'SCCTL=$(printf \'%s\\n\' "$SCBLK" | awk \'f==0 && index($0, "SEATASSERT_N + 1") > 0 {f=1; next} {print}\' | grep -acF \'SEATASSERT_N=$(( SEATASSERT_N + 1 ))\' || true)\n'
               'say "   SEAT-CONTENT arms INSIDE the fenced block=$SCARM :: filled rows=$FILLEDN :: the regressed copy (one increment deleted) reads $SCCTL"\n'
               'a13 "$( [ "${SCARM:-0}" = "${FILLEDN:-0}" ] && echo 1 || echo 0 )" "the FENCED BLOCK carries EXACTLY one arm per FILLED row ($SCARM == $FILLEDN) -- a file-wide count cannot say this"\n'
               'a13 "$( { [ "${SCCTL:-0}" = "$(( SCARM - 1 ))" ] && [ "${SCCTL:-0}" != "${FILLEDN:-0}" ]; } && echo 1 || echo 0 )" "CONTROL RED :: the same predicate over a REGRESSED copy reads $SCCTL and misses $FILLEDN, so the equality above can go red"',
               'sc-arm13-blockscoped-armcount')

# --- \u26a0 A `claude/PENDING-BRANCH` REF IS A **REF FILL POINT**: the SHA is known and the branch name
#     was NOT given.  The ASSEMBLY refuses that shape at its SEAT PREFLIGHT, and that refusal is the
#     gate; a checker that refused every unfilled fill point would refuse every template.  So the arm
#     REPORTS the fill point and asserts the assembly's refusal for it EXISTS.
S.subline_contains('  case "$b" in claude/PENDING-*) [ "$s" = "PENDING" ] ||',
                   '  case "$b" in\n'
                   '    claude/PENDING-BRANCH)\n'
                   '      say "   row $n is a REF FILL POINT :: the SHA [$s] is known and the BRANCH NAME was not given.  The assembly REFUSES this shape at its SEAT PREFLIGHT; the name is NOT guessed here, because a row whose branch nobody named cannot have its class shape or its forbidden paths measured at all."\n'
                   '      REFFILL=$(( ${REFFILL:-0} + 1 ))\n'
                   '      RFR=$(grep -ac \'carries a FILLED SHA\' "$F" | tr -d \'\\r\')\n'
                   '      say "   the assembly\'s own preflight refusal for that shape is present x${RFR:-0} (want >= 1 -- a fill point with no refusal behind it is a default)"\n'
                   '      [ "${RFR:-0}" -ge 1 ] || { say "   FAIL: a row carries a REF fill point and NOTHING refuses it"; PHBAD=1; } ;;\n'
                   '    claude/PENDING-*) [ "$s" = "PENDING" ] || { say "   FAIL: row $n has a FILLED SHA ($s) against the PLACEHOLDER ref [$b]"; PHBAD=1; } ;;\n'
                   '  esac',
                   'sc-arm2-reffill')
S.subline_contains("      say \"   FAIL: seat $n's pin $s is NOT an ancestor of the tip $tip",
                   '      case "$b" in\n'
                   '        claude/PENDING-BRANCH) say "   row $n :: REF FILL POINT -- the branch name was not given, so there is NO TIP to compare the pin $s against.  UNMEASURED by construction, and the assembly refuses the row until the name is filled."; continue ;;\n'
                   '      esac\n'
                   '      say "   FAIL: row $n\'s pin $s is NOT an ancestor of the tip $tip -- the branch was REWRITTEN, or the pin names a different branch.  Ask the owner before re-pinning."',
                   'sc-arm3-reffill')

# --- ⚠ ARM 13's "defect (c)" LIST NAMED A **ROW-SPECIFIC** LAND ANCHOR, and that anchor was deleted
#     with its arm (the SEAT-CONTENT block is an empty slot in a template).  Requiring it here would be
#     the checker asserting a stamp nobody writes -- the same fault one layer up.  The replacement
#     members are stamps only THIS train's assembly produces and that a TEMPLATE can guarantee: its
#     fill-point stamp, its declared-stack stamp and its patch-id arm.
S.subline_contains("for rq in 'LEG U ARM 1' 'LEG U ARM 2 MET' 'LEG U ARM 3 MET' 'prediction FILTERED per target' 'A-row8 generic cross-package alias qualifier' 'BASE ANCESTRY OK'; do",
                   "for rq in 'LEG U ARM 1' 'LEG U ARM 2 MET' 'LEG U ARM 3 MET' 'prediction FILTERED per target' 'FILL POINTS OK ::' 'SEAT TABLE DECLARED STACKS ::' 'PATCH-ID ARM' 'BASE ANCESTRY OK'; do",
                   'sc-arm13-defectc')

# ---- the NEW arms (a)..(g) ----------------------------------------------------------------------
SCNEW = r'''
say ""
say "== (a) NO SEAT-NUMBER LITERAL CARRIES A CLAIM, in the assemble, the land and the dry-read =="
say "   ⚠ Train 47's assembly made THIRTY-EIGHT assertions of the form \"seat N is ...\", \"seat N's ...\""
say "     or \"row N is a <class> seat\" -- in stamps AND in the prose beside gates.  Each was a fact"
say "     about THAT table, and a derive inherits them silently because they read like documentation."
say "   ⚠ THE PREDICATE IS ONE DEFINITION RUN OVER WHATEVER FILE IT IS HANDED, so the arm and its"
say "     control are literally the same code -- which is what makes the control a control."
say "   ⚠ THE PREDICATE IS **CASE-INSENSITIVE**, AND THAT IS A CORRECTION.  The first cut of this arm"
say "     used grep -acE and read ZERO on the derived assembly while ELEVEN of the forbidden shape"
say "     survived in it -- ten as SHOUTED prose beside a gate, and one as a LIVE LEG K STAMP that"
say "     announced a row number as the provenance of the \`sync\` gate.  An arm that cannot see upper"
say "     case reports the loudest instance of its own defect as clean."
SEATLIT_RE="seat [0-9]+ is |seat [0-9]+'s |row [0-9]+ is a"
seatlit_count(){ grep -aciE "$SEATLIT_RE" "$1" 2>/dev/null | tr -d '\r'; }
ABAD=0
for f in "$F" "$SELFDIR/coord-train48-land.sh" "$SELFDIR/coord-train48-land-dryread.sh"; do
  if [ ! -f "$f" ]; then say "   FAIL: [$f] is absent, so the arm could not run over it"; ABAD=1; continue; fi
  n=$(seatlit_count "$f"); n="${n:-0}"
  if [ "$n" = "0" ]; then say "   ok   $(basename "$f") :: seat-number literals=0"
  else say "   FAIL $(basename "$f") :: $n seat-number literal(s) survive:"; grep -aniE "$SEATLIT_RE" "$f" | head -8 | cut -c1-180 | sed 's/^/        /'; ABAD=1; fi
done
# --- THE RED CONTROL.  ⚠ IT IS **MEASURED**, NOT ASSUMED: the train-47 assembly carries the shape and
#     is the honest control; the train-47 LAND and DRY-READ read ZERO for it, so for those two the
#     control is a PLANTED line instead, and that difference is REPORTED rather than papered over.
if [ -f "$T47ASM" ]; then
  c=$(seatlit_count "$T47ASM"); c="${c:-0}"
  if [ "$c" -ge 1 ]; then say "   ok   CONTROL (train-47 assembly) :: the same predicate reads $c -- the arm CAN fire"
  else say "   FAIL CONTROL: the predicate reads 0 on the train-47 assembly, so its zero on the derived file says nothing"; ABAD=1; fi
else
  say "   FAIL CONTROL UNMEASURED :: [$T47ASM] is not present (override with T47DIR=<dir>) -- an arm that has never been made to fire is an assertion wearing a measurement's clothes"
  ABAD=1
fi
PLANT="/tmp/${TAG}-seatlit-control.txt"
{ printf '%s\n' 'stamp "row 4 is a docs seat, which is the premise this arm forbids"'; printf '%s\n' "# seat 7's own deliverable"; } > "$PLANT"
c=$(seatlit_count "$PLANT"); c="${c:-0}"
if [ "$c" -ge 2 ]; then say "   ok   CONTROL (planted, for the LAND and DRY-READ whose train-47 originals read ZERO) :: the predicate reads $c on two planted lines"
else say "   FAIL CONTROL: the predicate reads $c on a planted file carrying two of the forbidden shapes"; ABAD=1; fi
rm -f "$PLANT"
[ "$ABAD" = "0" ] && say "   PASS: no seat-number literal carries a claim in any of the three files, and the predicate was SHOWN to fire" || FAIL=1

say ""
say "== (b) EVERY UNANCHORED SUBSTRING MATCH ON A TOOL'S OUTPUT IS ON A WRITTEN WHITELIST =="
say "   ⚠ THE HAZARD, MEASURED RATHER THAN ASSERTED: the behavioral runner prints NOT MEASURED both as"
say "     a per-project ROW and inside its own COUNT lines (BehavioralRunner/Program.cs lines 549, 858"
say "     and 1171 build them), so a substring match counts a tally that reads ZERO as a hit.  A"
say "     verdict read out of a report is an ANCHORED ROW MATCH; what is LEFT unanchored is left"
say "     DELIBERATELY, each with a reason, and this arm refuses a NEW one."
say "   ⚠ THE DETECTOR IS NARROW ON PURPOSE: it fires only where a grep with -c or -q reads one of the"
say "     named TOOL-OUTPUT sigils and its pattern does not begin with ^.  Widening it to every grep in"
say "     the file would bury the finding in matches over the repository's own source."
TOOLSIGILS='"$L5" "$L5ENUM" "$L2B" "$L2" "$L0" "$LR2" "$LR" "$LD" "$L" "$G1LOG" "$G3LOG" "$lg" "$ASM" "$PIDCEN" "$PIDST"'
unanchored_sites(){ # $1 = file -> "<line>\t<text>" per in-scope site
  awk -v sig="$TOOLSIGILS" '
    { l=$0
      s=l; sub(/^[ \t]+/, "", s)
      if (substr(s,1,1) == "#") next
      if (l !~ /grep -a?[a-zA-Z]*[cq]/) next
      n=split(sig, S, " "); hit=0
      for (i=1;i<=n;i++) if (index(l, S[i])) hit=1
      if (!hit) next
      p=l
      if (!sub(/^.*grep -[a-zA-Z]+[ \t]+(--[ \t]+)?/, "", p)) next
      q=substr(p,1,1)
      if (q != "\047" && q != "\"") next
      if (substr(p,2,1) == "^") next
      printf "%d\t%s\n", NR, l
    }' "$1"
}
# --- THE WHITELIST.  ⚠ ONE LINE PER SITE, EACH JUSTIFIED, AND THE COUNT IS ASSERTED BOTH WAYS: a NEW
#     site is a refusal, and a REMOVED one is a refusal too, because a whitelist that outlives its
#     site is an exemption nobody re-reads.
WL_ASM="G1PASS_ANY=
G3CLEAN_ANY=
G3BAD_ANY=
CS0576=\$(grep -ac 'CS0576' \"\$L0\"
REBUILT=\$(grep -ac 'Building go2cs.exe
CS=\$(grep -acE 'error CS[0-9]+' \"\$L2\")
CS=\$(grep -acE 'error CS[0-9]+' \"\$L2B\")
RCS=\$(grep -acE 'error CS[0-9]+' \"\$LR2\"
RSTALE=\$(grep -acE 'test manifest is
orph=\$(grep -acF -- 'ORPHANED DISCLOSURE'
named=0; [ -n \"\${LEGU_NAME:-}\" ]
LEG 3 golib-\$cfg BUILD exit=\$brc
ABORT=\$(grep -ac 'Test Run Aborted'
nr=\$(grep -ac \"\$arm\" \"\$LR\"
S_NM=\$(grep -acEi 'NOT MEASURED'
S_CS0576=\$(grep -ac 'CS0576'
S_REBUILT=\$(grep -ac 'Building go2cs.exe
S_SKIP=\$(grep -acEi 'platform-exclusive"
#   why each is LEFT (in the order above):
#     1-3  the three CONVERTED sites keep their unanchored count as a deliberate SECOND READING beside
#          the anchored one, and a disagreement between the two is STAMPED as a finding.
#     4-5  'CS0576' is a compiler error CODE and 'Building go2cs.exe (converter sources changed)' is a
#          whole emitted line; both are READINGS, stamped and never asserted.
#     6-9  MSBuild diagnostics are printed INDENTED behind a `file(line,col): ` prefix, so a `^` anchor
#          would read ZERO and turn a RED build GREEN -- strictly worse than the substring.
#    10-11 the orphan-disclosure tool's own row spelling is not measurable from any artifact in this
#          directory, and an anchor that does not match what the producer prints refuses a healthy run.
#    12    an MSTest 'Test Run Aborted' line's column is set by the runner, not by this script.
#    13    the arm-name mentions are a READING: zeros are normal on a passing MSTest run.
#    14    ⚠ THE MEASURED DUAL-FORM SITE.  The runner prints NOT MEASURED in count lines as well as in
#          rows; it is LEFT because its only consumer is a STAMP -- it is never asserted -- and the
#          dual form is recorded here so the next derive meets it rather than discovering it.
#    15-17 readings, stamped and never asserted.
WL_LAND="PRERES=\$(grep -ac 'PRE-RESOLVED MERGE COMMITTED'
WX_N=\$(grep -acE -- \"\$WRAPEXIT_PAT\"
WX_ANY=\$(grep -ac 'assembly exit='
grep -aq \"union BLOB == that seat's BLOB\""
#     the record's stamps are '[<ts>] <text>', so a ^ anchor would have to carry the timestamp shape
#     and would then assert the stamp FORMAT rather than the verdict;
#     WX_ANY is DELIBERATELY the unanchored SECOND reading of the anchored WX_N and the gate requires
#     the two to AGREE -- converting it would collapse two readings into one.
#     \u26a0 THE REFUSAL-SCAN AND EXIT-SCAN LINES ARE **NOT** ON THIS LIST AND THAT IS A MEASUREMENT, NOT AN
#     OMISSION: the detector reads the LAST grep on a line, and on both of those the last grep is
#     `grep -ac .` -- a pattern, not a phrase -- so they are out of scope by construction rather than
#     by exemption.  Recorded here so the next derive meets the detector's rule instead of rediscovering it.
#     ⚠ THE FOURTH ENTRY IS **NEW IN TRAIN 48** AND IT ARRIVED BY INHERITANCE.  Train 47's land grew an
#     A7-ADMISSION READER on 2026-09-13 (run 8 refused a landing whose seven modified goldens its own
#     assembly had admitted), and that reader's second grep is an unanchored phrase over the RECORD.  It is
#     LEFT unanchored for the same reason as the two above -- the record's stamps carry a '[<ts>] ' prefix,
#     so a ^ anchor would assert the stamp FORMAT -- and it is on the list rather than exempted silently,
#     which is the whole point of a whitelist whose LENGTH is asserted against the site count.
WL_DRY="grep -aqF -- \"\$w\" \"\$ASM\" || bad=
if grep -aqF -- \"\$bad\" \"\$ASM\"; then say"
#     both read the ASSEMBLY SCRIPT's own text rather than a tool's verdict, and the second is arm A's
#     own NEGATIVE CONTROL -- a control that anchored its lookup would stop being one.
BBAD=0
wl_check(){ # $1 = file  $2 = the whitelist
  local f="$1" wl="$2" n=0 miss=0 want line txt
  [ -f "$f" ] || { say "   FAIL: [$f] is absent"; BBAD=1; return; }
  want=$(printf '%s\n' "$wl" | grep -ac .)
  while IFS="$(printf '\t')" read -r line txt; do
    [ -n "${line:-}" ] || continue
    n=$(( n + 1 ))
    if [ "$(printf '%s\n' "$wl" | while IFS= read -r w; do [ -n "$w" ] || continue; case "$txt" in *"$w"*) printf 'x' ;; esac; done | grep -ac x || true)" = "0" ]; then
      say "   FAIL NEW unanchored site :: $(basename "$f"):$line :: $(printf '%s' "$txt" | sed 's/^[[:space:]]*//' | cut -c1-150)"
      miss=$(( miss + 1 ))
    fi
  done <<< "$(unanchored_sites "$f")"
  say "   $(basename "$f") :: in-scope unanchored sites=$n :: whitelist entries=$want :: NOT on the whitelist=$miss"
  { [ "$miss" = "0" ] && [ "$n" = "$want" ]; } || BBAD=1
  [ "$n" = "$want" ] || say "   FAIL: the site count ($n) and the whitelist length ($want) disagree -- a site was added or removed and the whitelist was not re-read"
}
wl_check "$F" "$WL_ASM"
wl_check "$SELFDIR/coord-train48-land.sh" "$WL_LAND"
wl_check "$SELFDIR/coord-train48-land-dryread.sh" "$WL_DRY"
# the RED control: a planted line of exactly the forbidden shape must be SEEN by the detector.
PLANTB="/tmp/${TAG}-unanchored-control.sh"
printf '%s\n' 'PLANTED=$(grep -ac "0 DUPLICATE patch-id(s)" "$L5" || true)' > "$PLANTB"
c=$(unanchored_sites "$PLANTB" | grep -ac . || true)
if [ "${c:-0}" -ge 1 ]; then say "   ok   CONTROL :: the detector FIRES on a planted unanchored verdict match over a tool log ($c)"
else say "   FAIL CONTROL: the detector did not fire on a planted site, so every zero above says nothing"; BBAD=1; fi
rm -f "$PLANTB"
[ "$BBAD" = "0" ] && say "   PASS: every in-scope unanchored site is on the written whitelist, the counts agree, and the detector was SHOWN to fire" || FAIL=1

say ""
say "== (c) the SEAT TABLE parser: stack-on= is ACCEPTED and the three bad shapes REFUSE =="
say "   ⚠ A VALIDATOR THAT IS ONLY EVER RUN ON THE REAL TABLE IS A VALIDATOR WHOSE REFUSALS HAVE NEVER"
say "     BEEN SEEN TO FIRE.  The functions below are EXTRACTED FROM THE ASSEMBLY and run over planted"
say "     tables, so what is exercised is the live definition rather than a retyped copy."
stamp(){ say "        $*"; }
eval "$(blk '^SEAT_OPT_KEYS=' '^SEAT_OPT_KEYS=' 'SEAT_OPT_KEYS')" || exit 3
eval "$(blk '^seat_opt\(\)' '^\}$' 'seat_opt')" || exit 3
eval "$(blk '^seat_row\(\)' '^seat_row\(\)' 'seat_row')" || exit 3
eval "$(blk '^seat_opts_validate\(\)' '^\}$' 'seat_opts_validate')" || exit 3
eval "$(blk '^seat_class_placeholders\(\)' '^\}$' 'seat_class_placeholders')" || exit 3
CBAD=0
armc(){ # $1 label  $2 planted table  $3 wanted rc  $4 a fragment the refusal must carry ('' = none)
  local out rc
  out=$(SEAT_TABLE="$2"; seat_opts_validate 2>&1); rc=$?
  if [ "$rc" != "$3" ]; then
    say "   FAIL $1 :: the validator exited $rc where $3 was wanted"; printf '%s\n' "$out" | sed 's/^/        /'; CBAD=1; return
  fi
  if [ -n "$4" ]; then
    case "$out" in
      *"$4"*) say "   ok   $1 :: refused (exit $rc) AND named the reason [$4]" ;;
      *) say "   FAIL $1 :: refused with exit $rc but did NOT name the reason [$4] -- a refusal nobody can read is a refusal nobody acts on"; printf '%s\n' "$out" | sed 's/^/        /'; CBAD=1 ;;
    esac
  else
    say "   ok   $1 :: ACCEPTED (exit $rc)"
  fi
}
armc 'stack-on naming an EARLIER row' "1|claude/a|111111111|docs|tip
2|claude/b|222222222|docs|tip|stack-on=1" 0 ''
armc 'stack-on naming a MISSING row' "1|claude/a|111111111|docs|tip
2|claude/b|222222222|docs|tip|stack-on=9" 1 'exists in this table'
armc 'stack-on naming a LATER row' "1|claude/a|111111111|docs|tip|stack-on=2
2|claude/b|222222222|docs|tip" 1 'merges AFTER it'
armc 'stack-on naming ITSELF' "1|claude/a|111111111|docs|tip
2|claude/b|222222222|docs|tip|stack-on=2" 1 'stacked on ITSELF'
armc 'an UNKNOWN option key' "1|claude/a|111111111|docs|tip|stacked-on=1" 1 'unknown option key'
armc 'an optional field that is not key=value' "1|claude/a|111111111|docs|tip|[a-z]+\.cs\$" 1 'not <key>=<value>'
# and the allowed= reader, over a row that carries BOTH keys in EITHER order.
for tbl in "3|claude/c|333333333|docs|tip|allowed=^x/y\.cs\$|stack-on=1" "3|claude/c|333333333|docs|tip|stack-on=1|allowed=^x/y\.cs\$"; do
  SEAT_TABLE="1|claude/a|111111111|docs|tip
2|claude/b|222222222|docs|tip
$tbl"
  got=$(seat_opt "$(seat_row 3)" allowed)
  sot=$(seat_opt "$(seat_row 3)" stack-on)
  if [ "$got" = '^x/y\.cs$' ] && [ "$sot" = "1" ]; then say "   ok   both keys read ORDER-FREE from row 3 :: allowed=[$got] stack-on=[$sot]"
  else say "   FAIL: the optional keys are read POSITIONALLY :: allowed=[$got] (want ^x/y\\.cs\$) stack-on=[$sot] (want 1)"; CBAD=1; fi
done
# and a FILLED row carrying the class placeholder must be REPORTED by the reader the assembly consumes.
SEAT_TABLE="1|claude/a|111111111|PENDING-class|tip
2|claude/b|PENDING|PENDING-class|tip"
pc=$(seat_class_placeholders | tr '\n' ' ')
if [ "$(printf '%s' "$pc" | tr -d ' ')" = "1" ]; then say "   ok   the class placeholder is reported for the FILLED row only :: [$pc]"
else say "   FAIL: seat_class_placeholders read [$pc] where only the FILLED row should be reported"; CBAD=1; fi
unset -f stamp
[ "$CBAD" = "0" ] && say "   PASS: stack-on= is accepted, the three bad shapes and two malformed fields REFUSE BY NAME, and both keys are order-free" || FAIL=1

say ""
say "== (d) the PATCH-ID ARM's four branches are present in the assembly, each by its STAMP =="
say "   ⚠ THE BRANCH THAT MATTERS MOST IS THE ONE THAT CANNOT RUN: an arm whose tool is missing must"
say "     set FAILED, because an instrument that could not run has not found nothing."
say "   ⚠ THE PREDICATE IS A **FUNCTION OVER A FILE**, so the arm and its control are the same code."
say "     Its first cut read \$F directly and had no in-run control at all: its reds were shown once,"
say "     by hand, against a planted copy that is not reproducible from the shipped instrument.  An arm"
say "     whose control lives in a note is an assertion wearing a measurement's clothes."
d_defects(){ # $1 = file -> one "ok<TAB>..." or "FAIL<TAB>..." line per branch and per anchored token
  local f="$1" pair a b ca cb tok c
  for pair in \
    'PATCH-ID ARM UNMEASURED ::|fail_gate PATCH-ID-ARM-UNMEASURED' \
    'PATCH-ID ARM self-test exit=|fail_gate PATCH-ID-ARM-SELFTEST' \
    'PATCH-ID ARM CLEAN ::|==> CENSUS (CLEAN|RED)' \
    'PATCH-ID ARM REFUSED :: exit=1|fail_gate PATCH-ID-ARM' ; do
    a="${pair%%|*}"; b="${pair#*|}"
    ca=$(grep -acF -- "$a" "$f" || true); cb=$(grep -acF -- "$b" "$f" || true)
    if [ "${ca:-0}" -ge 1 ] && [ "${cb:-0}" -ge 1 ]; then printf 'ok\t[%s] x%s with [%s] x%s\n' "$a" "$ca" "$b" "$cb"
    else printf 'FAIL\tthe patch-id branch [%s] (x%s) or its consequence [%s] (x%s) is absent\n' "$a" "${ca:-0}" "$b" "${cb:-0}"; fi
  done
  for tok in '^DUPLICATE patch-id ' '^UNDECLARED STACK: ' '^SELF-TEST CLEAN -- ' '^declared-stack SHA ' 'seat-duplication-census.sh' '--self-test' '--stack'; do
    c=$(grep -acF -- "$tok" "$f" || true)
    if [ "${c:-0}" -ge 1 ]; then printf 'ok\tthe arm reads [%s] x%s\n' "$tok" "$c"
    else printf 'FAIL\t[%s] is absent -- the arm would read its tool'"'"'s verdict as a SUBSTRING, or would not run the tool'"'"'s own controls\n' "$tok"; fi
  done
}
DBAD=0
while IFS="$(printf '\t')" read -r v m; do
  [ -n "${v:-}" ] || continue
  if [ "$v" = "ok" ]; then say "   ok   $m"; else say "   FAIL: $m"; DBAD=1; fi
done <<< "$(d_defects "$F")"
# --- THE RED CONTROL, IN-RUN.  A COPY of the assembly with the CLEAN stamp renamed out of it must be
#     SEEN, or every ok above says only that this arm can read a file.
PLANTD="/tmp/${TAG}-patchid-control.sh"
sed 's/PATCH-ID ARM CLEAN ::/PATCH-ID ARM DELETED-BY-PLANT ::/' "$F" > "$PLANTD"
DCTL=$(d_defects "$PLANTD" | grep -ac '^FAIL' || true)
if [ "${DCTL:-0}" -ge 1 ]; then
  say "   ok   CONTROL :: with the CLEAN stamp renamed out of a COPY of the assembly the predicate reports ${DCTL} absence(s) -- the arm CAN fire"
  while IFS= read -r l; do [ -n "$l" ] && say "        CONTROL red: $l"; done <<< "$(d_defects "$PLANTD" | grep -a '^FAIL' | head -2 | sed 's/^FAIL\t//' | cut -c1-150)"
else say "   FAIL CONTROL: the predicate found nothing wrong with a COPY whose CLEAN stamp was renamed away, so its zeros say nothing"; DBAD=1; fi
rm -f "$PLANTD"
[ "$DBAD" = "0" ] && say "   PASS: UNMEASURED->FAILED, the self-test gate, the CLEAN stamp and the named refusal are all present, every verdict read is anchored, and the predicate was SHOWN to fire" || FAIL=1

say ""
say "== (e) the LAUNCH WRAPPER exports TRAIN48_REQUIRE_ALL and ends in 'exit \$rc' =="
say "   ⚠ A WRAPPER WHOSE LAST STATEMENT IS A PIPE REPORTS THE LAST COMMAND'S STATUS, so a script that"
say "     exited 1 is announced as 0.  And the switch must be EXPORTED rather than one-shot prefixed: a"
say "     nohup/start layer can drop a one-shot prefix between the assignment and the child."
LW="$SELFDIR/launch-run1.sh"
EBAD=0
# ⚠ THE PREDICATE IS A FUNCTION OVER A FILE, for the same reason (d)'s is: its reds were shown once by
#   hand against a planted wrapper and were not reproducible from the shipped instrument.
e_defects(){ # $1 = file -> a READING line, then one "FAIL<TAB>..." per defect
  local w="$1" exp last rcc
  exp=$(grep -acE '^[[:space:]]*export TRAIN48_REQUIRE_ALL=1[[:space:]]*$' "$w" || true)
  last=$(grep -av '^[[:space:]]*$' "$w" | tail -1 | tr -d '\r')
  rcc=$(grep -acE '; rc=\$\?' "$w" || true)
  printf 'READ\texport TRAIN48_REQUIRE_ALL=1 lines=%s (want 1) :: rc=$? capture lines=%s (want 1) :: last non-empty line=[%s]\n' "${exp:-0}" "${rcc:-0}" "$last"
  [ "${exp:-0}" = "1" ] || printf 'FAIL\tthe switch is not EXPORTED on its own line\n'
  [ "${rcc:-0}" -ge 1 ] || printf 'FAIL\tthe wrapper does not capture rc=$? as the first statement after the run\n'
  case "$last" in
    'exit $rc') printf 'ok\tthe wrapper ends in '"'"'exit $rc'"'"'\n' ;;
    *) printf 'FAIL\tthe wrapper'"'"'s last statement is [%s], not '"'"'exit $rc'"'"'\n' "$last" ;;
  esac
}
if [ ! -f "$LW" ]; then say "   FAIL: [$LW] is absent -- the launch shape is asserted on a file that does not exist"; EBAD=1
else
  while IFS="$(printf '\t')" read -r v m; do
    [ -n "${v:-}" ] || continue
    case "$v" in
      READ) say "   $m" ;;
      ok)   say "   ok   $m" ;;
      *)    say "   FAIL: $m"; EBAD=1 ;;
    esac
  done <<< "$(e_defects "$LW")"
  # --- THE RED CONTROL, IN-RUN: a planted wrapper that one-shot-prefixes the switch and ends in a tail.
  PLANTE="/tmp/${TAG}-launch-control.sh"
  { printf '%s\n' 'TRAIN48_REQUIRE_ALL=1 bash coord-train48-assemble-run1.sh > out.stdout 2>&1'
    printf '%s\n' 'tail -40 out.stdout'; } > "$PLANTE"
  ECTL=$(e_defects "$PLANTE" | grep -ac '^FAIL' || true)
  if [ "${ECTL:-0}" -ge 3 ]; then
    say "   ok   CONTROL :: a planted wrapper that one-shot-prefixes the switch and ends in a tail reads ${ECTL} defect(s) -- the arm CAN fire on all three"
    while IFS= read -r l; do [ -n "$l" ] && say "        CONTROL red: $l"; done <<< "$(e_defects "$PLANTE" | grep -a '^FAIL' | sed 's/^FAIL\t//' | cut -c1-150)"
  else say "   FAIL CONTROL: the predicate reads ${ECTL:-0} defect(s) on a wrapper carrying all three, so its greens above say nothing"; EBAD=1; fi
  rm -f "$PLANTE"
fi
[ "$EBAD" = "0" ] && say "   PASS: the launch wrapper exports the switch and exits on the captured status, and the predicate was SHOWN to fire" || FAIL=1

say ""
say "== (f) NO 'train47' TOKEN SURVIVES OUTSIDE A PROVENANCE COMMENT =="   # [t47-control]
say "   ⚠ A PROVENANCE COMMENT IS A COMMENT LINE THAT NAMES THE TEMPLATE FILE.  Keeping those is the"
say "     point: a derive that scrubbed its own parentage would delete the record of what this file was"
say "     derived FROM, which is the one fact a re-derive needs."
t47_live(){ # $1 = file -> lines carrying `train47` that are NOT provenance comments  [t47-control]
  awk '{ l=$0
         if (index(l, "train47") == 0) next   # [t47-control]
         s=l; sub(/^[ \t]+/, "", s)
         if (substr(s,1,1) == "#" && index(l, "coord-train47") > 0) next   # a provenance comment names the template  [t47-control]
         if (index(l, "[t47-control]") > 0) next                              # an explicitly MARKED control-wiring line
         printf "%d\t%s\n", NR, l }' "$1"
}
FBAD=0
for f in "$F" "$SELFDIR/coord-train48-rehearse.sh" "$SELFDIR/coord-train48-land.sh" "$SELFDIR/coord-train48-land-dryread.sh" "$SELFDIR/launch-run1.sh" "${BASH_SOURCE[0]}"; do
  [ -f "$f" ] || { say "   FAIL: [$f] is absent"; FBAD=1; continue; }
  n=$(t47_live "$f" | grep -ac . || true)
  prov=$(grep -acF -- 'coord-train47' "$f" || true)   # [t47-control]
  if [ "${n:-0}" = "0" ]; then say "   ok   $(basename "$f") :: live train47 tokens=0 (provenance mentions=$prov)"   # [t47-control]
  else say "   FAIL $(basename "$f") :: $n live train47 token(s):"; t47_live "$f" | head -6 | cut -c1-170 | sed 's/^/        /'; FBAD=1; fi   # [t47-control]
done
if [ -f "$T47ASM" ]; then
  c=$(t47_live "$T47ASM" | grep -ac . || true)
  if [ "${c:-0}" -ge 1 ]; then say "   ok   CONTROL (train-47 assembly) :: the same predicate reads $c live token(s) -- the arm CAN fire"
  else say "   FAIL CONTROL: the predicate reads 0 on the train-47 original, so its zeros above say nothing"; FBAD=1; fi
else
  say "   FAIL CONTROL UNMEASURED :: [$T47ASM] is absent (override with T47DIR=<dir>)"; FBAD=1
fi
[ "$FBAD" = "0" ] && say "   PASS: no live train47 token survives, provenance comments are kept, and the predicate was SHOWN to fire" || FAIL=1   # [t47-control]

say ""
say "== (g) THE FOUR FILL POINTS REFUSE AT LAUNCH, each BY NAME =="
say "   ⚠ 'A GATE REFUSED' AND 'A GATE COULD NOT BE CONFIGURED' ARE DIFFERENT STATES, so each refusal"
say "     names its own fill point.  F2 and F4 refuse BEFORE the run lock is taken -- a placeholder met"
say "     three hundred lines and one lock into the run has already cost the operator what a refusal is for."
say "   ⚠ THE PREDICATE IS A FUNCTION OVER A FILE, AND IT HAS **TWO INDEPENDENT BRANCHES** -- the"
say "     refusal-and-reason presence check and the refusal-BEFORE-the-lock ORDER check.  Its first cut"
say "     read \$F directly and had no control at all, which is the one state this whole arm exists to"
say "     forbid: an arm that has never been made to fire is an assertion wearing a measurement's clothes."
GBAD=0
CP=$(grep -aE "^T48_CONTAIN_PIN='" "$F" | head -1 | sed -E "s/^T48_CONTAIN_PIN='([^']*)'.*/\1/")
G3E=$(grep -aE "^EXPECT_G3='" "$F" | head -1 | sed -E "s/^EXPECT_G3='([^']*)'.*/\1/")
say "   T48_CONTAIN_PIN=[${CP:-ABSENT}] (F2) :: EXPECT_G3=[${G3E:-ABSENT}] (F4) :: WT fill point and SEAT_TABLE are F1 and F3"
g_defects(){ # $1 = file -> "ok/READ/FAIL<TAB>..." per fill point, then the ORDER branch
  local f="$1" pair a b ca cb fpo lk fp
  for pair in 'FILL POINT F2 REFUSED|the containment pin is not filled' 'FILL POINT F4 REFUSED|census expectation is not filled' 'WORKTREE UNFILLED ::|name the assembly worktree' ; do
    a="${pair%%|*}"; b="${pair#*|}"
    ca=$(grep -acF -- "$a" "$f" || true); cb=$(grep -acF -- "$b" "$f" || true)
    if [ "${ca:-0}" -ge 1 ] && [ "${cb:-0}" -ge 1 ]; then printf 'ok\t[%s] x%s names its reason [%s] x%s\n' "$a" "$ca" "$b" "$cb"
    else printf 'FAIL\tthe refusal [%s] (x%s) or its reason [%s] (x%s) is absent\n' "$a" "${ca:-0}" "$b" "${cb:-0}"; fi
  done
  fpo=$(grep -acF -- 'FILL POINTS OK ::' "$f" || true)
  lk=$(grep -nF -- 'LOCK="/tmp/' "$f" | head -1 | cut -d: -f1)
  fp=$(grep -nF -- 'FILL POINTS UNFILLED' "$f" | head -1 | cut -d: -f1)
  printf 'READ\tthe positive stamp '"'"'FILL POINTS OK ::'"'"' x%s (want 1) :: the fill-point refusal is at line %s, the run lock at line %s (the refusal must come FIRST)\n' "${fpo:-0}" "${fp:-none}" "${lk:-none}"
  { [ "${fpo:-0}" = "1" ] && [ -n "${fp:-}" ] && [ -n "${lk:-}" ] && [ "$fp" -lt "$lk" ]; } || printf 'FAIL\tthe fill points are not both stamped and refused BEFORE the run lock is taken\n'
}
while IFS="$(printf '\t')" read -r v m; do
  [ -n "${v:-}" ] || continue
  case "$v" in
    READ) say "   $m" ;;
    ok)   say "   ok   $m" ;;
    *)    say "   FAIL: $m"; GBAD=1 ;;
  esac
done <<< "$(g_defects "$F")"
# --- THE RED CONTROLS, IN-RUN, ONE PER BRANCH.  ⚠ TWO PLANTS, BECAUSE TWO BRANCHES: a single plant
#     that reddened both would leave either one able to hide behind the other.
PLANTG1="/tmp/${TAG}-fillpoint-reason-control.sh"
sed 's/the containment pin is not filled/the containment pin is REDACTED-BY-PLANT/' "$F" > "$PLANTG1"
G1CTL=$(g_defects "$PLANTG1" | grep -ac '^FAIL' || true)
PLANTG2="/tmp/${TAG}-fillpoint-order-control.sh"
{ printf '%s\n' 'LOCK="/tmp/planted-early.lock"'; cat "$F"; } > "$PLANTG2"
G2CTL=$(g_defects "$PLANTG2" | grep -ac '^FAIL' || true)
if [ "${G1CTL:-0}" -ge 1 ] && [ "${G2CTL:-0}" -ge 1 ]; then
  say "   ok   CONTROL (reason branch) :: a COPY with F2's reason text redacted reads ${G1CTL} defect(s) -- that branch CAN fire"
  say "   ok   CONTROL (order branch)  :: a COPY with the run lock moved to line 1 reads ${G2CTL} defect(s) -- that branch CAN fire"
  while IFS= read -r l; do [ -n "$l" ] && say "        CONTROL red: $l"; done <<< "$( { g_defects "$PLANTG1"; g_defects "$PLANTG2"; } | grep -a '^FAIL' | sed 's/^FAIL\t//' | cut -c1-150)"
else say "   FAIL CONTROL: the reason branch reads ${G1CTL:-0} and the order branch ${G2CTL:-0} on their own planted copies, so the greens above say nothing"; GBAD=1; fi
rm -f "$PLANTG1" "$PLANTG2"
[ "$GBAD" = "0" ] && say "   PASS: every fill point refuses by name, before the lock, and BOTH branches were SHOWN to fire" || FAIL=1

say ""
[ "${OFFLINE_UNMEASURED:-0}" = "0" ] || say "⚠ OFFLINE :: $OFFLINE_UNMEASURED pin(s) could not be resolved because T48_SELFCHECK_OFFLINE=1 forbade the fetch.  They are counted as FAILURES above and they are UNMEASURED, not findings: re-run without the switch, outside any freeze, to turn them into measurements."
say "=== SELF-CHECK DONE overallFail=$FAIL offlineUnmeasured=${OFFLINE_UNMEASURED:-0} ==="
exit $FAIL
'''
S.block_to_eof(r'^\[ "\$RQMISS" = "0" \] && say', SCNEW, 'sc-new-arms', start_off=1)
S.absent_in_code('EXPECT_46', 'sc-no-expect46')
# ⚠ THE SELF-CHECK IS THE ONE FILE THAT MAY NOT BE HELD TO THE `grep -q`/`pipefail` ABSENCE: its
#   arm-12 predicates ARE the detectors for those shapes, so a blanket absence check would refuse
#   the instrument for carrying the thing it detects.  What is asserted instead is that the
#   detectors are still THERE -- an absence check turned inside out.
# ---- (F7) THE RENAME CORRUPTED TWO RUN RECORDS.  LD1 and LD2 are LESSONS, and a lesson names the run
#      that taught it.  Train 47 run 4 happened and has a log on disk; train 48 has never run, so
#      "Train 48 run 4 predicted ..." is a measurement attributed to a battery that does not exist.
S.gsub('Train 48 run 4', 'Train 47 run 4', 'f7-sc-run-record', minimum=2, maximum=2)
S.absent('Train 48 run 4', 'f7-sc-no-invented-run-record')
# ---- (F7/LR1) A THIRD RUN RECORD, ADDED TO THE TEMPLATE AFTER THIS DERIVE WAS FIRST CUT.  Train 47
#      gained lesson LR1 on 2026-09-13 (the LEG R / LEG U restores now name docs/validation), and its
#      provenance sentence cites the run that MEASURED the defect: train 47 run 5 read LEG U's
#      post-restore tree dirty=2.  The blanket rename re-attributes that measurement to train 48,
#      which has never run -- the same corruption class as the two run records above.  Restored BY
#      NAME and guarded by its own absence check, because the guard is what makes a RE-RUN of the
#      rename unable to re-introduce it; a fix without the guard is a hand patch the next derive undoes.
S.gsub('Train 48 run 5', 'Train 47 run 5', 'f7-sc-run-record-lr1', minimum=1, maximum=1)
S.absent('Train 48 run 5', 'f7-sc-no-invented-run-record-lr1')
# ---- (F7/LA1) A FOURTH RUN RECORD, ADDED TO THE TEMPLATE AFTER §17'S DERIVE.  Train 47 gained lesson
#      LA1 on 2026-09-13 (no FAILED=1 setter under a stamp that says 'never fatal'), and its
#      provenance sentence cites the run that MEASURED the defect: train 47 run 6 ended
#      overallFailed=1 with every leg green because LEG 4 stamped the advisory count 'counted, never
#      fatal' and set FAILED=1 on the next line.  The blanket rename re-attributes that measurement to
#      train 48, which has never run -- the same corruption class as the three run records above.
#      Restored BY NAME and guarded by its own absence check, for the reason §17.1 gives: the guard is
#      what makes a RE-RUN of the rename unable to re-introduce it.
S.gsub('Train 48 run 6', 'Train 47 run 6', 'f7-sc-run-record-la1', minimum=1, maximum=1)
S.absent('Train 48 run 6', 'f7-sc-no-invented-run-record-la1')

# ---- (F7/LB1+LC1) A FIFTH AND SIXTH RUN RECORD, ADDED TO THE TEMPLATE AFTER §18.7'S DERIVE.  Train
#      47's LANDING of run 8 refused twice on its own instrument and the self-check grew TWO lessons
#      from it: LC1 (the read-back compared a 9-char ls-remote cut against a 10-char --short HEAD and
#      declared a LANDED push NOT LANDED) and LB1 (the modified-behavioral gate was unconditional and
#      refused seven goldens the assembly's own A7 had admitted by a seat's allowed= ruling).  BOTH
#      provenance sentences cite `Train 47 run 8`, and the blanket rename re-attributes both to a
#      train 48 that has never run -- the same corruption class as the four run records above.  ONE
#      gsub covers both (`minimum=2, maximum=2` is what asserts that it is exactly both), and the
#      `absent()` is what makes a RE-RUN of the rename unable to re-introduce them.
S.gsub('Train 48 run 8', 'Train 47 run 8', 'f7-sc-run-record-lb1lc1', minimum=2, maximum=2)
S.absent('Train 48 run 8', 'f7-sc-no-invented-run-record-lb1lc1')
S.require("l9_count(){", 'sc-keeps-l9-detector')
S.require("l4_count(){", 'sc-keeps-l4-detector')
S.require("l11_count(){", 'sc-keeps-l11-detector')

# ============================================================================================
# D6 -- **THE SELF-CHECK'S OWN CONTROLS FOR THE D5 RULING** (COORD 2026-09-13 19:05).
#       Safety-floor item 13: a gate that has never been made to fail proves nothing.  Four arms,
#       each with the control that makes its green worth reading.
# ============================================================================================

# ---- ⚠ **THE SEAT-TABLE END ANCHOR, RETARGETED FOR THE SAME REASON AS THE REHEARSAL'S.**  The
#      self-check extracts SEAT_TABLE by `blk` between `^SEAT_TABLE="1\|` and `\|tip"$`.  Train 48's
#      LAST row ends `...|tip|allowed=...)?$"`, so the old end anchor matches NO line, blk emits its
#      abort statement and the whole self-check exits 3 -- an instrument that cannot read the table it
#      is checking.  Retargeted to the CLOSING QUOTE, which is the thing it was naming all along.
S.subline_contains("""eval "$(blk '^SEAT_TABLE="1\\|' '\\|tip"$' 'SEAT_TABLE')" || exit 3""",
                   """eval "$(blk '^SEAT_TABLE="1\\|' '"$' 'SEAT_TABLE')" || exit 3""",
                   'sc-table-end-anchor')

# ---- ⚠ **ARM 3 WAS TIP-MODE-BLIND, AND D4 MADE THAT A FALSE SENTENCE.**  Train 47's arm said
#      "merge_seat will ABORT with this diagnosis" of EVERY branch grown past its pin.  That is true
#      of a `tip` row and FALSE of a `sha` row: SHA mode exists precisely to pin BEHIND a tip and to
#      STAMP what is deliberately not carried.  Row 4 is such a row now (COORD D4), and an instrument
#      that announces an abort the assembly will not perform sends a reader to fix a healthy table --
#      the false-red shape this repository names by a different route every few trains.
S.subline_contains("""      say "   seat $n ($c) $b :: ⚠ THE BRANCH GREW -- tip is $tip, $ahead commit(s) BEYOND the pin $s, which IS an ancestor.  merge_seat will ABORT with this diagnosis, which is the mechanism WORKING.  The added commit(s):\"""",
                   """      case "$m" in
        sha) say "   seat $n ($c, SHA mode) $b :: tip is $tip, $ahead commit(s) BEYOND the pin $s, which IS an ancestor -- **DELIBERATELY NOT CARRIED**.  merge_seat will PROCEED and STAMP the excluded commit(s) BY NAME; a SHA-mode row is a row pinned BEHIND its tip on purpose.  The excluded commit(s):" ;;
        *)   say "   seat $n ($c, tip mode) $b :: ⚠ THE BRANCH GREW -- tip is $tip, $ahead commit(s) BEYOND the pin $s, which IS an ancestor.  merge_seat will ABORT with this diagnosis, which is the mechanism WORKING.  The added commit(s):" ;;
      esac""",
                   'sc-arm3-tipmode-aware')

# ---- (d) ARM 8 KEEPS `allowedByRuling=N` -- asserted as a KEPT token rather than assumed.
S.require('allowedByRuling=', 'sc-arm8-keeps-allowedbyruling')

# ---- ⚠ ARM 8 READ THE RULING **POSITIONALLY**, AND WITH TWO OPTIONAL KEYS THAT IS A WIDENING.
S.subline_contains("""    alw=''; case "${a:-}" in allowed=*) alw="${a#allowed=}";; esac""",
                   """    # ⚠ READ THE RULING AS A key=value FIELD, NEVER POSITIONALLY (COORD 2026-09-13 19:05, D6).  With
    #   two optional keys `read -r n b s c m a` puts fields SIX AND BEYOND into `a` DELIMITERS AND ALL,
    #   so `${a#allowed=}` yields the ERE with `|stack-on=NN` WELDED ONTO IT -- an ALTERNATION nobody
    #   wrote, which widens the exemption to anything matching that literal and makes the ruling's
    #   named paths un-enumerable.  Row 13 of this table carries exactly that shape.
    alw=$(printf '%s\\n' "$SEAT_TABLE" | awk -F'|' -v r="$n" '$1==r{for(i=6;i<=NF;i++) if ($i ~ /^allowed=/) {print substr($i,9); exit}}')""",
                   'sc-arm8-allowed-keyvalue')

# ---- the allowed= ROW COUNT scans fields 6..NF for the same reason.
S.subline_contains("""ALLOWR=$(printf '%s\\n' "$SEAT_TABLE" | awk -F'|' '$6 ~ /^allowed=/' | grep -ac . || true)""",
                   """ALLOWR=$(printf '%s\\n' "$SEAT_TABLE" | awk -F'|' '{for(i=6;i<=NF;i++) if ($i ~ /^allowed=/) {print $1; break}}' | grep -ac . || true)""",
                   'sc-allowr-keyvalue')

SC_ALW = r'''
say ""
say "== (e) merge_seat HONOURS THE PER-ROW allowed= RULING -- STATIC, WITH A REGRESSED COPY AS ITS RED CONTROL =="
say "   COORD ruling 2026-09-13 19:05 (D5).  Train 47's merge_seat consulted the CLASS alone: the"
say "   forbidden arm and the shape arm never read allowed=, and only A7 did -- scoped to"
say "   src/tests/Behavioral.  Four rows of this table rule paths OUTSIDE that scope, and every one of"
say "   them would have ABORTED at assembly on its own class while the ruling at the row read as prose."
alw_arm(){ # $1 = a script -> "forbiddenArmFilters shapeArmFilters identityBlocks a7bArms"
  printf '%s %s %s %s' \
    "$(grep -acF -- 'diff --name-only "$mb" "$want" -- $forbidden | alw_filter "$alw"' "$1" || true)" \
    "$(grep -acF -- 'diff --name-only "$mb" "$want" | alw_filter "$alw"' "$1" || true)" \
    "$(grep -acF -- 'ALLOWED-IDENTITY-AFTER-MERGE' "$1" || true)" \
    "$(grep -acF -- 'fail_gate A7b-allowed-identity' "$1" || true)"
}
PLANTH="/tmp/${TAG}-allowed-filter-control.sh"
# ⚠ THE CONTROL IS A COPY WITH THE FILTER **SED'd OUT**, i.e. exactly train 47's shape.  A control
#   built by any other edit would be reddening something else and calling it this arm.
sed 's/ | alw_filter "$alw"//g' "$F" > "$PLANTH"
read -r AF AS AI AB <<EOF
$(alw_arm "$F")
EOF
read -r RF RS RI RB <<EOF
$(alw_arm "$PLANTH")
EOF
say "   derived   :: forbiddenArmFilters=$AF (want >= 2) shapeArmFilters=$AS (want >= 2) identityBlocks=$AI (want >= 1) a7bArms=$AB (want >= 1)"
say "   REGRESSED :: forbiddenArmFilters=$RF shapeArmFilters=$RS identityBlocks=$RI a7bArms=$RB"
if [ "${AF:-0}" -ge 2 ] && [ "${AS:-0}" -ge 2 ] && [ "${AI:-0}" -ge 1 ] && [ "${AB:-0}" -ge 1 ]; then
  say "   PASS: BOTH merge_seat readings filter through the seat's own allowed= ruling, the post-merge BLOB-IDENTITY block is present, and A7b is wired beside A7"
else
  say "   FAIL: merge_seat does NOT honour allowed= (forbiddenArmFilters=$AF want>=2, shapeArmFilters=$AS want>=2, identityBlocks=$AI want>=1, a7bArms=$AB want>=1).  A row whose ruling names a path outside its class would ABORT at assembly and the ruling would be decoration."
  FAIL=1
fi
if [ "${RF:-1}" = "0" ] && [ "${RS:-1}" = "0" ]; then
  say "   ok   CONTROL (regressed copy) :: with ' | alw_filter \"\$alw\"' sed'd out the SAME predicate reads forbiddenArmFilters=0 shapeArmFilters=0 and NAMES the regression -- a gate never made to fail proves nothing"
else
  say "   FAIL CONTROL: the regressed copy still reads forbiddenArmFilters=$RF shapeArmFilters=$RS, so the green above says nothing about the filter it claims to read"
  FAIL=1
fi
rm -f "$PLANTH"

say ""
# ⚠⚠ **SEAT_TABLE IS RE-EXTRACTED FROM THE FILE HERE, AND THAT IS A DEFECT THESE ARMS FOUND IN
#   THEMSELVES ON THEIR FIRST RUN.**  The arms above PLANT synthetic tables into this shell to
#   exercise the validators' refusals, and the last plant LEAVES the global SEAT_TABLE holding TWO
#   synthetic rows.  The first cut of the arms below read that global and reported `rows carrying an
#   allowed= ruling=0 :: STALE ruled paths=0` -- a PASS taken over a table nobody seated, which is the
#   vacuous-green shape exactly: every count zero, nothing refused, and the arm reads healthy.  The
#   table is re-read FROM THE FILE, which is the only authority, and the row count is STAMPED so a
#   reader can see WHICH table these arms measured rather than assuming.
eval "$(blk '^SEAT_TABLE="1\|' '"$' 'SEAT_TABLE (re-read for the allowed= arms)')" || exit 3
ALWTBLROWS=$(printf '%s\n' "$SEAT_TABLE" | grep -c .)
say "   the seat table was RE-READ from the file for arms (f) and (g) :: rows=$ALWTBLROWS -- the planted controls above leave the global holding SYNTHETIC rows"
# ⚠ AND THE RE-READ IS ITSELF CHECKED, against arm 2's INDEPENDENT reading of the same table.  A
#   re-read that silently returned the plant again would restore exactly the vacuous green this block
#   exists to remove, and nothing else in the run would say so.
if [ "${ALWTBLROWS:-0}" = "${ROWS:-0}" ] && [ "${ALWTBLROWS:-0}" -ge 1 ]; then
  say "   ok   the re-read table has $ALWTBLROWS rows, matching arm 2's independent reading of $ROWS -- arms (f) and (g) measure the SEATED table"
else
  say "   FAIL: the re-read table has ${ALWTBLROWS:-0} rows against arm 2's reading of ${ROWS:-0} -- arms (f) and (g) would be measuring a table nobody seated, and every count they print would be vacuous"
  FAIL=1
fi
say "== (f) EVERY allowed= RULING NAMES A PATH THE ROW ACTUALLY TOUCHES -- ONLINE, AGAINST THE REAL DIFF =="
say "   A ruled path the seat does not change exempts NOTHING and hides that it exempts nothing.  The"
say "   assembly refuses it as a STALE RULING at seat time; this arm reads it BEFORE the worktree exists."
say "   ⚠⚠ AND IT USES THE **ASSEMBLER'S OWN BASE RULE** (COORD E2, 2026-09-13 ~20:00).  merge_seat"
say "   measures a seat against merge-base(HEAD, want); by the time a row declaring stack-on=N merges,"
say "   HEAD ALREADY CARRIES ROW N, so the seat's measured diff is its OWN delta.  Reading a stacked row"
say "   against the TRAIN'S BASE instead credits it with every file its PARENT changed -- which is"
say "   precisely how one inherited ruling read LIVE in this arm and STALE at assembly."
# ⚠ THE PREDICATE IS A FUNCTION OVER A TABLE **GIVEN TO IT**, not a loop over the global, for one
#   reason: a predicate that can only read the live table can never be shown to FAIL, and safety-floor
#   item 13 says such a gate proves nothing.  The controls below hand it synthetic tables built from
#   REAL SHAs of this very train, so the red control reddens the arm ITSELF and not a copy of it.
stale_ruled(){ # $1 = a seat table.  stdout: tab-tagged ROW/PATH/ok/FAIL/UNM lines.  rc=1 if any STALE.
  local T="$1" sbad=0 n b s c m rest aw sob sobsha mbref mbwhy mbf files p
  while IFS='|' read -r n b s c m rest; do
    [ -n "$n" ] || continue
    [ "$s" = "PENDING" ] && continue
    aw=$(printf '%s\n' "$T" | awk -F'|' -v r="$n" '$1==r{for(i=6;i<=NF;i++) if ($i ~ /^allowed=/) {print substr($i,9); exit}}')
    [ -n "$aw" ] || continue
    printf 'ROW\t%s\n' "$n"
    if ! git -C "$G" cat-file -e "${s}^{commit}" 2>/dev/null; then
      printf 'UNM\trow %s (%s) :: the pin %s is not in this clone -- UNMEASURED, and an arm that could not run has not found nothing\n' "$n" "$b" "$s"; continue
    fi
    sob=$(printf '%s\n' "$T" | awk -F'|' -v r="$n" '$1==r{for(i=6;i<=NF;i++) if ($i ~ /^stack-on=/) {print substr($i,10); exit}}')
    mbref="$BASE_EXPECT"; mbwhy="the train's base"
    if [ -n "$sob" ]; then
      sobsha=$(printf '%s\n' "$T" | awk -F'|' -v r="$sob" '$1==r{print $3}')
      if [ -n "$sobsha" ] && [ "$sobsha" != "PENDING" ] && git -C "$G" cat-file -e "${sobsha}^{commit}" 2>/dev/null; then
        mbref="$sobsha"; mbwhy="row $sob's PINNED SHA $sobsha (DECLARED stack-on=$sob)"
      else
        printf 'UNM\trow %s (%s) :: declares stack-on=%s and row %s pins [%s], which does not resolve here -- the STACKED BASE is UNMEASURABLE and no reading is taken.  Falling back to the train base would credit this row with its PARENT files, which is the over-read this rule removes\n' "$n" "$b" "$sob" "$sob" "${sobsha:-ABSENT}"; continue
      fi
    fi
    mbf=$(git -C "$G" merge-base "$mbref" "$s" 2>/dev/null)
    files=$(git -C "$G" -c core.quotepath=false diff --name-only "$mbf" "$s")
    for p in $(printf '%s' "$aw" | sed 's/^\^//; s/\$$//; s/[()]//g' | tr '?' '\n' | sed 's/\\//g' | grep -a .); do
      printf 'PATH\t%s\n' "$p"
      if [ "$(printf '%s\n' "$files" | grep -acxF -- "$p" || true)" = "0" ]; then
        printf 'FAIL\trow %s (%s @%s) rules allowed= [%s] and its REAL diff against %s does NOT touch it -- a STALE RULING\n' "$n" "$b" "$s" "$p" "$mbwhy"; sbad=1
      else
        printf 'ok\trow %s (%s) allowed= [%s] IS in the real diff measured against %s\n' "$n" "$b" "$p" "$mbwhy"
      fi
    done
  done <<< "$T"
  return $sbad
}
SRO=$(stale_ruled "$SEAT_TABLE"); SRRC=$?
ALWROWS=$(printf '%s\n' "$SRO" | grep -ac '^ROW	' || true)
ALWPATHS=$(printf '%s\n' "$SRO" | grep -ac '^PATH	' || true)
ALWSTALE=$(printf '%s\n' "$SRO" | grep -ac '^FAIL	' || true)
ALWUNM=$(printf '%s\n' "$SRO" | grep -ac '^UNM	' || true)
while IFS= read -r l; do
  case "$l" in
    ROW*|PATH*|'') continue ;;
    FAIL*) say "   FAIL: $(printf '%s' "$l" | sed 's/^FAIL\t//')" ;;
    UNM*)  say "   FAIL (UNMEASURED): $(printf '%s' "$l" | sed 's/^UNM\t//')" ;;
    *)     say "   ok   $(printf '%s' "$l" | sed 's/^ok\t//')" ;;
  esac
done <<< "$SRO"
say "   arm (f) :: rows carrying an allowed= ruling=$ALWROWS :: ruled paths read=$ALWPATHS :: STALE ruled paths=$ALWSTALE (must be 0) :: rows UNMEASURED=$ALWUNM"
if [ "$SRRC" = "0" ] && [ "${ALWUNM:-0}" = "0" ]; then
  say "   PASS: every ruled path is in its row's real diff, and every stacked row was measured against THE BASE merge_seat WILL USE"
else
  say "   FAIL: a ruling is STALE or a row could not be read -- $ALWSTALE stale, $ALWUNM unmeasured"
  FAIL=1
fi

# ⚠ **THE CONTROLS FOR THE BASE RULE, BUILT FROM REAL SHAs OF THIS TRAIN** (COORD E2).  A synthetic
#   pair is DERIVED from the first row of this table that declares stack-on= and whose parent pin
#   resolves: the child, its parent, a path only the PARENT touches (the RED path) and a path in the
#   child's OWN delta (the GREEN path).  No row number and no SHA is typed here -- a control written
#   against a literal row is a control that rots the next time the table is re-seated.
CTLC=''; CTLP=''; CTLB=''; CTLPB=''; CTLRED=''; CTLGREEN=''; CTLPAIR=''
while IFS='|' read -r n b s c m rest; do
  [ -n "$n" ] || continue
  [ "$s" = "PENDING" ] && continue
  cso=$(printf '%s\n' "$SEAT_TABLE" | awk -F'|' -v r="$n" '$1==r{for(i=6;i<=NF;i++) if ($i ~ /^stack-on=/) {print substr($i,10); exit}}')
  [ -n "$cso" ] || continue
  cpb=$(printf '%s\n' "$SEAT_TABLE" | awk -F'|' -v r="$cso" '$1==r{print $2}')
  cps=$(printf '%s\n' "$SEAT_TABLE" | awk -F'|' -v r="$cso" '$1==r{print $3}')
  [ -n "$cps" ] && [ "$cps" != "PENDING" ] || continue
  git -C "$G" cat-file -e "${s}^{commit}" 2>/dev/null || continue
  git -C "$G" cat-file -e "${cps}^{commit}" 2>/dev/null || continue
  own=$(git -C "$G" -c core.quotepath=false diff --name-only "$(git -C "$G" merge-base "$cps" "$s")" "$s")
  par=$(git -C "$G" -c core.quotepath=false diff --name-only "$(git -C "$G" merge-base "$BASE_EXPECT" "$cps")" "$cps")
  red=$(printf '%s\n' "$par" | while IFS= read -r pp; do [ -n "$pp" ] || continue; printf '%s\n' "$own" | grep -aqxF -- "$pp" || { printf '%s\n' "$pp"; break; }; done | head -1)
  green=$(printf '%s\n' "$own" | grep -a . | head -1)
  [ -n "$red" ] && [ -n "$green" ] || continue
  CTLC="$s"; CTLP="$cps"; CTLB="$b"; CTLPB="$cpb"; CTLRED="$red"; CTLGREEN="$green"; CTLPAIR="row $n stacked on row $cso"
  break
done <<< "$SEAT_TABLE"
if [ -z "$CTLC" ]; then
  say "   FAIL (UNMEASURED): no stacked pair in this table could supply BOTH a parent-only path and a child path, so the base rule has NOT been made to fail and its green above says nothing"
  FAIL=1
else
  # ⚠ ONLY `.` IS ESCAPED FOR THE ERE.  These are repository paths -- alphanumerics, `/`, `.`, `-`,
  #   `_` -- and `|` can never appear in one because the table itself is `awk -F'|'`.
  ere_of(){ printf '%s' "$1" | sed 's/\./\\./g'; }
  say "   the control pair is DERIVED :: $CTLPAIR :: child $CTLB @$CTLC on parent $CTLPB @$CTLP"
  say "   RED path (the PARENT touches it, the child's own delta does NOT) :: [$CTLRED]"
  say "   GREEN path (in the child's OWN delta) :: [$CTLGREEN]"
  CTBL_R="1|$CTLPB|$CTLP|docs|tip
2|$CTLB|$CTLC|docs|tip|allowed=^$(ere_of "$CTLRED")\$|stack-on=1"
  CTBL_G="1|$CTLPB|$CTLP|docs|tip
2|$CTLB|$CTLC|docs|tip|allowed=^$(ere_of "$CTLGREEN")\$|stack-on=1"
  CTBL_N="1|$CTLPB|$CTLP|docs|tip
2|$CTLB|$CTLC|docs|tip|allowed=^$(ere_of "$CTLRED")\$"
  CR_OUT=$(stale_ruled "$CTBL_R"); CR_RC=$?
  CG_OUT=$(stale_ruled "$CTBL_G"); CG_RC=$?
  CN_OUT=$(stale_ruled "$CTBL_N"); CN_RC=$?
  if [ "$CR_RC" != "0" ] && [ "$(printf '%s\n' "$CR_OUT" | grep -acF -- "$CTLRED" || true)" != "0" ]; then
    say "   ok   CONTROL RED :: a stacked row ruling [$CTLRED] -- a path only its BASE touches -- reads STALE and the path is NAMED:"
    printf '%s\n' "$CR_OUT" | grep -a '^FAIL	' | while IFS= read -r l; do say "        CONTROL red: $(printf '%s' "$l" | sed 's/^FAIL\t//' | cut -c1-170)"; done
  else
    say "   FAIL CONTROL RED: the stacked row ruling a parent-only path read rc=$CR_RC and did not name [$CTLRED] -- the stale-ruling predicate never looked, and arm (f)'s green says nothing"
    FAIL=1
  fi
  if [ "$CG_RC" = "0" ] && [ "$(printf '%s\n' "$CG_OUT" | grep -ac '^ok	' || true)" != "0" ]; then
    say "   ok   CONTROL GREEN :: the SAME stacked row ruling [$CTLGREEN] -- a path in its OWN delta -- PASSES, so the red above is a reading and not a predicate that refuses everything"
  else
    say "   FAIL CONTROL GREEN: a ruling naming a path in the child's own delta read rc=$CG_RC with no ok line -- the predicate refuses even a LIVE ruling, which would make every red above meaningless"
    FAIL=1
  fi
  # ⚠ THE THIRD READING IS WHAT MAKES THE FIRST TWO ABOUT **THE BASE RULE** RATHER THAN ABOUT THE
  #   PATHS.  Strip `stack-on=` from the red table and nothing else: the row is then measured against
  #   the train's base, the parent's commits are inside that diff, and the SAME ruling PASSES.  If
  #   this reading also redded, the red control would be evidence about the path and not about E2.
  if [ "$CN_RC" = "0" ]; then
    say "   ok   DISCRIMINATOR :: the SAME table with stack-on= REMOVED passes on [$CTLRED] -- so the red control is produced BY the assembler base rule and by nothing else about that path"
  else
    say "   FAIL DISCRIMINATOR: the red ruling reds even WITHOUT stack-on= (rc=$CN_RC), so the control is not evidence about the base rule -- it would red under train 47's shape too"
    FAIL=1
  fi
fi

say ""
say "== (g) TWO ROWS RULING THE SAME PATH NEED A DECLARED STACK -- TABLE ARM, WITH A PLANTED RED CONTROL =="
say "   A7b's owner rule is 'the LAST row in merge order'.  That is a DECISION and it is only defensible"
say "   when the two rows are declared a stack; otherwise two seats rule one path with no stated order"
say "   and the union's blob would be compared against whichever row the loop happened to read last."
dupruled(){ # $1 = a seat table -> prints FAIL lines, returns 1 if any
  local T="$1" bad=0 p rws last so
  ALLP=$(while IFS='|' read -r n b s c m rest; do
    [ -n "$n" ] || continue
    aw=$(printf '%s\n' "$T" | awk -F'|' -v r="$n" '$1==r{for(i=6;i<=NF;i++) if ($i ~ /^allowed=/) {print substr($i,9); exit}}')
    [ -n "$aw" ] || continue
    printf '%s' "$aw" | sed 's/^\^//; s/\$$//; s/[()]//g' | tr '?' '\n' | sed 's/\\//g' | grep -a . | sed "s|^|$n |"
  done <<< "$T")
  for p in $(printf '%s\n' "$ALLP" | awk 'NF==2{print $2}' | sort | uniq -d); do
    rws=$(printf '%s\n' "$ALLP" | awk -v q="$p" '$2==q{print $1}' | sort -n | tr '\n' ' ')
    last=$(printf '%s\n' "$ALLP" | awk -v q="$p" '$2==q{print $1}' | sort -n | tail -1)
    so=$(printf '%s\n' "$T" | awk -F'|' -v r="$last" '$1==r{for(i=6;i<=NF;i++) if ($i ~ /^stack-on=/) {print substr($i,10); exit}}')
    case " $rws" in
      *" ${so:-<none>} "*) printf 'ok\t[%s] is ruled by rows [%s] and the LAST of them (row %s) DECLARES stack-on=%s -- the owner is unambiguous and A7b compares the union against THAT row\n' "$p" "$rws" "$last" "$so" ;;
      *) printf 'FAIL\t[%s] is ruled by rows [%s] with NO declared stack among them (row %s declares stack-on=[%s]) -- two seats rule one path and nothing says which owns it\n' "$p" "$rws" "$last" "${so:-none}"; bad=1 ;;
    esac
  done
  return $bad
}
DUPOUT=$(dupruled "$SEAT_TABLE"); DUPRC=$?
while IFS= read -r l; do [ -n "$l" ] && say "   $(printf '%s' "$l" | sed 's/\t/ /')"; done <<< "$DUPOUT"
if [ "$DUPRC" = "0" ]; then
  say "   PASS: no path is ruled by two rows without a declared stack"
else
  say "   FAIL: a path is ruled by two rows with no declared stack -- the A7b owner rule has nothing to stand on"
  FAIL=1
fi
# ⚠ THE PLANTED RED CONTROL.  The same predicate over a table whose two rows rule one path with NO
#   stack-on must REFUSE and must NAME the path.  Without it the PASS above is indistinguishable from
#   a predicate that never looked.
DUPPLANT="1|claude/a|111111111|docs|tip|allowed=^(docs/x\.md)?(docs/y\.md)?$
2|claude/b|222222222|docs|tip|allowed=^(docs/x\.md)?(docs/z\.md)?$"
DUPCTL=$(dupruled "$DUPPLANT"); DUPCTLRC=$?
if [ "$DUPCTLRC" != "0" ] && [ "$(printf '%s\n' "$DUPCTL" | grep -acF 'docs/x.md' || true)" != "0" ]; then
  say "   ok   CONTROL (planted) :: two rows ruling docs/x.md with no stack-on is REFUSED and the path is NAMED:"
  while IFS= read -r l; do [ -n "$l" ] && say "        CONTROL red: $(printf '%s' "$l" | sed 's/\t/ /' | cut -c1-150)"; done <<< "$DUPCTL"
else
  say "   FAIL CONTROL: the planted overlap read rc=$DUPCTLRC and did not name the path, so the green above says nothing"
  FAIL=1
fi
'''
S.insert_after(r'^\[ "\$GBAD" = "0" \] && say "   PASS: every fill point refuses by name', SC_ALW, 'sc-allowed-arms')

# ---- (F1a) ARM 10'S DECOY IS RE-ANCHORED ON THE READING, NOT ON THE LIST OF EXEMPTIONS -------------
#      The decoy grepped for the refusal BY ITS WHOLE SPELLING, exemption list included.  F1a adds
#      `eol=lf` beside `-text` in that list, and the spelling-exact predicate then read ZERO and
#      reported "the non-exempt refusal is missing, so the arm is a blanket pass for docs/" -- MEASURED
#      on the first online run after the fix.  An arm that can only see one spelling of a thing reports
#      the absence of that SPELLING as the absence of the THING; the predicate is re-anchored on the
#      part of the sentence that is about the READING and cannot move when the list grows again.
S.subline_contains("NEX=$(grep -c 'is NOT ",
                   r'''NEX=$(grep -c 'has a non-CRLF working-tree form' "$F")''',
                   'f1a-arm10-decoy-reanchored')

# ---- (F1a/F1b/F2) THE THREE RULED CHANGES GET THEIR OWN ARMS, EACH WITH A RED CONTROL --------------
#      ⚠ A GATE THAT HAS NEVER BEEN MADE TO FAIL PROVES NOTHING, and these three sites are exactly the
#      kind a later derive "improves": a CONDITIONAL exemption is one edit away from a blanket one, an
#      extra git command in an apply reads like noise, and an owe moved off a class reads like a
#      loosening.  (h) is STATIC with a copy regressed to train 47's exact shape; (i) is BEHAVIOURAL --
#      it extracts the assembly's OWN g11a_arm and runs it over a synthetic vector and over the
#      class-derived one, so what is measured is the live function rather than a retyped copy.
SC_F1F2 = r'''say ""
say "== (h) F1a/F1b :: G10d's eol=lf PIN EXEMPTION AND THE PRE-RESOLVED APPLY'S RE-MATERIALISATION, EACH WITH A REGRESSED COPY AS ITS RED CONTROL =="
say "   COORD rulings 2026-09-13, both MEASURED by run 2 on a healthy tree: DATA-sweep-row-walltimes.md"
say "   was refused on a CORRECT text eol=lf pin (attr=[set] i/lf w/lf, CR=0 LF=395), and"
say "   BOARD-next-validation-candidates.md (attr unspecified, i/lf w/lf, CR=0 LF=24779) was refused"
say "   because the PRE-RESOLVED apply copied an LF union file into the worktree and added it, leaving"
say "   a worktree form this box's autocrlf would never have produced."
f1_arm(){ # $1 = a script -> "eolAttrReads pinBranches pinCountStamped rematerialisations"
  printf '%s %s %s %s' \
    "$(grep -acF -- 'eolv=$(git check-attr eol --' "$1" || true)" \
    "$(grep -acF -- 'if [ "$eolv" = "lf" ]; then' "$1" || true)" \
    "$(grep -acF -- 'eolPinned=$G10DEOL' "$1" || true)" \
    "$(grep -acF -- 'git add -- "$up" && rm -f -- "$up" && git checkout -- "$up"' "$1" || true)"
}
F1CTL="/tmp/${TAG}-f1-regressed.sh"
# ⚠ THE CONTROL IS A COPY REGRESSED TO **TRAIN 47'S EXACT SHAPE**: the pin branch's guard neutered so
#   it can never be taken, and the re-materialising checkout sed'd out of the apply.  A control built
#   by any other edit would be reddening something else and calling it these arms.
sed -e 's/if \[ "\$eolv" = "lf" \]; then/if [ "$eolv" = "lf-NEVER" ]; then/' -e 's/ && rm -f -- "\$up" && git checkout -- "\$up"//' "$F" > "$F1CTL"
read -r EA EB ES ER <<EOF
$(f1_arm "$F")
EOF
read -r RA RB RS RR <<EOF
$(f1_arm "$F1CTL")
EOF
say "   derived   :: eolAttrReads=$EA (want 1) pinBranches=$EB (want 1) pinCountStamped=$ES (want 1) rematerialisations=$ER (want 1)"
say "   REGRESSED :: eolAttrReads=$RA pinBranches=$RB pinCountStamped=$RS rematerialisations=$RR"
if [ "${EA:-0}" -ge 1 ] && [ "${EB:-0}" -ge 1 ] && [ "${ES:-0}" -ge 1 ] && [ "${ER:-0}" -ge 1 ]; then
  say "   PASS: G10d reads the eol attribute, exempts an eol=lf pin whose two layers AGREE, stamps that count SEPARATELY from textExempt, and the PRE-RESOLVED apply re-materialises every resolved path through git"
else
  say "   FAIL: a half is missing (eolAttrReads=$EA pinBranches=$EB pinCountStamped=$ES rematerialisations=$ER, each want >= 1) -- a correct pin would be refused, or the apply's own LF artefact would be read as a seat's defect"
  FAIL=1
fi
if [ "${RB:-1}" = "0" ] && [ "${RR:-1}" = "0" ]; then
  say "   ok   CONTROL (regressed copy) :: with the pin branch neutered and the checkout sed'd out -- train 47's exact shape -- the SAME predicate reads pinBranches=0 rematerialisations=0, so the green above can go red"
else
  say "   FAIL CONTROL: the regressed copy still reads pinBranches=$RB rematerialisations=$RR, so the green above says nothing about the two sites it claims to read"
  FAIL=1
fi
rm -f "$F1CTL"

say ""
say "== (i) F2 :: G11(a)'s golibtests OWE IS DELTA-DERIVED -- THE ASSEMBLY'S OWN g11a_arm OVER A SYNTHETIC OWED VECTOR, WITH THE CLASS-DERIVED OWE AS ITS RED CONTROL =="
say "   COORD ruling 2026-09-13.  Run 2 read JUSTIFICATION FALSE :: src/tests/GolibTests=0 where the"
say "   OWED vector says a merged class owes it -- on a tree whose golib-corpus-handown rows touch"
say "   src/core/runtime and src/go2cs and nothing under src/tests/GolibTests.  A class says what a"
say "   seat MAY touch; the arm refuses on what a seat DID touch, and those are different questions."
f2_arm(){ # $1 = a script -> "deltaDerivations classSetters stampNamesDelta golibStillClassDerived"
  printf '%s %s %s %s' \
    "$(grep -acF -- 'OWED_GT_FILES=$(git -c core.quotepath=false diff --name-only "$BASE" HEAD -- src/tests/GolibTests' "$1" || true)" \
    "$(grep -acF -- 'OWED_GOLIB=1; OWED_GT=1' "$1" || true)" \
    "$(grep -acF -- 'golibtests=$OWED_GT (from the DELTA: $OWED_GT_FILES file(s))' "$1" || true)" \
    "$(grep -acF -- 'OWED_GOLIB=1 ;; esac' "$1" || true)"
}
read -r D1 D2 D3 D4 <<EOF
$(f2_arm "$F")
EOF
say "   deltaDerivations=$D1 (want 1) :: class-derived OWED_GT setters=$D2 (want 0) :: the stamp NAMES the delta=$D3 (want 1) :: OWED_GOLIB still class-derived=$D4 (want 1)"
if [ "${D1:-0}" = "1" ] && [ "${D2:-1}" = "0" ] && [ "${D3:-0}" = "1" ] && [ "${D4:-0}" = "1" ]; then
  say "   PASS: golibtests is measured from the merged DELTA and stamped as such, and OWED_GOLIB is left class-derived so LEG 3 still RUNS for the golib family"
else
  say "   FAIL: the golibtests owe is still a class's potential (deltaDerivations=$D1 classSetters=$D2 stampNamesDelta=$D3 golibClassDerived=$D4)"
  FAIL=1
fi
# the BEHAVIOURAL half: the assembly's OWN justification arm, run over a SYNTHETIC owed vector.
eval "$(blk '^g11a_arm\(\)' '^\}$' 'g11a_arm (the justification arm itself)')" || exit 3
G11ABAD=0
g11a_arm 0 0 'src/tests/GolibTests' 'SYNTHETIC: a golib-corpus-handown class whose merged delta carries NO GolibTests file'
SYNTH="$G11ABAD"
G11ABAD=0
g11a_arm 1 0 'src/tests/GolibTests' 'SYNTHETIC REGRESSION: the class-derived owe, which is run 2 exactly'
REGR="$G11ABAD"
say "   the arm over a DELTA-derived owe (owed=0 measured=0) sets G11ABAD=$SYNTH (want 0) :: over the CLASS-derived owe (owed=1 measured=0) it sets G11ABAD=$REGR (want 1)"
if [ "$SYNTH" = "0" ] && [ "$REGR" = "1" ]; then
  say "   ok   CONTROL :: the SAME function passes the synthetic vector and REFUSES the regressed one -- the arm keeps its teeth and simply cannot go FALSE on a class's potential any more"
else
  say "   FAIL CONTROL: synthetic=$SYNTH regressed=$REGR -- either the arm no longer refuses at all (its teeth are gone) or it still refuses a delta-derived zero"
  FAIL=1
fi

'''
S.subline_contains('[ "${OFFLINE_UNMEASURED:-0}" = "0" ] || say',
                   SC_F1F2 + '[ "${OFFLINE_UNMEASURED:-0}" = "0" ] || say "⚠ OFFLINE :: $OFFLINE_UNMEASURED pin(s) could not be resolved because T48_SELFCHECK_OFFLINE=1 forbade the fetch.  They are counted as FAILURES above and they are UNMEASURED, not findings: re-run without the switch, outside any freeze, to turn them into measurements."',
                   'f1f2-selfcheck-arms')

SC_LINES = S.write('coord-train48-derive-selfcheck.sh')
for r in S.report:
    print('   ' + r)
print('   self-check ops=%d  lines %d -> %d' % (S.ops, S.orig_n, SC_LINES))


# ============================================================================================
# THE LAUNCH WRAPPER AND THE REFERENCE COPY
# ============================================================================================
# ⚠ A SECOND WRITE GUARD FOR THE FILES THAT ARE NOT DERIVED FROM A TEMPLATE.  The launch wrapper has
#   no train-47 parent worth editing (it is six lines), and the reference copy is a VERBATIM copy --
#   but both are writes, and a derive that can write an unnamed path is the door that opens by itself.
PLAIN_DST = {'launch-run1.sh', 't47-derive.py.reference'}


def write_plain(name, text, binary_src=None):
    if name not in PLAIN_DST:
        raise SystemExit('   WRITE REFUSED: [%s] is not a train-48 plain destination.' % name)
    path = os.path.join(HERE, name)
    if binary_src is not None:
        with io.open(binary_src, encoding='utf-8', newline='') as f:
            text = f.read()
    with io.open(path, 'w', encoding='utf-8', newline='') as f:
        f.write(text)
    n = text.count(NL)
    cr = text.count('\r')
    if cr:
        raise SystemExit('   WROTE CR BYTES into %s (%d) -- the train-47 template is pure LF' % (name, cr))
    if name.endswith('.sh'):
        rc = os.system('bash -n "%s"' % path.replace('\\', '/'))
        if rc != 0:
            raise SystemExit('   WROTE AN UNPARSEABLE SCRIPT: %s' % name)
    print('   wrote %-42s %5d lines  CR=%d' % (name, n, cr))
    return n


LAUNCH = r"""# TRAIN 48 -- LAUNCH WRAPPER FOR RUN 1.  Derived from train 47's launch-run4.sh, same shape.
# ⚠ (1) THE SWITCH IS **EXPORTED**, NOT ONE-SHOT PREFIXED.  A `nohup`/`start` layer can drop a one-shot
#       VAR=x prefix between the assignment and the child, and the run would then proceed as a PARTIAL
#       train while reading as a landing.  ⚠ AND IT IS `TRAIN48_REQUIRE_ALL`, NEVER the un-prefixed
#       `REQUIRE_ALL`: that name is the assembly's own internal variable and is assigned
#       unconditionally, so exporting it would configure NOTHING.  The assembly refuses an inherited
#       un-prefixed spelling by name rather than overwriting it silently.
# ⚠ (2) THE LAST STATEMENT IS `exit $rc`.  A wrapper whose last statement is a `tail` (or any pipe)
#       reports the LAST command's status, so a script that exited 1 is announced as 0.  `rc=$?` is
#       captured as the FIRST statement after the run, before anything else can reset it.
# ⚠ (3) IT LAUNCHES A **PER-RUN COPY**.  bash reads a script incrementally BY BYTE OFFSET, so an edit
#       above the running position reparses the next command from the middle of a line -- a train
#       assembly died thirty minutes in on exactly that.  Make the copy first:
#           cp coord-train48-assemble.sh coord-train48-assemble-run1.sh
#       The labels derive from the basename, so the copy relabels itself and nothing is written out.
export TRAIN48_REQUIRE_ALL=1
cd "$(dirname "$0")"
bash coord-train48-assemble-run1.sh > coord-train48-assemble-run1.stdout 2>&1; rc=$?
# the wrapper's OWN trailing line -- the land's exit scan requires it as the record's last non-empty line (2026-09-13 run 8)
printf 'assembly exit=%s\n' "$rc" >> coord-train48-assemble-run1.stdout
echo "=== ASSEMBLY EXIT rc=$rc ==="
tail -40 coord-train48-assemble-run1.stdout
exit $rc
"""
LAUNCH_LINES = write_plain('launch-run1.sh', LAUNCH)
REF_LINES = write_plain('t47-derive.py.reference', '', binary_src=os.path.join(SRC, 't47-derive.py'))


# ============================================================================================
# THE REPORT
# ============================================================================================
print('')
print('=== TRAIN 48 TEMPLATE DERIVE ===')
print('interpreter : %s' % sys.version.split()[0])
print('template    : %s/coord-train47-assemble.sh' % SRC)
print('sha256      : %s' % T47ASM_SHA)
TOTAL = A.ops + R.ops + S.ops + L.ops + D.ops
print('')
print('%-42s %6s %6s %6s' % ('file', 'src', 'out', 'ops'))
for doc, dst, out_lines in ((A, 'coord-train48-assemble.sh', ASM_LINES),
                            (R, 'coord-train48-rehearse.sh', REH_LINES),
                            (S, 'coord-train48-derive-selfcheck.sh', SC_LINES),
                            (L, 'coord-train48-land.sh', LAND_LINES),
                            (D, 'coord-train48-land-dryread.sh', DR_LINES)):
    print('%-42s %6d %6d %6d' % (dst, doc.orig_n, out_lines, doc.ops))
print('%-42s %6s %6d %6s' % ('launch-run1.sh', '-', LAUNCH_LINES, '-'))
print('%-42s %6s %6d %6s' % ('t47-derive.py.reference', '-', REF_LINES, 'verbatim'))
print('')
print('TOTAL asserted operations: %d' % TOTAL)
print('=== DERIVE DONE ===')
