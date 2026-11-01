# Main-return wait, the trace arm on the i9

- Tip: claude/g-main-return-wait @ 8914486591. The exit hold is reverted, so golib equals 5d017630ae's.
- Build: a fresh checkout at the tip, then kit/make-trace.sh `on` (all 9 TRACE lines landed). One Native AOT publish of GoroutineRecoveringWhenMainReturns from it (build.txt), then make-trace.sh `restore`. Both files matched HEAD's blobs again. The patch was never committed.
- smoke-trace.txt: one idle traced run, the normal order. An idle untraced run wrote no file.
- Box: i9 (13th Gen Intel Core i9-13900K), win-x64. .NET SDK 10.0.403; ILCompiler 10.0.14. WHEA 0 from 2026-11-01 08:08 to 09:17 (local, CST).
- Harness: harness/race4.sh is race3.sh plus one traced label. Each round runs the traced label first, with GO2CS_MRW_TRACE set to traces-<load>/trace-<round>.txt, then the untraced label with it unset. So traced runs are the odd runs and untraced the even ones, all from the one binary. SPIN_S=14400; the spinner was alive for every run.
- Loads: hi (`loadhi` n8) and then n32 (`load` 32), 100 traced plus 100 untraced each.

## Files

- traces-hi/, traces-n32/: every traced run's trace (`<us>` TAB `<managed thread id>` TAB `<event>`). trace-<i>.txt pairs with runs.tsv's `m4-tr-<load>` row <i>.
- table-hi.txt, table-n32.txt (harness/r4-classify.py):
  - every late traced run, in ms from tail-start: park, the counted goroutine's system calls, the derived wait start, the wait's end (full line) and tail-wait-done;
  - its class;
  - three on-time traced runs;
  - the counts.
- A late run's stdout holds "recovered". Its print is the parked goroutine's last system call after its park.
- Classes:
  - A: the print is before tail-start.
  - B: the print is between tail-start and the wait's end. B is split by the wait's start, derived as wait-release minus its elapsedMs:
    - B-before-wait;
    - B-in-wait;
    - plain B when the wait ended by wait-exit (no elapsedMs).
  - B0: the print is after tail-start, and the wait found the count already 0 (no wait-release or wait-exit).
  - C: the print is after tail-wait-done.
  - D: the print is between the wait's end and tail-wait-done (not one of the brief's classes).
- tally.tsv, runs.tsv: per-arm counts and every run (400 rows).
- run.txt, affinity.tsv: the driver's log and 2 s affinity samples. Every readable test-process sample reads 0xF Normal.
