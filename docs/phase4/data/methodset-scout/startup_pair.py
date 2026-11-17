"""startup_pair.py A_EXE B_EXE N : wall-clock start-up of two builds of one program, interleaved A,B,A,B,... N pairs
after 3 warm-up runs each; prints median and mean per side, the median of the paired differences (B - A), and an
A/A control (A's odd runs against A's even runs) so the noise floor is on the same page."""
import statistics, subprocess, sys, time

def run(exe):
    t = time.perf_counter()
    subprocess.run([exe], stdout=subprocess.DEVNULL, stderr=subprocess.DEVNULL, check=True)
    return (time.perf_counter() - t) * 1000.0

def main(a, b, n):
    for _ in range(3):
        run(a); run(b)
    ta, tb = [], []
    for _ in range(n):
        ta.append(run(a)); tb.append(run(b))
    d = [y - x for x, y in zip(ta, tb)]
    aa = [ta[i + 1] - ta[i] for i in range(0, len(ta) - 1, 2)]
    print(f"A median {statistics.median(ta):.1f} ms mean {statistics.mean(ta):.1f}")
    print(f"B median {statistics.median(tb):.1f} ms mean {statistics.mean(tb):.1f}")
    print(f"B-A paired median {statistics.median(d):+.1f} ms (n={n})")
    print(f"A/A control median {statistics.median(aa):+.1f} ms (n={len(aa)})")

if __name__ == "__main__":
    main(sys.argv[1], sys.argv[2], int(sys.argv[3]))
