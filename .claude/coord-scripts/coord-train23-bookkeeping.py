# coord-train23-bookkeeping.py -- run AFTER coord-train23-land-launch.sh prints LAND DONE. Argument: the landed SHA.
# 1. doctrine accumulator: append 'BATCH 8 LANDED (231-386) with train 23 at <sha>' and open batch 9 from 387.
# 2. seat ledger: the landing line.  3. handoff: the landed SHA and the next-train state.
import io, sys, time
sha = sys.argv[1]
NL = "\n"
M = "~/.claude/projects/C--Projects-go2cs/memory/"
d = M + "doctrine-batch3-accumulator.md"; s = io.open(d, encoding="utf-8", newline="").read()
if "BATCH 8 LANDED" not in s:
    s = s.rstrip(NL) + NL + NL + "**BATCH 8 LANDED (items 231-386) with train 23 at %s (2026-09-04) as seat SUB-DOC8 (86e16e9f5). BATCH 9 OPEN from item 387** -- the accumulator above 386 (387-4xx) is batch 9's; it lands as SUB-DOC9 on train 24 or 25 when cut." % sha + NL
    io.open(d, "w", encoding="utf-8", newline="").write(s); print("doctrine: batch 8 landed, batch 9 open")
L = M + "train14-seat-ledger.md"; g = io.open(L, encoding="utf-8", newline="").read()
g += NL + "**" + time.strftime("%H:%M") + " TRAIN 23 LANDED master=%s** (19 seats + the PointerOutParameter regen + the CompositeLiteralElements union re-baseline). Landed post: coord-entry-train23-landed.md. Next: C2 darwin arm64 census dispatch; purge the coordinator worktree; train 24 assembles when C1Q12 (A/B), GI1, RINC2 tips are final." % sha + NL
io.open(L, "w", encoding="utf-8", newline="").write(g); print("ledger")
H = M + "coordinator-handoff-state.md"; h = io.open(H, encoding="utf-8", newline="").read()
h = h.replace("master fd2e618b9", "master %s (train 23 landed 2026-09-04)" % sha)
if "TRAIN 23 LANDED master=" not in h:
    h = h.rstrip(NL) + NL + "- TRAIN 23 LANDED master=%s (2026-09-04). Train 24 next: coord-train24-assemble.sh with CONTROL_SHA=%s; seats C2SIG 3137e4e80e, SUBQ18 85301839f, SUBQ23 c7de2e643, C1Q12 960e518f9 (on the A/B reading), RTRACE ae8e50459, GI1/RINC2/SUBDOC9 when announced. Reset coord-nistec-ctrl to the new master (the land script does it)." % (sha, sha) + NL
io.open(H, "w", encoding="utf-8", newline="").write(h); print("handoff")
