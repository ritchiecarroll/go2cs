"""r4-classify.py <tag> <mode> <out.txt> -- the trace arm's table, for one load.

For each traced run (work/m4-tr-<tag>-<mode>/out-<i>.txt, traces-<tag>/trace-<i>.txt): times in ms relative to
tail-start for the counted goroutine's park, its system calls (the print's WriteFile among them), the wait's end
(wait-release, with its full line, or wait-exit) and tail-wait-done. A late run's stdout holds "recovered"; its print
is the parked goroutine's last system call after its park. Classes (G's brief):
  A  the print is before tail-start
  B  the print is between tail-start and the wait's end
  C  the print is after tail-wait-done
  (B is split by the derived wait start: B-before-wait, B-in-wait; plain B when the wait ended by wait-exit)
  D  the print is between the wait's end and tail-wait-done (not in the brief; listed if it happens)
  B0 the print is after tail-start, and the wait found the count already 0 (no wait-release or wait-exit)
  ?  a late run whose trace lacks the events to place it
Prints every late run in full, three on-time traced runs, and the counts, including the untraced half.
"""
import os, re, sys

P = r"C:\go2cs-build\logs\pado"
tag, mode, out_path = sys.argv[1], sys.argv[2], sys.argv[3]
tr_dir = os.path.join(P, "r4", "traces-" + tag)
work_tr = os.path.join(P, "work", f"m4-tr-{tag}-{mode}")
work_un = os.path.join(P, "work", f"m4-un-{tag}-{mode}")

def late(path):
    return "recovered" in open(path, encoding="utf-8", errors="replace").read()

def runs(d):
    return sorted(int(m.group(1)) for f in os.listdir(d) if (m := re.match(r"out-(\d+)\.txt$", f)))

def parse(path):
    ev = []
    for line in open(path, encoding="utf-8", errors="replace"):
        c = line.rstrip("\n").split("\t", 2)
        if len(c) == 3 and c[0].isdigit():
            ev.append((int(c[0]), c[1], c[2]))
    return ev

def place(i):
    path = os.path.join(tr_dir, f"trace-{i}.txt")
    if not os.path.exists(path):
        return None, "no trace file", []
    ev = parse(path)
    t0 = next((t for t, _, e in ev if e.startswith("tail-start")), None)
    park = next(((t, e) for t, _, e in ev if e.startswith("park ")), None)
    end = next(((t, e) for t, _, e in ev if e.startswith("wait-release") or e.startswith("wait-exit")), None)
    done = next((t for t, _, e in ev if e.startswith("tail-wait-done")), None)
    g = re.search(r"goroutine=(\d+)", park[1]).group(1) if park else None
    sys_after = [(t, e) for t, _, e in ev if e.startswith("syscall ") and g and f"goroutine={g} " in e + " " and park and t >= park[0]]
    rel = lambda t: "-" if t is None or t0 is None else f"{(t - t0) / 1000.0:+.3f}"
    # The wait's own start, derived: wait-release carries its elapsedMs (wait-exit does not).
    m = re.search(r"elapsedMs=([0-9.]+)", end[1]) if end else None
    ws = end[0] - int(float(m.group(1)) * 1000) if m else None
    lines = [f"  tail-start {'present' if t0 is not None else 'ABSENT'}",
             f"  park       {rel(park[0]) if park else '-'}  {park[1] if park else ''}",
             *[f"  syscall    {rel(t)}  {e}" for t, e in sys_after],
             f"  wait-start {rel(ws)}  (derived: wait-end minus elapsedMs)",
             f"  wait-end   {rel(end[0]) if end else '-'}  {end[1] if end else ''}",
             f"  wait-done  {rel(done)}"]
    if t0 is None or not sys_after:
        return "?", "", lines
    pt = sys_after[-1][0]
    if pt < t0:
        cls = "A"
    elif end is not None and pt < end[0]:
        # B, split by the derived wait start where there is one: before main's wait began, or inside it
        cls = "B" + ("" if ws is None else ("-before-wait" if pt < ws else "-in-wait"))
    elif done is not None and pt > done:
        cls = "C"
    elif end is not None and done is not None:
        cls = "D"
    elif end is None and done is not None:
        cls = "B0"  # after tail-start, and the wait found the count already 0: it never looped
    else:
        cls = "?"
    return cls, "", lines

tr, un = runs(work_tr), runs(work_un)
late_tr = [i for i in tr if late(os.path.join(work_tr, f"out-{i}.txt"))]
late_un = [i for i in un if late(os.path.join(work_un, f"out-{i}.txt"))]
counts = {}
with open(out_path, "w", encoding="utf-8") as o:
    o.write(f"# trace arm, {tag} ({mode}): late = stdout holds \"recovered\"; times are ms from tail-start\n")
    for i in late_tr:
        cls, note, lines = place(i)
        counts[cls] = counts.get(cls, 0) + 1
        o.write(f"\nLATE traced run {i}: class {cls} {note}\n" + "\n".join(lines) + "\n")
    shown = 0
    for i in tr:
        if i in late_tr or shown == 3:
            continue
        cls, note, lines = place(i)
        o.write(f"\nON-TIME traced run {i}\n" + "\n".join(lines) + "\n")
        shown += 1
    o.write(f"\nCOUNTS late traced {len(late_tr)}/{len(tr)}, late untraced {len(late_un)}/{len(un)}, classes "
            + " ".join(f"{k}={counts.get(k, 0)}" for k in ("A", "B-before-wait", "B-in-wait", "B", "B0", "C", "D", "?")) + "\n")
print(open(out_path, encoding="utf-8").read().splitlines()[-1])
