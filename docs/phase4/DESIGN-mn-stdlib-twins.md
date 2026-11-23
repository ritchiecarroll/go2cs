# DESIGN — M:N row 7: the standard library built and packed with twins (.5), and the gate it ships on

Row 7 of the .5 plan (COORD, 2026-11-21: M:N option (b) is .5 work, the scheduler off by default). Today a stdlib call
from an opted-in program stays plain and holds its carrier (L-min, DESIGN-mn-twin-set.md 11.3; the closure under
`GO2CS_MN=1` read peak OS threads 38-40 for eight goroutines). Row 7 builds the converted standard library with twins,
so a stdlib call that can park parks. It is binary and permanent: by O3, a package that ships twins keeps them, under
one identity and one variant. So it ships in .5 only if row 10's perf window passes the thresholds in section 4,
written here before row 10 runs. A miss stops for an owner ruling, not for tuning (COORD's condition 1 on the .6 plan).

## 1. What row 7 builds

| piece | what | lane-days |
|---|---|---|
| 1 | hosts made `partial`: the hand-owned hosts the generator refuses (`host-not-partial`: stage (c) counted 94 methods and 2 interface members on the closure), as a hand-owned corpus edit | 1 |
| 2 | sync's `Lockꞏtwin` takes the canonical receiver (`this ж<Mutex>`), with the generator's own-shape rule (TwinGenerator.BoxTwin.cs `IsBoxTwinTarget`) moved with it, or a twin-built sync gets the generator's box forwarder beside it (CS0111; FREEZE-SHEET 7b). A contended Lock parks instead of holding the carrier (7a's interim fix took the lock through Lock) | 1 |
| 3 | sync's and time's hand twins public again, as frozen surface of a twinned stdlib (7b made them internal for L-min) | 0.5 |
| 4 | R5, the hand-owned population: 127 public Go functions whose bodies are hand-written (`partial` declarations with `_impl.cs` bodies; 15 packages, mostly atomics). Each gets a forwarder from a per-package list of the Go declarations it stands for; most never park, so most are marked | 1-1.5 |
| 5 | the stdlib built and packed with twins: `-stdlib` packs every converted package with `GoTwins=true` (one variant per identity, O3) | 0.5 |
| 6 | gates: GO2CS0017 = 0 and GO2CS0018 = 0 over the whole stdlib build; CNR both ways (a consumer bound plain runs against twinned packages; a twinned consumer against them); the 4 Windows `LockOSThread` users read under the pin | 1-2 |
| | **total** | **5-7.5** |

**Gosched and N1 (COORD's ask, 2026-11-21).** `runtime.Gosched` is NOT weak under N1: it is a deliberate yield, the
only point where a spinning goroutine gives its carrier back (S2 arm 1's hang), so a caller that spins on it must
become a twin, and a weak Gosched would bring the hang back for every caller that is not a twin for another reason.
Its population is small: in the stdlib corpus one converted non-test caller (crypto/internal/fips140/check/checktest),
the rest inside the hand-owned runtime, which row 7 does not twin.

## 2. What does not change

- A consumer that does not opt in binds the plain members, as today. Twins add members; they move none (R2, the
  contract sentence).
- The scheduler stays off by default. A twinned stdlib under a program with `GO2CS_MN` unset runs its plain bodies.
- Generics stay out (E1).

## 3. Predictions (before any code)

| # | prediction | stop line |
|---|---|---|
| R7-1 | the stdlib builds with twins with GO2CS0017 = 0 and GO2CS0018 = 0 | any |
| R7-2 | R5's 127 functions each get a forwarder; at most 10 park (unmarked) | over 20 parking: a design read |
| R7-3 | the closure under `GO2CS_MN=1` with a twinned stdlib: EQUAL to Go, 3 runs, and peak OS threads fall from 38-40 to 30 or fewer | output differs: stop; threads not lower: section 4's T4 decides |
| R7-4 | CNR both ways: identical | any difference |

## 4. The gate row 10 reads (thresholds stated before row 10 runs)

Row 10 times opted-in against plain on the Performance suite, sqlite-cs and TypeScript-go-cs, JIT and Native AOT, and
measures the stdlib's size and build. Row 7 ships in .5 only if ALL of these hold. Every time threshold is beyond the
A/A band, which row 10 measures first (an A/A pair per leg, as glyph kit (b) did).

| # | threshold | why this line |
|---|---|---|
| T1 | SIZE: the stdlib packages' assemblies, summed, with twins are at most **+8%** over the same tree plain | on ONE tree, stage (c) read +5.4% to +6.1% (arms A, B vs P) and 7a's forwarders add +0.5% (B7 vs B0): about +6.6% expected. The owner accepted "+5-6% bytes per assembly" (O3, 2026-11-20); +8% leaves headroom without passing a surprise. (A first draft said +10% from a CROSS-tree +8%, 7a's S against stage (c)'s P, which mixes two converters; amended before row 10, 2026-11-21) |
| T2 | BUILD: the `-stdlib` corpus build, clean and uncontended, is at most **1.8x** plain | stage (c) read 1.5-1.8x on the closure |
| T3 | A consumer that does NOT opt in, built against the twinned stdlib: every Performance-suite row, sqlite-cs and TypeScript-go-cs within **+3%** of the plain stdlib (beyond A/A), JIT and AOT; its AOT executable at most **+1%** larger | the twins are there for everyone (O3), so a program that never asked must not pay. ILC drops unreachable twins, so AOT should read near zero |
| T4 | An opted-in program, `GO2CS_MN=1`: the closure's peak OS threads at least **25% lower** than the plain stdlib's 38-40, and its output EQUAL to Go | this is what row 7 buys. If threads do not fall, a twinned stdlib costs everyone T1-T3 for nothing |
| T5 | An opted-in program with the scheduler OFF: within **+5%** of plain on the suite rows it covers, JIT and AOT | the default (scheduler off) must not punish opting in |

REPORTED, not gated: the opted-in programs with the scheduler ON against Go and against plain (the park step and the
carrier counts), since the default-on question is (c), not .5.

A miss on any of T1-T5: row 7 does not ship in .5. The owner rules on what does: "stdlib plain, holds a carrier"
stays the shipping text (P4 holds both texts until then).

## 5. Order

1 and 2 (the corpus edit and sync), then 3 and 4, then 5, then 6's gates; row 10 reads section 4 last. Row 6 (the
park-step cut) lands first, so row 10 times the cut.

## 6. Pieces 2 and 3 delivered first (dated block, 2026-11-22, G)

COORD moved pieces 2 and 3 ahead of the rest of row 7 as a .5 RELEASE GATE (docs/phase4/inbox/G/20261122T110220Z-COORD.md),
after a hang in the twinned MnMixedStorm at n=100,000 (about 1 run in 10 on the train-3 golib) was captured on the train-4
tip 1f2a9afdfc. The capture, from a dotnet-stack report of the hung process (the 2.2 GB heap dump is kept locally until
this row and the convoy row are verified, then deleted):

- 46,223 OS threads: 25,409 blocked in plain sync.Mutex.Lock (the twin forwarded to Lock), and 19,228 back from plain
  time.Sleep, queued in GoScheduler.ExitBlocked on the ONE P semaphore s_p (12,027 waiting, 7,201 spinning on its
  monitor); 27 idle carriers; 114% of one core. A convoy at s_p, reached because both stdlib calls blocked threads.
- Injections were not the source (231-372 per run while peak carriers reached 15,692).

**The surface (COORD's freeze ruling, docs/phase4/inbox/G/20261122T114603Z-COORD.md).** Two members are frozen from .5:

- go.sync `[EditorBrowsable(Never)] public static GoTask Lockꞏtwin(this ж<Mutex> Ꮡm)`, replacing the internal
  `Lockꞏtwin(this ref Mutex)` forwarder. It parks: the CAS fast path, then Lock's own protocol on the same gate, with the
  gate's WaitAsync completing a GoParkSlot. With the scheduler off, or the block or mutex profile on, it takes plain Lock.
  With no own-shape twin beside it, the generator's IsBoxTwinTarget routes a twinned caller's `s.mu.Lock()` to the box.
- go.time `[EditorBrowsable(Never)] public static GoTask Sleepꞏtwin(Duration d)`, internal to public, body unchanged.

GolibTests' StdlibTwinSurfaceTests guards both signatures, read off the built assemblies; TwinMethodSetTests covers the
box-receiver shape (IsTwinMember keeps both out of every Go method set).

**Readings, twinned, GO2CS_MN=1 (windows, 12 cores, P=12), train-4 tip before and after:**

| program | before | after |
|---|---|---|
| MnLockSleep (n=2,000: Sleep, then one contended Mutex) | main untwinned: nothing could park, runs on threads | about 30 peak OS threads, about 5,994 parks, EQUAL to Go |
| MnMixedStorm n=10,000 | 542-561 peak OS threads, 20,000 parks | 37-42 peak OS threads, about 30,100 parks |
| MnMixedStorm n=100,000 | 1,000-46,000 threads, 7-76 s, about 1 hang in 10 | 45-52 peak OS threads, 3.2-6.6 s (3 runs, before the 50x2 gate) |
| MnLockFair (n=4,000: half park on the field, half block through a sync.Locker) | - | every goroutine acquires, EQUAL to Go, 0.73 s; peak OS threads 141-153 (the blocked half) |

Not in these pieces (stated): a Mutex the caller cannot name a box for stays a plain call; sync.Locker interface calls
keep the plain path; a twin-built sync (piece 5) must not emit the generator's box forwarder beside this hand twin
(CS0111). Go's mutex starvation mode is not modelled: the gate serves waiters in SemaphoreSlim's order.

**Review folds (dated block, 2026-11-22, G; the review was CLEAR with no MUST).**

- Both twins decide by the CALLING THREAD, `GoScheduler.OnCarrier` (made public, one line; hidden tier 2, read only by
  go.sync and go.time), not by GO2CS_MN: off a carrier they take plain Lock and plain Sleep. Deciding by
  GoScheduler.Enabled, a twin off a carrier with MN=1 queued a waiter on the gate and then threw in ParkPending, leaving
  that waiter for a later Unlock to wake. GolibTests OffACarrierTheStdlibTwinsTakeTheirPlainPath holds it.
- The wake cost, stated beside Lockꞏtwin: SemaphoreSlim completes an async waiter's task with
  RunContinuationsAsynchronously, so a parked waiter is woken through one ThreadPool hop. MnLockWake (2,000 hand-offs,
  windows, P=12): parked (twinned, MN=1) p50 17-19 us, p90 31-32 us, p99 55-58 us, max 2.9-5.3 ms; the plain blocking
  hand-off (MN unset) p50 13 us, p90 24-28 us, p99 38-44 us, max about 3 ms. The hop costs about 5 us at the median and
  15 us at p99. (Go's own figure on windows is below its clock's resolution there.)
- StdlibTwinSurfaceTests also holds that no other public twin ships in go.sync or go.time. The slow path is
  `lockSlowAsync`, with the park scope inside the try and `leave` in its own finally.

## 7. Pieces 1, 4, 5 and 6, and the ref-blocked calls (dated block, 2026-11-23, G)

The pieces after 2 and 3 are recorded in their own DATA files:
- piece 1, the hosts made `partial` and GO2CS0021: [DATA-mn-row7-hosts.md](DATA-mn-row7-hosts.md);
- pieces 4, 5 and 6 (R5 by its declaration's file, `GoStdlibTwins`, the whole standard library twinned on three
  targets, and the rewriter cures it needed): [DATA-mn-row7-r5.md](DATA-mn-row7-r5.md);
- the ref-blocked calls (the Go-order move and inline re-derivation): [DATA-mn-row7-refblock.md](DATA-mn-row7-refblock.md).

**The flip is row 10's.** `GoStdlibTwins` (src/core/Directory.Build.props) is OFF by default, and the repoguard
TestStdlibTwinsDefaultOff holds it off. It turns on by default, and the `go.*` packages are packed with twins, only on
row 10's ruling against section 4's T1-T5 (the owner's 2026-11-21 ruling). Until then a twinned standard library is
built only by a gate or row 10 itself, with `-p:GoStdlibTwins=true`.
