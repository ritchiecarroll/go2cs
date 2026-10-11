#!/usr/bin/env bash
# TRAIN FL landing, PREPARATION. DERIVED 2026-10-08 from trainQ/tQ-land-prep.sh. FL's changes (trainFL/tFL-CHANGES.md):
# 4b/4e classify the FACE LIFT (FL1: one class per step; the refresh at FL is the whole corpus of committed test
# sources the windows boxes re-emit, by construction) and 4e adds the face lift's own census: the leaving-attribute
# lines in committed test sources at HEAD and in the refreshed worktree (what is left belongs to rows no windows box
# swept, to sources -tests does not emit on windows, or to the by-design keeps). THE ROUTE (FL2, Q's landing): prep at
# the battery's own head, COORD signs 'refresh: TRAIN T3', a follow-up then lands as a single-parent 'fixup-N: TRAIN T3'
# ON the refresh (tT3-fixup.sh FIXUP_N=k+1 admits the refresh below it), and the final reads run at that head with
# BATTERY_HEAD_OK=<the battery's head> (tT3-land-rereads.sh chains them). Q's header follows.
# TRAIN Q landing, PREPARATION (checklist section 6, step 1; its own steps are numbered 2a to 4e below, P's numbers,
# which the log names keep), at the union's battery head. It STOPS at the first
# failure and it NEVER commits, pushes or posts: COORD reads every line, signs the refresh commit and lands by hand.
#   RUN=<the battery's run folder> MASTER=<the union's base, full sha> HEADFULL=<the battery head, full sha> \
#     I9P=<the i9's tracked-changes.patch> I9P_SHA256=<its sha256 as the i9 posted it> bash tT3-land-prep.sh
# DERIVED 2026-10-06 from coord-scratch/tP/tP-land-prep.sh, the script COORD wrote for P's landing (it carried P's
# shas and one P-only cross-check; here every sha is a REQUIRED variable and that cross-check is gone), with P's
# lessons (trainQ/tQ-CHANGES.md Q5, Q6, Q8):
#   * every git read of the worktree goes through tcn (UNREAD when git fails) and no MSYS_NO_PATHCONV is set here;
#   * pwsh is started the way the battery's roster guard starts it: under the pinned-go PATH with DOTNET_ROOT unset.
#     pwsh on this box is a .NET 8 global tool and does not START when the .NET 10 SDK folder is first on PATH (P's
#     landing read 'rc=150' there before it was fixed to this form).
#   2a  the landing precheck (tT3-helpers.py precheck, head mode) over the battery's own seats-effective.txt
#   2b  the roster guard under PowerShell 7 and Windows PowerShell 5.1: pass, and 0 rows with an execution config
#   3   the bank step, STATED from the battery's ROSTERROW lines (a row that moved is COORD's to bank by hand, after
#       this script; at P no row moved)
#   4a  the tree back at HEAD (the T legs' material is kept as the battery's T-rewrites.patch)
#   4b  testsrc-refresh over the i7's S and T patches and the i9's patch: the committed TEST SOURCES OF RECORD ARE THE
#       WINDOWS EMISSION (the i7's and the i9's rewrites at ONE head; a linux lane's rewrites are never input).
#       What it COMMITS-TO-BE: tracked *_test.cs, package_test_info.cs and package_info_internal_test.cs under src/core
#       that are neither hand-owned nor golib. What it REFUSES, by category, never written: production files
#       (package_init.cs, go2cs_test_host.cs), proof pages (docs/validation), csproj, review siblings (*.cs.auto),
#       hand-owned files, anything that creates, deletes, renames or binary-patches a file, and any file two windows
#       inputs rewrite DIFFERENTLY (a CONFLICT: exit 1).
#   4c  git apply --check, then git apply
#   4d  testsrc-refresh --check-worktree: ONLY committed test sources changed
#   4e  Q (Q18): the class census of what is about to be committed. EXPECT the N-PARTIAL class in bulk (the carrier
#       renders a no-inline method as a partial method; 738 such lines in 173 committed test sources at TRAIN P's
#       landed line, of which the two boxes' rows re-emit theirs) and READ every OTHER hunk before signing.
# After COORD's signed refresh commit: tT3-land-final-reads.sh at THAT head (the converter suite again, the owed re-reads).
set -u
SD=$(cd "$(dirname "$0")" && pwd)
RUN=${RUN:?set RUN to the run folder of the battery of record}
MASTER=${MASTER:?set MASTER to the base the union was assembled on (full sha)}
HEADFULL=${HEADFULL:?set HEADFULL to the head the battery read (full sha)}
I9P=${I9P:?set I9P to the i9 tracked-changes.patch (with context) for THIS head}
I9P_SHA256=${I9P_SHA256:?set I9P_SHA256 to the sha256 the i9 posted for that patch}
W=${W:-/h/go2cs-tmp-coord/tT3}
case "$W" in /h/go2cs-tmp-coord/tT3|/h/go2cs-tmp-coord/tT3-[A-Za-z0-9]*) ;; *) echo "ABORT: W=$W is not the tT3 worktree or a tT3-<name> rehearsal worktree"; exit 2 ;; esac
L=$RUN/tT3-logs
stamp(){ echo "$(date +%H:%M:%S) $*"; }
die(){ stamp "STOP: $*"; exit 1; }
# >>> tcn (tT3-controls.sh arm TC extracts the line between the markers from EVERY script that carries one and runs its four arms on each copy)
tcn(){ local o rc; o=$(git -C "$1" status --porcelain --untracked-files=no 2>/dev/null); rc=$?; [ "$rc" = 0 ] || { echo UNREAD; return 1; }; printf '%s\n' "$o" | grep -c .; }
# <<< tcn
# >>> runtie (tT3-controls.sh arm RT extracts the lines between the markers from both landing scripts)
# runtie <run folder>: the battery of record read FROM ITS OWN FILES. Prints 'rc=<n> pre=<head10> end=<head10> exit=<n>
# owed=<n>' (a field that did not read is '?'); returns 1 when the folder holds no battery.console.log or SUMMARY.
# Each field is the LAST such line (a SUMMARY is appended to: the last battery that wrote it is the one that ended).
runtie(){
  local run=$1 cons sum rc pre end ex owed
  cons=$run/battery.console.log; sum=$run/tT3-logs/SUMMARY.txt
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
# its control can run every arm on this box (tT3-controls.sh arm RT, the same arms on both scripts' copies). Prints ONE
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
  [ "$pre" != '?' ] && [ "$end" != '?' ] && [ "$ex" != '?' ] || { echo "REFUSED: tT3-logs/SUMMARY.txt does not hold a battery's PRE head, END head and EXIT lines ($rt): it did not run to its end"; return 2; }
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
[ -z "${MSYS_NO_PATHCONV:-}" ] || die "MSYS_NO_PATHCONV is set in this shell: every git -C read below would be vacuous (Q5). Unset it and relaunch"
# REVIEW ROUND 1 (trainQ/tQ-CHANGES.md, REVIEW ROUND 1, S2): RUN WAS TIED TO NOTHING. The check was 'some battery rc=
# line exists', with the rc VALUE unread and no read of WHICH head that battery ran at: COORD's P fragment was tied by
# literal shas, and making RUN and HEADFULL free variables lost the tie. With run1 at the first fixup and run2 at a
# fixup-N, RUN=run1 HEADFULL=<fixup-N> would have built the refresh from the OLDER converter's rewrites, and git apply
# --check plus --check-worktree (which reads the CLASS of file, not its content) would not have noticed. Now the
# battery's own PRE and END lines must both name HEADFULL's first ten characters, and its rc line and EXIT line must
# both read 0 or 7 (BATTERY_RC_OK=<n> for a red COORD ruled by name: stamped here and on PREP DONE).
[ -d "$RUN" ] || die "RUN=$RUN is not a folder"
RT=$(runtie "$RUN") || die "$RUN holds no battery.console.log or no tT3-logs/SUMMARY.txt: it is not a battery's run folder"
BG=$(rungate prep "$RT" "${HEADFULL:0:10}") || die "${BG#REFUSED: } [run folder $RUN; HEADFULL ${HEADFULL:0:10}: the S and T rewrites of another head are NEVER the refresh's input]"
BVER=${BG#OK: }
[ "$(git -C "$W" rev-parse HEAD 2>/dev/null)" = "$HEADFULL" ] || die "$W is not at $HEADFULL"
[ "$(tcn "$W")" != UNREAD ] || die "git did not read $W"
stamp "PRE battery of record: $RUN -- $BVER (what it owes is read at the refresh head by tT3-land-final-reads.sh)"
RM=$(git -C /h/Projects/go2cs ls-remote origin refs/heads/master | cut -f1)
[ -n "$RM" ] || die "ls-remote origin master read nothing"
[ "$RM" = "$MASTER" ] || die "origin master is ${RM:0:10}, not the base ${MASTER:0:10}: merge it into the union first and re-read (tT3-README.md MS11); this script prepares a landing on an unmoved master only"
[ "$(sha256sum "$I9P" | cut -c1-64)" = "$I9P_SHA256" ] || die "the i9's refresh patch does not hash as posted ($(sha256sum "$I9P" | cut -c1-16)...)"
for f in S-rewrites.patch T-rewrites.patch seats-effective.txt; do [ -s "$L/$f" ] || die "missing $L/$f"; done
WB=$(cygpath -w "$W")
ORIGPATH="$PATH"
# The PATH pwsh STARTS under: the pinned go first, then the launching shell's PATH with every entry that ends in
# /dotnet10 removed (this shell may be a build shell that already carries the .NET 10 SDK folder first).
GOPIN_PATH="$HOME/sdk/go1.24.13/bin:$(printf '%s' "$ORIGPATH" | tr ':' '\n' | grep -v -i -E '/dotnet10/?$' | paste -sd: -)"
PWSH=$HOME/.dotnet/tools/pwsh
export PATH="$HOME/sdk/go1.24.13/bin:$HOME/dotnet10:$PATH"
hp(){ ( cd "$RUN" && python -B tT3-helpers.py "$@" ); }

pv=$(env -u DOTNET_ROOT PATH="$GOPIN_PATH" "$PWSH" -NoProfile -Command '$PSVersionTable.PSVersion.ToString()' 2>&1 < /dev/null | tr -d '\r' | tail -n 1); prc=$?
case "$pv" in [0-9]*.[0-9]*) stamp "PRE pwsh starts under the pinned-go PATH: $pv" ;; *) die "pwsh does not START under the pinned-go PATH (read: ${pv:0:160}): the roster guard's PowerShell 7 arm would read rc 150, not a verdict" ;; esac

stamp "STEP 2a: the landing precheck (head mode)"
hp precheck "$WB" "$MASTER" tT3-logs/seats-effective.txt head > "$L/landing-precheck.log" 2>&1; rc=$?
tail -n 1 "$L/landing-precheck.log" | cut -c1-200
[ "$rc" = 0 ] && tail -n 1 "$L/landing-precheck.log" | grep -q 'PRECHECK hard-failures=0 notes=0 mode=head' \
  || { grep -a -v '^ok ' "$L/landing-precheck.log" | head -n 12 | cut -c1-240; die "precheck rc=$rc, or its last line is not 'hard-failures=0 notes=0' (landing-precheck.log: a NOTE is read, not waved through)"; }

stamp "STEP 2b: the roster guard, PowerShell 7 and Windows PowerShell 5.1 (pwsh starts under the pinned-go PATH, DOTNET_ROOT unset: Q6)"
( cd "$W" && env -u DOTNET_ROOT PATH="$GOPIN_PATH" "$PWSH" -NoProfile -File src/check-roster-format.ps1 ) > "$L/landing-roster.log" 2>&1; r7=$?
( cd "$W" && env -u DOTNET_ROOT PATH="$GOPIN_PATH" powershell -NoProfile -ExecutionPolicy Bypass -File src/check-roster-format.ps1 ) > "$L/landing-roster51.log" 2>&1; r5=$?
for f in landing-roster.log landing-roster51.log; do tr -d '\r' < "$L/$f" | grep -a 'roster format guard:' | tail -n 1 | cut -c1-220; done
[ "$r7" = 0 ] && [ "$r5" = 0 ] || die "roster guard rc pwsh=$r7 powershell=$r5 (150 from pwsh = it did not START: read landing-roster.log's first lines)"
for f in landing-roster.log landing-roster51.log; do tr -d '\r' < "$L/$f" | grep -a 'roster format guard:' | grep -q ' 0 with an execution config' || die "$f: no 'roster format guard:' line reading 0 rows with an execution config"; done

stamp "STEP 3: the bank step, stated from the battery: $(grep -a -E 'ROSTERROW [^ ]+:' "$RUN/battery.console.log" | sed 's/^[0-9:]* *//' | tr '\n' ';' | cut -c1-400)"
nmoved=$(grep -a -E 'ROSTERROW [^ ]+:' "$RUN/battery.console.log" | grep -vc 'AT BANKED COUNTS')
stamp "        rows NOT at their banked counts: $nmoved (0 = nothing to bank; any other number is COORD's to bank by hand before the refresh commit, with the row's own record)"

stamp "STEP 4a: the tree back at HEAD (tracked changes now: $(tcn "$W"); they are the T legs' material, kept as T-rewrites.patch)"
[ "$(git -C "$W" status --porcelain --untracked-files=no | grep -c '^ D')" = 0 ] || die "tracked deletions in the worktree (floor 8)"
git -C "$W" checkout -q -- . || die "restore failed"
[ "$(tcn "$W")" = 0 ] || die "the tree is not clean after the restore ($(tcn "$W"))"

stamp "STEP 4b: testsrc-refresh (inputs: the i7's S and T patches and the i9's patch, all windows emission at ${HEADFULL:0:10})"
RW=$(cygpath -w "$L")
hp testsrc-refresh "$WB" --out "$RW\\testsrc-refresh.patch" "$RW\\S-rewrites.patch" "$RW\\T-rewrites.patch" "$(cygpath -w "$I9P")" > "$L/testsrc-refresh.log" 2>&1; rc=$?
grep -a 'REFRESH-VERDICT' "$L/testsrc-refresh.log" | cut -c1-300
grep -a 'REFRESH EXCLUDED' "$L/testsrc-refresh.log" | cut -c1-220
grep -a -E 'REFRESH (REFUSED|CONFLICT)' "$L/testsrc-refresh.log" | head -n 8 | cut -c1-220
[ "$rc" = 0 ] && grep -a 'REFRESH-VERDICT' "$L/testsrc-refresh.log" | grep -q 'refused=0 conflicts=0' || die "testsrc-refresh rc=$rc, or refused / conflicts is not 0 (a CONFLICT is two windows re-emissions of one file that disagree: root it, never pick one)"

stamp "STEP 4c: git apply --check, then git apply"
git -C "$W" apply --check "$L/testsrc-refresh.patch" || die "git apply --check refused the refresh patch"
git -C "$W" apply "$L/testsrc-refresh.patch" || die "git apply failed"

stamp "STEP 4d: --check-worktree"
hp testsrc-refresh "$WB" --check-worktree > "$L/testsrc-refresh-check.log" 2>&1; rc=$?
grep -a 'REFRESH-WT-VERDICT' "$L/testsrc-refresh-check.log" | cut -c1-300
[ "$rc" = 0 ] && grep -a 'REFRESH-WT-VERDICT' "$L/testsrc-refresh-check.log" | grep -q 'ONLY committed test sources changed' || die "--check-worktree rc=$rc"

stamp "STEP 4e: the class census of what is about to be committed (Q18)"
git -C "$W" diff -U0 > "$L/testsrc-refresh-applied-U0.patch"
hp teattr "$RW\\testsrc-refresh-applied-U0.patch" > "$L/testsrc-refresh-class.log" 2>&1
grep -a '^TE files=' "$L/testsrc-refresh-class.log" | cut -c1-330
pred=$(git -C "$W" grep -hE '^[[:space:]]*(\[[A-Za-z]+\] )*\[MethodImpl\(MethodImplOptions\.NoInlining\)\] (\[[A-Za-z]+\] )*(public|internal|private) ' HEAD -- 'src/core/*_test.cs' | grep -vcE '\[GoInit\]| init\(')
left=$(git -C "$W" grep -hE '^[[:space:]]*(\[[A-Za-z]+\] )*\[MethodImpl\(MethodImplOptions\.NoInlining\)\] (\[[A-Za-z]+\] )*(public|internal|private) ' -- 'src/core/*_test.cs' | grep -vcE '\[GoInit\]| init\(')
stamp "        no-inline method declarations still in the attribute form: $pred at HEAD, $left in the refreshed worktree (EXPECT the difference = the TE line's partial-lines; what is left belongs to rows no windows box swept, or to sources -tests does not emit on windows: name them in the refresh message)"
# T3 (T3-4): FL1's face-lift census of the refresh is dropped (the face lift landed with FL's refresh 0574b8336c).
stamp "        OTHER hunks: READ each one in testsrc-refresh-applied-U0.patch before signing (teattr lists none by name without a baseline: run 'python -B tT3-helpers.py teattr --baseline <P's refresh patch> <this patch>' for the list)"

stamp "PREP DONE at head ${HEADFULL:0:10}, from the battery of record $RUN ($BVER; its PRE and END lines name this head). Worktree: $(tcn "$W") tracked file(s) changed, deletions $(git -C "$W" status --porcelain --untracked-files=no | grep -c '^ D'). NOT committed, NOT pushed. NEXT: COORD signs 'refresh: TRAIN T3 -- ...' (single parent, on ${HEADFULL:0:10}), reads the signature as its own command, then runs tT3-land-final-reads.sh at THAT head with the SAME RUN"
git -C "$W" status --porcelain --untracked-files=no | cut -c1-120 | head -n 40
