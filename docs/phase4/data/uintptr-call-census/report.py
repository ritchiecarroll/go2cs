import io, os, re, sys
# report.py <worktree> <ucc dir> <readings dir> -- reads run-census.sh's <flavour>.ucc files and writes, per flavour,
# readings/<flavour>.tsv (file, line, class, consumer, callee, kind; paths relative to src/core), then prints:
#   1. class x file-kind tallies per flavour;
#   2. BACK-TO-POINTER sites whose callee is not a Pointer.FromRef conversion, production vs test files;
#   3. the TEXT census of hand-written files, each hit reconciled with the typed records.
# File kind: manual = a LINE beginning `[module: GoManualConversion]` or `[module: go.GoManualConversion]` (most
# hand-owns spell it qualified; the converter's placeholder comments quote the marker in prose, so a substring test
# admits converted files); impl = *_impl.cs; handowned = src/core/unsafe or
# src/core/testing; converted = everything else.
sys.stdout.reconfigure(encoding='utf-8')
W, U, OUT = sys.argv[1], sys.argv[2], sys.argv[3]
CORE = os.path.join(W, 'src', 'core')
FLAVOURS = ('windows-amd64', 'linux-amd64', 'darwin-amd64', 'linux-arm64')
UCC = re.compile(r'^(?P<file>.+?)\((?P<line>\d+),\d+\): warning UCC001: UCC\t(?P<cls>[^\t]+)\t(?P<callee>[^\t]+)\t(?P<text>[^\t]*)\t')
CALL = re.compile(r'\(uintptr\)\s*\(*\s*(?:[@\w\u0080-￿]+(?:<[^<>()]*>)?\s*\.\s*)*[@\w\u0080-￿]+(?:<[^<>()]*>)?\s*\(')
_kinds = {}


def kind_of(rel):
    if rel not in _kinds:
        try:
            s = io.open(os.path.join(CORE, rel), encoding='utf-8', errors='replace').read()
        except OSError:
            s = ''
        _kinds[rel] = ('manual' if any(re.match(r'\[module:\s*(go\.)?GoManualConversion\]', l.strip()) for l in s.splitlines())
                       else 'impl' if rel.endswith('_impl.cs')
                       else 'handowned' if rel.split('/')[0] in ('unsafe', 'testing') else 'converted')
    return _kinds[rel]


typed = {}
os.makedirs(OUT, exist_ok=True)
print('1. Classes by file kind')
for fl in FLAVOURS:
    rows = []
    for raw in io.open(os.path.join(U, fl + '.ucc'), encoding='utf-8', errors='replace'):
        m = UCC.match(raw.strip())
        if not m:
            continue
        rel = m['file'].replace('\\', '/').split('/src/core/')[-1]
        cls, _, consumer = m['cls'].partition(':')
        rows.append((rel, int(m['line']), cls, consumer, m['callee'], kind_of(rel)))
        typed.setdefault((rel, int(m['line'])), set()).add((fl, cls))
    rows.sort()
    with io.open(os.path.join(OUT, fl + '.tsv'), 'w', encoding='utf-8', newline='\n') as f:
        f.write('file\tline\tclass\tconsumer\tcallee\tkind\n')
        for r in rows:
            f.write('\t'.join(map(str, r)) + '\n')
    tally = {}
    for r in rows:
        tally[(r[5], r[2])] = tally.get((r[5], r[2]), 0) + 1
    print(f'   {fl}: {len(rows)} sites  ' + '  '.join(f'{k}/{c}={n}' for (k, c), n in sorted(tally.items())))

print('2. BACK-TO-POINTER sites whose callee is not a Pointer.FromRef conversion')
for fl in FLAVOURS:
    split = {}
    for raw in io.open(os.path.join(OUT, fl + '.tsv'), encoding='utf-8'):
        f = raw.rstrip('\n').split('\t')
        if f[2] != 'BACK-TO-POINTER' or 'Pointer.FromRef' in f[4]:
            continue
        side = 'test' if f[0].endswith('_test.cs') else 'production'
        split[(side, f[5], f[0])] = split.get((side, f[5], f[0]), 0) + 1
    total = sum(split.values())
    print(f'   {fl}: {total}' + ''.join(f'\n      {side} {k} {rel} {n}' for (side, k, rel), n in sorted(split.items())))

print('3. Text census of hand-written files, reconciled')
counts = {}
for d, dirs, fs in os.walk(CORE):
    dirs[:] = [x for x in dirs if x not in ('bin', 'obj', 'Generated')]
    rel_d = os.path.relpath(d, CORE).replace('\\', '/')
    if rel_d.startswith('golib'):
        continue
    for name in fs:
        if not name.endswith('.cs'):
            continue
        rel = (rel_d + '/' + name) if rel_d != '.' else name
        k = kind_of(rel)
        if k == 'converted':
            continue
        counts['files'] = counts.get('files', 0) + 1
        for i, line in enumerate(io.open(os.path.join(CORE, rel), encoding='utf-8', errors='replace'), 1):
            for m in CALL.finditer(line.split('//')[0]):
                hit = typed.get((rel, i))
                state = ('typed:' + ','.join(sorted({c for _, c in hit}))) if hit else 'not an unsafe.Pointer call'
                counts[state.split(':')[0]] = counts.get(state.split(':')[0], 0) + 1
                print(f'   {k:9} {rel}:{i}  {state}  {m.group(0).strip()[:60]}')
print(f'   {counts}')
