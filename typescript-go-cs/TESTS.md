<!-- typescript-go-cs:tests -->
# Tests

This page tracks TypeScript's own Go tests, converted to C# and run. It has two parts: the unit tests of four
compiler packages, run through go2cs's test pipeline, and the compiler conformance suite, run by hand.

## Unit tests

Each test package is converted with `go2cs -tests -recurse`, built, and run, and its results are compared, test by
test, with native `go test` on the same machine. The module is trimmed to the packages these four test packages need.
Both sides are built with the tags `purego`, `math_big_pure_go`, `safe` and `noasm`. A count is every result
`go test` reports, tests and subtests together.

The numbers below are measured on go2cs master at `4a56f17a3a`, not on a release: the fixes they need are newer than
go2cs 1.26.11.1, and the release itself is not measured here. They are measured on linux/amd64 with Go 1.26.11, the
.NET 10 SDK, at TypeScript commit `ba29a562991ceb942481e2a00810b4e5e4062e52`.
<!-- Source: P5 20261127 refresh (COORD 20261127T134057Z). go2cs master 4a56f17a3a (train 18). Converted side:
     go2cs -tests -recurse over the trimmed module (the four test packages and the module packages they need), run in
     two phases on one output root, -test-action convert and then -test-action compare, so that the escape probe's
     build cache could be removed between them for disk space; the compare phase reconverts nothing. Each package's
     go2cs_test_comparison.json reads status validated, matched true, errors 0. Native side: go test -json -count=1
     -tags purego,math_big_pure_go,safe,noasm -run '^Test' per package on the same box. Exclusions are benchmarks and
     the one fuzz target, not disclosures: ast 7, checker 1, parser 2 (BenchmarkParse, FuzzParser), scanner 0.
     Earlier reading, on a development tree before these fixes landed: P1 20261120T215925Z, relayed by COORD
     20261120T215954Z (same counts in all four packages); platform and counting level P1 20261121T081908Z. -->

| Package | Native `go test` | Converted, at master `4a56f17a3a` | Status |
|:--|:--|:--|:--|
| `internal/scanner` | 16 pass | 16 pass | **matches** |
| `internal/parser` | 7 pass | 7 pass | **matches** |
| `internal/ast` | 552 pass | 552 pass | **matches** |
| `internal/checker` | 2 pass | 2 pass | **matches** |

**Not compared.** Benchmarks in `ast`, `checker` and `parser` run on neither side. A plain native `go test` of
`parser` also runs the seed corpus of the fuzz target `FuzzParser`, and the table leaves it out: go2cs does not run
benchmarks or fuzz targets in converted tests yet. No test is disclosed.

**Open walls:** none for these four packages at go2cs master.

## Compiler conformance suite

TypeScript's compiler and conformance baseline test (`TestLocal`) is converted and run by hand: the converted test
host is published and run directly, and its verdicts are compared with native ones by a separate script, not by
go2cs's test pipeline.

This is measured on a go2cs development tree from 2026-11-21 that carried one fix not in any release, the `sync.Pool`
cleanup fix, which is now on go2cs master. It is measured on linux/amd64 with 4 cores, Release build. It is not
re-run at master `4a56f17a3a`.
<!-- Source: P5 20261121T034525Z (not re-run in the 20261127 refresh: budget; the fix, 4e855b3716, is an ancestor of
     master 4a56f17a3a); record DATA-ts-conformance-scout.md section 9 on claude/p5-ts-conformance-scout
     943a751d38; relayed by COORD 20261121T034700Z. The queued fix is P5's sync.Pool cleanup seat. -->

| | Native | Converted |
|:--|:--|:--|
| Configurations | 15,164 (13,444 pass, 1,720 skip) | 15,164 (13,444 pass, 1,720 skip) |
| Baseline subtests | 119,720 (119,712 pass, 8 skip) | 119,720 (119,712 pass, 8 skip) |
| Differences from native | | **0** |
| Wall time | 85 s | 2,644 s |

No configuration runs out of memory in the converted run.

**One harness step.** The run sets `TSC_REPO_ROOT` to the TypeScript checkout. The test finds its baselines from its
own source location, and in converted code `runtime.Caller` reports module paths rather than file-system paths.
