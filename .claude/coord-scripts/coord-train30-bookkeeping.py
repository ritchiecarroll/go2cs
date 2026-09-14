# coord-train30-bookkeeping.py -- run AFTER coord-train30-land-launch.sh prints LAND DONE. Argument: the landed SHA.
import io, sys, time, glob
NL = chr(10); sha = sys.argv[1]
M = "~/.claude/projects/C--Projects-go2cs/memory/"
seats = "C2Q44 eed11b550, C2INC9 d185e28b8, C2Q56L 0ac8a607c, C2INC10 4efd81cf5, SUBQ63 66a73ab03, SUBQ60 16d1943ac, SUBQ59 1dd5bf492, GBD 58e83c419, GED b4337813a, GCD 6db8d95a2, GFVCR 2f43ef7b3, RE2B ca74dd433, C1Q61 e33e14ccf, C1Q64 7ab3d6fa6, C1Q58D 44fba8cf6, GDC 67eba534f"
L = M + "train14-seat-ledger.md"; g = io.open(L, encoding="utf-8", newline="").read()
g += NL + "**" + time.strftime("%Y-%m-%d %H:%M") + " TRAIN 30 LANDED master " + sha + " (16 seats: " + seats + "). Next train derives from train 30's scripts.**" + NL
io.open(L, "w", encoding="utf-8", newline="").write(g)
H = M + "coordinator-handoff-state.md"; h = io.open(H, encoding="utf-8", newline="").read()
h += NL + "**" + time.strftime("%H:%M") + " TRAIN 30 LANDED " + sha + ". Post coord-entry-train30-landed.md (fill <WHAT_MOVES>); purge musing-moser; dry-run reset to origin/master; derive train 31 (coord-derive-train31.py from train 30's scripts). G's os sweep under the deferred class can bank now (guard landed).**" + NL
io.open(H, "w", encoding="utf-8", newline="").write(h)
for q in ["q44","q59","q60","q61","q63","q64","q58"]:
    for f in glob.glob("C:/Projects/go2cs/.claude/coord-scripts/coord-queue-" + q + "*.md"):
        s = io.open(f, encoding="utf-8", newline="").read(); s += NL + "**" + time.strftime("%Y-%m-%d %H:%M") + " LANDED with train 30 at " + sha + ".**" + NL; io.open(f, "w", encoding="utf-8", newline="").write(s)
print("ledger"); print("handoff"); print("queues updated")
