# CENSUS — the comment census's anchor: reading past converter scaffolding

> **State: MEASURED, 2026-10-12.** Branch `claude/c1-census-anchor`, stacked on seat F (`77ad8eef81`); the head is
> `3d1afafc2b`. An instrument change, approved by COORD (2026-10-12, "both census improvements are APPROVED; they are
> instrument changes"), proposed in [CENSUS-comment-parity-seat-H2-I.md](CENSUS-comment-parity-seat-H2-I.md) §3 and
> [CENSUS-comment-parity-seat-F.md](CENSUS-comment-parity-seat-F.md) §4. No converter code changes, so no emission
> moves: every number below re-reads the same trees with the old instrument (`77ad8eef81`) and the new one.
>
> **Headline:** misplaced comments fall 66 / 61 / 60 (W/L/D) on seat F's emission and 58 on the committed corpus
> the ratchet reads. Every class that moves is an `anchor-differs` class, and every one falls; every finding that
> goes is accounted for by one of the three rules below, and no finding appears.

## 1. The commits (the instrument change after its red test)

- `ff72aa8842`: `testdata/anchor` plants four comments, each where Go has it, behind a capture copy, a heap box, the
  `goto` a labeled `continue` becomes, and the `)` closing a struct literal. Red at `77ad8eef81`: the census reads all
  four as `misplaced:*/anchor-differs`. A fifth, above a capture copy that the wrong statement follows, stays
  misplaced under the new instrument too: the change cannot accept any line at all.
- `3d1afafc2b`: the instrument change, the ratchet's new baseline, and the fixture's `projitems` entries.

## 2. What changed

An own-line comment's anchor is the first C# code line after it; the census compares it with Go's next code line by
the first identifier Go has there. Three rules, each of which only accepts more lines as the anchor:

- **(a) scaffolding.** A func literal's capture copy (`var xʗ1 = x;`) or a heap box for an escaping local
  (`ref var x = ref heap<T>(out var Ꮡx);`) that does not carry Go's identifier is skipped. One that does carry it is
  the statement's own translation and stays the anchor (`var out [N]byte` becomes such a box).
- **(a) labeled continue and break.** COORD's approval listed the `goto` among the scaffolding to skip. Read in the
  corpus, it is not scaffolding ahead of the statement: `goto continue_L;` *is* Go's `continue L`, with the label
  renamed. So the census reads the label `L` on that line rather than skipping it. That also covers a trailing comment
  on it.
- **(b) closing token.** A comment above a closing brace accepts a C# line opening with `)` as well as `}`: a
  struct literal is a constructor call in C#.

## 3. Old and new, per class, per target

The same trees under both instruments: the committed `src/core` (the ratchet's reading: flat files, else the
windows, linux, darwin flavour) and seat F's emission (`census-fc`, `58413a5f57`) per target. Every `misplaced:` and
`missed:` class; **bold** where the instruments differ.

| class | committed (ratchet) | windows | linux | darwin |
|---|---|---|---|---|
| `misplaced:after-case-label/anchor-differs` | 16 | 1 | 1 | 1 |
| `misplaced:after-case-label/trailing-to-own-line` | 183 | 0 | 0 | 0 |
| `misplaced:after-open-brace/anchor-differs` | 0 | 1 | 3 | 1 |
| `misplaced:after-open-brace/trailing-to-own-line` | 366 | 11 | 11 | 11 |
| `misplaced:before-case/anchor-differs` | 87 | 0 | 0 | 0 |
| `misplaced:before-case/own-line-to-trailing` | 61 | 0 | 0 | 0 |
| `misplaced:before-close-brace/anchor-differs` | 165 | 13 | 12 | 12 |
| `misplaced:before-close-brace/own-line-to-trailing` | 5 | 0 | 0 | 0 |
| `misplaced:decl-doc/anchor-differs` | **46 → 44** | **70 → 69** | **71 → 69** | **70 → 69** |
| `misplaced:decl-doc/own-line-to-trailing` | 2 | 0 | 0 | 0 |
| `misplaced:field-doc/anchor-differs` | 10 | 9 | 9 | 8 |
| `misplaced:free-in-type/anchor-differs` | 13 | 13 | 8 | 8 |
| `misplaced:free-stmt/anchor-differs` | **726 → 672** | **244 → 187** | **227 → 174** | **224 → 171** |
| `misplaced:free-stmt/own-line-to-trailing` | 283 | 0 | 0 | 0 |
| `misplaced:free-top-level/anchor-differs` | 9 | 3 | 2 | 3 |
| `misplaced:in-composite-literal/anchor-differs` | 844 | **15 → 10** | **26 → 23** | **21 → 18** |
| `misplaced:in-composite-literal/own-line-to-trailing` | 22 | 0 | 0 | 0 |
| `misplaced:in-composite-literal/trailing-to-own-line` | 1574 | 4 | 4 | 4 |
| `misplaced:in-expression/anchor-differs` | 10 | 2 | 2 | 1 |
| `misplaced:in-expression/own-line-to-trailing` | 4 | 0 | 0 | 0 |
| `misplaced:in-expression/trailing-to-own-line` | 991 | 992 | 990 | 994 |
| `misplaced:spec-doc-in-group/anchor-differs` | 2 | **2 → 1** | **2 → 1** | **2 → 1** |
| `misplaced:trailing-field/anchor-differs` | 12 | 12 | 11 | 11 |
| `misplaced:trailing-spec/anchor-differs` | 11 | 11 | 11 | 11 |
| `misplaced:trailing-spec/trailing-to-own-line` | 1 | 1 | 1 | 1 |
| `misplaced:trailing-stmt/anchor-differs` | **61 → 59** | **58 → 56** | **57 → 55** | **57 → 55** |
| `misplaced:trailing-stmt/trailing-to-own-line` | 42 | 32 | 37 | 29 |
| `missed:after-case-label` | 2 | 0 | 0 | 0 |
| `missed:decl-doc` | 432 | 15 | 12 | 12 |
| `missed:field-doc` | 17 | 17 | 13 | 13 |
| `missed:free-stmt` | 6 | 0 | 0 | 0 |
| `missed:in-composite-literal` | 1 | 0 | 0 | 0 |
| `missed:in-expression` | 1 | 1 | 1 | 1 |
| `missed:spec-doc-in-group` | 954 | 1 | 1 | 1 |
| `missed:trailing-field` | 2 | 0 | 0 | 0 |
| `missed:trailing-spec` | 86 | 73 | 74 | 76 |
| `missed:trailing-stmt` | 1 | 0 | 0 | 0 |
| **misplaced, all classes** | **5546 → 5488** | **1494 → 1428** | **1485 → 1424** | **1469 → 1409** |

28 other classes (blank-line, eol, group, top, blank2, partial) read the same under both instruments on every root.

The ratchet's baseline (`testdata/corpus_baseline.txt`) moves on exactly the three lines the committed column marks:
`decl-doc/anchor-differs` 46 → 44, `free-stmt/anchor-differs` 726 → 672, `trailing-stmt/anchor-differs` 61 → 59.
The ratchet failed on those three falls at `ff72aa8842` + the instrument change and passed once the baseline was
rewritten in the same commit.

## 4. Every finding that goes, accounted for

Each finding the old instrument lists and the new one does not, by the shape of the C# line it anchored on:

| root | gone | capture copy | heap box | labeled goto | `)` | unexplained | new findings |
|---|---|---|---|---|---|---|---|
| committed | 58 | 45 | 6 | 7 | 0 | 0 | 0 |
| windows | 66 | 48 | 3 | 10 | 5 | 0 | 0 |
| linux | 61 | 47 | 4 | 7 | 3 | 0 | 0 |
| darwin | 60 | 47 | 3 | 7 | 3 | 0 | 0 |

Each root's count of findings gone equals its fall in the counters, so no finding is hidden by the census's
400-per-class-per-file example limit (§5). The committed corpus shows no `)` case: its literals' comments are still
the pre-F placement, written above the declaration.

## 5. Correction: the misplaced totals in seats A-BC to F were read from capped findings

The census keeps at most 400 example findings per class per file. My "misplaced comment groups" probe counted those
findings, not the census's counters, and two files exceed the limit: `strconv/eisel_lemire.go`
(`in-composite-literal/trailing-to-own-line`, 696 before seat F, 0 after) and x/text `norm/tables15.0.0.go`
(`in-expression/trailing-to-own-line`, 941). The per-class counts in those records come from the counters and stand.
The totals, from the counters (W/L/D):

| arm | counters | recorded (W, from findings) |
|---|---|---|
| seat A-BC's base (`census-base`) | 5,361 / 5,236 / 5,151 | 4,519 |
| seat H-rest+I's head (`9b523f1584`) | 3,818 / 3,723 / 3,664 | 2,976 |
| seat F's head (`58413a5f57`) | 1,494 / 1,485 / 1,469 | 948 |

Both files read the same at the base and at H-rest+I's head (696 and 941), so the deltas recorded for seats A-BC, D+E1+L,
H and H-rest+I stand, and their absolute totals are 837 higher on every target. The arms between are not kept. Seat F's
fall is 2,324 on windows (3,818 → 1,494), not 2,028: eisel_lemire's 296 unlisted rows moved with the rest. The seated
records take no commits; this section carries the correction.

## 6. Gates

- `go test ./...` in `src/go2cs` green at `3d1afafc2b` (`-timeout 40m`).
- The census's own tests: the planted good/bad/bad2 readings are unchanged; the new anchor test is red at
  `ff72aa8842` and green at `3d1afafc2b`; the ratchet is red on the three falls before the baseline rewrite and green
  after it.

## 7. Prediction

None was stamped before the old/new reading; this seat skipped that step. What the design guarantees instead, and the
reading confirms: no class rises and no finding appears, because each rule only accepts more anchors.
