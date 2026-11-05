# DATA — the DeferPanic row's first readings

> **State: MEASURED (2026-11-05), on a linux cloud box, not the table's host.** These are the first readings of the
> Performance suite's new row, `PerfDeferPanic`, taken when it joined the suite. The published table
> ([`../../src/tests/Performance/README.md`](../../src/tests/Performance/README.md), measured on G-LAPTOP against
> `go.*` 1.26.10.2) is unchanged, and the row has no cell in it: this box is a different machine class, and
> ratios from different hardware are never mixed into that table. Sections are **appended and superseded, never
> overwritten.**

## 1. Why the row exists

C1's D1 seat (Goexit from `main`) changed golib's defer and panic paths, and the suite had no row that runs either,
so its cost A/B ran outside the suite: one converted program, golib base against change, 9 alternating runs each,
JIT only. It read defer 43.18 -> 43.72 ns/op and panic+recover 3,424 -> 3,466 ns/op (medians), both inside each
arm's own spread. COORD's seat brief asked for a suite row that a later change to those paths can be measured by.

## 2. The row

`src/tests/Performance/PerfDeferPanic/PerfDeferPanic.go`: one program, three loops, each timed on its own and
printed as a **named part** beside the row's total.

| part | shape | calls |
|:--|:--|--:|
| `defer_literal` | a deferred func literal that writes the function's named result | 5,000,000 |
| `defer_method` | a deferred method value, `add := c.add; defer add(i & 3)` | 5,000,000 |
| `panic_recover` | `panic(errBoom)` recovered by a deferred func literal that calls `recover()` | 200,000 |

The program prints `elapsed_ns.<part>: <n>` for each part and `elapsed_ns: <n>` for the total. The runner change
in the same seat (`PerformanceRunner/TimingLines.cs`) makes Verify drop the part lines as it drops the total, and
the report write a table of named parts under the execution-time table. The total is still the row's time. Rows
that time no part are unchanged.

The converted shapes, read from `PerfDeferPanic.cs`: each function is a `try`/`catch`/`finally` around a `GoFrame`.
The literal is a lambda capturing `r` and `i`, so each call allocates a closure. The method value is a new lambda
over the receiver's box, copied once more before the `defer`. The recover body runs `recover()` and compares it
with `errBoom` through `AreEqual`.

## 3. Host and toolchain

| | |
|:--|:--|
| host | a linux cloud container: Intel Xeon @ 2.80GHz, 4 cores, 15 GB, no swap; Ubuntu 24.04.5 |
| Go | go1.26.10, `linux/amd64` |
| .NET | SDK 10.0.403, runtime 10.0.14; RID `linux-x64` for the C# columns |
| packages | the in-tree `src/core`, linux flavour (`GoTargetOS=linux`), by project reference: the suite's in-tree mode, not the published `go.*` packages |
| runner | `run-performance.ps1 --filter DeferPanic`, branch `claude/p2-perf-defer-panic` on master `4dc48b62e2` |
| AOT image | `PerfDeferPanic`, 148,704,920 bytes, sha256 `4f6251208fd685ea…`; publish 2,354 s on this box |
| sources | `PerfDeferPanic.go` sha256 `8028a1ec3007bc61…`, `PerfDeferPanic.cs` sha256 `9c400d56c014bc12…` |

## 4. Readings

Median of 5 runs, 1 discarded warmup, milliseconds, ratios to the Go column timed in the same run.

**Run A, JIT only** (`--no-aot`):

| | Go | C# (JIT) |
|:--|--:|--:|
| total | 112.1 | 1,844.2 (16.45×) |
| `defer_literal` | 25.7 | 426.9 (16.62×) |
| `defer_method` | 31.9 | 574.4 (18.00×) |
| `panic_recover` | 55.2 | 842.8 (15.27×) |
| peak working set (MB) | 2.2 | 100.7 |

**Run B, all three columns:**

| | Go | C# (JIT) | C# (Native AOT) |
|:--|--:|--:|--:|
| total | 116.4 | 1,616.1 (13.89×) | 1,250.3 (10.74×) |
| `defer_literal` | 29.5 | 333.5 (11.32×) | 336.9 (11.44×) |
| `defer_method` | 32.7 | 514.1 (15.72×) | 530.1 (16.21×) |
| `panic_recover` | 53.7 | 766.6 (14.27×) | 327.2 (6.09×) |
| peak working set (MB) | 2.3 | 100.8 | 215.3 |

Per call, from run B: Go about 5.9, 6.5 and 268 ns; C# JIT about 67, 103 and 3,833 ns; Native AOT about 67, 106
and 1,636 ns.

**Notes on these readings:**
- **Run A's JIT total is 14% above run B's** (1,844.2 against 1,616.1 ms), run A's defer parts being the slower. Run B's
  JIT leg ran after a 39-minute AOT publish, run A's after a fresh build. Neither is a baseline for the other; a
  change's A/B on this row runs its two arms alternating in one window, as C1's did.
- **Native AOT is faster than the JIT on `panic_recover`** (327.2 against 766.6 ms) and level with it on both defer
  parts. Not investigated here.
- **The AOT peak working set is twice the JIT's** (215.3 against 100.8 MB). Not investigated here.
- **The defer parts cost more per call than C1's program read** (67 to 106 ns against 43 ns). These shapes allocate
  a closure or a lambda per call; C1's shape is not recorded in its post.

## 5. The prediction, scored

Stated before the first C# run (Go had been read: 6 to 7, 6.5 and 275 ns per call):

| line | predicted | read | |
|:--|:--|:--|:--|
| JIT `defer_literal` | 25 to 60 ns/op | 85 (A), 67 (B) | MISSED |
| JIT `defer_method` | 30 to 80 ns/op | 115 (A), 103 (B) | MISSED |
| JIT `panic_recover` | 2 to 6 µs/op | 4.2 (A), 3.8 (B) | MET |
| JIT total | 0.7 to 1.9 s, 6 to 15× Go | 1.84 s and 16.45× (A); 1.62 s and 13.89× (B) | MET in seconds; ratio MISSED in A, MET in B |
| AOT defer parts | 0.7 to 1.5× the JIT | 1.01× and 1.03× | MET |
| AOT `panic_recover` | 1.2 to 3× the JIT | 0.43× | MISSED, in the other direction |
| AOT total | 1.0 to 4.3 s | 1.25 s | MET |
| `panic_recover` the largest part | in both C# columns | JIT yes; AOT no (`defer_method` is) | MET for JIT, MISSED for AOT |
| each defer part under a third of the total | in both C# columns | JIT yes (21 to 32%); AOT no (`defer_method` 42%) | MET for JIT, MISSED for AOT |

The stated falsifier, "a defer part above 100 ns/op under the JIT means the shapes do not measure what C1 measured",
**fired** on `defer_method` in both runs. What it says is in section 4's last note: these shapes allocate per call.
The row keeps them, because they are the brief's shapes and the ordinary way Go code defers.

## 6. Gates

- `PerformanceRunnerTests`: 61 of 62 pass. `TimingLinesTests` is 5 of 5; it failed 2 of 5 before the runner
  change. The one failure, `HeadlineGuardTests.ABuildFileOutsideTheListedChainIsRefused`, fails identically at
  master on this box. It spells its project directory as a Windows path, `C:\go2cs\src\tests\Performance\PerfFib`,
  and on linux `Path.GetDirectoryName` does not split at a backslash, so the walk up from the project never reaches
  `C:\go2cs`, where the test plants its stray file. It is not this seat's.
- The row under the runner: Transpile, Build (Go, JIT, AOT), Verify and Measure all pass, in both runs.
