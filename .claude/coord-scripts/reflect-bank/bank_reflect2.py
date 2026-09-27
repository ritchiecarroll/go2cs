import sys
P = 'docs/README.md'
raw = open(P, encoding='utf-8', newline='').read()
nl = '\r\n' if '\r\n' in raw else '\n'
pairs = [
    ('**219 of the 230 testable standard-library' + nl + 'packages (95.2%)',
     '**220 of the 230 testable standard-library' + nl + 'packages (95.7%)'),
    ('it validates on master since' + nl + '2026-09-25 (1,387 verdicts). The',
     'it validates on master since' + nl + '2026-09-25 (1,387 verdicts), and `reflect` validates since 2026-09-27. The'),
]
for old, new in pairs:
    if raw.count(old) != 1:
        sys.exit(f'README: {raw.count(old)} of {old[:60]!r}')
    raw = raw.replace(old, new)
open(P, 'w', encoding='utf-8', newline='').write(raw)
print('README edited')

I = 'docs/validation/index.md'
iraw = open(I, encoding='utf-8', newline='').read()
inl = '\r\n' if '\r\n' in iraw else '\n'
anchor = '| `regexp` | [`regexp.md`](current/regexp.md) |'
line = '| `reflect` | [`reflect.md`](current/reflect.md) | [`src/core/reflect`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/reflect) |'
if iraw.count(anchor) != 1 or line in iraw:
    sys.exit('index anchor problem')
p = iraw.index(anchor)
open(I, 'w', encoding='utf-8', newline='').write(iraw[:p] + line + inl + iraw[p:])
print('index edited')
