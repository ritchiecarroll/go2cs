import subprocess, sys, io, re
# apply-block-delta.py <LANE> <mailbox-sha> KEY[,KEY...]
# Takes the fenced replacement text a lane posted (lines starting with KEY: plus their indented
# continuation lines) and replaces that key's line(s) inside the lane's block in RESUME-SESSIONS.md.
# Nothing is retyped; the text is copied from the mailbox commit.
lane, sha, keys = sys.argv[1], sys.argv[2], sys.argv[3].split(",")
MB = "C:/Projects/go2cs-mailbox-coord"
RS = "C:/go2cs-tmp-coord/hnd/docs/phase4/RESUME-SESSIONS.md"
subprocess.run(["git", "-C", MB, "fetch", "-q", "origin", "claude/mailbox"])
diff = subprocess.run(["git", "-C", MB, "show", sha, "--format=", "--", "."], capture_output=True).stdout.decode("utf-8", errors="replace")
added = [l[1:] for l in diff.split("\n") if l.startswith("+") and not l.startswith("+++")]
# lanes post their keys INSIDE a fence, often indented by two spaces: dedent exactly that so the KEY: match holds;
# continuation lines keep their remaining indentation (still "starts with a space")
added = [l.lstrip() if re.match(r"^\s+[A-Z][A-Z0-9-]*:", l) else l for l in added]
# collect replacement text per key: a KEY: line and its continuation lines (indented, not another KEY:)
repl = {}
i = 0
keyrx = re.compile(r"^([A-Z][A-Z0-9-]*):")
while i < len(added):
    l = added[i]
    m = keyrx.match(l)
    if m and m.group(1) in keys and m.group(1) not in repl:
        block = [l]
        j = i + 1
        while j < len(added) and added[j].startswith(" ") and not keyrx.match(added[j].strip()):
            block.append(added[j]); j += 1
        repl[m.group(1)] = block
        i = j; continue
    i += 1
missing = [k for k in keys if k not in repl]
if missing:
    print("REFUSED: replacement text not found for", missing); sys.exit(2)
text = io.open(RS, encoding="utf-8").read()
m = re.search(r"^## \d+\. %s .*?\n\n```\n(.*?)\n```\n" % re.escape(lane), text, re.S | re.M)
if not m:
    print("REFUSED: no block for lane", lane); sys.exit(2)
lines = m.group(1).split("\n")
out = []
done = set()
for l in lines:
    s = l.lstrip()
    km = keyrx.match(s)
    if km and km.group(1) in repl and km.group(1) not in done:
        out.extend("  " + r for r in repl[km.group(1)])
        out.append("  (delta applied from mailbox %s)" % sha[:9])
        done.add(km.group(1))
    else:
        out.append(l)
notfound = [k for k in keys if k not in done]
if notfound:
    print("REFUSED: keys not present in the lane's block:", notfound); sys.exit(2)
text = text[:m.start(1)] + "\n".join(out) + text[m.end(1):]
io.open(RS, "w", encoding="utf-8", newline="\n").write(text)
print("applied delta for %s: %s from %s" % (lane, ",".join(keys), sha[:9]))
