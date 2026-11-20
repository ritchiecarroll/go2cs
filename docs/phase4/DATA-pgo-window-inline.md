# DATA: PerfString's PGO regression and the slice<T>.Window inline (R, 2026-11-20)

> **A point-in-time record.** G's post-publish perf window measured PerfString 2x slower at 1.26.11.1 (".4") than at
> 1.26.10.2 (".2"), JIT and ReadyToRun, with the .4 JIT leg BIMODAL (10 of 21 runs at .2 speed). This record holds the two
> bisects, the mechanism read from the JIT's own inlining events, the fix arms, and one measurement confound found on the
> way. Measured on R-LAPTOP (Windows 11, x64), .NET SDK 10.0.400, runtime 10.0.11 (G's versions), in-tree golib built
> Release. Base = master `c423f1593e`; the cut is `claude/r-window-inline` on it.

## 1. How every number here is taken, and the confound that forced it

- **Launch.** A benchmark is launched as PerformanceRunner launches it: a .NET `Process` with stdout redirected to a pipe
  the parent reads. Script: `pgo-run-ps.ps1`, 21 runs per arm unless stated, arms alternated.
- **CallCountingDelayMs = 0 in every arm.** It is the value the converter's csproj template writes. The template has
  written it since before the .2 record, so G's cells ran with it. The committed in-tree `PerfString.csproj` at the .2
  record lacks it (an older emission). Without it the .2 arm reads 1318-1530 ms.
- **THE CONFOUND.** The first bisect and the first fix arms were timed through Git Bash `$(...)` capture, which hands the
  child an MSYS pipe as stdout. Under that launch the same binary reads differently, for example cut4:
  - MSYS-pipe stdout: ~1170 ms;
  - stdout to a file: 640-780 ms;
  - the runner's launch: ~656 ms.

  A minimal environment (`env -i`) does not change the MSYS reading, so it is the stdout handle, not an environment
  variable. UNROOTED. It is filed on the BOARD as its own item; it is not this seat's. Every number below is the runner's
  launch unless marked "bash".

## 2. The two bisects

All 528 first-parent commits from the .2 record `1ca308d85f` to the .4 record `5dd5cafd5b`. Each arm builds
PerfString Release with in-tree golib, from a clean tree.

- **Bisect 1 (bash; the MSYS-pipe manifestation, NOT the runner's regression).**
  - Last good `06610aaa65` (fp331), median 582. First bad `2db66518be` (fp332, batch 287), median 2212.
  - Re-run alternating: 577/579 against 2219/1930.
  - The posted predictions (c5a21d17c6, 4167bfbcc2, 280a01952c) all MISSED.
- **Under the runner's launch that boundary does not hold.** The .2 record reads 11/11 fast (median 612). fp264, fp330,
  fp331, fp338, fp363, fp396 and the .4 record are all bimodal, 2-4 of 11 fast, medians 1758-1909. So the runner's
  regression is already present by fp264 (batch 284).
- **Bisect 2 (runner launch, (0, 264]).** Run after the seat, for this record only: see section 7.

## 3. The mechanism

The runtime's own JIT inlining events were read by an in-process `EventListener` on the runtime provider's JitTracing
keyword, loaded as a startup hook. The rows below are compiles into `main_package.run`.

- **`slice<T>.Window` is 146 IL bytes** at master (140 at the .2 record). That is over the JIT's 128-byte inline limit
  for a call site without a trusted profile.
- **At master it is refused** "too many il bytes" (always=True) in every run read: 5/5 under the runner's launch. A
  cascade of "unprofitable inline" refusals follows (ToSpan, GrowCapacity, AppendedElemDims, a slice ctor). Those helpers
  stay calls in the hot loop, and run's OSR body shrinks (1016 -> 781 lines, read at fp331/fp332).
- **It is a size threshold, not devirtualization or the inline budget.** Knob arms on UNCHANGED binaries:
  - `DOTNET_JitExtDefaultPolicyMaxIL=0x100` brings both fp332 and .4 back to .2 speed, and Window inlines again.
  - `DOTNET_OSR_HitLimit=100` (a later OSR compile, so more profile is gathered first) does the same.
  - `DOTNET_TieredPGO=0` reads ~2600 for .2 and .4 alike.
- **Reading:** the inline succeeds only when the JIT trusts run's early on-stack-replacement profile. When it does not,
  the plain limit applies. The bimodal split is that trust decided by timing.
- **UNROOTED:** which input change flips the trust. Under bash it moves at 2db66518be, whose own code (a byte on ж<T>,
  IPlainHeapRoot, FieldRefBox's cached facts) is not on the hot path.

## 4. The fix arms (runner's launch unless marked)

| arm | change | JIT median, 21 runs | fast (<900 ms) |
|---|---|---|---|
| base | master | 1156 / 677 (bimodal) | 10/21, 11/21 |
| .2 record | - | 606 / 610 | 21/21, 21/21 |
| cut1 | Window split, `WindowRare` an INSTANCE NoInlining method | 1947 (7 runs) | - |
| cut4 | Window split, `WindowRare` STATIC, fields passed by value | 656 / 661 (also 668 / 661) | 19/21, 19/21 |
| cut5 | cut4 + a field-only private ctor for Window's managed arm | **588 / 582** | 20/21, 21/21 |

- **THE TRAP (cut1).** A NoInlining helper that is an INSTANCE method on a struct receiver poisons the fast path, even
  though the arm it holds is cold.
  - Under bash with the inline knob: base reads 613 ms and cut1 ~1900; the static form (cut4) restores 578-620.
  - Under the runner's launch: cut1 reads 1947, cut4 656.
  - Reading: the instance call passes `this` by ref and address-exposes the caller's slice local. Measured by these arms,
    not proven beyond them.
  - So a cold-arm split must be STATIC. That is BOARD item kk.
- **cut4 -> cut5.** At cut4 the one size refusal left on the reslice path is Window -> the checked
  `slice(T[], low, high, max)` constructor (152 IL). Window has already made Go's checks, and the window lies within a live
  slice's capacity, so cut5 gives it a constructor that assigns the four fields alone. That is worth about 75 ms here.
- **Window's IL**: 146 at master; 69 at the cut. `WindowRare` holds the zero-size, native and nil arms.
- **The inline read at cut5** (runner's launch): Window is inlined into run in 3/3 runs. The Window -> ctor size refusal is
  gone.

## 5. ReadyToRun and Native AOT

- **ReadyToRun** (`dotnet publish -c Release -r win-x64`; golib ReadyToRun, the program's own code JIT-compiled per
  GoKeepOwnCodeOutOfReadyToRun):
  - base: 2294 / 2346 ms, every run slow.
  - cut5: 1114 / 1104 ms, with 5 and 3 of 21 runs at 2.2-3.0 s.
  - the .2 record: 1171 / 1199 ms, unimodal. It was published with master's PerfString.csproj, so its own code stays out
    of ReadyToRun as G's legs do.

  So ReadyToRun returns to the .2 level. On this box ReadyToRun is slower than the JIT at .2 as well. cut5's few slow
  ReadyToRun runs are NAMED and not rooted.
- **Native AOT** (`-p:PerfAot=true`): base 2439 / 2429 ms; cut5 2317 / 2311. No dynamic PGO there, and no regression: the
  cut is about 5% faster.

## 6. Census

- **Methods over 128 IL bytes** on golib's slice<T>, map<K,V>, array<T>, sslice<T>, @string, GoGrowthFacts<T> and builtin
  types, read from the cut's golib.dll metadata: 95 in all.
  - slice<T>: Append 642, AppendZeroed 411, OverNativeMemory 260, Overlaps 229, four checked ctors 141-215, Equals 171,
    AppendWindow 171, IndexOf 156, ZeroSizeSpan 143 (NoInlining), AliasOfElement 142, the Range indexer 139.
  - map<K,V>: growStep 289, .ctor 244, keyCarriesZero 239, ReflectFirstEntry 184, growHinted 146.
  - Also array<T>.Equals 171, sslice<T>'s indexer 137 and GoGrowthFacts<T>.RoundUpCapacity 178. builtin holds 73, led by
    TryTypeAssert 860, AreEqual 812 and the ConvertTo* family.
  - @string has none.
- **On a measured hot path** (PerfString's run, its inline events under the runner's launch), only two size refusals
  remain at the cut:
  - GrowCapacity -> RoundUpCapacity, on the growth arm, which this loop does not reach;
  - System.Buffer.Memmove -> SpanHelpers.Memmove, the BCL's own.

  Window and its ctor were the hot ones, and the cut fixes both.
- **Changed: none of the others.** No arm measured them on a hot path, and the ruling converts a helper only where an arm
  shows it pays.
- **INSTANCE NoInlining helpers** on struct receivers, the trap's shape (BOARD kk):
  - slice<T>.NativeElementRef and slice<T>.ZeroSizeSpan;
  - map<K,V>.setReplacingKey, setCopyingKey, setNilKey, addNilKey, removeNilKey and tryGetNilKey (map<K,V> is a readonly
    struct).

  Not converted: no arm here measured them on a hot path. NativeElementRef sits inside the inlined indexers (Sieve,
  MatMul), so it is the first to measure.

## 7. Bisect 2 (runner's launch)

Run after the seat was READY (2026-11-20, 14:57-15:40Z). It is for this record, not a gate. The window was (0, 264],
15 runs per arm, verdict by median (< 900 ms GOOD), clean builds.

| idx | commit | median | fast | verdict |
|---|---|---|---|---|
| 132 | `0904811dcf` | 1857 | 4/15 | BAD |
| 66 | `e8888b95d5` | 619 | 15/15 | GOOD |
| 99 | `31dc1870cb` | 1814 | 6/15 | BAD |
| 82 | `7fbb8fabfa` | 1808 | 3/15 | BAD |
| 74 | `0441f175cd` | 1791 | 4/15 | BAD |
| 70 | `14c62f1096` | 1888 | 2/15 | BAD |
| 68 | `c13a9cf690` | 1863 | 3/15 | BAD |
| 67 | `4aebdadc1a` | 619 | 15/15 | GOOD |

- **Last good `4aebdadc1a` (fp67). First bad `c13a9cf690` (fp68): batch 209**, lane R's own item 7.
  `GoZeroSizeFacts<T>.IsZeroSize` became an inlined pre-gate (GoZeroSize.cs +42), and Window's first arm reads it.
- **The inline read at the boundary** (runner's launch, 4 runs each):
  - fp67 refuses Window "too many il bytes" in 4/4 runs, and all 4 are FAST (595-652 ms).
  - fp68 refuses it in 3/4 runs, which are SLOW (1860-1973). The one run that inlines it is fast (600).

  So before batch 209 the out-of-line Window cost nothing that mattered. From batch 209 on, only the inlined Window is
  fast, and the bimodal split is whether it inlines. The cut makes it inline every time.
- **UNROOTED:** why batch 209 makes the out-of-line path slow. Two OSR listings, fp67 and fp68, both happened to be fast
  runs, and their call lists are identical (953 and 997 lines). A slow fp68 listing is the next read. The fix does not
  depend on it.
- **Bisect 1's boundary** (2db66518be, batch 287) is where the MSYS-pipe manifestation moves. It is not this
  regression.

## 8. Predictions, scored

Posted at 10:36Z, before any fix arm:
1. **Red -> green on the inline events: MET.** Window was refused at master and inlined at the cut, under both launches.
2. **JIT median within 15% of .2 (<= ~650), unimodal, no run > 1200.**
   - cut1 and cut4 MISSED. That is what found the trap, the launch confound and the ctor.
   - The seated cut5 reads 588 / 582 against .2's 606 / 610, with 41 of 42 runs fast.
   - One run at 1083 sits under the 1200 bar. Scored MET for cut5, with the earlier misses kept visible here.
3. **ReadyToRun ~= the cut's JIT: MISSED as worded.** ReadyToRun is slower than the JIT at .2 too on this box. It returns
   to .2's ReadyToRun level (section 5).
4. **Native AOT not slower: MET.** About 5% faster.
5. **GolibTests the same, no alloc row moving:** section 9.

## 9. GolibTests

- **The cut**, windows flavour, built and then run `--no-build`. Raw rc captured before any pipe, and checked for
  "Test Run Aborted":
  - Release: 2955 passed, 0 failed, 56 skipped, Total 3011, rc 0, not aborted.
  - Debug: 2930 passed, 0 failed, 81 skipped, Total 3011, rc 0, not aborted.
- **No allocation added.** WindowRare is a static method on a cold arm, and the field-only ctor assigns four fields. The
  Release run includes GolibTests' allocation rows, all passing.
- **Master `c423f1593e`, the same two runs:** Release 2955 / 0 / 56, Total 3011; Debug 2930 / 0 / 81, Total 3011. Both
  rc 0 and not aborted. The cut equals master in every count, so the Debug skip count (81 against Release's 56) is the
  suite's own configuration, not this change.
