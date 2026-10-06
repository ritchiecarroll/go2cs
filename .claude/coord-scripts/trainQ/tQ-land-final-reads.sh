#!/usr/bin/env bash
# TRAIN Q landing, the FINAL READS AT THE HEAD THAT LANDS (checklist section 6, step 3), one after the other:
#   RUN=<the battery's run folder> HEADFULL=<the refresh head, full sha> bash tQ-land-final-reads.sh
# DERIVED 2026-10-06 from coord-scratch/tP/land-final-reads.sh, the script COORD wrote when the read-only audit of P's
# landing returned two should-fix items (trainQ/tQ-CHANGES.md Q8, Q4, Q5):
#   1. THE CONVERTER SUITE AT THE REFRESH HEAD: go test ./... in src/go2cs. The battery read it at the union's fixup
#      head, ONE COMMIT BELOW what lands: the refresh commits the re-emitted test sources, and the suite's tree guards
#      walk every .cs under src/core (the by-ref receiver guard, the attribute guards, the stdlib-metadata guard). At Q
#      the refresh is LARGE by construction (the carrier's partial rendering in every swept test source), so a guard
#      that reads those files meets them for the first time here. NOT under GOWORK=off (the suite's
#      TestModuleCachePoisonedGoWorkLoad needs the ambient lookup: that switch belongs to module legs only).
#      EXPECT rc 0, every package 'ok', and 0 tracked changes after it.
#   2. WHAT THE BATTERY OWES (Q4): every line of RUN/mod-logs/OWED-rereads.txt is a reading the battery could not close
#      (a MOD KNOWN-EXTERNAL: the Go oracle failed on a network or quota text; a timing-class line). Each is re-read
#      ALONE at this head by tQ-reread.sh (x/mod once, x/sync three times and once more into the same root), over a
#      CLEAN tree. A line with no CLEAN re-read at THIS head is a STOP: the landing is not green on a reading nobody
#      took.
# REVIEW ROUND 1 (trainQ/tQ-CHANGES.md, REVIEW ROUND 1, S1): THE OWED GATE PASSED BY ABSENCE, and RUN was tied to
# nothing. 'No OWED file' was the normal clean state AND what a wrong or stale RUN looks like (the run1 of a battery
# relaunched into run2: P had exactly that pair), so a battery that ended EXIT 7 could land with its owed reading never
# taken; mkdir -p even created a mistyped RUN. Now, before anything is read or written:
#   * RUN must be the run folder of A BATTERY THAT ENDED: battery.console.log with a 'battery rc=' line and
#     tQ-logs/SUMMARY.txt with its PRE, END and EXIT lines (runtie reads them; the folder is never created here);
#   * THAT battery read THIS train's head: its 'PRE head=' and END 'head=' are one sha, and it is HEADFULL (no refresh
#     was needed) or HEADFULL's parent (the refresh sits on it). Anything else is refused unless COORD names the
#     battery's head, BATTERY_HEAD_OK=<its 10 chars>, and that commit is an ancestor of HEADFULL (a hand bank commit
#     under the refresh): stamped;
#   * the battery ended EXIT 0 or EXIT 7, and its rc line says the same (BATTERY_RC_OK=<n> for a red COORD ruled by
#     name: stamped);
#   * mod-logs/OWED-rereads.txt EXISTS (the module legs always create it; absent = STOP) and holds exactly as many
#     lines as the SUMMARY's 'OWED AT THE LANDING HEAD ... owed-lines=<n>' says.
# It never commits, pushes or posts. Last line: 'FINAL READS at <head>: GREEN' (exit 0) or a STOP line (exit 1).
set -u
SD=$(cd "$(dirname "$0")" && pwd)
RUN=${RUN:?set RUN to the run folder of the battery of record}
HEADFULL=${HEADFULL:?set HEADFULL to the head that lands (the signed refresh commit), full sha}
W=${W:-/h/go2cs-tmp-coord/tQ}
case "$W" in /h/go2cs-tmp-coord/tQ|/h/go2cs-tmp-coord/tQ-[A-Za-z0-9]*) ;; *) echo "ABORT: W=$W is not the tQ worktree or a tQ-<name> rehearsal worktree"; exit 2 ;; esac
stamp(){ echo "$(date +%H:%M:%S) $*"; }
# >>> tcn (tQ-controls.sh arm TC extracts the line between the markers from EVERY script that carries one and runs its four arms on each copy)
tcn(){ local o rc; o=$(git -C "$1" status --porcelain --untracked-files=no 2>/dev/null); rc=$?; [ "$rc" = 0 ] || { echo UNREAD; return 1; }; printf '%s\n' "$o" | grep -c .; }
# <<< tcn
# >>> runtie (tQ-controls.sh arm RT extracts the lines between the markers from both landing scripts)
# runtie <run folder>: the battery of record read FROM ITS OWN FILES. Prints 'rc=<n> pre=<head10> end=<head10> exit=<n>
# owed=<n>' (a field that did not read is '?'); returns 1 when the folder holds no battery.console.log or SUMMARY.
# Each field is the LAST such line (a SUMMARY is appended to: the last battery that wrote it is the one that ended).
runtie(){
  local run=$1 cons sum rc pre end ex owed
  cons=$run/battery.console.log; sum=$run/tQ-logs/SUMMARY.txt
  [ -f "$cons" ] && [ -f "$sum" ] || { echo "rc=? pre=? end=? exit=? owed=?"; return 1; }
  rc=$(tr -d '\r' < "$cons" | sed -n 's/^battery rc=\([0-9][0-9]*\)$/\1/p' | tail -n 1)
  pre=$(tr -d '\r' < "$sum" | sed -n 's/^[0-9:]* PRE head=\([0-9a-f]\{10\}\) .*/\1/p' | tail -n 1)
  end=$(tr -d '\r' < "$sum" | sed -n 's/^[0-9:]* END tracked-changes=.* head=\([0-9a-f]\{10\}\) .*/\1/p' | tail -n 1)
  ex=$(tr -d '\r' < "$sum" | sed -n 's/^[0-9:]* EXIT \([0-9][0-9]*\): .*/\1/p' | tail -n 1)
  owed=$(tr -d '\r' < "$sum" | sed -n 's/^[0-9:]* OWED AT THE LANDING HEAD .* owed-lines=\([0-9A-Z]*\): .*/\1/p' | tail -n 1)
  echo "rc=${rc:-?} pre=${pre:-?} end=${end:-?} exit=${ex:-?} owed=${owed:-?}"
}
# rungate <prep|final> <runtie's line> <head, 10 chars> [<parent, 10 chars> <owed-file line count or MISSING> <yes|no:
# the battery's head is an ancestor of the head>] -- THE DECISION, as one function with no git and no file in it, so
# its control can run every arm on this box (tQ-controls.sh arm RT, the same arms on both scripts' copies). Prints ONE
# line and returns: 'OK: ...' 0; 'REFUSED: ...' 2 (this run folder is not the battery of record for this head);
# 'STOP: ...' 1 (final only: what the battery owes is unread or disagrees with its own SUMMARY).
#   prep   the battery read THIS head (its PRE and END lines), and ended EXIT 0 or 7 with the same rc line.
#   final  the battery read this head or its PARENT (the refresh sits on it), or the head COORD names with
#          BATTERY_HEAD_OK=<10 chars> when that commit is an ancestor; EXIT 0 or 7; and the owed file EXISTS with
#          exactly the line count the battery's own OWED line stamped.
# BATTERY_RC_OK=<n> admits a battery that ended EXIT n with rc n (a red COORD ruled by name), in both modes.
rungate(){
  local mode=$1 rt=$2 h=$3 par=${4:-} ow=${5:-} anc=${6:-no} kv rc='?' pre='?' end='?' ex='?' owed='?' tie ver
  for kv in $rt; do case "$kv" in rc=*) rc=${kv#rc=} ;; pre=*) pre=${kv#pre=} ;; end=*) end=${kv#end=} ;; exit=*) ex=${kv#exit=} ;; owed=*) owed=${kv#owed=} ;; esac; done
  [ "$rc" != '?' ] || { echo "REFUSED: battery.console.log holds no 'battery rc=<n>' line: the battery has not ended (or was not launched through its launcher)"; return 2; }
  [ "$pre" != '?' ] && [ "$end" != '?' ] && [ "$ex" != '?' ] || { echo "REFUSED: tQ-logs/SUMMARY.txt does not hold a battery's PRE head, END head and EXIT lines ($rt): it did not run to its end"; return 2; }
  [ "$pre" = "$end" ] || { echo "REFUSED: the battery's PRE head ($pre) is not its END head ($end): the SUMMARY holds two batteries, or the tree moved under one"; return 2; }
  if [ "$pre" = "$h" ]; then tie="the battery read this head ($h) itself"
  elif [ "$mode" = final ] && [ -n "$par" ] && [ "$pre" = "$par" ]; then tie="the battery read this head's parent $par (the refresh sits on it)"
  elif [ "$mode" = final ] && [ -n "${BATTERY_HEAD_OK:-}" ] && [ "$BATTERY_HEAD_OK" = "$pre" ] && [ "$anc" = yes ]; then tie="BATTERY_HEAD_OK=$pre (COORD's explicit call): the battery's head is an ancestor of this head"
  else echo "REFUSED: the battery of this run folder read head $pre, not $h$([ "$mode" = final ] && echo " and not its parent ${par:-(none)}"): it is NOT the battery of record for this head (a stale run folder: run1 beside run2?); its rewrites and its owed lines are another head's$([ "$mode" = final ] && echo ". BATTERY_HEAD_OK=<the battery's head, 10 chars> only by COORD's explicit call, and only for an ancestor")"; return 2; fi
  case "$ex:$rc" in
    0:0|7:7) ver="EXIT $ex" ;;
    *) if [ -n "${BATTERY_RC_OK:-}" ] && [ "$BATTERY_RC_OK" = "$rc" ] && [ "$ex" = "$rc" ]; then ver="EXIT $ex, BATTERY_RC_OK=$rc (COORD's explicit call: a red ruled by name)"
       else echo "REFUSED: the battery of record ended EXIT $ex with 'battery rc=$rc' (want 0, or 7 = green with a reading owed at the landing head; the two lines must agree). A red that COORD ruled by name: BATTERY_RC_OK=$rc"; return 2; fi ;;
  esac
  if [ "$mode" = final ]; then
    [ "$ow" != MISSING ] || { echo "STOP: mod-logs/OWED-rereads.txt does not exist. The module legs ALWAYS create it (empty when nothing is owed), so its absence means they did not run in this folder: what is owed at this head is UNREAD, never 'nothing'"; return 1; }
    [ "$owed" = "$ow" ] || { echo "STOP: the battery's SUMMARY says owed-lines=$owed and mod-logs/OWED-rereads.txt holds $ow line(s): the file is not the one that battery read (edited, or another run's)"; return 1; }
    [ "$ex" != 0 ] || [ "$ow" = 0 ] || { echo "STOP: the battery ended EXIT 0 and mod-logs/OWED-rereads.txt holds $ow line(s): an owed reading ends a battery EXIT 7"; return 1; }
  fi
  echo "OK: $tie; $ver; owed-lines=$owed"
}
# owedcount <file>: MISSING when the owed file does not exist (NEVER 0: its absence is not 'nothing owed'), else the
# count of its non-empty lines. Inside the markers so the control runs it on a folder with and without the file.
owedcount(){ local n; if [ -f "$1" ]; then n=$(tr -d '\r' < "$1" | grep -c .); echo "${n:-0}"; else echo MISSING; fi; }
# <<< runtie
[ -z "${MSYS_NO_PATHCONV:-}" ] || { stamp "REFUSED: MSYS_NO_PATHCONV is set in this shell: every git -C read below would be vacuous (Q5)"; exit 2; }
[ -d "$RUN" ] || { stamp "REFUSED: RUN=$RUN is not a folder (the run folder of the battery of record is never created here: a mistyped RUN is a stop)"; exit 2; }
RT=$(runtie "$RUN") || { stamp "REFUSED: $RUN holds no battery.console.log or no tQ-logs/SUMMARY.txt: it is not a battery's run folder"; exit 2; }
[ "$(git -C "$W" rev-parse HEAD 2>/dev/null)" = "$HEADFULL" ] || { stamp "REFUSED: $W is not at $HEADFULL"; exit 2; }
PARFULL=$(git -C "$W" rev-parse -q --verify "$HEADFULL^1" 2>/dev/null || true)
BPRE=$(printf '%s' "$RT" | sed -n 's/.* pre=\([0-9a-f?]*\) .*/\1/p')
BANC=no; case "$BPRE" in [0-9a-f][0-9a-f][0-9a-f][0-9a-f][0-9a-f][0-9a-f][0-9a-f][0-9a-f][0-9a-f][0-9a-f]) git -C "$W" merge-base --is-ancestor "$BPRE" "$HEADFULL" 2>/dev/null && BANC=yes ;; esac
OW="$RUN/mod-logs/OWED-rereads.txt"
nowed=$(owedcount "$OW")   # MISSING when the file is absent, never 0 (the control plants both)
BG=$(rungate final "$RT" "${HEADFULL:0:10}" "${PARFULL:0:10}" "$nowed" "$BANC"); bgrc=$?
[ "$bgrc" = 0 ] || { stamp "$BG [run folder $RUN; the head that lands ${HEADFULL:0:10}]"; exit "$bgrc"; }
X=$RUN/landing; mkdir -p "$X" || exit 2
subj=$(git -C "$W" log -1 --format=%s HEAD)
case "$subj" in 'refresh: TRAIN Q'*|'fixup: TRAIN Q'*|'fixup-'[0-9]*': TRAIN Q'*) ;; *) stamp "NOTE: the head's subject is not a TRAIN Q refresh or fixup commit (${subj:0:80}): this reads the head it was given" ;; esac
case "$(git -C "$W" log -1 --format=%G? HEAD)" in G|U) ;; *) stamp "REFUSED: the head is not signed (want G or U): the commit that lands is signed BEFORE its final reads"; exit 2 ;; esac
TC=$(tcn "$W")
[ "$TC" = 0 ] || { stamp "REFUSED: tracked changes in the worktree: $TC (UNREAD = git did not read it). The final reads are taken over a CLEAN tree"; exit 2; }
stamp "PRE head=$(git -C "$W" rev-parse --short=10 HEAD) (${subj:0:70}) tree=$(git -C "$W" rev-parse --short=10 'HEAD^{tree}') tracked changes=$TC (read through tcn before any path setting)"
stamp "PRE battery of record: $RUN -- ${BG#OK: } (mod-logs/OWED-rereads.txt exists and holds $nowed line(s), the count that battery stamped)"
GR=$(cygpath -w "$HOME/sdk/go1.24.13")
case "$GR" in */*) stamp "ABORT: GOROOT spelling (floor 6)"; exit 2 ;; esac
(
  export PATH="$HOME/sdk/go1.24.13/bin:$HOME/dotnet10:$PATH"
  export GOROOT="$GR" GOTOOLCHAIN=local CGO_ENABLED=0 GOFLAGS=
  export DOTNET_ROOT="$(cygpath -w "$HOME/dotnet10")" MSBUILDDISABLENODEREUSE=1 DOTNET_CLI_TELEMETRY_OPTOUT=1
  export TEMP="H:\go2cs-tmp-coord\coord-scratch\tmp" TMP="H:\go2cs-tmp-coord\coord-scratch\tmp" TMPDIR=/h/go2cs-tmp-coord/coord-scratch/tmp
  export GOCACHE="H:\go-cache\go-build" GOTMPDIR="H:\go2cs-tmp-coord\coord-scratch\tmp" NUGET_PACKAGES="H:\nuget\packages"
  unset GOWORK GO2CS_MODULE_ROOT
  cd "$W/src/go2cs" && go test -count=1 -timeout 90m ./... > "$X/land-gotest.log" 2>&1
); grc=$?
stamp "1. go test ./... at the head that lands: rc=$grc"
grep -a -E '^(--- FAIL|FAIL|ok  |panic:)' "$X/land-gotest.log" | cut -c1-160
nok=$(grep -a -c -E '^ok  ' "$X/land-gotest.log"); nfail=$(grep -a -c -E '^(--- FAIL|FAIL)' "$X/land-gotest.log")
TC2=$(tcn "$W")
stamp "   packages ok=$nok FAIL lines=$nfail; tracked changes after go test: $TC2 (EXPECT 0)"
[ "$grc" = 0 ] && [ "$nok" -ge 1 ] && [ "$nfail" = 0 ] || { stamp "STOP: the converter suite is not green at the head that lands (land-gotest.log)"; exit 1; }
[ "$TC2" = 0 ] || { stamp "STOP: go test left tracked changes ($TC2)"; git -C "$W" status --porcelain --untracked-files=no | head -n 5; exit 1; }

stamp "2. owed re-reads: $nowed line(s) in $(basename "$OW") (the file exists and its count is the battery's own: read above, before anything was written)"
if [ "$nowed" != 0 ]; then
  tr -d '\r' < "$OW" | cut -c1-240 | sed 's/^/     /'
  need=''
  grep -aq '^XM-EXTERNAL ' "$OW" && need="$need xmod"
  grep -aqE '^(XS-EXTERNAL|XS-TIMING) ' "$OW" && need="$need xsync"
  other=$(tr -d '\r' < "$OW" | grep -avcE '^(XM-EXTERNAL|XS-EXTERNAL|XS-TIMING) ')
  [ "$other" = 0 ] || { stamp "STOP: $other owed line(s) of a class this script does not know how to re-read: COORD reads them by hand"; exit 1; }
  for what in $need; do
    stamp "   re-reading $what ALONE at this head (tQ-reread.sh)"
    HEADFULL=$HEADFULL RUN=$RUN W=$W bash "$SD/tQ-reread.sh" "$what" land > "$X/reread-$what.console.log" 2>&1; rrc=$?
    tail -n 6 "$X/reread-$what.console.log" | cut -c1-240 | sed 's/^/     /'
    [ "$rrc" = 0 ] && grep -aqE "^REREAD $what land at ${HEADFULL:0:10}: CLEAN " "$RUN/rereads.txt" 2>/dev/null \
      || { stamp "STOP: the owed re-read of $what is not CLEAN at this head (rc=$rrc; $X/reread-$what.console.log). An external failure that repeats is still evidence, and it is COORD's ruling by name, never this script's pass"; exit 1; }
    TC3=$(tcn "$W"); [ "$TC3" = 0 ] || { stamp "STOP: tracked changes after the $what re-read: $TC3"; exit 1; }
  done
fi
stamp "FINAL READS at $(git -C "$W" rev-parse --short=10 HEAD): GREEN (the converter suite at the head that lands; the battery of record $(basename "$RUN") tied to this head: ${BG#OK: }; owed re-reads: $([ "$nowed" = 0 ] && echo 'the file exists and is empty: none owed' || echo "$nowed line(s), each re-read CLEAN")). NOT pushed: announce, then push (floor 9)"
