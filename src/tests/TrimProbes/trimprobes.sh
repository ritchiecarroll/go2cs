#!/bin/bash
# trimprobes.sh -- publish the trim probes against a go2cs source tree, run each, and compare its output with Go's.
#
#   usage: trimprobes.sh <go2cs src dir> [arm] [rid]
#     <go2cs src dir>  the tree under test's src directory (the one holding core/ and gen/), absolute path
#     arm              default (the template-default `dotnet publish`: ILLink partial trim, ReadyToRun, no AOT; the
#                      consumer's D arm mirrors it) | F (Native AOT, full trim) | P (Native AOT, partial trim; consumer,
#                      genprobe and c32a only -- slow: hours per program)
#     rid              default: win-x64 on Windows, linux-x64 on Linux, osx-arm64 / osx-x64 on macOS
#
# Prints one tab-separated row per program (program, arm, publish rc, run exit, verdict, executable bytes) and exits 1
# if any program fails its expectation for the arm. Expectations: under the default arm every program publishes
# (rc 0) and runs: consumer prints its PACKAGE-SYMBOLS line with empty stderr, the other four equal expected.txt (Go's
# own output). Under F, genprobe stops by design at family E (GoReflect.buildFieldAccessor, exit 2), so its verdict is
# reported, not judged; c32a, c32b and c32c are judged against expected.txt.
set -u
SRC=${1:?usage: trimprobes.sh <go2cs src dir> [arm] [rid]}; ARM=${2:-default}; RID=${3:-}
HERE=$(cd "$(dirname "$0")" && pwd)
case "$(uname -s)" in
  MINGW*|MSYS*|CYGWIN*) RID=${RID:-win-x64}; EXE=.exe; SRCARG=$(cygpath -w "$SRC") ;;
  Linux*) RID=${RID:-linux-x64}; EXE=; SRCARG=$SRC ;;
  Darwin*) RID=${RID:-$( [ "$(uname -m)" = arm64 ] && echo osx-arm64 || echo osx-x64 )}; EXE=; SRCARG=$SRC ;;
esac
[ -d "$SRC/core/golib" ] || { echo "not a go2cs src dir: $SRC" >&2; exit 2; }
OUT=$HERE/out-$ARM-$RID; rm -rf "${OUT:?}"; mkdir -p "$OUT"
fail=0
for p in consumer genprobe c32a c32b c32c; do
  dir=$HERE/$p; proj=$(ls "$dir"/*.csproj | head -1); name=$(sed -n 's#.*<AssemblyName>\(.*\)</AssemblyName>.*#\1#p' "$proj" | head -1)
  [ -n "$name" ] || name=$(basename "$proj" .csproj)
  case $ARM in
    default) armprop=; [ $p = consumer ] && armprop=-p:FtArm=D ;;
    F|P) armprop=-p:FtArm=$ARM; grep -q "'\$(FtArm)'=='$ARM'" "$proj" || { printf '%s\t%s\t-\t-\tno %s arm\t-\n' $p $ARM $ARM; continue; } ;;
    *) echo "unknown arm $ARM" >&2; exit 2 ;;
  esac
  rm -rf "$dir/bin" "$dir/obj" "$dir/Generated"
  (cd "$dir" && dotnet publish "$(basename "$proj")" -c Release -r "$RID" -nologo -v:m -clp:NoSummary "-p:go2csPath=$SRCARG/" $armprop -o "$OUT/$p" > "$OUT/publish-$p.log" 2>&1); rc=$?
  exe=$OUT/$p/$name$EXE; x=-; verdict=NO-EXECUTABLE; size=-
  if [ $rc = 0 ] && [ -f "$exe" ]; then
    size=$(wc -c < "$exe" | tr -d ' ')
    (cd "$OUT/$p" && "$exe" > "$OUT/run-$p.out" 2> "$OUT/run-$p.err"); x=$?
    if [ $p = consumer ]; then
      tr -d '\r' < "$OUT/run-$p.out" | grep -qE '^PACKAGE-SYMBOLS: ' && [ ! -s "$OUT/run-$p.err" ] && verdict=RUNS-CLEAN || verdict=DIFFERS
    # CR stripped on BOTH sides: a core.autocrlf=true checkout writes expected.txt with CRLF (.gitattributes pins it to
    # LF as well), and a Windows program writes CRLF.
    elif diff -q <(tr -d '\r' < "$OUT/run-$p.out") <(tr -d '\r' < "$dir/expected.txt") > /dev/null; then verdict=GO-EQUAL
    else verdict="DIFFERS($(grep -m1 -oE '[A-Za-z]+Exception' "$OUT/run-$p.err"))"; fi
  fi
  printf '%s\t%s\t%s\t%s\t%s\t%s\n' $p $ARM $rc "$x" "$verdict" "$size"
  judged=1; [ $ARM = F ] && [ $p = genprobe ] && judged=0
  [ $judged = 1 ] && case $verdict in GO-EQUAL|RUNS-CLEAN) ;; *) fail=1 ;; esac
  rm -rf "$dir/bin" "$dir/obj" "$dir/Generated"
done
exit $fail
