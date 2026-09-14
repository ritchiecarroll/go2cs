import subprocess, io, json, re
# C2 posted its STATE BLOCK delta as "KEY<spaces>text" lines (no colon) inside a fence (241eb474f section 4).
# Copy NEXT and BLOCKED-ON out of the commit into a refresh spec -- nothing retyped.
MB = "C:/Projects/go2cs-mailbox-coord"; SHA = "241eb474fdea87562ff925c0ba0da9f98d2c78be"
diff = subprocess.run(["git", "-C", MB, "show", SHA, "--format=", "--", "."], capture_output=True).stdout.decode("utf-8", errors="replace")
added = [l[1:] for l in diff.split("\n") if l.startswith("+") and not l.startswith("+++")]
i = next(k for k, l in enumerate(added) if "STATE BLOCK DELTA" in l)
keys = {}
cur = None
for l in added[i + 1:]:
    if l.strip().startswith("```"): break
    m = re.match(r"^\s{2}(NEXT|BLOCKED-ON|BRANCH)\s{2,}(.*)$", l)
    if m:
        cur = m.group(1); keys[cur] = m.group(2).strip()
    elif cur and l.startswith("    ") and l.strip():
        keys[cur] += " " + l.strip()
spec = {"keys": {"C2": {k: v + " (C2 241eb474f, folded from the fenced delta)" for k, v in keys.items() if k in ("NEXT", "BLOCKED-ON")}},
        "log": "2026-09-13 22:00 -- C2's delta (241eb474f section 4) folded: NEXT + BLOCKED-ON copied from the fence; its BRANCH pin re-read from origin"}
io.open("~/AppData/Local/Temp/claude/C--Projects-go2cs/1e6e99a3-6151-40a5-abe6-501eb96e0a7f/scratchpad/resume-delta-2200.json", "w", encoding="utf-8").write(json.dumps(spec, ensure_ascii=False, indent=1))
print("keys copied:", {k: len(v) for k, v in keys.items()})
