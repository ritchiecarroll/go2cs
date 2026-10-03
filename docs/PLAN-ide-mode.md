# PLAN — go2cs IDE mode: write Go, run and debug as .NET

> **STATUS: GATED EXPERIMENT APPROVED (owner, 2026-10-03).** The owner accepted the scout's recommendations
> ("sound reasonable"): a gated experiment, not a Go-first IDE flagship (Q1). The Phase 0 feasibility spike runs on a
> cloud lane. Phase 1, the golib debugger views, is approved on its own merits (Q8) and queued where it does not block
> the main mission, together with the other items that are useful whatever the spike finds. Everything after the
> decision gate waits for the spike's numbers and the owner's hands-on verdict. The rest of §7's questions (Q2 to Q7)
> keep their recommendations as the working answers and are re-confirmed when each comes due.

## Owner rulings and the queue (2026-10-03)

**The owner's words:** "The IDE scout recommendations sound reasonable. At this point, letting a cloud lane handle
spike sounds like the right move, let's proceed. ... First to queue are the 'useful regardless of spike' items: go
ahead and queue debugger views (where appropriate and non-blocking on main mission)."

| Item | Owner | Train | Why now |
|:--|:--|:--|:--|
| **Phase 0 spike**, cloud half: preflight, E1, E2 (netcoredbg), E5, E6, E6b, E7, E8 (CLI part), E9's measurements and recommendation | C2 (cloud lane) | none: a record, `docs/phase4/SPIKE-ide-mode.md` | Approved; no converter or corpus change |
| Phase 0 spike, interactive half: E2 (vsdbg), E3 (Visual Studio), E4 (Rider), E8 (IDE part) | the owner or a desktop lane | none | After the cloud half, about 2-3 owner hours |
| **Phase 1: golib debugger views** (§3) | G, after R4 | P (after the 1.24.13.4 release) | Serves every go.* consumer in vsdbg and Rider; changes no emission |
| **Incremental `.cs` writes** (Phase 2's corpus-neutral converter item) | the i9, after K and its generator follow-ups | P | Faster re-conversion for every `-recurse` user, IDE mode or not |
| **`go2cs -version`** and **canonical MSBuild diagnostics** (Phase 2/3 converter CLI items) | P2, after its TRAIN N legs | P | Release provenance in bug reports; CI and editor problem matchers |
| **"Consuming converted Go from C#" guide** (Phase 3 docs) | R, after the first-wave pre-flight | with or after the first-wave packages | The real audience (§4) exists today: C# already calls converted Go |

**Coordinator corrections to the scout** (it read the tree at TRAIN N's union, before TRAIN O):
- macOS: release 1.24.13.4 ships `osx-x64` and `osx-arm64` packages (TRAIN O's macOS-flavors seat), and the behavioral
  suite passes on both Apple chips at TRAIN N. The "macOS developers: weak" fit in §4 improves once the walkthrough's
  darwin door (darwin-xsys-libc) closes.
- nugetgo.net launches with the 1.24.13.4 announcement (TRAIN O's docs seat), not in Q1 2027. Phase 3's placement in §6
  gets easier, not harder.

---

*The plan below is the scout's revision 2 (two challenge rounds: 3 blockers, 21 should-fix and 12 nits applied). In-repo facts were read at commit `59ee0d21bf`. External facts are cited by URL in §8. Effort is given in lane-weeks (one lane, one focused week) as ranges, not commitments.*

---

## 1. The answer

**What it is.** A developer edits Go and presses F5, or builds. The Go is transpiled to C# and runs under the .NET debugger. Breakpoints, stepping, stack frames and compile errors land on `.go` lines. This is the TypeScript loop: transpile, run, map back to the source.

**Who it is really for.** The group with a real need is a .NET team that keeps Go as the source of truth for shared code. Their entry point is a C# app that *references* Go code, not a Go `main`.
- C# calling converted Go already works today. A converted package is an ordinary .NET assembly: an exported Go function is a public static method on the package's `<name>_package` class, and the go.* NuGet packages are consumed exactly this way.
- What is missing is a guide to that calling surface, and a build step that keeps the C# twin in sync with the Go.

**Can it be built?** Very likely, mostly from pieces that already exist:
- .NET's `#line` directive carries the Go file and line into the PDB. Razor relies on it. The C# extension already debugs Q# through generated C#: its manifest lists `qsharp` for breakpoints and among the `coreclr` debugger's languages. Stock .NET debuggers need no changes.
- go2cs already computes the C#-line → Go-line table (the position map) and ships it in every converted assembly.
- go2cs already converts a module and its dependencies into a buildable solution (`-recurse=nuget`, with nugetgo mapping and a lock file).

Two earlier assumptions were wrong, and both make the work larger:
- The converter rewrites every `.cs` file it reconverts. Only non-source outputs skip byte-identical writes, so every reconverted project recompiles.
- `#line` numbers lines differently from the position map, so the directive writer is real work, not a dump of the table.

**Should we do it?** Not as a flagship. The TypeScript comparison fails in three ways:
- Go already runs natively everywhere .NET runs.
- Go source cannot call .NET APIs.
- The debugger's variables and expressions stay C#.

Editing Go is also first-class only in VS Code. Visual Studio has no Go support of its own. JetBrains' Go plugin does not run in Rider. GoLand, used by 28% of Go developers, has no .NET debugger.

**Recommendation: a gated experiment, then build integration for C# projects that consume Go. Not a Go-first IDE product.**
1. **Phase 1 stands alone.** Approve the golib debugger views on their own merits. They help every go.* consumer who debugs C# in vsdbg (Microsoft VS Code, Visual Studio) or Rider, and they are not part of the IDE decision.
2. **Thin slice (3.5–6.5 lane-weeks).** The feasibility spike, a source-mapped build mode, and a hand-wired VS Code loop the owner can press F5 on.
3. **Build integration (Phase 3)** only if the E1 latency numbers and the owner's hands-on verdict pass. Its main product is a `<GoModuleReference>` item for C# projects. A Go-`main` template is secondary.
4. **A VS Code extension** only as a showcase after that.

.NET API interop from Go source stays **out of v1**.

| Question | Short answer |
|:--|:--|
| Can it be built? | Very likely. E2 (`.go` breakpoints binding through `#line` in vsdbg and netcoredbg) is the stop gate, with Razor and Q# as precedent. Still open: E1 (latency), E8 (build shape), E9 (where the `#line` text is produced) and the Visual Studio and Rider breakpoint gutters (E3, E4). The Phase 0 spike settles them. |
| Should it be built? | As a gated experiment, yes. As a product, only as build integration for C# projects that consume Go, and only if the spike's numbers and the owner's hands-on verdict pass. No Go-first IDE flagship unless people outside the fleet ask to call .NET from Go. |
| Licence blockers? | None. vsdbg cannot be bundled and refuses to run in VS Code forks, so forks need netcoredbg (MIT), which shows golib values raw. Bundled converter binaries are AGPL and ship with their licence text and a source link. |
| Cost | Phase 1 (standalone): 1–2 lane-weeks. Thin slice: 3.5–6.5. Through build integration: 7.5–14.5. Through a VS Code extension: 9.5–17.5. Then about 2–4 a year to keep it working, plus one owner signing hand per release. Interop is a separate, multi-month question. |
| When | Phase 1 and the spike: any small window. Thin slice: next, and off converter source if E9 picks a build-time pass. Build integration: can start before Q1 2027, because the converter side of nugetgo already exists; gated on E1 and the hands-on verdict. Public preview: no earlier than the Go 1.26 hop, and honest only if the corpus then hops every Go release. |

---

## 2. How it would work end to end

| # | Step | What happens | Component (owner) |
|:--|:--|:--|:--|
| 1 | **Set up once** | **Primary:** a C# project adds `<GoModuleReference Include="../shared" />` (the analogue of Grpc.Tools' `<Protobuf>` item) through a `PackageReference` to the build package. **Secondary:** a Go-`main` project beside `go.mod`, from `dotnet new go2cs-app`. | Build package + template (new) |
| 2 | **Edit Go** | VS Code with the official Go extension (gopls) is the primary documented editor. Visual Studio and Rider have no first-party Go editing; there the user mostly edits C#, and the Go is a referenced input. See the per-IDE matrix below. | Go extension (not ours) |
| 3 | **Trigger** | F5 in VS Code, Visual Studio or Rider; `dotnet build`, `dotnet run`, `dotnet watch`, CI. Design-time builds (project load, edits) never convert. | IDE or CLI |
| 4 | **Decide what to transpile** | A content-hash stamp over every input that changes the output (listed below) skips conversion when nothing changed. Visual Studio's fast up-to-date check is told about `.go` inputs (`UpToDateCheckInput`) and the stamp (`UpToDateCheckBuilt`), and `dotnet watch` gets `.go` as `Watch` items. Without those, both skip the conversion and run stale output. The converter skips byte-identical writes for project files, `.slnx`, icons, readmes, embeds and the platform merge, but **rewrites every converted `.cs`**, so every reconverted project recompiles. Phase 2 fixes that. | Build package (new); one small converter fix |
| 5 | **Transpile** | `go2cs -recurse=module,nuget` into `obj/go2cs/<Config>/`. nugetgo-mapped dependencies restore as packages. One conversion at a time per output root (a lock). The output directory is always passed, `GOROOT` is taken verbatim from `go env GOROOT`, and `GOTOOLCHAIN=local` is forced. Debug builds get the source-mapped (`#line`) text; Release builds get the default emission. | Build package + converter |
| 6 | **Build** | Roslyn and go2cs-gen compile against one exactly pinned tuple: converter build, `go.lib`, `go.gen` and `go.<pkg>` revision. | Existing, plus pinning |
| 7 | **Errors on Go lines** | Three kinds (see below). | Converter (format), build package |
| 8 | **Run and debug** | The debugger launches the built assembly. Breakpoints set in `.go` bind through the PDB, stepping moves by Go line, and the call stack shows `.go:line`. Locals and Watch show C# names and golib types. | vsdbg (Microsoft VS Code, Visual Studio), Rider's own debugger, netcoredbg (forks) |
| 9 | **Read values** | Phase 1's `DebuggerDisplay`/`DebuggerTypeProxy` make `slice<T>`, `map<K,V>`, `@string`, `ж<T>` and `channel<T>` expand like Go values in vsdbg and Rider. netcoredbg supports neither attribute, so forks see raw golib internals. | golib (standalone Phase 1) |
| 10 | **Runtime traces** | **Positions:** under `#line` the PDB already names the `.go` file and line. The position-map lookup, keyed by the `.cs` file name, misses, and the runtime reports the PDB's position, which is Go's. To be confirmed by E7. **Function-literal names:** the same lookup misses, so closure frames fall back to the Roslyn-derived ordinal, which matches Go's `Outer.funcN` only by coincidence. Phase 2 adds an identity record per Go file so the names stay right. Path separators are already normalized to `/` in every mode. | golib/runtime (one addition, one test) |

**Stamp inputs (step 4).** A content hash, not timestamps, over:
- every `.go` file in the load closure, including `replace` targets and `go.work`;
- GOOS, GOARCH and RID, and build tags;
- converter flags (source-mapped mode on or off);
- the GOROOT Go version, `GOFLAGS` and `CGO_ENABLED`;
- the companion C# folder's contents;
- the resolved package tuple;
- the converter's own identity (the `-tests` digest already computes an exe hash, or `vcs.revision`).

**How `#line` numbers lines (steps 5 and 8).** `#line N "file"` numbers the *next* line N and every later line N+1, N+2… until the next directive. The position map works differently: every C# line up to the next record answers the earlier record's Go line. A naive dump of the table therefore puts three kinds of line on the wrong Go line, sometimes past the end of the file: continuation lines of a multi-line statement, unhidden lines between records, and method or lambda closing braces (which carry sequence points in Debug). The writer rules:
- `#line N "/abs/path/file.go"` before each mapped statement's first line.
- After that line, either re-anchor (`#line N` again) on every C# line whose Go line is not the previous line + 1, or emit `#line hidden` right after the statement's first line.
- `#line hidden` for the second and later statements of one Go line.
- Scaffolding (usings, aliases, lifted helpers) goes under `#line default`, then `#line hidden`. Diagnostics inside a hidden block still report the *current* mapping. Without `#line default`, a scaffolding error after `#line N "x.go"` would show as `x.go(N+k)`; with it, the error stays on the `.cs` file.
- Closing braces are either hidden or mapped to Go's closing-brace line. The second needs `go/token` End positions, which today's line-only table does not carry.
- `#pragma checksum` with the `.go` file's SHA-256.
- Never a directive inside a verbatim, raw or multi-line string literal.

The C# 10 span form (`#line (l,c)-(l,c) "file"`) does not change this: lines after the first are still numbered relative to the directive.

**The three kinds of error (step 7):**
1. **Go errors.** gopls shows them in VS Code. The converter's own type-check failures print in MSBuild's canonical `file(line,col): error CODE: text` form.
2. **Converter refusals**, such as `import "C"`, in the same form.
3. **C# errors in generated code.** Under `#line`, errors in mapped code move onto the Go line, and scaffolding errors stay on the `.cs` file. Labelling them as go2cs defects works differently per IDE:
   - VS Code: a `$go2cs` problem matcher sets its own source, so these can read "go2cs defect, not a Go error".
   - Visual Studio and Rider: csc diagnostics reach the Error List through the build logger, and a project cannot rewrite their text. On a Go line, the CS-prefixed code is the only marker.

**Components**

| Component | Home and licence | Already exists | New |
|:--|:--|:--|:--|
| Converter changes | `src/go2cs`, AGPL-3.0-only + Output Exception | Sentinel → table pipeline (`positionMapOperations.go`); `-recurse` / `=module` / `=nuget`; nugetgo mapping and `go2cs.nuget.lock`; `-tests` input digest and converter identity | `.cs` writes through the byte-identical skip; `go2cs -version`; canonical diagnostics; the `#line` writer if E9 picks option (a) |
| Build package (working name `Go2cs.Sdk`) + `dotnet new` template | New folder, MIT | Generated project shape: `Directory.Build.props/targets` with `.artifacts` redirection, per-project `.slnx`, NuGet references | `GoModuleReference`, targets, stamp, design-time and up-to-date wiring, locks, version pin, RID selection, converter delivery, compare target, headless gate; the `#line` pass if E9 picks (b) or (c) |
| VS Code extension | New, MIT | Nothing. The Tour's browser loop is the nearest relative. | Debug type, program resolution, task provider, problem matcher, "Show C#" view |
| golib debugger views (standalone) | `src/core/golib` and `src/gen/go2cs-gen`, MIT | `ToString` on slice, map, string, array and `ж<T>` | Type proxies, display attributes, channel `ToString`, goroutine thread names |
| Docs | `docs/`, MIT | README `-recurse` walkthrough, Tour | "Consuming converted Go from C#" (first), "Run your Go on .NET", a statement of the supported surface, troubleshooting |

**Per-IDE matrix**

| IDE | Edit Go | Transpile on build | Debug on `.go` lines | Scope |
|:--|:--|:--|:--|:--|
| VS Code (Microsoft) | Strong: official Go extension + gopls | Phase 2.5 tasks, then Phase 3 | vsdbg through the C# extension (E2); readable values after Phase 1 | The primary documented Go-editing loop |
| VS Code forks (Cursor, VSCodium, Windsurf) | Go extension from Open VSX | Same | netcoredbg only (4b): raw golib values; no Intel-macOS or Windows-on-ARM release builds | Optional, 4b |
| Visual Studio | No first-party Go support; third-party LSP only | Phase 3 (MSBuild-native) | vsdbg; `.go` gutter breakpoints unknown (E3) | The C#-consumer scenario |
| Rider | JetBrains' Go plugin runs only in IntelliJ IDEA Ultimate and GoLand. Third-party gopls bridges exist (Go Portable, Go Language Helper on LSP4IJ). | Phase 3 | Rider's debugger; `.go` breakpoints unknown (E4) | The C#-consumer scenario |
| GoLand (28% of Go developers) | Strong | No .NET build | No .NET debugger | Unserved |

---

## 3. Work breakdown

| Phase | What | Effort (lane-weeks) | Can start when |
|:--|:--|:--|:--|
| 0 | Feasibility spike (E1–E9) | 1–1.5 | Now, on a box with no battery running (preflight below). No corpus or converter change. |
| 1 | golib debugger views. **Standalone:** not counted in the IDE cost. | 1–2 | A golib/go.gen window opens. Ships with the next package release. |
| 2 | Source-mapped build mode (design per E9), plus incremental `.cs` writes | 2–4; +1–2 for column spans (2b) | E2 is green and E9 is chosen. If E9 picks (a), the owner also answers Q2. |
| 2.5 | Hand-wired loop: a VS Code recipe or a `go2cs run` wrapper | 0.5–1 | Phase 2 is done. |
| — | **Decision gate:** the E1 numbers and the owner's hands-on verdict on Phase 2.5 | — | — |
| 3 | Build integration: C#-consumer `GoModuleReference` first, Go-`main` template second | 4–8 | The gate passes, and the uuid pilot's package exists (a local feed is enough). |
| 4 | VS Code extension | 2–3; +1–2 for a fork build with netcoredbg (4b) | Phase 3 passes its exit test. |
| Keep-alive | Keeps Phases 2.5–4 working across go2cs releases, Go hops, editor releases and three OSes | 0.25–0.5 per release (about 2–4 a year), plus one owner signing hand per release | From the first shipped package or extension. |
| 5 | Optional extras | 0.5–4 each | Evidence of demand per item. |
| X | **.NET API interop from Go source** | 10–20+ for a first useful subset; open-ended after | Evidence and a DESIGN doc. **Out of v1.** |

### Phase 0: Feasibility spike

**Scope.** Answer the mechanics questions before writing product code. Insert directives by hand into converted samples; the converter does not change.

**Preflight**, before any experiment runs on a cloud lane: enough free disk for a go.* closure restore plus builds, nuget.org reachable, and a .NET 10 SDK installed. If any is missing, run the experiment on a desktop box with no battery running, into its own output root.

| # | Settles | Who can run it |
|:--|:--|:--|
| E1 | **Latency, with the prediction written down before measuring:** warm edit-to-breakpoint at most 5 s on hello-world and at most 15 s on a 10-package module. Above twice those, drop the F5-on-Go positioning. The C#-consumer build integration can live with a slower loop, because Go edits there are occasional (a judgment). The yardstick is `go run` (about a second for a small program, warm), not tsc. Use samples that convert today: the README's `fatih/color` demo, the hashset module, and a synthetic N-package module. Time the changed-package path separately from the whole-module path, run both with and without the `.cs` write fix (simulated with a staging copy), and report the cold first restore of the go.* closure separately. | A cloud lane, after preflight |
| E2 | `.go` breakpoints bind through `#line` in vsdbg and in netcoredbg. The stack shows `.go`. `requireExactSource` works with `#pragma checksum`. The netcoredbg half also records how golib values display there, including whether golib's `ToString` overrides show. Precedent: Q# in the C# extension. | netcoredbg half: a cloud lane (scriptable over DAP). vsdbg half: interactive Microsoft VS Code. |
| E3 | Visual Studio: F9 in a `.go` file sets and binds a breakpoint. | Interactive Visual Studio |
| E4 | Rider: `.go` breakpoints without a plugin. | Interactive Rider |
| E5 | Hot Reload through regenerated `.cs` under `dotnet watch`, including whether edits to the assembly-level `GoPositionMap` attribute strings apply. | A cloud lane |
| E6 | Stepping density on a loop/defer/goroutine sample: every statement mapped against first statement per Go line with the rest hidden. Includes multi-line statements and closing-brace stops. | Mostly a cloud lane |
| E6b | The line-model check: hand-insert directives on a sample with a multi-line call, a hoisted temp and a collapsed expression-bodied lambda, and confirm the breakpoint lines under the re-anchor rule. | A cloud lane |
| E7 | `runtime.Caller` under `#line`. Position half: no double mapping, and the file identity becomes the absolute `.go` path, which matches Go's untrimmed build. Name half: function-literal frame names, including a closure inside a closure. | A cloud lane |
| E8 | The build shape (Phase 3, risk 1): can one project restore, build and F5 the multi-project output in Visual Studio and Rider, in both the C#-consumer shape and the Go-`main` shape? Does Visual Studio's fast up-to-date check rebuild after a `.go` edit once told about `.go` inputs? Does loading the project in Visual Studio or Rider trigger no conversion? | Partly a cloud lane; Visual Studio and Rider interactive |
| E9 | Where the `#line` text comes from (options below). Chosen before any Phase 2 code is written. | A cloud lane, then an owner ruling |

**E9 options**

| Option | Line fidelity | go2cs-gen output | Licence home | Published packages | Converter source freeze |
|:--|:--|:--|:--|:--|:--|
| **(a)** A converter flag (`-linedirectives`) writes directives during the walk | Best. The walk knows statement starts, continuations and scaffolding, and could carry End positions and columns (2b). | Not covered (no Go lines; keep it hidden) | AGPL converter | Need reconversion | Collides during batteries; needs the Q2 ruling |
| **(b)** An MSBuild task in the MIT build package decodes `GoPositionMap` from `package_info.cs` and compiles a transient `#line` copy. This is the banked BOARD design. | Line-only and statement-level. It can re-anchor every line to its predecessor record, but cannot tell scaffolding from continuation. No columns, no closing-brace lines. | Not covered | MIT build package | Any locally converted tree; not the compiled go.* packages | None. The default emission is untouched, so Q2 is moot. |
| **(c)** A post-build rewrite of portable-PDB sequence points from the table | Same as (b). The `.cs` and IL stay byte-identical, and compile errors stay on the `.cs` (no Go-line errors). | Not covered | MIT build package | Wherever the assembly's PDB is available (whether go.* packages ship one is to be checked) | None; Q2 is moot. |

A hybrid, (b) or (c) fed by a richer table that marks statement starts, changes the committed `package_info` and so needs the same ruling as (a). **Preliminary lean, to be confirmed by E6 and E6b:** (b) for the thin slice, because it is MIT, avoids freeze collisions and works on today's trees. Move to (a) only if the line-only model steps badly.

- **Exists:** the position map gives the line numbers. The Tour pipeline shows the loop. A golib test already compiles `#line` regions (`CallerFrameTestVariantNamingTests.cs:336-361`).
- **New:** throwaway samples and a one-page result.
- **Risks:** the safety floor. Run on a box with no battery running, into its own output root.
- **Exit test:** each experiment is answered green or red, with screenshots for E2–E4 and numbers for E1.
  - If E2 is red in both debuggers, the plan stops.
  - If E1 is above twice the prediction, drop the F5-on-Go positioning; keep only the C#-consumer build integration, if wanted.
  - If only E3 or E4 is red, that adds a Phase 5 plugin.

### Phase 1: golib debugger views (standalone)

**Scope.** Readable golib values in vsdbg (Microsoft VS Code, Visual Studio) and Rider's debugger. The audience is every go.* NuGet consumer who debugs C# holding slice, map, string, pointer or channel values. This has nothing to do with IDE mode, changes no emission (consistent with the owner's ruling), and is justified on its own. It sits here because it shares the debugging theme, but its cost is **not** in the IDE totals and its value is not part of the IDE verdict. netcoredbg, the forks' debugger, supports neither `DebuggerDisplay` nor `DebuggerTypeProxy`.

**Homes:** `go.lib` (`src/core/golib`) and `go.gen` (`src/gen/go2cs-gen`). Both packages change in the release.

**Deliverables:**
- `DebuggerTypeProxy` + `DebuggerDisplay` for:
  - `slice<T>`: len, cap and elements, not the backing-array internals.
  - `array<T>`.
  - `map<K,V>`: key/value pairs.
  - `@string`: the text, with bytes on expand.
  - `ж<T>`: the pointee.
  - `channel<T>`: len, cap, closed.
- A `ToString` for `channel<T>`, which has none today, so a debugger without display attributes still shows something useful.
- Step-over attributes aimed at generator output compiled into the user's assembly: defer frames, goroutine launch, and any shell kinds not yet marked. The interface-shell forwarders already carry `DebuggerNonUserCode`.
  - `go.lib` ships as an optimized Release build, and Just My Code (on by default in Visual Studio and vsdbg) already treats optimized code as non-user code. NuGet consumers therefore already get the step-over effect inside golib. Attributes there matter only for fleet builds of golib from source in Debug.
  - Choose `DebuggerNonUserCode` over `DebuggerStepThrough` for frames that call user code, based on the exit checks.
- Goroutine thread names ("goroutine N"), set once at thread creation in `Goroutine.cs`. The scheduler design calls for them (`DESIGN-cooperative-scheduler.md:309-310`), but the code never sets them. Every goroutine gets its own dedicated thread by design, not a ThreadPool thread, so there is no reused-thread renaming problem.

**Exists:**
- `ToString` overrides on slice, map, string, array and `ж<T>`.
- `DebuggerNonUserCode` on 2 golib types (`GoexitException`, `PanicException`) and 1 method (`error.cs`).
- `PanicException` hides 15 members with `DebuggerBrowsable(Never)`.
- go2cs-gen marks the interface-shell forwarders `DebuggerNonUserCode` at 2 sites.
- No type proxies or `DebuggerDisplay` anywhere.

**New:** attributes and small proxy classes.

**Risks:**
- golib is shared by everything. The change rides a package release and must never land while a battery is running.
- A proxy must not allocate heavily, or take locks that hang the debugger on a huge or concurrently written map.

**Exit test:**
- Unit tests on each proxy's members.
- A manual checklist in Visual Studio, Microsoft VS Code and Rider: slice, map, string, pointer and channel each expand like Go.
- Step Into on `defer f()` and on `go f()` lands in `f`. F11 on an interface method call skips the shell.
- Behavioral suite unchanged.

### Phase 2: Source-mapped build mode

**Scope.** Produce `#line`-mapped C# for Debug builds from the design E9 picks, with the default emission unchanged. Plus one small converter fix the whole loop needs.

**Deliverables:**
- **Incremental `.cs` writes** (converter, corpus-neutral). Route `writeOutputFile` through `needToWriteFile`, as every other output already is, so an unchanged package's `.cs` keeps its timestamp.
  - Prove no content drift with `check-no-regression.ps1`, and prove the timestamp effect with a test that an unchanged reconvert leaves every `.cs` untouched.
  - Fallback if it cannot land in time: the build package converts into a staging folder and copies only changed files.
  - It can land in any converter window outside a battery freeze, ahead of the rest of the phase.
- **The directive writer**, following the rules in §2: re-anchor or hide after each statement's first line; scaffolding under `#line default` + `#line hidden`; a decision on closing braces; absolute `.go` paths; `#pragma checksum` (SHA-256); never inside a string literal.
- **Function-literal names under `#line`.** An identity-table position-map record per Go file, keyed by its `.go` file name: each line maps to itself and carries the same function-literal spans. The runtime's literal-naming lookup then resolves without double mapping.
- **A per-construct audit matrix:** multi-line expressions, hoisted temps, closures and collapsed lambdas, defer/finally, switch lowering, goto and labels, select.
- **A scripted stepping oracle** over a fixed sample, driving netcoredbg over DAP. Phase 3's headless gate reuses it.
- Converter diagnostics in canonical MSBuild format.
- Tests:
  - The default emission stays byte-identical, proved with `check-no-regression.ps1`.
  - Whole-corpus invariant 1: source-mapped output with its directive lines deleted is byte-identical to the default emission.
  - Whole-corpus invariant 2: a Roslyn parse of every source-mapped file finds exactly as many line-directive trivia as lines were inserted, so no literal swallowed one.
  - Each invariant is made to fail once on purpose before it is trusted (safety floor #13).
  - A deliberate C# compile error on a statement line and another on a continuation line, each reported at the right `.go` line. A scaffolding error is reported on the `.cs`.
  - E7 as a behavioral-style probe, including closure-in-closure names.
  - A repoguard that no committed `.cs` or golden carries `#line`.

**Sub-phase 2b (optional).** The C# 10 span form `#line (l,c)-(l,c) "file"` gives a precise step highlight. It needs go/token columns and End positions, which today's line-only table does not carry.

**Exists:**
- The sentinel and table (`positionMapOperations.go:23-35`). Sentinels are written at statements and function declarations.
- The arc was already priced (BOARD 13676-13714). This exact idea was banked as an opt-in, transient, build-time pass generated from the position-map tables (BOARD 16949-16967).
- The runtime already tolerates `#line`, and it normalizes path separators in every mode.

**New:** as listed above.

**Risks:**
- **Stepping density and line-model fidelity.** The Q# team reported wrong binding and wrapper frames in the call stack. The writer rules and the stepping oracle decide the experience.
- **The generated `.cs` cannot take breakpoints in this mode.** That is acceptable because the mode is opt-in. Fleet root-causing keeps using the default mode.
- **go2cs-gen output is out of reach under every option.** It has no Go lines and must stay hidden or marked as non-user code.
- **Under option (a) only:** converter source freezes during batteries, and the Q2 ruling.

**Exit test:**
- On a sample module, the stepping oracle shows that every Go statement line takes a breakpoint and steps once, multi-line statements included, and that no stop lands past a file's end.
- The default-mode corpus diff is empty. Both invariants are green, and each has failed once on purpose.
- The deliberate-error tests pass.

### Phase 2.5: Hand-wired loop

**Scope.** The cheapest loop someone can actually run. This is TypeScript's own recipe from before editors had plug-ins: a `preLaunchTask` that converts and builds, plus a `coreclr` launch configuration. No build package, no extension.

**Deliverables:**
- A documented VS Code recipe:
  - `tasks.json` runs go2cs (output directory passed, `GOTOOLCHAIN=local`), then `dotnet build` with the `$msCompile` matcher.
  - `launch.json` of type `coreclr` points at the built assembly. The path comes from `dotnet msbuild -getProperty:TargetPath`, because generated output lands under `.artifacts/bin/<12-character hash>/` and is not guessable.
- Or a tiny `go2cs run` wrapper that does the same and that IDE tasks call. The wrapper is also Phase 3's guaranteed fallback.

**Exit test.** The owner presses F5 on a sample, stops at a `.go` breakpoint, steps, reads a value, and writes a one-paragraph hands-on verdict. That verdict and the E1 numbers gate Phase 3.

### Phase 3: Build integration (the real "TypeScript step")

**Scope.** `dotnet build`, `dotnet run`, `dotnet watch`, F5 in Visual Studio and Rider, and CI all transpile Go first, with no plug-in. The primary scenario is a C# app that references a local Go module. The analogues are Grpc.Tools' `<Protobuf>` item and Microsoft.TypeScript.MSBuild.

**Deliverables:**
- **Primary: `<GoModuleReference Include="../shared" />`** for C# projects. The conversion produces one project per Go package, and the C# project references them, either by injecting `ProjectReference` items at restore time or through the wrapper. E8 decides.
- **A "Consuming converted Go from C#" guide:** `string` and `@string`, slices against arrays and `Span<T>`, multiple returns, error values, pointers (`ж<T>`). Price any small facade or helper work it shows is needed.
- **Secondary:** a `dotnet new go2cs-app` template for a Go-`main` sample.
- **Build hygiene:**
  - Skip conversion when `'$(DesignTimeBuild)' == 'true'` or `'$(BuildingProject)' != 'true'`. Design-time builds run on project load and on project changes in Visual Studio, Rider and C# Dev Kit; `dotnet watch` runs one too, with `DotNetWatchBuild=true`.
  - Declare `**/*.go`, `go.mod` and `go.sum` as `UpToDateCheckInput` and the stamp as `UpToDateCheckBuilt`, and add `.go` files as `Watch` items.
  - The content-hash stamp over the full input list in §2.
- **Safety on users' machines.** The repo's own safety-floor failure modes now run unattended:
  1. A lock (named mutex or lock file) per output root and per shared cache, so an IDE build, `dotnet watch`, a CLI build and a second IDE instance never run two conversions into one root.
  2. Separate output roots per configuration (`obj/go2cs/<Config>`), since Debug and Release convert differently.
  3. `GOROOT` taken verbatim from `go env GOROOT`. A forward-slash spelling misroutes the whole emission and still reports success.
  4. `GOTOOLCHAIN=local` forced. Otherwise opening a project can silently download and run a newer toolchain.
  5. The output directory always passed.
  6. VS Code Workspace Trust honoured.
- **Exact version pin.** Each package version pins one tuple: converter build, `go.lib`, `go.gen` and `go.*` revision.
  - The package writes `GoStdLibVersion` exactly. Today `-recurse=nuget` writes `1.24.13.*`, which floats to the newest NuGet revision; publishing a new go.* revision would silently change what yesterday's project runs.
  - The converter hash goes into the stamp, and a mismatched converter is refused with a message naming both versions.
  - This needs a `go2cs -version` flag that prints the release tuple. None exists today; it is a small converter item.
- **Converter delivery** (Q4): prebuilt RID-specific converter binaries, as a dotnet tool or inside the build package, shipping the AGPL text and a source link at the matching tag.
- RID-aware conversion (`win-x64` or `linux-x64` flavour).
- Go toolchain handling: a clear message when `go.mod` asks for a newer Go than the corpus release.
- **A "Compare with go run" target.** It runs both, diffs stdout, stderr and the exit code, and labels any difference "go2cs divergence". It builds on the `-tests` compare machinery, which today diffs test verdicts by name. Every F5 error banner links the supported-surface statement (ValidatedTestPackages).
- **A companion C# folder** per Go package, documented only with an explicit stability statement (no stability, or a named small surface). The package records the Go file's hash in the companion's header and warns when the Go file changes.
- **A headless end-to-end gate.** It drives netcoredbg over DAP: build the sample, break at `main.go:N`, assert that the top frame is `.go:N`, read a slice local. It runs on Windows, Linux and macOS CI legs for every release and every hop, and is made to fail once on purpose.
- The docs.

**Exists:**
- `-recurse=nuget` and `-recurse=module`, which combine as `-recurse=module,nuget`.
- nugetgo mapping, on by default, with the `-nuget-map*` flags and `go2cs.nuget.lock` (SHA-512 pins). Not yet exercised end to end: a mapped package that itself depends on another third-party package.
- `Directory.Build.props/targets` with artifacts redirection.
- The byte-identical skip for non-source outputs (`projectFileWriter.go:750-758`). The `.cs` gap is Phase 2's fix.
- The `-tests` input-digest pattern (`testConversion.go:6409-6413`) and converter identity (`converterRevision`: exe hash or `vcs.revision`).
- The hand-own probe on every output path (`conversionDriver.go:313-339`).

**New:** everything on the MSBuild side, plus `go2cs -version`.

**Risks:**
1. **The output is many projects, not one set of files.** go2cs emits one project (one assembly) per Go package, each with its own NuGet references.
   - The Grpc.Tools pattern (add generated `.cs` to the current project's `Compile` items) fits only a single-package module.
   - Multi-package modules need one of two shapes:
     - a project that converts, restores and builds the generated projects through the MSBuild task, and references or launches the result (precedent: Microsoft.Build.Traversal); or
     - the two-step wrapper (`go2cs run` or a dotnet tool) that IDE tasks call.
   - NuGet restore runs before targets, so the generated projects need their own restore pass, or the conversion has to run at restore time.
   - This is the phase's main design decision. E8 reduces the risk; the wrapper is the guaranteed fallback.
2. **Go release lock.** Modules must resolve to the corpus release (`docs/README.md:205`, `276-278`). Many current modules and recent `golang.org/x/*` versions need a newer Go. Until the hops land, a build of a modern Go module fails at step 4 with a clear message rather than a mystery.
3. **Brand risk.** C# compile errors in generated code read as the user's fault; §2 says how each IDE labels them. Worse is a difference at run time: a user stepping through "their Go" that behaves differently will conclude their Go is wrong.
   - Causes include a converter gap, a throwing stub, or golib's stated residual: an ordinary channel or sync deadlock blocks instead of raising Go's fatal "all goroutines are asleep" error (`Goroutine.cs:850-853`).
   - Source-mapped debugging hides the C# layer, which makes this worse.
   - Mitigation: the compare target, the supported-surface link, a bug-report path.
4. **macOS.** Developers there load the Windows flavour of platform-varying packages until the macOS run layer lands.
5. **Version skew.** Covered by the exact pin.
6. **Companion-file drift.** Companion files bind to converted-code internals: the package-class layout, `@string`, `slice<T>` and `ж<T>` shapes, `Ꮡ`-prefixed names, interface shells. Those change across releases (the ж-box reduction, the IfaceShell work), and a whole-file replacement silently drifts from its Go file.
   - Mitigation: the stability statement and the hash warnings.
   - Prefer a function-level replacement mechanism on a small documented surface over whole-file replacement. That is a design item: today's only function-level seam, the bodyless `partial`, breaks native Go builds.

**Exit test.** The primary sample is a C# app that references a local Go module with one third-party dependency:
- The C# app calls an exported Go function. Editing the `.go` file and rebuilding changes the result.
- Edit a `.go` file, press F5 in Visual Studio, and see the change.
- Loading the project in Visual Studio or Rider triggers no conversion.
- A rebuild with no changes skips conversion.
- A planted Go type error and a converter refusal both land on the `.go` line, in Visual Studio's Error List and in VS Code's Problems.
- In Debug, F11 from the C# call lands in the `.go` source. `.go` gutter breakpoints work in Visual Studio and Rider if E3/E4 were green; otherwise function breakpoints do.
- The headless gate is green on all three OS legs and has failed once on purpose.
- Secondary: in the Go-`main` sample, a `.go` edit shows up in `dotnet run`.

### Phase 4: VS Code extension

**Scope.** One small extension. Editing stays with the official Go extension.

**Deliverables:**
- A debug type `go2cs` ("Go (.NET via go2cs)"). Its configuration provider resolves to `coreclr`, the pattern the Docker extension uses, so the C# extension's vsdbg does the debugging in Microsoft VS Code.
- Program resolution: read `TargetPath` (`dotnet msbuild -getProperty:TargetPath`) or a launch manifest that the build package writes, because output lands in a hashed `.artifacts/bin/<token>/` folder.
- `contributes.breakpoints` for `go` only as a fallback when the Go extension is absent; the Go extension already contributes it.
- A task provider (`go2cs: build`, `go2cs: watch`) that drives Phase 3.
- A `$go2cs` problem matcher, alongside `$msCompile`.
- A "Run / Debug on .NET" code lens on `func main`.
- A read-only "Show C#" side panel, scroll-synced through the position map. It succeeds the Tour as the showcase.
- `untrustedWorkspaces` declared unsupported, so nothing converts in Restricted Mode.
- The Marketplace build depends on the C# extension, not on C# Dev Kit.

**Sub-phase 4b (optional).** An Open VSX build for Cursor, VSCodium and Windsurf that bundles netcoredbg through a `DebugAdapterDescriptorFactory`.
- Forks get Go-line stepping but raw golib values, because netcoredbg supports neither `DebuggerDisplay` nor `DebuggerTypeProxy`.
- netcoredbg's latest release (3.2.0-1092, June 2026) ships builds only for Linux x64 and arm64, macOS arm64 and Windows x64. Intel macOS and Windows on ARM need a source build or stay unsupported.
- SharpDbg (MIT; supports both attributes) is an alternative worth evaluating.

**Exists:** nothing on the extension side.

**New:** all of it. It is TypeScript-only and touches neither the corpus nor the converter, so it suits a cloud lane.

**Risks:**
- API churn: VS Code has shipped stable releases weekly since 1.111 (March 2026), and the C# extension changes its debugger plumbing.
- Collisions with the Go extension. Avoid them: no language registration, no language server, a distinct debug type, a separate diagnostics collection.
- The vsdbg licence: never bundle it, never enable it in forks.

**Exit test:**
- On a clean machine with only the Go and C# extensions installed: install, open the sample, press F5, stop at a `.go` breakpoint, step, and read a slice in Variables (under vsdbg).
- Extension tests (`@vscode/test-electron`) cover configuration resolution, program resolution and tasks.

### Phase 5: Optional extras (each waits for evidence)

| Item | Trigger | Effort |
|:--|:--|:--|
| Visual Studio VSIX that lets `.go` files take breakpoints | E3 red, and demand from Visual Studio users | 1–2 |
| Rider plugin for `.go` breakpoints | E4 red, and demand from Rider users | 2–4 |
| Hot Reload | E5 shows common edits apply. Line shifts rewrite the `GoPositionMap` attribute strings. Roslyn lists adding and modifying custom attributes as supported since VS 17.0, but assembly-level attributes are unconfirmed (E5), and under `#line` the records could be omitted. `dotnet watch` watches only `Watch` items (by default `Compile` and `EmbeddedResource`), so `.go` files must be added and must re-run conversion. | Spike 0.5–1, then to be decided |
| SourceLink and Go-mapped stdlib symbols: F11 into Go's real stdlib at the pinned tag (the BOARD idea) | Demand from package consumers. It changes how go.* PDBs are built, so it is a release decision. | 2–4 |
| "Run test on .NET" through `-tests` in the Test Explorer | Phase 4 adoption | 1–2 |
| Go-syntax Watch expressions (a DAP proxy) | Strong demand. Clean only over netcoredbg; proxying vsdbg is a licence grey area. | Large; not recommended |

### Phase X: .NET API interop from Go source (the large open question)

**What it would mean.** Go source names .NET types, for example `import "dotnet/System/IO"`. The code type-checks under gopls and converts to direct C# member access.

**Today, nothing exists:** no pseudo-import, no `//go2cs:` directive, no mapping from .NET types to Go types. The only seams need hand-written C#:
- A bodyless Go function becomes a C# `partial`, filled in by a `*_impl.cs` companion (`visitFuncDecl.go:1347-1356`).
- A whole-file hand-own (`[module: GoManualConversion]`) is never overwritten.
- Any extra `.cs` file in a package folder compiles into that package (`csproj-template.xml:141-143`).

**What the full version takes.** Each item is a design problem, not a task:
1. **A binding generator.** It reads .NET reference assemblies and emits Go stub packages: real Go with panicking bodies, so gopls and vet keep working. This is the `.d.ts` analogue.
2. **Mapping rules:**
   - overloads become name suffixes;
   - properties and indexers become accessors;
   - .NET generics map to Go generics only where the constraints allow;
   - exceptions become panics, with a helper that recovers them into `error`;
   - `Task` becomes blocking or a channel;
   - `ref`, `out`, `Span<T>` and ref structs have no Go value form;
   - nullability and events also need rules.
3. **A converter rewrite** from stub calls to direct member access.
4. **Subclassing and attributes** (`MonoBehaviour`, Godot `Node`, controllers, `[Function]`, `[ExcelFunction]`). This is a new emission path for classes that derive from .NET bases, and every UI, game and plugin-host scenario needs it.

**Prior art.**
- Every transpiler that survived built a declaration ecosystem: DefinitelyTyped, ScalablyTyped, kotlin-wrappers, gobind's `import "Java/..."`.
- Every automatic generator stalled on overloads and generics. JetBrains removed Dukat.
- Fable's creator calls bindings a constant drain that "is never on par with native".

**Effort.** 10–20+ lane-weeks for a useful subset without subclassing; open-ended after that.

**Shared machinery.** It is structurally the cgo P/Invoke bridge's first phases (`PLAN-cgo-interop.md` §6, Phase 1–2: extract foreign declarations, generate calls and stubs). If both go ahead, they should share one design.

**Recommendation: OUT of v1.** v1's interop story is the direction that already works: **C# calling converted Go**, documented by Phase 3's guide.
- The companion-folder pattern is documented only with Phase 3's stability statement and drift warnings. In it, the functions that touch .NET live in their own Go file with ordinary Go fallback bodies, so `go build` and gopls stay happy, and a hand-owned C# replacement for that file sits in the companion folder.
- The bodyless-plus-`partial` route is less suitable for users: Go accepts bodyless declarations only when the package has non-Go parts, which breaks native builds.

Open an interop DESIGN doc only on the evidence in §4, and no earlier than the cgo bridge's first phases.

---

## 4. Usefulness verdict

**Where the TypeScript comparison holds and where it fails**

| TypeScript property | go2cs |
|:--|:--|
| The target has no alternative (the browser runs only JavaScript) | **No.** Go deploys natively. Most Go developers work on macOS or Linux and deploy to Linux. |
| Output is nearly the source, so source-mapped debugging feels native | **No.** `mToFlush.v += 100` is `ᏑmToFlush.ValueSlot.Value.v += 100` in the debugger (golden `ClosureCapturedPointerAddress`). |
| Calling platform APIs is free | **No.** Go source cannot name a .NET type. |
| One source of truth, regenerated by a build step | **Yes.** This is go2cs's real advantage over one-off ports, LLM-made ones included: the Go stays canonical and the C# twin is checked against Go's own tests. |
| Fast, incremental, errors on source lines | **Not yet.** Latency is unmeasured (E1), and today every reconverted `.cs` is rewritten. It is the purpose of Phases 2–3. |

**Who benefits**

| Group | Need | Served by | Verdict |
|:--|:--|:--|:--|
| .NET team that keeps Go as the source of truth for shared code and calls it from C# | Rebuild the C# twin on every Go change; call it from C# | Phase 3 (`GoModuleReference`) and the C#-consumer guide | **Real: the main case** |
| go.* NuGet consumers debugging C# that holds golib values | Readable values | Phase 1 (standalone) | **Real, independent of IDE mode** (vsdbg and Rider only) |
| go2cs users converting or publishing modules (nugetgo, Target Atlas) | Debug a conversion against Go lines | Phases 2–2.5 | Real, small group |
| Learners comparing Go with C# | Side-by-side view | Phase 4 "Show C#" | Cheap showcase |
| The fleet | — | — | **Weak.** Lanes are agent sessions that root-cause from emitted `.cs`, goldens and test output. Debugger views change no test output, and source-mapped mode hides the `.cs` they read. |

**Who does not benefit**

| Group | Why |
|:--|:--|
| A Go developer who wants "Go on .NET" for its own sake | Go already runs there, and usually faster. On go2cs's JIT benchmarks (measured 2026-08-25 with go1.23.1), most workloads take 1.4x–6.8x Go's time; Map is faster than Go (0.67x), StringView is at parity (1.01x), and IfaceShell is 34.9x. Startup is about 279 ms against 23 ms, and peak working set at startup is 47.4 MB against 4.3 MB (`docs/Performance.md:100-127`). |
| GoLand users (28% of Go developers) | No .NET build and no .NET debugger in GoLand. |
| Anyone who needs .NET APIs from Go: WinForms/WPF/MAUI, Unity or Godot scripts, plugin hosts | Blocked by Phase X. Revit-style hosts are also on .NET 8, and the corpus is `net10.0`-only. |
| Developers on current Go | Go 1.24 is out of upstream support, and current modules need a newer Go (Roadmap, *Go releases*). |
| macOS developers (60% of Go survey respondents develop on macOS) | The macOS run layer has no date (Roadmap, *Platforms*). |
| Enterprises that ban AGPL tools | The converter is AGPL. The commercial licence in `LICENSING.md` is the answer. |

**Market signals**
- RemObjects Gold has offered this exact idea since 2020: Go on .NET, Visual Studio integration, .NET API access. Go is included in the $999 Elements Developer License. In our opinion it shows little public pull.
- Other Go→.NET compilers are abandoned.
- GopherJS, the closest Go analogue, has 1.21.0 (June 2024) as its latest release, targeting Go 1.21. That illustrates the version-lag risk.
- No go2cs issue asks for IDE integration or for calling .NET from Go. That is weak evidence: nobody can ask for a capability they cannot find.
- Microsoft now serves Go developers natively (first-class Go on Azure Functions, in preview).

**Verdict.** Build a gated thin slice; do not build a Go-first IDE product.
- Every scenario where IDE mode is essential also needs .NET interop.
- Every scenario that works without interop is served by build integration, and its main audience edits C# and references Go.
- The thin slice's value is the decision it enables (E1, the E9 choice, the owner's hands-on verdict), plus a tangible proof of the "code in Go, run in .NET" line in `docs/Background.md:31`. It does not pay for itself through the fleet, and Phase 2 on its own serves only people who convert by hand with a flag.
- Phase 1 is approved or declined on its own merits.
- A shipped integration costs about 2–4 lane-weeks a year to keep working, plus an owner signing hand per release. A one-owner project should count that before starting Phase 3.

**Discoverability and measurement**
- **Launch vehicle:** the NuGet build package and its `dotnet new` template. .NET teams find tools through NuGet and `dotnet new search`. The VS Code extension targets Go developers, who mostly do not benefit, so its install count is a secondary signal only.
- **Drop metric, defined before launch:** downloads of the build package and template over the 90 days after an announcement, against a threshold the owner sets in advance. Suggested: under about 1,000 downloads and no issues filed by strangers means stop at Phase 3 and skip Phase 4.

**Evidence that would change the verdict**

| Toward a bigger investment (interop, more IDEs) | Toward dropping even the slice |
|:--|:--|
| People outside the fleet ask to call .NET APIs from Go | E1 is above twice its prediction, even on the changed-package path |
| The build package passes its download threshold, with issues filed by strangers | The 90-day download metric misses its threshold |
| A sponsor with a funded Unity, Godot or plugin-host case | Third-party pilots show most modules need converter fixes before they convert, compile and pass. A suggested bar: under ~70% of a Target Atlas sample converting cleanly means F5 mostly shows generated-code errors. |
| Converted output proven under Unity's CoreCLR, and separately under IL2CPP (which would open consoles, the one place native Go cannot go) | The owner's hands-on verdict on Phase 2.5 is negative |

**Measure before any public claim:** incremental conversion + build latency on a mid-size module, and the share of real modules that hit throwing stubs or cgo.

---

## 5. Licensing

*An engineering reading, not legal advice.*

### What we would build on

| Component | Terms | What a go2cs extension or build package may do |
|:--|:--|:--|
| **vsdbg** (Microsoft .NET debugger, shipped in the C# extension) | Proprietary. Usable only with Microsoft VS Code, Visual Studio or Visual Studio for Mac. No redistribution. Refuses to run in forks. | **May** hand a `coreclr` launch configuration to the C# extension in Microsoft VS Code (the Docker and Ionide pattern). **May not** bundle or download it, or make it work in forks. |
| **C# extension** (`ms-dotnettools.csharp`) | Source MIT. The Marketplace binary, vsdbg included, is under Microsoft terms that limit its use to Microsoft IDEs, and Microsoft does not publish it to Open VSX. | Depend on it in the Marketplace build only. |
| **C# Dev Kit** | Visual Studio licence terms: free for personal, academic and OSS use and for commercial teams of up to 5; larger organisations need Visual Studio Professional or higher. | **Do not require it.** Target the `coreclr` debug type, not `dotnet`. |
| **netcoredbg** (Samsung) | MIT; speaks DAP. Release builds for Linux x64/arm64, macOS arm64 and Windows x64 only. No `DebuggerDisplay` or `DebuggerTypeProxy` support. | May be bundled, with its notice. This is the fork path. |
| **SharpDbg** | MIT; supports `DebuggerDisplay` and `DebuggerTypeProxy` | A fork-path alternative to evaluate. |
| VS Code extension API, Code-OSS | MIT. Publishing to the Marketplace and to Open VSX is free. | Normal use. |
| Go toolchain, gopls | BSD-3-Clause. The VS Code Go extension is MIT. | Coexist; we redistribute nothing. go2cs already uses `golang.org/x/tools` (BSD-3), which is compatible with AGPL. |
| .NET SDK, Roslyn, MSBuild | MIT | Build on them. |
| Visual Studio, Rider | Licensed to the user (Visual Studio Community/Professional terms; Rider free for non-commercial use, paid for commercial) | The user's concern. The MSBuild route needs no plug-in in either. |
| The "Go" name and logo | go.dev brand guidelines | Plain-text nominative use is fine. No stylized mark, no implied endorsement. |

### What we would publish under

| Deliverable | Licence | Notes |
|:--|:--|:--|
| VS Code extension, MSBuild build package and targets, template | **MIT** | The owner is the sole `AUTHORS` holder, so any licence is open to him. MIT matches golib and the generators and maximizes adoption. |
| Converter, including any `#line` mode and the bundled binaries | AGPL-3.0-only + Converter Output Exception, unchanged | MIT tooling that invokes go2cs as a separate process is aggregation. Bundling the binary in a NuGet package, dotnet tool or VSIX (the v1 plan, Q4) conveys it, so the package ships the licence and a source link at the matching tag. |
| Generated C# | Output Exception: not a covered work; scaffolding is MIT | Unchanged. `#line` and checksum lines are scaffolding. |
| golib debugger views | MIT | Ship as part of `go.lib` and `go.gen`. |
| go.* stdlib | BSD-3-Clause | Unchanged. |

### Blockers and cautions

- **No blocker.**
- **Debugger licensing decides which IDEs we can support.** vsdbg works only in Microsoft's IDEs, so forks need netcoredbg (raw golib values) or a SharpDbg-style alternative. Never ship vsdbg.
- **AGPL bans.** Some enterprises ban AGPL tools (Google's policy, for example), and that hits the "Go developer in a .NET shop" group. The commercial-licence line in `LICENSING.md` already answers it. The MIT tooling and the Output Exception keep user code clear either way.
- **Hosted service.** A public transpile service built on a *modified* converter triggers AGPL §13 (offer the source to network users). Nothing like that is planned; it matters only if the Tour is ever hosted publicly with a modified converter.
- **The "go2cs" name and logo are reserved** (`LICENSING.md:194-201`), so third-party forks of the extension could not use them.

---

## 6. Placement in the go2cs plan

There is no IDE, tooling or debugging item in `docs/Roadmap.md` or in any `docs/PLAN-*.md`. The only record is the banked BOARD idea, marked "post-terminal ... Nothing here is scheduled" (BOARD 16965-16967).

**What it waits for**

| Prerequisite | Roadmap heading | Why IDE mode needs it |
|:--|:--|:--|
| Third-party modules convert and pass their tests: the uuid pilot, then gojq (the first with a dependency), then jwt | *Real-world Go modules* | A build of a real module is only as good as module conversion |
| nugetgo: the **converter side already exists** (mapping on by default, `-nuget-map*` flags, `go2cs.nuget.lock` with SHA-512 pins). Still outstanding: the publishing-identity decisions, the uuid pilot's published package, the registry launch and its CI, and a mapped package that itself depends on another third-party package (not yet exercised end to end). | *NuGet packages of converted modules* | In the loop, dependencies should restore rather than reconvert. Phase 3 and nugetgo both define how a module build resolves dependencies. The Roadmap's "converter integration … Q1 2027" line lags the code; COORD may want to refresh it separately. |
| Loud failure on `import "C"` | *cgo* | A clear refusal at build time instead of a silent skip |
| The corpus on a supported Go release. The Go 1.26 hop (Q4 2026) is the first supported landing, supported until Go 1.28 ships (about February 2027). The 1.27 hop is needed to stay supported, and after that a hop every Go release. | *Go releases* | A public "press F5 on your Go" claim on an out-of-support release is a brand problem |
| The hop-cadence decision the Roadmap schedules for Q4 2026 | *Go releases* | Committing to an IDE public preview means committing to a hop every Go release. The preview should be an explicit input to that decision. |
| The macOS run layer | *Platforms* | 60% of Go survey respondents develop on macOS |
| The .NET SDK and C# LangVersion pin | *.NET* | The build package pins the same values |
| Assembly-backed declarations implemented | *Phase 5 — Implement assembly-backed declarations in C#* | Fewer throwing stubs at run time |

**Proposed queue**

| # | When | What | Displaces |
|:--|:--|:--|:--|
| 1 | Any small window, off the critical path | **Phase 0.** A cloud lane that passes preflight runs E1, the netcoredbg half of E2, E5, E6, E6b, E7 and E9's measurements. The owner or a desktop lane runs the vsdbg half of E2, plus E3 and E4, each in under an hour. | 1–1.5 cloud-lane weeks on its own credits; 2–3 owner hours |
| 1 | The next golib/go.gen window | **Phase 1 (standalone)**, shipping with the next package release | A slot in a package release, beside the ж-box and IfaceShell work |
| 2 | After Phase 0 | **The `.cs` write fix**, in any converter window outside a battery freeze | Hours, plus one `check-no-regression` run |
| 3 | After Phase 0 and E9 | **Phases 2 and 2.5.** Under E9 (b) or (c) they are build-package work: no converter source, so they can run during hop season on a cloud lane. Under (a), after the Go 1.25 hop and outside battery freezes. | Under (b)/(c): cloud-lane time only. Under (a): converter-lane time in hop season, which holds three hops (1.25, 1.26, 1.27) in about five months |
| 4 | After the decision gate | **Phase 3.** The converter side of nugetgo exists, so this need not wait for the Roadmap's Q1 2027 row. It waits on the uuid pilot's package (a local feed is enough) and, ideally, the transitive-dependency case. Design it alongside the publishing-identity decisions, on the same lane as nugetgo publishing or in close coordination. | 4–8 lane-weeks taken from the module-pilot and nugetgo-publishing lane (gojq, jwt) or from the macOS run layer, in a packed Q4–Q1 window |
| 5 | After Phase 3's exit test | **Phase 4** as a private preview | Cloud-lane time only |
| 6 | Public preview | No earlier than the Go 1.26 hop; needs the 1.27 hop to stay supported, then a hop every Go release. macOS stays labelled preview until its run layer lands. | Commits the project to a hop every six months, indefinitely |
| 7 | Interop | Not before the cgo bridge's first phases, and only on evidence | — |

**Parallelism**
- Phases 0 and 1 can run alongside anything.
- Phases 2 and 2.5 under E9 (b) or (c) can run alongside the hops, the module pilots and the macOS run layer. Under (a), they respect battery source freezes.
- Phase 3 touches the same module-build surface as nugetgo publishing: give it the same lane, or coordinate closely.
- Phase 4 is independent of the corpus and the converter, so it is a good fit for a cloud lane on its own credits.

**Rough timeline (lane-weeks)**

| Option | Phases | Lane-weeks | What you get |
|:--|:--|:--|:--|
| Standalone, approved separately | 1 | 1–2 | Readable slice, map, string, pointer and channel values in vsdbg and Rider for every go.* consumer |
| **Thin slice (ends at the decision gate)** | 0 + 2 + 2.5 | 3.5–6.5 | Go-line breakpoints, stepping and errors through a hand-wired VS Code loop the owner can press F5 on; the E1 numbers; the E9 design choice |
| IDE-neutral v1 | + 3 | 7.5–14.5 | A C# project references a Go module and rebuilds it on every Go edit, in Visual Studio, Rider, VS Code and CI; a Go-`main` template; a headless gate on three OSes |
| Full v1 | + 4 | 9.5–17.5 | One-click Run/Debug on .NET in VS Code |
| Keep-alive, after the first release | — | 0.25–0.5 per release (about 2–4 a year), plus one owner signing hand per release | Keeps working across go2cs releases, Go hops, weekly VS Code releases and three OSes |
| Extended | + 2b, 4b and chosen Phase 5 items | +2–14 | Column-precise stepping, forks, Visual Studio and Rider plugins |
| Interop | X | 10–20+ for a subset | Go calling .NET. A separate decision. |

Elapsed time depends more on owner review bandwidth than on lane-weeks. With one lane working part-time beside the hops, the thin slice fits in Q4 2026 if it stays off converter source; Phase 3 realistically lands in Q1 2027; the full v1 spans roughly two quarters.

---

## 7. Open questions for the owner

| # | Question | Recommendation |
|:--|:--|:--|
| Q1 | **Ambition:** a gated experiment, or a flagship "Go-first IDE" product? | The gated experiment. If it passes, the product is build integration for C# projects that consume Go. The VS Code extension is a showcase, not the product. Revisit on the §4 evidence. |
| Q2 | **Opt-in `#line` emission in the converter.** The default-emission ruling leaves an opt-in mode open. Do you accept one that is never the default and never committed? | Needed only if E9 picks option (a). Under (b) or (c) the converter's emission never changes, so the question is moot. If (a): yes, guarded by the byte-identity test for the default, the two invariants and a repoguard against `#line` in any committed file. |
| Q3 | **Interop in v1?** | Out. v1 documents C# calling converted Go. The companion-folder pattern is documented only with a stability statement and drift warnings. Open a DESIGN doc only on evidence, shared with the cgo bridge design. |
| Q4 | **Converter delivery:** require a `go install`ed go2cs, or bundle prebuilt AGPL binaries? | **Bundle from day one**: prebuilt RID-specific binaries as a dotnet tool or inside the build package, with the AGPL text and a source link at the matching tag, plus a new `go2cs -version` the package can check. `go install` is not possible today: the module path is the bare `go2cs`, which nothing can fetch, release tags are four-part (`nuget-1.24.13.3`), which is not valid Go module semver, and the documented install is clone then `go install .`. Making it possible would need a fetchable module path and a semver tag scheme for a subdirectory module, with a decision on the fourth release digit (a pre-release suffix sorts below the release, and `+build` metadata is ignored). |
| Q5 | **Identity, home and licence:** package ID (touches the pending NuGet prefix reservation), Marketplace/Open VSX publisher, and whether this lives in the main repo or a separate one | Use the same publishing identity as the nugetgo decisions. Keep it in the main repo under an MIT-licensed folder until a contributor base appears. MIT for the extension and build package. |
| Q6 | **Visibility and measurement:** when does this appear on the Roadmap and README, and what counts as adoption? | Add a Roadmap line when Phase 3 is queued. Launch through NuGet and `dotnet new`, and set the 90-day download threshold before announcing. Hold the public preview for a supported Go release, with macOS marked as preview. |
| Q7 | **Hop cadence:** is an IDE public preview an input to the Q4 2026 hop-cadence decision? | Yes. No public preview without committing to a hop every Go release. |
| Q8 | **Phase 1 on its own:** approve the golib debugger views independently of the IDE decision? | Yes, at the next golib/go.gen window. They serve every go.* consumer in vsdbg and Rider and change no emission. |

---

## 8. Sources

### In-repo (at `59ee0d21bf`)

**Prior discussion of the idea**
- `docs/phase4/BOARD-next-validation-candidates.md:16949-16967`: "FUTURE ERA: step-through-GO debugging", the banked idea of an opt-in build mode that injects `#line` transiently at build time from the position-map tables, plus SourceLink. Unscheduled.
- `docs/phase4/BOARD-next-validation-candidates.md:13676-13714`: the `#line` arc re-priced. It reaches `StackFrame`; Roslyn normalizes directive paths to backslashes and `goSourcePath` restores the separator, and the two halves compose; CS diagnostics relocate to Go positions; +28–47% lines.
- `docs/phase4/DESIGN-position-map.md:51`: option A (`#line`) disqualified *for the committed corpus*. §3.1 moved the records into `package_info.cs`.
- `docs/ValidatedTestPackages.md:502`: owner ruling that line-number-only tests are disclosed with no emission change.
- `docs/Background.md:31`: "code in Go, run in .NET".

**Position map and runtime**
- `src/go2cs/positionMapOperations.go:23-35`: the position map's sentinel → table design.
- `src/core/golib/GoPositionMapAttribute.cs:1-5` (MIT header), `:19-24` (consumers), `:50-56` (line-only encoding; predecessor lookup: "a C# line between two records belongs to the earlier one"), `:58-69` (function-literal spans; the Roslyn numbering "matches Go's counter only by coincidence").
- `src/core/runtime/managed_impl.cs:1937-1971` (`goFuncLiteralSuffix`: same record lookup, falls back to the Roslyn-derived ordinal on a miss), `:2223-2226` (`goSourcePath` replaces `\` with `/` for every frame), `:2312-2331` (lookup keyed by the PDB path's file name against `CsFile`).
- `src/tests/GolibTests/CallerFrameTestVariantNamingTests.cs:336-361`: golib already compiles `#line` regions.

**Converter modes, writes and build output**
- `docs/README.md:161-170` (install: clone, `cd src/go2cs`, `go install .`), `:177-213` (modes and options), `:205` (`-recurse=nuget` Go-release lock), `:206-210` (nugetgo mapping on by default, `go2cs.nuget.lock`, transitive case not yet exercised), `:276-290` (Go ≤ 1.24.13 pinning, `GOTOOLCHAIN=local`, silent toolchain download), `:308-362` (output layout, `.slnx`, F5; per-platform flavours and version skew at `:337-345`).
- `src/go2cs/go.mod:1`: `module go2cs`. Repository tags `nuget-1.24.13.1` to `.3`.
- `src/go2cs/main.go:255-280`: flag definitions, including `-nuget-map*`; no version flag.
- `src/go2cs/projectFileWriter.go:16-18, 750-781`: `needToWriteFile`, and `writeOutputFile`, which always calls `os.Create`. Callers of `needToWriteFile`: `projectFileWriter.go:74/601/683`, `moduleConverter.go:779/971`, `solutionGenerator.go:93`, `platformEmit.go:781/849/873`, `embedDirective.go:838`, `readme.go:223`, `testConversion.go` (test-host files). None writes converted `.cs`.
- `src/go2cs/conversionDriver.go:549`: the normal converted-source write path; `:313-339`: hand-own probe on every output path.
- `src/go2cs/moduleConverter.go:210-222, 837-915`: per-project `.slnx`, `Directory.Build.props/targets`, `.artifacts` redirection with a 12-character SHA-256 token (`:854-862`), `GoStdLibVersion` floating on `.*` (`:896-905`), platform pin.
- `src/go2cs/nugetMap.go`, `nugetLock.go`, `nugetSubstitution.go`: the nugetgo converter integration.
- `src/go2cs/testConversion.go:6409-6413`: `-tests` input-digest manifest; `:6573-6600`: `converterRevision` (exe hash, else `vcs.revision`).
- `src/go2cs/visitFuncDecl.go:1347-1356`: bodyless Go func → C# `partial` + companion.
- `src/go2cs/csproj-template.xml:141-143`: every `.cs` in a package folder compiles.
- `src/core/strings/package_info.cs:78` and `strings.cs:402`: an exported Go function is a public static method on `public static partial class strings_package`.
- `src/tour/README.md:28-30, 110-112, 171-177`; `src/tour/pipeline.go:261-303`: the existing convert → build → run loop.
- `src/push-nuget.ps1:125`: packages are built in `Release` by default.
- `.claude/rules/harness-gates.md:339-340`: per-file conversion is sub-second and `go/packages` load dominates; a single core package builds in ~6 s warm, ~60 s cold.

**golib and go2cs-gen**
- `src/core/golib/slice.cs:956`, `map.cs:1006`, `array.cs:702`, `string.cs:351`, `ж.cs:485`: today's `ToString` overrides; `channel.cs` has none.
- `src/core/golib/GoexitException.cs:34`, `PanicException.cs:19` (types), `error.cs:126` (method): `DebuggerNonUserCode`. `PanicException.cs`: 15 × `DebuggerBrowsable(Never)`.
- `src/gen/go2cs-gen/Templates/InterfaceType/InterfaceShellEmitter.cs:185, 302`: interface-shell forwarders marked `DebuggerNonUserCode`.
- `src/core/golib/runtime/Goroutine.cs:27-35` (a dedicated thread per goroutine; why not the ThreadPool), `:850-853` (an ordinary channel or sync deadlock blocks rather than being reported), `:1021-1024` (`new Thread(...)`, no `Name`); `docs/phase4/DESIGN-cooperative-scheduler.md:309-310`: the design calls for goroutine thread names.

**Performance**
- `docs/Performance.md:100-127`: environment line (go1.23.1, 2026-08-25), JIT and Native AOT ratios, startup and working set; `:55-58`: Native AOT publish takes hours, so it stays out of the loop.

**Licensing and plan**
- `LICENSING.md:1-60, 179-201`; `src/go2cs/LICENSE-EXCEPTION:29-53`: licence matrix, Output Exception, commercial licence, CLA, name reservation.
- `docs/Roadmap.md`: *Go releases* (58-68), *.NET* (72), *Platforms* (109), *Real-world Go modules* (130), *NuGet packages of converted modules* (158-174), *cgo* (176), *Phase 5* (204), *Timeline* (321-330).
- `docs/PLAN-cgo-interop.md` §6, Phase 1–2 (declaration extraction, P/Invoke generation); `docs/PLAN-nugetgo.md`.
- `CLAUDE.md` safety floor #1, #3, #4, #6, #12, #13 (the failure modes Phase 3 guards against on users' machines).

### External

**TypeScript model and MSBuild precedent**
- TypeScript debugging (`preLaunchTask`, `sourceMap`, `outFiles`): https://code.visualstudio.com/docs/typescript/typescript-debugging
- TypeScript compiling and VS Code tasks / problem matchers: https://code.visualstudio.com/docs/typescript/typescript-compiling · https://code.visualstudio.com/docs/debugtest/tasks
- TypeScript design goals: https://github.com/microsoft/TypeScript/wiki/TypeScript-Design-Goals
- Microsoft.TypeScript.MSBuild: https://learn.microsoft.com/en-us/visualstudio/javascript/compile-typescript-code-nuget · https://github.com/Microsoft/TypeScript/wiki/TypeScript-MSBuild-In-depth
- MSBuild project SDKs: https://learn.microsoft.com/en-us/visualstudio/msbuild/how-to-use-project-sdk
- Incremental builds: https://learn.microsoft.com/en-us/visualstudio/msbuild/incremental-builds
- Generated files in MSBuild: https://learn.microsoft.com/en-us/visualstudio/msbuild/customize-builds-for-generated-files
- Grpc.Tools build integration: https://github.com/grpc/grpc/blob/master/src/csharp/BUILD-INTEGRATION.md
- Visual Studio fast up-to-date check (`UpToDateCheckInput`, `UpToDateCheckBuilt`): https://github.com/dotnet/project-system/blob/main/docs/up-to-date-check.md
- Design-time builds (`DesignTimeBuild`, `BuildingProject`): https://github.com/dotnet/project-system/blob/main/docs/design-time-builds.md

**`#line`, PDBs and debugger views**
- C# `#line` and `#pragma checksum` ("forces the next line's number"; `#line hidden` "doesn't affect file names or line numbers in error reporting"): https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/preprocessor-directives
- Enhanced `#line` (C# 10): https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/proposals/csharp-10.0/enhanced-line-directives · https://github.com/dotnet/csharplang/blob/main/proposals/csharp-10.0/enhanced-line-directives.md
- Portable PDB format: https://github.com/dotnet/runtime/blob/main/docs/design/specs/PortablePdb-Metadata.md
- `#line hidden` / 0xFEEFEE: https://learn.microsoft.com/en-us/archive/blogs/jmstall/line-hidden-and-0xfeefee-sequence-points
- `#line` sources embedded in the PDB: https://github.com/dotnet/roslyn/issues/12625
- `#line` paths must be absolute: https://github.com/dotnet/vscode-csharp/issues/304
- Razor `#line` fragility: https://github.com/dotnet/razor/issues/12243
- Q# generated-C# debugging write-up: https://ryan-moreno.github.io/blog/qsharp-debug-feature/
- C# extension manifest (`qsharp` breakpoints and `coreclr` languages; licence field): https://raw.githubusercontent.com/dotnet/vscode-csharp/main/package.json
- `DebuggerDisplay`/`DebuggerTypeProxy`: https://learn.microsoft.com/en-us/visualstudio/debugger/create-custom-views-of-managed-objects
- Just My Code and optimized libraries: https://code.visualstudio.com/docs/csharp/debugger-settings

**Hot Reload and `dotnet watch`**
- Hot Reload: https://learn.microsoft.com/en-us/visualstudio/debugger/hot-reload
- Supported edits (VS page defers to Roslyn for .NET 6+): https://learn.microsoft.com/en-us/visualstudio/debugger/supported-code-changes-csharp
- Roslyn EnC supported edits ("Add and modify custom attributes", VS 17.0): https://github.com/dotnet/roslyn/blob/main/docs/wiki/EnC-Supported-Edits.md
- `dotnet watch` (`Watch` items, default `Compile`/`EmbeddedResource`, `DotNetWatchBuild`): https://learn.microsoft.com/en-us/dotnet/core/tools/dotnet-watch

**VS Code extension mechanics**
- Debugger extension guide (`breakpoints` contribution, configuration providers): https://code.visualstudio.com/api/extension-guides/debugger-extension
- Contribution points: https://code.visualstudio.com/api/references/contribution-points
- Docker-style debug type resolution: https://github.com/microsoft/vscode/issues/110889 · https://github.com/microsoft/vscode/pull/143054
- Ionide (F#) breakpoints with no debugger of its own: https://raw.githubusercontent.com/ionide/ionide-vscode-fsharp/main/release/package.json
- vscode-go manifest (MIT, `go` breakpoints): https://raw.githubusercontent.com/golang/vscode-go/master/extension/package.json
- VS Code weekly stable releases from 1.111 (March 2026): https://visualstudiomagazine.com/articles/2026/03/11/vs-code-1-111-debuts-weekly-stable-cadence-expands-agent-controls.aspx · https://adtmag.com/articles/2026/03/11/visual-studio-code-editor.aspx

**Visual Studio, Rider and GoLand**
- Visual Studio editor language support: https://learn.microsoft.com/en-us/visualstudio/ide/adding-visual-studio-editor-support-for-other-languages
- Visual Studio LSP client (no debugger support yet): https://learn.microsoft.com/en-us/visualstudio/extensibility/adding-an-lsp-extension
- Concord expression-evaluator samples: https://github.com/microsoft/ConcordExtensibilitySamples
- JetBrains Go plugin (IntelliJ IDEA Ultimate and GoLand, not Rider): https://plugins.jetbrains.com/plugin/9568-go
- Third-party gopls bridges for other JetBrains IDEs: https://plugins.jetbrains.com/plugin/32932-go-portable · https://plugins.jetbrains.com/plugin/34233-go-language-helper

**Debugger and tooling licences**
- vsdbg licence: https://visualstudio.microsoft.com/license-terms/mt644895/ · https://github.com/dotnet/vscode-csharp/blob/main/docs/debugger/Microsoft-.NET-Core-Debugger-licensing-and-Microsoft-Visual-Studio-Code.md · https://github.com/dotnet/vscode-csharp/issues/7412 · https://learn.microsoft.com/en-us/answers/questions/2152877/vscode-debug-dotnet-you-may-only-use-the-microsoft
- netcoredbg (MIT) and its release assets: https://github.com/Samsung/netcoredbg · https://github.com/Samsung/netcoredbg/releases · https://api.github.com/repos/Samsung/netcoredbg/releases/latest
- SharpDbg (MIT; comparison table with netcoredbg): https://github.com/MattParkerDev/sharpdbg
- C# Dev Kit licensing: https://code.visualstudio.com/docs/csharp/cs-dev-kit-faq · https://devclass.com/2023/06/07/microsoft-improves-c-support-in-visual-studio-code-but-full-commercial-use-requires-paid-license/
- Rider non-commercial: https://blog.jetbrains.com/blog/2024/10/24/webstorm-and-rider-are-now-free-for-non-commercial-use/
- Open VSX for forks: https://github.com/VSCodium/vscodium/blob/master/docs/extensions.md
- GPL FAQ (aggregation, program output): https://www.gnu.org/licenses/gpl-faq.html
- AGPL-3.0 §13: https://www.gnu.org/licenses/agpl-3.0.en.html
- Google AGPL policy: https://opensource.google/documentation/reference/using/agpl-policy
- Go brand: https://go.dev/brand
- golang.org/x/tools (gopls) licence: https://github.com/golang/tools/blob/master/LICENSE
- Go modules reference (semver, pre-release ordering, build metadata): https://go.dev/ref/mod

**Prior art**
- GopherJS: https://github.com/gopherjs/gopherjs · https://github.com/gopherjs/gopherjs/releases
- Fable's future discussion: https://github.com/fable-compiler/Fable/discussions/3351
- Kotlin 1.8.20 (Dukat removed): https://kotlinlang.org/docs/whatsnew1820.html
- Scala.js semantics: https://www.scala-js.org/doc/semantics.html
- IKVM: https://github.com/ikvmnet/ikvm
- gobind reverse bindings: https://pkg.go.dev/golang.org/x/mobile/cmd/gobind
- Haxe licensing model: https://haxe.org/foundation/open-source.html
- PeachPie MSBuild SDK: https://docs.peachpie.io/msbuild/
- RemObjects Gold and pricing ("Go is included with the full Elements Developer License", $999): https://elements.docs.remobjects.com/Gold/ · https://www.remobjects.com/elements/Pricing.aspx · https://talk.remobjects.com/c/elements/gold/

**Market and audience**
- Go developer survey 2025 (VS Code 37%, GoLand 28%; macOS 60%, Linux 58%): https://go.dev/blog/survey2025
- Go on Azure Functions: https://learn.microsoft.com/en-us/azure/azure-functions/functions-custom-handlers
- Godot `net10.0`: https://github.com/godotengine/godot/pull/123738
- Unity CoreCLR: https://discussions.unity.com/t/path-to-coreclr-2026-upgrade-guide/1714279
- Revit 2026 SDK references: https://packages.nuget.org/packages/Autodesk.Revit.Sdk.Refs.2026/1.2.0
- Go `wasmexport`: https://go.dev/blog/wasmexport · wasmtime-dotnet: https://github.com/bytecodealliance/wasmtime-dotnet
- Go c-shared multi-runtime issue: https://github.com/golang/go/issues/65050
