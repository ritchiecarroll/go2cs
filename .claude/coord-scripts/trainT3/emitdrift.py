import os, sys
# usage: emitdrift.py <emitted root> <pristine seed root> <sentinel file> <out-written> <out-drift> [<out-siblings>] [<out-editorconfig>] [<out-copy-dir>]
# TRAIN Q (trainQ/tQ-CHANGES.md Q10; DERIVED 2026-10-06 from trainP/emitdrift.py): DRIFT IS READ FROM BYTES, NEVER FROM
# A MODIFICATION TIME. The i9's answer to P's open question (COORD's notes, 2026-10-04 20:38: 'no always-write flag;
# an emission check compares bytes against the seeded root; the set the converter produced comes from the converter's
# own output list, never mtime'). Through P this script walked the files NEWER than a sentinel and compared those with
# the seed. At Q both arms' converters write a source only when its bytes differ (i9-incremental-cs-writes-r2 landed
# with P), and a marker-bearing source whose resolved bytes are unchanged gets its PREVIOUS time back
# (incrementalWrites.go), so 'newer than the sentinel' is no longer what 'the conversion changed' means. Now EVERY
# file under <emitted>/src/core is compared with its seed, CR-stripped: both roots are git archives of one commit, so
# a difference is the conversion's own, whatever the file's time. <out-written> is still the newer-than-the-sentinel
# list, kept as a READING and for the marker scan; nothing is decided by it. A file whose bytes changed and whose time
# did not is counted (the fourth number printed) and is drift like any other.
# P's note: what <out-written> MEANS for the union arm changed with i9-incremental-cs-writes-r2 (a source whose
# emission equals the seeded file is not written); P kept O's bytes (trainP/tP-CHANGES.md P10).
# TRAIN O (trainO/tO-CHANGES.md O9, N's Q16 applied): a file named .editorconfig (the per-file warning entries the CONVERTER writes
# beside a package's csproj: p2-converter-warning-clears' warningEntries.go, a TRAIN O row) is ORDINARY EMISSION. It is in
# written and, when its CR-stripped bytes differ from the seed (or it is absent there: NEW), in drift, and its drifting
# copy goes to <out-copy-dir> like any other file, so tT3-emitcheck.sh attributes it and tT3-regen-apply.py copies it under
# REGEN_ALLOW. The seventh output still LISTS every .editorconfig a conversion wrote (tagged NEW / changed / same) as a
# READING (tT3-emitcheck.sh's EDITORCONFIG lines); it no longer keeps the file out of written/drift. Review round 1: a
# committed .editorconfig the conversion DELETED is listed too (drift '<rel> DELETED', seventh output tagged DELETED).
# TRAIN N (trainN/tN-CHANGES.md item N7, review round 1): the same file was kept OUT of written/drift, because no row of N's
# list carried the converter change and a conversion writing one was unexpected (a die in N's fixup, a finding in N's leg E).
# Lists files under <emitted>/src/core newer than the sentinel, and those whose CR-stripped bytes differ from the pristine
# seed (or are absent from it: NEW). *.cs.auto REVIEW SIBLINGS are kept OUT of written/drift (the build never compiles
# them) and, when <out-siblings> is given, reported there as their own class: a written sibling whose CR-stripped bytes
# differ from the seed's. TRAIN J onward: every train's fixup refreshes that class from the union arm (ruling 857afcb47e;
# before it, the check excluded siblings entirely and they went stale one converter seat at a time).
# FIXUP STOP 2026-10-03 (COORD, at N): the optional EIGHTH argument <out-copy-dir> receives a CR-stripped copy of every
# drifting non-sibling file that exists in the seed, at its relative path, so tT3-emitcheck.sh can compare the base arm's
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
oldchanged = 0
base = os.path.join(root, 'src', 'core')
for dp, dn, fn in os.walk(base):
    for n in fn:
        p = os.path.join(dp, n)
        newer = os.stat(p).st_mtime > t0
        rel = os.path.relpath(p, root).replace(os.sep, '/')
        q = os.path.join(seed, rel)
        sibling = n.endswith('.cs.auto')
        if newer and not sibling:
            written.append(rel)   # a READING (Q10): the files this run's converter wrote, by time
        new = not os.path.exists(q)
        if new:
            changed = True
        else:
            a = open(p, 'rb').read().replace(b'\r', b'')
            b = open(q, 'rb').read().replace(b'\r', b'')
            changed = a != b
        if n == '.editorconfig' and (newer or changed):   # O9: listed as a READING, then handled below like any other file
            edconf.append(rel + (' NEW' if new else ' changed' if changed else ' same'))
        if not changed:
            continue
        if not newer: oldchanged += 1   # bytes moved under an unmoved time: drift all the same (Q10)
        if new:
            (siblings if sibling else drift).append(rel + ' NEW'); continue
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
# longer holds is listed: in drift as '<rel> DELETED' (tT3-emitcheck.sh attributes it by path; tT3-regen-apply.py REFUSES
# it by name, since the fixup never applies a deletion: floor 8, tT3-README.md MS24) and in the seventh output. Both roots
# are git archives of one commit (tT3-emitcheck.sh), so absence after the run is the conversion's own deletion.
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
print(len(written), len(drift), len(siblings), oldchanged)
