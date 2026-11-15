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

## Amendment, 2026-11-15 (R): census v2 for P5's review (C5, C6), and the run.sh build fix

P5's review of the design (`DESIGN-array-reassign-in-place.md` §8) asked for two more censuses: every assignment FORM
that writes an existing location (C5), and every `*p = …` whose `p` is a `(*[N]T)(unsafe.Pointer(…))` result (C6).
`reassigncensus-main.go` is now **v2** (v1 is this file at c41b321558). It appends two columns and leaves the first six
as v1 printed them:

| Column | Values |
|---|---|
| `form` | `SINGLE` (`a = b`), `RECV` (`a = <-ch`), `SELECT` (`case a = <-ch:`), `TUPLE-N` (`a, b = x, y`), `TUPLE-CALL` (`a, b = f()`), `TUPLE-COMMAOK` (`a, ok = m[k]`, `x.(T)`, `<-ch`), `DEFINE-REDECL` (`a, err := f()` with `a` already declared: it assigns `a`), `RANGE` (`for k, v = range`) |
| `door` | for `DEREF` only: `UNSAFE-CONV` (`p` is a `(*[N]T)(unsafe.Pointer(…))` result, directly or through a variable it was stored into), `SLICE-CONV` (a `(*[N]T)(slice)` result, door A's managed window), `OTHER`; `-` for every other dest |

v2 also counts the two forms v1 left out: a redeclared `:=` operand and a range `=` operand. v1's rows are the v2 rows
whose form is neither.

**Control.** `control-v2-forms.go` (run as `main.go` in its own module): every form and every door kind once. 15 rows, each as designed
(`readings/v2/control-v2-forms.tsv`); the `:=` in it counts its redeclared operand and not the new `err`.

**v1 reproduced.** On all eight std readings, the v2 rows minus `DEFINE-REDECL` and `RANGE`, cut to six columns, are
byte-identical to the v1 file (`readings/` beside `readings/v2/`).

| Reading (Go 1.26.11) | SINGLE | TUPLE-CALL | TUPLE-N | RANGE | RECV, SELECT, COMMAOK, DEFINE-REDECL | DEREF: UNSAFE-CONV / OTHER |
|---|---|---|---|---|---|---|
| std windows/amd64 | 196 | 24 | 8 | 0 | 0 | 1 / 30 |
| std linux/amd64 | 198 | 24 | 8 | 0 | 0 | 1 / 28 |
| std darwin/amd64 | 196 | 24 | 8 | 0 | 0 | 1 / 28 |
| std linux/arm64 | 198 | 24 | 8 | 0 | 0 | 1 / 28 |
| std with tests, windows/amd64 | 340 | 26 | 8 | 1 | 0 | 1 / 106 |
| std with tests, linux/amd64 | 347 | 26 | 8 | 1 | 0 | 1 / 106 |
| std with tests, darwin/amd64 | 341 | 26 | 8 | 1 | 0 | 1 / 104 |
| std with tests, linux/arm64 | 345 | 26 | 8 | 1 | 0 | 1 / 105 |
| sqlite v1.62.0 closure, linux/amd64 (12 packages) | 392 | 0 | 1 | 0 | 0 | 69 / 71 |
| sqlite v1.62.0 closure, windows/amd64 (22 packages) | 60 | 0 | 0 | 0 | 0 | 6 / 24 |

- The tuple forms are 28 statements per target in production std, in seven packages (`fips140/drbg`, `mldsa`, `mlkem`,
  `internal/trace/internal/tracev1`, `log/slog`, `net`, vendored `dnsmessage`). The one RANGE is a test
  (`image/jpeg/dct_test.go:19`).
- std's one UNSAFE-CONV is `runtime/mfinal.go:252`, in the hand-owned `mfinal.cs` (unreachable; §4 of the design).
- sqlite's UNSAFE-CONV sites, by `c6class.py`: linux 68 unnamed `*(*[N]T)(…)` and 1 named
  (`*(*uuid.Uuid_t)(unsafe.Pointer(dst)) = …`); windows 6 unnamed. These write libc's heap: native memory, where the
  pointer is a `NativeArrayBox`.
- Paths under the module cache read `$GOMODCACHE/…`; `c6class.py` expands them from `go env GOMODCACHE`.

**run.sh build fix** (found by G): it built the tool under the caller's `GOOS`/`GOARCH`, so a closure run with
`GOOS=linux` set built a linux binary that this Windows host cannot run. It now builds with both unset. Red, then green:
the c41b321558 run.sh under `GOOS=linux GOARCH=amd64` exits 127 (no runnable binary); this one reads the control's 15
rows. `sqlite-closure.sh` is the closure run above, through run.sh.

```
bash docs/phase4/data/array-reassign-census/sqlite-closure.sh <module dir> <scratch dir> <out dir>
```
