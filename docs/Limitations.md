# Limitations: where the seams are

So you're about to point go2cs at your own Go code. Before you spend an evening on it, here is where it
will fail, where it will fight you, and where it will quietly do something a little different from Go --
better you hear it from me than from a stack trace. Specific bugs with workarounds live in
[Known issues](KnownIssues.md). This page is the limits that come with running Go on .NET, plus the rough
spots real code hits today.

<!-- Tightened 2026-11-15 (P4, owner request relayed by COORD: "a more concise read", with the cgo items grouped). The
     cgo items of the old "What won't make the trip" and "What converts, but doesn't behave exactly like Go" sections
     moved, with their provenance comments, to the new "cgo" section; the rest was shortened, merged or moved, and
     nothing still true and user-facing was dropped. Figures that no standing process re-measures are stated in words;
     each comment keeps the measured numbers. The pre-tightening text is this file at master 091935257c. -->

This page describes go2cs as it is on the master branch and in the 1.26.11.1 release. Where the two differ, it says
which one a limit applies to.
<!-- 2026-11-23 .5 docs pass (P1), at master 97cca467a2 (owner rule of 2026-11-22: the current state, in present tense).
     Every bullet was read against master 97cca467a2 and the 1.26.11.1 tag (nuget-1.26.11.1, ec3e170e4b). No entry
     here affects only releases before 1.26.11.1, so nothing moved to an older-versions section. Changes:
     - this paragraph;
     - the func-word bullet: master only (TC-13, train 5, claude/h-tc13-funcword, merge 3d7d5e6ec0); 1.26.11.1 loses
       the store; "until cgo callbacks bind functions to code addresses" restated (UX-8);
     - the unsafe header views bullet: "The general fix is planned" restated (UX-8);
     - testify: objx's suite validates on master (train 6, merge bb6267d0e4; no objx row at the tag);
     - cgo, "What converts": reading C memory through a C struct's pointer field and through **C.T, master only
       (trains 4 and 6: claude/h-cgo-sb2a, merge 1f2a9afdfc; claude/h-cgo-1c, merge 5f2f2d0aa3);
     - cgo, "One target": -recurse -cgo, cross-target CC, the per-RID pack and the load-time target check are master
       only (train 3: claude/c2-cgo-nuget-pack, merges 8de0f7ece2 and 640d45c9eb; claude/c2-cgo-pack-2b, merge
       ef9be9bd32; train 4: claude/c2-cgo-a6, merge c9143ae615; train 5: claude/c2-cgo-gopointer, merge 54b02b555d);
       "still to come" and "is planned, and is what will change this" restated (UX-8);
     - cgo, "Where it differs from Go": the "five places" count (UX-13) checked against the list and laid out as five
       cases in two groups. -->

## The short version

**Good odds tonight:** pure Go that builds with the Go release go2cs tracks and `CGO_ENABLED=0`, leans mostly on the
standard library, and runs on Windows, Linux or macOS. **A longer evening:** lots of third-party modules,
`unsafe`, assembly fast paths, cgo, or tests that pull in testify.

* **Your code must build with the Go release go2cs tracks**, the one in
  [Requirements](README.md#requirements), which the go2cs dotnet tool downloads for you.
* **cgo converts only through `-cgo`, and only calls of C functions a shared library defines** ([cgo](#cgo)).
* **Assembly, `//go:linkname` and deep `unsafe` tricks compile clean, then fail at run time.** Many
  libraries ship a pure-Go fallback behind a build tag. A plain `-recurse` applies the common ones (`safe`, `purego`,
  `noasm`) for you; an explicit `-tags` replaces them, so restate them beside your own.
* **By default, each live goroutine holds an operating-system thread.** Thousands alive at once are fine; Go's
  million are out of reach.
* **It's usually slower than Go**, starts slower and uses more memory.
* **One conversion targets one operating system.**
* **Most failures are loud. A few differences are silent**: see the last bullet under
  [what behaves differently](#what-converts-but-doesnt-behave-exactly-like-go).

## What won't make the trip

* **cgo beyond calls of shared-library C functions**: see [cgo](#cgo).
* **A self-referential constraint over a pointer, when a method shares the element's storage.**
  `Walk[*Node]` under `Walk[N Tree[N]]` with `Children() []N` stops the run with `error GO2CS2002`,
  naming the site and the method. Two related shapes over a pointer element are refused the same way, as
  `GO2CS2005` and `GO2CS2006` ([diagnostic codes](DiagnosticCodes.md)). A pointer element reaches the generic code through a proxy object, and
  a `[]N`, `map[K]N`, `chan N` or `*N` the proxy passed along would be a copy, not Go's shared value.
  The same function over a value element converts, and so does a constraint whose methods pass only
  `N`, funcs and arrays ([how](ConversionStrategies-Reference/generic-constraints.md#what-a-proxy-forwards-and-the-instantiations-go2cs-refuses)).
  No Go 1.26 standard-library package reaches it.
  <!-- The refusal exits non-zero (docs/DiagnosticCodes.md, GO2CS2002), and the converted standard library is
       a complete -stdlib conversion of Go 1.26.10, so no package in it reaches the refusal. -->
* **Go newer than the library tracks.** A newer `go` line in your `go.mod`, or any dependency's, gets
  `package requires newer Go version` or a refusal naming both releases. Set `GOTOOLCHAIN=local` and pin
  older versions until `go build ./...` passes, as the
  [module walkthrough](ConvertingAModule.md#step-by-step) does.
* **Hand-written assembly.** A function whose body lives in a `.s` file usually becomes a stub. The
  conversion names each one (`warning GO2CS2012`, [diagnostic codes](DiagnosticCodes.md)), the
  project builds clean, and the first call throws a `NotImplementedException` saying
  `no implementation reached this compilation`. Convert with the library's pure-Go build tag
  ([below](#third-party-modules-where-they-trip-today)), or write the body in C#. `//go:linkname` into
  the runtime gets the same stub, outside a short list the converter knows.
* **One generics form.** `type Cell[T any] = struct{ Row, Col T }`, a generic alias of an anonymous
  struct or interface, gets a warning, and every use is a C# compile error. Name the struct instead.
* **Code that doesn't type-check for the target** still converts, with the literal text `invalid type`
  where a type is missing (`error GO2CS1003`), and the C# build stops there.

## What converts, but doesn't behave exactly like Go

* **Reflection cannot read a struct whose arrays are held inline.** When a package reads native memory as a
  struct, through `(*T)(unsafe.Pointer(p))`, go2cs lays that struct out as Go does and holds its array fields inside
  it. Indexing, `len`, `range` and printing one of those arrays work as in Go. Reflection over the struct that holds
  them does not: `fmt.Printf("%v", s)`, `reflect.ValueOf(s).Field(i)` and anything else that walks the struct's
  fields panics with `reflect: field T.F is a Go array held inline (go.arrays); reflection does not read or address
  it`. Print the fields one at a time instead.
  <!-- docs/phase4/DESIGN-array-value-storage.md 7.7 and its part-D block (I0, D3). Measured 2026-11-13 (C2) on the
       InlineArrayFields behavioral fixture with two lines added: fmt.Printf("%v\n", x) over an admitted Named panicked
       "reflect: field Named.FzName is a Go array held inline (go.arrays); reflection does not read or address it"
       where Go printed {6 [115 101 99 111 110 100 0] 4.5}; fmt of the field alone (h.Buf) matched Go. The census at
       part D's P1 counted 0 fmt %v and reflect uses reaching an admitted type in modernc.org/sqlite's lib and libc. -->
* **A slice of an array held inline panics when the struct around it also holds a string, a slice, a map, an interface
  or a pointer.** A slice of such an array (`p.ID.Data4[:2]`) uses the array's own bytes, as in Go, so go2cs holds
  the storage still for as long as the slice lives. It cannot hold still a struct that holds a Go reference, so the
  slice panics with `inline array slice: an array held inline in a struct that also holds a Go reference cannot be
  held still, so it cannot be sliced`. This is ordinary Go, and programs that use go-winio will hit it when they keep
  a GUID beside a string: `type P struct { ID guid.GUID; name string }`, then `p.ID.Data4[:2]`. go-winio's own GUID
  methods are not affected, because each slices its own copy of the GUID. Two forms work today: a `copy` into or out of
  the array with constant bounds (`copy(buf, p.ID.Data4[:])`), and slicing a local copy of the array
  (`d := p.ID.Data4; s := d[:2]`) when writes need not reach `p`.
  <!-- P3's review of go-winio Part 2, item 3b, and COORD's ruling 3b (2026-11-15): disclosed for .4, the cure is a .5
       BOARD row (a slice backing that is an object plus a field offset, with no pin). The arms that assert the named
       panic are GolibTests' InlineSliceStorageTests.AFieldOfAReferenceBearingRootIsRefusedByName and the refroot mode
       of TestInlineArrayForeignSliceReviewShapes (src/go2cs/inlineArrayForeignSlice_test.go). -->
* **By default, goroutines are operating-system threads** ([why](ConversionStrategies.md#goroutines)). A goroutine
  holds its thread for as long as it lives; when it ends, the thread runs the next `go` statement, so starting
  goroutines one after another is cheap and holding many alive at once is not. On Windows x64, ten thousand parked
  goroutines cost several times Go's memory and seconds to start, a hundred thousand take minutes, and a million do
  not finish starting. With threads, `runtime.GOMAXPROCS(1)` does not make anything run one at a time. Bound your fan-out with a
  worker pool, and remember that a `net/http` server spends one on every open connection. On macOS the kernel caps
  the threads one process may run (`kern.num_taskthreads`, a few thousand on the hosted macOS 15 machines measured),
  and the `go` statement past it ends the program
  ([Known issues](KnownIssues.md#on-macos-a-go-statement-past-the-systems-thread-limit-ends-the-program)).
  <!-- Measured by C1 on 2026-11-08 (run 42497561630, claude/c1-darwin-threadcap a85e0cf11, the GolibTests probe
       DarwinGoroutineThreadCapProbe), on GitHub-hosted macOS runners, both shipped mac RIDs:
       - osx-arm64, macOS 15.8.2: kern.num_taskthreads 2048; the first go statement to fail found 2,027 live parked
         goroutines;
       - osx-x64, macOS 15.7.9: kern.num_taskthreads 3072; the first failure at 3,051.
       Each is the cap less about 21 runtime and host threads, exact to the thread. The go statement itself throws
       System.OutOfMemoryException, synchronously, from the thread start at src/core/golib/runtime/Goroutine.cs:1352
       (`GoroutineThreadPool.Run(() => Run(body, goroutine, bubble), stack);`); a smaller stack reserve does not raise
       the cap. Found when internal/synctest's TestWaitGroupManyBubbles (about 10,100 live goroutines) died on both mac
       runners at the 1.26.10.3 re-bank. A developer Mac's cap may differ. -->
  <!-- [Sizing M:N scheduling](DESIGN-mn-scheduler-sizing.md) has the measurements and what removing
       the limit would take (internal record, unlinked 2026-10-26, OQ-1). -->
  <!-- Thread reuse: src/core/golib/runtime/Goroutine.cs:1352 `GoroutineThreadPool.Run(() => Run(body, goroutine, bubble), stack);`.
       The figures are lane R's ParkStorm reading in docs/DESIGN-mn-scheduler-sizing.md §1 (Windows x64, 16 hardware
       threads, 31 GB RAM, Go 1.26.10, Release, today's 256 MiB stack reserve): 10,000 parked at 622 MB and 4.4 s against
       Go's 99 MB and 0.07 s; 100,000 at 5,904 MB and 302 s; 1,000,000 never parked within 600 s. -->
* **A deadlock usually hangs** where Go prints `all goroutines are asleep - deadlock!`. The message
  appears only when every goroutine, `main` included, waits on a nil channel or an empty `select {}`, and then
  with Go's goroutine blocks. A deadlock on an ordinary channel or lock blocks forever: break in with a debugger,
  where, by default, each goroutine is a thread named `goroutine-N`.
  <!-- src/core/golib/runtime/Goroutine.cs:1096 `internal static bool IsForeverWait(WaitReason reason) =>`
       and :1169 `internal static bool IsProvableDeadlock(IEnumerable<(WaitReason Reason, bool Readied, bool IsMain)> users)`. -->
* **Stacks don't grow.** `main` runs on a thread that reserves 1 GiB, which covers Go's 1 GB limit for it, and
  a goroutine's thread reserves 256 MB (`GO2CS_GOROUTINE_STACK` changes it), or 16 MB when the converter proves it
  cannot recurse ([how](ConversionStrategies.md#goroutines)). Recursion past a thread's reserve ends the process
  with .NET's `Stack overflow.`, with no panic and no deferred calls, where Go prints `fatal error: stack overflow`
  and exits 2. Your main package's `init` functions run on the startup thread's smaller stack, and so does `main`
  when an `init` called `runtime.LockOSThread`, as in Go.
  <!-- C46 (COORD ruling 2026-11-11, option A): src/core/golib/runtime/MainThread.cs `public const int StackReserve =
       1024 * 1024 * 1024;` and its LockOSThread rule; go2cs-gen MainEntryGenerator writes the entry point. The init
       residual: the main package's init is a module initializer of the entry assembly, which the CLR runs before
       Main on the startup thread (1 MiB on Windows, usually 8 MiB on linux and macOS). The overflow message is its own
       seat (BOARD). Guards: Behavioral MainDeepRecursion (2,000,000 levels in main; at the base it aborted after
       174,430 frames on linux), GolibTests MainThreadTests. -->
* **`recover()` catches Go panics, not every .NET exception.** Nil dereference, divide by zero and index
  out of range recover as in Go. Any other .NET exception, `NotImplementedException` included, runs your deferred
  calls and keeps unwinding, and `recover()` returns nil. As in Go, `recover()` stops a panic only when the deferred
  function calls it directly, with two exceptions ([how](ConversionStrategies.md#defer--panic--recover)): an
  exported function that calls `recover()` stops the panic even when a deferred function calls it, and an
  unexported one that another package defers through a func value returns nil.
  <!-- docs/ConversionStrategies.md, the recover-in-helper paragraph below the catchError sample; the read:
       src/core/golib/builtin.cs:370 `public static object? recover(bool calledByDefer) => calledByDefer ? recover() : null;`. -->
* **A crash's traceback is sometimes shorter than Go's.** An unrecovered panic prints Go's panic lines, the
  whole chain included (`panic: message [recovered, repanicked]`), and exits with status 2, a goroutine's panic
  while `main` returns included. Below them, the frames match Go's for a panic in a function with one deferred
  call. When several functions on the stack each defer a call, only the function that panicked is shown, and a
  named deferred function that panics again shows the line it is declared on.
  <!-- Measured 2026-10-24 by C2 on linux-x64, .NET SDK 10.0.112, converter and corpus at f8fe15c97e (batch 36), seven
       one-file programs converted with -recurse and built with -p:GoTargetOS=linux, each compared with go run under
       Go 1.26.10. Identical frames and lines: a no-import `panic("message")`; a panic in work() with one deferred call,
       called from main (work, then main). A named deferred handler that recovers and panics again: the same three
       frames (handler, panic, main.main), with handler at its declaration line 3 where Go prints line 5, and `panic()`
       where Go prints its argument words. A deferred closure that does the same: converted main.main:8 alone; Go
       main.main.func1, panic, main.main. Three functions each deferring a call, panicking in the innermost: converted
       main.c alone; Go c, b, a, main. A goroutine panic, with main sleeping or waiting on a WaitGroup the goroutine's
       deferred Done releases: exit 2 and the report in both, converted `goroutine 1 [running]` with no `created by`
       line where Go prints `goroutine 18` and `created by main.main in goroutine 1`. The chain is rendered by
       src/core/golib/runtime/CrashReport.cs:259 `// printpanics, oldest first: a panic whose older neighbour it re-raised is not printed again.`
       and :275 `text.Append(repanicked[i] ? " [recovered, repanicked]" : " [recovered]");`. The closure frame stays
       until a post-hop converter change (COORD, 2026-10-24); the goroutine report on main's return is batch 35's. -->
* **`unsafe` works over plain numbers and stops at anything .NET's garbage collector tracks**
  ([the rules](ConversionStrategies.md#unsafepointer-and-uintptr)). A pointer to anything holding a
  string, slice, map, interface, pointer or fixed-size array gets a stand-in number, not an address, so
  the older tricks (the `*(*string)(unsafe.Pointer(&b))` pun, a hand-built slice header) don't convert
  mechanically.
* **A func variable takes a pointer word stored into it only when the word is another func value's word.**
  `*(*unsafe.Pointer)(unsafe.Pointer(&f)) = p` copies the func when `p` was read the same way from another func,
  and nil is the nil func. A hand-built `funcval` whose word is a code address panics by name: go2cs does not bind a
  function to a code address, which belongs with callbacks from C into Go, and those are not supported yet
  ([func words](ConversionStrategies-Reference/pointers.md#a-func-value-carried-as-a-number--one-word-per-static-function-through-gofunccookie)).
  This is the master branch. In go2cs 1.26.11.1 such a store is lost with no message: the func variable keeps the
  value it had, so a func that was nil stays nil and a call through it panics.
  <!-- 2026-11-23 .5 docs pass (P1), at master 97cca467a2: the store landed in train 5 (TC-13, claude/h-tc13-funcword,
       merge 3d7d5e6ec0; commit 9cec06ef4c: the earlier emission wrote p into a fresh box nothing reads, so f stayed
       nil and the next call was a nil func call, measured by Behavioral FuncWordShapes). "until cgo callbacks bind
       functions to code addresses" (UX-8) is restated as the present state: //export is refused by name (the cgo
       section below), and 9cec06ef4c places binding a PC with cgo callbacks. -->

* **A func value's word does not keep a closure alive.** Held as an `unsafe.Pointer` or a `uintptr`, the word of a
  closure names it only while something else holds the closure. Once nothing does and a collection runs, storing the
  word back into a func variable, or calling through it, panics by name ("names no live func"). In Go, the
  `unsafe.Pointer` keeps the closure alive. A top-level function's word is good for the life of the process.
* **One array view can end the process.** Go's `(*[N]E)(unsafe.Pointer(&x.f))[:]`, where `f` is an array field
  read as an array of another element type (an `[8]int8` name read as `[8]byte`), works when `x` is a variable or
  a pointer, and through any chain of named fields from one (`&x.a.f`). When the field is promoted from an embedded
  struct (`&w.f`), or belongs to a slice element (`&s[i].f`) or to a call's result (`&g().f`), the converted
  program can stop with an access violation, which `recover` does not catch. Take the field's address from a
  variable or a pointer instead: write the embedded field out (`&w.inner.f`), or bind the element or the result
  first (`p := &s[i]`, then `&p.f`)
  ([the field view](ConversionStrategies-Reference/pointers.md#the-bytes-of-one-field-through-ntunsafepointerpf-the-field-view)).
  <!-- 2026-11-15, P5, the box-kind seat. The fix (the open slice routed through the array view door) is ruled
       for .5 (COORD 13:44Z): BOARD row of 2026-11-15, "the open slice of a pointer conversion over an array field
       of another element type ends the process". This bullet is the disclosure for .4, ruled in the same line: an
       access violation that ends the process is the worst failure class. Measured on linux/amd64, Release, one
       arm per process, each arm under a recover, predictions written first
       (docs/phase4/data/array-pointer-doors/reinterpret-box-1115/probe-open/): the three roots named end the
       process with System.AccessViolationException in slice<byte>'s constructor and the recover does not run;
       the three workaround forms print Go's lines. Census: 0 sites in the standard library on four targets and 0
       in the demo closures. DESIGN-array-pointer-doors.md 19.7. -->
* **The address of another package's pointer- or interface-typed variable is the address of a copy.** An
  exported variable with a value type has its own storage, so `&otherpkg.V`, `f := otherpkg.V.Reset`,
  `flag.BoolVar(&otherpkg.Debug, ...)` and `&otherpkg.S.Field` reach the variable itself, as in Go. Two kinds are
  still handed to another package as a copy when it takes their address, so a write through it never reaches the
  variable: a variable whose type is a pointer or an interface (`&otherpkg.ErrMissing`), and a variable whose
  storage lives somewhere else (`runtime.MemProfileRate`, or one declared with a two-argument `//go:linkname`).
  Reading them, assigning to them and calling their methods work. Take the address in the declaring package, or
  have it export an accessor
  ([Known issues](KnownIssues.md#writes-through-the-address-of-another-packages-pointer-or-interface-variable-are-lost)).
  <!-- The fix of 1.26.10.2 (docs/phase4/DESIGN-crosspkg-exported-box.md, ACCEPTED; owner's option (b) of 2026-10-25,
       COORD's OQ-1 ruling value kind only). The owner boxes an exported value-kind var:
       src/go2cs/globalAddressOperations.go:365 (`func markExportedBoxOnlyGlobals(pkg *types.Package, files []*ast.File, into map[types.Object]bool) {`),
       skipping a pointer, an interface, `package main`, and a linkname forward or registered external-storage var:
       :407 (`func linknameForwardVars(pkg *types.Package, files []*ast.File) map[string]bool {`) and
       :442 (`var externalStorageVars = map[string]string{`). An importer runs the same predicate over the owner's
       syntax, :479 (`func importedGlobalIsBoxed(varObj *types.Var) bool {`), and roots a field or element address on
       the owner's box, :535 (`func (v *Visitor) rootsAtImportedBoxedGlobal(expr ast.Expr) bool {`).
       Behavioral guard: src/tests/Behavioral/CrossPackageExportedVarAddress (every shape above, against Go's output).
       Before 1.26.10.2 every non-pointer exported var bound a copy unless its own package addressed it (diagnosed by
       lane R on windows/amd64 and linux/amd64 at 9b0269c2e8, 2026-10-25). -->
* **Garbage collection is .NET's.** An object your function just stopped using may not be collectible
  until the function returns, so "drop the reference, call `runtime.GC()`, expect the finalizer" can
  wait forever. `GOGC` and `GOMEMLIMIT` are read and reported, by `debug.SetGCPercent` for example, but
  .NET's collector does not follow them.
  <!-- src/core/runtime/managed_impl.cs:1654 `internal static void gcinitController() => ᏑgcController.init(readGOGC(), readGOMEMLIMIT());`;
       nothing in src/core/runtime or src/core/golib sets a GCSettings value or a GC heap limit from them. -->
* **Runtime introspection is thinner than Go's.** A dump of all goroutines lists each one and what it waits on,
  with frames for the calling goroutine only. `pprof.StartCPUProfile` gives a profile with no samples (profile with
  .NET's tools). `debug.ReadBuildInfo()` reports the module, its dependencies, `GOOS`, `GOARCH`, `CGO_ENABLED` and
  `DefaultGODEBUG`, but no `-compiler`, `-buildmode`, `GOAMD64` or `vcs.*` settings and no replacements, and nothing
  for a program converted outside a module ([how](ConversionStrategies.md#package-conversion)). On Windows,
  `time.LoadLocation("Europe/Berlin")` fails until you import `time/tzdata` (or set `ZONEINFO`).
  <!-- Goroutine dump: docs/ConversionStrategies-Reference/manual-conversions.md, "runtime.Stack(all: true) enumerates
       every goroutine" (another goroutine prints `[stack unavailable: ...]`). CPU samples need the opt-in companion
       src/core/go2cs.CpuProfiler, which only generated test projects set and which is not packaged
       (src/core/go2cs.CpuProfiler/go2cs.CpuProfiler.csproj:31 `<IsPackable>false</IsPackable>`). Build info:
       docs/ConversionStrategies.md, "A main package's project also carries the program's build info"; the missing => lines
       are a BOARD row of 2026-10-22 (H); a test host lists a main package's dep lines and none for any other package,
       as Go's test binary does. -->
* **A few quiet ones**, each rare, where converted code gives a different answer and says nothing:
  * A variadic function that keeps, returns or captures its `...T` parameter, or `append`s or `copy`s
    into it, works on a private copy: after `f(s...)`, its writes are not the caller's `s`.
  * Copying a struct by value shares any fixed-size array inside an embedded struct.
  * A slice of one number type laid over another's storage
    (`unsafe.Slice((*uint64)(unsafe.Pointer(&b[0])), n)`) is a copy: writes through it never reach `b`.
  * A write through `unsafe.StringData`'s pointer into a string literal or a one-byte string faults in Go, because
    those bytes are read-only. In go2cs those bytes are shared by every equal string, so the write changes every
    later evaluation of that literal, in every package, or every one-byte string of that byte. Set
    `GO2CS_CHECK_STRING_WRITES=1` ([Tuning](Tuning.md#checking-writes-through-string-data)) to have it caught by
    name: `unsafe.StringData` then gives the pointer a private copy of the bytes, so the write reaches no other
    string, and the program panics at the next read through that pointer (`*p`, or a pointer `unsafe.Add` made from
    it) or at the next `unsafe.StringData` over the same string. A write through a view that reads the bytes
    directly, a slice from `unsafe.Slice(p, n)` or a `(*[N]byte)(unsafe.Pointer(p))` array pointer, also lands in
    the copy, and is reported only if a later access goes through one of those two checks. The `Data` of a
    `reflect.StringHeader` laid over such a string names the same copy. With the check on, three cases are still
    not covered, and none of them panics:
    - A literal hoisted into a `static readonly` field (`"..."u8`), which the standard library does throughout and
      a package converted with `-hoist-literals` does too. A write through `unsafe.StringData` changes that literal
      wherever it is used: after a write through the pointer to `err.(*strconv.NumError).Func` from
      `strconv.ParseBool("x")`, every later such error names the changed function.
    - A literal written in place as `"..."u8` (format strings and other string arguments in converted code). It
      becomes a new array at each evaluation, so a write through `unsafe.StringData` is private to that one value.
    - A string `unsafe.String` builds from `unsafe.StringData`'s pointer. It shares the private copy, so it reads
      any byte later written there, and `unsafe.StringData` over it hands out a pointer with no check.

    A heap string (one built at run time) behaves as in Go: the write changes it and every string sharing its bytes.
  <!-- 2026-11-24 (P5, D5). COORD ruled option A 20261124T103731Z (a private copy and a next-access panic, always on,
       cut f4318664d4 + cf7e3b212b); the owner then ruled it an OPT-IN DIAGNOSTIC, off by default (COORD
       20261124T133212Z: "no need for normal runtime work where Go already protects converted code"), after the 50,000-
       literal bench read the always-on registry at about twice master's gen-2 pause and three times its gen-0 pause.
       golib ReadOnlyStringBacking.Enabled reads GO2CS_CHECK_STRING_WRITES once at start-up; off, no literal is
       registered and StringData makes no lookup. Guarded by GolibTests ReadOnlyStringDataTests: the switch-off row pins
       this default (the write reaches the equal literal), and the switch-on rows run in a child host (red at master
       b2c9435159 on the literal, one-byte and unsafe.Slice arms; the next-access check neutered fails the three panic
       arms by name). Behavioral StringDataIdentity compares identity, heap and near-miss arms against go run with the
       switch off. The header's Data goes through the same door (ж.StringHeaderBox.cs (H) and the pointer shape),
       guarded by GolibTests StringHeaderNumberTests W4. The uncovered cases, counted by P3 at cf7e3b212b
       (COORD/20261124T130505Z-P3.md): (a) 10,799 hoisted `xˢᵇ = "..."u8` declarations in 1,284 files under src/core,
       test files included, plus any user package converted with -hoist-literals (hoistedLiteralOperations.go); P3's
       scenario is strconv/number.cs `parseBoolˢᵇ`. (b) A non-hoisted "..."u8 operand converts through implicit
       operator @string(ReadOnlySpan<byte>) into a fresh array per evaluation. (c) unsafe.String over the shadow (P3
       NIT 4). Registering hoisted field backings is dropped from .5; if ever wanted it is diagnostic-only, an OPEN
       BOARD row (2026-11-24). -->
  * A pointer's number kept in a `uintptr` and turned back into a pointer in a later statement
    (`n := uintptr(unsafe.Pointer(p))`, then `(*T)(unsafe.Pointer(n))`) can name memory that has since moved, and
    the number does not keep the value alive. Go calls this invalid too, but it often works there. Do the
    arithmetic and the conversion in one expression, or use `unsafe.Add`.
  * A pointer to a field made by arithmetic does not compare equal to the field's own address:
    `(*int64)(unsafe.Add(unsafe.Pointer(&p), 8)) == &p.y` is false where Go says true.
  <!-- docs/phase4/DESIGN-interior-pointer-root.md (P5, 2026-11-14): the first item is its section 4.2 and the
       review's SHOULD of that date; the second is shape 9 of its section 1.1, measured false at master 314e1d8b60
       and at H's 83fbf28560, disclosed by COORD's ruling 2 (section 10.9) until increment I5 lands. Read again
       on 2026-11-14 at master c2919b8b16 with the named-array floor, before this landed: for a local and for a
       heap value Go prints `true`, the conversion `false`, and both read the field's value. -->
  <!-- 2026-11-15 (P4, docs audit before 1.26.11.1): the bullet "A type switch's `case int:` also catches an
       `int32` or `rune`..." was removed: fixed in 1.26.10.2 (NEWS.md, November 2026 entry; converter
       visitTypeSwitchStmt.go, "an int32 (or rune) must NOT take the int arm"; Behavioral TypeSwitchIntWidthCases). -->
  * A left shift of an untyped constant that takes a named integer type from its context, such as
    `m & (1 << uint(32-1-i))` with `m` an `fs.FileMode`, wraps its count at the type's width: a count of the
    width or more gives the wrapped result where Go gives 0, and a negative count does not panic.
  <!-- BOARD-next-validation-candidates.md, the 2026-10-25 row "an untyped-constant LEFT shift with a NAMED result
       still masks its count" (H): the converter re-types the constant through the named type's underlying basic, so
       `(FileMode)((uint32)1 << (int)((nuint)(32 - 1 - i)))` uses C#'s native shift, which masks the count (n & 31).
       Every corpus count is below the width by construction, so no banked suite reads a wrong value; listed here on
       2026-10-26 (R's docs follow-up) until the fix lands. -->
  * In a cgo package, a pointer into Go memory rebuilt from a number can reach C unchecked in three narrow
    cases ([cgo](#cgo)).

## cgo

**What converts.** Without `-cgo`, a package whose build selects a file with `import "C"` stops the run with
`error GO2CS2001`, which names the file. With `-cgo`, on a machine with a C compiler (`go env CC`), a package
converts when every C use it makes is a call of a C function a shared library defines, passing integers,
floating-point numbers and pointers, structs of integers and pointers by value, one struct or one scalar by
pointer, or a `void *` that is nil or points at C memory. `C.CString`, `C.CBytes`, `C.GoString`, `C.GoStringN`,
`C.GoBytes`, `C.malloc`, `C.free` and `defer C.f(…)` convert, with the package's C types and constants. A
program that uses the converted package needs only the C library.

On the master branch, Go code that walks C memory converts too: it reads and writes a pointer field of a C struct
that lives in C memory (`l.next`), and loads and stores through a `**C.T` (`*p`, as a walk down a C list such as
`addrinfo`'s `ai_next` does). A pointer field reached through a nested struct value, an array of pointers, a field
whose type is a C pointer typedef, and a union are still refused by name, and so is storing a pointer into Go memory
into C memory. In go2cs 1.26.11.1, reading a pointer field of a C struct in C memory fails with a panic.
<!-- 2026-11-23 .5 docs pass (P1), at master 97cca467a2: cgo increment 3.2 seat 1a (train 4, claude/h-cgo-sb2a, merge
     1f2a9afdfc; docs/phase4/DESIGN-cgo-phase3-marshaling.md section 18.9: [GoCgoStruct], the GET/SET accessor at the
     field's Go offset, and the four shapes left out by name) and seat 1c (train 6, claude/h-cgo-1c, merge 5f2f2d0aa3;
     section 18.11: the **C.T load and store over C memory). Arms: s6nextnil, s6nextdirect and s6name back to plain
     rows (commit 01ecb22bb9), s6next by 1c, s6store as Go and s6storego refused by name. Before them the hosted
     os-matrix cgo legs (run 44883224786) read golib's "cannot read native memory as ...: its CLR layout is not its Go
     layout" refusal for all four rows on five targets (commit e6210fa543), and section 18.1 read s6next as a nil
     dereference panic on 2026-11-15; which of the two 1.26.11.1 prints is not measured, so the text says "a panic".
     1.26.11.1 has neither seat. Go memory
     handed to C without a copy (seat 1b, the pinned class) has not landed, so the refusals below stand. -->

**One target.** A `-cgo` conversion is for one operating system and architecture: the machine's own, or another one
named with `-platforms` when `CC` names that target's C compiler (for example `zig cc -target <triple>`). The converted
package refuses its C bindings on any other: when the package loads, before any of its code runs. A single-package
conversion builds against a go2cs clone (`dotnet build -p:go2csPath=<go2cs>/src/`). Under `-recurse -cgo` a module's
cgo packages convert with the module, and under `-recurse=nuget -cgo` they reference the `go.*` packages. A module
with a cgo package packs for publishing as a package for each of `win-x64`, `linux-x64`, `osx-x64` and
`linux-arm64`; `osx-arm64` is left out until the `go.*` packages ship a true `osx-arm64` build. Such a package is
published only against a `go.lib` that carries the load-time target check, which the go.* 1.26.11.1 packages do not.
The standard library itself is converted with `CGO_ENABLED=0`, and a module converted with `-recurse -cgo` uses it as
it is. So a package that imports `net` or `os/user` gets their builds without cgo, the ones Go itself uses with
`CGO_ENABLED=0`: `net` resolves host names with Go's own resolver, not the C library's, and `os/user` reads the user
database files directly. go2cs does not convert the `net` resolver with cgo yet ([Roadmap](Roadmap.md#cgo)). A
package that imports `runtime/cgo` or `plugin` is refused by name: without cgo they are not working builds.

That is the master branch. go2cs 1.26.11.1 converts one cgo package at a time, for the machine's own operating system
and architecture only: there `-cgo` does not combine with `-recurse`, a cgo package cannot be packed, and the target
check runs at the package's first C call rather than when it loads.
<!-- 2026-11-23 .5 docs pass (P1), at master 97cca467a2. "Packing a cgo package against the go.* packages for every
     platform is still to come" and "Converting the net resolver with cgo is planned, and is what will change this"
     (UX-8) are restated in the present tense. Sources:
     - -recurse -cgo and -recurse=nuget -cgo: train 3, claude/c2-cgo-nuget-pack INC 1 (merge 8de0f7ece2; commits
       4531a7a37e and 32c6b517e5); route refusals skip their package: claude/c2-cgo-route-skip (merge f74b4d7385).
     - Another target with CC naming its compiler: INC 2a (commit 07378119f9; R3 steps aside without a CC, 78e369a4a9,
       merge 640d45c9eb).
     - The per-RID pack: claude/c2-cgo-pack-2b (merge ef9be9bd32): win-x64, linux-x64, osx-x64 and linux-arm64, osx-arm64
       left out by name (docs/NugetgoPublish.md, "Cgo"; an internal runbook, so not linked).
     - The load-time target check (A6): train 4, claude/c2-cgo-a6 (merge c9143ae615, commit 77e91b3c0c):
       GoCgoLibraries.Register refuses in the cgo package's module initializer; before it, Resolve refused at the
       first C call. nugetgo-pack -Cgo refuses a restored go.lib without it ("the published 1.26.11.1 is refused by
       name"), and src/_nugetpush.ps1 pushes a -Cgo pack only with the "a6:" record. Master's golib carries it, so the
       next go.lib release carries it; no version is named here because none is set.
     - The local-closure rehearsal and the module-cache release with no go.mod: train 5, claude/c2-cgo-gopointer
       (merge 54b02b555d), not user-facing beyond the above.
     - At nuget-1.26.11.1, main.go's -cgo help read "Not combinable with -stdlib, -tests or -recurse" and converted for
       the host only (this section's text at the tag: "One package, one machine").
     - The net resolver: no merge in trains 1 to 7 converts it. -->

<!-- src/go2cs/main.go:307 (`Not combinable with -stdlib or -tests. With -recurse, every non-standard-library package of the module loads cgo-on and each cgo package converts this way`); src/go2cs/csproj-template.xml:135 (`<ProjectReference Include="$(go2csPath)gen/go2cs-gen/go2cs-gen.csproj"`) and src/go2cs/csproj-template.xml:187 (`<ProjectReference Include="$(go2csPath)core/golib/golib.csproj" />`). COORD's ruling O2, 2026-11-14 (cgo 3.1's release-path seat). -->

**What is refused, by name.** Each of these is not supported yet; [the Roadmap](Roadmap.md#cgo) says what is planned.
- a pointer into Go memory handed to C as a `void *`, or to `C.GoBytes`, `C.GoString` or `C.GoStringN`.
  A pointer into a slice or an array is refused when the package is converted;
  `unsafe.Pointer(&x)` is refused when the call runs, with a message that names the call. Inside a method
  with a pointer receiver, `unsafe.Pointer(p)` of a pointer of the receiver's own type is refused when the
  package is converted: move the call into a function that takes the pointer as a parameter;
- a function the preamble itself defines, and a `#define` that is not a constant, both of which need
  compiled C;
- `//export`;
- a C variable, a C function used as a value, and `go C.f(…)` with a pointer cmd/cgo checks;
- a C function Go's own cgo build does not import by its C name: one a header renames (macOS on x64 makes
  `fstat` the symbol `fstat$INODE64`), or one the C compiler inlines. go2cs cannot tell the two apart, so it
  refuses both rather than bind a function C would not call:
  `error GO2CS2001: C.abs: cmd/cgo's own binary does not import a symbol named abs, ...`. Which
  functions those are depends on the compiler and the C library: `abs` and `labs` everywhere, and with glibc
  also `atoi`, `atol`, `toupper`, `tolower` and `fabs`.
  <!-- abs and labs refused on Linux, macOS and Windows; the glibc set measured on Linux x64 with gcc 15 and with
       clang 22. [plan](PLAN-cgo-interop.md) (internal record, unlinked 2026-10-26, OQ-1);
       docs/phase4/DESIGN-cgo-phase2-pinvoke.md §11.20 -->

**Where it differs from Go.** go2cs never hands C a pointer into Go's memory: it hands C a copy of the one
struct a pointer points at, and copies it back after the call. So cgo's pointer check differs from Go's in
five cases, all where a program breaks or bends cgo's pointer rules. In two, Go panics and go2cs proceeds:
- a pointer held in a variable, when the object it points into also holds a Go pointer;
- a pointer held in a variable, when the object it points into lives in package-level memory.

Go checks a pointer held in a variable against the whole object it points into. go2cs proceeds in both cases, since C
only sees the copy. This variable check is the one most cgo calls in the Go toolchain's own cgo packages take.

In three, Go proceeds and go2cs refuses the call when it is made:
- a struct member that points at memory pinned with `runtime.Pinner`;
- any pointer under `GODEBUG=cgocheck=0`;
- a struct passed by value whose pointer member points into Go memory that holds no pointer.
<!-- 2026-11-23 .5 docs pass (P1), UX-13, at master 97cca467a2: the passage said "five places" over two bullets of
     2 and 3 cases, so the count was right and the list hid it. The five are DESIGN-cgo-phase2-pinvoke.md §11.16's
     amendment of 2026-11-11 (fixture functions PackageLevel, Sibling, Pinned and ByValue, plus cgocheck=0, an
     environment setting with no fixture). Re-read at 97cca467a2: src/go2cs/testdata/cgo/phase2 is unchanged since
     nuget-1.26.11.1; golib's GoCgoArgs.PointerMember still refuses every managed pointer member with cmd/cgo's
     "argument of cgo function has Go pointer to unpinned Go pointer" (covering Pinned and ByValue), and no switch
     reads cgocheck. Go memory handed to C without a copy (cgo 3.2 seat 1b) has not landed in trains 1 to 7, so
     none of the five changed, and the count is the same in 1.26.11.1. -->

<!-- docs/phase4/DESIGN-cgo-phase2-pinvoke.md §11.16 and §11.20 (COORD's M1 ruling, 2026-11-11); the census is
     TestCgoCheckClosureFormsAcrossGorootsCgoPackages, hosted run 42973745743: address 31, variable 50, index 22,
     slice 0, defer 9, go 0 over GOROOT's 29 cgo packages (50 of the 81 checked calls that pass no element of a
     slice or an array take the variable check). -->

**A known memory-safety gap: a pointer rebuilt from a number, in three narrow cases.** go2cs refuses, by
name, any address inside Go memory that it is holding still, even after the address has been through a
`uintptr`: `u := uintptr(unsafe.Pointer(p))`, then `unsafe.Pointer(u + n)`, the reversed sum
`unsafe.Pointer(n + uintptr(unsafe.Pointer(p)))`, and a slice element viewed as another type
(`(*C.int)(unsafe.Pointer(&b[4]))`) and then held as a `uintptr`. Go's own one-expression form,
`unsafe.Pointer(uintptr(unsafe.Pointer(p)) + n)`, converts to `unsafe.Add`, which go2cs refuses by name, as
it does `unsafe.Pointer(p)` itself. Three cases still cross, because go2cs is no longer holding the memory,
or never was. In each, the call goes ahead, and the memory may move while C uses it:
- a number kept after nothing uses `p` any more. go2cs stops holding the memory still, and the number reads
  as C memory. Go's own rules already forbid keeping a pointer as a `uintptr` like this;
- a number taken inside a method with a pointer receiver, from a pointer of the receiver's own type. go2cs
  makes that number without holding the memory still. Passing such a pointer to C directly is refused when
  the package is converted; taking it through a `uintptr` first is not caught;
- Go memory held still before the program's first cgo package was loaded, which can be missing from
  go2cs's record of what it holds.
<!-- docs/phase4/DESIGN-cgo-phase3-marshaling.md §16.9 (stop line S3.1-E, COORD's ruling C, 2026-11-12); the arm is
     GolibTests' APointerOverManagedStorageCrossesAVoidPointerOnlyAsTheFourDisclosedNumbers. 2026-11-14 (H, the
     release-notes audit): the one-expression form is emitted as unsafe.Add on master (src/go2cs/pointerArithmetic.go;
     TestPointerArithmeticFormEmitsUnsafeAdd, whose controls "held" and "reversed" stay numbers), and unsafe.Add's
     result keeps its box, so the door refuses it (the goadd arms, hosted cgo-e2e on linux-x64, win-x64, osx-arm64
     and osx-x64). The interior arm is GolibTests' ANumberBuiltByArithmeticOverAGoPointerCrossesAsDisclosed. -->
<!-- 2026-11-15 (H): narrowed by the pin-extent seat to the three cases docs/phase4/DESIGN-cgo-pin-extents.md
     section 4 leaves open: rows 7 (a released pin), 8 (a number minted with no pin) and 11 (an early pin; its routes
     are in section 11.2). The two arms named above were rewritten by that seat to assert the refusal:
     APointerOverManagedStorageNeverCrossesAVoidPointer and
     ANumberBuiltByArithmeticOverAGoPointerIsRefusedForEveryKindOfPin. Row 7 is AReleasedPinLeavesNoStaleRefusal;
     row 11 is asserted as disclosed in PinExtentsChildTests; row 8 is the receiver-type FromRef arm of
     DESIGN-cgo-phase3-marshaling.md section 16.9, whose direct form the cgo route refuses at conversion. Moved into
     the cgo section at the batch-308 merge, where P4 gave cgo a section of its own. -->

**Platforms.** A converted cgo program was built and run against the Go build of the same program on
linux/amd64, darwin/amd64, darwin/arm64 and windows/amd64, and printed what Go printed apart from the
differences above.
<!-- cgo Phase 2's G1 gate (docs/phase4/DESIGN-cgo-phase2-pinvoke.md section 11.22: hosted legs on linux/amd64,
     darwin/amd64, darwin/arm64 and windows/amd64) and Phase 3 increment 3.1 (43ac9e33fa, batch 305;
     DESIGN-cgo-phase3-marshaling.md section 16; hosted cgo-e2e legs 43638330006 linux-x64, 43638338397 win-x64,
     43638346986 osx-arm64 and osx-x64). -->

## What works, but costs you something

* **Speed.** In the [performance comparison](Performance.md), measured on `win-x64`, most compute lands
  within a small multiple of Go's time, string-heavy work takes several times longer, maps run at parity or
  better, and asking at run time whether a value satisfies an interface (`v.(fmt.Stringer)`) is the slowest
  thing converted code does. [Tuning converted code](Tuning.md) says which deployment and runtime settings to
  choose. Measure your own hot path, in Release.
* **Startup, memory and shipping.** A converted program loads .NET and one DLL per converted Go package it uses,
  and the JIT compiles each method the first time it runs; package-level tables are built when first read. You ship
  a .NET app, not Go's small static binary: `dotnet build` output needs the .NET 10 runtime, and
  `dotnet publish -r <runtime>` gives a self-contained folder compiled ahead of time with ReadyToRun, which starts
  noticeably faster (on linux-x64). `dotnet run` and `dotnet build` never get ReadyToRun
  ([how](ConsumingGoFromCSharp.md#rough-edges)).
  <!-- ReadyToRun: src/go2cs/csproj-template.xml:58 `<PublishReadyToRun>true</PublishReadyToRun>`; the startup figure is
       P1's PerfStartup reading cited in docs/ConsumingGoFromCSharp.md (linux-x64, median 488 ms JIT, 329 ms ReadyToRun,
       about a third less). Lazy tables: docs/ConversionStrategies.md, the [GoLazy] holder paragraphs (a package var
       whose initializer depends on nothing that changes is built on its first read). -->
* **Trimming.** A converted project carries the `partial` trim mode, and a C# project you write gets the same
  default from the packages. A trimmed publish runs, and its output matches the untrimmed build's. It warns
  (on linux-x64): one IL2104 for each `go.*` package with trim warnings, a handful for a program that uses `fmt`, and
  `-p:TrimmerSingleWarn=false` lists the warnings behind them, most of them golib's, `runtime`'s and `reflect`'s use
  of reflection. A project built from the converted source lists them all. A Native AOT publish adds two more codes: one IL3053
  for each package with AOT analysis warnings (three: `runtime`, `internal.abi` and `reflect`), and one IL3000, where
  `runtime` reads `Assembly.Location` to open a symbol file; the path is empty in a native image, and `runtime` falls
  back to reading the file another way.
  <!-- IL3053 and IL3000: P3's cold first-user pass on the published 1.26.11.1 packages (linux-x64, .NET SDK 10.0.404,
       mailbox 2026-11-19 23:12Z): the colordemo's `dotnet publish -r linux-x64 -p:PublishAot=true` warned 12 times,
       8 IL2104 (the eight packages named below), 3 IL3053 (runtime, internal.abi, reflect) and 1 IL3000
       (runtime/managed_impl.cs, Assembly.Location in openPortablePdb). The codes were named only in
       PLAN-golib-full-trim.md before. First post-.4 docs batch, COORD 2026-11-20 07:19Z. -->
  <!-- Trim warnings: P1's reconciliation, linux-x64, go1.26.10, .NET SDK 10.0.112, at a8821c781a
       (claude/p1-trim-recon ddf7c054d7, docs/phase4/DATA-trim-recon-p1-2026-10-24.md §2-§4). A fmt program converted
       with -recurse=nuget against a local feed of the closure's packages: rc 0 and 8 IL2104, one each for errors,
       internal.reflectlite, internal.sync, golib, reflect, runtime, time and unsafe; with -p:TrimmerSingleWarn=false,
       110 (golib 41, runtime 40, reflect 14, time 6, unsafe 5, three others). The same program built with project
       references logs the 110 directly, which is C2's reading of the same day. In every arm the trimmed stderr equals
       the untrimmed build's. -->
* **Single-file and Native AOT.** A single-file publish needs ReadyToRun compiled per assembly
  ([Known issues](KnownIssues.md#a-single-file-publish-with-composite-readytorun-runs-out-of-memory)). Native AOT
  starts much faster and holds less memory, but once running it is slower than the JIT on most rows of the
  comparison (`win-x64`), and a publish compiles every converted package your program references in full
  ([Known issues](KnownIssues.md#a-native-aot-publish-of-a-program-that-references-the-packages-is-slow-and-large)
  has the measurements).
* **Allocation.** Converted code allocates where Go's compiler doesn't, so expect more garbage and
  failing `testing.AllocsPerRun` assertions.
* **Crypto, most hashing and `math/big` run Go's portable code**, the way Go builds with `-tags purego`:
  correct, validated, and slower than Go's assembly. A TLS handshake is where you will notice.

## Third-party modules: where they trip today

The [validated modules](ValidatedModules.md) page lists the modules validated so far.

* **A dependency's assembly converts to methods that throw when called, unless a tag deselects it.**
  A plain `-recurse` (your module plus its dependencies) applies `safe`, `purego` and `noasm` for
  you, so a fast path behind `!purego` or `!noasm` is replaced by the library's pure-Go fallback
  ([Build tags for a module](ConvertingAModule.md#build-tags-for-a-module)). A file gated on the
  architecture alone has no such fallback, and the conversion names each stub it leaves
  (`warning GO2CS2012`). Where the same package already holds the answer in Go under another name, go2cs
  forwards the stub to it: zeebo/xxh3's `accumNEON` and `accumBlockNEON`, which its arm64 build calls for
  every input over 240 bytes, run its own `accumScalar` and `accumBlockScalar`. Plain xxh3 still stops
  earlier, on every architecture, at its `unsafe` string views ([below](#unsafe-header-views)); use the
  repository's copy of it named there. An explicit `-tags` replaces every default, so include
  `safe,purego,noasm` with yours, and under `-tests` include `math_big_pure_go` too.
  <!-- The sibling rows: src/go2cs/asmForwardBodies.go, guarded by TestAsmSiblingForwardEmission and
       TestAsmSiblingForwardIsRefusedBySite. The earlier text here said hashing works because the converted
       klauspost/cpuid never reports AVX2/SSE2 under noasm; that held for the amd64 vector stubs and missed both
       the header views (every architecture) and arm64, where accum_stubs_arm64.go sets hasNEON = true as a
       constant. -->
* **A test suite that imports testify depends on the `safe` tag.** Its bundled go-spew does pointer
  arithmetic into `reflect.Value` at load, which the runtime library refuses by design, and go-spew's
  `safe` tag selects its fallback. Drop the tag, with `-module-safe-tag=false` or an explicit `-tags` that
  leaves it out, and the converted test program dies at startup. With the tag on, testify works: on the master
  branch, github.com/stretchr/objx's suite, which uses testify, validates
  ([validated modules](ValidatedModules.md)).
  <!-- 2026-11-23 .5 docs pass (P1), at master 97cca467a2: objx v0.5.2 and v0.5.3 rows in docs/ValidatedModules.md,
       landed in train 6 (merge bb6267d0e4); docs/ValidatedModules.md at nuget-1.26.11.1 has no objx row. -->

* **A hung test is stopped, where `go test` waits.** A converted test still running 15 minutes after it
  started is reported as a failure and marked abandoned, and the run moves on to the next test
  (`-test-deadline` changes the limit, [Command line](CommandLine.md)). Its goroutines keep running, so
  every later result says it ran after the abandoned test.
  <!-- docs/CommandLine.md, the -test-deadline row (default 15m; 0 turns it off). A converted test's argv has the
       shape go test gives a test binary:
       src/core/testing/TestOptions.cs:245-247 `keeps argv exactly the shape…gives a…test binary`. pflag's suite
       reads identical to its published proof at 9dcf0bafe1 (docs/phase4/DATA-p2-nugetgo-trial-2026-10-24.md §3). -->
* **Converter bugs that surface as C# compile errors**, usually in corners of the type system. In your
  own code, reshape the Go. In a dependency, point a `replace` at a patched copy, or fix the generated
  `.cs` and mark the file `[module: go.GoManualConversion]` so the next conversion leaves it alone. New
  ones still turn up: [tell me](#when-you-hit-a-seam).
* <a id="unsafe-header-views"></a>**`unsafe` views of a string's or a slice's bytes are not supported yet.** Go code can read a string or a
  `[]byte` through its header, `*(*struct{ p unsafe.Pointer; n uint })(unsafe.Pointer(&s))`, and then read bytes at
  `unsafe.Pointer(uintptr(p) + i)` or through `(*[8]byte)(p)`. Converted, these stop with a named runtime panic,
  because the string or slice lives in .NET-managed memory with no fixed address to offset. github.com/zeebo/xxh3
  is the common case; the repository carries an `unsafe`-free copy of it that computes the same hashes, for a
  `replace github.com/zeebo/xxh3 => <go2cs clone>/src/forks/github.com/zeebo/xxh3@v1.1.0`. go2cs has no general
  fix for these views yet.
  <!-- 2026-11-23 .5 docs pass (P1), UX-8: was "The general fix is planned." No header-view change landed in trains 1
       to 7 (no matching merge in `git log --first-parent nuget-1.26.11.1..97cca467a2`), so the bullet states the
       present state. -->

  <!-- C45 (COORD rulings 2026-11-11): option D for 1.26.10.4, the fork src/forks/github.com/zeebo/xxh3@v1.1.0 with
       its FORK.md and identity/check.sh (16,632 results byte-identical to the original under real Go); option A, the
       general fix, is a post-1.26.10.4 train, docs/phase4/DESIGN-unsafe-header-views.md on branch
       claude/c1-c45-header-views. The panics: "cannot read native memory as <header type>: its CLR layout is not its
       Go layout", "unsafe pointer arithmetic on a managed pointer with no address", and "cannot view native memory as
       array<Byte>: the address has no managed element storage behind it" (Behavioral UnsafeHeaderViewFamily, parked
       on that branch). -->
* **One bad dependency blocks everything above it.** A package that fails to convert, or that the
  converter refuses, is reported and skipped: the rest of the module still converts, and every project
  that imports it fails to build. The run ends with exit 1, and its summary names each such package
  and, for a refused one, each converted package that imports it. `-recurse=module`
  [converts only your packages](ConvertingAModule.md#optional-convert-the-module-only-and-deal-with-its-dependencies-later).

## What "validated" means, and what it doesn't

A package's own Go `Test` functions were converted, run, and compared with `go test` verdict for
verdict, by full test name. That's a real bar, and narrower than the word sounds:

* **Tests only.** Examples, benchmarks and fuzz targets never run. No race detector either.
  <!-- src/go2cs/testConversion.go:4262 `entry.Kind, entry.Status, entry.Reason = "fuzz", "unsupported", "fuzz execution is deferred to Phase 4D"`;
       Example- and Benchmark-only test files are left out of the compile set, :859
       `// Phase-4D file exclusion (option-a ruling): drop Example/Benchmark-only test files from the`. -->
* **Matched is not passed.** A test that skips on both sides counts as a match and measures nothing.
* **Disclosed differences are real differences.** Where a Go test asserts something a managed runtime
  can't satisfy, or doesn't yet, its exact failure is pinned and listed on the package's proof page,
  linked from [Validated Test Packages](ValidatedTestPackages.md)
  ([Disclosures](ValidatedTestPackages.md#disclosures) explains the kinds). Read the list for a package
  you lean on.
* **Nothing about your dependencies.** `go2cs -tests -recurse` tests the packages of the module you
  point it at, never its dependencies, so run it on each dependency you lean on.

## Platforms and toolchain

* **Go's security fixes arrive with go2cs releases.** The converted standard library is a conversion of
  the Go release in [Requirements](README.md#requirements). A fix Go issues later reaches you when go2cs
  converts and publishes that patch. A project converted with `-recurse=nuget` takes it on its next
  restore, because its `GoStdLibVersion` is the Go minor with a float, such as `1.26.*`
  ([Roadmap](Roadmap.md#go-releases)).
  <!-- The float: src/go2cs/toolchainResolution.go:525 `return strings.TrimPrefix(minor, "go") + ".*"`; the release
       lines: docs/PLAN-version-lines.md (master carries the current Go line with patch hops). -->
* **One Go release, one .NET release.** Convert with the Go release in
  [Requirements](README.md#requirements), which the go2cs dotnet tool downloads for you; building the
  converter from a clone needs Go 1.26.11 or later (the release `src/go2cs/go.mod` names). `go2cs -version`
  should show that one release throughout. Rebuild after
  every pull. Each [GitHub release](https://github.com/ritchiecarroll/go2cs/releases) carries the converter
  prebuilt for six platforms, with Authenticode-signed Windows executables and a GPG-signed `SHA256SUMS`
  (`SHA256SUMS.asc`, by the key that signs the release tag). Converted projects and the `go.*` packages target
  `net10.0`.
  <!-- .github/workflows/release-binaries.yml: a nuget-N.N.N.N tag builds the converter for six platforms and drafts a
       GitHub Release with the archives, SHA256SUMS and the attestation bundle. src/sign-release-binaries.ps1 (the
       release machine) signs the Windows executables and SHA256SUMS on the draft; .github/workflows/attest-release.yml
       attests the archives as the draft then holds them. Signing began with the 1.26.10.2 release. -->
* **Where it runs.** The standard library is validated on x64 Windows, on Linux, x64 and Arm64 (glibc, on
  Microsoft's build of .NET: Ubuntu's own package has an entry in [Known issues](KnownIssues.md)), and on macOS,
  Intel and Apple silicon. The [roster](ValidatedTestPackages.md) shows each package's counts on each platform; two
  Windows-only packages have no Linux or macOS reading. The packages ship `win-x64`, `linux-x64`, `linux-arm64`, `osx-x64` and `osx-arm64`; a build for any other platform
  is refused or warned about
  ([Known issues](KnownIssues.md#platforms-the-packages-do-not-ship)).
  <!-- linux-arm64: the roster's Linux line (236 of 236 banked at linux-arm64, S7 waves, DATA-la64-s7-bank-plan.md). -->
* **One conversion, one operating system.** Go picks build-tagged files at conversion time, so convert
  once per OS (`-platforms linux/amd64`), into separate output folders.

## Reading and debugging the output

* **You debug the C#.** Breakpoints, stepping and compiler errors are on the generated `.cs`. A Go
  panic's own traceback does name the Go file and line, read from the program's `.pdb` files: a publish
  with `-p:DebugType=none` keeps the frames and loses every file and line.
  <!-- P1's reconciliation, linux-x64 (claude/p1-trim-recon ddf7c054d7, docs/phase4/DATA-trim-recon-p1-2026-10-24.md §5
       finding 2): "With -p:DebugType=none the traceback keeps its frames but loses every file:line line." -->
* **Some of the letters aren't on your keyboard.** `ж<T>`, `Ꮡx`, `goǃ(...)`, `ᐸꟷ(ch)`: several look like
  punctuation and are letters, so an ASCII search finds nothing. Keep the
  [glyph table](ConversionStrategies.md#reading-converted-code-names-and-glyphs) open.
* **Don't hand-edit converted files, the `.csproj` included.** The next conversion rewrites them, so pass
  build settings with `-p:`. For code, fix the Go, keep your own C# in a separate file, or mark a whole
  file `[module: go.GoManualConversion]` ([how](ConversionStrategies.md#manually-converted-declarations))
  and merge later Go changes yourself. A renamed Go file leaves its old `.cs` behind, still compiled.
* **Start with `go2cs -recurse=nuget <module> <out>`**, the one form whose output restores and builds
  with nothing staged. Go comments are dropped unless you pass `-comments`.
* **Calling converted Go from your own C#?** See [Rough edges](ConsumingGoFromCSharp.md#rough-edges).
  The road runs one way: Go source can't call a .NET API.

## What you might expect to break, and doesn't

The language itself (generics, closures, struct embedding, range-over-func, typed nil pointers in
interfaces, randomized map order), `defer`/`panic`/`recover`, channels and `select`, `reflect`
(`encoding/json` and `text/template` pass their own suites on it, on Windows, Linux and macOS), and `net/http` with HTTP/2 and
`crypto/tls` over real sockets.

## When you hit a seam

1. **Start from a clean Go build**: `GOTOOLCHAIN=local`, then `go build ./...` for the same OS and tags.
2. **Read the conversion's warnings and its summary line**, not the exit code.
3. **A clean build that throws `NotImplementedException`** wants a pure-Go build tag. **A C# compile
   error in converted code** is a converter bug until proven otherwise.
4. **A behavior difference in the standard library?** Check the package's disclosures on
   [Validated Test Packages](ValidatedTestPackages.md).
5. **Check [Known issues](KnownIssues.md)**, then
   [open an issue](https://github.com/ritchiecarroll/go2cs/issues) with the output of `go2cs -version`,
   your OS, the error text, and the smallest Go that reproduces it.

If you find a seam that isn't on this page, that's the one I most want to hear about.
