# coord-derive-train30.py -- derive the train-27 assemble/rehearse/land/land-launch scripts from train 26's with the seat block rewritten.
import io, re, os
NL = chr(10); BS = chr(92)
SP = 'C:/Projects/go2cs/.claude/coord-scripts/'
SEATS = [
    ('C1ERB', 'claude/c1-elemrefbox-native-slice', '810b03087', 'coord-merge-c1-elemrefbox.txt', 'C1: ElemRefBox refuses a native-backed slice as managed backing at all FIVE sites and native elements take ADDRESS identity (the shared-empty-array false identity closed); guard 4 arms, red control 3-of-4'),
    ('C2INC10B', 'claude/c2-darwin-inc10', '5d53a5ad9', 'coord-merge-c2-inc10b.txt', 'C2 darwin INCREMENT 10 (b): the exec seam -- forkExec over posix_spawn, Exec over an unmanaged execve, pipe over a native pair, the Foreground-failure reap; on the seated 10 (a) tip'),
    ('C1REAP', 'claude/c1-exec-foreground-reap', '3af4c88ec', 'coord-merge-c1-reap.txt', 'C1 Q72: the linux exec seam reaps the child killed on the TIOCSPGRP failure path (Go Wait4/EINTR), comment corrected, guard'),
    ('C1RT8', 'claude/c1-runtime-inc8', 'b7a58eda0', 'coord-merge-c1-rt8.txt', 'C1 runtime INCREMENT 8: the native-backed array pointer -- W1 write form over the pointer element (converter) + W2b native read seam (virtual consultation in arrayView); the eight page-allocator rows produce verdicts'),
    ('C1Q58A', 'claude/c1-q58-record-amended', 'a3ee3945c', 'coord-merge-c1-q58-amend.txt', 'C1 Q58 record AMENDED: the increment-8 measurement (88 rows mute -> 88 named refusals; the residual = the converted representation of a fixed-size array VALUE field, a golib/emission model question recorded, not built) -- docs only'),
    ('C2Q44A', 'claude/c2-q44-record-amend', '66a6bdb96', 'coord-merge-c2-q44-amend.txt', 'C2 Q44 record AMENDED: the platform asymmetry (a kernel READ through a token returns EMPTY on Windows where POSIX answers EFAULT; a kernel WRITE FAULTS), train 30s four-row bill, prediction 7 scored WRONG on all three specifics, and section 5s census named as blind to a second door into the same class -- docs only'),
    ('RE2C', 'claude/reflect-embedded-inc-e2b', '3226509d7', 'coord-merge-r-e2c.txt', 'R E2c: getStructMembers path-scoped seenTypes + depth-aware promoted-method rule (gen); the deeper row green'),
    ('GUDP', 'claude/g-wsasendto-seat', '52c01fbb9', 'coord-merge-g-wsasendto.txt', 'G: the WINDOWS DATAGRAM SEND increment -- the sizing record carried in unchanged plus the cut: one registration, one hand-owned body, one synchronous arm, one behavioral guard and one census assertion, with TWO sizing misses scored in a dated section rather than edited into the prediction'),
    ('SUBDOC12', 'claude/sub-doc12', '6779206fc', 'coord-merge-sub-doc12.txt', 'SUB-DOC12: doctrine items 517-605 folded into CLAUDE.md -- 89 items, every one MERGED into an existing passage (zero new sections), seven passages CORRECTED rather than extended, five sub-clauses dropped as duplicates or board material -- docs only'),
    ('C1PPD', 'claude/c1-pprof-push-design', 'f6124065f', 'coord-merge-c1-pprof-design.txt', 'C1: the profile linkname PUSH design with the graph invariant measured FIRST (Gos push direction taken literally is ILLEGAL at 38/36/36 cycles, the pull is 0 on all three) and Gos own directive ARITY splitting the eight onto two registries the converter already has -- plus both board rows dated-corrected -- docs only'),
    ('C1PPP', 'claude/c1-pprof-push', '99c408704', 'coord-merge-c1-pprof-push.txt', 'C1: the profile linkname PUSH cut -- FIVE entries not eight, a measured NULL on both rows, and the finding is that the web rows blocker MOVED one deeper onto the single destination neither registry can serve'),
    ('C1SELF', 'claude/c1-pprof-selfsymbol', 'cf2b9015e', 'coord-merge-c1-selfsymbol.txt', 'C1: the self-symbol widening -- population one, payoff measured at a host that stops dying and eleven verdicts, PLUS the guard hole the widening created and closed: a two-way shape assertion over three shapes would have passed a row the matcher rejects, forwarding nothing silently'),
    ('RUNIQ', 'claude/laneR-unique-liveness', '1bb544a18', 'coord-merge-r-unique-liveness.txt', 'R: uniques FIRST disclosure manifest -- one codegen-liveness entry whose reason carries the six-arm measurement rather than the conclusion, taking the row to 19 matched + 1 disclosed = 20 of 20 at the configuration of record'),
    ('RDENOM', 'claude/laneR-roster-denominators', 'cb04ece1c', 'coord-merge-r-roster-denominators.txt', 'R: the roster row NAMES what each of its two denominators ranges over, plus the five per-file dispositions -- one line, one column, no schema change, guard positive-controlled against the row itself'),

    ("COORDFRONT", "claude/coord-frontier-measured", "e1df777af", "coord-merge-coord-frontier.txt", "board: the three capability-frontier rows measured at the assembly head"),
    ("SUBDOC13", "claude/coord-subdoc13", "0aa24496b", "coord-merge-subdoc13.txt", "CLAUDE.md doctrine batch 13, items 606-672 -- MERGE AFTER SUBDOC12, six shared anchors, re-count numbered sequences"),

    ("COORDUTT", "claude/coord-utt-toolchain-pin", "b45bf6773", "coord-merge-coord-utt-pin.txt", "UpdateTestTargets refuses to mint goldens under an unpinned toolchain"),
]


def rd(p): return io.open(p, encoding='utf-8', newline='').read()
def wr(p, t):
    assert 'coord-train30-' not in p, ('REFUSED: a derive for train 31 must never write a train-30 file (the running train)', p)
    io.open(p, 'w', encoding='utf-8', newline='').write(t)
def strip_seat_vars(t):
    return NL.join(l for l in t.split(NL) if not re.match(r'^[A-Z0-9]+_SHA="\$\{[A-Z0-9]+_SHA:-', l))
def add_worktree_guard(path, wt):
    # WORKTREE GUARD (doctrine 515): a train script without its own cd runs in the caller's cwd.
    t = io.open(path, encoding='utf-8', newline='').read()
    if 'show-toplevel' in t: return
    g = 'cd /c/Projects/go2cs/.claude/worktrees/' + wt + ' || exit 2' + chr(10) + '[ "$(git rev-parse --show-toplevel)" = "C:/Projects/go2cs/.claude/worktrees/' + wt + '" ] || { echo "WRONG WORKTREE: $(pwd)"; exit 2; }' + chr(10)
    i = t.find('set -u'); assert i >= 0, path
    j = t.find(chr(10), i) + 1
    wr(path, t[:j] + g + t[j:])
def add_impl_test_class_leg(path):
    # doctrine 542: every banked row with a *_impl_test.cs companion gets its test host BUILT at assembly, and the sweep list carries it.
    t = io.open(path, encoding='utf-8', newline='').read()
    if 'coord-train31-reflectlite-' in t: return
    t = t.replace('crypto/internal/alias,slices ', 'crypto/internal/alias,slices,internal/reflectlite ', 1)
    lines = t.split(chr(10)); out = []
    for l in lines:
        out.append(l)
        if l.startswith('stamp "LEG reflect '):
            out.append('stamp "BATTERY: internal/reflectlite -tests build (doctrine 542: a banked row with a *_impl_test.cs companion)"')
            out.append('powershell -NoProfile -ExecutionPolicy Bypass -File "$SP/coord-reflect-tests-build.ps1" -Package internal/reflectlite > "$SP/coord-train31-reflectlite-$ts.log" 2>&1; echo "reflectlite exit=$?" >> "$SP/coord-train31-reflectlite-$ts.log"')
            out.append('stamp "LEG reflectlite $(tail -1 "$SP/coord-train31-reflectlite-$ts.log") :: $(grep -aE \'exit=\' "$SP/coord-train31-reflectlite-$ts.log" | tr \'\\n\' \' \')"')
    wr(path, chr(10).join(out))
# --- assemble + rehearse
for src, dst in [('coord-train30-assemble.sh', 'coord-train31-assemble.sh'), ('coord-train30-rehearse.sh', 'coord-train31-rehearse.sh')]:
    t = rd(SP + src).replace('train30', 'train31').replace('TRAIN 30', 'TRAIN 31').replace('coord-t30-resolutions', 'coord-t31-resolutions')
    t = strip_seat_vars(t)
    varblock = NL.join('%s_SHA="${%s_SHA:-%s}"; %s_BRANCH="${%s_BRANCH:-%s}"   # %s' % (v, v, s, v, v, b, lab[:80]) for v, b, s, m, lab in SEATS)
    t = t.replace('export MSBUILDDISABLENODEREUSE=1', 'export MSBUILDDISABLENODEREUSE=1' + NL + varblock, 1)
    lines = t.split(NL); seat_idx = [i for i, l in enumerate(lines) if l.startswith('seat ')]
    assert seat_idx, src
    first = seat_idx[0]
    newseats = ['seat "$%s_BRANCH" "$%s_SHA" "$SP/%s" "%s"' % (v, v, m, lab) for v, b, s, m, lab in SEATS]
    lines = [l for i, l in enumerate(lines) if i not in seat_idx]
    lines[first:first] = newseats
    wr(SP + dst, NL.join(lines))
# --- land
t = rd(SP + 'coord-train30-land.sh').replace('train30', 'train31').replace('TRAIN 30', 'TRAIN 31')
t = re.sub(r'^for b in .*$', 'for b in ' + ' '.join('${%s_SHA:+${%s_BRANCH:-%s}}' % (v, v, b) for v, b, s, m, lab in SEATS) + '; do', t, count=1, flags=re.M)
wr(SP + 'coord-train31-land.sh', t)
# --- land-launch: replace the env assignment lines
t = rd(SP + 'coord-train30-land-launch.sh').replace('train30', 'train31').replace('TRAIN 30', 'TRAIN 31')
lines = t.split(NL); env_idx = [i for i, l in enumerate(lines) if re.match(r'^[A-Z0-9]+_SHA="\$\{[A-Z0-9]+_SHA:-', l)]
assert env_idx
first = env_idx[0]
newenv = ['%s_SHA="${%s_SHA:-%s}" %s_BRANCH="${%s_BRANCH:-%s}" %s' % (v, v, s, v, v, b, BS) for v, b, s, m, lab in SEATS]
lines = [l for i, l in enumerate(lines) if i not in env_idx]
lines[first:first] = newenv
wr(SP + 'coord-train31-land-launch.sh', NL.join(lines))
os.makedirs(SP + 'coord-t30-resolutions', exist_ok=True)
print('derived train-31 scripts:', [f for f in os.listdir(SP) if f.startswith('coord-train31-')])
add_worktree_guard('coord-train31-assemble.sh', 'musing-moser-d4552c')
add_worktree_guard('coord-train31-rehearse.sh', 'coord-t25-dryrun')
add_worktree_guard('coord-train31-land.sh', 'musing-moser-d4552c')
add_impl_test_class_leg('coord-train31-assemble.sh')
