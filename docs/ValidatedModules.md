# Validated Modules

Go modules whose own `_test.go` suites go2cs converted with `go2cs -tests -recurse`,
built against the converted standard library, ran under the Go-semantics test host, and compared
verdict for verdict against a clean `go test -json` baseline of the same sources — the same bar the
[standard-library roster](ValidatedTestPackages.md) holds every package to. A module appears here
only when, in every package the run reached, each test either matched `go test` or is disclosed, by its exact
failure, as something the converted code does not do.

Each module's pages are written by the converter beside its conversion and copied here unedited:
the module summary sums the per-package proof pages, and each proof page lists every verdict. Every
module's summary is the `VALIDATION.md` its NuGet package carries, below the package's notices.
One row of the `timefmt-go` proof page differs from the converter's text: its test name holds a pipe, and the row
writes it as an HTML code element so that GitHub and this site show the same name.

<!-- Provenance (2026-11-15, P4): pages copied from the go.* 1.26.10.3 wave kits (claude/c2-nugetgo-* and the
1261003 kits), each per-package page sha256-equal to its kit's file and each MODULE.md equal to its kit's
VALIDATION.md below the leading notice lines. The one exception is timefmt-go@v0.1.8/index.md line 26: the
converter's proof-page writer (validationProofPages.go, escapeProofCell) puts a backslash-pipe inside a code span,
which TestDocsTableCodeSpanHasNoEscapedPipe refuses; the row is rewritten in the guard's HTML form. Kit file
sha256 90f63ae649aa9c00..., page as committed 4341668c19ea7164.... A converter fix makes the page byte-equal again
at the next regeneration. -->

The rows are readings with Go 1.26.10, built on the go.* 1.26.10.3 standard-library packages, on `linux/amd64`,
at `Release` with the tiered JIT off, the test host's default. Each summary gives the module's counts of matched
and disclosed verdicts. `mousetrap` has no Go tests, so its row is a behavioral proof instead: one hosted row per
RID it ships.

| Module | Version | Packages | Platform | Proof |
|:--|:--|--:|:--|:--|
| `golang.org/x/sync` | v0.19.0 | 4 | `linux/amd64` | [summary](validation/modules/golang.org/x/sync@v0.19.0/MODULE.md) |
| `github.com/google/uuid` | v1.6.0 | 1 | `linux/amd64` | [summary](validation/modules/github.com/google/uuid@v1.6.0/MODULE.md) |
| `github.com/golang-jwt/jwt/v5` | v5.3.1 | 2 | `linux/amd64` | [summary](validation/modules/github.com/golang-jwt/jwt/v5@v5.3.1/MODULE.md) |
| `github.com/ritchiecarroll/hashset` | v1.0.0 | 1 | `linux/amd64` | [summary](validation/modules/github.com/ritchiecarroll/hashset@v1.0.0/MODULE.md) |
| `github.com/spf13/pflag` | v1.0.9 | 1 | `linux/amd64` | [summary](validation/modules/github.com/spf13/pflag@v1.0.9/MODULE.md) |
| `github.com/inconshreveable/mousetrap` | v1.1.0 | 1 | `win-x64`, `linux-x64`, `osx-x64`, `osx-arm64` | [behavior rows](validation/modules/github.com/inconshreveable/mousetrap@v1.1.0/MODULE.md) |
| `github.com/spf13/cobra` | v1.10.2 | 1 | `linux/amd64` | [summary](validation/modules/github.com/spf13/cobra@v1.10.2/MODULE.md) |
| `github.com/itchyny/timefmt-go` | v0.1.8 | 1 | `linux/amd64` | [summary](validation/modules/github.com/itchyny/timefmt-go@v0.1.8/MODULE.md) |
| `github.com/itchyny/gojq` | v0.12.19 | 1 | `linux/amd64` | [summary](validation/modules/github.com/itchyny/gojq@v0.12.19/MODULE.md) |

**`golang.org/x/sync`:** `semaphore`'s `TestWeightedAcquire` gives each acquire attempt a 10 ms deadline. With
the tiered JIT off, the first attempt compiles the acquire path at full optimization before it can wait, which
on the `linux/amd64` machine that measured it takes longer than 10 ms, so the test fails there and is disclosed as
`runtime-capability`; the semaphore proof page pins the exact failure. The test depends on timing: on a machine
fast enough to pass it, the entry reads as orphaned and the package still validates.

**`github.com/google/uuid`:** `TestClockSeqRace` skips on both sides. `TestVersion6` has an upstream race that can
fail it on either runtime, so it carries a timing-sensitive disclosure; in this reading both sides passed it, and
the entry is reported as orphaned, the ruled alternative reading.

**`github.com/golang-jwt/jwt/v5`:** the module also holds `cmd/jwt` and `test`, which have no tests of their own.

**`github.com/inconshreveable/mousetrap`:** each behavior row is a sample program run on that RID's own hosted
runner: the converted program and `go run` printed the same bytes, and the row names the assembly that loaded.
No row stands for another RID.

**`github.com/spf13/cobra` and `github.com/itchyny/gojq`:** each summary names one more package that was validated
and is not packed, so it is not counted in the row and its proof page is not shipped: `cobra/doc`, whose imports
are not go2cs NuGet packages, and gojq's `cli`, whose closure is deferred. gojq's row is the library root;
`cmd/gojq` is a main package.
