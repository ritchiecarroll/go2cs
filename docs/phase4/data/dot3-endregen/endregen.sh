#!/usr/bin/env bash
# .3 END REGEN (freeze fixed-point): a SEEDED three-target -stdlib -comments reconvert at master a0fba9b08a against the
# committed src/core, layout L3. One conversion on the box. Produces the root, the marker gate and the differing-file list;
# the overlay and the hunk attribution are done after, by hand, from those.
set -u
source /c/go2cs-tmp/h10-scratch/env.sh >/dev/null 2>&1
WT=/c/go2cs-tmp/r-endregen
R=/c/go2cs-tmp/r-endregen-run
rm -rf "$R" && mkdir -p "$R"
LOG=$R/endregen.log
exec > >(tee -a "$LOG") 2>&1
echo "start $(date -u +%FT%TZ) HEAD=$(git -C $WT rev-parse HEAD) go=$(go version) GOROOT=$(go env GOROOT)"

# converter, proven fresh
(cd $WT/src/go2cs && go build -o "C:/go2cs-tmp/r-endregen-run/go2cs.exe" .) || { echo "ABORT: build"; exit 10; }
ls -la --time-style=full-iso "$R/go2cs.exe"; go version "$R/go2cs.exe"

# seed: src/core (no bin/obj/Generated), version.props, docs/validation, docs/ValidatedTestPackages.md
mkdir -p "$R/root/src" "$R/root/docs"
(cd $WT/src && tar --exclude='*/bin' --exclude='*/obj' --exclude='*/Generated' -cf - core) | tar -xf - -C "$R/root/src" || { echo "ABORT: seed"; exit 11; }
cp $WT/src/version.props "$R/root/src/" && cp -r $WT/docs/validation "$R/root/docs/" && cp $WT/docs/ValidatedTestPackages.md "$R/root/docs/" || { echo "ABORT: seed2"; exit 12; }
echo "seeded .cs: committed=$(cd $WT/src/core && git ls-files '*.cs' | wc -l) root=$(find $R/root/src/core -name '*.cs' | wc -l)"

# marker census BEFORE (line-anchored, whole file), as content hashes (CR-stripped)
(cd $WT/src/core && grep -rlE --include='*.cs' '^\s*\[module:\s*(go\.)?GoManualConversion\]' . | sort) > "$R/marked.txt"
echo "marked files: $(wc -l < $R/marked.txt)"
while read -r f; do printf '%s %s\n' "$(tr -d '\r' < "$WT/src/core/$f" | sha256sum | cut -c1-16)" "$f"; done < "$R/marked.txt" > "$R/marked-before.txt"

touch "$R/sentinel"; sleep 2
t0=$(date +%s)
"$R/go2cs.exe" -stdlib -comments -platforms windows/amd64,linux/amd64,darwin/amd64 \
   -platform-stage "C:/go2cs-tmp/r-endregen-run/stage" -go2cspath "C:/go2cs-tmp/r-endregen-run/root/src" > "$R/conv.out" 2>&1
rc=$?
echo "convert rc=$rc wall=$(( $(date +%s) - t0 ))s"
tail -4 "$R/conv.out"
[ $rc -eq 0 ] || { echo "ABORT: convert rc=$rc"; exit 20; }
for t in windows-amd64 linux-amd64 darwin-amd64; do
  echo "stage $t: total .cs $(find $R/stage/$t/src/core -name '*.cs' | wc -l), written this run $(find $R/stage/$t/src/core -name '*.cs' -newer $R/sentinel | wc -l)"
done
echo "root written this run: $(find $R/root/src/core -type f -newer $R/sentinel | wc -l)"
echo "DYNTYPE files in root: $(grep -rl 'DYNTYPE' $R/root/src/core --include='*.cs' | wc -l)"

# marker gate, path-precise: every marked committed file must be unchanged in the root (CR-stripped)
bad=0
while read -r h f; do
  h2=$(tr -d '\r' < "$R/root/src/core/$f" | sha256sum | cut -c1-16)
  [ "$h" = "$h2" ] || { echo "MARKER GATE FAIL: $f"; bad=$((bad+1)); }
done < "$R/marked-before.txt"
echo "marker gate: $bad clobbered of $(wc -l < $R/marked-before.txt)"

# the differing set, CR-stripped, against the committed tree (root holds seeded + emitted)
(cd "$R/root/src/core" && find . -type f \( -name '*.cs' -o -name '*.cs.auto' -o -name '*.csproj' -o -name 'README.md' \) | sort) > "$R/root-files.txt"
: > "$R/differ.txt"; : > "$R/new.txt"
while read -r f; do
  if [ -f "$WT/src/core/$f" ]; then
    cmp -s <(tr -d '\r' < "$WT/src/core/$f") <(tr -d '\r' < "$R/root/src/core/$f") || echo "$f" >> "$R/differ.txt"
  else
    echo "$f" >> "$R/new.txt"
  fi
done < "$R/root-files.txt"
echo "differ (CR-stripped): $(wc -l < $R/differ.txt)  new files: $(wc -l < $R/new.txt)"
echo "  of differ: .cs.auto $(grep -c '\.cs\.auto$' $R/differ.txt) README $(grep -c 'README.md$' $R/differ.txt) csproj $(grep -c '\.csproj$' $R/differ.txt)"
echo "end $(date -u +%FT%TZ)"
