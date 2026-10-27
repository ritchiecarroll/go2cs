#!/usr/bin/env python3
"""classify_test_refresh.py -- classify a corpus drift diff against "the .2 test-artifact refresh" class.

COORD's ruling (2026-10-27) names the class test-refresh-1262: the converted *_test.cs artifacts were not regenerated
after two converter seats changed emission inside converted test sources. Exactly two line shapes belong to it:

  (a) seat #3:  a go statement gains a trailing GoStack.Bounded argument
                    `});`                  -> `}, GoStack.Bounded);`
                    `goǃ(f, a, b);`        -> `goǃ(f, a, b, GoStack.Bounded);`
  (b) seat #10: an element-address compare stops boxing
                    `Ꮡ(b, 0) != Ꮡ(got, 0)` -> `Ꮡelem(b, 0) != Ꮡelem(got, 0)`   (also `Ꮡelem<T>(...)`)

Input: a `git diff --ignore-cr-at-eol -U0 -- src/core` file (UTF-8). Each hunk's removed lines are paired in order with
its added lines; a pair belongs to the class only if undoing the shape(s) on the added line gives back the removed
line EXACTLY. Anything else is printed as UNCLASSIFIED and is a STOP:
  - a pair that differs in any other way (including any GoPositionMap re-encode -- none has been observed; if one
    appears it is reported here for a human to rule, never absorbed);
  - a hunk whose removed and added line counts differ;
  - any drifted file that is not a *_test.cs (production drift is never this class);
  - a `p.Ꮡelem<E>(i)` method form: its pre-refresh spelling has not been observed, so it is not normalized here.

Usage: py -3 -I classify_test_refresh.py <diff-file> [<diff-file> ...]
Prints one line per file: <path> test-refresh-1262 a=<n> b=<n>   or   <path> UNCLASSIFIED <reason>,
then a TOTAL line. Exit 0 = every drifted line is in the class; 1 = at least one UNCLASSIFIED; 2 = usage/no input.
"""
import re
import sys

sys.stdout.reconfigure(encoding='utf-8')

BOUNDED = re.compile(r', GoStack\.Bounded\)')
BOUNDED_ONLY = re.compile(r'\(GoStack\.Bounded\)')
ELEM = re.compile(r'(?<![\w.])Ꮡelem(?:<[^<>()]*>)?\(')


def undo_a(line):
    out = BOUNDED.sub(')', line)
    return BOUNDED_ONLY.sub('()', out)


def undo_b(line):
    return ELEM.sub('Ꮡ(', line)


def classify(paths):
    files = {}      # path -> {'a': n, 'b': n, 'bad': [reasons]}
    cur = None
    rem, add = [], []

    def flush():
        if cur is None or (not rem and not add):
            return
        f = files[cur]
        if len(rem) != len(add):
            f['bad'].append(f'hunk with {len(rem)} removed vs {len(add)} added lines')
            return
        for r, a in zip(rem, add):
            if undo_a(a) == r:
                f['a'] += 1
            elif undo_b(a) == r:
                f['b'] += 1
            elif undo_b(undo_a(a)) == r:
                f['a'] += 1
                f['b'] += 1
            else:
                f['bad'].append('pair outside the class: -' + r.strip() + '  +' + a.strip())

    for p in paths:
        with open(p, encoding='utf-8') as fh:
            for raw in fh:
                line = raw.rstrip('\n').rstrip('\r')
                if line.startswith('diff --git '):
                    flush(); rem, add = [], []
                    cur = line.split(' b/', 1)[1]
                    files.setdefault(cur, {'a': 0, 'b': 0, 'bad': []})
                    if not cur.endswith('_test.cs'):
                        files[cur]['bad'].append('not a *_test.cs file')
                elif line.startswith('@@'):
                    flush(); rem, add = [], []
                elif line.startswith('---') or line.startswith('+++'):
                    continue
                elif line.startswith('-'):
                    rem.append(line[1:])
                elif line.startswith('+'):
                    add.append(line[1:])
    flush()
    return files


def main():
    if len(sys.argv) < 2:
        print(__doc__)
        return 2
    files = classify(sys.argv[1:])
    if not files:
        print('NO DRIFTED FILE in the input -- nothing was classified')
        return 2
    bad = 0
    ta = tb = 0
    for path in sorted(files):
        f = files[path]
        if f['bad']:
            bad += 1
            for reason in f['bad']:
                print(f'{path}\tUNCLASSIFIED\t{reason}')
        else:
            print(f'{path}\ttest-refresh-1262\ta={f["a"]}\tb={f["b"]}')
        ta += f['a']; tb += f['b']
    print(f'TOTAL files={len(files)} in_class={len(files) - bad} unclassified={bad} lines_a={ta} lines_b={tb}')
    return 1 if bad else 0


if __name__ == '__main__':
    sys.exit(main())
