#!/usr/bin/env bash
# tO-premap.sh -- TRAIN O conflict PRE-map with NO worktree (DRAFT 2026-10-03, UNCOMMITTED; COORD reviews).
# Every merge is a temp-index 3-way: read-tree -m --aggressive -i <merge-base> <ours> <theirs>, then git merge-file
# over each unmerged path. Nothing is checked out anywhere, so it is legal beside a running battery (the mid-battery
# source freeze binds a WORKTREE; this touches none). git 2.35 on the i7 has no 'merge-tree --write-tree'
# (merge-hazards: "A conflict dry-run does not need git merge-tree --write-tree").
# It writes loose objects only (blobs, trees, dangling chain commits made with commit-tree --no-gpg-sign); no ref.
#
# Usage:
#   BASE=<commit> ROWS=<file> OUT=<dir> [MODE=chain|alone|pairs] [ONCONFLICT=union|skip] bash tO-premap.sh
#   BASE   O's base. TRAIN N's landed master once it exists; until then pass a stand-in EXPLICITLY (never a literal here).
#   ROWS   merge order, one row per line: 'name|sha|...' (the tO-seats-draft.txt format) or 'name sha'; '#' lines skipped.
#   MODE   chain (default): each row merges into the previous step's tree, in ROWS order.
#          alone: each row merged alone onto BASE.
#          pairs: every pair of rows whose OWN footprints (vs merge-base with BASE) share a path, merged pairwise.
#   ONCONFLICT (chain only) union (default): a conflicted text path is carried forward as 'git merge-file --union'
#          (both sides' lines kept) and a modify/delete keeps the modified side, so LATER rows still meet BOTH rows'
#          changes; the carried tree is NOT a ruled resolution. skip: the conflicted row is left out (tN-conflict-map.sh's
#          semantics: what the assembler would see if the row were dropped).
# Limits (stated, not hidden): no rename detection (read-tree has none; 0 renames in the O rows at drafting);
# merge-file's default diff (myers) where git merge uses ort's; a criss-cross merge base is reported, the first used.
# The sequential 'git merge' rehearsal at the real BASE stays the reading of record (train-assembly section 7).
# Merged blobs are written with 'hash-object --no-filters' (this box has core.autocrlf=true; a filtered write would
# re-encode a CRLF blob and the tree would stop being git's). commit.gpgsign=true here, hence --no-gpg-sign.
# CONTROLS (2026-10-03, tO-premap.md section 0): N's 30 merges replayed from 8f46a9adae reproduce all 30 real merge
# trees byte for byte (last f0f33a3875c2 = 8fbc1b0a13^{tree}); MS21 items 1-3 and p1-hashset-module vs N's union
# read CONFLICT on the named files.
set -u
export MSYS_NO_PATHCONV=1
GD=${GD:-H:/Projects/go2cs/.git}
: "${BASE:?BASE is required: the O base commit, or an explicit stand-in}"
: "${ROWS:?ROWS is required}"
: "${OUT:?OUT is required (a scratch directory)}"
MODE=${MODE:-chain}
ONCONFLICT=${ONCONFLICT:-union}
g() { git --git-dir="$GD" -c core.quotepath=false "$@"; }
mkdir -p "$OUT" || exit 2
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
echo "PREMAP mode=$MODE onconflict=$ONCONFLICT base=$(g rev-parse --short=10 "$BASEC") rows=${#NAMES[@]} ($ROWS) $(date '+%Y-%m-%d %H:%M %Z')"
EMPTY="$OUT/empty.blob"; : > "$EMPTY"
ZERO=0000000000000000000000000000000000000000

# merge3 <ours> <theirs> <onconflict>: sets MBASE MBNOTE RES_TREE CONFL[] (path:kind) ; returns 0 clean, 1 conflicts, 2 error
merge3() {
  local ours=$1 theirs=$2 oc=$3 mbs nmb idx
  mbs=$(g merge-base --all "$ours" "$theirs"); nmb=$(printf '%s\n' "$mbs" | grep -c .)
  [ "$nmb" -ge 1 ] || { echo "  ERROR: no merge base"; return 2; }
  MBASE=$(printf '%s\n' "$mbs" | head -1); MBNOTE=""; [ "$nmb" -gt 1 ] && MBNOTE=" CRISS-CROSS($nmb bases; first used)"
  idx="$OUT/idx"; rm -f "$idx"
  GIT_INDEX_FILE="$idx" g read-tree -m --aggressive -i "$MBASE" "$ours" "$theirs" || { echo "  ERROR: read-tree"; return 2; }
  CONFL=()
  local cur="" m1="" s1="" m2="" s2="" m3="" s3="" ent meta p mode sha st
  flush() {
    [ -n "$cur" ] || return 0
    local kind="" fb="$OUT/f1" fo="$OUT/f2" ft="$OUT/f3" res="$OUT/res" rc newmode blob
    if [ -n "$s2" ] && [ -n "$s3" ]; then
      if [ -n "$s1" ]; then g cat-file blob "$s1" > "$fb"; kind=content; else cp "$EMPTY" "$fb"; kind=add/add; fi
      g cat-file blob "$s2" > "$fo"; g cat-file blob "$s3" > "$ft"
      if [ "$m2" = "$m3" ]; then newmode=$m2; elif [ "$m2" = "${m1:-x}" ]; then newmode=$m3; elif [ "$m3" = "${m1:-x}" ]; then newmode=$m2; else newmode=$m3; kind="$kind+mode"; fi
      git merge-file -p -L ours -L base -L theirs "$fo" "$fb" "$ft" > "$res" 2>/dev/null; rc=$?
      if [ "$rc" = 0 ] && [[ "$kind" != *mode ]]; then
        blob=$(g hash-object -w --no-filters "$res")
        printf '0 %s\t%s\0%s %s\t%s\0' "$ZERO" "$cur" "$newmode" "$blob" "$cur" | GIT_INDEX_FILE="$idx" g update-index -z --index-info
      else
        if [ "$rc" -ge 128 ]; then kind="$kind(binary)"; else kind="$kind($rc hunk(s))"; fi
        CONFL+=("$cur:$kind")
        if [ "$oc" = union ]; then
          if [ "$rc" -ge 128 ]; then cp "$ft" "$res"; else git merge-file -p --union "$fo" "$fb" "$ft" > "$res" 2>/dev/null; fi
          blob=$(g hash-object -w --no-filters "$res")
          printf '0 %s\t%s\0%s %s\t%s\0' "$ZERO" "$cur" "$newmode" "$blob" "$cur" | GIT_INDEX_FILE="$idx" g update-index -z --index-info
        fi
      fi
    elif [ -n "$s1" ] && [ -n "$s2" ] && [ -z "$s3" ]; then
      CONFL+=("$cur:modify(chain)/delete(row)")
      [ "$oc" = union ] && printf '0 %s\t%s\0%s %s\t%s\0' "$ZERO" "$cur" "$m2" "$s2" "$cur" | GIT_INDEX_FILE="$idx" g update-index -z --index-info
    elif [ -n "$s1" ] && [ -z "$s2" ] && [ -n "$s3" ]; then
      CONFL+=("$cur:delete(chain)/modify(row)")
      [ "$oc" = union ] && printf '0 %s\t%s\0%s %s\t%s\0' "$ZERO" "$cur" "$m3" "$s3" "$cur" | GIT_INDEX_FILE="$idx" g update-index -z --index-info
    else
      CONFL+=("$cur:other(stages ${s1:+1}${s2:+2}${s3:+3})")
    fi
    cur=""; m1=""; s1=""; m2=""; s2=""; m3=""; s3=""
  }
  while IFS= read -r -d '' ent; do
    meta=${ent%%$'\t'*}; p=${ent#*$'\t'}; read -r mode sha st <<<"$meta"
    [ "$p" != "$cur" ] && flush
    cur=$p
    case "$st" in 1) m1=$mode; s1=$sha;; 2) m2=$mode; s2=$sha;; 3) m3=$mode; s3=$sha;; esac
  done < <(GIT_INDEX_FILE="$idx" g ls-files -u -z)
  flush
  RES_TREE=""
  if [ "${#CONFL[@]}" = 0 ] || [ "$oc" = union ]; then RES_TREE=$(GIT_INDEX_FILE="$idx" g write-tree) || { echo "  ERROR: write-tree"; return 2; }; fi
  [ "${#CONFL[@]}" = 0 ] && return 0 || return 1
}

ownfp() { g diff --name-only -z "$(g merge-base "$BASEC" "$1")" "$1" | tr '\0' '\n' | LC_ALL=C sort; }

case "$MODE" in
alone)
  nbad=0
  for i in "${!NAMES[@]}"; do
    n=${NAMES[$i]}; s=${SHAS[$i]}
    if g merge-base --is-ancestor "$s" "$BASEC"; then echo "ALONE $n ${s:0:10}: already contained in BASE"; continue; fi
    merge3 "$BASEC" "$s" skip; rc=$?
    if [ $rc = 0 ]; then echo "ALONE $n ${s:0:10}: clean (mb ${MBASE:0:10}$MBNOTE)"
    elif [ $rc = 1 ]; then nbad=$((nbad+1)); echo "ALONE $n ${s:0:10}: CONFLICT (mb ${MBASE:0:10}$MBNOTE) in: ${CONFL[*]}"
    else nbad=$((nbad+1)); echo "ALONE $n: ERROR"; fi
  done
  echo "ALONE DONE rows=${#NAMES[@]} not-clean=$nbad";;
pairs)
  for i in "${!NAMES[@]}"; do ownfp "${SHAS[$i]}" > "$OUT/own.$i"; done
  np=0; nc=0
  for i in "${!NAMES[@]}"; do for j in "${!NAMES[@]}"; do [ "$j" -gt "$i" ] || continue
    a=${SHAS[$i]}; b=${SHAS[$j]}
    sh=$(LC_ALL=C comm -12 "$OUT/own.$i" "$OUT/own.$j")
    [ -n "$sh" ] || continue
    if g merge-base --is-ancestor "$a" "$b" || g merge-base --is-ancestor "$b" "$a"; then echo "PAIR ${NAMES[$i]} x ${NAMES[$j]}: ANCESTRY (stacked; not a pair)"; continue; fi
    np=$((np+1)); nsh=$(printf '%s\n' "$sh" | grep -c .)
    merge3 "$a" "$b" skip; rc=$?
    if [ $rc = 0 ]; then echo "PAIR ${NAMES[$i]} x ${NAMES[$j]}: clean (mb ${MBASE:0:10}$MBNOTE; shared $nsh: $(echo $sh | cut -c1-400))"
    else nc=$((nc+1)); echo "PAIR ${NAMES[$i]} x ${NAMES[$j]}: CONFLICT (mb ${MBASE:0:10}$MBNOTE) in: ${CONFL[*]}"; fi
  done; done
  echo "PAIRS DONE overlapping-pairs=$np conflicting=$nc";;
chain)
  prev=$BASEC; nbad=0; k=0
  : > "$OUT/chain.tsv"
  for i in "${!NAMES[@]}"; do
    n=${NAMES[$i]}; s=${SHAS[$i]}; k=$((k+1))
    if g merge-base --is-ancestor "$s" "$prev"; then echo "STEP $k $n ${s:0:10}: already contained"; continue; fi
    mb0=$(g merge-base "$prev" "$s")
    sh=$(LC_ALL=C comm -12 <(g diff --name-only -z "$mb0" "$s" | tr '\0' '\n' | LC_ALL=C sort) <(g diff --name-only -z "$mb0" "$prev" | tr '\0' '\n' | LC_ALL=C sort))
    nsh=$(printf '%s\n' "$sh" | grep -c .)
    merge3 "$prev" "$s" "$ONCONFLICT"; rc=$?
    if [ $rc = 2 ]; then echo "STEP $k $n: ERROR"; exit 3; fi
    if [ $rc = 0 ]; then
      echo "STEP $k $n ${s:0:10}: clean (mb ${MBASE:0:10}$MBNOTE; shared paths $nsh: $(echo $sh | cut -c1-600))"
    else
      nbad=$((nbad+1)); echo "STEP $k $n ${s:0:10}: CONFLICT (mb ${MBASE:0:10}$MBNOTE) in: ${CONFL[*]}  [shared paths $nsh]"
    fi
    if [ $rc = 0 ] || [ "$ONCONFLICT" = union ]; then
      prev=$(printf 'premap step %s: %s\n' "$k" "$n" | g -c user.name=premap -c user.email=premap@invalid commit-tree --no-gpg-sign "$RES_TREE" -p "$prev" -p "$s") || exit 3
    fi
    printf '%s\t%s\t%s\t%s\n' "$k" "$n" "${s:0:10}" "$(g rev-parse --short=10 "$prev")" >> "$OUT/chain.tsv"
  done
  echo "CHAIN DONE rows=${#NAMES[@]} conflicted-steps=$nbad onconflict=$ONCONFLICT head=$(g rev-parse --short=12 "$prev") tree=$(g rev-parse --short=12 "$prev^{tree}") (dangling commit: no ref)";;
*) echo "ABORT: MODE $MODE"; exit 2;;
esac
