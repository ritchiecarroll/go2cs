# The whole-array reassignment census (R, 2026-11-15)

**What it counts.** P5's finding (2026-11-15): in Go an array variable is its storage, so `a = b` copies elements INTO
`a`, and a slice of `a`, `&a[i]` or `a`'s address taken earlier still names `a` afterwards. The converted code holds an
array as `array<T>`, a reference to a backing, and a whole assignment swaps that backing: `a = b.Clone()`,
`z.buf = new array<byte>(10){…}`, `globl8 = new byte[]{…}.array()`. A struct is replaced whole with its arrays cloned
(`e = e.ΔClone()`). So whatever was taken from the destination BEFORE the assignment keeps the old elements. This census
is step 1, sizing the cut (step 2: an in-place copy into the destination's own backing).

**The tool.** `reassigncensus-main.go` (go/packages + go/types). It finds every `=` whose left side is an existing
location whose type is an array (`ARRAY`), or a struct holding an array by value at any depth (`STRUCT`). The left side
of a `:=`, a `var` declaration, a range clause, an argument, a return or a map value is new storage in Go too, and is
not counted. Each site is one TSV row: `target  package  file:line  kind  dest  alias`.

| Column | Values |
|---|---|
| `dest` | `LOCAL`, `PARAM`, `CAPTURED` (used by a closure), `PKGVAR`, `FIELD` (`x.f`), `ELEM` (`x[i]`), `DEREF` (`*p`), `OTHER` |
| `alias` (LOCAL and PARAM) | `HELD-BEFORE`: the function stores an alias of the variable (`x[:]`, `&x`, `&x[i]`, also through a conversion such as `unsafe.Pointer(&x[0])`) before the assignment. `HELD-LOOP`: only after it, but inside a loop that holds the assignment. `HELD-AFTER`: only after it, outside such a loop. `TRANSIENT`: an alias used only within a statement (a call argument, an operand). `NONE`: no alias at all, so the clone and an in-place copy cannot be told apart |
| `alias` (every other dest) | `UNKNOWN`: the location is reachable elsewhere, so an alias can be taken where this function cannot see |

**Positive control: P5's two kits** (`claude/p5-array-door-a` 996ce6c9bf, `docs/phase4/data/array-pointer-doors/door-a-1115/`
`reassign-plain/` and `reassign/`). Every whole assignment in both is found: 7 in the plain kit, 9 in the unsafe kit.
Every line P5 measured as differing from Go reads `HELD-BEFORE` (the locals) or `UNKNOWN` (the field, the package
variable, the deref). Two instrument holes, found by this control and fixed before the readings: an alias stored
through a conversion read `TRANSIENT`; and an alias stored after the assignment read as one stored before, now
`HELD-AFTER`. `readings/control-*.tsv`.

## Readings: the standard library, Go 1.26.11, the corpus's tags (`purego`, `math_big_pure_go`)

| Target | Sites, production | With tests | UNKNOWN | TRANSIENT | NONE | HELD-AFTER | HELD-BEFORE or LOOP |
|---|---|---|---|---|---|---|---|
| windows/amd64 | 228 | 374 | 162 | 34 | 22 | 10 | **0** |
| linux/amd64 | 230 | 381 | 164 | 34 | 22 | 10 | **0** |
| darwin/amd64 | 228 | 375 | 162 | 34 | 22 | 10 | **0** |
| linux/arm64 | 230 | 379 | 164 | 34 | 22 | 10 | **0** |

(The class columns are production code; the with-tests readings have 0 HELD-BEFORE or LOOP too.) 244 distinct
production sites across the four targets. 0 load errors on every run.

**What the readings say.**
- **The cut's size.** Every site is one emitted assignment line that the cut respells, so about 230 lines per target in
  the production corpus, plus the test emissions (about 145 more). The heaviest packages are
  `crypto/internal/fips140/mlkem` and `mldsa`, `internal/trace`, `runtime` and the vendored `dnsmessage`.
- **Reach in the standard library.** No local or parameter in the standard library stores an alias before a whole
  assignment, or inside a loop that holds one. The 10 `HELD-AFTER` sites (`dnsmessage`'s `rb, err = …; r = &rb`, and
  `drbg`'s `seed, err = …` with `return &seed` after the loop) take the alias after. So a divergence in the standard
  library could only come from an `UNKNOWN` destination (a field, an element, a deref, a package variable) whose alias
  is taken elsewhere and is still live. That is not decidable here. The cut is correct for those either way, and has no
  observable effect on the `NONE` and `TRANSIENT` sites.
- **Not read here: the demo closures.** sqlite-cs is G's and TypeScript is P4's. Run it with `run.sh` (below).

## Running it

```
bash docs/phase4/data/array-reassign-census/run.sh <scratch dir> <out dir>                       # std, four targets
GOOS=linux GOARCH=amd64 bash docs/phase4/data/array-reassign-census/run.sh <scratch> <out> <module dir> ./... # a closure
```

It needs Go 1.26.11 and golang.org/x/tools v0.52.0 (src/go2cs's own pin) in the module cache. The tool builds outside
the repository; this record carries its sources under renamed files (`reassigncensus-*`), so the tree holds no stray
Go module.
