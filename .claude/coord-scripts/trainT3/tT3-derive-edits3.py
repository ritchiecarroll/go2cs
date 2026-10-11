# TRAIN T3 -- the lane briefs (applied after tT3-derive-edits2.py). R0 renamed every name in them; this pass puts the
# pushed union ref in, corrects the two sentences that called T3 the face lift, and puts a T3 block on top that leads.
#   python -B tT3-derive-edits3.py <this folder>
import os, sys
D = sys.argv[1]
def ed(f, pairs, top):
    p = os.path.join(D, f)
    with open(p, encoding='utf-8', newline='') as fh: t = fh.read()
    for old, new, n in pairs:
        assert t.count(old) == n, f'{f}: x{t.count(old)} (want {n}): {old[:70]!r}'
        t = t.replace(old, new)
    first, rest = t.split('\n', 1)
    t = first + '\n\n' + top + '\n' + rest
    assert '\r' not in t
    with open(p, 'w', encoding='utf-8', newline='\n') as fh: fh.write(t)

TOP = """**T3 (2026-10-10), READ THIS FIRST; it leads every paragraph below.** TRAIN T3 is NOT the face lift and NOT a corpus
re-conversion: 24 rows (C1's darwin executable path, argv0, crc32 arm64, a disclosure retirement, the CNR alias-drift
entries and Phase 5 stubs; C2's module manifest rows, the nugetgo cuts 1-8 and pack README seat, the go.* README seat;
G's trim stages 3a to 3c-2b(iii) and the trim plan), base master `4e6322d770`. The union is pushed as
`claude/coord-trainT3-union` (COORD names the SHA in the GO: the T3 fixup, or a `fixup-N: TRAIN T3` on top). What the
FL text below says about the face lift does NOT apply: the face lift landed with TRAIN FL (refresh `0574b8336c`), the
face-lift -tests classes are OFF in this kit (`tT3-helpers.py` FL_CLASSES), and NO T3 row changes -stdlib or -tests
emission, so your sweeps are PREDICTED to rewrite ~0 committed test sources beyond the known line-ending / standing
OTHER set; any content rewrite is read BY NAME and posted. The previous train's record is FL's (your FL shard patch is
the TE baseline). The GolibTests classes T3 adds are G's trim guards: TrimStage3aTests, TrimStage3c1Tests,
TrimStage3c2aTests, TrimStage3c2bTests, GoZeroConstructionGuardTests (GoZeroResidualTests, SliceHeaderReinterpretTests
and CallerFrameTestVariantNamingTests changed); CNR N at the union = 871 (878 enumerated, 7 platform skips on windows;
on linux read your own enumeration); row 5 adds OsGetpagesize and SyscallKeystonePulls to CNR's documented alias-drift
set (OsGetpagesize's golden is the WINDOWS emission at row 1's tip, so a linux transpile is the drifting side).
"""

ed('tT3-lane-brief-i9.md', [
    ("**TRAIN T3 is the face lift**: it re-converts the corpus and a release follows its\nlanding.",
     "**(FL's sentence, superseded by the T3 block above: T3 is not the face lift.)**", 1),
    ("`train-t3-union`", "`claude/coord-trainT3-union`", 1),
    ("git fetch origin train-t3-union", "git fetch origin claude/coord-trainT3-union", 1),
], TOP)
ed('tT3-lane-brief-linux.md', [
    ("TRAIN T3 is the face lift (rows 1-10 hand-merged on `train-t3-union`,", "(FL's text, superseded by the T3 block above:) TRAIN FL was the face lift (rows 1-10 hand-merged on FL's union,", 1),
    ("`train-t3-union` / `<UNION", "`claude/coord-trainT3-union` / `<UNION", 1),
], TOP)
print('EDITS3 applied')
