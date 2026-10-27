#!/usr/bin/env bash
# Runs the ParkStorm demonstrator (parkstorm/) once on linux and samples it while its goroutines are parked: the
# linux twin of park-storm.ps1, with the same tab-separated result line:
#
#     reserve  n  parked_s  private_mb  working_set_mb  threads  released_s  outcome
#
# private_mb is RssAnon (resident anonymous memory) and working_set_mb is VmRSS, both from /proc/<pid>/status; linux
# has no per-process commit charge, so these are not the same quantities as the windows harness's private bytes and
# working set. Name the platform beside every figure.
#
# Usage: park-storm.sh <exe> <N> [hold seconds=5] [reserve] [max RssAnon GB=16] [timeout seconds=600]
# An empty reserve leaves GO2CS_GOROUTINE_STACK unset (the default reservation).
set -u

exe=$1 n=$2 hold=${3:-5} reserve=${4:-} max_gb=${5:-16} timeout_s=${6:-600}
out=$(mktemp)

if [ -n "$reserve" ]; then
    GO2CS_GOROUTINE_STACK=$reserve "$exe" "$n" "$hold" > "$out" 2>&1 &
else
    env -u GO2CS_GOROUTINE_STACK "$exe" "$n" "$hold" > "$out" 2>&1 &
fi

pid=$!
start=$(date +%s)
outcome=ok
snap_private= snap_ws= snap_threads=

field() { awk -v k="$1:" '$1 == k { print $2 }' "/proc/$pid/status" 2>/dev/null; }

while kill -0 "$pid" 2>/dev/null; do
    sleep 0.5

    if [ -z "$snap_threads" ] && grep -q '^PARKED ' "$out"; then
        snap_private=$(( $(field RssAnon) / 1024 ))
        snap_ws=$(( $(field VmRSS) / 1024 ))
        snap_threads=$(field Threads)
    fi

    anon=$(field RssAnon)
    if [ -n "$anon" ] && [ "$anon" -gt $(( max_gb * 1024 * 1024 )) ]; then outcome=stopped-memory; kill -9 "$pid"; break; fi
    if [ $(( $(date +%s) - start )) -gt "$timeout_s" ]; then outcome=stopped-time; kill -9 "$pid"; break; fi
done

wait "$pid" 2>/dev/null
rc=$?

parked=$(sed -n 's/^PARKED [0-9]* goroutines=[0-9]* in \([0-9.]*\)s.*/\1/p' "$out" | head -1)
released=$(sed -n 's/^RELEASED [0-9]* in \([0-9.]*\)s.*/\1/p' "$out" | head -1)
if [ "$outcome" = ok ] && [ "$rc" -ne 0 ]; then outcome="exit-$rc"; fi

# A run that ended early says how far it got (its last STARTED line) and why (its last lines, on stderr).
if [ "$outcome" != ok ]; then
    reached=$(grep '^STARTED ' "$out" | tail -1)
    outcome="$outcome (${reached:-no STARTED line})"
    tail -3 "$out" | sed 's/^/# /' >&2
fi
rm -f "$out"

printf '%s\t%s\t%s\t%s\t%s\t%s\t%s\t%s\n' "${reserve:-default}" "$n" "$parked" "$snap_private" "$snap_ws" "$snap_threads" "$released" "$outcome"
