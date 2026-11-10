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
