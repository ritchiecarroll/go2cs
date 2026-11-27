<!-- sqlite-cs:tests -->
# Tests

This page tracks modernc.org/sqlite's own Go tests, converted to C# and run. Each test package is converted with
`go2cs -tests`, built, and run, and its results are compared, test by test, with native `go test` on the same machine.
Both sides are built with the tags `purego`, `safe` and `noasm`; the conversion also takes `math_big_pure_go`. A count
is every result `go test` reports, tests and subtests together.

The numbers below are measured on go2cs master at `4a56f17a3a`, not on a release: the fixes they need are newer than
go2cs 1.26.11.1, and the release itself is not measured here. They are measured on linux/amd64 with Go 1.26.11, the
.NET 10 SDK, a Release build, and modernc.org/sqlite v1.62.0.
<!-- Source: P5 20261127 refresh (COORD 20261127T134057Z). go2cs master 4a56f17a3a (train 18). Converted side:
     go2cs -tests -recurse -test-action all over a copy of modernc.org/sqlite@v1.62.0 with the module disclosures and
     -module-version v1.62.0; each package's go2cs_test_comparison.json reads status validated, matched true, errors 0.
     Native side: go test -json -count=1 -tags safe,purego,noasm per package on the same box. The native root run reports
     42 more verdicts than the comparison: the 41 seed-corpus inputs of FuzzSingleStatement (fuzz targets are not run in
     converted tests) and the disclosed TestSEHTruncatedShm; every other native verdict is in the comparison and agrees.
     Earlier reading, on a development tree before these fixes landed: P5 20261120T202932Z and 20261120T183926Z
     (same counts in all four packages). -->

| Package | Native `go test` | Converted, at master `4a56f17a3a` | Status |
|:--|:--|:--|:--|
| `modernc.org/sqlite` | 396 pass, 3 skip | 396 pass, 3 skip | **matches** (`TestSEHTruncatedShm` disclosed, below) |
| `modernc.org/sqlite/internal/vendorstamp` | 38 pass | 38 pass | **matches** |
| `modernc.org/sqlite/pcache` | 19 pass | 19 pass | **matches** |
| `modernc.org/sqlite/vfs` | 9 pass | 9 pass | **matches** |

The native column counts the tests both sides run. A plain native `go test` of the root package also reports the
41 seed inputs of the fuzz target `FuzzSingleStatement` and the disclosed test.

The same tests skip on both sides on linux/amd64.

**`TestSEHTruncatedShm` is disclosed rather than run.** It needs a hardware memory fault to be recovered as a Go
panic, which .NET cannot do: the fault ends the process. It is withdrawn from both sides of the comparison on Linux,
and the reason is recorded with the conversion.

**Not compared.** Benchmarks, in the root package and in `pcache`, run on neither side. Native `go test` runs the
seed-corpus inputs of the fuzz target `FuzzSingleStatement`, and the table leaves them out: go2cs does not run
benchmarks or fuzz targets in converted tests yet.

**Open walls:** none on linux/amd64 for these four packages at go2cs master, besides the disclosed test above.
Windows and macOS have not been run.
