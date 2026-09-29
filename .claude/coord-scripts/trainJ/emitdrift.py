import os, sys
# usage: emitdrift.py <emitted root> <pristine seed root> <sentinel file> <out-written> <out-drift> [<out-siblings>]
# Lists files under <emitted>/src/core newer than the sentinel, and those whose CR-stripped bytes differ from the pristine
# seed (or are absent from it: NEW). *.cs.auto REVIEW SIBLINGS are kept OUT of written/drift (the build never compiles
# them) and, when <out-siblings> is given, reported there as their own class: a written sibling whose CR-stripped bytes
# differ from the seed's. TRAIN J onward: every train's fixup refreshes that class from the union arm (ruling 857afcb47e;
# before it, the check excluded siblings entirely and they went stale one converter seat at a time).
root, seed, sentinel, outw, outd = sys.argv[1:6]
outs = sys.argv[6] if len(sys.argv) > 6 else None
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
print(len(written), len(drift), len(siblings))
