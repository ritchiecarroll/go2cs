#!/usr/bin/env bash
# igates.sh -- the AOT indexer-inline seat's DISASSEMBLY GATES (COORD 2026-11-06), run BEFORE any timing.
# Arms P (master) and D (R's AggressiveInlining on slice<T>'s int/nint/ulong getters), from inl/<arm>/<Program>/ with PDBs.
# Hot functions: PerfSieve main.sieve, PerfMatMul main.matmul, PerfTlsHandshake bigmod.addMulVVW.
# A call counts as an INDEXER CALL when its target's name contains get_Item, OR when the target is a one-instruction
# jmp stub to such a name: identical-body folding can hide an indexer behind another symbol (§11.7's gate hazard).
# Gate: every hot function in P makes at least one indexer call (the control: the shape exists to be removed), and in D
# none does. Every call target of every hot function is printed, so a renamed indexer is visible to the reader too.
export DOTNET_ROOT='C:\Users\Admin\dotnet10' PATH="/c/Users/Admin/dotnet10:$PATH"
S=/c/go2cs/scratch/p1262; I=${IGATES_DIR:-$S/inl}; F=$S/symfind/out/symfind.exe
D=$(ls -d "/c/Program Files/Microsoft Visual Studio/2022/Community/VC/Tools/MSVC/"*/bin/Hostx64/x64 | tail -1)
fail=0
dis() { # <exe> <pdbdir> <symbol> -> disassembly lines of that symbol's extent on stdout
  local r; r=$("$F" "$(cygpath -w "$1")" "$(cygpath -w "$2")" "$3") || return 1
  MSYS_NO_PATHCONV=1 "$D/dumpbin.exe" /nologo /disasm /range:$(echo "$r" | cut -f2),$(echo "$r" | cut -f3) "$(cygpath -w "$1")" | grep -E "^  0"; }
for row in "PerfSieve PerfSieve_go_main_package__sieve" "PerfMatMul PerfMatMul_go_main_package__matmul" \
           "PerfTlsHandshake crypto_internal_fips140_bigmod_go_crypto_internal_fips140_bigmod_package__addMulVVW"; do
  set -- $row; prog=$1; fn=$2
  for arm in P D; do
    exe=$I/$arm/$prog/$prog.exe; pd=$I/$arm/$prog
    body=$(dis "$exe" "$pd" "$fn") || { echo "FAIL  $arm $prog: symfind could not find $fn"; fail=1; continue; }
    echo "$body" > "$I/$arm/$prog/dis-$(echo $fn | sed 's/.*__//').txt"
    n=0
    while read -r tgt; do
      [ -n "$tgt" ] || continue
      if echo "$tgt" | grep -q "get_Item"; then n=$((n + 1)); echo "      $arm $prog call $tgt  [indexer]"; continue; fi
      stub=$(dis "$exe" "$pd" "$tgt" 2>/dev/null)
      if [ "$(echo "$stub" | grep -c .)" = 1 ] && echo "$stub" | grep -qE "jmp +[^ ]*get_Item"; then
        n=$((n + 1)); echo "      $arm $prog call $tgt  [folded stub -> $(echo "$stub" | grep -oE '[^ ]*get_Item[^ ]*')]"
      else echo "      $arm $prog call $tgt"; fi
    done < <(echo "$body" | grep -oE "call +[A-Za-z_<][^ ]*" | awk '{print $2}' | sort | uniq)
    exp=yes; [ $arm = D ] && exp=no; got=no; [ $n -gt 0 ] && got=yes
    if [ "$got" = "$exp" ]; then echo "ok    $arm $prog $(echo $fn | sed 's/.*__//') makes indexer calls: $got (distinct targets $n)"
    else echo "FAIL  $arm $prog $(echo $fn | sed 's/.*__//') makes indexer calls: $got, expected $exp"; fail=1; fi
  done
done
[ $fail = 0 ] && echo "GATES PASS" || echo "GATES FAIL"; exit $fail
