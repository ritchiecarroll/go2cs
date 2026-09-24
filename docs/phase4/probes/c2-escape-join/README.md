# c2-escape-join -- the literal and no-copy-idiom census, joined to Go's escape verdicts (point-in-time record)

Input to [`DESIGN-string-literal-allocation.md`](../../DESIGN-string-literal-allocation.md) §8.1R, §8.2
and §8.2R. It is committed so the §8.2R figures can be regenerated (COORD's verification, fix 13).

```
GOOS=<os> GOARCH=amd64 go build -a -gcflags=-m std 2> m-<os>.txt     # go1.24.13, about 137K lines
go build -o c2escapejoin . && GOOS=<os> ./c2escapejoin -m m-<os>.txt > census-<os>.txt
```

**What it counts** (`main.go`), over Go SOURCE (std plus its tests, for the GOOS given):
- **`J`:** string parameters and string-literal arguments, joined to `-m`'s parameter verdicts by
  declaration position.
- **`L`:** format-position and degenerate-slug literal arguments by length. It uses copies of the
  converter's own predicates.
- **`M`:** `m[string(b)]` reads, split by whether the converter's `mapReadTmpStringKey` covers them, and
  composite-key reads.
- **`R`:** `string(x)` of an integer, `[]byte("const")`, and concatenations by operand count, each
  joined to `-m`'s expression verdict. `-m` reports a conversion at its argument and a concatenation at
  its outermost `+`.

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
