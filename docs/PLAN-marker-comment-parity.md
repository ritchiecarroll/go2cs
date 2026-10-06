# PLAN — fewer attributes in converted code: what can move into a comment, what can go, what must stay

> **STATUS: RULED — the owner, 2026-10-06, the whole plan, with `[GoStr]` added (5.7).** His rule: everything that
> must stay an attribute stays an attribute; everything else that can be hidden in generated code should be. This plan
> is now the campaign's record. It began as a scout for the owner, who asked whether the trick behind the
> channel-direction fix (a fact carried by a short comment and a generated file instead of a visible attribute) could
> take other noisy attributes out of the converted code. Every number is measured over the committed tree at master
> `e1ad9dbc11` (2026-10-06) unless the text says otherwise; section 11's counts are at master `7098b8d3f9`.
>
> **The order (COORD, 2026-10-06).** TRAIN Q lands first, on today's rendering. The face lift is the train after it,
> a strict chain, because every step rewrites the same corpus files and goldens: A `[GoRecv]` removed (5.1), B plain
> `[GoType]` removed (5.2), C `[GoType("…")]` to a comment (5.3), F `[GoEmbedded]` (5.6), E `[GoTag]` (5.5), D
> `[GoArrayDims]` (5.4), S `[GoStr]` (5.7), then the kinds section 11 moves. F, E and D share one mechanism: a
> type-level record naming the member, and a reader that refuses a mismatch by name. Hand-written files keep the
> attributes and every reader accepts them, permanently.

---

## 1. The short answer

Yes for some, and the biggest win does not need a comment at all.

- **`[GoRecv]` can simply go.** It is on 5,107 method declarations in the converted standard library's production code,
  and in converted code it always says something the line already says: the method takes its receiver as `this ref`.
  The converter writes the attribute exactly when it writes `this ref`, so a generator and the runtime can read the
  signature instead. No comment replaces it.
- **Plain `[GoType]` can probably go too.** It is on 3,017 type declarations, and in converted code it marks every type
  except 28. Dropping it needs one rule for those exceptions, so it is second.
- **Four smaller kinds could move into a short Go-looking comment**: the underlying type of a defined type, array
  lengths on parameters, struct tags, and embedded fields. The first is read at run time from the type, like plain
  `[GoType]`. The other three are read from a field or a parameter, so they move the way the channel directions did,
  at the same kind of cost.
- **Four kinds must stay**, because the C# compiler or the .NET runtime reads them as attributes and nothing else can
  replace them. A fifth, `[MethodImpl(NoInlining)]`, was listed here at first; most of its uses can move after all, by a
  different route (section 10, added after a probe).

The recommendation is to cut `[GoRecv]` first (section 8).

## 2. How the channel-direction fix carries a fact without visible text

Go's `func(c <-chan int)` and `func(c chan int)` both become a C# parameter of type `channel<nint>`. The converter
already wrote the direction as a comment beside the type, for the reader:

```csharp
internal static void recvLocal(/*<-*/channel<nint> c) {
}
```

The fix, on `claude/c2-func-chan-dir`, leaves that line exactly as it was. A source generator in go2cs-gen reads the
`/*<-*/` comment at build time and writes a separate generated file holding the fact as an ordinary attribute, on a
part of the declaring type that nobody reads:

```csharp
// generated at build time, never committed
[global::go.GoSigChanDir("recvLocal", new global::System.Type[] { typeof(global::go.channel<nint>) },
    new global::go.GoChanDir[] { global::go.GoChanDir.Recv }, new global::go.GoChanDir[] {  })]
partial class main_package
{
}
```

At run time, reflection finds that record by method name and parameter types. If a record ever disagrees with the
method it names (wrong number of positions, or a direction on something that is not a channel), the reader stops with
an error naming the method instead of guessing.

Three facts about this route decide where it can be reused.

1. **A generated file can add an attribute to a type, but not to a method, field or parameter.** C# lets a `partial`
   type be declared in several files, and attributes on any part belong to the type. Methods, fields and parameters
   have no such second declaration. So a fact the runtime reads from a *type* can be re-emitted unchanged, while a fact
   it reads from a *member* has to become a type-level record that names the member, with the runtime reader rewritten
   to look there.
2. **The comment now carries information the program depends on.** If someone deletes it, or a tool drops it, the fact
   is gone. Whether anything notices depends on the kind (section 6).
3. **The build-time cost was not measurable.** The fix added one new pass of the generator over every method and type in
   every converted project. The whole standard library built in 350.4 s on average with it and 354.7 s without
   (three runs each, same machine; the runs spread from 340.5 s to 367.8 s). The difference is inside the spread.

## 3. The census

Every attribute the converter writes into converted code, counted by kind. "Production" is the converted standard
library's ordinary code files; "tests" is its converted `_test.go` files; "metadata" is the per-package bookkeeping
files (`package_info.cs`, `package_test_info.cs` and the test host), which a person reading the code normally skips;
"behavioral" is the golden output of the behavioral test programs (their `main.cs.target`, not their metadata).
Hand-written files (golib, the `*_impl.cs` companions, and whole files marked as hand conversions) are excluded. The
committed tree keeps a separate copy of some packages for each of windows, linux and darwin, so a kind that lives in
those packages is counted once per copy.

| Attribute | Production | Tests | Metadata | Behavioral | What it says | Who reads it |
|---|---:|---:|---:|---:|---|---|
| `[GoRecv]` | 5,107 | 675 | — | 342 | this method has a pointer receiver | generators (build), runtime method sets |
| `[GoType]` | 3,993 | 3,482 | — | 1,657 | this type came from Go (with an argument: its underlying type) | generators (build), runtime reflection |
| `[MethodImpl(...)]` | 207 | 932 | — | 229 | do not inline this method | the .NET JIT |
| `[GoArrayDims(N)]` | 149 | 64 | — | 33 | this array parameter or field has length N | runtime reflection |
| `[GoTag(...)]` | 74 | 472 | — | 17 | the Go struct tag of this field | runtime reflection (`reflect.StructTag`) |
| `[GoInit]` | 72 | 50 | 6,833 | 14 | run this method when the package loads | the C# compiler (it is .NET's `[ModuleInitializer]`) |
| `[GoEmbedded]` | 58 | 171 | — | 37 | this field is an embedded field | runtime reflection, one generator |
| `[GoStr]` | 29 | — | — | — | generate the string-argument twin of this function | a generator |
| `[FieldOffset]`, `[StructLayout]`, `[DllImport]` | 33 | 29 | — | 21 | memory layout and native calls | the .NET runtime |
| `[GoLocalName]`, `[GoValueClone]`, `[GoPackage]` | 7 | 308 | 1,549 | 42 | naming and copy bookkeeping for types | runtime reflection, generators |
| assembly-level records (`GoImplement`, `GoImplicitConv`, `GoTypeAlias`, `GoDynamicTypeLift`, …) | — | — | 4,809 | — | interface implementations, conversions, aliases | generators |
| everything else (`GoChanDir`, `GoMapKeyDims`, `GoDescriptorType`, `GoWrapper`) | 5 | 16 | — | 10 | rare cases | runtime reflection |

Two things stand out. In the code a person reads, two kinds are almost all of it: `[GoRecv]` and `[GoType]` together
are 9,100 of the 9,738 attributes in production files. And the large assembly-level records already live in the
metadata files, away from the code; this plan leaves them alone.

The census script and its raw output are not committed; the method is: strip comments and strings, find every
`[Name(`, `[Name]` or `[Name<` that opens an attribute section, keep only names that are real attribute classes, and
count per file population.

## 4. What must stay, and why

| Attribute | Why it cannot move |
|---|---|
| `[MethodImpl(MethodImplOptions.NoInlining)]` | The JIT reads it from the method's metadata, and a type-level record means nothing to the JIT. The converter writes it where Go code asks for its caller (`runtime.Caller`), so inlining would change the answer. **Amended:** a generator *can* attach it if the converted method is declared `partial`; section 10 measures that route. Only the uses on lambdas and local functions must stay. |
| `[GoInit]` | It is a `using` alias for .NET's `[ModuleInitializer]`, which the C# compiler itself acts on. A generator could write a wrapper initializer that calls the method, but the order in which initializers run would then depend on the generator rather than on the converter, and Go's package initialization order is part of correct behavior. Almost all of these are in metadata files anyway (6,833 of 6,955). |
| `[StructLayout]`, `[FieldOffset]`, `[DllImport]` | The .NET runtime lays out memory and binds native calls from them. They only appear where Go code describes a native structure or calls the operating system. |
| the assembly-level records | Not noise in the code a reader opens: they sit in `package_info.cs`. |

## 5. The candidates, one by one

### 5.1 `[GoRecv]` — remove it, no comment needed

What it says: this method was a Go method with a pointer receiver (`func (b *Reader) Read(...)`). The generator uses it
to add a second version of the method that takes a `ж<Reader>` (a pointer box), and the runtime uses it to decide which
methods belong to `T` and which to `*T`.

Why it is redundant in converted code: the converter writes the receiver of every pointer method as `this ref T`, and it
writes `[GoRecv]` on a method exactly when the signature starts with `this ref` (`src/go2cs/visitFuncDecl.go`, and the
promoted-method forwarders in `visitStructType.go`). Measured over the committed tree: 5,107 production and 675 test
methods take `this ref`, and every one carries `[GoRecv]`. In the hand-written files, 203 do and one does not: a private
helper, `userArenaKeep` in `runtime/arena_impl.cs`. Today the runtime would read that one as a value receiver passed by
reference to avoid a copy (golib calls this a copy-bound receiver). A cut must check whether any method set ever sees
it, and give it an explicit attribute if one does.

Before and after, `bufio.(*Reader).Read`:

```csharp
// before
[GoRecv] public static (nint n, error err) Read(this ref Reader b, slice<byte> p) {

// after
public static (nint n, error err) Read(this ref Reader b, slice<byte> p) {
```

What changes:

- the converter stops writing `[GoRecv]`;
- the generators that look for it in source (`RecvGenerator` and eight others) test for a `this ref` first parameter
  instead, and the one place that reads it from a compiled assembly (`MethodDeclarationSyntaxExtensions.cs`, used when
  one package's generator looks at another package's methods) tests for a by-reference receiver;
- the runtime's method-set code (`TypeExtensions.GoMethodSets.cs`, `AdapterBinder.cs`) reads "by-reference receiver means
  pointer receiver, unless the method says it is a copy-bound value receiver";
- the one hand-written exception gains that explicit attribute if the check finds it matters;
- for one transition period every reader accepts a `[GoRecv]` that is still present, so hand-written files and
  assemblies converted before the change keep working.

Risk: medium. Nothing is lost if a formatter touches the file, because there is no comment. The risk is in the
readers: method sets drive interface satisfaction, so a mistake shows up as a wrong answer from `reflect` or a failed
interface assertion, not as a build error. The existing reflection canaries exercise exactly this.

### 5.2 Plain `[GoType]` — remove it, with one rule for the exceptions

What it says, without an argument: this type came from Go, so the generators add the members converted code relies on,
and reflection treats it as a defined Go type.

How close it is to redundant: in converted production files, every type declared inside the package class carries it
(3,017 plain, 815 with an argument), except 16; in converted test files, all but 12. Those 28 need reading one by one
before a rule is chosen (for example `unicode.d`, `runtime.semTable`, and benchmark helper types in `slices`): whether
each is a Go type the converter left unmarked, or a type that must stay unmarked, decides the rule.

Before and after, `bufio.Scanner`:

```csharp
// before
[GoType] partial struct Scanner {

// after
partial struct Scanner {
```

Unlike `[GoRecv]`, this one is read at run time from the *type*, so the generator can put the attribute back on the
part of the type it already generates. Reflection then sees exactly what it sees today.

What changes:

- the converter stops writing plain `[GoType]`;
- the generators' shared test for "is this a Go type" (`Common.cs`) becomes "a type declared inside a `*_package` class
  in a converted file", with hand-written files keeping the attribute as their opt-in;
- the type generator adds `[GoType]` to its own generated part, skipping types that already carry it so the attribute
  is never declared twice.

Risk: medium-low, but the rule for "converted file" must be exact. A hand-written helper type that the rule wrongly
treats as a Go type would get generated members it does not expect. That usually fails the build loudly, but not
always.

### 5.3 `[GoType("…")]` with an argument — move it into a short comment

What it says: this defined type's underlying type, for example `num:int64` for `time.Duration`. The generator chooses what
to build from that exact text, and reflection reads it.

Before and after, `time.Duration`:

```csharp
// before
[GoType("num:int64")] partial struct Duration;

// after
partial struct Duration /*num:int64*/;
```

The comment holds the same text the attribute holds, because the generators decide what to build from that text.
Rewriting it into Go's own spelling (`/*int64*/`) is possible later, but it means teaching every generator to read a
second spelling, so it is a separate step. The type generator re-emits the attribute on its generated part, as in 5.2.

Risk: medium. A deleted comment turns a defined number type into a plain struct. That is expected to fail the build in
almost every case, since the type's arithmetic and conversions would disappear; a cut should confirm it with a test.

### 5.4 `[GoArrayDims(N)]` — move it into a Go-looking comment

What it says: this parameter is an array of length N. C# has no fixed-length array type, so the length is otherwise lost,
and reflection needs it (`reflect.New` of the parameter's type would make a zero-length array without it).

Before and after, `time.Duration.format`, from Go's `func (d Duration) format(buf *[32]byte) int`:

```csharp
// before
internal static nint format(this Duration d, [GoArrayDims(32)] ж<array<byte>> Ꮡbuf) {

// after
internal static nint format(this Duration d, /*[32]*/ж<array<byte>> Ꮡbuf) {
```

This one reads most like Go, but it is the most work. The fact is read from a parameter, so it must become a type-level
record naming the method and the parameter, and the eight runtime files that use it today would read the record instead. A
field's array length is usually already in its initializer (`array<any> Params = new(1);`), so fields need it less often.

Risk: high for its size (149 production uses). A deleted comment silently gives a zero-length array. The guard is that
the converter writes the length on every array parameter, so a check in the build can reject an `array<T>` parameter in
a converted file that has no length comment.

### 5.5 `[GoTag(...)]` — move it into a comment shaped like Go's tag

What it says: the Go struct tag of a field, which `encoding/json`, `encoding/xml` and others read through reflection.

Before and after, `net/rpc/jsonrpc.clientRequest`, from Go's ``Method string `json:"method"` ``:

```csharp
// before
[GoTag(@"json:""method""")]
public @string Method;

// after
public @string Method; /*`json:"method"`*/
```

The tag moves from the line above the field to the end of the field's own line, in Go's backquote spelling, and the
doubled quotes of the C# string go away. Like 5.4 it is read from a field, so it needs a type-level record and a
change to the runtime readers. Most uses are in tests (472 of 563), so the production gain is modest.

Risk: medium, and one detail: a tag containing `*/` would end the comment early, so the converter must escape it, and the
generator must reverse the escape. A deleted tag comment is silent (most tags are optional), so the reader cannot refuse
a missing one; the generator tests and the behavioral tests are the guard.

### 5.6 `[GoEmbedded]` — move it into a comment

What it says: this field is a Go embedded field, whose methods are promoted to the outer struct.

Before and after, `io.nopCloser`:

```csharp
// before
[GoEmbedded] public Reader Reader;

// after
/*embed*/ public Reader Reader;
```

It cannot be inferred from the name alone, because a Go field may be named the same as its type without being embedded.
It is read from a field, so it needs a record like 5.4. Only 58 production uses. If the comment is deleted, reflection
reports the field as not embedded (`StructField.Anonymous` reads false); what the one generator that reads it does
without it must be checked before a cut.

### 5.7 `[GoStr]` — remove it; the signature already says it (RULED: it moves, 2026-10-06)

The owner ruled that it moves too, though it has only 29 uses, so the campaign does every kind the same way. It needs
no marker in its place: the converter's rule for writing it is visible in the signature. `[GoStr]` marks the member
of an sstring twin that carries the Go body, and the converter types each twinned parameter `sstring`
(`sstringTwinSignature`). Nothing else in converted code takes an `sstring` parameter: at master `7098b8d3f9`, 29
converted methods have one and all 29 carry `[GoStr]`; no hand-owned file (the `*_impl.cs` companions and the manual
conversions) declares one. The only reader is `StrGenerator`, which would select a method by "a parameter of type
`sstring`" instead of by the attribute, and still accept the attribute on a hand-written member.

Before and after, from `src/core/unicode/utf8/utf8.cs` (Go: `func DecodeRuneInString(s string) (r rune, size int)`):

```csharp
// before
[GoStr] public static (rune r, nint size) DecodeRuneInString(sstring s) {

// after
public static (rune r, nint size) DecodeRuneInString(sstring s) {
```

## 6. Keeping a comment safe once it carries a fact

| Kind | If the comment or attribute is deleted | Guard |
|---|---|---|
| `[GoRecv]` removed | nothing to delete: the fact is the `this ref` signature | runtime and generator tests over both receiver forms |
| plain `[GoType]` removed | nothing to delete: the fact is where the type is declared | generator tests over converted and hand-written files; the build fails if a hand-written type is misread in most cases |
| `/*num:int64*/` | the build fails almost always (the type loses its arithmetic) | generator tests built from the converter's own output |
| `/*[32]*/` | silent: a zero-length array at run time | a build check that every `array<T>` parameter in a converted file has its length comment |
| ``/*`json:"method"`*/`` | silent: the tag is gone | generator tests and the behavioral tests; the reader refuses a record that names a field that is not there |
| `/*embed*/` | silent in reflection (`StructField.Anonymous` reads false) | generator tests; the reader refuses a record naming a missing field |

General rules, the same as the channel-direction fix uses today:

- **Tests are built from the converter's own output.** Each generator test reads a committed behavioral golden, so if
  the converter ever changes how it spells a comment, the test fails rather than passing on a hand-written copy of the
  old spelling.
- **A record that contradicts its member is refused by name.** The runtime never applies a fact to a member that cannot
  hold it.
- **Hand-written files keep the attributes.** Every generator accepts the attribute or the comment. A hand-written file
  never has to adopt a comment it does not want.
- **Formatters.** `dotnet format` and the IDE keep comments in place; a refactoring that rewrites a parameter list can
  drop one. Converted files are regenerated from Go, not edited, so this matters mostly for hand edits, which is why
  the silent kinds need the build check.
- **Packages consumed from NuGet.** The generators ran when the package was packed, and the attributes they produced are
  compiled into the assembly. A consumer's runtime reads the compiled attributes, so nothing changes for 5.2 and 5.3.
  For 5.1 and 5.4–5.6 the compiled shape changes, so the readers accept both the old and the new shape during the
  transition, and a consumer must not mix go2cs packages built before and after the change with a newer runtime
  library. The packages are already released together as one version, so this holds today.

## 7. The cost of reading a comment instead of an attribute

All of go2cs-gen's generators already walk every declaration in every converted project and test attribute names as
text; none uses Roslyn's newer attribute-indexed lookup, which skips files that do not contain a given attribute. So
testing a comment or a signature instead of an attribute name is the same kind of work on the same nodes. The
channel-direction fix measured the extra pass as within the build's normal spread (section 2).

If the generators are ever rewritten as incremental generators, that changes: Roslyn can find attribute-marked
declarations through an index, while comment markers would still need every declaration visited. Removing an attribute
in favor of the signature (5.1, 5.2) has no such cost, since the generator still looks at the declaration it is about
to process.

## 8. Ranking, and the one to cut first

| Rank | Kind | Lines saved in production code (estimate) | Risk | Work |
|---|---|---:|---|---|
| 1 | `[GoRecv]` removed | 5,107 method lines lose a 9-character prefix | medium: method sets | converter, 9 generator files, the runtime method-set readers |
| 2 | plain `[GoType]` removed | 3,017 type lines | medium-low: the "converted file" rule | converter, shared generator test, type generator |
| 3 | `[GoType("…")]` to a comment | 815 type lines | medium | converter, the generators that read the argument |
| 4 | `[GoTag(...)]` to a comment | 74 (472 more in tests) | medium, silent if deleted | converter, a record, runtime readers |
| 5 | `[GoArrayDims(N)]` to a comment | 149 | high for its size, silent if deleted | converter, a record, 8 runtime files, a build check |
| 6 | `[GoEmbedded]` to a comment | 58 | low-medium | converter, a record, runtime readers |

**Cut `[GoRecv]` first.** It is the largest single kind, and it needs no comment, so nothing can be stripped. The
information is already visible in every line it decorates, so the reader loses nothing. What it changes, the method
sets, is what the reflection canaries and the behavioral suite already exercise. Its corpus footprint is easy to
predict: one prefix removed per method, on 5,107 production lines, 675 converted test lines, and 342 behavioral golden
lines (plus their committed outputs). The exact count belongs to the cut's own measurement.

Plain `[GoType]` should follow once the 28 exceptions have been read. The comment kinds (3–6) are worth doing only after
those two land and the owner has seen the result, since each one adds a comment that the program then depends on,
which the code did not have before.

## 9. What a cut would owe

The same gates the channel-direction fix ran:

- a failing test first;
- generator tests built from the converter's own output;
- the runtime reader tests;
- the full behavioral suite;
- the converter suite;
- the corpus re-conversion on all three platforms, with the footprint predicted beforehand;
- a whole-standard-library build, with the build time compared before and after on one machine;
- the largest reflection-using validated packages, as canaries.

For `[GoRecv]` specifically, the method-set code is on the path of every interface assertion. Its canaries are the five
largest validated packages that import `reflect`, chosen fresh from the roster when the cut runs.

## 10. Addendum (2026-10-06): `[MethodImpl(NoInlining)]` through a partial method

> **RULED 2026-10-06 (owner: "partial ok").** The converter writes a no-inline METHOD as a partial method's implementing
> part and go2cs-gen writes the declaring part carrying the attribute; lambdas, local functions and shapes that cannot be
> partial keep the prefix; one rendering for the standard library, modules and the behavioral tests. The deciding gate:
> the set of methods whose metadata carries the no-inline flag is identical before and after. The carrier is cut on
> `claude/c2-noinline-partial`.

Section 4 first listed `[MethodImpl(MethodImplOptions.NoInlining)]` as impossible to move, because a generated file
cannot attach an attribute to a method declared somewhere else. C# has one exception: a **partial method** is declared in
two parts, and the attributes of both parts belong to the one compiled method. If the converter wrote such a method as
`partial`, keeping its body, go2cs-gen could write the other part, carrying the attribute. The visible price is the word
`partial` where the 43-character prefix stands today.

Before and after, `log.(*Logger).Print`, which Go code reaches through `runtime.Caller`:

```csharp
// before (converted code)
[MethodImpl(MethodImplOptions.NoInlining)] public static void Print(this ж<Logger> Ꮡl, params ꓸꓸꓸany vʗp) {

// after (converted code)
public static partial void Print(this ж<Logger> Ꮡl, params ꓸꓸꓸany vʗp) {

// generated at build time, never committed
[MethodImpl(MethodImplOptions.NoInlining)] public static partial void Print(this ж<Logger> Ꮡl, params ꓸꓸꓸany vʗp);
```

**The probe.** A small program put each shape converted methods take into this form: a static method returning a named
tuple, an extension method with a `this ref` receiver, one with a value receiver, a generic method with a constraint, a
method with `ref` and `params` parameters, one with a default value, and one taking a pointer under `unsafe`. Each
declaring part carried the attribute; each implementing part held the body. A plain method with no attribute was the
control. With warnings treated as errors, it compiled clean. Read back at run time:

| Shape | `NoInlining` in the compiled method | Kept as its own frame by the JIT |
|---|---|---|
| static, named-tuple result | yes | yes |
| extension, `this ref` receiver | yes | yes |
| extension, value receiver | yes | yes |
| generic with a constraint | yes | yes |
| `ref` and `params` parameters | yes | yes |
| a default value | yes | yes |
| a pointer, `unsafe` | yes | yes |
| control (no attribute) | no | no: inlined into its caller |

"Kept as its own frame" was read by having each method call a helper that names its caller, with tiered compilation
off so the JIT optimizes, and inlines, from the first call. The control's row is what shows the test can tell the two
apart.

**What cannot take this form.** A lambda or a local function cannot be a partial method, and the converter also writes
the attribute on func literals that reach `runtime.Caller`. In converted production code, 189 of the 207 uses are on
methods and could move; 16 are on lambdas and 2 on local functions, and those stay. In converted tests the split is
734 methods, 191 lambdas, 7 local functions.

**The rules the generated part must follow.** C# requires the two parts to agree, so the generator copies the
implementing part's signature exactly: return type including tuple element names, parameter names (a mismatch is the
warning CS8826, the same one the darwin build shows today in a hand-written file), `this`, `ref` and `params`, generic
constraints, and `unsafe`. One thing moves rather than copies: a default parameter value belongs on the declaring
part; C# ignores it on the implementing part and warns (CS1066). Go functions have no default values, but a cut should
count whether the converter ever emits one on these methods.

**How the generator knows which methods want it.** A method written `partial`, with a body, and with no other
declaration of it in the compilation. Converted code already uses partial methods the other way round: the converter
writes a declaration *without* a body for an assembly or cgo function, and a hand-written `*_impl.cs` file supplies the
body. Those always have their declaring part already, so the two uses never collide. Hand-written files that want the
attribute keep writing it.

One generator has to change with it: the one that writes the `ж<T>` overload of a pointer-receiver method reads the
`NoInlining` attribute from the source today, so its forwarder is kept as a frame too. It cannot see another
generator's output, so it would test for the same "`partial` with a body and no declaration" shape instead.

**What happens when something goes wrong.** If the generator does not run, C# rejects an implementing part that has no
declaring part (error CS0759), so the build fails loudly; the attribute cannot be lost silently. For packages consumed
from NuGet, the merged attribute is compiled into the assembly, so nothing changes for consumers.

**Where it ranks.** Each line it touches gets 35 characters shorter (a 43-character prefix becomes the 8-character
word `partial`), against 9 for `[GoRecv]`, but on far fewer lines (189 in production, against 5,107). It touches only
the converter and two generators, not the runtime. Its value is highest exactly
where the attribute is about to multiply: G's remedy for a logging library's caller frames adds 103 of these prefixes to
one module's converted code. If the owner chooses this route, those 103 would read `partial` instead.

— C2

**RULED EXCEPTION, 2026-10-06: an `init` is never carried by `partial`.** G's stacked record found a marked Go `init`
running out of order. C# runs module initializers in declaration order, and a partial method's declaration is its
declaring part, which the generator writes into a file that sorts after every source file, so a `partial` init ran
after the package's other inits (Go: a.go, main.go 1, main.go 2, z.go; the carrier: a.go, main.go 1, z.go, main.go 2).
An init that takes the mark keeps `[MethodImpl(MethodImplOptions.NoInlining)] [GoInit]`, in the same fallback set as
a lambda and a local function, for a different reason: its position is its meaning. The generator refuses a
hand-written partial init by name (GO2CS0003, an error). Neither the corpus nor the behavioral goldens held a marked
init, so the measured footprint does not change. Seated on `claude/c2-noinline-partial` (`c02f80e935`).

## 11. Every other inline attribute kind (RULED 2026-10-06 by the owner's rule)

Sections 4 and 5 rule the large kinds. This table rules every other attribute the converter writes inline in
converted code, counted at master `7098b8d3f9` over the same populations as section 3 (applications anywhere in a
file, parameter lists included; hand-written files excluded). The owner's rule decides each row, not its count.

| Attribute | Production | Tests | Metadata | Behavioral | Sits on | Ruling |
|---|---:|---:|---:|---:|---|---|
| `[GoLocalName]` | 7 | 269 | 249 | 42 | a lifted function-local type (struct or descriptor interface) | **MOVES**, into the package's `TypeAccessibility` record in `package_info.cs` (or `package_test_info.cs`), which already carries it for production structs. Reflection reads it from the TYPE, through every partial declaration of it. |
| `[GoValueClone]` | 0 | 41 | 574 | 0 | a struct | **MOVES**, the same record. Production already moved; the 41 left are in converted test files (why the record missed them is for the cut to find). |
| `[GoChanDir]` | 0 | 6 | 0 | 2 | a defined channel type's wrapper struct | **MOVES with C (5.3).** It exists because `[GoType("chan …")]` drops the direction; the comment that replaces `[GoType("…")]` can spell the type as Go does (`chan<- int`), so the generator that re-emits `[GoType("…")]` on its generated part can emit this one beside it. |
| `[GoMapKeyDims]` | 2 | 1 | 0 | 2 | a field or parameter whose map key is an array | **MOVES with D (5.4)**: it is `[GoArrayDims]`'s twin for `Key()`, and rides the same member record. |
| `[GoDescriptorType]` | 3 | 1 | 0 | 3 | a field or parameter whose interface type a descriptor stands for | **MOVES with D**: its own documentation calls it the `[GoArrayDims]` pattern for a different lost datum, at the same positions. |
| `[StackTraceHidden]` | 51 | 10 | 0 | 1 | a `//go:linkname` or trampoline forwarder method | **MOVES, through section 10's carrier.** The .NET runtime and `runtime.Callers` read it from the method, and a generated declaring part can carry it as it carries `NoInlining`. The forwarder needs a second signal, since `partial` alone means no-inline: the design (the shortest marker the generator can read) is cut with S or after it. |
| `[GoWrapper]` | 0 | 8 | 0 | 3 | a lambda (a method expression's wrapper) | **STAYS.** The runtime reads it from `Delegate.Method`, the lambda's own generated method; a lambda has no partial form and no declaration a record can name. |
| `[GoPackage]` | 0 | 0 | 736 | 0 | the package class, in `package_info.cs` | **STAYS where it is**: already in the metadata file, not in code a reader opens. |
| `[GoTestMatchingConsoleOutput]` | 0 | 0 | — | — | a behavioral program's package class, in its `package_info.cs` | **STAYS**: metadata, read by the behavioral test harness. |
| assembly-level records (`GoPositionMap`, `GoImplement`, `GoDynamicTypeLift`, `GoImplicitConv`, `GoTypeAlias`, `GoCgoImportDynamic`, `GoSStringTwin`, `GoRefPrimary`) | 0 | 0 | 8,126 | 0 | the assembly, in metadata files | **STAY** (section 3): not in the code a reader opens. |

`[GoInit]`, `[StructLayout]`, `[FieldOffset]` and `[DllImport]` stay (section 4); `[MethodImpl]` moves for methods and
stays for lambdas, local functions, bodyless declarations and inits (section 10). One `[ModuleInitializer]` in
`src/core/go2cs.CpuProfiler` is in a hand-written project, not converted code.

— C2
