# The 1.26.10.2 CLOSING CHECKS (PLAN-release-1-26-10-2.md "The closing checks"), measured from the records AT ONE REF.
# Usage: py -3 -I closing_checks.py <repo> <ref> [--no-api]
#   Every file is read with `git show <ref>:<path>`, never from a working tree, so the reading names its tree.
#   The darwin runs' headSha is read from GitHub's public REST API (unauthenticated, read-only): the darwin TSVs do not
#   record it. --no-api skips that one check and says so.
# It prints one block per check and exits 1 when any check fails. It writes nothing.
import collections, csv, io, json, re, subprocess, sys, urllib.request

repo, ref = sys.argv[1], sys.argv[2]
use_api = '--no-api' not in sys.argv[3:]
TIP = 'd9dd8f54994297f34be694b7a4594fc6ea826024'
EV = 'docs/phase4/rebank-1.26.10.2'
OSES = ('windows', 'linux', 'darwin')
fails = []

def git(*a):
    return subprocess.run(['git', '-C', repo, *a], capture_output=True, check=True).stdout.decode('utf-8')

def show(path):
    return git('show', f'{ref}:{path}').replace('\r\n', '\n')

def ls(path):
    return [p for p in git('ls-tree', '--name-only', f'{ref}', path.rstrip('/') + '/').split('\n') if p]

def tsv(path):
    return list(csv.DictReader(io.StringIO(show(path)), delimiter='\t'))

def check(ok, label):
    print(('  ok    ' if ok else '  FAIL  ') + label)
    if not ok:
        fails.append(label)

def seg(matched, div):
    return f'{matched}' + (f' + {div}' if div else '')

sha = git('rev-parse', ref).strip()
tree = git('rev-parse', f'{ref}^{{tree}}').strip()
print(f'ref {ref} = {sha} (tree {tree})')

# --- The roster, HTML comments stripped -------------------------------------------------------------------------
ROW = re.compile(r'^\| \[`([^`]+)`\]\(')
SEGS = {o: re.compile(r' · ' + o + r': (\d+(?: \+ \d+)?|n/a)(?= · | \|$)') for o in ('linux', 'darwin')}
roster = {}
for line in show('docs/ValidatedTestPackages.md').split('\n'):
    m = ROW.match(line)
    if not m:
        continue
    bare = re.sub(r'<!--.*?-->', '', line)
    cells = re.split(r'(?<!\\)\|', bare)
    assert len(cells) == 6, (m.group(1), len(cells))
    r = {'windows': seg(int(cells[2]), int(cells[3]) if cells[3].strip() else 0)}
    proof = re.search(r' · \[proof\]\(([^)]+)\)', bare)   # the FIRST proof link is the row's own record
    r['proof'] = 'docs/' + proof.group(1) if proof else None
    for o, rx in SEGS.items():
        found = rx.findall(bare)
        assert len(found) <= 1, (m.group(1), o, found)
        r[o] = found[0] if found else None
    assert m.group(1) not in roster, m.group(1)
    roster[m.group(1)] = r
names = set(roster)
print(f'roster: {len(roster)} rows')
# eligible-<os>.txt lists the go1.26.10 POPULATION by test files; the four E-class rows outside the roster are unbanked
# and no leg dispatches them, so an OS's eligible set is its file's rows that the roster carries.
population = {o: set(show(f'docs/phase4/data/rebank-go1.26.10/eligible-{o}.txt').split()) for o in OSES}
eligible = {o: population[o] & names for o in OSES}
print('eligible: ' + ', '.join(f'{o} {len(eligible[o])}' for o in OSES)
      + f'; population rows outside the roster, never dispatched: {sorted(set.union(*population.values()) - names)}')

# --- The disclosure manifests ----------------------------------------------------------------------------------
def manifest(pkg):
    try:
        return json.loads(show(f'src/core/{pkg}/go2cs_test_disclosures.json')).get('disclosures', [])
    except subprocess.CalledProcessError:
        return []

def scope(entry, goos):
    """The entry's class on goos, or None when it is out of scope there (halves carry disjoint platforms)."""
    parts = entry.get('halves') or [entry]
    for p in parts:
        if not p.get('platforms') or goos in p['platforms']:
            return p.get('class', entry.get('class'))
    return None

def host_fatal_in_scope(pkg, goos):
    return sum(1 for e in manifest(pkg) if scope(e, goos) == 'host-fatal')

# =============================================================================================================
print('\nWINDOWS (PLAN-rebank-go126.md 15.2 at the freeze tip)')
win = {}
counts = collections.Counter()
for s in ('i9-S', 'H-S', 'G-S'):
    for r in tsv(f'{EV}/sweep/{s}.tsv'):
        counts[r['row']] += 1
        win[r['row']] = (s, r)
check(sum(counts.values()) == 238 and len(counts) == 238 and set(counts) == names,
      f'238 rows each exactly once: {sum(counts.values())} lines, {len(counts)} names, roster set equal {set(counts) == names}')
ruled = {'testing': ('COUNT', '82', '83', '1')}
odd = [(k, r['word'], r['count'], r['banked'], r['rc']) for k, (_, r) in win.items()
       if (r['word'], r['rc']) != ('PASS', '0') or r['count'] != r['banked']]
check(odd == [(k, *v) for k, v in ruled.items()], f'every word PASS at its banked count, rc 0, except the ruled class: {odd}')
check(all(r['real_drift'] == '0' and r['oracle_rerun'] == 'no' for _, r in win.values()), 'zero real drift, no oracle re-run')
off = [k for k, (_, r) in win.items() if r['count'] != roster[k]['windows'].split(' + ')[0]]
check(not off, f'every count equals the roster\'s windows matched count at this ref: {off}')
for s, dcol in (('i9-S', 'deleted_pre_restore'), ('H-S', 'del_census'), ('G-S', 'deleted')):
    text = show(f'{EV}/sweep/{s}.ledger.tsv')
    tip = re.search(r'[0-9a-f]{40}', text.split('\n')[0])
    lines = [l for l in text.split('\n') if l and not l.startswith('#') and not l.startswith('tree_head')
             and not re.fullmatch(r'[0-9a-f]{40}', l.strip())]
    hdr = lines[0].split('\t')
    rows = [dict(zip(hdr, l.split('\t'))) for l in lines[1:]]
    rows = [r for r in rows if r.get('row') and not r['row'].startswith('#')]
    vals = [r[dcol].split('=')[-1] for r in rows]
    if s == 'i9-S':
        vals += [r['porcelain_after'].split('=')[-1] for r in rows]
    end = [l for l in text.split('\n') if l.startswith('# SHARD END')]
    check(tip and tip.group(0) == TIP, f'{s} ledger tip line reads {tip.group(0)[:10] if tip else None}')
    check(len(rows) == sum(1 for v in win.values() if v[0] == s) and set(vals) == {'0'},
          f'{s}: {len(rows)} per-row deletion censuses, values {sorted(set(vals))}'
          + (f'; shard end in the ledger: {end[0][2:60]}...' if end else '; shard end: not in the ledger (see the evidence commit)'))

# =============================================================================================================
print('\nLINUX')
lin_ann = {k for k, r in roster.items() if r['linux'] not in (None, 'n/a')}
lin_na = {k for k, r in roster.items() if r['linux'] == 'n/a'}
check(lin_ann == eligible['linux'] and len(lin_ann) == 236, f'linux:-annotated set == eligible-linux.txt by name: {len(lin_ann)} / {len(eligible["linux"])}')
check(lin_na == {'internal/syscall/windows', 'internal/syscall/windows/registry'} and len(lin_ann | lin_na) == 238,
      f'n/a on exactly its 2 rows: {sorted(lin_na)}')
passes = {}
lc = collections.Counter()
for f in sorted(ls(f'{EV}/linux')):
    base = f.rsplit('/', 1)[-1]
    if base.endswith('.tsv') and '.ledger.' not in base:
        for r in tsv(f):
            lc[r['row']] += 1
            passes[r['row']] = (base[:-4], r)
check(set(lc) == eligible['linux'] and set(lc.values()) == {1},
      f'read + not dispatched = 236, each once: {len(lc)} read, {len(eligible["linux"] - set(lc))} not dispatched, max {max(lc.values())} per row')
rereads = {r['row']: ('rereads/P2-net', r) for r in tsv(f'{EV}/linux/rereads/P2-net.tsv')}
residual, hand = [], []
for k in sorted(lin_ann):
    stem, r = rereads.get(k, passes[k])
    if r['word'] != 'PASS':
        residual.append((k, r['word'], stem))
        continue
    want = seg(r['verdicts'] if r['matched'] == 'True' else r['matched'], int(r['disclosed']) - host_fatal_in_scope(k, 'linux'))
    if k in rereads:
        hand.append((k, want, passes[k][0], passes[k][1]['word']))
    if want != roster[k]['linux']:
        residual.append((k, roster[k]['linux'], want, stem))
check(not residual, f'residual set EMPTY (every numeric segment equals its 1.26.10.2 reading): {residual}')
print(f'        hand-applied under the ruling: {hand}')
bad = []
nled = 0
for f in sorted(ls(f'{EV}/linux')) + sorted(ls(f'{EV}/linux/rereads')):
    if f.endswith('.ledger.tsv'):
        for r in csv.DictReader(io.StringIO('\n'.join(l for l in show(f).split('\n') if not l.startswith('#'))), delimiter='\t'):
            nled += 1
            if r['corpus_commit'] != TIP or r['converter_commit'] != TIP:
                bad.append((f, r['row']))
check(not bad and nled == 237, f'every linux ledger line\'s corpus_commit and converter_commit read d9dd8f5499: {nled} lines (236 + net\'s re-read), {len(bad)} off')

# =============================================================================================================
print('\nDARWIN')
guard = show('src/check-roster-format.ps1')
def table(name):
    m = re.search(r'^\$' + name + r' = @\{(.*?)^\}', guard, re.M | re.S)
    return dict(re.findall(r"^\s*'([^']+)'\s*=\s*'((?:[^']|'')*)'", m.group(1), re.M)) if m else {}
agreements, ruledcounts = table('DarwinAgreementRulings'), table('DarwinRuledCounts')
print(f'        $DarwinAgreementRulings {sorted(agreements)}; $DarwinRuledCounts {sorted(ruledcounts) or "absent at this ref"}')
dar = collections.defaultdict(list)
for f in sorted(ls(f'{EV}/darwin')):
    for r in tsv(f):
        dar[r['row']].append((f.rsplit('/', 1)[-1][:-4], r))
of_record, superseded = {}, {}
for k, recs in dar.items():
    stems = sorted({s for s, _ in recs})
    if len(stems) > 1:
        assert all(stems[i].startswith(stems[i - 1]) for i in range(1, len(stems))), (k, stems)
        superseded[k] = stems[:-1]
    rs = [r for s, r in recs if s == stems[-1]]
    arm = [r for r in rs if r['RID'] == 'osx-arm64']
    x64 = [r for r in rs if r['RID'] == 'osx-x64']
    assert len(arm) == 1 and len(x64) == 1, (k, len(arm), len(x64))
    of_record[k] = (stems[-1], arm[0], x64[0])
check(set(dar) == eligible['darwin'] and len(dar) == 236,
      f'read + not dispatched = 236, each once per RID in its reading of record: {len(dar)} read, {len(eligible["darwin"] - set(dar))} not dispatched; superseded {superseded}')

def darwin_bank(k, a, x):
    da, dx = int(a['divergent']) - int(a['new']), int(x['divergent']) - int(x['new'])
    agreed = a['agree'] == x['agree'] == 'yes' or k in agreements
    if k in ruledcounts:
        m, d = ruledcounts[k].split(' -- ')[0].split('/')
        ok = a['status'] == x['status'] == 'COUNT' and agreed and a['matched'] == x['matched'] == m and da == dx == int(d)
        return (seg(m, da) if ok else None), 'ruled count'
    if a['status'] == x['status'] == 'validated' and agreed and a['matched'] == x['matched'] and da == dx:
        return seg(a['matched'], da), 'agreement ruling' if a['agree'] != 'yes' else ''
    return None, ''

banked, unbanked, mismatch = {}, [], []
for k in sorted(of_record):
    s, a, x = of_record[k]
    want, via = darwin_bank(k, a, x)
    if want is None:
        unbanked.append((k, s, a, x))
        if roster[k]['darwin'] is not None:
            mismatch.append((k, roster[k]['darwin'], 'no segment', s))
    else:
        banked[k] = (want, via)
        if roster[k]['darwin'] != want:
            mismatch.append((k, roster[k]['darwin'], want, s))
dar_ann = {k for k, r in roster.items() if r['darwin'] not in (None, 'n/a')}
check(dar_ann == set(banked), f'darwin:-annotated set == the banked readings: {len(dar_ann)} annotated, {len(banked)} bank, of 236 eligible')
check(not mismatch, f'every darwin segment equals its 1.26.10.2 reading, and every unbanked row carries none: {mismatch}')
print(f'        banked under a ruling: {[(k, v) for k, v in banked.items() if v[1]]}')
print('        eligible rows that do not bank (dispatch; osx-arm64 | osx-x64 as status matched/disclosed-divergent/new, agree):')
for k, s, a, x in unbanked:
    fmt = lambda r: f"{r['status']} {r['matched']}/{int(r['divergent']) - int(r['new'])}/{r['new']} agree {r['agree']}"
    print(f'          {k:32} {s:7} {fmt(a):34} | {fmt(x)}  roster: {roster[k]["darwin"] or "no segment"}')
if use_api:
    runs = {}
    for line in git('log', '--format=%s', ref, '--', f'{EV}/darwin').split('\n'):
        m = re.search(r're-pass: (C1-\S+), run (\d+) ', line)
        if m:
            runs[m.group(1)] = m.group(2)
    heads = {}
    for s, rid in sorted(runs.items()):
        req = urllib.request.Request(f'https://api.github.com/repos/ritchiecarroll/go2cs/actions/runs/{rid}',
                                     headers={'User-Agent': 'go2cs-closing-checks', 'Accept': 'application/vnd.github+json'})
        heads[s] = json.load(urllib.request.urlopen(req, timeout=30))['head_sha']
    stems = {f.rsplit('/', 1)[-1][:-4] for f in ls(f'{EV}/darwin')}
    check(set(runs) == stems and set(heads.values()) == {TIP},
          f'every darwin run\'s headSha reads d9dd8f5499: {len(heads)} runs for {len(stems)} TSVs, heads {sorted({h[:10] for h in heads.values()})}')
else:
    print('  SKIP  darwin headSha (--no-api)')

# =============================================================================================================
print('\nALL THREE PLATFORMS: disclosures orphaned on every OS')
# Per OS, an entry is NEEDED when that OS's reading of the row uses it, FREE when it is out of scope there or the
# reading uses no disclosure at all, and UNKNOWN when the reading uses some but the record does not name which.
#  - windows names them: the row's proof page marks each disclosed verdict (`## Verdicts`, the go2cs cell's leading
#    token, then `[disclosed]`), checked against the roster's windows count at this ref;
#  - linux and darwin record COUNTS only: U = disclosed - host-fatal in scope (linux), divergent - new per RID (darwin).
#    U = 0 frees every in-scope entry; U = M, the verdicts the in-scope non-host-fatal entries can cover, needs every
#    one; anything else is UNKNOWN for that row. M counts each subtest entry's parents too: a disclosed subtest fails
#    its parent, which the converter discloses with it (crypto/internal/fips140test's page: 7 entries, 8 disclosed
#    verdicts, TestNISTECAllocations among them).
#  - a host-fatal entry in scope withdraws its test before either side runs, so no run can read it passing: it is
#    NEEDED wherever it is in scope (the orphan predicate is a terminal pass, DESIGN-orphan-disclosure-check.md 3).
# An entry is ORPHANED ON ALL THREE when it is FREE on every OS.
manifests = sorted(p[len('src/core/'):-len('/go2cs_test_disclosures.json')]
                   for p in git('ls-tree', '-r', '--name-only', ref, 'src/core').split('\n') if p.endswith('/go2cs_test_disclosures.json'))
outside = {p: len(manifest(p)) for p in manifests if p not in names}
def page_marks(pkg):
    text = show(roster[pkg]['proof'])
    head = re.search(r'\*\*(\d+) matched(?: · (\d+) disclosed)?\*\*', text)
    sect = text.split('## Verdicts', 1)[1].split('\n## ', 1)[0]
    marked, seen = set(), set()
    for l in sect.split('\n'):
        c = [x.strip() for x in l.strip().strip('|').split('|')]
        if len(c) == 3 and c[0].startswith('`'):
            seen.add(c[0].strip('`'))
            if '[disclosed]' in c[2]:
                marked.add(c[0].strip('`'))
    return seg(int(head.group(1)), int(head.group(2) or 0)), marked, seen
result, basis = collections.Counter(), collections.Counter()
orphans, unknown, stale_pages, over, bounds = [], [], [], [], {}
for pkg in manifests:
    if pkg not in names:
        continue
    entries = manifest(pkg)
    if not entries:
        continue
    page_seg, marked, seen = page_marks(pkg)
    if page_seg != roster[pkg]['windows'] or len(marked) != int((roster[pkg]['windows'].split(' + ') + ['0'])[1]):
        stale_pages.append((pkg, page_seg, roster[pkg]['windows'], len(marked)))
    used = {}
    s, r = rereads.get(pkg, passes.get(pkg, (None, None)))
    used['linux'] = [int(r['disclosed']) - host_fatal_in_scope(pkg, 'linux')] if r else [None]
    _, a, x = of_record[pkg] if pkg in of_record else (None, None, None)
    used['darwin'] = [int(z['divergent']) - int(z['new']) for z in (a, x)] if a else [None]
    for e in entries:
        state = {}
        for o in OSES:
            c = scope(e, o)
            if c is None:
                state[o] = 'free:out-of-scope'
            elif c == 'host-fatal':
                state[o] = 'needed:host-fatal'
            elif o == 'windows':
                state[o] = 'needed:named' if e['name'] in marked else ('free:not-marked' if e['name'] in seen else 'free:not-on-page')
            else:
                cover = set()
                for f in entries:
                    if scope(f, o) not in (None, 'host-fatal'):
                        parts = f['name'].split('/')
                        cover |= {'/'.join(parts[:i]) for i in range(1, len(parts) + 1)}
                m = len(cover)
                bounds[(pkg, o)] = (m, used[o])
                us = used[o]
                if None in us or (o == 'linux' and pkg in lin_na):
                    state[o] = 'free:not-run'
                elif any(u > m for u in us):
                    over.append((pkg, o, us, m)); state[o] = 'unknown'
                elif any(u == m for u in us):
                    state[o] = 'needed:count'
                elif all(u == 0 for u in us):
                    state[o] = 'free:count-0'
                else:
                    state[o] = 'unknown'
        verdict = ('orphaned' if all(v.startswith('free') for v in state.values()) else
                   'needed' if any(v.startswith('needed') for v in state.values()) else 'unknown')
        result[verdict] += 1
        if verdict == 'needed':
            basis[next(k for k in ('needed:named', 'needed:host-fatal', 'needed:count') if k in state.values())] += 1
        (orphans if verdict == 'orphaned' else unknown if verdict == 'unknown' else []).append((pkg, e['name'], scope(e, 'windows') or e.get('class'), state))
print(f'        {len(manifests)} manifests at this ref; {len(outside)} have no roster row (no reading on any OS): {outside}')
print(f'        entries over roster rows: {sum(result.values())} = needed {result["needed"]} + orphaned on all three {result["orphaned"]} + unknown {result["unknown"]}')
print(f'        needed, by basis (named on windows, else host-fatal in scope, else a count): {dict(basis)}')
check(not over, f'no reading uses more disclosures than its manifest scopes in: {over}')
check(not stale_pages, f'every proof page reads the roster\'s windows count and marks that many disclosed verdicts: {stale_pages}')
print('        ORPHANED ON ALL THREE:')
for pkg, n, c, st in orphans:
    print(f'          {pkg} {n} [{c}] ' + ', '.join(f'{o} {st[o]}' for o in OSES))
print('        UNKNOWN (needed on no OS, and a linux or darwin count that cannot name it):')
for pkg, n, c, st in unknown:
    print(f'          {pkg} {n} [{c}] ' + ', '.join(f'{o} {st[o]}' for o in OSES))
print('        the counts behind each UNKNOWN (row, OS: M verdicts its in-scope entries can cover, U disclosed-divergent read):')
for (pkg, o) in sorted({(p, o) for p, _, _, st in unknown for o in OSES if st[o] == 'unknown'}):
    m, us = bounds[(pkg, o)]
    print(f'          {pkg} {o}: M {m}, U {us}; so {", ".join(str(m - u) for u in us)} of the {m} are not disclosed there')

print(f'\n{len(fails)} check(s) failed')
sys.exit(1 if fails else 0)
