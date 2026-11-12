# DESIGN — where a struct's promoted-method forwarders live (the CoreCLR methods-per-type limit)

> **State: PROPOSED (P1, 2026-11-10), for adversarial review before the seat is cut.** Ordered by COORD
> (2026-11-10 07:08Z) after the TypeScript experiment's `ast` package built for the first time and failed to load.
> Nothing here is implemented. Every claim is marked **measured** (with its ref and instrument) or **inferred**.
> Refs read: go2cs-gen and golib at `8abe08046e` (the embed-depth seat, in batch 281) and at master `66a2e023ca`;
> the files cited in §5 are identical at both unless §5 says otherwise.

## 1. The problem, measured

- **The failure** (the TS session, from metadata; relayed by COORD): the converted parse demo builds with rc 0 and
  every run dies before `main` with `System.TypeLoadException: Type '…ast_package' … contains more methods than the
  current implementation allows`. `ast.dll` holds 59,209 types and 256,275 methods; `ast_package` alone holds
  116,102. go2cs-gen's TypeGenerator wrote 107,835 of them: the forwarders of promoted methods, a `this T` (or
  `this ref T`) form and a `this ж<T>` form per promoted method per embedding struct, all into the one
  `partial class ast_package`. About 214 node types embed `Node`, which has about 250 methods.
- **The limit is 65,524, not 65,535** (measured, dotnet/runtime tag `v10.0.0`): `methodtablebuilder.h` defines
  `MAX_SLOT_INDEX = static_cast<UINT16>(-1) - 10`, and `MethodTableBuilder::EnumerateClassMethods` throws
  `IDS_CLASSLOAD_TOO_MANY_METHODS` when `MAX_SLOT_INDEX <= cMethAndGaps`. A type loads with at most 65,524
  MethodDef rows. A "just under 65,535" control between 65,525 and 65,535 fails to load.
- **What counts**: every MethodDef row of the type: methods, constructors, accessors, and what the C# compiler
  lowers onto the type (a local function becomes a method of its containing type). The C# compiler has no limit.
- **The standard library is far below it** (measured with the census tool of §9 on a linux Release build of a
  63-assembly `fmt` closure at `af91182836`): the largest type is `runtime_package` with 5,705 methods, 172 of them
  marked `[GoPromoted]`; then `syscall_package` 2,425 and `runtime.dll`'s `GoFieldAccess` 1,835.
- **Forwarders in the standard library** (measured by reading the generator's output left on disk by the embed-depth
  seat's stdlib builds; one flavour per package, darwin or windows, so a sizing and not a prediction): 2,618
  forwarder methods on 187 structs in 37 packages; the largest are `internal/abi` 511, `runtime` 373, `go/types` 242,
  `net/http` 240, and the largest single struct is `runtime.traceWriter` with 110.

## 2. The proposal: one holder class per embedding struct

Each struct's same-package forwarders move, unchanged, out of the package class into a top-level static class of
their own, written in the same generated file after the package class closes:

```csharp
// today, inside `public static partial class main_package { … }`
[global::go.GoPromoted] [global::go.GoRecv] internal static global::go.@string Pick(this ref eOuter target) => target.eFork.Pick();
internal static global::go.@string Pick(this ж<eOuter> Ꮡtarget)
{
    ref var target = ref Ꮡtarget.Value;
    return target.Pick();
}

// proposed, after the package class
[GeneratedCode("go2cs-gen", …), DebuggerNonUserCode]
public static class mainᴛeOuterᴛpkg
{
    [global::go.GoPromoted] [global::go.GoRecv] internal static global::go.@string Pick(this ref global::go.main_package.eOuter target) => target.eFork.Pick();
    internal static global::go.@string Pick(this global::go.ж<global::go.main_package.eOuter> Ꮡtarget)
    {
        ref var target = ref Ꮡtarget.Value;
        return target.Pick();
    }
}
```

- **The name** is `{pkg}ᴛ{Struct}ᴛpkg`, beside the two sibling classes the generator already writes:
  `{pkg}ᴛ{Struct}ᴛxpkg` (forwarders of methods promoted from ANOTHER package's embed,
  `StructTypeTemplate.EmitCrossPackage`) and `{pkg}ᴛ{struct}ᴛ{iface}ᴛpromoted` (the twins of an embedded
  interface's methods, `InterfaceImplTemplate`). The stem rule is `XpkgClassName`'s: the first `ᴛ` ends the package
  stem, and GO2CS0001 already reports a package stem that holds one.
- **The class is declared through `TemplateBase.DeclareExtensionHost`**, as the `xpkg` class is, so the header lists
  it for golib's extension scan under Native AOT. Under the JIT the scan walks the assembly's metadata for every
  top-level static class that declares an extension method, so it finds the holder with no listing.
- **Only two spellings change**: the receiver type, and the `T.Ꮡ<embed>` accessor in the box shim, become fully
  qualified, as `EmitCrossPackage` already writes them. Measured on the 2,618 standard-library forwarders: every
  other type in a signature is already `global::`-qualified, a C# keyword, a `System.` name or a type parameter the
  method itself declares.
- **This is Go's own shape**: Go's linker symbols for promotion wrappers are per type (`main.(*outer).Ptr`), not per
  package.

**Why it resolves the same.** C# gathers extension-method candidates per NAMESPACE level, from every static class
directly in the namespace and then from the namespaces its `using` directives import; the enclosing class is not a
level. Converted code reaches another package's methods through a namespace `using`
(`addMethodPackageNamespaceUsing`). A holder in the package's namespace is therefore in the same candidate set, at
the same level, as the package class is today, for the package's own code and for every importer. A `-tests`
white-box class also imports the production class with `using static`, but it sits in the production class's
namespace, so the production holders are in scope there by the namespace rule too. (Inferred from the C# rules; the
`xpkg` class is the measured precedent, guarded by GenTests' `CrossPackagePromotedForwarderTests`.)

## 3. The alternative: instance members on the partial struct (not recommended)

| Question | Instance member on the struct | Holder class (§2) |
|---|---|---|
| The `ж<T>` form | Cannot be an instance member of `T`. It stays an extension, so a static class is needed anyway. Leaving the roughly 54,000 `ж<T>` forms on `ast_package` leaves it near 62,000: under the limit by about 5%. | Both forms move. |
| Binding order | C# binds an applicable instance member before any extension. A type's own methods are extensions, so a gap in the generator's shadowing rule runs the wrong method silently. Today such a gap is CS0111 or CS0121. `TypeGenerator.GoMethodNames` already records this hazard for wrapper types. | Unchanged: both are extensions at one level. |
| Pointer-receiver methods | An instance call on a `readonly` or `in` receiver copies it silently and the write is lost. | `this ref T` refuses a non-variable at compile time, as today. |
| Member names | CS0542 for a method named like its struct; collisions with the struct's generated members and its promoted-field accessor properties; `Equals`, `GetHashCode`, `ToString`, `GetType`. | None: extensions have no such rules. |
| The run-time method set | golib reads Go method sets from extension methods (`GetGoMethodSetCandidates`, `AdapterBinder`, reflect's binder, `GoMethodText`), and reads value-set versus pointer-set from the receiver's ref kind and marks. Each reader would need an instance-member arm. | Unchanged: the scan reads every listed host. |
| Generic structs | Simpler: no re-declared type parameters. | As today: generic methods repeating the struct's parameters and constraints. |

The call site reads the same in both (`x.Pos()`). The instance form costs a second mechanism in the compiler's
binding order and in every run-time reader, and still needs the holder for `ж<T>`.

Two other shapes were considered and dropped. Emitting no forwarders and spelling the embed path at each call site
(`x.NodeBase.NodeDefault.Pos()`) breaks the visual rule and removes the methods from the run-time method set. One
generic forwarder per embedded method over a marker interface (`Pos<T>(this ref T) where T : IEmbeds<Node>`) removes
the N×M growth, but a shallower method through another embed then competes with it as an ambiguity (CS0121)
instead of shadowing it, and the run-time method set would have to be derived instead of read.

## 4. COORD's questions

1. **Call-site visibility.** Unchanged: `x.Pos()`. No converted call moves (§2).
2. **The `ж<T>` form.** It is why the forwarders are extensions, and it stays one, in the holder.
3. **Overload resolution against a type's own methods.** The embed-depth rules decide WHICH forwarders are written
   (shallowest depth wins, a tie promotes nothing, a declared method shadows). Placement does not enter them, and
   own methods and forwarders stay in one candidate set.
4. **Generic embedding types.** A holder is a non-generic class named from the struct's non-generic name; its
   methods are generic methods carrying the struct's type parameters and constraints, exactly as they are emitted
   now. Go cannot declare two types of one name, so the name is unique.
5. **Cross-package embedding.** Already in a sibling class (`ᴛxpkg`); it stays there, because the metadata harvest
   reads "found in the `xpkg` class" as "a cross-package promotion" (`GetMetadataPromotedMethods`). The parked D rows
   (a defined type whose chain reaches another package's struct) are a different generator path
   (`InheritedTypeTemplate`); this design does not close them, and member forwarders written for them later belong in
   the same per-type holder.
6. **Can any single type still exceed the limit?** §7.

## 5. What reads the forwarders where they are today (measured, by reading the code)

Moving the forwarders is safe only if every reader that assumes "a type's methods are on its package class" follows.

| Reader | What it assumes | Change |
|---|---|---|
| The converter's method EXPRESSION arm (`convSelectorExpr.go`) | `(*outer).Add` is emitted as the bare method group `((Func<ж<outer>, nint, nint>)(Add))`, or `pkg.Add` for another package's type. A bare or class-qualified group does not look in other classes. | Emit the lambda form the cross-package arms already emit (`(p0, p1) => p0.Add(p1)`, with their wrapper mark and nil-receiver read). Census, three targets: 0 sites in the production standard library, 3 in `reflect`'s `all_test.go`, 2 in the behavioral `PromotedMethodExpr`. |
| `ImplementGenerator`, foreign-struct arm | Scans the struct's package class in metadata for box-bound and ref-bound methods, then calls `pkg_package.M(m_box, …)`. | Scan the holder too; spell each call with the class that declares the method. |
| `StructTypeTemplate.GetMetadataPromotedMethods` and `BuildTreeNameCounts` | A metadata embed's methods are on its package class, plus its `xpkg` class. | Add the embed's holder as a same-package source. |
| `StructDeclarationSyntaxExtensions.GetForeignBoxReceiverMethodNames`, `GetForeignValueReceiverMethods`, `GetForeignBoxReceiverMethodReturnTypes` | The same assumption, for interface adapters over a foreign embed. | The same helper. |
| `MethodSet`, `MethodSetKeep` | They read the forwarders from the generated part's own text and name each by the class that declares it. | None expected; GenTests decides. |
| `FieldAccessGenerator.HasMethod`, `TypeGenerator.GoMethodNames` | Read the package class for a type's DECLARED methods and walk embeds themselves. | None expected. |
| golib's extension scan | Reads every listed host; a host's receivers decide its homes. | None: the holder's receivers are its own assembly's types. |
| Converted call sites and method VALUES | Instance form (`x.M()`, `x.M`). | None. |
| Hand-owned files | Unknown: a static-form call of a promoted method would be CS0103 or CS0117. | The three-target stdlib build is the instrument. |

One helper, `GoMethodHosts(type)` (the package class, then the holder found by name in the same namespace), replaces
the six metadata reads. It reads an older assembly that has no holder exactly as today.

**Mixed versions.** A consumer built with the older generator against a newer assembly would not see holder
forwarders through its metadata reads. The generator and the packages are released together; the note states the
pairing, it does not solve a mixed one.

## 6. A run-time behaviour that changes, toward Go (measured)

`runtime.Callers` counts a frame as Go's only when its top-level declaring class is a `<pkg>_package`
(`classifyGoSourceFrame`). The runtime's own doctrine is that "go2cs-gen forwarders are dispatch plumbing Go has no
frame for", but a same-package forwarder sits in the package class and carries no generated-code mark on the method,
so today it IS counted. A probe (two value embeds, a pointer embed; `runtime.Caller` read inside the promoted method)
at `af91182836`, Release, against Go 1.26.10:

| Call | Go | Converted, today |
|---|---|---|
| `o.Val()`, value method through two value embeds | `main.direct` | `main.mid.Val` |
| `p.Val()`, `p.Ptr()` through a pointer embed | `main.direct` | `main.pouter.Val`, `main.(*pouter).Ptr` |
| the value method through an interface | `main.viaIface` | `main.mid.Val` |
| both through method values | `main.viaValue` | `main.mid.Val`, `main.(*mid).Ptr` |
| both through method expressions | `main.viaExpr` | `main.mid.Val`, `main.(*mid).Ptr` |

Four of the eleven probe lines match today. **Prediction:** with the forwarders in a holder (not a package class, and
marked generated at class level like `xpkg`), the six lines above read as Go's, because the frames stop counting.
One further line differs for another reason and is not predicted to move: the NAME of a pointer-method value reads
`main.viaValue.func1` where Go reads `main.(*inner).Ptr-fm`; that is a BOARD row, not this seat.
The falsifier: any of the six lines unchanged, or a behavioral Output that passes today and fails after.

A panic traceback moves the same way (inferred, not probed): where a forwarder is the first Go frame of the panic
site today (a nil receiver read in the `ж<T>` form), the site will start at the caller. For a direct call Go names
the caller or the method, never a wrapper; the dispatched cases (interface, method value) are the ones the traceback
code already refuses to model (`RaisedByWrapper`, `adapterBeforeFirstGoFrame`). The full runner's Output phase is the
instrument.

## 7. Can a type still exceed the limit, and the guard

- **A holder** holds two methods per promoted method of ONE struct. It exceeds the limit only for a struct with more
  than 32,762 promoted methods.
- **The package class** still holds every function and every declared method of the package, a `ж<T>` twin per
  pointer-receiver method, and every lowered local function. A package with tens of thousands of declared methods
  (generated SDK and protocol code is the likely shape; inferred, no such module was measured) still fails.
- **Three other classes grow with the package or the assembly**: the compiler's `<pkg>_package/<>c` (every
  non-capturing lambda), `GoFieldAccess` (1,835 methods in `runtime.dll`), and `<pkg>_package_asserts`.

**Recommended guard: GO2CS0011, an error at compile time, from a compilation-end analyzer in go2cs-gen** (the
assembly already ships two analyzers). For every named type of the compilation, generated parts included, it counts
the members that are certain to become MethodDef rows (methods, constructors, accessors, operators) and refuses
above 65,524, naming the type, its count and the limit. The count is a lower bound of the loader's (lowered local
functions and lambda methods are not symbols), so it refuses every certain failure, the measured 116,102 among them,
and never refuses a type that would load. A generate-time count inside TypeGenerator alone was rejected: a generator
sees neither the other generators' output nor its own through the compilation, so it could count holders (which
never exceed) and not the package class (which can).

The exact instrument is the metadata census of §9. A type that passes GO2CS0011 and fails to load can only be one
pushed over by compiler-lowered methods; none is measured, and the census names it if one appears.

## 8. Counts afterwards

- `ast_package`: 116,102 − 107,835 = about 8,267, if all 107,835 TypeGenerator methods are forwarders (COORD's
  figures; the TS session reads the exact number).
- The worst holder in `ast`: about 2 × the promoted method set of a node type, 107,835 / 214 ≈ 504 (inferred from
  the same figures; requested below).
- The standard library: `runtime_package` falls from 5,705 by its forwarder count (373 in the sizing census), and
  no package class grows.
- Type count: one class more per struct that has at least one same-package forwarder (187 in the sizing census;
  about 214 in `ast`, against its 59,209 types).

## 9. Red, gates and footprint

**No tracked file holds generator output** (`*.g.cs` is ignored), so the generator change has no committed
footprint and a two-seeded reconvert cannot see it. The footprint has two halves:

1. **Converter half (tracked).** Two-seeded, three targets. Predicted from the census of §5: 0 production
   standard-library files; `PromotedMethodExpr/main.cs` (two statements) in the behavioral corpus; the three
   `reflect` test sites appear only in the `-tests` emission.
2. **Generator half (untracked).** A metadata census, before and after, per target, over the stdlib build. The
   instrument is a 60-line reader over `System.Reflection.Metadata` that loads nothing and prints, per type, its
   MethodDef count and its `[GoPromoted]` count (built and run for §1; proposed home `src/utilities`). Invariants, per
   assembly: the MethodDef total is unchanged; the `[GoPromoted]` total is unchanged; each package class falls by
   exactly the methods its holders gain; the type count rises by the number of holders. The prediction is derived per
   target from the before-arm's census, not from the sizing numbers of §1.

**Red first.**
- The TS session's generated repro (N methods embedded in M types, 2NM over the limit, with an under-limit control)
  reds at LOAD in the Output phase and passes after. Its control must be under 65,524 in total, not 65,535.
- The probe of §6 as behavioral lines: six lines red today.
- The method-expression sites: `PromotedMethodExpr` keeps printing Go's output (a compile red in between if the
  generator half lands without the converter half, which is why they are one seat).
- GO2CS0011: a GenTests case with one type over the limit, and a control at the limit.

**Gates.** go2cs-stdlib.slnx on three targets; go2cs.slnx Debug; GenTests and GolibTests (the extension-host list
tests hold every generated host to its listing); check-no-regression; the full behavioral runner; `-tests`
convert-and-build of `reflect` (method expressions), `net` (the metadata embed harvest) and one adapter-heavy row;
one Native AOT publish that prints through a promoted `String` (the `DynamicDependency` on the holder); go test,
licensing, repoguard.

## 10. Not in this seat

- Lifting the rule that skips a same-package metadata forwarder whose name is also a package function
  (`GetMetadataPromotedMethods`). It exists because a forwarder minted INTO the class shadows a bare call of the
  function; a holder removes the cause, so the skipped methods (absent from the Go method set today) could be minted.
  A separate red.
- Moving a package's OWN methods out of the package class (§7's second bullet). Only when one is measured.
- The pointer-method value's name (§6).
- The total method count (256,275 in `ast.dll`): placement fixes loading, not size.

## 11. Asked of the TS session, through COORD

1. The ten largest types of the parse demo's assemblies by MethodDef count, to confirm what remains on
   `ast_package` and whether `<>c`, `GoFieldAccess` or `_asserts` is anywhere near the limit.
2. The largest per-struct forwarder count in `ast` (the longest `// Promoted Struct Receivers` section).
3. Whether the demo's source holds a method expression that selects a promoted method (the census of §5 run there).

## 12. What a reviewer should try to break

- §2's claim that a sibling class in the same namespace changes no binding, for the package's own code, an
  importer, a `-tests` white-box assembly and an external-test assembly.
- §5's table: a reader of forwarders that is not listed.
- §6: a test that passes today BECAUSE the forwarder frame is counted.
- §7: a certain MethodDef that the analyzer's count would include and the loader does not (a false refusal).

## Amendment 2026-11-10 — the seat as cut: the owner's ruling, review r2, and what was measured

> Appended, not rewritten: §§1-12 above stand as proposed and as reviewed. Where this block and the text above
> disagree, this block is the seat. The review record is `claude/coord-forwarder-review-r2` `88e650c17c`.

### A. The ruling: the per-struct class, uniformly, and the break is owned in 1.26.10.4

The review's finding 1 was that §2 moves PUBLIC static methods between classes, which is a binary break for every
assembly compiled against the old placement (`MissingMethodException` at the first call), and that the note did not
say so. The owner ruled the uniform shape and the break ("better breaking changes now than before we're deep with
users"). The compatible hybrid (old placement kept below a threshold) is not built. What owns the break:

- **Suppressions are generated, never written by hand.** `src/tools/pushnuget/New-GoApiCompatMoveSuppressions.ps1`
  reads each merged package and its published baseline as metadata and accepts a removed member only where a class
  marked `[GoPromotedHost(typeof(S))]` now declares it with the same name, signature and attributes for the same
  struct `S`. Nullable annotations are compared decoded, position by position, because the compiler writes
  `NullableContext` on a class by the value most of its members share, so a moved method's own attributes change
  while its annotations do not. Anything else removed is listed as REFUSED and the script exits 1. Its suite is
  `Test-GoApiCompatMoves.ps1`. This release: 900 entries in 20 files.
- **`<GoBreakFloor>` moves to 1.26.10.4** (`src/core/golib/buildTransitive/go.lib.targets`), so a package built
  against an earlier go.lib fails its consumer's build as GO2CS3002 instead of failing at run time.
- **The release notes** carry the break under "Upgrading from 1.26.10.3" in the held 1.26.10.4 section of
  `docs/NEWS.md`.
- **The verdict instrument** is `push-nuget.ps1 -VerifyOnly` plus the pack-only rehearsal
  `push-nuget.ps1 -VersionSuffix local.1 -OutDir <scratch>`; only the second runs the guard.
- The rebuild of the published nugetgo packages is the owner's, after the release.

### B. §1 and §7 corrected: the limit is a SLOT law, and the guard is GO2CS0012

§1 read the limit as 65,524 MethodDef rows from the source constant. Measured on .NET 10.0.14 (linux-x64) by emitting
types with `PersistedAssemblyBuilder`, loading each in its own `AssemblyLoadContext` and bisecting:

| Shape | Most methods that load | One more |
|---|---|---|
| static class | 65,521 | fails |
| static class with a static constructor (counted) | 65,521 | fails |
| struct | 65,521 | fails |
| class deriving from `object` (its constructor counted) | 65,521 | fails |
| class whose base adds 1 virtual method | 65,520 | fails |
| class whose base adds 7 virtual methods | 65,514 | fails |
| class overriding 1 of `object`'s virtual methods | 65,522 | fails |
| class overriding 3 of them | 65,524 | fails |
| class overriding all 4 of them | 65,524 (the slot law alone allows 65,525) | fails |
| interface | 65,524 | fails |

**The law has two clauses: inherited virtual slots + (method definitions − overrides) ≤ 65,525** (`MAX_SLOT_INDEX`),
**and method definitions ≤ 65,524 on any type.** The second shows only where no inherited slot is left unoverridden:
the last two rows. `object` brings
four slots, so a static class, a struct or a class deriving from `object` loads with at most 65,521 methods. The TS
session's repro brackets the same edge from the converted side: 65,505 loads, 65,523 fails
(`claude/i7-ts-readiness-rerun` `ffcbd57694`).

The guard is **GO2CS0012** (GO2CS0011 was taken), a compilation-end analyzer (`MethodLimitAnalyzer`). Its threshold
is DERIVED per type, by COORD's ruling: 65,525 less the base chain's virtual slots, plus the type's own overrides,
and never above 65,524. It
does not count a partial method that has no implementation, nor a struct's implicit constructor. GenTests pins both
sides of the measured edge by LOADING the compiled fixture (65,521 loads, 65,522 does not), the repro's two readings,
a struct, a base type with virtual methods, a class that overrides all four, and an interface.

### C. §2 and §5 corrected: a struct's class is found by an attribute, not by its name

Review finding 6. The class carries `[GoPromotedHost(typeof(S))]` (golib, `GoPromotedHostAttribute`), and every
metadata reader finds a struct's class by that attribute (`GoMethodHosts.PromotedHost`). The name
`{pkg}ᴛ{Struct}ᴛpkg` is for a reader of the generated file and binds nothing. The class is `public` when its struct
is and `internal` otherwise, the struct's own scope.

### D. §9 corrected: the generator half is a SET diff, not count invariants

Review finding 3. §9's four count invariants could all hold while one method changed its signature. The instrument
is a Roslyn metadata dump of every type and method (static or instance, accessibility, ref kind, return type, name,
parameters, attribute names, constraints), per target, at base and at the fix. Each method of a promoted-method class
is read as a method of its struct's package class; the two multisets must then be EQUAL, and the only new types may
be the promoted-method classes. Result: on windows, linux and darwin, base `ae69e4ce74` against the fix
on the same base: 238,075 / 237,652 / 238,672 methods at base and two more at the fix, the new attribute's own
constructor and getter; 2,618 / 2,636 / 2,628 methods read back from the promoted-method classes; 0 only at base, 0
only at the fix, 0 moved methods without a base counterpart, 0 types gone or changed.

The first run read DIFFERS, for two reasons, and the second is a defect this note did not foresee:

1. The compiler names a lambda by the ORDINAL of its containing method within its class (`<M>b__423_0`), and the
   ordinal moved in every package class that lost forwarders (381 / 396 / 400 rows). No source names one. The
   instrument now reads the ordinal as N and keeps the index that counts within the method.
2. Four classes on each target held no method, only the comments for a struct that declares every method its embed
   would promote: crypto/ecdsa `PrivateKey`, image/color `NYCbCrA`, testing/quick `CheckEqualError` and x/crypto
   poly1305 `MAC`. An empty public class is a type nothing can bind and a break to take away later. The generator now
   declares no class for a struct that forwards nothing and writes its comments after the package class (GenTests
   `AStructThatForwardsNothingGetsNoClass`, which fails when the rule is taken out).

The new types are otherwise the promoted-method classes with at least one method (187 / 188 / 188), the attribute,
and 66 compiler-generated `<>O` classes nested in them: the cache of method-group delegates that the package class's
own `<>O` held at base.

### E. What a C# consumer sees

Review finding 4. `using go.pkg;` (the namespace) brings every method, promoted ones included, as before.
`using static go.pkg_package;` ALONE no longer brings a promoted method: it brings the package class's members and
nothing of a sibling class. `docs/ConsumingGoFromCSharp.md` says so, and GenTests `PromotedHostClassTests` pins both
(`UsingStaticAloneBringsDeclaredMethodsAndNoPromotedOne` expects CS1929).

### F. §6 scored: four of six fixed, two MISSED

§6 predicted that six probe lines would read as Go's once the forwarder frames stopped counting.

| Line | Predicted | Measured at the fix |
|---|---|---|
| `o.Val()` through two value embeds | Go's | Go's |
| `p.Val()` through a pointer embed | Go's | Go's |
| `p.Ptr()` through a pointer embed | Go's | Go's |
| both through method expressions | Go's | Go's |
| the value method through an interface | Go's | **MISSED**: `main.global::go.main_package.valer.Val` |
| both through method values | Go's | **MISSED**: `ok == false` and `main.viaValue.func1` |

Both misses are frames the forwarder frame used to stand in front of: a struct's generated explicit interface
implementation, and a method value's lambda. Neither is a forwarder, and both are measured at base behind the
forwarder's name. They are BOARD rows (2026-11-10, P1), with their roots; the behavioral test `PromotedMethodFrames`
prints the lines this seat fixes and none of those.

### G. Results

Trees: base is master `ae69e4ce74` for the rehearsals and the set diff, and master `38caf3558a` (batch 282, onto
which the seat was rebased) for the footprint and the gates. Batch 282 changes no file under `src/gen`.

- **Red first.** At the red commit GenTests does not compile (CS0103, `MethodLimitAnalyzer`), the converter test
  `TestMethodExpressionWrapperMark` fails on the promoted arm, and `PromotedMethodFrames` differs from Go 1.26.10 on
  7 of its 12 lines. At the fix: GenTests 489 of 489, the converter test passes, 0 of 12 lines differ.
- **The repro** (the generator committed on `claude/i7-ts-readiness-rerun` at `25df29a2b3`), each arm predicted
  before it was read: base `250 130 16` (65,521 methods) loads and matches Go; base `250 130 17` (65,522) builds and
  throws `TypeLoadException` before `main`; the fix at `250 130 17` and at `250 131` loads and matches Go.
- **Rehearsals** (`push-nuget.ps1 -VersionSuffix local.1 -OutDir <scratch>`). Base: `Binary compatibility: CLEAN
  -- 525 asset(s) of 362 package(s) compared with the revisions before 1.26.10.4 by apicompat 10.0.403`, floor
  1.26.10.3. The fix without suppressions: three builds, 0 errors; `Binary compatibility: 900 failure(s) across 362
  package(s)`, every one CP0002, in 20 packages.
- **The suppression generator** over those 362 packages: 900 entries, 20 files, 0 refused; 13 of the 20 packages can
  be imported by a program, 6 are internal and 1 is vendored. Two defects it showed on real packages and not on its
  first fixture now have fixture cases, each red before its fix: nullable context compared as written (6 of 32
  checks failed), and a namespace with a keyword segment such as `go.@internal.abi` (5 of 32).
- **The converter half's stdlib footprint** (two-seeded `-stdlib` reconvert, master's converter against the seat's,
  each emitting 1,949 / 2,023 / 2,022 files for windows / linux / darwin): 0 files differ. Predicted 0.
- **The behavioral corpus** (`check-no-regression.ps1`): `PromotedMethodExpr` alone moves, to the lambda form, and
  gains a line for `(*outer).Get`, a value method taken off the pointer type, which no test covered.
- **Review finding 5** (hand-owned code that names a forwarder in its static form): the three-target compile is the
  census, and it reads 0 errors.
- **Reach** (not gating): the nine go2cs packages published on nuget.org for this line hold 25 assemblies, which
  make 1,489 references to members of the standard library's package classes. None names a moved forwarder.

### H. The gates at the tip, what they found, and C34

Gated tree: `02d843fe97` on master `b9a6d261fc` (batch 283). This section is the one commit above it and changes
documents only. The first nine commits were gated on master `38caf3558a` (batch 282), then rebased to `e89725a9da`;
the one conflict was in the BOARD, where both sides had appended rows, and both are kept. The ten commits above
`e89725a9da` are fixes for what the gates then read, each with its red first: two in golib for readers the move
changed, with `c1001afba1`, which re-cites nine provenance notes the second fix shifted in `Goroutine.cs` (the
repository guard read them); and two for what `reflect`'s own tests read, one in the hand-owned runtime and one in
the converter, with the two goldens they move. Each gate names the commit it was read at. Two counts in the commit
messages were read on the earlier base, where GenTests had 489 tests before C34 and 492 with it; the rebased tree
has 493, the one more being master's.

**What the first gate pass found.** Three defects of the seat that none of its own tests had read. Each was fixed in
the commit it belongs to, before the branch was pushed.

- `go2cs.slnx` (Debug, windows) read CS0246 twice in behavioral `GenericEmbedPromotion`. A forwarder names its
  embed's type argument, and there the argument is a constraint proxy, which another go2cs-gen generator declares
  inside the package class. Inside the package class the bare name bound; in the struct's own class it did not. The
  class's file now imports the package class (`using static`), and GenTests
  `AForwarderThatNamesAGeneratedTypeBindsItThroughThePackageClass` compiles the case (CS0246 twice without the
  import).
- GolibTests `EveryCopyBoundReceiverInALoadedPackageIsAllowlisted` read 126 package classes and matched 0
  allowlisted receivers: it looked for copy-bound forwarders in the package classes. It now reads each struct's
  class through the attribute, and requires at least 20 package classes and 20 promoted-method classes read.
- The set diff of section D found a class with no member for 4 structs on each target (`crypto/ecdsa`'s
  `PrivateKey`, `image/color`'s `NYCbCrA`, `testing/quick`'s `CheckEqualError`, the vendored poly1305 `MAC`). Each
  declares every method its embed would promote. A struct that forwards nothing now gets no class (GenTests
  `AStructThatForwardsNothingGetsNoClass`, made to fail once), and the dump of the tip reads 0 promoted-method
  classes with no method on each target.

**Readers of a forwarder's CLASS that §5 and §6 did not list.** Four, three in golib and one in the hand-owned
runtime, all fixed in the seat with a red first. §5's table lists golib's extension scan as needing no change. That
holds for FINDING a forwarder; what a reader then takes from the found method's class was not in the table, which is
the miss §12 asked a reviewer to look for ("a reader of forwarders that is not listed"). §6's second falsifier FIRED
("a behavioral Output that passes today and fails after"), and is scored as that: the prediction that only frame
counts change was wrong.

| Reader | Takes from the class | Found by | Fix |
|---|---|---|---|
| `UnexportedMethodPackageMatches` (`runtime/TypeExtensions.GoMethodSets.cs`) | the package of a candidate for an interface's unexported method | the behavioral runner at `e89725a9da`: 931 of 932 | the class's `[GoPromotedHost]` struct's package class (`c022d3f86b`) |
| `RuntimePanicCheck.IsRuntimePackageType` | whether a bounds, slice, slice-to-array or shift panic was raised in package `runtime`, where Go throws | COORD's second read of `c022d3f86b` | the name's form, `go.runtimeᴛ<Struct>ᴛpkg` and the bridge's (`049f33f2af`) |
| `Goroutine.IsSystemCreator` | whether a goroutine's start function or creator is package `runtime`'s | the same read | the name's form, `go.runtimeᴛ<Struct>ᴛpkg` (`049f33f2af`) |
| `goFrameName` (`src/core/runtime/managed_impl.cs`) | the Go name of a func VALUE over a forwarder (`runtime.FuncForPC`) | behavioral `PromotedMethodFrames`, once it read the name of a method table's func | the class's `[GoPromotedHost]` struct's package class (`cf841d2287`) |

- The first is behavior every program can reach. `UnexportedIfaceDynamicAssert` printed 3 of Go's 5 lines
  differently at `e89725a9da`: it is `net`'s writev shape, `w.(buffersWriter)` over a `*TCPConn` that promotes the
  unexported `writeBuffers` from its embedded `conn`, and the assertion missed, so the converted program took the
  per-chunk fallback. The same project matches Go at `ae69e4ce74`, before the move. Red: GolibTests
  `GoUnexportedMethodPackageTests` gained the promoted case (1 failed of 5 at `b7b1a33ee3`) and a control (a foreign
  struct's promoted unexported method still does not satisfy another package's marker). At `c022d3f86b`, 5 of 5 and
  the project prints Go's 5 lines. The runner is the gate that read it; it had not completed on this branch before.
- The second and third are reached where a forwarder's frame stands for its promoted method, which a .NET trace
  shows when the JIT inlines the method into the forwarder, and where a `go` statement starts a promoted method. The
  converted runtime has no such `go` statement today. Both readers work from names (a trimmed or Native AOT image
  keeps a frame's names, not a class's attributes), so COORD ruled the name's form for them, the one place in the
  seat where the class is not found by its attribute (section C). One predicate reads it,
  `RuntimePanicCheck.IsPromotedMethodClassOf`, and each reader accepts the twin of what it accepted before: the
  first reads the internal-test bridge as `runtime`, the second never did. Red: `RuntimePanicCheckTests` and
  `SystemGoroutineTests` gained a case each and a control each (2 failed of 13 at `36c06e036c`); at `049f33f2af` the
  three fixtures' 18 tests pass, and the controls were made to fail once (the package stem left out of the form).
- The census made for these fixes read every use of a method's or a frame's declaring class in golib and in the
  hand-owned files. It did not list the fourth reader: the runtime's name derivation takes a package from a
  `_package` class and spells any other class as it stands, and the census read that as a frame's rule, which the
  move does not change, without asking what names a func VALUE over a forwarder. `reflect`'s tests led to it
  (below). One reader is left, as a BOARD row (2026-11-10, "a reflect panic text names its method by class name"):
  `reflect`'s `methodName`, unchanged in the ordinary case and not measured where a forwarder stands for an inlined
  method. A cross-package forwarder class (`…ᴛxpkg`) was never in the package class; nothing changes for it.

**What `reflect`'s own tests found: the method expression of §5 was the wrong form.** §5 planned the lambda for a
method expression that selects a promoted method, and `e82b7bc2ac` emitted it. §9 listed `reflect`'s `-tests` row
as convert-and-build; RUN at `c1001afba1` it failed one test the row passes on master, `TestEmbeddedMethods`
("Wrong method table for OuterInt"); every other failure there is one `docs/validation/current/reflect.md` discloses.

- In Go a method expression is ONE function, and it is the function the type's method table holds:
  `reflect.TypeOf(T).Method(i).Func.Pointer()` equals the expression's pc. `reflect` here tokens a func by the method
  its delegate invokes, and go2cs-gen's method set hands it the method group over the forwarder. Through 1.26.10.3
  the converter's bare group named the same forwarder. A lambda is another method, at every site.
- The fix (`6fe4982004`): the converter names the group over the struct's promoted-method class,
  `((Func<ж<outer>, nint, nint>)(mainᴛouterᴛpkg.Add))`, wherever a forwarder takes the expression's receiver as the
  delegate passes it. Two shapes keep the `[GoWrapper]` lambda because no forwarder has the receiver: a value
  method off the pointer type (`(*outer).Get`, a lambda on master too), and a pointer method off a value type that
  reaches it through a pointer embed (`outerp.Add`), which a bare group did not compile for before the seat (read
  from the overload set, not run there).
- With the group in place a second defect of the move showed, the fourth reader in the table above:
  `runtime.FuncForPC(pc).Name()` of such a func read `mainᴛouterᴛpkg.Val`, where Go reads `main.outer.Val` and
  `main.(*outer).Ptr`. At `c1001afba1` the func a method table holds already read so, and no gate read that name.
  The fix (`cf841d2287`) names an UNBOUND func over a forwarder as Go names its wrapper, for the embedding struct in
  its package at `<autogenerated>:1`; a forwarder's FRAME is still not a frame of the program (§6). It is ordered
  before the converter's fix, so no commit of the branch prints a name the one before it did not.
- Reds: `TestMethodExpressionWrapperMark` gained three rows (2 fail at `84693710f6`); `PromotedMethodExpr` gained
  six lines that ask "is the expression the method table's func" of five shapes and a control (at `84693710f6`, 9
  answers on 5 lines read `false` and the control agrees with Go); `PromotedMethodFrames` gained the line "method
  table name" (wrong at `f84ffae964`). At `02d843fe97` the two print Go's 16 and 13 lines.
- What is left is a BOARD row (2026-11-11, "a method expression rendered as a lambda is not the function the method
  table holds"): the lambda shapes, and a pointer method through a pointer embed, whose method-set arm is a lambda
  of its own. Each read the same before the seat.
- The lesson for the plan of a seat like this one: a row listed as convert-and-build is RUN when the seat changes
  what the row's tests compare.

**C34, folded in as its own two commits** (`5cd07c51c3` the tests, `65d08a5c3c` the fix; COORD, 2026-11-10 14:53Z).
The forwarder of an EXPORTED promoted method is public unless its signature names a type that is not public, on
every embed kind; the rule and its generic-embed guard are in `docs/ConversionStrategies-Reference/interfaces.md`
(the exported-method addendum). Red before the fix: 2 of `PromotedForwarderScopeTests`' 3 tests and one existing
pin, and CS1929 at 3 calls of behavioral `PromotedValueEmbedUser`. At the fix: every GenTests test passes, the
pair's 3 lines match Go, and the repro committed on `claude/i7-ts-readiness-rerun` at `4505b95021` builds with 0
errors and prints Go's `./index.js`, as its control does.

Its footprint, stated apart from the move's. The instrument is a direct set diff of every type and method of the
packed assemblies of two rehearsals: the seat without C34 (section G's second rehearsal, on master `ae69e4ce74`) and
the nine commits on master `38caf3558a`. It was made to fail once on three planted changes of accessibility and one
dropped row, and named them. It was not repeated after the rebase, where batch 283's own changes would be in the
diff; the dump of the rebased tip is read by a census instead, below.

- **Predicted** before the measurement, from the first rehearsal's assemblies: 268 forwarder methods in 12 packages
  go from internal to public, the same on each target, and nothing else moves. Falsifier named with it: a struct
  that embeds one of these structs from another package may mint forwarders it could not read before.
- **Measured**, windows, linux and darwin alike: 268 forwarder methods in 12 packages (42 classes). MET.
  `internal/abi` 112, `debug/dwarf` 52, `bufio` 40, `debug/macho` 18, `net/textproto` 16, `debug/gosym` 8,
  `text/template/parse` 6, `image` 4, `text/scanner` 4, the vendored `dnsmessage` 4, `crypto/rsa` 2, `runtime` 2.
- **"Nothing else moves": MISSED, by the named falsifier.** `reflect`'s five unexported structs that embed an
  `internal/abi` struct (`interfaceType`, `ptrType`, `sliceType`, `structType`, `structTypeUncommon`) each gain
  forwarders for seven methods of `abi.Type`: `Align`, `FieldAlign`, `GcSlice`, `HasName`, `IsDirectIface`,
  `Pointers`, `Size`. That is 70 internal methods in their cross-package classes on each target, and the seven
  methods in each struct's generated method set. `internal/abi`'s own forwarders for the seven were internal for
  their return types' spelling (`uintptr`, `bool`, `nint`, `slice<byte>`), so `reflect`'s generator could not read
  them. Go has all seven on each of the five structs. No other assembly gains a cross-package forwarder (468 to
  538 in all, the 70 in `reflect`).
- **At the rebased tip**, the census of the packed assemblies reads 187 / 188 / 188 promoted-method classes for
  windows / linux / darwin, none with no method, and 0 internal forwarders of an exported method in a public
  struct's class on each target.
- **The rest of that diff is not C34's**: batch 282's six files under `src/core` (golib's cgo argument helpers on
  every target; `internal/syscall/unix`, `runtime` and `syscall` on linux), and the 4 classes with no member that
  the rule above took away.
- **Goldens**: the `PromotedValueEmbed` pair, whose Go sources gained the shape, and nothing else.
  `check-no-regression.ps1` at `e89725a9da` reads all 1,012 behavioral packages byte-identical with the pair
  re-minted. C34 changes no converter file, so it has no standard-library conversion footprint of its own.
- **Binary compatibility**: C34 removes no public member, and the verdict below is read with the same 900 entries.

**The verdict at `e89725a9da`** (`push-nuget.ps1 -VersionSuffix local.1 -OutDir <scratch>`; three builds, 0 errors),
verbatim:

    ==> Binary compatibility: CLEAN -- 525 asset(s) of 362 package(s) compared with the revisions before 1.26.10.4 by apicompat 10.0.403; 0 package(s) with no published version on this line (reported, not gated)
    ==> Break floor: CLEAN -- <GoBreakFloor> 1.26.10.4; this release breaks go.archive.zip, go.bufio, go.debug.dwarf, go.debug.macho, go.encoding.binary, go.encoding.gob, go.go.types, go.image, go.net, go.net.textproto, go.reflect, go.runtime, go.text.template.parse

It was not repeated at `02d843fe97`. The ten commits above `e89725a9da` change no public member: private and
internal members of three golib files, private members of `runtime`'s hand-owned `managed_impl.cs`, one converter
file, tests and goldens. `push-nuget.ps1 -VerifyOnly` at `02d843fe97` reads `20 suppression file(s), 900 announced
break(s)` and `<GoBreakFloor> 1.26.10.4 is on the line of the next release, 1.26.10.4, and not above it`.

**Gates, each read at `02d843fe97`.** `check-no-regression.ps1`: 1,012 behavioral packages byte-identical. GenTests
493 of 493. `go2cs.slnx` Debug at `-p:GoTargetOS=windows`: 0 errors. GolibTests 2,584: Debug 2,530 passed and 54
skipped, Release 2,555 passed and 29 skipped, 0 failed. `Test-GoApiCompat.ps1` ran 99, failed 0;
`Test-GoApiCompatMoves.ps1` ran 33, failed 0. The behavioral runner (`run-behavioral.ps1`, five slices): 932
projects, all pass (its Output phase: 903 pass, 29 skipped, 0 fail), where `e89725a9da` read 931.

- **Go's own tests** (`-tests`, action `all`, Release, linux, in a scratch worktree), each row rc 0: `reflect` 445
  validated with 21 disclosed, its banked counts, where `c1001afba1` failed on `TestEmbeddedMethods`;
  `encoding/binary` 143, `archive/zip` 101, `bufio` 81, `debug/dwarf` 41, `net/textproto` 27, the counts
  `c1001afba1` read; `net`'s tests convert and build with 0 errors.
- **Method expressions written in test files** (a probe module through `-tests -recurse`; not committed). The
  converter names a struct's class differently for each place a test can declare or reach the struct, so each was
  read: an internal test file over the production struct (`global::go.wbprobeᴛOuterᴛpkg.Add`) and over a struct of
  its own (`wbprobe_internal_testᴛTestOuterᴛpkg.Add`), an external test file over the production struct and over a
  struct of its own (`wbprobe_testᴛExtOuterᴛpkg.Add`). Its 4 tests compare each expression with the method table's
  func and validate against `go test`.
- **The hand-owned runtime file** of `cf841d2287` compiles in `runtime` for windows, linux and darwin, 0 errors.
- **Native AOT** (linux-x64, the template's partial trim, one native image). A program whose promoted methods are
  reached only at run time (through `fmt`, two type assertions and two method expressions) prints Go's 5 lines.
  `PromotedMethodFrames` prints Go's 13 lines, among them the two that name an unbound forwarder, which the
  runtime reads from the class's attribute. Not measured: a full trim. `PublishAot` goes into the project file
  AFTER the conversion, which writes that file: the first reading of this gate set it before, published the
  template's ReadyToRun image, and was read as a pass from the program's output alone.
- **The standard-library conversion footprint** of the ten commits is not re-measured here. The one converter
  change is inside the arm for a same-package promoted method's expression, and section G's two-seeded reconvert
  read 0 files for that arm's first change on each target, so no production file enters it. The measurement at
  the tip follows this section as a dated block.
- `go test ./...` is read with this section in place (the repository guard reads its cites) and is in the seat's
  DONE post.

## Amendment 2026-11-11 — the seat merged with batch 284: gates, Native AOT and the conversion footprint

Tip: `c05dd26ae1`, one merge commit whose parents are the seat's `2ad04c9983` (section H's commit) and master
`50dd9a065c` (batch 284). This block is the one commit above it and changes this document only. It is the dated
block that section H says follows it. Batch 285 was frozen with the seat at `c05dd26ae1` before this block was
written, so the block rides a later batch.

### I. The merge

The coordinator ordered a merge of master, not a rebase, into every in-flight branch when batch 284 landed. Three
files conflicted, all of them documents.

- **The BOARD.** Both sides had appended rows at its end. Both are kept, master's five rows first and the seat's
  four after them. The merged file has 36,071 lines: master's 35,917 and the seat's 154.
- **Two reference pages** (`source-generators.md` and `struct-embedding.md`). Each holds one provenance note that
  both sides had re-cited, for different shifts of the file it cites. Each is re-cited at the line that holds its
  quote in the merged file, read there and taken from neither side. The repository guard named both cites as stale
  before that and passes after it.

The files that merged without a conflict were read as well. `ImplementGenerator.cs` takes hunks from both sides;
master's (constraint proxies of a non-generic interface, pointer adapters for an open generic foreign struct) read
no member of a package class, so none of them needs a struct's class. A census of the lines batch 284 adds under
`src/gen`, `src/core/golib` and `src/go2cs` found no new reader of a forwarder's class. `go2cs.slnx` and the four
behavioral test lists hold each side's additions once.

### J. Gates, each read at `c05dd26ae1`

- GenTests: 493 of 493. `go test ./...` in `src/go2cs`: 11 packages ok, the repository guard among them.
- `go2cs.slnx` Debug at `-p:GoTargetOS=windows`: 0 errors.
- GolibTests, 2,616 tests: Debug 2,555 passed and 61 skipped, Release 2,580 passed and 36 skipped, 0 failed.
- `Test-GoApiCompat.ps1` ran 99, failed 0; `Test-GoApiCompatMoves.ps1` ran 33, failed 0. `push-nuget.ps1
  -VerifyOnly`: `20 suppression file(s), 900 announced break(s)`, the floor 1.26.10.4 on the line of the next
  release and not above it. The pack rehearsal was not repeated on the merged tree, by the coordinator's ruling:
  the batch's own gate runs the release check.
- The behavioral runner, five slices: 940 projects, all pass (its Output phase: 911 pass, 29 skipped, 0 fail).
  Section H read 932; the eight more are master's.
- Go's own tests (`-tests`, action `all`, Release, linux, a scratch worktree at the tip), each row rc 0: `reflect`
  445 validated with 21 disclosed, its banked counts; `encoding/binary` 143, `archive/zip` 101, `bufio` 81,
  `debug/dwarf` 41, `net/textproto` 27, the counts section H read; `net`'s tests convert and build with 0 errors.
- **`check-no-regression.ps1` exits 1, on one file that is not the seat's.** It transpiles 1,036 packages and one
  file changes: `CgoPseudoPackageTypes/.editorconfig`, where `# CS0219: windows` becomes `# CS0219: linux
  windows`. The control: master `50dd9a065c`'s converter and the seat's each converted a copy of that project on
  this linux host, both emit that line, and the two outputs are identical in every file. The golden is batch
  284's and was captured on windows; the line depends on the host that converts. The coordinator routed it to the
  cgo lane. The runner's own transpile moves the same file and no other. Not measured: a whole
  `check-no-regression.ps1` at master on linux.
- Not repeated on the merged tree, and read in section H at `02d843fe97`: the probe of method expressions
  written in test files, and the compile of `runtime` for three targets.

### K. Native AOT, at `c05dd26ae1`

Both programs of section H, published as section H describes (`PublishAot` added to the project file after the
conversion, `-r linux-x64`), each to one native image with no assembly beside it: 159 MB and 162 MB. The program
whose promoted methods are reached only at run time prints Go's 5 lines. `PromotedMethodFrames` prints Go's 13
lines; the two that name an unbound forwarder read as Go prints them:

    method expr name: main.outer.Val main.(*outer).Ptr
    method table name: main.outer.Val main.(*outer).Ptr

### L. The standard-library conversion footprint, at `c05dd26ae1`

**Prediction, stated before the runs: 0 files on every target.** The seat's converter differs from master
`50dd9a065c`'s in three files (167 lines added, 3 removed), one of them a test; both emitting changes apply only
to the method expression of a same-package promoted method, and section G read 0 files for that arm's first
change.

**Instrument.** Two seeded `-stdlib -comments` reconverts diffed against each other (`diff -rq`, build output
excluded): arm A is master `50dd9a065c`'s converter, arm B the tip's. Each seed is the tip's tracked `src/core`
and `src/version.props` (4,662 `.cs` files, counted against `git ls-files` before each run). The runs are ONE
TARGET EACH, four pairs. A run that names all four targets was killed by the kernel for memory on the 15 GB
machine this was measured on (13.9 GB resident), and the converter at master refuses a multi-target run that
names fewer than the four declared targets. So each pair measures the change on one target's own file set, in
that target's single-target layout; a four-target run in one process is not measured.

| Target | Packages converted, each arm | Files that differ |
|---|---|---|
| linux/arm64 | 356 | 0 |
| windows/amd64 | 360 | 0 |
| linux/amd64 | 358 | 0 |
| darwin/amd64 | 358 | 0 |

**Read: 0 files differ on each of the four targets, as predicted.** Every run exits 0 and converts all of its
packages. Neither arm's log holds an unresolved lift marker.

**The instrument was made to fail.** A third converter, the tip's with one space added inside the cast that the
plain method-expression arm emits, converted a third seed for linux/arm64. Against arm B, 15 files differ, each a
file that holds a plain method expression: `compress/flate/deflate.cs` and `inflate.cs`, `compress/lzw/reader.cs`
and `writer.cs`, `internal/trace/order.cs` and `reader.cs`, `internal/zstd/block.cs`, `log/slog/value.cs`,
`net/http/httputil/persist.cs`, `net/linux/fd_posix.cs`, `os/linux/exec.cs`, `file_unix.cs` and `root_unix.cs`,
`regexp/syntax/regexp.cs`, and `runtime/time.cs`. For example, in `deflate.cs`:

    <         d.fill = ((Func<ж<compressor>, slice<byte>, nint>)(fillStore));
    >         d.fill = ((Func<ж<compressor>, slice<byte>, nint>)( fillStore));

The plant is in the arm beside the seat's, not in the seat's arm: the readings above say no standard-library file
reaches the seat's arm, so a plant there would also read 0 files. What the control shows is that this instrument
reports a converter whose emission differs. It was run for one target.
