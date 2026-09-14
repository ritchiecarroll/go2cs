import subprocess, sys, io, re
# fold-block.py <LANE> <mailbox-sha> [<section-marker-substring>]
# Extracts the fenced STATE BLOCK (the ``` block whose first line starts with "LANE:") from a mailbox
# commit and inserts it VERBATIM into RESUME-SESSIONS.md right under the lane's heading, renaming the
# heading's {PENDING: lane STATE BLOCK} marker.  No SHA is ever typed by hand (i9 29419cf30 s2).
lane, sha = sys.argv[1], sys.argv[2]
MB = "C:/Projects/go2cs-mailbox-coord"
RS = "C:/go2cs-tmp-coord/hnd/docs/phase4/RESUME-SESSIONS.md"
diff = subprocess.run(["git", "-C", MB, "show", sha, "--format=", "--", "."], capture_output=True).stdout.decode("utf-8", errors="replace")
added = [l[1:] for l in diff.split("\n") if l.startswith("+") and not l.startswith("+++")]
block, inside = [], False
for l in added:
    if not inside and l.strip().startswith("```") and False:
        pass
    if not inside:
        if l.startswith("LANE:"):
            inside = True; block.append(l)
        continue
    if l.strip().startswith("```"):
        break
    block.append(l)
if not block or not block[0].startswith("LANE:"):
    print("REFUSED: no STATE BLOCK (a fenced block starting with LANE:) found in %s" % sha); sys.exit(2)
n_branch = sum(1 for l in block if l.startswith("BRANCH:"))
bad = [l for l in block if l.startswith("BRANCH:") and not re.match(r"^BRANCH: \S+ [0-9a-f]{40} ", l)]
if bad:
    print("REFUSED: %d BRANCH line(s) not in the ruled shape:" % len(bad)); [print("  " + b[:120]) for b in bad[:5]]; sys.exit(2)
text = io.open(RS, encoding="utf-8").read()
heading_rx = re.compile(r"^(## \d+\. %s .*?)\{PENDING: lane STATE BLOCK\}\s*$" % re.escape(lane), re.M)
m = heading_rx.search(text)
if not m:
    print("REFUSED: no pending heading for lane %s" % lane); sys.exit(2)
stamp = subprocess.run(["git", "-C", MB, "log", "-1", "--format=%ad", "--date=format:%H:%M", sha], capture_output=True, text=True).stdout.strip()
new_heading = "%s— STATE BLOCK received %s (mailbox %s)" % (m.group(1), stamp, sha[:9])
inserted = new_heading + "\n\n```\n" + "\n".join("  " + l for l in block) + "\n```\n"
text = text[:m.start()] + inserted + text[m.end():]
io.open(RS, "w", encoding="utf-8", newline="\n").write(text)
print("folded %s: %d block lines, %d BRANCH lines, from %s" % (lane, len(block), n_branch, sha[:9]))
