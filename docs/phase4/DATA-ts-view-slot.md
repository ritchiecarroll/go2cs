# DATA: a box's first field view kept in its view slot (TypeScript perf item 2; R, 2026-11-21)

> **A point-in-time record.** Item 2 of P3's TypeScript performance plan (COORD ruling 2026-11-20 23:12Z) keeps a box's
> FIRST field view in the box's view slot and builds a `ViewEntry` list only from the second field on. The converted
> TypeScript checker takes one view per AST node per embedding level, and the list node was 22% of the bytes its run
> allocated. This record holds the predictions, every arm (one draft and three corrections), the falsifier that fired
> narrowly on the JIT, and COORD's ruling.
>
> **Box and tools:** R-LAPTOP, Windows 11 x64, .NET SDK 10.0.400, runtime 10.0.11. Base = master `1fa80494ee`; the seat
> is `claude/r-view-slot`. Every timing uses the runner's launch: a .NET `Process` with stdout redirected (never an MSYS
> pipe; BOARD mm), 21 runs per arm unless stated, arms alternated.

## 1. The change

- A view slot holds null, ONE view, or a `ViewEntry` list once a second field is asked of the box. That covers
  `SlottedStandardBox.m_views`, `FieldRefBox.m_views`, `ElemRefBox.m_views` (through `ж.ElementViews`) and the
  weak-table `ViewTable.Head`, now all `object?`.
- **Hit path:** `head is ViewEntry` first (sealed, not generic: one exact type compare even in shared generic code), then
  `head is FieldRefBox<TElem>` and its token.
- **Miss path:** `findIn<TElem>` uses exact type tests only. A token names one field, so a single view that can match it
  is a `FieldRefBox<TElem>`.
- **Publish:** stores the first view directly. A second field makes a two-entry list carrying the first view as its tail.
  It reads that view's token by exact type when the fields share an element type, and through the existing internal
  `IFirstFieldView.ViewToken` only when they do not.
- **Retention is unchanged:** one view per (parent, field), held for the parent's life.

## 2. Predictions (posted before the first arm, COORD inbox 2026-11-21)

P3's, scored as worded:
- checkdemo `ViewEntry` bytes -90% (3.6 GB -> under 0.4 GB);
- work-phase bytes -20%;
- type-check -2% to -4%.

Falsifier: `ViewEntry` stays above 1 GB, or PerfFieldView's hit path slows more than 3%. R added: GolibTests Release and
Debug identical to master.

## 3. The arms

**checkdemo.** golib has not changed between the 1.26.11.1 record and master, and both in-tree builds are assembly
version 1.26.11.1, so each arm swaps `golib.dll` alone into the item-0 phase-timed checkdemo (typescript-go-cs 6ba3f6b,
units-101, server GC as published). Stdout is identical in every run (`3EB1134A0D5BFF02`). One gc-verbose trace per arm,
then 3 timing rounds alternating.

**PerfFieldView.** In-tree builds of base and cut: JIT (Release build) and Native AOT (`-p:PerfAot=true`). 21 runs per arm,
2 rounds alternating.

| arm | change from the previous arm | PerfFieldView JIT | PerfFieldView AOT | checkdemo |
|---|---|---|---|---|
| draft | the slot holds one view or a list; the hit path tests `is FieldRefBox<TElem>` first; a new `IFieldView` | +5.4% / +4.6% | +9.5% / +8.4% | ViewEntry 3,616 -> 338 MB; bytes 16.28 -> 12.99 GB; work 82.7 -> 77.1 s (-6.8%); type-check -1.98% |
| remedy | the list tested first on the hit path | +2.6% / +2.3% | +8.0% / +13.1% | ViewEntry 334 MB; bytes 12.98 GB; work 81.6 -> 77.5 s (-5.0%) |
| arm 3 | `IFieldView` dropped; the token on `IFirstFieldView` | +5.5% / +4.2% | +8.8% / +11.8% | not run |
| **arm 4** | the miss path by exact type, no interface cast | +7.3% / +8.4% in its own battery; **+3.2%** interleaved (below) | **-1.6% / -0.4%** | ViewEntry 349 MB; bytes 12.98 GB (-20.3%); work 96.2 -> 92.6 s (-3.7%) |

- **The interleaved JIT reading.** Each arm's own battery ran in its own session, and the JIT medians drifted with the
  session. All four JIT binaries were then run in ONE session, interleaved:
  - round 1: master 3,738, remedy 3,905 (+4.5%), arm 3 3,882 (+3.9%), arm 4 3,857 (+3.2%) ms;
  - round 2: noisy (master 4,344 ms), arm 3 3,908, arm 4 3,988.

  So the variants are equal on JIT within a session, the cost is about +3 to +4.5%, and the apparent climb across arms
  was drift.
- **What arm 4 found.** PerfFieldView's second half calls `step()` on fresh element boxes (`&pts[i]`) 1.92 M times, each
  with 3 misses before 9 hits, about 5.8 M misses. The earlier arms' miss path cast a single-view slot to an interface,
  which Native AOT does slowly. Exact type tests closed AOT: -1.6% / -0.4%.
- **checkdemo across sessions:** work -6.8% (draft), -5.0% (remedy), -3.7% (arm 4). The box ran slower in arm 4's session
  (master 96.2 s against 82.7 and 81.6 s in the earlier ones). Allocation is the same in all three: -20%.

## 4. Scored

1. `ViewEntry` -90%, under 0.4 GB: **MET** (349 MB at arm 4).
2. Work-phase bytes -20%: **MET** (-20.3%).
3. Type-check -2% to -4%: **MISSED narrowly** (-1.98% at the draft; -0.3% at arm 4). Check fell (-14% at the draft) while
   bind rose (+6.5%, the same in every round), NAMED and not rooted. One candidate: a box's SECOND field costs two list
   nodes where master paid one.
4. **The falsifier FIRED narrowly on the JIT**, about +3 to +4.5% against the 3% bar (interleaved reading above). It did
   not fire on Native AOT (-1.6% / -0.4%), and not on `ViewEntry` (349 MB, under 1 GB).
5. GolibTests identical to master: **MET** at arm 4 (Release 2955/0/56, Debug 2930/0/81, Total 3011 each).
   - The draft failed one test, `TheConvertersListIsGolibsNamespace`, which caught the new `IFieldView`. Arm 3 removed
     the interface, and with it that line in `golibNamespaceNames.go`.

**COORD's ruling (2026-11-21 06:23Z):** "(a) arm 4 as it stands."
- AOT met.
- The JIT hit-path guard misses narrowly, at this box's session noise once the variants are interleaved.
- The real program gains (allocation -20%).
- The two-field slot (first token + first view + list, no type test) would have given back about half of that (+16 B on
  every view-carrying box, about +1.4 GB on checkdemo) for a micro-benchmark line.
- No further tuning.

## 5. Final reading at the merge tree (2026-11-21, after train 1)

The seat merged master `f604d2ae89` (train 1) cleanly at `7170a65e86`, so these readings take master `f604d2ae89` as the base.
The method is the same as in section 3: the item-0 checkdemo with `golib.dll` swapped, one gc-verbose trace per arm, then 3
timing rounds alternating. Stdout was identical in every run (`3EB1134A0D5BFF02`).

| | master `f604d2ae89` | seat `7170a65e86` | change |
|---|---|---|---|
| `ViewEntry` (trace) | 3,604.9 MB | 336.1 MB | -90.7% |
| allocated (whole trace) | 16,533.6 MB | 13,237.2 MB | -19.9% |
| work, 3 rounds (ms) | 96,219 / 98,743 / 89,239 | 93,435 / 92,733 / 89,364 | median -3.6% |
| bind + check, 3 rounds (ms) | 39,119 / 39,496 / 36,840 | 39,631 / 37,765 / 35,532 | median -3.5% |

- These numbers are a second reading. They do not change the scores in section 4, which stand as scored.
- The whole-trace allocation (-19.9%) is not the same measure as section 3's work-phase -20.3%.
- Type-check came in at -3.5% this time, inside P3's band. Paired by round it reads +1.3%, -4.4% and -3.6%. Bind fell here
  (median -7.9%) where it rose at the draft, and check rose (+3.5%). Session noise on this box is the size of the effect, so
  this reading does not root the section 4 note about bind.
- GolibTests at the seat: Release 3014/0/56 and Debug 2989/0/81, Total 3070 each. Master `f604d2ae89` reads the same in both
  configurations, which meets the prediction posted before master's run (item 2 adds no tests).
- Repoguard at the seat named 2 provenance notes in `docs/ConversionStrategies.md` that cite `ж.FieldRefBox.cs`. Both had moved
  5 and 11 lines when the `ViewToken` member was added. They are re-cited with their fingerprints unchanged.

## 6. Notes

- P3: after C2's item 1b (one view per forwarder chain instead of one per level), item 2 saves less on those chains. It
  stays correct.
- Raw logs and traces are on R-LAPTOP. The checkdemo kit and the phase-timed demo are on `claude/r-ts-prof-tables`
  (`ts-prof-r/`).
