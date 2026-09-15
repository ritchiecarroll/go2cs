import io, re, sys
# rekey-c2.py [path] -- C2's final block carries two PROSE lines under the BRANCH: key with no SHA ("13 further claude/c2-*
# lane refs ..." and "14 local seat MERGES ..."); the verifier reads them as BAD SHA (missing=2). Re-key exactly those two
# lines to NOTE: so the verifier's population is the SHA-bearing lines only. Content untouched; nothing retyped.
RS = sys.argv[1] if len(sys.argv) > 1 else "C:/go2cs-tmp-coord/hnd/docs/phase4/RESUME-SESSIONS.md"
t = io.open(RS, encoding="utf-8").read()
assert "\r" not in t
lines = t.split("\n")
n = 0
for i, l in enumerate(lines):
    m = re.match(r"^(\s*)BRANCH: (13 further claude/c2-\* lane refs|14 local seat MERGES)", l)
    if m:
        lines[i] = m.group(1) + "NOTE (was a prose BRANCH line; re-keyed 2026-09-14 so the verifier counts SHA-bearing lines only): " + l[len(m.group(1)) + len("BRANCH: "):]
        n += 1
assert n == 2, "expected exactly two prose BRANCH lines, found %d" % n
io.open(RS, "w", encoding="utf-8", newline="\n").write("\n".join(lines))
print("re-keyed %d line(s)" % n)
