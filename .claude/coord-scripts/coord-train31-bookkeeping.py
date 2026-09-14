# coord-train31-bookkeeping.py -- run AFTER coord-train31-land-launch.sh prints LAND DONE. Argument: the landed SHA.
import io, sys, time, glob
NL = chr(10); sha = sys.argv[1]
M = "~/.claude/projects/C--Projects-go2cs/memory/"
seats = "C1ERB 810b03087, C2INC10B 51884af75, C1REAP 3af4c88ec, C1RT8 b7a58eda0, C1Q58A a3ee3945c, C2Q44A 66a6bdb96, RE2C 17dbf98bd"
L = M + "train14-seat-ledger.md"; g = io.open(L, encoding="utf-8", newline="").read()
g += NL + "**" + time.strftime("%Y-%m-%d %H:%M") + " TRAIN 31 LANDED master " + sha + " (16 seats: " + seats + "). Next train derives from train 31's scripts.**" + NL
io.open(L, "w", encoding="utf-8", newline="").write(g)
H = M + "coordinator-handoff-state.md"; h = io.open(H, encoding="utf-8", newline="").read()
h += NL + "**" + time.strftime("%H:%M") + " TRAIN 31 LANDED " + sha + ". Post coord-entry-train31-landed.md (fill <WHAT_MOVES>); purge musing-moser; dry-run reset to origin/master; derive train 31 (coord-derive-train31.py from train 31's scripts). G's os sweep under the deferred class can bank now (guard landed).**" + NL
io.open(H, "w", encoding="utf-8", newline="").write(h)
for q in ["q72","q74","q58"]:
    for f in glob.glob("C:/Projects/go2cs/.claude/coord-scripts/coord-queue-" + q + "*.md"):
        s = io.open(f, encoding="utf-8", newline="").read(); s += NL + "**" + time.strftime("%Y-%m-%d %H:%M") + " LANDED with train 31 at " + sha + ".**" + NL; io.open(f, "w", encoding="utf-8", newline="").write(s)
print("ledger"); print("handoff"); print("queues updated")
