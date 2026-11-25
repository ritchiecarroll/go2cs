#!/usr/bin/env bash
# The M:N closure arm (DATA-mn-row7-closure.md): MnHttpClosure, converted with -recurse, built with twins
# (GoTwinsFor=MnHttpClosure), run under the scheduler and compared with Go.
#
#   bash src/tools/mnspike2/run-mn-closure.sh [results-dir]
#   Q1_CLOSURE_PROPS='-p:<Name>=<value> ...' bash src/tools/mnspike2/run-mn-closure.sh [results-dir]
#
# Q1_CLOSURE_PROPS adds MSBuild properties to the closure's build, each `-p:Name=value` with no space in it. It is how a
# caller twins the standard library under the program: GoStdlibTwins set to true there. This script never sets that
# switch itself: until row 10's T1-T5 ruling flips its default, no tracked file may turn it on (repoguard
# TestStdlibTwinsDefaultOff), so it comes from the caller's command line or a dispatch input, run by run.
#
# Steps: build the converter from src/go2cs; convert the program into results-dir/out; add the tree's
# Directory.Build.targets import to the output root (a converted program outside the tree needs it for the twin
# switches); build Release; run Q1_CLOSURE_RUNS times (default 3) under GO2CS_MN=1 GO2CS_MN_READING=1 with GOMAXPROCS as
# the caller set it; compare each run's stdout, READING lines removed, with `go run` of the program on this machine
# (go-output.txt, committed, is the fallback when go run fails). Prints one READING line per run and a summary line.
#
# With the standard library twinned, the build twins every converted package IN PLACE under src/core: a later plain
# build of the same tree rebuilds them. Run it after the Q1 kit, not before.
#
# FAILURE (rc 1) when the converter build, the conversion or the dotnet build fails, or when any run exits non-zero,
# differs from Go or is HUNG. Environment: Q1_GOOS (passed as GoTargetOS), Q1_CONFIG (default Release), Q1_TIMEOUT
# seconds per run (default 300), Q1_CLOSURE_RUNS, Q1_STACK (a dotnet-stack executable; default the one on PATH). go and
# dotnet must be on PATH.
#
# The deadline is the script's own, on every OS (run-deadline.sh). A run still alive at Q1_TIMEOUT has its managed
# stacks dumped with dotnet-stack to run<N>.stack, then gets TERM and, 10 s later, KILL; its READING line says HUNG and
# it counts as bad. go run takes the same deadline, with no stacks.
set -u

here="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
src="$(cd "$here/../.." && pwd)"
prog="$here/MnHttpClosure"
out="${1:-$PWD/mn-closure-results}"
config="${Q1_CONFIG:-Release}"
timeout_s="${Q1_TIMEOUT:-300}"
runs="${Q1_CLOSURE_RUNS:-3}"
mkdir -p "$out"
out="$(cd "$out" && pwd)"
summary="$out/summary.txt"
: > "$summary"
note() { echo "$*" | tee -a "$summary"; }
fail() { note "READING closure FAILURE $*"; exit 1; }

goos_arg=()
[ -n "${Q1_GOOS:-}" ] && goos_arg=("-p:GoTargetOS=$Q1_GOOS")
extra=()
for prop in ${Q1_CLOSURE_PROPS:-}; do
    case "$prop" in
        -p:*=*) [[ "$prop" =~ ^-p:[A-Za-z_][A-Za-z0-9_]*=[A-Za-z0-9_.\;]*$ ]] || { echo "Q1_CLOSURE_PROPS: '$prop' is not -p:Name=value (letters, digits, _ . ;)"; exit 1; }; extra+=("$prop") ;;
        *) echo "Q1_CLOSURE_PROPS: '$prop' is not -p:Name=value"; exit 1 ;;
    esac
done
# shellcheck source=run-deadline.sh
. "$here/run-deadline.sh"
stack_tool="${Q1_STACK:-$(command -v dotnet-stack 2>/dev/null || true)}"

note "closure: src=$src config=$config GOMAXPROCS=${GOMAXPROCS:-unset} GoTargetOS=${Q1_GOOS:-default} props=[${extra[*]+${extra[*]}}] deadline=${timeout_s}s stacks=${stack_tool:-none}"

(cd "$src/go2cs" && go build -o "$out/go2cs" .) > "$out/converter.log" 2>&1 || fail "converter build (converter.log)"

rm -rf "$out/out"
(cd "$prog" && "$out/go2cs" -recurse -go2cspath "$src" "$prog" "$out/out") > "$out/convert.log" 2>&1 || fail "convert (convert.log)"

dbt="$out/out/Directory.Build.targets"
[ -f "$dbt" ] || fail "no Directory.Build.targets in the output root"
# The output root's own targets file, plus an import of the tree's (where the twin switches are defined).
awk '/^<\/Project>/ { print "  <Import Project=\"$(go2csPath)Directory.Build.targets\" Condition=\"Exists('"'"'$(go2csPath)Directory.Build.targets'"'"')\" />" } { print }' "$dbt" > "$dbt.new" && mv "$dbt.new" "$dbt"
grep -q 'go2csPath)Directory.Build.targets' "$dbt" || fail "import not added to $dbt"

proj="$out/out/src/MnHttpClosure/MnHttpClosure.csproj"
[ -f "$proj" ] || fail "no converted project at $proj"
t0=$(date +%s)
dotnet build "$proj" -c "$config" -nologo -v:q -p:go2csPath="$src/" -p:UseSharedCompilation=false \
    ${goos_arg[@]+"${goos_arg[@]}"} -p:GoTwinsFor=MnHttpClosure ${extra[@]+"${extra[@]}"} > "$out/build.log" 2>&1
rc=$?
gd=$(grep -oE "GO2CS00(17|18|21)" "$out/build.log" | sort | uniq -c | tr -s ' ' | tr '\n' ' ')
note "build rc=$rc wall=$(( $(date +%s) - t0 ))s ${gd:+[$gd]}"
[ $rc -eq 0 ] || fail "dotnet build rc=$rc: $(grep -m1 -E ' error ' "$out/build.log" | sed 's/^ *//' | cut -c1-200)"

dll=$(dotnet msbuild "$proj" -nologo -p:Configuration="$config" -p:go2csPath="$src/" ${goos_arg[@]+"${goos_arg[@]}"} -getProperty:TargetPath 2>/dev/null | tr -d '\r')
[ -f "$dll" ] || fail "no assembly at '$dll'"

ref="$out/go.stdout"
if run_deadline "$timeout_s" "" sh -c 'cd "$1" && exec go run .' sh "$prog" > "$ref" 2> "$out/go.stderr"; then
    note "reference: go run on this machine$(cmp -s "$ref" "$prog/go-output.txt" && echo ', equal to go-output.txt' || echo ', DIFFERS from go-output.txt')"
else
    cp "$prog/go-output.txt" "$ref"; note "reference: go run failed (go.stderr); go-output.txt"
fi

bad=0
for r in $(seq 1 "$runs"); do
    run_deadline "$timeout_s" "$out/run$r.stack" sh -c 'cd "$1" && exec env GO2CS_MN=1 GO2CS_MN_READING=1 dotnet "$2"' sh "$(dirname "$dll")" "$dll" \
        > "$out/run$r.stdout" 2> "$out/run$r.stderr"
    c=$?
    if [ $c -eq 124 ]; then
        bad=$((bad + 1))
        note "READING closure run$r HUNG at ${timeout_s}s, killed | stacks: run$r.stack ($(wc -l < "$out/run$r.stack" | tr -d ' ') lines) | stdout $(wc -c < "$out/run$r.stdout" | tr -d ' ') bytes"
        continue
    fi
    grep -v 'READING' "$out/run$r.stdout" | tr -d '\r' > "$out/run$r.cmp"
    if cmp -s "$out/run$r.cmp" "$ref"; then eq=EQUAL; else eq=DIFFER; diff "$ref" "$out/run$r.cmp" > "$out/run$r.diff"; fi
    [ $c -eq 0 ] && [ $eq = EQUAL ] || bad=$((bad + 1))
    note "READING closure run$r exit=$c $eq | $(grep -h 'READING go-scheduler' "$out/run$r.stdout" "$out/run$r.stderr" | head -1 | tr -d '\r')"
done

[ $bad -eq 0 ] && note "READING closure PASS runs=$runs equal=$runs" || fail "runs=$runs bad=$bad"
