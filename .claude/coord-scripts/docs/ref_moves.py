#!/usr/bin/env python3
"""Apply a JSON manifest of structural operations to the conversion-strategy reference.

usage: ref_moves.py <repo-root> <manifest.json> --stub-base <ref> [--dry-run]
       ref_moves.py <repo-root> --self-test [--ref <ref>]

The manifest is {"ops": [...]}. Operations run in order, in memory; files are written only after every
operation succeeded, each with its original line ending, BOM and trailing newline.

  create      {file, lines}
      A new page. Fails if the file exists. Lines are written with CRLF, like the rest of the reference.
  heading     {file, before, text}
      Insert heading line `text` (and a blank line) above the one non-fenced line starting with `before`.
      For an entry that has only a bold lead: it needs a heading before it can move.
  split_line  {file, at, before, prefix}
      Split the one line containing `at` right after it: the head keeps `at` (right-trimmed), then the
      `before` lines, then `prefix` + the tail. Used to restore a lead lost in a splice.
  insert      {file, before_heading | after_line, lines}
      Insert lines above a heading (exact text) or after the one line equal to `after_line`.
  move        {src, heading, level, dst, dst_group?, after?, dst_level?, until?, stub}
      Move the block from the heading (exact text, at `level`) to the line before the next heading of
      the same or a higher level (or the page footer); `until` ends it earlier, before the first line
      of the block that starts with that text. With `dst_group` the block lands at the end of
      that H2 in dst (created at the end of the page if missing), its top heading at level 3; without
      it the block keeps its level (or takes `dst_level`). `after` places it after the named heading's
      block instead of at the end. `stub` is "inplace" (the anchor paragraph replaces the block) or
      "group:<H2 text>" (a list item under that H2 of src, created if missing).
      A stub is left for every heading of the block whose anchor existed in src at --stub-base: an
      `<a id="old-slug"></a>` plus "Moved to [Heading](path#new-slug)." A heading added after the base
      never had an anchor anyone could link to, so it leaves none.
      Links inside the block are re-relativized to dst. Every link in the repository's Markdown
      (docs/, .claude/, src/, README.md, CLAUDE.md) to src#old-slug is rewritten to dst#new-slug, and
      a doc path in src/migrate-tfm.ps1 whose anchored text moved is re-pointed. New slugs are
      computed in the destination page after insertion, with GitHub's -1/-2 suffixes.
      The move fails if any heading that stays behind (in src or dst) changes slug, or if an anchor it
      creates collides with one already on the page.
  index       {readme, new_pages: [{page, after}], children: {page: [pages]}, grouped: [pages],
               skip_h2: [texts]}
      Regenerate the Contents list of the reference index from the pages themselves: each page's H1,
      then its H2s (and, for a `grouped` page, the H3s under each H2), then its child pages nested.
  retarget    {file, from, to}
      Replace every link target exactly equal to `from` in one file (repairing a broken anchor).

--self-test renders the reference pages through GitHub's API (gh must be signed in) and checks that
every heading anchor GitHub produced equals the slug computed here.
"""
import json
import os
import re
import subprocess
import sys

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import ref_common as rc  # noqa: E402

RAW_OPEN = ('<!-- {% raw %} — Jekyll/Liquid guard: this page contains Go composite-literal and template '
            'syntax ({{ … }}) that Liquid would otherwise parse; the HTML comment hides the tag on GitHub. -->')
MIGRATE_SCRIPTS = ['src/migrate-tfm.ps1', 'src/migrate-gorelease.ps1']


class Fail(Exception):
    pass


class Repo:
    def __init__(self, root, stub_base):
        self.root = os.path.abspath(root)
        self.stub_base = stub_base
        self.docs = {}
        self.md_files = rc.repo_markdown_files(self.root)
        self.created = []
        self.log = []
        self.base_anchor_cache = {}
        self.raw_cache = {}

    def P(self, rel):
        return os.path.normpath(os.path.join(self.root, rel.replace('/', os.sep)))

    def rel(self, p):
        return os.path.relpath(p, self.root).replace(os.sep, '/')

    def doc(self, p):
        if p not in self.docs:
            if not os.path.exists(p):
                raise Fail(f'no such file: {self.rel(p)}')
            self.docs[p] = rc.Doc(p)
        return self.docs[p]

    def base_anchors(self, p):
        """Anchors the page had at --stub-base (empty when the page did not exist there)."""
        if p not in self.base_anchor_cache:
            r = subprocess.run(['git', '-C', self.root, 'show', f'{self.stub_base}:{self.rel(p)}'],
                               capture_output=True)
            if r.returncode != 0:
                self.base_anchor_cache[p] = set()
            else:
                text = r.stdout.decode('utf-8').replace('\r\n', '\n')
                self.base_anchor_cache[p] = rc.anchors(text.split('\n'))
        return self.base_anchor_cache[p]


# ------------------------------------------------------------------------------------------------
# page structure

def body_end(lines):
    """Index of the footer's '---' line (or of the trailing raw-guard/blank run when there is no footer)."""
    j = len(lines)
    while j > 0 and (lines[j - 1].strip() == '' or lines[j - 1].strip() == rc.RAW_CLOSE):
        j -= 1
    if j > 0 and ('[Index](' in lines[j - 1] or '[Reference index](' in lines[j - 1]) and lines[j - 1].lstrip().startswith('['):
        k = j - 1
        while k > 0 and lines[k - 1].strip() == '':
            k -= 1
        if k > 0 and lines[k - 1].strip() == '---':
            return k - 1
    return j


def trim_end(lines, start, end):
    while end > start and lines[end - 1].strip() == '':
        end -= 1
    return end


def block_range(lines, hs, k, limit=None):
    """[start, end) of heading hs[k]'s block: to the next heading of the same or a higher level."""
    i0, lv = hs[k][0], hs[k][1]
    end = body_end(lines) if limit is None else limit
    for h in hs[k + 1:]:
        if h[0] >= end:
            break
        if h[1] <= lv:
            end = h[0]
            break
    return i0, trim_end(lines, i0, end)


def find_heading(lines, text, level=None, what=''):
    hs = rc.heading_slugs(lines)
    ks = [k for k, h in enumerate(hs) if h[2] == text and (level is None or h[1] == level)]
    if len(ks) != 1:
        raise Fail(f'{what}: expected one heading {text!r} (level {level}), found {len(ks)}')
    return hs, ks[0]


def find_line(lines, pred, what):
    mask = rc.fence_mask(lines)
    ks = [i for i, l in enumerate(lines) if not mask[i] and pred(l)]
    if len(ks) != 1:
        raise Fail(f'{what}: expected one matching line, found {len(ks)}')
    return ks[0]


def splice(lines, pos, new):
    """Insert `new` at pos with exactly one blank line on each side (none added at the file edges)."""
    before = lines[:pos]
    after = lines[pos:]
    while before and before[-1].strip() == '':
        before.pop()
    while after and after[0].strip() == '':
        after.pop(0)
    out = list(before)
    if out:
        out.append('')
    out += new
    if after:
        out.append('')
    out += after
    return out, (len(before) + (1 if before else 0))


def remove(lines, start, end):
    before = lines[:start]
    after = lines[end:]
    while before and before[-1].strip() == '':
        before.pop()
    while after and after[0].strip() == '':
        after.pop(0)
    out = list(before)
    pos = len(out)
    if out and after:
        out.append('')
        pos += 1
    return out + after, pos


def link_label(text):
    t = re.sub(r'\[([^\]]*)\]\([^)]*\)', r'\1', text)
    return t


def needs_raw(lines):
    return any(('{{' in l or '{%' in l) and not l.startswith(rc.RAW_OPEN_PREFIX) and l.strip() != rc.RAW_CLOSE
               for l in lines)


def ensure_raw_guard(d):
    if needs_raw(d.lines) and not any(l.startswith(rc.RAW_OPEN_PREFIX) for l in d.lines):
        d.lines = [RAW_OPEN] + d.lines + [rc.RAW_CLOSE]
        return True
    return False


# ------------------------------------------------------------------------------------------------
# link resolution with the repo-root fallback some .claude/ files use

def resolve_any(repo, doc_path, target):
    """(absolute path, fragment, style) where style is 'rel', 'root' or 'self'; None when not a file link."""
    if not target or rc.is_external(target) or target.startswith('/'):
        return None
    path, _, frag = target.partition('#')
    if not path:
        return doc_path, frag, 'self'
    path = rc.link_path(path)
    p1 = os.path.normpath(os.path.join(os.path.dirname(doc_path), path))
    if os.path.exists(p1) or p1 in repo.docs:
        return p1, frag, 'rel'
    p2 = os.path.normpath(os.path.join(repo.root, path))
    if os.path.exists(p2) or p2 in repo.docs:
        return p2, frag, 'root'
    return p1, frag, 'rel'


def spell(repo, target_file, frag, doc_path, style):
    if target_file == doc_path:
        return '#' + frag if frag else rc.relpath(target_file, doc_path)
    if style == 'root':
        base = repo.rel(target_file)
    else:
        base = rc.relpath(target_file, doc_path)
    return base + ('#' + frag if frag else '')


# ------------------------------------------------------------------------------------------------
# operations

def op_create(repo, op):
    p = repo.P(op['file'])
    if os.path.exists(p) or p in repo.docs:
        raise Fail(f'create: {op["file"]} exists')
    d = rc.Doc.new(p, '\r\n')
    d.lines = list(op['lines'])
    repo.docs[p] = d
    repo.created.append(p)
    if p not in repo.md_files:
        repo.md_files.append(p)
    repo.log.append(f'create {op["file"]}')


def op_heading(repo, op):
    d = repo.doc(repo.P(op['file']))
    i = find_line(d.lines, lambda l: l.startswith(op['before']), f'heading {op["text"]!r}')
    if i > 0 and d.lines[i - 1].strip() != '':
        raise Fail(f'heading {op["text"]!r}: the line above {op["before"]!r} is not blank')
    d.lines[i:i] = [op['text'], '']
    repo.log.append(f'heading {op["file"]}: {op["text"]}')


def op_split_line(repo, op):
    d = repo.doc(repo.P(op['file']))
    i = find_line(d.lines, lambda l: op['at'] in l, f'split_line {op["at"]!r}')
    l = d.lines[i]
    if l.count(op['at']) != 1:
        raise Fail(f'split_line: {op["at"]!r} occurs more than once on its line')
    cut = l.index(op['at']) + len(op['at'])
    head, tail = l[:cut].rstrip(), l[cut:].lstrip()
    d.lines[i:i + 1] = [head] + list(op['before']) + [op['prefix'] + tail]
    repo.log.append(f'split_line {op["file"]}: after {op["at"]!r}')


def op_insert(repo, op):
    d = repo.doc(repo.P(op['file']))
    if 'before_heading' in op:
        hs, k = find_heading(d.lines, op['before_heading'], op.get('level'), 'insert')
        pos = hs[k][0]
    else:
        pos = find_line(d.lines, lambda l: l == op['after_line'], 'insert') + 1
    d.lines, _ = splice(d.lines, pos, list(op['lines']))
    repo.log.append(f'insert {op["file"]}: {len(op["lines"])} lines')


def op_move(repo, op):
    src, dst = repo.P(op['src']), repo.P(op['dst'])
    if src == dst:
        raise Fail('move: src == dst')
    S, D = repo.doc(src), repo.doc(dst)
    level = op['level']
    what = f'move {op["heading"][:60]!r}'
    hs, k = find_heading(S.lines, op['heading'], level, what)
    i0, i1 = block_range(S.lines, hs, k)
    if op.get('until'):
        umask = rc.fence_mask(S.lines)
        us = [i for i in range(i0 + 1, i1) if not umask[i] and S.lines[i].startswith(op['until'])]
        if not us:
            raise Fail(f'{what}: until-line {op["until"]!r} not inside the block')
        i1 = trim_end(S.lines, i0, us[0])
    block = S.lines[i0:i1]
    bmask = rc.fence_mask(block)
    bheads = [(i - i0, lv, t, s) for (i, lv, t, s) in hs if i0 <= i < i1]
    old_slugs = [s for (_, _, _, s) in bheads]
    src_keep_before = [s for (i, _, _, s) in hs if not (i0 <= i < i1)]
    dst_before = [s for (_, _, _, s) in rc.heading_slugs(D.lines)]
    dst_ids_before = set(rc.anchor_ids(D.lines))

    # heading levels
    tl = 3 if op.get('dst_group') else op.get('dst_level', level)
    shift = tl - level
    if shift:
        for (bi, lv, t, s) in bheads:
            nl = lv + shift
            if not 2 <= nl <= 6:
                raise Fail(f'{what}: heading level {lv} -> {nl} out of range')
            m = rc.HEADING_RE.match(block[bi])
            block[bi] = '#' * nl + block[bi][len(m.group(1)):]

    # placement in dst
    D_lines = D.lines
    if op.get('dst_group'):
        g = op['dst_group']
        ghs = [h for h in rc.headings(D_lines) if h[1] == 2 and h[2] == g]
        if len(ghs) > 1:
            raise Fail(f'{what}: group {g!r} occurs twice in {op["dst"]}')
        if not ghs:
            D_lines, _ = splice(D_lines, body_end(D_lines), ['## ' + g])
            repo.log.append(f'  group created in {op["dst"]}: {g}')
        dhs = rc.heading_slugs(D_lines)
        gk = [n for n, h in enumerate(dhs) if h[1] == 2 and h[2] == g][0]
        gstart, gend = block_range(D_lines, dhs, gk)
        if op.get('after'):
            ak = [n for n, h in enumerate(dhs) if h[2] == op['after'] and gstart <= h[0] < gend]
            if len(ak) != 1:
                raise Fail(f'{what}: after-heading {op["after"]!r} not found once in group {g!r}')
            _, pos = block_range(D_lines, dhs, ak[0], limit=gend if gend > dhs[ak[0]][0] else None)
        else:
            pos = gend
    else:
        dhs = rc.heading_slugs(D_lines)
        if op.get('after'):
            ak = [n for n, h in enumerate(dhs) if h[2] == op['after']]
            if len(ak) != 1:
                raise Fail(f'{what}: after-heading {op["after"]!r} not found once in {op["dst"]}')
            _, pos = block_range(D_lines, dhs, ak[0])
        else:
            pos = trim_end(D_lines, 0, body_end(D_lines))
    D_lines, ins = splice(D_lines, pos, block)

    # new slugs
    dhs_after = rc.heading_slugs(D_lines)
    by_line = {h[0]: h[3] for h in dhs_after}
    new_slugs = [by_line[ins + bi] for (bi, _, _, _) in bheads]
    slug_map = dict(zip(old_slugs, new_slugs))
    inserted = set(range(ins, ins + len(block)))
    dst_after_existing = [h[3] for h in dhs_after if h[0] not in inserted]
    if dst_after_existing != dst_before:
        raise Fail(f'{what}: inserting shifts the slugs of headings already in {op["dst"]}')
    clash = set(new_slugs) & dst_ids_before
    if clash:
        raise Fail(f'{what}: new heading anchors collide with <a id> anchors in {op["dst"]}: {clash}')

    # links inside the moved block, re-relativized to dst
    def block_fn(t, _i):
        r = resolve_any(repo, src, t)
        if r is None:
            return t
        p, frag, style = r
        if p == src and frag in slug_map and (style == 'self' or True):
            return '#' + slug_map[frag]
        if style == 'self':
            return spell(repo, src, frag, dst, 'rel')
        return spell(repo, p, frag, dst, style)
    seg = D_lines[ins:ins + len(block)]
    D_lines[ins:ins + len(block)] = rc.rewrite_lines(seg, block_fn, bmask)
    D.lines = D_lines

    # remove from src; stubs
    S_lines, rpos = remove(S.lines, i0, i1)
    src_keep_after = [h[3] for h in rc.heading_slugs(S_lines)]
    if src_keep_after != src_keep_before:
        raise Fail(f'{what}: removing the block shifts the slugs of headings that stay in {op["src"]}')
    existing_ids = set(rc.anchor_ids(S_lines))
    base = repo.base_anchors(src)
    stubbed = [(o, n, t) for (o, n, (_, _, t, _)) in zip(old_slugs, new_slugs, bheads) if o in base]
    stub_lines = []
    for o, n, t in stubbed:
        tgt = spell(repo, dst, n, src, 'rel')
        stub_lines.append(f'<a id="{o}"></a>Moved to [{link_label(t)}]({tgt}).')
    mode = op.get('stub', 'inplace')
    if stub_lines:
        if mode == 'inplace':
            para = []
            for n, sl in enumerate(stub_lines):
                if n:
                    para.append('')
                para.append(sl)
            S_lines, _ = splice(S_lines, rpos, para)
        elif mode.startswith('group:'):
            g = mode[len('group:'):]
            ghs = [h for h in rc.headings(S_lines) if h[1] == 2 and h[2] == g]
            if not ghs:
                S_lines, _ = splice(S_lines, trim_end(S_lines, 0, body_end(S_lines)), ['## ' + g])
            shs = rc.heading_slugs(S_lines)
            gk = [n for n, h in enumerate(shs) if h[1] == 2 and h[2] == g][0]
            gstart, gend = block_range(S_lines, shs, gk)
            items = ['- ' + sl for sl in stub_lines]
            if gend - gstart <= 1:
                S_lines, _ = splice(S_lines, gend, items)
            else:
                S_lines[gend:gend] = items
        else:
            raise Fail(f'{what}: unknown stub mode {mode!r}')
    new_ids = [o for (o, _, _) in stubbed]
    final_slugs = {h[3] for h in rc.heading_slugs(S_lines)}
    dup = {a for a in new_ids if new_ids.count(a) > 1} | (set(new_ids) & (existing_ids | final_slugs))
    if dup:
        raise Fail(f'{what}: stub anchors collide with anchors already in {op["src"]}: {dup}')
    S.lines = S_lines
    ensure_raw_guard(D) and repo.log.append(f'  raw guard added to {op["dst"]}')

    # inbound links across the repository
    src_base = os.path.basename(src)
    total = 0
    touched = []
    for p in repo.md_files:
        if p in repo.docs:
            text_has = any(src_base in l for l in repo.docs[p].lines)
        else:
            if p not in repo.raw_cache:
                try:
                    repo.raw_cache[p] = open(p, encoding='utf-8', errors='replace').read()
                except OSError:
                    repo.raw_cache[p] = ''
            text_has = src_base in repo.raw_cache[p]
        if not (text_has or p == src or p == dst):
            continue
        d = repo.doc(p)
        count = [0]

        def fn(t, _i, p=p, count=count):
            r = resolve_any(repo, p, t)
            if r is None:
                return t
            q, frag, style = r
            if q != src or frag not in slug_map:
                return t
            count[0] += 1
            return spell(repo, dst, slug_map[frag], p, 'rel' if style == 'self' else style)
        new = rc.rewrite_lines(d.lines, fn)
        if count[0]:
            d.lines = new
            total += count[0]
            touched.append(f'{repo.rel(p)}:{count[0]}')

    migrate_repoint(repo, src, dst, block)
    repo.log.append(f'move {op["src"]} -> {op["dst"]} ({len(block)} lines, {len(bheads)} headings, '
                    f'{len(stubbed)} stubs, {total} inbound links: {", ".join(touched) or "none"})  '
                    f'{op["heading"][:70]}')


def migrate_repoint(repo, src, dst, block):
    """Re-point a migrate-tfm.ps1 entry whose anchored text left src with this block."""
    src_win = repo.rel(src).replace('/', '\\')
    for rel in MIGRATE_SCRIPTS:
        p = repo.P(rel)
        if not os.path.exists(p):
            continue
        d = repo.doc(p)
        for i, l in enumerate(d.lines):
            if src_win in l or repo.rel(src) in l:
                if 'File =' not in l:
                    continue
                # the Old = string on this line or the next one
                seg = ' '.join(d.lines[i:i + 2])
                m = re.search(r'Old\s*=\s*"((?:[^"`]|`.)*)"', seg)
                if not m:
                    continue
                old = m.group(1).replace('`"', '"').replace('``', '`').replace("`'", "'")
                pat = re.escape(old).replace(re.escape('$From'), r'\S+').replace(re.escape('$To'), r'\S+')
                if any(re.search(pat, bl) for bl in block):
                    dst_win = repo.rel(dst).replace('/', '\\')
                    d.lines[i] = l.replace(src_win, dst_win).replace(repo.rel(src), repo.rel(dst))
                    repo.log.append(f'  {rel}:{i + 1} re-pointed to {repo.rel(dst)}')


def op_index(repo, op):
    readme = repo.P(op['readme'])
    R = repo.doc(readme)
    rdir = os.path.dirname(readme)
    top_re = re.compile(r'^- \*\*\[(.+)\]\(([^)#]+)\)\*\*$')
    tops = [m.group(2) for m in (top_re.match(l) for l in R.lines) if m]
    for np in op.get('new_pages', []):
        if np['page'] in tops:
            continue
        tops.insert(tops.index(np['after']) + 1, np['page'])
    children = op.get('children', {})
    grouped = set(op.get('grouped', []))
    skip = set(op.get('skip_h2', []))

    def rows(page, depth):
        p = os.path.normpath(os.path.join(rdir, page))
        d = repo.doc(p)
        hs = rc.heading_slugs(d.lines)
        h1 = [h for h in hs if h[1] == 1]
        if not h1:
            raise Fail(f'index: {page} has no H1')
        ind = '  ' * depth
        out = [f'{ind}- **[{h1[0][2]}]({page})**']
        in_skip = False
        for (_, lv, t, s) in hs:
            if lv == 2:
                in_skip = t in skip
                if not in_skip:
                    out.append(f'{ind}  - [{t}]({page}#{s})')
            elif lv == 3 and page in grouped and not in_skip:
                out.append(f'{ind}    - [{t}]({page}#{s})')
        for c in children.get(page, []):
            out += rows(c, depth + 1)
        return out

    body = []
    for t in tops:
        body += rows(t, 0)
    first = next(i for i, l in enumerate(R.lines) if top_re.match(l))
    last = first
    for i in range(first, len(R.lines)):
        if R.lines[i].startswith('- ') or R.lines[i].startswith('  '):
            last = i
        elif R.lines[i].strip() == '':
            continue
        else:
            break
    R.lines[first:last + 1] = body
    repo.log.append(f'index {op["readme"]}: {len(tops)} top-level pages, {len(body)} rows')


def op_retarget(repo, op):
    """Replace link targets exactly equal to `from` with `to` in one file (a broken anchor's repair)."""
    d = repo.doc(repo.P(op['file']))
    n = [0]

    def fn(t, _i):
        if t == op['from']:
            n[0] += 1
            return op['to']
        return t
    d.lines = rc.rewrite_lines(d.lines, fn)
    if not n[0]:
        raise Fail(f'retarget: no link {op["from"]!r} in {op["file"]}')
    repo.log.append(f'retarget {op["file"]}: {op["from"]} -> {op["to"]} (x{n[0]})')


OPS = {'create': op_create, 'heading': op_heading, 'split_line': op_split_line, 'insert': op_insert,
       'move': op_move, 'index': op_index, 'retarget': op_retarget}


# ------------------------------------------------------------------------------------------------

def self_test(root, ref):
    import html
    pages = []
    refdir = os.path.join(root, 'docs', 'ConversionStrategies-Reference')
    for dp, _, fns in os.walk(refdir):
        for fn in fns:
            if fn.endswith('.md'):
                pages.append(os.path.join(dp, fn))
    pages.append(os.path.join(root, 'docs', 'ConversionStrategies.md'))
    for extra in ('docs/Roadmap.md', 'docs/PLAN-hop-campaign.md', 'docs/phase3/DESIGN-pointer-core-typeparam.md'):
        pages.append(os.path.join(root, extra.replace('/', os.sep)))
    total = bad = 0
    for p in sorted(pages):
        rel = os.path.relpath(p, root).replace(os.sep, '/')
        r = subprocess.run(['gh', 'api', f'repos/ritchiecarroll/go2cs/contents/{rel}?ref={ref}',
                            '-H', 'Accept: application/vnd.github.html'], capture_output=True)
        if r.returncode != 0:
            print(f'  skip {rel}: not at {ref} on GitHub')
            continue
        ids = [html.unescape(x) for x in
               re.findall(r'<a id="user-content-([^"]*)" class="anchor"', r.stdout.decode('utf-8'))]
        raw = subprocess.run(['git', '-C', root, 'show', f'{ref}:{rel}'], capture_output=True).stdout
        lines = raw.decode('utf-8').replace('\r\n', '\n').split('\n')
        mine = [h[3] for h in rc.heading_slugs(lines)]
        texts = [h[2] for h in rc.heading_slugs(lines)]
        if len(ids) != len(mine):
            print(f'  COUNT {rel}: github {len(ids)} vs {len(mine)}')
            bad += 1
        for a, b, t in zip(ids, mine, texts):
            total += 1
            if a != b:
                bad += 1
                print(f'  MISMATCH {rel}: {t[:100]!r}\n    github {a}\n    mine   {b}')
    print(f'self-test: {total} headings compared with GitHub, {bad} mismatches')
    return 1 if bad else 0


def main():
    args = sys.argv[1:]
    if len(args) >= 2 and args[1] == '--self-test':
        ref = args[args.index('--ref') + 1] if '--ref' in args else 'HEAD'
        sys.exit(self_test(os.path.abspath(args[0]), ref))
    if len(args) < 2 or '--stub-base' not in args:
        print(__doc__)
        sys.exit(2)
    root, manifest = args[0], args[1]
    stub_base = args[args.index('--stub-base') + 1]
    dry = '--dry-run' in args
    repo = Repo(root, stub_base)
    ops = json.load(open(manifest, encoding='utf-8'))['ops']
    try:
        for n, op in enumerate(ops):
            fn = OPS.get(op['op'])
            if fn is None:
                raise Fail(f'op {n}: unknown op {op["op"]!r}')
            try:
                fn(repo, op)
            except Fail as e:
                raise Fail(f'op {n} ({op["op"]}): {e}')
    except Fail as e:
        for l in repo.log:
            print(l)
        print('FAIL:', e, file=sys.stderr)
        sys.exit(1)
    for l in repo.log:
        print(l)
    changed = [d for d in repo.docs.values() if d.changed()]
    for d in sorted(changed, key=lambda d: d.path):
        print(f'  {"would write" if dry else "write"} {repo.rel(d.path)} ({len(d.data())} bytes)')
        if not dry:
            d.write()
    print('DRY RUN: nothing written' if dry else f'WRITTEN: {len(changed)} files')


if __name__ == '__main__':
    main()
