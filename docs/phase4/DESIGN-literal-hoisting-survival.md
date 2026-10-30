# DESIGN -- does any hoisted string literal survive plain literals? (seat #52)

> Drafted 2026-10-30 by lane G, at master `893d48976f`, on the owner's ruling of 2026-10-30 as COORD relayed it:
> the standard library goes plain (seat #51, lane H), and hoisting survives only "in cases where it can be measured as
> faster", generalized from that measurement, or is dropped altogether. The measurements this record needs are lane
> R's (section 6). Nothing in the converter, golib or the generator changes in this seat. The prior record is
> `docs/phase4/proposals/plain-literal-cache.md` on `claude/coord-handover` (2026-10-17, i7, win-x64), cited below
> as "the record".
>
> **Closed 2026-10-30 (section 9).** The site classes were measured on G-LAPTOP, and the i9 ran three tests. The
> owner's literal policy is final: user code stays plain by default, `-hoist-literals` stays as a user opt-in, and the
> standard library emits the hoist mode, which is now the hybrid, by default. Sections 0 to 8 stand as drafted.
> Section 9 scores the predictions they made.

## 0. The decision rule, written before any reading

A hoisted form survives for a site class only if **all three** hold:

1. **It is faster in situ.** For that class, R's hoisted arm is faster than both plain arms (master's cache and D6)
   by more than the A/A arm's spread, on **both** the JIT at default tiering and Native AOT. A per-evaluation micro
   reading does not count, for the reason in section 2.
2. **The converter can select the class from syntax alone.** A class that only a profile can find cannot be
   generalized, because the converter has no profile.
3. **The class is one emission rule for every package.** A surviving class hoists in the standard library and in
   user code alike, so the corpus gates exercise it on every train. There is no flag and no mode.

If no class passes all three, **`-hoist-literals` is removed**: the flag, the mode, the `[GoHoistedLiterals]` stamp,
their tests and their docs (section 7). Removal is a full outcome, not a failure: the hoist mode is a second emission
path that has to be kept tested, and simplicity counts in its favour.

**Prediction, so the reading can prove it wrong:** removal. Option (b) cannot win, because of how interception
binds (section 4). The class most likely to win, a literal returned from a small function that is called from a hot
loop, is the one the converter cannot select (section 4a). D6 has already measured within noise of master's cache
(section 4c).

## 1. Two things called "hoisting"

**The mode.** `packageHoistsLiterals` is true for the standard library (`literalsHoistedFor`: one OR over
`stdLibImportPathOf`) and under `-hoist-literals`. In that mode every value-slot literal claims a
`static readonly @string` field, function-local string consts become fields (arm C, `collectLocalConsts`), the
readers of those fields feed init-order relocation (`packageHoistLitReaders`), go/defer arguments stay eager, and
`package_info.cs` carries `[assembly: GoHoistedLiterals]`, which tells `LiteralInterceptorGenerator` to skip the
assembly. Seat #51 takes the standard library out of the mode, which leaves `-hoist-literals` as its only entry.

**The residual hoists.** Plain mode still claims a field wherever no bare C# literal binds the right C# type
(the file comment of `hoistedLiteralOperations.go`):

- an `any` slot the generator cannot intercept;
- a named string type;
- an argument of a generic callee;
- an sstring twin;
- a multi-line raw literal.

These are not a mode. They are plain mode's fallback for type and correctness reasons, and **removing the mode does
not remove them**. Section 5 decides them separately, because the question there is not speed.

## 2. What is already measured, and why a micro reading cannot decide this

| Reading (the record) | Hoisted | Plain + cache |
|:--|--:|--:|
| one site, per evaluation, JIT (section 4) | 1.55 ns | 2.07 ns |
| one site, per evaluation, Native AOT (section 4) | 1.30 ns | 2.32 ns |
| `PerfStringMatch`, pure swap, JIT, median of 7 interleaved (section 5) | 1.000 | **1.066** (range 1.045 to 1.14) |
| `PerfStringMatch`, the full plain emission, JIT (section 5) | 1.000 | 0.991 (range 0.94 to 1.09) |
| `PerfStringMatch`, Native AOT | not run | not run |

A per-evaluation micro reading puts the hoisted field ahead on every class, because one load is always cheaper than
a hash, a load, a compare and a load. If the rule counted micro readings, hoisting would survive everywhere and the
question would be settled before it was asked. The record's in situ reading is what matters: in a loop whose next
iteration depends on the literal's result, the cache costs about 2 ns per evaluation (its section 5, from the JIT
dump), not the overlapped 0.5 ns. So R's arms put each class's literal on the loop's critical path.

The full plain emission measured neutral only because it also removed the copying `"…"u8` value sites (40 B per
evaluation). Seat #51 removes those in both modes, so after #51 the pure swap, +6.6% on the JIT, is the honest
baseline for a literal-dominated loop.

## 3. The candidate site classes

| Class | Go shape | Share of hoisted uses (the record, section 7.1) | Selectable by syntax? |
|:--|:--|:--|:--|
| C1 | an argument to an exact `string` parameter, in a loop body: `strings.HasPrefix(line, "//go:build")` | call arguments, 61.6% | yes, by loop depth |
| C2 | a return from a small function called from a loop: `kindName`'s `return "bool"` | returns, 3.5% | **no**: the literal is in no loop |
| C3 | a map index with a literal key, in a loop: `counts["build"]++` | map keys, 1.1% | yes, by loop depth |
| C4 | a struct-field or composite-element store, in a loop: `T{Name: "x"}` | composite elements: never hoisted (the hoist excludes them as Tier B), plain in plain mode; F1 here is a hand-written form | yes, by loop depth |
| C5 | a local assignment, in a loop: `s := "x"` | assignments, 6.0% | yes, by loop depth |
| C6 | an `any`-slot argument: `fmt.Println("x")` | `any` slots, 1.2% of production standard-library uses | yes, already: the interceptor's own predicate |

Not candidates: `==`, `!=` and `switch` against a literal, and concatenation. Both modes render these as `"…"u8`
spans with zero-cost operators, and there is no field to compare against.

`PerfStringMatch`, the only measured win, holds C1 (one site), C2 (four returns) and C3 (three map indexes). Nothing
splits its +6.6% among them, which is why R's arms measure each class alone.

## 4. Generalizing a winner: the three options

### (a) A converter heuristic: keep hoisting for a measured class

The natural syntactic proxy for "hot" is loop depth: hoist a value-slot literal whose enclosing function body puts it
inside a `for`. It selects C1, C3, C4 and C5.

- **It misses the measured win.** `kindName`'s four returns are in no loop; their function is called from one. The
  one-level extension ("in a loop, or in a function called from a loop") selects most of the functions in any
  package, which makes it the mode again under another name.
- **It hoists cold sites inside a winning class.** A loop that formats an error once per failure is in a loop. The
  rule cannot tell, so condition 1 holds for the class but not for every site in it. That is acceptable only because
  the cold site is as fast hoisted as plain; it costs readability, not speed.
- **The hoist machinery stays alive.** Naming, `-tests` seeding (`packageHoistNames`) and init-order relocation all
  stay, scoped to the surviving class.

### (b) A generator-backed per-site static, the way `any` slots work

The `any`-slot interceptor works because the literal's own conversion to `object` is a free reference conversion,
which the interceptor then overwrites with a pre-boxed Go string (`LiteralInterceptorGenerator.cs`, the class
remarks). An `@string` slot does not have that property:

- **An interceptor has the target's signature, so the argument is converted at the call site, against the target's
  parameter type, before the interceptor runs.** For an `@string` parameter, that conversion is
  `implicit operator @string(string)`, which is the cache lookup. Its acquire load and its miss path are side
  effects the JIT cannot drop, so the static the interceptor substitutes is paid on top of the lookup, not instead
  of it. The best case is equal to plain. R's arm F4 tests this prediction directly.
- **It reaches call arguments only.** Interceptors replace invocations. A return, a map index, an assignment and a
  field store (C2 to C5) are not invocations, and those are the uses `PerfStringMatch` measures.
- The only escape is a generated `string`-typed overload beside the callee. That is exactly the slot capture
  GO2CS0006 exists to refuse, and the record's section 10 measured overloads as worse than the cache.

Option (b) cannot make `-hoist-literals` unnecessary, because on the `@string` slots it cannot reach static-read
speed.

### (c) A faster cache lookup (the owner's 2026-10-17 item 3)

A hash cache's floor is a multiply, a key load, a compare and a value load. The adversarial look, each idea against
that floor:

| Idea | Verdict |
|:--|:--|
| **D6**: a fixed `static readonly` primary table, whose address the JIT folds, with the growable table as overflow | Measured: micro 1.85 ns (master's 2.07, hoisted 1.55); in situ 1 to 2% better, within noise; 8,192 cycled literals worse (19.1 ns). **The one lever worth an arm** (F3). |
| Drop the acquire on the hit path | Unsafe without a data dependency between key and value: the value is read from the slot, not through the key. Entry objects supply the dependency and measured slower (the record's D2, 2.48 ns). |
| A lazy `@string` that holds the C# string and transcodes on first use | Moves the check into every `@string` operation in the program. Rejected. |
| Pre-seed the cache from a module initializer | Removes only the first miss, never the hit cost. Rejected. |
| `Get` not inlined | Measured no better (the record, section 5). |

If D6 closes a winning class's gap to within the A/A band, the class does not survive: D6 lands instead, and the
mode is removed.

### (d) Keep `-hoist-literals` as a user flag with no corpus behind it

The record's section 8 already argues against this: an untested second path, which differs from plain in init-order
relocation and not only in spelling. Under condition 3 it cannot survive, whatever the readings say.

## 5. The residual hoists (H's input, 2026-10-30)

These are a type and correctness question, so no reading of R's decides them. The options per class:

| Residual | Inline replacement | Its cost | Recommendation |
|:--|:--|:--|:--|
| named string type | `(MyStr)(@string)"x"`: a cast through the cache | a cache hit, as for plain | the owner's readability call; nothing to measure, since cost equals plain |
| argument of a generic callee | `(@string)"x"`, which fixes `T` | a cache hit | as above |
| sstring twin | seat #51's territory | | defer to #51 |
| `any` slot the generator cannot intercept | `(object)(@string)"x"` | a box per evaluation, 32 B (the record, section 4) | **keep the internal pre-boxed field**: the inline form allocates |
| multi-line raw literal | none: `.Raw()` copies to strip the checkout's CRs | an allocation per evaluation | **keep the internal field**: correctness |

So the outcome of section 0 only ever removes the MODE. Plain mode keeps a small internal hoist for two classes,
under no flag and no stamp, exercised by the corpus because the standard library meets both. The first two rows can
go inline as a separate readability decision; the measurement is not needed for it.

## 6. The measurement map (R's brief, relayed by COORD 2026-10-30)

One Go-shaped kernel per class. The forms differ only at the literal site, and the literal's result feeds the next
iteration's input or index.

| Form | What it is | Classes |
|:--|:--|:--|
| F1 | hoisted `static readonly` field (the standard library before #51) | C1 to C6 |
| F2 | plain literal plus master's `StringLiteralCache` | C1 to C5 (C6: today's generated interceptor) |
| F3 | plain literal plus D6 | C1 to C5 |
| F4 | a hand-written interceptor with an `@string` parameter that overwrites the argument with a static field | C1 |
| F5 | F1 built as a second binary: the A/A noise band | C1 to C6 |

Runtimes: the JIT at default tiering, and Native AOT, both win-x64. 15 interleaved rounds per arm. Tiering off is not
measured: it is not a setting anyone should deploy (`docs/Tuning.md`). Optional: the `PerfStringMatch` macro under
Native AOT, hoisted against post-#51 plain, which has never been run (about 3.3 h per publish, the record's
section 5).

How each reading decides:

| Reading | Decides |
|:--|:--|
| F2/F1 and F3/F1 above the F5 band on both runtimes, for a class that is selectable | that class may survive, as an unconditional rule (option a, scoped to the class) |
| F3/F1 within the band where F2/F1 is above it | D6 lands; the class does not survive |
| F2/F1 above the band on the JIT only | the class does not survive (condition 1) |
| C2 above the band | recorded, but it does not survive (condition 2) |
| F4/F2 at or above 1 | option (b) is closed. Below 1 would refute section 4b, and the record is amended |
| C6: F2/F1 above the band | the interceptor's cost is the subject. That is a generator question, not a reason to restore the mode |

## 7. Removal scope, if the rule removes the mode

Read at master `893d48976f`:

- **Converter**:
  - `main.go`: the flag;
  - `commandLineOptions.go`: `hoistLiterals`;
  - `hoistedLiteralOperations.go`: `literalsHoistedFor`, and every `packageHoistsLiterals` branch, including arm C
    (`collectLocalConsts`) and the hoist-mode half of init-order feeding;
  - `goDeferConstArgs.go`;
  - `packageInfoWriter.go`: the stamp;
  - `conversionDriver.go`, `testConversion.go`, `packageStateOperations.go`;
  - the comment in `initOrderOperations.go`.
- **Tests**:
  - `hoistedLiteralsStamp_test.go`, `localStringConstHoist_test.go`, and the three mode toggles in
    `plainStringLiterals_test.go`;
  - the stamp cases in `GenTests/LiteralInterceptorGeneratorTests.cs`.
- **Generator**: the stamp check in `LiteralInterceptorGenerator.cs`.
- **golib**: `GoHoistedLiteralsAttribute.cs`. **Open item for the removal seat:** a package already published with
  the stamp names this type. Read whether any published package carries it, and keep the type, marked obsolete, if
  one does.
- **Corpus**: the 662 stamped `package_info.cs` and `package_test_info.cs` files, which #51 already rewrites.
- **Docs**:
  - `CommandLine.md` (the row, and its link to `Tuning.md`);
  - `ConversionStrategies.md`;
  - `ConversionStrategies-Reference/strings.md`;
  - a dated block in `DESIGN-string-literal-allocation.md`.

The residual hoists (section 5) stay. The removal touches neither their tests nor the behavioral goldens that show
them.

## 8. What this means for the tuning guide (seat #50)

Under section 0 the flag survives in no outcome: a winning class becomes an unconditional emission rule, which is
nothing a user tunes. So `docs/Tuning.md` needs no `-hoist-literals` paragraph either way. Only the `CommandLine.md`
row's link to the guide depends on the ruling, and it goes with the row if the flag is removed.

## 9. Readings, the ruling, and the predictions scored (2026-10-30)

### 9.1 The site classes, G-LAPTOP

The site classes ran on G-LAPTOP: an AMD Ryzen 5 PRO 6650U, win-x64, .NET 10.0.11. R built the instrument,
`src/tests/Performance/LiteralSiteBench` on `claude/r-seat52-literal-classes` @ `c82f304fde`. The run had no other
load and recorded no WHEA errors. Each class ran 15 interleaved rounds plus one discarded round. Every run exited 0
with equal checksums.

The control class ran first and on its own, and read inside the band on both runtimes. F4 was proved live: an
inverting copy printed checksum 1672 against 600.

The raw TSVs, the readouts, the F4 proof and the one change to the driver (an argument that orders the classes) are
in `docs/phase4/data/seat52-literal-classes`.

Cells are per-round ratios, median [min to max]. "Band" is the A/A arm, F5/F1. Every arm allocated 0 B per
evaluation except F1u8, which allocated 32 B.

| Class | Runtime | Band | F2/F1 (master's cache) | F3/F1 (D6) | Extra |
|:--|:--|:--|:--|:--|:--|
| control | JIT | 0.998 [0.985 to 1.031] | 1.004 [0.959 to 1.248] | 0.994 [0.941 to 1.009] | |
| control | AOT | 1.000 [0.992 to 1.010] | 1.000 [0.980 to 1.017] | 1.004 [0.982 to 1.227] | |
| C1 argument | JIT | 0.999 [0.949 to 1.213] | 1.637 [1.559 to 1.705] | 1.609 [1.532 to 2.569] | F4/F2 1.918 [1.859 to 2.455] |
| C1 argument | AOT | 0.998 [0.962 to 1.014] | 1.169 [1.137 to 1.185] | 1.147 [1.118 to 1.158] | F4/F2 1.260 [1.247 to 1.289] |
| C2 return | JIT | 1.000 [0.988 to 1.021] | 1.707 [1.659 to 1.756] | 1.664 [1.628 to 1.699] | |
| C2 return | AOT | 0.999 [0.975 to 1.034] | 1.772 [1.722 to 1.820] | 1.712 [1.686 to 1.771] | |
| C3 map key | JIT | 0.987 [0.957 to 1.033] | 1.131 [1.044 to 1.163] | 1.103 [1.040 to 1.149] | |
| C3 map key | AOT | 1.001 [0.981 to 1.204] | 1.024 [1.015 to 1.164] | 1.022 [0.992 to 1.102] | |
| C4 field store | JIT | 1.004 [0.981 to 1.127] | 1.078 [1.058 to 1.158] | 1.082 [1.064 to 1.191] | F1u8/F2 0.877 [0.821 to 0.949] |
| C4 field store | AOT | 1.000 [0.986 to 1.026] | 1.076 [1.068 to 1.154] | 1.058 [1.049 to 1.236] | F1u8/F2 0.928 [0.865 to 0.980] |
| C5 local | JIT | 1.001 [0.985 to 1.013] | 1.148 [1.125 to 1.156] | 1.129 [1.117 to 1.163] | |
| C5 local | AOT | 1.004 [0.988 to 1.007] | 1.112 [1.093 to 1.125] | 1.075 [1.058 to 1.099] | |
| C6 `any` slot | JIT | 0.999 [0.991 to 1.008] | 1.008 [1.000 to 1.025] | 1.009 [1.001 to 1.024] | |
| C6 `any` slot | AOT | 1.001 [0.985 to 1.007] | 1.003 [0.986 to 1.008] | 1.001 [0.993 to 1.067] | |

What the table shows, by the driver's disjoint-range test:
- The hoisted form is faster than both plain forms beyond the band for C1, C2 and C5 on both runtimes.
- C3 is beyond the band on the JIT only.
- C4 is beyond the band on Native AOT only. On the JIT it misses by one A/A outlier (1.127).
- C6 is inside the band on both runtimes.

### 9.2 The i9's three tests

All three ran on the i9: an Intel Core i9-13900K, win-x64, .NET 10.0.14. The arms were H (hoisted: the
`-hoist-literals` emission), HY (the hybrid) and P (plain). Each test ran 15 interleaved rounds and two passes, with
every output matching. Tests 1 and 2 ran at master `893d48976f`; test 3 ran at `aaec1d4ca8`.

1. **`PerfStringMatch`, in ms.**
   - JIT: H 598.5, HY 576.3, P 649.3, so P/HY is 1.13 (1.14 on the second pass; the paired range is 1.04 to 1.23).
   - AOT: H 569.3, HY 504.9, P 508.4, so P/HY is 1.004 to 1.007.
2. **`net/url` `ParseQuery`, ns and bytes per call.**
   - JIT: H 4,332, HY 4,378, P 4,328: flat. Bytes: H 5,392, HY and P 3,920.
   - AOT: H 4,729, HY 4,608, P 4,702, so P/HY is 1.02 in both passes. The i9 reads that as a layout effect.
3. **The deciding test: a user loop of 10M calls into small standard-library functions, H against P, with an H2 A/A
   arm.**
   - `Hash.String`: P/H is 1.686 and 1.782 on AOT (band 0.959 to 1.041), and 2.029 and 2.023 on the steady JIT (band
     0.976 to 1.024).
   - `FormatBool`: P/H is 1.121 and 1.250 on AOT (band 0.892 to 1.108).
   - `ChanDir`: beyond the band on AOT in one pass of two.
   - `AppendText`: flat.
   - **Counter-evidence, recorded and not counted:** on the JIT, plain `FormatBool` is inlined into the user loop and
     the hoisted one is not, so steady P/H is 0.86; the cause is unexplained.
   - The default-JIT rows are a Tier0 artifact (the standard-library callees never left Tier0). The i9 is
     investigating that, and then the inliner.

### 9.3 The ruling

The owner pre-registered the rule (ledger 2026-10-30 16:40): "If it makes little difference, it is settled, plain
literals in std-lib, otherwise, if there is a measured improvment, hosited by default". COORD applied it to test 3
(ledger 2026-10-30 18:02). The result:

- User code is plain by default.
- `-hoist-literals` stays as a user opt-in.
- The hoist mode becomes the hybrid of seat #51's point 2 (H, `58a59fc2dd`): it keeps the fields for healthy slugs, and
  every other site takes the plain mode's cached or cast form, so no site copies per evaluation.
- The standard library emits the hybrid by default.

Section 0 was not the deciding rule. Applied as written, it would have kept C1 and C5 as one unconditional in-loop
rule (each was faster on both runtimes, and loop depth can select it), failed C2 on condition 2, and removed the flag.
The owner's rule instead measured the C2 shape where it lives, at the call boundary into the standard library, and
kept the flag.

### 9.4 The predictions, scored as worded

| Prediction | As worded | Reading | Score |
|:--|:--|:--|:--|
| G, section 0 | "Prediction ... removal" | The flag stays (9.3); the in-situ gap held on both runtimes | **miss** |
| G, section 4b | Option (b) "cannot win"; F4 >= F2 | F4/F2 1.918 on the JIT, 1.260 on AOT | hit |
| G, section 0 | C2 is the class most likely to win, and the converter cannot select it | C2 is the largest gap on both runtimes (1.71, 1.77); not selectable | hit |
| G, section 4c | D6 does not close a gap | F3 moves each gap by at most about 4 points; none reaches the band | hit |
| G, section 2 | After #51, the pure swap's +6.6% (i7, JIT) is the honest baseline | The i9 measured P/HY 1.13 on the JIT: the right direction, about twice the size | partial |
| R, C1 | F2/F1 above the band; F3/F1 above but below F2/F1; F4/F2 >= 1 | as predicted on both runtimes | hit |
| R, C2 | above the band; F3 above but closer | as predicted on both runtimes | hit |
| R, C3 | inside the noise; the map operation, about 60 ns, dominates | AOT inside (55 ns); JIT beyond the band (28 ns) | half |
| R, C4 | above the band; F3 between; F1u8/F2 "far above 1" at 32 B | AOT above, JIT inside by one outlier; F3 between on AOT only; F1u8/F2 0.877 and 0.928, below 1, at 32 B | **miss** on F1u8, half on the rest |
| R, C5 | above the band | both runtimes | hit |
| R, C6 | inside the noise | both runtimes | hit |
| R, control | inside the noise | both runtimes | hit |
| R, AOT | "the same verdicts, with F2/F1 at least as large as on JIT" | C3 and C4 swap verdicts; F2/F1 is larger on AOT only for C2 | **miss** |
| R, bytes | 0 B except F1u8 | as predicted | hit |

One reading neither lane predicted: the i9's macro asymmetry (`PerfStringMatch` plain costs 13% on the JIT and nothing
on AOT) does not reproduce in the kernels, where AOT keeps the gap (C2 1.77). It looks like a property of that
program under AOT, not of the cache, and nothing has explained it.

### 9.5 Open items closed

- **`GoHoistedLiteralsAttribute`** (section 7): no removal seat follows, because the hoist mode survives as the
  standard library's default and as the user opt-in. The type stays live with the mode. Whether the hybrid still
  stamps its assemblies is seat #51's to state (H).
- **Section 8:** superseded. The flag survives, so `docs/Tuning.md` gains a `-hoist-literals` section (this seat), and
  the `CommandLine.md` row links to it.
