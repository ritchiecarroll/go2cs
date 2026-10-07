# tQ-helpers.py -- TRAIN Q battery helpers (COORD, i7). DERIVED 2026-10-06 from trainP/tP-helpers.py. Q's changes
# (trainQ/tQ-CHANGES.md): H3_TOKENS is EMPTY again (no Q row edits Goroutine.cs; g-debugger-views landed with P);
# S1_NONE gains i9-pack-symbols' two hand-written csproj and S1_PLAIN its third (Q15, COORD's QUESTION 5); MAPLINE
# matches the 'global::go.' spelling (Q9); te_class reads the no-inline PARTIAL rendering as a class of its own, and
# teattr, hunkclass and testsrc-refresh count it (Q18); treport and realmod read a record's addressPairs (Q13); realmod
# classes a Go-side network or quota failure EXTERNAL (Q4); hostwall takes the previous train's own stamp as an S row's
# baseline (Q11); the wall and TE readers' labels name TRAIN P as the previous train. EXEC_RULED stays EMPTY (the one
# Q row that touches the roster moves no count and no annotation: read with git). Run by its author ONLY as controls on
# kept records and patches (tQ-DERIVE-REPORT.md section 3): no build, no conversion. P's header follows.
# (P) DERIVED 2026-10-04 from trainO/tO-helpers.py. P's changes
# (trainP/tP-CHANGES.md P8): H3_TOKENS held g-debugger-views (the one P row that edits Goroutine.cs); S1_NONE gained
# ConsumerUsings.csproj (r-csharp-consumer-smoke-r2's hand-written fixture, which names no LangVersion); the wall and TE
# readers' labels named TRAIN O as the previous train. No arm's logic changed at P: precheck, csprojtemplate, cnrexpect and the
# rest were O's bytes. EXEC_RULED stayed EMPTY (no P row touches the roster: read with git, each row's merge-base..sha).
# O's header follows.
# (O) DERIVED 2026-10-03 from trainN/tN-helpers.py (from trainM/, from
# tL-helpers.py). O's changes (trainO/tO-CHANGES.md): precheck's own-base handles a row with SEVERAL best common
# ancestors (MULTI-BASE, O3: N's Q20), its COUNT arm credits an insert two rows make IDENTICALLY once (D1: A1/A2's
# PtrToAnonStructPtr hunk), a DARWIN annotation arm beside the linux one (O6: the two C1 banks); csprojtemplate also
# derives the class from the TEMPLATE FILE's own delta since each added csproj's cut (D2). EXEC_RULED and H3_TOKENS stay
# EMPTY (no O row adds an execution annotation or edits Goroutine.cs: read with git, each row's merge-base..sha).
# NOT RUN by its author: compile() only. N's header follows.
# (N) N's changes (trainN/tN-CHANGES.md): EXEC_RULED is EMPTY (G's g-slog-roster-tc0 drops log/slog's
# release-tiered: 0 execution-config rows, item N3); H3_TOKENS is EMPTY (no N row edits Goroutine.cs); the H5 prose arm
# reads 'No row opts back out' as 0; new readers cnrexpect (CNR's package count, derived: N2), uflinux (the linux UF
# carve-out: N9b) and testsrc-refresh (the MS13 refresh of committed -tests sources: N10). REVIEW ROUND 1 (2026-10-03,
# trainN/tN-CHANGES.md section 4): precheck's REG arm accounts for keys a seat REMOVES on purpose; csprojdrift explains a csproj a
# seat deletes; new reader csprojtemplate (MS20). M's header follows.
# (M) trx, treport, csprojdrift, rosterrow, outparity, wallcmp, trxmethods, deadlock, cmpnames and lockcheck are L's bytes
# (treport gains one ENV line). CHANGED for M (trainM/tM-CHANGES.md): precheck is re-derived for M's seats (L's K8/K9/K6
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
#   TRAIN N additions (trainN/tN-CHANGES.md):
#   cnrexpect   <repo> [<ref>] [<goos>] [<goarch>]  check-no-regression's package count at <ref>, by its own predicate
#                                                 (every directory under the behavioral root holding a *.go, minus the
#                                                 [GoPlatformExclusive]/[GoArchExclusive] packages <goos>/<goarch> cannot measure)
#   uflinux     <repo>                            the linux lanes' UF census: untracked paths under src/core, with the
#                                                 linux-only TEST SOURCES a -tests run emits carved out by name (go list)
#   testsrc-refresh <repo> --out <patch> [--check-worktree] <patch> ...
#                                                 MS13: the union of WINDOWS rewrite patches, filtered to committed test
#                                                 sources only (refuses everything else), per-file hunk classes
#   csprojtemplate <repo> <base> <patch> <added-list>   (review round 1, MS20) a csproj the union's CNR moved: only the
#                                                 union template's delta (derived from the seats' own csproj edits and,
#                                                 at O, from csproj-template.xml's own delta since each csproj's cut) and
#                                                 only in a csproj the union added; exit 1 otherwise
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
# P (P8): + ConsumerUsings.csproj (src/tests/PackageTests/ConsumerUsings, r-csharp-consumer-smoke-r2: a hand-written
# package-consumer fixture that names no LangVersion, read (0,0,0) with git at 86553e51b5; COORD's item of 2026-10-04
# 12:03). The same seat's src/tests/CSharpConsumer/CSharpConsumer.csproj carries the CONDITIONED line (1,0,1) since
# COORD's re-cut and needs no entry; c2-ide-spike's two csproj sit under docs/, which the census skips by prefix.
S1_NONE = {'ElemAliasProbe.csproj', 'RidCompileAsset.csproj', 'UpdateTestTargets.csproj', 'J0UuidConsumer.csproj', 'ConsumerUsings.csproj',
           'PublishSymbols.csproj', 'PublishSymbolsLib.csproj'}
# Q (Q15; the seat table's QUESTION 5): i9-pack-symbols (c0c876fb27) adds THREE hand-written csproj under
# src/tests/PackageTests, read with git at its tip: PublishSymbols/PublishSymbols.csproj and PublishSymbols/lib/
# PublishSymbolsLib.csproj name no LangVersion (0, 0, 0): added to S1_NONE above BY NAME; PackageSymbols/
# PackageSymbols.csproj carries a PLAIN '<LangVersion>14</LangVersion>' (0, 0, 1), the shape S1 otherwise rules for
# go2cs-gen.csproj alone. All three are package-consumer fixtures no in-tree build reads. As trainP's helper stood,
# precheck HARD-FAILS on each of them at the union. They are admitted here as a RULING AMENDMENT FOR COORD TO CONFIRM
# (K's J0UuidConsumer precedent); the other way out is one commit by the i9 that conditions the third file's line.
S1_PLAIN = {'go2cs-gen.csproj', 'PackageSymbols.csproj'}   # want (0, 0, 1): a plain LangVersion 14, no conditioned line

# The registration KEY of each COUNT file: one key per registered thing, unique in the file (measured at aa0a07d5fd and
# at the pre-map union 8d7305053f: 0 duplicates in all six).
REG_KEY = {'src/go2cs/go2cs-src.projitems': r'Include="([^"]+)"', 'src/go2cs.slnx': r'<Project Path="([^"]+)"'}
REG_KEY_TESTS = r'public void ([A-Za-z0-9_]+)\(\)'
MIDDOT = '·'
EXEC = re.compile(MIDDOT + r'\s*execution\s*:\s*([a-z][a-z0-9-]*)\s*(?=' + MIDDOT + r'|\|)')
# N: 'No row opts back out' is the roster's sentence once g-slog-roster-tc0 lands (582de36d1b, docs line 528); without
# 'no' here the H5 arm read it as -1 and printed a false NOTE.
WORDNUM = {'no': 0, 'zero': 0, 'one': 1, 'two': 2, 'three': 3, 'four': 4, 'five': 5}
# N (item N3): EMPTY. G's seat g-slog-roster-tc0 (582de36d1b) drops log/slog's 'execution: release-tiered', the last
# annotated row (TRAIN M ruled log/slog alone: tL-seats-draft.txt:41, :63; G's seat accepted 2026-10-02 18:25). The
# guard then prints '0 with an execution config' and EXEC_ROWS_EXPECT defaults to 0.
# O: still EMPTY. No O row adds an execution annotation (the two C1 banks add darwin annotations only; read with git).
# P: still EMPTY. No P row touches docs/ValidatedTestPackages.md at all (read with git 2026-10-04).
EXEC_RULED = []   # the roster rows that carry an execution config
# Verify round 2: EXEC_RULED is the ONE site of that ruling. tQ-fixup.sh and tQ-battery.sh take their EXEC_ROWS_EXPECT
# default from `execruled`, the battery derives its TC0 and tiered rows from `execrows` at the base and at HEAD, and the
# lane drivers read the roster of the union they run on; nothing else names the rows.
# Verify round 3 (RULED): `execrows` (ROW + EXEC above) is the ONE READER of a row's execution config, for the battery
# AND for both lane drivers (tQ-linux-legs.sh and tQ-i9-shard.sh call it; each stamps the list it derived). EXEC matches
# the ANNOTATION field only (a middle dot before, a middle dot or '|' after): two rows still carry the phrase
# 'execution: release-tiered' in their PROSE, which an unanchored grep read as the annotation.
LINUX_ANN = re.compile(MIDDOT + r'\s*linux:\s*(\d+)(?:\s*\+\s*(\d+))?')
# O (O6): the DARWIN annotation, the same shape. c1-darwin-pilot-bank and c1-darwin-wave2-bank append '· darwin: N [+ D]'
# to 22 rows (after the linux annotation, which they leave byte-identical: read with git at 73de37d0da).
DARWIN_ANN = re.compile(MIDDOT + r'\s*darwin:\s*(\d+)(?:\s*\+\s*(\d+))?')
# H3: an identifier each seat that edits golib's Goroutine.cs is known by. A seat that edits the file and has no entry
# here FAILS the arm by name (R1: a literal that is short for the union is a finding, never silence).
# N and O: EMPTY. No row of N's list nor of O's draft edits src/core/golib/runtime/Goroutine.cs (read with git
# 2026-10-03, each row's merge-base..sha); at M the one entry was g-godebug-pc-line: ['systemBasis'], now on master. A
# row that edits the file still FAILS the arm by name until an entry is added here.
# P (P8): ONE entry. g-debugger-views (5ec7b56a46, cut on 59ee0d21bf) adds three lines above Goroutine.Run: a two-line
# comment and the attribute below, which the file holds 0 times at N's and O's unions and exactly once at the seat (read
# with git 2026-10-04; O changed no line of the file). The arm then wants HEAD's count == the seat's (1). The key is
# the ROW NAME: if G's seat is re-pointed under another ref name, this entry moves with it (else: FAIL H3 by name for
# the new row, and a NOTE for the stale entry).
# Q: EMPTY again. g-debugger-views landed with P (its three lines are in the base), and NO Q row edits
# src/core/golib/runtime/Goroutine.cs (read with git 2026-10-06, each of the 61 rows' merge-base..sha; the three rows seated at review round 1 touch no golib file).
H3_TOKENS = {}
FIXUP_SUBJ = re.compile(r'^fixup(-\d+)?: TRAIN Q')

def reg_arm(rx, head_t, base_t, sides):
    # precheck's REG arithmetic for ONE registration file (review round 1; a function so its control can call it).
    # sides = [(label, text before, text after)]: each seat's OWN change (its merge-base -> its sha, in row order), then
    # each fixup's ('fixup...' labels). -> (keys at head, duplicates, base keys lost, removed keys back, seat keys missing,
    # {removed key: who removed it}). O (O3): 'before' may be a precomputed KEY SET instead of a text (a MULTI-BASE row's
    # synthesized base, precheck below).
    from collections import Counter
    keys = Counter(rx.findall(head_t)); kset = lambda t: set(t) if isinstance(t, (set, frozenset)) else set(rx.findall(t))
    dups = sorted(k for k, n in keys.items() if n > 1)
    removed, added, seatadd = {}, set(), []
    for label, before, after in sides:
        ks, km = kset(after), kset(before)
        for k in sorted(km - ks): removed.setdefault(k, label)
        added |= ks - km
        if not label.startswith('fixup'): seatadd += [(label, k) for k in sorted(ks - km)]
    lost = sorted(kset(base_t) - set(keys) - set(removed))
    back = sorted(f'{removed[k]}:{k}' for k in removed if k in keys and k not in added)
    miss = [f'{n}:{k}' for n, k in seatadd if k not in keys and k not in removed]   # a key a later stacked row removed is not missing
    return keys, dups, lost, back, miss, removed

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
    # Verify round 1: the drivers count a seat row as '^ref|' (tQ-assemble.sh, tQ-fixup.sh, tQ-battery.sh); seat_shas also
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
    # Verify round 2 (floor 9, COORD's ruling): the head of the union is 'fixup: TRAIN Q' followed by ZERO OR MORE
    # 'fixup-N: TRAIN Q' single-parent commits, never a replaced sha. The fixups' contribution is the sum of the net
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
    # O (O3; N's Q20, trainN/tN-DERIVE-REPORT.md section 4): with --all. A row cut on a LOCAL MERGE of two of {the base,
    # earlier rows} (O's two-parent re-cut routes for c2-nuget-followups and g-method-value-fm-record) has SEVERAL best
    # common ancestors; N's `git merge-base` printed one of them, so the row's 'own' diff carried the other base's changes
    # and COUNT read expect too high on a correct union. Such a row is MULTI-BASE: its own change is synthesized as
    # count(sha) - count(V), V the virtual merge of its bases B1..Bk, count(V) = count(B1) + sum_j (count(Bj) -
    # count(merge-base(B1, Bj))) -- exact when the bases' own changes do not overlap, which a registration file's distinct
    # entries satisfy; stated on its own line (COUNTBASE) so a reader sees which rows took the synthesis.
    own, mbs_of = [], {}
    for i, (name, sha) in enumerate(seats):
        mbs = git(repo, 'merge-base', '--all', sha, base, *[s for _, s in seats[:i]]).split()
        mb = mbs[0] if mbs else ''
        own.append((name, sha, mb)); mbs_of[name] = mbs
        if len(mbs) > 1:
            soft.append(f'ok   COUNTBASE multi-base row {name}: {len(mbs)} best common ancestors with the base and the earlier rows '
                        f'({" ".join(m[:10] for m in mbs)}): its own change is synthesized from all of them (COUNT and REG)')
            # Review round 1 (F2-7): the DARWIN, PROOF and H3 arms below still read a row's own change as mb..sha with mb
            # the FIRST base; for a MULTI-BASE row that diff also carries the other bases' edits, which those arms would
            # charge to the row. Not synthesized there (single-parent re-cuts stay the ruled route, Q18): a NOTE when the
            # row's diff touches a file one of those arms reads, so the reader checks that row's lines by hand.
            hit = [p for p in git(repo, 'diff', '--name-only', mb, sha, '--', 'docs/ValidatedTestPackages.md', 'docs/validation/current',
                                  'src/core/golib/runtime/Goroutine.cs').split('\n') if p]
            if hit:
                note.append(f'NOTE MULTI-BASE row {name}: its diff from its first base {mb[:10]} touches {" ".join(hit[:4])}{" ..." if len(hit) > 4 else ""}: '
                            f'the DARWIN, PROOF and H3 arms read that diff UNSYNTHESIZED (another base\'s edits may be charged to this row): read their lines for {name} by hand')
    lca_cache = {}
    def lca(a, b_):
        if (a, b_) not in lca_cache: lca_cache[(a, b_)] = git(repo, 'merge-base', a, b_).strip()
        return lca_cache[(a, b_)]
    def own_net(f, name, sha, mb):
        mbs = mbs_of.get(name) or [mb]
        tot = net(mbs[0], sha, '--', f)
        for bj in mbs[1:]: tot -= net(lca(mbs[0], bj), bj, '--', f)
        return tot
    # O (D1, trainO/tO-premap.md section 4): a line block two rows insert IDENTICALLY at one place collapses to ONE copy in the
    # 3-way merge, while summing both rows' own nets counts it twice: at O, A2's red commit b9da3e6a2d registers A1's
    # CheckPtrToAnonStructPtr in Compile, Target and TranspileTests, the same 3 lines at the same anchor as A1 b95dd8d6e6,
    # and N's arm would read expect = head + 3 on a CORRECT union. The credit is git's own rule, read hunk by hunk: a -U0
    # hunk of the row's own diff (its base -> its sha) that the UNION SIDE of its merge (its base -> the merge's first
    # parent) holds identically (same old start and length, same removed and added lines) is the one copy git keeps, so
    # its net leaves the expectation once. Head mode and worktree mode both read the committed seat merges; a row with no
    # merge on the first-parent line (the bare base: MS9b control 2) or a MULTI-BASE row gets no credit (stated).
    merge_of = {}
    for c_ in git(repo, 'rev-list', '--first-parent', f'{base}..HEAD').split():
        par = git(repo, 'rev-list', '--parents', '-n', '1', c_).split()[1:]
        if len(par) == 2: merge_of[par[1]] = c_
    full_sha = {name: git(repo, 'rev-parse', sha + '^{commit}').strip() for name, sha in seats}
    HUNK_RX = re.compile(r'^@@ -(\d+)(?:,(\d+))? \+\d+(?:,\d+)? @@')
    def u0_hunks(a, b_, f):
        hs, cur_h = set(), None
        for line in git(repo, 'diff', '-U0', '--no-color', '--no-ext-diff', a, b_, '--', f, check=False).split('\n'):
            m = HUNK_RX.match(line)
            if m:
                if cur_h: hs.add((cur_h[0], cur_h[1], tuple(cur_h[2]), tuple(cur_h[3])))
                cur_h = [int(m.group(1)), int(m.group(2)) if m.group(2) is not None else 1, [], []]; continue
            if cur_h is None or line.startswith('\\'): continue   # file headers come before the first hunk
            if line.startswith('-'): cur_h[2].append(line[1:].rstrip('\r'))
            elif line.startswith('+'): cur_h[3].append(line[1:].rstrip('\r'))
        if cur_h: hs.add((cur_h[0], cur_h[1], tuple(cur_h[2]), tuple(cur_h[3])))
        return hs
    def identical_credit(f, name, sha, mb):
        if len(mbs_of.get(name) or []) > 1: return 0
        mc = merge_of.get(full_sha.get(name, ''))
        if not mc or not git(repo, 'diff', '--name-only', mb, sha, '--', f).strip(): return 0
        shared = u0_hunks(mb, sha, f) & u0_hunks(mb, mc + '^1', f)
        return sum(len(h[3]) - len(h[2]) for h in shared)
    # COUNT: each registration file == base + the sum of each seat's OWN net inserts (+ the fixup's own), less the
    # identical-insert credit (O, D1).
    for f in COUNT_FILES:
        b = blob(repo, base, f).count('\n'); c = cur(f); h = -1 if c is None else c.count('\n')
        touch = [(name, own_net(f, name, sha, mb), identical_credit(f, name, sha, mb)) for name, sha, mb in own]
        touch = [t for t in touch if t[1] != 0 or t[2] != 0]
        fx = fixup_delta(f)
        exp = b + sum(t[1] - t[2] for t in touch) + fx
        line = (f'COUNT {f}: base={b} head={h} expect={exp} :: ' + ' '.join(f'{t[0]}:{t[1]:+d}' + (f'(identical-insert credit -{t[2]})' if t[2] else '') for t in touch)
                + (f' fixup:{fx:+d}' if fx else ''))
        (hard if h != exp else soft).append(('FAIL ' if h != exp else 'ok   ') + line)
    # REG: an append-only registry holds BOTH sides' keys. Every key of the base file and every key a seat's own
    # commits add is in the merged file, and no key is registered twice.
    # N (review round 1, 2026-10-03): the registries are NOT append-only at N. Two seats REMOVE a registered key on
    # purpose: p1-hashset-module drops HashSet.go from go2cs-src.projitems, p2-n-cleanup five guard tests (read with git,
    # each row's merge-base..sha over the six COUNT files; no other row removes one). At M no seat removed a key, so
    # 'base-keys-lost' was a hard FAIL on every correct N union. A key is now LOST only when no seat's own commits and no
    # fixup removed it; a key a seat or a fixup removed that the merged file STILL holds, with no seat or fixup adding it
    # back, is 'removed-key-back' (a merge that kept the deleted line: the 3-way's silent subtraction, inverted).
    # O: one row removes a key on purpose, p1-hashset-module (HashSet.go from go2cs-src.projitems; its re-cut keeps that);
    # PREDICTED: 'seat-removed=[p1-hashset-module:...HashSet.go]' and nothing else (O's bat2 read exactly that).
    # P: one row removes a key on purpose, c2-sibling-package-name-r2: it RENAMES the behavioral sub-package
    # AliasNamespaceShadow/sortlocal to sort, so go2cs.slnx loses the key tests/Behavioral/AliasNamespaceShadow/sortlocal/
    # AliasNamespaceShadow.sortlocal.csproj and gains four (the renamed one and SiblingPackageNames' three). PREDICTED on
    # go2cs.slnx: 'seat-removed=[c2-sibling-package-name-r2:tests/Behavioral/AliasNamespaceShadow/sortlocal/...csproj]';
    # every other file: seat-removed=[]. The arm reads keys, so a rename needs no rename detection.
    for f in COUNT_FILES:
        rx = re.compile(REG_KEY.get(f, REG_KEY_TESTS)); c = cur(f) or ''
        def before_of(name, mb):   # O3: a MULTI-BASE row's 'before' is the virtual merge of its bases, as a key set
            mbs = mbs_of.get(name) or [mb]
            if len(mbs) == 1: return at(mb, f)
            ks = set(rx.findall(at(mbs[0], f)))
            for bj in mbs[1:]:
                kb, kl = set(rx.findall(at(bj, f))), set(rx.findall(at(lca(mbs[0], bj), f)))
                ks |= kb - kl; ks -= kl - kb
            return ks
        sides = [(name, before_of(name, mb), at(sha, f)) for name, sha, mb in own if git(repo, 'diff', '--name-only', mb, sha, '--', f).strip()]
        sides += [('fixup', at(cx + '^', f), at(cx, f)) for cx in fxc]          # the committed fixup(s)
        if wt: sides.append(('fixup(worktree)', at('HEAD', f), c))              # the fixup's worktree edit, before its commit
        keys, dups, lost, back, miss, removed = reg_arm(rx, c, at(base, f), sides)
        bad = dups or lost or miss or back
        (hard if bad else soft).append(
            f'{"FAIL" if bad else "ok  "} REG {f}: keys={sum(keys.values())} duplicate={dups[:6]} base-keys-lost={lost[:6]} seat-keys-missing={miss[:6]} '
            f'removed-key-back={back[:6]} seat-removed={sorted(f"{v}:{k}" for k, v in removed.items())[:8]}')
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
    # Q (COORD, 2026-10-06, at the first precheck of the assembled union): a seat cut on a LOCAL MERGE of two earlier rows
    # that REWRITES a line one of them added (the carrier's one-fixture fix-up; C1's re-cut composing two workflow
    # conditions) reads here as that line LOST: this arm takes ONE merge-base, and with two best common ancestors the
    # other base's line is charged to the union side. The drop is the seat's own resolution, so it is admitted ONLY by a
    # ruling keyed on its CONTENT: tQ-ruled-both.txt, beside this file, one line 'row|path|lost-count|digest|ruling',
    # digest = the first 12 hex of sha256 over the lost lines, stripped, sorted, joined by newlines. Any other lost line on
    # that pair, a different count, a line that came back or was duplicated, still FAILS; a ruled line no merge used FAILS.
    import hashlib as _hl
    ruled_both, _rb = {}, os.path.join(os.path.dirname(os.path.abspath(__file__)), 'tQ-ruled-both.txt')
    if os.path.exists(_rb):
        for _l in open(_rb, encoding='utf-8').read().replace('\r', '').split('\n'):
            if not _l.strip() or _l.startswith('#'): continue
            _f = _l.split('|')
            if len(_f) < 5 or not _f[2].isdigit() or len(_f[3]) != 12: hard.append(f'FAIL BOTH-RULED a line of tQ-ruled-both.txt does not parse: {_l[:80]}'); continue
            ruled_both[(_f[0], _f[1], _f[2], _f[3])] = 0
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
            rtag = ''
            if lostl and not backl and not dupl:
                _dg = _hl.sha256('\n'.join(x.strip() for x in sorted(lostl)).encode('utf-8')).hexdigest()[:12]
                _k = (name, path, str(len(lostl)), _dg)
                if _k in ruled_both: ruled_both[_k] += 1; badb = False; rtag = f' :: RULED-LOST {len(lostl)} line(s) digest={_dg} (tQ-ruled-both.txt)'
                else: rtag = f' :: lost-digest={_dg}'
            (hard if badb else soft).append(
                f'{"FAIL" if badb else "ok  "} BOTH {name} {path} (merge {c[:10]}): lines added by either side={adds} removed={rems} lost={len(lostl)} back={len(backl)} duplicated={len(dupl)}'
                + (f' :: first lost: {lostl[0].strip()[:110]}' if lostl else '') + (f' :: first back: {backl[0].strip()[:110]}' if backl else '')
                + (f' :: first duplicated: {dupl[0].strip()[:110]}' if dupl else '') + rtag)
    for _k, _n in ruled_both.items():
        if _n == 0: hard.append(f'FAIL BOTH-RULED unused line of tQ-ruled-both.txt: {_k[0]} {_k[1]} lost={_k[2]} digest={_k[3]} (no merge lost exactly these lines)')
    for sha_, name in full_of.items():
        if sha_ not in merged_p2: hard.append(f'FAIL BOTH {name}: no first-parent merge of {sha_[:10]} between {base} and HEAD (the row is not merged as itself)')
    soft.append(f'ok   BOTH: {nmerge} first-parent merge(s), {nboth} (merge, path) pair(s) where both sides changed the path since their merge-base')
    # N: no N row moves a count either: g-slog-roster-tc0 drops ONE execution annotation (log/slog) and moves no N + D and no
    # linux annotation (225 / 225 rows, 223 / 223 linux annotations, read with these arms' own regexes at 582de36d1b
    # against 8f46a9adae, 2026-10-03; N's battery PRE-1 read exactly that at 59ee0d21bf).
    # O: no O row moves a count or a linux annotation either; the two C1 banks ADD 22 darwin annotations (ruled), which
    # the DARWIN arm below expects by derivation (read with git at 73de37d0da: their linux text is byte-identical).
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
    # O (O6): the DARWIN annotations. Unlike the linux ones they are RULED to move at O (c1-darwin-pilot-bank 10 rows,
    # c1-darwin-wave2-bank 12 more), so the expectation is DERIVED, never typed: the base's annotations, then each row's
    # OWN darwin edits (its own base -> its sha), in row order; the merged roster must hold exactly that. A darwin
    # annotation lost in a 3-way (the roster is the file the two banks and N's bank step share), one a row did not add, or
    # a changed value FAILS by name. At the bare base (MS9b control 2) it FAILS on all 22: the arm's negative control.
    dar = lambda t: {m.group(1): x.group(0) for line in t.split('\n') if (m := ROW.match(line)) and (x := DARWIN_ANN.search(line))}
    dbase, dh = dar(at(base, rp)), dar(rc_)
    dexp, dseat = dict(dbase), []
    for name, sha, mb in own:
        if not git(repo, 'diff', '--name-only', mb, sha, '--', rp).strip(): continue
        d0, d1 = dar(at(mb, rp)), dar(at(sha, rp))
        for k in sorted(set(d0) | set(d1)):
            if d0.get(k) == d1.get(k): continue
            dseat.append(f'{name}:{k}')
            if k in d1: dexp[k] = d1[k]
            else: dexp.pop(k, None)
    dbad = sorted(k for k in set(dexp) | set(dh) if dexp.get(k) != dh.get(k))
    (hard if dbad else soft).append(
        f'{"FAIL" if dbad else "ok  "} ROSTER darwin annotations base={len(dbase)} merged={len(dh)} expect={len(dexp)} (the base + {len(dseat)} row-own edit(s): '
        + (', '.join(sorted(set(x.split(':')[0] for x in dseat))) or 'none') + ') mismatched: '
        + (' '.join(f'{k}:[{dexp.get(k)}]->[{dh.get(k)}]' for k in dbad[:6]) or 'none'))
    # (2) "MUST NOT touch the frozen snapshot": every docs/validation/<release> folder of the base (all but current/).
    snap = [x for x in git(repo, 'diff', '--name-only', base, 'HEAD', '--', 'docs/validation').split('\n') if re.match(r'^docs/validation/\d', x)]
    (hard if snap else soft).append(f'{"FAIL" if snap else "ok  "} SNAPSHOT paths under a frozen docs/validation/<release>/ changed since {base}: {len(snap)} ' + ' '.join(snap[:5]))
    # (3) "Keep the regenerated proof pages": every page under docs/validation/current that a seat's OWN commits changed
    # (derived, row by row; at the 24-row list that is G's net.http.md and internal.godebug.md) is, at HEAD, the blob of
    # the LAST row that changed it. A page HEAD holds differently was edited by a merge, a follow-up or the fixup: FAIL,
    # to be read (after a bank step that regenerates a page this arm fails by design: tQ-README.md MS11 says so).
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
    # tL-seats-draft.txt:41 and :63; it moves together with EXEC_ROWS_EXPECT in tQ-fixup.sh and tQ-battery.sh.
    exl = ' '.join(f'{p}[{v}]' for p, v in execrows) or 'none'
    rul = ' '.join(f'{p}[{v}]' for p, v in EXEC_RULED) + ' alone' if EXEC_RULED else 'none: no row carries one'   # N: the ruled set is empty
    (hard if execrows != EXEC_RULED else soft).append(
        ('FAIL ROSTER execution annotations: ' + exl + ' (ruled: ' + rul + ')') if execrows != EXEC_RULED
        else 'ok   ROSTER execution annotations: ' + exl + ' (the ruled set; the roster guard is the count of record)')
    # H5 (a NOTE, never a failure: prose, G's to word, no guard counts it): the sentence that says how many rows opt
    # back out of the default execution config, against the number of rows that carry the annotation.
    m5 = re.search(r'\b([A-Za-z]+|\d+) rows? opts? back out', rc_)
    if m5:
        said = WORDNUM.get(m5.group(1).lower(), int(m5.group(1)) if m5.group(1).isdigit() else -1)
        if said != len(execrows):
            note.append(f'NOTE H5 {rp}: the prose says "{m5.group(0)}" while {len(execrows)} row(s) carry an execution annotation (a prose fixup is owed: COORD writes the words through tQ-fixup.sh PROSE_PATCH; tQ-README.md MS3)')
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
            hard.append(f'FAIL H3 seat {name} edits {gp} and tQ-helpers.py H3_TOKENS holds no identifier for it: add one (the arm does not read that seat\'s side)')
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
    # and no M seat goldens them. HARD. At M's frozen list (verify round 3) the arm read 0: row p2-test-overload-references
    # (e002a552a8) deleted that repro module in-seat. N: at the 27-row draft list the arm FIRES: r-jwt-promoted-iface-repros
    # (ee52639557) adds PromotedIfaceFromLibEmbed, PromotedIfaceFromLibEmbedLib and PromotedIfaceTestEmbed with go.mod and
    # Go sources only (read with git 2026-10-03): COORD's ruling, trainN/tN-README.md MS2 (N seated it in neither form).
    # O: r-atlas-batch-repros (d834821da0) adds FloatCompareUntypedMaxUint64 and PtrToAnonStructPtr with Go only; the arm
    # reads 0 at the union only because g-float-untyped-const-compare and r-ptrptr-anon-struct-lift each commit one
    # directory's emission (the trio boards as a set, as ruled 04:33); r-jwt-promoted-iface-repros is not a row as cut.
    # P: no row adds a Go-only directory (the pre-map's unioncheck read H4 = 0 at its chain head, 16 directories added,
    # each with its emission).
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
        if base_ in S1_PLAIN: want = (0, 0, 1)   # Q (Q15): go2cs-gen.csproj and, by COORD's amendment, PackageSymbols.csproj
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
        # Q (Q13; i9-address-variant-pairing-on-runtests): the record states what the address-variant pairing re-keyed.
        # oneToOne = a group of ONE address-embedding name a side, paired onto its normalized key; nToN = a group of N
        # names a side (the same N on both), paired BY RUN ORDER. Absent = nothing was paired. Neither moves the matched
        # count; nToN is a pairing by position, STATED here so it is never read as a match by name. What the pairing
        # REFUSED (groups of different sizes on the two sides) stays one-sided in the verdict maps: counted below as
        # unpaired-address-names. A non-zero count means that row is NOT validated on those names (a test ran on one
        # side only, or ran a different number of times): a mismatch to read, never a pairing to assume.
        ap = c.get('addressPairs') or {}
        unp = [k for k in diff if ADDRSHAPE.search(k) and (gv.get(k) is None) != (cv.get(k) is None)]
        print(f'{label} COMPARISON status={status} matched={matched} go={len(gv)} cs={len(cv)} disclosed={len(disc)} '
              f'withdrawn={len(wd)} skipped={len(c.get("skipped") or [])} errors(raw)={rawerr} error-names={len(errs)} undisclosed-verdict-diffs={len(diff)} '
              f'address-pairs=1:1:{ap.get("oneToOne", 0)}/N:N:{ap.get("nToN", 0)} unpaired-address-names={len(unp)}')
        if unp: print(f'{label} UNPAIRED-ADDRESS ' + ' '.join(unp)[:600])
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
        if st == 'D':
            # N (review round 1): a csproj a SEAT deletes is explained (p2-n-cleanup deletes src/tests/GenericTests/
            # GenericTests.csproj, the one deletion among the 27 rows). It counts as OTHER only when no seat commit touched it.
            touched = git(repo, 'log', '--no-merges', '--format=%h', f'{M}..{U}^', '--', p).split()
            (seat if touched else other).append(f'{p} (deleted; {",".join(touched[:3]) or "no seat commit"})')
            bump('seat-delete' if touched else 'OTHER-deleted')
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
    for s in seat[:20]: print('  SEAT-EDIT/DELETE ' + s)
    for s in other[:40]: print('  OTHER ' + s)
    print(f'CSPROJ verdict: {"TEMPLATE DRIFT ONLY (+ seat-edited or seat-deleted projects)" if not other else "UNEXPLAINED csproj drift: " + str(len(other))}')
    return 1 if other else 0

# ---------------------------------------------------------------------------------------------------------------- csprojtemplate
# N (review round 1, 2026-10-03; tQ-README.md MS20, the route is COORD's ruling): the behavioral csproj the N seats ADD
# were cut BEFORE the two template seats landed in the union (14 at the 27-row list, TrimMode count 0 in each at its
# own sha: g-publish-keep's ReadyToRun condition and TrimMode comment + line, and, in the four libraries, the old
# '<PackageId>go.$(AssemblyName)</PackageId>' c2-nugetgo-id-pattern's template (B) replaces). The union's CNR rewrites
# every csproj from the union template, so the fixup's step 5 died 'CNR moved a csproj', a die GOLDEN_CLASS=any cannot
# reach. This reader CLASSIFIES such a move; tQ-fixup.sh decides (CSPROJ_TEMPLATE=stop|accept).
# The TEMPLATE DELTA is DERIVED, never typed: the lines the seats' own commits removed from and added to EXISTING
# behavioral csproj between the base and HEAD (git diff --diff-filter=M). A line belongs to the delta when at least
# TMPL_MIN distinct files changed it the same way: a corpus-wide template change touches hundreds (g-publish-keep 791
# behavioral csproj, c2 55 libraries, read with git), a seat's own project edit one or a few. A moved csproj is
# TEMPLATE-ONLY when the union ADDED it (<added-list>: the fixup's newM-union.txt) and every line its hunks remove or add
# is in the delta. Exit 1 when any moved csproj is not.
#   csprojtemplate <repo> <base> <patch: git diff -U0 of the moved csproj vs HEAD> <added-list> [<head>]
#   (<head> defaults to HEAD; another ref is for a control read against a seat tip, trainN/tN-README.md section 8)
# O (D2, trainO/tO-premap.md section 6): at O the seats' own csproj edits (base..HEAD, base = N's landed master) hold ONE
# corpus-wide change, g-single-file-r2r-restore's ReadyToRun line; N's TrimMode lines landed in the BASE, so they are no
# seat's edit any more, and the 18 behavioral csproj the O rows add (cut on 8f46a9adae / e2008427b1, before N) would ALL
# read outside the class and CSPROJ_TEMPLATE=accept would still die. The template itself is the other source: for each
# moved csproj the union ADDED: an ADDED hunk line is in the class when the HEAD template holds it literally (stripped,
# non-empty; a placeholder line never appears in a csproj, so it admits nothing); a REMOVED hunk line when the template
# held it at that csproj's CUT (the parent of the commit that added it: git log --diff-filter=A base..head) and does not
# hold it at HEAD. The seats' own delta still admits either kind, as at N.
# Read with git: 8f46a9adae..4b30906aea changes the template by the TrimMode comment + line (and the PackageId marker,
# which renders go.$(AssemblyName) unchanged in an Exe csproj), and PtrToAnonStructPtr.csproj at b95dd8d6e6 differs
# from InitFrameNames.csproj at 4b30906aea by exactly those two lines beside its own name and references.
TMPL_MIN = 10
TMPL_FILE = 'src/go2cs/csproj-template.xml'
# Review round 1 (F2-8): a bare XML element tag on its own line (no attribute, no text): admitted by the HEAD-literal rule
# only through the seats' delta or a template count that grew since the csproj's cut.
TMPL_STRUCT = re.compile(r'^</?[A-Za-z][A-Za-z0-9_.:-]*\s*/?>$')
def csprojtemplate(repo, base, pf, addlist, head='HEAD'):
    d = git(repo, '-c', 'core.quotepath=false', 'diff', '-U0', '--no-renames', '--diff-filter=M', base, head, '--', 'src/tests/Behavioral/*.csproj')
    remf, addf, cur, nmod = {}, {}, None, set()
    for line in d.split('\n'):
        if line.startswith('diff --git '): cur = line.split(' b/', 1)[-1].strip(); nmod.add(cur); continue
        if line.startswith(('--- ', '+++ ', '@@', 'index ', 'old mode', 'new mode')): continue
        if line.startswith('-') and line[1:].strip(): remf.setdefault(line[1:].strip(), set()).add(cur)
        elif line.startswith('+') and line[1:].strip(): addf.setdefault(line[1:].strip(), set()).add(cur)
    drem = {l: len(fs) for l, fs in remf.items() if len(fs) >= TMPL_MIN}
    dadd = {l: len(fs) for l, fs in addf.items() if len(fs) >= TMPL_MIN}
    for l, n in sorted(drem.items()): print(f'CSPROJTEMPLATE delta removed ({n} files): {l[:160]}')
    for l, n in sorted(dadd.items()): print(f'CSPROJTEMPLATE delta added   ({n} files): {l[:160]}')
    added = set(x.strip() for x in open(addlist, encoding='utf-8').read().replace('\r', '').split('\n') if x.strip())
    # O (D2): the template file's own delta between each added csproj's cut and HEAD (see the header above TMPL_MIN).
    from collections import Counter
    tlist = lambda ref: [l.strip() for l in git(repo, 'show', f'{ref}:{TMPL_FILE}', check=False).replace('\r', '').split('\n') if l.strip()]
    tlines = lambda ref: set(tlist(ref))
    thead, tcut = tlines(head), {}
    thead_n = Counter(tlist(head))
    def tdelta(f):
        ac = git(repo, 'log', '--no-merges', '--diff-filter=A', '--format=%H', '-n', '1', f'{base}..{head}', '--', f, check=False).strip()
        cut = (ac + '^') if ac else base
        if cut not in tcut:
            old = tlines(cut)
            # (removed lines, added lines, and the lines whose COUNT in the template grew since the cut: a new block's
            # structural tags are there although the set difference cannot see them)
            tcut[cut] = (old - thead, thead - old, set((thead_n - Counter(tlist(cut))).keys()))
            print(f'CSPROJTEMPLATE template delta at the cut {git(repo, "rev-parse", "--short=10", cut, check=False).strip() or cut}: '
                  f'removed={len(tcut[cut][0])} added={len(tcut[cut][1])} :: ' + ' | '.join(sorted(tcut[cut][1]))[:300])
        return tcut[cut]
    per = {}
    for f, rem, add in te_hunks(pf):
        e = per.setdefault(f, {'h': 0, 'r': 0, 'a': 0, 'out': []})
        e['h'] += 1; e['r'] += len(rem); e['a'] += len(add)
        trem, tadd, tgain = tdelta(f) if f in added else (set(), set(), set())
        e['out'] += [f'-{x.strip()}' for x in rem if x.strip() and x.strip() not in drem and x.strip() not in trem]
        # an ADDED line is in the class when the HEAD template holds it literally (a cut-relative delta alone would refuse
        # a csproj re-cut onto a newer base that kept its older content: the template then already held the line at the cut)
        # Review round 1 (F2-8): EXCEPT a bare structural tag (<PropertyGroup>, </ItemGroup>, </Project>, ...): every
        # template holds those, so the literal rule would admit an extra block made of them unread. Such a line is in the
        # class only when the seats' delta has it or the template's own count of it grew since the csproj's cut.
        e['out'] += [f'+{x.strip()}' for x in add if x.strip() and x.strip() not in dadd and x.strip() not in tgain
                     and (x.strip() not in thead or TMPL_STRUCT.match(x.strip()))]
    tonly, other = [], []
    for f, e in sorted(per.items()):
        ok = f in added and not e['out']
        (tonly if ok else other).append(f)
        print(f'CSPROJTEMPLATE {f}: added-by-the-union={"yes" if f in added else "NO"} hunks={e["h"]} removed={e["r"]} added={e["a"]} '
              f'outside-the-delta={len(e["out"])}' + (f' first: {e["out"][0][:120]}' if e['out'] else ''))
    print(f'CSPROJTEMPLATE-VERDICT files={len(per)} template-only={len(tonly)} other={len(other)} :: delta removed-lines={len(drem)} '
          f'added-lines={len(dadd)} (from {len(nmod)} behavioral csproj the seats modified {base[:10]}..{head[:10]}, threshold {TMPL_MIN} files) + the template file delta at {len(tcut)} cut(s) (O, D2) :: '
          + ('every moved csproj is one the union added, moved only by the union template' if not other and per else
             'no moved csproj read' if not per else 'NOT the template class: ' + ' '.join(other)[:400]))
    return 0 if per and not other else 1

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
# TRAIN M's emission class on sweep-rewritten sources, kept for N: the committed test sources stay stale by G's class
# until the MS13 refresh (testsrc-refresh below, a bank step AFTER N's battery), so N's TE still reads it; the baseline
# is now TRAIN M's own patch (run3), so OTHER lists what N's seats move. L's D6 and cross-package signatures left with
# their seats (and with them the two raw-codepoint character classes tL-helpers.py carried at its lines 351-352). What
# M's seats changed:
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
# Q (Q9; the audit's carry item, COORD's notes of 2026-10-04 18:28 and 2026-10-05 20:37): the map line is written in
# TWO spellings. Read with git at TRAIN P's landed line: 3526 lines '[assembly: go.GoPositionMap(' and 413
# '[assembly: global::go.GoPositionMap('; P's regex matched the first only, so a re-encode of a line of the second
# spelling read as OTHER (the class count read low: P's refresh message was corrected by hand, 165 map lines).
MAPLINE = re.compile(r'^\[assembly: (global::)?(go\.)?GoPositionMap\(')
# Q (Q18): c2-noinline-partial (the carrier, a Q row) changes what a no-inline METHOD looks like. A method that takes
# the mark and has a body is the IMPLEMENTING part of a partial method: the word 'partial' stands where the attribute
# prefix stood, and go2cs-gen's NoInliningPartialGenerator writes the declaring part with the attribute. So
#     [MethodImpl(MethodImplOptions.NoInlining)] internal static void f(...) {     becomes
#     internal static partial void f(...) {
# (the using line stays; a lambda, a local function, a bodyless declaration and an init keep the attribute). The 74
# corpus files the seat committed are that line kind and nothing else (the reader's control: tQ-DERIVE-REPORT.md
# section 3). NO committed -tests source was regenerated by the seat, and 180 of them hold 938 prefix lines at TRAIN
# P's landed line, about 738 on a method declaration: every -tests run of the union re-emits those methods in the new
# form, so the sweeps' rewrites carry this class in bulk until the landing refresh (MS13) commits it. It is its own
# class, N-PARTIAL, counted and named, never OTHER: a removed line that holds the prefix, paired with an added line
# that is the same text with the prefix gone and ONE ' partial ' inserted.
PARTIAL = ' partial '

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
    # -> (class, noinline lines, using lines, map lines, partial lines)
    r, a = list(rem), list(add)
    n = u = m = p = 0
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
    for x in list(r):   # Q (Q18): the partial rendering of a no-inline method
        if NOINL in x:
            y = x.replace(NOINL, '', 1)
            z = next((w for w in a if PARTIAL in w and w.replace(PARTIAL, ' ', 1) == y), None)
            if z is not None: r.remove(x); a.remove(z); p += 1
    if r or a: return 'OTHER', n, u, m, p
    if p: return 'N-PARTIAL', n, u, m, p
    return ('G-FRAME' if n or u else 'MAP'), n, u, m, p

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
    cnt = {'G-FRAME': 0, 'MAP': 0, 'N-PARTIAL': 0, 'OTHER': 0}; nl = ul = pl = 0; other, gfiles, ofiles, pfiles = [], set(), set(), set()
    for h in hunks:
        c, n, u, m, p = te_class(h[1], h[2])
        cnt[c] += 1; nl += n; ul += u; pl += p
        if c == 'G-FRAME': gfiles.add(h[0])
        if c == 'N-PARTIAL': pfiles.add(h[0])
        if c == 'OTHER': other.append(h); ofiles.add(h[0])
    # Verify round 2 (tQ-README.md MS13, a RULED deferral): the files whose EVERY hunk is G's frame class or a map
    # re-encode are the committed test sources that are stale only by that class; the count is the number the queued
    # corpus-wide refresh of committed -tests sources starts from.
    gonly = gfiles - ofiles
    print(f'TE files={len(files)} hunks={len(hunks)} g-frame={cnt["G-FRAME"]} (noinline-lines={nl} using-lines={ul}, in {len(gfiles)} file(s); '
          f'g-frame-only files={len(gonly)}) map-only={cnt["MAP"]} other={cnt["OTHER"]} '
          f'n-partial={cnt["N-PARTIAL"]} (partial-lines={pl}, in {len(pfiles)} file(s); files with no OTHER hunk among them={len(pfiles - ofiles)})')
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
        print(f'TE BASELINE absent ({baseline}): the OTHER hunks were not compared with the previous train')
    print('TE VERDICT reading, not gated: at Q an N-PARTIAL hunk is the EXPECTED class (the carrier renders a no-inline method as a partial method and regenerated no committed test source: the landing refresh commits it), a G-FRAME hunk is a stale source the earlier refreshes missed, OTHER is what the Q seats move; '
          + ('no baseline was given, so OTHER is a count only' if new is None else
             f'{len(new)} OTHER hunk(s) are new against the baseline (TRAIN P patch): READ EACH (a candidate seat footprint or a G line beside standing drift)'))
    return 0

# ---------------------------------------------------------------------------------------------------------------- hunkclass
def hunkclass(patches):
    # The fixup's golden step: every hunk of the behavioral files the union's CNR regenerated must be G's frame class or
    # a position-map re-encode (train-assembly skill: "classify by the diff's CONTENT, the seat's own intended line,
    # nothing else"). One line per file, then the verdict. Exit 1 when any hunk is OTHER, 0 otherwise (0 hunks included).
    hunks = [h for pf in patches if os.path.exists(pf) for h in te_hunks(pf)]
    per = {}
    for h in hunks:
        c, n, u, m, p = te_class(h[1], h[2])
        d = per.setdefault(h[0], {'G-FRAME': 0, 'MAP': 0, 'N-PARTIAL': 0, 'OTHER': 0, 'n': 0, 'u': 0, 'p': 0})
        d[c] += 1; d['n'] += n; d['u'] += u; d['p'] += p
    other = sorted(f for f, d in per.items() if d['OTHER'])
    for f, d in sorted(per.items()):
        print(f'HUNKCLASS {f}: noinline-lines={d["n"]} using-lines={d["u"]} map-only-hunks={d["MAP"]} other-hunks={d["OTHER"]} n-partial-hunks={d["N-PARTIAL"]} partial-lines={d["p"]}')
    # Q (Q18): the no-inline partial rendering is a KNOWN class, like G's frame class: a golden that moves by it alone
    # predates the carrier (the rehearsal read exactly one, StatementTableFuncNames, and a row carries it).
    # REVIEW ROUND 1: KNOWN is not PREDICTED. The exit status still says only 'no OTHER hunk' (0), but the verdict line
    # now carries every class's hunk count and the files that hold an N-PARTIAL hunk, and the fixup's golden step READS
    # them: with the default GOLDEN_CLASS it stops on an N-PARTIAL golden (0 are predicted at Q; the one the rehearsal
    # read is a row), and its commit message is written from these counts, never from a fixed sentence.
    npf = sorted(f for f, d in per.items() if d['N-PARTIAL'])
    print(f'HUNKCLASS-VERDICT files={len(per)} hunks={len(hunks)} other-files={len(other)} '
          + ("every hunk is G's frame class, the no-inline partial rendering or a position-map re-encode" if not other else 'OTHER line kinds in: ' + ' '.join(other)[:500])
          + f' :: n-partial-hunks={sum(d["N-PARTIAL"] for d in per.values())} partial-lines={sum(d["p"] for d in per.values())}'
          + f' g-frame-hunks={sum(d["G-FRAME"] for d in per.values())} map-only-hunks={sum(d["MAP"] for d in per.values())} other-hunks={sum(d["OTHER"] for d in per.values())}'
          + f' n-partial-files={len(npf)}' + ((' [' + ' '.join(npf)[:400] + ']') if npf else ''))
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
        if r > 1.25: slow.append(f'{n} Q={w}s P(min)={kb}s x{r:.2f} rc={rc}')   # Q: this run vs TRAIN P's summary (the names L and K above are L's code)
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
# Q (Q4): the texts that class a Go-side failure EXTERNAL. A literal chosen by a ruling, not by a seat list; each
# alternative is a text Go's net / net/http / grpc stack prints for a failure OUTSIDE the process under test.
EXTERNAL_RX = re.compile(r'(ResourceExhausted|quota exhausted|dial tcp|i/o timeout|no such host|connection refused|connection reset by peer|'
                         r'TLS handshake timeout|network is unreachable|[Tt]emporary failure in name resolution|server misbehaving)')
# Q (Q13): an address-embedding test name, the shape pairAddressVariantNames normalizes (a 0x-prefixed hex run).
# REVIEW ROUND 1: the CONVERTER's own pattern (addressTokenPattern at i9-address-variant-pairing-on-runtests,
# src/go2cs/testConversion.go: `0x[0-9a-fA-F]+`, any length). The first draft asked for six or more hex digits, so a
# one-sided name with a short token was left out of 'unpaired-address-names' (a reading: the name still counted in
# the undisclosed verdict differences).
ADDRSHAPE = re.compile(r'0x[0-9a-fA-F]+')

def go_side_text(c):
    # -> [(test name or '', output text)] of the Go ORACLE's own output, from a comparison record: the 'go test: ...'
    # entries of errors (the converter appends go test's -json event stream to them) and the go-side stderr tail.
    out = []
    for e in c.get('errors') or []:
        if not isinstance(e, str) or not e.startswith('go test:'): continue
        for line in e.split('\n'):
            line = line.strip()
            if line.startswith('{'):
                try: ev = json.loads(line)
                except Exception: out.append(('', line)); continue
                if ev.get('Action') == 'output': out.append((ev.get('Test') or '', ev.get('Output') or ''))
            elif line: out.append(('', line))
    t = ((c.get('stderr') or {}).get('go') or {}).get('text') or ''
    if t: out.append(('', t))
    return out

def external_go(mism, g, gotext):
    # -> [(name, quoted text)] when EVERY differing name is a Go-side failure whose own oracle output (or its parent
    # test's) holds an external text; None otherwise. A differing name the oracle's output cannot be tied to keeps the
    # package a FAIL: an unattributed text never classes a failure.
    if not mism: return None
    res = []
    for k in mism:
        if g.get(k) != 'fail': return None
        par = k.split('/')[0]
        hit = next((m.group(0) + ': ' + txt.strip()[:110] for t, txt in gotext if t in (k, par) and (m := EXTERNAL_RX.search(txt))), None)
        if not hit: return None
        res.append((k, hit))
    return res

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
    npass = nknown = nfail = next_ = 0
    listed = set()
    for spec in specs:
        name, _, want = spec.partition('=')
        ip = module if name == '.' else f'{module}/{name}'
        listed.add(ip)
        rec = os.path.join(root, *([] if name == '.' else name.split('/')), 'go2cs_test_comparison.json')
        g, s, disc, status, apairs, gotext = {}, {}, set(), 'no-record', {}, []
        if os.path.exists(rec):
            try:
                c = json.load(open(rec, encoding='utf-8-sig'))
                g, s, status = c.get('go') or {}, c.get('csharp') or {}, c.get('status')
                disc = {x.split(' (')[0] for x in c.get('disclosed') or []}
                apairs = c.get('addressPairs') or {}; gotext = go_side_text(c)
            except Exception as e:
                status = f'unreadable-record ({type(e).__name__})'
        mism = sorted(k for k in set(g) | set(s) if g.get(k) != s.get(k) and k not in disc)
        match = sum(1 for k in g if g.get(k) == s.get(k))
        v = val.get(ip)
        facts = f'validated={v} record={status} go={len(g)} cs={len(s)} matching={match} differing={len(mism)}' + (
            f' address-pairs=1:1:{apairs.get("oneToOne", 0)}/N:N:{apairs.get("nToN", 0)}' if apairs else '')
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
        # Q (Q4; COORD's notes of 2026-10-05 14:49 and 20:37): KNOWN-EXTERNAL. A package that would read FAIL is classed
        # EXTERNAL when EVERY undisclosed differing name is a Go-side FAILURE (go=fail, whatever the C# side read) and
        # the ORACLE's own output for that test (the 'go test:' entry of the record's errors) holds a network or quota
        # text (EXTERNAL_RX). It is never a pass: the line names the tests and quotes the text, the verdict line counts
        # it apart, and it is OWED an isolated re-read at the landing head (tQ-reread.sh; tQ-land-final-reads.sh refuses
        # a landing head whose owed re-read is missing or not clean). P's case: x/mod sumdb/tlog
        # TestCertificateTransparency Go=FAIL C#=pass, 'rpc error: code = ResourceExhausted desc = quota exhausted' from
        # the certificate-transparency log server the Go test queries. A C#-side failure with the same text is NOT this
        # class (not ruled): it stays a FAIL to read.
        if verdict == 'FAIL' and ip in seen:
            ext = external_go(mism, g, gotext)
            if ext:
                verdict = 'EXTERNAL'
                why = (f'Go-side oracle failure(s) with a network or quota text, C# verdicts kept: ' + '; '.join(f'{k} go={g.get(k)} cs={s.get(k) or "-"} [{q}]' for k, q in ext)[:420]
                       + f' :: OWED: an isolated re-read at the landing head (tQ-reread.sh) :: {facts}')
        npass += verdict == 'PASS'; nknown += verdict == 'KNOWN'; nfail += verdict == 'FAIL'; next_ += verdict == 'EXTERNAL'
        print(f'REALMOD {ip}: {verdict} -- {why}')
    for ip in seen:
        if ip not in listed:
            nfail += 1; print(f'REALMOD {ip}: FAIL -- UNLISTED package in the run (validated={val.get(ip)}): no expectation is written for it')
    print(f'REALMOD-F4 "dotnet timed out" lines in the log: {tout} (EXPECT 0: the publish floor is max(-test-timeout, 30m))')
    print(f'REALMOD-VERDICT module={module} packages-in-log={len(seen)} pass={npass} known={nknown} fail={nfail} external={next_}'
          + (' (EXTERNAL is never a pass: each one is OWED an isolated re-read at the landing head)' if next_ else ''))
    return 1 if nfail else 0

# ================================================================================================ verify round 1 additions
# ---------------------------------------------------------------------------------------------------------------- hostwall
def hostwall(t0, path, *more):
    # TRAIN L's template lesson (tL-seats-draft.txt:40): compare the converted HOST's own package elapsed, not the row
    # wall (mostly MSBuild and publish). go2cs_test_results.json is the C# host's event record; its package-level
    # terminal events (test "") carry the host's elapsed seconds. A host that leaves through its exit path appends a
    # second one with elapsed 0, so the reading is the LARGEST (read at L's three kept records: runtime/pprof fail
    # 377.01 beside a 627 s leg; runtime fail 5647.68 then fail 0 beside 5891 s; the TestTracebackSystem filter fail
    # 6.41 then fail 0 beside 317 s). One HOSTWALL line: this row's, fresh against the leg's start, and its baseline.
    # A reading, never a gate; each S row's is the next train's baseline.
    # Q (Q11; the audit's carry item, COORD's notes of 2026-10-04 18:28): the S ROWS HAVE A BASELINE NOW. The previous
    # train's battery stamped one HOSTWALL line per S row in its own SUMMARY and kept no per-row record, so the
    # baseline of an S row is READ FROM THAT STAMP: --prev-summary <SUMMARY.txt> --leg 'S:<pkg>' takes the LAST
    # 'HOSTWALL cs-host=<n>s (<action>)' of that leg (P's run2 holds 98). A T leg still gives the kept record of the
    # same leg as a third positional argument. The line ends ' SLOWER' when the host took at least 1.5 times the
    # baseline AND at least 30 s more (the audit's case: os/exec 144 s -> 234 s at an unchanged count): a prompt to
    # read, counted by the battery at END, never a finding.
    lpath = psum = leg = None
    i = 0
    while i < len(more):
        if more[i] == '--prev-summary': psum = more[i + 1]; i += 2
        elif more[i] == '--leg': leg = more[i + 1]; i += 2
        else: lpath = more[i]; i += 1
    def el(p):
        if not p or not os.path.exists(p): return None
        try:
            with open(p, encoding='utf-8-sig') as f: ev = json.load(f).get('events') or []
        except Exception: return None
        pk = [(e.get('action'), float(e.get('elapsed') or 0)) for e in ev
              if isinstance(e, dict) and not e.get('test') and e.get('action') in ('pass', 'fail', 'skip', 'timeout')]
        return (pk[-1][0], max(x[1] for x in pk)) if pk else None
    prev = None
    if psum and leg and os.path.exists(psum):
        rx = re.compile(r'\s' + re.escape(leg) + r':\s.*HOSTWALL cs-host=(\d+)s \((\w+)\)')
        for line in open(psum, encoding='utf-8', errors='replace'):
            m = rx.search(line)
            if m: prev = (m.group(2), float(m.group(1)))
    if not os.path.exists(path): print('HOSTWALL no results record'); return 0
    if os.stat(path).st_mtime <= float(t0): print('HOSTWALL results record STALE (older than the leg): not read'); return 0
    a, b = el(path), el(lpath) or prev
    if not a: print('HOSTWALL no package-level terminal event in the record'); return 0
    tag = ''
    if b:
        r = a[1] / max(b[1], 1.0)
        tag = f' P={b[1]:.0f}s ({b[0]}) x{r:.2f}' + (' SLOWER' if r >= 1.5 and a[1] - b[1] >= 30 else '')
    elif psum:
        tag = ' P=none (the previous train stamped no HOSTWALL for this leg)'
    print(f'HOSTWALL cs-host={a[1]:.0f}s ({a[0]}){tag}')   # Q: the baseline is TRAIN P's (a kept record, or its SUMMARY stamp)
    return 0

# ---------------------------------------------------------------------------------------------------------------- live
def live(own_lock, wait=20.0):
    # Floor 1 ("never let two conversions overlap on one box") beyond this train's own lock, which only its own three
    # scripts take: the three readings of the train-assembly skill's LIVE gate. Nothing here kills or changes anything.
    #   1. another train's lock: <coord-scratch>/t<X>/.battery.lock other than <own_lock> (TRAIN L's is tL/.battery.lock);
    #   2. converter and harness processes, twice <wait> s apart (a CNR's go2cs.exe lives for seconds): go2cs.exe,
    #      go2cs.test.exe and BehavioralRunner.exe wherever they run (floor 1 is about the BOX), and -- verify round 2,
    #      COORD's ruling -- a dotnet.exe or a testhost.exe ONLY when its command line or its executable path names the
    #      tQ worktree or a path under the battery's scratch root (never by bare process name: an IDE's or another
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
    # The path conditions, in both slash spellings: the tQ worktree (a sibling of the scratch root) and the scratch root.
    top = os.path.dirname(root)
    frags = []
    for p in (os.path.join(top, 'tQ') + os.sep, root + os.sep):
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
    # tQ-battery.sh reads it at the base and at HEAD: a row annotated at HEAD must read tiered=True in its own record,
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
    # of 225 rows carry one at aa0a07d5fd), else its banked columns 2 and 3. tQ-linux-legs.sh compares each row's
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

# ================================================================================================ TRAIN N additions (carried at O and P)
# ---------------------------------------------------------------------------------------------------------------- cnrexpect
def cnrexpect(argv):
    # Item N2: CNR's package count, DERIVED, never typed. check-no-regression.ps1 (8f46a9adae, its step 2 and F8) transpiles
    # every directory under src/tests/Behavioral that holds a *.go file directly (recursive, bin/obj excluded), and its N
    # is enumerated-minus-skipped: a package whose package_info.cs carries a line-anchored [GoPlatformExclusive("goos",
    # ...)] without this host's goos, or [GoArchExclusive(...)] without its goarch, is skipped BY NAME. This reader applies
    # the same predicate to the TREE at <ref> (git only; PowerShell's -match is case-insensitive, so the attribute line is
    # matched without case). Control read at TRAIN M's landed master 8f46a9adae: TRAIN M's run3 CNR printed N=785 with six
    # platform-exclusive skips (MulticastGroupJoin, ScmRightsSeam, SendtoSeam, SetegidBroadcastSeam, UnixAbstractAddrName,
    # WritevIovecSeam): a reading of this function that differs at that ref is a defect in the reader.
    #   cnrexpect <repo> [<ref>] [--base <ref>] [--goos windows] [--goarch amd64]
    # With --base, the package directories the tree at <ref> adds or removes against <base> are listed too (CNRDELTA).
    pos, opt, i = [], {'--goos': 'windows', '--goarch': 'amd64', '--base': None}, 0
    while i < len(argv):
        if argv[i] in opt: opt[argv[i]] = argv[i + 1]; i += 2
        else: pos.append(argv[i]); i += 1
    repo, ref = pos[0], (pos[1] if len(pos) > 1 else 'HEAD')
    beh = 'src/tests/Behavioral/'
    def dirs_at(r):
        ds = set()
        for x in git(repo, '-c', 'core.quotepath=false', 'ls-tree', '-r', '--name-only', '-z', r, '--', beh.rstrip('/')).split('\0'):
            if not x.endswith('.go') or not x.startswith(beh): continue
            d = x.rsplit('/', 1)[0]
            if re.search(r'/(bin|obj)(/|$)', d[len(beh):]): continue
            if d + '/' == beh: continue   # a *.go directly in the behavioral root is not a package directory
            ds.add(d)
        return ds
    # One git grep for the two attributes over every behavioral package_info.cs at <ref> (one call, not one per
    # directory); the FIRST matching line of each attribute in a file is the one CNR reads.
    attrs = {}
    gg = git(repo, '-c', 'core.quotepath=false', 'grep', '-i', '-E', r'^[[:space:]]*\[(GoPlatformExclusive|GoArchExclusive)\(',
             ref, '--', 'src/tests/Behavioral/*package_info.cs', check=False)
    for line in gg.replace('\r', '').split('\n'):
        if not line.startswith(ref + ':'): continue
        f, _, content = line[len(ref) + 1:].partition(':')
        if not f.endswith('/package_info.cs'): continue
        kind = 'GoArchExclusive' if re.match(r'^\s*\[GoArchExclusive\(', content, re.I) else 'GoPlatformExclusive'
        attrs.setdefault((f.rsplit('/', 1)[0], kind), re.findall(r'"([^"]+)"', content))
    def excl(r, d, attr):
        return attrs.get((d, attr), [])
    ds = dirs_at(ref)
    skip = []
    for d in sorted(ds):
        pl, ar = excl(ref, d, 'GoPlatformExclusive'), excl(ref, d, 'GoArchExclusive')
        if (pl and opt['--goos'] not in pl) or (ar and opt['--goarch'] not in ar):
            skip.append((d[len(beh):], ', '.join(pl + ar)))
    for d, why in skip: print(f'CNRSKIP {d} [{why}]')
    if opt['--base']:
        b = dirs_at(opt['--base'])
        add = sorted(x[len(beh):] for x in ds - b); rem = sorted(x[len(beh):] for x in b - ds)
        print(f'CNRDELTA base={opt["--base"]} added={len(add)} [{" ".join(add)}] removed={len(rem)} [{" ".join(rem)}]')
    print(f'CNREXPECT ref={ref} goos={opt["--goos"]} goarch={opt["--goarch"]} enumerated={len(ds)} skipped={len(skip)} n={len(ds) - len(skip)}')
    return 0 if ds else 1

# ---------------------------------------------------------------------------------------------------------------- uflinux
def uflinux(repo):
    # Item N9b: the linux lanes' UF census. A -tests run on linux writes the package's LINUX test sources, and the
    # committed test corpus is the WINDOWS record, so a test file that only the linux build holds is emitted as an
    # UNTRACKED file (P1 at TRAIN M, 2026-10-02 18:58Z: 17 under os/ and os/exec/, '*_unix_test.cs and similar', the same
    # kind at the control aa0a07d5fd; P2 23:45Z: 21, among them runtime/pprof/rusage_test.cs). That class is carved out
    # BY NAME: an untracked src/core/<pkg>/<name>_test.cs whose <name>_test.go is a test file of <pkg> under GOOS=linux
    # (go list TestGoFiles + XTestGoFiles) and NOT one under GOOS=windows. Every other untracked path stays a MOVER for
    # the driver (exit 0 here when the census was read; 2 when go list could not be read: then nothing is carved).
    out = git(repo, '-c', 'core.quotepath=false', 'ls-files', '--others', '--exclude-standard', '-z', '--', 'src/core')
    paths = sorted(x for x in out.split('\0') if x)
    cand = {}
    for p in paths:
        m = re.match(r'^src/core/(.+)/([^/]+)_test\.cs$', p)
        if m: cand.setdefault(m.group(1), []).append((p, m.group(2) + '_test.go'))
    sets, ok = {}, True
    if cand:
        fmt = '{{.ImportPath}}|{{join .TestGoFiles ","}}|{{join .XTestGoFiles ","}}'
        for goos in ('linux', 'windows'):
            env = dict(os.environ, GOOS=goos, GOARCH='amd64', CGO_ENABLED='0', GOFLAGS='')
            r = subprocess.run(['go', 'list', '-e', '-f', fmt] + sorted(cand), capture_output=True, text=True, env=env,
                               cwd=os.environ.get('TMPDIR') or os.environ.get('TEMP') or None)
            if r.returncode != 0 and not r.stdout.strip(): ok = False; break
            for line in r.stdout.splitlines():
                f = line.split('|')
                if len(f) == 3: sets[(goos, f[0])] = set(x for x in (f[1] + ',' + f[2]).split(',') if x)
    carved, other = [], []
    for p in paths:
        m = re.match(r'^src/core/(.+)/([^/]+)_test\.cs$', p)
        if ok and m:
            pkg, gf = m.group(1), m.group(2) + '_test.go'
            if gf in sets.get(('linux', pkg), set()) and gf not in sets.get(('windows', pkg), set()):
                carved.append(p); print(f'UFLINUX carved {p} ({pkg}: {gf} is a GOOS=linux test file and not a GOOS=windows one)'); continue
        other.append(p); print(f'UFLINUX other {p}')
    print(f'UFLINUX untracked={len(paths)} linux-only-test-sources={len(carved)} other={len(other)} go-list={"ok" if ok else "FAILED (nothing carved)"}')
    return 0 if ok else 2

# ---------------------------------------------------------------------------------------------------------------- testsrc-refresh
# Item N10 / tQ-README.md MS13: the corpus-wide refresh of committed -tests sources, a POST-BATTERY BANK STEP. Its input
# is the union of the WINDOWS rewrite patches N's own -tests runs saved (the i7 battery's S-rewrites.patch and
# T-rewrites.patch, the i9 shard's tracked-changes.patch; their -U0 twins are accepted). Its class is committed TEST
# sources under src/core and nothing else: a *_test.cs, a package_test_info.cs, a package_info_internal_test.cs, that is
# tracked, not hand-owned (no line-anchored [module: GoManualConversion]) and not under src/core/golib. Refused and
# listed by category, never written: production files (package_init.cs is the -tests hook's rewrite, not a target;
# go2cs_test_host.cs), proof pages (docs/validation: host-conditioned), csproj, review siblings (*.cs.auto), anything
# else; and, inside the class, a block that creates, deletes, renames or binary-patches a file. Two inputs that rewrite
# ONE file differently are a CONFLICT (two windows re-emissions disagree: refused, exit 1). Per file: the hunk classes
# (G-FRAME and MAP as teattr reads them; ALIAS = fixup-2's using-alias lines, 8f46a9adae; SLICE = the older '.slice'
# spelling of a range index; OTHER = read it).
#   testsrc-refresh <repo> --out <filtered.patch> <patch> [<patch> ...]   build the filtered patch (nothing is applied)
#   testsrc-refresh <repo> --check-worktree                              after `git apply`: every changed path is in the class
ALIAS_USING = re.compile(r'^using [A-Za-z_][A-Za-z0-9_]* = global::[A-Za-z0-9_.]+;$')
SLICE_OLD = re.compile(r'\[[^\]]*\.\.[^\]]*\]')
def ts_class(rem, add):
    c, n, u, m, p = te_class(rem, add)
    if c != 'OTHER': return c
    r = [x.strip() for x in rem if x.strip()]; a = [x.strip() for x in add if x.strip()]
    if a and all(ALIAS_USING.match(x) for x in a) and all(x == USING_RT for x in r): return 'ALIAS'
    # the older '.slice' spelling class (TRAIN M's README M13; read in TRAIN M's run3 patch): a range index 'x[(int)(i)..]'
    # re-emitted as 'x.slice(i)', line for line
    if r and len(r) == len(a) and all(SLICE_OLD.search(x) for x in r) and all('.slice(' in x for x in a): return 'SLICE'
    return 'OTHER'

def ts_inclass(path, handown):
    if not path.startswith('src/core/') or path.startswith('src/core/golib/'): return False
    b = path.rsplit('/', 1)[-1]
    return (b.endswith('_test.cs') or b in ('package_test_info.cs', 'package_info_internal_test.cs')) and path not in handown

def ts_category(path):
    if path.startswith('docs/validation/'): return 'proof-page'
    if path.endswith('.csproj'): return 'csproj'
    if path.endswith('.cs.auto'): return 'review-sibling'
    if path.startswith('src/core/') and path.endswith('.cs'): return 'production'
    return 'other'

def ts_unq(x):
    x = x.strip()
    if x.startswith('"') and x.endswith('"'):
        x = x[1:-1].encode('latin-1', 'backslashreplace').decode('unicode_escape').encode('latin-1').decode('utf-8', 'surrogateescape')
    return x

def ts_blocks(pf):
    # -> [(path, raw bytes of the block, shape problems, U0?)]; the bytes are kept exactly (CRLF content survives)
    data = open(pf, 'rb').read()
    parts = re.split(rb'(?m)^(?=diff --git )', data)
    out = []
    for blk in parts:
        if not blk.startswith(b'diff --git '): continue
        txt = blk.decode('utf-8', 'surrogateescape')
        first = txt.split('\n', 1)[0].rstrip('\r')
        path, shape = None, []
        for line in txt.split('\n'):
            if line.startswith('+++ '):
                v = line[4:].rstrip('\r')
                if v.strip() != '/dev/null': path = ts_unq(v)[2:]
                break
            if line.startswith(('new file mode', 'deleted file mode', 'rename from', 'copy from')): shape.append(line.split(' mode')[0].split(' from')[0])
            if line.startswith(('GIT binary patch', 'Binary files ')): shape.append('binary')
        if path is None:
            rest = first[len('diff --git '):]
            toks = re.findall(r'"((?:[^"\\]|\\.)*)"', rest) if rest.startswith('"') else []
            path = ts_unq('"' + toks[-1] + '"')[2:] if toks else rest.rsplit(' b/', 1)[-1]
            shape = shape or ['no +++ line']
        u0 = not any(l.startswith(' ') for l in txt.split('\n') if not l.startswith(('diff --git', 'index ', '--- ', '+++ ', '@@')))
        out.append((path, blk, shape, u0))
    return out

def ts_changes(blk):
    # the file's removed and added lines, CR-stripped, as two sorted tuples: the change itself, whatever the context width
    rem, add, inh = [], [], False
    for line in blk.decode('utf-8', 'surrogateescape').split('\n'):
        if line.startswith('@@'): inh = True; continue
        if not inh or line.startswith(('--- ', '+++ ')): continue
        if line.startswith('-'): rem.append(line[1:].rstrip('\r'))
        elif line.startswith('+'): add.append(line[1:].rstrip('\r'))
    return tuple(sorted(rem)), tuple(sorted(add))

def ts_report(path, blk):
    import tempfile
    fd, tmp = tempfile.mkstemp(suffix='.patch'); os.write(fd, blk); os.close(fd)
    try: hs = te_hunks(tmp)
    finally: os.remove(tmp)
    cnt = {'G-FRAME': 0, 'MAP': 0, 'N-PARTIAL': 0, 'ALIAS': 0, 'SLICE': 0, 'OTHER': 0}; first = ''
    for h in hs:
        c = ts_class(h[1], h[2]); cnt[c] += 1
        if c == 'OTHER' and not first: first = f' first-other: -[{(h[1][0].strip() if h[1] else "")[:80]}] +[{(h[2][0].strip() if h[2] else "")[:80]}]'
    return cnt, len(hs), first

def testsrc_refresh(argv):
    out, check, pats, repo, i = None, False, [], None, 0
    while i < len(argv):
        if argv[i] == '--out': out = argv[i + 1]; i += 2
        elif argv[i] == '--check-worktree': check = True; i += 1
        elif repo is None: repo = argv[i]; i += 1
        else: pats.append(argv[i]); i += 1
    hl = git(repo, 'grep', '-lE', r'^\[module: (go\.)?GoManualConversion\]', 'HEAD', '--', 'src/core', check=False)
    handown = {x.split(':', 1)[1] for x in hl.split('\n') if ':' in x}
    if check:
        ch = [x for x in git(repo, '-c', 'core.quotepath=false', 'diff', '--name-only', '-z', 'HEAD').split('\0') if x]
        un = [x for x in git(repo, '-c', 'core.quotepath=false', 'ls-files', '--others', '--exclude-standard', '-z', '--', 'src/core').split('\0') if x]
        bad = [f'{x} ({ts_category(x) if not ts_inclass(x, handown) else "?"})' for x in ch if not ts_inclass(x, handown)] + [f'{x} (untracked)' for x in un]
        tot = {'G-FRAME': 0, 'MAP': 0, 'N-PARTIAL': 0, 'ALIAS': 0, 'SLICE': 0, 'OTHER': 0}
        for x in ch:
            if not ts_inclass(x, handown): continue
            blk = subprocess.run(['git', '-C', repo, 'diff', '-U0', 'HEAD', '--', x], capture_output=True).stdout
            cnt, nh, first = ts_report(x, blk)
            for k in tot: tot[k] += cnt[k]
            print(f'REFRESH-WT {x}: hunks={nh} g-frame={cnt["G-FRAME"]} map={cnt["MAP"]} alias={cnt["ALIAS"]} slice={cnt["SLICE"]} other={cnt["OTHER"]} n-partial={cnt["N-PARTIAL"]}{first}')
        for b in bad: print(f'REFRESH-WT REFUSED {b}')
        print(f'REFRESH-WT-VERDICT changed={len(ch)} in-class={sum(1 for x in ch if ts_inclass(x, handown))} refused={len(bad)} '
              f'hunks g-frame={tot["G-FRAME"]} map={tot["MAP"]} alias={tot["ALIAS"]} slice={tot["SLICE"]} other={tot["OTHER"]} n-partial={tot["N-PARTIAL"]} :: '
              + ('ONLY committed test sources changed: commit them signed as the refresh' if not bad else 'a path outside the class changed: restore it before any commit'))
        return 1 if bad else 0
    if not out or not pats:
        print('REFRESH usage: testsrc-refresh <repo> --out <filtered.patch> <patch> [<patch> ...] | <repo> --check-worktree'); return 2
    keep, excl, refused, conflicts, src = {}, {}, [], [], {}
    for pf in pats:
        if not os.path.exists(pf): print(f'REFRESH input absent: {pf}'); return 2
        for path, blk, shape, u0 in ts_blocks(pf):
            if not ts_inclass(path, handown):
                cat = 'hand-owned' if path in handown else 'golib' if path.startswith('src/core/golib/') else ts_category(path)
                excl.setdefault(cat, set()).add(path); continue
            if shape: refused.append(f'{path} ({", ".join(shape)} in {os.path.basename(pf)})'); continue
            ch = ts_changes(blk)
            src.setdefault(path, []).append(os.path.basename(pf))
            if path in keep:
                if keep[path][1] != ch: conflicts.append(f'{path} ({" vs ".join(src[path])})'); continue
                if keep[path][2] and not u0: keep[path] = (blk, ch, u0)   # prefer a full-context block over a -U0 one
            else:
                keep[path] = (blk, ch, u0)
    for c in conflicts: keep.pop(c.split(' (')[0], None)
    tot = {'G-FRAME': 0, 'MAP': 0, 'N-PARTIAL': 0, 'ALIAS': 0, 'SLICE': 0, 'OTHER': 0}; nh = 0; anyu0 = False
    with open(out, 'wb') as fh:
        for path in sorted(keep):
            blk, ch, u0 = keep[path]; anyu0 = anyu0 or u0
            fh.write(blk if blk.endswith(b'\n') else blk + b'\n')
            cnt, n, first = ts_report(path, blk); nh += n
            for k in tot: tot[k] += cnt[k]
            print(f'REFRESH {path}: from={",".join(src[path])} hunks={n} g-frame={cnt["G-FRAME"]} map={cnt["MAP"]} alias={cnt["ALIAS"]} slice={cnt["SLICE"]} other={cnt["OTHER"]} n-partial={cnt["N-PARTIAL"]}{" U0" if u0 else ""}{first}')
    for cat in sorted(excl):
        xs = sorted(excl[cat])
        print(f'REFRESH EXCLUDED {cat}: {len(xs)} :: ' + ' '.join(xs[:12]) + (f' ... and {len(xs) - 12} more' if len(xs) > 12 else ''))
    for r in refused: print(f'REFRESH REFUSED {r}')
    for c in conflicts: print(f'REFRESH CONFLICT {c}: two windows re-emissions rewrite this file differently')
    print(f'REFRESH-VERDICT files={len(keep)} hunks={nh} g-frame={tot["G-FRAME"]} map={tot["MAP"]} alias={tot["ALIAS"]} slice={tot["SLICE"]} other={tot["OTHER"]} n-partial={tot["N-PARTIAL"]} '
          f'excluded={sum(len(v) for v in excl.values())} refused={len(refused)} conflicts={len(conflicts)} out={out} :: apply with '
          f'git apply --check{" --unidiff-zero" if anyu0 else ""} then git apply{" --unidiff-zero" if anyu0 else ""}; then testsrc-refresh --check-worktree')
    return 1 if refused or conflicts else 0

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
          'rosterlinux': lambda: rosterlinux(*rest), 'cnrexpect': lambda: cnrexpect(rest), 'uflinux': lambda: uflinux(*rest),
          'testsrc-refresh': lambda: testsrc_refresh(rest), 'csprojtemplate': lambda: csprojtemplate(*rest)}[cmd]()
    sys.exit(rc or 0)
