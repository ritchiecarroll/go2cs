import ast, io, re
src = io.open("C:/Projects/go2cs/.claude/coord-scripts/coord-derive-train31.py", encoding='utf-8').read()
m = re.search(r'SEATS\s*=\s*\[', src)
i = m.end() - 1
d = 0
for j in range(i, len(src)):
    if src[j] == '[':
        d += 1
    elif src[j] == ']':
        d -= 1
        if d == 0:
            end = j + 1
            break
rows = [list(t[:4]) for t in ast.literal_eval(src[i:end])]
# CANDIDATES -- not seats in the derive script, but part of the intended train.
# The earlier rehearsal walked SEATS only and was blind to every one of these.
rows += [
    ["SUBDOC14", "claude/coord-subdoc14", "b96b26366", "coord-merge-subdoc14.txt"],
    ["GRFK", "claude/g-roster-figure-kind", "0632e9bba", "coord-merge-g-roster-figure-kind.txt"],
    # GUTF REMOVED: superseded -- its one-line change was re-cut as the os bank's first commit
    #   and landed; re-merging the branch yields a PHANTOM conflict against a superset file.
    ["GGME", "claude/g-guard-manifest-enum", "314bb2b9b", "coord-merge-subdoc14.txt"],
    ["GMISS", "claude/g-misspath-board", "4e6d14937", "coord-merge-g-misspath-board.txt"],
]
out = "~/AppData/Local/Temp/dropseats31.txt"
with io.open(out, 'w', encoding='utf-8', newline='\n') as f:
    for t in rows:
        f.write('|'.join(t) + '\n')
print('rehearsal list:', len(rows), '(16 seats + 4 candidates with drafted messages)')
