# p2-addr-deref-census -- every `&*p` in Go source, read off the syntax tree (point-in-time record)

The sqlite-cs W9b seat made `&*p` the pointer itself after Go's nil check (BOARD row of 2026-11-15, "the third
form"). Its first census was a text search for `&*` before a name or a parenthesis. It predicted a `-stdlib`
footprint of 0 files and the footprint read 1 line per target, at `runtime/malloc.go:899`, which Go writes
`&(*arenaList)`. This program is the census that sees that spelling. P2, 2026-11-15, Go 1.26.11. Not a gate.

## What it counts

A unary `&` whose operand, with any parentheses peeled, is a dereference. One line per site, `class<TAB>file:line`:

- `bare`: `&*p`, nothing between the two operators;
- `paren`: at least one parenthesis between them, `&(*p)`;
- `-conv` is added when the dereferenced operand is a call, `&*(*T)(x)` (a conversion or a call: the program
  reads syntax only and does not tell them apart);
- `-test` is added for a `_test.go` file.

Directories named `testdata` are skipped, and with `SKIPCMD=1` so is a top-level `cmd`. The totals go to stderr.

```
go build -o addrderef . && SKIPCMD=1 ./addrderef <dir> > census.txt
```

## Read on 2026-11-15

| Tree | Files | Sites | Classes |
|---|---|---|---|
| Go 1.26.11 `src`, outside `cmd` and `testdata` (`census-std.txt`) | 4353 | 16 | 15 `bare-conv`, 1 `paren` |
| `src/tests/Behavioral` at the seat's commit (`census-behavioral.txt`) | 1138 | 10 | 5 `bare`, 5 `bare-conv` |
| modernc.org/libc 1.80.1, every platform file | 1428 | 656 | 656 `bare-conv` |
| modernc.org/sqlite 1.62.0, every platform file | 726 | 117 | 117 `bare-conv` |

No file failed to parse in any of the four.

- **std**: the 15 `bare-conv` sites are on 10 lines of `crypto/internal/boring/sha.go` (build tag
  `boringcrypto`, not in the converted corpus). The one `paren` site is `runtime/malloc.go:899`, the footprint.
- **behavioral**: all 10 are in `AddressOfFieldThroughConversion` (8) and
  `AddressOfFieldThroughConversionNative` (2), the seat's own fixtures. They are the positive control: a census
  that read 0 there would be blind.
- **behavioral, after the miss**: the seat then added the `paren` spelling to `AddressOfFieldThroughConversion`
  (three sites, `main.go:144`, `:148` and `:149`), so the corpus reads 13 sites from that commit on: 5 `bare`,
  5 `bare-conv`, 3 `paren`. `census-behavioral.txt` is the reading before that commit.
- **libc and sqlite**: every site is the form the seat's fixtures carry, and none has a parenthesis between
  the operators.
