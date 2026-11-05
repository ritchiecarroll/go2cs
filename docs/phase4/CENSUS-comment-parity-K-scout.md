# CENSUS — comment parity class K: one-line members and top-level blank lines

> **State: SCOUT, 2026-11-05 (P2), for the owner. Read-only: no converter, generator or corpus file was changed.**
> Measured at master `8aaa6cebc8` against go1.26.10, over the 1,727 converted production files that have a Go source
> file. Class K of [PLAN-comment-and-spacing-parity.md](../PLAN-comment-and-spacing-parity.md) (§4.9, §7 decision 5),
> ruled and held since 2026-10-13. COORD's dispatch: "SCOUT first: census, the candidate directions, and a
> recommendation. The direction is a reads-like-Go call, so it goes to the owner before any build."
>
> **Headline.** Class K is two numbers. **1,069 blank lines** in C# sit between top-level declarations that Go writes with
> no blank line between them, in 235 files; **58** blank lines Go has are missing in C#, in 39 files. Of the 1,069, 600
> are adjacent one-line functions (the plan's "one-line methods"), 182 are functions with a multi-line side that the
> Go author also wrote adjacent, 262 are const and var declarations, and 25 are types. Of the 58, 45 are lifted
> anonymous-struct vars, and the other 13 are read wrongly by the instrument. Go has **1,528 one-line functions**; C#
> writes 1,491 of them as a three-line body that could be one line, and 36 need more than one statement.
> **Recommended:** write a blank line between top-level declarations only where Go has one, and write a function Go
> keeps on one line as a one-line block body, `{ return x.Lparen; }`. Leave the lifted-var shape as it is.

## 1. The instrument

Two independent derivations agree. The committed census (`src/go2cs/internal/commentcensus`, the corpus ratchet in
`testdata/corpus_baseline.txt`) reads `top:go-none-cs-blank 1069` and `top:go-blank-cs-none 58` at this master. The
plan's Appendix A instrument, extracted byte-identical (sha256 of `main.go` `811c06281713` both ways), reads the same
two numbers over the same 1,727 pairs.

The breakdown below comes from Appendix A with one addition (Appendix A of this record): the shape of the two
declarations around each finding, and for each Go one-line function, the shape of its C# body. Every count Appendix A
already printed is unchanged by the addition, on the corpus and on the plan's three planted renderings.

**Red test of the addition.** A planted `plant2.go` (two adjacent one-line methods, then a multi-line method with a
one-line empty function under it) and three renderings of it:

| Rendering | Instrument reads |
|:--|:--|
| `today` (three-line bodies, a blank line between every member) | `go-none-cs-blank 2` (`method/1line -> method/1line`, `method/multi -> func/1line`); one-line C#: `block-1` 2, `block-0` 1 |
| `oneline` (one-line block bodies, Go's blank lines) | `same 4`; `oneline` 3 |
| `expr` (expression-bodied, Go's blank lines) | `same 4`; `expr` 2, `oneline` 1 |

## 2. The 1,069 inserted blank lines, by the two declarations around each

| Previous → this declaration | Count |
|:--|--:|
| one-line method → one-line method | 470 |
| one-line func → one-line func (or method) | 130 |
| **one-line functions, both sides** | **600** |
| multi-line func → multi-line func | 128 |
| multi-line method → multi-line method | 21 |
| one-line ↔ multi-line function, either order | 33 |
| **functions with a multi-line side** | **182** |
| one-line const → one-line const | 147 |
| one-line var → one-line var | 62 |
| multi-line var → multi-line var | 44 |
| other const/var pairs | 9 |
| **const and var** | **262** |
| **types** | **25** |

So the plan's "nearly all one-line methods" is now 600 of 1,069. The other 469 are declarations a Go author wrote with
no blank line between them, and C# separates every top-level declaration with one, whatever Go did.

**Root cause, unchanged from the plan.** There is no separator emitter. `visitFile.go:167-169` calls `visitDecl` for
each declaration, and each emitter writes its own newlines: `visitFuncDecl.go:749` before a function,
`visitValueSpec.go:33` before a const or var spec, `visitGenDecl.go:41` after a const or var declaration. Class A's
body-level fix already has the helper that would decide it: `goBlankLineBetween` (`visitBlockStmt.go:587`), one blank line when any line between two nodes is blank in Go, a line
a comment covers not counting.

## 3. The 1,528 one-line Go functions, by the C# body they get

| Go body | C# body today | Count |
|:--|:--|--:|
| `return x` (one result) | one statement line | 1,143 |
| `return a, b` | one statement line | 19 |
| a call | one statement line | 118 |
| an assignment or `++`/`--` | one statement line | 58 |
| empty `{}` | `{` and `}` on two lines | 153 |
| **can be one line** | | **1,491** |
| one statement, but C# adds a prelude (a receiver deref, a heap box) | two or more lines | 22 |
| two or more statements on Go's one line | two or more lines | 14 |
| unread | | 1 |

The 36 that need more than one line in C# look like `net/http/server.go:679`, Go
`func (cr *connReader) unlock() { cr.mu.Unlock() }`, which C# writes as a deref line, the blank prelude separator, and
the call.

**Width.** Written on one line, the 1,491 come to 1,294 lines of 100 columns or less, 162 of 101 to 120, 30 of 121 to
150 and 5 over 150. 21 of the same functions are already past 100 columns in Go. The C# header is longer than Go's
(`this ref`, the C# type names), so 79 lines would be 40 or more columns wider than the Go line.

## 4. The 58 missing blank lines

| Shape | Count | What C# writes |
|:--|--:|:--|
| a var with an anonymous struct type (`internal/cpu/cpu.go:19`, `var X86 struct{…}`) | 45 | the doc, the lifted `partial struct X86ᴛ1 /*dyn*/ {…}`, then the var's box and property directly under its `}` |
| instrument reads the wrong line | 13 | 6 const groups whose first spec is `_` (the instrument looks for the second name, which sits under `_ᴛ1ʗ`), 5 vars whose lazy `initᴛX()` the instrument takes for the var, 2 vars whose tuple temp (`tupleᴛ1ʗ`, `net/rpc/debug.go:40`) carries the blank line above it |

The plan's 2026-10-11 reading had the doc separated from the var by the lifted struct. Seat D/E1/L
([CENSUS-comment-parity-seats-DEL.md](CENSUS-comment-parity-seats-DEL.md):67) moved the doc directly above the lifted
struct and left "the doc above the var itself" to K. Today the C# reads as one unit, in Go's order: the doc, the
struct's fields, then the name.

## 5. The directions

**K1, the separator (the 1,069).**

- **(a) Go's blank lines.** One blank line between two top-level declarations only where Go has one. Fixes all 1,069,
  and it is the same rule class A applies inside bodies. Without (b), the 600 adjacent one-liners become three-line
  bodies with nothing between them, which the plan rightly judges worse than today.
- **(b) Go's blank lines, except around a member that is one line in Go and several in C#** (the 36 of §3). Those keep
  a blank line on each side.
- **(c) Today.** A blank line between every pair of declarations.

**K2, the one-line function (the 1,491).**

- **(a) One-line block body.** `public static tokenꓸPos Pos(this ref ParenExpr x) { return x.Lparen; }`. Go's text with
  a semicolon. Covers all 1,491, including the 153 empty bodies (`{ }`), and keeps `return`, so a statement still reads
  as a statement.
- **(b) Expression-bodied.** `public static tokenꓸPos Pos(this ref ParenExpr x) => x.Lparen;`. Idiomatic C#, and the
  converter already writes a one-return func literal this way. Covers 1,338: an empty body has no expression form, so
  153 would still need (a)'s `{ }`. Drops `return`, and the corpus already writes `=>` for consts and var properties
  (`public static PEMCipher PEMCipherDES => 1;`), so a method would look like a property at a glance.
- **(c) Today.** Three lines.

Either collapse follows Go's own choice of one line, whatever the width. A width cap would leave some members of an
adjacent run on one line and others on three.

**K3, the lifted var (the 45).** (a) Leave it as it is: doc, struct, var, as one unit. (b) The plan's fix: the
struct, a blank line, then the doc and the var, which puts the doc under the struct whose fields it describes.

**Not in K.** gofmt also aligns the bodies of adjacent one-liners into a column
(`func (x *ParenExpr) Pos() token.Pos      { return x.Lparen }`). That padding is column alignment, so it belongs with
class J (held). K writes one space.

## 6. Recommendation

**K1 (b), K2 (a), K3 (a).** Go's blank lines between top-level declarations, a blank line kept around the 36 members
that cannot be one line, one-line block bodies for the 1,491, and the lifted var left alone.

Why: K2 (a) is Go's own text plus a semicolon, works for every one-line shape including the empty body, and does not
mix methods up with the `=>` properties the corpus already has. With one-line bodies in place, Go's blank lines
between declarations read as they do in Go, and K1 (b) keeps the 36 three-line C# members from sitting directly
against their neighbours. K3 (a) already reads in Go's order.

**What it moves, as measured here.** In the corpus: 235 files with an inserted blank line and 266 with a collapsible
one-liner (the two sets overlap; a two-seeded reconvert would give the union). In the behavioral tests, if K runs in
every conversion as decision 1 ruled for class A: 299 of 898 directories have a one-line function (909 functions), 150 have adjacent declarations
(412 pairs), 344 in all, whitespace only. The committed ratchet's `top:go-none-cs-blank 1069` falls to the number
the build measures. Estimate for the build after a ruling: 1 to 2 lane-days, plus the golden re-baseline and the corpus
footprint.

**What the owner rules:** K1 (a, b or c), K2 (a, b or c), K3 (a or b).

## Appendix A — the instrument addition (against the plan's Appendix A `main.go`, sha256 `811c06281713`)

Run as Appendix C of the plan, with GOROOT from `GOTOOLCHAIN=go1.26.10 go env GOROOT`. The behavioral counter is a
separate 100-line `go/ast` program (per directory: one-line `FuncDecl`s, and top-level declaration pairs with no blank
line between, imports skipped). Its red test is `plant2.go`: it reads 3 one-line functions and 2 adjacent pairs.

```diff
--- main.go (plan Appendix A)
+++ main.go (with the K breakdown)
@@ -151,6 +151,8 @@
 	goRunsMisaligned int
 	blank2           counter
 	topLevel         counter
+	kShape           counter
+	oneLine          counter
 	groupSpec        counter
 	body             counter
 	examples         map[string][]example
@@ -162,7 +164,7 @@
 
 func newResults() *results {
 	return &results{groupsByClass: counter{}, missedByClass: counter{}, partialByClass: counter{}, misplacedByClass: counter{},
-		misplacedKind: counter{}, checkedByClass: counter{}, eol: counter{}, blank2: counter{}, topLevel: counter{}, groupSpec: counter{},
+		misplacedKind: counter{}, checkedByClass: counter{}, eol: counter{}, blank2: counter{}, topLevel: counter{}, kShape: counter{}, oneLine: counter{}, groupSpec: counter{},
 		body: counter{}, examples: map[string][]example{}, missedByPkg: counter{}, groupsByPkg: counter{}, filesByClass: counter{}}
 }
 
@@ -509,6 +511,79 @@
 		return s
 	}
 	goBlankAbove := func(startLn int) bool { return g.blank[startLn-1] }
+	goOneLine := func(d ast.Decl) bool {
+		fd, ok := d.(*ast.FuncDecl)
+		return ok && fd.Body != nil && g.fset.Position(fd.Pos()).Line == g.fset.Position(fd.End()).Line
+	}
+	declShape := func(d ast.Decl) string {
+		switch x := d.(type) {
+		case *ast.FuncDecl:
+			k := "func"
+			if x.Recv != nil {
+				k = "method"
+			}
+			if goOneLine(d) {
+				return k + "/1line"
+			}
+			return k + "/multi"
+		case *ast.GenDecl:
+			k := genKind(x)
+			if g.fset.Position(x.Pos()).Line == g.fset.Position(x.End()).Line {
+				return k + "/1line"
+			}
+			return k + "/multi"
+		}
+		return "other"
+	}
+	// csBody: the C# member at header line cl -> "expr" (=> on the header), "oneline" ({...} closed on the header),
+	// or the count of non-blank body lines, and the single body line when there is one.
+	csBody := func(cl int) (string, int, string) {
+		h := cs.lines[cl-1]
+		t := strings.TrimSpace(h)
+		if strings.Contains(cs.codeText[cl-1], "=>") && !strings.HasSuffix(t, "{") {
+			return "expr", 0, ""
+		}
+		if !strings.HasSuffix(t, "{") {
+			if strings.HasSuffix(t, "}") {
+				return "oneline", 0, ""
+			}
+			return "unknown", 0, ""
+		}
+		ind := len(h) - len(strings.TrimLeft(h, " \t"))
+		n, one := 0, ""
+		for k := cl + 1; k <= len(cs.lines); k++ {
+			l := cs.lines[k-1]
+			if strings.TrimSpace(l) == "}" && len(l)-len(strings.TrimLeft(l, " \t")) == ind {
+				return "block", n, one
+			}
+			if !isBlankCS(cs, k) {
+				n++
+				one = strings.TrimSpace(l)
+			}
+		}
+		return "unknown", 0, ""
+	}
+	stmtKind := func(fd *ast.FuncDecl) string {
+		if len(fd.Body.List) == 0 {
+			return "empty"
+		}
+		if len(fd.Body.List) > 1 {
+			return "multi-stmt"
+		}
+		switch st := fd.Body.List[0].(type) {
+		case *ast.ReturnStmt:
+			if len(st.Results) > 1 {
+				return "return-tuple"
+			}
+			return "return"
+		case *ast.ExprStmt:
+			return "call"
+		case *ast.AssignStmt, *ast.IncDecStmt:
+			return "assign"
+		default:
+			return fmt.Sprintf("%T", st)[5:]
+		}
+	}
 	decls := g.file.Decls
 	for di, d := range decls {
 		name, startLn := "", 0
@@ -572,15 +647,62 @@
 			r.ex("top:unmapped:"+k, fmt.Sprintf("%s.go:%d", where, startLn), name)
 			continue
 		}
+		if fd, ok := d.(*ast.FuncDecl); ok && goOneLine(d) {
+			kind, n, one := csBody(cl)
+			sk := stmtKind(fd)
+			cshape := kind
+			if kind == "block" {
+				if n == 0 {
+					cshape = "block-0"
+				} else if n == 1 {
+					cshape = "block-1"
+				} else {
+					cshape = "block-n"
+				}
+			}
+			r.oneLine.add("go-1line-funcs", 1)
+			r.fileHit["oneline-any"] = true
+			if cshape == "block-1" || cshape == "block-0" {
+				r.fileHit["oneline-collapsible"] = true
+			}
+			r.oneLine.add("go-stmt:"+sk, 1)
+			r.oneLine.add("cs:"+cshape, 1)
+			r.oneLine.add("go-stmt:"+sk+" cs:"+cshape, 1)
+			r.ex("oneline:"+cshape+":"+sk, fmt.Sprintf("%s.go:%d -> .cs:%d", where, startLn, cl), strings.TrimSpace(cs.lines[cl-1])+" | "+one)
+			if cshape == "block-1" || cshape == "block-0" {
+				h := strings.TrimRight(cs.lines[cl-1], " \t")
+				w := len([]rune(h)) + 2 + len([]rune(one)) + 2
+				b := "<=100"
+				switch {
+				case w > 150:
+					b = ">150"
+				case w > 120:
+					b = "121-150"
+				case w > 100:
+					b = "101-120"
+				}
+				r.oneLine.add("collapsed-width "+b, 1)
+				gl := strings.ReplaceAll(g.src[startLn-1], "\t", "    ")
+				gw := len([]rune(strings.TrimRight(gl, " ")))
+				if gw > 100 {
+					r.oneLine.add("go-width >100", 1)
+				}
+				if w > gw+40 {
+					r.oneLine.add("collapsed-wider-than-go-by-40+", 1)
+				}
+			}
+		}
 		if di > 0 {
 			gb := goBlankAbove(startLn)
 			cb := isBlankCS(cs, csStart(cl)-1)
 			switch {
 			case gb && !cb:
 				r.topLevel.add("go-blank-cs-none", 1)
+				r.kShape.add("DROPPED "+declShape(decls[di-1])+" -> "+declShape(d), 1)
 				r.ex("top:go-blank-cs-none", fmt.Sprintf("%s.go:%d -> .cs:%d", where, startLn, cl), strings.TrimSpace(cs.lines[cl-1]))
 			case !gb && cb:
 				r.topLevel.add("go-none-cs-blank", 1)
+				r.kShape.add(declShape(decls[di-1])+" -> "+declShape(d), 1)
 				r.ex("top:go-none-cs-blank", fmt.Sprintf("%s.go:%d -> .cs:%d", where, startLn, cl), strings.TrimSpace(cs.lines[cl-1]))
 			default:
 				r.topLevel.add("same", 1)
@@ -849,6 +971,8 @@
 	fmt.Printf("EOL C#-side runs %d, misaligned %d\n", r.csRuns, r.csMisaligned)
 	printCounter("BLANK runs of 2+ in C# by context", r.blank2)
 	printCounter("TOP-LEVEL separation", r.topLevel)
+	printCounter("K shapes (prev -> this) of top-level separation findings", r.kShape)
+	printCounter("ONE-LINE Go funcs and their C# emission", r.oneLine)
 	printCounter("GROUPED const/var spec separation", r.groupSpec)
 	printCounter("FUNCTION-BODY blank lines", r.body)
 	printCounter("FILES with at least one finding, per class", r.filesByClass)
```
