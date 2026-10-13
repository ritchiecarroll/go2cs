#!/usr/bin/env bash
# TRAIN T4 run2, fixup launch A (the MEASUREMENT launch): no REGEN_ALLOW_FILE, so step 4 runs a FRESH emission check at
# the run2 union head and is PREDICTED to stop at the regen check (nothing written; the emission kept in tT4emitr2) --
# the route run1 took (abort2). The drift then gets the proof and a NEW exact path file; launch B re-uses the emission.
# Run1's successful launch form: GOROOT / GOTOOLCHAIN only, the AMBIENT PATH, DOTNET_ROOT unset (dotnet10 NOT put first).
R=/h/go2cs-tmp-coord/coord-scratch/tT4/run2
G=$R/go
export GOROOT="${USERPROFILE:?}\\sdk\\go1.24.13" GOTOOLCHAIN=local   # handover copy: the profile is read from the environment (the as-run copy spelled it literally)
unset DOTNET_ROOT
cd "$G" || exit 1
BASE=226afb810a21408ebda2412dd74d526345c46f65 EXPECT_HEAD=b258fe81db94baf80798e5781dc85688fc2c6bb1 FIXUP_N=1 \
bash ./tT4-fixup.sh > "$R/fixup.console.measure.log" 2>&1
rc=$?
echo "fixup rc=$rc" >> "$R/fixup.console.measure.log"
exit $rc
