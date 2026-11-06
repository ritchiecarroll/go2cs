#!/usr/bin/env bash
# pgates.sh -- item 7 pricing window, the DISASSEMBLY GATES (COORD-approved design, 2026-11-05/06), run BEFORE any timing.
# Inputs: price/<arm>/PerfTlsHandshake.{exe,pdb} for arms B0, A, AI (pulled and sha-checked by pricing.sh --pull).
# Gate 1 (cause 1, R's commit 1): golib slice<UIntPtr>.get_Item in B0 references GoZeroSizeFacts<UIntPtr>'s statics (the
#   control: the shape exists to be removed), and in A and AI it does NOT.
# Gate 2 (the inline, R's commit 2): bigmod addMulVVW calls math_bits Mul in B0 and A (control), and does NOT in AI. AI's
#   addMulVVW must carry no mulx (a default-ISA image); its calls to BigMul's software fallback are reported, not gated.
# Prints one line per check and GATES PASS / GATES FAIL; exit 0 only when every check shows its shape.
export DOTNET_ROOT='C:\Users\Admin\dotnet10' PATH="/c/Users/Admin/dotnet10:$PATH"
S=/c/go2cs/scratch/p1262; P=${PGATES_DIR:-$S/price}; F=$S/symfind/out/symfind.exe
D=$(ls -d "/c/Program Files/Microsoft Visual Studio/2022/Community/VC/Tools/MSVC/"*/bin/Hostx64/x64 | tail -1)
GI='golib_go_slice_1<UIntPtr>__get_Item_0'; AM=crypto_internal_fips140_bigmod_go_crypto_internal_fips140_bigmod_package__addMulVVW
fail=0
body() { # <arm> <symbol> -> disassembly of that symbol's extent into price/<arm>/dis-<tag>.txt, echo the path
  local X; X=$(cygpath -w $P/$1/PerfTlsHandshake.exe); local r; r=$("$F" "$X" "$(cygpath -w $P/$1)" "$2") || { echo "symfind FAILED $1 $2" >&2; return 1; }
  local a e; a=$(echo "$r" | cut -f2); e=$(echo "$r" | cut -f3)
  MSYS_NO_PATHCONV=1 "$D/dumpbin.exe" /nologo /disasm /range:$a,$e "$X" | grep -E "^  0" > "$P/$1/dis-$3.txt"; 
  # An identical-body-folded symbol is a one-instruction jmp stub: read the body it jumps to instead (the gate is vacuous otherwise).
  local t; t=$(grep -oE "jmp +[A-Za-z_][^ ]*$" "$P/$1/dis-$3.txt" | awk '{print $2}')
  if [ "$(wc -l < "$P/$1/dis-$3.txt")" = 1 ] && [ -n "$t" ]; then echo "info  $1 $2 is a folded stub -> $t" >&2; body $1 "$t" $3; return; fi
  echo "$P/$1/dis-$3.txt"; }
check() { # <label> <expect: yes|no> <count>
  local got=no; [ "$3" -gt 0 ] && got=yes
  if [ "$got" = "$2" ]; then echo "ok    $1 (expected $2, found $3)"; else echo "FAIL  $1 (expected $2, found $3)"; fail=1; fi; }
for arm in B0 A AI; do
  f=$(body $arm "$GI" get_Item) || { fail=1; continue; }
  n=$(grep -c "GoZeroSizeFacts" "$f"); exp=no; [ $arm = B0 ] && exp=yes
  check "$arm get_Item references GoZeroSizeFacts" $exp $n
  f=$(body $arm "$AM" addMulVVW) || { fail=1; continue; }
  n=$(grep -cE "call +math_bits_go_math_bits_package__Mul\b" "$f"); exp=yes; [ $arm = AI ] && exp=no
  check "$arm addMulVVW calls bits.Mul" $exp $n
  if [ $arm = AI ]; then n=$(grep -c "mulx" "$f"); check "AI addMulVVW has no mulx (default ISA)" no $n
    echo "info  AI addMulVVW calls to BigMul software fallback: $(grep -c "BigMul_g__SoftwareFallback" "$f") (0 can mean the fallback was inlined too)"; fi
done
[ $fail = 0 ] && echo "GATES PASS" || echo "GATES FAIL"; exit $fail
