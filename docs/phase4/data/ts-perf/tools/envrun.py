# envrun.py <runs> <exe> -- args... : wall, CPU and peak RSS per run of one command, with extra env from P4ENV
# ("K=V K=V"). One warm-up first. Prints the median and each run's output hash.
import hashlib, os, statistics, subprocess, sys, time
runs, exe, args = int(sys.argv[1]), sys.argv[2], sys.argv[4:]
env = dict(os.environ)
for kv in os.environ.get("P4ENV", "").split():
    k, v = kv.split("=", 1); env[k] = v
def one():
    t0 = time.perf_counter()
    p = subprocess.Popen([exe] + args, stdout=subprocess.PIPE, stderr=subprocess.DEVNULL, env=env)
    out = p.stdout.read(); _, st, ru = os.wait4(p.pid, 0)
    return time.perf_counter() - t0, ru.ru_utime + ru.ru_stime, ru.ru_maxrss / 1024, hashlib.sha256(out).hexdigest()[:12]
one()
rs = [one() for _ in range(runs)]
print(f"{os.environ.get('P4LABEL','')}\twall {statistics.median(r[0] for r in rs):.2f}\tcpu {statistics.median(r[1] for r in rs):.2f}\trss {statistics.median(r[2] for r in rs):.0f}MB\t{sorted({r[3] for r in rs})}", flush=True)
