# CENSUS — comment parity seat H: comments after `{`, on a case label and before `}`

> **State: MEASURED, 2026-10-12.** Branch `claude/c1-comment-parity-hi`, built on seat D+E1+L (`7b085c3f16`). Every
> "head" number is read at `dc9d12e6c3`. Method as in
> [CENSUS-comment-parity-seats-DEL.md](CENSUS-comment-parity-seats-DEL.md): one seeded `-stdlib -comments` emission
> per converter commit on windows, linux and darwin amd64, seeded from `14adc76d79`, each diffed against the arm
> before it; the `internal/commentcensus` census over each target's own files. The base arm is D+E1+L's head
> (`ae8b6289ef`). Taken by C1 on COORD's direction (ledger 2026-10-12 05:34, "C1 continues to F, G, H and I").
>
> **Scope.** Three of class H's four shapes. The fourth, a comment above a case label, and class I are not done
> here. Every placement change is under `-comments` only. One fix is not: the dead-alias scan (§2), whose effect
> without `-comments` CNR measures at 0.
>
> **Headline, windows (linux and darwin within a few):** misplaced comment groups 3,740 → 3,084 (657 fixed, 1 newly
> misplaced, 0 missed).
>
> | class | D+E1+L head | head |
> |---|---|---|
> | `misplaced:after-open-brace/trailing-to-own-line` | 355 | 12 |
> | `misplaced:after-case-label/trailing-to-own-line` | 205 | 1 |
> | `misplaced:before-close-brace/anchor-differs` | 161 | 58 |
> | `missed:*` (comments the census cannot find) | 0 | 0 |

## 1. Commits, in order (each converter commit after its red test)

| commit | what | .cs moved (W/L/D) |
|---|---|---|
| `84f91d01c0` / `ccc1580ced` | H: a one-line comment after `{` trails the `{` line (`writeLineTrailingComments`); one on a `case X:` line trails the clause's opening line; comments after a block's last statement are written above its `}` (`writeClosingComments`) | 260 / 259 / 259 vs D+E1+L |
| `67770c8deb` / `11ef3a1207` | the dead-alias scan reads the body without its comments (`csharpCodeWithoutComments`) | with the next row: 75 / 78 / 78 vs H |
| `2a25c53486` / `dc9d12e6c3` | a case label's comment trails the header before the clause's other comments | (as above) |

A function body's `{` is followed by the prelude placeholder, so a comment on that line stays on its own line below,
as before.

## 2. What the H arm found

**A comment decided emitted code.** H's arm changed code tokens in `runtime/<goos>/proc.cs` on all three targets.
`casGToPreemptScan`'s closing comment (`// We never notify gp.syncGroup …`) now sits inside the function, as in Go, and
the body gained `ref var gp = ref Ꮡgp.DerefOrNull();`. The converter keeps a pointer parameter's dereferencing alias
only when the converted body names the value. That scan read the body's comments as code, so any comment naming the
parameter kept the alias. The defect predates H: a comment between two statements already reached the scan.
`11ef3a1207` scans the body without its comments. Its lexer treats each literal form the converter emits as code
(regular, interpolated, verbatim and raw strings, character literals), so a `//` inside a string does not hide a
value use. The scan still keeps an alias on a coincidental match in code.

Effect at the stdlib, windows: **139 aliases removed in 75 files, none added**. Every removed line is
`ref var X = ref ᏑX.DerefOrNull();` with the blank line after it. Each was kept only by a comment naming `X`. The
alias is nil-deferring, so behavior does not change; the dead dereference goes. A second check, using an independent
stripper, searched each function for the removed name in code: it found two, `pkg` in `go/types/check.cs` and `decl`
in `go/types/stmt.cs`, and both are named-argument labels (`pkg: Ꮡpkg`), which the scan already excludes.

**A label's comment landed on a comment line.** H's arm read 10 comments as missed on windows. A comment on a `case
X:` line was appended to the last comment the same flush had just written, either one above the label or one inside
a multi-line case list. One sample is `// Exact powers of ten are <= 10^22. // int * 10^k` in `strconv/atof.go`; the
others are in `encoding/json`, `net/mail` and `go/parser`. `dc9d12e6c3` writes the label's comment first, trailing
the header, and only when the current line is the header. Missed is back to 0. The four comments that were above a
label now sit in the case body, which is the before-case shape not done here.

## 3. `unicode/utf8/utf8.cs`, before and after (windows; linux and darwin identical)

```diff
-    if (sz <= 2) {
-        // <= instead of == to help the compiler eliminate some bounds checks
+    if (sz <= 2) { // <= instead of == to help the compiler eliminate some bounds checks
         return ((rune)(((rune)((byte)(p0 & (byte)mask2)) << (int)(6)) | (rune)((byte)(b1 & (byte)maskx))), 2);
```

The same at `DecodeRuneInString`. `commentParityUtf8_test.go` drops its after-open-brace row (2 → 0).

## 4. Residuals, read

- **after-open-brace, 12.** One-line comments after a `{` that the converter follows with its own text: map type
  descriptors in `internal/abi` (`store ptr to key instead of key itself`), `go/doc`, `internal/profile` and
  `text/template/parse`.
- **before-close-brace, 58.** Mostly a comment that ends a case body or an empty clause ("nothing to do for now", "do
  nothing"). A switch body does not reach `writeClosingComments`. This shape goes with before-case in the next seat.
- Pre-existing, read and not touched: an `if` with an `else` is written `if (cond){`, with no space before the brace,
  at the base as well.

## 5. Gates

- `go test ./...` green at `ccc1580ced` and `dc9d12e6c3` (`-timeout 40m`).
- CNR emulation (878 behavioral projects without `-comments`, base binary `63f770b2fe`): ARMS AGREE, the same 15-file
  pre-existing drift, at `ccc1580ced` and at `dc9d12e6c3`. 0 goldens moved. This includes the alias scan, which also
  runs without `-comments`.

## 6. Predictions, scored (each stamped before its arm ran)

| arm | prediction | result |
|---|---|---|
| H `ccc1580ced` | 300–700 .cs; code tokens identical; after-open-brace, after-case-label, before-close-brace each → ≤ 60; misplaced −500..−700, ≤ 30 new; no other class +5 | files **MISSED** (260); code tokens **FIRED** (`proc.cs`, the alias scan, §2); after-open-brace 12, after-case-label 0, before-close-brace 57: MET; misplaced −660, 0 new: MET; missed findings appeared (10, §2) |
| follow-ups `dc9d12e6c3` | 5–60 .cs; only alias lines removed, none added; missed → 0; no class +5 | files **MISSED** (75); alias lines only, none added: MET; missed → 0: MET; classes +≤ 2: MET |
| CNR | arms agree, 0 goldens | MET at both heads |
