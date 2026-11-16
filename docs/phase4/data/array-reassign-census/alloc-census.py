# alloc-census.py <goroot> <pkg...> -- RA-4's census: every test function in the named packages' _test.go files whose body
# calls testing.AllocsPerRun (the alloc-assert rows), by package. A body is from `func TestX(` to the next top-level func.
import os, re, sys
sys.stdout.reconfigure(encoding='utf-8')
goroot, pkgs = sys.argv[1], sys.argv[2:]
func_re = re.compile(r'^func (\(\w+ \*?\w+\) )?(\w+)\(', re.M)
total = 0
for pkg in pkgs:
    d = os.path.join(goroot, 'src', pkg)
    if not os.path.isdir(d):
        print(f'{pkg}: MISSING'); continue
    rows = []
    for f in sorted(os.listdir(d)):
        if not f.endswith('_test.go'):
            continue
        src = open(os.path.join(d, f), encoding='utf-8', errors='replace').read()
        starts = [(m.start(), m.group(2)) for m in func_re.finditer(src)]
        for i, (at, name) in enumerate(starts):
            end = starts[i + 1][0] if i + 1 < len(starts) else len(src)
            if 'AllocsPerRun' in src[at:end]:
                rows.append(f'{f}:{name}')
    total += len(rows)
    print(f'{pkg}: {len(rows)}' + ('' if not rows else '  ' + ', '.join(rows)))
print('TOTAL', total)
