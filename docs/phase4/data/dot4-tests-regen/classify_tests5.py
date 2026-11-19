"""WHOLE-FILE attribution for the -tests regen. Per file, HEAD vs working tree (CR-stripped). Each named mechanism has
(a) an instance COUNTER read on both sides (the delta is the mechanism's footprint in that file) and (b) a canonical
rewrite. A file is EXPLAINED when, after every rewrite, the two sides' token streams are equal; the comparison drops
whitespace, braces and parentheses (the K, 204 and B3 shapes move only those) -- a stated loss. Unexplained files get a
residual diff of their canonical streams split on ';' for reading. Whole-file, so no difflib hunk-cut artifacts.

v5 (i9, the .4 regen at b63418fafc): classify_tests4.py with the paths as arguments and this week's mechanisms added
by name (COORD 13:57Z): Tour 3's switch/select temps, OQ2's method holders, R's array reassign (Assign/AssignFrom),
the unsafe.Pointer box and interior-pointer spellings, P2's shapes. The .3 mechanisms stay; on a tree that already
carries them their counters read 0.
Usage: python -I classify_tests5.py <repo> <numstat-file> <out-file> [<base-ref>]
The old side is <base-ref> (default HEAD; after the regen's commit, pass the master it was cut from), the new side the
working tree."""
import collections, difflib, re, subprocess, sys

repo, numstat, outpath = sys.argv[1], sys.argv[2], sys.argv[3]
base = sys.argv[4] if len(sys.argv) > 4 else 'HEAD'
files = [l.rstrip('\n').split('\t')[2] for l in open(numstat, encoding='utf-8') if l.strip()]
files = [f for f in files if not f.endswith('.cs.auto')]

def head(path):
    r = subprocess.run(['git', '-C', repo, 'show', base + ':' + path], capture_output=True)
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

# --- v5: this week's mechanisms -------------------------------------------------------------------------------------
NAME = r'[^\W\d]\w*'
EXPR_DECL = re.compile(r'^[ \t]*var ((' + NAME + r'?)ᴛ\d+) = (' + NAME + r');[ \t]*\n', re.M)
TEMP = re.compile(r'\b(?:expr|sel)ᴛ\d+\b')
TEMP_NAME = re.compile(r'\b' + r'[^\W\d][\w]*?ᴛ\d+\b')
HOLDER = re.compile(r'^\} partial class ' + NAME + r' \{[ \t]*\n', re.M)
ASSIGN = re.compile(r'\.(?:AssignFrom|Assign)\(')
VALUE_SET = re.compile(r'\.Value = ')
SELF_ASSIGN = re.compile(r'^[ \t]*(' + NAME + r') = \1;[ \t]*\n', re.M)
VALUE_CLONE_ATTR = re.compile(r'\[GoValueClone\([^\]\n]*\)\][ \t]*')
BARE_PARTIAL = re.compile(r'^[ \t]*partial struct [\w#]+ \{\}[ \t]*\n', re.M)
TYPE_SUFFIX = re.compile(r'(?<=\w)ᴛ(?![\w#])')
FUNC_VAR = re.compile(r'(?:Func|Action)<(?:[^<>]|<(?:[^<>]|<(?:[^<>]|<[^<>]*>)*>)*>)*> (' + NAME + r') = (?=[A-Za-z_@])')
SORT_IFACE = re.compile(r'\((?:sort\.)?Interface\)\((\([\w.]+\)\w+)\)')
SORT_IFACE2 = re.compile(r'\((?:sort\.)?Interface\)\(?(\w+)')
CAST_WRAP = re.compile(r'\(\((?:Func|Action)<(?:[^<>]|<(?:[^<>]|<(?:[^<>]|<[^<>]*>)*>)*>)*>\)\(([\w.]+)\)\)')
STAMP_BLOCK = re.compile(r'^// This package\'s string literals are hoisted.*?^\[assembly: GoHoistedLiterals\][ \t]*\n', re.M | re.S)
DECL_RUN = re.compile(r'(?:^[ \t]*(?:\[[^\n]*?\][ \t]*)*(?:public|internal) partial (?:struct|interface|class) [^\n]*\n)+', re.M)
VARIADIC_ALIAS = re.compile(r'^using (ꓸꓸꓸ\S+) = (Span<[^;\n]+>);[ \t]*\n', re.M)
AT_FILE = re.compile(r'(?<![\w@])@(?=file\b)')
LOCAL_ALIAS = re.compile(r'\b(?:fdType|syscallDescriptor)\b')
FIELD_ACCESS = re.compile(r'^\[assembly: GoFieldAccess\([^\n]*\)\][ \t]*\n', re.M)
W9B_SIMPLE = re.compile(r'(?<![\w.])Ꮡ(\w+)\.Ꮡ(\w+)')
W9B_REINTERP = re.compile(r'(Ꮡ\w+\.Reinterpret<[^;\n]*?>\(\))\.Ꮡ(\w+)')
DOOR_LOADARRAY = re.compile(r'\((' + NAME + r')\)@unsafe\.Pointer\.LoadArray<[^>\n]+>\(([^,()\n]+), \d+\)')
DOOR_FROMREF = re.compile(r'@unsafe\.Pointer\.FromRef\(ref (\w+)\)')
TUPLE_TEMP = re.compile(r'var (' + NAME + r')ᴛ(\d+)\)([^\n]*)\n[ \t]*\1\.(?:AssignFrom|Assign)\(\1ᴛ\2\);[ \t]*\n')


def sort_decl_runs(s):
    return DECL_RUN.sub(lambda m: ''.join(sorted(m.group(0).splitlines(True))), s)


def inline_variadic_alias(s):
    for m in list(VARIADIC_ALIAS.finditer(s)):
        s = s.replace(m.group(0), '', 1)
        s = re.sub(re.escape(m.group(1)) + r'(?![\w.])', m.group(2), s)
    return s


def drop_tested_impl(s):
    if not TESTED_PKG:
        return s
    return re.sub(r'^\[assembly: GoImplement<(?:global::)?(?:go\.)?(?:[\w@]+\.)*?' + re.escape(TESTED_PKG) + r'(?:_package)?\.[^\n]*\n', '', s, flags=re.M)


HOLDER_BLOCK = re.compile(r'^// A Go method is an extension method.*?^// </MethodHolders>[ \t]*\n', re.M | re.S)
END_COMMENT = re.compile(r'// end ' + NAME)
DELTA = re.compile('Δ')
HOIST = re.compile(r'^// Hoisted [^\n]*\n|^(?:internal|private|public) static (?:ref )?readonly [^\n=]*?\w[ˢᶜᶠ]\d*ᵇ? (?:=>|=) [^\n]*\n', re.M)
QDOT = re.compile(r'(?<=\w)ꓸ(?=\w)')
CONST_ALIAS = re.compile(r'^\[assembly: GoTypeAlias\("(' + NAME + r')", "(?:const:)?\1"\)\][ \t]*\n', re.M)
HOLDER_REF = re.compile(r'global::go\.((?:@?\w+\.)*?)(' + NAME + r')ᴛ' + NAME + r'\.(?=' + NAME + r')')
HOLDER_REF_SHORT = re.compile(r'(?<![\w.])' + NAME + r'ᴛ[^\W\d]\w*\.(?=' + NAME + r')')
PKG_QUAL = re.compile(r'(?<![\w.])(?:global::)?(?:@?\w+\.)*?(' + NAME + r')_package\.(?=' + NAME + r')')
GEN = r'(?:[^<>()]|<(?:[^<>]|<(?:[^<>]|<[^<>]*>)*>)*>)+'
INTERIOR = re.compile(r'\(ж<(' + GEN + r')>\)\(uintptr\)')
UINTPTR = re.compile(r'\(uintptr\)')
HOIST_NUM = re.compile(r'(?<=\w[ˢᶜᶠ])\d+\b')
ADAPTER = re.compile(r'(?<![\w.])\w+?_(?:internal_)?test\.(' + NAME + r'?)_(\w+ж\w+)')
USING_STATIC_PKG = re.compile(r'^using static (?:global::)?(?:@?\w+\.)*?(' + NAME + r')_package;', re.M)
USING_STATIC_LINE = re.compile(r'^using static [^\n;]*;[ \t]*\n', re.M)
UNSAFE_ADD = re.compile(r'@unsafe\.Add\(([^,()]+), ')
NILSAFE = re.compile(r'NilSafeDelegateConversion<')


def static_pkg_short(s):
    """OQ2: a type of a package the file imports with `using static` reads bare once no method in that package class
    collides with its name. Both sides drop the qualifier of every such package before an upper-case name."""
    for p in STATIC_BOTH:
        s = re.sub(r'(?<![\w.])' + re.escape(p) + r'\.(?=[A-Z])', '', s)
    return USING_STATIC_LINE.sub('', s)   # the using-static set itself (OQ2 adds the holder's) is dropped: a stated loss


def nilsafe_to_new(s):
    """C47: NilSafeDelegateConversion<T, U>(f) compares as new T(f): split the two type arguments at the top-level comma."""
    out, pos = [], 0
    while True:
        m = NILSAFE.search(s, pos)
        if not m:
            return ''.join(out) + s[pos:]
        i, depth, comma = m.end(), 1, None
        while i < len(s) and depth:
            c = s[i]
            if c in '<(':
                depth += 1
            elif c in '>)':
                depth -= 1
            elif c == ',' and depth == 1 and comma is None:
                comma = i
            i += 1
        if comma is None:
            out.append(s[pos:m.end()]); pos = m.end(); continue
        out.append(s[pos:m.start()] + 'new ' + s[m.end():comma].strip())
        pos = i   # just past the closing '>'
VALUE_CLONE = re.compile(r'^[ \t]*\[GoValueClone\([^\n]*\)\] partial struct ' + NAME + r' \{\}[ \t]*\n', re.M)
CLONE = re.compile(r'\.Clone\(\)')
BUILTIN = re.compile(r'(?<![\w.])builtin\.(?=' + NAME + r')')
USING_ALIAS = re.compile(r'^using ' + NAME + r' = (?:global::)?[\w.@]*_package;[ \t]*\n', re.M)
ASM_RUN = re.compile(r'(?:^\[assembly: [^\n]*\n)+', re.M)


def sort_asm_runs(s):
    return ASM_RUN.sub(lambda m: ''.join(sorted(m.group(0).splitlines(True))), s)
FUNC_WRAP = re.compile(r'new (?:Func|Action)<(?:[^<>]|<(?:[^<>]|<(?:[^<>]|<[^<>]*>)*>)*>)*>\((' + NAME + r'(?:\.' + NAME + r')*)\)')
PROJREF = re.compile(r'^[ \t]*<ProjectReference Include="[^"\n]*" />[ \t]*\n', re.M)


def inline_expr_temps(s):
    """Tour 3: a switch or select copies a bare identifier into a temp only where it must, and names the temp after the
    identifier. On BOTH sides every temp copy of a bare identifier (exprᴛN, selᴛN or <ident>ᴛN = <ident>) is inlined,
    its uses up to the next declaration of the same temp name replaced by the identifier; a temp of any other
    expression stays (and is then renamed to ᴛ# by TEMP_NAME)."""
    pos = 0
    while True:
        m = EXPR_DECL.search(s, pos)
        if not m:
            return s
        name, prefix, ident = m.group(1), m.group(2), m.group(3)
        bare = re.sub(r'ʗ\d+$', '', ident)
        if prefix not in ('expr', 'sel', ident, bare, re.sub(r'\d+$', '', bare), re.sub(r'(?:[Δʗ]\d+)+$', '', ident)):
            pos = m.end()
            continue
        nxt = re.compile(r'\bvar ' + re.escape(name) + r' =').search(s, m.end())
        end = nxt.start() if nxt else len(s)
        s = s[:m.start()] + re.sub(r'\b' + re.escape(name) + r'\b', ident, s[m.end():end]) + s[end:]
        pos = m.start()


MECH = [  # (label, counter on a side, rewrite of a side)
    ('R: an array destination of a tuple assignment takes a temp and AssignFrom (compared as the direct destination)',
     lambda s: len(TUPLE_TEMP.findall(s)), lambda s: TUPLE_TEMP.sub(r'\1)\3\n', s)),
    ('OQ2: a variadic alias over pkgꓸT written inline as Span<T> (alias inlined on both sides)',
     lambda s: len(VARIADIC_ALIAS.findall(s)), inline_variadic_alias),
    ('Tour 3: switch/select temps named after the identifier (exprᴛ inlined, selᴛ renamed)',
     lambda s: len(TEMP.findall(s)), lambda s: TEMP_NAME.sub('ᴛ#', inline_expr_temps(s))),
    ('OQ2: methods in holder partial classes (holder split lines)',
     lambda s: len(HOLDER.findall(s)), lambda s: HOLDER.sub('', s)),
    ('OQ2: the package_info MethodHolders block', lambda s: len(HOLDER_BLOCK.findall(s)), lambda s: HOLDER_BLOCK.sub('', s)),
    ('OQ2: a class-closing "// end" comment names the holder', lambda s: 0, lambda s: END_COMMENT.sub('// end', s)),
    ('OQ2: a Δ collision prefix dropped (a method no longer shares the package class)', lambda s: len(DELTA.findall(s)), lambda s: DELTA.sub('', s)),
    ('OQ2: hoisted literals re-homed per holder class (declaration lines dropped on both sides: a stated loss)',
     lambda s: len(HOIST.findall(s)), lambda s: HOIST.sub('', s)),
    ('OQ2: a qualified type alias (pkgꓸType) reads pkg.Type', lambda s: len(QDOT.findall(s)), lambda s: QDOT.sub('.', s)),
    ('OQ2: an identity or const: GoTypeAlias record dropped', lambda s: len(CONST_ALIAS.findall(s)), lambda s: CONST_ALIAS.sub('', s)),
    ('OQ2: a method value names its holder (global::go.pkgᴛT.M)', lambda s: len(HOLDER_REF.findall(s)), lambda s: HOLDER_REF.sub(r'global::go.\1\2_package.', s)),
    ('OQ2: a method expression names its holder (pkgᴛT.M)', lambda s: len(HOLDER_REF_SHORT.findall(s)), lambda s: HOLDER_REF_SHORT.sub('', s)),
    ('C47 (286): NilSafeDelegateConversion<T, U>(f) (compared as new T(f))', lambda s: len(NILSAFE.findall(s)), nilsafe_to_new),
    ('interior-pointer cut (837a413fdc): pointer arithmetic reads @unsafe.Add(p, n) (compared as (@unsafe.Pointer)(p + n))',
     lambda s: len(UNSAFE_ADD.findall(s)), lambda s: UNSAFE_ADD.sub(r'(@unsafe.Pointer)(\1 + ', s)),
    ('OQ2: hoisted literal numbering restarts per holder class (ˢN compared as ˢ: a stated loss)', lambda s: len(HOIST_NUM.findall(s)), lambda s: HOIST_NUM.sub('', s)),
    ('interior-pointer cut (837a413fdc): (ж<T>)(uintptr)(p) reads @unsafe.Pointer.To<T>(p)', lambda s: len(INTERIOR.findall(s)), lambda s: INTERIOR.sub(r'@unsafe.Pointer.To<\1>', s)),
    ('unsafe.Pointer box (41067a0fb7): a (uintptr) cast dropped (every (uintptr) removed on both sides: a stated loss)',
     lambda s: len(UINTPTR.findall(s)), lambda s: UINTPTR.sub('', s)),
    ('P2 W-zero-len / W-embed-array (312): GoValueClone records (attribute removed wherever it stands)',
     lambda s: len(VALUE_CLONE_ATTR.findall(s)), lambda s: BARE_PARTIAL.sub('', VALUE_CLONE_ATTR.sub('', s))),
    ('OQ2: a type no longer marked against a method name (Δsliceᴛ reads slice)', lambda s: len(TYPE_SUFFIX.findall(s)), lambda s: TYPE_SUFFIX.sub('', s)),
    ('OQ2: a function value is declared var once no holder-moved method shares its name', lambda s: len(FUNC_VAR.findall(s)), lambda s: FUNC_VAR.sub(r'var \1 = ', s)),
    ('OQ2: an explicit sort.Interface cast dropped once the call is unambiguous', lambda s: len(SORT_IFACE.findall(s)), lambda s: SORT_IFACE.sub(r'\1', s)),
    ('OQ2: a package-class qualifier (pkg_package.X, go.a.pkg_package.X) reads pkg.X', lambda s: len(PKG_QUAL.findall(s)), lambda s: PKG_QUAL.sub(r'\1.', s)),
    ('W18 ruling A (311): a foreign adapter lift is reused from its import (pkg_test.pkg_TжI reads pkg.TжI)',
     lambda s: len(ADAPTER.findall(s)), lambda s: ADAPTER.sub(r'\1.\2', s)),
    ('W18 ruling A (311): the reused adapter GoImplement record leaves the test info (records of the tested package dropped on both sides)',
     lambda s: 0, drop_tested_impl),
    ('#51 stamp rule: the [GoHoistedLiterals] stamp leaves an info file once its package boxes a bare literal (which literal: not traced)',
     lambda s: len(STAMP_BLOCK.findall(s)), lambda s: STAMP_BLOCK.sub('', s)),
    ('OQ2: type-declaration records re-sorted once a Δ name drops (runs compared sorted)', lambda s: 0, sort_decl_runs),
    ('OQ2: a local named file is escaped @file (production carries it since 60d0f629ce)', lambda s: len(AT_FILE.findall(s)), lambda s: AT_FILE.sub('', s)),
    ('OQ2: a conversion to a package-local alias is spelled by the alias once its target loses Δ (fdType, syscallDescriptor = syscall.Handle)',
     lambda s: len(LOCAL_ALIAS.findall(s)), lambda s: LOCAL_ALIAS.sub('syscall.Handle', s)),
    ('C47 (286): a func-value cast ((Func<..>)(f)) dropped at an any slot (compared as f)', lambda s: len(CAST_WRAP.findall(s)), lambda s: CAST_WRAP.sub(r'\1', s)),
    ('OQ2: an explicit (Interface) cast dropped once the call is unambiguous', lambda s: len(SORT_IFACE2.findall(s)), lambda s: SORT_IFACE2.sub(r'\1', s)),
    ('W9b (446aa16fcd): a field address through a pointer reads p.Ꮡf (GoFieldAccess records dropped on both sides: a stated loss)',
     lambda s: len(W9B_SIMPLE.findall(s)) + len(W9B_REINTERP.findall(s)),
     lambda s: FIELD_ACCESS.sub('', W9B_SIMPLE.sub(r'Ꮡ(\1.\2)', W9B_REINTERP.sub(r'Ꮡ((\1).Value.\2)', s)))),
    ('door B (5244cfbe2b): array-pointer element doors (LoadArray<byte>(p, n) and FromRef(ref x) -> Ꮡx)',
     lambda s: len(DOOR_LOADARRAY.findall(s)) + len(DOOR_FROMREF.findall(s)),
     lambda s: DOOR_FROMREF.sub(r'Ꮡ\1', DOOR_LOADARRAY.sub(r'~@unsafe.Pointer.To<\1>(\2)', s))),
    ('OQ2: a type of a using-static package reads bare once no holder-moved method collides (qualifier dropped on both sides)',
     lambda s: 0, static_pkg_short),
    ('OQ2: builtin.X no longer needed (a method no longer shadows the builtin)', lambda s: len(BUILTIN.findall(s)), lambda s: BUILTIN.sub('', s)),
    ('OQ2: package using-alias set (using pkg = go.pkg_package; lines dropped on both sides: a stated loss)',
     lambda s: len(USING_ALIAS.findall(s)), lambda s: USING_ALIAS.sub('', s)),
    ('OQ2: assembly records re-sorted once a Δ name drops (each run of records compared sorted)', lambda s: 0, sort_asm_runs),
    ('P2 (312): clone family (.Clone() compared absent; a parameter cloned on entry, a = a.Clone();, dropped)',
     lambda s: len(CLONE.findall(s)), lambda s: SELF_ASSIGN.sub('', CLONE.sub('', s))),
    ('C47 (286): a func-value argument is passed bare, not new Func<..>(f)', lambda s: len(FUNC_WRAP.findall(s)), lambda s: FUNC_WRAP.sub(r'\1', s)),
    ('6a184f1982 (11-11, -tests reference scan): a leaf-collision ProjectReference leaves the .tests.csproj (COORD 20261113T024608Z named seven)',
     lambda s: len(PROJREF.findall(s)), lambda s: PROJREF.sub('', s)),
    ('R: array/struct reassign reads Assign/AssignFrom (old p = x and p.Value = x both compared as p = x)',
     lambda s: len(ASSIGN.findall(s)), lambda s: VALUE_SET.sub(' = ', ASSIGN.sub(' = (', s))),
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
    STATIC_BOTH = set(USING_STATIC_PKG.findall(a)) & set(USING_STATIC_PKG.findall(b))   # symmetric by construction
    TESTED_PKG = f.split('/')[-2]   # the package under test: its directory's last segment
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
            so, sn = ';'.join(o), ';'.join(n)
            cm = difflib.SequenceMatcher(None, so, sn, autojunk=False)
            for tag, i1, i2, j1, j2 in cm.get_opcodes():   # the differing REGIONS, with 50 chars of context
                if tag != 'equal':
                    d.append('-' + so[max(0, i1 - 50):i2 + 20] + '   =>   +' + sn[max(0, j1 - 50):j2 + 20])
        residual.append((f, d))

out = [f'files {len(files)}  explained {explained}  unexplained {len(residual)}  (metadata re-encode files {meta_files})', '',
       'mechanism  files  net-instance-delta(new-old)']
for lab, _, _ in MECH:
    out.append(f'  {per_mech_files[lab]:5d}  {per_mech_delta[lab]:+7d}  {lab}')
out += ['', 'label combinations (files):']
for lab, n in combos.most_common():
    out.append(f'  {n:5d}  {" + ".join(sorted(lab))}')
out.append('')
for f, d in residual:
    out.append(f'-- {f}  ({len(d)} residual lines)')
    for x in d[:12]:
        out.append("   " + x[:400])
open(outpath, 'w', encoding='utf-8').write('\n'.join(out))
