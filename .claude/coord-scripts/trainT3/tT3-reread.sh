#!/usr/bin/env bash
# TRAIN Q -- the ISOLATED RE-READS a battery owes at the landing head (trainQ/tQ-CHANGES.md Q4, Q5, Q7).
#   HEADFULL=<the head, full sha> RUN=<the battery's run folder> bash tT3-reread.sh xmod|xsync [<label>]
# DERIVED 2026-10-06 from the two scripts COORD wrote during TRAIN P's battery and landing
# (coord-scratch/tP/xsync-reread.sh and xmod-reread.sh), with what their runs taught:
#   * a module is read ALONE on the i7, after the battery has printed its rc line (the worktree is frozen before);
#   * P's first x/mod re-read stamped 'tracked changes=0' over 16 rewritten files: its `git -C` ran after
#     MSYS_NO_PATHCONV=1 was exported, never found the tree, printed nothing, and `wc -l` read 0. HERE every git read
#     of the worktree happens BEFORE path conversion is switched off, through tcn (it prints UNREAD when git fails, and
#     its control runs first: tT3-controls.sh arm TC proves all four arms on this box); MSYS_NO_PATHCONV is given to the
#     ONE native command that needs it, on that command's own line, never exported.
#   xmod   golang.org/x/mod v0.33.0 once into a fresh root. EXPECT 9 of 9 packages CLEAN (zip's TestVCS names are the
#          known allowed set), sumdb/tlog 17. The closure of a MOD KNOWN-EXTERNAL (P: sumdb/tlog
#          TestCertificateTransparency, Go-side 'quota exhausted' from the certificate-transparency log server). If the
#          Go side fails AGAIN with a network or quota text the text is printed: the evidence stands, the re-read is
#          NOT clean, and COORD rules on it by name.
#   xsync  golang.org/x/sync v0.19.0 THREE times, each into a fresh root, then ONCE MORE into run 1's root with nothing
#          changed. EXPECT each: driver rc 0, 4 Validated lines summing 28, four records validated and matched with 0
#          errors; the second run into run 1's root: the same, and the same number of .pdb beside each host before and
#          after. The closure of a MOD timing-class line (semaphore TestWeightedAcquire, singleflight TestPanicDo).
# The module copies are the battery's own (RUN/fx/xmod/mod, RUN/fx/xsync/sync: copied from the module cache by
# tT3-modules-legs.sh, hashed there). One conversion at a time; one output root a run. Exit 0 only when the reading is
# CLEAN; the last line is 'REREAD <what> <label> at <head>: CLEAN|NOT CLEAN' and the same line is appended to
# RUN/rereads.txt, which tT3-land-final-reads.sh reads.
set -u
SD=$(cd "$(dirname "$0")" && pwd)
WHAT=${1:?usage: HEADFULL=<sha> RUN=<battery run folder> tT3-reread.sh xmod|xsync [<label>]}
LABEL=${2:-1}
HEADFULL=${HEADFULL:?set HEADFULL to the full sha the worktree must be at (the landing head)}
RUN=${RUN:?set RUN to the run folder of the battery of record}
W=${W:-/h/go2cs-tmp-coord/tT3}
case "$W" in /h/go2cs-tmp-coord/tT3|/h/go2cs-tmp-coord/tT3-[A-Za-z0-9]*) ;; *) echo "ABORT: W=$W is not the tT3 worktree or a tT3-<name> rehearsal worktree"; exit 2 ;; esac
case "$W" in *..*|*/) echo "ABORT: W=$W"; exit 2 ;; esac
case "$WHAT" in xmod|xsync) ;; *) echo "ABORT: '$WHAT' is not xmod or xsync"; exit 2 ;; esac
RR="$RUN/reread"
stamp(){ echo "$(date +%H:%M:%S) $*"; }
# tcn: the count of tracked changes in a worktree, or UNREAD when git did not read it. Never a pipe whose left side can
# fail in silence. (tT3-controls.sh arm TC: clean 0, one planted change 1, no such tree UNREAD, a /h/ path with path
# conversion off UNREAD.)
# >>> tcn (tT3-controls.sh arm TC extracts the line between the markers from EVERY script that carries one and runs its four arms on each copy)
tcn(){ local o rc; o=$(git -C "$1" status --porcelain --untracked-files=no 2>/dev/null); rc=$?; [ "$rc" = 0 ] || { echo UNREAD; return 1; }; printf '%s\n' "$o" | grep -c .; }
# <<< tcn
[ -z "${MSYS_NO_PATHCONV:-}" ] || { stamp "REFUSED: MSYS_NO_PATHCONV is set in this shell: every git -C read of the worktree would be vacuous (Q5). Unset it and relaunch"; exit 2; }
grep -aq 'battery rc=' "$RUN/battery.console.log" 2>/dev/null || { stamp "REFUSED: $RUN/battery.console.log holds no 'battery rc=' line (the battery has not ended: the worktree is frozen)"; exit 2; }
[ "$(git -C "$W" rev-parse HEAD 2>/dev/null)" = "$HEADFULL" ] || { stamp "REFUSED: $W is not at $HEADFULL (it reads $(git -C "$W" rev-parse --short=10 HEAD 2>/dev/null || echo UNREAD))"; exit 2; }
TC=$(tcn "$W")
stamp "PRE head=$(git -C "$W" rev-parse --short=10 HEAD) tracked changes in the worktree=$TC (read before any path setting; a re-read over a tree with tracked changes says so here: P's first x/mod re-read ran over 16 and could not tell)"
[ "$TC" != UNREAD ] || { stamp "ABORT: git did not read $W"; exit 2; }
WB=$(cygpath -w "$W")
GR=$(cygpath -w "$HOME/sdk/go1.24.13")
case "$GR" in */*) stamp "ABORT: GOROOT spelling has a forward slash (floor 6)"; exit 2 ;; esac
[ "$(env -u GOROOT "$HOME/sdk/go1.24.13/bin/go.exe" env GOROOT | tr -d '\r')" = "$GR" ] || { stamp "ABORT: the toolchain's own GOROOT spelling differs"; exit 2; }
export PATH="$HOME/sdk/go1.24.13/bin:$HOME/dotnet10:$PATH"
export GOROOT="$GR" GOTOOLCHAIN=local CGO_ENABLED=0 GOFLAGS=
go version | grep -q 'go1\.24\.13 ' || { stamp "ABORT: go on PATH is $(go version)"; exit 2; }
export DOTNET_ROOT="$(cygpath -w "$HOME/dotnet10")" MSBUILDDISABLENODEREUSE=1 DOTNET_CLI_TELEMETRY_OPTOUT=1
export TEMP="H:\go2cs-tmp-coord\coord-scratch\tmp" TMP="H:\go2cs-tmp-coord\coord-scratch\tmp" TMPDIR=/h/go2cs-tmp-coord/coord-scratch/tmp
export GOCACHE="H:\go-cache\go-build" GOTMPDIR="H:\go2cs-tmp-coord\coord-scratch\tmp" NUGET_PACKAGES="H:\nuget\packages"
export GOPROXY=off GOWORK=off GOSUMDB=off   # the module legs' own switches (GOWORK=off belongs to module legs ONLY: the converter suite is red under it)
unset GO2CS_MODULE_ROOT
mkdir -p "$RR" || exit 2
EXE="$RR/go2cs-head.exe"; EXEW=$(cygpath -w "$EXE")
( cd "$W/src/go2cs" && go build -o "$EXEW" . ) > "$RR/conv-build-$WHAT-$LABEL.log" 2>&1; brc=$?
stamp "converter built from the head: rc=$brc sha256=$(sha256sum "$EXE" 2>/dev/null | cut -c1-16)"
[ "$brc" = 0 ] && [ -s "$EXE" ] || { stamp "ABORT: converter build failed (conv-build-$WHAT-$LABEL.log)"; exit 2; }
conv(){ # module dir (unix), out root (unix), log (unix) -- ONE native command with path conversion off for its own line
  MSYS_NO_PATHCONV=1 "$EXE" -tests -test-action all -test-config Release -test-timeout 2m -recurse -go2cspath "$WB\\src" "$(cygpath -w "$1")" "$(cygpath -w "$2")" > "$3" 2>&1 < /dev/null
}
records(){ # out root (unix), module path, 'pkg=count ...' -> one line a package and RECORDS: CLEAN|NOT CLEAN; zip tolerates TestVCS names
  python -B - "$(cygpath -w "$1")" "$2" "${@:3}" <<'PY'
import io, json, os, sys
out, mod, want = sys.argv[1], sys.argv[2], dict(x.split('=') for x in sys.argv[3:])
ok = True
for pkg, n in sorted(want.items()):
    p = os.path.join(out, 'src', *mod.split('/'), *pkg.split('/'), 'go2cs_test_comparison.json')
    if not os.path.isfile(p):
        print('  %s: NO RECORD' % pkg); ok = False; continue
    d = json.load(io.open(p, encoding='utf-8-sig'))
    g, s = d.get('go') or {}, d.get('csharp') or {}
    errs = d.get('errors') or []
    diff = sorted(k for k in set(g) | set(s) if g.get(k) != s.get(k))
    ap = d.get('addressPairs') or {}
    print('  %s: status=%s matched=%s go=%d cs=%d errors=%d differing=%d address-pairs=1:1:%s/N:N:%s' % (pkg, d.get('status'), d.get('matched'), len(g), len(s), len(errs), len(diff), ap.get('oneToOne', 0), ap.get('nToN', 0)))
    if n == 'vcs':   # zip: its host builds and runs; only TestVCS names may differ (the box's network)
        if not g or not s or any(not k.startswith('TestVCS') for k in diff): ok = False
        continue
    for e in errs[:6]: print('      ' + str(e)[:200].replace('\n', ' '))
    if d.get('status') != 'validated' or not d.get('matched') or errs or len(g) != int(n): ok = False
print('RECORDS: %s' % ('CLEAN' if ok else 'NOT CLEAN'))
PY
}
exttext(){ tr -d '\r' < "$1" | grep -a -o -E '(ResourceExhausted|quota exhausted|dial tcp|i/o timeout|no such host|connection refused|connection reset by peer|TLS handshake timeout|network is unreachable)[^"]{0,80}' | sort | uniq -c | head -n 6; }
VERDICT='NOT CLEAN'
if [ "$WHAT" = xmod ]; then
  MOD="$RUN/fx/xmod/mod"
  [ -f "$MOD/go.mod" ] || { stamp "ABORT: the battery's module copy is missing ($MOD)"; exit 2; }
  OUT="$RR/xmod-$LABEL"; case "$OUT" in "$RUN"/reread/xmod-?*) rm -rf "$OUT" ;; *) exit 2 ;; esac; mkdir -p "$OUT"
  t0=$(date +%s); conv "$MOD" "$OUT" "$RR/xmod-$LABEL.log"; rc=$?
  stamp "RUN xmod $LABEL driver rc=$rc wall=$(( $(date +%s) - t0 ))s (non-zero is expected while zip's TestVCS names differ: the known allowed set)"
  records "$OUT" golang.org/x/mod modfile=323 module=16 semver=9 sumdb/dirhash=6 sumdb=4 sumdb/note=7 sumdb/storage=1 sumdb/tlog=17 zip=vcs | tee "$RR/xmod-$LABEL.read.txt"
  echo "--- Go-side network or quota text in the log (the evidence of an external failure, if any):"; exttext "$RR/xmod-$LABEL.log"
  grep -q '^RECORDS: CLEAN' "$RR/xmod-$LABEL.read.txt" && VERDICT=CLEAN
else
  MOD="$RUN/fx/xsync/sync"
  [ -f "$MOD/go.mod" ] || { stamp "ABORT: the battery's module copy is missing ($MOD)"; exit 2; }
  CLEAN=0
  for i in 1 2 3; do
    OUT="$RR/xsync-$LABEL-$i"; case "$OUT" in "$RUN"/reread/xsync-?*) rm -rf "$OUT" ;; *) exit 2 ;; esac; mkdir -p "$OUT"
    t0=$(date +%s); conv "$MOD" "$OUT" "$RR/xsync-$LABEL-$i.log"; rc=$?
    records "$OUT" golang.org/x/sync errgroup=5 semaphore=8 singleflight=12 syncmap=3 | tee "$RR/xsync-$LABEL-$i.read.txt"
    np=$(tr -d '\r' < "$RR/xsync-$LABEL-$i.log" | grep -a -c -E '^Validated [0-9]+ tests'); v=$(tr -d '\r' < "$RR/xsync-$LABEL-$i.log" | grep -a -E '^Validated [0-9]+ tests' | awk '{ s += $2 } END { print s + 0 }')
    if [ "$rc" = 0 ] && [ "$np" = 4 ] && [ "$v" = 28 ] && grep -q '^RECORDS: CLEAN' "$RR/xsync-$LABEL-$i.read.txt"; then CLEAN=$((CLEAN + 1)); r=CLEAN; else r='NOT CLEAN'; fi
    stamp "RUN xsync $i driver rc=$rc wall=$(( $(date +%s) - t0 ))s Validated lines=$np sum=$v (EXPECT 4 lines, sum 28): $r"
  done
  pdbs(){ for p in errgroup semaphore singleflight syncmap; do printf '%s=%s ' "$p" "$(ls "$RR/xsync-$LABEL-1/src/golang.org/x/sync/$p/bin/tests/publish"/*.pdb 2>/dev/null | wc -l)"; done; }
  # REVIEW ROUND 1: 'before = after' alone passed on NOTHING (both sides 0 when the publish folder is not where pdbs
  # looks). The floor is tT3-modules-legs.sh's: more than one .pdb beside EVERY host, before and after (P's kept clean
  # re-read: errgroup 173, semaphore 66, singleflight 67, syncmap 67).
  # >>> pdbfloor (tT3-controls.sh arm PF extracts the line between the markers; the same line as tT3-modules-legs.sh)
  pdbfloor(){ local t n ok=0; [ -n "$1" ] || return 1; for t in $1; do n=${t#*=}; case "$n" in ''|*[!0-9]*) ok=1 ;; *) [ "$n" -gt 1 ] || ok=1 ;; esac; done; return $ok; }
  # <<< pdbfloor
  B4=$(pdbs); t0=$(date +%s); conv "$MOD" "$RR/xsync-$LABEL-1" "$RR/xsync-$LABEL-1b.log"; rc=$?; AF=$(pdbs)
  records "$RR/xsync-$LABEL-1" golang.org/x/sync errgroup=5 semaphore=8 singleflight=12 syncmap=3 | tee "$RR/xsync-$LABEL-1b.read.txt"
  v=$(tr -d '\r' < "$RR/xsync-$LABEL-1b.log" | grep -a -E '^Validated [0-9]+ tests' | awk '{ s += $2 } END { print s + 0 }')
  pdbfloor "$B4"; fb=$?; pdbfloor "$AF"; fa=$?
  SECOND=0; [ "$rc" = 0 ] && [ "$v" = 28 ] && [ "$B4" = "$AF" ] && [ "$fb" = 0 ] && [ "$fa" = 0 ] && grep -q '^RECORDS: CLEAN' "$RR/xsync-$LABEL-1b.read.txt" && SECOND=1
  stamp "RUN xsync 1b (the unchanged second run into run 1's root) driver rc=$rc wall=$(( $(date +%s) - t0 ))s Validated sum=$v (EXPECT 28) :: .pdb beside each host before [$B4] after [$AF] (EXPECT equal and MORE THAN ONE each: floor before=$([ "$fb" = 0 ] && echo ok || echo FAILED) after=$([ "$fa" = 0 ] && echo ok || echo FAILED)) :: $([ "$SECOND" = 1 ] && echo CLEAN || echo 'NOT CLEAN')"
  stamp "xsync: isolated re-reads $CLEAN of 3 clean; second-run arm $([ "$SECOND" = 1 ] && echo clean || echo 'NOT CLEAN')"
  [ "$CLEAN" = 3 ] && [ "$SECOND" = 1 ] && VERDICT=CLEAN
fi
TC2=$(tcn "$W")
LINE="REREAD $WHAT $LABEL at ${HEADFULL:0:10}: $VERDICT (tracked changes in the worktree before=$TC after=$TC2; $(date '+%Y-%m-%d %H:%M'))"
echo "$LINE" | tee -a "$RUN/rereads.txt"
[ "$VERDICT" = CLEAN ]
