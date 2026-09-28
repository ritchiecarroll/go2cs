# `bytes` and `strings` tests pass, with disclosed-divergence

![go2cs](../images/go2cs-small.png)

> Full text of the July 18, 2026 announcement, condensed in the
> [go2cs News Archive](../NEWS.md#july-18-2026--bytes-and-strings-tests-pass-with-disclosed-divergence).

---

**Two more standard-library packages validate their own Go test suites in C#** — and they arrive with a
new piece of Phase-4 machinery. [`bytes`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/bytes)
validates **81 tests** and [`strings`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/strings)
**68** against `go test -json`, bringing the count of Phase-4-validated packages to four (after
`unicode/utf8` and `sort`).

Both packages contain a handful of tests that assert an **exact allocation count** via Go's
`testing.AllocsPerRun` — for example, `strings`'s `TestBuilderAllocs` insists a `Builder` heap-allocates
*exactly once*. These are unsatisfiable by design in a managed runtime: the CLR has no malloc counter (the
shim measures allocated *bytes* instead), and .NET genuinely allocates where Go's escape analysis
stack-allocates — an addressed `var b Builder` heap-boxes per run; `string(r)` materializes a `byte[]`
where Go uses a 4-byte stack buffer. A malloc-counting shim would fail these identically; the divergence
is the allocation *model*, not the measurement.

Rather than silently skip them, go2cs now discloses them at test level. Each affected package carries a
hand-owned, repo-committed `go2cs_test_disclosures.json` — reviewed like source, never generated — that
pins `{test, divergence class, expected failure signature}`. The differential oracle reclassifies a
Go-passes/C#-fails result as **disclosed-divergent** *only* when both the test name and the pinned failure
signature match; a disclosed test that fails any *other* way is still a hard mismatch, so the pin is an
integrity guard, not a blanket exemption. The validation summary reports the reclassified rows explicitly
(`… 7 disclosed-divergent (alloc-profile), …`). Packages without a manifest — `sort` and `utf8` —
compare strictly and are wholly unaffected.

---

*Phase-4 packages #3 and #4 · `sort` 63/63, `bytes` 81, `strings` 68 · reproduce from a clone via
[Try it yourself](../README.md#try-it-yourself--validate-a-converted-test-suite)*
