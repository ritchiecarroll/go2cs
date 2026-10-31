"""endregen_measure.py -- the measuring half of endregen.sh (run with python -I -X utf8).

  endregen_measure.py <run-dir> <goos> [<goos> ...]
      For each target, compare <run>/<goos>/src/core with <run>/committed/src/core, CR-normalised (CRLF -> LF), and
      write into <run>/report/:
        files-<goos>.tsv   one row per differing file: path, kind, change (modified | emitted-new), hunks, +lines, -lines
        diff-<goos>.patch  a git-style unified diff (3 lines of context) of every modified file, for pcensus classify
        markers.tsv        the path-precise marker gate: every committed [module: GoManualConversion] file, and per
                           target whether the root still holds it byte-for-byte (a fresh emission there is a CLOBBER)
      and print one summary block per target (counts by kind, totals, the marker gate). Exit 1 on any marker clobber.

  endregen_measure.py nulcheck <root-src-core> <stamp-file>
      Exit 1 when any .cs written after the stamp carries a NUL byte (a position sentinel left behind by a raced or
      failed emission); print the count either way.
"""
import difflib, os, re, sys

MARKER = re.compile(rb"^\s*\[module:\s*(go\.)?GoManualConversion\]", re.M)


def kind_of(path):
    name = path.rsplit("/", 1)[-1]
    if name in ("package_info.cs",) or name.startswith("package_info"):
        return "package_info.cs"
    if name.endswith("_test.cs") or name.startswith("package_test_info"):
        return "test.cs"
    if name.endswith(".cs.auto"):
        return ".cs.auto"
    if name.endswith(".cs"):
        return ".cs"
    if name.endswith(".csproj"):
        return ".csproj"
    if name == "README.md":
        return "README.md"
    return "other"


def walk(root):
    out = {}
    for dirpath, dirnames, filenames in os.walk(root):
        dirnames[:] = [d for d in dirnames if d not in ("bin", "obj", "Generated")]
        for f in filenames:
            full = os.path.join(dirpath, f)
            out[os.path.relpath(full, root).replace(os.sep, "/")] = full
    return out


def norm(path):
    with open(path, "rb") as fh:
        return fh.read().replace(b"\r\n", b"\n")


def lines_of(data):
    return data.decode("utf-8", errors="surrogateescape").split("\n")


def nulcheck(root, stamp):
    since = os.path.getmtime(stamp)
    bad = [p for p, full in walk(root).items() if p.endswith(".cs") and os.path.getmtime(full) > since and b"\x00" in open(full, "rb").read()]
    print(f"nulcheck: {len(bad)} written .cs carry a NUL")
    for p in bad[:20]:
        print("  " + p)
    return 1 if bad else 0


def measure(run, targets):
    rep = os.path.join(run, "report")
    base_root = os.path.join(run, "committed", "src", "core")
    base = walk(base_root)
    base_norm = {}

    def committed(p):
        if p not in base_norm:
            base_norm[p] = norm(base[p])
        return base_norm[p]

    marked = sorted(p for p in base if p.endswith(".cs") and MARKER.search(open(base[p], "rb").read()))
    gate = {p: {} for p in marked}
    clobbers = 0

    for t in targets:
        root = os.path.join(run, t, "src", "core")
        files = walk(root)
        rows, by_kind, plus, minus = [], {}, 0, 0
        with open(os.path.join(rep, f"diff-{t}.patch"), "w", encoding="utf-8", errors="surrogateescape", newline="\n") as patch:
            for p in sorted(files):
                if p == "convert.stamp":
                    continue
                new = norm(files[p])
                if p in base:
                    old = committed(p)
                    if old == new:
                        continue
                    a, b = lines_of(old), lines_of(new)
                    ops = [op for op in difflib.SequenceMatcher(None, a, b, autojunk=False).get_opcodes() if op[0] != "equal"]
                    add = sum(j2 - j1 for _, i1, i2, j1, j2 in ops)
                    rem = sum(i2 - i1 for _, i1, i2, j1, j2 in ops)
                    rows.append((p, kind_of(p), "modified", len(ops), add, rem))
                    patch.write(f"diff --git a/{p} b/{p}\n")
                    for ln in difflib.unified_diff(a, b, f"a/{p}", f"b/{p}", n=3, lineterm=""):
                        patch.write(ln + "\n")
                else:
                    n = len(lines_of(new))
                    rows.append((p, kind_of(p), "emitted-new", 1, n, 0))
                    add, rem = n, 0
                k = kind_of(p)
                by_kind[k] = by_kind.get(k, 0) + 1
                plus += add
                minus += rem
        with open(os.path.join(rep, f"files-{t}.tsv"), "w", encoding="utf-8", newline="\n") as tsv:
            tsv.write("path\tkind\tchange\thunks\tplus\tminus\n")
            for r in rows:
                tsv.write("\t".join(str(x) for x in r) + "\n")
        kinds = ", ".join(f"{k} {v}" for k, v in sorted(by_kind.items()))
        new_files = sum(1 for r in rows if r[2] == "emitted-new")
        print(f"MEASURE {t}: {len(rows)} files differ from committed ({kinds}); {new_files} emitted-new; "
              f"hunks {sum(r[3] for r in rows)}; +{plus}/-{minus} lines (CR-normalised)")
        hit = 0
        for p in marked:
            if p in files and norm(files[p]) == committed(p):
                gate[p][t] = "held"
            else:
                gate[p][t] = "CLOBBERED" if p in files else "MISSING"
                hit += 1
        clobbers += hit
        print(f"MARKER GATE {t}: {len(marked)} marked committed files, {hit} not held")

    with open(os.path.join(rep, "markers.tsv"), "w", encoding="utf-8", newline="\n") as tsv:
        tsv.write("path\t" + "\t".join(targets) + "\n")
        for p in marked:
            tsv.write(p + "\t" + "\t".join(gate[p].get(t, "") for t in targets) + "\n")
    return 1 if clobbers else 0


if __name__ == "__main__":
    if len(sys.argv) >= 2 and sys.argv[1] == "nulcheck":
        sys.exit(nulcheck(sys.argv[2], sys.argv[3]))
    if len(sys.argv) < 3:
        sys.exit(__doc__)
    sys.exit(measure(sys.argv[1], sys.argv[2:]))
