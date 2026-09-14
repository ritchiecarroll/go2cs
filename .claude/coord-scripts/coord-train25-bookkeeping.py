# coord-train25-bookkeeping.py -- run AFTER coord-train25-land-launch.sh prints LAND DONE. Argument: the landed SHA.
import io, sys, time
sha = sys.argv[1]
NL = "\n"
M = "~/.claude/projects/C--Projects-go2cs/memory/"
L = M + "train14-seat-ledger.md"; g = io.open(L, encoding="utf-8", newline="").read()
g += NL + "**" + time.strftime("%H:%M") + " TRAIN 25 LANDED master=%s** (eleven seats + the roster-header union fix; GQ35 and SUBQ39 ride train 26). Landed post: coord-entry-train25-landed.md. Next: remove sub-q27/sub-q29/sub-q36/coord-c1q31-verify worktrees (dirt check first); REBASE coord-t25-dryrun onto the landed master for train 26's rehearsal (seats GQ35 claude/g-q35-i1-l3-footprint, SUBQ39 claude/sub-q39, + later); derive coord-train26-*.sh from the train-25 scripts; reset coord-nistec-ctrl (the land script does it). Roster after this train: net/http 1345, testing 37 + 15, header 27,776 / 165 / 203 rows (from the guard)." % sha + NL
io.open(L, "w", encoding="utf-8", newline="").write(g); print("ledger")
H = M + "coordinator-handoff-state.md"; h = io.open(H, encoding="utf-8", newline="").read()
if "TRAIN 25 LANDED master=" not in h:
    h = h.rstrip(NL) + NL + "- TRAIN 25 LANDED master=%s (2026-09-04). Doctrine batch 10 stays OPEN (no docs seat on this train; items 448-475 accumulated). Train 26 next: rebase coord-t25-dryrun onto %s (or add a fresh dry-run worktree), seats GQ35 + SUBQ39 when announced (+ C1's runtime increments, C2's Q41, R's D, SUB-Q42's guard as they land); CONTROL_SHA=%s; the rehearsal BEFORE the assembly. Objective census after this train: unowned rows os (G's arc), runtime (C1 Linux increments + Q40 getg design), runtime/pprof (Q43 after SUBQ27 lands), net/http/pprof (after Q43), runtime/trace (Q28 cut), reflect (R's D), unique (Blocker A -- re-derive against R's 4.2 at the union)." % (sha, sha, sha) + NL
io.open(H, "w", encoding="utf-8", newline="").write(h); print("handoff")
