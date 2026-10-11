# TRAIN T3 -- the R0 rename of TRAIN FL's kit (coord-scratch/tFL/run2, read only), then T3's anchored edits
# (tT3-derive-edits.py). Run ONCE by the derive, 2026-10-10; kept as the record of what R0 did. Re-running it
# overwrites the renamed copies in this folder (never the hand-written files: README, CHANGES, checklist, the seat
# table, the ruled file, launch-battery.sh).
#   python -B tT3-derive.py <FL kit folder> <this folder>
import os, re, sys

SRC, DST = sys.argv[1], sys.argv[2]
CARRY = ['tFL-assemble.sh', 'tFL-battery.sh', 'tFL-conflict-map.sh', 'tFL-controls.sh', 'tFL-emitcheck.sh',
         'tFL-fixup.sh', 'tFL-helpers.py', 'tFL-i7-sweeps.txt', 'tFL-i9-shard.sh', 'tFL-i9-shard.txt', 'tFL-i9-te.sh',
         'tFL-land-final-reads.sh', 'tFL-land-prep.sh', 'tFL-land-rereads.sh', 'tFL-lane-brief-i9.md',
         'tFL-lane-brief-linux.md', 'tFL-linux-legs.sh', 'tFL-modules-legs.sh', 'tFL-ng-parse.ps1', 'tFL-regen-apply.py',
         'tFL-reread.sh', 'tFL-resolve-controls.sh', 'tFL-resolve.py', 'tFL-selfcheck.sh', 'tFL-follow.txt',
         'emitdrift.py']
# NOT carried: tFL-seats-draft.txt, tFL-ruled.txt, tFL-ruled-both.txt (rewritten), tFL-README.md, tFL-CHANGES.md,
# COORD-LAUNCH-CHECKLIST.md (rewritten), facelift-tests-hunk-classes.md (FL1's input: dropped), launch-battery.sh
# (rewritten), FL's logs and run records (read in place as the previous train's record).

def code_line(line, fname):
    # every name of the train moves
    if fname == 'tFL-linux-legs.sh':
        line = line.replace('claude/coord-trainFL-union', 'claude/coord-trainT3-union')   # the PUSHED name lanes read
    line = line.replace('claude/coord-trainFL-union', 'train-t3-union')                   # the LOCAL union branch
    line = line.replace('trainFL', 'trainT3').replace('TFL_', 'TT3_')
    line = line.replace('FL-CONTROLS', 'T3-CONTROLS').replace('FL-SELFCHECK', 'T3-SELFCHECK')
    line = line.replace('TRAIN FL', 'TRAIN T3')
    line = line.replace('tFL', 'tT3')
    return line

PTR = re.compile(r'trainFL/tFL-[A-Za-z0-9._-]+|trainFL/[A-Za-z0-9._-]+|coord-scratch/tFL/run2[A-Za-z0-9._/-]*')
SUBJ = re.compile(r"((?:'|\b)(?:fixup|fixup-N|fixup-\d+|refresh): TRAIN FL|into TRAIN FL)")

def comment_line(line):
    # pointers to FL's own records stay; sibling script names, scratch paths and quoted subjects move
    keep = {}
    def stash(m):
        k = f'\x00{len(keep)}\x00'; keep[k] = m.group(0); return k
    line = PTR.sub(stash, line)
    line = SUBJ.sub(lambda m: m.group(0).replace('TRAIN FL', 'TRAIN T3'), line)
    line = line.replace('tFL-', 'tT3-').replace('/h/go2cs-tmp-coord/tFL', '/h/go2cs-tmp-coord/tT3')
    line = line.replace('coord-scratch/tFL', 'coord-scratch/tT3')
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
            out.append(code_line(line, f))           # the briefs are prose a lane executes: every name moves
        elif line.lstrip().startswith('#') and not line.startswith('#!'):
            out.append(comment_line(line))
        else:
            out.append(code_line(line, f))
    dst = os.path.join(DST, f.replace('tFL-', 'tT3-'))
    with open(dst, 'w', encoding='utf-8', newline='\n') as fh: fh.write('\n'.join(out))
    n += 1
print(f'R0: {n} files renamed into {DST}')
