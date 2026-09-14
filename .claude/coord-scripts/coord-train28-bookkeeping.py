# coord-train28-bookkeeping.py -- run AFTER coord-train28-land-launch.sh prints LAND DONE. Argument: the landed SHA.
import io, sys, time
NL = chr(10)
sha = sys.argv[1]
M = "~/.claude/projects/C--Projects-go2cs/memory/"
L = M + "train14-seat-ledger.md"; g = io.open(L, encoding="utf-8", newline="").read()
g += NL + "**" + time.strftime("%H:%M") + " TRAIN 28 LANDED master=%s** (twelve seats: C1RT4 getg, C1RT5 Q54, SUBQ45C Pinner, SUBQ50 unsafe.String, RE2 rebased, C2Q41F, GFVD design, C2INC6 sigaction, RE3 roots 1-3, SUBQ22 elided named pointee, C1Q46 host-fatal HANG, SUBQ55 process-group kill). Landed post: coord-entry-train28-landed.md. Next: train 29 (11 seats wired; C2Q44 at C2's announce) -- reset coord-t25-dryrun to %s, REHEARSAL=1 CONTROL_SHA=%s bash coord-train29-rehearse.sh from the dry-run cwd, then assemble from musing-moser." % (sha, sha, sha) + NL
io.open(L, "w", encoding="utf-8", newline="").write(g)
H = M + "coordinator-handoff-state.md"; h = io.open(H, encoding="utf-8", newline="").read()
if "TRAIN 28 LANDED master=" not in h:
    h = h.rstrip(NL) + NL + "- TRAIN 28 LANDED master=%s (2026-09-05). Next: reset coord-t25-dryrun to %s; REHEARSAL=1 CONTROL_SHA=%s bash coord-train29-rehearse.sh (from the dry-run cwd); assemble train 29 from musing-moser with CONTROL_SHA=%s; sub-q45 worktree removable; sub-q22/sub-q57/sub-q50 worktrees stay until train 29 lands." % (sha, sha, sha, sha) + NL
io.open(H, "w", encoding="utf-8", newline="").write(h)
print("ledger"); print("handoff")
