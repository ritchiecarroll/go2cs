import io, re, sys, subprocess
# resume-tools.py -- scripted edits to RESUME-SESSIONS.md (hnd worktree). LF only. SHAs are read from origin, never typed.
#   set-key    LANE KEY FILE      replace the KEY: line (and its continuation lines) inside the lane's STATE BLOCK fence
#   set-prompt LANE FILE          normalize the lane's section and set its PASTE PROMPT fence from FILE (idempotent)
#   show       LANE               print the lane's section line span and its fences (a read, for checking)
RS = "C:/go2cs-tmp-coord/hnd/docs/phase4/RESUME-SESSIONS.md"
G = "C:/Projects/go2cs"
KEYRX = re.compile(r"^(\s*)([A-Z][A-Z0-9-]*):")
MARK = "PASTE PROMPT (revision 2026-09-14"

def ls_remote(ref):
    out = subprocess.run(["git", "-C", G, "ls-remote", "origin", "refs/heads/" + ref], capture_output=True).stdout.decode().strip()
    return out[:40] if out else None

def load():
    t = io.open(RS, encoding="utf-8").read()
    assert "\r" not in t, "RESUME-SESSIONS.md is LF-only by convention"
    return t.split("\n")

def save(lines):
    t = "\n".join(lines)
    assert "\r" not in t
    io.open(RS, "w", encoding="utf-8", newline="\n").write(t)

def section(lines, lane):
    a = None
    for i, l in enumerate(lines):
        if re.match(r"^## \d+\. %s\b" % re.escape(lane), l):
            a = i; break
    if a is None: raise SystemExit("REFUSED: no section for lane " + lane)
    b = a + 1
    # a section ends at the next NUMBERED heading; verbatim mailbox posts inside fences carry "## 2026-.." headings
    while b < len(lines) and not re.match(r"^## \d+\. ", lines[b]): b += 1
    return a, b  # [a, b)

def fences(lines, a, b):
    out, i = [], a
    while i < b:
        if lines[i].strip() == "```":
            j = i + 1
            while j < b and lines[j].strip() != "```": j += 1
            if j >= b: raise SystemExit("REFUSED: unterminated fence at line %d" % (i + 1))
            out.append((i, j)); i = j + 1
        else:
            i += 1
    return out  # list of (open_idx, close_idx)

def first_nonblank(lines, i, j):
    for k in range(i + 1, j):
        if lines[k].strip(): return lines[k]
    return ""

def is_old_prompt(lines, i, j):
    f = first_nonblank(lines, i, j).strip()
    return f.startswith("You are lane") or f.startswith("RESUME 2026")

def normalize(lines, lane):
    """Make the lane's FIRST fence a pure STATE BLOCK; drop old prompt fences; return (a, b)."""
    a, b = section(lines, lane)
    fs = fences(lines, a, b)
    if not fs: raise SystemExit("REFUSED: lane %s has no fence" % lane)
    i, j = fs[0]
    body = lines[i + 1:j]
    has_keys = any(KEYRX.match(l) and KEYRX.match(l).group(2) == "LANE" for l in body)
    if has_keys and is_old_prompt(lines, i, j):
        # C1 shape: prose header lines before the keys -> drop the prose; drop stale prompt lines inside
        k = next(n for n, l in enumerate(body) if KEYRX.match(l) and KEYRX.match(l).group(2) == "LANE")
        body = body[k:]
        body = [l for l in body if not re.match(r"^\s*(NEXT \(ruled|PROTOCOL:)", l)]
        lines[i + 1:j] = body
        print("  %s: first fence split -- prose dropped, %d stale line(s) removed" % (lane, (j - i - 1) - len(body)))
    elif not has_keys:
        # R shape: no STATE BLOCK at all -> insert a minimal one before the first fence
        skel = ls_remote("claude/laneR-docs-h6-skeleton") or "NOT-AT-ORIGIN"
        blk = ["```",
               "  LANE: R   MODEL: Opus 5/high (steward); Fable 5.1 in a ruling spurt   HOST: R-LAPTOP (owner travel; FLEET STANDBY, spurts only)",
               "  BRANCH: claude/laneR-docs-h6-skeleton %s yes accepted -- R's record of the H6 audit skeleton, amended by G (G fills it); pin re-read at origin by script" % skel,
               "  LOCAL-ONLY: none posted -- R's own STATE BLOCK is PENDING; posting it is R's first act on resume",
               "  WORKTREE: pending R's own block",
               "  NEXT: SAVE-STATE STEWARD per the paste prompt below, from claude/coord-handover at its tip; post R's own STATE BLOCK first (keys LANE / BRANCH with 40-char SHAs read from origin / LOCAL-ONLY / WORKTREE / NEXT / READ-FIRST / BLOCKED-ON / TOOLS)",
               "  READ-FIRST: 2cd01f8d6 (COORD ONLINE, ruling R5) - this file's COORD section - .claude/skills/save-state/SKILL.md - HANDOVER-coordinator.md blocks 12-15",
               "  BLOCKED-ON: owner -- the R-LAPTOP session opens in spurts only (travel standby)",
               "  TOOLS: pending R's own block (GOROOT pins, DOTNET_ROOT, python by env-var name and version, never a hostname)",
               "```"]
        lines[i:i] = blk
        print("  %s: minimal STATE BLOCK inserted (skeleton pin %s)" % (lane, skel[:9]))
    # drop old prompt fences anywhere in the section (recomputed after edits)
    a, b = section(lines, lane)
    fs = fences(lines, a, b)
    for (i, j) in reversed(fs[1:]):
        # the lane's CURRENT paste prompt sits right under the PASTE PROMPT marker line: never an "old" fence
        if i > 0 and lines[i - 1].startswith(MARK):
            continue
        if is_old_prompt(lines, i, j):
            # also drop a preceding blank line to keep spacing tidy
            del lines[i:j + 1]
            print("  %s: old prompt fence removed (%d lines)" % (lane, j - i + 1))
    return section(lines, lane)

def set_prompt(lane, path):
    lines = load()
    a, b = normalize(lines, lane)
    new = io.open(path, encoding="utf-8").read().replace("\r\n", "\n").rstrip("\n").split("\n")
    assert not any(re.search(r"<[A-Z0-9_]{3,}>", l) for l in new), "REFUSED: prompt carries an angle-bracket placeholder"
    assert not any("{LANE}" in l or "{MODEL}" in l or "{EFFORT}" in l or "{BOX}" in l for l in new), "REFUSED: unfilled brace"
    fs = fences(lines, a, b)
    # existing PASTE PROMPT marker?
    mk = next((n for n in range(a, b) if lines[n].startswith(MARK)), None)
    marker = ("%s 18:26 -- derived from the COORD ONLINE post 2cd01f8d6 and this lane's STATE BLOCK; verified on three lenses: refs at origin, "
              "security/format, actionability) -- paste as the FIRST message of a fresh session on this lane's machine, after the owner's GPG prime on a Windows box:" % MARK)
    if mk is not None:
        fo = next((i, j) for (i, j) in fences(lines, a, b) if i > mk)
        lines[fo[0] + 1:fo[1]] = new
        lines[mk] = marker
        print("  %s: PASTE PROMPT replaced (%d lines)" % (lane, len(new)))
    else:
        # insert after the first fence's close + the contiguous non-blank paragraph that follows it (WAKE / COORD NOTE)
        i, j = fs[0]
        k = j + 1
        while k < b and lines[k].strip(): k += 1
        ins = ["", marker, "```"] + new + ["```"]
        lines[k:k] = ins
        print("  %s: PASTE PROMPT inserted after line %d (%d lines)" % (lane, k, len(new)))
    save(lines)

def set_key(lane, key, path):
    lines = load()
    a, b = section(lines, lane)
    i, j = fences(lines, a, b)[0]
    text = io.open(path, encoding="utf-8").read().replace("\r\n", "\n").rstrip("\n").split("\n")
    assert text and text[0].strip(), "REFUSED: empty key text"
    idx = next((n for n in range(i + 1, j) if KEYRX.match(lines[n]) and KEYRX.match(lines[n]).group(2) == key), None)
    if idx is None: raise SystemExit("REFUSED: key %s not in lane %s block" % (key, lane))
    indent = KEYRX.match(lines[idx]).group(1)
    e = idx + 1
    while e < j and not KEYRX.match(lines[e]): e += 1
    repl = [indent + key + ": " + text[0].strip()] + [indent + "        " + t.strip() for t in text[1:]]
    old = lines[idx:e]
    lines[idx:e] = repl
    save(lines)
    print("  %s.%s: %d line(s) -> %d line(s)" % (lane, key, len(old), len(repl)))

def show(lane):
    lines = load()
    a, b = section(lines, lane)
    print("section %s: lines %d-%d" % (lane, a + 1, b))
    for (i, j) in fences(lines, a, b):
        print("  fence %d-%d: %s" % (i + 1, j + 1, first_nonblank(lines, i, j).strip()[:90]))

cmd = sys.argv[1]
if cmd == "set-key": set_key(sys.argv[2], sys.argv[3], sys.argv[4])
elif cmd == "set-prompt": set_prompt(sys.argv[2], sys.argv[3])
elif cmd == "show": show(sys.argv[2])
else: raise SystemExit("usage: set-key LANE KEY FILE | set-prompt LANE FILE | show LANE")
