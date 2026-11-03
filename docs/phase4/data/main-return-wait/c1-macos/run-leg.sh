#!/bin/bash
# The main-return wait seat's macOS leg (leg-briefs.md, "C1 macOS runner"), run by .github/workflows/c1-mrw-macos.yml on
# a hosted mac runner. It measures, in this order:
#   1. GolibTests MainExit* and ThreadCpuTime* at the tip (ThreadCpuTimeTests is the live CPU-time test);
#   2. R0: the amplifier GoroutinePanicAfterDeferredDone at BASE, JIT and Native AOT, idle and loaded;
#   3. R1: the amplifier at the TIP, the same four arms;
#   4. GoroutineRecoveringWhenMainReturns at the TIP, JIT and AOT, idle and loaded.
# Each program is built once and run directly N times. The load is 32 busy loops; macOS has no affinity call, so the
# program and the loops run unpinned. After every run the script counts the loops still alive.
#
#   run-leg.sh <tip checkout> <base checkout> <rid> <N> <out dir>
#
# Per run it records: arm, run, exit code, stderr bytes, whether stderr holds the panic report, whether stdout holds
# "recovered", spinners alive, wall ms. A run LOSES (amplifier) when it exits 0 with an empty stderr; it passes when it
# exits 2 with the report. A recovering run is LATE when it printed "recovered".
set -u
TIP=$1; BASE=$2; RID=$3; N=$4; OUT=$5
AMP=GoroutinePanicAfterDeferredDone
REC=GoroutineRecoveringWhenMainReturns
REPORT='panic: panic after deferred Done'
mkdir -p "$OUT"
LOG=$OUT/leg.log
say() { echo "$*" | tee -a "$LOG"; }

# A run gets at most 120 s; macOS has no `timeout`, and perl's alarm survives exec.
bounded() { perl -e 'alarm shift; exec @ARGV' 120 "$@"; }

say "== host: $(sysctl -n machdep.cpu.brand_string 2>/dev/null || grep -m1 'model name' /proc/cpuinfo | cut -d: -f2) cpus=$(sysctl -n hw.ncpu 2>/dev/null || nproc) rid=$RID os=$(sw_vers -productVersion 2>/dev/null || uname -sr)"
say "== dotnet: $(dotnet --version); runtime: $(dotnet --list-runtimes | grep Microsoft.NETCore.App | tail -1); bash $BASH_VERSION"
say "== tip $(git -C "$TIP" rev-parse --short=10 HEAD), base $(git -C "$BASE" rev-parse --short=10 HEAD), N=$N, GoTargetOS=${GoTargetOS:-UNSET}"
# Without GoTargetOS every build links the windows flavor of the corpus, and the program dies in its module
# initializer on any other host, exiting 2 -- which reads like the amplifier's pass. Refuse to run that way.
[ -n "${GoTargetOS:-}" ] || { say "REFUSED: GoTargetOS is unset"; exit 3; }

# ---- 1. GolibTests at the tip ----------------------------------------------------------------------------------------
# GolibTests runs WITHOUT GoTargetOS: under darwin it does not compile (NativeRootedFieldIdentityTests holds
# RawSockaddrInet4.Family as ж<ushort>, a byte on darwin). MainExit and ThreadCpuTime are golib, which chooses its
# thread-CPU-time reader at run time (OperatingSystem.IsMacOS -> thread_info), so the darwin path is what runs.
say "== GolibTests (MainExit*, ThreadCpuTime*) at the tip, GoTargetOS unset"
env -u GoTargetOS dotnet test "$TIP/src/tests/GolibTests/GolibTests.csproj" \
  --filter "FullyQualifiedName~MainExit|FullyQualifiedName~ThreadCpuTime" \
  --logger "console;verbosity=normal" > "$OUT/golibtests.log" 2>&1
say "golibtests exit=$? $(grep -E 'Passed!|Failed!|Total tests|Passed:|Failed:' "$OUT/golibtests.log" | tail -3 | tr '\n' ' ')"
grep -E '^[[:space:]]+(Passed|Failed|Skipped) ' "$OUT/golibtests.log" | sed 's/^ *//' | sort | uniq -c | tee -a "$LOG"

# ---- builds ------------------------------------------------------------------------------------------------------------
build() { # tree project runtime -> prints the executable path
  local tree=$1 proj=$2 runtime=$3 csproj="$1/src/tests/Behavioral/$2/$2.csproj"
  if [ "$runtime" = jit ]; then
    dotnet build "$csproj" -c Release -p:go2csPath="$tree/src/" > "$OUT/build-$4-$proj-jit.log" 2>&1 || return 1
    echo "$tree/src/tests/Behavioral/$proj/bin/Release/net10.0/$proj"
  else
    # PublishAot goes in the PROJECT, not on the command line: a global property reaches every referenced project,
    # and the netstandard2.0 source generator then fails with NETSDK1207. The file is restored straight after.
    perl -0pi -e 's#(<OutputType>Exe</OutputType>)#$1\n    <PublishAot>true</PublishAot>\n    <PublishReadyToRun>false</PublishReadyToRun>#' "$csproj"
    grep -q '<PublishAot>true</PublishAot>' "$csproj" || { git -C "$tree" checkout -- "$csproj"; return 1; }
    dotnet publish "$csproj" -c Release -r "$RID" -p:go2csPath="$tree/src/" -o "$OUT/aot-$4-$proj" \
      > "$OUT/build-$4-$proj-aot.log" 2>&1; local rc=$?
    git -C "$tree" checkout -- "$csproj"
    [ $rc = 0 ] || return 1
    echo "$OUT/aot-$4-$proj/$proj"
  fi
}
# macOS ships bash 3.2, which has no associative arrays: one variable per executable instead.
exe_key() { echo "EXE_$1" | tr -c 'A-Za-z0-9_\n' '_'; }
set_exe() { eval "$(exe_key "$1")=\"\$2\""; }
get_exe() { eval "echo \"\${$(exe_key "$1"):-}\""; }
for spec in "base $BASE $AMP" "tip $TIP $AMP" "tip $TIP $REC"; do
  set -- $spec
  for rt in jit aot; do
    t0=$(date +%s)
    if exe=$(build "$2" "$3" $rt "$1"); then
      set_exe "$1-$3-$rt" "$exe"
      say "built $1 $3 $rt in $(( $(date +%s) - t0 ))s"
    else
      say "BUILD FAILED: $1 $3 $rt (see build-$1-$3-$rt.log)"; tail -15 "$OUT/build-$1-$3-$rt.log" | tee -a "$LOG"
    fi
  done
done

# ---- the load ----------------------------------------------------------------------------------------------------------
SPIN=()
start_load() { SPIN=(); for i in $(seq 32); do yes > /dev/null & SPIN+=($!); done; sleep 2; }
stop_load() { kill "${SPIN[@]}" 2>/dev/null; wait "${SPIN[@]}" 2>/dev/null; SPIN=(); }
alive() { local n=0; for p in "${SPIN[@]}"; do kill -0 "$p" 2>/dev/null && n=$((n + 1)); done; echo $n; }

# ---- one arm: N direct runs of one executable ---------------------------------------------------------------------------
arm() { # name exe load
  local name=$1 exe=$2 load=$3 tsv="$OUT/$1.tsv"
  [ -n "$exe" ] || { say "ARM $name: NOT MEASURED (no executable)"; return; }
  [ "$load" = n32 ] && start_load
  printf 'run\texit\tstderr_bytes\treport\trecovered\tspinners\twall_ms\tinvalid\n' > "$tsv"
  for r in $(seq "$N"); do
    local t0 t1 rc
    t0=$(perl -MTime::HiRes=time -e 'printf "%d", time*1000')
    bounded "$exe" > "$OUT/run.out" 2> "$OUT/run.err"; rc=$?
    t1=$(perl -MTime::HiRes=time -e 'printf "%d", time*1000')
    local sp=-; [ "$load" = n32 ] && sp=$(alive)
    printf '%d\t%d\t%d\t%d\t%d\t%s\t%d\t%d\n' "$r" "$rc" "$(wc -c < "$OUT/run.err" | tr -d ' ')" \
      "$(grep -c -F "$REPORT" "$OUT/run.err")" "$(grep -c '^recovered$' "$OUT/run.out")" "$sp" $((t1 - t0)) \
      "$(grep -c -E 'TypeInitializationException|is running the "[a-z]+" build of the converted' "$OUT/run.err")" >> "$tsv"
    [ -e "$OUT/$name.first-run.err" ] || { cp "$OUT/run.out" "$OUT/$name.first-run.out"; cp "$OUT/run.err" "$OUT/$name.first-run.err"; }
    # Keep the first failing run's whole output for the record.
    if [ ! -e "$OUT/$name.first-odd.err" ] && { [ "$rc" = 0 ] && [ ! -s "$OUT/run.err" ] || grep -q '^recovered$' "$OUT/run.out"; }; then
      cp "$OUT/run.out" "$OUT/$name.first-odd.out"; cp "$OUT/run.err" "$OUT/$name.first-odd.err"
    fi
  done
  [ "$load" = n32 ] && stop_load
  # Summary: losses (exit 0, empty stderr), passes (exit 2 with the report), late prints, runs with every spinner alive.
  awk -F'\t' -v A="$name" -v L="$load" 'NR>1 {n++; if ($2==0 && $3==0) loss++; if ($2==2 && $4>0) pass++; if ($5>0) late++;
       if ($6=="32") full++; if ($6!="-" && $6!="32") short++; if ($8>0) bad++; w+=$7; codes[$2]++}
       END {c=""; for (k in codes) c=c k "x" codes[k] " "; printf "ARM %s load=%s N=%d loss=%s pass=%s late=%d invalid=%d spinners_all_alive=%s exits=[%s] mean_ms=%d\n",
            A, L, n, (A ~ /^r2-/ ? "-" : loss), (A ~ /^r2-/ ? "-" : pass), late, bad, (L=="n32" ? (short ? "NO(" short " short)" : "yes " full "/" n) : "-"), c, w/n}' "$tsv" | tee -a "$LOG"
}

for stage in "r0 base $AMP" "r1 tip $AMP" "r2 tip $REC"; do
  set -- $stage
  for rt in jit aot; do
    for load in idle n32; do
      arm "$1-$rt-$load" "$(get_exe "$2-$3-$rt")" "$load"
    done
  done
done
rm -f "$OUT/run.out" "$OUT/run.err"
say "== done"
