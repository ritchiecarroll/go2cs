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

Measured with Go 1.24.13 at `Release` with the tiered JIT off, the test host's default.

**`golang.org/x/sync`:** two runs on their own each matched all 28 verdicts. In a third run, made while the
machine was running other heavy test jobs, `semaphore`'s `TestWeightedAcquire` failed once. That test depends
on timing, so the matches above are claimed only for a machine that is not busy with other work.
