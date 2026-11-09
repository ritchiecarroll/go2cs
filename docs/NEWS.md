# 📰 go2cs News Archive

![go2cs](images/go2cs-small.png)

All project announcements, newest first. The [README](README.md) summarizes where the project stands
today; every announcement is recorded here, and the detail-heavy ones link to a companion page carrying
their full text.

---

## November 8, 2026 — Faster Native AOT, and a Release standard library from source

<!-- Written ahead and HELD for release day (the .2 precedent, e665f4ea51): "November XX" becomes the publish date as the
     last change before the release tag (GoCorpusMigration.md, the release ritual, item 1). Moved from
     docs/DRAFT-news-1-26-10-3.md by the 1.26.10.3 release-controls batch (claude/g-release-controls-1263, COORD
     2026-11-08), every PENDING item that had landed flipped against its committed record; what still waits on the
     publish is marked PENDING with the publish as its trigger. -->

**NuGet 1.26.10.3.** The second fix release on the Go 1.26 line. The converted standard library is still built from
Go 1.26.10, and converting a module still needs a Go 1.26 toolchain and the .NET 10 SDK. One change below breaks both
C# source that uses the packages and assemblies already compiled against 1.26.10.2; it is first under *Upgrading from
1.26.10.2*.
<!-- The compiled-C# break, VERIFIED at the tree (G finding, COORD ruling 2026-11-07): at the nuget-1.26.10.2 tag golib's
     slice<T>.Enumerator implements IEnumerator<(nint, T)>; at 1.26.10.3 (batch 231) a type of the same name implements
     IEnumerator<T>, with the pairs on slice<T>.RangeEnumerator. So get_Current's return type
     differs, and an assembly built against .2 that loops over a slice throws MissingMethodException at its first loop
     under .3. A nugetgo package's go.* dependency is a range up to the next Go minor (PLAN-nugetgo B4), so NuGet restores
     a .2-built package against .3 without an error. The nugetgo rebuild wave ships with this release (C2). The published .2
     revisions are deprecated once the wave is live (owner, 2026-11-07: "Can deprecate existing published modules, no
     prob"); an owner-account action, with C2 preparing the package and version list. -->

**Upgrading from 1.26.10.2.**

- **Rebuild what you built against 1.26.10.2.** An assembly or nugetgo package compiled against go.* 1.26.10.2 that
  loops over a Go slice fails at its first such loop under 1.26.10.3, because a slice's enumerator now gives values (see
  the next item). NuGet restores an older nugetgo package under 1.26.10.3 without an error, so the failure shows only
  when the program runs. Rebuild against 1.26.10.3, and move each `nugetgo.*` reference to its 1.26.10.3 revision.
  **PENDING (the owner's deprecation, once the nugetgo wave is live).** The 1.26.10.2 revisions of the published
  nugetgo packages are deprecated on nuget.org, each naming its 1.26.10.3 revision as the alternate.
- **`foreach` over a slice gives its values.** A Go slice is `slice<T>`, and `foreach` over it gives
  its elements, as over any C# collection. Go's `(index, value)` pairs come from `range(s)`, and the indexes alone
  from `range(len(s))`:

  <!-- illustration: the loop forms from B3's ConsumingGoFromCSharp.md (bb0c2bbfae), itself an adapted example -->
  ```csharp
  foreach (var (i, w) in range(words))     // Go: for i, w := range words
  foreach (var w in words) letters += len(w);  // Go: for _, w := range words
  foreach (var i in range(len(words)))     // Go: for i := range len(words)
  ```

  `foreach (var (i, v) in s)` no longer compiles; write `foreach (var (i, v) in range(s))`. A loop written
  `foreach (var i in range(s))` for the indexes alone now receives the pairs, so write `range(len(s))` there. The
  same holds for a Go array and for a named slice or array type; a named slice type ranges with its element type
  named, `range<uint>(x)`. [Consuming Go from C#](ConsumingGoFromCSharp.md) has the details.
  <!-- source: batch 231 (P2 B3, aaebdc2fb2), docs/ConsumingGoFromCSharp.md. Landed. -->
- **A collection expression builds a slice** where C# knows the target is a `slice<T>`:
  `slice<nint> primes = [2, 3, 5];` as Go's `primes := []int{2, 3, 5}`. `[]` is an empty slice, not nil.

**Revisions within a Go release stay compatible.** From 1.26.10.3 on, every go.* and nugetgo
package is checked against its previous revision before it ships. Within one Go release line (1.26.10.x), a newer
revision keeps every public member the previous one had, so code built against it keeps running. The exception is a
break that a release announces under *Upgrading*, as this one does for `foreach`.
<!-- source: the binary-compatibility guard, batch 239 (claude/g-binary-compat-guard f1172d0d00 + claude/g-apicompat-noassets
     4cb835968d), docs/phase4/DESIGN-binary-compat-guard.md section 6 (the policy) and PLAN-rebank-1-26-10-3.md section 8.
     The whole-surface census read 362 packages and 525 assets, plus the 7 published modules' 23, with 0 failures after
     the two suppression files: B3's 12 in golib, and batch 235's 5 in vendored cryptobyte, which no consumer can reach
     and so take no line here. Landed: batch 239, master 2e2b0bae6e -> d07c7600cb (release-nuget -VerifyOnly reads
     "2 suppression file(s), 17 announced break(s)"). -->


**Faster under Native AOT.** Reading a slice element is now an inlined bounds-checked load under Native AOT, as it
already was under the JIT. On `win-x64`, on one development machine, against the build before this change: a
matrix-multiply benchmark ran in about 36% of the time, a prime sieve in about 51%, and a TLS handshake in about 75%.
The JIT and ReadyToRun times did not move.
<!-- source: docs/phase4/DATA-hop126-perf.md §11.9 (AOT D/P MatMul 0.359, Sieve 0.514, TlsHandshake 0.751; JIT and R2R
     flat in both the fd1 and the template shape). Batch 224 (R, AOT indexer inline, da3f68520d), landed. -->

Two earlier changes in this release make the same programs faster under Native AOT. A slice element read no
longer asks whether its element type takes no storage, so matrix-multiply, sieve and sort benchmarks take 13% to
18% less time. A 64-bit multiply with a 128-bit result now inlines, which together with the first change makes a TLS
handshake take about 14% less time. Both hold at the default x64 instruction set. Building a converted program for
`x86-64-v3` (`<IlcInstructionSet>x86-64-v3</IlcInstructionSet>`) gives no gain across the benchmark suite, and an
image built that way refuses to start on a processor without AVX2, BMI1, BMI2, F16C, FMA or LZCNT.
<!-- source: DATA-hop126-perf.md §11.7 (option A: MatMul 0.823, Sieve 0.865, Sort 0.867; with the Mul inline,
     TlsHandshake 0.856; win-x64, default instruction set) and §11.6 (x86-64-v3 suite geomean 1.000; the cited
     DetectCPUFeatures fail-fast and the avx2 group). Batch 209. -->

The first type assertion to an interface a type satisfies by its methods, without declaring it, is about twice as
fast under Native AOT: about 117 ms in place of 254 ms in a small program on `linux-x64`, on one 4-core development
machine. Native AOT now reads a list of extension-method hosts written at build time instead of scanning every type.
The JIT is unchanged.
<!-- source: docs/phase4/DATA-extension-host-list.md §3 (AOT first assert 254.0 / 251.9 ms base, 119.7 / 114.4 ms with
     the list; linux lane box, 4 cores, ILC 10.0.14), DATA-aot-structural-assert.md (L1). Batches 210 and 219. -->
- Under Native AOT, a type assertion that needs a variadic method promoted through an embedded
  pointer, such as `testing.T`'s `Error(...any)`, now matches Go.
  <!-- source: DATA-extension-host-list.md §4 names the gap (line 38, TT value has Error). P1's fix: batch 225 (variadic shell, 0094ed89a7), landed. -->

**Single-file publishes start fast.** A plain `dotnet publish -p:PublishSingleFile=true` of a converted program now
compiles ReadyToRun one assembly at a time, as the publish profiles go2cs writes already do. The SDK's default for a
self-contained single file is one composite image, and compressed inside the single file it is decompressed at every
start. On `win-x64`, on one development machine, a small converted program started in about 0.47 s against about
3.5 s, from a 33 MB executable against a 571 MB one. A project that wants the composite image sets
`<PublishReadyToRunComposite>true</PublishReadyToRunComposite>`.
<!-- source: DATA-hop126-perf.md §11.8 (PerfStartup wall: per-assembly compressed 470 ms, composite compressed 3,480 ms;
     composite uncompressed 320 ms; exe 33 MB vs 571 MB). Batch 214 (C2, csproj-template). -->

**A conversion against standard library sources builds Release.** A program converted with `-recurse` or
`-recurse=module` against a local go2cs source tree (`-go2cspath`) no longer gets a solution file beside its project.
So `dotnet build -c Release` in the program's folder builds the standard library in Release, and
`dotnet publish -c Release -r <rid>` there succeeds. With 1.26.10.2, that build compiled the standard library in
Debug, about 21 times slower on a TLS handshake under the JIT, and the publish failed. A solution file an earlier
converter wrote is removed only when it is exactly the one that converter wrote. A program converted against the
published packages (`-recurse=nuget`) keeps its solution and was never affected. To build any converted program, name
its project file: `dotnet build MyProgram.csproj -c Release`.
<!-- source: DATA-hop126-perf.md §11.9 (the three-way build experiment, 64/67 Debug through the folder or the .slnx, 0/67
     through the csproj; TLS 1,071 ms via the csproj). Batch 223 (golib's publish guard) and batch 227 (H, no solution
     beside a local-reference conversion's project: 0 of 46 Debug, was 45; -recurse=nuget keeps its solution), landed. -->

**Faster P-521 and P-384.** The P-521 field multiply is hand-written, constant-time code in place of the generated
code, which made the first P-521 operation in a process, which builds a precomputed table, take 42.6 ms under
ReadyToRun where it took 520 ms, on `win-x64`, on one 4-core laptop. P-384's field multiply is rewritten the same
way: a P-384 scalar multiplication takes about a quarter of its earlier time under ReadyToRun (about 38 ms to under
10 ms) and about a third under the JIT, on `win-x64`, on one laptop.
<!-- source: docs/phase4/DATA-p521-table.md (R2R table build 520 ms -> 42.6 ms; JIT 93.7 -> 29.1 ms; win-x64, 4-core
     laptop H). Batch 218. P-384: docs/phase4/DATA-p384-field.md §6 (R-LAPTOP, win-x64, tiering off: ScalarMult
     ReadyToRun 38.3-38.7 ms -> 9.31-9.64 ms, JIT 15.5-15.6 ms -> 5.18-5.41 ms), batch 238 (claude/r-p384-field),
     landed. -->

**Faster P-256 handshakes.** P-256's field multiply is rewritten the same way. A TLS 1.3 handshake with a P-256
certificate and ECDHE takes about two thirds of its earlier time under ReadyToRun (about 62 ms to about 39 ms), on
`win-x64`, on one laptop. The default handshake, with an RSA certificate, does not change.
<!-- source: docs/phase4/DATA-p256-field.md §6 (R-LAPTOP, win-x64, tiering off: the P-256 TLS 1.3 handshake arm
     ReadyToRun 59.3-65.0 ms -> 38.2-39.6 ms, ratio 0.640/0.629; the default RSA arm 0.985, within noise), batch 245
     (claude/r-p256-field), landed. -->

**AES-GCM on Apple silicon.** On `osx-arm64`, `crypto/tls` prefers the AES-GCM cipher suites, as Go does there. The
converted standard library is built for `amd64` and runs as a native arm64 process on Apple silicon, so it now
reads the processor's AES and PMULL support from arm64.
<!-- source: src/core/crypto/tls/go2cs_test_disclosures.json note (batch 216, C1); DESIGN-aeshash-arm64.md. -->

**On Apple silicon, `runtime.GOARCH` still reports `amd64`.** The macOS packages are built once, for `darwin/amd64`,
and serve both Macs, so a converted program on an Apple-silicon Mac reports `amd64` where Go reports `arm64`
([Known issues](KnownIssues.md#on-apple-silicon-runtimegoarch-reports-amd64)). From this release each Mac's results
are compared with Go's own build for that Mac, so the difference now shows in validation. The tests of
`go/internal/gcimporter`, `debug/buildinfo` and `net/http/pprof` read the architecture: on the hosted `osx-arm64`
runners they differ from Go's, and on the hosted `osx-x64` runners they pass. A macOS count needs both, so those three
packages are not validated on macOS in this release. Reporting the real architecture on Apple silicon is planned work.
<!-- source: COORD 2026-11-08 (the docs gap the .3 darwin leg exposed). The per-Mac oracle is batch 208 (C1 (A),
     claude/c1-oracle-arch, -test-oracle-goarch) and the arch-strict banking rule (ledger 2026-11-06 09:06, ruling (1);
     check-roster-format section 2h). The constant is BOARD 2026-11-05 (C1), measured on the GitHub-hosted osx-arm64
     runner (run 41964964302); KnownIssues.md carries the at-master constant citations. Landed with the bank (batch 258,
     11eec8fff2). The darwin leg at 72043c1a66 (docs/phase4/rebank-1.26.10.3/darwin): debug/buildinfo C1-3 run
     42477307357, go/internal/gcimporter C1-5 run 42481111673, net/http/pprof C1-10 run 42486625026; each validated on
     osx-x64 and failed on osx-arm64, and under arch-strict banking (both RIDs must validate and agree) none banks a
     darwin segment (the roster rows' class (s) notes). -->

**`internal/synctest` is not validated on macOS in this release.** One of its tests holds about 10,000 goroutines
alive at once, and each converted goroutine is an operating-system thread, past the number of threads macOS lets one
process run; on the hosted macOS runners, `osx-arm64` and `osx-x64` alike, that test fails. [Limitations](Limitations.md#what-converts-but-doesnt-behave-exactly-like-go)
describes the limit and how to stay under it.
<!-- source: COORD 2026-11-08; C1's run 42497561630 (claude/c1-darwin-threadcap a85e0cf11) on the GitHub-hosted
     macOS runners, both mac RIDs: TestWaitGroupManyBubbles (about 10,100 live goroutines) dies with
     System.OutOfMemoryException from the thread start; kern.num_taskthreads 2048 (osx-arm64, macOS 15.8.2) and 3072
     (osx-x64, macOS 15.7.9). Landed with the bank (batch 258): internal/synctest's darwin leg C1-8 (run 42485762191)
     reads 56 of 59 matched with 3 divergent on BOTH RIDs, so it banks no darwin segment (the roster row's class (c)). -->

**PENDING (the publish: the nugetgo wave). gojq for .NET.** [`itchyny/gojq`](https://github.com/itchyny/gojq),
a jq implementation in Go, is a NuGet package: `nugetgo.github.com.itchyny.gojq` 0.12.19 gives a .NET program an
in-process, fully managed jq, with no native binary per platform. Its one dependency,
[`itchyny/timefmt-go`](https://github.com/itchyny/timefmt-go), ships with it as `nugetgo.github.com.itchyny.timefmt-go`
0.1.8. The package is gojq's library. Its command-line package is not included: gojq's own `gojq` command already does
that job. Every package in gojq 0.12.19 converts with no change to its Go source. The library's own tests are
converted and run against Go's: all 164 results match, with no exceptions, and that proof ships in the package as
VALIDATION.md. The command-line package's tests convert and pass in full too, 914 of 914, even though the package
ships the library only. The converted sources are in
[ritchiecarroll/gojq-cs](https://github.com/ritchiecarroll/gojq-cs) and
[ritchiecarroll/timefmt-go-cs](https://github.com/ritchiecarroll/timefmt-go-cs).
<!-- source: OQ-4 RULED 2026-11-07 (DESIGN-multi-package-modules.md; the owner on 4e: "Yes, command line deffered is fine,
     lib focus for .3"), PLAN-rebank-1-26-10-3.md section 8 (the closure plan: -ExcludePackage github.com/itchyny/gojq/cli;
     timefmt-go first, then gojq against it), SCOUT-gojq.md (P1) and SCOUT-timefmt-go.md (C2). The conversion repositories
     were created empty by the owner on 2026-11-07. PENDING until both packages publish in the wave; delete the item if
     gojq misses it.
     Counts filled 2026-11-07 from COORD's relay of P1's read at master 663561b7a3, where every gojq seat A-H had landed,
     with no source patches:
     - gojq v0.12.19 converted 16/16 packages;
     - the library root validated 164/164 Go test verdicts, 0 disclosed (the figure its package proof carries);
     - the cli validated 914/914, 0 disclosed, and is not packed at .3;
     - only standing-rule exclusions apply (benchmarks, examples, a fuzz target).
     COORD's ask was visitor voice, with no exact corpus count beyond the two validated figures; the 16 stays here. Before
     release, re-read the 164 against the published package's VALIDATION.md. timefmt-go's own count is not quoted:
     its package carries it. -->


**Also in this release:**

- A panic raised inside a deferred closure keeps the closure's frame in `recover`, `runtime.Callers` and the crash
  traceback, as in Go. <!-- batch 217 (R B1) -->
- Calling a value-receiver method through a nil pointer panics, as in Go, including when a pointer method calls a
  value method on its own receiver; there the arguments are also evaluated in Go's order. <!-- batch 221 (C2); batch
  235 (C2, claude/c2-thisref-box 13715e9df6): the self-call is emitted over the box, at TC0 as at TC1. Landed. -->
- A crash on a goroutine other than `main` prints that goroutine's id and, under its frames, Go's
  `created by <function> in goroutine <id>` line. <!-- batch 178 (C1, .3 S1) -->
- `runtime.Goexit` called from `main` runs `main`'s deferred calls, then waits for the other goroutines, as in Go.
  <!-- batch 190 (C1 D1) -->
- A deferred or `go` value-method call on a pointer copies the value at the `defer` or `go` statement, as in Go.
  <!-- batch 195 (C2) -->
- A shift by a constant count on a map element, a named type or a type parameter follows Go's shift rule.
  <!-- batches 164, 174, 186 (P1) -->
- Converted code reads closer to Go:
  - an `if` or `switch` with an init statement is no longer wrapped in its own block where Go's scope allows;
    <!-- batch 204 (H, init-scope) -->
  - a value method called through a pointer reads `p.M()`, not `(~p).M()`; <!-- batch 189 (C2) -->
  - blank lines Go does not have are dropped, and a short function stays on one line as Go wrote it;
    <!-- batch 222 (P2, class K) -->
  - a file imports `System.Runtime.CompilerServices` only where it writes a `MethodImpl` attribute.
    <!-- batch 166 (P1) -->
- A string literal's cache lookup costs less under full optimization. <!-- batch 167 (H) -->
- A conversion that runs long prints a warning at 2, 4, 8 and 16 minutes and every 16 minutes after.
  <!-- batch 179 (P2, .3 D2) -->
- A conversion no longer turns on cgo because Go would; it converts with `CGO_ENABLED=0` unless the environment sets
  it. <!-- batch 168 (C1) -->
- On Linux and macOS, a program started with `SIGINT`, `SIGQUIT` or `SIGCONT` ignored, as a background job started
  from a script is, keeps that ignore until it calls `signal.Notify`, and then receives the signal, as in Go. It used
  to die by such a `SIGINT` even without `signal.Notify`.
  <!-- batch 257 (G, claude/g-sigint-inherited-ign 2e3c8dd78b): GolibTests InheritedIgnoredSignalTests red at master,
       green at the fix, on linux (G-WSL) and on both mac RIDs (CI run 42488916420). -->
<!-- The "Also in this release" list was compiled from the merge log through batch 206 when the draft was written
     (2026-11-07); at release controls (2026-11-08) batch 257's signal fix was added. Batches 207-259 were not otherwise
     re-read for this list (most are records, guards, the bank and the gojq seats, which the gojq item covers); COORD may
     trim or extend. -->
<!-- COORD to trim: the "Also in this release" items from batch 164 on were added by G from the merge log since the
     1.26.10.2 tag (0b1ef0fc77), beyond the brief's list; each is user-visible by its merge subject. Not listed: test
     and release tooling, guards, records, the tour, docs-only batches, and 175 (NuGet-mappings provenance report),
     176 (test host temp root), 182 (bounded goroutine-stack mark), 205 (darwin getfsstat marshal) and 206 (extension
     scan on builtin uint64), each either internal or for COORD's call. -->

---

## November 4, 2026 — Another package's variables by address, and converted code that reads like Go

<!-- Written ahead and held unmerged until release day; dated November 4, 2026 (first dated November 3; the release ran past midnight after the tool-package fix) and landed as the last change before
     the release tag (GoCorpusMigration.md, the release ritual, item 1). -->

**NuGet 1.26.10.2.** The first fix release on the Go 1.26 line. The converted standard library is still built from
Go 1.26.10, and converting a module still needs a Go 1.26 toolchain and the .NET 10 SDK. Two of the changes below
break compiled C#. C# source that uses the packages compiles unchanged, apart from the one call listed under
*Upgrading from 1.26.10.1*, but an assembly built against an earlier package must be rebuilt against this one.

**Upgrading from 1.26.10.1.**

- **Rebuild what you built against 1.26.10.1.** An assembly or nugetgo package compiled against go.* 1.26.10.1 must
  be rebuilt against 1.26.10.2, or have its nugetgo.* references pinned to their 1.26.10.2 revisions. A read of
  another package's variable changed shape, and NuGet still restores a 1.26.10.1-built nugetgo package under
  1.26.10.2 without an error, so the failure shows at run time.
- `new StandardBox<T>(default)` no longer compiles; spell `nil` for a nil box or `default(T)` for a boxed zero value.

**Another package's variables, by address.** Taking the address of a variable another package exports now
reaches the variable, as in Go: `&otherpkg.V`, `flag.BoolVar(&otherpkg.Debug, ...)`, a pointer-receiver method
value such as `f := otherpkg.V.Reset`, and an address inside it, `&otherpkg.S.Field` or `&otherpkg.Arr[i]`. Only a
pointer- or interface-typed variable is still handed over as a copy ([Limitations](Limitations.md)). **This is a
breaking change for compiled C# code**: each exported variable of a value type is now a `ref` property over its
storage, not a field, so an assembly compiled against 1.26.10.1 must be rebuilt against 1.26.10.2. C# source that
reads or assigns `pkg.V` compiles unchanged. go.* packages now depend on each other at the exact release version,
so a project that mixes 1.26.10.1 and 1.26.10.2 packages fails at restore and names them. A read of such a variable
costs about 0.5 ns more than a field read on the JIT, and about 0.7 to 0.9 ns more under Native AOT (measured on
`win-x64`, one development machine).

**String constants the compiler can inline.** In the converted standard library a public string constant is now
read through a same-named property the JIT and Native AOT inline; C# that uses one compiles unchanged, but an
assembly built against an earlier package must be rebuilt.

**Install go2cs as a .NET tool, with no Go installed.** `dotnet tool install -g go2cs` installs the converter, a
native executable for Windows, Linux and macOS on x64 and Arm64. The first conversion that needs Go downloads the
Go release go2cs converts with, checks it against hashes built into go2cs, and keeps it in your user cache folder.
`go2cs toolchain` installs that Go ahead of time, or from a file on a machine with no network access.
[The go2cs dotnet tool](DotnetTool.md) has the details. Building the converter from source still works, and now
needs the .NET 10 SDK 10.0.400 or later ([Building go2cs](BuildingGo2cs.md)); converting a module needs no particular
.NET 10 SDK, as before.

**Converted code reads closer to the Go.** Converted C# is meant to read like the Go it came from, and this release
removes most of the remaining wrapping:

- A field through a pointer reads `p.f`, as in Go, where it read `(~p).f` or wrote `p.Value.f`; its address reads
  `p.Ꮡf`.
- A comparison with `nil` reads `err != nil`, and a zero value reads `default`, where both read `default!`.
- A slice literal reads `slice<error>([e1])`, a C# collection expression, where it read
  `new error[]{e1}.slice()`. A struct's zero literal reads `new T()`.
- A rune literal reads `'x'` wherever C# takes a char, without a `(rune)` cast.
- A `switch` on a value becomes a C# pattern switch, `case ' ' or '\t':`.
- `else if` stays on one line, an opening brace follows a space as gofmt writes it, and a variable whose address
  escapes is declared and initialized in one statement.
- Trailing comments that gofmt aligned stay aligned, and the comments C# still dropped are carried.
- A pointer parameter keeps its Go name, a conversion between numeric types takes one cast, and an endless loop
  reads `for (;;)`.

**Faster from the first call.** A converted program counts calls toward optimizing a method from the start, rather
than after .NET's usual quiet period of about 100 ms. On `win-x64`, a TLS handshake benchmark ran about a third
faster under the JIT, and a trivial program took about 24 ms longer to start. A ReadyToRun publish precompiles golib
and the standard library and leaves the program's own code to the JIT, which can then inline across it: on the
same machine, a published loop calling `strconv.FormatBool` ran in about an eighth of the time it took when
everything was precompiled.
`<GoReadyToRunUserCode>true</GoReadyToRunUserCode>` precompiles your own code too, and
`<GoCallCountingDelayMs>runtime</GoCallCountingDelayMs>` restores .NET's delay. [Tuning](Tuning.md) has the
measurements and every setting.

**Single-file publishes keep ReadyToRun.** The publish profiles go2cs writes beside a converted program now keep
ReadyToRun on and compile it one assembly at a time. 1.26.10.1's profiles turned it off, because a single-file
publish compiled as one composite image runs out of memory; a profile publish now gets ReadyToRun's faster start.
A program converted by an earlier go2cs keeps its old profiles, since go2cs never overwrites one:
[Known issues](KnownIssues.md#a-single-file-publish-with-composite-readytorun-runs-out-of-memory) says what to change.

**A goroutine's crash is not lost when `main` returns.** When `main` returns while another goroutine is running a
panic's deferred calls, the program waits while that goroutine can still run, as Go does, so the crash report
prints and the process exits with code 2. The wait used to be a fixed 20 ms, and on a busy machine a crashing
program could exit 0 with nothing printed. A goroutine blocked in its deferred calls, on a channel or in a system
call, does not hold the exit.

**Large package-level data is built on first use.** A package-level variable that holds a large pure-data value,
such as one of `unicode`'s tables, is built the first time it is read, not when its package loads. 1.26.10.1
already did this; it was not announced then.

**Also in this release:**

- A Native AOT program that uses `net` or `crypto/tls` runs again; on 1.26.10.1 it could exit with an error the
  first time it passed a struct pointer to a system call
  ([Known issues](KnownIssues.md#with-the-go-126101-packages-a-native-aot-program-that-uses-the-network-can-exit-with-an-error)).
- A type switch's `case int:` no longer also takes an `int32` or a rune, and `case uint:` no longer takes a `uint32`.
- A deferred or `go` method call on an array element uses the element as it is at the statement, as Go does.
- A goroutine the converter proves cannot recurse reserves a 16 MiB stack, where others reserve 256 MiB, so a
  program that starts many goroutines starts them faster ([Tuning](Tuning.md#goroutine-stacks)).
- A package-level `sync.WaitGroup` stays outside a `testing/synctest` bubble, as in Go.
- Panic messages name a Go `int32` as `int32` and a byte as `uint8`.
- `fmt` handles a Go `uint16` that C# holds as a `char`: `%v` of one no longer overflows the stack, and `%T` of a
  function type that uses one names `uint16`, not `Char`.
- Two pointers to the same element of natively viewed memory, such as an `mmap`-ed page seen as a slice and as an
  array pointer, compare equal, as in Go.
- `runtime.Caller` in a deferred or `go` function reports the caller Go reports, not an internal wrapper frame.
- Converting a single `.go` file names the output after that file, and a relative `-go2cspath` resolves.
- The release's Windows executables are Authenticode-signed, and its `SHA256SUMS` carries a GPG signature
  ([Limitations](Limitations.md)).
- A new guide, [Tuning](Tuning.md), covers choosing a deployment for converted code and the .NET runtime settings
  that matter for it.
- A package's validation page explains each test it discloses in plain words.
- The [Tour of go2cs](https://github.com/ritchiecarroll/go2cs/blob/master/src/tour/README.md) gains a Generated Code
  tab, which shows the source generators' output for the converted program.

---

## October 26, 2026 — The converted standard library moves to Go 1.26.10

**NuGet 1.26.10.1.** The converted standard library is now built from Go 1.26.10, a release Go still
supports, so it carries the Go security fixes issued through Go 1.26.10. Converting a module needs a Go 1.26
toolchain (any 1.26.x release) and the .NET 10 SDK as before; the standard library packages are converted from, and
validated on, Go 1.26.10. 1.24.13.7 stays the last release built from Go 1.24.
<!-- Corrected 2026-10-26 (settled ruling): the entry first said "Converting needs Go 1.26.10, exactly that release,
     and the .NET 10 SDK as before." That was wrong for module conversion, which needs only a Go 1.26 toolchain;
     the exact-release pin applies to converting and validating the standard library itself. -->

**Every package that can be validated passes Go's own tests at Go 1.26.10**, on Windows x64 and on
Linux x64. Each package was re-converted from Go 1.26.10's sources and re-validated from Go 1.26.10's
own test files, so every row's counts on the [roster](ValidatedTestPackages.md) are Go 1.26.10 readings.
Validation on macOS continues package by package.

**New and removed packages.** Go 1.26 adds packages and removes or renames others, and the `go.*`
packages follow it. 28 package IDs are new in this release:

go.crypto.hpke, go.crypto.internal.constanttime, go.crypto.internal.entropy.v1.0.0,
go.crypto.internal.fips140.mldsa, go.crypto.internal.fips140cache, go.crypto.internal.fips140deps.time,
go.crypto.internal.rand, go.crypto.mlkem.mlkemtest, go.go.internal.scannerhooks, go.internal.cgrouptest,
go.internal.routebsd, go.internal.runtime.cgroup, go.internal.runtime.gc, go.internal.runtime.gc.internal.gen,
go.internal.runtime.gc.scan, go.internal.runtime.pprof.label, go.internal.runtime.startlinetest,
go.internal.runtime.syscall.linux, go.internal.runtime.syscall.windows, go.internal.strconv,
go.internal.testhash, go.internal.trace.internal.testgen, go.internal.trace.internal.tracev1,
go.internal.trace.tracev2, go.math.big.internal.asmgen, go.net.http.internal.httpcommon,
go.testing.cryptotest, go.testing.synctest.

Ten package IDs have no Go 1.26 package behind them. Each is deprecated on nuget.org, pointing at
1.24.13.7, the last release that carries it, and at its Go 1.26 successor where there is one:

go.crypto.internal.hpke, go.internal.itoa, go.internal.runtime.syscall, go.internal.trace.event,
go.internal.trace.event.go122, go.internal.trace.internal.oldtrace,
go.internal.trace.internal.testgen.go122, go.log.slog.internal.slogtest,
go.runtime.internal.startlinetest, go.vendor.golang.org.x.net.route.

**A limitation to know about: the address of another package's variable.** When code takes the address
of a variable that a different package exports and that is not itself a pointer, the pointer refers to
a copy, so writes through it never reach the variable. That covers `&otherpkg.V`, a pointer-receiver
method value such as `f := otherpkg.V.Reset`, and the common `flag.BoolVar(&otherpkg.Debug, ...)`,
where parsing the flag leaves `otherpkg.Debug` unchanged. A direct call (`otherpkg.V.Reset()`) and a
read both work, and inside one package all of these work. To avoid it, keep the variable and the code
that takes its address in one package, or have the package export a pointer or an accessor function.
[Limitations](Limitations.md) and [Known issues](KnownIssues.md#writes-through-the-address-of-another-packages-pointer-or-interface-variable-are-lost)
carry the details.

**`crypto/tls` and process start-up.** Part of `crypto/tls`'s test suite runs BoringSSL's interop
cases, and it starts a new helper process for each one. A converted helper process takes about 10 s to
start and run its handshake, where Go's takes about 85 ms, measured on a Windows x64 desktop: .NET's
process start and package initialization, then compiling the handshake path in each new process. The
whole interop run would take over an hour, so it is reported as a host limit, and every other
`crypto/tls` test validates. The same machine read the same cost on 1.24.13.7: it is the per-process
cost of a converted program, not a Go 1.26 change. A program that starts many short-lived converted
processes pays that start-up each time. Reducing it (ReadyToRun-compiled test hosts, or tiered
compilation in spawned child processes) is planned work.

---

## October 14, 2026 — Converted code keeps Go's comments and spacing, and HTTPS works on macOS

**NuGet 1.24.13.7.** This is the last release built from Go 1.24. The next one moves the converted
standard library to Go 1.26.

**Comments and blank lines land where Go put them.** Converted C# is meant to read like the Go it came
from, and until now its comments and spacing did not always follow. A type's doc comment could be
missing, a comment inside a function could land after the code it described, and the blank lines that
group statements in a Go function were dropped or doubled. This release fixes those. One real example,
from the converted `internal/fmtsort`. The Go:

```go
// SortedMap is a slice of KeyValue pairs that simplifies sorting
// and iterating over map entries.
//
// Each KeyValue pair contains a map key and its corresponding value.
type SortedMap []KeyValue
```

Before, the declaration stood alone, with no comment:

<!-- source: src/core/internal/fmtsort/sort.cs:24 -->
```csharp
partial struct SortedMap /*[]KeyValue*/;
```

Now:

<!-- source: src/core/internal/fmtsort/sort.cs:20-24 -->
```csharp
// SortedMap is a slice of KeyValue pairs that simplifies sorting
// and iterating over map entries.
//
// Each KeyValue pair contains a map key and its corresponding value.
partial struct SortedMap /*[]KeyValue*/;
```

What changed, by kind:

- **Blank lines inside a function body** follow Go's, in every conversion.
- **Doc comments** stay with the type, alias or declaration group they document.
- **Comments inside expressions**, in a composite literal, between arguments, or beside an operand, are
  written at their Go position.

Comments are carried when you convert with `-comments`, as the converted standard library is. The blank
lines apply to every conversion. Nothing that runs is different: the change is in comments and
whitespace, and every validated package holds its count.

**A multi-line raw string keeps its exact bytes.** A Go raw string literal that spans lines now converts
to a C# raw string literal, closed by a call that restores Go's rule for such literals. The string has
the same bytes as in Go however the file's line endings were checked out.

**HTTPS on macOS.** Verifying a certificate against the system's roots reached an unimplemented stub on
macOS, so an ordinary `https` request from a converted program failed there, on Intel and on Apple
silicon. It now calls the system's own verification, as Go does.

**Native AOT, fully trimmed.** Two defects that showed only in a Native AOT publish with `TrimMode` set
to `full` are fixed. The default publish and programs running under the JIT never had them.

- **`fmt` calls a value's own methods again.** A type's Go methods are no longer trimmed away when they
  are reached only at run time, so `fmt` calls `String()` or `Error()` and no longer prints the raw
  value. A `time.Duration` prints as `1.5s`, not as a count of nanoseconds. This is the defect 1.24.13.6
  shipped with and listed under Known issues.
- **Reflection reaches a struct's fields through a pointer.** Reading or setting a field by reflection on
  a struct reached through a pointer, with `reflect.ValueOf(&v).Elem().Field(i)` or `FieldByName`, and
  taking that field's address, now works. In 1.24.13.6 it stopped with a `PlatformNotSupportedException`.

The price is size again, and the choice is the same as before: behaving like Go comes first. Measured on
`win-x64`, a fully trimmed C# program that references the packages is about 26% larger than it was in
1.24.13.6.

**Two pointers to one field are equal, however the field was reached.** A field promoted from an embedded
struct can be named two ways, `&s.X` and `&s.Inner.X`. The two pointers now compare equal, as they do in
Go. Before, they could compare unequal. This one applied to every program, under the JIT as well.

**Starting a goroutine is cheaper.** The runtime library recorded where each goroutine was started by
walking the whole call stack. It now resolves that once for each `go` statement. Code that starts
goroutines from deep recursion was the worst case, and is the one that improves most.

**Converted modules.** Packages published through [nugetgo.net](https://nugetgo.net) can now carry
separate assets for each platform, as the `go.*` packages do, for a module whose Go source differs by
operating system.

**Also in this release.** The [Tour of go2cs](https://github.com/ritchiecarroll/go2cs/blob/master/src/tour/README.md)
pins the converter and package versions it runs with. The README says plainly what to expect from the
project: it is one person's work, replies are best-effort, and it says where to ask.

The `go.*` packages are built and versioned together, so use one version of them in a project.

## October 12, 2026 — Reflection works under Native AOT, and macOS validation passes three quarters

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
([Known issues](KnownIssues.md#a-native-aot-publish-of-a-program-that-references-the-packages-is-slow-and-large)). Three cases still take the old path under Native AOT only: a conversion to an
interface inside generic code, a container reachable only through a field another package does not
export, and a struct whose own package never takes a pointer to it.

One defect is known in a fully trimmed publish: `fmt` prints some named types in their raw form
([fixed in 1.24.13.7](#october-14-2026--converted-code-keeps-gos-comments-and-spacing-and-https-works-on-macos)).
The default publish is not affected, and the fix is in the next release.

**Converted modules as NuGet packages.** Three Go modules are published at
[nugetgo.net](https://nugetgo.net): `hashset`, `google/uuid` and `golang-jwt/jwt` v5. The tooling that
packs them now refuses a package whose standard-library references are not a published release, records
no path from the machine that built it, and binds the package's validation record to the exact files
packed. A package's README on nuget.org is the author's own: the pack keeps one fenced block in it up to
date, with the badges and the provenance, and leaves the rest alone. For a conversion, `-nuget-map-feed`
reads mapped packages from a feed you name, such as a local folder, instead of nuget.org
([Command-line options](CommandLine.md#options)).

**Every `go.*` package says how it was checked, and how to call it.** The Tests badge on each package's
README now reads the two numbers that matter, tests matched and tests disclosed, and links the package's
proof page. Below it, one line names the C# class that holds the package's functions, so a C# developer
who lands on `go.strings` knows to call `go.strings_package`.

**Two calls that reached a stub now answer as Go does.** A measurement of which exported functions can
reach an unimplemented stub found two on Windows and Linux.
<!-- The measurement: [the record](phase4/CENSUS-phase5-exported-reach.md) (an internal record; its visible link was
     removed 2026-10-26 under owner ruling OQ-1 on fleet language in visitor pages). -->
`runtime/coverage.ClearCounters` now returns Go's
own error for a program built without coverage, and `runtime.Breakpoint` breaks into an attached
debugger, or without one stops the program with Go's report line.

**macOS.** Validation of the standard library on macOS passes three quarters of the roster: 171 of 225
packages now carry a macOS count, each agreed between Apple silicon and Intel
([the roster](ValidatedTestPackages.md)). Four fixes came out of that work. `os.Executable` returns the
program's path. The memory page size is the machine's real one, 16 KB on Apple silicon, where it was
fixed at 4 KB. `hash/crc32` uses the processor's CRC instructions on Arm64. And on Linux and macOS
`os.Args[0]` is the name the program was started with, as it is in Go, so a program that starts itself
again under another name recognizes itself. Verifying a certificate against the system's roots still reaches a stub on macOS; that
fix is promoted ahead of the rest of the macOS work.

The `go.*` packages are built and versioned together, so use one version of them in a project.

## October 9, 2026 — Converted code reads like Go, and Native AOT runs on Windows

<!-- attribute-shown: the announcement names the attributes that left converted code -->
**go2cs converts Go into C# that a Go developer can read, and this release removes most of what stood
in the way.** Until now, converted code carried attributes that told the build what Go's own syntax
already says: `[GoRecv]` on every method with a pointer receiver, `[GoType]` on every type, and others
on embedded fields, struct tags and array parameters. In the converted standard library they stood on
more than five thousand method declarations alone. They are gone from converted code.

Nothing they said is lost. Each fact now sits where a Go reader already looks:

- **In the signature.** A pointer receiver is `this ref T`. A Go type is a `partial struct` or
  `partial interface` declared in its package. A function with a string-view twin takes an `sstring`.
- **In a short comment written the way Go writes it.** A defined type states its underlying type after
  its name, `partial struct Duration /*num:int64*/;`. An embedded field says `/*embed*/`. A struct tag
  follows its field in Go's backquoted spelling. An array's length is Go's own prefix, `/*[32]*/`.

The go2cs source generators read the signature or the comment at build time and write the attribute form
into generated code, where the runtime and `reflect` find it. So the converted file is left with the
code, and what it compiles to is unchanged.
[Converted code, before and after the face lift](BeforeAndAfterTheFaceLift.md) shows nine real examples from
the converted standard library on both sides of the change, each beside the Go line it came from, and
[the reference](ConversionStrategies-Reference/source-generators.md#marker-comments-what-converted-code-carries-instead-of-an-attribute)
lists every marker and the few attributes that stay, with the reason for each.

The whole standard library was converted again for this release, and every validated package holds its
count: the roster's packages pass Go's own tests at the same verdict counts on Windows and on Linux as
before the change. Hand-written C# is untouched by it. A file written by hand keeps its attributes, and
the generators and the runtime read either spelling. The `go.*` packages are built and versioned
together, so use one version of them in a project.

**A C# project that references the `go.*` packages now runs under Native AOT on Windows.** With the
1.24.13.4 packages such a program, published with `PublishAot`, stopped on Windows at its first call to
`runtime.Caller`, which Go's `log` and `testing` packages reach. It now runs. The Native AOT check that
gates the release publishes a C# consumer of the packages and runs it on `linux-x64`, `win-x64` and
`osx-x64`, and it passes on all three. `osx-arm64` is not measured. A project that sets no trim mode of
its own gets the packages' default, a partial trim, so the startup failure the 1.24.13.4 packages showed
without that setting is gone too. The publish is still slow: over an hour on a hosted CI machine for Linux and for Windows, and several
hours for an Intel Mac.

**A fully trimmed Native AOT build runs for the first time.** With `TrimMode` set to `full`, a consumer
of the packages used to stop at startup, because a full trim removed members the runtime library reaches
by reflection. Each package now registers the Go types it defines, and the consumer runs. The cost is
size: a fully trimmed executable keeps every Go type the program can reach, so it is about twice as large
as before (12.4 MB to 24.5 MB for the measured consumer), where it used to keep less and fail. The
default partial trim is unchanged, in size and in behavior. The runtime library's own trim warnings fall
from 76 to 32 in the same change. This is measured on `win-x64`, on one development machine; the hosted
check measures the default trim.

**Fixes since 1.24.13.4 that a user of the packages or the converter sees:**

- **Stack traces keep their file and line numbers.** The packages now carry their symbol files, so a
  program that references them gets Go file names and line numbers in a panic's traceback and from
  `runtime.Caller`. Publishing a converted program a second time no longer loses them (Windows, Linux and
  macOS). No assembly or symbol file in the packages names a path of the machine that built it.
- **A caller's line is the line of its call.** A call inside a statement that spans several lines reports
  its own line, as Go does, where it used to report the statement's first.
- **The converter accepts more Go.** An explicitly instantiated generic function passed as an argument,
  an exported alias of an unexported type, a parenthesized type in a type assertion, a function-local
  pointer type used as a conversion, a defined type over another package's type used as a conversion, a
  promoted method with a parameter named `target`, and a method on a named map or channel type that
  shares a name with the library type's own member each convert and compile now.

## October 1, 2026 — Every implementable standard-library package validates

**Every implementable package in Go 1.24.13's standard library now validates its own test suite in
C#.** That is **225 of 225**, and **225 of the 230** testable packages, **97.8%**, at **69,777
matching verdicts** against `go test -json`, with **373** divergences disclosed by exact failure
signature. On Linux, all 223 applicable rows validate at their own Linux counts, at 69,617 matching
verdicts with 392 disclosed. The other two, `internal/syscall/windows` and
`internal/syscall/windows/registry`, are Windows-exclusive and have nothing to compile there.

The last two packages validated today. **`runtime` validates at 10,819 matching verdicts, with 71
disclosed** (10,810 and 73 on Linux). This is the Go runtime's own suite, run against the managed
runtime that replaces Go's. It covers the scheduler and preemption, channels and `select`, finalizers
and cleanups, and `defer`, `panic` and `recover` in every ordering, with Go-spelled tracebacks and
crash reports read back from child processes. It also covers maps, stack growth and Windows
callbacks. One test, `TestMapIterDeleteReplace`, contributes 10,001 of those verdicts through its
subtests, and the roster row says so. Of the 71 disclosures, 33 name something the managed runtime
cannot truthfully describe: Go's SEH unwinder, its inline tree, GC bitmaps, the tiny allocator's
layout. Another 24 are deferred. They are allocation counts, plus one live-heap metric that the
managed host has no Go mark phase to produce, and each carries a measured reading and the plan that
retires it. When the Go 1.23.12 record closed on September 7, this suite could be read only as far as
index 104, where a crash killed the test host. Now every verdict is read and accounted for.

**`runtime/pprof` validates at 145 matching verdicts, with 7 disclosed** (147 and 7 on Linux). It
covers CPU, heap, goroutine, block and mutex profiles taken in the managed runtime, labels raced
across goroutines, and the profile builder's proto encoding. Six of its tests wait for another
goroutine's interior frames in `runtime.Stack`. Because the CLR cannot walk a foreign thread's stack,
those six are withdrawn from both sides by name.

The September 24 announcement stood at 218 of 230. Since then, every one of the six implementable
packages it named as not yet validated has validated. `net/http` validated again on September 25, at
all 1,387 verdicts, once Go 1.24's `internal/synctest` was supported. `internal/synctest` itself validated
at 28 of 28 on September 27, the same day as `reflect`, now at 396 matching with 22 disclosed.
`net/http/pprof` validated at 15 of 15 on September 28, and `runtime` and `runtime/pprof` complete the six
today. A seventh package joined because the implementable set itself grew. On September 28,
`runtime/trace` stopped being an exclusion: go2cs's managed execution tracer writes a trace that Go's own parser accepts,
both of its verdicts matched, and the implementable set grew from 224 to 225. The same day
`runtime/debug` left the roster, when its disclosure for `TestStack` was found to match by substring
and absorb assertion lines it should not have. It returned on September 30, at 8 matching with 1
disclosed, and `TestStack` is now a plain pass.

The figure needs to be read exactly. **A row is all-or-nothing.** A package joins only when every
eligible `Test` verdict matches Go's or is disclosed, so a package matching every verdict but one
scores zero. A disclosure names a specific Go assertion that the converted suite provably cannot
satisfy, and it is pinned by exact failure signature in a committed manifest, so any other failure is a
hard mismatch. Disclosures are never a tolerance, and nothing is skipped quietly; the few tests
withdrawn are withdrawn from both sides, by name. Validation counts `Test` functions. Runnable
examples, fuzz seed corpora and benchmarks are the next validation slice. Within that scope, this is
the goal Phase 4 was given. The 100% counts finished packages, judged by Go's own tests, and the 373
disclosures are where the record says what a managed runtime cannot do, or does not do yet. Every
one of the 225 rows links a proof page that lists Go's verdict beside go2cs's, test by test.

Five testable packages sit outside the implementable set. Every one of them is an internal package
that no Go program outside the standard library can import, and each is listed with its class,
mechanism and evidence. `runtime/internal/wasitest` is a WASI test package that runs no test on
`windows/amd64`, because its only selecting test skips unless the target is `wasip1/wasm`.
`internal/unsafeheader` builds live slices and strings by writing raw header fields, a memory model
a managed runtime deliberately does not have, so a pass would be fabrication rather than implementation.
The other three run cleanly but validate nothing about the port. `net/internal/cgotest`'s
only test has an empty body, `internal/copyright` scans the GOROOT both sides share for copyright
notices, and `crypto/internal/fips140deps` shells out to `go list` to check the Go tree's import
policy. An excluded package rejoins the count the day its evidence changes, as `runtime/trace`'s did.

Go's own tests first passed in C# on [July 17](#july-17-2026--gos-own-tests-now-pass-in-c), with
`unicode/utf8`'s fourteen verdicts. Seventy-six days later, the last implementable package validated.

The converted standard library ships as **NuGet 1.24.13.3**, targeting .NET 10. It is the first
release published with every implementable package validated. Every proof page is frozen at
`validation/1.24.13.3` for the badges the packed READMEs link, and the exact shipped tree is
browsable at the `nuget-1.24.13.3` tag.

With the standard library validated, the work turns to real Go code brought to .NET. Whole modules
will be converted together and validated against their own test suites, so that a module is judged by
its own tests the way the standard library is. Alongside that comes nugetgo.net, a registry for
converted Go modules.

*Roster commit `133ca704e` · tag `nuget-1.24.13.3`*

## September 24, 2026 — The converted standard library moves to Go 1.24.13, and 218 packages validate against it

**go2cs now converts Go 1.24.13's standard library**, and the validated roster crossed the hop
re-derived, not carried: **218 of the 230 testable standard-library packages validate their own
Go 1.24.13 test suites in C#** — **56,974 matching verdicts** against `go test -json`, with **283**
divergences disclosed by exact failure signature — and, measured against the 224 packages a faithful
managed conversion can honestly validate at all, **97.3%**. On Linux, 187 of the 216 applicable rows
validate at their own Linux counts, at 53,048 matching verdicts. Every validated row is proved by a
run at Go 1.24.13.

The count rises by fourteen from the Go 1.23.12 record's 204 while the testable set grows by fifteen,
so both percentages dip — 97.6% to 97.3% of the implementable set, 94.9% to 94.8% of the testable
one. **One package validated at Go 1.23.12 is not validated at Go 1.24.13: `net/http`.** It matches
1,370 of its 1,387 verdicts, and all 17 divergences trace to tests run under Go 1.24's new
`internal/synctest`, the runtime support behind the experimental `testing/synctest`, which go2cs does
not yet support; it still ships as `go.net.http`. Go 1.24 moved ten validated packages to new import
paths, and their tests validate under the packages that now hold them. Counting those, twenty-five
rows join, among them `unique` and Go 1.24's new `crypto/hkdf`, `crypto/mlkem`, `crypto/pbkdf2`,
`crypto/sha3` and `weak`. Six implementable packages are not yet validated: `reflect`, `runtime`,
`runtime/pprof`, `net/http/pprof`, `net/http` and Go 1.24's new `internal/synctest`.

On Linux the count falls from 198 to 187: the relocated packages' tests are not yet run on Linux at
their new paths, and `go/internal/srcimporter` has no Linux result, because Go's own test of it fails
on the Linux reference machine; it stays validated on Windows. The verdict total doubles because Go's
suites grew — `crypto/cipher` alone matches 27,272 verdicts, against 13 at Go 1.23.12 — so verdict
totals do not compare across Go releases.

Converted programs get Go 1.24's APIs — `os.Root`, `weak.Pointer`, `crypto/mlkem` and
`strings.Lines`, among others — and `//go:embed` support. go2cs itself now builds with Go 1.24.13, and
a `-recurse=nuget` conversion needs its module to resolve to Go 1.24; to stay on Go 1.23, build the
converter at the `nuget-1.23.12.3` tag with Go 1.23.12 and use the 1.23.12.3 packages. Go 1.24's
FIPS 140-3 module converts and its tests validate, but go2cs makes no FIPS 140-3 claim: the module's
integrity self-check hashes a binary layout that a .NET assembly does not have, so under
`GODEBUG=fips140=on` the converted check reports success without verifying anything.

Fifty-one package IDs are new, six of them for packages a Go program can import: `go.crypto.fips140`,
`go.crypto.hkdf`, `go.crypto.mlkem`, `go.crypto.pbkdf2`, `go.crypto.sha3` and `go.weak`. Fourteen end
at 1.23.12.3, their last release, because Go 1.24 moved or deleted their packages; none is importable
outside the standard library, and each stays restorable at 1.23.12.3 and is never unlisted.

The converted standard library publishes as **NuGet 1.24.13.1** — 344 packages, author-signed, still
targeting .NET 10 — with every proof page frozen at `validation/1.24.13.1` for the badges the packed
READMEs link, and the exact shipped tree browsable at the `nuget-1.24.13.1` tag. The Go 1.23.12
record stays frozen as it shipped, at `validation/1.23.12.3`.

*Full story: [The converted standard library moves to Go 1.24.13](news/2026-09-24-stdlib-moves-to-go-1-24-13.md)
· tag `nuget-1.24.13.1`*

## September 7, 2026 — The Go 1.23.12 record closes at its anchor; the corpus hops to Go 1.24

**The Go 1.23.12 validation record is closed.** It stands at **204 of the 215 testable
standard-library packages** — **28,459 matching verdicts** against `go test -json`, with **167**
divergences disclosed by exact failure signature — and, measured against the 209 packages a faithful
managed conversion can honestly validate at all, **97.6%**. On Linux, 198 of the 202 applicable rows
validate at their own Linux counts, at 23,199 matching verdicts. Those figures are an **anchor**
rather than a running total: the corpus now moves to **Go 1.24.13** instead of driving this release
to 100% first.

Five packages remain unbanked here — `reflect`, `runtime`, `unique`, `runtime/pprof` and
`net/http/pprof` — and none of them is written off. A version hop re-derives **every** roster row from
the new release's own test sources — numerator, denominator and disclosure set alike — so all five
re-validate against Go 1.24 on exactly the footing of the 204 that banked. What holds each back
differs: `reflect` is measured and under active converter and runtime work; `runtime` is measured
only as far as a host-killing crash — `TestCrashWhileTracing`, at index 104 of its 883 verdicts —
which leaves everything after it unread; `unique` is measured at 19 of its 20 verdicts under the
configuration of record and waits on planned runtime-model work; and `runtime/pprof` and
`net/http/pprof` are each measured on two hosts and held by a capability or classification decision.
<!-- Pre-2026-10-26 wording: "waits on a ruled runtime-model arc" / "held by a capability or classification
     ruling". -->

The reordering is deliberate, and it is worth saying why, because the percentage means something
narrower than it looks. **The metric is package-based, not content-based**: a row is all-or-nothing,
so a package matching most of its verdicts still scores zero, exactly as one matching none of them
does — which is what keeps the roster honest, and what makes the headline a count of finished
packages rather than of passing tests. What is left is therefore a long tail of
runtime-model work — Go's own heap layout, its type descriptors, its profiling internals — that does
not stand between a user and a converted program. The project is measured against a trustable platform
that migrates packages and applications from Go to C#, and a more recent Go standard library unlocks
more of that than the tail does. This is not a compromise but a reordering: the hop that unlocks user
capabilities happens first, and the remaining guardrails are handled after it.

The 1.23.12 corpus ships one final NuGet release as that anchor before the pin moves — the release
that freezes its roster, its proof pages and every package README at the record above. It has to come
first: the pin bump resets the build counter, so once the corpus names Go 1.24 there is no longer any
version left in which the 1.23.12 record could ship.

## August 29, 2026 — Over 90% of the standard library's test suites pass in C#

**189 of the 215 testable standard-library packages validate their own Go 1.23.12 test suites in
C#** — **26,043 matching verdicts** against a clean `go test -json` baseline, with **148** divergences
disclosed by exact failure signature and nothing else waived. Seven of the 215 cannot be validated at
all — no eligible tests on this platform, a broken upstream oracle, or a suite whose whole subject is
the raw memory layout a managed runtime deliberately does not have — so against the implementable set
of 208 the roster reads **90.9%**. On Linux, 178 of the 188 applicable rows validate at their own Linux
counts, at 21,807 matching verdicts.

The push past 90% landed **`net` itself — 472 verdicts, the roster's largest networking row** — banked
the day two phantom DNS servers and a router forwarder stopped impersonating an oracle. A host whose
*own* `go test net` fails is disqualified as a reference, because the two sides of a differential would
then be running different oracles; qualifying one was the work. `reflect` compiled and executed for the
first time alongside it, and `net/netip`, `net/http/cookiejar` and `debug/pe` joined the roster in a
single day.

The converted standard library publishes as **NuGet 1.23.12.2** — 307 packages, author-signed — with
this release's 189 proof pages frozen at `validation/1.23.12.2` for the badges the packed READMEs link,
and the exact shipped tree browsable at the `nuget-1.23.12.2` tag.

*Added to the archive on September 7, 2026; the announcement itself ran on the project's front page
from August 29. Release freeze and announcement: `773afa2c2` · published: `d2da277f5`*

## August 25, 2026 — Both runtime pins move: .NET 10, Go 1.23.12 — and the whole roster re-proves itself

**go2cs now targets .NET 10 and converts Go 1.23.12**, and the validated roster crossed the hop the
only honest way: re-derived, not carried. Every one of the **162 banked packages re-validated from
the new release's own `_test.go` sources** — **18,598 matching verdicts** against a clean
`go test -json` baseline at the new pins, with the 85 disclosures re-signed against the new
sources — count and per-row set unchanged. The
29 new verdicts over the 1.23.1 record come exactly from the four suites whose verdict counts
moved with the release (`time`, `os/exec`, `syscall`, `database/sql`), reconciled to the digit
across the machines that ran the re-validation shards. A version hop invalidates every row's numerator,
denominator and disclosure set by design; there is no carry-forward path, so agreement at the new
pins is measured, never assumed.

The .NET half moved 955 project files to `net10.0` with zero corpus-emission drift across all three
OS flavors — and immediately earned its keep: C# 14's new implicit array-to-span conversion
changed which overload form a variadic call binds, exposing that the corpus's variadic-slice
binding had been correct *by accident* under C# 13. The converter now emits the binding that is
correct on purpose.

The converted standard library publishes as **NuGet 1.23.12.1** — each package README freezing its
validation badges against this release's own proof snapshot, with the exact shipped tree browsable
at the `nuget-1.23.12.1` tag.

## August 22, 2026 — Over 75% of the standard library's test suites pass in C#

**162 of the 215 testable standard-library packages (75.3%) validate their own Go 1.23.1 test
suites in C#** — 18,569 matching verdicts against a clean `go test -json` baseline, with
85 signature-pinned disclosed divergences. Each suite is converted from Go's own
`_test.go` sources, built against the converted standard library, run under the Go-semantics test
host, and compared verdict for verdict; a package joins the roster only when every eligible test
agrees.

The milestone is tagged because something genuinely hard landed with it. **A converted stack frame
now reports a Go file and line.** The position is derived from conversion-time facts rather than
composed — file and line ship together, because either alone names a position that exists in
neither tree — and the identity is build-shape-faithful: the published standard library reports
the same trimpath form Go bakes into a published binary, while a converted user module reports the
source path Go would have baked for that build. `runtime/debug`, `log`, `log/slog` and `flag` all
rest on it.

Three more capabilities landed alongside. **Channel direction and map key/element dimensions ride
the type descriptor**, so `reflect` distinguishes `<-chan int` from `chan int` and a decode target
can measure an array it has never populated — which retired an entire disclosure class on its own
recorded remedy and banked both template packages. **A native pointer slot holds the pointer's
value**, not a managed reference, closing a round-trip that had also hidden a GC-invisible
dangling-reference hazard. And **an always-on GC recorder** supplies real pause and release facts
to `runtime/debug`'s statistics surface at zero cost per read.

What the work declined is part of the record. A disclosure names something the managed runtime
provably cannot satisfy — never something merely unimplemented — so packages that reach 105 of 106
or 210 of 267 stay off the roster with their remaining root named and priced. Four disclosure
classes carry the roster's divergences today; a fifth retired itself the day its remedy landed,
exactly as it was written to.
<!-- Pre-2026-10-26 wording: "What the campaign declined ..." / "carry the campaign's divergences today". -->

This is Go 1.23.1's terminal validation marker. Every roster row re-derives from a release's own
test sources at a version hop, so validation continues on Go 1.23.12 — the release users would
actually choose — with the 1.23 story living on its own branch.

## August 8, 2026 — Go programs run on Linux

Converted Go programs now **run on Linux, byte-identical to `go run`**: `fmt.Println("hello, 世界")`,
a program crossing `os.Args`, `os.Getenv` and `time.Now()`, and the README's own
[real-world walkthrough](README.md#converting-a-real-world-module) — `fatih/color` printing true ANSI
colour under a real PTY, with the `isatty` branch agreeing with Go in both directions (plain when piped,
coloured on a terminal). The whole effort landed in one continuous arc: the repository checks out
deterministically on any filesystem, the converter and every harness instrument run natively on Linux,
the standard library compiles for **windows, linux and darwin from one tree** (per-GOOS source folders
selected by `$(GoTargetOS)`, windows the default, with 141 of 141 shared-source packages measured
IL-identical across flavors), and each Go package ships as **one NuGet package** carrying RID-specific
assemblies only where source genuinely varies.

At the bottom of it all sits **one measured keystone**: Linux's entire syscall surface crosses the
kernel through a single `libc syscall(2)` binding whose three claims were probed rather than argued —
the variadic ABI with a real six-argument `mmap`, the second return register shown to be *exact* (the
kernel preserves `RDX`), and errno round-tripped through a deliberate fault. The road there surfaced
exactly two converter defect families and a handful of linkname wiring gaps — including the lesson that
a forward alone can *pass a run and be wrong* (`os.Args` silently empty), which is why every wiring row
now pairs with its populated truth. The FFI surface simultaneously converged on source-generated
`[LibraryImport]` bindings, where non-blittable signatures fail at **compile time** — three latent
marshalling hazards surfaced during the migration, each converted explicitly.

Stated plainly: the published `1.23.1.4` packages still carry Windows-only assemblies — the Linux
experience ships with the next release; a Linux consumer of the few platform-divergent packages also
needs the compile-surface answer scheduled next; and darwin binaries are compile-proven and
IL-identity-backed but have never been executed here. Windows did not move a byte through any of it:
every merge kept the converter's behavioral-test output byte-identical, the behavioral suite green, and the 110-package validated sweep at
13,628 verdicts with zero failures.
<!-- Pre-2026-10-26 wording: "The Windows lane did not move a byte through any of it" (the lane that held the
     Windows gates while Linux work merged); "the campaign continues on Go 1.23.12"; "The whole campaign landed". -->

## August 8, 2026 — Over half the standard library validates; defers reach zero allocation

Three days took the validated roster from 73 packages to **110 of 215 (51.2%)** — 13,628 matching
verdicts against `go test`, 50 disclosed divergences, not one added in the climb. The largest single
advance was also the quietest: a re-scout of never-measured packages found **34 that validated with no
changes at all** — the corpus had grown past them as shared machinery landed. The loudest was
architectural: **`defer` now compiles to an inline `try`/`catch`/`finally` over a `ref struct` frame** —
no closure object, no delegate per defer, zero allocation for non-capturing defers — replacing the
execution-context lambda while *improving* behavioral fidelity (a capture-semantics divergence class
died by construction). Three runtime capabilities were hand-implemented on managed primitives following
the established pattern: Go's concurrent hash-trie map, weak pointers, and caller-frame walks. And three
user-reported `-recurse` failures — a quoted `go.mod` directive, a C# keyword inside an import path, and
a type renderer chasing its own tail through `<-chan` — were each root-caused, fixed, and answered on
the issue the same day, ending with `gopkg.in/yaml.v3` converting, compiling, and running byte-identical
to `go run`.

## July 26, 2026 — More than a quarter of the standard library's test suites pass in C#

**57 of the 215 testable standard-library packages validated their own Go test suites in C# — 26.5%
of the Phase-4 target**, at **1,459 matching test results** with **47** honestly disclosed as
divergent rather than quietly dropped. A package counted only when *every* `Test` function's verdict
matched a clean `go test -json -count=1` baseline. The set moved well past leaf packages: `sync`'s own
concurrency suite, the RE2 engine in `regexp`, `strconv`'s float formatting, the `crypto/sha*` family,
and the reflection-driven `errors`, `encoding/binary` and `go/token` — the last round-tripping a
`FileSet` through the real converted `encoding/gob`. The same push made goroutine exit, string-literal
allocation and structural interface assertion behave the way Go specifies, and corrected two published
performance figures that had been measuring the wrong artifact. Per-package counts are in
[Validated Test Packages](ValidatedTestPackages.md); the measured numbers in [Performance](Performance.md).

*Full story:
[More than a quarter of the standard library's test suites pass in C#](news/2026-07-26-quarter-of-stdlib-tests-pass.md) · commit `44fcc4f04` · reproduce any row from a
clone via [Try it yourself](README.md#try-it-yourself--validate-a-converted-test-suite)*

---

## July 18, 2026 — `unicode/utf16` validates; disclosed-divergence generalizes

**Phase-4 package #5.** [`unicode/utf16`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/unicode/utf16)
validated its own Go test suite in C# — **8 tests agreeing outright** against `go test -json`, plus one
honestly disclosed. The structural twin of the very first validated package (`unicode/utf8`), it
round-trips UTF-16 encode/decode with results checked by `reflect.DeepEqual`, exercised through the
converted reflection bridge. Its significance was the ninth test: `TestAllocationsDecode` asserts a
zero-allocation `Decode`, which Go reaches only through compiler escape analysis and a managed runtime
provably cannot match. `unicode/utf16` was the first package to reuse the disclosed-divergence manifest
as a **general tool** rather than a two-package special case, pinning that one `alloc-profile` row by
exact failure signature while a separate test proved the decoded output correct — a mechanism that
generalizes cleanly is a mechanism that was designed right.

*Full story:
[`unicode/utf16` validates; disclosed-divergence generalizes](news/2026-07-18-utf16-validates.md) · Phase-4 package #5 · 8 + 1 disclosed (alloc-profile)
· reproduce from a clone via [Try it yourself](README.md#try-it-yourself--validate-a-converted-test-suite)*

---

## July 18, 2026 — `bytes` and `strings` tests pass, with disclosed-divergence

**Two more standard-library packages validated their own Go test suites in C#** — and they arrived with
a new piece of Phase-4 machinery. [`bytes`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/bytes)
validated **81 tests** and [`strings`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/strings)
**68** against `go test -json`, bringing the Phase-4-validated count to four after `unicode/utf8` and
`sort`. Both contain tests that assert an **exact allocation count** via `testing.AllocsPerRun` —
unsatisfiable by design in a managed runtime, where the divergence is the allocation *model*, not the
measurement. Rather than silently skip them, go2cs began disclosing them at test level: a hand-owned,
repo-committed `go2cs_test_disclosures.json` pins `{test, divergence class, expected failure
signature}`, and the differential oracle reclassifies a result only when both the test name and the
pinned signature match — an integrity guard, not a blanket exemption. Packages without a manifest
compare strictly.

*Full story:
[`bytes` and `strings` tests pass, with disclosed-divergence](news/2026-07-18-bytes-strings-disclosed-divergence.md) · Phase-4 packages #3 and
#4 · `sort` 63/63, `bytes` 81, `strings` 68 · reproduce from a clone via
[Try it yourself](README.md#try-it-yourself--validate-a-converted-test-suite)*

---

## July 17, 2026 — Go's own tests now pass in C#

**A standard-library package's own Go test suite — converted to C# — ran and agreed with `go test`,
verdict for verdict.** [`unicode/utf8`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/unicode/utf8)'s
real test suite (Go 1.23.1) validated **14/14** through the new converted-test pipeline: the `_test.go`
files are transpiled to C#, built against the converted standard library, executed under a Go-semantics
test host, and differentially compared against a clean `go test -json` baseline by full test name — with
every benchmark and example declaration honestly disclosed rather than silently skipped. One week after
"the whole standard library *compiles*," the answer to *"but does it **run**?"* had its first
machine-checked proof, [reproducible from a clone](README.md#try-it-yourself--validate-a-converted-test-suite).
This opened the Phase 4 operational era — **real Go tests, not compilation, are the currency of
correctness** — with `sort`, `strings` and `bytes` next in line.

*Tag: [`utf8-tests-green-2026-07-17`](https://github.com/ritchiecarroll/go2cs/releases/tag/utf8-tests-green-2026-07-17)
· commit `337a928df`*

---

## July 14, 2026 — The converted Go standard library is on NuGet

**The converted Go standard library, the `golib` runtime, and the `go2cs-gen` analyzer are published to
[nuget.org](https://www.nuget.org/packages?q=go2cs%20ritchiecarroll)** as `go.<pkg>` /
[`go.lib`](https://www.nuget.org/packages/go.lib) / [`go.gen`](https://www.nuget.org/packages/go.gen),
versioned `1.23.1.<build>` from `src/version.props`. The converter's new `-recurse=nuget` mode emits
matching `<PackageReference>` entries — defaulting `$(GoStdLibVersion)` to a floating release — so a
converted end-user app or library restores the whole go2cs stack from NuGet with **no local go2cs source
checkout**; the app's own and third-party converted packages stay project references. See
[Converting a real-world module](README.md#converting-a-real-world-module) for the end-to-end walkthrough.

*Tag: [`nuget-stdlib-2026-07-14`](https://github.com/ritchiecarroll/go2cs/releases/tag/nuget-stdlib-2026-07-14)
· commits `2363af0e6`, `2e15eec9d`, `dd821a556`*

---

## July 10, 2026 — The entire Go standard library compiles in .NET

**All 302 packages of the auto-converted Go standard library (Go 1.23.1) compile
cleanly as .NET assemblies — zero errors, zero exclusions.** Every package you'd expect to be hard is
in that number: `runtime`, `reflect`, `net/http`, `go/types`, `crypto/tls`, `database/sql`,
`encoding/json`. The transpiled output is not a demo subset — it is the standard library, end to end,
emitted by the converter, transpiled Go to C#, then compiled by Roslyn. NOTE: don't get _too_ excited,
this is _fully compilable_ not _fully runnable_ — that's the next phase (underway; see the July 17 item
above)! However, simple apps will run, try
[converting a real-world module](README.md#converting-a-real-world-module). Read more about this
[milestone's details](StdLibCompileMilestone.md) and
[current status](README.md#status) in the README.

*Tag: [`stdlib-green-2026-07-10`](https://github.com/ritchiecarroll/go2cs/releases/tag/stdlib-green-2026-07-10)
· commit `5cc07f8f33`*

---

## June 27, 2026 — The `math` package compiles clean

The full-conversion **`math` package compiles clean** — a core, widely-imported standard-library
package, and with it a major step in the Phase 3 drive to compile the whole auto-converted standard
library. The session that landed it greened nine full-conversion packages (`unicode`,
`internal/trace/event`, `unicode/utf16`, `internal/platform`, `image/color`, `runtime/internal/sys`,
`runtime/internal/math`, `math/bits`, and `math`) via 19 behaviorally-tested converter and generator
fixes. The dominant theme was comprehensive untyped-constant typing, plus shadowing fixes,
namespace-collision qualification, composite self-qualification, and relational-pattern guards.

*Tag: [`math-green-2026-06-27`](https://github.com/ritchiecarroll/go2cs/releases/tag/math-green-2026-06-27)
· commit `914d4bd72`*

---

## May 5, 2025 — First full standard-library auto-conversion

The rewritten Go-based converter completed its **first full standard-library auto-conversion**: the
whole Go standard library (~301 projects) converted end to end. "Converted" here meant the transpiler
did not crash and every Go source file received a corresponding C# file — not yet that the emitted C#
compiles. Driving this full conversion to a clean compile became Phase 3, finished on
July 10, 2026 (above).

*Tag: [`full-conversion-2025-05`](https://github.com/ritchiecarroll/go2cs/releases/tag/full-conversion-2025-05)
(`cc14584c7`, May 11) · commit `6ca1c45b7`*

---

## January 12, 2025 — The converter is rewritten in Go ("go2cs" version 2)

**Major project restructuring** — the "go2cs iteration 2" generation begins: the converter is
re-implemented **in Go** on the official `go/ast` + `go/types` toolchain, replacing the original C#
converter built on an ANTLR4 Go grammar; T4 templates are replaced by raw string literals; and Roslyn
source generators take over the auto-generated ancillary code that supplies Go semantics at compile
time. The ANTLR4/C# converter is retired.

*Commit: `87465f5f5`*

---

## November 19, 2022 — .NET 7.0, C# 11, and UTF-8 string literals

From the ANTLR4-era converter's News:

* Project has been updated to use .NET 7.0 / C# 11.
* String literals are encoded using UTF-8 (C# `u8` string suffix) which uses the `ReadOnlySpan<byte>`
  ref struct. This should make Go strings faster since strings do not have to be converted to UTF-8
  from UTF-16. Also added an experimental
  [`sstring`](https://github.com/ritchiecarroll/go2cs/blob/master/src/core/golib/sstring.cs),
  a ref struct implementation of a Go string.
* Code conversions now better match original Go code styling.

*Commit: `d90f267d4`*

---

## March 13, 2022 — `v0.1.2` release

**go2cs `v0.1.2` is released** — a tagged release of the mature ANTLR4-era converter. Converted code
now targets **.NET 6.0 / C# 10**, using file-scoped namespaces and reduced indentation to better match
the original Go code's styling, with new command-line options for pre-C#-10-compatible output and ANSI
brace style, options to skip GOOS/GOARCH- and cgo-targeted files, and the ANTLR4 grammar synchronized
to the official source.

*Tag: [`v0.1.2`](https://github.com/ritchiecarroll/go2cs/releases/tag/v0.1.2) (`289b939db`)*

---

## January 5, 2021 — Go as a scripting language for Unity and Godot

Example usages of go2cs allow [Go](https://go.dev/ref/spec) to serve as the **scripting language
for the [Unity](https://unity.com/) and [Godot](https://godotengine.org/) game-engine platforms** —
see the [GoUnity](https://github.com/ritchiecarroll/GoUnity) and
[GodotGo](https://github.com/ritchiecarroll/GodotGo) projects. The project has also been updated to
**.NET 5.0** and supports
[publishing as a self-contained executable](https://docs.microsoft.com/en-us/dotnet/core/deploying/#publish-self-contained).

*Commits: `efb497b3a`, `e5c2d7cbc`*

---

## August 29, 2020 — First full conversion of the Go standard library (ANTLR4 era)

The initial conversion of the **full Go source library** completed without failing — the converter's first end-to-end pass over the entire standard library, committed to `src/go-src-converted`.
The warnings in that conversion's build log laid out the road map of the parsing and conversion work
remaining. Converted code at the time targeted .NET Core 3.1 / C# 8.0, and simple conversions depended
on `src/gocore` — the small, manually-converted subset of the Go library that later became the
curated baseline in `src/core`, since retired in favor of the converted standard library.

*Commit: `8e2d6e8e6`*
