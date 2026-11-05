#!/usr/bin/env bash
# Reproduces the scout corpus and its three readings (docs/PLAN-generated-code-format.md, section 1).
# Usage: collect.sh <worktree root> <scratch dir>. Linux, .NET SDK 10.0.403, GoTargetOS=linux.
# The builds write bin/obj/Generated under the worktree: purge them afterwards.
set -u
root=${1:?worktree root}; out=${2:?scratch dir}
here=$(cd "$(dirname "$0")" && pwd)
build() { timeout 7200 dotnet build "$1" -c Release -nodeReuse:false -p:go2csPath="$root/src/" -p:GoTargetOS=linux -p:EmitCompilerGeneratedFiles=true > "$out/build-$(basename "$1" .csproj).log" 2>&1; echo "$1 rc=$?"; }
build "$root/src/core/reflect/reflect.csproj"
for t in AnonymousStructs ChanDirectionChain MapCloneLinkname ConstraintProxyEmbeddedInterface InlineConstraintLift EmbeddedInterfaceWitness GenericPointerInterfaceImpl GenericInterfaceCrossPackage; do
  build "$root/src/tests/Behavioral/$t/$t.csproj"
done
# Copy every generated file, keeping its path under src/.
mkdir -p "$out/corpus" && (cd "$root/src" && find core tests -path '*/Generated/go2cs-gen/*' -name '*.g.cs' -exec cp --parents {} "$out/corpus/" \;)
cp "$root/src/.editorconfig" "$out/corpus/.editorconfig"
# Arm (a): dotnet format, on a copy.
cp -r "$out/corpus" "$out/fmtcheck"
dotnet format whitespace "$out/fmtcheck" --folder --include-generated --verify-no-changes --report "$out/fmtcheck/report.json"; echo "format rc=$?"
# Arms (b) and (c): genfmt.
dotnet build "$here/genfmt/genfmt.csproj" -c Release -o "$out/genfmt-bin" > /dev/null; echo "genfmt build rc=$?"
dotnet "$out/genfmt-bin/genfmt.dll" "$out/corpus" "$out/examples.tsv" > "$out/files.tsv"; echo "genfmt rc=$?"
python3 "$here/census.py" "$out/files.tsv" "$out/fmtcheck/report.json" > "$out/census-by-generator.tsv"
