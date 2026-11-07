#!/usr/bin/env bash
# ibuild2-launch.sh -- the indexer-inline RE-WINDOW's rebuild (COORD 2026-11-06, after the Debug-stdlib root cause): every JIT
# and R2R arm rebuilt by naming the csproj explicitly (ibuild.sh / rbuild.sh, patched), then GATED by asmflags at 0 Debug
# assemblies per closure. ibuild.sh's template R2R step still fails IL1034 (golib's publish guard); rbuild.sh replaces it
# with the accepted fd1 shape and rewrites the manifests. Writes inl-src/asmgate.txt and inl-src/rebuild.done (rc) last.
S=/c/go2cs/scratch/p1262; O=$S/inl-src
rm -f "$O/rebuild.done" "$O/finished"
bash "$S/ibuild.sh" 53b3eb88d7 da3f68520d > "$O/launch2.out" 2>&1; echo "ibuild rc=$?" > "$O/asmgate.txt"
bash "$S/rbuild.sh" >> "$O/launch2.out" 2>&1; echo "rbuild rc=$?" >> "$O/asmgate.txt"
export DOTNET_ROOT='C:\Users\Admin\dotnet10' PATH="/c/Users/Admin/dotnet10:$PATH"
bad=0
for d in P/PerfTlsHandshake/jit D/PerfTlsHandshake/jit P/PerfSieve/jit D/PerfSieve/jit P/PerfMatMul/jit D/PerfMatMul/jit P/PerfStartup/r2r D/PerfStartup/r2r; do
  out=$("$S/asmflags/out/asmflags.exe" $(ls "$O/$d"/*.dll | grep -vE "/(Microsoft|System|netstandard|mscorlib)" | xargs -I{} cygpath -w {}))
  n=$(echo "$out" | grep -c .); off=$(echo "$out" | grep -c 'jitopt=OFF'); r2r=$(echo "$out" | grep -c 'r2r=yes')
  echo "$d assemblies=$n debug=$off r2r=$r2r" >> "$O/asmgate.txt"; [ "$off" = 0 ] && [ "$n" -gt 0 ] || bad=1
done
[ $bad = 0 ] && echo "ASMGATE PASS" >> "$O/asmgate.txt" || echo "ASMGATE FAIL" >> "$O/asmgate.txt"
echo $bad > "$O/rebuild.done"
