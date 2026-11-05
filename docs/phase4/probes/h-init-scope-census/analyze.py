# Init-statement scope census: tables and candidate-shape populations over census.py's joined.jsonl.
#   py -I -X utf8 analyze.py <out dir>
import json, sys, collections, re

OUT = sys.argv[1].rstrip('/\\') + '/'
rows = [json.loads(l) for l in open(OUT + 'joined.jsonl', encoding='utf-8')]
INIT = ('if', 'elseif', 'switch', 'typeswitch')
blocks = [r for r in rows if r['go']['item'] in INIT]
gobare = [r for r in rows if r['go']['item'] == 'block']
core = [r for r in blocks if r['targets'] != ['golden']]
gold = [r for r in blocks if r['targets'] == ['golden']]


def stmt_kind(g):
    if g['item'] == 'if':
        return {'none': 'if', 'else': 'if/else', 'elseif': 'if, heading an else-if chain'}[g['chain']]
    return {'elseif': 'else-if (its own init)', 'switch': 'switch', 'typeswitch': 'type switch'}[g['item']]


INITK = {'assert': 'comma-ok assertion', 'maplookup': 'comma-ok map lookup', 'chanrecv': 'comma-ok channel receive',
         'chanrecv1': 'channel receive (1 value)', 'multicall': 'call, multiple results', 'define1': 'plain := (1 name)',
         'parallel': 'parallel := / =', 'assign1': 'assignment =', 'incdec': '++/--', 'expr': 'expression (call)',
         'send': 'send', 'other': 'other'}


def table(pop, title):
    print(f'\n### {title}: {len(pop)} blocks')
    sk = collections.Counter(stmt_kind(r['go']) for r in pop)
    ik = collections.Counter(INITK[r['go']['init']] + ('' if r['go']['define'] or r['go']['init'] in ('incdec', 'expr', 'send') else ' (no :=)') for r in pop)
    cross = collections.Counter((stmt_kind(r['go']), INITK[r['go']['init']]) for r in pop)
    print('by statement:', sk.most_common())
    print('by init:', ik.most_common())
    kinds = [k for k, _ in sk.most_common()]
    inits = [k for k, _ in collections.Counter(INITK[r['go']['init']] for r in pop).most_common()]
    print('| init \\ statement | ' + ' | '.join(kinds) + ' | total |')
    for i in inits:
        cells = [cross.get((k, i), 0) for k in kinds]
        print(f'| {i} | ' + ' | '.join(str(c) for c in cells) + f' | {sum(cells)} |')


table(blocks, 'CORPUS (core production, unique files, + goldens)')
table(core, 'core production (unique files)')
table(gold, 'behavioral goldens')
for t in ('windows', 'linux', 'darwin'):
    print(f'target {t}: {sum(1 for r in core if t in r["targets"])} blocks in {len({r["cs"] for r in core if t in r["targets"]})} files')
print(f'Go bare blocks (not init blocks, reported only): {len(gobare)}')

# ---- (a) drop the braces: bare forms only (if / switch / type switch; an else-if is its else's block)
spans = collections.defaultdict(list)          # file -> [(start, end, declared)] of every init block
for r in blocks:
    c = r['cs_item']
    spans[r['cs']].append((c['line'], c['endLine'], set(c['declared'])))


def sibling_only(r):
    # every hit is inside ANOTHER init block that itself declares the hit name
    c = r['cs_item']
    if not c['hitLines']:
        return False
    others = [s for s in spans[r['cs']] if s[0] != c['line']]
    return all(any(s[0] <= hl <= s[1] for s in others) for hl in c['hitLines'])


def reasons_a(r):
    c = r['cs_item']
    out = []
    if c['hits']:
        out.append('collision: sibling init blocks only' if sibling_only(r) else 'collision: other')
    if (c['labelInSpace'] or c['labeledParent']) and c['lambdaInBlock']:
        out.append('label in scope + closure in block')
    return out


bare = [r for r in blocks if r['go']['item'] != 'elseif']
safe_a = [r for r in bare if not reasons_a(r)]
print(f'\n(a) drop braces: {len(safe_a)} of {len(bare)} bare init blocks safe ({100*len(safe_a)/len(bare):.1f}%)')
print('  safe by statement:', collections.Counter(stmt_kind(r['go']) for r in safe_a).most_common())
print('  safe non-declaring inits:', sum(1 for r in safe_a if not r['cs_item']['declared']))
print('  refused, by reason:', collections.Counter(' + '.join(reasons_a(r)) for r in bare if reasons_a(r)).most_common())
hitnames = collections.Counter()
for r in bare:
    for n in r['cs_item']['hits']:
        hitnames[re.sub(r'Δ\d*$', '', n)] += 1
print('  colliding names (Δ-stripped):', hitnames.most_common(12))
print('  label+closure refusals:', sum(1 for r in bare if 'label in scope + closure in block' in reasons_a(r)))

# ---- (b)/(c) condition-borne shapes, for if-kind items
def simple(r):
    c = r['cs_item']
    return len(c['kinds']) == 2 and c['kinds'][1] == 'if' and c['first'].startswith('var ')


def uses(m, n):
    return (m or {}).get(n, 0)


def shape_b(r):
    g = r['go']
    if g['item'] not in ('if', 'elseif') or not g['define'] or not simple(r):
        return None
    names = g['names']
    if g['init'] in ('assert', 'maplookup', 'chanrecv') and len(names) == 2:
        v, ok = names
        okelse = ok != '_' and (uses(g.get('usesBody'), ok) or uses(g.get('usesElse'), ok))
        if g['condIsOk'] and not okelse and (v == '_' or not uses(g.get('usesElse'), v)):
            return 'b1 `is (var v, true)`'
        if g['condNotOk'] and not okelse and (v == '_' or not uses(g.get('usesBody'), v)):
            return 'b2 `is not (var v, true)`'
    return 'b3 `is var (...) && cond`'


def shape_c(r):
    g = r['go']
    if g['item'] not in ('if', 'elseif') or not g['define'] or not simple(r) or g['init'] != 'assert':
        return None
    v, ok = g['names']
    if g['condIsOk'] and ok != '_' and not uses(g.get('usesBody'), ok) and not uses(g.get('usesElse'), ok):
        return 'c `x._<T>(out var v)`'
    return None


ifk = [r for r in blocks if r['go']['item'] in ('if', 'elseif')]
for label, fn in (('(b) pattern in the condition', shape_b), ('(c) golib out-var assertion', shape_c)):
    got = [r for r in ifk if fn(r)]
    print(f'\n{label}: applicable to {len(got)} of {len(ifk)} if/else-if init blocks')
    print('  by form:', collections.Counter(fn(r) for r in got).most_common())
    heads = [r for r in got if r['go']['item'] == 'if']
    eifs = [r for r in got if r['go']['item'] == 'elseif']
    print(f'  head ifs {len(heads)} (of which collision-free {sum(1 for r in heads if not reasons_a(r))}); else-ifs {len(eifs)} (nested scope: no new collision)')
notsimple = [r for r in ifk if r['go']['define'] and not simple(r)]
print('\nif-kind := inits NOT in the simple [var-decl; if] shape:', len(notsimple),
      collections.Counter((r['cs_item']['first'].split(' ')[0], len(r['cs_item']['kinds'])) for r in notsimple).most_common(8))

# ---- the composite recommendation
eif = [r for r in blocks if r['go']['item'] == 'elseif']
gone_a = len(safe_a)
gone_eif = sum(1 for r in eif if shape_b(r) or shape_c(r))
print(f'\nCOMPOSITE: (a) on bare blocks {gone_a} + (b)/(c) on else-ifs {gone_eif} = {gone_a + gone_eif} of {len(blocks)} '
      f'({100*(gone_a + gone_eif)/len(blocks):.1f}%) init blocks go away')
sib = [r for r in bare if reasons_a(r) == ['collision: sibling init blocks only']]
print(f'  + sibling-merge candidates (refused only for sibling init blocks): {len(sib)}')
for t in ('windows', 'linux', 'darwin', 'golden'):
    pop = [r for r in blocks if t in r['targets']]
    g = sum(1 for r in pop if (r['go']['item'] != 'elseif' and not reasons_a(r)) or (r['go']['item'] == 'elseif' and (shape_b(r) or shape_c(r))))
    print(f'  {t}: {g} of {len(pop)} ({100*g/len(pop):.1f}%)')

# ---- examples
def ex(pred, n=1):
    for r in [r for r in blocks if pred(r)][:n]:
        print('   ', r['cs'].split('/src/')[1], r['cs_item']['line'], '|', r['cs_item']['first'][:110], '| if (', r['cs_item']['cond'][:60], ')')


print('\nEXAMPLES')
print(' (a) safe assert:'); ex(lambda r: r['go']['item'] == 'if' and r['go']['init'] == 'assert' and not reasons_a(r), 2)
print(' (a) refused sibling ok:'); ex(lambda r: reasons_a(r) == ['collision: sibling init blocks only'], 2)
print(' (a) refused other:'); ex(lambda r: r['go']['item'] != 'elseif' and reasons_a(r) == ['collision: other'], 3)
print(' (a) refused label+closure:'); ex(lambda r: r['go']['item'] != 'elseif' and 'label in scope + closure in block' in reasons_a(r), 2)
print(' switch safe:'); ex(lambda r: r['go']['item'] == 'switch' and not reasons_a(r), 2)
print(' typeswitch safe:'); ex(lambda r: r['go']['item'] == 'typeswitch' and not reasons_a(r), 2)
print(' else-if b1:'); ex(lambda r: r['go']['item'] == 'elseif' and (shape_b(r) or '').startswith('b1'), 2)
print(' else-if b3:'); ex(lambda r: r['go']['item'] == 'elseif' and (shape_b(r) or '').startswith('b3'), 2)
print(' else-if not simple:'); ex(lambda r: r['go']['item'] == 'elseif' and not shape_b(r), 3)
print(' multicall head safe:'); ex(lambda r: r['go']['item'] == 'if' and r['go']['init'] == 'multicall' and not reasons_a(r), 2)

# ---- strict (a): the predicate the IL proof transforms (no closure in the block at all)
def strict(r):
    c = r['cs_item']
    return r['go']['item'] != 'elseif' and not reasons_a(r) and not c['lambdaInBlock']


st = [r for r in bare if strict(r)]
eif_ok = [r for r in eif if shape_b(r) or shape_c(r)]
print(f'\nSTRICT (a): {len(st)} of {len(bare)} bare blocks ({100*len(st)/len(bare):.1f}%); closure-in-block kept braced: {sum(1 for r in bare if not reasons_a(r) and r["cs_item"]["lambdaInBlock"])}')
print(f'STRICT COMPOSITE: {len(st)} + else-if {len(eif_ok)} = {len(st)+len(eif_ok)} of {len(blocks)} ({100*(len(st)+len(eif_ok))/len(blocks):.1f}%)')
for t in ('windows', 'linux', 'darwin', 'golden'):
    pop = [r for r in blocks if t in r['targets']]
    g = sum(1 for r in pop if strict(r) or (r['go']['item'] == 'elseif' and (shape_b(r) or shape_c(r))))
    files = {r['cs'] for r in pop if strict(r) or (r['go']['item'] == 'elseif' and (shape_b(r) or shape_c(r)))}
    print(f'  {t}: {g} of {len(pop)} ({100*g/len(pop):.1f}%) in {len(files)} files')
print('  strict by statement:', collections.Counter(stmt_kind(r['go']) for r in st).most_common())
print('  strict by init:', collections.Counter(INITK[r['go']['init']] for r in st).most_common())
kept = [r for r in blocks if not (strict(r) or (r['go']['item'] == 'elseif' and (shape_b(r) or shape_c(r))))]
print('  kept braced, by reason:', collections.Counter(
    ('else-if not in the simple shape' if r['go']['item'] == 'elseif' else
     ' + '.join(reasons_a(r)) or 'closure in block') for r in kept).most_common())

# ---- rescue: a refused comma-ok head-if whose collision is ONLY its `ok` name, which (b1)/(b2)/(c) never declare
def rescued(r):
    g, c = r['go'], r['cs_item']
    if g['item'] != 'if' or strict(r) or c['lambdaInBlock'] or not c['hits']:
        return None
    if ((c['labelInSpace'] or c['labeledParent']) and c['lambdaInBlock']):
        return None
    form = shape_c(r) or ((shape_b(r) or '') if (shape_b(r) or '').startswith(('b1', 'b2')) else None)
    if not form:
        return None
    okname = c['declared'] and [d for d in c['declared'] if re.sub(r'Δ\d*$', '', d) == g['names'][1]]
    hit = set(c['hits'])
    return form if okname and hit <= set(okname) else None


rs = [r for r in blocks if rescued(r)]
print(f'\nRESCUE by dropping `ok`: {len(rs)} refused head-if blocks', collections.Counter(rescued(r) for r in rs).most_common())
total = len(st) + len(eif_ok) + len(rs)
print(f'WITH RESCUE: {total} of {len(blocks)} ({100*total/len(blocks):.1f}%)')
for t in ('windows', 'linux', 'darwin', 'golden'):
    pop = [r for r in blocks if t in r['targets']]
    g = sum(1 for r in pop if strict(r) or (r['go']['item'] == 'elseif' and (shape_b(r) or shape_c(r))) or rescued(r))
    print(f'  {t}: {g} of {len(pop)} ({100*g/len(pop):.1f}%)')
left = [r for r in blocks if not (strict(r) or (r['go']['item'] == 'elseif' and (shape_b(r) or shape_c(r))) or rescued(r))]
print('  still braced, by reason:', collections.Counter(
    ('else-if not in the simple shape' if r['go']['item'] == 'elseif' else
     ' + '.join(reasons_a(r)) or 'closure in block') for r in left).most_common())
