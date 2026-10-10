"""Remove HTML comments from the pages of a BUILT Jekyll site.

The Markdown sources keep their comments: provenance (dated derivations, SHAs, measurements, the
`<!-- source: ... -->` lines beside examples) lives in HTML comments by project rule, so GitHub and a
reader of the file still see it. The published page source does not need it, so this step removes
every `<!-- ... -->` from every .html file under _site after the build and before the site check.

Left untouched:
  - the contents of <script>, <style>, <textarea> and <title> (text there is not parsed as markup,
    so a "<!--" inside is not a comment);
  - attribute values (the scanner steps over each tag, quotes included);
  - IE conditional comments: <!--[if ...]> ... <![endif]--> and the <!--<![endif]--> closer.

A comment's removal leaves nothing behind, except that a comment standing alone on its line takes
the line with it, so the page does not gain runs of blank lines.

Usage: python site-strip-comments.py <_site dir>
"""
import os, re, sys

sys.stdout.reconfigure(encoding="utf-8", errors="replace")

RAW_TEXT = ("script", "style", "textarea", "title")
TAG_NAME = re.compile(r"<([A-Za-z][A-Za-z0-9:-]*)")


def is_conditional(body):
    """An IE conditional comment: '[if ...]>' opener, '<![endif]' closer, or both in one comment."""
    b = body.strip()
    return b.startswith("[if") or b.startswith("<![endif]") or b.endswith("<![endif]")


def tag_end(text, i):
    """Index just past the '>' closing the tag that starts at text[i], stepping over quoted values."""
    n, quote = len(text), None
    j = i + 1
    while j < n:
        c = text[j]
        if quote:
            if c == quote:
                quote = None
        elif c in "\"'":
            quote = c
        elif c == ">":
            return j + 1
        j += 1
    return n


def strip(text):
    """Return (text without comments, number removed, number of conditional comments kept)."""
    out, i, n = [], 0, len(text)
    removed = kept = 0
    while i < n:
        lt = text.find("<", i)
        if lt < 0:
            out.append(text[i:])
            break
        out.append(text[i:lt])
        if text.startswith("<!--", lt):
            # <!--> and <!---> are complete (empty) comments in HTML.
            if text.startswith("<!-->", lt):
                end, body = lt + 5, ""
            elif text.startswith("<!--->", lt):
                end, body = lt + 6, ""
            else:
                close = text.find("-->", lt + 4)
                end = n if close < 0 else close + 3
                body = text[lt + 4:close if close >= 0 else n]
            if is_conditional(body):
                out.append(text[lt:end])
                kept += 1
            else:
                removed += 1
                # A comment alone on its line takes the line with it.
                line_start = text.rfind("\n", 0, lt) + 1
                head = text[line_start:lt]
                tail_end = text.find("\n", end)
                tail = text[end:] if tail_end < 0 else text[end:tail_end]
                if line_start >= i and not head.strip() and not tail.strip():
                    out[-1] = out[-1][:len(out[-1]) - len(head)]
                    end = n if tail_end < 0 else tail_end + 1
            i = end
            continue
        m = TAG_NAME.match(text, lt)
        if not m:
            out.append("<")
            i = lt + 1
            continue
        end = tag_end(text, lt)
        out.append(text[lt:end])
        name = m.group(1).lower()
        if name in RAW_TEXT and not text[lt:end].rstrip(">").rstrip().endswith("/"):
            close = re.compile(r"</" + name + r"[\s>/]", re.I).search(text, end)
            stop = n if close is None else close.start()
            out.append(text[end:stop])
            end = stop
        i = end
    return "".join(out), removed, kept


def main():
    site = sys.argv[1]
    pages = changed = removed = kept = 0
    for dp, _, fns in os.walk(site):
        for fn in fns:
            if not fn.endswith(".html"):
                continue
            full = os.path.join(dp, fn)
            with open(full, encoding="utf-8", errors="surrogateescape") as f:
                text = f.read()
            new, r, k = strip(text)
            pages += 1
            kept += k
            if r:
                changed += 1
                removed += r
                with open(full, "w", encoding="utf-8", errors="surrogateescape", newline="") as f:
                    f.write(new)
    print(f"HTML comments removed: {removed} on {changed} of {pages} pages; IE conditional comments kept: {kept}")
    return 0


if __name__ == "__main__":
    sys.exit(main())
