# `runtime.Goexit` from the main goroutine — a design

> **STATUS: PROPOSED (lane C2, 2026-10-25; branch `claude/c2-design-goexit-main`, cut from the 1.26 hop tip
> `9b0269c2e8`).** Design only: no code lands from this note. It is ruled for the **first .2 seat**, not the hop. The
> mechanism changes the filter and the tail of golib's deferring frame, the hottest exception path the runtime has,
> and after the freeze that puts every roster row in its re-read set.
>
> This is option A of [the Goexit design record](phase4/DESIGN-goexit.md) §3. That record's option C, the shape
> golib ships today, refuses a Goexit from the main goroutine. It asks for the rulings listed in
> [Open questions](#open-questions).

## What Go does

`runtime.Goexit` ends the goroutine that calls it after running its deferred calls, and `recover()` in those calls
returns nil. Called from the main goroutine, it ends `main` without returning, and the program keeps running its other
goroutines. When none of them can run any more, the scheduler's dead-program check reports one of two fatal errors and
the process exits with status 2:

- **No user goroutine is left:**
  `fatal error: no goroutines (main called runtime.Goexit) - deadlock!`
- **Some are left, every one blocked, and no timer pending:**
  `fatal error: all goroutines are asleep - deadlock!`

Until then the program behaves normally. Another goroutine can call `os.Exit`, which ends the process with its own
status, or panic, which prints its report and exits 2.

A Goexit can reach `main` through a library. When a `seq` function passed to `iter.Pull` calls `runtime.Goexit`, the
next call to `next` propagates it into its caller, which can be `main`.

<!-- Go side read at go1.26.10: GOROOT/src/runtime/proc.go:6424-6426 (the grunning == 0 report) and :6468 (the
all-asleep report); GOROOT/src/iter/iter.go (next re-raises a seq's Goexit). Go's own tests of the shape:
GOROOT/src/runtime/crash_test.go TestGoexitDeadlock, TestGoexitCrash, TestGoexitInPanic,
TestRecoveredPanicAfterGoexit, TestRecoverBeforePanicAfterGoexit and TestRecoverBeforePanicAfterGoexit2. -->

## What go2cs does today

A Goexit on any goroutine other than main is fully supported: it throws golib's `GoexitException`, the deferring
frames run their deferred calls during the unwind, and the goroutine root swallows the exception. From the main
goroutine, `runtime.Goexit` throws `NotSupportedException` naming the Goexit design record, and the program exits 2
with that message instead of Go's.

<!-- src/core/runtime/managed_impl.cs:1499 (`if (!Goroutine.OnGoroutine)`), :1511 (`throw new GoexitException();`).
The goroutine root: src/core/golib/runtime/Goroutine.cs:373 (`public static bool OnGoroutine => t_current is { IsMain: false };`). -->

## Why the goroutine shape does not carry over

A converted program's `main` is the CLR entry point, and nothing catches an exception above it. When an exception
escapes the entry point, .NET runs the unhandled-exception handler and then ends the process **without running any
`finally` block** on the way out. A `GoexitException` thrown on the main thread would therefore skip every deferred call
in `main` and in the functions it is running, which Go runs first. Measured on linux/amd64 with .NET 10: a
`throw` inside nested `try`/`finally` blocks below `Main` printed the handler's line and none of the `finally` lines.

A Goexit from main therefore needs a frame that catches the exception above every deferred call it must run.

## The design

Every converted function that defers already has such a frame. Its emitted shape is a `try` whose `catch` is filtered
by `GoFrame.IsPanic` and whose `finally` ends in `ᒐ.Run()`, the call that runs the frame's deferred calls. A deferred
receiver-field call that the converter lowers into plain code sits in that same `finally`, so it is covered too.

<!-- src/tests/Behavioral/DeferFinallyLowering/main.cs.target:37 (`catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }`),
:38 (`finally { if (ᒐd2) Ꮡx.DerefOrNull().b.done(); if (ᒐd1) Ꮡx.DerefOrNull().a.done(); ᒐ.Run(); }`). -->

The design uses those frames as the chain of catch points:

1. **Goexit on main marks the main goroutine as exited** and throws a `GoexitException` that records it came from main.
2. **The innermost deferring frame catches it.** `GoFrame.IsPanic` accepts a main-goroutine `GoexitException` through a
   marker that `recover()` cannot observe, so the frame's `finally` runs its deferred calls and `recover()` in them
   returns nil, as it does for a goroutine's Goexit.
3. **`GoFrame.Run` throws a fresh main-goroutine `GoexitException` at its end.** An exception thrown from a `finally`
   replaces the `catch` block's `return`, so the function does not return to its caller. The next deferring frame out
   catches it in turn, and the chain repeats until the outermost one.
4. **The outermost throw reaches golib's main-thread backstop with every deferred call already run.** There the main
   thread parks. It waits on the live-goroutine registry, which signals when its count of user goroutines falls to
   zero, and then prints `fatal error: no goroutines (main called runtime.Goexit) - deadlock!` and exits 2.
5. **The dead-program check counts an exited main as blocked forever.** golib's forever-wait detector reports
   `all goroutines are asleep - deadlock!` when every remaining user goroutine sits in a wait nothing can end and no
   timer is pending; with main exited, the same rule applies to the goroutines that are left.

`os.Exit` and a panic on another goroutine keep their existing paths: both end the process from their own thread
while the main thread is parked.

<!-- The filter: src/core/golib/GoFrame.cs:672 (`public static bool IsPanic(Exception ex, [NotNullWhen(true)] out PanicException? panic)`),
:683 (`if (ex is not GoexitException)`). A Goexit raised INSIDE a deferred call is already caught there:
:366 (`catch (GoexitException goexit)`). The registry and the detector: src/core/golib/runtime/Goroutine.cs:1281
(`private static void Unregister(Goroutine goroutine)`), :911 (`internal static bool AllGoroutinesForeverBlocked()`),
:958 (`internal static bool IsProvableDeadlock(`). The backstop: src/core/golib/runtime/MainExit.cs:109
(`internal static void InstallBackstop(UnhandledExceptionEventHandler backstop) =>`). The deadlock text:
src/core/golib/runtime/FatalError.cs:16 (`return "all goroutines are asleep - deadlock!";`). -->

### What it leaves out

- **A plain C# `finally` with no deferring frame above it.** Hand-written golib or standard-library code on main's
  stack whose cleanup is a C# `finally`, and which has no converted deferring function above it, does not run that
  cleanup, because the exception still ends unhandled. Converted code is not affected: every Go `defer` sits in a
  deferring frame.
- **A deadlock on ordinary waits.** When the remaining goroutines block on ordinary channel or `sync` waits, Go reports
  "all goroutines are asleep" and golib keeps blocking. That is golib's existing rule for every program: only a wait
  nothing can end is provably dead in the managed runtime, where a timer, an I/O completion or a host thread can still
  wake a goroutine.

### The alternative

A generated entry point would catch above `main` itself, with no residual. go2cs-gen emits a class whose `Main`
calls the converted `main` inside a `try`, and the project's `StartupObject` names it. Converted code is unchanged, but
the csproj template gains the `StartupObject` line, so every program's project file moves. In the behavioral corpus
alone that is most of the projects, since most are programs. It also has to be squared with the test host, which
brings its own `Main`. This design does not recommend it for the size of the residual it removes.

<!-- Behavioral corpus at 9b0269c2e8: 854 of 937 tracked src/tests/Behavioral/*/*.csproj files are <OutputType>Exe. -->

## Owner and footprint

- **Owner: golib.** `GoFrame.cs` (the filter and the re-throw), `runtime/Goroutine.cs` (the exited-main state and the
  count signal), `runtime/MainExit.cs` and `runtime/CrashReport.cs` (the main-thread park), and `FatalError.cs` (the
  "no goroutines" text). Plus one hunk in the hand-owned `src/core/runtime/managed_impl.cs`, where the gate becomes the
  throw.
- **Converter and go2cs-gen:** no change.
- **Converted files:** none move. No standard-library code calls Goexit from the main goroutine; its callers
  (`iter.Pull`'s `next`, `os/user` on windows, `testing`) run on goroutines or on the test host's test goroutines.
- **Goldens:** one new behavioral project, `GoexitFromMain`. Nothing re-baselines.
- **Docs:** this note's as-built section; the Goexit design record (pointer to the as-built); the main conversion page's
  Defer / Panic / Recover section and the Reference manual-conversions page, both of which describe the gate.

## Risks

- **The hot exception path.** `GoFrame.IsPanic` runs for every exception that crosses a deferring frame, and
  `GoFrame.Run` runs at the end of every deferring call. The added work is one type test on the exception path and one
  flag test at `Run`'s end, with nothing on the normal return path. After the freeze, a change here puts every row in
  the re-read set, which is why it waits for the .2 seat.
- **A wrong catch.** If the filter accepted a goroutine's `GoexitException`, a goroutine's Goexit would stop at the
  first deferring frame and return to its caller. The marker must come only from the main goroutine, and the
  `GoexitDefers` guard must stay byte-identical.
- **Park and exit ordering.** The main thread parks inside the unhandled-exception handler while other threads may call
  `os.Exit` or report a panic. `MainExit` already serializes the fatal report and the process exit; the park must take
  part in that ordering rather than add a second exit path.

## Measurement plan

1. **Red first.** Each arm below fails at the tip on the gate's `NotSupportedException`.
2. **The arms,** in one behavioral project `GoexitFromMain`, each compared with `go run` on stdout, the first stderr
   line and the exit code:
   - (a) main defers run, `recover()` in them reads nil, a goroutine finishes its work, then the "no goroutines" fatal
     and exit 2;
   - (b) a goroutine calls `os.Exit(0)` after main's Goexit: exit 0;
   - (c) a goroutine panics after main's Goexit: its panic report, exit 2;
   - (d) the remaining goroutine sits in `select {}`: "all goroutines are asleep", exit 2;
   - (e) the Goexit is raised two deferring frames below `main`, so the re-throw chain runs;
   - (f) the Goexit reaches `main` through `iter.Pull`.
3. **Controls.** `GoexitDefers` (the goroutine shape) and `GoroutinePanicExitCode` stay byte-identical, and GolibTests
   pass in Debug and Release.
4. **Re-reads.** At the .2 seat, the rows the seat's train re-reads; the small rows that exercise Goexit (`sync`'s
   `TestOnceFuncGoexit`, `iter`'s pull tests, `testing`'s panic tests) are read first. The runtime row's own Goexit tests
   build their child programs with the real Go toolchain, so they do not reach this path.
5. **Cost.** The Performance suite's panic and defer cases, JIT and Native AOT, before and after.

## Open questions

1. **The marker's shape.** `IsPanic`'s `out` parameter is a `PanicException` that `recover()` reads. The marker can be a
   dedicated non-recoverable subclass, or the filter can return the exception through a second channel. The first keeps
   the emitted `catch` unchanged.
2. **The alternative's future.** If the residual of hand-written `finally` blocks ever matters, the generated entry point
   can follow as its own increment. Should the .2 seat size it alongside?
3. **The test host.** A test binary's `TestMain` runs on the main goroutine. This design covers it the same way, through
   the deferring frames above the Goexit; whether the test host should instead treat it as a failed run is a separate
   choice.

<!-- Provenance. Lane C2, 2026-10-25. The sizing this note expands: COORD inbox 2026-10-25T19:05:37Z, ruled the same
day: deferred to the first .2 seat, write the design now. The .NET measurement: a net10.0 console program with
`AppDomain.CurrentDomain.UnhandledException += (s, e) => { Console.WriteLine("handler " + …); Environment.Exit(2); };`
and `try { Inner(); } finally { Console.WriteLine("finally main"); }` around a throwing `Inner` that has its own
finally printed only "handler X" and exited 2, on linux/amd64, .NET SDK 10.0.112. -->
