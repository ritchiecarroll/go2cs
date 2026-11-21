<!-- sqlite-cs:tests -->
# Tests

This page tracks modernc.org/sqlite's own Go tests, converted to C# and run. Each test package is converted with
`go2cs -tests`, built, and run, and its results are compared, test by test, with native `go test` on the same machine.
The native side runs with the build tags the conversion uses (`safe`, `purego`, `noasm`).

The numbers below are measured on go2cs's development tree, not on a release: the fixes they need are queued for the
next go2cs release and are not in go2cs 1.26.11.1. They are measured on linux/amd64 with Go 1.26.11 and
modernc.org/sqlite v1.62.0.
<!-- Sources: P5 20261120T202932Z (root package validated on a development tree = master + the queued test-pipeline,
     native-array and variadic fixes; linux/amd64) and P5 20261120T183926Z (pcache, vfs, internal/vendorstamp), relayed by
     COORD 20261120T203049Z and 20261120T184032Z. The disclosure is the module disclosure JSON landed in train 1
     (a3f3d0c9ba1, claude/p5-wall-d-manifest). -->

| Package | Native `go test` | Converted | Status |
|:--|:--|:--|:--|
| `modernc.org/sqlite` | 396 pass, 3 skip | 396 pass, 3 skip | **matches** (one test disclosed, below) |
| `modernc.org/sqlite/internal/vendorstamp` | 38 pass | 38 pass | **matches** |
| `modernc.org/sqlite/pcache` | 19 pass | 19 pass | **matches** |
| `modernc.org/sqlite/vfs` | 9 pass | 9 pass | **matches** |

The same three tests skip on both sides on linux/amd64.

**One test is disclosed rather than run.** `TestSEHTruncatedShm` needs a hardware memory fault to be recovered as a Go
panic, which .NET cannot do: the fault ends the process. It is excluded from both sides of the comparison, and the
reason is recorded with the conversion.

**Open walls:** none on linux/amd64 for these four packages, with the queued fixes. Windows has not been run yet.
