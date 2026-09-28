#!/usr/bin/env bash
# =================================================================================================
# fleet-msg-arms.sh -- the arms for fleet-msg.sh and fleet-read.sh. PROTOCOL v4.
#
# Runs entirely against a THROWAWAY BARE REMOTE and throwaway clones created under a temp directory
# of its own. It never names, fetches from or pushes to the real claude/mailbox: the only thing it
# takes from the real clone is a COPY of the census files (read out of origin/master) and a COPY of
# the inbox skeleton, and arm (f) proves it left the real clone's inbox and ledger byte-identical.
#
# A gate that has never been made to fail proves nothing, so (b) plants a token that must refuse and
# (d) is run BOTH ways -- over the budget without the override, and the same file with it.
#
#   a  a message lands at the right path with the right header
#   b  a planted identifier -> REFUSED, and nothing is written, staged, committed or pushed
#   c  two clones push back to back -> both present, the second retried after one rejection
#   d  a 61-line body -> refused; the same file with FLEET_LONG=1 -> accepted
#   e  fleet-read.sh reads the range after a POSITION cursor, inbox/FLEET/ too, NEXT-SINCE = the tip
#   f  the real clone's inbox + ledger fingerprint, byte-equal before and after
#   g  a ledger line stamped in 2001 but COMMITTED after the cursor is printed and counted
#   h  an inbox file whose NAME sorts before consumed ones is printed when its COMMIT is in range
#   i  NEXT-SINCE is the tip SHA read, and a legacy cursor in the future is repaired out loud
#
# (g), (h) and (i) are each run TWICE: first on the PRE-FIX fleet-read.sh -- the real one, taken by
# its blob id, not a paraphrase of it -- where each must fail, and then on the tool beside this
# harness, where each must pass. A red half that does not reproduce fails the arm rather than
# passing it quietly, because an arm whose defect has gone missing measures nothing.
#
# Exits non-zero if any arm fails. Env: FLEET_ARMS_TMP overrides the temp root.
# =================================================================================================
set -u

HERE="$(cd -- "$(dirname -- "$0")" && pwd)"
MSG="$HERE/fleet-msg.sh"
RDR="$HERE/fleet-read.sh"
FAILED=0
ok()   { echo "ARM $1: PASS -- $2"; }
bad()  { echo "ARM $1: FAIL -- $2"; FAILED=$((FAILED + 1)); }
setupfail() { echo "SETUP FAIL: $*" >&2; exit 2; }
# See fleet-msg.sh: the shell's cd and git's no-path-conversion cannot ride one command line.
gshow() { ( cd -- "$1" && MSYS_NO_PATHCONV=1 git show "$2" ); }

[ -f "$MSG" ] || setupfail "fleet-msg.sh is not beside this harness"
[ -f "$RDR" ] || setupfail "fleet-read.sh is not beside this harness"
REAL="$(git -C "$HERE" rev-parse --show-toplevel 2>/dev/null)"; rc=$?
{ [ "$rc" -eq 0 ] && [ -n "$REAL" ]; } || setupfail "this harness is not inside a clone"

TD="$(mktemp -d "${FLEET_ARMS_TMP:-${TMPDIR:-/tmp}}/fleet-arms.XXXXXX" 2>/dev/null)"
{ [ -n "$TD" ] && [ -d "$TD" ]; } || setupfail "could not create a throwaway directory"
trap 'if [ -n "${TD:-}" ] && [ -d "$TD" ] && [ "${FLEET_ARMS_KEEP:-0}" != "1" ]; then rm -rf -- "$TD"; fi' EXIT INT TERM
echo "throwaway root: $TD   (FLEET_ARMS_KEEP=1 keeps it and its per-arm output files)"

# ---- arm (f), first half: the real clone's fingerprint BEFORE anything runs ------------------------
fingerprint() {
    local root="$1" p="" h=""
    ( cd -- "$root" 2>/dev/null || return 1
      find docs/phase4/inbox -type f 2>/dev/null | sort | while IFS= read -r p; do
          h="$(sha256sum < "$p")"; printf '%s\t%s\n' "$p" "${h%% *}"
      done
      if [ -f docs/phase4/LEDGER.md ]; then
          h="$(sha256sum < docs/phase4/LEDGER.md)"; printf '%s\t%s\n' "docs/phase4/LEDGER.md" "${h%% *}"
      fi )
}
FP_BEFORE="$(fingerprint "$REAL")"
[ -n "$FP_BEFORE" ] || setupfail "the real clone's inbox fingerprint came back empty -- arm f would be vacuous"

# ---- the throwaway remote -------------------------------------------------------------------------
BARE="$TD/remote.git"
git init --quiet --bare "$BARE" || setupfail "could not create the throwaway bare remote"
SEED="$TD/seed"
git init --quiet "$SEED" || setupfail "could not create the seed clone"
idpin() { git -C "$1" config user.email "arms@placeholder.invalid"; git -C "$1" config user.name "fleet arms"; git -C "$1" config commit.gpgsign false; }
idpin "$SEED"
git -C "$SEED" checkout -q -B master

mkdir -p "$SEED/.claude/coord-scripts"
for f in coord-identifier-census.sh coord-identifier-patterns.txt coord-identifier-hashes.txt; do
    gshow "$REAL" "origin/master:.claude/coord-scripts/$f" > "$SEED/.claude/coord-scripts/$f" 2>/dev/null; rc=$?
    sz="$(wc -c < "$SEED/.claude/coord-scripts/$f")"; sz="${sz// /}"
    [ "${sz:-0}" -gt 1000 ] || setupfail "the copy of $f is $sz bytes -- the arms would run against an empty census"
    [ "$rc" -eq 0 ] || setupfail "could not copy $f out of the real clone's origin/master (rc=$rc)"
done
git -C "$SEED" add -- .claude/coord-scripts && git -C "$SEED" commit -q -m "arms: the census, copied" || setupfail "could not commit the census copy"
git -C "$SEED" push -q "$BARE" master || setupfail "could not push the throwaway master"

git -C "$SEED" checkout -q -B claude/mailbox master
for L in COORD C1 C2 R G i9 P1 P2 FLEET; do
    mkdir -p "$SEED/docs/phase4/inbox/$L"
    cp -- "$REAL/docs/phase4/inbox/$L/README.md" "$SEED/docs/phase4/inbox/$L/README.md" || setupfail "the real clone has no inbox/$L/README.md"
done
cp -- "$REAL/docs/phase4/LEDGER.md" "$SEED/docs/phase4/LEDGER.md" || setupfail "the real clone has no LEDGER.md"
# The line arm (e) looks for in an unfiltered read is DERIVED from the ledger it seeded, never a
# sentence typed in here: an unfiltered read prints the last 20 lines, so any line named by hand
# drops out of that window the week the ledger grows past it and reds an arm that is working.
LAST_LEDGER="$(grep -E '^[0-9]{4}-[0-9]{2}-[0-9]{2} [0-9]{2}:[0-9]{2} ' "$SEED/docs/phase4/LEDGER.md" | tail -1)"
[ -n "$LAST_LEDGER" ] || setupfail "the seeded LEDGER.md has no dated line -- arm (e)'s ledger half would be vacuous"
git -C "$SEED" add -- docs/phase4 && git -C "$SEED" commit -q -m "arms: the inbox skeleton, copied" || setupfail "could not commit the skeleton"
git -C "$SEED" push -q "$BARE" claude/mailbox || setupfail "could not push the throwaway claude/mailbox"

SEEDTIP="$(git -C "$BARE" rev-parse claude/mailbox)"
TDR="$(cd -- "$TD" && pwd)"
# TWO INDEPENDENT CHECKS that a clone is the throwaway one, because either alone can be fooled.
# The path check canonicalises BOTH sides through `cd`+`pwd`: git prints a remote URL in the host's
# own spelling (C:/... under MSYS) while $TD is in the shell's (/tmp/...), and a raw prefix match on
# the two spellings refuses every clone -- the first run of this harness did exactly that. The
# identity check does not involve a spelling at all: the clone's origin/claude/mailbox must be the
# tip this harness just built, which no real clone can be.
mkclone() {
    git clone -q "$BARE" "$TD/$1" || setupfail "could not clone $1"
    git -C "$TD/$1" checkout -q claude/mailbox || setupfail "$1 has no claude/mailbox"
    idpin "$TD/$1"
    u="$(git -C "$TD/$1" remote get-url origin)"
    ur="$(cd -- "$u" 2>/dev/null && pwd)"
    case "${ur:-/}" in "$TDR"/*) ;; *) setupfail "$1's origin does not resolve under the throwaway root -- refusing to run the arms" ;; esac
    git -C "$TD/$1" merge-base --is-ancestor "$SEEDTIP" origin/claude/mailbox 2>/dev/null; rc=$?
    [ "$rc" -eq 0 ] || setupfail "$1's origin/claude/mailbox does not descend from the tip this harness built -- refusing to run the arms"
}
mkclone A
BARETIP() { git -C "$BARE" rev-parse claude/mailbox; }

send() { # send <clone> <from> <to> <subject> <bodyfile> <outfile>  -- returns fleet-msg.sh's rc
    FLEET_LANE="$2" FLEET_MAILBOX_CLONE="$TD/$1" bash "$MSG" "$3" "$4" "$5" > "$6" 2>&1
}
field() { awk -v k="$1" '$1==k{print $2}' "$2"; }
has()  { grep -q  -- "$1" "$2"; }
hasF() { grep -Fq -- "$1" "$2"; }

# ---- the PRE-FIX reader, for the red half of (g) (h) and (i) ---------------------------------------
# fleet-read.sh exactly as it stood at ce2641b18d, the tip the time-cursor defect was measured at,
# named by its BLOB ID. A blob id is stable whatever commit later touches the path and whatever line
# endings a checkout uses, where "the commit before the fix" stops being the pre-fix version the
# moment anything else edits the file, and a re-implementation of the old logic would only ever
# prove itself. If the blob is not in this clone the three arms FAIL and say so; they never skip.
PREBLOB="67b24aceb917c7e3795b599e8209e1a4148d262c"
PRE="$TD/fleet-read-prefix.sh"
gcat() { ( cd -- "$1" && MSYS_NO_PATHCONV=1 git cat-file blob "$2" ); }
gcat "$REAL" "$PREBLOB" > "$PRE" 2>/dev/null; prc=$?
PRE_OK=1; { [ "$prc" -eq 0 ] && [ -s "$PRE" ]; } || PRE_OK=0

readnew() { FLEET_MAILBOX_CLONE="$TD/A" bash "$RDR" "$1" ${2:+"$2"} > "$3" 2>&1; }
readold() { FLEET_MAILBOX_CLONE="$TD/A" bash "$PRE" "$1" ${2:+"$2"} > "$3" 2>&1; }
utc()     { date -u +%Y%m%dT%H%M%SZ; }
# plant <path relative to the clone> <append|write> <line> -- ONE commit at the throwaway remote.
# Written with git rather than fleet-msg.sh on purpose: the point of (g) and (h) is a name and a
# stamp that fleet-msg.sh's own clock would never produce.
plant() {
    local rel="$1" mode="$2" line="$3"
    git -C "$TD/A" fetch -q origin claude/mailbox || return 1
    git -C "$TD/A" reset -q --hard FETCH_HEAD || return 1
    mkdir -p -- "$(dirname -- "$TD/A/$rel")" || return 1
    if [ "$mode" = "append" ]; then printf '%s\n' "$line" >> "$TD/A/$rel"; else printf '%s\n' "$line" > "$TD/A/$rel"; fi
    git -C "$TD/A" add -- "$rel" || return 1
    git -C "$TD/A" commit -q -m "arms: a planted line" || return 1
    git -C "$TD/A" push -q origin claude/mailbox || return 1
}

# ---- (a) ------------------------------------------------------------------------------------------
printf 'One line WHAT.\nOne line of evidence.\nNEXT: nothing.\n' > "$TD/body-a.md"
send A R C1 "arm a -- a message lands" "$TD/body-a.md" "$TD/out-a"; rc_a=$?
FILE_A="$(field FILE "$TD/out-a")"
if [ "$rc_a" -ne 0 ]; then bad a "fleet-msg.sh exited $rc_a; see $TD/out-a"
else
    gshow "$BARE" "claude/mailbox:$FILE_A" > "$TD/blob-a" 2>/dev/null; brc=$?
    h1="$(sed -n '1p' "$TD/blob-a")"; h2="$(sed -n '2p' "$TD/blob-a")"
    case "$FILE_A" in docs/phase4/inbox/C1/*-R.md) pathok=1 ;; *) pathok=0 ;; esac
    if   [ "$brc" -ne 0 ];                          then bad a "the file is not at the remote (rc=$brc)"
    elif [ "$pathok" -ne 1 ];                       then bad a "wrong path: $FILE_A"
    elif [ "$h1" != "# arm a -- a message lands" ]; then bad a "header line 1 is '$h1'"
    else case "$h2" in "from: R · to: C1 · "*) ok a "$FILE_A, both header lines as specified" ;;
                       *) bad a "header line 2 is '$h2'" ;; esac
    fi
fi
# The cut arm (e) reads from is a POSITION -- the tip that holds arm (a)'s message -- and not the
# stamp on its name: the stamp is a second or so before the commit that carries it, so a cursor
# taken from the name can land either side of that commit and the arm would flap.
CUT_A="$(BARETIP)"
SINCE_A_LEGACY="$(utc)"

# ---- (b) the planted token. Assembled from pieces so this harness itself stays census-clean. -------
seg="Users"; acct="qwertyname"; sep="/"
printf 'One line WHAT.\nthe path was C:%s%s%s%s%snotes.txt\nNEXT: nothing.\n' "$sep" "$seg" "$sep" "$acct" "$sep" > "$TD/body-b.md"
tip0="$(BARETIP)"; head0="$(git -C "$TD/A" rev-parse HEAD)"
send A R C1 "arm b -- a planted identifier" "$TD/body-b.md" "$TD/out-b"; rc_b=$?
tip1="$(BARETIP)"; head1="$(git -C "$TD/A" rev-parse HEAD)"
idx="$(git -C "$TD/A" diff --cached --name-only)"; wt="$(git -C "$TD/A" status --porcelain)"
# rc must be THREE -- the census's own refusal -- and not merely non-zero. The first run of this
# harness passed arm (b) on rc=2, which was fleet-msg.sh dying because it could not LOAD the census
# at all: a refusal for the wrong reason reads exactly like the arm working.
if   [ "$rc_b" -eq 0 ];        then bad b "fleet-msg.sh accepted a planted identifier"
elif [ "$rc_b" -ne 3 ];        then bad b "refused with rc=$rc_b, which is not the census's rc=3 -- the refusal is not the one under test"
elif [ "$tip1" != "$tip0" ];   then bad b "the remote tip moved on a refusal"
elif [ "$head1" != "$head0" ]; then bad b "the clone committed on a refusal"
elif [ -n "$idx" ];            then bad b "the index was left holding '$idx'"
elif [ -n "$wt" ];             then bad b "the worktree was left dirty: '$wt'"
else ok b "refused (rc=$rc_b), remote tip, HEAD, index and worktree all unchanged"
fi

# ---- (c) two clones, back to back ------------------------------------------------------------------
# Both cloned HERE, at one tip, so the first push is a fast-forward and the second cannot be: if the
# first already needed a retry the second's retry would not be the thing measured, which is why the
# arm asserts attempts=1 on the first as well as >1 on the second.
mkclone P; mkclone Q
printf 'One line WHAT.\nNEXT: nothing.\n' > "$TD/body-c.md"
send P G C2 "arm c -- first of two" "$TD/body-c.md" "$TD/out-c1"; rc_c1=$?
sleep 1
send Q i9 C2 "arm c -- second of two" "$TD/body-c.md" "$TD/out-c2"; rc_c2=$?
F_C1="$(field FILE "$TD/out-c1")"; F_C2="$(field FILE "$TD/out-c2")"
N_C1="$(field ATTEMPTS "$TD/out-c1")"; N_C2="$(field ATTEMPTS "$TD/out-c2")"
git -C "$BARE" show "claude/mailbox:$F_C1" > /dev/null 2>&1; p1=$?
git -C "$BARE" show "claude/mailbox:$F_C2" > /dev/null 2>&1; p2=$?
if   [ "$rc_c1" -ne 0 ] || [ "$rc_c2" -ne 0 ]; then bad c "exits $rc_c1 / $rc_c2; see $TD/out-c1 and $TD/out-c2"
elif [ "$p1" -ne 0 ] || [ "$p2" -ne 0 ];       then bad c "one of the two is missing at the remote ($p1/$p2)"
elif [ "${N_C1:-0}" -ne 1 ];                   then bad c "the first push took $N_C1 attempts, so the second's retry is not the thing measured"
elif [ "${N_C2:-0}" -lt 2 ];                   then bad c "the second push took $N_C2 attempts -- it was never rejected, so the retry is untested"
else ok c "both present, no conflict; first attempts=$N_C1, second attempts=$N_C2 (rejected then rebased)"
fi

# ---- (d) the budget, both ways ---------------------------------------------------------------------
i=1; : > "$TD/body-d.md"
while [ "$i" -le 61 ]; do printf 'line %s\n' "$i" >> "$TD/body-d.md"; i=$((i + 1)); done
send A R C2 "arm d -- 61 lines" "$TD/body-d.md" "$TD/out-d1"; rc_d1=$?
FLEET_LONG=1 FLEET_LANE=R FLEET_MAILBOX_CLONE="$TD/A" bash "$MSG" C2 "arm d -- 61 lines, declared" "$TD/body-d.md" > "$TD/out-d2" 2>&1; rc_d2=$?
if   [ "$rc_d1" -eq 0 ]; then bad d "a 61-line body went out without FLEET_LONG"
elif [ "$rc_d2" -ne 0 ]; then bad d "the same file was refused WITH FLEET_LONG=1 (rc=$rc_d2) -- the gate is not the line count"
else ok d "61 lines refused (rc=$rc_d1); the same file with FLEET_LONG=1 accepted"
fi

# ---- (e) the reader ---------------------------------------------------------------------------------
sleep 1
printf 'One line WHAT.\nNEXT: nothing.\n' > "$TD/body-e.md"
send A G C1 "arm e -- after the cut" "$TD/body-e.md" "$TD/out-e1"; rc_e1=$?
send A COORD FLEET "arm e -- a ruling to everyone" "$TD/body-e.md" "$TD/out-e2"; rc_e2=$?
F_E1="$(field FILE "$TD/out-e1")"; F_E2="$(field FILE "$TD/out-e2")"
TIP_E="$(BARETIP)"
readnew C1 "$CUT_A" "$TD/out-read-since"; rc_r1=$?
readnew C1 ""       "$TD/out-read-all";   rc_r2=$?
readnew C1 "$SINCE_A_LEGACY" "$TD/out-read-legacy"; rc_r3=$?
NS="$(field NEXT-SINCE "$TD/out-read-since")"
NSL="$(field NEXT-SINCE "$TD/out-read-legacy")"
M_E="$(field MESSAGES "$TD/out-read-since")"
if   [ "$rc_e1" -ne 0 ] || [ "$rc_e2" -ne 0 ]; then bad e "could not seed the reader (rc $rc_e1 / $rc_e2)"
elif [ "$rc_r1" -ne 0 ] || [ "$rc_r2" -ne 0 ] || [ "$rc_r3" -ne 0 ]; then bad e "fleet-read.sh exited $rc_r1 / $rc_r2 / $rc_r3"
elif has "$FILE_A" "$TD/out-read-since";       then bad e "the range read still listed the file AT the cut"
elif ! has "$F_E1" "$TD/out-read-since";       then bad e "the range read missed the later message to C1"
elif ! has "$F_E2" "$TD/out-read-since";       then bad e "the range read missed inbox/FLEET/"
elif [ "${M_E:-0}" -ne 2 ];                    then bad e "MESSAGES $M_E -- the range after the cut holds exactly the two later files"
elif ! has "$FILE_A" "$TD/out-read-all";       then bad e "the unfiltered read missed the file at the cut"
elif ! hasF "$LAST_LEDGER" "$TD/out-read-all"; then bad e "the unfiltered read did not print the ledger's newest line"
elif [ -z "$NS" ];                             then bad e "no NEXT-SINCE was printed"
elif [ "$NS" != "$TIP_E" ];                    then bad e "NEXT-SINCE=$NS is not the tip it read ($TIP_E)"
elif ! has "$F_E1" "$TD/out-read-legacy";      then bad e "the legacy time cursor $SINCE_A_LEGACY did not resolve to a position -- the later message is missing"
elif [ "$NSL" != "$TIP_E" ];                   then bad e "the legacy read's NEXT-SINCE=$NSL is not the tip ($TIP_E)"
else ok e "the range after $CUT_A held exactly $M_E files (the one at the cut excluded, inbox/FLEET/ included), the unfiltered read printed the ledger tail, NEXT-SINCE=$NS, and the legacy time form resolved to the same tip"
fi

# ---- (g) a ledger line STAMPED in 2001 and COMMITTED after the cursor --------------------------------
# The pre-fix half is asserted as "the planted line is absent", not as "LEDGER 0": the old tool also
# hides every OTHER line whose stamp is behind the reader's UTC clock, so the count it prints depends
# on the box's timezone while the planted line's invisibility does not. Both counts are reported.
CUT_G="$(BARETIP)"; T_G="$(utc)"
G_LINE='2001-01-01 00:00 · RULING · 000000g00 · arm g -- stamped in 2001, committed after the cut'
plant docs/phase4/LEDGER.md append "$G_LINE"; rc_gp=$?
readnew C1 "$CUT_G" "$TD/out-g-new"; rc_gn=$?
readold C1 "$T_G"   "$TD/out-g-old"; rc_go=$?
L_GN="$(field LEDGER "$TD/out-g-new")"; L_GO="$(field LEDGER "$TD/out-g-old")"
if   [ "$rc_gp" -ne 0 ];   then bad g "could not plant the back-stamped ledger line (rc=$rc_gp)"
elif [ "$PRE_OK" -ne 1 ];  then bad g "the pre-fix reader (blob $PREBLOB) is not in this clone -- the red half cannot be shown"
elif [ "$rc_go" -ne 0 ];   then bad g "the pre-fix reader exited $rc_go; see $TD/out-g-old"
elif hasF "$G_LINE" "$TD/out-g-old"; then bad g "the PRE-FIX reader printed the back-stamped line -- the defect does not reproduce, so this arm measures nothing"
elif [ "$rc_gn" -ne 0 ];   then bad g "fleet-read.sh exited $rc_gn; see $TD/out-g-new"
elif ! hasF "$G_LINE" "$TD/out-g-new"; then bad g "the fixed reader did not print the back-stamped line"
elif [ "${L_GN:-0}" -ne 1 ]; then bad g "LEDGER $L_GN -- not the one line the range $CUT_G..tip added"
else ok g "pre-fix LEDGER $L_GO, the 2001 line invisible at $T_G; fixed LEDGER $L_GN with the line printed"
fi

# ---- (h) an inbox file whose NAME sorts before the names already consumed ----------------------------
# Not written through fleet-msg.sh: its own clock can only ever produce a name that sorts LAST, which
# is exactly why the name-ordered reader looked correct for as long as one box wrote all the files.
CUT_H="$(BARETIP)"; T_H="$(utc)"
H_FILE="docs/phase4/inbox/C1/20250101T000000Z-R.md"
plant "$H_FILE" write "# arm h -- a name from January 2025, committed now"; rc_hp=$?
readnew C1 "$CUT_H" "$TD/out-h-new"; rc_hn=$?
readold C1 "$T_H"   "$TD/out-h-old"; rc_ho=$?
M_HN="$(field MESSAGES "$TD/out-h-new")"; M_HO="$(field MESSAGES "$TD/out-h-old")"
if   [ "$rc_hp" -ne 0 ];   then bad h "could not plant the early-named inbox file (rc=$rc_hp)"
elif [ "$PRE_OK" -ne 1 ];  then bad h "the pre-fix reader (blob $PREBLOB) is not in this clone -- the red half cannot be shown"
elif [ "$rc_ho" -ne 0 ];   then bad h "the pre-fix reader exited $rc_ho; see $TD/out-h-old"
elif has "$H_FILE" "$TD/out-h-old"; then bad h "the PRE-FIX reader listed the early-named file -- the defect does not reproduce, so this arm measures nothing"
elif [ "$rc_hn" -ne 0 ];   then bad h "fleet-read.sh exited $rc_hn; see $TD/out-h-new"
elif ! has "$H_FILE" "$TD/out-h-new"; then bad h "the fixed reader did not list the early-named file"
elif [ "${M_HN:-0}" -ne 1 ]; then bad h "MESSAGES $M_HN -- not the one file the range $CUT_H..tip added"
else ok h "pre-fix MESSAGES $M_HO, $H_FILE invisible at $T_H; fixed MESSAGES $M_HN with the file listed"
fi

# ---- (i) NEXT-SINCE is the tip read, and a legacy cursor in the future is repaired -------------------
CUT_I="$(BARETIP)"; T_I="$(utc)"
I_LINE='2099-01-01 00:00 · RULING · 000000i00 · arm i -- a stamp from the future'
plant docs/phase4/LEDGER.md append "$I_LINE"; rc_ip=$?
TIP_I="$(BARETIP)"
readnew C1 "$CUT_I" "$TD/out-i-new"; rc_in=$?
readold C1 "$T_I"   "$TD/out-i-old"; rc_io=$?
NS_IN="$(field NEXT-SINCE "$TD/out-i-new")"; NS_IO="$(field NEXT-SINCE "$TD/out-i-old")"
FUT="$(date -u -d "@$(( $(date -u +%s) + 86400 ))" +%Y%m%dT%H%M%SZ)"; rc_fut=$?
readnew C1 "$FUT" "$TD/out-i-fut"; rc_if=$?
readold C1 "$FUT" "$TD/out-i-futold"; rc_ifo=$?
NS_IF="$(field NEXT-SINCE "$TD/out-i-fut")"
if   [ "$rc_ip" -ne 0 ] || [ "$rc_fut" -ne 0 ]; then bad i "could not plant the 2099 line / read the clock (rc $rc_ip / $rc_fut)"
elif [ "$PRE_OK" -ne 1 ];  then bad i "the pre-fix reader (blob $PREBLOB) is not in this clone -- the red half cannot be shown"
elif [ "$rc_io" -ne 0 ] || [ "$rc_ifo" -ne 0 ]; then bad i "the pre-fix reader exited $rc_io / $rc_ifo"
elif [ "${NS_IO:0:4}" != "2099" ]; then bad i "the PRE-FIX reader's NEXT-SINCE is '$NS_IO', not the 2099 stamp it read -- the defect does not reproduce"
elif hasF "is in the future" "$TD/out-i-futold"; then bad i "the PRE-FIX reader repaired a future cursor -- the defect does not reproduce"
elif [ "$rc_in" -ne 0 ] || [ "$rc_if" -ne 0 ]; then bad i "fleet-read.sh exited $rc_in / $rc_if"
elif [ "$NS_IN" != "$TIP_I" ]; then bad i "NEXT-SINCE=$NS_IN is not the tip it read ($TIP_I)"
elif ! hasF "$I_LINE" "$TD/out-i-new"; then bad i "the 2099 line was not printed -- a stamp still decides membership"
elif ! hasF "SINCE $FUT is in the future -- repaired to" "$TD/out-i-fut"; then bad i "a cursor a day ahead was not repaired out loud"
elif [ "$NS_IF" != "$TIP_I" ]; then bad i "the repaired read's NEXT-SINCE=$NS_IF is not the tip ($TIP_I)"
elif ! hasF "$G_LINE" "$TD/out-i-fut"; then bad i "the repaired cursor did not reach arm (g)'s line, which landed minutes ago"
else ok i "pre-fix NEXT-SINCE=$NS_IO (the stamp, hours ahead) and no repair; fixed NEXT-SINCE=$NS_IN = the tip, $FUT repaired out loud and still reaching the recent range"
fi

# ---- (f) arm (f), second half ------------------------------------------------------------------------
FP_AFTER="$(fingerprint "$REAL")"
if [ "$FP_BEFORE" = "$FP_AFTER" ]; then
    n=0; while IFS= read -r _l; do n=$((n + 1)); done <<< "$FP_BEFORE"
    ok f "the real clone's inbox + ledger fingerprint is byte-equal over $n paths"
else
    bad f "the real clone's inbox or ledger CHANGED while the arms ran"
    diff <(printf '%s\n' "$FP_BEFORE") <(printf '%s\n' "$FP_AFTER") | sed -n '1,20p'
fi

echo "----"
if [ "$FAILED" -eq 0 ]; then echo "ARMS: 9/9 PASS"; exit 0; fi
echo "ARMS: $FAILED FAILED"; exit 1
