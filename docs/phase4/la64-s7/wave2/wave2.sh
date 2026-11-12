#!/usr/bin/env bash
# wave2.sh <check|plan|refs|dispatch|runs> STATE_DIR
#
# A copy of wave1/wave1.sh (lane G); only the shard list, the row total and these header lines differ.
# S7 wave 2 (docs/phase4/DATA-la64-s7-bank-plan.md, section 2.2): six sweep-shard dispatches, goos=linux,
# arch=both, each on its OWN tip ref at master's tip, once the gate commits are on master.
#
#   check     read origin: master's tip, and whether each gate commit is an ancestor of it. rc 0 only when READY.
#   plan      check, then print what refs and dispatch WOULD do. Changes nothing.
#   refs      check (must be READY), then push one tip ref per shard at master's tip.
#   dispatch  check (must be READY), then one dispatch per shard on its own tip ref; records the run ids.
#   runs      list the recorded runs with head SHA and state; a head SHA that is not the recorded tip is named.
#
# Nothing here banks: a wave's rows are READ from the artifacts afterwards. Wave 2 is dispatched only after
# wave 1's rows are READ; check does not read that, so the dispatching lane confirms it on the mailbox first.
#
# STATE_DIR  a directory OUTSIDE the repository: status.txt, runs.tsv and the dispatch logs go there.
# Environment:
#   TIP_PREFIX  the tip refs' prefix, in the dispatching lane's own namespace (claude/<lane>-tip). Needed by
#               plan, refs and dispatch. Shard i of six gets <TIP_PREFIX>-<master sha10>-<i>.
#   GH          the gh executable (default: gh on PATH).
#   REPO        owner/name passed to gh as --repo (default: gh resolves it from this clone's remote).
#   GATES       "sha:name ..." replacing the gate list, for a seat that landed restated under another commit.
set -u
MODE=${1:?mode: check|plan|refs|dispatch|runs}
S=${2:?state directory (outside the repository)}
GH=${GH:-gh}
HERE=$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)
G=$(cd "$(git -C "$HERE" rev-parse --show-toplevel)" && pwd) || exit 2
SHARDS="A-3 A-10 A-5 A-11 A-14 A-15"
# gate commits: the stub bodies seat (sched_getaffinity, exit, osyield) and the S7-a fixups. The ledger edit is
# reported, not a gate.
GATES=${GATES:-"0282fe03f4:stub-bodies fbb9169c42:s7a-fixups"}
REPORT="03f3f0edc2:r3-ledger"
R=()
[ -n "${REPO:-}" ] && R=(--repo "$REPO")
[ -d "$S" ] || { echo "state directory $S does not exist"; exit 2; }
case "$(cd "$S" && pwd)/" in "$G"/*) echo "state directory is inside the repository"; exit 2 ;; esac
ST=$S/status.txt
say() { echo "$(date +%H:%M:%S) $*" | tee -a "$ST"; }
gh_() { (cd "$G" && "$GH" "$@" ${R[@]+"${R[@]}"}); }

git -C "$G" fetch -q origin master 2>> "$ST"
M=$(git -C "$G" ls-remote origin refs/heads/master | cut -c1-40)
[ -n "$M" ] || { say "master not read from origin"; exit 2; }
git -C "$G" cat-file -e "$M^{commit}" 2>/dev/null || { say "master's tip $M is not in this clone after the fetch"; exit 2; }
T=${M:0:10}
ready=1
say "$MODE: master=$T"
for g in $GATES; do
  s=${g%%:*}
  n=${g##*:}
  if git -C "$G" merge-base --is-ancestor "$s" "$M" 2>/dev/null; then say "gate $n $s: on master"; else say "gate $n $s: NOT an ancestor of master"; ready=0; fi
done
for g in $REPORT; do
  s=${g%%:*}
  n=${g##*:}
  if git -C "$G" merge-base --is-ancestor "$s" "$M" 2>/dev/null; then say "report $n $s: on master"; else say "report $n $s: not on master"; fi
done
total=0
for k in $SHARDS; do
  f=$HERE/$k.filter
  [ -s "$f" ] || { say "missing filter $f"; exit 2; }
  c=$(tr ',' '\n' < "$f" | grep -c .)
  total=$((total + c))
done
say "filters: 6 shards, $total rows (plan: 80)"
[ "$total" = 80 ] || { say "row total is not 80"; exit 2; }
if [ "$ready" = 1 ]; then say "READY"; else say "NOT READY"; fi
[ "$MODE" = check ] && { [ "$ready" = 1 ]; exit $?; }

if [ "$MODE" = runs ]; then
  [ -s "$S/runs.tsv" ] || { say "no runs recorded"; exit 2; }
  while IFS=$'\t' read -r k ref run tip; do
    j=$(gh_ run view "$run" --json headSha,status,conclusion --jq '"\(.headSha) \(.status) \(.conclusion)"' 2>&1)
    h=${j%% *}
    say "$k $ref run=$run ${j#* } head=${h:0:10} $([ "$h" = "$tip" ] && echo at-recorded-tip || echo "NOT-THE-RECORDED-TIP ${tip:0:10}")"
  done < "$S/runs.tsv"
  exit 0
fi

case "$MODE" in plan|refs|dispatch) ;; *) say "unknown mode $MODE"; exit 2 ;; esac
[ -n "${TIP_PREFIX:-}" ] || { say "refused: $MODE needs TIP_PREFIX (claude/<lane>-tip)"; exit 2; }
if [ "$MODE" != plan ] && [ "$ready" != 1 ]; then say "refused: $MODE needs READY"; exit 3; fi

i=0
for k in $SHARDS; do
  i=$((i + 1))
  ref=$TIP_PREFIX-$T-$i
  case "$MODE" in
    plan)
      say "would push $T -> $ref, then dispatch $k ($(tr ',' '\n' < "$HERE/$k.filter" | grep -c .) rows) on it"
      ;;
    refs)
      git -C "$G" push -q origin "$M:refs/heads/$ref" >> "$ST" 2>&1
      rc=$?
      say "ref $ref push rc=$rc remote=$(git -C "$G" ls-remote origin "refs/heads/$ref" | cut -c1-10)"
      [ "$rc" = 0 ] || exit 4
      ;;
    dispatch)
      r=$(git -C "$G" ls-remote origin "refs/heads/$ref" | cut -c1-40)
      [ "$r" = "$M" ] || { say "ref $ref is at ${r:0:10}, not the tip; stopped"; exit 4; }
      cut -f1 "$S/runs.tsv" 2>/dev/null | grep -qx "$k" && { say "$k already has a recorded run; skipped"; continue; }
      gh_ workflow run os-matrix.yml --ref "$ref" -f goos=linux -f stage=sweep-shard -f arch=both -f "filter=$(cat "$HERE/$k.filter")" > "$S/$k-dispatch-log.txt" 2>&1
      rc=$?
      run=$(grep -o 'runs/[0-9]*' "$S/$k-dispatch-log.txt" | head -n 1 | cut -d/ -f2)
      if [ "$rc" = 0 ] && [ -z "$run" ]; then
        # gh printed no run URL: read the newest run of this workflow on the shard's own ref.
        n=0
        until [ -n "$run" ] || [ "$n" -ge 12 ]; do
          sleep 5
          n=$((n + 1))
          run=$(gh_ run list --workflow os-matrix.yml --branch "$ref" --limit 1 --json databaseId --jq '.[0].databaseId' 2>> "$ST")
        done
      fi
      say "dispatch $k on $ref rc=$rc run=${run:-none}"
      [ "$rc" = 0 ] && [ -n "$run" ] || exit 4
      printf '%s\t%s\t%s\t%s\n' "$k" "$ref" "$run" "$M" >> "$S/runs.tsv"
      ;;
  esac
done
exit 0
