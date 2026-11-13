# layout.py: stage the typescript-go-cs repository from a go2cs -recurse conversion, then fill its pages.
#
#   python -I layout.py stage --tree <converted root> --tsc <tsc module dir> --ts-root <TypeScript checkout> \
#                             --proof <this dir> --dest <staging dir>
#   python -I layout.py fill  --dest <staging dir> --proof <this dir> --results <compare.py out dir> \
#                             --converter <go2cs commit> --gostdlib <go.* version> --sdk <.NET SDK> \
#                             --platform <goos/goarch measured> --goversion <go version> [--handpatches <file>]
#                             [--owner-readme <the repository's README.md>]
#   python -I layout.py census --dest <staging dir> [--deny <local file of name regexes>]
#
# stage copies ONLY the parse closure of cmd/parsedemo (go list -deps, non-standard packages), each third-party
# module's license files, TypeScript's LICENSE and NOTICE.txt verbatim, the Go sources of the demo and the corpus
# extractor, the samples and the harness, and writes the solution. It rewrites one thing in the converted C#: the
# absolute source path a local main module's GoPositionMap records becomes the module-path form `go build -trimpath`
# gives (MODIFICATIONS.md states it). fill writes PROOF.md from the measured results only, and the README values as a patch
# on the owner's README (see readme_patch). census fails
# on any local path, user name or internal name in the staged tree.
import argparse, datetime, filecmp, json, os, re, shutil, subprocess, sys

sys.stdout.reconfigure(encoding="utf-8")
MOD = "github.com/microsoft/TypeScript/tsc"
TS_COMMIT = "ba29a562991ceb942481e2a00810b4e5e4062e52"
DEMOS = ("parsedemo", "checkdemo", "emitdemo", "mfdemo")  # the published closure: the union of the four demos
LICENSE_NAMES = re.compile(r"^(LICEN[CS]E|COPYING|NOTICE|PATENTS|AUTHORS|CONTRIBUTORS)(\..*)?$", re.I)
SKIP_DIRS = {"bin", "obj", "Generated", ".artifacts"}


def golist(tsc):
    fmt = "{{if not .Standard}}{{.ImportPath}}\t{{.Module.Path}}\t{{.Module.Version}}\t{{.Module.Dir}}{{end}}"
    env = dict(os.environ, GOWORK="off")
    out = subprocess.run(["go", "list", "-deps", "-f", fmt] + ["./cmd/" + d for d in DEMOS], cwd=tsc, env=env,
                         capture_output=True, text=True, check=True).stdout
    return [l.split("\t") for l in out.splitlines() if l.strip()]


def copy_files(src, dst, with_subdirs=()):
    os.makedirs(dst, exist_ok=True)
    n = 0
    for e in os.scandir(src):
        if e.is_file():
            shutil.copy2(e.path, os.path.join(dst, e.name)); n += 1
        elif e.is_dir() and e.name in with_subdirs:
            shutil.copytree(e.path, os.path.join(dst, e.name), dirs_exist_ok=True); n += 1
    return n


def stage(A):
    if os.path.exists(A.dest):
        sys.exit(f"refusing: {A.dest} exists")
    pkgs = golist(A.tsc)
    report = {"packages": [], "modules": {}, "positionmap_rewrites": 0}
    for imp, mod, ver, moddir in pkgs:
        rel = ("src/" if mod == MOD else "pkg/") + imp
        src = os.path.join(A.tree, rel)
        if not os.path.isdir(src):
            sys.exit(f"missing in the converted tree: {rel}")
        n = copy_files(src, os.path.join(A.dest, rel), with_subdirs=("Properties",))
        report["packages"].append({"import": imp, "path": rel, "files": n})
        if mod != MOD and mod not in report["modules"]:
            mroot = os.path.join(A.tree, "pkg", mod)
            lic = [e.name for e in os.scandir(mroot) if e.is_file() and LICENSE_NAMES.match(e.name)]
            if not lic:
                sys.exit(f"no license file for module {mod} in the converted tree")
            for name in lic:
                os.makedirs(os.path.join(A.dest, "pkg", mod), exist_ok=True)
                shutil.copy2(os.path.join(mroot, name), os.path.join(A.dest, "pkg", mod, name))
                if not filecmp.cmp(os.path.join(mroot, name), os.path.join(moddir, name), shallow=False):
                    sys.exit(f"{mod}/{name} differs from the module's own copy")
            if os.path.exists(os.path.join(moddir, "FORK.md")):  # a go2cs fork (xxh3, by go.mod replace): its FORK.md
                shutil.copy2(os.path.join(moddir, "FORK.md"), os.path.join(A.dest, "pkg", mod, "FORK.md"))  # travels too
                lic = lic + ["FORK.md"]
            report["modules"][mod] = {"version": ver, "licenses": lic}
    # root files
    for f in ("Directory.Build.props", "Directory.Build.targets"):
        shutil.copy2(os.path.join(A.tree, f), os.path.join(A.dest, f))
    shutil.copy2(os.path.join(A.ts_root, "LICENSE.txt"), os.path.join(A.dest, "LICENSE"))
    shutil.copy2(os.path.join(A.ts_root, "NOTICE.txt"), os.path.join(A.dest, "NOTICE.txt"))
    if not filecmp.cmp(os.path.join(A.ts_root, "LICENSE.txt"), os.path.join(A.tsc, "LICENSE"), shallow=False):
        sys.exit("TypeScript's LICENSE.txt and tsc/LICENSE differ")
    for name in DEMOS + ("parsecorpus-extract",):
        os.makedirs(os.path.join(A.dest, "go", "cmd", name), exist_ok=True)
        shutil.copy2(os.path.join(A.tsc, "cmd", name, "main.go"), os.path.join(A.dest, "go", "cmd", name, "main.go"))
    shutil.copytree(os.path.join(A.proof, "samples"), os.path.join(A.dest, "samples"))
    os.makedirs(os.path.join(A.dest, "proof"), exist_ok=True)
    for f in ("compare.py", "typecheck_slice.py", "demo_compare.py"):
        shutil.copy2(os.path.join(A.proof, f), os.path.join(A.dest, "proof", f))
    for f in (".gitignore", ".gitattributes", "MODIFICATIONS.md"):
        shutil.copy2(os.path.join(A.proof, "templates", f), os.path.join(A.dest, f))
    # GoPositionMap: <absolute module dir>/<file>  ->  github.com/microsoft/TypeScript/tsc/<file>
    # (any absolute checkout of the module: the tree may have been converted from another copy than --tsc)
    pat = re.compile(r'(GoPositionMap\(")(?:[A-Za-z]:)?/[^"]*?/tsc/(?=(?:internal|cmd)/)')
    for root, dirs, files in os.walk(os.path.join(A.dest, "src")):
        for f in files:
            if f.endswith(".cs"):
                p = os.path.join(root, f)
                s = open(p, encoding="utf-8-sig").read()
                t, k = pat.subn(r"\g<1>" + MOD + "/", s)
                if k:
                    with open(p, "w", encoding="utf-8", newline="") as o:
                        o.write(t)
                    report["positionmap_rewrites"] += k
    if report["positionmap_rewrites"]:  # MODIFICATIONS.md says no path was rewritten; keep it true
        sys.exit(f"{report['positionmap_rewrites']} GoPositionMap paths needed a rewrite; MODIFICATIONS.md must say so")
    # solution
    projs = sorted(os.path.relpath(os.path.join(r, f), A.dest).replace("\\", "/")
                   for r, _, fs in os.walk(A.dest) for f in fs if f.endswith(".csproj"))
    with open(os.path.join(A.dest, "typescript-go-cs.slnx"), "w", encoding="utf-8", newline="\r\n") as o:
        o.write("<Solution>\n" + "".join(f'  <Project Path="{p}" />\n' for p in projs) + "</Solution>\n")
    report["projects"] = projs
    json.dump(report, open(os.path.join(A.proof, "stage-report.json"), "w", encoding="utf-8"), indent=1)
    print(f"staged {len(pkgs)} packages ({len(report['modules'])} third-party modules), {len(projs)} projects, "
          f"{report['positionmap_rewrites']} GoPositionMap paths rewritten")


def slice_rows(sl):
    """Class table and mismatch list of one typecheck_slice.py pass (slice.json)."""
    rows = "\n".join(f"| {k} | {v:,} |" for k, v in sorted(sl["classes"].items(), key=lambda kv: -kv[1])) or "| (none) | 0 |"
    mism = sl["mismatches"]
    if not mism:
        return rows, "No file differs."
    lst = "\n".join(f"| `{m['index']}` | {m['class']} | `{m['source']}` | `{(m.get('go') or '')[:80]}` / `{(m.get('cs') or '')[:80]}` |"
                    for m in mism[:300])
    more = f"\n\n{len(mism) - 300:,} more are in the pass's `slice.json`." if len(mism) > 300 else ""
    return rows, "| File | Class | Source | First difference, native / converted |\n|:--|:--|:--|:--|\n" + lst + more


def verdict(ok):
    return "identical" if ok else "NOT identical"


def fill(A):
    R = A.results
    J = lambda *p: json.load(open(os.path.join(R, *p), encoding="utf-8"))
    T = lambda *p: open(os.path.join(R, *p), encoding="utf-8", errors="replace").read().rstrip("\n")
    P = J("parse", "results.json")
    d, c = P["demo"], P["corpus"]
    ck, em, mf = J("checkdemo.json"), J("emitdemo.json"), J("mfdemo.json")
    tc, ef = J("check-full", "slice.json"), J("emit-full", "slice.json")
    for name, sl in (("type-check", tc), ("emit", ef)):
        if sl["units"] != sl["pool"]:
            sys.exit(f"the {name} pass covers {sl['units']} of {sl['pool']} files: a partial pass is not a reading")
    hp = open(A.handpatches, encoding="utf-8").read().strip() if A.handpatches else ""
    rep = json.load(open(os.path.join(A.proof, "stage-report.json"), encoding="utf-8"))
    ts_pkgs = sum(1 for x in rep["packages"] if x["path"].startswith("src/") and "/tsc/internal/" in x["path"])
    pkg_pkgs = sum(1 for x in rep["packages"] if x["path"].startswith("pkg/"))

    def frac(n, u):
        return f"**identical on all {u:,}**" if n == u else f"**identical on {n:,} of {u:,}**"

    all_ok = (d["match"] and c["matched"] == c["units"] and ck["match"] and em["match"] and mf["match"]
              and tc["matched"] == tc["units"] and ef["matched"] == ef["units"])
    headline = ("On .NET, the converted code produces the same output as the native Go build, byte for byte, on every "
                "comparison below." if all_ok else
                "On .NET, the converted code reproduces the native Go build's output byte for byte as counted below.")
    trows, tlist = slice_rows(tc)
    erows, elist = slice_rows(ef)
    crow = "\n".join("| " + " | ".join(l.split("\t")) + " |" for l in open(os.path.join(R, "controls.tsv"), encoding="utf-8").read().strip().split("\n"))
    color = lambda n, u: "brightgreen" if n == u else "yellow"
    v = {
        "HEADLINE": headline, "TS_PKGS": str(ts_pkgs), "PKG_PKGS": str(pkg_pkgs),
        "PARSE_DEMO": verdict(d["match"]), "CHECK_DEMO": verdict(ck["match"]), "EMIT_DEMO": verdict(em["match"]),
        "MF_RESULT": verdict(mf["match"]),
        "CORPUS_UNITS": f"{c['units']:,}", "CORPUS_RESULT": frac(c["matched"], c["units"]),
        "TC_UNITS": f"{tc['units']:,}", "TC_RESULT": frac(tc["matched"], tc["units"]),
        "EM_UNITS": f"{ef['units']:,}", "EM_RESULT": frac(ef["matched"], ef["units"]),
        "CORPUS_N": f"{c['matched']:,}".replace(",", "%2C"), "CORPUS_UNITS_B": f"{c['units']:,}".replace(",", "%2C"),
        "TC_N": f"{tc['matched']:,}", "TC_UNITS_B": f"{tc['units']:,}".replace(",", "%2C"),
        "EM_N": f"{ef['matched']:,}", "EM_UNITS_B": f"{ef['units']:,}".replace(",", "%2C"),
        "C_COLOR": color(c["matched"], c["units"]), "TC_COLOR": color(tc["matched"], tc["units"]),
        "EM_COLOR": color(ef["matched"], ef["units"]),
        "DEMO_VERDICT": "byte-identical" if d["match"] else "NOT byte-identical",
        "DEMO_LINES": f"{d['go']['lines']:,}", "DEMO_SHA_GO": d["go"]["sha256"], "DEMO_SHA_CS": d["cs"]["sha256"],
        "DEMO_GO": T("parse", "demo.go.txt"), "DEMO_CS": T("parse", "demo.cs.txt"),
        "N": f"{c['matched']:,}", "UNITS": f"{c['units']:,}", "NODES": f"{c['nodes']:,}", "DIAGS": f"{c['diagnostics']:,}",
        "SECS_GO": f"{c['native']['seconds']:,}", "SECS_CS": f"{c['converted']['seconds']:,}",
        "CLASS_ROWS": "\n".join(f"| {k} | {v:,} |" for k, v in c["classes"].items()) or "| (none) | 0 |",
        "MISMATCH_ROWS": "\n".join(f"| `{m['index']}` | {m['class']} | `{m['source']}` | |" for m in c["mismatches"][:300]) or "| | (none) | | |",
        "CHECK_VERDICT": "byte-identical" if ck["match"] else "NOT byte-identical",
        "CHECK_SHA_GO": ck["go"]["sha256"], "CHECK_SHA_CS": ck["cs"]["sha256"], "CHECK_GO": T("checkdemo.go.txt"),
        "TC_N_P": "", "TC_SECS_GO": f"{tc['seconds']['go']:,}", "TC_SECS_CS": f"{tc['seconds']['cs']:,}",
        "TC_CLASS_ROWS": trows, "TC_MISMATCH_ROWS": tlist,
        "EM_SECS_GO": f"{ef['seconds']['go']:,}", "EM_SECS_CS": f"{ef['seconds']['cs']:,}",
        "EM_CLASS_ROWS": erows, "EM_MISMATCH_ROWS": elist,
        "EMIT_VERDICT": "byte-identical" if em["match"] else "NOT byte-identical",
        "EMIT_SHA_GO": em["go"]["sha256"], "EMIT_SHA_CS": em["cs"]["sha256"],
        "MF_VERDICT": "byte-identical" if mf["match"] else "NOT byte-identical",
        "MF_SHA_GO": mf["go"]["sha256"], "MF_SHA_CS": mf["cs"]["sha256"], "MF_GO": T("mfdemo.go.txt"),
        "CONTROL_ROWS": crow,
        "CONVERTER": A.converter, "GOSTDLIB": A.gostdlib, "SDK": A.sdk, "PLATFORM": A.platform, "GOVERSION": A.goversion,
        "DATE": datetime.date.today().isoformat(), "TS_COMMIT": TS_COMMIT, "TS_SHORT": TS_COMMIT[:10],
        "HANDPATCHES": hp if hp else "None. Every C# file under `src/` and `pkg/` is the converter's output, unedited.",
    }
    v["TC_N"] = f"{tc['matched']:,}"
    filled = {}
    for name in ("README.md", "PROOF.md"):
        t = open(os.path.join(A.proof, "templates", name + ".tmpl"), encoding="utf-8").read()
        for k, val in v.items():
            t = t.replace("{{" + k + "}}", val)
        left = re.findall(r"\{\{[A-Z_]+\}\}", t)
        if left:
            sys.exit(f"{name}: unfilled {left}")
        filled[name] = t
    open(os.path.join(A.dest, "PROOF.md"), "w", encoding="utf-8", newline="\n").write(filled["PROOF.md"])
    readme_patch(A, v, filled["README.md"])
    out = os.path.join(A.dest, "proof")
    shutil.rmtree(os.path.join(out, "results"), ignore_errors=True)
    shutil.copytree(R, os.path.join(out, "results"), ignore=shutil.ignore_patterns("units.jsonl", "PAUSE", "*.log"))
    print("filled PROOF.md and the README values: parse", c["matched"], "/", c["units"], "| type-check", tc["matched"], "/",
          tc["units"], "| emit", ef["matched"], "/", ef["units"], "| demos", d["match"], ck["match"], em["match"], mf["match"])


# 2026-11-12 (COORD 16:43Z): the repository's README.md is the OWNER'S text and is never overwritten. fill writes no
# README.md into the staged tree. It writes, OUTSIDE the tree in <dest>.readme/: values.json (every fill value),
# README.generated.md (this kit's template, filled, for reference) and, with --owner-readme <his README.md>,
# README.patch: a unified diff that changes his file only where it asks for a value, i.e. every {{KEY}} placeholder
# and the block between the RESULTS markers, which takes the generated block. The i7 applies it on top after COORD
# has seen the diff. A README with neither placeholders nor markers gives an empty patch, and fill says so.
RESULTS_BEGIN, RESULTS_END = "<!-- typescript-go-cs:results -->", "<!-- /typescript-go-cs:results -->"


def readme_patch(A, v, generated):
    import difflib
    out = A.dest.rstrip("/\\") + ".readme"
    os.makedirs(out, exist_ok=True)
    if os.path.exists(os.path.join(out, "README.patch")):
        os.remove(os.path.join(out, "README.patch"))      # never leave a patch from an earlier fill beside new values
    json.dump(v, open(os.path.join(out, "values.json"), "w", encoding="utf-8", newline="\n"), indent=1, ensure_ascii=False)
    open(os.path.join(out, "README.generated.md"), "w", encoding="utf-8", newline="\n").write(generated)
    if os.path.exists(os.path.join(A.dest, "README.md")):
        sys.exit("the staged tree holds a README.md: the repository's README is the owner's, remove it from the bundle")
    if not A.owner_readme:
        print(f"README: values and the generated page in {out}; no --owner-readme, so no patch")
        return
    owner = open(A.owner_readme, encoding="utf-8").read()
    new = owner
    for k, val in v.items():
        new = new.replace("{{" + k + "}}", val)
    block = lambda t: t[t.index(RESULTS_BEGIN):t.index(RESULTS_END) + len(RESULTS_END)] if RESULTS_BEGIN in t and RESULTS_END in t else None
    if block(new) is not None and block(generated) is not None:
        new = new.replace(block(new), block(generated), 1)
    left = re.findall(r"\{\{[A-Z_]+\}\}", new)
    if left:
        sys.exit(f"owner README asks for values fill does not have: {sorted(set(left))}")
    diff = "".join(difflib.unified_diff(owner.splitlines(True), new.splitlines(True), "a/README.md", "b/README.md"))
    open(os.path.join(out, "README.patch"), "w", encoding="utf-8", newline="\n").write(diff)
    print(f"README.patch: {diff.count(chr(10))} lines" if diff else "README.patch: EMPTY (no placeholders or results markers in the owner's README)")


# Everywhere: no local path or user name. In the files this project writes (not the converted C#, not TypeScript's
# verbatim LICENSE/NOTICE, not third-party licenses): no internal working names either.
PATHS = [r"/home/", r"/root/", r"\b[A-Z]:[\\/]", r"[\\/]Users[\\/]"]
INTERNAL = [r"\bCOORD\b", r"\bi7\b", r"\bi9\b", r"\bmailbox\b", r"\bfleet\b",
            r"\blanes?\b", r"\bseats?\b", r"scratch", r"\bowner\b", r"EXPERIMENT", r"\bBOARD\b"]
# Names (people, machines, working names) are NOT written here: census --deny <file> reads one regex per line from a
# local file the operator keeps, and applies each everywhere, like PATHS.
AUTHORED = re.compile(r"^(README\.md|PROOF\.md|MODIFICATIONS\.md|\.git\w+|[^/]*\.slnx|proof/[^/]*\.py|go/.*|samples/.*)$")


def census(A):
    hits = []
    deny = [l.strip() for l in open(A.deny, encoding="utf-8")] if A.deny else []
    deny = [l for l in deny if l and not l.startswith("#")]
    if not deny:
        print("census: WARNING no --deny file, so no name is checked")
    for root, dirs, files in os.walk(A.dest):
        dirs[:] = [x for x in dirs if x not in SKIP_DIRS and x != ".git"]
        for f in files:
            p = os.path.join(root, f)
            try:
                s = open(p, encoding="utf-8", errors="strict").read()
            except (UnicodeDecodeError, OSError):
                continue
            rel = os.path.relpath(p, A.dest).replace("\\", "/")
            # proof data quotes TypeScript's test-case file names, some of which are Windows-style (C:/foo/...):
            # there, only the drive-letter rule is relaxed.
            paths = [x for x in PATHS if not (rel.startswith("proof/") and not rel.endswith(".py") and x.startswith(r"\b[A-Z]"))]
            for pat in paths + deny + (INTERNAL if AUTHORED.match(rel) else []):
                for m in re.finditer(pat, s):
                    line = s.count("\n", 0, m.start()) + 1
                    hits.append(f"{rel}:{line}: {pat}: {s[max(0, m.start() - 40):m.end() + 40]!r}")
    print(f"census: {len(hits)} hit(s)")
    for h in hits[:200]:
        print(" ", h)
    sys.exit(1 if hits else 0)


ap = argparse.ArgumentParser()
ap.add_argument("cmd", choices=("stage", "fill", "census"))
for a in ("tree", "tsc", "ts-root", "proof", "dest", "results", "converter", "gostdlib", "sdk", "platform", "goversion",
          "handpatches", "owner-readme", "deny"):
    ap.add_argument("--" + a)
A = ap.parse_args()
{"stage": stage, "fill": fill, "census": census}[A.cmd](A)
