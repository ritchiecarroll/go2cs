import subprocess, sys, io, re, json
# refresh-resume.py <spec.json>
# (a) every BRANCH line's 40-char SHA is RE-READ at origin (git ls-remote) and replaced when the ref moved -- never typed;
# (b) spec["add"] = [[LANE, ref, description], ...] appends a BRANCH line after the lane's last BRANCH line, SHA from ls-remote;
# (c) spec["next"] = {LANE: text} replaces the lane's NEXT: line (and its continuation lines);
# (d) spec["log"] appends one line to the Revision log; (e) spec["handover"] = path appends that file to
#     HANDOVER-coordinator.md with the target's own line ending.
spec = json.load(io.open(sys.argv[1], encoding="utf-8"))
RS = "C:/go2cs-tmp-coord/hnd/docs/phase4/RESUME-SESSIONS.md"
HO = "C:/go2cs-tmp-coord/hnd/docs/phase4/HANDOVER-coordinator.md"
G = "C:/Projects/go2cs"
keyrx = re.compile(r"^([A-Z][A-Z0-9-]*):")
text = io.open(RS, encoding="utf-8").read()
assert "\r" not in text, "RESUME-SESSIONS.md is LF-only by convention"
lines = text.split("\n")

def ls_remote(ref):
    out = subprocess.run(["git", "-C", G, "ls-remote", "origin", "refs/heads/" + ref], capture_output=True).stdout.decode().strip()
    return out[:40] if out else None

# (a) refresh every BRANCH pin
moved, absent, same = [], [], 0
for i, l in enumerate(lines):
    m = re.match(r"^(\s*BRANCH: )(\S+) ([0-9a-f]{40}) (.*)$", l)
    if not m:
        continue
    ref, sha = m.group(2), m.group(3)
    tip = ls_remote(ref)
    if tip is None:
        absent.append((ref, sha[:9])); continue
    if tip != sha:
        lines[i] = m.group(1) + ref + " " + tip + " " + m.group(4)
        moved.append((ref, sha[:9], tip[:9]))
    else:
        same += 1
print("BRANCH pins: %d unchanged, %d moved, %d not at origin (left as written)" % (same, len(moved), len(absent)))
for r, a, b in moved: print("  MOVED   %-48s %s -> %s" % (r, a, b))
for r, a in absent: print("  ABSENT  %-48s %s (pruned or local; verifier classifies)" % (r, a))

def lane_span(lane):
    # the fenced block right under "## N. LANE ..." -- returns (first_line_idx, last_line_idx) inside the fence
    for i, l in enumerate(lines):
        if re.match(r"^## \d+\. %s\b" % re.escape(lane), l):
            j = i + 1
            while j < len(lines) and lines[j].strip() != "```": j += 1
            k = j + 1
            while k < len(lines) and lines[k].strip() != "```": k += 1
            return j + 1, k - 1
    raise SystemExit("REFUSED: no block for lane " + lane)

# (b) add BRANCH lines
present_refs = set(re.findall(r"^\s*BRANCH: (\S+) ", "\n".join(lines), re.M))
for lane, ref, desc in spec.get("add", []):
    if ref in present_refs:
        print("  SKIP add (already present somewhere): %s" % ref); continue
    tip = ls_remote(ref)
    if tip is None:
        print("  REFUSED add (not at origin): %s" % ref); continue
    a, b = lane_span(lane)
    last = max(i for i in range(a, b + 1) if lines[i].lstrip().startswith("BRANCH: "))
    lines.insert(last + 1, "  BRANCH: %s %s %s" % (ref, tip, desc))
    present_refs.add(ref)
    print("  ADDED   %-48s %s -> %s block" % (ref, tip[:9], lane))

# (c) replace NEXT: (spec["next"]) or any single-occurrence KEY (spec["keys"] = {LANE: {KEY: text}})
repls = {}
for lane, txt in spec.get("next", {}).items():
    repls.setdefault(lane, {})["NEXT"] = txt
for lane, kv in spec.get("keys", {}).items():
    repls.setdefault(lane, {}).update(kv)
for lane, kv in repls.items():
    for key, txt in kv.items():
        a, b = lane_span(lane)
        idx = [i for i in range(a, b + 1) if lines[i].lstrip().startswith(key + ":")]
        if len(idx) != 1:
            print("  REFUSED %s for %s: %d %s: lines" % (key, lane, len(idx), key)); continue
        i = idx[0]
        j = i + 1
        while j <= b and not keyrx.match(lines[j].lstrip()) and lines[j].strip() != "":
            j += 1
        old = lines[i:j]
        lines[i:j] = ["  %s: %s" % (key, txt)]
        print("  %s %s replaced (%d old line(s)): %s" % (key, lane, len(old), old[0][:100]))

# (d) revision log
if spec.get("log"):
    lines.append("- " + spec["log"]) if lines[-1] != "" else lines.insert(len(lines) - 1, "- " + spec["log"])
    print("  LOG line appended")

io.open(RS, "w", encoding="utf-8", newline="\n").write("\n".join(lines))

# (e) handover block
if spec.get("handover"):
    raw = io.open(HO, "rb").read()
    nl = b"\r\n" if b"\r\n" in raw else b"\n"
    block = io.open(spec["handover"], encoding="utf-8").read().replace("\r\n", "\n")
    if not raw.endswith(nl): raw += nl
    raw += nl + block.rstrip("\n").replace("\n", nl.decode()).encode("utf-8") + nl
    io.open(HO, "wb").write(raw)
    print("  HANDOVER block appended (%s line endings)" % ("CRLF" if nl == b"\r\n" else "LF"))
