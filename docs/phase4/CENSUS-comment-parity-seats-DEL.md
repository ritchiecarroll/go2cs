# CENSUS — comment parity seats D, E1 and L: Go's blank lines between specs, none of the converter's own

> **State: MEASURED, 2026-10-12.** Branch `claude/c1-comment-parity-del`, built on seat B+C (`7b9b147ee8`); every
> "head" number is read at `ae8b6289ef`. Method as in
> [CENSUS-comment-parity-seats-A-BC.md](CENSUS-comment-parity-seats-A-BC.md): one seeded `-stdlib -comments` emission
> per commit on windows, linux and darwin amd64, every arm seeded from `14adc76d79` and diffed against the arm before
> it; the `internal/commentcensus` census over each target's own files. The base arm is B+C's head (`5ae3b2b10d`;
> `7b9b147ee8` adds a record only). Go is read at go1.24.13. Taken by C1 on COORD's direction (ledger 2026-10-12
> 01:14, "C1 continues to D, E and L").
>
> **Scope.** Every change is under `-comments` only, as seat A's is, so the behavioral goldens (captured without it)
> do not move: the plan's 23 + 15 E1 goldens and 21 L goldens stay as they are. **E2** (the blank line after a
> parameter prelude or named-result locals) is the owner's decision 2 and is not touched.
>
> **Headline (windows; linux and darwin within a few lines):**
>
> | class | B+C head | head |
> |---|---|---|
> | D `group:go-blank-cs-none` (Go blank line between specs, none in C#) | 689 | 0 |
> | D `group:go-none-cs-blank` | 0 | 0 |
> | E1 `body:extra:after-lowered-switch` | 618 | 55 |
> | E1 `body:extra:before-close-brace` | 283 | 0 |
> | L `blank2:scaffold` | 3 | 0 |
> | L `blank2:member-boundary` | 23 | 12 |
> | misplaced comment groups | 3,740 | 3,740 |
>
> Code tokens are identical in every arm: every changed file reads the same once comments and whitespace are
> removed. No comment is added or lost.

## 1. Commits, in order (each converter commit after its red test)

| commit | what | .cs moved (W/L/D) vs the arm before | lines (W) |
|---|---|---|---|
| `4c17c5a347` / `a1e9263598` | D: `writeSpecGap` keeps Go's blank line before a const or var spec in a group | 171 / 169 / 164 | +809 blank |
| `2a3c4f16a3` / `f7ed9b361f` | E1: `trimTrailingLineBreak` ends a switch or type switch without a line break of its own | 449 / 455 / 455 | −1,440 blank |
| `9f61a84928` / `442c3fce33` | L: a struct lifted ahead of its var opens no second line (`inValueSpec`) | 45 / 43 / 42 | −71 blank |
| `6f4973540d` / `592a561834` | D follow-up: the gap is measured from the previous spec's trailing comment | 3 / 3 / 3 (with the next row) | −4 blank, 7 comment lines moved |
| `12a3ff50fe` / `ae8b6289ef` | hold only a moved `default:`'s own comments, not those above the next case | (as above) | |

`package_info.cs` moves its position-map lines only, in every arm; `.cs.auto` siblings move whitespace only (W: +25
in D, −13 in E1); no `.csproj` or README moves.

**The two follow-ups were found by these footprints:**
- D's arm raised `group:go-none-cs-blank` 0 → 3, all in `debug/elf/elf.go`. A spec ending in a `/* … */` comment
  spanning lines (`DF_SYMBOLIC`, `DF_STATIC_TLS`, `DT_PREINIT_ARRAY`) was measured from the spec's end, so the
  comment's own lines read as a gap. `592a561834` measures from the comment's end; it is back to 0.
- E1's arm raised `body:go-blank-cs-none` by 3. One of the three is the census mapping Go's `fmtFrac(…, 9)` line in
  `time.go` to the other `fmtFrac` call; the C# keeps Go's blank line where Go has it. The other two
  (`debug/dwarf/entry.go`'s `// address`, `crypto/internal/fips140/bigmod/nat.go`'s `// The following specialized
  cases …`) are comments above the case label that follows a leading `default:` the if/else lowering moves to the
  end. **This is a defect in seat B+C's `5ae3b2b10d`, which is already seated:** its hold took every comment in the
  default's span, which runs to the next clause, so these were written after the whole switch. The switch's own
  trailing line break had given them a blank line above, so E1 surfaced them. `ae8b6289ef` holds only the default's
  own comments: those before its last statement's end, or indented deeper than its `case` keyword. Both now sit
  above their case's first statement. They are the only change of that commit on any target.

## 2. Residuals, read

- **E1, 55 `after-lowered-switch`.** The census calls a C# blank line after a line ending `}}` an
  after-lowered-switch when the next line does not map to a Go element. Across the windows tree, 226 such blank
  lines sit in files this emission does not write (converted tests and hand-owned files keep the seed's text), and
  the one site read in a written file is Go's own blank line before a label (`database/sql/convert.go:177`,
  `nextCheck:`), which the census cannot map.
- **L, 12 `member-boundary` runs.** All 12 are inside block comments whose Go source has two blank lines in a row
  (`math/big/natdiv.go`, `sort/sort.go`, `text/template/doc.go`); the C# carries the text unchanged. The census counts
  blank-line runs inside comment text.
- **Class K is not done here.** A lifted var's doc is now directly above its lifted struct, with no blank line
  between; putting the doc above the var itself is class K's.

## 3. `unicode/utf8/utf8.cs`, before and after (windows; linux and darwin identical)

D: the four blank lines Go has inside the `const ( … )` group.

```diff
 internal static UntypedInt t5 => 0b11111000;
+
 internal static UntypedInt maskx => 0b00111111;
 internal static UntypedInt mask2 => 0b00011111;
 internal static UntypedInt mask3 => 0b00001111;
 internal static UntypedInt mask4 => 0b00000111;
+
 internal static UntypedInt rune1Max => /* 1<<7 - 1 */ 127;
 internal static UntypedInt rune2Max => /* 1<<11 - 1 */ 2047;
 internal static UntypedInt rune3Max => /* 1<<16 - 1 */ 65535;
+
 // The default lowest and highest continuation byte.
 internal static UntypedInt locb => 0b10000000;
 internal static UntypedInt hicb => 0b10111111;
+
 // These names of these constants are chosen to give nice alignment in the
```

E1: the converter's blank lines after a switch and before a closing brace, which Go never has (two of four shown).

```diff
     case {} when r <= MaxRune: {
         return 4;
     }}
-
     return -1;
 }
@@
             return 3;
         }}
     }
-
 }
```

L and the follow-ups do not change `utf8.cs`. `commentParityUtf8_test.go` drops its D and E1 rows; E2's
`body:extra:after-named-result-locals: 3` stays until decision 2.

## 4. Gates

- `go test ./...` in `src/go2cs` green at `442c3fce33` and `ae8b6289ef` (`-timeout 40m`; one earlier run at
  `442c3fce33` hit the default 10-minute timeout while a corpus arm ran beside it, with 0 test failures).
- CNR emulation (878 behavioral projects without `-comments`, base binary `63f770b2fe`): ARMS AGREE, the same 15-file
  pre-existing drift, at `442c3fce33` and at `ae8b6289ef`. 0 goldens moved.

## 5. Predictions, scored (each stamped before its arm ran)

| arm | prediction | result |
|---|---|---|
| D | 150–400 .cs; +600..+720 blank; 0 code, 0 comment; group blanks ≤ 40; `go-none-cs-blank` stays 0 | files MET; blank **MISSED** (+809); code/comment MET; group 689 → 0 MET; `go-none-cs-blank` falsifier **FIRED** (+3, `elf.go`, fixed by `592a561834`) |
| E1 | 400–900 .cs; −700..−1,100 blank; after-switch ≤ 40; before-brace falls; Go body blanks +≤ 5 | files MET; blank **MISSED** (−1,440); after-switch **MISSED** (55, §2); before-brace 283 → 0 MET; body +3 MET (read in §1) |
| L | 15–60 .cs; blank only; member-boundary ≤ 3; top does not rise | files MET; blank only MET; member-boundary **MISSED** (12, Go's own, §2); top MET (−2) |
| follow-ups | 2–15 .cs; elf −3 blank; comments +N/−N; `go-none-cs-blank` → 0; misplaced falls ≥ 2 | files MET (3); elf −3 MET; comments 7/7 MET; → 0 MET; misplaced **MISSED** (3,740 both: the census does not separate the after-switch placement from the above-the-case one) |
| CNR | arms agree, 0 goldens | MET at both heads |
