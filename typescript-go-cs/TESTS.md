<!-- typescript-go-cs:tests -->
# Tests

This page tracks TypeScript's own Go tests, converted to C# and run. It has two parts: the unit tests of four
compiler packages, run through go2cs's test pipeline, and the compiler conformance suite, run by hand.

## Unit tests

Each test package is converted with `go2cs -tests -recurse`, built, and run, and its results are compared, test by
test, with native `go test` on the same machine. The module is trimmed to the packages these four test packages need.
Both sides are built with the tags `purego`, `math_big_pure_go`, `safe` and `noasm`. A count is every result
`go test` reports, tests and subtests together.

The numbers below are measured on go2cs's development tree, not on a release: the fixes they need are queued for the
next go2cs release and are not in go2cs 1.26.11.1. They are measured on linux/amd64 with Go 1.26.11,
at TypeScript commit `ba29a562991ceb942481e2a00810b4e5e4062e52`.
<!-- Source: P1 20261120T215925Z (development tree = master + CLASS A + F5 05369b812b, queued for the next release; trimmed
     module, -tests -recurse), relayed by COORD 20261120T215954Z. Platform, tags and counting level: P1
     20261121T081908Z (linux/amd64; tests plus subtests, one row per go test -json verdict; the ast 7 and checker 1 are
     excluded benchmarks, not disclosures; 0 disclosed in all four packages). Earlier readings: 20261120T085451Z (all four build red on one test-reference cause) and
     20261120T165030Z (parser 6 of 7, one test did not finish). -->

| Package | Native `go test` | Converted | Status |
|:--|:--|:--|:--|
| `internal/scanner` | 16 pass | 16 pass | **matches** |
| `internal/parser` | 7 pass | 7 pass | **matches** |
| `internal/ast` | 552 pass | 552 pass | **matches** |
| `internal/checker` | 2 pass | 2 pass | **matches** |

**Not run, on either side.** Benchmarks and fuzz targets: seven benchmarks in `ast`, one in `checker`, and in `parser`
`BenchmarkParse` and the fuzz target `FuzzParser` with its 17,422 seed-corpus inputs. go2cs does not run benchmarks or
fuzz targets in converted tests yet. No test is disclosed.

**Open walls:** none for these four packages, with the queued fixes.

## Compiler conformance suite

TypeScript's compiler and conformance baseline test (`TestLocal`) was converted and run by hand: the converted test
host was published and run directly, and its verdicts were compared with native ones by a separate script, not by
go2cs's test pipeline.

This is measured on go2cs's development tree with one queued fix that is not in any release, on linux/amd64 with
4 cores, Release build.
<!-- Source: P5 20261121T034525Z; record DATA-ts-conformance-scout.md section 9 on claude/p5-ts-conformance-scout
     943a751d38; relayed by COORD 20261121T034700Z. The queued fix is P5's sync.Pool cleanup seat. -->

| | Native | Converted |
|:--|:--|:--|
| Configurations | 15,164 (13,444 pass, 1,720 skip) | 15,164 (13,444 pass, 1,720 skip) |
| Baseline subtests | 119,720 (119,712 pass, 8 skip) | 119,720 (119,712 pass, 8 skip) |
| Differences from native | | **0** |
| Wall time | 85 s | 2,644 s |

The converted run ran out of memory in no configuration.

**One harness step.** The run sets `TSC_REPO_ROOT` to the TypeScript checkout. The test finds its baselines from its
own source location, and in converted code `runtime.Caller` reports module paths rather than file-system paths.
