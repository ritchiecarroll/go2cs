# tN-regen-apply.py -- TRAIN N fixup step 4: copy the union converter's own emission over the corpus files the
# assembled union does not carry. DERIVED 2026-10-03 from trainM/tM-regen-apply.py, names only (NEW in M; TRAIN K's
# tK-k6check.py did this for two named files;
# this takes its file list from the emission check instead of from a name). NOT RUN by its author: py_compile only.
#
#   python tN-regen-apply.py check|apply <worktree> <scratch> <out-list> <siblings 0|1> <allow ERE>
#
# <scratch> is tN-emitcheck.sh's scratch after a KEEP_U_ROOTS=1 run at the ASSEMBLED union: it holds, per target
# (windows, linux, darwin), the seeded root U-<os> the union's converter wrote into, attr-<os>.txt (the union-attributable
# drift: written by the union arm, different from the union's own tree, and not standing drift of the base), sa-<os>
# (the *.cs.auto review siblings only the union arm rewrites) and written-U-<os>.txt (everything that target's run wrote).
#
# For every listed path the rules are checked for ALL paths before ONE byte is written:
#   1. it matches <allow> (the ruled set; a path outside it is a seat's uncommitted footprint nobody ruled on);
#   2. it exists in the worktree (a path the converter would ADD is a registration question, not a refresh);
#   3. it is not hand-owned (golib, the files directly in testing/, unsafe, *_impl.cs, a line-anchored
#      [module: GoManualConversion]);
#   3b. (verify round 1) the BASE arm does not drift on it too (dm-<os>, the base arm's drift list the check keeps in
#      the scratch): for that class the unit is the union's own HUNKS, and a whole-file copy would carry the base's
#      standing drift into a commit that says it carries none. Refused, for a hand-applied hunk;
#   3c. (verify round 1) the emitted bytes hold no unresolved deferred marker (a raced conversion: floor 1);
#   4. every target whose run WROTE the path emitted the same bytes, CR-stripped (a flat file that differs per target
#      wants an L3 split; a GOOS-foldered file is written by one target only);
#   5. the emission really differs from the worktree's file, CR-stripped (else there is nothing to apply).
# check = rules only. apply = rules, then the emitted bytes are copied VERBATIM (the converter's own line endings) and
# <out-list> gets one repo-relative path per line. Exit 1 on any refused path (nothing written), 2 on a usage or
# evidence problem. 0 listed paths is a valid reading: it prints so and writes an empty list.
import os, re, sys

TARGETS = ('windows', 'linux', 'darwin')
MANUAL = re.compile(rb'^\[module: (go\.)?GoManualConversion\]', re.M)
# Verify round 1. The hand-owned testing package is the files DIRECTLY in src/core/testing/; its sub-packages (fstest,
# iotest, quick, slogtest, internal/testdeps) are converted (TRAIN L's windows arm wrote 13 files there and not testing.cs).
HANDOWN_TESTING = re.compile(r'^src/core/testing/[^/]+$')
# The converter's two deferred-marker prefixes, as UTF-8 bytes (dynamicTypeOperations.go:25, adapterNameCollisions.go:41):
# a raced conversion exits 0 and leaves one file holding an unresolved one (floor 1's signature).
MARKERS = (b'\xc2\xabDYNTYPE:', b'\xc2\xabADAPTER:')   # the left guillemet as its two UTF-8 bytes (this file stays ASCII)

def readlist(p):
    if not os.path.exists(p): return None
    return [l.split(' ', 1)[0].strip() for l in open(p, encoding='utf-8', errors='surrogateescape') if l.strip()]

def nocr(p):
    with open(p, 'rb') as f: return f.read().replace(b'\r', b'')

def main(argv):
    if len(argv) != 6 or argv[0] not in ('check', 'apply'):
        print('REGEN usage: check|apply <worktree> <scratch> <out-list> <siblings 0|1> <allow ERE>'); return 2
    mode, wt, scratch, outp, sib, allow = argv
    allow_rx = re.compile(allow)
    where, written, base_drift = {}, {}, {}
    for os_ in TARGETS:
        a = readlist(os.path.join(scratch, f'attr-{os_}.txt'))
        w = readlist(os.path.join(scratch, f'written-U-{os_}.txt'))
        dm = readlist(os.path.join(scratch, f'dm-{os_}'))
        if a is None or w is None or dm is None or not os.path.isdir(os.path.join(scratch, f'U-{os_}')):
            print(f'REGEN evidence incomplete for {os_} under {scratch} (attr list, written list, the base arm\'s drift list dm-{os_} or the kept root is missing)'); return 2
        s = (readlist(os.path.join(scratch, f'sa-{os_}')) or []) if sib == '1' else []
        written[os_] = set(w)
        base_drift[os_] = set(dm)
        for p in a + s: where.setdefault(p, []).append(os_)
    plan, bad = [], []
    for p in sorted(where):
        issibling = p.endswith('.cs.auto')
        src = os.path.join(scratch, f'U-{where[p][0]}', *p.split('/'))
        dst = os.path.join(wt, *p.split('/'))
        why = None
        if not allow_rx.search(p): why = 'outside the allowed set'
        elif not os.path.isfile(dst): why = 'absent from the worktree (a NEW emitted path is a registration question)'
        elif not os.path.isfile(src): why = f'listed for {where[p][0]} but absent from its kept root'
        elif not issibling and (p.startswith(('src/core/golib/', 'src/core/unsafe/')) or HANDOWN_TESTING.match(p) or p.endswith('_impl.cs')
                                or MANUAL.search(nocr(dst))): why = 'hand-owned: a conversion must not have written it'
        elif not issibling and any(p in base_drift[o] for o in where[p]):
            why = ('the base arm drifts on this file too (standing drift): a whole-file copy would carry the base\'s drift into the fixup; '
                   'apply the union\'s own hunks by hand, never the whole file (corpus-reconvert skill, section 3)')
        elif not issibling and any(m in nocr(src) for m in MARKERS):
            why = 'unresolved deferred marker in the emission (a raced or interrupted conversion, floor 1): re-run the emission check alone on the box'
        else:
            new = nocr(src)
            # A review sibling is not in any written list (emitdrift keeps *.cs.auto out of it): compare it across the
            # targets that LIST it. Every other path: across every target whose run wrote it.
            peers = where[p] if issibling else [o for o in TARGETS if p in written[o]]
            for o in peers:
                q = os.path.join(scratch, f'U-{o}', *p.split('/'))
                if not os.path.isfile(q) or nocr(q) != new:
                    why = f'the {o} run wrote different bytes than the {where[p][0]} run (per-target emission of one path)'; break
            if why is None and nocr(dst) == new: why = 'the emission equals the worktree file (nothing to apply)'
        if why: bad.append((p, why))
        else: plan.append((p, src, dst, issibling))
    for p, why in bad: print(f'REGEN REFUSED {p}: {why}')
    nsib = sum(1 for x in plan if x[3])
    print(f'REGEN {mode}: listed={len(where)} applicable={len(plan)} (review siblings among them: {nsib}) refused={len(bad)} allow=/{allow}/ siblings={"on" if sib == "1" else "off"}')
    if bad: return 1
    for p, src, dst, issibling in plan:
        print(f'REGEN {"would apply" if mode == "check" else "applied"} {p} (targets: {",".join(where[p])}{"; review sibling" if issibling else ""})')
        if mode == 'apply':
            with open(src, 'rb') as f: data = f.read()
            with open(dst, 'wb') as f: f.write(data)
    if mode == 'apply':
        with open(outp, 'w', encoding='utf-8', newline='\n') as f: f.write(''.join(x[0] + '\n' for x in plan))
    return 0

if __name__ == '__main__':
    try: sys.stdout.reconfigure(encoding='utf-8', errors='replace')
    except Exception: pass
    sys.exit(main(sys.argv[1:]))
