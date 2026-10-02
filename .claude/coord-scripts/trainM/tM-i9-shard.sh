#!/usr/bin/env bash
# TRAIN M, the i9's COMPLEMENT SHARD at the TRAIN M union (Git Bash on the i9). Brief: tM-lane-brief-i9.md.
# DRAFT 2026-10-02, derived from tL-i9-shard.sh (the driver that read 132/132 at TRAIN L): same driver, same list. M's
# changes (tM-CHANGES.md): the seat asserts are READ from the seat list beside the script (every row an ancestor) instead
# of six written shas; the base and the fixup subject are M's; the named readings add the two rows whose EXECUTION CONFIG
# G's roster commits decide (internal/godebug at TC0, log/slog release-tiered), each with the config its own record
# states; an end-of-shard count of untracked files under src/core (c1-fixture-tracking). The deadlock-line count, the
# infra-rerun rule, the row-name ban on crypto/tls, crypto/x509, net and net/*, the TE patch and the HS stamp are L's.
# Launch from a PER-RUN COPY outside the clone (floor 4): copy this file + tM-i9-shard.txt + tM-seats-draft.txt +
# tM-helpers.py (verify round 3: the roster's execution annotations are read by the helper) into a
# run folder, run the copy OS-detached; logs land beside it. One row at a time, exact match, sequential. Every rc is
# captured BEFORE any pipe (floor 7). The clone is FROZEN while this runs (the sweep rebuilds go2cs.exe from disk for
# every row).
#   W=/d/<your clone> UNION=<full union SHA> GOROOT_WIN='<go env GOROOT, backslashes>' \
#     EXPECT_LIST_SHA=<sha256 of tM-i9-shard.txt, from the GO> EXPECT_ROWS=<row count, from the GO> bash ./tM-i9-shard.sh
# Optional: DOTNET_DIR=<a .NET 10 SDK dir to put first on PATH>; FIXUP=<the TRAIN M fixup SHA, asserted HEAD-or-ancestor>.
# VERIFY ROUND 2 (tM-CHANGES.md section 10):
#  * Each row's three gitignored record files are deleted before the row runs (and before an infra rerun): the converter
#    rewrites the comparison record only when its bytes change, so a byte-identical one kept its old mtime and read as
#    'no fresh record', and an older one could be read as the row's own.
#  * The named reading's freshness marker is the row's own (after an infra rerun the old test named a marker that does
#    not exist, and `-nt` is TRUE against a missing file, so any existing record read fresh).
#  * WHICH rows carry an execution config is read from the roster (R1): a row annotated at the base or at the union is a
#    config row, and its expected tiering is whether the union's roster line still carries the annotation. At the 24-row
#    list: internal/godebug False, log/slog True. No row name is typed for that.
#  * The exit status is 4 when a row failed OR a soft expectation missed: an ENV-MISMATCH, a config row with no fresh
#    record, module-path hosts != 0, the GoDefaultGodebug file count moved against the base, untracked paths under
#    src/core, a csproj among the rewrites. Each is stamped 'SOFT:' as well.
# VERIFY ROUND 3 (tM-CHANGES.md section 11; COORD's rulings):
#  * HIGH: the config rows are read by ONE reader, tM-helpers.py execrows (beside this script: a FOURTH file of the run
#    folder), from the roster's ANNOTATION field. Round 2's grep matched the phrase anywhere on a row's line, and the two
#    rows that dropped the annotation keep it in prose: it read internal/godebug as still tiered at the union and would
#    have posted ENV-MISMATCH (exit 4) for a row that ran exactly as ruled. EXPECT at the base internal/godebug, log/slog
#    and net/http; at the union log/slog alone. The base's list is the reader's control (log/slog must be in it).
#  * A config row's ENV verdict is gated POSITIVELY: a named reading that printed neither ENV-OK nor ENV-MISMATCH (the
#    record did not parse) is a NO-RECORD line, and so exit 4.
W=${W:?set W to the i9 clone (POSIX path)}
UNION=${UNION:?set UNION to the full union SHA COORD posted}
GOROOT_WIN=${GOROOT_WIN:?set GOROOT_WIN to the go1.24.13 root exactly as go env GOROOT prints it}
EXPECT_LIST_SHA=${EXPECT_LIST_SHA:?set EXPECT_LIST_SHA to the shard list sha256 COORD posted in the GO}
EXPECT_ROWS=${EXPECT_ROWS:?set EXPECT_ROWS to the shard row count COORD posted in the GO}
SD=$(cd "$(dirname "$0")" && pwd)
LIST="$SD/tM-i9-shard.txt"
LOGDIR=$SD/tM-i9-logs
mkdir -p "$LOGDIR"
SUM="$LOGDIR/SUMMARY.txt"
stamp(){ echo "$(date '+%Y-%m-%d %H:%M:%S') $*" | tee -a "$SUM"; }
abort(){ stamp "ABORT: $*"; exit 2; }
WWIN=$(cygpath -w "$W")
DRV=${WWIN:0:1}
# Verify round 1: the three helpers below run INSIDE the row loop, which reads the shard list on stdin
# (`while read ... done < "$LIST"`), and a powershell.exe that inherits that stdin can eat rows (the hazard
# tM-battery.sh's S loop names). Each now reads /dev/null; the loop's row count is asserted after it, and a failed row
# makes the exit code 4.
freegb(){ powershell -NoProfile -Command "[int]((Get-PSDrive $DRV).Free/1GB)" 2>/dev/null < /dev/null | tr -d '\r'; }
cfree(){ powershell -NoProfile -Command "[int]((Get-PSDrive C).Free/1GB)" 2>/dev/null < /dev/null | tr -d '\r'; }
whea_since(){ # ISO local time -> count of WHEA-Logger events in the System log since then (read-only)
  powershell -NoProfile -Command "@(Get-WinEvent -FilterHashtable @{LogName='System'; ProviderName='Microsoft-Windows-WHEA-Logger'; StartTime=[datetime]'$1'} -ErrorAction SilentlyContinue).Count" 2>/dev/null < /dev/null | tr -d '\r'
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
# TRAIN L's landed master + EVERY M seat, read from the seat list beside this script (ref|sha|notes rows). The seats
# that bear on the shard: g-godebug-pc-line (every goroutine's system/user class, the line a frame resolves to, the
# roster's execution configs: internal/godebug, log/slog, sync, context, io, log, go/build, encoding/json),
# i9-crashclass-byref-recv (src/core/testing/testing.cs: every test host; golib's binders), c1-fixture-tracking (the ten
# fixtures a -tests run used to leave untracked), p2-test-publish-timeout and p2-test-overload-references
# (testConversion.go: every host's publish deadline and its project references).
BASE=aa0a07d5fd
SEATS="$SD/tM-seats-draft.txt"
[ -f "$SEATS" ] || abort "no seat list beside the script ($SEATS): take it from the same handover commit as the driver"
git merge-base --is-ancestor "$BASE" HEAD || abort "TRAIN L's landed master $BASE is not an ancestor of the union"
nseat=0
while IFS='|' read -r ref sha desc; do
  nseat=$((nseat + 1)); git merge-base --is-ancestor "$sha" HEAD || abort "seat $ref $sha is not an ancestor of the union"
done < <(tr -d '\r' < "$SEATS" | grep -E '^[A-Za-z0-9._-]+\|')
[ "$nseat" -ge 1 ] || abort "the seat list holds no seat row"
# The TRAIN M FIXUP must be in the union: it carries the corpus files the union converter emits and no seat committed,
# and the goldens G's go-creator frame made stale. Independent checks:
if [ -n "${FIXUP:-}" ]; then
  git merge-base --is-ancestor "$FIXUP" HEAD || abort "FIXUP $FIXUP is not HEAD or an ancestor of the union"
fi
nfix=$(git log --format=%s "$BASE..HEAD" | grep -c '^fixup: TRAIN M')
[ "$nfix" -ge 1 ] || abort "no 'fixup: TRAIN M' commit between $BASE and the union (the union lacks the M fixup)"
git grep -l -F '<LangVersion>latest</LangVersion>' HEAD -- 'src/core/**/*.csproj' 'src/tests/Behavioral/**/*.csproj' \
    'src/tests/Performance/**/*.csproj' > "$LOGDIR/langversion-latest.txt"; rc=$?
[ $rc = 0 ] || [ $rc = 1 ] || abort "git grep (LangVersion latest) rc=$rc"
nlv=$(wc -l < "$LOGDIR/langversion-latest.txt")
[ "$nlv" = 0 ] || abort "$nlv csproj carry <LangVersion>latest</LangVersion> (langversion-latest.txt): a project cut at an older template reached the union unrepaired"
SDKV=$(dotnet --version 2>&1); rc=$?
[ $rc = 0 ] || abort "dotnet --version failed under the union's global.json (S1 pin 10.0.100 latestFeature): $SDKV"
stamp "PRE head=$(git rev-parse --short=10 HEAD) tree=$(git rev-parse --short=10 'HEAD^{tree}') $(go version | cut -d' ' -f3-4) sdk=$SDKV rows=$NROWS list-sha=${LSHA:0:16} seats=$nseat (all ancestors) GOFLAGS='${GOFLAGS:-}' free=$(freegb)G C=$(cfree)G"
# Verify round 2 (R1): the roster at the base and at the union, kept beside the logs as evidence (the annotations are
# read by the helper below). A config row = annotated in either; its expected tiering = annotated at the union.
git show "$BASE:docs/ValidatedTestPackages.md" > "$LOGDIR/roster-base.md" || abort "cannot read the roster at $BASE"
git show "HEAD:docs/ValidatedTestPackages.md" > "$LOGDIR/roster-head.md" || abort "cannot read the roster at HEAD"
# Verify round 3 (HIGH, RULED): ONE reader of a row's execution config, tM-helpers.py execrows (its ROW and EXEC regexes
# are the definition: the annotation field '<middle dot> execution: <value> <middle dot or |>' of a roster row). The
# grep that stood here matched 'execution: *release-tiered' anywhere on the row's line, PROSE included, and the two rows
# G's roster commits changed still say "It carried `execution: release-tiered` from ..." in their notes.
[ -f "$SD/tM-helpers.py" ] || abort "no tM-helpers.py beside the script ($SD): take it from the same handover commit as the driver (it reads the roster's execution annotations)"
exrows(){ # ref -> the rows annotated release-tiered at that ref, space-separated; rc 1 (and the helper's last line) when it did not read
  local o
  o=$(python -B "$(cygpath -w "$SD/tM-helpers.py")" execrows "$WWIN" "$1" 2>&1 < /dev/null) || { printf '%s' "$o" | tr -d '\r' | tail -n 1 | cut -c1-200; return 1; }
  printf '%s\n' "$o" | tr -d '\r' | grep '^EXECROW ' > "$LOGDIR/execrows-$1.txt"
  sed -n 's/^EXECROW \([^ ]*\) release-tiered$/\1/p' "$LOGDIR/execrows-$1.txt" | LC_ALL=C sort -u | tr '\n' ' '
}
[ "$(grep -cE '^\| \[`[^`]+`\]\(' "$LOGDIR/roster-head.md")" -ge "$NROWS" ] || abort "the roster at HEAD reads fewer rows than the shard holds (a format change?): the execution-config rows cannot be derived"
EX_BASE=$(exrows "$BASE") || abort "tM-helpers.py execrows did not read the roster at $BASE: $EX_BASE"
EX_HEAD=$(exrows HEAD) || abort "tM-helpers.py execrows did not read the roster at HEAD: $EX_HEAD"
case " $EX_BASE " in *" log/slog "*) ;; *) abort "the roster at $BASE reads no execution annotation on log/slog ([${EX_BASE% }]): execrows is not reading, so the config rows cannot be derived" ;; esac
exother=$(grep -v ' release-tiered$' "$LOGDIR/execrows-HEAD.txt" | tr '\n' ';')
[ -z "$exother" ] || abort "the roster at the union carries an execution config this driver cannot verify ($exother): ask COORD"
stamp "PRE execution-config rows, from the roster's ANNOTATION field (tM-helpers.py execrows; not prose): annotated 'execution: release-tiered' at the base=[${EX_BASE% }] at the union=[${EX_HEAD% }] (EXPECT internal/godebug, log/slog and net/http at the base, log/slog alone at the union; a row in either is read with an ENV verdict: expected tiered=True when it is in the union's list, False when it left it)"

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
  rm -f "src/core/$pkg/go2cs_test_comparison.json" "src/core/$pkg/go2cs_test_results.json" "src/core/$pkg/go2cs_test_results.xml"   # verify round 2: the record this row leaves is this row's
  leg "S[$i/$NROWS]:$pkg" "sweep-$n" powershell -NoProfile -ExecutionPolicy Bypass -File src/run-validated-sweep.ps1 -Filter "$pkg" -Exact; rc=$?
  suf="sweep-$n"; try=0
  while [ $rc != 0 ] && [ $try -lt 2 ] && infra "$LOGDIR/$suf.log"; do
    try=$((try + 1))
    aline=$(grep -aE "^\s*(PASS|FAIL|COUNT|DRIFT)" "$LOGDIR/$suf.log" | tail -n 1 | cut -c1-160)
    stamp "  S:$pkg: attempt $try rc=$rc ${aline:-NO VERDICT LINE}; BUILD-INFRA (MSB4166, no error CS, in $(fullout "$LOGDIR/$suf.log")) -- rerun $try of 2"
    suf="sweep-$n-rerun$try"
    rm -f "src/core/$pkg/go2cs_test_comparison.json" "src/core/$pkg/go2cs_test_results.json" "src/core/$pkg/go2cs_test_results.xml"
    leg "S[$i/$NROWS]:$pkg:rerun$try" "$suf" powershell -NoProfile -ExecutionPolicy Bypass -File src/run-validated-sweep.ps1 -Filter "$pkg" -Exact; rc=$?
  done
  line=$(grep -aE "^\s*(PASS|FAIL|COUNT|DRIFT)" "$LOGDIR/$suf.log" | tail -n 1 | cut -c1-200)
  if [ $rc = 0 ]; then pass=$((pass + 1)); else fail=$((fail + 1)); fi
  stamp "  S:$pkg: rc=$rc ${line:-NO VERDICT LINE}$( [ $try = 0 ] || echo " (final attempt after $try infra rerun(s); each earlier attempt's line is above)") $(dlcount "$LOGDIR/$suf.log" "src/core/$pkg" "$LOGDIR/sweep-$n.log.t0")"
  if [ $rc != 0 ]; then fo=$(fullout "$LOGDIR/$suf.log") && stamp "      full output: $fo"; fi
  case "$pkg" in   # named readings from the row's FRESH comparison record (newer than this row's start)
    internal/synctest) named='TestDeadlockRoot TestDeadlockChild' ;;   # checkdead under G's classification: select{} inside a bubble
    os) named='TestRemoveAllWithExecutedProcess' ;;                     # K's re-bank row, read again on NTFS
    internal/godebug) named='TestCmdBisect' ;;                          # G: TC0 now (the roster dropped release-tiered): EXPECT ENV tiered=False
    log/slog) named='TestSetDefault TestPanics TestCallDepth' ;;        # KEEPS release-tiered: EXPECT ENV tiered=True
    *) named='' ;;
  esac
  # verify round 1: the ENV line states its verdict. Verify round 2 (R1): WHICH rows and WHICH tiering come from the
  # roster (EX_BASE / EX_HEAD above), not from two typed row names.
  wantt=-
  case " $EX_BASE $EX_HEAD " in *" $pkg "*) wantt=False; case " $EX_HEAD " in *" $pkg "*) wantt=True ;; esac ;; esac
  if [ -n "$named" ] || [ "$wantt" != - ]; then
    C="src/core/$pkg/go2cs_test_comparison.json"
    # verify round 2: the marker is the ROW's (sweep-$n.log.t0, touched before the first attempt). The old test named
    # "$suf.log.t0", which does not exist after an infra rerun, and -nt is TRUE against a missing file.
    if [ -f "$C" ] && [ "$C" -nt "$LOGDIR/sweep-$n.log.t0" ]; then
      python - "$C" "$wantt" $named <<'PY' > "$LOGDIR/$suf.named.txt" 2>&1
import json, sys
c = json.load(open(sys.argv[1], encoding='utf-8-sig')); g, s = c.get('go') or {}, c.get('csharp') or {}
e = c.get('environment') or {}
want = sys.argv[2]
v = '' if want == '-' else (' ENV-OK' if str(e.get('tiered')) == want else ' ENV-MISMATCH (expected tiered=%s: post it)' % want)
print('ENV configuration=%s tiered=%s (the execution config this row ran at, from its own record)%s' % (e.get('configuration'), e.get('tiered'), v))
for n in sys.argv[3:]:
    ks = sorted(k for k in set(g) | set(s) if k == n or k.startswith(n + '/'))
    print('NAMED %s: %s' % (n, ' '.join('%s go=%s cs=%s' % (k, g.get(k), s.get(k)) for k in ks[:4]) or 'absent from both sides'))
PY
      # verify round 3: the ENV verdict is gated POSITIVELY. A record that did not parse left a traceback here, with
      # neither word in it, and the end-of-shard grep (ENV-MISMATCH or NO-RECORD) then read the row as verified.
      [ "$wantt" = - ] || grep -qE ' ENV-(OK|MISMATCH)' "$LOGDIR/$suf.named.txt" || printf '\nNO-RECORD %s (the ENV verdict was not printed: the record did not parse; read %s.named.txt)\n' "$pkg" "$suf" >> "$LOGDIR/$suf.named.txt"
      while IFS= read -r l <&3; do stamp "      $l"; done 3< "$LOGDIR/$suf.named.txt"   # fd 3: the row loop's stdin is the shard list
    else
      stamp "      NAMED $named: NO FRESH COMPARISON RECORD for $pkg (not read)"
      [ "$wantt" = - ] || echo "NO-RECORD $pkg (a config row with no fresh record: its execution config was not read)" > "$LOGDIR/$suf.named.txt"
    fi
  fi
done < "$LIST"

git status --porcelain | grep -v '^??' > "$LOGDIR/tracked-changes.txt"
# For COORD (no new runs): the rewrites as a -U0 patch (TE: COORD runs tM-i9-te.sh on it; at M the patch is EXPECTED to
# carry G's NoInlining lines, since G regenerated no committed test source) and the host censuses (HS: no rewritten
# stdlib host may carry a module-path argument; GoDefaultGodebug stays in exactly 3 corpus files).
git diff -U0 > "$LOGDIR/tracked-changes-U0.patch"
# Verify round 2: the four end-of-shard expectations were stamps; none reached the exit code. Each count is taken once
# into a variable, stamped as before, and a miss sets `soft` (exit 4 below) and prints a SOFT line. The GoDefaultGodebug
# file count is compared with the base's own (3 at aa0a07d5fd), read from git, not typed.
soft=0
hsm=$(git grep -lE '\}, "[^"]+"\);' -- 'src/core/*go2cs_test_host.cs' | wc -l); hsg=$(git grep -l GoDefaultGodebug -- src/core | wc -l); hsw=$(git grep -l GoDefaultGodebug "$BASE" -- src/core | wc -l)
stamp "HS module-path hosts=$hsm (want 0) GoDefaultGodebug files=$hsg (want $hsw, the base's count) :: TE patch $(wc -l < "$LOGDIR/tracked-changes-U0.patch") lines"
[ "$hsm" = 0 ] && [ "$hsg" = "$hsw" ] || { soft=1; stamp "SOFT: HS module-path hosts=$hsm (want 0) GoDefaultGodebug files=$hsg (want $hsw)"; }
# UF (c1-fixture-tracking): at TRAIN L this shard left untracked, not-ignored fixture copies under src/core (archive/tar,
# crypto/internal/fips140/bigmod and rsa, go/build, go/doc, go/parser); the seat committed them. EXPECT 0 now.
git status --porcelain -- src/core | grep '^??' > "$LOGDIR/untracked-core.txt"
ufn=$(grep -c . "$LOGDIR/untracked-core.txt")
stamp "UF untracked, not-ignored paths under src/core after the shard: $ufn (EXPECT 0) $(head -n 6 "$LOGDIR/untracked-core.txt" | tr '\n' ' ' | cut -c1-400)"
[ "$ufn" = 0 ] || { soft=1; stamp "SOFT: UF $ufn untracked, not-ignored path(s) under src/core (untracked-core.txt)"; }
ncsp=$(grep -c 'csproj' "$LOGDIR/tracked-changes.txt")
stamp "SWEPT rows=$i pass=$pass fail=$fail; tracked files the sweeps rewrote: $(wc -l < "$LOGDIR/tracked-changes.txt") (csproj: $ncsp; list in tracked-changes.txt)"
[ "$ncsp" = 0 ] || { soft=1; stamp "SOFT: $ncsp csproj among the rewrites (TRAIN L read 0 for these rows; F8's reference edge, or a host-project change): $(grep 'csproj' "$LOGDIR/tracked-changes.txt" | head -n 4 | tr '\n' ' ')"; }
envbad=$(grep -lE 'ENV-MISMATCH|^NO-RECORD ' "$LOGDIR"/*.named.txt 2>/dev/null | sed 's#.*/##; s#\.named\.txt$##' | tr '\n' ' ')
[ -z "$envbad" ] || { soft=1; stamp "SOFT: a config row read the wrong execution config, or left no fresh record: ${envbad% }"; }
git diff --stat > "$LOGDIR/tracked-changes.stat.txt"
P=$(find src -type d \( -name bin -o -name obj -o -name Generated \) -prune -print | wc -l)
find src -type d \( -name bin -o -name obj -o -name Generated \) -prune -exec rm -rf {} + 2>/dev/null
left=$(find src -type d \( -name bin -o -name obj -o -name Generated \) -prune -print | wc -l)
td=$(git status --porcelain | grep -c '^ D')
stamp "PURGE(after-shard) purged=$P remaining=$left tracked-deletions=$td free=$(freegb)G"
[ "$left" = 0 ] && [ "$td" = 0 ] || abort "purge incomplete or tracked deletions after the purge (floor 8)"
[ "$i" = "$NROWS" ] || abort "swept $i rows, the list holds $NROWS (rows were lost from the loop's stdin): the shard is INCOMPLETE"
stamp "END head=$(git rev-parse --short=10 HEAD) tracked-changes=$(git status --porcelain | grep -vc '^??')"
stamp "I9 SHARD DONE (TRAIN M) rows=$i pass=$pass fail=$fail soft=$soft"
[ "$fail" = 0 ] && [ "$soft" = 0 ] || exit 4   # verify round 1: a failed row no longer exits 0; verify round 2: nor does a missed soft expectation (the SOFT: lines)
