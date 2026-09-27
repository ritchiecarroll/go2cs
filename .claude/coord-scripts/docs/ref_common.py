#!/usr/bin/env python3
"""Shared Markdown helpers for the conversion-strategy reference tools (ref_moves.py, ref_invariant.py).

Everything here is line-based and fence-aware, and it reads and writes files byte-faithfully: a file's
line ending, BOM and trailing newline are detected on read and restored on write.

The slug function reproduces GitHub's heading anchors (github-slugger over the heading's rendered text):
inline code keeps its text verbatim (so `slice<T>` contributes "slicet"), raw HTML tags outside code are
dropped, entities are decoded, the text is lower-cased, every character that is not a letter, mark,
number, connector punctuation, hyphen or space is removed, and each space becomes a hyphen. A repeated
slug gets -1, -2, ... in document order. ref_moves.py --self-test checks it against headings whose
anchors GitHub's renderer produced.
"""
import html
import os
import re
import unicodedata

FENCE_RE = re.compile(r'^(\s{0,3})(`{3,}|~{3,})(.*)$')
HEADING_RE = re.compile(r'^(#{1,6})[ \t]+(.*?)(?:[ \t]+#+)?[ \t]*$')
CODE_SPAN_RE = re.compile(r'(`+)(.+?)(?<!`)\1(?!`)')
LINK_RE = re.compile(r'(\]\()([^()\s]*(?:\([^()\s]*\)[^()\s]*)*)((?:\s+"[^"]*")?\))')
REFDEF_RE = re.compile(r'^(\s{0,3}\[[^\]]+\]:\s*)(\S+)(.*)$')
ANCHOR_ID_RE = re.compile(r'<a\s+(?:id|name)="([^"]+)"')
HTML_TAG_RE = re.compile(r'</?[A-Za-z][A-Za-z0-9-]*(?:\s[^<>]*)?/?>')

RAW_OPEN_PREFIX = '<!-- {% raw %}'
RAW_CLOSE = '<!-- {% endraw %} -->'


class Doc:
    """A text file as lines, with the exact encoding facts needed to write it back unchanged."""

    def __init__(self, path):
        self.path = path
        raw = open(path, 'rb').read()
        self.bom = raw.startswith(b'\xef\xbb\xbf')
        text = raw[3:].decode('utf-8') if self.bom else raw.decode('utf-8')
        self.nl = '\r\n' if '\r\n' in text else '\n'
        self.endnl = text.endswith(self.nl)
        body = text[:-len(self.nl)] if self.endnl else text
        self.lines = body.split(self.nl) if body else []
        self.original = raw

    @classmethod
    def new(cls, path, nl='\r\n'):
        d = cls.__new__(cls)
        d.path, d.bom, d.nl, d.endnl, d.lines, d.original = path, False, nl, True, [], None
        return d

    def text(self):
        t = self.nl.join(self.lines) + (self.nl if self.endnl else '')
        return t

    def data(self):
        b = self.text().encode('utf-8')
        return (b'\xef\xbb\xbf' + b) if self.bom else b

    def changed(self):
        return self.original != self.data()

    def write(self):
        d = os.path.dirname(self.path)
        if d:
            os.makedirs(d, exist_ok=True)
        with open(self.path, 'wb') as f:
            f.write(self.data())


def fence_mask(lines):
    """True for every line inside (or delimiting) a fenced code block."""
    mask = [False] * len(lines)
    open_ch, open_len = None, 0
    for i, l in enumerate(lines):
        m = FENCE_RE.match(l)
        if open_ch is None:
            if m and not (m.group(2)[0] == '`' and '`' in m.group(3)):
                open_ch, open_len = m.group(2)[0], len(m.group(2))
                mask[i] = True
        else:
            mask[i] = True
            if m and m.group(2)[0] == open_ch and len(m.group(2)) >= open_len and m.group(3).strip() == '':
                open_ch = None
    return mask


def headings(lines, mask=None):
    """[(line index, level, text)] for every ATX heading outside fences."""
    if mask is None:
        mask = fence_mask(lines)
    out = []
    for i, l in enumerate(lines):
        if mask[i]:
            continue
        m = HEADING_RE.match(l)
        if m:
            out.append((i, len(m.group(1)), m.group(2)))
    return out


def _render_plain(t):
    t = re.sub(r'<!--.*?-->', '', t)
    t = re.sub(r'!\[([^\]]*)\]\([^)]*\)', r'\1', t)
    t = re.sub(r'\[([^\]]*)\]\([^)]*\)', r'\1', t)
    t = HTML_TAG_RE.sub('', t)
    t = re.sub(r'\\([!-/:-@\[-`{-~])', r'\1', t)
    t = html.unescape(t)
    # emphasis: '*' is removed by the slug anyway; '_' survives it, so strip underscore emphasis
    t = re.sub(r'(?<![A-Za-z0-9_])__(?=\S)(.+?)(?<=\S)__(?![A-Za-z0-9_])', r'\1', t)
    t = re.sub(r'(?<![A-Za-z0-9_])_(?=\S)(.+?)(?<=\S)_(?![A-Za-z0-9_])', r'\1', t)
    return t


def render_text(text):
    """The heading's text as GitHub renders it: code spans verbatim, markup elsewhere removed."""
    out = []
    pos = 0
    for m in CODE_SPAN_RE.finditer(text):
        out.append(_render_plain(text[pos:m.start()]))
        code = m.group(2)
        if len(code) >= 2 and code[0] == ' ' and code[-1] == ' ' and code.strip():
            code = code[1:-1]
        out.append(code)
        pos = m.end()
    out.append(_render_plain(text[pos:]))
    return ''.join(out)


def slug(text):
    keep = []
    for ch in render_text(text).strip().lower():
        cat = unicodedata.category(ch)
        if cat[0] in 'LMN' or cat == 'Pc' or ch in '- ':
            keep.append(ch)
    return ''.join(keep).replace(' ', '-')


def github_slugs(texts):
    """github-slugger: first occurrence 'x', later ones 'x-1', 'x-2', ... skipping any slug taken."""
    occ = {}
    res = []
    for t in texts:
        base = slug(t)
        s = base
        while s in occ:
            occ[base] += 1
            s = f'{base}-{occ[base]}'
        occ[s] = 0
        res.append(s)
    return res


def heading_slugs(lines):
    """[(line index, level, text, slug)] for a page."""
    hs = headings(lines)
    return [(i, lv, t, s) for (i, lv, t), s in zip(hs, github_slugs([t for _, _, t in hs]))]


def anchor_ids(lines):
    mask = fence_mask(lines)
    out = []
    for i, l in enumerate(lines):
        if mask[i]:
            continue
        for a in ANCHOR_ID_RE.findall(CODE_SPAN_RE.sub('', l)):
            out.append(a)
    return out


def anchors(lines):
    return {s for (_, _, _, s) in heading_slugs(lines)} | set(anchor_ids(lines))


def is_external(t):
    return re.match(r'^[a-zA-Z][a-zA-Z0-9+.-]*:', t) is not None or t.startswith('//')


def rewrite_links_in_line(line, fn):
    """Apply fn(target) -> target to every inline link target outside code spans."""
    parts = []
    pos = 0
    for m in CODE_SPAN_RE.finditer(line):
        parts.append(('t', line[pos:m.start()]))
        parts.append(('c', m.group(0)))
        pos = m.end()
    parts.append(('t', line[pos:]))
    out = []
    for kind, s in parts:
        if kind == 'c':
            out.append(s)
        else:
            out.append(LINK_RE.sub(lambda m: m.group(1) + fn(m.group(2)) + m.group(3), s))
    return ''.join(out)


def rewrite_lines(lines, fn, mask=None):
    """fn(target, line index) -> target, over inline links and reference definitions outside fences."""
    if mask is None:
        mask = fence_mask(lines)
    out = []
    for i, l in enumerate(lines):
        if mask[i]:
            out.append(l)
            continue
        m = REFDEF_RE.match(l)
        if m and not l.lstrip().startswith('[^'):
            out.append(m.group(1) + fn(m.group(2), i) + m.group(3))
            continue
        out.append(rewrite_links_in_line(l, lambda t, i=i: fn(t, i)))
    return out


def link_targets(lines):
    """[(line index, target)] for every link outside fences and code spans."""
    found = []
    rewrite_lines(lines, lambda t, i: (found.append((i, t)), t)[1])
    return found


def resolve(doc_path, target):
    """(absolute normalized path or None, fragment) for a relative link target written in doc_path."""
    path, _, frag = target.partition('#')
    if not path:
        return os.path.normpath(doc_path), frag
    if path.startswith('/') or is_external(target):
        return None, frag
    return os.path.normpath(os.path.join(os.path.dirname(doc_path), link_path(path))), frag


def link_path(path):
    """The file part of a link target as a path: query dropped, percent-escapes decoded."""
    from urllib.parse import unquote
    return unquote(path.split('?')[0])


def relpath(target_file, from_doc):
    return os.path.relpath(target_file, os.path.dirname(from_doc)).replace(os.sep, '/')


def normalize_line(l, is_heading_line):
    """A line with link targets blanked and heading level dropped (for the lossless check)."""
    if is_heading_line:
        m = HEADING_RE.match(l)
        l = '#' + m.group(2)
    l = rewrite_links_in_line(l, lambda t: '')
    rd = REFDEF_RE.match(l)
    if rd:
        l = rd.group(1)
    return l.rstrip()


def repo_markdown_files(root, tops=('docs/', '.claude/', 'src/'), singles=('README.md', 'CLAUDE.md')):
    """Every tracked or new (not ignored) .md under docs/, .claude/ and src/, plus README.md and CLAUDE.md."""
    import subprocess
    out = subprocess.run(['git', '-C', root, 'ls-files', '--cached', '--others', '--exclude-standard', '-z',
                          '--', '*.md'], capture_output=True, check=True).stdout.decode('utf-8')
    res = set()
    for rel in out.split('\0'):
        if not rel:
            continue
        if rel in singles or any(rel.startswith(t) for t in tops):
            p = os.path.normpath(os.path.join(root, rel))
            if os.path.exists(p):
                res.add(p)
    return sorted(res)
