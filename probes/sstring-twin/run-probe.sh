#!/usr/bin/env bash
# run-probe.sh ARM -- one arm of the sstring twin compile probe.
#   pass 1: every form defined; the compiler's error codes are mapped to forms by the #if FORM_x
#           block each error line falls in.
#   pass 2: only the forms that compiled; the exe prints which overload each one bound.
# Build parallelism is capped (-m:2) under the i9 hardware order.
export PATH="/c/Program Files/dotnet:/usr/bin:/mingw64/bin:$PATH"
export DOTNET_ROOT='C:\Program Files\dotnet'
ARM="$1"
[ -n "$ARM" ] || { echo "usage: run-probe.sh ARM"; exit 2; }
HERE="$(cd "$(dirname "$0")" && pwd)"
OUT="$HERE/results/$ARM"
mkdir -p "$OUT"
cd "$HERE" || exit 2

ALL="FORM_A FORM_B FORM_C FORM_D FORM_E FORM_E2 FORM_E3 FORM_E4 FORM_F FORM_G FORM_H FORM_N1 FORM_N2 FORM_N3"

write_props() {
  printf '<Project>\n  <PropertyGroup>\n    <ProbeForms>%s</ProbeForms>\n  </PropertyGroup>\n</Project>\n' "$(echo $1 | tr ' ' ';')" > forms.props
}

# pass 1
write_props "$ALL"
dotnet build Probe.csproj -m:2 -nologo -v:q -clp:NoSummary > "$OUT/pass1.log" 2>&1
echo "pass1 rc=$?" > "$OUT/summary.txt"

# map each error to the form whose #if block holds its line
grep -oE 'Forms\.cs\(([0-9]+),[0-9]+\): error CS[0-9]+' "$OUT/pass1.log" | sort -u |
while IFS= read -r e; do
  ln=$(echo "$e" | sed -E 's/.*\(([0-9]+),.*/\1/')
  code=$(echo "$e" | grep -oE 'CS[0-9]+')
  form=$(awk -v N="$ln" 'NR<=N && /^#if FORM_/{f=$2} NR==N{print f}' Forms.cs)
  echo "$form $code line=$ln"
done | sort -u > "$OUT/errors.txt"
other=$(grep -E 'error ' "$OUT/pass1.log" | grep -vc 'Forms\.cs(')
echo "errors outside Forms.cs: $other" >> "$OUT/summary.txt"

# pass 2: only the forms with no error
OK=""
for f in $ALL; do grep -q "^$f " "$OUT/errors.txt" || OK="$OK $f"; done
echo "pass2 forms:$OK" >> "$OUT/summary.txt"
write_props "$OK"
dotnet build Probe.csproj -m:2 -nologo -v:q -clp:NoSummary > "$OUT/pass2.log" 2>&1
rc=$?
echo "pass2 rc=$rc" >> "$OUT/summary.txt"
if [ $rc -eq 0 ]; then
  dotnet bin/Debug/net10.0/Probe.dll > "$OUT/run.txt" 2>&1
  echo "run rc=$?" >> "$OUT/summary.txt"
fi
rm -f forms.props
cat "$OUT/summary.txt" "$OUT/errors.txt"
[ -f "$OUT/run.txt" ] && cat "$OUT/run.txt"
