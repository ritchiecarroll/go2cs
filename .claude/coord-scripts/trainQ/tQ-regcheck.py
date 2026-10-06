#!/usr/bin/env python3
# R0 ONLY, NOT ADAPTED TO TRAIN Q (review round 1): this is trainP's pre-map reading tool under Q's name. It reads no ruled resolution (on Q's table every ruled row reads as a conflict), it needs git 2.44 or later through GITX, its comments below describe TRAIN P's pre-map, and it is on NO step of COORD-LAUNCH-CHECKLIST.md: the map of record is tQ-conflict-map.sh.
# tQ-regcheck.py <base> <head> <chain.tsv from tQ-premap.sh MODE=chain> [files...] (DRAFT 2026-10-04, UNCOMMITTED)
# Derived from trainO/tO-regcheck.py. Changes: the chain.tsv has six columns; a row the chain SKIPPED (its chain commit
# equal to the previous step's) is left out of the expectation and named; a row with SEVERAL best common ancestors
# (a re-cut on a local merge) is diffed against ort's merge of those ancestors, not the first one (O's Q20 / O3 class);
# and an IDENTICAL INSERT -- a -U0 hunk of the row's own diff that the union side of its step holds identically -- is
# credited once (git keeps one copy: O's D1 class), stamped on the writer.
# For each shared registration file (default: go2cs.slnx, go2cs-src.projitems, the four BehavioralTests lists), assert
#   COUNT: lines(head) == lines(base) + sum(each row's own net, own = merge-base(previous chain step, row)..row)
#   KEYS : every '+' line a row adds is present in head; every '-' line a row removes is absent (unless re-added by
#          another row); no line occurs in head more often than in base + the rows that add it (silent duplication).
# Controls 2026-10-04: see trainP/tP-premap.md section 0 (O's 38 rows replayed from 54f7f4439d; head = the bare base).
# REVIEW ROUND 1 (trainP/tP-CHANGES.md RR1-6): GD is read from the environment, as the shell scripts read it (it was a
# literal here). A row with TWO best common ancestors needs a merge of the two, which WRITES a tree: when no
# GIT_OBJECT_DIRECTORY is exported (a standalone run) that merge goes to a scratch object directory this script makes
# and removes, never to the shared store. A row with MORE than two is REFUSED by name (not-ok, rc 1): its own change
# cannot be synthesized here, and the first base alone is not a reading.
import subprocess, sys, collections, os, re, tempfile, shutil, atexit
GD = os.environ.get('GD', 'H:/Projects/go2cs/.git')
GITX = os.environ.get('GITX', 'C:/Program Files/Microsoft Visual Studio/2022/Community/Common7/IDE/CommonExtensions/Microsoft/TeamFoundation/Team Explorer/Git/cmd/git.exe')
NL = chr(10); CRNL = chr(13) + chr(10)
def git(*a):
    return subprocess.run(['git', '--git-dir=' + GD, '-c', 'core.quotepath=false'] + list(a), capture_output=True).stdout
def text(b):
    return b.decode('utf-8', 'replace').replace(CRNL, NL).split(NL)
base, head, rowsf = sys.argv[1], sys.argv[2], sys.argv[3]
if re.match(r'^/[A-Za-z]/', rowsf): rowsf = rowsf[1].upper() + ':' + rowsf[2:]   # a Git Bash path under MSYS_NO_PATHCONV=1
files = sys.argv[4:] or ['src/go2cs.slnx', 'src/go2cs/go2cs-src.projitems',
    'src/tests/Behavioral/BehavioralTests/CompileTests.cs', 'src/tests/Behavioral/BehavioralTests/OutputComparisonTests.cs',
    'src/tests/Behavioral/BehavioralTests/TargetComparisonTests.cs', 'src/tests/Behavioral/BehavioralTests/TranspileTests.cs']
rows = []; skipped = []; prev = git('rev-parse', base + '^{commit}').decode().strip()
for l in open(rowsf, encoding='utf-8'):
    f = l.rstrip(CRNL).split(chr(9))
    if len(f) < 4: continue
    n, s10, c = f[1], f[2], f[3]
    s = git('rev-parse', s10 + '^{commit}').decode().strip()
    c = git('rev-parse', c + '^{commit}').decode().strip()
    if c != prev: rows.append((n, s, prev))
    else: skipped.append(n)
    prev = c
if skipped: print('rows the chain left out (no expectation from them):', ' '.join(skipped))
HDR = re.compile(r'^@@ -(\d+)(?:,(\d+))? [+](\d+)(?:,(\d+))? @@')
def hunks(raw):
    out = []; cur = None
    for x in text(raw):
        m = HDR.match(x)
        if m:
            if cur: out.append((cur[0], cur[1], tuple(cur[2]), tuple(cur[3])))
            cur = [int(m.group(1)), int(m.group(2) or 1), [], []]
        elif cur is not None:
            if x.startswith('-') and not x.startswith('---'): cur[2].append(x[1:])
            elif x.startswith('+') and not x.startswith('+++'): cur[3].append(x[1:])
    if cur: out.append((cur[0], cur[1], tuple(cur[2]), tuple(cur[3])))
    return out
REF = {}; REFUSED = {}
def scratch_objects():
    # a merge writes a tree: never into the shared store. The caller's alternates (a pre-map OUT/objects) stay readable.
    if os.environ.get('GIT_OBJECT_DIRECTORY'): return
    d = tempfile.mkdtemp(prefix='tQ-regcheck-objects-')
    atexit.register(shutil.rmtree, d, True)
    alt = os.environ.get('GIT_ALTERNATE_OBJECT_DIRECTORIES', '')
    os.environ['GIT_OBJECT_DIRECTORY'] = d.replace(chr(92), '/')
    os.environ['GIT_ALTERNATE_OBJECT_DIRECTORIES'] = GD + '/objects' + (';' + alt if alt else '')
    print('NOTE: no GIT_OBJECT_DIRECTORY exported: the multi-base merge writes to a scratch object directory (%s), removed at exit; nothing goes to the shared store' % os.environ['GIT_OBJECT_DIRECTORY'])
def ownref(pv, s, name=''):
    # the commit or tree a row's OWN change is diffed against, and a tag for a multi-base row
    if (pv, s) in REF: return REF[(pv, s)]
    mbs = git('merge-base', '--all', pv, s).decode().split()
    ref = mbs[0]; tag = ''
    if len(mbs) == 2:
        scratch_objects()
        r = subprocess.run([GITX, '--git-dir=' + GD, 'merge-tree', '--write-tree', mbs[0], mbs[1]], capture_output=True)
        t = r.stdout.decode().split(NL)[0].strip()
        if r.returncode == 0 and len(t) == 40: ref = t; tag = ' MULTI-BASE(the two bases merged)'
        else: tag = ' MULTI-BASE(first base used: the two bases do not merge clean)'
    elif len(mbs) > 2:
        tag = ' MULTI-BASE(REFUSED: %d best common ancestors; the first base alone is not a reading)' % len(mbs)
        REFUSED[name or s[:10]] = len(mbs)
    REF[(pv, s)] = (ref, tag)
    return ref, tag
bad = 0
for f in files:
    bl = text(git('show', base + ':' + f)); hl = text(git('show', head + ':' + f))
    bc = collections.Counter(bl); hc = collections.Counter(hl)
    exp = len(bl); adds = collections.Counter(); rems = collections.Counter(); writers = []
    for n, s, pv in rows:
        ref, tag = ownref(pv, s, n)
        own = hunks(git('diff', '-U0', ref, s, '--', f))
        if not own: continue
        side = set(hunks(git('diff', '-U0', ref, pv, '--', f)))
        a = []; r = []; credit = 0
        for h in own:
            if h in side: credit += len(h[3]) - len(h[2]); continue
            r += h[2]; a += h[3]
        if credit: tag += ' identical-insert credit %+d' % -credit
        writers.append('%s(+%d/-%d%s)' % (n, len(a), len(r), tag))
        exp += len(a) - len(r); adds.update(a); rems.update(r)
    missing = [x for x in adds if hc[x] < 1]
    dup = [x for x in hc if x.strip() and hc[x] > bc[x] + adds[x]]
    back = [x for x in rems if hc[x] > max(0, bc[x] - rems[x]) + adds[x]]
    ok = (len(hl) == exp and not missing and not dup and not back)
    bad += 0 if ok else 1
    print('%s %s: base %d head %d expected %d writers=%d missing=%d dup=%d removed-back=%d' % ('ok' if ok else 'FAIL', f, len(bl), len(hl), exp, len(writers), len(missing), len(dup), len(back)))
    print('   writers:', ' '.join(writers))
    for x in missing[:5]: print('   missing:', x[:160])
    for x in dup[:5]: print('   dup:', x[:160])
    for x in back[:5]: print('   removed-back:', x[:160])
for n, k in sorted(REFUSED.items()):
    print('REFUSED row %s: %d best common ancestors with its chain step: its own change cannot be synthesized here (re-cut it on one base, or read it with the sequential git merge rehearsal)' % (n, k))
print('REGCHECK files=%d not-ok=%d%s' % (len(files), bad, ' refused-multi-base-rows=%d' % len(REFUSED) if REFUSED else ''))
sys.exit(1 if (bad or REFUSED) else 0)
