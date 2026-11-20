#!/bin/bash
# ts-control.sh -- cut 8: server GC (as published) vs workstation concurrent GC (DOTNET_gcServer=0). Untraced walls, 3 each,
# alternating; then one gc-collect trace each for the GC share.
export PATH="/usr/bin:/bin:$PATH"
S="<scratch>"
PW=~/.dotnet/tools/pwsh.exe
for i in 1 2 3; do
  $PW -NoProfile -File "$S/ts-profile.frozen.ps1" -Tag srv-wall$i -NoTrace | tr -d '\r'
  $PW -NoProfile -File "$S/ts-profile.frozen.ps1" -Tag wks-wall$i -NoTrace -Env DOTNET_gcServer=0 | tr -d '\r'
done
$PW -NoProfile -File "$S/ts-profile.frozen.ps1" -Tag srv-gc -Profiles gc-collect | tr -d '\r'
$PW -NoProfile -File "$S/ts-profile.frozen.ps1" -Tag wks-gc -Profiles gc-collect -Env DOTNET_gcServer=0 | tr -d '\r'
echo CONTROL-DONE
