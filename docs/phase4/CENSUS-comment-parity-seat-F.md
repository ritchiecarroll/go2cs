# CENSUS — comment parity seat F: comments inside composite literals

> **State: MEASURED, 2026-10-12.** Branch `claude/c1-comment-parity-f`, built on seat H-rest+I (`9ef2e91a7c`); every
> "head" number is read at `58413a5f57`. Method as in
> [CENSUS-comment-parity-seat-H2-I.md](CENSUS-comment-parity-seat-H2-I.md): one seeded `-stdlib -comments` emission per
> arm on windows, linux and darwin amd64, seeded from `14adc76d79`, each diffed against the arm before it; the
> `internal/commentcensus` census over each target's own files. The base arm is seat H-rest+I's head (`9b523f1584`).
> Taken by C1 on COORD's direction (ledger 2026-10-12 06:57, "C1 continues to F, G and I").
>
> **Headline, windows (linux and darwin within a few):** misplaced comment groups 2,976 → 948 (2,037 fixed, 9 newly
> misplaced, 0 missed). Comments inside composite literals: 2,334 → 19. Code tokens are identical in every changed
> file, and every change is under `-comments` only.
>
> | class | base | head |
> |---|---|---|
> | `misplaced:in-composite-literal/trailing-to-own-line` | 1,563 | 4 |
> | `misplaced:in-composite-literal/anchor-differs` | 749 | 15 |
> | `misplaced:in-composite-literal/own-line-to-trailing` | 22 | 0 |
> | `eol:moved-to-own-line` | 65 | 3 |
> | `missed:*` | 0 | 0 |

## 1. Commits, in order (each converter commit after its red test)

| commit | what | .cs moved (W/L/D), vs the arm before |
|---|---|---|
| `0d3ead8834` / `809235b514` | F: `writeListComments` writes the comments ahead of each element or argument of a list that spans lines: one on the previous item's line trails it, with no blank line after it; any other takes its own line. A literal writes the comments after its last element above its `}`, which then always starts its own line. A const or var spec writes only the comments before it above its doc; any inside it that its emission does not reach still go above the doc | 135 / 136 / 131 vs base |
| `b6f95cf297` | registers the new test file in `go2cs-src.projitems` | — |
| `a1ff39f00e` / `c4b4f37c0a` | a statement's trailing comment stops at its block's `}` | with the next two rows: 39 / 40 / 40 vs F |
| `09d0975646` / `02c5dbc69f` | an own-line comment in a list keeps the blank line Go has above it | (as above) |
| `b62e89fff9` / `1f42c5ddbb` | a list writes only the comments after its opening bracket | (as above) |
| `af09ee0ed9` / `58413a5f57` | the bound of `c4b4f37c0a` applies to func literal bodies only | 5 / 5 / 5 vs the arm before |

Head vs base: 121 / 122 / 116 .cs; `package_info.cs` moves position-map lines only (57 / 58 / 55 files);
`runtime/mfinal.cs.auto` moves 20 comment lines, the hand-own's review sibling. Windows: −3,485 / +1,964 comment-only
lines (row labels rejoin their rows), −66 / +35 blank lines.

Call arguments that span lines share the list path, so a comment between two arguments now trails its argument with
no blank line after it, as in Go. That is the only effect on class G, whose open shapes (binary operands, parameters)
wait for the owner's decision 3; `misplaced:in-expression` is 986 → 992 (§4).

## 2. What the arms found

**A comment decided emitted code (F's arm).** `internal/profile/encode.cs` changed code tokens on all three targets.
`func(b *buffer, m message) error { return decodeUint64(b, &m.(*Location).ID) }, // optional uint64 id = 1;` labels
the element after the func literal's `}`. With the spec no longer writing its literal's comments above the
declaration, the literal body's `return` took the label as its own trailing comment, so the lambda kept a block body.
In `locationDecoder`, which the init-order pass moves into an init method, the line read
`(…) => decodeUint64(…); // optional uint64 id = 1,`: the element's comma inside the comment, which does not compile.
`c4b4f37c0a` stops a statement's trailing comment at the enclosing block's `}`. Its own arm found that this was too
broad: four one-line func declarations (`func (f *xorNonceAEAD) NonceSize() int { return 8 } // 64-bit sequence
number`, also in `net/http/h2_bundle.go` and twice in x/text `norm/forminfo.go`) lost their label to the next
declaration. `58413a5f57` applies the bound to func literal bodies only; those labels are back on their `return`, as
at the base.

**A comment was lost (F's arm), four on windows.** The three `encode.go` labels above, and `math/big/rat.go`'s
`// z == 0; normalize sign and denominator`. A switch lowered to if/else renders the next case's condition before the
body above it is written; the first argument of `len(z.b.abs)` took every pending comment ahead of it, reaching back
into the previous case body, and that render was not the one written. `1f42c5ddbb` makes a list take only the comments
after its opening bracket. Missed is back to 0.

**Go's blank line above an own-line list comment** (`archive/zip/struct.go`, `// time bits …`) was dropped;
`02c5dbc69f` keeps it.

## 3. `unicode/utf8/utf8.cs`, before and after (windows; linux and darwin identical)

```diff
-//   1   2   3   4   5   6   7   8   9   A   B   C   D   E   F
-// 0x00-0x0F
-// … (16 more row labels and the second heading, all above the doc)
 // first is information about the first byte in a UTF-8 sequence.
 internal static array<uint8> first = new uint8[]{
-    @as, @as, @as, @as, @as, @as, @as, @as, @as, @as, @as, @as, @as, @as, @as, @as,
+    //   1   2   3   4   5   6   7   8   9   A   B   C   D   E   F
+    @as, @as, @as, @as, @as, @as, @as, @as, @as, @as, @as, @as, @as, @as, @as, @as, // 0x00-0x0F
 …
-    s5, s6, s6, s6, s7, xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, xx
+    s5, s6, s6, s6, s7, xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, xx // 0xF0-0xFF
 }.array();
```

This is the plan's W2 "after F". `commentParityUtf8_test.go` drops both F rows. Its J rows move: the row labels are
end-of-line comments again, after rows `@as` widens, so `eol:misaligned` and `eol:cs-ragged-run` read 3 where they
read 2, and `eol:moved-to-own-line` (2) goes.

## 4. Residuals, read

- **in-composite-literal, 19 (W).** Read by sample: a comment after a literal's last element, now above its closing
  brace as in Go, where the next C# line (`))`) is not the one the census anchors in Go (`},`): emitter.go's
  `// TODO(mknyszek): …`, `os/types_windows.go`'s `// fileStat.path is used …`.
- **Newly misplaced, 9 (W).** The two above, and seven comments on an operand of `||` (five `// message type and
  uint24 length field` in `crypto/tls/handshake_messages.go`, `go/ast/import.go`, `go/scanner/scanner.go`). At the
  base the next call's argument list took them, also misplaced; now they wait for the next flush, as every other
  class G comment does. Decision 3 places them.
- **in-expression, 992 (W).** Class G, decision 3.

## 5. Gates

- `go test ./...` in `src/go2cs` green at `58413a5f57` (`-timeout 40m`). At `809235b514` its one failure was
  `TestProjitemsRegistersEveryGoSource`, naming the new test file; `b6f95cf297` registers it.
- CNR emulation (878 behavioral projects without `-comments`, base binary `63f770b2fe`): ARMS AGREE, the same 15-file
  pre-existing drift, at `58413a5f57`. 0 goldens moved.

## 6. Predictions, scored (each stamped before its arm ran)

| arm | prediction | result |
|---|---|---|
| F `809235b514` (12:47Z) | 100–250 .cs | MET (135 / 136 / 131) |
| | code tokens identical | **FIRED** (`encode.cs`, §2; fixed by `c4b4f37c0a`, then `58413a5f57`) |
| | in-composite-literal 2,334 → ≤ 300 | MET (19) |
| | in-expression trailing-to-own-line falls by 100–700 | **MISSED** (+6: the class is binary operands, not call arguments) |
| | misplaced falls by 1,900–2,900, ≤ 50 new | MET (−2,021, 19 new) |
| | missed stays 0 (allow ≤ 5) | in the band (4), each read as a defect and fixed (§2) |
| | comment-only lines fall by 1,200–2,500 | MET (−1,518) |
| | blank lines fall by 100–1,000 | **MISSED** (−63) |
| | no class but F, G, J moves by more than 5 | **MISSED** (`body:extra:unexplained` −59, `free-stmt` −9: both falls) |
| follow-ups `1f42c5ddbb` (13:15Z) | 5–60 .cs vs F | MET (39 / 40 / 40) |
| | code tokens vs base identical | MET |
| | missed 4 → 0 | MET |
| | newly misplaced vs base ≤ 10 | **MISSED** (13: four from `c4b4f37c0a`'s bound, §2) |
| | blank +5..+60; `go-blank-cs-none` falls ≥ 3 | MET (+32; −4) |
| | in-composite-literal ≤ 30; misplaced vs F moves ≤ 30 | MET (19; −3) |
| last `58413a5f57` (13:40Z) | 3–30 .cs vs the arm before | MET (5 / 5 / 5) |
| | code tokens vs base identical | MET |
| | trailing-to-own-line ≤ 34; newly misplaced vs base ≤ 9 | MET (32; 9) |
| | missed 0; in-composite-literal ≤ 30 | MET |
| | CNR: arms agree, 0 goldens | MET (ARMS AGREE, same 15-file drift) |

The first two arms' CNR runs were stopped when a later commit superseded their binary; CNR is read at the head.
