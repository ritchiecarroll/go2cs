# Resolve every conflict hunk in the given files by taking the incoming ("theirs") side of that hunk only.
import sys
for p in sys.argv[1:]:
    raw = open(p, encoding='utf-8', newline='').read()
    nl = '\r\n' if '\r\n' in raw else '\n'
    out, state, n = [], None, 0
    for l in raw.split(nl):
        if l.startswith('<<<<<<< '): state = 'ours'; n += 1; continue
        if state and l == '=======': state = 'theirs'; continue
        if state and l.startswith('>>>>>>> '): state = None; continue
        if state == 'ours': continue
        out.append(l)
    open(p, 'w', encoding='utf-8', newline='').write(nl.join(out))
    print(f'{p}: {n} hunk(s) -> theirs')
