# Init-statement scope census driver: builds the per-target file sets from each package's
# GoPositionMap records, runs the Go-side (gocensus) and C#-side (cscensus) instruments over them,
# and reconciles the two per file, joining items in source order.
#   py -I -X utf8 census.py <repo root> <out dir> [--plant]
import json, os, re, subprocess, sys, collections

ROOT = sys.argv[1].rstrip('/\\') + '/'
OUT = sys.argv[2].rstrip('/\\') + '/'
PLANT = '--plant' in sys.argv
HERE = os.path.dirname(os.path.abspath(__file__)).replace('\\', '/') + '/'
GOROOT = subprocess.run(['go', 'env', 'GOROOT'], capture_output=True, text=True).stdout.strip().replace('\\', '/')
CORE = ROOT + 'src/core/'
BEH = ROOT + 'src/tests/Behavioral/'
TARGETS = ('windows', 'linux', 'darwin')
MAP = re.compile(r'GoPositionMap\("([^"]+)", "([^"]+)"')
MARK = '[module: GoManualConversion]'
os.makedirs(OUT, exist_ok=True)


def maps(path):
    try:
        return MAP.findall(open(path, encoding='utf-8-sig').read())
    except FileNotFoundError:
        return None


# ---- the populations: cs file -> go file, and which targets compile each cs file
pairs = {}                                   # cs abs path -> go abs path
targets_of = collections.defaultdict(set)    # cs abs path -> {targets} (or {'golden'})
skipped = collections.Counter()
for dirpath, dirs, files in os.walk(CORE):
    dirs[:] = [d for d in dirs if d not in ('bin', 'obj', 'Generated', 'testdata')]
    dp = dirpath.replace('\\', '/') + '/'
    if not any(f.endswith('.csproj') and not f.endswith('.tests.csproj') for f in files):
        continue
    l3 = [t for t in TARGETS if os.path.exists(dp + t + '/package_info.cs')]
    for t in TARGETS:
        mf = dp + t + '/package_info.cs' if l3 else dp + 'package_info.cs'
        if l3 and t not in l3:
            continue                          # a platform-exclusive L3 package is absent on t
        for go, cs in maps(mf) or []:
            csp = dp + t + '/' + cs if l3 and os.path.exists(dp + t + '/' + cs) else dp + cs
            if not os.path.exists(csp):
                skipped['cs-missing'] += 1
                continue
            if MARK in open(csp, encoding='utf-8-sig').read():
                skipped['hand-owned'] += 1
                continue
            gop = GOROOT + '/src/' + go
            if not os.path.exists(gop):
                skipped['go-missing'] += 1
                continue
            pairs[csp] = gop
            targets_of[csp].add(t)
for dirpath, dirs, files in os.walk(BEH):
    dirs[:] = [d for d in dirs if d not in ('bin', 'obj', 'Generated')]
    dp = dirpath.replace('\\', '/') + '/'
    if 'package_info.cs' not in files:
        continue
    for go, cs in maps(dp + 'package_info.cs') or []:
        csp = dp + cs + '.target'
        if not os.path.exists(csp) or not os.path.exists(dp + go):
            skipped['golden-missing'] += 1
            continue
        pairs[csp] = dp + go
        targets_of[csp].add('golden')

keys = sorted(pairs)
key_of = {p: str(i) for i, p in enumerate(keys)}

if PLANT:
    # RED-FIRST: plant one init block in a copy of io.go and io.cs; both instruments must count it
    import shutil
    pd = OUT + 'plant/'
    os.makedirs(pd, exist_ok=True)
    src_cs = CORE + 'io/io.cs'
    src_go = GOROOT + '/src/io/io.go'
    pc, pg = pd + 'io.cs', pd + 'io.go'
    cs_text = open(src_cs, encoding='utf-8-sig').read()
    go_text = open(src_go, encoding='utf-8').read()
    cs_text = cs_text.replace('    return w.Write(slice<byte>(s));\n',
                              '    {\n        var (bw, ok) = w._<ByteWriter>(ᐧ); if (ok) {\n            _ = bw;\n        }\n    }\n    return w.Write(slice<byte>(s));\n', 1)
    go_text = go_text.replace('\treturn w.Write([]byte(s))\n',
                              '\tif bw, ok := w.(ByteWriter); ok {\n\t\t_ = bw\n\t}\n\treturn w.Write([]byte(s))\n', 1)
    open(pc, 'w', encoding='utf-8').write(cs_text)
    open(pg, 'w', encoding='utf-8').write(go_text)
    for p in list(pairs):
        if p == src_cs:
            del pairs[p]
            pairs[pc] = pg
            targets_of[pc] = targets_of.pop(p)
    keys = sorted(pairs)
    key_of = {p: str(i) for i, p in enumerate(keys)}


def run(exe, inp):
    r = subprocess.run(exe, input=inp, capture_output=True, text=True, encoding='utf-8')
    if r.returncode != 0:
        sys.exit(f'instrument failed rc={r.returncode}: {r.stderr[-2000:]}')
    return [json.loads(l) for l in r.stdout.splitlines() if l.strip()], r.stderr


win = lambda p: p.replace('/', '\\')
go_items, go_err = run([HERE + 'gocensus.exe'], ''.join(f'{key_of[p]}\t{win(pairs[p])}\n' for p in keys))
cs_items, cs_err = run(['dotnet', HERE + 'cscensus/bin/Release/net10.0/cscensus.dll'],
                       ''.join(f'{key_of[p]}\t{win(p)}\n' for p in keys))
open(OUT + 'go-stderr.txt', 'w', encoding='utf-8').write(go_err)
open(OUT + 'cs-stderr.txt', 'w', encoding='utf-8').write(cs_err)

by_go = collections.defaultdict(list)
by_cs = collections.defaultdict(list)
for it in go_items:
    by_go[it['key']].append(it)
for it in cs_items:
    by_cs[it['key']].append(it)

strip = lambda n: re.sub(r'Δ\d*$', '', n.lstrip('@').lstrip('Δ'))
joined, mismatched = [], []
for p in keys:
    k = key_of[p]
    g, c = by_go.get(k, []), by_cs.get(k, [])
    gi = [x for x in g]
    # the else-if blocks are the cs 'elseif' form; every other go item is a cs 'bare' block
    if len(gi) != len(c) or any((x['item'] == 'elseif') != (y['form'] == 'elseif') for x, y in zip(gi, c)):
        mismatched.append({'cs': p, 'go': pairs[p], 'go_n': len(gi), 'cs_n': len(c),
                           'go_items': [(x['line'], x['item']) for x in gi],
                           'cs_items': [(y['line'], y['form'], y['lastKind']) for y in c]})
        continue
    for x, y in zip(gi, c):
        # name check: every go LHS name that the init declares appears among the block's declared names
        ok = True
        if x['item'] != 'block' and x['define']:
            dn = {strip(d) for d in y['declared']}
            ok = all(n == '_' or strip(n) in dn for n in x['names'])
        joined.append({'cs': p, 'targets': sorted(targets_of[p]), 'go': x, 'cs_item': y, 'name_ok': ok})

json.dump({'files': len(keys), 'skipped': skipped, 'mismatched': mismatched}, open(OUT + 'reconcile.json', 'w', encoding='utf-8'), indent=1)
with open(OUT + 'joined.jsonl', 'w', encoding='utf-8') as f:
    for j in joined:
        f.write(json.dumps(j, ensure_ascii=False) + '\n')
print(f'files {len(keys)} (skipped {dict(skipped)}); go items {len(go_items)}; cs items {len(cs_items)}; '
      f'joined {len(joined)}; mismatched files {len(mismatched)}; name check failures {sum(not j["name_ok"] for j in joined)}')
