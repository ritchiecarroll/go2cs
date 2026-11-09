# Audit: the user-facing docs at 1.26.10.3

> **State: RECORD (2026-11-09), P2.** A findings list with proposed fixes. No visitor page is edited in this cut.
> Read against master `e45fd4feb5`; commands run on linux-x64 (a cloud container, .NET SDK 10.0, the go2cs
> dotnet tool 1.26.10.3 from nuget.org). Point-in-time: amend with dated blocks, never rewrite.

## 1. Scope: the pages a visitor reaches

The set is a link-walk from [`docs/README.md`](../README.md), which is both the repository README and the
go2cs.net index. Depth 1 is every `docs/*.md` page the README links; depth 2 is every `docs/*.md` page those
link that the README does not. Every depth-2 page was then walked once more: its further `docs/*.md` links are
either already in the set or sit inside HTML comments, where they point at the internal records
`docs/_config.yml` excludes from the site (`phase3/`, `phase4/`, `doctrine/`, `PLAN-*`, `DESIGN-*`, the two runbooks,
`TestingInfrastructureRequirements.md`). No visible link on a visitor page reaches an excluded record. `ConversionStrategies.md`'s reference
sub-pages were left out of the page set.

| Depth | Pages |
|---|---|
| 0 | `README.md`; `NEWS.md`, its latest entry only (November 8, 2026, lines 11-238) |
| 1 | `Architecture`, `BuildingGo2cs`, `CommandLine`, `ConsumingGoFromCSharp`, `ConversionStrategies`, `ConvertingAModule`, `DiagnosticCodes`, `DotnetTool`, `FAQ`, `Features`, `KnownIssues`, `Limitations`, `Roadmap`, `Tuning`, `ValidatedTestPackages` (head and prose; table rows checked only for the rules below) |
| 2 | `Background`, `BeforeAndAfterTheFaceLift`, `CIMatrix`, `Glossary`, `Performance`, `RoadmapHistory`, `StdLibCompileMilestone`, `ValidatedModules` |

25 pages in all. Each page was read in full, visible text only. HTML comments are invisible to a visitor and
were ignored, except where a comment marks a block as generated or script-checked (rule 3).

**Method.** Two passes. (a) Scripted, over every page: every relative link and `#anchor` resolved against the
tree at master; every converter flag a page names compared with `go2cs -h` built at master; a scan for
"TypeScript", "tsc", hostnames, user paths, IP addresses and nicknames outside the allowed set. (b) Five readers,
each given the rules below and a group of pages, read every line and quoted what they found; each quoted finding
was kept with its page:line, and the claims the top ten rest on were re-read at the tree before this record was
written. Every pasteable command a visitor would run first was run (§5).

**Rules applied** (COORD, 2026-11-09): present tense on a reference page (NEWS is the history page); every
measured claim names its platform or RID; no exact corpus figures unless a process re-measures them; no
TypeScript on visitor pages; nicknames only (i7, i9, G-LAPTOP, R-LAPTOP, C1, C2, AZ1, P1, P2), and no internal
process jargon a visitor cannot follow; links resolve at master; every pasteable command works; nothing
contradicts `go2cs -h` at master.

## 2. Summary

189 findings on 24 of the 25 pages (`Features.md` has none).

| Class | Findings |
|---|---|
| wrong | 17 |
| stale | 40 |
| broken link | 0 |
| tense | 56 |
| platform-unnamed | 30 |
| figure | 17 |
| unmeasured | 5 |
| other | 24 |
| **total** | **189** |

Clean on the scripted checks: no visible TypeScript mention on any page (the only hits are inside HTML
comments); no hostname, user name, home path or IP address in visible text apart from `CIMatrix.md:280` (an
Azure blob-storage host name, finding F167); no nickname outside the allowed set; every relative link and
anchor resolves at master. Three links leave `docs/` (`Glossary.md:10`, `CIMatrix.md:7`,
`RoadmapHistory.md:37/84/152`); they are not broken, because `.github/site-outside-links.py` rewrites them to
their GitHub copies, but two of them lead a go2cs.net visitor into the fleet's agent instructions, which the owner's
OQ-1 ruling of 2026-10-26 keeps off the site (findings on `CIMatrix.md:7` and `RoadmapHistory.md:37`).

**Top ten**, by what a first-time visitor hits first and what it costs them:

1. **A visible `PENDING` marker on the README and on the released NEWS entry promises gojq on NuGet.** nuget.org has no `nugetgo.github.com.itchyny.gojq` or `...timefmt-go` package. Remove the sentence and the NEWS item until the publish. (F7, F16, F86)
2. **The consumer five-minute start fails when followed in order.** `go.strings` and `go.fmt` work; adding `nugetgo.github.com.google.uuid` fails restore with NU1107, because no nugetgo package has a 1.26.10.3 revision yet. NEWS tells readers to move to those revisions, says the restore is silent, and carries a second visible `PENDING` marker. (F26, F11, F12)
3. **The README's one-package command fails for a dotnet-tool user.** `go2cs package_dir out` emits `$(go2csPath)core` references with no clone behind them; the output does not build (10 errors). (F9)
4. **Four pages say you need Go installed to convert.** The README says the dotnet tool downloads Go 1.26.10 itself, and `go2cs toolchain env` did exactly that here. (F10, F108, F38, F45)
5. **A pasteable example pins the standard library to 1.26.10.1**, two releases back: `dotnet build -p:GoStdLibVersion=1.26.10.1`. (F28)
6. **Limitations lists two traceback gaps that 1.26.10.3 fixed** (a panic inside a deferred closure; a goroutine crash's id and `created by` line), contradicting the NEWS entry for the same release. (F39, F40)
7. **Known issues presents a fixed issue as current** (local standard-library references building Debug, fixed in 1.26.10.3) and gives no release for the fix. (F47, F49)
8. **Roadmap calls the current release 'in preparation'** and lists its shipped work as planned. (F74, F80, F91, F93)
9. **Roadmap states the macOS two-chip rule backwards** ("records the stricter of the two"), against Roadmap.md:281, Limitations.md:316 and the roster's own agreement rule. (F73)
10. **The command-line reference says it lists every option and omits eight of `go2cs -h`'s flags**; and `go2cs -h` itself misstates the `-stdlib`/`-tests` default tags (§4). (F96, F97)

Next in line: the `Tuning.md` deployment bullets (22-34) quote the Go 1.23.1 table, and the current
`Performance.md` table reverses three of their four comparisons; `ValidatedModules.md:22` labels its five
module rows as a Go 1.24.13 reading that has not been re-measured; `Performance.md:345` closes an HTML comment
and opens a list item on one line, which can render as raw text (fix in `src/tests/Performance/README.md`, which
that page copies); `CIMatrix.md:280` names a real Azure blob-storage host.

## 3. Findings, in the order a visitor meets them

Pages are ordered as a first-time visitor reaches them: the README, then the pages in the order the README
first links them, then the depth-2 pages. Within a page, by line. Findings are numbered F1, F2, ... for reference from the summary. "As read" quotes visible text (shortened with
"..." where long). Line numbers are 1-based at `e45fd4feb5`.

- **F1** `docs/README.md:18` · tense
  - As read: "and the same validation is in progress on macOS."
  - Proposed: "and macOS packages are validated package by package; the roster shows which rows carry a macOS count."

- **F2** `docs/README.md:25` · tense
  - As read: "The second fix release on the Go 1.26 line, still built from Go 1.26.10."
  - Proposed: "The second fix release on the Go 1.26 line, built from Go 1.26.10." (NEWS teaser block; whether a teaser is exempt is the owner's call)

- **F3** `docs/README.md:26` · tense
  - As read: "Native AOT now inlines a slice read as the JIT does"
  - Proposed: "Native AOT inlines a slice read as the JIT does" (teaser)

- **F4** `docs/README.md:26` · platform-unnamed
  - As read: "so the same programs run faster there, and the P-256, P-384 and P-521 curves are faster under ReadyToRun."
  - Proposed: add the platform: "...run faster there on win-x64, and ... faster under ReadyToRun on win-x64." (NEWS.md:72,121,123,131 name win-x64)

- **F5** `docs/README.md:28` · platform-unnamed
  - As read: "and a plain single-file publish starts fast."
  - Proposed: "and a plain single-file publish starts fast (measured on win-x64)." (NEWS.md:101)

- **F6** `docs/README.md:28` · tense
  - As read: "A slice's `foreach` now gives its values"
  - Proposed: "A slice's `foreach` gives its values" (teaser)

- **F7** `docs/README.md:30` · stale
  - As read: "**PENDING (the publish):** gojq, a jq implementation in Go, ships as a NuGet package."
  - Proposed: Checked: nuget.org returns 404 for nugetgo.github.com.itchyny.gojq and nugetgo.github.com.itchyny.timefmt-go. Remove the sentence until the package publishes (the same holds for NEWS.md:167-177 and Roadmap.md:313)

- **F8** `docs/README.md:107` · tense
  - As read: "macOS, on Intel and Apple silicon, has packages and is being validated package by package"
  - Proposed: "macOS, on Intel and Apple silicon, has packages; a package that validates there shows a macOS count on the roster"

- **F9** `docs/README.md:149` · wrong
  - As read: "go2cs package_dir out                    # one package"
  - Proposed: Run with the dotnet tool alone (no clone), it warns that the runtime root "is not a converted go2cs tree" and emits $(go2csPath)core project references; `dotnet build` of the output fails with 10 errors (CS0246 'fmt_package' not found), measured on linux-x64. Say what it needs: "go2cs -go2cspath <go2cs>/src package_dir out   # one package, against a go2cs clone; use -recurse=nuget for a project that restores from NuGet"

- **F10** `docs/NEWS.md:20` · wrong
  - As read: "and converting a module still needs a Go 1.26 toolchain and the .NET 10 SDK."
  - Proposed: "and converting a module uses a Go 1.26 toolchain (the go2cs dotnet tool downloads Go 1.26.10 itself) and the .NET 10 SDK." (README.md:100-102: the tool needs no Go installation)

- **F11** `docs/NEWS.md:36` · wrong
  - As read: "NuGet restores an older nugetgo package under 1.26.10.3 without an error, ... move each `nugetgo.*` reference to its 1.26.10.3 revision."
  - Proposed: Two problems, both measured on linux-x64: (1) no nugetgo package has a 1.26.10.3 revision on nuget.org (newest uuid 1.6.0 (revision 2), x/sync 0.19.0 (revision 2), hashset 1.0.0 (revision 4) each depend on go.* [1.26.10.2, 1.27.0)); (2) restoring uuid 1.6.0 (revision 2) beside go.fmt 1.26.10.3 is not silent: it fails with NU1107 on go.unicode.utf16 (see ConsumingGoFromCSharp.md:45). Reword once the wave is live: "Restoring an older nugetgo package beside go.* 1.26.10.3 can fail with NU1107, or succeed and fail at the first slice loop; move each nugetgo.* reference to its 1.26.10.3 revision."

- **F12** `docs/NEWS.md:38` · stale
  - As read: "**PENDING (the owner's deprecation, once the nugetgo wave is live).** The 1.26.10.2 revisions of the published nugetgo packages are deprecated on nuget.org"
  - Proposed: Released entry, visible PENDING marker. If the deprecation is done, remove the marker; if not, word it as future or remove it.

- **F13** `docs/NEWS.md:79` · platform-unnamed
  - As read: "so matrix-multiply, sieve and sort benchmarks take 13% to 18% less time. A 64-bit multiply ... makes a TLS handshake take about 14% less time. Both hold at the default x64 instruction set."
  - Proposed: prefix "On `win-x64`, on one development machine," (the source comment at :84-86 says win-x64)

- **F14** `docs/NEWS.md:82` · platform-unnamed
  - As read: "Building a converted program for `x86-64-v3` ... gives no gain across the benchmark suite"
  - Proposed: "...gives no gain across the benchmark suite on `win-x64`" (source comment :85)

- **F15** `docs/NEWS.md:110` · platform-unnamed
  - As read: "that build compiled the standard library in Debug, about 21 times slower on a TLS handshake under the JIT"
  - Proposed: name the RID and machine (DATA-hop126-perf.md §11.9; the comment at :115-117 gives none)

- **F16** `docs/NEWS.md:167` · stale
  - As read: "**PENDING (the publish: the nugetgo wave). gojq for .NET.**"
  - Proposed: Visible PENDING marker on a released entry. If gojq 0.12.19 and timefmt-go 0.1.8 are on nuget.org remove the marker; if not delete the item (as the comment at :181-182 instructs), with README.md:30.

- **F17** `docs/NEWS.md:173` · platform-unnamed
  - As read: "all 164 results match, with no exceptions, and that proof ships in the package as VALIDATION.md. The command-line package's tests convert and pass in full too, 914 of 914"
  - Proposed: name the RID the 164 and 914 were measured on; re-read 164 against the published VALIDATION.md (comment at :190)

- **F18** `docs/NEWS.md:173` · figure
  - As read: "all 164 results match ... 914 of 914"
  - Proposed: Exact counts for one module, kept deliberately (comment :189). Keep only if exact counts in a NEWS entry are accepted; otherwise "every result matches".

- **F19** `docs/ConversionStrategies.md:1653` · other
  - As read: "carry `notes`, which the package's proof page prints above the verdicts"
  - Proposed: "proof page" is defined neither here nor in Glossary.md: "the package's test report page (its proof page, see ValidatedTestPackages.md)"

- **F20** `docs/ConversionStrategies.md:8647` · platform-unnamed
  - As read: "That reservation is address space rather than memory, but it is what makes a hundred thousand goroutines parked at once slow to start."
  - Proposed: add "(measured on win-x64, R-LAPTOP)"; the HTML comment at 8682-8684 holds the measurement (100,000 goroutines, 361 s at 256 MiB against 70 s at 16 MiB)

- **F21** `docs/ConversionStrategies.md:8676` · tense
  - As read: "so a goroutine that does I/O, formats with `fmt` or serves a connection keeps today's reservation."
  - Proposed: "...keeps the full 256 MiB reservation."

- **F22** `docs/ConversionStrategies.md:8696` · tense
  - As read: "That is a property of today's .NET rather than of the conversion"
  - Proposed: "That is a property of .NET 10 rather than of the conversion"

- **F23** `docs/ConversionStrategies.md:8704` · platform-unnamed
  - As read: "Go's goroutines reach about a million... A Go program that starts hundreds of thousands of goroutines at once does not run the same way after conversion"
  - Proposed: borderline: no OS named (its comment says the source figure has none)

- **F24** `docs/ConversionStrategies.md:12174` · platform-unnamed
  - As read: "Go's compiler replaces each call with a few machine instructions, and the converted body is slower."
  - Proposed: name the platform measured, or state it as expected: "and a converted body cannot match that."

- **F25** `docs/ConversionStrategies.md:12482` · other
  - As read: "A `go2cs -stdlib` conversion ... or a `go2cs -tests` conversion ... applies `purego,math_big_pure_go` by default."
  - Proposed: page matches the code (commandLineOptions.go:276 defaultStdLibBuildTags); go2cs -h's -stdlib/-tags help says only "purego": fix the flag help, not the doc

- **F26** `docs/ConsumingGoFromCSharp.md:45` · wrong
  - As read: "dotnet add package nugetgo.github.com.google.uuid" (after "dotnet add package go.strings" / "go.fmt", lines 22-23)
  - Proposed: Run in order on linux-x64, the first half works (`dotnet run` prints `HELLO, GOPHER 13`), and adding uuid breaks restore: `error NU1107: Version conflict detected for go.unicode.utf16` (go.fmt 1.26.10.3 needs utf16 = 1.26.10.3; uuid 1.6.0 (revision 2) -> go.encoding.json 1.26.10.2 needs utf16 = 1.26.10.2). The newest uuid revision is built against go.* 1.26.10.2 and no 1.26.10.3 revision is published. Until the nugetgo wave publishes, either pin the example to go.* 1.26.10.2 (`dotnet add package go.fmt --version 1.26.10.2`, same for go.strings) or hold the uuid half of the example

- **F27** `docs/ConsumingGoFromCSharp.md:67` · tense
  - As read: "`go.*` 1.26.10.2 is such a release: it turns exported package-level value variables, such as `os.Args`, into ref properties, so an assembly compiled against 1.26.10.1 ... fails"
  - Proposed: Release history; move to NEWS. Replace with: "For example, exported package-level value variables such as `os.Args` are ref properties; an assembly compiled against a `go.*` release that exposed them as fields fails with `MissingFieldException` until it is rebuilt."

- **F28** `docs/ConsumingGoFromCSharp.md:80` · stale
  - As read: "for example `dotnet build -p:GoStdLibVersion=1.26.10.1`"
  - Proposed: Current release is 1.26.10.3: `dotnet build -p:GoStdLibVersion=1.26.10.3`.

- **F29** `docs/ValidatedTestPackages.md:11` · other
  - As read: "## Where Phase 4 stands"
  - Proposed: "## Where validation stands" ("Phase 4" is internal)

- **F30** `docs/ValidatedTestPackages.md:13` · figure
  - As read: "Phase 4 progress: **238 / 242 testable packages validated — 98.3%**"
  - Proposed: Exact corpus figures, hand-edited. No generator marker, but src/check-roster-format.ps1 recomputes them from the table and fails on a mismatch (page line 810); the table sums agree. Keep if that checker counts as the re-measuring process; drop "Phase 4"

- **F31** `docs/ValidatedTestPackages.md:15` · figure
  - As read: "**77,381 matching test verdicts · 372 disclosed** *(updated 2026-11-08 — grows as packages validate)*"
  - Proposed: Same checker caveat. Drop "grows as packages validate": every implementable package is validated (line 17)

- **F32** `docs/ValidatedTestPackages.md:17` · figure
  - As read: "Against the implementable set (242 − 4 excluded = 238): 238 / 238 — 100.0%."
  - Proposed: Same checker caveat

- **F33** `docs/ValidatedTestPackages.md:21` · tense
  - As read: "including the 13 packages new at Go 1.26.10 and `internal/unsafeheader`, which rejoined from the excluded list"
  - Proposed: "**Every implementable package is validated**; the four packages outside the 238 are listed ..."

- **F34** `docs/ValidatedTestPackages.md:26` · figure
  - As read: "**Linux: 236 of 236 applicable rows validated at their Linux counts** — 77,498 matching verdicts · 404 disclosed · 2 rows platform-exclusive"
  - Proposed: Same checker caveat; "layout-L3 csproj" on the same line is jargon: "its project compiles nothing under GoTargetOS=linux"

- **F35** `docs/ValidatedTestPackages.md:465` · wrong
  - As read: "`path/filepath` offers 54 eligible verdicts on Linux where Windows offers 61"
  - Proposed: The roster row (737) gives Windows 67: "where Windows offers 67", or drop the numbers

- **F36** `docs/ValidatedTestPackages.md:614` · other
  - As read: "'s ~520 stdout comparisons all run through this package, so banking it strengthens each of them"
  - Proposed: "so validating it strengthens each of them" ("banking" is internal)

- **F37** `docs/ValidatedTestPackages.md:812` · other
  - As read: "along with every banked and excluded row's membership in it"
  - Proposed: "every validated and excluded row's membership"

- **F38** `docs/Limitations.md:15` · wrong
  - As read: "**You need the Go release go2cs tracks on your PATH**, exactly the one in [Requirements]\(README.md#requirements)"
  - Proposed: Contradicts README.md:100-102 (the dotnet tool downloads Go 1.26.10 on first use; verified by running `go2cs toolchain env`). "**Your code must build with the Go release go2cs tracks** ([Requirements]\(README.md#requirements)); the go2cs dotnet tool downloads that Go for you"

- **F39** `docs/Limitations.md:113` · stale
  - As read: "A panic raised again inside a deferred closure shows one frame, the function the first panic left, where Go shows the closure and the frames above it."
  - Proposed: Fixed in 1.26.10.3 (NEWS.md:196). Remove, and fix "three cases" at line 111-112

- **F40** `docs/Limitations.md:116` · stale
  - As read: "A goroutine's crash is labelled `goroutine 1`, and Go's `created by` line is missing."
  - Proposed: Fixed in 1.26.10.3 (NEWS.md:202; Goroutine.cs:548). Remove

- **F41** `docs/Limitations.md:201` · platform-unnamed
  - As read: "Most compute lands within a small multiple of Go's time, string-heavy work takes several times longer, maps run at parity or better"
  - Proposed: "On win-x64, in the [performance comparison]\(Performance.md), most compute..."

- **F42** `docs/Limitations.md:216` · figure
  - As read: "one IL2104 for each package with trim warnings, eight for a program that uses `fmt`"
  - Proposed: "one IL2104 for each package with trim warnings, a handful for a program that uses `fmt`"

- **F43** `docs/Limitations.md:221` · stale
  - As read: "and a Native AOT publish is slow either way ([Known issues]\(KnownIssues.md))."
  - Proposed: KnownIssues.md:207-210 reports a few minutes after golib's fix: "and a Native AOT publish compiles every referenced package in full ([Known issues]\(KnownIssues.md#a-native-aot-publish-of-a-program-that-references-the-packages-is-slow-and-large))."

- **F44** `docs/Limitations.md:236` · platform-unnamed
  - As read: "**Native AOT** starts much faster and holds less memory, but once running it is slower than the JIT on most rows of the comparison"
  - Proposed: Name the platform ("on win-x64, in the comparison,")

- **F45** `docs/Limitations.md:302` · wrong
  - As read: "`go version` has to print exactly the release in [Requirements]\(README.md#requirements) when you convert, and when you `go install` the converter from a clone"
  - Proposed: README.md:102-103: the dotnet tool brings its own Go; building from source needs "Go 1.26". "Convert with the Go release in Requirements (the dotnet tool downloads it); building the converter from a clone needs the Go release src/go2cs/go.mod names"

- **F46** `docs/Limitations.md:344` · platform-unnamed
  - As read: "`reflect` (`encoding/json` and `text/template` pass their own suites on it), and `net/http` with HTTP/2 and `crypto/tls` over real sockets."
  - Proposed: "...pass their own suites on it, on x64 Windows and Linux)..."

- **F47** `docs/KnownIssues.md:59` · stale
  - As read: "## A program converted with local standard-library references runs its standard library built Debug"
  - Proposed: Fixed in 1.26.10.3 (NEWS November 8; moduleConverter.go:720 writes no .slnx for local references at master). Retitle: "## With go2cs 1.26.10.2 or earlier, a program converted with local standard-library references runs its standard library built Debug"

- **F48** `docs/KnownIssues.md:62` · platform-unnamed
  - As read: "its standard-library assemblies come out built Debug, unoptimized, and it runs slower than the same program built from its `.csproj`."
  - Proposed: "...runs slower (about 21 times on a TLS handshake under the JIT, on win-x64, one development machine)...", or drop the speed claim

- **F49** `docs/KnownIssues.md:73` · stale
  - As read: "**Fix.** Re-convert with a converter that includes the fix."
  - Proposed: "**Fix.** Re-convert with go2cs 1.26.10.3 or later. It writes no solution for a local-reference conversion, and removes the one an earlier converter wrote, if that file is unchanged."

- **F50** `docs/KnownIssues.md:104` · tense
  - As read: "No converted Go program has reproduced it so far, only a synthetic reproduction."
  - Proposed: "No converted Go program is known to reproduce it; only a synthetic reproduction does."

- **F51** `docs/KnownIssues.md:121` · platform-unnamed
  - As read: "It costs about 90 ms (16%) more startup time, and steady-state loop speed stays within measurement noise."
  - Proposed: "On linux-x64 (Canonical's .NET 10, one 4-core machine) it costs about 90 ms (16%) more startup time, ..."

- **F52** `docs/KnownIssues.md:199` · wrong
  - As read: "The check that gates each release publishes a C# consumer of the packages and runs it on `linux-x64`, `win-x64`, `osx-arm64` and `osx-x64`."
  - Proposed: Align with Roadmap.md:257-259 and os-matrix.yml:1969: "...runs it on linux-x64, win-x64 and osx-arm64; on osx-x64 the publish is read when it finishes inside the hosted runner's time limit, and reported as not measured when it does not."

- **F53** `docs/KnownIssues.md:204` · stale
  - As read: "A full trim (`TrimMode` set to `full`) produces a program that runs as well. That is measured on `win-x64`, on one development machine"
  - Proposed: The reading (hidden comment 189-197) is from 1.24.13.5: re-measure on the 1.26.10.3 packages, or name the release measured

- **F54** `docs/KnownIssues.md:209` · stale
  - As read: "the publish took under four minutes on a hosted CI runner for Linux and for Windows, about six minutes on a Mac with Apple silicon and about eleven on an Intel Mac (measured 2026-10-14)."
  - Proposed: The comment at 222-224 says these were taken at a Go 1.24.13 tree: re-measure on the 1.26.10.3 packages, or name the packages measured

- **F55** `docs/KnownIssues.md:210` · tense
  - As read: "Before golib's generic-cycle fix of that day, the same publish took about an hour on Linux and Windows and nearly five hours on an Intel Mac, ..."
  - Proposed: remove (history; the section that follows covers 1.24.13.7 and earlier)

- **F56** `docs/KnownIssues.md:214` · tense
  - As read: "The fix first shipped in the `go.*` 1.26.10.1 packages; for 1.24.13.7 and earlier, see the section that follows."
  - Proposed: "For the 1.24.13.7 packages and earlier, see the section that follows."

- **F57** `docs/KnownIssues.md:265` · figure
  - As read: "Measured on `win-x64`: 16 of the performance suite's 17 programs published this way, and the TLS handshake program failed at link."
  - Proposed: "Measured on win-x64: every program of the performance suite but the TLS handshake published this way; that one failed at link."

- **F58** `docs/KnownIssues.md:298` · stale
  - As read: "A converted program's project carries the same line, so a plain single-file publish is not affected either. A project converted by an earlier go2cs does not"
  - Proposed: Name the release (the template line arrived in 1.26.10.3): "A project converted by go2cs 1.26.10.3 or later carries the same line... A project converted by an earlier go2cs does not"

- **F59** `docs/Tuning.md:22` · stale
  - As read: "The `win-x64` comparisons below come from the performance comparison's Go 1.23.1 table under History, measured on an earlier converter release"
  - Proposed: Re-derive the deployment bullets (22-34) from Performance.md's current table (win-x64, go.* 1.26.10.2)

- **F60** `docs/Tuning.md:25` · stale
  - As read: "a Native AOT program started in roughly an eighth of the time the same program took under the JIT"
  - Proposed: Current table (win-x64): 158.8 vs 506.4 ms, "in about a third of the time"

- **F61** `docs/Tuning.md:28` · stale
  - As read: "the JIT ran slice-indexing, floating-point and pointer-heavy loops faster than Native AOT, which took roughly two to three times as long on each"
  - Proposed: Current table: Sieve 2.2x, MatMul 3.3x, RefLower 1.26x. Drop "pointer-heavy" or give its own ratio

- **F62** `docs/Tuning.md:31` · stale
  - As read: "converted map code ran faster than Go under both the JIT and Native AOT, and fastest under Native AOT"
  - Proposed: Current table: JIT 251.0, ReadyToRun 252.0, AOT 285.6 ms, "fastest under the JIT and ReadyToRun"

- **F63** `docs/Tuning.md:33` · stale
  - As read: "calling interface methods and asserting to concrete types took within about 15% of the same time under the JIT and Native AOT"
  - Proposed: Current table: AOT about 1.5-1.7x the JIT (IfaceCall 559.6 vs 336.2, Iface 572.8 vs 375.3). Rewrite

- **F64** `docs/Tuning.md:34` · stale
  - As read: "asserting a value to an interface it satisfies only structurally, took about two thirds longer under Native AOT than under the JIT"
  - Proposed: Current table IfaceShell 1,177.5 vs 602.7 ms, "about twice as long"

- **F65** `docs/Tuning.md:179` · figure
  - As read: "a TLS handshake benchmark that loads 112 packages"
  - Proposed: "a TLS handshake benchmark that loads over a hundred packages"

- **F66** `docs/Tuning.md:213` · tense
  - As read: "Most of the JIT speed-up from go.* 1.26.10.1 to 1.26.10.2 is this setting: with the delay at 100, the 1.26.10.2 JIT times sit on 1.26.10.1's."
  - Proposed: remove (release history)

- **F67** `docs/Tuning.md:285` · other
  - As read: "-hoist-literals" described as a field plus `ref readonly` accessor
  - Proposed: `go2cs -h` says only "becomes one static readonly field". Not contradictory; align the help text with the page

- **F68** `docs/Tuning.md:334` · wrong
  - As read: "Every comparison in this list after the first was measured against the direct field read, before the accessor existed"
  - Proposed: The whole-program bullet (329) was re-measured at 1.26.10.2 per the page's own comment: "The second and third bullets were measured against the direct field read; the accessor reads faster than that field."

- **F69** `docs/Tuning.md:360` · platform-unnamed
  - As read: "A literal passed as an `any` argument, `fmt.Println("x")`, measured the same both ways"
  - Proposed: "On win-x64, a literal passed as an `any` argument ... measured the same both ways"

- **F70** `docs/Tuning.md:361` · tense
  - As read: "Before the accessor, a small function that read a hoisted field could run slower than its plain form, ...; the accessor removed that case."
  - Proposed: remove

- **F71** `docs/Tuning.md:413` · tense
  - As read: "### What else was considered"
  - Proposed: "### Options that do not change run-time cost"

- **F72** `docs/Tuning.md:417` · stale
  - As read: Options table lists -var ... -nuget-map-*, -module-*, -tests, -test-*
  - Proposed: Add the `go2cs -h` flags it omits (-allow-stale-converter, -platform-stage, -ref-census, -dual-recv, -dual-recv-params), saying whether each changes cost

- **F73** `docs/Roadmap.md:146` · wrong
  - As read: "Where the two chips' results differ, the row records the stricter of the two and discloses the difference."
  - Proposed: Contradicts Roadmap.md:281-282, Limitations.md:316-317 and ValidatedTestPackages.md:474-476 (a row gains its macOS count only when both chips agree). "A row gains its macOS count only when both chips agree with each other and with Go."

- **F74** `docs/Roadmap.md:173` · stale
  - As read: "Fix releases on the Go 1.26.10 line continue; 1.26.10.3 is in preparation."
  - Proposed: "Fix releases on the Go 1.26.10 line continue; 1.26.10.3 is the current release."

- **F75** `docs/Roadmap.md:198` · stale
  - As read: "**A .NET 11 measurement stage follows the Go 1.26 hop**, once .NET 11 has shipped (November 2026)"
  - Proposed: "hop" is internal jargon; check .NET 11's status on publish day: "A .NET 11 measurement stage follows, once .NET 11 ships (November 2026)"

- **F76** `docs/Roadmap.md:213` · platform-unnamed
  - As read: "The converted test host publishes as a self-contained, relocatable single-file executable, so Go tests that copy and relaunch their own binary pass."
  - Proposed: "...relaunch their own binary pass on Windows and Linux."

- **F77** `docs/Roadmap.md:235` · tense
  - As read: "- **Shipped in 1.24.13.5.** Every package registers the Go types it defines, ..."
  - Proposed: remove (release history; NEWS/RoadmapHistory)

- **F78** `docs/Roadmap.md:239` · tense
  - As read: "- **Shipped in 1.24.13.6.** `reflect` works under Native AOT without generating code at run time"
  - Proposed: remove, or keep only "`reflect` works under Native AOT without generating code at run time."

- **F79** `docs/Roadmap.md:241` · tense
  - As read: "- **Shipped in 1.24.13.7.** ... a Native AOT publish now takes longer: the `osx-x64` publish no longer fits the hosted runner ..."
  - Proposed: remove (history; also contradicts KnownIssues.md:209, about eleven minutes on an Intel Mac after the fix)

- **F80** `docs/Roadmap.md:245` · stale
  - As read: "- **In 1.26.10.3 (in preparation).** Reading a slice element inlines under Native AOT..."
  - Proposed: 1.26.10.3 is released: remove the bullet (NEWS has it), or state it as present fact

- **F81** `docs/Roadmap.md:248` · platform-unnamed
  - As read: "A 64-bit multiply with a 128-bit result inlines too, which speeds up the TLS handshake."
  - Proposed: "...which makes a TLS handshake about 14% faster (win-x64, one development machine)."

- **F82** `docs/Roadmap.md:250` · platform-unnamed
  - As read: "A single-file publish compiles ReadyToRun one assembly at a time, which starts in a fraction of the time of the SDK's compressed composite image."
  - Proposed: "...about 0.47 s against about 3.5 s on win-x64, on one development machine."

- **F83** `docs/Roadmap.md:254` · platform-unnamed
  - As read: "Building for `x86-64-v3` gave no gain across the benchmark suite"
  - Proposed: "...across the benchmark suite on win-x64"

- **F84** `docs/Roadmap.md:294` · figure
  - As read: "Five modules pass, measured with Go 1.24.13, and a larger one is next."
  - Proposed: "The modules on [Validated Modules]\(ValidatedModules.md) pass, measured with Go 1.24.13, and a larger one is next."

- **F85** `docs/Roadmap.md:302` · figure
  - As read: "Five modules validate, each with the Go release and on the platforms [Validated Modules]\(ValidatedModules.md) names:"
  - Proposed: "These modules validate, each with the Go release and on the platforms Validated Modules names:"

- **F86** `docs/Roadmap.md:313` · stale
  - As read: "| [`itchyny/gojq`] v0.12.19, with its dependency [`itchyny/timefmt-go`] v0.1.8 | ... | Q4 2026 |"
  - Proposed: NEWS November 8 says gojq ships with 1.26.10.3 (marked PENDING); not on nuget.org at audit time. Keep as planned until published, then move to the validated list

- **F87** `docs/Roadmap.md:394` · tense
  - As read: "Every call it confirmed reaching one now reaches a real body instead."
  - Proposed: "Every call it confirmed reaching one reaches a real body."

- **F88** `docs/Roadmap.md:397` · tense
  - As read: "On macOS, verifying a certificate against the system's root certificates and CPU profiling no longer reach a stub."
  - Proposed: "On macOS, verifying a certificate against the system's root certificates and CPU profiling have real bodies."

- **F89** `docs/Roadmap.md:407` · tense
  - As read: "That is how most of the surface that matters has been implemented so far, including the gaps the macOS validation found:"
  - Proposed: "That is how most of the surface that matters is implemented, including on macOS:"

- **F90** `docs/Roadmap.md:474` · stale
  - As read: "- **Goroutine scheduling** (planned next, after 1.26.10.3):"
  - Proposed: "- **Goroutine scheduling** (planned next):"

- **F91** `docs/Roadmap.md:492` · stale
  - As read: "- **Building against local standard-library sources** (planned for 1.26.10.3): ... Until then, build the project file itself, `dotnet build MyProgram.csproj -c Release`."
  - Proposed: remove (shipped in 1.26.10.3, NEWS November 8; moduleConverter.go:720)

- **F92** `docs/Roadmap.md:545` · wrong
  - As read: "The first is recorded in the [struct-types reference]\(ConversionStrategies-Reference/struct-types.md#a-global-addressed-only-by-the-packages-own-_testgo-is-still-heap-boxed)"
  - Proposed: That section is about a global addressed only by its own _test.go, not the cross-package case the sentence describes. Link [Limitations]\(Limitations.md#what-converts-but-doesnt-behave-exactly-like-go) instead

- **F93** `docs/Roadmap.md:572` · stale
  - As read: "| Now | The 1.26.10.3 fix release. macOS validation ..."
  - Proposed: "| Now | macOS validation of the standard library, package by package. ..."

- **F94** `docs/Roadmap.md:573` · stale
  - As read: "Goroutine scheduling, after 1.26.10.3."
  - Proposed: "Goroutine scheduling."

- **F95** `docs/DotnetTool.md:30` · platform-unnamed
  - As read: "The download is about 65 to 75 MB and unpacks to about 240 MB, once per release."
  - Proposed: Name the platforms the sizes were measured on (e.g. "65 MB (linux-x64) to 75 MB (win-x64) ... about 240 MB unpacked on linux-x64"), or re-measure for each RID.

- **F96** `docs/CommandLine.md:3` · wrong
  - As read: "Every option the converter accepts."
  - Proposed: The table is missing eight flags that help.txt lists: -allow-stale-converter, -dual-recv, -dual-recv-params, -platform-stage, -ref-census, -test-allow-handown, -test-oracle-goarch, -test-publish-binlog. Either add rows for them or change the sentence to "The options a conversion uses; `go2cs -h` lists every flag, including maintainer-only ones."

- **F97** `docs/CommandLine.md:60` · other
  - As read: "`-stdlib` and `-tests` apply `purego` and `math_big_pure_go` by default, and `-recurse` adds `safe`."
  - Proposed: This differs from help.txt, whose -stdlib and -tags entries say only "-tags purego". The code matches the doc (commandLineOptions.go:276 defaultStdLibBuildTags = purego, math_big_pure_go; resolveBuildTags applies it for -stdlib OR -tests). Keep the doc and fix the help strings in main.go.

- **F98** `docs/CommandLine.md:72` · tense
  - As read: "~~`-cgo`~~ | ~~Also convert cgo-targeted files.~~ Not yet functional, but planned: ... this flag comes alive with it."
  - Proposed: A struck-through old description is change history. Replace with: "`-cgo` | Reserved for cgo support, which has no C# conversion yet: a source that imports "C" is refused (GO2CS2001) whatever this flag says. A P/Invoke-backed bridge is on the [Roadmap]\(Roadmap.md#cgo)." (This also matches help.txt.)

- **F99** `docs/DiagnosticCodes.md:29` · tense
  - As read: "No operator is generated: the one such a record used to produce boxed a copy of the value, so the conversion compiled while a write through the box never reached the original."
  - Proposed: "No operator is generated: such an operator would box a copy of the value, so a write through the box would never reach the original."

- **F100** `docs/ConvertingAModule.md:160` · other
  - As read: "Use the current go2cs packages, with a converter built from a checkout at or after the commit that published them."
  - Proposed: Contributor wording a dotnet-tool user cannot act on. Replace with "Use a go2cs converter and `go.*` packages of the same release (`go2cs -version` names it)."

- **F101** `docs/ConvertingAModule.md:164` · tense
  - As read: "The standard library is being validated package by package on macOS; a roster row shows a macOS count once its package validates there, so don't expect ... yet."
  - Proposed: Campaign narration. Replace with "On macOS, a [roster]\(ValidatedTestPackages.md) row with a macOS count is validated there; a package without one is not yet validated on macOS."

- **F102** `docs/ConvertingAModule.md:195` · platform-unnamed
  - As read: "this `fatih/color` example **compiles clean** — app plus all four dependency projects — **and runs**."
  - Proposed: Name the platforms: "compiles clean and runs on windows/amd64 and linux/amd64 (and on macOS, see the note above)".

- **F103** `docs/FAQ.md:20` · platform-unnamed
  - As read: "Most compute lands within a small multiple of Go's time, string-heavy work takes several times longer, and maps run at parity with Go or better."
  - Proposed: add the platform: "On <RID> (see the performance comparison), most compute lands..."

- **F104** `docs/FAQ.md:24` · tense
  - As read: "Some optimization is already in, such as"
  - Proposed: "go2cs applies some optimizations, such as"

- **F105** `docs/FAQ.md:63` · tense
  - As read: "The Go release it tracks today is still supported upstream."
  - Proposed: "The Go release it tracks is supported upstream."

- **F106** `docs/FAQ.md:69` · tense
  - As read: "macOS, on Intel and Apple silicon, has packages and is being validated package by package"
  - Proposed: "macOS, on Intel and Apple silicon, has packages; a row on the roster shows a macOS count once its package validates there."

- **F107** `docs/FAQ.md:122` · platform-unnamed
  - As read: "starts much faster than the same program under the JIT ... once running the program is slower than the JIT on most measurements."
  - Proposed: name the RID (or "on <RID>, see Known issues"); re-check "slower than the JIT on most measurements" after 1.26.10.3 (NEWS.md:71-90 reports large Native AOT speed-ups)

- **F108** `docs/FAQ.md:137` · wrong
  - As read: "It does not need Go installed to run. You need Go only to convert."
  - Proposed: "It does not need Go installed to run. Converting uses a Go toolchain, which the go2cs dotnet tool downloads on first use." (README.md:100-102)

- **F109** `docs/Architecture.md:19` · stale
  - As read: "dispatch: a single file or package (`conversionDriver.go`), the standard library (`-stdlib`), a module (`-recurse`), or a package's tests (`-tests`)."
  - Proposed: main.go:206 also dispatches the `go2cs toolchain` subcommand. Add ", and the `toolchain` subcommand (`toolchainCommand.go`, `toolchainProvision.go`)".

- **F110** `docs/Architecture.md:95` · stale
  - As read: "| Generator | Trigger | Produces |" (table lists 16 generators)
  - Proposed: src/gen/go2cs-gen also has [Generator] classes BoundedGoroutineInterceptorGenerator ([GoBoundedGoroutine] intercepts the goǃ call; GO2CS0010) and ExtensionHostListGenerator ([assembly: GoExtensionHostList] / GoExtensionHost records read by golib's extension-method scan under Native AOT). Add a row for each.

- **F111** `docs/BuildingGo2cs.md:79` · figure
  - As read: "Validated 14 tests against go test (0 skipped identically on both sides, 1 disclosed-divergent (deferred), 49 disclosed-unsupported declarations excluded)."
  - Proposed: Exact test counts, no generator marker (the comment at line 81 is a hand-written measurement note). Have a script re-measure the block, or show the line's shape ("Validated <n> tests against go test (...)") and link the unicode/utf8 proof page for current figures.

- **F112** `docs/BuildingGo2cs.md:79` · platform-unnamed
  - As read: "Expected final line: ... Validated 14 tests against go test ..."
  - Proposed: The platform appears only in the hidden comment (linux/amd64). Add visible text: "Expected final line (linux/amd64; other platforms can differ in counts):".

- **F113** `docs/BuildingGo2cs.md:91` · other
  - As read: "The same command validates every other package on the table"
  - Proposed: There is no table on this page. Replace with "every other package in [Validated Test Packages]\(ValidatedTestPackages.md)".

- **F114** `docs/BuildingGo2cs.md:97` · wrong
  - As read: "ReadyToRun is opt-in — set `PublishReadyToRun=true` in the environment to publish a precompiled host"
  - Proposed: testConversion.go:348 readyToRunTestPackages always publishes crypto/tls ReadyToRun. Add: "(crypto/tls's host is always published ReadyToRun, for its start-up cost)".

- **F115** `docs/Background.md:3` · tense
  - As read: "this project [started]\(RoadmapHistory.md#milestones) as a way to bring Go into my C# world ... These days the reasons for a project like this are a lot better"
  - Proposed: Author's origin story; whether Background is exempt is the owner's call. If not, keep one present-tense sentence, history to RoadmapHistory.

- **F116** `docs/Background.md:5` · tense
  - As read: "The converter took two tries to get right. The first was written in C# and read Go through an ANTLR grammar... So I did what every developer dreads and started over. Today's converter is written in Go"
  - Proposed: "The converter is written in Go and reads code with the same packages Go's own tools use (`go/ast` and `go/types`)..." (two-tries history to RoadmapHistory)

- **F117** `docs/Background.md:9` · platform-unnamed
  - As read: "and the converted app's output matches `go run`."
  - Proposed: "and on <RID> the converted app's output matches `go run`."

- **F118** `docs/Background.md:13` · tense
  - As read: "*code in Go, run in .NET* -- something until now unheard of."
  - Proposed: remove "-- something until now unheard of"

- **F119** `docs/Background.md:13` · unmeasured
  - As read: "Converting is fast and cheap, so you can even keep writing Go and convert again"
  - Proposed: "You can keep writing Go and convert again" (or give a measured conversion time with its platform)

- **F120** `docs/Background.md:15` · other
  - As read: "[side-by-side table]\(Features.md#real-standard-library-conversions-side-by-side)"
  - Proposed: link to README.md#real-standard-library-conversions-side-by-side (the Features section is a one-line pointer to the README)

- **F121** `docs/Background.md:30` · platform-unnamed
  - As read: "Go wins most benchmarks, starts faster and travels lighter on memory."
  - Proposed: add "(on <RID>, see the performance comparison)"

- **F122** `docs/Background.md:39` · unmeasured
  - As read: "it should run as fast as other comparable .NET applications"
  - Proposed: remove, or support with a measurement and its platform

- **F123** `docs/Background.md:41` · platform-unnamed
  - As read: "maps, and the string comparisons the converter can optimize, run at parity with Go, or faster."
  - Proposed: name the platform from Performance.md

- **F124** `docs/Background.md:43` · unmeasured
  - As read: "unless you publish with Native AOT, which closes most of that gap at the cost of a slower, heavier build."
  - Proposed: name the measurement and platform (the Known issues AOT entry), or "which narrows that gap"

- **F125** `docs/Performance.md:101` · platform-unnamed
  - As read: "each publish took about 9 to 10 minutes on the machine that compiled it, with the compiler peaking at 12.4 to 13.5 GB"
  - Proposed: Name the build host and OS (the comment says the i9): "on the i9 (Windows, win-x64 target)"

- **F126** `docs/Performance.md:194` · tense
  - As read: "From `go.*` 1.26.10.2 on, a converted program publishes its own code IL-only ... Under 1.24.13.7 the program's own code was ReadyToRun too."
  - Proposed: "A converted program publishes its own code IL-only and golib and the packages ReadyToRun."

- **F127** `docs/Performance.md:202` · tense
  - As read: "**TlsHandshake's Native AOT cell is its first reading.** Under `go.*` 1.24.13.7 that program's Native AOT publish cannot link"
  - Proposed: remove (History at 406 has it)

- **F128** `docs/Performance.md:204` · tense
  - As read: "**Two rows are slower under full optimization than with 1.24.13.7.** ..."
  - Proposed: "Under Native AOT, StringMatch pays a per-use conversion of its string literals and IfaceShellChurn pays its interface-shell cache on every miss."

- **F129** `docs/Performance.md:254` · tense
  - As read: "and the number to watch as the eligibility surface widens"
  - Proposed: remove

- **F130** `docs/Performance.md:269` · tense
  - As read: "Currently about 2× to 2.6× on this producer→consumer churn ... a cost that planned cooperative-scheduler work addresses"
  - Proposed: "About 2× to 2.6× Go on this producer→consumer churn: the rendezvous rides managed synchronization primitives where Go's scheduler hands off directly."

- **F131** `docs/Performance.md:296` · tense
  - As read: "(before that work this shape ran about 25× Go; the lowering brought the JIT within 3×)"
  - Proposed: remove

- **F132** `docs/Performance.md:323` · platform-unnamed
  - As read: "largely single-threaded (~1.1–1.3 effective cores regardless of machine)"
  - Proposed: Name the machines and OS, or remove

- **F133** `docs/Performance.md:324` · stale
  - As read: "Measured at the move to .NET 10: **hours per publish** on laptop-class hardware ... a full AOT re-baseline was days serial"
  - Proposed: Lead with the current figure (about 9-10 min per publish, 339-342); move the pre-fix narration to History

- **F134** `docs/Performance.md:330` · stale
  - As read: "A publish peaked at roughly **15–18 GB working set**, and at 22.2 GB when re-measured on 2026-10-13 (re-measure at each Go or .NET release move ...)"
  - Proposed: Use the current 12.4-13.5 GB (24 GB for TlsHandshake) with the host named; drop the maintainer instruction

- **F135** `docs/Performance.md:335` · tense
  - As read: "**Since 2026-10-14.** The build time and memory above were measured before golib's generic-cycle fix of that day."
  - Proposed: Merge into one present-tense build-cost bullet with the current figures

- **F136** `docs/Performance.md:345` · other
  - As read: "... Build host -m:4, affinity 0xF, serial. -->- **Why it is this way.** ILC is a **whole-program** compiler"
  - Proposed: The comment closes and the next list item starts on the same line, so the item can render as raw text. Put "- **Why it is this way.**" on its own line (fix in src/tests/Performance/README.md, which this page copies)

- **F137** `docs/Performance.md:554` · unmeasured
  - As read: "A floor exploration compared stock-SDK trim profiles with bflat and found that the floor is a trim-rooting question, not a toolchain question."
  - Proposed: Name platform and date, or remove

- **F138** `docs/Performance.md:561` · other
  - As read: "(Links are absolute because this file mirrors verbatim to `docs/Performance.md` after each canonical run.)"
  - Proposed: remove (maintainer note)

- **F139** `docs/ValidatedModules.md:14` · stale
  - As read: table lists five modules; NEWS.md:167-177 says gojq 0.12.19 ships with a VALIDATION.md proof
  - Proposed: If gojq and timefmt-go published in the 1.26.10.3 wave, add their rows.

- **F140** `docs/ValidatedModules.md:16` · figure
  - As read: table rows :16-20, e.g. "| `golang.org/x/sync` | v0.19.0 | 4 | 27 | 1 | `linux/amd64` |"
  - Proposed: Exact per-module counts in a hand-maintained table with no generator (they match the MODULE.md totals 27/1, 54/0, 189/0, 37/0, 177/0). Generate the table by script, or drop the count columns and link the summaries.

- **F141** `docs/ValidatedModules.md:22` · stale
  - As read: "Measured with Go 1.24.13 at `Release` with the tiered JIT off, the test host's default. These five modules have not been re-measured with Go 1.26.10."
  - Proposed: Re-measure with Go 1.26.10 / 1.26.10.3; until then label the table as a Go 1.24.13 reading.

- **F142** `docs/ValidatedModules.md:29` · figure
  - As read: "the same machine matches all 8 of `semaphore`'s tests and all 28 of the module's. On `windows/amd64`, two runs on their own each matched all 28 verdicts"
  - Proposed: "matches every one of `semaphore`'s tests and every test in the module ... each matched every verdict"

- **F143** `docs/Glossary.md:10` · other
  - As read: "[`Baseline-vs-FullConversion.md`]\(../src/archived/Baseline-vs-FullConversion.md)"
  - Proposed: Not broken: .github/site-outside-links.py rewrites a link that leaves docs/ to its GitHub copy. It points a visitor at an archived internal file; keep, or drop the link

- **F144** `docs/Glossary.md:71` · tense
  - As read: "the stack-string `sstring` emission ... first fired on exactly **one** standard-library site, under a deliberately conservative rule."
  - Proposed: "...for example, a first increment that fires on a single site under a deliberately conservative rule."

- **F145** `docs/Glossary.md:141` · stale
  - As read: "each AOT publish compiles the full converted-stdlib closure, ~25 min per benchmark on an older six-core desktop, which is why a full perf run takes hours."
  - Proposed: Contradicts KnownIssues.md:207-210. "each AOT publish compiles every go.* assembly the program references in full ([Known issues]\(KnownIssues.md#a-native-aot-publish-of-a-program-that-references-the-packages-is-slow-and-large))." (also names no OS)

- **F146** `docs/Glossary.md:183` · stale
  - As read: "the converter emits MSBuild `.csproj` project files (carrying the `$(go2csPath)` property) plus `.slnx` solutions for the converted output."
  - Proposed: "...plus, under -recurse=nuget, .slnx solutions" (moduleConverter.go:720)

- **F147** `docs/CIMatrix.md:7` · other
  - As read: "See [`.claude/rules/corpus.md`]\(../.claude/rules/corpus.md) for corpus mechanics ... [`.claude/rules/harness-gates.md`]"
  - Proposed: The .claude/rules files are the fleet's agent instructions; the owner's OQ-1 ruling of 2026-10-26 (quoted in docsExcludedLinks_test.go) is that nothing reachable from go2cs.net carries the fleet's process, and the site rewrites these links to their GitHub copies rather than dropping them. Remove both links, or point to BuildingGo2cs.md / Architecture.md

- **F148** `docs/CIMatrix.md:10` · tense
  - As read: "The darwin leg replays the census-first pattern used to bring up Linux."
  - Proposed: remove

- **F149** `docs/CIMatrix.md:20` · tense
  - As read: "Compiled **cross-target from Windows**, it stopped at a known wall in `os`'s directory walk (the L3 gate lessons in ...)"
  - Proposed: remove (settled history; "L3 gate lessons" is jargon)

- **F150** `docs/CIMatrix.md:23` · tense
  - As read: "both have compiled clean since 2026-08-23 (below). This is the rule the standard library itself was brought up under"
  - Proposed: "Both compile clean."

- **F151** `docs/CIMatrix.md:50` · tense
  - As read: "A one-line compile error in the darwin flavor once went unnoticed for **seven days**, until the next darwin dispatch found it"
  - Proposed: "Without a schedule, a darwin-only break goes unnoticed until the next dispatch."

- **F152** `docs/CIMatrix.md:52` · platform-unnamed
  - As read: "A census is 10–17 runner minutes."
  - Proposed: Name the runners (macos-15 / macos-15-intel) and the date

- **F153** `docs/CIMatrix.md:61` · tense
  - As read: "Dispatch behaviour is unchanged by construction: nothing outside that one branch was re-decided. ..."
  - Proposed: remove (change narration)

- **F154** `docs/CIMatrix.md:67` · tense
  - As read: "darwin has compiled clean since 2026-08-23 (census run 32649840220, zero errors on both architectures)"
  - Proposed: "darwin compiles clean on both architectures."

- **F155** `docs/CIMatrix.md:99` · other
  - As read: "Dispatch capability is per-TOOL, not per-account: on a container whose egress policy blocks the Actions blob domain, `gh workflow run` returned **403** while the GitHub MCP `actions_run_trigger` returned **204**"
  - Proposed: Agent-session detail a visitor cannot use: remove

- **F156** `docs/CIMatrix.md:127` · other
  - As read: "so a framework hop cannot make it report zero on a green build"
  - Proposed: "so a target-framework change cannot ..." ("hop" is internal)

- **F157** `docs/CIMatrix.md:148` · other
  - As read: "`container/heap` is the default filter — the project's provisioning calibration row"
  - Proposed: "`container/heap` is the default filter, a small, quick row"

- **F158** `docs/CIMatrix.md:168` · tense
  - As read: "it calls every `sort` form whose converted body once called itself"
  - Proposed: "it calls every `sort` form, so a self-recursive converted body fails ..."

- **F159** `docs/CIMatrix.md:189` · tense
  - As read: "(1.24.13.7 ships both packages that way). The 1.24.13.6 rehearsal shipped decodemeta's Windows build in `lib/` only, ..."
  - Proposed: remove the release history; keep what arm G checks

- **F160** `docs/CIMatrix.md:196` · stale
  - As read: "(a converter that emits `unsafe.ArrayPointer`, which the published 1.24.13.3 lacks, fails here)"
  - Proposed: remove, or use a version-neutral example

- **F161** `docs/CIMatrix.md:202` · stale
  - As read: "gh workflow run os-matrix.yml -f stage=release-smoke -f published_version=1.26.10.1"
  - Proposed: Example names an old release: use 1.26.10.3 or <version>

- **F162** `docs/CIMatrix.md:218` · wrong
  - As read: "For this fixture's 31 assemblies that takes about 4 min on hosted linux and windows"
  - Proposed: The page's own budget row (360) gives linux 212 s, windows 167 s: "about 3 min"

- **F163** `docs/CIMatrix.md:220` · wrong
  - As read: "Before that it took about 70 min on hosted linux, which is why it became its own stage"
  - Proposed: Contradicts 232 and 360 ("linux-x64 56"); pick one figure, or remove as history

- **F164** `docs/CIMatrix.md:224` · tense
  - As read: "Without that default, the published 1.24.13.4 crashed such an executable at startup ... It was non-gating on its first release; from the release after"
  - Proposed: "A red holds the release."

- **F165** `docs/CIMatrix.md:230` · tense
  - As read: "Before golib's generic-cycle fix it read NOT MEASURED: on a hosted `macos-15` ... runner the publish did not finish inside the 6 hours"
  - Proposed: "osx-arm64 publishes in about 6 min (run 38587876788)."

- **F166** `docs/CIMatrix.md:239` · tense
  - As read: "an osx-x64 publish once ran past 6 h and the job was cancelled in its native link ... The AOT step now stops at `aot_bound_minutes`"
  - Proposed: "The AOT step stops at `aot_bound_minutes` (330 by default)."

- **F167** `docs/CIMatrix.md:280` · other
  - As read: "(`<an Azure blob-storage host>`, reached by a 302 from the GitHub API)"
  - Proposed: Rule 5 (real hostname): "(GitHub's Azure blob storage, reached by a redirect from the API)"

- **F168** `docs/CIMatrix.md:286` · tense
  - As read: "The summary and the artifact are written exactly as before"
  - Proposed: "The summary and the artifact are still written in full"

- **F169** `docs/CIMatrix.md:299` · other
  - As read: "**Whoever triggers a run reports its result.** ... A result that stays in a GitHub run page is a result nobody has."
  - Proposed: Internal team process: remove or recast for a contributor

- **F170** `docs/CIMatrix.md:306` · other
  - As read: "Relay a sweep row from its **comparison RECORD**, never from its pass line"
  - Proposed: "Read a sweep row from its comparison record ..."

- **F171** `docs/CIMatrix.md:327` · tense
  - As read: "Some stages are now measured (the census, the `pack` job, `aot-smoke`); the other budgets here are still estimates."
  - Proposed: "The census, `pack` and `aot-smoke` budgets are measured; the others are estimates."

- **F172** `docs/CIMatrix.md:342` · tense
  - As read: "The timeouts below are scaled estimates, not measurements. Some stages have since been measured"
  - Proposed: Say which are measured and which are estimates, in present tense

- **F173** `docs/CIMatrix.md:351` · stale
  - As read: Budgets table has no `golib-tests` or `stack-probe` row
  - Proposed: Add both (workflow: 90 min each, provisional)

- **F174** `docs/CIMatrix.md:358` · wrong
  - As read: "converter `go build` plus five small restore/build/run arms"
  - Proposed: release-smoke has seven arms (A-G, lines 166-192): "seven"

- **F175** `docs/CIMatrix.md:372` · other
  - As read: "warm state can never make the batch a no-op"
  - Proposed: "... make the run a no-op"

- **F176** `docs/CIMatrix.md:377` · tense
  - As read: "Everything below was untested by construction when it was written — the first dispatch was the experiment. ..."
  - Proposed: Turn the section into present-tense facts; drop the SETTLED entries (381-405, 414-416) or move them to history

- **F177** `docs/CIMatrix.md:389` · tense
  - As read: "**SETTLED — it could not at first; it can now.** The first darwin `behavioral-smoke` compiled and golden-matched 20/20, then failed all twenty ..."
  - Proposed: "A darwin `behavioral-full` runs on both legs, and the roster shows ..."

- **F178** `docs/CIMatrix.md:402` · figure
  - As read: "`fmt`'s closure carries thirteen L3 packages including `os`, `runtime` and `syscall`"
  - Proposed: "`fmt`'s closure carries platform-varying packages including `os`, `runtime` and `syscall`"

- **F179** `docs/CIMatrix.md:408` · stale
  - As read: "Linux has already proven those goldens platform-invariant for the behavioral corpus; darwin has not, and whether the converter's darwin path is exercised on **arm64** at all is itself untested"
  - Proposed: Contradicted by line 394 and by roster rows validated on osx-arm64: update or remove

- **F180** `docs/CIMatrix.md:414` · tense
  - As read: "`macos-15-intel` was a comparatively new label when this was written; its `osx-x64` legs have since run the census"
  - Proposed: "`macos-15-intel` runs the census, `behavioral-full` and `aot-smoke` (run 38575258530)."

- **F181** `docs/BeforeAndAfterTheFaceLift.md:3` · tense
  - As read: "Until release 1.24.13.5 the converted code carried a set of attributes ... The face lift removes them"
  - Proposed: The page is a before/after narrative. Move it to RoadmapHistory, or recast as "attribute form vs marker form" in present tense (individual lines 8, 14, 44, 81, 85, 181, 280, 310, 312 follow from that choice)

- **F182** `docs/BeforeAndAfterTheFaceLift.md:20` · figure
  - As read: "the attribute repeated it on more than five thousand methods of the standard library"
  - Proposed: "the attribute repeated it on every pointer-receiver method of the standard library"

- **F183** `docs/BeforeAndAfterTheFaceLift.md:315` · unmeasured
  - As read: "The standard library's own test suites validate at the same counts before and after."
  - Proposed: Name the platform and release of both readings, or link the roster; otherwise remove

- **F184** `docs/RoadmapHistory.md:37` · other
  - As read: "See [`/CLAUDE.md`]\(../CLAUDE.md) *One tree*." (also :84 and :152)
  - Proposed: Same OQ-1 point as CIMatrix.md:7: CLAUDE.md is the agent-instructions file. Point to BuildingGo2cs.md or Architecture.md, or remove the links (also :84 and :152)

- **F185** `docs/RoadmapHistory.md:692` · other
  - As read: "evaluated with the .NET toolchain hop, where publish shapes are re-priced anyway"
  - Proposed: "evaluated with the next .NET release migration, ..." ("hop" is internal jargon)

- **F186** `docs/RoadmapHistory.md:695` · other
  - As read: "disclosed tests keep running in every sweep, so the day the host becomes relocatable they begin passing, the sweep's disclosure arithmetic breaks loudly"
  - Proposed: "...in every validation run, ... the run's disclosure totals stop adding up" ("sweep" is internal jargon)

- **F187** `docs/StdLibCompileMilestone.md:20` · figure
  - As read: "**939 commits, of which 683 are individually-gated converter, runtime, and source-generator fixes"
  - Proposed: Dated history page; acceptable if dated: "about 940 commits by 2026-07-10, about 680 of them..."

- **F188** `docs/StdLibCompileMilestone.md:22` · stale
  - As read: "each one locked in by a behavioral regression test (371 and counting)"
  - Proposed: "(371 at the time)"; "and counting" reads as a live figure (the tree has 925 behavioral test directories)

- **F189** `docs/StdLibCompileMilestone.md:25` · figure
  - As read: "a rebuild of all 302 packages after every wave of merges to prove zero regression."
  - Proposed: "a rebuild of the whole standard library after every wave of merges..." (302 is the Go 1.23.1 count)

## 4. Changes outside the visitor pages

Two findings are fixed in code, not in the docs, because the page is right and the help text is not:

- `go2cs -h`'s `-stdlib` and `-tags` entries say the default tag is `purego`. The code applies `purego` and
  `math_big_pure_go` for `-stdlib` and for `-tests` (`commandLineOptions.go:276`, `defaultStdLibBuildTags`), as
  `CommandLine.md:60` and `ConversionStrategies.md:12482` say. Fix the help strings in `main.go`.
- `go2cs -h`'s `-hoist-literals` entry says the literal "becomes one static readonly field"; `Tuning.md:285`
  describes the field and its `ref readonly` accessor. Not a contradiction; align the help with the page.

`docs/Performance.md` is copied from `src/tests/Performance/README.md` (its line 563 says so), so every
Performance finding is made in that README.

## 5. Commands

Run on linux-x64 in a cloud container, with the go2cs dotnet tool 1.26.10.3 from nuget.org and a scratch
`HOME`, so nothing was installed machine-wide. The container's proxy blocks `go.dev`, `nugetgo.net` and most
GitHub pages other than the repository; where that changed a result, the row says so.

| Page:line | Command as a visitor pastes it | Result |
|---|---|---|
| `README.md:118`, `DotnetTool.md:7` | `dotnet tool install -g go2cs` | Works. Run as `--tool-path` (same package, no global install): `Tool 'go2cs' (version '1.26.10.3') was successfully installed.` |
| `README.md:152`, `DotnetTool.md:8` | `go2cs -version` | Works: names release 1.26.10.3. |
| `CommandLine.md:28`, `DotnetTool.md:52` | `go2cs toolchain env` | Works: downloads go1.26.10 once and prints `GOROOT`, `GOVERSION=go1.26.10`, `GO`. `go.dev` was blocked here, and the tool fell back to `proxy.golang.org` on its own. |
| `README.md:147`, `CommandLine.md:22` | `go2cs -recurse=nuget module_dir out` | Works on a two-package module (an app importing `github.com/google/uuid`): converts, `dotnet build` 0 errors, output matches `go run`. The mapping registry (`nugetgo.net`) was blocked, so uuid converted locally rather than mapping to its NuGet package. |
| `README.md:148`, `CommandLine.md:24` | `go2cs -recurse=module,nuget module_dir out` | Works: converts the app only, referencing uuid. |
| `README.md:149`, `CommandLine.md:18` | `go2cs package_dir out` | **Fails for a dotnet-tool user** (finding on `README.md:149`): converts with a warning that the runtime root is not a converted go2cs tree, and the output's `dotnet build` stops at 10 errors (CS0246). |
| `README.md:150` | `go2cs -platforms linux/amd64 -recurse=nuget module_dir out-linux` | Works. Run with `-platforms windows/amd64`, since linux is this host's default. Converted, not built. |
| `README.md:151`, `CommandLine.md:27` | `go2cs -tests -recurse -go2cspath <go2cs>/src -test-action all module_dir out` | Works against a clone at master: `Validated 1 tests against go test (0 skipped identically on both sides, 0 disclosed-unsupported declarations excluded).` It also prints `WARNING: ".../package_info.cs" is package main's package_info.cs, not main's; writing a fresh one from the template`, a message that reads as self-contradictory; worth a converter look. |
| `ConvertingAModule.md:39-180` | the `fatih/color` walkthrough: `go mod init`, `main.go`, `GOTOOLCHAIN=local`, `go get github.com/fatih/color@v1.18.0`, `go mod tidy`, `go build ./...`, `go2cs -recurse=nuget . csharp`, `dotnet build example.com.colordemo.slnx -c Debug`, `dotnet run -c Debug` | Works end to end (bash forms): 5 projects, build 0 warnings 0 errors, prints `hello from fatih/color`. `go` was the toolchain the tool downloaded. |
| `ConsumingGoFromCSharp.md:22-38` | `dotnet add package go.strings`, `dotnet add package go.fmt`, `dotnet run` | Works: resolves 1.26.10.3; prints `HELLO, GOPHER 13`. |
| `ConsumingGoFromCSharp.md:45` | `dotnet add package nugetgo.github.com.google.uuid` | **Fails at restore** in the same project (finding on that line): NU1107 on `go.unicode.utf16`. |

**NuGet names**, checked against nuget.org's flat container on 2026-11-09. A nugetgo package's fourth version part is written "(revision N)" here, since a dotted quad trips the identifier census's address arm:

| Package | Newest versions | Note |
|---|---|---|
| `go2cs` | 1.26.10.2, 1.26.10.3 | |
| `go.lib`, `go.gen`, `go.strings`, `go.fmt` | ... 1.26.10.2, 1.26.10.3 | |
| `nugetgo.github.com.google.uuid` | 1.6.0, 1.6.0 (revision 1), 1.6.0 (revision 2) | 1.6.0 (revision 2) depends on go.* [1.26.10.2, 1.27.0) |
| `nugetgo.golang.org.x.sync` | 0.19.0, 0.19.0 (revision 1), 0.19.0 (revision 2) | same range |
| `nugetgo.github.com.ritchiecarroll.hashset` | ... 1.0.0 (revision 3), 1.0.0 (revision 4) | go.lib [1.26.10.2, 1.27.0) |
| `nugetgo.github.com.golang-jwt.jwt.v5`, `...spf13.pflag`, `...spf13.cobra`, `...inconshreveable.mousetrap` | present | |
| `nugetgo.github.com.itchyny.gojq`, `nugetgo.github.com.itchyny.timefmt-go` | **not found** | named by `README.md:30`, `NEWS.md:167`, `Roadmap.md:313` |
| `nugetgo.github.com.fatih.color`, `...mattn.go-isatty`, `...mattn.go-runewidth`, `...itchyny.go-yaml`, `...clipperhouse.*`, `nugetgo.golang.org.x.sys` | not found | named only as conversion examples or inside comments, not as `dotnet add package` targets |

**Listed, not run.** These were collected with page:line by the readers and are kept in the audit kit; they
were not run here, for the reason given:

- PowerShell-only forms (`DotnetTool.md:65-66`, `ConvertingAModule.md:29, 60-68, 90, 103`,
  `BuildingGo2cs.md:67`): need a Windows host; their bash twins on the same pages were run where a bash twin
  exists (`ConvertingAModule.md:33, 72-80, 107`).
- Build-from-a-clone flows (`README.md:131-135` `go install .`, `BuildingGo2cs.md:26-43`, `Architecture.md:26-27`,
  `ConvertingAModule.md:274-304` `deploy-core.ps1`, `Performance.md:110-114` `run-performance.ps1`,
  `Glossary.md:38, 60`, `Roadmap.md:585-590`): contributor procedures that build the whole tree; the converter
  itself was built from this checkout with `go build`, and `-go2cspath <clone>/src` was exercised above.
- CI dispatches (`CIMatrix.md:77-78, 202, 268`, `gh workflow run ...`): they start runs on the repository, an
  outward-facing action this audit does not take. Their flags were read against `.github/workflows/os-matrix.yml`.
- `dotnet publish` variants (`KnownIssues.md`, `Limitations.md`, `Tuning.md`, `Performance.md:67-69`): each
  takes minutes per RID and Native AOT needs a native toolchain; not run in this pass.
- `dnx go2cs -- <arguments>` (`DotnetTool.md:17`): .NET 10's `dnx` was not exercised.

## 6. Not checked

- External links: the container's proxy answers 403 or 000 for most non-NuGet, non-repository hosts
  (`go.dev`, `nugetgo.net`, `go2cs.net`, GitHub pages other than the repository's own). Links to nuget.org and to
  the repository returned 200. The rest are unchecked, not broken.
- `Roadmap.md:341-342` ("The package-ID prefixes are not reserved yet"): NuGet's reservation status needs the
  web UI.
- `Roadmap.md:198` and `Roadmap.md:171-172` state .NET 11 and Go release dates; not verified offline.
- `Limitations.md:115` (several deferring functions on the stack) and `Limitations.md:190-192` (a variable-count
  shift on a named type): NEWS for 1.26.10.3 fixes neighbouring cases; whether these two are fixed was not
  confirmed.
- `BuildingGo2cs.md:12` says an SDK below 10.0.400 fails, while the comment on its expected-line run records SDK
  10.0.112. One of the two is wrong, or that run did not apply `global.json`.
- `ConvertingAModule.md:143-156` and several `ConsumingGoFromCSharp.md` snippets carry hidden "illustration:
  adapted" markers; they were not compared with real converter output.
- `ValidatedTestPackages.md`'s roster rows were checked for the naming and TypeScript rules and for internal
  consistency with the head figures, not row by row against the comparison records.
