import io, os, re, sys, json

# ⚠ NO LOOKBEHIND ON THE DOT. The first version wrote (?<![A-Za-z0-9_.]) and therefore excluded
# `Proc(...).Call(...)` -- the PRIMARY shape COORD named -- while still reporting a plausible count.
FUNNELS = re.compile(r'(?<![A-Za-z0-9_])(Syscall(?:6|9|12|15|18|N)?|Call)\s*\(')
ADDR = 'Ꮡ'   # Ꮡ

def strip_comments(t):
    # crude but adequate: kill // to EOL and /* */; keep newlines so line numbers survive
    out = []
    i, n = 0, len(t)
    while i < n:
        if t.startswith('//', i):
            j = t.find('\n', i)
            if j < 0: break
            out.append(' ' * (j - i)); i = j
        elif t.startswith('/*', i):
            j = t.find('*/', i)
            if j < 0: break
            seg = t[i:j+2]
            out.append(''.join(c if c == '\n' else ' ' for c in seg)); i = j + 2
        else:
            out.append(t[i]); i += 1
    return ''.join(out)

def args_of(t, open_paren):
    depth, i, n = 0, open_paren, len(t)
    while i < n:
        c = t[i]
        if c == '(': depth += 1
        elif c == ')':
            depth -= 1
            if depth == 0: return t[open_paren+1:i], i
        i += 1
    return None, None

def split_args(s):
    parts, depth, cur = [], 0, []
    for c in s:
        if c in '([{': depth += 1
        elif c in ')]}': depth -= 1
        if c == ',' and depth == 0:
            parts.append(''.join(cur)); cur = []
        else:
            cur.append(c)
    parts.append(''.join(cur))
    return [p.strip() for p in parts]

def walk(roots, skip_dirs):
    for root in roots:
        for dp, dn, fn in os.walk(root):
            dn[:] = [d for d in dn if d not in skip_dirs]
            for f in fn:
                if f.endswith('.cs'):
                    yield os.path.join(dp, f)

rows = []
roots = sys.argv[1:]
for path in walk(roots, {'obj', 'bin', 'Generated', 'linux', 'darwin'}):
    raw = io.open(path, 'r', encoding='utf-8', errors='replace', newline='').read()
    t = strip_comments(raw)
    for m in FUNNELS.finditer(t):
        callee = m.group(1)
        body, close = args_of(t, m.end() - 1)
        if body is None: continue
        # NOTE: the address-of may not be in the parens at all (see the indirection block below),
        # so this is no longer a filter -- the decision moves to the classification below.
        line = t.count('\n', 0, m.start()) + 1
        # ⚠ ONE LEVEL OF INDIRECTION IS REQUIRED, not optional. The dominant emission shape for a
        # syscall wrapper is `var _p0 = (uintptr)Ꮡx;` followed by `Syscall(proc, _p0, ...)`, so a
        # predicate that only looks INSIDE the call's parens reports its own predicate: it found 16
        # where a plain line grep found 33 and there are 133 address-of-to-uintptr casts on this
        # flavour. Each simple-identifier argument is resolved back to its most recent assignment in
        # the preceding text and tested for an address-of. Arguments that cannot be resolved are
        # COUNTED AS UNKNOWN rather than as absent.
        argl = split_args(body)
        idx = [i for i, a in enumerate(argl) if ADDR in a]
        window = t[max(0, m.start() - 4000):m.start()]
        indirect, unknown = [], []
        for i, a in enumerate(argl):
            if i in idx:
                continue
            if re.fullmatch(r'[A-Za-z_][A-Za-z0-9_]*', a):
                asn = re.findall(r'(?:var\s+|uintptr\s+|nuint\s+)?' + re.escape(a) + r'\s*=\s*([^;]*);', window)
                if asn:
                    if ADDR in asn[-1]:
                        indirect.append(i)
                else:
                    unknown.append(i)
            elif ADDR not in a and re.search(r'[A-Za-z_][A-Za-z0-9_]*', a):
                pass
        if not idx and not indirect:
            continue
        # the receiver text just before the call, to name the API for a Proc(...).Call
        pre = t[max(0, m.start() - 160):m.start()]
        api = ''
        pm = re.findall(r'Proc\(([^)]*)\)', pre)
        if pm: api = pm[-1]

        # ⚠ A BARE `.Call(` IS NOT A SYSCALL FUNNEL. Removing the dot-lookbehind (needed for
        # Proc(...).Call) let Go's OWN methods named Call in -- net/rpc's client and service calls --
        # and they arrived with plausible-looking address-of arguments (Ꮡcodec, Ꮡargs, Ꮡreply): 15 of
        # 43 sites, all in net/rpc and net/rpc/jsonrpc, none of them native at all. One
        # over-restriction traded for one over-match, and only the per-package breakdown showed it.
        # A Call site therefore counts only when its receiver chain names a Proc.
        if callee == 'Call' and not api:
            continue
        rows.append({'file': path, 'line': line, 'callee': callee, 'api': api,
                     'argc': len(argl), 'direct': idx, 'indirect': indirect,
                     'unknown': unknown,
                     'args': [argl[i][:90] for i in (idx + indirect)]})

print("SITES (funnel calls whose argument list contains an address-of):", len(rows))
byc = {}
for r in rows: byc[r['callee']] = byc.get(r['callee'], 0) + 1
print("by funnel:", json.dumps(byc, sort_keys=True))
print("direct-arg sites  :", sum(1 for r in rows if r['direct']))
print("indirect-only     :", sum(1 for r in rows if r['indirect'] and not r['direct']))
print("args UNRESOLVED   :", sum(len(r['unknown']) for r in rows), "(counted as unknown, never as absent)")
apis = sorted({r['api'] for r in rows if r['api']})
print("named APIs        :", len(apis))
print("EnumTimeFormatsEx present (POSITIVE CONTROL):", any('EnumTimeFormatsEx' in (r['api'] or '') or 'enumTimeFormatsEx' in (r['api'] or '') for r in rows))
io.open('c2-table2-sites.json', 'w', encoding='utf-8').write(json.dumps(rows, indent=1, ensure_ascii=False))
print("wrote c2-table2-sites.json")
