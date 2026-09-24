# c2-escape-join -- the literal and no-copy-idiom census, joined to Go's escape verdicts (point-in-time record)

Input to [`DESIGN-string-literal-allocation.md`](../../DESIGN-string-literal-allocation.md) §8.1R, §8.2
and §8.2R. It is committed so the §8.2R figures can be regenerated (COORD's verification, fix 13).

```
GOOS=<os> GOARCH=amd64 go build -a -gcflags=-m std 2> m-<os>.txt     # go1.24.13, about 137K lines
go build -o c2escapejoin . && GOOS=<os> ./c2escapejoin -m m-<os>.txt > census-<os>.txt
```

Run both commands with `GOTOOLCHAIN=go1.24.13`: `-m` prints absolute GOROOT paths, and the join matches
them against the GOROOT the program loads, so the two must be the same toolchain. *(2026-09-24, revision 4:
the census files committed in the WIP save `d29c5e3542` came from a run whose joins all failed, so every
verdict row read `no-verdict`. They were regenerated at the revision-4 commit from the same inputs.)*

`-poskey compiler` (the default) keys a conversion's verdict at its operand's COMPILER position: an
operator for a binary or unary operand, `[` for an index or slice, `(` for a call, `.` for a selector.
This is G's finding 4 (`DESIGN-nonescaping-locals.md` on `claude/g-rec-b-oracle` at `814603bbbb`).
`-poskey ast` keys at `ast.Node.Pos()`, as revision 3 did. The two differ only in `R1`: 4 production
`string(rune/int)` sites per GOOS move from no-verdict to 2 heap and 2 noescape.

A verdict is taken only from a `-m` line whose printed text is the construct being joined (`string(…)`
for R1, `[]byte` or the zero-copy line for R5, ` + ` for R4). An inlined callee's allocation is printed at
the caller's call position (G's finding 2), which is also a call operand's compiler position, so without
the check a key could take another expression's verdict. A key that holds only other expressions' lines
is counted in its own `no-verdict, of which …` row; at this commit no such row appears on any GOOS, and the
output is identical with and without the check.

**The UTF-16 carve-out** (`utf16-tuple-sites.py`, run from `src/core`; output `utf16-tuple-sites.txt`): the
non-empty string literals that are whole elements of a tuple return or a tuple assignment. They are emitted
as UTF-16 C# literals, so a table keyed on u8 addresses cannot serve them. It is a lower bound: a literal
that is an operand inside an element is not counted.

**What it counts** (`main.go`), over Go SOURCE (std plus its tests, for the GOOS given):
- **`J`:** string parameters and string-literal arguments, joined to `-m`'s parameter verdicts by
  declaration position.
- **`L`:** format-position and degenerate-slug literal arguments by length. It uses copies of the
  converter's own predicates.
- **`M`:** `m[string(b)]` reads, split by whether the converter's `mapReadTmpStringKey` covers them, and
  composite-key reads.
- **`R`:** `string(x)` of an integer, `[]byte("const")`, and concatenations by operand count, each
  joined to `-m`'s expression verdict. `-m` reports a conversion at its operand's compiler position and a
  concatenation at its outermost `+`. `[]byte("const")` has a fourth verdict, `zero-copy`: Go 1.22+ prints
  `zero-copy string->[]byte conversion` for a read-only one.
- **`I`:** every `string([]byte)` conversion, by what consumes it: a map index (I1), a comparison (I2), a
  concatenation (I3), a switch tag (I5), a call argument (I6), a binding (I8), a return (I9), elsewhere
  (I7). §8.2R's G2, G3, G4 and O1 rows are I2, I5, I8 and I6.

**Exclusions are counted, not dropped:**
- a call through a func value, a conversion or a generic instantiation (no callee declaration to join);
- an interface method;
- a variadic-tail argument.

**Caveats:**
- Test files carry no expression verdict, because `-m` over `std` does not compile them.
- A verdict describes Go's function, not a hand-owned go2cs file.

**Files:**
- `census-windows.txt`, `census-linux.txt`, `census-darwin.txt`;
- `m-digest.tsv`: `-m` verdict lines per std package directory for each GOOS. The full outputs are
  regenerated, not committed.
