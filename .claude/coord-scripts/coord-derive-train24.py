# coord-derive-train24.py -- derive coord-train24-assemble.sh from the train-23 template (run from the coord-scripts dir).
# Changes owed from train 23's battery (handoff note 2026-09-04): (a) every battery leg stamps its exit AND one-line verdict into the
# ASSEMBLY log; (b) the union CNR's CHANGED set is stamped by name; (c) the PointerOutParameter regen step (train-23-specific) is dropped;
# (d) the seat list is train 24's. The land script's prune list is generated separately (coord-train24-land.sh) from the same seat table.
import io, re, sys
src = io.open("coord-train23-assemble.sh", encoding="utf-8", newline="").read()
NL = "\n"
lines = src.split(NL)

SEATS = [
    # (var, branch, msgfile, label)  -- order: registry-touching first (union resolver), then converter, golib/runtime, host, docs
    ("C2SIG",  "claude/c2-darwin-signote",     "coord-merge-c2-darwin-signote.txt",     "C2: darwin run layer increment 4 Scope B -- pipe/read/write1 hand-owned over libc, the mute-exit-138 baseline's root (SHA at the post)"),
    ("SUBQ18", "claude/sub-q18",               "coord-merge-sub-q18.txt",               "SUB-Q18: the testing row in test-only mode -- option B relocation, the complete-declared-set fix, testing.Testing capability, the host-reference guard; host fix re-points the seat (SHA at the post)"),
    ("SUBQ23", "claude/sub-q23",               "coord-merge-sub-q23.txt",               "SUB-Q23: the finalizer dispatch deadlock -- fing-analogue runner, bounded drain, TestGoroutineCounts answers in 10 s (SHA at the post)"),
    ("C1Q12",  "claude/c1-q12-main-identity",  "coord-merge-c1-q12-main-identity.txt",  "C1: Q12 remedy (C) main-goroutine identity + Q8 ProcessExit flush + (A) NoInlining/identity skip + (B) creator at launch (SHA at the post)"),
    ("RTRACE", "claude/r-trace-recon",         "coord-merge-r-trace-recon.txt",         "R: runtime/trace recon + untestable-by-capability classification, docs only (SHA at the post)"),
    ("GI1",    "claude/g-i1-parameter-half",   "coord-merge-g-i1.txt",                  "G: I1 revived -- the parameter half of the os alloc arc, first site Ꮡwop (SHA at the post)"),
    ("SUBDOC9","claude/sub-doc9",              "coord-merge-sub-doc9.txt",              "SUB-DOC9: doctrine batch 9 landed into CLAUDE.md, docs only (SHA at the post)"),
]

out = []
i = 0
# 1. header comment block (lines 2..9) rewritten
out.append(lines[0])
out.append("# coord-train24-assemble.sh -- merge train 24 on top of the train-23 landed master (set CONTROL_SHA to it), then run its battery. Seats (remote tips; an unset SHA skips):")
for var, br, msg, label in SEATS:
    out.append("#   %s_SHA %s  (%s)" % (var.ljust(7), br.ljust(34), label))
out.append("# Battery: converter suite + full CNR (CHANGED set stamped by name); syscall-linux; slnx; GolibTests + alone; reflect build; FULL behavioral; sweeps; nistec pair; reflect RUN.")
out.append("# Every leg stamps `LEG <name> exit=<code> :: <verdict>` into THIS log (the Monitor tails it); per-leg logs keep the detail.")
# skip original header lines 2..9 (index 1..8)
i = 9
while i < len(lines):
    ln = lines[i]
    if ln.startswith("GREC8_SHA="):
        vals = []
        for var, br, msg, label in SEATS:
            vals.append('%s_SHA="${%s_SHA:-}"; %s_BRANCH="${%s_BRANCH:-%s}"' % (var, var, var, var, br))
        out.append("; ".join(vals))
        i += 1; continue
    if ln.startswith("seat claude/g-record-i1-retired") or ln.startswith('seat "$C2PIN_BRANCH"') or ln.startswith("seat "):
        # drop all original seat lines; emit the new seat table once (at the first)
        if not any(l.startswith('seat "$C2SIG_BRANCH"') for l in out):
            for var, br, msg, label in SEATS:
                out.append('seat "$%s_BRANCH" "$%s_SHA" "$SP/%s" "%s"' % (var, var, msg, label.replace('"', "'")))
        i += 1; continue
    if ln.startswith("# C2 pin finding 3:"):
        # drop the regen block through the line before the ASSEMBLED stamp
        while i < len(lines) and not lines[i].startswith('stamp "TRAIN 23 ASSEMBLED'):
            i += 1
        continue
    out.append(ln); i += 1

s = NL.join(out)
s = s.replace("train23", "train24").replace("TRAIN 23", "TRAIN 24").replace("train-23 seats", "train-24 seats")
s = s.replace("vs the train-22 head", "vs the train-23 head")
# (a)+(b): leg stamps. Insert a stamp after each leg's log line.
def after(anchor_sub, stamp_line):
    global s
    idx = s.find(anchor_sub); assert idx >= 0, anchor_sub
    end = s.find(NL, idx); s = s[:end+1] + stamp_line + NL + s[end+1:]
after('echo "suite+cnr exit=$?" >> "$SP/coord-train24-suite-cnr-$ts.log"',
      'stamp "LEG suite+cnr $(tail -1 "$SP/coord-train24-suite-cnr-$ts.log") :: $(grep -aE \'^ok|LEG1 END|LEG2 END\' "$SP/coord-train24-suite-cnr-$ts.log" | tr \'\\n\' \' \') :: CHANGED: $(sed -n \'/CHANGED converter output/,/^\\[/p\' "$SP/coord-train24-suite-cnr-$ts.log" | grep -aE \'^ +[AM?] \' | tr \'\\n\' \' \')"')
after('echo "slnx exit=$?" >> "$SP/coord-train24-slnx-$ts.log"',
      'stamp "LEG slnx $(tail -1 "$SP/coord-train24-slnx-$ts.log") :: $(grep -aE \'SLNX BUILD END\' "$SP/coord-train24-slnx-$ts.log" | tail -1)"')
after('echo "golibtests exit=$?" >> "$SP/coord-train24-golibtests-$ts.log"',
      'stamp "LEG golibtests $(tail -1 "$SP/coord-train24-golibtests-$ts.log") :: $(grep -aE \'count-matched|Aborted|Failed:\' "$SP/coord-train24-golibtests-$ts.log" | tail -2 | tr \'\\n\' \' \')"')
after('echo "alone exit=$?" >> "$SP/coord-train24-alone-$ts.log"',
      'stamp "LEG alone $(tail -1 "$SP/coord-train24-alone-$ts.log") :: $(grep -a \'EACH-CLASS-ALONE END\' "$SP/coord-train24-alone-$ts.log" | tail -1)"')
after('echo "reflect exit=$?" >> "$SP/coord-train24-reflect-$ts.log"',
      'stamp "LEG reflect $(tail -1 "$SP/coord-train24-reflect-$ts.log") :: $(grep -aE \'exit=\' "$SP/coord-train24-reflect-$ts.log" | tr \'\\n\' \' \')"')
io.open("coord-train24-assemble.sh", "w", encoding="utf-8", newline="").write(s)
print("written; seat lines:", s.count('\nseat "$'), "; regen block present:", "PointerOutParameter" in s, "; LEG stamps:", s.count('stamp "LEG '))
