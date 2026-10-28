# Tour of go2cs

Tour of go2cs places the official, locally hosted **Tour of Go** beside a live
Go-to-C# workspace. The Go lesson remains owned and rendered by the upstream
Tour. A same-origin bridge reads the current editor text and sends that exact
text to the local go2cs server.

The interface is deliberately parallel:

- The original Tour occupies the left two-thirds of the window.
- Generated C# occupies the right third, with a draggable divider.
- **Code**, **Package Info** and **Project** tabs show the matching `.cs`,
  `package_info.cs` and `.csproj`.
- The **Generated Code** tab browses what go2cs-gen's source generators wrote
  for the conversion once its build has run: interface implementations,
  pointer-receiver overloads, embedding forwarders, field properties and
  literal interceptors. A back/up bar stays above a folder explorer; opening a
  `.cs` file shows it read-only in the same place, highlighted as the Code tab
  is, and back or up returns to the folder it came from.
- Conversion runs with comments on, so a lesson's own commentary crosses over
  with the code it explains: a Go end-of-line comment stays an end-of-line
  comment, on the statement it annotates
  ([Comments](../../docs/ConversionStrategies-Reference/comments.md#where-a-comment-is-attached-and-where-it-is-not)).
- The **Runtime** selector chooses NuGet packages, a deployed stdlib, or live
  checkout source without changing the Go lesson.
- **Transpile**, **Build**, and **.NET Run** keep their output separate.
- Program output carries the Tour's own colors: what the program wrote to
  standard error is red, the host's notices are muted, and long lines scroll
  instead of wrapping — so both panes break in the same places.
- A picture written by `golang.org/x/tour/pic` appears in **.NET Run** as an
  image, as it does in the Tour.
- The header links to worked exercise solutions; `-solutions-url` retargets or
  removes it.
- Navigating to a Tour page converts it automatically.
- Editing Go marks the C# stale until **Convert** is selected.
- **Run with Go** converts, builds, and runs .NET whenever the Go program runs.
  It is on by default; turning it off is remembered by the browser.
- A successful conversion starts its .NET build at once, in the background, and
  the **Build** tab shows that build while it runs and its output when it ends,
  so **Run** usually finds the build already finished or under way.
- **Run** builds and executes the current conversion; it becomes **Kill** while
  the process is active.

![Tour of go2cs showing Go and generated C# side by side](https://go2cs.net/images/tour-of-go2cs.png)

## Requirements

- Go, at the release `src/go2cs/go.mod` names. With no `go` on `PATH`, the start scripts use the Go the
  go2cs dotnet tool downloads (`go2cs toolchain env`), so installing the tool is enough
- The .NET 10 SDK, the one the repository's `global.json` pins
- A local clone of this `go2cs` repository
- Network access on the first start to install the official offline Tour and restore the go2cs NuGet
  packages, and when an exercise first imports a Go module that is not already in the local module cache

The app binds to loopback by default. It executes editor content as local code,
so it should not be exposed to an untrusted network.

## Start on Windows

From the repository root:

```powershell
.\src\tour\scripts\start.ps1 -Runtime nuget
```

The bootstrap script verifies Go and .NET, installs
`golang.org/x/website/tour` when needed (pinned to a revision that builds with
Go 1.26; see `$TourVersion` in the script), and opens
<http://127.0.0.1:4000>. `-Runtime nuget` builds converted lessons against the
published go2cs packages, so no standard library has to be staged first; the
alternatives are described under *.NET runtime sources* below, and the
**Runtime** selector switches between them without a restart.

To leave the browser closed or use another loopback port:

```powershell
.\src\tour\scripts\start.ps1 -Runtime nuget -NoOpen -ListenAddress 127.0.0.1:4100
```

## Start on macOS or Linux

```sh
./src/tour/scripts/start.sh -runtime=nuget
```

## Direct server options

After installing the upstream Tour yourself:

```sh
cd src/tour
go run . -no-open
```

Useful options:

- `-addr=127.0.0.1:4000`: address for Tour of go2cs
- `-tour-addr=127.0.0.1:3999`: private address for the upstream Tour
- `-repo=/path/to/go2cs`: explicit repository root
- `-runtime=core|deployed|nuget`: initial .NET runtime source (see *.NET
  runtime sources* below for the default order)
- `-deployed-root=/path/to/go2cs`: root created by `deploy-core.ps1`
- `-nuget-source=/path/or/feed`: folder or feed containing go2cs packages
- `-nuget-version=<version>`: package version to restore (default: `GO2CS_NUGET_VERSION`, else `<GoStdLibVersion>.<GoBuildNumber>` from `src/version.props`)
- `-solutions-url=https://…`: target of the header's exercise-solutions link;
  empty removes the link, and only `http`/`https` targets are shown
- `-solutions-text=Exercise solutions`: text for that link
- `-tool-timeout=15m`: budget for the one-time go2cs build described below —
  a duration (`15m`) or a whole number of seconds (`900`)
- `-no-tour`: do not launch the upstream Tour process
- `-no-open`: do not open a browser
- `-no-warmup`: skip the .NET warmup described below, so the first lesson pays
  the cold start itself

`start.ps1` and `start.sh` pass any option they do not recognize straight
through, so `.\src\tour\scripts\start.ps1 -Runtime nuget -solutions-url=` starts
the app with the header link removed. Through `start.ps1`, give a value after a
space (`-nuget-version 1.26.10.1`, `-solutions-url https://...`): PowerShell
splits a `-name=value` argument at a `.` or a `:`.

`GO_TOUR_BIN` can point to the upstream `tour` executable. `GO2CS_BIN` can point
to an explicitly managed prebuilt go2cs executable, which skips the build entirely.

## Startup warms up .NET

The first trip through the pipeline after a start is the slow one: it builds
go2cs, restores the `go.*` packages into the NuGet cache, and starts MSBuild and
the C# compiler. The server makes that trip itself as soon as it starts, in the
background, by converting, building, and running the Tour's own hello-world
program in a scratch workspace of its own — the same stages, runtime, and
environment a lesson uses. The server prints that it is ready, and opens the
browser, only when the warmup ends, so the first lesson a user sees already
runs warm: its first run takes about as long as any later one.

The server answers requests throughout. A page opened during the warmup shows
**Warming up .NET...** in the header while it runs and **.NET ready** briefly
when it ends. A conversion proceeds right away; its
.NET build, like any build that arrives mid-warmup, waits for the warmup to
finish rather than racing it for the same packages, then builds warm. If the warmup fails, the
server logs why and the page shows nothing: the first lesson simply pays the
cold start. `-no-warmup` turns it off.

## The converter is built from the checkout

Unless `GO2CS_BIN` names one, the converter is built into `src/tour/.cache`
from the current checkout, by the warmup (or, with `-no-warmup`, by the first
conversion), so that first build can take minutes while later conversions take
seconds. A cached executable is reused across restarts
only when it is newer than every file under `src/go2cs` and still identifies
itself when run, so a converter left by an older checkout is rebuilt rather than
driven by a newer pipeline.

That build gets ten minutes, as a guard against a hung `go build`, not because
it is expected to take that long. Raise or lower it with
`-tool-timeout` or `GO2CS_TOOL_TIMEOUT` (a duration such as `15m`, or a whole
number of seconds). A build that runs out of time is reported as the **Build
go2cs** stage, not as a failed transpile: the converter never ran, so nothing
was converted.

## .NET runtime sources

When `-runtime` is omitted, the server picks NuGet packages, then a detected
deployed stdlib, then core source — in a plain checkout a package source and
version always resolve, so NuGet is effectively always the default.

**NuGet packages** (`-runtime=nuget`) rewrites the generated project references
to `go.gen`, `go.lib`, and the required `go.*` packages, so lessons build
against the published converted standard library with nothing staged locally.
The server prefers the local `src/artifacts/nupkg` feed when packages exist,
then falls back to nuget.org. The version comes from `src/version.props`.
Override either value with `-nuget-source` / `GO2CS_NUGET_SOURCE` and
`-nuget-version` / `GO2CS_NUGET_VERSION`.

**Deployed stdlib** (`-runtime=deployed`) builds against a staged copy of the
standard library at `$GOPATH/src/go2cs`, produced by:

```powershell
.\src\deploy-core.ps1
```

Override the root with `-deployed-root` or `GO2CS_DEPLOYED_ROOT`. A root only
counts if it has `core/VERSION`, `core/golib/golib.csproj`, and
`gen/go2cs-gen/go2cs-gen.csproj`. Choose this mode to share one staged tree
across multiple apps or machines.

**Core source** (`-runtime=core`) builds directly against the current
checkout's `src/core` — the complete converted standard library as live
source, no staging. Any converter or golib edit is picked up immediately, so
this is the mode for developing go2cs itself.

## Keyboard controls

- `Ctrl`/`Cmd` + `Enter`: convert edited Go
- `Shift` + `Enter`: run the current .NET conversion
- Focus the divider and use `Left Arrow` / `Right Arrow`: resize the panes

## Development checks

```sh
cd src/tour
go test ./...
go vet ./...
```

Building go2cs and the first restore of the `go.*` packages are paid by the warmup (above). Each submission gets a temporary
Go module; the server runs `go mod tidy`, recursively converts imported third-party packages, and keeps
the generated app/dependency graph isolated from the selected runtime tree. The Code and Project tabs
still show only the submitted app. Converted workspaces expire after 30 minutes. The request body is
limited to 256 KiB; the transpile and run stages have a 20-second timeout, the .NET build five minutes,
dependency resolution two minutes for an initial download, and the one-time converter build ten.

The `dotnet build` of a submission starts as soon as its conversion succeeds, in the background, and
the Build stage reports that build. The page asks for it straight after the conversion, so the Build tab
fills in on its own, and once the build passes the page settles on **.NET Run**, ready, just as the Go
side waits for Run (unless the reader has already picked another tab). Run waits on that same request if the build is still running, or goes straight to
the program if it has passed, so a synchronized Go and .NET run pays only for whatever part of the build
is left. The program is launched directly, as `dotnet <assembly>`, at the path the build's own output
names; `dotnet run --no-build` would evaluate the project again before every launch, which on
Windows x64 more than doubled a hello-world run. The program sees the same working directory and environment either
way (its `runtimeconfig.json` sits beside the assembly), and a build whose output names no assembly is
run with `dotnet run --no-build --project` as before. Converting a newer
submission (every Tour page converts itself on arrival) cancels an older one's unfinished build, as
does the expiry of its workspace; should the older conversion still be run, it builds then. Aborting
the Run request cancels the running program; a background build it was waiting for carries on, so the
next Run finds it finished.

The Generated Code tab reads `GET /api/generated?conversionId=<id>&path=<folder or file>`. The converted
project emits its generators' output (`EmitCompilerGeneratedFiles`), and the `Directory.Build.targets` go2cs
writes beside the converted tree routes it to `.artifacts/gen/<project token>`; the tab adds nothing to the
build and only reads that folder, labelling each token folder with the project it belongs to. The endpoint
answers `pending` while the conversion's build runs and `unavailable` before any build has written the folder.
Nothing outside `.artifacts/gen` is reachable: a path that is absolute, contains a backslash or a `..` segment,
crosses a symbolic link, or resolves outside the folder is refused with 400. A listing carries folders and
`.cs` files only, and only a `.cs` file (up to 4 MiB) is served.

## Integration design

The official Tour runs unchanged on a private loopback port. This server
reverse-proxies `/tour/`, `/images/`, `/_/` and the websocket endpoint. It
injects the source bridge and a small thematic stylesheet, starts the Tour in
its dark theme with syntax highlighting on, adds a `go` line to the module the
Tour writes for each lesson, and hosts the outer interface on port 4000.

The websocket proxy translates the browser-facing origin to the private Tour
origin so the upstream same-origin check remains effective. The server owns
only the go2cs/.NET half. A successful conversion is retained briefly, and
built as soon as it is saved, so Run executes the exact project shown in the
Project tab.
