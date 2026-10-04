#!/usr/bin/env python3
"""Prototype of PLAN-ide-mode E9 option (b): decode a converted package's GoPositionMap records and write a
transient copy of one converted .cs file carrying #line directives back to its Go file.

usage: linedirect.py <package_info.cs> <converted .cs> <output .cs> [--mode reanchor|hidden|naive] [--goroot DIR] [--skip-comment-records]

A standard-library record names its Go file relative to GOROOT/src; --goroot resolves it.

The table semantics (positionMapOperations.go): one record per mapped C# line, ascending; every C# line up to the next
record answers the earlier record's Go line. Option (b) sees ONLY that table, so it cannot tell a statement's
continuation lines from scaffolding between statements:
  reanchor: every line after a record is re-anchored (`#line G`) to that record's Go line;
  hidden:   a record's own line is mapped (`#line G`), the lines after it are `#line hidden` until the next record.
Lines before the first record are scaffolding: `#line default` then `#line hidden` (diagnostics stay on the .cs).
"""
import base64, hashlib, re, sys

def decode(table):
    data, i, cs, go, out = base64.b64decode(table), 0, 0, 0, []
    def uvarint():
        nonlocal i
        shift = value = 0
        while True:
            b = data[i]; i += 1
            value |= (b & 0x7F) << shift
            if b < 0x80: return value
            shift += 7
    while i < len(data):
        b = data[i]
        if b & 0x80:
            advance, zz = (b >> 4) & 0x07, b & 0x0F; i += 1
        elif b == 0:
            i += 1; advance = uvarint(); zz = uvarint()
        else:
            raise ValueError(f"corrupt table byte {b:#x} at {i}")
        cs += advance + 1
        go += (zz >> 1) ^ -(zz & 1)
        out.append((cs, go))
    return out

def records(info_text):
    rx = re.compile(r'GoPositionMap\("((?:[^"\\]|\\.)*)",\s*"((?:[^"\\]|\\.)*)",\s*"([A-Za-z0-9+/=]*)"')
    return {m.group(2): (m.group(1).replace('\\\\', '\\'), decode(m.group(3))) for m in rx.finditer(info_text)}

def raw_string_lines(lines):
    """Lines inside a multi-line raw (\"\"\") or verbatim (@\") string: never put a directive before them."""
    inside, odd = set(), False
    for n, line in enumerate(lines, 1):
        if odd: inside.add(n)
        if line.count('"""') % 2 == 1: odd = not odd
    return inside

def main():
    info, src, dst = sys.argv[1:4]
    mode = sys.argv[sys.argv.index('--mode') + 1] if '--mode' in sys.argv else 'reanchor'
    recs = records(open(info, encoding='utf-8').read())
    name = src.replace('\\', '/').rsplit('/', 1)[-1]
    if name not in recs: sys.exit(f"no GoPositionMap record for {name}")
    gofile, table = recs[name]
    skipcomments = '--skip-comment-records' in sys.argv
    if '--goroot' in sys.argv and not gofile.startswith('/') and ':' not in gofile:
        gofile = sys.argv[sys.argv.index('--goroot') + 1].rstrip('/') + '/src/' + gofile
    golines = dict(table)
    lines = open(src, encoding='utf-8').read().split('\n')
    if skipcomments:
        # A record whose C# line holds only a comment (the converter puts a leading comment on the record's line)
        # moves to the next line with code, unless that line has a record of its own.
        moved = {}
        for c, g in sorted(golines.items()):
            n = c
            while n <= len(lines) and (lines[n-1].strip().startswith('//') or not lines[n-1].strip()) and (n == c or n not in golines):
                n += 1
            moved[n if n <= len(lines) and n not in moved and (n == c or n not in golines) else c] = g
        golines = moved
    unsafe = raw_string_lines(lines)
    digest = hashlib.sha256(open(gofile, 'rb').read()).hexdigest()
    out = [f'#pragma checksum "{gofile}" "{{8829d00f-11b8-4213-878b-770e8597ac16}}" "{digest}"']
    current, state = None, None
    for n, line in enumerate(lines, 1):
        directive = None
        if n in golines:
            current = golines[n]; directive = f'#line {current} "{gofile}"'; state = 'mapped'
        elif current is None:
            if state != 'scaffold': directive = '#line default\n#line hidden'; state = 'scaffold'
        elif mode == 'naive':
            pass  # the plan's counter-example: directives before records only, so later lines count N+1, N+2, ...
        elif mode == 'reanchor':
            directive = f'#line {current} "{gofile}"'
        elif state == 'mapped':
            directive = '#line hidden'; state = 'hidden'
        if directive and n not in unsafe and not line.lstrip().startswith('#'):
            out.append(directive)
        out.append(line)
    open(dst, 'w', encoding='utf-8').write('\n'.join(out))
    print(f"{name}: {len(table)} records, mode {mode}, go file {gofile}")

if __name__ == '__main__':
    main()
