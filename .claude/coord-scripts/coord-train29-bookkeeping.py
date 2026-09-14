# coord-train29-bookkeeping.py -- run AFTER coord-train29-land-launch.sh prints LAND DONE. Argument: the landed SHA.
import io, sys, time
NL = chr(10)
sha = sys.argv[1]
M = "~/.claude/projects/C--Projects-go2cs/memory/"
L = M + "train14-seat-ledger.md"; g = io.open(L, encoding="utf-8", newline="").read()
g += NL + "**" + time.strftime("%H:%M") + " TRAIN 29 LANDED master=%s** (seats: C2Q49 KeepAlive widening, GFVC field-view cache, SUBQ57 named-array empty literal, C1RT6 memory family, RE3B roots 4/5 + amendment + 7b-7d, C2Q56D + C2Q52D designs, C2INC7 libcCall result, C2INC8 sockaddr twin, C2Q52B signal bridge, SUBQ62 ledger flavour axis, SUBDOC11 doctrine batch 11; plus C2Q44 / C1RT7 if announced before assembly). Landed post: coord-entry-train29-landed.md. Next: train 30 (GA + later seats) -- reset coord-t25-dryrun to %s, REHEARSAL=1 CONTROL_SHA=%s bash coord-train30-rehearse.sh from the dry-run cwd, then assemble from musing-moser." % (sha, sha, sha) + NL
io.open(L, "w", encoding="utf-8", newline="").write(g)
H = M + "coordinator-handoff-state.md"; h = io.open(H, encoding="utf-8", newline="").read()
if "TRAIN 29 LANDED master=" not in h:
    h = h.rstrip(NL) + NL + "- TRAIN 29 LANDED master=%s (2026-09-05). Next: reset coord-t25-dryrun to %s; REHEARSAL=1 CONTROL_SHA=%s bash coord-train30-rehearse.sh (from the dry-run cwd); assemble train 30 from musing-moser with CONTROL_SHA=%s; sub-q57 and sub-q62 worktrees removable after this landing." % (sha, sha, sha, sha) + NL
io.open(H, "w", encoding="utf-8", newline="").write(h)
print("ledger"); print("handoff")
