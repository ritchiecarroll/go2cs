# Strings (`@string` and `sstring`)

[Reference index](README.md) · [Summary of this topic](../ConversionStrategies.md#strings-string-and-sstring)

Go's `string` becomes golib [`@string`](https://github.com/ritchiecarroll/go2cs/blob/master/src/core/golib/string.cs), never `System.String`. Go strings are immutable byte sequences, so `len`, indexing, `range`, comparison, concatenation and conversions must follow Go's byte model, not C#'s UTF-16 model. A second type, golib [`sstring`](https://github.com/ritchiecarroll/go2cs/blob/master/src/core/golib/sstring.cs), is a stack-only view that allocates nothing, used where the converter can prove a string never escapes.

| Go | C# | Where |
|:--|:--|:--|
| a `string` value | `@string` | everywhere by default |
| `"text"` | `"text"u8` (a `ReadOnlySpan<byte>`) | converts to `@string` where a value is needed |
| a literal used as a value | `static readonly @string textˢ` | [hoisted literals](#a-value-materializing-string-literal-is-hoisted-to-a-static-readonly-field-beside-its-first-use) |
| `const name = "text"` inside a function | `static readonly @string nameᶜ` | [local string constants](#a-function-local-string-const-hoists-to-a-static-readonly-field-under-its-own-name) |
| a literal with raw non-UTF-8 bytes | `((@string)(new byte[]{…}))` | [raw-byte literals](#a-string-literal-with-raw-byte-escapes-emits-a-byte-array-string) |
| `type Token string` | `[GoType("@string")] partial struct Token` | [named string types](#named-string-types) |
| `string(b)` read and discarded | `(sstring)b` | [conversion views](#a-non-escaping-stringbyte-local-emits-the-stack-string-sstring) |
| a registered function's `string` parameter | an `sstring` member plus a generated `@string` member | [sstring twins](#an-sstring-twin-a-registered-function-gains-an-sstring-overload-that-calls-bind) |

## `@string` is a byte string, and slicing it is a window

`@string` holds a backing byte array plus an **offset and a length**, the shape of Go's string header. `s[i:j]` returns a window over the same array: it is O(1) and allocates nothing. That keeps the common rune-walking loop linear:

```go
for i := 0; i < len(s); {
	r, size := utf8.DecodeRuneInString(s[i:])
	i += size
}
```

Sharing the array is safe because `@string` is immutable, and every conversion out to mutable storage copies: `[]byte(s)`, `[]rune(s)` and the `byte[]` operator. The backing array is private, so no consumer can read past its window by mistake. `unsafe.StringData` pins a window that does not start at the array's start by copying its bytes first.

The zero value is null-safe and reads as `""`, so `default!` stands for Go's zero string. As a generic type argument a string is `@string` too (`Pair<nint, @string>`), which satisfies the `new()` constraint every converted type parameter carries.

## String literals render as `u8` spans

A literal renders as a C# UTF-8 literal and converts to `@string` only where a string value is needed:

```go
var s string = "ready"
bs := []byte("hello")
rs := []rune("héllo")
```
```csharp
@string s = "ready"u8;
var bs = slice<byte>("hello"u8);
var rs = slice<rune>((@string)"héllo");
```

The span form is used in every slot that accepts it: assignments, `string` parameters, struct fields (positional or keyed), typed slice and array elements, and concatenations. C# folds `"a"u8 + "b"u8` into one literal. A concatenation with a string value binds golib's `operator +(@string, ReadOnlySpan<byte>)`, which copies the literal's bytes straight into the result.

Some slots cannot take a span, and the literal changes form there:

| Slot | Rendering | Why |
|:--|:--|:--|
| an `any` / interface slot | `(@string)"text"u8` | a span cannot be boxed |
| `panic`, `print`, a vararg `...any` concatenation | `"text"` (a C# string) | the operand of an `object` concat cannot be a span |
| a conversion to a named string type | `((errorString)(@string)"kaboom"u8)` | C# applies only one user-defined conversion |
| `""` returned as a named string type | `(@string)""` | the same one-conversion limit |
| a sliced literal in a concatenation | `((@string)"012345678"u8[..n]) + "000"u8[..m]` | a slice of a `u8` literal is not a literal, so C# cannot fold it |
| `[]rune("…")`, or `[]byte` of a constant concatenation | `slice<rune>((@string)"héllo")` | the rune conversion goes through `@string` |

Parentheses are transparent: `(x)` renders exactly as `x` would in the same slot. The behavioral tests `StringLiteralSliceConversion`, `NamedStringConversion`, `NamedStringZeroValue`, `CompositeElementStringConcat`, `AnyStringLitComposite` and `ParenthesizedConcatContext` cover these forms.

## A string indexed by a wide/unsigned integer takes an `(int)` cast

A **string** indexed by a wide/unsigned integer takes the same `(int)` cast: a string LITERAL renders as a `ReadOnlySpan<byte>` (`"…"u8`) whose indexer takes `int`, so a `uintptr` index is CS1503 — runtime `heapdump.go`'s `"0123456789abcdef"[pc&15]` emitted `"…"u8[(uintptr)(pc & 15)]`. The index-expression emission routes a wide-kind index on any string-typed base through the cast — `"…"u8[(int)((uintptr)(pc & 15))]` — and an `@string` *variable*'s indexer binds an `int` argument too, so both renders are covered; an int/small index is unchanged. (Guarded by the `ArrayWideIndexAddress` extension — literal and variable string bases with `uintptr`/`uint64` indexes, byte values vs Go.)

## A string literal with raw-byte escapes emits a byte-array `@string`

A C# literal cannot always hold Go's bytes. Go's `\xHH` is exactly one byte, but C#'s `\x` takes one to four hex digits and names a UTF-16 code unit. A `u8` literal also re-encodes any character at or above U+0080 as two or more bytes. So a literal that holds a raw byte emits the exact bytes:

```go
const zipdata = "\x50\x4b\xdb50\xff"
const octal   = "\377\200A"
```
```csharp
internal static readonly @string zipdata = ((@string)(new byte[]{0x50, 0x4b, 0xdb, 0x35, 0x30, 0xff}));
internal static readonly @string octal = ((@string)(new byte[]{0xff, 0x80, 0x41}));
```

The byte-array form is used when:
- a `\xHH` escape is `\x80` or higher, or is followed by a hex digit (C# would read the digit as part of the escape);
- a `\NNN` octal escape is `\200` or higher (octal escapes below `\200` render as `\uXXXX`);
- a folded `const` value (a constant concatenation) is not valid UTF-8, or would need one of the escapes above.

A `var` initializer renders as an expression, so each literal piece of a concatenation takes its own form. The outer parentheses matter: `"…"[i]` must index the `@string`, not the inner `byte[]`. Literals written with real UTF-8 characters (`"Michał"`) keep the readable `u8` form. `convBasicLit.stringLiteralNeedsByteArray` decides it. The behavioral tests `HexByteStringLiteral`, `ByteTableStringConst` and `ByteTableStringVar` guard it.

## Converting between strings and slices

| Go | C# |
|:--|:--|
| `[]byte(s)`, `[]rune(s)` | `slice<byte>(s)`, `slice<rune>(s)`, which copy |
| `string(b)`, `string(r)` | `(@string)b`, `(@string)r`, which copy (or a [view](#a-non-escaping-stringbyte-local-emits-the-stack-string-sstring)) |
| a defined string type to `[]byte` | `slice<byte>((@string)v)`: the explicit `@string` step leaves one implicit conversion |
| to or from a slice of a defined byte type | `widen<byte, Uint8>(slice<byte>((@string)"hello"u8), elemᴛ0 => (Uint8)elemᴛ0)`: an element-wise copy, which is what Go's conversion costs anyway |
| a literal to a named `[]byte` type | `((htmlSig)slice<byte>((@string)"<!DOCTYPE HTML"u8))` |

`string([]rune)` writes U+FFFD for each invalid rune (a surrogate, or a value outside `0..0x10FFFF`), as Go does. golib's `builtin.ToUTF8Bytes` is the single encoder. The behavioral tests `DefinedElemStringConversion`, `NamedByteSliceFromStringLit` and `InvalidRuneString` cover these.

## A value-materializing string literal is HOISTED to a `static readonly` field beside its first use

Go keeps literals in read-only memory, so `return "true"` allocates nothing. In C# each conversion of a `u8` literal to `@string` copies it. So a literal that becomes a value is hoisted: a package-wide pre-pass (`hoistedLiteralOperations.go`) gives each such literal one field, declared above the function that first uses it, and every use names the field:

```go
func FormatBool(b bool) string {
	if b {
		return "true"
	}
	return "false"
}
```
```csharp
// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string trueˢ = "true"u8;
private static readonly @string falseˢ = "false"u8;

public static @string FormatBool(bool b) {
    if (b) {
        return trueˢ;
    }
    return falseˢ;
}
```

**What hoists:** a returned value; an assignment to a local, parameter or field; a `string` or `...string` argument; an `any` target (argument, result, send, assignment); a standalone map-index key; a conversion to a named string type. A literal used **only** in `any` slots is hoisted pre-boxed, as `static readonly object xˢ = (@string)"…"u8;`, so those uses allocate nothing.

**What stays inline:**

| Context | Why |
|:--|:--|
| comparisons, including lowered `switch` chains | a `u8` span compares in place |
| concatenation operands | the concat operator reads the span directly |
| `[]byte("…")` / `[]rune("…")` sources | the result must be a fresh mutable copy anyway |
| a format string: the `string` parameter before the `...any` of a callee named `…f` | it slugs badly (`"%v"`). When the callee is an [sstring twin](#an-sstring-twin-a-registered-function-gains-an-sstring-overload-that-calls-bind), the literal binds a view and costs nothing |
| a slug with no ASCII word, or of 3 characters or fewer | the name would say less than the value |
| `""` | it already costs nothing |
| composite-literal elements and keys | thousands of fields would move table-building work into type initializers |
| `func init()` bodies and package-level initializers | they run once |
| a function literal outside any function declaration | there is nowhere to declare the field |
| raw-byte literals | they already take the byte-array form |
| declarations in a hand-owned file or function | the converter does not emit them |
| arguments of builtins (`panic`, `print`, `copy`, …) | each builtin has its own emission path |

**Naming.** The field is a camelCase slug of the literal's ASCII words, at most 24 characters in total, plus the `ˢ` marker (`HoistedLiteralMarker`). An all-caps word folds whole (`"TESTING KEY"` → `testingKeyˢ`). A literal whose first word alone exceeds 24 characters gets no name and stays inline. Colliding slugs take a package-wide ordinal (`fooˢ`, `fooˢ2`). The marker keeps the names clear of C# keywords and Go identifiers.

**Initialization order.** C# runs static field initializers in text order within a file and in an unspecified order across files. A function that reads a hoisted field is registered as a reader, so a package-level `var` initializer that reaches one moves into the ordered static constructor (`initOrderOperations`), which runs after every field initializer. The `-tests` conversion has no static constructor to move into, so there a function a package-level initializer can reach does not hoist its literals.

**Two-pass `-tests` conversion.** An internal test file emits into the production class, so its pass is seeded with the production literal-to-field map and may only reference those fields, never declare them again. An external `<pkg>_test` package declares its own. Production output is identical whether or not tests are converted.

Emission is one substitution in `convExpr`'s `*ast.BasicLit` arm; the decisions are made before any file emits. The behavioral test `StringLiteralHoisting` covers every row above.

## A function-LOCAL string const hoists to a `static readonly` field under its own name

A string constant declared inside a function would otherwise be a local initialized from a `u8` literal, copied into a new `@string` on every call. It hoists to one field named after the constant, with the `ᶜ` marker (`HoistedConstMarker`, shared with [local big constants](named-numeric-types.md#a-function-local-gobigconst-hoists-its-parse-to-a-static-readonly-field)). The local copies the field, which allocates nothing, so every reference is unchanged:

```go
func Atoi(s string) (int, error) {
	const fnAtoi = "Atoi"
	…
}
```
```csharp
// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string fnAtoiᶜ = "Atoi"u8;

public static (nint, error) Atoi(@string s) {
    @string fnAtoi = fnAtoiᶜ;
    …
}
```

- The same name in two functions takes an ordinal (`fnAtoiᶜ1`).
- The field is `private`, or `internal` in a test-friend assembly (every standard-library package).
- A named string type keeps its type (`static readonly Kind kᶜ = "kind"u8;`).
- An empty constant stays a local. A package-level constant is already its own field and is unchanged.

The decision is made in the literal pre-pass (`collectLocalConsts`), and the function joins the hoisted-field readers. So the initialization-order rule above applies, and so do the pre-pass's exclusions: hand-owned code and `func init()`. The behavioral test `LocalStringConstHoist` and the unit test `TestLocalStringConstHoistsUnderItsOwnName` cover it.

## Named string types

`type Token string` becomes a `[GoType("@string")]` wrapper struct. The `InheritedType` template gives it the string surface, since C# indexing and `+` do not apply user-defined conversions:
- `byte this[int]` and `this[nint]` indexers;
- a `Range` indexer that returns the wrapper, so a sub-slice keeps the named type;
- `nint Length` for `len`;
- an implicit `ReadOnlySpan<byte>` conversion, so `u8` literals compare and assign;
- `+(T, T)`, `+(T, ReadOnlySpan<byte>)` and `+(ReadOnlySpan<byte>, T)`, so a concatenation keeps the named type and its methods.

```go
type Token string
func (t Token) First() byte { return t[0] }
const done Token = "done"
next := done + "-next"
```
```csharp
[GoType("@string")] partial struct Token;
internal static readonly Token done = "done"u8;
public static byte First(this Token t) => t[0];
Token next = done + "-next"u8;
```

A typed constant keeps its named type, whatever its value expression (a literal, a conversion such as `mapOp("Load")`, or a folded concatenation). It renders through the `u8` bridge, or through `(@string)` for a raw backtick value. A `:=` local keeps its declared type, and heap-boxes like any other local when its address escapes. An untyped string constant stays `@string`. The behavioral tests `NamedStringConsts`, `NamedStringDefine` and `NamedStringConcat` cover these.

## `sstring`: a string view that allocates nothing

`sstring` is a `readonly ref struct` over a `ReadOnlySpan<byte>`. Because it is a `ref struct`, C# rejects every way a string could escape: storing it in a field, array or map, boxing it, capturing it in a lambda, or using it as a type argument. A misuse is therefore a compile error, never an aliasing bug.

What it supports:

| Operation | Behavior |
|:--|:--|
| from `@string`, `slice<byte>`, `byte[]` | an implicit zero-copy view |
| from a `u8` literal | an implicit zero-copy view |
| from a C# `string` | encodes to UTF-8 (allocates) |
| to `@string`, `slice<byte>`, `byte[]` | copies (this is where an escaping string pays) |
| `len`, `s[i]`, `s[i..j]` | as `@string`; a sub-slice is another view |
| `==`, `<`, … against a `u8` literal, `@string` or `sstring` | compares the bytes in place |
| `+` with any string form | returns a new `@string`, copying each operand once |
| `for i, r := range s` | a `ref struct` enumerator that yields `(byte index, rune)` and allocates nothing |
| `append(b, s...)` | the `ꓸꓸꓸ` spread, a `ReadOnlySpan<byte>` |

The converter emits `sstring` in two places: conversion views (next) and [twins](#an-sstring-twin-a-registered-function-gains-an-sstring-overload-that-calls-bind).

Performance: `src/tests/Performance/PerfStringView` measures the view on keyword comparisons over a runtime-built buffer, and `PerfString` is the `@string` baseline, whose conversions are not eligible.

## A non-escaping `string([]byte)` local emits the stack-string `sstring`

Go skips the copy in `string(b)` when the string is only read while `b` cannot change. The converter recovers that for four shapes (`markSStringEligible` and the `markSString…` passes in the escape analysis):

- **A local** `s := string(b)`, where `b` is an unnamed `[]byte` that the function never writes, and every use of `s` is `len`, an index, a comparison, a `switch` tag or a concatenation operand. It is never passed on, stored, ranged, returned or reassigned.
- **A comparison operand**: `string(b) == other`.
- **A `switch` tag**: `switch string(b) { … }`. A string switch lowers to a temp and `==` comparisons.
- **A concatenation operand**: `string(b) + other`. The result is a new `@string`; only the operand's copy is skipped.

In the last three, `other` (or every `case` label) must be unable to change `b` before the view is read: a literal, a plain read of a variable or field, or another `string([]byte)`. A function call or a named string type keeps the `@string` copy.

```go
if string(hdr[:4]) == "ZLIB" { … }
switch string(cmd) {
case "get": …
}
```
```csharp
if (((sstring)(hdr[..4])) == "ZLIB"u8) { … }
var exprᴛ1 = ((sstring)cmd);
if (exprᴛ1 == "get"u8) { … }
```

A conversion of a bare, never-written identifier that repeats (two or more uses, or one inside a loop) is hoisted to a single `sstring` temp at function scope (`planSStringHoists`), because the JIT does not hoist a `ref struct` view out of a loop. The behavioral test `SStringElision` covers every eligible and every rejected shape.

## An sstring TWIN: a registered function gains an `sstring` overload that calls bind

A `string` parameter is an `@string`, so every literal argument is copied into one: `fmt.Sprintf("xxx")` allocates the literal and then the result, where Go allocates only the result. A **twin** gives a registered function a second member that takes `sstring`:

- **The converter** emits the member that carries the Go body, with each registered parameter typed `sstring` and marked `[GoStr]`:

  ```csharp
  [GoStr] public static @string Sprintf(sstring format, params ꓸꓸꓸany aʗp) {
      …
  }
  ```

- **`StrGenerator`** (go2cs-gen) emits the companions into a generated file. The first is the `@string` member, which forwards under a lower overload priority. The second, for a package-level function only, is the canonical value delegate (attribute names shortened):

  ```csharp
  [GeneratedCode("go2cs-gen", …), OverloadResolutionPriority(-1)]
  public static global::go.@string Sprintf(global::go.@string format, params global::System.Span<object> aʗp) => Sprintf((global::go.sstring)format, aʗp);

  public static readonly global::go.Funcꓸꓸꓸ<global::go.@string, object, global::go.@string> Sprintfᶠ =
      [global::go.GoTwinForwarder("Sprintf")] static (global::go.@string format, global::System.Span<object> aʗp) => Sprintf(format, aʗp);
  ```

**Calls do not change.** Wherever both members apply, the priority picks the `sstring` one. That covers a `u8` literal (through `sstring`'s implicit conversion), an `@string` (as a view) and a C# string. `fmt.Sprintf("xxx"u8)` copies nothing for its argument.

**Function values name the delegate.** With two members there is no single method group, and converting one to a delegate typed on `@string` is CS0123, even with a cast. So the converter renders a func-value use of a package-level twin as `Nameᶠ` (`FuncValueMarker`):

```csharp
["printf"u8] = ((Funcꓸꓸꓸ<@string, any, @string>)(fmt.Sprintfᶠ)),
Funcꓸꓸꓸ<@string, any, error> noVetErrorf = fmt.Errorfᶠ;
```

- One delegate object serves every site, so `reflect.ValueOf(fmt.Sprintf).Pointer()` is equal at every site, as in Go.
- The lambda carries `[GoTwinForwarder("Sprintf")]`, so `runtime.FuncForPC(…).Name()` reads `fmt.Sprintf`.
- The lambda's body is an ordinary call, which binds the `sstring` member directly.
- A traceback skips both companions: the forwarder through `[GeneratedCode]`, the lambda through `[GoTwinForwarder]`.
- A twinned method has no canonical delegate. Using one as a method value stops the conversion.

**Deferred and `go` calls** take the temp-parameter lambda form, `defer(ᴛ1 => Count(ᴛ1), …)`. The arguments stay `@string` generic type arguments, since a `ref struct` cannot be one.

**Pointer receivers.** RecvGenerator gives the `[GoStr]` member its `ж<T>` overload. A pointer-receiver call with an `@string` argument binds it through the implicit view, so the forwarder needs none.

**Records.** `package_info.cs` publishes each exported package-level twin to other packages:

```csharp
// <SStringTwins>
[assembly: GoSStringTwin("Sprintf")]
// </SStringTwins>
```

A converting package reads the record, or the embedded standard-library metadata under `-recurse=nuget`, and renders its func values of `fmt.Sprintf` as `fmt.Sprintfᶠ`. The section is omitted when a package has no twins.

**The list.** Twins are an explicit registry, `sstringTwins` in `sstringTwinOperations.go`, keyed `"<pkgPath>.<Func>"` or `"<pkgPath>.<Recv>.<method>"` and listing the twinned parameter indices. It holds fmt's format parameters and the helpers they pass them to, plus `unicode/utf8`'s `DecodeRuneInString` and `RuneCountInString`: 32 parameters in 29 functions. The converter refuses an entry (`validateSStringTwin`) that:
- is hand-owned, has no body, or is generic;
- registers an index that is out of range or is the variadic tail;
- has a registered parameter that is not `string`;
- has a blank parameter (`_` or unnamed), which the `@string` forwarder could not pass on;
- captures a registered parameter in a closure, or uses it in a `defer` or `go` statement;
- binds a registered parameter to a local, because the implicit `sstring` → `@string` conversion would copy silently.

**Guards.**
- `TestSStringTwinEmission` covers the emission, the value sites (bare and cross-package), the defer and go forms, and the records.
- `TestNoSStringTwinMethodGroupInCorpus` fails on any method-group use of a twin in `src/core` or the behavioral goldens.
- The behavioral test `SStringTwinPilot` compares every call form, value site, identity and name against Go.

---

[← Slices and Arrays](slices-and-arrays.md) · [Index](README.md) · [Maps and Channels →](maps-and-channels.md)
