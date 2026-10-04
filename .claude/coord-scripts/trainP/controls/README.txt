TRAIN P -- controls for tP-helpers.py teattr (the TE reading) and hunkclass (the fixup's golden step)
====================================================================================================

P (2026-10-04): the two patches are TRAIN M's (N's and O's), byte-identical (cmp against trainO/controls), and the readers
they control are O's bytes: teattr's VERDICT wording names P and its baseline (TRAIN O's patch), and nothing it counts
moved, so the PREDICTED lines below hold for tP-helpers.py unchanged. P's changed readers have their own controls,
run at the derive on SYNTHETIC inputs (trainP/tP-DERIVE-REPORT.md section 3; no build, no conversion, nothing in a
worktree): tP-regen-apply.py's rule 4 under incremental writes (the one-target case refused, O's reader accepting the
same evidence); the battery's UF arm and its E_BISECT derivation; the i9 driver's UF arm; the release-fix predicate (3
at O's union, 0 at the sort seat, read with git). Precheck's H3 entry and S1_NONE addition are read by MS9b's two
precheck runs (README).
O (2026-10-03): the two patches are TRAIN M's (and N's), byte-identical, and so are the readers they control: teattr's
VERDICT wording changed again at O (the G-frame class after N's MS13 refresh) and nothing it counts. The PREDICTED lines
below hold for tP-helpers.py unchanged. O's changed readers have no patch control: precheck's identical-insert credit
(D1), MULTI-BASE synthesis (O3) and DARWIN arm (O6), and csprojtemplate's template-file delta (D2); their controls are
manual step MS9b of tP-README.md (planned, with predictions; none was run by the derive: no script of this set was run).
N (2026-10-03): N's new readers had their own controls, run by N's derive workflow (read-only, 2026-10-03):
  cnrexpect at TRAIN M's landed master 8f46a9adae --base aa0a07d5fd reads enumerated=791 skipped=6 n=785 and added=7,
    which is TRAIN M's run3 CNR line (N=785, the six platform-exclusive skips by name) and M's seven projects;
  testsrc-refresh over TRAIN M's run3 S-rewrites.patch + T-rewrites-U0.patch builds a filtered patch of 338 committed
    test sources and EXCLUDES 27 paths by category (19 production package_init.cs and similar, 2 proof pages, 5 review
    siblings, 1 README), refused 0, conflicts 0 (coord-scratch/tN/derive/smoke-refresh.log; nothing applied).
The precheck, LIVE-gate and seat-table controls are manual steps MS9b and MS9c of tP-README.md, with O's predictions.
The text below is M's with the file names renamed; its manual-step references now name tP-README.md's MS steps. Its
M-history pointers (refs/coord/tM-map/..., coord-scratch/tM/asm1/...) are restored to M's names (N's derive had renamed
them to tN, which never held those files: trainO/tO-CHANGES.md O13).

Both patches were produced with git reads only (2026-10-02). NEITHER READER WAS RUN on them: the author of this draft
may not execute a draft script. The readings below are PREDICTED by reading te_hunks / te_class against the patch
text. Run both before the first battery and compare; a different line is a defect in the reader, not in the patch.

te-pos-goframe.patch   POSITIVE control for the G-FRAME and MAP classes
    git diff -U0 0d04adbb36^ 0d04adbb36 -- src/tests/Behavioral/CaptureHoistThroughConversion/
    (one project of G's own golden re-baseline: 3 files, 5 hunks, 25 lines)
    python -B tP-helpers.py teattr controls/te-pos-goframe.patch
      PREDICTED  TE files=3 hunks=5 g-frame=4 (noinline-lines=2 using-lines=2, in 2 file(s); g-frame-only files=2) map-only=1 other=0
    python -B tP-helpers.py hunkclass controls/te-pos-goframe.patch
      PREDICTED  three HUNKCLASS lines (main.cs and main.cs.target: noinline-lines=1 using-lines=1; package_info.cs:
                 map-only-hunks=1), then HUNKCLASS-VERDICT files=3 hunks=5 other-files=0, exit 0

te-neg-i9-testing.patch   NEGATIVE control: a seat's attribute insertion that is NOT G's
    git diff -U0 02a0b44467 7a8d02f6fe -- src/core/testing/testing.cs
    (the i9 seat's 25 '[GoRecv] ' insertions: the same SHAPE as G's NoInlining prefix, a different attribute)
    python -B tP-helpers.py teattr controls/te-neg-i9-testing.patch
      PREDICTED  TE files=1 hunks=25 g-frame=0 (noinline-lines=0 using-lines=0, in 0 file(s); g-frame-only files=0) map-only=0 other=25
    python -B tP-helpers.py hunkclass controls/te-neg-i9-testing.patch
      PREDICTED  HUNKCLASS-VERDICT files=1 hunks=25 other-files=1 ..., exit 1

Baseline arm (teattr --baseline): with the positive control as BOTH patch and baseline, the reader must print
'OTHER hunks this patch carries and the baseline does not: 0'; with the negative control as the patch and the positive
control as the baseline it must print 25.

TRAIN L's three controls (te-pos-d6.patch, te-pos-xpkg.patch, te-neg-L-src-core.patch) tested signatures of two L
seats and are not carried.

Verify round 1: the controls for precheck (two read-only runs: the pre-map union's worktree and a checkout at
aa0a07d5fd) and for the LIVE gate (a dummy lock folder) need no patch file; their commands and PREDICTED readings are
manual step MS9b in tP-README.md.

Verify round 2: the TE line gained 'g-frame-only files=<n>' (the files whose every hunk is G's frame class or a map
re-encode: the number manual step MS13's ruled deferral starts from); the two PREDICTED TE lines above carry it. The
controls for tP-assemble.sh's table check (three planted seat lists under TABLE_ONLY=1) need no patch file either:
manual step MS9c in tP-README.md. tP-regen-apply.py's refusal arm is controlled inside tP-fixup.sh (step 4, allow=/^$/).

Verify round 3 (the seat list is FROZEN: 24 rows, rows-sha256=0b3fd4358320, map union 11c188daf3): the precheck
control on the map union of the frozen list is PREDICTED rc 0 with NO FAIL line (hazard H4 is resolved inside its seat,
row p2-test-overload-references e002a552a8), not "exactly one FAIL, H4" as rounds 1 and 2 predicted for the earlier
lists. The H4 arm's FIRING control is the same command on the 06:31 list's union 382ceeb838 (kept as
refs/coord/tM-map/382ceeb838) with a copy of the list whose row 18 is 5556ef86cb: manual step MS9b in tP-README.md.
M9c on the frozen list: COORD's three planted lists are coord-scratch/tM/asm1/c1.txt, c2.txt, c3.txt (08:15 to
08:17), and asm1/table-real.log holds the real list's 'TABLE OK: 24 rows ... rows-sha256=0b3fd4358320'. The planted
runs' own output is not kept in that folder: this note records the lists, not their readings.
