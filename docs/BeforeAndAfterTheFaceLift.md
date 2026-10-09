# Converted code, before and after the face lift

go2cs converts Go into C# that a Go developer can read. Until this release the converted code carried a
set of attributes that told the build what Go's own syntax already says: that a method has a pointer
receiver, that a type came from Go, that a field is embedded. They were accurate, and they were noise.
The face lift removes them from converted code.

Nothing is lost. Each fact now sits where a Go reader already looks: in the signature, or in a short
comment written the way Go writes it. The go2cs source generators read the signature or the comment at
build time and write the attribute into generated code, where the runtime finds it. The converted file is
left with the code.

Every example on this page is a real line of the converted Go standard library. "Before" is the file as
it stood before the face lift, "after" is the same file in this release, and the Go line is the one both
were converted from. The marks that remain, and why, are listed in
[Marker comments: what converted code carries instead of an attribute](ConversionStrategies-Reference/source-generators.md#marker-comments-what-converted-code-carries-instead-of-an-attribute).

## A pointer receiver is just `this ref`

`strings.Builder.Len` has a pointer receiver. The C# receiver was already `this ref Builder b`, and that `ref` is the whole fact; the attribute repeated it on more than five thousand methods of the standard library.

Go, `strings/builder.go`:

<!-- source: Go toolchain src/strings/builder.go:45 (Go 1.24.13; the Go source of src/core/strings/builder.cs, which the repository does not hold) -->
```go
func (b *Builder) Len() int { return len(b.buf) }
```

Before, `src/core/strings/builder.cs`:

<!-- before the face lift: git show 541766413e:src/core/strings/builder.cs, lines 48 -->
<!-- attribute-shown: the converted code as it was before the face lift -->
```csharp
[GoRecv] public static nint Len(this ref Builder b) {
```

After, the same file:

<!-- source: src/core/strings/builder.cs:48 -->
```csharp
public static nint Len(this ref Builder b) {
```

From the `this ref` receiver a source generator writes the overload that takes a pointer, as it did from the attribute.

## A Go type needs no mark

`container/list.List` is a struct. Every struct and interface the converter declares in a package is a Go type, so the declaration says nothing more than Go's does.

Go, `container/list/list.go`:

<!-- source: Go toolchain src/container/list/list.go:48-51 (Go 1.24.13; the Go source of src/core/container/list/list.cs, which the repository does not hold) -->
```go
type List struct {
	root Element // sentinel list element, only &root, root.prev, and root.next are used
	len  int     // current list length excluding (this) sentinel element
}
```

Before, `src/core/container/list/list.cs`:

<!-- before the face lift: git show 541766413e:src/core/container/list/list.cs, lines 52-55 -->
<!-- attribute-shown: the converted code as it was before the face lift -->
```csharp
[GoType] partial struct List {
    internal Element root; // sentinel list element, only &root, root.prev, and root.next are used
    internal nint len;    // current list length excluding (this) sentinel element
}
```

After, the same file:

<!-- source: src/core/container/list/list.cs:52-55 -->
```csharp
partial struct List {
    internal Element root; // sentinel list element, only &root, root.prev, and root.next are used
    internal nint len;    // current list length excluding (this) sentinel element
}
```

The generator completes the `partial struct` (its constructors, equality and `ToString`) exactly as before.

## A defined type states its underlying type after its name

`time.Duration` is defined over `int64`. C# has no such declaration, so the converter writes an empty struct and says what it wraps. That used to be an attribute's argument in front of the declaration; it is now a comment behind the name, where Go puts the underlying type.

Go, `time/time.go`:

<!-- source: Go toolchain src/time/time.go:911 (Go 1.24.13; the Go source of src/core/time/time.cs, which the repository does not hold) -->
```go
type Duration int64
```

Before, `src/core/time/time.cs`:

<!-- before the face lift: git show 541766413e:src/core/time/time.cs, lines 910 -->
<!-- attribute-shown: the converted code as it was before the face lift -->
```csharp
[GoType("num:int64")] partial struct Duration;
```

After, the same file:

<!-- source: src/core/time/time.cs:910 -->
```csharp
partial struct Duration /*num:int64*/;
```

The generator builds the struct's value, operators and conversions from that comment.

## A slice type reads the same way

`sort.IntSlice` is defined over `[]int`.

Go, `sort/sort.go`:

<!-- source: Go toolchain src/sort/sort.go:121 (Go 1.24.13; the Go source of src/core/sort/sort.cs, which the repository does not hold) -->
```go
type IntSlice []int
```

Before, `src/core/sort/sort.cs`:

<!-- before the face lift: git show 541766413e:src/core/sort/sort.cs, lines 113 -->
<!-- attribute-shown: the converted code as it was before the face lift -->
```csharp
[GoType("[]nint")] partial struct IntSlice;
```

After, the same file:

<!-- source: src/core/sort/sort.cs:113 -->
```csharp
partial struct IntSlice /*[]nint*/;
```

`nint` is Go's `int`. The text between the comment marks is the type the wrapper holds.

## An embedded field says `/*embed*/`

`sort.reverse` embeds `Interface`. Go embeds a field by leaving out its name; C# needs the name, so something has to say the field is embedded.

Go, `sort/sort.go`:

<!-- source: Go toolchain src/sort/sort.go:88-92 (Go 1.24.13; the Go source of src/core/sort/sort.cs, which the repository does not hold) -->
```go
type reverse struct {
	// This embedded Interface permits Reverse to use the methods of
	// another Interface implementation.
	Interface
}
```

Before, `src/core/sort/sort.cs`:

<!-- before the face lift: git show 541766413e:src/core/sort/sort.cs, lines 83-87 -->
<!-- attribute-shown: the converted code as it was before the face lift -->
```csharp
[GoType] partial struct reverse {
    // This embedded Interface permits Reverse to use the methods of
    // another Interface implementation.
    [GoEmbedded] public Interface Interface;
}
```

After, the same file:

<!-- source: src/core/sort/sort.cs:83-87 -->
```csharp
partial struct reverse {
    // This embedded Interface permits Reverse to use the methods of
    // another Interface implementation.
    /*embed*/ public Interface Interface;
}
```

Method promotion, and `reflect`'s `Anonymous`, follow from the comment.

## A struct tag keeps Go's spelling

`net/rpc/jsonrpc`'s `clientRequest` carries JSON tags. Each tag sat on its own line above the field, as a C# verbatim string with doubled quotes. It now follows the field, backquotes and all, where Go puts it.

Go, `net/rpc/jsonrpc/client.go`:

<!-- source: Go toolchain src/net/rpc/jsonrpc/client.go:46-50 (Go 1.24.13; the Go source of src/core/net/rpc/jsonrpc/client.cs, which the repository does not hold) -->
```go
type clientRequest struct {
	Method string `json:"method"`
	Params [1]any `json:"params"`
	Id     uint64 `json:"id"`
}
```

Before, `src/core/net/rpc/jsonrpc/client.cs`:

<!-- before the face lift: git show 541766413e:src/core/net/rpc/jsonrpc/client.cs, lines 46-53 -->
<!-- attribute-shown: the converted code as it was before the face lift -->
```csharp
[GoType] partial struct clientRequest {
    [GoTag(@"json:""method""")]
    public @string Method;
    [GoTag(@"json:""params""")]
    public array<any> Params = new(1);
    [GoTag(@"json:""id""")]
    public uint64 Id;
}
```

After, the same file:

<!-- source: src/core/net/rpc/jsonrpc/client.cs:46-50 -->
```csharp
partial struct clientRequest {
    public @string Method; /*`json:"method"`*/
    public array<any> Params = new(1); /*`json:"params"`*/
    public uint64 Id; /*`json:"id"`*/
}
```

The struct is three lines shorter and reads line for line as the Go does. `encoding/json` receives the same tag through `reflect`.

## An array length is Go's own prefix

`time.Duration.format` takes a pointer to a 32-byte array. C#'s `array<byte>` does not carry a length in its type, so the length has to be written somewhere.

Go, `time/time.go`:

<!-- source: Go toolchain src/time/time.go:953 (Go 1.24.13; the Go source of src/core/time/time.cs, which the repository does not hold) -->
```go
func (d Duration) format(buf *[32]byte) int {
```

Before, `src/core/time/time.cs`:

<!-- before the face lift: git show 541766413e:src/core/time/time.cs, lines 953 -->
<!-- attribute-shown: the converted code as it was before the face lift -->
```csharp
internal static nint format(this Duration d, [GoArrayDims(32)] ж<array<byte>> Ꮡbuf) {
```

After, the same file:

<!-- source: src/core/time/time.cs:953 -->
```csharp
internal static nint format(this Duration d, /*[32]*/ ж<array<byte>> Ꮡbuf) {
```

`ж<array<byte>>` is a pointer to the array, and `/*[32]*/` is the `[32]` of Go's `*[32]byte`.

## A string twin is the function that takes an `sstring`

`utf8.DecodeRuneInString` is one of the functions go2cs emits twice, so that a string literal passed to it is viewed and not copied. The attribute marked the member that carries the Go body; its `sstring` parameter already says which one that is.

Go, `unicode/utf8/utf8.go`:

<!-- source: Go toolchain src/unicode/utf8/utf8.go:205 (Go 1.24.13; the Go source of src/core/unicode/utf8/utf8.cs, which the repository does not hold) -->
```go
func DecodeRuneInString(s string) (r rune, size int) {
```

Before, `src/core/unicode/utf8/utf8.cs`:

<!-- before the face lift: git show 541766413e:src/core/unicode/utf8/utf8.cs, lines 213 -->
<!-- attribute-shown: the converted code as it was before the face lift -->
```csharp
[GoStr] public static (rune r, nint size) DecodeRuneInString(sstring s) {
```

After, the same file:

<!-- source: src/core/unicode/utf8/utf8.cs:213 -->
```csharp
public static (rune r, nint size) DecodeRuneInString(sstring s) {
```

A generator writes the `@string` overload from it.

## A linkname forwarder says `/*linkname*/`

`time/tzdata` declares a function whose body lives in package `time`, by `//go:linkname`. The converter writes a forwarder. It used to carry a fully qualified .NET attribute to stay out of stack traces; it now carries one word, and the generator writes the attribute on the method's generated half.

Go, `time/tzdata/tzdata.go`:

<!-- source: Go toolchain src/time/tzdata/tzdata.go:31-32 (Go 1.24.13; the Go source of src/core/time/tzdata/tzdata.cs, which the repository does not hold) -->
```go
//go:linkname registerLoadFromEmbeddedTZData time.registerLoadFromEmbeddedTZData
func registerLoadFromEmbeddedTZData(func(string) (string, error))
```

Before, `src/core/time/tzdata/tzdata.cs`:

<!-- before the face lift: git show 541766413e:src/core/time/tzdata/tzdata.cs, lines 30-33 -->
```csharp
//go:linkname registerLoadFromEmbeddedTZData time.registerLoadFromEmbeddedTZData
[global::System.Diagnostics.StackTraceHidden] internal static void registerLoadFromEmbeddedTZData(Func<@string, (@string, error)> _) {
    go.time_package.registerLoadFromEmbeddedTZData(_);
}
```

After, the same file:

<!-- source: src/core/time/tzdata/tzdata.cs:30-33 -->
```csharp
//go:linkname registerLoadFromEmbeddedTZData time.registerLoadFromEmbeddedTZData
/*linkname*/ internal static partial void registerLoadFromEmbeddedTZData(Func<@string, (@string, error)> _) {
    go.time_package.registerLoadFromEmbeddedTZData(_);
}
```

Go's own `//go:linkname` comment was carried into the C# before and still is.

## What did not change

- **What the code does.** The face lift changes how converted code is written, not what it compiles to.
  The standard library's own test suites validate at the same counts before and after.
- **Hand-written C#.** A file written by hand keeps the attributes, and the generators and the runtime
  read either spelling, so nothing written against an earlier release has to change.
- **The few attributes that must stay.** `[GoInit]` on a package's `init` functions, the layout
  attributes on structs that mirror native memory, and the no-inline attribute on a func literal are read
  by the C# compiler or the .NET runtime from that exact declaration. The
  [reference](ConversionStrategies-Reference/source-generators.md#what-stays-an-attribute-and-why) lists
  each one with its reason.
