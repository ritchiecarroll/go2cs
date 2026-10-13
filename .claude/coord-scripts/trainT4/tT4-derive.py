# TRAIN T4 -- the R0 rename of TRAIN T3's kit (coord-scratch/tT3/run2, read only: it landed T3 at 4c9c68a32f and carries
# run2's two patches, the rc regex '\( (DEADLINE was [^)]*)\)\{0,1\}' in both landing scripts and MASTER_MOVED_OK in
# land-prep, plus run2's emitcheck seed of docs/ValidatedTestPackages.md). Then T4's anchored edits (tT4-derive-edits.py).
# Run ONCE by the derive, 2026-10-12; kept as the record of what R0 did. Re-running it overwrites the renamed copies in
# this folder (never the hand-written files: README, CHANGES, checklist, the seat table, the ruled file, launch-battery.sh).
#   python -B tT4-derive.py <T3 kit folder> <this folder>
import os, sys

SRC, DST = sys.argv[1], sys.argv[2]
CARRY = ['tT3-assemble.sh', 'tT3-battery.sh', 'tT3-conflict-map.sh', 'tT3-controls.sh', 'tT3-emitcheck.sh',
         'tT3-fixup.sh', 'tT3-helpers.py', 'tT3-i7-sweeps.txt', 'tT3-i9-shard.sh', 'tT3-i9-shard.txt', 'tT3-i9-te.sh',
         'tT3-land-final-reads.sh', 'tT3-land-prep.sh', 'tT3-land-rereads.sh', 'tT3-lane-brief-i9.md',
         'tT3-lane-brief-linux.md', 'tT3-linux-legs.sh', 'tT3-modules-legs.sh', 'tT3-ng-parse.ps1', 'tT3-regen-apply.py',
         'tT3-reread.sh', 'tT3-resolve-controls.sh', 'tT3-resolve.py', 'tT3-selfcheck.sh', 'tT3-follow.txt',
         'emitdrift.py']
# NOT carried: tT3-seats-draft.txt, tT3-ruled.txt (rewritten), tT3-README.md, tT3-CHANGES.md, COORD-LAUNCH-CHECKLIST.md,
# launch-battery.sh (rewritten), tT3-derive*.py (T3's own derive record), T3's logs and run records (read in place as the
# previous train's record: coord-scratch/tT3/run2).

def code_line(line):
    # every name of the train moves (T3 had no hand union and no MAP_FROM, so the rules are T3's own R0 with one more
    # step: the pushed union name already sits in tT3-linux-legs.sh and the briefs as claude/coord-trainT3-union)
    line = line.replace('claude/coord-trainT3-union', 'claude/coord-trainT4-union')   # the PUSHED name lanes read
    line = line.replace('train-t3-union', 'train-t4-union')                           # the LOCAL union branch
    line = line.replace('trainT3', 'trainT4').replace('TT3_', 'TT4_')
    line = line.replace('T3-CONTROLS', 'T4-CONTROLS').replace('T3-SELFCHECK', 'T4-SELFCHECK')
    line = line.replace('TRAIN T3', 'TRAIN T4')
    line = line.replace('tT3', 'tT4')
    return line

import re
PTR = re.compile(r'trainT3/tT3-[A-Za-z0-9._-]+|trainT3/[A-Za-z0-9._-]+|coord-scratch/tT3/run[12][A-Za-z0-9._/-]*')
SUBJ = re.compile(r"((?:'|\b)(?:fixup|fixup-N|fixup-\d+|refresh): TRAIN T3|into TRAIN T3)")

def comment_line(line):
    # pointers to T3's own records stay; sibling script names, scratch paths and quoted subjects move
    keep = {}
    def stash(m):
        k = f'\x00{len(keep)}\x00'; keep[k] = m.group(0); return k
    line = PTR.sub(stash, line)
    line = SUBJ.sub(lambda m: m.group(0).replace('TRAIN T3', 'TRAIN T4'), line)
    line = line.replace('tT3-', 'tT4-').replace('/h/go2cs-tmp-coord/tT3', '/h/go2cs-tmp-coord/tT4')
    line = line.replace('coord-scratch/tT3', 'coord-scratch/tT4')
    for k, v in keep.items(): line = line.replace(k, v)
    return line

n = 0
for f in CARRY:
    src = os.path.join(SRC, f)
    with open(src, encoding='utf-8', newline='') as fh: text = fh.read()
    assert '\r' not in text, f
    out = []
    for line in text.split('\n'):
        if f.endswith('.md'):
            out.append(code_line(line))           # the briefs are prose a lane executes: every name moves
        elif line.lstrip().startswith('#') and not line.startswith('#!'):
            out.append(comment_line(line))
        else:
            out.append(code_line(line))
    dst = os.path.join(DST, f.replace('tT3-', 'tT4-'))
    with open(dst, 'w', encoding='utf-8', newline='\n') as fh: fh.write('\n'.join(out))
    n += 1
print(f'R0: {n} files renamed into {DST}')
