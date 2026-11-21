#!/bin/bash
# ts-c7-battery.sh -- candidate 7: A (Release build) / B (publish, golib+stdlib R2R) / C (B + GoReadyToRunUserCode=true), the
# item-0 phase-timed copy, 101 inputs, runner launch, 3 rounds alternating (A B C, C B A, A B C).
export PATH="/usr/bin:/bin:$PATH"
S="<scratch>"
PW=~/.dotnet/tools/pwsh.exe
declare -A X=([A]='C:\go2cs-tmp\ts-prof\timed-bin\checkdemo.exe' [B]='C:\go2cs-tmp\ts-prof\pub-B\checkdemo.exe' [C]='C:\go2cs-tmp\ts-prof\pub-C\checkdemo.exe')
for order in "A B C" "C B A" "A B C"; do r=$((r+1)); for arm in $order; do
  $PW -NoProfile -File "$S/ts-phase.frozen.ps1" -Tag c7-$arm-r$r -Exe "${X[$arm]}" | tr -d '\r'
done; done
echo C7-DONE
