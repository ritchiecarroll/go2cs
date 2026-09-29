# DESIGN — 2-index slice bounds through golib methods (S-c R1-A)

**Design only. Nothing here is cut.** Dated 2026-09-28 (i9), read at master 2ff42f7a16. This is the design review
that the slice-bounds sizing (`docs/phase4/briefs/sizing-slice-bounds-2026-09-28.md`, route R1) sent R1-A to. Every
number below was measured for this document: the corpus census (go/packages over `std` with tests, windows / linux /
darwin), two converted probes at master (each case in its own process), and a compile-check of the C# overload rules.

## 1. The problem, measured at master

`convSliceExpr` emits every 2-index slice expression (`x[lo:]`, `x[:hi]`, `x[lo:hi]`) as a C# Range, and
`getRangeIndexer` narrows each non-literal bound with `(int)(x)`: `s[(int)(lo)..(int)(hi)]`. A C# Range cannot
carry a negative bound, since `System.Index` throws in the conversion before any golib code runs. It also cannot
carry a bound past int32, since the `(int)` truncates first.

Probe R1aShapes has 29 cases covering slice, named slice, array, pointer-to-array, named array, string, named string
and string literal, with signed, int64, uint32 and uint64 bounds. At master, 28 of the 29 differ from `go run`, in
four classes:

| class | n | what happens | cases (Go's text in brackets) |
|---|---|---|---|
| A. escapes `recover()` | 10 | CLR `ArgumentOutOfRangeException`, process exits | negative bound on slice, named slice, array, string (`[:-1]`, `[-1:]`); `s[:u]` with `u = 1<<64-1` (`(int)` gives -1); any out-of-range bound on a string LITERAL (`"abc"u8[..(int)(i)]` is a `ReadOnlySpan<byte>` Range) |
| B. silent wrong result | 2 | no panic, `len 5` | `s[:big]` and `s[:i64]` with the value `1<<32+5` (`[:4294967301] with capacity 10`) |
| C. wrong text | 5 | a Go panic with the wrong words | `s[big:]` prints `[5:3]` (Go `[4294967301:3]`); `str[:big]` prints `[:5] with length 3`; array, pointer-to-array and named array past their length print `with capacity 3` where Go prints `with length 3` |
| D. wrong panic type | 11 | Go's text, but `recover()` yields a string, not a `runtime.Error` | every remaining case: slice past cap, `[4:2]`, `[4:]`, `uint32` past cap, and the string cases golib already checks (G1) |

Class B also reaches CONSTANT bounds (probe R1aConst, `s[:c]` with `c = 1<<32+5`, which Go compiles for a slice and
panics on at run time):
- a named constant (`s[..(int)(big)]`) compiles and runs silently, `<nil>`;
- so does a constant expression (`s[..(int)(unchecked((nint)(4294967301L)))]`);
- only a bare int literal fails, and it fails to COMPILE (CS0029, `nint` to `System.Index`).

Class C's array text is wrong for ANY value past the length, not only for extreme ones: `array<T>.this[Range]`
reaches the `slice<T>` constructor's check, which reports a capacity. Class D covers every slice-bounds panic golib
raises. `RuntimeErrorPanic.SliceBoundsOutOfRange` and (at master) `StringSliceBoundsOutOfRange` return a plain string
value. Only the index panics go through `BoundsErrorValue`, Go's own `runtime.boundsError`.

**Adjacent defect in the 3-index form (probe R1aSentinel, 5 of 5 silent).** `s[lo:hi:max]` is emitted as
`s.slice(lo, hi, max)`. golib's `slice` family uses `-1` for an omitted bound
(`slice(this in slice<T>, nint low = -1, nint high = -1, nint max = -1)`), so a RUNTIME `-1` is read as "omitted".
`s[neg:2:5]` gives `len 2 cap 5` (Go `[-1::]`), `s[0:neg:5]` gives `len 3 cap 5` (Go `[:-1:]`), and `s[0:2:neg]`
gives `len 2 cap 10` (Go `[::-1]`). The array overload does the same. §6 folds the fix into R1-A, because R1-A adds
overloads to this same family.

**Side finding, not in scope.** A string literal sliced into an interface (`var x any = "abc"[:i]`) fails to
compile at master (CS0029, `ReadOnlySpan<byte>` to `object`), with constant bounds too. It is a separate emission
defect.

## 2. Footprint, re-measured by expression

The sizing counted BOUNDS. A rewrite touches EXPRESSIONS, so this census counts both. Its control reproduces the
sizing's non-constant bound counts exactly on all three targets:
- slice: 2994+820 / 3009+858 / 3052+825
- string: 1098+262 / 1091+256 / 1087+256

A site is rewritten when at least one bound is non-constant, or is a constant that does not fit int32 (§5). The type-parameter `subslice<S, E>` route (58 prod per
target) is already nint and checked, and is excluded. So is the `(*[N]T)(p)[lo:hi]` pointer-cast form
(7 / 5 / 5 prod), which builds a Span rather than a Range; see §8.

| (prod + test) | windows | linux | darwin |
|---|---|---|---|
| **R1-A sites** | **3477 + 927** | **3484 + 955** | **3522 + 926** |
| files holding them | 529 + 242 | 536 + 252 | 543 + 247 |
| 2-index sites with only constant bounds (unchanged) | 2588 + 998 | 2560 + 1023 | 2609 + 1012 |
| of those, a constant bound past int32 (rewritten too) | 0 + 0 | 0 + 0 | 0 + 0 |
| non-constant bounds: fits nint as-is (int, int8..int32, uint8, uint16) | 3886 + 1047 | 3893 + 1079 | 3940 + 1048 |
| non-constant bounds: int64 | 46 + 18 | 46 + 20 | 46 + 18 |
| non-constant bounds: uint, uint32, uint64, uintptr | 225 + 16 | 225 + 14 | 218 + 14 |
| sites mixing a signed and an unsigned bound | 1 + 1 | 2 + 1 | 2 + 1 |
| 3-index sites with a non-constant bound (§6) | 109 + 15 | 102 + 12 | 100 + 12 |

(The bound rows include the type-parameter bounds, so they exceed the control by those. The bound rows are not
site counts.)

The constant-past-int32 arm is not vacuous. On a fixture holding the probe's three constant forms plus one in-range
constant, it reports 3 of 4.

The windows sites by base (prod + test):
- slice 2083 + 640, named slice 165 + 4;
- array 271 + 41, named array 8 + 1, pointer-to-array 22 + 0;
- string 909 + 236, named string 6 + 0, string literal 1 + 5;
- `string | []byte` type parameter 12 + 0.

By form, over all 3542 + 927 windows sites with a non-constant bound (the two excluded routes included):
`lo:` 1474 + 223, `:hi` 1167 + 440, `lo:hi` 901 + 264.

So R1-A is about 4,400 expressions in about 530 production files per target, not "about 5,200 sites". It is still
the largest emission change of the migration.

## 3. Options

- **R1-A (recommended):** a site with a non-constant bound calls a golib method with Go's checks, Go's codes and
  Go's value. Constant-only sites keep the Range.
  - It removes classes A and B, and class C's two truncated texts, at the rewritten sites.
  - Its golib half removes class D and class C's three array texts everywhere, including the constant-only Range
    sites, because they share the panic builders.
- **R1-B:** keep the Range and wrap each non-constant bound in a checking `GoIndex(x)`.
  - The footprint is the same.
  - The wrapper cannot know the length or capacity, so class A turns into class C.
- **R1-C:** golib only.
  - It fixes C and D.
  - It cannot reach A or B, which happen in the caller's conversion.

R1-A is proposed as two commits in one seat. Commit 1 is golib and the generator only (§4 and §6), with zero emission
change. Commit 2 is the converter plus the corpus (§5). Commit 1 alone already clears class D and class C's three
array texts, and can be gated by GolibTests before a single corpus file moves. Class C's two truncated texts
(`[5:3]`, `[:5] with length 3`) are made by the `(int)` in the caller, so only commit 2 reaches them.

## 4. golib and generator API

All the new methods take `nint` bounds and have NO sentinel.

**Arity forms:**
- `x.slice(lo)` for `x[lo:]`;
- `x.slice(lo, hi)` for `x[lo:hi]`;
- `x[:hi]` is `x.slice(0, hi)`, because Go's omitted low IS 0.

**Receivers and return types:**

| receiver | form | returns | Go's high check |
|---|---|---|---|
| `slice<T>` | extension; CS0542 forbids an instance member named `slice` in `slice<T>` | `slice<T>` | against cap (Acap) |
| `array<T>` (array, pointer-to-array via `(~p)`, deref-aliased `ref array<T>`) | extension, next to the existing one | `slice<T>` | against length (Alen) |
| `@string` | instance method | `@string` | length (Alen) |
| `sstring` (bodies of the 29 registered sstring twins, fmt and unicode/utf8) | instance method | `sstring` | length (Alen) |
| `ReadOnlySpan<byte>` (a string literal, `"abc"u8`) | extension | `ReadOnlySpan<byte>` | length (Alen) |
| `IByteSeq<TSelf, T>` (`string \| []byte` type parameter) | interface members; `slice<T>` implements them explicitly | `TSelf` | per the implementer |
| go2cs-gen named slice (ISliceTypeTemplate) | instance forwarder | the wrapper (Go: `nat[a:b]` is a `nat`) | cap |
| go2cs-gen named array (IArrayTypeTemplate) | instance forwarder to `Value` | `slice<T>` | length |
| go2cs-gen named string (the string surface in InheritedTypeTemplate) | instance forwarder | the wrapper | length |

**Checks, in Go's order.** First high, then low:
- High: `0 <= hi <= cap` (a slice) or `<= len` (the others), one unsigned compare, so a negative hi fails here.
- Low: `0 <= lo <= hi`; the `lo:` form compares against len, so a negative lo fails here.

**Panic value.** On failure the method raises `BoundsErrorValue(x, y, signed, code)` with Go's codes:
`boundsSliceAlen` 1, `boundsSliceAcap` 2, `boundsSliceB` 3. The Go runtime's own `boundsError.Error()` then
formats the text, including the negative forms `[:-1]` and `[-1:]`. The fallback string used when the hook is not
registered must match. Today's builders format `(0, -1)` as `[0:-1]`, where Go prints `[:-1]`. They also have no
length form for arrays (class C). Both builders are corrected in commit 1, and the Range indexers route through the
same builders.

**Overload facts, compile-checked (net10.0 scratch project):**
- An exact-arity `slice(lo)` / `slice(lo, hi)` overload beats the optional-parameter method: the tie-break prefers
  the candidate that needs no default arguments. `slice()` and the 3-argument call still bind the old method.
- The converted corpus calls `.slice(` 13,006 times: 12,868 with no argument (composite literals, variadic packs)
  and 138 with three. None pass one or two arguments.
  - So no existing converted call changes meaning.
  - The only 2-argument callers are golib's `GoReflect.sliceWindow`, two calls whose bounds `reflect.Value.Slice`
    has already checked.
- An explicit interface member named `slice` inside `slice<T>` compiles (no CS0542). This is the `IByteSeq` route.
- `uint`, `uint32`, `ulong` and `long` do not convert implicitly to `nint` (CS0266). A mixed `(nint, ulong)`
  argument pair against `(nint, nint)` + `(ulong, ulong)` fails LOUDLY with CS1503, never silently.
- No Go named slice, string or array type in GOROOT declares a method named `slice`. The only `slice` methods are on
  a struct (`index/suffixarray.ints`) and on unrelated pointer receivers. So the wrapper forwarders cannot shadow a
  Go method. The cut re-runs this check.

**Unsigned bounds (225 / 225 / 218 prod), the reviewer's choice:**
- **U1 (recommended):** `castWideIntegerToNint`, the 3-index rule C1 shipped: an int64 or wide unsigned bound takes
  `(nint)`.
  - It is exact below 2^63.
  - An unsigned bound at or above 2^63 still panics, but with a negative number in the text
    (`[:-1]` for Go's `[:18446744073709551615] with capacity 10`).
  - One rule for every slice bound, and no new overloads.
- **U2:** add `ulong` overloads (signed=false in the value), which give the exact text.
  - Cost: one or two more overloads per receiver row above.
  - The 1–2 mixed prod sites per target would still take `(nint)`.

## 5. Converter emission

The change is in `convSliceExpr`: the four 2-index branches and the `string | []byte` type-parameter branch. If any
present bound is non-constant, or is a constant whose value does not fit int32, the site uses the method form.
Otherwise it keeps its Range, byte for byte.

```
// Go                     master                              R1-A
s[i:j]                    s[(int)(i)..(int)(j)]               s.slice(i, j)
s[:n]                     s[..(int)(n)]                       s.slice(0, n)
str[i:]                   str[(int)(i)..]                     str.slice(i)
p[:n]      (p *[4]T)      (~p)[..(int)(n)]                    (~p).slice(0, n)
s[i:5]                    s[(int)(i)..5]                      s.slice(i, 5)
s[:u]      (u uint64)     s[..(int)(u)]                       s.slice(0, (nint)(u))        (U1)
s[2:]                     s[2..]                              unchanged (constant, fits int32)
s[:big]    (const 1<<32+5) s[..(int)(big)]                    s.slice(0, big)
```

- Bounds render through `castWideIntegerToNint`, not `getRangeIndexer`. `getRangeIndexer` stays for the Range
  sites and for the pointer-cast Span path.
- The receiver rendering (`(~p)`, `.Value` on a named array, a deref-aliased parameter) is unchanged: the method is
  called on exactly the expression the Range was applied to.

**Why the remaining constant sites may keep the Range.** Go rejects a negative constant bound. A constant within
int32 survives the `(int)` exactly. So a remaining Range can only fail with a value inside int32, which the Range
indexers check through the corrected builders (classes C and D). A constant past int32 does NOT stay safe under the
Range, as §1 measured, which is why it is routed to the method. There are none in the corpus today, and the same rule
turns the int literal's compile failure into a call.

## 6. The 3-index sentinel, folded in

The fix replaces the optional-parameter method with explicit arities in the same family:
- `slice()` is the whole value;
- `slice(lo)`, `slice(lo, hi)` and `slice(lo, hi, max)` have no sentinel;
- this applies to the `slice<T>`, `T[]`, `Span<T>`, `IEnumerable<T>`, `array<T>` and `@string` overloads.

A 3-parameter overload cannot sit next to the optional method; that pair is CS0111. The converter then emits `0` for
the omitted low of `s[:h:max]`, where it emits `-1` today. The tracked corpus holds 62 `.slice(-1,` calls in 32 files.
Every one of those files has its Go source in GOROOT, so they are converter output, and the sampled lines are the
converter's `[:h:max]` form (`bytes.go`'s `s[: m+sepSave : m+sepSave]` is `s.slice(-1, m + sepSave, m + sepSave)`).
A `-1` left behind anywhere after the change fails LOUDLY (`[-1::]`), not silently.

The footprint is the `[:h:max]` subset of the 124 / 114 / 112 3-index sites in §2. The rest of the 3-index emission
does not move.

## 7. Performance

This is not measured, and it is not claimed.
- The Range path builds two `System.Index` values (sign-bit encoded), resolves them, and calls `Reslice`.
- The method path calls `Reslice` directly, with the same checks.

The expectation is neutral or better. The cut measures it:
- a Release micro-benchmark of `s[i:j]` and `str[i:j]`, Range against method, tiered and TC0;
- the canary wall.

Both readings go in the seat message.

## 8. Gates at the cut, residuals, interactions

**Gates at the cut:**
- A red first: a behavioral project holding the R1aShapes and R1aSentinel cases in ONE process. At master it dies
  on the first class-A case; after the fix every case recovers with Go's text and `runtime.Error`.
- GolibTests for every receiver × form × failure (text, code, signed).
- Converter unit tests.
- GenTests for the three templates.
- CNR re-baselined.
- The two-seeded three-target `-stdlib` footprint, predicted from the §2 file sets (529 / 536 / 543 prod) and
  reconciled in both directions.
- The corpus applied by the hunk rule.
- The `-tests` hosts for the test sites.
- GolibTests on windows and on a fresh linux tree.
- The full suite at -m:4.

**Residuals:**
- U1's unsigned-at-or-above-2^63 text.
- The `(*[N]T)(p)[lo:hi]` pointer-cast form (7 / 5 / 5 prod). It is unsafe code, and it still passes a negative or
  huge bound to the `ReadOnlySpan` constructor. Go checks hi against N there; this design does not.
- The CS0029 string-literal-to-interface defect (§1).
- `sslice` keeps its Range: the converter never emits `sslice` (0 corpus files).

**Interactions:**
- TRAIN I's slice-bounds follow-ups (5aaa2f6be5) already move `StringSliceBoundsOutOfRange` onto `BoundsErrorValue`
  with codes 1 and 3. Commit 1 extends that to the slice and array builders and to code 2. So the cut's base must
  contain it: TRAIN I, or its union.
- The type-parameter `subslice<S, E>` / `subslice3` route (58 prod) is already sentinel-free and does not move.
