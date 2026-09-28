# The converted standard library moves to Go 1.24.13

![go2cs](../images/go2cs-small.png)

> Full text of the September 24, 2026 announcement, condensed in the
> [go2cs News Archive](../NEWS.md#september-24-2026--the-converted-standard-library-moves-to-go-12413-and-218-packages-validate-against-it).

---

## The measure

**go2cs now converts Go 1.24.13's standard library**, and the validated roster crossed the hop
re-derived, not carried: **218 of the 230 testable standard-library packages validate their own
Go 1.24.13 test suites in C#** — **56,974 matching verdicts** against `go test -json`, with **283**
divergences disclosed by exact failure signature — and, measured against the 224 packages a faithful
managed conversion can honestly validate at all, **97.3%**. On Linux, 187 of the 216 applicable rows
validate at their own Linux counts, at 53,048 matching verdicts. Every validated row is proved by a
run at Go 1.24.13, and the [roster](../ValidatedTestPackages.md) links each row's proof page.

## What moved

The count rises by fourteen from the Go 1.23.12 record's 204 while the testable set grows by fifteen,
so both percentages dip — 97.6% to 97.3% of the implementable set, 94.9% to 94.8% of the testable
one — and every movement has a name.

**One package validated at Go 1.23.12 is not validated at Go 1.24.13: `net/http`.** It matches 1,370
of its 1,387 verdicts. All 17 divergences trace to tests that run inside Go 1.24's new synctest
"bubbles" — `internal/synctest`, the runtime support behind the experimental `testing/synctest`
package — which go2cs does not yet support. `net/http` still ships as `go.net.http`.

Go 1.24 moved ten validated packages to new import paths, and none is a loss: their tests validate
under the packages that now hold them. Counting those, twenty-five rows join the roster, among them
`unique`, one of the five packages the Go 1.23.12 record left open, and Go 1.24's new public packages
`crypto/hkdf`, `crypto/mlkem`, `crypto/pbkdf2`, `crypto/sha3` and `weak`. Six implementable packages
are not yet validated: `reflect`, `runtime`, `runtime/pprof`, `net/http/pprof`, `net/http` and
Go 1.24's new `internal/synctest`.

## Linux

On Linux the count falls from 198 to 187, for two named reasons. The relocated packages' tests are not
yet run on Linux at their new paths. And `go/internal/srcimporter` has no Linux result: Go's own test
of it fails on the Linux reference machine, which leaves nothing to compare against, so it stays
validated on Windows. Of the 29 validated packages without a Linux result, 25 joined the roster at
this release.

The verdict total doubles, but that is Go's suites growing rather than a wider claim — `crypto/cipher`
alone matches 27,272 verdicts at Go 1.24.13, against 13 at Go 1.23.12 — so verdict totals do not
compare across Go releases, and the package count is the headline.

## For converted programs

The converted standard library carries Go 1.24's APIs — `os.Root`, `weak.Pointer`, `crypto/mlkem` and
`strings.Lines`, among others. `//go:embed` is honored for the first time: its patterns resolve at
conversion time, and its files are embedded behind their `string`, `[]byte` or `embed.FS` variable.

go2cs itself now builds with Go 1.24.13, where it built with Go 1.23.12. A `-recurse=nuget` conversion
run with Go 1.24.13 references the 1.24.13 packages, and it needs the module to resolve to Go 1.24:
go2cs refuses, before it writes a file, a module that resolves to a different Go release —
Go 1.23 included — rather than hand back a project that cannot restore. To stay on Go 1.23, build the
converter at the `nuget-1.23.12.3` tag with Go 1.23.12 and use the 1.23.12.3 packages.

Go 1.24's FIPS 140-3 module converts and its tests validate, but go2cs makes no FIPS 140-3 claim. The
module's integrity self-check hashes a binary layout that Go's linker writes and a .NET assembly does
not have, so under `GODEBUG=fips140=on` the converted check reports success without verifying
anything.

## Package IDs

Fifty-one package IDs are new, six of them for packages a Go program can import: `go.crypto.fips140`,
`go.crypto.hkdf`, `go.crypto.mlkem`, `go.crypto.pbkdf2`, `go.crypto.sha3` and `go.weak`.

Fourteen IDs end at 1.23.12.3, their last release, because Go 1.24 moved or deleted their packages:

| Go 1.23.12 package | Package ID | At Go 1.24.13 |
|:--|:--|:--|
| `crypto/internal/alias` | `go.crypto.internal.alias` | `crypto/internal/fips140/alias` |
| `crypto/internal/bigmod` | `go.crypto.internal.bigmod` | `crypto/internal/fips140/bigmod` |
| `crypto/internal/edwards25519` | `go.crypto.internal.edwards25519` | `crypto/internal/fips140/edwards25519` |
| `crypto/internal/edwards25519/field` | `go.crypto.internal.edwards25519.field` | `crypto/internal/fips140/edwards25519/field` |
| `crypto/internal/mlkem768` | `go.crypto.internal.mlkem768` | `crypto/internal/fips140/mlkem`, behind the public `crypto/mlkem` |
| `crypto/internal/nistec` | `go.crypto.internal.nistec` | `crypto/internal/fips140/nistec` |
| `crypto/internal/nistec/fiat` | `go.crypto.internal.nistec.fiat` | `crypto/internal/fips140/nistec/fiat` |
| `internal/concurrent` | `go.internal.concurrent` | `internal/sync` |
| `internal/weak` | `go.internal.weak` | the public `weak` |
| `runtime/internal/math` | `go.runtime.internal.math` | `internal/runtime/math` |
| `runtime/internal/sys` | `go.runtime.internal.sys` | `internal/runtime/sys` |
| `vendor/golang.org/x/crypto/hkdf` | `go.vendor.golang.org.x.crypto.hkdf` | `crypto/internal/fips140/hkdf`, behind the public `crypto/hkdf` |
| `vendor/golang.org/x/crypto/sha3` | `go.vendor.golang.org.x.crypto.sha3` | `crypto/internal/fips140/sha3`, behind the public `crypto/sha3` |
| `go/internal/typeparams` | `go.go.internal.typeparams` | deleted, with no successor |

Code outside the standard library cannot import any of the fourteen, so a converted project reaches
their successors through the new packages' own dependencies. Each ended ID stays restorable at
1.23.12.3 and is never unlisted.

## The release

The converted standard library publishes as **NuGet 1.24.13.1** — 344 packages, author-signed, still
targeting .NET 10 — with every proof page frozen at `validation/1.24.13.1` for the badges the packed
READMEs link, and the exact shipped tree browsable at the `nuget-1.24.13.1` tag. The Go 1.23.12
record stays frozen as it shipped, at `validation/1.23.12.3`.
