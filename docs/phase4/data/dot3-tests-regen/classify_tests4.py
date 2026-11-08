"""WHOLE-FILE attribution for the -tests regen. Per file, HEAD vs working tree (CR-stripped). Each named mechanism has
(a) an instance COUNTER read on both sides (the delta is the mechanism's footprint in that file) and (b) a canonical
rewrite. A file is EXPLAINED when, after every rewrite, the two sides' token streams are equal; the comparison drops
whitespace, braces and parentheses (the K, 204 and B3 shapes move only those) -- a stated loss. Unexplained files get a
residual diff of their canonical streams split on ';' for reading. Whole-file, so no difflib hunk-cut artifacts."""
import collections, difflib, re, subprocess

repo = r'C:/go2cs-tmp/r-testsregen'
files = [l.rstrip('\n').split('\t')[2] for l in open(r'C:/go2cs-tmp/r-kit/tests-numstat.txt', encoding='utf-8') if l.strip()]
files = [f for f in files if not f.endswith('.cs.auto')]

def head(path):
    r = subprocess.run(['git', '-C', repo, 'show', 'HEAD:' + path], capture_output=True)
    return r.stdout.decode('utf-8').replace('\r', '')

def work(path):
    return open(repo + '/' + path, encoding='utf-8').read().replace('\r', '')

SLICE_LIT = re.compile(r'slice<(?:[^<>()\[\]]|<(?:[^<>]|<[^<>]*>)*>)+>\(\[')
USING_MI = re.compile(r'^[ \t]*using MethodImpl(?:Attribute|Options) = global::System\.Runtime\.CompilerServices\.MethodImpl(?:Attribute|Options);[ \t]*\n', re.M)
DEREF = re.compile(r'\(~([^()\s]+(?:\([^()]*\))?)\)\.')
INIT_SCOPE = re.compile(r'^[ \t]*\{[ \t]*\n[ \t]*[^\n]*; (?:if|switch) \(', re.M)
ONE_LINE_FN = re.compile(r'^[^\n]*\) \{ [^\n]*; \}[ \t]*$', re.M)
BLANK = re.compile(r'^[ \t]*$', re.M)
RANGE = re.compile(r'\bin range(?:<(?:[^<>]|<(?:[^<>]|<[^<>]*>)*>)*>)?\(')
RANGE_LEN = re.compile(r'\bin range\((?:builtin\.)?len\(')
FOLD = re.compile(r'\(out (?:var [^)]+|_)\)|is (?:not )?\(var |is var \(')
GUSING = re.compile(r'^global using [^\n]*\n', re.M)
QSTR = re.compile(r'"((?:[^"\\]|\\.)*)"')
ASM = re.compile(r'^\[assembly: (Go[A-Za-z]+)\((.*)$', re.M)

MECH = [  # (label, counter on a side, rewrite of a side)
    ('166: unused CompilerServices usings dropped', lambda s: len(USING_MI.findall(s)), lambda s: USING_MI.sub('', s)),
    ('189: value method through a pointer reads p.M()', lambda s: len(DEREF.findall(s)), lambda s: DEREF.sub(r'\1.', s)),
    ('B3: range(len(x)) form', lambda s: len(RANGE_LEN.findall(s)), lambda s: RANGE_LEN.sub('in ((', s)),
    ('B3: range(...) over a collection', lambda s: len(RANGE.findall(s)), lambda s: RANGE.sub('in (', s)),
    ('B3: bare slice literal [..]', lambda s: len(SLICE_LIT.findall(s)), lambda s: SLICE_LIT.sub('([', s)),
    ('B3: global using for a range<T> element alias', lambda s: len(GUSING.findall(s)), lambda s: GUSING.sub('', s)),
    ('204: init-scope block dropped', lambda s: len(INIT_SCOPE.findall(s)), lambda s: s),
    ('204: init folded into the condition (out / is var)', lambda s: len(FOLD.findall(s)), lambda s: s),
    ('217: deferred-closure field in the position map', lambda s: sum(1 for m in ASM.finditer(s) if m.group(1) == 'GoPositionMap' and len(QSTR.findall(m.group(2))) >= 8), lambda s: s),
    ('K: one-line function bodies', lambda s: len(ONE_LINE_FN.findall(s)), lambda s: s),
    ('K: blank lines', lambda s: len(BLANK.findall(s)), lambda s: s),
]

def trim(l):
    while l and l[-1] == '':
        l = l[:-1]
    return l

def asmnorm(s):
    def f(m):
        if m.group(1) == 'GoPositionMap':
            q = QSTR.findall(m.group(2))
            return '[assembly: GoPositionMap(' + ','.join([q[0], q[1]] + trim([re.sub(r'\d+', '#', x) for x in q[3:7]])) + ')]'
        return '[assembly: ' + m.group(1) + '(' + re.sub(r'\b\d+\b', '#', m.group(2))
    return ASM.sub(f, s)

def toks(s):
    return re.sub(r'[\s(){}]+', '', s)

ID = r'[\w\u0080-ᐦᐨ-￿]+'   # identifier chars, excluding the ᐧ (U+1427) comma-ok marker
ELSE_B3 = (re.compile(r'elsevar([^=;]+)=([^;]+?);if'), r'elseif\2isvar\1&&')
FOLDS = [  # 204 (b)/(c): the UNFOLDED old shape on the token stream, rewritten to the folded shape
    (re.compile(r'var_,(' + ID + r')=([^;]+?,ꟷ\]);if!\1'), r'if\2isnot_,true'),
    (re.compile(r'var_,(' + ID + r')=([^;]+?,ꟷ\]);if\1'), r'if\2is_,true'),
    (re.compile(r'var(' + ID + r'),(' + ID + r')=([^;]+?,ꟷ\]);if!\2'), r'if\3isnotvar\1,true'),
    (re.compile(r'var(' + ID + r'),(' + ID + r')=([^;]+?,ꟷ\]);if\2'), r'if\3isvar\1,true'),
    (re.compile(r'var_,(' + ID + r')=([^;]+?)ᐧ;if(!?)\1'), r'if\3\2out_'),
    (re.compile(r'var(' + ID + r'),(' + ID + r')=([^;]+?)ᐧ;if(!?)\2'), r'if\4\3outvar\1'),
]

def fold_variants(t):
    """The two rule orders: (b)/(c) before (b3), and (b3) first. A chunk is explained if EITHER yields the new side."""
    a = t
    for rx, rep in FOLDS:
        a = rx.sub(rep, a)
    a = ELSE_B3[0].sub(ELSE_B3[1], a)
    b = ELSE_B3[0].sub(ELSE_B3[1], t)
    for rx, rep in FOLDS:
        b = rx.sub(rep, b)
    return {a, b}

def chunks_explained(ta, tb):
    """Token streams split on ';'; every differing chunk must be reproduced by a fold variant of its old side."""
    sa, sb = ta.split(';'), tb.split(';')
    sm = difflib.SequenceMatcher(None, sa, sb, autojunk=False)
    bad = []
    for tag, i1, i2, j1, j2 in sm.get_opcodes():
        if tag == 'equal':
            continue
        o, n = ';'.join(sa[i1:i2]), ';'.join(sb[j1:j2])
        if n not in fold_variants(o):
            bad.append((sa[i1:i2], sb[j1:j2]))
    return bad

per_mech_files = collections.Counter(); per_mech_delta = collections.Counter(); combos = collections.Counter()
explained = 0; residual = []
meta_files = 0
for f in files:
    a, b = head(f), work(f)
    labs = set()
    for lab, cnt, rw in MECH:
        ca, cb = cnt(a), cnt(b)
        if ca != cb:
            labs.add(lab); per_mech_files[lab] += 1; per_mech_delta[lab] += cb - ca
        a, b = rw(a), rw(b)
    a2, b2 = asmnorm(a), asmnorm(b)
    if (a2 != a or b2 != b) and (a != b) and toks(a2) == toks(b2) and toks(a) != toks(b):
        labs.add('metadata records re-encoded (positions / line numbers / maps)'); meta_files += 1
    a, b = a2, b2
    bad = [] if toks(a) == toks(b) else chunks_explained(toks(a), toks(b))
    if not bad:
        explained += 1
        if not labs:
            labs.add('K: layout only (rejoin / indentation)')
        combos[frozenset(labs)] += 1
    else:
        combos[frozenset(labs | {'UNEXPLAINED'})] += 1
        d = []
        for o, n in bad:
            d += ['-' + x for x in o] + ['+' + x for x in n]
        residual.append((f, d))

out = [f'files {len(files)}  explained {explained}  unexplained {len(residual)}  (metadata re-encode files {meta_files})', '',
       'mechanism  files  net-instance-delta(new-old)']
for lab, _, _ in MECH:
    out.append(f'  {per_mech_files[lab]:5d}  {per_mech_delta[lab]:+7d}  {lab}')
out += ['', 'label combinations (files):']
for lab, n in combos.most_common():
    out.append(f'  {n:5d}  {" + ".join(sorted(lab))}')
out.append('')
for f, d in residual[:60]:
    out.append(f'-- {f}  ({len(d)} residual lines)')
    for x in d[:12]:
        out.append('   ' + x[:200])
open(r'C:/go2cs-tmp/r-kit/tests-classified4.txt', 'w', encoding='utf-8').write('\n'.join(out))
