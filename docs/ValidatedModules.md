# Validated Modules

Third-party Go modules whose own `_test.go` suites go2cs converted with `go2cs -tests -recurse`,
built against the converted standard library, ran under the Go-semantics test host, and compared
verdict for verdict against a clean `go test -json` baseline of the same sources — the same bar the
[standard-library roster](ValidatedTestPackages.md) holds every package to. A module appears here
only when every package the run reached matched `go test` test for test.

Each module's pages are written by the converter beside its conversion and copied here unedited:
the module summary sums the per-package proof pages, and each proof page lists every verdict.

| Module | Version | Packages | Matched | Disclosed | Platform | Proof |
|:--|:--|--:|--:|--:|:--|:--|
| `golang.org/x/sync` | v0.19.0 | 4 | 28 | 0 | `windows/amd64` | [summary](validation/modules/golang.org/x/sync@v0.19.0/MODULE.md) |
| `github.com/google/uuid` | v1.6.0 | 1 | 54 | 0 | `windows/amd64`, `linux/amd64` | [summary](validation/modules/github.com/google/uuid@v1.6.0/MODULE.md) |
| `github.com/golang-jwt/jwt/v5` | v5.3.1 | 2 | 189 | 0 | `windows/amd64`, `linux/amd64` | [summary](validation/modules/github.com/golang-jwt/jwt/v5@v5.3.1/MODULE.md) |

Measured with Go 1.24.13 at `Release` with the tiered JIT off, the test host's default.

**`golang.org/x/sync`:** two runs on their own each matched all 28 verdicts. In a third run, made while the
machine was running other heavy test jobs, `semaphore`'s `TestWeightedAcquire` failed once. That test depends
on timing, so the matches above are claimed only for a machine that is not busy with other work.

**`github.com/google/uuid` and `github.com/golang-jwt/jwt/v5`:** each was read once on `windows/amd64` (the proof
pages linked above) and once on `linux/amd64`, from the same module sources (their module sums match), and both
platforms read the same counts with no test differing. `uuid`'s `TestClockSeqRace` skips on both sides. The
`linux/amd64` run was made as root, which can hide a permission defect in how a module is staged, so it vouches for
the counts, not for staging under an ordinary account; its proof pages are not reproduced here. `jwt/v5` also holds
`cmd/jwt` and `test`, which have no tests of their own.
