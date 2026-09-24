# The O1 survival census -- results (C2, 2026-09-24)

**What this is.** The census that `DESIGN-string-literal-allocation.md` §8.2.3 named "the first census to
run", assigned by COORD after the owner's ruling on `c555d91c55` (ledger "OWNER RULING · c555d91c55").
C1's record of that ruling is §8.4 on `claude/c1-literal-tier-ruling` at `70e010aa46`.

It is a READ of Go source joined to go1.24.13's `-gcflags=-m`. It is not a gate: nothing was converted,
built or run on .NET. The predicates and controls are in [`README.md`](README.md), the raw outputs are
`census-<os>.txt` and `params-<os>.tsv`, and the program is `main.go`.

It is recorded here rather than as a design block because C1's §8.4 is appended at the same place in the
design, on another branch. COORD places the block.

## 1. The answer

The population is the production literal arguments that Go keeps off the heap: a noescape string
parameter of a static callee. It reproduces the ruling's figure exactly:
- windows: **4,835** (2,344, plus 2,491 in format position);
- linux: 4,802 (2,313 + 2,489);
- darwin: 4,803 (2,314 + 2,489).

A site survives when its parameter can be an `sstring` with no copy and no compile error, and so can every
parameter it passes the string on to. The census computes survival three times:
- **S0:** today's golib and converter;
- **S1:** golib's `sstring` gains an enumerator, `copy` and the spread;
- **S2:** as S1, and the converter emits an adapter lambda wherever a flipped function is used as a value.

| surviving noescape literal sites | windows | linux | darwin |
|:--|--:|--:|--:|
| **S0**, total (format position) | 476 (81) | 475 (81) | 474 (81) |
| **S1**, total (format position) | 528 (81) | 527 (81) | 526 (81) |
| **S2**, total (format position) | **2,766 (2,233)** | 2,762 (2,231) | 2,761 (2,231) |
| S2 as a share of the population | 57.2% | 57.5% | 57.5% |
| S2 format position, as a share of the format population | 89.6% | 89.6% | 89.6% |
| format-position share of the S2 survivors | 80.7% | 80.8% | 80.8% |
| `string(b)` arguments surviving, S0 / S1 / S2 (of 84 / 87 / 87 joined) | 0 / 4 / 14 | 0 / 4 / 14 | 0 / 4 / 14 |

**What the numbers say:**
- **Today, O1 reaches about 10% of its population** (476 of 4,835), and only 81 of 2,491 format-position
  literals.
- **The adapter is the lever, not the golib gaps.** Closing the gaps adds 52 sites. The adapter adds
  2,238, almost all of them format position.
  - The reason is small: fmt's own `Errorf`, `Sprintf`, `Fprintf` and `Printf` are each used as a value
    somewhere.
  - `parsenum`, on the path of every format string, is used as a value by `fmt/export_test.go:8`.
  - One value site fixes a delegate signature, and the fixed point then fails everything upstream.
- **At S2, O1 covers 90% of arm B's format-position population.** The 258 format sites that still fail
  are hand-owned callees (105), the cascade (66), binder or reflection reach (44), a closure (30) and a
  composite store (13).
- **`string(b)` gains little:** 14 of 84 joined sites at S2. Of the 70 that fail, 62 bind a parameter
  that Go says leaks.

## 2. Why the rest fails (windows, S2, first reason by precedence)

Of 4,835 sites, 2,069 fail:

| first reason | params | literal sites | of which format |
|:--|--:|--:|--:|
| hand-owned go2cs file (a converter change does not reach it) | 34 | 989 | 105 |
| cascade: passed on to a parameter that fails (roots below) | 58 | 522 | 66 |
| method matching an interface method (binder: exact parameter types) | 5 | 262 | 32 |
| exported method (reflection reach) | 21 | 139 | 12 |
| argument of a generic function | 3 | 44 | 0 |
| converted to an interface (box) | 4 | 33 | 0 |
| captured by a closure | 1 | 30 | 30 |
| map key (no `sstring` map indexer) | 5 | 29 | 0 |
| stored in a composite literal | 1 | 13 | 13 |
| other (2 uses the classifier does not name) | 2 | 7 | 0 |
| passed in a `...string` tail | 1 | 1 | 0 |

- **Hand-owned: `runtime.throw` alone is 726 sites.** It is a whole-file hand-own, and it is also
  captured by a closure and used as a value.
- The others are `testing.(common).Errorf` (77), `reflect.add` (27), `runtime.panicCheck1` (20) and
  `reflect.(Value).FieldByName` (16).
- A hand edit, not a converter change, would reach these sites.

**Cascade roots** (each cascading parameter's chain followed to the first parameter that fails for its own
reason):

| root parameter | its own reason | sites behind it |
|:--|:--|--:|
| `internal/stringslite.Index` #1 | passes the needle to `bytealg.IndexRabinKarp[T string \| []byte]`, a generic function | **248** |
| `strings.(Builder).WriteString` #0 | binder, reflection | 74 |
| `internal/bytealg.IndexString` #0 | no Go body (assembly) | 68 |
| `log.(Logger).Printf` #0 | reflection, closure | 45 |
| `bytes.(Buffer).WriteString` #0 | binder, reflection | 23 |
| `log.Printf` #0 | closure | 21 |
| `runtime.(timer).trace1` #0 | box | 14 |
| `net/textproto.(Conn).Cmd` #0 | reflection | 12 |
| nine more (among them `bytealg.IndexByteString`, assembly, 4) | | 17 |

- **One generic call blocks the strings search family.** `stringslite.Index` sits under `strings.Index`,
  `Contains`, `Cut`, `Count`, `Split` and `Replace`, and it hands its needle to a generic Rabin-Karp.
- Together with the assembly-bodied `bytealg.IndexString` and `IndexByteString` (4), that is **320
  sites**.
- Neither root is a converter change:
  - go2cs supplies `IndexString` in a hand-written companion (internal/bytealg/bytealg_impl.cs:55,
    over the partial declaration in index_native.cs:19);
  - `IndexRabinKarp<T>` is emitted as a generic (bytealg.cs:72);
  - an `sstring` overload of either is a hand change.
- That is the second-largest lever after the adapter. It is not sized further here.

**Every-reason view** (a parameter counts once per reason; windows S2). It shows what a single relaxation
would expose:
- the binder: 8 params, 354 sites;
- reflection reach: 38 params, 547 sites;
- a closure capture: 5 params, 766 sites, mostly `runtime.throw`.

## 3. The callees, and the fmt format-position pilot

**Callees by surviving literal sites** (windows S2, format position in brackets):

| callee | sites |
|:--|--:|
| `fmt.Errorf` | (920) |
| `fmt.Sprintf` | (446) |
| `fmt.Fprintf` | (286) |
| `go/types.(Checker).errorf` | (130) |
| `strings.HasPrefix` | 115 |
| `encoding/gob.errorf` | (64) |
| `strings.HasSuffix` | 48 |
| `go/types.(Checker).sprintf` | (46) |
| `testing/fstest.(fsTester).errorf` | (46) |
| `go/types.(error_).addf` | (44) |
| `fmt.Printf` | (39) |

The wrappers below fmt (go/types, gob, fstest) survive only because the fmt entry they forward to does.

**The fmt format-position pilot the ruling names** (§8.4, step 1). Literals enter fmt through its EXPORTED
entry points:
- `Errorf` 920, `Sprintf` 446, `Fprintf` 286, `Printf` 39, `Appendf` 2, `Sscanf` 1: **1,694 sites directly**;
- their wrappers add most of the other 539 format-position survivors.

A pilot confined to fmt's unexported helpers therefore changes no literal count on its own. The count gain
arrives when the exported format parameters flip. That is a visible signature change in fmt's public
surface, so it is the owner's readability call.

**The parameters the pilot flips** (S2, closed under onward passing; `census-<os>.txt`, PILOT). There are
33 on windows and 31 on linux and darwin, where no production literal reaches `Sscanf`:
- fmt's exported format parameters: `Errorf` #0, `Sprintf` #0, `Fprintf` #1, `Printf` #0, `Appendf` #1;
  on windows also `Sscanf` #1 and, through it, `Fscanf` #1;
- fmt's unexported helpers that survive at S2, 24 parameters (the chain those entries reach is among them):
  - `(pp)`: `doPrintf` #0, `argNumber` #1, `catchPanic` #2, `fmtBytes` #2;
  - package-level: `parseArgNumber` #0, `parsenum` #0, `hasX` #0, `indexRune` #0;
  - `(buffer).writeString` #0;
  - `(fmt)`: `padString` #0, `fmtSbx` #0 and #2, `fmtSx` #0 and #1, `fmtBx` #1, `fmtInteger` #4;
  - `(ss)`: `doScanf` #0, `accept` #0, `advance` #0, `consume` #0, `peek` #0, `okVerb` #1 and #2,
    `scanNumber` #0;
- two exported helpers outside fmt: `unicode/utf8.DecodeRuneInString` #0 and `RuneCountInString` #0.

**The pilot's preconditions, each measured:**
- **The spread.** `(buffer).writeString` does `append(*b, s...)`. Without an `sstring` spread (S0) the
  whole chain fails.
- **Adapters at 8 value sites:**
  - `fmt.Sprintf` ×3: text/template/funcs.go:51 in production, then text/template/parse/parse_test.go:333
    and runtime/import_test.go:35;
  - `fmt.Fprintf` ×2: reflect/all_test.go:3703 and :3709, reflective `Call` and `CallSlice`, which the
    adapter's `@string` signature keeps working;
  - `fmt.Errorf` ×1: fmt/errors_test.go:17;
  - `fmt.Printf` ×1: crypto/internal/fips140test/check_test.go:117. It takes the function's PC, and an
    adapter changes that identity;
  - `fmt.parsenum` ×1: fmt/export_test.go:8.
- **The explicit span → `sstring` operator** (sstring.cs:185). A `"…"u8` literal argument binds an
  `sstring` parameter only through it. It is §8.2R.2's known precondition, with its negative tests.
- Across all of S2: **13 parameters need an adapter, at 26 value sites** (per GOOS). The functions of
  4 of those parameters have a production value site.

## 4. The residual: what `sstring` cannot reach (windows, at S2)

This is what a future Tier 2 would be sized against.

**Production literal arguments with no survivor:**

| population | sites |
|:--|--:|
| a noescape parameter that fails (§2) | 2,069 |
| a parameter Go says leaks to the heap or the result, or has no verdict | 3,248 |
| no parameter to retype: no callee declaration (a func value, or a builtin with a recorded signature) | 2,747 |
| no parameter to retype: a conversion operand | 355 |
| no parameter to retype: a variadic tail | 249 |
| no parameter to retype: a builtin argument with no recorded signature | 100 |
| no parameter to retype: an interface method | 30 |

The 2,069 split into:
- **fixed signature (hand-owned, binder, reflection): 1,390**;
- cascade: 522;
- stored: 42 (map key 29, composite 13);
- boxed: 33;
- other: 82 (generic 44, closure 30, other 7, `...string` 1).

**Production string literals outside call arguments** (R2), which O1 cannot reach by construction:

| context | windows | linux | darwin |
|:--|--:|--:|--:|
| a composite literal element (stored) | 17,881 | 17,758 | 17,565 |
| a comparison operand | 1,843 | 1,893 | 1,891 |
| a concatenation operand | 1,761 | 1,764 | 1,737 |
| returned | 1,182 | 1,094 | 1,097 |
| a switch case | 954 | 968 | 963 |
| assigned to a variable | 820 | 828 | 822 |
| a map key | 112 | 112 | 112 |

- Comparison and switch-case literals already compare against a u8 span without materializing an
  `@string`, so they are not residual.
- A concatenation operand allocates the concatenation, which is G9's territory.
- **The stored, returned and assigned contexts are the residual proper:** about 20,000 literal
  OCCURRENCES. Each costs one counted copy per EVALUATION, and a census of occurrences cannot say how
  often each is evaluated.
- Set aside, because they are not run-time string values: 1,589 in const declarations (27 of them
  `len("…")` arguments), 5,889 import paths and 74 struct tags.
- **Reconciliation, checked:**
  - R2's call-argument rows plus the 27 const-declaration call arguments equal P's literal rows,
    conversions excluded: 11,251 / 11,001 / 10,957;
  - R2's conversion operands equal P's: 355 / 353 / 344.

## 5. Caveats

- **Scope.** Production functions only. Test call sites are not in the population, but test files are
  read for func-value uses, since a test's value use fixes the signature too.
- **The interface-match class over-approximates.** It fires on any interface in the load with the same
  method name and types. The generator's static adapters bridge `@string` to `sstring`
  (InterfaceImplTemplate.cs:294, §8.2R.2), so some binder cases may be recoverable. Not measured.
- **Hand-owned files are flagged, not re-read.** A hand edit could flip them.
- **Returning a view is disqualifying here** (`truncateString`, so `(fmt).fmtS` and `(fmt).fmtQ` fail).
  An `sstring`-returning variant is a larger change, not sized.
- **Survival is a prediction about C#,** read from Go source and golib's `sstring` surface at this tree.
  No flip has been compiled. The pilot's own build is the first read.
- **Linux-only box, but no .NET was used.** The three GOOS rows come from three `-m` inputs and three
  type-checks on one machine.
