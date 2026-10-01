#!/usr/bin/env bash
# TRAIN L, the i9's COMPLEMENT SHARD at the TRAIN L union (Git Bash on the i9). Brief: tL-lane-brief-i9.md.
# DRAFT 2 2026-10-01 (critic 0 #16: deadlock lines also read from the full-output file and the fresh record's stderr
# tails; critic 0 #19: a row-name abort on crypto/tls, crypto/x509, net and net/*).
# DRAFT 2026-10-01 from tK-i9-shard.sh: same driver; L changes = the union/fixup/seat asserts, GO2CS_MODULE_ROOT unset
# (r-m4), a per-row deadlock-line count (g-deadlock), named readings from the fresh comparison record for internal/synctest
# and os, and two end-of-shard artifacts for COORD: tracked-changes-U0.patch (TE) and the host censuses (HS).
# Launch from a PER-RUN COPY outside the clone (floor 4): copy this file + tL-i9-shard.txt into a run folder, run the
# copy OS-detached; logs land beside it. One row at a time, exact match, sequential. Every rc is captured BEFORE any
# pipe (floor 7). The clone is FROZEN while this runs (the sweep rebuilds go2cs.exe from disk for every row).
#   W=/d/<your clone> UNION=<full union SHA> GOROOT_WIN='<go env GOROOT, backslashes>' \
#     EXPECT_LIST_SHA=<sha256 of tL-i9-shard.txt, from the GO> EXPECT_ROWS=<row count, from the GO> bash ./tL-i9-shard.sh
# Optional: DOTNET_DIR=<a .NET 10 SDK dir to put first on PATH>; FIXUP=<the TRAIN L fixup SHA, asserted HEAD-or-ancestor>.
W=${W:?set W to the i9 clone (POSIX path)}
UNION=${UNION:?set UNION to the full union SHA COORD posted}
GOROOT_WIN=${GOROOT_WIN:?set GOROOT_WIN to the go1.24.13 root exactly as go env GOROOT prints it}
EXPECT_LIST_SHA=${EXPECT_LIST_SHA:?set EXPECT_LIST_SHA to the shard list sha256 COORD posted in the GO}
EXPECT_ROWS=${EXPECT_ROWS:?set EXPECT_ROWS to the shard row count COORD posted in the GO}
SD=$(cd "$(dirname "$0")" && pwd)
LIST="$SD/tL-i9-shard.txt"
LOGDIR=$SD/tL-i9-logs
mkdir -p "$LOGDIR"
SUM="$LOGDIR/SUMMARY.txt"
stamp(){ echo "$(date '+%Y-%m-%d %H:%M:%S') $*" | tee -a "$SUM"; }
abort(){ stamp "ABORT: $*"; exit 2; }
WWIN=$(cygpath -w "$W")
DRV=${WWIN:0:1}
freegb(){ powershell -NoProfile -Command "[int]((Get-PSDrive $DRV).Free/1GB)" 2>/dev/null | tr -d '\r'; }
cfree(){ powershell -NoProfile -Command "[int]((Get-PSDrive C).Free/1GB)" 2>/dev/null | tr -d '\r'; }
whea_since(){ # ISO local time -> count of WHEA-Logger events in the System log since then (read-only)
  powershell -NoProfile -Command "@(Get-WinEvent -FilterHashtable @{LogName='System'; ProviderName='Microsoft-Windows-WHEA-Logger'; StartTime=[datetime]'$1'} -ErrorAction SilentlyContinue).Count" 2>/dev/null | tr -d '\r'
}

[ -f "$LIST" ] || abort "no shard list beside the script ($LIST)"
LSHA=$(sha256sum "$LIST" | cut -d' ' -f1)
[ "$LSHA" = "$EXPECT_LIST_SHA" ] || abort "shard list sha256 $LSHA != $EXPECT_LIST_SHA (the GO's)"
NROWS=$(grep -c . "$LIST")
[ "$NROWS" = "$EXPECT_ROWS" ] || abort "shard list has $NROWS rows, the GO says $EXPECT_ROWS"
[ -z "$(tr -d '\r' < "$LIST" | grep . | LC_ALL=C sort | uniq -d)" ] || abort "shard list repeats a row"
# The standing TLS/net exclusion by ROW NAME (critic 0 #19), not only through the list's sha256: no crypto/tls, no
# crypto/x509 (it stays on the i7 with them), no net or net/* row may be swept here.
BANNED=$(tr -d '\r' < "$LIST" | grep -E '^(crypto/tls|crypto/x509|net)(/|$)' | tr '\n' ' ')
[ -z "$BANNED" ] || abort "the shard list holds row(s) the i9's standing exclusion bans: $BANNED"

# floor 6, checked on the value itself BEFORE it is exported (a self-comparison after the export cannot fail): a
# forward slash here misroutes the whole emission into namespace go.std.* and still exits reporting success.
case "$GOROOT_WIN" in */*) abort "GOROOT_WIN has a forward slash (floor 6): $GOROOT_WIN";; esac
[[ "$GOROOT_WIN" =~ ^[A-Za-z]:\\ ]] || abort "GOROOT_WIN is not a drive-rooted backslash path (floor 6): $GOROOT_WIN"
GRV="$(cygpath -u "$GOROOT_WIN")/VERSION"
[ -f "$GRV" ] || abort "no VERSION file under GOROOT_WIN ($GRV)"
head -n 1 "$GRV" | tr -d '\r' | grep -qx 'go1\.24\.13' || abort "GOROOT_WIN's VERSION reads '$(head -n 1 "$GRV")', not go1.24.13"

export GOROOT="$GOROOT_WIN" GOTOOLCHAIN=local CGO_ENABLED=0
export PATH="$(cygpath -u "$GOROOT_WIN")/bin:$PATH"
if [ -n "${DOTNET_DIR:-}" ]; then export PATH="$DOTNET_DIR:$PATH" DOTNET_ROOT="$(cygpath -w "$DOTNET_DIR")"; fi
export MSBUILDDISABLENODEREUSE=1 DOTNET_CLI_TELEMETRY_OPTOUT=1
# r-m4: the converter sets GO2CS_MODULE_ROOT per host child for MODULE packages only; never inherit one.
unset GO2CS_MODULE_ROOT

cd "$W" || abort "no clone at $W"
[ "$(git rev-parse HEAD)" = "$UNION" ] || abort "HEAD $(git rev-parse --short=10 HEAD) != $UNION (check out the union detached first)"
[ "$(git status --porcelain | grep -vc '^??')" = 0 ] || abort "tracked changes before the shard"
[ "$(go env GOROOT)" = "$GOROOT_WIN" ] || abort "GOROOT pin reads $(go env GOROOT) (floor 6)"
go version | grep -q 'go1\.24\.13 ' || abort "go is $(go version), not go1.24.13"
# TRAIN K's landed master + the L seats that bear on the shard: c1-token-ids (token path: encoding/json, internal/fmtsort,
# sync), g-deadlock-checkdead (internal/synctest, sync, context), r-m4 (every host stages through TryStageModule), r-m6
# (every host's godebug init), c2-publish-binlog-onK (the sweep's Save-RowOutput -OutDir).
for s in 75648a022b 667d052869 0377dbff39 e9009f2945 6a079a9675 df200fa6f8; do
  git merge-base --is-ancestor "$s" HEAD || abort "seat $s is not an ancestor of the union"
done
# The TRAIN L FIXUP must be in the union: two behavioral csproj (ForClauseSpill, CrossPackagePromotedValueMethod) were cut
# at J's template; without the fixup they still carry <LangVersion>latest</LangVersion>. Independent checks:
if [ -n "${FIXUP:-}" ]; then
  git merge-base --is-ancestor "$FIXUP" HEAD || abort "FIXUP $FIXUP is not HEAD or an ancestor of the union"
fi
nfix=$(git log --format=%s 75648a022b..HEAD | grep -c '^fixup: TRAIN L')
[ "$nfix" -ge 1 ] || abort "no 'fixup: TRAIN L' commit between 75648a022b and the union (the union lacks the L fixup)"
git grep -l -F '<LangVersion>latest</LangVersion>' HEAD -- 'src/core/**/*.csproj' 'src/tests/Behavioral/**/*.csproj' \
    'src/tests/Performance/**/*.csproj' > "$LOGDIR/langversion-latest.txt"; rc=$?
[ $rc = 0 ] || [ $rc = 1 ] || abort "git grep (LangVersion latest) rc=$rc"
nlv=$(wc -l < "$LOGDIR/langversion-latest.txt")
[ "$nlv" = 0 ] || abort "union lacks the S1 fixup: $nlv csproj still carry <LangVersion>latest</LangVersion> (langversion-latest.txt)"
SDKV=$(dotnet --version 2>&1); rc=$?
[ $rc = 0 ] || abort "dotnet --version failed under the union's global.json (S1 pin 10.0.100 latestFeature): $SDKV"
stamp "PRE head=$(git rev-parse --short=10 HEAD) tree=$(git rev-parse --short=10 'HEAD^{tree}') $(go version | cut -d' ' -f3-4) sdk=$SDKV rows=$NROWS list-sha=${LSHA:0:16} GOFLAGS='${GOFLAGS:-}' free=$(freegb)G C=$(cfree)G"

leg(){ # name, log-suffix, command...
  local name=$1 suf=$2; shift 2
  local L="$LOGDIR/$suf.log" t0 f c rc w0
  f=$(freegb); [ "${f:-0}" -ge 30 ] || { stamp "LEG $name ABORT: free disk ${f}G < 30G"; exit 3; }
  c=$(cfree); [ "${c:-0}" -ge 8 ] || { stamp "LEG $name ABORT: C: free ${c}G < 8G"; exit 3; }
  w0=$(date '+%Y-%m-%dT%H:%M:%S'); t0=$(date +%s)
  "$@" > "$L" 2>&1 < /dev/null; rc=$?
  stamp "LEG $name rc=$rc wall=$(( $(date +%s) - t0 ))s whea=$(whea_since "$w0") free=$(freegb)G log=$suf.log"
  return $rc
}
fullout(){ # console log -> the POSIX path of the row's saved full output, or nothing (rc 1). The sweep captures the
  # converter's output and, on a FAIL, prints only its last three lines and then 'full output: <path>'
  # (<evidence root>/<pkg>/runfinal/row-output.txt). COUNT/DRIFT verdicts print no such line.
  local p
  p=$(LC_ALL=C sed -n 's/^.*full output: //p' "$1" | tail -n 1 | tr -d '\r')   # sed, not grep -P (Git Bash's locale)
  [ -n "$p" ] || return 1
  p=$(cygpath -u "$p"); [ -f "$p" ] || return 1
  echo "$p"
}
infra(){ # console log -- a BUILD-INFRA failure (the host never produced a verdict), never a verdict failure.
  # Infra = the FULL output names MSB4166 or 'child node ... exited prematurely', AND carries no 'error CS####', AND the
  # console shows no GENERATED TYPE MISSING (c2-gen-guard's class; the sweep re-runs that itself). No full-output line
  # means no evidence: a verdict failure, never rerun.
  local f
  f=$(fullout "$1") || return 1
  grep -aqiE 'MSB4166|child node.{0,12}exited prematurely' "$f" || return 1
  grep -aqE 'error CS[0-9]+' "$f" && return 1
  grep -aqE 'GENERATED TYPE MISSING|output-missing class REPRODUCED' "$1" && return 1
  return 0
}

dlcount(){ # console log, record dir, t0 marker -> 'deadlock-lines=<total> (console=a fullout=b stderr=c record=...)'.
  # Critic 0 #16: the sweep console prints no host output on PASS and 3 lines on FAIL, so a console-only count is
  # structurally 0; this also reads the row's 'full output:' file and the FRESH comparison record's stderr tails.
  local con fo fc rec
  con=$(grep -ac 'all goroutines are asleep' "$1")
  if fo=$(fullout "$1"); then fc=$(grep -ac 'all goroutines are asleep' "$fo"); else fc=none; fi
  if [ -f "$2/go2cs_test_comparison.json" ] && [ "$2/go2cs_test_comparison.json" -nt "$3" ]; then
    rec=$(python - "$2/go2cs_test_comparison.json" <<'PY' | tr -dc '0-9'
import json, sys
st = json.load(open(sys.argv[1], encoding='utf-8-sig')).get('stderr') or {}
print(sum(((st.get(k) or {}).get('text') or '').count('all goroutines are asleep') for k in ('go', 'csharp')))
PY
)
    [ -n "$rec" ] || rec=unreadable
  else rec=no-fresh-record; fi
  local fn=$fc rn=$rec
  case "$fn" in *[!0-9]*|'') fn=0 ;; esac
  case "$rn" in *[!0-9]*|'') rn=0 ;; esac
  echo "deadlock-lines=$(( con + fn + rn )) (console=$con fullout=$fc record-stderr=$rec)"
}
pass=0; fail=0; i=0
while IFS= read -r pkg; do
  pkg=${pkg%$'\r'}; [ -z "$pkg" ] && continue
  i=$((i + 1)); n=$(echo "$pkg" | tr '/' '.')
  touch "$LOGDIR/sweep-$n.log.t0"
  leg "S[$i/$NROWS]:$pkg" "sweep-$n" powershell -NoProfile -ExecutionPolicy Bypass -File src/run-validated-sweep.ps1 -Filter "$pkg" -Exact; rc=$?
  suf="sweep-$n"; try=0
  while [ $rc != 0 ] && [ $try -lt 2 ] && infra "$LOGDIR/$suf.log"; do
    try=$((try + 1))
    aline=$(grep -aE "^\s*(PASS|FAIL|COUNT|DRIFT)" "$LOGDIR/$suf.log" | tail -n 1 | cut -c1-160)
    stamp "  S:$pkg: attempt $try rc=$rc ${aline:-NO VERDICT LINE}; BUILD-INFRA (MSB4166, no error CS, in $(fullout "$LOGDIR/$suf.log")) -- rerun $try of 2"
    suf="sweep-$n-rerun$try"
    leg "S[$i/$NROWS]:$pkg:rerun$try" "$suf" powershell -NoProfile -ExecutionPolicy Bypass -File src/run-validated-sweep.ps1 -Filter "$pkg" -Exact; rc=$?
  done
  line=$(grep -aE "^\s*(PASS|FAIL|COUNT|DRIFT)" "$LOGDIR/$suf.log" | tail -n 1 | cut -c1-200)
  if [ $rc = 0 ]; then pass=$((pass + 1)); else fail=$((fail + 1)); fi
  stamp "  S:$pkg: rc=$rc ${line:-NO VERDICT LINE}$( [ $try = 0 ] || echo " (final attempt after $try infra rerun(s); each earlier attempt's line is above)") $(dlcount "$LOGDIR/$suf.log" "src/core/$pkg" "$LOGDIR/sweep-$n.log.t0")"
  if [ $rc != 0 ]; then fo=$(fullout "$LOGDIR/$suf.log") && stamp "      full output: $fo"; fi
  case "$pkg" in   # named readings from the row's FRESH comparison record (newer than this row's start)
    internal/synctest) named='TestDeadlockRoot TestDeadlockChild' ;;   # g-deadlock: select{} inside a bubble
    os) named='TestRemoveAllWithExecutedProcess' ;;                     # K's re-bank row, read again on NTFS
    *) named='' ;;
  esac
  if [ -n "$named" ]; then
    C="src/core/$pkg/go2cs_test_comparison.json"
    if [ -f "$C" ] && [ "$C" -nt "$LOGDIR/$suf.log.t0" ]; then
      python - "$C" $named <<'PY' | while IFS= read -r l; do stamp "      $l"; done
import json, sys
c = json.load(open(sys.argv[1], encoding='utf-8-sig')); g, s = c.get('go') or {}, c.get('csharp') or {}
for n in sys.argv[2:]:
    ks = sorted(k for k in set(g) | set(s) if k == n or k.startswith(n + '/'))
    print('NAMED %s: %s' % (n, ' '.join('%s go=%s cs=%s' % (k, g.get(k), s.get(k)) for k in ks[:4]) or 'absent from both sides'))
PY
    else
      stamp "      NAMED $named: NO FRESH COMPARISON RECORD for $pkg (not read)"
    fi
  fi
done < "$LIST"

git status --porcelain | grep -v '^??' > "$LOGDIR/tracked-changes.txt"
# For COORD (no new runs): the rewrites as a -U0 patch (TE: COORD runs tL-helpers.py teattr on it) and the host censuses
# (HS: no rewritten stdlib host may carry a module-path argument; GoDefaultGodebug stays in exactly 3 corpus files).
git diff -U0 > "$LOGDIR/tracked-changes-U0.patch"
stamp "HS module-path hosts=$(git grep -lE '\}, "[^"]+"\);' -- 'src/core/*go2cs_test_host.cs' | wc -l) (want 0) GoDefaultGodebug files=$(git grep -l GoDefaultGodebug -- src/core | wc -l) (want 3) :: TE patch $(wc -l < "$LOGDIR/tracked-changes-U0.patch") lines"
stamp "SWEPT rows=$i pass=$pass fail=$fail; tracked files the sweeps rewrote: $(wc -l < "$LOGDIR/tracked-changes.txt") (csproj: $(grep -c 'csproj' "$LOGDIR/tracked-changes.txt"); list in tracked-changes.txt)"
git diff --stat > "$LOGDIR/tracked-changes.stat.txt"
P=$(find src -type d \( -name bin -o -name obj -o -name Generated \) -prune -print | wc -l)
find src -type d \( -name bin -o -name obj -o -name Generated \) -prune -exec rm -rf {} + 2>/dev/null
left=$(find src -type d \( -name bin -o -name obj -o -name Generated \) -prune -print | wc -l)
td=$(git status --porcelain | grep -c '^ D')
stamp "PURGE(after-shard) purged=$P remaining=$left tracked-deletions=$td free=$(freegb)G"
[ "$left" = 0 ] && [ "$td" = 0 ] || abort "purge incomplete or tracked deletions after the purge (floor 8)"
stamp "END head=$(git rev-parse --short=10 HEAD) tracked-changes=$(git status --porcelain | grep -vc '^??')"
stamp "I9 SHARD DONE (TRAIN L)"
