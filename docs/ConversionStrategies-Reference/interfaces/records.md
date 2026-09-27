# Interfaces: GoImplement Records

[Reference index](../README.md) · [Interfaces](../interfaces.md) · [Summary of this topic](../../ConversionStrategies.md#interfaces)

This page covers the `[assembly: GoImplement<T, Iface>]` records a package emits for each type and interface it pairs: when a pair is recorded, how records are de-duplicated, and the sealing-marker stubs of a cross-package interface.

## Recording

### Structural interface satisfaction emits C# interface inheritance
Go converts `fs.File` to `io.Reader` implicitly because the method set suffices; C# interfaces
are nominal. When a declared interface's method set **strictly contains** an EXPORTED method
interface from a **directly imported** package (checked with `types.Implements`), the converter
emits real C# inheritance at the declaration and **skips re-declaring the covered members**
(redeclaring would HIDE the base member — implementers would need both):

```csharp
[GoType] partial interface File :
    io_package.ReadCloser
{
    (FileInfo, error) Stat();
}
```

Every downstream interface-to-interface conversion then becomes an implicit reference
conversion — identity-preserving (the dynamic value flows through type asserts, unlike an
adapter wrapper) and zero-cost (os's `CopyFS` passes an `fs.File` to `io.Copy`, CS1503).
Details: only the **minimal covering set** is listed (`ReadCloser` subsumes `Reader`/`Closer`);
the strict-subset guard rules out inheritance cycles (equal method sets never inherit);
candidates covered by a declared **embed** are skipped (the embed emission handles those);
bases reference the **file-local package alias** (`io.ReadCloser`, user-ruled style) via
getAliasQualifiedTypeName, which also registers the using — needed because the declaring Go file may not
import the candidate's package (`fs.go` declares `File` without importing `io`); lifted/dyn
and constraint interfaces are excluded. **Multiple non-subsuming bases sharing a method**
(`CrossPkgLib.Sealed` and `.Rated` both carry `Label`): both are inherited, and the shared
member is **re-declared** — a member covered by exactly one listed base is inherited/skipped,
but one covered by two or more is re-declared so it hides both inherited slots and member
lookup through the derived interface stays unambiguous (CS0121). Go needs only one method to
satisfy all; the C# implementers satisfy every slot with the same public method. Consequently the converter **never records an interface-to-interface
`GoImplement`** — the generator's impl types are structs, and an interface-typed record kills
its whole run. Bounds (banked): candidates come from direct imports only — same-package
structural pairs, the universe `error`, and non-imported-package pairs would still surface as
compile errors and would need the adapter complement. Guarded by `CrossPkgUser` (`namedLabel :
CrossPkgLib_package.Labeled`, passed to `CrossPkgLib.Describe`).

### A GoImplement record is gated on the method set actually satisfying the interface
Every `[assembly: GoImplement<T, Iface>]` record makes the `ImplementGenerator` emit implementation glue whose members forward to T's like-named methods — so a record whose Go method set does NOT satisfy the interface generates a forwarder to a method that does not exist. The corpus case: net/http's `err = http2GoAwayError{LastStreamID: …, ErrCode: cc.goAway.ErrCode, …}` — the keyed composite's sparse-array `ident` context leaks the `error`-typed LHS onto each FIELD value, and the `ErrCode` field's value recorded `GoImplement<http2ErrCode, error>` even though `http2ErrCode` has only `String()`/`stringToken()` (its generated `Error() => this.Error()` was CS1929). `convertToInterfaceType` now folds a `types.Implements` check over the recorded form's method set (T for a value record, `*T` for a `ж<T>` record) into `recordableBase`, which gates both the record and the matching adapter-wrapping emissions. A conversion the Go checker admitted always passes the check, so the gate can only drop pairs a caller composed from mismatched types; a type-param-carrying target skips the check (`types.Implements` is undefined for uninstantiated generics, and the open-generic conversion emission must stay). The full-stdlib A/B for this change is exactly one removed line — the false `http2ErrCode` record. (Guarded by the NEGATIVE `KeyedLiteralIfaceAssign` behavioral test: a keyed literal assigned to an `error` variable whose field-value type has `String()` but no `Error()` — a reintroduced record fails the compile phase.)

### A white-box PRODUCTION↔PRODUCTION pointer pair is already implemented — do not record it again

Under the `whitebox-reference` test model the internal bridge is the SAME Go package as production, so
a `*prodT → prodIface` cast inside an internal `_test.go` reads as local and records its own
`[assembly: GoImplement<T, Iface>(Pointer = true)]`. Production, though, is a **referenced assembly**
that already generated that adapter from its own record — and `InternalsVisibleTo <assembly>.tests`
makes even an unexported adapter class reachable. The duplicate record makes go2cs-gen emit a SECOND
adapter under a test anchor, and that copy resolves its forwarding members in the TEST class's scope:
`context`'s `contains(pc.children, cc)` converts `*cancelCtx`/`*timerCtx` to `canceler` for a map key,
and the duplicate bound `Done` to the unrelated `afterFuncContext.Done` extension (CS1929) while
emitting `cancel` with an **empty body** — a silently degraded override, not merely a build error.

The fix suppresses only the RECORD, which is what makes it small: `resolveAdapterNameMarkers` resolves
a pair that reached no record to the unqualified name it would have had, and that name is production's
own adapter, so the cast site repoints with no other change.

```csharp
!contains((~pc).children, new global::go.context_package.cancelCtxжcanceler(cc))   // production's, not a copy
```

It is gated on production ACTUALLY carrying the pair — its `package_info.cs` is loaded by
`convertTestVariant` into `importedPointerImplements` — never assumed: a pair only the test converts
still needs its local record. Reaching that set also required `canonicalRecordIfaceName` to strip a
leading `global::`, which names no package and never appears in a parsed record; the deliberate
non-collapse it documents is untouched, since a genuinely foreign pair still keys as
`net.http_package.ΔHandler` against the record's `http_package.ΔHandler`. This is the POINTER twin of
the value arm's `whiteboxProductionTarget` carve-out — note the pointer target arrives as a
`*types.Pointer`, so the shared `whiteboxProductionTarget` flag (computed from the unwrapped VALUE
form) is structurally false there and the check must unwrap and ask directly.

### A generic struct implementing an interface BY VALUE partials at its OPEN definition
A Go method on a generic type is declared for every instantiation, so `func (g G[T]) M()` makes
`G[int]`, `G[string]` and `G[G[int]]` all satisfy an interface with `M`. The converter records a
`[assembly: GoImplement<…>]` per instantiation it sees, and `ImplementGenerator`'s value-form arm
wrote one `partial struct` per record, spelled with the record's TYPE ARGUMENTS:
`partial struct G<IntPtr> : I`. C# reads that argument list as a **type-parameter list**, so the
declaration disagrees with the converter's own `partial struct G<T>` (CS0264) and the mismatched
parts stop merging — every member the template writes then lands in the containing **static**
package class instead (CS0715 on the operators, CS0708 on `Equals`/`GetHashCode`/`ToString`,
CS0563 and CS0540 in the cascade). The arm now emits ONE partial against the open definition, keyed
by `(OriginalDefinition, interface)` so all instantiations of a pair fold into it; the member and
value-pair dedupe indexes key on the same open form, since two interfaces over one open generic
share a single partial. Constraints are deliberately omitted — a partial declaration may leave them
off and they merge from the converter's declaration, so omission can never raise CS0265. The
pointer-adapter arm had always done this (`emittedGenericPointerAdapters`, crypto/elliptic's
`nistCurve[Point]`); this is its value-form sibling.

Behind it sat a second, independent defect in the shared `GetSimpleName` helper, and it is the one
that explains why the two packages holding this class both name their generic with a **single
letter**. Asked to drop a type-argument list, the helper tested `typeName.IndexOf('<') > 1` — so
`G<T>`, whose `<` sits at index 1, kept its arguments. `StructTypeTemplate` derives the constructor
name from that call, and emitted `public G<T>(NilType _)`, which is not a constructor to C#: the
`partial struct G<T>` scope never opens and the same spill follows. Every multi-character generic
in the corpus (`meta<T>`, `nistCurve<Point>`, `Handle<T>`) cleared the guard, which is why this
survived to the first single-letter one. The guard is now `> 0` and indexes the *simple* name
rather than the full one — the latter also closes a latent, currently unreached miscut on a dotted
generic (`a.Map<K, V>` indexed at 5 into an 8-character `Map<K, V>`, yielding `Map<K`).

Measured on `internal/reflectlite` (`type B[T any] struct{}`) and `runtime/debug`
(`type G[T any] struct{}` with `var dummy I = G[int]{}` and `var dummy2 I = G[G[int]]{}`), the two
packages the board recorded behind one CS0715 root. `runtime/debug` moves from build-blocked to a
measured **2 of 9**; `internal/reflectlite` clears this root and stops on five unrelated ones.
Guarded by the `GenericValueInterfaceImpl` behavioral test — a single-letter generic held as an
interface at three instantiations, a sibling type named exactly like the type parameter (the
`runtime/debug` shape that made the spilled members render as `debug_test_package.T`), struct
equality, struct-versus-interface comparison, and interface dispatch over a mixed slice, all
output-compared against `go run`.

## De-duplication

### GoImplement records de-duplicate at attribute emission
os converts dirEntry to fs.DirEntry both through its own alias (`type DirEntry = fs.DirEntry`) and through the io/fs name - two records for ONE interface made the generator emit the explicit implementation twice (CS8646/CS0111). The de-duplication happens at ATTRIBUTE EMISSION with the ALIASED record winning (its simple name resolves via the package usings); normalizing the RECORD KEY instead was twice wrong - qualified attr names break generator name resolution and flip the alias-locality gate. **Measurement lesson:** those declaration-phase errors had SUPPRESSED all of os's method-body diagnostics (Roslyn phase gating) - a package is not truly measured until its declaration errors are zero.

**The comparison must run on the EMITTED spelling, not the raw registry key (2026-08-08).** The
covered set is built from `exportedTypeAliases`, whose values `visitTypeSpec` has already
canonicalized — it reverts a file-local import rename before recording the alias target — while the
registry key keeps whatever rendering the cast site produced. os aliases its `io` import to `Δio`
(io is shadowed once io/fs is in the reference closure), so on the **unix** flavors
`unixDirent`→`fs.DirEntry` registers as `DirEntry` *and* as `Δio.fs_package.DirEntry`, and **neither**
compares equal to the canonical `io.fs_package.DirEntry` the covered set holds. Both records were
emitted for the one pair and `unixDirentжDirEntry` was composed twice (CS0102, CS0111 ×9, CS8646 ×4).

That also produced a **third** adapter spelling, worth recording because it looks like a separate
defect and is not: two records composing one adapter name is exactly what `adapterNameCollisionSet`
exists to detect, so it saw a FALSE collision, applied its collision-conditional rule and qualified
the foreign side of one cast site to `unixDirentжfs_DirEntry` — a name neither record produces
(CS0246). Removing the duplicate removes the collision, and all three call sites in `file_unix.cs`
converge. The key is now built with `qualifyLocalTypeRef`, the same canonicalization the emission
applies, so the two sides are comparable by construction. Windows is unaffected, and that was
measured rather than argued: its `os` registers the renamed-canonical spelling ALONE, with no
alias-keyed record, so nothing is covered and the qualified record is still the only record.
(Guarded by `implementRecordAliasCanonicalization_test.go`, whose controls pin both the Windows shape
and a genuinely distinct second implementation that must NOT collapse.)

**The interface-inheritance PRUNE exempts pairs that generate their own adapter CLASS.** The same
attribute-emission stage also drops a "lower" GoImplement record when the SAME implementing type is
recorded against a derived interface that C#-inherits it (elf's errorReader against both io.ReadSeeker
and io.Reader — the two value-form partial-struct implementations would implement `Read` twice,
CS0111/CS8646). That prune is only valid for the value-boxing PARTIAL-STRUCT form (one type, one
interface list). A pair whose implementation is a DISTINCT generated adapter class must survive, since
each cast site references the adapter for the EXACT interface it targets — the ж<T> pointer form was
already exempt, and the same now holds for the value-form adapter classes (`<src>ᴠ<iface>`): an
**interface-sourced** conversion (net/http wraps `net.Conn` values as `io.Reader`/`io.Writer` — the
prune dropped both pairs under the also-recorded `Conn→ReadWriteCloser`, so every
`new net_ConnᴠWriter(…)` referenced a class the generator never emitted, CS0246 ×17 in net/http and
recurring in net/rpc and httputil) and a **foreign-struct value** conversion
(`<pkg>_<T>ᴠ<iface>`) are marked at recording time (`adapterClassImplementations` in
`convertToInterfaceType`) and skipped by the prune. (Guarded by `IfaceToIfaceNarrow` — one source
interface converted to a full-surface embedded-interface target AND to its narrower bases at
argument, assignment, and return positions, dispatch output-compared vs Go.)

#### A `global::` root escape is a THIRD spelling of one type, and the record sets dedupe on text

The de-duplication above compares rendered attribute lines, so every distinct spelling of one type is
a distinct record. The alias case is the one that section documents; the **root escape** is the same
defect reached by a different route, and it is invisible in a production conversion. `-tests` alone
mints `global::` — `testAliasShadowOperations` / `convSelectorExpr` escape to the root whenever a test
package's own class shadows the leading segment of a qualified reference — so one test package can
register the same (impl, interface) pair from an escaped site and a bare site and emit both records.
go2cs-gen resolves both to the SAME symbol and mints the adapter twice: `net/http`'s
`http_HandlerFuncᴠΔHandler` came out as both `-val.g.cs` and `-val.1.g.cs`, giving CS0102 + CS0111 ×5
+ CS8646 ×2 against a test suite that had never run.

`dedupeRootEscapedRecords` collapses lines that differ only by root escapes, as a shared pass over
**both** record sections — `GoImplement` and `GoImplicitConv` are built by the same
`qualifyLocalTypeRef` rendering over the same registries, so they carry the same exposure and a fix in
one alone would only wait for the other. It runs BEFORE `recordEmittedPointerAdapterPairs`, so the
adapter-naming authority sees the deduplicated set and cannot manufacture the false collision that the
alias case's third spelling came from.

The **escaped** spelling wins a collapse: it is shadow-proof by construction, which is why the
machinery minted it, and keeping the bare form could reintroduce the shadow the escape exists to
defeat. That is the opposite preference from the alias case — there the ALIASED (simple) form wins
because the qualified form breaks generator name resolution — and the two are consistent once stated
as one rule: **keep the spelling that resolves under the most conditions.** A root escape adds
resolution guarantees; a package qualifier removed one. Ties fall to the lexicographically smaller
line so the output stays deterministic. The pass is an exact identity on any record set without an
escape, which is what makes the whole production corpus provably unaffected. (Guarded by
`rootEscapedRecordDedupe_test.go`, whose four cases pin the collapse, the escape-count preference
independent of sort order, a negative control that distinct records all survive, and the
production-inertness identity.)

#### The promoted-method twins class is named for the PACKAGE and the pair, not the pair alone

A struct that satisfies an interface member by **promotion** gets an `internal static class
<pkg>ᴛ<struct>ᴛ<iface>ᴛpromoted` of extension twins, because go2cs's runtime method set is built from
extension methods and a promoted method is the one kind of Go method that never became one. The class
sits at NAMESPACE scope — deliberately a sibling of the package class, so its twins cannot intercept a
bare-name call — and it was named for the (struct, interface) pair alone.

That name is unique per package class, and a namespace holds several of them. Go's **internal**
(`package http`) and **external** (`package http_test`) test variants are distinct packages that may
each declare a type of the same name; they emit correctly into distinct classes
(`http_internal_test_package`, `http_test_package`) and share one namespace. `net/http` declares
`dumpConn` in both `requestwrite_test.go` and `transport_test.go`, each promoting `io.Reader` and
`io.Writer`, so `dumpConnᴛReaderᴛpromoted` and `dumpConnᴛWriterᴛpromoted` were each declared twice in
`go.net` — CS0101 ×2. Nothing about the type model is wrong there; only this cargo name was
under-qualified, and adding the owning package makes "two generated files can never collide" true
rather than merely intended.

Qualified **unconditionally**, which is the opposite of the collision-conditional rule the pointer
ADAPTER names follow, and the difference is blast radius rather than principle: an adapter name
appears at thousands of construction sites, whereas no source-level call ever binds a twin (the struct
MEMBER always wins) and the method-set registry discovers them by scanning every non-nested static
class, never by name. The rename is invisible everywhere except in its own declaration.

## Sealing markers

### A cross-package interface's unexported sealing marker is stubbed
Go seals an interface to its defining package with an **unexported marker method** — `ast.Expr`'s
`exprNode()`, `ast.Stmt`'s `stmtNode()`, `ast.Decl`'s `declNode()`, `text/template/parse.Node`'s
`tree()`/`writeTo()`. The method's C# implementation is an **internal** extension in the interface's
own assembly (`internal static void exprNode(this ref IndexExpr _)`), so an adapter generated where
the interface is CONSUMED — `go/internal/typeparams` casting go/ast's `*IndexExpr` to `ast.Expr`, or
`text/template` casting `*parse.RangeNode` to `parse.Node` — cannot see it: forwarding
`m_box.Value.exprNode()` is CS1061. The C# interface member itself is public (unexported Go methods
render without a modifier), so it is still *required* — dropping it is CS0535, and an *internal*
interface member cannot be implemented cross-assembly at all. Because Go never lets a sealing marker
be called from outside its package, the adapter satisfies the member with a **no-op / `default!`
stub** instead of forwarding:
```csharp
void global::go.go.ast_package.Expr.exprNode() { }                       // void marker
global::go.…parse_package.Tree global::go.…parse_package.Node.tree() => default!;   // non-void marker
```
The `ImplementGenerator` flags a method as an inaccessible marker when its Go name is unexported
(`GetScope == "internal"`), its declaring assembly differs from the one the adapter is generated
into, **and the struct declares no method of that name in the current compilation**
(`MethodInfo.IsInaccessibleMarker`); a SAME-assembly impl keeps forwarding (the internal extension is
accessible there). Both the pointer (`AdapterImplTemplate`) and value (`ValueAdapterImplTemplate`)
adapters emit the stub. This greens `go/internal/typeparams` (whose only errors were the two
`exprNode` forwards) and is a prerequisite for `text/template`/`go/doc`. (Guarded by
`CrossPkgLib`/`CrossPkgUser`: the sealed `Emitter` interface with an unexported `emitNode()`, a
`*Leaf` implementing it, cast to `Emitter` in the consumer assembly — CS1061 without the stub.)

That third clause is the correction the white-box test model forced (2026-08-09, `internal/profile`).
The assembly comparison is a *proxy* for "there is nothing to forward to", and it answers wrongly for
the one shape where a single Go package spans two C# assemblies: an **INTERNAL (white-box) test
package**. `internal/profile`'s `proto_test.go` is `package profile` — it declares `packedInts` and
its `encode`/`decoder` methods for the production package's own unexported `message` interface. Same
Go package, different C# assembly, and genuinely reachable, because the test model mints an
`InternalsVisibleTo` grant for exactly this. Stubbing there is worse than a compile error: the
adapter COMPILES and *silently does nothing*, so `marshal(source)` returned an empty buffer and
`unmarshal` decoded nothing, with no diagnostic at any layer. Requiring the absence of a local
implementation — the struct's own value/ref extensions plus its direct-`ж` primaries — leaves every
genuine marker stubbed unchanged, because a FOREIGN struct never declares the sealing method (Go
forbids implementing another package's unexported method at all, so a `[GoImplement]` record naming
an unexported interface method can only come from a struct in that same Go package). The guard is
`internal/profile`'s own banked suite: the shape needs a white-box test package, which the behavioral
corpus has no way to express.

The white-box model then forced a SECOND correction to the same clause (2026-08-19, the crypto/tls
regression). `internal/profile`'s shape declares the STRUCT in the test compilation, so the
local-implementation evidence — gathered from the struct's own declaration syntax — was findable
there. crypto/tls's `TestMarshalUnmarshal` is the mirror shape: the struct is a PRODUCTION type
(`*SessionState`, metadata-only in the test compilation) and only its METHODS are test-declared
(handshake_messages_test.go's `marshal`/`unmarshal`, the sole source of its `handshakeMessage`
satisfaction — Go lets a package's test files add methods to its production types). With no local
declaration syntax the evidence set was EMPTY, both members classified as markers, and the identical
silent-stub failure recurred: marshal answered an empty buffer with nil error and the test reported
"failed to unmarshal" with no diagnostic. The evidence now also covers the friend bridge's
extensions by receiver simple name, in BOTH receiver forms — direct-`ж`, and `[GoRecv] ref` (which
forwards through its RecvGenerator `ж`-twin, the same routing `IsRefRecv` applies to a local
declaration). Genuine markers still stub: a foreign struct with no bridge has no such extensions
anywhere in the compilation. (Guarded by `GenTests.WhiteboxBridgeAdapterTests`, which runs the real
generator over a two-assembly model of the shape, and the ref-scan rows in
`GenTests.FriendBridgeBoxReceiverTests` — the behavioral corpus still cannot express a white-box
test package.)


#### …so the DECLARING package must own the adapter, and its speculative record carves out for it

The stub above rests on one sentence — *"Go never lets a sealing marker be called from outside its
package"* — which is true and is not the whole rule. The marker cannot be called from outside; it is
called **inside**, on a value the consumer boxed, which is the entire reason a sealing interface has
unexported members in the first place. `text/template/parse.ErrorContext(n Node)` opens with
`tree := n.tree()`; `html/template` boxes an `&n.BranchNode` into that `Node`. Nothing in
html/template can call `tree()` — and nothing has to, because `parse` does it for them. A stubbed
`tree()` answers `default!` there, `ErrorContext` then substitutes its own receiver, and at
html/template's `(*parse.Tree)(nil).ErrorContext(e.Node)` call site that receiver is also nil, so
`~tree` nil-dereferences. `TestErrors` was the symptom, three packages from the record that was never
written (2026-08-20).

So the stub is a **last resort, not a design**, and it must be unreachable for any pair that can be
realized properly. It can always be realized in ONE assembly: Go scopes an interface with an
unexported method to its declaring package, so every type that will ever implement it is declared
there, and the declaring assembly's own adapter forwards the marker natively (its extension is
`internal`, and that is the assembly it is internal to). A consumer then references the exported
`pkg.TжIface` through the existing foreign-adapter-exists arm and mints nothing. That is what
[`recordSamePackageImplements`](#a-package-records-the-pairs-it-satisfies-not-only-the-ones-it-witnesses) already
does for the pairs a package satisfies but never witnesses — it simply withheld this one.

The gate it withheld on is `generatorCanForwardPointerMethodSet`, which demands every interface method
resolve DIRECTLY on the type (no promotion at all), on the stated reasoning that *"withholding a
speculative record is always safe, because the consumer keeps the local adapter it had before"*. That
sentence is true of an all-exported interface and **false of a sealed one**, where the local adapter
is not a fallback but an adapter that cannot work. `*parse.BranchNode` is exactly the shape: it
implements `Node`, but `Type()` and `Position()` are promoted from its embedded `NodeType`/`Pos`, so
the strict gate refused — and `parse` never casts a `*BranchNode` itself (it casts the
`If`/`Range`/`With` wrappers), so nothing demanded the record either.

The carve-out is therefore keyed on the interface, not on the type: **when the interface carries an
unexported method, the pointer record falls back to the VALUE form's depth-2 bound instead of being
withheld.** It stays bounded — a promotion deeper than one embed hop is still refused, exactly as
before — and the shape the strict gate was written for (a speculative record whose promoted member
resolves through the wrong embedded POINTER hop, `StructPointerPromotionWithInterface`'s
`MyCustomError`) is an all-exported interface that never reaches the arm. Corpus footprint at
`text/template/parse`: one line, `[assembly: GoImplement<BranchNode, Node>(Pointer = true)]`.

What remains stubbed is what should be: a pair the declaring package genuinely cannot realize
(promotion deeper than one hop, a generic, an unexported target) would still mint a consumer-local
adapter whose marker is a stub, silently. **Censused after the fix, the standard library holds no such
pair**: of the **1,307** `ImplementGenerator` adapters a whole-stdlib reconvert generates, **zero**
carry a `=> default!` or empty-body member. The shape stays reachable — it is what to suspect when a
sealed interface's member answers a plausible zero — but it has no instance today.

Guarded by `CrossPkgLib`/`CrossPkgUser`, extended for this: `Emitter` gains a VALUE-returning sealed
member `nodeTag() string` and the lib gains `DescribeEmitter(e Emitter)`, the `ErrorContext` shape —
a declaring-package reader of the sealed member, on a value the consumer boxed. `*Leaf` (whole method
set declared directly, so its record was never withheld) is the control and reads `leaf/lf` either
way; `*Branch` (Emit promoted through its `EmitBase` embed) read **`branch/`** before the carve-out
and reads `branch/brn` after — proven by neutering the carve-out and running the pair.

## Cross-package records

### A foreign implement record is keyed in ONE spelling, and a VALUE one is trusted only for a partial struct

The record scraped above answers one question at a cast site: *does the dependency's own assembly
already implement this pair?* If it does, the bare value converts implicitly and a local
`<pkg>_<T>ᴠ<Iface>` value adapter is dead machinery. Getting the answer wrong in either direction is
expensive, so both halves — the KEY and the TRUST — are stated precisely here.

**The key.** `implementRecordKey` composes `<declaring package>|<C# simple type>|<pkg>_package.<Iface>`
and is called by BOTH sides of BOTH record sets: `loadPackageImplementLines`, over records parsed from a
dependency's `package_info.cs`, and the value arms *and* the foreign-pointer arm of
`convertToInterfaceType`, over a cast being converted. That it is one function is the whole point — the
two sides used to compose it independently, over different alphabets, and agreed only when the
dependency's import path was a single segment:

| dependency | load side | use side | |
|:--|:--|:--|:--|
| `io` | `io\|noBody\|io_package.ReadCloser` | `io\|noBody\|io_package.ReadCloser` | match |
| `encoding/binary` | `binary\|bigEndian\|binary_package.ByteOrder` | `binary\|bigEndian\|encoding.binary_package.ByteOrder` | **miss** |
| `image/color` | `color\|ΔRGBA\|color_package.Color` | `color\|RGBA\|image.color_package.Color` | **miss** |
| `text/template/parse` (ptr) | `parse\|ListNode\|parse_package.Node` | `parse\|ListNode\|text.template.parse_package.Node` | **miss** |
| `go/types` (ptr) | `types\|TypeName\|go.types_package.Object` | `types\|TypeName\|types_package.Object` | **miss** |
| `image` (ptr) | `image\|ΔRGBA\|image_package.Image` | `image\|RGBA\|image_package.Image` | **miss** |

Two divergences, and the second is easy to miss because it only shows on a collision-renamed type.
(1) The INTERFACE side: a parsed record names the recording package's own interface BARE and a foreign
one whole (`go.image.color_package.Color`), while a cast site always renders the full namespace chain.
`canonicalImplementRecordIfaceName` drops everything ahead of the `<pkg>_package` segment, so both reduce
to `color_package.Color`; a member path under the class (`y_package.Outer.Inner`) survives intact. The
package CLASS must stay — the simple name alone collides, and image's `Paletted`→`image.Image` record
must not satisfy a `Paletted`→`draw.Image` cast. Note that neither side is reliably the *longer* one, so
a "strip the chain" heuristic would not do: `go/types` records its OWN `Object` fully qualified where the
cast site renders it short, while `text/template/parse` records its own `Node` bare where the cast site
renders it whole. Which spelling a file produces depends on its own using/alias context — which is
exactly why a canonical form, and not either side's raw text, is the key. (2) The TYPE side: a record
carries the EMITTED C# name, so image/color's `RGBA` (collision-renamed against its own `RGBA()` method)
is `ΔRGBA` there, while the use side was naming the GO type. Both sides now reduce the emitted name.

The DECLARING-package component is what keeps a record honest. A package may record a value pair for a
type declared in a THIRD assembly — `image` re-declared all of image/color's models — and go2cs-gen
realizes that as a local adapter class, not as the type implementing the interface. The use side names
the TARGET's package, so such a record can never satisfy a cast (`image|Alpha|…` against
`color|Alpha|…`).

**The trust.** A record says the declaring assembly implements the pair; it does not say HOW.
`ImplementGenerator` makes every named Go type a `partial struct T : Iface` that really does implement
it — struct, slice (`[GoType("[]Color")] partial struct Palette`), map, channel, numeric
(`[GoType("num:nint")] partial struct ΔSignal`) — with exactly one exception: a named FUNC type arrives
as a C# **delegate**, which cannot be a partial struct, so its `TypeKind.Delegate` arm emits an adapter
CLASS in the declaring assembly instead. `valueRecordRealizesAsPartialStruct` gates on the target's Go
underlying being a non-`*types.Signature`, at the use site where `go/types` can still see it. Without
that gate the fix hands a bare delegate to an interface slot — CS0029 for net/http's
`HandlerFunc` → `ΔHandler` in `expvar`, `net/http/cgi` and three more.

**The POINTER set shares the key and needs no trust gate.** `[assembly: GoImplement<T, Iface>(Pointer =
true)]` is not "the declaring assembly implements this somehow" — it is exactly the shape
`ImplementGenerator` realizes as the public adapter class `<T>ж<Iface>`, so the record's existence *is*
the answer and there is nothing further to ask. (The delegate hazard that forces
`valueRecordRealizesAsPartialStruct` on the value side cannot arise: a pointer record already means the
adapter route was taken.) An earlier ruling kept this set's key un-collapsed on the reasoning that
matching a foreign record suppresses a LOCAL record the consumer needs; that hazard is real but it is a
*realization* question, not a *key* question, and on the pointer side it does not exist at all.

One consequence had to be fixed with the key, and only the collision-renamed types reach it: a foreign
type whose name is Δ-renamed resolves through a whole-TYPE `global using` alias (`imageꓸRGBA =
go.image_package.ΔRGBA`), which is a single identifier rather than a path — and the adapter is a MEMBER
of the declaring package's class, so composing onto the alias names nothing (`imageꓸRGBAжImage`, CS0246
×11 across five packages). The foreign-adapter arm therefore rebuilds a dotless base as the file's
package qualifier plus the type's EMITTED simple name — `image.ΔRGBAжImage`, which is exactly what the
declaring assembly's generator composed (`image/image.cs` reads `new ΔRGBAжImage(…)` for its own casts).

**Pointer footprint,** from a whole-stdlib A/B with both roots seeded (304/304 converted per side):
31 files, **66 constructions**, every one the same edit — `new <pkg>_<T>ж<Iface>(x)` becomes
`new <pkg>.<T>ж<Iface>(x)`, the declaring package's own adapter — plus the 37 `(Pointer = true)` records
that existed only to generate those local classes. Zero additions anywhere and the total adapter-
construction census is unchanged at 4348, so it is a one-for-one redirection, not a removal. By
declaring package: `text/template/parse`→`Node` 33, `go/types`→`Object`/`ΔType` 20, `image`→`Image` 4,
`net/http`→`RoundTripper`/`ΔHandler` 4, `net/url`→`error` 2, `net/textproto`→`error` 1,
`go/internal/srcimporter`→`types.Importer` 1, `go/build/constraint`→`Expr` 1. `go2cs-stdlib.slnx`
builds 0 errors on the overlaid tree.

The pointer form's symptom is milder than the value form's and worth stating precisely, because it is
what makes this an increment rather than a bug fix. The generated adapter's `Equals` compares
`IжAdapter.Box` by reference, so a redundant local adapter and the declaring assembly's own one still
compare equal and still alias the same object — no observable divergence was reproduced. What is wrong
is duplication plus a **non-deterministic dynamic type**: each adapter's module initializer calls
`AdapterRegistry.Register(typeof(ж<T>), typeof(Iface), …)`, which is first-wins, so which assembly's
class a type-assert re-wraps into depends on assembly load order. The value form's second-identity
failure (`image/png`'s `%v`, below) is the same defect one degree worse.

**Footprint,** from a whole-stdlib A/B with both roots seeded (302/302 converted per side): 13 files,
every changed line the same edit — `new <pkg>_<T>ᴠ<Iface>(x)` becomes `x` — plus the 16
`[assembly: GoImplement]` records that existed only to generate those adapters. **497 constructions**
go away (472 of them in `image/color/palette`'s two palette literals); the rest of the corpus adapter
census is identical count for count, `HandlerFuncᴠΔHandler` included. Two survivors are instructive
because they are NOT this defect: `color.Palette`→`color.Model` (5) and `encoding/binary`'s
`bigEndian`/`littleEndian`→`ByteOrder` (79) have no record to match at all — neither package ever
converts that pair itself, so nothing writes the record and the local adapter is the only realization.
A pair a package satisfies but never records is a separate root, closed by the section below.

This is not merely a wasted allocation. The adapter is a **second identity** for one Go value: `reflect`
and `fmt` see the wrapper where the Value's own type says the wrapped struct, which is how it surfaced —
`image/png`'s `diff` printing `%v` of a `color.Color` died with `System.ArgumentException: Field 'R' … is
not a field on the target object which is of type 'go.image_package+color_NRGBAᴠColor'`. (Guarded by the
`ForeignValueImplementSuppression` behavioral test — a sibling package at a multi-segment path that
converts its own values, a collision-renamed implementer, a second implementer, and a named FUNC type as
the live negative; the pre-fix converter emits five adapters where the fixed one emits the func's alone.
`ValueAdapterDynamicType` was its byte-identical complement — its sibling never converts, so its four
adapters were real — until the declaring side began recording pairs it merely satisfies (next section),
which is exactly that sibling's shape; its assertions now prove the bare value instead. The pointer form
is guarded by `ForeignPointerImplementSuppression`, whose sibling `tone` self-converts a
collision-renamed `*Tone` and an ordinary `*Plain` — both must reference tone's own adapters — against
two live negatives that must keep minting their own: `*Lone`, a pair `tone` satisfies but never records,
and `shade.Level`, an interface with the same SIMPLE name as `tone.Level`. The pre-fix converter emits
four local adapters there where the fixed one emits the two negatives' alone. Unit-guarded by
`TestImplementRecordKeyBothCompositionsAgree`,
`TestImplementRecordKeyKeepsPackageClassDiscrimination` and `TestValueRecordRealizesAsPartialStruct`.)

### A package records the pairs it SATISFIES, not only the ones it witnesses

Every `[assembly: GoImplement<T, Iface>]` the converter writes comes from a **cast it converted** —
`convertToInterfaceType` records the pair it just emitted. Go satisfies an interface **structurally**,
so a package can implement one of its own interfaces completely and never write a conversion:
`encoding/binary` declares `type bigEndian struct{}` with the whole `ByteOrder` method set and exports
`var BigEndian bigEndian`, with no `var _ ByteOrder = BigEndian` anywhere. No cast, no record — so
`binary_package.bigEndian` was emitted as a partial struct that does **not** implement `ByteOrder`, and
every consumer minted its own `binary_bigEndianᴠByteOrder` adapter. This is the one place where *the
declaring assembly implements this pair* is TRUE in Go and FALSE in the emitted C#, and it is the root
the section above measured but did not close.

`recordSamePackageImplements` (`samePackageImplements.go`, called from `processConversion` after
the file visits and before `writePackageInfoFile`) walks the package scope and records the VALUE-form
pairs the package satisfies. `encoding/binary`'s metadata gains:

```csharp
// <InterfaceImplementations>
[assembly: GoImplement<bigEndian, AppendByteOrder>]
[assembly: GoImplement<bigEndian, ByteOrder>]
[assembly: GoImplement<littleEndian, AppendByteOrder>]
[assembly: GoImplement<littleEndian, ByteOrder>]
[assembly: GoImplement<nativeEndian, AppendByteOrder>]
[assembly: GoImplement<nativeEndian, ByteOrder>]
// </InterfaceImplementations>
```

and every consumer hands over the bare value, its own record and adapter gone with it — `debug/dwarf`'s
`d.Value.order = new binary_bigEndianᴠByteOrder(binary.BigEndian)` becomes
`d.Value.order = binary.BigEndian`, and `crypto/x509`'s
`crypto.SignerOpts signerOpts = new crypto_HashᴠSignerOpts(hashFunc)` becomes
`crypto.SignerOpts signerOpts = hashFunc`.

It records **through** `convertToInterfaceType` with an EMPTY expression — the record-only probe path
`convCompositeLit` / `convTypeAssertExpr` / `visitValueSpec` already use, since every emission arm is
gated on `exprResult != ""`. That is the whole design: a synthesized pair is composed, keyed and pruned
exactly as a real cast would compose, key and prune it, so no second naming path can drift from the cast
site's — the divergence that made the FOREIGN lookup miss for six weeks. Scope names arrive sorted, so
the record order is deterministic across runs.

**Five gates bound it, and each one is load-bearing.**

* **The interface is EXPORTED.** A record is a CROSS-ASSEMBLY contract — it exists so another
  assembly's cast can drop its local adapter — and no other assembly can name an unexported interface,
  so a record for one could never be consulted. The package's own casts already record what it needs
  internally.
* **The target's underlying is NOT a `*types.Signature`.** A named FUNC type is a C# delegate, which
  cannot be a partial struct, so `ImplementGenerator` emits an adapter CLASS for it; a consumer trusting
  THAT record hands a bare delegate to an interface slot (CS0029 — net/http's `HandlerFunc` → `ΔHandler`).
  This is the declaring-side half of `valueRecordRealizesAsPartialStruct` above.
* **Neither side is GENERIC.** A type argument cannot appear in an assembly-attribute type argument
  (CS0246) — the same exclusion `convertToInterfaceType`'s `targetIsOpenGeneric` makes.
* **Both sides are declared in a file this run CONVERTS.** A package scope holds every file's
  declarations, including build-constraint-excluded ones, and a record naming a type no emitted file
  declares is CS0246.
* **Every interface method is REALIZABLE by the generator** — it resolves on the type itself or
  through at most ONE embedded field (`types.LookupFieldOrMethod` index length ≤ 2).
  `ImplementGenerator` forwards a promoted member through a single embed hop and says so ("Go's
  promotion ambiguity rules make multi-embed satisfaction rare; extend when needed"), so a deeper
  promotion emits a forwarder through the WRONG hop: `CrossPkgUser`'s `rig` embeds
  `CrossPkgLib.Device`, which embeds `Sensor`, where `Label` lives, and
  `CrossPkgLib_package.Label(this.Device)` is CS1503 for want of `this.Device.Sensor`. Promotion
  through an embedded INTERFACE is the common shape this still admits (`sort`'s `reverse` embeds
  `Interface`; `debug/macho`'s segment types embed `LoadBytes`). The bound is deliberately
  CONSERVATIVE rather than a model of the generator's exact reach — it costs exactly two stdlib
  records (`net`'s `tcpConnWithoutReadFrom`/`tcpConnWithoutWriteTo`→`Conn`, whose `*TCPConn` hop the
  generator's `embedHopDeepPaths` arm can in fact follow), neither of which has a consumer, and
  withholding a speculative record is always safe: the consumer keeps the adapter it had before.

The gates bind only the SPECULATIVE recorder. A pair the source actually casts is DEMANDED and still
records at its cast site — promotion depth and all — so none of this narrows existing behavior.

The **POINTER** method set was deliberately out of scope here — `types.Implements(*T, Iface)` is the far
larger set, its records are adapter-class *existence* signals with a different trust rule, and it was owed
its own increment with its own measured footprint. That increment has since landed; see the next section.

**Footprint,** from a whole-stdlib A/B with both roots seeded (302/302 converted per side, 3690 files
compared CRLF-normalized): **68 files**, split evenly between metadata and code. **33 records** appear
across sixteen declaring packages; **31** go away — three dropped by the existing interface-inheritance
prune because a newly recorded pair subsumes one a cast had recorded (`flag`'s `textValue`→`Value` under
`Getter`, and `net`/`runtime`'s `errorString`→`error` under their own `ΔError`, which embeds it), and
twenty-eight consumer-local foreign records that existed only to generate an adapter. **89 adapter
constructions** disappear across 34 files — exactly the census the previous section predicted, pair for
pair: `binary_bigEndianᴠByteOrder` 43, `binary_littleEndianᴠByteOrder` 36, `color_PaletteᴠModel` 5,
`crypto_HashᴠSignerOpts` 5. Every changed consumer line is the same edit, the adapter construction
unwrapped to its argument; the full stdlib solution builds with 0 errors.

Most of the 33 new records have no consumer today — they are the rule stating what Go already says
(`sort`'s `reverse`→`Interface`, `image`'s `Rectangle`→`RGBA64Image`, `debug/macho`'s five `Load`
implementers, `io`'s `discard`→`StringWriter`), and they cost one assembly attribute each. Notably
ABSENT is `net/http`'s `HandlerFunc`→`ΔHandler`: the delegate gate holds on the corpus instance that
motivated it.

Guarded by the `SamePackageImplementNoWitness` behavioral test — a sibling `ledger` package that
declares an exported interface and value implementers and never converts one to the other, with its
negatives live rather than asserted (a named FUNC type, an unexported interface, a generic; the
pointer-only implementer was a fourth until the next section made it a positive). The pre-fix converter
records nothing and mints four adapters where the fixed one mints one; the delegate negative
(`ledger_MeterᴠMetric`) is byte-identical across the fix. `CrossPkgLib`/`CrossPkgUser` and
`ValueAdapterDynamicType` carry the same shape and re-baselined to the bare value. The realizability gate
is guarded by COMPILE rather than by a golden, and by the corpus case that found it: `CrossPkgUser`'s
`rig` is the depth-2 promotion, so dropping the gate puts a `GoImplement<rig, Labeled>` back and the suite
goes red on CS1503 in the generated forwarder.

### The POINTER method set records the same way, for a different contract

The section above closed the VALUE half of "the declaring assembly implements this pair is TRUE in Go and
FALSE in the emitted C#" and named the POINTER half as owed. This is that increment.

**Why it is not just "the same rule with a bigger set."** A value record and a pointer record are consumed
differently, and the difference decides both the gates and the failure mode. A VALUE record licenses an
IMPLICIT conversion: the declaring assembly's `partial struct T : Iface` means a consumer hands over the
bare value and names nothing. A POINTER record is an adapter-class EXISTENCE signal — `Pointer = true` is
exactly the shape `ImplementGenerator` realizes as `<T>ж<Iface>` — and the consumer CONSUMES it by NAME,
emitting `new pkg.TжIface(x)` where it would otherwise mint its own `pkg_TжIface`.

**Why cast-site sourcing is not good enough, stated as the bug it caused.** Every record the converter
writes comes from a cast it converted, so a pair's record lives or dies with the ONE body that happens to
witness it. `syscall`'s three `Sockaddr{Inet4,Inet6,Unix} → Sockaddr` pairs are witnessed by exactly one
method body, `(*RawSockaddrAny).Sockaddr`, and hand-owning that single function — which the blittable-
mirror work has every reason to want — silently dropped all three `(Pointer = true)` records, after which a
reconvert of `net` minted `syscall_SockaddrInet4жΔSockaddr` beside `syscall`'s own. Nothing failed to
compile; the L10 lane found it only because it re-converted a dependent and diffed. The pointer form's
symptom is milder than the value form's `%v`-over-the-wrapper crash — both adapters wrap the same box and
compare equal — but it is a NON-DETERMINISTIC dynamic type: each adapter's module initializer calls
`AdapterRegistry.Register` first-wins, so which class a type assert re-wraps into follows assembly load
order. Sourcing the record from the METHOD SET makes the pair independent of which bodies a run converts,
which is the root fix rather than a rule about what may be hand-owned.

`recordSamePackageImplements` (the renamed `recordSamePackageValueImplements`) therefore asks BOTH
questions of every candidate and records both forms. The pointer set is a SUPERSET of the value set, so a
value-satisfied pair is recorded twice, and that is deliberate: Go's `T` and `*T` are two dynamic types,
realized as the partial struct and the adapter respectively, and dropping the pointer record for such a
pair leaves a consumer's `var i Iface = &t` with nothing to reference.

**The gates are the same five, plus one.** The added gate is the trust rule made mechanical:

* **BOTH sides are EXPORTED** (`pointerRecordIsPubliclyRealizable`). `ImplementGenerator` scopes the
  adapter class `public` only when the struct and the interface are each public, `internal` otherwise. A
  record is a cross-assembly contract, and this form's contract is *"this class exists and you may name
  it"* — so a record naming an unexported participant advertises a class no consumer can reference
  (CS0122), an existence signal that is a lie. The value form needs only the interface gate because its
  contract is realized by a conversion that names nothing, which is why the two rules differ here and only
  here.

The realizability gate is not merely re-asked of `*T` — it is TIGHTENED, and that is the second place
the two forms genuinely differ. `generatorCanForwardPointerMethodSet` requires every interface method to
resolve **DIRECTLY** on the type (index length 1), admitting no promotion at all, where the value bound
admits one embed hop. A partial struct's explicit implementation resolves a promoted member the way the
converter's own call sites do; the ж adapter does not. Its promoted-member arms are keyed on embedded
**POINTER** fields (`GetEmbeddedPointerHopNames`), and with exactly one such field the single-hop arm
takes every unbound member *unconditionally* — which the generator says outright, and is right to, because
for a DEMANDED record that member's promotion is what type-checked the cast. For a SPECULATIVE record it
is not: the member's true source may be a different embed entirely.

`StructPointerPromotionWithInterface`'s `MyCustomError` is the corpus instance, and the `go2cs.slnx` build
found it rather than reasoning did. It embeds BOTH the `Abser` interface and `*MyError`; `Abs` is promoted
from the INTERFACE, but the adapter's lone pointer embed is `*MyError`, so the generated forwarder bound
`Abs` against `MyError` — where the only candidate in scope was `time.Abs(Duration)`: **CS1929**, in a
generated file, naming `time` from a test about struct promotion. **Depth is not the discriminator** (that
promotion is index length 2, which the value bound admits); the KIND of hop is, and modelling the
generator's exact hop selection inside the converter would duplicate its internals in a second place —
the very drift this recorder's design exists to prevent. So the bound is conservative in the same spirit
as the value one and safe in the same way: withholding a speculative record leaves the consumer with the
local adapter it already had. A pair the source actually CASTS is untouched and keeps the full promotion
support the generator was built for, which is what that behavioral test guards. A named FUNC type is
excluded before either question is asked, as before.

**Footprint,** from a whole-stdlib A/B with both roots seeded (304/304 converted per side; marker gate
54 marked files / 43 `*_impl.cs` companions / **0 violations** on both roots): **75 files** — 35
`package_info.cs` and 40 code — with **0** `.csproj` and **0** `README.md` moved. **184 records appear**
across 22 declaring packages (`go/ast` 96, `image` 17, `io` 14, `image/color` 12, `database/sql` 8,
`net` 7, `math/rand/v2` 4, `sort` 3, …) and **117 go away**, every one a consumer-local duplicate the
declaring assembly now owns: `go/parser` 49, `go/types` 28, `go/doc` 8, `go/printer` 5 (all of them
`go/ast` node types), `net/http` 4, the five `debug/*` + `internal/xcoff` readers' `io.SectionReader`
pairs, `net/http/httputil`'s `io.Pipe{Reader,Writer}`, and one each for `sync.Mutex`→`Locker`,
`image/color.RGBA64`→`Color` and `parse.BranchNode`→`Node`. Net corpus movement is **+67** pointer
records (1,071 → 1,138), not the 548 the deferral's raw pair count suggested — most of that set was
already recorded from cast sites, and the gates take the rest.

**318 adapter constructions** are repointed across 40 files, every changed line the same edit —
`new pkg_TжIface(x)` becomes `new pkg.TжIface(x)` — which is the second-identity elimination measured:
318 sites that used to name a locally minted duplicate now name the declaring assembly's one adapter.
There is no third family; a classifier over the whole diff reports **zero** unclassified added lines.

Guarded by `SamePackageImplementNoWitness`, whose `*Tally → Metric` pair moved from negative to positive
(the consumer now references `ledger.TallyжMetric` instead of minting `ledger_TallyжMetric`) and which
gained the negative this gate needs — `tick`, an UNEXPORTED target whose pointer set implements the
exported interface, kept live through `ledger.Count` and absent from `ledger`'s metadata. Also guarded by
`ForeignPointerImplementSuppression`, where `Lone` — a pair `tone` satisfies and never casts — flipped the
same way and is now that test's proof that a record needs no witnessing cast, while its `shade.Level`
negative (a same-SIMPLE-named interface in another package, which must keep its local adapter) is
byte-identical across the change. Unit-guarded by `TestPointerRecordIsPubliclyRealizable`,
`TestGeneratorCanForwardMethodSetDepthBound` and `TestPointerMethodSetSubsumesValueMethodSet`.

**Acceptance witness — the L10 probe, re-run to prove the absence of what it once measured.** With
`RawSockaddrAny.Sockaddr` suppressed through `manualConversionFuncs` (a scratch build on each side, so
the only variable is the recorder), `syscall` and `net` were reconverted into seeded roots:

| | `syscall`'s `(Pointer = true)` Sockaddr records | `net`'s six construction sites |
|---|---|---|
| pre-increment converter + suppression | **absent** (all three) | `new syscall_SockaddrInet4жΔSockaddr(…)` — locally minted duplicates |
| post-increment converter + suppression | **all three present** | `new syscall.SockaddrInet4жΔSockaddr(…)` — syscall's own adapter |

The pre-increment row is the regression exactly as L10 measured it; the post-increment row is the same
probe finding nothing to report. That is what makes the hand-own safe rather than merely discouraged.

---

[← Interfaces](../interfaces.md) · [Index](../README.md)
