# CENSUS — comment parity seat E2: one blank line between the converter's prelude and the Go body

> **State: MEASURED, 2026-10-12.** Branch `claude/c1-comment-parity-e2`, stacked on seat C aliases (`eef1e8a7d7`).
> The owner's decision 2 (COORD, 2026-10-12): keep exactly one blank line between the converter's prelude (receiver or
> parameter setup, named-result locals) and the Go body; drop any extra. Method as in
> [CENSUS-comment-parity-seat-F.md](CENSUS-comment-parity-seat-F.md): one seeded `-stdlib -comments` emission on
> windows, linux and darwin amd64, seeded from `14adc76d79`, diffed against seat C's head arm (`31dd8dcae9`).
>
> **Headline:** the converter wrote the blank line after the prelude's FIRST part, so a function whose prelude has two
> or more parts (named results with a variadic, an array clone, a pointer deref or a heap box) had a blank line inside
> its prelude and none before its body. It now writes one blank line after the whole prelude. 264 / 268 / 269 functions
> in 143 / 147 / 146 files (W/L/D) move one blank line each; no other line changes. The census now reads that one blank
> line as the ruled separator instead of a defect. Two behavioral goldens move, whitespace only.

## 1. Commits, in order (each change after its red test)

| commit | what |
|---|---|
| `7ce68096e8` / `5a24b1127b` | the prelude's parts are written line after line and one newline closes it: one blank line between prelude and body, none inside. Red at the parent on the four multi-part functions of the test (`sum`, `esc`, and the frame forms `get`, `rec`); the single-part `one` and `val` already passed |
| `8911663bff` / `05fee620cb` | the census reads that blank line as `prelude-separator`, not a defect class; the corpus ratchet and the utf8 table are re-stated |
| `decf9cf025` | goldens: `NilReceiverNormalization` and `ZeroValueArrayNamedResult` (`main.cs` and `main.cs.target`), one function each with a two-part prelude; each equal to its base with blank lines stripped |

`unicode/utf8/utf8.cs` does not move: its preludes are single-part (`rune r = default!;` / `nint size = default!;`,
then the blank line, then the body). Its census row `body:extra:after-named-result-locals: 3` goes, because those three
blank lines are now read as the ruled separator; the utf8 test table drops it.

A function before and after (`text/tabwriter.flush`, a frame form):

```diff
     try {
         ref var b = ref Ꮡb.DerefOrNull();
-
         ref var err = ref Ꮡerr.ValueSlot;
+
         defer(Ꮡb.handlePanic, Ꮡerr, flushˢ, ref ᒐ);
```

## 2. The census: what counts as the separator

`preludeSeparator` (census.go) accepts a body's blank line as the separator when the lines from the opening brace down
to it are all forms the converter writes there (`preludeLineRe`: named-result locals and variadic slices, array clones,
pointer derefs, heap boxes and named-result aliases, a frame's `GoFrame ᒐ = default;` and `try {`), exactly one blank
line follows them, and the next line opens the Go body's first line: its comment, or the first C# line holding all its
identifiers, with only scaffolding, a bare `{` or comments between. Its fixture (`testdata/prelude`): `good` is the
converter's emission, four separators and no body class, including `first`, whose Go body opens with `var z int`, a line
that reads like the prelude; `bad` puts the blank line inside the prelude (three functions) and two after it (one).
The gate was made to fail: with the "only scaffolding between" check removed, `bad` reads two false separators.

On the committed corpus, which predates the converter fix, the ratchet moves only by the instrument:
`after-named-result-locals` 808 → 202, `after-param-prelude` 3,128 → 210, `unexplained` 946 → 866 (the `ref heap(x, out
var Ꮡx)` box, which the old prev-line regex never matched).

## 3. Footprint and census, head vs seat C

| target | .cs (+ .cs.auto) | functions moved | blank-line count per file | code and comment lines | `package_info.cs` |
|---|---|---|---|---|---|
| windows | 138 + 5 | 264 | unchanged in every file | identical (each file equal with blank lines stripped) | 0 |
| linux | 142 + 5 | 268 | unchanged | identical | 0 |
| darwin | 142 + 4 | 269 | unchanged | identical | 0 |

Census, the committed instrument (`05fee620cb`) on both arms:

| class (W; L, D) | seat C | head |
|---|---|---|
| `prelude-separator` (not a defect) | 3,427; 3,323; 3,255 | 3,603; 3,495; 3,422 |
| `body:extra:after-named-result-locals` | 186; 162; 163 | 58; 44; 50 |
| `body:extra:after-param-prelude` | 210; 202; 199 | 192; 178; 175 |
| `body:extra:unexplained` | 2,306; 2,212; 2,170 | 2,270; 2,176; 2,134 |
| `body:go-blank-cs-none` | 1,882; 1,836; 1,811 | 1,876; 1,830; 1,805 |
| misplaced comment groups | 882; 878; 863 | the same; 0 fixed, 0 newly misplaced |

No other class moves. Of the 264 functions moved on windows, the census reads 176 as separators; the falls in the
three `extra:` classes and in `go-blank-cs-none` sum to 182 = 176 + 6.

**Residuals, read (windows).** `after-named-result-locals` 58 and `after-param-prelude` 192 are, by sample, two kinds.
(1) Real separators the census does not accept, because C# drops or renames some of the Go first line's identifiers:
`return (*headerV7)(unsafe.Pointer(b))` becomes `return Ꮡ((headerV7)(b.Value));` (no `unsafe`, `Pointer`),
`for i, c := range b` becomes `foreach (var (i, vᴛ1) in b.Value)`. (2) Blank lines in a body the census cannot map to
Go's (`go-blank-unmapped`) whose previous line happens to read like a prelude line (`err = default!;`,
`slice<byte> mask = default!;`). Neither is a converter defect of class E2. A census cut that accepted half the
identifiers was tried and reverted (§5).

## 4. Gates

- `go test ./...` in `src/go2cs` green at `05fee620cb` (`-timeout 40m`). The census fixture gate was made to fail first (§2).
- CNR emulation (878 behavioral projects without `-comments`, base binary `63f770b2fe`): ARMS DIFFER on exactly two files, the two goldens above, each the cut arm's fresh transpile at `5a24b1127b`; the same 15-file pre-existing drift, byte-identical in both arms; 0 transpile failures; 0 tracked deletions after restore.

## 5. Predictions, scored (stamped 17:15Z, before the arm ran)

| prediction | result |
|---|---|
| P1 380–460 .cs per target (from a scan of seat C's emission: 417 / 416 / 417 files) | **MISSED** (143 / 147 / 146). The scan's predicate was loose on one side and narrow on the other: it counted a Go `var x T` statement after a one-part prelude as a prelude line (291 scanned files did not move), and it lacked the named-result alias `ref var err = ref Ꮡerr.ValueSlot;` (17 files moved outside it). The mechanism held: every moved file is one of these functions |
| P2 code and comment lines identical; blank-line count per file unchanged | MET (every file, every target) |
| P3 0 `package_info.cs` | MET |
| P4 old instrument (W): `after-named-result-locals` falls 600–930; `after-param-prelude` rises by the same within 50; no other class moves more than 10 | fall **MISSED** (−139, as P1); rise MET (+140); others MET (`unexplained` −7, `go-blank-cs-none` −6) |
| P5 CNR: 8–12 goldens move, whitespace only; the 15-file drift unchanged | **MISSED** in count (2 goldens, not 8–12: the same scan over-count as P1); MET in kind (whitespace only, each equal with blank lines stripped; the 15-file drift unchanged) |
| P6 `utf8.cs` does not move | MET |

**The census cut tried and reverted.** Reading the residuals, I stamped (17:39Z) a cut that accepted the line after the
blank when it held half the Go first line's identifiers: Q1, `after-param-prelude` 192 → ≤ 100 and
`after-named-result-locals` 58 → ≤ 40 on windows, no other class moving; Q2, every census fixture unchanged. Q2 FIRED:
`bad`'s `esc` read `a = a.Clone();` (half of `n = a[1]`'s identifiers) as the body's first line. A third cut, stamped 17:41Z (half only
on a line that does not read like the prelude), passed the fixtures but MISSED Q1 (192 → 148, 58 → 51) and moved
`unexplained` by −9, which its falsifier forbade. Neither was committed; the instrument is `05fee620cb`'s.
