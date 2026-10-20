# CENSUS — comment parity seats A and B+C: are the moved comments placed correctly?

> **State: MEASURED, 2026-10-12.** Seat A is `claude/c1-comment-parity-a` at `63f770b2fe` (seated). Seats B+C and
> seat A's follow-ups are on `claude/c1-comment-parity-bc`; every number below for "head" is read at `5ae3b2b10d`.
> Method, per `docs/PLAN-comment-and-spacing-parity.md` (on `claude/c1-scout-comment-parity`, `59a6c93b9e`, not yet on
> master) and the corpus-reconvert skill: one seeded `-stdlib -comments` emission per commit on windows, linux and darwin amd64, every arm seeded from
> the same tree (`14adc76d79`), arms diffed against each other. The census is the `internal/commentcensus` package
> (seat A0) run over each target's own files (flat files plus that target's GOOS flavour). Go source is read at the
> pinned toolchain, go1.24.13. Taken by C1 on COORD's question about seat A.
>
> **The question.** Seat A moved about 1,350 comment lines. Are they placed correctly, i.e. does the
> `misplaced:free-stmt` count fall rather than shift?
>
> **Answer: it falls.** Misplaced groups are keyed by their Go location, so a group counts as fixed only if it is
> misplaced in the before arm and not in the after arm, and as newly misplaced only in the reverse case:
>
> | arm (W / L / D) | misplaced groups | fixed | newly misplaced | free-stmt fixed / new |
> |---|---|---|---|---|
> | base → A `63f770b2fe` | 4,519→4,286 / 4,391→4,161 / 4,309→4,076 | 285 / 287 / 286 | 52 / 57 / 53 | 276/44 · 278/49 · 277/44 |
> | base → head `5ae3b2b10d` | 4,519→3,740 / 4,391→3,625 / 4,309→3,563 | 836 / 821 / 800 | 57 / 55 / 54 | 723/5 · 713/5 · 695/5 |
>
> The 44–49 groups seat A newly misplaced had a single mechanism: a comment opening a case body was written after the
> body's first statement. `0f8032e631` fixes it, and it also fixes about 480 groups per target that were already
> misplaced at base. Of the 57 newly misplaced at head (windows): 27 are `decl-doc` by design (seat C writes a
> methodless func type's doc above its marker comment), 15 are `after-case-label` (class H's; see §3), 7 `before-case`,
> 5 `free-stmt`, 2 `spec-doc-in-group`, 1 `trailing-stmt`.

## 1. The commits after seat A, in order

Every converter commit follows a red-first test commit. Three of the fixes were found by the footprint of the commit
before them. The arms are not a chain of independent seats: the bottom four commits correct the case-body comment
placement that `0f8032e631` introduced.

| commit | what | found by | .cs files moved (W/L/D) vs previous arm |
|---|---|---|---|
| `51659e166f` / `50189a44a9` | seats B and C: grouped spec docs, named non-struct type docs | plan | 416 / 424 / 418 vs A |
| `0f8032e631` | a comment opening a case body stays above its first statement | A's misplaced table | 224 / 227 / 222 vs B+C |
| `e20035e0db` / `80acd8fe27` | B+C regression: `//go:embed` / `//go:linkname` reads took the written doc | reading the code | 0 / 0 / 0 |
| `50be18e610` / `2ee369944f` | a leading `default:` the lowering moves to the end keeps its comments | footprint of `0f8032e631` | 53 / 52 / 52 (superseded) |
| `da5d306dd9` / `9dccf25a96` | an empty clause's comment, and one above a case label, stay out of argument lists | footprint of `2ee369944f` | 133 / 130 / 131 (superseded) |
| `e8be5b407c` / `5ae3b2b10d` | a moved default's comments are held until it is emitted | footprint of `9dccf25a96` | 165 / 162 / 164 vs `0f8032e631` |

In every arm the code tokens are identical: every changed `.cs` file is identical with comments and whitespace
removed, except one lambda in `archive/tar/reader.cs`, where a block body became an expression body. `package_info.cs`
changes are position-map lines only, and no `.csproj` or README moved.

**The directive regression.** `50189a44a9` passed each grouped spec its own doc in place of the GenDecl's doc. The
`//go:embed` and `//go:linkname` reads take that argument together with `valueSpec.Doc`, so a grouped variable's
directive was read twice, and a `[]byte` embed was refused ("2 patterns, 1 files"). It does not reach the stdlib:
the `80acd8fe27` arm moved 0 files, which matches the prediction. It does not reach CNR either, because no behavioral
test has a grouped directive. `TestAGroupedSpecReadsItsEmbedDirectiveOnce` is its guard; it is green at `63f770b2fe`,
red at `e20035e0db` and green at `80acd8fe27`.

**Comments written inside an argument list** (a code line where `//` follows `(` or `,`; W / L / D):

| base | A | B+C | `0f8032e631` | head |
|---|---|---|---|---|
| 1,824 / 1,810 / 1,803 | 1,691 / 1,677 / 1,670 | same as A | 1,513 / 1,498 / 1,496 | 1,445 / 1,433 / 1,430 |

The remaining 1,445 come from flushes this work does not touch (statement-internal positions; classes H and others).

## 2. Census, base → head (windows; linux and darwin agree to within a few percent)

| class | base | head |
|---|---|---|
| `body:go-blank-cs-none` (a Go blank line between statements, none in C#) | 11,740 | 1,909 |
| `misplaced:free-stmt` (anchor-differs + own-line-to-trailing) | 698 + 275 | 248 + 7 |
| `misplaced:before-case` (anchor-differs + own-line-to-trailing) | 85 + 61 | 61 + 0 |
| `missed:spec-doc-in-group` | 923 | 1 |
| `missed:decl-doc` | 422 | 15 |
| `group:go-none-cs-blank` (a blank line in C# between group members, none in Go) | 1,701 | 0 |
| `misplaced:after-case-label/trailing-to-own-line` | 175 | 205 |
| `misplaced:decl-doc/anchor-differs` | 44 | 70 |
| `group:go-blank-cs-none` (class D's) | 555 | 689 |

Of the remaining 15 `missed:decl-doc`, most are on aliases and on defined-over-interface types (PLAN decision 6). The
remaining `missed:spec-doc-in-group` is `crypto/x509/x509.go:47`. `body:extra:unexplained` rises from 896 to 2,913. This
is census mapping, not emission: in a 12-sample check, 10 matched Go, 1 was already present at base and 2 could not be
located.

## 3. What a reader should know

- **`after-case-label` rises by 25 against `0f8032e631`.** 14 of those groups were already misplaced there, in the
  wrong clause; 11 were inside an argument list (`append(result, // left key < right key`), which the census reads as a
  correctly trailing comment. At head they sit on their own line above the clause's first statement. Placing them on
  the `if (…) {` line is class H's job.
- **Correction to `5ae3b2b10d`'s message.** It says `0f8032e631` wrote a comment trailing a case label above the first
  statement. That is only true when an own-line comment followed it; alone, that comment stayed pending and went into
  an argument list. The code is unchanged by this correction.
- **Pre-existing, not touched here:** a lowered switch that ends a loop body leaves a blank line before the loop's `}`
  (read at base and at A with the same sample).

## 4. `unicode/utf8/utf8.cs`, before and after (windows; the other targets are identical)

Seat A (base → `63f770b2fe`): the one body blank line `utf8.go` has that the base emission lost.

```diff
@@ -483,6 +483,7 @@
     // when generating code for p[8:], bringing it to parity with
     // ValidString, which was 20% faster on long ASCII strings.
     p = p.slice(0, len(p), len(p));
+
     // Fast path. Check for and skip 8 bytes of ASCII characters per iteration.
     while (len(p) >= 8) {
```

Seats B+C (`63f770b2fe` → `50189a44a9`): the blank lines between grouped constants go, and the two spec docs inside
the `const ( … )` group come back. The blank line Go has above `// The default lowest…` is class D's.

```diff
 // Numbers fundamental to the encoding.
 public static UntypedInt RuneError => /* '�' */ 65533; // the "error" Rune or "Unicode replacement character"
-
 public static UntypedInt RuneSelf => 0x80; // characters below RuneSelf are represented as themselves in a single byte.
-
 public static UntypedInt MaxRune => /* '\U0010FFFF' */ 1114111; // Maximum valid Unicode code point.
-
 public static UntypedInt UTFMax => 4; // maximum number of bytes of a UTF-8 encoded Unicode character.
 
 // Code points in the surrogate range are not valid for UTF-8.
 internal static UntypedInt surrogateMin => 0xD800;
-
 internal static UntypedInt surrogateMax => 0xDFFF;
@@ -40,8 +36,13 @@
 internal static UntypedInt rune3Max => /* 1<<16 - 1 */ 65535;
+// The default lowest and highest continuation byte.
 internal static UntypedInt locb => 0b10000000;
 internal static UntypedInt hicb => 0b10111111;
+// These names of these constants are chosen to give nice alignment in the
+// table below. The first nibble is an index into acceptRanges or F for
+// special one-byte cases. The second nibble is the Rune length or the
+// Status for the special one-byte case.
 internal static UntypedInt xx => 0xF1; // invalid: size 1
```

The commits after B+C do not change `utf8.cs`, because `utf8.go` has no switch with a comment in a case body. For
that effect, here is `regexp/syntax/simplify.cs`, base → head (windows): every comment returns to the statement it
documents in `simplify.go`, including the three at column 0 and the two inside `new Regexp(…)`.

```diff
     if (exprᴛ1 == OpCapture || exprᴛ1 == OpConcat || exprᴛ1 == OpAlternate) {
+        // Simplify children, building new Regexp if children change.
         var nre = Ꮡre;
         foreach (var (i, sub) in re.Sub) {
-            // Simplify children, building new Regexp if children change.
             var nsub = sub.Simplify();
@@
     if (exprᴛ1 == OpRepeat) {
+        // Special special case: x{0} matches the empty string
+        // and doesn't even need to consider x.
         if (re.Min == 0 && re.Max == 0) {
-            // Special special case: x{0} matches the empty string
-            // and doesn't even need to consider x.
             return Ꮡ(new Regexp(Op: OpEmptyMatch));
         }
+
+        // The fun begins.
         var sub = re.Sub[0].Simplify();
+
+        // x{n,} means at least n matches of x.
         if (re.Max == -1) {
-            // The fun begins.
-            // x{n,} means at least n matches of x.
             // Special case: x{0,} is x*.
@@
+
+        // Special case x{0} handled above.
+
+        // Special case: x{1} is just x.
         if (re.Min == 1 && re.Max == 1) {
-            // Special case x{0} handled above.
-            // Special case: x{1} is just x.
             return sub;
         }
-// General case: x{n,m} means n copies of x and m copies of x?
-// The machine will do less work if we nest the final m copies,
-// so that x{2,5} = xx(x(x(x)?)?)?
+
+        // General case: x{n,m} means n copies of x and m copies of x?
+        // The machine will do less work if we nest the final m copies,
+        // so that x{2,5} = xx(x(x(x)?)?)?
@@
-        return Ꮡ(new Regexp( // Some degenerate case like min > max or min < max < 0.
- // Handle as impossible match.
-Op: OpNoMatch));
+
+        // Some degenerate case like min > max or min < max < 0.
+        // Handle as impossible match.
+        return Ꮡ(new Regexp(Op: OpNoMatch));
```

## 5. Gates

- `go test ./...` in `src/go2cs`: green at `0f8032e631`, `2ee369944f`, `9dccf25a96` and `5ae3b2b10d`.
- CNR emulation (878 behavioral projects transpiled in place without `-comments`, base binary `63f770b2fe`): ARMS
  AGREE, the same 15-file pre-existing drift, at `80acd8fe27` and at `5ae3b2b10d`. 0 behavioral goldens moved: every
  change after B+C's doc writing is under `-comments` only.
- Footprint marker gate, `.csproj`, README: 0 in every arm.

## 6. Predictions, scored (each was stamped before its arm ran)

| arm | prediction | result |
|---|---|---|
| `0f8032e631` | 25–60 .cs; equal comment +/−; free-stmt −35..−60 | **MISSED**: 224 .cs; comments left code lines (+186 comment lines); free-stmt −477. Code tokens MET. |
| `80acd8fe27` | 0 files | MET |
| CNR `80acd8fe27` | arms agree, 0 goldens | MET |
| CNR `5ae3b2b10d` | arms agree, 0 goldens | MET |
| `2ee369944f` | 5–40 .cs; no comment lost | **MISSED**: 53 .cs; 15 comments per target went into argument lists (the falsifier fired), fixed by `9dccf25a96` |
| `9dccf25a96` | 10–60 .cs; no comment enters a code line | **MISSED**: 133 .cs; the falsifier fired on 4 sites per target, fixed by `5ae3b2b10d` |
| `5ae3b2b10d` | 15–80 .cs; no code line gains `//`; misplaced falls; no class +5 | .cs **MISSED** (165). The `//` falsifier fired on 1 file, `bigmod/nat.cs`: Go's own trailing comment (nat.go:799) back on its statement. Misplaced falls: MET. The class falsifier fired: `after-case-label` +25 (§3). Code tokens MET. |
