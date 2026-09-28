"""Rewrite links that climb out of docs/ in a BUILT Jekyll site so they reach the repository on GitHub.

The docs are written for GitHub, where a relative link such as ../src/core/weak/pointer.cs opens the
file in the repository. The Pages site publishes docs/ only, so the same link climbs out of the site
and 404s (the 2026-09-27 Pages audit, item 5). This step runs after the Pages build and before the
site check: every href/src in every .html page that resolves OUTSIDE docs/ but INSIDE the repository
is replaced by its GitHub URL at the commit being built (blob/ for a file, tree/ for a directory),
keeping any #fragment or ?query. The Markdown sources are untouched, so GitHub renders them as before.

Usage: python site-outside-links.py <_site dir> <repository root> <commit sha> [owner/repo]
"""
import os, posixpath, re, sys
from urllib.parse import urlsplit, unquote

sys.stdout.reconfigure(encoding="utf-8", errors="replace")

ATTR = re.compile(r'(\s(?:href|src)=")([^"]*)(")')


def rewrite(site, repo, sha, slug):
    pages = changed = links = 0
    for dp, _, fns in os.walk(site):
        for fn in fns:
            if not fn.endswith(".html"):
                continue
            full = os.path.join(dp, fn)
            rel = os.path.relpath(full, site).replace(os.sep, "/")
            src_dir = posixpath.join("docs", posixpath.dirname(rel))
            with open(full, encoding="utf-8", errors="surrogateescape") as f:
                text = f.read()
            count = 0

            def fix(m):
                nonlocal count
                raw = m.group(2)
                sp = urlsplit(raw)
                if sp.scheme or raw.startswith(("//", "#", "/", "mailto:", "data:", "javascript:")) or not sp.path:
                    return m.group(0)
                target = posixpath.normpath(posixpath.join(src_dir, unquote(sp.path)))
                if target == "docs" or target.startswith("docs/") or target.startswith("../") or target == "..":
                    return m.group(0)
                on_disk = os.path.join(repo, *target.split("/"))
                if os.path.isdir(on_disk):
                    kind = "tree"
                elif os.path.isfile(on_disk):
                    kind = "blob"
                else:
                    return m.group(0)
                url = f"https://github.com/{slug}/{kind}/{sha}/{target}"
                if sp.query:
                    url += "?" + sp.query
                if sp.fragment:
                    url += "#" + sp.fragment
                count += 1
                return m.group(1) + url + m.group(3)

            new = ATTR.sub(fix, text)
            pages += 1
            if count:
                changed += 1
                links += count
                with open(full, "w", encoding="utf-8", errors="surrogateescape", newline="") as f:
                    f.write(new)
    print(f"outside-docs links rewritten: {links} on {changed} of {pages} pages (commit {sha[:10]})")
    return links


if __name__ == "__main__":
    site, repo, sha = sys.argv[1], sys.argv[2], sys.argv[3]
    slug = sys.argv[4] if len(sys.argv) > 4 else "ritchiecarroll/go2cs"
    rewrite(site, repo, sha, slug)
