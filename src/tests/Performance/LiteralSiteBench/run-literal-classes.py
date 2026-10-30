"""Seat #52 site-class A/B: LiteralSiteBench, one binary per literal form, interleaved.

Usage: python -I -X utf8 run-literal-classes.py <repo> <d6-checkout> <rounds> <out.tsv> <kinds>
  <repo>         the checkout holding LSB-F1, LSB-F5 and LSB-F2 under src/tests/Performance (README.md, Build)
  <d6-checkout>  the checkout holding LSB-F3, whose golib carries d6-reconstructed.patch
  <kinds>        comma list of jit,aot

Forms (each its own binary): F1 hoisted, F5 its A/A copy, F2 plain + master's cache, F3 plain + D6 reconstructed;
F4 (C1 only) is the plain binary's c1i kernel (the hand-written interceptor); F1u8 (C4 only) is the hoisted binary's
c4u8 kernel (the literal inline as u8, -hoist-literals' real emission for a composite element).

Each round runs every arm of a class adjacent, in an order that reverses every other round; one discarded warm-up
round precedes the measured ones. Every arm of a class must print the same checksum and exit 0, or it reads MISMATCH.
Readout per (kind, class): each arm's median ns/eval [range] and bytes/eval; the per-round ratios F2/F1, F3/F1 (and
F4/F2 for C1) as median [range], read against the A/A band F5/F1 [range]. Verdict for a ratio: OUTSIDE the band when
its range is disjoint from the A/A range (hoisting FASTER if the ratio sits above the band, SLOWER if below), else
INSIDE NOISE.
"""
import os, re, statistics, subprocess, sys

repo, d6, rounds, out, kinds = sys.argv[1], sys.argv[2], int(sys.argv[3]), sys.argv[4], sys.argv[5].split(",")
perf = lambda tree, form: os.path.join(tree, "src", "tests", "Performance", "LSB-" + form)
PERF = {"F1": perf(repo, "F1"), "F5": perf(repo, "F5"), "F2": perf(repo, "F2"), "F3": perf(d6, "F3")}
BASE = [("F1", "F1", None), ("F5", "F5", None), ("F2", "F2", None), ("F3", "F3", None)]
CLASSES = [("c1", BASE + [("F4", "F2", "c1i")]), ("c2", BASE), ("c3", BASE), ("c4", BASE + [("F1u8", "F1", "c4u8")]),
           ("c5", BASE), ("c6", BASE), ("control", BASE)]

env = dict(os.environ)
for k in [k for k in env if k.startswith("DOTNET_Tiered") or k.startswith("DOTNET_TC_") or k.startswith("DOTNET_Jit")]:
    del env[k]

def exe(form, kind):
    return os.path.join(PERF[form], "bin", "Release", "net10.0" if kind == "jit" else "aot", "LiteralSiteBench.exe")

missing = [exe(f, k) for f in PERF for k in kinds if not os.path.isfile(exe(f, k))]
if missing:
    print("MISSING", *missing, sep="\n  ")
    sys.exit(2)

def run(kind, form, kernel):
    p = subprocess.run([exe(form, kind), kernel], capture_output=True, text=True, env=env, timeout=120)
    get = lambda key: re.search(rf"^{key}:\s*(\S+)", p.stdout, re.M)
    ck, ns, by = get("checksum"), get("ns_per_eval"), get("bytes_per_eval")
    return p.returncode, ck.group(1) if ck else None, float(ns.group(1)) if ns else None, float(by.group(1)) if by else None

def span(v):
    return f"{statistics.median(v):.3f} [{min(v):.3f}-{max(v):.3f}]"

with open(out, "w", encoding="utf-8") as f:
    f.write("kind\tclass\tround\tarm\tns_per_eval\tbytes_per_eval\trc\n")
    for kind in kinds:
        for cls, arms in CLASSES:
            ns = {a: [] for a, _, _ in arms}
            by = {a: [] for a, _, _ in arms}
            ref, bad = None, False
            for r in range(rounds + 1):
                for label, form, kernel in (arms if r % 2 == 0 else list(reversed(arms))):
                    rc, ck, t, b = run(kind, form, kernel or cls)
                    if ref is None:
                        ref = ck
                    if rc != 0 or ck is None or ck != ref or t is None:
                        bad = True
                        print(f"  BAD {kind} {cls} {label} round {r}: rc={rc} checksum={ck} ref={ref}", flush=True)
                        continue
                    if r > 0:
                        ns[label].append(t)
                        by[label].append(b)
                        f.write(f"{kind}\t{cls}\t{r}\t{label}\t{t}\t{b}\t{rc}\n")
                f.flush()
            if bad:
                print(f"{kind} {cls} | MISMATCH", flush=True)
                continue
            arm_cells = [f"{a} {span(ns[a])} {max(by[a]):.1f}B" for a, _, _ in arms]
            ratio = lambda a, b: [x / y for x, y in zip(ns[a], ns[b])]
            aa = ratio("F5", "F1")
            lo, hi = min(aa), max(aa)
            def verdict(v):
                if min(v) > hi:
                    return "OUTSIDE: hoisting FASTER"
                if max(v) < lo:
                    return "OUTSIDE: hoisting SLOWER"
                return "INSIDE NOISE"
            ratios = [f"A/A F5/F1 {span(aa)}"]
            for a in ("F2", "F3"):
                v = ratio(a, "F1")
                ratios.append(f"{a}/F1 {span(v)} {verdict(v)}")
            if cls == "c1":
                v = ratio("F4", "F2")
                ratios.append(f"F4/F2 {span(v)} " + ("F4 SLOWER" if min(v) > 1 else "F4 FASTER" if max(v) < 1 else "straddles 1"))
            if cls == "c4":
                v = ratio("F1u8", "F2")
                ratios.append(f"F1u8/F2 {span(v)}")
            print(f"{kind} {cls} | " + " | ".join(arm_cells) + " || " + " | ".join(ratios), flush=True)
