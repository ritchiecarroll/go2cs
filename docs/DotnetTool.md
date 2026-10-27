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
