#!/usr/bin/env bash
# coord-train24-suite-read.sh -- the FULL behavioral suite's honest read for train 24: every failing project by name, whether or not
# the assembly's KNOWN_RED allowance (a train-23 residue: ReflectArrayOf) would have hidden it. Run after the suite leg's stamp.
set -u
SP="C:/Projects/go2cs/.claude/coord-scripts"
F=$(ls -t "$SP"/coord-train24-runner-*.log 2>/dev/null | head -1)
[ -n "$F" ] || { echo "no runner log"; exit 2; }
echo "runner log: $F ($(wc -c < "$F") bytes, NUL=$(head -c 400 "$F" | tr -d -c '\000' | wc -c))"
tr -d '\000' < "$F" > "$F.clean"
echo "summary: $(grep -aE '^(PASS|FAIL)\s+\(' "$F.clean" | tail -1)"
echo "phases:  $(grep -aE '^\[(Transpile|Compile|Target|Output)\]' "$F.clean" | tr '\n' ' ' | cut -c1-300)"
fails=$(grep -aE '^\s{2}[A-Za-z0-9_]+ \[' "$F.clean" | sort -u)
n=$(echo "$fails" | grep -c .)
echo "failing projects (ALL, known-red allowance ignored): $n"
[ "$n" != "0" ] && echo "$fails"
echo "ReflectArrayOf explicitly: $(grep -acE '^\s{2}ReflectArrayOf \[' "$F.clean") fail line(s)"
rm -f "$F.clean"
[ "$n" = "0" ] && exit 0 || exit 1
