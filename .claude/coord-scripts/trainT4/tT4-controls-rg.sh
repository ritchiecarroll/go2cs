#!/usr/bin/env bash
# tT4-controls-rg.sh -- T4-12: run ONLY the regen-apply arms (RG and RGF) of tT4-controls.sh, extracted verbatim from
# the text between its '# --- RG' and '# --- ED' markers, with the kit's own verdict()/w() definitions.
#   OUT=<scratch under coord-scratch/tT4/> bash tT4-controls-rg.sh
set -u
SD=$(cd "$(dirname "$0")" && pwd)
OUT=${OUT:?set OUT to a scratch directory under /h/go2cs-tmp-coord/coord-scratch/tT4/}
case "$OUT" in /h/go2cs-tmp-coord/coord-scratch/tT4/*) ;; *) echo "ABORT: OUT=$OUT"; exit 2 ;; esac
mkdir -p "$OUT"
FAILED=0
eval "$(grep -E '^(verdict|w)\(\)' "$SD/tT4-controls.sh")"
frag=$(awk '/^# -+ RG$/{on=1} /^# -+ ED$/{on=0} on' "$SD/tT4-controls.sh")
[ -n "$frag" ] && printf '%s\n' "$frag" | grep -q 'verdict RGF' && printf '%s\n' "$frag" | grep -q 'verdict RG ' || { echo "ABORT: the RG / RGF arms were not extracted"; exit 2; }
eval "$frag"
echo "T4-CONTROLS-RG DONE failed=$FAILED (arms RG, RGF; scratch: $OUT)"
[ "$FAILED" = 0 ]
