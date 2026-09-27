"""Emulate Liquid 4.0.4's tokenizer over the files Jekyll (github-pages v232) renders under docs/.

Liquid runs over the WHOLE file, fenced code included, before Markdown. Its tokenizer is
  TemplateParser = /({%.*?%}|{{.*?}}?|{%|{{)/m     (dot matches newline)
A '{{' token ends at the FIRST '}' (optionally '}}'); a token that does not end in '}}' raises
"Variable ... was not properly terminated". A '{%' without '%}' raises "Tag ... was not properly
terminated". A '{% name ... %}' with an unknown name raises "Unknown tag". A well-formed '{{ x }}'
renders SILENTLY (usually as empty text): that is content loss, not an error.

Usage: python liquid_scan.py <docs-root> [--json out.json]
"""
import json, os, re, sys

sys.stdout.reconfigure(encoding="utf-8", errors="replace")

TOKEN = re.compile(r"(\{%.*?%\}|\{\{.*?\}\}?|\{%|\{\{)", re.S)
VAR_OK = re.compile(r"\A\{\{-?(.*?)-?\}\}\Z", re.S)
TAG_OK = re.compile(r"\A\{%-?\s*(\w+)\s*(.*?)-?%\}\Z", re.S)
# Tags known to Jekyll 3.10 + Liquid 4 + the github-pages plugin set.
KNOWN_TAGS = {
    "assign", "capture", "endcapture", "case", "when", "else", "endcase", "comment", "endcomment",
    "cycle", "for", "endfor", "break", "continue", "if", "elsif", "endif", "unless", "endunless",
    "ifchanged", "endifchanged", "include", "increment", "decrement", "raw", "endraw", "tablerow",
    "endtablerow", "highlight", "endhighlight", "link", "post_url", "include_relative", "seo",
    "github_edit_link", "avatar", "feed_meta",
}


def rendered_files(root):
    """Files Jekyll renders through Liquid: .md/.markdown (jekyll-optional-front-matter renders
    every Markdown file) and any other file that starts with YAML front matter. Paths starting
    with _ . # ~ are excluded by Jekyll, except the _layouts/_includes it reads as templates."""
    for dp, dns, fns in os.walk(root):
        rel = os.path.relpath(dp, root)
        parts = [] if rel == "." else rel.replace("\\", "/").split("/")
        if any(p[:1] in "_.#~" for p in parts):
            dns[:] = []
            continue
        dns[:] = [d for d in dns if d[:1] not in "_.#~"]
        for fn in fns:
            if fn[:1] in "_.#~":
                continue
            p = os.path.join(dp, fn)
            if fn.lower().endswith((".md", ".markdown")):
                yield p, "markdown"
            else:
                try:
                    with open(p, "rb") as f:
                        head = f.read(5)
                except OSError:
                    continue
                if head.lstrip(b"\xef\xbb\xbf").startswith(b"---"):
                    yield p, "front-matter"


def line_of(text, idx):
    return text.count("\n", 0, idx) + 1


def scan(path):
    with open(path, "rb") as f:
        raw = f.read()
    text = raw.decode("utf-8", errors="replace")
    out = []
    # Strip YAML front matter the way Jekyll does (only when the file starts with ---).
    body_start = 0
    m = re.match(r"\A(﻿)?---\s*\r?\n(.*?\r?\n)?(---|\.\.\.)\s*\r?\n", text, re.S)
    if m:
        body_start = m.end()
    elif re.match(r"\A(﻿)?---", text):
        out.append(("front-matter-unterminated", 1, text[:40]))
    raw_depth = False
    for tm in TOKEN.finditer(text, body_start):
        tok = tm.group(0)
        ln = line_of(text, tm.start())
        if raw_depth:
            t = TAG_OK.match(tok)
            if t and t.group(1) == "endraw":
                raw_depth = False
            continue
        if tok.startswith("{{"):
            if VAR_OK.match(tok):
                out.append(("silent-variable", ln, tok[:80]))
            else:
                out.append(("ERROR-variable-unterminated", ln, tok[:80]))
        else:
            t = TAG_OK.match(tok)
            if not t:
                out.append(("ERROR-tag-unterminated", ln, tok[:80]))
            elif t.group(1) not in KNOWN_TAGS:
                out.append(("ERROR-unknown-tag", ln, tok[:80]))
            elif t.group(1) == "raw":
                raw_depth = True
            else:
                out.append(("liquid-tag", ln, tok[:80]))
    if raw_depth:
        out.append(("ERROR-raw-unclosed", line_of(text, len(text)), ""))
    return out


def main():
    root = sys.argv[1]
    results = {}
    counts = {}
    for p, kind in sorted(rendered_files(root)):
        hits = scan(p)
        if hits:
            rel = os.path.relpath(p, root).replace("\\", "/")
            results[rel] = [{"class": c, "line": l, "text": t} for c, l, t in hits]
            for c, _, _ in hits:
                counts[c] = counts.get(c, 0) + 1
    files_by_class = {}
    for rel, hs in results.items():
        for h in hs:
            files_by_class.setdefault(h["class"], set()).add(rel)
    print("files with any token:", len(results))
    for c in sorted(counts):
        print(f"  {c}: {counts[c]} hits in {len(files_by_class[c])} files")
    for rel, hs in results.items():
        errs = [h for h in hs if h["class"].startswith("ERROR")]
        sil = [h for h in hs if h["class"] == "silent-variable"]
        tags = [h for h in hs if h["class"] == "liquid-tag"]
        print(f"{rel}: errors={len(errs)} silent={len(sil)} tags={len(tags)}"
              + (f" first-error L{errs[0]['line']} {errs[0]['text']!r}" if errs else ""))
    if "--json" in sys.argv:
        with open(sys.argv[sys.argv.index("--json") + 1], "w", encoding="utf-8") as f:
            json.dump(results, f, ensure_ascii=False, indent=1)


if __name__ == "__main__":
    main()
