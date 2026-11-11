#!/usr/bin/env python3
"""The linux/amd64 versus linux/arm64 file delta of the roster's linux rows, under the corpus build tags.

For every row of the 1.26.10.3 linux evidence it asks `go list` which Go files the package selects at
GOOS=linux with GOARCH=amd64 and with GOARCH=arm64, under `-tags purego,math_big_pure_go` (the tags a
`-stdlib` and a `-tests` conversion load with), and prints:

  - the rows whose TEST files differ, with the top-level Test functions each side alone declares. Those
    names are the verdicts a linux-arm64 reading is predicted to lack (a floor: subtests are not counted);
  - the rows whose SOURCE files differ.

Nothing is built or run for the target. Pins: go1.26.10 first on PATH, GOTOOLCHAIN=local.

    python3 filedelta.py            (from this folder)
"""
import glob
import io
import json
import os
import re
import subprocess

HERE = os.path.dirname(os.path.abspath(__file__))
EVIDENCE = os.path.join(HERE, '..', '..', 'rebank-1.26.10.3', 'linux')
TAGS = 'purego,math_big_pure_go'
FUNC = re.compile(r'^func (Test[A-Z0-9_]\w*|Test)\(', re.M)


def roster_rows():
    rows = set()
    for path in glob.glob(os.path.join(EVIDENCE, '*.tsv')):
        if path.endswith('.ledger.tsv'):
            continue
        lines = [l for l in io.open(path, encoding='utf-8').read().replace('\r', '').split('\n') if l and not l.startswith('#')]
        for line in lines[1:]:
            rows.add(line.split('\t')[0])
    return sorted(rows)


def go_list(goroot, arch, rows):
    env = dict(os.environ, GOOS='linux', GOARCH=arch, GOTOOLCHAIN='local', GOFLAGS='')
    fields = '-json=ImportPath,GoFiles,CgoFiles,TestGoFiles,XTestGoFiles,Error'
    p = subprocess.run(['go', 'list', '-e', '-tags', TAGS, fields] + rows, env=env, capture_output=True, text=True, cwd=os.path.join(goroot, 'src'))
    if p.returncode != 0:
        raise SystemExit('go list at %s: rc=%d %s' % (arch, p.returncode, p.stderr[:400]))
    out, dec, s, i = {}, json.JSONDecoder(), p.stdout, 0
    while i < len(s):
        while i < len(s) and s[i].isspace():
            i += 1
        if i >= len(s):
            break
        o, i = dec.raw_decode(s, i)
        if o.get('Error'):
            raise SystemExit('go list at %s: %s: %s' % (arch, o['ImportPath'], o['Error']))
        out[o['ImportPath']] = o
    return out


def test_names(goroot, pkg, files):
    d = os.path.join(goroot, 'src', pkg.replace('/', os.sep))
    return [m for f in files for m in FUNC.findall(io.open(os.path.join(d, f), encoding='utf-8').read())]


def main():
    goroot = subprocess.run(['go', 'env', 'GOROOT'], capture_output=True, text=True).stdout.strip()
    rows = roster_rows()
    amd, arm = go_list(goroot, 'amd64', rows), go_list(goroot, 'arm64', rows)
    print('rows=%d amd64=%d arm64=%d tags=%s' % (len(rows), len(amd), len(arm), TAGS))
    test_rows, src_rows, names = [], [], 0
    for k in rows:
        a, r = amd[k], arm[k]
        ta = set((a.get('TestGoFiles') or []) + (a.get('XTestGoFiles') or []))
        tr = set((r.get('TestGoFiles') or []) + (r.get('XTestGoFiles') or []))
        sa = set((a.get('GoFiles') or []) + (a.get('CgoFiles') or []))
        sr = set((r.get('GoFiles') or []) + (r.get('CgoFiles') or []))
        if ta != tr:
            na, nr = test_names(goroot, k, sorted(ta - tr)), test_names(goroot, k, sorted(tr - ta))
            names += len(na) + len(nr)
            test_rows.append('%s\tamd64-only=%s\t%s\tarm64-only=%s\t%s' % (k, ','.join(sorted(ta - tr)), ';'.join(na), ','.join(sorted(tr - ta)), ';'.join(nr)))
        if sa != sr:
            src_rows.append('%s\tamd64-only=%s\tarm64-only=%s' % (k, ','.join(sorted(sa - sr)), ','.join(sorted(sr - sa))))
    print('rows with a test-file delta: %d (top-level Test names on one side only: %d)' % (len(test_rows), names))
    print('\n'.join(test_rows))
    print('rows with a source-file delta: %d' % len(src_rows))
    print('\n'.join(src_rows))


if __name__ == '__main__':
    main()
