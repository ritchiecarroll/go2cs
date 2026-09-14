import io, re
# A lane section = STATE BLOCK fence (indented keys; the record COORD refreshes) + WAKE paragraph + the PASTE PROMPT fence.
# The paste prompts for i9 and C2 still carried their own STATE/NEXT/TOOLS/BLOCKED-ON/READ FIRST text from the
# 15:xx fold -- a second, unrefreshed copy of the state (i9 6520a98801: two NEXT keys, two TOOLS keys, a deleted
# ref named as live).  This rewrites every lane's paste prompt so that the ONLY state it carries is a pointer to
# the STATE BLOCK above it.  Identity/role lines (before the first state key) and the PROTOCOL line are kept.
RS = "C:/go2cs-tmp-coord/hnd/docs/phase4/RESUME-SESSIONS.md"
L = io.open(RS, encoding="utf-8").read().split("\n")
secs = [i for i, l in enumerate(L) if re.match(r"^## \d+\. ", l)]
STATE_KEYS = re.compile(r"^(READ FIRST|STATE|NEXT|TOOLS|BLOCKED-ON|WAKE|BRANCHES?|LOCAL-ONLY|WORKTREES?)\b")
POINTER = [
    "STATE: your STATE BLOCK is the fenced block at the TOP of this section (keys LANE / BRANCH / LOCAL-ONLY / WORKTREE /",
    "  NEXT / READ-FIRST / BLOCKED-ON / TOOLS) plus the WAKE paragraph under it.  That block is the ONLY record: COORD",
    "  re-reads every BRANCH pin from origin and re-derives NEXT, BLOCKED-ON and READ-FIRST from the LATEST ruling",
    "  at every refresh.  Nothing else in this section carries state; a NEXT found anywhere else is stale by",
    "  construction (i9 6520a98801, G f2f6240a1).  Read it top to bottom before the first command.",
]
changed = []
for si, s in enumerate(secs):
    e = secs[si + 1] if si + 1 < len(secs) else len(L)
    m = re.match(r"^## \d+\. (i9|C1|C2|G|R)\b", L[s])
    if not m:
        continue
    lane = m.group(1)
    fences = [i for i in range(s, e) if L[i].strip() == "```"]
    if len(fences) < 4:
        continue  # no separate paste prompt fence beyond the STATE BLOCK
    a, b = fences[2], fences[3]
    body = L[a + 1:b]
    # identity lines = everything before the first state key line
    first = next((k for k, l in enumerate(body) if STATE_KEYS.match(l)), None)
    if first is None:
        continue
    identity = body[:first]
    protocol = [l for l in body[first:] if l.startswith("PROTOCOL:")]
    new = identity + POINTER + (protocol or ["PROTOCOL: as COORD's section (post tool, watcher line, announce-then-push, nicknames only)."])
    dropped = [l for l in body[first:] if not l.startswith("PROTOCOL:")]
    L[a + 1:b] = new
    # recompute section indices after the edit
    delta = len(new) - len(body)
    secs = [x + delta if x > a else x for x in secs]
    changed.append((lane, len(dropped)))
    print("%s: paste prompt rewritten -- %d embedded state line(s) dropped, %d identity line(s) kept, PROTOCOL %s" % (lane, len(dropped), len(identity), "kept" if protocol else "added"))
io.open(RS, "w", encoding="utf-8", newline="\n").write("\n".join(L))
print("lanes changed:", changed)
