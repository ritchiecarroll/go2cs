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
[`recordSamePackageImplements`](../package-conversion.md#a-package-records-the-pairs-it-satisfies-not-only-the-ones-it-witnesses) already
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

---

[← Interfaces](../interfaces.md) · [Index](../README.md)
