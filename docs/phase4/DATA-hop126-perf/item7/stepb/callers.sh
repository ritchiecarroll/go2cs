#!/usr/bin/env bash
# callers.sh -- Step B: does the hot caller addMulVVW still CALL math/bits.Mul in each image? Disassembles addMulVVW by its
# PDB extent (symfind) and counts its call targets, naming the image's own bits.Mul address.
export DOTNET_ROOT='C:\Users\Admin\dotnet10' PATH="/c/Users/Admin/dotnet10:$PATH"
S=/c/go2cs/scratch/p1262; F=$S/symfind/out/symfind.exe
D=$(ls -d "/c/Program Files/Microsoft Visual Studio/2022/Community/VC/Tools/MSVC/"*/bin/Hostx64/x64 | tail -1)
n=crypto_internal_fips140_bigmod_go_crypto_internal_fips140_bigmod_package__addMulVVW
for img in v3 trace/aot; do
  X=$(cygpath -w $S/$img/PerfTlsHandshake.exe); P=$(cygpath -w $S/$img); tag=$(basename $img)
  r=$("$F" "$X" "$P" $n) || { echo "$img: symfind failed for addMulVVW"; exit 2; }
  m=$("$F" "$X" "$P" math_bits_go_math_bits_package__Mul | cut -f2) || { echo "$img: symfind failed for Mul"; exit 2; }
  a=$(echo "$r" | cut -f2); e=$(echo "$r" | cut -f3)
  MSYS_NO_PATHCONV=1 "$D/dumpbin.exe" /nologo /disasm /range:$a,$e "$X" | grep -E "^  0" > $S/v3/dis-$tag-addMulVVW.txt
  echo "$img addMulVVW $a size=$(echo "$r" | cut -f4) instr=$(wc -l < $S/v3/dis-$tag-addMulVVW.txt) mulx=$(grep -c mulx $S/v3/dis-$tag-addMulVVW.txt) bits.Mul=$m"
  grep -oE "call +[^ ]+" $S/v3/dis-$tag-addMulVVW.txt | awk '{print $2}' | sort | uniq -c
done
