import io, re, sys

def edit(path, pairs):
    raw = open(path, encoding='utf-8', newline='').read()
    for old, new in pairs:
        n = raw.count(old)
        if n != 1:
            sys.exit(f'{path}: expected 1 of {old[:70]!r}, found {n}')
        raw = raw.replace(old, new)
    open(path, 'w', encoding='utf-8', newline='').write(raw)
    print('edited', path)

R = 'docs/ValidatedTestPackages.md'
raw = open(R, encoding='utf-8', newline='').read()
nl = '\r\n' if '\r\n' in raw else '\n'

row = ('| [`reflect`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/reflect) | 395 | 23 | '
       "Go's run-time reflection, the reflection bridge's own suite: `TypeOf`/`ValueOf` over every kind, "
       '`Kind` and `String` for named, generic and function-local types, struct fields with tags, embedding '
       'and `VisibleFields`, method sets and `Method`/`MethodByName` calls, `Call` and `MakeFunc`, `Set`/`Addr`/`CanSet` '
       'and the settability rules, `Convert`/`CanConvert`, `DeepEqual`, `IsZero` and `Comparable`, maps with '
       '`MapIndex`, `SetMapIndex` and `MapIter`, channels with `Select`, `Copy` and `Swapper`, the `Seq`/`Seq2` '
       'iterators, and the types built at run time by `PointerTo`, `SliceOf`, `ArrayOf`, `MapOf`, `ChanOf`, `FuncOf` '
       'and `StructOf`. The 23 disclosures are runtime capabilities the managed runtime does not have (a pointer compared '
       'as a number, GC bitmaps, address arithmetic, write-protected memory, an assembly trampoline), stack-allocation '
       'counts, and one frame-liveness assert. '
       '· linux: 395 + 23 · [proof](validation/current/reflect.md) |')
anchor = '| [`regexp`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/regexp) |'
if raw.count(anchor) != 1:
    sys.exit('regexp row anchor not unique')
i = raw.index(anchor)
raw = raw[:i] + row + nl + raw[i:]
open(R, 'w', encoding='utf-8', newline='').write(raw)

edit(R, [
    ('> ### Phase 4 progress: **219 / 230 testable packages validated — 95.2%**',
     '> ### Phase 4 progress: **220 / 230 testable packages validated — 95.7%**'),
    ('> **58,366 matching test verdicts · 278 disclosed** *(updated 2026-09-26',
     '> **58,761 matching test verdicts · 301 disclosed** *(updated 2026-09-27'),
    ('> **Against the implementable set (230 − 6 excluded = 224): 219 / 224 — 97.8%.**',
     '> **Against the implementable set (230 − 6 excluded = 224): 220 / 224 — 98.2%.**'),
    ('> **Linux: 216 of 217 applicable rows validated at their Linux counts** — 57,298 matching verdicts · 297 disclosed',
     '> **Linux: 217 of 218 applicable rows validated at their Linux counts** — 57,693 matching verdicts · 320 disclosed'),
])

raw = open(R, encoding='utf-8', newline='').read()
anchor2 = '`net/http` has since re-entered: it banked at 1,387 of 1,387 with TRAIN A on 2026-09-25'
j = raw.index(anchor2)
k = raw.index(nl + nl, j)
note = (nl + nl + '`reflect` has since banked: 395 of 418 matching with 23 disclosed, with TRAIN E on 2026-09-27, which' + nl +
        'leaves four candidates. This block\'s identities and its candidates table read as they stood at the close.')
raw = raw[:k] + note + raw[k:]
open(R, 'w', encoding='utf-8', newline='').write(raw)
print('roster: row, header and candidates note')

edit('docs/README.md', [
    ('**219 of the 230 testable standard-library\npackages (95.2%)'.replace('\n', nl),
     '**220 of the 230 testable standard-library\npackages (95.7%)'.replace('\n', nl)),
    ('it validates on master since\n2026-09-25 (1,387 verdicts). The'.replace('\n', nl),
     'it validates on master since\n2026-09-25 (1,387 verdicts), and `reflect` validates since 2026-09-27. The'.replace('\n', nl)),
])

I = 'docs/validation/index.md'
iraw = open(I, encoding='utf-8', newline='').read()
inl = '\r\n' if '\r\n' in iraw else '\n'
ianchor = '| `regexp` | [`regexp.md`](current/regexp.md) |'
if iraw.count(ianchor) != 1:
    sys.exit('index anchor not unique')
line = '| `reflect` | [`reflect.md`](current/reflect.md) | [`src/core/reflect`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/reflect) |'
p = iraw.index(ianchor)
iraw = iraw[:p] + line + inl + iraw[p:]
open(I, 'w', encoding='utf-8', newline='').write(iraw)
print('index: reflect line')
