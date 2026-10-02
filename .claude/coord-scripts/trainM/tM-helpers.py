# tM-helpers.py -- TRAIN M battery helpers (COORD, i7). DRAFT 2026-10-02, derived from tL-helpers.py: coverage, canaries,
# trx, treport, csprojdrift, rosterrow, outparity, wallcmp, trxmethods, deadlock, cmpnames and lockcheck are L's bytes
# (treport gains one ENV line). CHANGED for M (tM-CHANGES.md): precheck is re-derived for M's seats (L's K8/K9/K6
# REGRESSION asserts described TRAIN K's resolutions and are removed; the M arms are the registry key union, the
# both-sides line check of every merge on the union (each path both of its sides changed), the roster counts, the Go-only behavioral
# directories and the Goroutine.cs identifiers of both sides); teattr reads M's emission class (G's go-creator frame and
# caller-skip window) and, with --baseline, the hunks TRAIN L's sweep did not rewrite; realmod reads a real module's
# -tests -recurse run by name. NOT RUN by its author: `python -m py_compile` only.
# Pure readers: git, the tree, go list, the artifacts a leg wrote and (live) the process list. Nothing here
# builds, converts, edits or runs a test. Every subcommand prints its findings and exits non-zero only when a HARD rule
# fails (the battery decides what a non-zero means for its leg).
#   precheck    <repo> <base> <seats-file> [head|worktree]
#                                                 registration COUNT asserts (projitems/slnx/4 BehavioralTests files, the
#                                                 fixup's own inserts counted), REG key union, BOTH-sides lines, ROSTER
#                                                 counts, H3 Goroutine.cs, H4 Go-only dirs, S1 census, optin check, markers
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
#   teattr      [--baseline <L patch>] <patch> [<patch> ...]
#                                                 TE: hunks of sweep-rewritten sources by class -- G's frame class (the
#                                                 NoInlining prefix, its using line, a position-map re-encode) vs OTHER;
#                                                 with --baseline, the OTHER hunks TRAIN L's same reading did not carry
#   hunkclass   <patch> [<patch> ...]             the fixup's golden step: per file, hunks by the same classes; exit 1 on OTHER
#   wallcmp     <M-summary> <L-summary> [...]     ROW legs' wall (S:/T:/NR:) M vs the fastest PASSING L reading; > 1.25x listed
#   realmod     --log L --root <outRoot>/src/<module> --module <module path> <pkg>=<spec> ...
#                                                 a real module's -tests -recurse run: per package Validated N, or the ONE
#                                                 named KNOWN non-pass, or FAIL (spec: N | N/T:Name:go:cs | build[:Prefix])
#   hostwall    <leg t0> <results.json> [<L's results.json>]
#                                                 the converted host's own package elapsed, fresh, beside TRAIN L's (a reading)
#   live        <own lock dir> [<seconds>]        floor 1 across trains: other trains' locks, converter/harness processes
#                                                 (listed, never killed), run logs still being written; exit 1 on any
#   verify round 2 (ruling R1: the drivers derive their by-name lists at run time; these three read the roster for them):
#   execrows    <repo> [<ref>]                    the roster rows carrying an execution annotation at <ref> (default HEAD)
#   execruled                                     the RULED set (EXEC_RULED below, the ONE site of that ruling) and its count
#   rosterlinux <repo> <pkg>                      a row's LINUX expectation: its 'linux: N + D' annotation, else its banked N + D
#   TRAIN L's DRAFT 2 additions (tL-CHANGES.md):
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

# The registration KEY of each COUNT file: one key per registered thing, unique in the file (measured at aa0a07d5fd and
# at the pre-map union 8d7305053f: 0 duplicates in all six).
REG_KEY = {'src/go2cs/go2cs-src.projitems': r'Include="([^"]+)"', 'src/go2cs.slnx': r'<Project Path="([^"]+)"'}
REG_KEY_TESTS = r'public void ([A-Za-z0-9_]+)\(\)'
MIDDOT = '·'
EXEC = re.compile(MIDDOT + r'\s*execution\s*:\s*([a-z][a-z0-9-]*)\s*(?=' + MIDDOT + r'|\|)')
WORDNUM = {'one': 1, 'two': 2, 'three': 3, 'four': 4, 'five': 5}
EXEC_RULED = [('log/slog', 'release-tiered')]   # the roster rows that carry an execution config (tL-seats-draft.txt:41, :63)
# Verify round 2: EXEC_RULED is the ONE site of that ruling. tM-fixup.sh and tM-battery.sh take their EXEC_ROWS_EXPECT
# default from `execruled`, the battery derives its TC0 and tiered rows from `execrows` at the base and at HEAD, and the
# lane drivers read the roster of the union they run on; nothing else names the rows.
# Verify round 3 (RULED): `execrows` (ROW + EXEC above) is the ONE READER of a row's execution config, for the battery
# AND for both lane drivers (tM-linux-legs.sh and tM-i9-shard.sh call it; each stamps the list it derived). EXEC matches
# the ANNOTATION field only (a middle dot before, a middle dot or '|' after): two rows still carry the phrase
# 'execution: release-tiered' in their PROSE, which an unanchored grep read as the annotation.
LINUX_ANN = re.compile(MIDDOT + r'\s*linux:\s*(\d+)(?:\s*\+\s*(\d+))?')
# H3: an identifier each seat that edits golib's Goroutine.cs is known by. A seat that edits the file and has no entry
# here FAILS the arm by name (R1: a literal that is short for the union is a finding, never silence).
H3_TOKENS = {'g-godebug-pc-line': ['systemBasis']}
FIXUP_SUBJ = re.compile(r'^fixup(-\d+)?: TRAIN M')

def precheck(repo, base, seats_file, mode='head'):
    # mode 'head'      (the battery): every arm reads HEAD. When HEAD is the train's fixup commit, the fixup's OWN net
    #                  inserts into a COUNT file are a contributor (TRAIN L's lesson: its fixup's six D4 lines were
    #                  nobody's, PRE-1 read expect + 6, and the battery was relaunched with PRECHECK_MODE=warn).
    # mode 'worktree'  (the fixup, before its commit): the same arms read the WORKTREE's files and the fixup's
    #                  contribution is the worktree's delta against HEAD, so this run PREDICTS the battery's PRE-1 at the
    #                  commit the fixup is about to make (at L the COUNT arm read HEAD while the edit sat in the worktree).
    from collections import Counter
    hard, soft, note = [], [], []
    seats = seat_shas(seats_file)
    # Verify round 1: the drivers count a seat row as '^ref|' (tM-assemble.sh, tM-fixup.sh, tM-battery.sh); seat_shas also
    # wants a lower-case sha and a notes column. A row one pattern takes and the other drops would be merged and
    # ancestry-checked by the drivers and silently missing from every arm below.
    nshell = sum(1 for l in open(seats_file, encoding='utf-8') if re.match(r'^[A-Za-z0-9._-]+\|', l))
    if nshell != len(seats):
        hard.append(f'FAIL SEATS {nshell} rows match the drivers\' row pattern, {len(seats)} parse as ref|sha|notes (lower-case sha, a notes column)')
    wt = mode == 'worktree'
    def cur(path):   # the file as this mode reads it, CR-stripped; None when absent
        if wt:
            try:
                with open(f'{repo}/{path}', 'rb') as fh: return fh.read().decode('utf-8', 'surrogateescape').replace('\r', '')
            except OSError: return None
        r = subprocess.run(['git', '-C', repo, 'show', f'HEAD:{path}'], capture_output=True)
        return r.stdout.decode('utf-8', 'surrogateescape').replace('\r', '') if r.returncode == 0 else None
    def at(ref, path):   # a committed blob, CR-stripped; '' when the path does not exist at that ref
        r = subprocess.run(['git', '-C', repo, 'show', f'{ref}:{path}'], capture_output=True)
        return r.stdout.decode('utf-8', 'surrogateescape').replace('\r', '') if r.returncode == 0 else ''
    def net(*a):         # net inserted lines of one path from a numstat
        ns = git(repo, 'diff', '--numstat', *a).split()
        return int(ns[0]) - int(ns[1]) if ns and ns[0].isdigit() else 0
    # Verify round 2 (floor 9, COORD's ruling): the head of the union is 'fixup: TRAIN M' followed by ZERO OR MORE
    # 'fixup-N: TRAIN M' single-parent commits, never a replaced sha. The fixups' contribution is the sum of the net
    # inserts of every single-parent commit on the first-parent line that carries a fixup subject, wherever it sits (at
    # the landing HEAD is the merge of master and the fixups are below it: round 1's test read HEAD's subject alone and
    # then counted nothing). The battery's PRE is the gate of the SHAPE; here a single-parent commit with another
    # subject contributes nothing, so a COUNT file it touched fails.
    fxc = []
    for c in git(repo, 'rev-list', '--first-parent', f'{base}..HEAD').split():
        if len(git(repo, 'rev-list', '--parents', '-n', '1', c).split()) == 2 and FIXUP_SUBJ.match(git(repo, 'log', '-1', '--format=%s', c)):
            fxc.append(c)
    def fixup_delta(f):
        return sum(net(c + '^', c, '--', f) for c in fxc) + (net('HEAD', '--', f) if wt else 0)   # the committed fixup(s) + the worktree's own edit
    # Each seat's OWN base, in ROW ORDER (the merge order): the best common ancestor of the seat with the train's base
    # and every EARLIER row. A seat stacked on an earlier seat then contributes only its own commits, and two seats
    # stacked on ONE earlier seat do not each re-count that seat's inserts (the hazard L's ancestor rule left open:
    # it dropped the shared base seat and kept its inserts inside every seat stacked on it).
    own = []
    for i, (name, sha) in enumerate(seats):
        mb = git(repo, 'merge-base', sha, base, *[s for _, s in seats[:i]]).strip()
        own.append((name, sha, mb))
    # COUNT: each registration file == base + the sum of each seat's OWN net inserts (+ the fixup's own).
    for f in COUNT_FILES:
        b = blob(repo, base, f).count('\n'); c = cur(f); h = -1 if c is None else c.count('\n')
        touch = [(name, net(mb, sha, '--', f)) for name, sha, mb in own]
        touch = [t for t in touch if t[1] != 0]
        fx = fixup_delta(f)
        exp = b + sum(t[1] for t in touch) + fx
        line = (f'COUNT {f}: base={b} head={h} expect={exp} :: ' + ' '.join(f'{t[0]}:{t[1]:+d}' for t in touch)
                + (f' fixup:{fx:+d}' if fx else ''))
        (hard if h != exp else soft).append(('FAIL ' if h != exp else 'ok   ') + line)
    # REG: an append-only registry holds BOTH sides' keys. Every key of the base file and every key a seat's own
    # commits add is in the merged file, and no key is registered twice.
    for f in COUNT_FILES:
        rx = re.compile(REG_KEY.get(f, REG_KEY_TESTS)); c = cur(f) or ''
        keys = Counter(rx.findall(c))
        dups = sorted(k for k, n in keys.items() if n > 1)
        lost = sorted(set(rx.findall(at(base, f))) - set(keys))
        miss = []
        for name, sha, mb in own:
            if net(mb, sha, '--', f) == 0 and not git(repo, 'diff', '--name-only', mb, sha, '--', f).strip(): continue
            for k in sorted(set(rx.findall(at(sha, f))) - set(rx.findall(at(mb, f)))):
                if k not in keys: miss.append(f'{name}:{k}')
        bad = dups or lost or miss
        (hard if bad else soft).append(
            f'{"FAIL" if bad else "ok  "} REG {f}: keys={sum(keys.values())} duplicate={dups[:6]} base-keys-lost={lost[:6]} seat-keys-missing={miss[:6]}')
    # REMOVED FROM L (premises of TRAIN K's assembly, two trains old): K9 (ThreadStateCensusTests rows == base, and five
    # runtime GoPositionMap lines pinned to K seat shas ed7859d20e / b9c8948630) and K6 (os/{linux,darwin}/file_unix.cs
    # StackTraceHidden >= 1). No M seat touches ThreadStateCensusTests.cs or os/*/file_unix.cs, and G's seat re-encodes
    # runtime/*/package_info.cs map lines for OTHER sources, so a pin on those five lines says nothing about M. K8's two
    # gated-Register strings survive below as one member of H3, compared with the base's own count, not with a literal.
    #
    # BOTH (seat-footprints 1d/2c), PER MERGE since verify round 1: for every two-parent commit c on the union's
    # first-parent line, every path that BOTH of its sides changed since their merge-base (the union so far, c^1, and
    # the merged seat or follow-up, c^2) is read in c's OWN tree. For each substantial line either side ADDED, c holds
    # it (count >= merge-base count + the deltas) and holds it no more often than both sides added it; for each line
    # either side REMOVED, c does not hold it again. That is the 3-way arithmetic of ONE merge, so it covers seat vs
    # master (the roster: G's two row changes AND master's TRAIN L provenance block; src/_roster.ps1; Goroutine.cs;
    # projitems) AND seat vs the seats merged before it (the registration files of the stacked P2 rows), and a ruled
    # follow-up that edits a line its seat added is read at ITS merge, where the edit is one side's change, instead
    # of reading as the seat's line 'lost' from the final tree (the round-0 arm compared every seat with HEAD).
    # Read commits-only at the pre-map union 8d7305053f (notes/_r1-both-permerge.py): 15 merges, 32 pairs, 0 lost,
    # 0 back, 0 duplicated. A listed row with no merge of its own on the first-parent line is a FAIL (at the bare base:
    # 15 of 15). The fixup's own commit has one parent and is not read here; neither is the worktree.
    full_of = {git(repo, 'rev-parse', sha + '^{commit}').strip(): name for name, sha in seats}
    cnt = lambda t: Counter(l for l in t.split('\n') if len(l.strip()) >= 12)
    names = lambda a, b: set(x for x in git(repo, '-c', 'core.quotepath=false', 'diff', '--name-only', '-z', a, b).split('\0') if x)
    nboth, nmerge, merged_p2 = 0, 0, set()
    for c in git(repo, 'rev-list', '--first-parent', f'{base}..HEAD').split():
        par = git(repo, 'rev-list', '--parents', '-n', '1', c).split()[1:]
        if len(par) != 2: continue
        p1, p2 = par; nmerge += 1; merged_p2.add(p2)
        name = full_of.get(p2, 'unlisted-' + p2[:10])
        mb = git(repo, 'merge-base', p1, p2).strip()
        for path in sorted(names(mb, p2) & names(mb, p1)):
            nboth += 1
            ts, tb, th = at(p2, path), at(p1, path), at(c, path)
            if not th and (ts or tb): hard.append(f'FAIL BOTH {name} {path}: absent from the merge {c[:10]} while a side still holds it'); continue
            cm, cs_, cb, ch = cnt(at(mb, path)), cnt(ts), cnt(tb), cnt(th)
            lostl, backl, dupl, adds, rems = [], [], [], 0, 0
            for l in set(cs_) | set(cb) | set(cm):
                ds, db = cs_[l] - cm[l], cb[l] - cm[l]
                if ds > 0 or db > 0:
                    adds += 1
                    need = cm[l] + (max(ds, db) if ds > 0 and db > 0 else ds + db)
                    if ch[l] < need: lostl.append(l)
                    if ch[l] > cm[l] + max(ds, 0) + max(db, 0): dupl.append(l)
                elif ds < 0 or db < 0:
                    rems += 1
                    if ch[l] > cm[l] + min(ds, db): backl.append(l)
            badb = lostl or backl or dupl
            (hard if badb else soft).append(
                f'{"FAIL" if badb else "ok  "} BOTH {name} {path} (merge {c[:10]}): lines added by either side={adds} removed={rems} lost={len(lostl)} back={len(backl)} duplicated={len(dupl)}'
                + (f' :: first lost: {lostl[0].strip()[:110]}' if lostl else '') + (f' :: first back: {backl[0].strip()[:110]}' if backl else '')
                + (f' :: first duplicated: {dupl[0].strip()[:110]}' if dupl else ''))
    for sha_, name in full_of.items():
        if sha_ not in merged_p2: hard.append(f'FAIL BOTH {name}: no first-parent merge of {sha_[:10]} between {base} and HEAD (the row is not merged as itself)')
    soft.append(f'ok   BOTH: {nmerge} first-parent merge(s), {nboth} (merge, path) pair(s) where both sides changed the path since their merge-base')
    # ROSTER: no M seat is ruled to move a count ("No count in the table or the header changes", aa0a07d5fd; G's two
    # roster commits change two rows' execution annotation only). The execution-annotation rows are asserted BY IDENTITY
    # below; check-roster-format.ps1 (fixup step PRERES, battery legs G1/G2) is the instrument of record for their count.
    rp = 'docs/ValidatedTestPackages.md'; rc_ = cur(rp) or ''
    rowsof = lambda t: {m.group(1): (int(m.group(2)), int(m.group(3) or 0)) for m in ROW.finditer(t)}
    rb, rh = rowsof(at(base, rp)), rowsof(rc_)
    moved = sorted(k for k in set(rb) | set(rh) if rb.get(k) != rh.get(k))
    (hard if moved or not rh else soft).append(
        f'{"FAIL" if moved or not rh else "ok  "} ROSTER rows base={len(rb)} merged={len(rh)} rows whose N + D moved or that came or went: '
        + (' '.join(f'{k}:{rb.get(k)}->{rh.get(k)}' for k in moved[:6]) or 'none'))
    # Verify round 2 (M-requirements section 3, four bank/roster items that had no leg). (1) "No count changes" covers the
    # LINUX annotations too (runtime 10810 + 73, runtime/pprof 147 + 7, ...): rowsof() compares columns 2 and 3 only.
    lin = lambda t: {m.group(1): x.group(0) for line in t.split('\n') if (m := ROW.match(line)) and (x := LINUX_ANN.search(line))}
    lb, lh = lin(at(base, rp)), lin(rc_)
    lmoved = sorted(k for k in set(lb) | set(lh) if lb.get(k) != lh.get(k))
    (hard if lmoved else soft).append(
        f'{"FAIL" if lmoved else "ok  "} ROSTER linux annotations base={len(lb)} merged={len(lh)} moved, came or went: '
        + (' '.join(f'{k}:[{lb.get(k)}]->[{lh.get(k)}]' for k in lmoved[:6]) or 'none'))
    # (2) "MUST NOT touch the frozen snapshot": every docs/validation/<release> folder of the base (all but current/).
    snap = [x for x in git(repo, 'diff', '--name-only', base, 'HEAD', '--', 'docs/validation').split('\n') if re.match(r'^docs/validation/\d', x)]
    (hard if snap else soft).append(f'{"FAIL" if snap else "ok  "} SNAPSHOT paths under a frozen docs/validation/<release>/ changed since {base}: {len(snap)} ' + ' '.join(snap[:5]))
    # (3) "Keep the regenerated proof pages": every page under docs/validation/current that a seat's OWN commits changed
    # (derived, row by row; at the 24-row list that is G's net.http.md and internal.godebug.md) is, at HEAD, the blob of
    # the LAST row that changed it. A page HEAD holds differently was edited by a merge, a follow-up or the fixup: FAIL,
    # to be read (after a bank step that regenerates a page this arm fails by design: tM-README.md M11 says so).
    page_owner = {}
    for name, sha, mb in own:
        for x in git(repo, 'diff', '--name-only', mb, sha, '--', 'docs/validation/current').split('\n'):
            if x: page_owner[x] = (name, sha)
    pbad = [f'{x} (last changed by {n_})' for x, (n_, s_) in sorted(page_owner.items())
            if git(repo, 'rev-parse', '-q', '--verify', f'HEAD:{x}', check=False).strip() != git(repo, 'rev-parse', '-q', '--verify', f'{s_}:{x}', check=False).strip()]
    (hard if pbad else soft).append(f'{"FAIL" if pbad else "ok  "} PROOF pages the seats regenerated ({len(page_owner)}: {" ".join(sorted(page_owner))[:200] or "none"}) that HEAD holds as another blob: '
                                    + (' '.join(pbad[:4]) or 'none'))
    execrows = [(m.group(1), e.group(1)) for line in rc_.split('\n') if (m := ROW.match(line)) for e in EXEC.finditer(line)]
    # Verify round 1: WHICH row keeps the annotation is asserted here, seconds in (the roster guard gates the COUNT; the
    # row's identity was first tested hours into the battery, at S:log/slog). EXEC_RULED is the ruling of
    # tL-seats-draft.txt:41 and :63; it moves together with EXEC_ROWS_EXPECT in tM-fixup.sh and tM-battery.sh.
    exl = ' '.join(f'{p}[{v}]' for p, v in execrows) or 'none'
    (hard if execrows != EXEC_RULED else soft).append(
        ('FAIL ROSTER execution annotations: ' + exl + ' (ruled: ' + ' '.join(f'{p}[{v}]' for p, v in EXEC_RULED) + ' alone)') if execrows != EXEC_RULED
        else 'ok   ROSTER execution annotations: ' + exl + ' (the ruled set; the roster guard is the count of record)')
    # H5 (a NOTE, never a failure: prose, G's to word, no guard counts it): the sentence that says how many rows opt
    # back out of the default execution config, against the number of rows that carry the annotation.
    m5 = re.search(r'\b([A-Za-z]+|\d+) rows? opts? back out', rc_)
    if m5:
        said = WORDNUM.get(m5.group(1).lower(), int(m5.group(1)) if m5.group(1).isdigit() else -1)
        if said != len(execrows):
            note.append(f'NOTE H5 {rp}: the prose says "{m5.group(0)}" while {len(execrows)} row(s) carry an execution annotation (docs fixup owed: tM-README.md manual step M3)')
    # H3 (seat-footprints): Goroutine.cs is edited by TRAIN L's checkdead (master) and by G's goroutine entry
    # classification (g-godebug-pc-line), in different regions, and G changes the fact checkdead reads. The check a tree
    # can answer is that BOTH sides' identifiers are in the merged file at each side's own count; whether checkdead is
    # still right under G's classification is the battery's (GolibTests by name, the three behavioral guards, net/rpc x5,
    # runtime's TestTracebackSystem/panic).
    gp = 'src/core/golib/runtime/Goroutine.cs'; g = cur(gp) or ''
    h3 = []
    for tok in ('AllGoroutinesForeverBlocked', 'IsProvableDeadlock', 'm_profileLabels = ', 's_live[goroutine.Id] = goroutine;'):
        wb = at(base, gp).count(tok)
        h3.append((tok, g.count(tok), wb, 'master'))
    # Verify round 2 (R1): WHICH seats edit Goroutine.cs is derived from each row's own commits (at the 24-row list:
    # g-godebug-pc-line alone); the identifier each is known by is the literal H3_TOKENS, counted at the LAST row that
    # edits the file (a stacked row holds the earlier one's text). A row that edits the file with no entry there FAILS.
    h3seats = [(name, sha) for name, sha, mb in own if git(repo, 'diff', '--name-only', mb, sha, '--', gp).strip()]
    for name, sha in h3seats:
        if name.endswith('-followup'): continue   # a ruled follow-up of a listed seat: read by the BOTH arm at its own merge
        if name not in H3_TOKENS:
            hard.append(f'FAIL H3 seat {name} edits {gp} and tM-helpers.py H3_TOKENS holds no identifier for it: add one (the arm does not read that seat\'s side)')
            continue
        for tok in H3_TOKENS[name]:
            h3.append((tok, g.count(tok), at(h3seats[-1][1], gp).count(tok), name))
    for name in H3_TOKENS:
        if name not in [n_ for n_, _ in h3seats]:
            note.append(f'NOTE H3: {name} (H3_TOKENS) is not a row that edits {gp} in this list, so its side was not checked')
    h3.append(('<<<<<<<', g.count('<<<<<<<'), 0, 'markers'))
    bad3 = [t for t in h3 if t[1] != t[2] or (t[3] != 'markers' and t[2] == 0)]
    (hard if bad3 else soft).append(f'{"FAIL" if bad3 else "ok  "} H3 {gp}: ' + ' '.join(f'[{t[0]}]={t[1]} (want {t[2]}, {t[3]})' for t in h3))
    # H4 (seat-footprints): a behavioral directory that holds Go sources and no tracked C# is transpiled by
    # check-no-regression.ps1 (it enumerates every directory with a *.go file), whose `git status` then lists the
    # untracked emission as CHANGED. 0 such directories at aa0a07d5fd; p1-m-repros adds TestNeedsTransitiveApiRef/a and /b
    # and no M seat goldens them. HARD. At the FROZEN list (verify round 3) the arm reads 0: row p2-test-overload-references
    # (e002a552a8) deletes that repro module in-seat, so no follow-up merge is owed (tM-README.md M2; 0 at 11c188daf3).
    gdir, cdir = set(), set()
    for x in git(repo, '-c', 'core.quotepath=false', 'ls-tree', '-r', '--name-only', '-z', 'HEAD', '--', 'src/tests/Behavioral').split('\0'):
        d = x.rsplit('/', 1)[0]
        if x.endswith('.go'): gdir.add(d)
        elif x.endswith('.cs'): cdir.add(d)
    goonly = sorted(gdir - cdir)
    (hard if goonly else soft).append(f'{"FAIL" if goonly else "ok  "} H4 Go-only behavioral directories (tracked *.go, no tracked *.cs): {len(goonly)} {" ".join(goonly[:8])}')
    # Conflict markers anywhere in the TREE (verify round 1: the round-0 scan read src/ only, and the one file the seat
    # list tells COORD to resolve by hunk, docs/ValidatedTestPackages.md, is under docs/, as are the proof pages, the
    # FINDING record and every hand-made follow-up merge's docs). 0 hits over the whole tree at the pre-map union.
    mk = git(repo, 'grep', '-nE', '^(<<<<<<<|>>>>>>>)( |$)', 'HEAD', check=False).strip()
    (hard if mk else soft).append(('FAIL markers: ' + mk.replace('\n', ' | ')[:600]) if mk else 'ok   no conflict markers in the tree')
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
    for l in note: print(l)
    for l in hard: print(l)
    print(f'PRECHECK hard-failures={len(hard)} notes={len(note)} mode={mode}')
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
        # M: the execution config the record itself states (testEnvironmentRecord). G's roster commits move net/http
        # and internal/godebug to the default (Release, tiering off) and leave log/slog release-tiered; the sweep reads
        # the config from the roster, so the row's record is where a wrong config would show.
        ev = c.get('environment') or {}
        print(f'{label} ENV configuration={ev.get("configuration")} tiered={ev.get("tiered")}')
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
    # read from the TREE so the PRE section names the gap before leg C does. History: at TRAIN L's assembled union
    # 6960c8071f it read 718 attributes vs 716 listed and L's fixup added the two rows. M: the same rule emulated with
    # git grep over the pre-map union 8d7305053f reads 724 == 724, no gap; this is the reading at the real union.
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
# TRAIN M's emission class on sweep-rewritten sources. L's D6 and cross-package signatures left with their seats (and
# with them the two raw-codepoint character classes tL-helpers.py carried at its lines 351-352). What M's seats change:
#   G (g-godebug-pc-line): a function or literal whose own body executes a `go`, and a constant-skip Caller/Callers
#     window frame, gains '[MethodImpl(MethodImplOptions.NoInlining)] ' in front of its declaration; the file gains
#     'using System.Runtime.CompilerServices;'; the package's GoPositionMap line is re-encoded. Those are the line kinds
#     G's own golden re-baseline read (0d04adbb36: "55 functions gaining NoInlining, 35 using lines, 35 package_info
#     map re-encodes, no other line kind"). G regenerated NO committed *_test.cs, so a sweep's rewrites are EXPECTED to
#     carry this class: it is counted and named, and it is not a finding.
#   P2's F-batch, c1-darwin-linkname-pulls and the i9's seat claim a 0 footprint on the corpus a windows sweep rewrites,
#     and no signature is INFERRED for them here. They are read through --baseline: the OTHER hunks of this patch that
#     TRAIN L's same reading (same box, same rows, one train earlier) did not carry. That list is something to READ.
# A hunk is G-FRAME when every changed line in it is a NoInlining prefix insertion or the using line, MAP when it is
# only a position-map re-encode, and OTHER otherwise. NOT RUN by its author; controls/ holds one patch of each kind.
NOINL = '[MethodImpl(MethodImplOptions.NoInlining)] '
USING_RT = 'using System.Runtime.CompilerServices;'
MAPLINE = re.compile(r'^\[assembly: (go\.)?GoPositionMap\(')

def te_hunks(pf):
    # -> [(file, removed lines, added lines)], one entry per hunk of a unified diff (context lines are ignored)
    out, cur, rem, add, inh = [], None, [], [], False
    def flush():
        nonlocal rem, add
        if cur is not None and (rem or add): out.append((cur, tuple(rem), tuple(add)))
        rem, add = [], []
    for line in open(pf, encoding='utf-8', errors='replace'):
        line = line.rstrip('\r\n')
        if line.startswith('diff --git '):
            flush(); cur = line.split(' b/', 1)[-1].strip(); inh = False
        elif line.startswith('@@'):
            flush(); inh = True
        elif inh and line.startswith('-'): rem.append(line[1:])
        elif inh and line.startswith('+'): add.append(line[1:])
    flush()
    return out

def te_class(rem, add):
    # -> (class, noinline lines, using lines, map lines)
    r, a = list(rem), list(add)
    n = u = m = 0
    for x in list(a):
        if x.strip() == USING_RT: a.remove(x); u += 1
    rm, am = [x for x in r if MAPLINE.match(x)], [x for x in a if MAPLINE.match(x)]
    if rm and len(rm) == len(am):
        for x in rm: r.remove(x)
        for x in am: a.remove(x)
        m = len(am)
    for x in list(a):
        if NOINL in x:
            y = x.replace(NOINL, '', 1)
            if y in r: r.remove(y); a.remove(x); n += 1
    if r or a: return 'OTHER', n, u, m
    return ('G-FRAME' if n or u else 'MAP'), n, u, m

def teattr(argv):
    baseline, patches, i = None, [], 0
    while i < len(argv):
        if argv[i] == '--baseline': baseline = argv[i + 1]; i += 2
        else: patches.append(argv[i]); i += 1
    patches = [p for p in patches if os.path.exists(p)]
    if not patches:
        print('TE no readable patch'); return 2
    hunks = [h for pf in patches for h in te_hunks(pf)]
    files = {h[0] for h in hunks}
    cnt = {'G-FRAME': 0, 'MAP': 0, 'OTHER': 0}; nl = ul = 0; other, gfiles, ofiles = [], set(), set()
    for h in hunks:
        c, n, u, m = te_class(h[1], h[2])
        cnt[c] += 1; nl += n; ul += u
        if c == 'G-FRAME': gfiles.add(h[0])
        if c == 'OTHER': other.append(h); ofiles.add(h[0])
    # Verify round 2 (tM-README.md M13, a RULED deferral): the files whose EVERY hunk is G's frame class or a map
    # re-encode are the committed test sources that are stale only by that class; the count is the number the queued
    # corpus-wide refresh of committed -tests sources starts from.
    gonly = gfiles - ofiles
    print(f'TE files={len(files)} hunks={len(hunks)} g-frame={cnt["G-FRAME"]} (noinline-lines={nl} using-lines={ul}, in {len(gfiles)} file(s); '
          f'g-frame-only files={len(gonly)}) map-only={cnt["MAP"]} other={cnt["OTHER"]}')
    new = None
    if baseline and os.path.exists(baseline):
        bh = te_hunks(baseline); bset = set(bh); bfiles = {h[0] for h in bh}
        new = [h for h in other if h not in bset]
        nf = {h[0] for h in new}
        print(f'TE BASELINE {os.path.basename(baseline)}: files={len(bfiles)} hunks={len(bh)} :: OTHER hunks this patch carries and the baseline '
              f'does not: {len(new)} in {len(nf)} file(s) ({len(nf - bfiles)} of those files the baseline never touched)')
        for h in new[:25]:
            print(f'TE NEW {h[0]}: -[{(h[1][0].strip() if h[1] else "")[:100]}] +[{(h[2][0].strip() if h[2] else "")[:100]}] ({len(h[1])} removed, {len(h[2])} added)')
        if len(new) > 25: print(f'TE NEW ... and {len(new) - 25} more')
    elif baseline:
        print(f'TE BASELINE absent ({baseline}): the OTHER hunks were not compared with TRAIN L')
    print('TE VERDICT reading, not gated: G-FRAME is M\'s expected class (G regenerated no committed test source); '
          + ('no baseline was given, so OTHER is a count only' if new is None else
             f'{len(new)} OTHER hunk(s) are new against TRAIN L: READ EACH (a candidate seat footprint or a G line beside standing drift)'))
    return 0

# ---------------------------------------------------------------------------------------------------------------- hunkclass
def hunkclass(patches):
    # The fixup's golden step: every hunk of the behavioral files the union's CNR regenerated must be G's frame class or
    # a position-map re-encode (train-assembly skill: "classify by the diff's CONTENT, the seat's own intended line,
    # nothing else"). One line per file, then the verdict. Exit 1 when any hunk is OTHER, 0 otherwise (0 hunks included).
    hunks = [h for pf in patches if os.path.exists(pf) for h in te_hunks(pf)]
    per = {}
    for h in hunks:
        c, n, u, m = te_class(h[1], h[2])
        d = per.setdefault(h[0], {'G-FRAME': 0, 'MAP': 0, 'OTHER': 0, 'n': 0, 'u': 0})
        d[c] += 1; d['n'] += n; d['u'] += u
    other = sorted(f for f, d in per.items() if d['OTHER'])
    for f, d in sorted(per.items()):
        print(f'HUNKCLASS {f}: noinline-lines={d["n"]} using-lines={d["u"]} map-only-hunks={d["MAP"]} other-hunks={d["OTHER"]}')
    print(f'HUNKCLASS-VERDICT files={len(per)} hunks={len(hunks)} other-files={len(other)} '
          + ("every hunk is G's frame class or a position-map re-encode" if not other else 'OTHER line kinds in: ' + ' '.join(other)[:500]))
    return 1 if other else 0

# ---------------------------------------------------------------------------------------------------------------- wallcmp
# TRAIN L's lesson (tL-seats-draft.txt:40): this compares each ROW leg's WALL, which is mostly MSBuild and publish, not
# the converted host's package elapsed; at L both flags it raised were noise. It stays a prompt to read, never a gate.
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
        if r > 1.25: slow.append(f'{n} M={w}s L(min)={kb}s x{r:.2f} rc={rc}')   # M: this run vs TRAIN L's summaries (the names L and K above are L's code)
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

# ================================================================================================ TRAIN M additions
# ---------------------------------------------------------------------------------------------------------------- realmod
def realmod(argv):
    # A real third-party module through `go2cs -tests -recurse <moduleDir> <outRoot>` (M3's driver): ONE verdict per
    # package, read from the driver's log (the '-tests <importPath> -> <dir>' header, then that package's 'Validated N
    # tests against go test' line) and from the package's own comparison record under <outRoot>/src/<importPath>.
    #   <pkg>=N               the package validates exactly N; anything else is FAIL
    #   <pkg>=N/T:Name:go:cs  a KNOWN non-pass: the ONLY undisclosed names whose verdicts differ are Name or Name/<sub>,
    #                         and Name itself reads go=<go> cs=<cs> ('-' = no verdict on that side, '*' = any). Reads
    #                         KNOWN, never a red. N of T (matching names of all Go-side names in the record) is P2's
    #                         linux reading: a different count is stated as COUNT-NOTE, not failed, because HOW that
    #                         reading counted (record keys, subtests included) is INFERRED. The same package
    #                         validating T reads CLEARED (the host finding was fixed), which is a pass.
    #   <pkg>=build[:Prefix]  the package's host built and ran (a record with verdicts on both sides) AND every
    #                         undisclosed differing name starts with Prefix (none may differ without one); counts REPORTED
    # '.' is the module's root package. A package the log names and no spec lists is FAIL (anything unlisted is a red).
    # Exit 1 when any package reads FAIL.
    a, specs, i = {}, [], 0
    while i < len(argv):
        if argv[i].startswith('--'): a[argv[i]] = argv[i + 1]; i += 2
        else: specs.append(argv[i]); i += 1
    log, root, module = a['--log'], a['--root'], a['--module']
    val, seen, pkg, tout = {}, [], None, 0
    if os.path.exists(log):
        for line in open(log, encoding='utf-8', errors='replace'):
            line = line.rstrip('\r\n')
            m = re.match(r'^-tests (\S+) -> ', line)
            if m: pkg = m.group(1); seen.append(pkg); continue
            m = re.match(r'^Validated (\d+) tests against go test', line)
            if m and pkg: val[pkg] = int(m.group(1))
            if 'dotnet timed out' in line: tout += 1
    npass = nknown = nfail = 0
    listed = set()
    for spec in specs:
        name, _, want = spec.partition('=')
        ip = module if name == '.' else f'{module}/{name}'
        listed.add(ip)
        rec = os.path.join(root, *([] if name == '.' else name.split('/')), 'go2cs_test_comparison.json')
        g, s, disc, status = {}, {}, set(), 'no-record'
        if os.path.exists(rec):
            try:
                c = json.load(open(rec, encoding='utf-8-sig'))
                g, s, status = c.get('go') or {}, c.get('csharp') or {}, c.get('status')
                disc = {x.split(' (')[0] for x in c.get('disclosed') or []}
            except Exception as e:
                status = f'unreadable-record ({type(e).__name__})'
        mism = sorted(k for k in set(g) | set(s) if g.get(k) != s.get(k) and k not in disc)
        match = sum(1 for k in g if g.get(k) == s.get(k))
        v = val.get(ip)
        facts = f'validated={v} record={status} go={len(g)} cs={len(s)} matching={match} differing={len(mism)}'
        if ip not in seen:
            verdict, why = 'FAIL', f'the log names no "-tests {ip}" header (the package was not reached) :: {facts}'
        elif want.split(':')[0] == 'build':
            # Verify round 1: 'build' alone swallowed ANY new failure in the package (every differing name was text).
            # Now a differing name outside the allowed prefix is a FAIL; plain 'build' allows none.
            allow = want.partition(':')[2]
            ok = bool(g) and bool(s)
            stray = [k for k in mism if not (allow and k.startswith(allow))]
            verdict = 'PASS' if ok and not stray else 'FAIL'
            why = ((('built and ran; differing names outside the allowed prefix: ' + (' '.join(stray[:12]) or 'none') + ' :: ') if ok
                    else 'no verdicts on both sides: the host did not build or run :: ') + facts
                   + (f' allowed-differing({allow}): {sum(1 for k in mism if k not in stray)}' if allow else ''))
        elif '/' not in want:
            ok = v == int(want)
            verdict, why = ('PASS' if ok else 'FAIL'), f'expect Validated {want} :: {facts}' + (f' differing: {" ".join(mism[:8])}' if mism else '')
        else:
            counts, kname, kgo, kcs = (want.split(':') + ['', '', ''])[:4]
            kn, kt = (int(x) for x in counts.split('/'))
            sideok = lambda got, w: w == '*' or (got or '-') == w
            if v is not None:
                ok = v == kt
                verdict, why = ('PASS' if ok else 'FAIL'), (f'CLEARED: validates {v}, the KNOWN non-pass {kname} did not occur :: ' if ok else f'validates {v}, expected {kt} (or {kn} of {kt} with {kname} KNOWN) :: ') + facts
            elif mism and all(k == kname or k.startswith(kname + '/') for k in mism) and sideok(g.get(kname), kgo) and sideok(s.get(kname), kcs):
                verdict = 'KNOWN'
                why = (f'{kname} go={g.get(kname) or "-"} cs={s.get(kname) or "-"} is the only differing name ({match} of {len(g)} matching)'
                       + ('' if (match, len(g)) == (kn, kt) else f' COUNT-NOTE: the linux reading was {kn} of {kt}') + f' :: {facts}')
            else:
                verdict, why = 'FAIL', (f'not the KNOWN shape ({kname} go={kgo} cs={kcs} alone): {kname} reads go={g.get(kname) or "-"} cs={s.get(kname) or "-"}; '
                                        f'differing: {" ".join(mism[:8]) or "none (and not validated)"} :: {facts}')
        npass += verdict == 'PASS'; nknown += verdict == 'KNOWN'; nfail += verdict == 'FAIL'
        print(f'REALMOD {ip}: {verdict} -- {why}')
    for ip in seen:
        if ip not in listed:
            nfail += 1; print(f'REALMOD {ip}: FAIL -- UNLISTED package in the run (validated={val.get(ip)}): no expectation is written for it')
    print(f'REALMOD-F4 "dotnet timed out" lines in the log: {tout} (EXPECT 0: the publish floor is max(-test-timeout, 30m))')
    print(f'REALMOD-VERDICT module={module} packages-in-log={len(seen)} pass={npass} known={nknown} fail={nfail}')
    return 1 if nfail else 0

# ================================================================================================ verify round 1 additions
# ---------------------------------------------------------------------------------------------------------------- hostwall
def hostwall(t0, path, lpath=None):
    # TRAIN L's template lesson (tL-seats-draft.txt:40): compare the converted HOST's own package elapsed, not the row
    # wall (mostly MSBuild and publish). go2cs_test_results.json is the C# host's event record; its package-level
    # terminal events (test "") carry the host's elapsed seconds. A host that leaves through its exit path appends a
    # second one with elapsed 0, so the reading is the LARGEST (read at L's three kept records: runtime/pprof fail
    # 377.01 beside a 627 s leg; runtime fail 5647.68 then fail 0 beside 5891 s; the TestTracebackSystem filter fail
    # 6.41 then fail 0 beside 317 s). One HOSTWALL line: this row's, fresh against the leg's start, and, when TRAIN L's
    # kept record of the same leg is given, L's and the ratio. A reading, never a gate; each S row's is TRAIN N's baseline.
    def el(p):
        if not p or not os.path.exists(p): return None
        try:
            with open(p, encoding='utf-8-sig') as f: ev = json.load(f).get('events') or []
        except Exception: return None
        pk = [(e.get('action'), float(e.get('elapsed') or 0)) for e in ev
              if isinstance(e, dict) and not e.get('test') and e.get('action') in ('pass', 'fail', 'skip', 'timeout')]
        return (pk[-1][0], max(x[1] for x in pk)) if pk else None
    if not os.path.exists(path): print('HOSTWALL no results record'); return 0
    if os.stat(path).st_mtime <= float(t0): print('HOSTWALL results record STALE (older than the leg): not read'); return 0
    a, b = el(path), el(lpath)
    if not a: print('HOSTWALL no package-level terminal event in the record'); return 0
    print(f'HOSTWALL cs-host={a[1]:.0f}s ({a[0]})' + (f' L={b[1]:.0f}s ({b[0]}) x{a[1] / max(b[1], 1.0):.2f}' if b else ''))
    return 0

# ---------------------------------------------------------------------------------------------------------------- live
def live(own_lock, wait=20.0):
    # Floor 1 ("never let two conversions overlap on one box") beyond this train's own lock, which only its own three
    # scripts take: the three readings of the train-assembly skill's LIVE gate. Nothing here kills or changes anything.
    #   1. another train's lock: <coord-scratch>/t<X>/.battery.lock other than <own_lock> (TRAIN L's is tL/.battery.lock);
    #   2. converter and harness processes, twice <wait> s apart (a CNR's go2cs.exe lives for seconds): go2cs.exe,
    #      go2cs.test.exe and BehavioralRunner.exe wherever they run (floor 1 is about the BOX), and -- verify round 2,
    #      COORD's ruling -- a dotnet.exe or a testhost.exe ONLY when its command line or its executable path names the
    #      tM worktree or a path under the battery's scratch root (never by bare process name: an IDE's or another
    #      checkout's dotnet is not this train's business). Neither this python nor the querying powershell is in that
    #      set, so the census cannot match itself;
    #   3. a run log still being written: a SUMMARY.txt one or two folders under a train's scratch folder with no
    #      DONE / ABORT / STOP stamp in its tail, beside a file written in the last 15 minutes (a run folder kept
    #      elsewhere is not seen: readings 1 and 2 are the ones that do not depend on where a run was launched).
    # One LIVE line per hit, listed and never killed (floor 5). Exit 1 on any hit: the caller refuses, or runs past it
    # on COORD's explicit LIVE_ACK=1.
    import glob
    ownabs = os.path.abspath(own_lock)
    root = os.path.dirname(os.path.dirname(ownabs))
    hits = []
    try: trains = [os.path.join(root, d) for d in os.listdir(root) if re.match(r'^t[A-Z]', d) and os.path.isdir(os.path.join(root, d))]
    except OSError: trains = []
    for t in trains:
        lk = os.path.join(t, '.battery.lock')
        if os.path.isdir(lk) and os.path.normcase(lk) != os.path.normcase(ownabs): hits.append(f'LIVE lock {lk}')
    # The path conditions, in both slash spellings: the tM worktree (a sibling of the scratch root) and the scratch root.
    top = os.path.dirname(root)
    frags = []
    for p in (os.path.join(top, 'tM') + os.sep, root + os.sep):
        frags += [p, p.replace('\\', '/')]
    psl = '@(' + ', '.join("'" + f.replace("'", "''") + "'" for f in dict.fromkeys(frags)) + ')'
    ps = ("$n = @('go2cs.exe', 'go2cs.test.exe', 'BehavioralRunner.exe'); $h = @('dotnet.exe', 'testhost.exe'); $f = " + psl + "; "
          "Get-CimInstance Win32_Process | Where-Object { $x = $_; $x.ProcessId -ne $PID -and ($n -contains $x.Name -or "
          "($h -contains $x.Name -and @($f | Where-Object { ($x.CommandLine -and $x.CommandLine.ToLower().Contains($_.ToLower())) -or "
          "($x.ExecutablePath -and $x.ExecutablePath.ToLower().Contains($_.ToLower())) }).Count -gt 0)) } | "
          "ForEach-Object { '{0} {1} {2}' -f $_.ProcessId, $_.Name, $_.ExecutablePath }")
    for k in (1, 2):
        r = subprocess.run(['powershell', '-NoProfile', '-Command', ps], capture_output=True, text=True, stdin=subprocess.DEVNULL)
        if r.returncode != 0:
            hits.append(f'LIVE census unreadable (powershell rc={r.returncode}): the process reading is NOT MEASURED'); break
        got = [l.strip() for l in r.stdout.splitlines() if l.strip()]
        hits += [f'LIVE process (reading {k}) {x}' for x in got]
        if got or k == 2: break
        time.sleep(float(wait))
    now = time.time()
    for t in trains:
        for s in glob.glob(os.path.join(t, '*', 'SUMMARY.txt')) + glob.glob(os.path.join(t, '*', '*', 'SUMMARY.txt')):
            try:
                with open(s, encoding='utf-8', errors='replace') as fh: tail = fh.read()[-4000:]
                d = os.path.dirname(s)
                newest = max(os.stat(os.path.join(d, f)).st_mtime for f in os.listdir(d))
            except (OSError, ValueError): continue
            if not re.search(r'\b(DONE|ABORT|STOP|STOPPED)\b', tail) and now - newest < 900:
                hits.append(f'LIVE log {s}: no DONE/ABORT/STOP stamp, and a file beside it was written {int(now - newest)} s ago')
    for h in hits: print(h)
    print(f'LIVE-VERDICT {"none" if not hits else str(len(hits)) + " hit(s)"} (other trains\' locks under {root}; process census; run logs of {len(trains)} train folder(s))')
    return 1 if hits else 0

# ================================================================================================ verify round 2 additions
# ---------------------------------------------------------------------------------------------------------------- execrows
def execrows(repo, ref='HEAD'):
    # The roster rows that carry an execution annotation at <ref>, by the same two regexes precheck's ROSTER arm uses.
    # tM-battery.sh reads it at the base and at HEAD: a row annotated at HEAD must read tiered=True in its own record,
    # a row annotated at the base and not at HEAD must read tiered=False (ruling R1: neither list is written anywhere).
    t = blob(repo, ref, 'docs/ValidatedTestPackages.md').replace('\r', '')
    rows = [(m.group(1), e.group(1)) for line in t.split('\n') if (m := ROW.match(line)) for e in EXEC.finditer(line)]
    for p, v in rows: print(f'EXECROW {p} {v}')
    print(f'EXECROWS ref={ref} n={len(rows)} roster-rows={len(ROW.findall(t))}')
    return 0 if ROW.search(t) else 1   # a roster that reads 0 rows is a format change, never 'no annotated row'

def execruled():
    print(f'EXECRULED n={len(EXEC_RULED)} ' + ' '.join(f'{p}[{v}]' for p, v in EXEC_RULED))
    return 0

# ---------------------------------------------------------------------------------------------------------------- rosterlinux
def rosterlinux(repo, pkg):
    # A row's LINUX expectation, from the roster of the tree the lane checked out: its 'linux: N + D' annotation (223
    # of 225 rows carry one at aa0a07d5fd), else its banked columns 2 and 3. tM-linux-legs.sh compares each row's
    # reading with it, so the driver holds no count of its own (one stated exception there: `os` on a uid-0 box).
    t = blob(repo, 'HEAD', 'docs/ValidatedTestPackages.md').replace('\r', '')
    for line in t.split('\n'):
        m = ROW.match(line)
        if not m or m.group(1) != pkg: continue
        x = LINUX_ANN.search(line)
        if x: print(f'ROSTERLINUX {pkg}: {x.group(1)} {x.group(2) or 0} (the row\'s linux annotation)')
        else: print(f'ROSTERLINUX {pkg}: {m.group(2)} {m.group(3) or 0} (no linux annotation: the banked columns)')
        return 0
    print(f'ROSTERLINUX {pkg}: no roster row'); return 1

if __name__ == '__main__':
    try: sys.stdout.reconfigure(encoding='utf-8', errors='replace')   # L: identifiers carry ᐧ/ᴛ/Δ (cp1252 console)
    except Exception: pass
    cmd, rest = sys.argv[1], sys.argv[2:]
    rc = {'precheck': lambda: precheck(*rest), 'coverage': lambda: coverage(*rest), 'canaries': lambda: canaries(*rest),
          'trx': lambda: trx(rest[0], rest[1:]), 'treport': lambda: treport(rest), 'csprojdrift': lambda: csprojdrift(*rest),
          'rosterrow': lambda: rosterrow(*rest), 'outparity': lambda: outparity(*rest), 'teattr': lambda: teattr(rest),
          'wallcmp': lambda: wallcmp(rest[0], rest[1:]), 'trxmethods': lambda: trxmethods(*rest),
          'deadlock': lambda: deadlock(rest), 'cmpnames': lambda: cmpnames(*rest), 'lockcheck': lambda: lockcheck(*rest),
          'realmod': lambda: realmod(rest), 'hunkclass': lambda: hunkclass(rest), 'hostwall': lambda: hostwall(*rest),
          'live': lambda: live(*rest), 'execrows': lambda: execrows(*rest), 'execruled': lambda: execruled(),
          'rosterlinux': lambda: rosterlinux(*rest)}[cmd]()
    sys.exit(rc or 0)
