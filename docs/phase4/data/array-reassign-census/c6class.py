# c6class.py <v2 tsv...> -- P5's C6: split the `*p = ...` sites whose p is a (*[N]T)(unsafe.Pointer(...)) result
# (door column UNSAFE-CONV) by the pointer's array type as the source spells it: an unnamed `*(*[N]T)(...)`, a
# named `*(*N)(...)`, or through a variable. Reads each site's source line; a `$GOMODCACHE/` path is expanded from
# `go env GOMODCACHE`. Run with `py -3 -I`.
import collections
import re
import subprocess
import sys

sys.stdout.reconfigure(encoding='utf-8')
modcache = subprocess.run(['go', 'env', 'GOMODCACHE'], capture_output=True, text=True, check=True).stdout.strip()

for tsv in sys.argv[1:]:
    c = collections.Counter()
    other = []
    for row in open(tsv, encoding='utf-8'):
        f = row.rstrip('\n').split('\t')
        if len(f) < 8 or f[7] != 'UNSAFE-CONV':
            continue
        path, line = f[2].rsplit(':', 1)
        if path.startswith('$GOMODCACHE/'):
            path = modcache + '/' + path[len('$GOMODCACHE/'):]
        src = open(path, encoding='utf-8').read().split('\n')[int(line) - 1].strip()
        if src.startswith('*(*['):
            c['unnamed-direct'] += 1
        elif re.match(r'^\*\(\*[A-Za-z_]', src):
            c['named-direct'] += 1
            other.append(src)
        else:
            c['via-var'] += 1
            other.append(src)
    print(tsv, dict(c))
    for s in other[:8]:
        print('   ', s[:120])
