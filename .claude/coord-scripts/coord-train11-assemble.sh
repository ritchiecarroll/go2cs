#!/usr/bin/env bash
# coord-train11-assemble.sh -- merge train 11 after train 10 is pushed, then run its battery. Seats (remote tips, a NOTE
# if the tip differs from the announced SHA; an unset SHA skips the seat):
#   F8_SHA       claude/c2-f8-platform-exclusive       (F8: platform-exclusive marker + loud skip; MUST precede recvmsg -- branch name is a guess until C2 posts it)
#   RECVMSG_SHA  claude/c2-syscall-recvmsg             (moved from train 10: needs F8 for the Windows slnx/CNR/runner)
#   S2_SHA       claude/c2-syscall-unix-msg            (internal/syscall/unix Recvmsg/SendmsgN Inet4/6 over factored helpers; depends on recvmsg)
#   MATHBITS_SHA claude/g-mathbits-intrinsics          (math/bits registry + bits_impl.cs; converter class)
#   I9UTT_SHA    claude/i9-updatetesttargets-ordinal   (UTT ordinal sort + -test-config; converter + harness; rebased tip)
#   CHANDIR_SHA  claude/reflect-tail-r-chandir         (reflect chanDir; one-train cut)
#   ADDMUL_SHA   claude/g-board-addmulvvw              (board block: addMulVVW 12.7x, 93/7; docs only)
#   I9SWEEP_SHA  claude/i9-sweep-testconfig            (sweep -TestConfig/-TestTiered + release-tc0 fix; shared .ps1 -- needs C2 pwsh7 smoke)
# Battery: converter suite + full CNR; syscall.csproj linux; go2cs.slnx (compiles every behavioral project -- the COMPILE
# gate S2's stub removal and the UTT reorder owe); GolibTests + each-class-alone; reflect build; filtered behavioral
# run of UdpWriteMsgAddrPort + SendtoSeam + TypedNilPtrArrayDims (the reordered classes' MSTest side is the runner's
# concern); sweeps: math/bits, math/big, crypto/rsa, crypto/x509, nistec (cost, both directions), utf8, sort, strings,
# os/exec, unicode/utf8's -test-config Release smoke is i9's; reflect RUN.
set -uo pipefail
SP="C:/Projects/go2cs/.claude/coord-scripts"
export GOROOT='$HOME\sdk\go1.23.12'
export DOTNET_ROOT='$HOME\dotnet10'
export PATH="$HOME/sdk/go1.23.12/bin:$HOME/dotnet10:$PATH"
export MSBUILDDISABLENODEREUSE=1
RECVMSG_SHA="${RECVMSG_SHA:-e20510be9}"; F8_SHA="${F8_SHA:-aef9867416}"; S2_SHA="${S2_SHA:-fb0e7416e}"; MATHBITS_SHA="${MATHBITS_SHA:-}"; I9UTT_SHA="${I9UTT_SHA:-47c3b1e85}"; CHANDIR_SHA="${CHANDIR_SHA:-2a8c84bae}"; ADDMUL_SHA="${ADDMUL_SHA:-9be21c9f2}"; I9SWEEP_SHA="${I9SWEEP_SHA:-ac385553e}"
ts=$(date +%Y%m%d-%H%M%S)
log="$SP/coord-train11-merge-$ts.log"
stamp() { echo "[$(date '+%F %T')] $*" | tee -a "$log"; }
stamp "TRAIN 11 ASSEMBLE START head=$(git rev-parse --short HEAD) clean=$(git status --porcelain | wc -l)"
[ "$(git status --porcelain | wc -l)" = "0" ] || { stamp "worktree dirty -- ABORT"; exit 1; }
git fetch -q origin master || { stamp "fetch failed -- ABORT"; exit 1; }
git merge-base --is-ancestor origin/master HEAD || { stamp "HEAD is not on top of origin/master -- ABORT"; exit 1; }
merge_one() { # ref msg label
  if git merge --no-ff -S -q -F "$2" "$1"; then stamp "merged $3 -> $(git rev-parse --short HEAD)"; else
    unmerged=$(git diff --name-only --diff-filter=U | tr '\n' ' ')
    if [ "$unmerged" = "src/go2cs/manualTypeOperations.go " ]; then
      python - <<'PY'
import io,re
p='src/go2cs/manualTypeOperations.go'; s=io.open(p,encoding='utf-8',newline='').read(); n=s.count('<<<<<<<')
s2=re.sub(r'<<<<<<< [^\r\n]*\r?\n(.*?)=======\r?\n(.*?)>>>>>>> [^\r\n]*\r?\n', lambda m: m.group(1)+m.group(2), s, flags=re.S)
assert '<<<<<<<' not in s2 and '>>>>>>>' not in s2, 'residual markers'
io.open(p,'w',encoding='utf-8',newline='').write(s2); print('manualTypeOperations.go: resolved', n, 'block(s) by keeping both sides')
PY
      (cd src/go2cs && gofmt -w manualTypeOperations.go && go build ./...) || { stamp "go build after union FAILED -- ABORT"; git merge --abort 2>/dev/null; exit 1; }
      git add src/go2cs/manualTypeOperations.go && git commit -S -q -F "$2" || { stamp "$3 merge commit FAILED"; git merge --abort 2>/dev/null; exit 1; }
      stamp "merged $3 (registration union) -> $(git rev-parse --short HEAD)"
    elif [ -n "$unmerged" ] && ! echo "$unmerged" | tr ' ' '
' | grep -v '^$' | grep -vqE '^src/tests/Behavioral/BehavioralTests/(Compile|OutputComparison|TargetComparison|Transpile)Tests\.cs$'; then
      git checkout --ours -- $unmerged && git add $unmerged && git commit -S -q -F "$2" || { stamp "$3 test-class resolution FAILED"; git merge --abort 2>/dev/null; exit 1; }
      NEED_UTT=1; stamp "merged $3 (four test classes taken from ours; UTT re-derives them after the seats) -> $(git rev-parse --short HEAD)"
    else stamp "$3 merge CONFLICTS: $unmerged -- ABORT for hand resolution"; git merge --abort 2>/dev/null; exit 1; fi
  fi
}
seat() { # branch sha msg label
  [ -n "$2" ] || { stamp "$4: SHA unset -- seat SKIPPED"; return; }
  git fetch -q origin "$1" || { stamp "fetch $1 failed -- ABORT"; exit 1; }
  [ "$(git rev-parse origin/$1)" = "$(git rev-parse $2 2>/dev/null)" ] || stamp "NOTE: $1 tip is not $2 (announced) -- merging the remote tip; fix the message file"
  merge_one "origin/$1" "$3" "$4"
}
seat claude/c2-syscall-recvmsg "$RECVMSG_SHA" "$SP/coord-merge-c2-recvmsg.txt" "C2 Recvmsg seam"
seat claude/c2-syscall-unix-msg "$S2_SHA" "$SP/coord-merge-c2-s2.txt" "C2 S2"
seat claude/c2-f8-platform-exclusive "$F8_SHA" "$SP/coord-merge-c2-f8.txt" "C2 F8 platform-exclusive"
seat claude/g-mathbits-intrinsics "$MATHBITS_SHA" "$SP/coord-merge-g-mathbits.txt" "G math/bits"
seat claude/i9-updatetesttargets-ordinal "$I9UTT_SHA" "$SP/coord-merge-i9-testconfig.txt" "i9 UTT + test-config"
seat claude/reflect-tail-r-chandir "$CHANDIR_SHA" "$SP/coord-merge-r-chandir.txt" "R chanDir"
seat claude/g-board-addmulvvw "$ADDMUL_SHA" "$SP/coord-merge-g-board-addmulvvw.txt" "G board addMulVVW"
seat claude/i9-sweep-testconfig "$I9SWEEP_SHA" "$SP/coord-merge-i9-sweep-testconfig.txt" "i9 sweep test-config"
# Re-derive the four behavioral test classes with the (now ordinal) UpdateTestTargets -- a pure function of the project set,
# so hand-inserted +3 lines from R/C2 and i9's reorder reconcile mechanically. No --createTargetFiles (that would re-baseline goldens).
NEED_UTT="${NEED_UTT:-1}"
if [ "$NEED_UTT" = "1" ]; then
  (cd src/utilities/UpdateTestTargets && dotnet build -c Debug -p:UseSharedCompilation=false -clp:ErrorsOnly >/dev/null 2>&1) || { stamp "UpdateTestTargets build FAILED -- ABORT"; exit 1; }
  (cd src/utilities/UpdateTestTargets/bin/Debug/net10.0 && ./UpdateTestTargets.exe >/dev/null 2>&1) || { stamp "UpdateTestTargets run FAILED -- ABORT"; exit 1; }
  changed=$(git status --porcelain | wc -l); other=$(git status --porcelain | grep -vE 'BehavioralTests/(Compile|OutputComparison|TargetComparison|Transpile)Tests\.cs$' | wc -l)
  [ "$other" = "0" ] || { stamp "UTT touched files outside the four classes: $(git status --porcelain | tr '
' ' ') -- ABORT"; exit 1; }
  if [ "$changed" != "0" ]; then git add src/tests/Behavioral/BehavioralTests/*Tests.cs && git commit -S -q -m "tests: re-derive the four behavioral test classes (ordinal UpdateTestTargets) after the train-11 seats

The seats carry hand-inserted +3 registrations (R's ReflectChanNarrowing) beside i9's ordinal reorder of the same blocks; the utility is a pure function of the project set, so one run reconciles them. numstat: $(git diff --cached --numstat | tr '
' ' ')

Co-Authored-By: Claude Fable 5.1 <noreply@anthropic.com>" && stamp "UTT re-derived the four classes -> $(git rev-parse --short HEAD)"; else stamp "UTT: four classes already canonical (0 0)"; fi
  (cd src/utilities/UpdateTestTargets/bin/Debug/net10.0 && ./UpdateTestTargets.exe >/dev/null 2>&1); [ "$(git status --porcelain | wc -l)" = "0" ] && stamp "UTT idempotency: 0 0" || { stamp "UTT NOT idempotent: $(git status --porcelain | tr '
' ' ') -- ABORT"; exit 1; }
fi
(cd src/go2cs && go build -o bin/go2cs.exe . ) && stamp "converter rebuilt at $(git rev-parse --short HEAD): $(stat -c %s src/go2cs/bin/go2cs.exe) B" || { stamp "converter build FAILED -- ABORT"; exit 1; }
stamp "TRAIN 11 ASSEMBLED head=$(git rev-parse --short HEAD) clean=$(git status --porcelain | wc -l)"

stamp "BATTERY: converter suite + full CNR"
powershell -NoProfile -ExecutionPolicy Bypass -File "$SP/coord-union-battery.ps1" > "$SP/coord-train11-suite-cnr-$ts.log" 2>&1; echo "suite+cnr exit=$?" >> "$SP/coord-train11-suite-cnr-$ts.log"
git checkout -q HEAD -- src/tests/Behavioral 2>/dev/null; stamp "post-CNR restore: behavioral dirt reverted ($(git status --porcelain -- src/tests/Behavioral | wc -l) left)"
stamp "BATTERY: syscall.csproj on linux (obj purged first)"
powershell -NoProfile -ExecutionPolicy Bypass -Command "\$env:DOTNET_ROOT='$HOME\dotnet10'; \$env:PATH=\"\$env:DOTNET_ROOT;\$env:PATH\"; \$env:MSBUILDDISABLENODEREUSE='1'; Set-Location '$(pwd -W)/src/core/syscall'; Remove-Item -Recurse -Force bin,obj -ErrorAction SilentlyContinue; dotnet build syscall.csproj -c Debug -p:GoTargetOS=linux -p:UseSharedCompilation=false -clp:ErrorsOnly 2>&1 | Select-String -Pattern 'error (CS|MSB|NETSDK)[0-9]+|Build succeeded|error\(s\)' | Select-Object -First 10; Remove-Item -Recurse -Force bin,obj -ErrorAction SilentlyContinue" > "$SP/coord-train11-syscall-linux-$ts.log" 2>&1; echo "syscall-linux exit=$?" >> "$SP/coord-train11-syscall-linux-$ts.log"
stamp "BATTERY: slnx (the behavioral COMPILE gate)"
powershell -NoProfile -ExecutionPolicy Bypass -File "$SP/coord-slnx-build.ps1" > "$SP/coord-train11-slnx-$ts.log" 2>&1; echo "slnx exit=$?" >> "$SP/coord-train11-slnx-$ts.log"
stamp "BATTERY: GolibTests --no-build + each-class-alone"
powershell -NoProfile -ExecutionPolicy Bypass -File "$SP/coord-golibtests.ps1" -NoBuild > "$SP/coord-train11-golibtests-$ts.log" 2>&1; echo "golibtests exit=$?" >> "$SP/coord-train11-golibtests-$ts.log"
powershell -NoProfile -ExecutionPolicy Bypass -File "$SP/coord-golibtests-alone.ps1" > "$SP/coord-train11-alone-$ts.log" 2>&1; echo "alone exit=$?" >> "$SP/coord-train11-alone-$ts.log"
stamp "BATTERY: reflect -tests build"
powershell -NoProfile -ExecutionPolicy Bypass -File "$SP/coord-reflect-tests-build.ps1" > "$SP/coord-train11-reflect-$ts.log" 2>&1; echo "reflect exit=$?" >> "$SP/coord-train11-reflect-$ts.log"
stamp "BATTERY: filtered behavioral run (UdpWriteMsgAddrPort, SendtoSeam, TypedNilPtrArrayDims)"
(cd src/tests/Behavioral && for f in UdpWriteMsgAddrPort SendtoSeam TypedNilPtrArrayDims; do powershell -NoProfile -ExecutionPolicy Bypass -File ./run-behavioral.ps1 --filter $f; echo "runner $f exit=$?"; done) > "$SP/coord-train11-runner-$ts.log" 2>&1; git checkout HEAD -- src/tests/Behavioral 2>/dev/null
stamp "BATTERY: sweeps"
powershell -NoProfile -ExecutionPolicy Bypass -File "$SP/coord-union-battery.ps1" -SkipSuite -SkipCNR -Sweeps math/bits,math/big,crypto/rsa,crypto/x509,crypto/internal/nistec,encoding/json,encoding/xml,go/types,unicode/utf8,sort,strings,os/exec -SweepTimeout 10m > "$SP/coord-train11-sweeps-$ts.log" 2>&1; echo "sweeps exit=$?" >> "$SP/coord-train11-sweeps-$ts.log"
stamp "BATTERY: reflect RUN"
powershell -NoProfile -ExecutionPolicy Bypass -File "$SP/coord-reflect-run.ps1" -Package reflect -Timeout 30m -Label coord-train11 > "$SP/coord-train11-reflectrun-$ts.log" 2>&1; echo "reflectrun exit=$?" >> "$SP/coord-train11-reflectrun-$ts.log"
stamp "TRAIN 11 CHAIN DONE"
