# tT4-r2-fixdiff.py -- TRAIN T4 run2: what the gap fix (row 2's 07cacea829) moved, read two ways against run1.
#   python -B tT4-r2-fixdiff.py emit <repo> <run1-fixup-commit> <emission-scratch> <out>
#       per target: every path of attr-<os>.txt, the run2 EMISSION (U-<os> root) vs run1's COMMITTED blob
#   python -B tT4-r2-fixdiff.py tree <repo> <run1-fixup-tree> <run2-fixup-commit> <a..b> <out>
#       every path that differs between the two fixup trees, classified
# Kinds (CR stripped on both sides first; a GoPositionMap reading is tT4-drift-tokens.py's 'pkginfo' check):
#   SAME         byte-equal after the CR strip
#   BLANK-DEL    a .cs file whose run2 lines are run1's with whitespace-only lines DELETED, nothing else (difflib: every
#                non-equal opcode is a delete, every deleted line is blank); the deleted-line count is summed
#   MAP-ONLY     a package_info.cs whose differing lines are all GoPositionMap lines, the (go, cs) key order unchanged
#   DELTA        (tree mode) a path of the row range a..b, holding b's blob in the run2 fixup
#   OTHER        anything else: named, and the verdict fails
import difflib, importlib.util, os, subprocess, sys
from collections import Counter

HERE = os.path.dirname(os.path.abspath(__file__))
spec = importlib.util.spec_from_file_location('dt', os.path.join(HERE, 'tT4-drift-tokens.py'))
dt = importlib.util.module_from_spec(spec); spec.loader.exec_module(dt)

def git(repo, *a):
    return subprocess.run(['git', '-C', repo] + list(a), capture_output=True, check=True).stdout

def blobs(repo, rev, paths):
    p = subprocess.Popen(['git', '-C', repo, 'cat-file', '--batch'], stdin=subprocess.PIPE, stdout=subprocess.PIPE)
    out = {}
    for x in paths:
        p.stdin.write(f'{rev}:{x}\n'.encode('utf-8')); p.stdin.flush()
        h = p.stdout.readline().decode()
        if h.endswith('missing\n'): out[x] = None; continue
        n = int(h.split()[2]); d = p.stdout.read(n); p.stdout.read(1)
        out[x] = d.replace(b'\r', b'').decode('utf-8', errors='surrogateescape')
    p.stdin.close(); p.wait()
    return out

def classify(path, a, b):
    """-> (kind, deleted-blank-lines, detail)"""
    if a is None or b is None: return 'OTHER', 0, 'absent on one side'
    if a == b: return 'SAME', 0, ''
    if path.endswith('/package_info.cs'):
        r = dt.check('pkginfo', a, b)
        return ('MAP-ONLY', 0, '') if r is None else ('OTHER', 0, r)
    if path.endswith('.cs'):
        la, lb = a.split('\n'), b.split('\n')
        nd = 0
        for tag, i1, i2, j1, j2 in difflib.SequenceMatcher(None, la, lb, autojunk=False).get_opcodes():
            if tag == 'equal': continue
            if tag == 'delete' and all(not x.strip() for x in la[i1:i2]): nd += i2 - i1; continue
            return 'OTHER', 0, f'{tag} run1 lines {i1 + 1}-{i2} [{" | ".join(la[i1:i2])[:160]}] -> run2 lines {j1 + 1}-{j2} [{" | ".join(lb[j1:j2])[:160]}]'
        return 'BLANK-DEL', nd, ''
    return 'OTHER', 0, 'not a .cs file'

def emit(repo, r1, es, outp):
    ok = True; lines = []; tot = {}
    for o in ('windows', 'linux', 'darwin'):
        ps = [l.split(' ', 1)[0].strip() for l in open(os.path.join(es, f'attr-{o}.txt'), encoding='utf-8') if l.strip()]
        bl = blobs(repo, r1, ps); c = Counter(); nd = 0
        for p in ps:
            e = dt.rd(os.path.join(es, f'U-{o}', *p.split('/')))
            k, n, d = classify(p, bl[p], e); c[k] += 1; nd += n
            if k != 'SAME': lines.append(f'{k} {o} {p}' + (f' :: -{n} blank' if n else '') + (f' :: {d}' if d else ''))
            if k == 'OTHER': ok = False
        tot[o] = (len(ps), c, nd)
    v = 'FIXDIFF-EMIT ' + ' '.join(f'{o}:paths={t[0]} same={t[1]["SAME"]} blank-del-files={t[1]["BLANK-DEL"]} blank-lines-removed={t[2]} map-only={t[1]["MAP-ONLY"]} other={t[1]["OTHER"]}' for o, t in tot.items()) + f' VERDICT={"OK" if ok else "OTHER"}'
    with open(outp, 'w', encoding='utf-8', newline='\n') as f:
        f.write(f'# emit: run2 emission {es} vs run1 committed {r1}\n' + '\n'.join(lines) + '\n' + v + '\n')
    print(v); return 0 if ok else 1

def tree(repo, t1, c2, rg, outp):
    a, b = rg.split('..')
    delta = set(git(repo, 'diff', '--name-only', a, b).decode().split())
    ns = [l.split('\t') for l in git(repo, 'diff', '--name-status', '--no-renames', t1, c2).decode().splitlines() if l.strip()]
    paths = [x[1] for x in ns]
    b1 = blobs(repo, t1, paths); b2 = blobs(repo, c2, paths)
    c = Counter(); nd = 0; lines = []; ok = True; dok = 0
    for st, p in ns:
        if p in delta:
            k = 'DELTA'
            same = git(repo, 'rev-parse', f'{c2}:{p}') == git(repo, 'rev-parse', f'{b}:{p}')
            if same: dok += 1
            else: k = 'OTHER'; ok = False
            c[k] += 1; lines.append(f'{k} {st} {p}' + ('' if same else ' :: not the range tip blob')); continue
        if not p.startswith('src/core/') or st != 'M':
            c['OTHER'] += 1; ok = False; lines.append(f'OTHER {st} {p} :: outside src/core or not a modification'); continue
        k, n, d = classify(p, b1[p], b2[p]); c[k] += 1; nd += n
        if k in ('OTHER', 'SAME'): ok = ok and k == 'SAME'
        lines.append(f'{k} {st} {p}' + (f' :: -{n} blank' if n else '') + (f' :: {d}' if d else ''))
    miss = sorted(delta - set(paths))
    if miss: ok = False
    v = (f'FIXDIFF-TREE {t1[:10]} -> {c2[:10]}: paths={len(ns)} DELTA={c["DELTA"]} (of {len(delta)} in {rg}; tip blob {dok}; missing {miss}) '
         f'BLANK-DEL={c["BLANK-DEL"]} (blank lines removed {nd}) MAP-ONLY={c["MAP-ONLY"]} SAME={c["SAME"]} OTHER={c["OTHER"]} VERDICT={"OK" if ok else "OTHER"}')
    with open(outp, 'w', encoding='utf-8', newline='\n') as f:
        f.write(f'# tree: run1 fixup tree {t1} vs run2 fixup {c2}, delta {rg}\n' + '\n'.join(lines) + '\n' + v + '\n')
    print(v); return 0 if ok else 1

def plants(repo, r1):
    # floor 13: classify() on planted edits of two of run1's committed files; each plant must read its stated kind.
    cs, pk = 'src/core/archive/tar/common.cs', 'src/core/archive/zip/package_info.cs'
    b = blobs(repo, r1, [cs, pk]); a, m = b[cs], b[pk]
    la = a.split('\n'); bi = next(i for i, x in enumerate(la) if not x.strip() and i > 5)
    ci = next(i for i, x in enumerate(la) if x.strip().startswith('return ') and i > 5)
    lm = m.split('\n'); mi = next(i for i, x in enumerate(lm) if x.startswith('[assembly:') and 'GoPositionMap(' in x)
    ni = next(i for i, x in enumerate(lm) if x.startswith('[assembly:') and 'GoPositionMap' not in x)
    def j(L): return '\n'.join(L)
    def mapedit(x): k = x.rfind(')'); return x[:k] + ' ' + x[k:]
    cases = [
        ('one blank line deleted', cs, a, j(la[:bi] + la[bi + 1:]), 'BLANK-DEL'),
        ('a code line deleted', cs, a, j(la[:ci] + la[ci + 1:]), 'OTHER'),
        ('a blank line inserted', cs, a, j(la[:ci] + [''] + la[ci:]), 'OTHER'),
        ('a code token changed', cs, a, j(la[:ci] + [la[ci].replace('return ', 'return Planted_', 1)] + la[ci + 1:]), 'OTHER'),
        ('unchanged', cs, a, a, 'SAME'),
        ('a GoPositionMap line re-encoded', pk, m, j(lm[:mi] + [mapedit(lm[mi])] + lm[mi + 1:]), 'MAP-ONLY'),
        ('a non-map attribute line changed', pk, m, j(lm[:ni] + [lm[ni] + ' '] + lm[ni + 1:]), 'OTHER'),
    ]
    ok = True
    for name, p, x, y, want in cases:
        k, n, d = classify(p, x, y); good = k == want and (k != 'BLANK-DEL' or n == 1)
        ok = ok and good
        print(f'PLANT {name}: {k}{" -" + str(n) if n else ""} (EXPECT {want}) {"ok" if good else "WRONG"}' + (f' :: {d[:120]}' if d else ''))
    print(f'FIXDIFF-PLANTS {"OK" if ok else "FAILED"} ({len(cases)} plants)')
    return 0 if ok else 1

if __name__ == '__main__':
    try: sys.stdout.reconfigure(encoding='utf-8', errors='replace')
    except Exception: pass
    m = sys.argv[1] if len(sys.argv) > 1 else ''
    if m == 'plants' and len(sys.argv) == 4: sys.exit(plants(*sys.argv[2:]))
    if m == 'emit' and len(sys.argv) == 6: sys.exit(emit(*sys.argv[2:]))
    if m == 'tree' and len(sys.argv) == 7: sys.exit(tree(*sys.argv[2:]))
    print(__doc__ or 'usage'); sys.exit(2)
