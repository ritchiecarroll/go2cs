#!/usr/bin/env python3
"""check_includes.py <staged tree>: every relative, literal EmbeddedResource/Compile/ProjectReference/Import path in every .csproj must exist.
A staged tree that drops a file a project names does not build for a visitor (the go:embed data files did)."""
import os, re, sys
root = sys.argv[1]; missing = []; n = 0
for r, _, fs in os.walk(root):
    for f in fs:
        if not f.endswith(".csproj"): continue
        p = os.path.join(r, f); s = open(p, encoding="utf-8").read()
        for m in re.finditer(r'<(?:EmbeddedResource|Compile|ProjectReference|Import)\s+(?:Include|Project)="([^"]+)"', s):
            v = m.group(1)
            if any(c in v for c in "$%*@;") or v.startswith(("http", "Microsoft.")): continue
            n += 1
            if not os.path.exists(os.path.normpath(os.path.join(r, v.replace("\\", "/")))):
                missing.append(f"{os.path.relpath(p, root)}: {v}")
print(f"includes checked {n}, missing {len(missing)}")
for x in missing[:40]: print("  MISSING", x)
sys.exit(1 if missing else 0)
