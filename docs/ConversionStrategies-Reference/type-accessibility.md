# Type Accessibility and Publicization

[Reference index](README.md) · [Summary of this topic](../ConversionStrategies.md#functions-and-methods)

This page covers the C# access modifier each converted declaration receives, and when an unexported Go type is emitted `public` because an exported signature exposes it.

## Publicization

### Publicization decides WHAT a type's modifier is; the test-bridge arm only decides WHERE

`visitTypeSpec` writes a `[GoType]` declaration's access modifier from one of two sources, and they
answer different questions. `packagePublicizedTypes` answers *what* the modifier must be — an
unexported type reached by an exported field, var, or callable signature has to be `public` or C#
rejects the referrer (CS0050/CS0051/CS0052). `testInlineTypeAccess` answers *where* it is written: a
white-box bridge type carries its modifier inline rather than through `package_info.cs`'s
`<TypeAccessibility>` section, because its metadata anchor can be a different test class.

Asking the inline arm FIRST made it answer both — from the name alone — so a publicized bridge type
stayed `internal`. `context`'s internal test file declares `type testingT interface{…}` and the
exported `func XTestParentFinishesChild(t testingT)` that `x_test.go` calls; the publicization pre-pass
records `testingT` (it runs over the test-augmented package, so the exported `*types.Func` arm fires),
but the emission ignored it: `internal partial interface testingT` under a `public` method, CS0051 ×4.
Publicization now outranks, and the inline arm supplies the DEFAULT:

```csharp
[GoType] public partial interface testingT {   // was: internal
```

This is why `signatureReferencesUnexportedProductionType` (which downgrades an exported *test-file*
function whose signature names an unexported PRODUCTION type) is correctly restricted to production
types: a test-declared type is meant to be handled by publicization, and now is.

### An exported func type publicizes the unexported types in its signature
An EXPORTED named func type becomes a `public` C# delegate; an unexported type in its signature —
x/text/unicode/bidi's `type Option func(*options)`, where `options` is package-private — is then
less accessible than the delegate (CS0059, "inconsistent accessibility"). The type-accessibility
pass, which already publicizes the unexported types exposed by an exported struct field / package
var / method signature, also walks an exported named type whose underlying is a `*types.Signature`
and publicizes the unexported named types in its parameters and results:

```go
type options struct{ … }        // unexported
type Option func(*options)       // exported -> public delegate
```
```csharp
[GoType] public partial struct options { … }   // publicized to match the delegate
public delegate void Option(ж<options> _);
```

Only a package with an exported func type over an unexported type is affected (no golden churn). (Guarded by the `PublicizedFuncTypeParam` behavioral test.)

**A func-TYPED exported field or var publicizes the unexported types in the func signature.** The
accessibility walk that publicizes an unexported type exposed by an exported field / package var
(`collectUnexportedNamedTypes`, CS0052) peels `pointer`/`slice`/`array`/`map`/`chan` wrappers to reach
the element type — but stopped at a `*types.Signature`, so an unexported type reachable ONLY through a
func-typed field's signature was left `internal`. crypto/internal/hpke's

```go
type hkdfKDF struct{ … }                          // unexported
var SupportedKDFs = map[uint16]func() *hkdfKDF{…}  // exported var -> public field
```

emits `public static map<uint16, Func<ж<hkdfKDF>>> SupportedKDFs`, whose type embeds `hkdfKDF` through
the func RESULT — but `[GoType] partial struct hkdfKDF` defaulted to `internal`, less accessible than
the public field (CS0052). `collectUnexportedNamedTypes` now has a `*types.Signature` case that recurses
into the signature's PARAMS and RESULTS through the same named-only walk (which handles a nested func
result in turn), so `hkdfKDF` is publicized to `[GoType] public partial struct hkdfKDF` (and its exported
methods go public via the receiver-access cascade). Both sides of the signature are covered — a func
PARAMETER exposes an unexported type just as a func RESULT does (`var Appliers = []func(*cfg)` →
`public static slice<Action<ж<cfg>>> Appliers`, publicizing `cfg`). This routes through the named-only
`collectUnexportedNamedTypes`, NOT the signature-context `collectSignatureTypes`: a lifted anonymous
struct/interface written in the func signature stays the CS0050/CS0051 signature domain, so only genuinely
func-reachable NAMED types publicize here. (Guarded by the `FuncFieldUnexportedType` behavioral test — a
public `map[uint16]func() *hkdfState` var whose func result exposes an unexported type, plus a
`[]func(*cfg)` var whose func parameter exposes another, output-compared vs Go; both fail CS0052 without
the publicize.)

**A publicized wrapper reaches through an UNNAMED composite RHS to its element type.** A defined type whose `[GoType]` wrapper is emitted `public` (exported, or unexported-but-publicized) exposes its written RHS through the wrapper's `Value`/ctor/indexer/operators, so an unexported RHS type must be publicized too. This holds not just for a NAMED RHS (`type EncoderBuffer encoder`) but for an UNNAMED composite RHS whose ELEMENT is an unexported named type: `type ringElement [256]fieldElement` exposes `fieldElement` through the array-wrapper's indexer/`Value`/`ToSpan`, so `fieldElement` must be publicized (crypto/internal/mlkem768, CS0050/CS0051/CS0053/CS0054/CS0056/CS0057). `collectPublicizedWrapperRHS` therefore feeds the RHS unconditionally to the pointer/slice/array/map/chan-peeling walk (`collectUnexportedNamedTypes`) rather than gating on a named RHS. The walk has no `*types.Struct` case, so a struct RHS stays a no-op — an exported field of an unexported struct-field type is the CS0052 domain and is intentionally left internal. (Guarded by the `NamedArrayWrapper` extension — an exported `Grid [3]unit` over an unexported `unit`, output vs Go.)

### A test-file exported helper over an unexported PRODUCTION type is emitted `internal` (the MIRROR)
The publicization passes above run over a single `*types.Package` and raise a production type's
accessibility to match the exported production surface that exposes it. A `_test.go`-declared helper
is the **mirror** case and needs the *opposite* resolution. In the `-tests` pipeline the production
sources are converted first and independently (no test files), so an unexported production type is
already emitted `internal` on disk; the test files are converted afterward. Go's `strconv/internal_test.go`
declares an EXPORTED helper returning that internal production type:

```go
// internal_test.go (package strconv) — exports access to strconv internals for tests
func NewDecimal(i uint64) *decimal { … }   // decimal is package-private, emitted `internal`
```

Emitting `NewDecimal` `public` (its capitalized name) makes it a public method whose result is the
less-accessible internal `decimal` — CS0050. Publicizing `decimal` is **not** the fix here: production
was already emitted (and is not re-emitted in the test pass), so the map entry would be inert, and a
public API surface for a test-only helper is semantically wrong. In the recompile test model the test
assembly is self-contained (production + internal-`package strconv` + external-`package strconv_test`
files all compile into ONE assembly, with no cross-assembly consumer of a test symbol), so the correct
and sufficient resolution is to downgrade the helper to `internal`:

```csharp
internal static ж<@decimal> NewDecimal(uint64 i) { … }   // was public → CS0050; internal ≤ any prod access
```

`visitFuncDecl` downgrades an exported free function (`Recv == nil`) declared in a `_test.go` file when
its signature references, in any param/result position (peeling pointer/slice/array/map/chan),
an unexported same-package named type **declared in a production file**
(`signatureReferencesUnexportedProductionType`). The production-file restriction is essential and is
what distinguishes this from `sort`'s `example_multi_test.go`, whose exported `OrderedBy(...) *multiSorter`
returns an unexported type declared **in a test file**: that type is publicized AND re-emitted `public`
within the same test pass (the framework above), so its referrer compiles as public and must **not**
be flipped. Only a production-declared unexported type stays `internal`-on-disk and forces the
downgrade. The change fires solely inside `_test.go` conversion, so normal-path output is byte-identical
(check-no-regression clean across the behavioral corpus) and no already-validated package drifts
(`sort` re-validates 63/63, `SetOptimize(bool) bool` in the same file stays `public`). This is the
blocker that lets `strconv`'s test host reach compilation of its file-reading suites (`TestFp`/`TestAtof`
read `testdata/testfp.txt` via `os.Open` + `bufio.Scanner`). No behavioral guard is expressible — the
`-tests` recompile model has no normal-path analogue — so the guard is the `strconv` pipeline (its
`internal_test.cs` emits `NewDecimal` `internal`; the CS0050 no longer blocks).

**The same downgrade applies to a package-level VAR or CONST — the CS0052 half of the rule.** A
white-box test file's exported *value* faces the identical arithmetic on a *field* rather than a
method: `internal/cpu`'s `export_test.go` declares `var Options = options` over the production
`type option struct{…}`, and a `public` field of type `slice<option>` is **CS0052 — inconsistent
accessibility**, the field's type being less accessible than the field. `visitValueSpec` runs every
package-level var/const access through `testDeclaredValueAccess`, which applies exactly the
predicate the func rule uses (`typeReferencesUnexportedProductionNamed`, peeling
pointer/slice/array/map/chan) and downgrades to `internal` on a hit; the production-file restriction
and the self-contained-assembly reasoning carry over unchanged.

This half only became reachable when the white-box **bridge class** started carrying an access
modifier. Before [the unconditional bridge metadata
unit](shadowing.md#test-suites-reference-the-production-project-instead-of-recompiling-it), an internal
test file's `partial class cpu_internal_test_package {` was the class's ONLY declaration, and a
top-level C# class with no modifier is `internal` — so its `public` members were internal *in
effect* and the inconsistency never arose. Making the bridge `public static partial` (which a
record-less mixed suite needs, or an extension method in an internal test file is CS1106) exposed
every such field at once. `internal/cpu`'s whole 8-verdict suite sat behind the one line.
(Guarded by `TestExportedTestFileVarOverProductionTypeIsDowngraded`, with three negative controls:
an exported production element type, a test-file-declared element type — which the publicize pass
re-emits `public` in this same pass — and a production-declared exported var over the same
unexported type, which stays `public` because the gate is the declaring FILE, not the type.)

### A FUNCTION-LOCAL type is emitted `internal` — its Go name's case carries no export meaning
Both rules above read an access modifier out of an identifier's first rune, which is exactly what Go's
export convention licenses — **for a package-level identifier**. A type declared *inside a function
body* is a different animal: it is unreachable from outside that function by construction, so Go
draws no visibility distinction between `S8` and `embed2` there. Neither is exported; neither can be.

go2cs hoists such a type to package scope under a `<Func>_<name>` identifier (see the lift sections),
and that hoist is where the meaning gets invented. Go's `encoding/json` `decode_test.go` is the
witness — one function, two local types, one a field of the other:

```go
func TestUnmarshalEmbeddedUnexported(t *testing.T) {
	type embed2 struct{ Q int }
	type S8 struct {
		embed2
		R int
	}
	…
}
```

The white-box bridge arm asked `generatedTypeScope` for the **local** name, so the two siblings landed
on opposite sides:

```csharp
[GoType("dyn")] [GoLocalName("embed2")] internal partial struct TestUnmarshalEmbeddedUnexported_embed2 { … }
[GoType("dyn")] [GoLocalName("S8")]     public   partial struct TestUnmarshalEmbeddedUnexported_S8 {
    public TestUnmarshalEmbeddedUnexported_embed2 embed2;   // CS0053 — less accessible than the property
}
```

and a lifted **anonymous** struct, which carries no modifier at all, was scoped by go2cs-gen's own
rule from the **hoisted** name — inheriting the case of the enclosing function, so
`TestEncoderSetEscapeHTML_type` came out `public` and its exported fields over the package-level
unexported `strMarshaler` were CS0052.

`localTypeAccess` (`typeAccessibilityOperations.go`) resolves both by emitting a function-local type
`internal`, consumed at the three points that finalize a modifier — `visitTypeSpec`'s bridge arm, and
the lift defaults in `visitStructType` and `visitInterfaceType`. `internal` is faithful (no Go
consumer outside the function can name the type) and sufficient (every emitted C# consumer — the
hoisted siblings and the converted function body — compiles into the same test assembly). Writing it
**inline** is load-bearing: go2cs-gen reproduces a modifier the declaration already carries and falls
back to its name rule only for a bare declaration, so pinning it inline is what stops the generator
from re-deriving `public` and colliding (CS0262). This was the entire compile wall of `encoding/json`'s
suite — **76 errors across CS0050/CS0051/CS0052/CS0053, four codes, one cause**.

The rule is scoped to the bridge arm. On the production path the modifier is left empty and
`recordTypeAccessibility` pins `generatedTypeScope` of the **mangled** name, which gives every local
type of one function the same modifier — uniform, and therefore consistent, though for a reason
nobody chose. The same latent mixture is expressible there (a function-local struct with an exported
field of a package-level unexported type); no corpus package presents it, and flipping production
local types would move a public value adapter's operand out from under it, so it is recorded rather
than pre-emptively changed. Guarded by `TestFunctionLocalTypesShareOneAccessibility`, which pins all
three shapes — the uppercase local, the lowercase local, and the anonymous lift reaching a
package-level unexported production type — and fails without the fix.

### A publicized unexported interface is emitted `public`
The accessibility pass records an unexported **interface** used in an exported surface exactly like a
struct or func type — testing's `type testDeps interface { … }` reached through `func MainStart(deps
testDeps, …) *M` is interned into `packagePublicizedTypes`, and `visitTypeSpec` sets
`pendingTypeAccess = "public "`. But on the EMISSION side, every top-level type-kind emitter consumes
`v.pendingTypeAccess` (struct, array, map, ident, the inline selector/star cases) *except*
`visitInterfaceType`, which dropped it — so the interface always emitted `[GoType] partial interface
testDeps`, defaulting to C# `internal`, less accessible than the `public` member that references it
(CS0051). `visitInterfaceType` now reads-and-clears `pendingTypeAccess` at entry (so the lifted/anonymous
interfaces it visits recursively see an empty value) and folds the modifier into the post-attribute slot,
emitting `[GoType] public partial interface testDeps`. Non-publicized interfaces are unchanged (no churn).
(Guarded by the `PublicizedInterfaceParam` behavioral test — an exported function taking an unexported
interface whose method returns a built-in type, output-compared vs Go.) The **transitive** cascade also
walks a publicized interface's method signatures: the `collectMethodSignatureUnexportedTypes` fixpoint step
walked a type's `named.NumMethods()` (declared receiver methods) but that is **0 for a defined interface**
— an interface's methods live on its underlying `*types.Interface`. It now also iterates
`iface.NumMethods()` for a publicized interface, so an unexported NAMED type in a public interface member's
parameter/result signature is publicized in turn (CS0051/CS0050).

**…and an interface member is public whether or not the GO method is exported (2026-08-08).** That
walk still ran each method through a gate that returned early on `!method.Exported()`. The gate is
right for a CONCRETE method — an unexported one emits `internal static … sockaddr(this
ж<SockaddrInet4> …)` and exposes nothing — and wrong for an interface member, which
`visitInterfaceType` emits with **no access modifier** and which C# therefore makes implicitly
**public**. Go's case convention simply does not survive into the emitted surface, so it is the
EMITTED C# accessibility, not the Go exportedness, that decides what must be lifted; the gate now
takes an explicit flag, set only on the interface arm. syscall's `Sockaddr` is the archetype and the
idiom is deliberate Go: `sockaddr() (unsafe.Pointer, _Socklen, error)` is unexported precisely so
that only the package can implement the interface — a SEALED interface — yet the emitted member
returns the unexported `_Socklen` from a public interface (**CS0050** on every unix flavor; Windows
spells the same method with `int32`, which is why the corpus never saw it). `go/types` is the other
reached case, and a subtler one: its exported `Object` interface has `color() color` and
`setColor(color)`, and the wrapper only ever compiled because the type's `Δ` collision-rename made
`TypeGenerator`'s name-based scope rule read the leading Greek capital as exported and emit it
`public` by accident. It is now publicized on purpose. (Guarded by
`typeAccessibilityInterface_test.go`, whose negative controls fail if the gate is dropped outright
rather than narrowed — a concrete unexported method must still publicize nothing.)

A public callable's signature can also reference a **lifted anonymous** type, which the NAMED-only cascade
above cannot reach — testing's `testDeps.CoordinateFuzzing(… corpusEntry …)` / `RunFuzzWorker` / `ReadCorpus`,
where `type corpusEntry = struct{…}` is an ALIAS to an anonymous struct. The signature type is not a
`*types.Named` but a lift (`corpusEntryᴛ1`), a synthesized name over a raw `types.Type` with no
`*types.Object`, so `packagePublicizedTypes` (keyed by object) cannot hold it. A parallel set
`packagePublicizedLiftedTypes` (keyed by the alias-stripped anonymous `types.Type`) fills the gap: a
SIGNATURE-context walker `collectSignatureTypes` — used by the exported-func, exported named-func-type, and
method/interface-method signature paths — records any lifted anonymous struct/interface it reaches, and the
lift emission in `visitStructType` consults `isPublicizedLiftedType` and emits `public`. This is deliberately
**signature-scoped** and does *not* fold into the shared named-only `collectUnexportedNamedTypes`: an exported
**field/var** of an anonymous struct is the CS0052 domain (a public struct/var over an internal anon field
type is legal while its own enclosing type is internal), so only signature positions lift — keeping golden
churn to the one genuinely-affected shape. (Guarded by the `PublicizedInterfaceAnonAlias` behavioral test — an
unexported interface publicized through an exported function, whose method both takes and returns a
`type = struct{…}` alias, output-compared vs Go; it fails to compile with CS0050/CS0051 without the lift
publicize.)

### Publicized unexported types make their exported methods public
An unexported Go type reachable through an exported surface (an exported var — `var BigEndian
bigEndian` — an exported field, or an exported function's signature) is emitted `public`
(`packagePublicizedTypes`, CS0052/CS0050). Its **exported methods** must then be public too —
Go callers hold such values through the exported var and call the methods cross-package, but the
receiver-based access rule alone rendered them `internal` (extension methods invisible outside
the assembly: `binary.BigEndian.Uint32(...)` CS1061). The receiver-access checks in
`visitFuncDecl` treat a publicized receiver as public, and `collectPublicizedTypes` **cascades**
through the publicized types' exported method signatures to a fixpoint (a newly public method's
unexported parameter/result types get publicized in turn, or the public method would be CS0050).
Unexported methods stay `internal` regardless.

---

[Index](README.md)
