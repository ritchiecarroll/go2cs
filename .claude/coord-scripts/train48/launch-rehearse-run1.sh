# TRAIN 48 -- LAUNCH WRAPPER FOR REHEARSAL RUN 1 (2026-09-13, after the D1-D6 re-derive).
# ⚠ IT LAUNCHES A **PER-RUN COPY** of the rehearsal, for the reason launch-run1.sh gives: bash reads a
#   script incrementally BY BYTE OFFSET, so an edit above the running position reparses the next
#   command from the middle of a line.
# ⚠ ONLY WHAT A LAUNCH NEEDS IS FILLED:
#     REHEARSE_WT  the throwaway worktree, OUTSIDE the repository and outside any battery's tree.
#     M            the master to rehearse ONTO -- pinned to 271300cea so the rehearsal describes the
#                  same base the self-check measured every seat against.  The rehearsal creates and
#                  removes this worktree itself (`git worktree add --detach`).
#   NOVET is NOT set: the per-seat `go build` + `go vet` + TestLicensing IS the rehearsal's contract.
# ⚠ rc IS CAPTURED AS THE FIRST STATEMENT AFTER THE RUN, before any pipe can reset it.
export REHEARSE_WT=/c/go2cs-tmp-coord/t48-rehearse
export M=271300cea03a2f47bd7dd8d9ed392c6249dac4c4
cd "$(dirname "$0")"
bash coord-train48-rehearse-run1.sh > coord-train48-rehearse-run1.stdout 2>&1; rc=$?
printf 'rehearsal exit=%s\n' "$rc" >> coord-train48-rehearse-run1.stdout
echo "=== REHEARSAL EXIT rc=$rc ==="
tail -60 coord-train48-rehearse-run1.stdout
exit $rc
