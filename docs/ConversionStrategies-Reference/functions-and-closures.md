# Function Values and Closures

[Reference index](README.md) · [Summary of this topic](../ConversionStrategies.md#function-values-and-closures)

This page covers Go function values in C#: func types and the delegates they lower to, func literals and the result types they state, and how a closure captures the variables it uses.

## Function types and delegates

### A methodless named func type renders as its base delegate

Go treats a named func type as freely interconvertible with its underlying `func(...)` when the
type has **no methods** — the name is purely documentary. `type releaseConn func(error)`
(database/sql) and `type CancelFunc func()` (context) are assigned to and from anonymous
`func(...)` values without conversion: `grabConn` returns `releaseConn`, `queryDC` takes
`func(error)`, and Go passes one to the other. Emitting the named type as a *distinct* C#
delegate (`ΔreleaseConn`) broke this — the base `Action<error>` its underlying renders to has no
implicit conversion to it (CS1503/CS0029), and the mismatch even excluded the `ж`-receiver
overload of methods taking such a param, so `db.pingDC(...)` on a boxed `*DB` failed with CS1929.

A **non-generic** named func type with **no methods** is therefore rendered AS its base C#
delegate (`Action`/`Func<…>`) everywhere it is referenced (`getAliasQualifiedTypeName`/`getFullyQualifiedTypeName` return
the underlying signature), and its declaration is skipped (`visitFuncType` emits only a marker
comment). Every named↔underlying conversion becomes identity, exactly as Go models it:
```csharp
// type releaseConn is a methodless func type — rendered inline as its base delegate
internal static (ж<driverConn>, Action<error>, error) grabConn(this ж<ΔConn> Ꮡc, context.Context _) { … }
internal static error queryDC(this ref DB db, …, Action<error> release, …) { … }
```
Three exclusions keep the collapse sound — a type is left as a named delegate if any holds:
- it **has methods** (its method set is meaningful — the `FirstClassFunctions`/`hashFunc` wrap case
  below still applies);
- it is **generic** (it is referenced as `Seq<V>`, and the type parameter must stay in scope — see
  the generic-`Seq` range-over-func case);
- its **signature references another named func type, including itself**. A *self-referential* func
  type — `type stateFn func(*machine) stateFn` (a Go state machine, `NamedFuncTypeStateMachine`) —
  has no finite base-delegate form (`Func<M, Func<M, …>>` is infinite); and a reference to another
  named func type (`strategy func(score) action`) would leave that name undefined after collapse.
  Only the *leaves* of the func-type reference graph collapse; a referencing type stays named and
  renders the collapsed leaf inside its own signature.

Because the collapse applies at both the declaration and every reference, and to foreign types too
(context's `CancelFunc` collapses in context's own conversion, so database/sql sees `Action`),
consistency holds across packages. One position needed a companion fix: a variadic `...Option`
element is package-class-qualified (`main_package.Option`) for a package-local named type, which
would mangle a collapsed delegate to `main_package.Action` (CS0426) — `variadicElementType` now
skips the qualifier when the element collapsed. Cleared 13 of database/sql's 17 errors (the whole
named-func family + the CS1929 it masked). Guarded by `MethodlessFuncType` (a function returning
the named type, one taking the anonymous underlying, a struct field, and a tuple-deconstruction
seam across the two); regression-checked against the self-referential (`NamedFuncTypeStateMachine`,
unchanged), nested-reference (`FirstClassFunctions`), and variadic-param (`PublicizedFuncTypeParam`)
cases.

**A collapsed methodless func type must NOT export a `[GoTypeAlias]`.** When such a type is *also*
collision-renamed — `type Filter func(...)` alongside a method `Filter` (go/ast's `Filter` vs
`(CommentMap).Filter`; the `ReservedTypeMethodCollision` shape) — the rename records an exported
`[assembly: GoTypeAlias("Filter", "ΔFilter")]` so consumers can name the renamed type. But because
the type collapses to its base delegate, **no `<pkg>_package.ΔFilter` type is ever emitted** — so a
consumer that loads the alias generates `global using astꓸFilter = go.go.ast_package.ΔFilter;`
naming a nonexistent type (go/doc referencing `ast.Filter`, CS0426). `visitFuncType` now records
each collapsed methodless func type's name in `packageInlineFuncTypeNames`, and the exported-type-
alias emission skips any alias whose key *or* value matches (the collision path stores the alias
under the renamed value `ΔFilter`, the plain path under the raw name) — so the alias is never
exported and the consumer renders `ast.Filter` inline as `Func<nint, bool>` through the normal
collapse. (Guarded by the `CrossPkgUser` extension — a cross-package `CrossPkgLib.Sift` methodless
func type colliding with a `Sift` method, named as a var type and rendered inline, output vs Go; and
by `ReservedTypeMethodCollision` whose `[GoTypeAlias]` is now correctly absent.)

When such a collapsed delegate's signature carries a parameter whose type lives in a **sub-package**
(an import path with a slash), the `Func<…>`/`Action<…>` rendering must qualify that type as the
package **class**, not the namespace. The collapsed signature is produced from the Go signature's
`t.String()`, which keeps the canonical import PATH inline — `func(*sync/atomic.Int32) int32`,
`func(string, io/fs.DirEntry, error) error` (path/filepath's `WalkDirFunc`) — losing the file's import
alias. `convertToCSFullTypeName` converted the whole slash-bearing string as one import path, dotting
the type straight into the namespace: `sync.atomic.Int32` / `io.fs.DirEntry` — CS0234, since `atomic`
is not a namespace of `go.sync` (the type lives in class `atomic_package`). It now splits the trailing
`.TypeName` off at the first `.` after the last path `/`, converts the package path with the class
suffix, and re-appends: `sync.atomic_package.Int32`, `io.fs_package.DirEntry`. The suffix is only added
when the path segment does not already carry it — some callers (a recorded `[GoType]` underlying,
`sync/atomic_package.Uint32`) hand a pre-suffixed path, which would otherwise double to
`atomic_package_package` (a `DefinedTypeOverPkgType` regression, caught and gated). The behavioral
corpus is byte-identical except the intended change, and an A/B reconvert of net+go/types (same package
set) is byte-identical — only the func-type-subpackage-param shape moves. (Guarded by the
`SubpackageFuncTypeParam` behavioral test — a methodless `applyFunc func(*atomic.Int32) int32` whose
collapsed delegate carries the `sync/atomic` sub-package parameter, output-compared vs Go; the same
shape drives path/filepath's `WalkDir`/`Walk` referencing `io/fs.DirEntry`/`FileInfo`.)

A collapsed func type's **parameter list must not be double-converted**. `convertToCSFullTypeName`'s
`func(` handler split the parameter string with `extractTypes`, then re-ran `convertToCSTypeName` over each
result — but `extractTypes` already renders a NAMED parameter in C# form (it strips the Go name and converts
the type). Re-feeding an already-C# `map<@string, ж<Object>>` through the `map<` arm's `splitMapKeyValue`
mis-parsed it into `map<@string, ж<Object>, >` — a spurious trailing empty type arg (CS1031 "Type expected",
go/ast's `NewPackage` taking `type Importer func(imports map[string]*Object, path string) (…)`). The fix makes
`extractTypes` **always** return C#-form (the bare-type/unnamed branch now converts in place too, matching the
named branch), and the caller trusts that output directly instead of a second pass. This is byte-identical
everywhere except named-parameter func types — bare-type func types (`func(int, string)`) were already
converted once and stay so, just at the `extractTypes` site rather than the caller. (Guarded by the
`NamedFuncTypeMapParam` behavioral test — `type Importer func(imports map[string]*Node, path string) (pkg
*Node, err error)` used as a function parameter, output-compared vs Go; CNR byte-identical across the corpus,
and an A/B reconvert of go/ast shows only that one collapsed-delegate parameter shape moving.)

**A collapsed methodless named func type's DELEGATE TYPE renders through `iifeDelegateType`, not the string
path.** The double-conversion fix above kept the collapsed delegate on `convertToCSFullTypeName`'s `func(`
string handler — but that string domain naively slash→dots a cross-package element's import PATH. go/doc
passes `simpleImporter` to `ast.NewPackage` (whose `importer` is `ast.Importer`, a methodless func type), so
the converter wraps the method group in the collapsed base delegate
`new Func<map<@string, ж<go.ast.Object>>, @string, (ж<go.ast.Object> pkg, error err)>(simpleImporter)` —
`go/ast.Object` mangled to `go.ast.Object` (no `_package` class, no file alias), so `ast` is not a namespace
of `go` (CS0234 ×2), and the resulting error-typed delegate then fails the method-group→delegate conversion
(CS0123). `getCSharpTypeName` now routes a methodless named func type through the SAME structural
`iifeDelegateType` path an ANONYMOUS signature already takes (that path exists precisely because the string
path mangles slash-bearing package paths), naming each element via `aliasedElementTypeName` — so the
cross-package `ast.Object` keeps its `ast` alias (and a Δ-renamed foreign element its recorded `ꓸ`-alias):
`new Func<map<@string, ж<ast.Object>>, @string, (ж<ast.Object>, error)>(simpleImporter)`. The only visible
change for a SAME-package/single-segment element is that a multi-result signature's delegate type drops its
Go result-tuple element NAMES (`(ж<Node> pkg, error err)` → `(ж<Node>, error)`), matching how anonymous
signatures already render — cosmetic, both compile. An A/B full-stdlib reconvert moves **11 files, all
equal-or-better**: the go/doc mangle fixed, plus `go/parser`/`go/scanner` (`go.token_package.ΔPosition` →
`tokenꓸPosition`), `go/internal/gccgoimporter` (a malformed `(io.ReadCloser>, error)` → valid),
`internal/trace/traceviewer` (`net.http_package.Request` → `http.Request`), and `path/filepath`
(`io.fs_package.DirEntry` → `fs.DirEntry`) all cleaned up, with `bufio`/`go/ast`/`nettest` only dropping
cosmetic tuple names; CNR touches only three existing goldens (`NamedFuncTypeMapParam`,
`SubpackageFuncTypeParam`, `FirstClassFunctions`), all the same pattern. (Guarded by the `CrossPkgUser`
extension — a package-level `simpleResolve` passed as a METHOD GROUP to `CrossPkgLib.Resolve`, whose
`Resolver` is a methodless func type naming the cross-package `*CrossPkgLib.Node`, so the wrapped delegate
renders `ж<CrossPkgLib.Node>` via the alias, output-compared vs Go. The single-segment producer compiles
either way, so the byte-golden — unnamed vs Go-named result tuple — is what guards the routing; the exact
slash-bearing CS0234/CS0123 needs a multi-segment producer like go/ast, verified by the go/doc source A/B.
go/doc's own remaining block is the SHARED generated-adapter forwarding of go/ast's unexported interface
marker methods — a separate root.)

A companion root cleared path/filepath fully: a **cross-package type ALIAS whose target lives in yet
another package** — `os.FileInfo = fs.FileInfo` (os/types.go, target in `io/fs`) — is emitted as an
assembly-scoped `global using FileInfo = go.io.fs_package.FileInfo;` in **os's own** conversion, never as
a member of the os package's C# class, so a cross-package reference `os_package.FileInfo` does not resolve
(CS0426, path/filepath's `lstat = os.Lstat` func value). `getAliasQualifiedTypeName` now renders such an alias as its
**target** — `os.FileInfo` → `fs.FileInfo` (→ `io.fs_package.FileInfo` via the file's `fs` using). Gated
to a **different-package target**: an alias to a SAME-package type (`CrossPkgLib.Temperature = Celsius`)
already resolves through the existing `ꓸ` global-using alias (`CrossPkgLibꓸTemperature`) and is left
untouched — narrowing here reverted a `CrossPkgUser` churn the blanket form caused. CNR byte-identical;
an A/B of os+io/ioutil (same package set) shows only that one intended resolution (io/ioutil's `ReadDir`
sort lambda moved `osꓸFileInfo` → `fs.FileInfo`, matching the file's other `fs.FileInfo` refs — still
compiles). **GUARD OWED** — the shape needs three packages (B declares `Y`, A aliases `type X = B.Y`, C
references `A.X`), which neither the single-package baseline nor the 2-package `CrossPkg` harness
expresses; validated by the `core/path/filepath` build (1→0) + io/ioutil build.)

**A func type renders structurally in EVERY type-name path — the signature never stringifies.**
`getAliasQualifiedTypeName` now carries a `*types.Signature` arm (`signatureTypeName`, beside `iifeDelegateType`)
mirroring the slice/map/chan composite arms: Go syntax — `func(name type, …) results` — with every
parameter/result type resolved **recursively**, so a cross-package element keeps the file's short
import alias exactly like the neighboring map/slice fields. Previously only *some* positions routed
through the structural `iifeDelegateType` (var declarations; variadic or slash-bearing struct
fields); every other position — a struct field of a **named** methodless func type (go/importer's
`importer gccgoimporter.Importer`), a MAP field's func **value** type (net/http's `TLSNextProto`
maps), a same-package named func field (traceviewer's `f MutatorUtilFunc`) — reached
`convertToCSFullTypeName` as `t.String()` text with import PATHS inline, and the slash heuristics
mangled those one of **three ways** depending on the string's shape: the whole-string
path-conversion arm fires when no dot-after-slash precedes the first `[` (a leading `map[` bracket),
naively dotting every path — `ж<go.types.Package>`, `ж<crypto.tls.Conn>` (no `_package` class, and
under a `go.go`-nested namespace the leading segment binds the child namespace — CS0234) — while the
split-at-dot arm mangles a mid-signature path to a classed-but-unrooted form
(`@internal.trace_package.UtilFlags`, traceviewer mmu.cs). With the structural arm the string
reaching the parser is slash-free (`func(*Server, *tls.Conn, Handler)`) and each element converts
through the normal alias route. Result NAMES are preserved, so a named multi-result field keeps its
named C# tuple (the display-path advantage the old struct-field routing existed to protect); a
same-package/builtin signature renders byte-identically to the old `t.String()` path (zero churn —
CNR confirmed across all 331 behavioral projects). A variadic tail renders `...elem` (which the old
path's `..`-strip reduced to the unparseable `.elem`) and lowers through the parser to the golib
`ꓸꓸꓸ` delegate family (next paragraph). One side effect: element recursion passes through
`getAliasQualifiedTypeName`'s foreign-ALIAS arm, so a signature naming a cross-package alias (`os.FileInfo` →
`io/fs.FileInfo`) now registers the **target's** package for a file-local using — a few stdlib files
gain a benign `using fs = …;` alias line (`collectTypePackages`' Named case does not match a
`*types.Alias`, so the old path never registered it). Whole-stdlib A/B footprint: 20 files — the
go/importer field fixed, `ж<tls.Conn>` in net/http server/transport/h2_bundle (field + composite
literals), traceviewer's `Func<trace.UtilFlags, (slice<slice<trace.MutatorUtil>>, error)>`,
go/scanner's `err` field moving to the canonical `tokenꓸPosition` alias (the old
`go.token_package.ΔPosition` resolved only by go.go-namespace luck), the variadic type-assert
target below, one comment-alignment shift, and the benign using-line additions. Cleared the
IMP-2/HTTP-3 CS0234 cluster (net/http ×8 + go/importer ×6 + traceviewer). (Guarded by the
`SynthesizedDelegateChildPkg` behavioral test — a nested CHILD subpackage (slash-bearing import
path) whose `*inner.Record` rides a named methodless func-type field with a nested-tuple lookup
param AND a `map[string]func(*inner.Record, string)` field, both invoked at runtime vs Go.)

**The func-type string parser splits parameters at TOP-LEVEL commas only — and a variadic tail
lowers to the `ꓸꓸꓸ` delegate family.** `extractTypes` split the parameter list with a naive
`strings.Split(signature, ",")`, so a nested func param returning a TUPLE — `lookup func(string)
(io.ReadCloser, error)` (go/internal/gccgoimporter's `Importer`, surfacing as go/importer's
`gccgoimports.importer` field) — shredded at the tuple's interior comma, unbalancing the assembled
delegate: `Func<@string, (io.ReadCloser>, error)` (the inner `>` closes before the tuple's second
element — a 6-error syntax cascade, IMP-1). `splitTopLevelParams` tracks `<>`/`()`/`[]`/`{}` depth
(with the channel-arrow `<-` guard `splitMapKeyValue` already carries) and splits only at depth 0.
On top of that, a variadic tail (`...elem`, from the structural render above) converts its ELEMENT
type in `extractTypes` and carries an ellipsis-family marker that the `func(` assembler hoists into
the delegate FAMILY name — `Actionꓸꓸꓸ<@string, any>` — mirroring `iifeDelegateType`'s lowering
exactly. That fixed the variadic func type as a type-ASSERTION target as a rider:
`.(func(string, ...any))` (net/http transport.go's `tLogKey` logger) previously emitted the
unparseable `._<Action<@string, .any>>(ᐧ)` and now renders `._<Actionꓸꓸꓸ<@string, any>>(ᐧ)`.
(Guarded by the `FuncFieldNestedTupleParam` behavioral test — builtin-typed struct fields with
nested-func-returning-tuple params in both the anonymous and named-collapse forms plus a
named-tuple-result sibling, all invoked at runtime vs Go.)

A **type ASSERTION** whose target is a methodless func type must assert against the **collapsed
delegate**, not the (never-emitted) name. `ci.(Compressor)` where
`type Compressor func(io.Writer) (io.WriteCloser, error)` (archive/zip's compressor/decompressor
registries) rendered `ci._<Compressor>()` — `convTypeAssertExpr` converts the target via `convExpr`,
which emits the bare ident, and after collapse `Compressor` is undefined (CS0246). When the asserted
target is a methodless named func type, the assertion now renders its `getCSharpTypeName` (the collapsed
`Func<…>`): `ci._<Func<io.Writer, (io.WriteCloser, error)>>()` — matching how the stored value was
emitted (a collapsed delegate). Other assertion targets are unchanged. (Guarded by the
`MethodlessFuncTypeAssert` behavioral test — `i.(Compressor)` on a matching and a non-matching dynamic
type, output-compared vs Go; CNR byte-identical and an A/B of archive/zip shows only the two intended
`_<Compressor>`/`_<Decompressor>` → `_<Func<…>>` lines.)

An **UNINITIALIZED local `var` of a methodless named func type** renders its declared type through the
same structural path. `visitValueSpec`'s no-initializer branch computed the type from
`convertToCSTypeName(getAliasQualifiedTypeName(...))` (the string path) and only re-routed a bare *anonymous*
`*types.Signature` through `getCSharpTypeName`; a methodless NAMED func type is a `*types.Named`, so it kept
the string render — and that render mangles a slash-bearing cross-package element. go/parser's `parseDecl`
declares `var f parseSpecFunction`
(`type parseSpecFunction func(doc *ast.CommentGroup, keyword token.Token, iota int) ast.Spec`), which
emitted `Func<ж<go.ast.CommentGroup>, go.token.Token, nint, go.ast_package.Spec> f = default!;` — the
`go.ast`/`go.token` elements re-root to the nonexistent `go.go.ast`/`go.go.token` (CS0234), and the
declared delegate then mismatched the lambdas assigned to `f` and the `parseGenDecl(keyword, f)` parameter,
which render the SAME Go types structurally as `ast.CommentGroup`/`token.Token` (CS1661/CS1678/CS1503 — 12
errors, all this one declaration). The no-initializer branch now routes a func-typed var (anonymous
signature OR methodless named func, via `methodlessNamedFuncSignature`) through `getCSharpTypeName` →
`iifeDelegateType`, whose `aliasedElementTypeName` keeps each element's `pkg.Type` alias:
`Func<ж<ast.CommentGroup>, token.Token, nint, ast.Spec> f = default!;`. This precedence matches
`getCSharpTypeName`'s own — the func render wins over the foreign-alias route (which for a methodless named
func would point at the SKIPPED delegate declaration); a non-func foreign-renamed local keeps its alias
unchanged. An A/B full-stdlib reconvert moves exactly one file (go/parser/parser.cs), greening go.parser
outright. (Guarded by the `MethodlessFuncType` extension — an uninitialized `var find lookup` where
`type lookup func(string) (path string, ok bool)`; the byte-golden captures the structural render
`Func<@string, (@string, bool)>` — dropping the Go result NAMES the string path keeps — output-compared vs
Go. As with the delegate-routing sibling above, a single-segment/same-package producer compiles either way,
so the unnamed-vs-Go-named result tuple is what guards the routing; the exact slash-bearing CS0234 needs a
multi-segment producer like go/ast, verified by the go/parser source A/B.)

### Named delegate types wrap mismatched initializers
A NAMED func-type field initialized with a value of a DIFFERENT delegate type has no implicit
C# conversion: internal/concurrent's `keyHash: mapType.Hasher` feeds a `hashFunc` field from a
`Func<…>` field. The composite-literal walk resolves each element's field BY NAME (keyed-aware)
and wraps mismatched delegate values in the target delegate's constructor —
`keyHash: new hashFunc((~mapType).Hasher)` (the wrap splits a C# named-argument label first).
FuncLit and nil initializers stay bare. Guarded by `FirstClassFunctions`
(`handler`/`provider`/`registry`).

### A named delegate value passed to a structural func parameter re-wraps
The MIRROR of the argument-position named-delegate wrap: a **structural** (written-anonymous) func
parameter receiving a value of a **named** delegate type — net/http h2_bundle's
`sc.scheduleHandler(…, handler)`, where `handler` is `HandlerFunc` and the parameter is
`func(ResponseWriter, *Request)` (CS1503). Go converts named→structural implicitly; C# needs the
same delegate re-wrap, targeting the synthesized structural delegate:

```go
type Handler func(int, string) string   // has a method → distinct C# delegate
func invoke(f func(int, string) string, n int, s string) string { return f(n, s) }
var h Handler = describe
invoke(h, 1, "a")
```
```csharp
invoke(new Func<nint, @string, @string>(h), 1, "a"u8);
```

Two argument shapes render named and take the wrap: a value whose **Go type** is a named func type
(with methods), and a `:=` local **declared from a method group**, which the declaration emission
types with the matching package named delegate (`HandlerFunc handler = Ꮡsc.Value.handler.ServeHTTP;`
— the bare-function-value `:=` rule above) even though go/types keeps it structural — the exact
h2_bundle shape. A **methodless** named func type already *renders* as the structural delegate
(`methodlessNamedFuncSignature` collapses it — same C# type), so it stays bare; method groups and
func literals themselves convert natively. A generic structural parameter (unsubstituted type
params) also stays native. (Guarded by the `NamedDelegateStructuralParam` behavioral test —
named-with-method and method-group-declared locals wrapped, methodless/method-group/func-literal
controls bare, values vs Go.)

The same mirror applies to a **composite-literal FIELD** (2026-07-17; sort's test-suite
conversion): the composite walk previously wrapped only the named-field ← different-delegate
direction, so GOROOT sort example_keys_test's `planetSorter{planets: planets, by: by}` — a `By`
value (named, with a `Sort` method) initializing the written structural field `by func(p1, p2
*Planet) bool` — emitted the bare `by: by` against the `Func<ж<Planet>, ж<Planet>, bool>`
constructor parameter (CS1503; the Phase-4 blocker-map row B10b). The structural-field arm now
applies the identical named-rendering test and wrap: `by: new Func<ж<Planet>, ж<Planet>,
bool>(by)`. Method groups, func literals, and nil stay bare, and generic fields stay native, as
at call sites. (Guarded by the `NamedFuncTypeStructuralField` behavioral test — the By-with-method
sorter pattern wrapped, a method-group field initializer control bare, values vs Go.)

### Func-typed fields with a cross-package (slash-path) type render structurally
A func-typed struct field whose signature names a type from a **multi-segment** import path —
testing/quick's `Config.Values func([]reflect.Value, *rand.Rand)`, where `rand` is `math/rand` —
must render as a structural `Action`/`Func<…>` delegate via `getCSharpTypeName`, not through the
string display path. The display path stringifies the signature as `func([]reflect.Value,
*math/rand.Rand)` and splits the slash-bearing import path on `/`, emitting the dotted
`math.rand.Rand`; but `math` aliases to `math_package`, so `math.rand` resolves to the nonexistent
`math_package.rand` (CS0426). The structural renderer recurses per signature element and qualifies
each named type by its package **name**:

```go
type Config struct {
    Values func([]reflect.Value, *rand.Rand)   // rand is math/rand
}
```
```csharp
public Action<slice<reflectꓸValue>, ж<rand.Rand>> Values;
```

The re-routing is gated on the signature string containing `/` **or the signature being variadic**:
the string path cannot render a variadic signature at all — `getAliasQualifiedTypeName`'s `..` strip reduces the
ellipsis of go/build's `JoinPath func(elem ...string) string` (Context, build.go:84) to `.string`,
emitting the unparseable `Func<.@string, @string>` (CS1031 + CS1003 ×2, all three go.build errors),
and even unstripped it has no variadic lowering. Structurally such a field renders the golib
variadic delegate family (`public Funcꓸꓸꓸ<@string, @string> JoinPath;` — see the variadic-lowering
section below), which loose-arg, empty and spread calls through the field all bind. Every other
func field keeps the display path: `func(string) (importPath string, ok bool)` preserves its named
tuple elements that the structural renderer drops. (Guarded by the `FuncTypeParam` behavioral
test's `runner.gen` field, and by `VariadicFuncFields` — a struct with variadic func-typed fields
assigned from a named func and func literals, called loose/empty/spread — for the variadic arm.)

### A variadic func type lowers to the golib `Actionꓸꓸꓸ`/`Funcꓸꓸꓸ` delegates

A **variadic function TYPE used as a value** — a parameter, variable, struct field, or collapsed
methodless named type such as go/types' `reportf func(format string, args ...interface{})` — used
to have three mutually incompatible lowerings: the delegate type rendered `Action<@string,
slice<any>>` (no `params` — the BCL `Action` cannot express one), a variadic func LITERAL emitted
the named-function convention `(@string format, params ꓸꓸꓸany argsʗp) => …` (CS1661/CS1678
against that `Action`), and calls through the value passed loose Go-style args as if `params`
existed (`reportf("…"u8, (~f).typ)` — CS1503; `reportf("empty type set"u8)` — CS7036).

The lowering now targets a golib delegate family carrying a real C# 13 `params Span<T>` tail
(`src/core/golib/variadic.cs`; fixed-arity prefixes up to eight mirror the BCL Action/Func family,
and the `ꓸꓸꓸ` suffix reads as Go's `...`):

```csharp
public delegate void Actionꓸꓸꓸ<T1, TArg>(T1 arg1, params Span<TArg> args);
public delegate TResult Funcꓸꓸꓸ<T1, TArg, out TResult>(T1 arg1, params Span<TArg> args);
```

`iifeDelegateType` — the single structural lowering every `getCSharpTypeName(*types.Signature)` and
collapsed methodless named func type routes through — names the family when `sig.Variadic()` and
passes the variadic **element** type as the last type argument. Everything else then agrees with
**zero changes** to the other emissions, because the parameter types match by identity
(`ꓸꓸꓸT` *is* `Span<T>`):

- the named-function convention (`internal static @string gather(@string prefix, params ꓸꓸꓸnint
  valsʗp)`) converts as a method group — `apply(gather)` stays bare;
- a variadic func literal (`(@string prefix, params ꓸꓸꓸnint valsʗp) => …`, C# 13 params lambda)
  converts natively — go/types' `comparable(typ, true, default!, (@string format, params ꓸꓸꓸany
  argsʗp) => {…})` now binds its `Actionꓸꓸꓸ<@string, any>` parameter;
- calls through the value pass loose args or an empty tail via C# `params` expansion, and a Go
  spread (`f(nums...)`) binds the slice's `.ꓸꓸꓸ` Span in normal form;
- a C# consumer calls a transpiled printf-style callback naturally (`ctx.Logf("…", a, b)`) — the
  library use case that ruled out the pack-into-a-`slice<T>` alternative.

A `:=`-declared variadic func literal is untouched: it keeps C#'s natural (params-capable) lambda
type under `var` (the `VariadicClosureSpread` shape). One deliberate residue: `defer`/`goǃ` of a
call **through a variadic func value** would need to capture the `Span` tail, which a ref struct
cannot be — pack into a slice at such a site. ⚠ **That residue now has its one demonstrated
consumer, and it is a TEST file** (found 2026-08-19, lane `claude/variadic-call`): the census of
the whole Go 1.23 tree finds exactly ONE `defer`/`go` of a variadic func literal —
`html/template/examplefiles_test.go:90`, `defer func(dirs ...string){…}(dir1, dir2)` — which emits
`defer((params ꓸꓸꓸstring dirsʗp) => {…}, dir1, dir2, ref ᒐ)` and fails inference against
`builtin.defer<T1,T2>(Action<T1,T2>, T1, T2, ref GoFrame)`: **CS0411**. That is why the claim used
to read "no stdlib occurrence" — the original A/B was over PRODUCTION sources, and the shape lives
only in a `_test.go`, so nothing before the Phase-4 `-tests` pipeline could see it. It is one of the
two roots now blocking `html/template`'s 243 verdicts. Full-stdlib
A/B footprint: go/types predicates.cs/expr.cs plus every file that renders a variadic func type
structurally (inspected file-by-file at introduction). (Guarded by `VariadicFuncValues` — a named
func AND a func literal satisfying a variadic func-typed param, loose/empty/spread calls through
it, and a nil-compared variadic func-typed var — output-compared vs Go.)

**A type-ASSERTION target routes through the same structural lowering.** `convTypeAssertExpr` rendered
the asserted type by converting the TYPE EXPRESSION through the string-based type-name path, which
skips the variadic lowering above — net/http transport.go's
`cw.(func(string, ...any))` emitted `._<Action<@string, .any>>(ᐧ)` with a literal `.any` (CS1001, the
`...` mangled instead of lowered). An anonymous-signature assert target now renders through
`getCSharpTypeName` → `iifeDelegateType`, exactly like the collapsed methodless NAMED func target already
did: `._<Actionꓸꓸꓸ<@string, any>>(ᐧ)`. Non-variadic signatures render identically on both paths, so the
only full-stdlib delta is the transport.cs site. (Guarded by `VariadicFuncTypeAssert` — a positive
variadic assert invoked through the asserted value, a negative assert on a non-func value, and a
non-variadic anonymous func assert, output-compared vs Go.)

**…and the BOXING side needs the matching cast, or the two can never meet (2026-08-20).** Rendering
the assert target through `iifeDelegateType` fixes the *reading* half; the *writing* half is where the
value acquires a dynamic type, and for a variadic func that type is C#'s, not Go's. C# gives a method
group or lambda at an untyped destination a **natural function type**: for a non-variadic signature
that is `Func<…>`/`Action<…>` — go2cs's own lowering, so the two already agree and nothing is emitted
— but a `params` signature has no BCL delegate, so C# **synthesizes** one and the box carries
`<>f__AnonymousDelegate0` forever. html/template's `funcMap` is `map[string]any` of `func(...any)
string` escapers assigned as method groups, and its own `TestRedundantFuncs` reads them back with
`funcMap[n].(func(...any) string)`: `interface conversion: interface {} is <>f__AnonymousDelegate0,
not go.Funcꓸꓸꓸ<object, @string>`. The assert was right, the box was wrong, and both were emitted by
the same converter.

So a variadic func entering EMPTY-INTERFACE space is cast to its Go func type at the boundary —
`((Funcꓸꓸꓸ<any, @string>)(attrEscaper))` — which is the same carry-your-Go-type rule the pointer box
and the untyped-constant box already apply at that same finite set of slots, and it lives with them in
`typedNilInterfaceBoxing.go`. Both sides now name the type through `getCSharpTypeName` →
`iifeDelegateType`, one renderer, so they cannot drift. The cast is a no-op wherever the value already
has that type (a typed var, a call result), so it widens nothing; a NON-empty interface target needs
nothing either, since a bare func type has no methods and satisfies no other Go interface. (Guarded by
the extension to `VariadicFuncTypeAssert` — a variadic func literal direct to `any`, a variadic method
group as a `map[string]any` element, through a plain assignment, and as an `[]any{…}` element, each
asserted back; plus a NON-variadic literal direct to `any` as the control that must keep matching
without a cast. Neutering the cast prints `no match` on all four and leaves the control passing.)

**A variadic METHOD VALUE was the one shape in the family still frozen at fixed arity (2026-08-26).**
`errorf := t.Errorf` — go/types' and `slices`' own idiom, `errorf = t.Logf` one statement later, then
loose Go-style calls — has TWO emissions, and both dropped the variadic tail. A bound method value
forwards through a lambda carrying the method's own parameters, and that lambda rendered the tail as
the plain `slice<T>` the signature *stores* rather than the `params ꓸꓸꓸT` convention every declared
variadic function uses: `(@string p1, slice<any> p2) => Ꮡt.Errorf(p1, p2)`. Every call through the
value was then an arity error — `errorf("…", n)` CS1503 on a bare `n` against `slice<any>`,
`errorf("…", a, b)` CS1593 "does not take 3 arguments", `errorf("…")` CS7036 — which is the same
family the lambda's explicit parameters were introduced to fix, one level in. The tail now renders
through `variadicParamType`, the same routine the named-function convention uses (a file-local
`using ꓸꓸꓸT = Span<…>;` alias where one is mintable, inline `Span<T>` otherwise), so the forwarded
argument binds the receiving `params ꓸꓸꓸany` parameter directly and the call inside the lambda is
unchanged.

The DECLARATION is the second half, and it is not optional. A `params` lambda has no BCL delegate, so
`var` gives it a **synthesized** natural type — which binds that lambda and gives C# no reason to hand
the same type to the second lambda the reassignment installs. `visitAssignStmt`'s method-group branch
therefore names golib's variadic delegate family when the signature is variadic and no package named
func type matches — `Actionꓸꓸꓸ<@string, any> emit = (@string p1, params ꓸꓸꓸany p2) => …` — reusing
`iifeDelegateType`, the same lowering `getCSharpTypeName` already gives every func type used as a
value, so there is exactly one spelling of this type in the emission. Non-variadic method values keep
`var`, unchanged. (Guarded by the `VariadicFuncValues` extension — a pointer receiver's variadic
method bound by `:=`, conditionally reassigned to a second variadic method, then called with loose
args, an empty tail and a spread; it fails on the pre-change converter with CS1503 + CS1593 + CS7036,
which is exactly the `slices` `TestGrow`/`TestConcat` error set.)

A/B footprint: this is the half of the arc that moves anything outside its own guard, and it moves
two lines. CNR at 645 behavioral packages reports `DeferCallOrder` and `GoCallVariations`, both
`f1 := fmt.Println` — a variadic PACKAGE function bound as a method value, which was the same
`var`-inferred synthesized delegate and is now `Funcꓸꓸꓸ<any, (nint, error)>`. Both still compile and
still match `go run`. The whole converted standard library re-emits byte-identically (4,173 artifacts,
0 changed), because the rule fires on nothing else: a method value whose signature is not variadic
never reaches it.

## Function literals

### A func literal that is only ever CALLED emits as a C# LOCAL FUNCTION
A C# lambda that captures anything allocates **two** heap objects every time the lambda expression is evaluated: a display class holding the captured variables, and a delegate bound to it. That is charged per call of the *enclosing* function, whether or not the closure is ever invoked — 88 bytes for the two-word case, measured. Go allocates neither when its escape analysis proves the closure does not outlive the frame, which is why `time`'s `TestUnmarshalTextAllocations` asserts `want 0 allocs`, and why `parseRFC3339`'s `parseUint := func(…)` was 88 of that row's 216.

A `name := func(…){…}` whose variable is **only ever the callee of a call** is therefore emitted as a C# *local function* instead:
```csharp
//  Go:   ok := true
//        parseUint := func(s bytes, min, max int) (x int) { … ok = false … return x }
var ok = true;
nint /*x*/ parseUint(bytes sΔ1, nint minΔ1, nint max) {
    nint x = default!;
    …
    ok = false;          // the SAME `ok` — both sites are rewritten to one struct-closure field
    …
    return x;
}
nint year = parseUint(s[0..4], 0, 9999);
```
Roslyn compiles a local function that is never converted to a delegate with a **by-ref struct closure**: the captured variables move into a struct that lives in the enclosing frame and is passed as a hidden `ref` parameter. There is still exactly one storage location per captured variable — the enclosing method's own uses are rewritten to the same field — so sharing, write-visibility and the capture-snapshot machinery are all unchanged. Only the heap objects are gone. The result type is rendered by the same helper `visitFuncDecl` uses, so a named Go result keeps its `/*x*/` comment and a local function reads exactly like a declared one; a single-return literal keeps the expression-bodied collapse (`byte num2(slice<byte> bΔ1) => …;`).

**The "only ever called" proof is what keeps that compilation available**, not a convenience: converting a local function to a delegate anywhere makes Roslyn fall back to a heap display class, and a local function has no value form to give a store, a return, an argument or a comparison in the first place. Every reference other than the declaring occurrence must be a call callee — which also subsumes reassignment (`f = …` is a non-call use of `f`) and address-taking, so the emitted name can never be required as a first-class value. Three further gates: the statement must be a `:=` **define** with one LHS ident and one RHS literal (a mixed `f, err := …` re-use records the name in `Uses`, not `Defs`, and binds no fresh object); it must be in statement position, since a local function is a declaration and cannot sit in a `for`/`if`/`switch` init clause; and the enclosing function declaration must be known (a literal inside a package-level `var` initializer is left alone).

A literal that **defers or recovers** is no bar: its frame is an ordinary local of the local function, declared in the local function's own body like any other, so the whole shape stays allocation-free —

```csharp
nint /*r*/ guard(nint n) {
    nint r = default!;
    GoFrame ᒐ = default;
    try {
        defer(() => {
            {
                var e = recover(); if (e != default!) {
                    r = -1;
                }
            }
        }, ref ᒐ);
        if (n < 0) {
            throw panic("negative");
        }
        r = n * 2; goto ᒐdone;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return r;
}
```

Go's two-step recursion idiom (`var f func(int) int; f = func(int) int {…}`) is an ASSIGN, not a DEFINE, so it is not this shape at all and keeps the lambda — correctly, since the recursive reference reads `f` as a value. (Guarded by the `LocalFunctionEmission` behavioral test: the `parseUint` shape with a named result and a mutated capture, the expression-bodied collapse, a struct-and-array capture mutated through the local function, two nested levels, and the deferring/recovering literal above — plus four negative controls, one per disqualifying reason: value use, reassignment, the recursion two-step, and argument position. The golden pins the emitted form; the stdout comparison against `go run` pins the capture semantics.)

### A func literal in an `any` slot states its Go result type explicitly
A function literal converted into a real **empty-interface** parameter has no delegate target
type, so C# natural-types it from its return arms — `func(x int) int { return 0 }` inferred
`Func<nint, int>` (the literal `0` is C# `int`, i.e. Go `int32`), and the natural type becomes
the value's runtime dynamic type, which reflection then classifies: `func(int) int` and
`func(int) int32` collapsed to ONE managed type, so `quick.CheckEqual` saw equal func types
where Go's differ (testing/quick's TestFailure #3). The emission states the declared Go result
type explicitly:
```go
CheckEqual(func(x int) int { return 0 }, func(x int) int32 { return 0 }, nil)
```
```csharp
CheckEqual(nint (nint x) => 0, int (nint x) => 0, default!);
```
Scoped to single-result literals in `any` slots (`CallExprContext.emptyInterfaceArgs` →
`LambdaContext.untypedInterfaceTarget` → convFuncLit's explicit-return-type mechanism);
target-typed positions are untouched — their delegate supplies the type, and an explicit return
type there could only add identity-match constraints against hand-written stub delegate types.
Multi-result `any`-slot literals kept natural tuple typing until html/template supplied the
consumer that caveat was waiting for (see below). Guarded
by the `LiftedLocalTypes` behavioral test; operationally by testing/quick's banked suite.

**The same slot is reached through a KEYED COMPOSITE, and there the loss is total rather than
merely imprecise.** The argument position above was the first consumer; a `map[K]any` value, an
`any` struct field and a sparse-`[N]any` element are the same empty-interface slot arrived at
through `convKeyValueExpr` instead of `convExprList`, and they were not marked. For a literal
with a reachable `return` the natural type is at least a func type of the right arity, so the
defect only narrowed a result type. For a literal whose body **never completes normally** there
is no return statement to infer from at all, so C# infers `Action` and the Go result type is
gone outright:
```go
FuncMap{"die": func() bool { panic("die") }}   // text/template exec_test
```
```csharp
["die"u8] = bool () => { throw panic("die"); }  // was: () => { throw panic("die"); }
```
The reflection bridge then reports `NumOut() == 0` — truthfully, because the datum is missing
from the emission, not from the bridge — and `text/template`'s own `goodFunc` rejects a function
Go accepts ("function die has 0 return values; should be 1 or 2"), panicking as the FuncMap is
registered and taking **16 of that package's 52 verdicts** with it. The mark is applied where the
value's declared slot is already resolved, so all three keyed forms are covered by one predicate;
a slot with a CONCRETE func type (`map[string]func() bool`) has a delegate target and is
deliberately left exactly as it was. Guarded by `untypedInterfaceFuncLit_test.go`
(`TestUntypedInterfaceFuncLitResultType` — the panic-only literal, a normal-return literal, an
`any` struct field, the MULTI-result arm, and the concrete-slot control), each arm proven
failing-first independently.

**The MULTI-result arm has the same owner from the opposite end.** The single-result rule above
was scoped for want of a demonstrated consumer; `html/template`'s escape_test is one. Its
`FuncMap{"pred": func(a ...any) (any, error) {…}}` renders every arm as a C# tuple carrying a
typeless element — `return (i - 1, default!)` and `return (default!, fmt.Errorf(…))` — so where
the panic-only literal has NO arm to infer from, this has arms that contribute nothing. Neither
fixes a delegate type, and inference fails outright (CS8917, then CS1662/CS8716 on each return).
The declared result tuple is stated explicitly through `generateResultSignature`, the same helper
the generic-inference arm already used:
```csharp
["pred"u8] = (any, error) (params ꓸꓸꓸany aʗp) => { … }
```

### Function-literal named results

A func **literal** with named results declares them at the top of its emitted block, zero-initialized — Go's semantics for `next = func() (v1 V, ok1 bool) { …; return }` (the `iter.Pull` shape): a bare `return` yields the named results as currently assigned, so the lambda emits `() => { V v1 = default!; bool ok1 = default!; …; return (v1, ok1); }`. Without the declarations the emitted tuple referenced undeclared names (CS0103 — the `iter` package's last wave-1 errors). Two interactions: a named-results literal whose *first* statement is a bare `return` must NOT collapse to an expression-bodied lambda (the names exist only as block declarations), and the `namedReturnDefer` path (named results that deferred code mutates) keeps its own arrangement — declarations *before* the `try`, returned after the `finally`. Declarations reuse the shadow-aware naming, so a literal result shadowing an outer local renames consistently in both the declaration and the return (`nΔ1`). (Guarded by the `FuncLitArgCapture` extension — bare returns with assigned and zero named results, plus the first-statement-bare-return shape, values vs Go.)

Because a named result lives in the literal's OWN scope, a reference to it in the body is the result, never an outer-scope capture — so named results are excluded from the lambda-capture set (`convFuncLit`) exactly as parameters are. text/template's `readFileFS` returns `func(file string) (name string, b []byte, err error)`, whose closure captures the enclosing `fsys` AND writes `b` via the captured tuple call `b, err = fs.ReadFile(fsys, file)`. Because the closure genuinely captures `fsys`, the capture analysis ran and mis-flagged `b` too — hoisting `var bʗ1 = b;` into the enclosing function, where `b` does not exist (CS0103), and renaming the body's `b` to the captured `bʗ1`. Filtering the named-result names out of the capture set (alongside the parameter names) leaves `b` a plain in-block declaration. (Guarded by `CrossPkgUser`'s `makeScanner` — a captured closure returning named results, one written via a tuple call whose RHS uses the capture, output-compared vs Go; crypto/x509 and html/template shared the same latent shape.)

### Function-literal parameters share the body scope
Go declares parameters in the function block, so a body-level `fpath, err := ...` REUSES a literal's `err` parameter. The variable analysis gives literals ONE merged scope (params + body declarations) mirroring real function declarations; a separate param scope had made the `:=` a shadow declaration beside later reuses (CS0841/CS0128, os CopyFS's WalkDir literal). Guarded by `LambdaFunctions` (`probe`).

## Function-literal return types

### Function literals returning `unsafe.Pointer` state their return type
A literal with a single `unsafe.Pointer` result can mix return arms of DIFFERENT C# types (reflect `deepEqual`'s `ptrval`: `(uintptr)v.pointer()` on one arm, the raw `v.ptr` on the other), which defeats C# lambda return-type inference (CS8917). The emitted lambda states its return type explicitly; each arm then converts implicitly through the golib operators:
```csharp
var pick = @unsafe.Pointer (bool u) => {
```
Guarded by `PointerCastSliceRange` (compile-shape).

### Interface-returning literals with distinct arm types state their return type too
The same inference gap hits an interface result whose arms return DIFFERENT concrete types — net ipsock.go's `inetaddr := func(ip IPAddr) Addr` returns three pointer-adapter classes (`TCPAddrжΔAddr` / `UDPAddrжΔAddr` / `IPAddrжΔAddr`), which share only the interface (CS8917). When a single non-empty-interface result's return arms carry two or more distinct types, the lambda states the return type explicitly (`Addr (IPAddr ip) => …`); each arm then converts implicitly. Single-typed literals keep the inferred form (zero churn). (Guarded by the `InterfaceCasting` extension `makeAnimal` — an adapter arm plus a value arm, runtime-verified.)

**And the opposite end of the same gap: arms that are ALL untyped `nil`.** `client := func(*TCPConn)
error { <-serverDone; return nil }` (net `net_test`) renders its only arm as `default!`, which carries
no natural type at all — so where the rule above has too many candidate types, this has none, and it
is the same CS8917. The interface arm now also states the return type when the literal is in
assignment position, has a return, and **every** single-result arm is untyped nil. Drift-free by
construction: an all-`default!` arm set never had an inferable natural type, so every site the rule
touches is a site that did not compile. It is the single-result twin of the multi-result
`!hasFullyTypedArm` rule immediately below, and it stays out of argument/return position for the same
reason that one does — those literals are target-typed by their delegate, so nothing is inferred.
(Guarded by the `FuncLitStringConcatReturn` extension: an all-`nil` literal in both `:=` and `var`
form, plus a mixed-arm control that must KEEP inferring.)

### Multi-value literals with no fully-typed arm state their return type — named results included
The single-result inference gaps above generalize to any MULTI-result literal where EVERY return arm carries a typeless element — `return nil, nil, nil, nil, err` on the error arms and `return dnsNames, ips, emails, uriDomains, nil` on the success arm (crypto/x509 `parseNameConstraintsExtension`'s `getValues := func(subtrees) (dnsNames []string, ips []*net.IPNet, emails, uriDomains []string, err error)`). A C# tuple literal with any untyped element has no natural type, so no arm fixes the lambda's return type and delegate-type inference fails (CS8917). The lambda states its tuple return type explicitly, and each `nil` then takes its target element type:
```csharp
var getValues = (slice<@string> dnsNames, slice<ж<net.IPNet>> ips, slice<@string> emails, slice<@string> uriDomains, error err) (cryptobyte.String subtrees) => { … };
```
NAMED results are now included (they were previously excluded): the trigger — a multi-result literal with a return arm but NO fully-typed arm — is identical whether the results are named or not. A bare `return` (which returns the named results) never matches the result arity, so it neither marks has-return nor a false fully-typed arm; a named literal that DOES have a fully-typed explicit arm keeps inferred typing (no return-type prefix, no churn). Guarded by `NamedResultLambdaInfer` (a five-result named-result closure whose error arms return `nil,nil,err` and success arm `e,o,nil`).

### String-returning literals in assignment position state their return type
A literal with a single Go `string` result can mix return arms of DIFFERENT C# types even though every arm is a Go string: a bare string literal is a `"…"u8` `ReadOnlySpan<byte>`, a literal+variable concat binds golib's `operator +(@string, @string)` (so it is `@string` regardless of u8 suppression), and a call into a hand-written stub can return C# `string` (the baseline `fmt.Sprintf` does). `@string` and `string` convert implicitly in BOTH directions, so a lambda mixing those arms has no unique best common type and its delegate type is not inferable — CS8917 on `pick := func(v any) string {…}` whose `case string:` arm returns `"string:" + t` alongside `fmt.Sprintf` arms. In assignment position (`var pick = …`, where C# must infer the delegate type), the lambda states its return type explicitly and each arm then converts to `@string` in place:
```csharp
var pick = @string (any v) => {
```
Argument/return/composite-element literals are target-typed by their receiving delegate type (no inference to fail — and an explicit return type could only add an identity-match constraint against stub delegate types), so they keep the plain form; the Go `var` declaration form emits an explicit delegate type (`Func<@string, bool, @string> pad = …`) and is likewise immune. Gated to the basic string kind — a named string type would need its own conversions. Guarded by `FuncLitStringConcatReturn` (`:=` literals mixing concat, u8-literal, and stub-`Sprintf` arms — including a type-switch body and a right-side literal concat — plus the `var` form; runtime-verified).

The TUPLE-ELEMENT sibling: in a MULTI-result literal, a bare string literal element is worse than typeless — it is *wrongly* typed. Inside a tuple the literal emits as a bare C# `string` (u8 spans cannot be tuple elements), so an arm with no `nil` and no string variable — internal/fuzz `fuzzOnce`'s `return dur, coverageSnapshot, ""` (`func(entry CorpusEntry) (dur time.Duration, cov []byte, errMsg string)`) — counted as "fully typed" in the multi-result scan above and suppressed the explicit tuple return type, letting inference *succeed with the wrong element type*: the destructured `errMsg` was C# `string`, which has no `!=` against a `"…"u8` span (CS0019 rather than CS8917). A basic-string constant literal element whose declared result element is Go `string` now also marks its arm not-fully-typed, so the same explicit-tuple emission fires:
```csharp
var fuzzOnce = (time.Duration dur, slice<byte> cov, @string errMsg) (CorpusEntry entry) => { … };
```
and each `""` converts to `@string` in place via target typing. Same assignment-position gate; a literal whose string elements are all variables keeps inferred typing (the full-stdlib A/B footprint was exactly internal/fuzz worker.cs plus two latent-identity repairs, internal/coverage/decodecounter `sget` and net ipsock.go `addrErr`, both re-proven green). Guarded by the `FuncLitStringConcatReturn` extensions `fuzzish` (named results, `!= ""` on every destructured element) and `sget` (unnamed `(string, error)`); the pre-fix converter fails them with exactly CS0019 ×4.

The NUMERIC sibling: an untyped numeric constant literal element against a differently-SIZED declared result element is wrongly typed the same way. The literal emits bare, so the arm infers the literal's natural C# type — an INT literal is C# `int`, a FLOAT literal C# `double` — where the Go result is e.g. `int64`: net/http ServeContent's `sizeFunc := func() (int64, error) { …; return 0, errSeeker }` had no `nil`/string-literal element on its error arms, counted "fully typed", and inferred `Func<(int, error errSeeker)>` — rejected at the `serveContent(…, sizeFunc, …)` call because delegate types are invariant (CS1662/CS0029/CS1503, and the leaked `errSeeker` element name rides the inferred tuple). Such an element now also marks its arm not-fully-typed, so the same explicit-tuple emission fires:
```csharp
var sizeFunc = (int64, error) () => { …; return (0, errSeeker); };
```
A declared element the literal's natural type already matches (`int32` for an INT literal, `float64` for a FLOAT literal) infers correctly and stays inferred, and Go `int` (C# `nint`) is deliberately exempt — `return 0, err` against `(int, error)` results is pervasive and green today (the element converts implicitly at every use site), so marking it would churn stdlib-wide for no observed defect, the same reasoning that keeps `lambdaConstReturnCastType` away from signed single results. A SUB-negated literal (`return -1, …`) is unwrapped and marked the same way. (Full-stdlib A/B footprint: net/http fs.cs `sizeFunc` — the target — plus three latent same-shape repairs, internal/coverage/decodecounter `rdu32` ×3 and net/http h2_bundle `allocatePromisedID` (both `(uint32, error)`) and internal/zstd `fetchHuff` (`(uint16, error)`). Guarded by the `FuncLitNumericTupleReturn` behavioral test — the sizeFunc shape and a float64 shape both PASSED to typed function parameters, the `-1` arm, and the int/float64-identical controls that must keep inferred typing; the pre-fix converter fails it with exactly the fs.cs trio CS1662/CS0029/CS1503 ×2.)

That Go-`int` exemption is **narrowed, not absolute**: it holds only while every numeric arm at the declared-`int` position is a literal. When such a position carries BOTH a bare-`0` arm (naturally C# `int`) AND a **non-literal** Go-`int` arm (`i + 1` → C# `nint`) — and the other tuple slots on the non-literal arms are typeless (`default!`), leaving a single naturally-typed literal arm to drive inference — C# infers the delegate's first element as `int`, so the `nint` arm then fails to convert (CS0029/CS1662) and the assignment-inferred delegate is rejected at the invariant use site (CS0407). This is bufio `ExampleScanner_*`'s `onComma := func(…) (advance int, token []byte, err error) { …; return i+1, data[:i], nil; …; return 0, data, ErrFinalToken; }`. The multi-result scan therefore additionally records, per declared-`int` position, whether an int-LITERAL arm and a non-literal `nint`-expression arm both occur; when they do, the explicit `(nint, …)` return type is forced (`(nint advance, slice<byte> token, error err) (…) => …`) and each arm converts in place. The all-literal `return 0, err` shape has no non-literal arm, so it keeps its inferred emission untouched — the corpus is undisturbed (behavioral CNR byte-identical across all 451 projects). (Guarded by the `FuncLitNumericTupleReturn` extension — the `mixedIntArms`/`onComma` shape assigned and passed to a typed parameter, alongside the unchanged all-literal `intControl`; the pre-fix converter fails it with CS0029/CS1662/CS1503.)

### Numeric-returning literals with untyped-constant arms state their return type
The SINGLE-result numeric sibling of the string arm above (2026-07-17; the Phase-4 blocker-map row B7b — strings ×3, bytes ×2): a literal with a declared numeric result whose return arm references a **named untyped constant** — strings/bytes TestMap's `maxRune := func(rune) rune { return unicode.MaxRune }`. The const reference emits as a golib `Untyped*` wrapper reference (`Δunicode.MaxRune`, an `UntypedInt` static), and the wrapper's implicit conversions run in **both** directions with every numeric type. So in natural-inference position an all-const arm set infers the wrapper delegate — `var maxRune = (rune r) => Δunicode.MaxRune;` is `Func<rune, UntypedInt>`, rejected at the invariant-delegate `Map(maxRune, …)` call (CS1503) — and a mixed const/typed arm set (TestMap's `encode`, mixing `unicode.MaxRune`/`utf8.RuneSelf` with the `rune` parameter) has no unique best common type at all (CS8917). When any top-level return arm is a bare named untyped-const reference, the lambda states the declared return type explicitly and each arm converts in place:
```csharp
var maxFn = rune (rune _) => maxRune;
```
Same gates as the string arm: assignment position only (argument/return/composite-element literals are target-typed — no inference to fail), and a BASIC numeric result (a named numeric type would need a second user conversion the wrapper cannot chain — the `lambdaConstReturnCastType` named-type rationale).

**Literal-only arm sets are NOT automatically safe** (corrected 2026-08-15, the `crypto/tls` lane). This section used to end "literal-only arm sets stay inferred (no churn): an int literal is already C# `int`". They *are* concretely typed — but a BARE int literal is C# `int`, which is the declared type only when the Go result is `int32`. `crypto/tls` `TestCipherSuites`' comparator is the counterexample:
```go
isBetter := func(a, b uint16) int { …; return -1; …; return +1; …; return 0 }
…
if !slices.IsSortedFunc(prefOrder, isBetter) { … }
```
Every arm rendered as C# `int` against a Go `int` (C# `nint`) result, so the inferred delegate was `Func<ushort, ushort, int>`. Every *call* of the variable accepted it (`int` converts to `nint`); the delegate-VALUED use did not, delegate types being invariant — `CS1503: cannot convert from 'System.Func<ushort, ushort, int>' to 'System.Func<ushort, ushort, nint>'`, one of the package's four build errors. So a third arm joins: when **every** top-level single-result return arm is an INT literal and the declared basic result is an integer type other than `int32`, the declared type is stated:
```csharp
var isBetter = nint (uint16 a, uint16 b) => { …; return -1; …; return +1; …; return 0; };
```
**The gate is keyed to what the converter EMITS, not to the literal's Go-side natural type.** Two wider cuts were written and each measured to over-apply before this one:

| shape | emitted arm | C# infers | prefix? |
|:--|:--|:--|:--:|
| `func(…) int { return -1 }` | `-1` (bare) | `int` — wrong | **yes** |
| `func(bool) int64 { return 9 }` | `9` (bare) | `int` — wrong | **yes** |
| `func(bool) int32 { return 100 }` | `100` (bare) | `int32` — right | no |
| `func() float32 { return 0.5 }` | `0.5F` | `float` — right | no |
| `func() float64 { return 3 }` | `3D` | `double` — right | no |

Only a declared **integer** width leaves the literal bare: a floating result carries its width into the literal, and `int32`/`rune` *is* the bare literal's own C# type. Three further things bound the rule. Any arm the predicate cannot classify suppresses it — every non-INT-literal expression is *assumed* to carry the declared type — so mixed arm sets keep their present emission, as does a bare `return` against named results. A literal bound to a name that is only ever CALLED never reaches this code at all: `localFunctionDefine` has already emitted it as a C# local function carrying an explicit result type, so the arm can only fire where the delegate type is genuinely observable. And both literal SIGNS are stripped (`numericBasicLit` now unwraps unary `+` as well as unary `-`): Go writes an explicitly-positive literal precisely where it pairs with a negative one, which is the comparator shape this arm exists for, and treating `+1` as a non-literal blinded the predicate to half its own arm set — it did, on the first cut, where the fix silently emitted nothing at all. Not covered, and with no measured instance: a MIXED arm set whose declared type is NARROWER than the type C# picks (`func(…) uint16` with one `0` arm and one `ushort` arm, where `ushort` widens to `int`); that needs the natural C# type of an arbitrary expression, which the predicate deliberately does not attempt. (All five table rows sit side by side in the `FuncLitUntypedConstReturn` behavioral test, so the split stays pinned to the emission rather than to this table.)

A constant operator **expression** arm containing a named untyped constant counts the same as the bare reference (2026-07-17; the B7b gap — bytes TestMap's `invalidRune := func(r rune) rune { return utf8.MaxRune + 1 }` was the one remaining bytes build error): the operator result keeps the wrapper type, so the inferred delegate was `Func<int, UntypedInt>` against Map's `Func<int, int>` parameter (CS1503). The arm test (`returnArmKeepsUntypedWrapper`) walks paren/unary/binary trees for an untyped-named-const leaf, **except** when a constant fold (`overflowingConstLiteral` / `floatContextConstLiteral`) rewrites the whole arm to a plain literal — that emission is concretely typed and needs no prefix. All other gates unchanged. (Guarded by the `FuncLitUntypedConstReturn` behavioral test — the single-arm CS1503 shape, the mixed-arm CS8917 shape, an `int64` result with a beyond-int32 const arm, the const-expression arm (`return maxRune + 1`), plus literal-only and argument-position controls that must keep the plain form; output-compared vs Go.)

### A literal in GENERIC-RESULT inference position states its return type
The arms above all describe **natural-inference** position — a literal assigned to a `var`, where no
delegate target exists. A literal passed as an ARGUMENT is normally target-typed by its parameter and
needs no prefix, and the earlier rules say exactly that. There is one argument shape where that is
false: the callee is **generic** and the parameter's declared signature returns a **type parameter**
— `sync.OnceValue[T any](f func() T)`, `sync.OnceValues[T1, T2 any](f func() (T1, T2))`. There the
parameter type is not yet a concrete delegate; C# must infer the type argument **from the lambda's
own return expressions**, so the Go result type go/types already resolved is ignored. Two shapes then
break (both live in sync's `oncefunc_test.go`):

- **No arm yields a C# type at all** — a body terminated by `panic` (`func() any { calls++; panic("x") }`
  emits a statement lambda whose only exit is a `throw`), or one whose sole arm is an untyped `nil`
  (`return default!`). Inference has nothing to work from: CS0411 ×4.
- **An arm's NATURAL C# type differs from the declared Go result** — `func() int { return 42 }` is
  naturally `Func<int>`, where Go's `int` is `nint`: the wrong delegate is inferred and the
  declaration it initializes rejects it (CS0029).

A func literal in that position now states its declared result type, which fixes the type argument to
exactly Go's for every arm shape (so, unlike the natural-inference arms, no arm inspection is needed):

```go
var onceValue = sync.OnceValue(func() int { return 42 })
f := sync.OnceValue(func() any { calls++; panic("x") })
g := sync.OnceValues(func() (any, any) { buf[0] = 1; return nil, nil })
```
```csharp
internal static Func<nint> onceValue = Δsync.OnceValue(nint () => 42);
var f = Δsync.OnceValue(any () => { Ꮡcalls.Value++; throw panic("x"); });
var g = Δsync.OnceValues((any, any) () => { bufʗ3[0] = 1; return (default!, default!); });
```

The gate is the **result** position specifically. A type parameter appearing only in the func-typed
parameter's own PARAMETER list — the `slices.SortFunc(x, func(a, b E) int)` shape — is inferred from
the lambda's already-typed parameters and stays unprefixed; marking those would churn every such call
site in the corpus for no defect. Full-stdlib footprint: two files, both package-level
`sync.OnceValue` initializers (`internal/sysinfo`'s `CPUName`, `internal/syscall/windows`'s
`SupportUnixSocket`/`SupportTCPInitialRTONoSYNRetransmissions`). (Guarded by
`GenericResultLambdaInfer` — the `nint`/`any`/panic-terminated/two-result shapes, a concrete
multi-result instantiation, and the parameter-position negative control, output-compared vs Go.)

### A returned FUNC LITERAL is typeless in C#

Every arm above asks the same question — *does this return expression carry a natural C# type?* — and
each was written against the Go-side shapes seen so far: an untyped `nil`, a bare constant, an untyped
const wrapper. A Go **function literal** is a fourth shape, and it is typeless for a reason none of
those tests notice: it is fully typed in Go, and it renders as a bare C# lambda, which has no natural
type at all.

A function's own returns need no help — a declared C# result type target-types them, so a method whose
body sits in a frame simply returns the literal (`context`'s `afterFuncContext.AfterFunc`, which
returns a `Func<bool>` literal from a defer-holding body). The one site that does need help is **a
literal returned from inside another literal.** `lambdaConstReturnCastType` already casts a bare
integer literal returned inside a lambda for exactly this reason (CS8917). Its sibling
`lambdaFuncLitReturnCastType` names the declared result type of a returned func literal, under the same
gates — inside a lambda conversion only (a named function's returned literal is target-typed by its
declared C# return type), and only when the declared result is a NAMED func type, whose emitted
delegate name is what a cast needs:

```go
mergeCancel := func(ctx, cancelCtx Context) (Context, CancelFunc) {
    …
    return ctx, func() { stop(); cancel(Canceled) }
}
```
```csharp
(context.Context, Action) mergeCancel(context.Context ctx, context.Context cancelCtx) {
    …
    return (ctx, (Action)(() => {
        stopʗ1();
        cancelʗ3(context.Canceled);
    }));
}
```

(`mergeCancel` is itself only ever called, so it takes the local-function emission above — still a
lambda conversion, which is what the gate tests.) Without the cast the tuple has no natural type, so
neither does the enclosing conversion — CS8917 on the declaration and CS8130 at every deconstruction
of its result. Naming the type is also the more faithful rendering: Go's declared result there *is*
`CancelFunc`, a methodless func type, which renders inline as its base delegate `Action`. An UNNAMED `func() bool` result would need the synthesized
`Func<…>`/`Action` spelling and has no corpus site today, so it is deliberately left.

## Capture

### Capturing the address of a heap-boxed local in a closure
A local whose address is taken (`&m`) is heap-boxed: the converter emits `ref var m = ref heap(new T(), out var Ꮡm)`, where `Ꮡm` is the box and `m` is a `ref`-local alias of `Ꮡm.Value`. When a **function literal captures such a local and takes its address inside the closure**, the variable must be referenced through the box, not snapshot-copied. A C# `ref`-local cannot be captured by a lambda (CS8175), and the older snapshot capture (`var mʗ1 = m;`) is wrong twice over: it copies the *value* out of the box (so writes through the captured `&m` are lost), and the copy declaration is a statement that has nowhere valid to land when the literal sits in an expression position — e.g. a func literal passed as a **call argument** (`run(func(){ use(&m) })`) or a local initializer (`f := func(){ use(&m) }`).

The fix: a heap-boxed local whose address is taken inside a lambda is marked *box-ref* and the snapshot is suppressed. The box `Ꮡm` is a plain local (a capturable reference), so the C# closure captures it by reference — matching Go's capture-by-reference semantics. Inside the closure the converter then renders every form through the box:

```csharp
ref var m = ref heap(new box(), out var Ꮡm);
run(() => {
    set(Ꮡm);                       // &m  → Ꮡm
    Ꮡm.Value.y = Ꮡm.Value.x + 1;       // value use of m → Ꮡm.Value
});
// &m.field (value struct field) → Ꮡm.of(box.Ꮡfield)
```

This also covers `&m.field` (a value-struct field address inside the closure: `Ꮡm.of(box.Ꮡfield)`). The detection is scoped to the bare `&m` and value-struct `&m.field` forms (the ones with a box-ref emission form); an element address `&m[i]` keeps the existing snapshot path. The behavioral test `FuncLitArgCapture` guards the call-argument, value-use, field-address, and initializer cases.

### A capture that is WRITTEN after the capture point routes to shared storage, not a snapshot
Go closures share the ONE variable with the enclosing function. The value snapshot the converter uses for captured structs/arrays/slices/maps/chans (`var tʗ1 = t;` hoisted before the lambda, in-lambda references renamed to `tʗ1`) is therefore only *observationally* correct while **neither side writes the variable after the snapshot point**. Once anything does, the snapshot silently diverges — the program compiles and runs, with wrong values:

- a closure's writes land in a divorced copy: `bump := func() { t.total += 100 }` never affects `t` (probe-proven: Go 106, snapshot 6);
- body writes after the lambda's creation are invisible to a read-only closure: `get := func() int { return t.total }; t.total += 10` reads the stale copy (Go 15, snapshot 5);
- a **deferred** literal observes the variable's registration-time value instead of Go's final value (`defer func(){ fmt.Println(t.total) }(); t.total = 42` — Go 42, snapshot 5);
- two closures over one variable each get their own copy, so a writer and a reader stop communicating entirely.

The converter now detects **written-after-capture** per variable during analysis (`varShareFacts`, one cached scan of the enclosing declaration) and routes such captures to shared storage. Writes counted, conservatively by syntax: an assignment or `++`/`--` whose target roots at the variable's own storage (`t = …`, `t.f.g = …`, array `a[i] = …` — but *not* through a deref, an implicit pointer deref `p.f = …`, or a slice/map element, which a snapshot copy shares anyway); a pointer-receiver method call on the variable held as a value (Go's implicit `&t`); a `for t = range` clause; an explicit `&t` anywhere or an uncalled pointer-receiver method value (an **alias** — later writes through it are syntactically invisible, so it counts at any position, e.g. `p := &t; get := func(){…}; p.total = 50`); and any of these inside any func literal (the literal may run at any time). A plain body write counts only if positioned after a referencing literal or sharing a `for`/`range` loop with one (a later iteration's write follows an earlier iteration's creation).

The routing, by variable shape:

```csharp
// Heap-boxed variable (escaping struct local, aliased int, …) → by-box (boxRefVars):
ref var t = ref heap<Tally>(out var Ꮡt);
t = new Tally(5, "s");
var bump = () => {
    Ꮡt.Value.total += 100;      // value use → Ꮡt.Value: writes the ONE box the body reads
};

// Unboxed variable (value parameter; slice/map/chan local, whose copy diverges on
// reassignment) → NATIVE C# capture — no snapshot, no rename; the display class
// shares the local exactly as Go shares the variable:
internal static void probeB1(Tally t) {
    var bump = () => {
        t.total += 100;          // captures the parameter itself
    };
    bump();
    t.total++;                   // 106, matching Go
```

This applies only to genuine **closure-body** references (a func literal's body, directly or as a go/defer statement's literal callee). A go/defer statement's non-literal callee/receiver expression and its call arguments keep their statement-time evaluation — `defer fmt.Println(t.total)` still prints the registration-time value, which IS Go's argument semantics. Read-only-after-capture variables keep the snapshot (observationally identical, zero churn — the vast majority of stdlib captures). A **loop-statement-defined** variable (for-init or range clause) also keeps it: its per-lambda snapshot approximates Go 1.22's per-iteration variable, which shared routing would break. A literal's own parameters/results are not captures and are excluded. (The remaining known gap, deliberately out of scope here: a `for`-init variable captured by closures diverges from Go 1.22 per-iteration semantics — the C# `for` control variable is shared across iterations — tracked as its own defect.)

The native route makes the emitted C# read exactly like the Go for the parameter case; the by-box route reuses the box-ref machinery above (including `ValueSlot` for inherently-heap locals: a captured slice that the closure reassigns emits `Ꮡs.ValueSlot = append(Ꮡs.ValueSlot, …)` against a materialized `heap<slice<T>>` box). (Guarded by the `ClosureWriteVisibility` behavioral test — 19 probes: boxed/plain × local/param × closure-writes/body-writes-after × plain/defer/go/IIFE/loop-created contexts, plus slice/map reassignment, alias writes, two-closure sharing, and the read-only/defer-argument/range controls that must KEEP snapshot semantics.)

**A NAMED RESULT routed to shared storage declares its box too.** The `defer func(){ hook(written, err) }()` idiom is exactly the written-after-capture shape above with the captured variable being a *named result* — Go's deferred closure must observe the FINAL named-result values. When the escape analysis marks such a result (an interface-typed result is blanket-marked the first time it is reused on a mixed `v, err := …` define; a value-type one when `&x` is taken), the render sites duly go through the box (`Ꮡerr.ValueSlot` inside the deferred literal) — but the named-result declaration prologue emitted only the plain `error err = default!;`, leaving `Ꮡerr` undeclared (CS0103 — internal/poll `SendFile`'s deferred `TestHookDidSendFile`, the single error skip-cascading ~80 os-dependent packages). A box-backed named result (`identHasHeapBox`, the same gate plain locals use) now declares the box, in three shapes:

- **No defer frame** (plain function, or a closure writing the result): the full escaping-local form at the declaration site — `ref var err = ref heap<error>(out var Ꮡerr);` — body and bare returns keep reading the plain alias, nested closures read/write `Ꮡerr.ValueSlot`. A value-type result with `&x` gets `ref var x = ref heap(new nint(), out var Ꮡx);`, making the write through `&x` visible to the bare return (previously that shape was also CS0103).
- **namedReturnDeferMode** (a function whose results are declared before the frame's `try`): the decls sit OUTSIDE the `try`, and the deferred closures that read them are lambdas, which cannot capture a `ref` local (CS8175) — so the outside line creates only the box (`heap<error>(out var Ꮡerr);`), the `try` re-derives the value alias inside (`ref var err = ref Ꮡerr.ValueSlot;`, exactly like a deref'd pointer parameter's `ref var fd = ref Ꮡfd.Value;`), and the post-`finally` return reads through the box: `ᒐdone: return (written, Ꮡerr.ValueSlot);` (internal/poll `sendfile_windows.cs`).
- **Func-literal sibling** (a literal with named results + defer + post-capture writes): same split, except the literal's body is itself a lambda conversion, so every in-`try` use already renders through the box — including the explicit-return rewrite's assignment targets (`(var v, Ꮡe.ValueSlot) = pair(n);`) — and the literal's trailing `return (w, Ꮡe.ValueSlot);` reads the box.

The box-read accessor follows the box-ref rule above: `.ValueSlot` for an inherently-heap result (reading the held reference is not a dereference), `.Value` for a value-type box. Results NOT escape-marked are untouched — `written` in the same defer stays a plain local captured natively by the C# closure, which already observes the final value. (Guarded by the `NamedResultDeferCapture` behavioral test — value + error named results logged by a deferred closure with post-capture writes and bare returns, the `&x` value-result, the func-literal sibling, and a non-defer closure write; output-compared vs Go, proving the deferred observation of FINAL values. Stdlib footprint: 12 functions across 10 files — internal/poll, net/http, go/parser, crypto/tls, internal/fuzz, debug/buildinfo, both go importers, net/textproto.)

**A PARAMETER routed to shared storage declares its box too** — the third position of the same family (plain locals, named results, parameters). A parameter can be escape-marked without any capture-mode method call: a body-top-level mixed `:=` REDECLARES the parameter object (the spec's redeclaration rule includes the parameter lists when the block is the function body), so the define walker escape-analyzes it — and an interface-typed one is blanket-marked. When such a parameter is also captured by a closure and written after the capture point, the routing above sends it by-box (`Ꮡctx.ValueSlot` inside the lambda) — but the parameter prologue only boxed for the capture-mode (direct-ж) trigger, leaving the box undeclared (CS0103): database/sql `beginDC`'s `ctx` (redeclared by `ctx, cancel := context.WithCancel(ctx)` after `withLock`'s closure captured it) and go/types `nify`'s `x, y` (swapped by `x, y = y, x` and redeclared by `xorig, x := x, Unalias(x)` after the trace defer captured them). `paramNeedsHeapBox` (and its func-literal analogue `funcLitHeapBoxParamIdents`) now also fires for a box-ref-routed parameter, emitting the exact capture-mode form — the signature takes the incoming value as `ctxʗp` and the preamble declares `ref var ctx = ref heap(ctxʗp, out var Ꮡctx);` (inside the frame's `try` when the function has a frame, where the box is an ordinary capturable local). Body statements keep reading/writing the plain ref alias — the redeclare emits `(ctx, var cancel) = …` against it — so both sides hit the ONE box, and a deferred observer sees Go's FINAL values. The check rides the declaring-ident lookups, so a box-ref'd value RECEIVER (never `ʗp`-renamed by the signature paths) can never take the param form. (Guarded by the `WrittenCaptureParam` behavioral test — the beginDC redeclare shape, the nify deferred-observer shape (named result + defer frame), a closure-write read back by the body, the func-literal sibling, and an inherently-heap slice param; all output-compared vs Go. Stdlib footprint: exactly `database/sql/sql.cs` + `go/types/unify.cs`.)

### A write that ENCLOSES the literal counts as written-after-capture — the self-recursive closure

The write scan above compares *positions*: a body write counts when it sits after a referencing literal, or shares a loop with one. Go's standard recursive-closure idiom defeats a pure position test, because the write **starts before** the literal it contains:

```go
var check func(uint32, []bool) bool
check = func(pc uint32, m []bool) (ok bool) {
	…
	ok = check(inst.Out, m) && check(inst.Arg, m)   // recurses through the variable
	…
}
```

The assignment statement's position is that of `check` on its left, which precedes the literal on its right — yet the RHS is evaluated *first*, so the store to `check` unambiguously happens after the literal exists. Scored as read-only-after-capture, the capture took the snapshot path (`var checkʗ1 = check;` hoisted **above** the assignment) and every recursive call invoked the still-**null** delegate: a `NullReferenceException` on the first recursion. In regexp's `makeOnePass` that is the entire ambiguity check, so `^.$` — and most of the package — became uncompilable. The scan now also counts a write whose syntactic extent *contains* a referencing literal (`w.pos < lit.pos < w.end`), which routes the capture to shared storage; `check` escapes, so it takes the by-box form and the recursion resolves against the box the assignment fills:

```csharp
ref var check = ref heap<Func<uint32, slice<bool>, bool>>(out var Ꮡcheck);
check = (uint32 pc, slice<bool> mΔ1) => {
    …
    ok = Ꮡcheck.ValueSlot((~inst).Out, mΔ1) && Ꮡcheck.ValueSlot((~inst).Arg, mΔ1);
    …
};
```

The same edge covers **mutually** recursive closures (`even`/`odd`, each literal enclosed by the write to its own name while reading the other), and it generalizes beyond closures: any write that evaluates a referencing literal as part of itself — `t.mutate(func(){ use(t) })` — now counts. (Guarded by the `ClosureWriteVisibility` probes Q1/Q2 — a self-recursive sum and a mutually recursive parity pair; the pre-fix converter compiles both and nil-derefs at the first recursive call.)

### A nested closure's capture snapshot reads the enclosing closure's snapshot
When a heap-boxed **ref-local is used by VALUE** (its address is not taken) and captured by NESTED closures, it is not box-ref'd — it is snapshot-copied: the converter declares `var mʗ1 = m;` before the closure and the closure uses `mʗ1`, so the uncapturable `ref`-local `m` is never referenced inside the lambda. The snapshot chain must be threaded through each level. A capture generated for an **inner** closure that lands inside an **outer** closure's body must read the outer closure's snapshot, not the enclosing method's ref-local — the shape testing/fuzz.go's `run` closure has, capturing `fn := reflect.ValueOf(ff)` (a heap-boxed `reflect.Value`) and spawning `go tRunner(t, func(t){ … fn.Call(args) })` from inside itself, where the method-level `fn` is a ref-local uncapturable inside a closure (CS8175). The guard's own shape emits it:

```csharp
ref var p = ref heap<payload>(out var Ꮡp);
p = new payload(vals: new nint[]{1, 2, 3, 4}.slice());
var @out = new channel<nint>(1);
var outʗ1 = @out;
var pʗ1 = p;                   // outer's snapshot (before the outer closure)
void outer() {
    var outʗ2 = outʗ1;
    var pʗ2 = pʗ1;             // the goroutine's snapshot reads outer's pʗ1, NOT p
    goǃ(() => {
        outʗ2.ᐸꟷ(pʗ2.sum());
    });
}
```

`generateCaptureDeclarations` finds the RHS by walking the conversion stack outward past pass-through levels (a `go`/`defer` statement's own `enterLambdaConversion`, which carries an empty rename map) to the first enclosing lambda that renamed the variable. It skips the capture's OWN owner state — `pendingCaptures` is shared across a function's lambdas, so an outer lambda's snapshot can be generated while converting an inner func-literal argument (`go dnsWaitGroupDone(ch, func(){})`, net/lookup.go), leaving the owner's state on the stack with a rename equal to the name being declared; adopting it would emit a self-reference `var fʗ1 = fʗ1;` (CS0841). Byte-identical corpus-wide except where a nested closure re-captures a heap-boxed local. Guarded by `FuncLitArgCapture` (a heap-boxed struct re-captured in an inner goroutine — CS8175 without the fix — and the `go f(x, func(){})` self-reference shape) and by `DeferValueFieldPtrReceiver` (a defer inside a lambda).

### A nested closure must not clobber the enclosing closure's capture state
The per-lambda conversion state — `conversionInLambda` (are we inside a closure body?) plus the capture-name maps (`currentLambdaVars`/`currentLambdaVarObjs`) — is what makes closure-body emission rewrite captured references to their box/copy forms: a captured local `s` reads as `sʗ1`, and the current method's **direct-ж receiver** (`func (s *Stmt) …` emitted `this ж<Stmt> Ꮡs`, whose body alias `ref var s = ref Ꮡs.Value` is a `ref`-local that **cannot** be captured by a C# closure) reads through its box as `Ꮡs.Value`. That state was *set* on entering a closure but **reset to `false`/`nil` on exit**, not restored — so a closure that contains an **inner** closure had its state wiped the moment the inner one finished, and every reference in the *outer* closure body **after** the inner one fell back to the bare, un-rewritten name. For a receiver field-read that is a bare ref-local capture — `database/sql (*Stmt).QueryContext`'s `s.db.retry(func(){ …; rows.releaseConn = func(err){…}; if s.cg != nil { … } })`, where `s.cg` sits after the inner `releaseConn` closure — the emission was `s.cg` (CS8175, "cannot use ref local `s` inside an anonymous method/lambda"); the equivalent captured-local case silently split a variable between its bare form and its `ʗ1` copy within one closure. The fix makes `enterLambdaConversion`/`exitLambdaConversion` a proper **LIFO save/restore stack** (`conversionStack`): entering pushes the current state and installs fresh state; exiting **restores the enclosing closure's** state instead of resetting. A closure at top level still restores to `false`/empty (unchanged), so the change is inert except where a closure body continues after a nested closure — there the receiver box-read (`Ꮡs.Value.cg`, `Ꮡs.Value.cg.txCtx()`) and the captured-local copy name are now applied consistently across the whole body. (Guarded by the `NestedLambdaReceiverField` behavioral test — a direct-ж receiver method whose closure holds a nested closure followed by a non-call receiver field read, a field-method call, and another field read, all verified to render `Ꮡs.Value.<field>` and output-compared vs Go; cleared `database/sql`'s 2×CS8175 and re-baselined `DeferValueFieldPtrReceiver` whose defer-then-body sequence exercises the same restore.)

### A package-level global referenced inside a closure is not captured

A **package-level global** referenced inside a closure is *not* captured at all — it is a C# static, accessed live. A value snapshot (`var gʗ1 = g`) would copy the struct (so `&gʗ1` has no box → CS0103, and writes through the global from inside the closure would be lost) and is semantically wrong, since Go reads/writes the live global. For an address-taken (heap-boxed) global the closure references the static box `Ꮡg` directly — a method call routes as `Ꮡg.method()` and a field address as `Ꮡg.of(T.Ꮡfield)`. (Guarded by `GlobalCapturedInClosure`; the runtime does this in every `systemstack(func(){ … mheap_ … })`.)

### A variable DECLARED INSIDE a closure is not captured BY it
The escape analysis heap-boxes a local when something *outside* its frame can reach its storage; a closure is one such route, because the emitted C# serves the shared variable through a `ж<T>` box. The closure arm of that analysis matched on any mention of the object lexically inside a function literal's body — and for a variable declared *there*, that mention is its own declaration. So a literal's own local was treated as if the literal closed over it:
```csharp
//  Go:   testing.AllocsPerRun(100, func() { var t Time; t.UnmarshalText(in) })
Δtesting.AllocsPerRun(100, () => {
    ref var tΔ1 = ref heap(new Δtime.Time(), out var ᏑtΔ1);   // ← 128 B, and ᏑtΔ1 is never used
    tΔ1.UnmarshalText(inʗ1);
});
```
The box `ᏑtΔ1` is **never referenced anywhere in the emitted body** — `UnmarshalText` is a `this ref Time` extension, which binds the variable directly — while the identical two statements written outside a closure emitted a plain `Δtime.Time tΔ1 = default!;`. The arm now skips an object whose declaration position lies inside the literal, and the emission is the plain local. That was the other 128 of `time`'s 216.

The narrowing direction of an escape rule is the dangerous one — an under-box drops writes silently — so the proof is stated rather than assumed. Go scoping puts a literal's own local out of reach of every other frame, so there is nothing for a shared box to make visible; and every route by which such a local can *still* genuinely escape is decided by an arm that walks the **whole enclosing function body**, literal bodies included: `&x` / `&x.f` / `&x[i]` (the address-of arm), a pointer argument (the call arm), a `go`/`defer` use (their own arms), a capture-mode method call, and a pointer-receiver **method value** — Go's `(&x).M` written without the `&`. None of them is lost. The skip also keeps descending rather than stopping, so a literal **nested** inside the skipped one — which does close over the variable — still gets its own turn through the arm and still marks the escape.

(Guarded by the `ClosureLocalNoHeapBox` behavioral test. Five of its eight probes are the boxes that must SURVIVE, one per escape route, and each writes through the escaping alias and reads the value back so an over-narrowed rule prints a wrong number rather than merely emitting a different shape; the two positive probes are the pointer-receiver-method and copy-only shapes that now emit plain locals. Its N3 probe is the nesting case, and it is also the interaction test with the local-function rule above: the nested literal is emitted as a local function and captures the surviving box.)

## Capture hoists

### The capture snapshot is a STATEMENT, so every position that can hold a func literal owes it a hoist target
`var sʗ1 = s;` is a declaration statement, and C# has no statement slot inside an argument list — so a
capturing literal in expression position must send its snapshot to a **hoist sink** the enclosing
statement flushes ahead of itself. `convFuncLit` consults two, in order: the explicit
`LambdaContext.deferredDecls` builder that `go`/`defer`/`return` thread through the expression
contexts, then the ambient `v.hoistedDecls` that the assignment, expression-statement, `if`, `for`,
`range` and var-spec forms install. With neither, the decls emit **inline** and the file stops
parsing — `CS1003 ',' expected` + `CS1026 ')' expected` + `CS1002 ';' expected` + `CS1513 '}' expected`,
per site, the first of which reads as a defect in whatever token happens to follow.

Two positions had no sink, and between them they were the entire parse wall that kept `net/http`'s
1,352-verdict suite from ever running (28 diagnostics, 7 clusters, 2 of 35 converted test files):

- **A CONVERSION is transparent to the hoist.** `HandlerFunc(func(rw, req){ … conn … })` handed to
  `go Serve(ls, …)` (serve_test) is a *type conversion* whose operand is the literal. The conversion
  fork of `convCallExpr` rendered that operand with **no expression contexts at all**, so the wrapper
  made the literal invisible to the sink the `go` statement had already provided, and the snapshot
  landed inside the delegate-creation argument list `new Δhttp.HandlerFunc(var connʗ1 = conn; …)`.
  The fix adopts the ambient target for the conversion operand exactly as the `&composite` and
  composite-literal arms of `convExpr` already do — gated on a non-nil sink, so every other
  conversion keeps rendering with the nil contexts it always had. It matters only where a statement
  supplies the *explicit* builder and no ambient one, i.e. the `go`, `defer` and `return` forms; the
  statement forms that install `v.hoistedDecls` were already served by the second lookup.
- **A channel SEND supplied no sink of either kind.** `handlerc <- HandlerFunc(func(w, r){ … ts … })`
  (client_test) broke for the same reason with the wrapper, and a **bare** capturing literal sent to a
  channel broke without one. `visitSendStmt` now installs `v.hoistedDecls` and writes it before the
  send, the same shape `visitExprStmt` uses — under the same `useNewLine` test, because a `SendStmt`
  is a `SimpleStmt` and can also be a `for`/`if` init-or-post clause, where there is no statement slot
  to hoist into and the enclosing statement's own sink must stand.

The general rule the two share: **a conversion, an adapter wrap, or any other expression wrapper must
not be able to hide a func literal from the enclosing statement's hoist.** (Guarded by the
`CaptureHoistThroughConversion` behavioral test — ten shapes covering `go`, `defer`, channel send,
interface-element send, plain call, return and assignment positions, wrapped and bare, all
output-compared against `go run`; the seven affected shapes reproduce the CS1003/CS1026/CS1002
cluster with the fix reverted.)

### A func-literal ARGUMENT inside an `if`/`for` condition hoists its captures before the statement
The same capture-snapshot hazard occurs when a capturing func literal is passed as a call argument
**inside a condition**. `go/types` is dense with this shape — `underIs(t, func(u Type) bool { … })`,
`typeSet().is(func(t *term) bool { … })` — and the literal's snapshot declarations (`var suʗ1 = su;`)
are statements, invalid inside the condition expression. `visitExprStmt` / `visitAssignStmt` already
route such decls to a pre-statement hoist buffer (`v.hoistedDecls`), but `visitIfStmt` and
`visitForStmt` converted the condition with `convExpr(cond, nil)` and no hoist target, so the decls
were dumped inline into the condition (`if (tpar.underIs(` `var suʗ1 = su;` `(ΔType u) => { … }))` →
CS1003/CS1026/CS1002/CS1022/CS1513, ~63 errors across `go/types` alone). Both statement emitters now
convert the condition into a hoist buffer and write any collected decls on their own lines **before**
the `if`/`for`, mirroring `visitExprStmt`:

```csharp
ΔType su = default!;
var suʗ1 = su;
if (tpar.underIs((ΔType u) => {
    …
    if (suʗ1 != default!) { u = match(suʗ1, u); … }
})) { … }
```

The condition is converted **after** an `if`/`for` init clause (preserving capture-counter ordering),
and the `if`-with-init sub-block hoists between the init and the `if`. The traditional `for` reuses the
existing `ForVarInitMarker` slot — the hoisted condition decls are emitted at the same pre-`for` position
as the for-init heap allocations. The hoist buffer is empty for a condition with no capturing func-literal
argument, so the behavioral corpus is byte-identical; the only stdlib deltas are five `go/types` files
(`under.cs`, `builtins.cs`, `expr.cs`, `index.cs`, `instantiate.cs`) and one `crypto/tls`
`slices.ContainsFunc` call. This clears the **syntax-error layer** in those files (`go/types` had ~63
`CS100x`/`CS1026` from this one construct); it does not by itself green `go/types`, which compiles far
enough afterward to surface a deeper layer of latent semantic defects (a `map[token.Token]func()`
mis-lowered to a malformed explicit-interface `IDictionary`/`ICollection` implementation, named-slice
wrappers not satisfying `IArray.Source`, `token` resolution) — the frontier moves from syntax to
semantics, "progress, not regression." (Guarded by `FuncLitCaptureInCondition` — a func literal
capturing an enclosing map, passed as an argument inside a plain `if` condition, an `if` condition with
an init clause, a traditional `for` condition, and a while-style `for` condition, all output-compared vs
Go.)

### A func-literal ARGUMENT inside a return expression hoists its captures before the `return`

The third statement position with the same hazard: a capturing func literal passed as a call argument
**inside a return expression** — net/http's `findHandler` returns
`HandlerFunc(func(w ResponseWriter, r *Request) { … allowedMethods … }), "", nil, nil`, and traceviewer's
`MainHandler` returns `http.HandlerFunc(func(){ … views … })`. A **direct** func-literal result threads
`lambdaContext.deferredDecls` (the go/defer/return channel in `convFuncLit`), but a literal nested as a
call **argument** falls back to the pre-statement hoist sink, which `visitReturnStmt` never provided —
the snapshot declaration was dumped inline inside the return expression (10 syntax errors in `server.cs`,
a 4-error cascade in traceviewer). `visitReturnStmt` now provides the same hoist buffer as
`visitExprStmt`/`visitIfStmt`/`visitForStmt` and splices it before the `return` through its existing
`DeferredDeclsMarker` slot (ahead of any deferred tuple-deconstruction temps):

```csharp
var allowedʗ1 = allowed;
return (wrap((@string msg) => {
    fmt.Println(allowedʗ1[0] + ":" + msg, len(allowedʗ1));
}), "label", default!);
```

The buffer is empty for a return with no capturing-literal argument, so the behavioral corpus is
byte-identical. (Guarded by `ReturnTupleFuncLitArg` — a slice-capturing literal as a call argument inside
a three-result return tuple, and a map-capturing one inside a single-result return, output-compared vs
Go.)

### A func literal inside a RANGE expression hoists its captures before the loop

The fourth statement position, and the one the table-driven test idiom lands on constantly:

```go
for _, test := range []struct {
    desc string
    f    func()
}{
    {desc: "WithCancel(bg)", f: func() { c, cancel := WithCancel(bg); cancel(); <-c.Done() }},
    …
} {
```

The literal's snapshot declaration (`var bgʗ1 = bg;`) is a **statement**, and the composite-literal
element position it would be written into is pure expression context. `visitRangeStmt` converted the
range expression with `convExpr(rangeStmt.X, nil)` — no hoist target — so the decl was dumped inline
after the `f:` argument name, and the whole file died in a syntax cascade (`context`'s `x_test.cs`:
CS1003/CS1026/CS1002/CS1513/CS0106 ×195, from `TestAllocs` and `TestCause` alone).

`visitRangeStmt` now converts the range expression into a hoist buffer and **splices** the collected
decls in at the statement's own start — it records that offset before conversion and inserts there
afterwards (`spliceOutput`, the positional twin of `replaceMarker`), because the `foreach` header is
emitted much further down through a dozen different arms:

```csharp
    var bgʗ1 = bg;
foreach (var (_, test) in new TestAllocs_type[]{
    new(desc: "WithCancel(bg)"u8, f: () => { var (c, cancel) = WithCancel(bgʗ1); … }),
    …
}.slice()) {
```

The buffer is empty for a range expression with no capturing func literal, so the behavioral corpus is
byte-identical. (Guarded by `RangeExprFuncLitCapture` — slice- and map-capturing literals as struct
fields of a ranged composite literal, plus a bare `[]func(string)` element list, output-compared vs Go;
its A/B reproduces the cascade exactly.)

**The class, stated once:** `visitExprStmt`, `visitAssignStmt`, `visitIfStmt`, `visitForStmt`,
`visitReturnStmt`, `visitValueSpec` and now `visitRangeStmt` each provide the pre-statement sink. The
statement kinds that still do not — a `switch` tag, a `select` comm-clause, a bare send — have no
demonstrated corpus site, and each would repeat this failure exactly. They are deliberately not widened
speculatively: the tell that one has been reached is a syntax cascade whose first error sits on the line
after a `<name>:` argument label.

### The enclosing statement's hoist buffer does NOT extend into a literal's BODY

Those four positions all work the same way: the enclosing statement opens a hoist buffer, and a
capturing func literal inside it writes its snapshot declarations there. The buffer is a valid position
for **that literal's own** captures — they name bindings from the enclosing scope, which exists before
the statement. It is not a valid position for anything the literal's **body** hoists: a statement inside
the body opens its own buffer, and a nested literal whose captures name a binding declared *inside* this
body would be declared outside it.

`time`'s `BenchmarkStaggeredTickerLatency` nests three levels of `b.Run(…, func(b *testing.B){…})`. The
middle literal `make`s a `stats` slice; the innermost `go func(…)` captures it. The snapshots landed in
the OUTER literal's `b.Run(…)` statement buffer — two blocks above the declaration:

```csharp
for (nint tickersPerP = 1; …; tickersPerP++) {
    nint tickerCount = gmp * tickersPerP;
    var statsʗ1 = stats;                      // CS0103 — `stats` is declared below, inside bΔ2
    bΔ1.Run(…, (ж<Δtesting.B> bΔ2) => {
        var stats = new slice<…>(tickerCount);
```

`convFuncLit` now detaches `v.hoistedDecls` for the duration of the body walk and restores it after, so
a nested hoist can only reach a position inside the body. The literal's own captures are unaffected —
they are flushed before the body is converted, while the enclosing buffer is still installed. (Guarded
by `FuncLitArgCapture` case 15.)

---

[Index](README.md)
