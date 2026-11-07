#!/usr/bin/env bash
# ibuild.sh <P-sha> <D-sha> -- the indexer-inline seat's JIT and R2R arms, SOURCE-BUILT on G (COORD option b, 2026-11-06).
# NOT timed; runs outside any window. R's AOT images are package-built by the drop recipe; these arms are source-built:
# go2cs -recurse with -go2cspath at each commit's worktree (local project references to that commit's src/core). The axis is
# the same (P = base, D = R's slice.cs change); the build shape is named in the record.
#   JIT: TlsHandshake, Sieve, MatMul -> dotnet build -c Release.     R2R: Startup -> dotnet publish -c Release -r win-x64.
# Gates: P and D differ ONLY in src/core/golib/slice.cs among the paths that reach a binary (src/core, src/go2cs); one
# converter, built at P, with D's src/go2cs tree hash equal to P's. Writes inl-src/<arm>/<prog>/{jit|r2r}/ and a
# manifest per arm (identity + sha256 of every program dll/exe), inl-src/build.log, inl-src/finished last.
set -u
S=/c/go2cs/scratch/p1262; O=$S/inl-src; L=$O/build.log; P=${1:?P sha}; Dv=${2:?D sha}
W=/c/go2cs/abw; REPO=/c/Projects/go2cs
export DOTNET_ROOT='C:\Users\Admin\dotnet10' PATH="/c/Users/Admin/sdk/go1.26.10/bin:/c/Users/Admin/dotnet10:$PATH"
export GOTOOLCHAIN=local CGO_ENABLED=0 MSBUILDDISABLENODEREUSE=1 DOTNET_CLI_USE_MSBUILD_SERVER=0 UseSharedCompilation=false
unset GOROOT GOFLAGS
mkdir -p "$O"; echo "start $(date '+%F %T') P=$P D=$Dv dotnet=$(dotnet --version) go=$(go version | cut -d' ' -f3)" > "$L"
fin() { echo "end $(date '+%F %T') rc=$1" >> "$L"; echo "$1" > "$O/finished"; exit "$1"; }
git -C "$REPO" fetch -q origin || { echo "fetch failed" >> "$L"; fin 3; }
ch=$(git -C "$REPO" diff --name-only "$P" "$Dv" -- src/core src/go2cs)
echo "binary-reaching diff P..D: $(echo "$ch" | tr '\n' ' ')" >> "$L"
echo "all paths P..D: $(git -C "$REPO" diff --name-only "$P" "$Dv" | tr '\n' ' ')" >> "$L"
[ "$ch" = "src/core/golib/slice.cs" ] || { echo "GATE FAIL: P and D differ beyond src/core/golib/slice.cs" >> "$L"; fin 5; }
[ "$(git -C "$REPO" rev-parse "$P:src/go2cs")" = "$(git -C "$REPO" rev-parse "$Dv:src/go2cs")" ] || { echo "GATE FAIL: converter trees differ" >> "$L"; fin 5; }
for arm in P D; do sha=$P; [ $arm = D ] && sha=$Dv
  t=$W/g-inl-$(echo $arm | tr A-Z a-z)
  [ -d "$t" ] || git -C "$REPO" worktree add -q --detach "$t" "$sha" || { echo "worktree $arm failed" >> "$L"; fin 3; }
  [ "$(git -C "$t" rev-parse HEAD)" = "$(git -C "$REPO" rev-parse "$sha")" ] || { echo "worktree $arm not at $sha" >> "$L"; fin 3; }
  echo "$arm tree $t HEAD $(git -C "$t" rev-parse HEAD) golib $(git -C "$t" rev-parse HEAD:src/core/golib)" >> "$L"
done
CONV=$O/go2cs-P.exe
( cd $W/g-inl-p/src/go2cs && go build -trimpath -o "$(cygpath -w "$CONV")" . ) >> "$L" 2>&1 || { echo "converter build failed" >> "$L"; fin 3; }
echo "converter $(sha256sum "$CONV" | cut -c1-16)" >> "$L"
fail=0
for row in "PerfTlsHandshake jit" "PerfSieve jit" "PerfMatMul jit" "PerfStartup r2r"; do set -- $row; p=$1; kind=$2
  for arm in P D; do t=$W/g-inl-$(echo $arm | tr A-Z a-z); w=$O/tmp/$arm/$p; rm -rf "$w"; mkdir -p "$w/src" "$w/out"
    cp "$t/src/tests/Performance/$p/"*.go "$t/src/tests/Performance/$p/go.mod" "$w/src/"
    ( cd "$w/src" && "$CONV" -recurse "-go2cspath=$(cygpath -w "$t/src")" "$(cygpath -w "$w/src")" "$(cygpath -w "$w/out")" ) >> "$L" 2>&1; rc=$?
    pd=$(dirname "$(find "$w/out" -name "$p.csproj" | head -1)")
    [ $rc -eq 0 ] && [ -d "$pd" ] || { echo "$arm/$p CONVERT FAILED rc=$rc" >> "$L"; fail=1; continue; }
    dst=$O/$arm/$p/$kind; rm -rf "$dst"
    if [ $kind = jit ]; then ( cd "$pd" && dotnet build "$p.csproj" -nologo -clp:ErrorsOnly -c Release "-p:OutDir=$(cygpath -w "$dst")\\" ) >> "$L" 2>&1; rb=$?
    else ( cd "$pd" && dotnet publish "$p.csproj" -nologo -clp:ErrorsOnly -c Release -r win-x64 -o "$(cygpath -w "$dst")" ) >> "$L" 2>&1; rb=$?; fi
    echo "$arm/$p $kind rc=$rb exe $(sha256sum "$dst/$p.exe" 2>/dev/null | cut -c1-16) dll $(sha256sum "$dst/$p.dll" 2>/dev/null | cut -c1-16) golib $(sha256sum "$dst/golib.dll" 2>/dev/null | cut -c1-16)" >> "$L"
    [ $rb -eq 0 ] && [ -f "$dst/$p.exe" ] || fail=1
  done
done
for arm in P D; do t=$W/g-inl-$(echo $arm | tr A-Z a-z)
  { echo "# inline seat arm $arm (source-built on G): HEAD $(git -C "$t" rev-parse HEAD), golib tree $(git -C "$t" rev-parse HEAD:src/core/golib), converter $(sha256sum "$CONV" | cut -c1-64)"
    ( cd "$O/$arm" && find . -name '*.dll' -o -name '*.exe' | sort | xargs sha256sum ); } > "$O/manifest-$arm.sha256"
done
rm -rf "$O/tmp"; fin $fail
