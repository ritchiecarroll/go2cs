# t.py: wall, CPU and peak RSS of one command, one process per run. Usage: t.py RUNS WARMUP -- cmd args...
import os, statistics, subprocess, sys, time
runs, warm = int(sys.argv[1]), int(sys.argv[2]); cmd = sys.argv[4:]
rows = []
for i in range(warm + runs):
    t0 = time.perf_counter()
    p = subprocess.Popen(cmd, stdout=subprocess.DEVNULL, stderr=subprocess.PIPE)
    err = p.stderr.read()
    _, st, ru = os.wait4(p.pid, 0)
    w = time.perf_counter() - t0
    if i >= warm: rows.append((w, ru.ru_utime + ru.ru_stime, ru.ru_maxrss / 1024, st))
    last = err
med = lambda k: statistics.median(r[k] for r in rows)
print(f"wall median {med(0)*1000:.0f} ms (min {min(r[0] for r in rows)*1000:.0f}, max {max(r[0] for r in rows)*1000:.0f}); cpu {med(1)*1000:.0f} ms; rss {med(2):.0f} MB; status {set(r[3] for r in rows)}")
if last: sys.stdout.write(last.decode(errors="replace")[-3000:])
