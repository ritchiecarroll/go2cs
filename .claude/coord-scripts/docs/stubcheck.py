"""Every "Moved to" stub's <a id> must sit on a page where that anchor existed at the base (a published anchor
lives on at its old page), and must not also be a heading slug somewhere else on the same page."""
import os, re, subprocess, sys
sys.path.insert(0, r'H:\go2cs-tmp-coord\docs-ref\.claude\coord-scripts\docs')
import ref_common as rc

ROOT = r'H:\go2cs-tmp-coord\docs-ref'
BASES = sys.argv[1:] or ['1dae85e093']
REF = 'docs/ConversionStrategies-Reference'
STUB = re.compile(r'^\s*(?:- )?<a id="([^"]+)"></a>Moved to \[')


def base_anchors(ref):
    out = subprocess.run(['git', '-C', ROOT, 'ls-tree', '-r', '--name-only', ref, '--', REF, REF + '.md'],
                         capture_output=True, text=True, encoding='utf-8').stdout.split()
    res = {}
    for f in out:
        if not f.endswith('.md'):
            continue
        t = subprocess.run(['git', '-C', ROOT, 'show', f'{ref}:{f}'], capture_output=True).stdout.decode('utf-8')
        res[f] = rc.anchors(t.replace('\r\n', '\n').split('\n'))
    return res


bases = {b: base_anchors(b) for b in BASES}
bad = 0
n = 0
TREE = os.environ.get('STUBTREE', ROOT)
for dp, _, fns in os.walk(os.path.join(TREE, REF.replace('/', os.sep))):
    for fn in fns:
        if not fn.endswith('.md'):
            continue
        p = os.path.join(dp, fn)
        rel = os.path.relpath(p, TREE).replace(os.sep, '/')
        lines = rc.Doc(p).lines
        for i, l in enumerate(lines):
            m = STUB.match(l)
            if not m:
                continue
            n += 1
            a = m.group(1)
            ok = any(a in bases[b].get(rel, set()) for b in BASES)
            if not ok:
                bad += 1
                where = sorted({f for b in BASES for f, s in bases[b].items() if a in s})
                print(f'MISPLACED {rel}:{i + 1} #{a[:70]} -- published on {where}')
print(f'{n} stubs checked, {bad} misplaced')
sys.exit(1 if bad else 0)
