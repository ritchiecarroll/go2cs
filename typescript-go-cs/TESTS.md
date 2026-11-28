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
go2cs 1.26.11.1, and the release itself is not measured here. They are measured on linux/amd64 and on windows/amd64,
each with Go 1.26.11, the .NET 10 SDK and a Release build (tiered JIT off), at TypeScript commit
`ba29a562991ceb942481e2a00810b4e5e4062e52`.
These four packages need no change to TypeScript's source: none of the tests compared here reads the repository's
test data.

### linux/amd64

<!-- Source: P5 20261127 refresh (COORD 20261127T134057Z). go2cs master 4a56f17a3a (train 18). Converted side:
     go2cs -tests -recurse over the trimmed module (the four test packages and the module packages they need), run in
     two phases on one output root, -test-action convert and then -test-action compare, so that the escape probe's
     build cache could be removed between them for disk space; the compare phase reconverts nothing. Each package's
     go2cs_test_comparison.json reads status validated, matched true, errors 0. Native side: go test -json -count=1
     -tags purego,math_big_pure_go,safe,noasm -run '^Test' per package on the same box. Exclusions are benchmarks and
     the one fuzz target, not disclosures: ast 7, checker 1, parser 2 (BenchmarkParse, FuzzParser), scanner 0.
     Harness patch check (COORD 20261127T193818Z item 1): the only callers of internal/repo in the four packages are
     BenchmarkParse, FuzzParser and BenchmarkNewChecker, all excluded; the module copy's internal/repo/paths.go was
     not patched for this run.
     Earlier reading, on a development tree before these fixes landed: P1 20261120T215925Z, relayed by COORD
     20261120T215954Z (same counts in all four packages); platform and counting level P1 20261121T081908Z. -->

| Package | Native `go test` | Converted, at master `4a56f17a3a` | Status |
|:--|:--|:--|:--|
| `internal/scanner` | 16 pass | 16 pass | **matches** |
| `internal/parser` | 7 pass | 7 pass | **matches** |
| `internal/ast` | 552 pass | 552 pass | **matches** |
| `internal/checker` | 2 pass | 2 pass | **matches** |

### windows/amd64

<!-- Source: i9 20261128T004420Z, for COORD 20261127T201951Z. go2cs master 4a56f17a3a (train 18), Go 1.26.11
     windows/amd64, .NET SDK 10.0.404, Release (tiered JIT off), TypeScript ba29a562991ceb942481e2a00810b4e5e4062e52.
     Converted side: a module copy of tsc/ holding _test.go only in the four test packages and trimmed to the 66 module
     packages their tests need (go list -deps -test), with the xxh3 replace to
     <go2cs>/src/forks/github.com/zeebo/xxh3@v1.1.0; go2cs -tests -recurse -test-action all -test-timeout 30m in one
     phase on one fresh output root, default tags purego,math_big_pure_go,safe,noasm. Each package's
     go2cs_test_comparison.json reads status validated, matched true, errors 0, disclosed 0. Native side: go test -json
     -count=1 -tags purego,math_big_pure_go,safe,noasm -run '^Test' per package on the same box. Exclusions as linux:
     ast 7, checker 1, parser 2, scanner 0. internal/repo/paths.go was not patched. The 30m deadline was passed as a
     precaution; whether the 2m default fits was not measured. -->

| Package | Native `go test` | Converted, at master `4a56f17a3a` | Status |
|:--|:--|:--|:--|
| `internal/scanner` | 16 pass | 16 pass | **matches** |
| `internal/parser` | 7 pass | 7 pass | **matches** |
| `internal/ast` | 552 pass | 552 pass | **matches** |
| `internal/checker` | 2 pass | 2 pass | **matches** |

### Notes

**Not compared.** Benchmarks in `ast`, `checker` and `parser` run on neither side. A plain native `go test` of
`parser` also runs the seed corpus of the fuzz target `FuzzParser`, and the tables leave it out: go2cs does not run
benchmarks or fuzz targets in converted tests yet. No test is disclosed.

**Open walls:** none on linux/amd64 or windows/amd64 for these four packages at go2cs master. macOS has not been run.

## Compiler conformance suite

TypeScript's compiler and conformance baseline test (`TestLocal`) is converted and run by hand: the converted test
host is published and run directly, and its verdicts are compared with native ones by a separate script, not by
go2cs's test pipeline.

This is measured on a go2cs development tree from 2026-11-21, neither a release nor master, with the `sync.Pool`
cleanup fix applied; that fix is on go2cs master. It is measured on linux/amd64 with 4 logical processors and a
Release build. It is not re-run at master `4a56f17a3a`.
<!-- Source: P5 20261121T034525Z; record docs/phase4/DATA-ts-conformance-scout.md (section 9) on go2cs master;
     relayed by COORD 20261121T034700Z. The fix is P5's sync.Pool cleanup seat, 4e855b3716, an ancestor of master
     4a56f17a3a. Not re-run in the 20261127 refresh. -->

| | Native | Converted |
|:--|:--|:--|
| Configurations | 15,164 (13,444 pass, 1,720 skip) | 15,164 (13,444 pass, 1,720 skip) |
| Baseline subtests | 119,720 (119,712 pass, 8 skip) | 119,720 (119,712 pass, 8 skip) |
| Differences from native | | **0** |
| Wall time | 85 s | 2,644 s |

The wall times are test time and exclude the build. No configuration
runs out of memory in the converted run.

**One change to TypeScript's source, on both sides.** The test finds its baselines by walking up from its own source
file, which `runtime.Caller` names. In converted code `runtime.Caller` reports a module path, the shape a Go
`-trimpath` build reports, and the test panics on that shape. So the copy of TypeScript used for this run patches
`internal/repo/paths.go`: before that panic, it returns `os.Getenv("TSC_REPO_ROOT")` when the variable is set. That
package is converted again, and the converted run sets `TSC_REPO_ROOT` to the TypeScript checkout. The native run
uses the same patched copy with the variable unset, so it takes its usual path.
The conversion published in this repository does not carry this patch, so
[MODIFICATIONS.md](MODIFICATIONS.md) does not list it.
<!-- Source: docs/phase4/DATA-ts-conformance-scout.md section 3.3 on go2cs master. -->
