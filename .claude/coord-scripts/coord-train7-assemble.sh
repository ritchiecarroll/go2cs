#!/usr/bin/env bash
# coord-train7-assemble.sh -- merge train 7's branches in order into the coordinator worktree, resolving the
# two known conflicts deterministically, then launch its battery chain. Run from the worktree root.
# Preconditions: master pushed at train 6's head; worktree clean; no battery running.
set -uo pipefail
SP="C:/Projects/go2cs/.claude/coord-scripts"
export GOROOT='$HOME\sdk\go1.23.12'
export DOTNET_ROOT='$HOME\dotnet10'
export PATH="$HOME/sdk/go1.23.12/bin:$HOME/dotnet10:$PATH"
export MSBUILDDISABLENODEREUSE=1
ts=$(date +%Y%m%d-%H%M%S)
log="$SP/coord-train7-merge-$ts.log"
stamp() { echo "[$(date '+%F %T')] $*" | tee -a "$log"; }

stamp "TRAIN 7 ASSEMBLE START head=$(git rev-parse --short HEAD) clean=$(git status --porcelain | wc -l)"
[ "$(git status --porcelain | wc -l)" = "0" ] || { stamp "worktree dirty -- ABORT"; exit 1; }
git fetch -q origin master claude/c2-structof-gcbits claude/reflect-tail-r-lite claude/c2-syscall-linux-nil-guard \
  claude/c1-gated-stamp claude/c2-tz-pin claude/i9-runtime-regen claude/g-nethttp-ladder-text \
  claude/c1-syscall-platform-skip claude/g-perf-tls-handshake || { stamp "fetch failed -- ABORT"; exit 1; }
git merge-base --is-ancestor origin/master HEAD || { stamp "HEAD is not on top of origin/master -- ABORT"; exit 1; }

merge_plain() { # ref msgfile label
  if git merge --no-ff -S -q -F "$2" "$1"; then stamp "merged $3 -> $(git rev-parse --short HEAD)"; else stamp "MERGE FAILED $3"; git merge --abort 2>/dev/null; exit 1; fi
}

# 1. C2 pair (items 1-3 + gcbits): the branch re-applies train 6's first four commits under new SHAs plus item 3;
#    the only content conflict is GoReflect.TypeLayout.cs, where the branch's file is the superset -- take it whole.
git merge --no-ff --no-commit -q origin/claude/c2-structof-gcbits 2>&1 | grep -i conflict | tee -a "$log"
if git diff --name-only --diff-filter=U | grep -q .; then
  for f in $(git diff --name-only --diff-filter=U); do
    case "$f" in
      src/core/golib/GoReflect.TypeLayout.cs) git checkout --theirs -- "$f"; git add "$f"; stamp "took branch file for $f";;
      src/go2cs/manualTypeOperations.go)
        python - <<'EOF'
import io
p='src/go2cs/manualTypeOperations.go'
raw=io.open(p,encoding='utf-8',newline='').read(); nl='\r\n' if '\r\n' in raw else '\n'
L=raw.split(nl); out=[]; i=0; n=0
while i < len(L):
    if L[i].startswith('<<<<<<< '):
        j=i+1; ours=[]
        while not L[j].startswith('======='): ours.append(L[j]); j+=1
        k=j+1; theirs=[]
        while not L[k].startswith('>>>>>>> '): theirs.append(L[k]); k+=1
        keys=lambda ls: [l.strip() for l in ls if '":' in l]
        merged=ours+[t for t in theirs if t.strip() not in [o.strip() for o in ours]]
        out.extend(merged); n+=1; i=k+1
    else: out.append(L[i]); i+=1
io.open(p,'w',encoding='utf-8',newline='').write(nl.join(out)); print('manualTypeOperations.go: resolved %d block(s) keeping both sides' % n)
EOF
        gofmt -w "$f"; git add "$f"; stamp "kept both sides in $f (gofmt applied)";;
      *) stamp "UNEXPECTED CONFLICT in $f -- ABORT"; git merge --abort; exit 1;;
    esac
  done
fi
[ "$(git diff --name-only --diff-filter=U | wc -l)" = "0" ] || { stamp "unresolved conflicts remain -- ABORT"; git merge --abort; exit 1; }
git diff --quiet origin/claude/c2-structof-gcbits -- src/core/golib/GoReflect.TypeLayout.cs || { stamp "TypeLayout.cs != branch file after resolution -- ABORT"; git merge --abort; exit 1; }
git -c core.editor=true commit -S -q -F "$SP/coord-merge-c2-pair.txt" && stamp "merged C2 pair -> $(git rev-parse --short HEAD)"
( cd src/go2cs && go build ./... ) && stamp "go build ok after C2 pair" || { stamp "GO BUILD FAILED after C2 pair -- ABORT"; exit 1; }

merge_plain origin/claude/reflect-tail-r-lite       "$SP/coord-merge-r-tail-rebased.txt"  "R rebased tip"
merge_plain origin/claude/c2-syscall-linux-nil-guard "$SP/coord-merge-c2-nil-guard.txt"    "C2 nil-guard"
merge_plain origin/claude/c1-gated-stamp             "$SP/coord-merge-c1-gated-stamp.txt"  "C1 gated stamp"
merge_plain origin/claude/c2-tz-pin                  "$SP/coord-merge-c2-tz-pin.txt"       "C2 TZ pin"
merge_plain origin/claude/i9-runtime-regen           "$SP/coord-merge-i9-regen.txt"        "i9 runtime regen"
merge_plain origin/claude/g-nethttp-ladder-text      "$SP/coord-merge-g-ladder-text.txt"   "G ladder text"
merge_plain origin/claude/c1-syscall-platform-skip   "$SP/coord-merge-c1-mints.txt"        "C1 mints"
merge_plain origin/claude/g-perf-tls-handshake       "$SP/coord-merge-g-perf-tls.txt"      "G perf row (blocked)"

( cd src/go2cs && gofmt -l . | sed 's/^/gofmt would reformat: /' | tee -a "$log"; go build ./... ) && stamp "go build ok at train head" || { stamp "GO BUILD FAILED at train head"; exit 1; }
stamp "TRAIN 7 ASSEMBLED head=$(git rev-parse --short HEAD) clean=$(git status --porcelain | wc -l)"
# The battery chain is launched by the caller (background) after this script reports ASSEMBLED.
