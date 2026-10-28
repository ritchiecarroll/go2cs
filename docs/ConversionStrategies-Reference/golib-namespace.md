# The `go.golib` support namespace
<!-- {% raw %} — Jekyll/Liquid guard: this page contains Go composite-literal and template syntax ({{ … }}) that Liquid would otherwise parse; the HTML comment hides the tag on GitHub. -->

[Reference index](README.md) · [Summary of this topic](../ConversionStrategies.md#the-gogolib-support-namespace)

golib's hand-written support types (`SparseArray<T>`, `PinnedBuffer`, `TypeExtensions`, `HashCode`, `FatalError`, …) live in the **`go.golib`** child namespace — deliberately NOT `go.<any Go package name>`. A name such as `go.runtime` would collide with the real `runtime` package: converted code imports runtime as `using runtime = runtime_package;` inside `namespace go`, and a child namespace `go.runtime` visible from any referenced assembly (golib is referenced by *every* project) wins simple-name lookup over the alias — CS0576 at every `runtime.X` use.<!-- the namespace was originally go.runtime; iter/internal/weak surfaced the collision in wave 1 --> The same reasoning forbids `go.internal`, `go.sync`, etc.; `golib` is not a Go stdlib package name, so the child namespace can never collide with an import alias. Emitted code references these types via the child namespace (`new golib.SparseArray<T>{…}`), which resolves inside `namespace go` with no using directive.

The general form of this collision — a REAL parent/child package pair — is handled by **Δ-renaming the import alias**. A C# using alias declared inside a namespace conflicts with a same-named child namespace visible from ANY transitively referenced assembly (CS0576 at every use), and transitivity makes this common: `runtime.csproj` itself references `runtime/internal/math|sys` (namespace `go.runtime.@internal`), so *every* package importing `runtime` sees a `go.runtime` child namespace — `iter` and `internal/weak` have it (`weak`, in namespace `go.@internal`, collides with `go.@internal.runtime` from `internal/runtime/*` instead). A pre-pass computes the package's transitive Go import closure (exactly mirroring MSBuild's transitive ProjectReference visibility), derives every child-namespace chain it contributes, and Δ-renames any import alias the current package's namespace would capture: `using Δruntime = runtime_package;` with uses `Δruntime.Goexit()` — the established collision marker. The rename propagates through one lookup to the using emission, package-qualifier identifiers, and cross-package type-name prefixes; a package with no collision emits byte-identically. (The behavioral corpus sees this on `io` — the real Go closure contains `os → io/fs`, hence `go.io` — captured in the `AnonymousInterfaces` golden as `Δio`.)

Three properties of the rename, established empirically:<!-- 2026-07-16 review of the Δmath emissions; ruled working-as-designed --> **(1) The trigger is per-file and, for a top-level parent package, fires only in packages emitting into `namespace go`.** The collision key is `<packageNS>.<alias>`, and usings are per-file — so `math` (whose own closure always contains `math/bits`, hence `go.math`) renders as `Δmath` in exactly the `namespace go` importers' math-importing files (fmt's scan, reflect's value, expvar, …), while every nested-namespace importer (`go.compress`, `go.crypto`, …) keeps the clean `using math = math_package;`: an alias declared inside the file-scoped nested namespace wins simple-name lookup before the outer `go.math` is consulted. That inner-scope exemption is why the clean form dominates the corpus. **(2) The rename is required wherever the closure is visible.** Every consumption has transitive reference visibility — `src/core` itself, NuGet packages, `-recurse` apps — and there the clean alias is CS0576 at every use, and hoisting it to compilation-unit scope merely trades that for CS0234 (inside `namespace go` the child namespace shadows the alias). Output must be context-independent, so the conservative rename stands.<!-- When this was written the clean alias still compiled against the hand-owned stub baseline, whose csprojs omitted the Go closure (core/math referenced just golib); that baseline retired 2026-08-01 (commit 2e8066da6). --> **(3) The marker cannot be swapped onto "the colliding item".** That item is the child namespace itself — the import-path-mirroring namespace of `math/bits` et al., baked into separately-compiled referenced assemblies — so there is nothing local to rename, and renaming the namespace would break the path-mirroring invariant corpus-wide (the mechanism covers `Δruntime`, `Δsync`, `Δio`, `Δsyscall`, `Δunicode`, …)<!-- ×48, ×38, ×23 files at the time -->. The `MathFloatBits` and `GoNamespaceShadow` goldens pin the `Δmath` form.

## Foreign renamed types reference the recorded imported-type alias
A cross-package type that is renamed (or Go-aliased) inside its own package -- `syscall` declares `ΔHandle` for its type-vs-method-colliding `Handle` -- must be referenced through the recorded imported-type alias (`global using syscallꓸHandle = go.syscall_package.ΔHandle`): the raw qualified render (`Δsyscall.Handle`) names a type that does not exist (CS0426 x26, internal/poll). The substitution lives at the C#-NAME layers -- `getCSharpTypeName` (delegate elements, parameters, results) and `getScopeCheckedTypeName` (named struct fields) -- and deliberately NOT in `getAliasQualifiedTypeName`: the Go-shaped name layer also feeds promoted-embed MEMBER naming, where the substitution renamed and rescoped the generated accessors (reflect CS8799 regression on the first cut). The GoImplicitConv assembly attributes record type names under the file-local import qualifier, so the resolving `using` in package_info.cs declares that same qualifier (`using Δsyscall = go.syscall_package;`).

A **pointer/box (or other composite) element** — `*time.Location` as a func result (archive/zip's `timeZone`), a `*syscall.Handle` parameter, a slice/map element — is renamed too, but by a *different route* that does not need `getAliasQualifiedTypeName` (so the CS8799 landmine is untouched): `getAliasQualifiedTypeName` renders the Go-shaped `*time.Location` (unrenamed, per above), then the downstream `convertToCSFullTypeName` applies `getAliasedTypeName` to the FINAL string identifier — substituting `time.Location → timeꓸLocation` before boxing — yielding `ж<timeꓸLocation>`. So the alias reaches every position that flows through the C# type-name conversion (values, pointers, boxes, composite elements alike), **provided `importedTypeAliases` is populated**. That map is loaded from the imported package's `package_info.cs` (the `[GoTypeAlias]` round-trip), so a *fresh full reconvert* renders the alias everywhere; a **stale/partial overlay** that lacks the up-to-date `package_info.cs` renders the raw name and mis-reports CS0426 — the failure is in the measurement tree, not the converter (a clean reconvert renders internal/trace/testtrace's `trace.Time`/`Event`/`Stack` as `traceꓸTime` and archive/zip's `*time.Location` as `ж<timeꓸLocation>`).<!-- both were bank-diagnosed as converter roots before a clean reconvert showed otherwise -->

The map is populated for the WHOLE package before any file converts. Loaded INCREMENTALLY —
`visitImportSpec` loads a package's aliases only when it visits an import of that package, and files
convert in sorted-filename order — a foreign renamed type reached TRANSITIVELY (through a value whose
package the current FILE does not itself import) would render its raw (nonexistent) name if that file
converted before any file that DOES import the package. go/printer's
`comment.go` (`slash := list[0].Slash`, a `token.Pos` read through `ast.Comment`, importing only `go/ast`)
sorts first, so its `slash` heap box would emit `heap<go.token_package.Pos>` instead of `heap<tokenꓸPos>`
(= `go.go.token_package.ΔPos`) — CS0426. A package-level pre-pass
(`preloadImportedTypeAliases`, run before the file-conversion loop) loads the exported aliases of every
package ANY file imports, up front. The load is deduped per imported package, so it only FRONT-LOADS what
`visitImportSpec` does incrementally; the alias set is file-order-independent, and because it only ADDS
aliases a transitive-use file would otherwise miss, it can only turn a WRONG render right (a
compiling package has no wrong-rendered renamed type).<!-- at the change: CNR byte-identical across the
behavioral corpus, and an A/B full-stdlib reconvert changed exactly one file (go/printer/comment.cs, the sole
such site in the stdlib), greening go.printer alongside an append-disambiguation root --> (Guarded by the three-package
`TransitiveAliasPreload` fixture:
`CrossPkgBox.Box` carries a field of `CrossPkgLib`'s Δ-renamed `Status`; the test's `a_boxed.go` (sorts
first) reads it transitively — `return &s` heap-boxes `s`, rendering `heap<CrossPkgLibꓸStatus>` — while
importing only `CrossPkgBox`, and `z_main.go` (sorts last) is the only file importing `CrossPkgLib`. Without
the preload the box renders the nonexistent `CrossPkgLib_package.Status` (CS0426); output-compared vs Go. It
needs three packages, which the two-package `CrossPkgLib`/`CrossPkgUser` pair cannot express.)<!-- 4 phases
green at the time; cf. the os.FileInfo alias root, then GUARD OWED for that reason -->

The preload still covers only packages **some file imports**. A foreign renamed type reached ONLY through
ANOTHER package's signature — go/types renders go/ast's `FieldFilter` (`func(string, reflect.Value) bool`)
when it passes `ast.NotNilFilter` to `ast.Fprint`, and **no go/types file imports `reflect`** — has no alias
loaded from the preload, so the synthesized delegate wrap would render the raw name: `new Func<@string, reflect.Value,
bool>(ast.NotNilFilter)` — `Value` resolved inside `reflect_package` (CS0426), and the mismatched delegate
then fails the method-group conversion (CS0123). `aliasedElementTypeName` (the delegate-element rename
route) therefore loads the owning package's exported aliases **on demand** when a foreign named element has no
registered alias — `loadImportedTypeAliases` is deduped per package, so a miss costs one probe — and the
resolving `global using reflectꓸValue = go.reflect_package.ΔValue;` rides the normal package_info emission
(the consumer sees the type through its importer's **transitive** assembly reference). For LOCAL modules the
resolver map (`importPackageDirs`) is captured over the **transitive** import closure rather than direct
imports only, so the same on-demand load works outside GOROOT. (Guarded by `SynthesizedDelegateCrossPkg`:
`CrossPkgFuncLib.Picker func(CrossPkgLib.Status) bool` + exported `Hot` matching it; the consumer imports
only `CrossPkgFuncLib` and passes `Hot` where a `Picker` is expected — the wrap must render
`new Func<CrossPkgLibꓸStatus, bool>(CrossPkgFuncLib.Hot)`; output-compared vs Go.)
<!-- illustration: adapted, owed a real excerpt -->
```csharp
public static Func<CrossPkgLibꓸStatus, nint> CheckFunc = (CrossPkgLibꓸStatus st) => st.Code * 2;
internal static (CrossPkgLibꓸStatus, nint) gauge(CrossPkgLibꓸStatus st) {
internal static ж<CrossPkgLibꓸStatus> statusPtr(ж<CrossPkgLibꓸStatus> Ꮡst) {  // *Status → box of the alias
```
Guarded by `CrossPkgUser` (`CheckFunc`/`gauge`/`meterBox` -- delegate, signature, and field positions; `statusPtr`/`ledger` -- a `*CrossPkgLib.Status` pointer as a func parameter, result, and struct field, each boxed as `ж<CrossPkgLibꓸStatus>`).

## A foreign package's collision rename is derived from that package, not from the conversion run

Everything above depends on `importedTypeAliases` being **populated**, and the only source it had was the
dependency's emitted `package_info.cs` — an artifact that exists only once that dependency has been
converted **into the output root this run resolves against**. From that source alone the spelling of a foreign renamed
member
would depend on the *composition of the run*: a full `-stdlib` run converts `time` before `archive/tar`, so
`writer.cs` emits `tw.hdr.ModTime.Round(time.ΔSecond)`, while converting `archive/tar` **alone**
(`go2cs -stdlib archive/tar`) would emit the unrenamed `time.Second`, which binds the `Second(this Time)`
extension method group — CS0019/CS1503/CS0023, it does not compile. That would hit **every end-user path**, where
the stdlib is by definition *not* part of the run: a standalone `go2cs <dir>` and `-recurse` alike. `time`
is the worst case (`Second`/`Minute`/`Hour`/`Nanosecond`/`UTC`/`Local` all collide with `Time`'s accessors,
and `Location`/`Month`/`Weekday` are collision-renamed types), so even the four-line program
`d := 2 * time.Second` would fail to compile.

**Invariant:** a foreign package's collision renames are a function of **that package's own declarations**,
never of which packages the current run converts. `foreignCollisionTypeAliases` derives them from the
dependency's loaded `go/types` scope — an exported package-level const/var/defined-type whose name is also a
method or function name of the same package, exactly `performNameCollisionAnalysis`'s rule — reproducing the
`GoTypeAlias` entries the dependency's own conversion publishes, and feeding them through the *same*
normalization as parsed ones (`applyExportedTypeAliases`) so a derived target is qualified identically. The
derivation runs only where the loader finds no `package_info.cs` on disk, so a conversion that *can* read
the real artifact is untouched. This is the same discipline `packageHasMethodNamed` applies to the cross-package *field*
rename
above: recompute a foreign package's collisions from its own `types.Package` rather than from run-accumulated
state.

Three shapes are reproduced, matching what the dependency's conversion publishes:

| Dependency declares | Published entry | Consumer emits |
|---|---|---|
| `const Second Duration` + `func (Time) Second() int` | `("Second", "const:ΔSecond")` | `time.ΔSecond` |
| `type Month int` + `func (Time) Month() Month` | `("Month", "ΔMonth")` | `timeꓸMonth` |
| `type Token any` + `func (*Decoder) Token() Token` | `("Token", "ΔToken")`, `("ΔToken", "object")` | `object` |

Two shapes are deliberately **not** derived, because publishing a wrong target is worse than publishing
none: a methodless named func type (rendered inline as its base delegate, so no `<pkg>_package.Δname` type
exists to alias — `go/doc`'s `ast.Filter`, the same skip `writePackageInfoFile` applies), and a defined type
over a **non-empty named interface**, whose alias target is a `visitTypeSpec`-only rendering of the RHS (no
instance exists in the corpus<!-- the 302-package corpus at the time -->). Both keep the unrenamed emission.

A derived alias's `global using` is emitted into the consumer's `package_info.cs` **only when an emitted
reference resolved through it**. A parsed alias set describes an assembly that provably declares every
target; a derived set describes what go2cs *would* emit for that dependency's Go source — true of any real
conversion, but not of a hand-written replacement that declares fewer types (an unused `global using` to a type it
lacks is CS0426 in every consumer).<!-- history: the retired baseline core/time stub declared no
ΔLocation/ΔMonth/ΔWeekday, and an unused global using to one was CS0426 in every behavioral test that
imported time. --> Gating on use keeps the derived metadata's reach to the code that actually names
the
renamed member — where the rename is required for the reference to bind at all — at the cost of a
single-package conversion omitting the *unused* alias declarations a full run emits. Every emitted
**reference** is identical either way: a single-package `-stdlib archive/tar` reconvert matches the
full-run corpus in all code, and the `time.Second` program compiles and runs.

Two neighboring classes of run-composition dependence share the loader and the symptom but are *not*
collision renames, so this derivation does not cover them: a dependency's re-exported Go type **aliases**
(closed next) and its **GoImplement** pairs (`loadPackageImplements`, open — see the end of the next
subsection).

(Guarded by `foreignNameCollisions_test.go`: a two-package fixture whose `dep` carries one of every shape —
colliding const, colliding type, colliding empty-interface type, methodless func type, and a non-colliding
control — asserting the derivation, its independence from run-accumulated `nameCollisions` state, and the
end-to-end render (`dep.ΔSecond`, `depꓸMonth`) with no `package_info.cs` present.)

## A foreign package's re-exported type ALIAS is derived from that package too

The sibling class, and the one that hits real end-user code hardest. `os` declares
`type FileMode = fs.FileMode` (likewise `FileInfo`, `DirEntry`, `PathError`), and a re-export takes
`visitTypeSpec`'s **using-alias** arm: the converted `os` emits an assembly-scoped
`global using FileMode = go.io.fs_package.FileMode;` and publishes
`[assembly: GoTypeAlias("FileMode", "go.io.fs_package.FileMode")]`. The re-export is therefore a *using
alias inside os's assembly*, **never a member of `os_package`** — so a consumer converted without that
artifact would emit `os.PathError` and get `CS0426: the type name 'PathError' does not exist in the type
'os_package'`. Exactly the run-composition dependence of the collision renames, one metadata class over.

`foreignTypeAliases.go` derives these under the same invariant — what a dependency publishes is a function
of that package's own declarations — from its `go/types` scope, plus its syntax for the one distinction only
a declaration's RHS carries. Two declarations take the using-alias route and are reproduced:

| Dependency declares | Published entry | Consumer emits |
|---|---|---|
| `type FileMode = fs.FileMode` | `("FileMode", "go.io.fs_package.FileMode")` | `osꓸFileMode` |
| `type Kind = abi.Kind` (and `abi` Δ-renames `Kind`) | `("Kind", "go.@internal.abi_package.ΔKind")` | `reflectliteꓸKind` |
| `type PublicKey any` (a DEFINED type over the empty interface) | `("PublicKey", "object")` | `object` |
| `type Reader io.Reader` (a DEFINED type over a named interface) | `("Reader", "go.io_package.Reader")` | `pkgꓸReader` |

Three details make the reproduction exact rather than approximate:

* **The alias TARGET carries the target package's OWN collision rename.** `internal/reflectlite`'s
  `type Kind = abi.Kind` publishes `go.@internal.abi_package.ΔKind`, because `internal/abi` Δ-renames `Kind`
  against `(*Type).Kind()`. In a full run that Δ arrives from `abi`'s parsed `package_info.cs` (the
  `importedTypeAliases` consult in `convertToCSFullTypeName`'s default arm); the derivation recomputes it
  with the same foreign-package-aware `packageHasMethodNamed` test the collision analysis uses, so the two
  sources agree.
* **A Go type ALIAS is exempt from the collision analysis.** `performNameCollisionAnalysis` records only
  *defined* types (`!typeSpec.Assign.IsValid()`), so an alias name that *also* names a method is **not**
  Δ-renamed — `reflectlite` declares both `type Kind = abi.Kind` and `(*rtype).Kind()` and still publishes
  the plain source name `Kind`. The two derivations split on exactly that line: a colliding *defined* type
  belongs to the collision analysis (which owns the `Token`/`ΔToken`/`object` two-hop), a colliding *alias* to
  this one. Getting that boundary wrong either double-publishes one source name with two targets or drops
  `reflectlite`'s entry entirely.
* **An empty-interface target is `object`, imported BARE.** `type PublicKey any` is not an alias at all but
  a defined type over the empty interface, which has exactly that interface's method set and so takes the
  using-alias arm too. Its target is the C# keyword, not a package member (`isCSharpBuiltinTypeName`) —
  `crypto`'s `PublicKey`/`PrivateKey`/`DecrypterOpts`, `plugin`'s `Symbol`, `database/sql/driver`'s `Value`.

**Deliberately not derived** (a wrong target is worse than none — a missing entry leaves the reference
unrenamed, a wrong one names a type that does not exist): a composite or basic RHS
(`type Table = map[string]int`, whose rendering runs the whole `convertToCSFullTypeName` lowering) and an
alias-to-an-alias chain; an **anonymous** struct/interface RHS, which is *lifted* under a generated name
only a conversion assigns (`internal/fuzz`'s `type CorpusEntry = struct{…}` → `CorpusEntryᴛ1`); a generic
target; a methodless named **func** type, rendered inline as its base delegate with no named type to point at
(the same omission `typeCollisionAliases` and `writePackageInfoFile` make); and a target that is **itself**
emitted as a using alias by its own package, which would need a second hop this derivation does not follow.
Each declines by shape, from the dependency's own declarations, so the decision is stable across runs.

Same use-gating as the collision renames: a derived alias's `global using` reaches the consumer's
`package_info.cs` only once an emitted reference has resolved through it. Evidence, taken with the derivation
neutered and restored: a standalone `os.FileMode`/`os.FileInfo`/`os.PathError` + `fs.WalkDir` program
converted with `go2cs <dir>` against an output root holding **no** converted stdlib fails with the CS0426
above without it, and with it compiles and runs with output byte-identical to `go run .`; a single-package
`-stdlib crypto/ecdh` reconverts `ecdh.cs` byte-identically to the full-run corpus, where without it it emits
the nonexistent `crypto.PublicKey`.<!-- a whole-stdlib reconvert was byte-for-byte unchanged at the change -->

**Open — the `GoImplement` pairs** (`loadPackageImplements`), and deliberately so:
they are recorded at CONVERSION time from the cast and witness sites a dependency's own bodies contain, so
which adapter classes its assembly actually carries is a product of its **emission**, not of its
declarations. There is nothing sound to compute from `go/types`: an over-approximation (every exported type
× every exported interface) would name adapters that do not exist — CS0246, strictly worse than what the
converter does, where the consumer records and emits its own local adapter (`io_SectionReaderжReader` instead of the
provider's `io.SectionReaderжReader`), which compiles and behaves identically and only duplicates the class.
A derivation would not close it; a concrete-to-interface conversion that went through a runtime-constructed
shell (see [Interfaces](interfaces.md#every-eligible-interface-carries-runtime-duck-typing-shells--the-sole-resolver-of-a-structural-assert))
instead of a compile-time adapter would leave no per-pair record to be missing.

(Guarded by `foreignTypeAliases_test.go`: a three-package fixture — a consumer, the `dep` whose re-exports
are under test, and the `other` it re-exports from — carrying one declaration of every published shape and
every declined one, asserting the derivation, its independence from run-accumulated state, and the end-to-end
render with no `package_info.cs` present.)

## A DOT-IMPORTED renamed type is spelled through the same alias as the qualified reference

The two subsections above are about the alias metadata being **derived**; this one is about it being
**used**. Having the right alias minted is not the same as reaching it, and one reference path did not.

A dot import (`. "go/types"`) makes a foreign type's reference a bare `*ast.Ident` — there is no selector for
the qualified-name resolver to rewrite — yet the type may still be collision-renamed inside its own package.
The **type-driven** positions were always fine: a declaration, a parameter, a conversion and a field all
resolve from `types.Type` through `getCSharpTypeName`/`getScopeCheckedTypeName`, both of which consult
`foreignAliasedTypeName`. That is why `var mu Mutex` through a dot import works (`DotImportRenamedPackage`). The two
**AST-ident** type positions are the exception: a *type-assertion target* and a
*composite-literal type* render through `convIdent`'s `isType` arm, which by itself returns the bare sanitized Go
name and consults nothing.

So `internal/types/errors`, whose external test file dot-imports `go/types`, would emit `err._<Error>(ᐧ)` and
`new Info(…)` against declarations named `ΔError` and `ΔInfo` — `go/types` renames `Error` for its own
`func (err Error) Error() string` and `Info` for the unrelated `func (b *Basic) Info() BasicInfo` — while
that test's own `package_test_info.cs` mints `global using typesꓸError = …ΔError;` and
`typesꓸInfo` (CS0246 ×2).

**Invariant:** one Go type has one C# spelling, whatever the source called it. `convIdent`'s `isType` arm
routes through `foreignAliasedTypeName` — the *same* recorded-alias lookup the qualified path takes — so
`Info{…}` and `types.Info{…}` emit the identical `typesꓸInfo`. It is a no-op for a same-package type and for
any type with no registered alias.

<!-- illustration: not converter output -->
```csharp
var m = new renamedlibꓸMarker(Name: "alpha", Size: 3);            // composite literal  (not: new Marker(…))
var (got, ok) = Describe("delta", 9)._<renamedlibꓸMarker>(ᐧ);    // type assertion     (not: _<Marker>(ᐧ))
var pl = new Plain(Note: "eta");                                   // NOT renamed — bare, unchanged
var l = new ΔLocal(Tag: "iota");                                   // same-package rename — local, no alias
```

The rename rule itself is `performNameCollisionAnalysis`'s and is worth stating exactly, because the second
half is easy to miss: a package-level named element collides when **some** package-level `FuncDecl` in that
package shares its name. Both a method on the type itself (`Error`) and a method on an unrelated type
(`Info`) supply it; since Go forbids a type and a free function sharing a package-scope name, the collision
can only ever come from a method.

(Guarded by `DotImportRenamedType`: a sibling library package declaring one type of each collision shape
plus a non-renamed control, consumed across the package boundary through a dot import via composite
literals — value and pointer — and type assertions in comma-ok, single-value and missed forms, with a
same-package renamed type as the second control; output-compared vs `go run`. Verified to FAIL as CS0246
with the alias lookup reverted.)

<a id="the-reflection-bridge-answers-a-read-where-the-answer-exists--four-members-that-did-not-2026-08-19"></a>
## The reflection bridge answers a read where the answer EXISTS

Four unrelated reads through `reflect` answer from what golib already computes, where a degraded answer would read as a real one. They share one discipline: a bridged member either honors the read or refuses it by name — never a plausible-looking substitute, and never a descriptor field left unpopulated because it *seemed* unanswerable when the answer is computed one layer down.<!-- 2026-08-19; the r39d rule applied in the direction it is usually NOT applied -->

| Member | A degraded bridge answers | go2cs answers |
|:--|:--|:--|
| `Value.Index` on a **string** | `panic: reflect: call of reflect.Value.Index on string Value` — Go's message for a kind that does not support indexing AT ALL | the i'th BYTE as a non-addressable `uint8` Value, Go's own arm |
| `Value.Slice` on a **string** | the same ValueError | a string of the receiver's OWN type, so a NAMED string stays named (Go returns `Value{v.typ(), …}`) |
| `Value.Slice3` | `invalid memory address or nil pointer dereference` — the auto conversion reinterprets the never-populated `ptr` slot as a raw `unsafeheader.Slice` and edits it in place | hand-owned over the same golib window machinery `Slice` uses, so the two- and three-index forms cannot disagree about what a window is |
| `StructField.Offset` | `0` — a REAL answer for a field at the front of a struct, so an unpopulated descriptor read as a LAYOUT failure | the Go (amd64) offset, from the same memoized layout walk `internal/abi`'s `StructType()` already publishes |

`Offset` is the one worth stating at length, because there is a stated reason to leave it empty: *a Go byte offset exists only to be added to a data pointer, and managed storage has no such pointer*. That is true of `Offset` as an ADDRESS and false of `Offset` as layout METADATA, which is the only way anything reads it here — `unique`'s clone sequencer and `internal/reflectlite` through `abi`, and `sync/atomic`'s `TestAutoAligned64`, which asserts `TypeOf(&struct{_ uint32; i Int64}{}).Elem().Field(1).Offset == 8`. Reading it off `GoReflect.GoFieldOffsets` is what makes the two Go-specific layout rules come out right where a naive `Marshal.OffsetOf` would not: a Go **zero-size** field occupies nothing (its C# surrogate is one byte), and an `align64`-bearing field is padded to its 8-byte boundary. Refusing still applies where it should<!-- the r39d rule --> — a struct holding a field whose Go size is unknowable makes every later offset a guess, `GoFieldOffsets` answers `null` for the whole struct there, and every field keeps the zero rather than a plausible number.

`text/template`'s `index` and `slice` builtins are the measured consumers of the first three (`{{index `x` 0}}`, `{{slice .S 1 2}}`, `{{slice .SICap 6 10 10}}`).<!-- closing them took text/template from 49 to 50 of 52 --> Guarded by `ReflectStringWindow` (every arm plus Go's own out-of-range, wrong-kind and reversed-bound panic texts, and a byte-slice control that the string arms did not disturb the container path) and by `ReflectStructTagCopy` (offsets across a zero-size/aligned/tail layout, and a promoted field whose offset is relative to its own declaring struct).

## A typed nil keeps its type across BOTH interface-space boundaries

`Value.Interface()` packs a nil pointer as Go's typed nil — a non-nil `any` carrying `(type=*T, value=nil)` — and two
other paths use that one nil encoding too. Without it both show as `text/template` mis-rendering a value Go renders
through its method:

1. **`Value.Call` into an INTERFACE-typed parameter.** Go's assignment to such a parameter BUILDS an eface, and an eface keeps the type half. Handing the slot's raw `null` across would make the callee's `reflect.ValueOf(arg)` answer the INVALID zero Value — `{{html .NIL}}` over a nil `*int` would print `&lt;no value&gt;` where Go prints `&lt;nil&gt;`. `marshalCallArg` packs through `packInterfaceValue` when — and only when — the destination is interface space; a concrete parameter type builds no eface and is untouched.
2. **A nil RECEIVER dispatched through the runtime duck-typing SHELL tier.** Go's method set belongs to the TYPE, so `(*W)(nil).Error()` dispatches normally and the method decides what nil means (`if w == nil { return "nilW" }` is an idiom, not an edge case). golib's `error<T>` chooses between its `ж`-receiver and by-value overloads BEFORE reading `m_target_ptr.Value`; reading first would throw on exactly that value, for a pointee the `ж` overload never needs. Such a throw would be invisible: `fmt`'s `handleMethods` wraps every `Error()` call in Go's own `catchPanic`, which prints `<nil>` for a nil-pointer argument, so the symptom would be a wrong RENDERING, ONLY where the pair resolves through the runtime shell rather than through a generated nominal adapter — i.e. only when nothing in the program converts that type to that interface explicitly. Dereferencing on the by-value path is kept, because Go dereferences there too and a nil pointer must still panic.

Guarded by `ReflectTypedNilInterface`, which carries a type reached only through the shell tier and the concrete-parameter control for the `Call` arm.

## Converted programs write UTF-8 stdout — the ambient console code page never reaches the bytes
Go writes stdout as raw UTF-8, unconditionally: `fmt.Println("Hello, 世界")` emits the same bytes to a
terminal, a pipe, or a file. .NET does not. `Console.Out` is constructed with `Console.OutputEncoding`,
which on Windows defaults to the **console output code page** (`GetConsoleOutputCP()` — the OEM page, 437
on a stock US install), and to the ANSI default (1252) when the process has no console at all — the case for a
program launched by the behavioral runner (`CreateNoWindow = true`) or by the tour's `.NET Run` pane
(`src/tour/pipeline.go` `runStage`, whose `command.Stdout` is an in-process writer). Encoding a rune that code
page cannot represent is not an error in .NET; the encoder substitutes `?`, so the Tour of Go's first
lesson would render `Hello, ??` with no diagnostic anywhere (on windows/amd64).

golib forecloses this in its `[ModuleInitializer]` (`src/core/golib/builtin.cs`), which runs before any
converted code: `Console.OutputEncoding = Console.InputEncoding = Encoding.UTF8`. The setter also discards
any already-created `Console.Out`, so the writer is rebuilt on the UTF-8 encoding, and .NET strips the
encoding's preamble for console writers — no BOM is prepended. A failing `SetConsoleOutputCP` (no console
attached) is tolerated, so the redirected case is covered as fully as the interactive one. `fmt` itself
reaches the same place by a different route and needs nothing added: it writes through `os.Stdout` →
`internal/poll` → the platform write (`syscall.WriteFile` on Windows), which hands the `[]byte` over
verbatim and is byte-transparent by construction. What depends on the encoding above is output written
through `System.Console`, such as golib's `print`/`println` and its unhandled-exception report on
`Console.Error`.<!-- When this was written the baseline core/fmt stub, a proxy over Console.Write/Console.WriteLine,
also depended on it; the stub baseline retired 2026-08-01 (commit 2e8066da6). The stub's one other divergence
retired with it: Console.WriteLine terminated with Environment.NewLine (CRLF on Windows) where Go always writes \n. -->

Guarded by the `UnicodeConsoleOutput` behavioral test, which prints CJK, Greek, Cyrillic, a math symbol and
an astral-plane emoji, and is stdout-compared against the Go binary. The guard is differential, so it holds
even under a lossy capture: the runner decodes both children's bytes with the same encoding, and mojibake
never equals `?`.<!-- Neutering the golib line and running under `chcp 437` failed it with `stdout mismatch C# vs Go`
when fmt was the stub; restoring the line passed in the same console. --> The converted `fmt` writes Go's `\n`
unchanged on every platform, since the platform write passes the bytes through.

---

[← Labeled Control Flow and Loop Variables](labels-and-loop-variables.md) · [Index](README.md) · [Source Generators →](source-generators.md)
<!-- {% endraw %} -->
