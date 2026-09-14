# coord-train26-bookkeeping.py -- run AFTER coord-train26-land-launch.sh prints LAND DONE. Argument: the landed SHA.
import io, sys, time
NL = chr(10)
sha = sys.argv[1]
M = "~/.claude/projects/C--Projects-go2cs/memory/"
L = M + "train14-seat-ledger.md"; g = io.open(L, encoding="utf-8", newline="").read()
g += NL + "**" + time.strftime("%H:%M") + " TRAIN 26 LANDED master=%s** (nine seats: GQ35, SUBQ39, C1RT2, C2CRASH, SUBQ42, SUBQ40, C1BILL, C2Q44D, C2MIR). Landed post: coord-entry-train26-landed.md. Next: train 27 (GB2 after rebase + nss.cs pair, GQ48 c5e552949, C1RT3, C2 Q44 cut, C2 Q49, R's D, SUB-Q43) -- rehearse in coord-t25-dryrun reset to %s; Q47 (getg cut) dispatchable now that SUBQ40's design is on master; the KeepAlive census guard's darwin arm rides with Q49." % (sha, sha) + NL
io.open(L, "w", encoding="utf-8", newline="").write(g)
H = M + "coordinator-handoff-state.md"; h = io.open(H, encoding="utf-8", newline="").read()
if "TRAIN 26 LANDED master=" not in h:
    h = h.rstrip(NL) + NL + "- TRAIN 26 LANDED master=%s (2026-09-05). Train 27 next: reset coord-t25-dryrun to %s, rehearse with coord-train27-rehearse.sh (REHEARSAL=1), assemble with coord-train27-assemble.sh (CONTROL_SHA=%s), land with coord-train27-land-launch.sh. Q47 dispatchable (SUBQ40 design landed)." % (sha, sha, sha) + NL
h = h.replace("TRAIN 26 ASSEMBLING (run 1 launched 20:19 in musing-moser; monitor armed) after REHEARSAL 3 clean -- SCRIPTS READY", "TRAIN 26 LANDED " + sha)
io.open(H, "w", encoding="utf-8", newline="").write(h)
print("ledger"); print("handoff")
