# DESIGN -- does any hoisted string literal survive plain literals? (seat #52)

> Drafted 2026-10-30 by lane G, at master `893d48976f`, on the owner's ruling of 2026-10-30 as COORD relayed it:
> the standard library goes plain (seat #51, lane H), and hoisting survives only "in cases where it can be measured as
> faster", generalized from that measurement, or is dropped altogether. The measurements this record needs are lane
> R's (section 6). Nothing in the converter, golib or the generator changes in this seat. The prior record is
> `docs/phase4/proposals/plain-literal-cache.md` on `claude/coord-handover` (2026-10-17, i7, win-x64), cited below
> as "the record".

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
