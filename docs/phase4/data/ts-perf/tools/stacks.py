# stacks.py: inclusive and exclusive time per frame from a speedscope file written by `dotnet-trace convert`.
#
#   python -I stacks.py <file.speedscope.json> [--top 40] [--threads 3] [--group] [--json out.json]
#
# Each thread is replayed from its open/close events. Exclusive time goes to the frame on top of the stack; inclusive
# time goes once to every distinct frame on the stack (recursion counted once). With --group, frames are folded to
# their module or namespace prefix (e.g. "go.slice`1", "go.builtin", "System.Collections.Generic") to read by
# construct rather than by method.
import argparse, collections, json, re

ap = argparse.ArgumentParser()
ap.add_argument("file")
ap.add_argument("--top", type=int, default=40)
ap.add_argument("--threads", type=int, default=3)
ap.add_argument("--group", action="store_true")
ap.add_argument("--json")
ap.add_argument("--callers", help="substring of a frame: print the callers (3 frames up) of the time spent in it")
A = ap.parse_args()
d = json.load(open(A.file, encoding="utf-8"))
frames = [f["name"] for f in d["shared"]["frames"]]
PSEUDO = {"CPU_TIME", "UNMANAGED_CODE_TIME"}


def group(name):
    # "module!Namespace.Type.Method(args)" -> "module!Namespace.Type"
    n = re.sub(r"\(.*$", "", name)
    mod, _, rest = n.partition("!")
    if not rest:
        return n
    parts = rest.split(".")
    return mod + "!" + ".".join(parts[:-1]) if len(parts) > 1 else n


out = {"threads": []}
profiles = sorted(d["profiles"], key=lambda p: -len(p.get("events", [])))
for p in profiles[:A.threads]:
    excl, incl, kinds, callers = collections.Counter(), collections.Counter(), collections.Counter(), collections.Counter()
    stack, last = [], None
    total = 0.0
    for e in p["events"]:
        t = e["at"]
        if last is not None and stack:
            dt = t - last
            total += dt
            names = [group(frames[i]) if A.group else frames[i] for i in stack]
            # dotnet-sampled-thread-time ends each stack in a pseudo frame: CPU_TIME (running managed code) or
            # UNMANAGED_CODE_TIME (native code: the runtime, the JIT, the GC, or a blocked wait). Exclusive time goes
            # to the real frame under it, tagged with the kind; the pseudo frame is counted as its own kind total.
            kind = ""
            if names and names[-1] in PSEUDO:
                kind = "native " if names[-1] == "UNMANAGED_CODE_TIME" else ""
                kinds[names[-1]] += dt
                names = names[:-1]
            if names:
                excl[kind + names[-1]] += dt
            if A.callers:
                for k in range(len(names) - 1, -1, -1):
                    if A.callers in names[k]:
                        callers[" <- ".join(re.sub(r"\(.*$", "", x).split("!")[-1][-70:] for x in names[max(0, k - 3):k][::-1])] += dt
                        break
            for n in set(names):
                incl[n] += dt
        last = t
        if e["type"] == "O":
            stack.append(e["frame"])
        else:
            if stack and stack[-1] == e["frame"]:
                stack.pop()
            elif e["frame"] in stack:
                del stack[len(stack) - 1 - stack[::-1].index(e["frame"]):]
    th = {"thread": p["name"], "ms": round(total, 1), "kinds": {k: round(v, 1) for k, v in kinds.items()},
          "exclusive": [(n, round(v, 1), round(100 * v / max(total, 1e-9), 1)) for n, v in excl.most_common(A.top)],
          "inclusive": [(n, round(v, 1), round(100 * v / max(total, 1e-9), 1)) for n, v in incl.most_common(A.top)]}
    out["threads"].append(th)
    print(f"== {p['name']}: {total:,.0f} ms sampled; " + ", ".join(f"{k} {v:,.0f} ms" for k, v in kinds.items()))
    print("  -- exclusive")
    for n, v, pc in th["exclusive"]:
        print(f"  {pc:5.1f}% {v:10,.0f} ms  {n[:150]}")
    if A.callers:
        print(f"  -- callers of {A.callers}")
        for n, v in callers.most_common(A.top):
            print(f"  {100 * v / max(total, 1e-9):5.1f}% {v:10,.0f} ms  {n}")
        continue
    print("  -- inclusive")
    for n, v, pc in th["inclusive"]:
        print(f"  {pc:5.1f}% {v:10,.0f} ms  {n[:150]}")
if A.json:
    json.dump(out, open(A.json, "w", encoding="utf-8"), indent=1)
