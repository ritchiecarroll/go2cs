#!/usr/bin/env bash
# TRAIN M linux legs (P1 / P2) at the TRAIN M union. Brief: tM-lane-brief-linux.md (same folder). DRAFT 2026-10-02.
# NOT RUN by its author. Derived from P1's seat p1-linux-legs-template (6175c999c0:
# .claude/coord-scripts/templates/linux-legs.sh), which is trainL/tL-linux-legs.sh (the file TRAIN L's P1 ran) with
# exactly three fixes, each still marked "TEMPLATE FIX n" below and each kept:
#   1. LB passes GoTargetOS=linux to the behavioral runner (without it the C# side is the windows flavour).
#   2. The clone check also refuses UNTRACKED files under any path the union tracks (LEGS_CHECK_ONLY=1 runs just it).
#   3. The "HANG/ABORT/CRASH" flag matches the host's crash banner, not Blame's normal completion line.
# What the template said to edit per train is M's here (tM-CHANGES.md): the seat asserts are READ from the seat list
# beside the script; the fixup gate, the GolibTests class filter, the named converter tests, the rows and the behavioral
# projects are M's; a row may be read at a NON-default execution config (log/slog keeps release-tiered) and its record
# must say so; P1 gains leg LM, the two real modules through -tests -recurse.
# Save THREE files OUTSIDE your build clone, in ONE folder, and run the driver IN THE BACKGROUND (one reading at a time
# on the box): this file, tM-helpers.py and tM-seats-draft.txt, all from the same handover commit.
#   LANE=P1 UNION=<full union SHA from COORD> W=<your build clone> GOROOT=<go1.24.13 root> [FIXUP=<fixup SHA>] bash ~/tM/tM-linux-legs.sh
# Dry check of the clone only (no checkout, no legs): LEGS_CHECK_ONLY=1 on the same command line.
# Legs run SEQUENTIALLY. Every leg's rc is captured BEFORE any pipe (floor 7). Evidence lands in $R, outside the clone.
# The clone is only READ except for what the -tests pipeline and the behavioral runner write (restored after each row)
# and build output (purged between heavy legs). Nothing is committed or pushed by this script.
# P1: GolibTests x2 + the M classes (the linux-only DarwinStdDescriptorContractTests among them); LCn/LCf the converter
#     suite with M's tests by name; rows net/http and internal/godebug at TC0, log/slog release-tiered, sync, os and
#     os/exec (the i9 seat's siginfo_linux.cs compiles only here); LPB; LB the M behavioral projects on linux; LM the two
#     real modules (the box P2's fix batch end acceptance was written for).
# P2: GolibTests Release + the M classes (second box); runtime/pprof once; the FULL runtime row with
#     TestTracebackSystem/panic DISCLOSED by name (checkdead and G's entry classification together); TBS-LOOP x10.
# VERIFY ROUND 2 (tM-CHANGES.md section 10; COORD's rulings):
#  R1  No by-name list is typed here. Derived from git between $BASE and the union, and stamped (DERIVED lines): the
#      converter tests the union adds (LCn), the GolibTests classes it adds or changes minus the ones a linux build does
#      not compile (GTL), the behavioral projects it adds (LB). Each row is read at the execution config its ROSTER line
#      carries at the union (rowleg: -test-tiered when the row's annotation says so: verify round 3), and each row's expected
#      counts are its roster line's linux annotation (tM-helpers.py rosterlinux). The literals left (the hazard-H3
#      neighbours GT_NEIGH, LB_FIXED, the rows of L4 / L5 / L6 and their named tests) are checked against the tree.
#  *   THE DRIVER CAN FAIL NOW (RULED). Every stamp stays, and each expectation that misses is also a MOVER line and is
#      counted: a leg's rc, a row whose N + D differs from its expectation, no fresh record, a name that must pass or
#      read disclosed and does not, a class NOT FOUND or a failed GolibTests name, a missing LCn pass, a REALMOD FAIL
#      line or a missing REALMOD verdict. END prints movers=<n> and the exit status is 4 when movers or freshness-fails
#      is not 0.
#  *   P2's full runtime row: 210 m per child, 450 m outer are the DEFAULTS (RULED): TRAIN L read that row at 9313 s wall
#      on that box against a 9000 s per-child deadline, and M adds cost in every host.
# VERIFY ROUND 3 (tM-CHANGES.md section 11; COORD's rulings wf/notes/COORD-RULINGS-r3.md):
#  *   HIGH: a row's execution config is its roster ANNOTATION field, read by ONE reader, tM-helpers.py execrows (the
#      reader the i7 battery uses). Round 2's own grep matched the phrase anywhere on the row's line, and net/http and
#      internal/godebug still carry it in PROSE: both rows would have been run tiered, at the config G's seat retired.
#      The list is derived once, stamped ('DERIVED execution-config rows'), and the base's list is the reader's control.
#  *   Every expectation is a gate now: a failed GolibTests build, a TRX reader that printed no totals line, LB's build,
#      each LB project's rc (SetegidBroadcastSeam excepted: its residual is expected), LB or LM NOT MEASURED, LM's F4
#      line at the 2m default, LPB, UF and a deadlock line in a TBS iteration each raise a MOVER (exit 4).
set -u
LANE=${LANE:?set LANE=P1 or LANE=P2}
UNION=${UNION:?set UNION to the full union SHA COORD posted}
W=${W:?set W to your build clone}
GOROOT=${GOROOT:?set GOROOT to the go1.24.13 root, spelled exactly as go env GOROOT prints it}
R=${R:-$HOME/tM-linux-$LANE}
HD=$(cd "$(dirname "$0")" && pwd)      # the folder holding this driver, tM-helpers.py and tM-seats-draft.txt
SEATS=${SEATS:-$HD/tM-seats-draft.txt}
BASE=aa0a07d5fd                         # TRAIN L's landed master
REALMOD_TEST_TIMEOUT=${REALMOD_TEST_TIMEOUT:-2m}   # P1's leg LM: the package deadline, stated (2m = the default P2's acceptance was read at)
FLOOR_GB=${FLOOR_GB:-15}
TBS_N=${TBS_N:-10}               # P2: TestTracebackSystem iterations
TBS_LOAD=${TBS_LOAD:-1}          # P2: 1 = run the iterations beside nproc busy loops (K's red appeared only under load)
case "$LANE" in P1|P2) ;; *) echo "LANE must be P1 or P2"; exit 2;; esac
mkdir -p "$R" || exit 2
SUM="$R/SUMMARY.txt"
stamp(){ echo "$(date -u '+%Y-%m-%d %H:%M:%SZ') $*" | tee -a "$SUM"; }
abort(){ stamp "ABORT: $*"; exit 2; }
freegb(){ df -BG --output=avail "$W" | tail -n 1 | tr -dc '0-9'; }

# --- 0. the clone: clean, then the union, detached -------------------------------------------------------------
cd "$W" || abort "no clone at $W"
[ "$(git status --porcelain | grep -vc '^??')" = 0 ] || abort "tracked changes in $W before checkout (stash or reset them yourself first)"
git fetch -q origin claude/coord-trainM-union || abort "fetch of claude/coord-trainM-union failed"
[ "$(git rev-parse FETCH_HEAD)" = "$UNION" ] || abort "origin claude/coord-trainM-union is $(git rev-parse --short=10 FETCH_HEAD), not $UNION (ask COORD before reading anything)"
# TEMPLATE FIX 2: the check above counts TRACKED changes only. Earlier -tests runs leave UNTRACKED files under tracked
# paths (P2's first TRAIN L launch failed its checkout on 33 of them), and a leftover that does not collide still
# pollutes the tree the reading is taken in. Refuse any untracked, non-ignored file whose path the union tracks or that
# sits under a directory the union tracks, naming each (build output is ignored and is not counted).
UTLIST=$(python3 - "$UNION" <<'PY'
import subprocess, sys
def git(*a): return subprocess.run(["git", *a], check=True, capture_output=True).stdout.decode("utf-8", "surrogateescape")
tracked = [p for p in git("ls-tree", "-r", "--name-only", "-z", sys.argv[1]).split("\0") if p]
untracked = [p for p in git("ls-files", "--others", "--exclude-standard", "-z").split("\0") if p]
files, dirs = set(tracked), set()
for p in tracked:
    d = p
    while "/" in d:
        d = d.rsplit("/", 1)[0]
        if d in dirs: break
        dirs.add(d)
bad = []
for p in untracked:
    if p in files:
        bad.append((p, "a path the union tracks: the checkout would fail"))
        continue
    d = p
    while "/" in d:
        d = d.rsplit("/", 1)[0]
        if d in dirs:
            bad.append((p, "under the directory " + d + ", which the union tracks"))
            break
for p, why in bad[:40]: print("  untracked " + p + " (" + why + ")")
if len(bad) > 40: print("  ... and %d more" % (len(bad) - 40))
sys.exit(1 if bad else 0)
PY
); utrc=$?
[ $utrc = 0 ] || abort "untracked files under paths the union tracks in $W (clean them yourself first, by exact path):
$UTLIST"
if [ "${LEGS_CHECK_ONLY:-}" = 1 ]; then stamp "CHECK-ONLY ok: $W has no tracked changes and no untracked files under the union's tracked paths; stopping before the checkout"; exit 0; fi
git checkout -q --detach "$UNION" || abort "checkout $UNION failed"
[ "$(git rev-parse HEAD)" = "$UNION" ] || abort "HEAD is not $UNION"
# TRAIN L's landed master plus EVERY M seat: the rows of the seat list beside this driver (ref|sha|notes), never a list
# of shas written here.
[ -f "$SEATS" ] || abort "no seat list at $SEATS (save tM-seats-draft.txt beside the driver, from the same handover commit)"
[ -f "$HD/tM-helpers.py" ] || abort "no tM-helpers.py beside the driver (leg LM reads its verdicts with it)"
git merge-base --is-ancestor "$BASE" HEAD || abort "TRAIN L's landed master $BASE is not an ancestor of the union"
nseat=0
while IFS='|' read -r ref sha desc; do
  nseat=$((nseat + 1)); git merge-base --is-ancestor "$sha" HEAD || abort "seat $ref $sha is not an ancestor of the union"
done < <(grep -E '^[A-Za-z0-9._-]+\|' "$SEATS")
[ "$nseat" -ge 1 ] || abort "the seat list holds no seat row"
# The TRAIN M FIXUP must be in the union (the corpus files the union converter emits and no seat committed; the goldens
# G's go-creator frame made stale).
if [ -n "${FIXUP:-}" ]; then
  git merge-base --is-ancestor "$FIXUP" HEAD || abort "FIXUP $FIXUP is not HEAD or an ancestor of the union"
fi
nfix=$(git log --format=%s "$BASE..HEAD" | grep -c '^fixup: TRAIN M')
[ "$nfix" -ge 1 ] || abort "no 'fixup: TRAIN M' commit between $BASE and the union (the union lacks the M fixup)"
git grep -l -F '<LangVersion>latest</LangVersion>' HEAD -- 'src/core/**/*.csproj' 'src/tests/Behavioral/**/*.csproj' \
    'src/tests/Performance/**/*.csproj' > "$R/langversion-latest.txt"; rc=$?
[ $rc = 0 ] || [ $rc = 1 ] || abort "git grep (LangVersion latest) rc=$rc"
nlv=$(wc -l < "$R/langversion-latest.txt")
[ "$nlv" = 0 ] || abort "$nlv csproj carry <LangVersion>latest</LangVersion> (langversion-latest.txt): a project cut at an older template reached the union unrepaired"

# --- toolchain: go1.24.13 pinned, Microsoft .NET 10.0.12, GoTargetOS NOT exported (K verbatim) ----------------
case "$GOROOT" in /*) ;; *) abort "GOROOT is not an absolute path: $GOROOT";; esac
case "$GOROOT" in *\\*|*/) abort "GOROOT has a backslash or a trailing slash: $GOROOT";; esac
[ -f "$GOROOT/VERSION" ] || abort "no VERSION file under GOROOT ($GOROOT)"
head -n 1 "$GOROOT/VERSION" | grep -qx 'go1\.24\.13' || abort "GOROOT's VERSION reads '$(head -n 1 "$GOROOT/VERSION")', not go1.24.13"
export GOROOT GOTOOLCHAIN=local CGO_ENABLED=0 GOFLAGS=
export PATH="$GOROOT/bin:$PATH"
unset GoTargetOS
# r-m4: the converter sets GO2CS_MODULE_ROOT per host child for module packages only; never inherit one.
unset GO2CS_MODULE_ROOT
[ "$(go env GOROOT)" = "$GOROOT" ] || abort "go env GOROOT reads $(go env GOROOT), not $GOROOT (floor 6)"
go version | grep -q 'go1\.24\.13 ' || abort "go is $(go version), not go1.24.13"
ENVL=$(bash "$W/docs/phase4/recipes/microsoft-dotnet/use-ms-dotnet.sh") || abort "use-ms-dotnet.sh failed"
eval "$ENVL"
export DOTNET_CLI_TELEMETRY_OPTOUT=1 MSBUILDDISABLENODEREUSE=1
[ "$(command -v dotnet)" = "$DOTNET_ROOT/dotnet" ] || abort "dotnet resolves to $(command -v dotnet), not $DOTNET_ROOT/dotnet"
dotnet build-server shutdown > "$R/build-server-shutdown.log" 2>&1
SDKV=$(cd "$W" && dotnet --version 2>&1); rc=$?
[ $rc = 0 ] || abort "dotnet --version in the clone failed under the union's global.json (S1 pin): $SDKV"
[ "$(env | grep -c '^GoTargetOS=')" = 0 ] || abort "GoTargetOS is still exported"
stamp "PRE lane=$LANE uid=$(id -u) head=$(git rev-parse --short=10 HEAD) tree=$(git rev-parse --short=10 'HEAD^{tree}') seats=$nseat (all ancestors) $(go version | cut -d' ' -f3-4) sdk=$SDKV DOTNET_ROOT=$DOTNET_ROOT nproc=$(nproc) free=$(freegb)G"

leg(){ # name, log-suffix, outer-timeout, command...   (K verbatim)
  local name=$1 suf=$2 tmo=$3; shift 3
  local L="$R/$suf.log" t0 f rc
  f=$(freegb); [ "${f:-0}" -ge "$FLOOR_GB" ] || { stamp "LEG $name ABORT: free disk ${f}G < ${FLOOR_GB}G"; exit 3; }
  t0=$(date +%s)
  timeout -k 120 "$tmo" "$@" > "$L" 2>&1 < /dev/null; rc=$?
  stamp "LEG $name rc=$rc wall=$(( $(date +%s) - t0 ))s free=$(freegb)G log=$suf.log"
  return $rc
}
EXE="$W/src/go2cs/bin/go2cs"
CONV0=
buildconv(){ # label   (K verbatim)
  local rc sha
  (cd "$W/src/go2cs" && go build -o bin/go2cs .) > "$R/conv-build-$1.log" 2>&1; rc=$?
  [ $rc = 0 ] && [ -x "$EXE" ] || abort "converter build ($1) rc=$rc"
  sha=$(sha256sum "$EXE" | cut -c1-16)
  [ -z "$CONV0" ] && CONV0=$sha
  stamp "BUILD converter ($1) sha=$sha$( [ "$sha" = "$CONV0" ] || echo " DIFFERS from the first build $CONV0")"
}
purge(){ # label   (K verbatim)
  local P left td
  P=$(find "$W/src" -type d \( -name bin -o -name obj -o -name Generated \) -prune -print | wc -l)
  find "$W/src" -type d \( -name bin -o -name obj -o -name Generated \) -prune -exec rm -rf {} + 2>/dev/null
  left=$(find "$W/src" -type d \( -name bin -o -name obj -o -name Generated \) -prune -print | wc -l)
  td=$(git -C "$W" status --porcelain | grep -c '^ D')
  stamp "PURGE($1) purged=$P remaining=$left tracked-deletions=$td free=$(freegb)G"
  [ "$left" = 0 ] && [ "$td" = 0 ] || abort "purge incomplete or tracked deletions (floor 8)"
  buildconv "after-$1"
}
FRESH_FAILS=0
# Verify round 2 (RULED): an expectation that misses is a MOVER: stamped where it is found, counted, and the exit status.
MOVERS=0
mover(){ MOVERS=$((MOVERS + 1)); stamp "      MOVER: $*"; }
hpx(){ python3 -B "$HD/tM-helpers.py" "$@"; }
# M: the execution config a row is READ at. Every row runs Release with tiering OFF (the driver's default, the roster's
# default) EXCEPT a row whose roster line carries 'execution: release-tiered': after G's two roster commits that is
# log/slog alone. Such a row is run with -test-tiered and EXPECT_TIERED=True, and its record must say tiered=True;
# every other record must say tiered=False (net/http and internal/godebug now among them).
# Verify round 2 (R1): WHICH row that is, is read from the roster of the checked-out union (tiered_row), never typed.
EXPECT_TIERED=False
# Verify round 3 (HIGH, RULED): ONE reader of that fact, tM-helpers.py execrows (its ROW and EXEC regexes are the
# definition: the row's annotation field '<middle dot> execution: <value> <middle dot or |>', never the phrase anywhere
# on the row's line). Round 2's grep here matched 'execution: *release-tiered' over the whole row, and the two rows G's
# roster commits changed still say "It carried `execution: release-tiered` from ..." in their notes: net/http and
# internal/godebug read tiered, were run with -test-tiered, and their records agreed with the wrong expectation.
# Derived ONCE, at the union and at the base; the base's list is the reader's control (log/slog is annotated there: a
# reader that returns nothing for it is not reading); both lists are stamped.
EXL=$(hpx execrows "$W" HEAD 2>&1) || abort "tM-helpers.py execrows did not read the roster at the union: $(printf '%s' "$EXL" | tail -n 1 | cut -c1-200)"
EXLB=$(hpx execrows "$W" "$BASE" 2>&1) || abort "tM-helpers.py execrows did not read the roster at $BASE: $(printf '%s' "$EXLB" | tail -n 1 | cut -c1-200)"
EXEC_HEAD=$(printf '%s\n' "$EXL" | tr -d '\r' | sed -n 's/^EXECROW \([^ ]*\) \([^ ]*\)$/\1[\2]/p' | tr '\n' ' ')
EXEC_BASE=$(printf '%s\n' "$EXLB" | tr -d '\r' | sed -n 's/^EXECROW \([^ ]*\) \([^ ]*\)$/\1[\2]/p' | tr '\n' ' ')
TIERED_LIST=$(printf '%s\n' "$EXL" | tr -d '\r' | sed -n 's/^EXECROW \([^ ]*\) release-tiered$/\1/p' | tr '\n' ' ')
case " $EXEC_BASE " in *" log/slog[release-tiered] "*) ;; *) abort "the roster at $BASE reads no execution annotation on log/slog ([${EXEC_BASE% }]): execrows is not reading, so no row's config can be derived" ;; esac
stamp "DERIVED execution-config rows (the roster's annotation field, tM-helpers.py execrows; not prose): at the union=[${EXEC_HEAD% }] at the base $BASE=[${EXEC_BASE% }]; run with -test-tiered here: [${TIERED_LIST% }] (EXPECT log/slog alone at the union)"
exoth=$(printf '%s\n' "$EXL" | tr -d '\r' | sed -n '/ release-tiered$/!s/^EXECROW //p' | tr '\n' ';')
[ -z "$exoth" ] || mover "the roster carries an execution config this driver has no switch for (${exoth%;}): such a row would be read at the default config"
tiered_row(){ case " $TIERED_LIST " in *" $1 "*) return 0 ;; esac; return 1; }
# The counts a row is expected to read: EXPECT_V='N D'. 'roster' (the default) = the row's roster line at the union: its
# 'linux: N + D' annotation, else its banked N + D (tM-helpers.py rosterlinux). '' = no count gate (a filtered run).
# ONE stated exception: `os` on a uid-0 box. Go's own oracle skips TestFilePermissions as root, so the root reading is
# 903 + 2 where the banked non-root annotation is 912 + 2 (P1's sizing, 2026-09-30; the brief, section 0).
EXPECT_V=roster
OS_ROOT_EXPECT='903 2'
PASS_NOW=''    # names that must read go=pass cs=pass in the fresh record (each must also be in NAMED_NOW)
summ(){ # comparison.json manifest.json   (K verbatim, + the expected tiering of THIS row)
  python3 - "$1" "$2" "$R/converter-revision.txt" "$EXPECT_TIERED" <<'PY'
import json, os, sys
c = json.load(open(sys.argv[1]))
try: m = json.load(open(sys.argv[2]))
except Exception: m = {}
e = c.get("environment", {})
fails = []
if "go1.24.13" not in (e.get("oracleGoVersion") or ""): fails.append("oracleGoVersion=%r" % e.get("oracleGoVersion"))
if e.get("configuration") != "Release": fails.append("configuration=%r" % e.get("configuration"))
want_tiered = sys.argv[4] == "True"
if e.get("tiered") is not want_tiered: fails.append("tiered=%r (this row is read at tiered=%s)" % (e.get("tiered"), want_tiered))
if m.get("targetGOOS") != "linux": fails.append("targetGOOS=%r" % m.get("targetGOOS"))
rev, rf = m.get("converterRevision"), sys.argv[3]
if not rev: fails.append("converterRevision missing")
elif os.path.exists(rf):
    first = open(rf).read().strip()
    if rev != first: fails.append("converterRevision=%s differs from the first row's %s" % (rev, first))
else: open(rf, "w").write(rev + "\n")
print("      record: status=%s matched=%s go=%d cs=%d skipped=%d disclosed=%d excluded=%d errors=%d orphans=%d config=%s tiered=%s oracle=%s goos=%s conv=%s" % (
  c.get("status"), c.get("matched"), len(c.get("go") or {}), len(c.get("csharp") or {}), len(c.get("skipped") or []),
  len(c.get("disclosed") or []), len(c.get("excluded") or []), len(c.get("errors") or []), len(c.get("orphanedDisclosures") or []),
  e.get("configuration"), e.get("tiered"), (e.get("oracleGoVersion") or "?").replace("go version ", ""), m.get("targetGOOS"), m.get("converterRevision")))
for x in (c.get("errors") or [])[:12]: print("      ERR " + x[:180])
for o in (c.get("orphanedDisclosures") or [])[:6]: print("      ORPHAN " + json.dumps(o)[:180])
for f in fails: print("      FRESHNESS FAIL: " + f)
sys.exit(1 if fails else 0)
PY
}
named(){ # comparison.json name... -- each named test's verdicts and whether it is disclosed (L addition)
  python3 - "$@" <<'PY'
import json, sys
c = json.load(open(sys.argv[1])); g, s = c.get("go") or {}, c.get("csharp") or {}
disc = {d.split(" (")[0] for d in c.get("disclosed") or []}
for n in sys.argv[2:]:
    ks = sorted(k for k in set(g) | set(s) if k == n or k.startswith(n + "/"))
    if not ks: print("      NAMED %s: absent from both sides" % n); continue
    for k in ks[:8]: print("      NAMED %s: go=%s cs=%s%s" % (k, g.get(k), s.get(k), " disclosed" if k in disc else ""))
PY
}
recdl(){ # comparison.json -> deadlock lines in its stderr tails (go + csharp); critic 0 #16 (DRAFT 2)
  python3 - "$1" <<'PY'
import json, sys
try: st = json.load(open(sys.argv[1])).get("stderr") or {}
except Exception: print("unreadable"); sys.exit(0)
print(sum(((st.get(k) or {}).get("text") or "").count("all goroutines are asleep") for k in ("go", "csharp")))
PY
}
NAMED_NOW=''   # tleg reads these names from the fresh record when set (L addition)
tleg(){ # pkg, per-child -test-timeout, outer timeout, suffix, extra converter args...   (K verbatim + NAMED_NOW + deadlock count)
  local pkg=$1 to=$2 otm=$3 suf=$4; shift 4
  local n out C M L changed src lrc vl gv gd want pn
  n=$(echo "$pkg" | tr '/' '.'); out="$W/src/core/$pkg"; C="$out/go2cs_test_comparison.json"; M="$out/go2cs_test_manifest.json"
  rm -f "$C" "$out/go2cs_test_results.json" "$out/go2cs_test_results.xml"
  # Verify round 1: R defaults to a fixed folder that a re-run reuses, and the files below are written only when THIS run
  # reaches its compare. Left from an earlier run they made disclosed() stamp 'DISCLOSED' (and the TBS-LOOP line count a
  # record) for a run whose host died before the compare. Removed first, like the three result files above.
  rm -f "$R/.named-$n-$suf" "$R/.summ-$n-$suf" "$R/tests-$n-$suf.comparison.json" "$R/tests-$n-$suf.manifest.json"
  touch "$R/.t0-$n-$suf"
  # floor 3: the output directory is the SECOND positional. -platforms defaults to this host (linux/amd64).
  leg "T:$pkg:$suf" "tests-$n-$suf" "$otm" "$EXE" -tests -test-action all -test-timeout "$to" -test-config Release "$@" \
      -go2cspath "$W/src" "$GOROOT/src/$pkg" "$out"
  lrc=$?   # verify round 2: the leg's rc, read before anything else (floor 7); it used to be dropped
  L="$R/tests-$n-$suf.log"
  stamp "  T:$pkg:$suf: $(grep -aE 'Validated [0-9]+ tests|comparison failed' "$L" | tail -n 1 | cut -c1-400) :: deadlock-lines=$(grep -ac 'all goroutines are asleep' "$L")"
  # Verify round 2 (RULED): the gates. rc: every leg of the brief is expected to exit 0 (a filtered TBS iteration too:
  # TRAIN L read it rc 0, Validated 0 + 3 disclosed). Counts: the row's N + D against its expectation.
  [ "$lrc" = 0 ] || mover "T:$pkg:$suf rc=$lrc (expected 0)"
  if [ -n "$EXPECT_V" ]; then
    want=$EXPECT_V
    [ "$want" != roster ] || want=$(hpx rosterlinux "$W" "$pkg" 2>/dev/null | sed -nE 's/^ROSTERLINUX [^:]+: ([0-9]+) ([0-9]+) .*/\1 \2/p')
    vl=$(grep -aE 'Validated [0-9]+ tests' "$L" | tail -n 1)
    gv=$(printf '%s' "$vl" | sed -nE 's/.*Validated ([0-9]+) tests.*/\1/p'); gd=$(printf '%s' "$vl" | sed -nE 's/.*[^0-9]([0-9]+) disclosed-divergent.*/\1/p')
    stamp "      counts: read ${gv:-none} + ${gd:-0}; expected ${want:-UNREAD (no roster row read for $pkg)} ($([ "$EXPECT_V" = roster ] && echo "the roster line's linux reading at the union" || echo "stated by the driver: $EXPECT_V"))"
    [ -n "$want" ] && [ "${gv:-x} ${gd:-0}" = "$want" ] || mover "T:$pkg:$suf read ${gv:-none} + ${gd:-0}, expected ${want:-an unread roster row}"
  fi
  if [ -f "$C" ] && [ "$C" -nt "$R/.t0-$n-$suf" ]; then
    cp "$C" "$R/tests-$n-$suf.comparison.json"; [ -f "$M" ] && cp "$M" "$R/tests-$n-$suf.manifest.json"
    summ "$C" "$M" > "$R/.summ-$n-$suf" 2>&1; src=$?     # floor 7: rc before any pipe
    cat "$R/.summ-$n-$suf" >> "$SUM"; cat "$R/.summ-$n-$suf"
    if [ $src != 0 ]; then FRESH_FAILS=$((FRESH_FAILS + 1)); stamp "      FRESHNESS FAIL on $pkg:$suf: this record is NOT a reading at the pinned toolchain"; fi
    if [ -n "$NAMED_NOW" ]; then named "$C" $NAMED_NOW > "$R/.named-$n-$suf" 2>&1; cat "$R/.named-$n-$suf" >> "$SUM"; cat "$R/.named-$n-$suf"; fi
    for pn in $PASS_NOW; do   # verify round 2: a name the brief states as pass/pass, read BY NAME from this run's record
      grep -aqE "NAMED $pn: go=pass cs=pass *\$" "$R/.named-$n-$suf" 2>/dev/null || mover "T:$pkg:$suf $pn does not read go=pass cs=pass by name ($(grep -a "NAMED $pn:" "$R/.named-$n-$suf" 2>/dev/null | head -n 1 | sed 's/^ *//'))"
    done
    stamp "      deadlock lines in the record's stderr tails (go + csharp): $(recdl "$C")"
  else
    stamp "      NO FRESH COMPARISON RECORD (the run did not reach its compare; read the log tail)"
    mover "T:$pkg:$suf left no fresh comparison record: not a reading"
  fi
  git -C "$W" diff --stat -- "src/core/$pkg" > "$R/tests-$n-$suf.srcdiff.txt"
  git -C "$W" diff -U0 -- "src/core/$pkg" > "$R/tests-$n-$suf.srcdiff.patch"
  changed=$(git -C "$W" status --porcelain -- "src/core/$pkg" | grep -vc '^??')
  stamp "      tracked files the run rewrote under src/core/$pkg: $changed (csproj among them: $(grep -c 'csproj' "$R/tests-$n-$suf.srcdiff.txt"))"
  git -C "$W" checkout -q -- "src/core/$pkg"
  [ "$(git -C "$W" status --porcelain | grep -c '^ D')" = 0 ] || abort "tracked deletions after restoring src/core/$pkg"
}
rowleg(){ # pkg, per-child -test-timeout, outer timeout -- a ROW, read at the execution config its roster line carries at the union
  if tiered_row "$1"; then
    stamp "  row $1: the roster row's ANNOTATION carries execution: release-tiered (execrows): run with -test-tiered, its record must say tiered=True"
    EXPECT_TIERED=True; tleg "$1" "$2" "$3" row -test-tiered; EXPECT_TIERED=False
  else
    tleg "$1" "$2" "$3" row
  fi
}
# GolibTests classes the M seats add or change, and the ones hazard H3 names (every name read at the pre-map union):
#   G                      SystemGoroutineTests (5) ReturnSiteLineTests (1) TestGoroutineCreatorTests CreatedByPositionTests
#                          TracebackDecorationTests SkipCountedWalkerFrameTests
#   i9-crashclass          CopyBoundReceiverTests (9) CopyBoundReceiverAllowlistTests (1) NoUncountedBackingAllocationsTests
#   c1-darwin-std-hygiene  DarwinStdDescriptorContractTests (2): compiled ONLY off GoTargetOS=linux, so only this lane reads it
#   H3 (TRAIN L's checkdead under G's classification)  ForeverWaitDeadlockDecisionTests GoroutineProfileInstantTests
#                          GoroutineParkAccountingTests RuntimeParkTransitionTests BubbledChannelTests SyncMutexProfileTests
#   the testing package    ModuleAncestryTests
# Verify round 2 (R1): the names above describe the 15 drafted rows. The LISTS are derived here from the checked-out
# union. GT_LIST = the classes the union adds or changes (the file name is the class name) minus the ones a linux build
# does not compile (GolibTests.csproj removes them when GoTargetOS is set and is not windows: read from the csproj), plus
# the literal neighbours GT_NEIGH, each of which must have a file at HEAD (a miss is a MOVER). At the 24-row map union
# the union adds 8 classes (the two c1 darwin ones, the two P2 host ones among them) and changes 2.
GT_NEIGH='TestGoroutineCreatorTests CreatedByPositionTests TracebackDecorationTests SkipCountedWalkerFrameTests NoUncountedBackingAllocationsTests ForeverWaitDeadlockDecisionTests GoroutineProfileInstantTests GoroutineParkAccountingTests RuntimeParkTransitionTests BubbledChannelTests SyncMutexProfileTests ModuleAncestryTests'
GT_AC=$(git -C "$W" diff --name-only --diff-filter=AM "$BASE" HEAD -- 'src/tests/GolibTests/*Tests.cs' | sed 's#.*/##; s#\.cs$##' | LC_ALL=C sort -u | tr '\n' ' ')
GT_OFFBOX=$(git -C "$W" show HEAD:src/tests/GolibTests/GolibTests.csproj | tr -d '\r' | awk '/<ItemGroup/ { c = $0 } /<\/ItemGroup>/ { c = "" } /<Compile Remove="/ { if (c ~ /!= .windows./) { f = $0; sub(/.*Remove="/, "", f); sub(/".*/, "", f); sub(/\.cs$/, "", f); print f } }' | tr '\n' ' ')
GT_LIST=''
for c in $(printf '%s\n' $GT_AC $GT_NEIGH | LC_ALL=C sort -u); do case " $GT_OFFBOX " in *" $c "*) ;; *) GT_LIST="$GT_LIST$c " ;; esac; done
for c in $GT_NEIGH; do git -C "$W" cat-file -e "HEAD:src/tests/GolibTests/$c.cs" 2>/dev/null || mover "the literal GT_NEIGH names $c and src/tests/GolibTests/$c.cs is not in the union's tree"; done
GTL=$(printf 'FullyQualifiedName~%s|' $GT_LIST); GTL=${GTL%|}
stamp "DERIVED GolibTests classes read by name: added or changed by the union=[${GT_AC% }] + $(echo $GT_NEIGH | wc -w) literal neighbours, minus the ones not compiled for linux = $(echo $GT_LIST | wc -w): ${GT_LIST% }"
# The converter and repoguard tests the union ADDS ('+func Test...(' in the diff of src/go2cs) plus the five projitems
# tests (base tree guards the i7's leg CB also reads by name): leg LCn's list. 73 added at the 24-row map union.
CM=$( { git -C "$W" diff "$BASE" HEAD -- src/go2cs | sed -n 's/^+func \(Test[A-Za-z0-9_]*\)(.*/\1/p'; printf '%s\n' TestProjitemsRegistersEveryGoSource TestProjitemsHasNoDanglingEntries TestProjitemsKeepsItsByteOrderMarkAndConsistentLineEndings TestProjitemsRegistrationClassifierFires TestProjitemsInsertionHintTakesTheNearestPredecessor; } | LC_ALL=C sort -u | tr '\n' '|'); CM=${CM%|}
stamp "DERIVED converter tests read by name (LCn): $(echo "$CM" | tr '|' ' ' | wc -w) names"
# The behavioral projects the union ADDS (top-level directories with an added csproj), then the literal set LB_FIXED:
# TRAIN L's five checkdead guards and the linux-exclusive five. Each literal must be a directory at HEAD.
LB_NEW=$(git -C "$W" diff --name-only --diff-filter=A "$BASE" HEAD -- src/tests/Behavioral | grep -E '^src/tests/Behavioral/[^/]+/[^/]+\.csproj$' | cut -d/ -f4 | grep -vxE 'BehavioralTests|BehavioralRunner' | LC_ALL=C sort -u | tr '\n' ' ')
LB_FIXED='ForeverWaitWorkersMainReturns MainSelectForeverWorkerExits MainSelectForeverAfterFunc ChannelReceiveFromNil ChannelSendToNil LinuxSpawnBasics StdoutCloseEofBarrier UnixAbstractAddrName WritevIovecSeam SetegidBroadcastSeam'
for g in $LB_FIXED; do git -C "$W" cat-file -e "HEAD:src/tests/Behavioral/$g" 2>/dev/null || mover "the literal LB_FIXED names $g and it is not a directory under src/tests/Behavioral in the union's tree"; done
LB_LIST=$(printf '%s\n' $LB_NEW $LB_FIXED | awk '!seen[$0]++' | tr '\n' ' ')
stamp "DERIVED behavioral projects (LB): added by the union=[${LB_NEW% }] + $(echo $LB_FIXED | wc -w) literal = $(echo $LB_LIST | wc -w)"
# The host seats, read from the seat list beside the driver (the i7's rule: a ref p2-*test-list* or a row noted H1; a ref
# p2-*event-line* or a row noted H2). In the union, the package validates in full; absent, it reads the KNOWN shape.
H1SEAT=$(grep -E '^[A-Za-z0-9._-]+\|' "$SEATS" | tr -d '\r' | awk -F'|' '$1 ~ /^p2-.*test-list/ || $3 ~ /(^|[^A-Za-z0-9])H1([^A-Za-z0-9]|$)/ { print $1 }' | head -n 1)
H2SEAT=$(grep -E '^[A-Za-z0-9._-]+\|' "$SEATS" | tr -d '\r' | awk -F'|' '$1 ~ /^p2-.*event-line/ || $3 ~ /(^|[^A-Za-z0-9])H2([^A-Za-z0-9]|$)/ { print $1 }' | head -n 1)
stamp "DERIVED host seats in the union (LM's expectations follow them): H1=${H1SEAT:-ABSENT} H2=${H2SEAT:-ABSENT}"
GP="$W/src/tests/GolibTests/GolibTests.csproj"
gtbuild(){ # cfg   (K verbatim)
  leg "GT-build-$1" "gt-build-$1" 60m env GoTargetOS=linux dotnet build "$GP" -c "$1" -p:GoTargetOS=linux -p:UseSharedCompilation=false
}
gt(){ # cfg, tag, extra dotnet-test args...   (K verbatim)
  local cfg=$1 tag=$2 L; shift 2
  rm -f "$R/gt-$cfg-$tag.trx"
  leg "GT-$cfg-$tag" "gt-$cfg-$tag" 90m env GoTargetOS=linux dotnet test "$GP" -c "$cfg" --no-build -p:GoTargetOS=linux \
      --blame-hang-timeout 20m --blame-hang-dump-type none --logger 'console;verbosity=normal' \
      --logger "trx;LogFileName=gt-$cfg-$tag.trx" --results-directory "$R" "$@"
  L="$R/gt-$cfg-$tag.log"
  stamp "  GT-$cfg-$tag: $(grep -aE '(Passed|Failed)!' "$L" | tail -n 1 | cut -c1-200)"
  stamp "      failed: $(grep -aE '^\s+Failed [A-Za-z]' "$L" | sed 's/^ *Failed //' | cut -d' ' -f1 | tr '\n' ' ' | cut -c1-400)"
  stamp "      skipped: $(grep -aE '^\s+Skipped [A-Za-z]' "$L" | sed 's/^ *Skipped //' | cut -d' ' -f1 | tr '\n' ' ' | cut -c1-400)"
  # Verify round 1: the console names METHODS, so a filter clause that matched nothing (a class not compiled, or
  # renamed) only lowered the total. The i7's by-class TRX reader (tM-helpers.py trx, beside this driver) reads every
  # class of GTL: a class with 0 results is NOT FOUND, and DarwinStdDescriptorContractTests, which only a linux build
  # holds, is stated with its count (2 methods at 35fb6b2bf8).
  if [ -f "$R/gt-$cfg-$tag.trx" ]; then
    python3 -B "$HD/tM-helpers.py" trx "$R/gt-$cfg-$tag.trx" $(echo "$GTL" | sed 's/FullyQualifiedName~//g; s/|/ /g') > "$R/gt-$cfg-$tag.classes.txt" 2>&1; trxrc=$?
    # verify round 3: a reader that throws is a FAILED reading. Its rc was dropped and the only gate was 'NOT FOUND' in its
    # output, so a traceback read as "every class has results". trx prints 'TRX totals' first: no such line, or rc != 0
    # (trx returns 0 whenever it read the file), is a MOVER.
    { [ "$trxrc" = 0 ] && grep -aq '^TRX totals ' "$R/gt-$cfg-$tag.classes.txt"; } || mover "GT-$cfg-$tag: the TRX reader did not read (rc=$trxrc, no 'TRX totals' line in gt-$cfg-$tag.classes.txt): the by-class reading is NOT MEASURED"
    stamp "      classes NOT FOUND: [$(grep -a 'NOT FOUND' "$R/gt-$cfg-$tag.classes.txt" | sed 's/^TRX class //; s/:.*//' | tr '\n' ' ')] DarwinStdDescriptorContractTests=[$(grep -a '^TRX class DarwinStdDescriptorContractTests:' "$R/gt-$cfg-$tag.classes.txt" | sed 's/^[^:]*: //')] (EXPECT none NOT FOUND, and Passed=2; per-class table: gt-$cfg-$tag.classes.txt)"
    # verify round 2: a missing by-name pass is a MOVER (a class of the derived list with 0 results)
    gtnf=$(grep -a 'NOT FOUND' "$R/gt-$cfg-$tag.classes.txt" | sed 's/^TRX class //; s/:.*//' | tr '\n' ' ')
    [ -z "$gtnf" ] || mover "GT-$cfg-$tag classes NOT FOUND: ${gtnf% }"
  else
    stamp "      NO TRX: the by-class reading is NOT MEASURED"
    mover "GT-$cfg-$tag wrote no TRX: the by-class reading is NOT MEASURED"
  fi
  # verify round 2: 0 failed is the expectation of every GolibTests run in the brief; no result line is not a pass
  gtfl=$(grep -aE '^\s+Failed [A-Za-z]' "$L" | sed 's/^ *Failed //' | cut -d' ' -f1 | tr '\n' ' ')
  [ -z "$gtfl" ] || mover "GT-$cfg-$tag failed: $(echo "$gtfl" | cut -c1-300)"
  grep -aE '(Passed|Failed)!' "$L" | tail -n 1 | tr -s ' ' | grep -qE 'Failed: 0,' || mover "GT-$cfg-$tag: no 'Failed: 0' result line ($(grep -aE '(Passed|Failed)!' "$L" | tail -n 1 | tr -s ' ' | cut -c1-120))"
  crashbanner "$L" && stamp "      HANG/ABORT/CRASH text present in the log: read it"
}
crashbanner(){ # log -> rc 0 when the host's abort/crash/hang banner is present   (TEMPLATE FIX 3)
  # The old pattern matched the bare word Blame, which every passing run prints as "Data collector 'Blame' message: All
  # tests finished running, Sequence file will not be generated.": a false flag on 3 of 3 clean TRAIN L GolibTests logs.
  # Match the two banners and any Blame message that is not that normal completion line (a hang or a dump).
  { grep -aE 'The active test run was aborted|Test host process crashed' "$1"
    grep -a "Data collector 'Blame' message:" "$1" | grep -av 'All tests finished running'; } | grep -aq .
}
disclosed(){ # lane-row suffix, test name -- M: a named test must be IN the fresh record and tagged disclosed
  local f="$R/.named-$1" l
  l=$(grep -a "NAMED $2:" "$f" 2>/dev/null | head -n 1 | sed 's/^ *//')
  case "$l" in *" disclosed") stamp "      $2: DISCLOSED ($l)" ;; *) mover "$1: $2 does not read disclosed (${l:-absent from the record}): post it" ;; esac   # verify round 2: counted, not only stamped
}

buildconv first
if [ "$LANE" = P1 ]; then
  # L1 -- the quick STOP gate (K's): a linux -tests row with GoTargetOS unset validates 14 under the union.
  tleg unicode/utf8 10m 240m plain
  grep -aqE 'Validated 14 tests' "$R/tests-unicode.utf8-plain.log" || abort "L1 unicode/utf8 did not validate 14: STOP and post"
  [ "$FRESH_FAILS" = 0 ] || abort "L1 unicode/utf8 record failed its freshness checks: STOP and post"
  purge after-L1
  # L2 -- GolibTests Release and Debug, FULL, then the M classes by name. Three M seats touch golib (G, the i9,
  # c1-darwin-std-hygiene: "a golib-touching seat runs GolibTests at BOTH configurations"); none of the i9's two seats was
  # ever run on linux, and DarwinStdDescriptorContractTests exists only in a linux build.
  for cfg in Release Debug; do
    # verify round 3: a failed build skipped the run and raised nothing (leg() stamps its rc and never counts it), so
    # the brief's 'GolibTests Release and Debug, FULL: 0 failed' could go unread at exit 0.
    if gtbuild "$cfg"; then gt "$cfg" full; else mover "GT-build-$cfg failed (gt-build-$cfg.log): GolibTests $cfg FULL was NOT RUN"; fi
  done
  gt Release mseats --filter "$GTL"
  purge after-GT
  # LCn -- M's converter and repoguard tests BY NAME on linux (-v; '--- PASS' each, no SKIP): the 19 seat names the i7's
  # leg CB reads (CB also reads TestStdLibMetadataInSync, a tree guard read on the i7 and in the fixup; it runs here in
  # LCf). LCf -- the full suite: a red outside the named set is reported, not chased.
  # Verify round 2 (R1): CM is derived above from the union's own diff (it was 19 typed names); the package list is
  # './...' (a seat can add a package: c2-s2-source-metadata adds two); a name that is not PASS, or any SKIP / FAIL
  # line, is a MOVER (a test that skips on linux by design is then a finding to post with its skip text).
  leg LCn conv-named 45m bash -c "cd '$W/src/go2cs' && go test -count=1 -timeout 40m -v -run '^($CM)\$' ./..."
  miss=''; for t in $(echo "$CM" | tr '|' ' '); do
    grep -aqE -- "--- PASS: $t \(" "$R/conv-named.log" || miss="$miss $t"; done
  lcsk=$(grep -acE -- '--- (SKIP|FAIL):' "$R/conv-named.log")
  stamp "  LCn: not-PASS=[${miss# }] skip/fail-lines=$lcsk [$(grep -aE -- '--- (SKIP|FAIL):' "$R/conv-named.log" | awk '{ print $3 }' | head -n 8 | tr '\n' ' ')] ($(echo "$CM" | tr '|' ' ' | wc -w) names, derived)"
  [ -z "$miss" ] && [ "$lcsk" = 0 ] || mover "LCn: not-PASS=[${miss# }] skip/fail-lines=$lcsk"
  leg LCf conv-full 60m bash -c "cd '$W/src/go2cs' && go test -count=1 -timeout 50m ./..."
  stamp "  LCf: $(grep -aE '^(ok|FAIL|---)' "$R/conv-full.log" | tr '\n' ' ' | cut -c1-500)"
  stamp "      tracked changes after LC: $(git -C "$W" status --porcelain | grep -vc '^??') (expect 0)"
  purge after-LC
  # L4 -- the rows G's roster commits decide, at the config the ROSTER carries after them:
  #   net/http          the row default now (Release, tiering off): linux 1387, the TestMain leak check clean (rc 0),
  #                     TestRegisterErr (+ /a) pass/pass
  #   internal/godebug  the row default now: linux 5, TestCmdBisect pass/pass
  #   log/slog          KEEPS release-tiered: run with -test-tiered, linux 199 + 17, its record must say tiered=True
  # then sync (46 + 6: checkdead's neighbours under G's classification) and os + os/exec, the two rows whose linux
  # build carries the i9 seat's internal/syscall/unix/linux/siginfo_linux.cs (a file that compiles nowhere else).
  # Verify round 2 (R1): each row goes through rowleg, which reads the row's execution config from the roster at the
  # union (log/slog alone carries one), and its counts are the roster line's linux reading (EXPECT_V=roster). The two
  # names the brief states as pass/pass are gated by name (PASS_NOW).
  NAMED_NOW='TestRegisterErr'; PASS_NOW='TestRegisterErr'; rowleg net/http 60m 300m; NAMED_NOW=''; PASS_NOW=''
  NAMED_NOW='TestCmdBisect'; PASS_NOW='TestCmdBisect'; rowleg internal/godebug 30m 240m; NAMED_NOW=''; PASS_NOW=''
  NAMED_NOW='TestSetDefault TestPanics TestCallDepth'; rowleg log/slog 30m 240m; NAMED_NOW=''
  for pkg in sync os os/exec; do
    [ "$pkg" = os ] && [ "$(id -u)" = 0 ] && EXPECT_V=$OS_ROOT_EXPECT   # the root reading (see OS_ROOT_EXPECT)
    rowleg "$pkg" 30m 240m; EXPECT_V=roster
  done
  # LPB -- F4 rewrote publishTestHost's deadline; this is TRAIN L's composed reading of that function on linux: the test
  # host's publish carries BOTH -p:GoTargetOS=linux (the record's targetGOOS, checked by summ) and -bl (the binlog EXISTS
  # during the run: a 1 s background poll; a passing publish then deletes it). EXPECT rc 0, SEEN, absent afterwards.
  BL="$W/src/core/cmp/bin/tests/publish.binlog"; rm -f "$R/.lpb-seen"
  # Verifier: a leftover binlog would read SEEN before this leg's publish starts, so record it (pre=PRESENT means SEEN
  # cannot be trusted; post it) and delete it before the poll starts.
  lpbpre=$([ -e "$BL" ] && echo PRESENT || echo absent); rm -f "$BL"
  ( while :; do [ -e "$BL" ] && : > "$R/.lpb-seen"; sleep 1; done ) > /dev/null 2>&1 & LPBPID=$!
  trap '[ -n "${LPBPID:-}" ] && kill "$LPBPID" 2>/dev/null' EXIT   # an abort inside tleg must not orphan the poll (killed by PID)
  tleg cmp 10m 60m binlog -test-publish-binlog
  lpbrc=$(grep -aoE 'LEG T:cmp:binlog rc=[0-9]+' "$SUM" | tail -n 1 | sed 's/.*rc=//')
  kill "$LPBPID" 2>/dev/null; wait "$LPBPID" 2>/dev/null; LPBPID=''; trap - EXIT
  lpbdur=$([ -e "$R/.lpb-seen" ] && echo SEEN || echo never-seen); lpbaft=$([ -e "$BL" ] && echo PRESENT || echo absent)
  stamp "  LPB (EXPECT rc 0, pre=absent, binlog SEEN during, absent after): rc=$lpbrc pre=$lpbpre during=$lpbdur after=$lpbaft"
  # verify round 3: LPB and UF were stamps. Each expectation of the brief's LPB row is a gate (the leg's rc itself is
  # tleg's mover), and so is UF's 0.
  [ "$lpbpre" = absent ] && [ "$lpbdur" = SEEN ] && [ "$lpbaft" = absent ] || mover "LPB: pre=$lpbpre during=$lpbdur after=$lpbaft (expected absent, SEEN, absent)"
  ufn=$(git -C "$W" status --porcelain -- src/core | grep -c '^??')
  stamp "  UF untracked, not-ignored paths under src/core after the rows (c1-fixture-tracking; EXPECT 0): $ufn $(git -C "$W" status --porcelain -- src/core | grep '^??' | head -n 5 | tr '\n' ' ')"
  [ "$ufn" = 0 ] || mover "UF after the rows: $ufn untracked, not-ignored path(s) under src/core (expected 0)"
  purge after-L4
  # LB -- the behavioral projects on linux through the runner run-behavioral.ps1 wraps (no pwsh here).
  #   M's seven: PromotedPtrMethodValueSet (the i9; never run off windows) and P2's six F-batch guards.
  #   H3: TRAIN L's five checkdead guards under G's classification (the main-alone pair: stderr first line
  #       'fatal error: all goroutines are asleep - deadlock!', exit 2 on both sides).
  #   c1-darwin-std-hygiene's seat claim: LinuxSpawnBasics and StdoutCloseEofBarrier PASS on linux.
  #   G's linux goldens: UnixAbstractAddrName and WritevIovecSeam were re-baselined on linux by the seat; and
  #       SetegidBroadcastSeam took the seat's 3 lines BY HUNK, so its Target phase is expected to read a residual that is
  #       exactly the go1.24 alias family (lines 5/43/60/61, hop-owned): report the residual's lines, do not call it red.
  # The runner's output folder (bin/Debug/net*) is INFERRED from run-behavioral.ps1: a missing executable is NOT
  # MEASURED (false-green route #6), never a pass.
  BR="$W/src/tests/Behavioral/BehavioralRunner"
  leg LB-build behav-runner-build 30m env GoTargetOS=linux dotnet build "$BR/BehavioralRunner.csproj" -c Debug   # TEMPLATE FIX 1 (as the runs)
  lbbrc=$?; [ "$lbbrc" = 0 ] || mover "LB-build rc=$lbbrc (behav-runner-build.log)"   # verify round 3: the build's rc is a gate
  BRX=$(ls "$BR"/bin/Debug/net*/BehavioralRunner 2>/dev/null | head -n 1)
  # TEMPLATE FIX 1: the runner must see GoTargetOS=linux. The driver unsets it for the -tests rows (p1-tests-goos passes it
  # itself), but the runner has no such seam: without it the C# side is the windows flavour (kernel32.dll DllNotFound,
  # exit 2 vs 0 on 6 of 7 projects at TRAIN L). Set per command with env, so GoTargetOS stays unexported in the driver.
  lbcmd(){ printf "cd '%s/src/tests/Behavioral' && env GoTargetOS=linux '%s' --filter '%s'" "$W" "$BRX" "$1"; }
  if [ -n "$BRX" ] && [ -x "$BRX" ]; then
    for p in $LB_LIST; do   # verify round 2 (R1): derived above (the projects the union adds) + the literal LB_FIXED; 17 at the 24-row map union
      stamp "  LB command: bash -c \"$(lbcmd "$p")\""
      leg "LB:$p" "behav-$p" 60m bash -c "$(lbcmd "$p")"; lbrc=$?
      stamp "  LB:$p: $(grep -aE '(Transpile|Compile|Target|Output).*(pass|fail)|compared|exit code' "$R/behav-$p.log" | tr -s ' ' | tr '\n' ' ' | cut -c1-300)"
      # verify round 3: each project's rc was dropped, so the M guards and TRAIN L's checkdead guards could not red the
      # driver. The runner exits 0 when every phase of the selected project(s) passes (the main-alone pair's exit 2 on
      # both sides is a pass), 1 on a failed phase, 2 when the filter matched nothing. ONE stated exception:
      # SetegidBroadcastSeam's Target phase is EXPECTED to read the go1.24 alias-family residual (the brief), so its rc is
      # stamped and read by the lane, not gated.
      case "$p" in
        SetegidBroadcastSeam) stamp "      LB:$p rc=$lbrc (not gated: the Target residual is expected; report its lines)" ;;
        *) [ "$lbrc" = 0 ] || mover "LB:$p rc=$lbrc (behav-$p.log)" ;;
      esac
    done
    # TRAIN L's parse fix: NUL-separated names from git, the restore's rc read.
    git -C "$W" diff --name-only -z HEAD > "$R/lb-rewrites.paths.z"
    stamp "      tracked changes after LB: $(tr -cd '\0' < "$R/lb-rewrites.paths.z" | wc -c) (expect 0 beyond SetegidBroadcastSeam's residual; any are saved to lb-rewrites.patch, then restored)"
    if [ -s "$R/lb-rewrites.paths.z" ]; then
      git -C "$W" diff HEAD > "$R/lb-rewrites.patch"
      git -C "$W" restore --source=HEAD --staged --worktree --pathspec-from-file="$R/lb-rewrites.paths.z" --pathspec-file-nul; lrc=$?
      [ "$lrc" = 0 ] && [ "$(git -C "$W" status --porcelain | grep -vc '^??')" = 0 ] || abort "LB restore rc=$lrc left tracked changes: STOP and post"
    fi
  else
    stamp "  LB: NOT MEASURED -- no BehavioralRunner executable under $BR/bin/Debug/net*/ (read behav-runner-build.log)"
    mover "LB NOT MEASURED (no BehavioralRunner executable; behav-runner-build.log): none of the $(echo $LB_LIST | wc -w) LB projects was read"   # verify round 3
  fi
  purge after-LB
  # LM -- the two REAL MODULES through `go2cs -tests -recurse <moduleDir> <outRoot>`, -test-timeout stated. These are the
  # readings P2's fix batch END ACCEPTANCE was written from (linux, P2's local merge): errgroup 5, syncmap 3,
  # singleflight 11 of 12 (TestPanicDoChan: host finding H1), semaphore 7 of 8 (TestWeightedAcquire: the known
  # timing-class non-pass; P2's label was "TC0 timing", its cause is NOT MEASURED); modfile 323, module 16, semver 9, sumdb/dirhash 6, sumdb 4, sumdb/note 7, sumdb/storage 1, sumdb/tlog 16 of
  # 17 (TestCertificateTransparency: host finding H2), zip builds and runs. The union is the first PUSHED tree with all six
  # fixes. The three KNOWN non-passes are read BY NAME and stamped KNOWN; anything else is a FAIL. F4: 0 'dotnet timed
  # out' lines. Each module is COPIED out of the module cache (never a working directory there), GOPROXY=off, no download.
  # A uid-0 box masks permission defects in the module staging: this reading does not stand in for a non-root one.
  LMX="$R/lm"; rm -rf "$LMX"; mkdir -p "$LMX/fx" "$LMX/out"
  GMC=$(go env GOMODCACHE)
  lmrun(){ # tag, module copy, out root
    leg "LM:$1" "lm-$1" 240m env GOPROXY=off GOWORK=off GOSUMDB=off "$EXE" -tests -test-action all -test-config Release \
        -test-timeout "$REALMOD_TEST_TIMEOUT" -recurse -go2cspath "$W/src" "$2" "$3"
  }
  lmread(){ # tag, module path, spec...
    local t=$1 m=$2; shift 2
    python3 -B "$HD/tM-helpers.py" realmod --log "$R/lm-$t.log" --root "$LMX/out/$t/src/$m" --module "$m" "$@" > "$R/lm-$t.realmod.txt" 2>&1
    grep -a '^REALMOD' "$R/lm-$t.realmod.txt" | while IFS= read -r l; do stamp "  LM:$t ${l:0:340}"; done
    # verify round 2 (RULED): a REALMOD FAIL line (a package outside PASS / KNOWN / CLEARED) and a reader that printed
    # no verdict are MOVERS. The loop above runs in a pipe, so the count is taken here, from the file.
    lmf=$(grep -acE '^REALMOD [^ ]+: FAIL -- ' "$R/lm-$t.realmod.txt")
    [ "$lmf" = 0 ] || mover "LM:$t $lmf package(s) read FAIL: $(grep -aE '^REALMOD [^ ]+: FAIL -- ' "$R/lm-$t.realmod.txt" | sed 's/^REALMOD //; s/: FAIL.*//' | tr '\n' ' ' | cut -c1-300)"
    grep -aq '^REALMOD-VERDICT' "$R/lm-$t.realmod.txt" || mover "LM:$t the reader printed no REALMOD-VERDICT line (lm-$t.realmod.txt)"
    # verify round 3: F4's '0 dotnet timed out lines' could not fail here (the i7 gates it in tM-modules-legs.sh, and
    # this is the box P2's acceptance was written for). The verdict exists only at the 2m default: at a longer
    # -test-timeout a first publish fits inside the deadline with or without F4, so it is stamped NOT MEASURED there.
    f4=$(grep -a '^REALMOD-F4' "$R/lm-$t.realmod.txt" | grep -oE 'log: [0-9]+' | tr -dc '0-9')
    case "$REALMOD_TEST_TIMEOUT" in
      2m|2m0s|120s) [ "${f4:-x}" = 0 ] || mover "LM:$t F4: ${f4:-unread} 'dotnet timed out' line(s) at -test-timeout $REALMOD_TEST_TIMEOUT (expected 0)" ;;
      *) stamp "  LM:$t F4: NOT MEASURED -- -test-timeout $REALMOD_TEST_TIMEOUT is not the 2m default (lines=${f4:-unread}): post F4 as NOT MEASURED" ;;
    esac
  }
  if [ -f "$GMC/golang.org/x/sync@v0.19.0/go.mod" ]; then
    cp -r "$GMC/golang.org/x/sync@v0.19.0" "$LMX/fx/sync"
    stamp "  LM:xsync copy: files=$(find "$LMX/fx/sync" -type f | wc -l) read-only=$(find "$LMX/fx/sync" -type f ! -perm -u+w | wc -l) uid=$(id -u)"
    lmrun xsync "$LMX/fx/sync" "$LMX/out/xsync"
    # verify round 2: with the H1 host seat in the union singleflight is expected in full (12); absent, the KNOWN shape
    SFSPEC='singleflight=11/12:TestPanicDoChan:pass:-'; [ -z "$H1SEAT" ] || SFSPEC='singleflight=12'
    lmread xsync golang.org/x/sync errgroup=5 syncmap=3 "$SFSPEC" 'semaphore=7/8:TestWeightedAcquire:pass:fail'
  else
    stamp "  LM:xsync: NOT MEASURED -- golang.org/x/sync@v0.19.0 is not in $GMC (never downloaded here)"
    mover "LM:xsync NOT MEASURED: the module is not in the module cache (P2's fix-batch end acceptance was not read on this box)"   # verify round 3: NOT MEASURED is not a pass
  fi
  XT="$GMC/cache/download/golang.org/x/tools/@v"
  # Verify round 1: the pin is tM-modules-legs.sh's (the i7 copy) with its checks, which this copy had lost: BOTH cache
  # files the pin reads must exist, and the pin is verified (one go.mod line edited, both hashes h1:) before the run. A
  # missing .mod gave an empty hash, a malformed go.sum line and a module run whose failure read as the seats'.
  XMSKIP=0
  if [ -f "$GMC/golang.org/x/mod@v0.33.0/go.mod" ] && { [ -f "$XT/v0.41.0.zip" ] || { [ -f "$XT/v0.42.0.ziphash" ] && [ -f "$XT/v0.42.0.mod" ]; }; }; then
    cp -r "$GMC/golang.org/x/mod@v0.33.0" "$LMX/fx/mod"
    if [ ! -f "$XT/v0.41.0.zip" ]; then
      # P1's STATED DEVIATION (FINDING record, section 3): x/mod requires x/tools v0.41.0 and the cache holds v0.42.0, so
      # the COPY's go.mod names v0.42.0 and its go.sum gains that version's two lines, built from the cache's own files.
      chmod u+w "$LMX/fx/mod/go.mod" "$LMX/fx/mod/go.sum"
      had=$(grep -c 'golang.org/x/tools v0.41.0' "$LMX/fx/mod/go.mod")
      sed -i 's#golang.org/x/tools v0.41.0#golang.org/x/tools v0.42.0#' "$LMX/fx/mod/go.mod"
      zh=$(tr -d '\r\n' < "$XT/v0.42.0.ziphash")
      mh=$(python3 -c "import hashlib,base64,sys; d=open(sys.argv[1],'rb').read(); s=hashlib.sha256(d).hexdigest()+'  go.mod'+chr(10); print('h1:'+base64.b64encode(hashlib.sha256(s.encode()).digest()).decode())" "$XT/v0.42.0.mod")
      printf 'golang.org/x/tools v0.42.0 %s\ngolang.org/x/tools v0.42.0/go.mod %s\n' "$zh" "$mh" >> "$LMX/fx/mod/go.sum"
      stamp "  LM:xmod pin (STATED DEVIATION): go.mod require golang.org/x/tools v0.41.0 -> v0.42.0 (lines edited: $had), go.sum +2 lines ($zh ; go.mod $mh)"
      case "$had:$zh:$mh" in 1:h1:*:h1:*) ;; *) XMSKIP=1 ;; esac
      [ "$(grep -c 'golang.org/x/tools v0.42.0' "$LMX/fx/mod/go.mod")" = 1 ] || XMSKIP=1
    else
      stamp "  LM:xmod pin: none needed (the cache holds golang.org/x/tools v0.41.0): the LITERAL module"
    fi
    if [ "$XMSKIP" = 1 ]; then
      stamp "  LM:xmod: NOT MEASURED -- the pin failed (go.mod lines edited=$had zip=[$zh] mod=[$mh]): nothing was run"
      mover "LM:xmod NOT MEASURED: the x/tools pin failed, nothing was run"   # verify round 3
    else
    lmrun xmod "$LMX/fx/mod" "$LMX/out/xmod"
    if grep -aq 'go mod download preflight failed' "$R/lm-xmod.log"; then
      stamp "  LM:xmod: NOT MEASURED -- the module-lock preflight refused it under GOPROXY=off: $(grep -a 'preflight failed' "$R/lm-xmod.log" | head -n 1 | cut -c1-220)"
      mover "LM:xmod NOT MEASURED: the module-lock preflight refused the module under GOPROXY=off (lm-xmod.log)"   # verify round 3
    else
      TLSPEC='sumdb/tlog=16/17:TestCertificateTransparency:*:-'; [ -z "$H2SEAT" ] || TLSPEC='sumdb/tlog=17'   # verify round 2: the H2 host seat in the union means 17 (its Go side reads a network log: read a red there against the network first)
      lmread xmod golang.org/x/mod modfile=323 module=16 semver=9 sumdb/dirhash=6 sumdb=4 sumdb/note=7 sumdb/storage=1 "$TLSPEC" zip=build:TestVCS
      # verify round 3 (RULED): with the H2 seat in the union a red on sumdb/tlog is read against the NETWORK first, and stamped so
      grep -aqE '^REALMOD golang\.org/x/mod/sumdb/tlog: FAIL -- ' "$R/lm-xmod.realmod.txt" && stamp "  LM:xmod sumdb/tlog reads FAIL: read it against this box's NETWORK first (TestCertificateTransparency's Go side reads a network log); only then against the H2 seat ${H2SEAT:-(absent)}"
    fi
    fi
  else
    stamp "  LM:xmod: NOT MEASURED -- golang.org/x/mod@v0.33.0, or golang.org/x/tools at v0.41.0 or v0.42.0, is not in $GMC (never downloaded here)"
    mover "LM:xmod NOT MEASURED: the module or its x/tools requirement is not in the module cache"   # verify round 3
  fi
  stamp "      tracked changes after LM: $(git -C "$W" status --porcelain | grep -vc '^??') (expect 0: the module legs write only under $LMX)"
  purge after-LM
else
  # L2 -- GolibTests Release, FULL and PLAIN, then the M classes by name (the second box)
  gtbuild Release || abort "GolibTests Release build failed"
  gt Release full
  gt Release mseats --filter "$GTL"
  purge after-GT
  # L5 -- runtime/pprof once (banked linux: 147 + 7): G's PC->line fix names TestMemoryProfiler (+ /debug=1); TRAIN L's
  # checkdead takes s_profileGate EXCLUSIVE, the goroutine profile's gate, and now meets G's goroutine classification.
  NAMED_NOW='TestGoroutineProfileConcurrency TestBlockProfile TestMemoryProfiler'; rowleg runtime/pprof 30m 240m; NAMED_NOW=''
  purge after-pprof
  # L6 -- the FULL linux runtime row (banked linux: 10810 + 73). THE re-read the seat draft assigns to the M union
  # (tL-seats-draft.txt:59): on G's branch alone this lane read TestTracebackSystem/panic RED, 'deadlock-first', because
  # the branch lacked TRAIN L's checkdead. The union holds both: /panic and /trap must read DISCLOSED-divergent (not an
  # undisclosed red), the crash family pass/pass, and TestLineNumber disclosed (G's commit names it).
  NAMED_NOW='TestTracebackSystem TestLineNumber TestSimpleDeadlock TestInitDeadlock TestLockedDeadlock TestLockedDeadlock2 TestGoexitDeadlock TestGoNil TestMainGoroutineID TestNoHelperGoroutines TestPanicDeadlockGosched TestPanicDeadlockSyscall TestStopTheWorldDeadlock'
  # Verify round 1: the budget was L's (150m per child, 330m outer) and L's own reading of this row on this box was
  # 9313 s wall against that 9000 s per-child deadline (tL-seats-draft.txt:33).
  # Verify round 2 (HIGH, RULED): 210m per child and 450m outer are the DEFAULTS. The host's share of a runtime leg is
  # 0.96 on the i7 (5648 s of 5891 s) and 0.93 on G's box (3655 s of 3914 s, tL-seats-draft.txt:78), which puts this
  # box's C# child near 8,700 to 8,940 s against 9,000 s BEFORE M adds NoInlining on every go-executing function, a
  # second PDB reader and [GoRecv] in every host. A timeout here costs 2.5 h and leaves no reading of THE re-read this
  # train owes. The outer cap still bounds a hang (Go child + C# child + build fit inside 450 m).
  rowleg runtime "${RT_CHILD:-210m}" "${RT_OUTER:-450m}"
  NAMED_NOW=''
  disclosed runtime-row TestTracebackSystem/panic
  disclosed runtime-row TestTracebackSystem/trap
  purge after-runtime
  # TBS-LOOP -- hazard H3 under load: K's linux final head read /panic RED on a loaded box (the 200 ms race on the
  # child's 'go child(); select {}'); TRAIN L read it disclosed 10 of 10. TBS_N iterations of the filtered row (DIAGNOSTIC
  # ONLY: a filtered run never banks), each reading /panic and /trap disclosed-divergent on the pinned signature and 0
  # deadlock lines. TBS_LOAD=1 runs nproc busy loops beside them (INFERRED load shape), killed by PID (floor 5).
  LOADPIDS=''
  if [ "$TBS_LOAD" = 1 ]; then
    for i in $(seq "$(nproc)"); do ( while :; do :; done ) & LOADPIDS="$LOADPIDS $!"; done
    trap '[ -n "$LOADPIDS" ] && kill $LOADPIDS 2>/dev/null' EXIT
    stamp "TBS load: $(echo $LOADPIDS | wc -w) busy loops (PIDs$LOADPIDS)"
  fi
  NAMED_NOW='TestTracebackSystem'; EXPECT_V=''   # a filtered run has no banked count: its gates are rc 0, a fresh record, and the name read disclosed
  for i in $(seq "$TBS_N"); do tleg runtime 30m 90m "tbs$i" -test-filter '^TestTracebackSystem$'; disclosed "runtime-tbs$i" TestTracebackSystem/panic; done
  NAMED_NOW=''; EXPECT_V=roster
  [ -n "$LOADPIDS" ] && kill $LOADPIDS 2>/dev/null; LOADPIDS=''
  stamp "TBS-LOOP: iterations=$TBS_N load=$TBS_LOAD; deadlock-lines per iteration (log+record-stderr): $(for i in $(seq "$TBS_N"); do printf '%s+%s ' "$(grep -ac 'all goroutines are asleep' "$R/tests-runtime-tbs$i.log")" "$([ -f "$R/tests-runtime-tbs$i.comparison.json" ] && recdl "$R/tests-runtime-tbs$i.comparison.json" || echo no-record)"; done)"
  # verify round 3: the brief's section 4 calls a deadlock line a mover and the count above was only a stamp. Per
  # iteration: the log count and the fresh record's stderr-tail count must both be 0 (an iteration with no fresh record
  # is already tleg's mover and is not counted twice; an unreadable record is a failed reading).
  for i in $(seq "$TBS_N"); do
    tbl=$(grep -ac 'all goroutines are asleep' "$R/tests-runtime-tbs$i.log" 2>/dev/null)
    [ -f "$R/tests-runtime-tbs$i.comparison.json" ] || { [ "${tbl:-x}" = 0 ] || mover "TBS iteration $i: deadlock lines in the log=${tbl:-unread} (expected 0)"; continue; }
    tbr=$(recdl "$R/tests-runtime-tbs$i.comparison.json")
    [ "${tbl:-x}" = 0 ] && [ "$tbr" = 0 ] || mover "TBS iteration $i: deadlock lines log=${tbl:-unread} record-stderr=$tbr (expected 0 and 0)"
  done
  purge after-tbs
fi
# Verify round 1: the UF reading for BOTH lanes (round 0 stamped it only inside P1's branch; P2 runs runtime/pprof, the
# full runtime row and the filtered runtime runs, and its END line reported tracked changes and deletions only).
ufe=$(git -C "$W" status --porcelain -- src/core | grep -c '^??')
stamp "UF untracked, not-ignored paths under src/core at END (c1-fixture-tracking; EXPECT 0): $ufe $(git -C "$W" status --porcelain -- src/core | grep '^??' | head -n 5 | tr '\n' ' ')"
[ "$ufe" = 0 ] || mover "UF at END: $ufe untracked, not-ignored path(s) under src/core (expected 0)"   # verify round 3: a gate, not only a stamp
stamp "END freshness-fails=$FRESH_FAILS movers=$MOVERS head=$(git -C "$W" rev-parse --short=10 HEAD) tracked-changes=$(git -C "$W" status --porcelain | grep -vc '^??') tracked-deletions=$(git -C "$W" status --porcelain | grep -c '^ D')"
stamp "LINUX LEGS DONE ($LANE, TRAIN M)"
# Verify round 2 (RULED): the driver could not fail. Every expectation was a stamp and the last command an unconditional
# success, so a mover was found only by a lane that read every line. Exit 4 when any MOVER line or any freshness fail
# was stamped: the lines themselves are in SUMMARY.txt ('MOVER:' and 'FRESHNESS FAIL').
[ "$MOVERS" = 0 ] && [ "$FRESH_FAILS" = 0 ] || { stamp "EXIT 4: movers=$MOVERS freshness-fails=$FRESH_FAILS (grep 'MOVER:' and 'FRESHNESS FAIL' in $SUM)"; exit 4; }
exit 0
