# `unicode/utf16` validates; disclosed-divergence generalizes

![go2cs](../images/go2cs-small.png)

> Full text of the July 18, 2026 announcement, condensed in the
> [go2cs News Archive](../NEWS.md#july-18-2026--unicodeutf16-validates-disclosed-divergence-generalizes).

---

**Phase-4 package #5.** [`unicode/utf16`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/unicode/utf16)
validates its own Go test suite in C# — **8 tests agreeing outright** against `go test -json`, plus one
honestly disclosed. The structural twin of the very first validated package (`unicode/utf8`), it round-trips
UTF-16 encode/decode with results checked by `reflect.DeepEqual` — exercised here through the converted
reflection bridge — and all eight correctness tests match verdict for verdict.

Its significance is what the ninth test demonstrates. `TestAllocationsDecode` asserts that `Decode` returns
its `[]rune` with **zero** heap allocations — a result Go reaches only through compiler escape analysis, so
the test guards itself with `testenv.SkipIfOptimizationOff`. The managed runtime provably cannot match it: a
returned `slice<rune>` is always a heap allocation, no matter how the method is written. This is the same
*allocation-model* divergence `bytes` and `strings` disclosed a day earlier — and `unicode/utf16` is the
first package to reuse the [disclosed-divergence manifest](../README.md#try-it-yourself--validate-a-converted-test-suite)
as a **general tool** rather than a two-package special case. Its `go2cs_test_disclosures.json` pins one
`alloc-profile` row by exact failure signature (`"Decode allocated "`), while the separate `TestDecode`
independently proves the decoded output is correct — so the disclosure covers exactly the allocation
profile and nothing else. A mechanism that generalizes cleanly to the next package is a mechanism that was
designed right.

---

*Phase-4 package #5 · `unicode/utf16` 8 + 1 disclosed (alloc-profile) · reproduce from a clone via
[Try it yourself](../README.md#try-it-yourself--validate-a-converted-test-suite)*
