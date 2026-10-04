#!/usr/bin/env bash
# tP-gofmt-parse.sh <base> <head> <scratch> -- PARSE-ONLY reading of the Go files head changes under src/go2cs
# (DRAFT 2026-10-04, UNCOMMITTED). No build and no go test: the blobs are written to a scratch folder and read with
# 'gofmt -e -l'. A parse error is a merge that cannot compile; a file gofmt lists at head and not at base is formatting
# the merge (or a union carry) moved. The blobs are the store's LF text, so 'gofmt -l' IS a signal here (it is not in a
# CRLF checkout). Controls 2026-10-04 (tP-premap.md section 0): ort's MARKED tree for a conflicted pair reads 18 errors;
# a REFINED union of the same file reads 15; the unrefined union and every clean chain head read 0.
set -u
export MSYS_NO_PATHCONV=1
GD=${GD:-H:/Projects/go2cs/.git}
GF=${GOFMT:-/h/sdk/go1.24.13/bin/gofmt.exe}
g() { git --git-dir="$GD" -c core.quotepath=false "$@"; }
b=$1; h=$2; S=$3; rm -rf "$S"; mkdir -p "$S/base" "$S/head"
[ -x "$GF" ] || { echo "GOFMT-PARSE: no gofmt at $GF (set GOFMT): NOT READ"; exit 2; }
n=0
while IFS= read -r p; do
  # REVIEW ROUND 1 (trainP/tP-CHANGES.md RR1-7): the blob keeps its path under src/go2cs. Written by BASENAME, two changed
  # files of one name (main.go at the top and under src/go2cs/internal/...) overwrote each other: one was never parsed
  # while n counted both. gofmt walks the scratch folder recursively, and both lists carry the same relative spelling.
  [ -n "$p" ] || continue; n=$((n+1)); f=${p#src/go2cs/}
  mkdir -p "$S/head/$(dirname "$f")"
  g cat-file blob "$h:$p" > "$S/head/$f"
  g cat-file -e "$b:$p" 2>/dev/null && { mkdir -p "$S/base/$(dirname "$f")"; g cat-file blob "$b:$p" > "$S/base/$f"; }
done < <(g diff --name-only --diff-filter=AM "$b" "$h" -- 'src/go2cs/*.go')
( cd "$S/head" && "$GF" -e -l . > ../head.list 2> ../head.err ); hrc=$?
( cd "$S/base" && "$GF" -e -l . > ../base.list 2> ../base.err )
echo "GOFMT-PARSE $(g rev-parse --short=10 "$b")..$(g rev-parse --short=10 "$h") go-files-changed-under-src/go2cs=$n gofmt-rc=$hrc unformatted-at-head=$(grep -c . "$S/head.list") of-which-new=$(LC_ALL=C comm -23 <(sort "$S/head.list") <(sort "$S/base.list") | grep -c .) parse-errors=$(grep -c . "$S/head.err")"
sed 's/^/  ERR /' "$S/head.err" | head -20
LC_ALL=C comm -23 <(sort "$S/head.list") <(sort "$S/base.list") | sed 's/^/  UNFORMATTED (new) /' | head -20
rm -rf "$S"
