<!-- sqlite-cs:tests -->
# Tests

This page tracks modernc.org/sqlite's own Go tests, converted to C# and run. Each test package is converted with
`go2cs -tests`, built, and run, and its results are compared, test by test, with native `go test` on the same machine.
Both sides are built with the tags `purego`, `safe` and `noasm`; the conversion also takes `math_big_pure_go`. A count
is every result `go test` reports, tests and subtests together.

The numbers below are measured on go2cs master at `4a56f17a3a`, not on a release: the fixes they need are newer than
go2cs 1.26.11.1, and the release itself is not measured here. They are measured on linux/amd64 and on windows/amd64,
each with Go 1.26.11, the .NET 10 SDK, a Release build, and modernc.org/sqlite v1.62.0.

## linux/amd64

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

## windows/amd64

<!-- Source: i9 20261128T000538Z (full run) and 20261127T220455Z (first reading), for COORD 20261127T201951Z.
     go2cs master 4a56f17a3a (train 18), Go 1.26.11 windows/amd64, .NET SDK 10.0.404, Release (tiered JIT off).
     Converted side: go2cs -tests -recurse -test-action all -test-timeout 30m over a copy of
     modernc.org/sqlite@v1.62.0, -module-disclosures <go2cs>/src/tests/ModuleDisclosures, -module-version v1.62.0,
     default tags purego,math_big_pure_go,safe,noasm, one fresh output root. internal/vendorstamp, pcache and vfs read
     status validated, matched true, errors 0; the root reads failing with one divergence, TestVec (Go pass, C# fail).
     Native side: go test -json -count=1 -tags safe,purego,noasm per package on the same box. The plain native root run
     reports 41 more verdicts than the comparison, the seed-corpus inputs of FuzzSingleStatement.
     Disclosed 0: the TestSEHTruncatedShm entry is linux-only, and on windows the test skips itself on both sides.
     Four fewer verdicts per side than linux: go list selects ofd_linux_test.go on linux (TestOFDLockSurvivesOSClose,
     TestOFDLockInterleavedReadersWrite, TestOFDLockReadOnlyFirstLocker, TestOFDLockFailedFirstLock,
     TestOFDLockingTooLate, TestOFDLockingSetter) and ofd_other_test.go on windows (TestOFDLockingUnavailable), and
     TestSEHTruncatedShm is a counted skip here: 396 - 6 + 1 = 391 pass, 3 + 1 = 4 skip.
     Deadline: the default -test-timeout 2m stops both sides of the root package on this box, and 10m stops the
     converted side at 313 verdicts (its top-level tests take about 752 s); 30m completes both sides.
     TestVec: every vector reads as zeros in the converted run (vec_to_json, hex(vec_f32(...)) and every distance
     read 0). sqlite-vec's windows build parses numbers with libc.X__mingw_strtod, which modernc.org/libc v1.80.1
     forwards to Xstrtod; on windows/amd64 that calls msvcrt strtod through syscall and reads the double from r2.
     Go's runtime fills r2 from XMM0 (internal/runtime/syscall/windows/asm_windows_amd64.s); go2cs's trampoline
     (src/core/syscall/windows/dll_windows.cs) returns r2 = 0. i9 20261127T222311Z, correction 20261127T222331Z. -->

| Package | Native `go test` | Converted, at master `4a56f17a3a` | Status |
|:--|:--|:--|:--|
| `modernc.org/sqlite` | 391 pass, 4 skip | 390 pass, 1 fail, 4 skip | **one test fails**: `TestVec` (known issue, below) |
| `modernc.org/sqlite/internal/vendorstamp` | 38 pass | 38 pass | **matches** |
| `modernc.org/sqlite/pcache` | 19 pass | 19 pass | **matches** |
| `modernc.org/sqlite/vfs` | 9 pass | 9 pass | **matches** |

On the Windows machine measured here, go2cs's default package deadline stops both sides of the root package, native
`go test` included, and a 10-minute deadline still cuts the converted side short. The Windows run passes
`-test-timeout 30m`.

## Notes

The native column counts the tests both sides run. A plain native `go test` of the root package also reports the
seed inputs of the fuzz target `FuzzSingleStatement`, and on Linux the disclosed test.

The same tests skip on both sides on each platform. On Windows, `TestSEHTruncatedShm` skips itself on both sides,
because a file that is mapped cannot be truncated there.

The two platforms run slightly different tests: the root package's OFD-lock tests are Linux-only, and other platforms
run one test in their place.

**`TestSEHTruncatedShm` is disclosed rather than run.** It needs a hardware memory fault to be recovered as a Go
panic, which .NET cannot do: the fault ends the process. It is withdrawn from both sides of the comparison on Linux,
and the reason is recorded with the conversion.

**Known issue on Windows: `TestVec` fails.** In the converted program, a floating-point value that a C runtime function
returns through Go's syscall package reads as 0 on Windows (native Go reads it correctly), so sqlite-vec's vector values
are lost and TestVec fails. A fix is in progress. The sqlite-vec extension parses each number in a vector with the C
runtime's `strtod`, so on Windows every vector reads as zeros, and the test's nearest-neighbour query returns its rows
in the wrong order. On Linux the numbers are parsed without that call, and the test matches.

**Not compared.** Benchmarks, in the root package and in `pcache`, run on neither side. Native `go test` runs the
seed-corpus inputs of the fuzz target `FuzzSingleStatement`, and the tables leave them out: go2cs does not run
benchmarks or fuzz targets in converted tests yet.

**Open walls:** on linux/amd64, none for these four packages at go2cs master, besides the disclosed test above. On
windows/amd64, `TestVec`, above. macOS has not been run.
