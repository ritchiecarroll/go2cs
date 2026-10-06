#!/usr/bin/env python3
# R0 ONLY, NOT ADAPTED TO TRAIN Q (review round 1): this is trainP's pre-map reading tool under Q's name. It reads no ruled resolution (on Q's table every ruled row reads as a conflict), it needs git 2.44 or later through GITX, its comments below describe TRAIN P's pre-map, and it is on NO step of COORD-LAUNCH-CHECKLIST.md: the map of record is tQ-conflict-map.sh.
# tQ-unioncheck.py <base> <head> (DRAFT 2026-10-04, UNCOMMITTED) -- what a CLEAN merge cannot say, read from git objects.
# No worktree, no build: every reading is a census of two trees (the base and a chain head from tQ-premap.sh).
#   A PROJITEMS  every top-level src/go2cs/*.go at head has exactly one go2cs-src.projitems entry and every entry a file
#                (the converter's own projitemsIntegrity_test reads the same thing under go test).
#   B BEHAVIORAL every behavioral project directory head adds (a <dir>/<dir>.csproj under src/tests/Behavioral) is in
#                go2cs.slnx once and in each of the four BehavioralTests lists once; its csproj's TrimMode count is
#                stamped (0 = a pre-N template: the CSPROJ_TEMPLATE class); a directory with main.go and no main.cs is H4.
#   C GODECL     duplicate top-level declarations in package main of src/go2cs (func, method by receiver, type), head
#                against base: two seats adding one name merge clean and compile nowhere. Build-tagged twins exist at the
#                base, so only names duplicated at head and NOT at base are listed. A declaration is read only where it
#                sits in CODE: the battery's lexical scan (tQ-battery.sh gotests, P13: block comment and raw string
#                carried across lines; line comments, interpreted strings and rune literals ended on their line).
#                REVIEW ROUND 1 (trainP/tP-CHANGES.md RR1-2): this census still used O's backtick PARITY, which P13 proved
#                wrong: measured at eb88ab9492 it skipped 161 real top-level declarations of package main (72 in
#                convCallExpr.go, three P writers; 60 in testConversion.go, four P writers), so two rows adding one
#                name in exactly those files was invisible. The scan's own control runs first (floor 13) and is stamped.
#   D HASHSET    MS19's scan over the Go lines head ADDS under src/go2cs (unqualified HashSet[ / NewHashSet): O removed
#                HashSet.go, so each hit is a compile error a merge cannot see.
#   E MARKERS    conflict markers in any file head changes against base (a union-carried chain must read 0).
#   F PKGINFO    src/core/**/package_info.cs head changes: the line kinds that moved (GoPositionMap only, or other), and
#                whether src/go2cs/stdlib-metadata.txt moved with them (merge-hazards: the preflight pair).
# Exit 0 always: a census, not a gate. Controls: trainP/tP-premap.md section 0.
import subprocess, sys, re, collections, os
GD = os.environ.get('GD', 'H:/Projects/go2cs/.git')   # review round 1: read from the environment, as the shell scripts read it
NL = chr(10); CR = chr(13); TAB = chr(9); BT = chr(96); BS = chr(92); DQ = chr(34); SQ = chr(39)
def git(*a):
    return subprocess.run(['git', '--git-dir=' + GD, '-c', 'core.quotepath=false'] + list(a), capture_output=True).stdout
def show(rev, path):
    return git('show', rev + ':' + path).decode('utf-8', 'replace').replace(CR, '')
def lstree(rev, path):
    out = {}
    for l in git('ls-tree', '-r', rev, '--', path).decode('utf-8', 'replace').split(NL):
        if not l: continue
        meta, p = l.split(TAB, 1); out[p] = meta.split()[2]
    return out
base, head = sys.argv[1], sys.argv[2]
print('UNIONCHECK base=%s head=%s tree=%s' % (git('rev-parse', '--short=10', base).decode().strip(), git('rev-parse', '--short=10', head).decode().strip(), git('rev-parse', '--short=12', head + '^{tree}').decode().strip()))

# A PROJITEMS
def projitems(rev):
    gof = sorted(p.split('/')[-1] for p in lstree(rev, 'src/go2cs/') if p.count('/') == 2 and p.endswith('.go'))
    ent = re.findall(r'Include="[$][(]MSBuildThisFileDirectory[)]([^"]+)"', show(rev, 'src/go2cs/go2cs-src.projitems'))
    c = collections.Counter(ent)
    gos = set(gof)
    # entries naming a subdirectory (internal\...) are other packages' files: outside this census
    return gof, c, [f for f in gof if c[f] != 1], [e for e in c if e.endswith('.go') and chr(92) not in e and '/' not in e and e not in gos]
for tag, rev in (('base', base), ('head', head)):
    gof, c, bad, orphan = projitems(rev)
    print('A PROJITEMS %s: go files=%d entries=%d files-without-exactly-one-entry=%d %s entries-without-a-file=%d %s' % (tag, len(gof), sum(c.values()), len(bad), bad[:8], len(orphan), orphan[:8]))

# B BEHAVIORAL
BEH = 'src/tests/Behavioral/'
LISTS = ['CompileTests.cs', 'OutputComparisonTests.cs', 'TargetComparisonTests.cs', 'TranspileTests.cs']
def projects(rev):
    t = lstree(rev, BEH); d = set()
    for p in t:
        parts = p[len(BEH):].split('/')
        if len(parts) == 2 and parts[1] == parts[0] + '.csproj': d.add(parts[0])
    return d, t
pb, tb = projects(base); ph, th = projects(head)
new = sorted(ph - pb); gone = sorted(pb - ph)
slnx = show(head, 'src/go2cs.slnx'); lists = {l: show(head, BEH + 'BehavioralTests/' + l) for l in LISTS}
print('B BEHAVIORAL projects base=%d head=%d added=%d removed=%d %s' % (len(pb), len(ph), len(new), len(gone), gone))
def top(n):
    pre = BEH + n + '/'
    return [p[len(pre):] for p in th if p.startswith(pre) and '/' not in p[len(pre):]]
def goonly(n):
    t = top(n)
    return any(p.endswith('.go') for p in t) and not any(p.endswith('.cs') and p != 'package_info.cs' for p in t)
for n in new:
    s = slnx.count('"tests/Behavioral/%s/%s.csproj"' % (n, n))
    lc = [len(re.findall(r'[(]"%s"[)]' % re.escape(n), lists[l])) for l in LISTS]
    cs = show(head, BEH + '%s/%s.csproj' % (n, n))
    h4 = goonly(n)
    tg = [p for p in top(n) if p.endswith('.cs.target')]
    tgok = bool(tg) and all((BEH + n + '/' + p[:-len('.target')]) in th for p in tg)
    verdict = 'ok' if s == 1 and lc == [1, 1, 1, 1] and not h4 and tgok else 'CHECK'
    print('  %s %s: slnx=%d lists=%s TrimMode-line=%s ReadyToRun-lines=%d golden(.cs + .cs.target)=%s%s' % (verdict, n, s, lc, 'yes' if '<TrimMode' in cs else 'NO (a pre-N template)', len(re.findall(r'<PublishReadyToRun', cs)), 'yes' if tgok else 'NO', ' H4(Go-only)' if h4 else ''))
# every directory head adds whose top level holds a .go file and no converted .cs (H4), whether or not it has a csproj
dirs_b = set(p[len(BEH):].split('/')[0] for p in tb); dirs_h = set(p[len(BEH):].split('/')[0] for p in th)
h4 = [d for d in sorted(dirs_h - dirs_b) if goonly(d)]
print('B H4 new behavioral directories with Go source and no converted .cs: %d %s; new directories=%d %s' % (len(h4), h4, len(dirs_h - dirs_b), sorted(dirs_h - dirs_b)))
subs = sorted(p for p in (set(re.findall(r'Path="(tests/Behavioral/[^"]+)"', slnx))) if ('src/' + p) not in th)
print('B SLNX behavioral entries at head with no file in the tree: %d %s' % (len(subs), subs[:6]))

# C GODECL
FUNC = re.compile(r'^func (?:[(]\s*\w*\s*[*]?\s*([A-Za-z_]\w*)(?:\[[^\]]*\])?\s*[)]\s*)?([A-Za-z_]\w*)\s*[\[(]')
TYPE = re.compile(r'^type ([A-Za-z_]\w*)\b')
def codelines(body):
    # tQ-battery.sh's gotests state machine (P13), line for line: yields (line, True when the line STARTS in code).
    # st 0 = code, 1 = inside a block comment, 2 = inside a raw string; both carry across lines. A line comment, an
    # interpreted string and a rune literal (escapes skipped) end on their own line.
    st = 0
    for line in body.replace(CR, '').split(NL):
        yield line, st == 0
        i = 0; n = len(line)
        while i < n:
            c = line[i]; d = line[i:i + 2]
            if st == 1:
                if d == '*/': st = 0; i += 2
                else: i += 1
                continue
            if st == 2:
                if c == BT: st = 0
                i += 1; continue
            if d == '//': break
            if d == '/*': st = 1; i += 2; continue
            if c == BT: st = 2; i += 1; continue
            if c == DQ or c == SQ:
                i += 1
                while i < n:
                    e = line[i]
                    if e == BS: i += 2; continue
                    i += 1
                    if e == c: break
                continue
            i += 1
def bodydecls(body, fname, out):
    for line, incode in codelines(body):
        if not incode: continue
        m = FUNC.match(line)
        if m and m.group(2) not in ('init', '_'): out[('method ' + m.group(1) + '.' if m.group(1) else 'func ') + m.group(2)].append(fname)
        m = TYPE.match(line)
        if m: out['type ' + m.group(1)].append(fname)
def decls(rev):
    t = lstree(rev, 'src/go2cs/'); out = collections.defaultdict(list)
    files = [p for p in t if p.count('/') == 2 and p.endswith('.go')]
    blob = subprocess.run(['git', '--git-dir=' + GD, 'cat-file', '--batch'], input=NL.join(t[p] for p in files).encode() + NL.encode(), capture_output=True).stdout
    pos = 0
    for p in files:
        e = blob.index(NL.encode(), pos); size = int(blob[pos:e].split()[2]); body = blob[e + 1:e + 1 + size].decode('utf-8', 'replace'); pos = e + 1 + size + 1
        if not re.search(r'(?m)^package main\b', body): continue
        bodydecls(body, p.split('/')[-1], out)
    return out
# The scan's own control (the battery's gotests_ctl, with declarations of both kinds): a backtick held in an interpreted
# string, a raw string, a rune literal, a block comment and a line comment. EXPECT exactly the two real declarations.
CTL = NL.join(['package main', 'var a = ' + DQ + 'x' + BT + 'y' + BS + DQ + DQ + ' // one backtick in an interpreted string',
    'func plantedReal1() {}', 'var b = ' + BT, 'func plantedInRaw() {', BT, 'var c = ' + SQ + BT + SQ, '/* a block comment',
    'func plantedInComment() {}', '*/ // a ' + BT + ' in a line comment', 'type plantedReal2 struct{}'])
ctl = collections.defaultdict(list); bodydecls(CTL, 'ctl.go', ctl)
ctlok = sorted(ctl) == ['func plantedReal1', 'type plantedReal2']
print('C GODECL scan control: %s (read %s; expected [func plantedReal1, type plantedReal2])' % ('ok' if ctlok else 'CONTROL FAILED: the census below is NOT a reading', sorted(ctl)))
db = decls(base); dh = decls(head)
dupb = {k for k, v in db.items() if len(v) > 1}; duph = {k: v for k, v in dh.items() if len(v) > 1}
newdup = {k: v for k, v in duph.items() if k not in dupb}
print('C GODECL package main: declarations base=%d head=%d; names declared more than once base=%d head=%d; NEW at head=%d' % (len(db), len(dh), len(dupb), len(duph), len(newdup)))
for k, v in sorted(newdup.items()): print('  DUPLICATE %s: %s' % (k, ' '.join(v)))

# D HASHSET (MS19's regex over the lines head adds)
d = git('diff', '-U0', base, head, '--', 'src/go2cs/*.go').decode('utf-8', 'replace').split(NL)
HS = re.compile(r'(^|[^.A-Za-z_])(HashSet\[|NewHashSet\b)'); cur = ''; hits = collections.Counter(); hl = []
for l in d:
    if l.startswith('+++ '): cur = l[6:]
    elif l.startswith('+') and not l.startswith('+++') and HS.search(l[1:]): hits[cur] += 1; hl.append(cur + ': ' + l[1:].strip()[:150])
print('D HASHSET unqualified uses in added src/go2cs lines: %d in %d file(s)' % (sum(hits.values()), len(hits)))
for x in hl[:10]: print('  ' + x)

# E MARKERS
ch = [l for l in git('diff', '--name-only', '--diff-filter=AM', base, head).decode('utf-8', 'replace').split(NL) if l]
th_all = lstree(head, '.') if False else None
mk = []
blobs = {}
for l in git('diff', '--raw', '--no-renames', '--diff-filter=AM', base, head).decode('utf-8', 'replace').split(NL):
    if not l: continue
    meta, p = l.split(TAB, 1); blobs[p] = meta.split()[3]
raw = subprocess.run(['git', '--git-dir=' + GD, 'cat-file', '--batch'], input=NL.join(blobs.values()).encode() + NL.encode(), capture_output=True).stdout
pos = 0; unread = 0
MK = re.compile(rb'(?m)^(<<<<<<<|>>>>>>>) ')
for p, sha in blobs.items():
    e = raw.index(NL.encode(), pos); f = raw[pos:e].split()
    if len(f) < 3: unread += 1; pos = e + 1; continue
    size = int(f[2]); body = raw[e + 1:e + 1 + size]; pos = e + 1 + size + 1
    if MK.search(body): mk.append(p)
print('E MARKERS files changed against base=%d read=%d unread=%d with a conflict marker=%d %s' % (len(blobs), len(blobs) - unread, unread, len(mk), mk[:8]))

# F PKGINFO
pk = [p for p in blobs if p.startswith('src/core/') and p.endswith('/package_info.cs')]
kinds = collections.Counter(); other = collections.defaultdict(list)
for p in pk:
    for l in git('diff', '-U0', base, head, '--', p).decode('utf-8', 'replace').split(NL):
        if (l.startswith('+') and not l.startswith('+++')) or (l.startswith('-') and not l.startswith('---')):
            b = l[1:].strip()
            k = 'GoPositionMap' if (b.startswith('[assembly: ') and 'go.GoPositionMap(' in b[:40]) else ('namespace' if b.startswith('namespace ') else ('using' if b.startswith(('using ', 'global using ')) else 'other'))
            kinds[k] += 1
            if k == 'other': other[p].append(l[:140])
meta = 'src/go2cs/stdlib-metadata.txt' in blobs
print('F PKGINFO src/core package_info.cs changed=%d; changed lines by kind: %s; stdlib-metadata.txt changed: %s' % (len(pk), dict(kinds), 'yes' if meta else 'no'))
for p, ls in sorted(other.items()):
    print('  other-kind lines in %s: %d' % (p, len(ls)))
    for x in ls[:6]: print('    ' + x)
