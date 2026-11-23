# DRAFT — the go.* release notes after 1.26.11.1

<!-- DRAFT for owner and COORD review, started 2026-11-20 by P4 (COORD 08:02Z) from master 2d6cf69fd6, on the .3 and .4
     precedent (docs/DRAFT-news-1-26-10-3.md, then docs/DRAFT-news-1-26-10-4.md, which moved into docs/NEWS.md at the
     1.26.11.1 release, 0da8ce11c4). Nothing here is published. The release this draft is for is the next one after
     1.26.11.1 (the .5 work); its version number and date are set at release.
     The two new diagnostic codes (the shape witness's and the deferred-marker refusal's) are not spelled here: the
     registry guard (repoguard TestGo2csDiagnosticCodeRegistry) refuses a code on a user page before the code is
     registered, so each is named at release, read from docs/DiagnosticCodes.md once its seat has landed.
     How to prune: every bullet opens with a status mark. LANDED <sha> is on master; QUEUED <branch> <tip> is a seat
     waiting for a batch, read from that seat's own READY post. At release, drop every bullet still QUEUED, remove the
     marks, and merge what is left into the entry's prose (GoCorpusMigration.md, the release ritual, item 1). The seats
     LANDED on master since the 1.26.11.1 release commit 5dd5cafd5b are all documentation (batches 318 and 319 and the
     owner's "Did AI write this?" page). Each bullet names its source in an HTML comment beside it. -->

---

## <DATE>, 2026 — <TITLE>

**Upgrading from 1.26.11.1.**

- **[QUEUED claude/p1-vp-array-literal c9c62bed56]** **An unnamed array literal now reads `array<nint>([1, 2, 3])`.**
  On every converted platform, a Go array literal of an unnamed array type converts to a call of golib's
  `array<T>(…)` over a C# collection expression, where it was `new nint[]{1, 2, 3}.array()`. C# source written by
  hand outside namespace `go` that has `using static go.builtin` and reaches a static member of golib's array or
  slice type, such as `array<T>.Alias`, now fails to compile with CS0119, because the builtin functions `array<T>(…)`
  and `slice<T>(…)` are found first. Write `go.array<T>.Alias` and `go.slice<T>` instead. See
  [Public surface](PublicSurface.md).
  <!-- P1 Tour item 4, READY 2026-11-19 01:50Z (inbox/COORD/20261119T015056Z-P1.md), claude/p1-vp-array-literal
       c9c62bed56. The emitted form: 09efdde4df (converter + golib) and the regen 49a75818de, measured on windows/amd64,
       linux/amd64, darwin/amd64 and linux/arm64. The CS0119 break: docs/PublicSurface.md (63a8fd6037), accepted by
       COORD 2026-11-14 without the priced fix. Named, keyed, sparse and empty literals, and named array types, keep
       their earlier forms (convCompositeLit.go arrayLiteralTakesBuiltin). -->
- **[QUEUED claude/p1-main-holders a2035f6656]** **Package `main` keeps its methods in the package class.** On every
  converted platform, a method of a type declared in package `main` is now a member of `main_package`, beside the
  package's functions, as the Go file reads; it was a member of a separate holder class, `mainᴛ<Type>`. A type of
  `main` keeps its holder only where one of its methods has the name of something the package reads bare (a
  package-level name, a universe name such as `len`, an import's name, or a member of golib's `builtin` class),
  which the method would otherwise hide. Library packages keep their holders. C# source written by hand against a
  converted program that names `mainᴛ<Type>` names `main_package` instead. A converted file whose last method sits
  in a holder now closes it with a bare `}`.
  <!-- P1 main-holders, owner ruling 2026-11-18 (FLEET 11-19), READY at a2035f6656 2026-11-19 21:12Z
       (inbox/COORD/20261119T211252Z-P1.md; P3's read 20261119T212356Z-P3.md). The rule and the clash list:
       docs/ConversionStrategies.md, "Package main keeps its methods in the package class" (383eeb05c3, b9195efa78);
       the converter 4d22819305 and 3a5109de87; a type a test file adds a method to keeps its holder, a2035f6656. -->
- **[LANDED afb01dfb6c]** **A Go method on a named slice or array type takes the
  generated wrapper member of the same name.** A Go method named `Slice`, `Append`, `ToSpan`, `GetEnumerator`,
  `Length`, `Source`, `Capacity`, `Low`, `High`, `Available`, `IsNil`, `Make` or `Wrap` on a named slice or array type
  (or on a type defined over a named array) now runs when Go code calls it; before, the wrapper's own member of that
  name could run instead, silently when the call fit its parameters. The wrapper member stays reachable through its
  golib interface, e.g. `((ISlice)s).Capacity`; an array's `Slice`, which no interface declares, becomes `ΔSlice`. A
  type named like one of those members now converts as well. C# source written by hand that called such a member on
  such a type calls it through the interface instead.
  <!-- R7 seat 1 (COORD 212140Z, the name-collision campaign; P3's read inbox/COORD/20261122T061749Z-P3.md, its wording).
       The rule: ISliceTypeTemplate / IArrayTypeTemplate / IArrayViewTypeTemplate ClaimableMembers, bf8153dbb8; the
       guard GenTests NamedSliceArrayClaimedMemberTests; behavioral NamedSliceMethodShadow b5c2b6b7a2. Census by the
       generator over src/core's 4126 generated slice/array/struct wrappers: the only claiming types are hash/fnv's
       sum128 and sum128a (Clone, claimed before this seat). sqlite-cs's modules and typescript-go, by the syntactic
       census: no slice or array type claims a listed name. -->
- **[QUEUED claude/r-r7-chan-map-num (tip in its READY post)]** **The same holds for named channel, map, numeric,
  string and pointer types.** A Go method named `Capacity`, `Length`, `IsClosed`, `IsUnbuffered`, `Receive`,
  `Received`, `Receiving`, `Sending`, `Make` or `GetEnumerator` on a named channel; `Length`, `IsNil`, `Count`, `Make`
  or `GetEnumerator` on a named map; `CompareTo` on a named number; or `Length`, `CompareTo` or `GetEnumerator` on a
  named string now runs when Go code calls it, and the type still ranges, selects, sorts and measures as Go's does. A
  type named like one of its own wrapper's members converts as well, a named pointer type included: `type Value
  *[3]int`, `type of *point`. The member stays reachable through its golib interface where one declares it, e.g.
  `((IChannel)ch).Capacity`, and becomes `Δ<name>` otherwise (`ΔIsClosed`); a pointer type named `Value` reads its
  pointee through `Value‿`. With `-uco=false`, a channel whose type declares one of those methods is sent to and
  received from with the default mode's operators. Slicing a named pointer to an unnamed array (`type P *[3]int`,
  `p[1:]`) also converts now. C# source written by hand that called such a member on such a type calls it through
  the interface instead.
  <!-- R7 seat 2: GenTests NamedChanMapNumericClaimedMemberTests (red b6733d8a1a); behavioral NamedChanMapNumericShadow;
       go2cs TestNamedChanMethodModeSpelling; the fix 62f1816922. Census over std at go1.26.11 through go/types, four
       targets (windows, linux, darwin amd64; linux arm64), 966-968 packages each: the only claiming types are net/http
       Header, net/textproto MIMEHeader and net/url Values (Add, Set: claimed before this seat), no named pointer or
       channel carries a listed name, and no site slices a named pointer to an array. -->


**What changed.**

- **[QUEUED claude/r-shape-witness 5a4655646b]** **A generic under a method-set constraint that is not
  self-referential now converts within its own package.** On every converted platform, a generic function or type
  whose constraint lists methods, and whose signature puts the type parameter where go2cs's constraint proxy cannot
  carry it (a generic named type, or the type parameter in a result, a map, a channel or a pointer), takes a shape
  witness: the type parameter stays the Go type, and its method calls go through a witness type that
  `package_info.cs` declares. Before, go2cs refused these with `GO2CS2005`. A site the witness cannot place yet, such
  as a constraint or type argument from another package, is refused with a new error code that names the site. See
  [Diagnostic codes](DiagnosticCodes.md).
  <!-- R shape-witness stage 1, READY 2026-11-19 11:53Z at 202d0ee3ad (inbox/COORD/20261119T115335Z-R.md), P3's
       review fixes at 529de5fc92, tip 5a4655646b (20261119T201550Z-R.md, run 8 20261119T224840Z-R.md). Stage 1 is
       same-package constraints only; %T and stack traces naming a carrier are stage 3. Design:
       docs/phase4/DESIGN-constraint-proxy-nonself.md section 12. Footprint in the standard library: one package, on
       all four targets. -->
- **[QUEUED claude/c2-recurse-importer-seed 18960ca538]** **Converting a module: two packages that spell one
  anonymous struct now agree on its layout by themselves.** When code such as ccgo's output declares the same
  anonymous struct in two packages (modernc.org/sqlite's `TLARGE_INTEGER` and modernc.org/libc's `largeInteger`),
  a `go2cs -recurse` run now seeds the declaring import so that both packages hold the struct's arrays the same way,
  and a value of it crosses between them by value. Before, this took `-inline-arrays=<import path>#<TypeName>` by
  hand, which still works. This was measured on windows/amd64, linux/amd64, linux/arm64 and darwin/amd64. See
  [`-inline-arrays`](CommandLine.md).
  <!-- C2 seat (3)+(A), the -recurse importer seed (W18 ruling C item 3, scope b), merged with master at 18960ca538
       (inbox/COORD/20261120T065353Z-C2.md); the dotnet legs were run by P2 (20261120T065907Z-P2.md onward). The rule:
       docs/ConversionStrategies-Reference/slices-and-arrays.md, "A struct read from native memory holds its arrays
       inline, and seeds make two packages agree" (f8ad290f77, ae7eaf1ee4). Check C2's final READY post before release:
       READY waited for the CNR leg when this draft was started. -->
- **[QUEUED claude/c1-test-refs-ext a750c69c8c]** **A converted test project references what its extension-method
  calls need.** A test that calls a method which C# resolves among same-named extension methods needs every
  assembly those candidates' signatures name; a `-tests` project now references them, where a module's converted
  tests could fail to compile for a missing reference. modernc.org/sqlite's tests are the case: they now reach
  `math/big` and `database/sql/driver`. Standard-library test projects gain at most a reference or two (measured on
  windows/amd64, linux/amd64 and darwin/amd64).
  <!-- C1 CLASS A, re-READY 2026-11-20 07:37Z (inbox/COORD/20261120T073727Z-C1.md), claude/c1-test-refs-ext
       a750c69c8c; the census and the ruling: 20261117T145045Z-C1.md onward. The footprint: windows adds
       internal/reflectlite+iter and internal/syscall/windows/registry+io/fs, linux and darwin internal/reflectlite+iter
       (docs/phase4/data/test-refs-ext/footprint-1120/). The sqlite reading is linux/amd64 from the module probe;
       the full sqlite conversion with this seat is P5's rerun, not measured by C1. -->
- **[QUEUED claude/c1-oracle-tags 2a7225ce67]** **Test comparison: the Go side builds a module that reaches
  `math/big` through a linkname.** The `go test` run that a converted test suite is compared against now drops the
  `math_big_pure_go` tag everywhere except on `math/big`'s own row, so a module that pulls `math/big` by linkname
  links. A module's proof page now records the build tags of that Go run. Measured on windows/amd64, linux/amd64 and
  darwin/amd64: `math/big` is the only standard-library row whose files change.
  <!-- C1 CLASS D, merged with master at 2a7225ce67 (inbox/COORD/20261120T072952Z-C1.md, 20261120T073802Z-C1.md);
       the fix 366fca939d, the tags on proof pages 8d9104967f, the go list reading ae9a2a8e55
       (docs/phase4/data/oracle-tags-1120/). -->
- **[QUEUED claude/p2-rwmutex-go-layout 7df4159494]** **`sync.RWMutex` is Go's own code.** On every converted
  platform, `sync.RWMutex` is now the converted `rwmutex.go`, Go's five fields over the semaphore golib already
  provides, where it was a hand-written class around a managed monitor. A copy of a `RWMutex` is a separate lock, as
  in Go, and `reflect` sees its fields. golib's semaphore now drops the state of a word nothing is waiting on, so a
  lock that once blocked no longer keeps its owner alive, and a waiter that asks to queue first (Go's `lifo`) is
  queued first.
  <!-- P2 RWMutex, re-READY 2026-11-19 19:07Z at 7df4159494 (inbox/COORD/20261119T190738Z-P2.md; P3's read
       20261119T195639Z-P3.md). The record: docs/ConversionStrategies-Reference/manual-conversions.md, "sync.RWMutex
       is Go's own code, and an empty semaphore bucket is retired" (dd2a8479cb); lifo 7df4159494. Rows re-read on
       linux: sync, internal/sync and runtime. golib's API only gained an overload (ApiCompat 0). -->
- **[QUEUED claude/p2-set-default-gomaxprocs d017656199, claude/p2-gomaxprocs-update 55efd75b6e]** **`GOMAXPROCS`
  follows Go's rule.** `runtime.NumCPU` is the number of CPUs the operating system lets the process run on, and the
  starting `GOMAXPROCS` is the `GOMAXPROCS` environment variable or Go's default for that count, on every converted
  platform; neither reads .NET's `DOTNET_PROCESSOR_COUNT`. `runtime.SetDefaultGOMAXPROCS` now works. On linux, the
  default is lowered to the cgroup's CPU limit, as in Go. While the setting is not custom, it follows a change of the
  process's CPU affinity or CPU limit at run time, as Go 1.25 and later do. A converted test suite's `-parallel`
  defaults to `GOMAXPROCS`.
  <!-- P2 GOMAXPROCS seat, re-gated at d017656199 (inbox/COORD/20261119T090538Z-P2.md and the READYs after it), and
       W-gmp-e, the automatic update, at 55efd75b6e (20261119T140946Z-P2.md, 20261119T152831Z-P2.md). The record:
       docs/ConversionStrategies-Reference/manual-conversions.md, the GOMAXPROCS and SetDefaultGOMAXPROCS rows
       (535af1736e, 68e89965ad, ca516221a1). Measured on linux/amd64; the affinity arm runs in GolibTests. Known cost,
       recorded and not emulated: the update tick runs while the program is idle, where Go parks it. -->
- **[QUEUED claude/p5-dot5-godebug-adapter cbc0aa3cc8]** **The converter stops where it would have left a marker in
  its output.** Some names are known only after a package's records are final, so the converter writes a marker and
  rewrites it afterwards. When that pass cannot finish a file, go2cs now stops with a new error code that
  names the file, where it warned and left a marker that does not compile. A package written entirely by hand now
  has the markers in its review copies (`.cs.auto`) rewritten too. This applies on every converted platform. See
  [Diagnostic codes](DiagnosticCodes.md).
  <!-- P5 godebug adapter seat, READY 2026-11-19 12:37Z at 95d7d7b04f (inbox/COORD/20261119T123751Z-P5.md),
       renumbered at cbc0aa3cc8 (20261119T185441Z-P5.md). The one corpus file it moves:
       internal/godebug/godebug.cs.auto, the same line on all four targets. -->
- **[QUEUED claude/i9-lever-e 43982146a0]** **A Native AOT build of converted code is smaller.** golib's pointer
  boxes keep their per-type tables in one shared holder, where each instantiation had its own. A Native AOT publish
  of the converted TypeScript parser demo on win-x64 has about 29% fewer symbol names and a 13% smaller image, and
  the AOT compiler's peak memory is about 25% lower; its output is unchanged.
  <!-- i9 lever E, READY 2026-11-18 04:54Z at 43982146a0 (inbox/COORD/20261118T045402Z-i9.md). The readings: the
       BOARD row "lever E seat" in 43982146a0 (parsedemo at f36b6b0d48, win-x64: names -29.18%, image -12.64%, ILC
       peak -24.56%). JIT timing within noise; process allocation about 53 KB lower per run. -->
- **[QUEUED claude/p4-blank-import-note 7de4e2e267]** **A blank import's comment says what happens to the package's
  `init`.** On every converted platform, Go's `import _ "image/png"` converts to the comment
  `// import _ "image/png" (init forced in ImportInitializers)`. The other two endings are `nothing to initialize`
  and, for a test file's blank import of the package under test, `compiled into this assembly; no hook`.
  <!-- P4 blank-import note, READY 2026-11-19 08:13Z at 7de4e2e267 (inbox/COORD/20261119T081355Z-P4.md); landing order
       after Tour item 4 and P1's main-holders (COORD). The text: docs/ConversionStrategies.md, the blank-import
       paragraph, and ConversionStrategies-Reference/package-conversion.md. -->

**Known issues.**

- **[QUEUED i9, not cut yet]** **A `-recurse=nuget` conversion no longer reads an older local go2cs root.** The
  known issue added after 1.26.11.1, where such a conversion read `~/go2cs` (or `GO2CSPATH`, or `-go2cspath`) ahead
  of the metadata embedded in go2cs and emitted names the published packages lack, is fixed, and its entry leaves
  [Known issues](KnownIssues.md).
  <!-- The entry: batch 319 (2d6cf69fd6, P4 a0d69468d4), measured on windows-amd64 and linux-x64. The fix is i9's .5
       seat, not cut when this draft was started (COORD 2026-11-20 08:02Z): replace this mark with its branch and tip
       when it is READY, and check the wording against its READY post. -->

**Documentation.**

- **[LANDED a98c5175be, 42541a338c]** A page answers [Did AI write this?](AIAssistance.md), with an FAQ entry and
  a link from the README and the milestones table.
  <!-- The owner's text with COORD's review pass (a98c5175be); the links 42541a338c. -->
- **[LANDED 1153738889]** [Known issues](KnownIssues.md) reads the Native AOT cost again on the published packages
  (linux-x64), and says what to do when a `nugetgo` package built on 1.26.10.3 meets the 1.26.11.1 packages at
  restore (NU1107). [The go2cs tool](DotnetTool.md) says that it downloads Go on first use. The FAQ says which
  parts of cgo convert.
  <!-- Batch 318, P4 2df5299bfa (COORD 2026-11-20 07:19Z). Its NEWS change edits the 1.26.11.1 entry itself and needs no
       bullet here. -->
- **[LANDED 2d6cf69fd6]** [Known issues](KnownIssues.md) names the stale local go2cs root that a `-recurse=nuget`
  conversion reads (see the known issue above; drop both bullets together if the fix ships).
  <!-- Batch 319, P4 a0d69468d4. -->
