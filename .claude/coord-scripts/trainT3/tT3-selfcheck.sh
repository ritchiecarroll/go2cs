#!/usr/bin/env bash
# TRAIN T3 (2026-10-10): S7 counts TRAIN FL's names (the previous train); the derive's own two .py are parsed by S6.
# TRAIN FL: DERIVED 2026-10-08 from trainQ/tQ-selfcheck.sh; S7 now counts TRAIN Q names (the previous train), and the
# premap set is not carried (EXEMPT is empty). Q header follows.
# TRAIN Q -- a STATIC check of the script set itself (trainQ/tQ-CHANGES.md Q5, Q6). Nothing is run but bash -n, a
# Python parse and greps; nothing is written outside a temporary plant file under OUT.
#   OUT=<scratch dir under /h/go2cs-tmp-coord/coord-scratch/tT3/> bash tT3-selfcheck.sh
# What it reads in every *.sh of this folder (each rule has a PLANTED control, run first: a rule that does not fire on
# its plant voids the check):
#   S1  no `git -C` on a line that also sets MSYS_NO_PATHCONV, and none between an `export MSYS_NO_PATHCONV=1` and its
#       `unset MSYS_NO_PATHCONV` (P's landing: such a read finds no tree, prints nothing, and a count reads 0).
#       tT3-premap*.sh, trainQ/tQ-footprints.sh, trainQ/tQ-hunks.sh and trainQ/tQ-gofmt-parse.sh export it for a whole script by design and
#       hand native git Windows paths through GITX: they are listed as EXEMPT, by name, never silently.
#       ITS LIMIT (review round 1, measured on a plant): S1 reads LINES, top to bottom. A `git -C` inside a FUNCTION
#       that is DEFINED outside an export region and CALLED inside one is not seen (the plant read S1 ok). The runtime
#       guards cover what a line rule cannot: the three landing scripts REFUSE to start with MSYS_NO_PATHCONV set, the
#       battery and the module legs prove git still reads the tree after each of their exports (gitalive), and every
#       tracked-changes count goes through tcn, whose every copy tT3-controls.sh arm TC extracts and runs.
#   S2  no apostrophe inside a ${VAR:?message}: bash opens a quoted string there and the script still passes bash -n
#       when a later apostrophe happens to close it (found in this derive, in two scripts).
#   S3  every "$PWSH" is started as `env -u DOTNET_ROOT PATH="$GOPIN_PATH" ... "$PWSH"` (pwsh is a .NET 8 global tool
#       on the i7 and does not start with the .NET 10 SDK folder first on PATH).
#   S4  no profile path, account name or host name: no drive-rooted Users path, and not the value of $USERNAME / $USER
#       / $COMPUTERNAME of the box that runs the check (read from the environment, never written here).
#   S5  no CR byte in any file of the folder.
#   S6  bash -n on every *.sh; a Python parse of every *.py.
#   S7  the survivors of the previous train's names, COUNTED per file (a reading: each is a pointer to P's own records
#       or P's battery of record; trainQ/tQ-CHANGES.md section 1 justifies them).
# Last line: Q-SELFCHECK DONE failed=<n>.
set -u
SD=$(cd "$(dirname "$0")" && pwd)
OUT=${OUT:?set OUT to a scratch directory under /h/go2cs-tmp-coord/coord-scratch/tT3/}
case "$OUT" in /h/go2cs-tmp-coord/coord-scratch/tT3/?*) ;; *) echo "ABORT: OUT=$OUT is not under /h/go2cs-tmp-coord/coord-scratch/tT3/"; exit 2 ;; esac
case "$OUT" in *..*) echo "ABORT: OUT holds '..'"; exit 2 ;; esac
mkdir -p "$OUT" || exit 2
FAILED=0
verdict(){ if [ "$2" = 0 ]; then echo "SELFCHECK $1: ok -- $3"; else echo "SELFCHECK $1: FAILED -- $3"; FAILED=$((FAILED + 1)); fi; }
EXEMPT=''   # FL: the premap set (trainQ/tQ-premap*.sh and its helpers) is NOT carried into trainT3
s1(){ # file -> the offending lines 'n: text'
  awk '
    { line = $0 }
    /^[[:space:]]*#/ { next }
    /export MSYS_NO_PATHCONV=1/ { on = 1 }
    /unset MSYS_NO_PATHCONV/ { on = 0 }
    (on || line ~ /MSYS_NO_PATHCONV=1[^;|&]*git -C/) && line ~ /git -C/ { print NR ": " substr(line, 1, 110) }' "$1"
}
s2(){ grep -nE '\$\{[A-Za-z_][A-Za-z0-9_]*:\?[^}]*'"'" "$1" | cut -c1-120; }
s3(){ grep -n '"\$PWSH"' "$1" | grep -v '^[0-9]*:[[:space:]]*#' | grep -vE 'env -u DOTNET_ROOT PATH="\$GOPIN_PATH"[^|;]*"\$PWSH"|"\$0" -NoProfile -Command' | cut -c1-120; }
# ---- the planted controls
P=$OUT/selfcheck-plant.sh
printf '%s\n' 'export MSYS_NO_PATHCONV=1' 'tc=$(git -C "$W" status --porcelain | wc -l)' 'unset MSYS_NO_PATHCONV' 'x=$(git -C "$W" rev-parse HEAD)' 'MSYS_NO_PATHCONV=1 git -C "$W" log' \
  "B=\${BASE:?set BASE to TRAIN P's landed line}" '"$PWSH" -NoProfile -File x.ps1' 'env -u DOTNET_ROOT PATH="$GOPIN_PATH" "$PWSH" -NoProfile -File y.ps1' > "$P"
c1=$(s1 "$P" | wc -l); c2=$(s2 "$P" | wc -l); c3=$(s3 "$P" | wc -l)
[ "$c1" = 2 ] && [ "$c2" = 1 ] && [ "$c3" = 1 ]
verdict PLANTS "$?" "the three line rules on a planted file: S1 fires on $c1 line(s) (EXPECT 2: inside the export region, and on the one-line form; the git -C after the unset is clean), S2 on $c2 (EXPECT 1), S3 on $c3 (EXPECT 1: the bare start, not the env form)"
rm -f "$P"
# ---- the folder
b1=''; b2=''; b3=''; ex=''
for f in "$SD"/*.sh; do
  n=$(basename "$f")
  case " $EXEMPT " in *" $n "*) [ -z "$(grep -l 'MSYS_NO_PATHCONV' "$f")" ] || ex="$ex$n "; h=$(s2 "$f"); [ -z "$h" ] || b2="$b2$n:[$h] "; continue ;; esac
  # The two CONTROL scripts plant the patterns on purpose (this file's plant lines; tT3-controls.sh arm TC reads the
  # vacuous form to show it): they are skipped for S1 and S2, BY NAME, and named in S1's line.
  case "$n" in tT3-selfcheck.sh|tT3-controls.sh) ex="$ex$n(control) "; h=$(s3 "$f"); [ -z "$h" ] || b3="$b3$n:[$h] "; continue ;; esac
  h=$(s1 "$f"); [ -z "$h" ] || b1="$b1$n:[$(echo "$h" | head -n 3 | tr '\n' ';')] "
  h=$(s2 "$f"); [ -z "$h" ] || b2="$b2$n:[$(echo "$h" | head -n 3 | tr '\n' ';')] "
  h=$(s3 "$f"); [ -z "$h" ] || b3="$b3$n:[$(echo "$h" | head -n 3 | tr '\n' ';')] "
done
[ -z "$b1" ]; verdict S1 "$?" "git -C under MSYS_NO_PATHCONV: ${b1:-none} (EXEMPT by design, whole-script exports with GITX and Windows paths: ${ex:-none})"
[ -z "$b2" ]; verdict S2 "$?" "an apostrophe inside a \${VAR:?...} message: ${b2:-none}"
[ -z "$b3" ]; verdict S3 "$?" "a pwsh start that is not the battery's own form: ${b3:-none}"
ids=''
for v in "${USERNAME:-}" "${USER:-}" "${COMPUTERNAME:-}" "${HOSTNAME:-}"; do [ "${#v}" -ge 4 ] && ids="$ids$v|"; done
h=$(grep -n -i -E "(${ids}[A-Za-z]:[\\\\/]+Users[\\\\/]|/[a-z]/Users/)" "$SD"/*.sh "$SD"/*.py "$SD"/*.md "$SD"/*.txt "$SD"/*.ps1 2>/dev/null | cut -d: -f1,2 | sed 's#.*/##' | head -n 8 | tr '\n' ' ')
[ -z "$h" ]; verdict S4 "$?" "a profile path, or this box's account or host name (read from the environment), in a file of the folder: ${h:-none}"
crs=''; for f in "$SD"/*.sh "$SD"/*.py "$SD"/*.md "$SD"/*.txt "$SD"/*.ps1; do [ "$(tr -cd '\r' < "$f" | wc -c)" = 0 ] || crs="$crs$(basename "$f") "; done
[ -z "$crs" ]; verdict S5 "$?" "files holding a CR byte: ${crs:-none}"
bn=''; nsh=0; for f in "$SD"/*.sh; do nsh=$((nsh + 1)); bash -n "$f" 2>/dev/null || bn="$bn$(basename "$f") "; done
pn=''; npy=0; for f in "$SD"/*.py; do npy=$((npy + 1)); python -B -c "import ast,sys; ast.parse(open(sys.argv[1],encoding='utf-8').read())" "$(cygpath -w "$f")" 2>/dev/null || pn="$pn$(basename "$f") "; done
[ -z "$bn$pn" ]; verdict S6 "$?" "bash -n on $nsh .sh: ${bn:-all pass}; Python parse of $npy .py: ${pn:-all pass}"
echo "SELFCHECK S7 (a reading): lines naming the previous train, per file:"
for f in "$SD"/*.sh "$SD"/*.py "$SD"/*.ps1 "$SD"/*.txt "$SD"/*.md; do
  c=$(grep -c -E '/tFL\b|tFLemit|trainFL|coord-trainFL-union|TRAIN FL\b|tFL-' "$f"); [ "$c" = 0 ] || printf '    %-34s %s\n' "$(basename "$f")" "$c"
done
echo "T3-SELFCHECK DONE failed=$FAILED"
[ "$FAILED" = 0 ]
