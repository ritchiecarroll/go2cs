#!/usr/bin/env python3
"""Corpus counts for DESIGN-interior-pointer-root.md. Run from the repo root.

Counts, over the tracked non-test .cs files under src/core:
  cast     every `(ж<T>)(uintptr)(` (T matched by bracket depth, so two casts on one line are two)
  overAdd  the casts whose operand starts with `@unsafe.Add(`
  fromRef  every `Pointer.FromRef(` call
  fromRef+ the lines that hold a FromRef call and add to its number: `(uintptr)@unsafe.Pointer.FromRef(...) +`
"""
import collections, re, subprocess

names = subprocess.run(["git", "-c", "core.quotepath=false", "ls-files", "-z", "src/core"],
                       capture_output=True, check=True).stdout.decode("utf-8").split("\0")
files = [f for f in names if f.endswith(".cs") and not f.endswith("_test.cs")]
TAIL = ">)(uintptr)("
cast, over_add, from_ref = collections.Counter(), collections.Counter(), collections.Counter()
plus = []
for f in files:
    top = f.split("/")[2]
    s = open(f, encoding="utf-8").read()
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
            cast[top] += 1
            if s.startswith("@unsafe.Add(", i + len(TAIL)):
                over_add[top] += 1
        i = s.find(TAIL, i + 1)
    from_ref[top] += s.count("Pointer.FromRef(")
    for n, line in enumerate(s.split("\n"), 1):
        if re.search(r"\(uintptr\)@unsafe\.Pointer\.FromRef\([^)]*\)\s*\+", line):
            plus.append(f"{f}:{n}")

def show(label, c):
    print(f"{label}: {sum(c.values())}")
    for k, v in c.most_common():
        if v:
            print(f"  {v:5d}  {k}")

print(f"files: {len(files)}")
show("cast", cast)
show("overAdd", over_add)
show("fromRef", from_ref)
print(f"fromRef+: {len(plus)}")
for p in plus:
    print("  " + p)
