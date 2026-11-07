#!/usr/bin/env bash
# P-256 at the union (master 31dc1870cb + the seat): images, landing A/B, then the full battery. Serial, alone on R-LAPTOP.
set -u
source /c/go2cs-tmp/h10-scratch/env.sh >/dev/null 2>&1
export DOTNET_ROOT='%USERPROFILE%\dotnet10'
export PATH="$HOME/dotnet10:/c/Program Files (x86)/Microsoft Visual Studio/Installer:$PATH"
PWSH=$HOME/.dotnet/tools/pwsh.exe
WT=/c/go2cs-tmp/r-p256
K=/c/go2cs-tmp/r-kit
B=$K/p256bench-base; P=$K/p256bench; T=$K/tlsp256
L=$K/p256bench/union
mkdir -p "$L"
echo "start $(date -u +%FT%TZ) HEAD=$(git -C $WT rev-parse HEAD)"
step() { local name=$1; shift; local t0=$(date +%s); "$@" > "$L/$name.log" 2>&1 < /dev/null; local rc=$?; echo "STEP $name rc=$rc wall=$(( $(date +%s) - t0 ))s"; }

# --- images ---
rm -rf /c/go2cs-tmp/r-p256-base4 && mkdir -p /c/go2cs-tmp/r-p256-base4 && (cd $WT && git archive 31dc1870cb src) | tar -x -C /c/go2cs-tmp/r-p256-base4 || exit 3
GB=C:/go2cs-tmp/r-p256-base4/src/; GC=C:/go2cs-tmp/r-p256/src/
(cd $WT/src/go2cs && go build -o C:/go2cs-tmp/r-kit/go2cs-p256u.exe .) || exit 4
rm -rf $T/cs3 && mkdir -p $T/cs3 && cp $T/main.go $T/go.mod $T/go2cs.ico $T/cs3/ && (cd $T/cs3 && $K/go2cs-p256u.exe -go2cspath C:/go2cs-tmp/r-p256/src C:/go2cs-tmp/r-kit/tlsp256/cs3 C:/go2cs-tmp/r-kit/tlsp256/cs3 > conv.log 2>&1) || exit 5
echo "tls main.cs union vs B3 conversion: $(cmp -s <(tr -d '\r' < $T/cs2/main.cs) <(tr -d '\r' < $T/cs3/main.cs) && echo same || echo DIFFERS)"
step img-bench-base bash -c "cd $B && rm -rf out-jit out-r2r obj bin && dotnet build p256bench.csproj -c Release -p:go2csPath=$GB -o out-jit -v q -nologo && dotnet publish p256bench.csproj -c Release -p:go2csPath=$GB -p:R2RArm=true -o out-r2r -v q -nologo"
step img-bench-cut bash -c "cd $P && rm -rf out-jit-land out-r2r-land obj bin && dotnet build p256bench.csproj -c Release -p:go2csPath=$GC -o out-jit-land -v q -nologo && dotnet publish p256bench.csproj -c Release -p:go2csPath=$GC -p:R2RArm=true -o out-r2r-land -p:PublishReadyToRunCrossgen2ExtraArgs=--codegenopt:JitDisasm=p256Mul%3B--codegenopt:JitStdOutFile=C:/go2cs-tmp/r-kit/p256bench/union/r2r-final-p256Mul.txt -v q -nologo"
for arm in base cut; do
  if [ $arm = base ]; then G=$GB; else G=$GC; fi
  step img-tls-$arm bash -c "rm -rf $T/cs-$arm && cp -r $T/cs3 $T/cs-$arm && cd $T/cs-$arm && dotnet build TlsP256Scout.csproj -c Release -p:go2csPath=$G -v q -nologo && dotnet publish TlsP256Scout.csproj -c Release -p:go2csPath=$G -r win-x64 --self-contained -p:PublishSingleFile=true -p:PublishTrimmed=false -o out-r2r -v q -nologo"
done

# --- landing A/B at the union ---
sed -e 's#/c/go2cs-tmp/r-kit/p256bench/landing2#/c/go2cs-tmp/r-kit/p256bench/union-ab#g' -e 's#p256bench/landing2/#p256bench/union-ab/#g' $P/landing-ab.sh > $L/union-ab.sh
step landing-ab bash $L/union-ab.sh
grep -E "CHECK|fiat methods|rc=" "$L/landing-ab.log"

# --- battery ---
step gotest bash -c "cd $WT/src/go2cs && go test ./... -timeout 60m -count=1"
grep -E "^(FAIL|---)" "$L/gotest.log" | head
step golibtests bash -c "cd $WT/src/tests/GolibTests && dotnet test -c Release"
grep -E "Passed!|Failed!" "$L/golibtests.log"
step behavioral bash -c "cd $WT/src/tests/Behavioral && $PWSH -NoProfile -NonInteractive -File run-behavioral.ps1 --filter CurveBytes"
tail -7 "$L/behavioral.log"
step slnx bash -c "cd $WT/src && dotnet build go2cs.slnx -c Debug -v q -nologo"
grep -cE " error " "$L/slnx.log"
step cnr bash -c "cd $WT/src/tests/Behavioral && $PWSH -NoProfile -NonInteractive -File check-no-regression.ps1"
grep -E "REGRESSION|CHANGED| M src" "$L/cnr.log" | tail -4 | cut -c1-300
for pkg in crypto/internal/fips140/nistec crypto/ecdh crypto/ecdsa crypto/internal/fips140test crypto/tls; do
  n=sweep-$(echo $pkg | tr '/' '_')
  step $n bash -c "cd $WT/src && $PWSH -NoProfile -NonInteractive -File run-validated-sweep.ps1 -Filter $pkg -Exact"
  grep -E "PASS|FAIL" "$L/$n.log" | tail -2
  echo "tracked deletions after $pkg: $(git -C $WT status --porcelain | grep -c '^ D')"
done
echo "porcelain at end:"; git -C $WT status --porcelain | grep -v '^??' | head -40
echo "end $(date -u +%FT%TZ)"
