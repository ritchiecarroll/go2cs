import json, sys, collections
R = json.load(open(sys.argv[1])); md, tsv = sys.argv[2], sys.argv[3]
READ = {
 ("crypto/tls","TestBogoSuite"): "KEEP. host-conditional: this page is the capability-absent host state (no BoGo runner, one agreeing verdict); the pin serves the host-limit state (the roster's crypto/tls row). The predicate fires by construction here -- DESIGN §5 already named it the entry least likely to be stale.",
 ("crypto/cipher","TestGCMAsm"): "Strong orphan on windows: the pin's own premise is that the converted side SKIPS (no asm), and the converted side records a terminal PASS. linux/darwin build the same purego file set, so the same is expected there -- unmeasured.",
 ("math/big","TestNewIntAllocs"): "Not an orphan by the ruled predicate (fail/fail is not reported), but it absorbs NOTHING on this run: Go ALSO fails at 1.24.13 on this axis, which contradicts the pin's premise (Go stack-allocates NewInt's argument). Re-read before re-sign.",
 ("os","TestUTF16Alloc"): "LIVE on windows (absorbed). The test is windows-only, so linux/darwin carry no such test: scope the pin to windows rather than drop it.",
 ("os/signal","TestTerminalSignal"): "ORPHAN under the corpus build on every axis: signal_cgo_test.go is `(unix...) && cgo`, and the corpus converts at CGO_ENABLED=0. Live only if a cgo axis is ever added.",
 ("runtime/debug","TestPanicOnFault"): "Windows-only orphan (ruled 2026-09-22): panic_test.go is unix-only; the row's `linux: 4 + 5` may still use it. Scope it, never drop it.",
}
for (p, n) in [("debug/gosym","TestSymVersion"),("internal/cpu","TestDisableAllCapabilities"),("internal/cpu","TestDisableSSE3"),("os/exec","TestExtraFiles")]:
    READ[(p, n)] = "skip/skip on windows: absorbs nothing here, and is not an orphan by the ruled predicate. Its reason names the linux shape (Go runs, the converted side skips)."
for r in R:
    if r["verdict"] == "ORPHAN-windows+darwin" and (r["pkg"], r["name"]) not in READ:
        READ[(r["pkg"], r["name"])] = "linux-only test: scope the pin to linux."
    if r["verdict"] == "ORPHAN-windows" and r["exists"] == ".ld" and (r["pkg"], r["name"]) not in READ:
        READ[(r["pkg"], r["name"])] = "unix-only test: scope the pin to linux + darwin."
    if r["pkg"] == "encoding/binary" and r["win"] == "pass/pass":
        READ[(r["pkg"], r["name"])] = "Terminal pass on both sides: a true orphan on windows (the batch-8b cell correction's 8 -> 6)."
    if r["win"] == "no-page" and r["verdict"] == "UNMEASURED":
        READ.setdefault((r["pkg"], r["name"]), "diverged in the i9 s1 1.24.13 run (supporting only)" if r.get("evidence") else "")
def cell(s): return s.replace("|", "\\|")
tot = collections.Counter(r["verdict"] for r in R)
cls = collections.defaultdict(collections.Counter)
for r in R: cls[r["cls"]][r["verdict"]] += 1
V = ["LIVE","ORPHAN-ALL","ORPHAN-windows","ORPHAN-windows+darwin","ORPHAN-linux+darwin","RENAMED","SIGNATURE-DRIFT","UNMEASURED"]
live_abs = sum(1 for r in R if r["verdict"]=="LIVE" and r["win"].endswith("*"))
L = []
L += ["# CENSUS — orphan disclosure pins at the batch-7 stamp (go1.24.13)", "",
 "**Read-only record** (C1, 2026-09-22), cut on `3469154a95`, the batch-7 STAMP of `claude/version-go1.24.13`. It rules nothing: the H10 step-3 re-sign seat is ruled from it. Point-in-time — amend with dated blocks, never rewrite.", "",
 "## What was read", "",
 f"- **Every committed manifest** at the stamp: {len({r['pkg'] for r in R})} `go2cs_test_disclosures.json`, **{len(R)} pins**, parsed as JSON.",
 "- **(a) Existence per GOOS.** The pin's TOP-LEVEL `Test` function in the package's `TestGoFiles`+`XTestGoFiles` from `go list` at the **1.24.13 GOROOT**, for `windows`, `linux` and `darwin` (`GOARCH=amd64`, `CGO_ENABLED=0`, `-tags purego,math_big_pure_go` — the corpus build of record). Build guards are therefore READ, not assumed. A subtest segment is not statically checkable; the page is its evidence.",
 "- **(b) Absorption in the row's 1.24.13 run.** The package's OWN proof page at the stamp, `## Verdicts` section only, the C# cell normalized to its leading token (DESIGN-orphan-disclosure-check §5, both corrections). **The ruled predicate (2026-09-06):** a pin is an orphan when the converted side records a **terminal `pass`** for its name. `fail/fail`, `skip/skip` and an absent name are NOT orphans by it — \"did not fail\" is not the test.",
 "- **(c) Signature.** Found verbatim (or, with rendered digits/verbs removed, every literal chunk) in the package's Go sources at **1.24.13**, and separately at **1.23.12**. `RUN` = absorbed on the page, so the 1.24.13 run itself matched it. `SRC` = still in Go's 1.24.13 source. `GONE` = in 1.23.12 source only (drift). `UNSEEN` = in neither, i.e. managed-side text no Go source contains.", "",
 "**Limits, load-bearing.** The pages are the **windows/amd64** record: a linux or darwin ORPHAN here comes only from the build guard (the test does not exist there), never from a run, and LIVE means live on windows. Three unbanked packages have **no page** (`reflect`, `runtime`, `runtime/pprof`): their pins are UNMEASURED, not cleared. The i9 s1 evidence lists which of them DIVERGED in a 1.24.13 run and that is noted, but absence from a diverged list is the forbidden inversion and is never read as a pass.", "",
 "**One name is shown by manifest index, not spelled.** A `net/netip` `TestParsePrefixAllocs` subtest is named by an IP-prefix literal from Go's own test table; the pre-push identifier gate refuses any IPv4-shaped token in an entry, and widening its admit set is an instrument change outside this record. The exact name is `disclosures[<n>]` of the committed manifest.", "",
 "## Verdicts", "",
 "| Verdict | Meaning |", "|:--|:--|",
 "| `LIVE` | exists on every applicable GOOS and is not a terminal pass on windows |",
 "| `ORPHAN-<GOOS>` | on those GOOS the test does not exist under the corpus build, or (windows only) the converted side records a terminal pass |",
 "| `ORPHAN-ALL` | orphan on every applicable GOOS |",
 "| `RENAMED` | the top-level test exists on no GOOS here but under that name in another package at 1.24.13 |",
 "| `SIGNATURE-DRIFT` | the signature is in 1.23.12 source and not 1.24.13 source, and the pin absorbed nothing |",
 "| `UNMEASURED` | the package has no page at the stamp |", "",
 "## Totals", "",
 "| Verdict | Pins |", "|:--|--:|"] + [f"| `{v}` | {tot.get(v,0)} |" for v in V] + [f"| **total** | **{len(R)}** |", "",
 f"`LIVE` {tot['LIVE']} = **{live_abs} absorbed** on the windows page + **{tot['LIVE']-live_abs} that absorb nothing on windows** (four `skip/skip`, one `fail/fail`; see the judgment list).", "",
 "Windows run state (sums to the total, a second derivation): " + ", ".join(f"`{k}` {v}" for k, v in sorted(collections.Counter(r['win'].replace('*',' (disclosed)') for r in R).items())) + ".", "",
 "Signature state: " + ", ".join(f"`{k}` {v}" for k, v in sorted(collections.Counter(r['sig'] for r in R).items())) + ". **No `GONE`: no pin's signature left Go's source between 1.23.12 and 1.24.13.** The 21 `UNSEEN` are all managed-side texts (posix_spawn seam refusals, `no execution tracer`, `no program counter exists`), unverifiable from Go source by construction; 69 absorbed pins likewise match managed-side text, confirmed by the run. The one fuzzy match with no run behind it, `reflect` `TestMapIterSet` (`wanted 0 alloc, got `), is Go's `\"wanted %d alloc, got %d\"` at want = 0, unchanged at both versions.", "",
 "### By class", "", "| Class | " + " | ".join(V) + " | total |", "|:--|" + "--:|" * (len(V) + 1)]
for c in sorted(cls, key=lambda k: -sum(cls[k].values())):
    L.append(f"| `{c}` | " + " | ".join(str(cls[c].get(v, 0) or "") for v in V) + f" | {sum(cls[c].values())} |")
L += ["", "## Controls", "",
 "- **Known answers reproduced:** `encoding/binary` `TestSizeAllocs/{binary.Struct,complex128,complex64}` → `ORPHAN-windows` on a terminal pass; `runtime/debug` `TestPanicOnFault` → `ORPHAN-windows` from its unix-only build guard (both measured independently earlier the same day).",
 "- **Page parser:** on every page a manifest package has, the `## Verdicts` rows parsed equal the headline's matched + disclosed — ALL EQUAL.",
 "- **go list** found test files for every manifest package on at least one GOOS (no silent empty package).", "",
 "## Needing judgment, not a label", ""]
for r in R:
    k = (r["pkg"], r["name"])
    if k in READ and READ[k] and not READ[k].startswith("diverged") and not READ[k].startswith("linux-only") and not READ[k].startswith("unix-only") and not READ[k].startswith("Terminal pass"):
        L.append(f"- **`{r['pkg']}` `{r['shown']}`** (`{r['cls']}`, {r['win'].replace('*',' disclosed')}, verdict `{r['verdict']}`): {READ[k]}")
L += ["", "## Per row", "", "Columns: exists at 1.24.13 on **w**indows / **l**inux / **d**arwin (`.` = not built there); windows run = Go / C# on the page (`*` = absorbed as disclosed); signature per (c).", ""]
for pkg in sorted({r["pkg"] for r in R}):
    rs = [r for r in R if r["pkg"] == pkg]
    c = collections.Counter(r["verdict"] for r in rs)
    L += [f"### `{pkg}` — {len(rs)} pins" + (" · banked row" if rs[0]["banked"] else " · NOT a banked row") + " · " + ", ".join(f"{v} {n}" for v, n in sorted(c.items())), "",
          "| Pin | Class | w l d | Windows run | Signature | Verdict | Reading |", "|:--|:--|:--:|:--|:--|:--|:--|"]
    for r in rs:
        L.append(f"| `{cell(r['shown'])}` | `{r['cls']}` | `{r['exists']}` | {r['win']} | {r['sig']} | **{r['verdict']}** | {cell(READ.get((r['pkg'], r['name']), ''))} |")
    L.append("")
open(md, "w", encoding="utf-8", newline="\n").write("\n".join(L) + "\n")
with open(tsv, "w", encoding="utf-8", newline="\n") as f:
    cols = ["pkg","shown","cls","platforms","exists","existed123","win","sig","sig124","sig123","moved","evidence","verdict"]
    f.write("\t".join(cols) + "\n")
    for r in R: f.write("\t".join(str(r.get(k, "")) for k in cols) + "\n")
print("rendered", len(L), "lines")
