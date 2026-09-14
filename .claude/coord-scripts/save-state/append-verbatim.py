import subprocess, sys, io, re
# append-verbatim.py <LANE> <mailbox-sha> <label>
# Appends the WHOLE added text of a mailbox post VERBATIM (as a fenced block) at the end of the lane's section,
# for a final block whose shape the fold scripts cannot parse.  Nothing retyped, nothing dropped.
lane, sha, label = sys.argv[1], sys.argv[2], sys.argv[3]
MB = "C:/Projects/go2cs-mailbox-coord"
RS = "C:/go2cs-tmp-coord/hnd/docs/phase4/RESUME-SESSIONS.md"
subprocess.run(["git", "-C", MB, "fetch", "-q", "origin", "claude/mailbox"])
diff = subprocess.run(["git", "-C", MB, "show", sha, "--format=", "--", "."], capture_output=True).stdout.decode("utf-8", errors="replace")
added = [l[1:] for l in diff.split("\n") if l.startswith("+") and not l.startswith("+++")]
added = [l.replace("```", "'''") for l in added]  # keep the outer fence intact
lines = io.open(RS, encoding="utf-8").read().split("\n")
hi = next(i for i, l in enumerate(lines) if re.match(r"^## \d+\. %s\b" % re.escape(lane), l))
nxt = next((i for i in range(hi + 1, len(lines)) if re.match(r"^## \d+\. ", lines[i])), len(lines))
ins = nxt
while ins > hi and lines[ins - 1].strip() == "": ins -= 1
block = ["", "%s (mailbox %s, VERBATIM -- a shape the fold scripts do not parse; the STATE BLOCK at the top of this section is the last machine-folded one):" % (label, sha[:9]), "```"] + added + ["```", ""]
lines[ins:ins] = block
io.open(RS, "w", encoding="utf-8", newline="\n").write("\n".join(lines))
print("%s: appended %d verbatim lines from %s" % (lane, len(added), sha[:9]))
