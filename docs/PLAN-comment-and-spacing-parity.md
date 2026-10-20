# PLAN — comment and spacing parity: comments in the right place, none missed, Go's column alignment, Go's blank lines

> **STATUS: SCOUT, 2026-10-11, for the owner. Measured at master `20279db5f6` against go1.24.13. Read-only: no
> converter, generator or corpus file was changed.** Taken by C1 on COORD's dispatch of the owner's request. Every
> number is a corpus-wide census over the 1,616 converted production files that have a Go source file (hand-owned
> files excluded), by one instrument per class, each red-tested on planted input (§1). Root causes are cited
> `file:function:line` at that master. Visible-emission changes go to the owner before any build: reads-like-Go is
> his call (§7).
>
> **STATUS UPDATE, 2026-10-13 (C1, at COORD's direction):** the plan's seats are RULED. Classes A through I and L land
> in TRAIN T4 as one chain: seats A, B+C, D/E1/L, H, H2+I, F, the census anchor seat, C aliases, E2, A-scope and G,
> with the gap fix found at T4's census re-baseline (claude/c1-comment-parity-gap-fix: a var or const statement's doc
> in a body no longer gains a blank line Go does not have). Classes K (one-line members) and J (EOL alignment) are
> ruled and HELD for the first post-hop train. Each seat's record is `docs/phase4/CENSUS-comment-parity-seat*.md`;
> the numbers below are the scout's, at `20279db5f6`, and are not restated here.
>
> **The owner's ask (verbatim):** "a comment pass improvement. Specifically targeted at getting comments in the right
> places, picking up comments that are currently missed, better end of line comment alignment (on the order of
> `go fmt`), then, looking for automated code blank lines mismatches, two or more blank lines when there should have
> been one and zero blank lines separating elements when there should be one (expect this last one to be low hanging
> fruit)."
>
> **Builds on:** [`ConversionStrategies-Reference/comments.md`](ConversionStrategies-Reference/comments.md) (the
> standalone-comment path, the trailing-comment rule and its three deferred shapes) and
> [`PLAN-marker-comment-parity.md`](PLAN-marker-comment-parity.md) (comments that CARRY A FACT: `/*num:int64*/`,
> `/*[32]*/`, tag and embed comments). Those fact-carrying comments are converter-written, not Go's, and nothing in
> this plan moves or re-pads them: every proposed pass below skips a comment that is not Go's text (§6).

---

## 0. The answer in one screen

| # | Class | Measured (corpus) | Files | Root cause | Fix effort | Goldens moved |
|:--|:--|:--|--:|:--|:--|:--|
| **A** | **Zero blank lines where Go has one, inside function bodies** | **11,226 of 13,948** Go body blank lines have no C# blank (only 337 kept; 2,385 not mapped) | 767 | a shadowed variable makes the gap logic dead code, `visitBlockStmt.go:59-66` | **0.5–1 day** | 0 (gated on `-comments`) |
| **B** | **Missed comments: doc comments of specs inside a group** | **954 of 958** lost | ≤ 407 | `visitGenDecl.go:22-27` passes only `genDecl.Doc` | 0.5–1 day | 0 |
| **C** | **Missed comments: doc comments of named non-struct types** (`type Flags uint16`) | **422 of 429** lost; aliases 7 of 9 | (in B's 407) | `visitTypeSpec.go:299-407` gives `doc` only to the struct and interface arms | 0.5–1 day | 0 |
| **D** | Blank lines between specs of a group | 617 Go blanks dropped, 2,034 blanks inserted | 131 / 122 | `writeDocString` writes a newline even when the doc was already written (`writeOperations.go:304-306`); no line-gap check in `visitValueSpec` | 0.5 day | 0 |
| **E** | Extra converter blank lines inside bodies (scaffolding) | 3,130 after parameter preludes, 808 after named-result locals, 598 after a lowered switch, 322 before a closing brace | — | `visitFuncDecl.go:1105-1152`, `visitSwitchStmt.go:933-940` | 0.5 day | 160 (§4.4): owner's call |
| **F** | Misplaced comments in composite literals | 2,440 of 3,476 (1,574 trailing → own line; 844 anchored elsewhere) | 85 | `visitValueSpec.go:28` flushes every comment up to the spec's END before the declaration is written | 1–2 days | 0 |
| **G** | Misplaced comments in other expressions | 1,005 of 1,039 (991 trailing → own line) | 40 | `convExprList.go:79` writes them on their own line; `convBinaryExpr` has no flush | 1 day | 0 |
| **H** | Misplaced after `{` / after `case X:` / before a case / before `}` | 366 of 366 / 199 of 215 / 148 of 234 / 170 of 180 | 166 / 54 / 79 / 108 | the three shapes comments.md defers; case bodies have no flush of their own | 1.5–2 days | 0 |
| **I** | Misplaced free statement comments | 1,009 of 12,070 (726 anchored elsewhere, 283 own line → trailing) | 286 | mostly H's flush gaps (empty blocks, case bodies) | in H | 0 |
| **J** | **EOL alignment** | of **1,161** Go runs gofmt aligned, **95** stay aligned; 770 misaligned, 109 split by blank lines, 80 moved to own lines | 338 + 63 | statement comments get one space by design; spec and field comments replay Go's source padding (`writeCommentString:324-332`) | 2–3 days | 0 |
| **K** | Top-level separation | 59 Go blanks dropped (lifted anonymous-struct vars); 1,067 blanks inserted between declarations Go writes adjacently (one-line methods) | 40 / 220 | §4.9 | 0.5 day | some (§4.9) |
| **L** | Runs of 2+ blank lines | **72 runs** (69 at member boundaries, 3 near scaffolding), 144 lines | 17 | §4.9 | in K | 21 |

Comment lines overall: **100,236 present, 4,233 missed (4.1%)**; groups by class in §2.

**The owner's guess is confirmed, and it is the biggest single defect:** Go's blank lines inside function bodies are
dropped almost everywhere (A), and the cause is a one-line bug, not a missing feature. **B and C are the "picking up
missed comments" half and are nearly as cheap.** Two or more blank lines (L) are rare: 72 runs.

**Total effort: 8–12 lane-days** for A–L, plus about 0.5 day per fix for the corpus footprint (the two-seeded diff),
**10–15 lane-days in all**. **Order (§6):** A → B+C+D with K's lifted var and L → E (owner's call) → H+I → F+G → J (K's one-liners only if ruled). **Timing (§6):**
land the train before the Go 1.26 hop's H0 baseline, not inside the hop: H4 requires a byte-identical corpus check
and H5's attribution would be flooded by about 1,000 formatting-only file diffs.

---

## 1. The instrument and its red tests

`ccensus` (Appendix A, uncommitted; Go, standard library only) pairs every converted production `.cs` in `src/core`
with its Go source under `GOROOT/src` by package path and file name (flat file preferred, else the windows, linux,
darwin flavour), skipping hand-owned files (`[module: GoManualConversion]`, `*_impl.cs`), test files and
`package_info.cs`. 1,616 pairs. One measure per class:

| Class | Measure |
|:--|:--|
| **Missed** | every Go comment group (go/ast, `ParseComments`) is classified by where Go puts it (below); its text lines, whitespace-normalized, are assigned in file order to C# comment occurrences of the same text (a C# lexer that skips string, verbatim, raw and char literals). A group with no line found is MISSED, some lines found is PARTIAL. Directives (`//go:`, `//line`) are counted apart |
| **Misplaced** | for a fully present group: Go's anchor is the code on the comment's own line (trailing) or the next code line (leading); C#'s anchor is found the same way, skipping attribute lines. MISPLACED if trailing became own-line or the reverse, or if the first identifier of Go's anchor line is absent from C#'s anchor line (identifiers normalized for the converter's `Ꮡ`, `Δ`, `@`, `ʗp` decorations) |
| **EOL alignment** | Go runs: consecutive Go lines at one indentation that each end in a `//` comment. gofmt aligned a run if its comment columns are equal (tabs at 8). For each aligned run, the C# lines of its comments are ALIGNED (consecutive, one column), MISALIGNED (consecutive, ragged), SPLIT (not consecutive) or MOVED (a comment no longer trails) |
| **2+ blank lines** | runs of two or more blank lines in C#, classified by their neighbours as member boundary, inside a body or scaffolding |
| **0 blank lines** | top level: each Go declaration's blank-line-above against the C# declaration found by its name (ordered search, declaration-shaped match); groups: the same per spec; bodies: each Go blank line inside a function body is matched to its element in C# (a comment by text, a statement by its identifiers) and checked for a blank above; every C# blank in a body that no Go blank explains is classified as scaffolding (after a parameter prelude, after named-result locals, after a lowered switch, before `}`) or unexplained |

Go comment classes: `file-header` (before `package`), `package-doc`, `decl-doc` (with the declaration kind),
`spec-doc-in-group`, `field-doc`, `trailing-spec`, `trailing-field` (the AST-attached ones); for free comments,
`in-composite-literal`, `in-expression`, `free-in-type`, `after-open-brace`, `after-case-label`, `trailing-stmt`,
`before-else`, `before-case`, `before-close-brace`, `free-stmt`, `free-top-level`.

**Red tests (planted input, Appendix B).** A planted Go file and three C# renderings of it:

| Rendering | Planted defects | Instrument reads |
|:--|:--|:--|
| `good` (Go's structure exactly) | none | **every class zero**; EOL 2 of 2 runs aligned; top, group and body separation all same |
| `bad` | a missed group doc; a trailing comment after `{` moved into the body; a composite-literal row comment hoisted above the declaration; a ragged EOL run; two blank lines between declarations; a dropped top-level blank; a dropped group blank; a dropped body blank | **each named exactly once**, nothing else |
| `bad2` | a comment moved to a different statement; a doc comment pulled onto the previous `}`; an EOL run split by a blank; a blank inserted inside a group; a blank before a closing brace; an extra body blank | **each named exactly once** |

Two instrument faults were found by this red-testing and corpus sampling and fixed before any number here was taken:
an any-shared-identifier anchor test passed a comment moved to a neighbouring statement (now: Go's first identifier
must be on C#'s anchor line), and doc comments above an attribute or a `Ꮡ`-renamed receiver read as misplaced
(attribute lines are skipped; decorations are normalized). Each large class was then sampled against the files
(§3 quotes the samples).

---

## 2. What Go has, and what survives

| Go comment class | Groups | Missed | Partial | Misplaced |
|:--|--:|--:|--:|--:|
| decl-doc | 12,773 | **432** (type-other 422 of 429, alias 7 of 9) | 8 | 48 |
| free-stmt | 12,082 | 6 | 6 | 1,009 |
| in-composite-literal | 3,477 | 1 | 0 | **2,440** |
| trailing-spec | 2,844 | 85 (import lines, §4.2) | 0 | 12 |
| trailing-field | 2,634 | 2 | 0 | 12 |
| trailing-stmt | 2,462 | 1 | 0 | 103 |
| file-header | 1,894 | 0 | 0 | — |
| field-doc | 1,878 | 17 | 1 | 10 |
| in-expression | 1,040 | 1 | 0 | **1,005** |
| spec-doc-in-group | 958 | **954** | 2 | 2 |
| directive | 700 | — | — | — |
| free-top-level | 628 | 0 | 0 | 9 |
| after-open-brace | 366 | 0 | 0 | **366** |
| package-doc | 263 | 0 | 0 | — |
| before-case | 234 | 0 | 0 | 148 |
| after-case-label | 217 | 2 | 0 | 199 |
| before-close-brace | 180 | 0 | 0 | 170 |
| free-in-type | 31 | 0 | 0 | 13 |

Missed by package, top: `internal/types/errors` 151 of 163 (its error-code const group), `runtime` 119 of 3,556,
`html/template` 81 of 456, `net/http` 70 of 2,486, `debug/elf` 66 of 969, `go/ast` 60 of 501, `crypto/tls` 49 of 908.

## 3. The worked example: `unicode/utf8/utf8.cs` against Go's `utf8.go`

The owner's exemplar, converted with `-comments` as the standard library is. The census of this one file (§1's
instrument, `-only unicode/utf8/utf8`): 90 comment groups; **2 missed** (5 lines), **19 misplaced** (17 in the `first`
table, 2 after a header brace); of **4** Go runs gofmt aligned, **1** stays aligned (the `acceptRange` fields),
1 misaligned, 1 split, 2 moved; group spacing **4** Go blanks dropped and **4** blanks inserted; body spacing **1** Go
blank dropped and **7** scaffolding blanks added (3 after named-result locals, 2 after a lowered switch, 2 before a
closing brace). No run of two blank lines. Classes A, B, D, E, F, H (after a header brace) and J appear in this file;
C, G, I, K, L and the case-label shapes do not, and §4 gives their corpus examples. Each block shows Go, then today's C#, then the C# after the fix named in its heading (the after-fix
text is hand-rendered to the proposed rule, not converter output).

### W1. The constants: B (missed group docs), D (group blank lines), J (alignment)

Go (`utf8.go:10-61`):

```go
// The conditions RuneError==unicode.ReplacementChar and
// MaxRune==unicode.MaxRune are verified in the tests.
// Defining them locally avoids this package depending on package unicode.

// Numbers fundamental to the encoding.
const (
	RuneError = '\uFFFD'     // the "error" Rune or "Unicode replacement character"
	RuneSelf  = 0x80         // characters below RuneSelf are represented as themselves in a single byte.
	MaxRune   = '\U0010FFFF' // Maximum valid Unicode code point.
	UTFMax    = 4            // maximum number of bytes of a UTF-8 encoded Unicode character.
)

// Code points in the surrogate range are not valid for UTF-8.
const (
	surrogateMin = 0xD800
	surrogateMax = 0xDFFF
)

const (
	t1 = 0b00000000
	tx = 0b10000000
	t2 = 0b11000000
	t3 = 0b11100000
	t4 = 0b11110000
	t5 = 0b11111000

	maskx = 0b00111111
	mask2 = 0b00011111
	mask3 = 0b00001111
	mask4 = 0b00000111

	rune1Max = 1<<7 - 1
	rune2Max = 1<<11 - 1
	rune3Max = 1<<16 - 1

	// The default lowest and highest continuation byte.
	locb = 0b10000000
	hicb = 0b10111111

	// These names of these constants are chosen to give nice alignment in the
	// table below. The first nibble is an index into acceptRanges or F for
	// special one-byte cases. The second nibble is the Rune length or the
	// Status for the special one-byte case.
	xx = 0xF1 // invalid: size 1
	as = 0xF0 // ASCII: size 1
	s1 = 0x02 // accept 0, size 2
	s2 = 0x13 // accept 1, size 3
	s3 = 0x03 // accept 0, size 3
	s4 = 0x23 // accept 2, size 3
	s5 = 0x34 // accept 3, size 4
	s6 = 0x04 // accept 0, size 4
	s7 = 0x44 // accept 4, size 4
```

C# today (`utf8.cs:12-54`): the two docs inside the group (`The default lowest…`, `These names…`) are gone; the
blank lines between Go's sub-groups are gone; a blank line is inserted after each documented spec
(RuneError…UTFMax, surrogateMin); the RuneError run, which gofmt aligned, is split; the xx…s7 run is ragged by one
column because `@as` is one character longer than `xx`.

```csharp
// The conditions RuneError==unicode.ReplacementChar and
// MaxRune==unicode.MaxRune are verified in the tests.
// Defining them locally avoids this package depending on package unicode.

// Numbers fundamental to the encoding.
public static UntypedInt RuneError => /* '\uFFFD' */ 65533; // the "error" Rune or "Unicode replacement character"

public static UntypedInt RuneSelf => 0x80; // characters below RuneSelf are represented as themselves in a single byte.

public static UntypedInt MaxRune => /* '\U0010FFFF' */ 1114111; // Maximum valid Unicode code point.

public static UntypedInt UTFMax => 4; // maximum number of bytes of a UTF-8 encoded Unicode character.

// Code points in the surrogate range are not valid for UTF-8.
internal static UntypedInt surrogateMin => 0xD800;

internal static UntypedInt surrogateMax => 0xDFFF;

internal static UntypedInt t1 => 0b00000000;
internal static UntypedInt tx => 0b10000000;
internal static UntypedInt t2 => 0b11000000;
internal static UntypedInt t3 => 0b11100000;
internal static UntypedInt t4 => 0b11110000;
internal static UntypedInt t5 => 0b11111000;
internal static UntypedInt maskx => 0b00111111;
internal static UntypedInt mask2 => 0b00011111;
internal static UntypedInt mask3 => 0b00001111;
internal static UntypedInt mask4 => 0b00000111;
internal static UntypedInt rune1Max => /* 1<<7 - 1 */ 127;
internal static UntypedInt rune2Max => /* 1<<11 - 1 */ 2047;
internal static UntypedInt rune3Max => /* 1<<16 - 1 */ 65535;
internal static UntypedInt locb => 0b10000000;
internal static UntypedInt hicb => 0b10111111;
internal static UntypedInt xx => 0xF1; // invalid: size 1
internal static UntypedInt @as => 0xF0; // ASCII: size 1
internal static UntypedInt s1 => 0x02; // accept 0, size 2
internal static UntypedInt s2 => 0x13; // accept 1, size 3
internal static UntypedInt s3 => 0x03; // accept 0, size 3
internal static UntypedInt s4 => 0x23; // accept 2, size 3
internal static UntypedInt s5 => 0x34; // accept 3, size 4
internal static UntypedInt s6 => 0x04; // accept 0, size 4
internal static UntypedInt s7 => 0x44; // accept 4, size 4

```

C# after B, D and J:

```csharp
// The conditions RuneError==unicode.ReplacementChar and
// MaxRune==unicode.MaxRune are verified in the tests.
// Defining them locally avoids this package depending on package unicode.

// Numbers fundamental to the encoding.
public static UntypedInt RuneError => /* '\uFFFD' */ 65533;     // the "error" Rune or "Unicode replacement character"
public static UntypedInt RuneSelf => 0x80;                      // characters below RuneSelf are represented as themselves in a single byte.
public static UntypedInt MaxRune => /* '\U0010FFFF' */ 1114111; // Maximum valid Unicode code point.
public static UntypedInt UTFMax => 4;                           // maximum number of bytes of a UTF-8 encoded Unicode character.

// Code points in the surrogate range are not valid for UTF-8.
internal static UntypedInt surrogateMin => 0xD800;
internal static UntypedInt surrogateMax => 0xDFFF;

internal static UntypedInt t1 => 0b00000000;
internal static UntypedInt tx => 0b10000000;
internal static UntypedInt t2 => 0b11000000;
internal static UntypedInt t3 => 0b11100000;
internal static UntypedInt t4 => 0b11110000;
internal static UntypedInt t5 => 0b11111000;

internal static UntypedInt maskx => 0b00111111;
internal static UntypedInt mask2 => 0b00011111;
internal static UntypedInt mask3 => 0b00001111;
internal static UntypedInt mask4 => 0b00000111;

internal static UntypedInt rune1Max => /* 1<<7 - 1 */ 127;
internal static UntypedInt rune2Max => /* 1<<11 - 1 */ 2047;
internal static UntypedInt rune3Max => /* 1<<16 - 1 */ 65535;

// The default lowest and highest continuation byte.
internal static UntypedInt locb => 0b10000000;
internal static UntypedInt hicb => 0b10111111;

// These names of these constants are chosen to give nice alignment in the
// table below. The first nibble is an index into acceptRanges or F for
// special one-byte cases. The second nibble is the Rune length or the
// Status for the special one-byte case.
internal static UntypedInt xx => 0xF1;  // invalid: size 1
internal static UntypedInt @as => 0xF0; // ASCII: size 1
internal static UntypedInt s1 => 0x02;  // accept 0, size 2
internal static UntypedInt s2 => 0x13;  // accept 1, size 3
internal static UntypedInt s3 => 0x03;  // accept 0, size 3
internal static UntypedInt s4 => 0x23;  // accept 2, size 3
internal static UntypedInt s5 => 0x34;  // accept 3, size 4
internal static UntypedInt s6 => 0x04;  // accept 0, size 4
internal static UntypedInt s7 => 0x44;  // accept 4, size 4
```

(The Go comment says the names were "chosen to give nice alignment in the table below". `as` is a C# keyword and
becomes `@as`, which breaks that table's alignment by one column per cell in C#; nothing in this plan restores it.)

### W2. The `first` table: F (composite-literal comments) and J

Go (`utf8.go:71-90`): a header comment line before each half and a trailing row label on every row.

```go
var first = [256]uint8{
	//   1   2   3   4   5   6   7   8   9   A   B   C   D   E   F
	as, as, as, as, as, as, as, as, as, as, as, as, as, as, as, as, // 0x00-0x0F
	as, as, as, as, as, as, as, as, as, as, as, as, as, as, as, as, // 0x10-0x1F
	as, as, as, as, as, as, as, as, as, as, as, as, as, as, as, as, // 0x20-0x2F
	as, as, as, as, as, as, as, as, as, as, as, as, as, as, as, as, // 0x30-0x3F
	as, as, as, as, as, as, as, as, as, as, as, as, as, as, as, as, // 0x40-0x4F
	as, as, as, as, as, as, as, as, as, as, as, as, as, as, as, as, // 0x50-0x5F
	as, as, as, as, as, as, as, as, as, as, as, as, as, as, as, as, // 0x60-0x6F
	as, as, as, as, as, as, as, as, as, as, as, as, as, as, as, as, // 0x70-0x7F
	//   1   2   3   4   5   6   7   8   9   A   B   C   D   E   F
	xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, // 0x80-0x8F
	xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, // 0x90-0x9F
	xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, // 0xA0-0xAF
	xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, // 0xB0-0xBF
	xx, xx, s1, s1, s1, s1, s1, s1, s1, s1, s1, s1, s1, s1, s1, s1, // 0xC0-0xCF
	s1, s1, s1, s1, s1, s1, s1, s1, s1, s1, s1, s1, s1, s1, s1, s1, // 0xD0-0xDF
	s2, s3, s3, s3, s3, s3, s3, s3, s3, s3, s3, s3, s3, s4, s3, s3, // 0xE0-0xEF
	s5, s6, s6, s6, s7, xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, // 0xF0-0xFF
}
```

C# today (`utf8.cs:59-96`): all 18 comments are hoisted above the declaration, above its own doc comment, and stripped
from their rows (`visitValueSpec.go:28` flushes every pending comment up to the spec's END before writing the spec).

```csharp
//   1   2   3   4   5   6   7   8   9   A   B   C   D   E   F
// 0x00-0x0F
// 0x10-0x1F
// 0x20-0x2F
// 0x30-0x3F
// 0x40-0x4F
// 0x50-0x5F
// 0x60-0x6F
// 0x70-0x7F
//   1   2   3   4   5   6   7   8   9   A   B   C   D   E   F
// 0x80-0x8F
// 0x90-0x9F
// 0xA0-0xAF
// 0xB0-0xBF
// 0xC0-0xCF
// 0xD0-0xDF
// 0xE0-0xEF
// 0xF0-0xFF
// first is information about the first byte in a UTF-8 sequence.
internal static array<uint8> first = new uint8[]{
    @as, @as, @as, @as, @as, @as, @as, @as, @as, @as, @as, @as, @as, @as, @as, @as,
    @as, @as, @as, @as, @as, @as, @as, @as, @as, @as, @as, @as, @as, @as, @as, @as,
    @as, @as, @as, @as, @as, @as, @as, @as, @as, @as, @as, @as, @as, @as, @as, @as,
    @as, @as, @as, @as, @as, @as, @as, @as, @as, @as, @as, @as, @as, @as, @as, @as,
    @as, @as, @as, @as, @as, @as, @as, @as, @as, @as, @as, @as, @as, @as, @as, @as,
    @as, @as, @as, @as, @as, @as, @as, @as, @as, @as, @as, @as, @as, @as, @as, @as,
    @as, @as, @as, @as, @as, @as, @as, @as, @as, @as, @as, @as, @as, @as, @as, @as,
    @as, @as, @as, @as, @as, @as, @as, @as, @as, @as, @as, @as, @as, @as, @as, @as,
    xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, xx,
    xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, xx,
    xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, xx,
    xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, xx,
    xx, xx, s1, s1, s1, s1, s1, s1, s1, s1, s1, s1, s1, s1, s1, s1,
    s1, s1, s1, s1, s1, s1, s1, s1, s1, s1, s1, s1, s1, s1, s1, s1,
    s2, s3, s3, s3, s3, s3, s3, s3, s3, s3, s3, s3, s3, s4, s3, s3,
    s5, s6, s6, s6, s7, xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, xx
}.array();

```

C# after F and J:

```csharp
// first is information about the first byte in a UTF-8 sequence.
internal static array<uint8> first = new uint8[]{
    //   1   2   3   4   5   6   7   8   9   A   B   C   D   E   F
    @as, @as, @as, @as, @as, @as, @as, @as, @as, @as, @as, @as, @as, @as, @as, @as, // 0x00-0x0F
    @as, @as, @as, @as, @as, @as, @as, @as, @as, @as, @as, @as, @as, @as, @as, @as, // 0x10-0x1F
    @as, @as, @as, @as, @as, @as, @as, @as, @as, @as, @as, @as, @as, @as, @as, @as, // 0x20-0x2F
    @as, @as, @as, @as, @as, @as, @as, @as, @as, @as, @as, @as, @as, @as, @as, @as, // 0x30-0x3F
    @as, @as, @as, @as, @as, @as, @as, @as, @as, @as, @as, @as, @as, @as, @as, @as, // 0x40-0x4F
    @as, @as, @as, @as, @as, @as, @as, @as, @as, @as, @as, @as, @as, @as, @as, @as, // 0x50-0x5F
    @as, @as, @as, @as, @as, @as, @as, @as, @as, @as, @as, @as, @as, @as, @as, @as, // 0x60-0x6F
    @as, @as, @as, @as, @as, @as, @as, @as, @as, @as, @as, @as, @as, @as, @as, @as, // 0x70-0x7F
    //   1   2   3   4   5   6   7   8   9   A   B   C   D   E   F
    xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, // 0x80-0x8F
    xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, // 0x90-0x9F
    xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, // 0xA0-0xAF
    xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, // 0xB0-0xBF
    xx, xx, s1, s1, s1, s1, s1, s1, s1, s1, s1, s1, s1, s1, s1, s1, // 0xC0-0xCF
    s1, s1, s1, s1, s1, s1, s1, s1, s1, s1, s1, s1, s1, s1, s1, s1, // 0xD0-0xDF
    s2, s3, s3, s3, s3, s3, s3, s3, s3, s3, s3, s3, s3, s4, s3, s3, // 0xE0-0xEF
    s5, s6, s6, s6, s7, xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, // 0xF0-0xFF
}.array();
```

### W3. After a header brace: H

Go (`utf8.go:180-182`), C# today (`utf8.cs:187-190`), C# after H:

```go
	if sz <= 2 { // <= instead of == to help the compiler eliminate some bounds checks
		return rune(p0&mask2)<<6 | rune(b1&maskx), 2
	}
```

```csharp
    if (sz <= 2) {
        // <= instead of == to help the compiler eliminate some bounds checks
        return ((rune)(((rune)((byte)(p0 & (byte)mask2)) << (int)(6)) | (rune)((byte)(b1 & (byte)maskx))), 2);
    }
```

```csharp
    if (sz <= 2) { // <= instead of == to help the compiler eliminate some bounds checks
        return ((rune)(((rune)((byte)(p0 & (byte)mask2)) << (int)(6)) | (rune)((byte)(b1 & (byte)maskx))), 2);
    }
```

### W4. A blank line inside a body: A (the low-hanging one)

Go (`utf8.go:439-447`), C# today (`utf8.cs:480-487`), C# after A:

```go
// Valid reports whether p consists entirely of valid UTF-8-encoded runes.
func Valid(p []byte) bool {
	// This optimization avoids the need to recompute the capacity
	// when generating code for p[8:], bringing it to parity with
	// ValidString, which was 20% faster on long ASCII strings.
	p = p[:len(p):len(p)]

	// Fast path. Check for and skip 8 bytes of ASCII characters per iteration.
	for len(p) >= 8 {
```

```csharp
// Valid reports whether p consists entirely of valid UTF-8-encoded runes.
public static bool Valid(slice<byte> p) {
    // This optimization avoids the need to recompute the capacity
    // when generating code for p[8:], bringing it to parity with
    // ValidString, which was 20% faster on long ASCII strings.
    p = p.slice(0, len(p), len(p));
    // Fast path. Check for and skip 8 bytes of ASCII characters per iteration.
    while (len(p) >= 8) {
```

```csharp
public static bool Valid(slice<byte> p) {
    // This optimization avoids the need to recompute the capacity
    // when generating code for p[8:], bringing it to parity with
    // ValidString, which was 20% faster on long ASCII strings.
    p = p.slice(0, len(p), len(p));

    // Fast path. Check for and skip 8 bytes of ASCII characters per iteration.
    while (len(p) >= 8) {
```

### W5. Converter scaffolding blanks: E (owner's call, §7)

Named-result locals (`DecodeLastRune`, `utf8.cs:262-266`): Go's body starts at `end := len(p)`; the C# adds two locals
and a blank line Go does not have.

```csharp
public static (rune r, nint size) DecodeLastRune(slice<byte> p) {
    rune r = default!;
    nint size = default!;

    nint end = len(p);
```

A lowered `switch` (`encodeRuneNonASCII`, `utf8.cs:405-414`): a blank before the function's closing brace, which Go
never has (`RuneLen`, `utf8.cs:342-361`, also has one right after its switch's `}}`).

```csharp
            _ = p[2]; // eliminate bounds checks
            p[0] = runeErrorByte0;
            p[1] = runeErrorByte1;
            p[2] = runeErrorByte2;
            return 3;
        }}
    }

}

```

After E (faithful option: no blank Go does not have):

```csharp
public static (rune r, nint size) DecodeLastRune(slice<byte> p) {
    rune r = default!;
    nint size = default!;
    nint end = len(p);
```

```csharp
            return 3;
        }}
    }
}
```

### W6. What already matches

The file header, the package doc, all 20 declaration docs, every statement comment (`return true // ASCII, invalid or
valid.` stays trailing), the `acceptRange` field comments (the one aligned run), and top-level separation (23 of 23
declarations). The doc of a named non-struct type (C) does not occur in this file; `regexp/syntax/parse.go`'s
`type Flags uint16` is the corpus example (§4.2).


## 4. The classes: cause, fix, effort, churn, guard

Effort is in lane-days, an estimate, and includes a converter test with positive and negative controls and the corpus
footprint (the two-seeded diff of the `corpus-reconvert` skill). "Goldens" means behavioral `.cs.target` files, which
are captured **without** `-comments`: a fix that only runs under `-comments` moves none. "Corpus files" are the
converted files the census found with at least one instance (of 1,616).

### 4.1 A — blank lines inside function bodies (the owner's "zero where there should be one")

**Measured:** 13,948 Go blank lines between statements inside function bodies; **11,226 have no C# blank line**, 337
do, 2,385 could not be mapped to a C# element (mostly statements whose identifiers the lowering renames). Corpus files: 767.
Samples: `archive/zip/writer.go:351`, `compress/flate/dict_decoder.go:125`, `archive/tar/reader.go:159`,
`compress/flate/inflate.go:643`, and W4.

**Root cause: the logic exists and is dead.** `visitBlockStmt.go:48-73` is the only code that reads statement lines:

- line 59 declares `var wrote bool`; line 61 `if wrote, lines := v.writeStandAloneCommentString(...); wrote {`
  declares a NEW `wrote` scoped to that `if`, so the outer one stays false and line 65
  (`if wrote && currentLine-lastLine > 1`) never fires;
- even with the shadowing fixed it would keep a blank only when a comment sits in the gap, and only under
  `-comments` (line 49);
- line 62's `lastLine -= lines - 1` is wrong too: `visitFile.go:85-88` stores each `//` line as its own entry, so
  `lines` comes back 0 for a `//` group (`writeOperations.go:115-119`), and the shadowing fix alone would ADD two
  false blank lines above every three-line comment.

Case and comm-clause bodies (`visitSwitchStmt.go:723`, `visitTypeSwitchStmt.go:178`, `visitSelectStmt.go:398`) call
`visitListStmt` directly and have no gap logic at all.

**Fix:** for each statement after the first, take the start line of the first comment group between the previous
statement's `End()` and this statement (or the statement's own line when there is none); if it is more than one line
past the previous statement's last line, write one `v.newline` before the comments (gofmt collapses runs to one).
Remove the shadowed variable and the `lines` arithmetic. Put the loop in one helper and call it from the case and
comm-clause bodies too (H needs the same helper). **Effort 0.5–1.** **Corpus files 767** (whitespace only: a
whitespace-insensitive diff is empty). **Goldens 0** if the gap check stays under `-comments`; about **650 of the 819
behavioral directories** if it runs everywhere (653 test Go files carry 3,939 body blank lines) — §7 decision 1.
**Guard:** a converter test with a blank line before a statement, before a commented statement and inside a case
body, plus the negative control (no blank in Go, none in C#; a three-line comment gains no blank); and the census
ratchet (§5).

### 4.2 B and C — missed doc comments (the owner's "picking up missed comments")

**Measured:** 4,233 of 104,469 Go comment lines (4.1%) are missing from the C#. Almost all of it is two shapes:

- **B, a spec's doc inside a grouped declaration:** 954 of 958 lost (const 787 of 791, var 99, type 66). Examples:
  `utf8.go:45,49` (W1), `crypto/x509/verify.go:49` (`UnconstrainedName results when…`), `archive/tar/format.go:59`
  (`FormatUSTAR represents…`), `go/doc/doc.go:103`, and `internal/types/errors` (151 of its 163 groups: the error-code
  table loses its documentation).
- **C, the doc of a named non-struct, non-interface type** (`type Flags uint16`, `type hostLookupOrder int`): 422 of
  429 lost; type aliases 7 of 9. Example: `regexp/syntax/parse.go:53` → `parse.cs:48`, `partial struct Flags
  /*num:uint16*/;` with no doc.
- The rest is small: 85 trailing comments on import specs (`_ "unsafe" // for linkname`; imports become `using` lines,
  a benign loss) and 17 field docs.

**Root cause, B:** `visitGenDecl.go:16-36` passes `genDecl.Doc` to every spec (lines 22, 24, 27) and never `spec.Doc`;
`visitValueSpec.go:26-28` writes the doc it is given; `valueSpec.Doc` is read only for the linkname check
(`visitValueSpec.go:271`) and `typeSpec.Doc` never. `visitFile.go:58-68` removes every AST-attached group from the
free-comment set, so the spec doc is neither passed in nor flushed: a silent loss. **C:** `visitTypeSpec.go:299-407`
hands `doc` only to the struct (404) and interface (309) arms; the identifier arm goes to `visitIdent.go:66-104`,
which writes `partial struct X /*num:…*/;` with no doc; aliases go to `typeAliasDeclarations` (178), which takes none.

**Fix:** B, in `visitGenDecl`, for a grouped declaration write `genDecl.Doc` once before the group and pass each
spec's own `Doc`; ungrouped declarations keep `genDecl.Doc` (go/parser leaves the spec's `Doc` nil there). C, write
the doc centrally in `visitTypeSpec` for every arm other than struct and interface; for an alias, at the point Go
declared it (§7 decision 6). **Effort 0.5–1 each.** **Corpus files: at most 407** (the files with any missed group,
imports included). **Goldens 0.** **Guard:** the census ratchet holds missed-by-class at its new value (B and C near
zero); a converter test per shape.

### 4.3 D — blank lines between the specs of a group

**Measured:** 617 Go blank lines between sub-groups dropped (W1: before `maskx`, `rune1Max`, `locb`, `xx`), and
**2,034 blank lines inserted** between specs Go writes adjacently (W1: after RuneError, RuneSelf, MaxRune;
`regexp/syntax/parse.cs`: `opLeftParen`, blank, `opVerticalBar`). Corpus files 131 and 122.

**Root cause:** inserted: `writeOperations.go:writeDocString:299-306` writes the indent and a newline whenever `doc`
is non-nil, even when `writeCommentString` returned early because the group's doc was already written
(`processedComments`, line 314). Every spec after the first in a documented group gains a blank line (undocumented
groups, like `t1…t5`, do not). Dropped: `visitValueSpec.go:27` writes exactly one newline per spec and nothing
compares Go line numbers. **Fix:** `writeCommentString` returns whether it wrote, and `writeDocString` writes its
newline only then; `visitGenDecl` passes the previous spec's end line and a spec whose doc-or-name starts more than one
line later gets one blank. **Effort 0.5.** **Goldens 0** under `-comments` (ungated, up to 5, all in
`StdLibInternalAbi`). **Guard:** W1's constant block is the test.

### 4.4 E — the converter's own blank lines inside bodies

**Measured** (C# blank lines in bodies that no Go blank explains): 3,130 after a receiver or parameter prelude
(`ref var p = ref Ꮡp.DerefOrNull();`, `heap<…>(out var Ꮡx)`, `f = f.Clone();`, `valsʗp.sslice()`), 808 after
named-result locals (W5), 598 after a lowered switch's `}}`, 322 before a function's closing brace, 953 other.

**Root cause:** `visitFuncDecl.go:1105` (named results and the variadic prelude), `:1111` (`Clone`), `:1119`
(`DerefOrNull`), `:1129` (heap boxes), `:1141` (cgo), `:1152` (named-result aliases) each end the prelude with a
newline; `visitSwitchStmt.go:933-934` writes `}` and then a newline after a lowered switch (every statement already
starts with its own newline), and `:938-940` closes a switch's init sub-block with `writeOutputLn`, which together with
`visitBlockStmt.go:105` puts a blank line before `}`. **Fix:** E1, drop the after-switch and before-brace newlines
(`writeOutput` instead of `writeOutputLn`): Go never has either. E2, the prelude newlines: drop them (faithful), or keep
exactly one as a deliberate separator between converter scaffolding and the Go body — §7 decision 2. **Effort 0.5.**
**Goldens: 160 of 865 carry at least one** (136 prelude, 23 after-switch, 15 before-brace) — these blanks are written
without `-comments` too, so this fix moves goldens unless gated.

### 4.5 F — comments inside composite literals

**Measured:** 2,440 of 3,476 misplaced: 1,574 trailing row comments moved to their own line, 844 anchored to a
different line. Corpus files 85. W2 is the shape at package level; `vendor/golang.org/x/net/idna/tables15.0.0.go`
(`// Block 0x96, offset 0x488` on every block) is the largest single file.

**Root cause:** package level: `visitValueSpec.go:28` calls `v.writeDoc(doc, valueSpec.End())`, and `writeDocString`
first flushes every pending free comment up to `targetPos`, the spec's END: every comment inside the literal is
written before the declaration, above its own doc comment. Inside function bodies the comments do reach
`convExprList.go:79`, but the prefix adds a newline and the `exprOnNewLine` branch a second one, so a trailing comment
stays trailing and a blank line follows it (`time/time.go:1487` → `time.cs:1508-1510`); comments after the last element
are never flushed (`convCompositeLit.go:1185`). **Fix:** in `visitValueSpec`, flush only up to `valueSpec.Pos()`
before the doc, and drain up to `End()` after the spec as a backstop so nothing leaks into the next declaration. In
`convExprList`: a comment on the previous element's line is written ` // c` before that line's newline; an own-line
comment is newline, indent, comment; add a flush before the closing `}` and `)` that always ends with a newline so
`}.array()` is never commented out (the `trailingComments_test.go` controls). **Effort 1–2.** **Goldens 0.**

### 4.6 G — comments inside other expressions

**Measured:** 1,005 of 1,039 misplaced, 991 of them trailing comments moved to their own line. Corpus files 40.
Example: `crypto/tls/ech.go:250-253` (a comment on each `||` operand) → `ech.cs:279-282` (all four inside the `if`
body). **Root cause:** call arguments share `convExprList`'s path (F); `convBinaryExpr` writes the whole expression
on one line with no flush; function-parameter comments are flushed only up to `funcDecl.Pos()`
(`visitFuncDecl.go:652`) and drain into the body. **Fix, three options (§7 decision 3):** write an operand's or
parameter's comment inline as `/* c */` (no layout change); or keep Go's line breaks in these expressions (layout
change, moves goldens unless gated); or leave them as own-line comments at the start of the body. **Effort 1.**

### 4.7 H and I — after `{`, after `case X:`, before a case, before `}`, and free statement comments

**Measured:** after a header brace 366 of 366; after a case label 199 of 215; before a case clause 148 of 234; before a
closing brace 170 of 180; free statement comments 1,009 of 12,070 (726 anchored elsewhere, 283 own-line made
trailing). Corpus files 166 / 54 / 79 / 108 / 286. These are the three shapes comments.md lists as deferred, plus the
case bodies that have no flush of their own. Samples:

- `regexp/syntax/parse.go:628-630`, `if i == start { // Nothing to do… } else if …`: the empty block never flushes and
  the comment moves into the next `else if` body (`parse.cs:612-615`);
- `encoding/json/decode.go:901-904`: a comment before `}` and a `case 't', 'f': // true, false` label comment are both
  flushed into the first nested `if` of the next case (`decode.cs:997-1000`);
- `fmt/print.go:953-955`: a `default:` holding only `// Already 0, false.` comes out after the whole switch
  (`print.cs:1225`).

**Root cause:** after `{`: the comment lies before `List[0].Pos()`, so `visitBlockStmt.go:45` writes it on its own line
inside the body; nothing checks the line of the brace. After `case X:`: the case loops call only `visitListStmt`, with
no leading or between-statement flush, so these comments drift to whatever flushes next. Before `}`: no flush to
`Rbrace` before the block closes. **Fix:** a trailing-comment variant keyed on `blockStmt.Lbrace`'s line written right
after the `{`; the shared statement-list helper (4.1) for every case and comm-clause body, with a trailing comment keyed
on `caseClause.Colon` and a flush up to the next clause or `Rbrace` at the body's end; a flush to `Rbrace` before
`indentLevel--`. **Effort 1.5–2.** **Goldens 0.** **Guard:** extend `trailingComments_test.go` with the three shapes
(its controls — a whole-line comment and a multi-line block comment are never pulled onto a code line — must keep
holding) and re-run `displacedCommentDrain_test.go`. **Re-measure I after H:** most of it is H's flush gaps.

### 4.8 J — end-of-line alignment "on the order of go fmt"

**Measured:** 1,161 runs of consecutive Go lines whose trailing comments gofmt aligned into one column (8 more Go runs
are deliberately unaligned by gofmt's section rules). In C#: **95 aligned**, 770 consecutive but ragged, 109 split by
inserted blank lines, 80 with comments moved to their own line, 7 with a comment missing. C#-side: of 850 runs of
consecutive trailing comments, 764 are ragged. Corpus files 338 (misaligned) and 63 (split).

**How gofmt does it:** go/printer ends each line's code with a cell terminator before a same-line comment
(`writeCommentPrefix`, `go/printer/printer.go:349-397`; struct fields and value specs use `vtab`, `nodes.go:554, 578,
1620-1633`). text/tabwriter aligns a column block of consecutive lines that all have that cell, and ends the block at
the first line without it, so a blank line, a comment-free line or a different indentation breaks the run
(`text/tabwriter/tabwriter.go:351-390`). A section break flushes the writer: `linebreak` (`nodes.go:46-61`) is called
with `newSection` when the previous statement spanned several lines (`nodes.go:1203`, fields `561, 617`), and
`exprList` breaks alignment when an element's size against the running geometric mean is ≥ 2.5 or ≤ 0.4 for keys over
40 characters (`nodes.go:184-262`). The column is the longest code in the block plus one space, counted in runes
(`printer.go:1383-1396`).

**go2cs today:** statement comments get one space, by design (comments.md: Go's padding was computed for Go's line
lengths); spec and field comments replay Go's padding, `writeCommentString:324-332` writing `comment.Slash -
targetPos` spaces where `targetPos` is Go's offset plus an estimated `typeLenDeviation` (`visitValueSpec.go:1373` and
others), which is ragged (`regexp/syntax/parse.cs:50-62`) or clamped to one space (W1).

**Fix:** a pure post-pass, `alignTrailingComments(text)`, run on each finished file at `writeOutputFile`
(`projectFileWriter.go:761`, before `writeSourceIfChanged`) and again in `rewriteDeferredMarkers`
(`deferredMarkerOperations.go:50`, written at about line 101, because that pass changes line widths). It lexes C#
(skipping string, verbatim, raw, `u8` and char literals and block comments), finds runs of consecutive lines with code
and a trailing `//` at one indentation, ends a run at a blank line, a comment-only line, a comment-free line, an
indentation change or a line ending in an open bracket, and pads each run to the longest code plus one. It never adds
or removes a line, so the position map stays valid. The writeCommentString padding becomes one space (the pass does
the rest). Options for §7 decision 4: also apply gofmt's 2.5 ratio break so one very long line does not push a run far
right; and whether to align the converter's own trailing comments or only Go's (the position map can tell them apart).
**Effort 2–3.** **Goldens 0** (they carry no trailing comments). Run it after every placement fix: each of those
changes widths.

### 4.9 K and L — top-level separation and runs of two or more blank lines

**K, measured:** 59 Go declarations with a blank line above have none in C#: a var with an anonymous struct type.
`internal/coverage/rtcov/rtcov.go:38`, `var Meta struct{…}` → `rtcov.cs:41-52`: the doc, then a blank line, then the
lifted `partial struct Metaᴛ1 /*dyn*/`, then `public static Metaᴛ1 Meta;` directly under its `}` — the doc is no longer
next to its declaration. And **1,067 blank lines inserted** between declarations Go writes adjacently, nearly all
one-line methods (`go/ast/ast.go:510`, `func (x *StarExpr) Pos() token.Pos { return x.Star }`) that C# writes as
three-line bodies (`go/ast/ast.cs:535-537`). **Root cause:** there is no separator emitter; `visitFile.go:155-157` calls
`visitDecl` and each emitter writes its own newlines (`visitFuncDecl.go:649` before, `:1543` after; `visitValueSpec.go:27`;
`visitGenDecl.go:33`). **Fix:** for the lifted var, emit the lifted struct first, then a blank, then the doc and the var.
For adjacency: only if C# also writes those methods as one-line expression-bodied members — three-line methods with no
blank between them read worse than today (§7 decision 5). **Effort 0.5** (lifted var); 1–2 more for one-liners.

**L, measured:** 72 runs of two or more blank lines (144 lines) in the 1,616 paired files: 69 at member boundaries,
3 near scaffolding (101 in all non-test converted files, 371 including tests). Nearly all sit before a lifted `/*dyn*/`
struct: `runtime/trace/trace.cs:162-164`. **Root cause:** `visitValueSpec.go:27` writes a newline, then
`visitStructType.go:266-270` writes another when it lifts a struct (`target == nil && !inFunction`; callers
`visitValueSpec.go:315, 568, 582, 612`). **Fix:** skip the second newline when the lift comes from `visitValueSpec`.
**Effort: in K's 0.5.** **Goldens: 21** (18 of their runs at `/*dyn*/`).

## 5. Guards

**The exemplar as the golden (`utf8.cs`).** A converter test, `commentParityUtf8_test.go`, converts
`GOROOT/src/unicode/utf8` with `-comments` into a temporary root and runs the census measures (§1, ported into
`src/go2cs` as a test helper) on `utf8.go` against the emitted `utf8.cs`, asserting:

- missed: 0 (imports aside); misplaced: 0;
- every Go run gofmt aligned is aligned in C#, and no ragged C# run exists;
- group, top-level and body blank lines equal to Go's, except the scaffolding §7 decision 2 keeps.

Today it fails with W1–W5's numbers; each fix turns its class green in this one file, which makes it the train's
acceptance test. Its red test is the planted pair (Appendix B), kept as testdata: the `good` rendering must pass and
`bad`/`bad2` must fail naming each planted site.

**The corpus ratchet.** `TestCommentParityCensus` runs the census over `src/core` against GOROOT (about 16 seconds)
under the plain `go test ./...` every lane runs, against a committed baseline of counts per class: a rise fails by
class and file; a fall must update the baseline in the same commit (the `declaredNotImplemented_test.go` declared-set
pattern). This keeps a later converter change from silently re-dropping comments or blank lines.

## 6. Order and timing

**Order, low-hanging first:**

1. **A** (0.5–1): one bug, the largest class, golden-neutral under `-comments`; introduces the shared statement-list
   helper.
2. **B + C + D, with K's lifted var and L** (2–3): small, independent emitters; golden-neutral except L's 21.
3. **E1** (0.5; plus E2 if ruled): moves up to 160 goldens, so it is its own seat.
4. **H + I** (1.5–2): uses A's helper for case bodies; re-measure I after.
5. **F + G** (2–3): composite literals and expressions; G per decision 3.
6. **J** (2–3): last, because every placement fix changes line widths; K's one-liners only if decision 5 says so.

**8–12 lane-days of fixes, 10–15 with the corpus footprint of each.**

**Timing against the Go 1.26 hop (`PLAN-hop-go126`, on `claude/i9-plan-hop-go126`).** Every fix re-emits corpus files:
about 1,000 files for the whole train, mostly whitespace and comment moves. Three options:

- **Before H0 (recommended).** Land the train as its own formatting train before the hop starts, ideally inside the final
  1.24.13.N release (§2 O1 of the hop plan) so the readable output ships now. Cost: the 10–15 lane-days, plus one
  corpus reconvert and review on the 1.24.13 corpus. Saves: the hop's H0 `.cs.auto` baseline is taken with the final
  formatting, so H5's diff, H5c's review, the §4 attribution checkpoint and the H6 classification of 79 hand-owned files
  carry only Go-version deltas.
- **Inside the hop: not recommended.** H4 requires the converter changes it carries to be byte-identical on the
  outgoing corpus (CNR), which these are not, so they would need a ruling to ride there; riding at H5 would mix about
  1,000 formatting diffs into H5c and the attribution checkpoint, which costs more review than the one reconvert it
  saves, and would need a whitespace-insensitive H5c diff plus a separate comment-move census to attribute.
- **After the hop (post-H12).** No hop risk; the improvement waits the hop's 3.5–5 weeks and is applied to the
  1.26 corpus, where the census re-takes in 16 seconds.

Golden-neutral fixes (A, B, C, D, F, H, J under `-comments`) leave H9's golden rebank untouched in every option.

## 7. What the owner's hands are needed for

Reads-like-Go is the owner's call; these change visible emission and go to him before any build:

1. **A's scope:** keep Go's body blank lines only under `-comments` (the corpus and the Tour; 0 goldens), or in every
   conversion (about 650 of 819 behavioral directories re-baselined, whitespace only)?
2. **E2:** the blank line after a receiver or parameter prelude and after named-result locals (3,938 in the corpus,
   in 136 goldens): drop it (faithful to Go), or keep exactly one as a separator between converter scaffolding and
   the Go body?
3. **G:** a comment on an expression operand or parameter: inline `/* c */`, Go's line breaks, or leave it as an
   own-line comment at the top of the body?
4. **J:** align every trailing comment or only Go's; apply gofmt's long-line break (ratio 2.5) or align whole runs?
5. **K:** adjacent one-line methods: collapse to one-line expression-bodied members and keep Go's adjacency, or keep
   three-line bodies with a blank line between (today)?
6. **C, aliases:** a type alias's doc above its `global using` line, or at the place Go declared it?

## Appendix A — the instrument (uncommitted; `ccensus`, three files of one `main` package, standard library only)

Run from a directory holding these files and a `go.mod` (`module ccensus`, `go 1.24`):

```
go run . -core <tree>/src/core -goroot "$(go env GOROOT)" -out <dir> [-only unicode/utf8/utf8]
```

It prints every count in this plan and writes `<dir>/examples.tsv` (up to 400 examples per class: class, Go and C#
location, the comment's first line or the code).

### `cslex.go`

```go
package main

import (
	"strings"
)

// csComment is one comment in a C# file. A block comment spanning lines yields one csComment per line.
type csComment struct {
	line     int    // 1-based
	col      int    // visual column of the comment start (tabs expanded to 4)
	text     string // normalized text (delimiters stripped, whitespace collapsed)
	trailing bool   // code precedes it on the same line
	block    bool
}

type csFile struct {
	lines    []string
	comments []csComment
	// codeOnLine[i] is true when line i+1 carries non-comment, non-blank text
	codeOnLine []bool
	// codeText[i] is the line's code with comments and string/char literal contents removed
	codeText []string
}

func visualCol(line string, byteIdx int, tab int) int {
	col := 0
	for i, r := range line {
		if i >= byteIdx {
			break
		}
		if r == '\t' {
			col += tab - col%tab
		} else {
			col++
		}
	}
	return col
}

func normText(s string) string {
	return strings.Join(strings.Fields(s), " ")
}

// lexCS finds comments in C# source, skipping string and char literals (regular, verbatim, interpolated,
// and raw """ strings) so that a // inside a literal is never read as a comment.
func lexCS(src string) *csFile {
	src = strings.ReplaceAll(src, "\r\n", "\n")
	f := &csFile{lines: strings.Split(src, "\n")}
	n := len(f.lines)
	f.codeOnLine = make([]bool, n)
	f.codeText = make([]string, n)
	inBlock := false
	blockStartLine := 0
	_ = blockStartLine
	inVerbatim := false // verbatim or raw string continuing across lines
	rawQuotes := 0
	for li, line := range f.lines {
		var code strings.Builder
		i := 0
		for i < len(line) {
			if inBlock {
				end := strings.Index(line[i:], "*/")
				seg := line[i:]
				if end >= 0 {
					seg = line[i : i+end]
				}
				t := normText(seg)
				if t != "" && t != "*" {
					f.comments = append(f.comments, csComment{line: li + 1, col: visualCol(line, i, 4), text: strings.TrimPrefix(t, "* "), trailing: false, block: true})
				}
				if end < 0 {
					i = len(line)
					break
				}
				i += end + 2
				inBlock = false
				continue
			}
			if inVerbatim {
				if rawQuotes > 0 {
					q := strings.Repeat("\"", rawQuotes)
					end := strings.Index(line[i:], q)
					if end < 0 {
						i = len(line)
						break
					}
					i += end + rawQuotes
					inVerbatim = false
					rawQuotes = 0
					code.WriteString("\"\"")
					continue
				}
				// verbatim: "" is an escaped quote
				for i < len(line) {
					if line[i] == '"' {
						if i+1 < len(line) && line[i+1] == '"' {
							i += 2
							continue
						}
						i++
						inVerbatim = false
						code.WriteString("\"")
						break
					}
					i++
				}
				continue
			}
			c := line[i]
			switch {
			case c == '/' && i+1 < len(line) && line[i+1] == '/':
				t := normText(line[i+2:])
				trailing := strings.TrimSpace(code.String()) != ""
				f.comments = append(f.comments, csComment{line: li + 1, col: visualCol(line, i, 4), text: strings.TrimPrefix(strings.TrimPrefix(t, "/"), " "), trailing: trailing})
				i = len(line)
			case c == '/' && i+1 < len(line) && line[i+1] == '*':
				end := strings.Index(line[i+2:], "*/")
				trailing := strings.TrimSpace(code.String()) != ""
				if end >= 0 {
					t := normText(line[i+2 : i+2+end])
					// value comments the converter writes inside code (/* '�' */) are recorded too;
					// they never match a Go comment's text, so they cannot hide a miss
					f.comments = append(f.comments, csComment{line: li + 1, col: visualCol(line, i, 4), text: t, trailing: trailing, block: true})
					code.WriteString(" ")
					i += 2 + end + 2
				} else {
					t := normText(line[i+2:])
					if t != "" {
						f.comments = append(f.comments, csComment{line: li + 1, col: visualCol(line, i, 4), text: t, trailing: trailing, block: true})
					}
					inBlock = true
					i = len(line)
				}
			case c == '"' && i+2 < len(line) && line[i+1] == '"' && line[i+2] == '"':
				// raw string literal
				q := 3
				for i+q < len(line) && line[i+q] == '"' {
					q++
				}
				end := strings.Index(line[i+q:], strings.Repeat("\"", q))
				code.WriteString("\"\"")
				if end < 0 {
					inVerbatim = true
					rawQuotes = q
					i = len(line)
				} else {
					i += q + end + q
				}
			case c == '@' && i+1 < len(line) && line[i+1] == '"', c == '$' && i+2 < len(line) && line[i+1] == '@' && line[i+2] == '"', c == '@' && i+2 < len(line) && line[i+1] == '$' && line[i+2] == '"':
				j := strings.IndexByte(line[i:], '"') + i + 1
				code.WriteString("\"")
				i = j
				inVerbatim = true
				for i < len(line) {
					if line[i] == '"' {
						if i+1 < len(line) && line[i+1] == '"' {
							i += 2
							continue
						}
						i++
						inVerbatim = false
						code.WriteString("\"")
						break
					}
					i++
				}
			case c == '"':
				i++
				for i < len(line) {
					if line[i] == '\\' {
						i += 2
						continue
					}
					if line[i] == '"' {
						i++
						break
					}
					i++
				}
				code.WriteString("\"\"")
			case c == '\'':
				// char literal: '\'' or 'x' or '\u0000'
				j := i + 1
				for j < len(line) {
					if line[j] == '\\' {
						j += 2
						continue
					}
					if line[j] == '\'' {
						j++
						break
					}
					j++
				}
				code.WriteString("' '")
				i = j
			default:
				code.WriteByte(c)
				i++
			}
		}
		f.codeText[li] = code.String()
		f.codeOnLine[li] = strings.TrimSpace(code.String()) != ""
	}
	return f
}
```

### `golex.go`

```go
package main

import (
	"go/ast"
	"go/parser"
	"go/scanner"
	"go/token"
	"os"
	"strings"
)

type goGroup struct {
	class    string   // see classify
	declKind string   // for decl-doc: func, method, const, var, type-struct, type-interface, type-alias, type-other
	startLn  int      // first line of the group
	endLn    int      // last line
	lines    []string // normalized text lines (empty lines dropped)
	lineNos  []int    // source line of each text line
	trailing bool     // shares its first line with code before it
	cols     []int    // visual column (tab 8) of each text line's comment start, for // comments
}

type goFile struct {
	path    string
	src     []string // lines
	fset    *token.FileSet
	file    *ast.File
	groups  []*goGroup
	tokLine map[int][]token.Token // non-comment tokens per line
	idents  map[int][]string      // identifiers per line
	firstTok map[int]token.Token  // first non-comment token on the line
	blank   map[int]bool
}

func parseGo(path string) (*goFile, error) {
	data, err := os.ReadFile(path)
	if err != nil {
		return nil, err
	}
	fset := token.NewFileSet()
	f, err := parser.ParseFile(fset, path, data, parser.ParseComments|parser.SkipObjectResolution)
	if err != nil {
		return nil, err
	}
	g := &goFile{path: path, fset: fset, file: f, tokLine: map[int][]token.Token{}, idents: map[int][]string{}, firstTok: map[int]token.Token{}, blank: map[int]bool{}}
	g.src = strings.Split(strings.ReplaceAll(string(data), "\r\n", "\n"), "\n")
	for i, l := range g.src {
		if strings.TrimSpace(l) == "" {
			g.blank[i+1] = true
		}
	}
	// token scan
	var s scanner.Scanner
	tf := token.NewFileSet().AddFile(path, -1, len(data))
	s.Init(tf, data, nil, 0)
	for {
		pos, tok, lit := s.Scan()
		if tok == token.EOF {
			break
		}
		if tok == token.SEMICOLON && lit == "\n" {
			continue
		}
		ln := tf.Line(pos)
		if _, ok := g.firstTok[ln]; !ok {
			g.firstTok[ln] = tok
		}
		g.tokLine[ln] = append(g.tokLine[ln], tok)
		if tok == token.IDENT {
			g.idents[ln] = append(g.idents[ln], lit)
		}
	}
	g.classify()
	return g, nil
}

func isDirective(text string) bool {
	return strings.HasPrefix(text, "//go:") || strings.HasPrefix(text, "//line ") || strings.HasPrefix(text, "//export ") || strings.HasPrefix(text, "//extern ") || strings.HasPrefix(text, "//sys") || strings.HasPrefix(text, "// +build")
}

func (g *goFile) classify() {
	f := g.file
	kind := map[*ast.CommentGroup]string{}
	dk := map[*ast.CommentGroup]string{}
	if f.Doc != nil {
		kind[f.Doc] = "package-doc"
	}
	ast.Inspect(f, func(n ast.Node) bool {
		switch x := n.(type) {
		case *ast.FuncDecl:
			if x.Doc != nil {
				kind[x.Doc] = "decl-doc"
				if x.Recv != nil {
					dk[x.Doc] = "method"
				} else {
					dk[x.Doc] = "func"
				}
			}
		case *ast.GenDecl:
			if x.Doc != nil {
				kind[x.Doc] = "decl-doc"
				dk[x.Doc] = genKind(x)
			}
			grouped := x.Lparen.IsValid()
			for _, sp := range x.Specs {
				var doc, com *ast.CommentGroup
				switch s := sp.(type) {
				case *ast.ValueSpec:
					doc, com = s.Doc, s.Comment
				case *ast.TypeSpec:
					doc, com = s.Doc, s.Comment
				case *ast.ImportSpec:
					doc, com = s.Doc, s.Comment
				}
				if doc != nil {
					if grouped {
						kind[doc] = "spec-doc-in-group"
					} else {
						kind[doc] = "decl-doc"
					}
					dk[doc] = specKind(x.Tok, sp)
				}
				if com != nil {
					kind[com] = "trailing-spec"
				}
			}
		case *ast.Field:
			if x.Doc != nil {
				kind[x.Doc] = "field-doc"
			}
			if x.Comment != nil {
				kind[x.Comment] = "trailing-field"
			}
		}
		return true
	})
	// enclosing-node lookup for free comments
	type span struct {
		pos, end token.Pos
		n        ast.Node
	}
	var spans []span
	ast.Inspect(f, func(n ast.Node) bool {
		if n == nil {
			return false
		}
		switch n.(type) {
		case *ast.CompositeLit, *ast.CallExpr, *ast.BinaryExpr, *ast.FuncType, *ast.IndexExpr, *ast.KeyValueExpr, *ast.ParenExpr, *ast.FuncLit, *ast.BlockStmt, *ast.StructType, *ast.InterfaceType:
			spans = append(spans, span{n.Pos(), n.End(), n})
		}
		return true
	})
	for _, cg := range f.Comments {
		gg := &goGroup{}
		startLn := g.fset.Position(cg.Pos()).Line
		gg.startLn = startLn
		gg.endLn = g.fset.Position(cg.End()).Line
		allDirective := true
		for _, c := range cg.List {
			ln := g.fset.Position(c.Pos()).Line
			col := g.fset.Position(c.Pos()).Column
			if !isDirective(c.Text) {
				allDirective = false
			}
			if strings.HasPrefix(c.Text, "//") {
				t := normText(c.Text[2:])
				t = strings.TrimPrefix(t, "/")
				t = strings.TrimSpace(t)
				if t != "" {
					gg.lines = append(gg.lines, t)
					gg.lineNos = append(gg.lineNos, ln)
					gg.cols = append(gg.cols, visualCol(g.src[ln-1], col-1, 8))
				}
			} else {
				body := strings.TrimSuffix(strings.TrimPrefix(c.Text, "/*"), "*/")
				for k, bl := range strings.Split(body, "\n") {
					t := normText(bl)
					t = strings.TrimPrefix(t, "* ")
					if t == "*" {
						t = ""
					}
					if t != "" {
						gg.lines = append(gg.lines, t)
						gg.lineNos = append(gg.lineNos, ln+k)
						gg.cols = append(gg.cols, -1)
					}
				}
			}
		}
		// trailing: a code token earlier on the group's first line
		toks := g.tokLine[startLn]
		if len(toks) > 0 {
			// tokens on that line that precede the comment: compare columns via source text
			line := g.src[startLn-1]
			cpos := g.fset.Position(cg.Pos()).Column - 1
			if strings.TrimSpace(line[:cpos]) != "" {
				gg.trailing = true
			}
		}
		if allDirective {
			gg.class = "directive"
		} else if k, ok := kind[cg]; ok {
			gg.class = k
			gg.declKind = dk[cg]
		} else if cg.End() < f.Package {
			gg.class = "file-header"
		} else {
			// innermost enclosing span
			var inner ast.Node
			var innerSize token.Pos = 1 << 60
			for _, s := range spans {
				if s.pos < cg.Pos() && cg.End() <= s.end && s.end-s.pos < innerSize {
					inner, innerSize = s.n, s.end-s.pos
				}
			}
			nextTok := token.ILLEGAL
			for ln := gg.endLn + 1; ln <= len(g.src); ln++ {
				if t, ok := g.firstTok[ln]; ok {
					nextTok = t
					break
				}
			}
			switch inner.(type) {
			case *ast.CompositeLit:
				gg.class = "in-composite-literal"
			case *ast.CallExpr, *ast.BinaryExpr, *ast.FuncType, *ast.IndexExpr, *ast.KeyValueExpr, *ast.ParenExpr:
				gg.class = "in-expression"
			case *ast.StructType, *ast.InterfaceType:
				gg.class = "free-in-type"
			default:
				switch {
				case gg.trailing:
					before := strings.TrimSpace(g.src[startLn-1][:g.fset.Position(cg.Pos()).Column-1])
					switch {
					case strings.HasSuffix(before, "{"):
						gg.class = "after-open-brace"
					case strings.HasSuffix(before, ":") && (g.firstTok[startLn] == token.CASE || g.firstTok[startLn] == token.DEFAULT):
						gg.class = "after-case-label"
					default:
						gg.class = "trailing-stmt"
					}
				case nextTok == token.ELSE:
					gg.class = "before-else"
				case nextTok == token.CASE || nextTok == token.DEFAULT:
					gg.class = "before-case"
				case nextTok == token.RBRACE:
					gg.class = "before-close-brace"
				case inner == nil:
					gg.class = "free-top-level"
				default:
					gg.class = "free-stmt"
				}
			}
		}
		g.groups = append(g.groups, gg)
	}
}

func genKind(x *ast.GenDecl) string {
	if len(x.Specs) == 0 {
		return x.Tok.String()
	}
	return specKind(x.Tok, x.Specs[0])
}

func specKind(tok token.Token, sp ast.Spec) string {
	if ts, ok := sp.(*ast.TypeSpec); ok {
		switch {
		case ts.Assign.IsValid():
			return "type-alias"
		}
		switch ts.Type.(type) {
		case *ast.StructType:
			return "type-struct"
		case *ast.InterfaceType:
			return "type-interface"
		}
		return "type-other"
	}
	return tok.String()
}
```

### `main.go`

```go
// ccensus: comment-placement, missed-comment, EOL-alignment and blank-line census of the converted corpus
// (src/core) against Go's own source at the pinned GOROOT. Read-only. One measure per class; see -h.
//
//	go run . -core <src/core> -goroot <GOROOT> -out <dir> [-only <pkg>/<file>.cs] [-v]
package main

import (
	"bufio"
	"flag"
	"fmt"
	"go/ast"
	"go/token"
	"os"
	"path/filepath"
	"regexp"
	"sort"
	"strings"
)

var goosDirs = map[string]bool{"windows": true, "linux": true, "darwin": true}

type pair struct {
	pkg, base, cs, gofile, flavor string
}

func findPairs(core, goroot string) []pair {
	type cand struct {
		p    pair
		rank int
	}
	best := map[string]cand{}
	filepath.Walk(core, func(path string, info os.FileInfo, err error) error {
		if err != nil {
			return nil
		}
		rel, _ := filepath.Rel(core, path)
		rel = filepath.ToSlash(rel)
		if info.IsDir() {
			b := info.Name()
			if b == "golib" || b == "obj" || b == "bin" || b == "Generated" || b == "go2cs" || rel == "unsafe" || rel == "testing" {
				return filepath.SkipDir
			}
			return nil
		}
		name := info.Name()
		if !strings.HasSuffix(name, ".cs") || strings.HasSuffix(name, "_test.cs") || strings.HasSuffix(name, "_impl.cs") ||
			name == "package_info.cs" || name == "package_test_info.cs" || name == "go2cs_test_host.cs" || strings.HasPrefix(name, "package_") {
			return nil
		}
		dir := filepath.Dir(rel)
		flavor := ""
		if goosDirs[filepath.Base(dir)] {
			parent := filepath.Dir(dir)
			// a flavour dir has no project of its own and sits in a package dir that has one
			if m, _ := filepath.Glob(filepath.Join(core, dir, "*.csproj")); len(m) == 0 {
				if m2, _ := filepath.Glob(filepath.Join(core, parent, "*.csproj")); len(m2) > 0 {
					flavor = filepath.Base(dir)
					dir = parent
				}
			}
		}
		data, err := os.ReadFile(path)
		if err != nil || strings.Contains(string(data), "GoManualConversion") {
			return nil
		}
		base := strings.TrimSuffix(name, ".cs")
		gofile := filepath.Join(goroot, "src", filepath.FromSlash(dir), base+".go")
		if _, err := os.Stat(gofile); err != nil {
			return nil
		}
		rank := map[string]int{"": 0, "windows": 1, "linux": 2, "darwin": 3}[flavor]
		key := dir + "/" + base
		if c, ok := best[key]; !ok || rank < c.rank {
			best[key] = cand{pair{dir, base, path, gofile, flavor}, rank}
		}
		return nil
	})
	var out []pair
	for _, c := range best {
		out = append(out, c.p)
	}
	sort.Slice(out, func(i, j int) bool { return out[i].pkg+"/"+out[i].base < out[j].pkg+"/"+out[j].base })
	return out
}

var keywords = map[string]bool{}

func init() {
	for _, k := range strings.Fields(`if else for return var func const type package import switch case default go defer select range struct interface map chan break continue goto fallthrough nil true false
public internal private protected static partial class readonly new ref out in this var using namespace foreach while do throw try catch finally null void bool byte int uint long ulong short ushort float double string object nint nuint rune
@string array slice ж Ꮡ sizeof typeof is as unsafe fixed get set value goto lock checked unchecked override virtual abstract sealed extern implicit explicit operator event delegate enum struct`) {
		keywords[k] = true
	}
}

var identRe = regexp.MustCompile(`[\pL_][\pL\pN_]*`)

var decorRe = regexp.MustCompile(`(ʗp|ᴛ\d*|ʗ\d*)$`)

func csIdents(code string) map[string]bool {
	m := map[string]bool{}
	for _, w := range identRe.FindAllString(code, -1) {
		if !keywords[w] {
			m[w] = true
			n := strings.TrimLeft(w, "ᏑΔ@")
			n = decorRe.ReplaceAllString(n, "")
			m[n] = true
		}
	}
	return m
}

func goIdents(ids []string) map[string]bool {
	m := map[string]bool{}
	for _, w := range ids {
		if !keywords[w] && w != "_" {
			m[w] = true
		}
	}
	return m
}

func intersects(a, b map[string]bool) bool {
	for k := range a {
		if b[k] {
			return true
		}
	}
	return false
}

type counter map[string]int

func (c counter) add(k string, n int) { c[k] += n }

type example struct{ class, where, detail string }

type results struct {
	files            int
	groupsByClass    counter
	missedByClass    counter
	partialByClass   counter
	missedLines      int
	presentLines     int
	misplacedByClass counter
	misplacedKind    counter
	checkedByClass   counter
	eol              counter
	csMisaligned     int
	csRuns           int
	goRunsMisaligned int
	blank2           counter
	topLevel         counter
	groupSpec        counter
	body             counter
	examples         map[string][]example
	missedByPkg      counter
	filesByClass     counter
	fileHit          map[string]bool
	groupsByPkg      counter
}

func newResults() *results {
	return &results{groupsByClass: counter{}, missedByClass: counter{}, partialByClass: counter{}, misplacedByClass: counter{},
		misplacedKind: counter{}, checkedByClass: counter{}, eol: counter{}, blank2: counter{}, topLevel: counter{}, groupSpec: counter{},
		body: counter{}, examples: map[string][]example{}, missedByPkg: counter{}, groupsByPkg: counter{}, filesByClass: counter{}}
}

func (r *results) ex(class, where, detail string) {
	if r.fileHit != nil {
		k := class
		if i := strings.Index(k[strings.Index(k, ":")+1:], ":"); i >= 0 && strings.HasPrefix(k, "missed:") {
			k = "missed:" + k[strings.Index(k, ":")+1:][:i]
		}
		if strings.HasPrefix(k, "missed:") || strings.HasPrefix(k, "partial:") {
			k = "missed-or-partial"
		} else if strings.HasPrefix(k, "misplaced:") {
			k = "misplaced:" + strings.Split(k, ":")[1]
		}
		r.fileHit[k] = true
	}
	if len(r.examples[class]) < 400 {
		r.examples[class] = append(r.examples[class], example{class, where, detail})
	}
}

func isBlankCS(cs *csFile, ln int) bool { // 1-based
	return ln >= 1 && ln <= len(cs.lines) && strings.TrimSpace(cs.lines[ln-1]) == ""
}

func commentOnly(cs *csFile, ln int) bool {
	return !cs.codeOnLine[ln-1] && strings.TrimSpace(cs.lines[ln-1]) != ""
}

var attrRe = regexp.MustCompile(`^\s*\[[A-Za-z]`)

func nextCodeLineCS(cs *csFile, ln int) int {
	for l := ln + 1; l <= len(cs.lines); l++ {
		if cs.codeOnLine[l-1] && !attrRe.MatchString(cs.codeText[l-1]) {
			return l
		}
	}
	return 0
}

func analyze(p pair, r *results, verbose bool) {
	g, err := parseGo(p.gofile)
	if err != nil {
		return
	}
	data, err := os.ReadFile(p.cs)
	if err != nil {
		return
	}
	cs := lexCS(string(data))
	r.files++
	r.fileHit = map[string]bool{}
	defer func() {
		for k := range r.fileHit {
			r.filesByClass.add(k, 1)
		}
	}()
	where := p.pkg + "/" + p.base

	// ---- 1+2: assign Go comment lines to C# comment occurrences, in order ----
	queue := map[string][]int{}
	for i, c := range cs.comments {
		queue[c.text] = append(queue[c.text], i)
	}
	used := map[int]bool{}
	take := func(t string) int {
		q := queue[t]
		for len(q) > 0 {
			i := q[0]
			q = q[1:]
			if !used[i] {
				used[i] = true
				queue[t] = q
				return i
			}
		}
		queue[t] = q
		return -1
	}
	firstCS := map[*goGroup]int{}
	lineCS := map[int]int{} // go line -> cs comment index (first comment on that line)
	for _, gg := range g.groups {
		if len(gg.lines) == 0 {
			continue
		}
		r.groupsByClass.add(gg.class, 1)
		if gg.declKind != "" {
			r.groupsByClass.add("  by decl kind: "+gg.class+" / "+gg.declKind, 1)
		}
		r.groupsByPkg.add(p.pkg, 1)
		found := 0
		first := -1
		for k, t := range gg.lines {
			i := take(t)
			if i >= 0 {
				found++
				if first < 0 {
					first = i
				}
				if _, ok := lineCS[gg.lineNos[k]]; !ok {
					lineCS[gg.lineNos[k]] = i
				}
			}
		}
		r.missedLines += len(gg.lines) - found
		r.presentLines += found
		switch {
		case found == 0:
			r.missedByClass.add(gg.class, 1)
			if gg.declKind != "" {
				r.missedByClass.add("  by decl kind: "+gg.class+" / "+gg.declKind, 1)
			}
			r.missedByPkg.add(p.pkg, 1)
			r.ex("missed:"+gg.class, fmt.Sprintf("%s.go:%d", where, gg.startLn), gg.lines[0])
		case found < len(gg.lines):
			r.partialByClass.add(gg.class, 1)
			r.ex("partial:"+gg.class, fmt.Sprintf("%s.go:%d", where, gg.startLn), fmt.Sprintf("%d of %d lines; first: %s", found, len(gg.lines), gg.lines[0]))
		}
		if found == len(gg.lines) {
			firstCS[gg] = first
		}
	}

	// ---- 1: misplacement of fully-present groups ----
	for _, gg := range g.groups {
		i, ok := firstCS[gg]
		if !ok || gg.class == "directive" || gg.class == "file-header" || gg.class == "package-doc" {
			continue
		}
		c := cs.comments[i]
		r.checkedByClass.add(gg.class, 1)
		var goAnchor map[string]bool
		goFirst := ""
		goNextTok := token.ILLEGAL
		firstOf := func(ids []string) string {
			for _, w := range ids {
				if !keywords[w] && w != "_" {
					return w
				}
			}
			return ""
		}
		if gg.trailing {
			goAnchor = goIdents(g.idents[gg.startLn])
			goFirst = firstOf(g.idents[gg.startLn])
		} else {
			for ln := gg.endLn + 1; ln <= len(g.src); ln++ {
				if t, ok := g.firstTok[ln]; ok {
					goAnchor = goIdents(g.idents[ln])
					goFirst = firstOf(g.idents[ln])
					goNextTok = t
					break
				}
			}
		}
		var csAnchor map[string]bool
		csNext := ""
		if c.trailing {
			csAnchor = csIdents(cs.codeText[c.line-1])
		} else {
			// skip the rest of this comment block, then the next code line
			nl := nextCodeLineCS(cs, c.line)
			if nl > 0 {
				csAnchor = csIdents(cs.codeText[nl-1])
				csNext = strings.TrimSpace(cs.codeText[nl-1])
			}
		}
		kind := ""
		switch {
		case gg.trailing && !c.trailing:
			kind = "trailing-to-own-line"
		case !gg.trailing && c.trailing:
			kind = "own-line-to-trailing"
		case len(goAnchor) > 0 && len(csAnchor) > 0 && !csAnchor[goFirst]:
			kind = "anchor-differs"
		case len(goAnchor) == 0 && goNextTok == token.RBRACE && !strings.HasPrefix(csNext, "}"):
			kind = "anchor-differs"
		}
		if kind != "" {
			r.misplacedByClass.add(gg.class, 1)
			r.misplacedKind.add(gg.class+" / "+kind, 1)
			r.ex("misplaced:"+gg.class+":"+kind, fmt.Sprintf("%s.go:%d -> .cs:%d", where, gg.startLn, c.line), gg.lines[0])
		}
	}

	// ---- 3: EOL alignment, Go-anchored ----
	type tl struct {
		line, col int
		indent    string
	}
	var goTrail []tl
	for _, gg := range g.groups {
		if gg.trailing && len(gg.cols) > 0 && gg.cols[0] >= 0 && gg.class != "directive" {
			src := g.src[gg.startLn-1]
			ind := src[:len(src)-len(strings.TrimLeft(src, "\t "))]
			goTrail = append(goTrail, tl{gg.startLn, gg.cols[0], ind})
		}
	}
	sort.Slice(goTrail, func(i, j int) bool { return goTrail[i].line < goTrail[j].line })
	for i := 0; i < len(goTrail); {
		j := i + 1
		for j < len(goTrail) && goTrail[j].line == goTrail[j-1].line+1 && goTrail[j].indent == goTrail[i].indent {
			j++
		}
		if j-i >= 2 {
			run := goTrail[i:j]
			aligned := true
			for _, t := range run {
				if t.col != run[0].col {
					aligned = false
				}
			}
			if !aligned {
				r.goRunsMisaligned++
				r.eol.add("go-run-not-aligned-by-gofmt", 1)
			} else {
				status := ""
				var csl []int
				for _, t := range run {
					ci, ok := lineCS[t.line]
					if !ok {
						status = "missing"
						break
					}
					if !cs.comments[ci].trailing {
						status = "moved-to-own-line"
						break
					}
					csl = append(csl, ci)
				}
				if status == "" {
					consecutive, same := true, true
					for k := 1; k < len(csl); k++ {
						if cs.comments[csl[k]].line != cs.comments[csl[k-1]].line+1 {
							consecutive = false
						}
						if cs.comments[csl[k]].col != cs.comments[csl[0]].col {
							same = false
						}
					}
					switch {
					case consecutive && same:
						status = "aligned"
					case consecutive:
						status = "misaligned"
					default:
						status = "split"
					}
				}
				r.eol.add("go-aligned-run:"+status, 1)
				r.eol.add("go-aligned-run-lines:"+status, len(run))
				if status != "aligned" {
					r.ex("eol:"+status, fmt.Sprintf("%s.go:%d-%d", where, run[0].line, run[len(run)-1].line), strings.TrimSpace(g.src[run[0].line-1]))
				}
			}
		}
		i = j
	}
	// C#-side runs of consecutive trailing comments
	var csTrail []csComment
	seenLine := map[int]bool{}
	for _, c := range cs.comments {
		if c.trailing && !c.block && !seenLine[c.line] {
			csTrail = append(csTrail, c)
			seenLine[c.line] = true
		}
	}
	for i := 0; i < len(csTrail); {
		j := i + 1
		for j < len(csTrail) && csTrail[j].line == csTrail[j-1].line+1 {
			j++
		}
		if j-i >= 2 {
			r.csRuns++
			for k := i + 1; k < j; k++ {
				if csTrail[k].col != csTrail[i].col {
					r.csMisaligned++
					break
				}
			}
		}
		i = j
	}

	// ---- 4a: runs of 2+ blank lines in C# ----
	for ln := 1; ln <= len(cs.lines); {
		if !isBlankCS(cs, ln) {
			ln++
			continue
		}
		e := ln
		for e+1 <= len(cs.lines) && isBlankCS(cs, e+1) {
			e++
		}
		if e-ln+1 >= 2 && e < len(cs.lines) {
			prev, next := "", ""
			if ln > 1 {
				prev = cs.lines[ln-2]
			}
			if e < len(cs.lines) {
				next = cs.lines[e]
			}
			ctx := blankContext(prev, next)
			r.blank2.add(ctx, 1)
			r.blank2.add("lines", e-ln+1)
			r.ex("blank2:"+ctx, fmt.Sprintf("%s.cs:%d", where, ln), strings.TrimSpace(prev)+" ⏎⏎ "+strings.TrimSpace(next))
		}
		ln = e + 1
	}

	// ---- 4b: blank-line separation, Go vs C# ----
	declLines := []int{}
	for i := range cs.lines {
		if isCSDecl(cs, i+1) {
			declLines = append(declLines, i+1)
		}
	}
	cursor := 0
	findDeclKind := func(name, kind string) int {
		q := `(^|[^\pL\pN_@ΔᏑ])Ꮡ?Δ?@?` + regexp.QuoteMeta(name)
		var re *regexp.Regexp
		switch kind {
		case "func":
			re = regexp.MustCompile(q + `\s*[(<]`)
		case "type":
			re = regexp.MustCompile(`(struct|interface|class|record|delegate)\s+Δ?@?` + regexp.QuoteMeta(name) + `([^\pL\pN_]|$)`)
		default:
			re = regexp.MustCompile(q + `\s*(=>|=|;|,)`)
		}
		for k := cursor; k < len(declLines); k++ {
			if re.MatchString(cs.codeText[declLines[k]-1]) {
				cursor = k + 1
				return declLines[k]
			}
		}
		return 0
	}
	findDecl := func(name string) int { return findDeclKind(name, "value") }
	csStart := func(ln int) int { // walk up over comments and attributes
		s := ln
		for s-1 >= 1 && (commentOnly(cs, s-1) || attrRe.MatchString(cs.lines[s-2])) {
			s--
		}
		return s
	}
	goBlankAbove := func(startLn int) bool { return g.blank[startLn-1] }
	decls := g.file.Decls
	for di, d := range decls {
		name, startLn := "", 0
		switch x := d.(type) {
		case *ast.FuncDecl:
			name = x.Name.Name
			startLn = g.fset.Position(x.Pos()).Line
			if x.Doc != nil {
				startLn = g.fset.Position(x.Doc.Pos()).Line
			}
		case *ast.GenDecl:
			if x.Tok == token.IMPORT {
				continue
			}
			startLn = g.fset.Position(x.Pos()).Line
			if x.Doc != nil {
				startLn = g.fset.Position(x.Doc.Pos()).Line
			}
			for _, sp := range x.Specs {
				switch s := sp.(type) {
				case *ast.ValueSpec:
					for _, n := range s.Names {
						if n.Name != "_" {
							name = n.Name
							break
						}
					}
				case *ast.TypeSpec:
					name = s.Name.Name
				}
				if name != "" {
					break
				}
			}
		}
		if name == "" || name == "init" || name == "_" {
			continue
		}
		dkind := "value"
		switch x := d.(type) {
		case *ast.FuncDecl:
			dkind = "func"
		case *ast.GenDecl:
			if x.Tok == token.TYPE {
				dkind = "type"
			}
		}
		save := cursor
		cl := findDeclKind(name, dkind)
		if cl == 0 {
			cursor = save
		}
		if cl == 0 {
			k := "func"
			if gd, ok := d.(*ast.GenDecl); ok {
				k = genKind(gd)
			} else if d.(*ast.FuncDecl).Recv != nil {
				k = "method"
			}
			r.topLevel.add("unmapped:"+k, 1)
			r.ex("top:unmapped:"+k, fmt.Sprintf("%s.go:%d", where, startLn), name)
			continue
		}
		if di > 0 {
			gb := goBlankAbove(startLn)
			cb := isBlankCS(cs, csStart(cl)-1)
			switch {
			case gb && !cb:
				r.topLevel.add("go-blank-cs-none", 1)
				r.ex("top:go-blank-cs-none", fmt.Sprintf("%s.go:%d -> .cs:%d", where, startLn, cl), strings.TrimSpace(cs.lines[cl-1]))
			case !gb && cb:
				r.topLevel.add("go-none-cs-blank", 1)
				r.ex("top:go-none-cs-blank", fmt.Sprintf("%s.go:%d -> .cs:%d", where, startLn, cl), strings.TrimSpace(cs.lines[cl-1]))
			default:
				r.topLevel.add("same", 1)
			}
		}
		// grouped const/var specs
		if gd, ok := d.(*ast.GenDecl); ok && gd.Lparen.IsValid() && (gd.Tok == token.CONST || gd.Tok == token.VAR) {
			for si, sp := range gd.Specs {
				vs := sp.(*ast.ValueSpec)
				if si == 0 {
					continue
				}
				nm := vs.Names[0].Name
				if nm == "_" {
					continue
				}
				sl := g.fset.Position(vs.Pos()).Line
				if vs.Doc != nil {
					sl = g.fset.Position(vs.Doc.Pos()).Line
				}
				scl := findDecl(nm)
				if scl == 0 {
					r.groupSpec.add("unmapped", 1)
					continue
				}
				gb := g.blank[sl-1]
				cb := isBlankCS(cs, csStart(scl)-1)
				switch {
				case gb && !cb:
					r.groupSpec.add("go-blank-cs-none", 1)
					r.ex("group:go-blank-cs-none", fmt.Sprintf("%s.go:%d -> .cs:%d", where, sl, scl), strings.TrimSpace(cs.lines[scl-1]))
				case !gb && cb:
					r.groupSpec.add("go-none-cs-blank", 1)
					r.ex("group:go-none-cs-blank", fmt.Sprintf("%s.go:%d -> .cs:%d", where, sl, scl), strings.TrimSpace(cs.lines[scl-1]))
				default:
					r.groupSpec.add("same", 1)
				}
			}
		}
		// function bodies: each Go blank line, matched to its element in C#; each C# blank, classified
		if fd, ok := d.(*ast.FuncDecl); ok && fd.Body != nil {
			lb := g.fset.Position(fd.Body.Lbrace).Line
			rb := g.fset.Position(fd.Body.Rbrace).Line
			cOpen, cEnd, okb := csBodyRange(cs, cl)
			if !okb {
				r.body.add("cs-body-not-found", 1)
				continue
			}
			r.body.add("functions", 1)
			matchedStarts := map[int]bool{}
			cur := cOpen + 1
			for L := lb + 1; L < rb; L++ {
				if !g.blank[L-1] || g.blank[L] {
					continue
				}
				// L is the first non-blank line after a Go blank line inside the body
				csLine := 0
				if ci, ok := lineCS[L]; ok && cs.comments[ci].line > cOpen && cs.comments[ci].line < cEnd {
					csLine = cs.comments[ci].line
				} else if ids := goIdents(g.idents[L]); len(ids) > 0 {
					for c := cur; c < cEnd; c++ {
						if !cs.codeOnLine[c-1] {
							continue
						}
						ci := csIdents(cs.codeText[c-1])
						all := true
						for k := range ids {
							if !ci[k] {
								all = false
								break
							}
						}
						if all {
							csLine = c
							break
						}
					}
				}
				r.body.add("go-blank-lines", 1)
				if csLine == 0 {
					r.body.add("go-blank:unmapped", 1)
					continue
				}
				cur = csLine + 1
				st := csLine
				for st-1 > cOpen && (commentOnly(cs, st-1) || attrRe.MatchString(cs.lines[st-2])) {
					st--
				}
				matchedStarts[st] = true
				if isBlankCS(cs, st-1) {
					r.body.add("go-blank:cs-blank", 1)
				} else {
					r.body.add("go-blank:cs-NONE", 1)
					r.ex("body:go-blank-cs-none", fmt.Sprintf("%s.go:%d -> .cs:%d", where, L, st), strings.TrimSpace(g.src[L-1]))
				}
			}
			for c := cOpen + 1; c < cEnd; c++ {
				if !isBlankCS(cs, c) || isBlankCS(cs, c-1) {
					continue
				}
				nx := c + 1
				for nx < cEnd && isBlankCS(cs, nx) {
					nx++
				}
				if matchedStarts[nx] {
					continue
				}
				prev := strings.TrimSpace(cs.codeText[c-2])
				kind := "extra:unexplained"
				switch {
				case strings.HasSuffix(prev, "= default!;") || strings.HasSuffix(prev, "= default;"):
					kind = "extra:after-named-result-locals"
				case strings.HasSuffix(prev, "}}"):
					kind = "extra:after-lowered-switch"
				case preludeRe.MatchString(prev):
					kind = "extra:after-param-prelude"
				case c-1 == cOpen:
					kind = "extra:after-open-brace"
				case nx == cEnd || strings.HasPrefix(strings.TrimSpace(cs.codeText[nx-1]), "}"):
					kind = "extra:before-close-brace"
				}
				r.body.add(kind, 1)
				if kind == "extra:unexplained" {
					r.ex("body:"+kind, fmt.Sprintf("%s.cs:%d", where, c), prev+" ⏎ "+strings.TrimSpace(cs.lines[nx-1]))
				}
			}
		}
	}
	if verbose {
		fmt.Fprintf(os.Stderr, "%s: %d groups\n", where, len(g.groups))
	}
}

var preludeRe = regexp.MustCompile(`(DerefOrNull\(\);|heap<.*>\(out var \S+\);|= \S+\.Clone\(\);|\.sslice\(\);|= GoZero<.*>\(\);|= nonnil\(ref .*;|= default!;)$`)

var csDeclRe = regexp.MustCompile(`^(\[[^\]]*\]\s*)*(public|internal|private|protected|partial|static|readonly|unsafe|const|ref|file|new)\b`)

func isCSDecl(cs *csFile, ln int) bool {
	return csDeclRe.MatchString(cs.codeText[ln-1]) && !strings.HasPrefix(cs.codeText[ln-1], "partial class ") && !strings.Contains(cs.codeText[ln-1], "_package")
}

// csBodyRange returns the lines of the brace body that opens on or after line ln: its opening and closing lines.
func csBodyRange(cs *csFile, ln int) (int, int, bool) {
	depth := 0
	open := 0
	for l := ln; l <= len(cs.lines) && l < ln+5000; l++ {
		code := cs.codeText[l-1]
		if open == 0 && strings.Contains(code, "=>") && !strings.Contains(code, "{") {
			return 0, 0, false
		}
		for _, ch := range code {
			switch ch {
			case '{':
				if open == 0 {
					open = l
				}
				depth++
			case '}':
				depth--
				if open > 0 && depth == 0 {
					return open, l, true
				}
			}
		}
	}
	return 0, 0, false
}

func csBodyBlanks(cs *csFile, ln int) (int, bool) {
	depth := 0
	started := false
	blanks := 0
	for l := ln; l <= len(cs.lines) && l < ln+5000; l++ {
		code := cs.codeText[l-1]
		if !started {
			if strings.Contains(code, "=>") && !strings.Contains(code, "{") {
				return 0, false
			}
		}
		if started && depth > 0 && strings.TrimSpace(cs.lines[l-1]) == "" {
			blanks++
		}
		for _, ch := range code {
			switch ch {
			case '{':
				depth++
				started = true
			case '}':
				depth--
				if started && depth == 0 {
					return blanks, true
				}
			}
		}
	}
	return 0, false
}

func blankContext(prev, next string) string {
	p, n := strings.TrimSpace(prev), strings.TrimSpace(next)
	scaffold := regexp.MustCompile(`^(namespace |using |partial class |public static partial class |} // end |#region|#endregion|\[assembly|\[module|// <auto-generated|// Hoisted|// Static|// Package|\[GoPackage)`)
	switch {
	case scaffold.MatchString(p) || scaffold.MatchString(n) || p == "}" && strings.HasPrefix(n, "} //"):
		return "scaffold"
	case prev != "" && (prev[0] == ' ' || prev[0] == '\t') && next != "" && (next[0] == ' ' || next[0] == '\t'):
		return "inside-body"
	default:
		return "member-boundary"
	}
}

func dump(out string, r *results) {
	os.MkdirAll(out, 0o755)
	f, _ := os.Create(filepath.Join(out, "examples.tsv"))
	w := bufio.NewWriter(f)
	var keys []string
	for k := range r.examples {
		keys = append(keys, k)
	}
	sort.Strings(keys)
	for _, k := range keys {
		for _, e := range r.examples[k] {
			fmt.Fprintf(w, "%s\t%s\t%s\n", e.class, e.where, e.detail)
		}
	}
	w.Flush()
	f.Close()
}

func printCounter(title string, c counter) {
	fmt.Printf("%s\n", title)
	var keys []string
	for k := range c {
		keys = append(keys, k)
	}
	sort.Strings(keys)
	for _, k := range keys {
		fmt.Printf("  %-55s %8d\n", k, c[k])
	}
}

func main() {
	core := flag.String("core", "", "src/core")
	goroot := flag.String("goroot", "", "GOROOT")
	out := flag.String("out", "out", "output dir")
	only := flag.String("only", "", "pkg/base to analyze alone")
	verbose := flag.Bool("v", false, "")
	flag.Parse()
	pairs := findPairs(*core, *goroot)
	r := newResults()
	for _, p := range pairs {
		if *only != "" && p.pkg+"/"+p.base != *only {
			continue
		}
		analyze(p, r, *verbose)
	}
	fmt.Printf("pairs %d, analyzed %d\n", len(pairs), r.files)
	fmt.Printf("comment lines: present %d, missed %d\n", r.presentLines, r.missedLines)
	printCounter("groups by class", r.groupsByClass)
	printCounter("MISSED groups (no line found) by class", r.missedByClass)
	printCounter("PARTIAL groups by class", r.partialByClass)
	printCounter("checked for placement by class", r.checkedByClass)
	printCounter("MISPLACED by class", r.misplacedByClass)
	printCounter("MISPLACED by class / kind", r.misplacedKind)
	printCounter("EOL (Go-anchored runs)", r.eol)
	fmt.Printf("EOL C#-side runs %d, misaligned %d\n", r.csRuns, r.csMisaligned)
	printCounter("BLANK runs of 2+ in C# by context", r.blank2)
	printCounter("TOP-LEVEL separation", r.topLevel)
	printCounter("GROUPED const/var spec separation", r.groupSpec)
	printCounter("FUNCTION-BODY blank lines", r.body)
	printCounter("FILES with at least one finding, per class", r.filesByClass)
	type kv struct {
		k string
		v int
	}
	var mp []kv
	for k, v := range r.missedByPkg {
		mp = append(mp, kv{k, v})
	}
	sort.Slice(mp, func(i, j int) bool { return mp[i].v > mp[j].v })
	fmt.Println("MISSED groups by package (top 25): missed / groups")
	for i, e := range mp {
		if i >= 25 {
			break
		}
		fmt.Printf("  %-40s %6d / %6d\n", e.k, e.v, r.groupsByPkg[e.k])
	}
	dump(*out, r)
}
```

## Appendix B — the planted red-test input

The Go file is `goroot/src/plant/plant.go`; each rendering is `<root>/plant/plant.cs`, read with
`go run . -core <root> -goroot goroot`. §1's table says what each one plants and what the instrument reads.

### `plant.go`

```go
// Copyright 2026 header.
// Use of this source code.

package plant

// Numbers.
const (
	A = 1 // first
	B = 2 // second

	// Doc for C.
	C = 3
)

// F does things.
func F(x int) int {
	y := x + 1

	// second part
	z := y * 2
	if z > 3 { // big
		return z
	}
	t := []int{
		1, // one
		2, // two
	}
	return y + len(t)
}

// G is next.
func G() {}

// H is last.
func H() {}
```

### `good/plant/plant.cs`

```csharp
// Copyright 2026 header.
// Use of this source code.
namespace go;

partial class plant_package {

// Numbers.
public static UntypedInt A => 1; // first
public static UntypedInt B => 2; // second

// Doc for C.
public static UntypedInt C => 3;

// F does things.
public static nint F(nint x) {
    var y = x + 1;

    // second part
    var z = y * 2;
    if (z > 3) { // big
        return z;
    }
    var t = new nint[]{
        1, // one
        2, // two
    }.slice();
    return y + len(t);
}

// G is next.
public static void G() {
}

// H is last.
public static void H() {
}

} // end plant_package
```

### `bad/plant/plant.cs`

```csharp
// Copyright 2026 header.
// Use of this source code.
namespace go;

partial class plant_package {

// Numbers.
public static UntypedInt A => 1; // first
public static UntypedInt B => 0x2; // second
public static UntypedInt C => 3;

// one
// F does things.
public static nint F(nint x) {
    var y = x + 1;
    // second part
    var z = y * 2;
    if (z > 3) {
        // big
        return z;
    }
    var t = new nint[]{
        1,
        2, // two
    }.slice();
    return y + len(t);
}
// G is next.
public static void G() {
}


// H is last.
public static void H() {
}

} // end plant_package
```

### `bad2/plant/plant.cs`

```csharp
// Copyright 2026 header.
// Use of this source code.
namespace go;

partial class plant_package {

// Numbers.
public static UntypedInt A => 1; // first

public static UntypedInt B => 2; // second

// Doc for C.
public static UntypedInt C => 3;

// F does things.
public static nint F(nint x) {
    nint r = default!;

    // second part
    var y = x + 1;

    var z = y * 2;
    if (z > 3) { // big
        return z;
    }
    var t = new nint[]{
        1, // one
        2, // two
    }.slice();
    return y + len(t);

} // G is next.

public static void G() {
}

// H is last.
public static void H() {
}

} // end plant_package
```

## Appendix C — how to re-take this reading

```
git worktree add <dir> 20279db5f6
# save Appendix A in <tools> with a go.mod; the pinned toolchain's GOROOT is the Go side
cd <tools> && go run . -core <dir>/src/core -goroot "$(go env GOROOT)" -out out        # §0, §2, §4
go run . -core <dir>/src/core -goroot "$(go env GOROOT)" -out out-utf8 -only unicode/utf8/utf8   # §3
for v in good bad bad2; do go run . -core <plant>/$v -goroot <plant>/goroot -out out-$v; done   # §1 red tests
```

The root causes were traced read-only at the same master; every `file:line` above is at `20279db5f6`.
