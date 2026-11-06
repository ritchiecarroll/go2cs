The probe is a scratch copy of src/tests/Performance/PerfIfaceShell (its .csproj, go.mod and package_info.cs, the
assembly renamed P1AotProbe) placed at src/tests/Performance/_p1aot_Probe and deleted after the seat. Only Main
differs from the converted PerfIfaceShell.cs: P1AotProbe.cs here is the whole file as run.
build.sh: JIT Release build and Native AOT publish (-p:PerfAot=true, the Performance suite's gate).
run.sh: 1 warm-up + 24 launches; wall from the shell, the P1PROBE line from the program.
marks.patch.py: the scratch phase timers (never committed), applied to golib, built, run 6 times + 1 warm-up
with P1MARK_OUT set, then reverted with git checkout.
