# The go2cs dotnet tool

`go2cs` is published on NuGet as a .NET tool. It is the quickest way to start: one command installs the
converter, and it needs no Go installation of its own.

```shell
dotnet tool install -g go2cs
go2cs -version
```

The tool is the converter itself, a native executable for Windows, Linux and macOS on x64 and Arm64. The .NET
SDK picks the build for your machine when it installs the tool. Building the converted C# still needs the
[.NET 10 SDK](https://dotnet.microsoft.com/download), and the first build restores the `go.*` packages from
NuGet.

Update it with `dotnet tool update -g go2cs`, and remove it with `dotnet tool uninstall -g go2cs`. To run it once
without installing it, use `dnx go2cs -- <arguments>`.

## The Go it converts with

The converter reads Go with Go's own tools: it loads and type-checks the source with the Go toolchain, and
compiles it once to collect hints for the C#. A tool release converts with one Go release, the one its `go.*`
packages are converted from (Go 1.26.10 for go2cs 1.26.10.x). It uses the first of these that is set:

1. `GO2CS_GOROOT`, the root of an existing install of that release. Any other release is refused.
2. `GOROOT`, when you set it. go2cs leaves your choice alone.
3. The `go` on `PATH`, when it is that release.
4. Its own copy in the user cache folder, downloaded on the first conversion that needs it.

The download is about 65 to 75 MB and unpacks to about 240 MB, once per release. It comes from
[go.dev](https://go.dev/dl/), or else from the Go module proxy (`GOPROXY`, which defaults to
`https://proxy.golang.org`) as the `golang.org/toolchain` module. Either way go2cs checks it against hashes
built into the converter before it uses it: go.dev's SHA-256 for the release archive, or the
`sum.golang.org` hash for the module. A file that does not match is deleted and the next source is tried.

`go2cs -version` and `go2cs -help` never download anything.

| Platform | The copy lives in |
|:--|:--|
| Windows | `%LocalAppData%\go2cs\toolchains\go1.26.10.windows-<arch>` |
| macOS | `~/Library/Caches/go2cs/toolchains/go1.26.10.darwin-<arch>` |
| Linux | `$XDG_CACHE_HOME/go2cs/toolchains/go1.26.10.linux-<arch>`, or `~/.cache/...` |

A module whose `go` line names a newer Go than the tool converts with is refused with a message that says so:
use the go2cs release built for that Go, or lower the module's `go` line.

## `go2cs toolchain`

```shell
go2cs toolchain install             # download the Go now, for example before going offline
go2cs toolchain install -from FILE  # install it from a release archive or toolchain module zip you have
go2cs toolchain env                 # print GOROOT, GOVERSION and GO, the go command go2cs uses
go2cs toolchain prune               # remove copies of other Go releases; -all removes this one too
```

`install -from` takes the release archive go.dev publishes (`go1.26.10.<os>-<arch>.tar.gz`, or `.zip` on Windows)
or the module zip a Go module proxy serves, and checks it against the same hashes, so a machine with no network
access can be set up from a file copied to it.

`toolchain env` is for scripts. The
[Tour of go2cs](https://github.com/ritchiecarroll/go2cs/blob/master/src/tour/README.md) start scripts use it to
run the tour with the tool's Go when no other Go is on `PATH`. To run `go build` with the same Go yourself:

```powershell
$go = (go2cs toolchain env | Select-String '^GO=').Line.Substring(3)
& $go build ./...
```

```shell
"$(go2cs toolchain env | sed -n 's/^GO=//p')" build ./...
```

## The C compiler for `-cgo`

A conversion with `-cgo` loads the package with cgo on, which runs Go's `cmd/cgo` and needs a C compiler on the
converting machine.

> **Status:** the choice below reaches the `-cgo` load in a following change. Until that lands, a `-cgo` load uses
> `CC` or the platform's default compiler exactly as the `go` command does, and `-cc` and `-cc-download` change
> nothing. `go2cs toolchain install -cc` already works.

Once it lands, go2cs uses the first of these that applies:

1. `-cc CMD`, used as given. It may carry arguments, as `zig cc` does, and it never downloads.
2. `CC` from the environment, as the `go` command reads it.
3. The platform's default compiler on `PATH`: `clang` on macOS, `gcc` elsewhere, the names Go itself defaults to.
4. The C toolchain go2cs pins for the OS, downloaded into the user cache on first use:

| OS | Pinned toolchain | Why |
|:--|:--|:--|
| Linux, macOS | zig 0.17.0, run as `zig cc` | One download; it links against the system's own C libraries. |
| Windows | llvm-mingw 20261020 (UCRT, x86_64), run as its `x86_64-w64-mingw32-clang` | It resolves C functions to the Windows DLLs (`cos` and `strlen` to the UCRT, `GetCurrentProcessId` to KERNEL32), as Go's own MinGW build does. zig links them into the program statically, which leaves nothing to call from C#. |

`-cc pinned` goes straight to step 4. That is the reproducible choice: a host compiler is not reproducible, so every
conversion whose output is compared byte for byte passes it. `-cc pinned:zig` and `-cc pinned:llvm-mingw` choose one
pinned toolchain explicitly, wherever go2cs pins it (zig on every platform, llvm-mingw on Windows x64). `-cc host`
refuses step 4. `-cc-download=false` refuses to download, but still uses a pinned toolchain already in the cache.
Every refusal names the steps it tried, and no step falls back to loading the package with cgo off.

Every archive, from any source, is checked against a SHA-256 built into the converter before anything is unpacked.
A mismatch deletes the file and the next source is tried, so a source can fail a download but cannot substitute an
archive.
- **zig** comes from ziglang.org's community mirrors, in random order, and then from ziglang.org itself, as ziglang.org
  asks of automated downloads. Its hashes are generated from ziglang.org's download index. On Linux and macOS its
  `.tar.xz` archive is unpacked with the system `tar`. On Linux that needs `xz` installed (Debian-slim and Alpine images
  lack it), and go2cs says so before downloading anything. zig's caches go beside it, under go2cs's own cache folder.
- **llvm-mingw** comes from its GitHub release. Its publisher signs nothing, so it is pinned by the release asset's
  SHA-256 digest alone. Its archive is a `.zip` and needs no other tool.

| Platform | The pinned toolchain lives in |
|:--|:--|
| Windows | `%LocalAppData%\go2cs\toolchains\llvm-mingw-20261020-windows-amd64` (or `zig-0.17.0-windows-<arch>` when chosen) |
| macOS | `~/Library/Caches/go2cs/toolchains/zig-0.17.0-darwin-<arch>` |
| Linux | `$XDG_CACHE_HOME/go2cs/toolchains/zig-0.17.0-linux-<arch>`, or `~/.cache/...` |

```shell
go2cs toolchain install -cc                  # download this OS's pinned C toolchain now
go2cs toolchain install -cc -pin zig         # or a named one: zig, or llvm-mingw (Windows x64)
go2cs toolchain install -cc -from FILE       # install it from a release archive you have, checked against the pin
```

go2cs downloads zig from its publishers (ziglang.org or a mirror), verifies it against a pinned SHA-256, and
redistributes nothing. zig bundles Apple's darwin SDK headers under the APSL; using them is governed by those terms.
Converted output contains no file from zig: it carries the C declarations cmd/cgo derives from the headers a package
includes, as a Go cgo build does.
On Windows the default is llvm-mingw, which go2cs likewise downloads from its publisher and does not redistribute.

## Go's telemetry

The Go the tool downloads is Go's own release, unchanged, so it keeps Go's default telemetry setting: local
counters, written under your user configuration folder and never uploaded unless you opt in with
`go telemetry on`. `go telemetry off` turns them off.

## Building from source instead

[Installing the converter](README.md#installing-the-converter) builds go2cs from a clone with `go install`.
That build uses the Go you build it with, and downloads nothing.

## License

The converter is AGPL-3.0-only with the go2cs Converter Output Exception: the C# it generates does not inherit
the AGPL by being generated ([LICENSING.md](https://github.com/ritchiecarroll/go2cs/blob/master/LICENSING.md)).
The tool downloads Go from Go's own sources and does not redistribute it; Go is under its BSD license.
