# Validated Modules

Go modules whose own `_test.go` suites go2cs converted with `go2cs -tests -recurse`,
built against the converted standard library, ran under the Go-semantics test host, and compared
verdict for verdict against a clean `go test -json` baseline of the same sources — the same bar the
[standard-library roster](ValidatedTestPackages.md) holds every package to. A module appears here
only when, in every package the run reached, each test either matched `go test` or is disclosed, by its exact
failure, as something the converted code does not do.

Each module's pages are written by the converter beside its conversion and copied here unedited:
the module summary sums the per-package proof pages, and each proof page lists every verdict. Every
module's summary is the `VALIDATION.md` its NuGet package carries, below the package's notice.

| Module | Version | Packages | Matched | Disclosed | Platform | Proof |
|:--|:--|--:|--:|--:|:--|:--|
| `golang.org/x/sync` | v0.19.0 | 4 | 27 | 1 | `linux/amd64` | [summary](validation/modules/golang.org/x/sync@v0.19.0/MODULE.md) |
| `github.com/google/uuid` | v1.6.0 | 1 | 54 | 0 | `linux/amd64` | [summary](validation/modules/github.com/google/uuid@v1.6.0/MODULE.md) |
| `github.com/golang-jwt/jwt/v5` | v5.3.1 | 2 | 189 | 0 | `linux/amd64` | [summary](validation/modules/github.com/golang-jwt/jwt/v5@v5.3.1/MODULE.md) |
| `github.com/ritchiecarroll/hashset` | v1.0.0 | 1 | 37 | 0 | `linux/amd64` | [summary](validation/modules/github.com/ritchiecarroll/hashset@v1.0.0/MODULE.md) |
| `github.com/spf13/pflag` | v1.0.9 | 1 | 177 | 0 | `linux/amd64` | [summary](validation/modules/github.com/spf13/pflag@v1.0.9/MODULE.md) |

Measured with Go 1.24.13 at `Release` with the tiered JIT off, the test host's default.

**`golang.org/x/sync`:** `semaphore`'s `TestWeightedAcquire` gives each acquire attempt a 10 ms deadline. With
the tiered JIT off, the first attempt compiles the acquire path at full optimization before it can wait, which
on the `linux/amd64` machine measured takes longer than 10 ms, so the test fails there and is disclosed as
`runtime-capability`; the semaphore proof page pins the exact failure. With the tiered JIT on (`-test-tiered`),
the same machine matches all 8 of `semaphore`'s tests and all 28 of the module's. On `windows/amd64`, two runs on
their own each matched all 28 verdicts, and a third, made while the machine was running other heavy test jobs,
failed `TestWeightedAcquire` once; those proof pages are not reproduced here. That test depends on timing, so the
result above is claimed only for a machine that is not busy with other work.

**`github.com/google/uuid` and `github.com/golang-jwt/jwt/v5`:** the proof pages linked above are the `linux/amd64`
readings their packages carry. Each module was also read on `windows/amd64` from the same module sources (their
module sums match), with the same counts and no test differing; those proof pages are not reproduced here.
`uuid`'s `TestClockSeqRace` skips on both sides. `uuid`'s `TestVersion6` has an upstream race that can fail it on
either runtime; its proof page cites the race without disclosing it, and the proof is a run in which both sides
passed. `jwt/v5` also holds `cmd/jwt` and `test`, which have no tests of their own.
