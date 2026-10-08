# tFL-resolve.py -- the RULED resolutions of TRAIN FL's assembly and map: 'keep both', in merge order or (FL7) in the
# ordinal order of the hunk's lines, and nothing else. DERIVED 2026-10-08 from trainQ/tQ-resolve.py. FL7: COORD
# assembled FL's first ten rows BY HAND and resolved row 8's src/go2cs/go2cs-src.projitems add-against-add 'kept both
# in alphabetical order' (ledger 04:11 on 2026-10-08): the row's pointerReceiverMark_test.go line ABOVE the union's
# positionCalls_test.go. append-both writes the union's line first and would give a tree the hand union does not hold;
# the kind append-both-sorted writes both sides' lines of each hunk sorted (case-insensitive, stripped), which is that
# resolution. Every assert below holds for both kinds. Q's header follows.
# (Q) tQ-resolve.py -- the RULED resolutions of TRAIN Q's assembly: 'keep both in merge order', and nothing else.
#
#   python -B tFL-resolve.py <worktree> <ruled file> <row ref> <row sha, full> [--dry]
#
# Called by tFL-conflict-map.sh and tFL-assemble.sh, in a worktree whose merge of <row sha> has just stopped on a
# conflict (MERGE_HEAD present). The mechanism is TRAIN K's (trainK/tK-resolve.py + tK-assemble.sh: a resolver keyed by
# the seat, with the expected conflict set, that exits non-zero when an assumption does not hold; K's kind K9 was this
# one: an adjacent insert against an adjacent insert, both sides kept). Trains L to P had NO resolution: any conflict
# aborted and the lane re-cut (trainP/tP-assemble.sh: 'UNRULED conflict', exit 3). Q's rehearsal (trainQ/tQ-map.md)
# read conflicts that are RULED 'keep both in merge order' in named rows and named files, so K's mechanism returns,
# generalised from three hard-coded seats to a table (tFL-ruled.txt) and from a text search to the index's three stages.
#
# What it does, for EVERY unmerged path of the merge in progress, in two phases (nothing is written unless every path
# passed phase 1):
#   1. the path must be RULED for this row: a line 'ref|sha10|path|append-both|...' of the ruled file whose ref is the
#      row's and whose sha10 is the first ten characters of the row's sha (a re-cut under the same name does NOT
#      inherit a ruling). Anything else is UNRULED: the row and every hunk are named, exit 3.
#   2. the three index stages must all exist (a both-added or a deleted-against-modified path is not this class).
#   3. the three blobs are merged by `git merge-file -p --diff3` (trainQ/tQ-map.md section B: the DEFAULT conflict style
#      and --union both refine the hunk and drop the lines the two sides share, so a registration list loses a
#      [TestMethod] line and the BOARD loses nine lines; the diff3 form does not). The markers are 23 characters wide and
#      no input line may start with one, so a marker is never confused with content.
#   4. every hunk must have an EMPTY BASE part (an insert against an insert at one point) and two non-empty sides. A
#      hunk in which both sides edit existing lines is NOT 'append against append': named, exit 3.
#   5. resolved text = the union's lines so far (ours), then the row's (theirs): merge order.
#   6. asserts on the result: lines(result) = lines(ours) + lines(theirs) - lines(base) (no line was dropped or doubled
#      by the resolution); the hunk count the ruling names; and by file kind: the BOARD keeps ONE raw opener and ONE
#      endraw closer, the closer the FINAL line; a BehavioralTests list holds as many [TestMethod] lines as Check
#      methods; go2cs.slnx registers no project twice.
# Phase 2 writes each result as a blob (git hash-object -w --no-filters), sets it as the path's stage 0 with the
# union side's mode (git update-index --cacheinfo) and lets git write the worktree file (git checkout-index -f), so the
# checkout's own line-ending rules apply (the tree's .gitattributes pin eol=crlf on .cs and .slnx).
#
# --dry stops after phase 1 (the controls). Exit: 0 every unmerged path resolved (or would be); 3 a refusal, nothing
# written; 2 usage or a git failure.
import os, subprocess, sys, tempfile

MARK = 23
KIND = 'append-both'
KINDS = ('append-both', 'append-both-sorted')   # FL7
BOARD = 'docs/phase4/BOARD-next-validation-candidates.md'
LISTS = 'src/tests/Behavioral/BehavioralTests/'
SLNX = 'src/go2cs.slnx'

def run(wt, *a, data=None):
    r = subprocess.run(['git', '-C', wt] + list(a), input=data, capture_output=True)
    return r.returncode, r.stdout, r.stderr

def out(wt, *a):
    rc, o, e = run(wt, *a)
    if rc != 0:
        print(f'RESOLVE-ERROR git {" ".join(a)} rc={rc}: {e.decode("utf-8", "replace").strip()[:300]}'); sys.exit(2)
    return o

def ruled(path_file, ref, sha):
    r = {}
    if not os.path.exists(path_file): return r
    for line in open(path_file, encoding='utf-8'):
        line = line.rstrip('\r\n')
        if not line.strip() or line.lstrip().startswith('#'): continue
        f = line.split('|')
        if len(f) < 4:
            print(f'RESOLVE-ERROR malformed ruled line (want ref|sha10|path|kind[|hunks=N][|note]): {line[:160]}'); sys.exit(2)
        if f[0].strip() != ref or not sha.startswith(f[1].strip()) or len(f[1].strip()) < 10: continue
        want = 1
        for x in f[4:]:
            if x.strip().startswith('hunks='): want = int(x.strip()[6:])
        r[f[2].strip()] = (f[3].strip(), want, '|'.join(f[4:]).strip())
    return r

def lines(b):
    return b.splitlines(keepends=True)

def starts_marker(l):
    return any(l.startswith(c * MARK) for c in (b'<', b'|', b'=', b'>'))

def merge3(wt, path, stages, tmp):
    names = {}
    for st in (1, 2, 3):
        p = os.path.join(tmp, f's{st}')
        with open(p, 'wb') as f: f.write(out(wt, 'cat-file', 'blob', stages[st][1]))
        names[st] = p
    blobs = {st: open(names[st], 'rb').read() for st in names}
    for st in blobs:
        if any(starts_marker(l) for l in lines(blobs[st])):
            return None, blobs, f'stage {st} holds a line that starts with {MARK} marker characters: the hunk reader cannot tell it from a marker'
    r = subprocess.run(['git', 'merge-file', '-p', '--diff3', f'--marker-size={MARK}', '-L', 'ours', '-L', 'base', '-L', 'theirs',
                        names[2], names[1], names[3]], capture_output=True)
    if r.returncode < 0 or r.returncode >= 128:
        return None, blobs, f'git merge-file rc={r.returncode}: {r.stderr.decode("utf-8", "replace").strip()[:200]}'
    return r.stdout, blobs, None

def hunks(merged, kind=KIND):
    # -> (resolved bytes, [(ours lines, base lines, theirs lines)]); the parts are lists of byte lines
    # FL7: kind 'append-both-sorted' writes each hunk's two sides sorted (stripped, case-insensitive; stable)
    res, hs, st, o, b, t = [], [], 0, [], [], []
    for l in lines(merged):
        if st == 0:
            if l.startswith(b'<' * MARK + b' '): st, o, b, t = 1, [], [], []
            else: res.append(l)
        elif st == 1:
            if l.startswith(b'|' * MARK + b' '): st = 2
            elif l.rstrip(b'\r\n') == b'=' * MARK: st = 3
            else: o.append(l)
        elif st == 2:
            if l.rstrip(b'\r\n') == b'=' * MARK: st = 3
            else: b.append(l)
        else:
            if l.startswith(b'>' * MARK + b' '):
                hs.append((o, b, t)); st = 0
                res += (o + t) if kind != 'append-both-sorted' else sorted(o + t, key=lambda l: l.decode('utf-8', 'replace').strip().lower())
            else: t.append(l)
    if st != 0: return None, hs
    return b''.join(res), hs

def first(ls):
    return (ls[0].decode('utf-8', 'replace').strip()[:70] if ls else '')

def checks(path, result, blobs, nh, want):
    bad = []
    n = lambda x: len(lines(x))
    if n(result) != n(blobs[2]) + n(blobs[3]) - n(blobs[1]):
        bad.append(f'lines {n(result)} != ours {n(blobs[2])} + theirs {n(blobs[3])} - base {n(blobs[1])}: a line was dropped or doubled')
    if nh != want: bad.append(f'{nh} hunk(s), the ruling names {want}')
    if result and not result.endswith(b'\n') and (blobs[2].endswith(b'\n') or blobs[3].endswith(b'\n')):
        bad.append('the result lost its final newline')
    rl = lines(result)
    if path == BOARD:
        op = [i for i, l in enumerate(rl) if l.startswith(b'<!-- {% raw %}')]
        cl = [i for i, l in enumerate(rl) if l.startswith(b'<!-- {% endraw %}')]
        if len(op) != 1 or len(cl) != 1 or cl[0] != len(rl) - 1:
            bad.append(f'the BOARD guard: {len(op)} raw opener line(s), {len(cl)} endraw closer line(s), closer at line {cl[0] + 1 if cl else "none"} of {len(rl)} (want 1, 1, the FINAL line)')
    elif path.startswith(LISTS) and path.endswith('Tests.cs'):
        tm = sum(1 for l in rl if l.strip() == b'[TestMethod]')
        ck = sum(1 for l in rl if l.lstrip().startswith(b'public void Check') and b'(' in l)
        if tm != ck: bad.append(f'{tm} [TestMethod] line(s) for {ck} Check method(s): an attribute line was lost or doubled')
        names = [l.split(b'public void ', 1)[1].split(b'(', 1)[0] for l in rl if l.lstrip().startswith(b'public void Check') and b'(' in l]
        dup = sorted({x.decode() for x in names if names.count(x) > 1})
        if dup: bad.append(f'a Check method registered twice: {" ".join(dup)[:200]}')
    elif path == SLNX:
        pr = [l.split(b'Path="', 1)[1].split(b'"', 1)[0] for l in rl if b'<Project Path="' in l]
        dup = sorted({x.decode() for x in pr if pr.count(x) > 1})
        if dup: bad.append(f'a project registered twice: {" ".join(dup)[:200]}')
        pc = lambda x: sum(1 for l in lines(x) if b'<Project Path="' in l)
        if len(pr) != pc(blobs[2]) + pc(blobs[3]) - pc(blobs[1]):
            bad.append(f'Project lines {len(pr)} != ours {pc(blobs[2])} + theirs {pc(blobs[3])} - base {pc(blobs[1])}')
    return bad

def main(argv):
    dry = '--dry' in argv
    argv = [a for a in argv if a != '--dry']
    if len(argv) != 4:
        print('RESOLVE usage: tFL-resolve.py <worktree> <ruled file> <row ref> <row sha, full> [--dry]'); return 2
    wt, rf, ref, sha = argv
    rc, _, _ = run(wt, 'rev-parse', '-q', '--verify', 'MERGE_HEAD')
    if rc != 0:
        print('RESOLVE-ERROR no merge is in progress in the worktree (no MERGE_HEAD)'); return 2
    mh = out(wt, 'rev-parse', 'MERGE_HEAD').decode().strip()
    if mh != sha:
        print(f'RESOLVE-ERROR the merge in progress is of {mh[:10]}, not of the row sha {sha[:10]}'); return 2
    un = [p for p in out(wt, '-c', 'core.quotepath=false', 'diff', '--name-only', '--diff-filter=U', '-z').decode('utf-8', 'surrogateescape').split('\0') if p]
    if not un:
        print('RESOLVE-ERROR the merge in progress has no unmerged path'); return 2
    rules = ruled(rf, ref, sha)
    plan, refused = [], 0
    with tempfile.TemporaryDirectory(prefix='tFL-resolve-') as tmp:
        for path in un:
            stages = {}
            for rec in out(wt, 'ls-files', '-u', '-z', '--', path).decode('utf-8', 'surrogateescape').split('\0'):
                if not rec: continue
                meta, _ = rec.split('\t', 1)
                mode, bsha, st = meta.split()
                stages[int(st)] = (mode, bsha)
            if sorted(stages) != [1, 2, 3]:
                print(f'UNRULED row={ref} path={path}: index stages {sorted(stages)} (a both-added or a deleted-against-modified path is never "append against append")'); refused += 1; continue
            merged, blobs, err = merge3(wt, path, stages, tmp)
            if err:
                print(f'UNRULED row={ref} path={path}: {err}'); refused += 1; continue
            result, hs = hunks(merged)
            desc = '; '.join(f'hunk {i + 1}: ours {len(o)} / base {len(b)} / theirs {len(t)} line(s), ours starts [{first(o)}], theirs starts [{first(t)}]' for i, (o, b, t) in enumerate(hs))
            if result is None:
                print(f'UNRULED row={ref} path={path}: the diff3 form did not parse (an unterminated hunk)'); refused += 1; continue
            if path not in rules:
                print(f'UNRULED row={ref} ({sha[:10]}) path={path}: no ruling in {os.path.basename(rf)} names this row and this file :: {len(hs)} hunk(s): {desc}'); refused += 1; continue
            kind, want, note = rules[path]
            if kind not in KINDS:
                print(f'UNRULED row={ref} path={path}: the ruling\'s kind is "{kind}", this resolver knows only {" and ".join(KINDS)}'); refused += 1; continue
            if kind != KIND: result, hs = hunks(merged, kind)   # FL7: the same hunks, the sorted order
            shape = [i + 1 for i, (o, b, t) in enumerate(hs) if b or not o or not t]
            if not hs or shape:
                print(f'UNRULED row={ref} path={path}: RULED {kind}, and hunk(s) {shape or "none found"} are not an insert against an insert (the base part must be EMPTY and both sides non-empty) :: {desc}'); refused += 1; continue
            bad = checks(path, result, blobs, len(hs), want)
            if bad:
                print(f'UNRULED row={ref} path={path}: RULED {kind}, and the resolved text fails: {" | ".join(bad)} :: {desc}'); refused += 1; continue
            plan.append((path, stages[2][0], result, hs, note, kind))
        if refused:
            print(f'RESOLVE-VERDICT row={ref} unmerged={len(un)} refused={refused} resolved=0 (NOTHING was written: the merge is as git left it)'); return 3
        for path, mode, result, hs, note, kind in plan:
            o = sum(len(h[0]) for h in hs); t = sum(len(h[2]) for h in hs)
            print(f'RESOLVED row={ref} path={path} kind={kind} hunks={len(hs)} ours={o} theirs={t} line(s) kept, {"ours first" if kind == KIND else "sorted"}; result {len(lines(result))} lines{" (dry: not written)" if dry else ""} :: {note[:120]}')
            if dry: continue
            p = os.path.join(tmp, 'result')
            with open(p, 'wb') as f: f.write(result)
            bsha = out(wt, 'hash-object', '-w', '--no-filters', p).decode().strip()
            out(wt, 'update-index', '--cacheinfo', f'{mode},{bsha},{path}')
            out(wt, 'checkout-index', '-f', '--', path)
            got = out(wt, 'rev-parse', f':{path}').decode().strip()
            if got != bsha:
                print(f'RESOLVE-ERROR {path}: the index holds {got[:10]} after the write, not {bsha[:10]}'); return 2
    left = [p for p in out(wt, 'diff', '--name-only', '--diff-filter=U', '-z').decode('utf-8', 'surrogateescape').split('\0') if p]
    if left and not dry:
        print(f'RESOLVE-ERROR unmerged paths remain after the writes: {" ".join(left)[:300]}'); return 2
    print(f'RESOLVE-VERDICT row={ref} unmerged={len(un)} refused=0 resolved={len(plan)}{" (dry)" if dry else ""}')
    return 0

if __name__ == '__main__':
    try: sys.stdout.reconfigure(encoding='utf-8', errors='replace')
    except Exception: pass
    sys.exit(main(sys.argv[1:]))
