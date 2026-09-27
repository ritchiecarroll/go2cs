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

## A doc-comment link resolves to a fully-qualified, version-pinned URL

A converted package's `README.md` is its package-level Go doc comment rendered to Markdown, and a Go doc
comment can link. Left to `go/doc/comment`'s defaults, those links come out **site-root-relative**:
`[io.Reader]` renders as `[io.Reader](/io#Reader)`, because `Printer.DocLinkBaseURL` defaults to empty and
`DocLink.DefaultURL` then composes a path from the site root. That is exactly right for pkg.go.dev, which
serves the documentation at its own root, and exactly wrong everywhere this README is actually read:
GitHub resolves `/io#Reader` against `github.com`, Pages/Jekyll against the site root, and nuget.org
against `nuget.org`. The link is dead in all three.

The emitter therefore installs its own `Printer.DocLinkURL` (`renderPackageDoc` in `readme.go`, resolver in
`readmeDocLinks.go`). A standard-library target pins the Go release that produced the conversion —
`https://pkg.go.dev/io@go1.24.13#Reader` — which is the same rule, and the same honesty doctrine, the Docs
badge beside it already follows.

**Completeness is structural here, not a judgement call, because the grammar is closed.**
`go/doc/comment`'s `Text` interface has exactly four implementations — `Plain`, `Italic`, `*Link`,
`*DocLink` — and only two carry a URL:

* **`*Link` URLs are absolute by construction.** Both of the parser's two link sources require a scheme:
  `parseLink` rejects a `[text]: url` definition whose url has no `isScheme(...)://`, and `autoURL` rejects
  inline text on the same test (the accepted schemes are `file`, `ftp`, `gopher`, `http`, `https`,
  `mailto`, `nntp`). A `*Link` therefore *cannot* reach the emitter with a relative URL, and passes through
  untouched — which is also what the "already-absolute URLs are left alone" rule asks for.
* **`*DocLink` is the sole relative-URL producer**, and its own documentation enumerates the exhaustive set
  of five field combinations. `resolveDocLinkURL` answers all five.

| `DocLink` fields | Emitted URL |
|---|---|
| `ImportPath` | `https://pkg.go.dev/io@go1.24.13` |
| `ImportPath`, `Name` | `https://pkg.go.dev/io@go1.24.13#Reader` |
| `ImportPath`, `Recv`, `Name` | `https://pkg.go.dev/io@go1.24.13#Writer.Write` |
| `Name` | `https://pkg.go.dev/<current>@go1.24.13#Name` |
| `Recv`, `Name` | `https://pkg.go.dev/<current>@go1.24.13#Recv.Name` |

The two same-package forms cannot occur today — the converter leaves `Parser.LookupSym` nil, so `[NewInt]`
stays literal text rather than becoming a link (which is why the corpus is full of escaped `\[Int]`,
`\[Encoder]`, `\[Decode]`: those are not dead links, they are not links at all, and pkg.go.dev shows an
unresolvable name the same way). Answering them anyway is what makes the resolver total against the
*grammar* rather than against today's census, so enabling `LookupSym` later needs no second pass here.

**An external module path is pinned only when the distribution actually pinned it.** A path whose first
element carries a dot is a module, not a std package, and cannot be pinned to a Go release — it is not a Go
release artifact. When GOROOT vendors that exact package, `src/vendor/modules.txt` records the snapshot the
conversion read and the URL states it (`golang.org/x/sys@v0.22.0/cpu#X86`). When it does not —
`golang.org/x/sys/windows` is referenced by std doc comments but is **not** among the x/sys packages GOROOT
vendors — the URL is emitted fully qualified but **unversioned** rather than borrowing the pin from the
module's other vendored packages. A fabricated pin is worse than an unpinned link: the unpinned one still
resolves on all three surfaces, which is the entire defect being fixed. Same degradation the Source·Go
badge makes for the same reason — an unresolvable pin costs precision, never correctness.

Corpus census at the change: **99 relative link occurrences across 38 of 307 emitted READMEs** (40
package-only `/pkg`, 57 `/pkg#Name`, 2 `/pkg#Recv.Name`, 0 bare-fragment `#Name` — exactly the distribution
the grammar predicts with `LookupSym` nil), against 1,899 already-absolute targets that pass through
unchanged.

Guarded by `readmeDocLinks_test.go`, which enumerates the five combinations rather than sampling them and
fails on any target that still begins at the site root, plus an end-to-end case over real godoc markup that
asserts both halves of the contract — every doc link qualified, every absolute link untouched.

**One thing that looks like this defect and is not.** `src/core/image/README.md` renders
`\[Go Security Policy]([https://go.dev/security/policy](https://go.dev/security/policy))`. That is upstream
Go writing **Markdown** link syntax inside a doc comment (`image/image.go:37`), which `go/doc/comment` does
not support; pkg.go.dev renders it identically. Faithful conversion of an upstream quirk, not an emitter
defect.

---

[← Manually-Converted Declarations](manual-conversions.md) · [Index](README.md) · [Deterministic Output →](deterministic-output.md)
