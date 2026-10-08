# Source Generators

[Reference index](README.md) · [Summary of this topic](../ConversionStrategies.md#source-generators)
Several Go semantics cannot be written directly in C#, so the converter emits compact, attributed partial declarations and lets a set of Roslyn source generators (`src/gen/go2cs-gen/`, referenced as an analyzer by every converted project) synthesize the rest at compile time. This keeps the visible converted code close to the Go original. The principal generators and attributes:

* **`TypeGenerator`** — driven by a converted type declaration: a `partial` struct, class or interface declared in a package class, with its definition, when it has one, in the comment after its name. Emits the body of a converted type: a struct's members and equality, a named numeric/slice/array/map/channel type's wrapper and operators (see [Named Numeric Types and Constant Contexts](named-numeric-types.md#named-numeric-types-and-constant-contexts) and [Slices and Arrays](slices-and-arrays.md#slices-and-arrays)), and struct-embedding field/method promotion.
* **`ImplementGenerator`** — wires up Go's duck-typed [interfaces](interfaces.md#interfaces): finds the concrete types that satisfy each `partial interface` and emits the implementation glue and implicit conversions.
* **`RecvGenerator`** — emits pointer-receiver overloads for methods whose receiver is `this ref T`, so each is also callable through the pointer/box form. A **variadic** method keeps its `params` in the generated overload: cryptobyte's `func (b *Builder) add(bytes ...byte)` emits the value form `add(this ref Builder b, params Span<byte> bytesʗp)`, but the `ж<Builder>` overload had dropped `params` (a bare `Span<byte>`), so a call passing individual elements through a box (`c.add(0xff)`, `c` a `ж<Builder>` closure parameter) could not bind it and fell back to the ref-receiver value method — CS1929. `GetMethodInfo` now preserves the `params` modifier (the Go variadic is always the last, non-receiver parameter, so it never lands on the `this ж<T>` receiver). Guarded by `VariadicBoxReceiver` (a `*sink` with `add(bytes ...byte)` called on a box — via a closure and directly — with zero, one, several, and spread arguments, values vs Go).
* **`ImplicitConvGenerator`** — emits the implicit conversion operators that let a [named type](type-aliasing.md#type-definitions) and its underlying types be used interchangeably.
* **`StrGenerator`** — driven by an `sstring` parameter. For an [sstring twin](strings.md#an-sstring-twin-a-registered-function-gains-an-sstring-overload-that-calls-bind), emits the `@string` overload that forwards to the `sstring` member under `[OverloadResolutionPriority(-1)]`, and for a package-level function the canonical value delegate `<Name>ᶠ`. Types are rendered fully qualified from the symbols, because the converted file's `using` aliases are not in scope in a generated file.
* **`PartialStubGenerator`** — emits a throwing `partial` implementation for any bodyless `partial` method that has no other implementing part (e.g. assembly/cgo functions with no convertible body), while leaving real hand-written companion implementations untouched.
* **`NoInliningPartialGenerator`** — the other direction: for a `partial` method that HAS a body and no declaring part, emits the declaring part carrying `[MethodImpl(MethodImplOptions.NoInlining)]`. This is how the converter's no-inline mark reaches a declared method; see [The no-inline mark rides a generated declaring part](#the-no-inline-mark-rides-a-generated-declaring-part).

What the generators read is, for the most part, the code itself: where a type is declared, a receiver's `ref`, a parameter's type, and a few short comments written in Go's own spelling. [Marker comments: what converted code carries instead of an attribute](#marker-comments-what-converted-code-carries-instead-of-an-attribute) lists each of them and the attributes that remain. The records a package keeps out of view are classified in [Extended attributes: what stays on the declaration and what moves](#extended-attributes-what-stays-on-the-declaration-and-what-moves).

**A defined type over a NAMED type passes the underlying's name to the wrapper template, never as a
kind.** `type MyBool bool` and `type winlibcall libcall` reach `InheritedTypeTemplate` with the
underlying's name as its `TypeClass`, and that name is read (the `bool` arm prints Go's lowercase
`true`). The template's KIND arms key on six values the kind arms set themselves — `Array`, `Slice`,
`Map`, `Channel`, `Pointer`, `Numeric` — so a user type spelled exactly one of them took that kind's
template: mapstructure's tests declare `type MapCopy Map` over a struct named `Map`, which generated
`IDictionary` members over a struct. `NamedUnderlyingTypeClass` re-keys only those six names, so such
a wrapper generates exactly as one over any other name. (Guarded by GenTests'
`KindNamedUnderlyingTests`, which compares each of the six with a wrapper over `Plain`.)

**A generator's view of ACCESSIBILITY is provisional — its own output is what supplies the access modifier (2026-07-25).** The converter emits a Go type as a bare `partial interface X` (or `partial struct X`) nested in the package class and leaves the access modifier to `TypeGenerator`, which derives it from the Go export convention (`GetScope` — `public` for an exported name, `internal` otherwise; an explicit modifier on the converter's part wins). A C# nested type with no modifier is **private**, so until that generated partial exists the declaration is private — accessible from inside its own package class and *inaccessible from any other class in the assembly*. A generator cannot see its own output, so a semantic query that crosses package classes sees the provisional accessibility, not the real one.

That bit io's external test package, and it is the first shape in the corpus that can hit it: `io_test` and `io` compile into ONE assembly (the recompile test-project model), as two classes. `io_test.closer : io.Closer` and `io_test.testMultiWriter_sink : io.Writer` bound their base to an `IErrorTypeSymbol` (`CandidateReason.Inaccessible`, candidate `go.io_package.Closer/Interface/Private`) with **zero members**, so every method the base contributes silently vanished from the generated interface shell and adapters — while the FINAL compilation, which does have the generated `public partial interface Closer`, still demanded them: four distinct `CS0535` (six sites) across `Δcloser<T>`, `ΔcloserᴛObj`, `PipeReaderжcloser`, `PipeWriterжcloser`, `ΔtestMultiWriter_sink<T>` and `bytes_BufferжtestMultiWriter_sink`. The same `io.Closer` resolved normally as a base of `io.ReadCloser` *inside* `io_package`, and `fmt.Stringer` resolved normally from a referenced assembly (already `public` in metadata) — which is what made the failure look spelling-related. It is not: writing the base `global::go.io_package.Closer` fails identically.

`Common.GetAllBaseInterfaces` replaces the raw `AllInterfaces` walk in `InterfaceDeclarationSyntaxExtensions.GetInterfaceMethods` and in both of `ImplementGenerator`'s method-collection paths. It recovers an `Inaccessible` base from the error symbol's `CandidateSymbols` when the candidate is an interface declared in **this compilation's assembly**, and folds in that recovered base's own transitive bases (`AllInterfaces` cannot traverse through an error symbol). The recovery is sound rather than a bypass: the generator is about to declare that very type `public` or `internal`, both reachable from anywhere in the assembly. A genuinely inaccessible *foreign* type keeps its error symbol.

The deeper alternative — having the converter emit the access modifier on its own partial, so the pre-generation source stops understating it — would remove the whole class of provisional-accessibility blind spots, but writing that modifier onto the *inline* converted type declaration re-baselines every converted `.cs` in the corpus and in ~490 behavioral goldens, and coarsens the Go-shaped declaration the converter works to keep readable. It was left on the table here and taken the following day in the form below. (No behavioral guard is possible for this row: one behavioral project is one Go package is one C# assembly, so two package classes never share an assembly there. The reproducer is the `-tests` pipeline on `io`, whose six `CS0535` clear; gated by the full behavioral suite 490/490, the 302-package corpus build, and the bytes/strings/encoding/binary/strconv pipeline canaries all at banked counts.)

## `package_info.cs`'s `TypeAccessibility` section pins each type's accessibility IN SOURCE

**Resolved (2026-07-25).** The blind spot above is closed at the root, without touching a single converted `.cs`: `package_info.cs` gains a **`TypeAccessibility`** section, emitted **inside the package class body** (its entries are type declarations, and the types they name are nested in that class), carrying one condensed single-line partial declaration per converter-emitted converted type:

```csharp
[GoPackage("io")]
public static partial class io_package
{
    // <TypeAccessibility>
    internal partial struct discard {}
    internal partial struct nopCloser {}
    public partial interface Closer {}
    public partial interface Reader {}
    public partial struct LimitedReader {}
    // </TypeAccessibility>
}
```

C# lets a partial type carry its access modifier on any **one** of its parts, so the inline `partial interface Closer` stays bare and Go-shaped while this part fixes the accessibility — and it is fixed in **source**, so a generator's semantic query across package classes sees `public`/`internal` instead of the provisional `private`. The section carries its own explanatory prose in the style of the file's other sections; a package info file written before the section existed has the prose and markers inserted on the next conversion (`ensureTypeAccessibilitySection`), so no migration step is needed.

Details that make it a pure relocation of the modifier rather than a change of it:

* **The rule is `TypeGenerator`'s, mirrored exactly.** `recordTypeAccessibility` uses the explicit modifier the converter emitted inline (the publicization pre-pass's `public`) when there is one, and otherwise `generatedTypeScope`, a Go-side mirror of `Common.GetScope`. Deliberately **not** `getAccess`: `GetScope` reads the C# identifier verbatim, so a `Δ` collision-rename (a Greek capital) reads as exported where `getAccess` strips the prefix first. Mirroring the generator is what keeps the corpus's effective accessibility byte-for-byte unchanged and avoids `CS0262` between the two parts.
* **Generic types repeat the type-parameter list, never the constraints** (`internal partial struct entry<K, V> {}`) — a partial declaration may leave constraints to another part. An arity-0 *constraint* interface repeats the CRTP `<ΔT>` marker list the inline declaration carries.
* **Every partial-emitting kind is covered:** struct (defined struct, map, channel, slice/array, defined-over-selector), class (named pointer types), interface. A named **func** type is emitted as a C# `delegate`, which is not partial and already carries its own modifier, so it has no entry.
* **Hand-owned files are deliberately excluded.** A file whose destination `.cs` carries `[module: go.GoManualConversion]` has its converter output redirected to the non-compiled `.cs.auto` sibling, so the declarations that actually compile are the author's — their kind, name and modifier are the author's to choose, and a generated entry could contradict them (`CS0261`/`CS0262`) or conjure a phantom empty type. Across the 302-package corpus that leaves 19 marked files, of which 12 declare converted types (~48 declarations, mostly `runtime/runtime2.cs`) — and exactly **one** interface among them, `sync.Locker` in the hand-rewritten `sync/mutex.cs`. That single declaration is why **`Common.GetAllBaseInterfaces`'s `Inaccessible` recovery is RETAINED** rather than retired: it is now a narrow backstop for hand-owned declarations instead of the primary mechanism. Writing the modifier explicitly on a hand-owned declaration removes the need for it there.
* **Test variants get the same treatment, split by owning class.** Under the recompile test model the production entries reach `package_test_info.cs` through the verbatim seed from `package_info.cs`, the internal variant's test-local types merge into that same (production-class) section, and the **external** variant's types — which live in `<pkg>_test_package` — go to `package_info_external_test.cs`, whose first and only class is the test class. `writeExternalVariantMetadata` clears the accessibility set before writing `package_test_info.cs` so a test-class type can never land in the production class, and the unit is now written whenever the external variant declares types, not only when it records test-anchored `GoImplement`/`GoImplicitConv` attributes. Under ordinary reference the test class is the anchor; under white-box reference the external test class is preferred, with the internal bridge used when no external variant exists.
* **The generator still writes the modifier on its own part** — the section is a third part with the *same* modifier, which C# accepts. Removing it is not safe while the section deliberately does not cover hand-owned declarations: those parts would become `private`.
* **Generator-side consequence:** a converted type now has two converter-written parts, so any lookup that resolves a type by **name** through the syntax trees can land on either. `Compilation.FindStructDeclaration` and `ImplicitConvGenerator.GetStructDeclaration` — both of which go on to read members or the definition comment — now prefer the converter's own declaration (`Common.IsGoTypeDefinition`), falling back to first-match. Without that, syntax-tree order (i.e. compile-item order, where `package_info.cs` sorts ahead of the package sources in roughly half the corpus) would silently decide.

Measured, with a positive control (the `-tests` pipeline on `io`, whose `CS0535` cluster is the reproducer): recovery **on** + section on → 0 `CS0535`; recovery **neutered** + section on → 0; recovery neutered + section **off** → the cluster returns. Gates: full behavioral suite 490/490 across all four phases (460 output-compared, 30 skipped) with **every** main `.cs` golden byte-identical — the churn is 490/490 `package_info.cs`, additions only; seeded 305-package reconvert (14/14 `.cs.auto`, no marked file clobbered) + overlay + full corpus build 0 errors; converter `go test` and `GenTests` green; pipeline canaries at banked counts (errors 61, encoding/csv 71, io/fs 18, bytes 81 with 7 disclosed).

## Extended attributes: what stays on the declaration and what moves

The converted type declaration is the line a reader of converted code actually reads, so every *other* attribute stamped on it is machinery competing with the Go original for that reader's attention. `package_info.cs` already exists to hold per-type records out of view, and the `TypeAccessibility` section above already moved the access modifier there. **A stamp can follow it whenever its consumer reads the attribute off the TYPE rather than off a particular declaration** — C# unions the attributes of every part of a partial type, so which part carries one is invisible to runtime reflection and to any generator that resolves the symbol.

That single criterion classifies the whole surface. Apart from the no-inline mark, which a declared method now carries as the word `partial` ([below](#the-no-inline-mark-rides-a-generated-declaring-part)) and a func literal as `[MethodImpl(MethodImplOptions.NoInlining)]`, and the explicit layout of a struct with a zero-size field (`[StructLayout]`/`[FieldOffset]`), the converter stamps nothing from the BCL. The type's own definition, a receiver, a struct tag, an embedded field and an array length are no longer attributes at all ([Marker comments](#marker-comments-what-converted-code-carries-instead-of-an-attribute)), so the vocabulary of attributes is exactly this:

| Stamp | Lands on | Consumer | Verdict |
|---|---|---|---|
| `[GoValueClone("f1", "f2")]` | struct | `TypeGenerator`, reading field names to emit `Clone()` | **Moved** |
| `[GoLocalName("Point")]` | struct (lifted function-local named type) | golib's reflection bridge, `GoReflect.TypeNaming` | **Moved** |
| `[GoInit]` | **method** | the **C# compiler** — it is a `using` alias for `ModuleInitializerAttribute` | **Must stay** — the compiler requires it on the method it initializes with |
| `[GoPackage]`, `[GoImplement<T,I>]`, `[GoImplicitConv<S,T>]`, `[GoTypeAlias]`, `[GoSStringTwin]` | package class / assembly | generators, runtime, and the converter's own next run | **Already there** — these are emitted into `package_info.cs` and never touched a mainline declaration |
| `[GoManualConversion]`, `[GoRequiresUnsafe]` | module | the converter | **Already off** — hand-written, module-scoped |
| `[GoInterfaceShell]`, `[GoTwinForwarder]`, `[GoReflectCompanion]` | interface / lambda / field | golib | **Not converter-emitted** — the first two written by the generators (`[GoTwinForwarder]` by `StrGenerator`, on a twin delegate's lambda), the last by hand |

So the movable set is `[GoValueClone]` and `[GoLocalName]`, and both moved. `[GoValueClone("intbuf")] partial struct pp {` reads `partial struct pp {`, with the record in `package_info.cs` carrying the rest:

```csharp
    // <TypeAccessibility>
    [GoValueClone("grid")] internal partial struct holder {}
    [GoValueClone("b")] internal partial struct inner {}
    internal partial struct row {}
    // </TypeAccessibility>
```

Mechanics worth knowing:

* **The attributes and the access modifier travel together, by construction.** `recordTypeAccessibility` takes the stamps as an argument and returns what the caller must still write inline — empty when the record absorbed them. So the two paths that write **no** record keep the stamps on the declaration and cannot lose them: a hand-owned file (whose emission goes to the non-compiled `.cs.auto` review sibling) and a `-tests` bridge unit (whose accessibility is inline because its metadata anchor can be a different test class). A hand-written conversion therefore still stamps `[GoValueClone]` inline, and it still works.
* **`TypeGenerator` reads the stamp across every partial declaration**, starting with the one its receiver matched and continuing through the symbol's other `DeclaringSyntaxReferences`. That is what makes both placements equivalent rather than one replacing the other. The match itself stays syntactic, as it is for every attribute this generator reads.
* **The section sorts on the DECLARATION, not the line.** `typeAccessibilityKey` strips the attribute prefix before comparing, so a stamped entry keeps the place its accessibility/kind/name earns instead of being pulled into a leading block by its `[`. Sorting the raw line is legal but scrambles a section whose whole value is being readable at a glance.
* **Not a semantic change anywhere.** The relocation moves *where the attribute is written*, exactly as the `TypeAccessibility` section moved where the modifier is written. Nothing observes a difference: the generated `Clone()` is identical, and `GoReflect`'s `%T` output is identical.


## Marker comments: what converted code carries instead of an attribute

Converted code carries very few attributes. Where C# needs a fact that Go's syntax states without a keyword,
the converter puts that fact in one of two places a Go reader already looks:

- **In the signature.** A pointer receiver is `this ref T`. A string-twin function takes an `sstring`. A Go type
  is a type declared in the package class. Nothing is added to say so twice.
- **In a short comment written the way Go writes it.** An embedded field, a struct tag, an array length and a
  defined type's underlying type each become a comment beside the declaration they describe.

At build time the go2cs source generators read the signature or the comment and write the attribute form into
a generated part of the same type. The runtime and reflection read that generated part. The converted file
stays the code a person reads; the attributes live where nobody has to.

| Go | Converted C# | What carries the fact |
|---|---|---|
| `func (b *Reader) Read(p []byte)` | `Read(this ref Reader b, slice<byte> p)` | the `ref` on the receiver |
| `type Scanner struct { … }` | `partial struct Scanner {` | its place in the package class |
| `type Duration int64` | `partial struct Duration /*num:int64*/;` | the comment after the name |
| `struct { io.Reader }` (an embedded interface) | `/*embed*/ public Reader Reader;` | the comment before the field |
| ``Method string `json:"method"` `` | ``public @string Method; /*`json:"method"`*/`` | the comment after the field |
| `func hash(b [32]byte)` | `hash(/*[32]*/ array<byte> b)` | the comment before the type |
| `func DecodeRuneInString(s string)` | `DecodeRuneInString(sstring s)` | the `sstring` parameter |

### What each one carries, and why a Go reader does not need it

**A pointer receiver is `this ref T`.** Go's `func (b *Reader)` becomes an extension method whose receiver is
passed by reference, and a value receiver `func (d Duration)` becomes `this Duration d`. The `ref` is the whole
fact: it is what lets the method change the caller's value, as Go's pointer receiver does. From it the
generator writes the overload that takes a pointer (`ж<Reader>`), and the runtime decides which methods belong
to `Reader` and which to `*Reader`. A Go reader sees a receiver and whether it is a pointer, which is what the
Go line showed.

<!-- source: src/core/bufio/bufio.cs:215 -->
```csharp
public static (nint n, error err) Read(this ref Reader b, slice<byte> p) {
```

**A Go type needs no mark.** Every struct and interface the converter declares inside a package class is a Go
type, so the declaration is just `partial struct Scanner {`. The generator completes it (its constructor,
equality, promoted members) and marks its own generated part, so reflection reports a defined Go type exactly
as before.

**A defined type states its underlying type after its name.** `type Duration int64` has no body in C#; the
generator builds one from the comment. `num:int64` says "a number over `int64`", and the other forms name a
slice, a map, a channel, a pointer or another named type the same way. If the comment is deleted the type loses
its arithmetic and conversions, and the build fails rather than running wrong.

<!-- source: src/core/time/time.cs:910 -->
```csharp
partial struct Duration /*num:int64*/;
```

**An embedded field says `/*embed*/`.** Go embeds a field by leaving out its name; C# needs the name, and a
Go field may share its type's name without being embedded, so the name alone cannot say it. The comment does,
on an embedded interface, an embedded predeclared type, and a pointer to one. Promotion of the embedded
type's methods, and `reflect`'s `Anonymous`, follow from it. An embedded struct value needs no comment: it is
a `partial ref` property (`public partial ref Inner Inner { get; }`).

<!-- source: src/go2cs/testdata/markercomments/main.cs:41-42 -->
```csharp
    /*embed*/ public Reader Reader;
    /*embed*/ internal nint @int;
```

**A struct tag keeps Go's spelling, at the end of the field's line.** The tag sits where Go puts it and reads
as Go wrote it, backquotes included. `encoding/json` and every other package that reads tags through `reflect`
receives the same string. A tag that a comment cannot hold as written uses Go's quoted spelling instead: one
that contains a backquote, a `*/`, or any character Go does not count as printable (a tab, or U+2028), or that
is not valid UTF-8. Inside the quoted spelling a `*/` is written `*\x2f`, so the tag cannot end the comment
early. A grouped declaration carries the tag once, as Go does.

<!-- source: src/go2cs/testdata/markercomments/main.cs:46-47, 50 -->
```csharp
    public @string Plain; /*`json:"plain"`*/
    public nint Grouped, Pair; /*`json:"g"`*/
    public @string Quoted; /*"a:\"`b`\""*/
```

**An array length is Go's own prefix, before the type.** C# has no fixed-length array type, so `[32]byte`
becomes `array<byte>` and the length would be lost. The comment `/*[32]*/` keeps it, written as Go's array
prefix, on a parameter, on a field, and on a defined type whose inner lengths its definition does not spell.
Nested arrays list each length in order.

<!-- source: src/go2cs/testdata/markercomments/main.cs:68, 77, 83 -->
```csharp
    internal /*[3]*/ ж<array<nint>> p;
internal static nint hash(/*[32]*/ array<byte> b) {
internal static void fill(nint n, ref array<int32> p, /*[4][8]*/ array<array<byte>> grid) {
```

**A string twin is the function that takes an `sstring`.** A few registered functions exist twice, so that a
string literal passed to one binds a view and is not copied (see
[An sstring twin](strings.md#an-sstring-twin-a-registered-function-gains-an-sstring-overload-that-calls-bind)).
The converter writes the Go body once, on the member whose parameter is an `sstring`, and the generator writes
the `@string` overload from it. No other converted function takes an `sstring` parameter, so the parameter
type is the mark.

<!-- source: src/core/unicode/utf8/utf8.cs:213 -->
```csharp
public static (rune r, nint size) DecodeRuneInString(sstring s) {
```

### What keeps a comment safe once the program depends on it

- **A Go comment is never taken for a marker.** When the converter carries one of Go's own comments into the
  C#, and that comment opens the way a marker does, it is written with a space after the `/*`. A Go
  `/*embed*/` arrives as `/* embed*/`, which no generator reads. A marker is also read in one position only,
  the one the converter writes it in.
- **A record that names a member the type does not have is refused by name.** The runtime checks each
  generated record against the type the first time it is read, and stops with the member's name instead of
  applying a fact to the wrong thing.
- **A comment no record can carry is a build error** (`GO2CS0003`), never a silent loss. So is a record whose
  key would match two methods of one type; the error names both.
- **The generators' tests are built from the converter's own output**, so a change in how the converter spells
  a marker fails a test instead of passing against a hand-written copy of the old spelling.

Converted files are regenerated from Go, not edited by hand, so a marker is normally written and read by
tools on both sides.

### What stays an attribute, and why

An attribute stays in converted code when the C# compiler or the .NET runtime reads it as an attribute from
that exact declaration, or when the declaration has no name a generated record could use.

<!-- attribute-shown: a converted func literal or local function keeps [GoArrayDims] on its parameters -->
| Still an attribute | Where | Why it cannot move |
|---|---|---|
| `[GoInit]` | a package's `init` functions | It is .NET's module-initializer attribute under a Go name. The C# compiler acts on it, and initializers run in declaration order, which is Go's package initialization order. |
| `[StructLayout]`, `[FieldOffset]`, `[DllImport]` | a struct or function that describes native memory or calls the operating system | The .NET runtime lays out memory and binds native calls from them. |
| `[MethodImpl(MethodImplOptions.NoInlining)]` | a func literal, a local function, an `init`, or a declaration with no body | The JIT reads it from the method itself. A declared method carries the same mark as the word `partial` (see [The no-inline mark rides a generated declaring part](#the-no-inline-mark-rides-a-generated-declaring-part)); a lambda or a local function cannot be partial, an `init` keeps its place in the file, and a declaration with no body is already the declaring part. |
| `[GoArrayDims(N)]` | an array parameter of a func literal or a local function | The compiler names those methods, so no record can refer to them. |
| `[GoWrapper]` | the lambda that wraps a method expression | The runtime reads it from the lambda's own method, which has no declaration to attach a record to. |

A package's bookkeeping attributes (`[GoPackage]`, `[GoImplement]`, `[GoImplicitConv]`, `[GoTypeAlias]` and
the rest) are in `package_info.cs`, not in the code a reader opens.

### Hand-written code

<!-- attribute-shown: hand-written code keeps the attributes -->
A hand-written file (a `*_impl.cs` companion, or a whole file marked as a manual conversion) keeps the
attributes: `[GoRecv]`, `[GoType]`, `[GoType("…")]`, `[GoEmbedded]`, `[GoTag]`, `[GoArrayDims]` and `[GoStr]`
all mean what they always have, and every generator and the runtime accept either spelling. Nobody writing C#
by hand has to adopt a comment.

Inside a converted package the signature rules read a hand-written file too: an unmarked `this ref` receiver
is a pointer receiver, and an `sstring` parameter makes a twin. The repository's guards require the attribute
on such a method, so a hand-written file always says what it means.

<!-- attribute-shown: a hand-owned package reads the attributes only -->
In a package written entirely by hand (`testing` and `unsafe`), marked `[assembly: GoHandOwnedPackage]`,
nothing is inferred from a signature or from where a type is declared: `[GoRecv]`, `[GoType]` and `[GoStr]`
are the only way to say those things there.

<!-- source: src/core/internal/poll/fd_mutex_impl.cs:150 -->
<!-- attribute-shown: a hand-written file keeps the attribute -->
```csharp
[GoRecv] internal static bool rwlock(this ref fdMutex mu, bool read) {
```

## The no-inline mark rides a generated declaring part

**Rule (owner ruling 2026-10-06, `docs/PLAN-marker-comment-parity.md` section 10).** A function whose frame
a Go stack walk counts must not be inlined by the JIT, or `runtime.Caller`, `runtime.Callers`,
`runtime.Stack` and the profiles name the wrong function. `computeNoInliningClosure`
(`src/go2cs/callerInliningAnalysis.go`) decides which functions those are: a direct `runtime.Caller` /
`runtime.Callers` user, a thin forwarder into one, a function inside a constant skip window, a hop into a
skip-counted walker, a `//go:noinline` function, a goroutine creator, and a thin allocator in a package
that reads the heap profile. How each takes the mark:

| Shape | Emitted form | Why |
|---|---|---|
| Declared function or method with a body | `partial` where the `[MethodImpl(MethodImplOptions.NoInlining)] ` prefix stood: `internal static partial @string ptr(this ref counter c) {` | `NoInliningPartialGenerator` writes the declaring part with the attribute; C# merges the attributes of both parts into the one compiled method |
| Func literal (lambda) | `[MethodImpl(MethodImplOptions.NoInlining)] @string () => here()` | a lambda has no partial form |
| Local function (a literal only ever called) | `[MethodImpl(MethodImplOptions.NoInlining)] @string local() {` | a local function has no partial form |
| `init` | `[MethodImpl(MethodImplOptions.NoInlining)] [GoInit] internal static void initΔ2() {` | C# runs module initializers in declaration order, and a partial method's declaration is its DECLARING part, a generated file that sorts after every source file: a `partial` init ran after the package's other inits (found by G's stacked record, ruled 2026-10-06). The generator refuses a hand-written partial init by name (GO2CS0003, an error) |
| Declaration with no Go body | `[MethodImpl(MethodImplOptions.NoInlining)] internal static partial … f(…);` | already the declaring part; its body is a `*_impl.cs` companion |
| The `runtime` package's own `init` functions | neither | they are emitted as never-called methods, `/* [GoInit] runtime bootstrap init - not run; .NET is the runtime */`, with no attribute in either rendering |
| Hand-owned files | the attribute, written by hand | not converter output |

The converter still registers `System.Runtime.CompilerServices` (or, in a file with a `using static`,
the `MethodImplAttribute`/`MethodImplOptions` aliases) for a carrier, so a file's using block reads the
same in both renderings and the change to a converted file is exactly the signature line.

**What the generator matches.** `NoInliningPartials.IsCarrier`: a `partial` method with a body whose symbol
has no declaring part (`IsPartialDefinition: false`, `PartialDefinitionPart: null`). A converted bodyless
declaration plus its `*_impl.cs` body always has a declaring part, so it never matches; neither does a
method without `partial`. A match that is a module initializer is refused by name (GO2CS0003) and gets no
declaring part, so the build stays red at that method.

**What it writes.** One `<file>.noinline.g.cs` per source file that holds carriers. The signature is
copied as text from the implementing part with its attributes, body and the space before the body
removed, so modifiers, return type (tuple element names included), type parameters, parameters (`this`,
`ref`, `params`, names) and constraints match exactly; a mismatched parameter name would be CS8826. The
file repeats the source file's `extern alias` and `using` directives, except `global using` (already in
scope everywhere; repeating an alias one is CS1537), then the namespace's own usings, then the namespace
and the containing type chain re-opened as `partial`. The attribute is written fully qualified, so it
binds whatever the file's usings are. A default parameter value would have to move to the declaring part
(CS1066 on the implementing one); the converter writes none on a carrier.

**Readers of the mark.** `RecvGenerator` makes a pointer-receiver method's `ж<T>` overload no-inline when
the method is, and `StrGenerator` does the same for an `sstring` twin's `@string` forwarder
(`RecvGenerator.HasNoInliningMark`). Neither sees another generator's output, so both test the carrier
shape as well as the attribute.

**Failure mode.** If the generator does not run, every carrier is CS0759 (an implementing part with no
declaring part): a build failure, never a silently inlinable method. A NuGet package carries the merged
attribute in its compiled metadata.

**Guards.**
* GenTests `NoInliningPartialGeneratorTests` compiles the converter's rendering of each carrier shape
  (thin forwarder, `params`, named-tuple result, generic, `this ref` and `this` receivers, a pointer
  receiver on a generic type, `Main`) and reads `NoInlining` back from the emitted metadata; checks that
  a hand-written partial `init` is refused by name;
  checks the declaring part's text; checks that a lambda and a local function keep their own attribute and
  gain nothing, that a hand-owned declaration-plus-body pair gains nothing, that a carrier beside a
  `global using` alias compiles, and that the `ж<T>` overloads of `ptr` and `get<T>` are no-inline while
  an unmarked method's is not.
* Behavioral `NoInlinePartial` runs with tiered compilation off (`runtimeconfig.template.json`), so the
  first call is optimized code, and compares its output with `go run`: every shape prints the frame
  `runtime.Caller` names. Built with a generator that writes the declaring part without the attribute,
  six of its lines (`plain`, `variadic`, `generic`, `ptr`, `val`, `get`) print `main.main` instead.
* Behavioral `InitOrderNoInline` puts inits in three files, one of them marked by `//go:noinline`, and
  compares the order they print with `go run`; with the marked init written `partial` it ran last.
* The converter's frame tests (`noinlineDirective_test.go`, `callerSkipWindowFrames_test.go` and the
  others) read the mark through `keepsOwnFrame` (a ` static partial ` declaration line that ends in a
  body), and `goCreatorFrame_test.go` reads a literal's attribute directly.

---

[← The `go.golib` support namespace](golib-namespace.md) · [Index](README.md) · [The standard-library conversion applies `-tags purego` →](purego.md)
