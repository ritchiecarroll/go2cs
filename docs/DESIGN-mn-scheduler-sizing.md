# M:N goroutine scheduling — a sizing note

> **STATUS: PROPOSED (lane R, 2026-10-24; branch `claude/r-mn-sizing`, cut from the 1.26 hop tip `9dcf0bafe1`).**
> Design only: no code lands from this note, and nothing in it is for the hop. It sizes one decision — whether, when
> and how far go2cs should move from one OS thread per live goroutine to Go's M:N model — and asks the coordinator
> for the rulings listed in §7. It builds on [the cooperative scheduler](phase4/DESIGN-cooperative-scheduler.md),
> whose §8 already records the principle question this note cannot settle by itself.

## 1. The gap, measured

Batch 31 made goroutines start on **reused** threads ([`GoroutineThreadPool`](../src/core/golib/runtime/GoroutineThreadPool.cs)).
That fixed **churn**: a program that starts goroutines one after another no longer pays a thread creation for each
(runtime's TestChanSendBarrier starts 10M of them and now finishes in under a minute). It did not change what a
**live** goroutine costs. Each one still owns a whole OS thread for as long as it lives, and `GOMAXPROCS` schedules
nothing. Go instead multiplexes every goroutine onto `GOMAXPROCS` OS threads, each goroutine on a small stack that grows.

The demonstrator (`ParkStorm`, Appendix A) parks N goroutines on one channel at the same moment, holds them parked
for five seconds, then releases them all. A harness samples the process while they are parked. Measured on R-LAPTOP
(Windows x64, 16 hardware threads, 31 GB RAM), Go 1.26.10, the converted build from the hop tip, Release, one run at a
time. The watchdog stopped any run past 20 GB of private memory or 600 s.

| Parked goroutines | Go: memory, OS threads, time to park | Converted (256 MiB reserve, today's default) | Converted (1 MiB reserve) |
|---|---|---|---|
| 10,000 | 99 MB, 10 threads, 0.07 s | 622 MB, 10,012 threads, 4.4 s | — |
| 100,000 | 869 MB, 16 threads, 0.62 s | 5,904 MB, 100,010 threads, **302 s** (release 74 s) | 5,247 MB, 100,009 threads, 65 s (release 63 s) |
| 1,000,000 | 8,544 MB, 19 threads, 7.9 s | **never parked**: stopped at 600 s with ~140,000 threads, 8.3 GB | **never parked**: stopped at 600 s with ~294,000 threads, 15.3 GB |

What the numbers say:

- **Memory per parked goroutine.** Go spends about 8.5 KB (a 2 KB stack that has not grown, plus its `g`). The
  converted program spends about **52–59 KB**, almost all of it the OS thread: a kernel stack, the thread's
  environment block, the committed top of its user stack and the CLR's per-thread data. One million parked
  goroutines would need more than 50 GB.
- **Starting them gets slower as more are alive.** 10,000 start at about 2,300 a second; 100,000 average about 330
  a second. With the 256 MiB reserve, 1,000,000 had not finished after ten minutes.
- **The stack reserve matters at scale.** At 10,000 goroutines a 1 MiB reserve and a 256 MiB one start at the same
  speed (measured 2026-10-23). At 100,000 the smaller reserve parks **4.6 times faster**. Separately, 256 MiB per
  thread across 1,000,000 threads is 256 TB of address space, twice the 128 TB a 64-bit Windows process has, so
  today's default cannot reach a million even with unlimited memory.
- **Where it fails.** Ours stays usable up to about 10<sup>4</sup> live goroutines, which is the bound the cooperative
  scheduler design accepted. It becomes impractical near 10<sup>5</sup> (minutes to start, gigabytes held) and
  cannot reach 10<sup>6</sup> on this machine. Go handles 10<sup>6</sup> in 8 seconds on the same box.

## 2. What real code hits the gap

The gap is about how many goroutines are **parked at once**. How many a program starts over its lifetime no longer
matters (§1).

- **Servers with many idle connections.** `net/http` runs one goroutine per connection, plus one per request in
  flight, and an HTTP/2 connection adds a reader and a writer. A server holding 10,000 keep-alive or WebSocket
  connections holds 10,000+ parked goroutines: about 600 MB and several seconds of start-up cost here, against
  about 100 MB in Go. At 100,000 connections, the scale chat, push and IoT gateways run at, the converted server is
  not viable.
- **Fan-out pipelines.** `errgroup`, worker-per-item patterns and `singleflight`-style deduplication start a
  goroutine per input and park most of them on a channel or a `WaitGroup`. A fan-out of 50,000 items is ordinary Go;
  it holds 50,000 threads here.
- **Timers and sleepers.** A goroutine per pending timeout (`time.Sleep`, or `select` on `time.After`) is idiomatic
  Go. Every one of them holds a thread here.
- **Stress tests.** A test that parks tens of thousands of goroutines at once to stress a lock, a channel or a
  server would show the gap directly.

The validated corpus rows pass today, runtime's included since batch 31, so whatever they park at once fits the
model. How close any of them comes to the bound has not been measured. The gap matters most to programs that **use**
go2cs, not to the corpus's own tests.

## 3. What M:N needs in golib

M:N means a parked goroutine holds **no** thread. On the CLR a thread's stack cannot be set aside and resumed later,
because there is no supported stack switching. So a goroutine that can park must be able to give up its frames
another way. This section lists what that touches. The counts are from the hop tip.

1. **Park points.** The [park inventory](phase4/DESIGN-cooperative-scheduler.md#6-the-park-inventory--every-site-todays-primitive-its-go-counterpart-its-stage)
   lists every place converted or hand-owned code blocks a thread on a Go wait. These are channel send and receive,
   `select`, the `sync` semaphore (Mutex, RWMutex, WaitGroup), `sync.Cond`, `internal/poll`'s fd mutex, `time.Sleep`,
   timer delivery, the netpoll waits and the testing gate. All of them block a thread today (`SemaphoreSlim`,
   `ManualResetEventSlim`, a waitable timer). Under M:N each must **return** to a scheduler with the goroutine's state
   saved, then resume it on whatever thread is free. The park accounting landed in S3 (`Goroutine.Park`), so every
   site already reports itself; none of them can give up its thread.
2. **Stacks.** Go goroutines start at 2 KB and grow by copying. The CLR has no growable or switchable stacks. The only
   ways for a parked goroutine to hold no stack are to **lower the function to a state machine**, as C# `async`
   does, or for the runtime to provide continuations. §4 covers both.
3. **Preemption.** Go preempts a goroutine that runs a long time, asynchronously since 1.14. With M:N on a fixed set
   of workers, a goroutine in a tight loop with no park point keeps its worker forever, and others starve. That was
   Go's behavior before 1.14. Without runtime support, preemption has to be **cooperative**: the converter inserts a
   yield check at loop back-edges in the code that runs on a worker.
4. **Thread-bound state.** Everything golib keeps per thread would have to be kept per goroutine. That is the 29
   entries GolibTests' `ThreadStateCensusTests` classifies, together with `recover()`'s panic slots, the defer
   frame's sequence entries and `getg()`'s cache. Batch 31 made each of them reset between goroutines. Under M:N one
   goroutine's frames would run on several threads in turn, so each must move into the goroutine object instead.
5. **`GoFrame`.** The defer and recover frame is a `ref struct`, emitted in 2,672 functions of the corpus. C# forbids
   a `ref struct` from living across an `await`, so a function that both defers and can park needs a different frame.
6. **`LockOSThread`.** A locked goroutine must run every one of its frames on the same OS thread. Under M:N that means
   a dedicated thread for the goroutine, as Go gives it a dedicated M. That is today's model, kept for those
   goroutines only.
7. **Native calls.** The corpus makes blocking native calls through 87 `DllImport`/`LibraryImport` sites in 28 files
   and 50 unmanaged function-pointer calls (the syscall dispatcher among them). A goroutine in a blocking native call
   holds its worker. Go handles this by handing its P to another M when a syscall blocks (`entersyscall`, plus sysmon
   retaking a P from a long call). golib would need the same: mark the worker blocked and start or wake another, so
   other goroutines keep running.
8. **Timers and netpoll.** These map well. .NET's own timers and async sockets already complete without holding a
   thread, so under M:N a sleep or an idle socket would cost nothing. The netpoll seam ([§7 of the cooperative
   scheduler](phase4/DESIGN-cooperative-scheduler.md#7-the-netpoll-boundary--which-side-owns-what)) is where that is
   wired.

**What the cooperative scheduler arc already provides.** It provides goroutine identity and a registry that does not
depend on threads (`Goroutine`, `Goroutine.Current`, ids by creation), park accounting at every site, the thread
reuse pool and its reset census (batch 31), and a scheduler-shaped `Coro` for `iter.Pull` and range-over-func. All of
these carry over. What it does **not** provide is a way for a parked goroutine to give up its thread. Its own §8
names async lowering as the only known way past the thread bound, and records it against the conversion principle.

## 4. The options

**A. Keep a thread per goroutine; raise the ceiling.** Make the default stack reserve small (1 MiB, the CLR's
default), and let the converter mark the goroutines that need more, for example those whose body reaches a recursive
call. This keeps everything that works today. Measured gain: 100,000 goroutines park 4.6 times faster, and the
address-space wall moves past 10<sup>6</sup>. Memory per goroutine stays at about 50 KB, so 10<sup>6</sup> still
needs 50 GB. *Risk:* a .NET stack overflow cannot be caught and kills the process, so an under-reserved goroutine
that recurses deeply crashes where Go would have grown its stack. The converter's recursion analysis has to be
conservative. *Ceiling:* about 10<sup>5</sup> live goroutines on a 32–64 GB machine.

**B. Selective async lowering, with goroutines multiplexed on `GOMAXPROCS` workers.** The converter computes which
functions can reach a park point, through direct calls, func values and interface methods. Only those functions are
emitted as `async`, and a goroutine whose body can park runs as a task on a golib scheduler with `GOMAXPROCS`
workers. Everything that cannot park stays exactly as it is. .NET 11's **Runtime Async** (an opt-in preview: the
runtime, not the compiler, keeps the suspended state) is the cost model to target. It lowers the per-call price of
an async method, though it does not remove the signature change
([.NET 11 runtime notes](https://learn.microsoft.com/en-us/dotnet/core/whats-new/dotnet-11/runtime)). What it
requires:

- a whole-program "can park" analysis, and a policy for dynamic calls: an interface method or func value whose
  target may park has to be called as if it might;
- an async-safe defer frame for the functions that both defer and park (§3.5);
- moving the thread-bound state into the goroutine (§3.4);
- syscall handoff (§3.7) and cooperative preemption checks (§3.3);
- `LockOSThread` goroutines on dedicated threads (§3.6).

*Cost to the output:* every parking function and its callers read `await` in the emitted C#. The cooperative
scheduler's §8 records that as a change to the conversion principle ("no implicit async"), which the coordinator
would have to amend first. *Unknown that decides it:* how much of the corpus is "can park". If parking reaches most
functions through `io.Reader`, `fmt` and `net`, the selective form collapses into full lowering.

**C. Full async lowering.** Every function becomes async. Already rejected in the cooperative scheduler's §8 for the
universal cost and the visible output. Not revisited here.

**D. Routes that are not available.**

- **OS fibers.** CoreCLR has no supported fiber mode, and its GC and stack walking assume one stack per thread.
- **Switching stacks by hand.** This is the same problem below the runtime, and it breaks GC stack walks.
- **Waiting for the runtime to provide green threads.** Microsoft's runtimelab green-threads experiment
  (dotnet/runtimelab issue #2398) concluded in favour of keeping async/await; this reading comes from a secondary
  source, since the issue itself was not reachable.

## 5. Staged plan, with estimates

Estimates are in lane-weeks (one lane, full time), and they are judgement, not measurement. Each stage ends with a
measured gate on the demonstrator.

| Stage | What | Estimate | Gate | Main risk |
|---|---|---|---|---|
| S0 | Thread per goroutine, reused after it ends (batch 31) | done | runtime row validated; churn fixed | — |
| S1 | Option A: small default reserve, converter-marked larger reserves for recursive goroutine bodies | 1–2 | 100k parked under 90 s and 6 GB; no stack-overflow regression on the corpus rows | an unmarked deep recursion now crashes |
| S2 | **Sizing spike for B, no emission change.** A census through the converter's own analysis: what share of functions and `go` bodies can park, how many dynamic calls must be treated as parking. Plus a hand-written Runtime Async prototype of ParkStorm on .NET 11 preview | 2–3 | a go / no-go number: the can-park share and the prototype's 10<sup>6</sup> memory | the can-park share comes out too high and B collapses into C |
| S3 | Option B behind an opt-in converter flag: can-park analysis, async emission for that set, golib work-stealing scheduler on `GOMAXPROCS` workers, async-safe defer frame, per-goroutine state, syscall handoff, cooperative preemption, dedicated threads for locked goroutines | 14–22 | ParkStorm 10<sup>6</sup> parks in under 60 s and 20 GB; the corpus rows equal Go with the flag on | the defer frame and `recover()` across `await` is the hardest unbuilt piece |
| S4 | Re-baseline: goldens and corpus emission under the flag, gates and docs | 4–6 | CNR and the banked rows equal Go with the flag on and off | the corpus is large, and its churn is the cost the principle was written to avoid |

The full arc is roughly **20–30 lane-weeks** after S1. S1 alone is cheap and independent.

## 6. Recommendation

1. **Do S1** after the hop. It is cheap, it keeps the model everything is validated against, and it moves the
   practical ceiling from about 10<sup>4</sup> to about 10<sup>5</sup>.
2. **Do the S2 spike** before deciding anything larger. Its one number, the share of the corpus that can park,
   decides whether selective lowering is a real option or full lowering in disguise.
3. **Do not start S3** without a coordinator amendment of the conversion principle, and without a real consumer that
   needs more than 10<sup>5</sup> live goroutines. The cooperative scheduler's §8 already sets that bar, and this note
   adds the measured gap to it.

## 7. Rulings asked

- **R1.** Accept the measured gap (§1) as the documented shape of the divergence: ours is usable to about
  10<sup>4</sup> parked goroutines and impractical near 10<sup>5</sup>.
- **R2.** Schedule S1 after the 1.26 publish.
- **R3.** Schedule the S2 spike, and rule on S3 from its numbers.
- **R4.** Confirm that S3 needs an amendment of the conversion principle before any code. That is the cooperative
  scheduler's §8, restated with these numbers.

## Appendix A — the demonstrator

`ParkStorm` parks N goroutines on one channel, holds them, then releases them. It was converted with the hop-tip
converter as a standalone `package main` and built in Release.

```go
package main

import (
	"fmt"
	"os"
	"runtime"
	"strconv"
	"sync"
	"time"
)

func main() {
	n, _ := strconv.Atoi(os.Args[1])
	hold, _ := strconv.Atoi(os.Args[2])
	release := make(chan struct{})
	var started, finished sync.WaitGroup
	started.Add(n)
	finished.Add(n)

	t0 := time.Now()

	for i := 0; i < n; i++ {
		go func() {
			started.Done()
			<-release
			finished.Done()
		}()
	}

	started.Wait()
	fmt.Printf("PARKED %d goroutines=%d in %.2fs\n", n, runtime.NumGoroutine(), time.Since(t0).Seconds())

	time.Sleep(time.Duration(hold) * time.Second) // the harness samples memory and threads here

	t1 := time.Now()
	close(release)
	finished.Wait()
	fmt.Printf("RELEASED %d in %.2fs\n", n, time.Since(t1).Seconds())
}
```

The run logged progress every tenth of N, which is how the stopped runs report how far they got; that logging is
omitted above. The harness samples private bytes, working set and OS thread count every 500 ms, takes a snapshot
once the program prints `PARKED`, and stops a run past 20 GB of private memory or 600 s. The 1 MiB rows set
`GO2CS_GOROUTINE_STACK=1MiB`.
