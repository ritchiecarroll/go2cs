# The go2cs dotnet tool

`go2cs` is published on NuGet as a .NET tool. It is the quickest way to start: one command installs the
converter, and it needs no Go installation of its own.

```shell
dotnet tool install -g go2cs
go2cs -version
```

Until the first conversion, `go2cs -version` prints `go.packages=unknown` and `go.toolchain=unknown`: the tool
downloads its Go on first use, and these two lines are read from that Go.
<!-- src/go2cs/versionReport.go: go.packages and go.toolchain come from goVersion() (readme.go, `go env GOVERSION`)
     and print "unknown" when it is empty. P3's cold first-user pass on the published 1.26.11.1 tool (mailbox
     2026-11-19 23:12Z, finding b): unknown before the first conversion, 1.26.* and go1.26.11 after it. First post-.4
     docs batch, COORD 2026-11-20 07:19Z. -->

The tool is the converter itself, a native executable for Windows, Linux and macOS on x64 and Arm64. The .NET
SDK picks the build for your machine when it installs the tool. Building the converted C# still needs the
[.NET 10 SDK](https://dotnet.microsoft.com/download), and the first build restores the `go.*` packages from
NuGet.
A single-package `-cgo` conversion builds against a go2cs clone (`dotnet build -p:go2csPath=<go2cs>/src/`). Under
`-recurse=nuget -cgo`, a module's cgo packages convert with it and reference the `go.*` packages, for the machine's
own operating system and architecture, or for another target when `CC` names that target's C compiler (see `-cgo` in
[Command line](CommandLine.md)). A module's cgo package is packed for publishing for `win-x64`,
`linux-x64`, `osx-x64`, `linux-arm64` and `osx-arm64`.
<!-- 2026-11-28 (P5, the cgo arm on claude/p5-cgo-osx-arm64, stacked on D6 of DESIGN-darwin-arm64-flavour.md): osx-arm64
     joins the -Cgo pack with its own darwin/arm64 flavor (zig aarch64-macos), read by cgo-nuget-e2e's J3 osx-arm64 leg.
     Before it: "`osx-arm64` is left out until the `go.*` packages ship a true `osx-arm64` build." -->
<!-- 2026-11-23 .5 docs pass (P1), UX-8, at master 97cca467a2 (owner: resolve each "next release" in present tense, as
     true at .5): "planned for the next release" described the pack before it landed. It landed in train 3
     (claude/c2-cgo-nuget-pack, claude/c2-cgo-pack-2b, merge ef9be9bd32) and its push gate, the run-time target check
     A6, in train 4 (claude/c2-cgo-a6, merge c9143ae615): docs/NugetgoPublish.md "Cgo" states the four RIDs, the
     osx-arm64 exclusion, and that a pack is pushed only against a go.lib that carries A6, which the .5 go.lib does.
     The other-target sentence: commit 07378119f9 (merge 640d45c9eb, train 3), CommandLine.md's -cgo row. -->
<!-- src/go2cs/internal/clihelp/options.go, the -cgo entry's history comment ("Not combinable with -stdlib or -tests. With -recurse, every non-standard-library package of the module loads cgo-on and each cgo package converts this way"); src/go2cs/csproj-template.xml:135 (`<ProjectReference Include="$(go2csPath)gen/go2cs-gen/go2cs-gen.csproj"`) and src/go2cs/csproj-template.xml:187 (`<ProjectReference Include="$(go2csPath)core/golib/golib.csproj" />`). COORD's ruling O2, 2026-11-14 (cgo 3.1's release-path seat). -->

Update it with `dotnet tool update -g go2cs`, and remove it with `dotnet tool uninstall -g go2cs`. To run it once
without installing it, use `dnx go2cs -- <arguments>`.

## The Go it converts with

The converter reads Go with Go's own tools: it loads and type-checks the source with the Go toolchain, and
compiles it once to collect hints for the C#. A tool release converts with one Go release, the one its `go.*`
packages are converted from (Go 1.26.11 for go2cs 1.26.11.x). It uses the first of these that is set:

1. `GO2CS_GOROOT`, the root of an existing install of that release. Any other release is refused.
2. `GOROOT`, when you set it. go2cs leaves your choice alone.
3. The `go` on `PATH`, when it is that release.
4. Its own copy in the user cache folder, downloaded on the first conversion that needs it.

The download is a Go release archive, tens of megabytes that unpack to a few hundred, once per release. It comes from
[go.dev](https://go.dev/dl/), or else from the Go module proxy (`GOPROXY`, which defaults to
`https://proxy.golang.org`) as the `golang.org/toolchain` module. Either way go2cs checks it against hashes
built into the converter before it uses it: go.dev's SHA-256 for the release archive, or the
`sum.golang.org` hash for the module. A file that does not match is deleted and the next source is tried.

`go2cs -version` and `go2cs -help` never download anything.

| Platform | The copy lives in |
|:--|:--|
| Windows | `%LocalAppData%\go2cs\toolchains\go1.26.11.windows-<arch>` |
| macOS | `~/Library/Caches/go2cs/toolchains/go1.26.11.darwin-<arch>` |
| Linux | `$XDG_CACHE_HOME/go2cs/toolchains/go1.26.11.linux-<arch>`, or `~/.cache/...` |

A module whose `go` line names a newer Go than the tool converts with is refused with a message that says so:
use the go2cs release built for that Go, or lower the module's `go` line.

Go is needed only to convert. Building, running and deploying the converted C# need .NET alone: the `go.*`
packages are already C#, so neither the build machine nor the machine the program runs on needs Go. Two
conversion steps need more than Go. A `-tests` run also runs `go test`, to compare its results with the
converted tests. A `-cgo` conversion needs a C compiler, but only while it converts: the converted package
calls the C library's functions directly, so building it needs no C compiler, and running it needs the C
library itself.
<!-- Measured 2026-11-24 (P1), linux-x64, master cb6def82a2, .NET SDK 10.0.115. A program using fmt, strings, sort, a
     map, a goroutine and a channel was converted, then built (`dotnet build -c Release -p:GoTargetOS=linux`) and run
     in `env -i` with PATH=/usr/lib/dotnet:/usr/bin:/bin: no go on PATH, GOROOT and GO2CS_GOROOT unset. Traced with
     `strace -f -e trace=execve,openat`: the build executed only dotnet (4) and csc (66), the run only the program;
     neither opened any path under a Go install, the module cache or go2cs's toolchain cache (0 matches). The program
     printed what `go run` prints. A cgo program calling `add` from a shared library libadd.so was converted with
     `-cgo` (gcc as CC), then built the same way: dotnet (4) and csc (50) only, no C compiler executed, and it ran
     with only libadd.so on LD_LIBRARY_PATH, printing what `go run` prints ("add: 42"). -->

## `go2cs toolchain`

```shell
go2cs toolchain install             # download the Go now, for example before going offline
go2cs toolchain install -from FILE  # install it from a release archive or toolchain module zip you have
go2cs toolchain env                 # print GOROOT, GOVERSION and GO, the go command go2cs uses
go2cs toolchain prune               # remove copies of other Go releases; -all removes this one too
```

`install -from` takes the release archive go.dev publishes (`go1.26.11.<os>-<arch>.tar.gz`, or `.zip` on Windows)
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
