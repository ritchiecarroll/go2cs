#!/usr/bin/env python3
# ORPHAN census of every committed go2cs_test_disclosures.json at REF (read-only).
# Instrument: manifests parsed as JSON; each package's OWN proof page at REF, ## Verdicts section only,
# C# cell normalized to its leading token (DESIGN-orphan-disclosure-check.md section 5, corrections 1-2);
# per-GOOS existence of the pin's top-level Test function from `go list` at the 1.24.13 GOROOT with the
# corpus build (CGO_ENABLED=0, -tags purego,math_big_pure_go), GOARCH=amd64; signatures searched in the
# package's sources at 1.24.13 and 1.23.12.
import json, os, re, subprocess, sys, collections, pathlib
REF = sys.argv[1]; OUT = pathlib.Path(sys.argv[2])
GOOSES = ["windows", "linux", "darwin"]
ROOT = {v: subprocess.run(["go", "env", "GOROOT"], env={**os.environ, "GOTOOLCHAIN": f"go{v}"}, capture_output=True, text=True, cwd="/tmp", check=True).stdout.strip() for v in ("1.24.13", "1.23.12")}
git = lambda *a: subprocess.run(["git", *a], capture_output=True, text=True)
show = lambda p: git("show", f"{REF}:{p}")

manifests = sorted(l for l in git("ls-tree", "-r", "--name-only", REF, "src/core").stdout.split("\n") if l.endswith("/go2cs_test_disclosures.json"))
roster = show("docs/ValidatedTestPackages.md").stdout
banked = set(re.findall(r"^\| \[`([^`]+)`\]\(", roster, re.M))

def golist(pkg, goos, ver):
    env = {**os.environ, "GOTOOLCHAIN": f"go{ver}", "GOOS": goos, "GOARCH": "amd64", "CGO_ENABLED": "0", "GOFLAGS": ""}
    r = subprocess.run(["go", "list", "-e", "-json", "-tags", "purego,math_big_pure_go", pkg], env=env, capture_output=True, text=True, cwd="/tmp")
    if r.returncode or not r.stdout.strip(): return None
    return json.loads(r.stdout)

def testfuncs(info):
    if not info: return set()
    out = set()
    for f in (info.get("TestGoFiles") or []) + (info.get("XTestGoFiles") or []):
        out |= set(re.findall(r"^func (Test\w*)\(", open(os.path.join(info["Dir"], f), encoding="utf-8").read(), re.M))
    return out

def pkgsources(pkg, ver):
    d = os.path.join(ROOT[ver], "src", pkg)
    if not os.path.isdir(d): return ""
    return "\n".join(open(os.path.join(d, f), encoding="utf-8", errors="replace").read() for f in sorted(os.listdir(d)) if f.endswith(".go"))

def sig_in(sig, text):
    if not sig: return "none"
    if sig in text: return "exact"
    chunks = [c for c in re.split(r"\d+|%[a-zA-Z]", sig) if len(c.strip()) >= 6]
    return "chunks" if chunks and all(c in text for c in chunks) else "no"

def page_verdicts(pkg):
    r = show(f"docs/validation/current/{pkg.replace('/', '.')}.md")
    if r.returncode: return None
    sec = re.search(r"^## Verdicts\s*$(.*?)(?=^## |\Z)", r.stdout, re.M | re.S)
    rows = {}
    for m in re.finditer(r"^\| `(.+?)` \| ([^|]+?) \| ([^|]+?) \|\s*$", sec.group(1) if sec else "", re.M):
        name = m.group(1).replace("\\|", "|")
        rows[name] = (m.group(2).strip().split()[0], m.group(3).strip().split()[0], "disclosed" in m.group(3))
    return rows

allstd = None
def moved_to(top):
    global allstd
    if allstd is None:
        allstd = subprocess.run(["grep", "-rlE", "--include=*_test.go", r"^func Test\w*\(", os.path.join(ROOT["1.24.13"], "src")], capture_output=True, text=True).stdout.split()
    hits = []
    for f in allstd:
        if re.search(rf"^func {re.escape(top)}\(", open(f, encoding="utf-8", errors="replace").read(), re.M):
            hits.append(os.path.relpath(os.path.dirname(f), os.path.join(ROOT["1.24.13"], "src")))
    return sorted(set(hits))

records = []
for mf in manifests:
    pkg = mf[len("src/core/"):-len("/go2cs_test_disclosures.json")]
    entries = json.loads(show(mf).stdout)["disclosures"]
    exist = {g: testfuncs(golist(pkg, g, "1.24.13")) for g in GOOSES}
    exist123 = testfuncs(golist(pkg, "windows", "1.23.12")) | testfuncs(golist(pkg, "linux", "1.23.12"))
    listed = any(exist.values())
    page = page_verdicts(pkg)
    src124, src123 = pkgsources(pkg, "1.24.13"), pkgsources(pkg, "1.23.12")
    for e in entries:
        name, cls, sig = e["name"], e.get("class", ""), e.get("signature", "")
        top = name.split("/")[0]
        applicable = list(e.get("platforms") or GOOSES)
        ex = {g: top in exist[g] for g in GOOSES}
        # windows run state, per the ruled predicate (a terminal C# pass), from the page
        if page is None: win = "no-page"
        elif name in page:
            go, cs, disc = page[name]
            win = f"{go}/{cs}" + ("*" if disc else "")
        else: win = "not-on-page"
        s124, s123 = sig_in(sig, src124), sig_in(sig, src123)
        absorbed = win.endswith("*")
        sigstat = "RUN" if absorbed else ("SRC" if s124 != "no" else ("GONE" if s123 != "no" else "UNSEEN"))
        orphan_goos = [g for g in applicable if not ex[g]]
        if "windows" in applicable and ex["windows"] and page is not None and name in page and page[name][1] == "pass":
            orphan_goos.append("windows")
        orphan_goos = sorted(set(orphan_goos), key=GOOSES.index)
        moved = []
        if not any(ex.values()):
            moved = [m for m in moved_to(top) if m != pkg]
            verdict = "RENAMED" if moved else "ORPHAN-ALL"
        elif orphan_goos and set(orphan_goos) >= set(applicable): verdict = "ORPHAN-ALL"
        elif orphan_goos: verdict = "ORPHAN-" + "+".join(orphan_goos)
        elif page is None: verdict = "UNMEASURED"
        elif win == "not-on-page": verdict = "NOT-ON-PAGE"
        else: verdict = "LIVE"
        if sigstat == "GONE" and verdict in ("LIVE", "UNMEASURED", "NOT-ON-PAGE"): verdict = "SIGNATURE-DRIFT"
        records.append(dict(pkg=pkg, banked=pkg in banked, name=name, cls=cls, platforms=",".join(e.get("platforms") or []) or "-",
                            exists="".join(g[0] if ex[g] else "." for g in GOOSES), existed123=top in exist123, listed=listed,
                            win=win, sig=sigstat, sig124=s124, sig123=s123, moved=";".join(moved), verdict=verdict))

OUT.write_text(json.dumps(records, indent=1))
c = collections.Counter(r["verdict"] for r in records)
print(f"{len(manifests)} manifests · {len(records)} pins ·", dict(sorted(c.items())))
print("go list failed (no test files found on any GOOS):", sorted({r['pkg'] for r in records if not r['listed']}))
