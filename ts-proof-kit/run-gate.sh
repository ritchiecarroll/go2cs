#!/usr/bin/env bash
# run-gate.sh <go2cs ref> <local|nuget> [GoStdLibVersion] : the typescript-go-cs gate on linux, end to end, at one ref.
# RESUMABLE: every step writes into $W/<tag>/ and is skipped when its output exists, so a run cut off by a time limit
# is continued by running the same command again. Every step is gated on the previous one's exit code.
#   1. a detached worktree of the go2cs clone at <ref>, and its go2cs binary
#   2. the module: TypeScript ba29a56299 tsc/ minus testdata/, plus go/cmd/* from this kit; NATIVE demos built from it
#      unmodified; the corpus extracted; then a copy with the xxh3 fork's replace + go mod tidy for the conversion
#   3. go2cs -recurse (local) or -recurse=nuget; the five refused packages are skipped (rc 1 by design)
#   4. dotnet build of the four demos, Release, GoTargetOS=linux
#   5. compare.py (parse demo + corpus), demo_compare.py x3, typecheck_slice.py x2 (type-check, emit: all 7,404)
#   6. layout.py stage + fill + census into $W/<tag>/stage
# Env: GO2CS (a go2cs clone), TS (a TypeScript checkout at ba29a56299), W (work root), KIT (this directory),
#      DENY (a local file of name regexes for the census; never committed),
#      OWNER_README (optional: the repository's README.md; fill then writes README.patch beside the staged tree).
set -u
ref="$1"; mode="$2"; gsv="${3:-}"
: "${GO2CS:?}" "${TS:?}" "${W:?}"; KIT="${KIT:-$(cd "$(dirname "$0")" && pwd)}"
export GOTOOLCHAIN=local CGO_ENABLED=0 GOWORK=off GOSUMDB=sum.golang.org GOPROXY=https://proxy.golang.org
export GoTargetOS=linux MSBUILDDISABLENODEREUSE=1 DOTNET_CLI_TELEMETRY_OPTOUT=1
git -C "$GO2CS" fetch -q origin || { echo "fetch failed"; exit 1; }
sha=$(git -C "$GO2CS" rev-parse --short=10 "$ref^{commit}") || { echo "bad ref $ref"; exit 1; }
T="$W/gate-$sha-$mode"; mkdir -p "$T"; log="$T/gate.log"
step() { echo "== $(date -u +%H:%M:%SZ) $*" | tee -a "$log"; }
die() { echo "FAILED: $*" | tee -a "$log"; exit 1; }
WT="$T/go2cs"; MODN="$T/tsc-native"; MODC="$T/tsc-convert"; OUT="$T/out"; R="$T/results"; B="$T/bin"
mkdir -p "$B" "$R"

[ -x "$B/go2cs" ] || { step "worktree + go2cs at $sha"
  [ -d "$WT" ] || git -C "$GO2CS" worktree add -q --detach "$WT" "$sha" || die worktree
  (cd "$WT/src/go2cs" && go build -o "$B/go2cs" .) || die "go2cs build"; }

[ -f "$B/.native-ok" ] || { step "native module, demos, corpus"
  rm -rf "$MODN" && mkdir -p "$MODN" && (cd "$TS/tsc" && tar --exclude=./testdata -cf - .) | (cd "$MODN" && tar xf -) || die copy
  cp -r "$KIT/go/cmd/." "$MODN/cmd/" || die "demo copy"
  (cd "$MODN" && go mod download && go mod verify) >> "$log" 2>&1 || die "mod verify"
  for d in parsedemo checkdemo emitdemo mfdemo parsecorpus-extract; do (cd "$MODN" && go build -o "$B/$d-go" ./cmd/$d) || die "native $d"; done
  [ -d "$T/corpus" ] || "$B/parsecorpus-extract-go" "$TS/tsc" "$T/corpus" >> "$log" 2>&1 || die extract
  touch "$B/.native-ok"; }

[ -f "$OUT/.convert-done" ] || { step "convert ($mode)"
  rm -rf "$MODC" "$OUT" && cp -a "$MODN" "$MODC" || die copy
  (cd "$MODC" && go mod edit -replace "github.com/zeebo/xxh3=$WT/src/forks/github.com/zeebo/xxh3@v1.1.0" && go mod tidy) >> "$log" 2>&1 || die replace
  flag=-recurse; [ "$mode" = nuget ] && flag=-recurse=nuget
  "$B/go2cs" $flag -go2cspath "$WT/src" "$MODC" "$OUT" > "$T/convert.log" 2>&1; rc=$?
  echo "convert rc=$rc (1 expected: refused packages skipped)" | tee -a "$log"
  grep -q "packages converted" "$T/convert.log" || die "convert did not finish"
  go clean -cache; touch "$OUT/.convert-done"; }

C="$OUT/src/github.com/microsoft/TypeScript/tsc/cmd"; P=(); [ -n "$gsv" ] && P=(-p:GoStdLibVersion=$gsv)
for d in checkdemo parsedemo emitdemo mfdemo; do
  [ -f "$B/.built-$d" ] && continue; step "build $d"
  dotnet build "$C/$d/$d.csproj" -c Release -m:2 -p:UseSharedCompilation=false -nodeReuse:false "${P[@]}" > "$T/build-$d.log" 2>&1 || die "build $d"
  exe=$(grep -o "$d -> .*/$d.dll" "$T/build-$d.log" | tail -1 | sed "s#.* -> ##; s#\.dll\$##"); [ -x "$exe" ] || die "no apphost for $d"
  ln -sf "$exe" "$B/$d-cs" && touch "$B/.built-$d"
done

S="$KIT/samples"
[ -f "$R/parse/results.json" ] || { step "parse demo + corpus"; python3 -I "$KIT/compare.py" --native "$B/parsedemo-go" --converted "$B/parsedemo-cs" --samples "$S" --corpus "$T/corpus" --out "$R/parse" >> "$log" 2>&1; }
[ -f "$R/checkdemo.json" ] || { step "check demo"; python3 -I "$KIT/demo_compare.py" --name checkdemo --native "$B/checkdemo-go" --converted "$B/checkdemo-cs" --cwd "$S" --out "$R" -- check.ts good.ts >> "$log" 2>&1; }
[ -f "$R/emitdemo.json" ] || { step "emit demo"; python3 -I "$KIT/demo_compare.py" --name emitdemo --native "$B/emitdemo-go" --converted "$B/emitdemo-cs" --cwd "$S" --out "$R" -- good.ts bad.ts emit.ts >> "$log" 2>&1; }
[ -f "$R/mfdemo.json" ] || { step "multi-file"; python3 -I "$KIT/demo_compare.py" --name mfdemo --native "$B/mfdemo-go" --converted "$B/mfdemo-cs" --cwd "$S/projects" --out "$R" -- shop legacy >> "$log" 2>&1; }
for p in emit check; do
  [ -f "$R/$p-full/slice.json" ] && continue; step "full $p pass (resumable)"
  python3 -I "$KIT/typecheck_slice.py" --native "$B/${p}demo-go" --converted "$B/${p}demo-cs" --corpus "$T/corpus" --out "$R/$p-full" --every 1 --timeout 3600 >> "$T/$p-full.log" 2>&1
  [ -f "$R/$p-full/slice.json" ] || die "$p pass incomplete"
done
cp "$KIT/controls.tsv" "$R/controls.tsv" || die "controls.tsv (measured by hand on this box) missing from the kit"

step "stage + fill + census"
rm -rf "$T/stage" "$T/proof-report"; mkdir -p "$T/proof-report"
python3 -I "$KIT/layout.py" stage --tree "$OUT" --tsc "$MODC" --ts-root "$TS" --proof "$KIT" --dest "$T/stage" >> "$log" 2>&1 || die stage
python3 -I "$KIT/layout.py" fill --dest "$T/stage" --proof "$KIT" --results "$R" --converter "$sha" --gostdlib "${gsv:-local}" \
  --sdk "$(dotnet --version)" --platform "$(go env GOOS)/$(go env GOARCH)" --goversion "$(go version | cut -d' ' -f3)" ${OWNER_README:+--owner-readme "$OWNER_README"} >> "$log" 2>&1 || die fill
python3 -I "$KIT/layout.py" census --dest "$T/stage" ${DENY:+--deny "$DENY"} 2>&1 | tee -a "$log"; crc=${PIPESTATUS[0]}
step "done; census rc=$crc"; exit $crc
