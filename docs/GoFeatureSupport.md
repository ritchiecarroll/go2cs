# Go feature support

Most of Go converts and behaves as it does in Go: slices, maps, pointers, closures, `defer`, interfaces,
generics, goroutines, channels and `select`, reflection, and the whole standard library, which is
validated against Go's own tests. The tables below go feature by feature. Each note links to how the
feature maps to C#, to a working example from the behavioral test suite, or, for a feature with limits,
to the page that describes them.

| Symbol | Meaning |
|---|---|
| ✅ | Supported: converts and behaves as in Go. |
| ⚠️ | Supported with limits: the note says which. |
| ❌ | Not supported yet. |

An *example* link opens a behavioral test: a Go program, its converted C#, and the output both must
print. [Limitations](Limitations.md) and [Known issues](KnownIssues.md) have the detail behind every
⚠️ and ❌.

<!-- Assumed release state: the cgo, linux/arm64 and windows/arm64 rows describe the state planned for the
     next release and get a final accuracy pass before it ships. Grounding: cgo, docs/PLAN-cgo-interop.md and the
     converter's cgo*.go files; linux/arm64, docs/phase4/DESIGN-linux-arm64-flavour.md; windows/arm64,
     docs/KnownIssues.md (GO2CS3004). Internal records stay unlinked from published pages. -->

<div class="feature-matrix" markdown="1">

## Types and values

| Feature | Status | Notes |
|---|---|---|
| Booleans, integers, floating point | ✅ | `int` and `uint` are platform-width; integer overflow wraps and division by zero panics, as in Go. [How](ConversionStrategies.md#integer-types-and-arithmetic) · [example](../src/tests/Behavioral/DivideByZeroPanic/) |
| Complex numbers | ✅ | `complex64`, `complex128`, `real`, `imag`; `math/cmplx` passes Go's tests. [Example](../src/tests/Behavioral/ComplexFormat/) |
| Strings and runes | ✅ | UTF-8 bytes, as in Go: indexing reads bytes, `range` decodes runes. [How](ConversionStrategies.md#strings-string-and-sstring) · [example](../src/tests/Behavioral/StringByteSemantics/) |
| Constants, untyped constants, `iota` | ✅ | Exact constant arithmetic, and Go's literal spelling kept in the output. [How](ConversionStrategies.md#constant-values) · [example](../src/tests/Behavioral/IotaEnum/) |
| Arrays | ✅ | Value semantics: assigning or passing an array copies it. [How](ConversionStrategies.md#slices-and-arrays) · [example](../src/tests/Behavioral/ArrayPassByValue/) |
| Slices | ✅ | Shared backing arrays, `append` growth, three-index slices, nil versus empty. [How](ConversionStrategies.md#slices-and-arrays) · [example](../src/tests/Behavioral/SliceAliasing/) |
| Maps | ✅ | Nil-map reads, comma-ok, `delete`, `clear`, randomized iteration order. [How](ConversionStrategies.md#maps) · [example](../src/tests/Behavioral/NilMapOperations/) |
| Structs | ✅ | Value types, `==` comparison, anonymous structs, and tags that `reflect` reads. [How](ConversionStrategies.md#struct-types) · [example](../src/tests/Behavioral/AnonymousStructs/) |
| Pointers | ✅ | `&x`, `new`, pointers to fields, elements and pointers; a local whose address escapes moves to the heap. [How](ConversionStrategies.md#pointers) · [example](../src/tests/Behavioral/PointerToPointer/) |
| `nil` and zero values | ✅ | Every type's zero value, and a typed nil pointer inside an interface. [How](ConversionStrategies.md#nil-and-zero-values) · [example](../src/tests/Behavioral/TypedNilInterface/) |
| Defined (named) types | ✅ | Conversions and constant contexts as in Go. [How](ConversionStrategies.md#named-numeric-types-and-constant-contexts) · [example](../src/tests/Behavioral/NamedNumericConversion/) |
| Type aliases | ✅ | Including exported aliases used from another package. [How](ConversionStrategies.md#type-aliasing) |
| Shifts | ⚠️ | Go's rules, including the panic on a negative count. One rare form, an untyped-constant left shift typed by a named integer, wraps an oversized count ([Limitations](Limitations.md#what-converts-but-doesnt-behave-exactly-like-go)). [Example](../src/tests/Behavioral/GoShiftSemantics/) |
| The address of another package's variable | ⚠️ | Real for a value-typed variable. For one whose type is a pointer or an interface it points at a copy ([Known issues](KnownIssues.md#writes-through-the-address-of-another-packages-pointer-or-interface-variable-are-lost)). |

## Functions and control flow

| Feature | Status | Notes |
|---|---|---|
| Multiple results and comma-ok forms | ✅ | Results become C# tuples, evaluated in Go's order. [How](ConversionStrategies.md#multi-result-values-and-comma-ok-forms) · [example](../src/tests/Behavioral/MultiValueReturnOrder/) |
| Named results | ✅ | Including results that a deferred closure reads and changes. [Example](../src/tests/Behavioral/NamedReturnDefer/) |
| Function values and closures | ✅ | Closures share captured variables, as in Go. [How](ConversionStrategies.md#function-values-and-closures) · [example](../src/tests/Behavioral/VariableCapture/) |
| Method values and method expressions | ✅ | `f := v.M` and `T.M`. [Example](../src/tests/Behavioral/MethodSelector/) · [example](../src/tests/Behavioral/MethodExpression/) |
| `defer` | ✅ | Last in, first out, with arguments evaluated at the `defer` statement. [How](ConversionStrategies.md#defer--panic--recover) · [example](../src/tests/Behavioral/DeferCallOrder/) |
| `init` functions and package initialization order | ✅ | Go's dependency order, across files. [How](ConversionStrategies.md#package-level-variable-initialization-order) · [example](../src/tests/Behavioral/MultiFileInitOrder/) |
| `if`, `for`, `switch`, `fallthrough` | ✅ | [How](ConversionStrategies.md#expression-switch-statements) · [example](../src/tests/Behavioral/SwitchFallthroughDefault/) |
| `for` loop variables per iteration | ✅ | Each iteration gets its own variable, as in Go. [Example](../src/tests/Behavioral/ForLoopPerIterationVars/) |
| `range` over slices, arrays, strings, maps, channels and integers | ✅ | [How](ConversionStrategies.md#loops-range-and-labels) · [example](../src/tests/Behavioral/RangeStatements/) · [integers](../src/tests/Behavioral/RangeOverIntegerTypes/) |
| `range` over functions, and `iter` | ✅ | Range-over-func loops and `iter.Pull`; package `iter` passes Go's tests. [Example](../src/tests/Behavioral/IterPullRendezvous/) |
| Labels, labeled `break`/`continue`, `goto` | ✅ | [How](ConversionStrategies-Reference/labels-and-loop-variables.md) · [example](../src/tests/Behavioral/LabeledEmptyStmt/) |
| Built-in functions | ✅ | `append`, `copy`, `len`, `cap`, `make`, `new`, `delete`, `close`, `clear`, `min`, `max` and the rest. [How](ConversionStrategies.md#built-in-functions) · [example](../src/tests/Behavioral/MinMaxBuiltin/) |
| Variadic functions | ⚠️ | `f(s...)` works. A function that keeps, returns or captures its `...T` parameter, or appends or copies into it, works on a private copy of the caller's slice ([Limitations](Limitations.md#what-converts-but-doesnt-behave-exactly-like-go)). [Example](../src/tests/Behavioral/VariadicFuncValues/) |
| `panic` and `recover` | ⚠️ | Go panics and run-time errors (nil dereference, index out of range, divide by zero) recover as in Go. Other .NET exceptions run the deferred calls but `recover()` returns nil ([Limitations](Limitations.md#what-converts-but-doesnt-behave-exactly-like-go)). [Example](../src/tests/Behavioral/PanicRecover/) |

## Methods and interfaces

| Feature | Status | Notes |
|---|---|---|
| Methods on any defined type | ✅ | Receiver methods become C# extension methods. [How](ConversionStrategies.md#functions-and-methods) · [example](../src/tests/Behavioral/ReceiverPointerValue/) |
| Pointer and value receivers | ✅ | Go's automatic `&` and `*` at the call site. [How](ConversionStrategies.md#implicit-pointer-dereferencing) · [example](../src/tests/Behavioral/ValueReceiverPointerMethodCall/) |
| Methods on a nil receiver | ✅ | [Example](../src/tests/Behavioral/NilReceiverMethods/) |
| Interfaces, satisfied structurally | ✅ | No `implements` needed: source generators build the C# glue. [How](ConversionStrategies.md#interfaces) · [example](../src/tests/Behavioral/InterfaceImplementation/) |
| Embedded and anonymous interfaces | ✅ | [Example](../src/tests/Behavioral/InterfaceInheritance/) · [anonymous](../src/tests/Behavioral/AnonymousInterfaces/) |
| Empty interface (`any`) | ✅ | Including `any` as a map key. [How](ConversionStrategies.md#empty-interface-any) · [example](../src/tests/Behavioral/AnyKeyMap/) |
| Interface comparison | ✅ | `==` on interface values compares dynamic type and value. [Example](../src/tests/Behavioral/InterfaceEqualityTyped/) |
| Struct embedding and promotion | ✅ | Promoted fields and methods, through pointers, across several levels and packages. [How](ConversionStrategies.md#struct-type-embedding) · [example](../src/tests/Behavioral/StructPromotion/) |
| Type assertions | ✅ | Both the panicking form and comma-ok. [Example](../src/tests/Behavioral/TypeAssert/) |
| Type switches | ✅ | [How](ConversionStrategies.md#type-switch-statements) · [example](../src/tests/Behavioral/TypeSwitch/). [Limitations](Limitations.md#what-converts-but-doesnt-behave-exactly-like-go) lists one rare `int`/`int32` case (status to confirm). |

## Generics

| Feature | Status | Notes |
|---|---|---|
| Generic functions | ✅ | [How](ConversionStrategies.md#generics) · [example](../src/tests/Behavioral/GenericFuncDecl/) |
| Generic types and their methods | ✅ | [Example](../src/tests/Behavioral/GenericTypeDecl/) |
| Constraints and type sets | ✅ | Unions, `~T`, `comparable`, and constraints with methods. [How](ConversionStrategies-Reference/generic-constraints.md) · [example](../src/tests/Behavioral/Constraints/) |
| Type inference | ✅ | [Example](../src/tests/Behavioral/TypeInference/) |
| Generic type aliases | ⚠️ | Converted, except an alias whose target is an anonymous struct or interface ([Limitations](Limitations.md#what-wont-make-the-trip)). [Example](../src/tests/Behavioral/GenericTypeAlias/) |
| A pointer element under a self-referential constraint whose method returns a collection of the element | ❌ | `Walk[*Node]` under `Walk[N Tree[N]]` with `Children() []N` is refused with `GO2CS2002`; the same code over a value element converts ([Limitations](Limitations.md#what-wont-make-the-trip)). |

## Concurrency

| Feature | Status | Notes |
|---|---|---|
| Channels, buffered and unbuffered | ✅ | [How](ConversionStrategies.md#channels-and-select) · [example](../src/tests/Behavioral/ChannelRendezvous/) · [buffered](../src/tests/Behavioral/ChannelCapLen/) |
| `close`, and closed channels | ✅ | Receive from a closed channel, panic on send, and blocked goroutines woken. [Example](../src/tests/Behavioral/ChannelReceiveFromClosed/) · [send](../src/tests/Behavioral/ChannelSendToClosed/) |
| Nil channels | ✅ | Block forever, and drop out of a `select`. [Example](../src/tests/Behavioral/NilChannelInSelect/) |
| `select` | ✅ | With `default`, and a random choice among ready cases. [Example](../src/tests/Behavioral/SelectStatement/) · [fairness](../src/tests/Behavioral/SelectRandomFairness/) |
| `sync`, `sync/atomic` | ✅ | Built on .NET primitives; both packages pass Go's tests. [Example](../src/tests/Behavioral/AtomicValues/) |
| `context`, timers and tickers | ✅ | `context` passes Go's tests. [Example](../src/tests/Behavioral/SyncTimerChannel/) |
| `runtime.Gosched`, `Goexit`, `LockOSThread` | ✅ | [Example](../src/tests/Behavioral/GoexitDefers/) |
| `testing/synctest` | ✅ | Passes Go's tests ([roster](ValidatedTestPackages.md)). |
| Goroutines | ⚠️ | Go's semantics, on real threads, not rewritten into `async`. Each live goroutine holds an operating-system thread, so thousands at once are fine and hundreds of thousands are not; macOS and Linux each cap the count lower ([Limitations](Limitations.md#what-converts-but-doesnt-behave-exactly-like-go), [Known issues](KnownIssues.md)). [How](ConversionStrategies.md#goroutines) · [example](../src/tests/Behavioral/GoCallVariations/) |
| Goroutine stacks | ⚠️ | Stacks do not grow: each goroutine reserves a large fixed stack, and recursion past it ends the process ([Tuning](Tuning.md#goroutine-stacks)). |
| `runtime.GOMAXPROCS` | ⚠️ | Read and set, but it does not limit parallelism: `GOMAXPROCS(1)` does not run goroutines one at a time ([Limitations](Limitations.md#what-converts-but-doesnt-behave-exactly-like-go)). |
| Deadlock detection | ⚠️ | Go's `all goroutines are asleep` message appears only when every goroutine waits on a nil channel or an empty `select {}`; any other deadlock hangs ([Limitations](Limitations.md#what-converts-but-doesnt-behave-exactly-like-go)). |
| The race detector (`-race`) | ❌ | Validation runs without it ([Limitations](Limitations.md#what-validated-means-and-what-it-doesnt)); converted code has no race detector (status to confirm). |

## Memory and runtime

| Feature | Status | Notes |
|---|---|---|
| `unsafe.Sizeof`, `Alignof`, `Offsetof` | ✅ | Fold to Go's numbers. [Example](../src/tests/Behavioral/UnsafeOperations/) |
| `reflect` | ✅ | `encoding/json` and `text/template` pass Go's tests on it; `StructOf`, `MakeFunc` and the rest. [How](ConversionStrategies.md#reflection-reflect) · [example](../src/tests/Behavioral/ReflectStructOf/) |
| `runtime.Caller`, `Callers`, `FuncForPC` | ✅ | Go file and line, read from the program's debug symbols. [Example](../src/tests/Behavioral/RuntimeCallerFrames/) · [FuncForPC](../src/tests/Behavioral/FuncForPCName/) |
| Garbage collection | ⚠️ | .NET's collector. `GOGC` and `GOMEMLIMIT` are read and reported, but the collector does not follow them ([Limitations](Limitations.md#what-converts-but-doesnt-behave-exactly-like-go)). |
| Finalizers, `runtime.AddCleanup`, `weak` | ⚠️ | Run on .NET object lifetime: an object may not be collectable until the function that dropped it returns. [Example](../src/tests/Behavioral/SetFinalizerBridge/) |
| `unsafe.Pointer` and `uintptr` | ⚠️ | Work over plain data. A pointer to anything .NET's collector tracks (a string, slice, map, interface or pointer inside) gets a stand-in number, so tricks such as a hand-built slice header do not convert. [How](ConversionStrategies.md#unsafepointer-and-uintptr) · [example](../src/tests/Behavioral/UnsafePointerReinterpret/) |
| `unsafe.Slice`, `unsafe.String` and friends | ⚠️ | Work, except that a slice of one number type laid over another's storage is a copy ([Limitations](Limitations.md#what-converts-but-doesnt-behave-exactly-like-go)). [Example](../src/tests/Behavioral/UnsafeSliceDataAliasing/) |
| `reflect` under Native AOT | ⚠️ | A type built at run time (`StructOf`) needs the JIT, and a few shapes no compiled code names are refused by name, as a panic `recover` can catch ([FAQ](FAQ.md#does-it-work-with-native-aot)). [Example](../src/tests/Behavioral/AotReflectGaps/) |
| Panic output and exit status | ⚠️ | Go's panic lines and exit status 2. The traceback is sometimes shorter than Go's ([Limitations](Limitations.md#what-converts-but-doesnt-behave-exactly-like-go)). [Example](../src/tests/Behavioral/GoroutinePanicExitCode/) |
| Goroutine dumps (`runtime.Stack` for all goroutines) | ⚠️ | Lists every goroutine and what it waits on, with frames for the calling goroutine only. |
| Profiling | ⚠️ | Heap and allocation profiles and `runtime/trace` annotations work; a CPU profile has no samples, so profile with .NET's tools. [Example](../src/tests/Behavioral/MemProfileAllocFrames/) · [trace](../src/tests/Behavioral/TraceUserAnnotations/) |
| `debug.ReadBuildInfo` | ⚠️ | The module, its dependencies and the main build settings; no compiler, build-mode or `vcs.*` settings. [Example](../src/tests/Behavioral/BuildInfoSettings/) |

## Packages, build and tests

| Feature | Status | Notes |
|---|---|---|
| Packages and imports | ✅ | Each package becomes a namespace and its own assembly. [How](ConversionStrategies.md#package-conversion) · [example](../src/tests/Behavioral/CrossPkgUser/) |
| `internal` packages | ✅ | Converted like any package; the standard library's are. |
| Modules and third-party dependencies | ✅ | `-recurse=nuget` converts a module and everything it imports ([walkthrough](ConvertingAModule.md)). Some dependencies need a build tag ([Limitations](Limitations.md#third-party-modules-where-they-trip-today)). |
| Build tags, `GOOS`, `GOARCH` | ✅ | Files are selected as Go selects them; one conversion targets one operating system. [Example](../src/tests/Behavioral/FileNameBuildConstraints/) |
| `//go:embed` | ✅ | `string`, `[]byte` and `embed.FS`. [Example](../src/tests/Behavioral/EmbedDirectiveForms/) |
| The Go language version | ✅ | Code must build with the Go release go2cs tracks ([Requirements](README.md#requirements)); a newer one is refused by name. |
| Go's standard library | ✅ | Converted in full and validated against Go's own tests, package by package ([roster](ValidatedTestPackages.md)). |
| `Test` functions, through `-tests` | ✅ | Converts a package's tests, runs them, and compares each verdict with `go test` ([how](BuildingGo2cs.md#validate-a-converted-test-suite)). |
| `//go:linkname` | ⚠️ | A link to a function or variable with a C# body forwards to it; any other converts to a stub that fails when called. [How](ConversionStrategies.md#functions-without-a-go-body) · [example](../src/tests/Behavioral/LinknameVarPull/) |
| Native AOT, trimming, ReadyToRun | ⚠️ | All work. A Native AOT publish is slow and large ([FAQ](FAQ.md#does-it-work-with-native-aot)). |
| Examples, benchmarks and fuzz targets | ❌ | Not run yet ([Roadmap](Roadmap.md#phase-4d-examples-fuzz-seed-corpora-and-benchmarks)). |

## Platforms

| Target | Status | Notes |
|---|---|---|
| `windows/amd64` | ✅ | The standard library is validated here. |
| `linux/amd64` | ✅ | glibc, validated here. Ubuntu's own .NET package has a [known issue](KnownIssues.md#ubuntus-packaged-net-10-can-end-a-converted-program-early). |
| `darwin/amd64`, `darwin/arm64` | ⚠️ | Packages ship for both chips and validation is package by package: a [roster](ValidatedTestPackages.md) row shows a macOS count once its package validates. On Apple silicon the standard library is the `amd64` build ([Known issues](KnownIssues.md#on-apple-silicon-runtimegoarch-reports-amd64)). |
| `linux/arm64` | ⚠️ | Packages ship for it (status to confirm). [Platforms](Roadmap.md#platforms) |
| `windows/arm64` | ⚠️ | Builds with a warning and runs the Windows build, whose architecture-specific choices follow `amd64`; not validated. [Platforms](Roadmap.md#platforms) |
| Other targets (`wasm`, the BSDs, 32-bit) | ❌ | [Platforms](Roadmap.md#platforms) |

## Interop

| Feature | Status | Notes |
|---|---|---|
| Calling converted Go from C# | ✅ | A converted package is an ordinary .NET library ([Consuming converted Go](ConsumingGoFromCSharp.md)). [Example](../src/tests/CSharpConsumer/) |
| System calls | ✅ | The `syscall` package on Windows, Linux and macOS, including `syscall.NewCallback` on Windows. [Example](../src/tests/Behavioral/LinuxSpawnBasics/) · [callback](../src/tests/Behavioral/WindowsNewCallback/) |
| cgo: calling C functions | ⚠️ | A package that calls C library functions converts and compiles, and loads its C library at run time. C types, typedefs and constants arrive through a `C` pseudo-package, and structs pass by value or by pointer ([Roadmap](Roadmap.md#cgo)). |
| Calling .NET from Go source | ❌ | The road runs one way: Go source cannot name a .NET API ([Limitations](Limitations.md#reading-and-debugging-the-output)). |
| cgo: callbacks, C globals, unions, inline C | ❌ | Function pointers and callbacks into Go, C global variables, unions by value, flexible array members, a pointer into an array element as an argument, and C code written inline in the Go file ([Roadmap](Roadmap.md#cgo)). |
| Hand-written assembly (`.s` files) | ❌ | Not translated: such a function converts to a stub that fails when called. Many libraries have a pure-Go build tag ([Limitations](Limitations.md#what-wont-make-the-trip)). The standard library uses Go's portable code and hand-written C# instead ([how](ConversionStrategies.md#the-standard-library-reproduces-go--tags-purego)). |
| `plugin` | ❌ | `plugin.Open` returns `plugin: not implemented`, as in a Go build without cgo. |

</div>
