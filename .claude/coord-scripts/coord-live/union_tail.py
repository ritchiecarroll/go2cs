# Resolve a single both-appended conflict: ours, then a closing brace for ours' last member, then theirs.
import sys
p = sys.argv[1]
raw = open(p, encoding='utf-8', newline='').read()
nl = '\r\n' if '\r\n' in raw else '\n'
L = raw.split(nl)
a = [i for i, l in enumerate(L) if l.startswith('<<<<<<< ')]
m = [i for i, l in enumerate(L) if l == '=======']
b = [i for i, l in enumerate(L) if l.startswith('>>>>>>> ')]
assert len(a) == len(m) == len(b) == 1, (len(a), len(m), len(b))
ours = L[a[0] + 1:m[0]]; theirs = L[m[0] + 1:b[0]]
open(p, 'w', encoding='utf-8', newline='').write(nl.join(L[:a[0]] + ours + ['    }', ''] + theirs + L[b[0] + 1:]))
print(f'{p}: ours {len(ours)} + theirs {len(theirs)}')
