"""Rewrite links in a BUILT Jekyll site that reach files the site does not publish, so they open on GitHub.

The docs are written for GitHub, where a relative link such as ../src/core/weak/pointer.cs opens the
file in the repository. The Pages site publishes docs/ only, so the same link climbs out of the site
and 404s (the 2026-09-27 Pages audit, item 5). The same holds for a link into a docs/ path that
docs/_config.yml EXCLUDES from the build (the internal records: phase3/, phase4/, doctrine/): the file
is in the repository, but not on the site.

This step runs after the Pages build and before the site check. Every href/src in every .html page
that resolves to a repository path OUTSIDE docs/, or to a docs/ path the build excludes, is replaced
by its GitHub URL at the commit being built (blob/ for a file, tree/ for a directory), keeping any
#fragment or ?query. The Markdown sources are untouched, so GitHub renders them as before.

It also fails the run (exit 1) if the build published anything the exclude list names, so an
exclusion that did not take effect cannot deploy unnoticed.

Usage: python site-outside-links.py <_site dir> <repository root> <commit sha> [owner/repo]
"""
import os, posixpath, re, sys
from urllib.parse import urlsplit, unquote

sys.stdout.reconfigure(encoding="utf-8", errors="replace")

ATTR = re.compile(r'(\s(?:href|src)=")([^"]*)(")')


def excluded_patterns(repo):
    """The exclude list of docs/_config.yml: a block list ('exclude:' then '  - item' lines) or a flow
    list ('exclude: [a, b]'). Only plain path entries are expected; a quoted entry is unquoted."""
    path = os.path.join(repo, "docs", "_config.yml")
    if not os.path.isfile(path):
        return []
    items, in_block = [], False
    with open(path, encoding="utf-8") as f:
        for line in f:
            text = line.split("#", 1)[0].rstrip()
            if in_block:
                m = re.match(r"^\s+-\s*(.+)$", text)
                if m:
                    items.append(m.group(1))
                    continue
                if not text.strip():
                    continue
                in_block = False
            m = re.match(r"^exclude:\s*(.*)$", text)
            if m:
                rest = m.group(1).strip()
                if rest.startswith("[") and rest.endswith("]"):
                    items += [x for x in (s.strip() for s in rest[1:-1].split(",")) if x]
                elif not rest:
                    in_block = True
    return [x.strip().strip("'\"") for x in items if x.strip().strip("'\"")]


def is_excluded(docs_rel, patterns):
    """Jekyll's own string rule: a source path is excluded when it starts with an exclude entry."""
    return any(docs_rel.startswith(p) for p in patterns)


def built_excluded(site, patterns):
    """Every built path that an exclude entry names: proof the exclusion did not take effect."""
    found = []
    for dp, dns, fns in os.walk(site):
        for name in dns + fns:
            rel = os.path.relpath(os.path.join(dp, name), site).replace(os.sep, "/")
            if is_excluded(rel, patterns):
                found.append(rel)
    return sorted(found)


def rewrite(site, repo, sha, slug, patterns):
    pages = changed = links = into_excluded = 0
    for dp, _, fns in os.walk(site):
        for fn in fns:
            if not fn.endswith(".html"):
                continue
            full = os.path.join(dp, fn)
            rel = os.path.relpath(full, site).replace(os.sep, "/")
            src_dir = posixpath.join("docs", posixpath.dirname(rel))
            with open(full, encoding="utf-8", errors="surrogateescape") as f:
                text = f.read()
            count = excl = 0

            def fix(m):
                nonlocal count, excl
                raw = m.group(2)
                sp = urlsplit(raw)
                if sp.scheme or raw.startswith(("//", "#", "/", "mailto:", "data:", "javascript:")) or not sp.path:
                    return m.group(0)
                target = posixpath.normpath(posixpath.join(src_dir, unquote(sp.path)))
                if target.startswith("../") or target == "..":
                    return m.group(0)
                in_docs = target == "docs" or target.startswith("docs/")
                if in_docs and not (target != "docs" and is_excluded(target[len("docs/"):], patterns)):
                    return m.group(0)
                on_disk = os.path.join(repo, *target.split("/"))
                if os.path.isdir(on_disk):
                    kind = "tree"
                elif os.path.isfile(on_disk):
                    kind = "blob"
                else:
                    return m.group(0)
                if in_docs:
                    excl += 1
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
                into_excluded += excl
                with open(full, "w", encoding="utf-8", errors="surrogateescape", newline="") as f:
                    f.write(new)
    print(f"links rewritten to GitHub: {links} on {changed} of {pages} pages (commit {sha[:10]}); "
          f"{links - into_excluded} climb out of docs/, {into_excluded} reach an excluded docs/ path")
    return links


def main():
    site, repo, sha = sys.argv[1], sys.argv[2], sys.argv[3]
    slug = sys.argv[4] if len(sys.argv) > 4 else "ritchiecarroll/go2cs"
    patterns = excluded_patterns(repo)
    print(f"excluded from the build (docs/_config.yml): {patterns}")
    built = built_excluded(site, patterns)
    if built:
        print(f"ERROR: the build published {len(built)} path(s) the exclude list names; the exclusion did not take effect:")
        for p in built[:20]:
            print(f"    {p}")
        return 1
    rewrite(site, repo, sha, slug, patterns)
    return 0


if __name__ == "__main__":
    sys.exit(main())
