# DESIGN — the Go-format crash report for an unhandled panic

> **Status: IMPLEMENTED (2026-08-21).** Built exactly as specified below; see
> [§10 As built](#10-as-built) for the shape on disk, the measured result and the one thing the
> measurement added to the picture. `runtime/debug`'s `TestSetCrashOutput` moved fail → **pass**,
> all six assertions, taking the package to 4 matched / 5 divergent and banking it as roster
> row #162.

> **Scope.** What a converted program prints, and where, when a panic reaches the top with nobody
> to recover it. The format is **Go's own**; nothing here is invented. The arc exists because
> `runtime/debug`'s `TestSetCrashOutput` measures exactly this surface and found go2cs printing a
> .NET exception dump instead — see the board entry *"MEASURED, DOES NOT BANK — `runtime/debug`'s
> NINTH verdict is `TestSetCrashOutput`"* (2026-08-21) for the rooting and the price.

## 1. What Go prints

Go's runtime writes a crash report to standard error and dies with exit status 2. Its shape is
recorded verbatim in Go's own test source (`runtime/debug/stack_test.go`, the comment above
`TestSetCrashOutput`'s read-back):

```
panic: oops

goroutine 1 [running]:
runtime/debug_test.TestMain(0x1400007e0a0)
	GOROOT/src/runtime/debug/stack_test.go:33 +0x18c
main.main()
	_testmain.go:71 +0x170
```

Four elements, in order:

1. `panic: ` followed by the panic **value**, rendered by Go's `preprintpanics` rule — an `error`
   prints its `Error()`, a `fmt.Stringer` its `String()`, anything else its `%v` form.
2. A **blank line**.
3. `goroutine N [running]:`
4. The traceback — one `<pkg>.<Func>()` line per frame, each followed by a tab-indented
   `<file>:<line>`.

Elements 3 and 4 are *exactly* what converted `debug.Stack()` already produces, and that production
is banked: `runtime/debug`'s `TestStack` compares four converted frames against Go's own
expectations — package path, receiver spelling, Go file and Go line — and agrees on all four (the
fifth is the ruled `host-limit` frame). Element 1 is exactly what `PanicException.PanicText` already
computes, and it is banked by the `PanicValueRendering` behavioral test. **This arc composes banked
ingredients; it renders nothing new.**

Two properties of Go's report that the composition must preserve, because `TestSetCrashOutput` pins
them:

* The report goes to stderr **in addition to** whatever the program itself wrote there. The child
  in that test does `println("hello")` before panicking, and Go's stderr carries both.
* When `debug.SetCrashOutput(f, …)` is in force, the report **also** goes to `f` — and `f` receives
  the report and *nothing else*. `hello` must not be in the crash file.

## 2. Where it lands, and where it does not

The wrong answer is `runtime/debug`. `SetCrashOutput` is only the *destination* half; the text
`TestSetCrashOutput` reads is chosen by whatever prints when a panic escapes, and today that is a
.NET framing produced in two places:

| Site | What it prints today |
|:--|:--|
| `golib` — `builtin.InitializeGoLib`'s `AppDomain.CurrentDomain.UnhandledException` backstop | `panic: <value>` — the first line only, then `Environment.Exit(2)` |
| `core/testing` — `TestHost.Run`'s outer `catch (Exception ex)` | `ex.ToString()`, i.e. `System.AggregateException: One or more errors occurred. (oops) ---> go.PanicException: oops` over a CLR frame list, then `return 2` |

Both are "the host deciding what an escaped panic prints", and both must print the same report.
That is the whole blast radius, and it is corpus-wide by construction: golib's backstop is the
terminal path for *every* converted program's unhandled panic, including one that escapes a
goroutine.

**A recovered panic is untouched.** Nothing on the recover path, the `GoFrame` capture/claim
protocol, or the per-test panic report in `TestExecution` changes; only a panic that has already
lost every chance to be recovered reaches the printer.

## 3. The printer, and how it reaches a Go-spelled traceback

The printer is **`go.golib.CrashReport`** — golib, because golib is the only assembly both the test
host and every converted program share (`core/testing` references golib and `time`, and nothing
else).

golib cannot spell a Go frame name or map a converted `.cs` line back to its Go position: that
machinery — `goFrameName`, the receiver qualifier, `goFramePosition` and the position-map decode —
lives in `core/runtime`'s `managed_impl.cs`, which sits **above** golib. The dependency is therefore
inverted exactly as `RuntimeErrorPanic.IntegerDivideByZeroValue` already inverts it for the
divide-by-zero panic value: **golib declares the hook, `core/runtime` fills it from its own module
initializer.** That precedent is banked (math/bits' `TestDiv32PanicZero`) and its write-up states
this design's fallback rule in so many words — *"when nothing has registered … the panic falls back
to the plain message below, which still reads and prints identically."*

So:

* `CrashReport.TracebackRenderer` — `Func<PanicException, Exception, string>?`, installed by
  `core/runtime`'s existing `[ModuleInitializer]`. It returns elements 3 and 4 as one block, from
  the *same* `appendGoFrames` that `runtime.Stack` uses. No second renderer exists.
* With no renderer registered, the report is `panic: <value>` and nothing more — **byte-identical
  to what golib prints today**. An uninstalled renderer is therefore a strict non-regression, never
  a wrong report, and never a Go-shaped header over frames golib could only spell in .NET.

The alternative — relocating the ~400 lines of traceback machinery from `managed_impl.cs` into
golib so no hook is needed — was considered and refused: it rewrites machinery banked hours earlier
(rows #160–#161) for no behavior any consumer can observe, and the inverted hook is the
repository's already-ruled idiom for precisely this layering.

### Which stack the traceback comes from

`PanicTrace` first, then the exception that actually travelled:

* `PanicTrace` is the origin snapshot taken at the first `GoFrame` catch. It is the right answer
  whenever a panic passed through a deferred sequence, because re-raising a stored instance resets
  `Exception.StackTrace` to the re-raise point.
* When no frame ever caught it — a `panic()` in a function with no defer, which is exactly
  `TestMain`'s shape in `TestSetCrashOutput` — `PanicTrace` is null and the travelling exception
  still carries the throw site.
* For a synthesized runtime-error panic (nil dereference, divide by zero) the `PanicException` was
  never thrown at all, so the **original** .NET exception is the one with frames. That is why the
  renderer takes both the panic and the exception that travelled, rather than only the panic.

### `goroutine N`

The header is `goroutine 1 [running]:` — inherited verbatim from `runtime.Stack`, not decided here.
golib's goroutine ids are deliberately opaque and are not Go's numbering, so this arc reuses the
banked convention rather than minting a number. A panic on a non-main goroutine therefore reports
`goroutine 1`; that divergence predates this arc, is guarded where it is observable
(`sync`'s `TestOnceFuncPanicTraceback`), and retires with goroutine identity numbering, not here.

> **Amended 2026-11-05 (C1, S1).** Retired. The crash header now names the goroutine that panicked,
> through the same `appendGoroutineHeader` `runtime.Stack` uses, and its block ends with Go's
> `created by <func> in goroutine <parent>` and the `go` statement's position (`appendCreatedBy`).
> The escaping panic disposes the goroutine root's scope before the backstop reports, so the root's
> first-pass filter records the goroutine (`Goroutine.Crashing`). Main still reports `goroutine 1`;
> a thread with no goroutine identity keeps the literal header. Guarded by the behavioral test
> `CrashGoroutineCreatedBy`. `TestOnceFuncPanicTraceback` reads `debug.Stack()`, not this path.

## 4. The tee — `SetCrashOutput`'s fd

The slot already exists: `runtime/debug`'s hand-owned `runtime_setCrashFD` remembers an fd and is
inert. It moves to golib (`CrashReport.SetCrashOutputFd`), keeping Go's contract exactly — swap,
return the previous value, `^uintptr(0)` means unset — and `runtime_setCrashFD` forwards to it.
That placement is *more* faithful than the status quo, not less: in Go the slot lives in `runtime`
(`runtime.crashFD`, reached by `//go:linkname runtime_setCrashFD runtime.setCrashFD`), and golib is
this project's runtime library.

`CrashReport.Report` writes the composed report to stderr and, when the slot holds an fd, to that
fd as well. The asymmetry `TestSetCrashOutput` pins falls out of that and needs no rule: program
output reaches stderr through `println`/`os.Stderr` and never through this path, so the crash fd
only ever receives what `Report` hands it.

The fd is a real OS handle on both platform shapes (Windows: a `DuplicateHandle` result;
Unix: a dup'd descriptor), which is what `SafeFileHandle` means on each, so one `FileStream` over a
non-owning `SafeFileHandle` serves both. The handle is not closed — `SetCrashOutput` owns it, and
Go's own comment explains why it must outlive the caller's `File`.

> **2026-11-11 (C1, i9's sub-finding in run 42969401661):** the tee now covers every fatal report golib writes. Go
> copies whatever it prints while dying to the crash fd: a panic, a throw, a deadlock, main's Goexit. Census of golib's
> fatal-report writers:
> - Already teed: `CrashReport.Report` (a Go panic), `FatalReport.Fatal` (throw / fatal, and channel faults) and
>   `MainGoexit.Park` (main called Goexit).
> - Not teed, and fixed: the process backstop's NON-Go branch (a managed exception escaping a goroutine wrote
>   `ex.ToString()` to stderr only), its `unhandled exception:` fallback, and `builtin.fatal`, the deadlock path
>   ("all goroutines are asleep - deadlock!", five sites in channel.cs; a Debug build throws instead and so reaches
>   the backstop).
> - All three now write through one helper, `builtin.WriteFatalReport`: stderr first, guarded on its own, then
>   `CrashReport.WriteToCrashOutput`.
> - Left alone: `runtime/linux/signal_posix_impl.cs`'s "no tgkill/exit_group numbers" exit, a runtime-internal
>   refusal for an architecture with no syscall table, which is not a report Go would write.
>
> Guarded by GolibTests' `BackstopCrashOutputTests`. A child test host points the crash output at a file and its
> stderr at another, throws `InvalidOperationException` on a real goroutine, and dies through the backstop. The two
> files must be equal. Before the fix the crash file was empty.

## 5. Exit code

**Unchanged: 2.** golib already exits 2; the test host already returns 2. Nothing about process
termination moves, including the `finally` that removes the isolated run directory.

## 6. Blast radius, measured

Every converted program's unhandled-panic stderr gains lines. The **first line does not change** —
it is `panic: <value>` before and after — and that is the property every existing comparison rests
on:

* `BehavioralRunner`'s Output phase compares exit codes, full stdout, and **the first line of
  stderr only** (`Program.cs`, `FirstLine`, which also trims a trailing `\r`). Its own comment gives
  the reason: "Go's panic report appends a machine-specific goroutine stack trace, so a full
  comparison can never match; the first line carries the deterministic report." Adding elements 2–4
  *below* the first line therefore leaves every comparison's input identical, while the added lines
  match Go's shape where previously there were none.
* Census of behavioral projects containing `panic(` at all: 28, of which 19 recover. The guard
  written for this surface is **`GoroutinePanicExitCode`**, whose own header documents the
  first-line contract and asserts exit 2 with the report on stderr rather than stdout. No behavioral
  project carries a stderr **golden**; the comparison is against `go run` at run time, so there is
  nothing to re-baseline.
* No committed artifact anywhere in `src/tests` or `src/core` contains the string
  `System.AggregateException`, so nothing expects the framing being removed.
* Across the banked roster's committed test sources, exactly one file mentions `panic: ` as text —
  `regexp/exec_test.cs`, where it is a `fmt.Errorf` format string of the test's own making, not an
  observation of this surface.

The verification standard for the gate: any Output-phase movement in the full behavioral suite is
investigated **by name**. A comparison that moves *toward* Go is still a movement to be explained,
and a comparison that moves away is a defect.

## 7. Adversarial review (charter §7)

Three lenses, each a way the printer could make things worse than the dump it replaces.

**(a) A panic *during* the crash print.** The printer runs on a process that is already dying, and
it must never replace the panic's report with a report about the printer. So: the stderr write
happens **first** and independently, the tee is separately guarded, and every part of `Report` is
inside a `catch`-all that falls back to the single `panic: <value>` line. The renderer is invoked
inside that guard too — a renderer that throws (a hostile `MethodBase`, an assembly whose attributes
will not load) costs the traceback, never the report. `Environment.Exit(2)` is reached on every
path, including the failing ones, because the exit is sequenced after the printer and not inside it.
This is the same posture `readGoPositionMaps` already takes ("a traceback is diagnostic output, and
it must not be the thing that takes a program down").

**(b) A panic value whose rendering itself panics.** Already solved and deliberately reused rather
than reimplemented: `PanicException.Message` computes `PanicText(State)` once, lazily, inside a
`try` that answers `"panic while printing panic value"` — Go's own words for the same event. The
printer reads `Message`; it never touches `State`. Two consequences worth stating: the substitution
still happens exactly once and exactly at the moment Go performs it (`preprintpanics` runs only for
a panic that is about to be printed, which is precisely this path), and a recovered panic still
never invokes a user `Error()`/`String()` at all.

**(c) Double panic / recover, and the panic-ownership doctrine.** The printer is a **reader**. It
touches none of `GoFuncRoot`'s three slots — `CapturedPanic`, `HandledPanic`, `UnclaimedPanic` — and
does not call `recover()`, `GoFrame.Run`, `ArmPanicClaim` or `ClaimPanic`. It reads
`PanicException.PanicTrace`, which is set at capture and inherited on re-panic, and it reads it
only after every frame has declined to recover. A re-panic from a deferred call therefore reaches
the printer as one panic carrying the *original* origin — which is what Go's traceback shows for
the same program — and the frame-owned re-raise protocol is unchanged in both mechanism and
ordering. The one behavior that changes for the test host is that its outer `catch` stops
*describing* an escaped panic as an infrastructure error and starts *reporting* it as Go does; the
catch itself stays, so the `finally` still tears down the isolated run directory, and a genuine
infrastructure exception is still reported the way it always was.

## 8. Deliberately not in scope

* Go's `[recovered]` annotation and its nested-panic chain (`panic: A [recovered]\n\tpanic: B`).
  Nothing measures it; adding it would be invention.
* Frame argument words (`(0x1400007e0a0)`) and PC offsets (`+0x18c`). The banked frame rendering
  omits both and `TestStack` agrees with Go without them.
* `all goroutines` dumps. `runtime.Stack(buf, true)` cannot honestly answer them under the CLR (no
  supported cross-thread stack walk); that refusal is recorded at `runtime.Stack` and stands.
* Goroutine identity numbering — see §3.

## 9. Gates

golib and test-host change class, so:

| Gate | Why |
|:--|:--|
| `GolibTests`, with the new printer guards proven **failing-first** | format exactness including the blank line, the fallback shape, the tee asymmetry, and exit-code preservation |
| Full behavioral suite (`run-behavioral.ps1`) | golib is linked by everything; any Output-phase movement investigated by name |
| `go2cs.slnx --no-incremental`, 0 errors | the only gate that compiles the non-generated solution members after a golib/runtime API change |
| `runtime/debug` pipeline, `-test-action all` | the row this arc exists for: `TestSetCrashOutput` must pass all six assertions |

## 10. As built

Five files, no machinery relocated.

| File | What it is |
|:--|:--|
| `src/core/golib/runtime/CrashReport.cs` | new — the printer: the renderer hook, the crash-output descriptor slot, `TryUnwrapPanic`, `Format` and `Report` |
| `src/core/golib/builtin.cs` | the `AppDomain.UnhandledException` backstop reports through it; exit 2 unchanged |
| `src/core/runtime/managed_impl.cs` | `ᴛRegisterCrashTraceback` fills the hook from a module initializer, with `crashTraceback` composing the header over the same `appendGoFrames` `runtime.Stack` uses |
| `src/core/runtime/debug/stubs_impl.cs` | `runtime_setCrashFD` forwards to golib's slot; the local field is gone |
| `src/core/testing/TestHost.cs` | an escaped panic is reported as Go's crash, not as an infrastructure error; still `return 2`, so the `finally` still tears the run directory down |

Guarded by `src/tests/GolibTests/CrashReportTests.cs` — fifteen tests, of which **five fail against
the pre-arc state**, proven by neutering all three halves at once (composition, tee, registration):
format exactness including the blank line, the tee both directions, the composed stderr report with
and without a tee failure, and the registration itself. The other ten pin behavior the arc must
*preserve* — the fallback's byte-identical single line, Go's `preprintpanics` value rendering, the
unwrap through `Task.Wait`, and that `Report` returns rather than throws when every one of its
moving parts faults at once.

**What the measurement added.** The child's report reads exactly as Go's does, and its `TestMain`
frame names the **Go** file and line:

```
panic: oops

goroutine 1 [running]:
runtime/debug_test.TestMain()
	runtime/debug/stack_test.go:37
go.testing_runtime.TestHost.RunTests()
	.../src/core/testing/TestHost.cs:271
…
```

That mixture is the position map's doctrine working as ruled, not a defect: `TestMain` is converted
Go and reports a Go position, while the frames beneath it are the hand-owned host and the BCL, which
have no conversion relationship to any Go source and honestly report their own. Go's own report has
`main.main()` and `_testmain.go` there. The three strings `TestSetCrashOutput` requires —
`panic: oops`, `goroutine 1`, `debug_test.TestMain` — are present in both the crash file and stderr,
`hello` is in stderr and not in the crash file, and the child exits 2.
