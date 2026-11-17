#!/usr/bin/env python3
"""Counts for the 2026-11-14 review amendment of DESIGN-interior-pointer-root.md. Run from the repo root.

Over the tracked .cs files under src/core, golib and unsafe excluded:
  fromRef     every line that calls `@unsafe.Pointer.FromRef(`, with its file's kind
  fromBox     the count of `Pointer.FromBox(` and `Pointer.FromPinnedBox(` calls
  rawPointer  every line that casts a number to a raw C# pointer: `(E*)(uintptr)`
  handOwned   per hand-owned file, the old cast in both spellings: `(ж<T>)(uintptr)(` and `(ж<T>)(uintptr)x`
A file is hand-owned when it carries the module marker on a line of its own or is an `_impl` companion.
"""
import re, subprocess

names = subprocess.run(["git", "-c", "core.quotepath=false", "ls-files", "-z", "src/core"],
                       capture_output=True, check=True).stdout.decode("utf-8").split("\0")
MARK = re.compile(r"^\s*\[module:\s*(go\.)?GoManualConversion\]", re.M)
RAW = re.compile(r"\([A-Za-z_][A-Za-z0-9_.<>]*\*\)\(uintptr\)")
TAIL = ">)(uintptr)"


def kind(f, s):
    if f.endswith(".cs.auto"):
        return "review sibling"
    if MARK.search(s) or f.endswith("_impl.cs") or f.endswith("_impl_test.cs"):
        return "hand-owned"
    return "converted test" if f.endswith("_test.cs") else "converted"


def casts(s):
    paren = loose = 0
    i = s.find(TAIL)
    while i >= 0:
        depth, j = 0, i
        while j >= 0:
            if s[j] == ">":
                depth += 1
            elif s[j] == "<":
                depth -= 1
                if depth == 0:
                    break
            j -= 1
        if s[j - 2:j] == "(ж":
            if s[i + len(TAIL):i + len(TAIL) + 1] == "(":
                paren += 1
            else:
                loose += 1
        i = s.find(TAIL, i + 1)
    return paren, loose


from_ref, raw, hand = [], [], []
box = {"FromBox": [0, 0], "FromPinnedBox": [0, 0]}
for f in sorted(names):
    if not (f.endswith(".cs") or f.endswith(".cs.auto")):
        continue
    if f.startswith("src/core/golib/") or f.startswith("src/core/unsafe/"):
        continue
    s = open(f, encoding="utf-8").read()
    k = kind(f, s)
    for door in box:
        n = s.count("Pointer." + door + "(")
        box[door][0] += n
        box[door][1] += 1 if n else 0
    for n, line in enumerate(s.split("\n"), 1):
        code = line.split("//")[0]
        if "@unsafe.Pointer.FromRef(" in code:
            from_ref.append(f"{k}\t{'cast' if '(uintptr)@unsafe.Pointer.FromRef(' in code else 'bare'}\t{f[9:]}:{n}")
        if RAW.search(code):
            raw.append(f"{k}\t{'FromRef' if 'Pointer.FromRef(' in code else '-'}\t{f[9:]}:{n}")
    if k == "hand-owned":
        paren, loose = casts(s)
        if paren or loose:
            hand.append((f[9:], paren, loose))

print(f"fromRef lines (comments excluded): {len(from_ref)}")
print("\n".join(from_ref))
for door, (calls, files) in box.items():
    print(f"{door}: {calls} calls in {files} files")
print(f"rawPointer lines (comments excluded): {len(raw)}")
print("\n".join(raw))
print(f"handOwned files with the old cast: {len(hand)}; "
      f"parenthesised {sum(h[1] for h in hand)}, unparenthesised {sum(h[2] for h in hand)}")
for f, paren, loose in hand:
    print(f"{paren}\t{loose}\t{f}")
