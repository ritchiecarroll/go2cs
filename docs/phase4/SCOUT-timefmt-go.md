# Scout: `github.com/itchyny/timefmt-go` v0.1.8

A point-in-time measurement for the Q4 2026 module conversions (Roadmap: `itchyny/gojq` v0.12.19 with its
dependency `itchyny/timefmt-go` v0.1.8). This record sizes the leaf: timefmt-go is gojq's only third-party
dependency. It changes nothing on master and touches no `src/core` file.

**Verdict: timefmt-go v0.1.8 validates clean.** Its own test suite, converted and run in C#, matches `go test`
verdict for verdict: 417 matched, 0 disclosed, the same result in 3 of 3 runs. It converts and builds with no
error, and a converted program that uses it publishes as Native AOT and as a single file, each matching `go run`.
It is gojq's first proven dependency.

## Provenance

| Item | Value |
|:--|:--|
| Module | `github.com/itchyny/timefmt-go` v0.1.8, `h1:1YEo1JvfXeAHKdjelbYr/uCuhkybaHCeTkH8Bo791OI=` |
| Latest tag | v0.1.9 (2026-10-01), `h1:J4j1qMqZbg5jycwT6zIKfRgZ9aqqMxqanaI6Rr9Ezs8=` |
| Fetched | through `proxy.golang.org`, checksums verified against `sum.golang.org` (`go mod verify`: all modules verified) |
| Base | master `c6cddb6903` (batch 234), `src/core` as committed there |
| Converter | built from that tree's `src/go2cs` with go1.26.10 (`converterRevision` `exe-ae109353cd24e245`) |
| Toolchain | go1.26.10 linux/amd64, `GOTOOLCHAIN=local`, `GOROOT` spelled as `go env GOROOT` prints it |
| .NET | SDK 10.0.112 (the 10.0.1xx band), linux/amd64 |
| Authorization | the owner allowed downloading, building and running this module for measurement, 2026-11-07 |

The module is one package, `go 1.24`, with no dependencies: `format.go`, `parse.go` and `timefmt.go` (938 lines).
Its tests are an external `timefmt_test` package in three files: 4 `Test` functions, 4 examples, 2 fuzz targets
and 10 benchmarks. Production code imports `time`, `strconv`, `strings` and `errors`; the tests add `fmt`, `log`
and `testing`.

## The prediction, scored

Posted before the first build (mailbox, 2026-11-07):

| Predicted | Measured | Score |
|:--|:--|:--|
| Library conversion rc 0, no lift markers | rc 0, 0 markers, 1 of 1 package | MET |
| Library C# build: 0 to 4 errors | 0 errors | MET |
| Likely classes: goto/labels (16 gotos to `K`, `L`, `M`), typed-constant arithmetic | neither fired: every `goto` and label emits as C# `goto` and labels, and builds | MET (no class) |
| `-tests` conversion rc 0, test project builds with 0 to 2 errors | rc 0, 0 errors | MET |
| Test verdicts | not predicted | — |

With no build error, there is no error-code histogram and no class to remedy. Two advisories are printed and
neither is an error:

- `move2heap` at `format.go:385` (`buf` appends at more than one site without observing its capacity), the
  converter's ordinary refusal to lower that shape to the stack.
- The package license is unspecified: the module's `LICENSE` sits at the module root, not beside the converted
  project. `nugetgo-pack.ps1` packs it as the package's license file, so it matters only for a plain `dotnet pack`.

## Go baseline

`go test -json -count=1 ./...` in a scratch copy of the module: rc 0, 10 top-level results passed (4 tests, 4
examples, 2 fuzz seed runs) and 413 subtests passed. With `-tags safe` the result is the same. The machine's zone
is UTC (`parse.go` reads `time.Local`).

## Converted tests against Go

`go2cs -tests -recurse -test-action all -platforms linux/amd64 -go2cspath <tree>/src
-module-disclosures <tree>/src/tests/ModuleDisclosures <module copy> <scratch root>`, rc 0 in 30 s:

- **417 matched, 0 disclosed**: the 4 tests and their 413 subtests. Both sides were built with
  `-tags purego,math_big_pure_go,safe`, at `Release` with the tiered JIT off.
- 16 declarations are excluded from both sides, as the pipeline excludes them for every module: the 10 benchmarks,
  the 4 examples and the 2 fuzz targets ("deferred to Phase 4D").
- `-test-action compare` on the same artifacts read 417 matched again, twice: 3 of 3 runs.
- The input digest on the proof page is `sha256-0c5ef098bedfbf018bc102c9c6ae344e93c79f3cac75f13aab3e135001e11b41`.

**The proof is not a release proof.** It was read from a scratch copy, so the converter resolves no module version
("not a module-cache directory") and consults no disclosure manifest. None is needed, since nothing diverged. The
proof that ships with a package is converted from the module-cache path at a pack tree `T`, with the converter `G`
that packs it, as `docs/NugetgoPublish.md` section 1 requires; its input digest will differ from the one above.

## Publish smoke

A seven-line program of its own (one `Format` with most directives, one `Parse`), converted with plain `-recurse`
(2 of 2 packages), published for `linux-x64` and run three times each against `go run`:

| Publish | Result | Size | Start-up, 3 runs |
|:--|:--|--:|:--|
| Single file (`-p:PublishSingleFile=true`, ReadyToRun, compressed) | rc 0, 0 errors, 359 s | 34.7 MB | 679, 542, 570 ms; 3 of 3 match `go run` |
| Native AOT (`PublishAot` in the project) | rc 0, 0 errors, 1501 s | 151.2 MB | 231, 216, 189 ms; 3 of 3 match `go run` |

- `-p:PublishAot=true` on the command line fails with NETSDK1207 at the netstandard2.0 `go2cs-gen` analyzer. That
  is the known global-property trap, and `PublishAot` set in the app's project avoids it, as the Performance suite's
  `Directory.Build.targets` does.
- The Native AOT publish prints 746 distinct trim and AOT analysis warnings, and none names a timefmt-go member.
  626 are IL3054 generic-expansion warnings in golib's element boxes. The other 120 are reflection warnings
  (IL2026, IL2070, IL3050 and others) in golib and the standard library.

## What nugetgo packaging needs

Measured against `docs/NugetgoPublish.md`:

- **Package ID** `nugetgo.github.com.itchyny.timefmt-go`, the dotted module path. nuget.org reads 404 for it, so it
  is free.
- **Upstream license** MIT (`Copyright (c) 2020-2025 itchyny`); the pack takes `-LicenseSpdx MIT`.
- **Proof**: the module has Go tests, so it takes the validated route (section 1), not the behavioral one. A
  `MODULE.md` from a `-test-action all` run at `T` from the module-cache path, then `-recurse=nuget` into the pack
  root, with each project's `GoInputDigest` equal to its row.
- **Flavors**: the module has no GOOS-specific file, so the pack's per-GOOS conversions are expected to compare equal
  and ship one neutral assembly set. The pack's own check decides that; it is not measured here.
- **Closure**: the library references `go.time`, `go.strconv`, `go.strings` and `go.errors` and their closure, all
  in the published `go.*` release.
- **Row**: `community`. The upstream org is not the conversion's, so `-UpstreamPublishes` does not apply and the
  conversion repository is the owner's to create (section 0, item 6).
- **Order**: gojq's `-recurse=nuget` conversion references this package once the registry maps it, so it is packed
  and published before gojq.

## v0.1.9

v0.1.9 is a fix release: 15 changelog entries, mostly `Parse` fixes (`%p`, `%s`, ISO week dates, `%Z` names), and
the removal of `defer` from parsing. `parse.go` changes on 253 lines and `format.go` on 36. Its `go` directive is
still 1.24. It was read and compared, not converted. gojq v0.12.19 pins v0.1.8 (gojq's scout confirms), so v0.1.8 is
the version a gojq conversion needs; v0.1.9 is a candidate for a later revision.

## Not measured

- `windows/amd64` and `darwin`: every reading above is `linux/amd64`.
- Tiered JIT on (`-test-tiered`).
- The pack itself, the rehearsal and the publish (`docs/NugetgoPublish.md` sections 2 to 6).

All scratch roots, module copies and published binaries were deleted after the readings.

— C2 (2026-11-07)
