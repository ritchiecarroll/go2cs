#!/usr/bin/env bash
# chain-check.sh : MnGoschedFair's in-repo gate (P3's SHOULD 1 on the Gosched-park seat). The fairness row tests
# GoScheduler.MaxRunNextChain only while the ping-pong pair really chains through the run-next slot. A change that
# makes one side synchronous again (the row's first version: a `for range` ponger, a synchronous foreach in a twin,
# N11) leaves the row green whatever the bound is. So this script ASSERTS the chain formed, from the scheduler's own
# READING line: run-next-steps >= 100 x gosched-yields (about 128 per yield with the bound at work). A vacuous row
# reads CHAIN-FAIL by name; a hang (the bound lifted) reads CHAIN-FAIL at the deadline; so does an output unlike Go's.
#
# Usage, from anywhere: bash src/tools/mnspike2/MnGoschedFair/chain-check.sh   (exit 0: chain formed, EQUAL to Go)
# Needs dotnet and go on PATH. Builds the fixture with generated twins (GoTwinsFor) against this checkout's src/.
# Portable to the os-matrix hosts (macOS has no `timeout`): the deadline is a poll loop. MN_CHAIN_DEADLINE: seconds.
set -u
here=$(cd "$(dirname "$0")" && pwd)
src=$(cd "$here/../../.." && pwd)
if command -v cygpath > /dev/null 2>&1; then srcpath=$(cygpath -m "$src"); else srcpath=$src; fi
work=${RUNNER_TEMP:-${TMPDIR:-/tmp}}/mn-chain-check
mkdir -p "$work"
deadline=${MN_CHAIN_DEADLINE:-120}

fail() { echo "READING mn-chain CHAIN-FAIL $*"; exit 1; }

# Go's reference output, one P.
(cd "$here" && GOMAXPROCS=1 go run .) > "$work/go-out.txt" 2> "$work/go-err.txt" ||
    fail "go run failed: $(head -n 3 "$work/go-err.txt" | tr '\n' ' ')"

# The C# build, with twins.
(cd "$here" && GoTwinsFor=MnGoschedFair dotnet build MnGoschedFair.csproj -c Release -nologo -v q \
    "-p:go2csPath=$srcpath/") > "$work/build.txt" 2>&1 ||
    fail "build failed: $(grep -m 3 ' error ' "$work/build.txt" | cut -c1-200 | tr '\n' ' ')"
dll=$here/bin/Release/net10.0/MnGoschedFair.dll
[ -f "$dll" ] || fail "no build output at $dll"

# One P, no injection: a carrier the monitor injects for a cold start-up step would serve the yielder whatever the
# bound is.
GO2CS_MN=1 GO2CS_MN_PROCS=1 GO2CS_MN_MAXINJECT=0 GO2CS_MN_READING=1 dotnet "$dll" \
    > "$work/cs-out.txt" 2> "$work/cs-err.txt" &
pid=$!
waited=0
while kill -0 $pid 2> /dev/null; do
    if [ $waited -ge $((deadline * 4)) ]; then
        kill -9 $pid 2> /dev/null
        wait $pid 2> /dev/null
        fail "no exit in ${deadline}s: the yielder starved (MaxRunNextChain not at work?)"
    fi
    sleep 0.25
    waited=$((waited + 1))
done
wait $pid
code=$?
[ $code -eq 0 ] || fail "exit $code: $(head -n 3 "$work/cs-err.txt" | cut -c1-200 | tr '\n' ' ')"

reading=$(grep -a 'READING go-scheduler' "$work/cs-err.txt" | tail -n 1 | tr -d '\r')
yields=$(echo "$reading" | grep -o 'gosched-yields=[0-9]*' | cut -d= -f2)
steps=$(echo "$reading" | grep -o 'run-next-steps=[0-9]*' | cut -d= -f2)
[ -n "$yields" ] && [ -n "$steps" ] || fail "no gosched-yields/run-next-steps in the READING line: [$reading]"
[ "$yields" -gt 0 ] || fail "gosched-yields=0: the yielder never parked (Gosched not twinned?)"
[ "$steps" -ge $((100 * yields)) ] ||
    fail "run-next=$steps yields=$yields (want >= 100 x yields): the pair did not chain, a vacuous row"

cs=$(tr -d '\r' < "$work/cs-out.txt")
go=$(tr -d '\r' < "$work/go-out.txt")
[ "$cs" = "$go" ] || fail "output DIFFERS from Go: cs=[$(echo "$cs" | tr '\n' ';')] go=[$(echo "$go" | tr '\n' ';')]"
echo "READING mn-chain OK run-next=$steps yields=$yields EQUAL-TO-GO"
