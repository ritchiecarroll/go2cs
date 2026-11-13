# demo_compare.py: one demo, native Go vs converted C#, byte for byte.
#
#   python -I demo_compare.py --name <name> --native <exe> --converted <dll|exe> --cwd <dir> --out <dir> -- <args...>
#
# Runs both sides with the same arguments in <cwd>, writes <out>/<name>.go.txt, <name>.cs.txt and <name>.json:
# {"match": true only when both exit 0 and the outputs are byte-identical, "go": {...}, "cs": {...}}.
import argparse, hashlib, json, os, subprocess, sys, time

sys.stdout.reconfigure(encoding="utf-8")
ap = argparse.ArgumentParser()
for a in ("name", "native", "converted", "cwd", "out"):
    ap.add_argument("--" + a, required=True)
ap.add_argument("--dotnet", default="dotnet")
ap.add_argument("--timeout", type=int, default=3600)
ap.add_argument("args", nargs="*")
A = ap.parse_args()
os.makedirs(A.out, exist_ok=True)
res = {"name": A.name, "args": A.args}
for side in ("go", "cs"):
    exe = [A.native] if side == "go" else ([A.dotnet, A.converted] if A.converted.lower().endswith(".dll") else [A.converted])
    t = time.time()
    try:
        p = subprocess.run(exe + A.args, cwd=A.cwd, capture_output=True, timeout=A.timeout)
        rc, out, err = p.returncode, p.stdout, p.stderr
    except subprocess.TimeoutExpired as e:
        rc, out, err = "timeout", e.stdout or b"", e.stderr or b""
    open(os.path.join(A.out, f"{A.name}.{side}.txt"), "wb").write(out)
    res[side] = {"rc": rc, "bytes": len(out), "lines": out.count(b"\n"), "sha256": hashlib.sha256(out).hexdigest(),
                 "seconds": round(time.time() - t, 1), "stderr_tail": err.decode("utf-8", "replace")[-2000:]}
res["match"] = res["go"]["rc"] == 0 and res["cs"]["rc"] == 0 and res["go"]["sha256"] == res["cs"]["sha256"]
json.dump(res, open(os.path.join(A.out, f"{A.name}.json"), "w", encoding="utf-8"), indent=1)
print(f"{A.name}: {'MATCH' if res['match'] else 'DIFFER'} go rc={res['go']['rc']} {res['go']['sha256'][:12]} cs rc={res['cs']['rc']} {res['cs']['sha256'][:12]}")
sys.exit(0 if res["match"] else 1)
