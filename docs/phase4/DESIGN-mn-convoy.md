# DESIGN — M:N: the P-token hand-back (the s_p convoy row, .5)

COORD's ruling of 2026-11-22 (docs/phase4/inbox/G/20261122T110220Z-COORD.md item 2): the convoy that the train-4 storm
capture read at GoScheduler's P semaphore `s_p` is a .5 row of its own, after row 7 pieces 2+3 removed the blocking
calls that produced it in MnMixedStorm. A blocking call through any path that remains (cgo, syscalls, foreign threads,
the stdlib calls row 7 does not twin) can recreate it.

## 1. What was captured

The hung storm (train-4 tip 1f2a9afdfc, n=100,000, windows, P=12), from a dotnet-stack report: 46,223 OS threads, of
them 19,228 back from a blocking call and queued in GoScheduler.ExitBlocked on `s_p`, a SemaphoreSlim: 12,027 waiting
on it and 7,201 spinning in Monitor.Enter on its one internal lock; 114% of one core. Every wait and every release of
a SemaphoreSlim goes through that monitor.

## 2. The arms

- **base**: `s_p` is a SemaphoreSlim (the code that shipped through train 4).
- **fifo**: `s_p` is PTokens (golib/runtime/GoScheduler.PTokens.cs): free tokens taken by compare-and-swap, waiters in a
  lock-free FIFO queue, each on its own event, granted by compare-and-swap; a waiter is counted before it queues and
  retries after, so no wakeup is lost. GolibTests PTokensTests holds exact conservation and the lost-wakeup arm. This
  arm lives on claude/g-mn-convoy-fifo-arm, unseated and never landed, kept so the miss stays reproducible.

One declaration line in GoScheduler.cs switches the arms; every reading below restores it byte-identical.

## 3. Predictions, written before the A/B, and their scores

| # | prediction (as posted) | score |
|---|---|---|
| P1 | MnBlockReturn n=20,000, MN=1: return phase 0.40 s or less (3x+ the base's 1.16-1.33 s); under 2x is a design read | **MISSED** |
| P2 | peak OS threads unchanged (~20,000) | met |
| P3 | spawn phase unchanged (4.2-4.6 s) | met |
| P4 | MnMixedStorm n=100,000 unchanged | not run on the fifo arm |
| P5 | default mode does not move | met |
| P6 | no regression; PTokens rows pass | rows pass; the rest not run on the fifo arm |

**P1, the read.** 10 runs each (windows, P=12): base 741-1,474 ms, median ~925; fifo 1,430-2,653 ms, median ~1,570,
about 1.7x SLOWER. A strict-FIFO hand-back grants each token to the OLDEST waiter, which is asleep in the kernel, so
every grant costs a kernel wake. SemaphoreSlim's waiters spin before they sleep, and a spinner takes a released token
hot with no wake. Its monitor is what convoys, and only in the tail. MnBlockReturn's return phase is also bounded by
20,000 runnable threads at once: default mode pays 2.1 s with no scheduler at all. So it measures the median
hand-back badly, and it is not the instrument the row is judged by.

## 4. The second instrument: MnHandBack

100 goroutine pairs ping-pong 1,000 times through plain Locks reached via a sync.Locker, on carriers. The pair's function
is a twin, through one runtime.Gosched: without it the goroutines ran plain and never touched `s_p`, and the first
version read both arms alike. So each cycle is one block -> return through ExitBlocked, on 200 threads. Per hand-back,
MN=1, windows P=12, 3 runs each: base p50 2.4-31 us, p99 346-552 us, ~180 ms wall; fifo p50 173-192 us, p99 366-456 us,
~400 ms wall. The kernel wake per grant is visible directly at p50.


## 5. COORD's (B): the tail at 50 runs per arm, windows and linux (MN=1)

MnBlockReturn return phase. Linux runs n=10,000: at n=20,000 this box (WSL, max_map_count 65530) aborts the process with
PAL_SEHException while it creates the blocked threads, an environment limit (Go's own default caps a program at 10,000
threads, debug.SetMaxThreads), not a convoy.

| OS | arm | median | p90 | max | over 5 s |
|---|---|---|---|---|---|
| windows P=12, n=20,000 | base | 1,013 ms | 1,506 | 27,856 | **1 in 50** |
| windows P=12, n=20,000 | fifo | 1,624 | 2,096 | 2,413 | 0 |
| linux P=10, n=10,000 | base | 1,235 | 1,615 | 5,579 | **1 in 50** |
| linux P=10, n=10,000 | fifo | 1,144 | 1,327 | 2,237 | 0 |

MnHandBack, 10 runs per arm: no p99 convoy in the base at P=12 (windows p99 287-622 us; linux 333-582 us). The base tail
is REAL on both OSes, so the row went to (A), COORD's rule.

## 6. Design (A): the spin tier

PTokens keeps the lock-free count and FIFO sleepers and adds a SPIN tier ahead of the queue: a waiter spins on the free
count (35 SpinWait steps, never sleeping), counted in m_spinning. Release frees its token to the count and returns while
a spinner is counted (a spinner takes it hot, with no kernel wake), else grants the oldest sleeper. The tier's own hazard
is a release left for a spinner that then gives up: the LAST spinner to leave hands any token still free to the queue
(LeaveSpin), and the interlocked operation on each side is a full fence, so either order of the release's read of
m_spinning and the spinner's decrement serves the sleeper. GolibTests holds it with a deterministic row (a seam in the
give-up window), red with the hand-off removed.

Predictions A1-A5 (posted before the run) and the low-P arm AP1-AP2 (COORD's addition), scored:

| # | prediction | reading | score |
|---|---|---|---|
| A1 | MnBlockReturn median at or under the base's, 0 convoys in 50 per OS | windows 995 ms vs 987 (+0.8%), max 1,548, 0 of 50; linux 1,169 vs 1,235, max 1,984, 0 of 50 | **met** |
| A2 | MnHandBack p50 under 60 us, p99 at or under 700 us, wall at or under 250 ms | windows p50 med 20.2, p99 med 373, wall 174; linux p50 16.7, p99 369, wall 228 | **met** |
| A3 | default mode does not move | windows MnBlockReturn MN unset 2,084-2,194 ms vs 2,064-2,084; hand-back in band | **met** |
| AP1 | P=2: MnHandBack within 10% of the base | base p50 8.8 us, p99 4,260 us, wall 647 ms; (A) p50 1.6, p99 509, wall 237 | **met** (better) |
| AP2 | P=2: MnBlockReturn median within 10% of the base, 0 over 5 s | base median 25,656 ms (every run over 5 s); (A) 984 ms, 0 over 5 s | **met** (26x) |
| A4 | the rows pass, each red with its fix removed | the post-queue retry removed: AReleaseInTheWaitersWindowIsNotLost reds (alone); LeaveSpin's hand-off removed: the FIRST give-up row stayed GREEN (vacuous: a spinner that gives up wins the free token by its own post-queue retry), so it was replaced by TokensLeftForASpinnerThatTakesFewerReachTheSleeper (two releases left for one spinner, which takes one and holds it), red with the hand-off removed | met after the fold |
| A5 | no regression | MnMixedStorm n=100,000 50 runs: 0 hangs, 0 differences; the S2 arms and MnLockSleep/Fair/Wake EQUAL to Go with MN set and unset; GenTests 761/761; GolibTests Release 3106/53/0, Debug 3081/78/0; the Q1 kit re-run after the disk fill (section 7) | met |

At P=2 the base's hand-back convoys outright (p99 above 4 ms, MnBlockReturn 22-31 s for one release of 20,000); the spin
tier removes it.

## 7. A Linux limit found on the way: about 16,000 simultaneously blocked threads

At n=20,000 MnBlockReturn aborted on linux (Ubuntu 22.04 under WSL, default settings): `terminate called after throwing
an instance of 'PAL_SEHException'` while the blocked threads were created, then SIGABRT (rc 134). Not the thread ceiling
(threads-max 127,859; ulimit -u 63,929) and not memory (15 GB, nothing in dmesg). Read-only test of the hypothesis
(COORD's ruling 2): a live n=10,000 run peaked at 40,427 memory mappings with 9,985 threads (scheduler on; 39,643 at 9,566
with it off), about 4 mappings per thread, which projects to about 81,000 at 20,000 threads, past `vm.max_map_count`
65,530: the limit falls at about 16,000 threads. It is the limit KnownIssues already names for goroutines (lane R's
16,226-16,229 threads, 2026-11-09), reached here by goroutines BLOCKED in plain calls: under the M:N scheduler a
goroutine parked in a twin holds no thread, so the limit counts only those blocked in calls the twins do not cover. Go,
whose blocked goroutines are not threads, runs the same program. This row's linux readings therefore use n=10,000.

The aborts were costly in their own right: WSL's core pattern pipes every Linux crash dump to the WINDOWS side
(`|/wsl-capture-crash`, writing to %LOCALAPPDATA%\Temp\wsl-crashes), a piped dump ignores `ulimit -c 0`, and seven
aborted runs wrote 43 GB there, filling C: mid-gate (the Q1 kit's 22 builds failed for space). The owner has since
allowed WSL changes for this work: a many-thread Linux run now proves dump suppression first (core_pattern set to a
file for the run, `ulimit -c 0`, DOTNET_DbgEnableMiniDump=0, DOTNET_EnableCrashReport=0, one deliberate abort that
leaves no file). WSL refused sparse mode for its disk as unsafe, so it was not forced.

## 8. The review folds (COORD's PTokens review, 2026-11-22)

The review seated (A) with folds, all in PTokens:

- **API for GOMAXPROCS** (C1's SetProcs, merged here): `Wait(0)` only tries the free count (no spin, no queue);
  `Wait(timeout)` withdraws its node by compare-and-swap on timeout; a failed withdraw means the node was granted, so
  it returns true; `Release(n)` runs the spinner check and the hand-off like `Release()`, never a bare addition.
- **arm64 ordering**: a full fence between each interlocked write and the read of the other side's variable that
  follows it (Release, LeaveSpin, the sleeper's count-and-enqueue before its retry, the hand-off loop).
- **Backoff**: the hand-off backs off (SpinWait) when it finds a sleeper counted but the queue empty.
- **Spin cap**: at most max(1, P/2) spinners, as Go keeps at most GOMAXPROCS/2 Ms spinning; an arrival over the cap
  queues at once and is never counted, so no release leaves a token for it.
- **Bounded queue**: `m_waiting` counts the live (Waiting) nodes and is decremented by whoever moves a node out of
  Waiting; the event is created only when a waiter sleeps; each arrival reaps the withdrawn nodes at the queue's head.
- **Interrupt**: a sleeper interrupted in its wait withdraws (or, granted already, gives the token back) and rethrows;
  the spin tier uncounts its spinner on every way out.
- **Rows**, each red with its fix removed: withdraw against grant (the double-token give-back), `Release(n)` wakes
  min(n, sleepers), `Wait(0)` never spins or queues, a timed-out wait leaves nothing behind, a grant racing a
  timeout is reported as a taken token, timed waits under stress conserve tokens, an interrupted wait leaves nothing
  behind. The stress row alone stayed GREEN with the failed-withdraw fix removed (the race is too narrow to hit), so
  the timeout race got its own seamed row, red without that fix. Gates: the 11 rows green 3 of 3; each of the eight
  fixes removed in turn reds its row(s), the file restored byte-identical; GolibTests Release 3113/53/0, Debug
  3088/78/0.

Re-score predictions R1-R4, written before the run (windows, base = SemaphoreSlim and F = the folded PTokens, one
session, GO2CS_MN=1, 10 runs per arm per row; MnBlockReturn n=20,000):

| # | prediction | reading | score |
|---|---|---|---|
| R1 (A1) | P=12: F's MnBlockReturn median within 10% of the base's, 0 over 5 s | base median 899 ms (750-1,905, p90 1,443); F 1,141 (890-1,193), +27%; 0 over 5 s in both | **missed** |
| R2 (A2) | P=12: F's MnHandBack p50 median under 60 us, p99 median at or under 700 us, wall median at or under 250 ms | F p50 med 5.5 us, p99 med 429 us, wall med 179 ms (base 34.8, 357, 185) | **met** |
| R3 (AP1) | P=2: F's MnHandBack p50 and wall medians at or under the base's, p99 median at or under 1,000 us | base p50 14.5, p99 3,949, wall 668; F p50 14.1, p99 2,558, wall 626 ((A) before the folds: p50 1.6, p99 509, wall 237) | **missed** (p99) |
| R4 (AP2) | P=2: F's MnBlockReturn median under 2,000 ms, 0 over 5 s (the base convoys: every run over 5 s) | base median 24,847 ms (all 10 over 5 s); F 910 ms, 0 over 5 s | **met** |

The cap is the fold most likely to move a reading: at P=2 it admits one spinner where (A) admitted any number, so a
hand-back that a second spinner took hot may now wake a sleeper. R3's p99 bar is wider than A2's for that reason.

Read at c11cb99971, windows, one session. Against (A)'s own readings before the folds (section 6), F lost the P=2
hand-back gain (wall 237 to 626 ms, p99 509 to 2,558 us) and moved MnBlockReturn at P=12 from 995 to 1,141 ms. The
prediction named the cap as the likeliest cause; the folds also added fences and the lazy event.

**R5, the attribution arm, predicted before its run.** One axis: F against F with the spin cap lifted (m_spinCap =
int.MaxValue, nothing else changed), the same four blocks, 10 runs each, one session. If the cap is the cost, F-nocap
reads P=2 MnHandBack p99 median at or under 1,000 us and wall median at or under 300 ms, and P=12 MnBlockReturn median
within 10% of R1's base (at or under 989 ms), while F re-reads within 10% of its R1-R3 readings (the control). If
F-nocap reads like F, the cost is in the other folds (fences, lazy event, reaping) and the cap is not the cause.

**R5 reading** (one session, F then F-nocap, 10 runs per block):

| block | F (the control) | F-nocap |
|---|---|---|
| P=12 MnBlockReturn median | 981 ms (845-1,155) | 990 ms (802-1,192) |
| P=12 MnHandBack p50 / p99 / wall medians | 3.4 us / 436 us / 185 ms | 22.2 us / 381 us / 176 ms |
| P=2 MnBlockReturn median | 971 ms | 833 ms |
| P=2 MnHandBack p50 / p99 / wall medians | 15.5 us / 2,437 us / 643 ms | 1.8 us / 335 us / 236 ms |

Scored: the P=2 leg is **met** (p99 335 us, wall 236 ms: the cap is the P=2 hand-back cost, and lifting it restores
(A)'s band). The P=12 leg is **missed by 1 ms** (990 against 989), and moot: F and F-nocap read alike there. The
control is **missed** on P=12 MnBlockReturn alone (981 here against 1,141 in R1, -14%): that block moves by about 15%
between sessions, so R1's miss is not attributable to the folds; the control held on every MnHandBack block.

Finding: at P=2 a cap of max(1, P/2) leaves one spinner, and a hand-back pair needs a second to take a token hot; at
P=12 the cap (6) never binds in these fixtures. The cap stays as COORD folded it until a ruling: keep it, lift it, or
floor it at 2 (max(2, P/2)), which would read as F-nocap at P=2 only if two spinners suffice there (not measured).
