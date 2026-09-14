# coord-train27-bookkeeping.py -- run AFTER coord-train27-land-launch.sh prints LAND DONE. Argument: the landed SHA.
import io, sys, time
NL = chr(10)
sha = sys.argv[1]
M = "~/.claude/projects/C--Projects-go2cs/memory/"
L = M + "train14-seat-ledger.md"; g = io.open(L, encoding="utf-8", newline="").read()
g += NL + "**" + time.strftime("%H:%M") + " TRAIN 27 LANDED master=%s** (eight seats: GB2, GQ48, C1RT3, RD, SUBQ43, SUBDOC10, SUBQ45 design, C2CEN25). Landed post: coord-entry-train27-landed.md. Next: train 28 (C2 Q44 + Q49 cuts, C1 getg, SUB-Q45 cut, SUB-Q50, G spike, R E2) -- rehearse in coord-t25-dryrun reset to %s; reflect TestChanOf+TestTypes read FIXED at this landing (D); runtime/pprof at 147 verdicts (Q43)." % (sha, sha) + NL
io.open(L, "w", encoding="utf-8", newline="").write(g)
H = M + "coordinator-handoff-state.md"; h = io.open(H, encoding="utf-8", newline="").read()
if "TRAIN 27 LANDED master=" not in h:
    h = h.rstrip(NL) + NL + "- TRAIN 27 LANDED master=%s (2026-09-05). Train 27 next: reset coord-t25-dryrun to %s, rehearse with coord-train27-rehearse.sh (REHEARSAL=1), assemble with coord-train27-assemble.sh (CONTROL_SHA=%s), land with coord-train27-land-launch.sh. Q51 (pprof bank) waits on Q47; Q52 (darwin signal design) on C2 after Q44/Q49." % (sha, sha, sha) + NL
h = h.replace("TRAIN 27 ASSEMBLING (run 1 launched 20:19 in musing-moser; monitor armed) after REHEARSAL 3 clean -- SCRIPTS READY", "TRAIN 27 LANDED " + sha)
io.open(H, "w", encoding="utf-8", newline="").write(h)
print("ledger"); print("handoff")
