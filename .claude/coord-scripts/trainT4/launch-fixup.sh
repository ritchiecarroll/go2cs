#!/usr/bin/env bash
# TRAIN T4 run2, fixup launch B: the exact path file (T4-12) built from run2's OWN drift (byte-identical to run1's list:
# 1,548 paths, sha256 268702927ca7), the proof re-run on run2's emission (drift-tokens-report.txt, drift-classes.txt).
# Step 4 RE-USES launch A's kept emission in tT4emitr2 (same head b258fe81db, same base). Run1's launch form: GOROOT /
# GOTOOLCHAIN only, the ambient PATH, DOTNET_ROOT unset.
R=/h/go2cs-tmp-coord/coord-scratch/tT4/run2
G=$R/go
export GOROOT="${USERPROFILE:?}\\sdk\\go1.24.13" GOTOOLCHAIN=local   # handover copy: the profile is read from the environment (the as-run copy spelled it literally)
unset DOTNET_ROOT
cd "$G" || exit 1
BASE=226afb810a21408ebda2412dd74d526345c46f65 EXPECT_HEAD=b258fe81db94baf80798e5781dc85688fc2c6bb1 FIXUP_N=1 \
REGEN_ALLOW_FILE="$R/tT4-regen-allow-row2.txt" \
REGEN_ALLOW_NOTE="REGEN_ALLOW: row 2 comment-parity footprint; comment/whitespace-only plus two named code classes (dead alias drop 11ef3a1207, lambda expression body 58413a5f57), classified OTHER=0 by tT4-drift-classify.py (run2: includes the var/const doc-gap fix 07cacea829)" \
bash ./tT4-fixup.sh > "$R/fixup.console.log" 2>&1
rc=$?
echo "fixup rc=$rc" >> "$R/fixup.console.log"
exit $rc
