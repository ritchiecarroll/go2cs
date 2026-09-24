# The UTF-16 carve-out (DESIGN-string-literal-allocation §8.1R3.2; COORD's revision-3 note, fix 7).
# A string literal that is a WHOLE element of a tuple return or a tuple assignment is emitted as a UTF-16
# C# literal (a u8 span cannot be a ValueTuple element: visitReturnStmt.go:349-354,
# visitAssignStmt.go:1700-1703), so it converts through the counted `operator @string(string)`
# (string.cs:450), which a table keyed on u8 addresses cannot serve. The empty literal is not counted
# (AllocationCounter.Utf8ToBytes counts only a non-empty value), so it is not listed.
#
# usage, from src/core:  python3 utf16-tuple-sites.py > utf16-tuple-sites.txt
# Scope: git-tracked production .cs (no *_test.cs, no package_test_info.cs, no golib, no testdata), one
# line per statement. A literal that is an OPERAND inside an element (`"[" + host + "]"`) is not counted
# here: the regex cannot separate it from a cast or a nested call, so this is a LOWER bound.
import re, subprocess

files = [f for f in subprocess.run(['git', 'ls-files', '-z', '--', '*.cs'], capture_output=True, check=True)
         .stdout.decode('utf-8').split('\0')
         if f and not re.search(r'(_test\.cs$|package_test_info|(^|/)golib/|testdata)', f)]
elem = re.compile(r'^"((?:[^"\\]|\\.)+)"$')
ret = re.compile(r'^\s*return \((.*)\);\s*$')
asg = re.compile(r'^\s*\((?:[^()]|\([^()]*\))*\) = \((.*)\);\s*$')


def split(s):
    out, depth, cur, quoted, esc = [], 0, '', False, False
    for ch in s:
        if quoted:
            cur += ch
            if esc: esc = False
            elif ch == '\\': esc = True
            elif ch == '"': quoted = False
            continue
        if ch == '"': quoted = True; cur += ch; continue
        if ch in '([{<': depth += 1
        if ch in ')]}>': depth -= 1
        if ch == ',' and depth == 0: out.append(cur.strip()); cur = ''; continue
        cur += ch
    out.append(cur.strip())
    return out


rows = []
for f in files:
    flavour = next((g for g in ('linux', 'darwin', 'windows') if f'/{g}/' in f), 'flat')
    for i, line in enumerate(open(f, encoding='utf-8', errors='replace'), 1):
        for kind, rx in (('return', ret), ('assign', asg)):
            m = rx.match(line)
            n = sum(1 for e in split(m.group(1)) if elem.match(e)) if m else 0
            if n:
                rows.append((flavour, kind, f, i, n))

for g in ('windows', 'linux', 'darwin'):
    mine = [r for r in rows if r[0] in ('flat', g)]
    print(f"{g}\tstatements {len(mine)}\tliterals {sum(r[4] for r in mine)}")
for flavour, kind, f, i, n in rows:
    print(f"{flavour}\t{kind}\t{f}:{i}\t{n}")
