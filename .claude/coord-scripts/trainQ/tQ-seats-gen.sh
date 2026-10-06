#!/bin/bash
# TRAIN Q seat table, v0, generated from COORD's accepted list (ledger + notes). Read-only git object reads.
#   usage: tQ-seats-gen.sh > tQ-seats-draft-v0.txt
# Row = ref|seated sha|notes. Row order = merge order. A stack-on token names an EARLIER row that is an ancestor.
set -u
R=/h/Projects/go2cs
BASE=7098b8d3f958e68dfcd23a35d5030427b2638357
# lane:shortsha[:status]   (status: empty = accepted; CAND = candidate, a reading is out)
ROWS="
C2:476fbd381c C2:d6a2a9d04d C2:f378bd2251 C2:426f4ea780 C2:45082709bc C2:50f3b034aa C2:3c38c44e25
G:1643862c92 C2:b6cdc9dd7e G:f4ca0f236c C2:a51a2cf441 C2:0bada15b86 C2:4334638651 C2:aa09b35a4c
C2:8d4ab2d7fd C2:60faaf0b24 C2:9dee75539e C2:c585fa199d C2:3196a93cdc C2:5aab7c91f2 C2:a8db3c7e98
G:2598dad4f3 G:f6c182c23f G:7372953bd3 G:0a40cc595c G:40a76d780b G:e981a26260 G:82c3b347dc G:5254ba28eb
G:0ee5681fe0 G:015e3c279e G:24bb3cf68b G:52a5d2e707 G:03b142b713 G:0c6a2618fa G:8dde211641 G:d83c31465b
G:ca436147c0 G:e6b30193a3 G:be4ce10078 G:6481ddbabb G:e13e0e2ecc G:2b3aeb0630
i9:7d750ba428 i9:330dc41313 i9:631100d823 i9:fc921c871f i9:1f6e822670 i9:61cebb3ad5 i9:1804f36748
i9:a6ba14579a i9:c0c876fb27
C1:fd2b421bba C1:dae4ce596f C1:ab6aa8f443:CAND C1:20d7ac347e C1:3b093f935a
P1:d0a8d82373
C2:e7fcff2244 G:633b045e8a:CAND
"
git -C "$R" cat-file -e "$BASE^{commit}" || { echo "REFUSED: base missing" >&2; exit 2; }
echo "# TRAIN Q seat table v0 -- generated $(date '+%Y-%m-%d %H:%M') from COORD's accepted list; base master $BASE"
echo "# row = ref|seated sha|notes. Row order = merge order. PENDING rows are comments at the end."
n=0; seen=""
for row in $ROWS; do
    lane="${row%%:*}"; rest="${row#*:}"; short="${rest%%:*}"; status=""
    [ "$rest" != "$short" ] && status="${rest#*:}"
    full=$(git -C "$R" rev-parse --verify -q "$short^{commit}") || { echo "REFUSED: $short does not resolve" >&2; exit 2; }
    ref=$(git -C "$R" for-each-ref --points-at "$full" --format='%(refname:short)' refs/remotes/origin | sed 's#^origin/##' | grep -v -E -- '-probe|-control' | head -n 1)
    [ -n "$ref" ] || ref="<NO-REMOTE-REF-AT-$short>"
    mb=$(git -C "$R" merge-base "$BASE" "$full")
    commits=$(git -C "$R" rev-list --count "$BASE..$full")
    files=$(git -C "$R" diff --name-only "$mb" "$full" | wc -l)
    stack=""
    for prev in $seen; do
        git -C "$R" merge-base --is-ancestor "$prev" "$full" && stack="$stack stack-on=$(git -C "$R" rev-parse --short=10 "$prev")"
    done
    onbase="cut=$(echo "$mb" | cut -c1-10)"; [ "$mb" = "$BASE" ] && onbase="cut=BASE"
    n=$((n + 1))
    echo "$ref|$(echo "$full" | cut -c1-10)|$lane; $onbase; commits-over-base=$commits; files=$files;${stack:- no-stack}${status:+; $status}"
    seen="$seen $full"
done
echo "# rows: $n"
echo "# PENDING: C2 claude/c2-noinline-partial-on-nameof (the carrier + merge of G ca436147c0 + StatementTableFuncNames re-baseline): directly AFTER c2-noinline-partial, BEFORE g-r2m-caller-closure-v2."
echo "# CAND rows: c1-golib-trim-default (hosted probe 37495989079 out); g-r2m-caller-closure-v2 (logrus acceptance read owed by G)."
echo "# SUPERSEDED, NOT ROWS: i9 850ff04577 (by 1f6e822670); C1 9ff16da875 (carried by dae4ce596f); G 48808637c9; G 0912a84755 (record)."
