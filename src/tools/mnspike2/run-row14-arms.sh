#!/usr/bin/env bash
# M:N row 14 (G's 215035Z hand-off; the box-twin record's section 12 and 12.1): MnHttpClosure with its closure twinned,
# timed under the scheduler on ONE .NET box, three arms in one invocation:
#
#   d1  the shipping default (byref-param on)            the scored arm
#   n1  arm N: -p:GoTwinsArm=true -p:GoTwinsBoxParam=false  the base the predictions are stated against
#   d2  the shipping default again, into its own folder   with d1, the A/A pair (the box's own spread)
#
#   bash src/tools/mnspike2/run-row14-arms.sh [results-root]
#
# Every arm is run-mn-closure.sh with GOMAXPROCS=12 (unless the caller set it) and Q1_CLOSURE_RUNS=5, so each one
# converts, builds Release and runs the closure 5 times under GO2CS_MN=1 GO2CS_MN_READING=1, comparing each run with Go.
# This script adds what the row needs on top: the arm switches, a purge before every arm, the bytes, the twin reports,
# and one readings file, row14-readings.md, in the shape of docs/phase4/DATA-mn-row14-template.md.
#
# How the closure is twinned comes from the CALLER, never from this file: ROW14_TWIN_PROPS holds the MSBuild properties
# every arm adds, each -p:Name=value. Row 14 twins the whole closure through the standard library switch, whose
# spelling turned on may appear in no tracked script (repoguard TestStdlibTwinsDefaultOff); the command line to pass is
# in docs/phase4/DATA-mn-row14-template.md. ROW14_TWIN_PROPS=none twins only MnHttpClosure itself (run-mn-closure.sh's
# own -p:GoTwinsFor=MnHttpClosure). Unset, the script stops and says so.
#
# Floors (DESIGN-mn-box-twin.md 12.1): one closure job at a time on the box; at least ROW14_DISK_GB free (default 25)
# before every arm; run from a clone at a committed SHA that nothing edits while the arms run. Before every arm the
# script deletes bin, obj and Generated under THIS tree's src/core (a twinned build rewrites them in place) and the
# arm's own output; it deletes nothing else.
#
# PDB-path rule: go.*.dll embeds each PDB path, so byte totals compare only between paths of equal length. The arm
# folders are all two characters (d1, n1, d2) under one root in one tree, so the three totals compare.
#
# Exit: 0 when all three arms ran (the scoring lines say MET or MISSED; a MISS is a reading, not a script failure);
# 1 when an arm failed (build, conversion, a run that differed from Go or hung) or a floor was not met.
# Environment: ROW14_RUNS (default 5), ROW14_TWIN_PROPS (required), ROW14_DISK_GB (25), and run-mn-closure.sh's own
# Q1_TIMEOUT, Q1_GOOS, Q1_CONFIG, Q1_STACK. go and dotnet must be on PATH.
set -u

here="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
src="$(cd "$here/../.." && pwd)"
root="${1:-$PWD/row14-results}"
runs="${ROW14_RUNS:-5}"
twins="${ROW14_TWIN_PROPS-}"
floor_gb="${ROW14_DISK_GB:-25}"
config="${Q1_CONFIG:-Release}"
export GOMAXPROCS="${GOMAXPROCS:-12}"

mkdir -p "$root"
root="$(cd "$root" && pwd)"
log="$root/row14.log"
: > "$log"
say() { echo "$*" | tee -a "$log"; }
die() { say "ROW14 FAILURE $*"; exit 1; }

command -v dotnet > /dev/null || die "dotnet is not on PATH"
command -v go > /dev/null || die "go is not on PATH"
case "$twins" in
    "") die "ROW14_TWIN_PROPS is unset: pass the twin properties (docs/phase4/DATA-mn-row14-template.md), or none" ;;
    none) twin_props="" ;;
    *) twin_props="$twins" ;;
esac
[ -n "${GO2CS_MN:-}${GoTwinsFor:-}${GoStdlibTwins:-}" ] && die "GO2CS_MN, GoTwinsFor or GoStdlibTwins is set in the environment; the arms set their own"

cores=$(getconf _NPROCESSORS_ONLN 2>/dev/null || sysctl -n hw.ncpu 2>/dev/null || echo "?")
sha=$(git -C "$src" rev-parse HEAD 2>/dev/null || echo "?")
dirty=$(git -C "$src" status --porcelain 2>/dev/null | grep -v '^??' | wc -l | tr -d ' ')
say "row14: tree=$src sha=$sha tracked-changes=$dirty cores=$cores GOMAXPROCS=$GOMAXPROCS runs=$runs twins=$twins config=$config"
say "row14: host $(uname -sm); $(dotnet --version 2>/dev/null | head -1 | sed 's/^/.NET SDK /'); $(go version 2>/dev/null)"
[ "$dirty" = "0" ] || say "row14: WARNING the tree has $dirty tracked change(s): the readings name a SHA the tree does not match"
[ "$cores" != "?" ] && [ "$cores" -lt "$GOMAXPROCS" ] && say "row14: WARNING $cores cores under GOMAXPROCS=$GOMAXPROCS: PB-1 and peak threads are host-sensitive; name the box in the record"

free_gb() { df -Pk "$1" | awk 'NR == 2 { printf "%d", $4 / 1048576 }'; }

purge() {
    # bin, obj and Generated under this tree's src/core only. Nothing tracked lives in them (checked below).
    find "$src/core" -type d \( -name bin -o -name obj -o -name Generated \) -prune -exec rm -rf {} + 2>/dev/null
    rm -rf "$1"
}
tracked=$(git -C "$src" ls-files -- src/core 2>/dev/null | grep -cE '/(bin|obj|Generated)/' || true)
[ "${tracked:-0}" = "0" ] || die "src/core tracks $tracked file(s) under bin/obj/Generated; the purge would delete them"

median() { sort -n | awk '{ v[NR] = $1 } END { if (NR == 0) { print "?"; exit } m = int((NR + 1) / 2); if (NR % 2) print v[m]; else printf "%.1f\n", (v[m] + v[m + 1]) / 2 }'; }
field() { sed -n "s/.* $1=\([0-9][0-9]*\).*/\1/p"; }

arm() {
    local name=$1 props=$2 dir="$root/$1"
    local gb
    gb=$(free_gb "$root")
    [ "$gb" -ge "$floor_gb" ] || die "arm $name: ${gb} GB free under $root, the floor is ${floor_gb} GB"
    say ""
    say "== arm $name: props=[$props] free=${gb}GB"
    purge "$dir"
    mkdir -p "$dir"
    local stamp="$dir/stamp"
    : > "$stamp"
    sleep 1
    local t0=$SECONDS
    Q1_CLOSURE_RUNS="$runs" Q1_CLOSURE_PROPS="$props -p:EmitCompilerGeneratedFiles=true" \
        bash "$here/run-mn-closure.sh" "$dir" > "$dir/closure.log" 2>&1
    local rc=$?
    grep -E '^(closure:|build rc=|reference:|READING closure)' "$dir/summary.txt" 2>/dev/null | sed 's/^/   /' | tee -a "$log"
    say "   arm $name: run-mn-closure.sh rc=$rc in $((SECONDS - t0))s"
    [ $rc -eq 0 ] || die "arm $name: run-mn-closure.sh rc=$rc (see $dir/summary.txt and $dir/closure.log)"

    # Per-run readings: handoffs + thread-blocks (PB-1's total), peak OS threads, wall.
    grep '^READING closure run' "$dir/summary.txt" > "$dir/runs.txt"
    : > "$dir/total.txt"; : > "$dir/tb.txt"; : > "$dir/threads.txt"; : > "$dir/wall.txt"
    while IFS= read -r line; do
        local h t p w
        h=$(echo "$line" | field handoffs); t=$(echo "$line" | field thread-blocks)
        p=$(echo "$line" | field peak-os-threads); w=$(echo "$line" | sed -n 's/.* wall=\([0-9][0-9]*\)ms.*/\1/p')
        [ -n "$h" ] && [ -n "$t" ] && [ -n "$p" ] || die "arm $name: a run line carries no scheduler reading: $line"
        echo $((h + t)) >> "$dir/total.txt"; echo "$t" >> "$dir/tb.txt"; echo "$p" >> "$dir/threads.txt"; echo "${w:-0}" >> "$dir/wall.txt"
    done < "$dir/runs.txt"

    # PB-4: go.*.dll beside the program's assembly, summed.
    local bindir
    bindir=$(dirname "$(find "$dir/out/src/MnHttpClosure/bin" -name MnHttpClosure.dll -path "*$config*" 2>/dev/null | head -1)")
    [ -d "$bindir" ] || die "arm $name: no MnHttpClosure.dll under $dir/out/src/MnHttpClosure/bin"
    ls -l "$bindir"/go.*.dll 2>/dev/null | awk '{ s += $5; n++ } END { print n + 0, s + 0 }' > "$dir/bytes.txt"

    # Host-independent: the twin reports this arm's build wrote, summed (12.1's report lines). Kept whole for the reader.
    find "$src/core" "$dir/out" -name 'go2cs.twins.report.g.cs' -newer "$stamp" 2>/dev/null > "$dir/reports.list"
    xargs cat < "$dir/reports.list" 2>/dev/null | grep -E '^// (candidates=|await-sites=|iface:|forwarders-sync=|refused |box:)' > "$dir/reports.txt"
    {
        echo "$(wc -l < "$dir/reports.list" | tr -d ' ')"
        grep -o 'twinned=[0-9]*' "$dir/reports.txt" | awk -F= '{ s += $2 } END { print s + 0 }'
        grep -o 'refused byref-param=[0-9]*' "$dir/reports.txt" | awk -F= '{ s += $2 } END { print s + 0 }'
        grep '^// box:' "$dir/reports.txt" | grep -o 'plain=[0-9]*' | awk -F= '{ s += $2 } END { print s + 0 }'
    } > "$dir/twins.txt"
}

[ "$twins" != none ] && say "row14: a standard-library twin switch builds every src/core package in place; a later plain build of this tree rebuilds them."
arm d1 "$twin_props"
arm n1 "$twin_props -p:GoTwinsArm=true -p:GoTwinsBoxParam=false"
arm d2 "$twin_props"

# ---- the readings ----
row() {
    local a=$1 d="$root/$1"
    local tot tb th wl n bytes rep tw rb np
    tot=$(median < "$d/total.txt"); tb=$(sort -n "$d/tb.txt" | tail -1); th=$(sort -n "$d/threads.txt" | tail -1)
    wl=$(median < "$d/wall.txt")
    read -r n bytes < "$d/bytes.txt"
    { read -r rep; read -r tw; read -r rb; read -r np; } < "$d/twins.txt"
    echo "| $a | $(tr '\n' ' ' < "$d/total.txt")| $tot | $tb | $(tr '\n' ' ' < "$d/threads.txt")| $th | $(tr '\n' ' ' < "$d/wall.txt")| $wl | $n | $bytes | $rep | $tw | $rb | $np |"
}
md="$root/row14-readings.md"
{
    echo "# Row 14 readings"
    echo
    sed -n '1,2p' "$log" | sed 's/^/    /'
    echo
    echo "| arm | total per run (handoffs + thread-blocks) | total, median | thread-blocks, max | peak OS threads per run | peak, max | wall ms per run | wall, median | go.*.dll | bytes | reports | twinned | refused byref-param | box plain |"
    echo "|---|---|---:|---:|---|---:|---|---:|---:|---:|---:|---:|---:|---:|"
    row d1; row n1; row d2
} > "$md"

# ---- scoring, per DESIGN-mn-box-twin.md 12.1 (which supersedes 12 where they differ); predictions as written ----
n_tot=$(median < "$root/n1/total.txt"); n_thr=$(sort -n "$root/n1/threads.txt" | tail -1)
read -r _ n_bytes < "$root/n1/bytes.txt"
score() {
    local a=$1 d="$root/$1"
    local tot tb thr bytes fall delta
    tot=$(median < "$d/total.txt"); tb=$(sort -n "$d/tb.txt" | tail -1); thr=$(sort -n "$d/threads.txt" | tail -1)
    read -r _ bytes < "$d/bytes.txt"
    fall=$(awk -v n="$n_tot" -v d="$tot" 'BEGIN { printf "%.1f", (n - d) * 100 / n }')
    delta=$((bytes - n_bytes))
    local pb1=MISSED pb4=MISSED
    awk -v f="$fall" 'BEGIN { exit !(f >= 4 && f <= 21) }' && [ "$tb" = 0 ] && [ "$thr" -le "$n_thr" ] && pb1=MET
    if [ $delta -ge 52429 ] && [ $delta -le 367002 ] && [ $delta -ge 50000 ] && [ $delta -le 350000 ]; then pb4=MET
    elif [ $delta -ge 50000 ] && [ $delta -le 367002 ]; then pb4="MET in one unit only (MB 10^6 against MiB): name both"; fi
    echo "- $a against n1: PB-1 $pb1 (total median $n_tot -> $tot, a fall of $fall% against 4-21%; thread-blocks max $tb against 0; peak OS threads max $thr against n1's $n_thr). PB-4 $pb4 (go.*.dll $n_bytes -> $bytes, $delta bytes against +0.05 to +0.35 MB). PB-6 MET (every run EQUAL to Go; the arm would have failed otherwise)."
}
d1_tot=$(median < "$root/d1/total.txt"); d2_tot=$(median < "$root/d2/total.txt")
read -r _ d1_b < "$root/d1/bytes.txt"; read -r _ d2_b < "$root/d2/bytes.txt"
{
    echo
    echo "## Scoring (DESIGN-mn-box-twin.md 12.1 as written; arm n1 is N on this box)"
    echo
    score d1
    score d2
    echo "- A/A (d1 against d2): total median $d1_tot against $d2_tot (spread $(awk -v a="$d1_tot" -v b="$d2_tot" 'BEGIN { d = a - b; if (d < 0) d = -d; printf "%.1f", d }')); go.*.dll $d1_b against $d2_b ($((d1_b - d2_b)) bytes). A d1/d2 pair that disagrees on MET is inside the band: say so rather than pick one."
    echo "- As written in section 12 on G's box (12 cores): total 105-128 and peak OS threads at most 39 in every run. Quote it only when this box is G's."
    echo "- Not read by row 14: SB-3 (needs the plain arm P), PB-2/3/5 are host-independent (the table's report columns, for the reader)."
} >> "$md"
cat "$md" | tee -a "$log" > /dev/null
say ""
say "row14: readings in $md"
exit 0
