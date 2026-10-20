# go2cs diagnostic codes

go2cs reports problems with a `GO2CS` code from three sides. The converter (`go2cs`) prints its codes on stderr
while it converts Go to C#. The source generators and analyzer in `go2cs-gen` report theirs in the C# build of a
converted project, beside the compiler's own `CS` codes, and so do go2cs's MSBuild targets. Every code any side
can report is listed below.

The number says which side reported it:

- **`GO2CS0xxx`**: `go2cs-gen`, in the C# build.
- **`GO2CS1xxx`**: the converter, for a Go package that did not load cleanly.
- **`GO2CS2xxx`**: the converter, for a Go input it refuses to convert.
- **`GO2CS3xxx`**: go2cs's MSBuild targets (`go.lib.targets`), in the C# build.

The converter prints each of its codes on stderr in MSBuild's canonical error format; the
[command-line options](CommandLine.md#diagnostics) page shows the line format. A `go2cs-gen` or build code
appears in the build output like any compiler diagnostic, and a warning can be suppressed the usual way
(`NoWarn`).

| Code | Side | Severity | Meaning | What to do |
|:--|:--|:--|:--|:--|
| `GO2CS0001` | go2cs-gen | Warning | A package class stem contains the lifted-name marker `ᴛ`, so the cross-package forwarder class the generator mints for a struct may collide with another struct's (`CS0101`). No Go package in the standard library has such a stem. | Report it as a go2cs issue, with the package path the message names. A `CS0101` on a generated `…xpkg` class is this collision. |
| `GO2CS0002` | go2cs-gen | Warning | A go2cs-gen record (an assembly or type attribute the converter emits) could not be read, so it was skipped and nothing was generated for it. The omission can only fail later and loudly, at the use site, or nothing was owed. | Re-convert the package. A record the converter emitted that the generator cannot read is a go2cs issue; a record edited by hand is fixed or removed. |
| `GO2CS0003` | go2cs-gen | Error | A go2cs-gen record cannot be generated: for example a Go comment no record can carry, a record whose key would match two methods of one type, or a module initializer that would need a generated no-inlining declaring part. The build stays red at the record, so it cannot go green with members missing at run time. | Read the message, which names the record and the reason. In converter output this is a go2cs issue; in a hand-written file, change the record it names. |
| `GO2CS0004` | go2cs-gen | Error | A C# string literal in converted code binds a type Go's string is not: an `object` or interface parameter, a type parameter inferred as `System.String`, or a `System.String` overload chosen over the Go one. The program would see a C# string where Go has a Go string. | In converter output, report it as a go2cs issue. In hand-written code inside a converted package, pass a Go string (`(@string)"text"`). A golib parameter that is meant to take a C# string is marked `[GoTakesCSharpString]`. |
| `GO2CS0005` | go2cs-gen | Error | A C# string literal in converted code reaches an `any` (object) slot of a call that go2cs-gen cannot intercept, so the callee would see `System.String` where Go has a Go string. The message names the reason: a delegate (a Go func value), a generic method or a method of a generic type, a method generated code cannot call, a by-reference return, or a literal holding a lone UTF-16 surrogate. | In converter output, report it as a go2cs issue: the converter passes a bare literal to an `any` slot only where the call can be intercepted. In hand-written code inside a converted package, pass a Go string (`(@string)"text"u8`). |
| `GO2CS0006` | go2cs-gen | Error | A C# string literal in converted code binds a member that does not take it as `any`, while another member of the same method group does: an overload beside the converted callee (often one taking `go.@string`, which C# prefers to `object`) has captured a call that Go makes to one function. | Remove or rename the overload the message names, or pass a Go string explicitly if that overload is the one meant. In converter output with no hand-written overload, report it as a go2cs issue. |
| `GO2CS0008` | go2cs-gen | Error | A `GoImplicitConv` record pairs a type with its own box, `<T, ж<T>>` (in either order). Go never converts between a value and a pointer implicitly, so the record means the converter placed a value where Go passes a pointer. No operator is generated: the one this record used to produce boxed a copy of the value, so the conversion compiled while a write through the box never reached the original. The build fails at the record, and the site that needed the conversion fails to compile. | In converter output, report it as a go2cs issue, naming the record and the failing site: the converter must pass the pointer's box there. In hand-written code, pass the box (`Ꮡx`) instead of the value. |
| `GO2CS1000` | converter | Error | The package did not load cleanly: an error `go/packages` does not classify. The package still converts best-effort. | Fix the error the message names, so that `go build` succeeds for the package, then convert again. |
| `GO2CS1001` | converter | Error | The package did not load cleanly: a `go list` error, such as a missing module or no Go files for this build. | Fix the module or build setup the message names (`go.mod`, `GOOS`/`GOARCH`, build tags) so that `go list` succeeds. |
| `GO2CS1002` | converter | Error | The package did not load cleanly: a Go parse error. The package still converts best-effort. | Fix the Go syntax error at the position given. |
| `GO2CS1003` | converter | Error | The package did not fully type-check. It still converts best-effort, but code that depends on the failing expression is emitted untyped and will not compile. | Fix the type error at the position given, so that `go build` succeeds for the package, then convert again. |
| `GO2CS2001` | converter | Error | Refused: a cgo source (`import "C"`) is selected for this build, and cgo has no C# conversion yet. The run exits non-zero. | Convert with `CGO_ENABLED=0`, or exclude the file from the build. |
| `GO2CS2002` | converter | Error | Refused: a generic instantiation over a pointer element (`Walk[*Node]` under `N Tree[N]`) needs a C# constraint proxy, and a method of the constraint passes a value that shares the element's storage in Go (`Tree.Children (returns []N)`; a map, channel or pointer of `N`, or a generic type carrying `N`). A proxy is a separate object per pointer and cannot forward that sharing. Each site and method is named, and the run exits non-zero. | Instantiate over a value element (value-receiver methods), which needs no proxy. Or shape the method so it forwards exactly: return `N` itself, a tuple, a func or iterator (`iter.Seq[N]`), or an array `[K]N`. See [generic constraints](ConversionStrategies-Reference/generic-constraints.md#what-a-proxy-forwards-and-the-instantiations-go2cs-refuses). |
| `GO2CS2003` | converter | Error | Refused: a conversion to a type the converter hand-converts (runtime's `guintptr` family) takes the method's own pointer receiver through `unsafe.Pointer`, for example `guintptr(unsafe.Pointer(gp))` in a method of `*g`. The type stores the pointed-to value's box, and a receiver is emitted as the value itself with no box to pass, so the conversion would hold a copy. The run exits non-zero. | Hand-convert the method with a `this ж<T>` receiver (list it in `manualConversionFuncs`), as runtime's `g.guintptr` is. |
| `GO2CS3001` | build | Error | The C# compiler running the build is older than Roslyn 4.14, the version go2cs-gen is built against. The generator could not load, and Go string literals in `any` slots would reach their callees as `System.String`. | Build with the .NET 10 SDK and its own compiler. Remove any `Microsoft.Net.Compilers.Toolset` package that pins an older compiler. |

## Adding a code

Each side has one registry, and a code is declared there and nowhere else:

- the converter's codes are the `const` block in `src/go2cs/diagnosticOutput.go`;
- `go2cs-gen`'s codes are the `DiagnosticDescriptor` fields of `GeneratorDiagnostics` in
  `src/gen/go2cs-gen/GeneratorDiagnostics.cs`;
- the build's codes are the properties of the `Go2csDiagnosticCodes` block in
  `src/core/golib/buildTransitive/go.lib.targets`, each raised by an `Error` or `Warning` task whose `Code` names
  the property. No other targets file raises a `GO2CS` code.

A new code takes the next free number in its side's range, and a row in the table above in the same change. The
repository test `TestGo2csDiagnosticCodeRegistry` (`src/go2cs/internal/repoguard`) fails, naming the code, when a
code is used in the source or in a user-facing page but not registered, registered twice, registered outside its
side's range, missing its row here, or (on the build side) raised by no task, and when a row names a code that is
not registered.
