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
#   e  fleet-read.sh lists only what is after SINCE, reads inbox/FLEET/ too, prints NEXT-SINCE
#   f  the real clone's inbox + ledger fingerprint, byte-equal before and after
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
for L in COORD C1 C2 R G i9 FLEET; do
    mkdir -p "$SEED/docs/phase4/inbox/$L"
    cp -- "$REAL/docs/phase4/inbox/$L/README.md" "$SEED/docs/phase4/inbox/$L/README.md" || setupfail "the real clone has no inbox/$L/README.md"
done
cp -- "$REAL/docs/phase4/LEDGER.md" "$SEED/docs/phase4/LEDGER.md" || setupfail "the real clone has no LEDGER.md"
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
B_A="${FILE_A##*/}"; SINCE_A="${B_A%%-*}"
FLEET_MAILBOX_CLONE="$TD/A" bash "$RDR" C1 "$SINCE_A" > "$TD/out-read-since" 2>&1; rc_r1=$?
FLEET_MAILBOX_CLONE="$TD/A" bash "$RDR" C1 > "$TD/out-read-all" 2>&1; rc_r2=$?
NS="$(field NEXT-SINCE "$TD/out-read-since")"
has() { grep -q -- "$1" "$2"; }
if   [ "$rc_e1" -ne 0 ] || [ "$rc_e2" -ne 0 ]; then bad e "could not seed the reader (rc $rc_e1 / $rc_e2)"
elif [ "$rc_r1" -ne 0 ] || [ "$rc_r2" -ne 0 ]; then bad e "fleet-read.sh exited $rc_r1 / $rc_r2"
elif has "$FILE_A" "$TD/out-read-since";       then bad e "the SINCE read still listed the file AT the cut"
elif ! has "$F_E1" "$TD/out-read-since";       then bad e "the SINCE read missed the later message to C1"
elif ! has "$F_E2" "$TD/out-read-since";       then bad e "the SINCE read missed inbox/FLEET/"
elif ! has "$FILE_A" "$TD/out-read-all";       then bad e "the unfiltered read missed the file at the cut"
elif ! has 'PROTOCOL v4 addressed comms' "$TD/out-read-all"; then bad e "the unfiltered read printed no ledger line"
elif [ -z "$NS" ];                             then bad e "no NEXT-SINCE was printed"
elif [[ ! "$NS" > "$SINCE_A" ]];               then bad e "NEXT-SINCE=$NS did not advance past $SINCE_A"
else ok e "SINCE=$SINCE_A excluded the file at the cut, listed the later one and inbox/FLEET/, ledger read, NEXT-SINCE=$NS"
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
if [ "$FAILED" -eq 0 ]; then echo "ARMS: 6/6 PASS"; exit 0; fi
echo "ARMS: $FAILED FAILED"; exit 1
