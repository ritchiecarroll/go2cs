# SCOUT: converted TypeScript against native Go, where the time goes

Lane P4, 2026-11-13. Ordered by COORD (mailbox, 2026-11-13 14:02Z) at the owner's request. This is measurement only:
no S item below is implemented, and nothing of TypeScript's source or the converted C# is committed here. The data
and tools are in [`data/ts-perf/`](data/ts-perf/).

## 1. Setup

- **Code under test:** go2cs master `651c4ad147`, converting microsoft/TypeScript `ba29a56299` (module
  `github.com/microsoft/TypeScript/tsc`). The conversion used `-recurse`, local refs, and the labelled xxh3 `replace`.
  The five refused packages are skipped as usual.
- **Builds:** the four demos (`parsedemo`, `checkdemo`, `emitdemo`, `mfdemo`), Release, `GoTargetOS=linux`, JIT. The
  runtime settings are the template defaults: tiered compilation with dynamic PGO, workstation concurrent GC, and
  `CallCountingDelayMs=0`.
- **Native side:** the same demos built by go1.26.10 from the unmodified module.
- **Box:** a cloud linux/amd64 VM with 4 vCPU (Xeon at 2.1 GHz) and 15 GB of RAM, running .NET SDK 10.0.400. Nothing
  else was running during the timed runs.
- **Method:** `tools/bench.py` runs one process per run, with 1 warm-up and then 5 timed runs, and reports the median.
  It takes wall time, CPU time and peak RSS per process from `wait4`. Every run's stdout is hashed, and every
  converted run matched native's hash.
- **Inputs:**
  - every 74th unit of the 7,404 compiler-test units (101 files, `data/ts-perf/units-101.txt`), each checked or emitted
    as its own program, all in one process;
  - the 17,422-unit parser corpus;
  - the `shop` and `legacy` tsconfig projects;
  - `good.ts` for the start-up cases.

## 2. Baseline

| Case | What runs | Go wall | C# wall | Ratio, wall | Ratio, CPU | C# peak RSS |
|:--|:--|--:|--:|--:|--:|--:|
| hello | `parsedemo good.ts`: process start plus one small parse | 0.004 s | 2.08 s | 519x | 810x | 223 MB |
| startup | `checkdemo good.ts`: one small program type-checked, lib.d.ts included | 0.068 s | 5.35 s | 78.6x | 103x | 385 MB |
| (a) parse | the 17,422-unit corpus, digest per unit | 1.97 s | 26.6 s | **13.5x** | 17.2x | 1,049 MB |
| (b) type-check | 101 units, every diagnostic | 4.66 s | 119.2 s | **25.6x** | 22.6x | 1,510 MB |
| (c) emit | the same 101 units, JS and `.d.ts` | 4.28 s | 118.7 s | **27.7x** | 24.3x | 1,206 MB |
| (d) multi-file | `mfdemo shop legacy` | 0.51 s | 17.5 s | **34.2x** | 45.9x | 1,013 MB |

The spread between runs is small. Type-check C# ranged 115.2 to 121.7 s, and Go ranged 4.61 to 4.77 s
(`bench-master.json` has every run).

The remembered 37x matches the type-check and emit range once a process start per batch is added. The type-check
pass of record ran in batches of 20 units, each paying the start-up cost.

**Native AOT** did not build on this box. ILC stopped with "Insufficient memory" after 722 s at 15 GB, so there is no
AOT column. **ReadyToRun** built (self-contained, 111 s) and changed nothing that matters: start-up went from 4.89 to
4.57 s, and 25 type-check units from 35.8 to 36.1 s.

**Predictions** (posted before measuring, mailbox `f6cb45fd8`), scored as worded:

| | Prediction | Measured | Score |
|:--|:--|:--|:--|
| P1 | startup 50 to 150x, point 80x | 78.6x | MET |
| P2 | parse corpus 6 to 10x, point 8x | 13.5x | MISSED |
| P3 | type-check 15 to 35x, point 22x | 25.6x | MET |
| P4 | emit 15 to 35x, point 22x | 27.7x | MET |
| P5 | multi-file 20 to 60x, point 35x | 34.2x | MET |
| P6 | AOT start-up under 10x, throughput within 30% of JIT | AOT did not build | not scored |
| P7 | 10 to 30% of time in GC; 3 to 10x Go's bytes | GC pauses are 37% of wall (42.0 of 114 s); Go's bytes not measured | MISSED (first half) |
| P8 | top 3: map hashing, interface dispatch, ж/escape allocation, with GC in the top 5 | GC is first. Field views (ж) and struct hashing are high. Interface dispatch is not in the top 20 | MISSED |

P2 note: at batch 286 the same corpus measured 22.0 s against 2.8 s (7.9x). That was a single unbenched run, and it
was the prior I predicted from. I do not claim a regression from it.

## 3. Where the time goes (type-check, 101 units)

From `dotnet-counters` (`counters-check.txt`):
- the run allocated **28.0 GB**;
- GC pauses took **42.0 s** of the 114 s wall;
- there were 116 gen2 collections (`alloc-check.txt` counts 239 GCs);
- the JIT compiled 68,852 methods, using 35.5 s of background-thread CPU.

CPU sampling (`cpu-check.txt`, EventPipe thread-time at about 100 Hz) puts **66% of the main thread's samples inside
`Thread.PollGC`**, which is the thread waiting at a safe point while the GC runs.

That has one consequence for reading every other row. An allocation site's inclusive time includes whatever GC pause
happened to begin there, so inclusive time overstates what removing the site would save. The experiments in §4 are
the evidence for each estimate. Inclusive shares are given only to locate the sites.

Allocation by source, from the gc-verbose trace (`alloc-check.txt`):

| Share of bytes | Allocated by | go2cs construct |
|--:|:--|:--|
| 38.8% | `go.sstring.op_Implicit(sstring)`, called from `utf8.DecodeRuneInString` | an sstring twin whose callee takes `@string` copies the rest of the source text for each non-ASCII character, and the copies go on the large-object heap |
| about 37% | `ж<T>.viewOfSlow`: `FieldRefBox<…Base>` and `ViewEntry`, one per embedded hop of each AST node | the field-view cache: views are minted and kept alive per (box, field) |
| 5.9% | `slices.Grow` | slice growth (as in Go) |
| 2.2% | `core.New(Arena)` | arena node allocation (as in Go) |
| 1.6% | `ast.GetSymbolTable` | named-map `== nil`: two boxes per call (see S3) |

Exclusive CPU after PollGC, in order:
- `Buffer.Memmove`, 4.3%: mostly the copies above.
- `ValueType.GetHashCode`, 2.2%: struct keys without a hash override. Mainly `checker.CacheHashKey`, a named struct
  over `xxh3.Uint128`, and `CachedSignatureKey`.
- `viewOfSlow` over the AST base types, about 12% across all node types.
- `Coro.Start` and `Coro.Switch` together, 2.4%: range-over-func.

`cpu-check-by-type.txt` folds the frames by type.

**Parse corpus** (`cpu-parse.txt`, `alloc-parse.txt`): 63% PollGC. The demo's own digest (fmt plus sha256) is a large
share of this case. `fmt.doPrintf` and `handleMethods` allocate 24% of the bytes, and sha256's portable
`blockGeneric` takes 6.3% of CPU, where Go uses SHA-NI assembly. The field views allocate about 25%. `Coro.Start` in
`finishSourceFile` costs about 4% (a thread start per file, through `getCommentPragmas`'s range-over-func).

**Start-up** (`cpu-hello.txt`): 1.9 to 2.1 s before any real work.
- About 0.46 s of the main thread is fmt's first type assertion building Go method sets by reflection
  (`TypeExtensions.StructurallyImplements`, `GetExtensionMethodsHomedIn`).
- The rest is module initializers (`<Module>..cctor`, `ᴛRegisterAdapters`, `GoDeferredRegistrations.Run`) and the
  JIT. ReadyToRun did not move it.

## 4. Experiments

All arms run `checkdemo` over the first 25 of the 101 units, with 1 warm-up and the median of 3 (`tools/envrun.py`).
Every run produced the same output hash. Nothing was committed: each arm is a runtime setting, a startup hook, or
one rebuilt assembly copied over a copy of the master build. The full table is in `experiments.tsv`, and the knob
sweep in `runtime-knobs.tsv`.

| Arm | Wall | vs base | CPU | Peak RSS |
|:--|--:|--:|--:|--:|
| base (master build) | 35.8 s | | 54.4 s | 1,252 MB |
| server GC (`gcServer=1`, DATAS off) | 24.4 s | **−32%** | 50.4 s | 1,447 MB |
| server GC, DATAS on (the .NET 10 default for server) | 25.3 s | −29% | | |
| `gcConcurrent=0` | 33.2 s | −7% | | |
| `GCgen0size=256 MB` | 35.4 s | none | | |
| TieredPGO off / TieredCompilation off | 37.0 / 37.3 s | +3% / +4% | | |
| field-view cache off (startup hook sets `FieldViewCache.Disabled`) | 30.8 s | **−16%** | | |
| utf8 slow path takes `sstring` (rebuilt `go.unicode.utf8.dll`) | 34.1 s | −5% | | |
| named-map `== nil` without reflection (generator patch, rebuilt `ast.dll`) | 33.5 s | −2% | 51.5 s | 1,228 MB |
| utf8 fix + view cache off | 28.0 s | −22% | 45.1 s | 1,083 MB |
| utf8 fix + view cache off + server GC | 22.7 s | **−37%** | 40.8 s | 3,223 MB |

The utf8 fix removes the 39% of bytes and halves the GC count: 22 GCs to 11, and gen2 17 to 5, on a 5-unit trace.
Even so, the wall time moves only 5%. The named-map fix removes every reflective `ValueType.Equals` call (0.0% in a
trace of that build), yet it gains only 2%, despite a 19.6% inclusive share at master. The GC cost follows the size of
the live heap, not the bytes allocated. That is why the view cache, which keeps objects alive, is the larger lever.

## 5. Items

### S: simple, low risk, no visible emission change

**S1. Server GC for converted executables.** −32% wall and −7% CPU on type-check; +15% peak RSS, and much more when
combined with the other fixes. The change is a `csproj-template.xml` property, overridable the way
`GoCallCountingDelayMs` is, for example `<ServerGarbageCollector Condition="'$(ServerGarbageCollector)'==''">true</…>`
plus `GCDynamicAdaptationMode=0`. It changes no code, but it changes the resource profile of every converted app: one
GC heap and one thread per core, and more memory. That makes it a policy choice as much as a code change. The
alternative is to document it as an opt-in.

**S2. The `unicode/utf8` sstring twin copies its input.** `DecodeRuneInString(sstring)` calls
`decodeRuneInStringSlow(@string)` (`src/core/unicode/utf8/utf8.cs:231`), so the implicit `sstring`→`@string`
conversion copies `s`. Every call site in the TypeScript scanner passes `text[pos:]`, so each non-ASCII character
copies the rest of the file.
- Measured: −5% wall, and 39% fewer bytes.
- Smallest fix: add `"unicode/utf8.decodeRuneInStringSlow": {0}` to the twin list
  (`src/go2cs/sstringTwinOperations.go:82`). This moves the unicode/utf8 emission by one parameter type plus a
  generated forwarder.
- Durable fix: have `validateSStringTwin` refuse a twin body that passes its `sstring` parameter to an `@string`
  parameter, since that is a hidden copy. That would also catch any fmt twin with the same shape (not audited here).

**S3. A named map's `== nil` goes through reflection.** `InheritedTypeTemplate.NilComparisonExpression` emits
`value.Equals(default(S))` for Map and Channel (`src/gen/go2cs-gen/Templates/InheritedType/InheritedTypeTemplate.cs:262`).
The wrapper struct has no `Equals(S)`, so the call boxes both values and runs `ValueType.Equals`, which reads fields by
reflection.
- Fix: delegate as the Slice arm does, `"Map" => $"value.{Value} == nil"` (map<K,V> has `operator ==(map, NilType)`),
  and the same for Channel if channel<T> has the operator.
- Measured: −2%. It is generated code, so goldens do not move.

**S4. Generated struct equality boxes.** Generated structs declare `Equals(T)` and `GetHashCode` but not
`IEquatable<T>` (`StructTypeTemplate.cs:153`). `Dictionary` therefore takes `ObjectEqualityComparer`, which boxes the
probe key on every comparison. Their `GetHashCode` calls `golib.HashCode.Combine(params object[])`
(`src/core/golib/runtime/HashCode.cs:25`; `StructTypeTemplate.cs:2739`), which allocates an array and boxes every
field.

A named type over a struct, such as `CacheHashKey`, has neither override, so it falls back to `ValueType.GetHashCode`
and `ValueType.Equals`.

Fixes:
- add `IEquatable<T>` to the struct base list;
- give golib typed `Combine<T1…T8>` overloads, which the emitted call binds with no emission change;
- give struct-kind named wrappers the `EqualityOverrides` that holders already get (`InheritedTypeTemplate.cs:676`).

Not measured as an arm. `ValueType.GetHashCode` is 2.2% exclusive and `HashCode.Combine` 0.2%, so I expect a few
percent at most.

### B: queued (design, visible emission, or corpus-wide)

**B1. Field-view retention (the largest measured lever after GC mode).** About 37% of the bytes are `FieldRefBox` and
`ViewEntry`, minted once per (box, field) and kept alive for the box's life (`src/core/golib/ж.Views.cs`; the design
is `DESIGN-field-view-cache.md`, ruled f1083bff5).

TypeScript's AST embeds about 9 levels deep (`Identifier` → `PrimaryExpressionBase` → … → `NodeBase` → `NodeDefault`),
so each node gains a chain of views. Switching the cache off, so views are minted per call and die young, was 16%
faster here. That inverts the design's micro-benchmark, which measured per-call cost and not retention.

Options:
1. revisit the gate, for example no caching for views of embedded struct fields of a boxed struct, or a weak slot;
2. better, a converter emission that reaches a promoted method's embedded receiver by `ref` into the parent box, so no
   view object is minted at all. This is the ref-primary direction.

It is a ruled design with corpus-wide reach, so it is B.

**B2. Range-over-func starts a thread.** `for x := range seq` becomes a `foreach` over `YieldFunctionEnumerable`,
which runs `seq` on a `Coro`: a dedicated thread plus a semaphore handoff per element (`golib/runtime/Coro.cs:56`,
`YieldFunctionEnumerator.cs`). That costs 2.4% in type-check and about 4 to 5% in parse, mostly thread start-up in
`finishSourceFile`/`getCommentPragmas`.

The class remarks already name the fix: lower the loop into a direct call of `seq`, with the body as the yield
callback and `break`/`continue`/`return`/`goto` carried as state. It is a visible emission change.

**B3. Start-up, about 2 s per process.** Go method sets are discovered by reflection on the first interface assertion
(0.46 s for fmt's first `Stringer` check), plus module initializers and adapter registration. ReadyToRun does not
help, so the cost is the work itself and not the JIT.
- Fix direction: generator-emitted method-set tables per assembly in place of reflection over extension hosts.
- This dominates small runs: `hello` is 519x and `startup` 79x.

**B4. Native AOT for large closures.** AOT is the other answer to start-up, and it did not build here: ILC needed more
than 15 GB for checkdemo's closure. The work would be to measure ILC's peak on a larger box, then trim what the
closure roots by reflection.

**B5. The parse demo's digest.** fmt's reflection-driven formatting (24% of the bytes in parse) and sha256 without
SIMD (6.3% of CPU) are stdlib costs that this demo's harness exposes; TypeScript itself is not involved. sha256 could
take `System.Security.Cryptography.SHA256` behind the converted API, a hand-own like crypto's others. Out of scope for
TypeScript; noted for the stdlib.

**B6. Allocation volume in general.** After S2 and B1, the remaining bytes are slices (`slices.Grow` 5.9%), arena
nodes, `ElemRefBox` element views (about 3%) and symbol tables. These mostly mirror Go's own allocations, and the GC
cost they carry follows the live heap. There is no single cheap item left in that group.

## 6. Reading

- **Correctness:** none of the experiments changed a byte of output.
- **Cheapest large win:** S1, the GC mode, at a memory cost the owner should weigh.
- **Cheapest code fixes:** S2 to S4, worth a few percent each. S2 also halves the GC count, which matters more on
  larger inputs.
- **Biggest structural item:** B1, the field-view retention, at 16% measured.
- **Fixed cost of small runs:** B3, start-up.

Taken together, S1, S2 and B1 brought 25 units from 35.8 s to 22.7 s. At that rate type-check would be about 16x Go
instead of 25.6x. That figure is an extrapolation from the 25-unit arm and has not been measured on the 101 units.
