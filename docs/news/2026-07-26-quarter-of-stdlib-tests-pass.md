# More than a quarter of the standard library's test suites pass in C#

![go2cs](../images/go2cs-small.png)

> Full text of the July 26, 2026 announcement, condensed in the
> [go2cs News Archive](../NEWS.md#july-26-2026--more-than-a-quarter-of-the-standard-librarys-test-suites-pass-in-c).

---

## The measure

**57 of the 215 testable standard-library packages now validate their own Go test suites in C# —
26.5% of the Phase-4 target.** The measure is unchanged since the first package passed nine days ago:
a package counts only when *every* `Test` function's verdict matches a clean `go test -json -count=1`
baseline, compared by full Go test name. The current total is **1,459 matching test results** across
the set, with **47** results disclosed as divergent rather than quietly dropped. The whole table, and
the single command that reproduces any row from a clone, is in
[Validated Test Packages](../ValidatedTestPackages.md).

The mechanics have not moved because they did not need to. Each package's `_test.go` files are
transpiled to C#, built against the converted standard library, and run in an isolated process under
the hand-owned Go-semantics test host; the differential oracle then diffs the host's terminal results
against the `go test` baseline. A package that *almost* passes does not appear on the table. Where a
Go test asserts something a managed runtime provably cannot satisfy — an exact allocation count via
`testing.AllocsPerRun`, which Go reaches through compiler escape analysis, or a collectibility check
Go answers from per-safepoint liveness maps — the package carries a hand-owned, repo-committed
`go2cs_test_disclosures.json` pinning `{test, divergence class, expected failure signature}`. The
oracle reclassifies such a result only when the test name *and* the failure signature both match; a
disclosed test that fails any other way is still a hard mismatch, and packages without a manifest
compare strictly. That mechanism was built for two packages and has since carried 47 rows across the
corpus without being loosened once.

## The concurrency runtime became real, and `sync` validates

Go's `Mutex` is a state machine over an
`int32` co-designed with the runtime's sleeping semaphore — starvation-mode ownership is handed to one
*specific* waiter with exact ticket semantics — and no .NET primitive reproduces that; an emulated
semaphore trips "inconsistent mutex state" under sustained contention. So `Mutex`, `RWMutex`,
`WaitGroup` and `Pool` are hand-owned native implementations answering Go's *contract*. Beneath them,
the `//go:linkname` runtime primitives `sync` reaches for are a faithful port of Go's own `sema.go`: a
sleeping semaphore keyed by the address of the `uint32`, with the count living *in* the word the
pointer addresses exactly as in Go — which is what lets `sync`'s own `TestSemaphore` seed `*s = 1` and
expect the first acquire not to park — and a ticketed notify list where releasing ticket *t* reaches
only the waiter holding *t*, so a later arrival cannot steal a `NotifyOne` intended for someone else.
`sync.Cond` rides on that as the ordinary converted file; only its "must not be copied" checker is
hand-owned, because Go implements it by storing the checker's own address inside itself and an address
is meaningless under a moving collector — the managed form asks the same question against reference
identity instead. `sync.Pool` shows the same discipline in miniature: Go's lock-free `poolDequeue`
ring is kept whole — packed head/tail, fullness test, CAS protocol — and the fork is confined to the
one thing that cannot survive the crossing. Go's ring hangs its ownership protocol on an `eface`
slot's *type word*, and a CLR `any` is a single reference, so that word had been doing double duty as
type tag and value; the slot becomes one managed reference with null as the empty sentinel.

Goroutines learned to leave properly, too. `runtime.Goexit` now has the shape Go actually specifies: a
dedicated `GoexitException` that is deliberately *not* a `PanicException`, so the goroutine's deferred
calls run during the unwind while `recover()` stays blind to it — the distinction `sync.OnceFunc` and
`testing.FailNow` both depend on. And a goroutine that crashes now fails one test instead of taking
down the whole run, which had been turning single defects into what looked like package-wide
infrastructure walls. Several long-standing `golib` defects fell out of this work rather than out of
inspection: a Go map must accept a `nil` key and give it a slot of its own, and a pointer *to* nil is
not the nil pointer — `atomic.Pointer` had been quietly eating one, which is why `sync.Map`'s
expunge protocol degenerated.

## The reflection bridge reached the packages that actually use reflection

`errors` (61 tests)
exercises `Value.Set` and addressability; `encoding/binary` (137) drives reflective construction and
write-back; `testing/quick` (8) generates values and invokes them through `Value.Call`;
`internal/fmtsort` (3) is `fmt`'s own map-key ordering, which needs `Value.Convert` and arithmetically
ordered pointer and channel tokens. The one worth singling out is `go/token` (31): its tests serialize
a `FileSet` through `encoding/gob` and read it back, which means the converted gob `Encoder` and
`Decoder` — the real engines, not a stub — are driving the converted reflect type-relation mirrors
end to end, in C#. A reflection layer that satisfies gob is a reflection layer that satisfies most
things. (Gob's *own* test suite is not yet validated; that remains open.)

## What the campaign keeps finding is not cosmetic

Every package on the table arrived by way of
defects that compiled perfectly and then behaved wrongly, and the fixes are general rather than local.
A Go struct carrying a fixed-size array field copies that array *inline*; the emitted `array<T>` is a
struct over a shared backing, so a C# struct copy shared it — which is why `crypto/sha256`'s `Sum`,
whose whole purpose is to finalize a copy while the caller keeps writing the original, destroyed the
caller's state and returned the hash of the empty string. A Go constant of a named, string, complex or
untyped type has no C# `const` form and becomes a `static readonly` field, so it carries the same
cross-file initialization-order hazard a package variable does: `regexp/syntax`'s `opNames` table read
every one of its `Op` keys as zero and collapsed nineteen entries into slot 0. The `regexp` pair alone
exposed five such silent miscompiles — among them a blank result mixed with a named one silently
dropping the named-return-defer machinery, so a recovered panic returned the zero tuple and every
expression that must fail to parse reported success instead. Each was root-caused against the emitted
C#, fixed at its own layer, and locked in by a regression test.

## Go string literals live in RODATA, and now the converted C# does too

A Go binary allocates
nothing for `return "true"`, `s == "true"`, or `HasPrefix(line, "//go:build")`; the converted C# was
paying a heap allocation — usually with a UTF-16→UTF-8 transcode — at *every evaluation* of those
sites, because each literal materialized a fresh backing array. A four-tier arc closed the gap. String
comparison against a literal now happens span-in-place, for `@string` and for named string types
alike, so comparisons allocate nothing at all, ever. Literals in `any`/interface slots render as `u8`
spans instead of transcoding. And a whole-package pre-pass hoists every value-materializing literal to
a single `static readonly` field placed immediately above its first consumer, pre-boxed when every use
in the package is an interface target — so a literal now costs at most one allocation per program run.
On the `StringMatch` benchmark, which exists precisely to measure these shapes, the JIT gap against Go
went from **11.75× to 9.16×** and Native AOT from **12.18× to 9.06×**, with the neighboring
`StringView`, `String` and `Map` rows flat.

## Structural interface satisfaction moved entirely to run time

Go satisfies interfaces
structurally; C# does not, so the converter used to guess — it scanned each package for concrete types
that structurally matched an interface and pre-recorded a nominal adapter for every match it found.
The guess was incomplete by construction: a dynamic type living in an assembly converted *later* than
the asserting package was unreachable, so `io/fs` recorded `subFS` and could never see `os.dirFS`.
Every non-generic named interface now emits a pair of runtime "shells" into its own assembly instead —
a delegate-bound generic shell for pointer-sourced values and a reflective one for value-typed ones —
so an assertion resolves where the concrete type actually lives, with none of the recorder's blind
spots. Once the shells were in place the heuristic recorders had nothing left to do: they were retired
behind a default-off flag, proved inert against a full corpus reconvert and the behavioral suite, then
deleted — about 495 lines of guessing. The rule is now simply that a record is written only for a
conversion the source *declares*, and structural satisfaction is resolved at run time.

## One of our published performance numbers was wrong, and this is how

`IfaceShell` is the benchmark
that measures exactly those shells — runtime duck-typed assertion, the one Go operation C# has no
native answer for — and it was published at **189×** the Go time on the JIT. It was not measuring a JIT
binary. The converter's csproj template pinned `$(OutDir)` to `bin\$(Configuration)\$(TargetFramework)\`,
and MSBuild copies build outputs to `$(OutDir)`, not `$(OutputPath)` — so the perf suite's
`$(BaseOutputPath)` isolation for the Native AOT publish had never once taken effect. The AOT publish's
build step wrote its self-contained, dynamic-code-disabled binary straight over the JIT binary, and the
Measure phase timed *that* and labelled the column "JIT". Worse, the damage was sticky: MSBuild's
incremental check then saw the outputs up to date, so later JIT-only builds did not repair it and the
error survived every re-measure. The same single line silently defeated two other isolation intents —
every converted Phase-4 test host had been writing into its production package's output directory, and
any end-user project setting `BaseOutputPath` was ignored the same way — so the fix went into the
templates, which now defer to an explicit `$(BaseOutputPath)`. Correcting the configuration alone took
the row from ~189× to ~59×; a Go-style per-interface itab cache, a monomorphic slot in front of it, and
arity-based invoker dispatch took it the rest of the way to the **39.83×** now published. The durable
part is not the number but the guard: the performance runner now inspects each JIT binary's
`runtimeconfig.json` before timing it and **fails the run** on a self-contained or
dynamic-code-disabled binary rather than quietly shipping a figure. On its first outing it rejected all
eleven stale binaries, exactly the condition it was built to catch. A benchmark suite that can silently
measure the wrong artifact is not a measurement instrument, and the project's standing rule is that a
false green must be made impossible rather than noticed.

## Where performance actually stands

The honest table, measured on one machine across Go, C# JIT and
C# Native AOT, is in [Performance](../Performance.md). Native AOT startup is within a few percent of Go
(1.16×); recursive integer work is 1.23× on the JIT and 1.10× under AOT; `map` is *faster* than Go
(0.85× on the JIT, 0.32× under AOT, riding .NET's `Dictionary`). Channels are 2.03×, down from 3.39×
before the channels redesign brought real unbuffered rendezvous and single-fire select. The
`[]byte`→`string` round-trip remains the largest honest gap in ordinary code at 10.66×. The three
interface rows now separate the way the design says they should: pure method dispatch through an
interface value costs 2.79× (2.46× AOT), ordinary Go interface code — dispatch, a known-type comma-ok
assertion, a closed type switch — costs 5.84× (4.18×), and only runtime *structural* assertion reaches
39.83×. That last row is the price of a capability C# does not have, and ordinary interface use never
touches it. The 5.84× row is itself a correction: it was published at **158×**, a real and reproducible
figure that turned out to be measuring an uncached `GetCustomAttribute` call on `golib`'s type-assert
miss path rather than the cost of the construct — memoizing that answer and gating one marker probe
took it to where it is, and the pre-fix table is archived beside the current one rather than discarded.
These are the numbers as measured; none of them are projections. Compiling was the Phase-3 milestone
and running is the Phase-4 one — a quarter of the way through it, the thing being proved has not
changed: real Go tests, not compilation, are the currency of correctness.

---

*Phase-4 progress: 57 / 215 testable packages (26.5%) · 1,459 matching verdicts · 47 disclosed ·
commit `44fcc4f04` · reproduce any row from a clone via
[Try it yourself](../README.md#try-it-yourself--validate-a-converted-test-suite)*
