#!/usr/bin/env bash
# R0 ONLY, NOT ADAPTED TO TRAIN Q (review round 1): this is trainP's pre-map reading tool under Q's name. It reads no ruled resolution (on Q's table every ruled row reads as a conflict), it needs git 2.44 or later through GITX, its comments below describe TRAIN P's pre-map, and it is on NO step of COORD-LAUNCH-CHECKLIST.md: the map of record is tQ-conflict-map.sh.
# tQ-premap.sh -- TRAIN Q conflict PRE-map with NO worktree (DRAFT 2026-10-04, UNCOMMITTED; COORD reviews).
# Every merge is git's own merge-ort: 'git merge-tree --write-tree --messages <ours> <theirs>'. ort computes the
# merge base itself (a criss-cross pair gets a recursive virtual base: tO-premap.sh used the FIRST base only and read
# a false README conflict on c2-nuget-followups-r2 at O) and detects renames (tO-premap.sh's read-tree had none).
# The box's default git is 2.35.2 (no 'merge-tree --write-tree'): this script runs every git command through GITX,
# a git >= 2.44 (default: the one Visual Studio 2022 bundles, 2.55.0 at drafting; 2.44 since review round 1). Nothing is checked out anywhere and
# no ref or index of any worktree is touched, so it is legal beside a running battery (the mid-battery source freeze
# binds a WORKTREE). It writes objects only: merged blobs and trees, and DANGLING chain commits made with
# 'commit-tree --no-gpg-sign' at a FIXED date (a re-run reproduces the same commit shas, not only the same trees).
# WHERE THE OBJECTS GO (OBJDIR): by default a SCRATCH object directory (OUT/objects), with the shared store read
# through GIT_ALTERNATE_OBJECT_DIRECTORIES, so NOTHING is written to H:/Projects/go2cs/.git. Measured 2026-10-04: the
# first run of this script wrote into the shared store and took it from 5,633 to 7,017 loose objects, past gc.auto's
# 6,700, so the next 'git fetch' ran 'gc --auto' and warned 'too many unreachable loose objects' (unreachable objects
# cannot be packed away; they age out after gc.pruneExpire). To READ a scratch chain commit or tree afterwards:
#   GIT_ALTERNATE_OBJECT_DIRECTORIES=<OUT, Windows style>/objects git -C /h/Projects/go2cs show <sha>:<path>
# (tQ-regcheck.py, tQ-unioncheck.py and premap-run1's readers need the same variable exported.) OBJDIR=shared writes
# into the shared store instead (COORD's explicit call: the shas are then readable with plain git by every worktree).
#
# Usage:
#   BASE=<commit> ROWS=<file> OUT=<dir> [MODE=chain|alone|pairs|replay] [ONCONFLICT=union|skip] [OBJDIR=<dir>|shared] bash tQ-premap.sh
#   BASE   P's base = TRAIN O's landed master once it exists; until then pass a stand-in EXPLICITLY (never a literal here).
#   ROWS   merge order, one row per line: 'name|sha|...' (the tQ-seats-draft.txt format) or 'name sha'; '#' lines skipped.
#   OUT    a scratch directory (either path style; converted with cygpath -m for native git).
#   MODE   chain (default): each row merges into the previous step's commit, in ROWS order.
#          alone: each row merged alone onto BASE; writes OUT/onbase.tsv (name, row sha, the row-on-BASE commit).
#          pairs: every pair of rows, each FIRST put on BASE (its alone merge, conflicts carried as a union), then
#                 merged with the other: what remains is seat-versus-seat, with the base's own conflicts factored out.
#          replay: CONTROL. REPLAY_FROM..REPLAY_TO first-parent merges, each re-merged from its two real parents and
#                 its tree compared with the real merge commit's tree.
#   ONCONFLICT (chain; also how alone/pairs put a conflicted row on BASE) union (default): a conflicted text path is
#          carried forward as 'git merge-file --union --diff3' of the three stages ort reports: both sides' regions
#          kept WHOLE, so LATER rows still meet BOTH rows' changes. --diff3 matters: the default (refined) union moves
#          a line both regions share out of the conflict and keeps it ONCE, and where that line is a closing brace the
#          carried Go file stops parsing (measured 2026-10-04 on importAliasOperations.go: 502 lines and 15 gofmt
#          errors refined, 504 = 473 + 19 + 12 and 0 errors unrefined). The carried tree is NOT a ruled resolution.
#          skip: the conflicted row is left out (what the assembler sees if the row is dropped).
# Limits (stated, not hidden): a merge that compiles nowhere is invisible here; a union-carried path is not a
# resolution; the sequential 'git merge' rehearsal at the real BASE stays the reading of record (train-assembly 7).
set -u
export MSYS_NO_PATHCONV=1
GD=${GD:-H:/Projects/go2cs/.git}
GITX=${GITX:-/c/Program Files/Microsoft Visual Studio/2022/Community/Common7/IDE/CommonExtensions/Microsoft/TeamFoundation/Team Explorer/Git/cmd/git.exe}
: "${OUT:?OUT is required (a scratch directory)}"
MODE=${MODE:-chain}
ONCONFLICT=${ONCONFLICT:-union}
[ -x "$GITX" ] || { echo "ABORT: GITX '$GITX' is not an executable git"; exit 2; }
g() { "$GITX" --git-dir="$GD" -c core.quotepath=false "$@"; }
gv=$("$GITX" --version | sed 's/^git version //')
gmaj=${gv%%.*}; grest=${gv#*.}; gmin=${grest%%.*}
# REVIEW ROUND 1 (trainP/tP-CHANGES.md RR1-5): the gate was 2.38 ('merge-tree --write-tree'), but the hunk count and the
# union carry call 'git merge-file --diff-algorithm', which is 2.44's. On a git between the two (and on this box's
# default 2.35.2, reproduced: rc 129, 0 bytes) the carried blob was EMPTY and the hunk count read 0, silently: stderr
# went to /dev/null and no rc was read. The gate is 2.44 now, and mt() reads merge-file's rc and refuses an empty carry.
{ [ "$gmaj" -gt 2 ] || { [ "$gmaj" = 2 ] && [ "$gmin" -ge 44 ]; }; } || { echo "ABORT: GITX is git $gv; this script needs >= 2.44 ('merge-tree --write-tree' is 2.38's, 'merge-file --diff-algorithm' is 2.44's)"; exit 2; }
mkdir -p "$OUT" || exit 2
OUTW=$(cygpath -m "$OUT")
OBJDIR=${OBJDIR:-$OUT/objects}
if [ "$OBJDIR" != shared ]; then
  mkdir -p "$OBJDIR" || exit 2
  export GIT_OBJECT_DIRECTORY=$(cygpath -m "$OBJDIR")
  export GIT_ALTERNATE_OBJECT_DIRECTORIES="$GD/objects${GIT_ALTERNATE_OBJECT_DIRECTORIES:+;$GIT_ALTERNATE_OBJECT_DIRECTORIES}"
  OBJNOTE="objects -> $GIT_OBJECT_DIRECTORY (scratch; the shared store is read-only here)"
else
  OBJNOTE="objects -> the SHARED store $GD/objects (OBJDIR=shared)"
fi
ZERO=0000000000000000000000000000000000000000
export GIT_AUTHOR_NAME=premap GIT_AUTHOR_EMAIL=premap@invalid GIT_COMMITTER_NAME=premap GIT_COMMITTER_EMAIL=premap@invalid
export GIT_AUTHOR_DATE='1700000000 +0000' GIT_COMMITTER_DATE='1700000000 +0000'

# mt <ours> <theirs> <onconflict>
#   sets MT_TREE (ort's tree; conflicted paths hold markers), MT_CARRY (the tree to carry: MT_TREE when clean, the
#   union-carried tree when conflicted and onconflict=union, empty otherwise), MT_CONF[] (path:kind), MT_AUTO[] (paths
#   ort content-merged), MT_NB (number of best common ancestors), MT_MSG (the CONFLICT lines).
#   returns 0 clean, 1 conflicts, 2 error
mt() {
  local ours=$1 theirs=$2 oc=$3 raw rc line path mode sha st
  MT_TREE=""; MT_CARRY=""; MT_CONF=(); MT_AUTO=(); MT_MSG=""
  MT_NB=$(g merge-base --all "$ours" "$theirs" | grep -c .)
  [ "$MT_NB" -ge 1 ] || { echo "  ERROR: no merge base for $ours $theirs"; return 2; }
  raw="$OUT/mt.raw"
  g merge-tree --write-tree --messages "$ours" "$theirs" > "$raw" 2> "$OUT/mt.err"; rc=$?
  [ "$rc" = 0 ] || [ "$rc" = 1 ] || { echo "  ERROR: merge-tree rc=$rc: $(head -3 "$OUT/mt.err")"; return 2; }
  MT_TREE=$(head -1 "$raw" | tr -d '\r')
  [ "${#MT_TREE}" = 40 ] || { echo "  ERROR: merge-tree printed no tree"; return 2; }
  # section 2 (conflicted file info: '<mode> <oid> <stage>TAB<path>') ends at the first empty line; section 3 = messages
  local insec=1 n=0; declare -A s1=() s2=() s3=() m2=() m3=() seen=(); local order=()
  while IFS= read -r line; do
    line=${line%$'\r'}; n=$((n+1)); [ "$n" = 1 ] && continue
    if [ "$insec" = 1 ]; then
      if [ -z "$line" ]; then insec=0; continue; fi
      path=${line#*$'\t'}; read -r mode sha st <<<"${line%%$'\t'*}"
      [ -n "${seen[$path]:-}" ] || { seen[$path]=1; order+=("$path"); }
      case "$st" in 1) s1[$path]=$sha;; 2) s2[$path]=$sha; m2[$path]=$mode;; 3) s3[$path]=$sha; m3[$path]=$mode;; esac
    else
      case "$line" in
        'Auto-merging '*) MT_AUTO+=("${line#Auto-merging }");;
        'CONFLICT '*) MT_MSG+="    $line"$'\n';;
      esac
    fi
  done < "$raw"
  if [ "$rc" = 0 ]; then MT_CARRY=$MT_TREE; return 0; fi
  local idx="$OUTW/mt.idx" fb="$OUTW/f1" fo="$OUTW/f2" ft="$OUTW/f3" res="$OUT/res" blob kind hunks newmode mrc
  if [ "$oc" = union ]; then rm -f "$OUT/mt.idx"; GIT_INDEX_FILE="$idx" g read-tree "$MT_TREE" || { echo "  ERROR: read-tree"; return 2; }; fi
  for path in "${order[@]}"; do
    if [ -n "${s2[$path]:-}" ] && [ -n "${s3[$path]:-}" ]; then
      if [ -n "${s1[$path]:-}" ]; then g cat-file blob "${s1[$path]}" > "$OUT/f1"; kind=content; else : > "$OUT/f1"; kind=add/add; fi
      g cat-file blob "${s2[$path]}" > "$OUT/f2"; g cat-file blob "${s3[$path]}" > "$OUT/f3"
      # merge-file's rc is its conflict count (0 to 127); 128 and above is an ERROR (129: an option this git lacks).
      g merge-file -p --diff-algorithm histogram "$fo" "$fb" "$ft" > "$OUT/mf.out" 2> "$OUT/mf.err"; mrc=$?
      [ "$mrc" -lt 128 ] || { echo "  ERROR: merge-file rc=$mrc on $path: $(head -n 2 "$OUT/mf.err" | tr '\n' ' ')"; return 2; }
      hunks=$(grep -c '^<<<<<<< ' "$OUT/mf.out")
      MT_CONF+=("$path:$kind($hunks hunk(s))")
      if [ "$oc" = union ]; then
        g merge-file -p --union --diff3 --diff-algorithm histogram "$fo" "$fb" "$ft" > "$res" 2> "$OUT/mf.err"; mrc=$?
        [ "$mrc" -lt 128 ] || { echo "  ERROR: merge-file --union rc=$mrc on $path: $(head -n 2 "$OUT/mf.err" | tr '\n' ' ')"; return 2; }
        if [ ! -s "$res" ] && [ -s "$OUT/f2" ] && [ -s "$OUT/f3" ]; then echo "  ERROR: the union carry of $path is EMPTY while both sides hold bytes"; return 2; fi
        blob=$(g hash-object -w --no-filters "$OUTW/res"); newmode=${m2[$path]}
        GIT_INDEX_FILE="$idx" g update-index --add --cacheinfo "$newmode,$blob,$path" || { echo "  ERROR: update-index $path"; return 2; }
      fi
    elif [ -n "${s2[$path]:-}" ]; then MT_CONF+=("$path:modify(ours)/delete(theirs)")
    elif [ -n "${s3[$path]:-}" ]; then MT_CONF+=("$path:delete(ours)/modify(theirs)")
    else MT_CONF+=("$path:other(base only)"); fi
  done
  [ "${#MT_CONF[@]}" -ge 1 ] || MT_CONF+=("(no staged path: read the messages)")
  if [ "$oc" = union ]; then MT_CARRY=$(GIT_INDEX_FILE="$idx" g write-tree) || { echo "  ERROR: write-tree"; return 2; }; fi
  return 1
}
commit() { # commit <tree> <message> <parent>...   -> a dangling commit
  local t=$1 m=$2; shift 2; local ps=(); local p; for p in "$@"; do ps+=(-p "$p"); done
  printf '%s\n' "$m" | g commit-tree --no-gpg-sign "$t" "${ps[@]}"
}
nbnote() { [ "$MT_NB" -gt 1 ] && printf ' CRISS-CROSS(%s bases: ort builds the virtual base)' "$MT_NB"; }

if [ "$MODE" = replay ]; then
  : "${REPLAY_FROM:?REPLAY_FROM is required}" "${REPLAY_TO:?REPLAY_TO is required}"
  echo "REPLAY control: first-parent merges of $REPLAY_FROM..$REPLAY_TO, each re-merged from its real parents (git $gv) $(date '+%Y-%m-%d %H:%M %Z')"
  echo "REPLAY $OBJNOTE"
  n=0; same=0; : > "$OUT/replay.tsv"
  while read -r m p1 p2 rest; do
    [ -n "${p2:-}" ] || { echo "REPLAY ${m:0:10}: not a merge ($(g log -1 --format=%s "$m" | cut -c1-70)): skipped"; continue; }
    [ -z "${rest:-}" ] || { echo "REPLAY ${m:0:10}: octopus: skipped"; continue; }
    n=$((n+1)); mt "$p1" "$p2" skip; rc=$?
    real=$(g rev-parse "$m^{tree}")
    if [ "$rc" = 0 ] && [ "$MT_TREE" = "$real" ]; then same=$((same+1)); v=IDENTICAL; elif [ "$rc" = 0 ]; then v="CLEAN-BUT-DIFFERENT (ours ${MT_TREE:0:12})"; else v="CONFLICT here (${MT_CONF[*]})"; fi
    echo "REPLAY $n ${m:0:10} <- ${p2:0:10} ($(g log -1 --format=%s "$m" | sed 's/^Merge //' | cut -c1-60)): $v tree ${real:0:12} automerged=${#MT_AUTO[@]}$(nbnote)"
    printf '%s\t%s\t%s\t%s\t%s\n' "$n" "${m:0:10}" "${p2:0:10}" "$v" "$MT_NB" >> "$OUT/replay.tsv"
  done < <(g log --first-parent --reverse --format='%H %P' "$REPLAY_FROM..$REPLAY_TO")
  echo "REPLAY DONE merges=$n identical=$same"
  exit 0
fi

: "${BASE:?BASE is required: the P base commit, or an explicit stand-in}"
: "${ROWS:?ROWS is required}"
BASEC=$(g rev-parse --verify -q "$BASE^{commit}") || { echo "ABORT: BASE $BASE does not resolve"; exit 2; }
NAMES=(); SHAS=()
while IFS= read -r line; do
  line=${line%$'\r'}
  case "$line" in ''|'#'*) continue;; esac
  if [[ "$line" == *'|'* ]]; then n=${line%%|*}; r=${line#*|}; s=${r%%|*}; else read -r n s _ <<<"$line"; fi
  full=$(g rev-parse --verify -q "$s^{commit}") || { echo "ABORT: row $n sha $s does not resolve (fetch it first)"; exit 2; }
  NAMES+=("$n"); SHAS+=("$full")
done < "$ROWS"
[ "${#NAMES[@]}" -ge 1 ] || { echo "ABORT: no rows in $ROWS"; exit 2; }
echo "PREMAP mode=$MODE onconflict=$ONCONFLICT base=${BASEC:0:10} rows=${#NAMES[@]} ($(basename "$ROWS")) git=$gv $(date '+%Y-%m-%d %H:%M %Z')"
echo "PREMAP $OBJNOTE"

# onbase <i>: the row put on BASE. Sets OB (a commit), OBNOTE.
onbase() {
  local s=${SHAS[$1]} n=${NAMES[$1]}
  if g merge-base --is-ancestor "$BASEC" "$s"; then OB=$s; OBNOTE="descends from BASE"; OBRC=0; return 0; fi
  if g merge-base --is-ancestor "$s" "$BASEC"; then OB=$BASEC; OBNOTE="already contained in BASE"; OBRC=0; return 0; fi
  mt "$BASEC" "$s" union; OBRC=$?
  [ "$OBRC" = 2 ] && return 2
  OB=$(commit "$MT_CARRY" "premap: $n ${s:0:10} on base ${BASEC:0:10}" "$BASEC" "$s") || return 2
  OBNOTE="merged onto BASE"
}

case "$MODE" in
alone)
  nbad=0; : > "$OUT/onbase.tsv"
  for i in "${!NAMES[@]}"; do
    n=${NAMES[$i]}; s=${SHAS[$i]}
    cut=$(g merge-base "$BASEC" "$s"); onbase "$i" || { echo "ALONE $n: ERROR"; nbad=$((nbad+1)); continue; }
    if [ "$OBNOTE" != "merged onto BASE" ]; then
      echo "ALONE $n ${s:0:10}: clean ($OBNOTE: the merge result is the row's own tree $(g rev-parse --short=12 "$OB^{tree}"))"
    elif [ "$OBRC" = 0 ]; then
      echo "ALONE $n ${s:0:10}: clean (cut ${cut:0:10}$(nbnote); tree ${MT_TREE:0:12}; ort content-merged ${#MT_AUTO[@]}: ${MT_AUTO[*]:-none})"
    else
      nbad=$((nbad+1)); echo "ALONE $n ${s:0:10}: CONFLICT (cut ${cut:0:10}$(nbnote)) in: ${MT_CONF[*]}  [clean content merges: $(( ${#MT_AUTO[@]} - ${#MT_CONF[@]} ))]"; printf '%s' "$MT_MSG"
    fi
    printf '%s\t%s\t%s\t%s\n' "$n" "$s" "$OB" "$OBRC" >> "$OUT/onbase.tsv"
  done
  echo "ALONE DONE rows=${#NAMES[@]} not-clean=$nbad";;
pairs)
  OBS=(); for i in "${!NAMES[@]}"; do onbase "$i" || { echo "PAIRS: ERROR putting ${NAMES[$i]} on BASE"; exit 3; }; OBS+=("$OB"); done
  np=0; nc=0; : > "$OUT/pairs.tsv"
  for i in "${!NAMES[@]}"; do for j in "${!NAMES[@]}"; do [ "$j" -gt "$i" ] || continue
    a=${SHAS[$i]}; b=${SHAS[$j]}
    if g merge-base --is-ancestor "$a" "$b" || g merge-base --is-ancestor "$b" "$a"; then echo "PAIR ${NAMES[$i]} x ${NAMES[$j]}: ANCESTRY (stacked; not a pair)"; continue; fi
    mt "${OBS[$i]}" "${OBS[$j]}" skip; rc=$?
    [ "$rc" = 2 ] && { echo "PAIR ${NAMES[$i]} x ${NAMES[$j]}: ERROR"; continue; }
    [ "${#MT_AUTO[@]}" -gt 0 ] || [ "$rc" = 1 ] || continue
    np=$((np+1))
    if [ "$rc" = 0 ]; then echo "PAIR ${NAMES[$i]} x ${NAMES[$j]}: clean$(nbnote); ort content-merged ${#MT_AUTO[@]}: ${MT_AUTO[*]}"
    else nc=$((nc+1)); echo "PAIR ${NAMES[$i]} x ${NAMES[$j]}: CONFLICT$(nbnote) in: ${MT_CONF[*]} || all content-merged paths: ${MT_AUTO[*]}"; fi
    for p in "${MT_AUTO[@]}"; do
      v=clean; for c in "${MT_CONF[@]:-}"; do [ "${c%%:*}" = "$p" ] && v=CONFLICT; done
      printf '%s\t%s\t%s\t%s\n' "$p" "${NAMES[$i]}" "${NAMES[$j]}" "$v" >> "$OUT/pairs.tsv"
    done
  done; done
  echo "PAIRS DONE (each row first put on BASE) pairs-with-a-both-changed-path=$np conflicting=$nc";;
chain)
  prev=$BASEC; nbad=0; k=0
  : > "$OUT/chain.tsv"; : > "$OUT/chain-merged-paths.txt"
  for i in "${!NAMES[@]}"; do
    n=${NAMES[$i]}; s=${SHAS[$i]}; k=$((k+1))
    if g merge-base --is-ancestor "$s" "$prev"; then echo "STEP $k $n ${s:0:10}: already contained"; continue; fi
    mt "$prev" "$s" "$ONCONFLICT"; rc=$?
    if [ $rc = 2 ]; then echo "STEP $k $n: ERROR"; exit 3; fi
    # every path ort content-merged at this step (clean or not): the list tQ-premap-all.sh hands to tQ-regcheck.py
    [ "${#MT_AUTO[@]}" = 0 ] || printf '%s\n' "${MT_AUTO[@]}" >> "$OUT/chain-merged-paths.txt"
    if [ $rc = 0 ]; then
      echo "STEP $k $n ${s:0:10}: clean$(nbnote); ort content-merged ${#MT_AUTO[@]}: ${MT_AUTO[*]:-none}"; st=clean
    else
      nbad=$((nbad+1)); st="CONFLICT ${MT_CONF[*]}"
      echo "STEP $k $n ${s:0:10}: CONFLICT$(nbnote) in: ${MT_CONF[*]}  [other content merges, clean: $(( ${#MT_AUTO[@]} - ${#MT_CONF[@]} ))]"; printf '%s' "$MT_MSG"
    fi
    if [ $rc = 0 ] || [ "$ONCONFLICT" = union ]; then
      prev=$(commit "$MT_CARRY" "premap step $k: $n" "$prev" "$s") || exit 3
    fi
    printf '%s\t%s\t%s\t%s\t%s\t%s\n' "$k" "$n" "${s:0:10}" "$(g rev-parse --short=10 "$prev")" "$(g rev-parse --short=12 "$prev^{tree}")" "$st" >> "$OUT/chain.tsv"
  done
  echo "CHAIN DONE rows=${#NAMES[@]} conflicted-steps=$nbad onconflict=$ONCONFLICT head=$(g rev-parse --short=12 "$prev") tree=$(g rev-parse --short=12 "$prev^{tree}") (dangling commit: no ref)";;
*) echo "ABORT: MODE $MODE"; exit 2;;
esac
