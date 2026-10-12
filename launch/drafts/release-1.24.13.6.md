# Draft: the GitHub Release body for 1.24.13.6

> DRAFT. Everything below the line is the release body, ready to paste or to pass to
> `gh release create nuget-1.24.13.6 --notes-file`. It is the 1.24.13.6 NEWS entry
> (`claude/r-news-1-24-13-6`) with its links made absolute, plus an install section. If the NEWS entry
> changes in review, regenerate this from it. **Release title:** `go2cs 1.24.13.6: Reflection works under Native AOT, and macOS validation passes three quarters`
>
> The "Binaries" paragraph describes what the tag-triggered workflow attaches. Delete it if this release
> is published without them.

---

**NuGet 1.24.13.6.** This release is about converted Go behaving the same in more places: under Native
AOT, on macOS, and at the edges where a call used to reach an unimplemented stub.

**Reflection under Native AOT.** Go's `reflect` creates values of types it learns about at run time.
Until now the runtime library did that by generating code at run time, which Native AOT cannot do, so a
program that reflected over its own types could fail once published that way. The source generators now
write those operations at build time, for every type a package defines and every container type that can
reach `reflect`. `reflect.New`, `reflect.Zero`, the `Make` functions and conversions between struct types
behave under Native AOT as they do under the JIT, in a default publish and in a fully trimmed one.

This costs size, and the choice is deliberate: behaving like Go comes before being small. Measured on
`win-x64`, a C# program that references the packages publishes about 10% larger with the default trim
and about 4% larger fully trimmed, and programs that lean on reflection over generic types grow about 12%
fully trimmed. A Native AOT publish is still slow and large to begin with
([Known issues](https://go2cs.net/KnownIssues.html#a-native-aot-publish-of-a-program-that-references-the-packages-is-slow-and-large)). Three cases still take the old path under Native AOT only: a conversion to an
interface inside generic code, a container reachable only through a field another package does not
export, and a struct whose own package never takes a pointer to it.

**Converted modules as NuGet packages.** Three Go modules are published at
[nugetgo.net](https://nugetgo.net): `hashset`, `google/uuid` and `golang-jwt/jwt` v5. The tooling that
packs them now refuses a package whose standard-library references are not a published release, records
no path from the machine that built it, and binds the package's validation record to the exact files
packed. A package's README on nuget.org is the author's own: the pack keeps one fenced block in it up to
date, with the badges and the provenance, and leaves the rest alone. For a conversion, `-nuget-map-feed`
reads mapped packages from a feed you name, such as a local folder, instead of nuget.org
([Command-line options](https://go2cs.net/CommandLine.html#options)).

**Every `go.*` package says how it was checked, and how to call it.** The Tests badge on each package's
README now reads the two numbers that matter, tests matched and tests disclosed, and links the package's
proof page. Below it, one line names the C# class that holds the package's functions, so a C# developer
who lands on `go.strings` knows to call `go.strings_package`.

**Two calls that reached a stub now answer as Go does.** A measurement of which exported functions can
reach an unimplemented stub found two on Windows and Linux
([the record](https://github.com/ritchiecarroll/go2cs/blob/master/docs/phase4/CENSUS-phase5-exported-reach.md)). `runtime/coverage.ClearCounters` now returns Go's
own error for a program built without coverage, and `runtime.Breakpoint` breaks into an attached
debugger, or without one stops the program with Go's report line.

**macOS.** Validation of the standard library on macOS passes three quarters of the roster: 171 of 225
packages now carry a macOS count, each agreed between Apple silicon and Intel
([the roster](https://go2cs.net/ValidatedTestPackages.html)). Four fixes came out of that work. `os.Executable` returns the
program's path. The memory page size is the machine's real one, 16 KB on Apple silicon, where it was
fixed at 4 KB. `hash/crc32` uses the processor's CRC instructions on Arm64. And on Linux and macOS
`os.Args[0]` is the name the program was started with, as it is in Go, so a program that starts itself
again under another name recognizes itself. Verifying a certificate against the system's roots still reaches a stub on macOS; that
fix is promoted ahead of the rest of the macOS work.

The `go.*` packages are built and versioned together, so use one version of them in a project.

## Install

The packages are on NuGet: the converted Go standard library as
[`go.*`](https://www.nuget.org/packages?q=go2cs%20ritchiecarroll), the runtime as
[`go.lib`](https://www.nuget.org/packages/go.lib) and the source generators as
[`go.gen`](https://www.nuget.org/packages/go.gen), all at 1.24.13.6. A conversion made with
`go2cs -recurse=nuget` references them for you.

**Binaries.** The converter is attached below for six platforms (`win-x64`, `win-arm64`, `linux-x64`,
`linux-arm64`, `osx-x64`, `osx-arm64`), with `SHA256SUMS`. Each was built from this tag by the
repository's own workflow, which `gh attestation verify <file> --repo ritchiecarroll/go2cs` confirms.
They are not code-signed. Converting still needs Go 1.24.13 and the .NET 10 SDK on your machine: see
[Requirements](https://go2cs.net/#requirements). Or build the converter yourself:
[Building go2cs from source](https://go2cs.net/BuildingGo2cs.html).

**Full announcement:** [go2cs News](https://go2cs.net/NEWS.html). **Limits:**
[Limitations](https://go2cs.net/Limitations.html) and [Known issues](https://go2cs.net/KnownIssues.html).
