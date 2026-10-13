# tT4-drift-tokens.py -- TRAIN T4, step 4 RED: is the union's corpus drift comment and whitespace ONLY?
#
# COORD 2026-10-12: row 2 (claude/c1-comment-parity-g, seats D, E1, L, H, F, C, E2, A-scope, G) changes the converter's
# -comments emission and committed no src/core footprint. Its footprint is admitted by an explicit REGEN_ALLOW ONLY if
# every drifted (target, path) pair is proven comment / whitespace only. This is that proof.
#
#   python -B tT4-drift-tokens.py all  <worktree> <scratch> <out-report>   every path of attr-<os>.txt: HEAD blob vs U-<os> root
#   python -B tT4-drift-tokens.py pair <committed-file> <emitted-file> [kind]   one pair (kind: cs | pkginfo; default by name)
#
# Rules (CR stripped on both sides first: .gitattributes pins converter output to CRLF at checkout, the blob is LF):
#  * *.cs / *.cs.auto: C# comments (// to end of line, /* ... */) are removed, NEVER inside a literal: regular "..." with
#    escapes, verbatim @"...", raw """...""" (any quote count, one- and multi-line), interpolated $"...", $@"...", @$"...",
#    $$"""...""" (holes scanned as code, nested literals and braces included), char literals '...'. A literal is ONE
#    token holding its exact text. The rest splits on whitespace and punctuation (identifier/number runs, one token per
#    punctuation char). The token sequences must be IDENTICAL. A lex that ends inside a literal, a comment or a hole, or a
#    token stream whose (), [], {} do not balance, is a FAILURE (a mis-lex could hide code in a "comment").
#  * package_info.cs: every differing line (either side) must be a GoPositionMap attribute line
#    ([assembly: go.GoPositionMap(...)] or [assembly: global::go.GoPositionMap(...)]); the sequence of
#    (go file, cs file) keys of the map lines must be identical on both sides.
#  * any other file kind: FAIL (named), unless the bytes are equal after CR stripping and whitespace collapsing.
import difflib, os, re, subprocess, sys

IDCH = re.compile(r'[A-Za-z0-9_]')

class LexError(Exception):
    pass

class Lexer:
    def __init__(self, s):
        self.s = s; self.n = len(s)
        self.toks = []   # (text, line)

    def line(self, i):
        return self.s.count('\n', 0, i) + 1

    def string_at(self, i):
        """If a string literal starts at i, return its end index (exclusive); else None."""
        s, n = self.s, self.n
        j = i; dollars = 0; verb = False
        while j < n and s[j] in '$@':
            if s[j] == '$': dollars += 1
            else:
                if verb: return None
                verb = True
            j += 1
        if j >= n or s[j] != '"': return None
        # count the opening quote run
        q = 0
        while j + q < n and s[j + q] == '"': q += 1
        if q >= 3 and not verb:
            return self.raw(j, q, dollars)
        if verb:
            return self.verbatim(j + 1, dollars > 0)
        if q == 2:
            return j + 2   # "" (or $"" ) empty
        return self.regular(j + 1, dollars > 0)

    def regular(self, j, interp):
        s, n = self.s, self.n
        while j < n:
            c = s[j]
            if c == '\\': j += 2; continue
            if c == '"': return j + 1
            if c == '\n': raise LexError(f'newline inside a regular string literal at line {self.line(j)}')
            if interp and c == '{':
                if j + 1 < n and s[j + 1] == '{': j += 2; continue
                j = self.hole(j + 1, 1); continue
            if interp and c == '}':
                if j + 1 < n and s[j + 1] == '}': j += 2; continue
                raise LexError(f'lone }} in an interpolated string at line {self.line(j)}')
            j += 1
        raise LexError('unterminated regular string literal at EOF')

    def verbatim(self, j, interp):
        s, n = self.s, self.n
        while j < n:
            c = s[j]
            if c == '"':
                if j + 1 < n and s[j + 1] == '"': j += 2; continue
                return j + 1
            if interp and c == '{':
                if j + 1 < n and s[j + 1] == '{': j += 2; continue
                j = self.hole(j + 1, 1); continue
            if interp and c == '}':
                if j + 1 < n and s[j + 1] == '}': j += 2; continue
                raise LexError(f'lone }} in an interpolated verbatim string at line {self.line(j)}')
            j += 1
        raise LexError('unterminated verbatim string literal at EOF')

    def raw(self, j, q, dollars):
        s, n = self.s, self.n
        k = j + q
        close = '"' * q
        while k < n:
            if s.startswith(close, k):
                # the closing delimiter is exactly q quotes; a longer run is an error in C#
                r = 0
                while k + r < n and s[k + r] == '"': r += 1
                if r > q: raise LexError(f'quote run of {r} > {q} inside a raw string at line {self.line(k)}')
                return k + q
            if dollars and s[k] == '{':
                r = 0
                while k + r < n and s[k + r] == '{': r += 1
                if r >= dollars:
                    k = self.hole(k + r, dollars); continue
                k += r; continue
            k += 1
        raise LexError('unterminated raw string literal at EOF')

    def hole(self, j, nclose):
        """Scan an interpolation hole's code from j to its closing brace(s); return the index after them."""
        end = self.code(j, in_hole=True)
        # self.code stops AT the first '}' of depth 0
        s = self.s
        for _ in range(nclose):
            if end >= self.n or s[end] != '}': raise LexError(f'interpolation hole not closed by {nclose} brace(s) at line {self.line(end)}')
            end += 1
        return end

    def code(self, i, in_hole=False):
        s, n = self.s, self.n
        depth = 0
        out = [] if in_hole else self.toks
        while i < n:
            c = s[i]
            if c.isspace(): i += 1; continue
            if c == '/' and i + 1 < n and s[i + 1] == '/':
                e = s.find('\n', i)
                i = n if e < 0 else e
                continue
            if c == '/' and i + 1 < n and s[i + 1] == '*':
                e = s.find('*/', i + 2)
                if e < 0: raise LexError(f'unterminated /* comment from line {self.line(i)}')
                i = e + 2; continue
            if c in '$@"':
                e = self.string_at(i)
                if e is not None:
                    out.append((s[i:e], i)); i = e; continue
            if c == "'":
                j = i + 1
                if j < n and s[j] == '\\': j += 2
                else: j += 1
                while j < n and s[j] != "'" and s[j] != '\n': j += 1
                if j >= n or s[j] != "'": raise LexError(f'unterminated char literal at line {self.line(i)}')
                out.append((s[i:j + 1], i)); i = j + 1; continue
            if IDCH.match(c) or ord(c) > 127:
                j = i + 1
                while j < n and (IDCH.match(s[j]) or (ord(s[j]) > 127 and not s[j].isspace())): j += 1
                out.append((s[i:j], i)); i = j; continue
            if in_hole:
                if c in '({[': depth += 1
                elif c in ')]': depth -= 1
                elif c == '}':
                    if depth == 0: return i
                    depth -= 1
            out.append((c, i)); i += 1
        if in_hole: raise LexError('interpolation hole runs to EOF')
        return i

def tokens(text):
    lx = Lexer(text)
    lx.code(0)
    toks = lx.toks
    bal = {'(': 0, '[': 0, '{': 0}
    pair = {')': '(', ']': '[', '}': '{'}
    for t, _ in toks:
        if t in bal: bal[t] += 1
        elif t in pair:
            bal[pair[t]] -= 1
            if bal[pair[t]] < 0: raise LexError(f'unbalanced {t} in the token stream')
    if any(bal.values()): raise LexError(f'unbalanced brackets in the token stream: {bal}')
    return lx, toks

def excerpt(lx, toks, k):
    if k >= len(toks): return '<EOF>'
    ln = lx.line(toks[k][1])
    return f'line {ln}: ' + ' '.join(t for t, _ in toks[max(0, k - 4):k + 5])[:200]

def check_cs(a, b):
    try:
        la, ta = tokens(a)
    except LexError as e:
        return f'LEX committed: {e}'
    try:
        lb, tb = tokens(b)
    except LexError as e:
        return f'LEX emitted: {e}'
    if not ta and a.strip(): return 'no tokens read from a non-empty committed file'
    sa = [t for t, _ in ta]; sb = [t for t, _ in tb]
    if sa == sb: return None
    k = 0
    while k < min(len(sa), len(sb)) and sa[k] == sb[k]: k += 1
    return f'TOKENS DIFFER at token {k} (committed {len(sa)} / emitted {len(sb)}) :: committed {excerpt(la, ta, k)} || emitted {excerpt(lb, tb, k)}'

MAPLINE = re.compile(r'^\s*\[assembly:\s*(?:global::)?go\.GoPositionMap\("([^"]*)",\s*"([^"]*)"')

def check_pkginfo(a, b):
    al = a.split('\n'); bl = b.split('\n')
    bad = []
    for tag, i1, i2, j1, j2 in difflib.SequenceMatcher(None, al, bl, autojunk=False).get_opcodes():
        if tag == 'equal': continue
        for d, x in enumerate(al[i1:i2]):
            if not MAPLINE.match(x): bad.append(f'committed line {i1 + 1 + d}: {x[:160]}')
        for d, x in enumerate(bl[j1:j2]):
            if not MAPLINE.match(x): bad.append(f'emitted line {j1 + 1 + d}: {x[:160]}')
    if bad: return 'NON-MAP LINE DIFFERS: ' + ' | '.join(bad[:4]) + (f' (+{len(bad) - 4} more)' if len(bad) > 4 else '')
    ka = [m.groups() for m in map(MAPLINE.match, al) if m]; kb = [m.groups() for m in map(MAPLINE.match, bl) if m]
    if ka != kb: return f'MAP KEYS DIFFER: committed {len(ka)} map lines, emitted {len(kb)}; first difference {next(((x, y) for x, y in zip(ka, kb) if x != y), "length")}'
    if not ka and a != b: return 'differs with no map line'
    return None

def kind_of(p):
    base = p.replace('\\', '/').rsplit('/', 1)[-1]
    if base == 'package_info.cs': return 'pkginfo'
    if base.endswith('.cs') or base.endswith('.cs.auto'): return 'cs'
    return 'other'

def check(kind, a, b):
    if kind == 'pkginfo': return check_pkginfo(a, b)
    if kind == 'cs': return check_cs(a, b)
    return None if ' '.join(a.split()) == ' '.join(b.split()) else 'OTHER FILE KIND, not whitespace-only'

def rd(p):
    with open(p, 'rb') as f: return f.read().replace(b'\r', b'').decode('utf-8', errors='surrogateescape')

def main(argv):
    if argv and argv[0] == 'pair' and len(argv) in (3, 4):
        k = argv[3] if len(argv) == 4 else kind_of(argv[2])
        r = check(k, rd(argv[1]), rd(argv[2]))
        print(f'PAIR {"PASS" if r is None else "FAIL"} kind={k} {argv[2]}' + ('' if r is None else f' :: {r}'))
        return 0 if r is None else 1
    if not argv or argv[0] != 'all' or len(argv) != 4:
        print(__doc__ or 'usage: all <worktree> <scratch> <report> | pair <a> <b> [kind]'); return 2
    wt, scratch, rep = argv[1:]
    head = subprocess.run(['git', '-C', wt, 'rev-parse', 'HEAD'], capture_output=True, text=True).stdout.strip()
    pairs = []
    for o in ('windows', 'linux', 'darwin'):
        with open(os.path.join(scratch, f'attr-{o}.txt'), encoding='utf-8') as f:
            for l in f:
                p = l.split(' ', 1)[0].strip()
                if p: pairs.append((o, p))
    paths = sorted(set(p for _, p in pairs))
    # the committed blobs, one git cat-file --batch for every distinct path
    proc = subprocess.Popen(['git', '-C', wt, 'cat-file', '--batch'], stdin=subprocess.PIPE, stdout=subprocess.PIPE)
    blob = {}
    for p in paths:
        proc.stdin.write(f'{head}:{p}\n'.encode('utf-8')); proc.stdin.flush()
        hdr = proc.stdout.readline().decode()
        if hdr.endswith('missing\n'): blob[p] = None; continue
        size = int(hdr.split()[2]); data = proc.stdout.read(size); proc.stdout.read(1)
        blob[p] = data.replace(b'\r', b'').decode('utf-8', errors='surrogateescape')
    proc.stdin.close(); proc.wait()
    res = {o: [0, 0] for o in ('windows', 'linux', 'darwin')}
    kinds = {}
    fails = []
    with open(rep, 'w', encoding='utf-8', newline='\n') as out:
        out.write(f'# tT4-drift-tokens.py all at HEAD {head}, scratch {scratch}: {len(pairs)} (target, path) pairs, {len(paths)} distinct paths\n')
        for o, p in pairs:
            k = kind_of(p); kinds[k] = kinds.get(k, 0) + 1
            ep = os.path.join(scratch, f'U-{o}', *p.split('/'))
            if blob[p] is None: r = 'absent at HEAD'
            elif not os.path.isfile(ep): r = 'absent from the U root'
            else:
                e = rd(ep)
                r = 'IDENTICAL after CR strip (not drift)' if e == blob[p] else check(k, blob[p], e)
            if r is None: res[o][0] += 1; out.write(f'PASS {o} {k} {p}\n')
            else: res[o][1] += 1; fails.append((o, p, r)); out.write(f'FAIL {o} {k} {p} :: {r}\n')
        line = 'DRIFT-TOKENS ' + ' '.join(f'{o}:pass={v[0]}/fail={v[1]}' for o, v in res.items()) + f' total-pairs={len(pairs)} distinct-paths={len(paths)} kinds={kinds} failures={len(fails)}'
        out.write(line + '\n')
    for o, p, r in fails[:200]: print(f'FAIL {o} {p} :: {r}')
    print(line)
    return 0 if not fails else 1

if __name__ == '__main__':
    try: sys.stdout.reconfigure(encoding='utf-8', errors='replace')
    except Exception: pass
    sys.exit(main(sys.argv[1:]))
