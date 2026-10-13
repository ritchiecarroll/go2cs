# CENSUS — comment parity: the rest of class H, and class I read

> **State: MEASURED, 2026-10-12.** Branch `claude/c1-comment-parity-i`, built on seat H (`6eca08c036`); the head is
> `9b523f1584`. Method as in [CENSUS-comment-parity-seat-H.md](CENSUS-comment-parity-seat-H.md): one seeded
> `-stdlib -comments` emission on windows, linux and darwin amd64, seeded from `14adc76d79` and diffed against seat
> H's head arm (`dc9d12e6c3`); the `internal/commentcensus` census over each target's own files. Taken by C1 on
> COORD's direction (ledger 2026-10-12 06:57, "C1 continues to F, G and I").
>
> **Headline, windows (linux and darwin within a few):** misplaced comment groups 3,084 → 2,976 (108 fixed, 0 newly
> misplaced, 0 missed). `misplaced:before-case` 62 → 0, `misplaced:before-close-brace` 58 → 13. Code tokens are
> identical in every changed file, comment lines move one for one (+314 / −314), and every change is under
> `-comments` only.
>
> **Class I needs no converter change, by measurement (§3).** Most of the 255 `misplaced:free-stmt` findings read as
> correctly placed: the census anchor does not skip the scaffolding the converter writes before a statement.

## 1. The commit (after its red test)

`6f07d4dff0` / `9b523f1584`:
- `writeAboveLabelComments` writes the comments Go has on their own lines above a `case` or `default` label above
  the clause's C# header, at its indentation. It is called in every clause loop that writes a header: the if/else
  chain and both C# `switch` forms, the type switch, and select. A moved default's held comments are released
  first.
- An empty block writes the comments inside it. Before, `if op == Over { // No-op. } else { … }` (image/draw) wrote
  the comment in the `else`.
- `visitStatementList` writes the comments after a case body's last statement, indented deeper than its label, at
  the end of that body. Before, the next clause wrote them.

Footprint (.cs W/L/D): 93 / 87 / 88; `package_info.cs` position-map lines only; code tokens identical; +314 / −314
comment lines, −3 / +1 blank lines (W).

## 2. Residual of class H, read

- **before-close-brace, 13 (W).** These are comments ending a block that the converter writes outside
  `visitBlockStmt`, such as an `if` with an init statement wrapped in an outer block. Left for a later pass.
- **after-open-brace, 12 (W).** Unchanged from seat H: a `{` that the converter follows with its own text.

## 3. Class I: the 255 free-statement findings, read

For each `misplaced:free-stmt` finding (windows, at seat H's head), I compared the identifiers on the first code line
after the comment in Go with those on the first code line after it in C#. Renaming suffixes (`Δn`, `ᴛn`, `ʗn`), box
marks (`Ꮡ`, `~`) and keywords were removed first. They agree on at least half of Go's identifiers in **152 of 255**;
**29** could not be read (files the emission does not write, or no code line after the comment). The **74** that
disagree were read by sample: nearly all sit directly above converter scaffolding that precedes the Go statement:
- closure capture copies, `var blkʗ1 = blk;` before a func literal (archive/tar, crypto/internal/cryptotest, tls);
- `goto continue_X;` / `goto break_X;` for a labeled `continue` / `break` (crypto/rsa, fmt/print);
- a heap box before a declaration that escapes, `ref var err = ref heap<error>(out var Ꮡerr);` (go/internal/gcimporter).

The comment stands above the Go statement's translation, as Go has it; the census reads the scaffolding line as the
anchor. A converter change would move nothing here. Teaching the census to skip these line forms would lower the
count, and that changes the ratchet's baseline, so it is proposed, not done. Two genuine residuals were seen in
the sample: comments inside a multi-line case list (go/parser), and one above a lifted local function (go/doc).

## 4. Gates

- `go test ./...` in `src/go2cs` green at `9b523f1584` (`-timeout 40m`).
- CNR emulation (878 behavioral projects without `-comments`, base binary `63f770b2fe`): ARMS AGREE, the same 15-file
  pre-existing drift, at `9b523f1584`. 0 goldens moved.

## 5. Prediction, scored (stamped before the arm ran)

| prediction | result |
|---|---|
| 100–300 .cs per target | **MISSED** (93 / 87 / 88) |
| code tokens identical; comment lines +N/−N equal; blank ±≤ 40 | MET (+314/−314; −3/+1) |
| before-case 62 → ≤ 10 | MET (0) |
| before-close-brace 58 → ≤ 25 | MET (13) |
| misplaced −80..−150, ≤ 15 new | MET (−108, 0 new) |
| free-stmt moves ≤ 20; no other class +5; missed stays 0 | MET (0; +1 at most; 0) |
| CNR: arms agree, 0 goldens | MET (ARMS AGREE, same 15-file drift) |
