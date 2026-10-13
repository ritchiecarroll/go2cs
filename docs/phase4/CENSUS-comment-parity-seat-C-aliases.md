# CENSUS — comment parity seat C (aliases): an alias's doc above its `global using` line

> **State: MEASURED, 2026-10-12.** Branch `claude/c1-comment-parity-ca`, stacked on the census-anchor seat
> (`fabd73c92a`); the head is `31dd8dcae9`. The owner's decision 6 (COORD, 2026-10-12, "Go with recommendations on
> comments"): an alias's doc goes above its `global using` line, the only legal place C# has for the alias. Method as
> in [CENSUS-comment-parity-seat-F.md](CENSUS-comment-parity-seat-F.md): one seeded `-stdlib -comments` emission per
> converter commit on windows, linux and darwin amd64, seeded from `14adc76d79`, each diffed against the arm before
> it; the base arm is seat F's head (`58413a5f57`; the census-anchor seat changed no emission). The census is
> [the anchor seat's](CENSUS-comment-census-anchor.md) instrument, over both sides of every arm.
>
> **Headline, head vs base:** 16 .cs files per target gain the docs of their aliases, each set apart from the file's
> leading comments by one blank line: +136 comment lines and +27 blank lines on every target, no code line and no other
> comment line changed. `missed:decl-doc` 15 → 2 / 12 → 1 / 12 → 1 (W/L/D); no other census class moves. Every change
> is under `-comments` only: 0 goldens.

## 1. Commits, in order (each converter commit after its red test)

| commit | what | .cs moved (W/L/D), vs the arm before |
|---|---|---|
| `5a5b56c666` / `9667a7d6df` | an alias's doc, and that of a defined type over an interface (the same C# form), is written directly above its `global using` line, at the file's top level | 16 / 16 / 16 vs base |
| `2ba4a2ae01` / `992369a92a` | the doc keeps the blank line Go has above the declaration, the first alias's included; none when nothing is above the using block | 16 / 16 / 16 |
| `e04eb67e41` / `31dd8dcae9` | the separator and doc are written only when the doc has text left | 2 / 2 / 2 |

`package_info.cs` moves position-map lines only, in every arm.

## 2. What the arms found

**The first doc ran on from the file's header (first arm).** The using block sits directly under a file's leading
comments: the license header, a package doc or a file comment. A doc written there with no blank line read as the end
of that header. In go/types `stmt.cs`, `// This file implements typechecking of statements.` ran straight into the
alias's `// Convert go/ast form to local variables.`, and so in all 16 files. `992369a92a` writes the blank line Go has
above the declaration and drops it when the block opens the file.

**A bare blank line where no doc was left (found by the second arm's census).** `blank2:member-boundary` rose 12 → 14 on
every target: an alias of an anonymous struct (`internal/fuzz`'s `CorpusEntry`, net/http's `http2timer`) lifts the struct,
and the lifted struct takes the doc. The alias branch then had no text to write, but wrote its newlines: a bare blank
line in the first arm, two in a row with the separator. `31dd8dcae9` writes nothing when the doc has nothing left.

## 3. Footprint and census, head vs base

| target | .cs | + comment | + blank | code lines changed | `missed:decl-doc` | any other class |
|---|---|---|---|---|---|---|
| windows | 16 | 136 | 27 | 0 | 15 → 2 | unchanged |
| linux | 16 | 136 | 27 | 0 | 12 → 1 | unchanged |
| darwin | 16 | 136 | 27 | 0 | 12 → 1 | unchanged |

Head vs base is the sum of the three arms: the first +136 comment and +15 blank lines (13 above a documented alias that
follows another using line of its file, and the two bare blanks); the second +16, the separator above each file's first doc; the third −4, the two
bare blanks and the two separators that had nothing to set apart. +15 +16 −4 = +27. Code tokens are identical in every changed file of every arm. The files: crypto, database/sql/driver,
debug/buildinfo, encoding/json (stream.go), encoding/xml, go/types (stmt.go), internal/abi (map_select_swiss.go),
internal/fuzz, internal/reflectlite, net/http (h2_bundle.go), os (dir.go, error.go, types.go), plugin, reflect
(map_swiss.go, type.go). `os/dir.cs` at the head:

```diff
 // license that can be found in the LICENSE file.
+
+// A DirEntry is an entry read from a directory
+// (using the [ReadDir] function or a [File.ReadDir] method).
 global using DirEntry = go.io.fs_package.DirEntry;
```

The residual `missed:decl-doc`, read: `crypto/x509/x509.go:1748` (a function-local declaration's doc, every target)
and, on windows, `net/net.go:213` (`Close closes the connection.`, a method's doc). Neither is an alias.

`unicode/utf8/utf8.cs` declares no alias and does not move in any arm.

## 4. Gates

- `go test ./...` in `src/go2cs` green at `992369a92a` and at `31dd8dcae9` (`-timeout 40m`). A run started at
  `9667a7d6df` was stopped by PID when `992369a92a` superseded it.
- CNR emulation (878 behavioral projects without `-comments`, base binary `63f770b2fe`): ARMS AGREE, the same 15-file
  pre-existing drift, at `9667a7d6df` and at `31dd8dcae9`. 0 goldens moved.

## 5. Predictions, scored (each stamped before its arm ran)

| arm | prediction | result |
|---|---|---|
| first `9667a7d6df` (15:23Z) | 5–40 .cs, `package_info.cs` map lines only | MET (16 / 16 / 16) |
| | code tokens identical; comment and blank lines added only, above a `global using` | MET (+136 / +15, none removed) |
| | `missed:decl-doc` falls by 5–40 (W); no misplaced class rises by more than 5 | MET (−13; no other class moves) |
| | CNR: arms agree, 0 goldens | MET (ARMS AGREE, same 15-file drift) |
| second `992369a92a` (16:05Z) | 10–16 .cs, +1 blank line each, nothing else | MET (16, +16 blank) |
| | code tokens and comment lines identical | MET |
| | no census class moves by more than 3 | MET in count (+2), but the +2 is the defect in §2, fixed by the third arm |
| third `31dd8dcae9` (16:26Z) | 2 .cs (fuzz.cs, h2_bundle.cs), −2 blank lines each, nothing else | MET |
| | code tokens and comment lines identical | MET |
| | `blank2:member-boundary` 14 → 12; nothing else moves | MET |
| | vs base: `missed:decl-doc` 15/12/12 → 2/1/1, nothing else moves | MET |
| | CNR: arms agree, 0 goldens | MET (ARMS AGREE, same 15-file drift) |

The second arm's CNR was stopped by PID when `31dd8dcae9` superseded its binary; CNR is read at the first arm and at the head.
