#!/usr/bin/env bash
# passb_patch.sh <tree> : hand-patch C1 (11 lines / 7 files) and C2 (1 line) in a COPY of the -recurse output, the way the
# converter emits the same shapes elsewhere (same-package: NжFail_n + GoImplement; cross-package: <T>ж<Iface> + qualified GoImplement).
set -e
T="$1/src/github.com/microsoft/TypeScript/tsc/internal"
D='go.github.com.microsoft.TypeScript.tsc.@internal.debug_package.FailBadSyntaxKind_node'
# assembly attributes must precede types: insert right after the first 'using static go.' line
addattr() { awk -v a="$2" '{print} !d && /^using static go\./ {print a; d=1}' "$1" > "$1.tmp" && mv "$1.tmp" "$1"; grep -qF "$2" "$1" || { echo "attr not placed in $1"; exit 1; }; }
A='go.github.com.microsoft.TypeScript.tsc.@internal.ast_package.Node'
sed -i 's/new Nodeжinterface{KindString() string}(/new NodeжFailBadSyntaxKind_node(/' "$T/ast/utilities.cs"
addattr "$T/ast/package_info.cs" "[assembly: GoImplement<Node, $D>(Pointer = true)]"
for f in ls/importTracker.cs ls/inlay_hints.cs pseudochecker/lookup.cs scanner/utilities.cs transformers/estransforms/classfields.cs transformers/tstransforms/typeserializer.cs; do
  sed -i 's/new ast_Nodeжinterface{KindString() string}(/new ast_NodeжFailBadSyntaxKind_node(/g' "$T/$f"
  pi="$T/$(dirname $f)/package_info.cs"; grep -qF "$D" "$pi" || addattr "$pi" "[assembly: GoImplement<$A, $D>(Pointer = true)]"
done
sed -i 's|new collections.Set<struct{name string; kind github.com/microsoft/TypeScript/tsc/internal/lsp/lsproto.SymbolKind; rng github.com/microsoft/TypeScript/tsc/internal/lsp/lsproto.Range}>()|new collections.Set<ProvideDocumentSymbols_key>()|' "$T/ls/symbols.cs"
left=$(grep -rlE "interface\{KindString|struct\{name string" --include=*.cs "$T" | wc -l); echo "raw-Go sites left: $left"
