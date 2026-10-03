import os, sys
# usage: emitdrift.py <emitted root> <pristine seed root> <sentinel file> <out-written> <out-drift> [<out-siblings>] [<out-editorconfig>]
# TRAIN N (item N7): a file named .editorconfig (P2's per-file warning entries beside a csproj) is kept OUT of written
# and drift like a review sibling and, when <out-editorconfig> is given, listed there whenever a conversion wrote it
# (newer than the sentinel), tagged NEW / changed / same. Without the seventh argument the file is still kept out of
# written/drift and simply not listed (TRAIN M's six-argument call reads as before for every other file).
# REVIEW ROUND 1 (2026-10-03): the entries are NOT hand-written. COORD's notes (tL-seats-draft.txt:83, :87) rule them a
# compiler config entry THE CONVERTER EMITS where it knows the pattern, in P2's PENDING converter-warning-clears seat. At
# the 27-row list no row carries that converter change and no .editorconfig exists under src/core (base or any row), so a
# conversion writing one is unexpected: listed here, a die in the fixup, a finding in leg E. Should that seat board, this
# class becomes ordinary emission (drift-compared, copied by the fixup): COORD's ruling, tN-README.md section 7.
# Lists files under <emitted>/src/core newer than the sentinel, and those whose CR-stripped bytes differ from the pristine
# seed (or are absent from it: NEW). *.cs.auto REVIEW SIBLINGS are kept OUT of written/drift (the build never compiles
# them) and, when <out-siblings> is given, reported there as their own class: a written sibling whose CR-stripped bytes
# differ from the seed's. TRAIN J onward: every train's fixup refreshes that class from the union arm (ruling 857afcb47e;
# before it, the check excluded siblings entirely and they went stale one converter seat at a time).
root, seed, sentinel, outw, outd = sys.argv[1:6]
outs = sys.argv[6] if len(sys.argv) > 6 else None
oute = sys.argv[7] if len(sys.argv) > 7 else None
edconf = []
t0 = os.stat(sentinel).st_mtime
written, drift, siblings = [], [], []
base = os.path.join(root, 'src', 'core')
for dp, dn, fn in os.walk(base):
    for n in fn:
        p = os.path.join(dp, n)
        if os.stat(p).st_mtime <= t0:
            continue
        rel = os.path.relpath(p, root).replace(os.sep, '/')
        q = os.path.join(seed, rel)
        if n == '.editorconfig':
            if not os.path.exists(q): edconf.append(rel + ' NEW')
            else: edconf.append(rel + (' same' if open(p, 'rb').read().replace(b'\r', b'') == open(q, 'rb').read().replace(b'\r', b'') else ' changed'))
            continue
        sibling = n.endswith('.cs.auto')
        if not sibling:
            written.append(rel)
        if not os.path.exists(q):
            (siblings if sibling else drift).append(rel + ' NEW'); continue
        a = open(p, 'rb').read().replace(b'\r', b'')
        b = open(q, 'rb').read().replace(b'\r', b'')
        if a != b:
            (siblings if sibling else drift).append(rel)
written.sort(); drift.sort(); siblings.sort()
open(outw, 'w', encoding='utf-8', newline='\n').write(''.join(x + '\n' for x in written))
open(outd, 'w', encoding='utf-8', newline='\n').write(''.join(x + '\n' for x in drift))
if outs:
    open(outs, 'w', encoding='utf-8', newline='\n').write(''.join(x + '\n' for x in siblings))
if oute:
    edconf.sort()
    open(oute, 'w', encoding='utf-8', newline='\n').write(''.join(x + '\n' for x in edconf))
print(len(written), len(drift), len(siblings))
