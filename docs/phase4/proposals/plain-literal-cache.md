# Plain string literals in `@string` slots: a literal cache in golib, measured

Experiment lane, 2026-10-17. Worktree `H:\go2cs-tmp-coord\plit`, local branch
`claude/coord-plain-literal-exp` (cut from the Go 1.26 hop tip `3bdac60827`), NOT pushed. Probe sources,
logs and binaries are in `H:\go2cs-tmp-coord\plit-probe`. Builds on `u8-zero-copy-string.md` (its
Route P, section 8) and `string-literal-interceptors.md` (its census, section 5). Host: i7-5820K
(Haswell-E), Windows 11, SDK 10.0.400, runtime 10.0.11, win-x64. Every micro row is 20M calls, best of
5, bytes from `GC.GetAllocatedBytesForCurrentThread`.

## 1. Answer

**The cache works, and it is cheap on a hit.** `implicit operator @string(string)` now looks the C#
string up in a process-wide table keyed by the literal's address. A C# string literal is a frozen,
interned object, so the address is stable and the entry leaks nothing. Through the real golib:

| `F("hello, world")` with `F(@string)` | JIT, default tiering | JIT, tiering off | Native AOT |
|:--|--:|--:|--:|
| today's hoisted field `F(helloˢ)` | 1.55 ns, 0 B | 1.55 ns, 0 B | 1.30 ns, 0 B |
| plain literal, **cache hit** | **2.07 ns, 0 B** | **2.15 ns, 0 B** | **2.32 ns, 0 B** |
| plain literal, base golib (no cache) | 18.8 ns, 40 B | 22.5 ns, 40 B | 18.7 ns, 40 B |
| run-time (non-literal) string, cache | 28.3–29.2 ns, 40 B | 31.8 ns, 40 B | 29.8–30.2 ns, 40 B |
| run-time string, base golib | 19.2–19.8 ns, 40 B | 23.0 ns, 40 B | 18.4–18.6 ns, 40 B |

Isolated, a plain literal costs **+0.5 ns (JIT) / +1.0 ns (AOT)** per evaluation over the hoisted
field, at 0 B.

**In a real hot loop it costs more: about 2 ns per evaluation.** On the suite's literal-heavy
`PerfStringMatch`, swapping the hoisted fields for plain literals costs **+6.6%** (JIT, median of 7
interleaved passes). The dependent loads sit on the loop's critical path; section 5 has the dump.

The complete plain emission the converter would produce is **neutral on the same benchmark (−0.9%,
within the ±4% noise)**. It also turns today's non-hoisted `"…"u8` value sites, which copy 40 B per
evaluation, into 0 B hits. On today's emission the golib change alone is inert: every suite row is
within noise, Fib control included.

Other costs:

- **A C# string that is NOT a literal now converts 9–11 ns slower** (+45–60%), for the probe and the
  frozen check. These are hand-written golib or C# call sites, not converted Go.
- The `@string` header is unchanged (16 B).
- GolibTests passes in full; section 3.

**Recommendation** (sections 8–10):

1. **GO: plain literals in `@string` slots as the DEFAULT emission**, with this cache in golib. Land
   the cache first, on its own: it is inert for today's emission.
2. **Keep the hoist as an opt-in mode only if the stdlib uses it.** Give `-stdlib` the hoist mode,
   and user conversions plain.
   - The only measured gain (single-digit percent, in literal-dominated hot loops) lives in library
     parsing and formatting code.
   - Shipping the stdlib in that mode is also the only way the second emission path stays tested:
     every train's corpus gates exercise it.
   - If the owner wants the stdlib to read like Go too, go plain everywhere, keep no second mode, and
     accept about 2 ns per hot literal evaluation.
   - A user-facing flag nobody's corpus exercises is not worth carrying.
3. **`any` slots: keep the pre-boxed hoist** (`fmt.Println(helloˢ)`). If the owner wants
   `fmt.Println("hello")` there, use **interceptors with an uncovered-literal error**, not
   hand-added overloads (section 10).

## 2. The cache (golib), and why this design

Files: `src/core/golib/StringLiteralCache.cs` (new), `src/core/golib/string.cs:520-526` (the operator
now returns `StringLiteralCache.Get(value)`).

- **Admission:** only strings with `GC.GetGeneration(s) == int.MaxValue`, which means frozen (on the
  non-GC heap).
  - Every C# literal of a non-collectible module is frozen, on JIT and Native AOT (measured).
  - A run-time string is gen 0-2, so it is converted as before and never stored.
  - A literal of a collectible AssemblyLoadContext is not frozen (gen 0), so it is not admitted. The
    module unloads (tested).
  - Exception: a collectible module's literal that a non-collectible module also holds resolves,
    through the runtime's global intern table, to the default context's frozen instance. That
    instance is admitted, and it is safe, because it belongs to the default context.
  - The table is therefore bounded by the distinct literals the process evaluates, the same bound Go's
    RODATA has.
- **Key and lookup:**
  - The literal's address, hashed by Fibonacci multiply-shift. Frozen objects never move.
  - The lookup compares references, never contents. A run-time string with a literal's content is a
    miss and converts normally, as Go's `string(b)` would.
- **Layout:**
  - Open addressing with linear probing over inline `(string Key, @string Value)` struct slots.
  - At most half full; grown by doubling and published whole.
- **Thread safety:**
  - Reads take no lock: a slot's value is written before its key is published with
    `Volatile.Write`, and readers `Volatile.Read` the key first.
  - Inserts take one `Lock`. A racing first conversion returns the winner's value, so every
    evaluation of one literal yields one backing array, as a hoisted field does.
- **Behavior:**
  - The bytes come from `AllocationCounter.Utf8ToBytes`, the same call as before (`Encoding.UTF8`: a
    lone surrogate becomes EF BF BD, and null becomes "").
  - The shared value is the immutable `@string` a hoisted field already shares. Its exits
    (`[]byte(s)`, `byte[]`) copy.
- **Structure:**
  - The hit path is inlined into the caller.
  - The probe continuation, the miss and the insert are three `NoInlining` methods, and none of them
    holds an `@string` local. Section 6 explains why that matters.

Candidates measured side by side. Stand-ins are in `plit-probe/bench/Caches.cs`; the run is
`run-cand-default.txt` (JIT, default tiering), plus `run-cand-tc0.txt` and
`run-cand-aot.txt` (AOT; transcribed, see its header).

| Candidate | Hit, one site | 8 sites | 1,024 literals cycled | 8,192 cycled | Run-time string (miss) | 8 threads, hit |
|:--|--:|--:|--:|--:|--:|--:|
| hoisted field (reference) | 1.55 | 1.29 | 1.81 | 1.81 | — | 2.07 |
| D1 direct-mapped 4,096 slots (the earlier Route P) | 2.06 | 1.91 | **17.9, 29 B** | **44.2, 79 B** | 25.1 | 3.32 |
| D2 open-addressed, `Entry` objects | 2.48 | 2.32 | 4.33 | 15.4 | 28.0 | 3.89 |
| D3 `ConditionalWeakTable` (interned admission) | 10.9 | | | | **113** (`IsInterned` ≈ 100 ns) | |
| D4 `ConcurrentDictionary`, reference comparer | 5.90 | 4.51 | 8.13 | 14.8 | 24.9 | 7.19 |
| **D5 open-addressed, struct slots (chosen)** | **2.07** | **2.07** | **3.69** | **10.9** | 28.1 | **3.67** |

Why D5:

- **D1** (the earlier probe's Route P) **thrashes.** Two hot literals sharing a slot evict each other,
  and every eviction re-transcodes and allocates. With 1,024 live literals it already averages 29 B
  per call, and the stdlib alone hoists about 4,300 distinct literals in production code. It is
  rejected.
- **D3** costs 100 ns per miss in `string.IsInterned`.
- **D4** costs 3–4 ns more per hit than D5.
- **D5** matches D1's hit cost without thrashing, scales best under table pressure (inline keys, no
  pointer chase), and stays flat under contention (no write on the hit path).

## 3. Correctness

New GolibTests arm, `src/tests/GolibTests/StringLiteralCacheTests.cs` (+ `.Literals.cs`, 600 distinct
literals). All 8 pass at Release:

1. The bytes equal `Encoding.UTF8.GetBytes` on miss and hit. Cases: ASCII, `héllo, 世界 😀`, embedded
   NUL, a lone high surrogate, a lone low surrogate, a reversed pair, and "". The hash and `Equals`
   match the uncached constructor.
2. A literal is admitted, and every evaluation shares one backing array.
3. A run-time string, even one with a cached literal's content, is never admitted, adds no entry,
   gets a fresh backing per conversion, and leaves the literal cached.
4. null converts to "".
5. Writes through `[]byte(s)`, `byte[]` and a slice-of-substring copy leave the cached value intact.
6. 16 threads × 50 rounds race first conversions of 600 literals, forcing table growth under
   contention. Each literal yields one backing across all threads, the bytes are correct, and all
   600 are cached.
7. AllocationCounter charges a hit 0 and a run-time miss 1. 1,000 hits allocate 0 B.
8. A collectible module (compiled with Roslyn, loaded into a collectible ALC) converts its own
   literal correctly. That literal is not frozen and not admitted, and the context unloads. The
   shared-literal case resolves to the global frozen instance.

Full GolibTests, Windows flavour, named-test logging (`--logger "console;verbosity=normal"`, logs
`plit-probe/gt-full-*.log`):

- **First Release run: 1,800 total, 1 failed, 25 skipped.**
  - The failure was `NoUncountedBackingAllocations`, golib's own guard. It flagged the new slot
    table (`new Slot[1 << log2]`) as an allocation that bypasses AllocationCounter.
  - It is infrastructure, not a Go-visible backing: the UTF-8 backing each slot holds IS charged. So
    it is now allow-listed by name, with that reason, in the guard's Infrastructure block.
- **Final Release run, after the fix: 1,800 total, 1,775 passed, 0 failed, 25 skipped.** No abort or
  crash banner.
- **Debug: 1,800 total, 1,767 passed, 0 failed, 33 skipped.** The Debug-only skip set is exactly the 8
  JIT-optimizer-gated tests `harness-gates.md` names (compared by name, not count); the Release-only
  skip set is empty. The Release skip set is identical across both Release runs.
- **The gate was made to fail first.** I admitted every string (dropped the frozen check), and
  `ARunTimeStringIsNeverCachedEvenWithALiteralsContent` and
  `ACollectibleModulesLiteralIsNotAdmittedAndTheModuleUnloads` failed, naming the defect. Then I
  restored the source; its SHA-256 is unchanged (`4c18a97feb93c70d…`), and the final runs used the
  restored build.

## 4. Micro-benchmarks: the full real-golib A/B

Same benchmark binary, golib swapped (`golib-base` vs `golib-cache`, hashes in `plit-probe`). Each JIT
arm ran twice, interleaved (`run-real-default.txt`, `run-real-tc0.txt`, `run-real-aot.txt`); this
table is the second pass. ns per conversion, B per conversion.

| Scenario | JIT base | JIT cache | JIT tc0 base | JIT tc0 cache | AOT base | AOT cache |
|:--|--:|--:|--:|--:|--:|--:|
| hoisted field (control) | 1.55 | 1.56 | 1.54 | 1.55 | 1.29 | 1.29 |
| one literal site | 18.9 / 40 | **2.07 / 0** | 22.5 / 40 | **2.15 / 0** | 18.8 / 40 | **2.33 / 0** |
| run-time string | 19.8 / 40 | 28.3 / 40 | 23.0 / 40 | 31.8 / 40 | 18.4 / 40 | 29.8 / 40 |
| 8 sites per iteration | 17.5 / 33 | **2.01 / 0** | 19.1 / 33 | 2.23 / 0 | 17.4 / 33 | 2.31 / 0 |
| 1,024 literals cycled (hoisted array 1.55) | 20.7 / 40 | 3.78 / 0 | 24.0 / 40 | 3.85 / 0 | 19.8 / 40 | 4.34 / 0 |
| 8,192 literals cycled (hoisted array 1.56) | 20.5 / 40 | 12.2 / 0 | 23.7 / 40 | 12.5 / 0 | 20.1 / 40 | 13.7 / 0 |
| `any` slot, `(object)(@string)"lit"` | 25.1 / 72 | 11.1 / 32 | 27.3 / 72 | 10.6 / 32 | 24.5 / 72 | 10.8 / 32 |
| 8 threads, same literal (ns/call/thread) | 33.3 | **3.46** | 36.8 | 3.56 | 31.7 | 3.83 |

What the rows say:

- The **8,192-literal** row is a worst case, with a cache-sized working set cycled in a tight loop.
  Even there, a plain literal is 0 B and 40% faster than today's non-hoisted literal.
- **Contention:** the uncached conversion degrades from 18.9 to 33 ns per thread at 8 threads (the
  allocator is the contended resource). Cache hits stay at 3.5 ns, against 2.0 hoisted.
- **`any` slot:** the cache halves today's inline `(@string)"x"u8` boxing (72 B to 32 B), but a box
  per call remains. Only a pre-boxed object reaches 0 B (section 10).

## 5. Macro: the performance suite

The suite's own runner (`src/tests/Performance/PerformanceRunner`) did the work:

- `--phase build,verify` once.
- Then `--phase verify,measure --runs 10 --no-aot` per arm, with each arm's `golib.dll` copied into
  every benchmark's `bin/Release/net10.0`.
- So the JIT binaries and the converted stdlib are byte-identical across arms. The A/B is one axis:
  golib.

Driver: `plit-probe/perf-ab.sh`; logs `perf-*.log`. Every run passed Verify (C# output equal to Go's).
JIT medians, ms. Go is shown for scale (Go 1.26.9 on this box).

Two traps on the way:

- `--phase measure` alone prints a table of `n/a` and **exits 0 / PASS in 0.4 s**: it measures
  nothing without Verify. It needs `verify,measure`.
- Single-run noise on this host is about ±4%. Identical `PerfStringMatch` binaries ranged from 1,404
  to 1,522 ms across passes.

**A. The golib change alone, on today's emission** (two interleaved passes each):

| Benchmark | Go | base golib | cache golib | Delta |
|:--|--:|--:|--:|:--|
| String | 156 | 831.4 / 824.4 | 821.5 / 830.0 | noise |
| StringView | 10.5 | 29.3 / 28.1 | 27.8 / 28.7 | noise |
| StringMatch (hoisted, as emitted today) | 525–531 | 1,456.6 / 1,465.4 | 1,460.8 / 1,469.2 | +0.3%, noise |
| Fib (control) | 136–146 | 159.3 / 156.1 | 157.6 / 161.5 | noise |

Today's converted code never reaches `implicit operator @string(string)` in a hot loop: every literal
is `u8` or hoisted. So the cache is **inert on the current emission**, which is the "no regression"
half of the result.

The other perf projects hoist only `checksum:`/`elapsed_ns:` (pre-boxed, evaluated once).
`PerfTlsHandshake`'s literal sites are setup-only. None has a hot literal site, so only
`PerfStringMatch` is a meaningful plain-literal subject.

**B. Plain-literal call sites, hand-edited copies of `PerfStringMatch`.** Both are untracked folders,
committed on the experiment branch only; the `.go` is identical apart from its name.

- `PerfStringMatchPlain/PerfStringMatchPlain.cs` is `PerfStringMatch.cs` with:
  - all 7 `static readonly @string` hoisted declarations deleted (`boolˢ goosˢ goarchˢ wordˢ goBuildˢ
    buildˢ commentˢ`);
  - their 9 uses written as plain C# literals: `return "bool";` ×4 shapes,
    `strings.HasPrefix(line, "//go:build")`, and `counts["build"]++`, `counts["comment"]++`,
    `counts["build"] * 3 + counts["comment"]`;
  - today's NON-hoisted `strings.HasPrefix(line, "// "u8)` (degenerate slug, 40 B per evaluation)
    written as `"// "`. That is exactly the section 7 emission.
- Left unchanged: the static `lines`/`words` composites (`u8`, evaluated once), the `switch` lowering
  (`exprᴛ1 == "true"u8`, span operators), and the pre-boxed `checksumˢ`/`elapsedNsˢ` (`any` slots).
- `PerfStringMatchPlainKeepU8` is the same, but keeps `"// "u8`. It isolates the pure
  hoisted-to-plain swap.

| Variant (cache golib unless noted) | median of 7 interleaved passes | vs hoisted, same pass |
|:--|--:|:--|
| StringMatch, hoisted (today) | 1,460 | — |
| **StringMatchPlain (the section 7 emission)** | **1,440** | **0.991** (range 0.94–1.09): neutral to slightly faster |
| StringMatchPlainKeepU8 (pure swap only) | 1,557 | **1.066** (range 1.045–1.14) |
| StringMatchPlain on BASE golib (plain literals, no cache) | 2,713 / 2,737 | 1.86 |

What this means:

- **In situ, a hoisted-to-plain swap costs about 2 ns per evaluation, not 0.5.**
  - The arithmetic: 2.4 literal evaluations per iteration × 20M iterations = 48M evaluations;
    +97 ms / 48M ≈ 2.0 ns.
  - The cause, from the JIT dump (`plit-probe/sm-*.asm`): each inlined lookup is a chain of dependent
    loads (table, slots, key, value) on the loop's critical path. `kindName` grows from 136 to 578
    bytes at Tier 1, and the OSR'd `run` loop from 5,964 to 6,797 bytes.
  - The micro-benchmark's 0.5 ns is the overlapped cost; 2 ns is the cost in a dependent chain.
  - **That is above the owner's 0.7 ns figure.**
- **The full plain emission is neutral here**, because it also turns today's copying non-hoisted `u8`
  value-slot sites into 0 B hits. Without the cache, plain literals would be an 86% regression.

Two variants were tried for the in-situ cost:

- `Get` marked `NoInlining`: no better (KeepU8 1,598 / 1,602).
- **D6**, a fixed `static readonly` 4,096-slot primary table (address folded by the JIT, no table
  load) with the growable table as overflow: micro hit 1.85 ns (against 2.07), and in situ about
  1–2% better on KeepU8 (1,586 / 1,557 against 1,600 / 1,591), within noise. Its 8,192-literal row
  is worse (19.1 ns, overflow probing). Source is kept at `plit-probe/StringLiteralCache.d6.cs`. It
  is not adopted; it is a tuning option if the stdlib goes plain.

**Native AOT macro: not run.**

- One AOT publish of a perf benchmark costs about 3.3 h and peaks at 15–18 GB
  (`src/tests/Performance/README.md`). This box had 8.7 GB free, and the A/B needs 3 arms × 3
  binaries.
- Hoisted-vs-plain is a call-site property, so it would also need separate publishes per variant.
- The micro AOT rows (section 4: +1.0 ns hit, 0 B) stand in for it.

## 6. A finding on the way: the Haswell AVX/SSE transition trap (tiering off only)

The first cut of the miss path cost **80 ns** for a run-time string with `DOTNET_TieredCompilation=0`,
against 23 ns today. Bisected (`plit-probe/gg`):

- The cost appeared only when the calling method's frame was large enough for the JIT to zero it
  with a 256-bit `vmovdqu ymm` store.
- It vanished with `DOTNET_EnableAVX=0` (but not with `EnableAVX2=0`).
- It **also hit today's plain transcoding** from such a frame: 26.5 ns from a small frame, 74.7 ns from
  a large one. It is not a cache cost.
- Under default tiering it disappears (big-frame `GetByteCount` 9.3 ns, against 50.7 ns at tc0).

The likely mechanism: with tiering off, CoreLib's precompiled ReadyToRun code runs and is never
re-jitted, and that code uses legacy-SSE encodings. After the 256-bit store leaves the upper YMM state
dirty, Haswell charges a state-transition penalty (about 70 cycles each way) on entering that code.
Fix in the cache: keep `@string` locals out of the miss-path methods. With that fix, tc0 miss = 31.8 ns.

Note for the corpus: **any converted method with a large frame that calls CoreLib R2R code under
`TieredCompilation=0` on Haswell-class CPUs pays this.** Release `-tests` hosts run with TC=0 by
default (`-test-tiered` flag help text). It is out of scope here and worth a BOARD row.

## 7. Converter design (written, not built)

Where hoisting lives today (tree `3bdac60827`):

- `src/go2cs/hoistedLiteralOperations.go`:
  - `:21-40` (rationale);
  - `consider` `:444`, the exclusions: empty literal, raw-byte escape, seeded, pre-boxed;
  - `hoistTarget` `:504` (return / assign / value spec / send / map index / call);
  - `callArgTarget` `:602` (builtins, conversions, `*f` format positions, variadics);
  - `classifyTarget` `:683` (string, named string and empty interface qualify; a type parameter or a
    non-empty interface is excluded);
  - pre-boxing `:779`;
  - declarations `writeHoistedLiteralDecls` `:1008`.
- Substitution: `convExpr.go:547` (`hoistedLiteralName`), and `convCallExpr.go:4728`.
- Rendering: `convBasicLit.go:786-823`:
  - raw bytes `:793` via `stringLiteralNeedsByteArray` `:47`;
  - the `u8` suffix `:812`;
  - multi-line raw `.Raw()` `:820`.
- The marker: `symbols.go:69` (`HoistedLiteralMarker`).

### 7.1 The change

`consider` keeps classifying exactly as today, but in plain mode it **registers only `hoistUseAny`
uses** (they keep a pre-boxed `object` field). It records the `hoistUseValue` nodes in a new
`packagePlainLits` set instead. `convExpr`'s BasicLit arm then renders a node in that set as the bare
C# literal (`u8StringOK=false`, `castToGoString=false`): `"hello"`. The C# compiler binds
`implicit operator @string(string)` and the cache does the rest.

Covered contexts:

- `@string` call arguments (61.6% of today's hoisted uses);
- returns (3.5%);
- `=`/`:=`/`var` (6.0%);
- map keys (1.1%);
- channel sends;
- named-string conversions (the wrapper takes `@string`);
- function-local string consts (arm C, `:290`): the `static readonly` field can become a plain local
  initializer.

That is about 74% of the 22,854 hoisted uses, plus every one of about 11,400 `@string` declarations.
A literal with mixed uses is better off than today: its `any` uses get a pre-boxed field (today they
box 32 B per call through the `@string` field), and its value uses go plain.

The same rendering should also replace today's NON-hoisted `"…"u8` in a value slot. These are the
degenerate-slug sites (`"// "`, `"OK"`), composite elements, and init-reachable functions:
`HasPrefix(line, "// "u8)` copies 40 B per evaluation today and costs 0 B plain. The interceptor
census counts about 2,066 such `(@string)"…"u8` forms.

### 7.2 Exceptions: keep today's form

| Site | Keep | Why (measured where noted) |
|:--|:--|:--|
| `any` / `...any` / empty-interface slots | pre-boxed hoist | a C# `string` there is the WRONG dynamic type (`System.String`, not `go.string`); compiles silently |
| C# generic inference positions: a golib generic helper, `goǃ`/`deferǃ`, any method whose parameter is a C# type parameter | `u8`/hoist | **measured**: `Index<T>(slice<T>, T)` called with `(slice<@string>, "x")` binds **T = string**, silently |
| Go type-parameter slots | as today (already excluded, `classifyTarget`) | the instantiation decides |
| sstring-twin parameters (`fmt.Printf/Sprintf/Errorf/...`, `utf8.RuneCountInString`, ...; `GoSStringTwin` records) and sstring locals | `"…"u8` | `sstring(ReadOnlySpan<byte>)` is a zero-copy view, while `sstring(string)` transcodes and allocates per call (`sstring.cs:76`). Format positions are already excluded (`:649-658`). Alternative: route `sstring(string)` through the cache too, a one-line golib change |
| `==` / `!=` / `switch` lowering | `"…"u8` | zero-cost span operators. Plain `s == "x"` compiles and binds `@string==(@string,@string)` (measured: 0 objects), but costs a cache hit plus a compare |
| concatenation (`s + "x"`, `+=`) | `"…"u8` | `operator +(@string, ReadOnlySpan<byte>)` consumes the span with no lookup. Plain compiles (measured) |
| raw-byte literals (`"\xff"`, octal >= 0x80) | byte-array form (`convBasicLit.go:793`) | a C# literal cannot hold invalid UTF-8 |
| multi-line raw strings | hoist (`"""…"""u8.Raw()`) | `Raw()` strips CRs by copying. A plain verbatim literal would bake the checkout's CRLF into the cached value. Keep these hoisted |
| `panic("…")`, `print`/`println`, `[]byte("…")`, `copy` | as today | builtins have their own paths (`callArgTarget` `:615-624`) |
| `const` declarations | as today | already their own `static readonly` field |

Lone surrogates cannot reach a C# literal: Go rejects `"\uD800"`, and byte escapes take the raw-byte
path. So the plain form is byte-exact for every literal it is used for.

Fail-open hazard, and its guard. A plain C# string, unlike a `u8` span, converts to `object`, to `T`
and to `string` overloads without a compile error. So the classification must be the existing
`callArgTarget` / `classifyTarget` decision (Go slot type) **plus** the C# callee's parameter type
when it is generic or has an sstring twin. Add a GolibTests or go2cs-gen analyzer arm: no plain
string literal may bind to an `object`, a C# type parameter or a `System.String` parameter in
converted code. The analyzer is cheap (an `IOperation` walk over `ILiteralOperation` of type
`string`), and it turns the fail-open into a build error.

## 8. Option: plain by default, hoisting as an opt-in flag

Cost to keep both paths:

- **Code: small.** The hoist pass exists. Plain mode is one branch in `consider` (value uses go to
  `packagePlainLits`), one rendering arm in `convExpr`, and a flag (`-hoist-literals`, beside `-var`
  at `main.go:281`, mirrored into a package-level bool the way `dualRecvEnabled` is,
  `captureModeOperations.go:542`). About a day including docs (`strings.md` hoisting section, symbols
  table).
- **Init-order coupling stays in both modes.** `packageHoistLitReaders` feeds `collectMovedInitVars`
  (`:50-55`, §4.4): plain value uses no longer read a field, so fewer initializers relocate in plain
  mode. The two modes then differ in **static-constructor relocation**, not just in literal spelling.
  That is a real second code path through init ordering. It is the part that needs coverage, not the
  spelling.

Testing burden:

- **Goldens** are generated in the default (plain) mode, so the behavioral tree tests plain. That
  means a re-baseline of every golden carrying a hoisted value-slot literal (most of them),
  `StringLiteralHoisting` and `LocalStringConstHoist` included.
- **The alternate mode** needs, at minimum:
  1. a `go2cs` unit test that converts the three hoisting fixtures (`StringLiteralHoisting`,
     `LocalStringConstHoist`, `CaptureHoistThroughConversion`) with the flag on, and diffs against
     checked-in `*.hoist.cs.target` goldens (the `refVerdictPublication_test.go:113` toggle pattern);
  2. one behavioral arm that compiles and runs those fixtures in hoist mode against Go's output;
  3. the init-order case (`encoding/pem`'s `var pemData = testingKey(…)`, `:125-130`) as a fixture in
     BOTH modes.

  A corpus-wide hoist-mode reconvert would only be exercised if something ships in that mode.
  Otherwise the mode rots between hops, and that is the main argument against keeping it.

Does the gain justify it?

- Isolated, the difference is 0.5 ns (JIT) / 1.0 ns (AOT) per literal evaluation.
- In a literal-dominated hot loop it is about 2 ns per evaluation: +6.6% on `PerfStringMatch` for the
  pure swap.
- The complete plain emission on that benchmark is neutral (−0.9%), because it also removes today's
  40 B copying sites.
- So the mode buys back single-digit percent, and only in code whose hot loops evaluate hoisted
  value-slot literals. In practice that is stdlib parsing and formatting paths (`strconv`, `time`,
  `net/http` headers, `encoding/*` keys), not user programs.

## 9. Recommendation on the two modes

- **Plain is the default emission.** Readability is the owner's objective. User programs rarely
  evaluate a literal in a dependent hot chain, and the full plain emission measured neutral on the
  suite's most literal-heavy benchmark: the copying non-hoisted `u8` sites get cheaper.
- **The owner's 0.7 ns premise holds only for isolated evaluations.** In a literal-dominated loop the
  cost is about 2 ns per evaluation, +6.6% on `PerfStringMatch` for a pure swap.
- **Keep the hoist mode (`-hoist-literals`) only as the stdlib's conversion mode**, with the `-stdlib`
  driver turning it on.
  - The measured gain lives in library parse and format hot paths.
  - The corpus gates every train already runs (`-stdlib` reconvert, solution build, `-tests`
    roster) then test the hoist mode for free, and the behavioral goldens test plain. Neither path
    rots.
  - That is worth the roughly one day of plumbing plus the three-fixture unit test from section 8.
- **A user-facing hoist flag with no corpus behind it is not worth it.** It would be the untested
  path, and it would differ from plain in init-order relocation, not only in spelling.
- **If the owner wants the stdlib to read like Go too:** go plain everywhere, keep no second mode,
  delete the value-slot half of the hoist pass, and accept about 2 ns per hot literal evaluation in
  library code. D6 (section 5) is the tuning lever if that matters.
- **Land the cache first, on its own.** It is a golib-only change with no emission churn, inert for
  today's converted code. It speeds up every hand-written C#-literal-to-`@string` site in golib and
  the hand-owned packages (panic messages, error strings), at +9–11 ns for run-time strings. Then
  flip the converter.

## 10. `any` slots: overloads, interceptors, a pre-boxed cache entry, or the hoist?

Measured (`run-cand-default.txt` / `run-cand-aot.txt`). The callee is shaped like
`fmt.Println(params ReadOnlySpan<object>)`. The interceptor row is the earlier scout's.

| `any`-slot call | JIT ns, B | AOT ns, B |
|:--|--:|--:|
| hoisted pre-boxed object (today) | 2.08, 0 | 2.33, 0 |
| interceptor `Println("…")` (scout, `string-literal-interceptors.md` §6) | 2.56–2.69, 0 | 2.75, 0 |
| cache entry with a lazily created PRE-BOXED object, called explicitly | 3.75, 0 | 4.98, 0 |
| hand-added `Println(string)` overload → pre-boxed cache entry | 4.69, 0 | 5.76, 0 |
| cached `@string`, boxed per call | 11.5, 32 | 10.0, 32 |
| today's inline `(@string)"…"u8` | 25, 72 | 25, 72 |
| raw `System.String` (wrong dynamic type) | 2.07, 0 | 2.32, 0 |

Hand-added overloads (`Println(string)` beside `Println(params ReadOnlySpan<object>)`):

- **Arity.**
  - An exact `(string)` overload covers only the one-argument call: 416 of the hoisted `Println`
    sites in the behavioral goldens.
  - A `(string, params ReadOnlySpan<object>)` overload binds every literal-FIRST call (measured: it
    wins over the params-only form), which adds 2,258 sites. But it must rebuild the argument span
    with the box prepended, and that costs an array or a pooled buffer.
  - A literal in a LATER position (`Println(x, " = ", y)`, 301 sites) is never covered.
- **Ambiguity.** **Measured: `Println(null)` and `Println(default!)` are CS0121** with that overload
  set. Every nil argument the converter emits into an `any` slot would need a cast. Worse, a
  `(string)` overload alone silently captures a `null` argument as a string (C# prefers `string` over
  `object`). The overload must then print `<nil>` itself, as Go does, not `""`.
- **Boxing.** Plain into a `(string)` overload pays the cache probe plus the box lookup: 4.7 ns
  against 2.1 hoisted. A per-literal pre-boxed cache entry gets it to 0 B, but at 2.3x the hoisted
  cost.
- **Maintenance.** `fmt`, `log` and `testing` are converted packages, so the overloads would be
  hand-owned companions (`*_impl.cs`) to re-check at every Go hop, for each of `Print`, `Println`,
  `Sprint`, `Sprintln`, `Fprint*` (with a writer first), `log.Print*`, `t.Log`/`t.Error`/`t.Fatal`/
  `t.Skip`, `errors.Join`, ... about 25 entry points × 2 shapes.
- **Knowing which sites may go plain.** The converter must know, per call, that an overload exists
  and binds. That is a hand-maintained allow-list in the converter. It fails open: a missed site
  still compiles and passes `System.String`.

Interceptors (the scout's design, its §7.1):

- They cover any arity and any position, and run at full speed (2.6 ns, 0 B).
- They need no hand-owned stdlib code and no allow-list. The converter knows `hoistUseAny` already.
- Their one hazard, a skipped site passing `System.String`, is closed by the generator reporting an
  **error** for every plain string literal in an `object`/`params object` slot it did not intercept.
  The same analyzer section 7.2 proposes covers this too.
- Costs: about 170 B of IL per site and +0.5 ms build per site (scout §6). About 5,700 `any`-slot
  sites corpus-wide, two thirds of them in behavioral goldens and tests.

Recommendation, in order:

1. **Leave `any` slots on the pre-boxed hoist for now.** They are 1.2% of production-stdlib uses, so
   performance does not motivate change. The readability case is real, though: 66% of
   behavioral-golden uses, the `fmt.Println("hello")` shape readers see first.
2. **If the owner wants `fmt.Println("hello")`, do it with interceptors plus the
   uncovered-literal error,** as a separate arc after plain `@string` literals land. Do not use
   overloads: they cover a minority of shapes, add CS0121 on nil arguments and a silent nil-capture,
   and are hand-owned per hop.
3. **A pre-boxed cache entry is worth adding to golib only as the interceptor's backing store**
   (`StringLiteralCache` can hold the box lazily per slot, prototyped in `Caches.cs` `SS.GetBoxed`).
   It is not worth having as a user-visible API.

## 11. Reproduce, commits, cleanup

Commits, local, signed, **not pushed**, on `claude/coord-plain-literal-exp` in
`H:\go2cs-tmp-coord\plit`:

- `7452f51cbc`: golib `StringLiteralCache.cs` plus the operator in `string.cs`, the GolibTests arm,
  and the allow-list line.
- `efc9a4b6dc`: the two hand-edited perf copies. Never run the runner's transpile phase over them: it
  would overwrite the hand edits.

Probe tree `H:\go2cs-tmp-coord\plit-probe`:

- `bench/`: micro-benchmark. `Caches.cs` holds candidates D1–D5 and the pre-boxed `SS.GetBoxed`;
  `Program.cs` / `Program2.cs` hold the scenarios.
- `golib-base`, `golib-cache`, `golib-noinl`, `golib-d6`: the swapped DLLs.
- `bout/`: JIT and AOT bench builds (deleted in cleanup; rebuild with the commands below).
- `gg/`: bisection probes.
- `shape/`: compile-shape probes.
- `perf-ab.sh`: the suite A/B driver.
- `run-*.txt`, `perf-*.log`, `gt-*.log`: results.
- `sm-*.asm`: JIT dumps.
- `StringLiteralCache.d6.cs`: the D6 variant.

Commands:

```
export PATH=<the .NET 10 SDK directory>:$PATH DOTNET_ROOT=<the .NET 10 SDK directory>
# micro (outputs must live OUTSIDE the bench project dir, or RAR resolves a stale golib.dll from it)
dotnet build bench -c Release -p:GolibDir='..\golib-cache\' -o bout/jit-cache
dotnet bout/jit-cache/Bench.dll real        # | all | lit | rt | any | mt ; DOTNET_TieredCompilation=0 for tc0
dotnet publish bench/Bench.csproj -c Release -r win-x64 -p:ProbeAot=true -p:GolibDir='..\golib-cache\' -o bout/aot-cache
# suite
PerformanceRunner.exe --filter String --no-aot --phase build,verify
bash perf-ab.sh                              # verify,measure --runs 10 per arm, golib swapped
```

Cleanup: all 284 bin/obj directories under the worktree `src/` were purged, plus the probe tree's build outputs; the probe
sources and logs are kept.
