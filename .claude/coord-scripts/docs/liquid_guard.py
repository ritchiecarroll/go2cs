"""Place the Jekyll/Liquid raw guard on docs pages, AFTER the H1.

GitHub Pages renders every docs/ Markdown file through Liquid before Markdown, fenced code included.
A page whose text holds '{{' or '{%' carries a raw guard hidden in HTML comments:
  line 1:  # Title
  line 2:  <!-- {% raw %} — Jekyll/Liquid guard: ... -->
  ...
  last:    <!-- {% endraw %} -->
The guard goes AFTER the H1 because jekyll-titles-from-headings reads the page title only from a
heading at the very start of the file; a guard on line 1 makes the published page's title fall
back to the site name.

Modes (paths are files):
  move  <files...>   an existing guard on line 1 with the H1 on line 2: swap the two lines
  add   <files...>   no guard yet: insert the opener after the H1 and append the closer
Line endings, BOM and the trailing newline are preserved. Refuses anything it does not recognise.
"""
import sys

sys.stdout.reconfigure(encoding="utf-8", errors="replace")

OPEN = ("<!-- {% raw %} — Jekyll/Liquid guard: this page contains Go composite-literal and template "
        "syntax ({{ … }}) that Liquid would otherwise parse; the HTML comment hides the tag on GitHub. -->")
CLOSE = "<!-- {% endraw %} -->"
BOM = "﻿"


def load(path):
    with open(path, "rb") as f:
        raw = f.read()
    text = raw.decode("utf-8")
    bom = text.startswith(BOM)
    if bom:
        text = text[1:]
    nl = "\r\n" if "\r\n" in text else "\n"
    lines = text.split(nl)
    return bom, nl, lines


def save(path, bom, nl, lines):
    text = nl.join(lines)
    if bom:
        text = BOM + text
    with open(path, "wb") as f:
        f.write(text.encode("utf-8"))


def move(path):
    bom, nl, lines = load(path)
    if not ("{% raw %}" in lines[0] and lines[0].startswith("<!--") and lines[1].startswith("# ")):
        raise SystemExit(f"REFUSED move {path}: line 1 is not a raw guard or line 2 is not an H1")
    lines[0], lines[1] = lines[1], lines[0]
    save(path, bom, nl, lines)
    print(f"moved   {path}")


def add(path):
    bom, nl, lines = load(path)
    body = nl.join(lines)
    if "{% raw %}" in body or "{% endraw %}" in body:
        raise SystemExit(f"REFUSED add {path}: it already holds a raw tag")
    if not lines[0].startswith("# "):
        raise SystemExit(f"REFUSED add {path}: line 1 is not an H1")
    lines.insert(1, OPEN)
    # Closer: keep the file's trailing-newline shape. split() leaves '' as the last element when
    # the file ends with a newline.
    if lines[-1] == "":
        lines[-1:] = [CLOSE, ""]
    else:
        lines.append(CLOSE)
    save(path, bom, nl, lines)
    print(f"added   {path}")


def main():
    mode, files = sys.argv[1], sys.argv[2:]
    for p in files:
        {"move": move, "add": add}[mode](p)


if __name__ == "__main__":
    main()
