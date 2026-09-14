import ast, io, re
src = io.open("C:/Projects/go2cs/.claude/coord-scripts/coord-derive-train30.py", encoding='utf-8').read()
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
rows = [t for t in ast.literal_eval(src[i:end]) if t[0] != 'C2Q44']
out = "~/AppData/Local/Temp/dropseats.txt"
with io.open(out, 'w', encoding='utf-8', newline='\n') as f:
    for t in rows:
        f.write('|'.join([t[0], t[1], t[2], t[3]]) + '\n')
print('wrote', len(rows), 'seats to', out)
