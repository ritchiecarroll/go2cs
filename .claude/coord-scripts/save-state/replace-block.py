import subprocess, sys, io, re
# replace-block.py <LANE> <mailbox-sha>
# Extracts the fenced STATE BLOCK (the ``` block whose first line starts with "LANE:") from a mailbox commit and
# REPLACES the lane's existing STATE BLOCK fence in RESUME-SESSIONS.md with it VERBATIM (two-space indented like
# the existing blocks).  Nothing is retyped.  Refuses when the posted block has no BRANCH line with a 40-char SHA.
lane, sha = sys.argv[1], sys.argv[2]
MB = "C:/Projects/go2cs-mailbox-coord"
RS = "C:/go2cs-tmp-coord/hnd/docs/phase4/RESUME-SESSIONS.md"
subprocess.run(["git", "-C", MB, "fetch", "-q", "origin", "claude/mailbox"])
diff = subprocess.run(["git", "-C", MB, "show", sha, "--format=", "--", "."], capture_output=True).stdout.decode("utf-8", errors="replace")
added = [l[1:] for l in diff.split("\n") if l.startswith("+") and not l.startswith("+++")]
# lanes post the block inside a fence, often indented: dedent key lines and their continuations by the LANE line's indent
first = next((l for l in added if re.match(r"^\s*LANE:", l)), None)
if first is not None:
    ind = len(first) - len(first.lstrip())
    added = [l[ind:] if l.startswith(" " * ind) and ind > 0 else l for l in added]
block, inside = [], False
for l in added:
    if not inside:
        if l.startswith("LANE:"):
            inside = True; block.append(l)
        continue
    if l.strip().startswith("```"):
        break
    block.append(l)
if not block:
    print("REFUSED: no fenced block starting with LANE: in", sha); sys.exit(2)
if not block[0].split()[1].rstrip(":").upper().startswith(lane.upper()):
    print("REFUSED: the block's LANE is", block[0][:40], "not", lane); sys.exit(2)
bad = [l for l in block if l.startswith("BRANCH:") and not re.match(r"^BRANCH: \S+ [0-9a-f]{40} ", l)]
lenient = len(sys.argv) > 3 and sys.argv[3] == "--lenient"
if bad and not lenient:
    print("REFUSED: BRANCH lines without a 40-char SHA:", bad[:3]); sys.exit(2)
if bad and lenient:
    print("WARNING (lenient fold): %d BRANCH line(s) without a 40-char SHA kept verbatim (the verifier classifies them):" % len(bad))
    for l in bad[:4]: print("   ", l[:110])
text = io.open(RS, encoding="utf-8").read()
lines = text.split("\n")
hi = next(i for i, l in enumerate(lines) if re.match(r"^## \d+\. %s\b" % re.escape(lane), l))
f1 = next(i for i in range(hi, len(lines)) if lines[i].strip() == "```")
f2 = next(i for i in range(f1 + 1, len(lines)) if lines[i].strip() == "```")
old = lines[f1 + 1:f2]
assert any(l.lstrip().startswith("LANE:") for l in old), "the first fence under the heading is not a STATE BLOCK"
new = ["  " + l for l in block]
lines[f1 + 1:f2] = new
# heading: record the fold
lines[hi] = re.sub(r" — STATE BLOCK received .*$", "", lines[hi]) + " — STATE BLOCK received (mailbox %s)" % sha[:9]
io.open(RS, "w", encoding="utf-8", newline="\n").write("\n".join(lines))
nb = sum(1 for l in block if l.startswith("BRANCH:"))
print("%s: STATE BLOCK replaced verbatim from %s (%d old lines -> %d new; BRANCH lines %d)" % (lane, sha[:9], len(old), len(new), nb))
