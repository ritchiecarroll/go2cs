# Test Host and Disclosures

[Reference index](README.md) · [Summary of this topic](../ConversionStrategies.md#converted-tests)

This page covers the `testing` shim and the converted test host, and the disclosure classes that record a test whose Go result the managed runtime provably cannot reproduce.

## The testing shim

### `testing.AllocsPerRun`

**`testing.AllocsPerRun` (`core/testing/testing.cs`, Phase-4 testing shim).** Go's version counts **mallocs**: it pins `GOMAXPROCS(1)`, runs `f` once as a warmup, then `runs` more times, and returns the `runtime.MemStats.Mallocs` delta divided (as integers) by `runs`. The CLR exposes no malloc counter, so the shim measures allocated **bytes** on the calling thread instead (`GC.GetAllocatedBytesForCurrentThread()` — precise, and inherently thread-scoped, which stands in for the GOMAXPROCS pinning; like Go's, `f` is assumed single-threaded — allocations made by goroutines `f` spawns land on other threads and are not observed). The mapping is deliberately honest rather than count-approximating: **zero maps exactly** (0 bytes ⟺ 0 mallocs — and the stdlib tests that use AllocsPerRun overwhelmingly assert zero, e.g. sort's `TestSearchWrappersDontAlloc` and the strings/bytes no-alloc guards), while a nonzero result is the average allocated bytes per run, floored at 1 so amortized sub-byte-per-run allocation can never masquerade as the exact-zero case. A converted test asserting a specific nonzero *count* therefore diverges as a loud failure in the differential oracle instead of silently passing — the disclosed outcome. (`runs == 0` divides by zero, a runtime-error panic exactly where Go's own integer division panics.) The capability sits in the converter's supported list (`supportedTestCapabilities`, `testConversion.go`), so tests requiring it convert as *included*; guarded by `TestAllocsPerRunCapabilityIsSupported` (converter) and `TestingRuntimeTests.AllocsPerRunMapsZeroExactlyAndReportsBytesWhenAllocating` (shim).

**That no COUNT is available is measured, not assumed, and a nonzero result now says which unit it is in (r56d).** The value the shim returns is rendered by Go's own `"got %v allocs"` format, so a byte figure was reaching the page wearing the word *allocs* — `crypto/internal/nistec`'s row reads `got 21964011.0`, and nothing on it said that was 21 MB rather than 22 million objects. Since a disclosed divergence may never paper over a go2cs-owned defect, an invisible unit at the seam is itself the defect. The survey behind the claim (net9.0/9.0.18, x64) is recorded on the declaration: the whole public `GC` surface exposes byte totals only; `GetAllocatedBytesForCurrentThread` is exact (40.000 B/object over 1, 10, 1e3 and 1e5 allocations of a 40-byte type) yet cannot separate count from size, one `byte[40000]` and 1,000 40-byte objects both reading ≈40,000 B; `GCAllocationTick` is a byte-threshold *sample*, 378 events per 1,000,000 allocations (one per ≈105,820 B); `GCSampledObjectAllocation` — whose `ObjectCountForTypeSample` payload *would* be a count — raises **zero** events through an in-process `EventListener` in every configuration tried (High `0x200000`, Low `0x2000000`, both, and all keywords `0xFFFFFFFFFFFF`, at Verbose and Informational), with the GC keyword's own tick count as the live positive control; `System.Runtime`'s 27 EventCounters offer only `alloc-rate`, bytes per interval; and runtime events reach an in-process listener **asynchronously** — zero visible immediately after the measured loop, settling ≈117 ms later — so no event-derived figure could be returned by a synchronous call anyway. Accordingly a **nonzero** result records its unit once on the running test (`TestExecution.NoteMeasurementUnitOnce`), landing beside the assert's own message and riding the `TestEvent` into `results.json`; the **zero** case is deliberately left silent, because there the two units agree exactly (0 bytes ⟺ 0 allocations) and a test that passes on the zero answer keeps its output byte-identical to before the seam existed. A true count *is* obtainable from go2cs's own runtime rather than the CLR's — golib allocates essentially every Go-semantic object, so counting there mirrors what Go's `Mallocs` already is, a runtime-owned counter rather than a platform facility (proven in r56d: nistec's P256 body allocates 241,077 golib objects per run for its 21,963,547 bytes) — but it is deliberately not taken, since a count that silently omits allocation sites is worse than an honest byte figure and an audited-total census of golib's allocation sites is a design-with-user arc.

The strings suite exposed the two divergence classes this mapping discloses (full analysis:
`docs/phase4/StringsBytes-BlockerMap.md`, *AllocsPerRun divergence analysis*): **count-shape
asserts** (`TestBuilderAllocs` wants exactly 1 malloc; the shim reports bytes, so any nonzero
diverges loudly — by design), and **allocation-profile divergences**, where the zero-shape itself
is unsatisfiable because the managed model allocates where Go's compiler doesn't: an addressed
local (`var b Builder` + pointer-receiver calls) heap-boxes per run where Go stack-allocates
(`TestBuilderGrow`'s growLen=0 leg), and `string(r)` materializes a `byte[]` where Go uses a
stack buffer (`TestIndexRune`). Neither class is a shim defect — a malloc-counting shim would
fail the same asserts — and neither is faked.

**A third class is neither, and must not be filed as either: ELIMINABLE inefficiency.** Because zero
maps exactly, a want-zero assert is faithfully representable — so when one fails, the honest reading is
that the converted code genuinely allocates, and the question is *what*, not *whether the unit is
comparable*. `time`'s `TestUnmarshalTextAllocations` (`got 3784 allocs, want 0`) measured out at **3664
bytes/run** for `Time.UnmarshalText`, and profiling `parseRFC3339` — where nearly all of it lives —
attributes it to two shapes, both in shared machinery and both removable:

| Shape | Cost | Why | Status |
|:--|--:|:--|:--|
| `s[a:b]` on a <code>string &#124; &#91;&#93;byte</code>-constrained value | 48 B each | `IByteSeq<T>`'s range indexer returned the **interface**, so the `@string`/`slice<byte>` struct result was boxed | **fixed** — self-referential `IByteSeq<TSelf, T>` |
| `[]byte(s)` on the same (`new slice<byte>(sΔ1)`) | 48 B each | boxed the type-parameter value again to reach the interface | **fixed** — `ToSlice` extension |
| `len(s)` on the same | 48 B each | the `len<T>(IByteSeq<T>)` overload took an interface parameter | **fixed** — `len<TSeq>(TSeq) where TSeq : IByteSeq` |
| `for i, c := range s` over a `slice<T>` | **136 B, fixed** | the range enumerator allocated once per loop, independent of length; the indexed form allocates **0** | **fixed** — struct enumerator |

Six `parseUint` calls at ~232 B each, the closure and delegate for `parseUint` itself (112 B), the
fractional-second scan (~1728 B in the same shapes) and `Date` (~240 B) accounted for the total. None of
it was CLR-necessary: Go monomorphizes the union-constrained generic and stack-allocates all of it, and
nothing here was boxing that a managed model *must* do — it was boxing that the `IByteSeq`
modeling and range lowering happened to do. **So this row was performance work, not a disclosure
candidate** (a disclosure is only for asserts the CLR provably cannot satisfy — see the campaign
charter §5), and it was resolved as such: the range enumerator became a struct (every converted
`for i, v := range s` in the corpus had been paying it), and the union-constraint boxing was removed
wholesale by the self-referential redesign described under
[Allocation-free union-constrained bodies](generic-constraints.md#allocation-free-union-constrained-bodies) — a
`parseRFC3339`-shaped body over `slice<byte>` measures **0 B/parse** where it measured 720.

### The testing shim's compile-only benchmark surface and `CoverMode`

**The testing shim's compile-only benchmark surface and `CoverMode` (`core/testing/testing.cs`).** Capability-excluded test and benchmark declarations still **compile** — exclusion gates the run registry, not emission — so every member their bodies reference must exist even though the code never executes (a broken emission inside an excluded test blocks the whole package build; see the strings/bytes blocker map, B6). The `B` surface (`N`, `Run`, `ReportAllocs`, `SetBytes`, `ResetTimer`, `StartTimer`, `StopTimer`, `Errorf`, `Fatal`, `Fatalf`) is therefore compile-only: safe non-throwing no-ops, with the params-taking members carrying explicit `ж<B>` overloads exactly as `T`'s do (ref-like `params` Spans are outside the RecvGenerator's synthesis). `testing.CoverMode()` returns `""` — not a stub-lie but Go's exact coverage-off value: the sole caller across the strings/bytes suites (strings' TestIndexRune) branches on `CoverMode() == ""` and so takes the same path as an uncovered `go test` run. Guarded by `TestingRuntimeTests.BenchmarkCompileSurfaceIsNoOpAndCoverModeReportsCoverageOff`, which compile-references every member through both receiver shapes and asserts the coverage-off semantic — removing any member fails the suite at build.

**The same rule extends to `testing.F` (2026-07-20).** A `Fuzz*` declaration is classified disclosed-unsupported in the manifest exactly as a benchmark is (`testConversion.go` already emitted the `fuzz`/"deferred to Phase 4D" entry), but its converted body still compiles into the test assembly — and `F` simply did not exist, so math/big's `func FuzzExpMont(f *testing.F)` (nat_test.go) failed the whole package build with CS0426 *the type name 'F' does not exist in the type 'testing_package'*. `F` now mirrors `B`: a compile-only struct whose members are safe non-throwing no-ops, with explicit `ж<F>` overloads on the params-taking members. Its member set is Go 1.23's full public surface for `*testing.F` — the `TB` members it inherits from the embedded `common`, plus its own `Add` and `Fuzz` — declared complete under the same anti-drift rule as `TB` above rather than trimmed to today's callers. `Fuzz` takes a **`System.Delegate`**: a Go fuzz target's signature is arbitrary (`*testing.T` followed by the fuzzed argument types), and the converted body is an explicitly-typed lambda, so C# infers its natural `Action<…>` and converts — no per-arity overload set is needed. Nothing is invoked and no seed corpus is retained, because there is no fuzzing engine to consume either. This is not math/big-specific: roughly seventeen stdlib packages ship fuzz targets (archive/tar, archive/zip, compress/gzip, encoding/csv, encoding/json, html, image/{gif,jpeg,png}, net/netip, time, syscall, …), every one of which would hit the identical build blocker. At Go 1.24.13 the set follows 1.24's surface: `Chdir` and `Context`, which Go 1.24 added to `TB` and `F` inherits from `common`, joined it (`src/core/testing/testing.cs:589-591`).

## The test host

### `-test-timeout` is the PACKAGE deadline, and it reaches both sides

**`-test-timeout` is the PACKAGE deadline, and it reaches both sides.** The flag used to bound only the
child *process*, leaving `go test` and the converted host each on its own **10-minute** default — so no
value of it could let a slower-than-Go suite finish. `hash/maphash` self-terminated at exactly 600 s
under `-test-timeout 40m`, reporting its still-running `TestSmhasherAvalanche` as an empty verdict that
reads exactly like a real failure (the very "slow ≠ hung" trap, one layer below where it was known).
The compare/run actions now pass the flag through as `go test -timeout` **and** the host's own
`-timeout`, and kill the child one minute later (`testChildTimeoutGrace`) purely as a safety net — the
in-process deadline must fire first, because it is the side that writes results. Threading it also
exposed that the host's duration parser accepted only a *single* unit, rejecting `time.Duration`'s own
`String()` output (`30m0s` → "invalid Go-style duration", exit 2 before a test ran); it now parses Go's
real grammar, a sequence of decimal-and-unit pairs over ns/us/µs/ms/s/m/h, so `1h30m` works as it does
for `go test`. maphash's C# suite needs ~15 min where Go's needs 7.6 s — a performance gap, recorded as
such, not a correctness one.

### The test host treats an escaping `GoexitException` as Go's `tRunner` does

**The test host treats an escaping `GoexitException` as Go's `tRunner` does**, and each test's
dedicated thread is marked a goroutine (`Goroutine.Enter`) because in Go a test body IS one. Go's
`FailNow` is *specified* as "mark failed, then `runtime.Goexit`", so a Goexit escaping a test body
means the test ended without completing — Go reports `errNilPanicOrGoexit` ("test executed panic(nil)
or runtime.Goexit") against that test. The host logs exactly that text and fails the one test, where
Go additionally panics the whole binary; keeping the run alive leaves the rest of the package
measurable. This is also the path `testing.T.FailNow` fidelity will take.

### An unhandled NON-panic exception on a goroutine fails ONE test

**An unhandled NON-panic exception on a goroutine fails ONE test instead of killing the host — and
that containment is TEST-HOST-ONLY.** A converted program keeps Go's fidelity (an unhandled failure in
a goroutine is process death), which is golib's default: nothing contains it and it reaches the
AppDomain backstop. A host that runs many independent Go programs in one process is different — the
crash discarded every result not yet written and blanked the tail of the package, so one defect read as
a mass infrastructure wall. `Goroutine.ContainUnhandledExceptions(policy)` lets such a host install a
containment policy, which golib consults through an exception **filter** (never a catch-and-rethrow, so
the uncontained path is bit-for-bit the old behavior — unhandled, stack intact, intervening `finally`
blocks unrun). A panic is never offered to a policy. The host attributes the failure to the right test
with an `AsyncLocal` set on the test thread: it flows with the `ExecutionContext` that `Thread.Start`
captures, which is exactly how golib dispatches a goroutine, so the attribution survives goroutines
spawning goroutines. If the crashed goroutine was the one that would
have unblocked its test, that test now waits for the package timeout — which still writes every result
gathered so far, where the crash wrote none.

### A PANIC on a goroutine still kills the host

**A PANIC on a goroutine still kills the host — but it no longer takes the run's evidence with it
(2026-08-14).** Containment stops at panics deliberately (above), so the fatal path stayed: golib's
backstop printed `panic: <value>` and exited 2. For a converted PROGRAM that is right and complete —
it is Go's own report. For a host it was the hardest possible diagnostic: no frame, no test name, and
every verdict already gathered discarded unwritten. `net/rpc/jsonrpc` is the case that made it
concrete — a `reflect`-allocated `[1]int` reply panicked on an rpc goroutine, the host died, and the
package recorded **0** verdicts where it had been recording 6, so a strictly BETTER corpus read as a
worse one and the one real failure read as three.

`Goroutine.ObserveUnhandledPanic(observer)` is the other half of the containment pair, and it
contains nothing: the observer runs from an exception filter that **always declines**, so the panic
still reaches the backstop with its stack intact and the intervening `finally` blocks unrun —
byte-identical to an unobserved run, which is what keeps Go's fidelity and the differential oracle's
view of it. What the filter buys is the moment BEFORE the unwind. The test host uses it to attribute
the panic to the test whose goroutine it was (the same `AsyncLocal`), report that test's terminal
`fail` carrying the full traceback (`PanicException.StackTrace`, which prefers the panic's ORIGIN over
the frames it unwound through), and flush the result and JUnit files. A goroutine panic now costs the
TAIL of a run rather than all of it — which is also what `go test` shows, since a Go binary that dies
this way has already streamed the verdicts it reached. Guard:
`GolibTests/GoroutineRootPanicTests`, over the root's whole policy — a panic observed and still
escaping, its fault site surviving, a mapped runtime-error panic, a non-panic exception still going to
containment and not to the observer, and `Goexit` reaching neither.

### The unhandled-exception backstop prints the whole exception chain

**The unhandled-exception backstop prints the whole exception chain for a NON-panic failure.** A real
panic keeps Go's report shape (`panic: <value>` on stderr, exit 2). Anything else is a *defect to
diagnose*, and `ex.Message` alone threw the evidence away: a `TypeInitializationException`'s own
message merely names the type and says "see inner exception", so the actual fault and its stack were
lost (a whole `gob` run's real cause was invisible this way). The backstop now writes `ex.ToString()`
for the non-panic case, carrying the full inner-exception chain and stacks.

### A converted `TestMain`'s `flag.Parse()` must know the host's own arguments

`internal/fuzz` builds clean afterwards (0 errors, from four parse-error families) and then stops one layer further out, on an infrastructure root that is **not** this one and is worth recording precisely: its `worker_test.go` `TestMain` calls `flag.Parse()`, and the converted `flag.CommandLine` has never been told about the host's own `--json` / `--result` / `--junit` / `-timeout` arguments, so the run dies with `flag provided but not defined: -json` before any test executes. In Go, `testing.M` registers those flags on `flag.CommandLine` before `TestMain` runs, which is what makes the same `flag.Parse()` legal there. This also **corrects** the board's attribution of the identical symptom on `go/internal/srcimporter` ("the process the host launches is not the go2cs test host"): the process IS the host — what it lacks is the flag registration.

**That second layer is now closed, host-only (2026-08-14) — `src/core/testing/TestFlagBridge.cs`, and `internal/fuzz` banks 52/52 behind it.** The host declares its OWN command line on the converted `flag.CommandLine` immediately before it invokes a converted `TestMain`, which is exactly what `testing.Init()` does for `-test.*` in Go and exactly what was missing: nothing is consumed or hidden from the parse, the names are simply *defined*, so `flag.Parse()` recognizes them. The full `-test.*` set is registered — with this run's real values for `test.run`/`test.parallel`/`test.v`/`test.short`/`test.count`/`test.timeout`/`test.shuffle` — rather than only the spellings the host was handed, because converted tests READ those flags back (`os/exec` gates on `flag.Lookup("test.run").Value.String()`, `runtime` on `flag.Lookup("test.parallel").Value.(flag.Getter).Get().(int)`); registering less would trade a parse error for a nil dereference, and it is why the typed registrars are used and not `flag.Func`, whose `funcValue` has an empty `String()` and is no `Getter`. The `flag` package is bound **late, by name**, not by project reference: Go's `testing` imports `flag`, but the generated test csproj sets `DisableTransitiveProjectReferences=true` (load-bearing against CS0576), so a `testing` → `flag` reference does not deploy `flag.dll` beside the 124 of 141 test hosts whose package does not import `flag` — measured, along with a +33% build cost on every test project — and it is not needed, because the converted `flag` package is present in a test compilation **iff** that package imports it, which is precisely when `flag.CommandLine` is observable at all. A name the test package already defined is skipped, since the converted `FlagSet.Var` panics on redefinition. Go's `M.Run` also parses when `flag.Parsed()` is false; that is deliberately NOT mirrored, because no member of the class needs it and an unconditional parse would newly reach `ExitOnError` for packages that merely reference `flag`.

**A THIRD layer of the same concern — the host now STOPS parsing at the first non-flag argument, which is Go's rule (2026-08-14, lane `claude/host-argv-stop`; `src/core/testing/TestOptions.cs`).** The bridge above made a converted `flag.Parse()` *recognize* the host's arguments; this makes the host recognize the PROGRAM's. `flag.(*FlagSet).parseOne` stops at the first token that is not at least two characters long and starting with `-`, leaving it and everything after it for the program — and a Go test binary is a program: its `TestMain` may take arguments, and `os/exec` drives its entire helper protocol that way, re-executing the test binary as `exec.Command(exePath(t), "cat")` and dispatching on `flag.Args()[0]`. The converted host's `TestOptions.Parse` had no stopping rule at all; its `default:` arm threw `unsupported converted test option: cat`, so every helper child died at startup with exit 2 before `TestMain` was entered, and the PARENT then reported the downstream symptom (`echo: want "foo bar baz\n", got ""`, `ExitCode got 2, want 42`) — one host defect reading as twenty unrelated failures across **26** of `os/exec`'s comparison rows. Nothing else was needed to make the remainder visible: the converted `os` package fills `os.Args` from the real command line independently of this parser (on Windows through `syscall.GetCommandLine` + `commandLineToArgv`, on unix through `runtime.argslice`/`goargs_impl.cs`), so the host's whole obligation is to stop, and to leave the trailing tokens untouched on the way past. **Stopping is not ignoring**, and the difference is load-bearing in both directions: `exe cat -n` must leave `-n` to the child rather than parse it as a host flag, while an unrecognized `-flag` appearing BEFORE any non-flag is the host's own command line being wrong — Go errors there too, so it still exits 2 (with Go's own wording, `flag provided but not defined: -x`, since this host stands in for a Go test binary and its stderr is read beside one). The rest of `parseOne` is mirrored for the same reason: a lone `-` is a non-flag by the length test, `--` terminates the flags and is itself consumed, `---x`/`-=x` are `bad flag syntax`, a non-boolean flag takes the NEXT token as its value even when that token looks like a flag (`-run -v` filters on `-v`), and one or two leading dashes name the same flag — the `--json` ≡ `-json` equivalence `TestFlagBridge` already assumed when it republishes these options under their undashed names. (Guarded by `TestingRuntimeTests.FlagParsingStopsAtTheFirstNonFlagAndLeavesTheRestToTheProgram`, which pins all of it through the host's public surface: recognized flags in both dash spellings, the stop, a trailing `-v` that must neither be consumed nor rejected, `-`/`--`, and the unknown-flag and bad-syntax errors before a stop.) `os/exec`'s root A is closed by this — 23 of the 26 rows joined the agreeing set — but the package does NOT bank: 27 rows remain on the declared `relocatable single-file test executable` host limit, and the 3 residual `TestWaitInterrupt` rows revealed a THIRD root underneath, `os/signal`'s runtime primitives being unimplemented partial stubs. See the board.

**A FOURTH layer, which CORRECTS one sentence of the third — the package's OWN flags participate, because the package is now initialized before the host decides (2026-08-17, lane `claude/tls-finish`; `src/core/testing/TestOptions.cs`, `TestHost.cs`, `TestFlagBridge.cs`).** The layer above says "an unrecognized `-flag` appearing BEFORE any non-flag is the host's own command line being wrong". That is right only when nobody else could own the name, and there is a whole class where somebody does: a package that declares its own `flag.Bool`/`flag.String` at package level. `crypto/tls` is the corpus's example — BoGo re-executes the test binary as its TLS shim (`-shim-path=os.Args[0] -shim-extra-flags=-bogo-mode`), `-bogo-mode` is a `flag.Bool` in `handshake_test.go`, and all **3,242** BoGo cases died at startup on `flag provided but not defined: -bogo-mode` before `TestMain` ran. **The mechanism is ORDER, not tolerance.** Go's test binary reaches exactly ONE `flag.Parse()`, and by then `testing.Init()` has defined the `-test.*` set *and* the package's own package-level variable initializers have run, so both vocabularies live in a single flag set — an unknown name is still an error there, just a later and better-informed one. The converted host's parse necessarily runs earlier than the package's initialization, so it was answering a question it could not yet answer. Three changes restore Go's order: (1) `TestOptions.Parse`'s `default:` arm records the name in `UnrecognizedFlag` and STOPS, exactly as a non-flag token stops it — stopping rather than skipping is forced, because nothing at that point knows a foreign flag's ARITY and `-port 5000`'s value is indistinguishable from a program argument, so everything from the unrecognized name onward is the program's (which does mean a HOST flag placed after a package flag is left to `flag.Parse()` rather than read here; the pipeline places the host's own flags first, and Go's single parse makes the ordering irrelevant on its side); (2) `TestHost.Run` runs the package's own initialization the way Go runs it before main — `RuntimeHelpers.RunClassConstructor` over the declaring types of the registry's delegates, which ARE the converted package's classes — but only when the parse actually met an unrecognized name, so every other run keeps initialization exactly where it was; (3) the verdict is then taken against the converted `flag.CommandLine` (`TestFlagBridge.IsDefined`), after the package's flags and the host's bridged flags are both declared, and an undefined name is still `flag provided but not defined: -x` with flag's own `ExitOnError` code. **The rejection moved; it did not go away** — and it did not move at all for the 124 of 141 test projects whose package does not import `flag`, where `IsDefined` answers false because no flag can exist rather than as a fallback. Ordering the package's initialization BEFORE `TestFlagBridge.Register` also makes that registrar's redefinition guard real for the first time (it skips a name the package already defined; previously the package had not yet run, so the guard could never fire). ⚠ **Creating a delegate over a static method does NOT run its declaring type's static constructor** — `ldftn`+`newobj` is neither a static-field access nor an invocation — so the generated host's `registry.Add("TestX", pkg_test_package.TestX, …)` lines leave the package uninitialized until the first test BODY runs; that measured fact is what makes step (2) load-bearing rather than defensive. Still deliberately NOT mirrored: `M.Run`'s `if !flag.Parsed() { flag.Parse() }`, for the reason the second layer already gives. (Guarded by `TestingRuntimeTests.APackageRegisteredFlagParticipatesAndAnUndefinedOneIsStillRejected`, which stands a class whose *static constructor* declares a flag in for the package under test — written with an explicit static ctor so the CLR's precise non-`beforefieldinit` rules apply — and pins all four claims: the package's flag participates, a flag before it is still the host's, a flag after it belongs to the program, and an undefined name is still exit 2. It is the one test needing the converted `flag` package present, so `BehavioralTests.csproj` references it; `testing.csproj` still must not, and does not.) The payoff is measured at value level: the converted host answers `-bogo-mode -is-handshaker-supported` with `No` byte-identically to Go's test binary, and a filtered BoGo case (`-bogo-filter Client-Verify-ECDSA-TLS1`) **passes** — the converted TLS stack completing a handshake against BoringSSL's own runner. See the board for why `crypto/tls` still does not bank.

## Disclosed divergences

### The disclosed-divergence manifest

**The disclosed-divergence manifest (2026-07-18 ruling — implemented).** These provably
unsatisfiable divergences are disclosed at TEST level, extending the declaration-level
"disclosed-unsupported" vocabulary: an affected package carries a hand-owned, repo-committed
`go2cs_test_disclosures.json` beside its converted sources (never generated — reviewed like
source; deliberately absent from `src/core/.gitignore`'s regenerated-artifact list),
pinning `{name, class, signature, reason}` per divergent test. The `-test-action compare` oracle
(`matchTerminalStatuses`, `testConversion.go`) reclassifies a Go=pass/C#=fail row as
**disclosed-divergent** ONLY when the exact test name is pinned AND the captured C# failure
output contains the pinned signature substring — the converted host attaches each test's
accumulated log text to its terminal event, which is what the signature matches against. The
signature pin is the integrity guard: a pinned test failing with ANY other signature (e.g. an
index-semantics leg regressing) is still a mismatch, a pinned test in any other status pair
(including C#=infrastructure-error) is still a mismatch, and a package with no manifest compares
strictly — sort and utf8 are unaffected. The validation summary discloses the reclassified rows
alongside the excluded declarations (`… 7 disclosed-divergent (alloc-profile), …`), subtracting
them from the validated count, and the nonzero C# host exit the disclosed failures cause is
forgiven only when `go test` itself was clean and every divergence matched its pin (zero
mismatches — a truncated host run surfaces as one-sided rows and stays fatal). An empty
signature (which would substring-match anything) and duplicate names are load-time errors, never
silent no-ops. Guards: `TestDisclosedDivergenceOracle` (signature match discloses / different
signature still fails / no manifest strict / direction+status pairs never widen) and
`TestDisclosureManifestLoading` (absent-file no-op; empty-signature and duplicate rejection).
First users: `bytes` (7 alloc-profile rows) and `strings` (3 alloc-count-semantics + 1
alloc-profile), validating as Phase-4 packages #3 and #4. `unicode/utf16` (package #5) is the first
to reuse the mechanism as a general tool rather than a bytes/strings special case: its lone
`TestAllocationsDecode` asserts `Decode` returns its non-escaping `[]rune` with **zero** allocations —
which Go reaches only through escape analysis (the test guards itself with `testenv.SkipIfOptimizationOff`),
and which the managed runtime provably cannot, since a returned `slice<rune>` is always a heap allocation.
It discloses one `alloc-profile` row (signature `"Decode allocated "`) while `TestDecode` independently
proves the decoded output is correct — the disclosure covers exactly the allocation profile, nothing else.

### A pin whose GO side is not deterministic — the `hostConditional` annotation

**A pin whose GO side is not deterministic — the `hostConditional` annotation (2026-08-20 coordinator
ruling — implemented).** A disclosure asserts *Go passes, C# provably cannot*, so it is only stable
while the **Go** side is stable. `crypto/tls`'s `TestBogoSuite` is the first pinned row whose Go side
is not: `go test` downloads the pinned boringssl module, builds BoringSSL's own runner from it, and
whether that runner passes, expands into its 3,243-row case matrix, or fails is decided per host and
per run by network reachability and by the 10-minute deadline Go's test binary carries by default.
That makes the pin brittle in **both** directions — the sweep goes red when Go starts failing (Go
fail / C# fail reads as an ordinary *agreed failure*, so the row leaves the disclosed set and
`crypto/tls` reports 401 + 1 where the roster banks 400 + 2), and red again when Go goes back to
passing on a quieter host. Nothing in a go2cs branch can move either reading: the baseline is
`go test -json` over GOROOT's own sources, where the BoGo shim is Go's own test binary.

> *At go1.24.13 (after this ruling):* the BoGo fan-out is 3,419 rows (1 parent + 1,022 pass + 2,396
> skip), `crypto/tls` banks 4759 + 1, and `TestBogoSuite` agrees pass/pass on its proof page, so the
> annotated entry discloses nothing there. The figures in this entry are its date's.

The ruling **refused** the broad remedy — accepting agreement-on-failure as satisfying a disclosure
*in general*, which would make every pin self-satisfying the moment its baseline broke for any
environmental reason, quietly decaying *Go passes, C# provably cannot* into *C# fails* — and adopted
a per-row annotation instead. A manifest entry may carry `hostConditional`, whose non-empty value IS
the marker **and** the one sentence naming the environmental dependency (so a row can never be marked
without saying what it depends on; a blank one is a load-time error, like an empty signature). An
annotated row is accepted in **exactly two shapes** and accounts as **disclosed — never as matching**
in both: Go pass / C# fail (the pinned divergence) and Go fail / C# fail (agreement, where the Go
premise itself fails). The roster arithmetic is therefore host-stable — `crypto/tls` reads 400 + 2 on
every machine. Go-side subtest children of an annotated row ride the existing withdrawal rule in the
second shape too, because shape (b) is precisely the shape in which Go DID reach its fan-out;
without that, `TestBogoSuite`'s 3,243 case rows land as one-sided mismatches on a run where the
converted side never moved. **The tolerance is confined to the half that was never deterministic:**
the signature pin governs both shapes, so a C# failure that moved is a strict mismatch under the same
wording the first shape uses, a C# side that starts passing leaves the disclosed set and the sweep's
`disclosed count moved` check fires, and an *unannotated* row gains no second shape at all. Only a
row with coordinator-accepted rooting evidence is annotated; `TestBogoSuite` is the first and only
member. The generated proof page renders the row's own note from the manifest — naming the
dependency and both accepted shapes, the model the roster's `internal/zstd` row sets — rather than
carrying a hand edit a regeneration would drop. Guards:
`TestHostConditionalDisclosureAccountsInBothShapes` (both shapes disclose; a moved C# signature is a
mismatch; a C# side that starts passing leaves the set; an unannotated row stays an ordinary
agreement), `TestHostConditionalRootWithdrawsGoOnlyDescendantsWhenGoFails` (the flood exclusion, with
the un-annotated control that floods), `TestHostConditionalRowRendersDisclosedWhenBothSidesFail` (the
page's totals, its disclosed marker and the note) and `TestHostConditionalMarkerMustNameItsDependency`.
Shape (b) cannot be forced on a host whose Go BoGo run passes, so those fixtures ARE its proof.

### `codegen-liveness` — a frame holds what Go has already dropped

A second disclosed-divergence class alongside `alloc-profile`, first pinned by `sync` (packages
`TestOnceXGC` ×3 subtests and `TestPoolGC`). Go's GC consults **per-safepoint liveness maps**: a
local dies at its last use, even in the middle of a running function. The CLR's GC info is
conservative in two ways that a GC-lifetime test can see from inside its own frame:

* **Tier-0 / MinOpts codegen reports every frame local live for the whole method.** Every test method
  runs *once*, so it is jitted at tier 0 — in Release as well as Debug — and the pipeline builds the
  test host unoptimized on top of that. Measured: a bare `byte[]` local is still alive after a full
  blocking collect, and becomes collectible under `DOTNET_TieredCompilation=0`.
* **A by-value struct argument larger than a machine word is passed by hidden reference.** A
  `slice<T>` is four words, so the x64 ABI makes the caller materialize a stack temp and pass its
  address — address-exposed, therefore untracked, therefore reported live for the whole frame. This
  one holds in **fully optimized** code, which is what makes it provable rather than a build-flag
  artifact: a probe frame that registers a finalizer on `buf`'s storage releases it (finalizer fires)
  when nothing else touches `buf`, and pins it for the frame's lifetime when the frame merely passes
  `buf` by value to one function.

`TestOnceXGC` is unsatisfiable on the second point *at every layer go2cs owns*: its own body is
`f := fn(buf)`, and both `gcwaitfin()` checks happen inside that frame. `sync.OnceFunc` genuinely
does drop the wrapped function — measured directly, the backing array is released after the first
call — so the disclosure covers the CLR's frame conservatism, not a retention bug. The two real bugs
the investigation *did* find (SetFinalizer keying on the pointer box; `Ꮡ`'s `in` parameter pinning the
array) were fixed at their layers first; only what remained was disclosed.

The class's bar and its standing measurements live with the roster's disclosure classes in
[Validated Test Packages](../ValidatedTestPackages.md): the 2026-08-30 tier-0 A/B found the first point above
disappears under a Release publish with `DOTNET_TieredCompilation=0`, so a row that needs that
configuration says so on its own line, `execution: release-tc0` (`internal/weak` is the first). `sync`'s
three `TestOnceXGC` subtest pins, and how they count toward its Disclosed column, are recorded in
[DATA-alloc-pins-rand-sync](../phase4/DATA-alloc-pins-rand-sync.md);
[CENSUS-type-name-erasure](../phase4/CENSUS-type-name-erasure.md) §8 derives how `unique`'s GC rows move
between matched and `codegen-liveness` once its subtest names match Go's.

### `host-limit` — the third disclosed-divergence class: what the test HOST cannot BE

`alloc-profile` and `codegen-liveness` both name something the managed runtime cannot **measure**.
`host-limit` names something the converted test binary cannot **be** — a property of the deployment
shape rather than of an assertion. Ruled 2026-08-15 and first pinned by `os/exec`, whose 25 leaf rows
under `TestCommand` and `TestLookPathWindows` carry it (their 2 parents ride the disclosed-parent
aggregation rule, carrying no failure text of their own).

**The one capability named today: a relocatable single-file test executable.** Go's test binary is
statically linked, so a test may copy it and run the copy — `os/exec`'s `installExe` does exactly
that, and both of its `LookPath`/`Command` fixture tables are built on it. A converted test host is
an **apphost**: a stub bound at build time to a managed assembly of the same base name that must sit
beside it. Copy the one file and hostfxr answers `LibHostAppRootFindFailure` — `exit status
0x8000809a`, *"The application to execute does not exist"* — which is byte for byte what the tests
report, and is the pinned signature. Satisfying it means publishing every converted test host
self-contained single-file: ~70 MB and a publish rather than a build, per package.

**The bar, and why the class stays narrow.** A `host-limit` entry must pin a **structural** property
of the current deployment shape — provable from how the artifact is built, not from how far an
implementation has got — and never an unimplemented-but-fixable defect. The distinction is the same
one that keeps `log` unbanked rather than disclosed: `log`'s `TestAll` wants a `.go:63` position, and
a Go-source position map would satisfy it exactly, so it is a deferred capability and no disclosure.
Nothing about an apphost's binding is deferred work of that kind; it is what the artifact *is*.

**Why a disclosure rather than a capability gate**, given that `unsupportedRuntimeCapabilities` names
this very capability for `os_test.TestRemoveAllWithExecutedProcess`. The gate arm was built and
measured on `os/exec` before the ruling, and it fails on three counts recorded in
[BOARD-next-validation-candidates](../phase4/BOARD-next-validation-candidates.md): a gate keys on the
DECLARATION, so it withdrew 40 verdict rows where only 27 were failing, destroying 13 live agreeing
passes; it is self-defeating against a `TestMain` that asserts the whole suite ran, because greening
the suite is what arms that census (`helper command unused: "printpath"`, host exit 1, package
validates at no count); and it hides the very rows whose future passing is the signal the limit has
been lifted. A disclosure keeps every row running, visible and compared, so the class **retires
itself**: build the single-file host and these 25 rows start passing, the disclosed count stops
matching, and the sweep fails until the entries are removed. `os`'s gate entry predates the ruling
and `os` is not yet on the roster; its disposition is decided when it banks.

### `deferred` and `structural` — the two labels every allocation-count disclosure resolves into

Ruled 2026-09-05 (coordinator, owner-ratified the same day) and landed in the schema and the roster
guard together. They exist because `alloc-profile` had come to carry two different claims under one
word. An `AllocsPerRun`-style assertion measures **Go's escape analysis** — a compiler optimization
the CLR JIT does not perform — rather than a behavioural property: the value written, read or
returned is identical on both sides. So such an assertion is **never disclosed as CLR-structural
merely because it fails today**, which is ruling #1 of 2026-08-02 restated; but it may be **deferred
against a named plan to reach it**, which is the amendment.

| label | the claim | what the entry must carry |
|---|---|---|
| `deferred` | the CLR **can** meet the assertion in principle | `want`, `reading`, `plan` — all three, or the entry is refused |
| `structural` | **no** managed implementation can meet it, proved in the reason with the object Go keeps off the heap NAMED | no `plan` — naming one contradicts the claim |

**`deferred` is a commitment, not a quieter disclosure.** The owner's strengthening makes the plan a
hard requirement: an entry with no executable plan is refused by the guard, and a plan whose design
record is retired without a replacement fails the row at its next sweep exactly as a regression would.
One plan may serve **many** entries by **mechanism family** — a box minted per address-take, an
element take, an out-parameter, an intermediate buffer in a string conversion — so the requirement is
met by family rather than by a bespoke design per entry.

**The three fields, and why each is required.** `want` is the assertion's own bound, readable without
opening the test. `reading` is the measured current value **with the configuration named**, because a
reading taken at another configuration is not comparable (Release with tiering off is the measurement
of record) and one with no tree named cannot be re-checked. **Nothing compares it with the run yet**
(present tense, dated 2026-09-23, ledger 03:37 X(5)): the loader and check-roster-format's 2c arm only
check that the field is present, so until the sweep-side comparator of
[`DESIGN-allocation-counting.md`](../phase4/DESIGN-allocation-counting.md) §9 (v) lands, COORD compares
each deferred entry's unit note against its `reading` by hand at every sweep read, and a reading moving
**away** from the want is a finding. Every `reading` therefore LEADS with its per-run figure.
`plan` is the design record and increment that closes it.

**Which instrument's number goes in `reading`.** The converted host's own `AllocsPerRun` value, not a
lane's probe: that is what the assertion measures and what a regression must be measured in. A probe
over golib's own allocation sites counts a **different population** — a defer's delegate, a params
array and an interface box sit outside it — so a design record's ladder and the roster's printed
reading are two numbers, and an entry names both rather than letting either stand for the other.


**A structural FLOOR under a deferrable excess (ruled 2026-09-05, from the reflect census).** An
entry can have both at once, and neither label alone is honest about it: `reflect`'s
`TestDeepEqualAllocs` rows want 0 at a signature where boxing a value type into `object` allocates by
construction — a floor of two per run — while the readings run to 53 objects, an excess of ordinary
reducible bridge work. `structural` would bury 51 reducible objects; `deferred` would name a want no
plan can reach. So a **deferred** entry may carry a `floor`: an object count greater than its want,
with a `proof` sketch beside it. Its retirement condition becomes *the host's reading equals the
floor*, and at that point the entry **re-labels `structural`** with the proof already attached and its
plan discharged. The plan requirement is unchanged — the excess is what the plan retires — and a floor
is a CLAIM the census can falsify: a segment reading zero where a floor was predicted retires the
floor, not the entry. Refused: a floor on a `structural` entry (that label claims nothing is
reducible), a floor with no proof, a floor that does not exceed its want (nothing is deferred, so the
entry is simply structural), and a floor beside a want that does not LEAD with its number — refusing
an uncheckable pairing beats guessing which number in a sentence was meant. `floor: 0` means absent,
which is sound because a legal floor is always at least 1.

**The R3 shape: Go's whole budget lies outside the counted population (ruled 2026-09-23, ledger 03:37
X(4) and O2).** Some asserts budget a NONZERO number of allocations that Go spends entirely on objects
golib's counter never charges -- `log/slog`'s `2 pairs` wants exactly 2, and in Go those are the two
`any` conversions of its non-constant arguments, which the managed side performs as uncounted CLR boxes
(`9 kvs` is the EXCEPTION: its 10 includes one `Record.back` slice that both sides count, so once its
surplus is gone it reads COUNT 1 against an exact 10 and never reaches the BYTES arm; it carries NO
pre-declared end label, and its relabel is ruled when it arrives -- COORD's ruling, ledger 3942e083ad).
Such an entry is
`deferred` on its COUNTED SURPLUS (every counted object is excess, because Go's budget sits outside the
count) and carries a **pre-declared end label of `alloc-count-semantics`**: when the plan has removed
the surplus, the count reads zero while bytes are nonzero, the host takes the BYTES arm, and the entry
relabels to the incomparable-unit class rather than retiring. That end label is declared ONLY for this
shape; no want-0 COUNT entry pre-declares one (a flip to BYTES there is a relabel trigger ruled when it
happens), and whether a want-0 BYTES entry with a nameable uncounted residue belongs in
`alloc-count-semantics` is an open post-hop question.

**And a THIRD label stays, for a different reason.** `alloc-count-semantics` (8 entries at this ruling; 5 at go1.24.13) names an
assertion whose UNIT cannot be measured on the host — `reflect`'s `TestChanAlloc` wants 1 where our
counter is silent and the figure is bytes. That is not a bigger number, it is a different unit, so it
is neither deferrable nor structural. The three live labels are therefore `deferred` (measurable, can
be met, plan), `structural` (measurable, cannot be met, proof) and `alloc-count-semantics` (the unit
cannot be measured, stated). Only the bare `alloc-profile` label retires.

**Enforcement, in two places on purpose.** `loadTestDisclosures` refuses a malformed entry at compare
time, so a broken entry fails the sweep of the row that carries it; `check-roster-format.ps1` reads
**every** committed manifest in one pass, so a mislabelled entry is caught the day it lands rather
than at that row's next rebank. Both refusals name the missing field. The bare `alloc-profile` label
is **legacy**: it still loads, because rows re-classify at their own next rebank and never wholesale,
and an over-eager guard would take every unswept row down.

---

[Index](README.md)
