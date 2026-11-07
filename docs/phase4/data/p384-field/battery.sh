#!/usr/bin/env bash
# P-384 seat gate battery, serial, alone on the box, at the worktree's HEAD. Launched from a per-run COPY of this script.
set -u
source /c/go2cs-tmp/h10-scratch/env.sh >/dev/null 2>&1
export DOTNET_ROOT='%USERPROFILE%\dotnet10'
export PATH="$HOME/dotnet10:/c/Program Files (x86)/Microsoft Visual Studio/Installer:$PATH"
PWSH=$HOME/.dotnet/tools/pwsh.exe
WT=/c/go2cs-tmp/r-p384
L=/c/go2cs-tmp/r-kit/p384bench/battery
mkdir -p "$L"
echo "battery start $(date -u +%FT%TZ) HEAD=$(git -C $WT rev-parse HEAD)"
step() { local name=$1; shift; local t0=$(date +%s); "$@" > "$L/$name.log" 2>&1; local rc=$?; echo "STEP $name rc=$rc wall=$(( $(date +%s) - t0 ))s"; }

step gotest bash -c "cd $WT/src/go2cs && go test ./... -timeout 60m -count=1"
grep -E "^(ok|FAIL|---)" "$L/gotest.log" | grep -v "^ok" | head -20
step golibtests bash -c "cd $WT/src/tests/GolibTests && dotnet test -c Release"
grep -E "Passed!|Failed!" "$L/golibtests.log"
step behavioral bash -c "cd $WT/src/tests/Behavioral && $PWSH -NoProfile -File run-behavioral.ps1 --filter CurveBytes"
tail -8 "$L/behavioral.log"
step slnx bash -c "cd $WT/src && dotnet build go2cs.slnx -c Debug -v q -nologo"
grep -cE " error " "$L/slnx.log"
step cnr bash -c "cd $WT/src/tests/Behavioral && $PWSH -NoProfile -File check-no-regression.ps1"
tail -6 "$L/cnr.log"
for pkg in crypto/internal/fips140/nistec crypto/ecdh crypto/ecdsa crypto/internal/fips140test crypto/tls; do
  n=sweep-$(echo $pkg | tr '/' '_')
  step $n bash -c "cd $WT/src && $PWSH -NoProfile -File run-validated-sweep.ps1 -Filter $pkg -Exact"
  grep -E "PASS|FAIL|banked|SUMMARY" "$L/$n.log" | tail -3
  echo "tracked deletions after $pkg: $(git -C $WT status --porcelain | grep -c '^ D')"
done
echo "porcelain at end:"; git -C $WT status --porcelain | grep -v '^??' | head -20
echo "battery end $(date -u +%FT%TZ)"
