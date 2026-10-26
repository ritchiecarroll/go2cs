# PLAN — a `dotnet tool` installer for go2cs that needs no Go installation
<!-- {% raw %} -->

> **STATUS: ACCEPTED (the owner, 2026-10-26): build the `go2cs` dotnet tool for 1.26.10.2, with the
> download design. The rulings on §10 are in §11.** Originally PROPOSED (COORD research, 2026-10-26). An instance plan
> answering the owner's ask of 2026-10-26: what it takes to ship go2cs as a .NET tool that brings its own Go,
> where it lives, what it costs to build and to keep, and what to recommend. Measurements are from the i7
> (win-x64) against Go 1.26.10 and a converter built at master `d269ef477b`; a linux-amd64 archive was measured
> for size only. Open questions for the owner are in §10.

---

## 0. Recommendation (summary)

1. **Build it, in this repository, as one NuGet tool ID `go2cs`** (`dotnet tool install -g go2cs`, or one-shot
   `dnx go2cs ...`), versioned and released with the `go.*` packages (tool `1.26.10.N` ↔ `go.*` `1.26.10.N`).
2. **Do not bundle Go in the package. Fetch the exact pinned Go on first use** into a go2cs-owned cache,
   verified against SHA-256 values **baked into the converter at release time** (not read from the network).
   Bundling works and fits nuget.org, but it buys almost nothing: a consumer must already be online to restore
   the `go.*` packages, and it costs Go redistribution, AV/notarization exposure and a re-bundle per Go patch.
3. **Put the provisioning logic in the converter itself** (Go code: fetch, verify, extract, export `GOROOT`/`PATH`/
   `GOTOOLCHAIN=local`), so the GitHub release archives gain the same "no Go needed" behavior for free. The tool
   package is then thin packaging of the existing six native binaries.
4. **Package shape:** a pointer package plus six RID packages whose entry point is the native `go2cs` binary
   (`Runner="executable"`), hand-packed on one Linux runner in `release-binaries.yml`. Fallback if the CLI refuses a
   non-.NET entry point: a small framework-dependent .NET shim (the consumer has the .NET 10 SDK anyway).
5. **Offline/air-gapped and corporate users** get `go2cs toolchain install --from <archive>` (same hash check),
   `GO2CS_GOROOT` to name an existing exact-version Go, and module-proxy (`GOPROXY`) as a second download source.
6. **Estimate:** ~8-11 lane-days to production (prototype 2-3, production 3-4, CI/release 2-3, docs 1); steady state
   ~0.25 lane-day per release and ~0.25 per Go patch hop. Bundling instead: ~11-15 lane-days and ~0.5-1 per patch.

## 1. What Go the converter needs at run time

**1a. The `go` command.** Direct invocations (non-test sources at `d269ef477b`):

| Site | Command | Purpose |
|---|---|---|
| `directiveOperations.go:289` | `go list -find -f {{context.ToolTags}} unsafe` | build tool tags (falls back to `build.Default` on failure) |
| `modulesLock.go:176` | `go mod download` (`-mod=readonly`) | dependency preflight; needs network or a warm module cache |
| `projectFileWriter.go:45` | `go env <name>` | GOROOT/GOPATH/GOVERSION resolution (`main.go:216-225`, `toolchainResolution.go`) |
| `stackAppendHints.go:681-712` | `go build` / `go test -c` with `-gcflags=-m -d=variablemakehash=vy` | the stack-append harvest, for **three targets** (`windows/linux/darwin amd64`) |
| `testConversion.go:10506` | `go test`, the oracle (`-tests` mode) | contributor validation only |

Plus every `packages.Load` (`conversionDriver.go:206-220`, `moduleConverter.go:262`, `moduleTestsDriver.go:115`,
`linknameOperations.go:160`, `testConversion.go:761,1775`, `stdLibConverter.go:319,391`), which shells out to
`go list` on `PATH`.

**1b. GOROOT/src.** `go/types` type-checks the standard library from source (`LoadAllSyntax`); `checkGoRootSpelling`
(`main.go:161`) refuses a GOROOT with no `src`. Not needed: `src/cmd`, `testdata`, `*_test.go`, `*.syso` (consumer
modes only; `-tests` needs the full tree).

**1c. The compiler is NOT optional.** Measured on the i7, converting a one-file module with `-recurse=nuget`
(isolated `GOCACHE`/`GOPATH`, `GOENV=off`, `GOTOOLCHAIN=local`, `CGO_ENABLED=0`):

| Trimmed GOROOT | Result |
|---|---|
| `go` + trimmed `src`, no `pkg/tool` | **fails at load**: `go: no such tool "compile"` (from `go list` under `packages.Load`) |
| + `compile` | fails: `no such tool "asm"` |
| + `compile`, `asm`, `pkg/include` | fails: `no such tool "link"` |
| + `compile`, `asm`, `link`, `pkg/include` | **converts; output byte-identical** to the full install (stack-append hint present) |
| same, minus every `*.syso` | byte-identical |

So `go list` itself requires `compile`, `asm` and `link` to exist, and the harvest really compiles: a cold run spends
**~20-37 s** and writes **~104 MB** of `GOCACHE` compiling the standard library for the three harvest targets; a warm
rerun takes **~3 s**. cgo, `cover`, `vet`, `fix`, `gofmt`, `api/`, `doc/`, `misc/`, `test/` and `lib/` are not
needed for consumer conversion (`-tests` needs `vet`, since `go test` runs it). The minimal working set is
`bin/go`, `pkg/tool/<os_arch>/{compile,asm,link}`, `pkg/include`, `go.env`, `VERSION`, `LICENSE`, `PATENTS`,
and `src` minus `cmd`, `testdata`, `*_test.go`, `*.syso`.

**1d. Version pinning is part of correctness.** The converter emits `$(GoStdLibVersion)` from `GOVERSION`
(`toolchainResolution.go`), and the `go.*` packages are converted from exactly one Go release (docs/README.md
"Requirements": "Go 1.26.10, exactly that release"). A tool that brings its own pinned Go makes that requirement true
by construction instead of by documentation.

**1e. Hazard measured incidentally:** the i7's ambient environment names a different, older Go in `GOROOT`; building
with the pinned `go` binary under that environment failed with `compile: version "go1.23.1" does not match go tool
version "go1.26.10"`. Any provisioner must set `GOROOT`, prepend its `bin` to `PATH`, and set `GOTOOLCHAIN=local`
for every child, and the converter's own `loaderGoRootDecision` refusal (`main.go:479`) must agree with it.

## 2. Sizes

| Item | win-x64 | linux-x64 | Source |
|---|---|---|---|
| Official Go 1.26.10 archive | 71.5 MB (zip) | 63.8 MB (tar.gz) | go.dev/dl JSON index |
| Other RIDs | win-arm64 68.2, linux-arm64 60.9, osx-x64 64.7, osx-arm64 61.7 MB | | same |
| Toolchain module zip (`golang.org/toolchain`) | 72.9 MB | 71.7 MB | proxy.golang.org (osx-arm64 69.6 MB) |
| Full install on disk | 256 MB | 238 MB | measured |
| Minimal set (§1c) on disk | 93 MB (3,717 files) | ~93 MB | measured (win) |
| Minimal set compressed | ~31 MB (zip -9, with syso: 32.6) | 26.5 MB tar.gz (30.6 with syso) | measured |
| go2cs release archive | 12.5 MB | 12.4 MB | `nuget-1.26.10.1` Release assets (11.2-12.7 MB across six) |

**nuget.org limit: about 250 MB per package** ([Publish a package, "Package size limits"](https://learn.microsoft.com/nuget/nuget-org/publish-a-package)).
A per-RID bundled tool package (go2cs + minimal Go ≈ 40-45 MB) fits easily. A single all-RID package does not
sensibly fit (six × ~43 MB ≈ 260 MB). A single all-RID package **without** Go (six go2cs binaries ≈ 72 MB) fits.
Longest path in the minimal set is 75 characters, so a tool-store GOROOT stays under Windows' 260-character limit.

## 3. The .NET tool shape

.NET SDK 10 adds RID-specific, self-contained and Native AOT tools ([Create RID-specific, self-contained, and AOT
.NET tools](https://learn.microsoft.com/dotnet/core/tools/rid-specific-tools)): a top-level **pointer package**
(`<id>.<ver>.nupkg`) lists `<id>.<rid>.<ver>.nupkg` packages through `ToolPackageRuntimeIdentifiers`; the CLI picks
the RID package on `dotnet tool install` and on `dnx`; an `any` entry gives a framework-dependent fallback; all RID
packages must carry the pointer's exact version and be **pushed before the pointer**. A RID package's
`DotnetToolSettings.xml` names a native entry point with `Runner="executable"` (the AOT form), and a RID-specific
tool may stay framework-dependent with `PublishSelfContained=false`
([Andrew Lock, .NET 10 tool packaging](https://andrewlock.net/exploring-dotnet-10-preview-features-7-packaging-self-contained-and-native-aot-dotnet-tools-for-nuget)).

Candidate contents per RID package:
- **(T1, preferred)** the Go-built `go2cs` binary as the `Runner="executable"` entry point, `LICENSE`,
  `LICENSE-EXCEPTION`, `THIRD-PARTY-NOTICES`; hand-authored nuspec, packed on one Linux runner (no AOT
  per-OS pack needed). **Unverified:** that the CLI launches a non-.NET executable entry point without complaint,
  and that Unix execute bits survive NuGet extraction. Both are prototype gates (§8).
- **(T2, fallback)** a tiny framework-dependent .NET shim that execs the bundled native `go2cs` (and `chmod +x`s it).
  The consumer already has the .NET 10 SDK, so framework-dependent costs nothing; AOT would need per-OS packing.
- **(T3)** one `any` package carrying all six binaries plus a shim that picks one: a single push, ~72 MB; viable
  only without bundled Go.

## 4. Bundle versus download

| | **B: bundle minimal Go per RID** | **D: download pinned Go on first use** (recommended) | **S: status quo, Go required** |
|---|---|---|---|
| Package size | ~43 MB per RID package | ~12 MB per RID package | ~12 MB |
| First run | convert immediately (plus the cold harvest) | + one ~64-73 MB download, verify, extract | user installs Go first |
| Offline | works after install | needs `--from <archive>` or `GO2CS_GOROOT`; but `go.*` restore needs network anyway | works |
| Trust | we redistribute Google's binaries; our signature covers them | bytes from go.dev, checked against hashes baked into the signed converter | user's own install |
| Corporate proxy | NuGet feed only | `HTTPS_PROXY` honored by Go's `net/http`; `GOPROXY` route for the toolchain module; archive pre-seed | n/a |
| Go patch hop | re-download six archives, re-trim, re-verify, repack | regenerate six SHA-256 lines (scriptable from go.dev JSON) | docs bump |
| Notices / licensing | must ship Go's notices (§5) | none shipped; Google's own terms at download | none |
| AV / notarization | bundled `go.exe`/`compile.exe` are unsigned (Authenticode `NotSigned`, measured) inside our package | same binaries, but not ours to scan-gate | n/a |
| `-tests` mode | needs the full tree, not the minimal set | full archive extracted: works | works |

Pinned hashes are copied into the converter by `go generate` from go.dev's JSON index for `VERSION` (the
same pattern as `corpus-release.txt`), and a repoguard-style test asserts they match the pinned Go version.
A hash fetched at run time from go.dev would add nothing over TLS; a baked hash makes the signed converter the root
of trust. Cache: a go2cs-owned directory (`%LOCALAPPDATA%\go2cs\toolchains\go1.26.10`, `$XDG_CACHE_HOME/go2cs/...`),
deliberately **not** `GOMODCACHE`, because a GOROOT under the module cache is the case `conversionDriver.go` documents
as reclassifying the standard library. Extract the full archive (~240-256 MB) so `-tests` also works; offer
`go2cs toolchain prune`.

## 5. Licensing (obligations as the license texts state them; not legal advice)

- Go is BSD-3-Clause, "Copyright 2009 The Go Authors" (`LICENSE`): redistribution of source must retain the notice,
  conditions and disclaimer; redistribution **in binary form must reproduce them "in the documentation and/or other
  materials provided with the distribution"**; Google's and contributors' names may not endorse derived products.
- `PATENTS` is a grant from Google with a termination clause; it imposes no notice duty but ships alongside.
- The `go`, `compile`, `asm` and `link` binaries embed only `golang.org/x/*` code (crypto, mod, net, sync, sys,
  telemetry, text, tools; read from the binaries), all under the same Go Authors BSD text. Trimming away `src/cmd`
  removes their `cmd/vendor` LICENSE files, so a bundle must copy them into a notices file.
- Excluding `*.syso` also removes non-BSD material: BoringSSL objects (OpenSSL/ISC terms,
  `src/crypto/internal/boring/LICENSE`) and the race runtime (LLVM ThreadSanitizer, `src/runtime/race/README`).
- go2cs itself is AGPL-3.0-only with an output exception; a bundle's NuGet license expression would be compound
  (`AGPL-3.0-only AND BSD-3-Clause`). Option D ships none of Go and carries none of this.

## 6. Where it lives

**In this repository.** The tool's version is the `go.*` version, its payload is the converter that
`release-binaries.yml` already builds for the six RIDs, and the provisioning code belongs in `src/go2cs`. A separate
repo would duplicate the version stamp, the record-commit lookup and the attestation, and would drift on every patch
hop. Proposed layout: `src/go2cs/toolchainProvision.go` (+ generated `toolchainPins.go`), `src/tools/go2cs-tool/`
(nuspec templates and `DotnetToolSettings.xml`), packing in `release-binaries.yml`, signing and pushing in the
existing ritual.

Release-ritual fit:
- **Signing.** The ritual signs locally with one PIN (`release-nuget.ps1` Phase 2 via `sign-nupkgs.ps1`); the tool
  packages are built in CI, so they must be fetched from the draft Release (SHA-256-checked against the attested
  `SHA256SUMS`) into the signing set. That is a runbook change ("Release day", `GoCorpusMigration.md`).
- **The push cap.** nuget.org allows **350 pushes per hour per API key**
  ([Rate limits](https://learn.microsoft.com/nuget/api/rate-limits)); the `go.*` set already sits close to that, and
  the tool adds seven (six RID + pointer). Push the tool after the `go.*` hour or under its own scoped key, RID
  packages first and the pointer last.
- **Version lines** (`PLAN-version-lines.md`): each line's tool pins its own Go; a `release/go1.N` maintenance release
  ships its own tool version through the same steps. Go 1.27 is already out, so this applies as soon as a 1.27 hop lands.
- **ID.** `go2cs` returned 404 from nuget.org's flat container on 2026-10-26 (no published versions; reservation
  status unknown).

## 7. Estimate (lane-days; one Opus lane ≈ one engineer-day)

| Phase | D (recommended) | B (bundle) |
|---|---|---|
| Prototype: provisioner, win-x64 + linux-x64 packs, local-feed install, `dnx`, convert + `dotnet build` | 2-3 | 2-3 |
| Production: six RIDs, cache locking, `--from`, `GO2CS_GOROOT`, proxy route, errors, unit tests with a fake server, pin guard | 3-4 | 4-5 (+ trim recipe, notices, per-RID verification) |
| CI + release: pack in `release-binaries.yml`, six-runner install smoke, sign/push steps, runbook amendments | 2-3 | 3-4 (+ size and AV checks) |
| Docs: README "Requirements"/"Installing", ConvertingAModule, a tool page | 1 | 1 |
| **Total to production** | **8-11** | **10-13** (+ 1-2 slack for AV/notarization findings) |
| Per release, same Go | ~0.25 | ~0.25 |
| Per Go patch hop | ~0.25 (regenerate pins, re-smoke) | 0.5-1 (re-bundle, re-trim, re-verify) |
| Per Go minor hop | ~0.5 (re-check what `go list` requires) | ~1 |

Assumptions: the T1 package shape works or T2 costs under a day; macOS runners already in `release-binaries.yml`
host the smoke; no Authenticode or Apple Developer ID signing of go2cs itself is in scope; the converter's
existing load/harvest paths need no change beyond exporting the provisioned environment.

## 8. Risks

1. **The compiler dependency is real.** Any "lighter" bundle without `compile`/`asm`/`link` fails at load (§1c);
   a future Go may require another tool. Gate: the six-runner smoke converts a module that exercises the harvest and
   diffs against a full-install conversion.
2. **Silent emission drift.** If the harvest's `go build` fails, `stackAppendHarvest` returns "" and the conversion
   proceeds without stack-append hints (`stackAppendHints.go:714-720`): a broken bundle would emit different C# and
   exit 0. The smoke must compare emission, not only exit codes.
3. **Ambient Go conflicts** (§1e): `GOROOT`, `PATH`, `GOTOOLCHAIN`, and settings written by `go env -w` (which
   `GOENV` would otherwise apply). Export `GOROOT`/`PATH`/`GOTOOLCHAIN=local` per child; keep the user's
   `GOPROXY`/`GOPRIVATE`/`GOFLAGS`, which module resolution legitimately needs.
4. **A module that demands newer Go** (`go 1.27` or a `toolchain` line) fails under `GOTOOLCHAIN=local`. That is correct
   (the `go.*` packages are 1.26.10), but the message must say so.
5. **Unix execute bits** on extracted package files (B, and T1/T2 entry points) are unverified; the shim or
   provisioner may need to `chmod`.
6. **Antivirus and SmartScreen.** Go's Windows binaries and go2cs are unsigned (measured); nuget.org virus-scans
   packages. B puts those binaries inside our package; D keeps them as Google's download.
7. **macOS.** CLI downloads are not quarantined, and Go's linker ad-hoc signs darwin binaries, so Gatekeeper
   should not block either path; verify on both macOS runners.
8. **First-run cost.** D adds a ~64-73 MB download; both add the ~20-40 s cold harvest and ~104 MB of `GOCACHE`.
9. **Consumers still need the .NET 10 SDK** to build the output, and network for `go.*` restore and
   `go mod download`. The tool removes one prerequisite, not all of them.
10. **Toolchain telemetry.** The bundled or downloaded `go` keeps Go's default local telemetry counters in the user
    config directory; document it rather than change it.
11. **Release surface.** Seven more packages to sign and push per release, the push-cap interaction (§6), and a
    CI-to-signing-box transfer that today does not exist.

## 9. Option ranking

D + T1 (recommended) > D + T2 > B per RID > S. Choose B only if the owner wants an install that converts with no
network at all after `dotnet tool install`, which `-recurse=nuget` cannot use anyway without a reachable `go.*` feed.

## 10. Open questions for the owner

- **OQ-1.** Download on first use (D) or bundle (B)? This plan recommends D.
- **OQ-2.** Package ID `go2cs` (command `go2cs`), and should the tool push under its own scoped API key to stay clear
  of the 350/hour cap?
- **OQ-3.** Sign the CI-built tool packages with the release certificate (adds a CI-to-signing step), or publish
  them author-unsigned and rely on the attested Release?
- **OQ-4.** When should it land: with `1.26.10.2`, or as its own release after it?
- **OQ-5.** Should the GitHub release archives also gain provisioning (it comes with D/T1 at no extra cost), and
  should the README lead with `dotnet tool install -g go2cs` over clone-and-`go install`?

## 11. Rulings (2026-10-26)

<!-- The owner, in the COORD chat: "Sure, let's go with `go2cs` dotnet tool for 1.26.10.2 -- sounds like it would
     install from dotnet as a tool and 'just work'. Very cool for .NET first timers experimenting with Go
     conversions." OQ-1, OQ-2's ID, OQ-4 and OQ-5's README half are his; the rest are COORD's, within that frame. -->

- **OQ-1: download on first use (D).** The tool installs and "just works": the first conversion fetches the pinned
  Go, verifies it and runs it, with no separate Go install.
- **OQ-2: the ID and command are `go2cs`, under the same API key as the `go.*` packages.** The tool's seven packages
  take the release past nuget.org's 350-push window, which the resumable, rate-aware push (1.26.10.2 seat #6) handles.
- **OQ-3: the tool packages are signed, like every package.** The owner's account has the signing certificate
  registered, so nuget.org refuses an unsigned push. The tool packages are therefore packed on the release machine
  by the release ritual, from six `go2cs` binaries cross-compiled there from the release record commit (the converter
  is pure Go, so cross-compiling needs no other toolchain), and signed in the same PIN session as the `go.*` packages.
  `release-binaries.yml` keeps building the attested GitHub release archives from the same commit.
- **OQ-4: it lands with 1.26.10.2.**
- **OQ-5: yes to both.** The release archives gain provisioning too. The README and the home page lead with
  `dotnet tool install -g go2cs` for a first-time user, and keep the build-from-source route below it.
- **Signing reaches the binaries too (the owner, 2026-10-26).** Every Windows `go2cs` executable the project ships
  is Authenticode-signed with the release certificate: the ones inside the tool packages, and the ones in the
  GitHub release archives. The release machine downloads the CI-built archives, signs the Windows executables in
  the release's PIN session, re-packs them and pushes them back to the draft Release. The certificate is an
  Authenticode certificate, so it signs Windows binaries only. The archives for every platform are covered by a
  detached GPG signature over `SHA256SUMS`, made with the key that signs the release tag. macOS signing and
  notarization need an Apple Developer ID, and remain an open question for the owner.
  1.26.10.2's seat #12 builds this; the attestation story for re-signed bytes is part of that seat.
<!-- The owner, in the COORD chat, 2026-10-26: "per questions: download, go2cs ID, same API key, signing would be
     good (also for release binaries, even if downloaded by i7, signed and pushed back), yes on README tool install." -->


<!-- {% endraw %} -->
