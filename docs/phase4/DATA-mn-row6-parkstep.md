# DATA — M:N row 6: the park step's delegate cut, as measured (2026-11-21)

Row 6 of the .5 plan: the THIN park step (764-805 ns Native AOT against the 900 ns line, FREEZE-SHEET.md). Branch
`claude/g-mn-row6-parkstep`: `6946237bc6` (MnCost's `ping` mode, the instrument) and `b9591d7f1e` (the cut). COORD
accepted the reading on 2026-11-21; the misses stay scored AS WORDED.

## The cut

A leaf park's twin (a channel operation, as a rule) is a fresh box on every call, and its await on the `GoParkSlot`
went through `GoTaskCore.UnsafeSuspend -> awaiter.UnsafeOnCompleted(box.ResumeAction)`. `ResumeAction` is cached per
box, so every leaf park paid one `Action`. The park path now carries the box itself: the slot's state, the carrier's
pending continuation, the run-queue item, the run-next step and the pin's delivery are `object` (an `Action` or a
parked twin's `GoTaskCore`), and RunStep resumes a box as `ResumeAsStep` would.

## The instrument

The sheet's park step was half a `ping` round trip on MnStormTwin, the hand-twin fixture, which no longer builds
(7b made its hand twins internal). MnCost gained `MN_COST_MODE=ping` on generated twins: a value bounced over two
unbuffered channels, two parks per round trip (read: 1,999,970 parks per 1,000,000 round trips).

Made to fail first: dotnet-trace's GC allocation ticks name each sampled type. `System.Action` read 1,207 of 19,549
samples on BASE and 0 of 18,218 on the CUT.

## The reading of record (run 2)

MnCost built with twins, `GO2CS_MN=1`, 4 processes per arm per flavour in the order base cut cut base, medians of
reps 1-4. Run 2, 17:54-18:45, free 31-32 GB, nothing else on the box. Run 1 (17:26-17:53) is VOID and used for
nothing: its Native AOT link failed before any timed round (`vswhere.exe` was not on the script's PATH).

| mode | JIT base -> cut | AOT base -> cut |
|---|---|---|
| ping ns per round trip | 1088.6 -> 1094.4 (+0.53%) | 1510.1 -> 1455.2 (-3.64%) |
| ping bytes per round trip | 816.0 -> 688.1 (-128.0) | 784.0 -> 688.0 (-96.0) |
| call ns | 5.13 -> 4.27 (-16.9%, see P6-4) | 7.92 -> 7.86 |
| mutex ns | 9.25 -> 9.19 | 12.18 -> 12.36 |
| defer ns | 31.9 -> 31.6 | 44.45 -> 44.38 |

Native AOT executable: 132,838,912 -> 132,839,936 bytes (+1,024).

## Predictions, scored as worded

| # | prediction | reading | verdict |
|---|---|---|---|
| P6-1 | bytes per round trip fall by 128 +-8, JIT and AOT | JIT -128.0; AOT -96.0 | JIT MET; AOT MISSED AS WORDED: a Native AOT delegate is 48 B, not 64, and 2 x 48 = 96 |
| P6-2 | ns per round trip fall 2-8%, JIT and AOT | JIT +0.53%; AOT -3.64% | AOT MET; JIT MISSED (no time gain), inside the +2% revert line |
| P6-3 | the AOT park step clears 900 ns by >= 110 ns | 755 -> 728 ns: 172 ns under | MET (base on this instrument already read 755) |
| P6-4 | controls within +-3% | 5 of 6 within +-1.5% | JIT call -16.9% MISSED: a FINDING, not a gain (below) |
| P6-5 | no behaviour change: GolibTests R/D, GenTests, the Q1 kit, closure arm S | all green (section below) | MET |

**P6-4, the JIT call control.** The `call` mode never parks, yet every cut process read faster (cut 3.95-4.73 ns,
base 4.85-5.51 ns), and a re-run alone reproduced it. With `DOTNET_TieredPGO=0` both arms read 7.7-7.9 ns, identical,
and Native AOT is identical too. It is a tiered-PGO profile effect around the changed golib methods, not the cut's
work, and is never to be quoted as a gain.

**Every Native AOT run exits 2** after its timed lines, in both arms, with the scheduler on and off: MnCost's final
`fmt.Println` reaches golib's `ReceiverKeyOf` (these arms predate P2's fix `18d5a2af71`). The timings are written
before it. The JIT output is EQUAL to Go (`ok 55 10 55 10`).

## The gates, P6-5 (2026-11-21, on d0330a86ac)

Unfiltered, with GOROOT and PATH pinned to go1.26.11 so the oracle tests run:
- GenTests 751 of 751.
- GolibTests Release 3,056 passed / 52 skipped / 0 failed of 3,108; Debug 3,031 / 77 / 0 of 3,108 (the expected skips).
- The 7b Q1 kit: 20 of 20 builds green (MnStormTwin and MnLaterArg fail as always), GO2CS0017 = 0 and GO2CS0018 = 0
  in all 22, and every twin and ifhall run that has a plain build is EQUAL to it (8 of 8).
- MnHttpClosure reconverted with this tree's converter (185 packages, 0 errors), arm S, the shipping rule: build 432 s,
  GO2CS0017 = 0, GO2CS0018 = 0, entry `RunMainꞏtwin` (the precondition), 3 runs under `GO2CS_MN=1` each EQUAL to Go
  with a READING line (steps 74, parks 36, thread-blocks 0).
- repoguard, full: rc 0.

The Q1 kit's first pass that evening built nothing and still read rc 0: its output folder did not exist. The kit now
creates it and counts what it read; a run with under 20 builds or 8 twin runs fails by name.
