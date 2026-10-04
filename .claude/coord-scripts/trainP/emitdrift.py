import os, sys
# usage: emitdrift.py <emitted root> <pristine seed root> <sentinel file> <out-written> <out-drift> [<out-siblings>] [<out-editorconfig>] [<out-copy-dir>]
# TRAIN P: O's bytes below this note (DERIVED 2026-10-04 from trainO/emitdrift.py; no logic change). What P changes is
# what <out-written> MEANS for the union arm: with i9-incremental-cs-writes-r2 in the union's converter a source whose
# emission equals the seeded file is not written, so 'written' is 'emitted with different bytes', not 'emitted'. Drift
# (below) is unaffected: it was always the written files that differ (trainP/tP-CHANGES.md P10; tP-emitcheck.sh header).
# TRAIN O (trainO/tO-CHANGES.md O9, N's Q16 applied): a file named .editorconfig (the per-file warning entries the CONVERTER writes
# beside a package's csproj: p2-converter-warning-clears' warningEntries.go, a TRAIN O row) is ORDINARY EMISSION. It is in
# written and, when its CR-stripped bytes differ from the seed (or it is absent there: NEW), in drift, and its drifting
# copy goes to <out-copy-dir> like any other file, so tP-emitcheck.sh attributes it and tP-regen-apply.py copies it under
# REGEN_ALLOW. The seventh output still LISTS every .editorconfig a conversion wrote (tagged NEW / changed / same) as a
# READING (tP-emitcheck.sh's EDITORCONFIG lines); it no longer keeps the file out of written/drift. Review round 1: a
# committed .editorconfig the conversion DELETED is listed too (drift '<rel> DELETED', seventh output tagged DELETED).
# TRAIN N (trainN/tN-CHANGES.md item N7, review round 1): the same file was kept OUT of written/drift, because no row of N's
# list carried the converter change and a conversion writing one was unexpected (a die in N's fixup, a finding in N's leg E).
# Lists files under <emitted>/src/core newer than the sentinel, and those whose CR-stripped bytes differ from the pristine
# seed (or are absent from it: NEW). *.cs.auto REVIEW SIBLINGS are kept OUT of written/drift (the build never compiles
# them) and, when <out-siblings> is given, reported there as their own class: a written sibling whose CR-stripped bytes
# differ from the seed's. TRAIN J onward: every train's fixup refreshes that class from the union arm (ruling 857afcb47e;
# before it, the check excluded siblings entirely and they went stale one converter seat at a time).
# FIXUP STOP 2026-10-03 (COORD, at N): the optional EIGHTH argument <out-copy-dir> receives a CR-stripped copy of every
# drifting non-sibling file that exists in the seed, at its relative path, so tP-emitcheck.sh can compare the base arm's
# drift with the union arm's on a file both arms drift on (STANDING per-target drift: at N's fixup, log/syslog's csproj,
# whose InternalsVisibleTo block is emitted only on the targets where the package has same-package tests, read as
# union-attributable because two seats edited other lines of it). Without it nothing is copied.
root, seed, sentinel, outw, outd = sys.argv[1:6]
outs = sys.argv[6] if len(sys.argv) > 6 else None
oute = sys.argv[7] if len(sys.argv) > 7 else None
outc = sys.argv[8] if len(sys.argv) > 8 else None
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
        if n == '.editorconfig':   # O9: listed as a READING, then handled below like any other written file
            if not os.path.exists(q): edconf.append(rel + ' NEW')
            else: edconf.append(rel + (' same' if open(p, 'rb').read().replace(b'\r', b'') == open(q, 'rb').read().replace(b'\r', b'') else ' changed'))
        sibling = n.endswith('.cs.auto')
        if not sibling:
            written.append(rel)
        if not os.path.exists(q):
            (siblings if sibling else drift).append(rel + ' NEW'); continue
        a = open(p, 'rb').read().replace(b'\r', b'')
        b = open(q, 'rb').read().replace(b'\r', b'')
        if a != b:
            (siblings if sibling else drift).append(rel)
            if outc and not sibling:
                c = os.path.join(outc, *rel.split('/'))
                os.makedirs(os.path.dirname(c), exist_ok=True)
                open(c, 'wb').write(a)
# REVIEW ROUND 1 (trainO/tO-CHANGES.md section 5, F1-2): a COMMITTED .editorconfig the conversion DELETED. The converter removes
# its own per-file warning-entries file when the package's model comes out empty (warningEntries.go writeWarningEntries:
# "an empty model deletes the converter's file"). A deletion writes nothing, so the walk above (files newer than the
# sentinel) never sees it: at O the union converter deletes p2's vendor/golang.org/x/sys/cpu/.editorconfig on darwin
# (c1-darwin-xsys-libc initializes the two fields its one CS0649 section covers), and leg E read 0 while the stale file
# stayed committed (WE arm B then reads it STALE). Every .editorconfig under <seed>/src/core that the emitted root no
# longer holds is listed: in drift as '<rel> DELETED' (tP-emitcheck.sh attributes it by path; tP-regen-apply.py REFUSES
# it by name, since the fixup never applies a deletion: floor 8, tP-README.md MS24) and in the seventh output. Both roots
# are git archives of one commit (tP-emitcheck.sh), so absence after the run is the conversion's own deletion.
for dp, dn, fn in os.walk(os.path.join(seed, 'src', 'core')):
    for n in fn:
        if n != '.editorconfig':
            continue
        rel = os.path.relpath(os.path.join(dp, n), seed).replace(os.sep, '/')
        if not os.path.exists(os.path.join(root, rel)):
            drift.append(rel + ' DELETED'); edconf.append(rel + ' DELETED')
written.sort(); drift.sort(); siblings.sort()
open(outw, 'w', encoding='utf-8', newline='\n').write(''.join(x + '\n' for x in written))
open(outd, 'w', encoding='utf-8', newline='\n').write(''.join(x + '\n' for x in drift))
if outs:
    open(outs, 'w', encoding='utf-8', newline='\n').write(''.join(x + '\n' for x in siblings))
if oute:
    edconf.sort()
    open(oute, 'w', encoding='utf-8', newline='\n').write(''.join(x + '\n' for x in edconf))
print(len(written), len(drift), len(siblings))
