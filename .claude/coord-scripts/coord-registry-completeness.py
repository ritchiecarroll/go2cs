#!/usr/bin/env python3
"""Assert every registry entry each seat ADDS survives the merge exactly once.

The silent-duplication class: three seats add different entries to one map in
manualTypeOperations.go. Different offsets, different names -> git merges them
clean and marks nothing, and either a lost entry or a doubled one compiles or
fails far from the merge. The COUNT is the check, not the diff.

Keyed on (package, name), NOT on name: "pipe" legitimately exists under both
"runtime" (at master) and "syscall" (added by c2-darwin-inc10). A per-name
"exactly once" assertion FALSE-FIRES on it -- verified before this was written.

  usage: coord-registry-completeness.py <merge-ref> [seat-branch ...]
         (default seats: the three that touch the file)
"""
import re
import subprocess
import sys

REPO = r"C:\Projects\go2cs"
PATH = "src/go2cs/manualTypeOperations.go"
MASTER = "origin/master"
DEFAULT_SEATS = [
    "origin/claude/g-wsasendto-seat",
    "origin/claude/c2-darwin-inc10",
    "origin/claude/c2-darwin-ptrout",
]

PKG = re.compile(r'^\t"([^"]+)":\s*\{')
ENTRY = re.compile(r'^\t\t"([^"]+)":\s*(goos\w+)')


def blob(ref):
    # utf-8 EXPLICIT: the file carries the converter glyph family and Python's
    # Windows default (cp1252) dies on it. It died loudly here, which is the
    # right failure mode -- a silent decode would have returned a short file
    # and every count below would have been wrong and plausible.
    out = subprocess.run(["git", "show", f"{ref}:{PATH}"], cwd=REPO,
                         capture_output=True, text=True,
                         encoding="utf-8", errors="strict")
    if out.returncode != 0:
        sys.exit(f"cannot read {PATH} at {ref}")
    return out.stdout.splitlines()


def pairs(ref):
    """(package, name) -> how many times it appears."""
    seen, pkg = {}, None
    for line in blob(ref):
        m = PKG.match(line)
        if m:
            pkg = m.group(1)
            continue
        m = ENTRY.match(line)
        if m and pkg:
            seen[(pkg, m.group(1))] = seen.get((pkg, m.group(1)), 0) + 1
    return seen


def main():
    if len(sys.argv) < 2:
        sys.exit(__doc__)
    target = sys.argv[1]
    seats = sys.argv[2:] or DEFAULT_SEATS

    base = pairs(MASTER)
    merged = pairs(target)

    owed, fail = {}, 0
    for seat in seats:
        added = {k for k in pairs(seat) if k not in base}
        for k in added:
            owed.setdefault(k, []).append(seat.split("/")[-1])

    print(f"registry completeness at {target}")
    print(f"  master entries {len(base)}   merged entries {len(merged)}")
    print(f"  entries owed by {len(seats)} seats: {len(owed)}\n")

    for (pkg, name), who in sorted(owed.items()):
        n = merged.get((pkg, name), 0)
        tag = "OK  " if n == 1 else ("LOST" if n == 0 else "DUP ")
        if n != 1:
            fail = 1
        print(f"  {tag} {pkg}.{name:<24} count={n}  from {','.join(who)}")

    # A duplicate ANYWHERE is a merge defect even if no seat owed it.
    strays = {k: v for k, v in merged.items() if v > 1}
    if strays:
        fail = 1
        print("\n  DUPLICATE (pkg,name) pairs in the merge result:")
        for (pkg, name), n in sorted(strays.items()):
            print(f"    {pkg}.{name} x{n}")

    print("\n" + ("REGISTRY COMPLETE" if not fail
                  else "REGISTRY VIOLATED -- do not push this merge"))
    return fail


if __name__ == "__main__":
    sys.exit(main())
