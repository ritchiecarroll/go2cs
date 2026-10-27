"""Pairing classifier for the sweep's real drift (COORD ruling 2026-10-27T23:27:49Z, class "test-refresh-1262").

Usage: python -I -X utf8 classify-drift.py <git diff -U0 file>

Each hunk's removed lines pair with its added lines in order; the counts must be equal. A pair is in the class
only if it is one of:
  (a) GOSTACK  -- the added line is the removed line with `, GoStack.Bounded` inserted before its final `);`
                  (the ruled `});` -> `}, GoStack.Bounded);`; any other ending is reported as a-other);
  (b) ELEM     -- every difference between the two lines is the text `elem` inserted directly after a `Ꮡ`
                  (`Ꮡ(x, i)` -> `Ꮡelem(x, i)`) or an `elem<...>` inserted there (`Ꮡelem<E>`);
  (c) RECV     -- a deferred or go method call's receiver moves from inside the lambda to an argument evaluated at
                  the statement (seat #4): `defer(() => RECV.M(ARGS), ref ᒐ)` -> `defer(ᴛ0 => ᴛ0.M(ARGS), RECV,
                  ref ᒐ)` and `goǃ(() => RECV.M(ARGS))` -> `goǃ(ᴛ0 => ᴛ0.M(ARGS), RECV)`, the indent, callee,
                  receiver text, method, arguments and trailing `ref ᒐ` all identical on both sides;
  (m) MAP      -- both lines carry `GoPositionMap` (the re-encode the shapes force).
A file whose name does not end in `_test.cs`, a hunk with unequal counts, or any other pair is OUTSIDE and the
classifier exits 1. Every pair is counted per file; nothing is sampled.
"""
import difflib, re, sys
from collections import Counter

ELEM_INS = re.compile(r'^elem(<[^()]*>)?$')
RECV_OLD = re.compile(r'^(\s*)(defer|goǃ)\(\(\) => (.+)\.(\w+)\(([^()]*)\)(, ref ᒐ)?\);$')
RECV_NEW = re.compile(r'^(\s*)(defer|goǃ)\(ᴛ0 => ᴛ0\.(\w+)\(([^()]*)\), (.+?)(, ref ᒐ)?\);$')


def recv_snapshot(rem, add):
    o, n = RECV_OLD.match(rem), RECV_NEW.match(add)
    return bool(o and n and o.group(1) == n.group(1) and o.group(2) == n.group(2) and o.group(3) == n.group(5)
                and o.group(4) == n.group(3) and o.group(5) == n.group(4) and o.group(6) == n.group(6))


def kind(rem, add):
    if 'GoPositionMap' in rem and 'GoPositionMap' in add:
        return 'm'
    if recv_snapshot(rem.rstrip(), add.rstrip()):
        return 'c'
    r, a = rem.rstrip(), add.rstrip()
    if r.endswith(');') and a == r[:-2] + ', GoStack.Bounded);':
        return 'a' if r.endswith('});') else 'a-other'
    sm = difflib.SequenceMatcher(None, r, a, autojunk=False)
    ops = [op for op in sm.get_opcodes() if op[0] != 'equal']
    if ops and all(t == 'insert' and r[i1 - 1:i1] == 'Ꮡ' and ELEM_INS.match(a[j1:j2]) for t, i1, i2, j1, j2 in ops):
        return 'b'
    return None


def main(path):
    files = {}
    outside = []
    cur = None
    rem, add = [], []

    def flush():
        if cur is None or (not rem and not add):
            return
        if len(rem) != len(add):
            outside.append((cur, f'hunk counts -{len(rem)} +{len(add)}', rem[:1], add[:1]))
        else:
            for r, a in zip(rem, add):
                k = kind(r, a)
                if k is None:
                    outside.append((cur, 'pair', r, a))
                else:
                    files[cur][k] += 1
        rem.clear(); add.clear()

    for ln in open(path, encoding='utf-8').read().splitlines():
        if ln.startswith('diff --git '):
            flush()
            cur = ln.split(' b/', 1)[1]
            files[cur] = Counter()
            if not cur.endswith('_test.cs'):
                outside.append((cur, 'not a test file', '', ''))
        elif ln.startswith('@@'):
            flush()
        elif ln.startswith('---') or ln.startswith('+++'):
            continue
        elif ln.startswith('-'):
            rem.append(ln[1:])
        elif ln.startswith('+'):
            add.append(ln[1:])
    flush()

    total = Counter()
    for f, c in files.items():
        total.update(c)
        print(f"{f}\t" + ' '.join(f"{k}={c[k]}" for k in ('a', 'a-other', 'b', 'c', 'm') if c[k]))
    print(f"FILES {len(files)}  PAIRS a={total['a']} a-other={total['a-other']} b={total['b']} c={total['c']} m={total['m']}  OUTSIDE {len(outside)}")
    for f, why, r, a in outside:
        print(f"OUTSIDE {f}: {why}\n  - {r}\n  + {a}")
    return 1 if outside else 0


if __name__ == '__main__':
    sys.exit(main(sys.argv[1]))
