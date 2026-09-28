#!/usr/bin/env python3
"""Check that a structural pass over the conversion-strategy reference lost nothing and broke no link.

usage: ref_invariant.py <repo-root> --base <ref> [--allow <allow.json>] [--quiet]

The reference is docs/ConversionStrategies-Reference/** plus the old single-page stub
docs/ConversionStrategies-Reference.md. Three checks, each against the tree at --base:

 (a) INVENTORY. The multiset of Test[A-Za-z0-9_]+ names, and the multiset of backticked spans, over the
     reference's content lines is unchanged. Nothing may disappear. An occurrence may be ADDED only by a
     line that is new since the base AND is a heading, a new page's text above its first H2, or a line
     the allow file names; every addition is listed.
 (b) LINES. Every non-blank content line of the base reference still exists in the current reference
     (as a multiset), compared with link targets blanked and heading levels dropped. A line the allow
     file records as split must be exactly reassembled by its two halves.
 (c) LINKS. Every relative link, in any Markdown file under docs/, .claude/, src/, README.md or
     CLAUDE.md, whose target lies in the reference resolves (file and anchor); and so does every
     relative link written in the reference itself or in docs/ConversionStrategies.md, wherever it
     points. Links broken at the base are reported apart; only a newly broken link fails the check.

Generated lines are excluded from (a) and (b) and checked on their own: the Contents rows of the
reference index, the rows of the old stub's anchor map (all must survive, with the same labels), the
"Moved to" stub lines, and each page's navigation lines.

The allow file is {"added_lines": [substring, ...], "split_lines": [{"orig": substring, "prefix": text}]}:
an added line containing a listed substring may add inventory; a base line containing `orig` that is
missing is accounted for only when it equals exactly <a current line> + " " + <the rest of a current
line that starts with `prefix`> (a line split in two with a restored lead in front of its tail).
A PROSE pass rewords lines on purpose, which (b) would otherwise read as loss: "edited_lines": [{"orig":
substring, "now": substring}] accounts for a missing base line containing `orig` only when some current
line contains `now`, so every reworded line is named, old and new, in the allow file.

Exit 0 only when all three checks pass.
"""
import collections
import json
import os
import re
import subprocess
import sys

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import ref_common as rc  # noqa: E402

REF_DIR = 'docs/ConversionStrategies-Reference'
LEGACY = 'docs/ConversionStrategies-Reference.md'
SUMMARY = 'docs/ConversionStrategies.md'
TEST_RE = re.compile(r'\bTest[A-Za-z0-9_]+')
STUB_RE = re.compile(r'^\s*(?:- )?<a id="[^"]+"></a>Moved to \[')
LEGACY_ROW_RE = re.compile(r'^\s*- <a id="[^"]+"></a>\[')
NAV_RE = re.compile(r'^\[(?:← |Reference index\]|Index\]|Up: )')


class Tree:
    """Read access to the repository either at a git ref or in the working tree."""

    def __init__(self, root, ref=None):
        self.root = root
        self.ref = ref
        self.cache = {}
        if ref:
            out = subprocess.run(['git', '-C', root, 'ls-tree', '-r', '--name-only', '-z', ref],
                                 capture_output=True, check=True).stdout.decode('utf-8')
            self.files = {f for f in out.split('\0') if f}
            self.dirs = set()
            for f in self.files:
                d = os.path.dirname(f)
                while d:
                    self.dirs.add(d)
                    d = os.path.dirname(d)
        else:
            self.files = None

    def exists(self, rel):
        if self.ref:
            return rel in self.files or rel in self.dirs
        return os.path.exists(os.path.join(self.root, rel))

    def _batch(self, rel):
        if not hasattr(self, '_proc'):
            self._proc = subprocess.Popen(['git', '-C', self.root, 'cat-file', '--batch'],
                                          stdin=subprocess.PIPE, stdout=subprocess.PIPE)
        self._proc.stdin.write(f'{self.ref}:{rel}\n'.encode('utf-8'))
        self._proc.stdin.flush()
        header = self._proc.stdout.readline().decode('utf-8')
        if header.endswith('missing\n'):
            return ''
        size = int(header.split()[2])
        data = self._proc.stdout.read(size)
        self._proc.stdout.read(1)
        return data.decode('utf-8', 'replace')

    def lines(self, rel):
        if rel not in self.cache:
            if self.ref:
                text = self._batch(rel) if rel in self.files else ''
            else:
                try:
                    text = open(os.path.join(self.root, rel), encoding='utf-8', errors='replace').read()
                except OSError:
                    text = ''
            if text.startswith('﻿'):
                text = text[1:]
            self.cache[rel] = text.replace('\r\n', '\n').split('\n')
        return self.cache[rel]

    def md_files(self, tops=('docs/', '.claude/', 'src/'), singles=('README.md', 'CLAUDE.md')):
        if self.ref:
            fs = self.files
        else:
            fs = {os.path.relpath(p, self.root).replace(os.sep, '/') for p in rc.repo_markdown_files(self.root)}
        return sorted(f for f in fs if f.endswith('.md') and (f in singles or any(f.startswith(t) for t in tops)))

    def reference_files(self):
        return [f for f in self.md_files() if f.startswith(REF_DIR + '/') or f == LEGACY]


def classify(rel, lines):
    """Per line: 'gen-index', 'gen-legacy', 'stub', 'nav', 'raw', 'blank' or 'content'."""
    kinds = []
    mask = rc.fence_mask(lines)
    in_contents = False
    for i, l in enumerate(lines):
        k = 'content'
        if not mask[i]:
            if l.strip() == '':
                k = 'blank'
            elif l.startswith(rc.RAW_OPEN_PREFIX) or l.strip() == rc.RAW_CLOSE:
                k = 'raw'
            elif rel == REF_DIR + '/README.md' and in_contents and (l.startswith('- ') or l.startswith('  ')):
                k = 'gen-index'
            elif rel == LEGACY and LEGACY_ROW_RE.match(l):
                k = 'gen-legacy'
            elif STUB_RE.match(l):
                k = 'stub'
            elif NAV_RE.match(l):
                k = 'nav'
            if rel == REF_DIR + '/README.md':
                h = rc.HEADING_RE.match(l)
                if h:
                    in_contents = h.group(2) == 'Contents'
        elif l.strip() == '':
            k = 'blank'
        kinds.append(k)
    return kinds


def spans(line):
    return [m.group(2) for m in rc.CODE_SPAN_RE.finditer(line)]


def norm(l, mask_i):
    if not mask_i and rc.HEADING_RE.match(l):
        return rc.normalize_line(l, True)
    if mask_i:
        return l.rstrip()
    return rc.normalize_line(l, False)


def gather(tree):
    """Content lines of the reference: [(rel, line index, raw line, normalized, is_heading, preamble_of_page)]."""
    out = []
    gen = {'index': [], 'legacy': [], 'stub': []}
    for rel in tree.reference_files():
        lines = tree.lines(rel)
        mask = rc.fence_mask(lines)
        kinds = classify(rel, lines)
        first_h2 = next((i for (i, lv, _) in rc.headings(lines, mask) if lv == 2), len(lines))
        for i, l in enumerate(lines):
            k = kinds[i]
            if k == 'content':
                is_h = (not mask[i]) and bool(rc.HEADING_RE.match(l))
                out.append((rel, i, l, norm(l, mask[i]), is_h, i < first_h2))
            elif k == 'gen-index':
                gen['index'].append((rel, i, l))
            elif k == 'gen-legacy':
                gen['legacy'].append((rel, i, l))
            elif k == 'stub':
                gen['stub'].append((rel, i, l))
    return out, gen


def check_inventory(base_c, cur_c, base_pages, allow):
    """(a): returns (errors, additions)."""
    errors, additions = [], []
    base_lines = collections.Counter(x[3] for x in base_c)
    cur_by_norm = collections.defaultdict(list)
    for x in cur_c:
        cur_by_norm[x[3]].append(x)
    # which current lines are new since the base (multiset difference, by normalized text)
    remaining = collections.Counter(base_lines)
    new_lines = []
    for x in cur_c:
        if remaining[x[3]] > 0:
            remaining[x[3]] -= 1
        else:
            new_lines.append(x)
    for label, extract in (('Test name', lambda l: TEST_RE.findall(l)), ('backticked span', spans)):
        b = collections.Counter()
        for x in base_c:
            b.update(extract(x[2]))
        c = collections.Counter()
        for x in cur_c:
            c.update(extract(x[2]))
        missing = b - c
        for s, n in sorted(missing.items()):
            where = [f'{x[0]}:{x[1] + 1}' for x in base_c if s in extract(x[2])][:3]
            errors.append(f'(a) {label} lost: {s!r} x{n} (base sites {", ".join(where)})')
        extra = c - b
        allowed = collections.Counter()
        for x in new_lines:
            ok = x[4] or (x[5] and x[0] not in base_pages) or any(a in x[2] for a in allow.get('added_lines', []))
            if ok:
                allowed.update(extract(x[2]))
        for s, n in sorted(extra.items()):
            if allowed[s] >= n:
                sites = [f'{x[0]}:{x[1] + 1}' for x in new_lines if s in extract(x[2])][:2]
                additions.append(f'{label} +{n} {s!r} ({", ".join(sites)})')
            else:
                sites = [f'{x[0]}:{x[1] + 1}' for x in cur_c if s in extract(x[2])][:3]
                errors.append(f'(a) {label} added outside a new heading/intro/allowed line: {s!r} x{n - allowed[s]} '
                              f'(current sites {", ".join(sites)})')
    return errors, additions


def check_lines(base_c, cur_c, allow):
    """(b)."""
    errors = []
    cur = collections.Counter(x[3] for x in cur_c if x[3].strip())
    need = collections.Counter(x[3] for x in base_c if x[3].strip())
    missing = need - cur
    cur_norm = [x[3] for x in cur_c]
    for l, n in missing.items():
        split = [s for s in allow.get('split_lines', []) if s['orig'] in l]
        if split and n == 1:
            pre = split[0]['prefix']
            heads = [c for c in cur_norm if c and l.startswith(c) and c != l]
            tails = [c[len(pre):] for c in cur_norm if c.startswith(pre)]
            if any(h + ' ' + t == l for h in heads for t in tails):
                continue
        edit = [e for e in allow.get('edited_lines', []) if e['orig'] in l]
        if edit and n == 1 and any(edit[0]['now'] in c for c in cur_norm):
            continue
        site = next(f'{x[0]}:{x[1] + 1}' for x in base_c if x[3] == l)
        errors.append(f'(b) base line missing x{n} (base {site}): {l[:140]!r}')
    return errors


def check_generated(base_gen, cur_gen):
    errors = []
    lab = lambda l: rc.rewrite_links_in_line(l, lambda t: '').strip()  # noqa: E731
    b = collections.Counter(lab(x[2]) for x in base_gen['legacy'])
    c = collections.Counter(lab(x[2]) for x in cur_gen['legacy'])
    for l, n in (b - c).items():
        errors.append(f'(b) legacy anchor-map row lost x{n}: {l[:140]!r}')
    return errors


def anchor_set(tree, rel):
    return rc.anchors(tree.lines(rel))


def check_links(tree):
    """(c): [(source rel, line no, target, reason)] for every broken link in scope."""
    bad = []
    ref_prefix = REF_DIR + '/'
    anchors_cache = {}
    for rel in tree.md_files():
        lines = tree.lines(rel)
        own_scope = rel.startswith(ref_prefix) or rel in (LEGACY, SUMMARY)
        for (i, t) in rc.link_targets(lines):
            if not t or rc.is_external(t) or t.startswith('/'):
                continue
            path, _, frag = t.partition('#')
            if path:
                p = os.path.normpath(os.path.join(os.path.dirname(rel), rc.link_path(path))).replace(os.sep, '/')
                if not tree.exists(p):
                    root_p = os.path.normpath(rc.link_path(path)).replace(os.sep, '/')
                    if rel.startswith('.claude/') and tree.exists(root_p):
                        p = root_p  # repo-root-relative spelling used by .claude/ files
            else:
                p = rel
            into_ref = p.startswith(ref_prefix) or p == LEGACY
            if not (into_ref or own_scope):
                continue
            if not tree.exists(p):
                bad.append((rel, i + 1, t, 'no file'))
                continue
            if frag and p.endswith('.md'):
                if p not in anchors_cache:
                    anchors_cache[p] = anchor_set(tree, p)
                if frag not in anchors_cache[p]:
                    bad.append((rel, i + 1, t, 'no anchor'))
    return bad


def main():
    args = sys.argv[1:]
    if not args or '--base' not in args:
        print(__doc__)
        sys.exit(2)
    root = os.path.abspath(args[0])
    base = args[args.index('--base') + 1]
    allow = {}
    if '--allow' in args:
        allow = json.load(open(args[args.index('--allow') + 1], encoding='utf-8'))
    quiet = '--quiet' in args

    bt, ct = Tree(root, base), Tree(root)
    base_c, base_gen = gather(bt)
    cur_c, cur_gen = gather(ct)
    base_pages = set(bt.reference_files())

    errors = []
    e, additions = check_inventory(base_c, cur_c, base_pages, allow)
    errors += e
    errors += check_lines(base_c, cur_c, allow)
    errors += check_generated(base_gen, cur_gen)

    bad_base = check_links(bt)
    bad_cur = check_links(ct)
    base_keys = collections.Counter((os.path.normpath(os.path.join(os.path.dirname(s), t.partition('#')[0])) if
                                     t.partition('#')[0] else s, t.partition('#')[2], r) for (s, _, t, r) in bad_base)
    new_bad = []
    for (s, n, t, r) in bad_cur:
        key = (os.path.normpath(os.path.join(os.path.dirname(s), t.partition('#')[0])) if t.partition('#')[0] else s,
               t.partition('#')[2], r)
        if base_keys[key] > 0:
            base_keys[key] -= 1
        else:
            new_bad.append((s, n, t, r))
    for (s, n, t, r) in new_bad:
        errors.append(f'(c) broken link {s}:{n} -> {t} ({r})')

    counts = collections.Counter(x[0] for x in cur_c)
    print(f'reference files: base {len(base_pages)}, current {len(set(ct.reference_files()))}')
    print(f'(a) Test names base {sum(len(TEST_RE.findall(x[2])) for x in base_c)} / current '
          f'{sum(len(TEST_RE.findall(x[2])) for x in cur_c)}; backticked spans base '
          f'{sum(len(spans(x[2])) for x in base_c)} / current {sum(len(spans(x[2])) for x in cur_c)}; '
          f'additions {len(additions)}')
    if not quiet:
        for a in additions:
            print('    +', a)
    print(f'(b) content lines base {sum(1 for x in base_c if x[3].strip())} / current '
          f'{sum(1 for x in cur_c if x[3].strip())}; legacy map rows base {len(base_gen["legacy"])} / current '
          f'{len(cur_gen["legacy"])}; stub lines {len(cur_gen["stub"])}; index rows {len(cur_gen["index"])}')
    print(f'(c) broken links in scope: base {len(bad_base)}, current {len(bad_cur)}, newly broken {len(new_bad)}')
    if bad_cur and not quiet:
        for b in bad_cur[:60]:
            print('    broken:', b)
    if errors:
        print(f'FAIL: {len(errors)} errors')
        for e in errors[:80]:
            print('  ', e)
        sys.exit(1)
    print('PASS')


if __name__ == '__main__':
    main()
