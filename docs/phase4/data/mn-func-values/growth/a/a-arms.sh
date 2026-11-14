#!/usr/bin/env bash
# a-arms.sh : shape (a) (propagating + fix 1 over the rule's set + the callback-only opt-out), net/http then
# TypeScript, report mode then creation mode, one after another. Mints no verdict.
set -u
D=/c/go2cs/scratch/mn2/proto/b/n3census
O=/c/go2cs/scratch/mn2/proto/b/n3code
M=$O/ts/tsc-native
export GOROOT=C:/Users/<user>/sdk/go1.26.10
export PATH="/c/Users/<user>/sdk/go1.26.10/bin:$PATH"
REACH="-no-func-values -long-only -exclude-runtime"
A="-seed-func-calls -seed-factories -factories-arm -opt-out-callbacks"
CUT='(*fmt.pp).handleMethods,(*fmt.ss).ReadRune,(*fmt.readRune).ReadRune'

"$D/census3.exe" $REACH $A -top 15 -why 'sort.Slice,slices.SortFunc[[]int int],strings.Map,(*sync.Once).Do' net/http > "$O/a-nethttp-report.txt" 2>&1
echo "nethttp report rc=$?"
"$D/census3.exe" $REACH $A -creation net/http -creation-out "$O/a-nethttp.tsv" net/http > "$O/a-nethttp.txt" 2>&1
echo "nethttp creation rc=$?"
cd "$M" || exit 2
"$D/census3.exe" $REACH -cut "$CUT" $A -top 15 ./cmd/checkdemo > "$O/a-ts-report.txt" 2>&1
echo "ts report rc=$?"
"$D/census3.exe" $REACH -cut "$CUT" $A -creation github.com/microsoft/TypeScript/tsc -creation-out "$O/a-ts.tsv" ./cmd/checkdemo > "$O/a-ts.txt" 2>&1
echo "ts creation rc=$?"
echo "a arms done"
