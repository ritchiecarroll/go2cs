#!/bin/bash
# ts-gen0.sh -- P3's cut 12: server GC as published vs DOTNET_GCgen0size=0x10000000 (256 MB gen0 budget), untraced walls,
# 3 each, alternating base first.
export PATH="/usr/bin:/bin:$PATH"
S="<scratch>"
PW=~/.dotnet/tools/pwsh.exe
for i in 1 2 3; do
  $PW -NoProfile -File "$S/ts-profile.frozen.ps1" -Tag g0base$i -NoTrace | tr -d '\r'
  $PW -NoProfile -File "$S/ts-profile.frozen.ps1" -Tag g0big$i -NoTrace -Env DOTNET_GCgen0size=0x10000000 | tr -d '\r'
done
echo GEN0-DONE
