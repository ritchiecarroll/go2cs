"""Check a BUILT Jekyll site (_site) the way a visitor's browser sees it.

For every .html page: collect element ids, the <title>, and every href/src. Resolve each
site-internal URL against the page's URL the way a browser does (clamping '..' at the root), and
report:
  - broken targets (no file, no dir/index.html, no .html twin), split into
      outside-site  : the Markdown link climbed out of docs/ (e.g. ../src/...), so the target lives
                      in the repository but not on the site
      inside-site   : a real site path that is missing
  - broken fragments (the target page has no element with that id)
  - pages whose <title> is the site default (the home page's title), meaning no page title was found

The docs-site workflow gates a deploy on this check with a BASELINE: the findings the site already
had when the check landed (the 2026-09-27 Pages audit). A finding not in the baseline fails the run;
a baseline finding that is gone is reported so the baseline can shrink. Line numbers are not part of
a finding's identity, so an edit above a known break does not read as a new one.

Usage: python site-check.py <_site dir> <out.json> [--baseline FILE] [--write-baseline FILE]
"""
import json, os, posixpath, re, sys
from html.parser import HTMLParser
from urllib.parse import urlsplit, unquote

sys.stdout.reconfigure(encoding="utf-8", errors="replace")


class Page(HTMLParser):
    def __init__(self):
        super().__init__(convert_charrefs=True)
        self.ids, self.refs, self.title, self._in_title = set(), [], "", False

    def handle_starttag(self, tag, attrs):
        a = dict(attrs)
        for k in ("id", "name"):
            if a.get(k) and (k == "id" or tag == "a"):
                self.ids.add(a[k])
        for k in ("href", "src"):
            if a.get(k) is not None and tag in ("a", "link", "script", "img", "iframe", "source"):
                self.refs.append((tag, k, a[k], self.getpos()[0]))
        if tag == "title":
            self._in_title = True

    def handle_endtag(self, tag):
        if tag == "title":
            self._in_title = False

    def handle_data(self, data):
        if self._in_title:
            self.title += data


def url_of(rel):
    u = "/" + rel.replace(os.sep, "/")
    return u


def exists(site, path):
    """Pages serves /a/b as a/b, a/b.html or a/b/index.html."""
    p = path.lstrip("/")
    full = os.path.join(site, *p.split("/")) if p else site
    if p == "" or path.endswith("/"):
        return os.path.isfile(os.path.join(full, "index.html")), os.path.join(p, "index.html")
    if os.path.isfile(full):
        return True, p
    if os.path.isfile(full + ".html"):
        return True, p + ".html"
    if os.path.isdir(full) and os.path.isfile(os.path.join(full, "index.html")):
        return True, posixpath.join(p, "index.html")
    return False, p


def findings(res):
    """Each finding as a stable key: its class, the page it is on, and the reference it makes."""
    keys = []
    for kind, items in res["broken"].items():
        keys += [f"broken {kind} :: {b['page']} :: {b['href']}" for b in items]
    keys += [f"fragment :: {x['page']} :: {x['href']}" for x in res["fragments"]]
    keys += [f"default title :: {p}" for p in res["default_titles"]]
    return keys


def compare(keys, baseline_path):
    """Fail on any finding the baseline does not hold (counted, so a second copy is new too)."""
    from collections import Counter
    with open(baseline_path, encoding="utf-8") as f:
        base = Counter(json.load(f)["findings"])
    now = Counter(keys)
    new = sorted((now - base).elements())
    gone = sorted((base - now).elements())
    print(f"baseline: {sum(base.values())} known findings; {len(new)} new, {len(gone)} fixed")
    for k in new:
        print(f"NEW: {k}")
    for k in gone[:50]:
        print(f"fixed (shrink the baseline): {k}")
    return 1 if new else 0


def main():
    args = sys.argv[1:]
    opts = {}
    for flag in ("--baseline", "--write-baseline"):
        if flag in args:
            i = args.index(flag)
            opts[flag] = args[i + 1]
            del args[i:i + 2]
    site, out = args[0], args[1]
    pages = {}
    for dp, _, fns in os.walk(site):
        for fn in fns:
            if fn.endswith(".html"):
                full = os.path.join(dp, fn)
                rel = os.path.relpath(full, site).replace(os.sep, "/")
                pg = Page()
                with open(full, encoding="utf-8", errors="replace") as f:
                    pg.feed(f.read())
                pages[rel] = pg
    default_titles = {}
    broken = {"outside-site": [], "inside-site": []}
    frag = []
    counts = {"pages": len(pages), "refs": 0, "external": 0, "internal": 0, "fragment-only": 0}
    for rel, pg in sorted(pages.items()):
        t = " ".join(pg.title.split())
        default_titles.setdefault(t, []).append(rel)
        base = url_of(rel)
        depth = rel.count("/")
        for tag, attr, raw, line in pg.refs:
            counts["refs"] += 1
            u = raw.strip()
            if not u or u.startswith(("mailto:", "javascript:", "data:", "tel:")):
                continue
            sp = urlsplit(u)
            if sp.scheme or u.startswith("//"):
                counts["external"] += 1
                continue
            if not sp.path:
                counts["fragment-only"] += 1
                target_rel, ok = rel, True
            else:
                counts["internal"] += 1
                if sp.path.startswith("/"):
                    resolved = sp.path
                    climbed = False
                else:
                    segs = sp.path.split("/")
                    ups = 0
                    for s in segs:
                        if s == "..":
                            ups += 1
                        elif s not in (".", ""):
                            break
                    climbed = ups > depth
                    resolved = posixpath.normpath(posixpath.join(posixpath.dirname(base), sp.path))
                    if sp.path.endswith("/") and not resolved.endswith("/"):
                        resolved += "/"
                    if not resolved.startswith("/"):
                        resolved = "/" + resolved.lstrip("./")
                ok, target_rel = exists(site, unquote(resolved))
                if not ok:
                    kind = "outside-site" if climbed else "inside-site"
                    broken[kind].append({"page": rel, "line": line, "tag": tag, "href": raw, "resolved": resolved})
                    continue
            if sp.fragment and target_rel.endswith(".html"):
                tp = pages.get(target_rel)
                fr = unquote(sp.fragment)
                if tp is not None and fr not in tp.ids and fr != "top":
                    frag.append({"page": rel, "line": line, "href": raw, "target": target_rel, "fragment": fr})
    home = " ".join(pages["index.html"].title.split()) if "index.html" in pages else None
    res = {"counts": counts, "broken": broken, "fragments": frag,
           "titles": {t: v for t, v in default_titles.items() if len(v) > 1},
           "default_titles": sorted(p for p in default_titles.get(home, []) if p != "index.html")}
    with open(out, "w", encoding="utf-8") as f:
        json.dump(res, f, ensure_ascii=False, indent=1)
    print(json.dumps(counts))
    for k, v in broken.items():
        pages_hit = len({b["page"] for b in v})
        print(f"broken {k}: {len(v)} refs on {pages_hit} pages")
        by_prefix = {}
        for b in v:
            key = "/".join(b["resolved"].split("/")[:3])
            by_prefix[key] = by_prefix.get(key, 0) + 1
        for p, n in sorted(by_prefix.items(), key=lambda x: -x[1])[:12]:
            print(f"    {n:6d}  {p}")
    print(f"broken fragments: {len(frag)} on {len({x['page'] for x in frag})} pages")
    print(f"pages with the site default title {home!r}: {len(res['default_titles'])} {res['default_titles'][:6]}")
    keys = findings(res)
    if "--write-baseline" in opts:
        with open(opts["--write-baseline"], "w", encoding="utf-8", newline="\n") as f:
            json.dump({"findings": sorted(keys)}, f, ensure_ascii=False, indent=1)
            f.write("\n")
        print(f"baseline written: {len(keys)} findings")
    if "--baseline" in opts:
        return compare(keys, opts["--baseline"])
    return 0


if __name__ == "__main__":
    sys.exit(main())
