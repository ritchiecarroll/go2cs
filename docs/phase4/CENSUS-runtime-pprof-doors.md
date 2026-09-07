# CENSUS — `runtime/pprof`, every row billed by door

**Point-in-time record.** Measured 2026-09-04 by SUB-Q43 on the coordinator box (i7 class), at master
`db9e95841` plus this branch's one committed file. Amended with dated blocks, never rewritten, never
executed from.

- **Oracle**: `go test -json -count=1` on the pinned toolchain, bare `go version` = `go1.23.12
  windows/amd64`. **183 verdict rows**, 181 pass + 2 skip, package `ok` in **75.067 s**.
- **Converted side**: the PUBLISHED single-file test host, **Release with
  `DOTNET_TieredCompilation=0`**, `.NET 10.0.11`, `timezone=UTC` — the configuration of record.
- **Instrument**: an accumulating skip-list probe driven straight at the published host, plus one
  isolation arm per candidate (`--run '^<name>$'`), plus one one-variable A/B. Every reading below is
  from a captured stream or from the pipeline's own comparison record, never from a summary line.

---

## 1. The headline

| | before | after |
|:--|--:|--:|
| C# verdicts produced | **2** (SUB-Q27's ungated run, 1,438 s) | **147** |
| rows agreeing with Go | 0 | **120** |
| host outcome | died on an unrecovered goroutine panic at alphabetical position 2 | ran to the end, **no death, no `timeout` event** |
| wall (host alone) | ~24 min, i.e. the package deadline | **48 s** |

The cut that produces it is **seven `host-fatal` disclosure entries** in a new, hand-owned
`src/core/runtime/pprof/go2cs_test_disclosures.json`. No converter, `golib`, `gen` or
`src/core/testing` change.

---

## 2. Why the host-killer is withdrawn rather than captured

Q43 was dispatched to ask whether SUB-Q23's finalizer runner generalises into a per-test
goroutine-panic capture. **It does not, and no such capture is admissible.** Three independent
statements, two of them already measured in this tree before Q43 ran:

1. **`golib` refuses it at the seam.** `Goroutine.ContainUnhandledExceptions`' own contract: a
   `PanicException` "is NEVER offered to the policy: a panic crossing a goroutine root keeps its
   Go-faithful fatal path even under a host, because that is what Go does and what the differential
   oracle must observe."
2. **`core/testing/TestHost.cs` already REVERSED containment, on evidence.** Containing an escaping
   goroutine exception cannot unblock whatever the dead goroutine was going to signal. `reflect`'s
   `TestOffsetLock` (2026-08-30) is the measured instance: contained, four sub-second failures
   presented as an unbounded hang that ate a 40-minute deadline and truncated the suite.
3. **This row is exactly that shape.** `awaitBlockedGoroutine` (pprof_test.go:1023) is an infinite
   `for { runtime.Gosched(); runtime.Stack(buf, true) }`; the panic comes from a *third* goroutine,
   the `time.AfterFunc` timer Go arms at `t.Deadline()-1s`. The polling loop keeps spinning whatever
   is done with the timer's panic. Containment would convert a bounded ~2-minute crash into an
   unbounded hang and a package-deadline kill.

And SUB-Q23's `ObserveUnhandledPanic` already does the half that *is* possible: **the row gets a real
`fail` verdict carrying the panic text before the process dies.** So a verdict was never the missing
thing — survival was.

The mechanism used is therefore the one the tree already ruled for this exact failure:
`hostFatalClass` (`testConversion.go:6385`, coordinator ruling 2026-09-02, previously a class of one).
**Correction to the dispatch's wording, stated rather than worked around:** `host-fatal` takes **no
signature** by its own design and the loader pins that (`disclosure entries require a signature except
for host-fatal`) — a withdrawn test produces no verdict on either side for a signature to match. The
panic text lives in the `reason`, which is where it stays auditable. No signature was invented.

---

## 3. The withdrawn set, each entry earned by its own isolation arm

Derived, not eyeballed: reverse-reachability over `awaitBlockedGoroutine` computed with `go/packages`
under the same conservative *any use of a same-package function* rule the converter's own capability
analysis uses. Every member was then run **alone**.

| test | isolation arm (`--run '^name$' -timeout 2m`) | rows |
|:--|:--|--:|
| `TestBlockMutexProfileInlineExpansion` | exit 2, 123 s, attributed `fail` at pprof_test.go:2784 | 3 |
| `TestBlockProfile` | exit 2, 122 s, attributed `fail` at pprof_test.go:816 | 3 |
| `TestMutexProfile` | exit 2, 125 s, attributed `fail` at pprof_test.go:1232 | 4 |
| `TestMutexProfileRateAdjust` | exit 2, 122 s, attributed `fail` at pprof_test.go:1351 | 1 |
| `TestProfileRecordNullPadding` | exit 2, 123 s, attributed `fail` at pprof_test.go:2838 | 6 |
| `TestProfilerStackDepth` | exit 2, 122 s, attributed `fail` at pprof_test.go:2544, **two** package-level death events | 5 |
| `TestGoroutineProfileLabelRace` | **exit 1, 182 s, ZERO verdicts, package deadline consumed whole** (`-timeout 3m`) | 3 |

Six of one shape and one of another; **25 rows** in total.

**The shared mechanism of the six.** `awaitBlockedGoroutine` waits for a *foreign goroutine's own
interior stack frames* — `(?m)^goroutine \d+ \[<state>\]:\n(?:.+\n\t.+\n)*runtime/pprof\.<fName>`.
The CLR offers no way to walk another thread's stack, and the converted `runtime.Stack(all)` says so
rather than hiding it: a foreign goroutine renders as header + `ForeignStackPlaceholder` + `created
by`. The regexp can never match on any input; the capability is absent **by construction**, not
unimplemented, and no increment of `runtime/pprof` can supply it.

**The seventh is different and is flagged for a ruling.** `TestGoroutineProfileLabelRace/reset` loops
until the goroutine profile's text contains the label `loop-i`. The converted goroutine profile
deliberately **withholds labels** — SUB-Q27's measured pointer-staleness defect, where a `labelMap`
address minted through `unsafe.Pointer.FromPinnedBox` went stale across a collection, read
`len == 1885431144`, and killed the host with `OutOfMemoryException` inside `printCountProfile`. With
no labels the substring never appears, `cancel()` is never called, and neither loop terminates. It is
placed in `host-fatal` rather than in the converter's `unsupportedRuntimeCapabilities` gate **on that
gate's own bar** — "add an entry ONLY for something provably unavailable, never for something merely
unimplemented" — and the label half is unimplemented, not unavailable. It is the class's first HANG
member, and it **retires the day the goroutine profile carries labels**.

---

## 4. The census, by row

183 Go rows. 25 withdrawn above; 1 already withdrawn by the converter's capability gate
(`runtime/pprof.TestFakeMapping`). **157 rows compared.**

| door | rows | note |
|:--|--:|:--|
| **matched** | **120** | 118 `pass`/`pass` + 2 `skip`/`skip` |
| **divergence** | 25 | a verdict on both sides, different |
| **stub** | 2 | a `NotImplementedException` naming a symbol |
| **unreached behind a skip** | 10 | `TestTryAdd`'s subtests, none of which runs because the parent takes Go's OWN skip path |
| *withdrawn (host-fatal)* | *25* | *§3 — counted in DISCLOSED, not in the 157* |
| *gated (capability registry)* | *1* | *`TestFakeMapping`, pre-existing* |

Read from the pipeline's own comparison record: `go` 157 rows, `csharp` 147 rows, **120 agreeing**,
`disclosed` 7, `gated` 1, `errors` 38 (37 row mismatches + one package-level `exit status 1`),
`environment` `{configuration: Release, tiered: false, oracleGoVersion: go version go1.23.12
windows/amd64}`. **157 = 120 + 37**, and **183 = 157 + 25 + 1**. The arithmetic closes.

⚠ **The matched count is dominated by ONE declaration and a bank must say so.**
`TestGoroutineProfileConcurrency` alone carries **104 of the 120** matched rows (its `goroutine
launches` table is 100 subtests). By DECLARATION the picture is 44 top-level tests, of which 14 carry
matched rows. Both numbers are honest; quoting only the first would not be.

---

## 5. The census, by door and mechanism — every symbol named

### 5.1 CPU-profile class — **13 rows, ONE root, and the root is `getg`**

`TestAtomicLoadStore64`, `TestCPUProfile`, `TestCPUProfileLabel`, `TestCPUProfileMultithreaded`,
`TestCPUProfileRecursion`, `TestCPUProfileWithFork`, `TestGoroutineSwitch`, `TestLabelRace`,
`TestLabelSystemstack`, `TestMathBigDivide`, `TestMorestack`, `TestTimeVDSO`, `TestTracebackAll`.

The first of them alphabetically reports
`infrastructure-error: System.NotImplementedException: getg: external (assembly or cgo) function is
not implemented`, reached through `StartCPUProfile → runtime.SetCPUProfileRate → setcpuprofilerate →
getg`. The other **twelve** report `cpu profiling already in use` — **not twelve defects, one leak.**
Go's own `StartCPUProfile` sets `cpu.profiling = true` *before* calling `runtime.SetCPUProfileRate`,
so the throw leaves the flag set and every later CPU test is refused by Go's own double-check.

**A/B, one variable, measured:** skipping only `TestAtomicLoadStore64` moved **exactly one row** —
`TestCPUProfile` changed from `fail: cpu profiling already in use` to
`infrastructure-error: getg` — and the remaining eleven were byte-identical. Which test takes the
throw is decided by alphabetical order alone; the class is one wall.

This confirms SUB-Q27's reading and sharpens it: **the CPU-profile class's first wall is `getg`, not
SIGPROF sampling**, and it is worth 13 rows, not 1. (Q40 designs a managed `getg`;
`proflabel_impl.cs`'s header argues the stub must stay a loud throw, 574 sites.)

### 5.2 Memory-profile class — 6 rows

`TestMemoryProfiler` + `/debug=1` + `/proto` (3), `TestGenericsHashKeyInPprofBuilder` (1),
`TestGenericsInlineLocations` (1), and `TestFakeMapping` (1, already capability-gated). The converted
`pprof_memProfileInternal` is an honest zero-record reader — `heap profile: 0: 0 [0: 1]` — so every
assertion about a sample's stack fails with an EMPTY `got`. Same root as the existing `TestFakeMapping`
gate entry, and the two `TestMemoryProfiler` leaves are the Option-B disclosed rows that entry's own
comment already names.

### 5.3 Block/mutex-profile class — 4 rows

`TestBlockProfileBias` reports
`infrastructure-error: System.NotImplementedException: blockevent: external (assembly or cgo) function
is not implemented` — the class's named stub. `TestMutexBlockFullAggregation` fails on
`did not see any samples in mutex profile for this test` / `... block profile ...` while **both its
subtests pass** (2 of its 3 rows are in the matched column): the aggregation logic is right and the
sample source is empty.

### 5.4 Inlining-determination — 12 rows

`TestCPUProfileInlining` and `TestTryAdd` take **Go's own skip path** on the converted side
(`Can't determine whether inlinedCallee was inlined into inlinedCaller.` /
`Can't determine whether anything was inlined into inlinedCallerDump.`), where Go passes. `TestTryAdd`'s
10 subtests then never run — one skip, ten absences, not eleven findings.

### 5.5 Proto-conversion, fixture-driven — 4 rows

`TestConvertCPUProfile` (1) and `TestConvertMemProfile` + `/allocs` + `/heap` (3). These do **not**
touch the live profiler: they convert a synthetic fixture. `TestConvertCPUProfile`'s `got` carries
`"Mapping": null` where `want` has a populated `Mapping` — a mapping-attribution difference in the
converter of records, and the most obviously *implementable* bucket in this census.

### 5.6 Goroutine-profile LABEL half — 1 row

`TestGoroutineCounts`: `expected sorted goroutine counts with Labels:` — SUB-Q27's withheld label
half, the same root as the withdrawn `TestGoroutineProfileLabelRace`.

---

## 6. Predictions, scored

Posted to the mailbox before the isolation probe's five remaining arms were read.

| | prediction | outcome |
|:--|:--|:--|
| P1 | the host-fatal set is 6 entries / 22 rows; a member dying at a stub first must not be minted | **CORRECT** — 6 of 6 killed in isolation, 22 rows |
| P2 | ONE further killer of a different shape stands behind them (range 0–2) | **CORRECT** — exactly one, and genuinely different: a hang, not a crash |
| P3 | ~161 rows billed; matched ≈ 35, stub ≈ 60, capability ≈ 25, divergence ≈ 40 | **WRONG on every bucket, and wrong in the good direction.** 157 rows; matched **120** (predicted 35), stub **2** (predicted 60), divergence **25** (predicted 40). I priced the package as mostly-stub and it is mostly-matched; the stub estimate was the worst, off by 30x, because I assumed every profile family would throw where in fact only two symbols (`getg`, `blockevent`) do and the rest return honest empty profiles |
| P4 | the cut is one new committed file; CNR not owed | **CORRECT** — one file, no converter/golib/gen/testing change, tree otherwise clean |

The P3 miss is the census's own justification: nobody could have priced this package from its names,
which is exactly the phantom-divergence shape the doctrine forbids.

---

## 7. What this census does NOT claim

- **It is not a bank.** No proof page, no roster row. The disclosed set is what the bank item needs and
  it is now measured: 7 host-fatal + 1 capability-gated + 37 mismatches against 120 agreeing rows.
- **Vacuous passes are not audited.** 104 of the 120 matched rows come from one declaration; a bank must
  ask of each matched row whether the converted side could have failed it, the bar `internal/abi`'s
  `TestFuncPC` set. That audit is not attempted here.
- **`net/http/pprof` inherits this order** — it stands behind `runtime/pprof` and cannot be censused
  before it.

## 8. Open, and routed rather than decided

1. **Ratify or withdraw the seventh entry.** `TestGoroutineProfileLabelRace` is the `host-fatal`
   class's first HANG. It is the one entry that would be deleted on a word, at the cost of the package
   becoming unmeasurable again (36 rows without a C# verdict, a 20-minute deadline burned).
2. **`getg` is worth 13 rows here**, not the 1 the earlier reading implied — a datum for whoever
   prices Q40's managed `getg`.
3. **The `StartCPUProfile` flag leak is Go's own ordering**, so nothing in the converted corpus can be
   blamed for it and nothing should be patched around it; it simply means the CPU class's 13 rows all
   lift or none do.

---

## AMENDMENT — 2026-09-07, lane G, master `5a27a8972` (G-LAPTOP, Ryzen 5 PRO 6650U)

**Committed under the owner's 2026-09-07 course correction**, whose second pre-pin gate is *commit the
measured-but-unbanked baselines* — H10 grants no carry-forward path, so a reading that lives only in a
scratchpad is destroyed by the hop. This block is that reading. **Measured, not banked; the row is
unbanked and this changes no roster figure.**

**Configuration of record**: Release, `DOTNET_TieredCompilation=0`, `.NET 10.0.400`, oracle
`go version go1.23.12 windows/amd64`, `CGO_ENABLED=0`, pins verified before the run and aborting on
mismatch. Pipeline invoked DIRECTLY (`-test-action all`) because the row is unbanked and a
roster-walking sweep refuses it. Record freshness checked: results file 191 ms OLDER than the
comparison — correct single-run ordering — no timeout event in plain OR escaped form, no `testFilter`
key, so the record is ungated.

| | rows | detail |
|:--|--:|:--|
| go | **157** | 155 pass · 2 skip |
| csharp | **147** | 118 pass · 23 fail · 4 skip · 2 infrastructure-error |
| errors array | 38 | |

**i9 reproduced 157 / 147 independently on different hardware the same day**, name for name.

### The 10 go rows carrying no C# verdict are ONE cause

Every one is a `TestTryAdd` subtest. **The C# side skips the PARENT** on Go's own guard — *"Can't
determine whether anything was inlined into inlinedCallerDump"* — and a skipped parent runs no
children. Under ruling #1 that is a **FEATURE GAP, not a disclosure** (the skip reason is our own
missing capability), and it costs **11 verdicts**, not the 1 a skip-count suggests, because the ten
are ABSENT rows rather than skipped ones.

⚠ The 7 host-fatal disclosures are in **NEITHER** map — unix-only tests on a Windows record — so they
were never candidates for a go-minus-csharp difference. All seven were minted **UNCHECKED**, the
converter stating that each *"is named by no committed proof page, so its exclusion was NOT cleared
against any platform"*. **That check can never clear this class on this platform.**

### The 23 failures are NOT 23 roots, and the shared message is a MASK

**12 of 23 carry the identical text `cpu profiling already in use`.** Run SOLO and gated,
`TestMorestack` never mentions cpu profiling at all:

```
System.NotImplementedException: asmcgocall: no implementation reached this
compilation (assembly, cgo, or a linkname whose push did not arrive)
```

An earlier test leaks the profiling flag; `StartCPUProfile` refuses at its precondition and the row
dies **before reaching the stub that actually blocks it**. **Clearing the leak REVEALS deeper
blockers rather than recovering rows** — for this row, onto the assembly/linkname frontier rather
than the profiler one. A shared failure message is evidence of a shared PRECONDITION, never of a
shared root: the precondition fails last, so it is the loudest and least informative signal.
**Established for one row only; the other eleven each need the same solo treatment (~2 min apiece)
and I did not generalise.**

Remaining 11 by family: memory profiler 6, convert 1, generics 2, goroutine counts 1, mutex
aggregation 1.

`gated` holds **`TestFakeMapping`**, whose own capability text calls it a **vacuous pass** — the
mapping/symbolization loop runs over an empty location set.

### ⚠ UNRECONCILED against this record's own 2026-09-04 block

That block reads **183 oracle verdict rows** (181 pass + 2 skip) on the i7; this one reads **157** go
rows (155 + 2). **I did not resolve the difference and am not asserting a cause.** Candidates not
tested: host-conditional tests between two different Windows boxes, or the two figures counting
different things (a verdict-row stream against comparison-record entries). Both blocks state their
box and their instrument, which is what lets a later reader settle it; **a number reconciled by
argument rather than measurement would be worse than one left open.**
