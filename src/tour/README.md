# Tour of go2cs

Tour of go2cs places the official, locally hosted **Tour of Go** beside a live
Go-to-C# workspace. The Go lesson remains owned and rendered by the upstream
Tour. A same-origin bridge reads the current editor text and sends that exact
text to the local go2cs server.

The interface is deliberately parallel:

- The original Tour occupies the left two-thirds of the window.
- Generated C# occupies the right third, with a draggable divider.
- **Code** and **Project** tabs show the matching `.cs` and `.csproj`.
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
- **Run with Go** optionally converts, builds, and runs .NET whenever the Go
  program runs.
- **Run** builds and executes the current conversion; it becomes **Kill** while
  the process is active.

![Tour of go2cs showing Go and generated C# side by side](https://go2cs.net/images/tour-of-go2cs.png)

## Requirements

- Go, at the release `src/go2cs/go.mod` names
- .NET SDK 10.0 or later
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
`golang.org/x/website/tour@latest` when needed, and opens
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

`start.ps1` and `start.sh` pass any option they do not recognize straight
through, so `.\src\tour\scripts\start.ps1 -Runtime nuget -solutions-url=` starts
the app with the header link removed.

`GO_TOUR_BIN` can point to the upstream `tour` executable. `GO2CS_BIN` can point
to an explicitly managed prebuilt go2cs executable, which skips the build entirely.

## The first conversion builds go2cs

Otherwise the converter is built into `src/tour/.cache` from the current
checkout, as part of the first conversion — so that conversion can take minutes
while later ones take seconds. A cached executable is reused across restarts
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

The first conversion can take longer because go2cs is built lazily (above). Each submission gets a temporary
Go module; the server runs `go mod tidy`, recursively converts imported third-party packages, and keeps
the generated app/dependency graph isolated from the selected runtime tree. The Code and Project tabs
still show only the submitted app. Converted workspaces expire after 30 minutes. The request body is
limited to 256 KiB; normal tool stages have a 20-second timeout, dependency resolution has two minutes
for an initial download, and the one-time converter build has ten. Aborting the Run request cancels the
active build or program.

## Integration design

The official Tour runs unchanged on a private loopback port. This server
reverse-proxies `/tour/`, `/images/`, and the websocket endpoint, injects only
the source bridge and a small thematic stylesheet, and hosts the outer
interface on port 4000.

The websocket proxy translates the browser-facing origin to the private Tour
origin so the upstream same-origin check remains effective. The server owns
only the go2cs/.NET half. A successful conversion is retained briefly so Run
can build and execute the exact project shown in the Project tab.
