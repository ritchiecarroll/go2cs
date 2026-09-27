# Floating-Point Formatting

[Reference index](README.md) · [Summary of this topic](../ConversionStrategies.md)

Go's default rendering of a float — `%v`, `%g`, and the bare `Println`/`Print`/`Sprint` paths — is
`strconv.FormatFloat(f, 'g', -1, bitSize)`: the shortest decimal digits that round-trip back to the same
float, laid out in `'e'` form when the decimal exponent is **below -4 or at/above 6**, and `'f'` form
otherwise. The exponent is lowercase, always signed, and always at least two digits.

.NET's default/`"R"` formatting *also* produces shortest-round-trip digits, but presents them differently:
it flips to exponent form on its own thresholds and writes an unpadded, uppercase exponent. The two agree
far more often than they disagree, which is what makes the disagreement easy to miss — the gap only opens
at the ends:

```go
fmt.Println(1000000.0)                   // Go: 1e+06        .NET default: 1000000
fmt.Println(1e-5)                        // Go: 1e-05        .NET default: 1E-05
fmt.Println(2.2250738585072014e-308)     // Go: …e-308       .NET default: …E-308
fmt.Println(999999.0)                    // Go: 999999       .NET default: 999999   (agree)
```

The 6 is the whole story for `%v`: it is why `1000000.0` prints as `1e+06` while `999999.0` prints in full,
and it comes from strconv's `formatDigits`, which pins the exponent threshold to a flat **6 whenever the
digits were the shortest round-trip** (`if shortest { eprec = 6 }`) rather than to the requested precision.
The threshold is *not* 21 — that figure appears in an older reading of the rule and matches JavaScript's
`Number.toString`, not Go's.

**The conversion:** the baseline stub `fmt` (`src/core/fmt/format.cs`) reproduces strconv's layout rather
than delegating to .NET's presentation. It leans on an empirical equivalence, verified by differential
fuzzing (below): **.NET and Go produce the same digits**. .NET's `"R"` is the same shortest round-trip, and
its `"E<n>"` rounds the *exact* binary value to `n+1` significant digits with the same round-half-to-even
that strconv's `shouldRoundUp` applies — including denormals, and exactly (not zero-padded) well past 17
digits, so `%.40e` of `0.1` agrees digit-for-digit. Only the *presentation* differs. So `FormatFloat` takes
.NET's digits, reduces them to strconv's `decimalSlice` shape (`DecomposeDigits`: significant digits, sign
and trailing zeros stripped, scaled so the value is `0.<digits> × 10^dp`), and lays them out through direct
ports of strconv's `fmtE` and `fmtF`. Fixed-point (`%f` with a precision) is the one case handed to .NET
whole — `"F<n>"` already matches `fmtF` exactly, and it wants digits at a decimal place rather than a
significant-digit count.

Verb defaults follow Go's `fmt`: `%v` renders as `%g`, `%F` as `%f`; `%e`/`%f` default to a precision of 6
and `%v`/`%g` to the shortest round-trip; an explicit precision overrules either. `float32` resolves its
digits **as a single** (`((float)value).ToString(…)`, never widened to double first), so `float32(1.0/3.0)`
prints Go's `0.33333334` and not the double's `0.3333333333333333`. Two Go quirks that a straight reading
of the rule misses, both caught by the fuzz:

* **`%g` promotes a zero precision to one significant digit — and that promotion feeds the exponent-form
  decision too.** strconv mutates `prec` itself (`case 'g', 'G': if prec == 0 { prec = 1 }`), not merely the
  digit count, so `%.0g` of `0.7` is `0.7`. Applying the promotion only to the digit count leaves the
  threshold test comparing against 0, which rounds the value away to `0`.
* **±Inf carries an inherent sign** (`+Inf`, never `∞`), the space flag demotes its `+` to a space, and
  Inf/NaN space-pad under the `0` flag because they do not look like numbers.

**Verification.** Beyond the fixed cases, a differential fuzz compared the stub against `go run` over
random float64/float32 **bit patterns** (so NaNs, infinities and denormals arise naturally), precisions 0–20
across `%g/%e/%f/%G/%E`, and values clustered on the threshold where the form flips: ~50,000 formatted
values over six seeds, all byte-identical. The `%.0g` promotion above was found this way — the hand-picked
cases all passed without it.

**Scope.** This is the hand-written baseline **stub** `fmt`, the proxy the behavioral corpus builds against.
The full conversion's `fmt` calls the *converted* `strconv`, which is Go's own digit code and needs none of
this. (Guarded by the `FloatFormatExponent` behavioral test — both thresholds from either side, `1e20`/`1e21`,
`1000000.0`, the `math.MaxFloat64`/`SmallestNonzeroFloat64` values, float32 counterparts, negative zero, and
every verb with and without precision, byte-compared against `go run`; ±Inf/NaN flag interactions live in
`PrintfWidthFlags`. The extreme values are spelled as literals because the converted `math` package's
constants are themselves rendered lossily — `MaxFloat64` as `1.79769e+308` — which is a separate converter
defect, deliberately not conflated with this one.)

<a id="a-folded-constant-of-a-named-type-carries-its-type-in-the-fold"></a>Moved to [A folded constant of a NAMED type carries its type in the fold](constants.md#a-folded-constant-of-a-named-type-carries-its-type-in-the-fold).

---

[← Named Numeric Types and Constant Contexts](named-numeric-types.md) · [Index](README.md) · [Nil and Zero Values →](nil-and-zero-values.md)
