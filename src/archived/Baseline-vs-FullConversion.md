# Baseline vs. Full Conversion — the separation contract *(SUPERSEDED 2026-08-01)*

**Archived.** This page is a historical record and is not kept current.

> **⚠ The separation this document names is OVER.** On **2026-08-01** the stub baseline retired and the
> converted standard library moved home to **`src/core`** (commit `2e8066da6`). There is now ONE tree and
> ONE path scheme — `$(go2csPath)core\<pkg>`, the reference the converter always emitted — and none of the
> rewrite / overlay / remap machinery described below survives. See [`/CLAUDE.md`](../../CLAUDE.md#one-tree-read-before-touching-srccore) *One tree*.
>
> This file is kept because parts of it are still **live doctrine**, not history: *Hand-owning a package to
> make it OPERATIONAL* (the `[module: GoManualConversion]` marker, the `*_impl.cs` companion, the
> `.cs.auto` review sibling) and *Child-process creation*. Read the separation-era sections as the record
> of how the project got here — and, for §5's overlay rules and *Regenerating the full conversion*'s
> two-root warning, as machinery that no longer exists. Wherever a path below reads
> `src/go-src-converted/…`, it is now `src/core/…`.

## How it used to be laid out

1. **Baseline stdlib — `src/core/<pkg>`**
   Small, **hand-finished, compiling** subset of the Go standard library. This is what the behavioral
   tests and converter-improvement loop built against; it had to stay green.

2. **Full auto-conversion — `src/go-src-converted/`**
   The entire Go standard library (302 packages, Go 1.23.1) auto-converted by `go2cs -stdlib`. The
   **ultimate goal — and as of 2026-07-10 all 302 packages compile clean** (commit `51ba5d9cf`, tag
   `stdlib-green-2026-07-10`; the Phase-3 milestone). Compiling, not yet operational — running Go's own
   package tests is Phase 4.

3. **Runtime — `src/core/golib/`**
   Hand-written C# runtime (`slice`, `map`, `channel`, `@string`, `builtin`, `ж<T>`, type aliases).
   **Shared by both** baseline and full conversion. **Never auto-overwritten** — some of it (`builtin`,
   `unsafe` helpers, assembly-backed routines) can never be produced by transpilation. *(Still true, and
   still at the same path.)*

### Why they had to stay separate

Both baseline and full emit into `namespace go` with `<pkg>_package` static partial classes. Referencing
both from one C# project produces duplicate-type collisions. So they were kept in **separate directories**
and **never referenced together** by a single project. *(The collision rule itself is permanent; what
changed is that there is no second tree left to collide with. The one place it still bites is `testing` —
which is why the hand-owned test host is skip-listed from conversion instead of living beside an
auto-converted twin.)*

## How the collision happened (history)

| Commit | Date | Event |
|---|---|---|
| `9792eeea2` | 2020-07-09 | Hand-converted stub created at `src/gocore/<pkg>` (Tour-of-Go support). |
| *(many)* | 2020–2025 | Stub maintained/refined for years; it was the working library. |
| `ba6fef6c9` | 2025-03-08 | `src/gocore` renamed → `src/core` (path change only). |
| **`3426298eb`** | 2025-05-05 01:51 | **Last clean baseline.** Stub compiles; tests green. |
| `6ca1c45b7` | 2025-05-05 01:59 | "Initial standard library conversion" — full stdlib written **on top of** `src/core`, overwriting the hand-finished packages (2,359 files, +508k lines). |
| `cc14584c7` | 2025-05-11 | Full-conversion work; tagged `full-conversion-2025-05`. |
| 2026-06-25 | 2026-06-25 | **Separation restored:** full conversion relocated to `src/go-src-converted/`; old stub restored into `src/core`; converter fixes; green baseline. |

The mistake was writing the full conversion **into the same directory** as the baseline instead of a
separate one. "All 305 packages converted successfully" meant the **transpiler did not crash** — not that
the emitted C# compiles. The overwrite replaced *compiling* `fmt`/`time`/etc. with *large machine-generated*
versions, which stalled the test loop.

The project was **originally designed** with this separation (`gocore` manual subset + `go-src-converted`
full auto-output), so restoring it realigns with the original design.

## How it was resolved (2026-06-25)

- Relocated the full conversion out of `src/core` into **`src/go-src-converted/`** (a 2604-file git rename);
  rewrote inter-package `csproj` refs and `go2cs.sln` paths; added `.gitignore` rules for the Go `debug`/
  `log` packages that collide with the VS `[Dd]ebug/`/`[Ll]og/` patterns.
- **Restored the old hand-finished stub from `3426298eb` into `src/core`.** Key finding: it **compiles
  cleanly against today's `golib`** — the feared API drift did not materialize, so it gave a green baseline
  immediately. Restored 14 packages; excluded the stub `testing` (drifted, 400 errors, referenced by no test).
- Scoped **`src/go2cs.sln`** to the baseline + tests; added **`src/go-src-converted.slnx`** for the 301 WIP
  projects.
- Result: `go2cs.sln` builds 79/79; behavioral suite green (216 tests).

## The contract (rules while the two trees existed — RETIRED 2026-08-01)

*Rules 1–3 and 5 below described the two-tree world. What replaced them: `src/core/<pkg>` is converter
output and is regenerated wholesale; hand-owned files simply live there and are protected by the
`[module: GoManualConversion]` marker (rule 4's promotion question was answered by moving the whole tree
at once); `golib` is unchanged; and there is no overlay step, because a reconvert writes exactly the
repository's own paths.*


1. **`src/core/<pkg>` is curated and must compile.** Treat it as hand-owned source. Do not bulk-overwrite
   it with `-stdlib` output.
2. **`src/go-src-converted/` is the full-conversion target.** All `go2cs -stdlib` runs write here via
   `-go2cspath`. It may be regenerated wholesale; nothing hand-edited lives here long-term (fixes belong in
   the converter or, for out-of-band pieces, in `golib`).
3. **`golib` is shared and never auto-generated.** Both trees reference `src/core/golib/golib.csproj`.
4. **Promotion `go-src-converted → core` is DEFERRED (strategy correction, 2026-07-01).** Earlier work
   promoted packages into `core` as they went *green* (compiling). That was premature — **compiling is not
   operating.** Promotion should happen only once a package's **converted Go unit tests pass** (Phase 4),
   and may not be needed at all (see *The corrected end-state* below). Until then, `core` stays the small
   bootstrap **stub** the behavioral tests build against (chicken-and-egg — the tests need a working library
   to run, and `go-src-converted` compiling doesn't yet mean it *works*). `sync/atomic` already living in
   `core` is fine — it remains a useful stub. **Do not promote further** on the basis of a clean compile.
   The converter is never pointed at the baseline directory.
5. **The canonical MANUAL files live in `core` and are copied BACK into `go-src-converted`.** Files marked
   `[module: GoManualConversion]` (the converter skips re-converting them) and hand-written `*_impl.cs`
   files are hand-owned in `src/core/<pkg>`. For a full-conversion **milestone** to be complete, these must
   be overlaid into their matching `src/go-src-converted/<pkg>` locations — that overlaid tree (auto-output
   + manual/asm stubs) **is the real final state.** `overlay.sh` already re-copies the `src/core` manual
   files after the cs/csproj copy; during these final compiling stages, do this **religiously**. The overlay
   must also copy the `<name>.cs.auto` review siblings the reconvert produces (a bare `*.cs` glob misses
   them) — and a reconvert only *produces* them when its output dir was seeded with the marked hand-owned
   files first, since the marker gate probes the destination `.cs` (see *Hand-owning a package…* below).
   Seeding is **safe** (2026-07-17 fix): a marked file is still analyzed and visited with its package —
   only its emission is redirected to the sibling — so every unmarked file of a seeded reconvert emits
   byte-identical to an unseeded run.

## The corrected end-state (2026-07-01) — compile first, operate later

The **milestone** is a **clean C# COMPILE** of the whole overlaid `go-src-converted` (auto-output + the
manual/`*_impl.cs`/asm stubs) — *not* an operational one. Operational correctness is Phase 4 (converting +
passing the Go unit tests). Getting there, for `runtime`:

- **Native-type pointer/unsafe ops are convertible.** Go and C# are both GC languages with pinning and
  unsafe pointers; native types share identical memory operations. Pointer parity for native types is the
  goal and is achievable (the hand-converted `unsafe`/`sync/atomic` code proves the overlap). Fix these in
  the converter/`golib` properly.
- **Managed-referent cases have a known model.** Where Go stashes a *managed* pointer inside a `uintptr`
  (`guintptr`/`muintptr`/`puintptr`…) to hide it from the GC, the C# equivalent holds the `ж<T>`/`object`
  **directly** (Volatile/Interlocked + `nilCanon`), never a `nuint` round-trip — exactly as
  `core/sync/atomic/type.cs`'s `atomic.Pointer<T>` and `reflectlite/value.cs`'s `object? m_target` do. A
  raw `uintptr` cannot hold a managed reference across a GC (the "compiles-but-crashes" trap).
- **Raw-metal on NON-native types is the dragon — stub it.** Memory-layout math, type-descriptor
  pointer-walking, and `*.asm` cannot be faithfully transpiled. When the loop hits this wall, the file gets
  an **immediate `[module: GoManualConversion]` task / review** — a hand-written C# equivalent, or a
  throwing stub that **won't exist in the final build** — not a converter fight. A `GoManualConversion`
  stub that makes the package COMPILE is an acceptable milestone solution; the faithful hand/asm
  implementation can follow.

So the loop no longer *stops* at the S1/CS0030 "architectural wall" — it **sorts**: convert the native-type
ops, apply the managed-referent model, and stub the genuine raw-metal dragons with `GoManualConversion`.

Versioned builds of the full conversion ship to **NuGet** as `go.<pkg>` / `go.lib` / `go.gen`. Once the
converted **Go tests** pass broadly enough, the chicken-and-egg is gone and `core` can be dropped
(behavioral tests reference NuGet) or replaced with prior operational `go-src-converted` source — TBD.

## Hand-owning a package to make it OPERATIONAL (Phase 4) — two patterns + the marker

Phase 3 stubbed the raw-metal dragons just to *compile*. Phase 4 (making packages *run*) needs the opposite
in places: a **faithful native reimplementation** where the literal Go→C# conversion can compile but cannot
work. The canonical case is **`sync`** (2026-07-11): its concurrency types are a state machine over the Go
**runtime sleeping semaphore** (`//go:linkname` `Semacquire`/`Semrelease`/`notifyList`/…), which is
co-designed with the mutex (starvation-mode ownership handed to one specific waiter via an exact ticket) and
**cannot be emulated on any .NET primitive** — every emulation deterministically trips `sync: inconsistent
mutex state` / `unlock of unlocked mutex` under sustained contention. The fix is to reimplement the *types*
natively on proven .NET primitives (`Mutex`→binary `SemaphoreSlim`, `WaitGroup`→counter+latch,
`RWMutex`→writer-preferring monitor lock). Expect more of this in Phase 4 (`time`, parts of `os`/`syscall`, …).

**"Proven .NET primitive" is not the same as "faithful" — `Cond`'s notify list (2026-07-25).** The first
`runtime_impl.cs` backed the `notifyList` linknames (`notifyListAdd`/`Wait`/`NotifyOne`/`NotifyAll`, which
`sync.Cond` is built on) with a plain counting `SemaphoreSlim`, reasoning that "Cond need not wake any
particular waiter". That reasoning is wrong, and Go has a test named after the failure: the runtime's list is
**ticketed** — `notifyListAdd` hands out a monotonic ticket and `NotifyOne` releases *that specific ticket's*
waiter. With banked permits instead, a waiter that arrives AFTER a Signal/Broadcast can consume the permit an
already-parked waiter was owed. `TestCondSignalStealing` (1000 iterations of exactly that race) then wedged
the whole suite, and `TestCondBroadcast` reported "goroutine woke up twice" because a goroutine looping back
into `Wait` could take a leftover Broadcast permit in the same round. The port is now faithful: a per-ticket
parked-waiter list, `NotifyOne` signalling the holder of ticket `notify`, `NotifyAll` draining the list and
setting `notify = wait`, and — covering the not-yet-parked race the same way Go does — a `Wait` whose ticket
is already behind `notify` (wraparound-safe signed comparison, Go's `less`) returning immediately instead of
parking. All four `sync.Cond` tests pass on it. The lesson generalizes: when the Go primitive's contract names
a *specific* waiter, a counting primitive is a divergence, not an implementation detail.

**`sync.Pool` — the raw-metal type can be the `any` itself (2026-07-26).** The same wall in its third
shape, and the sharpest one: `poolDequeue`'s ring stores `eface` slots — the two-word `{type, value}`
form of an interface — and keys ownership on the TYPE word (empty **iff** `typ == nil`; a consumer
releases a slot by storing nil into `typ` alone). An `any` under the CLR is ONE reference, so the
literal conversion reinterprets the struct as an `any` and the type word does double duty as the value:
a stored value reads back as its own type descriptor (`panic: interface conversion: interface {} is
unsafe.Pointer, not int`, which took the whole test host down and cost the 14 tests sorting after
`TestPoolChain`), and the empty-slot sentinel becomes indistinguishable from a stored value of that
type. The hand-owned `poolqueue.cs` forks **only** the slot — one managed reference, `null` as the
empty sentinel, a singleton standing in for Go's typed-nil `dequeueNil` marker, and Go's two-step
release collapsed into the single write a one-word slot makes atomic — leaving the packed head/tail,
the fullness test, the CAS protocol and the whole `poolChain` half untouched. `pool.cs` was already
hand-owned (its `[P]poolLocal` block is reached by pointer arithmetic through an `unsafe.Pointer`) and
is now a faithful port of Go's algorithm rather than a `ConcurrentBag`: private slot → shared chain →
steal → victim cache, with `poolCleanup` registered exactly as Go registers it. Two divergences are
stated in the file and in
[`ConversionStrategies-Reference/manual-conversions.md`](../../docs/ConversionStrategies-Reference/manual-conversions.md#syncpool--a-managed-reference-ring-slot-and-a-thread-affine-stand-in-for-the-p-pin):
`procPin` becomes a **thread-affine** shard index (so two threads can share a shard, which Pool closes
with interlocked private slots and a per-shard producer gate), and the cleanup is triggered by
*requested* collections rather than every GC cycle. `sync` goes 21 → 35 of 50 on this arc.

**`internal/concurrent.HashTrieMap` — the wall is a DESCRIPTOR READ, and the remedy is semantics over
mechanism (2026-08-03, user-ruled).** Go's hash-trie is seeded entirely from one runtime descriptor:
`NewHashTrieMap` takes `abi.TypeOf(m).MapType().Hasher` — a raw function pointer into the hashing machinery
the compiler emits for `map[K]V` — plus `Key.Equal`/`Elem.Equal`, its bit-compare thunks. All three take
`unsafe.Pointer`s and mean "hash / compare the bytes AT this address", which the managed reflection bridge
cannot honor: an address in the CLR names no value (two boxes holding equal strings sit at different
addresses; a pointee containing references moves across a GC), so an address-derived hash would stop
`unique.Make(x)` agreeing with itself — the inverse of the package's purpose. Populating `Hasher` with
something plausible is barred by the standing **inverse-atomic rule** (a descriptor field whose read cannot
be honored must stay EMPTY), so the literal conversion compiles and can never run: `NewHashTrieMap` threw
inside the package initializer of every `unique` consumer, and took `net/netip` — and `encoding/gob`'s
`TestNetIP` — with it. The whole file is hand-owned on the `sync` precedent, and the rewrite keeps only the
**semantics**: the exported API and its concurrency contract are preserved exactly, while the trie itself is
replaced by a `ConcurrentDictionary` whose guarantees line up member for member (LoadOrStore's single-winner
`TryAdd` retry, CompareAndDelete's atomic compare-and-remove pair overload, All's weakly-consistent
enumeration). Go's `keyHash`/`keyEqual`/`valEqual` triple becomes `EqualityComparer<K>/<V>.Default`, which is
verified to BE Go's `==` for every key shape the corpus interns — a `ж<T>` compares by pointer identity with
a matching identity hash and `abi.TypeFor<T>()` interns one descriptor box per `System.Type`; a `[GoType]`
struct carries a generated field-wise `Equals` plus a `HashCode.Combine` of the same fields; `@string`
compares and hashes by content. Detail, and the two walls it uncovered behind it (a cross-assembly
`//go:linkname` **PUSH** that never links, and `abi.TypeFor<T>()` returning the wrong object for an
INTERFACE `T`):
[`ConversionStrategies-Reference/manual-conversions.md`](../../docs/ConversionStrategies-Reference/manual-conversions.md#internalsynchashtriemap--a-managed-map-where-go-seeds-itself-from-maptypehasher).

⚠ **A single-Go-file package that hand-owns that file becomes FULLY hand-owned.** `unmarkedFileCount == 0`
makes the driver `continue` before `writeProjectFile`, so the package's `.csproj`, `package_info.cs` and
`README.md` are never re-emitted either — the position `internal/godebug` was already in, and now
`internal/concurrent` too. That is *stronger* protection than the marker alone (a seeded reconvert leaves
every file in the package byte-identical, proven both directions: strip the marker and the converter
overwrites `hashtriemap.cs` with its own 21 KB emission and rewrites `package_info.cs`), but it also means
those three files are yours from then on — `internal/concurrent`'s `package_info.cs` had to drop the
`<TypeAccessibility>` entries for the trie's `node`/`entry`/`indirect`, which no longer exist. One caveat:
the `.cs.auto` review sibling is NOT produced for such a package when the file is generic —
`emitAutoConversionSiblings` runs only six of the whole-package pre-passes and panics on `hashtriemap.go`
(*"visit file error: … nil pointer dereference … auto-conversion sibling skipped"*). Pre-existing, harmless
to the protection, board-rowed.

There are **two** ways a package carries hand-owned C#, and they are NOT interchangeable:

1. **`*_impl.cs` supplement — for SOME declarations in a file.** The converter emits the file normally but,
   for types/funcs listed in `manualConversionTypes` / `manualConversionFuncs` (`manualTypeOperations.go`),
   replaces the body with a `// … hand-converted … see the package's *_impl.cs` comment and a bodyless
   `partial`. A hand-written `<name>_impl.cs` companion (no matching `.go`, so a reconvert never touches it)
   supplies the real bodies. Use when only part of a converted file needs managed semantics (e.g.
   `sync/atomic`, `runtime/lock_sema`). The `*_impl.cs` file typically also carries `[module:
   GoManualConversion]` for documentation, but does not *need* it (nothing regenerates it).

2. **Whole-file replacement — for an ENTIRE file, and it REQUIRES the marker.** When the whole `<name>.cs`
   is hand-written (replacing the converted `<name>.go` output — e.g. sync's `mutex.cs`/`waitgroup.cs`/
   `rwmutex.cs`), it MUST carry `[module: GoManualConversion]`, or a `-stdlib` reconvert regenerates the Go
   version straight over it. `main.go`'s conversion loop calls `containsManualConversionMarker(<output>.cs)`
   for each `.go` file and **drops that file from the conversion set when the marker is present**
   (`directiveOperations.go`). This is the ONLY thing that makes a whole-file native reimplementation
   durable across reconverts.

   Complete inventory of whole-file replacements (every non-`*_impl` file carrying a real module-level
   marker in `src/go-src-converted`; grep-verified 2026-07-16, `poolqueue.cs` added 2026-07-26): sync
   `mutex.cs` / `waitgroup.cs` /
   `rwmutex.cs` / `pool.cs` / `poolqueue.cs`; runtime `runtime2.cs` / `mfinal.cs`; syscall `dll_windows.cs` /
   `exec_windows.cs` (2026-07-19 — `StartProcess` only; see *Child-process creation* below);
   internal/concurrent
   `hashtriemap.cs` (2026-08-03 — a ConcurrentDictionary-backed reimplementation replacing the hash-trie,
   because Go seeds the trie from `MapType().Hasher`, a descriptor field the managed bridge must leave
   empty; the marker makes this the package's *only* Go file to be hand-owned, so its `.csproj`,
   `package_info.cs` and `README.md` stop being re-emitted too — see above); internal/godebug
   `godebug.cs` (2026-07-17, blocker R2 — parses $GODEBUG once on first use instead of the Go runtime's
   update-hook cache; the literal conversion's embedded `*setting` promotion faults at runtime because the
   generated promoted-field box treats its held nil pointer as a nil dereference even for the assignment
   that would populate it; subsumed and removed the older `godebug_impl.cs` hook companion) (these live in
   `go-src-converted` only); sync/atomic `type.cs` / `value.cs` and unsafe `unsafe.cs` (canonical in
   `src/core`, byte-identical copies in `go-src-converted`); math `unsafe.cs` (2026-07-16 — Float32/64
   bits/frombits as direct `BitConverter` bit-cast intrinsics, replacing the literal conversion's
   `ж<T>`/`uintptr` round-trip that compiles but cannot reinterpret bits at runtime; canonical in
   `src/core/math`, byte-identical copy in `go-src-converted/math`; guarded by the `MathFloatBits`
   behavioral test). Several `*_impl.cs` companions also carry the marker (documentation only, per
   pattern 1): internal/abi `type_impl.cs`, reflect `value_impl.cs`, runtime `lock_sema_impl.cs` /
   `runtime2_impl.cs`, sync `runtime_impl.cs`, internal/poll `runtime_sema_impl.cs`, syscall
   `syscall_impl.cs` (2026-07-19) and `zsyscall_windows_impl.cs` (2026-08-01 —
   `GetTimeZoneInformation` only; the struct-passing seam, see *Child-process creation* below),
   math/rand + math/rand/v2 `rand_impl.cs` (2026-07-17,
   blocker R3 — `runtime.rand` linkname bodies on `Random.Shared`: OS-entropy seeded, thread-safe,
   non-deterministic run to run exactly like Go's runtime generator), os `tempfile_impl.cs`
   (2026-08-01 — the same `runtime.rand` linkname, reached by `nextRandom`, so `CreateTemp`/`MkdirTemp`
   can pick a name; `net` and `hash/maphash` still carry throwing `runtime_rand` stubs), and runtime
   `goenvs_impl.cs` (2026-08-01 — a `[ModuleInitializer]` snapshot of the process environment into
   `runtime.envs`, standing in for the `goenvs()` call in the `schedinit` go2cs never runs, without
   which `gogetenv` — and therefore `runtime.GOROOT()` — threw *"getenv before env init"*). Rule and
   measured reach for the last two: [`ConversionStrategies-Reference/manual-conversions.md`](../../docs/ConversionStrategies-Reference/manual-conversions.md#the-process-roots-a-converted-program-never-gets-from-a-go-bootstrap-runtimeenvs-osruntime_rand),
   *The process ROOTS a converted program never gets from a Go bootstrap*.

Marker mechanics: `[AttributeTargets.Module, AllowMultiple = true]` (golib `GoManualConversionAttribute`), so
one per file across a package is fine. The scanner wants it **before the first class**, so place it after the
`using`s and before the file-scoped namespace, written `[module: go.GoManualConversion]` (fully qualified so
it resolves without a `using go;`). **Verify** a whole-file override survives by reconverting the package into
a dir seeded with the hand-written file and confirming it stays byte-identical
(`go2cs -stdlib -go2cspath <seeded-root> <pkg>` → the marked `.cs` is untouched).

**Upgrade-time review — the `<name>.cs.auto` sibling (2026-07-16; emission model corrected 2026-07-17).** A
marker-skipped file would otherwise leave NO auto-converted output at all, so a Go-version upgrade would have
nothing to diff the hand-owned C# against. The converter therefore emits a non-compiled **`<name>.cs.auto`**
sibling beside every marked `<name>.cs` — the converter's best-effort auto conversion of the same `.go`, for
review only. It need not compile and is invisible to the build: generated csprojs compile
`<Compile Include="*.cs" />` only, which cannot match a name ending in `.auto`.

*How it is emitted matters (2026-07-17 defect fix).* A marked file is NOT dropped from the conversion
pipeline: it stays in the convert set and is analyzed and visited **with the package**, in normal file order,
so every piece of package-wide emission state its declarations feed — anonymous-struct lifts, package-var
registrations, escape/addressed-global analysis, imports, init/temp-var numbering — reaches the package's
other files exactly as in an unseeded conversion; only the file's WRITE target is redirected from `<name>.cs`
to `<name>.cs.auto` (main.go's file-visit loop). The original implementation instead skipped the marked
file's entire visit and emitted siblings in a separate last pass (`emitAutoConversionSiblings`), which
corrupted every OTHER file of a seeded package: runtime's `proc.cs` emitted raw Go `struct{…}` text where
`schedt`'s lifted anonymous-struct type names belong (unparseable C#, a CS1513/CS1022 cascade) and re-declared
the `newprocs = 0` package-var assignment as a shadowing local `var newprocs = 0;`, because runtime2.go's
state contributions never registered. With the fix, a seeded reconvert is **byte-identical to an unseeded
one** for every unmarked file — plus the marked `.cs` left untouched and the `.cs.auto` siblings added.
Guarded by the `ManualConversionSiblingState` behavioral test (a marked `state.cs` whose skipped `state.go`
declares an anonymous-struct-field type and package vars; the sibling `main.cs` consumes both).
`emitAutoConversionSiblings` (`src/go2cs/autoSiblingOperations.go`) remains only for FULLY hand-owned
packages, where the normal conversion path is skipped outright (no unmarked files, and no .csproj /
`package_info.cs` / `package_init.cs` regeneration).

Siblings are **committed** in `src/go-src-converted/<pkg>` and refreshed by reconverts — the gate probes the
DESTINATION `.cs`, so a reconvert only produces them when its output dir is seeded with the marked files
first (see *§5* above); with the fix that seeding is safe. `*_impl.cs` companions have no matching `.go`, so
they get no sibling; `unsafe` is never queued by `-stdlib` (compiler-intrinsic, `stdLibConverter.go`), so
`unsafe/unsafe.cs` has none either. Single-file conversion mode (`go2cs example.go`) emits no siblings — the
marker gate is not effective there to begin with.

The rule from *§5* still holds: canonical hand-owned files live under `src/core/<pkg>` and are overlaid into
`src/go-src-converted/<pkg>` (`overlay.sh`) — with the marker, an overlaid whole-file override then survives
the next reconvert instead of being clobbered.

### Child-process creation (`os/exec`) — what had to be hand-owned, and what did not

Re-executing the current binary is how `os`, `os/exec`, `runtime`, `flag`, `log` and a large number of
stdlib **test** suites exercise subprocess behavior, so this path gates a lot of Phase 4. Four distinct
layers were broken; only one of them warranted hand-owning.

1. **golib** — the reinterpret seam boxed a copy instead of aliasing the address, so `os.Environ()` walked
   the GC heap and freed it (`STATUS_HEAP_CORRUPTION`). Fixed generally in `ж<T>` (see
   *A reinterpreted raw address ALIASES native memory* in `ConversionStrategies-Reference.md`).
2. **Converter** — `unsafe.Pointer(p)` on a pointer parameter dereferenced it, so the nil out-pointer in
   `StartProcess`'s deferred `DuplicateHandle` panicked; and a switch `default:` clause could be emitted
   unchained, so **every** `(*Process).wait` returned "os: unexpected result from WaitForSingleObject".
   Both fixed in the converter with behavioral guards.
3. **Bodyless runtime-provided partials** — `syscall.Exit` / `Getpagesize` / `runtimeSetenv` /
   `runtimeUnsetenv` and `os.runtime_beforeExit` are provided by Go's runtime, so go2cs emits throwing
   stubs. Supplied as `*_impl.cs` companions (pattern 1). Without them no converted program could exit
   deliberately, and a child reported the stub panic instead of its own status.
4. **`syscall.StartProcess`** — the genuine hand-own (pattern 2). Go hands `CreateProcessW` a
   `*_STARTUPINFOEXW` whose fields are pointers into native memory; the converted struct holds them as
   golib `ж<T>` boxes — managed class references that are neither the right bytes at the right offsets nor
   marshalable at all (`unsafe.Sizeof(*si)` throws "cannot be marshaled as an unmanaged structure"). Same
   for `_PROC_THREAD_ATTRIBUTE_LIST` over an `array<byte>`. This is the memory-layout / raw-metal case, so
   `StartProcess` is transcribed against blittable `[StructLayout(LayoutKind.Sequential)]` mirrors and
   direct P/Invokes. **Every other declaration in `exec_windows.cs` is the converted output verbatim** —
   argument escaping, command-line building, environment-block building and path normalization are pure
   Go logic that converts faithfully — and the native code reuses those helpers plus the scalar-only
   converted wrappers (`GetCurrentProcess`, `DuplicateHandle`, `CloseHandle`).

Note what did **not** need hand-owning: `SecurityAttributes` is fully blittable (two `uint32`s and a
`uintptr`), so `CreatePipe` — and therefore `os.Pipe`, which `CombinedOutput` relies on — works through the
ordinary converted wrapper. The struct-passing seam only breaks for structs holding `ж<T>` fields.

**The seam has a SECOND member, and it is a CLASS (2026-08-01).** `syscall.GetTimeZoneInformation` is the
same shape one field-kind over: a golib `array<T>` where Go has an inline `[N]T`. `TIME_ZONE_INFORMATION`
is 172 bytes with two inline `WCHAR[32]` name buffers; the converted `Timezoneinformation` is ~64 bytes
with two managed references in their place, so the kernel wrote the native record over a smaller managed
object and `zoneinfo_windows.go`'s next `UTF16ToString(z.StandardName[:])` died in `slice<ushort>..ctor`.
Every converted program calling `time.Now().Weekday()` / `Location()` / `Local` on Windows crashed —
*diagnosed* in `time`, *caused* in `syscall`. It is hand-owned the same way, in `zsyscall_windows_impl.cs`,
with the generated wrapper reduced to a `manualConversionFuncs` placeholder. Two lessons generalize:
(1) the seam is **any** field that is a managed reference where the native layout has inline data or a raw
address — `array<T>` counts, not only `ж<T>`, so the sentence above is now too narrow; (2) verify a mirror
at **value** level, because wrong offsets return garbage *without* faulting — the `LocalTimeZone`
behavioral test compares zone abbreviations and offsets against `go run`. A census of `src/core/syscall`
finds **32** non-blittable structs and **10** wrappers passing one by address; the other nine are latent
and board-rowed rather than fixed speculatively.

The hand-owned implementation also copies every buffer handed to `CreateProcessW` (application name,
command line, environment block, working directory, handle list, attribute list) into **unmanaged** memory
for the duration of the call, freeing it in a `finally`. That closes the transient-pinned-address window
documented in `dll_windows.cs`, where golib's `ж`→`uintptr` conversion can only produce an address a
compacting GC might invalidate mid-call.

**Verified capability:** a converted program re-executes itself, passes argv and a modified environment,
has stdout *and* stderr captured through `CombinedOutput`, waits, and surfaces the child's exit code —
byte-identical to `go run` on the same program.

**Known limits.** Windows only (`StartProcess` is the Windows implementation; the POSIX
`fork`/`exec` path is untouched). Go's `SysProcAttr` surface is honored — `HideWindow`, `CmdLine`,
`CreationFlags`, `Token` (via `CreateProcessAsUserW`), `ProcessAttributes`/`ThreadAttributes`,
`NoInheritHandles`, `AdditionalInheritedHandles`, `ParentProcess` — but only the plain
`CreateProcessW`/`CreateProcessAsUserW` paths are exercised by tests so far. `syscall.Exec` remains
`EWINDOWS`, as in Go. Signals and process groups are unaddressed; `Process.Kill`/`Signal` route through
the ordinary converted `os` code and were not exercised here.

## Regenerating the standard library

Current Go converter (authoritative flags in `src/go2cs/main.go`):

```
# Whole stdlib. The converter writes to <go2cspath>/core/<pkg>, so pointing -go2cspath at src/
# regenerates the repository tree in place.
go2cs -stdlib -comments -go2cspath <repo>/src

# Specific packages only (used when greening a closure bottom-up):
go2cs -stdlib -comments -go2cspath <repo>/src fmt strings io sort time
```

> **Always pass `-comments` for stdlib conversion.** It defaults off, but the converted C# is a derivative
> work — the per-file `// Copyright … The Go Authors … BSD-style license` header **must be preserved**, and
> the Go doc-comments keep the output readable. Without the flag, headers and comments are stripped.

Package conversion is sequential (it relies on package-level converter state); output `.csproj` references are generated from detected imports.

> **Prefer a temp root when MEASURING.** `-go2cspath <tmp>` (seeded with a copy of `src/core`, so the
> `[module: GoManualConversion]` gate can see the marked files — see §5) keeps the repository clean and
> lets you diff the fresh output against the committed tree. A seeded whole-stdlib reconvert is
> byte-identical to the committed tree, so any difference is a real converter change.

## The old stub as a reference

The stub baseline is gone from the working tree. To inspect or recover individual files:

```
git show 3426298eb:src/core/fmt/print.cs          # the last clean stub (2025-05-05)
git show 2e8066da6~1:src/core/fmt/print.cs        # the stub as it stood the day it retired
git show 2e8066da6~1:src/go-src-converted/testing/testing.cs   # the auto-converted testing package
```

## Stale tooling

- **Fixed:** `src/deploy-core.bat` (`gocore`→`core`); `docs/README.md` (banner + corrected references).
- **Still stale:** `src/convert-gosrc.cmd` / `convert-gosrc.bat` invoke a retired `net6.0` C# `go2cs.exe`
  with old flags (`-s -r -e -g`); update to the Go converter's `-stdlib -go2cspath …` form.
