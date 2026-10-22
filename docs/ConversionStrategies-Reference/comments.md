# Comments

[Reference index](README.md) · [Summary of this topic](../ConversionStrategies.md#comments)

Two things are converted here, under different switches.

- **Blank lines inside a function body, and the one after the converter's prelude, follow Go in every
  conversion.**
- **Comments are carried only with `-comments`** (default off). The standard-library conversion turns it on,
  because the converted C# is a derivative work whose per-file `// Copyright … The Go Authors` header must
  survive and because Go's doc comments are most of what makes the output readable. So does `-tests`, and the
  [Tour of go2cs](../../src/tour/README.md), which converts the lesson text a reader is looking at. A plain
  conversion drops comments.

Behavioral goldens are captured **without** `-comments`, so they show the blank-line rules and never a comment.
The comment rules are guarded by converter tests that assert positions on fresh output, and by a census that
compares a whole corpus with its Go source (see [Guarded by](#guarded-by)).

## Where a comment is attached, and where it is not

Go's AST attaches a comment to a node only in specific places: a declaration's `Doc`, a struct field's
`Doc` and `Comment`, a spec's `Doc` and `Comment`. **Statements have no comment field.** A comment inside a
function body is free-floating: it exists in `File.Comments` and nowhere else.

The converter keeps every comment group, attached or not, with its Go position, and the emitter claims each
free one at the point that matches where Go put it: after a statement, after a `{`, on or above a `case`
label, before a `}`, between list items, or between operands. Each placement below is one such point. A
comment no point claims is written at the next place the converter writes loose comments, on its own line;
that fallback is what [Limits](#limits-where-a-comment-still-lands-elsewhere) describes.

<a id="the-rule"></a>

## Blank lines

### Between the statements of a body, Go's blank lines are kept

**Rule.** Where Go has one or more blank lines between two statements of a list, the C# has exactly one;
where Go has none, the C# adds none. A list is a block body, a `case` or type-switch clause body, or a
`select` clause body. There are never two blank lines in a row, never one right after `{`, and never one
right before `}`. A Go comment between the two statements does not count as a blank line; in a conversion
without `-comments`, a gap that holds only dropped comments still gives one blank line when Go had one.

This applies to every conversion. A behavioral golden, converted without `-comments`; the comment between
the two groups is dropped and leaves no extra blank behind:

<!-- source: src/tests/Behavioral/FirstClassFunctions/FirstClassFunctions.go:126-139 -->
```go
	converted := (func(score) (score, bool))(stay)
	result, turnIsOver := converted(score{player: 3, opponent: 5, thisTurn: 7})
	fmt.Println("converted func type call:", result.player, result.opponent, result.thisTurn, turnIsOver)

	// A call through a NAMED func-type FIELD deconstructs as one tuple call - the named
	// type's underlying signature carries the result count (bufio Scanner's split SplitFunc;
	// the shattered form emitted one full call per LHS element, tripling side effects).
	m := machine{split: func(s string) (int, string, error) { return len(s), s + "!", nil }}
	n, out, serr := m.split("hi")
	fmt.Println(n, out, serr == nil) // 2 hi! true

	p := provider{produce: func(x int) int { return x * 2 }}
	r := registry{h: p.produce}
	fmt.Println(r.h(21)) // 42
```

<!-- source: src/tests/Behavioral/FirstClassFunctions/FirstClassFunctions.cs.target:111-121 -->
```csharp
    var converted = (Func<score, (score, bool)>)(stay);
    var (result, turnIsOver) = converted(new score(player: 3, opponent: 5, thisTurn: 7));
    fmt.Println("converted func type call:", result.player, result.opponent, result.thisTurn, turnIsOver);

    var m = new machine(split: (@string s) => (len(s), s + "!", default!));
    var (n, @out, serr) = m.split("hi");
    fmt.Println(n, @out, serr == default!);

    var p = new provider(produce: (nint x) => x * 2);
    var r = new registry(h: new handler(p.produce));
    fmt.Println(r.h(21));
```

`"converted func type call:"` is a plain C# literal that go2cs-gen boxes as a Go string for `fmt.Println`
([strings](strings.md#a-literal-in-an-any-slot-is-a-plain-c-literal-too-and-go2cs-gen-boxes-it-as-a-go-string));
`@out` is the keyword-escaped name of Go's `out`.

With `-comments`, from the converted standard library, the blank line stays above the comment that opens
the next group:

<!-- source: GOROOT/src/internal/strconv/ftoa.go:429-437 -->
```go
		}
	}

	// e±
	dst = append(dst, fmt)
	exp := d.dp - 1
	if d.nd == 0 { // special case: 0 has exponent 0
		exp = 0
	}
```

<!-- source: src/core/internal/strconv/ftoa.cs:473-481 -->
```csharp
        }
    }

    // e±
    dst = append(dst, fmt);
    nint exp = d.dp - 1;
    if (d.nd == 0) { // special case: 0 has exponent 0
        exp = 0;
    }
```

**Why.** Go programmers group statements with blank lines, and the groups are part of what a reader follows.

**Limits.** A blank line that Go writes directly after a comment that opens a block is not kept: the comment
leads the block, and the blank after it is dropped. The converter's own lines are not Go statements, so a
range variable's copy can sit next to a blank line Go does not have.

### One blank line after the converter's prelude

**Rule.** A function's prelude is the lines the converter writes before the Go body: locals for named
results, the slice built from a variadic parameter, array clones, the dereferenced alias of a pointer
receiver or parameter, heap boxes. Its lines are written one after another, then exactly one blank line,
then the Go body. This applies to every conversion.

<!-- source: src/tests/Behavioral/FirstClassFunctions/FirstClassFunctions.go:94-98 -->
```go
func ratioString(vals ...int) string {
	total := 0
	for _, val := range vals {
		total += val
	}
```

<!-- source: src/tests/Behavioral/FirstClassFunctions/FirstClassFunctions.cs.target:79-85 -->
```csharp
internal static @string ratioString(params ꓸꓸꓸnint valsʗp) {
    var vals = valsʗp.sslice();

    nint total = 0;
    foreach (var (_, val) in vals) {
        total += val;
    }
```

`ꓸꓸꓸnint` is the converted variadic parameter type, and `valsʗp` is the raw `params` argument. With named
results and `-comments`:

<!-- source: GOROOT/src/strings/reader.go:38-42 -->
```go
// Read implements the [io.Reader] interface.
func (r *Reader) Read(b []byte) (n int, err error) {
	if r.i >= int64(len(r.s)) {
		return 0, io.EOF
	}
```

<!-- source: src/core/strings/reader.cs:40-47 -->
```csharp
// Read implements the [io.Reader] interface.
public static (nint n, error err) Read(this ref Reader r, slice<byte> b) {
    nint n = default!;
    error err = default!;

    if (r.i >= (int64)len(r.s)) {
        return (0, io.EOF);
    }
```

**Why.** The prelude is the converter's text, not Go's; one blank line sets it apart from the Go body the
reader is comparing with the original.

### After a switch, a blank line only where Go has one

**Rule.** With `-comments`, a converted `switch` or type switch ends at its closing `}}` with no line break
of its own, so the blank-line rule above decides what follows it: a blank line where Go has one, none where
it has none, and none before the `}` of a block that ends with a switch. A converted `select` never ends with
its own line break, so after a `select` the blank line follows Go in every conversion.

No blank line where Go has none:

<!-- source: GOROOT/src/unicode/utf16/utf16.go:89-94 -->
```go
		default:
			a[n] = uint16(replacementChar)
			n++
		}
	}
	return a[:n]
```

<!-- source: src/core/unicode/utf16/utf16.cs:92-98 -->
```csharp
        default: {
            a[n] = (uint16)replacementChar;
            n++;
            break;
        }}
    }
    return a.slice(0, n);
```

A blank line where Go has one:

<!-- source: GOROOT/src/internal/strconv/ftoa.go:452-456 -->
```go
	default:
		dst = append(dst, byte(exp/100)+'0', byte(exp/10)%10+'0', byte(exp%10)+'0')
	}

	return dst
```

<!-- source: src/core/internal/strconv/ftoa.cs:500-505 -->
```csharp
    default: {
        dst = append(dst, (byte)((byte)(exp / 100) + (rune)'0'), (byte)((byte)(exp / 10) % 10 + (rune)'0'), (byte)((byte)(exp % 10) + (rune)'0'));
        break;
    }}

    return dst;
```

**Limits.** Without `-comments` the converted switch still ends with its own line break, so a blank line
follows every switch whether Go has one or not. A behavioral golden shows it:

<!-- source: src/tests/Behavioral/AnyBoxedUntypedConst/main.go:52-55 -->
```go
	case string:
		return "string"
	}
	return "other"
```

<!-- source: src/tests/Behavioral/AnyBoxedUntypedConst/main.cs.target:61-65 -->
```csharp
    case @string: {
        return "string";
    }}

    return "other";
```

### Between the specs of a `const` or `var` group

**Rule.** With `-comments`, Go's blank line between two specs of a grouped declaration is kept as one blank
line, above the spec or above its doc comment, and none is added between specs Go writes next to each other.
The gap is measured from the end of the previous spec's trailing comment, which can span lines.

<!-- source: GOROOT/src/unicode/utf16/utf16.go:17-26 -->
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

<!-- source: src/core/unicode/utf16/utf16.cs:16-23 -->
```csharp
// 0xd800-0xdc00 encodes the high 10 bits of a pair.
// 0xdc00-0xe000 encodes the low 10 bits of a pair.
// the value is those 20 bits plus 0x10000.
internal static UntypedInt surr1 => 0xd800;
internal static UntypedInt surr2 => 0xdc00;
internal static UntypedInt surr3 => 0xe000;

internal static UntypedInt surrSelf => 0x10000;
```

**Limits.** Only `const` and `var` groups. Without `-comments` this rule is not applied.

## Doc comments

### The group's doc once, then each spec's own

**Rule.** In a grouped `const` or `var` declaration, the group's doc comment is written once, above the
first spec, and each spec's own doc comment is written above that spec. An ungrouped declaration takes its
declaration's doc.

<!-- source: GOROOT/src/encoding/csv/reader.go:86-94 -->
```go
// These are the errors that can be returned in [ParseError.Err].
var (
	ErrBareQuote  = errors.New("bare \" in non-quoted-field")
	ErrQuote      = errors.New("extraneous or missing \" in quoted-field")
	ErrFieldCount = errors.New("wrong number of fields")

	// Deprecated: ErrTrailingComma is no longer used.
	ErrTrailingComma = errors.New("extra delimiter at end of line")
)
```

<!-- source: src/core/encoding/csv/reader.cs:89-95 -->
```csharp
// These are the errors that can be returned in [ParseError.Err].
public static error ErrBareQuote = errors.New("bare \" in non-quoted-field"u8);
public static error ErrQuote = errors.New("extraneous or missing \" in quoted-field"u8);
public static error ErrFieldCount = errors.New("wrong number of fields"u8);

// Deprecated: ErrTrailingComma is no longer used.
public static error ErrTrailingComma = errors.New("extra delimiter at end of line"u8);
```

The same file shows the group rule above: the blank line Go has before `ErrTrailingComma`'s doc is kept.

### A named type that is not a struct or interface

**Rule.** The doc comment of a named numeric, slice, map, channel, pointer or function type is written above
its C# declaration, as a struct's is.

<!-- source: GOROOT/src/sort/sort.go:120-123 -->
```go
// Convenience types for common cases

// IntSlice attaches the methods of Interface to []int, sorting in increasing order.
type IntSlice []int
```

<!-- source: src/core/sort/sort.cs:116-119 -->
```csharp
// Convenience types for common cases

// IntSlice attaches the methods of Interface to []int, sorting in increasing order.
partial struct IntSlice /*[]nint*/;
```

`/*[]nint*/` is the converter's note of the underlying type; the source generators complete the struct
([marker comments](source-generators.md#marker-comments-what-converted-code-carries-instead-of-an-attribute)).
The comment above it, which Go leaves free, keeps its place and its blank line.

A function type with no methods has no C# declaration: it is written inline as its delegate type wherever it
is used. Its doc is written directly above the converter's line that says so:

<!-- source: GOROOT/src/bufio/scan.go:60-67 -->
```go
// scanning lines, a [SplitFunc] can return (0, nil, nil) to signal the
// [Scanner] to read more data into the slice and try again with a
// longer slice starting at the same point in the input.
//
// The function is never called with an empty data slice unless atEOF
// is true. If atEOF is true, however, data may be non-empty and,
// as always, holds unprocessed text.
type SplitFunc func(data []byte, atEOF bool) (advance int, token []byte, err error)
```

<!-- source: src/core/bufio/scan.cs:60-67 -->
```csharp
// scanning lines, a [SplitFunc] can return (0, nil, nil) to signal the
// [Scanner] to read more data into the slice and try again with a
// longer slice starting at the same point in the input.
//
// The function is never called with an empty data slice unless atEOF
// is true. If atEOF is true, however, data may be non-empty and,
// as always, holds unprocessed text.
// type SplitFunc is a methodless func type — rendered inline as its base delegate
```

### A type alias's doc, above its `global using`

**Rule.** A Go type alias becomes a `global using` line at the top of the file, above `namespace`. Its doc
comment is written directly above that line, at no indentation, not where Go declared the alias. One blank
line sets it apart from the file's header comments or from a previous `using` line.

<!-- source: GOROOT/src/os/error.go:45-46 -->
```go
// PathError records an error and the operation and file path that caused it.
type PathError = fs.PathError
```

<!-- source: src/core/os/error.cs:1-8 -->
```csharp
// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// PathError records an error and the operation and file path that caused it.
global using PathError = go.io.fs_package.PathError;

namespace go;
```

**Limits.** An alias of an anonymous struct or interface is converted as a lifted type, which takes the doc;
its `global using` line then has none.

## Comments inside a body

### A trailing comment stays on its statement's line

**Rule.** A comment written on the same source line as a statement's end is written after one space on that
statement's last emitted C# line.

<!-- source: GOROOT/src/strings/strings.go:1139-1141 -->
```go
	if old == new || n == 0 {
		return s // avoid allocation
	}
```

<!-- source: src/core/strings/strings.cs:1180-1182 -->
```csharp
    if (old == @new || n == 0) {
        return s; // avoid allocation
    }
```

**Details.**

- **The last emitted line, not the statement's line.** One Go statement often lowers to several C# lines (a
  heap-boxed define, a hoisted capture, a lowered switch). The comment is written where the output stands
  after the whole statement.
- **Only the statement-list slots are eligible:** a block body, a `case` body, a `select` clause body. The
  init clause of an `if`, `for` or `switch` is excluded, because the rest of the header follows it on the same
  line and a `//` there would comment the header out.
- **A multi-line `/* … */` is not inlined.** It keeps its own lines.
- **One space, not Go's column.** Go aligned its comments for Go's line lengths; the converted lines are
  different lengths, so the padding is not replayed. A struct field's or a spec's attached comment does keep
  Go's source-column padding.
- **A function literal keeps its block body when its one `return` carries a comment.** Such a literal
  normally collapses to an expression-bodied lambda, but a collapsed body sits inside the enclosing
  expression, where a `//` would comment out the rest of the line. The literal collapses only when the
  `return`'s own `;` ends its block. A comment written after a literal's closing `}`, on the same line, is not
  the inner statement's: it labels the list element the literal is in, and the literal still collapses.

**Why.** Before this rule a trailing comment waited for the next statement, so the last statement of a block
had none to lead and its comment was written after the block had closed, outside the construct it described.

### A comment after a block's `{` trails the `{`

**Rule.** A one-line comment after the `{` of an `if`, `else`, `for` or `switch` header is written on the
C# line that ends with that `{`. Here, from the `bidi` package of `golang.org/x/text`, which Go vendors into its own
tree:

<!-- source: GOROOT/src/vendor/golang.org/x/text/unicode/bidi/core.go:676-682 -->
```go
			if t == L {
				// no change
			} else if t == R {
				s.resolvedLevels[i] += 1
			} else { // t == AN || t == EN
				s.resolvedLevels[i] += 2
			}
```

<!-- source: src/core/vendor/golang.org/x/text/unicode/bidi/core.cs:724-731 -->
```csharp
            if (t == L){
                // no change
            } else 
            if (t == R){
                s.resolvedLevels[i] += 1;
            } else { // t == AN || t == EN
                s.resolvedLevels[i] += 2;
            }
```

The converter writes `else if` as `} else ` and then `if (…){` on the next line.

**Limits.** A function body's `{` is followed by the prelude, so a comment after it stays on its own line
below. So does a comment after a `{` that the converter follows with its own text, and a multi-line block
comment.

### On a `case` line, and above one

**Rule.** A one-line comment on a `case X:` or `default:` line trails that clause's C# header line. Comments
on their own lines above a `case` label are written above that clause's C# header, at its indentation. Both
hold in every lowered form: a C# `switch`, the `if`/`else` chain a switch can become, a type switch and
`select`.

<!-- source: GOROOT/src/unicode/utf16/utf16.go:80-85 -->
```go
		switch RuneLen(v) {
		case 1: // normal rune
			a[n] = uint16(v)
			n++
		case 2: // needs surrogate sequence
			r1, r2 := EncodeRune(v)
```

<!-- source: src/core/unicode/utf16/utf16.cs:79-86 -->
```csharp
        switch (RuneLen(v)) {
        case 1: { // normal rune
            a[n] = (uint16)v;
            n++;
            break;
        }
        case 2: { // needs surrogate sequence
            var (r1, r2) = EncodeRune(v);
```

Above a label:

<!-- source: GOROOT/src/internal/strconv/atof.go:434-440 -->
```go
	switch {
	case exp == 0:
		// an integer.
		return f, true
	// Exact integers are <= 10^15.
	// Exact powers of ten are <= 10^22.
	case exp > 0 && exp <= 15+22: // int * 10^k
```

<!-- source: src/core/internal/strconv/atof.cs:487-494 -->
```csharp
    switch (ᐧ) {
    case {} when exp is 0: {
        // an integer.
        return (f, true);
    }
    // Exact integers are <= 10^15.
    // Exact powers of ten are <= 10^22.
    case {} when exp > 0 && exp <= 15 + 22: { // int * 10^k
```

`ᐧ` is golib's `true`: a Go `switch` with no tag becomes `switch (ᐧ)` with `case {} when …` clauses.

When the `if`/`else` lowering emits a leading `default:` last, the default's own comments are held and
written with it; a comment at or left of its label's column documents the next `case` and stays above that
case.

### Opening a case body, or alone in an empty block

**Rule.** A comment that leads a `case` body stays directly above the body's first statement. A comment that
is an empty clause's whole body stays inside that clause, and a comment alone in an empty block stays in
that block.

<!-- source: GOROOT/src/unicode/utf16/utf16.go:102-109 -->
```go
	switch {
	case 0 <= r && r < surr1, surr3 <= r && r < surrSelf:
		// normal rune
		return append(a, uint16(r))
	case surrSelf <= r && r <= maxRune:
		// needs surrogate sequence
		r1, r2 := EncodeRune(r)
		return append(a, uint16(r1), uint16(r2))
```

<!-- source: src/core/unicode/utf16/utf16.cs:106-114 -->
```csharp
    switch (ᐧ) {
    case {} when (0 <= r && r < surr1) || (surr3 <= r && r < surrSelf): {
        // normal rune
        return append(a, (uint16)r);
    }
    case {} when surrSelf <= r && r <= maxRune: {
        // needs surrogate sequence
        var (r1, r2) = EncodeRune(r);
        return append(a, (uint16)r1, (uint16)r2);
```

An empty block:

<!-- source: GOROOT/src/encoding/xml/xml.go:1130-1136 -->
```go
		if b == '\r' {
			d.buf.WriteByte('\n')
		} else if b1 == '\r' && b == '\n' {
			// Skip \r\n--we already wrote \n.
		} else {
			d.buf.WriteByte(b)
		}
```

<!-- source: src/core/encoding/xml/xml.cs:1241-1248 -->
```csharp
        if (b == (rune)'\r'){
            d.buf.WriteByte((rune)'\n');
        } else 
        if (b1 == (rune)'\r' && b == (rune)'\n'){
            // Skip \r\n--we already wrote \n.
        } else {
            d.buf.WriteByte(b);
        }
```

### Before a block's closing brace

**Rule.** Comments after a block's last statement are written on their own lines above its `}`, inside the
block, keeping a blank line Go has above them. In a `case` body, comments after its last statement that are
indented deeper than the label stay at the end of that body.

<!-- source: GOROOT/src/internal/runtime/maps/table.go:1129-1135 -->
```go
			it.key = key
			it.elem = elem
			return
		}

		// Continue to next table.
	}
```

<!-- source: src/core/internal/runtime/maps/table.cs:1146-1152 -->
```csharp
            it.key = key;
            it.elem = elem;
            return;
        }

        // Continue to next table.
    }
```

The comment closes a `for` body in the map iterator of `internal/runtime/maps`, with the blank line Go leaves
above it.

**Limits.** Blocks the converter writes outside the ordinary block path, such as the outer block that holds
an `if` statement's init clause, do not take this rule yet.

## Comments inside expressions

### In a multi-line composite literal, or argument list

**Rule.** In a list that spans lines, a literal's elements or a call's arguments, a comment on the line where
an element or the opening bracket ends trails that line. Any other comment takes its own line at element
indentation, keeping a blank line Go has above it. Comments after a literal's last element are written above
its closing `}`, and a line comment forces that `}` onto its own line. A list takes only the comments after
its own opening bracket.

Trailing an element:

<!-- source: GOROOT/src/time/format.go:1615-1619 -->
```go
var unitMap = map[string]uint64{
	"ns": uint64(Nanosecond),
	"us": uint64(Microsecond),
	"µs": uint64(Microsecond), // U+00B5 = micro symbol
	"μs": uint64(Microsecond), // U+03BC = Greek letter mu
```

<!-- source: src/core/time/format.cs:1771-1775 -->
```csharp
internal static map<@string, uint64> unitMap = new map<@string, uint64>{
    ["ns"u8] = (uint64)(int64)ΔNanosecond,
    ["us"u8] = (uint64)(int64)Microsecond,
    ["µs"u8] = (uint64)(int64)Microsecond, // U+00B5 = micro symbol
    ["μs"u8] = (uint64)(int64)Microsecond, // U+03BC = Greek letter mu
```

After the last element, above the closing bracket:

<!-- source: GOROOT/src/database/sql/sql.go:2802-2806 -->
```go
			rows = &Rows{
				dc:    dc,
				rowsi: rowsi,
				// releaseConn set below
			}
```

<!-- source: src/core/database/sql/sql.cs:3192-3196 -->
```csharp
                Ꮡrows.ValueSlot = Ꮡ(new Rows(
                    dc: dc,
                    rowsi: Ꮡrowsi.ValueSlot
                    // releaseConn set below
                ));
```

`Ꮡ(…)` is golib's address-of, Go's `&`. The sample sits in a closure that `Stmt.QueryContext` passes to
`retry`, and `rows` and `rowsi` are locals of `QueryContext` that the closure assigns. So each lives in a heap
box, and the closure writes the box's `ValueSlot` (see
[Function Values and Closures](../ConversionStrategies.md#function-values-and-closures)).

**Limits.** A package-level `const` or `var` writes the comments before it above its doc, and the comments
inside its value where the literal reaches them. Any the emission does not reach are still written above the
doc, so they are misplaced but never lost.

### Between operands and call arguments

**Rule.** A comment between a binary expression's operands, or between a call's arguments, is written at its
Go position. Where Go breaks the line after an operator, the C# breaks there too, with the comment trailing
the operator and the right operand on the next line. A `/* … */` inside a line stays inline at its position.

<!-- source: GOROOT/src/go/printer/gobuild.go:138-144 -->
```go
func appendLines(x, y []byte) []byte {
	if len(y) > 0 && isNL(y[0]) && // y starts in blank line
		(len(x) == 0 || len(x) >= 2 && isNL(x[len(x)-1]) && isNL(x[len(x)-2])) { // x is empty or ends in blank line
		y = y[1:] // delete y's leading blank line
	}
	return append(x, y...)
}
```

<!-- source: src/core/go/printer/gobuild.cs:147-153 -->
```csharp
internal static slice<byte> appendLines(slice<byte> x, slice<byte> y) {
    if (len(y) > 0 && isNL(y[0]) && // y starts in blank line
        (len(x) == 0 || len(x) >= 2 && isNL(x[len(x) - 1]) && isNL(x[len(x) - 2]))) { // x is empty or ends in blank line
        y = y[1..]; // delete y's leading blank line
    }
    return appendꓸꓸꓸ(x, y);
}
```

`appendꓸꓸꓸ(x, y)` is Go's `append(x, y...)`. Inline between call arguments:

<!-- source: GOROOT/src/crypto/tls/handshake_client.go:864 -->
```go
		clientCipher = hs.suite.cipher(clientKey, clientIV, false /* not for reading */)
```

<!-- source: src/core/crypto/tls/handshake_client.cs:985 -->
```csharp
        clientCipher = (~hs.suite).cipher(clientKey, clientIV, false /* not for reading */);
```

`~` is golib's pointer dereference.

A `+` chain of string literals that carries comments and folds to one byte-array constant is written one Go
operand per line, each line's bytes followed by its comment.

<a id="what-is-still-deferred"></a>

## Limits: where a comment still lands elsewhere

A comment is placed only when the converter writes the construct it sits in through one of the points above.
A path that renders an expression some other way leaves the comment for the fallback, which writes it on its
own line at the next place it can. The comment is then misplaced, never lost:

- **A block comment between the last operand and `{`** moves into the block, as its first line:

<!-- source: GOROOT/src/math/big/float.go:876-877 -->
```go
			if p < 0 /* m <= 0.25 */ || p == 0 && x.mant.sticky(uint(len(x.mant))*_W-1) == 0 /* m == 0.5 */ {
				// underflow to ±0
```

<!-- source: src/core/math/big/float.cs:925-927 -->
```csharp
            if (p < 0 /* m <= 0.25 */ || p == 0 && x.mant.sticky((nuint)len(x.mant) * (nuint)_W - 1) == 0) {
                /* m == 0.5 */
                // underflow to ±0
```

- **A comment inside `make`'s size argument** is written on its own line after the statement, because the
  size is rendered and then rebuilt (`crypto/x509`'s `make([]pkix.Extension, 10 /* … */)`).
- **A comment before an expression's first operand** stays on its own line after the statement.
- **An integer constant folded to one number** has no operands left to carry a comment between them.

A few comments are still dropped, mostly trailing comments on specs and doc comments on fields in shapes the
converter writes through another path. The census below counts them per class, so this page names no number.

## Guarded by

- `trailingComments_test.go`: the trailing-comment rule, positionally, with negative controls (a whole-line
  comment and a multi-line block comment are not pulled onto a code line).
- `collapsedLambdaTrailingComment_test.go`: the function-literal rule, with a comment-free literal that still
  collapses.
- `bodyBlankLines_test.go`: blank lines in bodies with and without `-comments`, the switch rule, and the brace
  and `case` placements.
- `preludeSeparator_test.go`: the blank line after the prelude.
- `docComments_test.go`: group docs, non-struct type docs, group blank lines, and no blank-line runs around a
  lifted package-level struct.
- `aliasDocComments_test.go`: alias docs above their `global using`.
- `compositeLitComments_test.go`: composite literals and multi-line argument lists.
- `expressionComments_test.go`: operand and argument comments, and folded string constants.
- `commentParityUtf8_test.go`: Go's own `unicode/utf8` converted fresh and read against its Go source.
- `internal/commentcensus`: the census of every comment group and blank line in a corpus against its Go
  source, with a committed baseline that a change may lower and not raise.

<!-- Rewritten 2026-10-14 for the comment and spacing parity work (docs/PLAN-comment-and-spacing-parity.md;
     records docs/phase4/CENSUS-comment-parity-seat*.md). Every example is cut by line range from master
     53c1e4dc64, after the TRAIN T4 regeneration; Go lines from GOROOT go1.24.13. src/core/unicode/utf8/utf8.cs
     is NOT used: it was not regenerated by T4 (reported to COORD 2026-10-14). The earlier version of this page
     showed the trailing-comment rule's "before" output from the Tour; its "What is still deferred" list (after
     a header's {, after a case label, before a closing brace) is now implemented and is described above. -->

---

[← Manually-Converted Declarations](manual-conversions.md) · [Index](README.md) · [Deterministic Output →](deterministic-output.md)
