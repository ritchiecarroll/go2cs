# tL-helpers.py -- TRAIN L battery helpers (COORD, i7). DRAFT 2026-10-01: tK-helpers.py VERBATIM (precheck, coverage,
# canaries, trx, treport, csprojdrift) plus four L readers appended at the end (rosterrow, outparity, teattr, wallcmp).
# precheck's K8/K9/K6 asserts are carried as REGRESSION asserts: at the L union (6960c8071f) they read K8 (1,1,0),
# K6 x1 each, and runtime/*/package_info.cs is unchanged since K (measured read-only 2026-10-01); a red there means an
# L seat disturbed a K resolution. Pure readers: git, the tree, go list and the artifacts a leg wrote. Nothing here
# builds, converts, edits or runs a test. Every subcommand prints its findings and exits non-zero only when a HARD rule
# fails (the battery decides what a non-zero means for its leg).
#   precheck    <repo> <base> <seats-file>        registration count asserts (projitems/slnx/4 BehavioralTests files),
#                                                 K6/K8/K9 regression asserts, S1 census, optin check, markers
#   coverage    <repo> <i7-list> <i9-list>        roster rows vs the two sweep lists (MISSING / EXTRA / DUP)
#   canaries    <repo>                            the five largest banked reflect importers, derived now (go list)
#   trx         <trx> [class ...]                 dotnet test TRX: totals, failed names, per-named-class outcomes
#   treport     --dir D --log L --t0 E --rc N [--named a,b] [--env x,y] [--bank] [--label S]
#                                                 a -tests leg's or sweep row's reading, with the freshness check
#   csprojdrift <repo> <M> <U>                    committed csproj M vs U: the S1 + opt-in template edits and nothing else
#   rosterrow   <repo> <pkg> <validated> <disclosed-divergent>
#                                                 a T leg's reading vs the roster of record (col 2 matched, col 3 disclosed)
#   outparity   <repo>                            behavioral package_info [GoTestMatchingConsoleOutput] set vs the
#                                                 OutputComparisonTests.cs Check list (TestOutputComparisonList...'s rule)
#   teattr      <patch> [<patch> ...]             TE: added lines in sweep-rewritten sources carrying the D6 or the
#                                                 cross-package embed-hop signature (INFERRED patterns; expect 0)
#   wallcmp     <L-summary> <K-summary> [...]     ROW legs' wall (S:/T:/NR:) L vs the fastest PASSING K reading; > 1.25x listed
#   DRAFT 2 additions (CHANGES.md):
#   trxmethods  <trx> <class> <regex>             R3: a class's methods matching a name regex, FOUND (names+outcomes) / NOT-FOUND
#   deadlock    --log L [--dir D --t0 E --label S] deadlock-line count: console + 'full output:' file + fresh record stderr tails
#   cmpnames    <comparison.json> <go> <cs> <name>...  a control's predicted verdicts by name from its own record
#   lockcheck   <go2cs.modules.lock> <go.sum> <module@version>...  MR2: lock hashes == go.sum h1 lines; exact module set
import json, os, re, subprocess, sys, time
import xml.etree.ElementTree as ET

def git(repo, *a, check=True):
    r = subprocess.run(['git', '-C', repo] + list(a), capture_output=True)
    if check and r.returncode != 0:
        sys.exit(f'HELPER-FAIL git {" ".join(a)}: {r.stderr.decode("utf-8", "replace").strip()}')
    return r.stdout.decode('utf-8', 'surrogateescape')

def blob(repo, ref, path):
    return git(repo, 'show', f'{ref}:{path}')

def seat_shas(seats_file):
    out = []
    for line in open(seats_file, encoding='utf-8'):
        m = re.match(r'^([A-Za-z0-9._-]+)\|([0-9a-f]{7,40})\|', line)
        if m: out.append((m.group(1), m.group(2)))
    return out

ROW = re.compile(r'^\|\s*\[`([^`]+)`\]\([^)]*\)\s*\|\s*(\d+)\s*\|\s*(\d*)\s*\|', re.M)
def roster_rows(repo, ref='HEAD'):
    return [(m.group(1), int(m.group(2))) for m in ROW.finditer(blob(repo, ref, 'docs/ValidatedTestPackages.md'))]

# ---------------------------------------------------------------------------------------------------------------- precheck
COUNT_FILES = ['src/go2cs/go2cs-src.projitems', 'src/go2cs.slnx'] + [
    f'src/tests/Behavioral/BehavioralTests/{n}.cs' for n in ('CompileTests', 'OutputComparisonTests', 'TargetComparisonTests', 'TranspileTests')]
COND = "<LangVersion Condition=\"'$(LangVersion)'==''\">14</LangVersion>"
LATEST = '<LangVersion>latest</LangVersion>'
# The S1 REGENERATION ruling names ElemAliasProbe / RidCompileAsset / UpdateTestTargets. J0UuidConsumer.csproj
# (i9-j0-uuid-rehearsal, new in K, hand-written, names no LangVersion) is added BY NAME as a RULING AMENDMENT for COORD
# to confirm (tK-s1census.py already lists it); any other (0,0,0) is a hard FAIL, never a soft note.
S1_NONE = {'ElemAliasProbe.csproj', 'RidCompileAsset.csproj', 'UpdateTestTargets.csproj', 'J0UuidConsumer.csproj'}

def precheck(repo, base, seats_file):
    hard, soft = [], []
    seats = seat_shas(seats_file)
    # PRERES-K1 / K3: each registration file == base + the sum of each seat's OWN net inserts. A seat that is an
    # ancestor of another seat touching the same file is counted once (inside its descendant), never twice.
    for f in COUNT_FILES:
        b = blob(repo, base, f).count('\n'); h = blob(repo, 'HEAD', f).count('\n')
        touch = []
        for name, sha in seats:
            ns = git(repo, 'diff', '--numstat', f'{base}...{sha}', '--', f).split()
            if ns and ns[0].isdigit(): touch.append((name, sha, int(ns[0]) - int(ns[1])))
        kept = [t for t in touch if not any(t[1] != u[1] and subprocess.run(
            ['git', '-C', repo, 'merge-base', '--is-ancestor', t[1], u[1]]).returncode == 0 for u in touch)]
        exp = b + sum(t[2] for t in kept)
        line = f'COUNT {f}: base={b} head={h} expect={exp} :: ' + ' '.join(f'{t[0]}:{t[2]:+d}' for t in kept)
        (hard if h != exp else soft).append(('FAIL ' if h != exp else 'ok   ') + line)
    # PRERES-K8: the gated Register, decisive asserts (G review 12:38).
    g = open(f'{repo}/src/core/golib/runtime/Goroutine.cs', encoding='utf-8', errors='replace').read()
    k8 = (g.count('m_profileLabels = '), g.count('s_live[goroutine.Id] = goroutine;'), g.count('<<<<<<<'))
    (soft if k8 == (1, 1, 0) else hard).append(f'{"ok  " if k8 == (1, 1, 0) else "FAIL"} K8 Goroutine.cs m_profileLabels= {k8[0]} (want 1), s_live insert {k8[1]} (want 1), markers {k8[2]}')
    # PRERES-K9: census rows = base + 2 (both rows present); each changed map line equals its seat's line, x3 GOOS.
    p = 'src/tests/GolibTests/ThreadStateCensusTests.cs'
    rows = lambda t: len(re.findall(r'^\s*\("[^"]+\|[^"]+", Disposition\.', t, re.M))
    u = open(f'{repo}/{p}', encoding='utf-8', errors='replace').read(); rb, ru = rows(blob(repo, base, p)), rows(u)
    ok9 = ru == rb and 't_chunk' in u and 't_runtimeLockProfilePending' in u   # L: a REGRESSION assert (K added the +2; L's base carries them)
    (soft if ok9 else hard).append(f'{"ok  " if ok9 else "FAIL"} K9 census rows {rb} -> {ru} (want == base, a K regression assert; t_chunk and t_runtimeLockProfilePending present: {"t_chunk" in u and "t_runtimeLockProfilePending" in u})')
    for os_ in ('windows', 'linux', 'darwin'):
        pi = f'src/core/runtime/{os_}/package_info.cs'; cur = open(f'{repo}/{pi}', encoding='utf-8', errors='replace').read()
        for src, ref in (('print.go', 'ed7859d20e'), ('runtime.go', 'ed7859d20e'), ('chan.go', 'b9c8948630'),
                         ('lock_spinbit.go', 'b9c8948630'), ('lockrank_off.go', 'b9c8948630')):
            pat = re.compile(r'^\[assembly: go\.GoPositionMap\("runtime/' + re.escape(src) + r'".*$', re.M)
            want = pat.findall(blob(repo, ref, pi).replace('\r', ''))
            if len(want) != 1: continue
            if pat.findall(cur.replace('\r', '')) != want:
                hard.append(f'FAIL K9 {pi}: the map line for {src} is not {ref}\'s')
    soft.append('ok   K9 package_info map lines checked x3 (a mismatch is listed as FAIL above)')
    # PRERES-K6: the fixup refreshed os/{linux,darwin}/file_unix.cs from the union's seeded emission.
    for os_ in ('linux', 'darwin'):
        n = open(f'{repo}/src/core/os/{os_}/file_unix.cs', encoding='utf-8', errors='replace').read().count('StackTraceHidden')
        (soft if n >= 1 else hard).append(f'{"ok  " if n >= 1 else "FAIL"} K6 os/{os_}/file_unix.cs StackTraceHidden x{n} (want >= 1; 0 at base)')
    # Conflict markers anywhere under src/.
    mk = git(repo, 'grep', '-nE', '^(<<<<<<<|>>>>>>>) ', 'HEAD', '--', 'src', check=False).strip()
    (hard if mk else soft).append(('FAIL markers: ' + mk.replace('\n', ' | ')[:600]) if mk else 'ok   no conflict markers under src/')
    # S1 completeness census (P1's recipe, 05:22): (conditioned-14, latest, any LangVersion) per tracked csproj.
    tally = {}
    for path in git(repo, 'ls-files', '-z', '*.csproj').split('\0'):
        if not path or path.startswith(('src/archived/', 'docs/')): continue
        t = open(f'{repo}/{path}', encoding='utf-8', errors='replace').read()
        tri = (t.count(COND), t.count(LATEST), t.count('<LangVersion'))
        base_ = path.rsplit('/', 1)[-1]
        if base_ == 'go2cs-gen.csproj': want = (0, 0, 1)
        elif base_ in S1_NONE: want = (0, 0, 0)
        else: want = (1, 0, 1)
        if tri == want: tally['ok'] = tally.get('ok', 0) + 1; continue
        kind = 'UNLISTED-NONE' if tri == (0, 0, 0) else 'DOUBLE EDIT' if tri[0] >= 2 else 'MISS' if tri[1] >= 1 else 'UNEXPECTED'
        hard.append(f'FAIL S1 {kind} {tri} want {want}: {path}')
    soft.append(f'ok   S1 census: {tally.get("ok", 0)} csproj read as ruled')
    # C2's opt-in completeness check, run exactly as the recipe states (read-only).
    r = subprocess.run([sys.executable, f'{repo}/docs/phase4/recipes/quickjit-optin/optin.py', 'check', repo], capture_output=True, text=True)
    last = (r.stdout.strip().splitlines() or [''])[-1]
    ((soft if r.returncode == 0 and 'CHECK PASS' in r.stdout else hard)
     .append(f'{"ok  " if r.returncode == 0 else "FAIL"} optin.py check rc={r.returncode}: {last[:200]}'))
    for l in soft: print(l)
    for l in hard: print(l)
    print(f'PRECHECK hard-failures={len(hard)}')
    return 1 if hard else 0

# ---------------------------------------------------------------------------------------------------------------- coverage
def coverage(repo, i7, i9):
    rows = {p for p, _ in roster_rows(repo)}
    rd = lambda f: [l.strip() for l in open(f, encoding='utf-8') if l.strip() and not l.startswith('#')]
    a, b = rd(i7), rd(i9)
    for p in sorted(rows - set(a) - set(b)): print('MISSING', p)
    for p in sorted(set(a) - rows): print('EXTRA', p)
    for p in sorted(set(b) - rows): print('EXTRA-I9', p)
    for p in sorted(set(a) & set(b)): print('DUP', p)
    ex, ex9, dup = set(a) - rows, set(b) - rows, set(a) & set(b)
    dl = len(a) - len(set(a)) + len(b) - len(set(b))   # a row listed twice inside one list
    print(f'COVERAGE roster={len(rows)} i7={len(a)} i9={len(b)} missing={len(rows - set(a) - set(b))} '
          f'extra={len(ex)} extra-i9={len(ex9)} dup={len(dup)} dup-in-list={dl}')
    # Non-zero on a roster read of 0 rows (a format change reads 0, drops every i7 row as EXTRA and sweeps nothing),
    # on any EXTRA/EXTRA-I9/DUP, or when the roster is not exactly the two lists' disjoint union.
    bad = []
    if not rows: bad.append('roster read 0 rows')
    if ex or ex9 or dup or dl: bad.append('EXTRA/EXTRA-I9/DUP present')
    if len(rows) != len(a) + len(b): bad.append(f'roster {len(rows)} != i7 {len(a)} + i9 {len(b)}')
    print('COVERAGE-VERDICT ' + ('OK' if not bad else 'FAILED: ' + '; '.join(bad)))
    return 1 if bad else 0

# ---------------------------------------------------------------------------------------------------------------- canaries
def canaries(repo):
    rows = roster_rows(repo)
    fmt = '{{.ImportPath}}|{{join .Imports ","}}|{{join .TestImports ","}}|{{join .XTestImports ","}}'
    r = subprocess.run(['go', 'list', '-e', '-f', fmt] + [p for p, _ in rows], capture_output=True, text=True, cwd=os.environ.get('TEMP') or None)
    imp = {}
    for line in r.stdout.splitlines():
        f = line.split('|')
        if len(f) == 4: imp[f[0]] = 'reflect' in ','.join(f[1:]).split(',')
    ctl = {'encoding/json': True, 'cmp': False, 'go/doc/comment': False}
    bad = [f'{k} (want {"IN" if v else "OUT"}, read {imp.get(k)})' for k, v in ctl.items() if imp.get(k) is not v]
    top = sorted([(c, p) for p, c in rows if imp.get(p)], reverse=True)[:5]
    for c, p in top: print(f'CANARY {p} {c}')
    print(f'CANARY-CONTROLS {"OK" if not bad else "FAILED: " + "; ".join(bad)} (go list rc={r.returncode}, {len(imp)} of {len(rows)} rows read)')
    return 0 if not bad and len(top) == 5 else 1

# ---------------------------------------------------------------------------------------------------------------- trx
def trx(path, classes):
    ns = {'t': 'http://microsoft.com/schemas/VisualStudio/TeamTest/2010'}
    root = ET.parse(path).getroot()
    cls = {u.get('id'): (u.find('t:TestMethod', ns).get('className') or '').split(',')[0].strip().rsplit('.', 1)[-1] for u in root.iter(f'{{{ns["t"]}}}UnitTest')}
    per, failed, tot, byname = {}, [], {}, {}
    for r in root.iter(f'{{{ns["t"]}}}UnitTestResult'):
        o = r.get('outcome'); c = cls.get(r.get('testId'), '?'); tot[o] = tot.get(o, 0) + 1
        per.setdefault(c, {}).setdefault(o, 0); per[c][o] += 1
        byname.setdefault(f'{c}.{r.get("testName")}', []).append(o)
        if o == 'Failed': failed.append(f'{c}.{r.get("testName")}')
    print('TRX totals ' + ' '.join(f'{k}={v}' for k, v in sorted(tot.items())))
    print('TRX failed: ' + (' '.join(failed)[:800] or '(none)'))
    for c in classes:
        if '.' in c:   # Class.Method: that ONE test's outcome(s) by name (data rows share the method name)
            o = byname.get(c)
            print(f'TRX test {c}: ' + (','.join(o) if o else 'NOT FOUND (0 results)'))
            continue
        d = per.get(c)
        print(f'TRX class {c}: ' + (' '.join(f'{k}={v}' for k, v in sorted(d.items())) if d else 'NOT FOUND (0 results)'))
    return 0

# ---------------------------------------------------------------------------------------------------------------- treport
def treport(argv):
    a = {'--named': '', '--env': '', '--label': '', '--bank': False}
    i = 0
    while i < len(argv):
        if argv[i] == '--bank': a['--bank'] = True; i += 1
        else: a[argv[i]] = argv[i + 1]; i += 2
    d, log, t0, rc, label = a['--dir'], a['--log'], float(a['--t0']), int(a['--rc']), a['--label']
    cp, rp = os.path.join(d, 'go2cs_test_comparison.json'), os.path.join(d, 'go2cs_test_results.json')
    mt = lambda p: os.stat(p).st_mtime if os.path.exists(p) else None
    cm, rm = mt(cp), mt(rp)
    fresh_c, fresh_r = cm is not None and cm > t0, rm is not None and rm > t0
    hm = lambda m: 'absent' if m is None else time.strftime('%H:%M:%S', time.localtime(m))
    print(f'{label} FRESHNESS leg-start={hm(t0)} comparison={hm(cm)} ({"fresh" if fresh_c else "STALE"}) '
          f'results={hm(rm)} ({"fresh" if fresh_r else "STALE"}) results-vs-comparison={"n/a" if None in (cm, rm) else f"{rm - cm:+.0f}s"}')
    tail = ''
    if rm is not None:
        with open(rp, 'rb') as f:
            f.seek(max(0, os.path.getsize(rp) - 6000)); tail = f.read().decode('utf-8', 'replace')
    pk = re.findall(r'"test"\s*:\s*""\s*,\s*"action"\s*:\s*"([a-z]+)"(?:[^}]*?"output"\s*:\s*"([^"]{0,120}))?', tail)
    timeout = any(x[0] == 'timeout' for x in pk)
    print(f'{label} RESULTS-TAIL package-events={[x[0] for x in pk][-3:]} timeout={"YES " + pk[-1][1] if timeout else "no"}')
    vline, val, dd, du, sk = '', None, 0, 0, 0
    if log and os.path.exists(log):
        for line in open(log, encoding='utf-8', errors='replace'):
            if not line.lstrip().startswith('{') and 'tests against go test' in line and 'Validated' in line: vline = line.strip()
    if vline:
        val = int(re.search(r'Validated (\d+) tests', vline).group(1))
        g = lambda pat: int(m.group(1)) if (m := re.search(pat, vline)) else 0
        dd, du, sk = g(r'(\d+) disclosed-divergent'), g(r'(\d+) disclosed-unsupported'), g(r'(\d+) skipped identically')
    print(f'{label} VALIDATED-LINE ' + (vline[vline.index('Validated'):][:300] if vline else '(none in the log)'))
    errs, disc, wd, status, matched, gv, cv, rawerr = [], set(), set(), '?', None, {}, {}, None
    if cm is not None:
        c = json.load(open(cp, encoding='utf-8-sig'))
        status, matched, gv, cv = c.get('status'), c.get('matched'), c.get('go') or {}, c.get('csharp') or {}
        disc = {s.split(' (')[0] for s in c.get('disclosed') or []}; wd = set(c.get('withdrawn') or [])
        # errs = the verdict-shaped entries ('<name>: Go=...'); rawerr counts EVERY entry, including the converter's
        # non-verdict errors (e.g. 'test disclosures: ...'), which the bank reading must also see.
        rawerr = len(c.get('errors') or [])
        errs = [m.group(1) for e in c.get('errors') or [] if (m := re.match(r'^(\S+): Go=', e))]
        diff = sorted(k for k in set(gv) | set(cv) if gv.get(k) != cv.get(k) and k not in disc and k not in wd)
        print(f'{label} COMPARISON status={status} matched={matched} go={len(gv)} cs={len(cv)} disclosed={len(disc)} '
              f'withdrawn={len(wd)} skipped={len(c.get("skipped") or [])} errors(raw)={rawerr} error-names={len(errs)} undisclosed-verdict-diffs={len(diff)}')
        if errs: print(f'{label} ERROR-NAMES ' + ' '.join(errs)[:900])
        env = [x for x in a['--env'].split(',') if x]
        if env and errs:
            only = set(errs) <= set(env)
            print(f'{label} ENV-CLASS {"NAMED i7 ENVIRONMENT DIVERGENCE ONLY" if only else "NOT ONLY the named environment set"} '
                  f'(named: {",".join(env)}; outside: {" ".join(sorted(set(errs) - set(env)))[:300] or "none"})')
        for pre in [x for x in a['--named'].split(',') if x]:
            ks = sorted(k for k in set(gv) | set(cv) if k == pre or k.startswith(pre + '/') or (pre.endswith('*') and k.startswith(pre[:-1])))
            if not ks: print(f'{label} NAMED {pre}: absent from both sides'); continue
            for k in ks[:6]:
                tag = 'disclosed' if k in disc else 'withdrawn' if k in wd else 'ERROR' if k in errs else ''
                print(f'{label} NAMED {k}: go={gv.get(k)} cs={cv.get(k)} {tag}')
            if len(ks) > 6: print(f'{label} NAMED {pre}: +{len(ks) - 6} more')
    if a['--bank']:
        ok = (rc == 0 and val is not None and fresh_c and fresh_r and not timeout and not errs
              and rawerr == 0 and matched is True)
        print(f'{label} BANK-READING rc={rc} validated={val} disclosed-divergent={dd} disclosed-unsupported={du} skipped-identically={sk} '
              f'status={status} matched={matched} errors(raw)={rawerr} error-names={len(errs)} fresh(comparison)={fresh_c} fresh(results)={fresh_r} timeout={timeout} '
              f'=> BANK-ELIGIBLE {"YES" if ok else "NO"}')
    return 0

# ---------------------------------------------------------------------------------------------------------------- csprojdrift
ANCHOR = '    <EnableCompressionInSingleFile>true</EnableCompressionInSingleFile>'
OPTIN = '<!-- <TieredCompilationQuickJitForLoops>false</TieredCompilationQuickJitForLoops> -->'
MAIN_SIG = 'Enable native compiled output optimizations'
HAND_OWNED = {'src/core/golib/golib.csproj'}

def csprojdrift(repo, M, U):
    names = git(repo, 'diff', '--name-status', '-z', '--no-renames', M, U, '--', '*.csproj').split('\0')
    pairs = [(names[i], names[i + 1]) for i in range(0, len(names) - 1, 2)]
    want = []
    for st, p in pairs:
        if st == 'M': want += [f'{M}:{p}', f'{U}:{p}']
        elif st == 'A': want.append(f'{U}:{p}')
    want.append(f'{U}:src/go2cs/csproj-template.xml')
    cat = subprocess.run(['git', '-C', repo, 'cat-file', '--batch'], input=('\n'.join(want) + '\n').encode(), capture_output=True).stdout
    blobs, i = {}, 0
    for key in want:
        nl = cat.index(b'\n', i); hdr = cat[i:nl].split()
        if hdr[-1] == b'missing': blobs[key] = None; i = nl + 1; continue
        n = int(hdr[2]); blobs[key] = cat[nl + 1:nl + 1 + n].decode('utf-8', 'surrogateescape').replace('\r', ''); i = nl + 1 + n + 1
    t = blobs[f'{U}:src/go2cs/csproj-template.xml'].split('\n'); j = t.index(ANCHOR); k = t.index('  </PropertyGroup>', j)
    block = t[j + 1:k]
    cnt, other, seat = {}, [], []
    bump = lambda key: cnt.__setitem__(key, cnt.get(key, 0) + 1)
    for st, p in pairs:
        if st == 'A':
            u = blobs[f'{U}:{p}'] or ''
            if '<LangVersion' not in u and MAIN_SIG not in u:   # a hand-written project that never named a version
                bump('new-handwritten-no-langversion'); seat.append(f'{p} (new, hand-written, no LangVersion: S1 has nothing to edit)'); continue
            okn =(COND in u or p.endswith('go2cs-gen.csproj')) and (MAIN_SIG not in u or p in HAND_OWNED or OPTIN in u)
            bump('new-ok' if okn else 'new-MISSING-EDIT')
            if not okn: other.append(f'NEW {p}')
            continue
        if st != 'M': bump(f'status-{st}'); other.append(f'{st} {p}'); continue
        m, u = blobs[f'{M}:{p}'], blobs[f'{U}:{p}']
        x = m.replace(LATEST, '<LangVersion>14</LangVersion>' if p.endswith('go2cs-gen.csproj') else COND)
        s1 = x != m
        opt = MAIN_SIG in x and p not in HAND_OWNED and OPTIN not in x and ANCHOR in x.split('\n')
        if opt:
            L = x.split('\n'); a_ = L.index(ANCHOR); L[a_ + 1:a_ + 1] = block; x = '\n'.join(L)
        if x == u: bump('template-only:' + ('s1+optin' if s1 and opt else 's1' if s1 else 'optin' if opt else 'none')); continue
        touched = git(repo, 'log', '--no-merges', '--format=%h', f'{M}..{U}^', '--', p).split()
        (seat if touched else other).append(f'{p} ({",".join(touched[:3]) or "no seat commit"})')
        bump('seat-edit' if touched else 'OTHER')
    print('CSPROJ M-vs-U ' + ' '.join(f'{k}={v}' for k, v in sorted(cnt.items())) + f' (changed csproj={len(pairs)})')
    for s in seat[:20]: print('  SEAT-EDIT ' + s)
    for s in other[:40]: print('  OTHER ' + s)
    print(f'CSPROJ verdict: {"TEMPLATE DRIFT ONLY (+ seat-edited hand-written projects)" if not other else "UNEXPLAINED csproj drift: " + str(len(other))}')
    return 1 if other else 0

# ================================================================================================ TRAIN L additions
# ---------------------------------------------------------------------------------------------------------------- rosterrow
def rosterrow(repo, pkg, val, dd):
    # A T leg runs the converter directly (K's tleg), so nothing compares its reading with the roster: this does.
    # The roster's column 2 is the matched count, column 3 the disclosed-divergent count (empty = 0).
    t = blob(repo, 'HEAD', 'docs/ValidatedTestPackages.md')
    m = [x for x in ROW.finditer(t) if x.group(1) == pkg]
    if len(m) != 1:
        print(f'ROSTERROW {pkg}: {len(m)} roster rows (want 1)'); return 1
    want_v, want_d = int(m[0].group(2)), int(m[0].group(3) or 0)
    got_v = None if val in ('', 'None') else int(val)
    got_d = int(dd or 0)
    ok = got_v == want_v and got_d == want_d
    print(f'ROSTERROW {pkg}: banked {want_v} + {want_d}, read {got_v} + {got_d} => {"AT BANKED COUNTS" if ok else "MOVED"}')
    return 0 if ok else 1

# ---------------------------------------------------------------------------------------------------------------- outparity
def outparity(repo):
    # The rule TestOutputComparisonListMatchesConsoleOutputAttribute enforces (src/go2cs/behavioralPackageInfoAttributes_test.go),
    # read from the TREE so the PRE section names the gap before leg C does. At 6960c8071f: 718 attributes vs 716 listed,
    # the gap = ChannelReceiveFromNil + ChannelSendToNil (g-deadlock-checkdead added the attribute, not the D4 rows).
    beh = os.path.join(repo, 'src', 'tests', 'Behavioral')
    attr = set()
    for d in sorted(os.listdir(beh)):
        p = os.path.join(beh, d, 'package_info.cs')
        # matched exactly as the guard matches it: a TRIMMED line equal to the attribute (a commented-out
        # '//[GoTestMatchingConsoleOutput] -- TODO' line, e.g. FirstClassFunctions, is not a mark)
        if os.path.isfile(p) and any(l.strip() == '[GoTestMatchingConsoleOutput]' for l in open(p, encoding='utf-8', errors='replace').read().replace('\r\n', '\n').split('\n')):
            attr.add(d)
    oc = open(os.path.join(beh, 'BehavioralTests', 'OutputComparisonTests.cs'), encoding='utf-8', errors='replace').read()
    listed = re.findall(r'CheckTarget\("([^"]+)"\)', oc)
    dup = sorted({x for x in listed if listed.count(x) > 1})
    a_only, l_only = sorted(attr - set(listed)), sorted(set(listed) - attr)
    print(f'OUTPARITY attributes={len(attr)} listed={len(listed)} attribute-not-listed={a_only} listed-no-attribute={l_only} dup={dup}')
    return 1 if a_only or l_only or dup else 0

# ---------------------------------------------------------------------------------------------------------------- teattr
# INFERRED signatures (from ForClauseSpill.cs at 6960c8071f and 8213221317's message), read on ADDED lines only:
#   D6-break   'if (!(<cond>)) break;'       COND lowered into the body
#   D6-header  'for (<init>; ᐧ ;'  / 'while (ᐧ)'   a condition-less header (the ᐧ placeholder)
#   D6-goto    'goto continueᴛN;' / 'continueᴛN:;'   a continue routed to the post label
#   XPKG-hop   an added line that equals one of its hunk's removed lines plus only inserted member-access runs
#              ('.Time', '.Value.Time', '.inner.Time' ...: tokens alternating '.' and an identifier) -- the
#              cross-package embed-path walk. POSITIVE CONTROL (2026-10-01, read-only): over 9c4aff7f62..8213221317's
#              golden diff this matches 6 of the 7 changed lines (all 9 call/method-value sites; the method-EXPRESSION
#              lambda rewrite is not a pure insertion and is not matched), and the D6 patterns match D6's own golden
#              diff f819887fa3..33fb0565f6 (break 2, header 2, goto 4).
D6 = [('D6-break', re.compile(r'\bif \(!\(.*\)\) break;')), ('D6-header', re.compile(r'(\bfor \([^;]*; ᐧ ;|\bwhile \(ᐧ\))')),
      ('D6-goto', re.compile(r'(\bgoto continueᴛ\d+;|^\s*continueᴛ\d+:;)'))]
TOK = re.compile(r'[A-Za-z_-￿][A-Za-z0-9_-￿]*|\s+|.')
IDENT = re.compile(r'^[A-Za-z_-￿][A-Za-z0-9_-￿]*$')
def hop_only(r, a):
    import difflib
    rt, at = TOK.findall(r.strip()), TOK.findall(a.strip())
    ops = difflib.SequenceMatcher(None, rt, at, autojunk=False).get_opcodes()
    ins = 0
    for tag, i1, i2, j1, j2 in ops:
        if tag == 'equal': continue
        if tag != 'insert': return False
        ch = at[j1:j2]
        if len(ch) < 2 or len(ch) % 2: return False
        kinds = ['dot' if t == '.' else 'id' if IDENT.match(t) else 'x' for t in ch]
        if 'x' in kinds or any(kinds[k] == kinds[k + 1] for k in range(len(kinds) - 1)): return False
        ins += 1
    return ins > 0

def teattr(patches):
    hits, files, hunks = {}, set(), 0
    for pf in patches:
        cur, rem, add = None, [], []
        def flush():
            nonlocal rem, add
            if cur is None: rem, add = [], []; return
            for a in add:
                for name, rx in D6:
                    if rx.search(a): hits.setdefault(name, []).append(f'{cur}: {a.strip()[:140]}')
                if any(hop_only(r, a) for r in rem):
                    hits.setdefault('XPKG-hop', []).append(f'{cur}: {a.strip()[:140]}')
            rem, add = [], []
        for line in open(pf, encoding='utf-8', errors='replace'):
            if line.startswith('diff --git '):
                flush(); cur = line.split(' b/', 1)[-1].strip(); files.add(cur)
            elif line.startswith('@@'):
                flush(); hunks += 1
            elif line.startswith('-') and not line.startswith('---'): rem.append(line[1:])
            elif line.startswith('+') and not line.startswith('+++'): add.append(line[1:])
        flush()
    print(f'TE files={len(files)} hunks={hunks} ' + ' '.join(f'{k}={len(hits.get(k, []))}' for k in ('D6-break', 'D6-header', 'D6-goto', 'XPKG-hop')))
    for k, v in hits.items():
        for x in v[:15]: print(f'TE {k} {x}')
    print('TE VERDICT ' + ('0 signature lines (D6 and the embed hop moved no swept test source)' if not hits else
                          f'{sum(len(v) for v in hits.values())} signature line(s): READ EACH (a match is a candidate, not a finding: the patterns are INFERRED)'))
    return 1 if hits else 0

# ---------------------------------------------------------------------------------------------------------------- wallcmp
def wallcmp(lsum, ksums):
    rx = re.compile(r'LEG (\S+) rc=(\d+) wall=(\d+)s')
    def walls(p):
        d = {}
        for line in open(p, encoding='utf-8', errors='replace'):
            m = rx.search(line)
            if m: d[re.sub(r'^(S\[\d+/\d+\]|S):', 'S:', m.group(1))] = (int(m.group(3)), int(m.group(2)))
        return d
    L = walls(lsum); K = {}
    for k in ksums:
        for n, v in walls(k).items(): K.setdefault(n, []).append(v)
    slow = []
    for n, (w, rc) in sorted(L.items()):
        ok = [x[0] for x in K.get(n, []) if x[1] == 0]   # K readings that PASSED (K's run-1 net/rpc died early at 94 s)
        # rows only (S:/T:/NR:): build legs are noise (K's own GT-Debug read 222 s and 456 s in two runs)
        if not ok or w < 60 or not n.startswith(('S:', 'T:', 'NR:')): continue
        kb = min(ok)                           # the FASTEST passing K reading of that leg (K's battery and its re-read)
        r = w / max(kb, 1)
        if r > 1.25: slow.append(f'{n} L={w}s K(min)={kb}s x{r:.2f} rc={rc}')
    print(f'WALLCMP legs-compared={sum(1 for n in L if n in K)} slower-than-1.25x={len(slow)}')
    for s in slow: print('WALLCMP SLOW ' + s)
    return 0

# ================================================================================================ DRAFT 2 additions
# ---------------------------------------------------------------------------------------------------------------- trxmethods
def trxmethods(path, cls, rx):
    # R3: read C1's owed field-of-element probe BY METHOD NAME (never by a result count: a data-row method also reads >1).
    # Prints ONE 'TRXM' line first: FOUND (names + outcomes) or NOT-FOUND; then any match in another class, as information.
    ns = {'t': 'http://microsoft.com/schemas/VisualStudio/TeamTest/2010'}
    root = ET.parse(path).getroot()
    meta = {}
    for u in root.iter(f'{{{ns["t"]}}}UnitTest'):
        tm = u.find('t:TestMethod', ns)
        meta[u.get('id')] = ((tm.get('className') or '').split(',')[0].strip().rsplit('.', 1)[-1], tm.get('name') or '')
    hit, other, pat = {}, {}, re.compile(rx)
    for r in root.iter(f'{{{ns["t"]}}}UnitTestResult'):
        c, m = meta.get(r.get('testId'), ('?', r.get('testName') or ''))
        if not pat.search(m): continue
        (hit if c == cls else other).setdefault(f'{c}.{m}', []).append(r.get('outcome'))
    fmt = lambda d: ' '.join(f'{k}={",".join(v)}' for k, v in sorted(d.items()))
    print(f'TRXM class={cls} /{rx}/ ' + (f'FOUND {len(hit)}: {fmt(hit)}' if hit else 'NOT-FOUND (no method of that class matches)'))
    if other: print(f'TRXM elsewhere (information, not the probe): {fmt(other)[:600]}')
    return 0

# ---------------------------------------------------------------------------------------------------------------- deadlock
def deadlock(argv):
    # Critic 0 #16: count "all goroutines are asleep" in the console log, in the row's saved full output (the sweep prints
    # 'full output: <path>' on a FAIL only), and in the FRESH comparison record's stderr tails (go and csharp sides). The
    # sweep console carries no host output on PASS, so the console count alone is structurally 0.
    a = dict(zip(argv[0::2], argv[1::2]))
    needle = 'all goroutines are asleep'
    log, d, t0, label = a.get('--log', ''), a.get('--dir', ''), float(a.get('--t0', '0') or 0), a.get('--label', '')
    txt = open(log, encoding='utf-8', errors='replace').read() if log and os.path.exists(log) else ''
    con = txt.count(needle)
    fo = re.findall(r'full output: (.+)', txt)
    fop = fo[-1].strip() if fo else ''
    if fop and os.path.exists(fop):
        fcount = str(open(fop, encoding='utf-8', errors='replace').read().count(needle))
    else:
        fcount = 'none' if not fop else 'path-unreadable'
    cp = os.path.join(d, 'go2cs_test_comparison.json') if d else ''
    rec, sg, sc = 'absent', 0, 0
    if cp and os.path.exists(cp):
        rec = 'fresh' if os.stat(cp).st_mtime > t0 else 'STALE'
        if rec == 'fresh':
            st = (json.load(open(cp, encoding='utf-8-sig')).get('stderr') or {})
            sg = ((st.get('go') or {}).get('text') or '').count(needle)
            sc = ((st.get('csharp') or {}).get('text') or '').count(needle)
    total = con + (int(fcount) if fcount.isdigit() else 0) + sg + sc
    print(f'DEADLOCK total={total} console={con} fullout={fcount} stderr(go)={sg} stderr(cs)={sc} record={rec} ({label})')
    return 0

# ---------------------------------------------------------------------------------------------------------------- cmpnames
def cmpnames(path, want_go, want_cs, *names):
    # Critic 0 #17 / #8: a control's predicted red read BY NAME from its own comparison record (never a line count).
    if not os.path.exists(path):
        print(f'CMPNAMES record absent: {path}'); return 2
    c = json.load(open(path, encoding='utf-8-sig')); g, s = c.get('go') or {}, c.get('csharp') or {}
    bad = 0
    for n in names:
        ok = g.get(n) == want_go and s.get(n) == want_cs
        bad += 0 if ok else 1
        print(f'CMPNAME {n}: go={g.get(n)} cs={s.get(n)} (want go={want_go} cs={want_cs}) {"ok" if ok else "MISMATCH"}')
    print(f'CMPNAMES {"OK" if not bad else f"MISMATCH {bad} of {len(names)}"} status={c.get("status")}')
    return 1 if bad else 0

# ---------------------------------------------------------------------------------------------------------------- lockcheck
def lockcheck(lockfile, gosum, *want):
    # Critic 0 #9 (MR2): every go2cs.modules.lock entry's hash equals the fixture's go.sum h1 line for that module and
    # version, and the locked module set is exactly the named '<module>@<version>' list.
    if not os.path.exists(lockfile):
        print(f'LOCKCHECK lock absent: {lockfile}'); return 2
    sums = set(l.strip() for l in open(gosum, encoding='utf-8') if l.strip())
    got, bad = set(), 0
    for line in open(lockfile, encoding='utf-8'):
        line = line.strip()
        if not line or line.startswith('#'): continue
        f = line.split()
        if len(f) != 4: print(f'LOCKCHECK malformed: {line}'); bad += 1; continue
        got.add(f'{f[0]}@{f[1]}')
        ok = f[2] != '-' and f'{f[0]} {f[1]} {f[2]}' in sums
        bad += 0 if ok else 1
        print(f'LOCKCHECK {f[0]} {f[1]} {f[2][:20]}... {"== go.sum" if ok else "NOT the go.sum h1 line"}')
    if set(want) != got:
        bad += 1; print(f'LOCKCHECK module set {sorted(got)} != want {sorted(want)}')
    print(f'LOCKCHECK {"OK" if not bad else f"FAILED ({bad})"} entries={len(got)}')
    return 1 if bad else 0

if __name__ == '__main__':
    try: sys.stdout.reconfigure(encoding='utf-8', errors='replace')   # L: identifiers carry ᐧ/ᴛ/Δ (cp1252 console)
    except Exception: pass
    cmd, rest = sys.argv[1], sys.argv[2:]
    rc = {'precheck': lambda: precheck(*rest), 'coverage': lambda: coverage(*rest), 'canaries': lambda: canaries(*rest),
          'trx': lambda: trx(rest[0], rest[1:]), 'treport': lambda: treport(rest), 'csprojdrift': lambda: csprojdrift(*rest),
          'rosterrow': lambda: rosterrow(*rest), 'outparity': lambda: outparity(*rest), 'teattr': lambda: teattr(rest),
          'wallcmp': lambda: wallcmp(rest[0], rest[1:]), 'trxmethods': lambda: trxmethods(*rest),
          'deadlock': lambda: deadlock(rest), 'cmpnames': lambda: cmpnames(*rest), 'lockcheck': lambda: lockcheck(*rest)}[cmd]()
    sys.exit(rc or 0)
