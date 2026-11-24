#!/usr/bin/env bash
# The per-run deadline both M:N scripts share (run-q1-kit.sh, run-mn-closure.sh). Sourced, never run.
#
#   run_deadline SECONDS STACKFILE CMD...
#
# Runs CMD in the background and waits for it, up to SECONDS. CMD must exec the process to watch (`env ... dotnet x.dll`,
# or `sh -c '... exec ...'`), so that its pid is the one the deadline acts on. At the deadline it dumps that process's
# managed stacks to STACKFILE with dotnet-stack (when STACKFILE is set; Q1_STACK names the executable, else the one on
# PATH), sends TERM, sends KILL 10 s later if the process is still alive, and returns 124, the status coreutils timeout
# uses. Otherwise it returns CMD's own status. The caller reports 124 as HUNG and never counts the run as read.
#
# Why the scripts carry their own: until 2026-11-24 the deadline was coreutils timeout or gtimeout when one was on PATH,
# and nothing otherwise. macOS ships neither, and timeout without -k sends only TERM, so on row 15's darwin dry run
# (run 45310508763) closure run 3 hung for 3h17m to the job's wall, with empty output and no stacks. Portable to the
# bash 3.2 macOS ships.

run_deadline() {
    local secs=$1 stackfile=$2
    shift 2
    local tool="${Q1_STACK:-$(command -v dotnet-stack 2>/dev/null || true)}"
    "$@" &
    local pid=$! start=$SECONDS
    while kill -0 "$pid" 2>/dev/null; do
        if [ $((SECONDS - start)) -ge "$secs" ]; then
            if [ -n "$stackfile" ]; then
                if [ -n "$tool" ]; then
                    # Under a storm of thousands of threads dotnet-stack can exit 0 with an empty report (1 of 3 on a
                    # 20M-goroutine MnMixedStorm, 2026-11-24), so an empty report is taken once more and then said.
                    local try
                    for try in 1 2; do
                        "$tool" report -p "$pid" > "$stackfile" 2>&1 || echo "dotnet-stack exited $?" >> "$stackfile"
                        [ -s "$stackfile" ] && break
                        sleep 2
                    done
                    [ -s "$stackfile" ] || echo "dotnet-stack returned an empty report twice" > "$stackfile"
                else
                    echo "no dotnet-stack (Q1_STACK unset and none on PATH): no stacks taken" > "$stackfile"
                fi
            fi
            kill -TERM "$pid" 2>/dev/null
            local k=0
            while kill -0 "$pid" 2>/dev/null && [ $k -lt 10 ]; do sleep 1; k=$((k + 1)); done
            kill -KILL "$pid" 2>/dev/null
            wait "$pid" 2>/dev/null
            return 124
        fi
        sleep 1
    done
    wait "$pid"
}
