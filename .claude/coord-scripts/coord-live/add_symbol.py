import json
p = 'src/core/go2cs/symbols.json'
raw = open(p, encoding='utf-8', newline='').read()
nl = '\r\n' if '\r\n' in raw else '\n'
L = raw.split(nl)
if any('"name": "TypedNilUnsafePointerAccessor"' in l for l in L):
    raise SystemExit('already present')
i = [k for k, l in enumerate(L) if '"name": "TypedNilFuncAccessor"' in l][0]
j = i
while L[j].strip() != '},':
    j += 1
def ind(s):
    return s[:len(s) - len(s.lstrip())]
ind_obj = ind(L[i - 1]); ind_key = ind(L[i])
ci = [k for k in range(i, j) if L[k].strip().startswith('"TypedNilFuncAccessor is')][0]
ind_item = ind(L[ci])
lines = [
    "TypedNilUnsafePointerAccessor is the STATIC method on the hand-owned unsafe package's Pointer class",
    "that an unsafe.Pointer takes on its way into an EMPTY interface - the third arm of the typed-nil",
    "boundary, beside TypedNilBoxAccessor and TypedNilFuncAccessor. `unsafe.Pointer` is a CLASS",
    "(Pointer : StandardBox<uintptr>), so a nil one renders a C# null that boxes as nothing; this",
    "substitutes the class's canonical nil instance and is the identity for every real Pointer. It is",
    "a static call rather than an extension because the pointer arm's `OrTypedNil()` binds ж<uintptr>",
    "and would answer ж<uintptr>'s nil box, a different dynamic type. No `()`: the caller supplies the",
    "argument list.",
]
block = [ind_obj + '{', ind_key + '"name": "TypedNilUnsafePointerAccessor",', ind_key + '"value": "OrTypedNil",', ind_key + '"comment": [']
block += [ind_item + json.dumps(s, ensure_ascii=False) + (',' if n < len(lines) - 1 else '') for n, s in enumerate(lines)]
block += [ind_key + '],',
          ind_key + '"note": "hand-owned unsafe.Pointer static method applied at every unsafe.Pointer-into-`any` boxing site; emitted by the converter only, so no Symbols.cs projection."',
          ind_obj + '},']
L = L[:j + 1] + block + L[j + 1:]
json.loads(nl.join(L))
open(p, 'w', encoding='utf-8', newline='').write(nl.join(L))
print('inserted after line', j + 1)
