# DRAFT — the go.* 1.26.10.3 release notes

<!-- DRAFT for owner and COORD review, written 2026-11-07 on claude/g-dot3-prep from master 596f75e8ff. Nothing here is
     published. At release the entry below moves to the top of docs/NEWS.md (dated), and the same text, from
     "NuGet 1.26.10.3" down, is the GitHub Release body. Every PENDING item is marked; COORD fills each as its batch lands
     and deletes the item if it misses the release. Every number is read from a committed record named in an HTML
     comment beside it, never from a summary. -->

---

## <DATE>, 2026 — Faster Native AOT, and a Release standard library from source

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
  when the program runs. Rebuild against 1.26.10.3, and move each `nugetgo.*` reference to its 1.26.10.3 revision. The 1.26.10.2 revisions of
  the published nugetgo packages are deprecated on nuget.org, each naming its 1.26.10.3 revision as the alternate.
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
ReadyToRun where it took 520 ms, on `win-x64`, on one 4-core laptop. **PENDING (R): P-384's** field multiply is
rewritten the same way.
<!-- source: docs/phase4/DATA-p521-table.md (R2R table build 520 ms -> 42.6 ms; JIT 93.7 -> 29.1 ms; win-x64, 4-core
     laptop H). Batch 218. P-384: claude/r-p384-field, docs/phase4/DATA-p384-field.md, PENDING landing (after B3);
     fill its measured number from that record. -->

**PENDING (R), may move to the next release.** P-256's field multiply is rewritten the same way. A TLS 1.3
handshake with a P-256 certificate and ECDHE takes about two thirds of its earlier time under ReadyToRun (about
60 ms to about 40 ms), on `win-x64`, on one development machine. The default handshake, with an RSA certificate,
does not change.
<!-- source: R's P-256 field seat, GO'd 2026-11-07 (COORD), building on P-384. R2R on R's laptop, win-x64, scratch-
     measured: the P-256 field Mul about 0.20 of base; the P-256 + ECDHE TLS 1.3 handshake about 0.64 (60 -> 40 ms); the
     default RSA handshake unchanged. It rides 1.26.10.3 only if it lands before the end regen; otherwise delete this
     item and carry it to the next release. Replace the scratch figures with the seat's committed record at landing. -->

**AES-GCM on Apple silicon.** On `osx-arm64`, `crypto/tls` prefers the AES-GCM cipher suites, as Go does there. The
converted standard library is built for `amd64` and runs as a native arm64 process on Apple silicon, so it now
reads the processor's AES and PMULL support from arm64.
<!-- source: src/core/crypto/tls/go2cs_test_disclosures.json note (batch 216, C1); DESIGN-aeshash-arm64.md. -->

**PENDING (the darwin bank). On Apple silicon, `runtime.GOARCH` still reports `amd64`.** The macOS packages are built
once, for `darwin/amd64`, and serve both Macs, so a converted program on an Apple-silicon Mac reports `amd64` where Go
reports `arm64` ([Known issues](KnownIssues.md#on-apple-silicon-runtimegoarch-reports-amd64)). From this release each
Mac's results are compared with Go's own build for that Mac, so the difference now shows in validation. On `osx-arm64`,
the tests of `go/internal/gcimporter`, `debug/buildinfo` and `net/http/pprof` read the architecture and differ from Go's
there, and those three packages are not validated on macOS in this release. Reporting the real architecture on Apple
silicon is planned work.
<!-- source: COORD 2026-11-08 (the docs gap the .3 darwin leg exposed). The per-Mac oracle is batch 208 (C1 (A),
     claude/c1-oracle-arch, -test-oracle-goarch) and the arch-strict banking rule (ledger 2026-11-06 09:06, ruling (1);
     check-roster-format section 2h). The constant is BOARD 2026-11-05 (C1), measured on the GitHub-hosted osx-arm64
     runner (run 41964964302); KnownIssues.md carries the at-master constant citations. PENDING until the darwin bank
     lands; fill in the C1 darwin-leg run ids for the three rows. Whether osx-x64 banks them is not stated here: under
     arch-strict banking a darwin segment needs both RIDs, so check each row's darwin segment at the bank before adding
     a sentence about which platforms validate them. -->

**PENDING (the darwin bank). `internal/synctest` is not validated on macOS in this release.** One of its tests holds
about 10,000 goroutines alive at once, and each converted goroutine is an operating-system thread, past the number of
threads macOS lets one process run. [Limitations](Limitations.md#what-converts-but-doesnt-behave-exactly-like-go)
describes the limit and how to stay under it.
<!-- source: COORD 2026-11-08; C1's run 42497561630 (claude/c1-darwin-threadcap a85e0cf11) on the GitHub-hosted
     macOS runners, both mac RIDs: TestWaitGroupManyBubbles (about 10,100 live goroutines) dies with
     System.OutOfMemoryException from the thread start; kern.num_taskthreads 2048 (osx-arm64, macOS 15.8.2) and 3072
     (osx-x64, macOS 15.7.9). PENDING until the darwin bank lands. -->

**PENDING (the gojq seats and the nugetgo wave). gojq for .NET.** [`itchyny/gojq`](https://github.com/itchyny/gojq),
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
<!-- COORD to trim: the "Also in this release" items from batch 164 on were added by G from the merge log since the
     1.26.10.2 tag (0b1ef0fc77), beyond the brief's list; each is user-visible by its merge subject. Not listed: test
     and release tooling, guards, records, the tour, docs-only batches, and 175 (NuGet-mappings provenance report),
     176 (test host temp root), 182 (bounded goroutine-stack mark), 205 (darwin getfsstat marshal) and 206 (extension
     scan on builtin uint64), each either internal or for COORD's call. -->
