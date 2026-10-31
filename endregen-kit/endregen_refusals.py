"""endregen_refusals.py -- group a pcensus classify output's refusals for attribution (python -I -X utf8).

  endregen_refusals.py <pcensus-output.txt> [<out-prefix>]

Reads the REFUSED records (`REFUSED <REASON> <file> <hunk>` then `  - <old>` / `  + <new>`) and prints, per reason and
file kind, the number of refused lines and the most frequent normalised line SHAPES (identifiers, numbers and string
literals folded), so each class can be matched against a rung's mechanism. With <out-prefix>, also writes
<out-prefix>-unclassified.tsv (every UNCLASSIFIED pair, verbatim) and <out-prefix>-unpaired-files.tsv (refused lines per file).
"""
import collections, re, sys

REC = re.compile(r"^REFUSED (\S+) (\S+) (@@.*)$")


def kind(path):
    if re.search(r"package_info[^/]*\.cs$", path):
        return "package_info"
    for suf, k in ((".csproj", "csproj"), (".cs.auto", "cs.auto"), ("README.md", "README"), (".cs", "cs")):
        if path.endswith(suf):
            return k
    return "other"


def shape(line):
    s = line.strip()
    s = re.sub(r'"(?:[^"\\]|\\.)*"', '"S"', s)
    s = re.sub(r"\b\d+\b", "N", s)
    s = re.sub(r"[A-Za-z_\u0080-￿][\w\u0080-￿]*", lambda m: m.group(0) if m.group(0) in KEEP else "x", s)
    return s[:110]


KEEP = set("""internal private public static readonly ref var return new default if else for foreach switch case
break continue goto nint nuint bool string object void get set this partial struct class interface
GoFieldAccess GoPositionMap GoValueClone GoType GoImplement assembly module using namespace""".split())


def main(path, prefix=None):
    recs = []
    cur = None
    for ln in open(path, encoding="utf-8", errors="replace"):
        ln = ln.rstrip("\n")
        m = REC.match(ln)
        if m:
            cur = {"reason": m.group(1), "file": m.group(2), "hunk": m.group(3), "old": None, "new": None}
            recs.append(cur)
        elif cur is not None and ln.startswith("  - "):
            cur["old"] = ln[4:]
        elif cur is not None and ln.startswith("  + "):
            cur["new"] = ln[4:]
    groups = collections.defaultdict(list)
    for r in recs:
        groups[(r["reason"], kind(r["file"]))].append(r)
    for (reason, k), rs in sorted(groups.items(), key=lambda kv: -len(kv[1])):
        files = len({r["file"] for r in rs})
        print(f"== {reason} {k}: {len(rs)} lines in {files} files")
        side = collections.Counter()
        for r in rs:
            o, n = (r["old"] or "").strip(), (r["new"] or "").strip()
            if o and not n:
                side["-"] += 1
            elif n and not o:
                side["+"] += 1
            else:
                side["±"] += 1
        print("   sides: " + ", ".join(f"{s} {c}" for s, c in side.most_common()))
        shapes = collections.Counter()
        for r in rs:
            o, n = (r["old"] or "").strip(), (r["new"] or "").strip()
            shapes[("- " + shape(o) if o and not n else "+ " + shape(n) if n and not o else shape(o) + "  =>  " + shape(n))] += 1
        for s, c in shapes.most_common(25):
            print(f"   {c:6d}  {s}")
    tag_report(recs)
    if prefix:
        with open(prefix + "-unclassified.tsv", "w", encoding="utf-8", newline="\n") as fh:
            fh.write("file\thunk\told\tnew\n")
            for r in recs:
                if r["reason"] == "UNCLASSIFIED":
                    fh.write(f"{r['file']}\t{r['hunk']}\t{r['old']}\t{r['new']}\n")
        per = collections.Counter((r["reason"], r["file"]) for r in recs)
        with open(prefix + "-unpaired-files.tsv", "w", encoding="utf-8", newline="\n") as fh:
            fh.write("reason\tfile\tlines\n")
            for (reason, f), c in sorted(per.items()):
                fh.write(f"{reason}\t{f}\t{c}\n")


# The rung shapes pcensus has no item for, named by the rung table (ladder-rungs.tsv): #53's hoist-mode accessor form
# (a hoisted field turned private, with a `ref readonly` accessor) and its Library .csproj template inserts.
# The initializer may continue on later lines (a raw `"""` literal, a byte table), so `= …` need not end the line;
# the private field then carries the `ᵇ` backing suffix.
D53_DECL = re.compile(r"^\s*(internal|private|public) static (ref readonly|readonly) \S+ \S+( => ref \S+;| = .*)\s*(//.*)?$")

# Each mechanism's tell on the side that carries it: OLD (the committed line) and NEW (the emission). A refused line
# is attributed to every mechanism whose tell it carries; a line that carries none is UNTAGGED: a finding candidate.
OLD_TELLS = [("P1", r"\(~"), ("P2", r"\.of\("), ("N1|D1", r"default!"), ("E1", r"\)\{\s*$|^\s*else if "),
             ("S1", r"exprᴛ\d"), ("L1", r"\}\.slice\(\)|new [^()]+\[\]\{"), ("C1", r"\(rune\)"), ("Z1", r"new \w+\{\}"),
             ("P1", r"\.Value\.\w+ ="), ("S1", r"^\s*(else )?if \(")]
NEW_TELLS = [("S1", r"case var ᴛ\d* when"), ("L1", r"slice<[^>]+>\(\["), ("N1", r"[!=]= nil\b"), ("E1", r"\) \{\s*$"),
             ("P2", r"\w\.Ꮡ\w")]


def tags_of(old_lines, new_lines):
    """The mechanisms a refused RUN carries: a tell that occurs more often on the side that carries it."""
    old, new = "\n".join(old_lines), "\n".join(new_lines)
    tags = set()
    for name, rx in OLD_TELLS:
        if len(re.findall(rx, old, re.M)) > len(re.findall(rx, new, re.M)):
            tags.add(name)
    for name, rx in NEW_TELLS:
        if len(re.findall(rx, new, re.M)) > len(re.findall(rx, old, re.M)):
            tags.add(name)
    o_cmts = {ln.partition("//")[2].strip() for ln in old_lines if "//" in ln}
    for ln in new_lines:
        code, _, cmt = ln.partition("//")
        if cmt.strip() and cmt.strip() in o_cmts:
            tags.add("J")
            break
    return tags


def tag_report(recs):
    print("== ATTRIBUTION of the refusals pcensus has no item or composition for")
    decl = [r for r in recs if r["reason"] == "UNPAIRED" and D53_DECL.match(r["old"] or r["new"] or "x")]
    csproj = [r for r in recs if r["reason"] == "UNPAIRED" and kind(r["file"]) == "csproj"]
    print(f"   #53 accessor-form declarations (UNPAIRED, rung table #53): {len(decl)} lines in {len({r['file'] for r in decl})} files")
    print(f"   #53 Library .csproj template inserts (UNPAIRED, rung table #53): {len(csproj)} lines in {len({r['file'] for r in csproj})} files")
    skip = {id(r) for r in decl} | {id(r) for r in csproj}
    runs = collections.OrderedDict()
    for r in recs:
        if id(r) in skip or not ((r["old"] or "").strip() or (r["new"] or "").strip()):
            continue
        runs.setdefault((r["reason"], r["file"], r["hunk"]), []).append(r)
    sets = collections.Counter()
    untagged = []
    for (reason, f, h), rs in runs.items():
        olds = [r["old"] for r in rs if (r["old"] or "").strip()]
        news = [r["new"] for r in rs if (r["new"] or "").strip()]
        t = tags_of(olds, news)
        sets[f"{reason} {'+'.join(sorted(t)) if t else 'UNTAGGED'}"] += len(rs)
        if not t:
            untagged.append((reason, f, h, olds, news))
    for k, c in sets.most_common():
        print(f"   {c:6d} lines  {k}")
    print(f"   UNTAGGED runs: {len(untagged)} ({sum(len(o) + len(n) for _, _, _, o, n in untagged)} lines)")
    for reason, f, h, olds, news in untagged[:30]:
        print(f"      {reason} {f.split('/src/core/')[-1]} {h[:40]}")
        for o in olds[:3]:
            print(f"         - {o.strip()[:110]}")
        for n in news[:3]:
            print(f"         + {n.strip()[:110]}")


if __name__ == "__main__":
    if len(sys.argv) < 2:
        sys.exit(__doc__)
    main(sys.argv[1], sys.argv[2] if len(sys.argv) > 2 else None)
