# DESIGN: the Gosched escalation's clock on darwin

C1, 2026-11-08. COORD's ruling: GoschedBackoff's escalation on macOS waits on a high-resolution clock, as Windows'
does. This note records the measurements behind the choice, the cost model, and what linux does.

## The defect

`GoschedBackoff.Yield` escalates the 64th consecutive inert yield to a 1 ms sleep. On Windows that sleep waits on
`GoHighResolutionTimer` (ratified 2026-09-26). Everywhere else it is `Thread.Sleep(1)`.

On the hosted osx-arm64 runner, `Thread.Sleep(1)` sleeps about 9 ms. runtime/pprof's `TestGoroutineSwitch` makes
10 x 1,000,000 lone `Gosched` calls, which is at most 156,250 escalations. At the measured mean of 8.26 ms per
escalation, that is about 1,290 s, over the converted host's 900 s per-test deadline. The stack probe
(os-matrix run 42438677029) showed the test thread in `GoschedBackoff.Yield` -> `Thread.Sleep` at both 120 s and 870 s.
On osx-x64 the mean is 2.74 ms, about 429 s, and the test passes. Any Gosched-heavy program on Apple silicon pays the
same cost.

## Candidates, measured

`GoschedEscalationClockCandidatesProbe`, on claude/c1-gosched-darwin-latency at 27d675b9e, ran as os-matrix run
42445042481 on the hosted runners. Each candidate made 500 waits of 1 ms on a fresh thread. CPU is the waiting thread's
own time (`thread_info`), per wait.

| candidate | osx-arm64 (macOS 15.8, 3 cpus): mean / p50 / p90 / p99 / max ms, CPU | osx-x64 (macOS 15.7, 4 cpus): same |
|:--|:--|:--|
| `Thread.Sleep(1)` | 7.84 / 9.01 / 9.04 / 11.64 / 13.05, 20 us | 2.45 / 1.63 / 5.72 / 8.27 / 9.48, 46 us |
| `nanosleep` 1 ms | 6.93 / 8.98 / 9.03 / 12.73 / 13.57, 13 us | 1.85 / 1.35 / 2.94 / 6.89 / 8.40, 39 us |
| `mach_wait_until` +1 ms | 7.58 / 9.01 / 9.04 / 11.85 / 18.69, 11 us | 2.39 / 1.51 / 6.13 / 7.19 / 8.58, 36 us |
| **kqueue `EVFILT_TIMER` 1 ms, `NOTE_CRITICAL`** | **1.02 / 1.02 / 1.03 / 1.05 / 1.10, 7 us** | **1.12 / 1.11 / 1.16 / 1.27 / 1.97, 42 us** |
| `Thread.Yield` spin to 1 ms | 1.34 / 1.00 / 1.04 / 10.02 / 10.97, 865 us | 2.01 / 1.44 / 2.86 / 9.58 / 77.73, 115 us |

- The three sleeps, `Thread.Sleep`, `nanosleep` and `mach_wait_until`, read the same: about 9 ms at p50 on arm64,
  with a long tail on x64. The kernel coalesces their timers, and no plain sleep call avoids that here.
- The kqueue timer with `NOTE_CRITICAL` stays within 0.3 ms of 1 ms at p99 on both runners.
  - `sys/event.h` documents `NOTE_CRITICAL` as asking for minimal timer coalescing.
  - The same timer WITHOUT that flag was not measured. That axis was not varied, so the flag is chosen on the
    documented meaning, not on an A/B reading.
- The spin is accurate at the median, but it burns the processor (865 us of CPU per 1 ms wait on arm64). It still
  takes a 10 ms tail whenever the thread is preempted. It defeats the escalation's purpose, which is to leave the run
  queue so a starved peer can run.

## Choice

On macOS, the escalation waits on a one-shot kqueue `EVFILT_TIMER` (`NOTE_USECONDS | NOTE_CRITICAL`, 1000 us). This is
`GoCriticalTimer.TrySleep` in golib. As on Windows, the timer object is created per escalation (`kqueue`, one
`kevent` that registers and waits, `close`): a few microseconds against a 1 ms wait. No per-thread descriptor exists
to classify or leak. Any failure answers false, and the caller falls back to `Thread.Sleep(1)`. An interrupted wait
(`EINTR`) counts as having slept, because a backoff that wakes early is still a backoff.

## Cost model

- **Latency saved**, per escalation:
  - arm64: 8.26 -> about 1.02 ms mean, so `TestGoroutineSwitch`'s 156,250 escalations drop from about 1,290 s to
    about 160 s.
  - x64: 2.74 -> about 1.12 ms.
- **CPU burned**, per escalation: the probe read 7 us (arm64) and 42 us (x64) of thread CPU for the wait itself,
  against 20 us and 46 us for `Thread.Sleep(1)`. The per-escalation `kqueue` and `close` add two syscalls (a few
  microseconds, not separately measured). The escalation still leaves the run queue; nothing spins.
- **Wakeups**: about 1,000 per second per escalating thread instead of about 110 (arm64). That is the intended rate:
  the ratified escalation is 1 ms.

## Linux

Unchanged. `Thread.Sleep(1)` measured 1.09 ms p50 and 1.18 ms p99 on a local linux x64 host (a stand-in loop, not
golib). That is already the ratified 1 ms, and the latency arm stays Inconclusive there.

## Not rooted, and not chased here

In the latency arm on both mac runners (four readings, runs 42443148330 and 42445042481), the loop's total time
exceeds its timed escalations by 1.3 to 12.4 s across about 31,700 calls that only yielded: a derived 41 to 391 us
per `Thread.Yield`, varying run to run. It does not transfer to `TestGoroutineSwitch`: at 122 us (osx-x64's first
reading) its 10 M yields alone would take about 1,220 s, yet it passed in under 870 s. It is NOT ROOTED.
By COORD's ruling this seat does not chase it. The latency arm therefore bounds the TIMED escalations, not the loop's
total over the escalation count.
