# DRAFT — the go.* 1.26.10.4 release notes

<!-- DRAFT for owner and COORD review, started 2026-11-11 by seat C43 (P2) from master 50dd9a065c, on the .3 precedent
     (docs/DRAFT-news-1-26-10-3.md, b3ca4ef593). Nothing here is published. At release the entries below move to the top
     of docs/NEWS.md (dated), and the same text is the GitHub Release body. Each item names the committed record its
     facts are read from in an HTML comment beside it. -->

---

## <DATE>, 2026 — <TITLE>

<!-- One section per item, each owned by the seat that writes it, so the seats' merges touch separate lines (COORD,
     2026-11-11). At release the sections are merged into the entry's prose. -->

### Converting a module: default build tags (C43)

**Converting a module picks a library's pure-Go code over its assembly.** `go2cs -recurse`, in every form (local
references, `-recurse=module` and `-recurse=nuget`), now builds a module with the build tags `safe`, `purego` and
`noasm`, where it used `safe` alone, and prints them at the start of the run. A library that ships hand-written
assembly behind `!purego` or `!noasm` now converts its pure-Go version instead of methods that throw when called.
github.com/klauspost/cpuid/v2 is the case that prompted it: it ran its assembly CPU detection as the package
loaded, so every converted program that imported it, github.com/zeebo/xxh3 among them, stopped at startup. With
`noasm` it uses its pure-Go detection. An explicit `-tags` still replaces the default entirely, and
`-module-safe-tag=false` drops all three tags. None of the nine published nugetgo packages selects a different
file under the new tags, so none needs a rebuild for this change. See
[Build tags for a module](ConvertingAModule.md#build-tags-for-a-module).

A known limit: no tag removes assembly that a library gates on the architecture alone. Its declarations still
convert to methods that throw, and they stay reachable through code that forces the fast path on. xxh3's
vector routines are the case: hashing takes the portable path, because the converted cpuid reports no CPU
features, but xxh3's own tests switch the vector paths on and reach the throwing methods.
<!-- seat C43 (COORD, 2026-11-11), branch claude/p2-c43-recurse-tags. The default: src/go2cs/commandLineOptions.go
     defaultModuleBuildTags. The nine-package census, at both tag sets on windows/amd64, linux/amd64, linux/arm64,
     darwin/amd64 and darwin/arm64, and the cpuid/xxh3 control: docs/phase4/DATA-c43-recurse-tags.md. -->

### `main` keeps its name, and runs with Go's stack (C46)

**A converted program's `main` now has the stack Go gives it.** Go's main goroutine grows its stack to 1 GB, so
deep recursion in `main` is ordinary Go. A converted program used to run `main` on the process's startup thread,
whose stack is the operating system's default (1 MiB on Windows, usually 8 MiB on linux and macOS), and a deep
recursion there ended the process with `Stack overflow.` where Go printed its result. `main` now runs as the main
goroutine on a thread that reserves 1 GiB. The converted-test host runs `TestMain` the same way, as Go runs it on
the main goroutine. The cost is one extra thread at start-up: under half a millisecond on linux and Windows, and a
few milliseconds on macOS hosted runners, where thread creation is slower and more variable.

**Visible change: `func main` keeps its name in converted code.** It used to become C#'s `Main`; it is now
`main`, like every other Go function, and the C# entry point `Main` is generated beside it. Every converted
program's `main.cs` changes in that one line when it is converted again. A program converted by an earlier
release still builds: the generator adds nothing when a `Main` is already declared. One Go rule wins over the
stack: when an `init` function calls `runtime.LockOSThread`, `main` runs on the startup thread, as Go runs it
there, and keeps that thread's stack. The main package's own `init` functions also still run on the startup
thread. See [Limitations](Limitations.md).
<!-- seat C46 (COORD ruling 2026-11-11, option A), branch claude/c1-c46-main-stack. Design and measurements:
     docs/ConversionStrategies-Reference/package-conversion.md, "`main` keeps its name, and the entry point is
     generated"; start-up probe runs 42987722709, 42987726907, 42987730438. Confirmed on typescript-go: the two parse
     units that overflowed at 1 MB (binderBinaryExpressionStress.ts/Js) exit 0 with dumps byte-identical to native. -->
