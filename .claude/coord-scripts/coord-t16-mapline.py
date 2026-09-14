import io, re, sys, difflib
emit_p = sys.argv[1]; cur_p = sys.argv[2]
emit = io.open(emit_p, encoding='utf-8-sig', newline='').read()
cur = io.open(cur_p, encoding='utf-8-sig', newline='').read()
pat = re.compile(r'\[assembly: [^\r\n]*GoPositionMap\("syscall/syscall_linux\.go"[^\r\n]*')
e = pat.findall(emit); c = pat.findall(cur)
print("matches emission/committed:", len(e), len(c))
assert len(e) == 1 and len(c) == 1, (len(e), len(c))
if e[0] == c[0]:
    print("map line already identical -- nothing to apply"); sys.exit(0)
new = cur.replace(c[0], e[0], 1); assert new.count(e[0]) == 1
io.open(cur_p, 'w', encoding='utf-8', newline='').write(new)
d = [l for l in difflib.unified_diff(cur.splitlines(), new.splitlines(), lineterm='', n=0) if l.startswith(('+', '-')) and not l.startswith(('+++', '---'))]
print("applied delta lines:", len(d), "| all syscall_linux.go map lines:", all('GoPositionMap("syscall/syscall_linux.go"' in l for l in d))
allpat = re.compile(r'\[assembly: [^\r\n]*GoPositionMap\([^\r\n]*')
em = set(allpat.findall(emit)); cm = set(allpat.findall(new))
print("map lines still differing from the fresh emission (left to the regen):", len(cm - em))
