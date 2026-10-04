# Comments

[Reference index](README.md) · [Summary of this topic](../ConversionStrategies.md)

Comment conversion is opt-in (`-comments`, default **off**) and two consumers require it: the
standard-library conversion, because the converted C# is a derivative work whose per-file
`// Copyright … The Go Authors … BSD-style license` header must survive (and Go's doc comments are
most of what makes the output readable), and [Tour of go2cs](../../src/tour/README.md), which converts
the lesson text a reader is looking at — there, a comment that lands in the wrong place is the most
visible defect the converter can have. (Behavioral goldens are captured *without* comments, so
comment placement never moves a golden; it is guarded by converter tests instead.)

## Where a comment is attached, and where it is not

Go's AST attaches a comment to a node only in specific places — a declaration's `Doc`, a struct
field's `Doc`/`Comment`, a spec's `Doc`/`Comment`. **Statements have no comment field at all.** So a
statement's comment is *free-floating*: it exists in `File.Comments` and nowhere else.

`visitFile` builds that set explicitly — every comment in `File.Comments`, keyed by its `//`
position, minus every comment group reachable by walking the AST (the attached ones). What is left
is `standAloneComments`, and it is flushed at the next emission point that asks for one
(`writeStandAloneCommentString`, reached from `writeDocString` and from the block-statement loop),
written on its own line with the block's indentation.

That is right for a comment that stood on its own line in Go. It is wrong for one that trailed a
statement, and the failure was not merely cosmetic: with the comment held until the *next* emission
point, the last statement of a block had no next statement to lead, so the comment was written after
the block had already closed — **outside the construct it documented**. Both Tour examples showed
it, one of each shape:

```go
for i := range pow {
	pow[i] = 1 << uint(i) // == 2**i
}
```

```csharp
// before
foreach (var (i, _) in pow) {
    pow[i] = ((nint)1).Lsh((nuint)i);
}
// == 2**i
```

```go
p := &i         // point to i
fmt.Println(*p) // read i through the pointer
```

```csharp
// before
var p = Ꮡi;
// point to i
fmt.Println(p.Value);
// read i through the pointer
```

## The rule

**A comment written on the same source line as a statement's END is emitted on the same line as that
statement's LAST emitted line.** Everything else keeps the standalone path unchanged.

```csharp
// after
foreach (var (i, _) in pow) {
    pow[i] = ((nint)1).Lsh((nuint)i); // == 2**i
}
```

```csharp
// after
var p = Ꮡi; // point to i
fmt.Println(p.Value); // read i through the pointer
```

A single space separates code from comment. The source column is deliberately *not* reproduced: Go's
alignment was computed for Go's line lengths, and the converted lines are a different length, so
replaying the original padding produces ragged output rather than a column. (An attached comment —
a struct field's — still uses the source-column padding in `writeCommentString`; that path is
untouched.)

Four details make the rule hold generally:

* **"Last emitted line", not "the statement's line".** One Go statement routinely lowers to several
  C# lines (a heap-boxed define, a hoisted capture snapshot, a lowered switch). The comment is
  written where the output builder actually stands after the statement is emitted, which is the end
  of that whole emission.
* **The statement-LIST slots are the only ones eligible** (`visitListStmt`): a block body, a `case`
  body, a `select` comm-clause body — the places where a statement's text is known to end its line.
  The init clause of an `if`/`for`/`switch` is deliberately excluded, because the rest of the header
  follows it on the same emitted line; a `//` comment tucked in there would comment the header out.
  Those callers keep calling `visitStmt` directly.
* **A statement that closes its own line is handled** — `visitSwitchStmt` ends its emission with a
  newline — by writing the comment *ahead* of that terminator rather than at column zero of the next
  line.
* **A multi-line block comment is not inlined.** A `/* … */` that opens on the statement's line but
  closes on a later one cannot be tucked onto a single line, so it stays with the standalone path,
  which can indent its continuation lines.

Because the comment is now claimed by the statement it belongs to, it can no longer be carried past
the closing brace: `} // after the if/else` and `} // after the switch` land on the brace, and a
block's final statement keeps its comment inside the block.

**A single-return func literal whose return carries the comment keeps its block body.** Such a literal
normally collapses to an expression-bodied lambda (`(nint n) => n == limit`), but a collapsed body
has no line of its own: it sits inside the enclosing expression, so a trailing `//` comment would
comment out the rest of it (`wrap((nint n) => n < limit; // below the limit, "x"u8);`, CS1026 —
go-cmp's `return t.AssignableTo(errorIface) // Never true`). `convFuncLit` collapses only when the
return statement's own `;` ends the block; otherwise the block stays, and the comment ends its own
line inside it:

```csharp
var f = wrap((nint n) => {
    return n < limit; // below the limit
}, "x"u8);
```

(Guarded by `collapsedLambdaTrailingComment_test.go`, with a comment-free literal that still collapses
as the control.)

## What is still deferred

Three shapes remain genuinely leading and keep the standalone path — the comment stays where it
already was, which for the first two means *inside* the construct and for the third means after it:

* a comment after the `{` of a header (`for i := 0; i < n; i++ { // …`) leads the first body statement;
* a comment after a `case X:` label leads the case body — and, since a case body has no leading flush
  of its own, is still emitted at the next flush point past the switch;
* a whole-line comment standing before a block's closing brace with no statement after it.

Guarded by `trailingComments_test.go` — the two Tour examples plus the cross-statement-kind shapes,
asserting *positionally* (the comment shares its line with code that precedes it, and the block
closes after it) rather than by matching converted text, so an unrelated emission change does not
break the guard. The negative controls are in the same file: a whole-line comment and a multi-line
block comment must **not** be pulled onto a code line.

---

[← Manually-Converted Declarations](manual-conversions.md) · [Index](README.md) · [Deterministic Output →](deterministic-output.md)
