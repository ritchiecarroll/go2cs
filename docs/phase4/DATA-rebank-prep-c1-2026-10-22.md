# DATA — re-bank prep: encoding/binary, internal/unsafeheader, strconv (C1, 2026-10-22)

> **A point-in-time record.** Three items from COORD's re-bank checklist, each measured on linux at
> `claude/hop-go126` `2b51b02d50` before any edit. Each item ended on a precondition for COORD to rule
> on, so this record carries the readings and the proposed edits. **No roster figure and no manifest
> entry changes in this commit.**

## How it was measured

- **Invocation:** the sweep's own per-row line (`src/run-validated-sweep.ps1:1129`), run by hand
  because this host has no PowerShell:
  `go2cs -tests -test-action all -test-timeout 10m -go2cspath <src> <GOROOT>/src/<pkg> src/core/<pkg>`.
  None of the three rows carries an `execution:` annotation, a timeout floor or a deadline floor, so
  this is the whole line.
- **Environment:** go1.26.10, `linux/amd64`, `CGO_ENABLED=0`, Release with tiered JIT off (the
  pipeline default). One row at a time, in place in `src/core/<pkg>`.
- **Restore:** each run's rewritten test sources and proof page were restored by name afterwards.
- **Windows:** every windows figure below is read from the COMMITTED proof page at
  `2b51b02d50` (`git show HEAD:docs/validation/current/<pkg>.md`). Those pages are the go1.24.13
  windows record. This record holds no windows measurement at the tip.

| Row | Linux at `2b51b02d50` (go1.26.10) | Committed windows record (go1.24.13) | Roster today |
|:--|:--|:--|:--|
| `encoding/binary` | 143 matched + 3 disclosed, 324 s | 142 + 4 | 142 + 4 · linux: 140 + 6 |
| `internal/unsafeheader` | **6 matched, 0 disclosed**, 19 s | no page (excluded) | E3 exclusion, 6 verdicts |
| `strconv` | 39 matched + 11 disclosed (1 skip/skip among the matched), 45 s | 55 + 11 | 55 + 11 · linux: 55 + 11 · darwin: 55 + 11 |

## 1. `encoding/binary`: TestSizeAllocs does NOT pass on linux at this tip

On linux, `TestSizeAllocs/*binary.Struct` still FAILS (Go pass, C# fail), so its parent
`TestSizeAllocs` is disclosed as `aggregate`. `TestAppendAllocs` also still fails. The deferred
disclosure therefore cannot be retired as a whole on this reading. The report that it passes at
`8c091fc3aa` is NOT reproduced on linux at `2b51b02d50`. Which host and which subtests that reading
covered is not recorded here.

The pipeline's orphan report (`orphanedDisclosures`, linux) names three entries that no longer fire:

| Entry | Linux at the tip | Committed windows record |
|:--|:--|:--|
| `TestSizeAllocs/[1]binary.Struct` | pass/pass, orphaned | pass/fail, **disclosed** |
| `TestSizeAllocs/[]binary.Struct` | pass/pass, orphaned | pass/pass |
| `TestSizeAllocs/[]binary.Struct#01` | pass/pass, orphaned | pass/pass |
| `TestSizeAllocs/*binary.Struct` | pass/fail, disclosed | pass/fail, disclosed |
| `TestAppendAllocs` | pass/fail, disclosed | pass/fail, disclosed |

**Proposed:** remove the three orphaned entries once a windows reading AT THIS TIP shows them
pass/pass. One manifest serves every platform, so a removal is a cross-platform edit (manifest
doctrine rule 1). `[1]binary.Struct` was disclosed in the last windows record.
`*binary.Struct` and `TestAppendAllocs` stay. The row then reads 143 + 3 on linux.

## 2. `internal/unsafeheader`: the E3 exclusion's premise is refuted

All six verdicts match Go on linux at the tip:

- `TestTypeMatchesReflectType`, with its `/Slice` and `/String` subtests;
- `TestWriteThroughHeader`, with its `/Slice` and `/String` subtests.

The write-through seat (`claude/c1-unsafeheader-writethrough`, in this tip) gives a slice or string
header written through its generated setter a real commit to the variable it views. A pass is
therefore no longer fabrication, and E3's stated reason ("all 6 verdicts fail identically,
structurally rather than by defect") no longer holds. The roster's rejoin clause applies: an
exclusion rejoins the denominator the day its mechanism is implemented.

**Open, for COORD:** the rejoin moves the arithmetic. The banked columns and the committed test
sources are the WINDOWS record, and this record holds only a linux reading. Two shapes are possible:

- **(a)** Rejoin and bank in ONE change, from the windows sweep at the re-bank (the i9 already reads
  internal/unsafeheader for that seat). Numerator and implementable denominator both move by one,
  as `os/user`'s rejoin did on 2026-09-01.
- **(b)** Rejoin now as an unbanked candidate: implementable denominator +1 and numerator unchanged.
  The header's "every implementable package validates" sentence would then stop being true until the
  bank.

C1 recommends (a).

## 3. `strconv`: the 1.26 test set, reconciled by test-function name

**Population.** Between go1.24.13 and go1.26.10, `strconv`'s top-level `Test` functions went from
56 to 40, and none was added. The 16 that left moved to the new `internal/strconv`:

- **14 under the same name:** TestAtofRandom, TestAtofSlow, TestDecimalRound,
  TestDecimalRoundedInteger, TestDecimalShift, TestFormatFloatInvalidBitSize, TestFp, TestFtoa,
  TestFtoaPowersOfTwo, TestFtoaRandom, TestParseFloatIncorrectBitSize, TestParseFloatPrefix,
  TestRoundTrip and TestRoundTrip32.
- **2 renamed:** `TestMulByLog10Log2` → `TestMulLog10_2` and `TestMulByLog2Log10` →
  `TestMulLog2_10` (same body shape, over the renamed `mulLog10_2`/`mulLog2_10`).

The arithmetic closes with zero residue: 16 = 14 + 2.

**Verdicts.**

| | Verdicts | Top-level | Subtests | Matched | Disclosed |
|:--|:--:|:--:|:--:|:--:|:--:|
| Before: windows go1.24.13, committed page | 66 | 56 | 10 | 55 | 11 |
| After: linux go1.26.10, measured at the tip | 50 | 40 | 10 | 39 | 11 |

- **Matched.** The 16 removed verdicts are exactly the 16 moved tests, each a single pass/pass row on
  the windows page. So 55 − 16 = 39, and the verdict NAMES of the two sets differ by exactly those 16.
- **Disclosed.** Unchanged at 11. All ten manifest entries (`TestAllocationsFromBytes/*`, class
  `deferred`) name tests that stay in `strconv` at 1.26 and still fire on linux, and the 11th is their
  aggregate parent. No entry belonged to a moved test, and no orphan was reported.

**Prediction for the windows re-bank** (derived, not measured): 39 + 11, by the same subtraction
from the committed windows record. The darwin annotation (55 + 11 today) has no per-verdict record
here, and it is predicted the same way only if its 16 were single verdicts as well.

**Successor.** `internal/strconv` is in the go1.26.10 population of record
(`docs/phase4/data/population-go1.26.10.txt`) and has no roster row. Its 14 + 2 tests are therefore
not counted anywhere until it banks: a candidate row, and the place the 16 verdicts land.

**Open, for COORD:** writing 39 + 11 into the row moves the header sums by −16 matched on each axis
it is written on. The roster's era line, the go1.26.10 pin, the population N = 242, and every other
row's 1.24.13 figures move together at the re-bank. Should strconv's row change now with linux
measured and windows derived, or at the re-bank sweep with both measured? C1 recommends at the
re-bank, with the closure above as the expected reading.
