# go2cs features

What the converter does for you, then how much of Go it covers, then real converted files to judge it
by. For where it falls short, see [Limitations](Limitations.md).

## What the converter does

- **Output that reads like the Go it came from.** Receiver methods become extension methods, multiple
  returns become tuples and embedded structs become promoted fields. Numeric literals keep Go's
  formatting, and `-comments` carries the Go comments across. The machinery behind Go's semantics
  stays out of the converted files, in a runtime library and compile-time source generators.
- **Whole modules, not only files.** `-recurse` converts a module and every third-party package it
  imports, in dependency order, and writes a C# project and solution for each, ready to build
  ([Converting a real-world Go module](ConvertingAModule.md)).
- **Nothing to stage.** With `-recurse=nuget` the standard library, the runtime and the source
  generators are NuGet package references. A third-party module that someone has already converted and
  published is referenced as its package too, pinned with its hash in a lock file
  ([Mapping modules to NuGet packages](ConvertingAModule.md#mapping-modules-to-nuget-packages)).
- **A built-in check against Go's own tests.** `-tests` converts a package's `_test.go` suite, builds
  and runs it, runs `go test`, and compares the two verdict for verdict
  ([Validate a converted test suite](BuildingGo2cs.md#validate-a-converted-test-suite)). The same command validates a whole module.
- **Platform-aware conversion.** Files are selected by build tag, `GOOS` and `GOARCH` as Go selects
  them, for the platform you name with `-platforms`.
- **Repeatable output.** The same input converts to the same bytes, so converted code diffs cleanly
  in source control.
- **Errors your tools already read.** A problem in the Go input is reported in MSBuild's error format
  with a `GO2CS` code, which CI logs and editors pick up ([Diagnostics](BuildingGo2cs.md#diagnostics)).
- **Hand-written files survive.** A file you mark as manually converted is left alone by the next
  conversion ([how](ConversionStrategies.md#manually-converted-declarations)).
- **Managed first.** The output is portable managed C#. Native interop is a last resort, not the
  default.
- **Ordinary .NET deployment.** A converted program builds with `dotnet build`, runs with `dotnet run`
  and publishes self-contained with `dotnet publish`; a native launcher is built beside the assembly.

## Go language coverage

go2cs converts the full Go language surface except a generic type alias whose target is an anonymous
struct or interface type (`type A[T any] = struct{…}`), which it reports and does not yet convert.

**Types & values**

- Slices and arrays (backing-array aliasing preserved, as in Go), maps, and UTF-8-backed `@string`
- `int` / `uint` as platform-width native integers; named numeric types and untyped-constant semantics
- Constants and `iota`, preserving Go's numeric literal formatting (hex, binary, underscores, exponents)
- Pointers with automatic heap-boxing driven by escape analysis; `nil`; `unsafe.Pointer`
- Type definitions and aliases — including exported aliases that resolve across assembly boundaries, and Go 1.24 generic aliases, which render as their target type (except one whose target is an anonymous struct or interface)

**Functions & methods**

- Multiple return values and named results, as tuples
- Receiver methods as extension methods, with distinct pointer- and value-receiver overloads
- Function values, closures (honoring Go's shared-storage capture semantics), and variadic functions
- `defer` / `panic` / `recover`, including named results observed and mutated by deferred closures

**Concurrency**

- Goroutines, each on its own thread (behavioral equivalence first — not rewritten into `async`)
- Channels with channel-operator (`<-`) lowering, and `select`-statement lowering

**Composition & polymorphism**

- Struct embedding with promoted fields and methods (multi-hop and cross-package)
- Interfaces, satisfied structurally in Go and realized as nominal C# glue via Roslyn source generators
- Type assertions and type switches
- Generics: type parameters and constraints (unions, `comparable`, `~`-underlying, method sets)

**Control flow & packaging**

- `for` / `range`, labeled `break` / `continue`, expression and type switches, Go 1.22 loop variables
- The built-ins: `append`, `len`, `cap`, `make`, `new`, `copy`, `delete`, `close`, …
- Packages mapped to namespaces; cross-package imports compiled as separate, referenced assemblies
- Build-tag and `GOOS` / `GOARCH` platform file selection, and deterministic, byte-stable output

See [`ConversionStrategies.md`](ConversionStrategies.md) for an example-driven tour of how each construct
maps to C# (with [`ConversionStrategies-Reference/`](ConversionStrategies-Reference/README.md) for the full detail).

## Real standard-library conversions, side by side

The main page pairs converted standard-library files with their Go source, from simple to complex: [Real standard-library conversions, side by side](README.md#real-standard-library-conversions-side-by-side).
