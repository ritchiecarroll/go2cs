# coord-t24-resolve.py -- resolve a known train-24 conflict block in ONE file by a named rule, in place. Usage:
#   python coord-t24-resolve.py <file> <rule>   rules: theirs | both-ours-first | both-theirs-first
# Asserts exactly one conflict block, writes the file back byte-preserving CRLF/LF, prints line counts before/after.
import io, re, sys
path, rule = sys.argv[1], sys.argv[2]
raw = io.open(path, encoding="utf-8", newline="").read()
nl = "\r\n" if "\r\n" in raw else "\n"
m = re.search(r"<<<<<<< [^\r\n]*\r?\n(.*?)=======\r?\n(.*?)>>>>>>> [^\r\n]*\r?\n", raw, flags=re.S)
assert m, "no conflict block in " + path
assert raw.count("<<<<<<< ") == 1, "expected exactly one block, found %d" % raw.count("<<<<<<< ")
ours, theirs = m.group(1), m.group(2)
if rule == "theirs": body = theirs
elif rule == "both-ours-first": body = ours + theirs
elif rule == "both-theirs-first": body = theirs + ours
else: raise SystemExit("unknown rule " + rule)
out = raw[:m.start()] + body + raw[m.end():]
assert "<<<<<<<" not in out and ">>>>>>>" not in out and "\n=======\n" not in out.replace("\r\n", "\n"), "residual markers"
io.open(path, "w", encoding="utf-8", newline="").write(out)
print("%s: rule=%s ours=%d theirs=%d lines: with-markers=%d -> resolved=%d" % (path, rule, ours.count("\n"), theirs.count("\n"), raw.count("\n"), out.count("\n")))
