#!/usr/bin/env bash
# Run a command and PROVE which libcoreclr.so every process it starts loads: strace -f --seccomp-bpf (only openat
# stops a process), every successful openat of libcoreclr.so, short-lived processes included. PASS only when each
# loaded copy's sha256 equals the Microsoft runtime recorded by use-ms-dotnet.sh AND it links no libunwind. Exit: the command's own rc if it
# failed, else 0 PASS / 9 FAIL.
#   bash verify-ms-dotnet.sh dotnet test src/tests/GolibTests -c Release
set -uo pipefail
VER=10.0.12
DST=${DOTNET_MS_ROOT:-$HOME/.dotnet-ms}
WANT=$(cat "$DST/.ms-runtime-$VER" 2>/dev/null) || { echo "VERIFY FAIL: no Microsoft runtime recorded at $DST (run use-ms-dotnet.sh)"; exit 9; }
LOG=$(mktemp)
strace -f --seccomp-bpf -qq -e trace=openat -e signal=none -o "$LOG" "$@"; rc=$?
mapfile -t LOADED < <(grep -E 'openat\(.*libcoreclr\.so", [^)]*\) = [0-9]+' "$LOG" | sed -E 's/.*openat\([^"]*"([^"]*libcoreclr\.so)".*/\1/' | sort -u)
rm -f "$LOG"
[ $rc -ne 0 ] && echo "command rc=$rc"
[ ${#LOADED[@]} -gt 0 ] || { echo "VERIFY FAIL: no process loaded libcoreclr.so, so nothing was proven"; exit 9; }
bad=0
for p in "${LOADED[@]}"; do
  real=$(readlink -f "$p"); got=$(sha256sum "$real" | cut -d' ' -f1); uw=$(ldd "$real" | grep -c unwind)
  if [ "$got" = "$WANT" ] && [ "$uw" -eq 0 ]; then echo "  MICROSOFT $real sha256 ${got:0:16} libunwind-links 0"
  else echo "  NOT MICROSOFT $real sha256 ${got:0:16} libunwind-links $uw"; bad=1; fi
done
[ $bad -eq 0 ] && echo "VERIFY PASS: ${#LOADED[@]} libcoreclr path(s), all Microsoft's $VER" || echo "VERIFY FAIL: a process loaded a non-Microsoft runtime"
[ $rc -ne 0 ] && exit $rc
[ $bad -eq 0 ] && exit 0 || exit 9
