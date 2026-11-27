#!/bin/bash
# Drift regen: five-target -stdlib emission at master 317b273e58 into a SEEDED copy of the tree src
# (core, version.props, docs/validation and the roster; without the last two the READMEs lose their Tests badge).
set -u
S=${LOGDIR:?set LOGDIR to a scratch log directory}; mkdir -p $S
SRC=${SRC:?set SRC to the tree src directory at the measured commit}
BIN=${BIN:?set BIN to the converter binary path to build}
R=${ROOT:?set ROOT to the seeded output root (wiped)}
STAGE=${STAGE:?set STAGE to the platform staging root (wiped)}
export CGO_ENABLED=0  # GOROOT and PATH name the go1.26.11 toolchain, as go env GOROOT prints it
# A five-target run without a soft memory limit was OOM-killed at 13.9 GB resident (cgroup); P2 recipe.
export GOMEMLIMIT=8GiB
unset GOTOOLCHAIN
cd "$S"
echo "go: $(GOTOOLCHAIN=local go version) root $(go env GOROOT)"
(cd $SRC/go2cs && go build -o $BIN .) || { echo "STOP: build"; exit 4; }
echo "bin: $(go version $BIN)"
free=$(df --output=avail -BG / | tail -1 | tr -dc 0-9); echo "free ${free}G"
[ "$free" -ge 11 ] || { echo "STOP: ${free}G free"; exit 3; }
rm -rf $R $STAGE; mkdir -p $R/src $STAGE
(cd $SRC && tar --exclude='*/bin' --exclude='*/obj' --exclude='*/Generated' -cf - core version.props) | tar -C $R/src -xf -
(cd $SRC/.. && tar -cf - docs/validation docs/ValidatedTestPackages.md) | tar -C $R -xf -
echo "seeded docs: $(find $R/docs -type f | wc -l) files"
echo "seeded .cs=$(find $R/src/core -name '*.cs' | wc -l) src-tracked .cs=$(cd $SRC && git ls-files core | grep -c '\.cs$')"
( while sleep 30; do p=$(pgrep -f "^$BIN -stdlib" | head -1); [ -n "$p" ] && echo "$(date -u +%T) rss=$(( $(awk '/VmRSS/{print $2}' /proc/$p/status) / 1024 ))MB $(grep -c '=== \[' $S/emit.log)/5"; done ) > $S/rss.log 2>&1 &
SAMPLER=$!
start=$(date +%s)
$BIN -stdlib -comments -convert-timeout 20m -platforms windows/amd64,linux/amd64,darwin/amd64,linux/arm64,darwin/arm64 \
  -platform-stage $STAGE -go2cspath $R/src > $S/emit.log 2>&1
rc=$?
kill $SAMPLER 2>/dev/null
echo "rc=$rc wall=$(( $(date +%s)-start ))s free=$(df --output=avail -BG / | tail -1 | tr -dc 0-9)G"
tail -40 $S/emit.log
