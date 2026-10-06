#!/usr/bin/env bash
# R0 ONLY, NOT ADAPTED TO TRAIN Q (review round 1): this is trainP's pre-map reading tool under Q's name. It reads no ruled resolution (on Q's table every ruled row reads as a conflict), it needs git 2.44 or later through GITX, its comments below describe TRAIN P's pre-map, and it is on NO step of COORD-LAUNCH-CHECKLIST.md: the map of record is tQ-conflict-map.sh. It writes its merge objects into the SHARED object store unless GIT_OBJECT_DIRECTORY names a scratch one: set it.
# tQ-hunks.sh <ours> <theirs> [path...] -- print the conflict regions ort writes for one merge, diff3 style
# (DRAFT 2026-10-04, UNCOMMITTED). No worktree. With no path, every conflicted path. Context: 3 lines around each
# region. Objects go to a scratch directory when GIT_OBJECT_DIRECTORY is set by the caller (tQ-premap-all.sh's
# readers: export GIT_OBJECT_DIRECTORY=<OUT>/objects and GIT_ALTERNATE_OBJECT_DIRECTORIES=H:/Projects/go2cs/.git/objects),
# otherwise to the shared store.
set -u
export MSYS_NO_PATHCONV=1
GD=${GD:-H:/Projects/go2cs/.git}
GITX=${GITX:-/c/Program Files/Microsoft Visual Studio/2022/Community/Common7/IDE/CommonExtensions/Microsoft/TeamFoundation/Team Explorer/Git/cmd/git.exe}
g() { "$GITX" --git-dir="$GD" -c core.quotepath=false "$@"; }
o=$1; t=$2; shift 2
out=$(g -c merge.conflictStyle=diff3 merge-tree --write-tree --name-only "$o" "$t"); rc=$?
tree=$(printf '%s\n' "$out" | head -1)
echo "MERGE $(g rev-parse --short=10 "$o") x $(g rev-parse --short=10 "$t") rc=$rc tree=${tree:0:12} bases=$(g merge-base --all "$o" "$t" | cut -c1-10 | tr '\n' ' ')"
if [ $# -gt 0 ]; then paths=("$@"); else mapfile -t paths < <(printf '%s\n' "$out" | sed -n '2,/^$/p' | sed '/^$/d'); fi
for p in "${paths[@]}"; do
  echo "=== $p"
  g show "$tree:$p" | tr -d '\r' | awk '
    { line[NR]=$0 }
    /^<<<<<<< /{ s[++n]=NR } /^>>>>>>> /{ e[n]=NR }
    END { for (i=1;i<=n;i++) { a=s[i]-3; if (a<1) a=1; b=e[i]+3; if (b>NR) b=NR; printf("--- region %d: lines %d-%d of the marked file\n", i, s[i], e[i]); for (k=a;k<=b;k++) printf("%6d  %s\n", k, substr(line[k],1,400)) } }'
done
