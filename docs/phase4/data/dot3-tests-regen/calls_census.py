"""Census: per-call-table (GoPositionMap field 6) payloads LOST between two refs, keyed by (goFile, payload) with the
C#-line key stripped (a layout move re-keys an entry; only a payload that disappears is a loss). Usage:
python -I calls_census.py <repo> <refA> <refB|WORK> <outfile>"""
import collections, re, subprocess, sys

repo, ra, rb, outp = sys.argv[1:5]
QS = re.compile(r'"((?:[^"\\]|\\.)*)"')

def names(ref):
    if ref == 'WORK':
        r = subprocess.run(['git', '-C', repo, 'diff', '--name-only', ra, '--', '*package_info*.cs', '*package_test_info.cs'], capture_output=True)
    else:
        r = subprocess.run(['git', '-C', repo, 'diff', '--name-only', ra, ref, '--', '*package_info*.cs', '*package_test_info.cs'], capture_output=True)
    return [l for l in r.stdout.decode('utf-8').split('\n') if l.strip()]

def read(ref, path):
    if ref == 'WORK':
        try:
            return open(repo + '/' + path, encoding='utf-8').read()
        except FileNotFoundError:
            return ''
    r = subprocess.run(['git', '-C', repo, 'show', ref + ':' + path], capture_output=True)
    return r.stdout.decode('utf-8', 'replace')

def payloads(text):
    c = collections.Counter()
    for line in text.split('\n'):
        if 'GoPositionMap(' not in line:
            continue
        q = QS.findall(line)
        if len(q) > 5 and q[5]:
            for group in q[5].split(';'):
                key, _, ents = group.partition('=')
                c[(q[0], ents)] += 1
    return c

out = []; lost = 0; gained = 0
for p in names(rb):
    a, b = payloads(read(ra, p)), payloads(read(rb, p))
    for k in a:
        if a[k] > b.get(k, 0):
            lost += a[k] - b.get(k, 0); out.append(f'LOST  {p}  {k[0]}  {k[1]}')
    for k in b:
        if b[k] > a.get(k, 0):
            gained += b[k] - a.get(k, 0); out.append(f'GAINED {p}  {k[0]}  {k[1]}')
out.insert(0, f'{ra}..{rb}: per-call payloads lost {lost}, gained {gained}')
open(outp, 'w', encoding='utf-8').write('\n'.join(out))
