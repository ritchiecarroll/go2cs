# TRAIN 48 -- LAUNCH WRAPPER FOR RUN 1.  Derived from train 47's launch-run4.sh, same shape.
# ⚠ (1) THE SWITCH IS **EXPORTED**, NOT ONE-SHOT PREFIXED.  A `nohup`/`start` layer can drop a one-shot
#       VAR=x prefix between the assignment and the child, and the run would then proceed as a PARTIAL
#       train while reading as a landing.  ⚠ AND IT IS `TRAIN48_REQUIRE_ALL`, NEVER the un-prefixed
#       `REQUIRE_ALL`: that name is the assembly's own internal variable and is assigned
#       unconditionally, so exporting it would configure NOTHING.  The assembly refuses an inherited
#       un-prefixed spelling by name rather than overwriting it silently.
# ⚠ (2) THE LAST STATEMENT IS `exit $rc`.  A wrapper whose last statement is a `tail` (or any pipe)
#       reports the LAST command's status, so a script that exited 1 is announced as 0.  `rc=$?` is
#       captured as the FIRST statement after the run, before anything else can reset it.
# ⚠ (3) IT LAUNCHES A **PER-RUN COPY**.  bash reads a script incrementally BY BYTE OFFSET, so an edit
#       above the running position reparses the next command from the middle of a line -- a train
#       assembly died thirty minutes in on exactly that.  Make the copy first:
#           cp coord-train48-assemble.sh coord-train48-assemble-run1.sh
#       The labels derive from the basename, so the copy relabels itself and nothing is written out.
export TRAIN48_REQUIRE_ALL=1
cd "$(dirname "$0")"
bash coord-train48-assemble-run1.sh > coord-train48-assemble-run1.stdout 2>&1; rc=$?
# the wrapper's OWN trailing line -- the land's exit scan requires it as the record's last non-empty line (2026-09-13 run 8)
printf 'assembly exit=%s\n' "$rc" >> coord-train48-assemble-run1.stdout
echo "=== ASSEMBLY EXIT rc=$rc ==="
tail -40 coord-train48-assemble-run1.stdout
exit $rc
