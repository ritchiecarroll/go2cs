import subprocess, sys, io, re
# apply-wake.py <LANE> <mailbox-sha>
# Copies the lane's posted WAKE paragraph (the added lines from a line starting "WAKE" up to the next blank line or
# fence) over the file's existing "WAKE (<LANE>" paragraph under that lane's section.  Nothing retyped.
lane, sha = sys.argv[1], sys.argv[2]
MB = "C:/Projects/go2cs-mailbox-coord"
RS = "C:/go2cs-tmp-coord/hnd/docs/phase4/RESUME-SESSIONS.md"
subprocess.run(["git", "-C", MB, "fetch", "-q", "origin", "claude/mailbox"])
diff = subprocess.run(["git", "-C", MB, "show", sha, "--format=", "--", "."], capture_output=True).stdout.decode("utf-8", errors="replace")
added = [l[1:] for l in diff.split("\n") if l.startswith("+") and not l.startswith("+++")]
added = [l[2:] if l.startswith("  ") else l for l in added]
i = next((k for k, l in enumerate(added) if l.startswith("WAKE")), None)
if i is None:
    print("REFUSED: no WAKE paragraph in", sha); sys.exit(2)
para = []
for l in added[i:]:
    if l.strip() == "" or l.strip().startswith("```"):
        break
    para.append(l)
text = io.open(RS, encoding="utf-8").read()
lines = text.split("\n")
hi = next(k for k, l in enumerate(lines) if re.match(r"^## \d+\. %s\b" % re.escape(lane), l))
nxt = next((k for k in range(hi + 1, len(lines)) if re.match(r"^## \d+\. ", lines[k])), len(lines))
w = next((k for k in range(hi, nxt) if lines[k].startswith("WAKE")), None)
if w is None:
    print("REFUSED: no WAKE paragraph under the %s section" % lane); sys.exit(2)
e = w
while e + 1 < nxt and lines[e + 1].strip() != "" and not lines[e + 1].startswith("```"):
    e += 1
old = lines[w:e + 1]
lines[w:e + 1] = para
io.open(RS, "w", encoding="utf-8", newline="\n").write("\n".join(lines))
print("%s: WAKE paragraph replaced from %s (%d old lines -> %d)" % (lane, sha[:9], len(old), len(para)))
