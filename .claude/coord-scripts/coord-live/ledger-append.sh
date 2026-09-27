#!/usr/bin/env bash
# usage: ledger-append.sh <entry-file-with-STAMP_PLACEHOLDER> "<commit subject tail>"
# One line, stamped from the clock, refused if equal to the last line, censused, signed, pushed.
set -u
S="${COORD_SCRATCH:?set COORD_SCRATCH to the COORD scratch directory}"
ENTRY="$1"; SUBJ="$2"
MB=/h/Projects/go2cs-mailbox
cd "$MB" || exit 1
git pull -q --ff-only origin claude/mailbox || { echo PULL-FAILED; exit 1; }
ST=$(date '+%Y-%m-%d %H:%M')
sed "s/^STAMP_PLACEHOLDER/$ST/" "$ENTRY" > "$S/ledger-entry-stamped.txt"
[ "$(wc -l < "$S/ledger-entry-stamped.txt")" -eq 1 ] || { echo NOT-ONE-LINE; exit 1; }
grep -q '^STAMP_PLACEHOLDER' "$S/ledger-entry-stamped.txt" && { echo UNSTAMPED; exit 1; }
LAST=$(grep -v '^[[:space:]]*$' docs/phase4/LEDGER.md | tail -1 | tr -d '\r')
NEW=$(tr -d '\r' < "$S/ledger-entry-stamped.txt")
[ "$LAST" = "$NEW" ] && { echo DUP-OF-LAST; exit 1; }
( cd /h/Projects/go2cs && bash .claude/coord-scripts/coord-identifier-census.sh entry "$S/ledger-entry-stamped.txt" > "$S/census-ref.log" 2>&1 ) || { echo CENSUS-FIRED; tail -5 "$S/census-ref.log"; exit 1; }
# the file must end on a newline before the append, and no blank line is added
[ -n "$(tail -c 1 docs/phase4/LEDGER.md)" ] && printf '\n' >> docs/phase4/LEDGER.md
cat "$S/ledger-entry-stamped.txt" >> docs/phase4/LEDGER.md
git add docs/phase4/LEDGER.md
git commit -S -q -m "ledger: $ST -- $SUBJ" -m "Co-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>" || { echo COMMIT-FAILED; exit 1; }
git push -q origin HEAD:claude/mailbox || { git pull -q --rebase origin claude/mailbox && git push -q origin HEAD:claude/mailbox; } || { echo PUSH-FAILED; exit 1; }
tail -2 docs/phase4/LEDGER.md | cut -c1-60
git log --oneline -1
