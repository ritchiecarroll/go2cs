#!/usr/bin/env bash
# window2.sh : the one-CPU arms again (the first run failed at SetProcessAffinityMask, before any run); was: every timed run of the call-counting-delay record, in one perf window, one after the other:
#   1 steady state, all CPUs     (arms-steady.tsv)
#   2 start-up, all CPUs         (arms-startup.tsv)
#   3 start-up, ONE CPU          (arms-startup.tsv, CCD_AFFINITY=0x1)
#   4 steady state, ONE CPU, the hot phase and TlsHandshake only (arms-steady1.tsv)
# 15 interleaved rounds after a discarded warm-up (drive.py). The bracket: C:\go2cs-perf's file count and newest mtime,
# read before and after; a change voids the window. No runtime variable is inherited (the driver strips the tiering
# ones; DOTNET_ROLL_FORWARD and GOFLAGS are unset here). Writes out-*.txt, data-*.tsv, bracket2.txt, window2.done.
set -u
C=/c/go2cs/scratch/ccd
unset DOTNET_ROLL_FORWARD GOFLAGS
export DOTNET_ROOT=C:/Users/<user>/dotnet10 DOTNET_CLI_TELEMETRY_OPTOUT=1
snap() { echo "$(find /c/go2cs-perf -type f 2>/dev/null | wc -l) $(find /c/go2cs-perf -type f -printf '%T@\n' 2>/dev/null | sort -n | tail -n 1)"; }
b0=$(snap)
echo "before $b0 $(date -u +%H:%M:%SZ)" > "$C/bracket2.txt"
D=$(cygpath -w "$C/drive.py")
CCD_AFFINITY=0x1 py -3 -I -X utf8 "$D" "$(cygpath -w "$C/arms-startup.tsv")" 15 "$(cygpath -w "$C/data-startup1.tsv")" > "$C/out-startup1.txt" 2>&1
echo "startup1 rc=$? $(date -u +%H:%M:%SZ)" >> "$C/bracket2.txt"
CCD_AFFINITY=0x1 py -3 -I -X utf8 "$D" "$(cygpath -w "$C/arms-steady1.tsv")" 15 "$(cygpath -w "$C/data-steady1.tsv")" > "$C/out-steady1.txt" 2>&1
echo "steady1 rc=$? $(date -u +%H:%M:%SZ)" >> "$C/bracket2.txt"
b1=$(snap)
echo "after $b1 $(date -u +%H:%M:%SZ)" >> "$C/bracket2.txt"
[ "$b0" = "$b1" ] && v=HELD || v=VOID
echo "window bracket $v end=$(date -u +%H:%M:%SZ)" > "$C/window2.done"
