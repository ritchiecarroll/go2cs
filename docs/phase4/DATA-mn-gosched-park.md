# DATA — M:N Gosched park: runtime.Gosched in a twin is a park point (2026-11-21)

S2 arm 1 (P5's MnSpinCap, `a1c5bf2b6f`): goroutines spinning on `runtime.Gosched` beyond the P count starved the parked
goroutines under `GO2CS_MN=1`. Gosched was a thread yield, so a spinner's step never ended. Injection is once per step
and capped. COORD ruled cure (a), Go parity (docs/phase4/inbox/G/20261121T230957Z-COORD.md), and admitted
`builtin.Goschedꞏtwin` to `[mn-frozen]` (docs/phase4/inbox/G/20261121T231225Z-COORD.md). P3 cleared the shape
against Go's `goschedImpl -> globrunqput + wakep` and asked for six checks, (a)-(f).

Branch `claude/g-mn-gosched-park`: `ed6fc23e74` (the cure), `757c96ced3` (the rows), `e860391c66` (master
`42d78181d4` merged), `f05c71c6f8` (the converter's name list; the fairness row fixed).

## The cure

`builtin.Goschedꞏtwin()`, a twin entry point in golib. runtime is a hand-owned stdlib package with no public hand
twin under L-min, so the generator maps a twin's call to `runtime.Gosched` to it (`TwinGenerator.IsGoschedIntrinsic`).
On a carrier it is a yield park: the step ends, and the goroutine goes to the GLOBAL run queue's tail (never the
run-next slot); a pinned goroutine goes back to its own carrier. Off a carrier it is runtime.Gosched's own thread yield
and a completed task. READING gains `gosched-yields=N`.

## The gates, on the merged tip f05c71c6f8

| check | reading |
|---|---|
| MnSpinCap, red then green | at `a1c5bf2b6f`, `GO2CS_MN=1`, P=4 (MAXINJECT defaults to P, P5's condition): rc 124. On the cure: 3 of 3 EQUAL to Go, 19,624-22,697 yields; default P EQUAL; MN unset EQUAL on both |
| (a) off a carrier | GolibTests rows: a completed task on the test thread and on a non-carrier thread; made to fail first |
| (b) fairness | MnGoschedFair at ONE P, no injection: 3 of 3 EQUAL to Go, about 128,000 run-next steps per 1,000 yields. With `MaxRunNextChain` lifted it HANGS (rc 124); restored byte-identical |
| (c) a wake after the put | Enqueue claims an idle carrier and releases it (Go's wakep); with none idle the yielding carrier takes the global queue next |
| (d) locked | MnLockPin's gosched shape: 0 thread changes. Made to fail (a pinned yield sent to the global queue): 2,000; restored |
| (e) D4 | the MN-unset runs above, and the GenTests plain half |
| (f) population | one converted non-test stdlib caller (crypto/internal/fips140/check/checktest); the rest inside the hand-owned runtime |
| suites | GenTests 753/753; GolibTests R 3066/52/0 and D 3041/77/0 of 3118; the Q1 kit 20/20, GO2CS0017/18 = 0, 8 of 8 EQUAL to plain; repoguard full rc 0 |

## The fairness row was vacuous at first

Its made-to-fail run is what found it: with the run-next chain bound lifted, the first MnGoschedFair still went green
(`run-next-steps=2`). Two causes:
- The ponger's `for v := range a` is a synchronous foreach in a twin (N11). It blocks its thread instead of parking,
  so the pair never chained through run-next.
- At start-up the monitor injected a carrier for a cold step, and that second carrier served the yielder whatever the
  bound was.

The fix: the ponger receives explicitly, and the row runs with `GO2CS_MN_PROCS=1 GO2CS_MN_MAXINJECT=0`. The row then
reads about 128 run-next steps per yield with the bound, and hangs without it.

The first gate run (before the master merge) also met `TheConvertersListIsGolibsNamespace`, red in R and D: the
converter's golib name list lacked `Goschedꞏtwin`. The list was taken from the test's rendering, one line.
