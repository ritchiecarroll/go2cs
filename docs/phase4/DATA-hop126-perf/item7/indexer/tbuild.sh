#!/usr/bin/env bash
# tbuild.sh -- the indexer-inline re-window's TEMPLATE-shape R2R Startup arms (explicit csproj; `dotnet publish PerfStartup.csproj -c Release -r win-x64`, the converter template's own shape), beside the fd1 arms. NOT timed.
# It builds now because the csproj is named: the earlier IL1034 came from a bare folder publish building the generated .slnx, which
# publishes golib.csproj as a project of its own (G's correction to COORD, 2026-11-06), not from the template shape itself.
set -u
S=/c/go2cs/scratch/p1262; O=$S/inl-src; L=$O/tbuild.log; W=/c/go2cs/abw; CONV=$O/go2cs-P.exe
export DOTNET_ROOT='C:\Users\Admin\dotnet10' PATH="/c/Users/Admin/sdk/go1.26.10/bin:/c/Users/Admin/dotnet10:$PATH"
export GOTOOLCHAIN=local CGO_ENABLED=0 MSBUILDDISABLENODEREUSE=1 DOTNET_CLI_USE_MSBUILD_SERVER=0 UseSharedCompilation=false
unset GOROOT GOFLAGS
echo "start $(date '+%F %T') converter $(sha256sum "$CONV" | cut -c1-16)" > "$L"; fail=0; p=PerfStartup
for arm in P D; do t=$W/g-inl-$(echo $arm | tr A-Z a-z); w=$O/tmp/$arm/$p; rm -rf "$w"; mkdir -p "$w/src" "$w/out"
  cp "$t/src/tests/Performance/$p/"*.go "$t/src/tests/Performance/$p/go.mod" "$w/src/"
  ( cd "$w/src" && "$CONV" -recurse "-go2cspath=$(cygpath -w "$t/src")" "$(cygpath -w "$w/src")" "$(cygpath -w "$w/out")" ) >> "$L" 2>&1 || { echo "$arm convert FAILED" >> "$L"; fail=1; continue; }
  pd=$(dirname "$(find "$w/out" -name "$p.csproj" | head -1)"); dst=$O/$arm/$p/r2r-tmpl; rm -rf "$dst"
  ( cd "$pd" && dotnet publish "$p.csproj" -nologo -clp:ErrorsOnly -c Release -r win-x64 -o "$(cygpath -w "$dst")" ) >> "$L" 2>&1; rb=$?
  echo "$arm/$p r2r-template rc=$rb exe $(sha256sum "$dst/$p.exe" 2>/dev/null | cut -c1-16) golib $(sha256sum "$dst/golib.dll" 2>/dev/null | cut -c1-16)" >> "$L"
  [ $rb -eq 0 ] && [ -f "$dst/$p.exe" ] || fail=1
done
for arm in P D; do t=$W/g-inl-$(echo $arm | tr A-Z a-z)
  { echo "# inline seat arm $arm (source-built on G, explicit csproj; JIT build; R2R fd1 in r2r/, template in r2r-tmpl/): HEAD $(git -C "$t" rev-parse HEAD), golib tree $(git -C "$t" rev-parse HEAD:src/core/golib), converter $(sha256sum "$CONV" | cut -c1-64)"
    ( cd "$O/$arm" && find . -name '*.dll' -o -name '*.exe' | sort | xargs sha256sum ); } > "$O/manifest-$arm.sha256"
done
rm -rf "$O/tmp"; echo "end $(date '+%F %T') rc=$fail" >> "$L"; exit $fail
