# Async twins: goroutines on a small pool of threads

By default, each goroutine in a converted program runs on its own operating-system thread. A goroutine that waits,
on a channel, a lock or a timer, keeps that thread while it waits. Go itself runs goroutines on a small pool of
threads instead: a goroutine that waits gives its thread back, and another goroutine runs on it.

**Async twins** let a converted program work the same way. They are experimental and opt-in: a program gets them
only when it is built with twins and run with the scheduler switched on, as [How to opt in](#how-to-opt-in) describes.
A program that does not opt in runs exactly as it does without them.
<!-- 2026-11-24 (P4, row 18; COORD 2026-11-21 15:45Z, the owner's request; G's fact sheet 16:07Z and its correction
     23:39Z, re-read at master cb6def82a2). Written against master cb6def82a2. Passages marked (b)-dependent describe
     the standard library as it ships twinned or not; COORD names the shipping form when .5's scope freezes, and each
     is one edit. -->

## What an async twin is

An async twin is a second copy of a function that can wait without holding a thread. The source generator in every
converted project writes it beside the function, with the same name followed by `ꞏtwin`. Where the function blocks
its thread, its twin **parks**: it returns to the scheduler, which runs another goroutine on the same thread, and the
goroutine resumes, on whichever thread is free, when what it waited for arrives.

The twin of a function returns a task, a `GoTask` (or a `GoTask<T>` for a result), and its body awaits the twins of
the functions it calls. So a goroutine started on a twin runs as a chain of twins, each awaiting the next, down to the
operation that actually waits: a channel send or receive, a `select`, or a `time.Sleep`. A `sync.Mutex.Lock` that has
to wait parks as well, but only inside a function that can also wait on one of those
([Which code gets twins](#which-code-gets-twins)).

A public function that can never wait still gets a twin, so every caller can await it the same way. That twin is a
**forwarder**: it calls the plain function and returns a task that has already completed.

You do not call twins, and you do not see them in your converted code. The generator writes them into the build, and
the converted program's entry point starts `main`'s twin when the program is built with twins.

## Which code gets twins

- **Your projects, when you build them with twins.** Every public member of the converted code gets a twin:
  functions, methods, promoted methods of embedded structs and interface members. Members that are not public get one
  where the analysis needs it.
- **Two operations in the standard library, always.** <!-- (b)-dependent --> `sync.Mutex.Lock` and `time.Sleep` have
  twins written by hand. A `time.Sleep` parks in any twinned function. A `sync.Mutex.Lock` that has to wait parks only
  inside a function that can also wait on something else: a channel operation, a `select`, a `time.Sleep`, or a call
  that parks. A function whose only wait is `Lock` keeps its plain form, and a contended `Lock` there blocks its thread.
  Channel operations and `select` park in the runtime library itself.
  <!-- src/core/sync/mutex.cs, Lockꞏtwin: [GoParkWeak], "a call to Lock never makes its caller a twin"
       (P3's read, 2026-11-24). src/core/time/time_impl.cs, Sleepꞏtwin: no weak mark. -->
- **The rest of the standard library.** <!-- (b)-dependent --> The published `go.*` packages are built without twins.
  A standard-library call from a twinned program, such as a file read or a `sync.WaitGroup.Wait`, is a plain call,
  and the goroutine keeps its thread until the call returns.
- **Hand-written bodies.** A function whose body is written by hand rather than converted gets a forwarder twin,
  unless its author wrote the twin too, as for `sync.Mutex.Lock`. A forwarder runs the plain body, so a wait inside it
  keeps the thread.
- **Not twinned:** generic functions, methods of generic types and members of generic interfaces. A call to one of
  them is a plain call.
<!-- docs/phase4/DESIGN-mn-twin-set.md section 3: R1 population (promoted-method forwarders, interface members), R3
     forwarders, R5 hand-owned members get the forwarder unless an author-written twin replaces it, R6 unexported,
     E1 generic exclusion. Frame naming: src/core/runtime/managed_impl.cs (goTwinOrigin, "A twin's frame is named, and
     started, as the function it was generated from"). -->


## How to opt in

Opting in takes two switches: one when you build, one when you run.

**The build.** Build the projects with twins. The properties that do this are read by go2cs's build files, so they
work only for a project that sits under the `src` directory of a go2cs repository clone. A project anywhere else,
such as a `-recurse` output root outside the clone, builds without twins and says nothing.

- `GoTwinsFor` names the projects to build with twins: their project file names without `.csproj`, separated by `;`.
  Set it in the environment, or on the `dotnet build` command line with each `;` written as `%3B` (MSBuild otherwise
  ends the property at the `;`). Name every converted project of your program that should park, the executable
  included.
- `GoTwins=true` in one project's own file also builds that project with twins, but go2cs writes the project file
  again on each conversion, and the property is lost then, with no error. Use `GoTwinsFor`.

Setting `GoTwins` on the command line is not the same thing: a command-line property reaches every project the build
references, standard-library packages included. Use `GoTwinsFor` on the command line instead.
<!-- src/go2cs/projectFileWriter.go (needToWriteFile): the converter rewrites the project file and carries over only
     the GoHandOwnReferences block (P3's read, 2026-11-24; read from the code, not run). GoTwinsFor compares
     $(MSBuildProjectName) in src/Directory.Build.targets. -->

**The run.** Set the environment variable `GO2CS_MN=1` when you start the program. How many goroutines run at once
follows `GOMAXPROCS`, as in Go: the `GOMAXPROCS` environment variable at start, and `runtime.GOMAXPROCS(n)` while
the program runs.

Without `GO2CS_MN=1`, a program built with twins runs each goroutine on its own thread, as it would without them.

**How to tell it is on.** Run with `GO2CS_MN_READING=1` as well. The scheduler then writes one line to standard error
when the program exits, starting `READING go-scheduler`, with what it counted: `parks=` is how many times a goroutine
parked, and `peak-os-threads=` is the most threads the program ran at once. No line means the scheduler never
started: `GO2CS_MN` was not `1`, or no goroutine started on a twin, for example because the executable was not named
in `GoTwinsFor`. The line is for reading, and its fields may change between releases.
<!-- src/core/golib/runtime/GoScheduler.cs: GO2CS_MN is read as exactly "1"; the line is registered in Start(), which
     runs at the first Ready; Reading() writes parks= and peak-os-threads=. -->

**An example.** A program whose executable project is `App.csproj` and whose library project is `Lib.csproj`, both
under the clone's `src` directory:

```shell
dotnet build -c Release -p:GoTwinsFor=App%3BLib App.csproj
GO2CS_MN=1 GO2CS_MN_READING=1 dotnet run -c Release --no-build --project App.csproj
```

## What you get, and what it costs

**What you get.** A program with many goroutines that spend most of their time waiting no longer needs a thread for
each of them. Goroutines that wait in twinned code park and give their thread back, so the program runs on a pool of
threads near `GOMAXPROCS` in size instead of one per waiting goroutine.

**What stays on a thread.** A goroutine still holds its thread while it is inside a plain call that waits: a call into
code built without twins <!-- (b)-dependent -->, a function whose only wait is a `sync.Mutex.Lock`, a hand-written body, a native call, or a call the generator could not make awaitable (the
diagnostics below name each such site). The scheduler adds a thread when all of its threads are held this way, so
the program keeps running, but those goroutines cost a thread each, as they do by default.

**Build cost.** A project built with twins compiles a twin for each public member, so its build takes a little longer
and its assembly grows by a little. A project built without twins is unchanged.
<!-- G's fact sheet (2026-11-21), Windows x64, MnHttpClosure (186 projects, 184 twinned): go.*.dll +0.51%, build wall
     1.016x; Native AOT +185,856 bytes on the widest program (+0.09%). Measured on Windows x64 only. -->

**Run cost.** A call through a twin that does not park costs a few nanoseconds more than the plain call, and a park
and resume costs under a microsecond (measured on Windows x64). Code that waits rarely and computes a lot gains
nothing from twins.
<!-- G's fact sheet: non-parking twin call 1.9-2.2 ns scheduler off, 4.0-9.3 ns on (JIT); one park step 625-711 ns
     (JIT), 764-805 ns (Native AOT); src/tools/mnspike2/TwinRefs/FREEZE-SHEET.md. -->

## Limits

- **Experimental.** The scheduler and the twins are new, and are off unless you opt in. Report anything that behaves
  differently with them than without them.
- **Repository clones only.** The build properties work only for a project under the `src` directory of a go2cs
  repository clone. <!-- (b)-dependent --> A project that references the published `go.*` packages from NuGet cannot
  build with twins.
- **Stack depth.** With the scheduler on, a goroutine started on a twin runs on one of the pool's threads. Those
  threads reserve the same stack as a goroutine's own thread: 256 MiB, which `GO2CS_GOROUTINE_STACK` changes
  ([Limitations](Limitations.md#what-converts-but-doesnt-behave-exactly-like-go)). `main` runs there too, unless an
  `init` function locked it to its thread, so with the scheduler on `main` has that reserve rather than its usual
  1 GiB. If the system refuses the size, the scheduler halves it until a thread starts, and says so once on standard
  error.
  <!-- Post-row-4 truth (R's M:N row 4, claude/r-mn-row4-stack c1639909fc, train 11 with this page; COORD 2026-11-24
       17:32Z): GoScheduler s_carrierStackSize = Math.Max(Goroutine.StackReserve, MinCarrierStackSize = 1 MiB), halving
       on refusal with one named stderr line. Before it, carriers reserved a fixed 1 MiB (GoScheduler's
       CarrierStackSize at cb6def82a2). Goroutine.Multiplexed.cs RunMainTwin: goroutine 1 runs main's twin multiplexed, except when an init
       locked the main goroutine to the startup thread. -->
- **`runtime.LockOSThread`.** A goroutine that locks its thread keeps that thread to itself until it unlocks it, as in
  Go; the thread leaves the shared pool while it is locked.
- **Profiling.** With the block or mutex profile switched on, a `sync.Mutex.Lock` that has to wait takes the plain path
  and blocks its thread, so the profile records it exactly as it would without twins.
- **Expression order.** In an expression such as `fmt.Println(a, f())`, Go orders only the calls, so the read of `a`
  may come before or after `f()`. Converted code reads operands left to right by default; with twins, a call that can
  wait is sometimes evaluated ahead of the plain reads beside it. Both orders are allowed by the Go specification
  ([Known issues](KnownIssues.md#goroutines-on-a-shared-pool-of-threads-mn-are-experimental-and-off-unless-you-build-and-run-for-them)).
- **Stack frames.** A frame inside a twin is named for the function the twin was generated from, so stack traces and
  `runtime.Caller` read as they do without twins.

## Diagnostics

These come only from a build with twins. Each has a row in [Diagnostic codes](DiagnosticCodes.md).

- `GO2CS0014` (warning): a function that calls through a func value cannot get a twin, so that call stays plain.
- `GO2CS0015` (warning): a `go` statement starts its goroutine plain, on a thread, though the goroutine could park.
- `GO2CS0017` (error): a public member of a package built with twins got no twin. Report it as a go2cs issue,
  with the message.
- `GO2CS0018` (error): a build property that changes what the twin generator writes was set in a build with twins.
  Remove it.
- `GO2CS0019` (warning): an entry that marks a call as not parking, in the `GoTwinsWeak` or `GoTwinsCut` property or
  in go2cs's own table for the standard library, does nothing in this build.
- `GO2CS0020` (warning): a call through a func value whose shape the generator cannot test. If the function stored
  there parks, the call holds its thread.
- `GO2CS0021` (error): a method that can park is in a type not declared `partial`, so it cannot get a twin.

With `WarningsAsErrors`, the warnings stop the build.

## Future

A future release may enable async twins by default. Until then they are opt-in, and a program that does not opt in
runs each goroutine on its own thread.

## Related

- [Known issues: goroutines on a shared pool of threads (M:N)](KnownIssues.md#goroutines-on-a-shared-pool-of-threads-mn-are-experimental-and-off-unless-you-build-and-run-for-them)
- [Diagnostic codes](DiagnosticCodes.md)
- [Conversion strategies](ConversionStrategies.md), for how goroutines, channels and `select` convert
<!-- Design records (excluded from the published site): DESIGN-mn-scheduler-sizing.md, DESIGN-mn-scheduler.md,
     phase4/DESIGN-mn-twin-set.md, phase4/DESIGN-mn-nopark.md. -->
