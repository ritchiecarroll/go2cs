#!/usr/bin/env bash
# ts-arms.sh : the TS census arms (base report, then creation arms). Mints no verdict.
set -u
D=/c/go2cs/scratch/mn2/proto/b/n3census
O=/c/go2cs/scratch/mn2/proto/b/n3code
M=$O/ts/tsc-native
export GOROOT=C:/Users/<user>/sdk/go1.26.10
export PATH="/c/Users/<user>/sdk/go1.26.10/bin:$PATH"
CUT='(*fmt.pp).handleMethods,(*fmt.ss).ReadRune,(*fmt.readRune).ReadRune'
REACH="-no-func-values -long-only -exclude-runtime"
PREFIX=github.com/microsoft/TypeScript/tsc
cd "$M" || exit 2
"$D/census2.exe" $REACH -cut "$CUT" -top 10 -json "$O/ts-report-base.json" ./cmd/checkdemo > "$O/ts-report-base.txt" 2>&1; echo "report-base rc=$?"
"$D/census2.exe" $REACH -cut "$CUT" -seed-func-calls -seed-factories -factories-arm -top 10 -json "$O/ts-report-prop-fix1a.json" ./cmd/checkdemo > "$O/ts-report-prop-fix1a.txt" 2>&1; echo "report-prop rc=$?"
for spec in "base|" "leaf-fix1a|-seed-func-calls -seed-leaf-only -seed-factories -factories-arm" "prop-fix1a|-seed-func-calls -seed-factories -factories-arm"; do
    arm=${spec%%|*}
    flags=${spec#*|}
    "$D/census2.exe" $REACH -cut "$CUT" $flags -creation "$PREFIX" -creation-out "$O/ts-$arm.tsv" ./cmd/checkdemo > "$O/ts-$arm.txt" 2>&1
    echo "$arm rc=$?"
done
echo "ts arms done"
