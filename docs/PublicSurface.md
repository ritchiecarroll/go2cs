# What go2cs promises to C# code that uses it

A C# project that references a `go.*` or `nugetgo.*` package, or calls code go2cs converted, binds to names that go2cs
chose. This page says which of those names are a promise and which are not, and how a promised name may change.

There are three tiers. Every consumer-visible name is in one of them, or is a [contract member](#contract-members) of
a type, which goes with that type.

## Tier 1: frozen

These do not change. A change to any of them is a break, and takes the [deprecation path](#how-a-promised-name-changes).

- **The exported surface of a Go package that is not `internal` or `vendor`.** Every exported Go type, function,
  method, constant, variable and field keeps the C# name and shape go2cs gives it. When Go itself removes or
  renames one in a new Go release, the converted package follows Go; that is Go's own compatibility promise, not a
  go2cs change.
- **The type mapping.**
  - Go's numbers map by size, with `int` and `uint` as `nint` and `nuint` and `rune` as `int`
    ([Numbers](ConsumingGoFromCSharp.md#numbers)).
  - `string` is `go.@string`.
  - A function with several results returns a C# tuple.
  - A method with a pointer receiver takes `ref T` or `ж<T>`.
  - An exported package variable is a `ref` property.
  - Slices, maps, channels, arrays and pointers are golib's `slice<T>`, `map<K, V>`, `channel<T>`, `array<T>` and
    `ж<T>`.
- **golib's consumer API.** The types a C# program uses directly: `@string`, `slice<T>`, `map<K, V>`, `channel<T>`,
  `array<T>`, `ж<T>`, `error`, the numeric types such as `complex128` and `uintptr`, `builtin` and its functions
  (`len`, `make`, `append`, `nil`, `goǃ` and the rest), and the exceptions below. The full list is
  `src/core/golib/PublicSurface.golib.txt`; a type that is not on it is tier 2.
- **The exceptions a consumer catches.** `PanicException` and `GoErrorException`, their names and their properties.
- **Package IDs and the version scheme.** A standard-library package is `go.<import path>` (`go.encoding.hex`), a
  converted module's package is `nugetgo.<package path>` (`nugetgo.github.com.google.uuid`), and a version is the Go
  release followed by a fourth number that counts go2cs's builds of it (`1.26.10.4`).
- **`go2cs/source-metadata.txt`**, the file in every package that names the Go release and the go2cs build it came
  from. Its format is frozen; tools read it.

## Tier 2: stable within a Go minor release

These are what converted code itself uses. They do not change between revisions of one Go minor release (1.26.x).
They may change when the [break floor](ConsumingGoFromCSharp.md#the-break-floor) moves, and only then. C# code should
not name them, and IntelliSense hides them: each carries `[EditorBrowsable(EditorBrowsableState.Never)]`.

- **Generated plumbing**: the interface adapters (`SжI`, `SᴠI`), the `{pkg}_package_asserts` classes, the promoted
  method hosts (`…ᴛpkg`, `…ᴛxpkg`), the field-access token classes, constraint proxies, the canonical delegate
  fields (`Fᶠ`), and a named pointer type's `DerefOrNull`.
- **golib's emitted-code API**: the `Go…` attributes (`GoImplement`, `GoRecv`, `GoPromoted`, `GoType` and the rest),
  the adapter and type registries, `StandardBox<T>` and the other box classes, `GoFrame`, `sstring`, `UntypedInt`,
  and the runtime classes in namespace `go.golib`. Those that live in namespace `go` are listed in the baseline's
  `[go-tier2]` section. Adding one there needs a COORD ruling, cited beside it; any other new golib name goes in
  `go.golib`.
- **`GoexitException`**: `runtime.Goexit` unwinds with it. Catching it is not supported.

A type in a generated host is plumbing, but its methods are not. The promoted methods a host carries are part of the
Go method set, so they stay visible in member completion; only the host's own name is hidden.

## Contract members

go2cs-gen also writes public members onto the Go types themselves. Those a C# caller binds to are contract members:
they stay visible and go with the tier of the type they belong to. They are:

- the value contract of a Go type: its constructors, its operators and conversions (`==` against `nil`, `implicit`
  from `nil`), `Equals`, `GetHashCode` and `ToString`;
- overloads of a name the Go declaration has: the `ж<T>` form of a method, and the `@string` form of a function that
  takes `sstring`;
- field accessors (`Ꮡfield`), a named pointer type's `Value`, `ValueSlot`, `IsNull` and `IsNilPointer` and its `of`
  and `at` views, and `ΔClone`, `Make`, `Wrap` and `Source`.

Any other generated public member is hidden. The list is checked: GenTests' `HiddenPlumbingTests` fails on a generated
public member that is neither hidden nor one of these.

## Tier 3: no promise

These can change in any release. Only the first item is marked in the packages.

- **Go `internal` and `vendor` packages.** They are public to .NET, as Go's import rule has no .NET equivalent, but
  Go changes them freely between releases. Do not reference them. Each one's assembly records the root Go allows to
  import it (`[assembly: GoInternalRoot]`), its package class is hidden from IntelliSense, and C# that names one of
  its types from outside that root gets warning [`GO2CS0014`](DiagnosticCodes.md). The warning may become an error
  at a release whose notes say so.
- **Glyph-named temporaries** in converted code, and everything a build writes under `obj/`.
- **Tuple element names.** A Go result rename changes them. Use positional deconstruction when it matters.
- **Thread identity.** Which OS thread runs a goroutine, and whether code after a blocking call resumes on the same
  thread. `runtime.LockOSThread` is the way to pin a goroutine to its thread.
- **Platform-specific surface.** `syscall`, `runtime` and `os` differ per platform. A package's `lib/` folder holds
  the Windows build, and a C# project compiles against it.

## Diagnostic codes

- A `GO2CS` diagnostic code is never reused for a different diagnostic. A retired code stays retired
  ([Diagnostic codes](DiagnosticCodes.md)).
- A diagnostic's severity only tightens (a warning can become an error, never the reverse), and only at a release
  whose release notes say so.
- A converter flag's default does not change within a Go minor release.

## MSBuild properties

For a project that references `go.*` or `nugetgo.*` packages:

| Property | Default | What it does |
|:--|:--|:--|
| `GoConsumerUsings` | on | The global `using go;` and `using static go.builtin;`, and each directly referenced package's namespace. `false` turns them off. |
| `GoGlobalPackageAliases` | off | Also a global alias per referenced package, named for the Go package (`hex` for `go.encoding.hex_package`). |
| `GoAllowBelowBreakFloor` | off | Builds against a package below the break floor, with a warning instead of [`GO2CS3002`](DiagnosticCodes.md). |
| `GoBreakFloor` | the release's floor | The break floor itself. A `Directory.Build.targets` can lower it; the project file cannot. |
| `GoCompileRuntimeIdentifier` | the build's RID, else the host's | The platform whose build of a platform-split package the project compiles against. |
| `GoRidCompileAssets` | on | Compiles against that platform's build. `false` compiles against `lib/` (Windows). |
| `GoAllowReferenceFlavor` | off | As a global property only (`-p:`): runs the Windows build on a platform the packages do not ship, instead of `GO2CS3003`/`GO2CS3004`. |
| `GoCopyPackageSymbols` | on | Copies the packages' `.pdb` files to the output, so a Go traceback prints file and line. |
| `GoKeepSymbolsLoose` | on | Keeps those `.pdb` files beside a single-file publish. |
| `GoInternalPackageWarning` | on | Warning [`GO2CS0014`](DiagnosticCodes.md) where C# names a type of a Go `internal` or `vendor` package. `false` turns it off. |

For a converted project that builds against a local standard library: `GoTargetOS` (`windows`, `linux` or `darwin`)
picks the platform build of each package.

Other properties go.lib sets are internal and can change.

## How a promised name changes

A tier-1 or tier-2 name changes in one of two ways, and no other:

1. **When the old name can coexist with its replacement**, the old one stays for one release, marked `[Obsolete]`
   with the replacement named, and is removed in the next.
2. **When it cannot**, the change ships as a set:
   - the [break floor](ConsumingGoFromCSharp.md#the-break-floor) moves to the release that makes it, so a package
     built against the old surface stops the build by name instead of failing at run time;
   - the package's compatibility check records the change;
   - the release notes have an "Upgrading" section that says what changed and what to do;
   - every published `nugetgo.*` package is rebuilt against the new release.

Before each release, the previous release's C# consumer program (`src/tests/CSharpConsumer`) is built unchanged
against the new packages and must pass all its checks ([`release-smoke`](CIMatrix.md), arm H). That build fails when
a name the program uses is changed any other way; a name it does not use is not tested there. golib's own tier-1
types and members are held line by line by its baseline, `src/core/golib/PublicSurface.golib.txt`, whose test fails
on any change to them.

A release that breaks by design, the second way above, makes that build red on purpose. Its release smoke then builds
an updated consumer, named with the `consumer_ref` input, and must cite the break with `consumer_override`:
`floor=<the moved break floor>;news=<the release notes>`. The arm refuses the override unless the floor is the one the
tree declares and lies above the previous release, and the release notes hold an "Upgrading from <previous release>"
entry. Its verdict line then reads OVERRIDE, so the exception is visible in the release's record.

<!-- The contract adopted by the owner, 2026-11-11 (docs/PLAN-public-surface-stability.md, ruling block; OQ3 and the
     "freeze and document" items; seat C1). Sources: the type mapping, docs/ConversionStrategies.md; the MSBuild
     properties, src/core/golib/buildTransitive/go.lib.targets and go.lib.symbols.targets (GoTargetOS:
     docs/ConvertingAModule.md); the break floor and GO2CS3002/3003/3004, docs/ConsumingGoFromCSharp.md and
     docs/DiagnosticCodes.md; the code-reuse precedent, GO2CS2004; the package compatibility check,
     src/tools/pushnuget/GoApiCompat.psm1 and each package's CompatibilitySuppressions.xml. The golib baseline file
     and its test, the generated-plumbing marking and the interface rule below land in the same seat. -->

## Go interfaces you implement in C#

A C# class can implement a converted Go interface, as
[`io.Writer`](ConsumingGoFromCSharp.md) does in the consumer guide. go2cs never adds an abstract member to a
Go-named interface. Anything it adds is either implemented on the interface itself or lives on a separate interface,
so a class written against one release keeps compiling against the next.
