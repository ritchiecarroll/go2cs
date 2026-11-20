#!/usr/bin/env python3
"""i9 (COORD 2026-11-20 07:38Z): census of the standard-library names OQ2 un-Delta'd -- every name a stale runtime
root's package_info.cs can mis-name under -recurse=nuget before the precedence fix.

Reads src/go2cs/stdlib-metadata.txt (the embedded record, generated from src/core) at two refs through git's object
store: PRE (last record before OQ2, 2026-11-11) and POST (the cut's base). A Δ-rename is a GoTypeAlias whose target is
`Δ<Name>` or `const:Δ<Name>` for source `<Name>`. Reports, per package section, the renames present at PRE and absent at
POST (dropped by OQ2), and any still present at POST (none expected).
Usage: python -I census_oq2_renames.py <repo> <pre-ref> <post-ref>
"""
import re
import subprocess
import sys

repo, pre, post = sys.argv[1:4]
sys.stdout.reconfigure(encoding='utf-8')
ALIAS = re.compile(r'\[assembly: GoTypeAlias\("([^"]+)", "(const:)?Δ([^"]+)"\)\]')


def renames(ref):
    text = subprocess.run(['git', '-C', repo, 'show', f'{ref}:src/go2cs/stdlib-metadata.txt'],
                          capture_output=True).stdout.decode('utf-8')
    section, found = None, {}
    for line in text.splitlines():
        if line.startswith('##'):
            section = line[2:].strip()
            continue
        m = ALIAS.search(line)
        if m and section and m.group(1) == m.group(3):
            found.setdefault(section, set()).add((m.group(1), 'const' if m.group(2) else 'type'))
    return found


before, after = renames(pre), renames(post)
dropped = {s: sorted(v - after.get(s, set())) for s, v in before.items()}
dropped = {s: v for s, v in dropped.items() if v}
kept = {s: sorted(v) for s, v in after.items() if v}

total = sum(len(v) for v in dropped.values())
types = sum(1 for v in dropped.values() for _, k in v if k == 'type')
print(f'# PRE {pre[:10]}: {sum(len(v) for v in before.values())} Delta-renames in {len(before)} sections')
print(f'# POST {post[:10]}: {sum(len(v) for v in after.values())} Delta-renames in {len(after)} sections')
print(f'# dropped by OQ2: {total} ({types} types, {total - types} consts/vars) in {len(dropped)} sections')
for s in sorted(dropped):
    print(f'{s}\t' + ' '.join(f'{n}{"(const)" if k == "const" else ""}' for n, k in dropped[s]))
if kept:
    print('# STILL Delta-renamed at POST:')
    for s in sorted(kept):
        print(f'{s}\t' + ' '.join(n for n, _ in kept[s]))
