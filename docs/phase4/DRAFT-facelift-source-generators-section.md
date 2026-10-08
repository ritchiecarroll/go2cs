# DRAFT: the marker-comment section for `source-generators.md` (face-lift docs seat)

> **A draft, not a page.** It is the section the docs seat adds to
> `docs/ConversionStrategies-Reference/source-generators.md` when the face-lift chain's emission is final. It is
> written from the ruled plan (`docs/PLAN-marker-comment-parity.md`) and from the seat branches as they stood on
> 2026-10-07: `claude/c2-facelift-a-recv-q` `541bc5c22d` (A), `claude/g-plain-gotype-removed` `f464628bf5` (B),
> `claude/g-gotype-arg-comment` `6efdecafd1` (C) and `claude/c2-facelift-record-q` `5665b1d96f` (F, E, D, D2, S).
>
> **Where each sample comes from.** The marker lines are copied from the converter's own committed output on the
> record branch, `src/go2cs/testdata/markercomments/main.cs` at `5665b1d96f`, and each sample names its lines.
> The three samples that branch cannot show yet (A, B and C change no converter output until the chain's
> re-conversion) are marked OWED and are regenerated with every other sample.
>
> **Open before it moves into the page** (each is checked at the ref COORD names):
> 1. Every sample regenerated from that ref's emission; the OWED ones first.
> 2. The "what stays" table re-counted: the section 11 kinds that have moved by then leave it.
> 3. The page's present "Extended attributes" table rules `[GoTag]`, `[GoRecv]`, `[GoStr]` and `[GoArrayDims]`
>    as "must stay". This section replaces those four rows; the rest of that table stands.
> 4. The names of the generated records (`GoMemberRecord`, `GoParamDims`, `GoCopyBound`) confirmed at that ref.

---

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
| `struct { Reader }` | `/*embed*/ public Reader Reader;` | the comment before the field |
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

<!-- OWED: bufio.(*Reader).Read from src/core/bufio at the final emission -->
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

<!-- OWED: time.Duration from src/core/time at the final emission -->
```csharp
partial struct Duration /*num:int64*/;
```

**An embedded field says `/*embed*/`.** Go embeds a field by leaving out its name; C# needs the name, and a
Go field may share its type's name without being embedded, so the name alone cannot say it. The comment does.
Promotion of the embedded type's methods, and `reflect`'s `Anonymous`, follow from it.

<!-- source: src/go2cs/testdata/markercomments/main.cs:41-42 at claude/c2-facelift-record-q 5665b1d96f -->
```csharp
    /*embed*/ public Reader Reader;
    /*embed*/ internal nint @int;
```

**A struct tag keeps Go's spelling, at the end of the field's line.** The tag sits where Go puts it and reads
as Go wrote it, backquotes included. `encoding/json` and every other package that reads tags through `reflect`
receives the same string. A tag that a comment cannot hold as written (it contains a backquote, a `*/` or a
control character) uses Go's quoted spelling instead. A grouped declaration carries the tag once, as Go does.

<!-- source: src/go2cs/testdata/markercomments/main.cs:46-47,50 at claude/c2-facelift-record-q 5665b1d96f -->
```csharp
    public @string Plain; /*`json:"plain"`*/
    public nint Grouped, Pair; /*`json:"g"`*/
    public @string Quoted; /*"a:\"`b`\""*/
```

**An array length is Go's own prefix, before the type.** C# has no fixed-length array type, so `[32]byte`
becomes `array<byte>` and the length would be lost. The comment `/*[32]*/` keeps it, written as Go's array
prefix, on a parameter, on a field, and on a defined type whose inner lengths its definition does not spell.
Nested arrays list each length in order.

<!-- source: src/go2cs/testdata/markercomments/main.cs:68,77,83 at claude/c2-facelift-record-q 5665b1d96f -->
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

<!-- OWED: unicode/utf8.DecodeRuneInString from src/core/unicode/utf8 at the final emission -->
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

Everything that can move out of converted code has. What remains is there because the C# compiler or the .NET
runtime reads it as an attribute from that exact declaration, or because the declaration has no name a
generated record could use.

| Still an attribute | Where | Why it cannot move |
|---|---|---|
| `[GoInit]` | a package's `init` functions | It is .NET's module-initializer attribute under a Go name. The C# compiler acts on it, and initializers run in declaration order, which is Go's package initialization order. |
| `[StructLayout]`, `[FieldOffset]`, `[DllImport]` | a struct or function that describes native memory or calls the operating system | The .NET runtime lays out memory and binds native calls from them. |
| `[MethodImpl(MethodImplOptions.NoInlining)]` | a func literal, a local function, or an `init` | The JIT reads it from the method itself. A declared method carries the same mark as the word `partial` (see [The no-inline mark rides a generated declaring part](#the-no-inline-mark-rides-a-generated-declaring-part)); a lambda or a local function cannot be partial, and an `init` keeps its place in the file. |
| `[GoArrayDims(N)]` | an array parameter of a func literal or a local function | The compiler names those methods, so no record can refer to them. |
| `[GoWrapper]` | the lambda that wraps a method expression | The runtime reads it from the lambda's own method, which has no declaration to attach a record to. |

A package's bookkeeping attributes (`[GoPackage]`, `[GoImplement]`, `[GoImplicitConv]`, `[GoTypeAlias]` and
the rest) are in `package_info.cs`, not in the code a reader opens.

### Hand-written code

A hand-written file (a `*_impl.cs` companion, or a whole file marked as a manual conversion) keeps the
attributes: `[GoRecv]`, `[GoType]`, `[GoType("…")]`, `[GoEmbedded]`, `[GoTag]`, `[GoArrayDims]` and `[GoStr]`
all mean what they always have, and every generator and the runtime accept either spelling. Nobody writing C#
by hand has to adopt a comment. In a package written entirely by hand (`testing` and `unsafe`), marked
`[assembly: GoHandOwnedPackage]`, the attributes are the only spelling read, so nothing is inferred from a
signature there.

<!-- source: src/core/internal/poll/fd_mutex_impl.cs:150 at master 541766413e -->
<!-- attribute-shown: a hand-written file keeps the attribute -->
```csharp
[GoRecv] internal static bool rwlock(this ref fdMutex mu, bool read) {
```
