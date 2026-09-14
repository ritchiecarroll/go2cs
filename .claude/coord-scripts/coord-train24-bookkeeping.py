# coord-train24-bookkeeping.py -- run AFTER coord-train24-land-launch.sh prints LAND DONE. Argument: the landed SHA.
import io, sys, time
sha = sys.argv[1]
NL = "\n"
M = "~/.claude/projects/C--Projects-go2cs/memory/"
d = M + "doctrine-batch3-accumulator.md"; s = io.open(d, encoding="utf-8", newline="").read()
if "BATCH 9 LANDED" not in s:
    s = s.rstrip(NL) + NL + NL + "**BATCH 9 LANDED (items 387-447) with train 24 at %s (2026-09-04) as seat SUB-DOC9 (8508b0e3b). BATCH 10 OPEN from item 448** -- items 448 onward accumulate for the next docs seat." % sha + NL
    io.open(d, "w", encoding="utf-8", newline="").write(s); print("doctrine: batch 9 landed, batch 10 open at 448")
L = M + "train14-seat-ledger.md"; g = io.open(L, encoding="utf-8", newline="").read()
g += NL + "**" + time.strftime("%H:%M") + " TRAIN 24 LANDED master=%s** (nine seats + the union fix; RINC2 rides train 25). Landed post: coord-entry-train24-landed.md. Next: remove sub-q18/sub-q23/sub-doc9/coord-t24-dryrun worktrees (dirt check first); train 25 = RINC2 (4.2 + 2b) + G's B (claude/g-b-defer-finally) + Q32's block + later seats; reset coord-nistec-ctrl (the land script does it)." % sha + NL
io.open(L, "w", encoding="utf-8", newline="").write(g); print("ledger")
H = M + "coordinator-handoff-state.md"; h = io.open(H, encoding="utf-8", newline="").read()
if "TRAIN 24 LANDED master=" not in h:
    h = h.rstrip(NL) + NL + "- TRAIN 24 LANDED master=%s (2026-09-04). The testing row is BANKED on master (roster 203 rows / 27,772 / 169 / 94.4%%). Train 25 next: derive coord-train25-assemble.sh from the train-24 template (seats RINC2 claude/reflect-cargo-inc-2 when R announces, GB claude/g-b-defer-finally when G announces, SUBQ32 claude/sub-q32 when its block lands, + later); CONTROL_SHA=%s; the dry-run rehearsal (coord-t24-dryrun pattern) BEFORE the assembly; RINC2/GB touch golib + converter + behavioral goldens -- rehearse. Objective census: unowned now os (G's arc), runtime/pprof (Q27), net/http/pprof (after Q27), runtime/trace (Q28 cut after Q27), bcache (re-derive)." % (sha, sha) + NL
io.open(H, "w", encoding="utf-8", newline="").write(h); print("handoff")
