#!/bin/bash
# The start-up A/B probe's driver. usage: driver.sh <scratch dir> <converter binary> <arm: A|B|L> <program: SuHello|SuHttp>
# The scratch dir holds a `src` from `git archive HEAD src`; the programs are converted into src/tests/Behavioral/<program>.
set -e
W=$1; CONV=$2; ARM=$3; PROG=$4; P=$(cd "$(dirname "$0")" && pwd)
SRC=$W/src; H=$SRC/tests/Behavioral/$PROG
export DOTNET_CLI_TELEMETRY_OPTOUT=1 DOTNET_NOLOGO=1 GOTOOLCHAIN=go1.24.13
if [ ! -f "$H/$PROG.csproj" ]; then
  mkdir -p "$H"; printf 'module go2cs/%s\n\ngo 1.23\n' "$PROG" > "$H/go.mod"
  [ "$PROG" = SuHello ] && cp "$P/hello.go.txt" "$H/$PROG.go" || cp "$P/nethttp.go.txt" "$H/$PROG.go"
  cp "$SRC/tests/Behavioral/AppendUntypedConst/go2cs.ico" "$H/"
  "$CONV" "$H" "$H"
fi
cp "$P/LiteralTable.cs" "$SRC/core/golib/LiteralTable.cs"
find "$SRC" -name zz_regprobe.g.cs -delete
if [ "$ARM" = A ]; then
  python3 "$P/operator-patch.py" "$SRC" revert
else
  python3 "$P/operator-patch.py" "$SRC" apply
  [ -f "$W/asms-$PROG.txt" ] || { echo "run arm A first: it lists the closure"; exit 2; }
  python3 "$P/genreg.py" "$SRC" "$W/asms-$PROG.txt" "$W/report-$PROG.tsv" $([ "$ARM" = L ] && echo lazy || echo eager)
fi
(cd "$H" && dotnet build -c Release -p:GoTargetOS=linux -p:go2csPath="$SRC/" -clp:ErrorsOnly)
rm -rf "$W/$ARM-$PROG"; mkdir -p "$W/$ARM-$PROG"; cp -r "$H/bin/Release/net10.0" "$W/$ARM-$PROG/jit"
[ "$ARM" = A ] && ls "$W/A-$PROG/jit"/*.dll | xargs -n1 basename | sed 's/\.dll$//' > "$W/asms-$PROG.txt"
echo "built $ARM $PROG (Release, net10.0, GoTargetOS=linux)"
