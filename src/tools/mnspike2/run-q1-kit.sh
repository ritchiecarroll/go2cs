#!/usr/bin/env bash
# The M:N Q1 kit (DESIGN-mn-twin-set.md 12.5, Q1): builds the kit's programs with twins, plain and with the old
# interface switch pair (ifhall), runs every build, and compares each twin and ifhall run with its plain run.
#
#   bash src/tools/mnspike2/run-q1-kit.sh [results-dir]
#
# Builds one at a time, Release, in place. Runs with GOMAXPROCS as the caller set it. Writes, per build, the build log
# and, per run, stdout, stderr and the exit code, then one summary (summary.txt) and one READING line.
#
# Arms:
#   twins   GoTwinsFor=<project>, run under GO2CS_MN=1.
#   plain   no twin switch, run with GO2CS_MN unset.
#   ifhall  GoTwinsArm=true GoTwinsForIface=hidden GoTwinsForIfaceAll=true (the switch pair the default replaced in
#           7a), run under GO2CS_MN=1. Its assembly is expected to equal the twins build's, byte for byte.
#   hand    MnStormTwin only: a hand-twinned fixture with no Go main for the generated entry, so it builds with no
#           twin switch; its hand twins run under GO2CS_MN=1. Counted with the twin runs.
#
# A compare is a twin (or ifhall) run against the plain run of the same program: stdout and exit code must be equal.
# The guard: fewer than Q1_MIN_BUILDS green builds (default 20) or fewer than Q1_MIN_COMPARES compares (default 8)
# is a FAILURE, never a quiet pass, and so is any difference, or a twins build whose assembly equals its plain build's
# (the twin switch did not reach the compile).
#
# Environment: Q1_CONFIG (default Release), Q1_TIMEOUT seconds per run (default 300), Q1_GOOS passed as GoTargetOS
# (default: none), Q1_STACK (a dotnet-stack executable; default the one on PATH). dotnet must be on PATH.
#
# A run still alive at Q1_TIMEOUT is HUNG (run-deadline.sh): its managed stacks go to <tag>.stack, it is killed, it
# takes no compare, and the kit fails.
set -u

here="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
src="$(cd "$here/../.." && pwd)"
out="${1:-$PWD/q1-kit-results}"
config="${Q1_CONFIG:-Release}"
timeout_s="${Q1_TIMEOUT:-300}"
min_builds="${Q1_MIN_BUILDS:-20}"
min_compares="${Q1_MIN_COMPARES:-8}"
mkdir -p "$out"
out="$(cd "$out" && pwd)"

# program:arms. Paths are searched in tests/Behavioral, then tools/mnspike2.
kit=(
    "MnAotGeneric:twins,plain,ifhall"
    "MnIfaceTwin:twins,plain,ifhall"
    "MnBoxAlias:twins,plain"
    "MnFuncPair:twins,plain"
    "MnMixedStorm:twins,plain"
    "MnCost:twins,plain"
    "MnBlockedIO:twins"
    "MnDeepUnwind:twins"
    "MnMainExit:twins"
    "MnParkedCallers:twins"
    "MnContext:twins"
    "MnNativeWrap:twins"
    "MnStormTwin:hand"
    "MnLaterArg:twins"
)

goos_arg=()
[ -n "${Q1_GOOS:-}" ] && goos_arg=("-p:GoTargetOS=$Q1_GOOS")

# Portable to the bash 3.2 macOS ships: no associative arrays, and no bare expansion of an empty array under set -u.
if command -v sha256sum >/dev/null 2>&1; then sha() { sha256sum "$1" | cut -d' ' -f1; }; else sha() { shasum -a 256 "$1" | cut -d' ' -f1; }; fi
# shellcheck source=run-deadline.sh
. "$here/run-deadline.sh"

builds=0; green=0; runs=0; compares=0; differs=0; stale=0; hung=0
failed=(); differing=(); summary="$out/summary.txt"
: > "$summary"

note() { echo "$*" | tee -a "$summary"; }

arm_props() {
    case "$2" in
        twins)  echo "-p:GoTwinsFor=$1" ;;
        ifhall) echo "-p:GoTwinsFor=$1 -p:GoTwinsArm=true -p:GoTwinsForIface=hidden -p:GoTwinsForIfaceAll=true" ;;
        *)      echo "" ;;
    esac
}

note "Q1 kit: src=$src config=$config GOMAXPROCS=${GOMAXPROCS:-unset} GoTargetOS=${Q1_GOOS:-default}"

for entry in "${kit[@]}"; do
    prog="${entry%%:*}"; arms="${entry#*:}"
    dir=""
    for cand in "$src/tests/Behavioral/$prog" "$src/tools/mnspike2/$prog"; do
        [ -d "$cand" ] && { dir="$cand"; break; }
    done
    s_twins=""; s_plain=""; s_ifhall=""; s_hand=""; c_twins=""; c_plain=""; c_ifhall=""; c_hand=""

    for arm in ${arms//,/ }; do
        builds=$((builds + 1))
        tag="$prog.$arm"
        proj="$dir/$prog.csproj"
        if [ -z "$dir" ] || [ ! -f "$proj" ]; then
            note "BUILD $tag MISSING (no $prog.csproj: not converted)"
            failed+=("$tag:missing"); continue
        fi

        # shellcheck disable=SC2046
        dotnet build "$proj" -c "$config" -nologo -v:q -p:go2csPath="$src/" -p:UseSharedCompilation=false \
            ${goos_arg[@]+"${goos_arg[@]}"} $(arm_props "$prog" "$arm") > "$out/$tag.build.log" 2>&1
        rc=$?
        gd=$(grep -oE "GO2CS00(17|18|21)" "$out/$tag.build.log" | sort | uniq -c | tr -s ' ' | tr '\n' ' ')
        if [ $rc -ne 0 ]; then
            first=$(grep -m1 -E " error " "$out/$tag.build.log" | sed 's/^ *//' | cut -c1-200)
            note "BUILD $tag RED rc=$rc ${gd:+[$gd]} $first"
            failed+=("$tag:rc=$rc"); continue
        fi
        green=$((green + 1))

        dll=$(dotnet msbuild "$proj" -nologo -p:Configuration="$config" -p:go2csPath="$src/" ${goos_arg[@]+"${goos_arg[@]}"} -getProperty:TargetPath 2>/dev/null | tr -d '\r')
        if [ ! -f "$dll" ]; then
            note "BUILD $tag GREEN but no assembly at '$dll'"
            failed+=("$tag:no-assembly"); green=$((green - 1)); continue
        fi
        h=$(sha "$dll")
        mn=""; [ "$arm" != "plain" ] && mn=1
        runs=$((runs + 1))
        if [ -n "$mn" ]; then
            run_deadline "$timeout_s" "$out/$tag.stack" env GO2CS_MN=1 dotnet "$dll" > "$out/$tag.stdout" 2> "$out/$tag.stderr"
        else
            run_deadline "$timeout_s" "$out/$tag.stack" env -u GO2CS_MN dotnet "$dll" > "$out/$tag.stdout" 2> "$out/$tag.stderr"
        fi
        c=$?
        if [ $c -eq 124 ]; then
            # A hung run is never read: it takes no compare, and the kit fails.
            hung=$((hung + 1)); failed+=("$tag:hung"); c=HUNG
            note "BUILD $tag GREEN sha=${h:0:12} ${gd:+[$gd]} RUN HUNG at ${timeout_s}s, killed | stacks: $tag.stack ($(wc -l < "$out/$tag.stack" | tr -d ' ') lines)"
        else
            note "BUILD $tag GREEN sha=${h:0:12} ${gd:+[$gd]} RUN exit=$c lines=$(wc -l < "$out/$tag.stdout" | tr -d ' ')"
        fi
        eval "s_$arm=\$h; c_$arm=\$c"
    done

    # Compares: each twin-side run against the plain run of the same program.
    if [ -n "$c_plain" ]; then
        for arm in twins ifhall; do
            eval "ca=\$c_$arm; sa=\$s_$arm"
            [ -z "$ca" ] && continue
            if [ "$ca" = HUNG ] || [ "$c_plain" = HUNG ]; then
                note "UNREAD $prog.$arm vs $prog.plain: a run HUNG"; continue
            fi
            compares=$((compares + 1))
            if [ "$sa" = "$s_plain" ]; then
                note "STALE $prog.$arm: assembly equals the plain build's"; stale=$((stale + 1))
            fi
            if cmp -s "$out/$prog.$arm.stdout" "$out/$prog.plain.stdout" && [ "$ca" = "$c_plain" ]; then
                note "EQUAL $prog.$arm = $prog.plain"
            else
                note "DIFFER $prog.$arm vs $prog.plain (exit $ca vs $c_plain)"
                diff "$out/$prog.plain.stdout" "$out/$prog.$arm.stdout" > "$out/$prog.$arm.diff"
                differs=$((differs + 1)); differing+=("$prog.$arm")
            fi
        done
        if [ -n "$s_ifhall" ] && [ -n "$s_twins" ]; then
            [ "$s_ifhall" = "$s_twins" ] && note "SAME-SHA $prog: ifhall = twins" || note "NOTE $prog: ifhall sha differs from twins"
        fi
    fi
done

verdict=PASS
[ $green -lt "$min_builds" ] && verdict=FAILURE
[ $compares -lt "$min_compares" ] && verdict=FAILURE
[ $differs -ne 0 ] && verdict=FAILURE
[ $stale -ne 0 ] && verdict=FAILURE
[ $hung -ne 0 ] && verdict=FAILURE

note "READING Q1 $verdict builds=$green/$builds runs=$runs compares=$compares differ=$differs stale=$stale hung=$hung (guard: >=$min_builds builds, >=$min_compares compares) failed=[${failed[*]+${failed[*]}}] differing=[${differing[*]+${differing[*]}}]"
[ "$verdict" = PASS ]
