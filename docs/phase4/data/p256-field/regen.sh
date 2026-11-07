#!/usr/bin/env bash
# Two-seeded three-target -stdlib emission for the P-256 seat. Arms run SEQUENTIALLY (never overlap).
# RE-DERIVE on master 31dc1870cb (P-384 landed, 235-238): base converter = d05af07436 (red-first, no p256 entry); cut = working tree.
# Both roots seeded from ONE frozen snapshot of the working tree's src/core (which holds p256_field_impl.cs).
set -u
source /c/go2cs-tmp/h10-scratch/env.sh >/dev/null 2>&1
WT=/c/go2cs-tmp/r-p256
R=/c/go2cs-tmp/r-p256-regen3
LOG=$R/regen.log
rm -rf "$R" && mkdir -p "$R"
exec > >(tee -a "$LOG") 2>&1
echo "start $(date -u +%FT%TZ) go=$(go version) GOROOT=$(go env GOROOT)"

# Converters
mkdir -p "$R/conv-base" && (cd "$WT" && git archive d05af07436 src/go2cs) | tar -x -C "$R/conv-base" || exit 10
(cd "$R/conv-base/src/go2cs" && go build -o "C:/go2cs-tmp/r-p256-regen3/go2cs-base.exe" .) || exit 11
(cd "$WT/src/go2cs" && go build -o "C:/go2cs-tmp/r-p256-regen3/go2cs-cut.exe" .) || exit 12
ls -la --time-style=full-iso "$R/go2cs-base.exe" "$R/go2cs-cut.exe"
go version "$R/go2cs-base.exe" "$R/go2cs-cut.exe"
cmp -s "$R/go2cs-base.exe" "$R/go2cs-cut.exe" && { echo "ABORT: base and cut binaries identical"; exit 13; }

# One frozen snapshot, then seed both roots from it
SNAP=$R/snap
mkdir -p "$SNAP/src" "$SNAP/docs"
(cd "$WT/src" && tar --exclude='*/bin' --exclude='*/obj' --exclude='*/Generated' -cf - core) | tar -xf - -C "$SNAP/src" || exit 14
cp "$WT/src/version.props" "$SNAP/src/" && cp -r "$WT/docs/validation" "$SNAP/docs/" && cp "$WT/docs/ValidatedTestPackages.md" "$SNAP/docs/" || exit 15
for arm in base cut; do mkdir -p "$R/root-$arm" && cp -r "$SNAP/src" "$SNAP/docs" "$R/root-$arm/" || exit 16; done
echo "seeded .cs: snap=$(find "$SNAP/src/core" -name '*.cs' | wc -l) base=$(find "$R/root-base/src/core" -name '*.cs' | wc -l) cut=$(find "$R/root-cut/src/core" -name '*.cs' | wc -l)"
test -f "$R/root-cut/src/core/crypto/internal/fips140/nistec/fiat/p256_field_impl.cs" || { echo "ABORT: p256_field_impl.cs not seeded"; exit 17; }
touch "$R/sentinel"; sleep 2

for arm in base cut; do
  echo "== arm $arm $(date -u +%FT%TZ)"
  t0=$(date +%s)
  "$R/go2cs-$arm.exe" -stdlib -comments -platforms windows/amd64,linux/amd64,darwin/amd64 \
     -platform-stage "C:/go2cs-tmp/r-p256-regen3/stage-$arm" -go2cspath "C:/go2cs-tmp/r-p256-regen3/root-$arm/src" > "$R/conv-$arm.out" 2>&1
  rc=$?
  echo "arm $arm rc=$rc wall=$(( $(date +%s) - t0 ))s written=$(find "$R/root-$arm/src/core" -newer "$R/sentinel" -type f | wc -l)"
  tail -5 "$R/conv-$arm.out"
  [ $rc -eq 0 ] || exit 20
done
echo "DYNTYPE markers: base=$(grep -rl 'DYNTYPE' "$R/root-base/src/core" --include='*.cs' | wc -l) cut=$(grep -rl 'DYNTYPE' "$R/root-cut/src/core" --include='*.cs' | wc -l)"
echo "== diff base vs cut"
diff -rq "$R/root-base/src/core" "$R/root-cut/src/core"
echo "diff-rc=$?"
echo "end $(date -u +%FT%TZ)"
