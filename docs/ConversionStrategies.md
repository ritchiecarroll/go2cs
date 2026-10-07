# Conversion Strategies
<!-- {% raw %} — Jekyll/Liquid guard: this page contains Go composite-literal and template syntax ({{ … }}) that Liquid would otherwise parse; the HTML comment hides the tag on GitHub. -->

> **How `go2cs` turns each Go construct into C#, one section per topic, with the Go and the C# it
> becomes side by side.** This page is for a Go developer reading converted code, or anyone evaluating
> go2cs. Each section ends with a link into the [reference](ConversionStrategies-Reference/README.md),
> which holds every emitted form, edge case and guard test, for maintainers. Each section stands on its
> own, so you can start with whichever construct you care about.

The converted C# aims to be both **behaviorally** and **visually** similar to the Go it came from, so a Go
developer can read it and follow it. Two things make that possible: a hand-written runtime library,
[golib](#the-golib-runtime-library), and a set of Roslyn [source generators](#source-generators) that add
the members C# cannot spell directly.

> The C# snippets below are drawn from the actual converted standard library (`src/core/`)
> wherever possible, paired with their original Go source; the rest come from the behavioral
> tests. Each code block carries an HTML comment naming the file and line it was copied from. The glyphs
> you will see, such as `ж`, `Ꮡ` and `Δ`, are listed in
> [Reading Converted Code](#reading-converted-code-names-and-glyphs).

---

## Contents

- **Start here:** [At a glance](#at-a-glance) · [Reading Converted Code: Names and Glyphs](#reading-converted-code-names-and-glyphs) · [The golib Runtime Library](#the-golib-runtime-library)
- **Packages & projects:** [Package Conversion](#package-conversion) · [Package-Level Variable Initialization Order](#package-level-variable-initialization-order) · [Converted Tests](#converted-tests) · [Compiled Library versus Source Code](#compiled-library-versus-source-code)
- **Numbers, constants & nil:** [Constant Values](#constant-values) · [Integer Types and Arithmetic](#integer-types-and-arithmetic) · [Named Numeric Types and Constant Contexts](#named-numeric-types-and-constant-contexts) · [Nil and Zero Values](#nil-and-zero-values) · [Built-in Functions](#built-in-functions) · [Empty Interface (`any`)](#empty-interface-any)
- **Assignment & scope:** [Multi-Assignment and Evaluation Order](#multi-assignment-and-evaluation-order) · [Short Variable Redeclaration (Shadowing)](#short-variable-redeclaration-shadowing) · [Multi-Result Values and Comma-Ok Forms](#multi-result-values-and-comma-ok-forms)
- **Composite types:** [Slices and Arrays](#slices-and-arrays) · [Strings (`@string` and `sstring`)](#strings-string-and-sstring) · [Maps](#maps) · [Generics](#generics) · [Type Aliasing](#type-aliasing)
- **Functions & control flow:** [Functions and Methods](#functions-and-methods) · [Function Values and Closures](#function-values-and-closures) · [Loops, Range and Labels](#loops-range-and-labels) · [Expression Switch Statements](#expression-switch-statements) · [Type Switch Statements](#type-switch-statements) · [Defer / Panic / Recover](#defer--panic--recover)
- **Concurrency:** [Goroutines](#goroutines) · [Channels and `select`](#channels-and-select)
- **Types & polymorphism:** [Struct Types](#struct-types) · [Struct Type Embedding](#struct-type-embedding) · [Interfaces](#interfaces) · [Reflection (`reflect`)](#reflection-reflect)
- **Pointers & memory:** [Pointers](#pointers) · [Implicit Pointer Dereferencing](#implicit-pointer-dereferencing) · [`unsafe.Pointer` and `uintptr`](#unsafepointer-and-uintptr)
- **The machinery:** [Source Generators](#source-generators) · [Functions Without a Go Body](#functions-without-a-go-body) · [Manually-Converted Declarations](#manually-converted-declarations) · [The standard library reproduces Go `-tags purego`](#the-standard-library-reproduces-go--tags-purego) · [Comments](#comments) · [Packages That Do Not Type-Check](#packages-that-do-not-type-check) · [Deterministic Output](#deterministic-output)

---

## At a glance

go2cs turns Go source into C# that a Go developer can read line by line. A Go package becomes one C#
`partial` class whose members are all `static`. It is `partial` because each of the package's files adds
its own part to the same class. Go's types become C# types from the [golib](#the-golib-runtime-library)
runtime library, and a few Unicode glyphs mark the names the converter adds.

This section shows two short, complete examples, then a table that pairs every Go construct with the C#
form it takes. Each part links to the section that explains it in full.

<!-- This section runs longer than most on purpose: it is the page's entry point, so a reader who lands here first sees two whole, self-explaining examples before the table. -->

**A whole program.** This Go program reads a map with the comma-ok form. Its C# conversion follows it,
complete:

<!-- source: src/tests/Behavioral/MapCommaOk/main.go:10-39 -->
```go
package main

import "fmt"

func main() {
	m := map[string]int{"a": 1, "b": 2}

	// value + ok, present.
	v, ok := m["a"]
	fmt.Println(v, ok) // 1 true

	// value + ok, absent (value is the zero value).
	v2, ok2 := m["z"]
	fmt.Println(v2, ok2) // 0 false

	// blank value, present.
	_, ok3 := m["b"]
	fmt.Println(ok3) // true

	// if-init comma-ok with a blank value.
	if _, ok4 := m["z"]; !ok4 {
		fmt.Println("z absent") // printed
	}

	// reassignment (not a new declaration) into existing variables.
	var w int
	var present bool
	w, present = m["b"]
	fmt.Println(w, present) // 2 true
}
```
<!-- source: src/tests/Behavioral/MapCommaOk/main.cs.target:1-29 -->
```csharp
namespace go;

using fmt = fmt_package;

partial class main_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object zAbsentˢ = (@string)"z absent"u8;

internal static void Main() {
    var m = new map<@string, nint>{["a"u8] = 1, ["b"u8] = 2};
    var (v, ok) = m["a"u8, ꟷ];
    fmt.Println(v, ok);
    var (v2, ok2) = m["z"u8, ꟷ];
    fmt.Println(v2, ok2);
    var (_, ok3) = m["b"u8, ꟷ];
    fmt.Println(ok3);
    {
        var (_, ok4) = m["z"u8, ꟷ]; if (!ok4) {
            fmt.Println(zAbsentˢ);
        }
    }
    nint w = default!;
    bool present = default!;
    (w, present) = m["b"u8, ꟷ];
    fmt.Println(w, present);
}

} // end main_package
```

What to notice:

- **The package is a class.** `package main` becomes `partial class main_package` in `namespace go`, and
  each function is a `static` method of it. `func main` becomes `Main`, the C# entry point
  ([Package Conversion](#package-conversion)).
- **An import is a `using` alias.** `import "fmt"` becomes `using fmt = fmt_package;`, so calls still read
  `fmt.Println(…)`.
- **Go types map to golib types.** `map[string]int` becomes golib's `map<@string, nint>`. `@string` is
  Go's `string`, a byte string; the `@` lets C# use its keyword `string` as a name. `nint` is C#'s
  native-sized integer, which is Go's `int` ([Maps](#maps), [Strings](#strings-string-and-sstring),
  [Integer Types](#integer-types-and-arithmetic)).
- **String literals are UTF-8.** `"a"u8` is a C# UTF-8 literal. It becomes an `@string` wherever a string
  value is needed.
- **Comma-ok is a two-value indexer.** `v, ok := m["a"]` becomes `var (v, ok) = m["a"u8, ꟷ];`. The glyph
  `ꟷ` is a golib constant. Passing it picks the comma-ok form, which returns the value and a found flag
  ([Comma-Ok Forms](#multi-result-values-and-comma-ok-forms)).
- **Some literals are stored once.** `zAbsentˢ` is the `"z absent"` literal. The converter moves it into
  the `static readonly` field declared just before `Main`, and the `ˢ` suffix marks such a field. Other
  literals, such as the map keys, stay inline; [Strings](#strings-string-and-sstring) gives the rule.
- **A literal passed as `any` is boxed in advance.** `fmt.Println` takes `any` arguments, and in converted
  code `any` is an alias for C#'s `object`. So `zAbsentˢ` is an `object` field. The `(@string)` cast makes
  the boxed value a Go `string`, so `fmt` prints it as one ([Empty Interface](#empty-interface-any)).
- **An `if` with an init statement gets its own braces.** The extra `{ … }` keeps `ok4` scoped to the `if`,
  as in Go ([Shadowing](#short-variable-redeclaration-shadowing)). The init statement and the `if` stay on
  one line, as Go writes them.
- **`default!` is the zero value.** `var w int` becomes `nint w = default!;`. The `!` is C#'s
  null-forgiving operator; it only silences a nullable warning ([Nil and Zero Values](#nil-and-zero-values)).
- **Go comments are dropped by default.** This conversion ran without the `-comments` option, which keeps
  them ([Comments](#comments)). The `// Hoisted @string literals` line is a note the converter always writes.

**A type and a method.** A struct, a function that returns a pointer to it, and a method with a pointer
receiver:

<!-- source: src/tests/Behavioral/PackageVarInitOrder/registry.go:3-16 -->
```go
type reg struct {
	entries []string
	count   int
}

func newReg() *reg {
	return &reg{}
}

func (r *reg) add(name string) string {
	r.entries = append(r.entries, name)
	r.count++
	return name + "-added"
}
```
<!-- source: src/tests/Behavioral/PackageVarInitOrder/registry.cs.target:5-18 -->
```csharp
[GoType] partial struct reg {
    internal slice<@string> entries;
    internal nint count;
}

internal static ж<reg> newReg() {
    return Ꮡ(new reg(nil));
}

[GoRecv] internal static @string add(this ref reg r, @string name) {
    r.entries = append(r.entries, name);
    r.count++;
    return name + "-added"u8;
}
```

What to notice:

- **A struct holds only its fields.** `[GoType]` asks a [source generator](#source-generators) to add the
  rest at compile time: constructors, `==` and a Go-style `ToString()` ([Struct Types](#struct-types)).
  Lower-case Go names become `internal`, and capitalized ones become `public`.
- **A slice is golib's `slice<T>`.** `[]string` becomes `slice<@string>` ([Slices and Arrays](#slices-and-arrays)).
- **An empty struct literal passes `nil`.** Go's `reg{}` becomes `new reg(nil)`. It calls the generated
  constructor that builds the zero `reg`, with every field at its zero value ([Struct Types](#struct-types)).
- **A pointer is golib's heap box `ж<T>`** (read "zhe"). `*reg` becomes `ж<reg>`. In `&reg{}`, the
  function `Ꮡ(…)` takes the address: it boxes the new value on the heap and returns the `ж<reg>`. A name
  that starts with `Ꮡ`, such as `Ꮡx` in the table, is a different use of the glyph: it is the box that
  holds a local whose address is taken ([Pointers](#pointers)).
- **A pointer-receiver method is an extension method on `ref`.** `this ref reg r` lets the method change the
  caller's struct without a copy. `[GoRecv]` asks a generator to add an overload that takes a `ж<reg>`, so
  the method also works on a pointer ([Functions and Methods](#functions-and-methods)).
- **Built-ins keep their Go names.** `append` here, like `len`, `cap` and `copy`, is a golib function that
  behaves as Go's does ([Built-in Functions](#built-in-functions)).

**The glyphs, in brief.** The glyphs are Unicode letters, not operators, so C# accepts them in names;
`goǃ`, for example, ends in a letter that only looks like `!`. `ж<T>` is a pointer type, `Ꮡ` takes an
address, and `~p` reads through a pointer with golib's `~` operator on `ж<T>`. A `Δ` marks a renamed
name, such as `xΔ1` for a shadowing variable, and `@` escapes a C# keyword. `ꟷ` picks the comma-ok form of
a map index or channel receive, and `ᐧ` does the same for a type assertion.

`ᒐ` is the frame that runs a function's deferred calls, and `goǃ` starts a goroutine. `ᐸꟷ`, drawn to look
like Go's `<-`, sends to or receives from a channel. `ˢ` marks a string literal stored once.
[Reading Converted Code](#reading-converted-code-names-and-glyphs) lists every glyph with an example.

**Every construct, in one table.** Each row pairs a Go construct with the C# form it takes, and its first
column links to the section that explains it. The last column, Provided by, names where the behavior
lives; the converter writes all of the text. "converter" means the C# form alone does the job. "golib"
means a [golib](#the-golib-runtime-library) type or function implements it at run time. A generator name
(TypeGenerator, RecvGenerator, ImplementGenerator or PartialStubGenerator) means that
[source generator](#source-generators) adds members at compile time.

<!-- sources, one per row, top to bottom (every C# form is copied from real emission):
  src/core/bufio/bufio.cs:17 · src/core/bufio/bufio.cs:14 ·
  src/tests/Behavioral/PackageVarInitOrder/registry.cs.target:22 + src/core/image/png/reader.cs:1227 ·
  src/core/reflect/all_test.cs:59 + src/core/reflect/go2cs_test_host.cs:1 ·
  src/tests/Behavioral/MapCommaOk/MapCommaOk.csproj:150 (ProjectReference; NuGet via -recurse=nuget) ·
  src/core/unicode/utf8/utf8.cs:23 + src/core/archive/zip/struct.cs:34 ·
  src/tests/Behavioral/PointerToPointer/PointerToPointer.cs.target:17 + src/core/strconv/atoi.cs:145 ·
  src/core/golib/uintptr.cs:39 · src/core/bufio/bufio.csproj:131 (from src/go2cs/csproj-template.xml:116) ·
  src/core/encoding/gob/codec_test.cs:1156 ·
  src/core/golib/NilType.cs + src/tests/Behavioral/LambdaFunctions/LambdaFunctions.cs.target:62 ·
  src/tests/Behavioral/SliceAliasing/main.cs.target:8 · src/tests/Behavioral/MapCommaOk/MapCommaOk.csproj:104 ·
  src/tests/Behavioral/NamedReturnDefer/main.cs.target:41 · src/tests/Behavioral/ShadowedCompoundAssign/main.cs.target:10 ·
  src/core/strconv/atoi.cs:263 · src/tests/Behavioral/MapCommaOk/main.cs.target:12 ·
  src/tests/Behavioral/RangeStatements/RangeStatements.cs.target:20 · src/core/strconv/atoi.cs:260 ·
  src/tests/Behavioral/MapCommaOk/main.cs.target:11 · src/tests/Behavioral/GenericFuncDecl/GenericFuncDecl.cs.target:7 +
  src/tests/Behavioral/GenericInterfaceConstraint/GenericInterfaceConstraint.go:53 + GenericInterfaceConstraint.cs.target:53 ·
  src/tests/Behavioral/TypeConversionReturnType/TypeConversionReturnType.cs.target:1 ·
  src/tests/Behavioral/PackageVarInitOrder/registry.cs.target:14 · src/tests/Behavioral/LambdaFunctions/LambdaFunctions.cs.target:59 +
  src/tests/Behavioral/LocalFunctionEmission/main.cs.target:15 (local function only from a `name := func…` short declaration
  whose variable is only called; a `var f T = func…` stays a lambda, LambdaFunctions.go:49,52) ·
  src/tests/Behavioral/ForVariants/ForVariants.cs.target:53,59 ·
  src/tests/Behavioral/ExprSwitch/ExprSwitch.cs.target:101 · src/tests/Behavioral/TypeAssert/TypeAssert.cs.target:46,77 +
  src/tests/Behavioral/TypeSwitch/TypeSwitch.cs.target:52 · src/tests/Behavioral/DeferSimple/DeferSimple.cs.target:13-20 +
  src/tests/Behavioral/PanicRecover/PanicRecover.cs.target:26,49 · src/tests/Behavioral/SelectStatement/SelectStatement.cs.target:80 ·
  src/tests/Behavioral/SelectStatement/SelectStatement.cs.target:10,34,79,82 · src/tests/Behavioral/PackageVarInitOrder/registry.cs.target:5 ·
  src/tests/Behavioral/StructPromotion/StructPromotion.cs.target:30 · src/core/io/io.cs:86 ·
  src/tests/Behavioral/ReflectValueSingles/ReflectValueSingles.cs.target:56 + src/core/reflect/type.cs:1135 + src/core/golib/GoReflect.cs:19 ·
  src/tests/Behavioral/PointerToPointer/PointerToPointer.go:25 + PointerToPointer.cs.target:22,28 + src/core/golib/ж.cs:665 ·
  src/tests/Behavioral/ClosureSelfShadowCapture/main.cs.target:24 · src/tests/Behavioral/UnsafeOperations/UnsafeOperations.cs.target:80,93 ·
  src/core/math/dim_asm.cs:11 · src/core/unicode/utf8/utf8.cs:23 (the Comments row: placement is best effort, see the Comments section)
  Glyph-brief facts: `goǃ` ends in U+01C3 (a Latin letter); `~` on ж<T> is src/core/golib/ж.cs:665; `ꟷ`/`ᐧ` uses per the glyph table rows (golib const bool ꟷ = false, ᐧ = true).
  `new reg(nil)`: the generator's nil constructor builds the zero value, src/gen/go2cs-gen/Templates/StructType/StructTypeTemplate.cs (Struct Types section).
  License header always kept: Comments section ("Apart from the license header, Go's comments appear only with -comments"). -->

| Section | Go | C# | Provided by |
|---|---|---|---|
| [Packages](#package-conversion) | `package bufio` | `partial class bufio_package` in `namespace go`; functions are `static` methods | converter |
| [Packages](#package-conversion) | `import "unicode/utf8"` | `using utf8 = unicode.utf8_package;` and a reference to that package's project | converter |
| [Initialization](#package-level-variable-initialization-order) | `var names = []string{…}` · `func init()` | a `static` field · `[GoInit] internal static void init()`, run once when the package loads | converter |
| [Converted tests](#converted-tests) | `func TestBool(t *testing.T)` | a `static` method in a separate `<pkg>.tests` program, which runs each test the way `go test` does | converter |
| [Library or source](#compiled-library-versus-source-code) | an imported package | a project reference, or a NuGet reference to the pre-converted standard library | converter |
| [Constants](#constant-values) | `const UTFMax = 4` · `Deflate uint16 = 8` | `public static UntypedInt UTFMax => 4;`, a golib type that keeps an untyped constant's exact value · `public const uint16 Deflate = 8;` | converter |
| [Integers](#integer-types-and-arithmetic) | `int` · `uint` | `nint` · `nuint`, the C# native-sized integers | converter |
| [Integers](#integer-types-and-arithmetic) | `uintptr` | golib's `uintptr` struct | golib |
| [Integers](#integer-types-and-arithmetic) | `int32` · `rune` · `float64` · … | same-named C# aliases declared in each project file, such as `rune` for `System.Int32` | converter |
| [Named numbers](#named-numeric-types-and-constant-contexts) | `type Float float64` | `[GoType("num:float64")] public partial struct Float;` | TypeGenerator |
| [Nil and zero](#nil-and-zero-values) | `nil` | `default!`, or golib's `nil` in a pointer comparison or pointer argument | golib |
| [Built-ins](#built-in-functions) | `len(s)` · `append(s, x)` · `make([]uint32, 6)` | `len(s)` · `append(s, x)` · `new slice<uint32>(6)`, the first two from golib's `builtin` class | golib |
| [`any`](#empty-interface-any) | `any` · `interface{}` | `any`, an alias for `object` declared in each project file | converter |
| [Multi-assignment](#multi-assignment-and-evaluation-order) | `a, b = b, a` | `(a, b) = (b, a);` | converter |
| [Shadowing](#short-variable-redeclaration-shadowing) | an inner `x := 5` | `nint xΔ1 = 5;` | converter |
| [Multiple results](#multi-result-values-and-comma-ok-forms) | `func Atoi(s string) (int, error)` | `public static (nint, error) Atoi(@string s)` | converter |
| [Comma-ok](#multi-result-values-and-comma-ok-forms) | `v, ok := m["a"]` | `var (v, ok) = m["a"u8, ꟷ];` | golib |
| [Slices and arrays](#slices-and-arrays) | `[]int{2, 3, 4}` · `[N]T` | `new nint[]{2, 3, 4}.slice()` · `array<T>` | golib |
| [Strings](#strings-string-and-sstring) | `string` · `"Atoi"` | `@string` · `"Atoi"u8` | golib |
| [Maps](#maps) | `map[string]int{"a": 1, "b": 2}` | `new map<@string, nint>{["a"u8] = 1, ["b"u8] = 2}` | golib |
| [Generics](#generics) | `func Swap[T any](a, b T) (T, T)` · `[S Shape]` | `public static (T, T) Swap<T>(T a, T b)` · `where S : Shape` | converter |
| [Type aliasing](#type-aliasing) | `type P = *bool` | `global using P = go.ж<bool>;` | converter |
| [Methods](#functions-and-methods) | `func (r *reg) add(name string) string` | `[GoRecv] internal static @string add(this ref reg r, @string name)`, plus a `ж<reg>` overload | RecvGenerator |
| [Closures](#function-values-and-closures) | `func() string { … }` as a value | a lambda typed `Func<@string>`, or a C# local function when a `name := func…` variable is only ever called | converter |
| [Loops](#loops-range-and-labels) | `for _, n := range nums` · `break scan` | `foreach (var (_, n) in nums)` · `goto break_scan;` | converter |
| [Switch](#expression-switch-statements) | `case 4, 5, 6:` | `case 4 or 5 or 6:` | converter |
| [Type switch](#type-switch-statements) | `i.(string)` · `s, ok := i.(string)` · `switch i.(type)` | `i._<@string>()` · `var (s, ok) = i._<@string>(ᐧ)` · `switch (i.type())` | golib |
| [Defer and panic](#defer--panic--recover) | `defer f()` · `panic(v)` · `recover()` | `defer(…, ref ᒐ)` inside `try`/`catch`/`finally` · `throw panic(v)` · `recover()` | golib |
| [Goroutines](#goroutines) | `go generate(ch)` | `goǃ(generate, …)` | golib |
| [Channels](#channels-and-select) | `make(chan int)` · `ch <- 12` · `<-ch` · `select` | `new channel<nint>(0)` · `ch.ᐸꟷ(12)` · `ᐸꟷ(ch)` · `switch (select(…))` | golib |
| [Structs](#struct-types) | `type reg struct { entries []string; … }` | `[GoType] partial struct reg { internal slice<@string> entries; … }` | TypeGenerator |
| [Embedding](#struct-type-embedding) | `type Record struct { Person; Employee }` | `public partial ref Person Person { get; }`, plus the promoted fields and methods | TypeGenerator |
| [Interfaces](#interfaces) | `type Reader interface { … }` | `[GoType] partial interface Reader`, plus the glue that lets each type used as a `Reader` implement it | ImplementGenerator |
| [Reflection](#reflection-reflect) | `reflect.TypeOf(want)` | `reflect.TypeOf(want)`, unchanged: the converted `reflect` package, backed by golib | converter |
| [Pointers](#pointers) | `*T` · `&x` · `*p` | `ж<T>` · `Ꮡx` · `~p` to read, or `p.Value` to read or write | golib |
| [Implicit dereferencing](#implicit-pointer-dereferencing) | `s.val`, where `s` is a `*span` | `s.Value.val` | converter |
| [`unsafe`](#unsafepointer-and-uintptr) | `unsafe.Pointer` · `unsafe.Sizeof(x)` | `@unsafe.Pointer` · the constant `/* unsafe.Sizeof(x) */ 32` | converter |
| [No Go body](#functions-without-a-go-body) | `func archMax(x, y float64) float64` | `internal static partial float64 archMax(float64 x, float64 y);`, with a hand-written body or a throwing stub | PartialStubGenerator |
| [Comments](#comments) | `// maximum number of bytes …` | the same comment, near the same place (placement is best effort); kept only with `-comments`, except the license header, which is always kept | converter |

The machinery behind these forms has its own sections: [Source Generators](#source-generators),
[Manually-Converted Declarations](#manually-converted-declarations),
[the `purego` build](#the-standard-library-reproduces-go--tags-purego),
[Packages That Do Not Type-Check](#packages-that-do-not-type-check) and [Deterministic Output](#deterministic-output).

**Full detail:** [Reference → Contents](ConversionStrategies-Reference/README.md#contents) — one page per topic, with every emitted form, edge case and guard test.

---

## Reading Converted Code: Names and Glyphs

Converted C# keeps Go's names wherever it can: `bindAdd` stays `bindAdd`, and the entry point `main` becomes
`Main`. The naming rules in this section and the glyphs in the tables are the exceptions. The glyphs mark the
names and helpers the converter adds. Most are Unicode letters that C# accepts in identifiers and ordinary Go
code does not use, so they never clash with a Go name.

Go types such as `slice<T>`, `@string` (Go's `string`) and `ж<T>` (Go's pointer) come from golib, the
runtime library every converted project uses: see [The golib Runtime Library](#the-golib-runtime-library).

<!-- sources, in row order (behavioral paths under src/tests/Behavioral/): GoCallVariations/GoCallVariations.cs.target:58; UnsafeOperations/UnsafeOperations.cs.target:26; GoCallVariations.cs.target:40 and UnsafeOperations.cs.target:26; GoCallVariations.cs.target:42; ReservedNameShadows/main.cs.target:39, src/core/time/time.cs:324, src/core/bufio/bufio_test.cs:11; ExprSwitch/ExprSwitch.cs.target:144, MultiFileInitOrder/a_first.cs.target:11; GoCallVariations.cs.target:25; src/core/errors/join.cs:19 (heap-moved value parameter: AddressOfParamWrite/main.cs.target:50-51); GoCallVariations.cs.target:21, ChannelRendezvous/main.cs.target:32, InitOrderTupleSpecs/main.cs.target:9; GoCallVariations.cs.target:9, src/core/strconv/atoi.cs:260; SStringTwinPilot/main.cs.target:42; src/core/encoding/json/package_info.cs:17;
src/core/errors/join.cs:7, ExprSwitch.cs.target:27 (also src/core/flag/flag.cs:1176), ChannelRendezvous/main.cs.target:33; AnyStringLitChanSend/main.cs.target:66, GoCallVariations.cs.target:39, ChannelRendezvous/main.cs.target:34; GoCallVariations.cs.target:21; ChannelCapLen/main.cs.target:28, AnonymousStructs/AnonymousStructs.cs.target:39; src/core/strconv/atoi.cs:293, ExprSwitch.cs.target:129, src/core/strings/iter.cs:57; ExprSwitch.cs.target:283; AppendOfMake/AppendOfMake.cs.target:76; src/core/errors/join.cs:39, CaptureHoistThroughConversion/main.cs.target:41;
DefinedTypeOverInterface/main.cs.target:10; GenericTypeNameCompanion/main.cs.target:24; GenericTypeInstantiation/GenericTypeInstantiation.cs.target:44 and AnonymousInterfaces/AnonymousInterfaces.cs.target:51; AdapterNameInterfaceCollision/main.cs.target:25; GoCallVariations.cs.target:46; GoCallVariations.cs.target:47; UnsafeOperations.cs.target:4 and 23; src/core/strings/strings.cs:19; src/core/strings/export_test.cs:8 and strings_test.cs:24.
Box kinds: src/core/golib/ж.ElemRefBox.cs:40, ж.FieldRefBox.cs:40, ж.NativeBox.cs:41, ж.NativeArrayBox.cs:45, ж.SliceHeaderBox.cs:94, ж.HeaderSliceBox.cs:75 all derive from ж<T>.
appendꓸꓸꓸ: InterfaceCasting/InterfaceCasting.cs.target:323 (Funcꓸꓸꓸ: BlankIdentifierCollision/main.cs.target:88).
Buffer.Ꮡoff: PointerToPointer/PointerToPointer.cs.target:40 `PrintValPtr(Ꮡb.of(Buffer.Ꮡoff));` for Go PointerToPointer.go:49 `PrintValPtr(&b.off)`; the static field-reference members are generated by src/gen/go2cs-gen/Templates/StructType/StructTypeTemplate.cs:1357 (FieldReferences). ᒐdone: NamedReturnDefer/main.cs.target:92; [GoRecv]: ReservedNameShadows/main.cs.target:61.
/*<-*/: SelectStatement/SelectStatement.cs.target:70; _<T>(): TypeAssert/TypeAssert.cs.target:77 (comma-ok: src/core/strconv/atoi.cs:293); ᴋ: src/core/internal/syscall/unix/linux/getrandom.cs:38; break_/continue_: ForVariants/ForVariants.cs.target:59 (goto break_scan;) and :63 (continue_scan:;); main_point: LiftedLocalTypes/main.cs.target:17.
Glyph constants: src/go2cs/symbols.go, generated from src/core/go2cs/symbols.json (DescriptorCarrierSuffix in src/go2cs/visitTypeSpec.go, DescriptorCompanionSuffix in src/go2cs/descriptorCompanion.go). Values of ᐧ, ᐧᐧ, ꟷ and ꓸꓸꓸ: src/core/golib/builtin.cs:160-196. The slice spread s.ꓸꓸꓸ: src/core/golib/slice.cs:490.
Overload discriminators: the comma-ok overloads take a `bool _` whose value is unused: golib builtin.cs:611 (receive), map.cs:308 (map index), builtin.cs:2869-2873 (type assertion); the converter passes ꟷ (false) for receive/map and ᐧ (true) for type assertion. The select marker ꓸꓸꓸ is `public static readonly NilType ꓸꓸꓸ = nil;` (builtin.cs:185-196), whose doc comment says the channel operation then returns a SelectOp case descriptor that registers the pending operation with the select.
ᐧᐧ: builtin.cs:162-174, `public static readonly bool ᐧᐧ = true;` for a leading constant-true case of a tagless switch; a foldable `when true` makes C# reject the later cases. The summary's Expression Switch section does not cover it, so the row links the reference: docs/ConversionStrategies-Reference/expression-switch.md:14.
Lookalike code points (python unicodedata): ꓸ U+A4F8 LISU LETTER TONE MYA TI; ᐧ U+1427 CANADIAN SYLLABICS FINAL MIDDLE DOT; ꟷ U+A7F7 LATIN EPIGRAPHIC LETTER SIDEWAYS I; ᴛ U+1D1B LATIN LETTER SMALL CAPITAL T. All are letter categories (Lm/Lo/Ll), so C# reads them as identifier characters.
ᶠ is FuncValueMarker (symbols.json:172-183): only an sstring-twinned function has it, because a twinned function has no single method group; an ordinary function value stays a plain method group (GoCallVariations.cs.target:24).
_ΔpN: src/go2cs/visitFuncDecl.go:1226-1240 and :2416-2424; :53-57 and :1654 (a lone blank parameter stays `_` unless the body uses a `_ =` discard, because a parameter named `_` captures C#'s discards); interface methods and function types name every unnamed parameter: AnonymousInterfaces.cs.target:51 `(nint, error) Read(slice<byte> _Δp0);` (Go AnonymousInterfaces.go:28 `interface{ Read([]byte) (int, error) }`), BlankIdentifierCollision/main.cs.target:39 `internal delegate stateFn stateFn(nint _Δp0);`; a lone `_` that stays: CaptureHoistThroughConversion/main.cs.target:7 `public delegate void Handler(nint _);` (a named `_`); several: GenericTypeInstantiation.cs.target:44.
Δio: the import alias `io` collides with the child namespace `go.io` (from io/fs) inside `namespace go`: src/go2cs/conversionDriver.go:449, src/go2cs/perUnitAliasRenames_test.go:57.
Other renames beyond `main` -> `Main`: a Go function named `Main` becomes `ΔMain` (src/go2cs/identifierNaming.go:249-257).
The ʗp box-forwarder: the [GoRecv] generator's ж-forwarder, src/gen/go2cs-gen/Templates/ReceiverMethod/ReceiverMethodTemplate.cs:69-73 and :118. -->

**A first example.** This Go program declares a struct type, a method with a pointer receiver, and a
function that takes a pointer and binds the method to a variable:

<!-- source: src/tests/Behavioral/GoCallVariations/GoCallVariations.go:42-56 (lines 48-52, a blank line and an explanatory comment, omitted) -->
```go
type accum struct{ total int }

func (a *accum) add(n int) int {
	a.total += n
	return a.total
}
…
func bindAdd(a *accum) {
	add := a.add
	fmt.Println("bound add:", add(5), add(7)) // 5 12
}
```
<!-- source: src/tests/Behavioral/GoCallVariations/GoCallVariations.cs.target:46-61 -->
```csharp
[GoType] partial struct accum {
    internal nint total;
}

[GoRecv] internal static nint add(this ref accum a, nint n) {
    a.total += n;
    return a.total;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object boundAddˢ = (@string)"bound add:"u8;

internal static void bindAdd(ж<accum> Ꮡa) {
    var add = (nint p1) => Ꮡa.add(p1);
    fmt.Println(boundAddˢ, add(5), add(7));
}
```

Read it piece by piece:

- All of this sits inside `partial class main_package`, the one static class that holds a Go package
  ([Package Conversion](#package-conversion)).
- `[GoType] partial struct accum` is the Go struct. A [source generator](#source-generators) completes the
  other half of the `partial` type: constructors, `==` and more ([Struct Types](#struct-types)).
- `nint` is Go's `int`: both are the size of a pointer ([Integer Types and Arithmetic](#integer-types-and-arithmetic)).
- The method becomes a static extension method. Its pointer receiver `(a *accum)` is written
  `this ref accum a`, so the method changes the caller's value ([Functions and Methods](#functions-and-methods)).
- `[GoRecv]` asks the source generator for a second overload that takes the box `ж<accum>` and forwards to
  the `ref` method. That overload is what makes `Ꮡa.add(p1)` work.
- `ж<accum> Ꮡa` is Go's `a *accum`. `ж<T>` (read "zhe") is golib's heap box for a Go pointer, and the `Ꮡ`
  prefix marks a name that holds one ([Pointers](#pointers)).
- `"bound add:"u8` is a C# UTF-8 byte literal. A Go string is UTF-8 bytes too, so golib's `@string` is built
  straight from it ([Strings](#strings-string-and-sstring)).
- `boundAddˢ` is that literal, created once in a static field. The comment's RODATA is Go's read-only data. It
  is stored already boxed as `object` because it only ever goes to `fmt.Println`'s `...any` parameter
  ([Empty Interface](#empty-interface-any)).
- The method value `a.add` becomes a lambda that calls `add` on `Ꮡa`. `p1` is a parameter name the converter
  makes up ([Function Values and Closures](#function-values-and-closures)).

The caller takes the address of a new `accum` and reads a field through the pointer:

<!-- source: src/tests/Behavioral/GoCallVariations/GoCallVariations.go:35-37 -->
```go
acc := &accum{}
bindAdd(acc)
fmt.Println("accum total:", acc.total) // 12
```
<!-- source: src/tests/Behavioral/GoCallVariations/GoCallVariations.cs.target:17 and :40-42 -->
```csharp
private static readonly object accumTotalˢ = (@string)"accum total:"u8;
…
    var acc = Ꮡ(new accum(nil));
    bindAdd(acc);
    fmt.Println(accumTotalˢ, (~acc).total);
```

`new accum(nil)` is the empty literal `accum{}`. Its `nil` is golib's `nil` value, and it selects the
generated constructor that builds the zero value. Elsewhere, Go's `nil` usually becomes C#'s `default!`
([Nil and Zero Values](#nil-and-zero-values)).

`Ꮡ(…)` is Go's `&`: it puts the value in a `ж<accum>` box. `~acc` reads through the pointer, as Go's `*acc`
does, and panics if it is nil ([Implicit Pointer Dereferencing](#implicit-pointer-dereferencing)).

**Some glyphs look like punctuation but are letters.** `ꓸ` (U+A4F8) is not a dot, `ᐧ` (U+1427) is not a
middle dot, `ꟷ` (U+A7F7) is not a dash, and `ᴛ` (U+1D1B) is a small capital T. C# reads each one as part of
a name, so `reflectꓸValue` is one identifier, not a member access.

**The common glyphs.** Each row names a glyph or naming pattern, what it means, real examples from converted
code, and the section that explains it in full.

| Glyph or pattern | Meaning | Example | Explained in |
|---|---|---|---|
| `ж<T>`, `StandardBox<T>` | Go pointer `*T`: a golib heap box (read "zhe"). `StandardBox<T>` is the class a new box is created as when it holds its own value; other box kinds point into existing storage, such as an array element, a struct field or native memory | `ж<accum> Ꮡa`, `new StandardBox<Outer>(default(Outer))` | [Pointers](#pointers) |
| `Ꮡ` | Address-of `&x`. As a prefix: a variable that holds a box, or a generated field reference that Go's `&s.f` uses | `Ꮡ(new accum(nil))`, `ᏑgOuter`, `Ꮡb.of(Buffer.Ꮡoff)` for `&b.off` | [Pointers](#pointers) |
| `~p` | Reads through a pointer, and panics on nil like Go's `*p` | `(~acc).total` | [Implicit Pointer Dereferencing](#implicit-pointer-dereferencing) |
| `Δ` prefix | A name renamed to avoid a clash with a golib name, a method, `Main` or a namespace | `ΔGoFrame`, `ΔMonth`, `ΔMain`, `using Δio = io_package;` (`go.io` is also a namespace) | The naming rules in this section |
| `Δ1` suffix | A second declaration of the same name: a variable that shadows an outer one, or a repeated `init` function | `hourΔ1`, `initΔ1` | [Shadowing](#short-variable-redeclaration-shadowing); repeated `init`: [Functions and Methods](#functions-and-methods) |
| `ʗ1` suffix | A copy of a variable, taken for the lambda that uses it | `var f1ʗ1 = f1;` | [Function Values and Closures](#function-values-and-closures) |
| `ʗp` suffix | The raw incoming parameter, when the body copies it into a local of the Go name: a variadic pack becomes a slice, or a value whose address is taken moves into a heap box with `heap(…)` | `params ꓸꓸꓸerror errsʗp`; `readOnlyParam(Rect rʗp)` then `ref var r = ref heap(rʗp, out var Ꮡr);` | [Slices and Arrays](#slices-and-arrays) (variadic); [Pointers](#pointers) (address taken, the same `heap(…)` pattern as a local) |
| `ᴛ` | A name the converter makes up: a temporary, a lambda parameter or an init helper | `ᴛ1`, `selᴛ2`, `initᴛsingle` | [Multi-Assignment](#multi-assignment-and-evaluation-order); init helpers: [Initialization Order](#package-level-variable-initialization-order) |
| `ˢ`, `ᶜ` suffix | A string literal (`ˢ`) or a local constant (`ᶜ`), stored once in a static field whose declaration sits in the same file | `firstˢ`, `fnAtoiᶜ` | [Strings](#strings-string-and-sstring) |
| `ꓸ` | A dot inside one name, such as a package-qualified alias | `reflectꓸValue` | [Type Aliasing](#type-aliasing) |
| `ꓸꓸꓸ` in a type or name | Go's `...`: `ꓸꓸꓸT` aliases `Span<T>` for a variadic parameter; `s.ꓸꓸꓸ` and `appendꓸꓸꓸ` spread a slice; `Funcꓸꓸꓸ<…>` is a variadic func type | `using ꓸꓸꓸerror = Span<error>;`, `fmt.Sprintf(format, a.ꓸꓸꓸ)`, `appendꓸꓸꓸ(all, batch)` | [Slices and Arrays](#slices-and-arrays) |
| `ꓸꓸꓸ` as an argument | A channel send or receive that registers with a `select` instead of running at once | `ᐸꟷ(selᴛ2, ꓸꓸꓸ)` | [Channels and `select`](#channels-and-select) |
| `ᐸꟷ`, `ꟷᐳ` | Channel send, receive, and receive inside `select` | `ch.ᐸꟷ(textˢ)`, `ᐸꟷ(done)`, `selᴛ2.ꟷᐳ(out var v)` | [Channels and `select`](#channels-and-select) |
| `/*<-*/` | A channel's direction, kept as a comment: `/*<-*/channel<T>` is `<-chan T`, `channel/*<-*/<T>` is `chan<- T` | `/*<-*/channel<nint> src` | [Channels and `select`](#channels-and-select) |
| `goǃ` | The `go` statement | `goǃ(ᴛ1 => fmt.Println(ᴛ1), firstˢ)` | [Goroutines](#goroutines) |
| `ꟷ` | golib's `const bool ꟷ = false`. As an extra argument it picks the overload that returns `(value, ok)`, for a comma-ok receive or map lookup; its value is not used | `var (v, ok) = ᐸꟷ(d, ꟷ);`, `var (_, before) = seen[memo, ꟷ];` | [Comma-Ok Forms](#multi-result-values-and-comma-ok-forms) |
| `ᐧ` | golib's `const bool ᐧ = true`. It is the `true` of a tagless switch or endless loop, and the extra argument of a comma-ok type assertion | `switch (ᐧ)`, `while (ᐧ)`, `err._<ж<NumError>>(ᐧ)` | [Expression Switch](#expression-switch-statements); [Comma-Ok Forms](#multi-result-values-and-comma-ok-forms) |
| `._<T>()` | Type assertion `x.(T)`; with `ᐧ`, its comma-ok form | `i._<@string>()`, `err._<ж<NumError>>(ᐧ)` | [Empty Interface (`any`)](#empty-interface-any) |
| `ᒐ` | The `GoFrame` local that runs a function's deferred calls; names that start with `ᒐ`, such as the label `ᒐdone`, belong to the same frame | `GoFrame ᒐ = default;`, `ᒐdone: return (@out, label);` | [Defer / Panic / Recover](#defer--panic--recover) |
| `break_L`, `continue_L` | `goto` targets for Go's labeled `break L` and `continue L` | `goto break_scan;`, `continue_scan:;` | [Loops, Range and Labels](#loops-range-and-labels) |
| `XжI`, `XᴠI` | An adapter class: pointer `*X` (`ж`) or value `X` (`ᴠ`) as interface `I` | `new joinErrorжerror(e)`, `new HandlerᴠIface(…)` | [Interfaces](#interfaces) |
| `default!` | Go `nil` (golib `nil` in pointer contexts), and the zero value of a variable declared without a value | `return (n, default!);` | [Nil and Zero Values](#nil-and-zero-values) |
| `[GoType]`, `[GoRecv]` | Mark a converted Go type, or a pointer-receiver method; a source generator completes it | `[GoType] partial struct accum`, `[GoRecv] internal static nint len(this ref box b)` | [Source Generators](#source-generators) |
| `nint`, `nuint` | Go `int`, `uint` | `internal nint total;` | [Integer Types and Arithmetic](#integer-types-and-arithmetic) |
| `@name` | A C# keyword used as a name | `@in`, `@unsafe`, `@string` | The naming rules in this section |
| `<pkg>_package` | The static partial class that holds a package | `partial class strings_package` | [Package Conversion](#package-conversion) |

**Less common names.** These appear in specific kinds of code. A first read can skip them.

| Glyph or pattern | Meaning | Example | Explained in |
|---|---|---|---|
| `ᶠ` suffix | A shared delegate, used when a function that has an `sstring` twin is taken as a value. The twin is a second overload that takes golib's `sstring`, a string view that allocates nothing | `fmt.Sprintfᶠ` | [Strings](#strings-string-and-sstring) |
| `ᐧᐧ` | A `true` that C# does not treat as a constant (a `static readonly` field). It marks a leading `case true:` in a tagless switch, so C# still accepts the cases after it | `case {} when ᐧᐧ:` | [Reference → Expression Switch](ConversionStrategies-Reference/expression-switch.md#a-leading-constant-true-case-stays-opaque-to-the-compiler) |
| `ᴋ` | A temporary that keeps a pointer passed to a system call alive until the call returns | `var ᴋ0 = @unsafe.SliceData(p);` | [`unsafe.Pointer` and `uintptr`](#unsafepointer-and-uintptr) |
| `ᴅ`, `ᴺ` suffix | Keep a Go type name. `ᴅ` is an empty C# marker interface, never implemented, whose `[GoLocalName]` attribute holds the Go name of a type converted to a `using` alias. `ᴺ` is an extra type parameter for a type argument's name | `[GoLocalName("Token")] public interface Tokenᴅ { }`, `nameOf<T, Tᴺ>` | [Reflection](#reflection-reflect) |
| `_Δp0` | A made-up name for an unnamed or `_` parameter where a plain `_` would not work: C# forbids two parameters with the same name, and a parameter named `_` would capture the body's own `_ =` discards. Interface methods and function types name every unnamed parameter this way. Otherwise a lone blank parameter stays `_` | `Read(slice<byte> _Δp0)`, `Seq2Like<K, V>(K _Δp0, V _Δp1)` | The naming rules in this section |
| `Set‿` | The map write that a nested assignment `m[k1][k2] = v` calls on the element `m[k1]`, used only when the element's Go type declares a method named `Set` or is itself named `Set`. A Go method becomes an extension method, and C# runs a same-named member of the generated wrapper in its place, so the wrapper gives up that name: `Set` is dropped, and `Add`, `Remove`, `Clear`, `ContainsKey`, `TryGetValue` and a channel's `Send` and `Sent` become explicit interface implementations. `‿` (U+203F) is connector punctuation, legal in a C# name and impossible in a Go one | `envs["x"u8].Set‿(mixedˢ, "m"u8);` | [Source Generators](#source-generators) |
| `<func>_<type>` | A type declared inside a function, lifted to package scope | `main_point` | [Struct Types](#struct-types) |
| `<pkg>_internal_test_package`, `<pkg>_test_package` | The classes for in-package test files and for the external `_test` package | `strings_internal_test_package` | [Converted Tests](#converted-tests) |

**A C# keyword is escaped with `@`.** Go names such as `in`, `base` and `unsafe` are C# keywords. The `@`
prefix lets C# use them as names, so `import "unsafe"` becomes `using @unsafe = unsafe_package;`. Here a
struct field named `in` becomes `@in`:

<!-- source: src/tests/Behavioral/UnsafeOperations/UnsafeOperations.go:21-24 -->
```go
type Outer struct {
	head byte
	in   Inner
}
```
<!-- source: src/tests/Behavioral/UnsafeOperations/UnsafeOperations.cs.target:21-24 -->
```csharp
[GoType] partial struct Outer {
    internal byte head;
    internal Inner @in;
}
```

**A name that would clash gets a `Δ` prefix.** The converted code relies on golib names such as `GoFrame`,
`builtin` and `slice`. A Go name that matches one is renamed, so golib's own name still means golib's type.
For example, `GoFrame` is the golib type of the local that runs a function's deferred calls
([Defer / Panic / Recover](#defer--panic--recover)), so a Go struct named `GoFrame` becomes `ΔGoFrame`:

<!-- reserved-name list: src/go2cs/identifierNaming.go:87-94; type-vs-method rename: src/core/time/time.cs:324 `[GoType("num:nint")] partial struct ΔMonth;` (Go type Month and method Time.Month); the same test's deferInShadowedPackage (ReservedNameShadows/main.cs.target:43-55) declares golib's `GoFrame ᒐ` beside the renamed `ΔGoFrame` local type -->
<!-- source: src/tests/Behavioral/ReservedNameShadows/main.go:45 -->
```go
type GoFrame struct{ k int }
```
<!-- source: src/tests/Behavioral/ReservedNameShadows/main.cs.target:39-41 -->
```csharp
[GoType] partial struct ΔGoFrame {
    internal nint k;
}
```

A Go type that shares its name with a method is renamed the same way. For example, the time package's type
`Month` becomes `ΔMonth`, because `Time` also has a method named `Month`. A Go function named `Main` becomes
`ΔMain`, since C# reserves `Main` for the entry point.

**A capitalized Go name becomes `public`; any other name becomes `internal`.** This is Go's export rule in C#
terms. Lowercase `accum`, its field `total` and its method `add` are all `internal`, while the exported
function `GetPrintLn` is `public`. Its result type, Go's `func(string)`, becomes the C# delegate
`Action<@string>` ([Function Values and Closures](#function-values-and-closures)):

<!-- source: src/tests/Behavioral/GoCallVariations/GoCallVariations.go:58-62 -->
```go
func GetPrintLn() func(string) {
	return func(src string) {
		fmt.Println(src)
	}
}
```
<!-- source: src/tests/Behavioral/GoCallVariations/GoCallVariations.cs.target:63-67 -->
```csharp
public static Action<@string> GetPrintLn() {
    return (@string src) => {
        fmt.Println(src);
    };
}
```

A method is `public` only when both its own name and its receiver type's name are exported. A package-level
struct such as `accum` shows no modifier, because the [source generator](#source-generators) adds it on the
generated half of the `partial` type.

<!-- receiver clamp: src/go2cs/visitFuncDecl.go:730-738 (receiverAccess from :1654-1671; getAccess at src/go2cs/identifierNaming.go:262); example src/core/errors/join.cs:46 `[GoRecv] internal static @string Error(this ref joinError e) {` -->
<!-- generator adds the modifier: src/gen/go2cs-gen/Templates/StructType/StructTypeTemplate.cs:55, Templates/InterfaceType/InterfaceTypeTemplate.cs:50 (`{{Scope}} partial struct/interface`). "usually": lifted local types carry `internal` (LiftedLocalTypes/main.cs.target:37) and an unexported type an exported field exposes carries `public` (PublicizedFieldType/main.cs.target:9); package-level declarations otherwise emit `[GoType] partial` bare (about 1150 of about 1280 [GoType] lines in the behavioral goldens). A named function type becomes a delegate that carries its own modifier: src/tests/Behavioral/CaptureHoistThroughConversion/main.cs.target:7 `public delegate void Handler(nint _);` (Go main.go:23 `type Handler func(int)`); GenericTypeInstantiation.cs.target:44 `public delegate bool Seq2Like<K, V>(K _Δp0, V _Δp1);` -->
<!-- new accum(nil): the generated NilType constructor, src/gen/go2cs-gen/Templates/StructType/StructTypeTemplate.cs:1410; "an empty named-type literal passes nil" is the Struct Types section's rule. Section length (about 200 lines) exceeds the 120-line guide because the owner's review asks each section to stand alone: the worked example defines every glyph it uses before the tables, and the glyph index is split into common and less-common tables so a first read can stop at the first. -->

<!-- The glyph tables' intended reference home is a names-and-glyphs reference page (naming.md), which does not exist yet; until it lands, this section links shadowing.md, which is also the Shadowing section's reference page. -->
**Full detail:** [Reference → Shadowing the names go2cs itself spells](ConversionStrategies-Reference/shadowing.md#shadowing-the-names-go2cs-itself-spells-nil-golib-names-emitter-spelled-type-names-c-keywords) —
which Go names are `@`-escaped or `Δ`-renamed to avoid a clash with C# keywords or emitted names, and why.

---

<a id="the-gogolib-support-namespace"></a>

## The golib Runtime Library

Go's built-in types and behaviors become C# types and functions in [golib](../src/core/golib/). golib is the
hand-written runtime library behind every converted project. go2cs never generates it. Every converted
project references it, and it is the same library for every program.

**The project file go2cs writes adds golib for you.** By default it adds golib as a project reference to
the local source tree. A conversion run with `-recurse=nuget` references the published NuGet packages
instead, and golib arrives as the package `go.lib`
([Compiled Library versus Source Code](#compiled-library-versus-source-code)).

Converted code uses golib's names everywhere, such as `slice<T>`, `@string`, `len` and `nil`. Some of them
carry a glyph, a special character such as `ж` or `Δ`.
[Reading Converted Code: Names and Glyphs](#reading-converted-code-names-and-glyphs) lists every glyph.

**Go built-ins that .NET has no match for have golib counterparts.** This table names each one and links
the section that explains it.

| Go | golib | Section |
|---|---|---|
| `[]T`, `[N]T` | `slice<T>`, `array<T>` | [Slices and Arrays](#slices-and-arrays) |
| `string` | `@string`, an immutable byte string; `sstring`, its stack-only form | [Strings](#strings-string-and-sstring) |
| `map[K]V` | `map<K, V>` | [Maps](#maps) |
| `chan T` | `channel<T>` | [Channels and `select`](#channels-and-select) |
| `*T` | `ж<T>`, a heap box that a pointer refers to | [Pointers](#pointers) |
| `nil` | `nil`, a `NilType` value | [Nil and Zero Values](#nil-and-zero-values) |
| `error` | the `error` interface | [Interfaces](#interfaces) |
| `uintptr` | `uintptr`, a golib struct that wraps `nuint` | [`unsafe.Pointer` and `uintptr`](#unsafepointer-and-uintptr) |
| `len`, `cap`, `append`, `copy`, `delete`, `close`, `clear`, `min`, `max` | methods of the `builtin` class | [Built-in Functions](#built-in-functions) |
| `defer`, `panic`, `recover` | `defer`, `panic`, `recover` in `builtin` | [Defer / Panic / Recover](#defer--panic--recover) |
| `go f(x)` | `goǃ(f, x)` in `builtin` | [Goroutines](#goroutines) |

The `ǃ` in `goǃ` is a letter that looks like `!`. It keeps the method's name apart from `go`, the C#
namespace every converted file lives in.

**A small program shows golib at work.** This Go program makes a buffered channel and prints its length
and capacity:

<!-- source: src/tests/Behavioral/ChannelCapLen/main.go:6-41 -->
```go
package main

import "fmt"

func main() {
	ch := make(chan int, 3)
	fmt.Println(len(ch), cap(ch))
	…
}
```
<!-- source: src/tests/Behavioral/ChannelCapLen/main.cs.target:1-37 -->
```csharp
namespace go;

using fmt = fmt_package;

partial class main_package {

internal static void Main() {
    var ch = new channel<nint>(3);
    fmt.Println(len(ch), cap(ch));
    …
}

} // end main_package
```

Both versions print `0 3`: the new channel holds nothing yet and has room for 3 values.

`make(chan int, 3)` becomes a golib `channel<nint>` with a buffer of 3. `nint` is C#'s native-size integer,
the match for Go's `int` ([Integer Types and Arithmetic](#integer-types-and-arithmetic)). The Go package
becomes the C# class `main_package`, and `fmt` is an alias for the converted `fmt` package's class
([Package Conversion](#package-conversion)). The file has no `using` line for golib. The next two rules
say why.

**Built-in functions keep their Go names.** The project file go2cs writes imports golib's
[`builtin`](../src/core/golib/builtin.cs) class with `using static`. So a call such as `len(ch)` reads as it
does in Go. The same project file also maps Go's sized numeric names, and `any`, to .NET types:

<!-- source: src/tests/Behavioral/ChannelCapLen/ChannelCapLen.csproj:101-116 -->
```xml
    <Using Include="go.builtin" Static="True" />
…
    <Using Include="System.Object" Alias="any" />
    <Using Include="System.Byte" Alias="uint8" />
…
    <Using Include="System.Int32" Alias="int32" />
…
    <Using Include="System.Double" Alias="float64" />
…
    <Using Include="System.Int32" Alias="rune" />
```

**golib's Go-facing types sit directly in the `go` namespace.** Every converted package lives in `go` or in
a namespace nested inside it, such as `go.crypto`. So converted code names `slice<T>`, `channel<T>` or
`@string` with no `using` line.

**golib's internal helpers live one level down, in `go.golib`.** The library is named golib, and so is
this one sub-namespace. It holds runtime helpers that have no Go name, such as
[`SparseArray<T>`](../src/core/golib/runtime/SparseArray.cs). Code in `namespace go` reaches a helper as
`golib.<name>`, again with no `using` line.

`golib.SparseArray<T>` builds a slice from a literal keyed by index. The keys may skip positions:

<!-- source: src/tests/Behavioral/SparseArrayNamedIntKey/main.go:48-79 -->
```go
type kindT uint

var kindNames = []string{
	kindT(1): "one",
	kindT(3): "three",
}
…
	fmt.Println(kindNames[kindT(3)], len(kindNames)) // three 4
```
<!-- source: src/tests/Behavioral/SparseArrayNamedIntKey/main.cs.target:31-57 -->
```csharp
[GoType("num:nuint")] partial struct kindT;

internal static slice<@string> kindNames = new golib.SparseArray<@string>{
    [1] = "one"u8,
    [3] = "three"u8
}.slice();
…
    fmt.Println(kindNames[((kindT)3)], len(kindNames));
```

`.slice()` turns the result into a golib `slice<@string>`. Positions 0 and 2 hold the empty string, so the
length is 4, as in Go. Each `"…"u8` is a C# UTF-8 literal; `@string` converts it implicitly
([Strings](#strings-string-and-sstring)).

The other names in this example come from other sections. Go's `type kindT uint` becomes a struct marked
`[GoType("num:nuint")]`. That tells a source generator to fill in a numeric type backed by `nuint`, with its
operators and conversions ([Named Numeric Types and Constant Contexts](#named-numeric-types-and-constant-contexts)).
The literal's keys are constants, so `kindT(1)` becomes the plain index `1`. The index expression keeps
Go's `kindT(3)` as a cast. `kindT` converts implicitly to its underlying `nuint`, so it indexes the slice as
Go's `uint` does.

**An import alias sometimes takes the `Δ` rename mark.** `Δ` marks a name that go2cs changed to avoid a
clash in C#. A package's sub-packages live in a C# namespace named after it: `math/bits` lives in the
namespace `go.math`. Go's `math` package itself imports `math/bits`, so a program that imports `math` always
has `go.math` in view. Inside `namespace go`, the bare name `math` already means that namespace, so an alias
named `math` would clash. The alias becomes `Δmath`:

<!-- source: src/tests/Behavioral/MathFloatBits/main.go:3-12 -->
```go
import (
	"fmt"
	"math"
)
…
	fmt.Println(math.Float64bits(z))
```
<!-- source: src/tests/Behavioral/MathFloatBits/main.cs.target:1-11 -->
```csharp
namespace go;

using fmt = fmt_package;
using Δmath = math_package;
…
    fmt.Println(Δmath.Float64bits(z));
```

A file in a nested namespace, such as `go.crypto`, keeps the plain `using math = math_package;`. There C#
finds the file's own aliases before it looks in the outer `go` namespace, so no clash arises. The helper
namespace `go.golib` never causes this clash, because no Go package is named `golib`.

**Full detail:** [Reference → The go.golib support namespace](ConversionStrategies-Reference/golib-namespace.md#the-gogolib-support-namespace) — why golib's helpers avoid Go package names, exactly when an import alias takes the `Δ` rename mark, and how renamed types and aliases from other packages are spelled.

---

## Package Conversion

A Go package becomes one C# project. All of the package's code lives in one static class named
`<name>_package`, so package `registry` becomes `registry_package`. Each Go file becomes one `.cs` file
that adds its part to that class, which is why the class is `partial`.

The examples in this section come from one small test program. Package `main` imports a package named
`registry` and calls it. It also imports two packages, `pnglike` and `jpeglike`, with Go's blank import
`_`, only so that their `init` functions run.

**Package-level declarations become static members of the package class.** A function becomes a static
method, and a package variable becomes a static field. An exported (capitalized) name is `public`; any
other name is `internal`. Go methods become C# extension methods, shown in
[Functions and Methods](#functions-and-methods).

<!-- source: src/tests/Behavioral/BlankImportSideEffects/registry/registry.go:5-23 -->
```go
package registry

var decoders = map[string]string{}

// Register records a decoder. Called only from another package's init.
func Register(name, describe string) {
	decoders[name] = describe
}

// Lookup reports the decoder registered under name, if any.
func Lookup(name string) (string, bool) {
	describe, ok := decoders[name]
	return describe, ok
}

// Count reports how many decoders have registered so far.
func Count() int {
	return len(decoders)
}
```
<!-- source: src/tests/Behavioral/BlankImportSideEffects/registry/registry.cs:1-20 -->
```csharp
namespace go.BlankImportSideEffects;

partial class registry_package {

internal static map<@string, @string> decoders = new map<@string, @string>{};

public static void Register(@string name, @string describe) {
    decoders[name] = describe;
}

public static (@string, bool) Lookup(@string name) {
    var (describe, ok) = decoders[name, ꟷ];
    return (describe, ok);
}

public static nint Count() {
    return len(decoders);
}

} // end registry_package
```

Each file's part leaves off `public static`. The full class declaration, with those modifiers, is in the
project's generated `package_info.cs`, described later in this section.

The types come from golib, go2cs's hand-written C# runtime library
([The golib Runtime Library](#the-golib-runtime-library)). `@string` is Go's `string`
([Strings](#strings-string-and-sstring)), `map<K, V>` is Go's map ([Maps](#maps)), and `nint` is Go's
`int` ([Integer Types and Arithmetic](#integer-types-and-arithmetic)). `len` is golib's version of Go's
built-in ([Built-in Functions](#built-in-functions)).

`decoders[name, ꟷ]` is Go's comma-ok lookup. `ꟷ` is a golib marker constant that only asks for the
`(value, ok)` pair, and a Go multi-result becomes a C# tuple
([Multi-Result Values and Comma-Ok Forms](#multi-result-values-and-comma-ok-forms)). The Go comments are
missing because this test converts without `-comments` ([Comments](#comments)).

**The import path picks the namespace.** The path's leading segments become a namespace under the root
namespace `go`. The class takes the Go package name, which is almost always the path's last segment. So
`BlankImportSideEffects/registry` is `registry_package` in `namespace go.BlankImportSideEffects`. A
one-segment path sits directly in `namespace go`. That covers `fmt`, and also this test's `main` package,
whose import path is just the module name `BlankImportSideEffects`.

The standard library follows the same rule. `unicode/utf8` is `utf8_package` in `namespace go.unicode`.
`math/rand/v2` declares package `rand`, so it is `rand_package` in `namespace go.math.rand`.
<!-- sources: src/core/bufio/bufio.cs:14-15 (using utf8 = unicode.utf8_package; using unicode;); src/core/unicode/utf8/package_info.cs:63,66; src/core/math/rand/v2/package_info.cs:65,68; src/core/fmt/print.cs:4,18 -->

**An import becomes a `using` alias named for the package.** Converted code keeps calling
`registry.Count()`, exactly as in Go. The importer also opens the package's namespace
(`using BlankImportSideEffects;`), because C# finds extension methods, and so Go methods, only through an
open namespace. A blank import emits only a comment, since a `using _` alias would take over C#'s discard `_`:

<!-- source: src/tests/Behavioral/BlankImportSideEffects/main.go:10-18 -->
```go
package main

import (
	"fmt"

	_ "BlankImportSideEffects/jpeglike"
	_ "BlankImportSideEffects/pnglike"
	"BlankImportSideEffects/registry"
)
```
<!-- source: src/tests/Behavioral/BlankImportSideEffects/main.cs.target:1-9 -->
```csharp
namespace go;

using fmt = fmt_package;
// blank import: BlankImportSideEffects.jpeglike_package (side effects only; no using emitted — a `using _` alias hijacks C# discards)
// blank import: BlankImportSideEffects.pnglike_package (side effects only; no using emitted — a `using _` alias hijacks C# discards)
using registry = BlankImportSideEffects.registry_package;
using BlankImportSideEffects;

partial class main_package {
```

`main` sits in `namespace go`, so it can leave the `go.` prefix off `BlankImportSideEffects`. Other
files may spell it out as `go.BlankImportSideEffects`; both name the same namespace.

**`init` becomes a `[GoInit]` method, and `main` becomes `Main`.** `[GoInit]` is the project's alias for
C#'s `[ModuleInitializer]`, which .NET runs once, before the first use of anything in its assembly. The
rest of `main.cs` continues the `main_package` class opened in the previous example:

<!-- source: src/tests/Behavioral/BlankImportSideEffects/main.go:20-42 -->
```go
// countAtInit records what the registry held when THIS package's own init ran. Go orders an
// imported package's initialization before the importer's, blank imports included, so it is 2.
var countAtInit int

func init() {
	countAtInit = registry.Count()
}

func main() {
	fmt.Println("count at init:", countAtInit)

	// "giflike" is the negative control: never registered, so a lookup that "succeeds" for
	// everything would not prove anything.
	for _, name := range []string{"jpeglike", "pnglike", "giflike"} {
		if describe, ok := registry.Lookup(name); ok {
			fmt.Println("found:", name, "=>", describe)
		} else {
			fmt.Println("missing:", name)
		}
	}

	fmt.Println("count:", registry.Count())
}
```
<!-- source: src/tests/Behavioral/BlankImportSideEffects/main.cs.target:11-37 -->
```csharp
internal static nint countAtInit;

[GoInit] internal static void init() {
    countAtInit = registry.Count();
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object countAtInitˢ = (@string)"count at init:"u8;
private static readonly object foundˢ = (@string)"found:"u8;
private static readonly object missingˢ = (@string)"missing:"u8;
private static readonly object countˢ = (@string)"count:"u8;

internal static void Main() {
    fmt.Println(countAtInitˢ, countAtInit);
    foreach (var (_, name) in new @string[]{"jpeglike"u8, "pnglike"u8, "giflike"u8}.slice()) {
        {
            var (describe, ok) = registry.Lookup(name); if (ok){
                fmt.Println(foundˢ, name, (@string)"=>"u8, describe);
            } else {
                fmt.Println(missingˢ, name);
            }
        }
    }
    fmt.Println(countˢ, registry.Count());
}

} // end main_package
```

A Go string literal becomes a C# UTF-8 literal such as `"jpeglike"u8`, which converts to `@string`. The
names ending in `ˢ` are string literals the converter creates once, as `static readonly` fields. They
are typed `object` because `fmt.Println` takes `any` arguments
([Empty Interface](#empty-interface-any)), and `(@string)` makes each literal an `@string` before it is
stored. `"=>"` stays inline because a hoisted field is named after the literal's letters and digits, and
`"=>"` has none. Other literals stay inline where a field would not help; see
[Strings](#strings-string-and-sstring).

The `range` loop becomes a `foreach` ([Loops, Range and Labels](#loops-range-and-labels)). `.slice()`
turns the array into a golib slice, and enumerating it yields `(index, value)` pairs, as Go's `range`
does ([Slices and Arrays](#slices-and-arrays)). The extra braces around the `if` hold its init
statement's variables in their own scope, as Go does.

**Imported packages initialize first, blank imports included.** Go runs every imported package's `init`
before the importer's own. A `[GoInit]` method runs only when something in its assembly is first used.
That can come too late, and for a blank import it never comes, because nothing names the package.
`pnglike` does all of its work in `init`:

<!-- source: src/tests/Behavioral/BlankImportSideEffects/pnglike/pnglike.go:4-10 -->
```go
package pnglike

import "BlankImportSideEffects/registry"

func init() {
	registry.Register("pnglike", "pnglike decoder")
}
```
<!-- source: src/tests/Behavioral/BlankImportSideEffects/pnglike/pnglike.cs:1-12 -->
```csharp
namespace go.BlankImportSideEffects;

using registry = go.BlankImportSideEffects.registry_package;
using go.BlankImportSideEffects;

partial class pnglike_package {

[GoInit] internal static void init() {
    registry.Register("pnglike"u8, "pnglike decoder"u8);
}

} // end pnglike_package
```

**Each project also gets a generated `package_info.cs`.** It declares the package class itself, as
`public static partial class`, with `[GoPackage]` recording the Go package name. It also holds one
`[GoInit]` hook for each import that has anything to initialize, directly or through its own imports.
Each hook calls `builtin.initPackage`, which runs the imported package's `[GoInit]` methods if they have
not run yet:

<!-- source: src/tests/Behavioral/BlankImportSideEffects/package_info.cs:55-82 -->
```csharp
namespace go;

[GoPackage("main")]
[GoTestMatchingConsoleOutput]
public static partial class main_package
{
…
    // <ImportInitializers>
    [GoInit] internal static void initᴛᴛimportꓸBlankImportSideEffectsꓸjpeglike() => builtin.initPackage(typeof(BlankImportSideEffects.jpeglike_package));
    [GoInit] internal static void initᴛᴛimportꓸBlankImportSideEffectsꓸpnglike() => builtin.initPackage(typeof(BlankImportSideEffects.pnglike_package));
    [GoInit] internal static void initᴛᴛimportꓸBlankImportSideEffectsꓸregistry() => builtin.initPackage(typeof(BlankImportSideEffects.registry_package));
    [GoInit] internal static void initᴛᴛimportꓸfmt() => builtin.initPackage(typeof(fmt_package));
    // </ImportInitializers>
}
```

The hooks must run before the package's own `init`. C# runs an assembly's module initializers in the
order its files are compiled, and the project compiles `package_info.cs` first. So every import is
initialized before `main`'s `init` runs, and the program prints `count at init: 2`, just as Go does.
`builtin.initPackage` does not run the imported package's variable initializers; those still run the
first time the package class is used.

Each hook name reads as "init, import, path". `ᴛ` marks a name the converter makes up, doubled here to
keep these hooks apart from its other init helpers, and `ꓸ` stands in for the `/` of the import path
([Names and Glyphs](#reading-converted-code-names-and-glyphs)). `[GoTestMatchingConsoleOutput]` appears
only in tests; it marks a program whose output is compared with Go's. `package_info.cs` also holds
package-wide `global using` aliases for [Type Aliasing](#type-aliasing), and the attributes that drive
the [Source Generators](#source-generators).

**A `main` package becomes an executable; every other package becomes a library.** The `main` project
has `<OutputType>Exe</OutputType>`, and `registry`'s has `<OutputType>Library</OutputType>`. Each
imported package is its own project, and the importer references it. The project file also defines the
`GoInit` alias and compiles `package_info.cs` ahead of the other files:

<!-- source: src/tests/Behavioral/BlankImportSideEffects/BlankImportSideEffects.csproj:4-153 -->
```xml
    <OutputType>Exe</OutputType>
…
    <Using Include="System.Runtime.CompilerServices.ModuleInitializerAttribute" Alias="GoInitAttribute" />
…
    <Compile Include="package_info.cs" Condition="Exists('package_info.cs')" />
    <!-- Include only .cs files from current folder -->
    <Compile Include="*.cs" Exclude="package_info.cs" />
…
    <ProjectReference Include="$(go2csPath)core/golib/golib.csproj" />

    <ProjectReference Include="$(go2csPath)core/fmt/fmt.csproj" />
    <ProjectReference Include="jpeglike/BlankImportSideEffects.jpeglike.csproj" />
    <ProjectReference Include="pnglike/BlankImportSideEffects.pnglike.csproj" />
    <ProjectReference Include="registry/BlankImportSideEffects.registry.csproj" />
```
<!-- source: src/tests/Behavioral/BlankImportSideEffects/registry/BlankImportSideEffects.registry.csproj:4 (<OutputType>Library</OutputType>) -->

`$(go2csPath)` is where go2cs's runtime library ([golib](#the-golib-runtime-library)) and converted
standard library live. The standard library can also come from NuGet
([Compiled Library versus Source Code](#compiled-library-versus-source-code)).

**Build constraints pick each target's Go files.** The converter selects Go files by their build
constraints and file-name suffixes, as `go build` does. The standard library is converted for Windows,
Linux and macOS, each on amd64. Files whose C# differs between them go into `windows/`, `linux/` and
`darwin/` subfolders. For example, `os` keeps `dir_windows.cs` in `os/windows/` and `dirent_linux.cs` in
`os/linux/`.
<!-- source: docs/ConversionStrategies-Reference/package-conversion.md:373 (-platforms windows/amd64,linux/amd64,darwin/amd64) -->

The project compiles its flat files plus the one folder named by the `GoTargetOS` build property. It
defaults to `windows`; a build sets another target with, for example, `-p:GoTargetOS=linux`. A package
with OS subfolders keeps a `package_info.cs` in each one, and the project compiles the chosen target's
copy first. The `Exists` conditions let one project file serve both layouts:

<!-- source: src/core/os/os.csproj:140-164 -->
```xml
  <PropertyGroup Condition="'$(GoTargetOS)'==''">
    <GoTargetOS>windows</GoTargetOS>
  </PropertyGroup>
…
    <Compile Include="package_info.cs" Condition="Exists('package_info.cs')" />
    <Compile Include="$(GoTargetOS)/package_info.cs" Condition="Exists('$(GoTargetOS)/package_info.cs')" />
    <!-- Include only .cs files from current folder -->
    <Compile Include="*.cs" Exclude="package_info.cs" />
…
    <Compile Include="$(GoTargetOS)/*.cs" Exclude="$(GoTargetOS)/package_info.cs" />
```

**Three C# warnings that Go's own semantics draw are turned off per file, by an `.editorconfig` beside the
project.** Go folds `unsafe.Sizeof(x)` and a constant expression over a local const at compile time, and
the converter writes the folded value with the Go text in a comment, so a local whose every use folded is
declared and never read in C# (CS0219). A package variable with no initializer that no file of the package
writes, or a blank `var _ T`, is a field never assigned (CS0649); C# reports that on an internal field only
in a project that grants no `InternalsVisibleTo`, so a package with in-package tests holds none. And
`uint64(nsec) | x` with a signed `nsec` sign-extends in Go exactly as the C# cast does (CS0675). Changing
the emission to silence these would make it say something other than the Go, so the conversion derives
each fact from the Go source and writes one section per file that holds it, anchored with a leading slash
and naming the per-OS folder where the file lives in one. A section switches the warning off for that one file and nowhere else, which also
hides a genuine new instance in that file; that is the cost, and why the entries are per file rather than
a project-wide `NoWarn`. The entries follow the code: a conversion that no longer finds a fact drops its
section, and deletes the file when none is left. The file is the converter's only when its first line is
the go2cs marker; a package's own `.editorconfig` is never touched (the conversion prints a warning naming
the entries it would have written).

```ini
# go2cs: per-file warning entries, written by every conversion of this package -- do not edit
# Each section turns off one C# warning in one converted file, for a reason the converter
# derived from the Go source; the comment above it names the GOOS flavours that hold it.

[/linux/runtime1.cs]
# CS0219: linux
dotnet_diagnostic.CS0219.severity = none
```

**Full detail:** [Reference → Package Conversion](ConversionStrategies-Reference/package-conversion.md#package-conversion) — project names and paths, cross-package references and NuGet use, exported type aliases, the import-initialization rules including blank imports and test projects, build-constraint file selection, the per-OS layout including hand-written files, and the generated solution files.

---

## Package-Level Variable Initialization Order

Go initializes package-level vars in dependency order: a var is set only after every var it reads. C# sets
static fields in a different order. So go2cs moves the few vars where the two orders differ into a
generated static constructor. Every other package var keeps its plain, readable field initializer.

**The problem: C# runs static field initializers in file order.** A Go package becomes one C#
`partial class` named `<name>_package`, with one C# file per Go file ([Package Conversion](#package-conversion)).
Within one file, C# runs static field initializers top to bottom. Across the files of a partial class,
the order is undefined. A field that reads another field too early sees its zero value, or `null`.

**A var that reads a var declared later in its file moves.** It becomes a bare field plus an init method
named `initᴛ<name>`, placed right beside it. The package's generated static constructor calls that method,
in Go's order. Here `first` reads `base`, declared after it, so only `first` moves. Left as a field
initializer, `first` would read `base` as 0 and be 1, not 42.

<!-- source: src/tests/Behavioral/PackageVarInitOrder/main.go:8-10 -->
```go
var first = base + 1

var base = 41
```
<!-- source: src/tests/Behavioral/PackageVarInitOrder/main.cs.target:7-10 -->
```csharp
internal static nint first;
internal static void initᴛfirst() { first = @base + 1; }

internal static nint @base = 41;
```

In the C#, `nint` is Go's `int` ([Integer Types and Arithmetic](#integer-types-and-arithmetic)). The `ᴛ`
glyph marks a name the converter generates, so it never clashes with a Go name. `@base` is `base` with
C#'s `@` escape, because `base` is a C# keyword ([Names and Glyphs](#reading-converted-code-names-and-glyphs)).

**A var that reads a var from another file moves.** C# gives no order between files, so go2cs never
relies on one. Here `entryName`, in `entries.go`, calls a method on `registry`, declared in `registry.go`.
So `entryName` moves, and `registry` stays a plain field initializer. `newReg` returns a new `*reg`, and
the method `add` records a name in it.

<!-- source: src/tests/Behavioral/PackageVarInitOrder/entries.go:8 -->
```go
var entryName = registry.add("alpha")
```
<!-- source: src/tests/Behavioral/PackageVarInitOrder/registry.go:18 -->
```go
var registry = newReg()
```
<!-- source: src/tests/Behavioral/PackageVarInitOrder/entries.cs.target:5-6 -->
```csharp
internal static @string entryName;
internal static void initᴛentryName() { entryName = registry.add("alpha"u8); }
```
<!-- source: src/tests/Behavioral/PackageVarInitOrder/registry.cs.target:20 -->
```csharp
internal static ж<reg> registry = newReg();
```

Without the move, `entries.cs` could run first. `add` would then run on a still-`nil` `registry` and
fail with a nil dereference while the class initializes. `@string` is golib's Go string type, and
`"alpha"u8` is a UTF-8 string literal ([Strings](#strings-string-and-sstring)). `ж<reg>` is golib's heap
box, the C# form of Go's `*reg` ([Pointers](#pointers)).

**A var that reads a moved var moves too.** A moved var gets its value only in the static constructor,
after every field initializer has run. So `entryUpper` moves because it reads `entryName`, even though
`entryName` is declared above it in the same file.

<!-- source: src/tests/Behavioral/PackageVarInitOrder/entries.go:13 -->
```go
var entryUpper = entryName + "!"
```
<!-- source: src/tests/Behavioral/PackageVarInitOrder/entries.cs.target:8-9 -->
```csharp
internal static @string entryUpper;
internal static void initᴛentryUpper() { entryUpper = entryName + "!"u8; }
```

**A read inside a called function counts too.** Go traces dependencies through the functions an
initializer calls, and go2cs does the same. `stdinName` calls `describe`, and `describe` reads `names`,
a var declared in `registry.go`. So `stdinName` moves, while `names` keeps its field initializer.

<!-- source: src/tests/Behavioral/PackageVarInitOrder/main.go:15-17 -->
```go
func describe(n int) string {
	return names[n]
}
```
<!-- source: src/tests/Behavioral/PackageVarInitOrder/registry.go:20 -->
```go
var names = []string{"stdin", "stdout"}
```
<!-- source: src/tests/Behavioral/PackageVarInitOrder/entries.go:17 -->
```go
var stdinName = describe(0)
```
<!-- source: src/tests/Behavioral/PackageVarInitOrder/entries.cs.target:11-12 -->
```csharp
internal static @string stdinName;
internal static void initᴛstdinName() { stdinName = describe(0); }
```
<!-- source: src/tests/Behavioral/PackageVarInitOrder/registry.cs.target:22 -->
```csharp
internal static slice<@string> names = new @string[]{"stdin"u8, "stdout"u8}.slice();
```

`slice<@string>` is golib's form of Go's `[]string` ([Slices and Arrays](#slices-and-arrays)).

**A func literal called in an initializer counts the same way.** `computed` calls a func literal whose
body reads `base`, declared in `main.go`, so `computed` moves. The literal becomes a C# lambda, cast to
`Func<nint>` and called at once ([Function Values and Closures](#function-values-and-closures)).

<!-- source: src/tests/Behavioral/PackageVarInitOrder/entries.go:21 -->
```go
var computed = func() int { return base * 2 }()
```
<!-- source: src/tests/Behavioral/PackageVarInitOrder/entries.cs.target:14-17 -->
```csharp
internal static nint computed;
internal static void initᴛcomputed() { computed = ((Func<nint>)(() => {
    return @base * 2;
}))(); }
```

**A string constant counts as a dependency.** Most Go constants have no initialization order in C#
either. A constant becomes a C# `const`, or, when C# cannot declare it `const`, a property that returns
its value on each read ([Constant Values](#constant-values)). A string constant is different: a property
would build its `u8` string again on every read, so it stays a `static readonly` field with an
initializer. That field has the same order problem as a var. This holds for a plain `const s = "x"` and
for a named string type alike. Here `label` is a named string type, and `pipeLabel` reads its constant
`labelPipe`, declared in another file, so `pipeLabel` moves.

<!-- source: src/tests/Behavioral/PackageVarInitOrder/registry.go:54-56 -->
```go
type label string

const labelPipe label = "pipe"
```
<!-- source: src/tests/Behavioral/PackageVarInitOrder/entries.go:51 -->
```go
var pipeLabel = string(labelPipe) + "!"
```
<!-- source: src/tests/Behavioral/PackageVarInitOrder/registry.cs.target:48-50 -->
```csharp
[GoType("@string")] partial struct label;

internal static readonly label labelPipe = "pipe"u8;
```
<!-- source: src/tests/Behavioral/PackageVarInitOrder/entries.cs.target:29-30 -->
```csharp
internal static @string pipeLabel;
internal static void initᴛpipeLabel() { pipeLabel = ((@string)labelPipe) + "!"u8; }
```

`[GoType("@string")]` marks `label` as a named type built on `@string`; a source generator fills in its
members ([Source Generators](#source-generators)).

**A hoisted string literal counts the same way.** go2cs moves a string literal used inside a function
into a `static readonly` field whose name ends in `ˢ`, so the string is built once, not on every call
([Strings](#strings-string-and-sstring)). That field is a static field initializer too, declared in the
file that uses the literal. So a var whose initializer calls a function that reads such a field also
moves, even when it reads no package var.

Here `describe`, in `z_impl.go`, returns a literal. Its hoisted field, `derivedThroughALaterFileˢ`, is
declared in that later file, so `derivedAtInit`, in `a_decls.go`, moves.

<!-- source: src/tests/Behavioral/StringLiteralHoisting/a_decls.go:13 -->
```go
var derivedAtInit = describe()
```
<!-- source: src/tests/Behavioral/StringLiteralHoisting/z_impl.go:8-10 -->
```go
func describe() string {
	return "derived through a later file"
}
```
<!-- source: src/tests/Behavioral/StringLiteralHoisting/z_impl.cs.target:6-10 -->
```csharp
private static readonly @string derivedThroughALaterFileˢ = "derived through a later file"u8;

internal static @string describe() {
    return derivedThroughALaterFileˢ;
}
```
<!-- source: src/tests/Behavioral/StringLiteralHoisting/a_decls.cs.target:7-8 -->
```csharp
internal static @string derivedAtInit;
internal static void initᴛderivedAtInit() { derivedAtInit = describe(); }
```

**A generated `package_init.cs` calls the init methods in Go's order.** It declares the package class's
static constructor, which calls each `initᴛ` method. Go's own type checker computes the order, and go2cs
copies it. Go's rule: take vars in declaration order, with files in name order, but skip a var until
every var it reads is set. A package where no var moves has no `package_init.cs`. This is the whole
generated class for the `PackageVarInitOrder` example package:

<!-- source: src/tests/Behavioral/PackageVarInitOrder/package_init.cs:9-18 -->
```csharp
partial class main_package {
    static main_package() {
        initᴛpipeLabel();
        initᴛcomputed();
        initᴛfirst();
        initᴛentryName();
        initᴛentryUpper();
        initᴛstdinName();
    }
} // end main_package
```

The files here are `entries.go`, `main.go` and `registry.go`. `pipeLabel` reads only a constant, so it is
ready at once. `computed` and `first` wait for `base`. `entryName`, `entryUpper` and `stdinName` wait
for `registry` and `names`, in the last file.

**This order is safe because of how C# initializes a class.** C# runs every static field initializer, in
every file, before the static constructor body. So each moved var finds its unmoved dependencies already
set. The constructor then sets the moved vars in Go's order.

**A var that reads another package's var needs no ordering.** Each Go package is its own C# class.
.NET initializes a class before its static fields are first read, so the imported package is always ready.
This matches Go, which initializes imported packages before the package that imports them.

**An `initᴛ` method is not a Go `func init()`.** Go's `init` functions become `[GoInit]` methods, which
are .NET module initializers ([Functions and Methods](#functions-and-methods)). As in Go, an `init` body
sees every package var already set, because .NET initializes the package class before the body reads its
fields.

**Full detail:** [Reference → Package-Level Variable Initialization Order](ConversionStrategies-Reference/variable-initialization-order.md#package-level-variable-initialization-order) — how dependencies are traced, the other var shapes that move, and the tests that guard each.

---

## Converted Tests

<!-- Length: the section runs past the usual size because it carries complete paired examples (a whole table-driven test, the export_test.go pattern, the generated host) and repeats short glyph and golib reminders, so a reader landing here first needs no other section. -->

`go2cs -tests` converts a package's Go tests along with its code. Each `x_test.go` becomes `x_test.cs` beside
the production `.cs` files. The tests build into their own program, `<pkg>.tests`, and a generated host runs
them the way `go test` does. The `testing` package they call is go2cs's hand-written
[`testing`](../src/core/testing/), not a converted one.

The examples in this section use a few golib names. golib is go2cs's runtime library
([The golib Runtime Library](#the-golib-runtime-library)):

- `ж<T>` is a heap box that stands in for a Go pointer `*T`. A `Ꮡ` prefix marks a variable that holds one,
  so Go's `t *testing.T` becomes `ж<testing.T> Ꮡt` ([Pointers](#pointers)).
- `@string` is Go's `string`, a byte string. A literal such as `"path.go"u8` is its UTF-8 bytes
  ([Strings](#strings-string-and-sstring)).
- `slice<T>` is Go's `[]T` ([Slices and Arrays](#slices-and-arrays)), and `nint` is Go's `int`
  ([Integer Types and Arithmetic](#integer-types-and-arithmetic)).

The [glyph table](#reading-converted-code-names-and-glyphs) lists every glyph.

**A test file converts like any other Go file.** Its types, tables and helpers convert by the usual rules.
`func TestX(t *testing.T)` becomes a `public static void` method that takes `ж<testing.T> Ꮡt`. This is a
complete table-driven test from Go's `path` package:

<!-- source: GOROOT/src/path/path_test.go:5 -->
```go
package path_test

import (
	. "path"
	…
	"testing"
)
…
type ExtTest struct {
	path, ext string
}

var exttests = []ExtTest{
	{"path.go", ".go"},
	{"path.pb.go", ".go"},
	{"a.dir/b", ""},
	{"a.dir/b.go", ".go"},
	{"a.dir/", ""},
}

func TestExt(t *testing.T) {
	for _, test := range exttests {
		if x := Ext(test.path); x != test.ext {
			t.Errorf("Ext(%q) = %q, want %q", test.path, x, test.ext)
		}
	}
}
```
<!-- source: src/core/path/path_test.cs:4 -->
```csharp
namespace go;

using static path_package;
…
using testing = testing_package;

partial class path_test_package {
…
[GoType] partial struct ExtTest {
    internal @string path, ext;
}

internal static slice<ExtTest> exttests = new ExtTest[]{
    new("path.go"u8, ".go"u8),
    new("path.pb.go"u8, ".go"u8),
    new("a.dir/b"u8, ""u8),
    new("a.dir/b.go"u8, ".go"u8),
    new("a.dir/"u8, ""u8)
}.slice();

public static void TestExt(ж<testing.T> Ꮡt) {
    foreach (var (_, test) in exttests) {
        {
            @string x = Ext(test.path); if (x != test.ext) {
                Ꮡt.Errorf("Ext(%q) = %q, want %q"u8, test.path, x, test.ext);
            }
        }
    }
}
…
} // end path_test_package
```

How to read the C#:

- A Go package becomes a C# class named `<pkg>_package`. Every file of the package opens the same
  `partial class` ([Package Conversion](#package-conversion)).
- A plain import becomes an alias: `"testing"` becomes `using testing = testing_package;`, so `testing.T`
  keeps its Go spelling. The dot import `. "path"` becomes `using static path_package;`, so `Ext` needs no
  qualifier ([Package Conversion](#package-conversion)).
- `[GoType]` marks a Go struct. It is `partial` so a [source generator](#source-generators) can add Go
  members to it, such as the constructor that each table entry calls, `new("path.go"u8, ".go"u8)`
  ([Struct Types](#struct-types)).
- `.slice()` turns the C# array into a Go slice ([Slices and Arrays](#slices-and-arrays)).
- `range` becomes `foreach`. A slice enumerates as `(index, value)` pairs, so `for _, test := range` becomes
  `var (_, test)`, with `_` discarding the index as in Go ([Loops, Range and Labels](#loops-range-and-labels)).
- The init clause of `if x := Ext(test.path); …` gets its own braces, so `x` stays scoped to the `if`. The
  declaration and the `if` share one line on purpose, to mirror Go's layout.
- `t.Errorf` becomes `Ꮡt.Errorf`. Go methods become C# extension methods, and `Errorf` has one whose `this`
  is `ж<testing.T>`. So the method is called through the box, as Go calls it through the pointer
  ([Functions and Methods](#functions-and-methods)).

**Each kind of test file has its own class.** An external test package, `package path_test`, becomes the
class `path_test_package`. A same-package test file, such as `package strings` in `export_test.go`, goes to
`strings_internal_test_package`. Both classes live in the test program, not in the production assembly.

**Same-package tests still reach unexported names.** Go's unexported names are C# `internal`. The production
project grants the `.tests` assembly access to them with `<InternalsVisibleTo Include="$(AssemblyName).tests" />`.
So the internal test class reaches them, as Go's same-package tests do.

**Go's `export_test.go` pattern needs no hand edits.** Go test authors often add a same-package file that
wraps unexported code in exported names. The external tests then call those wrappers. In go2cs the wrapper
lands in the internal test class:

<!-- source: GOROOT/src/strings/export_test.go:5 -->
```go
package strings
…
func StringFind(pattern, text string) int {
	return makeStringFinder(pattern).next(text)
}
```
<!-- source: src/core/strings/export_test.cs:4 -->
```csharp
namespace go;

using static go.strings_package;

partial class strings_internal_test_package {
…
public static nint StringFind(@string pattern, @string text) {
    return makeStringFinder(pattern).next(text);
}
…
} // end strings_internal_test_package
```

`using static go.strings_package;` lets the wrapper call the unexported `makeStringFinder` by its bare name.

An external test file imports both the production class and the internal test class with `using static`.
A call to a wrapper still names its class. That spelling binds the call to the wrapper exactly, and never
to a same-named member of another imported class:

<!-- source: GOROOT/src/strings/search_test.go:5 -->
```go
package strings_test
…
		got := StringFind(tc.pat, tc.text)
```
<!-- source: src/core/strings/search_test.cs:4 -->
```csharp
namespace go;

using slices = slices_package;
using static strings_package;
using testing = testing_package;
using static go.strings_internal_test_package;

partial class strings_test_package {
…
        nint got = strings_internal_test_package.StringFind(tc.pat, tc.text);
```

**A generated host registers every runnable test.** `go2cs_test_host.cs` is the test program's `Main`. It
registers each test by its Go name, its C# method, and the Go file and line that declare it. Tests are found
at conversion time, not by reflection, so each result can name the Go file and line.

The next example is from `container/list`. Test code follows the same naming as production code: the Go
import path `container/list` gives the namespace `go.container` ([Package Conversion](#package-conversion)).
`list_test.go` is a same-package test file (`package list`), so its tests live in
`list_internal_test_package`:

<!-- source: GOROOT/src/container/list/list_test.go:5 -->
```go
package list
…
func TestExtending(t *testing.T) {
```
<!-- source: src/core/container/list/go2cs_test_host.cs:1 -->
```csharp
// Code generated by go2cs test conversion. DO NOT EDIT.
namespace go.container;

using go.testing_runtime;

internal static class Go2CsTestHost
{
    public static int Main(string[] args)
    {
        TestRegistry registry = new("container/list", new string[]
        {
            "example_test.go",
            "list.go",
            "list_test.go",
        });
        registry.Add("TestExtending", list_internal_test_package.TestExtending, "list_test.go", 159);
        …
        return TestHost.Run(registry, args);
    }
}
```

How to read the host:

- `TestRegistry` and `TestHost` live in `go.testing_runtime`, a namespace inside the hand-written
  `testing` package.
- The registry gets the Go import path, used when it reports results, and a list of the package's files.
- A Go test runs in its package directory and may read files there by relative path. The host copies the
  listed files into a separate run directory, so a test sees the same files and cannot change the source tree.
- `registry.Add` names the test, the C# method to call, and the Go file and line where the test is declared.

**The host takes `go test`'s flags.** It accepts the usual ones, such as `-run`, `-v` and `-count`. If the
package has a `TestMain`, the host registers it and calls it, as `go test` does.

**Examples, benchmarks and fuzz targets convert but do not run.** The host currently runs only tests and
`TestMain`. The other functions stay in any converted test file that also holds tests, as `BenchmarkClone`
does in [`clone_test.cs`](../src/core/strings/clone_test.cs). A file that holds only examples or benchmarks
gets no `.cs` at all.

**A test project normally references the production project.** `strings.tests.csproj` compiles only the
converted test files and references `strings.csproj`; it does not recompile the production sources.
Recompiling them would make a second, distinct copy of every production type. A value that another converted
package passes in would then not match the copy the test uses. Referencing keeps one copy of each type.

**Known differences from Go are listed beside the package.** A hand-written `go2cs_test_disclosures.json`
names each test whose C# result is known to differ from `go test`, with the reason. A test that fails
without such an entry counts as a mismatch. A third-party module keeps its manifests in the committed tree
`src/tests/ModuleDisclosures/<module path>@<version>/<package dir>/`, read when the run passes
`-module-disclosures src/tests/ModuleDisclosures`. A manifest there applies only to that exact module
version, and a manifest in the package's own output directory still takes precedence.

**Full detail:** [Reference → Test suites reference the production project](ConversionStrategies-Reference/shadowing.md#test-suites-reference-the-production-project-instead-of-recompiling-it) — the test-project models and when each applies, the internal bridge class and its metadata files, test-side name collisions, and exactly which test files get no `.cs`.

---

## Compiled Library versus Source Code

Go builds a program from the source of every package it imports. go2cs instead turns each Go package
into its own C# project, the way C# libraries are usually built and shipped. Converted code therefore
treats every package as a separately compiled library. You will notice two effects: some pointers become
heap boxes where Go keeps the value on the stack, and the standard library can come from NuGet.

**Go decides stack or heap by reading the code a pointer flows into.** When Go's compiler can see that a
called function never keeps a pointer beyond the call, even a function in another package, it keeps the
pointed-to value on the stack. A C# method's signature cannot change with what its body does, and an
exported function can be called from packages converted separately. So go2cs chooses each function's
signature from its own package alone.

**An exported function's pointer parameter is a heap box.** Go's `*T` becomes golib's `ж<T>`, a small
heap object that holds one value ([Pointers](#pointers)). Another package may call an exported
(capitalized) function, so its signature must never depend on its body. The rule follows the name alone:
it holds even in package `main`, which nothing imports. A local whose address is passed to such a
function lives in a box too, even where Go keeps that local on the stack:

<!-- source: src/tests/Behavioral/PointerToPointer/PointerToPointer.go:5-74 -->
```go
type Buffer struct {
	buf      []byte
	off      int
	lastRead int8
}
…
func main() {
…
	b := Buffer{}
	PrintValPtr(&b.off)
	PrintValPtr(&b.off)
…
}
…
func PrintValPtr(ptr *int) {
	fmt.Printf("Value available at *ptr = %d\n", *ptr)
	*ptr++
}
```
<!-- source: src/tests/Behavioral/PointerToPointer/PointerToPointer.cs.target:7-66 -->
```csharp
[GoType] partial struct Buffer {
    internal slice<byte> buf;
    internal nint off;
    internal int8 lastRead;
}
…
internal static void Main() {
…
    ref var b = ref heap<Buffer>(out var Ꮡb);
    b = new Buffer(nil);
    PrintValPtr(Ꮡb.of(Buffer.Ꮡoff));
    PrintValPtr(Ꮡb.of(Buffer.Ꮡoff));
…
}
…
public static void PrintValPtr(ж<nint> Ꮡptr) {
    ref var ptr = ref Ꮡptr.DerefOrNull();

    fmt.Printf("Value available at *ptr = %d\n"u8, ptr);
    ptr++;
}
```

Each call prints `b.off` and then increments it through the pointer, so the first call prints 0 and the
second prints 1. Reading the C#, line by line:

- A name starting with `Ꮡ` holds a box ([glyphs](#reading-converted-code-names-and-glyphs)).
  `heap<Buffer>(out var Ꮡb)` creates the box `Ꮡb` and returns a `ref` to the value inside it. So `b`
  reads and writes like the Go local.
- `b = new Buffer(nil);` is Go's `b := Buffer{}`, kept as its own line. The `nil` argument picks the
  constructor that sets every field to its Go zero value ([Nil and Zero Values](#nil-and-zero-values)).
- The `[GoType]` attribute asks a [source generator](#source-generators) to add that constructor. It
  also adds `Buffer.Ꮡoff`, a static accessor that names the `off` field ([Struct Types](#struct-types)).
- `Ꮡb.of(Buffer.Ꮡoff)` is Go's `&b.off`: a pointer to the `off` field inside the box `Ꮡb`.
- In `PrintValPtr`, `DerefOrNull()` binds `ptr` to the value the box holds. The body then uses `ptr`
  where Go writes `*ptr`. For a nil pointer the `ref` is null, and the first read or write through it
  raises Go's nil-dereference panic ([Implicit Pointer Dereferencing](#implicit-pointer-dereferencing)).
- `nint` is Go's `int` ([Integer Types and Arithmetic](#integer-types-and-arithmetic)), and `"…"u8` is a
  UTF-8 string literal ([Strings](#strings-string-and-sstring)).

**An unexported function can take a `ref` instead.** Only code in the same package can call an
unexported function, so the converter sees every caller. A pointer parameter that is only dereferenced
usually becomes a C# `ref` parameter, and `&x` at the call becomes `ref x`. A local whose address reaches
only such parameters stays a plain local, with no box:

<!-- source: src/tests/Behavioral/RefLoweredParams/main.go:16-132 -->
```go
func addTo(out *uint64, v uint64) {
	*out += v
}
…
func main() {
…
	var total uint64
	addTo(&total, 5)
	addTo(&total, 7)
	fmt.Println("total:", total)
…
}
```
<!-- source: src/tests/Behavioral/RefLoweredParams/main.cs.target:11-137 -->
```csharp
internal static void addTo(ref uint64 @out, uint64 v) {
    @out += v;
}
…
// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object totalˢ = (@string)"total:"u8;
…
internal static void Main() {
    uint64 total = default!;
    addTo(ref total, 5);
    addTo(ref total, 7);
    fmt.Println(totalˢ, total);
…
}
```

Reading the C#:

- `@out` is Go's `out`, escaped with `@` because `out` is a C# keyword.
- `uint64 total = default!;` is Go's `var total uint64`: `default` is the zero value, and the `!` only
  silences C# nullable warnings ([Nil and Zero Values](#nil-and-zero-values)).
- `totalˢ` is the Go literal `"total:"`, stored once in a static field. The `ˢ` suffix marks such a
  hoisted literal, and `@string` is golib's Go string type, a UTF-8 byte string
  ([Strings](#strings-string-and-sstring)).
- `totalˢ` is typed `object` because `fmt.Println` takes Go's `...any`
  ([Empty Interface](#empty-interface-any)).

**A deferred or `go` call keeps the box.** The call runs later, so it must hold the pointer until then.
For example, `defer printVal(&x)` keeps `x` in a box, even though `printVal` takes a `ref` everywhere
else ([Defer / Panic / Recover](#defer--panic--recover), [Goroutines](#goroutines), [Pointers](#pointers)).

**The standard library is referenced, never reconverted.** `go2cs -recurse` converts a program's own
packages and its third-party dependencies from source. (`-recurse=module` converts only the program's own
packages.) Every standard-library package the program imports is a reference to an already-converted
library. Plain `-recurse` makes these project references into the go2cs install, such as
`core/fmt/fmt.csproj`.

**`go2cs -recurse=nuget` references the standard library as NuGet packages.** Each package ID is `go.`
plus the Go import path, with dots for slashes: `net/http` becomes `go.net.http`. Every converted project
also gets `go.lib`, the [golib runtime](#the-golib-runtime-library), and `go.gen`, the
[source generators](#source-generators). The program's own packages stay project references. A program
that imports `fmt` gets these lines in its `.csproj`:

<!-- expected app .csproj lines; emitted by src/go2cs/projectFileWriter.go:387,390,471 -->
<!-- source: src/go2cs/moduleConverter_integration_test.go:262-264 -->
```xml
<PackageReference Include="go.fmt" Version="$(GoStdLibVersion)" />
<PackageReference Include="go.lib" Version="$(GoStdLibVersion)" />
<PackageReference Include="go.gen" Version="$(GoStdLibVersion)" PrivateAssets="all" />
```

**`$(GoStdLibVersion)` follows the Go release doing the conversion.** go2cs writes a
`Directory.Build.props` beside the converted projects that sets it with a floating NuGet revision. For
example, Go 1.N.P gives `1.N.P.*`, and restore takes the newest published `1.N.P.x` package. Set
the property yourself, for example with `-p:GoStdLibVersion=…`, to pin one exact version. <!-- default + override: src/go2cs/moduleConverter.go:661-665; version = the converting toolchain's GOVERSION (src/go2cs/readme.go:93-101); test: src/go2cs/moduleConverter_integration_test.go:294-306 -->

**`-recurse=nuget` refuses a mismatched Go release.** The published packages match one Go release, by
major and minor version. When the converting Go toolchain is a different release, the conversion stops
with an error, because its standard library would not match the packages and the project could not
restore. Only the major and minor numbers are compared, so a different patch number is accepted. <!-- src/go2cs/toolchainResolution.go:227-245 (version.Lang comparison: 1.23.1 vs 1.23.5 is accepted) -->

**Full detail:** [Reference → Compiled Library versus Source Code](ConversionStrategies-Reference/compiled-library-vs-source.md#compiled-library-versus-source-code) — why source availability shapes Go's escape analysis, and how the converter chooses between NuGet and source references.

---

## Constant Values

Go has two kinds of constant. A *typed* constant has a declared type, as in `const n int = 3`. An
*untyped* constant, as in `const n = 3`, has no fixed type and takes one from each place it is used.
go2cs keeps that difference visible, and it always writes the constant's exact value into the C#.

A converted constant takes one of three C# forms:

- a C# `const`, for a constant of a basic numeric type such as `int` or `float64`, and for any
  boolean constant, typed or untyped;
- a get-only static property, `static T name => value;`, for a constant C# cannot declare `const`:
  an untyped number, a named type, a `uintptr` (a golib struct), a complex number, or an `int` value
  too wide for 32 bits;
- a `static readonly` field, for any string constant, typed or untyped.

When the value is computed, a `/* … */` comment beside it keeps the Go expression.

**An untyped numeric constant is a property of a golib wrapper type.** golib is go2cs's
[runtime library](#the-golib-runtime-library). Its wrappers
[`UntypedInt`](../src/core/golib/UntypedInt.cs), [`UntypedFloat`](../src/core/golib/UntypedFloat.cs)
and [`UntypedComplex`](../src/core/golib/UntypedComplex.cs) hold the value and convert implicitly to
every Go numeric type. Hex spelling is kept, so masks stay recognizable:

<!-- source: GOROOT/src/compress/lzw/reader.go:39-43 -->
```go
const (
	maxWidth           = 12
	decoderInvalidCode = 0xffff
	flushBuffer        = 1 << maxWidth
)
```
<!-- source: src/core/compress/lzw/reader.cs:32-34 -->
```csharp
internal static UntypedInt maxWidth => 12;
internal static UntypedInt decoderInvalidCode => 0xffff;
internal static UntypedInt flushBuffer => /* 1 << maxWidth */ 4096;
```

`name => value` returns the value on each read and stores nothing. A Go constant needs no
initialization, and a property has none, so code anywhere in the package can read it in any order
([Package-Level Variable Initialization Order](#package-level-variable-initialization-order)).

**An untyped constant takes its type where it is used.** A `:=` from one gives the new variable Go's
default type for that constant: `rune` for a rune constant, `float64` for a float. The C# names that
type instead of writing `var`:

<!-- source: src/tests/Behavioral/UntypedConstDefine/main.go:8-34 -->
```go
const replacementChar = '�' // untyped rune (mirrors unicode.ReplacementChar)
const scale = 2.5                // untyped float
…
func main() {
	codepoint := replacementChar
	s := string(codepoint)
	fmt.Println(len(s), codepoint)

	factor := scale
	fmt.Println(factor * 2)
…
```
<!-- source: src/tests/Behavioral/UntypedConstDefine/main.cs.target:7-24 -->
```csharp
internal static UntypedInt replacementChar => /* '�' */ 65533;

internal static UntypedFloat scale => 2.5;
…
internal static void Main() {
    rune codepoint = replacementChar;
    @string s = ((@string)codepoint);
    fmt.Println(len(s), codepoint);
    float64 factor = scale;
    fmt.Println(factor * 2D);
…
```

A rune is an integer, so a rune constant is an `UntypedInt` too. The wrapper does not need to
remember that it was a rune: the converter reads each use's type from Go's type checker when it
converts, and writes that type into the C#.

`rune` and `float64` are C# aliases that keep Go's names, for C#'s 32-bit `int` (`System.Int32`) and
`double` ([Integer Types and Arithmetic](#integer-types-and-arithmetic)). `@string` is golib's Go
string ([Strings](#strings-string-and-sstring)). `2D` is a C# `double` literal: Go's untyped `2` takes
the `float64` type of `factor`.

**Passed to an `any` parameter, an untyped constant takes its default type.** Go gives an untyped
integer the type `int`, which is C# `nint`, so the C# casts to `(nint)`. Without the cast, the wrapper
struct itself would be passed, and `fmt` would print the struct instead of the number. `"size=%d"u8`
is a C# UTF-8 string literal:

<!-- source: src/tests/Behavioral/UntypedConstDefine/main.go:26-41 -->
```go
const fsize = 5
…
func main() {
…
	fmt.Println(fmt.Sprintf("size=%d", fsize+1))
```
<!-- source: src/tests/Behavioral/UntypedConstDefine/main.cs.target:17-29 -->
```csharp
internal static UntypedInt fsize => 5;
…
internal static void Main() {
…
    fmt.Println(fmt.Sprintf("size=%d"u8, (nint)(fsize + 1)));
```

**In arithmetic with a typed value, the constant is cast to that value's type.** The wrapper converts
to and from every numeric type, so C# could convert `a` to the wrapper instead. The result would then
have the wrong type: the code may not compile, or may compute at the wrong width. The cast selects
the operator Go uses, here `uint64` multiplication:

<!-- source: src/tests/Behavioral/UntypedConstArithmetic/main.go:10-17 -->
```go
const two32 = 1 << 32 // untyped const -> UntypedInt; value exceeds int32
…
func main() {
	var a uint64 = 100
	var b uint64 = 3
	fmt.Println(a*two32 + b) // 429496729603
…
```
<!-- source: src/tests/Behavioral/UntypedConstArithmetic/main.cs.target:7-16 -->
```csharp
internal static UntypedInt two32 => /* 1 << 32 */ 4294967296;
…
internal static void Main() {
    uint64 a = 100;
    uint64 b = 3;
    fmt.Println(a * (uint64)two32 + b);
…
```

**A typed constant of a basic numeric type is a C# `const`.** Go `int` is C# `nint`, a native-width
integer ([Integer Types and Arithmetic](#integer-types-and-arithmetic)). This file sits in the
package's `windows/` folder, which holds the files that differ between target operating systems
([Package Conversion](#package-conversion)):

<!-- source: GOROOT/src/os/file.go:80 -->
```go
	O_RDONLY int = syscall.O_RDONLY // open the file read-only.
```
<!-- source: src/core/os/windows/file.cs:86 -->
```csharp
public const nint O_RDONLY = /* syscall.O_RDONLY */ 0;             // open the file read-only.
```

C# accepts a `const nint` or `const nuint` only for a value that fits 32 bits. A wider value takes
the property form instead. `unchecked` lets C# accept a cast to `nint` of a value that only fits a
64-bit `nint`:

<!-- source: GOROOT/src/strings/strings.go:18 -->
```go
const maxInt = int(^uint(0) >> 1)
```
<!-- source: src/core/strings/strings.cs:21 -->
```csharp
internal static nint maxInt => /* int(^uint(0) >> 1) */ unchecked((nint)9223372036854775807);
```

A `uintptr` constant is a property for a similar reason: golib's `uintptr` is a struct, and C# has no
`const` of a struct type ([`unsafe.Pointer` and `uintptr`](#unsafepointer-and-uintptr)).

An untyped boolean constant is a plain `const bool`, with the Go expression in a comment:

<!-- source: src/tests/Behavioral/TypeInference/TypeInference.go:21 -->
```go
	const c = 3 < 4 // c is the untyped boolean constant true
```
<!-- source: src/tests/Behavioral/TypeInference/TypeInference.cs.target:27 -->
```csharp
    const bool c = /* 3 < 4 */ true;
```

**A constant of a named type is a property of that type.** A named numeric type such as
`type Order int` becomes a C# struct, and C# cannot declare a `const` of a struct type.
`[GoType("num:nint")] partial struct Order;` declares a struct over Go `int` (C# `nint`); a
[source generator](#source-generators) fills in its body, operators and conversions
([Named Numeric Types and Constant Contexts](#named-numeric-types-and-constant-contexts)).

In a constant group, `iota` and implicit repetition fold to plain values, one declaration per name.
The first keeps `/* iota */` to show where the numbering starts:

<!-- source: GOROOT/src/compress/lzw/reader.go:28-37 -->
```go
// Order specifies the bit ordering in an LZW data stream.
type Order int

const (
	// LSB means Least Significant Bits first, as used in the GIF file format.
	LSB Order = iota
	// MSB means Most Significant Bits first, as used in the TIFF and PDF
	// file formats.
	MSB
)
```
<!-- source: src/core/compress/lzw/reader.cs:27-30 -->
```csharp
[GoType("num:nint")] partial struct Order;

public static Order LSB => /* iota */ 0;
public static Order MSB => 1;
```

**A constant declared inside a function takes its type from its uses.** Every use of a local
constant is in view inside its function. When every use needs the same type, the constant is
declared at that type, and it is a C# `const` where C# allows one:

<!-- source: src/tests/Behavioral/UntypedConstDefine/main.go:120-128 -->
```go
func localPrecision(x float64) float64 {
	const c = 5.42857142857142815906e-01
	const d = -7.05306122448979611050e-01
	const smallestNormal = 2.22507385850720138309e-308 // 2**-1022 = 0x0010000000000000
	if x < smallestNormal {
		return d
	}
	return c + x*d
}
```
<!-- source: src/tests/Behavioral/UntypedConstDefine/main.cs.target:83-91 -->
```csharp
internal static float64 localPrecision(float64 x) {
    const float64 c = 5.42857142857142815906e-01;
    const float64 d = -7.05306122448979611050e-01;
    const float64 smallestNormal = 2.22507385850720138309e-308;
    if (x < smallestNormal) {
        return d;
    }
    return c + x * d;
}
```

A package-level untyped constant always keeps its wrapper. It can be used from any function or file of
the package, and those uses may need different types.

When a local constant's uses need different types, it keeps its wrapper type as a local variable:

<!-- source: src/tests/Behavioral/UntypedConstDefine/main.go:51-54 -->
```go
	const mixed = 0.25 // float64 AND float32 uses -> stays UntypedFloat
	var f64 float64 = mixed
	var f32 float32 = mixed
	fmt.Println(f64, f32)
```
<!-- source: src/tests/Behavioral/UntypedConstDefine/main.cs.target:36-39 -->
```csharp
        UntypedFloat mixed = 0.25;
        float64 f64 = mixed;
        float32 f32 = mixed;
        fmt.Println(f64, f32);
```

**A complex constant is a real complex value.** It is written as its real part plus its imaginary
part. `.i()` is a golib extension method that makes a number imaginary, so `3D.i()` is Go's `3i`. An
untyped or `complex128` constant uses C# `double` halves (`D`), and a `complex64` constant uses
`float` halves (`F`). Complex types are structs in C#, so each constant is a property:

<!-- source: src/tests/Behavioral/ComplexConstContext/main.go:16-26 -->
```go
const (
…
	cNegImag    = 2.25 - 0.75i        // a NEGATIVE imaginary part
	cPureImag   = 3i                  // a ZERO real part
…
	cFolded     = (1 + 2i) * (3 + 4i) // a folded complex EXPRESSION (-5+10i)
)
…
const c64 complex64 = 1.5 + 2.5i
```
<!-- source: src/tests/Behavioral/ComplexConstContext/main.cs.target:10-15 -->
```csharp
internal static UntypedComplex cNegImag => /* 2.25 - 0.75i */ 2.25D + -0.75D.i();
internal static UntypedComplex cPureImag => /* 3i */ 3D.i();
…
internal static UntypedComplex cFolded => /* (1 + 2i) * (3 + 4i) */ -5D + 10D.i();

internal static complex64 c64 => /* 1.5 + 2.5i */ 1.5F + 2.5F.i();
```

**A string constant is a `static readonly` field.** A string value is an allocation, and a field
creates it once, where a property would create it on every read. `@string` is golib's Go string, and
`"op"u8` is a C# UTF-8 literal ([Strings](#strings-string-and-sstring)). A constant of a named string
type is a field of that named type in the same way:

<!-- source: src/tests/Behavioral/NamedStringConsts/main.go:25 -->
```go
const prefix = "op"
```
<!-- source: src/tests/Behavioral/NamedStringConsts/main.cs.target:17 -->
```csharp
internal static readonly @string prefix = "op"u8;
```

**A string constant declared inside a function becomes a class field plus a local.** C# cannot declare
a static field inside a method, so the value moves up to the class. Its field name ends in `ᶜ`, which
keeps it apart from the local that keeps the Go name
([Reading Converted Code](#reading-converted-code-names-and-glyphs)). The local copies the field; an
`@string` copy shares the same bytes and allocates nothing:

<!-- source: src/tests/Behavioral/NamedStringConsts/main.go:42-56 -->
```go
func main() {
…
	const plain = "plain"
	fmt.Println(plain)
```
<!-- source: src/tests/Behavioral/NamedStringConsts/main.cs.target:44-61 -->
```csharp
// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
private static readonly @string plainᶜ = "plain"u8;
…
internal static void Main() {
…
    @string plain = plainᶜ;
    fmt.Println(plain);
```

**Full detail:** [Reference → Constant Values](ConversionStrategies-Reference/constants.md#constant-values) — why a property rather than a field, the `iota` forms, exact float and complex rendering, when a local constant keeps its wrapper, and `unchecked` casts for native-width values.

---

<a id="native-and-narrow-integer-types"></a>

## Integer Types and Arithmetic

Go's platform-sized `int` and `uint` become C#'s native-sized `nint` and `nuint`. Like Go's, they are 32 or 64
bits wide depending on the target platform. The fixed-width types keep their Go names, so most arithmetic
reads as it does in Go:

<!-- source: src/tests/Behavioral/MethodGroupGenericArg/main.go:5 -->
```go
func addInt(a, b int) int { return a + b }
```
<!-- source: src/tests/Behavioral/MethodGroupGenericArg/main.cs.target:7 -->
```csharp
internal static nint addInt(nint a, nint b) {
    return a + b;
}
```

In a few places C# arithmetic behaves differently from Go's: narrow types, unsigned negation, shifts and a
signed division by -1.
There the converter adds a cast, rewrites the expression or calls the
[golib](#the-golib-runtime-library) runtime library. Division by zero and slice bounds also need care. Each
case has its own paragraph in this section.

**The fixed-width names are aliases.** `int8` through `uint64`, and `rune`, are aliases of the C# primitives.
Each converted project declares them. So `uint8` is C#'s `byte`, `int64` is `long`, and `rune` is `int`.
Go's `byte` stays C#'s own `byte`, the same type as `uint8`.
<!-- each project file carries lines such as `<Using Include="System.Int32" Alias="rune" />`:
     src/core/bufio/bufio.csproj:131, from src/go2cs/csproj-template.xml:105-116 -->

**`uintptr` is its own type.** It becomes golib's [`uintptr`](../src/core/golib/uintptr.cs), a small struct
that holds one `nuint`. It is not an alias, because Go treats `uint` and `uintptr` as different types, and
`%T` and type switches tell them apart. A `uintptr` declaration reads as in Go:

<!-- source: src/tests/Behavioral/ArrayWideIndexAddress/main.go:14 -->
```go
var i uintptr = 2
```
<!-- source: src/tests/Behavioral/ArrayWideIndexAddress/main.cs.target:10 -->
```csharp
uintptr i = 2;
```

Its use with `unsafe.Pointer` is covered in [`unsafe.Pointer` and `uintptr`](#unsafepointer-and-uintptr).
<!-- uintptr: a distinct golib struct, not an alias of nuint, so an alias cannot erase the uint/uintptr
     difference (src/core/golib/uintptr.cs:16-20, :39). Goldens:
     src/tests/Behavioral/SwitchPointerSentinelCase/main.cs.target:89;
     src/tests/Behavioral/TypeSwitch/TypeSwitch.cs.target:93 (`case uintptr:` beside `case nuint:`). -->

**Narrow arithmetic is cast back to its type.** Go computes `int8`, `uint8`, `int16` and `uint16` arithmetic at
that width, so the `uint8` sum `200 + 100` wraps to 44. C# promotes these types to `int` first, so the sum is 300.
The converter casts the result back to the narrow type, which restores Go's wrapped value.

In this example, `takeU8` is a function that takes a `uint8`. In the C#, `fmt` is Go's `fmt` package,
converted to C# like any other package ([Package Conversion](#package-conversion)):

<!-- source: src/tests/Behavioral/NarrowArithmeticArg/main.go:28 -->
```go
var a, b uint8 = 200, 100

// Argument context.
fmt.Println(takeU8(a + b)) // 300 wraps to 44
```
<!-- source: src/tests/Behavioral/NarrowArithmeticArg/main.cs.target:29 -->
```csharp
uint8 a = 200;
uint8 b = 100;
fmt.Println(takeU8((uint8)(a + b)));
```

The same cast appears wherever the result is assigned, declared, returned or stored in a struct field:

<!-- source: src/tests/Behavioral/NarrowArithmeticArg/main.go:43 -->
```go
y := a + b // short-var declaration; y is uint8, wraps to 44
fmt.Println(y)

y = y + 1 // reassignment, 45
fmt.Println(y)
```
<!-- source: src/tests/Behavioral/NarrowArithmeticArg/main.cs.target:40 -->
```csharp
var y = (uint8)(a + b);
fmt.Println(y);
y = (uint8)(y + 1);
fmt.Println(y);
```

A comparison gets the cast too, even though nothing is stored. Without it, C# would compare 300, not 44:

<!-- source: src/tests/Behavioral/NarrowArithmeticArg/main.go:73 -->
```go
fmt.Println(a+b == 44)   // 300 wraps to 44 (uint8) -> true
```
<!-- source: src/tests/Behavioral/NarrowArithmeticArg/main.cs.target:56 -->
```csharp
fmt.Println((uint8)(a + b) == 44);
```

**A narrow result keeps Go's width wherever it goes.** When the result is passed as `any`, used as a map key or
an index, or converted to a wider type, the cast sits on the arithmetic itself. An `any` value keeps Go's type
too, so `%T` reports `int8`, not `int32`. Here `a` is an `int8` holding 100, so Go's `a + a` is -56:

<!-- source: src/tests/Behavioral/NarrowArithmeticSinks/main.go:63 -->
```go
var x any = a + a
```
<!-- source: src/tests/Behavioral/NarrowArithmeticSinks/main.cs.target:116 -->
```csharp
any x = (int8)(a + a);
```

No cast is added where only the low bits matter, such as more arithmetic of the same type or a conversion to
a type no wider.

**Wider types need no narrowing cast.** `int32` and wider types are not promoted, and C# arithmetic is
unchecked by default. So `+`, `-` and `*` on them wrap on overflow exactly as in Go, with no cast.

**Bitwise results are cast at every width.** The converter wraps each bitwise operator, such as `&`, `|` and
Go's "and not" `&^`, in a cast to its result type. The cast changes nothing at 32 or 64 bits; it only keeps the
type explicit. You see it in the `(uint32)(…)` and `(uint64)(…)` around the examples that follow.
<!-- src/go2cs/convBinaryExpr.go (~1675): a bitwise-shaped operand "is recursively emitted with its own
     concrete `(type)(…)` wrap". Goldens: ShiftPrecedenceUnsigned/main.cs.target:8, :19, :37. Plain wide `+`
     stays uncast. -->

**Unsigned negation subtracts from zero.** C#'s unary minus never keeps an unsigned type: it rejects `uint64`
and `nuint`, and widens smaller unsigned values to a signed type. Go's `-x` on an unsigned value of type `T`
becomes `((T)0 - x)`, which wraps the same way. The common "lowest set bit" idiom shows it:

<!-- source: src/tests/Behavioral/ShiftPrecedenceUnsigned/main.go:20 -->
```go
func lowestSetBit(x uint32) uint32 {
	return x & -x // wrap-around negation; isolates the lowest set bit
}
```
<!-- source: src/tests/Behavioral/ShiftPrecedenceUnsigned/main.cs.target:7 -->
```csharp
internal static uint32 lowestSetBit(uint32 x) {
    return (uint32)(x & ((uint32)0 - x));
}
```

Here `((uint32)0 - x)` is the negation. The outer `(uint32)(…)` is the bitwise cast around `&`.
<!-- unsigned negation: src/go2cs/convUnaryExpr.go:1228-1234; golden
     src/tests/Behavioral/ShiftPrecedenceUnsigned/main.cs.target:19
     `fmt.Println((uint64)(z & ((uint64)0 - z)));` for Go `fmt.Println(z & -z)` (main.go:29).
     C# spec: unary minus on uint converts to long, on byte/ushort promotes to int, on ulong/nuint
     is a compile error. -->

**Bitwise complement uses C#'s spelling.** Go's unary `^x` becomes C#'s `~x`, and Go's "and not" `x &^ y`
becomes `x & ~y`. This example clears the top bit of a `uint64`:

<!-- source: src/tests/Behavioral/ShiftPrecedenceUnsigned/main.go:63 -->
```go
var hi uint64 = 0xFFFFFFFFFFFFFFFF
fmt.Println(hi &^ (1 << 63)) // clear the high bit -> 0x7FFFFFFFFFFFFFFF
```
<!-- source: src/tests/Behavioral/ShiftPrecedenceUnsigned/main.cs.target:36 -->
```csharp
uint64 hi = 0xFFFFFFFFFFFFFFFFUL;
fmt.Println((uint64)(hi & ~(((uint64)1 << (int)(63)))));
```

The constant `1` becomes `(uint64)1`, so the shift happens at 64 bits, as Go's typed constant does
([Constant Values](#constant-values)). A large unsigned literal gets C#'s `UL` suffix so it compiles. The
shift's own parentheses and `(int)` count are explained in the next paragraph.
<!-- golden: src/tests/Behavioral/NarrowArithmeticArg/main.cs.target:32 `(uint8)(~a)` for Go `^a`. -->

**Shifts get parentheses and an `int` count.** Go's `<<` and `>>` bind tighter than `+` and `-`, but C#'s bind
looser. The converter wraps each shift in parentheses so the grouping stays Go's. C#'s shift also takes an
`int` count, while Go accepts any integer type, so the count is cast to `int`:

<!-- source: src/tests/Behavioral/ShiftPrecedenceUnsigned/main.go:32 -->
```go
var x uint64 = 0x100
fmt.Println(x>>4 + x) // (0x100>>4)+0x100 = 16 + 256 = 272
```
<!-- source: src/tests/Behavioral/ShiftPrecedenceUnsigned/main.cs.target:20 -->
```csharp
uint64 x = 0x100;
fmt.Println((x >> (int)(4)) + x);
```

**A shift count that can reach the width calls a golib helper.** In Go, shifting by the operand's full width or
more gives 0, or -1 when a negative value is shifted right. C# masks the count instead, so a 64-bit value
shifted by 64 comes back unchanged. When the count is not provably below the width, `>>` becomes `.Rsh(…)`
and `<<` becomes `.Lsh(…)`. These are extension methods from golib's
[`GoShift`](../src/core/golib/GoShift.cs), one set per operand type. The 64-bit ones are:

<!-- source: src/core/golib/GoShift.cs:38 -->
```csharp
[MethodImpl(MethodImplOptions.AggressiveInlining)]
public static uint64 Rsh(this uint64 x, uint64 n) => n >= 64 ? 0UL : x >> (int)n;

[MethodImpl(MethodImplOptions.AggressiveInlining)]
public static uint64 Lsh(this uint64 x, uint64 n) => n >= 64 ? 0UL : x << (int)n;

[MethodImpl(MethodImplOptions.AggressiveInlining)]
public static int64 Rsh(this int64 x, uint64 n) => n >= 64 ? x >> 63 : x >> (int)n;

[MethodImpl(MethodImplOptions.AggressiveInlining)]
public static int64 Lsh(this int64 x, uint64 n) => n >= 64 ? 0L : x << (int)n;
```

An unsigned count is taken as a `uint64`, so a count of any unsigned type, such as `nuint`, passes straight
in. In the signed `Rsh`, `x >> 63` fills the result with the sign bit: -1 for a negative value, 0 otherwise.

A signed count is taken as an `int64` instead (`one.Lsh((int64)(n))`), and binds a second set of overloads.
Go panics with the runtime error "negative shift amount" when a signed count is below zero at run time, and
these raise that same panic, which `recover` sees; a count of zero or more goes to the unsigned helper. So a
signed count is never kept native on the strength of `y % M` alone, since Go's `-3 % 8` is -3.

Here the counts come from a slice at run time, and several are 64 or more. In the C#,
`new nuint[]{…}.slice()` builds a golib [slice](#slices-and-arrays), and `foreach (var (_, k) in c)` is
Go's `for _, k := range c` ([Loops, Range and Labels](#loops-range-and-labels)):

<!-- source: src/tests/Behavioral/GoShiftSemantics/main.go:21 -->
```go
c := []uint{0, 1, 63, 64, 65, 200}
var u uint64 = 0x8000000000000001

// Unsigned 64-bit: count >= 64 -> 0 (both directions).
for _, k := range c {
	fmt.Println(u>>k, u<<k)
}
```
<!-- source: src/tests/Behavioral/GoShiftSemantics/main.cs.target:14 -->
```csharp
var c = new nuint[]{0, 1, 63, 64, 65, 200}.slice();
uint64 u = 0x8000000000000001UL;
foreach (var (_, k) in c) {
    fmt.Println(u.Rsh(k), u.Lsh(k));
}
```

**A count provably below the width keeps C#'s own operator.** That covers a constant in range, such as
`x >> 4`, and a count masked or reduced below the width, such as `k & 63` or `k % 64`. These are most shifts
in real code. Continuing the same example:

<!-- source: src/tests/Behavioral/GoShiftSemantics/main.go:41 -->
```go
fmt.Println(u >> (c[3] & 63)) // 64 & 63 = 0 -> u
fmt.Println(u >> (c[4] % 64)) // 65 % 64 = 1 -> u >> 1
```
<!-- source: src/tests/Behavioral/GoShiftSemantics/main.cs.target:26 -->
```csharp
fmt.Println((u >> (int)(((nuint)(c[3] & 63)))));
fmt.Println((u >> (int)((c[4] % 64))));
```

The `(nuint)` on the first line is the bitwise cast around `&`; `%` is not bitwise, so the second line has
none. The extra parentheses are harmless.

A named integer type, such as `type Word uint`, becomes a C# struct whose operators are generated at compile
time ([Named Numeric Types](#named-numeric-types-and-constant-contexts),
[Source Generators](#source-generators)). A shift on it keeps the plain `>>` in the converted code, because
that generated operator applies the same Go rule.

**Division and remainder check for -1.** Go defines the one signed division that overflows: the most
negative value divided by -1 wraps back to itself, and any value modulo -1 is 0. .NET throws an
`OverflowException` there instead, which a deferred `recover()` cannot catch. So a signed `int`, `int32` or
`int64` division or remainder where the divisor may be -1 and the dividend may be the most negative value
calls golib's `quo` or `rem`. Each checks for -1 and otherwise uses C#'s own `/` or `%`:

<!-- source: GOROOT/src/image/geom.go:40 -->
```go
return Point{p.X / k, p.Y / k}
```
<!-- source: src/core/image/geom.cs:41 -->
```csharp
return new Point(quo(p.X, k), quo(p.Y, k));
```

A constant divisor keeps the plain operator (a constant -1 folds), and so does a constant dividend other
than the most negative value, a `len` or `cap` dividend, and a named integer type, whose generated
operators carry the same check.

**Division by zero panics as in Go.** C# throws a `DivideByZeroException` for a zero divisor, from the plain
operators and from `quo` and `rem` alike. When golib's panic handling catches that exception, it turns it
into Go's `runtime error: integer divide by zero` panic.
So [`recover`](#defer--panic--recover) catches it as in Go, and an unrecovered one reports Go's message.
<!-- golden: src/tests/Behavioral/DivideByZeroPanic (safeDiv/safeMod recover, outerGuard crosses a frame);
     mapping: src/core/golib/runtime/RuntimeErrorPanic.cs:239 (TryAsPanic adopts DivideByZeroException as
     Go's integer-divide-by-zero runtime error). -->

**Slice bounds are cast to `int`.** Go's `int` bound converts to `nint`, but C#'s range syntax, `s[lo..hi]`,
accepts only an `int`. So the converter casts each bound to `int`. Here `s.pos` is a Go `int` field:

<!-- source: src/tests/Behavioral/AdapterNameInterfaceCollision/main.go:40 -->
```go
n := copy(p, s.data[s.pos:])
```
<!-- source: src/tests/Behavioral/AdapterNameInterfaceCollision/main.cs.target:23 -->
```csharp
nint n = copy(p, s.data[(int)(s.pos)..]);
```

Slicing itself is explained in [Slices and Arrays](#slices-and-arrays).

**Full detail:** [Reference → Native and Narrow Integer Types](ConversionStrategies-Reference/native-and-narrow-integers.md#native-and-narrow-integer-types) —
every place a narrow result is cast back, how a shift count is proven in range, shifts on named types, and
how integer literals are written.

---

## Named Numeric Types and Constant Contexts

A Go type defined over a number type, such as `type Duration int64`, becomes a small C# wrapper struct.
The converter emits one line: a `partial struct` marked
[`[GoType("num:<underlying>")]`](../src/core/golib/GoTypeAttribute.cs). The `TypeGenerator`
[source generator](#source-generators) writes the rest of the struct when the project compiles. So code that
uses the type reads almost line for line as in Go.

**What the generator adds.** The generated struct has one field that holds the value. It has implicit
conversions to and from the underlying type, so `nameOff s = 7;` needs no cast. It also has Go's operators,
each returning the named type. Go's number type names, such as `int64`, `uint8` and `float64`, are C# aliases
for `long`, `byte` and `double` ([Integer Types and Arithmetic](#integer-types-and-arithmetic)).

**A named type, its constants and its methods.** Here is Go's `time.Duration`, a count of nanoseconds, with
its unit constants and one method:

<!-- source: GOROOT/src/time/time.go:911-1090 -->
```go
type Duration int64
…
const (
	Nanosecond  Duration = 1
	Microsecond          = 1000 * Nanosecond
	Millisecond          = 1000 * Microsecond
	Second               = 1000 * Millisecond
	Minute               = 60 * Second
	Hour                 = 60 * Minute
)
…
func (d Duration) Seconds() float64 {
	sec := d / Second
	nsec := d % Second
	return float64(sec) + float64(nsec)/1e9
}
```
<!-- source: src/core/time/time.cs:910-1097 -->
```csharp
[GoType("num:int64")] partial struct Duration;
…
public static Duration ΔNanosecond => 1;

public static Duration Microsecond => /* 1000 * Nanosecond */ 1000;

public static Duration Millisecond => /* 1000 * Microsecond */ 1000000;

public static Duration ΔSecond => /* 1000 * Millisecond */ 1000000000;

public static Duration ΔMinute => /* 60 * Second */ 60000000000;

public static Duration ΔHour => /* 60 * Minute */ 3600000000000;
…
public static float64 Seconds(this Duration d) {
    var sec = d / ΔSecond;
    var nsec = d % ΔSecond;
    return (float64)(int64)sec + (float64)(int64)nsec / 1e9D;
}
```

Reading the C#, piece by piece:

- `[GoType("num:int64")]` says the struct wraps an `int64`. Go's `int` and `uint` appear as `num:nint` and
  `num:nuint`, C#'s native-sized integers ([Integer Types and Arithmetic](#integer-types-and-arithmetic)).
- Each constant is a static get-only property typed `Duration`. Its value is computed ahead of time, and the
  Go expression stays beside it as a `/* … */` comment ([Constant Values](#constant-values)).
- The method is a static extension method, `this Duration d`, so a call still reads `d.Seconds()`
  ([Functions and Methods](#functions-and-methods)).
- A `Δ` prefix marks a renamed name ([Names and Glyphs](#reading-converted-code-names-and-glyphs)). All of a
  package's top-level names, its types, constants, functions and methods, are members of one C# class,
  `time_package` ([Package Conversion](#package-conversion)). Go's `Time` type has a `Second()` method, so the
  constant `Second` would clash with it and becomes `ΔSecond`. `Microsecond` has no such twin and keeps its name.

**Operators keep the named type.** The wrapper has Go's arithmetic and comparison operators, `++` and `--`.
Over an integer it also has the bitwise and shift operators. Each operator that computes a value returns the
named type, as in Go, so `d / ΔSecond` is still a `Duration`. Shifts follow Go's rules: a shift by the full
bit width or more gives 0, where C# would wrap the count
([Integer Types and Arithmetic](#integer-types-and-arithmetic)). A C# shift count must be an `int`, so the
count gets an `(int)` cast:

<!-- source: src/tests/Behavioral/NamedTypeBitwiseConst/main.go:25-62 -->
```go
type word uint64
…
func maskFor(s uint) word {
	m := word(1)<<s - 1
	return m
}
```
<!-- source: src/tests/Behavioral/NamedTypeBitwiseConst/main.cs.target:21-50 -->
```csharp
[GoType("num:uint64")] partial struct word;
…
internal static word maskFor(nuint s) {
    var m = (((word)1) << (int)(s)) - 1;
    return m;
}
```

Here `m` is a `word`, because both `<<` and `-` return a `word`. Go's `uint` is C#'s `nuint`.

**A conversion goes through the underlying type.** Among Go's number types, the wrapper converts only to and
from its exact underlying type. It also accepts golib's untyped-constant wrappers, described in the next rules.
So `float64(sec)` in `Seconds` becomes `(float64)(int64)sec`: first to `int64`, then to `float64`. This gives
the same value as Go's conversion. The same rule applies in both directions:

<!-- source: src/tests/Behavioral/NamedNumericConversion/main.go:14-40 -->
```go
type traceArg uint64 // underlying uint64
…
type nameOff int32   // underlying int32
…
	var procs int32 = 5
	a := traceArg(procs) // int32 -> traceArg: ((traceArg)(uint64)procs)
…
	var s nameOff = 7
	e := uint64(s) // nameOff(int32) -> uint64: ((uint64)(int32)s)
```
<!-- source: src/tests/Behavioral/NamedNumericConversion/main.cs.target:7-29 -->
```csharp
[GoType("num:uint64")] partial struct traceArg;
…
[GoType("num:int32")] partial struct nameOff;
…
    int32 procs = 5;
    var a = ((traceArg)(uint64)procs);
…
    nameOff s = 7;
    var e = (uint64)(int32)s;
```

When the other type already is the underlying type, one cast is enough: `int32(s)` becomes `(int32)s`.

**An `iota` group becomes one property per name.** This is Go's usual way to write an enumeration. Inside a
`const` group, `iota` counts 0, 1, 2 and so on, and a line with no expression repeats the one before it. In C#
each value is computed ahead of time, and the first keeps its Go expression as a comment. The type is renamed
`ΔMonth` because `Time` also has a `Month()` method, and both live in `time_package`.

<!-- source: GOROOT/src/time/time.go:320-335 -->
```go
type Month int

const (
	January Month = 1 + iota
	February
	March
	…
	December
)
```
<!-- source: src/core/time/time.cs:324-337 -->
```csharp
[GoType("num:nint")] partial struct ΔMonth;

public static ΔMonth January => /* 1 + iota */ 1;
public static ΔMonth February => 2;
public static ΔMonth March => 3;
…
public static ΔMonth December => 12;
```

**A constant takes the type Go gives it in context.** An untyped Go constant, one declared with no type,
becomes a property of [`UntypedInt`](../src/core/golib/UntypedInt.cs). That is a struct from golib, the
go2cs runtime library ([The golib Runtime Library](#the-golib-runtime-library)), and it converts to any Go
integer type ([Constant Values](#constant-values)). Go gives such a constant the type of the operand beside
it. Where the operand beside it fixes that type, the converter adds a cast to it, so C# picks the same
operator Go does:

<!-- source: src/tests/Behavioral/NamedTypeBitwiseConst/main.go:10-16 -->
```go
type Tag uint8

const classConstructed = 0x20 // untyped const
const classContext = 0x80

func (t Tag) Constructed() Tag { return t | classConstructed }
func (t Tag) Context() Tag     { return t | classContext }
```
<!-- source: src/tests/Behavioral/NamedTypeBitwiseConst/main.cs.target:7-19 -->
```csharp
[GoType("num:uint8")] partial struct Tag;

internal static UntypedInt classConstructed => 0x20;

internal static UntypedInt classContext => 0x80;

public static Tag Constructed(this Tag t) {
    return (Tag)(t | (uint8)classConstructed);
}

public static Tag Context(this Tag t) {
    return (Tag)(t | (uint8)classContext);
}
```

The cast names the underlying `uint8`, not `Tag`. `uint8` converts to `Tag` by itself, so
`t | (uint8)classConstructed` uses `Tag`'s own `|` operator and the result is a `Tag`, as in Go. The outer
`(Tag)` cast restates Go's result type.

The same rule covers function arguments. Go's built-in `min` takes arguments of one type, so an untyped
constant argument is cast to the type of the others. In C#, `min` comes from golib's `builtin` class, which
every converted file can call without a prefix ([Built-in Functions](#built-in-functions)). Here `uintptr`
is golib's struct for Go's `uintptr` ([`unsafe.Pointer` and `uintptr`](#unsafepointer-and-uintptr)):

<!-- source: src/tests/Behavioral/MinMaxBuiltin/main.go:31-35 -->
```go
const limit = 128 << 10 // untyped
…
func clampU(n uintptr) uintptr { return min(n, limit) }
```
<!-- source: src/tests/Behavioral/MinMaxBuiltin/main.cs.target:7-13 -->
```csharp
internal static UntypedInt limit => /* 128 << 10 */ 131072;
…
internal static uintptr clampU(uintptr n) {
    return min(n, (uintptr)(limit));
}
```

**Values print as Go prints them.** A constant expression of a named type keeps that type after it is
computed. So `8 * time.Hour` stays a `Duration`, and `fmt.Println` prints it through `Duration`'s own `String`
method as `8h0m0s`, not as a count of nanoseconds
([detail](ConversionStrategies-Reference/floating-point-formatting.md#a-folded-constant-of-a-named-type-carries-its-type-in-the-fold)).
In this program the C# alias for the `time` package is `Δtime`. The `Δ` marks a rename: the program's blank
import of `time/tzdata` also brings a C# namespace named `time` into scope. `"4:"u8` is a Go string literal
([Strings](#strings-string-and-sstring)).

<!-- source: src/tests/Behavioral/PackageNameShadowing/main.go:42-67 -->
```go
func foldedConstant() (time.Duration, float64, int) {
	d := 8 * time.Hour
	return d, d.Seconds(), int((8 * time.Hour).Seconds())
}
…
	d, secs, east := foldedConstant()
	fmt.Println("4:", d, secs, east)
```
<!-- source: src/tests/Behavioral/PackageNameShadowing/main.cs.target:4-37 -->
```csharp
using Δtime = time_package;
…
internal static (Δtime.Duration, float64, nint) foldedConstant() {
    var d = (Δtime.Duration)(28800000000000L);
    return (d, d.Seconds(), (nint)((Δtime.Duration)(28800000000000L)).Seconds());
}
…
    var (d, secs, east) = foldedConstant();
    fmt.Println((@string)"4:"u8, d, secs, east);
```

Both programs print `4: 8h0m0s 28800 28800`.

**Full detail:** [Reference → Named Numeric Types and Constant Contexts](ConversionStrategies-Reference/named-numeric-types.md#named-numeric-types-and-constant-contexts) — the less common operators and casts, conversions between assemblies, constants too large for 64 bits, and named boolean and complex types.

---

## Nil and Zero Values

In Go, every variable starts at its type's zero value. `nil` is the zero value of pointers, slices, maps,
channels, functions and interfaces. Converted C# writes most zero values, and most `nil`s, as `default!`.

`default!` is C#'s default value for the target type. The `!` tells the compiler that a null here is
intended. The [golib runtime library](#the-golib-runtime-library) types give that default value Go's nil
behavior, so reading a nil map, or taking `len` of a nil slice, works as it does in Go.

A few Go forms need something other than `default!`. Each one has its own rule in this section:

| Go | C# | Why |
|---|---|---|
| `var s []byte`, a returned `nil` | `default!` | golib types treat C#'s default as Go's nil. |
| `p == nil`, or `nil` passed to a pointer parameter | `Ꮡp == nil`, `f(nil)` | golib's `nil` matches both forms of a nil pointer. |
| `var x any = (*int)(nil)` | `any x = ((ж<nint>)nil);` | The interface keeps the type `*int`. |
| `var dpi any = dp`, for a pointer `dp` | `any dpi = dp.OrTypedNil();` | The same, for a pointer variable. |
| `var a [5]int` | `array<nint> a = new(5);` | The array needs its length. |
| `var z holder`, a struct holding an array | `holder z = new();` | C#'s `default` skips field initializers. |
| `var e Embedded`, a struct that embeds a type | `Embedded e = new(nil);` | The embedded field lives in a heap box. |
| `var ar <-chan int` | `/*<-*/channel<nint> ar = /*<-*/channel<nint>.RecvOnly;` | The nil channel keeps its direction. |

**A variable declared without a value is `default!`, and `== nil` becomes `== default!`.** A Go slice is a
golib [`slice<T>`](#slices-and-arrays), which is nil when it has no backing array. So `[]byte{}` is empty
but not nil, as in Go. Here `probe` prints its arguments:

<!-- source: src/tests/Behavioral/SliceNilVsEmpty/main.go:10-19 -->
```go
func probe(name string, isNil bool, length, capacity int) {
	fmt.Println(name, isNil, length, capacity)
}

func main() {
	var zero []byte
	probe("zeroValue", zero == nil, len(zero), cap(zero))

	emptyLit := []byte{}
	probe("emptyLiteral", emptyLit == nil, len(emptyLit), cap(emptyLit))
```
<!-- source: src/tests/Behavioral/SliceNilVsEmpty/main.cs.target:8-14,33-37 -->
```csharp
internal static void probe(@string name, bool isNil, nint length, nint capacity) {
    fmt.Println(name, isNil, length, capacity);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string zeroValueˢ = "zeroValue"u8;
private static readonly @string emptyLiteralˢ = "emptyLiteral"u8;
…
internal static void Main() {
    slice<byte> zero = default!;
    probe(zeroValueˢ, zero == default!, len(zero), cap(zero));
    var emptyLit = new byte[]{}.slice();
    probe(emptyLiteralˢ, emptyLit == default!, len(emptyLit), cap(emptyLit));
```

Go and C# both print `zeroValue true 0 0`, then `emptyLiteral false 0 0`. `@string` is Go's `string`, and
`nint` is Go's `int` ([Integer Types](#integer-types-and-arithmetic)). `"…"u8` is a C# UTF-8 literal. Each
name ending in `ˢ` is a string literal stored once in a static field
([Strings](#strings-string-and-sstring)). `.slice()` wraps a C# array as a non-nil golib slice.

**Each nil value behaves as it does in Go.** The golib types, pointers included, give their nil value these
behaviors:

| Nil value | What happens |
|---|---|
| slice | `len` and `cap` are 0, and `append` works ([Slices and Arrays](#slices-and-arrays)). |
| map | golib's `map<K, V>` is a struct with no dictionary behind it, so nothing throws a C# null error. Reads return the zero value, `len` is 0, `range` is empty and `delete` does nothing. A write panics ([Maps](#maps)). |
| channel | A send or receive blocks forever, and in a `select` its case is never chosen ([Channels](#channels-and-select)). |
| pointer | Comparing it is safe. A method can be called on it, because a method is a C# extension method, which accepts a null receiver ([Functions and Methods](#functions-and-methods)). Reading or writing through it panics with Go's `invalid memory address or nil pointer dereference`, which `recover` catches ([Defer / Panic / Recover](#defer--panic--recover)). |
| interface | `== nil` is true only when it holds nothing at all, not even a nil pointer ([Interfaces](#interfaces)). |

**A pointer compared with `nil`, or passed `nil` as an argument, uses golib's `nil`.** A Go pointer `*T`
becomes golib's `ж<T>` heap box (read "zhe"). A pointer parameter `p` is named `Ꮡp` in C#: the `Ꮡ` prefix
marks a name that holds a box. When a body reads through the pointer, C# reaches the value by `ref`, either
as a `ref var p` local or as a `ref T p` parameter ([Pointers](#pointers),
[glyph table](#reading-converted-code-names-and-glyphs)). The functions below only compare or forward the
pointer, so they keep the box:

<!-- source: src/tests/Behavioral/DeadPointerParamAlias/main.go:5,9-14,17-18 -->
```go
type node struct{ val int }
…
func onlyNilCheck(p *node) string {
	if p == nil {
		return "nil"
	}
	return "set"
}
…
func inner(p *node) bool      { return p == nil }
func passThrough(p *node) bool { return inner(p) }
```
<!-- source: src/tests/Behavioral/DeadPointerParamAlias/main.cs.target:7-28,34-38 -->
```csharp
[GoType] partial struct node {
    internal nint val;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string nilˢ = "nil"u8;
private static readonly @string setˢ = "set"u8;

internal static @string onlyNilCheck(ж<node> Ꮡp) {
    if (Ꮡp == nil) {
        return nilˢ;
    }
    return setˢ;
}

internal static bool inner(ж<node> Ꮡp) {
    return Ꮡp == nil;
}

internal static bool passThrough(ж<node> Ꮡp) {
    return inner(Ꮡp);
}
…
internal static void Main() {
    fmt.Println(onlyNilCheck(nil));
    fmt.Println(onlyNilCheck(Ꮡ(new node(nil))));
    fmt.Println(passThrough(nil));
    fmt.Println(passThrough(Ꮡ(new node(nil))));
```

`passThrough` forwards its pointer to `inner`, which only compares it with `nil`. Go's `main` calls
`onlyNilCheck` and `passThrough` each once with `nil` and once with `&node{}`. Go and C# both print `nil`,
`set`, `true`, `false`. `&node{}` becomes `Ꮡ(new node(nil))`: `new node(nil)` builds a zero-value `node`,
and `Ꮡ(…)` boxes it and takes its address. `[GoType]` marks a struct that a
[source generator](#source-generators) completes. `nilˢ` and `setˢ` are hoisted string literals
([Strings](#strings-string-and-sstring)).

golib has two forms of a nil pointer. `default!` gives a C# `null`. Converting golib's `nil` to a pointer
type gives one shared nil box for that type. Go sees no difference, and comparing with golib's `nil` treats
both as nil, so pointer comparisons use it.

So a nil pointer that is declared, assigned or returned stays `default!`: `var nilNode *node` becomes
`ж<node> nilNode = default!;`. A `nil` passed to a pointer parameter stays golib's `nil`, which hands the
function the shared nil box. A returned `nil` error is an interface, so it is also `default!`.

**A nil pointer stored in an interface keeps its type.** In Go, `any((*int)(nil))` is not nil, and `%T`
prints `*int`. A C# `null` carries no type, so the converter stores the shared nil box instead. Converting
`nil` to a pointer type, as in `((ж<nint>)nil)`, gives that box directly:

<!-- source: src/tests/Behavioral/TypedNilInterface/TypedNilInterface.go:26-29 -->
```go
	// Bare any boxing of a typed nil keeps the type.
	var x any = (*int)(nil)
	fmt.Printf("%T\n", x)
	fmt.Println("x==nil", x == nil)
```
<!-- source: src/tests/Behavioral/TypedNilInterface/TypedNilInterface.cs.target:24,69-71 -->
```csharp
private static readonly object xNilˢ = (@string)"x==nil"u8;
…
    any x = ((ж<nint>)nil);
    fmt.Printf("%T\n"u8, x);
    fmt.Println(xNilˢ, x == default!);
```

Go and C# both print `*int`, then `x==nil false`. A literal that is only ever passed as `any`, such as an
argument to `fmt.Println`, is stored already boxed, as an `object` field
([Empty Interface](#empty-interface-any)). Other literals, like the `"%T\n"u8` format string, stay inline
([Strings](#strings-string-and-sstring)).

A pointer variable holds a plain `default!`, which carries no type. So a pointer variable entering an
interface passes through golib's `OrTypedNil()`, which swaps a C# `null` for the shared nil box:

<!-- source: src/tests/Behavioral/TypedNilInterface/TypedNilInterface.go:79,88-90 -->
```go
	var dp *int
…
	// var spec, then plain assignment
	var dpi any = dp
	fmt.Println("dpi==nil", dpi == nil, "dpi==x", dpi == x)
```
<!-- source: src/tests/Behavioral/TypedNilInterface/TypedNilInterface.cs.target:38-39,101,107-108 -->
```csharp
private static readonly object dpiNilˢ = (@string)"dpi==nil"u8;
private static readonly object dpiXˢ = (@string)"dpi==x"u8;
…
    ж<nint> dp = default!;
…
    any dpi = dp.OrTypedNil();
    fmt.Println(dpiNilˢ, dpi == default!, dpiXˢ, AreEqual(dpi, x));
```

Go and C# both print `dpi==nil false dpi==x true`. Here `x` is the interface from the previous example. An
interface is a C# `object`, so `dpi == default!` asks whether it holds nothing at all. C#'s `==` between two
objects compares only references, so two interface values are compared with golib's `AreEqual`, which
compares type and value as Go does ([Interfaces](#interfaces)).

<!-- A nil pointer to an array also keeps the array's length, because reflection can read N from a nil
*[N]T: `var a any = (*[3]byte)(nil)` becomes `any a = ж<array<byte>>.NilBoxOfDims(3L);`
(src/tests/Behavioral/TypedNilPtrArrayDims/main.cs.target:25). Removed from the summary body as
first-read detail; it belongs on the reference page. -->

**A fixed-size array is constructed, and so is a struct that holds one.** Go's `[N]T` becomes golib's
[`array<T>`](#slices-and-arrays), which keeps its length in the instance. So `var a [5]int` becomes
`array<nint> a = new(5);`, and a struct field `tbl [8]int` gets the field initializer `= new(8)`.

C#'s `default` runs no constructor and skips field initializers. So a struct holding an array becomes
`new()`, which runs them. A struct of plain fields stays `default!`:

<!-- source: src/tests/Behavioral/ZeroValueStructVar/main.go:16-31,34-35,48,54 -->
```go
type holder struct {
	name string
	tbl  [8]int
	tail []int
}

type wrapper struct {
	id int
	h  holder
}

type point struct {
	x, y int
}

func main() {
…
	var z holder
	fmt.Println(len(z.name), len(z.tbl), len(z.tail)) // 0 8 0
…
	var w wrapper
…
	var p point
```
<!-- source: src/tests/Behavioral/ZeroValueStructVar/main.cs.target:7-24,33,36 -->
```csharp
[GoType] partial struct holder {
    internal @string name;
    internal array<nint> tbl = new(8);
    internal slice<nint> tail;
}

[GoType] partial struct wrapper {
    internal nint id;
    internal holder h;
}

[GoType] partial struct point {
    internal nint x, y;
}

internal static void Main() {
    holder z = new();
    fmt.Println(len(z.name), len(z.tbl), len(z.tail));
…
    wrapper w = new();
…
    point p = default!;
```

`wrapper` is constructed too, because its `holder` field needs construction. `[GoType]` marks a struct that a
[source generator](#source-generators) completes, including its constructors ([Struct Types](#struct-types)).

**A struct that embeds another type is constructed with `new(nil)`.** The embedded field lives in a heap box.
C# reaches it through a `ref` property of the same name, and only a constructor allocates the box
([Struct Type Embedding](#struct-type-embedding)). The generator gives every struct a constructor that takes
golib's `nil` and builds the zero value. `new(nil)` calls it. Here `Padded` is a struct of plain fields, so
it stays `default!`, and `Embedded` embeds it:

<!-- source: src/tests/Behavioral/UnsafeOperations/UnsafeOperations.go:33-40,44-48,140-141 -->
```go
type Padded struct {
	flag  bool
	count int64
	tag   byte
	name  string
	data  []byte
	code  int32
}
…
type Embedded struct {
	lead byte
	Padded
	trail int16
}
…
	var p Padded
	var e Embedded
```
<!-- source: src/tests/Behavioral/UnsafeOperations/UnsafeOperations.cs.target:29-42,108-109 -->
```csharp
[GoType] partial struct Padded {
    internal bool flag;
    internal int64 count;
    internal byte tag;
    internal @string name;
    internal slice<byte> data;
    internal int32 code;
}

[GoType] partial struct Embedded {
    internal byte lead;
    public partial ref Padded Padded { get; }
    internal int16 trail;
}
…
    Padded p = default!;
    Embedded e = new(nil);
```

**A directional channel's zero value keeps its direction.** Go's `chan T`, `<-chan T` and `chan<- T` all
become golib's `channel<T>`, which has no direction in its C# type. The `/*<-*/` comment only shows where
Go's arrow was. So the nil value records the direction instead: `channel<T>.RecvOnly` is a nil
receive-only channel, and `channel<T>.SendOnly` is a nil send-only one
([Channels and `select`](#channels-and-select)):

<!-- source: src/tests/Behavioral/ReflectChanNarrowing/main.go:58-59 -->
```go
	var ar <-chan int
	var as chan<- int
```
<!-- source: src/tests/Behavioral/ReflectChanNarrowing/main.cs.target:50-51 -->
```csharp
    /*<-*/channel<nint> ar = /*<-*/channel<nint>.RecvOnly;
    channel/*<-*/<nint> @as = channel/*<-*/<nint>.SendOnly;
```

`@as` is `as` with C#'s `@` prefix, because `as` is a C# keyword. A struct field of directional channel type
gets the same value as its field initializer. A plain `chan T` stays `default!`.

**Full detail:** [Reference → Nil and Zero Values](ConversionStrategies-Reference/nil-and-zero-values.md#nil-and-zero-values) — how each golib type treats its nil value, how a typed nil is kept at every point where a pointer enters an interface, and how reflection reads it.

---

## Built-in Functions

Go's built-in functions keep their Go names in C#. `len`, `append`, `make` and the rest are static
methods of the [`builtin`](../src/core/golib/builtin.cs) class in [golib](#the-golib-runtime-library),
the go2cs runtime library. Every converted project imports that class with a global
`using static go.builtin`, so a call like `len(x)` reads the same in C# as it does in Go.

A few built-ins change shape, because C# needs them written differently. This table lists every
built-in and where it is explained in more depth:

| Go | C# | More in |
|---|---|---|
| `len(x)`, `cap(x)` | `len(x)`, `cap(x)`. Both return `nint`, which is how go2cs writes Go's `int`. The `len` of a string counts bytes, as in Go | [Slices and Arrays](#slices-and-arrays), [Strings](#strings-string-and-sstring) |
| `append(s, a, b)` | `append(s, a, b)` | [Slices and Arrays](#slices-and-arrays) |
| `append(s, t...)`, with `t` a slice | `appendꓸꓸꓸ(s, t)`. The `ꓸꓸꓸ` glyph stands for Go's `...` | [Reading Converted Code](#reading-converted-code-names-and-glyphs) |
| `copy(dst, src)` | `copy(dst, src)` | [Slices and Arrays](#slices-and-arrays) |
| `clear(x)` | `clear(x)`. It zeroes a slice's elements and empties a map | [Slices and Arrays](#slices-and-arrays), [Maps](#maps) |
| `make(T, …)` | a constructor: `new slice<T>(…)`, `new map<K, V>()` or `new channel<T>(…)` | [Slices and Arrays](#slices-and-arrays), [Maps](#maps), [Channels and `select`](#channels-and-select) |
| `new(T)` | `@new<T>()`, which returns a `ж<T>` (read "zhe"): golib's heap box for a Go pointer. The `@` lets C# use `new` as a name | [Pointers](#pointers), [Reading Converted Code](#reading-converted-code-names-and-glyphs) |
| `delete(m, k)` | `delete(m, k)` | [Maps](#maps) |
| `close(ch)` | `close(ch)` | [Channels and `select`](#channels-and-select) |
| `min(…)`, `max(…)` | `min(…)`, `max(…)` | — |
| `complex(r, i)`, `real(c)`, `imag(c)` | the same calls, over `complex64` or `complex128` | [Constant Values](#constant-values) |
| `print(…)`, `println(…)` | the same calls. They write to standard error, as Go's do | — |
| `panic(v)` | `throw panic(v)`. `panic` builds the exception, and `throw` tells C# that the code path ends here | [Defer / Panic / Recover](#defer--panic--recover) |
| `recover()` | `recover()` | [Defer / Panic / Recover](#defer--panic--recover) |

**Most calls do not change.** The arguments and their order stay as Go wrote them. Here a Go `[]byte`
literal becomes a C# array wrapped as a golib `slice<byte>` by `.slice()`
([Slices and Arrays](#slices-and-arrays)), and Go's `int` becomes `nint`, C#'s native-size integer
([Integer Types and Arithmetic](#integer-types-and-arithmetic)). The converter sometimes writes the
type Go infers, as in `nint n`; that declares the same variable `var` would:

<!-- source: src/tests/Behavioral/MinMaxBuiltin/main.go:73-76 -->
```go
x := []byte{1, 2, 3}
y := []byte{1, 2, 3, 4, 5}
n := min(len(x), len(y))
fmt.Println(n) // 3
```
<!-- source: src/tests/Behavioral/MinMaxBuiltin/main.cs.target:39-42 -->
```csharp
var x = new byte[]{1, 2, 3}.slice();
var y = new byte[]{1, 2, 3, 4, 5}.slice();
nint n = min(len(x), len(y));
fmt.Println(n);
```

**`make` becomes a constructor.** A Go slice, map or channel is a golib type in C#: `slice<T>`,
`map<K, V>` or `channel<T>`. So `make` becomes `new` on that type, and its size arguments pass
through unchanged. An unbuffered channel gets an explicit capacity of `0`:

<!-- source: src/tests/Behavioral/AppendOfMake/AppendOfMake.go:40 + AppendOfMake.cs.target:55 · src/tests/Behavioral/AnyKeyMap/AnyKeyMap.go:8 + AnyKeyMap.cs.target:9 · src/tests/Behavioral/ChannelCapLen/main.go:11,24 + main.cs.target:8,20 -->

| Go | C# |
|---|---|
| `s := make([]int, 2, 4)` | `var s = new slice<nint>(2, 4);` |
| `tbl = make(map[any]string)` | `tbl = new map<any, @string>();` |
| `ch := make(chan int, 3)` | `var ch = new channel<nint>(3);` |
| `u := make(chan string)` | `var u = new channel<@string>(0);` |

`@string` is go2cs's Go `string` ([Strings](#strings-string-and-sstring)). `any` is a global C#
alias for `object` that stands for Go's empty interface ([Empty Interface](#empty-interface-any)).

**Inside a generic function, `make` becomes golib's `make<S>(…)`.** When the type being made is a type
parameter, C# cannot call `new` with arguments on it. So the converter calls golib's `make`, passing
the type parameter. Here `S` is the slice type parameter, written `S ~[]E` in Go
([Generics](#generics)). The `where` lines tell C# what `S` and `E` can do; the long constraint for `E`
is omitted:

<!-- source: src/tests/Behavioral/GenericTypeInference/GenericTypeInference.go:26-32 -->
```go
func Scale[S ~[]E, E Integer](s S, c E) S {
	r := make(S, len(s))
	for i, v := range s {
		r[i] = v * c
	}
	return r
}
```
<!-- source: src/tests/Behavioral/GenericTypeInference/GenericTypeInference.cs.target:31-40 -->
```csharp
public static S Scale<S, E>(S s, E c)
    where S : /* ~[]E */ ISlice<E>, ISupportMake<S>, ISliceWrap<S, E>, new()
…
{
    var r = make<S>(len(s));
    foreach (var (i, v) in s) {
        r[i] = v * c;
    }
    return r;
}
```

The `for … range` loop becomes a C# `foreach` over index and value
([Loops, Range and Labels](#loops-range-and-labels)).

**Spreading a slice into `append` uses `appendꓸꓸꓸ`.** In Go, `append(s, m...)` adds every element of
`m`. In C#, the call is named `appendꓸꓸꓸ`, and the whole slice `m` passes as one argument. The `ꓸꓸꓸ`
glyph in a name stands for Go's `...` ([Reading Converted Code](#reading-converted-code-names-and-glyphs)).
This complete function shows `make` and the spread together:

<!-- source: src/tests/Behavioral/AppendOfMake/AppendOfMake.go:32-35 -->
```go
func named(s []int, n int) []int {
	m := make([]int, n)
	return append(s, m...)
}
```
<!-- source: src/tests/Behavioral/AppendOfMake/AppendOfMake.cs.target:34-37 -->
```csharp
internal static slice<nint> named(slice<nint> s, nint n) {
    var m = new slice<nint>(n);
    return appendꓸꓸꓸ(s, m);
}
```

**Appending `make(...)...` directly uses `makeꓸꓸꓸ`.** Go treats `append(s, make([]T, n)...)` as "grow
`s` by `n` zero elements" and never builds the temporary slice. In C#, `makeꓸꓸꓸ<T>(n)` returns a
small golib value that holds only the count. An `appendꓸꓸꓸ` overload takes that value and grows `s`
by `n` zero elements, so nothing extra is allocated. Converted code writes this form; you never need
to write it yourself:

<!-- source: src/tests/Behavioral/AppendOfMake/AppendOfMake.go:10-12 -->
```go
func extend(s []int, n int) []int {
	return append(s, make([]int, n)...)
}
```
<!-- source: src/tests/Behavioral/AppendOfMake/AppendOfMake.cs.target:7-9 -->
```csharp
internal static slice<nint> extend(slice<nint> s, nint n) {
    return appendꓸꓸꓸ(s, makeꓸꓸꓸ<nint>(n));
}
```

**`new(T)` returns a heap box.** Go's `new(T)` gives a pointer to a zeroed `T`. In C#, `@new<T>()`
returns a `ж<T>` (read "zhe"): golib's heap box, which stands in for a Go pointer
([Pointers](#pointers)). Here `Holder` is a struct with a `Name string` field. Go reaches the field
through the pointer automatically. C# reaches it through the box's `.Value`
([Implicit Pointer Dereferencing](#implicit-pointer-dereferencing)):

<!-- source: src/tests/Behavioral/CtorFieldInitializerOmitted/main.go:22-23 -->
```go
b := new(Holder)
b.Name = "a"
```
<!-- source: src/tests/Behavioral/CtorFieldInitializerOmitted/main.cs.target:15-16 -->
```csharp
var b = @new<Holder>();
b.Value.Name = "a"u8;
```

The `"a"u8` is a C# UTF-8 literal, the form go2cs uses for Go string literals
([Strings](#strings-string-and-sstring)).

**A method named like a built-in makes calls write `builtin.<name>`.** All functions and methods of a
Go package are members of one static C# class ([Functions and Methods](#functions-and-methods)). C#
looks in that class before it looks at `using static go.builtin`. So a method named `len` would hide
the built-in `len` for the whole package, including in functions like `freeLen` that have nothing to
do with the method's type. The converter therefore writes `builtin.len(…)` for each real built-in call
([detail](ConversionStrategies-Reference/shadowing.md#short-variable-redeclaration-shadowing)):

<!-- source: src/tests/Behavioral/ReservedNameShadows/main.go:53-62 -->
```go
type box struct{ items []int }
…
func (b *box) len() int { return len(b.items) + 1 }

func freeLen() int {
	s := []int{1, 2, 3}
	return len(s)
}
```
<!-- source: src/tests/Behavioral/ReservedNameShadows/main.cs.target:57-68 -->
```csharp
[GoType] partial struct box {
    internal slice<nint> items;
}

[GoRecv] internal static nint len(this ref box b) {
    return builtin.len(b.items) + 1;
}

internal static nint freeLen() {
    var s = new nint[]{1, 2, 3}.slice();
    return builtin.len(s);
}
```

`[GoType]` marks a converted Go type ([Struct Types](#struct-types)). `[GoRecv]` marks a method, and
`this ref box b` is its receiver: the method gets the `box` by reference, which is how go2cs writes a
pointer receiver. The method call itself stays `b.len()`.

**A local that shadows a built-in needs no prefix.** When Go code declares a local variable or
function named like a built-in, such as `make := func(…)`, Go calls the local too. So C# emits a
plain call to the local, and no `builtin.` prefix is needed.

**Full detail:** [Reference → Slices and Arrays](ConversionStrategies-Reference/slices-and-arrays.md#slices-and-arrays) — how `make`, `append`, `copy` and `clear` build, grow and zero elements, the spread forms and the append-of-make form, and their edge cases.

---

## Empty Interface (`any`)

Go's empty interface, `interface{}` or `any`, becomes C# `any`. In C#, `any` is an alias for `object` that
every converted project declares, so the converted code keeps Go's spelling. A value is boxed with its Go
type wherever it enters an `any`. Type assertions, type switches and `==` then see the same dynamic type
that Go sees.

**A type assertion becomes `._<T>()`.** Go's `x.(T)` becomes `x._<T>()`, a golib extension method on
`object`. It returns the value when the dynamic type is `T`. Otherwise it panics with Go's own
`interface conversion` message. Go's `int` is C# `nint`, the native-sized integer, so `.(int)` becomes
`._<nint>()` ([Integer Types and Arithmetic](#integer-types-and-arithmetic)):

<!-- source: src/tests/Behavioral/UntypedIntInterfaceBox/main.go:33-34 -->
```go
var a any = 7
fmt.Println(a.(int))
```
<!-- source: src/tests/Behavioral/UntypedIntInterfaceBox/main.cs.target:40-41 -->
```csharp
any a = (nint)(7);
fmt.Println(a._<nint>());
```

**An untyped constant boxes at its Go default type.** In Go, the `7` stored in an `any` is an `int`, so
the converter writes `(nint)(7)`. A bare C# `7` would box as a 32-bit `Int32`, and the later `.(int)`
would panic. The same rule lets a [type switch](#type-switch-statements) pick `case int:` rather than
`case int32:`.

**A string literal boxes as a Go `string`.** Go's `string` is golib's
[`@string`](#strings-string-and-sstring), a byte string; the `@` only escapes the C# keyword. A literal
stored in an `any` is cast to `@string`, so `x.(string)` and `case string:` match it. `"hello"u8` is a C#
UTF-8 literal, and `(@string)` turns it into a Go string.

**Most string literals are also created only once.** The converter [hoists](#strings-string-and-sstring)
each distinct literal into one `static readonly` field. The field is declared just above the first
function in the package that uses it, and its generated name ends in `ˢ`. So `"hello"` becomes the field
`helloˢ`. Some literals stay inline, such as an element of a composite literal: `[]any{"keep"}` keeps
`(@string)"keep"u8` in place. The generated comment gives the reason for hoisting. Go keeps string
literals in read-only memory (RODATA) at no run-time cost, so the C# allocates each literal only once.

**A hoisted literal whose every use in the package is an `any` is stored already boxed.** Its field has
type `object`. Every argument of `fmt.Println` is an `any`, because `Println` takes `...any`. If any use
needs a plain `string`, the field is a `@string` instead ([Strings](#strings-string-and-sstring)). Here
each literal feeds only `var i interface{}` or `Println`, so all five fields are `object`:

<!-- source: src/tests/Behavioral/TypeAssert/TypeAssert.go:34-50 -->
```go
func safeAssertions() {
	var i interface{} = "hello"

	// Type assertion with ok check
	if s, ok := i.(string); ok {
		fmt.Println("Value is a string:", s)
	} else {
		fmt.Println("Value is not a string")
	}

	// This will not panic, just set ok to false
	if n, ok := i.(int); ok {
		fmt.Println("Value is an int:", n)
	} else {
		fmt.Println("Value is not an int")
	}
}
```
<!-- source: src/tests/Behavioral/TypeAssert/TypeAssert.cs.target:36-59 -->
```csharp
// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object helloˢ = (@string)"hello"u8;
private static readonly object valueIsAStringˢ = (@string)"Value is a string:"u8;
private static readonly object valueIsNotAStringˢ = (@string)"Value is not a string"u8;
private static readonly object valueIsAnIntˢ = (@string)"Value is an int:"u8;
private static readonly object valueIsNotAnIntˢ = (@string)"Value is not an int"u8;

internal static void safeAssertions() {
    any i = helloˢ;
    {
        var (s, ok) = i._<@string>(ᐧ); if (ok){
            fmt.Println(valueIsAStringˢ, s);
        } else {
            fmt.Println(valueIsNotAStringˢ);
        }
    }
    {
        var (n, ok) = i._<nint>(ᐧ); if (ok){
            fmt.Println(valueIsAnIntˢ, n);
        } else {
            fmt.Println(valueIsNotAnIntˢ);
        }
    }
}
```

**The comma-ok form passes `ᐧ`.** `ᐧ` is golib's constant `true`. Its value is ignored: the extra argument
only selects the overload of `_<T>` that returns a `(value, ok)` tuple instead of panicking. That is
Go's `v, ok := x.(T)`. The extra braces keep `s`, `n` and `ok` scoped to their `if`, like Go's `if …; ok`
form ([Multi-Result Values and Comma-Ok Forms](#multi-result-values-and-comma-ok-forms)).

**`==` on interfaces compares dynamic type, then value.** It becomes a call to golib's `AreEqual`. Two
values are equal only when both their Go types and their values match, as in Go
([Interfaces](#interfaces)). `AreEqual` appears unqualified because every converted project imports
golib's builtins with `using static` ([Built-in Functions](#built-in-functions)):

<!-- source: src/tests/Behavioral/InterfaceUntypedIntCompare/InterfaceUntypedIntCompare.go:42-44 -->
```go
var x any = 42
fmt.Println(x == 42) // true
fmt.Println(x != 42) // false
```
<!-- source: src/tests/Behavioral/InterfaceUntypedIntCompare/InterfaceUntypedIntCompare.cs.target:33-35 -->
```csharp
any x = (nint)(42);
fmt.Println(AreEqual(x, (nint)(42)));
fmt.Println(!AreEqual(x, (nint)(42)));
```

The right-hand `42` is also an untyped constant compared with an `any`. Go gives it its default type
`int`, so it becomes `(nint)(42)`. As a bare `Int32`, it would never equal the `nint` inside `x`.

Like Go, comparing two interfaces that hold the same uncomparable type, such as two slices or two maps,
panics at run time. A test against `nil` stays a plain `x == default!`. `default!` is C#'s null for
`object`; the `!` tells the compiler the null is intended ([Nil and Zero Values](#nil-and-zero-values)).

**A pointer keeps its box.** Go stores the pointer itself in an interface, not a copy of the value it
points to. A Go pointer `*pp` is golib's `ж<pp>` (read "zhe"), a heap box ([Pointers](#pointers)). This
small program keeps a free list of `any` values, puts a `*pp` into it, and asserts it back out:

<!-- source: src/tests/Behavioral/PointerValueToInterfaceArg/main.go:14-63 -->
```go
type pp struct {
	id  int
	buf []byte
}

var freeList []any

func poolPut(x any) { freeList = append(freeList, x) } // x is the EMPTY interface

func poolGet() any {
	if n := len(freeList); n > 0 {
		x := freeList[n-1]
		freeList = freeList[:n-1]
		return x
	}
	return new(pp)
}
…
func keep(q *pp) { poolPut(q) }
…
	c := &pp{id: 100}
	keep(c)                // poolPut(c): stores the *pp box
	got := poolGet().(*pp) // must be c (same pointer), not a copy
	got.id = 200
	fmt.Println(c.id) // 200 — proves got IS c
```
<!-- source: src/tests/Behavioral/PointerValueToInterfaceArg/main.cs.target:7-59 -->
```csharp
[GoType] partial struct pp {
    internal nint id;
    internal slice<byte> buf;
}

internal static slice<any> freeList;

internal static void poolPut(any x) {
    freeList = append(freeList, x);
}

internal static any poolGet() {
    {
        nint n = len(freeList); if (n > 0) {
            var x = freeList[n - 1];
            freeList = freeList[..(int)(n - 1)];
            return x;
        }
    }
    return @new<pp>();
}
…
internal static void keep(ж<pp> Ꮡq) {
    poolPut(Ꮡq.OrTypedNil());
}
…
    var c = Ꮡ(new pp(id: 100));
    keep(c);
    var got = poolGet()._<ж<pp>>();
    got.Value.id = 200;
    fmt.Println((~c).id);
```

The Go struct `pp` becomes a C# struct marked `[GoType]` ([Struct Types](#struct-types)). `slice<T>` is
golib's Go slice, and `freeList[..(int)(n - 1)]` is Go's `freeList[:n-1]`
([Slices and Arrays](#slices-and-arrays)). golib's `@new<pp>()` is Go's `new(pp)`: it returns a box
holding a zero `pp`.

**`Ꮡ` has two uses here.** As a call, `Ꮡ(…)` is Go's `&`, so `Ꮡ(new pp(id: 100))` is `&pp{id: 100}`. As a
name prefix, it marks a pointer parameter, so `q` becomes `Ꮡq`. A pointer parameter that arrives as a box
takes the prefix. That leaves the plain Go name free for the pointed-to value when a body needs it
([Implicit Pointer Dereferencing](#implicit-pointer-dereferencing)). (A small private helper that only
dereferences its pointer can take `ref T` instead; see [Pointers](#pointers).) A local such as `c` keeps
its plain name.

**`keep` passes the box itself into the `any`.** So asserting it back with `._<ж<pp>>()` returns the same
pointer, and writing `got.id` changes `c.id`. `OrTypedNil()` keeps a nil `*pp` typed inside the
interface, as Go does ([detail](ConversionStrategies-Reference/pointers.md#a-pointer-value-passed-to-an-any-argument-takes-the-box)).

**`~` reads through a pointer, `.Value` writes through it.** `~c` returns a copy of the value `c` points
to, like Go's `*c`. `got.Value` is a `ref` to the real storage, so `got.Value.id = 200` writes into
the shared `pp` ([Implicit Pointer Dereferencing](#implicit-pointer-dereferencing)).

The [glyph table](#reading-converted-code-names-and-glyphs) lists every mark used here.

**Full detail:** [Reference → Empty Interface (`any`)](ConversionStrategies-Reference/empty-interface.md#empty-interface-any) — where string literals and untyped constants are boxed, and when comparing two interfaces panics.

---

## Multi-Assignment and Evaluation Order

Go's parallel assignment, `a, b = x, y`, becomes a C# tuple deconstruction, `(a, b) = (x, y)`. Both languages read every right-hand value before they store any, so the converted line behaves like the Go line and reads like it.

In the examples, Go's `int` appears as C# `nint`, a native-sized integer ([Integer Types and Arithmetic](#integer-types-and-arithmetic)).

**Reassigning several variables is one tuple.** Every value on the right is read first, and only then are the targets written. That is why a swap needs no temporary in either language. Here `left` and `right` are package-level variables.

<!-- source: src/tests/Behavioral/ParallelAssignmentHazard/main.go:49-59 -->
```go
	left, right = right, left
	fmt.Println(left, right) // 20 10
…
var left = 10
var right = 20
```
<!-- source: src/tests/Behavioral/ParallelAssignmentHazard/main.cs.target:31-42 -->
```csharp
    (left, right) = (right, left);
    fmt.Println(left, right);
…
internal static nint left = 10;

internal static nint right = 20;
```

**Fields, elements and pointer targets join the tuple too.** Written as separate stores, a swap would lose a value: the second store would read what the first one just wrote. So a struct field, a slice element or a target reached through a pointer is assigned in the same tuple as a plain variable. Here `e` is a pointer to an `edge`, while `f` and `g` are plain `edge` values.

<!-- source: src/tests/Behavioral/ParallelAssignmentHazard/main.go:38-56 -->
```go
	e := &edge{out: 7, arg: 9}
	e.out, e.arg = e.arg, e.out
	fmt.Println(e.out, e.arg) // 9 7   (not 9 9)

	// Field swap on a VALUE struct, plus a cross-struct rotate reading pre-assignment values.
	f := edge{out: 1, arg: 2}
	g := edge{out: 3, arg: 4}
	f.out, g.out, f.arg = g.out, f.arg, f.out
	fmt.Println(f.out, f.arg, g.out, g.arg) // 3 1 2 4
…
type edge struct {
	out int
	arg int
}
```
<!-- source: src/tests/Behavioral/ParallelAssignmentHazard/main.cs.target:24-38 -->
```csharp
    var e = Ꮡ(new edge(@out: 7, arg: 9));
    (e.Value.@out, e.Value.arg) = (e.Value.arg, e.Value.@out);
    fmt.Println((~e).@out, (~e).arg);
    var f = new edge(@out: 1, arg: 2);
    var g = new edge(@out: 3, arg: 4);
    (f.@out, g.@out, f.arg) = (g.@out, f.arg, f.@out);
    fmt.Println(f.@out, f.arg, g.@out, g.arg);
…
[GoType] partial struct edge {
    internal nint @out;
    internal nint arg;
}
```

A few things in the C# come from other sections:

- `Ꮡ(…)` is Go's `&`. It places the value in a heap box from golib, the go2cs runtime library. So `e` has type `ж<edge>`, golib's pointer type, even though `var` hides it ([Pointers](#pointers)).
- `e.Value` and `~e` both reach the struct that `e` points to. `e.Value` is a C# `ref` to the real struct, so the converter uses it where the struct is written. `~e` returns a copy, which is enough where the struct is only read ([Implicit Pointer Dereferencing](#implicit-pointer-dereferencing)).
- `@out` is the Go field `out`; `@` escapes a C# keyword ([Reading Converted Code](#reading-converted-code-names-and-glyphs)).
- `[GoType] partial struct` is a Go struct type ([Struct Types](#struct-types)).

**A `:=` that reuses a name declares only the new ones.** Go's `c, d := two(c)` assigns the existing `c` and declares a new `d`. The converter puts a declaration (`var`, or an explicit type such as `nint`) only in front of each new element. So one tuple both assigns `c` and declares `d`.

<!-- source: src/tests/Behavioral/PartialRedeclaration/main.go:10-26 -->
```go
func two(x int) (int, int) {
	return x, x * 10
}
…
func redeclareLocal() int {
	c := 5
	c, d := two(c) // c reused (a local), d new
	return c + d
}
```
<!-- source: src/tests/Behavioral/PartialRedeclaration/main.cs.target:7-23 -->
```csharp
internal static (nint, nint) two(nint x) {
    return (x, x * 10);
}
…
internal static nint redeclareLocal() {
    nint c = 5;
    (c, var d) = two(c);
    return c + d;
}
```

A Go function with several results returns a C# tuple, so its call deconstructs directly ([Multi-Result Values and Comma-Ok Forms](#multi-result-values-and-comma-ok-forms)). A `:=` in an inner block that reuses an outer name makes a new variable instead; that case is covered in [Short Variable Redeclaration (Shadowing)](#short-variable-redeclaration-shadowing).

**A `:=` becomes a tuple when a later value re-reads an earlier target.** In `x, frac := x>>3, x&7`, Go computes `frac` from the old `x`. Two separate C# statements would compute it from the new `x`. So the converter keeps the statement as one tuple. When no value re-reads a target, as with `m, n` here, the converter writes plain separate statements, which is what most `:=` lines look like.

<!-- source: src/tests/Behavioral/ParallelAssignmentHazard/main.go:13-30 -->
```go
	x := uint64(0b1011010) // 90
	x, frac := x>>3, x&7
	fmt.Println(x, frac) // 11 2  (2 = 90&7, NOT 3 = 11&7)
…
	m := 5
	m, n := m+1, 100
	fmt.Println(m, n) // 6 100
```
<!-- source: src/tests/Behavioral/ParallelAssignmentHazard/main.cs.target:8-23 -->
```csharp
    var x = (uint64)0b1011010;
    (x, var frac) = ((x >> (int)(3)), (uint64)(x & 7));
    fmt.Println(x, frac);
…
    nint m = 5;
    m = m + 1;
    nint n = 100;
    fmt.Println(m, n);
```

`uint64` is the go2cs alias for C#'s `ulong`, so `(uint64)` is an ordinary C# cast. The cast on `x & 7` states Go's result type; for a `ulong` it changes nothing. The `(int)` is there because a C# shift count must be an `int`, while Go accepts any integer type ([Integer Types and Arithmetic](#integer-types-and-arithmetic)).

**In a tuple of separate values, a new Go `int` is declared as `nint`, not `var`.** In a mixed tuple, `var` would infer a 32-bit C# `int` from a plain literal such as `100`, not Go's native-sized `nint`. So the converter always writes the explicit type for a new `int` (or `uint`) element there. When the right side is one call, as in `(c, var d) = two(c)` above, the call's result type already fixes each element, so `var` is safe there. This example also re-reads `a` and `b` on the right, so it stays one tuple for the same reason as `x, frac`.

<!-- source: src/tests/Behavioral/ParallelAssignmentHazard/main.go:18-20 -->
```go
	a, b := 10, 20
	a, b, c := b, a, a+b
	fmt.Println(a, b, c) // 20 10 30
```
<!-- source: src/tests/Behavioral/ParallelAssignmentHazard/main.cs.target:11-14 -->
```csharp
    nint a = 10;
    nint b = 20;
    (a, b, nint c) = (b, a, a + b);
    fmt.Println(a, b, c);
```

**A multi-value `return` runs its calls before it reads plain operands.** Go's spec runs the calls in a `return` left to right, but leaves open when a plain operand such as `c.n` is read. The standard Go compiler, gc, always runs the calls first and reads the plain operands after. go2cs matches gc, because that is what Go programs observe.

A C# tuple reads strictly left to right. So when a call can change an earlier operand, the converter first moves the call into a temporary. The temporary is a local whose name starts with `ᴛ`, the glyph go2cs uses for names it makes up ([Reading Converted Code](#reading-converted-code-names-and-glyphs)). Its number is only a counter that keeps names unique in the file; it has no other meaning.

Here `bump` has a pointer receiver, so it changes the caller's `c`.

<!-- source: src/tests/Behavioral/MultiValueReturnOrder/main.go:38-51 -->
```go
type counter struct {
	n int
}

func (c *counter) bump() int {
	c.n++
	return c.n
}

// readThenBump reads a FIELD of the value the later call mutates.
func readThenBump() (int, int) {
	var c counter
	return c.n, c.bump()
}
```
<!-- source: src/tests/Behavioral/MultiValueReturnOrder/main.cs.target:22-35 -->
```csharp
[GoType] partial struct counter {
    internal nint n;
}

[GoRecv] internal static nint bump(this ref counter c) {
    c.n++;
    return c.n;
}

internal static (nint, nint) readThenBump() {
    counter c = default!;
    var ᴛ2 = c.bump();
    return (c.n, ᴛ2);
}
```

Go's `readThenBump` returns `1 1`. Without the temporary, `return (c.n, c.bump())` would copy `c.n` while it is still `0` and return `0 1`. The same shape in a real parser would return an empty result beside a nil error.

In the C#, a pointer-receiver method becomes an extension method on `ref counter`, marked `[GoRecv]`; the `ref` is what lets `bump` change the caller's `c` ([Functions and Methods](#functions-and-methods)). `default!` is C#'s default value, here Go's zero `counter`. The `!` matters only for reference types, where it tells the compiler a null is intended; the converter writes it everywhere ([Nil and Zero Values](#nil-and-zero-values)).

**Most returns stay one tuple, as written in Go.** A call can change an earlier operand only when it gets that operand's address. That happens through a pointer-receiver method on it, an `&x` argument, or a pointer the operand is read through. The converter checks just those cases, within the `return` statement itself. When the call works on something else, as here, nothing moves.

<!-- source: src/tests/Behavioral/MultiValueReturnOrder/main.go:93-98 -->
```go
// unrelatedOperand's call touches a different variable entirely.
func unrelatedOperand() (int, int) {
	var a counter
	var b counter
	return a.n, b.bump()
}
```
<!-- source: src/tests/Behavioral/MultiValueReturnOrder/main.cs.target:74-78 -->
```csharp
internal static (nint, nint) unrelatedOperand() {
    counter a = default!;
    counter b = default!;
    return (a.n, b.bump());
}
```

A method with a value receiver never moves either: it works on a copy, so the caller cannot see any change.

**Full detail:** [Reference → Multi-Assignment and Evaluation Order](ConversionStrategies-Reference/multi-assignment.md#multi-assignment-and-evaluation-order) — the full rules for which targets make a tuple, the edge cases of a mixed `:=`, and exactly when a `return` moves its calls into temporaries.

---

## Short Variable Redeclaration (Shadowing)

Go's short variable declaration `:=` can reuse a name in two ways. In the same scope, it reuses the
existing variable and declares only the new names. In a nested block, it declares a new variable that
"shadows" the outer one. Only the second case needs a new name in C#.

Converted Go functions are static methods of the package's C# class, such as `main_package`
(see [Package Conversion](#package-conversion)). In the C# examples, `nint` is Go's `int`
(see [Integer Types and Arithmetic](#integer-types-and-arithmetic)).

**Reusing a variable in the same scope needs no rename.** In Go, `c, d := two(c)` assigns the existing
`c` and declares only `d`. The C# tuple assignment writes `var` on the new element alone, so `c` stays
the same variable.

<!-- source: src/tests/Behavioral/PartialRedeclaration/main.go:10-26 -->
```go
func two(x int) (int, int) {
	return x, x * 10
}
…
func redeclareLocal() int {
	c := 5
	c, d := two(c) // c reused (a local), d new
	return c + d
}
```
<!-- source: src/tests/Behavioral/PartialRedeclaration/main.cs.target:7-23 -->
```csharp
internal static (nint, nint) two(nint x) {
    return (x, x * 10);
}
…
internal static nint redeclareLocal() {
    nint c = 5;
    (c, var d) = two(c);
    return c + d;
}
```

Go's multiple results become a C# tuple
(see [Multi-Result Values and Comma-Ok Forms](#multi-result-values-and-comma-ok-forms)).

**A variable that shadows one in an enclosing block is renamed.** In Go, the inner variable hides the
outer one until its block ends, and the outer variable keeps its own value. C# does not allow this.
A C# local's scope covers its whole block, so a nested local may not reuse the name of any local in an
enclosing block.

So the converter renames the inner (shadowing) variable. It appends `Δ` and a number: `z` becomes `zΔ1`.
The outer variable keeps its Go name. Every use inside the inner scope is rewritten to the new name, so
the two variables behave exactly as they do in Go. The `Δ` glyph is listed in
[Reading Converted Code: Names and Glyphs](#reading-converted-code-names-and-glyphs).

**A nested variable is renamed even when the outer one comes later.** In this function the nested
`var z` appears first in the source, and the function-level `z := x * x` comes after it. Go accepts this.
C# checks the whole block regardless of order, so the nested `z` becomes `zΔ1`.

<!-- source: src/tests/Behavioral/NestedVarShadow/main.go:14-26 -->
```go
func f(x int) int {
	if x >= 2 {
		var z int // nested `var` decl; collides with the later function-level z
		if x > 5 {
			z = 100
		} else {
			z = 200
		}
		return z
	}
	z := x * x // function-level, declared textually AFTER the nested `var z`
	return z
}
```
<!-- source: src/tests/Behavioral/NestedVarShadow/main.cs.target:8-20 -->
```csharp
internal static nint f(nint x) {
    if (x >= 2) {
        nint zΔ1 = default!;
        if (x > 5){
            zΔ1 = 100;
        } else {
            zΔ1 = 200;
        }
        return zΔ1;
    }
    nint z = x * x;
    return z;
}
```

The Go `var z int` starts at its zero value. C# writes that as `default!`: the type's default value,
with `!` telling the compiler a null is intended. The converter writes it for every type, including
`nint` (see [Nil and Zero Values](#nil-and-zero-values)).

**Each further shadow of the same name takes the next number.** The numbers count the renames of one
name within one function, in source order. Here `v` and `ok` are declared three times: at function level,
in an `if` statement's init, and in the `for` loop body. The function-level pair keeps its names. The `if`
pair comes first in the source and becomes `vΔ1, okΔ1`. The loop-body pair becomes `vΔ2, okΔ2`.

<!-- source: src/tests/Behavioral/NestedVarShadow/main.go:118-137 -->
```go
func forwardOk() int {
	m := map[string]int{"a": 1, "b": 2, "c": 3}
	total := 0
	v, ok := m["a"]
	if ok {
		total += v
	}
	for n := 0; n < 2; n++ {
		if v, ok := m["b"]; ok {
			total += v
			continue
		}
		v, ok := m["c"]
		if !ok {
			break
		}
		total += v
	}
	return total
}
```
<!-- source: src/tests/Behavioral/NestedVarShadow/main.cs.target:106-127 -->
```csharp
internal static nint forwardOk() {
    var m = new map<@string, nint>{["a"u8] = 1, ["b"u8] = 2, ["c"u8] = 3};
    nint total = 0;
    var (v, ok) = m["a"u8, ꟷ];
    if (ok) {
        total += v;
    }
    for (nint n = 0; n < 2; n++) {
        {
            var (vΔ1, okΔ1) = m["b"u8, ꟷ]; if (okΔ1) {
                total += vΔ1;
                continue;
            }
        }
        var (vΔ2, okΔ2) = m["c"u8, ꟷ];
        if (!okΔ2) {
            break;
        }
        total += vΔ2;
    }
    return total;
}
```

A few other things in this example have their own sections:

- `map<@string, nint>` is golib's Go map, and `"a"u8` is a Go string literal (see [Maps](#maps) and [Strings](#strings-string-and-sstring)).
- `m["b"u8, ꟷ]` is the comma-ok lookup `v, ok := m["b"]`; `ꟷ` selects the form that also returns `ok` (see [Multi-Result Values and Comma-Ok Forms](#multi-result-values-and-comma-ok-forms)).
- The extra braces around the C# `if` keep `vΔ1` and `okΔ1` scoped to that `if`, as they are in Go.

**A local named like a built-in the function calls is renamed, and the call keeps its name.** Go's
built-ins, such as `len` and `cap`, are static methods of the [golib](#the-golib-runtime-library)
[`builtin`](../src/core/golib/builtin.cs) class. Converted code imports that class with `using static`,
so the calls read as in Go (see [Built-in Functions](#built-in-functions)). A C# local named `cap`
would hide the `cap` method, so the local becomes `capΔ1`.

<!-- source: src/tests/Behavioral/BuiltinShadowLocal/main.go:29-32 -->
```go
func capPlusOne(s []int) int {
	cap := cap(s)
	return cap + 1
}
```
<!-- source: src/tests/Behavioral/BuiltinShadowLocal/main.cs.target:19-22 -->
```csharp
internal static nint capPlusOne(slice<nint> s) {
    nint capΔ1 = cap(s);
    return capΔ1 + 1;
}
```

Here `slice<nint>` is golib's Go slice, `[]int` (see [Slices and Arrays](#slices-and-arrays)).

**A local that shadows a package-level variable is renamed, and earlier reads of the global name the
package class.** Package-level variables become static fields of the package's C# class, here
`main_package` (see [Package Conversion](#package-conversion)). In Go, a local's scope starts after its
declaration, so the first line of this function still reads the global `plainCounter`. In C#, a bare
`plainCounter` would bind to the local for the whole method.

<!-- source: src/tests/Behavioral/GlobalShadowedByLocal/main.go:25-35 -->
```go
var plainCounter = 100 // a plain global (no collision rename)
…
func plainGlobalShadow() int {
	x := plainCounter * 2 // reads GLOBAL plainCounter (200) before the local decl
	plainCounter := 5     // local shadows the global
	return x + plainCounter // 200 + 5 = 205
}
```
<!-- source: src/tests/Behavioral/GlobalShadowedByLocal/main.cs.target:5-124 -->
```csharp
partial class main_package {
…
internal static nint plainCounter = 100;

internal static nint plainGlobalShadow() {
    nint x = main_package.plainCounter * 2;
    nint plainCounterΔ1 = 5;
    return x + plainCounterΔ1;
}
…
} // end main_package
```

The read of the global is written `main_package.plainCounter`, which no local can hide. The converter
also renames the local to `plainCounterΔ1`, so the global and the local never share a C# name inside
the function. The function returns 205, as in Go. The Go comment's "collision rename" is a different
rule, for a package-level name that clashes with a method name; it does not apply here.

**Full detail:** [Reference → Short Variable Redeclaration (Shadowing)](ConversionStrategies-Reference/shadowing.md#short-variable-redeclaration-shadowing) — more shadowing cases, including package names, constants, closures and names that clash with methods.

---

## Multi-Result Values and Comma-Ok Forms

A Go function can return several values. A C# method returns one, so the converted function returns a
C# value tuple, and the caller takes it apart with a deconstruction: `var (a, b) = f();`. Go's comma-ok
forms, such as `v, ok := m[k]`, call a second [golib](#the-golib-runtime-library) overload that returns
a `(value, ok)` tuple.

In the examples, `nint` is Go's `int` ([Integer Types](#integer-types-and-arithmetic)) and `@string` is
Go's `string` ([Strings](#strings-string-and-sstring)). `default!` is C#'s default value, which golib
uses for Go's zero value and for `nil` ([Nil and Zero Values](#nil-and-zero-values)).

**A multi-result function returns a value tuple.** `return a, b` becomes `return (a, b);`, and
`a, b := f()` becomes `var (a, b) = f();`. The results here are named `_`, Go's blank identifier, so the
C# tuple elements stay unnamed:

<!-- source: src/tests/Behavioral/BlankMultiResult/main.go:8-26 -->
```go
func match(x, y int) (_, _ int) {
	if x > y {
		return y, x
	}
	return x, y
}
…
	a, b := match(5, 2)
	fmt.Println(a, b) // 2 5
```
<!-- source: src/tests/Behavioral/BlankMultiResult/main.cs.target:7-23 -->
```csharp
internal static (nint, nint) match(nint x, nint y) {
    if (x > y) {
        return (y, x);
    }
    return (x, y);
}
…
    var (a, b) = match(5, 2);
    fmt.Println(a, b);
```

**Named results keep their names.** The C# tuple names its elements as Go does. When the body uses a
named result, or has a bare `return`, that result is also a local, declared at the top of the function
with its zero value, `default!`. A result that is named only for documentation keeps its name on the
tuple type and gets no local. A bare `return` returns the locals' current values. Here is the whole of
`io.ReadAtLeast`:

<!-- source: GOROOT/src/io/io.go:329-344 -->
```go
func ReadAtLeast(r Reader, buf []byte, min int) (n int, err error) {
	if len(buf) < min {
		return 0, ErrShortBuffer
	}
	for n < min && err == nil {
		var nn int
		nn, err = r.Read(buf[n:])
		n += nn
	}
	if n >= min {
		err = nil
	} else if n > 0 && err == EOF {
		err = ErrUnexpectedEOF
	}
	return
}
```
<!-- source: src/core/io/io.cs:341-361 -->
```csharp
public static (nint n, error err) ReadAtLeast(Reader r, slice<byte> buf, nint min) {
    nint n = default!;
    error err = default!;

    if (len(buf) < min) {
        return (0, ErrShortBuffer);
    }
    while (n < min && err == default!) {
        nint nn = default!;
        (nn, err) = r.Read(buf[(int)(n)..]);
        n += nn;
    }
    if (n >= min){
        err = default!;
    } else 
    if (n > 0 && AreEqual(err, EOF)) {
        err = ErrUnexpectedEOF;
    }
    return (n, err);
}
```

`nn, err = r.Read(…)` assigns to variables that already exist, so its deconstruction
`(nn, err) = …` has no `var`. The converter writes `else if` across two lines; it is an ordinary
`else if`.

The other names come from other sections. `slice<byte>` is Go's `[]byte`, and `buf[(int)(n)..]` is Go's
`buf[n:]` ([Slices and Arrays](#slices-and-arrays)). `err == default!` is Go's `err == nil`. Comparing two
interface values, as in `err == EOF`, goes through golib's `AreEqual`, which follows Go's `==` rules
([Interfaces](#interfaces)).

**When `:=` declares only some of its names**, only the new names get `var`. Go's `:=` may reuse a
variable that already exists, as long as at least one name is new
([Short Variable Redeclaration](#short-variable-redeclaration-shadowing)). C# allows a deconstruction to
mix new and existing variables, so `rbr2, err := …` becomes `(var rbr2, err) = …;`:

<!-- source: src/tests/Behavioral/TupleMixedDeclareReassign/main.go:49-71 -->
```go
func makeCursor(pos int, tag string) (cursor, error) {
	return cursor{pos: pos, tag: tag}, nil
}
…
func streams() (int, int) {
	rbr1, err := makeCursor(10, "one")
	if err != nil {
		return 0, 0
	}
	rbr2, err := makeCursor(20, "two")
	if err != nil {
		return 0, 0
	}
…
```
<!-- source: src/tests/Behavioral/TupleMixedDeclareReassign/main.cs.target:47-67 -->
```csharp
internal static (cursor, error) makeCursor(nint pos, @string tag) {
    return (new cursor(pos: pos, tag: tag), default!);
}
…
// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string oneˢ = "one"u8;
private static readonly @string twoˢ = "two"u8;

internal static (nint, nint) streams() {
    var (rbr1, err) = makeCursor(10, oneˢ);
    if (err != default!) {
        return (0, 0);
    }
    (var rbr2, err) = makeCursor(20, twoˢ);
    if (err != default!) {
        return (0, 0);
    }
…
```

`oneˢ` and `twoˢ` are the string literals `"one"` and `"two"`. The converter moves each into a
`static readonly` field, so it is created only once ([Strings](#strings-string-and-sstring)). RODATA, in
the generated comment, is Go's read-only data section.

**A multi-value `return` keeps Go's order.** Go runs the calls in a `return` first, and reads the plain
values beside them afterwards. A C# tuple evaluates left to right instead. So when a call could change a
value beside it, the converter runs the call first, into a temporary. A name starting with `ᴛ` is a
temporary the converter makes up ([Names and Glyphs](#reading-converted-code-names-and-glyphs)):

<!-- source: src/tests/Behavioral/MultiValueReturnOrder/main.go:66-74 -->
```go
func addressArgument() (int, int) {
	n := 0
	return n, raise(&n)
}

func raise(n *int) int {
	*n += 10
	return *n
}
```
<!-- source: src/tests/Behavioral/MultiValueReturnOrder/main.cs.target:52-61 -->
```csharp
internal static (nint, nint) addressArgument() {
    nint n = 0;
    var ᴛ5 = raise(ref n);
    return (n, ᴛ5);
}

internal static nint raise(ref nint n) {
    n += 10;
    return n;
}
```

Both results are 10, as in Go. Without the temporary, C# would read `n` before `raise` changes it, and
return 0 and 10. Here the pointer parameter `*int` becomes `ref nint`, and `&n` becomes `ref n`
([Pointers](#pointers)). The numbers after `ᴛ` only keep the names unique and carry no meaning.

**A multi-result call passed straight to another call goes through temporaries.** Go lets `f(g())` pass
all of `g`'s results as `f`'s arguments. C# cannot pass one tuple as several arguments, so the converter
deconstructs `g()` into `ᴛ` temporaries first:

<!-- source: src/tests/Behavioral/TupleSpreadIntoCall/TupleSpreadIntoCall.go:16-52 -->
```go
func parts() (int, int) {
	return 3, 4
}
…
func combine(a, b int) int {
	return a*10 + b
}
…
	s := combine(parts())
```
<!-- source: src/tests/Behavioral/TupleSpreadIntoCall/TupleSpreadIntoCall.cs.target:11-42 -->
```csharp
internal static (nint, nint) parts() {
    return (3, 4);
}
…
internal static nint combine(nint a, nint b) {
    return a * 10 + b;
}
…
    var (ᴛ3, ᴛ4) = parts();
    nint s = combine(ᴛ3, ᴛ4);
```

**A comma-ok form calls a second overload.** In Go, a map read, a channel receive and a type assertion
give one value, or a value and a `bool` when assigned to two variables. C# cannot overload on return type
alone, so golib adds an overload with one extra `bool` argument that returns `(value, ok)`.

The extra argument's value is never read; it only makes C# pick the comma-ok overload. Map reads and
channel receives pass `ꟷ`, golib's constant `false`. Type assertions pass `ᐧ`, golib's constant `true`.
The two spellings are a convention and behave the same.

| Go operation | Single value | Comma-ok |
|---|---|---|
| Map read `m[k]` | `m[k]` | `m[k, ꟷ]` |
| Channel receive `<-ch` | `ᐸꟷ(ch)` | `ᐸꟷ(ch, ꟷ)` |
| Type assertion `x.(T)` | `x._<T>()` | `x._<T>(ᐧ)` |

A failed single-value assertion panics as in Go, and `recover` can catch it
([Defer / Panic / Recover](#defer--panic--recover)). A failed comma-ok form returns the zero value and
`false` instead.

**Map read:** `ok` reports whether the key is present, and an absent key gives the zero value. This
whole program shows each way to use the result:

<!-- source: src/tests/Behavioral/MapCommaOk/main.go:14-39 -->
```go
func main() {
	m := map[string]int{"a": 1, "b": 2}

	// value + ok, present.
	v, ok := m["a"]
	fmt.Println(v, ok) // 1 true

	// value + ok, absent (value is the zero value).
	v2, ok2 := m["z"]
	fmt.Println(v2, ok2) // 0 false

	// blank value, present.
	_, ok3 := m["b"]
	fmt.Println(ok3) // true

	// if-init comma-ok with a blank value.
	if _, ok4 := m["z"]; !ok4 {
		fmt.Println("z absent") // printed
	}

	// reassignment (not a new declaration) into existing variables.
	var w int
	var present bool
	w, present = m["b"]
	fmt.Println(w, present) // 2 true
}
```
<!-- source: src/tests/Behavioral/MapCommaOk/main.cs.target:7-27 -->
```csharp
// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object zAbsentˢ = (@string)"z absent"u8;

internal static void Main() {
    var m = new map<@string, nint>{["a"u8] = 1, ["b"u8] = 2};
    var (v, ok) = m["a"u8, ꟷ];
    fmt.Println(v, ok);
    var (v2, ok2) = m["z"u8, ꟷ];
    fmt.Println(v2, ok2);
    var (_, ok3) = m["b"u8, ꟷ];
    fmt.Println(ok3);
    {
        var (_, ok4) = m["z"u8, ꟷ]; if (!ok4) {
            fmt.Println(zAbsentˢ);
        }
    }
    nint w = default!;
    bool present = default!;
    (w, present) = m["b"u8, ꟷ];
    fmt.Println(w, present);
}
```

`map<K, V>` is golib's map ([Maps](#maps)). `"a"u8` is a C# UTF-8 string literal, which converts to
`@string` without a cast. `zAbsentˢ` is the string literal `"z absent"`, moved into a `static readonly`
field so it is created once ([Strings](#strings-string-and-sstring)). The field is typed `object` because
the literal is only passed as an `any` value, so the `(@string)` cast makes it a Go string before it is
stored. A blank value stays `_`, and an assignment to existing variables has no `var`.

**Channel receive:** `ok` is `false` once the channel is closed and empty. `channel<T>` is golib's
channel. Sending is a method on the channel, `d.ᐸꟷ(7)`, because the value goes into it. Receiving is a
function, `ᐸꟷ(d)`, because the value comes out of it. `ᐸꟷ` is drawn to look like Go's `<-`; the `ꟷ`
inside the name is part of the name, not the comma-ok argument
([Channels and `select`](#channels-and-select)). This prints `7 true`, `8 true`, then `0 false`:

<!-- source: src/tests/Behavioral/ChannelCapLen/main.go:30-39 -->
```go
	d := make(chan int, 2)
	d <- 7
	d <- 8
	close(d)
	v, ok := <-d
	fmt.Println(v, ok)
	v, ok = <-d
	fmt.Println(v, ok)
	v, ok = <-d
	fmt.Println(v, ok)
```
<!-- source: src/tests/Behavioral/ChannelCapLen/main.cs.target:24-33 -->
```csharp
    var d = new channel<nint>(2);
    d.ᐸꟷ(7);
    d.ᐸꟷ(8);
    close(d);
    var (v, ok) = ᐸꟷ(d, ꟷ);
    fmt.Println(v, ok);
    (v, ok) = ᐸꟷ(d, ꟷ);
    fmt.Println(v, ok);
    (v, ok) = ᐸꟷ(d, ꟷ);
    fmt.Println(v, ok);
```

**Type assertion:** `ok` reports whether the interface holds the asserted type. `x.(T)` becomes golib's
`x._<T>()`, and the extra `ᐧ` picks the comma-ok form. `any` is Go's empty interface, an alias for C#'s
`object` ([Empty Interface](#empty-interface-any)). The names ending in `ˢ` are the string literals from
the Go code, each moved into a `static readonly` field typed `object`, as in the map example:

<!-- source: src/tests/Behavioral/TypeAssert/TypeAssert.go:34-50 -->
```go
func safeAssertions() {
	var i interface{} = "hello"

	// Type assertion with ok check
	if s, ok := i.(string); ok {
		fmt.Println("Value is a string:", s)
	} else {
		fmt.Println("Value is not a string")
	}

	// This will not panic, just set ok to false
	if n, ok := i.(int); ok {
		fmt.Println("Value is an int:", n)
	} else {
		fmt.Println("Value is not an int")
	}
}
```
<!-- source: src/tests/Behavioral/TypeAssert/TypeAssert.cs.target:36-59 -->
```csharp
// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object helloˢ = (@string)"hello"u8;
private static readonly object valueIsAStringˢ = (@string)"Value is a string:"u8;
private static readonly object valueIsNotAStringˢ = (@string)"Value is not a string"u8;
private static readonly object valueIsAnIntˢ = (@string)"Value is an int:"u8;
private static readonly object valueIsNotAnIntˢ = (@string)"Value is not an int"u8;

internal static void safeAssertions() {
    any i = helloˢ;
    {
        var (s, ok) = i._<@string>(ᐧ); if (ok){
            fmt.Println(valueIsAStringˢ, s);
        } else {
            fmt.Println(valueIsNotAStringˢ);
        }
    }
    {
        var (n, ok) = i._<nint>(ᐧ); if (ok){
            fmt.Println(valueIsAnIntˢ, n);
        } else {
            fmt.Println(valueIsNotAnIntˢ);
        }
    }
}
```

**An `if` with a comma-ok initializer keeps Go's scope.** Go scopes the variables of
`if s, ok := …; ok` to that `if` and its `else`. The converter wraps the declaration and the `if` in a
C# `{ … }` block, so each `ok` in `safeAssertions` is a separate variable, as in Go. The map example's
`ok4` gets the same block.

**Full detail:** [Reference → Multi-Result Values and Comma-Ok Forms](ConversionStrategies-Reference/multi-result-and-comma-ok.md#multi-result-values-and-comma-ok-forms) — how a type assertion finds its target at run time, `var a, b = f()` at package level and in grouped declarations, exactly when a named result is declared, and how arguments reach variadic parameters.

---

## Slices and Arrays

Go slices become [golib](#the-golib-runtime-library) [`slice<T>`](../src/core/golib/slice.cs), and Go
arrays become [`array<T>`](../src/core/golib/array.cs). Golib is go2cs's hand-written C# runtime library.
Both types are structs over a shared C# `T[]` backing array. A slice adds a start, a length and a
capacity. An array has a fixed length.

Indexing, `len`, `cap`, `append` and `copy` keep their Go names and read as they do in Go
([Built-in Functions](#built-in-functions)). A `range` loop becomes a `foreach` over `(index, value)`
pairs ([Loops, Range and Labels](#loops-range-and-labels)).

Go's type names stay readable too. Go's `int` appears as C# `nint`, a native-sized integer. Sized names
such as `uint32` and `int64` keep their Go spelling: each converted project declares them as aliases
of the C# types. `uintptr` is golib's own `uintptr` struct
([Integer Types and Arithmetic](#integer-types-and-arithmetic)).
<!-- uintptr is a golib struct, not a project alias: src/core/golib/uintptr.cs:39; project aliases such as `<Using Include="System.UInt32" Alias="uint32" />` (src/core/bufio/bufio.csproj:122). -->

**Literals and `make`.** A composite literal builds a plain C# array and turns it into a Go value with
`.slice()` or `.array()`. `make` calls a constructor. So `[]uint32{7, 8, 9}` becomes
`new uint32[]{7, 8, 9}.slice()`, `[6]int{0, 10, 20, 30, 40, 50}` becomes
`new nint[]{0, 10, 20, 30, 40, 50}.array()`, and `make([]int, 3, 10)` becomes `new slice<nint>(3, 10)`.
<!-- source: src/tests/Behavioral/SliceAliasing/main.go:20, :34 and :44; src/tests/Behavioral/SliceAliasing/main.cs.target:10, :18 and :24 -->

`.slice(…)` is one golib method with optional bounds. With no arguments it turns a C# array into a
`slice<T>`. With bounds it is a Go slice expression, shown in the three-index rule in this section.

**A sub-slice shares its backing array.** `s[2:5]` becomes the C# range `s[2..5]`, and `s[2:]` becomes
`s[2..]`. A bound that is not a literal is cast to `int` (see *A range bound is cast to `int`* below).
A write through either slice shows through the other, as in Go. In this example `base` is a C#
keyword, so the variable appears as `@base`
([Names and Glyphs](#reading-converted-code-names-and-glyphs)).
<!-- Every uncast range bound in src/tests/Behavioral/**/*.cs.target is a literal (`[2..5]`, `[1..]`, `[..0]`); variable bounds appear cast, e.g. SliceNilVsEmpty/main.cs.target:58 `trim[..(int)(len(trim) - 1)]`. -->

<!-- source: src/tests/Behavioral/SliceAliasing/main.go:18-26 -->
```go
base := make([]uint32, 6)
d := base[2:5]
copy(d, []uint32{7, 8, 9})
fmt.Println(base, d, len(d), cap(d))

// element writes flow both directions between base and sub-slice
d[0] = 42
base[3] = 43
fmt.Println(base[2], d[1])
```
<!-- source: src/tests/Behavioral/SliceAliasing/main.cs.target:8-14 -->
```csharp
var @base = new slice<uint32>(6);
var d = @base[2..5];
copy(d, new uint32[]{7, 8, 9}.slice());
fmt.Println(@base, d, len(d), cap(d));
d[0] = 42;
@base[3] = 43;
fmt.Println(@base[2], d[1]);
```

`d` starts at `base[2]`, so `d[0]` and `base[2]` are the same element. The second `Println` prints
`42 43` in both languages.

**`append` shares while capacity allows.** Within capacity, `append` writes into the shared backing
array. Past capacity, it allocates a new array and the result detaches, as in Go.

<!-- source: src/tests/Behavioral/SliceAliasing/main.go:55-64 -->
```go
// append within capacity writes the shared backing in place
w := base[2:4]
x := append(w, 500)
x[0] = 501
fmt.Println(base, len(x), cap(x))

// append beyond capacity reallocates and detaches
y := append(x, 1, 2, 3)
y[0] = 999
fmt.Println(base[2], y[0], len(y), cap(y))
```
<!-- source: src/tests/Behavioral/SliceAliasing/main.cs.target:32-38 -->
```csharp
var w = @base[2..4];
var x = append(w, (uint32)(500));
x[0] = 501;
fmt.Println(@base, len(x), cap(x));
var y = append(x, (uint32)(1), (uint32)(2), (uint32)(3));
y[0] = 999;
fmt.Println(@base[2], y[0], len(y), cap(y));
```

`w` has length 2 and room for two more, so `x` still shares `base`, and `x[0] = 501` changes `base[2]`.
`y` needs more room than `x` has, so `y[0] = 999` leaves `base[2]` at `501`.

The untyped constants passed to `append` are cast to the element type, here `uint32`
([Constant Values](#constant-values)). `append` is a generic C# method, and a bare `500` would read as a
C# `int` there. A plain assignment such as `x[0] = 501` needs no cast, because C# converts the constant.
<!-- append is generic over T (src/core/golib/builtin.cs:345, :367), so T is inferred from every argument. -->

**A range bound is cast to `int`.** C# ranges take `int`, but Go's `int` is `nint`. So every range bound
that is not an integer literal is cast: `trim = trim[:len(trim)-1]` becomes
`trim = trim[..(int)(len(trim) - 1)];`. A literal bound, as in `@base[2..5]`, needs no cast.
<!-- source: src/tests/Behavioral/SliceNilVsEmpty/main.go:53 and src/tests/Behavioral/SliceNilVsEmpty/main.cs.target:58 -->

**A three-index slice calls `.slice(low, high, max)`.** A C# range has no capacity bound, so
`h := base[1:3:4]` becomes `var h = @base.slice(1, 3, 4);`. An omitted bound is passed as `-1`, which
means "use Go's default", so `arr[:n:n]` starts at 0. `.slice` takes Go's `int` (`nint`), so an `int`
bound passes as is. Here `n` is a `uintptr`, so it is cast to `int`: `arr.slice(-1, (int)(n), (int)(n))`.
<!-- source: src/tests/Behavioral/SliceAliasing/main.go:51 and src/tests/Behavioral/SliceAliasing/main.cs.target:29; src/tests/Behavioral/Slice3IndexWideBound/main.go:19 and src/tests/Behavioral/Slice3IndexWideBound/main.cs.target:11 -->
<!-- A three-index bound is cast only when it is wide or unsigned: `arr[:n:n]` over a `uintptr` n becomes `arr.slice(-1, (int)(n), (int)(n))` (src/tests/Behavioral/Slice3IndexWideBound/main.go:19 and main.cs.target:11); `nint` bounds pass uncast, `var e = arr.slice(-1, k, k);` (Slice3IndexWideBound/main.cs.target:19) and `len(anys)` (src/tests/Behavioral/AppendUntypedConst/main.cs.target:28). The -1 default: src/core/golib/slice.cs:1529-1532 (missing low = 0, high = len, max = cap). -->

**A nil slice is the default value.** `var zero []byte` becomes `slice<byte> zero = default!;`, and
`zero == nil` becomes `zero == default!`. `default!` is C#'s default value for the type. For `slice<T>`
that is a slice with no backing array, which is Go's nil. The `!` only silences C#'s nullable warning.
<!-- source: src/tests/Behavioral/SliceNilVsEmpty/main.go:15-16 and src/tests/Behavioral/SliceNilVsEmpty/main.cs.target:34-35 -->
<!-- slice<T> is a struct (src/core/golib/slice.cs:89); IsNil is `m_array is null` (slice.cs:101). -->

An empty literal such as `[]byte{}` becomes `new byte[]{}.slice()`. It has a backing array, so it is not
nil, as in Go. See [Nil and Zero Values](#nil-and-zero-values).
<!-- source: src/tests/Behavioral/SliceNilVsEmpty/main.go:18 and src/tests/Behavioral/SliceNilVsEmpty/main.cs.target:36 -->

**Arrays are values.** `array<T>` is a struct over a shared `T[]`, so a plain C# copy would share the
elements. The converter adds `.Clone()` wherever Go copies an array: assignment, parameters, results and
the other places Go copies a value. So `d := garr` becomes `var d = garr.Clone();`.
<!-- source: src/tests/Behavioral/ArrayPassByValue/ArrayPassByValue.go:53 and src/tests/Behavioral/ArrayPassByValue/ArrayPassByValue.cs.target:47 -->
<!-- The array-copy claim is scoped to "Go's array copy sites": a fixed array reached only through an embedded (promoted) struct field is not seen by the value-clone stamping, so a by-value copy of such a struct leaves the array backing shared (src/go2cs/arrayCloneOperations.go:44-52, :80-83). -->

An array parameter is cloned on entry, so the callee's writes stay local, as in Go:

<!-- source: src/tests/Behavioral/ArrayPassByValue/ArrayPassByValue.go:101-107 -->
```go
// Arrays are passed by value (a full copy)
func test(a [2]string) {
	// Update to array will be local
	fmt.Println(a[0], a[1])
	a[0] = "Goodbye"
	fmt.Println(a[0], a[1])
}
```
<!-- source: src/tests/Behavioral/ArrayPassByValue/ArrayPassByValue.cs.target:84-93 -->
```csharp
// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string goodbyeˢ = "Goodbye"u8;

internal static void test([GoArrayDims(2)] array<@string> a) {
    a = a.Clone();

    fmt.Println(a[0], a[1]);
    a[0] = goodbyeˢ;
    fmt.Println(a[0], a[1]);
}
```

`goodbyeˢ` is the `"Goodbye"` literal, created once as a static field; the `ˢ` suffix marks it
([Strings](#strings-string-and-sstring)). `[GoArrayDims(2)]` records the Go array length, which
`array<T>` does not carry, for [Reflection](#reflection-reflect).

Slicing an array does not copy it. `test3(a[:])` becomes `test3(a[..])`, and a write through that slice
reaches `a`, as in Go.
<!-- source: src/tests/Behavioral/ArrayPassByValue/ArrayPassByValue.go:23 and src/tests/Behavioral/ArrayPassByValue/ArrayPassByValue.cs.target:24 -->

A `range` over an array with a value variable iterates a copy, as in Go: `for i, v := range a` becomes
`foreach (var (i, v) in a.ΔRangeSnapshot())`, so a write to `a` inside the loop does not change later
values of `v` ([Loops, Range and Labels](#loops-range-and-labels)).
<!-- source: src/tests/Behavioral/ArrayRangeSnapshot/ArrayRangeSnapshot.go:18 and src/tests/Behavioral/ArrayRangeSnapshot/ArrayRangeSnapshot.cs.target:21 -->

**A struct with array fields copies them too.** In Go, copying a struct copies its array fields. Such a
struct gets a `ΔClone()` method, and a struct copy calls it: `c := d` becomes `var c = d.ΔClone();`.
A [source generator](#source-generators) writes it. The `Δ` prefix avoids a clash with a `Clone` method
the Go type may declare ([Struct Types](#struct-types)).
<!-- source: src/tests/Behavioral/StructArrayFieldValueCopy/StructArrayFieldValueCopy.go:60 and src/tests/Behavioral/StructArrayFieldValueCopy/StructArrayFieldValueCopy.cs.target:66 -->

**Converting a slice to an array copies; converting it to an array pointer shares.** Go has both forms,
and go2cs keeps the difference:

<!-- source: src/tests/Behavioral/SliceToArrayPointerAlias/main.go:28-41 -->
```go
func writeThrough(dst []byte) {
	d := (*[4]byte)(dst)
	d[0] = 0x11
	d[1] = 0x22
	d[2] = 0x33
	d[3] = 0x44
}

// valueCopy takes the VALUE conversion and mutates it — the original must be untouched.
func valueCopy(src []byte) [4]byte {
	a := [4]byte(src)
	a[0] = 0xff
	return a
}
```
<!-- source: src/tests/Behavioral/SliceToArrayPointerAlias/main.cs.target:7-19 -->
```csharp
internal static void writeThrough(slice<byte> dst) {
    var d = Ꮡ(array<byte>.Alias(dst, 4));
    d.Value[0] = 0x11;
    d.Value[1] = 0x22;
    d.Value[2] = 0x33;
    d.Value[3] = 0x44;
}

internal static array<byte> valueCopy(slice<byte> src) {
    var a = new array<byte>(src, 4);
    a[0] = 0xff;
    return a.Clone();
}
```

`[4]byte(src)` becomes `new array<byte>(src, 4)`, a copy, so `valueCopy` never changes `src`. Its
`return a` clones too, because a result is a copy site.

`(*[4]byte)(dst)` becomes `Ꮡ(array<byte>.Alias(dst, 4))`. `Alias` is an array window over the slice's
own storage. `Ꮡ(…)` is go2cs's address-of, Go's `&`, and gives a golib pointer box `ж<array<byte>>`;
`d.Value` reads through the box ([Pointers](#pointers)). The box holds the window, and the window still
points at `dst`'s backing array, so the writes in `writeThrough` land in the caller's slice, as in Go.
<!-- array<T>.Alias windows source.m_array without copying: src/core/golib/array.cs:160-168. -->

**Variadic parameters are spans.** A Go `...T` parameter becomes `params ꓸꓸꓸT`. The glyph `ꓸꓸꓸ` stands
for Go's `...`, and `ꓸꓸꓸT` is a file-level alias for C# `Span<T>`. The parameter is renamed with a `ʗp`
suffix, which marks an incoming parameter the body redeclares, so the body can declare the Go name `xs`
as a slice ([Names and Glyphs](#reading-converted-code-names-and-glyphs)).
<!-- A type-parameter element type has no legal alias name and keeps `params Span<T>`. -->
<!-- The ʗp suffix is also used for a boxed VALUE parameter (src/go2cs/convFuncLit.go:590, :637, :704), so it is not glossed as "pack"; glyph table: docs/ConversionStrategies.md:157. -->

<!-- source: src/tests/Behavioral/VariadicPackPassThrough/VariadicPackPassThrough.go:10-19 -->
```go
func bump(xs ...int) {
	for i := range xs {
		xs[i] += 10
	}
}

// forward only passes its pack on: the view reaches bump, and bump's writes reach the caller.
func forward(xs ...int) {
	bump(xs...)
}
```
<!-- source: src/tests/Behavioral/VariadicPackPassThrough/VariadicPackPassThrough.cs.target:5-21 -->
```csharp
using ꓸꓸꓸnint = Span<nint>;

partial class main_package {

internal static void bump(params ꓸꓸꓸnint xsʗp) {
    var xs = xsʗp.sslice();

    foreach (var (i, _) in xs) {
        xs[i] += 10;
    }
}

internal static void forward(params ꓸꓸꓸnint xsʗp) {
    var xs = xsʗp.sslice();

    bump(xs.ꓸꓸꓸ);
}
```

`xsʗp.sslice()` gives golib's [`sslice<T>`](../src/core/golib/sslice.cs): a stack-only view of the span
that allocates nothing. The converter uses it when the function only reads the pack, copies from it, or
passes it on to another call, as `forward` does. Passing it on keeps it inside the call chain.

**A spread passes the slice itself.** `bump(xs...)` becomes `bump(xs.ꓸꓸꓸ)`. `.ꓸꓸꓸ` is a property on
both `slice<T>` and `sslice<T>` that returns the slice's own storage as a `Span<T>`. So the writes in
`bump` reach the caller's `a`, as in Go:

<!-- source: src/tests/Behavioral/VariadicPackPassThrough/VariadicPackPassThrough.go:61-64 -->
```go
func main() {
	a := []int{1, 2, 3}
	forward(a...)
	fmt.Println("forward wrote through:", a)
```
<!-- source: src/tests/Behavioral/VariadicPackPassThrough/VariadicPackPassThrough.cs.target:91 and :104-107 -->
```csharp
private static readonly object forwardWroteThroughˢ = (@string)"forward wrote through:"u8;
…
internal static void Main() {
    var a = new nint[]{1, 2, 3}.slice();
    forward(a.ꓸꓸꓸ);
    fmt.Println(forwardWroteThroughˢ, a);
```

Both languages print `forward wrote through: [11 12 13]`. `forwardWroteThroughˢ` is the string literal,
created once and stored as an `object` because `Println` takes `any` values
([Strings](#strings-string-and-sstring), [Empty Interface](#empty-interface-any)). Spreading into
`append` has its own name: `append(dst, xs...)` becomes `appendꓸꓸꓸ(dst, xs)`.
<!-- source: src/tests/Behavioral/VariadicPackPassThrough/VariadicPackPassThrough.go:31 and src/tests/Behavioral/VariadicPackPassThrough/VariadicPackPassThrough.cs.target:36 -->
<!-- .ꓸꓸꓸ: src/core/golib/slice.cs:490 and src/core/golib/sslice.cs:52. -->

**Any other use of the pack copies it to the heap.** When the function keeps, returns or captures its
pack, or grows it with `append` or writes into it with `copy`, the body binds `xsʗp.slice()` instead, a
heap `slice<T>` copy:

<!-- source: src/tests/Behavioral/VariadicPackPassThrough/VariadicPackPassThrough.go:53 -->
```go
func keep(xs ...int) []int { return xs }
```
<!-- source: src/tests/Behavioral/VariadicPackPassThrough/VariadicPackPassThrough.cs.target:63-67 -->
```csharp
internal static slice<nint> keep(params ꓸꓸꓸnint xsʗp) {
    var xs = xsʗp.slice();

    return xs;
}
```
<!-- The refused shapes (append INTO the pack, copy's destination, return, capture): VariadicPackPassThrough.go:44-55 and cs.target:51-74 (`into` and `copyInto` bind `xsʗp.slice()`). -->

More on variadic parameters:
[Reference → Multi-Result Values and Comma-Ok Forms](ConversionStrategies-Reference/multi-result-and-comma-ok.md#multi-result-values-and-comma-ok-forms).
<!-- Known divergence, kept out of the summary body: a callee that binds `.slice()` (for example `keep`) works on a copy, so a spread of a caller's slice into it does not let its element writes or its returned slice reach the caller's storage as Go's would. VariadicPackPassThrough.go:44-48 records it as a pre-existing divergence that test does not claim to fix. -->
<!-- Length: this section runs past the usual budget because it covers two types, their copy rules and variadic packs; the owner's review asks for complete, self-standing examples over short fragments. -->

**Full detail:** [Reference → Slices and Arrays](ConversionStrategies-Reference/slices-and-arrays.md#slices-and-arrays) — named slice and array wrappers, keyed and sparse literals, declared-length array literals, zero-value element construction, every array clone site and deep copy, and nil-versus-empty identity.

---

## Strings (`@string` and `sstring`)

Go's `string` becomes golib [`@string`](../src/core/golib/string.cs), never C#'s `System.String`.
([golib](#the-golib-runtime-library) is the go2cs runtime library, and the `@` is C#'s prefix for a name
that is also a keyword.) An `@string` is an immutable sequence of bytes, as in Go. `len`, indexing,
comparison and concatenation work on those bytes, not on UTF-16 characters.

Slicing is cheap: `s[i:j]` is a window over the same bytes, not a copy. Sharing the bytes is safe
because nothing can change them.

**A `range` loop over a string yields runes at byte offsets.** It becomes a `foreach` over
`(index, rune)` pairs. The index is the byte offset where each rune starts, exactly as in Go:

<!-- source: src/tests/Behavioral/StringByteSemantics/main.go:11-14 -->
```go
s := "a¢b☺\U0001d11e!"
for i, r := range s {
	fmt.Println(i, int32(r), int32(s[i]))
}
```
<!-- source: src/tests/Behavioral/StringByteSemantics/main.cs.target:11-14 -->
```csharp
@string s = "a¢b☺\U0001d11e!"u8;
foreach (var (i, r) in s) {
    fmt.Println(i, (int32)r, (int32)s[i]);
}
```

`¢` takes two bytes, `☺` three and `𝄞` four, so both programs print the indexes 0, 1, 3, 4, 7 and 11.
[Loops, Range and Labels](#loops-range-and-labels) covers every `range` form.

**String literals render as C# UTF-8 literals.** A Go `"text"` becomes `"text"u8`. A `u8` literal is a
`ReadOnlySpan<byte>` over bytes compiled into the program, so by itself it costs nothing. Where a string
value is needed, it converts implicitly to `@string`, as in the assignment to `s` in the `range` example.
That conversion copies the bytes into a new array, so most literals used as values are stored once in a
field instead (see "A literal used as a value is created once" in this section).

**Conversions to and from `[]byte` copy**, as they do in Go. `[]byte(s)` becomes a golib
[`slice<byte>`](#slices-and-arrays) with its own copy of the bytes. `string(b)` becomes the cast
`(@string)b`, which copies them back. Go's `rune` keeps its name in C#, as an alias for `int32`:

<!-- source: src/tests/Behavioral/StringLiteralSliceConversion/main.go:26-30 -->
```go
bs := []byte("hello")
rs := []rune("héllo")

fmt.Println(len(bs), string(bs))
fmt.Println(len(rs), string(rs))
```
<!-- source: src/tests/Behavioral/StringLiteralSliceConversion/main.cs.target:21-24 -->
```csharp
var bs = slice<byte>("hello"u8);
var rs = slice<rune>((@string)"héllo");
fmt.Println(len(bs), ((@string)bs));
fmt.Println(len(rs), ((@string)rs));
```

A `[]byte` conversion takes the `u8` bytes directly. A `[]rune` conversion first makes the literal an
`@string`, because `@string` is what decodes UTF-8 into runes.

**A literal used as a value is created once.** Go keeps literals in read-only memory, so they cost
nothing at run time. To match that, the converter stores such a literal in a `static readonly` field of
the package's class ([Package Conversion](#package-conversion)). The field is declared just before the
first function that uses it. It is named from the literal's words, and the suffix `ˢ` marks it as
generated:

<!-- source: src/tests/Behavioral/StringLiteralHoisting/main.go:12-18 -->
```go
func kind(n int) string {
	if n == 0 {
		return "zero value return"
	}

	return "other value return"
}
```
<!-- source: src/tests/Behavioral/StringLiteralHoisting/main.cs.target:6-17 and :104 -->
```csharp
partial class main_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string zeroValueReturnˢ = "zero value return"u8;
private static readonly @string otherValueReturnˢ = "other value return"u8;

internal static @string kind(nint n) {
    if (n == 0) {
        return zeroValueReturnˢ;
    }
    return otherValueReturnˢ;
}
…
} // end main_package
```

Go's `int` becomes C#'s `nint` ([Integer Types and Arithmetic](#integer-types-and-arithmetic)). Each
distinct literal gets one field for the whole package, however many functions use it. Every use of the
literal names that field.

**A literal passed only as `any` is stored already boxed.** When every use of a literal is an `any`
slot, such as a `fmt.Println` argument, its field is an `object` holding the boxed `@string`. The call
then allocates nothing. [Empty Interface (`any`)](#empty-interface-any) explains the boxing:

<!-- source: src/tests/Behavioral/StringLiteralHoisting/main.go:81 -->
```go
fmt.Println("pre boxed any target")
```
<!-- source: src/tests/Behavioral/StringLiteralHoisting/main.cs.target:55, :69, :82 and :102 -->
```csharp
private static readonly object preBoxedAnyTargetˢ = (@string)"pre boxed any target"u8;
…
internal static void Main() {
…
    fmt.Println(preBoxedAnyTargetˢ);
…
}
```

**A literal stays inline where a field would not help.** A comparison or a concatenation reads the `u8`
bytes directly, so the literal needs no `@string` copy of its own there:

<!-- source: src/tests/Behavioral/StringLiteralHoisting/main.go:36-43 -->
```go
func isSentinel(word string) bool {
	return word == "comparison operand literal"
}
…
func withSuffix(suffix string) string {
	return "concat left operand " + suffix
}
```
<!-- source: src/tests/Behavioral/StringLiteralHoisting/main.cs.target:33-39 -->
```csharp
internal static bool isSentinel(@string word) {
    return word == "comparison operand literal"u8;
}

internal static @string withSuffix(@string suffix) {
    return "concat left operand "u8 + suffix;
}
```

A few other literals stay inline too. A format string such as `"%d items"` makes a poor field name. A
composite literal element (an entry in a slice, array, map or struct literal) stays inline because
hoisting every table entry would add thousands of fields and move the work of building tables into class
initialization. A literal whose ASCII words are too short to name a field stays inline as well. That is
why `s` in the `range` example keeps its literal: its only ASCII letters are `a` and `b`. The reference
lists
[every case that stays inline](ConversionStrategies-Reference/strings.md#a-value-materializing-string-literal-is-hoisted-to-a-static-readonly-field-beside-its-first-use).

**A string constant declared inside a function** is stored the same way. Its field takes the constant's
own name plus the suffix `ᶜ`. The local variable keeps the Go name `hello` in the function body. Copying
the field into it copies no bytes, because an `@string` is a small struct that refers to its bytes:

<!-- source: src/tests/Behavioral/LocalStringConstHoist/main.go:16-19 -->
```go
func makeGreeting(who string) string {
	const hello = "hello, "
	return hello + who
}
```
<!-- source: src/tests/Behavioral/LocalStringConstHoist/main.cs.target:16-22 -->
```csharp
// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
private static readonly @string helloᶜ = "hello, "u8;

internal static @string makeGreeting(@string who) {
    @string hello = helloᶜ;
    return hello + who;
}
```

**Named string types** (`type version string`) become wrapper structs. The `[GoType("@string")]`
attribute asks the [source generators](#source-generators) to fill in the wrapper. It keeps the whole
string surface: indexing, slicing, `len`, comparison, `+` and conversions. A concatenation keeps the
named type, as in Go. The type's methods become static extension methods, so `this version v` is the
receiver ([Functions and Methods](#functions-and-methods)):

<!-- source: src/tests/Behavioral/NamedStringConcat/main.go:13-17 -->
```go
type version string

func (v version) tag() string { return string(v) + "!" }

func bump(v version) version { return v + "-next" }
```
<!-- source: src/tests/Behavioral/NamedStringConcat/main.cs.target:7-15 -->
```csharp
[GoType("@string")] partial struct version;

internal static @string tag(this version v) {
    return ((@string)v) + "!"u8;
}

internal static version bump(version v) {
    return v + "-next"u8;
}
```

**`sstring` is a string view that allocates nothing.** The name reads as *stack string*. Golib's
[`sstring`](../src/core/golib/sstring.cs) is a `ref struct`: it lives only on the stack and points at
bytes that already exist. C# rejects at compile time any code that stores a `ref struct` in a field,
array or map, boxes it, or captures it in a lambda. So an `sstring` can never outlive its bytes.

It appears in two places: a `string([]byte)` conversion that is only read, and the string parameter of
some library functions. Each is shown in its own rule in this section.

**A `string([]byte)` that is only read becomes an `sstring`.** Go skips the copy when the string is only
read while its bytes cannot change. The converter does the same where it can prove it. Here each
converted string is compared once and then discarded:

<!-- source: src/tests/Behavioral/SStringElision/main.go:281-289 -->
```go
func prefix(buf []byte) bool {
	if len(buf) < 6 {
		return false
	}
	if string(buf[:3]) != "GET" {
		return false
	}
	return string(buf[3:6]) == " /x"
}
```
<!-- source: src/tests/Behavioral/SStringElision/main.cs.target:217-225 -->
```csharp
internal static bool prefix(slice<byte> buf) {
    if (len(buf) < 6) {
        return false;
    }
    if (((sstring)(buf[..3])) != "GET"u8) {
        return false;
    }
    return ((sstring)(buf[3..6])) == " /x"u8;
}
```

`buf[3..6]` is C#'s range syntax for Go's `buf[3:6]` ([Slices and Arrays](#slices-and-arrays)).

Where the converter cannot prove it, the conversion stays the copying `(@string)` cast. Here the string
is returned, so it must own its bytes:

<!-- source: src/tests/Behavioral/SStringElision/main.go:241-245 -->
```go
func returnedString() string {
	b := []byte("returned")
	r := string(b)
	return r
}
```
<!-- source: src/tests/Behavioral/SStringElision/main.cs.target:178-182 -->
```csharp
internal static @string returnedString() {
    var b = slice<byte>("returned"u8);
    @string r = ((@string)b);
    return r;
}
```

**Some library functions take their string as an `sstring`, plus an `@string` twin.** The converted
function declares its string parameter as `sstring`. The [source generators](#source-generators) add a
second overload, the *twin*, that takes an `@string` and forwards to it. C# picks the right overload, so
the call reads the same as in Go. `fmt.Sprintf` is one such function, so a `u8` literal argument binds
with no copy:

<!-- source: src/tests/Behavioral/SStringTwinPilot/main.go:19 -->
```go
fmt.Println(fmt.Sprintf("xxx"))
```
<!-- source: src/tests/Behavioral/SStringTwinPilot/main.cs.target:35 -->
```csharp
fmt.Println(fmt.Sprintf("xxx"u8));
```

Go's `func Sprintf(format string, a ...any) string` is declared this way in the converted `fmt` package:

<!-- source: src/core/fmt/print.cs:268-269 and :276 -->
```csharp
[GoStr] public static @string Sprintf(sstring format, params ꓸꓸꓸany aʗp) {
    var a = aʗp.slice();
…
}
```

The `[GoStr]` attribute asks the generators for the twin. `params ꓸꓸꓸany aʗp` is Go's variadic
`a ...any`: `ꓸꓸꓸany` is the file's alias for `Span<any>`, the `ʗp` suffix marks the raw incoming pack,
and the first line of the body turns it into the slice `a` that the Go code uses.
[Functions and Methods](#functions-and-methods) covers variadic parameters.

**A twinned function used as a value is named with `ᶠ`.** With two overloads, C# has no single method to
turn into a delegate. So the generators also declare one shared delegate field, `Sprintfᶠ`, and the
converter uses it wherever Go uses the function as a value:

<!-- source: src/tests/Behavioral/SStringTwinPilot/main.go:28-29 -->
```go
f := fmt.Sprintf
fmt.Println(f("via local %s", "ok"))
```
<!-- source: src/tests/Behavioral/SStringTwinPilot/main.cs.target:42-43 -->
```csharp
Funcꓸꓸꓸ<@string, any, @string> f = fmt.Sprintfᶠ;
fmt.Println(f("via local %s"u8, (@string)"ok"u8));
```

`Funcꓸꓸꓸ<@string, any, @string>` is the C# type for Go's `func(string, ...any) string`
([Function Values and Closures](#function-values-and-closures)). `"ok"` goes to an `any` slot, and a
`u8` span cannot be boxed, so `(@string)` first makes it a value. It is not stored in a field because
`ok` is too short to make a useful name. Every glyph in this section is listed in
[Reading Converted Code: Names and Glyphs](#reading-converted-code-names-and-glyphs).

**Full detail:** [Reference → Strings (`@string` and `sstring`)](ConversionStrategies-Reference/strings.md#strings-string-and-sstring) — windows and conversions, literal rendering and byte-array literals, the exact hoisting rules, named-string wrappers, `sstring` eligibility, and how functions are chosen to take `sstring`.

---

<a id="maps-and-channels"></a>
## Maps

Go's `map[K]V` becomes golib [`map<K, V>`](../src/core/golib/map.cs). Golib is go2cs's runtime library
([The golib Runtime Library](#the-golib-runtime-library)). `map<K, V>` is a small struct that wraps a .NET
`Dictionary`. Copying it copies only a reference, so every copy sees the same entries, as in Go.

Reads (`m[k]`) and writes (`m[k] = v`) keep Go's index syntax. `len(m)`, `delete(m, k)` and `clear(m)`
keep their Go names ([Built-in Functions](#built-in-functions)). Channels have their own section,
[Channels and `select`](#channels-and-select).

The examples use a few golib names. `@string` is Go's `string` ([Strings](#strings-string-and-sstring)),
and `nint` is Go's `int` ([Integer Types and Arithmetic](#integer-types-and-arithmetic)). `"a"u8` is a C#
UTF-8 string literal, which golib converts implicitly to `@string`. Other glyphs are listed in
[Names and Glyphs](#reading-converted-code-names-and-glyphs).

**A map literal becomes a C# index initializer, and comma-ok adds the `ꟷ` argument.** A plain read,
`m[k]`, stays `m[k]`. The comma-ok read, `v, ok := m[k]`, passes `ꟷ` as a second index:

<!-- source: src/tests/Behavioral/MapCommaOk/main.go:15-27 -->
```go
m := map[string]int{"a": 1, "b": 2}

// value + ok, present.
v, ok := m["a"]
fmt.Println(v, ok) // 1 true

// value + ok, absent (value is the zero value).
v2, ok2 := m["z"]
fmt.Println(v2, ok2) // 0 false

// blank value, present.
_, ok3 := m["b"]
fmt.Println(ok3) // true
```
<!-- source: src/tests/Behavioral/MapCommaOk/main.cs.target:11-17 -->
```csharp
var m = new map<@string, nint>{["a"u8] = 1, ["b"u8] = 2};
var (v, ok) = m["a"u8, ꟷ];
fmt.Println(v, ok);
var (v2, ok2) = m["z"u8, ꟷ];
fmt.Println(v2, ok2);
var (_, ok3) = m["b"u8, ꟷ];
fmt.Println(ok3);
```

`ꟷ` is a plain golib `bool` constant. Its value, `false`, is ignored: passing a second index is what
selects golib's two-value indexer. That indexer returns a tuple, `(value, present)`, which `var (v, ok)`
unpacks. A missing key gives the zero value and `false`, as in Go.
[Multi-Result Values and Comma-Ok Forms](#multi-result-values-and-comma-ok-forms) covers the other
comma-ok forms.

**`range` over a map becomes a `foreach` over key-value pairs.** Each pair unpacks into `(k, v)`, and a `_`
drops the part Go leaves out. The body may add and delete entries of the map it walks, as Go allows:

<!-- source: src/tests/Behavioral/MapMutateDuringRange/main.go:36-131 -->
```go
func main() {
…
	insert := map[string]int{"a": 1, "b": 2, "c": 3}
	visited := 0
	for k, v := range insert {
		if len(k) == 1 {
			visited++
			insert[k+"!"] = v * 10
		}
	}
	fmt.Println("insert visited:", visited)
…
}
```
<!-- source: src/tests/Behavioral/MapMutateDuringRange/main.cs.target:19-93 -->
```csharp
// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object insertVisitedˢ = (@string)"insert visited:"u8;
…
internal static void Main() {
    var insert = new map<@string, nint>{["a"u8] = 1, ["b"u8] = 2, ["c"u8] = 3};
    nint visited = 0;
    foreach (var (k, v) in insert) {
        if (len(k) == 1) {
            visited++;
            insert[k + "!"u8] = v * 10;
        }
    }
    fmt.Println(insertVisitedˢ, visited);
…
}
```

The visit order is unspecified in both languages. Go also leaves open whether an entry added during the
walk is visited, so code must not rely on it. [Loops, Range and Labels](#loops-range-and-labels) covers
`range` over other types.

Go's package functions become static methods of a class for the package, so `main` becomes `Main`
([Package Conversion](#package-conversion)). `insertVisitedˢ` is a hoisted literal: the string
`"insert visited:"`, stored once in a static field at class level, outside `Main`. The `ˢ` suffix marks
such a name, and [Strings](#strings-string-and-sstring) says which literals are hoisted.

The field is typed `object` because it is only passed to `fmt.Println` as an `any` value, so it is
converted to `any` once rather than on every call ([Empty Interface](#empty-interface-any)). In the
generated comment, `RODATA` is the read-only data section where Go keeps literals.

**A nil map reads as empty and panics on write.** `var m map[string]int` declares a nil map. The converter
writes Go's `nil` as `default!` ([Nil and Zero Values](#nil-and-zero-values) explains the `!`). For a map,
`default!` is the `map<K, V>` struct's default value, which holds no `Dictionary`:

<!-- source: src/tests/Behavioral/NilMapOperations/main.go:33-72 -->
```go
func main() {
	var m map[string]int // nil map

	// read absent key -> zero value
	fmt.Println(m["a"]) // 0

	// comma-ok read -> (zero, false)
	v, ok := m["a"]
	fmt.Println(v, ok) // 0 false

	// len(nil) -> 0
	fmt.Println(len(m)) // 0

	// range over nil -> no iterations
	count := 0
	for range m {
		count++
	}
	fmt.Println(count) // 0

	// delete(nil, k) -> no-op, no panic
	delete(m, "a")
	fmt.Println("delete ok") // delete ok

	// m == nil -> true for a nil map
	fmt.Println(m == nil) // true

	// an empty but non-nil map is NOT nil
	e := map[string]int{}
	fmt.Println(e == nil) // false
…
}
```
<!-- source: src/tests/Behavioral/NilMapOperations/main.cs.target:26-50 -->
```csharp
// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object deleteOkˢ = (@string)"delete ok"u8;

internal static void Main() {
    map<@string, nint> m = default!;
    fmt.Println(m["a"u8]);
    var (v, ok) = m["a"u8, ꟷ];
    fmt.Println(v, ok);
    fmt.Println(len(m));
    nint count = 0;
    foreach ((_, _) in m) {
        count++;
    }
    fmt.Println(count);
    delete(m, "a"u8);
    fmt.Println(deleteOkˢ);
    fmt.Println(m == default!);
    var e = new map<@string, nint>{};
    fmt.Println(e == default!);
…
}
```

Every read of a nil map sees no entries: an absent key gives the zero value, `len` is 0, and a `range`
runs no iterations. `m == nil` becomes `m == default!`, which is true only for a map with no `Dictionary`.
So an empty map built with `{}` is not nil, as in Go. `deleteOkˢ` is the hoisted literal `"delete ok"`.

A `range` with no variables unpacks each pair into two C# discards, `(_, _)`. This is ordinary C#
deconstruction in a `foreach`; the loop body uses neither part.

A write such as `m["x"] = 1` on a nil map panics with Go's message, "assignment to entry in nil map". A
deferred `recover` catches that panic as it would in Go
([Defer / Panic / Recover](#defer--panic--recover)).

**A named map type is a wrapper struct.** `type registry map[uint32]entry` becomes a `[GoType]` partial
struct declared with no body. Its comma-ok read looks like a plain map's:

<!-- source: src/tests/Behavioral/EmptyStructMapSet/EmptyStructMapSet.go:32-45 -->
```go
type entry struct {
	tag  string
	size int
}

type registry map[uint32]entry

func lookup(reg registry, id uint32) (string, bool) {
	e, ok := reg[id]
	if !ok {
		return "missing", false
	}
	return e.tag, true
}
```
<!-- source: src/tests/Behavioral/EmptyStructMapSet/EmptyStructMapSet.cs.target:16-32 -->
```csharp
[GoType] partial struct entry {
    internal @string tag;
    internal nint size;
}

[GoType("map[uint32, entry]")] partial struct registry;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string missingˢ = "missing"u8;

internal static (@string, bool) lookup(registry reg, uint32 id) {
    var (e, ok) = reg[id, ꟷ];
    if (!ok) {
        return (missingˢ, false);
    }
    return (e.tag, true);
}
```

The `[GoType]` attribute marks a converted Go type, and its argument names the underlying map. A
[source generator](#source-generators) writes the body of `registry` at compile time. It wraps a
`map<uint32, entry>` and adds the indexers, `len`, `range` and a size constructor.

Go's two results become a C# tuple ([Multi-Result Values](#multi-result-values-and-comma-ok-forms)), and
`entry` is an ordinary converted struct ([Struct Types](#struct-types)). `missingˢ` is the hoisted literal
`"missing"`. It stays an `@string`, not `object`, because the function returns it as a string.

A named type's literal wraps a plain map literal, and `make(registry, 4)` passes its size hint to the
constructor:

<!-- source: src/tests/Behavioral/EmptyStructMapSet/EmptyStructMapSet.go:74-84 -->
```go
reg := registry{2: {tag: "leaf", size: 8}}
t1, ok1 := lookup(reg, 2)
t2, ok2 := lookup(reg, 9)
fmt.Println(t1, ok1, t2, ok2) // leaf true missing false
…
reg2 := make(registry, 4)
reg2[7] = entry{tag: "cap", size: 1}
e7, ok7 := lookup(reg2, 7)
fmt.Println(len(reg2), e7, ok7) // 1 cap true
```
<!-- source: src/tests/Behavioral/EmptyStructMapSet/EmptyStructMapSet.cs.target:62-69 -->
```csharp
var reg = new registry(new map<uint32, entry>{[2] = new(tag: "leaf"u8, size: 8)});
var (t1, ok1) = lookup(reg, 2);
var (t2, ok2) = lookup(reg, 9);
fmt.Println(t1, ok1, t2, ok2);
var reg2 = new registry(4);
reg2[7] = new entry(tag: "cap"u8, size: 1);
var (e7, ok7) = lookup(reg2, 7);
fmt.Println(len(reg2), e7, ok7);
```

`new(tag: "leaf"u8, size: 8)` builds an `entry`. It calls the constructor the source generator gives every
converted struct, with one named argument per field ([Struct Types](#struct-types)).

**A lookup keyed by `string(b)` does not copy the bytes; a store does.** Here `k` is a `[]byte`, which
becomes golib `slice<byte>` ([Slices and Arrays](#slices-and-arrays)). A store keeps its key, so
`w[string(k)] = 42` copies the bytes with a plain `(@string)` conversion. A read never keeps its key, so
Go skips the copy there, and go2cs does the same with golib's `tmpstring(k)`:

<!-- source: src/tests/Behavioral/MapStringBytesLookup/main.go:14-55 -->
```go
func main() {
…
	w := map[string]int{}
	k := []byte("alpha")
	w[string(k)] = 42
	k[0] = 'Z'
	fmt.Println(w["alpha"], len(w))
	_, hit := w[string(k)] // "Zlpha" — must miss
	fmt.Println(hit)
…
}
```
<!-- source: src/tests/Behavioral/MapStringBytesLookup/main.cs.target:7-37 -->
```csharp
// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string alphaˢ = "alpha"u8;

internal static void Main() {
…
    var w = new map<@string, nint>{};
    var k = slice<byte>("alpha"u8);
    w[((@string)k)] = 42;
    k[0] = (rune)'Z';
    fmt.Println(w[alphaˢ], len(w));
    var (_, hit) = w[tmpstring(k), ꟷ];
    fmt.Println(hit);
…
}
```

`tmpstring(k)` returns a temporary `@string` that views the current bytes of `k`. That is safe because the
view is used only for the lookup. Changing `k` after the store does not touch the stored key, so the lookup
of `"Zlpha"` misses. Plain and comma-ok reads both take this path.

`slice<byte>("alpha"u8)` is golib's form of Go's `[]byte("alpha")`: it copies the literal's bytes into a
new slice. `alphaˢ` is the hoisted literal `"alpha"`. `(rune)'Z'` is Go's constant `'Z'`, where golib's
`rune` is `int32`. C# converts an in-range constant to `byte` implicitly, so the store into `k[0]` compiles.

**Full detail:** [Reference → Maps and Channels](ConversionStrategies-Reference/maps-and-channels.md#maps-and-channels) — how a range survives changes to its own map, nil and interface keys, exactly which `string(b)` lookups skip the copy, named map types, and map access through type parameters.

---

<a id="generic-constraints"></a>
## Generics

Go type parameters become C# generic type parameters. Go's square brackets become angle brackets, so
`Stack[T]` reads `Stack<T>`. Each Go constraint becomes a C# `where` clause. `any` and `comparable` add no
clause.

A C# `where` clause can name interfaces, a base class and a few keywords, but not Go's type sets. So the
converter picks the .NET or golib interfaces that let the body's operations compile.
[golib](#the-golib-runtime-library) is the go2cs runtime library that provides Go types such as `slice<T>`.
Converted code imports golib's built-in functions with `using static`, so calls such as `len`, `append`
and `AreEqual` need no class name ([Built-in Functions](#built-in-functions)).

**A method-set interface constraint becomes a plain `where` clause** that names the converted interface.
Calling the constraint's methods on the type parameter works as in Go:

<!-- source: src/tests/Behavioral/GenericInterfaceConstraint/GenericInterfaceConstraint.go:12 -->
```go
type Shape interface {
	Area() float64
	Name() string
}
…
func totalArea[S Shape](shapes []S) float64 {
	var sum float64
	for _, s := range shapes {
		sum += s.Area()
	}
	return sum
}
```
<!-- source: src/tests/Behavioral/GenericInterfaceConstraint/GenericInterfaceConstraint.cs.target:7 -->
```csharp
[GoType] partial interface Shape {
    float64 Area();
    @string Name();
}
…
internal static float64 totalArea<S>(slice<S> shapes)
    where S : Shape
{
    float64 sum = default!;
    foreach (var (_, s) in shapes) {
        sum += s.Area();
    }
    return sum;
}
```

`[GoType]` marks a converted Go type that a [source generator](#source-generators) completes. Go's `[]S`
is golib's [`slice<S>`](#slices-and-arrays), `@string` is Go's string
([Strings](#strings-string-and-sstring)), and `float64` is C# `double` under its Go name. `default!` is
the zero value ([Nil and Zero Values](#nil-and-zero-values)). A `range` loop becomes a `foreach` over
`(index, value)` pairs ([Loops, Range and Labels](#loops-range-and-labels)).

**A pointer type argument goes in through adapters.** In Go, `totalArea(circles)` with a `[]*Circle`
simply infers `S = *Circle`. In C#, a Go pointer is golib's heap box `ж<Circle>` ([Pointers](#pointers)),
and the box does not implement `Shape`. A generated adapter class, `CircleжShape`, does
([Interfaces](#interfaces)). So the call passes the slice through golib's `widen`, which builds a
`slice<Shape>` of adapters, and C# instantiates `S` as `Shape`:

<!-- source: src/tests/Behavioral/GenericInterfaceConstraint/GenericInterfaceConstraint.go:24 -->
```go
type Circle struct {
	R float64
}

func (c *Circle) Area() float64 {
	return 3.0 * c.R * c.R
}
…
	circles := []*Circle{&Circle{R: 1}, &Circle{R: 2}} // pointer instantiation — adapter-projected
…
	shapes := []Shape{&Circle{R: 1}, &Square{S: 2}}    // interface instantiation — direct
…
	fmt.Printf("circles: %.2f\n", totalArea(circles))
…
	fmt.Printf("shapes: %.2f\n", totalArea(shapes))
```
<!-- source: src/tests/Behavioral/GenericInterfaceConstraint/GenericInterfaceConstraint.cs.target:18 -->
```csharp
[GoType] partial struct Circle {
    public float64 R;
}

[GoRecv] public static float64 Area(this ref Circle c) {
    return 3.0D * c.R * c.R;
}
…
    var circles = new ж<Circle>[]{Ꮡ(new Circle(R: 1D)), Ꮡ(new Circle(R: 2D))}.slice();
…
    var shapes = new Shape[]{new CircleжShape(Ꮡ(new Circle(R: 1D))), new SquareжShape(Ꮡ(new Square(S: 2D)))}.slice();
…
    fmt.Printf("circles: %.2f\n"u8, totalArea(widen<ж<Circle>, Shape>(circles, elemᴛ0 => new CircleжShape(elemᴛ0))));
…
    fmt.Printf("shapes: %.2f\n"u8, totalArea(shapes));
```

`[GoRecv] … this ref Circle` is a pointer-receiver method ([Functions and Methods](#functions-and-methods)).
`Ꮡ(…)` takes an address, as Go's `&` does, and `.slice()` turns a C# array into a golib slice
([Slices and Arrays](#slices-and-arrays)). The ᴛ in `elemᴛ0` marks a name the converter makes up, here the
adapter lambda's parameter ([Reading Converted Code](#reading-converted-code-names-and-glyphs)). Each
adapter holds the original box, so a method called through it acts on the same `Circle`. A `[]Shape`
argument already satisfies the constraint, so `totalArea(shapes)` stays as Go writes it.

**An operator type set becomes `System.Numerics` operator interfaces**, so `+`, `==` and `<` compile on
the type parameter. A Go type set lists its allowed types with `|`. `~int` means any type whose underlying
type is `int`, such as `type MyInt int`. `cmp.Ordered` is Go's standard type set for types that support
`<`: the integer and floating-point types, and `string`.

<!-- source: GOROOT/src/cmp/cmp.go:28 -->
```go
func Less[T Ordered](x, y T) bool {
	return (isNaN(x) && !isNaN(y)) || x < y
}
…
func isNaN[T Ordered](x T) bool {
	return x != x
}
```
<!-- source: src/core/cmp/cmp.cs:29 -->
```csharp
public static bool Less<T>(T x, T y)
    where T : /* Ordered */ IAdditionOperators<T, T, T>, IEqualityOperators<T, T, bool>, IComparisonOperators<T, T, bool>, new()
{
    return (isNaN(x) && !isNaN(y)) || x < y;
}
…
internal static bool isNaN<T>(T x)
    where T : /* Ordered */ IAdditionOperators<T, T, T>, IEqualityOperators<T, T, bool>, IComparisonOperators<T, T, bool>, new()
{
    return !AreEqual(x, x);
}
```

The comment `/* Ordered */` names the Go constraint. Then comes one interface for each operator that
every type in the set supports. `Ordered` includes `string`, which has only `+` and comparisons, so it
gets these three. An integer-only type set also gets `-`, `*`, `/`, `%`, bitwise and shift interfaces.

The trailing `new()` is the C# constraint that allows `new T()`. go2cs adds it to every operator-set
clause, so a converted body can construct a `T` when it needs one.

`x < y` compiles through the comparison interface. But `==` and `!=` on a type-parameter value always
call golib's [`AreEqual`](../src/core/golib/builtin.cs), so `isNaN`'s `x != x` becomes `!AreEqual(x, x)`.
`AreEqual` compares at run time the way Go's `==` does, including a NaN never equaling itself.

**`comparable` adds no clause.** No C# constraint admits every type Go can compare with `==`, such as
structs, pointers and interfaces. Go's type checker has already checked every use, so the parameter stays
unconstrained, and `==` becomes `AreEqual` as under every other constraint:

<!-- source: src/tests/Behavioral/ReverseSortNaNOrder/ReverseSortNaNOrder.go:64 -->
```go
func eq[T comparable](a, b T) bool { return a == b }
```
<!-- source: src/tests/Behavioral/ReverseSortNaNOrder/ReverseSortNaNOrder.cs.target:71 -->
```csharp
internal static bool eq<T>(T a, T b) {
    return AreEqual(a, b);
}
```

**A generic type keeps its type parameters, and each method repeats them.** A Go method becomes a C#
extension method, and a pointer receiver becomes `[GoRecv] this ref`
([Functions and Methods](#functions-and-methods)). An extension method cannot borrow its receiver type's
parameters. So each method declares `<T>` and restates the `where` clause:

<!-- source: src/tests/Behavioral/GenericTypeInstantiation/GenericTypeInstantiation.go:6 -->
```go
type Stack[T ~int | ~string] struct {
	elements []T
}

func (s *Stack[T]) Push(element T) {
	s.elements = append(s.elements, element)
}

func (s *Stack[T]) Pop() (T, bool) {
	var zero T
	if len(s.elements) == 0 {
		return zero, false
	}

	index := len(s.elements) - 1
	element := s.elements[index]
	s.elements = s.elements[:index]
	return element, true
}
```
<!-- source: src/tests/Behavioral/GenericTypeInstantiation/GenericTypeInstantiation.cs.target:7 -->
```csharp
[GoType] partial struct Stack<T>
    where T : /* ~int | ~string */ IAdditionOperators<T, T, T>, IEqualityOperators<T, T, bool>, IComparisonOperators<T, T, bool>, new()
{
    internal slice<T> elements;
}

[GoRecv] public static void Push<T>(this ref Stack<T> s, T element)
    where T : /* ~int | ~string */ IAdditionOperators<T, T, T>, IEqualityOperators<T, T, bool>, IComparisonOperators<T, T, bool>, new()
{
    s.elements = append(s.elements, element);
}

[GoRecv] public static (T, bool) Pop<T>(this ref Stack<T> s)
    where T : /* ~int | ~string */ IAdditionOperators<T, T, T>, IEqualityOperators<T, T, bool>, IComparisonOperators<T, T, bool>, new()
{
    T zero = default!;
    if (len(s.elements) == 0) {
        return (zero, false);
    }
    nint index = len(s.elements) - 1;
    var element = s.elements[index];
    s.elements = s.elements[..(int)(index)];
    return (element, true);
}
```

The three `where` clauses are identical. The set `~int | ~string` includes `string`, so, as with
`Ordered`, only `+` and the comparisons are common to every member. Here too, `[GoType]` lets the source
generator complete `Stack<T>`, including its zero-value constructor used below.

`Pop`'s two results become a C# tuple ([Multi-Result Values](#multi-result-values-and-comma-ok-forms)).
`var zero T` becomes `T zero = default!`, the zero value of `T`. The sub-slice `s.elements[:index]` becomes
a C# range, which takes an `int` index. golib's range indexer shares the backing array as Go's `s[:i]`
does, and does not copy ([Slices and Arrays](#slices-and-arrays)).

Using the type reads as in Go. Go's `int` is C# `nint`
([Integer Types and Arithmetic](#integer-types-and-arithmetic)). `Stack[int]{}` calls the constructor that
the source generator adds for the zero value, which takes golib's `nil` as its argument
([Struct Types](#struct-types)). `"…"u8` is a UTF-8 string literal ([Strings](#strings-string-and-sstring)).

<!-- source: src/tests/Behavioral/GenericTypeInstantiation/GenericTypeInstantiation.go:58 -->
```go
	intStack := Stack[int]{}
	intStack.Push(10)
	intStack.Push(20)
	val, _ := intStack.Pop()
	fmt.Printf("Popped from int stack: %d\n", val)
```
<!-- source: src/tests/Behavioral/GenericTypeInstantiation/GenericTypeInstantiation.cs.target:65 -->
```csharp
    var intStack = new Stack<nint>(nil);
    intStack.Push(10);
    intStack.Push(20);
    var (val, _) = intStack.Pop();
    fmt.Printf("Popped from int stack: %d\n"u8, val);
```

**A slice type set `~[]E` becomes golib's slice interfaces.** `~[]E` admits any slice type whose elements
are `E`, including a named slice type such as `type numbers []int`. `ISlice<E>` is the interface every
slice type implements. `ISupportMake<S>` lets `make(S, n)` build an `S`. `ISliceWrap<S, E>` lets `s[i:j]`
and `append` return `S` again, as they do in Go.

<!-- source: src/tests/Behavioral/ConstrainedSliceParamInPlace/main.go:6 -->
```go
func reverse[E any](s []E) {
	for i, j := 0, len(s)-1; i < j; i, j = i+1, j-1 {
		s[i], s[j] = s[j], s[i]
	}
}
…
func reverseSeq[S ~[]E, E any](x S) {
	reverse(x)
}
```
<!-- source: src/tests/Behavioral/ConstrainedSliceParamInPlace/main.cs.target:7 -->
```csharp
internal static void reverse<E>(slice<E> s) {
    for ((nint i, nint j) = (0, len(s) - 1); i < j; (i, j) = (i + 1, j - 1)) {
        (s[i], s[j]) = (s[j], s[i]);
    }
}

internal static void reverseSeq<S, E>(S x)
    where S : /* ~[]E */ ISlice<E>, ISupportMake<S>, ISliceWrap<S, E>, new()
{
    reverse(new slice<E>(x));
}
```

`reverse` takes a plain `[]E`, so it needs no constraint. Its swap becomes a C# tuple assignment
([Multi-Assignment](#multi-assignment-and-evaluation-order)). Inside `reverseSeq`, passing `x` as a `[]E`
becomes `new slice<E>(x)`. That is a slice over the same backing array, as in Go, so `reverse` changes the
caller's elements.

**Type arguments are written out when Go writes them or C# cannot infer them.** Go infers `E` from the
constraint `S ~[]E`. C# infers type parameters only from argument types, and no argument has type `E`.
So each call to `reverseSeq` spells both type arguments:

<!-- source: src/tests/Behavioral/ConstrainedSliceParamInPlace/main.go:45 -->
```go
type numbers []int
…
	a := []int{1, 2, 3, 4, 5}
	reverseSeq(a)
	fmt.Println(a) // [5 4 3 2 1]
…
	b := numbers{10, 20, 30}
	reverseSeq(b)
	fmt.Println(b) // [30 20 10]
```
<!-- source: src/tests/Behavioral/ConstrainedSliceParamInPlace/main.cs.target:39 -->
```csharp
[GoType("[]nint")] partial struct numbers;
…
    var a = new nint[]{1, 2, 3, 4, 5}.slice();
    reverseSeq<slice<nint>, nint>(a);
    fmt.Println(a);
    var b = new numbers(new nint[]{10, 20, 30}.slice());
    reverseSeq<numbers, nint>(b);
    fmt.Println(b);
```

A named slice type such as `numbers` becomes a wrapper struct marked `[GoType("[]nint")]` that behaves as
a slice. A slice literal builds a C# array and turns it into a slice with `.slice()`
([Slices and Arrays](#slices-and-arrays)). Calls C# can infer, such as `totalArea(shapes)`, stay bare.

Where Go writes the type arguments itself, C# keeps them. Here `T` appears in no parameter, so Go
must name it. `stringerˢ` is the string literal `"stringer"`, hoisted to a field that the example shows
([Strings](#strings-string-and-sstring)):

<!-- source: src/tests/Behavioral/GenericTypeInstantiation/GenericTypeInstantiation.go:50 -->
```go
func describe[T any](label string) string {
	var zero T
	_ = zero
	return label
}
…
	fmt.Println(describe[fmt.Stringer]("stringer"))
```
<!-- source: src/tests/Behavioral/GenericTypeInstantiation/GenericTypeInstantiation.cs.target:50 -->
```csharp
internal static @string describe<T>(@string label) {
    T zero = default!;
    _ = zero;
    return label;
}
…
private static readonly @string stringerˢ = "stringer"u8;
…
    fmt.Println(describe<fmt.Stringer>(stringerˢ));
```

**Conversions to or from an integer type parameter go through golib**, because C# has no numeric cast to
or from a type parameter. In this example `Int` is the name of the type parameter, not a built-in type.
[`ConvertToUInt64<Int>`](../src/core/golib/builtin.TypeParamConversions.cs) turns an `Int` value into a
`uint64`, and `ConvertToType<Int>` turns a number into an `Int`:

<!-- source: src/tests/Behavioral/GenericTypeInference/GenericTypeInference.go:243 -->
```go
func halveN[Int ~int32 | ~int64](n Int) Int {
	if n <= 0 {
		return n
	}
	return Int(uint64(n) / 2)
}
```
<!-- source: src/tests/Behavioral/GenericTypeInference/GenericTypeInference.cs.target:246 -->
```csharp
internal static Int halveN<Int>(Int n)
    where Int : /* ~int32 | ~int64 */ IAdditionOperators<Int, Int, Int>, ISubtractionOperators<Int, Int, Int>, IMultiplyOperators<Int, Int, Int>, IDivisionOperators<Int, Int, Int>, IIncrementOperators<Int>, IDecrementOperators<Int>, IUnaryNegationOperators<Int, Int>, IModulusOperators<Int, Int, Int>, IBitwiseOperators<Int, Int, Int>, IShiftOperators<Int, int, Int>, IEqualityOperators<Int, Int, bool>, IComparisonOperators<Int, Int, bool>, new()
{
    if (n <= ConvertToType<Int>(0)) {
        return n;
    }
    return ConvertToType<Int>(ConvertToUInt64<Int>(n) / 2);
}
```

Even the constant `0` in `n <= 0` goes through `ConvertToType`, because C# cannot turn the literal `0` into
an `Int` either. The long `where` clause is what an integer-only type set becomes: every arithmetic,
bitwise, shift and comparison interface.

**Full detail:** [Reference → Generic Constraints](ConversionStrategies-Reference/generic-constraints.md#generic-constraints) — array and map type sets, pointer and self-referential constraints, unions such as `string | []byte`, per-field equality in generic structs, and how explicit type arguments are chosen, including when an untyped constant argument decides one.

---

## Type Aliasing

A Go alias declaration, `type A = B`, becomes a C# `global using A = B;` directive. Code declared with a
non-generic alias keeps the alias name as its type, so it reads like the Go source.

A plain C# `using` alias reaches one file; a `global using` reaches every file in the project. Each Go package
converts to one C# project ([Package Conversion](#package-conversion)), so the alias has package scope, just as in Go.

**An alias is not a type definition.** `type Temperature = Celsius` gives an existing type a second name.
`type Celsius float64` creates a new, distinct type, which becomes a C# `partial struct` as
[Named Numeric Types](#named-numeric-types-and-constant-contexts) describes. Here only the alias becomes a
`global using`:

<!-- source: src/tests/Behavioral/CrossPkgLib/lib.go:16-27 -->
```go
type Celsius float64
…
type Temperature = Celsius

// Boiling returns a Celsius value.
func Boiling() Celsius { return 100 }

// Freezing returns the alias type, so a consumer can name the imported alias.
func Freezing() Temperature { return 0 }
```
<!-- source: src/tests/Behavioral/CrossPkgLib/lib.cs.target:1-20 -->
```csharp
global using Temperature = go.CrossPkgLib_package.Celsius;
…
namespace go;

partial class CrossPkgLib_package {
…
[GoType("num:float64")] partial struct Celsius;

public static Celsius Boiling() {
    return 100D;
}

public static Temperature Freezing() {
    return 0D;
}
```

Every converted file sits in `namespace go`. A package's declarations live in one class named after it, here
`CrossPkgLib_package` ([Package Conversion](#package-conversion)). The `global using` directives come first, outside
both.

- `[GoType("num:float64")]` marks a converted Go type whose underlying type is `float64`. A
  [source generator](#source-generators) fills in its value, operators and conversions.
- `Freezing` is declared to return `Temperature`, and C# reads that as `Celsius`, exactly as Go does.
- `100D` is a C# `double` literal, the type Go's `float64` maps to.
- The target is spelled out in full, `go.CrossPkgLib_package.Celsius`. The rule *An alias target is written out in
  full*, in this section, explains why.

**A type definition over an interface also becomes a `global using`.** `type Token any` is a definition, not an
alias. But Go gives it exactly the interface's methods and lets it declare none of its own. So go2cs converts it as an
alias of that interface, and any value converts to it just as it converts to the interface:

<!-- source: src/tests/Behavioral/DefinedTypeOverInterface/main.go:13-51 -->
```go
// Defined type over the empty interface — aliased to object.
type Token any

// A named interface, and a defined type OVER it — aliased to the interface.
type Stringer interface {
	String() string
}

type Named Stringer

type point struct{ x, y int }

func (p point) String() string { return fmt.Sprintf("(%d,%d)", p.x, p.y) }
…
func main() {
…
	var n Named = point{3, 4} // concrete point -> Named (defined over the Stringer interface)
	fmt.Println(n.String())
}
```
<!-- source: src/tests/Behavioral/DefinedTypeOverInterface/main.cs.target:1-54 -->
```csharp
global using Token = object;
global using Named = go.main_package.Stringer;

namespace go;

using fmt = fmt_package;

partial class main_package {
…
[GoType] partial interface Stringer {
    @string String();
}
…
[GoType] partial struct point {
    internal nint x, y;
}

internal static @string String(this point p) {
    return fmt.Sprintf("(%d,%d)"u8, p.x, p.y);
}
…
internal static void Main() {
…
    Named n = new point(3, 4);
    fmt.Println(n.String());
}
```

- Go's `any` is C# `object` ([Empty Interface](#empty-interface-any)), so `Token` is an alias of `object`.
- `Named` is defined over the named interface `Stringer`, so it is an alias of `Stringer`. `main_package` is the class
  that holds package `main`.
- `Stringer` itself becomes a C# interface ([Interfaces](#interfaces)). The `point` struct satisfies it through its
  `String` method, which becomes a static extension method ([Functions and Methods](#functions-and-methods)).
- `new point(3, 4)` is the Go composite literal `point{3, 4}` ([Struct Types](#struct-types)).
- `@string` is golib's Go-style byte string, and `"(%d,%d)"u8` is a UTF-8 literal that becomes one
  ([Strings](#strings-string-and-sstring)). `nint` is Go's `int` ([Integer Types](#integer-types-and-arithmetic)).

**An alias target is written out in full.** Ordinary converted code relies on two things: `namespace go`, and
project-wide names such as `int64` that stand for C# types. So the `Header` struct writes `@string` and `int64`. C#
reads a `using` alias's target without either, so the same types are written `go.@string` and `long` there:

<!-- source: src/tests/Behavioral/PackageAliasRootedTypeArgs/main.go:35-88 -->
```go
type Header struct {
	Name string
	Size int64
}
…
type (
…
	names = []string
…
	sizes = map[string]int64
	u8s   = []uint8
	ifs   = []any
	cplx2 = []complex128
…
	fn  = func(string) int
	fn2 = func(Header) (string, error)
	fn0 = func()
…
)
```
<!-- source: src/tests/Behavioral/PackageAliasRootedTypeArgs/main.cs.target:1-37 -->
```csharp
global using names = go.slice<go.@string>;
…
global using sizes = go.map<go.@string, long>;
global using u8s = go.slice<byte>;
global using ifs = go.slice<object>;
global using cplx2 = go.slice<System.Numerics.Complex>;
…
global using fn = System.Func<go.@string, nint>;
global using fn2 = System.Func<go.main_package.Header, (go.@string, go.error)>;
global using fn0 = System.Action;
…
[GoType] partial struct Header {
    public @string Name;
    public int64 Size;
}
```

- Golib types carry their `go.` path: `go.@string`, `go.error`, and the [slice](#slices-and-arrays) and
  [map](#maps) types `go.slice<T>` and `go.map<K, V>`.
- Go's built-in numbers become C# keywords: `int` is `nint`, `int64` is `long` and `uint8` is `byte`
  ([Integer Types](#integer-types-and-arithmetic)). `any` becomes `object`.
- .NET types carry their `System.` path. `complex128` is `System.Numerics.Complex`.
- A func type becomes a `System.Func` or `System.Action` delegate
  ([Function Values and Closures](#function-values-and-closures)). Several results become a C# tuple
  ([Multi-Result Values](#multi-result-values-and-comma-ok-forms)).
- `Header`, a struct in the same package, is written through its package class: `go.main_package.Header`.

**An importing package gets its own copy of each exported alias.** A `global using` reaches only its own project, and
an imported package is a separate project. So go2cs declares every exported alias again in each importer.

Here the library package `AliasImportLib` exports a struct, `Box`, and eight aliases. Five of them are shown:

<!-- source: src/tests/Behavioral/AliasImportLib/lib.go:7-15 -->
```go
type Box struct{ V int }
…
type B2 = Box
type IntFn = func(int) int
type BoxFn = func(Box) Box
type DurFn = func(time.Duration) int
type Act = func()
```

In the library's own project each alias is an ordinary `global using`, such as
`global using IntFn = System.Func<nint, nint>;`. The library also lists each exported alias in its generated
`package_info.cs` file, as an assembly attribute:

<!-- source: src/tests/Behavioral/AliasImportLib/package_info.cs:37 -->
```csharp
[assembly: GoTypeAlias("IntFn", "System.Func<nint, nint>")]
```

When a package imports `AliasImportLib`, go2cs reads those attributes and writes the aliases into the importer's own
`package_info.cs` (the same five are shown). Each name becomes `<Package>ꓸ<Alias>`. The `ꓸ` is a Unicode letter that
stands in for Go's dot, because a C# name cannot contain `.` ([Names and Glyphs](#reading-converted-code-names-and-glyphs)):

<!-- source: src/tests/Behavioral/AliasImport/package_info.cs:13-17 -->
```csharp
global using AliasImportLibꓸAct = System.Action;
global using AliasImportLibꓸB2 = go.AliasImportLib_package.Box;
global using AliasImportLibꓸBoxFn = System.Func<go.AliasImportLib_package.Box, go.AliasImportLib_package.Box>;
global using AliasImportLibꓸDurFn = System.Func<go.time_package.Duration, nint>;
global using AliasImportLibꓸIntFn = System.Func<nint, nint>;
```

Code in the importer then names the alias as `AliasImportLibꓸIntFn` where Go writes `AliasImportLib.IntFn`:

<!-- source: src/tests/Behavioral/AliasImport/main.go:13-14 -->
```go
	var f AliasImportLib.IntFn = func(x int) int { return x + 1 }
	var g AliasImportLib.BoxFn = func(b AliasImportLib.Box) AliasImportLib.Box { return AliasImportLib.Box{V: b.V * 2} }
```
<!-- source: src/tests/Behavioral/AliasImport/main.cs.target:5-18 -->
```csharp
using AliasImportLib = AliasImportLib_package;
…
    AliasImportLibꓸIntFn f = (nint x) => x + 1;
    AliasImportLibꓸBoxFn g = (AliasImportLib.Box b) => new AliasImportLib.Box(V: b.V * 2);
```

- `using AliasImportLib = AliasImportLib_package;` lets `AliasImportLib.` name the library's package class.
- `Box` is a real type declared inside that class, so `AliasImportLib.Box` keeps its dot.
- An alias is not a member of any class, so C# cannot reach it as `AliasImportLib.IntFn`. The `ꓸ` copy stands in.
- The Go func literals become C# lambdas ([Function Values and Closures](#function-values-and-closures)), and the
  struct literal `Box{V: …}` becomes `new Box(V: …)` ([Struct Types](#struct-types)).

**A generic alias is replaced by its target at every use.** A C# `using` alias cannot declare type parameters.
A Go alias is identical to its target, so each use simply names the target, and the declaration survives as a comment.
A plain alias in the same file still gets its `global using`:

<!-- source: src/tests/Behavioral/GenericTypeAlias/main.go:9-23 -->
```go
type Pair[K comparable, V any] struct {
	Key K
	Val V
}

// Local generic aliases: of a generic type, partially instantiated, an alias of an alias, and of a map.
type P[K comparable, V any] = Pair[K, V]
type StrPair[V any] = Pair[string, V]
type SP[V any] = StrPair[V]
type IntMap[V any] = map[int]V

// A local PLAIN alias keeps its name.
type Pairs = []Pair[string, int]

func swap[T comparable](p P[T, T]) P[T, T] { return P[T, T]{Key: p.Val, Val: p.Key} }
```
<!-- source: src/tests/Behavioral/GenericTypeAlias/main.cs.target:1-22 -->
```csharp
global using Pairs = go.slice<go.main_package.Pair<go.@string, nint>>;
…
[GoType] partial struct Pair<K, V> {
    public K Key;
    public V Val;
}
// type P[K comparable, V any] = Pair[K, V]
// type StrPair[V any] = Pair[string, V]
// type SP[V any] = StrPair[V]
// type IntMap[V any] = map[int]V

internal static Pair<T, T> swap<T>(Pair<T, T> p) {
    return new Pair<T, T>(Key: p.Val, Val: p.Key);
}
```

`Pair<K, V>` is Go's generic struct as a C# generic struct ([Generics](#generics)). `swap` takes and returns
`P[T, T]` in Go, and `Pair<T, T>` in C#.

Every form expands the same way. A partially instantiated alias fills in its fixed argument, and an alias of an alias
resolves all the way to the real type:

<!-- source: src/tests/Behavioral/GenericTypeAlias/main.go:34-37 -->
```go
	var p P[string, int] = P[string, int]{Key: "a", Val: 1}
	sp := StrPair[bool]{Key: "b", Val: true}
	var sp2 SP[bool] = sp
	m := IntMap[string]{1: "one"}
```
<!-- source: src/tests/Behavioral/GenericTypeAlias/main.cs.target:38-41 -->
```csharp
    Pair<@string, nint> p = new Pair<@string, nint>(Key: "a"u8, Val: 1);
    var sp = new Pair<@string, bool>(Key: "b"u8, Val: true);
    Pair<@string, bool> sp2 = sp;
    var m = new map<nint, @string>{[1] = "one"u8};
```

`StrPair[bool]` and `SP[bool]` both become `Pair<@string, bool>`, and `IntMap[string]` becomes golib's
`map<nint, @string>` ([Maps](#maps)). `"a"u8` is a UTF-8 literal that becomes a golib `@string`
([Strings](#strings-string-and-sstring)).

**Full detail:** [Reference → Type Aliasing](ConversionStrategies-Reference/type-aliasing.md#type-aliasing) — how a defined type converts to and from its underlying type, the qualification rule for every kind of alias target, how an importer reads alias records, aliases of aliases, and the generic-alias forms that are not supported.

---

## Functions and Methods

A Go package becomes a C# `partial class` named `<name>_package` ([Package Conversion](#package-conversion)).
Each Go function becomes a `static` method of that class. Each Go method becomes a C# extension method:
the receiver is the first parameter, marked `this`, so a call such as `d.speak()` reads the same in both languages.

The receiver's C# form shows how the method uses it:

| Go receiver | C# receiver | Used when |
|---|---|---|
| `(d dog)` | `this dog d` | A value receiver: the method works on its own copy |
| `(c *cat)` | `[GoRecv] this ref cat c` | A pointer receiver that only reads and writes through the pointer |
| `(r *ring)` | `this ж<ring> Ꮡr` | A pointer receiver that needs the pointer itself: it stores, returns or compares it, takes a field's address, or passes it to a method that needs the box |

`ж<T>` (read "zhe") is golib's heap box, the C# form of Go's pointer `*T`. By convention, a name with the
`Ꮡ` prefix, such as `Ꮡr`, holds such a box. [Pointers](#pointers) explains both, and
[Names and Glyphs](#reading-converted-code-names-and-glyphs) lists every glyph.

**A value receiver is `this T`, and a pointer receiver is `[GoRecv] this ref T`.** A value receiver gets its
own copy, as in Go. A pointer receiver reads and writes the caller's storage through a C# `ref`, with no box
and no allocation. `[GoType]` marks a converted Go struct ([Struct Types](#struct-types)):

<!-- source: src/tests/Behavioral/AnyStringLitChanSend/main.go:9-23 -->
```go
type dog struct {
	name string
}

func (d dog) speak() string {
	return "woof:" + d.name
}

type cat struct {
	name string
}

func (c *cat) speak() string {
	return "meow:" + c.name
}
```
<!-- source: src/tests/Behavioral/AnyStringLitChanSend/main.cs.target:11-25 -->
```csharp
[GoType] partial struct dog {
    internal @string name;
}

internal static @string speak(this dog d) {
    return "woof:"u8 + d.name;
}

[GoType] partial struct cat {
    internal @string name;
}

[GoRecv] internal static @string speak(this ref cat c) {
    return "meow:"u8 + c.name;
}
```

`@string` is golib's Go string, and `"woof:"u8` is a C# UTF-8 literal ([Strings](#strings-string-and-sstring)).
`[GoRecv]` asks a [source generator](#source-generators) for a second overload that takes the box,
`this ж<cat>`, and forwards to this method. So `speak` works on a `cat` value and on a `*cat` pointer, as in Go.

**A value receiver copies arrays too.** Go copies a struct's arrays whenever it copies the struct. golib's
`array<T>` is a C# struct over a shared `T[]`, so a plain C# struct copy would share the elements. A value
receiver whose struct holds an array therefore starts with `d = d.ΔClone();`, a full copy that a
[source generator](#source-generators) writes. The `Δ` prefix keeps the name from clashing with a Go name
([Slices and Arrays](#slices-and-arrays)):

<!-- source: src/tests/Behavioral/StructArrayFieldValueCopy/StructArrayFieldValueCopy.go:10-26 -->
```go
type digest struct {
	h  [4]int
	x  [2]byte
	nx int
}

func (d *digest) bump() {
	for i := range d.h {
		d.h[i]++
	}
	d.x[0]++
	d.nx++
}

func (d digest) show() string {
	return fmt.Sprintf("%v %v %d", d.h, d.x, d.nx)
}
```
<!-- source: src/tests/Behavioral/StructArrayFieldValueCopy/StructArrayFieldValueCopy.cs.target:7-25 -->
```csharp
[GoType] partial struct digest {
    internal array<nint> h = new(4);
    internal array<byte> x = new(2);
    internal nint nx;
}

[GoRecv] internal static void bump(this ref digest d) {
    foreach (var (i, _) in d.h) {
        d.h[i]++;
    }
    d.x[0]++;
    d.nx++;
}

internal static @string show(this digest d) {
    d = d.ΔClone();

    return fmt.Sprintf("%v %v %d"u8, d.h, d.x, d.nx);
}
```

Go's `int` is C# `nint` ([Integer Types and Arithmetic](#integer-types-and-arithmetic)), and `range` becomes
`foreach` ([Loops, Range and Labels](#loops-range-and-labels)). The pointer receiver `bump` needs no clone,
because it works on the caller's own value.

**A method that needs the pointer itself takes the box, `this ж<T>`.** This happens when the body stores,
returns or compares the receiver, takes the address of one of its fields, or passes the receiver to a
method that needs the box. `initSelf` stores `r` in a field, so it takes the box `Ꮡr`. `linkTo` only writes
a field, so it keeps `this ref ring r`:

<!-- source: src/tests/Behavioral/ReceiverPointerValue/main.go:15-28 -->
```go
type ring struct {
	data int
	next *ring
}

// receiver assigned to its own pointer field (self-link)
func (r *ring) initSelf() {
	r.next = r
}

// receiver linked to another node
func (r *ring) linkTo(other *ring) {
	r.next = other
}
```
<!-- source: src/tests/Behavioral/ReceiverPointerValue/main.cs.target:7-20 -->
```csharp
[GoType] partial struct ring {
    internal nint data;
    internal ж<ring> next;
}

internal static void initSelf(this ж<ring> Ꮡr) {
    ref var r = ref Ꮡr.DerefOrNull();

    r.next = Ꮡr;
}

[GoRecv] internal static void linkTo(this ref ring r, ж<ring> Ꮡother) {
    r.next = Ꮡother;
}
```

The first line of `initSelf` binds `r` as a C# `ref` to the boxed value, so the body reads like Go, and
`Ꮡr` is the pointer itself. For a nil box, golib's `DerefOrNull()` returns a null `ref`. The first read or
write through `r` then fails, and golib reports it as Go's nil-pointer panic
([Implicit Pointer Dereferencing](#implicit-pointer-dereferencing)).
<!-- DerefOrNull returns Unsafe.NullRef<T>() for a nil box; the NullReferenceException at first use maps to "invalid memory address or nil pointer dereference": src/core/golib/ж.PointerExtensions.cs:445 -->

**On a pointer, a call reads as it does in Go.** Besides its use as a name prefix, `Ꮡ` is also a golib
function: `Ꮡ(value)` puts a value in a new box, like Go's `&`. So `&ring{…}` becomes `Ꮡ(new ring(…))`.
A box method such as `initSelf` takes the box directly. A `this ref` method such as `linkTo` binds the
`ж<T>` overload that `[GoRecv]` generates:

<!-- source: src/tests/Behavioral/ReceiverPointerValue/main.go:58-69 -->
```go
	a := &ring{data: 1}
	a.initSelf() // a.next = a
	…
	b := &ring{data: 2}
	c := &ring{data: 3}
	b.linkTo(c)
```
<!-- source: src/tests/Behavioral/ReceiverPointerValue/main.cs.target:45-52 -->
```csharp
    var a = Ꮡ(new ring(data: 1));
    a.initSelf();
    …
    var b = Ꮡ(new ring(data: 2));
    var c = Ꮡ(new ring(data: 3));
    b.linkTo(c);
```
<!-- builtin Ꮡ: src/core/golib/builtin.cs:2090 (public static ж<T> Ꮡ<T>(in T target)) -->

**A value method called on a pointer runs on a copy, as in Go.** The call reads the value first with `~`.
golib overloads C#'s `~` operator on `ж<T>` to mean Go's `*p`, so `~pb` is not a bitwise NOT. Here `pb`
points at `PeopleByAge`, a named slice type whose `Len` method has a value receiver, and `@new<T>()` is Go's `new(T)`:

<!-- source: src/tests/Behavioral/SortArrayType/SortArrayType.go:231-233 -->
```go
	pb := new(PeopleByAge)
	…
	fmt.Println(pb.Len()) // 3
```
<!-- source: src/tests/Behavioral/SortArrayType/SortArrayType.cs.target:200-202 -->
```csharp
    var pb = @new<PeopleByAge>();
    …
    fmt.Println((~pb).Len());
```

**On a value, Go takes the address for you, and so does the converted code.** In this complete program,
`c` is a plain `Counter` value. `Get` only reads a field, so `c.Get()` passes `c` by `ref`. `Set` and `Add`
take the address of the field `c.n`, so they need the box, and `c` lives in one:

<!-- source: src/tests/Behavioral/ReceiverFieldAddress/main.go:7-27 -->
```go
type Counter struct {
	n int32
}

func bump(p *int32, delta int32) int32 {
	*p += delta
	return *p
}

func (c *Counter) Add(delta int32) int32 { return bump(&c.n, delta) }
func (c *Counter) Set(v int32)           { *(&c.n) = v }
func (c *Counter) Get() int32            { return c.n }

func main() {
	var c Counter // value receiver; pointer-receiver methods called on it
	c.Set(100)
	fmt.Println("after Set:", c.Get())   // 100
	fmt.Println("Add 10:", c.Add(10))    // 110
	fmt.Println("Add 5:", c.Add(5))      // 115
	fmt.Println("final:", c.Get())       // 115
}
```
<!-- source: src/tests/Behavioral/ReceiverFieldAddress/main.cs.target:7-43 -->
```csharp
[GoType] partial struct Counter {
    internal int32 n;
}

internal static int32 bump(ref int32 p, int32 delta) {
    p += delta;
    return p;
}

public static int32 Add(this ж<Counter> Ꮡc, int32 delta) {
    ref var c = ref Ꮡc.DerefOrNull();

    return bump(ref nonnil(ref c).n, delta);
}

public static void Set(this ж<Counter> Ꮡc, int32 v) {
    (Ꮡc.of(Counter.Ꮡn)).Value = v;
}

[GoRecv] public static int32 Get(this ref Counter c) {
    return c.n;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object afterSetˢ = (@string)"after Set:"u8;
private static readonly object add10ˢ = (@string)"Add 10:"u8;
private static readonly object add5ˢ = (@string)"Add 5:"u8;
private static readonly object finalˢ = (@string)"final:"u8;

internal static void Main() {
    ref var c = ref heap(new Counter(), out var Ꮡc);
    Ꮡc.Set(100);
    fmt.Println(afterSetˢ, c.Get());
    fmt.Println(add10ˢ, Ꮡc.Add(10));
    fmt.Println(add5ˢ, Ꮡc.Add(5));
    fmt.Println(finalˢ, c.Get());
}
```

Reading the C# piece by piece:

- `heap(new Counter(), out var Ꮡc)` puts `c` in a box. `c` is a `ref` to the boxed value for ordinary use,
  and `Ꮡc` is the box that `Set` and `Add` receive ([Pointers](#pointers)).
- `Counter.Ꮡn` is a static accessor that a [source generator](#source-generators) adds to `Counter` for its
  field `n`. `Ꮡc.of(Counter.Ꮡn)` is Go's `&c.n`, a pointer to that field inside the box, and `.Value` is Go's `*`.
  `Set` uses only the box, so it binds no `ref var c`.
- `bump` only dereferences its pointer parameter, so it takes `ref int32 p`, and `Add` passes `ref nonnil(ref c).n`.
  Go panics at `&c.n` when `c` is nil, before the call. A C# `ref` to a field of a null `ref` does not fail
  when formed, so golib's `nonnil` checks first and raises Go's nil-pointer panic.
  `initSelf` needs no such check, because its plain field write fails at once.
- `afterSetˢ` and the other names ending in `ˢ` are the program's string literals. Go keeps literals in
  read-only memory, so the converter creates each one once, in a `static readonly` field. Each is typed
  `object`, Go's `any`, because its only use is an `any` argument to `fmt.Println`. Storing it as `object`
  avoids boxing the `@string` again on every call ([Strings](#strings-string-and-sstring),
  [Empty Interface](#empty-interface-any)).

<!-- nonnil rationale: src/core/golib/builtin.cs:2340-2348 (Go panics eagerly at &e.x; an interior reference over a null base faults nowhere at formation) -->

**Go's export rule becomes C# access.** A capitalized name is `public`, and any other name is `internal`
([Names and Glyphs](#reading-converted-code-names-and-glyphs)). So `Add` is `public` and `bump` is `internal`.
The entry point `func main` becomes `internal static void Main()`.

A method is `public` only when its type is emitted `public` too. That is an exported type, or an unexported
type that an exported signature exposes. C# forbids a public member from exposing an internal type, so such
a type is emitted `public` itself.
<!-- source: src/tests/Behavioral/PublicizedFieldType/main.go:52-55 (type tally int; func (cr CaseRange) Tally() tally) -> main.cs.target:29 ([GoType("num:nint")] public partial struct tally;) and :33 (public static tally Tally(this CaseRange cr)); receiver clamp with the publicized exception: src/go2cs/visitFuncDecl.go:1666-1672 -->

**A single named result stays on the signature as a comment.** The body declares it as a local, set to its
zero value, and a bare `return` returns it. `default!` is C#'s zero value
([Nil and Zero Values](#nil-and-zero-values)). Several results, named or not, become a C# tuple
([Multi-Result Values](#multi-result-values-and-comma-ok-forms)).

<!-- source: src/tests/Behavioral/NamedSliceConversion/main.go:25-30 -->
```go
func sum(s []int) (total int) {
	for _, v := range s {
		total += v
	}
	return
}
```
<!-- source: src/tests/Behavioral/NamedSliceConversion/main.cs.target:19-26 -->
```csharp
internal static nint /*total*/ sum(slice<nint> s) {
    nint total = default!;

    foreach (var (_, v) in s) {
        total += v;
    }
    return total;
}
```

`slice<nint>` is golib's Go slice of `int` ([Slices and Arrays](#slices-and-arrays)).

**A variadic parameter is a `params` span.** The name `ꓸꓸꓸ` is made of a Unicode letter that looks like a dot,
so it is a legal C# name that reads as Go's `...`. `ꓸꓸꓸnint` is a file-level alias for `Span<nint>`. The raw
pack arrives with the `ʗp` suffix, as `xsʗp`, and the body binds the Go name `xs`:

<!-- source: src/tests/Behavioral/VariadicPackPassThrough/VariadicPackPassThrough.go:21-27 -->
```go
func sum(xs ...int) int {
	t := 0
	for _, x := range xs {
		t += x
	}
	return t
}
```
<!-- source: src/tests/Behavioral/VariadicPackPassThrough/VariadicPackPassThrough.cs.target:5-31 -->
```csharp
using ꓸꓸꓸnint = Span<nint>;
…
internal static nint sum(params ꓸꓸꓸnint xsʗp) {
    var xs = xsʗp.sslice();

    nint t = 0;
    foreach (var (_, x) in xs) {
        t += x;
    }
    return t;
}
```

When every use of the pack stays inside the call, as here, `xs` is an `sslice<nint>`. That is a stack-only
slice view, a C# `ref struct`, that allocates nothing. When the pack escapes the call, the body binds
`xsʗp.slice()`, a heap `slice<nint>` copy ([Slices and Arrays](#slices-and-arrays)).

A call such as `sum(5)` stays `sum(5)`. Spreading a slice, `sum(a...)`, becomes `sum(a.ꓸꓸꓸ)`. The golib
property `.ꓸꓸꓸ` returns a `Span` over the slice's own storage, so nothing is copied.
<!-- source: src/tests/Behavioral/VariadicPackPassThrough/VariadicPackPassThrough.go:66 (fmt.Println("sum:", sum(a...), sum(), sum(5))) -> VariadicPackPassThrough.cs.target:108 (fmt.Println(sumˢ, sum(a.ꓸꓸꓸ), sum(), sum(5));); property: src/core/golib/slice.cs:490 (public Span<T> ꓸꓸꓸ => ToSpan()); sslice is a ref struct: src/core/golib/sslice.cs:19 -->

**An unnamed parameter gets a made-up name**, because C# requires one. A lone one is `_`. Otherwise they
become `_Δp0`, `_Δp1` and so on, and an unnamed variadic pack is just `ʗp`:

<!-- lone blank: src/tests/Behavioral/CaptureHoistThroughConversion/main.cs.target:7 (public delegate void Handler(nint _);) -->
<!-- source: src/tests/Behavioral/ReflectBridgeClosure/main.go:68 -->
```go
func variadic(string, ...int)             {}
```
<!-- source: src/tests/Behavioral/ReflectBridgeClosure/main.cs.target:64-65 -->
```csharp
internal static void variadic(@string _Δp0, params ꓸꓸꓸnint ʗp) {
}
```

**Each `init` becomes a `[GoInit]` method.** `[GoInit]` is golib's name for .NET's `[ModuleInitializer]`
attribute, so .NET runs the method once, when the package's assembly loads. [Package Conversion](#package-conversion)
explains how imported packages are made to initialize first. C# cannot declare two methods with the same
name and signature, so the first `init` keeps its name and the rest take a `Δ` suffix and a number, counted across the whole package:

<!-- source: src/tests/Behavioral/MultiFileInitOrder/a_first.go:8-16 -->
```go
var order []string

func init() {
	order = append(order, "a_first#1")
}

func init() {
	order = append(order, "a_first#2")
}
```
<!-- source: src/tests/Behavioral/MultiFileInitOrder/a_first.cs.target:5-13 -->
```csharp
internal static slice<@string> order;

[GoInit] internal static void init() {
    order = append(order, "a_first#1"u8);
}

[GoInit] internal static void initΔ1() {
    order = append(order, "a_first#2"u8);
}
```

A `u8` literal converts to `@string` implicitly, so `append` accepts it ([Strings](#strings-string-and-sstring)).
<!-- source: src/tests/Behavioral/MultiFileInitOrder/b_second.cs.target:5 and :9 (initΔ2, initΔ3); main.cs.target:7 (initΔ4). [GoInit] is ModuleInitializerAttribute: src/core/golib/builtin.cs:231. implicit operator @string(ReadOnlySpan<byte>): src/core/golib/string.cs:506 -->

Using a method as a value, such as `f := c.String`, is covered in
[Function Values and Closures](#function-values-and-closures). Methods on generic types are covered in [Generics](#generics).

**Full detail:** [Reference → Pointers](ConversionStrategies-Reference/pointers.md#pointers) — more on receivers and pointer parameters: box-taking methods called through fields, globals and slice elements, and nil receivers.

---

<a id="delegates-to-value-receiver-instances"></a>

## Function Values and Closures

Go func types become C# delegates. Func literals become C# lambdas or C# local functions. A closure shares
the variables it captures, as in Go, so a write on either side is seen by the other.

The examples use a few golib names. golib is the go2cs runtime library
([The golib Runtime Library](#the-golib-runtime-library)). `nint` is Go's `int`
([Integer Types and Arithmetic](#integer-types-and-arithmetic)). `@string` is Go's `string`, and `"hi"u8`
is a C# UTF-8 literal that becomes an `@string` where a string is needed
([Strings](#strings-string-and-sstring)).

**A func type becomes `Func<…>` or `Action<…>`.** Parameters map in order. A func with no result becomes an
`Action`, and several results become one tuple result
([Multi-Result Values](#multi-result-values-and-comma-ok-forms)). A nil func is a null delegate
([Nil and Zero Values](#nil-and-zero-values)).

<!-- source: src/tests/Behavioral/MethodValueReceiverEscape/main.go:42-48 -->
```go
func applyInt(f func(int) int, a int, b int) int { return f(a) + f(b) }

func applyPush(f func(int)) {
	f(1)
	f(2)
	f(3)
}
```
<!-- source: src/tests/Behavioral/MethodValueReceiverEscape/main.cs.target:32-40 -->
```csharp
internal static nint applyInt(Func<nint, nint> f, nint a, nint b) {
    return f(a) + f(b);
}

internal static void applyPush(Action<nint> f) {
    f(1);
    f(2);
    f(3);
}
```

**A variadic func type uses golib's [`Funcꓸꓸꓸ<…>`](../src/core/golib/variadic.cs) or `Actionꓸꓸꓸ<…>`.**
The glyph `ꓸꓸꓸ` reads as Go's `...`. The type arguments list the fixed parameters, then the variadic
element type, then the result. Calls through the value pass loose arguments, as in Go.

<!-- source: src/tests/Behavioral/VariadicFuncValues/main.go:23-29 -->
```go
func apply(f func(prefix string, vals ...int) string) {
	fmt.Println(f("loose", 1, 2, 3))
	fmt.Println(f("empty"))

	nums := []int{4, 5}
	fmt.Println(f("spread", nums...))
}
```
<!-- source: src/tests/Behavioral/VariadicFuncValues/main.cs.target:19-24 -->
```csharp
internal static void apply(Funcꓸꓸꓸ<@string, nint, @string> f) {
    fmt.Println(f("loose"u8, 1, 2, 3));
    fmt.Println(f("empty"u8));
    var nums = new nint[]{4, 5}.slice();
    fmt.Println(f("spread"u8, nums.ꓸꓸꓸ));
}
```

`new nint[]{4, 5}.slice()` is the slice literal `[]int{4, 5}` ([Slices and Arrays](#slices-and-arrays)).
The spread `nums...` becomes `nums.ꓸꓸꓸ`. [Functions and Methods](#functions-and-methods) covers variadic
parameters.

**A named func type with methods becomes a C# `delegate`.** Its methods become extension methods on the
delegate, as for any named type ([Functions and Methods](#functions-and-methods)).

<!-- source: src/tests/Behavioral/MethodExpression/main.go:74-76 -->
```go
type reader func() int

func (f reader) sum(extra int) int { return f() + extra }
```
<!-- source: src/tests/Behavioral/MethodExpression/main.cs.target:61-65 -->
```csharp
internal delegate nint reader();

internal static nint sum(this reader f, nint extra) {
    return f() + extra;
}
```

**A named func type without methods usually has no declaration of its own.** Each use is written as the
underlying `Func<…>` or `Action<…>`, because Go converts freely between the two. A comment marks where the
declaration would be. In this example, `Stringy` as a return type is written `Func<@string>`.

**A func literal used as a value becomes a lambda.** In the same example, `returnsAFunction` returns a
literal, so the literal becomes the C# lambda `() => { … }`.

<!-- source: src/tests/Behavioral/LambdaFunctions/LambdaFunctions.go:5-20 -->
```go
type Stringy func() string
…
func returnsAFunction() Stringy {
	return func() string {
		fmt.Printf("Inner stringy function\n")
		return "bar" // have to return a string to be stringy
	}
}
```
<!-- source: src/tests/Behavioral/LambdaFunctions/LambdaFunctions.cs.target:7-28 -->
```csharp
// type Stringy is a methodless func type — rendered inline as its base delegate
…
// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string barˢ = "bar"u8;

internal static Func<@string> returnsAFunction() {
    return () => {
        fmt.Printf("Inner stringy function\n"u8);
        return barˢ;
    };
}
```

`barˢ` is the `"bar"` literal, created once as a static field. The `ˢ` suffix marks such a hoisted literal,
and "RODATA" is Go's read-only data ([Strings](#strings-string-and-sstring)).

**A literal that its own `name := func…` declares, and that is only ever called, becomes a C# local
function.** A local function allocates nothing, while a lambda allocates a delegate. C# closures capture
variables, not values, as Go's do. So `add` sees the later write to `base`, and this prints `P2: 23 27`.

<!-- source: src/tests/Behavioral/LocalFunctionEmission/main.go:55-60 -->
```go
func p2() {
	base := 10
	add := func(a, b int) int { return a + b + base }
	base = 20
	fmt.Println("P2:", add(1, 2), add(3, 4))
}
```
<!-- source: src/tests/Behavioral/LocalFunctionEmission/main.cs.target:37-42 -->
```csharp
internal static void p2() {
    nint @base = 10;
    nint add(nint a, nint b) => a + b + @base;
    @base = 20;
    fmt.Println((@string)"P2:"u8, add(1, 2), add(3, 4));
}
```

`base` is a C# keyword, so the local is written `@base`
([Names and Glyphs](#reading-converted-code-names-and-glyphs)). `fmt.Println` takes `any` arguments, so the
literal needs an explicit cast, `(@string)"P2:"u8`, to become a boxable `@string`. A literal this short stays
inline; a longer one becomes a hoisted field ([Strings](#strings-string-and-sstring)).

**A literal whose name is also used as a value stays a lambda.** A local function has no value form. Here
`f := next` copies the function value, so `next` is a lambda. Both names share one `c`, and this prints
`P4: 2 3 3`.

<!-- source: src/tests/Behavioral/LocalFunctionEmission/main.go:80-86 -->
```go
func p4() {
	c := 0
	next := func() int { c++; return c }
	next()
	f := next
	fmt.Println("P4:", f(), next(), c)
}
```
<!-- source: src/tests/Behavioral/LocalFunctionEmission/main.cs.target:60-69 -->
```csharp
internal static void p4() {
    nint c = 0;
    var next = () => {
        c++;
        return c;
    };
    next();
    var f = next;
    fmt.Println((@string)"P4:"u8, f(), next(), c);
}
```

**How a closure reaches a captured variable depends on how that variable is stored.** A struct or array
local that a closure captures is usually kept in a heap box, much as Go's escape analysis moves a captured
variable to the heap ([Pointers](#pointers)). A C# lambda or local function cannot capture the `ref` alias
of a boxed value, so the converter picks one of three routes:

| Captured variable | The closure reaches it | Example |
|:--|:--|:--|
| A local of a basic type (number, bool, string) | Directly, as a plain C# capture | `@base` in `p2` |
| A boxed local that is written after the closure exists | Through its box, `Ꮡt.Value` | `probeA1` |
| A struct, array, slice, map or channel that nothing writes afterward | Through a copy, `tʗ1` | `probeA3` |

**A boxed local written after capture is reached through its box.** golib's `heap<Tally>(out var Ꮡt)`
allocates a box `Ꮡt` holding a zero `Tally`, and `t` is a C# `ref` alias of the value inside it. The
[`Ꮡ` prefix](#reading-converted-code-names-and-glyphs) marks a box, and `Ꮡt.Value` is the same value
reached through the box. The local function `bump` writes through `Ꮡt.Value`, so both sides change one
variable, and this prints `A1: 106 s`, as Go does.

<!-- source: src/tests/Behavioral/ClosureWriteVisibility/main.go:14-31 -->
```go
type Tally struct {
	total int
	log   string
}
…
func probeA1() {
	t := Tally{5, "s"}
	bump := func() { t.total += 100 }
	bump()
	t.total++
	fmt.Println("A1:", t.total, t.log)
}
```
<!-- source: src/tests/Behavioral/ClosureWriteVisibility/main.cs.target:7-26 -->
```csharp
[GoType] partial struct Tally {
    internal nint total;
    internal @string log;
}
…
internal static void probeA1() {
    ref var t = ref heap<Tally>(out var Ꮡt);
    t = new Tally(5, "s"u8);
    void bump() {
        Ꮡt.Value.total += 100;
    }
    bump();
    t.total++;
    fmt.Println((@string)"A1:"u8, t.total, t.log);
}
```

`[GoType]` marks a converted Go type ([Struct Types](#struct-types)).

**A captured variable that nothing writes after the closure exists is read through a copy.** The copy
always matches, because the variable never changes afterward. The `ʗ` suffix marks a copy the converter
makes for a closure or a method value, and the number keeps the names unique, as in `tʗ1`
([detail](ConversionStrategies-Reference/pointers.md#a-capture-that-is-written-after-the-capture-point-routes-to-shared-storage-not-a-snapshot)).
This uses the same `Tally` struct, and prints `A3: 7`.

<!-- source: src/tests/Behavioral/ClosureWriteVisibility/main.go:42-47 -->
```go
func probeA3() {
	t := Tally{5, "s"}
	t.total += 2
	get := func() int { return t.total }
	fmt.Println("A3:", get())
}
```
<!-- source: src/tests/Behavioral/ClosureWriteVisibility/main.cs.target:36-43 -->
```csharp
internal static void probeA3() {
    ref var t = ref heap<Tally>(out var Ꮡt);
    t = new Tally(5, "s"u8);
    t.total += 2;
    var tʗ1 = t;
    nint get() => tʗ1.total;
    fmt.Println((@string)"A3:"u8, get());
}
```

**A value-receiver method value copies its receiver.** `d.printName` binds the value `d` has at that
moment, so this program prints `Name = James` twice. The converter copies `d` into `dʗ1` and calls the
method on the copy from a lambda.

<!-- source: src/tests/Behavioral/VariableCapture/VariableCapture.go:5-19 -->
```go
type data struct {
	name string
}

func (d data) printName() {
	fmt.Println("Name =", d.name)
}

func main() {
	d := data{name: "James"}
	f1 := d.printName
	f1()
	d.name = "Gretchen"
	f1()
}
```
<!-- source: src/tests/Behavioral/VariableCapture/VariableCapture.cs.target:7-29 -->
```csharp
[GoType] partial struct data {
    internal @string name;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object nameˢ = (@string)"Name ="u8;

internal static void printName(this data d) {
    fmt.Println(nameˢ, d.name);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string gretchenˢ = "Gretchen"u8;

internal static void Main() {
    var d = new data(name: "James"u8);
    
    var dʗ1 = d;
    var f1 = () => dʗ1.printName();
    f1();
    d.name = gretchenˢ;
    f1();
}
```

`nameˢ` and `gretchenˢ` are hoisted string literals. Each is declared just before the first function that
uses it, which is why the header comment appears twice. `nameˢ` is typed `object` because it is only ever
passed to `fmt.Println` as an `any`, so it is boxed once ([Strings](#strings-string-and-sstring)).

**A pointer-receiver method value binds the variable itself.** `c.dec` is Go shorthand for `(&c).dec`, so
`c` moves into a heap box `Ꮡc`. That box has golib type `ж<counter>`: `ж<T>` is golib's heap box, and a
Go `*T` is a reference to one ([Pointers](#pointers)). The delegate binds to the box, so every write `dec`
makes lands in `c`. `viaLocal` returns `c.n` as 88, as in Go.

<!-- source: src/tests/Behavioral/MethodValueReceiverEscape/main.go:17-56 -->
```go
type counter struct {
	n     int
	calls int
}

// Pointer receiver: writes must reach the caller's variable.
func (c *counter) dec(step int) int {
	c.n -= step
	c.calls++
	return c.n
}
…
func viaLocal() (int, int) {
	c := counter{n: 100}
	sum := applyInt(c.dec, 5, 7)
	return c.n, sum
}
```
<!-- source: src/tests/Behavioral/MethodValueReceiverEscape/main.cs.target:7-47 -->
```csharp
[GoType] partial struct counter {
    internal nint n;
    internal nint calls;
}

[GoRecv] internal static nint dec(this ref counter c, nint step) {
    c.n -= step;
    c.calls++;
    return c.n;
}
…
internal static (nint, nint) viaLocal() {
    ref var c = ref heap<counter>(out var Ꮡc);
    c = new counter(n: 100);
    nint sum = applyInt(Ꮡc.dec, 5, 7);
    return (c.n, sum);
}
```

A pointer-receiver method is written as an extension method on `this ref counter`. Its `[GoRecv]` attribute
tells a [source generator](#source-generators) to add the matching `dec` overload on `ж<counter>`, which
`Ꮡc.dec` binds to ([Functions and Methods](#functions-and-methods)). `applyInt` is the function from the
first example in this section.

**Full detail:** [Reference → Function values and method values](ConversionStrategies-Reference/value-receiver-delegates.md#delegates-to-value-receiver-instances) — how each method-value and method-expression form binds its receiver.

---

<a id="labeled-control-flow-and-loop-variables"></a>
## Loops, Range and Labels
<!-- Length: about 330 visible lines, most of them code. Longer than the guide range by the owner's review rule that each rule gets a complete, self-explaining example and every hoisted literal shows its declaration; the copy-back and heap-box rules each gained a complete function from ForLoopPerIterationVars because a first reader could not picture them from words alone. Three rule groups here (for shapes and the constant-true loop, the Coro handoff and panic rethrow, init-clause blocks) have no home on the linked reference page yet, so they stay here in brief. -->

Go has one loop keyword, `for`. The converter turns each Go loop into a C# `for`, `while` or `foreach`,
chosen by the loop's shape, so a converted loop reads much like the original. Two things look new to a
reader: a loop variable is sometimes copied so that each iteration gets its own, and a labeled `break` or
`continue` becomes a `goto`. The small glyphs in these names (`ᐧ`, `Δ`, `ᴛ`, `Ꮡ`, `ˢ`) are explained where
they first appear here, and all of them are listed in [Reading Converted Code](#reading-converted-code-names-and-glyphs).

<!-- Inline forms from src/tests/Behavioral/ForVariants/ForVariants.go:12, :22, :78 -> ForVariants.cs.target:19, :25, :70 -->
<!-- The constant matters for reachability: golib builtin.cs documents that an infinite loop relies on ᐧ folding to avoid CS0161 (not all code paths return a value). -->
**A `for` keeps its shape.** A three-clause loop stays a C# `for`. A loop with only a condition,
`for i < 10 {`, becomes `while (i < 10) {`. An endless `for {` becomes `while (ᐧ) {`, where `ᐧ` is the
constant `true` from [golib](#the-golib-runtime-library), go2cs's runtime library. Because it is a
constant, C# knows the loop never ends, so the function needs no trailing `return`, just as in Go.

This example shows two things: the endless loop, and two loop variables that the converter renames.

<!-- source: src/tests/Behavioral/ForVarMasksBlockLevel/main.go:12 -->
```go
func compute(n int) int {
	total := 0
	count := 0
	for {
		for off := 0; off < n; off++ {
			total += off
		}
		for off := n; off > 0; off-- {
			total += off
		}
		off := n * 10 // block-level, declared after both loops in the same for{} body
		total += off
		count++
		if count >= 2 {
			return total
		}
	}
}
```
<!-- source: src/tests/Behavioral/ForVarMasksBlockLevel/main.cs.target:7 -->
```csharp
internal static nint compute(nint n) {
    nint total = 0;
    nint count = 0;
    while (ᐧ) {
        for (nint offΔ1 = 0; offΔ1 < n; offΔ1++) {
            total += offΔ1;
        }
        for (nint offΔ2 = n; offΔ2 > 0; offΔ2--) {
            total += offΔ2;
        }
        nint off = n * 10;
        total += off;
        count++;
        if (count >= 2) {
            return total;
        }
    }
}
```

Go's `int` becomes `nint` ([Integer Types](#integer-types-and-arithmetic)). The `Δ1` and `Δ2` suffixes
rename the two loop variables. C# does not let an inner scope reuse a name that its enclosing block
also declares, as `off` is here ([Shadowing](#short-variable-redeclaration-shadowing)).

<!-- sources by row (Go -> C#; GOROOT is Go 1.24.13; short paths are under src/tests/Behavioral/):
GOROOT/src/slices/iter.go:16 -> src/core/slices/iter.cs:17; ArrayRangeSnapshot/ArrayRangeSnapshot.go:18 -> .cs.target:21;
StringByteSemantics/main.go:12 -> main.cs.target:12; GenericTypeInference/GenericTypeInference.go:138 -> .cs.target:145;
ChannelSendToClosed/ChannelSendToClosed.go:15 -> .cs.target:18 (the source names the received value `i`; the table names it `v` so a reader does not take it for an index); RangeOverIntegerTypes/main.go:98 -> main.cs.target:80
ΔRangeSnapshot: src/core/golib/array.cs:567 (an allocation-free copy of the array for the loop).
Moved to the reference (range over every integer type): the row `for i := range b` -> `foreach (var i in range<uint8>(b))`,
RangeOverIntegerTypes/main.go:46 -> main.cs.target:37 -->
**Every `range` becomes a `foreach`.** Each golib collection type enumerates in Go's own terms. Two range
variables become a C# tuple, `var (i, v)`, and a blank `_` in the pair stays `_`:

| Go | C# | Notes |
|---|---|---|
| `for i, v := range s` | `foreach (var (i, v) in s)` | slice: index and element ([Slices and Arrays](#slices-and-arrays)) |
| `for i, v := range a` | `foreach (var (i, v) in a.ΔRangeSnapshot())` | array value: iterates a copy, as Go does ([Slices and Arrays](#slices-and-arrays)) |
| `for i, r := range s` | `foreach (var (i, r) in s)` | string: byte offset and rune ([Strings](#strings-string-and-sstring)) |
| `for k, v := range m` | `foreach (var (k, v) in m)` | map ([Maps](#maps)) |
| `for v := range c` | `foreach (var v in c)` | channel: each received value; ends when closed and drained ([Channels](#channels-and-select)) |
| `for i := range size` | `foreach (var i in range(size))` | integer: golib's `range` helper counts 0 to `size - 1` |

`ΔRangeSnapshot()` is a golib method that gives the loop its own copy of the array. The `Δ` prefix marks a
name chosen so it cannot clash with a Go name ([Names and Glyphs](#reading-converted-code-names-and-glyphs)).

<!-- Moved to the reference ("Reassigned or ref-bound range variable"): a range variable the body reassigns
iterates a temporary and the body declares a writable copy, foreach (var (_, rᴛ1) in s) { var r = rᴛ1; … }
(src/tests/Behavioral/RangeVarReassign/main.cs.target:22). -->
<!-- Temporary names come from src/go2cs/visitRangeStmt.go getTempVarName calls: "i"/"v" for slices, "i"/"r" for strings, the key name for others. -->
**A range that assigns with `=` copies into your variables.** A C# `foreach` must declare its own
variables. So when Go ranges into variables that already exist, the `foreach` declares temporaries, and
the body starts by copying them into the real variables. The `ᴛ` suffix marks a name the converter makes
up. The temporaries take generic names such as `iᴛ1` and `vᴛ1`, not your variables' names:

<!-- source: src/tests/Behavioral/RangeStatements/RangeStatements.go:13 -->
```go
nums := []int{2, 3, 4}
sum := 0

var i, num, total int

for i, num = range nums {
	sum += num
	total += i
}
```
<!-- source: src/tests/Behavioral/RangeStatements/RangeStatements.cs.target:20 -->
```csharp
var nums = new nint[]{2, 3, 4}.slice();
nint sum = 0;
nint i = default!;
nint num = default!;
nint total = default!;
foreach (var (iᴛ1, vᴛ1) in nums) {
    i = iᴛ1;
    num = vᴛ1;

    sum += num;
    total += i;
}
```

`new nint[]{…}.slice()` builds a golib [slice](#slices-and-arrays), Go's `int` becomes `nint`, and
`default!` is Go's zero value ([Nil and Zero Values](#nil-and-zero-values)).

<!-- A defer or go call that references i also selects the per-iteration form (src/go2cs/visitForStmt.go:337);
the copy-back fires when the body writes i or the variable is heap-boxed (visitForStmt.go:353; golden
src/tests/Behavioral/ForLoopPerIterationVars/main.cs.target:52-55, where the body only takes &i). -->
**Loop variables are per-iteration.** Go gives each iteration of `for i := …` its own `i`. A C# `for`
shares one variable across all iterations, so closures made in the loop would all see its last value.
When a closure captures `i`, the converted loop counts with a hidden `iᴛ1` and declares a fresh `i`
from it at the top of each iteration. This Go prints `g1: 0 1 2`, and so does the C#:

<!-- source: src/tests/Behavioral/ForLoopPerIterationVars/main.go:18 -->
```go
func g1() {
	var fs []func() int
	for i := 0; i < 3; i++ {
		fs = append(fs, func() int { return i })
	}
	fmt.Println("g1:", fs[0](), fs[1](), fs[2]())
}
```
<!-- source: src/tests/Behavioral/ForLoopPerIterationVars/main.cs.target:19 -->
```csharp
internal static void g1() {
    slice<Func<nint>> fs = default!;
    for (nint iᴛ1 = 0; iᴛ1 < 3; iᴛ1++) {
        var i = iᴛ1;
        fs = append(fs, () => i);
    }
    fmt.Println((@string)"g1:"u8, fs[0](), fs[1](), fs[2]());
}
```

`slice<Func<nint>>` is a golib [slice](#slices-and-arrays) of C# delegates. The Go function literal
becomes the lambda `() => i` ([Function Values and Closures](#function-values-and-closures)). A very short
literal such as `"g1:"` stays inline as `(@string)"g1:"u8`; most others move to a static field marked `ˢ`
([Strings](#strings-string-and-sstring)).

<!-- The copy-back example: src/tests/Behavioral/ForLoopPerIterationVars/main.go:27 (g2, a body that writes i and
continues) -> main.cs.target:28-47, where `iᴛ1 = i;` appears before the `continue` and at the end of the body.
Output: g2: 3, then g2v: 2, 4, 6. -->
**A body that changes `i` copies it back.** If the body writes `i`, the change must also advance the
loop. So the C# adds `iᴛ1 = i;` at the end of the body and before each `continue`. This Go prints
`g2: 3`, then 2, 4 and 6, and so does the C#:

<!-- source: src/tests/Behavioral/ForLoopPerIterationVars/main.go:27 -->
```go
func g2() {
	var fs []func() int
	for i := 0; i < 6; i++ {
		if i%2 == 0 {
			continue
		}
		i++
		fs = append(fs, func() int { return i })
	}
	fmt.Println("g2:", len(fs))
	for _, f := range fs {
		fmt.Println("g2v:", f())
	}
}
```
<!-- source: src/tests/Behavioral/ForLoopPerIterationVars/main.cs.target:28 -->
```csharp
// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object g2vˢ = (@string)"g2v:"u8;

internal static void g2() {
    slice<Func<nint>> fs = default!;
    for (nint iᴛ1 = 0; iᴛ1 < 6; iᴛ1++) {
        var i = iᴛ1;
        if (i % 2 == 0) {
            iᴛ1 = i;
            continue;
        }
        i++;
        fs = append(fs, () => i);
        iᴛ1 = i;
    }
    fmt.Println((@string)"g2:"u8, len(fs));
    foreach (var (_, f) in fs) {
        fmt.Println(g2vˢ, f());
    }
}
```

`g2vˢ` is the string literal `"g2v:"`, stored once in a static field. It is typed `object` because
`fmt.Println` takes `any` values ([Strings](#strings-string-and-sstring), [Empty Interface](#empty-interface-any)).

**A body that takes `&i` gets a fresh heap box each iteration.** Go's `&i` needs `i` to live on the heap,
so each iteration's `i` is stored in a golib heap box. This Go prints `g3: 0 1 2`, and so does the C#:

<!-- source: src/tests/Behavioral/ForLoopPerIterationVars/main.go:43 -->
```go
func g3() {
	var ps []*int
	for i := 0; i < 3; i++ {
		ps = append(ps, &i)
	}
	fmt.Println("g3:", *ps[0], *ps[1], *ps[2])
}
```
<!-- source: src/tests/Behavioral/ForLoopPerIterationVars/main.cs.target:49 -->
```csharp
internal static void g3() {
    slice<ж<nint>> ps = default!;
    for (nint iᴛ1 = 0; iᴛ1 < 3; iᴛ1++) {
        ref var i = ref heap<nint>(out var Ꮡi);
        i = iᴛ1;
        ps = append(ps, Ꮡi);
        iᴛ1 = i;
    }
    fmt.Println((@string)"g3:"u8, ps[0].Value, ps[1].Value, ps[2].Value);
}
```

`ж<nint>` is golib's heap box, the C# form of Go's `*int`. `heap<nint>(out var Ꮡi)` makes a new box:
`Ꮡi` is the box itself, Go's `&i`, and `i` is a C# `ref` to the value inside it. `.Value` reads through
the box like Go's `*p`. A boxed `i` is always copied back, since code holding the pointer can change it
([Pointers](#pointers)).

<!-- Reference gap: range over a function is covered only in generic-constraints.md (the named/generic Seq rule: .Invoke, spelled-out type arguments, yield false on break). No page states the Coro handoff or the panic rethrow into the ranging function (golib runtime/YieldFunctionEnumerator.cs). Move or add the rule in labels-and-loop-variables.md with its guard tests, and extend the Full detail line. -->
**Range over a function becomes a `foreach` over golib's `range` helper.** In Go, an iterator is a
function that takes a `yield` callback and calls it once per value. A named Go func type such as
`Seq[V]` becomes a C# delegate. The iterator's function literal becomes a lambda that takes
`Func<nint, bool> yield`, C#'s type for a function from `nint` to `bool`:

<!-- source: src/tests/Behavioral/GenericTypeInference/GenericTypeInference.go:110 -->
```go
type Seq[V any] func(yield func(V) bool)
…
func countdown(n int) Seq[int] {
	return func(yield func(int) bool) {
		for i := n; i > 0; i-- {
			if !yield(i) {
				return
			}
		}
	}
}
```
<!-- source: src/tests/Behavioral/GenericTypeInference/GenericTypeInference.cs.target:118 -->
```csharp
public delegate void Seq<V>(Func<V, bool> yield);
…
internal static Seq<nint> countdown(nint n) {
    return (Func<nint, bool> yield) => {
        for (nint i = n; i > 0; i--) {
            if (!yield(i)) {
                return;
            }
        }
    };
}
```

<!-- golib range overloads: src/core/golib/builtin.cs:761 range<T>(Action<Func<T, bool>>) and :774 range<T1, T2>(Action<Func<T1, T2, bool>>). -->
The loop hands the iterator to golib's `range` helper. The helper takes a plain callback,
`Action<Func<T, bool>>`, not the named `Seq<nint>` delegate, so the loop passes the delegate's `.Invoke`
method. C# cannot infer `T` from a method passed this way, so the element type is written out:
`range<nint>`. This loop sums 5, 4 and 3, then stops:

<!-- source: src/tests/Behavioral/GenericTypeInference/GenericTypeInference.go:169 -->
```go
sum := 0
for v := range countdown(5) {
	if v == 2 {
		break // range-over-func break: yield returns false, producer stops
	}
	sum += v
}
fmt.Println(sum)
```
<!-- source: src/tests/Behavioral/GenericTypeInference/GenericTypeInference.cs.target:177 -->
```csharp
nint sum = 0;
foreach (var v in range<nint>(countdown(5).Invoke)) {
    if (v == 2) {
        break;
    }
    sum += v;
}
fmt.Println(sum);
```

<!-- src/core/golib/runtime/Coro.cs:18 (a coro is not a goroutine; control alternates, the two contexts are never runnable at once); YieldFunctionEnumerator.cs:20-24 (seq on a second stack, a real goroutine) and :31-39 (early exit answers false; panic rethrown on the ranging side). -->
Go runs the iterator as an ordinary call on the same goroutine. A C# `foreach` cannot do that, so golib
runs the iterator on a second goroutine and uses its `Coro` handoff type to pass each value to the loop
([Goroutines](#goroutines)). The iterator and the loop take turns and never run at the same time. A
`break` or `return` in the loop makes `yield` return false, so the iterator finishes and runs its defers.

A panic in the iterator is rethrown in the ranging function, where its own defers can recover it
([Defer / Panic / Recover](#defer--panic--recover)).

<!-- Labeled switch placement: src/tests/Behavioral/SwitchBreakInCase/main.go:41 (Big: switch … break Big) -> main.cs.target:50-68, where `break_Big:;` follows the switch's closing brace. -->
**Labels become `goto` targets.** C# `break` and `continue` cannot name an outer loop. So the converter
keeps the Go label, adds `continue_L:;` at the end of the labeled loop's body and `break_L:;` just after
the loop, and turns `continue L` and `break L` into `goto` statements. A labeled `switch` gets the same
`break_L:;`, placed right after the switch's closing brace. A plain Go `goto L` stays `goto L;`:

<!-- source: src/tests/Behavioral/ForVariants/ForVariants.go:58 -->
```go
nums := []int{1, 2, 3, 4}
scan:
	for _, n := range nums {
		for _, m := range nums {
			if n == m {
				continue scan
			}
			if n+m > 5 {
				break scan
			}
			fmt.Println("pair", n, m)
		}
	}
```
<!-- source: src/tests/Behavioral/ForVariants/ForVariants.cs.target:9 -->
```csharp
private static readonly object pairˢ = (@string)"pair"u8;
…
    var nums = new nint[]{1, 2, 3, 4}.slice();
scan:
    foreach (var (_, n) in nums) {
        foreach (var (_, m) in nums) {
            if (n == m) {
                goto continue_scan;
            }
            if (n + m > 5) {
                goto break_scan;
            }
            fmt.Println(pairˢ, n, m);
        }
continue_scan:;
    }
break_scan:;
```

`pairˢ` is the string literal `"pair"`, stored once in a static field; the `ˢ` suffix marks such a field
([Strings](#strings-string-and-sstring)). It is typed `object` because `fmt.Println` takes `any` values
([Empty Interface](#empty-interface-any)). `new nint[]{…}.slice()` builds a golib [slice](#slices-and-arrays).

<!-- Reference gap: labels-and-loop-variables.md has no rule for the if/switch init clause block; add it there, with its guard tests, and extend the Full detail line. -->
<!-- A switch init clause gets the same block: src/tests/Behavioral/ExprSwitch/ExprSwitch.go:124 -> ExprSwitch.cs.target:179-181. -->
**An `if` or `switch` init clause opens a block.** C# `if` and `switch` have no init clause. So the
declaration and the statement share a new pair of braces, which keeps the variable scoped to the
statement as in Go. That is why the second `a` here can reuse the name:

<!-- source: src/tests/Behavioral/IfStatements/IfStatements.go:6 -->
```go
if a := -1; a < 0 {
	fmt.Println("a is less than 0")
}

if a := 1; a > 0 {
	fmt.Println("a is greater than 0")
}
```
<!-- source: src/tests/Behavioral/IfStatements/IfStatements.cs.target:8 -->
```csharp
private static readonly object aIsLessThan0ˢ = (@string)"a is less than 0"u8;
private static readonly object aIsGreaterThan0ˢ = (@string)"a is greater than 0"u8;
…
    {
        nint a = -1; if (a < 0) {
            fmt.Println(aIsLessThan0ˢ);
        }
    }
    {
        nint a = 1; if (a > 0) {
            fmt.Println(aIsGreaterThan0ˢ);
        }
    }
```

The two `ˢ` fields are the two string literals, each stored once in a static field and typed `object`
for `fmt.Println` ([Strings](#strings-string-and-sstring)). How the `switch` itself converts is in
[Expression Switch Statements](#expression-switch-statements).

**Full detail:** [Reference → Labeled Control Flow and Loop Variables](ConversionStrategies-Reference/labels-and-loop-variables.md#labeled-control-flow-and-loop-variables) — the copy-back rules and heap-boxed loop variables, when a range variable is copied, the `=` range form, range over every integer type, blank range variables, allocation-free enumerators, and labels on empty statements.

---

## Expression Switch Statements
<!-- Length: about 200 lines, above the usual range on purpose. The owner's review asks each section to stand on its own with complete examples, and this section has five lowered forms (C# switch, if chain, tagless switch with its ᐧᐧ case, init-statement block, fallthrough chain), each shown whole with its hoisted-literal declarations. Secondary rules (break wrapping, default placement, relational patterns, named-numeric and uintptr comparands) live in the reference. -->

Go's expression `switch` runs exactly one case. It tries the cases from top to bottom, and the labels of
one case from left to right, and stops at the first match. It never falls into the next case unless that
case ends in `fallthrough`. C# has a `switch` statement too, but a label that compares values must be a C#
compile-time constant. So the converter emits a real C# `switch` where the labels allow one, and `if`
statements where they do not.

A few names recur in the examples:

- A name ending in `ˢ`, such as `oneˢ`, is a string literal the converter stores once in a `static readonly`
  field ([Strings](#strings-string-and-sstring)). In these examples the fields are typed `object`, already
  boxed, because every use is an argument to `fmt.Print` or `fmt.Println`, which take `any`
  ([Empty Interface](#empty-interface-any)). A literal that is also used as a plain string is typed `@string`
  instead. A literal with fewer than three letters or digits, such as `" as "`, stays inline.
- `@string` is Go's `string`, a UTF-8 byte string, and `"…"u8` is a C# UTF-8 literal. `@string` comes from
  golib, the go2cs runtime library ([golib](#the-golib-runtime-library)).
- A name ending in `ᴛ` and a number, such as `exprᴛ1`, is a temporary the converter makes up. The number only
  keeps names unique within a function; it carries no meaning.
- `nint` is Go's `int` ([Integer Types](#integer-types-and-arithmetic)).

**Constant labels become a C# `switch`.** This needs every label to be a number or rune literal, or a
constant declared with an explicit numeric or boolean type (`const n int = 5`), and no `fallthrough`.
An untyped constant such as `const n = 5` is not a C# `const`: it becomes a value of a golib wrapper type
such as `UntypedInt` (a property at package level, a local inside a function)
([Constant Values](#constant-values)). So a switch with such a label takes the `if` chain described next.

A tag of a named type, such as `type Weekday int`, also rules out a C# `switch`. go2cs emits a named type
as a C# struct, and a plain number is not a constant of that struct
([Named Numeric Types](#named-numeric-types-and-constant-contexts)).

Each case body is wrapped in `{ }` and ends in `break;` (or a `return`), since C# requires it. A list of
labels becomes an `or` pattern: `case 4, 5, 6:` becomes `case 4 or 5 or 6:`. A block Go writes inside a
case stays a block. The final `}}` closes the last case block and the switch together:

<!-- source: src/tests/Behavioral/ExprSwitch/ExprSwitch.go:42-57 -->
```go
i := 2
fmt.Print("Write ", i, " as ")
switch i {
case 1:
	fmt.Println("one")
case 2:
	fmt.Println("two")
case 3:
	{
		fmt.Println("three")
	}
case 4, 5, 6:
	fmt.Println("four, five or siz")
default:
	fmt.Println("unknown")
}
```
<!-- source: src/tests/Behavioral/ExprSwitch/ExprSwitch.cs.target:40-108 -->
```csharp
private static readonly object writeˢ = (@string)"Write "u8;
private static readonly object oneˢ = (@string)"one"u8;
private static readonly object twoˢ = (@string)"two"u8;
private static readonly object threeˢ = (@string)"three"u8;
private static readonly object fourFiveOrSizˢ = (@string)"four, five or siz"u8;
private static readonly object unknownˢ = (@string)"unknown"u8;
…
    nint i = 2;
    fmt.Print(writeˢ, i, (@string)" as "u8);
    switch (i) {
    case 1: {
        fmt.Println(oneˢ);
        break;
    }
    case 2: {
        fmt.Println(twoˢ);
        break;
    }
    case 3: {
        {
            fmt.Println(threeˢ);
        }
        break;
    }
    case 4 or 5 or 6: {
        fmt.Println(fourFiveOrSizˢ);
        break;
    }
    default: {
        fmt.Println(unknownˢ);
        break;
    }}
```

**Other labels become an `if / else if` chain.** Variables, function calls and Go strings take the chain.
A Go string becomes golib's `@string`, a struct, so it cannot be a C# `const`. Untyped constants take it
too, including Go's `true` and `false`. So do constants of a named type, such as `time.Saturday`.

The tag is evaluated once into a temporary, `exprᴛ1`. Each case compares against it with `==`, or with an
`is` pattern when the label is a C# constant (`exprᴛ1 is 0`). A label list joins with `||`, and `default`
becomes the final `else`:

<!-- source: src/tests/Behavioral/ExprSwitch/ExprSwitch.go:72-79 -->
```go
switch time.Now().Weekday() {
case time.Saturday, time.Sunday:
	fmt.Println("It's the weekend")
case time.Monday: // Case Mon comment
	fmt.Println("Ugh, it's Monday")
default:
	fmt.Println("It's a weekday")
}
```
<!-- source: src/tests/Behavioral/ExprSwitch/ExprSwitch.cs.target:46-126 -->
```csharp
private static readonly object itSTheWeekendˢ = (@string)"It's the weekend"u8;
private static readonly object ughItSMondayˢ = (@string)"Ugh, it's Monday"u8;
private static readonly object itSAWeekdayˢ = (@string)"It's a weekday"u8;
…
    var exprᴛ1 = time.Now().Weekday();
    if (exprᴛ1 == time.Saturday || exprᴛ1 == time.Sunday) {
        fmt.Println(itSTheWeekendˢ);
    }
    else if (exprᴛ1 == time.Monday) {
        fmt.Println(ughItSMondayˢ);
    }
    else { /* default: */
        fmt.Println(itSAWeekdayˢ);
    }
```

The Go comment is absent because this test is converted without the `-comments` option ([Comments](#comments)).

**A switch with no tag becomes `switch (ᐧ)`.** In Go, `switch { … }` means `switch true { … }`. `ᐧ` (a small
raised dot) is golib's `const bool` equal to `true`, used where only the `when` conditions matter. Each case
becomes `case {} when <condition>:`. The `{}` pattern matches any value, so the `when` condition alone decides.

A comparison against a number literal (or a typed constant) may become a C# pattern with the same meaning,
such as `t.Hour() is < 12`:

<!-- source: src/tests/Behavioral/ExprSwitch/ExprSwitch.go:84-90 -->
```go
t := time.Now()
switch {
case t.Hour() < 12: // Before noon
	fmt.Println("It's before noon")
default: // After noon
	fmt.Println("It's after noon")
}
```
<!-- source: src/tests/Behavioral/ExprSwitch/ExprSwitch.cs.target:49-137 -->
```csharp
private static readonly object itSBeforeNoonˢ = (@string)"It's before noon"u8;
private static readonly object itSAfterNoonˢ = (@string)"It's after noon"u8;
…
    var t = time.Now();
    switch (ᐧ) {
    case {} when t.Hour() is < 12: {
        fmt.Println(itSBeforeNoonˢ);
        break;
    }
    default: {
        fmt.Println(itSAfterNoonˢ);
        break;
    }}
```

**A constant `true` case with cases after it becomes `case {} when ᐧᐧ:`.** Go accepts `case true:` first;
the later cases can never run, but Go still compiles them. C# would reject those later cases as unreachable.
So the converter writes `ᐧᐧ`, golib's `static readonly bool` equal to `true`, which C# cannot evaluate early:

<!-- source: src/tests/Behavioral/ExprSwitch/ExprSwitch.go:200-206 -->
```go
v := 3
switch {
case true:
	fmt.Println("strict checks disabled")
case v > 2:
	fmt.Println("unreachable but compiled")
}
```
<!-- source: src/tests/Behavioral/ExprSwitch/ExprSwitch.cs.target:74-290 -->
```csharp
private static readonly object strictChecksDisabledˢ = (@string)"strict checks disabled"u8;
private static readonly object unreachableButCompiledˢ = (@string)"unreachable but compiled"u8;
…
    nint v = 3;
    switch (ᐧ) {
    case {} when ᐧᐧ: {
        fmt.Println(strictChecksDisabledˢ);
        break;
    }
    case {} when v is > 2: {
        fmt.Println(unreachableButCompiledˢ);
        break;
    }}
```

**A switch with an init statement gets its own block.** Go's `switch x := f(); …` scopes `x` to the switch.
The C# opens a `{ }` block, declares the variable first, then emits the switch. When the name shadows an
outer variable, it gets a `Δ1` suffix ([Shadowing](#short-variable-redeclaration-shadowing)). After the
block, the outer `hour` still holds `1`, as in Go.

Several comparisons of one variable with number literals join into one pattern: `hour == 1, hour < 12,
hour == 2` becomes `hourΔ1 is 1 or < 12 or 2`. A pattern needs every label in the case to be one comparison
of the same left side with a constant. A label joined with `||` or `&&`, such as `hour == 2 || hour1 == 4`,
keeps plain `==`, `||` and `&&` operators:

<!-- source: src/tests/Behavioral/ExprSwitch/ExprSwitch.go:93-111 -->
```go
hour := 1
hour1 := time.Now().Hour()

switch hour := time.Now().Hour(); { // missing expression means "true"
case hour == 1, hour < 12, hour == 2:
	fmt.Println("Good morning!")
case hour == 1, hour < 12, hour == 2 || hour1 == 4:
	fmt.Println("Good morning (opt 2)!")
case hour < 17:
	fmt.Println("Good afternoon!")
case hour == 0:
	fmt.Println("Midnight!")
case hour == 0 && hour1 == 1:
	fmt.Println("Midnight (opt 2)!")
default:
	fmt.Println("Good evening!")
}

fmt.Println(hour)
```
<!-- source: src/tests/Behavioral/ExprSwitch/ExprSwitch.cs.target:51-170 -->
```csharp
private static readonly object goodMorningˢ = (@string)"Good morning!"u8;
private static readonly object goodMorningOpt2ˢ = (@string)"Good morning (opt 2)!"u8;
private static readonly object goodAfternoonˢ = (@string)"Good afternoon!"u8;
private static readonly object midnightˢ = (@string)"Midnight!"u8;
private static readonly object midnightOpt2ˢ = (@string)"Midnight (opt 2)!"u8;
private static readonly object goodEveningˢ = (@string)"Good evening!"u8;
…
    nint hour = 1;
    nint hour1 = time.Now().Hour();
    {
        nint hourΔ1 = time.Now().Hour();
        switch (ᐧ) {
        case {} when hourΔ1 is 1 or < 12 or 2: {
            fmt.Println(goodMorningˢ);
            break;
        }
        case {} when (hourΔ1 == 1) || (hourΔ1 < 12) || (hourΔ1 == 2 || hour1 == 4): {
            fmt.Println(goodMorningOpt2ˢ);
            break;
        }
        case {} when hourΔ1 is < 17: {
            fmt.Println(goodAfternoonˢ);
            break;
        }
        case {} when hourΔ1 is 0: {
            fmt.Println(midnightˢ);
            break;
        }
        case {} when hourΔ1 == 0 && hour1 == 1: {
            fmt.Println(midnightOpt2ˢ);
            break;
        }
        default: {
            fmt.Println(goodEveningˢ);
            break;
        }}
    }

    fmt.Println(hour);
```

**A switch that uses `fallthrough` becomes a chain of separate `if` statements.** This applies whether the
switch has a tag or not, because C# never lets a case run on into the next one, and its `goto case` needs a
constant label, which a `case {} when …` arm does not have. A case that can be fallen into starts a new `if`;
the other cases join the chain with `else if`. Two pieces of state drive the chain:

- `matchᴛ5` is a local that records that some case has already matched. A matching case sets it with
  `matchᴛ5 = true;`, written on the same line as its `if`. The last case before a final `default` skips it
  when nothing reads the flag afterwards, as `Foo(4)` does below.
- `fallthrough` is a golib flag, kept per thread. A case that ends in `fallthrough` sets it to `true`.
  Reading it returns its value and clears it.

A case that can be fallen into runs when the flag is set, or when nothing has matched yet and its own label
matches: `fallthrough || (!matchᴛ5 && <label>)`. C#'s `&&` binds tighter than `||`, so a single-label case
like `Foo(4)` needs no parentheses.

`default` runs when nothing matched, or when the case before it ends in `fallthrough`. In that second shape
it is guarded `if (fallthrough || !matchᴛ5)`. Here the case before `default` does not fall through, so it is
`else if (!matchᴛ5)`:

<!-- source: src/tests/Behavioral/ExprSwitch/ExprSwitch.go:30-33 -->
```go
func Foo(n int) int {
	fmt.Println(n)
	return n
}
```
<!-- source: src/tests/Behavioral/ExprSwitch/ExprSwitch.go:188-196 -->
```go
switch Foo(2) {
case Foo(1), Foo(2), Foo(3):
	fmt.Println("First case")
	fallthrough
case Foo(4):
	fmt.Println("Second case")
default:
	fmt.Println("Default case")
}
```
<!-- source: src/tests/Behavioral/ExprSwitch/ExprSwitch.cs.target:71-279 -->
```csharp
private static readonly object firstCaseˢ = (@string)"First case"u8;
private static readonly object secondCaseˢ = (@string)"Second case"u8;
private static readonly object defaultCaseˢ = (@string)"Default case"u8;
…
    var exprᴛ6 = Foo(2);
    var matchᴛ5 = false;
    if (exprᴛ6 == Foo(1) || exprᴛ6 == Foo(2) || exprᴛ6 == Foo(3)) { matchᴛ5 = true;
        fmt.Println(firstCaseˢ);
        fallthrough = true;
    }
    if (fallthrough || !matchᴛ5 && exprᴛ6 == Foo(4)) {
        fmt.Println(secondCaseˢ);
    }
    else if (!matchᴛ5) { /* default: */
        fmt.Println(defaultCaseˢ);
    }
```

Both programs print `2`, `1`, `2`, `First case`, `Second case`. C#'s `||` stops at the first true operand,
so `Foo(3)` and `Foo(4)` are never called, just as Go stops trying labels once one matches or a case is
entered by `fallthrough`.

**Full detail:** [Reference → Expression Switch Statements](ConversionStrategies-Reference/expression-switch.md#expression-switch-statements) —
how a `break` inside a case leaves it, where `default` may sit when cases fall through, when a condition becomes a C# pattern, and how named-type and `uintptr` tags and labels compare.

---

## Type Switch Statements

A Go type switch, `switch t := i.(type)`, becomes a C# pattern `switch` over `i.type()`. Each Go `case`
type becomes a C# type pattern, such as `case bool t:`, so C# picks the arm by the value's dynamic type.

**Why `i.type()` and not just `i`.** `type()` is a [golib](#the-golib-runtime-library) method that returns
the Go value the interface really holds. Sometimes the value travels inside a small generated wrapper, an
adapter, for example when a pointer is stored in a non-empty Go interface ([Interfaces](#interfaces)).
`type()` removes that wrapper, and turns a .NET `string` into Go's string type, so the C# patterns see the
Go value.

**Each clause is its own braced arm.** An arm ends in `break` or its own `return`, because Go clauses
never fall through. The last arm's closing brace and the switch's closing brace share one line, `}}`.
That is only layout; it has no meaning. [Expression Switch Statements](#expression-switch-statements)
uses the same arm shape for value switches.

**A complete example.** `whatAmI` is a Go function literal, so it becomes a C# local function inside
`Main` ([Function Values and Closures](#function-values-and-closures)). Go's `interface{}` becomes C#
`any`, an alias for `object` ([Empty Interface](#empty-interface-any)):

<!-- source: src/tests/Behavioral/TypeSwitch/TypeSwitch.go:12-30 (shown without the one leading tab of its enclosing main) -->
```go
whatAmI := func(i interface{}) {
	switch t := i.(type) {
	case nil:
		// A nil interface matches `case nil` — emitted as the C# `case null:` pattern.
		fmt.Println("I'm nil")
	case bool:
		fmt.Println("I'm a bool")
	case int, int64, uint64:
		fmt.Printf("I'm an int, specifically type %T\n", t)
	default:
		fmt.Printf("Don't know type %T\n", t)
	}
}
whatAmI(true)
whatAmI(1)
whatAmI(int64(2))
whatAmI(uint64(2))
whatAmI("hey")
whatAmI(nil)
```
<!-- source: src/tests/Behavioral/TypeSwitch/TypeSwitch.cs.target:8-10, 21-50 (the function body is shown without the four spaces of its enclosing Main) -->
```csharp
private static readonly object iMNilˢ = (@string)"I'm nil"u8;
private static readonly object iMABoolˢ = (@string)"I'm a bool"u8;
private static readonly object heyˢ = (@string)"hey"u8;
…
void whatAmI(any i) {
    switch (i.type()) {
    case null: {
        fmt.Println(iMNilˢ);
        break;
    }
    case bool t: {
        fmt.Println(iMABoolˢ);
        break;
    }
    case nint _:
    case int32 _:
    case int64 _:
    case uint64 _: {
        var t = i;
        fmt.Printf("I'm an int, specifically type %T\n"u8, t);
        break;
    }
    default: {
        var t = i;
        fmt.Printf("Don't know type %T\n"u8, t);
        break;
    }}
}
whatAmI(true);
whatAmI((nint)(1));
whatAmI((int64)2);
whatAmI((uint64)2);
whatAmI(heyˢ);
whatAmI(default!);
```

Both versions print the same six lines: `I'm a bool`; `I'm an int, specifically type int`, then the same
line ending in `int64` and in `uint64`; `Don't know type string`; and `I'm nil`. golib's `%T` prints Go
type names, so a C# `nint` prints as `int`. A few names in the C# need a word of explanation:

- `iMNilˢ`, `iMABoolˢ` and `heyˢ` are string literals created once, in static fields. The `ˢ` suffix marks
  such a generated name ([Strings](#strings-string-and-sstring)). Each holds a golib `@string`, Go's byte
  string, already boxed as an `object` because it is only ever passed as an `any`.
- `"…"u8` is a C# UTF-8 literal. A literal that becomes a string value is moved into a `ˢ` field. A
  format string, like the one passed to `Printf`, stays inline: a field name made from it would not read
  well, and the formatting itself costs far more than the literal.
- `(nint)(1)` boxes the untyped constant `1` at Go's default type `int`, which is C#
  [`nint`](#integer-types-and-arithmetic). A bare C# `1` would box as a 32-bit `int` instead
  ([Empty Interface](#empty-interface-any)).
- `default!` is Go's `nil`. It is C#'s default value, here `null`; the `!` only silences a nullable
  warning ([Nil and Zero Values](#nil-and-zero-values)).

**`case nil:` becomes `case null:`.** A nil interface is a C# `null`. No C# type pattern matches `null`,
so only this arm catches it.

**A single-type case binds its variable at that type.** `case bool:` becomes `case bool t:`. The arm
declares `t` as Go does, even when its body does not use it. In `sizeOf`, `t` is an `int32` in one arm
and an `int64` in the other, as in Go. Each arm is its own braced scope, so both can declare their own
`sz`:

<!-- source: src/tests/Behavioral/TypeSwitch/TypeSwitch.go:142-152 -->
```go
func sizeOf(v any) int {
	switch t := v.(type) {
	case int32:
		sz := int(t) + 1
		return sz
	case int64:
		sz := int(t) + 2
		return sz
	}
	return 0
}
```
<!-- source: src/tests/Behavioral/TypeSwitch/TypeSwitch.cs.target:171-182 -->
```csharp
internal static nint sizeOf(any v) {
    switch (v.type()) {
    case int32 t: {
        nint sz = (nint)t + 1;
        return sz;
    }
    case int64 t: {
        nint sz = (nint)t + 2;
        return sz;
    }}
    return 0;
}
```

Go's conversion `int(t)` becomes the C# cast `(nint)t` ([Integer Types](#integer-types-and-arithmetic)).

**`default` and multi-type cases keep the interface value.** In Go, the case variable of such a clause
has the switch operand's type. The C# arm therefore opens with `var t = i;`, so `t` is still an `any`.
A multi-type case stacks one label per type over a shared body. Each label binds only a discard, `_`,
because no single type fits all of them.

**A pointer case matches the pointer's box.** Go's `*T` becomes golib's `ж<T>`, a heap box read "zhe"
([Pointers](#pointers)). `case *bool:` becomes `case ж<bool> t:`, and Go's `*t = true` writes through the
box with `t.Value`:

<!-- source: src/tests/Behavioral/TypeSwitch/TypeSwitch.go:130-137 -->
```go
func scanInto(v any) {
	switch t := v.(type) {
	case *bool:
		*t = true
	case *int:
		*t = 42
	}
}
```
<!-- source: src/tests/Behavioral/TypeSwitch/TypeSwitch.cs.target:159-169 -->
```csharp
internal static void scanInto(any v) {
    switch (v.type()) {
    case ж<bool> t: {
        t.Value = true;
        break;
    }
    case ж<nint> t: {
        t.Value = 42;
        break;
    }}
}
```

**`case int:` also catches a C# `int`.** Go's `int` becomes the native-sized `nint`
([Integer Types](#integer-types-and-arithmetic)). A value can still reach an interface as a plain C#
`int`, for example from hand-written C# code, and its dynamic type is then `int32`. So the converter adds
an `int32` case that runs Go's `int` clause. `case uint:` adds `uint32` the same way.

How the extra case appears depends on the clause. In a multi-type case, such as `whatAmI`'s, it is one
more stacked `_` label. A single-type `case int` binds `v` as an `nint`, so an `int32` label cannot share
its body; the arm is repeated as `case int32 v:`, as the next example shows. When the switch already
lists `int32` (or `uint32`) as its own case, nothing is added, and each value goes to its own clause.

**An interface case matches by method set.** A C# type pattern only sees the interfaces a class
declares. Go satisfies an interface by having its methods, so a case such as `case error:` asks golib
instead ([Interfaces](#interfaces)). This switch inspects the value that `recover()` returns inside a
deferred function ([Defer / Panic / Recover](#defer--panic--recover)):

<!-- source: src/tests/Behavioral/PanicRecover/PanicRecover.go:45-57 (the switch is shown without the three tabs of its enclosing function literals) -->
```go
p := recover()
switch v := p.(type) {
case nil:
	out = "no panic"
case string:
	out = fmt.Sprintf("string(%s) eq-x=%v", v, v == "x")
case error:
	out = "error(" + v.Error() + ")"
case int:
	out = fmt.Sprintf("int(%d)", v)
default:
	out = "other (not a plain string)"
}
```
<!-- source: src/tests/Behavioral/PanicRecover/PanicRecover.cs.target:60-61, 69-95 (the switch is shown without the sixteen spaces of its enclosing lambdas) -->
```csharp
private static readonly @string noPanicˢ = "no panic"u8;
private static readonly @string otherNotAPlainStringˢ = "other (not a plain string)"u8;
…
var p = recover();
switch (p.type()) {
case null: {
    @out = noPanicˢ;
    break;
}
case @string v: {
    @out = fmt.Sprintf("string(%s) eq-x=%v"u8, v, v == "x"u8);
    break;
}
case {} Δv when Δv._<error>(out var v): {
    @out = "error("u8 + v.Error() + ")"u8;
    break;
}
case nint v: {
    @out = fmt.Sprintf("int(%d)"u8, v);
    break;
}
case int32 v: {
    @out = fmt.Sprintf("int(%d)"u8, v);
    break;
}
default: {
    var v = p;
    @out = otherNotAPlainStringˢ;
    break;
}}
```

`case error:` becomes `case {} Δv when Δv._<error>(out var v):`. The `{}` pattern matches any non-null
value and captures it as `Δv`. `Δv` is a made-up name: the `Δ` prefix keeps it from clashing with the `v`
that the same label binds ([Names and Glyphs](#reading-converted-code-names-and-glyphs)). The `when`
guard calls `_<T>`, golib's type assertion, in a form that returns `false` instead of panicking. On
success, `v` holds the value as an `error`.

The other names in this example:

- `@out` is Go's `out`, escaped with `@` because `out` is a C# keyword.
- `noPanicˢ` and `otherNotAPlainStringˢ` are string literals created once, in static fields. They are
  typed `@string` here because they are assigned to a string variable ([Strings](#strings-string-and-sstring)).
- `case string:` becomes `case @string v:`, since Go's `string` is golib's `@string`. An `@string`
  compares and concatenates directly with `"…"u8` literals, so `v == "x"u8` and `"error("u8 + …` read
  like the Go.
- `case int:` gets its repeated `case int32 v:` arm, as described earlier in this section.

**Full detail:** [Reference → Type Switch Statements](ConversionStrategies-Reference/type-switch.md#type-switch-statements) — switches with no case variable, how cases that share one C# type merge, anonymous interface labels, and how interface cases find methods declared on a pointer.

---

## Defer / Panic / Recover

Go's `defer`, `panic` and `recover` become a C# `try`/`catch`/`finally` plus three golib calls. The
function body stays inline, so it still reads like the Go. You will notice a local named `ᒐ`,
`defer(…, ref ᒐ)` calls, and `throw panic(x)`.

`defer`, `panic` and `recover` are static methods of golib's [`builtin`](../src/core/golib/builtin.cs)
class. Every converted file imports that class with `using static`, so the calls need no class name, as
in Go ([Built-in Functions](#built-in-functions)).

<!-- Condensed to the forms a reader meets: the inline body with its frame, eager defer arguments, named
results with an early return, unnamed results, a panic crossing a call, and which runtime errors recover()
sees. Longer than the usual summary section by owner rule (2026-09-27 review): each form is a complete small
example rather than one fragment full of ellipses, and each section stands on its own. In the reference: the
receiver-field call lowered into the finally (golden DeferFinallyLowering/main.cs.target:61; never in a
function that calls recover() or has named results), runtime.Goexit, the crash report, and
(manual-conversions.md, runtime.Stack sections) the Go-shaped tracebacks, the main-goroutine Goexit gate and
the NoInlining pin on runtime.Caller callers. src/core/golib/GoFrame.cs: m_d0..m_d3 are the inline slots; a
fifth registration allocates the List<Action> overflow. PanicException: src/core/golib/PanicException.cs:20.
Run() re-throws a panic no deferred call recovered: GoFrame.cs:110-130. Capture parks it for recover():
GoFrame.cs:318-340; builtin.recover reads and clears that slot: builtin.cs:278. defer/panic/recover live on
the partial class builtin (builtin.DeferRegistrations.cs:48, builtin.cs:251, :278); the project-wide
`using static go.builtin` is described at builtin.cs:33. A non-panic exception fails IsPanic and propagates
unchanged while the finally still runs the defers: GoFrame.cs:283-313. -->
**A function that defers wraps its body in `try`/`catch`/`finally`.** This is the smallest `defer`:

<!-- source: src/tests/Behavioral/DeferSimple/DeferSimple.go:5 -->
```go
func main() {
	fmt.Println("Open file")
	defer fmt.Println("Close file")
	fmt.Println("Write data to file")
}
```
<!-- source: src/tests/Behavioral/DeferSimple/DeferSimple.cs.target:7 -->
```csharp
// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object openFileˢ = (@string)"Open file"u8;
private static readonly object closeFileˢ = (@string)"Close file"u8;
private static readonly object writeDataToFileˢ = (@string)"Write data to file"u8;

internal static void Main() {
    GoFrame ᒐ = default;
    try {
        fmt.Println(openFileˢ);
        defer(ᴛ1 => fmt.Println(ᴛ1), closeFileˢ, ref ᒐ);
        fmt.Println(writeDataToFileˢ);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}
```

Each part has one job:

- `GoFrame ᒐ = default;` declares this call's list of deferred calls. golib's
  [`GoFrame`](../src/core/golib/GoFrame.cs) is a small `ref struct` that lives on the stack. The glyph
  `ᒐ` marks this frame and every name that belongs to it, such as `ᒐex`, `ᒐp` and the label `ᒐdone`
  ([Reading Converted Code](#reading-converted-code-names-and-glyphs)).
- `defer(…, ref ᒐ)` adds one call to that list. `ᒐ` is passed by `ref`, so the call lands in this
  frame and not in a copy.
- The `try` block is the Go body, statement for statement.
- The `catch` filter, `GoFrame.IsPanic`, accepts only Go panics. A Go panic is a golib `PanicException`;
  the filter also turns a few .NET exceptions, such as `DivideByZeroException`, into Go runtime errors.
  `GoFrame.Capture` then parks the panic where `recover()` can find it.
- The `finally` calls `ᒐ.Run()`. It runs the deferred calls, last added first, on every exit: the end
  of the body, a `return`, or a panic.

Any other .NET exception fails the filter and is not caught. The `finally` still runs the deferred calls,
but `recover()` does not see that exception and returns `nil`.

So the program prints "Open file", "Write data to file", then "Close file", as in Go.

The `ˢ` names are the Go string literals, each created once as a `static readonly` field
([Strings](#strings-string-and-sstring)). `@string` is golib's type for Go's `string`, and the `u8` suffix
makes a C# literal UTF-8 bytes, as Go stores strings. The `// Hoisted …` comment is the converter's;
RODATA is Go's read-only memory for literals.

These fields are typed `object` because they are only passed to `fmt.Println`, whose parameters are Go's
`any` ([Empty Interface](#empty-interface-any)). `ᴛ1` is a lambda parameter the converter names.

**`defer` evaluates its arguments at once**, as Go does; only the call waits. The arguments are passed to
`defer` beside the lambda, and the lambda receives them later as `ᴛ1`, `ᴛ2`:

<!-- source: src/tests/Behavioral/DeferEvalParam/DeferEvalParam.go:9 -->
```go
func printSquare(n int) {
	defer fmt.Println("Deferred square:", n*n)
	n++
	fmt.Println("Immediate n:", n)
}
```
<!-- source: src/tests/Behavioral/DeferEvalParam/DeferEvalParam.cs.target:11 -->
```csharp
// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object deferredSquareˢ = (@string)"Deferred square:"u8;
private static readonly object immediateNˢ = (@string)"Immediate n:"u8;

internal static void printSquare(nint n) {
    GoFrame ᒐ = default;
    try {
        defer((ᴛ1, ᴛ2) => fmt.Println(ᴛ1, ᴛ2), deferredSquareˢ, n * n, ref ᒐ);
        n++;
        fmt.Println(immediateNˢ, n);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}
```

`printSquare(5)` computes `n * n` while `n` is still 5. It prints "Immediate n: 6", then "Deferred
square: 25", as in Go. `nint` is Go's `int` ([Integer Types](#integer-types-and-arithmetic)).

**`panic(x)` becomes `throw panic(x)`, and `recover()` is a plain call.** golib's `panic` builds a
`PanicException` that carries `x`, and `throw` raises it. A deferred closure calls `recover()` directly:
it reads the panic that the `catch` parked, and stops it. This function panics and recovers in one place:

<!-- source: src/tests/Behavioral/NamedReturnDefer/main.go:66 -->
```go
// recover sets the named returns on panic; normal path leaves them.
func guarded(boom bool) (code int, msg string) {
	defer func() {
		if r := recover(); r != nil {
			code = -1
			msg = "recovered"
		}
	}()
	if boom {
		panic("kaboom")
	}
	code = 0
	msg = "ok"
	return code, msg
}
```
<!-- source: src/tests/Behavioral/NamedReturnDefer/main.cs.target:95 -->
```csharp
// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string recoveredˢ = "recovered"u8;

internal static (nint code, @string msg) guarded(bool boom) {
    nint code = default!;
    @string msg = default!;
    GoFrame ᒐ = default;
    try {
        defer(() => {
            {
                var r = recover(); if (r != default!) {
                    code = -1;
                    msg = recoveredˢ;
                }
            }
        }, ref ᒐ);
        if (boom) {
            throw panic("kaboom");
        }
        code = 0;
        msg = "ok"u8;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return (code, msg);
}
```

With `boom` true, the `throw` jumps to the `catch`, which parks the panic. The `finally` runs the deferred
closure, whose `recover()` returns "kaboom". The panic stops there, and `guarded` returns `-1, "recovered"`.
With `boom` false, `recover()` returns `nil`, and `guarded` returns `0, "ok"`.

`recover()` returns Go's `any`, a C# `object?` that is `null` when no panic is in flight. Converted code
spells Go's `nil` and zero values as `default!`, so `r != default!` is Go's `r != nil`
([Nil and Zero Values](#nil-and-zero-values)). The extra braces keep `r` scoped to the `if`, as Go's
`if r := …; r != nil` does.

<!-- Hoisting table: docs/ConversionStrategies-Reference/strings.md:120-138 (a slug of 3 characters or
fewer stays inline; builtin arguments such as panic's take their own path). The C# string to Go string
normalization: src/core/golib/builtin.cs:251. -->
`"recovered"` becomes the field `recoveredˢ`, but `"ok"` stays inline as `"ok"u8`: a field name would say
no more than a value that short. The literal passed to `panic` stays a plain C# string. golib's `panic`
turns it into a Go `string`, so `recover()` sees Go's type.

**Named results are declared before the `try` and returned after the `finally`.** Go runs deferred calls
after `return` sets the results, so a deferred call can still change them, as `guarded` does. A C# `finally`
cannot change a value already returned. So `code` and `msg` are plain locals, and the one real `return`
follows the `finally`.

The deferred lambda captures `code` and `msg` as ordinary C# closure variables, so its changes land in the
values returned. The C# tuple `(nint code, @string msg)` carries Go's two results
([Functions and Methods](#functions-and-methods)). Each Go `return` in the body only sets the results: in
`guarded`, `return code, msg` needs no C# statement, because the results already hold those values.

**An early `return` sets the results and jumps to the label `ᒐdone`** on the final `return`. Leaving the
`try` this way still runs the `finally`, so the deferred calls still see and change the results:

<!-- source: src/tests/Behavioral/NamedReturnDefer/main.go:57 -->
```go
func compute(x int) (out int, label string) {
	defer func() { out += 1000 }() // proves the named result is returned post-defer
	if x < 0 {
		out, label = -1, "neg"
		return out, label // early return of the named results (mid-body): keeps `return;`
	}
	return double(x), fmt.Sprintf("v=%d", x)
}
```
<!-- source: src/tests/Behavioral/NamedReturnDefer/main.cs.target:73 -->
```csharp
// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string negˢ = "neg"u8;

internal static (nint @out, @string label) compute(nint x) {
    nint @out = default!;
    @string label = default!;
    GoFrame ᒐ = default;
    try {
        defer(() => {
            @out += 1000;
        }, ref ᒐ);
        if (x < 0) {
            (@out, label) = (-1, negˢ);
            goto ᒐdone;
        }
        (@out, label) = (@double(x), fmt.Sprintf("v=%d"u8, x));
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return (@out, label);
}
```

`compute(-5)` sets `-1, "neg"` and jumps to `ᒐdone`. The `finally` adds 1000 on the way out, so it returns
`999, "neg"`. `compute(3)` returns `1006, "v=3"`. The last Go `return` needs no jump, because the `try`
simply ends.

`@out` and `@double` carry an `@` because `out` and `double` are C# keywords
([Reading Converted Code](#reading-converted-code-names-and-glyphs)). `double` is a small function in the
same test that returns `n * 2`. `(@out, label) = (-1, negˢ)` is Go's `out, label = -1, "neg"`
([Multi-Assignment](#multi-assignment-and-evaluation-order)).

**A function whose results are unnamed returns zero values after a recovered panic.** Its `catch` ends with
`return default!;`, which is Go's rule. A function with no results needs no `return` there. Here `pick`
recovers any panic its body raises:

<!-- source: src/tests/Behavioral/DeferInterfaceReturn/main.go:24 -->
```go
func pick(kind int) Shape {
	defer func() { _ = recover() }()
	if kind == 0 {
		return Circle{R: 2}
	}
	return Square{S: 5}
}
```
<!-- source: src/tests/Behavioral/DeferInterfaceReturn/main.cs.target:27 -->
```csharp
internal static Shape pick(nint kind) {
    GoFrame ᒐ = default;
    try {
        defer(() => {
            _ = recover();
        }, ref ᒐ);
        if (kind == 0) {
            return new Circle(R: 2);
        }
        return new Square(S: 5);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { ᒐ.Run(); }
}
```

`Shape` is an interface, and `Circle` and `Square` are structs that implement it
([Interfaces](#interfaces)). Unnamed results need no locals, so each Go `return` stays a C# `return`
inside the `try`.

If the body panicked, the `catch` would set the result to `nil`, the zero `Shape`. The `finally` would
then run the deferred `recover()`, and `pick` would return `nil`. If no deferred call recovers,
`ᒐ.Run()` throws the panic again, and that `return default!` never completes.

**A panic travels up through callers until a deferred `recover()` stops it.** When no deferred call in a
frame recovers, `ᒐ.Run()` throws the panic again after the deferred calls finish. The caller's frame then
catches it the same way. Here `divide` has no `defer`, so it has no frame, and `outerGuard` recovers. (`quo`
is golib's signed division, see [Integer Types and Arithmetic](#integer-types-and-arithmetic).)

<!-- source: src/tests/Behavioral/DivideByZeroPanic/main.go:36 -->
```go
func divide(a, b int) int { return a / b }

// the panic propagates across an intermediate (non-defer) frame to an outer recover.
func outerGuard(a, b int) (ok bool) {
	defer func() {
		if recover() != nil {
			ok = false
		}
	}()
	divide(a, b) // panic in divide() unwinds through this frame to the recover above
	return true
}
```
<!-- source: src/tests/Behavioral/DivideByZeroPanic/main.cs.target:51 -->
```csharp
internal static nint divide(nint a, nint b) {
    return quo(a, b);
}

internal static bool /*ok*/ outerGuard(nint a, nint b) {
    bool ok = default!;
    GoFrame ᒐ = default;
    try {
        defer(() => {
            if (recover() != default!) {
                ok = false;
            }
        }, ref ᒐ);
        divide(a, b);
        ok = true;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return ok;
}
```

`outerGuard(4, 2)` returns true, and `outerGuard(4, 0)` returns false. A single named result keeps its Go
name in a comment, `bool /*ok*/`. Go's `return true` becomes `ok = true;`, because the named result is
returned after the `finally`. A function literal that defers gets its own `ᒐ` inside its lambda
([Function Values and Closures](#function-values-and-closures)).

<!-- Verified against src/go2cs/refLoweringEmissionOperations.go:532 (the eager `nonnil(ref …)` wrap on a
pointer's deref alias, golden DirectBoxReceiverPassedWhole/main.cs.target:22), src/core/golib/builtin.cs
`nonnil`, src/core/golib/ж.cs `operator ~` and ж.StandardBox.cs `Value`, all throwing
RuntimeErrorPanic.NilPointerDereference(). No divide check is emitted anywhere in src/go2cs.
The catch filter maps DivideByZeroException and NullReferenceException:
src/core/golib/runtime/RuntimeErrorPanic.cs:231 (TryAsPanic). Bounds checks throw
RuntimeErrorPanic.IndexOutOfRange directly: src/core/golib/slice.cs:129, array.cs:286.
2026-09-27 accuracy pass: `divide` has no frame, so the mapping happens in outerGuard's catch filter. -->
**Runtime errors are panics too.** In `divide`, `a / b` with `b` zero raises .NET's `DivideByZeroException`.
`outerGuard`'s `catch`, the nearest frame up the stack, turns it into Go's panic,
`runtime error: integer divide by zero`. A nil pointer dereference is mapped the same way, and golib's own
bounds checks raise index-out-of-range panics directly. Each reaches `recover()` with Go's `runtime error`
message.

<!-- An unrecovered panic crashes the process as in Go: the `panic: …` report on stderr and exit code 2,
even from a goroutine (reference: defer-panic-recover.md, crash-report section).
runtime.Stack, runtime.Caller and the crash report name frames Go's way with the Go file and line
(a hand-owned whole-file replacement has no GoPositionMap record and reports its C# position); inside a
deferred call the rendered tracebacks still show the panic site. Reference:
manual-conversions.md#runtimestack-renders-a-go-shaped-traceback-and-recovers-the-panic-site. -->
**A panic that nothing recovers ends the program as Go does**: a `panic: …` report on stderr and exit
code 2, even when it starts in a [goroutine](#goroutines).

**Full detail:** [Reference → Defer / Panic / Recover](ConversionStrategies-Reference/defer-panic-recover.md#defer--panic--recover) — why the body is not a lambda, every named-result form, which deferred calls move into the `finally`, variadic and value-returning deferred calls, each runtime panic value, and the crash report.

---

## Goroutines

Go's `go` statement becomes a call to `goǃ`, a function in [golib](#the-golib-runtime-library), the
runtime library every converted program uses ([`goǃ`](../src/core/golib/builtin.GoroutineLaunchers.cs)).
`goǃ` runs the call on a new goroutine. The name ends in `ǃ`, a letter that looks like `!`, because a
C# name cannot contain `!` ([glyphs](#reading-converted-code-names-and-glyphs)).

Each goroutine is a dedicated operating-system thread. That is the main difference from Go, and the
last part of this section explains what it means for a program.

**`goǃ` takes the function and its arguments separately.** Most `goǃ` overloads take an `Action`, a C#
delegate that returns nothing, followed by the argument values. A matching set of twins takes a
`Func`, a delegate that returns a value, and throws the result away, as Go does. Each overload stores
the values and calls the delegate with them on the new goroutine:

<!-- source: src/core/golib/builtin.GoroutineLaunchers.cs:74-77 -->
```csharp
    public static void goǃ<T>(Action<T> action, T arg)
    {
        Goroutine.Start(() => action(arg), action);
    }
```

Overloads like this one exist for up to sixteen arguments.

**Arguments are evaluated at the `go` statement.** Because the arguments are passed to `goǃ`, they
are computed before the goroutine starts, as in Go:

<!-- source: src/tests/Behavioral/GoCallVariations/GoCallVariations.go:83-87 -->
```go
func printSquare(n int) {
	go fmt.Println("Go thread square:", n*n)
	n++
	fmt.Println("Immediate n:", n)
}
```
<!-- source: src/tests/Behavioral/GoCallVariations/GoCallVariations.cs.target:96-104 -->
```csharp
// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object goThreadSquareˢ = (@string)"Go thread square:"u8;
private static readonly object immediateNˢ = (@string)"Immediate n:"u8;

internal static void printSquare(nint n) {
    goǃ((ᴛ1, ᴛ2) => fmt.Println(ᴛ1, ᴛ2), goThreadSquareˢ, n * n);
    n++;
    fmt.Println(immediateNˢ, n);
}
```

Called as `printSquare(5)`, the goroutine prints 25 even when it runs after `n` has become 6. `n * n`
is computed when `goǃ` is called, not when the goroutine runs.

A few things in this snippet are explained in other sections:

- A name ending in `ˢ` is a string literal hoisted to a static field, so it is created once. `@string`
  is golib's Go string type, and `"…"u8` is a C# UTF-8 literal. The comment above the fields is
  generated; RODATA is the read-only data where Go keeps its literals ([Strings](#strings-string-and-sstring)).
- These fields are typed `object` because `fmt.Println` takes `any` arguments
  ([Empty Interface](#empty-interface-any)).
- Go's `int` becomes `nint`, a native-sized integer ([Integer Types](#integer-types-and-arithmetic)).

**Some callees are wrapped in a lambda.** When the function returns a value, takes a variable number
of arguments, or is a builtin, the converter does not pass it to `goǃ` directly. It writes a lambda
over temporary parameters named `ᴛ1`, `ᴛ2`, and so on. `fmt.Println` needs this form twice over: it is
variadic, and it returns `(n int, err error)`.

A function with fixed parameters and no result is passed as it is. This test shows both cases side by
side:

<!-- source: src/tests/Behavioral/GoStmtValueReturn/main.go:13-84 -->
```go
func sum(out chan int, a, b int) int {
	r := a + b
	out <- r
	return r
}
…
func emit(out chan int) {
	out <- 8
}

func main() {
	out := make(chan int)

	go sum(out, 3, 4) // value-returning, 3 params -> goǃ((ᴛ1, ᴛ2, ᴛ3) => sum(ᴛ1, ᴛ2, ᴛ3), out, 3, 4)
	fmt.Println("sum:", <-out)
	…
	go emit(out) // control: void method group, unchanged bare method group
	fmt.Println("emit:", <-out)
	…
}
```
<!-- source: src/tests/Behavioral/GoStmtValueReturn/main.cs.target:9-76 -->
```csharp
internal static nint sum(channel<nint> @out, nint a, nint b) {
    nint r = a + b;
    @out.ᐸꟷ(r);
    return r;
}
…
internal static void emit(channel<nint> @out) {
    @out.ᐸꟷ(8);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object sumˢ = (@string)"sum:"u8;
…
private static readonly object emitˢ = (@string)"emit:"u8;
…
internal static void Main() {
    var @out = new channel<nint>(0);
    goǃ((ᴛ1, ᴛ2, ᴛ3) => sum(ᴛ1, ᴛ2, ᴛ3), @out, 3, 4);
    fmt.Println(sumˢ, ᐸꟷ(@out));
    …
    goǃ(emit, @out);
    fmt.Println(emitˢ, ᐸꟷ(@out));
    …
}
```

The trailing comments in the Go are the test's own notes; they do not appear in the C#.

`sum` returns a value, so it gets the lambda. C# lets a lambda whose body is a single call convert to
an `Action`, and the call's result is then thrown away. That matches Go, which discards whatever a
goroutine's function returns. `emit` returns nothing, so `go emit(out)` becomes `goǃ(emit, @out)`.

`channel<nint>` is golib's Go channel, and `ᐸꟷ` is Go's `<-` arrow: `@out.ᐸꟷ(r)` sends and
`ᐸꟷ(@out)` receives ([Channels](#channels-and-select)). `out` is a C# keyword, so it is written
`@out`.

**A function literal becomes a lambda, and it shares captured variables.** `go func() { … }()` becomes
`goǃ(() => { … })`. As in Go, the literal captures the variable itself, not a copy of its value:

<!-- source: src/tests/Behavioral/GoCallVariations/GoCallVariations.go:8-40 -->
```go
func main() {
	…
	count := 1
	go func() {
		fmt.Println("Go count (closure):", count)
	}()
	count = 10
	fmt.Println("Count before Go:", count)
	…
}
```
<!-- source: src/tests/Behavioral/GoCallVariations/GoCallVariations.cs.target:8-44 -->
```csharp
// Hoisted @string literals (single allocation; Go keeps these in RODATA)
…
private static readonly object goCountClosureˢ = (@string)"Go count (closure):"u8;
private static readonly object countBeforeGoˢ = (@string)"Count before Go:"u8;
…
internal static void Main() {
    …
    nint count = 1;
    goǃ(() => {
        fmt.Println(goCountClosureˢ, count);
    });
    count = 10;
    fmt.Println(countBeforeGoˢ, count);
    …
}
```

The goroutine may print 1 or 10, depending on when it runs, exactly as the Go program may. In other
converted code you may see a captured variable read through a copy with a `ʗ` suffix, such as
`outʗ1`. The converter makes that copy only where it behaves like Go's shared variable: the variable
is not assigned again after the goroutine is created, or it is a loop variable, where the copy stands
in for Go's per-iteration variable ([Function Values and Closures](#function-values-and-closures)).

**An unrecovered panic in any goroutine ends the whole program**, as in Go. Go's `panic:` report goes
to standard error and the process exits with code 2. golib's `panic` returns an exception, and the
converted code throws it ([Defer / Panic / Recover](#defer--panic--recover)):

<!-- source: src/tests/Behavioral/GoroutinePanicExitCode/main.go:21-33 -->
```go
func main() {
	fmt.Println("before goroutine panic")

	done := make(chan struct{})

	go func() {
		panic("goroutine boom")
	}()

	// Never satisfied: the goroutine's unrecovered panic must crash the whole process with
	// exit code 2 before this receive can proceed.
	<-done
}
```
<!-- source: src/tests/Behavioral/GoroutinePanicExitCode/main.cs.target:7-17 -->
```csharp
// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object beforeGoroutinePanicˢ = (@string)"before goroutine panic"u8;

internal static void Main() {
    fmt.Println(beforeGoroutinePanicˢ);
    var done = new channel<EmptyStruct>(0);
    goǃ(() => {
        throw panic("goroutine boom");
    });
    ᐸꟷ(done);
}
```

`EmptyStruct` is golib's stand-in for Go's `struct{}` ([Struct Types](#struct-types)). `ᐸꟷ(done)` is
the receive `<-done`, which waits until the panic ends the process
([Channels](#channels-and-select)).

**A goroutine keeps its own thread from start to finish.** golib's
[`Goroutine`](../src/core/golib/runtime/Goroutine.cs) class starts a new background thread for every
`goǃ`. A goroutine that blocks on a channel, a lock or a sleep blocks only its own thread. As in Go,
the process exits when `main` returns, even while goroutines are still running.

**Converted code is ordinary blocking code, not `async` code.** A goroutine pauses by blocking its
thread, so each goroutine needs a thread of its own. `goǃ` does not use the .NET thread pool, because
a blocked goroutine would hold a shared pool thread and starve the others. golib cannot run many
goroutines on fewer threads either, because .NET cannot move a running call to another stack. That is
a property of today's .NET rather than of the conversion: goroutines are started in one place in golib,
so if .NET gains threads its runtime can park cheaply, golib can adopt them and converted code stays as
it is.

**Each goroutine thread reserves a large stack.** Go stacks grow as needed, but a .NET stack overflow
ends the process. Each goroutine thread therefore reserves 256 MB of address space; memory is
committed only as it is used. The `GO2CS_GOROUTINE_STACK` environment variable sets a different size.

**Operating-system threads bound live goroutines at roughly ten thousand.** Go's goroutines reach
about a million. The limit comes from how many threads the operating system can run at once, not from
the stack reservation. A Go program that starts hundreds of thousands of goroutines at once does not
run the same way after conversion.

**`runtime` keeps Go's goroutine contracts on top of threads.** `runtime.Goexit` runs the goroutine's
deferred calls and ends only that goroutine. Calling it from `main` itself is not supported and throws
an exception. `runtime.LockOSThread` holds trivially, because each goroutine already owns its thread.
These functions are hand-written C#
([Manually-Converted Declarations](#manually-converted-declarations)).

**`sync` and `sync/atomic` run on .NET primitives.** A `sync.Mutex` blocks its thread on a .NET
semaphore. Most `sync/atomic` functions are one line: `AddInt32` returns
`Interlocked.Add(ref addr.Value, delta)`. Here `addr` is a `ж<int32>`, golib's box for a Go pointer,
and `.Value` is the `int32` it points to ([Pointers](#pointers)).

**Full detail:** [Reference → Goroutine callees](ConversionStrategies-Reference/defer-panic-recover.md#a-value-returning-goroutine-callee-is-wrapped-in-a-discarding-lambda) —
the other `go`-statement forms (named function types, builtins, value receivers, multi-value arguments),
where captured locals are copied, and the tests that guard each form.

---

## Channels and `select`

Go's `chan T` becomes golib [`channel<T>`](../src/core/golib/channel.cs). golib is go2cs's runtime
library ([The golib Runtime Library](#the-golib-runtime-library)). `channel<T>` is a port of Go's own
channel runtime, so blocking, buffering and closing behave as in Go. What the reader notices is the
arrow glyph `ᐸꟷ`, which spells both send and receive
([glyph table](#reading-converted-code-names-and-glyphs)).

**Send is a method call and receive is a function call.** `ch <- v` becomes `ch.ᐸꟷ(v)`, and `<-ch`
becomes `ᐸꟷ(ch)`. `make(chan int, 2)` becomes `new channel<nint>(2)`, whose argument is the buffer size.
Go's `int` is C# `nint` ([Integer Types](#integer-types-and-arithmetic)):

<!-- source: src/tests/Behavioral/SelectStatement/SelectStatement.go:92-98 -->
```go
ch := make(chan int, 2)

ch <- 1
ch <- 2

fmt.Println(<-ch)
fmt.Println(<-ch)
```
<!-- source: src/tests/Behavioral/SelectStatement/SelectStatement.cs.target:109-113 -->
```csharp
var ch = new channel<nint>(2);
ch.ᐸꟷ(1);
ch.ᐸꟷ(2);
fmt.Println(ᐸꟷ(ch));
fmt.Println(ᐸꟷ(ch));
```

**`len`, `cap` and `close` keep their names, and a comma-ok receive adds `ꟷ`.** `ꟷ` is a golib constant
of type `bool`. Its value does not matter: passing it only selects the overload of `ᐸꟷ` that returns
`(value, ok)`. A comma-ok map read works the same way ([comma-ok forms](#multi-result-values-and-comma-ok-forms)).

A closed channel still yields its buffered values with `ok` true, then the zero value with `ok` false.
This program prints `7 true`, `8 true`, then `0 false`:

<!-- source: src/tests/Behavioral/ChannelCapLen/main.go:30-39 -->
```go
d := make(chan int, 2)
d <- 7
d <- 8
close(d)
v, ok := <-d
fmt.Println(v, ok)
v, ok = <-d
fmt.Println(v, ok)
v, ok = <-d
fmt.Println(v, ok)
```
<!-- source: src/tests/Behavioral/ChannelCapLen/main.cs.target:24-33 -->
```csharp
var d = new channel<nint>(2);
d.ᐸꟷ(7);
d.ᐸꟷ(8);
close(d);
var (v, ok) = ᐸꟷ(d, ꟷ);
fmt.Println(v, ok);
(v, ok) = ᐸꟷ(d, ꟷ);
fmt.Println(v, ok);
(v, ok) = ᐸꟷ(d, ꟷ);
fmt.Println(v, ok);
```

<!-- source for the nil-channel lines: src/tests/Behavioral/ChannelCapLen/main.go:27-28 -> main.cs.target:22-23 (`channel<nint> n = default!;`) -->
**Blocking and nil match Go.** `make(chan int)` becomes `new channel<nint>(0)`: an unbuffered channel,
so a send waits for a receiver. A nil channel is the C# `default!`, the zero value of `channel<T>`
([Nil and Zero Values](#nil-and-zero-values)). `len` and `cap` of a nil channel are 0. A send or
receive on a nil channel blocks forever, as in Go.

**A channel's direction shows as a marker comment.** The marker `/*<-*/` sits where Go's arrow sits.
Receive-only `<-chan T` renders as `/*<-*/channel<T>`, and send-only `chan<- T` as `channel/*<-*/<T>`.
All three directions are one C# type, so the comment is only for the reader.

In this example, `outputˢ` is the `"output"` literal, hoisted into a `static readonly` field so it is
created once ([Strings](#strings-string-and-sstring)). `@string` is golib's Go-style byte string, and a
`u8` literal is C#'s UTF-8 byte literal:

<!-- source: src/tests/Behavioral/SelectStatement/SelectStatement.go:38-40 -->
```go
func sendOnly(s chan<- string) {
	s <- "output"
}
```
<!-- source: src/tests/Behavioral/SelectStatement/SelectStatement.cs.target:47-51 -->
```csharp
private static readonly @string outputˢ = "output"u8;

internal static void sendOnly(channel/*<-*/<@string> s) {
    s.ᐸꟷ(outputˢ);
}
```

**Passing a channel where a directional one is expected keeps the direction in the value.** The call
wraps the argument in `WithDirection`, so the channel value knows its direction, as a Go value does.
`goǃ` is the [`go` statement](#goroutines). Go comments are dropped unless the converter runs with
`-comments` ([Comments](#comments)), so the Go comment here has no C# line:

<!-- source: src/tests/Behavioral/SelectStatement/SelectStatement.go:161-164 -->
```go
mychanl := make(chan string)

// function converts bidirectional channel to send only channel
go sendOnly(mychanl)
```
<!-- source: src/tests/Behavioral/SelectStatement/SelectStatement.cs.target:194-195 -->
```csharp
var mychanl = new channel<@string>(0);
goǃ(sendOnly, mychanl.WithDirection(GoChanDir.Send));
```

**`range` over a channel becomes `foreach`.** The loop receives each value and ends when the channel
is closed and drained, as Go's does ([Loops, Range and Labels](#loops-range-and-labels)). Again, the
Go comments are dropped by default ([Comments](#comments)):

<!-- source: src/tests/Behavioral/SelectStatement/SelectStatement.go:62-68 -->
```go
func filter(src <-chan int, dst chan<- int, prime int) {
	for i := range src { // Loop over values received from 'src'.
		if i%prime != 0 {
			dst <- i // Send 'i' to channel 'dst'.
		}
	}
}
```
<!-- source: src/tests/Behavioral/SelectStatement/SelectStatement.cs.target:70-76 -->
```csharp
internal static void filter(/*<-*/channel<nint> src, channel/*<-*/<nint> dst, nint prime) {
    foreach (var i in src) {
        if (i % prime != 0) {
            dst.ᐸꟷ(i);
        }
    }
}
```

**`select` becomes a C# `switch` over golib's `select(…)` call.** Four steps happen, in this order:

1. Each case's operands are evaluated once, in source order, into a temporary named `selᴛN`. The `ᴛ`
   marks a name the converter makes up, and `N` counts up through the file
   ([Multi-Assignment](#multi-assignment-and-evaluation-order)). For a receive, the temporary holds the
   channel. For a send, it holds the registration itself (step 2), which has already captured the
   channel and the value.
2. Each case is *registered* rather than performed: `ch.ᐸꟷ(v, ꓸꓸꓸ)` for a send and `ᐸꟷ(ch, ꓸꓸꓸ)` for a
   receive. `ꓸꓸꓸ` is a golib marker value. As a lone argument, it only picks the overload that registers
   the operation, the way `ꟷ` picks the comma-ok overload ([glyph table](#reading-converted-code-names-and-glyphs)).
3. `select(…)` waits until a case is ready, commits one chosen at random as Go does, and returns its
   position, counting from 0.
4. The matching `case N:` runs. A receive case adds a `when` guard that calls `ꟷᐳ`, the receive arrow
   reversed. For the chosen case the guard is always true. It exists so the received value can be handed
   to that case alone; `out _` discards it.

In this example, `while (ᐧ)` is Go's endless `for`, where `ᐧ` is golib's constant `true`
([Loops](#loops-range-and-labels)). `quitˢ` is the hoisted `"quit"` literal. It is stored as an `object`
because `fmt.Println` takes Go's `any` ([Empty Interface](#empty-interface-any)). The closing `}}`
ends the last case and the `switch` together:

<!-- source: src/tests/Behavioral/SelectStatement/SelectStatement.go:25-36 -->
```go
func fibonacci(f, quit chan int) {
	x, y := 0, 1
	for {
		select {
		case f <- x:
			x, y = y, x+y
		case <-quit:
			fmt.Println("quit")
			return
		}
	}
}
```
<!-- source: src/tests/Behavioral/SelectStatement/SelectStatement.cs.target:26-44 -->
```csharp
private static readonly object quitˢ = (@string)"quit"u8;

internal static void fibonacci(channel<nint> f, channel<nint> quit) {
    nint x = 0;
    nint y = 1;
    while (ᐧ) {
        var selᴛ1 = f.ᐸꟷ(x, ꓸꓸꓸ);
        var selᴛ2 = quit;
        switch (select(selᴛ1, ᐸꟷ(selᴛ2, ꓸꓸꓸ))) {
        case 0: {
            (x, y) = (y, x + y);
            break;
        }
        case 1 when selᴛ2.ꟷᐳ(out _): {
            fmt.Println(quitˢ);
            return;
        }}
    }
}
```

**A receive case that names a variable declares it in the `when` guard.** `case m := <-a:` becomes
`case 0 when selᴛ12.ꟷᐳ(out var m):`, as in the example that follows.

<!-- source: src/tests/Behavioral/SelectStatement/SelectStatement.go:117 (case v1, ok := <-ch3:) -> SelectStatement.cs.target:137 -->
A comma-ok case adds a second `out` variable: `case v1, ok := <-ch3:` becomes
`case 2 when selᴛ5.ꟷᐳ(out var v1, out var okΔ1):`. The `Δ1` suffix renames a variable whose name
is used again in the same function ([Shadowing](#short-variable-redeclaration-shadowing)).

When every case returns, Go needs no `return` after the `select`. C# cannot prove the `switch` always
returns, so an unreachable `return default!;` follows it. The `"a:"u8` literal stays inline, because a
literal in a concatenation is not hoisted ([Strings](#strings-string-and-sstring)):

<!-- source: src/tests/Behavioral/SelectStatement/SelectStatement.go:189-196 -->
```go
func firstMsg(a, b chan string) string {
	select {
	case m := <-a:
		return "a:" + m
	case m := <-b:
		return "b:" + m
	}
}
```
<!-- source: src/tests/Behavioral/SelectStatement/SelectStatement.cs.target:214-225 -->
```csharp
internal static @string firstMsg(channel<@string> a, channel<@string> b) {
    var selᴛ12 = a;
    var selᴛ13 = b;
    switch (select(ᐸꟷ(selᴛ12, ꓸꓸꓸ), ᐸꟷ(selᴛ13, ꓸꓸꓸ))) {
    case 0 when selᴛ12.ꟷᐳ(out var m): {
        return "a:"u8 + m;
    }
    case 1 when selᴛ13.ꟷᐳ(out var m): {
        return "b:"u8 + m;
    }}
    return default!;
}
```

**With a `default:` clause, the `select` calls `trySelect` instead.** It checks the same registrations
without waiting and returns -1 when no case is ready. The C# `default:` label then runs exactly when Go's
would. Go's `struct{}` is golib's [`EmptyStruct`](../src/core/golib/EmptyStruct.cs), a struct with no
fields. `doneˢ` and `pendingˢ` are the hoisted `"done"` and `"pending"` literals:

<!-- source: src/tests/Behavioral/SelectStatement/SelectStatement.go:200-207 -->
```go
func poll(done chan struct{}) string {
	select {
	case <-done:
		return "done"
	default:
	}
	return "pending"
}
```
<!-- source: src/tests/Behavioral/SelectStatement/SelectStatement.cs.target:228-241 -->
```csharp
private static readonly @string doneˢ = "done"u8;
private static readonly @string pendingˢ = "pending"u8;

internal static @string poll(channel<EmptyStruct> done) {
    var selᴛ14 = done;
    switch (trySelect(ᐸꟷ(selᴛ14, ꓸꓸꓸ))) {
    case 0 when selᴛ14.ꟷᐳ(out _): {
        return doneˢ;
    }
    default: {
        break;
    }}
    return pendingˢ;
}
```

<!-- source: src/tests/Behavioral/SelectStatement/SelectStatement.go:126-140 (ch1 = nil; close(ch2); the select picks the closed ch2) -> SelectStatement.cs.target:146-173; poll(done) before and after close(done): SelectStatement.go:177-180. Buffered-first on a closed channel: golib channel.cs is a port of Go's hchan/selectgo, and a select receive reads the same buffer (ChannelCapLen/main.go:30-39 shows it for plain receives). -->
**Nil and closed channels inside a `select` also match Go.** A nil channel's case is never chosen.
A closed channel's receive case is always ready: it yields any values still buffered, then the zero
value with `ok` false.

**Full detail:** [Reference → Maps and Channels](ConversionStrategies-Reference/maps-and-channels.md#select-statement-lowering-terminating-and-empty-clauses) — how golib ports Go's channel and `select` runtime, the exact operand-evaluation rules, and named channel types.

---

## Struct Types

A Go struct becomes a C# `partial struct` marked `[GoType]`. The converter writes only the fields, so the
declaration reads like the Go original. A [source generator](#source-generators) completes the other
`partial` half at compile time, adding constructors, equality and more. Like a Go struct, a C# struct is a
value type: it is copied on assignment and needs no heap allocation of its own.

**The declaration keeps Go's fields in Go's order.** An exported field (capitalized) is `public`, and an
unexported one is `internal`. Fields declared together, such as `x, y int`, stay on one line when they
share a type and an access level. A struct literal with field names becomes a constructor call with named
arguments:

<!-- source: src/tests/Behavioral/CombinedStructFields/CombinedStructFields.go:15-27 -->
```go
type mixed struct {
	X, y  int    // differing access -> separate lines
	p, q  int    // uniform -> combined
	label string // single name -> single line
}

func main() {
…
	m := mixed{X: 10, y: 20, p: 30, q: 40, label: "tag"}
	fmt.Println(m.X, m.y, m.p, m.q, m.label)
}
```
<!-- source: src/tests/Behavioral/CombinedStructFields/CombinedStructFields.cs.target:13-25 -->
```csharp
[GoType] partial struct mixed {
    public nint X;
    internal nint y;
    internal nint p, q;
    internal @string label;
}

internal static void Main() {
…
    var m = new mixed(X: 10, y: 20, p: 30, q: 40, label: "tag"u8);
    fmt.Println(m.X, m.y, m.p, m.q, m.label);
}
```

Go's `int` is C# `nint`, a native-size integer ([Integer Types and Arithmetic](#integer-types-and-arithmetic)).
Go's `string` is golib's `@string`, a byte string. `"tag"u8` is a C# UTF-8 literal that converts to one
([Strings](#strings-string-and-sstring)).

**The generator completes each struct.** For every `[GoType]` struct it adds:

- a constructor that takes every field as an optional argument, a parameterless one, and one that takes
  `nil` and gives the zero value;
- `==` and `!=`, which compare field by field, as Go's `==` does (a C# struct has no `==` of its own);
- a static accessor, `Ꮡname`, for each field, which `&s.name` uses (`Ꮡ` marks an address);
- a `ToString()` that prints the fields in Go's `%v` form: `Person{name: "Dr. Michał", age: 29}` prints
  `{Dr. Michał 29}`.
  <!-- %v form: src/tests/Behavioral/StructPromotion/StructPromotion.go:66-67 `person := Person{name: "Dr. Michał", age: 29}` / `fmt.Println(person) // {Dr. Michał 29}` -->

The [generator template](../src/gen/go2cs-gen/Templates/StructType/StructTypeTemplate.cs) shows the
exact members.

**A literal without field names passes the values in field order.** An omitted field keeps its zero value,
and an empty literal `Thing{}` becomes `new Thing(nil)`. A pointer field `*T` is a golib `ж<T>` (read
"zhe"): a class that holds one `T` on the heap and plays the role of Go's `*T`
([Pointers](#pointers), [glyph table](#reading-converted-code-names-and-glyphs)). A `nil` argument is
golib's `nil`:
<!-- empty literal: src/tests/Behavioral/LocalStructFieldAddr/main.go:88 `t := Thing{}` -> main.cs.target:88 `var t = new Thing(nil);` -->

<!-- source: src/tests/Behavioral/StructWithPointer/StructWithPointer.go:5-20 -->
```go
type ColorList struct {
    Total int
    Color string
    Next *ColorList
    NextNext **ColorList
}

func main() {
   red := ColorList{2, "red", nil, nil}
   blue := ColorList{2, "blue", nil, nil}

   red.Next = &blue

   fmt.Printf("Value of red = %v\n", red)
   fmt.Printf("Value of blue = %v\n", blue)
}
```
<!-- source: src/tests/Behavioral/StructWithPointer/StructWithPointer.cs.target:7-21 -->
```csharp
[GoType] partial struct ColorList {
    public nint Total;
    public @string Color;
    public ж<ColorList> Next;
    public ж<ж<ColorList>> NextNext;
}

internal static void Main() {
    var red = new ColorList(2, "red"u8, nil, nil);
    ref var blue = ref heap<ColorList>(out var Ꮡblue);
    blue = new ColorList(2, "blue"u8, nil, nil);
    red.Next = Ꮡblue;
    fmt.Printf("Value of red = %v\n"u8, red);
    fmt.Printf("Value of blue = %v\n"u8, blue);
}
```

**A struct whose address is taken lives in a box.** `&blue` needs a pointer that can outlive the call, so
golib's `heap<T>(out var Ꮡblue)` allocates a zeroed box, stores it in `Ꮡblue`, and returns a C# `ref`
to the value inside. The literal is then assigned through that `ref`. `blue` reads and writes the boxed
value, and `Ꮡblue` is the box itself, which is what `&blue` gives ([Pointers](#pointers)).

`Ꮡ` is also a golib function: `Ꮡ(value)` boxes a new value and returns the pointer. So `&T{…}` becomes
`Ꮡ(new T(…))`, and `&counter{n: 5}` becomes `Ꮡ(new counter(n: 5))`.
<!-- &T{…}: src/tests/Behavioral/IncDecPointerField/main.go:21 -> main.cs.target:21 `var @base = Ꮡ(new counter(n: 5));` -->

**`&s.f` points into the struct's box.** The generator's accessor `Thing.Ꮡval` is a static method that
returns a `ref` to the `val` field of a given `Thing`. The box method `.of(…)` applies it, so
`Ꮡx.of(Thing.Ꮡval)` is a `ж<nint>` pointing at `val` inside the box that holds `x`. `p.Value` is Go's
`*p`, so the write reaches `x`:
<!-- accessor shape: src/gen/go2cs-gen/Templates/StructType/StructTypeTemplate.cs:1391 `static ref {typeName} Ꮡ{member}(ref {StructName} instance) => ref instance.{member};`; of: src/core/golib/ж.cs:206-217 -->

<!-- source: src/tests/Behavioral/LocalStructFieldAddr/main.go:10-25 -->
```go
type Thing struct {
	val int
}
…
func plainFunc() int {
	x := Thing{val: 7}
	p := &x.val
	*p = 99
	return x.val
}
```
<!-- source: src/tests/Behavioral/LocalStructFieldAddr/main.cs.target:7-22 -->
```csharp
[GoType] partial struct Thing {
    internal nint val;
}
…
internal static nint plainFunc() {
    ref var x = ref heap<Thing>(out var Ꮡx);
    x = new Thing(val: 7);
    var p = Ꮡx.of(Thing.Ꮡval);
    p.Value = 99;
    return x.val;
}
```

**A declared zero value is `default!` when C#'s default already equals Go's zero.** `default!` is C#'s
all-zero value. The converter writes it the same way for every type; on a struct, the `!` has no effect
([Nil and Zero Values](#nil-and-zero-values)).

A struct with a fixed-size array field is built with `new()` instead. Golib's `array<T>` is Go's `[N]T`,
and its field initializer `new(8)` gives it Go's length 8, but C#'s `default` skips field initializers.
Golib's `slice<T>` is Go's `[]T` ([Slices and Arrays](#slices-and-arrays)):

<!-- source: src/tests/Behavioral/ZeroValueStructVar/main.go:16-57 -->
```go
type holder struct {
	name string
	tbl  [8]int
	tail []int
}
…
type point struct {
	x, y int
}

func main() {
…
	var z holder
…
	var p point
…
}
```
<!-- source: src/tests/Behavioral/ZeroValueStructVar/main.cs.target:7-39 -->
```csharp
[GoType] partial struct holder {
    internal @string name;
    internal array<nint> tbl = new(8);
    internal slice<nint> tail;
}
…
[GoType] partial struct point {
    internal nint x, y;
}

internal static void Main() {
…
    holder z = new();
…
    point p = default!;
…
}
```

A struct that [embeds](#struct-type-embedding) another type is built with `new(nil)`, because its
embedded field lives in a box that `default` would leave empty.
<!-- embedded: src/tests/Behavioral/UnsafeOperations/UnsafeOperations.go:44-48,141 `var e Embedded` -> UnsafeOperations.cs.target:38-42,109 `Embedded e = new(nil);` -->

**A struct is copied by value, as in Go.** A C# struct assignment copies the fields. But golib's
`array<T>` is a small struct that points at its element storage, so a plain copy would share the
elements. A struct holding an array therefore gets a generated `ΔClone()` that copies them, and each
copy calls it. The `Δ` prefix keeps the name clear of any Go name.

Here `newDigest` returns a filled-in `digest`, `bump` adds one to its fields through a pointer receiver,
and `show` formats them. `d` keeps its original values:

<!-- source: src/tests/Behavioral/StructArrayFieldValueCopy/StructArrayFieldValueCopy.go:10-63 -->
```go
type digest struct {
	h  [4]int
	x  [2]byte
	nx int
}

func (d *digest) bump() {
	for i := range d.h {
		d.h[i]++
	}
	d.x[0]++
	d.nx++
}

func (d digest) show() string {
	return fmt.Sprintf("%v %v %d", d.h, d.x, d.nx)
}
…
func newDigest() digest {
	return digest{h: [4]int{1, 2, 3, 4}, x: [2]byte{5, 6}, nx: 7}
}
…
func identCopy() {
	d := newDigest()
	c := d
	c.bump()
	fmt.Println("ident:      ", d.show(), "|", c.show())
}
```
<!-- source: src/tests/Behavioral/StructArrayFieldValueCopy/StructArrayFieldValueCopy.cs.target:7-69 -->
```csharp
[GoType] partial struct digest {
    internal array<nint> h = new(4);
    internal array<byte> x = new(2);
    internal nint nx;
}

[GoRecv] internal static void bump(this ref digest d) {
    foreach (var (i, _) in d.h) {
        d.h[i]++;
    }
    d.x[0]++;
    d.nx++;
}

internal static @string show(this digest d) {
    d = d.ΔClone();

    return fmt.Sprintf("%v %v %d"u8, d.h, d.x, d.nx);
}
…
internal static digest newDigest() {
    return new digest(h: new nint[]{1, 2, 3, 4}.array(), x: new byte[]{5, 6}.array(), nx: 7);
}
…
// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object identˢ = (@string)"ident:      "u8;

internal static void identCopy() {
    var d = newDigest();
    var c = d.ΔClone();
    c.bump();
    fmt.Println(identˢ, d.show(), (@string)"|"u8, c.show());
}
```

`c := d` becomes `d.ΔClone()`. A value receiver or value parameter of such a struct is cloned at the top
of the function, as `show` does with `d = d.ΔClone();`. Methods become C# extension methods, and
`[GoRecv]` marks a pointer receiver ([Functions and Methods](#functions-and-methods)).
<!-- parameter clone: StructArrayFieldValueCopy.go:35 takeValue(c digest) -> StructArrayFieldValueCopy.cs.target:32-33 `c = c.ΔClone();` -->

`identˢ` is the string literal `"ident:      "`, created once in a static field; the `ˢ` suffix marks
such a field ([Strings](#strings-string-and-sstring)). It is stored already boxed as `object` because it
is only used as an `any` value ([Empty Interface](#empty-interface-any)). A literal with no word in it,
such as `"|"`, stays inline and is cast to `@string` in place.

**A struct type declared inside a function moves to package scope**, since C# allows no type in a method
body. It takes the function's name as a prefix, so `point` in `main` becomes `main_point`. It is marked
`[GoType("dyn")]`, which tells the generator and golib's reflection that the type was lifted and its C#
name is not its Go name. An anonymous struct is lifted the same way, named after the first variable that
uses it. Go treats identical anonymous structs as one type, so `a` and `b` share `main_a`:

<!-- source: src/tests/Behavioral/LiftedLocalTypes/main.go:17-42 -->
```go
func main() {
…
	a := struct{ X int }{X: 1}
	var b struct{ X int }
	b = a
	fmt.Println(a == b)
…
	type point struct{ X, Y int }
	p := point{X: 1, Y: 2}
	fmt.Println(p.X + p.Y)
	fmt.Printf("%T %T\n", p, &p)
…
}
```
<!-- source: src/tests/Behavioral/LiftedLocalTypes/main.cs.target:13-35 -->
```csharp
[GoType("dyn")] internal partial struct main_a {
    public nint X;
}

[GoType("dyn")] internal partial struct main_point {
    public nint X, Y;
}

internal static void Main() {
    var a = new main_a(X: 1);
    main_a b = default!;
    b = a;
    fmt.Println(a == b);
    ref var p = ref heap<main_point>(out var Ꮡp);
    p = new main_point(X: 1, Y: 2);
    fmt.Println(p.X + p.Y);
    fmt.Printf("%T %T\n"u8, p, Ꮡp);
…
}
```

`p` lives in a box because `&p` is taken, and `Ꮡp` is that box. `%T` still prints Go's names,
`main.point *main.point`, not the C# names.

An anonymous struct in a parameter takes the function and parameter names, such as
`processAnonymousStruct_data`. A package variable `settings` of an anonymous struct type gets the type
`settingsᴛ1`: the variable already uses the name `settings`, so the type takes a numbered `ᴛ` suffix,
the mark of a name the converter makes up ([Names and Glyphs](#reading-converted-code-names-and-glyphs)).
<!-- parameter lift: src/tests/Behavioral/AnonymousStructs/AnonymousStructs.go:22-25 -> AnonymousStructs.cs.target:20-25; reuse in main: AnonymousStructs.go:57-60 -> AnonymousStructs.cs.target:61 `var anonPerson = new processAnonymousStruct_data(Name: "Bob"u8, Age: 25);` (an identical anonymous struct reuses a type already lifted in the same file) -->
<!-- ᴛ suffix: src/tests/Behavioral/AnonymousStructs/AnonymousStructs.go:16-19 `var settings = struct {…}{…}` -> AnonymousStructs.cs.target:13 `[GoType("dyn")] partial struct settingsᴛ1 {` -->

The empty struct `struct{}` is never lifted. It is golib's shared `EmptyStruct`, so `struct{}{}` becomes
`new EmptyStruct()`.
<!-- empty struct: src/tests/Behavioral/AnonymousStructs/AnonymousStructs.go:47 `seen[memo] = struct{}{}` -> AnonymousStructs.cs.target:40 `seen[memo] = new EmptyStruct();` -->

**Full detail:** [Reference → Struct Types](ConversionStrategies-Reference/struct-types.md#struct-types) — the rules for combined fields and access modifiers, every zero-value form, how lifted types are found and shared, and the positional-literal edge cases such as a one-field `nil` literal.

---

## Struct Type Embedding

Go has no inheritance. Instead, a struct can *embed* another type by listing the type with no field
name. The embedded type's fields and methods are then *promoted*: they can be used directly on the outer
struct. C# structs cannot inherit either. So the converter declares each embed as an ordinary member, and
a [source generator](#source-generators) writes the code that makes the promoted names work.

<!-- Section runs longer than the usual summary length: embedding has four distinct shapes (value embed,
copy semantics, pointer embed at any depth, interface embed) plus one call-site rewrite a reader will meet
in converted code. Each is shown with a complete example, and each glyph and golib type is explained where
it appears, so the section stands on its own for a reader who lands here first. -->

**An embedded struct becomes a `ref` property named for its type.** The converter emits
`partial ref T T { get; }`, and the generator supplies its body. The generator stores the embedded value
in a private field of the outer struct, and the property returns a `ref` to that field. It is a `ref`
because Go's `record.Person` is a variable: it can be assigned, have its address taken, or receive a
method call.
<!-- Generated body shape: docs/ConversionStrategies-Reference/struct-embedding.md, "An embedded struct is an
INLINE field" (`private @object ʗobject;` + `[UnscopedRef] internal partial ref @object @object => ref ʗobject;`);
emission at src/gen/go2cs-gen/Templates/StructType/StructTypeTemplate.cs:205. -->

**Promoted fields and methods are used on the outer value directly.** For each promoted field the
generator adds a `ref` property to the outer struct, so `record.name` reads and writes
`record.Person.name`. For each promoted method it adds a forwarding method, so `record.IsAdult()` calls
`IsAdult` on `record.Person`. A method the outer type declares itself hides the promoted one, as in Go.

Here `Record` embeds `Person` and `Employee`. `Record` declares its own `IsDr`, which also requires an age
over 18. So `record.IsDr()` calls that one and prints `false`:

<!-- source: src/tests/Behavioral/StructPromotion/StructPromotion.go:8-86 -->
```go
type Person struct {
	name string
	age  int32
}

func (p Person) IsDr() bool {
	return strings.HasPrefix(p.name, "Dr")
}

func (p Person) IsAdult() bool {
	return p.age >= 18
}

type Employee struct {
	position string
}

func (e Employee) IsManager() bool {
	return e.position == "manager"
}

type Record struct {
	Person
	Employee
}

func (p Record) IsDr() bool {
	return strings.HasPrefix(p.name, "Dr") && p.age > 18
}
…
func main() {
…
	record := Record{}
	record.name = "Dr. Michał"
	record.age = 18
	record.position = "software engineer"

	fmt.Println(record)             // {{Dr. Michał 18} {software engineer}}
	fmt.Println(record.name)        // Dr. Michał
	fmt.Println(record.age)         // 18
	fmt.Println(record.position)    // software engineer
	fmt.Println(record.IsAdult())   // true
	fmt.Println(record.IsManager()) // false
	fmt.Println(record.IsDr())      // false
…
}
```
<!-- source: src/tests/Behavioral/StructPromotion/StructPromotion.cs.target:8-81 -->
```csharp
[GoType] partial struct Person {
    internal @string name;
    internal int32 age;
}

public static bool IsDr(this Person p) {
    return strings.HasPrefix(p.name, "Dr"u8);
}

public static bool IsAdult(this Person p) {
    return p.age >= 18;
}

[GoType] partial struct Employee {
    internal @string position;
}

public static bool IsManager(this Employee e) {
    return e.position == "manager"u8;
}

[GoType] partial struct Record {
    public partial ref Person Person { get; }
    public partial ref Employee Employee { get; }
}

public static bool IsDr(this Record p) {
    return strings.HasPrefix(p.name, "Dr"u8) && p.age > 18;
}
…
private static readonly @string drMichaˢ = "Dr. Michał"u8;
private static readonly @string softwareEngineerˢ = "software engineer"u8;

internal static void Main() {
…
    var record = new Record(nil);
    record.name = drMichaˢ;
    record.age = 18;
    record.position = softwareEngineerˢ;
    fmt.Println(record);
    fmt.Println(record.name);
    fmt.Println(record.age);
    fmt.Println(record.position);
    fmt.Println(record.IsAdult());
    fmt.Println(record.IsManager());
    fmt.Println(record.IsDr());
…
}
```

Some other things in this example that the reader will see throughout converted code:

- `[GoType]` marks a converted Go struct that the generator completes ([Struct Types](#struct-types)).
- A capitalized (exported) Go name becomes `public`, and any other name becomes `internal`. So the
  `Person` embed is `public`, and the `name` field is `internal`
  ([Names and Glyphs](#reading-converted-code-names-and-glyphs)).
- A Go method becomes a static C# extension method, and `this Person p` is Go's value receiver
  ([Functions and Methods](#functions-and-methods)).
- `@string` is golib's Go string, and `"…"u8` is a C# UTF-8 literal ([Strings](#strings-string-and-sstring)).
- `drMichaˢ` and `softwareEngineerˢ` are string literals created once, as `static readonly` fields declared
  just before `Main`. The `ˢ` suffix marks such a field. Short literals such as `"Dr"`, and literals in
  a comparison such as `"manager"`, stay inline ([Strings](#strings-string-and-sstring)).
- `new Record(nil)` is Go's zero value `Record{}`. The converter writes the zero value of a struct that
  embeds another struct as a call to its generated constructor, `new T(nil)`, instead of C# `default`.
  The `nil` argument only selects that zero-value constructor ([Nil and Zero Values](#nil-and-zero-values)).
<!-- Accuracy: not every embedding struct takes this form. A struct that embeds only a same-package interface
keeps a plain field (reverse below is built as `new reverse(data)`), and a zero value golib makes as
`default(T)` (a missing-key map read, a fresh `make`d slice's elements) runs no constructor. See
docs/ConversionStrategies-Reference/struct-embedding.md, "Zero values of promoted-embed structs construct
through a generated constructor". Hoisting rules: docs/ConversionStrategies-Reference/strings.md, "What
stays inline" (3 characters or fewer; comparisons; composite-literal elements). -->

**The embedded struct is stored inline, so copying the outer struct copies it.** An embed is a field
like any other, not a separate heap object. After `b := a`, `b` has its own `inner`, and writing `b.n`
leaves `a.n` unchanged. This program prints `assign a: 1 a 10`, then `assign b: 2 b 20`:

<!-- source: src/tests/Behavioral/EmbeddedStructValueCopy/EmbeddedStructValueCopy.go:12-59 -->
```go
type inner struct {
	n   int
	tag string
}

type mid struct {
	inner
	extra int
}
…
	a := mid{inner: inner{n: 1, tag: "a"}, extra: 10}
	b := a
	b.n = 2
	b.tag = "b"
	b.extra = 20
	fmt.Println("assign a:", a.n, a.tag, a.extra)
	fmt.Println("assign b:", b.n, b.tag, b.extra)
```
<!-- source: src/tests/Behavioral/EmbeddedStructValueCopy/EmbeddedStructValueCopy.cs.target:7-71 -->
```csharp
[GoType] partial struct inner {
    internal nint n;
    internal @string tag;
}

[GoType] partial struct mid {
    internal partial ref inner inner { get; }
    internal nint extra;
}
…
private static readonly object assignAˢ = (@string)"assign a:"u8;
private static readonly object assignBˢ = (@string)"assign b:"u8;
…
    var a = new mid(inner: new inner(n: 1, tag: "a"u8), extra: 10);
    var b = a;
    b.n = 2;
    b.tag = "b"u8;
    b.extra = 20;
    fmt.Println(assignAˢ, a.n, a.tag, a.extra);
    fmt.Println(assignBˢ, b.n, b.tag, b.extra);
```

`var b = a` is a plain C# struct copy, and it copies the inline `inner` too. `assignAˢ` and `assignBˢ` are
the two label literals, created once. `fmt.Println` takes `any` (C# `object`) arguments, so each label is
stored already boxed as an `object` ([Empty Interface](#empty-interface-any)). The short literals `"a"`
and `"b"` stay inline ([Strings](#strings-string-and-sstring)). Go's `int` is C# `nint`
([Integer Types](#integer-types-and-arithmetic)).

**An embedded pointer becomes a `ref` property that holds a `ж<T>`.** `ж<T>` (read "zhe") is golib's
heap box for a Go pointer. `Ꮡ(…)` takes an address, like Go's `&`, so `&leaf{n: 0}` becomes
`Ꮡ(new leaf(n: 0))` ([Pointers](#pointers)). Promoted fields and methods reach through the pointer.

Copying the outer struct copies only the pointer, so both copies share one pointee, as in Go.
<!-- Shared pointee after a copy: EmbeddedStructValueCopy, ptrHolder (h3 := h1; h3.n = 70). -->

**Promotion reaches any depth.** In the program that follows, `top` embeds `mid`, which embeds `*leaf`.
Go's `t.bump()` and `t.n` convert unchanged: `top`'s generated members reach `mid`'s, which reach through
the pointer. This program's `mid` is its own type, unrelated to the `mid` of the copy example. It prints
`2 2 h`, then `11 11 t`:

<!-- source: src/tests/Behavioral/PointerEmbeddingPromotion/main.go:5-40 -->
```go
type leaf struct{ n int }

func (l *leaf) bump()    { l.n++ }
func (l *leaf) get() int { return l.n }
…
type holder struct {
	*leaf
	tag string
}
…
type mid struct {
	*leaf
}

type top struct {
	mid
	label string
}

func main() {
	h := holder{leaf: &leaf{n: 0}, tag: "h"}
	h.bump() // one-level: h.leaf.bump()
	h.bump()
	fmt.Println(h.get(), h.n, h.tag) // 2 2 h

	t := top{mid: mid{leaf: &leaf{n: 10}}, label: "t"}
	t.bump() // transitive: top -> mid -> *leaf -> leaf.bump()
	fmt.Println(t.get(), t.n, t.label) // 11 11 t
}
```
<!-- source: src/tests/Behavioral/PointerEmbeddingPromotion/main.cs.target:7-41 -->
```csharp
[GoType] partial struct leaf {
    internal nint n;
}

[GoRecv] internal static void bump(this ref leaf l) {
    l.n++;
}

[GoRecv] internal static nint get(this ref leaf l) {
    return l.n;
}

[GoType] partial struct holder {
    internal partial ref ж<leaf> leaf { get; }
    internal @string tag;
}

[GoType] partial struct mid {
    internal partial ref ж<leaf> leaf { get; }
}

[GoType] partial struct top {
    internal partial ref mid mid { get; }
    internal @string label;
}

internal static void Main() {
    var h = new holder(leaf: Ꮡ(new leaf(n: 0)), tag: "h"u8);
    h.bump();
    h.bump();
    fmt.Println(h.get(), h.n, h.tag);
    var t = new top(mid: new mid(leaf: Ꮡ(new leaf(n: 10))), label: "t"u8);
    t.bump();
    fmt.Println(t.get(), t.n, t.label);
}
```

`bump` and `get` have pointer receivers, `(l *leaf)`. Each becomes a static method whose receiver is
`this ref leaf l`, so it works on the caller's `leaf` through a C# `ref`. `[GoRecv]` marks such a method,
and asks a source generator for a second overload that takes the box, `ж<leaf>`
([Functions and Methods](#functions-and-methods)). The forwarder for the promoted `h.bump()` calls
`bump` on the `leaf` that `h`'s pointer points to.

**A pointer-receiver method promoted through a value embed is called on the real field.** Go runs
`o.bump(5)` as `(&o.inner).bump(5)`, so the method changes `o`'s own `inner`. The converter writes that
address out at the call site, as `o.of(outer.Ꮡinner).bump(5)`. So the method always works on `o`'s own
`inner` field, never on a copy:

<!-- source: src/tests/Behavioral/EmbeddedValuePointerMethod/main.go:21-76 -->
```go
type inner struct {
	n int
}

func (i *inner) bump(d int) { i.n += d } // pointer-receiver: MUTATES the embedded value
…
type outer struct {
	tag int
	inner
}
…
	o := &outer{tag: 7} // pointer local (newTimer `t := new(timeTimer)` shape)
	o.bump(5)           // promoted: (&o.inner).bump(5)
	o.bump(3)
```
<!-- source: src/tests/Behavioral/EmbeddedValuePointerMethod/main.cs.target:7-69 -->
```csharp
[GoType] partial struct inner {
    internal nint n;
}

[GoRecv] internal static void bump(this ref inner i, nint d) {
    i.n += d;
}
…
[GoType] partial struct outer {
    internal nint tag;
    internal partial ref inner inner { get; }
}
…
    var o = Ꮡ(new outer(tag: 7));
    o.of(outer.Ꮡinner).bump(5);
    o.of(outer.Ꮡinner).bump(3);
```

`o` is a Go pointer, so in C# it holds a box: `Ꮡ(new outer(tag: 7))` is `&outer{tag: 7}`. A name that
starts with `Ꮡ` stands for an address or a field reference
([Names and Glyphs](#reading-converted-code-names-and-glyphs)).

`outer.Ꮡinner` is a static helper the generator adds to `outer` for its `inner` field: it returns a `ref`
to that field. The box's `of` method applies it and returns a `ж<inner>` that points into `o`. So
`o.of(outer.Ꮡinner)` is Go's `&o.inner` ([Pointers](#pointers)).
<!-- Why the converter routes this call itself: docs/ConversionStrategies-Reference/struct-embedding.md,
"A pointer-receiver method promoted through a VALUE embed is routed at the call site, not by a generator
forwarder" (a forwarder body over the value field would copy it and lose the write). The of() surface:
src/core/golib/ж.cs:206-217.
Moved to the reference: inside another pointer-receiver method the receiver is already a C# `ref`, so its
field is addressable and Go's `c.set(1)` becomes `c.flags.set(1)`, with no box (src/tests/Behavioral/
EmbeddedValuePointerMethod/main.go:51-56; main.cs.target:47-52). Pointer-param form: main.go:35 ->
main.cs.target:28-30 (Ꮡo.of(outer.Ꮡinner).bump(100)). The [Implicit Pointer Dereferencing] section shows
`c.alloc(5)` from the same test. -->

**An embedded interface is a plain field.** An interface value is already a reference, so it needs no
`ref` property. A call through it reads as in Go: `r.Interface.Less(j, i)`. When the outer type is used
as that interface, the generator forwards each method the type does not declare itself to that field.

This is Go's `sort.Reverse` pattern. `Interface` is the program's own copy of `sort.Interface`. `reverse`
declares its own `Less`, which swaps the arguments. `Len` and `Swap` are not declared on `reverse`, so
calls to them go to the embedded value:

<!-- source: src/tests/Behavioral/ReverseSortNaNOrder/ReverseSortNaNOrder.go:9-39 -->
```go
type Interface interface {
	Len() int
	Less(i, j int) bool
	Swap(i, j int)
}
…
type reverse struct {
	Interface
}

func (r reverse) Less(i, j int) bool {
	return r.Interface.Less(j, i)
}

func Reverse(data Interface) Interface {
	return &reverse{data}
}
```
<!-- source: src/tests/Behavioral/ReverseSortNaNOrder/ReverseSortNaNOrder.cs.target:8-42 -->
```csharp
[GoType] partial interface Interface {
    nint Len();
    bool Less(nint i, nint j);
    void Swap(nint i, nint j);
}
…
[GoType] partial struct reverse {
    public Interface Interface;
}

internal static bool Less(this reverse r, nint i, nint j) {
    return r.Interface.Less(j, i);
}

public static Interface Reverse(Interface data) {
    return new reverseжInterface(Ꮡ(new reverse(data)));
}
```

A Go interface becomes a C# interface ([Interfaces](#interfaces)). `Reverse` returns the pointer
`&reverse{data}`. In Go, a pointer can call its type's value-receiver methods too, so the pointer
satisfies `Interface`. `reverseжInterface` is the generated adapter class that lets that pointer be used
as an `Interface`: `Less` goes to `reverse`'s own method, and `Len` and `Swap` go to `r.Interface`
([Interfaces](#interfaces)).
<!-- Generator input: src/tests/Behavioral/ReverseSortNaNOrder/package_info.cs:41-43 GoImplement<reverse, Interface>(Pointer = true / Promoted = true). Adapter naming XжI: summary glyph table. A direct call through an embedded interface on a pointer: src/core/os/signal/signal.cs:303 ((~cʗ1).Context.Done()). -->

**Full detail:** [Reference → Struct Type Embedding](ConversionStrategies-Reference/struct-embedding.md#struct-type-embedding) —
how embed members are named, embeds from other packages, nil embedded pointers, and how each kind of
promoted call is routed.

---

## Interfaces

A Go interface becomes a C# `partial interface` marked `[GoType]`. In Go, a type satisfies an interface
just by having its methods. C# needs each type to declare the interfaces it implements. A
[source generator](#source-generators) closes that gap at compile time, so converted code puts values into
interfaces and calls through them as Go does.

What you will notice in converted code: a struct or named number goes into an interface as itself. A
pointer, or a named function type, goes in wrapped in a small generated *adapter* class. A call through an
interface is an ordinary C# interface call.

The examples in this section use these names and glyphs
([Reading Converted Code](#reading-converted-code-names-and-glyphs) lists them all):

- `ж<T>` is Go's pointer `*T`: golib's heap box that holds one `T` (read "zhe"). `Ꮡ(…)` takes an address
  and yields that box ([Pointers](#pointers)).
- `XжI` and `XᴠI` are generated adapter classes: pointer `*X` (`ж`) or value `X` (`ᴠ`) as interface `I`.
- A name ending in `ˢ` is a string literal stored once in a static field
  ([Strings](#strings-string-and-sstring)). Each example shows its declaration.
- `ᐧ` is golib's constant `true`. Passed to a type assertion, it selects the comma-ok form.
- `ᴛ` marks a temporary the converter makes up, such as `exprᴛ1`.
- `[GoType]` marks a converted Go type, and `[GoRecv]` a pointer-receiver method. A generator completes
  both ([Source Generators](#source-generators)).

**An interface keeps its method list, and an embedded interface becomes a C# base interface.** Go's
`string` in a method signature becomes golib's [`@string`](#strings-string-and-sstring), Go's byte string:

<!-- source: src/tests/Behavioral/InterfaceCasting/InterfaceCasting.go:270-276 -->
```go
type rdr interface{ read() string }
type clsr interface{ close() string }

type rdCloser interface {
	rdr
	clsr
}
```
<!-- source: src/tests/Behavioral/InterfaceCasting/InterfaceCasting.cs.target:260-272 -->
```csharp
[GoType] partial interface rdr {
    @string read();
}

[GoType] partial interface clsr {
    @string close();
}

[GoType] partial interface rdCloser :
    rdr,
    clsr
{
}
```

The empty interface, `interface{}` or `any`, becomes C# `any`, an alias for `object`: see
[Empty Interface](#empty-interface-any). Go's built-in `error` is golib's [`error`](../src/core/golib/error.cs)
interface, and it works like any other interface.

**A struct value goes into an interface as itself.** A Go method becomes a C# extension method whose
`this` parameter is the receiver ([Functions and Methods](#functions-and-methods)). The generator adds
`error` to `MyError`'s own declaration and implements `Error()` by calling that extension method. So
returning a `MyError` as an `error` is a plain C# conversion:

<!-- source: src/tests/Behavioral/InterfaceCasting/InterfaceCasting.go:5-16 -->
```go
type MyError struct {
	description string
}

func (err MyError) Error() string {
	return fmt.Sprintf("error: %s", err.description)
}

// error is an interface - MyError is cast to error interface upon return
func f() error {
	return MyError{"foo"}
}
```
<!-- source: src/tests/Behavioral/InterfaceCasting/InterfaceCasting.cs.target:7-17 -->
```csharp
[GoType] partial struct MyError {
    internal @string description;
}

public static @string Error(this MyError err) {
    return fmt.Sprintf("error: %s"u8, err.description);
}

internal static error f() {
    return new MyError("foo"u8);
}
```

`"foo"u8` is a C# UTF-8 literal that becomes an `@string` ([Strings](#strings-string-and-sstring)).
Converting a struct to an interface copies it in C#, just as Go copies a value into an interface.

**A named function type goes in through a value adapter; a named number goes in directly.** The generator
can add an interface only to a C# struct declared in the package it compiles. A named numeric type is such
a struct ([Named Numeric Types](#named-numeric-types-and-constant-contexts)), so it converts directly. A
named function type becomes a C# delegate, and a delegate cannot implement an interface. So the generator
writes a small adapter class that holds the delegate and forwards each interface method to it.

Here `meter` is a function type and `gauge` a named `int`. Both have a `Value` method, so both satisfy
`valued`:

<!-- source: src/tests/Behavioral/LocalValueIfaceCallConversion/main.go:76-120 -->
```go
type meter func() int

func (m meter) Value() int { return m() }

type gauge int

func (g gauge) Value() int { return int(g) * 2 }

type valued interface {
	Value() int
}
…
	mv := valued(meter(func() int { return 11 }))
	gv := valued(gauge(4))
	fmt.Println("func-source:", mv.Value(), "numeric-source:", gv.Value())
```
<!-- source: src/tests/Behavioral/LocalValueIfaceCallConversion/main.cs.target:46-97 -->
```csharp
internal delegate nint meter();

internal static nint Value(this meter m) {
    return m();
}

[GoType("num:nint")] partial struct gauge;

internal static nint Value(this gauge g) {
    return (nint)g * 2;
}

[GoType] partial interface valued {
    nint Value();
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
…
private static readonly object funcSourceˢ = (@string)"func-source:"u8;
private static readonly object numericSourceˢ = (@string)"numeric-source:"u8;
…
    var mv = ((valued)new meterᴠvalued(new meter(() => 11)));
    var gv = ((valued)((gauge)4));
    fmt.Println(funcSourceˢ, mv.Value(), numericSourceˢ, gv.Value());
```

Reading the C#:

- Go's `int` is C# `nint` ([Integer Types](#integer-types-and-arithmetic)).
- Go's conversion `valued(x)` stays a C# cast, `(valued)x`.
- `meterᴠvalued` ("value `meter` as interface `valued`") is the generated adapter. Its `Value()` calls the
  `meter` delegate it holds.
- `gauge` needs no adapter: the generator adds `valued` to the `gauge` struct itself.
- `funcSourceˢ` and `numericSourceˢ` are the printed labels, stored once. They are typed `object` because
  they are only passed to `fmt.Println`'s `any` parameters.

A struct from *another* package also goes in through a value adapter, because the generator cannot add an
interface to a type compiled elsewhere. Its adapter name starts with the package, such as
`typelib_Markᴠstamper`.

**A pointer goes into an interface wrapped in an adapter.** A pointer is golib's generic box class
`ж<Counter>`, so the generator cannot add a user's interface to it. Instead it writes
`CounterжIncrementer` ("pointer `Counter` as interface `Incrementer`"), a small class that holds the box
and implements `Incrementer` by calling the methods on it.

`Counter` is a struct with one `int` field, `n`. Its methods `Inc` and `Total` have pointer receivers, so
only `*Counter`, not `Counter`, satisfies `Incrementer`:

<!-- source: src/tests/Behavioral/InterfaceCasting/InterfaceCasting.go:55-79 -->
```go
type Counter struct {
	n int
}

func addTo(p *int, delta int) {
	*p += delta
}

func (c *Counter) Inc() string {
	addTo(&c.n, 1)
	return "inc"
}

func (c *Counter) Total() int {
	return c.n
}
…
type Incrementer interface {
	Inc() string
	Total() int
}
```
<!-- source: src/tests/Behavioral/InterfaceCasting/InterfaceCasting.cs.target:60-89 -->
```csharp
[GoType] partial struct Counter {
    internal nint n;
}

internal static void addTo(ref nint p, nint delta) {
    p += delta;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string incˢ = "inc"u8;

public static @string Inc(this ж<Counter> Ꮡc) {
    ref var c = ref Ꮡc.DerefOrNull();

    addTo(ref nonnil(ref c).n, 1);
    return incˢ;
}

[GoRecv] public static nint Total(this ref Counter c) {
    return c.n;
}
…
[GoType] partial interface Incrementer {
    @string Inc();
    nint Total();
}
```

The two pointer-receiver methods take two forms ([Functions and Methods](#functions-and-methods)):

- `Inc` takes `&c.n`, so it needs the box itself: `this ж<Counter> Ꮡc`. `DerefOrNull()` binds `c` to the
  struct inside the box. `ref nonnil(ref c).n` is Go's `&c.n`, and `nonnil` panics as Go does if `c` is
  nil ([Pointers](#pointers)).
- `Total` only reads the struct, so it takes `this ref Counter`, with no box. `[GoRecv]` makes the
  generator add an overload that takes `ж<Counter>`, so `Total()` can be called on a box too.

Putting a `*Counter` into an `Incrementer`, then calling through it and asserting it back:

<!-- source: src/tests/Behavioral/InterfaceCasting/InterfaceCasting.go:134-145 -->
```go
c := &Counter{}
var inc Incrementer = c // interface value holds the POINTER
inc.Inc()
inc.Inc()
fmt.Println("via pointer:", c.Total()) // 2 - interface calls mutated the original

c.n = 10
fmt.Println("via interface:", inc.Total()) // 10 - pointer writes visible through interface

back, ok := inc.(*Counter) // assert back to the pointer
back.Inc()
fmt.Println("assert-back:", ok, c.Total(), back == c) // true 11 true
```
<!-- source: src/tests/Behavioral/InterfaceCasting/InterfaceCasting.cs.target:134-165 -->
```csharp
// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object viaPointerˢ = (@string)"via pointer:"u8;
private static readonly object viaInterfaceˢ = (@string)"via interface:"u8;
private static readonly object assertBackˢ = (@string)"assert-back:"u8;
…
var c = Ꮡ(new Counter(nil));
Incrementer inc = new CounterжIncrementer(c);
inc.Inc();
inc.Inc();
fmt.Println(viaPointerˢ, c.Total());
c.Value.n = 10;
fmt.Println(viaInterfaceˢ, inc.Total());
var (back, ok) = inc._<ж<Counter>>(ᐧ);
back.Inc();
fmt.Println(assertBackˢ, ok, c.Total(), back == c);
```

Reading the C#, line by line:

- The `ˢ` fields hold the printed labels, stored once ([Strings](#strings-string-and-sstring)).
- `new Counter(nil)` is the empty literal `Counter{}`: the `nil` argument picks the constructor that makes
  the zero value ([Struct Types](#struct-types)). `Ꮡ(…)` boxes it, so `c` is a `ж<Counter>`.
- `c.Value` is the struct inside the box, so `c.Value.n = 10` is Go's `c.n = 10`.
- Because the adapter holds the box itself, `inc.Inc()` changes the `Counter` that `c` points to, as in Go.
- `inc._<ж<Counter>>(ᐧ)` is the assertion `inc.(*Counter)`, described in the next rule. It returns that
  same box, so `back == c` is true.

**Calls, nil and assertions read as ordinary C#.** A call through an interface is a plain C# interface
call, such as `inc.Inc()`. A nil interface is `default!`, a null reference, so `return nil` becomes
`return default!;` ([Nil and Zero Values](#nil-and-zero-values)). The assertion `x.(T)` becomes golib's
[`x._<T>()`](../src/core/golib/builtin.cs), which panics if the dynamic type is not `T`. Adding the
argument `ᐧ`, golib's constant `true`, selects the comma-ok form that returns `(value, ok)` instead
([Comma-Ok Forms](#multi-result-values-and-comma-ok-forms)). A `switch x.(type)` is covered in
[Type Switch Statements](#type-switch-statements).

**Comparing interface values goes through golib's [`AreEqual`](../src/core/golib/builtin.cs).** Go compares
an interface value by its dynamic type and value. C#'s `==` compares references, and it has no operator
between an interface and a struct that implements it. `AreEqual` does what Go does.

In this example, `errno` is a named `uintptr` with an `Error` method, so it satisfies `error`. Like `gauge`,
it becomes a C# struct and goes into `error` directly
([Named Numeric Types](#named-numeric-types-and-constant-contexts)). The constant `errAgain` becomes a
read-only property ([Constant Values](#constant-values)):

<!-- source: src/tests/Behavioral/InterfaceImplementation/InterfaceImplementation.go:67-101 -->
```go
type errno uintptr

func (e errno) Error() string {
	return "errno"
}

const errAgain errno = 11

func mayFail(n int) error {
	if n > 0 {
		return errAgain
	}
	return nil
}

func checkErr(n int) {
	err := mayFail(n)
	if err == errAgain {
		fmt.Println("got again")
	}
	if err != errAgain {
		fmt.Println("not again")
	}
…
	switch err {
	case errAgain:
		fmt.Println("switch: again")
	case nil:
		fmt.Println("switch: nil")
	default:
		fmt.Println("switch: other")
	}
}
```
<!-- source: src/tests/Behavioral/InterfaceImplementation/InterfaceImplementation.cs.target:55-99 -->
```csharp
[GoType("num:uintptr")] partial struct errno;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string errnoˢ = "errno"u8;

internal static @string Error(this errno e) {
    return errnoˢ;
}

internal static errno errAgain => 11;

internal static error mayFail(nint n) {
    if (n > 0) {
        return errAgain;
    }
    return default!;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object gotAgainˢ = (@string)"got again"u8;
private static readonly object notAgainˢ = (@string)"not again"u8;
private static readonly object switchAgainˢ = (@string)"switch: again"u8;
private static readonly object switchNilˢ = (@string)"switch: nil"u8;
private static readonly object switchOtherˢ = (@string)"switch: other"u8;

internal static void checkErr(nint n) {
    var err = mayFail(n);
    if (AreEqual(err, errAgain)) {
        fmt.Println(gotAgainˢ);
    }
    if (!AreEqual(err, errAgain)) {
        fmt.Println(notAgainˢ);
    }
    var exprᴛ1 = err;
    if (AreEqual(exprᴛ1, errAgain)) {
        fmt.Println(switchAgainˢ);
    }
    else if (AreEqual(exprᴛ1, default!)) {
        fmt.Println(switchNilˢ);
    }
    else { /* default: */
        fmt.Println(switchOtherˢ);
    }

}
```

`==` becomes `AreEqual(…)` and `!=` becomes `!AreEqual(…)`. The `switch err` becomes an `if`/`else if`
chain over a temporary, `exprᴛ1`, and each case compares through `AreEqual` in the same way
([Expression Switch Statements](#expression-switch-statements)).

<!-- adapter unwrap: src/core/golib/builtin.cs:3284-3300 (IInterfaceAdapter.Value, IжAdapter.Box); interface-vs-pointer example: src/tests/Behavioral/InterfaceImplementation/InterfaceImplementation.go:29 `zoo[0] == f` -> InterfaceImplementation.cs.target:35 `AreEqual(zoo[0], f)` -->
`AreEqual` unwraps adapters first, so an interface holding a pointer equals that pointer: Go's
`zoo[0] == f`, where `zoo[0]` is an interface holding the pointer `f`, becomes `AreEqual(zoo[0], f)`.
Comparing two values of a type Go cannot compare, such as a slice or map, panics as it does in Go.

**Methods promoted by embedding satisfy interfaces as they do in Go.** A struct that embeds another type,
or an interface, gains its methods, and the adapters forward to them
([Struct Type Embedding](#struct-type-embedding)).

**Full detail:** [Reference → Interfaces](ConversionStrategies-Reference/interfaces.md#interfaces) — how the converter records which types satisfy which interfaces across packages, adapter naming and accessibility, value adapters for types from other packages, the run-time interface shells, keyword-named methods, and publicized unexported types.

---

## Reflection (`reflect`)

Go's `reflect` package converts like any other package, but its entry points are
[hand-written](#manually-converted-declarations). Go's `reflect` digs into the runtime's memory layout.
The C# version asks .NET for the value's `System.Type` instead, and golib's
[`GoReflect`](../src/core/golib/GoReflect.cs) class answers every question from that type
([The golib Runtime Library](#the-golib-runtime-library)).

A few things a CLR type cannot hold, such as a Go struct tag or a Go type name, travel as attributes the
converter writes on the converted code. In converted code you notice those attributes, such as `[GoTag]`,
and two naming marks, `ᴅ` and `ᴺ`. Every glyph is listed in
[Reading Converted Code](#reading-converted-code-names-and-glyphs).

**`Kind` follows the C# representation.** Each Go kind has its own C# form, so the kind is read off the type.
`nint` is Go `int`, `@string` is Go `string`, and `ж<T>` is golib's heap box for a Go pointer `*T`:

| C# type | `reflect.Kind` |
|---|---|
| `bool`, `nint`, `nuint`, `int8` … `uint64`, `uintptr`, `float32`, `float64`, `complex64`, `complex128` | the matching scalar kind (`nint` is `Int`) |
| `@string`, `slice<T>`, `array<T>`, `map<K, V>`, `channel<T>` | `String`, `Slice`, `Array`, `Map`, `Chan` |
| `ж<T>`, `@unsafe.Pointer` | `Pointer`, `UnsafePointer` |
| a delegate | `Func` |
| `object` (Go `any`) or a C# interface | `Interface` |
| a `[GoType]` struct | `Struct` |
| a named type, such as `[GoType("num:nint")] partial struct counter` (Go `type counter int`) | its underlying kind, here `Int` |

The `@` prefix lets a C# keyword, such as `string` or `unsafe`, serve as a name. `[GoType]` marks a type
converted from Go, and a [source generator](#source-generators) completes it. Its argument, when present,
is the Go type's underlying type; `num:` marks a numeric one
([Named Numeric Types](#named-numeric-types-and-constant-contexts)). The [Pointers](#pointers) and
[`unsafe.Pointer`](#unsafepointer-and-uintptr) sections explain the pointer rows.

**A struct tag is copied verbatim into `[GoTag]`.** A C# field has no place for a Go struct tag, so the
converter writes the tag as an attribute on the field. An untagged field gets no attribute. The tag is a C#
verbatim string, `@"…"`, in which `""` stands for one quote character:

<!-- source: src/tests/Behavioral/ReflectStructTagCopy/main.go:30-35 -->
```go
type record struct {
	Version  int
	Name     string `json:"name" asn1:"optional,explicit,tag:0"`
	Data     []byte `json:"data,omitempty"`
	Untagged bool
}
```
<!-- source: src/tests/Behavioral/ReflectStructTagCopy/main.cs.target:8-15 -->
```csharp
[GoType] partial struct record {
    public nint Version;
    [GoTag(@"json:""name"" asn1:""optional,explicit,tag:0""")]
    public @string Name;
    [GoTag(@"json:""data,omitempty""")]
    public slice<byte> Data;
    public bool Untagged;
}
```

**`StructField.Tag` reads the attribute back.** So reflection code, and the standard library's tag-driven
encoders such as `encoding/json` and `encoding/asn1`, see the same tag Go does:

<!-- source: src/tests/Behavioral/ReflectStructTagCopy/main.go:71-150 -->
```go
func main() {
	t := reflect.TypeOf(record{})

	for i := 0; i < t.NumField(); i++ {
		f := t.Field(i)
		fmt.Printf("%s raw=%q json=%q asn1=%q\n", f.Name, string(f.Tag), f.Tag.Get("json"), f.Tag.Get("asn1"))
	}

	// Lookup distinguishes an absent key from one explicitly set to the empty string.
	f := t.Field(1)
	v, ok := f.Tag.Lookup("json")
	fmt.Println("lookup json:", v, ok)
	v, ok = f.Tag.Lookup("missing")
	fmt.Println("lookup missing:", v, ok)
…
}
```
<!-- source: src/tests/Behavioral/ReflectStructTagCopy/main.cs.target:49-111 -->
```csharp
// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string jsonˢ = "json"u8;
private static readonly @string asn1ˢ = "asn1"u8;
private static readonly object lookupJsonˢ = (@string)"lookup json:"u8;
private static readonly @string missingˢ = "missing"u8;
private static readonly object lookupMissingˢ = (@string)"lookup missing:"u8;
…
internal static void Main() {
    var t = reflect.TypeOf(new record(nil));
    for (nint i = 0; i < t.NumField(); i++) {
        var fΔ1 = t.Field(i);
        fmt.Printf("%s raw=%q json=%q asn1=%q\n"u8, fΔ1.Name, ((@string)fΔ1.Tag), fΔ1.Tag.Get(jsonˢ), fΔ1.Tag.Get(asn1ˢ));
    }
    var f = t.Field(1);
    var (v, ok) = f.Tag.Lookup(jsonˢ);
    fmt.Println(lookupJsonˢ, v, ok);
    (v, ok) = f.Tag.Lookup(missingˢ);
    fmt.Println(lookupMissingˢ, v, ok);
…
}
```

A few names in that C# need a word:

- `jsonˢ`, `missingˢ` and the other `ˢ` names are Go's string literals, stored once in static fields. A
  literal used only as an `fmt.Println` argument, which is Go `any`, is stored already boxed as an `object`
  ([Strings](#strings-string-and-sstring), [Empty Interface](#empty-interface-any)).
- `"…"u8` is a C# UTF-8 literal, which golib reads as a Go string.
- `new record(nil)` is Go's empty literal `record{}`. It calls the constructor the generator adds to every
  struct for the zero value ([Struct Types](#struct-types)).
- `fΔ1` is the loop's `f`. It is renamed because the function declares another `f` after the loop, and C#
  does not allow an inner block to reuse a name its enclosing block declares
  ([Shadowing](#short-variable-redeclaration-shadowing)).
- `(@string)fΔ1.Tag` is Go's conversion `string(f.Tag)`, and `var (v, ok) = …` is Go's two-result
  assignment ([Comma-Ok Forms](#multi-result-values-and-comma-ok-forms)).

**Other attributes carry what a CLR type cannot hold.** For example, golib's `array<T>` does not hold its
length in its type, so `[GoArrayDims]` records it wherever `reflect` may need it:

<!-- source: src/tests/Behavioral/AddressOfParamWrite/main.go:62 -->
```go
func bumpParamElem(a [3]int) [3]int {
```
<!-- source: src/tests/Behavioral/AddressOfParamWrite/main.cs.target:43 -->
```csharp
internal static array<nint> bumpParamElem([GoArrayDims(3)] array<nint> a) {
```

Similar attributes record a channel direction, an embedded field and the Go name of a type declared inside
a function. The reference lists them.

**A type defined over an existing interface gets a descriptor carrier, marked `ᴅ`.** Go's `type eface any`
and `type namedIface fmt.Stringer` define new types that just reuse an existing interface. The converter
writes each as a C# `global using` alias, another name for `object` or for `Stringer`. The alias keeps
Go's rule that any fitting value can be assigned without a conversion. A type declared with its own
`interface { … }` list becomes a real C# interface instead.

An alias leaves nothing in compiled code, so `reflect` alone could not recover the name `eface`. The
converter therefore also emits an empty interface named with a `ᴅ` suffix, the *carrier*. Nothing ever
implements it; its `[GoLocalName]` only holds the Go name. A struct field of the aliased type points
`reflect` at the carrier with `[GoDescriptorType(Self = …)]`, where `Self` names the field's own type.
The comments in the test name the two shapes that gain a carrier, class (i) and (ii), and the controls
that must not:

<!-- source: src/tests/Behavioral/DescriptorCarrierFieldName/main.go:23-45 -->
```go
type eface any               // class (i): defined over the empty interface
type namedIface fmt.Stringer // class (ii): defined over a named NON-empty interface
type realIface interface {   // control: an inline definition is a real C# interface already
	Do()
}
type aliasIface = fmt.Stringer // control: a true Go alias must NOT gain a carrier

type holder struct {
	E eface
	N namedIface
	R realIface
	A aliasIface
}

func main() {
	t := reflect.TypeFor[holder]()

	for i := 0; i < t.NumField(); i++ {
		f := t.Field(i)
		fmt.Printf("%s Name=%q String=%q PkgPath=%q Kind=%v\n",
			f.Name, f.Type.Name(), f.Type.String(), f.Type.PkgPath(), f.Type.Kind())
	}
}
```
<!-- source: src/tests/Behavioral/DescriptorCarrierFieldName/main.cs.target:1-38 -->
```csharp
global using eface = object;
global using namedIface = go.fmt_package.Stringer;
global using aliasIface = go.fmt_package.Stringer;
…
// Descriptor carrier for `eface` — uninhabited; see GoDescriptorTypeAttribute.
[GoLocalName("eface")] internal interface efaceᴅ { }

// Descriptor carrier for `namedIface` — uninhabited; see GoDescriptorTypeAttribute.
[GoLocalName("namedIface")] internal interface namedIfaceᴅ { }


[GoType] public partial interface realIface {
    void Do();
}

[GoType] partial struct holder {
    [GoDescriptorType(Self = typeof(efaceᴅ))]
    public eface E;
    [GoDescriptorType(Self = typeof(namedIfaceᴅ))]
    public namedIface N;
    public realIface R;
    public aliasIface A;
}

internal static void Main() {
    var t = reflect.TypeFor<holder>();
    for (nint i = 0; i < t.NumField(); i++) {
        var f = t.Field(i);
        fmt.Printf("%s Name=%q String=%q PkgPath=%q Kind=%v\n"u8,
            f.Name, f.Type.Name(), f.Type.String(), f.Type.PkgPath(), f.Type.Kind());
    }
}
```

So, as in Go, the program reports field `E`'s type name as `eface` and field `N`'s as `namedIface`.
"Uninhabited" in the generated comment means no value ever has the carrier type. The other two fields
need no carrier. `realIface` is a real C# interface with its own name. `aliasIface` is a true Go alias
(`=`), and Go itself reports the target's name, `Stringer` ([Type Aliasing](#type-aliasing)).

**A generic function that passes its type parameter to `reflect.TypeFor` gains a companion, marked `ᴺ`.**
A generic type argument cannot carry an attribute, and `eface` and `any` are both `object` to the CLR. So
`nameOf[eface]` and `nameOf[any]` would be one C# instantiation, and `reflect` could not tell them apart.
Such a function gets a second type parameter, `Tᴺ`, and asks `reflect` about that one. `T` stays for every
other use of the type parameter:

<!-- source: src/tests/Behavioral/GenericTypeNameCompanion/main.go:36-48 -->
```go
type eface any                 // class (i): defined over the empty interface
type namedIface fmt.Stringer   // class (ii): defined over a named NON-empty interface
type aliasIface = fmt.Stringer // control: a true Go alias must NOT gain a carrier
type ordinary string           // control: an ordinary defined type
type inlineIface interface {   // control: an inline definition is a real C# interface already
	Do()
}

// nameOf reads a Go NAME off a TYPE PARAMETER — the shape that gains a companion.
func nameOf[T any](label string) {
	t := reflect.TypeFor[T]()
	fmt.Printf("%-11s Name=%q String=%q PkgPath=%q Kind=%v\n", label, t.Name(), t.String(), t.PkgPath(), t.Kind())
}
```
<!-- source: src/tests/Behavioral/GenericTypeNameCompanion/main.cs.target:1-27 -->
```csharp
global using eface = object;
global using namedIface = go.fmt_package.Stringer;
global using aliasIface = go.fmt_package.Stringer;
…
// Descriptor carrier for `eface` — uninhabited; see GoDescriptorTypeAttribute.
[GoLocalName("eface")] internal interface efaceᴅ { }

// Descriptor carrier for `namedIface` — uninhabited; see GoDescriptorTypeAttribute.
[GoLocalName("namedIface")] internal interface namedIfaceᴅ { }


[GoType("@string")] partial struct ordinary;

[GoType] partial interface inlineIface {
    void Do();
}

internal static void nameOf<T, Tᴺ>(@string label) {
    var t = reflect.TypeFor<Tᴺ>();
    fmt.Printf("%-11s Name=%q String=%q PkgPath=%q Kind=%v\n"u8, label, t.Name(), t.String(), t.PkgPath(), t.Kind());
}
```

**Each call site fills in the companion.** It passes the `ᴅ` carrier as `Tᴺ` when the type argument is a
Go type defined over an existing interface, such as `eface`. Otherwise it passes the type argument itself,
including a true Go alias such as `aliasIface`. In C#, `any` is a global alias for `object` that each
converted project file declares ([Empty Interface](#empty-interface-any)), and each `ˢ` name is a string
literal stored once in a static field ([Strings](#strings-string-and-sstring)):

<!-- source: src/tests/Behavioral/GenericTypeNameCompanion/main.go:57-67 -->
```go
func main() {
	nameOf[eface]("eface")
	nameOf[namedIface]("namedIface")
	nameOf[any]("any")
	nameOf[aliasIface]("aliasIface")
	nameOf[inlineIface]("inlineIface")
	nameOf[ordinary]("ordinary")
…
}
```
<!-- source: src/tests/Behavioral/GenericTypeNameCompanion/main.cs.target:33-52 -->
```csharp
// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string efaceˢ = "eface"u8;
private static readonly @string namedIfaceˢ = "namedIface"u8;
private static readonly @string anyˢ = "any"u8;
private static readonly @string aliasIfaceˢ = "aliasIface"u8;
private static readonly @string inlineIfaceˢ = "inlineIface"u8;
private static readonly @string ordinaryˢ = "ordinary"u8;
…
internal static void Main() {
    nameOf<eface, efaceᴅ>(efaceˢ);
    nameOf<namedIface, namedIfaceᴅ>(namedIfaceˢ);
    nameOf<any, any>(anyˢ);
    nameOf<aliasIface, aliasIface>(aliasIfaceˢ);
    nameOf<inlineIface, inlineIface>(inlineIfaceˢ);
    nameOf<ordinary, ordinary>(ordinaryˢ);
…
}
```

With the companion, `nameOf[eface]` prints `Name="eface"` and `nameOf[any]` prints `Name=""`, as in Go.
Any other generic function, including one that calls `reflect.TypeOf` on a value, keeps its declared type
parameters ([Generics](#generics)).

**Types built at run time are real CLR types.** `SliceOf`, `MapOf`, `ChanOf` and `PointerTo` return the
matching golib generic type, such as `slice<T>`, and `StructOf` emits a new .NET value type. So code that
receives such a type treats it like a converted one.

**Full detail:** [Reference → Manually-Converted Declarations: the reflection bridge](ConversionStrategies-Reference/manual-conversions.md#structfieldtag-is-a-real-read--the-converter-has-always-emitted-the-tag-nothing-had-ever-read-it) — the bridge's rules one by one, from tag reads and type names to assignability, the array-length and channel-direction attributes, and the run-time type constructors.

---

## Pointers

Go's `*T` becomes golib [`ж<T>`](../src/core/golib/%D0%B6.cs) (read "zhe"): a small heap object, a box, that
holds or points at one `T`. [golib](#the-golib-runtime-library) is the hand-written C# runtime library that
converted code uses. A pointer that points nowhere is written `nil` when code compares with it, and `default!`
when it is a zero value, as in a declaration or a `return nil` ([Nil and Zero Values](#nil-and-zero-values)).

The glyph `Ꮡ` (the Cherokee letter "su", U+13D1) marks an address. `Ꮡ(…)` makes a pointer, and a name such
as `Ꮡa` is the pointer to `a`. The [glyph table](#reading-converted-code-names-and-glyphs) lists every glyph.
At a glance:

| Go | C# | What it does |
|---|---|---|
| `*T` | `ж<T>` | The pointer type. `**T` is `ж<ж<T>>`. |
| `&x` | `Ꮡx` | The address of a local that lives in a box. |
| `&T{…}` | `Ꮡ(new T(…))` | Allocates a value and points at it. |
| `new(T)` | `@new<T>()` | Allocates a zero value and points at it. |
| `*p`, read | `~p` or `p.Value` | Reads the value; panics on nil. |
| `*p = v` | `p.Value = v` | Writes the real storage. |
| `&s[i]` | `Ꮡ(s, i)` | The address of a slice element. |
| `&a[i]` | `Ꮡa.at<T>(i)` | The address of an array element. |
| `&x.f` | `Ꮡx.of(T.Ꮡf)` | The address of a struct field. |

<!-- table sources, in row order (paths under src/tests/Behavioral/): PointerToPointer/PointerToPointer.cs.target:19-20 (ж<ж<nint>>); PointerToPointer.cs.target:22 (ptr = Ꮡa); IncDecPointerField/main.cs.target:21 (Ꮡ(new counter(n: 5))); PointerToArrayElementAddress/main.cs.target:20 (@new<grid>()); IncDecPointerField/main.cs.target:27 ((~@base).n) and PointerToPointer.cs.target:28 (ptr.Value for a whole-value *ptr read, Go PointerToPointer.go:35); SlicePointerIdentity/main.cs.target:59 (p.Value = 42); SlicePointerIdentity/main.cs.target:58 (Ꮡ(s, 1)); PointerToArrayElementAddress/main.cs.target:22 and SlicePointerIdentity/main.cs.target:53 (.at<nint>(1)); PointerToPointer.cs.target:40 (Ꮡb.of(Buffer.Ꮡoff)). `~` is ж<T>'s operator ~, src/core/golib/ж.cs:665 (returns ValueSlot, throws the nil-dereference panic); `.Value` performs the same nil check (ж.cs:105-119, ValueSlot is "identical to Value except it never throws"). box nil compare: src/tests/Behavioral/NilPointerParamMethods/main.cs.target:17; `return nil` -> `return default!`: same file :20 -->

**An address-taken local has two names for one storage.** Go moves a local to the heap when its address
may outlive the function. A C# `ref` local cannot do that, so golib's `heap(…)` allocates the local in a box.
`a` is a C# `ref` alias for ordinary reads and writes, and `Ꮡa` is the box, used wherever Go writes `&a`:

<!-- source: src/tests/Behavioral/PointerToPointer/PointerToPointer.go:17-29 -->
```go
	var a int
	var ptr *int
	var pptr **int
	var ppptr ***int

	a = 3000

	/* take the address of var */
	ptr = &a

	/* take the address of ptr using address of operator & */
	pptr = &ptr
	ppptr = &pptr
```
<!-- source: src/tests/Behavioral/PointerToPointer/PointerToPointer.cs.target:17-24 -->
```csharp
    ref var a = ref heap(new nint(), out var Ꮡa);
    ref var ptr = ref heap<ж<nint>>(out var Ꮡptr);
    ref var pptr = ref heap<ж<ж<nint>>>(out var Ꮡpptr);
    ж<ж<ж<nint>>> ppptr = default!;
    a = 3000;
    ptr = Ꮡa;
    pptr = Ꮡptr;
    ppptr = Ꮡpptr;
```

`a`, `ptr` and `pptr` have their address taken, so each gets a box. `heap(value, out var Ꮡa)` boxes a
starting value, and `heap<T>(out var Ꮡptr)` boxes the zero value of `T`. Both forms give the same pair of
names.

`ppptr` never has its address taken, so it stays a plain local, and `default!` is its nil zero value. A pointer
to a pointer is a box that holds a box. Go's `int` is `nint`
([Integer Types and Arithmetic](#integer-types-and-arithmetic)).

A local that a closure, a `go` statement or a `defer` statement shares with its caller can also get this
form, so both sides see one variable. [Function Values and Closures](#function-values-and-closures)
covers captures. <!-- other routes to the box: `&a`, a method that keeps its receiver's address, a pointer-method value: src/go2cs/escapeAnalysisOperations.go:1143, 1150. rule: src/go2cs/escapeAnalysisOperations.go:1127-1129 (`escapes = (closureContainsIdent && !isValueType(...)) || takesAddress || usedAsRef`; isValueType is true only for basic types, variableAnalysisOperations.go:2834-2843); go/defer arms: escapeAnalysisOperations.go:982, 1026; written-after-capture picks box versus snapshot routing: variableAnalysisOperations.go:2092-2097. reference-shaped types: escapeAnalysisOperations.go:2036-2044, 1852-1866; probeI1 main.go:152 -> main.cs.target:162. Examples in src/tests/Behavioral/ClosureWriteVisibility: probeA1 boxed and written through the box, main.cs.target:18-22 (`ref var t = ref heap<Tally>(out var Ꮡt);` … `Ꮡt.Value.total += 100;`); probeA3 boxed but read-only after all writes, so the closure takes a snapshot, main.cs.target:37-41 (`var tʗ1 = t;`); probeN1 captured int stays plain, main.go:193-197 -> main.cs.target:210-215 (`nint n = 0; void inc() { n++; }`). Reference: pointers.md:311. heap overloads: src/core/golib/builtin.cs:2298 (zero value) and :2322 (starting value). -->

**`&T{…}` and `new(T)` allocate a box directly.** `Ꮡ(…)` boxes a new value, and golib's `@new<T>()` boxes
a zero value. Here `base` and `c` are pointer locals, not boxed variables, so they have one name each:

<!-- source: src/tests/Behavioral/IncDecPointerField/main.go:10-31 -->
```go
type inner struct{ k int }

type counter struct {
	n   int
	sub inner
}

…
func get(c *counter) *counter { return c }

func main() {
	base := &counter{n: 5}
	base.sub.k = 3
	c := get(base) // c is a *counter LOCAL (heap box, not a ref-aliased parameter)

	c.n++     // direct field ++ through the pointer local
	c.n++     // 7
	c.sub.k-- // nested field -- through the pointer local

	// Read back through the original pointer to confirm the writes reached real storage.
	fmt.Println(base.n, base.sub.k) // 7 2
}
```
<!-- source: src/tests/Behavioral/IncDecPointerField/main.cs.target:7-28 -->
```csharp
[GoType] partial struct inner {
    internal nint k;
}

[GoType] partial struct counter {
    internal nint n;
    internal inner sub;
}

internal static ж<counter> get(ж<counter> Ꮡc) {
    return Ꮡc;
}

internal static void Main() {
    var @base = Ꮡ(new counter(n: 5));
    @base.Value.sub.k = 3;
    var c = get(@base);
    c.Value.n++;
    c.Value.n++;
    c.Value.sub.k--;
    fmt.Println((~@base).n, (~@base).sub.k);
}
```

`[GoType]` marks a converted Go type ([Struct Types](#struct-types)). `base` is a C# keyword, so it is
[escaped](#reading-converted-code-names-and-glyphs) as `@base`. `get` returns its pointer as it is, so its
parameter stays a plain box; the pointer-parameter rule in this section says when a parameter gets more.

**A read through a pointer uses `~p` or `p.Value`; a write uses `p.Value`.** `~` is an operator golib
defines on `ж<T>`, standing in for Go's `*`. `~p` gives a copy of the value, so it is the form for a field
read such as `(~@base).n`. `p.Value` gives a C# `ref` to the real storage, so it is the form for a write,
`++` or `--`.

Both forms panic on nil, like Go's `*p`. A plain whole-value read can also appear as `p.Value`: in the
pointer-to-pointer program, `fmt.Printf(…, *ptr)` becomes `fmt.Printf(…u8, ptr.Value)`.
[Implicit Pointer Dereferencing](#implicit-pointer-dereferencing) says where each form appears. <!-- PointerToPointer.go:35 -> PointerToPointer.cs.target:28; `~` rvalue vs `.Value` assignable: IncDecPointerField/main.go:1-5 (`(~mp).ncgocall++` would be an rvalue increment) -->

**An element or field address points into the real storage.** `Ꮡ(s, i)` is the address of slice element
`i`, so a write through it lands in the slice itself, and every slice that shares that storage sees it:

<!-- source: src/tests/Behavioral/SlicePointerIdentity/main.go:15-53 -->
```go
	s := make([]int, 4, 8)
…
	t := s[1:]
…
	p := &s[1]
	*p = 42
	fmt.Println("write thru :", s[1], t[0])
```
<!-- source: src/tests/Behavioral/SlicePointerIdentity/main.cs.target:24-60 -->
```csharp
private static readonly object writeThruˢ = (@string)"write thru :"u8;
…
    var s = new slice<nint>(4, 8);
…
    var t = s[1..];
…
    var p = Ꮡ(s, 1);
    p.Value = 42;
    fmt.Println(writeThruˢ, s[1], t[0]);
```

Both `s[1]` and `t[0]` print 42. `slice<T>` is golib's Go slice ([Slices and Arrays](#slices-and-arrays)).
`writeThruˢ` is the Go string literal `"write thru :"`, stored once in a static field. `"…"u8` is a C#
UTF-8 literal, and `@string` is golib's Go string. The field is typed `object` because `fmt.Println` takes
`any` arguments ([Strings](#strings-string-and-sstring), [Empty Interface](#empty-interface-any)).

An array element's address, `&a[i]`, uses `.at<T>(i)` on the array's pointer. Here `g` is a pointer to an
array, made by `new(grid)`, and `&g[j]` points into it:

<!-- source: src/tests/Behavioral/PointerToArrayElementAddress/main.go:11-30 -->
```go
type row [4]uint32

type grid [3]row
…
	g := new(grid)
	for j := 0; j < len(g); j++ {
		populate(&g[j], uint32(j*10))
	}
```
<!-- source: src/tests/Behavioral/PointerToArrayElementAddress/main.cs.target:7-23 -->
```csharp
[GoType("[4]uint32")] partial struct row;

[GoType("[3]row")] partial struct grid;
…
    var g = @new<grid>();
    for (nint j = 0; j < 3; j++) {
        populate(g.at<row>(j), (uint32)(j * 10));
    }
```

`populate` takes a `*row` and writes through it, so its writes land in `g`'s rows. `len` of an array is a
constant, so `len(g)` is written `3` ([Built-in Functions](#built-in-functions)). For an array local `a`
that lives in a box, the same address is `Ꮡa.at<T>(i)`. A field's address, `&x.f`, is `Ꮡx.of(T.Ꮡf)`; the
pointer-receiver rule in this section shows that form. <!-- array element: PointerToArrayElementAddress/main.go:29 -> main.cs.target:22; populate: main.go:18-22 -> main.cs.target:11-17; boxed array local: SlicePointerIdentity/main.cs.target:51-53 `Ꮡa.at<nint>(1)`; field: src/tests/Behavioral/ReceiverFieldAddress/main.go:17 -> main.cs.target:23; field-reference accessors such as Counter.Ꮡn come from the struct template, src/gen/go2cs-gen/Templates/StructType/StructTypeTemplate.cs:61 ({{FieldReferences}}) -->

**A pointer parameter keeps the box and binds a `ref` alias.** The box parameter is named with the `Ꮡ`
prefix. When the body dereferences it, the first line binds the Go name to the value with `DerefOrNull()`,
so the body reads like Go. A comparison with `nil` uses the box:

<!-- source: src/tests/Behavioral/NilPointerParamMethods/main.go:21-58 -->
```go
var errNilArg = errors.New("nil argument")
…
func checkArg(p *node, op string) error {
	if p == nil {
		return errNilArg
	}
	return nil
}
…
func readName(p *node) string {
	_ = checkArg(p, "readname")
	return p.name
}
```
<!-- source: src/tests/Behavioral/NilPointerParamMethods/main.cs.target:8-49 -->
```csharp
internal static error errNilArg = errors.New("nil argument"u8);
…
internal static error checkArg(ж<node> Ꮡp, @string op) {
    if (Ꮡp == nil) {
        return errNilArg;
    }
    return default!;
}
…
private static readonly @string readnameˢ = "readname"u8;

internal static @string readName(ж<node> Ꮡp) {
    ref var p = ref Ꮡp.DerefOrNull();

    _ = checkArg(Ꮡp, readnameˢ);
    return p.name;
}
```

`node` is a struct with a `name` field. `checkArg` never dereferences `p`, so it binds no alias, and its
`return nil` is the zero value `default!`. `readnameˢ` is the string literal `"readname"`, stored once in a
static field ([Strings](#strings-string-and-sstring)).

For a nil box, `DerefOrNull()` returns an empty C# `ref` instead of failing. So `readName(nil)` still runs
its first line, and panics with Go's nil-dereference message at `p.name`, just where Go does.
[Implicit Pointer Dereferencing](#implicit-pointer-dereferencing) has more on the alias. <!-- DerefOrNull returns Unsafe.NullRef<T>() for a nil box: src/core/golib/ж.PointerExtensions.cs:445; the NullReferenceException at first use maps to "invalid memory address or nil pointer dereference". box-only parameter, no alias: src/tests/Behavioral/IncDecPointerField/main.cs.target:16-18 -->

**An unexported package-level function that only dereferences a pointer parameter takes `ref T`.** A C#
`ref` parameter needs no box. Every caller of an unexported function is in the same package, so every call
site can change with it: `&x` becomes `ref x`. A local whose address only feeds such calls stays a plain
local:

<!-- source: src/tests/Behavioral/RefLoweredParams/main.go:16-96 -->
```go
func addTo(out *uint64, v uint64) {
	*out += v
}
…
	var total uint64
	addTo(&total, 5)
	addTo(&total, 7)
	fmt.Println("total:", total)
```
<!-- source: src/tests/Behavioral/RefLoweredParams/main.cs.target:11-111 -->
```csharp
internal static void addTo(ref uint64 @out, uint64 v) {
    @out += v;
}
…
private static readonly object totalˢ = (@string)"total:"u8;
…
    uint64 total = default!;
    addTo(ref total, 5);
    addTo(ref total, 7);
    fmt.Println(totalˢ, total);
```

`@out` is Go's `out`, escaped because it is a C# keyword. `totalˢ` is the string literal `"total:"`, stored
once in a static field and typed `object` because `fmt.Println` takes `any` arguments.

A `defer` or `go` call to such a function works too. The statement stores the address as a box when it
runs, and turns it into a `ref` only when the call itself runs, so the call sees the variable's latest value
([Defer / Panic / Recover](#defer--panic--recover), [Goroutines](#goroutines)). <!-- signature `internal static void addTo(ref uint64 @out, uint64 v)` at src/tests/Behavioral/RefLoweredParams/main.cs.target:11 (Go: main.go:16). unexported-only: docs/ConversionStrategies-Reference/pointers.md:159, :171 (exported functions, func-value uses and others keep the box; boxedBump used as a func value keeps ж<uint64>: main.cs.target:33-37). nonnil at a lowered call: RefLoweredParams/main.go:23 `addTo(&v.x, k)` -> main.cs.target:16 `addTo(ref nonnil(ref v).x, k);`; golib builtin.cs:2358. Stdlib instance: src/core/crypto/internal/fips140/nistec/fiat/p224.cs:127. defer keeps the box: main.go:73 `defer printVal(&x)` -> main.cs.target:68 `defer(ᴛ1 => printVal(ref ᴛ1.DerefOrNull()), Ꮡx, ref ᒐ);`; go: main.go:85 -> main.cs.target:92; reference: docs/ConversionStrategies-Reference/pointers.md:186, :190 -->

**A pointer receiver is `this ref T`, or the box `this ж<T>` when the method needs the pointer itself.**
Methods are C# extension methods, so the receiver is the first parameter
([Functions and Methods](#functions-and-methods)). A method that only reads and writes through its receiver
takes a C# `ref` to the caller's storage. A method that returns or compares its receiver, or takes a field's
address, takes the box:

<!-- source: src/tests/Behavioral/ReceiverFieldAddress/main.go:7-18 -->
```go
type Counter struct {
	n int32
}

func bump(p *int32, delta int32) int32 {
	*p += delta
	return *p
}

func (c *Counter) Add(delta int32) int32 { return bump(&c.n, delta) }
func (c *Counter) Set(v int32)           { *(&c.n) = v }
func (c *Counter) Get() int32            { return c.n }
```
<!-- source: src/tests/Behavioral/ReceiverFieldAddress/main.cs.target:7-28 -->
```csharp
[GoType] partial struct Counter {
    internal int32 n;
}

internal static int32 bump(ref int32 p, int32 delta) {
    p += delta;
    return p;
}

public static int32 Add(this ж<Counter> Ꮡc, int32 delta) {
    ref var c = ref Ꮡc.DerefOrNull();

    return bump(ref nonnil(ref c).n, delta);
}

public static void Set(this ж<Counter> Ꮡc, int32 v) {
    (Ꮡc.of(Counter.Ꮡn)).Value = v;
}

[GoRecv] public static int32 Get(this ref Counter c) {
    return c.n;
}
```

`Get` only reads, so it takes `this ref Counter`. `[GoRecv]` asks a [source generator](#source-generators)
to add a matching overload that takes `ж<Counter>`, so `Get` can also be called through a pointer.

`Set` and `Add` take `&c.n`, so they take the box. `Ꮡc.of(Counter.Ꮡn)` is `&c.n`, where `Counter.Ꮡn` is a
field reference the generator adds to the struct. `bump` only dereferences its pointer, so it takes
`ref int32`. `nonnil(ref c)` is golib's nil check on the alias: if the receiver is nil, it raises Go's nil
panic at `&c.n`, where Go raises it.

The caller's `var c Counter` is a value, and Go takes `&c` to call `Set` and `Add`. So `c` lives in a box, and
the calls go through `Ꮡc`, while `c.Get()` uses the `ref` alias:

<!-- source: src/tests/Behavioral/ReceiverFieldAddress/main.go:20-27 -->
```go
func main() {
	var c Counter // value receiver; pointer-receiver methods called on it
	c.Set(100)
	fmt.Println("after Set:", c.Get())   // 100
	fmt.Println("Add 10:", c.Add(10))    // 110
	fmt.Println("Add 5:", c.Add(5))      // 115
	fmt.Println("final:", c.Get())       // 115
}
```
<!-- source: src/tests/Behavioral/ReceiverFieldAddress/main.cs.target:30-43 -->
```csharp
// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object afterSetˢ = (@string)"after Set:"u8;
private static readonly object add10ˢ = (@string)"Add 10:"u8;
private static readonly object add5ˢ = (@string)"Add 5:"u8;
private static readonly object finalˢ = (@string)"final:"u8;

internal static void Main() {
    ref var c = ref heap(new Counter(), out var Ꮡc);
    Ꮡc.Set(100);
    fmt.Println(afterSetˢ, c.Get());
    fmt.Println(add10ˢ, Ꮡc.Add(10));
    fmt.Println(add5ˢ, Ꮡc.Add(5));
    fmt.Println(finalˢ, c.Get());
}
```

The four `ˢ` fields are the program's string literals, each stored once
([Strings](#strings-string-and-sstring)). <!-- Counter: src/tests/Behavioral/ReceiverFieldAddress/main.go:7-9 -> main.cs.target:7-9; nonnil: src/core/golib/builtin.cs:2358 (throws the same NilPointerDereference panic ж<T>.Value raises) -->

**A conversion between pointer types with the same underlying type shares the storage.** golib's
`Reinterpret<T, U>()` returns a view of the same box, not a copy. A write through either pointer shows
through the other:

<!-- source: src/tests/Behavioral/NamedNumericPointerReinterpret/main.go:62-98 -->
```go
type coord struct{ X, Y int }
type point coord
…
func bump(c *coord) {
	p := (*point)(c)
	p.X, p.Y = 3, 4
}
…
	xy := coord{1, 2}
	bump(&xy)
	fmt.Println(xy.X, xy.Y) // 3 4
```
<!-- source: src/tests/Behavioral/NamedNumericPointerReinterpret/main.cs.target:53-92 -->
```csharp
[GoType] partial struct coord {
    public nint X, Y;
}

[GoType("coord")] partial struct point;

internal static void bump(ж<coord> Ꮡc) {
    var p = Ꮡc.Reinterpret<coord, point>();
    (p.Value.X, p.Value.Y) = (3, 4);
}
…
    ref var xy = ref heap<coord>(out var Ꮡxy);
    xy = new coord(1, 2);
    bump(Ꮡxy);
    fmt.Println(xy.X, xy.Y);
```

`[GoType("coord")]` declares `point` as a new type over `coord`, and a
[source generator](#source-generators) fills in its body ([Struct Types](#struct-types)). `xy` is boxed at
its zero value and then assigned, because `bump(&xy)` needs its address. The program prints `3 4`. <!-- stdlib instance: GOROOT/src/flag/flag.go:130 `return (*boolValue)(p)` -> src/core/flag/flag.cs:133 `return Ꮡp.Reinterpret<bool, boolValue>();` -->

**Pointers compare by address.** Two pointers are equal when they point to the same storage, even when
they are taken through different slices: `Ꮡ(t, 0) == Ꮡ(s, 1)` is true when `t` is `s[1:]`. So a pointer works
as a [map](#maps) key. Raw addresses are covered in [`unsafe.Pointer` and `uintptr`](#unsafepointer-and-uintptr). <!-- equality: src/tests/Behavioral/SlicePointerIdentity/main.go:26-27 `t := s[1:]` / `&t[0] == &s[1]` -> main.cs.target:43-44 `Ꮡ(t, 0) == Ꮡ(s, 1)`; the whole file main.go:20-69 (element pointers as map keys: main.go:56-59 -> main.cs.target:61-64 `new map<ж<nint>, @string>{}`). pinning: golib pins the storage only where it is pinnable, src/core/golib/ж.cs:62 (PointerStorage: None = order token, Unpinnable = correct when taken but may move, Pinnable = pinned), ж.cs:547 (EnsureStableAddress), ж.cs:979 (uintptr conversion returns an order token for a reference-holding pointee) -->

**Full detail:** [Reference → Pointers](ConversionStrategies-Reference/pointers.md#pointers) — which parameters become `ref` and their call forms, the deferred nil panic, per-iteration loop boxes, closures over boxed locals, equality and pinning, and `unsafe.Pointer` conversions.

---

## Implicit Pointer Dereferencing

Go lets you skip the `*` and the `&` in two common places. `p.f` on a pointer `p` means `(*p).f`, and
calling a pointer method on a variable `v` means `(&v).M()`. Converted C# keeps these short forms
wherever it can, so most code reads like the Go. In short:

- **Pointer parameters and pointer receivers:** the body reads exactly like Go, through a C# `ref` alias.
- **Pointer locals:** `~p` reads the value and `p.Value` writes it.
- **Pointer method calls:** `v.M()` and `p.M()` both stay as written.

A few golib names carry this. The [glyph table](#reading-converted-code-names-and-glyphs) lists them all,
and [Pointers](#pointers) covers how pointers are made.

- `ж<T>` (read "zhe") is Go's pointer type `*T`: a reference-type box that holds, or points at, one `T`.
- `Ꮡ` has two roles. As a name prefix, as in `Ꮡt`, it marks a variable that holds the box. As a golib
  function, `Ꮡ(value)` boxes a copy of a value, like Go's `&T{…}`.
- `~p` is Go's `*p`. golib overloads C#'s unary `~` operator, normally bitwise complement, on `ж<T>`
  to mean "dereference". It returns a copy of the value and panics on a nil pointer, as Go does.
- `p.Value` is a C# `ref` to the value's real storage inside the box, so writes through it stick.

**A pointer parameter's body reads like Go.** A pointer parameter that stays a box has type `ж<T>` and
takes the `Ꮡ` prefix, here `Ꮡt`. When the body uses the value the pointer points at, the function first
binds a C# `ref` alias with the plain Go name, `t`. The body then uses `t` exactly as Go does, with no
dereference.

<!-- source: src/tests/Behavioral/PointerToArrayElementAddress/main.go:11-22 -->
```go
type row [4]uint32
…
func populate(t *row, base uint32) {
	for i := 0; i < len(t); i++ {
		t[i] = base + uint32(i)
	}
}
```
<!-- source: src/tests/Behavioral/PointerToArrayElementAddress/main.cs.target:7-17 -->
```csharp
[GoType("[4]uint32")] partial struct row;
…
internal static void populate(ж<row> Ꮡt, uint32 @base) {
    ref var t = ref Ꮡt.DerefOrNull();

    for (nint i = 0; i < 4; i++) {
        t[i] = @base + (uint32)i;
    }
}
```

`t[i] = …` writes straight into the caller's array, because `t` is a `ref` to it. For a nil pointer,
golib's `DerefOrNull()` returns a null `ref` instead of failing at once. The first read or write through
`t` then raises Go's nil pointer dereference panic, at the same point Go would.
<!-- DerefOrNull binds Unsafe.NullRef<T>() for a nil box; the first use throws NullReferenceException, which golib maps to Go's "invalid memory address or nil pointer dereference" runtime error: src/core/golib/ж.PointerExtensions.cs:445-451 and its remarks. -->

`row` is Go's array type `[4]uint32`, and `[GoType]` marks it for the [source generators](#source-generators).
An array's length is fixed, so `len(t)` becomes the constant `4` ([Slices and Arrays](#slices-and-arrays)).
Go's `int` is C#'s `nint` ([Integer Types and Arithmetic](#integer-types-and-arithmetic)). `@base` is Go's
`base`, [escaped](#reading-converted-code-names-and-glyphs) because `base` is a C# keyword.

**Some pointer parameters are a plain C# `ref T` instead.** When an unexported function only
dereferences a pointer parameter, the parameter is often a plain C# `ref T`, with no box and no `Ꮡ`
prefix. The parameter is already the alias, so the body needs no extra line.

<!-- source: src/tests/Behavioral/AddressOfParamWrite/main.go:44-46 -->
```go
func bump(p *int) {
	*p += 10
}
```
<!-- source: src/tests/Behavioral/AddressOfParamWrite/main.cs.target:29-31 -->
```csharp
internal static void bump(ref nint p) {
    p += 10;
}
```

A call passes a C# `ref` where Go passes an address: `bump(&b.R.Min)` becomes `bump(ref b.R.Min)`.
Either way the body reads like Go. [Pointers](#pointers) explains when each form is used.
<!-- call site: src/tests/Behavioral/AddressOfParamWrite/main.go:57 `bump(&b.R.Min)` -> main.cs.target:39 `bump(ref b.R.Min);`. Rule: docs/ConversionStrategies-Reference/pointers.md (A pointer parameter whose every use is a dereference is a `ref` parameter). -->

**A pointer local reads through `~` and writes through `.Value`.** A local that holds a pointer is the box
itself, so it keeps its plain Go name; there is no `ref` alias to take that name. A read uses `~p`, which
returns a copy of the value. A write, `++` or `--` goes through `p.Value` instead, because a write to a
copy would be lost.

This complete program shows both. `Ꮡ(new counter(n: 5))` is Go's `&counter{n: 5}`: `new counter(n: 5)`
builds the struct value ([Struct Types](#struct-types)), and `Ꮡ(…)` boxes it.

<!-- source: src/tests/Behavioral/IncDecPointerField/main.go:10-31 -->
```go
type inner struct{ k int }

type counter struct {
	n   int
	sub inner
}

//go:noinline
func get(c *counter) *counter { return c }

func main() {
	base := &counter{n: 5}
	base.sub.k = 3
	c := get(base) // c is a *counter LOCAL (heap box, not a ref-aliased parameter)

	c.n++     // direct field ++ through the pointer local
	c.n++     // 7
	c.sub.k-- // nested field -- through the pointer local

	// Read back through the original pointer to confirm the writes reached real storage.
	fmt.Println(base.n, base.sub.k) // 7 2
}
```
<!-- source: src/tests/Behavioral/IncDecPointerField/main.cs.target:7-28 -->
```csharp
[GoType] partial struct inner {
    internal nint k;
}

[GoType] partial struct counter {
    internal nint n;
    internal inner sub;
}

internal static ж<counter> get(ж<counter> Ꮡc) {
    return Ꮡc;
}

internal static void Main() {
    var @base = Ꮡ(new counter(n: 5));
    @base.Value.sub.k = 3;
    var c = get(@base);
    c.Value.n++;
    c.Value.n++;
    c.Value.sub.k--;
    fmt.Println((~@base).n, (~@base).sub.k);
}
```

`get` returns the pointer itself, so its parameter must stay a box: it is `ж<counter> Ꮡc`, with the `Ꮡ`
prefix, and `get` returns `Ꮡc` unchanged. `get` never reads the value, so it binds no `ref` alias.
<!-- a returned pointer disqualifies the ref T form: docs/ConversionStrategies-Reference/pointers.md ("returned, stored, captured..."). -->

A value-receiver method called through a pointer local is called on the copy `~p`. So the method gets
its own copy of the value, exactly as in Go.
<!-- source: src/tests/Behavioral/AddressOfParamWrite/main.go:190-191; src/tests/Behavioral/AddressOfParamWrite/main.cs.target:169 (rq := &Box{...}; rq.BumpedRecv() becomes (~rq).BumpedRecv(), where BumpedRecv is a value method, main.go:90). The worked example is left out here because its golden uses unrelated glyphs; the reference covers it. -->

**A pointer method on a variable takes the variable's address for you.** A pointer-receiver method
usually becomes a C# extension method whose receiver is `this ref T`, marked `[GoRecv]`
([Functions and Methods](#functions-and-methods)). C# passes the variable by reference, so `u.start(3)`
stays `u.start(3)` and changes `u` itself. There is no box and no allocation.

<!-- source: src/tests/Behavioral/ForMethodInitPost/main.go:5-40 -->
```go
type iter struct{ i, n int }

func (it *iter) start(n int) { it.i = 0; it.n = n }
func (it *iter) valid() bool { return it.i < it.n }
func (it *iter) next()       { it.i++ }
…
func main() {
	var u iter
	sum := 0
	for u.start(3); u.valid(); u.next() {
		sum += u.i
	}
	fmt.Println(sum)
	…
}
```
<!-- source: src/tests/Behavioral/ForMethodInitPost/main.cs.target:7-46 -->
```csharp
[GoType] partial struct iter {
    internal nint i, n;
}

[GoRecv] internal static void start(this ref iter it, nint n) {
    it.i = 0;
    it.n = n;
}

[GoRecv] internal static bool valid(this ref iter it) {
    return it.i < it.n;
}

[GoRecv] internal static void next(this ref iter it) {
    it.i++;
}
…
internal static void Main() {
    iter u = default!;
    nint sum = 0;
    for (u.start(3); u.valid(); u.next()) {
        sum += u.i;
    }
    fmt.Println(sum);
    …
}
```

`iter u = default!;` is Go's zero value for `var u iter` ([Nil and Zero Values](#nil-and-zero-values)).
The `!` only silences C# nullable warnings.
<!-- source: src/tests/Behavioral/EmbeddedValuePointerMethod/main.go:90-91 and main.cs.target:77-78 (c := chunk{}; c.alloc(5) stays c.alloc(5)); src/tests/Behavioral/EmbeddedValuePointerMethod/main.go:51; main.cs.target:47 ([GoRecv] internal static void alloc(this ref chunk c, uint16 n) {) -->
<!-- Moved to the reference (pointers.md): a method that keeps its receiver as a pointer takes this ж<T>, and a variable used as its receiver lives in a heap box, so b.Grow(n) becomes Ꮡb.Grow(n) (src/core/strings/strings.cs:522-523; src/core/strings/builder.cs:76). -->

**A pointer method on a pointer local is called the same way.** `[GoRecv]` asks the
[source generators](#source-generators) to add a second overload whose receiver is the box, `this ж<T>`.
That overload binds a `ref` to the box's value and forwards the call. So `b.linkTo(c)` stays
`b.linkTo(c)` when `b` is a pointer local.

A method that uses its receiver as a pointer, such as `initSelf`, which stores `r` in a pointer field,
works the other way round. It takes the box directly, `this ж<ring> Ꮡr`, and has no `[GoRecv]`. Its body
binds a `ref` alias `r`, like a pointer parameter, and a pointer local calls it directly: `a.initSelf()`.

<!-- source: src/tests/Behavioral/ReceiverPointerValue/main.go:15-84 -->
```go
type ring struct {
	data int
	next *ring
}

// receiver assigned to its own pointer field (self-link)
func (r *ring) initSelf() {
	r.next = r
}

// receiver linked to another node
func (r *ring) linkTo(other *ring) {
	r.next = other
}
…
func main() {
	a := &ring{data: 1}
	a.initSelf() // a.next = a
…
	b := &ring{data: 2}
	c := &ring{data: 3}
	b.linkTo(c)
	fmt.Println(b.next.data)         // 3
…
}
```
<!-- source: src/tests/Behavioral/ReceiverPointerValue/main.cs.target:7-62 -->
```csharp
[GoType] partial struct ring {
    internal nint data;
    internal ж<ring> next;
}

internal static void initSelf(this ж<ring> Ꮡr) {
    ref var r = ref Ꮡr.DerefOrNull();

    r.next = Ꮡr;
}

[GoRecv] internal static void linkTo(this ref ring r, ж<ring> Ꮡother) {
    r.next = Ꮡother;
}
…
internal static void Main() {
    var a = Ꮡ(new ring(data: 1));
    a.initSelf();
…
    var b = Ꮡ(new ring(data: 2));
    var c = Ꮡ(new ring(data: 3));
    b.linkTo(c);
    fmt.Println((~(~b).next).data);
…
}
```

Each pointer in a chain gets its own `~`. Go's `b.next.data` passes through two pointers, `b` and
`b.next`. So `(~b).next` reads `b`'s value and takes its `next` box, and the outer `~` reads that box's
value: `(~(~b).next).data`.
<!-- generator overload shape: src/gen/go2cs-gen/Templates/ReceiverMethod/ReceiverMethodTemplate.cs:97-102 (ref var r = ref Ꮡr.DerefOrNull(); r.linkTo(...)); chain read also at src/tests/Behavioral/ReceiverPointerValue/main.go:64 -> main.cs.target:48 ((~(~a).next).data). initSelf/advance/chain/pair are direct-ж receivers (this ж<ring> Ꮡr, no [GoRecv]) at main.cs.target:12, 22, 30, 40. -->

**Full detail:** [Reference → Implicit Pointer Dereferencing](ConversionStrategies-Reference/implicit-dereferencing.md#implicit-pointer-dereferencing) — how the converter decides that a selector base needs a dereference, promoted fields through a pointer local, nested and indexed write targets, and `*p.field` through parameters and receivers.

---

## `unsafe.Pointer` and `uintptr`
<!-- length: longer than the usual summary section by design: seven rules a reader meets in converted runtime and syscall code, each written to stand alone with a complete example and its names explained; six carry an example (the syscall keep-alive shows its C# only, because its Go lives in GOROOT and is quoted inline), order tokens have no emitted form and are described in words; unsafe.Slice/String aliasing and their copy cases live in the reference -->

Go's `unsafe.Pointer` becomes [`@unsafe.Pointer`](../src/core/unsafe/unsafe.cs), a class from go2cs's
hand-written `unsafe` package. It holds an address as a number. It can also hold the Go pointer it was
made from, so the object stays alive and the pointer can be recovered later. Go's `uintptr` becomes
golib's [`uintptr`](../src/core/golib/uintptr.cs) struct, an unsigned integer the size of an address.

A few names recur in this section. A Go pointer `*T` is a golib [`ж<T>`](#pointers) box (read "zhe"),
and a name starting with `Ꮡ` holds such a box. The package alias is `@unsafe` because `unsafe` is a C#
keyword. The [glyph table](#reading-converted-code-names-and-glyphs) lists every glyph.

Go's garbage collector does not move heap objects, and the CLR's does. So an address only means
something in C# while its storage is held still, or "pinned". The CLR can pin a value only when it holds
no managed references: no strings, slices, maps, interfaces or pointers inside. The rules below say when
pinning happens, and what a value that cannot be pinned gets instead.

**`Sizeof`, `Alignof` and `Offsetof` fold to Go's numbers.** The converter computes each one from Go's
layout rules and writes the number into the C#. The Go expression stays beside it as a comment:

<!-- source: src/tests/Behavioral/UnsafeOperations/UnsafeOperations.go:98-104 -->
```go
	var x struct {
		a int64
		b bool
		c string
	}
	const M, N = unsafe.Sizeof(x.c), unsafe.Sizeof(x)
	fmt.Println(M, N)
```
<!-- source: src/tests/Behavioral/UnsafeOperations/UnsafeOperations.cs.target:91-94 -->
```csharp
    main_x x = default!;
    uintptr M = /* unsafe.Sizeof(x.c) */ 16;
    uintptr N = /* unsafe.Sizeof(x) */ 32;
    fmt.Println(M, N);
```

`main_x` is the C# struct declared for the anonymous `struct { … }` type
([Struct Types](#struct-types)), and `default!` is its zero value
([Nil and Zero Values](#nil-and-zero-values)). The operand is never evaluated, as in Go.

`Alignof` and `Offsetof` fold the same way: `unsafe.Offsetof(x.c)` becomes
`/* unsafe.Offsetof(x.c) */ (uintptr)16`.
<!-- the Offsetof fold: src/tests/Behavioral/UnsafeOperations/UnsafeOperations.go:112 -> UnsafeOperations.cs.target:100; the other Alignof/Offsetof lines at UnsafeOperations.go:106-111 -> cs.target:95-99 -->
<!-- main_x declaration: src/tests/Behavioral/UnsafeOperations/UnsafeOperations.cs.target:62-66 (`[GoType("dyn")] internal partial struct main_x`). Go computes all three at compile time from the static type: UnsafeOperations.go:137-139 (comment); the non-blittable Padded case at UnsafeOperations.go:28-40 -> cs.target:111. -->

These are Go's numbers, not measurements of the C# struct. Adding them to a real address is meant for
storage laid out like Go's, such as an array of numbers. A struct that holds strings, slices or pointers
has no address at all in C#, so its pointers get order tokens (the order-token rule in this section).
<!-- the layout question: the CLR lays out a C# struct of unmanaged fields sequentially (the C# default for structs; go2cs adds no StructLayout), while a reference-bearing struct is CLR auto-layout and gets PointerStorage.None (src/core/golib/ж.StandardBox.cs:174). src/core/golib/runtime/RuntimeErrorPanic.cs:30-40 (remarks) records that a Go-layout offset into a CLR-auto-laid-out struct is refused loudly rather than answered. -->

**Converting a pointer to `uintptr` gives a real address**, when the value it points to holds no
strings, slices or pointers (for one that does, see the order-token rule in this section). Go's idiom
`uintptr(unsafe.Pointer(p))` becomes a plain `(uintptr)` cast of the pointer's box `Ꮡp`. The
`unsafe.Pointer` step in the middle leaves no trace in the C#. The cast pins the storage for as long as
the box lives, so the number stays valid. A nil pointer gives `0`, as in Go:

<!-- source: src/tests/Behavioral/UintptrUnsafePointerIdiom/main.go:25, 31-33 -->
```go
type point struct{ x, y int32 }
…
func addrOf(p *point) uintptr {
	return uintptr(unsafe.Pointer(p))
}
```
<!-- source: src/tests/Behavioral/UintptrUnsafePointerIdiom/main.cs.target:8-10, 15-17 -->
```csharp
[GoType] partial struct point {
    internal int32 x, y;
}
…
internal static uintptr addrOf(ж<point> Ꮡp) {
    return (uintptr)Ꮡp;
}
```

`point` holds only two `int32`s, so its storage can be pinned. `[GoType]` marks a type declared in Go,
which the [source generators](#source-generators) complete.
<!-- nil gives 0: UintptrUnsafePointerIdiom/main.go:45-46 -> main.cs.target:36-37 (`ж<point> missing = default!;` / `addrOf(missing) == 0`); golib's ж<T> -> uintptr operator returns 0 for a nil box (src/core/golib/ж.cs:933-945). point is reference-free (main.go:23-25), so it is pinnable. A reference-bearing pointee returns value.PointerOrderToken instead: ж.cs:979, with PointerStorage.None from ж.StandardBox.cs:173-174 and ж.ElemRefBox.cs:251-253. -->

**Address arithmetic works, and converting back aliases the same memory.** Adding an offset to a pinned
address gives the address of the neighbouring value. Converting that number back to a pointer and
reading through it reads the real storage, as in Go:

<!-- source: src/tests/Behavioral/UnsafeOperations/UnsafeOperations.go:78-83 -->
```go
	arr := [4]int{1, 2, 3, 4}
	arrptr := &arr[0]

	// Move the pointer to the next element in the array
	nextPtr := unsafe.Pointer(uintptr(unsafe.Pointer(arrptr)) + unsafe.Sizeof(arr[0]))
	fmt.Println("Value of the next element:", *(*int)(nextPtr))
```
<!-- source: src/tests/Behavioral/UnsafeOperations/UnsafeOperations.cs.target:59, 77-81 -->
```csharp
private static readonly object valueOfTheNextElementˢ = (@string)"Value of the next element:"u8;
…
    ref var arr = ref heap<array<nint>>(out var Ꮡarr);
    arr = new nint[]{1, 2, 3, 4}.array();
    var arrptr = Ꮡarr.at<nint>(0);
    @unsafe.Pointer nextPtr = (@unsafe.Pointer)((uintptr)arrptr + /* unsafe.Sizeof(arr[0]) */ (uintptr)8);
    fmt.Println(valueOfTheNextElementˢ, ~(ж<nint>)(uintptr)(nextPtr));
```

This prints `2`, the second element, under both Go and C#. In the C#:

- `array<nint>` is golib's fixed-size Go array, here `[4]int`, and `nint` is Go's `int`
  ([Slices and Arrays](#slices-and-arrays), [Integer Types and Arithmetic](#integer-types-and-arithmetic)).
- `heap<T>(out var Ꮡarr)` puts a zero `arr` in a box, `Ꮡarr`, because its address is taken. The next
  line stores the initial value ([Pointers](#pointers)).
- `Ꮡarr.at<nint>(0)` is `&arr[0]`, a pointer to one array element.
- `(ж<nint>)(uintptr)(nextPtr)` is Go's `(*int)(nextPtr)`. The converter goes through the number:
  `(uintptr)` takes the address out of the `@unsafe.Pointer`, and `(ж<nint>)` makes a pointer from it.
- `~` reads through a pointer, like Go's `*p`
  ([Implicit Pointer Dereferencing](#implicit-pointer-dereferencing)).
- `valueOfTheNextElementˢ` is the string literal, stored once in a static field. `(@string)"…"u8` is
  a Go string built from the UTF-8 bytes of a C# `u8` literal ([Strings](#strings-string-and-sstring)).
  The field is typed `object` because `fmt.Println` takes `any` values
  ([Empty Interface (`any`)](#empty-interface-any)).

`nextPtr` is built from a bare number, so it carries no box. The conversion back finds the element's
storage from its address. As in Go, a number alone does not keep its object alive: this works while
`arr` is still in use.
<!-- expected output: src/tests/Behavioral/UnsafeOperations (Go's `go run` output is the behavioral oracle; the second element of [4]int{1, 2, 3, 4}). The reverse conversion `explicit operator ж<T>(uintptr)` at src/core/golib/ж.cs:751 resolves a pinned address through its provenance record, a weak entry that dies with its box (ж.cs comment "the WEAK-ENTRY bound", around lines 895-900): a program that drops every reference and converts back later is one Go itself declares invalid. -->

**A pointer to a value that holds strings, slices or pointers gets an order token, not an address.**
The CLR cannot pin such a value, so golib hands out a unique stand-in number instead. A token compares
and sorts like an address. It converts back to the same pointer while that pointer is still in use, as
an address does in Go.
<!-- the three storage kinds: src/core/golib/ж.cs:62-83 (None = order token, Unpinnable, Pinnable); ж.StandardBox.cs:174 and ж.ElemRefBox.cs:252 (None for a reference-bearing T); ж.FieldRefBox.cs:153-161 (a field reference always names a real interior address, held still only when the enclosing variable can be pinned; the Unpinnable kind, whose address may move after it is taken, is documented at ж.cs:70-76). Shown by: src/tests/Behavioral/ReflectFieldAddrWrite (a reflect-projected token converted back to a pointer and written through); GolibTests PointerTokenConversionTests, ManagedPointerTokenMintTests. -->

A token has no memory behind it, so it has no neighbours. Adding an offset to a token and converting the
result back to a pointer raises a Go panic that says so. Like any Go panic, `recover` can catch it
([Defer / Panic / Recover](#defer--panic--recover)).
<!-- the panics: src/core/golib/ж.cs:751-930 (token arithmetic refused; arm 2a panics at dereference of an order token read as another type); src/core/golib/runtime/RuntimeErrorPanic.cs:41 (UnsafePointerArithmeticWithoutAddress, arithmetic refusal) and :75 (UnsafePointerOrderTokenDereferenced, order-token dereference refusal); GolibTests TokenArithmeticRefusalTests, OrderTokenOffsetZeroRefusalTests -->

**`unsafe.Pointer(p)` keeps what it points to alive.** A standalone conversion becomes a factory call
such as `@unsafe.Pointer.FromPinnedBox(e0)`. The `@unsafe.Pointer` it builds stores the source box beside
the number. Holding the box keeps the referent alive, as Go's collector does. `FromPinnedBox` also pins
the storage, so converting back to `uintptr` gives exactly the same address:

<!-- source: src/tests/Behavioral/UintptrUnsafePointerIdiom/main.go:50-54, 57-58 -->
```go
	var buf [4]int32
	e0, e2 := &buf[0], &buf[2]
	a0 := uintptr(unsafe.Pointer(e0))
	a2 := uintptr(unsafe.Pointer(e2))
	fmt.Println("stride:", a0 != 0, a2-a0 == 2*unsafe.Sizeof(buf[0]))
…
	up := unsafe.Pointer(e0)
	fmt.Println("round trip:", uintptr(up) == a0)
```
<!-- source: src/tests/Behavioral/UintptrUnsafePointerIdiom/main.cs.target:23-24, 38-44 -->
```csharp
private static readonly object strideˢ = (@string)"stride:"u8;
private static readonly object roundTripˢ = (@string)"round trip:"u8;
…
    ref var buf = ref heap(new array<int32>(4), out var Ꮡbuf);
    var (e0, e2) = (Ꮡbuf.at<int32>(0), Ꮡbuf.at<int32>(2));
    var a0 = (uintptr)e0;
    var a2 = (uintptr)e2;
    fmt.Println(strideˢ, a0 != 0, a2 - a0 == 2 * /* unsafe.Sizeof(buf[0]) */ (uintptr)4);
    @unsafe.Pointer up = @unsafe.Pointer.FromPinnedBox(e0);
    fmt.Println(roundTripˢ, (uintptr)up == a0);
```

Every comparison prints `true`, under Go and under the converted C#. The two element addresses are
exactly two `int32`s apart, and the round trip returns the same number. In the C#:

- `array<int32>` is golib's fixed-size Go array `[4]int32` ([Slices and Arrays](#slices-and-arrays)).
- `heap(value, out var Ꮡbuf)` puts `buf` in a box, `Ꮡbuf`, starting from the given value
  ([Pointers](#pointers)).
- `e0` and `e2` are `ж<int32>` boxes for `&buf[0]` and `&buf[2]`. The tuple assignment is Go's
  `e0, e2 := …` ([Multi-Assignment and Evaluation Order](#multi-assignment-and-evaluation-order)).
- `(uintptr)e0` is the collapsed `uintptr(unsafe.Pointer(e0))`, as in the `addrOf` example.
- `strideˢ` and `roundTripˢ` are the string literals, stored once in static fields. `(@string)"…"u8` is a
  Go string built from UTF-8 bytes ([Strings](#strings-string-and-sstring)).
<!-- FromPinnedBox versus FromBox: src/core/unsafe/unsafe.cs:452-497 (FromBox carries a pointer value across without pinning; FromPinnedBox takes the address through the box's own uintptr conversion, which pins, and retains the box). The reference lists when each factory is emitted. The program prints the same booleans under go run and the converted C#: UintptrUnsafePointerIdiom/main.go:9-11. -->

**Reading a number's bits as another number is a `bitcast`.** A value pun between two number types,
such as Go's `math.Float64bits`, becomes golib's [`bitcast<TSrc, TDst>`](../src/core/golib/builtin.cs).
It copies the bits and boxes nothing. It accepts only plain value types, with no strings, slices or
pointers inside:

<!-- source: src/tests/Behavioral/UnsafeOperations/UnsafeOperations.go:57-59 -->
```go
func Float64bits(f float64) uint64 {
	return *(*uint64)(unsafe.Pointer(&f))
}
```
<!-- source: src/tests/Behavioral/UnsafeOperations/UnsafeOperations.cs.target:50-52 -->
```csharp
public static uint64 Float64bits(float64 f) {
    return bitcast<float64, uint64>(f);
}
```
<!-- the reverse pun Float64frombits: UnsafeOperations.go:61-63 -> cs.target:54-56 (`bitcast<uint64, float64>(b)`); integer pun UnsafeOperations.go:95 -> cs.target:89 (`uint8 k = bitcast<int8, uint8>(i);`). bitcast: src/core/golib/builtin.cs:3181 (`where TSrc : unmanaged where TDst : unmanaged`). -->

**Reading a struct as another struct with the same fields is a `Reinterpret`.** Go's
`*(*T2)(unsafe.Pointer(&t1))` becomes `~Ꮡt1.Reinterpret<T1, T2>()`: a view of the same box, read as the
other type. Nothing is copied until `~` reads the value:

<!-- source: src/tests/Behavioral/UnsafeOperations/UnsafeOperations.go:8-14, 85-90 -->
```go
type T1 struct {
	a int32
}

type T2 struct {
	a int32
}
…
	var t1 T1
	t1.a = 42

	// Convert t1 to type T2
	t2 := *(*T2)(unsafe.Pointer(&t1))
	fmt.Println("Value of t2.a:", t2.a)
```
<!-- source: src/tests/Behavioral/UnsafeOperations/UnsafeOperations.cs.target:8-14, 60, 82-85 -->
```csharp
[GoType] partial struct T1 {
    internal int32 a;
}

[GoType] partial struct T2 {
    internal int32 a;
}
…
private static readonly object valueOfT2Aˢ = (@string)"Value of t2.a:"u8;
…
    ref var t1 = ref heap(new T1(), out var Ꮡt1);
    t1.a = 42;
    var t2 = ~Ꮡt1.Reinterpret<T1, T2>();
    fmt.Println(valueOfT2Aˢ, t2.a);
```

This prints `Value of t2.a: 42` under both Go and C#. `t1` is boxed as `Ꮡt1` because its address is
taken ([Pointers](#pointers)). `Reinterpret<T1, T2>()` returns a `ж<T2>` over the same storage, and `~`
reads through it ([Implicit Pointer Dereferencing](#implicit-pointer-dereferencing)).
`valueOfT2Aˢ` is the hoisted string literal ([Strings](#strings-string-and-sstring)).
<!-- struct pun: src/tests/Behavioral/UnsafeOperations/UnsafeOperations.go:89 -> UnsafeOperations.cs.target:84 (`var t2 = ~Ꮡt1.Reinterpret<T1, T2>();`). -->

**A pointer passed to a system call stays alive until the call returns, as in Go.** Go guarantees this
for `uintptr(unsafe.Pointer(p))` written directly in a `syscall.Syscall` argument list. The converter
moves the pointer into a temporary whose name starts with `ᴋ`, casts it for the call, then calls
`GC.KeepAlive` on it. The cast pins the storage, and keeping the box alive keeps it pinned, so the
kernel writes into storage that cannot move.

In `internal/syscall/unix`, Go's `GetRandom` calls
`syscall.Syscall(getrandomTrap, uintptr(unsafe.Pointer(unsafe.SliceData(p))), uintptr(len(p)), uintptr(flags))`.
It becomes:
<!-- Go side: Go 1.24.13 src/internal/syscall/unix/getrandom.go:36-39 (GOROOT; not in this repo): `syscall.Syscall(getrandomTrap, uintptr(unsafe.Pointer(unsafe.SliceData(p))), uintptr(len(p)), uintptr(flags))` -->
<!-- source: src/core/internal/syscall/unix/linux/getrandom.cs:26-27, 38-40, 47-48 -->
```csharp
// GetRandom calls the getrandom system call.
public static (nint n, error err) GetRandom(slice<byte> p, GetRandomFlag flags) {
…
    var ᴋ0 = @unsafe.SliceData(p);
        var (r1, _, errno) = syscall.Syscall(getrandomTrap, (uintptr)ᴋ0, (uintptr)len(p), (uintptr)flags);
    System.GC.KeepAlive(ᴋ0);
…
    return ((nint)r1, default!);
}
```

In the C#:

- `p` is a Go `[]byte`, golib's `slice<byte>` ([Slices and Arrays](#slices-and-arrays)).
- `@unsafe.SliceData(p)` is Go's `unsafe.SliceData`, so `ᴋ0` is a `ж<byte>` box for `p`'s first element.
- `(uintptr)ᴋ0` is the collapsed `uintptr(unsafe.Pointer(…))`, which pins the byte storage.
- The three `ᴋ0` lines run in order, in one block. The deeper indent of the `Syscall` line is only how
  the text is laid out.
- `getrandomTrap` is the package's system-call number. The first omitted lines try a faster path, and
  the later ones turn a failure number into a Go error.
- The tuple result is Go's multiple return ([Multi-Result Values](#multi-result-values-and-comma-ok-forms)).
<!-- SliceData returns ж<T>: src/core/unsafe/unsafe.cs:957. The omitted lines: getrandom.cs:28-37 (the vgetrandom fast path and the getrandomUnsupported check) and 41-46 (errno handling). The indent of line 39 is an emission layout artifact; lines 38-40 sit in the same block of GetRandom's body. The keep-alive temp appears across the converted syscall surface, e.g. src/core/internal/syscall/unix/linux/at.cs, src/core/syscall/darwin/syscall_darwin.cs, src/core/internal/syscall/windows/windows/zsyscall_windows.cs. No behavioral test emits this shape (src/tests/Behavioral has no `GC.KeepAlive(ᴋ` in any .cs.target). -->

**Full detail:** [Reference → Converting a Go pointer to `unsafe.Pointer`](ConversionStrategies-Reference/pointers.md#converting-a-go-pointer-to-unsafepointer) — every conversion form and when each factory is used, address arithmetic, pinning, order tokens, layout folding, value puns, the cases where `unsafe.Slice` or `unsafe.String` copies instead of aliasing, and the syscall keep-alive rule.

---

## Source Generators

Some Go behavior cannot be written directly in C#, or would need a lot of boilerplate code to write out. The
converter therefore emits a short declaration marked with an attribute. Roslyn source generators
([`src/gen/go2cs-gen`](../src/gen/go2cs-gen/)) write the rest while the project compiles. This keeps the
converted code short and close to the Go original.

**Generated code is not in the converted `.cs` files.** When converted code uses a constructor, overload,
adapter or conversion that you cannot find in any `.cs` file, a generator wrote it. The compiler saves that
output as files you can read. For a single package they are in the project's `Generated` folder. For a
`-recurse` conversion, which converts a package together with the packages it imports, they are under
`.artifacts/gen` in the output root.

**Two glyphs appear throughout these examples.** `ж<T>` (read "zhe") is a Go pointer `*T`: a
[golib](#the-golib-runtime-library) heap box that holds one value. `Ꮡ` marks an address. As a name
prefix it marks a variable that holds a box (`Ꮡb`) or a field reference (`Buffer.Ꮡoff`). Called as
`Ꮡ(v)`, it is a golib function that puts a value in a new box, the C# form of Go's `&T{…}`. See
[Pointers](#pointers) and [Names and Glyphs](#reading-converted-code-names-and-glyphs).

**A few golib types stand in for Go's.** `@string` is Go's `string`, `slice<T>` is `[]T`, and `nint` is
Go's `int`. A literal such as `"%s=%d"u8` is a C# UTF-8 string literal, which golib accepts as a Go
string. See [Strings](#strings-string-and-sstring), [Slices and Arrays](#slices-and-arrays) and
[Integer Types and Arithmetic](#integer-types-and-arithmetic).

**Each generator reacts to one marker the converter emits: an attribute, or, for stubs, a `partial` method with no body.**

| Generator | Driven by | Produces |
|---|---|---|
| `TypeGenerator` | `[GoType]` on a type | the body of each type: struct constructors, field references, equality and `ToString`; wrappers for named numeric, slice, array, map and channel types; for an interface, the code that converts a value to it at run time, as a type assertion does ([Interfaces](#interfaces)); members promoted by [struct embedding](#struct-type-embedding) |
| `RecvGenerator` | `[GoRecv]` on a method | a `ж<T>` overload of each pointer-receiver method, so it can be called on a box |
| `ImplementGenerator` | `[assembly: GoImplement<T, I>]` | the code that lets `T` or `*T` be used as interface `I` ([Interfaces](#interfaces)) |
| `ImplicitConvGenerator` | `[assembly: GoImplicitConv<S, T>]` | a conversion operator between two types C# cannot convert directly, such as two named numeric types or two structs with the same underlying type; the C# still writes the conversion where Go does, and the operator lets it compile |
| `StrGenerator` | `[GoStr]` on a method | for a function that takes its string as an `sstring` (golib's stack-only string view, a `ref struct`), the matching `@string` overload ([Strings](#strings-string-and-sstring)) |
| `PartialStubGenerator` | a `partial` method with no body | a stub that throws, when no hand-written body exists ([Functions Without a Go Body](#functions-without-a-go-body)) |

### A `[GoType]` struct lists only its fields

The converter writes a Go struct as a C# `partial struct` that holds only the fields. `partial` lets
`TypeGenerator` supply the other half: constructors, the `==` operator, `ToString`, and one field
reference per field, such as `Buffer.Ꮡoff` ([Struct Types](#struct-types)).

<!-- source: src/tests/Behavioral/PointerToPointer/PointerToPointer.go:5-74 -->
```go
type Buffer struct {
	buf      []byte
	off      int
	lastRead int8
}
…
	b := Buffer{}
	PrintValPtr(&b.off)
…
func PrintValPtr(ptr *int) {
	fmt.Printf("Value available at *ptr = %d\n", *ptr)
	*ptr++
}
```
<!-- source: src/tests/Behavioral/PointerToPointer/PointerToPointer.cs.target:7-66 -->
```csharp
[GoType] partial struct Buffer {
    internal slice<byte> buf;
    internal nint off;
    internal int8 lastRead;
}
…
    ref var b = ref heap<Buffer>(out var Ꮡb);
    b = new Buffer(nil);
    PrintValPtr(Ꮡb.of(Buffer.Ꮡoff));
…
public static void PrintValPtr(ж<nint> Ꮡptr) {
    ref var ptr = ref Ꮡptr.DerefOrNull();

    fmt.Printf("Value available at *ptr = %d\n"u8, ptr);
    ptr++;
}
```

Reading the C# line by line:

- Go takes the address of `b.off`, so `b` must live on the heap. golib's `heap(…)` allocates a box
  `Ꮡb` (a `ж<Buffer>`), and `b` is a C# `ref` to the value inside it ([Pointers](#pointers)).
- `new Buffer(nil)` is a generated constructor. Passing golib's `nil` asks for the zero value, as Go's
  `Buffer{}` does. Unlike C#'s `default`, it also allocates what a Go zero value needs, such as the
  storage of an array field ([Nil and Zero Values](#nil-and-zero-values)).
- `Buffer.Ꮡoff` is a generated field reference: a static accessor that returns a `ref` to the `off`
  field of a `Buffer`. `Ꮡb.of(Buffer.Ꮡoff)` uses it to make a `ж<nint>` that points at `off` inside the
  box, so a write through it changes `b.off`. That is Go's `&b.off`.
- `PrintValPtr` takes that box, Go's `*int`. It binds `ptr` as a `ref` to the value in the box, so the
  body reads like Go's `*ptr` ([Implicit Pointer Dereferencing](#implicit-pointer-dereferencing)).

### A pointer-receiver method gains a box overload

A Go method with a pointer receiver usually becomes a C# extension method on `ref T`, marked `[GoRecv]`.
The `ref` lets the method change the caller's value, as Go's pointer receiver does
([Functions and Methods](#functions-and-methods)). A method with a value receiver takes `T` itself and
has no `[GoRecv]`.

<!-- source: src/tests/Behavioral/NamedInterfacePointerMethodSet/main.go:32-76 -->
```go
type Mixed struct{ n int }
…
func (m *Mixed) Bump() { m.n++ }
…
	pm := &Mixed{n: 41}
	pm.Bump()
```
<!-- source: src/tests/Behavioral/NamedInterfacePointerMethodSet/main.cs.target:25-83 -->
```csharp
[GoType] partial struct Mixed {
    internal nint n;
}
…
[GoRecv] public static void Bump(this ref Mixed m) {
    m.n++;
}
…
    var pm = Ꮡ(new Mixed(n: 41));
    pm.Bump();
```

`new Mixed(n: 41)` is the generated constructor, with each field as a named argument. In Go, `pm` is a
pointer. In C#, `Ꮡ(…)` puts the new value in a heap box, so `pm` is a `ж<Mixed>`. C# cannot pass a box
where `ref Mixed` is expected. `RecvGenerator` therefore emits an overload of `Bump` on `this ж<Mixed>`
that dereferences the box and calls the `ref` version. That overload is what `pm.Bump()` binds, so the
call reads exactly like the Go.

**A method that needs its receiver as a real pointer is written on `ж<T>` directly.** Examples are taking
a field's address (`&c.n`), returning the receiver, storing or comparing it, and using it inside a
closure. Such a method has no `[GoRecv]` and needs no generated overload. Here the receiver is stored in
a struct's pointer field:

<!-- source: src/tests/Behavioral/DirectBoxReceiverPassedWhole/main.go:5-34 -->
```go
type mc struct{ n int }
…
type wrap struct {
	p   *mc
	tag int
}

func (c *mc) wrapped() wrap {
	return wrap{c, 7} // positional: the receiver into a pointer field
}
```
<!-- source: src/tests/Behavioral/DirectBoxReceiverPassedWhole/main.cs.target:7-33 -->
```csharp
[GoType] partial struct mc {
    internal nint n;
}
…
[GoType] partial struct wrap {
    internal ж<mc> p;
    internal nint tag;
}

internal static wrap wrapped(this ж<mc> Ꮡc) {
    return new wrap(Ꮡc, 7);
}
```

The receiver `Ꮡc` is the box itself, so the `wrap` it returns points at the caller's `mc`, as in Go
([Pointers](#pointers)).

### The converter decides interface satisfaction; a generator builds it

A Go type satisfies an interface just by having the right methods. A C# type must declare the interfaces
it implements. The converter knows, from Go's type checker, every place a type is used as an interface.
It records each distinct pair once, as an assembly attribute in the package's `package_info.cs` file
([Package Conversion](#package-conversion)).

<!-- source: src/tests/Behavioral/PointerInterfaceStructField/main.go:7-52 -->
```go
type Describer interface {
	Describe() string
}

…
type Setting struct {
	name  string
	value int
}

func (s *Setting) Describe() string {
	return fmt.Sprintf("%s=%d", s.name, s.value)
}

…
type holder struct {
	d     Describer
	label string
}

…
func assignDescriber(h *holder, s *Setting) {
	h.d = s
}
…
	replacement := Setting{name: "assigned", value: 11}
	assignDescriber(h, &replacement)
```
<!-- source: src/tests/Behavioral/PointerInterfaceStructField/main.cs.target:7-41 -->
```csharp
[GoType] partial interface Describer {
    @string Describe();
}

[GoType] partial struct Setting {
    internal @string name;
    internal nint value;
}

[GoRecv] public static @string Describe(this ref Setting s) {
    return fmt.Sprintf("%s=%d"u8, s.name, s.value);
}

[GoType] partial struct holder {
    internal Describer d;
    internal @string label;
}

internal static void assignDescriber(ref holder h, ж<Setting> Ꮡs) {
    h.d = new SettingжDescriber(Ꮡs);
}
…
    ref var replacement = ref heap<Setting>(out var Ꮡreplacement);
    replacement = new Setting(name: "assigned"u8, value: 11);
    assignDescriber(ref (h).DerefOrNull(), Ꮡreplacement);
```

Only `*Setting` has the `Describe` method, so `package_info.cs` records the pair with `Pointer = true`:

<!-- source: src/tests/Behavioral/PointerInterfaceStructField/package_info.cs:39 -->
```csharp
[assembly: GoImplement<Setting, Describer>(Pointer = true)]
```

From that attribute, `ImplementGenerator` emits the adapter class `SettingжDescriber`. The name reads
"`Setting` pointer (`ж`) as `Describer`": the class holds the `ж<Setting>` box and implements `Describer`
by calling `Describe` on it. Go's `h.d = s` becomes `h.d = new SettingжDescriber(Ꮡs)`. When a value
receiver provides the methods, the record has no `Pointer = true`, and the struct itself implements the
interface. [Interfaces](#interfaces) covers both forms, and how equality and type assertions see
through an adapter.

The two pointer parameters take different shapes, depending on how the body uses them:

- `s *Setting` stays a box, `ж<Setting> Ꮡs`, because the interface keeps the pointer.
- `h *holder` is only used to reach its value, so it becomes `ref holder h`. In `main`, `h` is a box
  made by `h := &holder{…}`. The call passes `ref (h).DerefOrNull()`, a `ref` to the value inside that
  box, so the write to `h.d` lands in the caller's `holder` ([Pointers](#pointers)).

**Full detail:** [Reference → Source Generators](ConversionStrategies-Reference/source-generators.md#source-generators) — what each generator emits in depth, which attributes the converter emits and where they land, and each generator's edge cases.

---

## Functions Without a Go Body

Go lets a function be declared with no body: only a signature, with the code kept somewhere else. The
body may be assembly (a `.s` file), a C function, or another package's function joined by the
`//go:linkname` directive. C# has no bodyless function, but it has a close match: a `partial` method,
a declaration whose body another part of the same class supplies.

So go2cs keeps each bodyless Go function's name and signature. By default it emits a C# `partial` method,
but when it can write the body itself it emits an ordinary method instead. The body arrives in one of
these ways:

- a hand-written companion file supplies it;
- the converter writes a one-line forwarder that calls the real function, when a `//go:linkname`
  directive names a function known to have a C# body, or, outside the standard library, when a
  one-instruction assembly jump (a trampoline) names a Go function with the same signature;
- in the standard library, Go's portable Go file is converted in place of the assembly declaration;
- for a `//go:linkname` push that go2cs cannot support, the converter writes a body that raises a Go
  panic explaining why;
- failing all of those, a generated stub throws when the function is called.

**A bodyless declaration stays a `partial` method.** Go's `math` package implements `archMax` in assembly
on most 64-bit platforms. That platform's file declares only the signature, plus a constant telling
callers that the fast version exists:

<!-- source: GOROOT/src/math/dim_asm.go:7-11 -->
```go
package math

const haveArchMax = true

func archMax(x, y float64) float64
```
<!-- source: src/core/math/dim_asm.cs:5-11 -->
```csharp
namespace go;

partial class math_package {

internal const bool haveArchMax = true;

internal static partial float64 archMax(float64 x, float64 y);
```

`math_package` is the C# class that holds the whole Go package (see [Package Conversion](#package-conversion)).
`float64` keeps its Go name as an alias for C# `double` (see [Integer Types and Arithmetic](#integer-types-and-arithmetic)).

Because the name and signature do not change, every caller converts as ordinary code. `math.Max` calls
`archMax` when the constant says the fast version exists. Otherwise it calls `max`, which here is not
Go's built-in `max` but `math`'s own unexported function in `dim.go`, converted like any other Go code:

<!-- source: GOROOT/src/math/dim.go:40-45 -->
```go
func Max(x, y float64) float64 {
	if haveArchMax {
		return archMax(x, y)
	}
	return max(x, y)
}
```
<!-- source: src/core/math/dim.cs:41-46 -->
```csharp
public static float64 Max(float64 x, float64 y) {
    if (haveArchMax) {
        return archMax(x, y);
    }
    return max(x, y);
}
```

**A hand-written companion supplies the body.** C# does not compile a `partial` method that no part
implements. For `archMax`, a hand-written file named `math_impl.cs` sits beside the converted files and
supplies the implementing half. It calls `max`, the same portable Go fallback `math.Max` uses. The file
fills `archMin` and the other `arch…` functions the same way:

<!-- source: src/core/math/math_impl.cs:8-34 -->
```csharp
namespace go;

partial class math_package
{
…
    internal static partial float64 archMax(float64 x, float64 y) => max(x, y);

    internal static partial float64 archMin(float64 x, float64 y) => min(x, y);
}
```

The converter never writes a `*_impl.cs` file, so converting the package again keeps the body.
[Manually-Converted Declarations](#manually-converted-declarations) explains these hand-owned files.

**`//go:linkname` joins two functions, in one of two directions.** The directive tells Go's linker that
two names are one function, even across packages and even when a name is unexported. Which side writes
the full directive decides the direction:

- In a **pull**, the bodyless side names the body it wants: `//go:linkname local pkg.name` on a bodyless
  `local` says "my body is `name` in package `pkg`".
- In a **push**, the side that owns the body names the bodyless declaration it fills, in another package.

A one-argument form, `//go:linkname name`, names no other function. It marks `name` as a linkname
endpoint that another package's directive may reach.

**A `//go:linkname` pull becomes a forwarder.** Package `time/tzdata` uses a pull to call into `time`, a
package it never imports:

<!-- source: GOROOT/src/time/tzdata/tzdata.go:29-36 -->
```go
// registerLoadFromEmbeddedTZData is defined in package time.
//
//go:linkname registerLoadFromEmbeddedTZData time.registerLoadFromEmbeddedTZData
func registerLoadFromEmbeddedTZData(func(string) (string, error))

func init() {
	registerLoadFromEmbeddedTZData(loadFromEmbeddedTZData)
}
```
<!-- source: src/core/time/tzdata/tzdata.cs:28-37 -->
```csharp
// registerLoadFromEmbeddedTZData is defined in package time.
//
//go:linkname registerLoadFromEmbeddedTZData time.registerLoadFromEmbeddedTZData
[global::System.Diagnostics.StackTraceHidden] internal static void registerLoadFromEmbeddedTZData(Func<@string, (@string, error)> _) {
    go.time_package.registerLoadFromEmbeddedTZData(_);
}

[GoInit] internal static void init() {
    registerLoadFromEmbeddedTZData(loadFromEmbeddedTZData);
}
```

Instead of a `partial` method, the converter writes a real body that calls the target. Reading the C#:

- `go.time_package` is the class for package `time`, spelled in full because the Go file has no import to
  alias.
- `Func<@string, (@string, error)>` is Go's `func(string) (string, error)`. `@string` is Go's string type
  from golib, go2cs's hand-written runtime library ([Strings](#strings-string-and-sstring)). A C# tuple
  carries several results ([Multi-Result Values](#multi-result-values-and-comma-ok-forms)), and a func
  type becomes a `Func<…>` delegate ([Function Values and Closures](#function-values-and-closures)).
- Go's unnamed parameter is named `_` in C#.
- `[StackTraceHidden]` keeps the forwarder out of .NET stack traces, because in Go the two names are one
  function.
- `[GoInit]` marks Go's `init` function ([Package-Level Variable Initialization Order](#package-level-variable-initialization-order)).

The converter cannot tell from the directive alone whether the target has a C# body: in Go, many linkname
targets are themselves assembly. So it forwards only to targets on a list, kept inside the converter, of
functions known to have one. Any other pull keeps the throwing stub described at the end of this section.

The target in `time` is ordinary Go, marked with the one-argument directive. Each converted package is its
own .NET assembly, and a lower-case Go name is normally `internal`. A function that carries that marker
and is on the target list is emitted `public` instead, so the forwarder in `time/tzdata` can reach it:

<!-- source: GOROOT/src/time/zoneinfo_read.go:20-26 -->
```go
// registerLoadFromEmbeddedTZData is called by the time/tzdata package,
// if it is imported.
//
//go:linkname registerLoadFromEmbeddedTZData
func registerLoadFromEmbeddedTZData(f func(string) (string, error)) {
	loadFromEmbeddedTZData = f
}
```
<!-- source: src/core/time/zoneinfo_read.cs:19-25 -->
```csharp
// registerLoadFromEmbeddedTZData is called by the time/tzdata package,
// if it is imported.
//
//go:linkname registerLoadFromEmbeddedTZData
public static void registerLoadFromEmbeddedTZData(Func<@string, (@string, error)> f) {
    loadFromEmbeddedTZData = f;
}
```

**A `//go:linkname` push forwards too, or panics with a reason.** Package `unique` declares a bodyless
function under the one-argument marker. Package `runtime` supplies the body with a push:
`//go:linkname unique_runtime_registerUniqueMapCleanup unique.runtime_registerUniqueMapCleanup`.

<!-- source: GOROOT/src/unique/handle.go:176-179 -->
```go
// Implemented in runtime.

//go:linkname runtime_registerUniqueMapCleanup
func runtime_registerUniqueMapCleanup(cleanup func())
```

The bodyless declaration in `unique` becomes a forwarder to the converted body in `runtime`. `runtime` is
a `using` alias for that package's class, and `Action` is the C# delegate for Go's `func()`:

<!-- source: src/core/unique/handle.cs:8-164 -->
```csharp
using runtime = runtime_package;
…
// Implemented in runtime.

//go:linkname runtime_registerUniqueMapCleanup
[global::System.Diagnostics.StackTraceHidden] internal static void runtime_registerUniqueMapCleanup(Action cleanup) {
    runtime.unique_runtime_registerUniqueMapCleanup(cleanup);
}
```

The converter reads one package at a time, so it cannot see `runtime`'s directive while converting
`unique`. A curated list inside the converter records each push pair instead, and a pair not on the list
keeps the throwing stub. Some pushed bodies need runtime machinery go2cs does not model. For those, the
declaration raises a Go panic (see [Defer / Panic / Recover](#defer--panic--recover)) whose message names
both halves of the pair and the reason.

**An assembly trampoline forwards the same way, outside the standard library.** Some Go assembly only
jumps to another Go function, in one instruction. `golang.org/x/sys/unix` does this with
`JMP syscall·Syscall(SB)`. That says the two names are one function, so the converter writes a forwarder
like the linkname one. It does so only when the two signatures match, and only in packages outside Go's
standard library. Inside the standard library, hand-written files cover that assembly.

**In the standard library, `purego` removes many bodyless declarations.** Go often ships a portable Go file
beside an assembly one, and chooses between them with the `purego` build tag. go2cs converts the library
[as Go builds with `-tags purego`](#the-standard-library-reproduces-go--tags-purego), so the portable file
is the one converted, and the C# method has an ordinary body. This does not cover every case. `math`'s
`dim_asm.go` is chosen by CPU architecture alone, with no `purego` tag, so `archMax` stays bodyless and
needs its companion file.

**Anything left gets a throwing stub.** When no part implements a `partial` method, a
[source generator](#source-generators) adds one at compile time, so the project still builds. The stub
throws `NotImplementedException` with a message that starts with the function's name. The gap therefore
surfaces at the first call, not at build time.

A function whose assembly jumps into a C library, such as `syscall`'s `libc_getgroups_trampoline` on
macOS, keeps this stub too. Go code only takes that function's address, and never calls it. The converter
records which C function the jump names, so the address resolves to the real C function.

**Full detail:** [Reference → `//go:linkname` and assembly forwarders](ConversionStrategies-Reference/manual-conversions.md#a-cross-package-golinkname-pull-emits-a-forwarder-not-a-throwing-stub) — how pull and push forwarders are chosen and how they bridge types, the curated target lists, assembly-trampoline limits, cgo dynamic-import records, stub addresses, and the guard tests.

---

## Manually-Converted Declarations

Almost all of the C# in go2cs's converted standard library is written by the converter. A small set of Go
declarations becomes C# written by hand instead, and the converter never writes over it. These
*hand-owned* files and functions sit beside converted code and read as ordinary C#.

**Hand ownership is mostly a standard-library tool.** Every example in this section comes from go2cs's
converted standard library under `src/core`. A project of your own can use the whole-file form described
here: mark a converted `.cs` file, and later conversions leave it alone. The single-declaration form is a
list built into the converter, so it covers only the standard library.

**A declaration is hand-owned when Go's mechanism has no .NET equivalent.** Such Go code hides a pointer
inside an integer, reads an interface's internal machine words through `unsafe.Pointer`, calls a scheduler
primitive, or is written in assembly. On .NET, an [`any`](#empty-interface-any) is one object reference,
with no separate words to read. A Go pointer's target lives in a `ж<T>`, golib's heap box for a pointed-to
value; see [Pointers](#pointers). The .NET garbage collector can move or free that box. If Go code hid its
address inside an integer, the collector could not see it, so such code cannot be converted mechanically.
The hand-written C# keeps Go's observable behavior and drops Go's mechanism.

Hand ownership comes in two sizes: a whole file, or a single declaration.

**A whole file is hand-owned with `[module: go.GoManualConversion]`.** This attribute at the top of a C#
file tells the converter to leave the file alone. When the file stands in for a converted Go file, the
converter writes its own version beside it as `<name>.cs.auto`. The project does not compile `.cs.auto`
files; they only show what the automatic conversion would be.

`sync/atomic`'s `Value` is one such file. Go stores an `any` as two words, a type pointer and a data
pointer. Go's `Load` reinterprets the `Value` as an `efaceWords` and reads each of the two words with an
atomic load:

<!-- source: GOROOT/src/sync/atomic/value.go:16-40 -->
```go
type Value struct {
	v any
}

// efaceWords is interface{} internal representation.
type efaceWords struct {
	typ  unsafe.Pointer
	data unsafe.Pointer
}

// Load returns the value set by the most recent Store.
// It returns nil if there has been no call to Store for this Value.
func (v *Value) Load() (val any) {
	vp := (*efaceWords)(unsafe.Pointer(v))
	typ := LoadPointer(&vp.typ)
	if typ == nil || typ == unsafe.Pointer(&firstStoreInProgress) {
		// First store not yet completed.
		return nil
	}
	data := LoadPointer(&vp.data)
	vlp := (*efaceWords)(unsafe.Pointer(&val))
	vlp.typ = typ
	vlp.data = data
	return
}
```

The hand-written C# keeps the `any` in its one field. It reads the field with .NET's `Volatile.Read`, an
atomic, ordered load of one object reference:

<!-- source: src/core/sync/atomic/value.cs:6-35 -->
```csharp
[module: go.GoManualConversion]

namespace go.sync;

partial class atomic_package {

// A Value provides an atomic load and store of a consistently typed value.
// The zero value for a Value returns nil from [Value.Load].
//
// A Value must not be copied after first use.
[GoType] partial struct Value {
    internal any v;
}
…
[GoRecv] public static any /*val*/ Load(this ref Value v) {
    return Volatile.Read(ref v.v);
}
```

The file uses the same shapes the converter emits, so callers see no difference:
- `atomic_package` is the `static partial class` that holds the Go package's members, so a Go method
  becomes a C# extension method. See [Package Conversion](#package-conversion).
- `[GoType]` marks a Go type. `[GoRecv]` marks a method with a Go pointer receiver, written
  `this ref Value v`; a source generator adds the overload that takes a pointer. See
  [Functions and Methods](#functions-and-methods) and [Source Generators](#source-generators).

`Store` and `Swap` read the field with `Volatile.Read` and replace it with `Interlocked.CompareExchange`.
They keep Go's panics for a nil or differently typed value.

**A single declaration is hand-owned through a list in the converter.** The converter keeps a list of Go
types and functions, by package and name, that it does not emit. In each one's place it writes a one-line
placeholder comment. A hand-written `*_impl.cs` file in the same package supplies the C# declaration. A
converted package is one C# `partial class` spread over many files, so callers see no difference.

`runtime.Gosched` is one. Its Go body calls `mcall`, a runtime primitive that switches to the scheduler's
own stack. .NET has no such primitive:

<!-- source: GOROOT/src/runtime/proc.go:362-365 -->
```go
func Gosched() {
	checkTimeouts()
	mcall(gosched_m)
}
```

The converted `proc.cs` keeps only a placeholder where `Gosched` would be:

<!-- source: src/core/runtime/windows/proc.cs:351 -->
```csharp
// go2cs generated this placeholder — func Gosched is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])
```

The placeholder names the attribute that the `*_impl.cs` file carries. It is the same attribute as
`go.GoManualConversion`; `go` is its namespace. The hand-owned `managed_impl.cs` supplies the method:

<!-- source: src/core/runtime/managed_impl.cs:262-274 -->
```csharp
// Gosched yields the processor, allowing other goroutines to run. It does not suspend the
// current goroutine, so execution resumes automatically.
public static void Gosched()
{
    …
    golib.GoschedBackoff.Yield();
}
```

The omitted lines are a comment on why a plain thread yield is not enough on Linux. Each go2cs goroutine
is a .NET thread (see [Goroutines](#goroutines)), so yielding the processor means yielding the thread.
[golib](#the-golib-runtime-library), go2cs's hand-written runtime library, provides
`GoschedBackoff.Yield` for this. It yields the current thread, and sleeps briefly when a plain yield
lets nothing else run.

**A function with no Go body is filled in by a hand-owned file.** Go writes some functions in assembly.
Like any [function without a Go body](#functions-without-a-go-body), such a function converts to a C#
`partial` method with no body. A hand-owned `*_impl.cs` file supplies the body. When none does, a
[source generator](#source-generators) supplies one that throws, so the project still compiles.

On Linux, Go's system call wrappers all end in `Syscall6`, which Go writes in assembly:

<!-- source: GOROOT/src/internal/runtime/syscall/syscall_linux.go:15-16 -->
```go
// Syscall6 calls system call number 'num' with arguments a1-6.
func Syscall6(num, a1, a2, a3, a4, a5, a6 uintptr) (r1, r2, errno uintptr)
```
<!-- source: src/core/internal/runtime/syscall/linux/syscall_linux.cs:15-16 -->
```csharp
// Syscall6 calls system call number 'num' with arguments a1-6.
public static partial (uintptr r1, uintptr r2, uintptr errno) Syscall6(uintptr num, uintptr a1, uintptr a2, uintptr a3, uintptr a4, uintptr a5, uintptr a6);
```

The three results become a C# tuple; see
[Multi-Result Values](#multi-result-values-and-comma-ok-forms). `uintptr` is golib's type for Go's
`uintptr`, an integer the size of a pointer; see [`unsafe.Pointer` and `uintptr`](#unsafepointer-and-uintptr).

The hand-owned `syscall_linux_impl.cs` gives `Syscall6` its body. In the ordinary case it hands its
arguments to `rawSyscall6`, which calls glibc's `syscall(2)` through one .NET native binding,
`libc_syscall`:

<!-- source: src/core/internal/runtime/syscall/linux/syscall_linux_impl.cs:96-150 -->
```csharp
[LibraryImport("libc", EntryPoint = "syscall", SetLastError = true)]
private static partial nint libc_syscall(nint number, nint a1, nint a2, nint a3, nint a4, nint a5, nint a6);
…
private static nint ToNative(uintptr value) => unchecked((nint)(nuint)value);
…
private static (uintptr r1, uintptr r2, uintptr errno) rawSyscall6(uintptr num, uintptr a1, uintptr a2, uintptr a3, uintptr a4, uintptr a5, uintptr a6) {
    nint result = libc_syscall(ToNative(num), ToNative(a1), ToNative(a2), ToNative(a3), ToNative(a4), ToNative(a5), ToNative(a6));

    // r1 is the raw return in both outcomes: on failure libc returns -1 and so does Go's asm
    // (`MOVQ $-1, AX`), so the same expression serves both branches.
    uintptr r1 = new uintptr(unchecked((nuint)result));

    return result == -1
        ? (r1, default(uintptr), new uintptr((nuint)Marshal.GetLastPInvokeError()))
        : (r1, a3, default(uintptr));
}
```

`SetLastError = true` makes .NET keep libc's error number, which `Marshal.GetLastPInvokeError()` reads
back as `errno`. `r2` matches what Go's own assembly reports: zero on failure, and the unchanged third
argument on success. Converted Linux wrappers such as open, read or stat all run on that one binding.
Go's network poller works the same way: its runtime hooks have no Go body, and a hand-owned
`runtime_netpoll_impl.cs` per operating system implements them.

**A hand-owned file can be specific to one operating system.** Go picks a file per operating system by
its name, such as `_linux.go`, or by a `//go:build` line. go2cs puts each operating system's files,
converted or hand-owned, in the package's `windows/`, `linux/` and `darwin/` subfolders. An entry in the
converter's list can likewise name the operating systems it covers.

**The main hand-owned surfaces implement Go's contract on .NET primitives:**
- `sync.Pool`, and `sync.Cond`'s copy check, which in Go stores the `Cond`'s own address as an integer.
- `runtime.SetFinalizer`, `runtime.AddCleanup` and `weak.Pointer`, on .NET object lifetime.
- `runtime.GOMAXPROCS`, `Gosched`, `LockOSThread` and `Goexit`, on .NET threads; see [Goroutines](#goroutines).
- `runtime.GC` and `runtime.ReadMemStats`, on the .NET garbage collector.
- `time`'s timers.
- `hash/crc32`, on .NET hardware intrinsics.
- `reflect` and `internal/reflectlite`; see [Reflection](#reflection-reflect).
- The network poller's runtime hooks and Linux's `Syscall6`, at the operating-system boundary.

**Full detail:** [Reference → Manually-Converted Declarations](ConversionStrategies-Reference/manual-conversions.md#manually-converted-declarations) — every hand-owned surface and why, `//go:linkname` forwarders in both directions, the poller and system call internals on each OS, native bindings, how hand-owned files and list entries are scoped per platform, and the reflection bridge.

---

## The standard library reproduces Go `-tags purego`

Go writes some of its hottest crypto and hash routines in `.s` assembly files. The Go source only declares
those functions, without a body. A transpiler reads Go source and cannot convert assembly, so the converted
standard library reproduces Go built with `-tags purego`: Go's own switch for portable, pure-Go code.

**A build tag chooses which Go files are compiled.** A `//go:build` line at the top of a Go file is a
condition, much like a C# `#if` around the whole file. The Go toolchain compiles the file only when the
condition holds. Passing `-tags purego` makes the name `purego` true, so files gated on `purego` join the
build and files gated on `!purego` leave it.

**The default build binds SHA-256 to assembly.** On amd64, Go compiles this file. Its three `block*`
functions have no body, because their code lives in an assembly file. The `//go:noescape` lines are hints to
Go's compiler and do not matter here:

<!-- The `_amd64` filename suffix also limits this file to that architecture. Neither this file nor the
     noasm file is vendored in this repo; both are read from the Go 1.24.13 GOROOT. -->
<!-- source: GOROOT/src/crypto/internal/fips140/sha256/sha256block_amd64.go:5-39 -->
```go
//go:build !purego
…
//go:noescape
func blockAMD64(dig *Digest, p []byte)

//go:noescape
func blockAVX2(dig *Digest, p []byte)

//go:noescape
func blockSHANI(dig *Digest, p []byte)

func block(dig *Digest, p []byte) {
	if useSHANI {
		blockSHANI(dig, p)
	} else if useAVX2 {
		blockAVX2(dig, p)
	} else {
		blockAMD64(dig, p)
	}
}
```

**Under `purego`, a sibling file with a real body is chosen instead.** Its build line is long, but only the
`|| purego` at the end matters: it selects the file on every architecture. Its `block` calls `blockGeneric`,
SHA-256 written in plain Go:

<!-- source: GOROOT/src/crypto/internal/fips140/sha256/sha256block_noasm.go:5-11 -->
```go
//go:build (!386 && !amd64 && !arm64 && !loong64 && !ppc64 && !ppc64le && !riscv64 && !s390x) || purego

package sha256

func block(dig *Digest, p []byte) {
	blockGeneric(dig, p)
}
```

**Only the selected file is converted.** The assembly-backed file is left out entirely, just as Go leaves it
out. The C# keeps the Go build line as a comment. The package path's parent becomes the namespace (`@internal`
escapes a C# keyword), and the package itself becomes `partial class sha256_package`; see
[Package Conversion](#package-conversion). The pointer parameter `*Digest` becomes a C# `ref` parameter
([Pointers](#pointers)). `slice<byte>` is Go's `[]byte`, from golib, go2cs's hand-written C# runtime library
([Slices and Arrays](#slices-and-arrays)):

<!-- blockGeneric itself converts from sha256block.go: src/core/crypto/internal/fips140/sha256/sha256block.cs:81 -->
<!-- source: src/core/crypto/internal/fips140/sha256/sha256block_noasm.cs:4-13 -->
```csharp
//go:build (!386 && !amd64 && !arm64 && !loong64 && !ppc64 && !ppc64le && !riscv64 && !s390x) || purego
namespace go.crypto.@internal.fips140;

partial class sha256_package {

internal static void block(ref Digest dig, slice<byte> p) {
    blockGeneric(ref dig, p);
}

} // end sha256_package
```

**The same choice holds on every target.** The tag is part of the conversion, so every converted platform
runs the same portable code. The pure-Go path is usually slower than Go's hand-tuned assembly. It is the
path go2cs can convert and run.

**The default is two tags: `purego` and `math/big`'s own `math_big_pure_go`.** These are options of the
`go2cs` command itself. A `go2cs -stdlib` conversion (the whole standard library) or a `go2cs -tests`
conversion ([Converted Tests](#converted-tests)) applies `purego,math_big_pure_go` by default. `math/big`
names its portable fallback `math_big_pure_go` rather than `purego`, so both tags are needed.

**Your own tags always win.** Passing go2cs's `-tags` option replaces the default, and `-tags=` clears it.
Other conversions, such as `go2cs -recurse` on your own program, apply exactly the tags you pass and nothing
else.

<!-- The math_big_pure_go member: arith_decl.go (`!math_big_pure_go`) declares eight bodyless
     functions whose bodies are arith_$GOARCH.s; arith_decl_pure.go (`math_big_pure_go`) forwards each
     to the `_g` pure-Go implementation in arith.go. With purego alone, every big.Int/Float/Rat
     arithmetic path compiled and threw on first use (surfaced as time.TestTruncateRound -> big.Int.Mul
     -> mulAddVWW). Default set: ../src/go2cs/commandLineOptions.go:235 (defaultStdLibBuildTags);
     resolveBuildTags (commandLineOptions.go:250) keys the default on the -stdlib and -tests flags, not on
     the package being standard library, so a -tests run on any package gets it. `-tags=` clears the
     default. A -stdlib run prints the tags it applies, e.g.
     `Applying build tags: purego,math_big_pure_go (default; pass -tags to override)`
     (../src/go2cs/stdLibConverter.go:60); -tests shares the default but does not print it. -->

**An assembly-backed declaration ends in one of three ways:**

- **A `purego` sibling exists.** The tag selects the real body, as for SHA-256. This is the common case.
- **The code is gated on architecture alone.** Some files, such as `hash/crc32`'s amd64 file, have no
  `purego` alternative. Hand-written C# supplies the body, as a companion file or a whole-file replacement;
  see [Manually-Converted Declarations](#manually-converted-declarations).
- **Nothing supplies a body.** The declaration compiles, but throws a `NotImplementedException` naming the
  function when it is called; see [Functions Without a Go Body](#functions-without-a-go-body).

<!-- hash/crc32: crc32_amd64.go has no //go:build line; its only constraint is the `_amd64` filename
     suffix, so `-tags purego` selects the same files as the default build. Its hand-owned whole-file
     replacement is ../src/core/hash/crc32/crc32_amd64.cs. -->

**Behavior matches the `purego` build, including its gaps.** go2cs converts what Go builds under `purego` and
does not patch it. Under `purego`, P-256's scalar inverse has no fast implementation, so Go selects a stub
that returns an error:

<!-- source: GOROOT/src/crypto/internal/fips140/nistec/p256_ordinv_noasm.go:5-13 -->
```go
//go:build (!amd64 && !arm64) || purego

package nistec

import "errors"

func P256OrdInverse(k []byte) ([]byte, error) {
	return nil, errors.New("unimplemented")
}
```

In the C#, the import becomes a `using` alias ([Package Conversion](#package-conversion)). The string
literal is hoisted into a static field whose name ends in `ˢ`. Its type `@string` is golib's Go string,
built from `"…"u8`, a C# UTF-8 byte literal ([Strings](#strings-string-and-sstring)). The two results
become a C# tuple ([Multi-Result Values](#multi-result-values-and-comma-ok-forms)), and `default!` is Go's
`nil` ([Nil and Zero Values](#nil-and-zero-values)):

<!-- source: src/core/crypto/internal/fips140/nistec/p256_ordinv_noasm.cs:4-18 -->
```csharp
//go:build (!amd64 && !arm64) || purego
namespace go.crypto.@internal.fips140;

using errors = errors_package;

partial class nistec_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string unimplementedˢ = "unimplemented"u8;

public static (slice<byte>, error) P256OrdInverse(slice<byte> k) {
    return (default!, errors.New(unimplementedˢ));
}

} // end nistec_package
```

**`crypto/elliptic` then panics, just as Go does.** The P-256 curve's `Inverse` method in `crypto/elliptic`
treats that error as impossible and panics. Real Go built with `-tags purego` panics here too, so the
converted code does the same. `crypto/ecdsa` handles the same error with a fallback, so ECDSA is unaffected:

<!-- source: GOROOT/src/crypto/elliptic/nistec_p256.go:24-27 -->
```go
	inverse, err := nistec.P256OrdInverse(scalar)
	if err != nil {
		panic("crypto/elliptic: nistec rejected normalized scalar")
	}
```

A Go `panic` becomes `throw panic(...)` ([Defer / Panic / Recover](#defer--panic--recover)). A literal passed
straight to `panic` stays inline as a plain C# string instead of becoming a hoisted `ˢ` field. The tuple
result is unpacked with `var (inverse, err) = …`:

<!-- source: src/core/crypto/elliptic/nistec_p256.cs:26-29 -->
```csharp
    var (inverse, err) = nistec.P256OrdInverse(scalar);
    if (err != default!) {
        throw panic("crypto/elliptic: nistec rejected normalized scalar");
    }
```

<!-- Upstream gating: crypto/elliptic/nistec_p256.go is `amd64 || arm64` with no `!purego`, while
     crypto/internal/fips140/nistec/p256_ordinv.go is `(amd64 || arm64) && !purego`; under purego
     p256_ordinv_noasm.go returns errors.New("unimplemented") and Inverse panics with
     `crypto/elliptic: nistec rejected normalized scalar`. Checked against the Go 1.24.13 tree; the
     panic text is in ../src/core/crypto/elliptic/nistec_p256.cs:28. The default (-tags=) build binds the
     asm ordinv and returns a valid inverse; crypto/ecdsa handles the same error with a fallback, so
     ECDSA is unaffected. Inverse is reached via the deprecated `invertible` interface. The inline panic
     literal is deliberate: the converter exempts `panic`'s argument from literal hoisting
     (src/go2cs/hoistedLiteralOperations.go:612-614), since it costs nothing until a panic fires. -->

**A module converts with `-tags safe`, for a different reason.** Some libraries reach into Go's runtime with
unsafe pointer arithmetic, adding a byte offset to a value to read one of its private fields. Converted code
keeps its values in .NET's own layout, so golib refuses that arithmetic rather than read the wrong memory.
Libraries that do this usually ship a fallback for builds tagged `safe`, so a `-recurse` conversion adds the
tag and the fallback is what converts. When a module is validated, Go's own `go test` run gets the same tags,
so the two sides compile the same files. The tag `appengine`, which some libraries read the same way, is not
added, because it also changes unrelated behavior. `-module-safe-tag=false` turns `safe` off. See
[Reference → Default build tags](ConversionStrategies-Reference/package-conversion.md#default-build-tags-purego-for-the-standard-library-safe-for-modules).

**Full detail:** [Reference → The standard-library conversion applies `-tags purego`](ConversionStrategies-Reference/purego.md#the-standard-library-conversion-applies--tags-purego) — why the tag is on by default and the alternatives weighed, how `-tests` shares it, the `math/big` fallback tag, the three outcomes with more packages named, and the `crypto/elliptic` gating in full.

---

## Comments

A Go comment that reaches the C# is copied word for word. A `//` line stays a `//` line, and a `/* … */` block stays a block. Most Go comments reach the C# only when you ask for them. Two kinds always appear: the license header, and short notes that the converter writes itself to explain a conversion choice.

**Go's comments appear only with `-comments`.** The flag is off by default, so an ordinary conversion drops Go's comments, except the license header. Pass the flag on the command line, as in `go2cs -comments <package_dir> <output_dir>`. The standard library is converted with `go2cs -stdlib -comments`, and [converted tests](#converted-tests), Go `_test.go` files converted with `go2cs -tests`, always turn it on.

The other sections of this page quote go2cs's behavioral tests: small Go programs in the go2cs repository, each stored with the C# the converter emits for it. Those programs are converted without `-comments`, which is why their C# shows no Go comments.

**The license header always survives.** A comment group ahead of `package` that mentions a copyright, a license or an SPDX tag is copied to the top of the C# file. The converted file is a derivative work of the Go source, so its notice travels with it. This program is converted without `-comments`: the header is kept, and the trailing comments on the two constants are dropped.

<!-- source: src/tests/Behavioral/FirstClassFunctions/FirstClassFunctions.go:1-15 -->
```go
// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

package main

import (
	"fmt"
	"math/rand"
)

const (
	win            = 100 // The winning score in a game of Pig
	gamesPerSeries = 10  // The number of games per series to simulate
)
```
<!-- source: src/tests/Behavioral/FirstClassFunctions/FirstClassFunctions.cs.target:1-15 -->
```csharp
// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

namespace go;

using fmt = fmt_package;
using rand = math.rand_package;
using math;
using ꓸꓸꓸnint = Span<nint>;

partial class main_package {

internal static UntypedInt win => 100;
internal static UntypedInt gamesPerSeries => 10;
```

The header lands above the `namespace` line. The `using` lines for the imports follow it, then `partial class main_package`, the class that holds the package's members ([Package Conversion](#package-conversion)). `ꓸꓸꓸnint` names the type of a variadic `...int` parameter used later in the file ([Slices and Arrays](#slices-and-arrays)). `UntypedInt` is the type golib, the go2cs runtime library, uses for an untyped Go constant ([Constant Values](#constant-values)).

**The converter's own notes always appear.** They are written with or without `-comments`, because they explain the C# rather than repeat the Go. In the same program, a named func type `action` gets no C# declaration of its own, and a converter note takes its place:

<!-- source: src/tests/Behavioral/FirstClassFunctions/FirstClassFunctions.go:23-44 -->
```go
// An action transitions stochastically to a resulting score.
type action func(current score) (result score, turnIsOver bool)
…
// A strategy chooses an action for any given score.
type strategy func(score) action
```
<!-- source: src/tests/Behavioral/FirstClassFunctions/FirstClassFunctions.cs.target:21-35 -->
```csharp
// type action is a methodless func type — rendered inline as its base delegate
…
internal delegate Func<score, (score, bool)> strategy(score _);
```

The note means that each use of `action` is written as the C# delegate type of the same shape, `Func<score, (score, bool)>`. You can see it as the result type of `strategy`. `strategy` keeps a `delegate` declaration because its signature names another func type ([Function Values and Closures](#function-values-and-closures)). Go's doc comments on both types are dropped, because this program is converted without `-comments`.

Another common note heads a group of [hoisted string literals](#strings-string-and-sstring). A hoisted literal is a string literal stored once in a `static readonly` field, whose generated name ends in `ˢ`:

<!-- source: src/tests/Behavioral/FirstClassFunctions/FirstClassFunctions.go:189-194 -->
```go
func joinPair(s string, err error) string {
	if err != nil {
		return "err"
	}
	return "got:" + s
}
```
<!-- source: src/tests/Behavioral/FirstClassFunctions/FirstClassFunctions.cs.target:168-176 -->
```csharp
// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string errˢ = "err"u8;

internal static @string joinPair(@string s, error err) {
    if (err != default!) {
        return errˢ;
    }
    return "got:"u8 + s;
}
```

`@string` is golib's Go string type, an immutable string of UTF-8 bytes, and `"err"u8` is a C# UTF-8 string literal. RODATA is the read-only data section of a compiled Go program, where Go keeps its literals. `default!` is Go's `nil` ([Nil and Zero Values](#nil-and-zero-values)). Which literals are hoisted is explained in [Strings](#strings-string-and-sstring).

**With `-comments`, doc comments stay above their declarations.** They stay plain `//` lines rather than becoming C# XML documentation comments. That keeps Go's doc links, like `[RuneError]`, reading exactly as in Go. A comment on its own line inside a function also keeps its line, indented with its block:

<!-- source: Go toolchain src/unicode/utf8/utf8.go:345-355 (Go 1.24.13; the Go source of src/core/unicode/utf8/utf8.cs, which the repository does not hold) -->
```go
// EncodeRune writes into p (which must be large enough) the UTF-8 encoding of the rune.
// If the rune is out of range, it writes the encoding of [RuneError].
// It returns the number of bytes written.
func EncodeRune(p []byte, r rune) int {
	// This function is inlineable for fast handling of ASCII.
	if uint32(r) <= rune1Max {
		p[0] = byte(r)
		return 1
	}
	return encodeRuneNonASCII(p, r)
}
```
<!-- source: src/core/unicode/utf8/utf8.cs:366-376 -->
```csharp
// EncodeRune writes into p (which must be large enough) the UTF-8 encoding of the rune.
// If the rune is out of range, it writes the encoding of [RuneError].
// It returns the number of bytes written.
public static nint EncodeRune(slice<byte> p, rune r) {
    // This function is inlineable for fast handling of ASCII.
    if ((uint32)r <= rune1Max) {
        p[0] = (byte)r;
        return 1;
    }
    return encodeRuneNonASCII(p, r);
}
```

Reading the C#: `nint` is Go's `int`, and `slice<byte>` is Go's `[]byte` ([Slices and Arrays](#slices-and-arrays)). `rune` and `uint32` keep Go's names as aliases for C# `int` and `uint`, declared in each project file ([Integer Types and Arithmetic](#integer-types-and-arithmetic)). `rune1Max` is a constant, and `encodeRuneNonASCII` a function, of the same package.

**A trailing comment stays on its line.** A struct field's trailing comment stays on the field's line:

<!-- source: Go toolchain src/unicode/utf8/utf8.go:92-97 (Go 1.24.13; the Go source of src/core/unicode/utf8/utf8.cs, which the repository does not hold) -->
```go
// acceptRange gives the range of valid values for the second byte in a UTF-8
// sequence.
type acceptRange struct {
	lo uint8 // lowest value for second byte.
	hi uint8 // highest value for second byte.
}
```
<!-- source: src/core/unicode/utf8/utf8.cs:97-102 -->
```csharp
// acceptRange gives the range of valid values for the second byte in a UTF-8
// sequence.
[GoType] partial struct acceptRange {
    internal uint8 lo; // lowest value for second byte.
    internal uint8 hi; // highest value for second byte.
}
```

`[GoType]` marks a converted Go type that a [source generator](#source-generators) completes ([Struct Types](#struct-types)). `uint8` is an alias for C# `byte`, declared in each project file ([Integer Types and Arithmetic](#integer-types-and-arithmetic)).

A statement's trailing comment follows the statement's last C# line, after one space. It does not keep Go's column alignment, because the converted lines have different lengths. Here `first` is a lookup table and `xx` a constant of the same package:

<!-- source: Go toolchain src/unicode/utf8/utf8.go:467-470 (Go 1.24.13; the Go source of src/core/unicode/utf8/utf8.cs, which the repository does not hold) -->
```go
		x := first[pi]
		if x == xx {
			return false // Illegal starter byte.
		}
```
<!-- source: src/core/unicode/utf8/utf8.cs:507-510 -->
```csharp
        var x = first[pi];
        if (x == xx) {
            return false; // Illegal starter byte.
        }
```

**Comment conversion is best effort.** Most comments land where Go put them, but not every converted comment reaches its desired output location, and a few are dropped. The code behaves the same either way; only the comments differ.

The reason is how Go's parser records comments. It attaches a comment only to a declaration, a struct field or a spec (one entry in a `const`, `var`, `type` or `import` group). A comment anywhere else, such as one inside a function body, is attached to nothing. A comment on the same line as the end of a statement is written after that statement. Any other loose comment is held and written at the next point where the converter writes loose comments, usually just before a later statement.

A comment after the `{` of an `if` or `for` header moves to its own line, at the top of the block. Here `d.nd` is the digit count of the number being formatted:

<!-- source: Go toolchain src/strconv/ftoa.go:411-413 (Go 1.24.13; the Go source of src/core/strconv/ftoa.cs, which the repository does not hold) -->
```go
	if d.nd == 0 { // special case: 0 has exponent 0
		exp = 0
	}
```
<!-- source: src/core/strconv/ftoa.cs:453-456 -->
```csharp
    if (d.nd == 0) {
        // special case: 0 has exponent 0
        exp = 0;
    }
```

A comment after a `case` label, or after the `{` of a `switch` header, is the clearest miss. It is not written inside the case. It waits for the next point where the converter writes loose comments, and that point can be well after the `switch`. Here the comments `// normal rune` and `// needs surrogate sequence` label two cases in Go. In the C# they land after the enclosing loop has closed, just ahead of the `return`:

<!-- source: Go toolchain src/unicode/utf16/utf16.go:68-95 (Go 1.24.13; the Go source of src/core/unicode/utf16/utf16.cs, which the repository does not hold) -->
```go
// Encode returns the UTF-16 encoding of the Unicode code point sequence s.
func Encode(s []rune) []uint16 {
	n := len(s)
	for _, v := range s {
		if v >= surrSelf {
			n++
		}
	}

	a := make([]uint16, n)
	n = 0
	for _, v := range s {
		switch RuneLen(v) {
		case 1: // normal rune
			a[n] = uint16(v)
			n++
		case 2: // needs surrogate sequence
			r1, r2 := EncodeRune(v)
			a[n] = uint16(r1)
			a[n+1] = uint16(r2)
			n += 2
		default:
			a[n] = uint16(replacementChar)
			n++
		}
	}
	return a[:n]
}
```
<!-- source: src/core/unicode/utf16/utf16.cs:64-98 -->
```csharp
// Encode returns the UTF-16 encoding of the Unicode code point sequence s.
public static slice<uint16> Encode(slice<rune> s) {
    nint n = len(s);
    foreach (var (_, v) in s) {
        if (v >= surrSelf) {
            n++;
        }
    }
    var a = new slice<uint16>(n);
    n = 0;
    foreach (var (_, v) in s) {
        switch (RuneLen(v)) {
        case 1: {
            a[n] = (uint16)v;
            n++;
            break;
        }
        case 2: {
            var (r1, r2) = EncodeRune(v);
            a[n] = (uint16)r1;
            a[n + 1] = (uint16)r2;
            n += 2;
            break;
        }
        default: {
            a[n] = (uint16)replacementChar;
            n++;
            break;
        }}

    }
    // normal rune
    // needs surrogate sequence
    return a[..(int)(n)];
}
```

`RuneLen`, `EncodeRune`, `surrSelf` and `replacementChar` are functions and constants of the same package. `rune` and `uint16` are aliases for C# `int` and `ushort` ([Integer Types and Arithmetic](#integer-types-and-arithmetic)). `foreach` is Go's `range` loop ([Loops, Range and Labels](#loops-range-and-labels)), and `new slice<uint16>(n)` and `a[..(int)(n)]` are Go's `make` and `a[:n]` ([Slices and Arrays](#slices-and-arrays)). Each case body gets its own braces and a `break`, and the `}}` plus the blank line after it is how the converter lays out every switch ([Expression Switch Statements](#expression-switch-statements)).

A comment on its own line inside a `case` body can even land inside the argument list of a later statement. The code is unchanged, because a `//` comment runs only to the end of its line:

<!-- source: Go toolchain src/unicode/utf16/utf16.go:97-112 (Go 1.24.13; the Go source of src/core/unicode/utf16/utf16.cs, which the repository does not hold) -->
```go
// AppendRune appends the UTF-16 encoding of the Unicode code point r
// to the end of p and returns the extended buffer. If the rune is not
// a valid Unicode code point, it appends the encoding of U+FFFD.
func AppendRune(a []uint16, r rune) []uint16 {
	// This function is inlineable for fast handling of ASCII.
	switch {
	case 0 <= r && r < surr1, surr3 <= r && r < surrSelf:
		// normal rune
		return append(a, uint16(r))
	case surrSelf <= r && r <= maxRune:
		// needs surrogate sequence
		r1, r2 := EncodeRune(r)
		return append(a, uint16(r1), uint16(r2))
	}
	return append(a, replacementChar)
}
```
<!-- source: src/core/unicode/utf16/utf16.cs:100-117 -->
```csharp
// AppendRune appends the UTF-16 encoding of the Unicode code point r
// to the end of p and returns the extended buffer. If the rune is not
// a valid Unicode code point, it appends the encoding of U+FFFD.
public static slice<uint16> AppendRune(slice<uint16> a, rune r) {
    // This function is inlineable for fast handling of ASCII.
    switch (ᐧ) {
    case {} when (0 <= r && r < surr1) || (surr3 <= r && r < surrSelf): {
        return append(a, // normal rune
 (uint16)r);
    }
    case {} when surrSelf <= r && r <= maxRune: {
        var (r1, r2) = EncodeRune(r);
        return append(a, // needs surrogate sequence
 (uint16)r1, (uint16)r2);
    }}

    return append(a, (uint16)(replacementChar));
}
```

A Go `switch` with no tag becomes `switch (ᐧ)`, where `ᐧ` is golib's constant `true`, and each case becomes `case {} when <condition>` ([Expression Switch Statements](#expression-switch-statements)). `append` is Go's built-in, provided by golib ([Built-in Functions](#built-in-functions)). `surr1`, `surr3` and `maxRune` are constants of the same package.

A comment written above one constant inside a `const ( … )` group is dropped. A doc comment directly above `const (` is kept, because Go attaches it to the whole group:

<!-- source: Go toolchain src/unicode/utf16/utf16.go:17-26 (Go 1.24.13; the Go source of src/core/unicode/utf16/utf16.cs, which the repository does not hold) -->
```go
const (
	// 0xd800-0xdc00 encodes the high 10 bits of a pair.
	// 0xdc00-0xe000 encodes the low 10 bits of a pair.
	// the value is those 20 bits plus 0x10000.
	surr1 = 0xd800
	surr2 = 0xdc00
	surr3 = 0xe000

	surrSelf = 0x10000
)
```
<!-- source: src/core/unicode/utf16/utf16.cs:16-19 -->
```csharp
internal static UntypedInt surr1 => 0xd800;
internal static UntypedInt surr2 => 0xdc00;
internal static UntypedInt surr3 => 0xe000;
internal static UntypedInt surrSelf => 0x10000;
```

**Full detail:** [Reference → Comments](ConversionStrategies-Reference/comments.md#comments) — how attached and free-floating comments are told apart, which statement positions take a trailing comment, multi-line block comments, and the shapes that still land away from where Go put them.

---

## Packages That Do Not Type-Check

Go's type checker gives every name and expression in a package a type. When it cannot resolve a name,
the package does not fully type-check. go2cs still converts such a package, and the run goes on. Only
the statements that use the missing name are affected. The rest of the file and the package convert
normally.

This mostly happens in application code. `go2cs -recurse` converts a program's module and the
third-party packages it imports, each into its own C# project ([Package Conversion](#package-conversion)).
Some names in those packages may not resolve on the machine doing the conversion.

For example, a function may be defined only in a `_linux.go` file, and Go skips that file when the
conversion runs on Windows. Or a dependency's own import may have failed to resolve. Go's standard
library always type-checks.

**The converter reports, then converts.** The functions shown here use two names that exist nowhere:
`undefinedFunc` and `UndefinedType`. The full test package has a few more unresolved names, such as
`fmt.NoSuchFunction`. The `healthy` function uses nothing missing.

<!-- source: src/go2cs/untypedPackageConversion_test.go:74-107 -->
```go
func addressTaken() {
	x := 1
	undefinedFunc(&x)
	fmt.Println(x)
}
…
func undefinedType() {
	var u UndefinedType
	w := &UndefinedType{Field: 3}
	fmt.Println(u, w)
}
…
func healthy() {
	total := 0
	for i := range 4 {
		total += i * i
	}
	fmt.Println("healthy:", total)
}
```

The converter prints one warning for the package, then converts every function in it. This is the
line that prints the warning:

<!-- source: src/go2cs/conversionDriver.go:224 -->
```text
WARNING: %s did not fully type-check; converting best-effort — code depending on the following is emitted untyped: %v
```

On the console, `%s` becomes the package's import path, such as `example.com/app`. `%v` becomes the
list of errors Go reported. Each error names a file, a line and the name that did not resolve.
"Emitted untyped" means Go recorded no type for that code, so the converter writes it without one.

**Healthy code converts in full.** `healthy` becomes an ordinary static method of the package class,
the same as in a package with no errors ([Functions and Methods](#functions-and-methods)).
`addressTaken` and `undefinedType` become methods too. The converter does not drop the file or the
package around them.

**Taking an address still converts.** `addressTaken` is in the example for a reason. When code takes
the address of a local such as `&x`, the converter checks how that address is used to decide whether
`x` needs a heap box ([Pointers](#pointers)). Here the callee has no type to check, and the converter
carries on without it.

**A missing type is written as `invalid type`.** Where a type is missing, the converter writes the
literal text `invalid type`. That is how Go's type checker prints a type it could not resolve. So
`var u UndefinedType` emits `invalid type u = default!`.

`default!` is C#'s `default` (the zero value) with the null-forgiving `!`, as converted code writes it
everywhere ([Nil and Zero Values](#nil-and-zero-values)). The rest of the code that uses a missing
name is written as close to the Go as the converter can manage.

**The C# build stops at those lines.** `invalid type` contains a space, so it is not valid C#.
Nothing in C# declares `undefinedFunc` either. The C# compiler reports errors on the code that uses
the missing names.

The project as a whole does not build until those lines are fixed. Everything else in it is already
converted correctly. The fix belongs in the Go source or its build setup, followed by a fresh
conversion.

**A converter fault stays inside one file.** If the converter itself fails while converting a file, it
skips that file. It prints a warning such as `WARNING: visit file error: <fault> in "main.go"`. The
package's other files still convert.

**A failed package stays inside a `-recurse` run.** A package that cannot be loaded at all, or whose
conversion fails, is recorded as failed. The next package then proceeds. At the end, the run names any
packages that failed.

**The standard-library conversion stops instead.** `go2cs -stdlib` converts Go's own standard library
in import order, so each package converts after the packages it depends on. A package that cannot be
loaded has no known place in that order. The conversion stops and lists every package that failed to
load, all at once.

**Full detail:** [Reference → Packages That Do Not Type-Check](ConversionStrategies-Reference/packages-that-do-not-type-check.md#packages-that-do-not-type-check) — why an unresolved expression has no type, how the converter tolerates it, how one package's fault stays inside that package, and the guard tests.

---

## Deterministic Output

Converting the same Go source with the same converter build, build tags and target platform produces
byte-identical C# every run. Build tags and the target platform matter because they choose which Go files
convert, such as `file_windows.go` or `file_linux.go` ([Package Conversion](#package-conversion)). Any
change in the converted C# comes from a change in the Go, its dependencies, the converter, the build tags
or the target platform.

So converted code can be diffed, committed and reviewed like any other source. go2cs's own golden tests
rely on this. Each converted file is compared with its checked-in `.cs.target` file, byte for byte apart
from line endings.

The converter is a Go program, and some things in Go vary from run to run, such as the order a map is
walked. Three rules keep the converter's output stable.

**Files convert one at a time, in sorted filename order.** Some generated names are numbered by one
counter shared across the whole package. So the order the files convert in decides the numbers.

Go's `init` function is the case a reader meets most often. Go allows any number of `init` functions in a
package, but C# needs a distinct name for each method. The first `init` keeps its name, and each later one
becomes `initΔ1`, `initΔ2` and so on. `Δ` marks a renamed name
([Names and Glyphs](#reading-converted-code-names-and-glyphs)).

Package-level declarations named `_` are numbered the same way, with names built from `_ᴛ1`, `_ᴛ2` and so
on, because C# cannot declare two members named `_`. `ᴛ` marks a name the converter makes up.

This test package has two Go files with two `init` functions each. `a_first.go` sorts first:

<!-- source: src/tests/Behavioral/MultiFileInitOrder/a_first.go:8-16 -->
```go
var order []string

func init() {
	order = append(order, "a_first#1")
}

func init() {
	order = append(order, "a_first#2")
}
```
<!-- source: src/tests/Behavioral/MultiFileInitOrder/a_first.cs.target:5-13 -->
```csharp
internal static slice<@string> order;

[GoInit] internal static void init() {
    order = append(order, "a_first#1"u8);
}

[GoInit] internal static void initΔ1() {
    order = append(order, "a_first#2"u8);
}
```

A few names in this C# come from golib, the go2cs runtime library
([The golib Runtime Library](#the-golib-runtime-library)):

* `slice<@string>` is Go's `[]string`: a golib slice of golib strings
  ([Slices and Arrays](#slices-and-arrays), [Strings](#strings-string-and-sstring)).
* `append` is golib's version of Go's built-in `append` ([Built-in Functions](#built-in-functions)).
* `"a_first#1"u8` is a C# UTF-8 string literal. It becomes an `@string`, which holds bytes as Go's
  string does.
* `[GoInit]` marks a method that runs as package initialization, before `Main`
  ([Package Conversion](#package-conversion)).

`b_second.go` converts next, so its numbering continues from `a_first.go` and starts at `initΔ2`:

<!-- source: src/tests/Behavioral/MultiFileInitOrder/b_second.go:3-9 -->
```go
func init() {
	order = append(order, "b_second#1")
}

func init() {
	order = append(order, "b_second#2")
}
```
<!-- source: src/tests/Behavioral/MultiFileInitOrder/b_second.cs.target:5-11 -->
```csharp
[GoInit] internal static void initΔ2() {
    order = append(order, "b_second#1"u8);
}

[GoInit] internal static void initΔ3() {
    order = append(order, "b_second#2"u8);
}
```

The package's third file, `main.go`, sorts last, so its one `init` becomes `initΔ4`:

<!-- source: src/tests/Behavioral/MultiFileInitOrder/main.go:5-7 -->
```go
func init() {
	order = append(order, "main#1")
}
```
<!-- source: src/tests/Behavioral/MultiFileInitOrder/main.cs.target:7-9 -->
```csharp
[GoInit] internal static void initΔ4() {
    order = append(order, "main#1"u8);
}
```

If the files converted in a different order, the same functions would get different numbers. The numbers
only name the methods. Go runs `init` functions file by file in sorted filename order, then in source order
within a file. This test's `main` prints the `order` list, and the converted program prints the same list
as Go. For how package-level variables get their starting values, see
[Package-Level Variable Initialization Order](#package-level-variable-initialization-order).
<!-- main.go:9-15 prints len(order) then each name; the test carries [GoTestMatchingConsoleOutput] (package_info.cs), so the C# output must equal Go's: a_first#1, a_first#2, b_second#1, b_second#2, main#1. File-order rule: comment at a_first.go:1-5. -->

**Packages convert in dependency order.** A run can convert many packages at once: the whole standard
library, or an application together with its third-party dependencies. Each package then converts after
the packages it imports.

This matters because an importer reads each dependency's converted `package_info.cs`. That generated file
records package-wide names, such as the aliases for renamed types ([Package Conversion](#package-conversion)).
So the dependency must be finished first. Packages with no dependency between them go in sorted
import-path order, so the queue is the same every run.
<!-- one shared graph orders both the -stdlib and -recurse drivers: src/go2cs/dependencyGraph.go:9-14 (shared core), :154-212 (no-dependency roots sorted first, then a depth-first visit from sorted roots); src/go2cs/moduleConverter.go:9-13, :111. -->

**Names the converter collects in a map are sorted before they are written.** Go's map iteration order
changes from run to run. Anything the converter gathers in a map is sorted first. Without this, two lines
that are correct in either order could swap places between runs, and the file would show a diff for no
reason.

Here is one place this shows. In Go's `math/big` package, `(*Int).And` swaps its two pointer
parameters:

<!-- source: GOROOT/src/math/big/int.go:1171-1191 -->
```go
func (z *Int) And(x, y *Int) *Int {
…
	// x.neg != y.neg
	if x.neg {
		x, y = y, x // & is symmetric
	}
…
```

In the C#, a Go pointer `*Int` is a `ж<ΔInt>`: a golib heap box holding an `Int` value
([Pointers](#pointers)). The type is renamed `ΔInt` because `math/big` also has a method named `Int`, on
`*Float`. C# cannot use one name for both a type and a method in the same package class.

A Go method becomes a C# extension method, which is why the receiver `Ꮡz` has `this`
([Functions and Methods](#functions-and-methods)). A parameter that holds a box takes the `Ꮡ` prefix, as
in `Ꮡx`.

The body also declares `x`, a C# `ref` alias to the value inside the box. This lets the body write
`x.neg` just as the Go does ([Implicit Pointer Dereferencing](#implicit-pointer-dereferencing)).
`DerefOrNull()` returns that reference. For a nil pointer, it lets the first use panic, as Go does.

<!-- source: src/core/math/big/int.cs:1303-1325 -->
```csharp
public static ж<ΔInt> And(this ж<ΔInt> Ꮡz, ж<ΔInt> Ꮡx, ж<ΔInt> Ꮡy) {
    ref var z = ref Ꮡz.DerefOrNull();
    ref var x = ref Ꮡx.DerefOrNull();
    ref var y = ref Ꮡy.DerefOrNull();
…
    // x.neg != y.neg
    if (x.neg) {
        (Ꮡx, Ꮡy) = (Ꮡy, Ꮡx); x = ref Ꮡx.DerefOrNull(); y = ref Ꮡy.DerefOrNull(); // & is symmetric
    }
…
```
<!-- ΔInt: type at src/core/math/big/int.cs:32 inside partial class big_package (:13); the colliding method is (*Float).Int, GOROOT/src/math/big/float.go:1080 -> src/core/math/big/float.cs:1115. DerefOrNull on a nil box returns Unsafe.NullRef: src/core/golib/ж.PointerExtensions.cs:445. -->

The swap exchanges the two boxes, so the aliases `x` and `y` must then be pointed at the new boxes. Either
order of those two statements is correct C#. The converter collects them in a map and writes them in
sorted name order. So `x` always comes before `y`, and the line is the same every run.

**Full detail:** [Reference → Deterministic Output](ConversionStrategies-Reference/deterministic-output.md#deterministic-output) — which shared converter state each rule protects, and the unstable or broken output it prevents.
<!-- {% endraw %} -->
