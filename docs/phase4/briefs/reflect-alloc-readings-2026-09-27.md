# reflect: readings for the last two alloc-profile disclosures (TestMapIterSet, TestMapAlloc)

Measurement sub-agent for COORD, 2026-09-27 17:50-18:10 CDT, on the i7.
Scope: readings only. No fix, no commit, no push, no ledger or mailbox post.

- **Tree:** `tF` worktree, branch `claude/coord-trainF`, HEAD `1aebd6a885` (= master). The tree is restored: `git status --porcelain | grep -v '^??' | wc -l` = **0**, and there are no `^ D` rows.
- **Pins:** go1.24.13, `go env GOROOT` printed the backslash spelling. GOTOOLCHAIN=local, CGO_ENABLED=0. .NET SDK 10.0.400 with runtime 10.0.11, the same runtime the test host self-contains.
- **Configuration of record:** Release, `DOTNET_TieredCompilation=0`, workstation GC, x64. A tiering-on arm was taken too (§5).
- **Ruling served:** ledger 2026-09-27 14:58, which asked for "TestMapIterSet's per-site BYTE attribution of about 3.3 KB/run, and TestMapAlloc's two AllocsPerRun blocks' unit notes plus per-site counts".

---

## 1. Instruments and their positive controls

### C# side

**Instrument A: the converted host itself.** This is the reading of record for a `reading` field.

- Command: `go2cs -tests -test-action all -test-config Release -test-filter '^(TestMapIterSet|TestMapAlloc)$'` at the tree HEAD, with a freshly built converter.
- The converter warned that a filtered run publishes nothing: it is diagnostic only, and no artifact was banked.
- The unit notes it wrote are quoted in §2 and §3.

**Instrument B: a scratch harness** (`<scratch>/reflect-alloc/csharness`). This is the per-site attribution.

- It references the host's own built assemblies from `src/core/reflect/bin/tests/Release/net10.0/win-x64`. Every call therefore runs the exact golib/reflect IL the host ran.
- Each closure body is copied verbatim from the converted `all_test.cs`.
- TestMapIterSet's map is built from the converted `valueTests` table, read out of `reflect.tests.dll`: 27 rows and 22 distinct keys, the same as Go.
- **The meter** is the one `testing.AllocsPerRun` reads:
  - bytes: `GC.GetAllocatedBytesForCurrentThread()`;
  - count: `go.AllocationCounter.CurrentThreadCount`, enabled the way the host enables it.
- **The window** matches AllocsPerRun: one warm-up call outside it, then N runs.
- **Sub-steps** are bracketed individually inside the real loop.
- **Private state** (`MapIter.mapEnum`, `Value.addrBox`, `Value.boxed`) is read via `[UnsafeAccessor]`. No source was touched.
- **Why this instrument:** bracketing measures exactly the population the assert's meter sees, in the same units. EventPipe allocation sampling would give type identity but a different, sampled population.
- **Identity checks:** object identity was confirmed by type name where it matters (the box type, the enumerator type, and Current's boxed type). Every size equals a control-measured size or an arithmetic layout.

**Positive and negative controls** (3 repetitions each, all exact):

| control | expected | read |
|---|---|---|
| empty closure | 0 B, 0 obj | 0 B, 0 obj |
| `new object()` | 24 B, 0 golib obj | 24 B/run, 0 obj |
| `AllocationCounter.NewArray<byte>(16)` | 40 B, 1 golib obj | 40 B/run, 1 obj/run |
| boxed `long` | 24 B | 24 B/run |
| boxed `KeyValuePair<@string,object>` | 40 B | 40 B/run |
| boxed `@string` | 32 B | 32 B/run |
| `testing.AllocsPerRun(100, new object())` | byte fallback, 24 | **24** |
| `testing.AllocsPerRun(100, NewArray<byte>(16))` | count, 1 | **1** |
| `testing.AllocsPerRun(100, empty)` | 0 | **0** |

The instrument can fail: it reads nonzero on a single known allocation, reads it at the right size, and separates counted from uncounted objects.

**Reproduction against the host:**

| test | harness bracket | host |
|---|---|---|
| TestMapIterSet | 33,120 B and 10 obj over 10 runs; harness AllocsPerRun **1** | 33,120 B / 10 |
| TestMapAlloc block 2 | 12,717,768 to 12,717,936 B and 100,200 obj over 100 runs; harness AllocsPerRun **1002** | 12,717,936 B / 100,200 |

The two agree to the byte. The ±168 B jitter across repetitions is one sporadic 168-B allocation in the TypeOf/MakeMapWithSize path, probably tied to GC timing (not identified). It never moves the count.

### Go side

**Instrument C: `go test -run '^(TestMapIterSet|TestMapAlloc)$' -count=1 -v -timeout 5m`** in `%GOROOT%\src\reflect`. Both tests PASS.

**Instrument D: a Go harness** (`<scratch>/reflect-alloc/goharness`).

- It runs the same three closures, verbatim, over the same valueTests map (22 keys).
- For each closure it reports `testing.AllocsPerRun`, plus `runtime.MemStats` Mallocs and TotalAlloc deltas under GOMAXPROCS(1) with a warm-up call.
- Positive control: an escaping `new(int)` reads AllocsPerRun **1**, 100 mallocs, 800 B over 100 runs. The empty closure reads 0/0.

**Second derivation from another host:** C2's linux census (`docs/phase4/probes/c2-mnz-census/census-linux-amd64.tsv`) records Go 0 / 0 / 6 mallocs per run for the same three sites. That agrees with this box.

---

## 2. TestMapIterSet (`all_test.go:346`; the assert is `want := 0`, "wanted %d alloc, got %d")

### Go's reading on this box

AllocsPerRun(10) = **0**, with **0 mallocs and 0 B** over 10 runs. The test's own comment names the reason: MapRange is inlineable, so the `*MapIter` stays on the stack. SetIterKey and SetIterValue typedmemmove in place.

### C#'s reading

- **Host note (instrument A):** "counted 10 go2cs-runtime object allocations (33,120 bytes) over 10 run(s) … allocation COUNT per run". The test reports "wanted 0 alloc, got 1".
- **Unit: COUNT, 1/run.** The CLR also allocates **3,312 B/run**, which confirms the ~3.3 KB figure exactly at TC0. With tiering on it is 3,336 B (§5).
- The CLR allocates **69 objects/run** in total, and golib's counter charges **1** of them.

### Per-site table (TC0, 1,000 runs; every row is exact, and the sum equals the whole closure)

| site | when | objects/run | B each | B/run | golib-counted |
|---|---|---:|---:|---:|---:|
| `MapRange` → `builtin.heap<MapIter>`: a `StandardBox<MapIter>` (the managed `*MapIter`) | per call (per MapRange) | 1 | 224 | 224 | **1** |
| `MapRange` → `((IEnumerable)map).GetEnumerator()`: the compiler-generated iterator `map<@string,object>.<enumerateStore>d__33` | per call | 1 | 72 | 72 | 0 |
| first `iter.Next()` → golib's range snapshot `KeyValuePair<@string,object>[22]` (map.cs enumerateStore; 24 + 24·len) | per range, scales with map len | 1 | 552 | 552 | 0 (by design: `NoUncountedBackingAllocationsTests` ByDesign row, "the range snapshot") |
| later `iter.Next()` (21 more + 1 final false) | per iteration | 0 | 0 | 0 | 0 |
| `SetIterKey` → `iter.Key()` → non-generic `IEnumerator.Current` boxes the `KeyValuePair<@string,object>` | per iteration | 22 | 40 | 880 | 0 |
| `SetIterKey` → `PropertyInfo.GetValue(cur)` for `Key` boxes the `@string` (a struct) | per iteration | 22 | 32 | 704 | 0 |
| `SetIterKey` → `GetType().GetProperty("Key")`, `makeTypedValue`, `k.Set(...)` | per iteration | 0 | 0 | 0 | 0 |
| `SetIterValue` → `iter.Value()` → `IEnumerator.Current` boxes the KVP again | per iteration | 22 | 40 | 880 | 0 |
| `SetIterValue` → `GetProperty("Value")`, `GetValue` (the value is already an object reference, so there is no box), `e.Set(...)` | per iteration | 0 | 0 | 0 | 0 |
| **total** | | **69** | | **3,312** | **1** |

**Formula:** B/run = 848 per range (224 + 72 + 24 + 24·n) + 112 per entry (40 + 32 + 40). With n = 22 that is 320 + 136·22 = **3,312**.

**Unit notes:**

- **C#:** one reported "alloc" is one golib-charged managed object. Here it is exactly the `StandardBox<MapIter>`, the counterpart of Go's `*MapIter`. The host's note says the count is a LOWER BOUND, and it is: 68 of the 69 objects are outside golib's counter (compiler-emitted iterator, BCL boxing, reflection boxing, and the by-design snapshot).
- **Go:** one "alloc" is one mallocgc, and there are none.
- **The meter asymmetry that matters:** the assert is want-zero. Per the meter rule it could pass only at zero BYTES, since a zero count with nonzero bytes switches to the byte arm. So removing the counted box alone would move the failure to the BYTES arm, which would read 3,088 B/run.

### PROPOSED label: `structural`

**One-line reason:** the one counted object is the `*MapIter` itself, the object the test's own comment says Go keeps off the heap, and go2cs cannot keep it off the heap.

- Go does it by inlining MapRange into the caller and stack-allocating the non-escaping iterator.
- go2cs represents `*MapIter` as `ж<MapIter>`, an abstract class, so the object is heap by construction. The hand-owned MapRange creates the box and returns it. Next, SetIterKey and SetIterValue then take it as a `ж<MapIter>` argument.
- No implementation that keeps Go's call boundary can avoid it. A shared or pooled box would alias two live iterators, which Go forbids.
- At TC0 the CLR JIT does not remove it either (measured: 224 B, count 1, on every run).
- Reading = floor = 1 object > want 0.

**Draft reading text:** "1 alloc/run, unit COUNT (10 golib objects, 33,120 B over 10 runs; 3,312 B/run of which 3,088 B are 68 uncounted CLR objects) at 1aebd6a885, windows/amd64, Release TC0".

**Where COORD should look hard.** The proof rests on the converter emitting no cross-function inlining. The reference text says an escape-analysis assert is "never … CLR-structural merely because it fails today". If COORD counts a converter-level inlining of inlineable pointer-returning callees as a plannable managed implementation, the entry becomes `deferred`. No design record names such a stage today; C1 found none, and a grep of docs/ for MapRange/MapIter finds none. That would leave it "reading owed / ruling", not deferred.

**The uncounted 3,088 B/run are named mechanisms that could be reduced,** but reducing them retires nothing while the box exists:

- a typed accessor instead of non-generic `IEnumerator.Current` plus `PropertyInfo.GetValue`, which would remove 112 B per entry;
- the per-range iterator (72 B) and snapshot (552 B).

**The current reason text is inaccurate.** It says "a KeyValuePair box per step". In fact there are two per step plus a boxed key, and the text omits the counted object altogether.

---

## 3. TestMapAlloc (`all_test.go:7385`; two AllocsPerRun blocks)

### Block 1: `m.SetMapIndex(k, v)` on `map[int]int` (k and v are non-addressable `ValueOf(5)`/`ValueOf(7)`); asserts `allocs > 0.5` fails

| side | reading |
|---|---|
| Go (this box) | AllocsPerRun(100) = **0**; 0 mallocs, 0 B |
| C# (harness, 3 reps) | **0 B, 0 golib obj** over 100 runs. The warm-up call alone is 808 B (first-call caches), which AllocsPerRun excludes. AllocsPerRun returns **0.0** |
| C# (host) | no failure line for block 1, so it passed |

- **Unit note:** none on either side. Zero bytes is exact in both units, and the host deliberately emits no note at zero.
- **Per-site:** `SetMapIndex` whole is 0 B. Its `.live` reads return the Values' stored boxes, which costs nothing because they are not addressable.
- **Block 1 matches Go and needs no label.**

### Block 2: `MakeMapWithSize(TypeOf(map[int]int{}), 1000)` plus 500 × (`val.SetInt(i)`; `mv.SetMapIndex(val, val)`), where `val` is addressable; asserts `allocs > 10` fails

**Go's reading on this box:**

- AllocsPerRun(100) = **6**: 604 to 605 mallocs and 36,992 B/run over 100 runs. The +4/+5 is background noise, and integer division gives 6.
- The MakeMapWithSize(1000)-only closure reads 6 mallocs and 36,992 B/run, so all six are the Swiss map's own make. The 500 SetInt/SetMapIndex pairs allocate nothing.
- The test's comment ("3 allocations") predates Swiss maps. go1.24.13 reads 6 against a bound of 10.

**C#'s reading:**

- **Host note:** "counted 100,200 go2cs-runtime object allocations (12,717,936 bytes) over 100 run(s) … allocation COUNT per run". The test reports "want at most 10 got 1002.000000".
- **Unit: COUNT, 1002/run**, with 127,176 B/run (+ the sporadic 168 B).
- The CLR allocates **3,506 objects/run**, and golib counts 1,002 of them.

**Per-site table** (TC0, 100 runs; the sum equals the whole closure):

| site | calls/run | CLR objects per call | B per call | B/run | golib obj/run |
|---|---:|---|---:|---:|---:|
| `new map<nint,nint>{}` (TypeOf's argument): the empty `NilKeyDictionary`. Go: 0, because the literal does not escape | 1 | 1 | 96 | 96 | **1** |
| `TypeOf(map)`: boxes the map struct into `any` | 1 | 1 | 24 | 24 (27.36 incl. jitter) | 0 |
| `MakeMapWithSize(t,1000)` → `GoReflect.MakeContainer`: the `NilKeyDictionary` (96) + entries `Entry<nint,nint>[1103]` (26,496) + buckets `int[1103]` (4,440) + the map struct boxed to `object` (24). 1103 = HashHelpers prime ≥ 1000 | 1 | 4 | 31,056 | 31,056 | **1** (the Dictionary; its two arrays are BCL-internal and uncounted) |
| `val.SetInt(i)`, split into the four sub-sites below | 500 | 4 | 120 | 60,000 | **1,000** |
| ↳ `setKinded(v, x, "SetInt"u8)`: the u8 literal copied into a fresh `@string` (byte[6]) because the parameter is `@string op` | 500 | 1 | 32 | 16,000 | **500** |
| ↳ `v.flag.mustBeAssignable(op)`: that `@string` converted to `System.String` (golib's `Utf8ToString`, counted) because the parameter is `string method`, **on the success path** | 500 | 1 | 40 | 20,000 | **500** |
| ↳ `object wide` = the int64 argument boxed (compiler) | 500 | 1 | 24 | 12,000 | 0 |
| ↳ `GoReflect.TryConvertTo(wide, nint)`: the converted nint boxed | 500 | 1 | 24 | 12,000 | 0 |
| ↳ `GoReflect.WritePointerSlot` | 500 | 0 | 0 | 0 | 0 |
| `mv.SetMapIndex(val, val)`: three reads of `.live` on the addressable `val` (key ×2: `GoDynamicTypeOf(key.live)` and `TryMarshalAssignable(key.live)`; elem ×1), each `ReadPointerSlot` boxing a nint | 500 | 3 | 72 | 36,000 | 0 |
| ↳ `TryMarshalAssignable`, `GoDynamicTypeOf`, `SetMapEntry` (the map stores `nint` unboxed) | 500 | 0 | 0 | 0 | 0 |
| **total** | | **3,506 objects/run** | | **127,176** | **1,002** |

**Unit notes:**

- **Same unit, different populations.** Both sides report a COUNT.
  - Go counts each mallocgc: the Swiss map's 6 allocations, **including its table arrays**.
  - golib counts objects golib asks for: 1 Dictionary per make, while the Dictionary's two arrays are uncounted. It also counts 2 string objects per SetInt call, which Go does not have at all.
  - So golib UNDER-counts the make (1 vs Go's 6) and OVER-counts SetInt (2 vs 0).
- **The host writes one unit note per TEST, not per block** (`NoteMeasurementUnitOnce`). The note in the results file is block 2's only because block 1 returns exactly 0. If block 1 ever read nonzero, the file would carry block 1's note and block 2 would have none.

**What-if arm** (not the test, and not a fix):

- The arm is block 2 with SetInt's store done through the same two golib calls setKinded makes (TryConvertTo + WritePointerSlot), minus the op-string materialization.
- It reads **COUNT 2/run** (200 golib obj; 9,117,768 B over 100 runs = 91,178 B/run), and `testing.AllocsPerRun` returns **2 ≤ 10**. The assert would PASS.
- The two remaining counted objects are the two Dictionaries.

### PROPOSED label: none. This is a real, cheap divergence with a named mechanism, and it should be fixed rather than labelled.

**One-line reason:** 1,000 of the 1,002 counted objects per run come from one mechanism.

- `setKinded`'s `@string op` materializes the "SetInt" literal as a counted `@string`, then converts it to a counted `System.String` for `mustBeAssignable`, on every successful call.
- This is the same mechanism `mustBeKind` already retired by taking spans (value_impl.cs:180-188, "As spans, the literal is copied into a @string only on the panic path").
- Removing it is measured to bring the host's reading to 2 ≤ 10, so the test passes and the disclosure retires.

**Fallback if COORD banks reflect before the fix:** `deferred`, with

- **want:** at most 10;
- **reading:** "1002 allocs/run, unit COUNT (100,200 golib objects, 12,717,936 B over 100 runs) at 1aebd6a885, windows/amd64, Release TC0";
- **plan:** reflect/value_impl.cs `setKinded` takes the op as `ReadOnlySpan<byte>` and materializes it only on the panic path, measured to reach COUNT 2.

**Two cautions for COORD:**

1. **After the fix the pass rests on the counter's stated coverage boundary.** About 91 KB/run of uncounted CLR allocation remains: 60,000 B of nint/int64 boxes in SetInt and SetMapIndex's `.live` reads, plus the Dictionary's 30,936 B of arrays. That is the host's documented LOWER-BOUND meter, not a false green by its own rules, but it should be stated in whatever record banks the row.
2. **The current reason is wrong on the measurement.** It says "The managed map stores boxed keys and values, so each assignment allocates". In fact `SetMapEntry` allocates 0 B and the map stores `nint` unboxed. The counted objects are SetInt's op strings, and the uncounted ones are Value `.live` boxing.

---

## 4. Things that look like real bugs

1. **`reflect.Value.SetUint` is missing Go's kind check.** This is a behavioral divergence.
   - `value_impl.cs` `SetUint` is only `setKinded(v, x, "SetUint"u8)`, with no `mustBeKind`. Its siblings SetBool, SetInt, SetFloat and SetComplex all have one.
   - Measured: `New(TypeOf(int)).Elem().SetUint(5)` stores 5 and returns.
   - Go 1.24.13 panics with `reflect: call of reflect.Value.SetUint on int Value` (verified with a Go program on this box).
   - Because `TryConvertTo` accepts uint64 → int, the fallback panic in setKinded never fires. reflect's suite evidently does not cover this case.
2. **Every reflect scalar setter pays two counted objects and 72 B on the success path** (the `setKinded` op string; §3). It inflates any alloc assert reached through `Set{Bool,Int,Uint,Float,Complex}` corpus-wide, not only TestMapAlloc.
3. **The TestMapAlloc disclosure signature `allocs per map assignment: want ` prefixes BOTH blocks' failure texts.** Block 1's is "…want 0 got", block 2's is "…want at most 10 got". A block-1 regression would therefore be silently absorbed by the block-2 disclosure. If the entry survives, pin `allocs per map assignment: want at most 10 got `.

**Not a bug, noted:**

- **reflect's committed test sources at HEAD are older than the converter's current emission.** A reconversion rewrites 3 tracked files: `all_test.cs` (hoisted string constants, `OrTypedNil` wraps, `makeꓸꓸꓸ`), `package_test_info.cs` (its position map) and `set_test.cs` (`OrTypedNil`). The two measured tests' bodies are identical. The diff is saved as `<scratch>/reflect-alloc/filtered-run-outputs/tracked-diff.patch`, and all three files were restored.
- The map range snapshot being uncounted is by design (the ByDesign row), not a census gap.

---

## 5. Warm vs cold, tiering on vs off

| test | warm-up call (TC0) |
|---|---|
| TestMapIterSet | 4,920 B on the very first call (first-call caches), 3,312 B on later warm-ups |
| TestMapAlloc block 1 | 808 B first call, 0 thereafter |
| TestMapAlloc block 2 | 129,560 B / 1,003 obj first call, 127,176 / 1,002 thereafter |

AllocsPerRun's warm-up call absorbs all of these first-call costs.

**Tiering ON** (`DOTNET_TieredCompilation=1`, same harness):

- The counts are identical everywhere.
- TestMapIterSet reads **3,336 B/run** (+24 B). The whole difference is in `GetEnumerator` (96 B vs 72 B). This is consistent with tier-0 code not eliminating the struct-to-interface box in `map.IEnumerable.GetEnumerator` (`((IEnumerable<…>)this)`).
- Block 1 reads 88 B once over 100 runs.
- Neither difference changes a verdict or a label. It is one more reason the reading of record is TC0.

---

## 6. Tree state and artifacts

**Tree:**

- `git restore -- src/core/reflect/all_test.cs src/core/reflect/package_test_info.cs src/core/reflect/set_test.cs`
- Final `git status --porcelain` is empty; the tracked-change count is **0** and there are no `^ D` rows.
- The filtered run overwrote the git-ignored full-battery outputs (`go2cs_test_comparison.json`, `go2cs_test_manifest.json`, `go2cs_test_results.json`, `go2cs_test_results.xml`, all from 16:30-16:33 today). **They were restored from a backup taken before the run,** so a diagnostic filtered run cannot be mistaken for the full suite.
- Untracked or ignored outputs that stay in the tree: `src/core/reflect/bin/tests/**` and `obj/tests/**`. These were rebuilt by the filtered run from the reconverted, now-restored sources. They are build output only.

**Scratch** (`<scratch>/reflect-alloc/`):

| file | contents |
|---|---|
| `run1.log` | the host pipeline log |
| `filtered-run-outputs/` | the filtered run's comparison/results json and the tracked-file diff |
| `battery-backup/` | the restored originals |
| `cs-run-tc0-{1,2,3}.log`, `cs-run-tc1.log` | harness runs |
| `gotest.log` | Go's own run |
| `goharness/main.go`, `csharness/Program.cs` | the instruments |

bin/obj were purged after the report.

**Reproduce on this box:**

1. Pins as in the header.
2. `cd src/go2cs && go build -o bin/go2cs.exe .`
3. `go2cs.exe -tests -test-action all -test-timeout 30m -test-config Release -test-filter '^(TestMapIterSet|TestMapAlloc)$' -go2cspath '<tree>\src' '%GOROOT%\src\reflect' '<tree>\src\core\reflect'`
4. `dotnet build -c Release` the harness, then run `csharness.exe` with `DOTNET_TieredCompilation=0`.
5. `go build` and run the Go harness.
6. Restore the three tracked files and the four ignored outputs afterwards.
