# 📰 go2cs News Archive

![go2cs](images/go2cs-small.png)

All project announcements, newest first. The [README](README.md) summarizes where the project stands
today; every announcement is recorded here, and the detail-heavy ones link to a companion page carrying
their full text.

---

## November 19, 2026 — Go 1.26.11, cgo conversion, linux-arm64 packages, and TypeScript's compiler and SQLite in C#

<!-- Written ahead and HELD for release day (the .2 and .3 precedent, e665f4ea51 and 2ed9f54759): "November XX" becomes
     the publish date as the last change before the release tag (GoCorpusMigration.md, the release ritual, item 1).
     Moved from docs/DRAFT-news-1-26-10-4.md by the 1.26.11.1 release-controls batch (claude/p4-release-controls-12611,
     COORD FLEET 2026-11-19 09:04Z): the draft's per-seat sections merged into one entry (their headings dropped, each
     body kept with its source comment); the section held since 2026-11-15 supplies the lead and *Upgrading from
     1.26.10.3*, its PENDING markers removed because every item it names landed before the .4 freeze (batches 305-312)
     and the release that was PENDING is this one; the standalone assembly-naming section folds into its Upgrading
     bullet; the Go 1.26.11 and break-floor sections follow the lead and the Upgrading list; batch 315 gets one
     paragraph. The draft's own header said: started 2026-11-11 by seat C43 (P2) from master 50dd9a065c, on the .3
     precedent; the release it was started for as 1.26.10.4 ships as 1.26.11.1, built from Go 1.26.11 (owner, relayed by
     COORD 2026-11-12). PENDING on the publish, not on any merge: the first Upgrading bullet's "move each `nugetgo.*`
     reference to its 1.26.11.1 revision" holds once the nugetgo wave (PLAN-rebank-1-26-11-1, "Release day") has
     published; reword it if a published module misses the wave. -->

**NuGet 1.26.11.1.** The fourth release on the Go 1.26 line converts the standard library from Go 1.26.11, a patch
release with security fixes; `go2cs -cgo` now converts a package that calls C; the go.* packages ship a linux-arm64
build; Microsoft's TypeScript compiler, written in Go, converts, compiles and type-checks; and a modernc.org/sqlite
program that writes and reads a database file prints exactly what native Go prints, on Windows and on Linux.
<!-- The headline sentence: the owner's title choice, 2026-11-19 ("I like #1", relayed by COORD 14:51Z). Each item is
     its section below: "The standard library is converted from Go 1.26.11"; "`go2cs -cgo` now converts a cgo
     package"; "The go.* packages now ship a linux-arm64 build"; "The Go implementation of TypeScript's compiler now
     converts, compiles and type-checks"; "Code that ccgo translated from C converts further" (the file demo, Windows
     x64 and Linux x64). -->
> For any code that happened to be compiled against 1.26.10.3 (minimal exposure expected for this at the moment),
> some of the changes below may break compiled assemblies, e.g., C# source that reaches a Go method through the
> package class or through `using static` alone, or that writes a name with a `Δ` prefix. They are listed under
> *Upgrading from 1.26.10.3*, and a build that would hit one of the binary breaks now stops with an error that names
> the package (`GO2CS3002`).
<!-- The owner's callout, as the published GitHub Release (nuget-1.26.11.1) carries it, word for word: it replaced the
     paragraph this entry was released with ("Changes below break assemblies already compiled against 1.26.10.3, ...").
     First post-.4 docs batch, COORD 2026-11-20 07:19Z. -->

**The standard library is converted from Go 1.26.11, a Go patch release with security fixes.** The converted
packages that change are `net/http` (its server and transfer code), `net` (its dialer), `crypto/rsa` and the FIPS
140 module, on every converted platform. Go 1.26.11's other fixes are in cgo, the go command, the compiler and the
linker, which have no converted counterpart. The go2cs tool, which downloads its own Go on first use, now downloads
Go 1.26.11. Every validated test package whose converted code moved was run again at Go 1.26.11 and validated
there. Each standard-library package's README now leads its badges with a "converted by go2cs" badge.
<!-- merge ffaafa144f (R, claude/r-hop-go12611), the 1.26.10 -> 1.26.11 patch hop under docs/GoCorpusMigration.md.
     The changed packages: the H5 regen 8a98787909 (predicted set MET: crypto/internal/fips140/*, crypto/rsa,
     internal/platform, net/{windows,linux,darwin}/dial.cs, net/http/server.cs and transfer.cs, on all four targets).
     The download: H2 b1e0cfb1b4, src/go2cs/toolchainPins.go (pinnedGoVersion "go1.26.11"). Re-banking: H10, the
     merge message (net/http, fips140test and net re-read at go1.26.11). The badge: 3ddde452d1. The security-fix list
     (net/http, cgo, the go command, compiler, linker) is the merge message's; no record in the tree itemizes it. -->

**Upgrading from 1.26.10.3.**

- **Rebuild what you built against 1.26.10.3.** A Go struct that embeds another struct of its own package
  gets a forwarder for each method the embedding promotes. Through 1.26.10.3 those forwarders were members of the
  package class (`bufio_package` for `bufio`); from 1.26.11.1 each struct's forwarders are in a class of the
  struct's own, beside the package class in the same namespace. In the standard library 900 public forwarders moved,
  counted once for each assembly a package ships, in 20 packages; a program can import 13 of the 20, and the other
  7 are internal or vendored. An assembly or nugetgo package compiled against go.* 1.26.10.3 that calls such a
  promoted method fails at that call under 1.26.11.1 with `MissingMethodException`. The build refuses the
  combination first: a package built below the new break floor is error `GO2CS3002`, which names the package
  ([Diagnostic codes](DiagnosticCodes.md)). Rebuild against 1.26.11.1, and move each `nugetgo.*` reference to its
  1.26.11.1 revision. A `nugetgo.*` revision built on 1.26.10.3 left beside go.* 1.26.11.1 fails earlier, at
  restore, with `NU1107`, before `GO2CS3002` can name it
  ([Known issues](KnownIssues.md#a-nugetgo-package-built-on-126103-beside-the-go-126111-packages-fails-at-restore)).
  <!-- source: the promoted-forwarder seat (docs/phase4/DESIGN-promoted-forwarder-placement.md; owner ruling
       2026-11-10: "better breaking changes now than before we're deep with users"). The moved members are announced
       to the binary-compatibility guard as generated CP0002 entries
       (src/tools/pushnuget/New-GoApiCompatMoveSuppressions.ps1; 900 entries in 20 files when this seat landed,
       generated since together with the method-holder moves and the U+0394 renames below), and <GoBreakFloor> is
       1.26.11.1. The nugetgo rebuild wave ships with this release (the owner's hands). -->
- **Standard-library assemblies are named after their packages.** Package `go.fmt` now ships `go.fmt.dll`,
  not `fmt.dll`, and likewise every converted standard-library assembly (`go.net.dll`, `go.crypto.tls.dll`,
  `go.go.ast.dll` for package `go.go.ast`, `go.testing.dll`). The package IDs do not change, nor do `golib.dll`
  (package go.lib), the go2cs-gen analyzer, or a converted module's own assemblies. A library converted and built
  against go.* 1.26.10.3 or earlier binds the standard library's assemblies by their OLD names, so it must be rebuilt
  against 1.26.11.1; the same break floor names it at build time (`GO2CS3002`) rather than letting it fail as a
  missing assembly at run time. A C# project that referenced a converted assembly by file name (`fmt.dll`) or by an
  assembly-qualified type name (`go.fmt_package, fmt`) uses the new name.
  <!-- source: OQ4, docs/PLAN-public-surface-stability.md (ruled), the design docs/phase4/DESIGN-assembly-identity.md;
       src/go2cs/assemblyIdentity.go. The binary-compatibility guard compares each renamed assembly with its 1.26.10.3
       baseline through a rename map keyed to this release (src/tools/pushnuget/GoApiCompat.psm1).
       The standalone draft section (R, merged here at the release cut): OQ4 (docs/PLAN-public-surface-stability.md, ruled), branch claude/r-oq4-assembly-rename:
       src/go2cs/assemblyIdentity.go; the design docs/phase4/DESIGN-assembly-identity.md and its census. -->
- **`using static` alone no longer brings methods.** C# that calls into a converted package with only
  `using static go.bufio_package;` still sees the package's types, functions, constants and variables, but no method:
  neither the methods its types declare nor a method a struct promotes from an embedded struct of the same package.
  Add the namespace as well: `using go;` (or `using go.<path>;` for a package below the root). Converted code already
  has it. [Consuming Go from C#](ConsumingGoFromCSharp.md) has the details.
  <!-- source: the same seat; GenTests PromotedHostClassTests.UsingStaticAloneBringsDeclaredMethodsAndNoPromotedOne
       (CS1929 without the namespace) and TheNamespaceBringsThePromotedMethods. -->
- **Every Go method is in its receiver type's method holder.** Through 1.26.10.3 the methods a package
  declares were members of the package class (`time_package`); from 1.26.11.1 the methods of each type are in a class
  of their own beside the package class, in the same namespace (`timeᴛTime` holds `time.Time`'s methods). A method is
  still an extension method on its receiver, so `t.Add(d)` reads as before once the namespace is imported (`using go;`,
  or `using go.<path>;` for a package below the root); `using static go.time_package;` alone no longer brings any
  method. A call written in static form through the package class, `time_package.Add(t, d)`, is now
  `t.Add(d)`. In the standard library more than eight thousand public methods moved, counted once for each assembly
  a package ships, in over two hundred packages, about 120 of which a program can import. An assembly compiled
  against 1.26.10.3 that calls a Go method fails under 1.26.11.1 with `MissingMethodException`, and the break floor
  refuses that combination first (`GO2CS3002`): rebuild against 1.26.11.1.
  <!-- source: the method-holder seat (OQ2, P1; docs/phase4/SCOUT-method-holder.md on
       claude/p1-oq2-method-holder-scout). The moved members are announced to the binary-compatibility guard by
       New-GoApiCompatMoveSuppressions.ps1 over the OQ2 cut's release-smoke pack (2026-11-15): 8,273 CP0002 move
       entries across all assets, promoted forwarders included, in 230 suppression files that also carry the renames
       below (13,234 entries in all). The package counts are read from those files: 213 carry a CP0002 entry, 120 of
       them reachable (no internal segment, not vendored). -->
- **Names that carried a `Δ` prefix keep their Go names.** Through 1.26.10.3 a type, constant or variable whose
  Go name a method of its package also used took a `Δ` prefix in C#, because both were members of the package class:
  `time.Second` was `time.ΔSecond`, `reflect.Type` was `reflect.ΔType`, `syscall.Handle` was `syscall.ΔHandle`, and
  `go/doc/comment`'s interface `Text` was `ΔText`, because `Printer` has a method `Text`. With each method in its
  receiver's holder the two names no longer meet, and from 1.26.11.1 every such name is its Go name. In the standard
  library this renames over two hundred types and dozens of constants and variables, and changes well over a thousand
  members that name them (fields, parameters, return types, interface members), in about a hundred packages, two
  thirds of which a program can import. C# source that names the prefixed form uses the Go name instead (`ΔText`
  becomes `Text`). An assembly compiled against 1.26.10.3 that binds a prefixed name fails under 1.26.11.1, and the
  break floor refuses that combination first (`GO2CS3002`): rebuild against 1.26.11.1.
  <!-- source: the method-holder seat (OQ2, P1); the converter's ShadowVarMarker no longer applies once methods leave
       the package class. Announced to the binary-compatibility guard as generated rename entries
       (New-GoApiCompatMoveSuppressions.ps1, the U+0394 drop, COORD ruling 2026-11-15 10:14Z: a removed member is a
       rename only when its documentation id with every U+0394 dropped is in the current surface exactly, or a move
       once its receiver and signature drop theirs): 4,961 entries across all assets, CP0001 369, CP0002 4,515, CP0006
       59, CP0008 16, CP0021 2. On the lib asset alone: 241 types (CP0001), 1,713 members whose id names a prefixed
       form, in 102 packages, 67 of them reachable.
       Merged at the release cut (COORD ruling 2026-11-19 09:32Z) with the seat's second bullet on the same mechanism,
       "A type, constant or variable named like a method keeps its Go name", whose source note was: the same seat;
       behavioral fixtures MethodHolderNames and MethodHolderAlias, and every golden that named a renamed element
       (goldens: re-baseline). -->
- **A struct whose only array has length zero no longer has `ΔClone`.** Through 1.26.10.3 a struct with a
  fixed-size array field implemented `go.IGoValueClone` and had a generated `ΔClone()`, because a C# copy of the
  struct shares the array where Go copies it, and every Go value copy called the clone. A zero-length array holds
  nothing to copy, so from 1.26.11.1 a struct whose only such field is zero-length has neither, and its value copy is
  a plain C# copy. In the standard library this is one type, `syscall.InotifyEvent` on Linux, which ends
  `Name [0]uint8`. C# source that called its `ΔClone()` copies the value instead. An assembly compiled against
  1.26.10.3 that calls it, or uses the struct as `IGoValueClone`, fails under 1.26.11.1, and the break floor refuses
  that combination first (`GO2CS3002`): rebuild against 1.26.11.1.
  <!-- source: P2's zero-length-array seat (54aa3a5b85, batch 312). Announced to the binary-compatibility guard as two
       generated entries (New-GoApiCompatMoveSuppressions.ps1, the clone drop: CP0008 for the struct, CP0002 for
       ΔClone) on runtimes/linux-x64 against 1.26.10.3. The generator over the 312 union's release-smoke pack (362
       packages) found no other: 13,236 entries in 230 files, 0 refused. linux-arm64 has no 1.26.10.3 asset to compare
       with; darwin and windows have no InotifyEvent. -->

**The build now checks every go2cs-built package it uses against the break floor.** The break floor is the newest
go.* release that changed members that compiled code binds; at this release it is 1.26.11.1. The check runs before
the C# compiler. A `go.*` package counts as its own version. A `nugetgo.*` package counts as the release its
`go2cs/source-metadata.txt` names, and as below the floor when it ships no such file. Each package below the floor is
error `GO2CS3002`, naming the package and the release it was built on, and one more error stops the build before it
compiles anything, where such a package used to restore cleanly and then fail at run time with
`MissingMethodException`. To build anyway and accept that risk, set
`<GoAllowBelowBreakFloor>true</GoAllowBelowBreakFloor>`: each error becomes a warning. The check arrives with go.lib
1.26.11.1, so a project whose go.lib is 1.26.10.3 or older is not checked. A module packed some other way, under an ID
that does not start with `nugetgo.`, is not checked either. See
[The break floor](ConsumingGoFromCSharp.md#the-break-floor).
<!-- merge f7e140e982 (P2, claude/p2-break-floor, owner-ruled for .4): GoCheckBreakFloor in
     src/core/golib/buildTransitive/go.lib.targets (BeforeTargets=CoreCompile, skipped for design-time builds);
     the GoBreakFloor property set to 1.26.11.1 in the same file (moved from 1.26.10.3 by the promoted-forwarder seat
     and re-keyed by the 1.26.11 hop, 5f7b282396). The adversarial review and COORD's ruling:
     docs/phase4/REVIEW-go-break-floor.md (d81da0c9e2). Wording of the remedy and the unchecked cases:
     docs/ConsumingGoFromCSharp.md "The break floor" and docs/DiagnosticCodes.md GO2CS3002. If the owner prefers, this
     section can instead be a short paragraph ahead of NEWS's "Upgrading from 1.26.10.3" bullets, which already cite
     GO2CS3002. -->

**A package whose types promote very many methods now loads.** The .NET runtime loads no type with more
than 65,521 methods. A converted package class held every promoted-method forwarder of every struct in the package,
so a package where many structs embed one method-rich base could pass that limit: it compiled, and its first use
threw `TypeLoadException`. With each struct's forwarders in a class of their own, the package class holds only what
the package declares. A type that would still exceed the limit is now a compile error, `GO2CS0012`, which names the
type and its method count.
<!-- source: the same seat. Measured shape: N methods on one base embedded by M structs put 2NM + 2N + 5 methods in
     the package class; N=250, M=131 (66,005) failed to load and M=130 (65,505) loaded. One converted package class
     held 116,102 methods, 107,835 of them forwarders. -->

**A promoted method that returns a string, a bool or several values can be called from another package.**
A Go struct that embeds an exported struct promotes its methods, and code in another package may call them on the
outer struct. The C# forwarder for such a method was `internal` whenever its return type's C# spelling ended in a
lowercase name (`@string`, `bool`, `error`, any multi-value return), so a call that Go accepts did not compile in
the converted program (`CS1929`). The forwarder of an exported method is now public unless its signature names a
type that is not public. In the standard library 268 forwarders in 12 packages became public, `bufio.ReadWriter`'s
`Read` and `Peek` among them; none was removed, so nothing compiled against 1.26.10.3 breaks on this change.
<!-- source: C34 of the TypeScript-Go readiness run (repro c34promotedbuiltinreturn at 4505b95021 on
     claude/i7-ts-readiness-rerun), folded into the promoted-forwarder seat. GenTests PromotedForwarderScopeTests;
     behavioral PromotedValueEmbedLib / PromotedValueEmbedUser. The count is of forwarder methods in one target's
     packed assemblies, the same on windows, linux and darwin (docs/phase4/DESIGN-promoted-forwarder-placement.md,
     amendment section H). -->

**A method whose receiver type is written through an alias runs on a nil pointer, as in Go.** Go accepts
`type AttrNode = Node` with `func (n *AttrNode) Mode() int`, and the method is `Node`'s: it can be called on a nil
pointer and test `n == nil` itself. The converter took such a method's receiver by reference, so the nil test read
the struct through the nil pointer and the call panicked where Go returns; a method that used the receiver as a
pointer in any other way did not compile. Such a method is now emitted as it is when the receiver is written with
the type's own name. The standard library has no such method, so its converted code is unchanged. A receiver that
is an alias of the pointer type itself (`type P = *T`) still does not compile after conversion.
<!-- source: C48 of the TypeScript-Go readiness run (one type-check unit panicked in
     ast.(*Node).GetResolutionModeOverride), P1; docs/phase4/DESIGN-alias-receiver-form.md. Behavioral
     AliasReceiverForms; converter tests TestAliasReceiverTakesTheNamedForm and
     TestAliasReceiverEmitsAsNamedReceiver. -->

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

**Some code instead of none: `go2cs -warn`.** A package that holds a site the converter refuses used to produce no
code at all. `-warn <code>` now downgrades the listed refusal to a warning, so the package converts and everything but
the refused site runs. It is opt-in per code and never a default, and a code is allowed only when every site it covers
fails loud when reached: the converted program panics there with the refusal text, rather than running on with a
silently wrong result. Today that is `GO2CS2002`, a generic instantiation whose self-referential constraint has a
method that cannot be forwarded (for example `Tree.Children` returning `[]N`). Each downgraded site is printed as
`warning GO2CS2002`, and every C# build of the project reports it again as the new `GO2CS0011`, so a project that is
never converted again still shows that it carries a known-broken path. See [`-warn`](CommandLine.md) and
[diagnostic codes](DiagnosticCodes.md).
<!-- the -warn seat (owner's .4, COORD 2026-11-10/11), branch claude/r-warn-option: src/go2cs/warnOption.go (the
     allow-list), src/core/golib/GoRefusalDowngradedAttribute.cs, src/gen/go2cs-gen/RefusalDowngradedAnalyzer.cs,
     behavioral RefusalDowngradeProxy. -->

**`-recurse` converts the rest of a module when one package is refused.** One package the converter refused (a
`GO2CS2001`, `GO2CS2002`, `GO2CS2003`, `GO2CS2005` or `GO2CS2006` error) used to end a `-recurse` run, so every package queued after it
went unconverted. Now the refused package is skipped: its diagnostics still print as errors and it gets no project,
but every other package converts. The run still ends with exit 1, naming each refused package and each converted
package that imports one, since those will not build until the refused package converts. A package that fails to
convert (it does not load, or the converter panics) was already skipped, but the run used to exit 0; it now exits 1
too, so a script that checks the exit code sees it. A single-package conversion still stops at a refusal and writes
nothing. `-tests -recurse` goes on past a refusal too, as `go test ./...` goes on past a package that does not build:
each tested package that the refused package does not reach is converted and tested, and each one it does reach is
named, with its reason, in the run's output (and in MODULE.md, on a run that writes one). See
[`-recurse`](CommandLine.md).
<!-- COORD rulings 20261111T053529Z and 20261111T060642Z, branch claude/r-warn-option: src/go2cs/recurseRefusal.go,
     moduleConverter.go convertAll / ConvertModule; the caller census in R's post 20261111T062125Z. The -tests -recurse
     half: COORD ruling 20261117T063529Z, branch claude/c1-tests-recurse-skip, moduleTestsDriver.go runModuleTests. -->

**A refusal is renumbered: `GO2CS2005`.** The refusals for a pointer type argument to a generic with a
non-self-referential method-set constraint (new in 1.26.11.1, alongside the conversions that now work for that shape)
are reported as `GO2CS2005` instead of `GO2CS2002`. Their sites cannot fail loud if downgraded, so `-warn` does not
accept `GO2CS2005`. A build script that matched `GO2CS2002` for those sites should match `GO2CS2005`.
<!-- branch claude/r-warn-option: src/go2cs/diagnosticOutput.go diagnosticNonSelfProxyRefused; docs/DiagnosticCodes.md. -->

**A new refusal: `GO2CS2006`.** A generic call that passes a `[]*T` where the callee declares `[]N` (`N` constrained
by a method-set interface) is converted by copying the slice element by element, so a callee that writes the slice
(assigns an element, appends into it, sorts it, or hands it to something that does) wrote the copy, and the caller
never saw the change, with no diagnostic. go2cs now refuses such a call by name, naming the callee's first write, and
converts it as before when the callee only reads the slice. No standard-library package is refused. `-warn` does not
accept `GO2CS2006`, since a downgraded site would run on silently wrong. See [diagnostic codes](DiagnosticCodes.md).
<!-- branch claude/c2-widening-write-refusal: src/go2cs/sliceWideningWriteRefusals.go; docs/DiagnosticCodes.md;
     the design record on claude/c2-widening-write-design (std census: 8 sites per target, 0 refused). -->

<!-- 2026-11-14 (H): the section "golib: `GoCgoLibraries.Register` gains a target (H)" is DROPPED, as R's note below
     suggests. GoCgoLibraries is new in this release (816009e491, batch 284; golib at nuget-1.26.10.3 has no such
     class), so an overload added to it changes nothing anyone compiled against, and the converter emits every call
     to it. The class is EditorBrowsable(Never). Its text was: "The method gains an overload that takes the target
     its bindings were converted for, Register(assembly, "linux/amd64", ...), and the original Register(assembly,
     ...) stays, so compiled code is unaffected. One call no longer compiles: Register(assembly, null) is ambiguous
     between the two overloads (CS0121). Converted code never writes it; the converter emits every call."
     Its provenance: docs/phase4/DESIGN-cgo-phase2-pinvoke.md §11.20 (increment 5b; the review's N3). -->
<!-- 2026-11-14 (R, the release-notes audit), for H: GoCgoLibraries is new in this release. It was added on the
     branch merged as 816009e491 (batch 284), and golib at nuget-1.26.10.3 has no such class, so no compiled code
     can be "unaffected" by an overload of it. Suggest folding the CS0121 note into "cgo: `go2cs -cgo` converts a
     package that calls C" below, or dropping this section. -->

**cgo converts more of what real programs write.** Under `-cgo`, `C.CString`, `C.CBytes`, `C.GoString`,
`C.GoStringN`, `C.GoBytes`, `C.malloc` and `C.free` now convert, a `void *` crosses when it is nil or points at C
memory, and `defer C.f(…)` converts. The helpers copy bytes, with no text encoding in between, and what `C.CString`
returns comes from the package's own `malloc`, so the package's own `C.free` frees it. go2cs still hands C no pointer
into Go memory: a pointer into a slice or an array is refused when the package is converted, and `unsafe.Pointer(&x)`
is refused when the call runs, each by name. Each of these was run against the Go build of the same program on
linux/amd64, darwin/amd64, darwin/arm64 and windows/amd64. Every arm printed what Go printed, or, where it hands C Go
memory, was refused by name. A `-cgo` conversion is one package, and it builds against a go2cs clone
(`dotnet build -p:go2csPath=<go2cs>/src/`); it does not use the `go.*` packages yet. Building a cgo package against
the `go.*` packages from the feed is planned for the next release.
<!-- src/go2cs/internal/clihelp/options.go, the -cgo entry's history comment ("Convert a single cgo package"; this entry describes the November 19 release, when -cgo did not combine with -recurse); src/go2cs/csproj-template.xml:135 (`<ProjectReference Include="$(go2csPath)gen/go2cs-gen/go2cs-gen.csproj"`) and src/go2cs/csproj-template.xml:187 (`<ProjectReference Include="$(go2csPath)core/golib/golib.csproj" />`). COORD's ruling O2, 2026-11-14 (cgo 3.1's release-path seat). -->

**A pointer rebuilt from a number is refused too, with a narrow memory-safety gap left.** go2cs refuses, by name,
any address inside Go memory that it is holding still, even after the address has been through a `uintptr`:
`u := uintptr(unsafe.Pointer(p))` followed by `C.f(unsafe.Pointer(u + n))`, the reversed sum
`unsafe.Pointer(n + uintptr(unsafe.Pointer(p)))`, and Go's own one-expression form, which converts to `unsafe.Add`.
Three cases still cross, because go2cs is no longer holding the memory, or never was. In each, the call goes ahead,
and the memory may move while C is using it: a number kept after the last use of the pointer it came from, which Go's
own rules already forbid; a number taken inside a method with a pointer receiver, from a pointer of the receiver's own
type; and Go memory held still before the program's first cgo package was loaded. A program that needs this
memory-safety guarantee should avoid those three. See [Limitations](Limitations.md#cgo).
<!-- 2026-11-15 (H): narrowed by the pin-extent seat (docs/phase4/DESIGN-cgo-pin-extents.md, section 11) to the three
     rows its section 4 leaves open: 7 (a released pin), 8 (a number minted with no pin) and 11 (an early pin). The
     arms are named in the Limitations entry's notes. The text it replaces said that go2cs "cannot yet tell such a
     number from an address in C memory", for every number held in a variable. -->

<!-- docs/phase4/DESIGN-cgo-phase3-marshaling.md §16 (increment 3.1) and §16.9 (the residue, COORD's ruling C). cgo
     Phase 3 is a STRETCH for this release by the owner's ruling: this section ships with the release that carries
     increment 3.1, and moves to that release's notes if it is not this one.
     2026-11-14 (H), answering R's note below: CONFIRMED, the stretch condition is met. Increment 3.1 is on master
     (43ac9e33fa, batch 305), and its re-gate merge 932ed46c19 is seated in batch 306, so this section ships with this
     release. The four targets are the hosted cgo-e2e legs 43638330006 (linux-x64), 43638338397 (win-x64) and
     43638346986 (osx-arm64 and osx-x64), 50 of 50 arms each, at 54b84f9ff7.
     The gap example is RE-READ, as R asked. The one-expression form is emitted as unsafe.Add on master
     (src/go2cs/pointerArithmetic.go; TestPointerArithmeticFormEmitsUnsafeAdd's "sum" arm). unsafe.Add's result keeps
     the box it was given, so the door refuses it, as the goadd and goaddlocal arms read on all four legs. That
     test's controls "held" (u + n) and "reversed" (n + uintptr(p)) stay numbers, which is the gap as now worded.
     The residue's design is docs/phase4/DESIGN-cgo-pin-extents.md (claude/h-cgo-pin-registry). -->
<!-- 2026-11-14 (R, the release-notes audit), for H and COORD: increment 3.1 is on master. 43ac9e33fa ("cgo Phase 3
     increment 3.1: the helpers, void * and the deferred call (GREEN)") came in with batch 305's f4b803dfd8, and
     docs/Limitations.md already describes the helpers. The stretch condition reads as met: confirm, and drop the
     comment above. The "known memory-safety gap" example (the one-expression form) may also read differently after
     the interior-pointer root, which converts that form to unsafe.Add: worth H's re-read. -->

**`*(*F)(e) = fn`, a func stored through an `unsafe.Pointer`, now stores what the target can hold.** Code translated
from C by ccgo fills a table of C function pointers this way, at a computed address
(`*(*func(…) int32)(unsafe.Pointer(uintptr(unsafe.Pointer(&table)) + 8)) = fn`). The store wrote a managed
reference into the table's memory before, and the number read back from the table named no func. It now stores the
func's word where the target is a word, and the func itself where the target is a func variable or a func field.
A target that is neither panics by name. The standard library has no such store. See
[Pointers](ConversionStrategies-Reference/pointers.md#a-func-value-carried-as-a-number--one-word-per-static-function-through-gofunccookie).
<!-- docs/phase4/DESIGN-interior-pointer-root.md 4.8 B1 and 10.4; golib's unsafe.Pointer.StoreFunc. Arms: the
     behavioral programs FuncWordShapes (the w3 rows) and InteriorPointerShapes (shape 1), GolibTests
     FuncStoreThroughPointerTests, the converter's TestFuncWordShapes. CENSUS, read 2026-11-14 at go1.26.10, tests
     included: 0 such stores in the standard library on windows/amd64, linux/amd64, darwin/amd64 and linux/arm64;
     9 in modernc.org/sqlite v1.62.0 (vfs/patches64.go:18-26), all of top-level functions. -->

**`unsafe.Add` over a nil pointer now answers the offset, as Go does.** `unsafe.Add(nil, n)` is the address `n`; it
answered nil before. Pointer arithmetic written in one expression, `unsafe.Pointer(uintptr(p) + n)`, converts to the
same call, so it gives the same answer over a nil `p`. A program that compared such a result with nil sees a non-nil
pointer now. No call site in the standard library does. See
[Pointers](ConversionStrategies-Reference/pointers.md#unsafepointeruintptrp--n-is-emitted-as-unsafeaddp-n--the-moved-pointer-keeps-what-p-carries).
<!-- docs/phase4/DESIGN-interior-pointer-root.md 10.7 (COORD's ruling of 2026-11-14 07:52Z); the arm is GolibTests
     InteriorPointerRootTests.AddOverANilPointerAnswersTheOffset, and the behavioral program UintptrUnsafePointerIdiom
     prints the same under Go. CENSUS, read 2026-11-14 at go1.26.10 (non-test sources outside cmd): 17 call sites of
     unsafe.Add. syscall/env_windows.go:88, 93; runtime/mgcmark_greenteagc.go:753, list.go:58, list_manual.go:65,
     malloc.go:1181; reflect/type.go:1205; internal/syscall/execenv/execenv_windows.go:37, 39, 44;
     internal/runtime/maps/group.go:131, 133, 139, 142; internal/runtime/gc/scan/scan_reference.go:32, scan_go.go:37,
     81. None passes a base that its own code allows to be nil together with an offset that is not 0: runtime/list.go
     and list_manual.go test `p == nil` before the call, and the others add to the address of a variable, a receiver
     or an allocation. -->

**`(*[N]T)(p)[lo:hi]` over an `unsafe.Pointer` is now a view of the memory `p` names, as in Go.** The slice was a
copy before, so a write through it never reached the memory. Code translated from C reaches memory this way
throughout: modernc.org/libc's `memcpy` is `copy((*RawMem)(unsafe.Pointer(dest))[:n:n], …)`, and with a copy as its
destination it copied nothing. The converted code now calls `@unsafe.Pointer.View`, and `copy` over an open form
(`copy((*RawMem)(p)[:], src)`) calls `@unsafe.Pointer.CopyInto` or `CopyFrom`. A view of C memory aliases it. So
does a view of a Go array or slice of the same element type, and a view of any other Go value that holds no
reference (a number, a struct of numbers, an array of another element type): the value is pinned and the view
keeps it in place. A Go value that holds a reference is still viewed as a copy. What a program can see change:

- a write through such a slice reaches the memory, and a later write to the memory shows through the slice;
- a two-index view has Go's capacity, the array type's length less the low bound, where it had its own length;
- a view or a copy that would pass the end of the Go array or the Go value it points into panics by name, where
  it read whatever lay past it;
- a length too large for a slice panics by name, where its low 32 bits were used.

The open form of an unnamed array type, `(*[N]T)(p)[lo:]`, takes the same view, and `copy` into or out of it the
same calls. It took them only when the array type was named: over an unnamed one it read the whole array as a
value, so over memory that holds no Go array of that type it had length 0, and as `copy`'s destination over the
bytes of a number it ended the process.

In the standard library 13 lines in four files of `runtime` take the new form, and the open form of an unnamed
array five more, in `reflect`, `internal/reflectlite` and `runtime`. See
[Pointers](ConversionStrategies-Reference/pointers.md#a-slice-taken-through-ntp-aliases-the-memory-p-names-the-view-doors).
<!-- docs/phase4/DESIGN-array-pointer-doors.md, sections 4, 10 and 11; golib's unsafe.Pointer.View, CopyInto and
     CopyFrom. The unnamed open form: batch 311, P5's D4 (claude/p5-d4-open-slice 06eccfb59d; converter
     e3adf166836, corpus d7a8cfe4262: 5 shipped lines, reflect/type.cs, internal/reflectlite/type.cs, runtime/type.cs,
     runtime/linux/os_linux.cs, runtime/windows/os_windows.cs, plus 2 lines of a .cs.auto review sibling; design
     16.10; behavioral OpenSliceUnnamedArray). Arms: the behavioral program ArrayPointerViewDoors (linux; native pages from syscall.Mmap and Go memory)
     and the goldens of PointerArrayViewForms and PointerCastSliceReinterpret, GolibTests ArrayPointerViewDoorTests,
     the converter's TestArrayPointerViewDoor, repoguard's TestArrayPointerViewDoors. The pinned case (a Go value
     of another type): design section 12, golib's IHeldStorage, GolibTests' stress arm and its control, 10,000
     rounds, 2026-11-15. FOOTPRINT, two-seeded, four
     targets, 2026-11-15 at go1.26.11: runtime/heapdump.cs 5 lines, iface.cs 2, mbitmap.cs 2,
     runtime/windows/os_windows.cs 4; the hand-owned internal/syscall/windows/registry value.cs is respelled by
     hand, 8 lines. -->

**An index, an element's address or the whole array through `(*A)(p)` reaches the memory as well.** `(*A)(p)[i]`
(a read, a store, a compound assignment or `++`), `&(*A)(p)[i]`, `*(*A)(p)` and `*(*A)(p) = v` now go to the
memory `p` names without making a box of the whole array, so an indexed loop over C memory allocates nothing.
modernc.org/libc writes a string's terminator this way (`(*RawMem)(unsafe.Pointer(p))[len(wd)] = 0`), and sqlite
indexes small unnamed arrays the same way. In the standard library 45 lines take the new form, nearly all of them in
`runtime`'s memory allocator. A named array type that holds references, read as a whole value from memory that cannot hold it, now panics by
name at that read. It used to store a managed reference into that memory, and nothing reported it. See
[Pointers](ConversionStrategies-Reference/pointers.md#an-index-an-elements-address-or-a-whole-array-through-ntp-reaches-the-memory-p-names-the-element-doors).
<!-- Door B: merge 0cbd1eae70 (P5, claude/p5-array-door-b 6946dcc33e): Element, ElementRef, LoadArray and StoreArray on
     @unsafe.Pointer; "0 B over C memory" (GolibTests ArrayPointerElementDoorTests.AnIndexedLoopOverNativeMemoryAllocatesNothing);
     regen 45 lines in 9 files (runtime malloc_generated.cs 30, the four per-target malloc.cs and malloc_stubs.cs 2
     each, alg.cs 1, lock_spinbit.cs 2, internal/runtime/maps runtime_faststr.cs 2; git diff --stat of the merge). The floor: merge a9dc1336a2 (P5,
     claude/p5-named-array-floor 4c8ee1cbd2): "a native box of a named-array wrapper (reference-holding, e.g. libc RawMem)
     is refused BY NAME at its first whole-value READ (ValueAddress), never at mint ... the silent managed-reference store
     into C or Go memory (arm M0) is gone". -->

**A view of one field's bytes over an ordinary Go struct now reads and writes the field, as in Go.** Go code reads
the two bytes of a port, or the halves of a 64-bit word, with `(*[2]byte)(unsafe.Pointer(&h.port))[1]`. Over a Go
struct holding no reference, only `len` of such a view was right: an index panicked with "index out of range with
length 0" or a nil dereference, and a `range` by value over it read zeros where Go read the field, with no error.
Now the view indexes the field in place (a read, a store, a compound assignment and `++`), and the whole array
through it works as Go's `*p` does: `*p` read as a value is a copy, `*p = v` stores in place, `p[lo:hi]` is a
slice of the field, and a `range` over `p` reads each element when its round comes, while a `range` over `*p`
ranges a copy.

The view also works over a struct that holds a string, a slice, a map, an interface or a pointer, which is where
`golang.org/x/sys` reads a socket address (`unix` on linux and macOS, `windows` on Windows), and through a chain of
field selections from a variable (`&sa.raw.port`). Over such a struct the index, the whole-array load and store,
`len` and a `range` that takes only the index work; a slice of the view, the address of one of its elements and a
`range` by value are refused by name, and so is a tuple assignment that stores the whole array through two views at
once. One related form can end the process: a slice taken where it is written, `(*[N]E)(unsafe.Pointer(&x.f))[:]`,
over a promoted field, a slice element's field or a call's result
([Limitations](Limitations.md)). See
[Pointers](ConversionStrategies-Reference/pointers.md#the-bytes-of-one-field-through-ntunsafepointerpf-the-field-view).
<!-- F1 parts A and B: merge 0d1f6f1f3f (P5, claude/p5-field-view-index d073be7130), BOARD 2026-11-15 "the field view
     answers over a Go struct, and is indexed in place where it is written" (the before-probe at 516586c4a9,
     linux/amd64 Release: over a Go struct with no reference, 9 forms panic "index out of range with length 0", 4 "nil
     pointer dereference", a range by value reads 0x0 where Go reads 0xf1). Part C: merge 0c1d295ea8 (claude/p5-field-view-whole
     da7cb0d29f), BOARD 2026-11-15 "the whole array through the field view is a load, a store or the view's window".
     Both footprint 0 on four targets. The reference-holding limit: BOARD 2026-11-15 "the field view of a struct that
     holds a reference is refused by name" and its census amendment (TypeScript closure: 17/17/4/7 sites on
     linux/amd64, linux/arm64, darwin/amd64, windows/amd64; nugetgo modules 17/17/4/0; all x/sys). The tuple store:
     BOARD 2026-11-15 "a tuple assignment that stores the whole array through two field views is refused by name"
     (proposed .5). The reference-holding struct and the chain: batch 310 merge e207d6f5b6 (P5, claude/p5-reinterpret-array-box
     a1a1d8534b), BOARD 2026-11-15 "a field view over a struct that holds a reference reads and writes the field's own
     bytes" (DONE; footprint lsf_linux.cs 3 lines on linux x2) and its row "a range by value, a slice and an element's
     address ... are refused by name" (proposed .5). The process-ending open slice: BOARD 2026-11-15 "the open slice of
     a pointer conversion over an array field of another element type ends the process" (.5), disclosed in
     docs/Limitations.md for this release. 2026-11-16 (P4): the earlier text's limit ("the view of a field of a struct
     that holds a string ... panics by name") was rewritten after 310 landed; if P5's D4 (311) cures the open slice in
     this release, drop that sentence. 2026-11-18 (P4, HOOK 311): D4 landed in 311 and does NOT cure it, so the
     sentence stays. D4 is the open slice of an UNNAMED array (the array-view section above); its own control keeps
     the open form over a field of another element type on the native field view (e3adf166836), and
     docs/Limitations.md's bullet is unchanged at master f36b6b0d48. -->

**A write through the address of a named array type reaches the array, also before the program has used it.** For
`type small [8]byte`, `var local small; fill(unsafe.Pointer(&local), 41)` left `local[0]` at 0, where Go prints 41.
The same write was lost through a package variable, a struct field, `new(small)`, and the address converted to a
`uintptr` or to a raw pointer, and so was a `memcpy` from libc into a fresh named array. It reaches the array now,
and a first use of the array on one goroutine racing another's first use of its address no longer loses writes
either. No line of the converted standard library changes; the fix is in golib.
See [Pointers](ConversionStrategies-Reference/pointers.md).
<!-- BOARD 2026-11-15 "a write at the ADDRESS of a named array that has not been used yet is lost" (arms la-le lost,
     lf/lg controls right, linux/amd64). Option 1: merge 882e3da582 (P5, claude/p5-lazy-named-array-pin 70abf5dd5a;
     7 red + 2 controls -> 9 green; behavioral NamedArrayAddressWrite = go run; golib only). Option 2: merge 85b1c4181b
     (claude/p5-holder-publish c6479baf76; 5 racing arms lost 6,735-78,243 of 200,000 -> 0, the control still loses;
     golib only, no emitted line). The lazy backing (the go2cs-gen InheritedType template's interlocked publish) and both
     pin paths (golib's ж box, `ValueSlot is IArray`) are in the nuget-1.26.10.3 tree, read with git grep at the tag, so
     this is a fix to released behaviour. -->

<!-- 2026-11-14 (R, the release-notes audit, COORD item 5): every first-parent merge from the nuget-1.26.10.3 tag
     (c35c0269b8) to master c2919b8b16 was classified, and each user-visible change with no section here or in
     NEWS.md's held entry got one below, verified against its merge and records. The heading names the owning
     seat so its owner can correct it. Ordered with the headline first; at release they merge into the prose. -->

<!-- 2026-11-16 (P4, COORD's order: the draft brought current through batch 309, checked against master 0c1d295ea8).
     Every first-parent merge of batches 306-309 was classified. A line was added for each user-visible change to
     released behaviour: door B and the named-array floor (0cbd1eae70, a9dc1336a2), the field view (0d1f6f1f3f,
     0c1d295ea8), the lost write (882e3da582, 85b1c4181b), five sqlite-cs shapes (6992717d09, c09d5e04da,
     39b40c8c3a, b62c67b6d8, 4c101b8e57), the inline-array slice limit (6b78cef1a1) and the validated-modules page
     (65be87fd89, 64a5cc7e24). Already carried by their own seats' edits: door A (2d7ac32ffb), the pin-extent fixes
     (dbd872acad), cgo 3.1 and the cgo truth pass (d658f700cb, 3b202a6c56). Written after P1's AOT reading (2026-11-16): 055f4e6230 and d39b1ffe00, in the
     Tuning section. LEFT OUT, each a fix to a feature new in this release, so no released build had
     the defect: the inline-array fixes 6b78cef1a1 (the foreign slice itself), b1c73ce844, d7f5a59f79, a6f526f31e
     and 63c2a339c7; the unsafe.Add box chain 18b36cb712, which came with batch 305's derived pointers. LEFT OUT as
     internal (CI, tests, records, design docs): 6f5def3cca (two darwin roster rows; the roster carries them),
     9b05e0dc4c, bd709b8206, 9c3f648b52, 1cfa713959, ec0a21ea2f, 3923dcda87, b4ac41960f, 5aa38c3ac2, 2c2c5b0ec0,
     b2214e00e4, 4415da65d3, 927441cd79, ec250198ff, d5a1f672fd, a0878172b2, be8c7a6859 (the zig pin for the
     linux-arm64 cgo smoke), 9615f7fc18 (proof-page pipe rendering; no committed page changed). -->

**`a = b` for an array now copies `b`'s elements into `a`, as Go does, so a slice of `a`, a pointer to one of its
elements, or `a`'s address taken before the assignment sees the new values.** The converted code gave `a` a new
array, so everything taken from `a` beforehand still named the old one: a read through it saw the old values, and a
write through it was lost. The same holds now for a field, a package variable, `*p = b`, a struct that holds
arrays, a multiple assignment, a `range` that assigns with `=` and a `select` that receives into an existing
variable. A conversion between array types, `x := [4]int(n)`, now copies `n`, where it shared `n`'s array. A store
into a map element still copies, because a map element is a value in Go. In the standard library 70 converted files
change across the four targets, each at such an assignment.
<!-- Batch 311, R's array reassign cut A (claude/r-array-reassign 99f34a588e; docs/phase4/DESIGN-array-reassign-in-place.md
     section 8 with P5's conditions). golib/go2cs-gen/converter 5953f454d02; corpus b6ef01b7672 (79 files, the
     four targets in one -platforms run, 356 packages each: 70 converted .cs, 8 package_info.cs map
     re-encodes, and mlkem/mldsa field.cs's N1 return clone, one allocation more per call there); the map element
     keeps its clone 7820722409e; the native pointer array's tail store 299221fe232. Behavioral ArrayReassignAliases
     (7 forms: slice, element pointer, array pointer and argument, from a variable, field, package var, through a
     pointer), ArrayReassignForms and ArrayReassignUnsafe; GolibTests ArrayAssignInPlaceTests. -->

**A copy of a struct copies an array the struct reaches through an embedded struct, and a conversion copies its
operand, as in Go.** A struct that held an array only through an embedded struct was copied without its array, so
the copy and the original shared it, and a write through one showed in the other. A conversion between struct types,
`T(v)`, built its result from `v`'s fields, so an array field, or a nested struct holding one, was shared with `v`
the same way. Both now copy. A conversion copies once, where its result is stored or passed, and not where it is only
read. See [Struct types](ConversionStrategies-Reference/struct-types.md#a-conversion-copies-its-operand).
<!-- Batch 312 (ledger afe47908f0): P2's W-embed-array (claude/p2-embed-array-copy 40699e4fe6; converter 084af172ce7,
     typeNeedsValueClone and structValueCloneFields now visit embedded value fields; fixture EmbeddedArrayCopy;
     corpus b2d40722376 from the two-seeded footprint on four targets) and W-conv-copy (claude/p2-wall-c a7540ab86d;
     structConversionCopy.go, b2e58928b51; fixture StructConversionCopy). The single clone: P2's union fix
     ad5e15194c (926566a93f1: the operand copy and R's result copy both fired; the result copy is kept; mldsa/mlkem
     field.cs lose 4 double clones, 9dd0f84c9a). -->

**`copy` and `append` copy an array element's value.** A slice whose elements are arrays (`[][4]byte`), or structs
that hold one, kept one inner array for two slices after `copy(dst, src)` or `append(dst, src...)`: a write through
`src[0]` showed in `dst[0]`. Each element is now copied as Go copies it. A copy into an existing slice adds no
allocation; an `append` that grows the slice allocates once for each such element it moves, where Go allocates none.
<!-- Batch 315 (P2, claude/p2-dot4-copy-inner-arrays 3f039d41e4; merge 280a01952c): golib GoElementCopy;
     docs/ConversionStrategies-Reference/slices-and-arrays.md, the known gap (1) closed; behavioral
     SliceArrayElementCopies (20 of 24 lines differed from Go at 8a86140f79) and GolibTests SliceArrayElementCopyTests.
     Added at the release cut (P4, release controls). No news item for batch 313 (P5, reflect array copy, eb0055ad3b):
     it cures the encoding/gob lost write that batch 311's in-place array assignment introduced inside this release,
     so no published release had it. Nor for 313's text/tabwriter holder fix or 4c5ed19247, both inside OQ2's holders
     before any release. -->

**A Go pointer, func, interface or `unsafe.Pointer` read or written whole through a pointer into native memory is
refused by name.** For `*p`, where `p` points into memory from C or the operating system and the value is one of
those types, the converted code took the memory's word for a managed reference: a read produced a reference to an
object that does not exist, and a write stored a reference where the garbage collector cannot see it. Both now
panic, naming the type. An `unsafe.Pointer` over memory Go allocated still reads and writes as before. golib's
maps, channels, strings, slices and arrays are not affected.
<!-- Batch 311, H's native-box reference floor (claude/h-nativebox-ref-floor 3ce394cbc0): golib 84e400bcc62,
     NativeBox<T>.ValueAddress throws NativeStructLayoutMismatch(T, "a managed reference where Go stores words") for
     a CLR reference-type T at a nonzero address; the exemption is COORD's ruling C, narrowed by source (managed Go
     storage: the converted runtime's map.go indirect key and atomic_pointer.go atomicwb). Found as cgo-e2e
     s6next's entry.next (DESIGN-cgo-phase3-marshaling.md section 18). The exempt read's managed-word hazard is an
     OPEN BOARD row for .5 (3ce394cbc0), cure in section 18.2. -->

**A pointer made inside a Go value now keeps that value alive and in place.** Go lets a program point into the middle
of a variable, a struct or a slice's array with `unsafe`: `(*T)(unsafe.Add(unsafe.Pointer(p), off))`, or in one
expression, `(*T)(unsafe.Pointer(uintptr(unsafe.Pointer(p)) + off))`. In Go that pointer keeps the memory alive.
Converted code turned it into a number on its way to `*T`, and a number keeps nothing: the new pointer held no
reference to the value and did not stop the .NET garbage collector from moving it. Once nothing else referred to the
value, a write through the derived pointer went to memory the collector had already reused (a probe on linux/amd64
read 0 where Go reads 22), and after the collector compacted the heap a read returned the old bytes from the old
address. This is plain Go, with no cgo involved, and nothing reported it.

Now `(*T)(p)` over an `unsafe.Pointer` converts to `unsafe.Pointer.To<T>(p)`, which receives the pointer itself, and
pointer arithmetic written in one expression converts to `unsafe.Add(p, n)`, which moves the pointer and keeps what
it pointed into. A method that does this arithmetic on its own pointer receiver passes on the receiver itself. An array
field reached through such a pointer, and a slice over it, keep the value too. In the standard library the one reached
site of this shape is `syscall.UnixRights` and `UnixCredentials` on linux and macOS; three stress runs of 300,000
messages each on linux/amd64 never caught it failing, and it is closed by the same rule.

**A pointer returned from a function keeps it too.** A function that returns an `unsafe.Pointer`, such as
`return unsafe.Add(unsafe.Pointer(&b[0]), 4)`, had its result turned into a number at every call. On windows/amd64 a
program that wrote through such pointers after a garbage collection missed the slice, and the stray writes corrupted
the .NET heap: the program died with `Fatal error. Internal CLR error. (0x80131506)`. The call's result is now used
as it is. The standard library's calls of this kind, most of them in `runtime`,
`internal/runtime/maps` and `reflect`, changed with it.

**What you need to do.** Convert your own code again. The standard library in the 1.26.11.1 packages already carries
the new forms. Code converted by an earlier go2cs still builds against the new packages, but it keeps the old
conversion, and with it the hole.

**Limits.** A pointer rebuilt from a `uintptr` kept in a variable (`u := uintptr(p)`, later
`unsafe.Pointer(u + n)`) still keeps nothing, as Go's own rules say of it. So do a sum whose first term is not the
pointer and a rounded or masked address. A typed pointer made by arithmetic is not equal to the field's own address
(`(*int64)(unsafe.Add(unsafe.Pointer(&p), 8)) == &p.y` is false, where Go says true). One cost: a method that does
pointer arithmetic on its receiver pays for keeping the value at each call. On linux/amd64 that is 6% to 20% in the
standard library's own shape, and nearly twice the time when the receiver is a field of another struct, which no
standard-library caller does. See
[Pointers](ConversionStrategies-Reference/pointers.md#tp-over-an-unsafepointer-is-emitted-as-unsafepointertotp--the-typed-pointer-keeps-what-p-carries).
<!-- The interior-pointer root: docs/phase4/DESIGN-interior-pointer-root.md (on master since
     batch 307, merge 3923dcda87) sections 1.1 (the 12-shape program, shape 8:
     Go 2 then 22, converted 2 then 0, linux/amd64, 3 runs), 1.3 (CmsgStress, 3 runs x 300,000 messages, 0 wrong,
     recorded LATENT), 4.1-4.5, 10.1 (H's U2, ATypedPointerConvertedFromAnInteriorUnsafePointerKeepsItsStorageAlive: red
     3 of 3 after a compacting collection, the pointer reads the old bytes at the old address), 10.12 (leftmost-term
     rule, the node rule), 10.14 (S11: ctrlGroup get/set +6%/+12% and +17%/+20% in the package's own shape, +77%/+95%
     on a field receiver, linux/amd64, 20,000,000 calls, median of 5). Merge f4b803dfd8 (cut 837a413fdc): commits
     3527460c88 (golib To<T>; the old number operator kept, so old emission still compiles), ba226db89e (the door; probe:
     shape 8 reads 22 as Go does, it read 0), 58f547b592 (arithmetic form, receiver node rule), 86247389c0 (H 4.4/4.5,
     array-field windows), 837a413fdc (regen: 160 files, 938 out / 910 in; the 19 touched packages build on
     linux/amd64, windows, darwin and linux/arm64; no test host run on the regenerated corpus). Shape 9 still differs:
     bd7559f9bd ("the original repro differs from Go on line 9 only"); pointers.md, Equality and Limits paragraphs.
     The UnixRights site in the linux and darwin builds of syscall (sockcmsg_unix.cs) now reads
     `@unsafe.Add(@unsafe.Pointer.FromPinnedBox(Ꮡh), ...)`.
     The returned pointer: docs/phase4/DESIGN-unsafe-pointer-call-result.md sections 2, 7 (UnsafePointerCallResultLifetime
     at master 3da19d0e2e: "Fatal error. Internal CLR error. (0x80131506)", 3 of 3 runs, windows/amd64) and 8 (108
     files, 758 lines each way, 17 packages; the windows/amd64 sweep of 57 rows all at their banked counts); merge
     a6cefba6ee. -->

**The go.* packages now ship a linux-arm64 build of the standard library.** Through 1.26.10.3 they shipped
`win-x64`, `linux-x64`, `osx-x64` and `osx-arm64`, and a program built for linux-arm64 compiled against, and ran, the
Windows build of every package whose code differs by platform, with no error. Each such package now carries a fifth
folder, `runtimes/linux-arm64/`, converted from Go for `linux/arm64`. It calls the kernel with arm64's own
system-call numbers and structure layouts, and reads the processor's features from the kernel as Go does. A project
built with `-r linux-arm64`, or on an arm64 Linux host, uses it with no change. Go's own tests for every package on
the [validated roster](ValidatedTestPackages.md) that runs on Linux were run on GitHub's hosted Arm runner
(`ubuntu-24.04-arm`), and every one is validated there as well as on linux-x64. `go2cs -stdlib` accepts `linux/arm64` as a fourth
`-platforms` target.

A known limit: 15 internal standard-library members that Go writes in arm64 assembly have no managed body yet and
throw when called. A program reaches one only through a standard-library path. See
[Known issues](KnownIssues.md#on-linux-arm64-15-standard-library-members-throw-when-called).
<!-- The design and seats: docs/phase4/DESIGN-linux-arm64-flavour.md (S0-S7) and its review
     docs/phase4/REVIEW-linux-arm64-flavour.md. Packaging: S6, 83789ecbcc (P2, claude/p2-la64-s6-packaging),
     docs/phase4/DATA-la64-s6-packaging.md section 8 (release-smoke run 42983079137: every platform-split package
     carries five RID folders); the shipped-RID statement: docs/KnownIssues.md "Platforms the packages do not ship"
     (amended by 2e98774b35). Corpus: S2 1dc059d34a (G). Hand-owns: S3 93eb8c392d, 85047f1165, a79e5af297, b328dd71e7,
     9c95daacb6 (i9), census docs/phase4/DATA-la64-s3-census.md. hwcap: S4 6c6ecc776c (C2). Validation: S7 banks
     e141a180ce (P2), 7c7dc687f8 and 6db3fc9dd3 (C1); docs/ValidatedTestPackages.md, the Linux headline
     ("linux-arm64: 236 of 236 banked, 0 pending"). The 15 members: KnownIssues, from S6 66c13e7168. -->

**Every package that applies to macOS is validated on Intel and on Apple silicon.** 1.26.10.3 validated 229 of the
236 packages that apply to macOS. In 1.26.11.1 all 236 are validated on Apple silicon (`osx-arm64`) and on Intel
(`osx-x64`), each chip compared with Go built for that chip, with one exception: `runtime`'s Intel reading is the
one 1.26.10.3 banked, because at 1.26.11.1 it read below its count twice, for two different causes. Its Apple-silicon
reading is this release's. See the macOS line in [Validated test packages](ValidatedTestPackages.md).
<!-- Added at the owner's question whether this is the first validated macOS release (COORD 2026-11-19 14:51Z): it is
     not, and it is the first in which every applicable package validates on both chips, runtime's osx-x64 cell
     excepted. Sources: the 1.26.11.1 bank, batch 316 (P5, claude/p5-bank-12611-assembly @ c75a37fdd3): the roster's
     macOS line ("236 of 236 applicable rows validated on Intel and on Apple silicon"; runtime's osx-x64 reading is
     the 1.26.10.3 bank's, read below its count twice at 1.26.11.1, BOARD 2026-11-19 P5 block) and its parity
     sentence. 1.26.10.3: the .3 darwin leg, 229 of 236 (not banked on macOS: debug/buildinfo,
     go/internal/gcimporter and net/http/pprof under arch-strict banking, internal/synctest, net, internal/trace,
     syscall). Kept out of the title until runtime validates on Intel at this release. -->

**A build for a platform the go.* packages do not ship now says so, instead of running the Windows build.** On
another operating system, or on Linux with another architecture (`linux-riscv64`, for one), the build stops with
the new error `GO2CS3003`, naming the platform and the first package that has no build for it. On `win-arm64` it
warns, `GO2CS3004`: the Windows build is that operating system's own, so the program runs. A build that only needs
to compile, and knowingly wants the Windows build, passes `-p:GoAllowReferenceFlavor=true` on the command line. A
portable app built on one platform and run on another, such as a linux-x64 build run on linux-arm64, prints a
warning on standard error at start-up.

**Two linux-x64 builds now pick the linux-x64 build.** Alpine (`linux-musl-x64`) compiled against the Windows build,
and failed to compile. A build with no RID on Ubuntu's packaged .NET SDK, which reports `ubuntu.24.04-x64` as its
own platform, also compiled against the Windows build, with no error. Both now resolve to linux-x64, as .NET does at
run time. Measured with SDK 10.0.403 and Ubuntu's 10.0.112 against go.* 1.26.10.3 from nuget.org. See
[Known issues](KnownIssues.md#platforms-the-packages-do-not-ship) and [diagnostic codes](DiagnosticCodes.md).
<!-- SEAT 0 of docs/phase4/DESIGN-linux-arm64-flavour.md (section 0, COORD ruling R2 with the review's F4), merge
     3d83f99d7b (C2, claude/c2-la64-seat0; branch commits 67b5578fa3 and 7346dc9015). The gate is
     src/tests/PackageTests/RidCompileAsset/test-rid-compile-asset.ps1 (10 arms; master failed the UNSHIPPED, swap-off,
     project-file opt-out, ALPINE and WIN-ARM64 arms, the Alpine build with CS0426). The run-time half is
     GoHostOfRecord.Verify(GOOS, GOARCH), GolibTests HostOfRecordTests. GO2CS3003's example moved from linux-arm64 to
     linux-riscv64 when S6 shipped linux-arm64 (2e98774b35). -->

**A new page says which names C# code can rely on.** [What go2cs promises to C# code that uses it](PublicSurface.md)
sorts every name a C# consumer sees into three tiers. Frozen: the exported surface of Go packages that are not
`internal` or `vendor`, the type mapping, golib's consumer API (`slice<T>`, `map<K, V>`, `@string`, `ж<T>`, `builtin`
and the rest), `PanicException` and `GoErrorException`, package IDs and the version scheme. Stable within a Go minor
release, and changed only when the break floor moves: generated plumbing and the golib API that converted code
calls. No promise: Go `internal` and `vendor` packages, tuple element names, and which thread runs a goroutine. The
page also lists the consumer MSBuild properties and the one path by which a promised name may change.

**IntelliSense hides the plumbing.** golib's types outside the consumer API, and the plumbing go2cs-gen generates
(interface adapters, `_package_asserts` classes, promoted-method host classes and the like), now carry
`[EditorBrowsable(Never)]`, so completion lists show the Go names. Nothing is renamed or removed, so code that names
one of them still compiles. Go `internal` packages are not marked yet, and no build warning names a use of one.
<!-- merge 1e654296e5 (C1, claude/c1-public-surface), the OQ3 seat and the freeze-and-document items of
     docs/PLAN-public-surface-stability.md (owner ruling 2026-11-11, merged as 9db360e89a). The page: 081ccaafaf and
     cb9f16c5b0 ("says only what the code does"). Hidden golib types and the baseline src/core/golib/PublicSurface.golib.txt:
     4883ac6bb0 ("No golib line moves"; GolibTests PublicSurfaceTests). Generated plumbing: 5a22ea46d5 and 5cba2d28a4
     (GenTests HiddenPlumbingTests). The OQ5 interface rule: 34582b3aef, d0caba1ca3 (GenTests GoInterfaceMemberGuardTests).
     OQ1 not done at this release: PublicSurface.md, Tier 3 ("At this release no package marks them"). -->

**`go2cs -cgo` now converts a cgo package.** Through 1.26.10.3 the flag did nothing and a package that imports `"C"`
was refused. Now, on a machine with a C compiler, `-cgo` converts one package when every C use it makes is a call of
a C function that a shared library defines. Integers, floating-point numbers and pointers cross as arguments and
results. A struct of integers and pointers passes by value, and one struct or one scalar passes by pointer: C gets a
copy for the call, and the copy is written back after it. A function that sets errno can be called in Go's two-result
form, `n, err := C.f(…)`. The package's C types, typedefs and constants convert with it, and the converted C# reaches
them as `C.<name>`, as Go does. Anything else is refused by name, before anything is written, with the phase that
will cover it: C variables, a C function used as a value, a function the preamble itself defines, `//export`, and a
function the C compiler inlines or a header renames (`abs` and `labs` everywhere), among others. The cgo package must
be converted on its own: `-cgo` does not combine with `-recurse`, `-tests` or `-stdlib`. See
[Limitations](Limitations.md#what-wont-make-the-trip) and [`-cgo`](CommandLine.md).

**What you need, and where it runs.** go2cs does not provide a C compiler. It uses the one Go's own cgo build would
use, `go env CC`, and converts for that machine's own operating system and architecture only. On Windows use
llvm-mingw or MinGW-w64 gcc: under zig, Windows links C runtime functions such as `cos` and `exp` statically, so no
library exports them and go2cs refuses those calls by name. A program that uses the converted package needs no C
compiler, only the C library it was converted against. Each library loads by the name captured at conversion (for
example `libc.so.6` on Linux, `/usr/lib/libSystem.B.dylib` on macOS) from where Go's own loader would look, and
never from the current directory. A build of the package for another target is refused by name, and on another
operating system or architecture the converted package refuses to call C, naming both. A converted cgo program was
built and run against the Go build of the same program on linux/amd64, darwin/amd64, darwin/arm64 and
windows/amd64, converted with zig on Linux and macOS and with llvm-mingw on Windows. Every check printed what Go
printed, apart from the differences [Limitations](Limitations.md#what-converts-but-doesnt-behave-exactly-like-go)
lists, all of them in programs that break cgo's pointer rules.

Two fixes reach code that never uses `-cgo`. A package whose Go files all need cgo used to convert to nothing and
exit 0; it is now refused with `GO2CS2001`, naming the cause. And when a package loads with cgo on (under `-cgo`, or
with `CGO_ENABLED=1` set), a plain Go file under `//go:build cgo` now converts; it used to be dropped with exit 0.
<!-- docs/PLAN-cgo-interop.md section 11 and 5.1; docs/phase4/DESIGN-cgo-phase2-pinvoke.md section 11 (11.5, 11.12,
     11.20 increment 5, 11.22 G1 with its third hosted legs 43218540465 linux, 43218554704 darwin, 43218567730
     windows, GREEN on four rows; the windows zig override reading 43218579321 refused C.cos and C.exp);
     docs/phase4/DESIGN-cgo-phase3-marshaling.md 15.1 R1 and R5 (increment 3.0) and 15.6 (darwin/arm64 hand-down).
     Merges: d25c05efd2 (the gate), 18faa6d239 / b07fd14562 / 1fc911cbc3 (Phase 1), a7e19fafd9, 6075a10aeb,
     ddb57f9308, 2e9096000e, 44fad66dde, 86ba10b003, 816009e491, ea56e6ec40, 118cfbc77b (Phase 2 increments),
     b7f47fd8c6 (the real route: go env CC, host-target refusal), 72cb06355b (G1), 4423d66052 (3.0), 9a3a6e6aa0.
     Code: src/go2cs/cgoRoute.go (cgoHostTarget, cgoCapture, cgoTargetBlocks), src/core/golib/runtime/GoCgoLibraries.cs
     (TargetMismatch, Places, "the current directory is never searched"). The R1 half for CGO_ENABLED=1 without -cgo
     was read at nuget-1.26.10.3: its conversionDriver.go re-check (line 342) passed options.buildTags with no cgo tag.
     The flag at 1.26.10.3: CommandLine.md's row read "Not yet functional". -->

**A converted module that reads a struct out of memory laid out by Go, or reinterprets one through
`unsafe.Pointer`, now gets Go's bytes when the struct holds arrays.** go2cs held every Go array field as golib's
`array<T>`, a managed reference, so reading such a struct whole from Go-laid-out memory was refused by name at run
time. Code that ccgo translated from C, such as modernc.org/sqlite and its libc, does this throughout. Now a struct
that its own package converts whole through `unsafe.Pointer` is laid out at Go's offsets and size, with each array
field a fixed-size inline array (`array8<byte>` for a `[8]byte`), and so is every struct it holds by value. The
inline arrays come from `go.arrays.dll`, a new assembly in the go.lib package beside `golib.dll`, which a program
loads only when it uses one. Indexing, `len`, `range` and printing such a field work as in Go. The standard library
is never laid out this way, so its converted code is unchanged.

For a module, the rule is automatic, and `-inline-arrays <pattern>` forces packages on, or `-<pattern>` off;
`-inline-arrays <import path>#<TypeName>` selects one type in its own package, for a type that another package lays
out at Go's bytes and passes back by value. A type
the rule picks but cannot hold inline (an array or struct over 4096 bytes, an array field sliced or addressed, a
struct from another package) converts as before, with warning `GO2CS2009`. Error `GO2CS2010` refuses a
reinterpret between two structs whose arrays disagree, inline on one side and not on the other, when the packages
are forced or the inline side belongs to another package. A known limit: reflection over a struct that holds inline
arrays, `fmt.Printf("%v", s)` among it, panics by name. Print the fields one at a time. A slice of an array held
inline panics by name when the struct around it also holds a string, a slice, a map, an interface or a pointer;
a `copy` into or out of the array with constant bounds works, and so does slicing a local copy of the array. See
[`-inline-arrays`](CommandLine.md), [Limitations](Limitations.md) and [diagnostic codes](DiagnosticCodes.md).

A struct the rule does not hold inline can still be copied out of such memory: `x := *p` decodes its bytes at Go's
offsets into a new value, arrays and arrays of arrays of numbers included, where it was refused by name. A struct
that holds any other reference is refused by name, naming the field. A zero-length array field (`[0]T`, which C
translations use for a trailing array) no longer counts against the struct's size checks. A whole read of a struct
that holds a reference, through a pointer reinterpreted from Go storage of another layout, is now refused by name
where it used to be read as if the layouts matched.
<!-- docs/phase4/DESIGN-array-value-storage.md section 7 (the scout, 7.3 for the before-state:
     src/core/golib/runtime/RuntimeErrorPanic.cs refuses the whole-struct read) and its dated part-D and mixed-pair
     amendments. The library: merge b6f3bbb44f (C2, I0-lib; go.arrays packed by src/core/golib/golib.csproj:160-178#f8f2d9,
     a pack without it fails). The converter: merge 817b56e8b0 (C2, I0 part D; std footprint 0; the census P1 on
     linux/amd64 over modernc.org/sqlite v1.62.0 and libc v1.80.1). Mixed pairs: merge 555e30c826 (C2,
     claude/c2-inline-pairs, 3c25375a04). Emission shape: src/tests/Behavioral/InlineArrayFields. The reflection
     limit: docs/Limitations.md "Reflection cannot read a struct whose arrays are held inline". The slice limit:
     docs/Limitations.md "A slice of an array held inline panics when the struct around it also holds a string, …",
     merge 6b78cef1a1 (C2, go-winio Part 2, claude/c2-inline-foreign-slice 0333434616; COORD ruling 3b, disclosed for
     .4). B2: merge 592c8346ef (C2, claude/c2-b2-value-decode 7dfbf9595e; e2d8c170f9 GoValue, 4c8ce23f81 the rvalue
     *p; GolibTests NativeBoxValueDecodeTests; footprint 0 x4). W14 (a) and B0': merge eeb8a79ff9 (C2, 953a27d8e3 and
     150d1fdbd4; BOARD 2026-11-15 W14 row). -->

**`type Q pair[int]` and `type P[T any] pair[T]` now convert.** A defined type whose underlying type is an
instantiated generic struct used to make the converter drop the whole file that declared it, and the run still
exited 0, so the package compiled without that file's declarations or failed later for want of them. The type now
converts to a wrapper over the instantiated struct: its fields and methods are reachable, a generic defined type
declares its own type parameters, and a composite literal of it works inside and outside its package. The same
holds when the generic is reached through a type alias, in the same package or another. The code `GO2CS2004`, which
development builds briefly reported for this shape, is retired and stays reserved. A known limit: a second defined
type stacked on such a type, when the struct is declared in another package, does not yet get every member and
conversion generated, and fails at the C# build. See [diagnostic codes](DiagnosticCodes.md).
<!-- P1, batches 279 and 280: bf488d1296 (claude/p1-ts-go2cs2004, d0227c54b3), afbcc32974 (claude/p1-ts-go2cs2004-alias,
     9bd562223b); the interim refusal was H's C17, 60b13275fb; DiagnosticCodes.md's "Retired" row restored by 596a66cc89.
     The file-drop with exit 0: BOARD row from H's C11 seat (240a4dea0e). Std 0 sites. The known limit: BOARD
     2026-11-09 "a defined type whose chain reaches ANOTHER package's struct forwards no members past the first hop"
     (D1-D6) and 2026-11-10 (alias of an unnamed type), both OPEN, PARKED (0 std reach). -->

**A converted module names no directory of your machine.** `-recurse` on a local module now records each of the
module's own source files the way `go build -trimpath` does, so a converted stack trace or `runtime.Caller` names the
file as `<module path>/<path in module>`, where it used to carry the absolute path of the converting machine. This is
the new `-trimpath` option, on by default; `-trimpath=false` keeps the absolute path, as an untrimmed `go build`
does. Standard-library and module-cache sources are recorded the same way either way. A module converted again
records its sources the new way. See [`-trimpath`](CommandLine.md).

**`-provenance` now says the file was changed.** The option writes two lines into each converted file, the Go source
file with its module and version, then `// Modified from the original by conversion to C# with go2cs <release>.`,
where it wrote one line naming the source file. The notice now follows the file's upstream copyright and license
comments, before the package doc comment, and still carries no timestamp. This is the notice Apache-2.0 asks of
modified files: convert a module under that license with `-provenance`. See [`-provenance`](CommandLine.md) and
[Converting a module](ConvertingAModule.md).
<!-- R, batch 283: 3362252ca8 (claude/r-trimpath, 0b365e34e8): src/go2cs/positionMapOperations.go,
     recurseTrimpath_test.go, src/core/runtime/managed_impl.cs; CNR 1004 identical; footprint 5567/5567 x3 (std
     unchanged). P2, batch 281: ae69e4ce74 (claude/p2-provenance-header, 93b0093adc), LICENSING.md and
     CommandLine.md; the nugetgo pack turns it on when -LicenseSpdx names Apache-2.0 and refuses a conversion
     without it (ConvertingAModule.md). Footprint 0 x3. The .3 text of -provenance: one line,
     `// Converted from Go source: "…"` (CommandLine.md at nuget-1.26.10.3). -->

**A conversion that loses a file now fails.** When go2cs faults on one file (a bug in go2cs itself), it recovers,
skips that file and converts the rest. The run used to print a warning and exit 0, so a conversion could lose a file,
with its functions, methods, `init` and interface records, and still read as success. Each skipped file is now
reported as `GO2CS2008`, every other file is still written, and the run ends with an error naming every skipped
file. This covers a hand-owned file's `.cs.auto` review sibling too. `-warn` does not accept `GO2CS2008`, since a
missing file is not loud. If you see it, report it as a go2cs issue naming the file; `-debug` re-raises the fault
with its stack. See [diagnostic codes](DiagnosticCodes.md).
<!-- C2, batch 295: a2bcf97a69 (claude/c2-dropped-file-exit, 068cdf1091): both recover sites (the per-file loop and
     emitAutoConversionSiblings); DiagnosticCodes.md GO2CS2008. Footprint 0 on 4 targets; CNR identical; -tests
     convert-only census over 242 packages, 0 dropped. One footprint run had dropped 61 files with rc 0 (the commit). -->

**The Go implementation of TypeScript's compiler now converts, compiles and type-checks.** go2cs uses Microsoft's
TypeScript compiler in Go (microsoft/TypeScript's `tsc` module, at a fixed commit) as a large real-world test, and
each shape it found that did not convert or did not compile is fixed in this release. In go2cs's readiness runs on
Windows, every package that TypeScript's parser, binder, type-checker and emitter need compiles with no hand edit
to the converted C#, and the converted parser's output matches native Go's on every file of TypeScript's parser
test corpus. Measured on Linux x64 (JIT, Release build) at this release's tree, the converted compiler's output
matches native Go's on all 17,422 files of TypeScript's parser test corpus, and type-checking and emitting each of
TypeScript's 7,404 single-file compiler tests, one at a time, matches native Go's output on every one. Five of its
language-server packages are still refused (`GO2CS2005`) and skipped, and xxh3 is used through the fork described
below.
The converted compiler is published, with its build and run instructions, in
[typescript-go-cs](https://github.com/ritchiecarroll/typescript-go-cs).
<!-- Was PENDING on the publish (COORD 2026-11-19 14:51Z); filled 2026-11-20 once typescript-go-cs was public
     (main 6ba3f6b7ce). -->

Among the shapes that now convert and run as in Go:

- a module that imports a path the standard library also vendors (golang.org/x/text, x/net, x/crypto and
  x/sys packages) now uses the module's package, not the standard library's private copy;
- a variadic method promoted from an embedded interface (a method value of it did not compile, and reflection
  counted one method too few);
- a method promoted through several levels of embedding, where a nearer level redeclares it;
- an interface that embeds two interfaces declaring the same method;
- a package that declares a type and a method of the same name (`type Kind` beside `func (t Token) Kind() Kind`),
  used from another package, including through an alias;
- a named string type used with `cmp.Ordered`, ranged over or converted from `[]byte` and `[]rune`, and a named
  `[]byte` satisfying `~[]byte | ~string`;
- named pointer types (`type P *T`): indexing, `len`, `range` and field access through them;
- `x == nil` on a type parameter constrained by pointer types (`[T *Node | *Symbol]`);
- a type parameter whose core type is `[]E` or `map[K]V`, returned or assigned as that type;
- a pointer type argument to a generic whose method-set constraint does not mention itself (`parse[O optionParser]`
  called with `&compilerOptionsParser{}`);
- two instantiations of another package's generic type converted to one interface;
- a pointer parameter re-pointed in a loop header, `for n = n.Parent; n != nil; n = n.Parent`;
- a `switch` on an array value with composite-literal cases, and a parenthesized type-switch case `case (*Node):`;
- `&^` with a constant on a `uintptr` alias, a `:=`-bound variadic func literal used as a value, a value passed to
  another package's anonymous interface parameter, and a package function and a method of the same name;

and a slice of pointers passed beside a callback to a generic function over an interface constraint
(`GetErrorBaseline[T Diagnostic](…, diagnostics []T, compare func(a, b T) int)` called with a `[]*T`), which did not
compile;

and others. Two of these answered wrong without any error before: the promoted variadic method's reflection count,
and a loop header that re-pointed a pointer parameter, whose condition read a stale value. The standard library
contains almost none of these shapes, so its converted code is unchanged by all but a handful of lines.
<!-- The TS readiness record docs/phase4/EXPERIMENT-ts-readiness-2026-11-07.md (catalogue C1-C29) and its data:
     rerun-preview-284p1-readings.txt (S4: closure rc 0, 0 errors, parse 26/26, bind 26/26, type-check 54/54, program
     and emit 63/63, "harness 68 of 68", "No hand-patch to any converted C#"), rerun-runahead-readings.txt (T5: 17,422
     of 17,422 parse units after C46; the full type-check pass 7,374 of 7,404 with the C45 xxh3 and C47 runtime
     patches; the 30 misses: C48 24, C49 5, C51 1), rerun-c48-c51-readings.txt (C48's 24 units match; C51 is
     ~110x per-call, correct output). All are Windows (i7) DIAGNOSTIC readings, not gates. TypeScript commit
     ba29a56299. 2026-11-16 (P4, through batch 310): the measured sentence is P4's emit gate at master 2c1ce10b25
     (batch 310), linux-x64, JIT, Release, go1.26.11, COORD ACCEPTED 2026-11-16 13:59Z (predictions posted 07:13Z,
     4/4 MET): parse 17,422/17,422, type-check 7,404/7,404, emit 7,404/7,404, the four demos byte-identical with Go,
     the same five packages refused. The earlier sentence ("matched native Go's output on all but a few dozen ...
     before fixes") was the Windows readiness reading below; the refusal code was GO2CS2002 before R's renumbering
     (the GO2CS2005 section above). The five refused packages: internal/ls/lsconv, internal/ls/autoimport, internal/ls,
     internal/project, internal/fourslash. Merges: C1 2663620296, C3 b5e99e005e, C4 f3ce8a0fc9, C5 ef137b6ae8,
     C6/C12 24973ed469 + 00bc75369f, C8 c021114195, C9 d0f9d63ca1, C10 248966d5e8, C11 240a4dea0e, C15 b6a3bc13ff,
     C16 c5a21d17c6, C17 60b13275fb, C18 d345469a7d, C19 e0468a5309, C22 eeecc94d3b, C22e d613a0c234, C24
     ee440ccf74, C28 7dcbfb74b4, C29 5b86e76ccb, C30 2563801e0f, C31 52cde97548 + 48f71f237c, C32 0804004099, C33
     15f1b979b2, C35 d770d97945, C37 b5ffdb166a, C38 8d19228a0c, C39 e240c5e815, C40 ad3fd922bf, C41 85c829ef0e,
     C42 08e00f6f9f. Std footprints: C28 6 files per target, C42 two files, C39 one line, C17 28 forwarders, the
     rest 0. C34, C43, C46 and C48 have their own sections. -->

**Code that ccgo translated from C converts further.** modernc.org/sqlite and modernc.org/libc are C libraries
translated to Go by ccgo, and converting them through go2cs found Go shapes that are rare in hand-written code but
everywhere in translated C. At this release's tree, modernc.org/sqlite v1.62.0 and every package it imports convert
with no package refused, on Windows x64 and on Linux x64. A converted program that creates a database file, writes
and reads it back in SQLite's default journal mode and then in WAL mode, and removes it, prints exactly what the
native Go program prints, on Windows x64 and on Linux x64. On Windows the conversion needs one extra value,
`-inline-arrays=modernc.org/libc#largeInteger`, without which the first database opened in WAL mode panics
([Known issues](KnownIssues.md#converting-modernc-sqlite-for-windows-needs-a-seed-for-libcs-largeinteger)). The
converted C# of both programs, for Windows x64 and Linux x64, is published with the recipe that reproduces it in
[sqlite-cs](https://github.com/ritchiecarroll/sqlite-cs). These shapes now convert and run as in Go:
<!-- Was PENDING on the publish (COORD 2026-11-19 14:51Z); filled 2026-11-20 once sqlite-cs carried the converted C#
     (main 175ef95f31: the 1.26.11.1 release converter, -recurse=nuget, GO2CSPATH empty; both demos byte-identical to
     native Go on windows/amd64 (i9) and linux/amd64 (P5)). -->

- a C function pointer carried as a `uintptr`: `__ccgo_fp(f)` and a call back through
  `*(*func(…))(unsafe.Pointer(&struct{ uintptr }{x}))`. Each top-level function gets one number, the same at every
  site, so C code that compares function pointers works, and a number that is not a function panics by name. These
  shapes used to stop the conversion or panic. See
  [Pointers](ConversionStrategies-Reference/pointers.md#a-func-value-carried-as-a-number--one-word-per-static-function-through-gofunccookie);
- a store through a cast pointer, `(*(*T)(p)).F = v` and `(*(*[N]T)(p))[i] = v`, which writes the memory `p`
  points at;
- every spelling of a slice over a pointer cast, `(*[1 << 30]T)(unsafe.Pointer(p))[:n:n]`, which views that memory
  instead of panicking, with Go's capacity for a three-index slice;
- `copy` into a struct that holds a fixed array, through a byte view of it
  (`copy((*[unsafe.Sizeof(d)]byte)(unsafe.Pointer(&d))[:], data)`, Go's record-decoding idiom), which panicked
  before; a struct holding a pointer, string, slice, map, channel, func or interface field is refused by name;
- a pointer conversion of a pointer conversion, `(*int8)(unsafe.Pointer((*int32)(p)))`;
- type aliases as C translations write them: an alias named like a C# keyword, an alias of an anonymous struct used
  across files and packages, an alias chain written in a file before the struct it ends at, and a declared type with
  a C name such as `float`, which keeps its own name;
- a defined type over a defined slice type (`type fermat nat` with `type nat []uint64`);
- a terminating `switch` whose guarded `default` falls through, a multiple assignment whose target is `*(*T)(x)`,
  and an integer converted to a named string type;
- a member that its package renames to avoid a name collision, reached through an import alias: `time` declares
  `Location` as `ΔLocation`, because `Time` has a method of that name, and libc imports `time` as `gotime`. The same
  holds for such a type reached only through another package's alias (`unix.Signal`, an alias of `syscall.Signal`,
  in a package that does not import `syscall`), for a pointer method called on a renamed package variable, and for
  another package taking that variable's address;
- the address of an element of an array whose element type is an alias of an alias of an anonymous struct that
  another package declares (`&X_iob[i]` over libc's `FILE`);
- an anonymous struct type declared inside a function and reinterpreted through `unsafe.Pointer`, and one
  declaration that names several variables of such a type;
- a pointer method called on a named result (`r.SetBigInt(bi)` in modernc.org/mathutil), which did not compile,
  and a pointer method called on a field of a named result, which compiled but wrote into a copy, so the write was lost;
- importing a converted package whose position map is one line longer than 64 KB, as modernc.org/sqlite's `lib`
  package's is. Its importers read none of its type aliases;
- an anonymous struct that another package declares identically but spells through its own aliases, as sqlite's
  `TLARGE_INTEGER` (fields of `DWORD` and `LONG`) does libc's `largeInteger` union, which now takes the other
  package's type where both lay it out alike, so a C function pointer minted in one package is read through the
  other's func type. On Windows sqlite lays `TLARGE_INTEGER` out at Go's bytes and libc does not, which is what the
  `-inline-arrays` value above settles;
- a struct that another package also declares, held as a field by a struct this package keeps (libc's `Titimerval`
  on Linux), which did not compile;
- an array-pointer view whose element is a ccgo union, such as `struct{ Fi [0]int32; Ff float32 }` (sqlite's `TExpr`
  and `TRtreeCoord`, libc's socket-address union), which had length 0 over native memory;
- a conversion between two struct types that both belong to other packages, `lib.Pitch(o)` with `o` an
  `other.Pitch`, which did not compile. go2cs writes one small conversion function for such a pair in the package
  that converts, since neither type's own assembly can hold it
  ([Interfaces](ConversionStrategies-Reference/interfaces.md#and-a-struct-pair-of-two-foreign-types-is-served-by-a-function-which-needs-no-host));
- one instantiation of a generic type used as a non-generic interface whose method names the type argument
  (`box[point]` as `interface{ val() point }`);
- a struct value moving between a named struct type and an unnamed struct type with the same fields, by an explicit
  conversion, by assignment or as an argument, and compared with `==` and `!=`, including a named type from another
  package. An unnamed struct type written twice in one file is one Go type, and a value now moves between the two
  places as Go allows;
- an anonymous struct declared inside a function as the element of an array-pointer conversion,
  `(*[N]struct{ … })(unsafe.Pointer(p))[i]`, which stopped the conversion;
- a string's bytes read through `reflect.StringHeader`, whose `Data` is a number: `unsafe.Pointer(h.Data)` now
  points at the string's bytes, as sqlite's table of C string constants (`__ccgo_ts`) needs;

and others. Three uses of that last shape are not converted yet: the instantiation as a type argument the interface
constrains, a required method promoted from an embedded field, and a second instantiation of the same type behind
the same interface in one package. Each is named by the new warning `GO2CS2007`, and the C# build then fails at that
site.

**A write through the address of a field reached through a pointer conversion reaches the field.**
`&(*T)(p).f` is the address of field `f` in the memory `p` points at, and `&*p` is `p` itself, after Go's nil check.
Both used to box a copy, so every write through the result was lost with no error, and the result never compared
equal to the field's address or to `p`. sqlite's mutexes record their owner through
`atomic.StoreInt32(&(*mutex)(unsafe.Pointer(m)).id, id)`, and libc's `fmod` and `remquo` shift the bits of a double
through `&*`, so both lost their writes. An increment or decrement through a conversion, `*(*int32)(x)++`, now
compiles. In the standard library one line changes, in `runtime`'s heap growth (`&(*arenaList)`). See
[Pointers](ConversionStrategies-Reference/pointers.md#the-address-of-a-field-reached-through-a-pointer-conversion-is-the-fields-own-address).

**Parentheses around the operand of `&` change nothing.** `&((*T)(p).f)` is the same address as `&(*T)(p).f`, and
`&(x)` the same as `&x`, as in Go. With the parentheses, go2cs took the address of a copy, so every write through it
was lost. libc's recursive pthread mutex records its owner that way, and on Linux sqlite stopped forever the first
time it opened a database, waiting on a lock it already held. In the standard library one line changes per target,
in `runtime`'s heap. See
[Pointers](ConversionStrategies-Reference/pointers.md#parentheses-around-an-address-of-operand-change-nothing).

**`sync.Mutex` is Go's eight bytes.** C code translated to Go keeps mutexes in memory it allocates itself: sqlite's
mutex is a struct holding a `sync.Mutex`, allocated with libc's `calloc` and reached through `unsafe.Pointer`. go2cs's
`sync.Mutex` held a managed reference, so reading one out of that memory was refused by name. It now holds only two
32-bit words, as Go's does, and works the same in the managed heap, on the stack and in C memory. An idle mutex is
eight zero bytes, so a copy of an idle mutex is a free mutex of its own, and contended locking no longer allocates.
One difference remains: a mutex copied while a goroutine is waiting on it can wake that goroutine through the copy.
Go's `vet` reports every copy of a mutex after first use.

**golang.org/x/sys/cpu reports the real CPU on amd64.** x/sys/cpu reads the processor's features with two assembly
functions, `cpuid` and `xgetbv`, from its `init`. They converted to methods that throw, so a converted program that
imported x/sys/cpu (modernc.org/libc does on windows and darwin) stopped before `main`. go2cs now forwards both to
managed bodies in golib: `cpuid` asks the processor through .NET, and `xgetbv`, which .NET does not expose, is
answered from what .NET reports about SSE, AVX and AVX-512. Measured on Windows x64, a converted module that imports
x/sys/cpu reports every one of its 29 feature flags as Go does. The forward applies to x/sys/cpu as a module
dependency; the standard library's private copy is not used by converted code. x/sys/cpu's arm64 feature reads are
not covered: they keep their throwing stubs. If a future x/sys/cpu declares either function with another signature,
go2cs refuses the package by name (`GO2CS2011`) rather than call a body that answers another question.

**go2cs names every function it cannot give a body.** A function in a third-party module that has no Go body
(written in assembly, or pulled from another package with `//go:linkname`) and that go2cs does not forward still
converts to a method that throws when called, and the conversion now names each one as warning `GO2CS2012`. A
program that never calls it is unaffected. If yours does, report it as a go2cs issue naming the function. See
[diagnostic codes](DiagnosticCodes.md).
<!-- sqlite-cs rows S1-S23 and the design docs/phase4/DESIGN-sqlite-cs-funcptr.md (S1, S21). Merges: S1 159f08e523
     (G; BOARD 2026-11-13 "a func value carried as a number takes one word per static function": r1/r2 stopped the
     transpile with "Unresolved dynamic struct type: struct{uintptr}", m1/m2/w1 panicked, w2 was silent; std 1 line,
     reflect funcName), S17/S20/S5 975aa3ac83, S13/S14/S6 adfcea58f6, S7/S4 811f3fdb3c, S18 a7e363527f, 3b 65404e5e63
     (P2); M1-M4 38d917d0be, S23/M5 2402236781 (P1); the closed carrier and GO2CS2007 3da19d0e2e (G; BOARD 2026-11-12
     "one instantiation of a generic type behind a non-generic interface now compiles and runs"); the reinterpret
     seam 71a08e8f9b (C1, docs/phase4/DESIGN-reinterpret-seam.md). S21: 839fd3634b (G; BOARD 2026-11-13
     "x/sys/cpu's cpuid and xgetbv take golib bodies by name": Windows x64, x/sys/cpu v0.49.0, exit 2 at master,
     29 flags equal Go's at the fix; gated on packages outside GOROOT; arm64 getisar0/getisar1/getpfr0/getzfr0 kept
     as stubs; darwin and linux conversions not run). GO2CS2012's reach on sqlite-cs: 8 stubs, none reached (1f2d0748ba).
     The sqlite-cs demo itself is NOT REACHED past start-up (BOARD S1 row, Q4: it stops at row S22).
     306-309 (P4, 2026-11-16, bringing the draft through batch 309): W2 6992717d09 (G; readPackageInfoLines' scanner
     stopped at its 64 KB default on lib's 123,448-byte GoPositionMap line; footprint 0/0 on 4 targets), W4/S10
     c09d5e04da (P2; CS0426 x5 fixture / x8 repro; footprint 0 x4), W5a 39b40c8c3a (G; CS0426 x4; footprint 0/0 on 4
     targets), S8c b62c67b6d8 (P2; CS1929 x6, and the silent write into a copy box on master; footprint 0 on 4 targets),
     L1 4c101b8e57 (G; 4 unresolved dynamic types at master; footprint 0 on 4 targets).
     310 (P4, 2026-11-16, through batch 310 at master 2c1ce10b25): the convert sentence is COORD's 310 landing post
     (20261116T070711Z): "a sqlite-cs convert-only leg, now in every battery, PASSES on windows + linux (W17 + W7
     cleared master's refusal)". Running the demo is NOT claimed: G's runs of record (docs/phase4/DATA-sqlite-cs-demo.md,
     runs A-C, byte-identical with native) each used stand-ins for walls since merged or still open (W9, the mutex,
     rides after 311). Bullets: W7 merge 8c8b697736 (G, ff06d10513: a GoTypeAlias record names the lift its forward
     alias chain reaches, not a marker); W11 merge 67113b4faa (P2, eccfffdcc9; ConversionStrategies-Reference
     golib-namespace.md; footprint 0 x4); W13 merge edd917b022 (G, 85df98bc0d + acab661c3c); W12 merge 3d95f43749
     (P2; struct-types.md; three neighbour shapes OPEN and loud, census reach 0, BOARD 2026-11-15 W12 row); W17 merge
     d5cfd11e06 (P5, d2911a658e; master 309 refused the sqlite-cs conversion, rc 1; std census 0 on four targets); W8
     merge 7b213dd6dd (H, fe14d5faf8; the header store with a changed Len is BOARD row 3, .5, which the sqlite path
     reads on none of four targets). The pointer paragraph: W9b merge 290c08b18a (P2; pointers.md's two new sections;
     behavioral AddressOfFieldThroughConversion and its Native twin; std footprint runtime/malloc.cs one line per
     target, BOARD 2026-11-15 W9b row). Left out: C1's linux/arm64 rawSyscall6 r2 (merge 45307c4307), a fix to the
     linux-arm64 build that first ships in this release.
     311 (P4, 2026-11-18, through batch 311 at master f36b6b0d48; ledger 859f1227cb): W18 (C2's W18-A seat, claude/c2-w18-alias-lift-reuse
     bd2a09c423 carries P1's W18 roots): f4e38409887 (an identical struct spelled through local aliases reuses the
     import's lift by the import's own spelling) and fae89006f6b (an attributed public-lift record reads as public;
     libc's largeIntegerᴛ1 carries [GoValueClone]); end-to-end arm red at 7185a1f735 with the func cookie panic.
     W18-A bd2a09c423 (linux-x64: libc's Titimerval reused uuid's Itimervalᴛ1 while its Ttimeval field kept libc's
     own lift, CS1503 x3 in G's closure build at 6bcc05d11f). W19 1152f6b6440 (P5; arrayPointerDoorUnionElement_test.go,
     four arms; std never admitted, unchanged by construction). The demo is still NOT claimed: the windows sqlite
     WAL pass stays blocked until batch 312, because W9a (P2's mutex, the W9 stand-in of G's runs) seats in 312, and
     G's windows WAL reading (W18) has not been retaken on a tree carrying both. The file-backed demo's run claim
     waits for 312's sqlite gates (windows: i9; linux: P4), each byte-identical with native or not claimed; the GUARD
     stands (name Linux only until windows reads MATCH). -->

**github.com/zeebo/xxh3 converts through a copy go2cs carries.** xxh3 reads a string's or a slice's bytes through
its header and at computed offsets, which go2cs cannot yet convert: the converted program stops with a named panic.
The repository now holds xxh3 v1.1.0 with every use of `unsafe` removed, at
`src/forks/github.com/zeebo/xxh3@v1.1.0`; it computes the same hashes bit for bit. A module that depends on xxh3
converts with `replace github.com/zeebo/xxh3 => <go2cs clone>/src/forks/github.com/zeebo/xxh3@v1.1.0` in its
`go.mod`. The general fix is planned. See [Limitations](Limitations.md).
<!-- C1, batch 286: d386315daa (claude/c1-c45-xxh3-fork, 9516d0524e); the fork's FORK.md and identity/check.sh:
     16,632 results byte-identical to the original under Go 1.26.10 on linux/amd64 (AVX2 and AVX-512, so the
     original took its vector paths), with a negative control; xxh3's own tests pass on the fork. The general fix:
     docs/phase4/DESIGN-unsafe-header-views.md on claude/c1-c45-header-views, a post-1.26.11.1 train. -->

**Inside a pointer-receiver method, `unsafe.Pointer` of a local copy of the receiver now points at the value.** In a
method on `*T`, `p := h` followed by `unsafe.Pointer(p)` converted to the address of the local variable `p` itself,
not of what `p` points to. A write through it landed on the local, the value never changed, and the program ran on
with a wrong answer. A field holding a `*T` and a local copied from another local did the same. They now point where
Go points; the receiver itself and a pointer parameter already did. Nothing in the standard library has this shape,
so only your own code changes when you convert it again.
<!-- Merge 269f475408 (claude/p2-fromref-local-box, e3e070dfe8). Behavioral UnsafePointerReceiverCopy: Go
     `7 8 0 9 10 0 11 12 13`, converted at the base `7 0 0 9 10 0 0 12 0`, fixed equal to Go (linux/amd64 box).
     Two-seeded -stdlib census: 0 changed lines on windows/amd64, linux/amd64, darwin/amd64 and linux/arm64. -->

**A nil func of a named func type is one value in an interface.** Such a value read differently depending on how it
reached the interface: asserting it back to its own interface type failed, an assertion to `error` reported
"interface is nil", a panic message named an internal C# class, and `reflect`'s `Call`, `Method` and
`MethodByName` crashed with a .NET `InvalidCastException`. Each now answers as Go does, and `reflect.Value.Call` on a
nil func panics with Go's `call of nil function`.

**A type switch finds a nil func in its case.** `case F:`, with `F` a func type, missed a typed nil func, and the
switch went on to a later case or to `default` without a word. It now matches `F`'s case. The standard library has
seven such labels, in `net/http` and its tests.

**A nil func passed to a named func type stays nil.** Passing a nil func value where a parameter or a struct field
has a named func type threw `Delegate to an instance method cannot have null 'this'` where Go passes nil. The value
now arrives as nil.

**A nil pointer is one nil inside generic code.** A nil `*T` written as the literal `nil` and one that is a zero
value (`*new(T)`) are the same nil in Go. Inside a generic function they were two: `x != *new(T)` was true for a nil
`x`, a map with pointer keys could hold two nil keys, and an array of nil pointers compared unequal to another. They
are one nil now. A typed nil pointer in an interface is still distinct from a nil interface, as in Go.
<!-- N1: merge c78f162712 (d3dc97e466): behavioral NilFuncInterfaceReads died with InvalidCastException at the parent,
     its output equals Go's after; GolibTests NilFuncCarrierRuleTests. N2: merge 9c77232c27 (d4eb94fd60: 7 label lines
     in 4 net/http files, 3 of them test files); behavioral NilFuncTypeSwitch (90 silent lines at base). C47: merge
     847c629b31 (e2247291ee, 8a35b29c4f; behavioral NilFuncNamedDelegateArg; the TS shape core.IfElse(..., nil) to an
     ErrorReporter). C49: merge 0430770b00 (1821f05128: builtin.AreEqual<T>, map isNilKey, array<T> equality;
     behavioral NilPtrIdentity, linux, all 31 lines equal Go from 12 differing). No platform is named for N1, N2 or C47
     in their records. -->

**reflect reads a value behind an interface as the value.** A named slice, map or other non-struct value held in an
interface through one of go2cs's adapters (a value converted to an interface it implements, or to `error`) was read
by `reflect` as the adapter: `Len` and `Cap` gave 0 or panicked, `Kind` reported `ptr`, `%v` and ranging over the
map printed nothing, and the program still exited 0. `sort.Slice` over such a slice panicked with
`call of Swapper on slice Value`. reflect now sees the value itself, and two `reflect.ValueOf` calls over one
interface value still compare equal.

**A float converts to an integer type parameter.** `T(f)` with `f` a `float64` and `T` constrained to an integer type
(`T ~int32`) did not compile after conversion. It now truncates toward zero, as Go does. An out-of-range value or NaN
is left to the implementation by Go; go2cs gives the answer Go's compiler gives on the machine the program runs on,
measured for all 11 integer kinds on amd64 and on arm64 (Apple silicon).

**A generic type whose `Error` method has a pointer receiver converts to `error` every time.** Such a conversion could
fail with "does not implement error.Error", depending on which packages had loaded and in what order. It now finds
the type's own method whatever the order.
<!-- reflect: merge c6fd06311c (6302266f1d): behavioral ReflectShellAccessors red at the parent (exit 2, "call of
     reflect.Value.Cap on slice Value"), green after; census 1028 reads; -tests reflect, fmt, errors, sort,
     encoding/json, encoding/binary, text/template: 0 tests moved. C36: merge c737e7760a (674f1c0bf8: CS1503 before;
     gc's amd64 table measured on go1.26.10 linux/amd64; 9413fb2f16: arm64 measured with go1.26.10 darwin/arm64 on a
     hosted osx-arm64 runner, run 42857184743; linux/arm64 not measured on a linux runner). error<T>: merge d9d52c0abd
     (df08b14978), docs/phase4/DATA-startup-method-sets-a2.md, "The difference" (GolibTests ErrorBoxLookupTests,
     both scan orders). -->

**A deadlock report shows every goroutine, as Go's does.** After `fatal error: all goroutines are asleep -
deadlock!` Go prints one block per goroutine: its wait reason, its frames, and the `go` statement that started it.
A converted program printed the line alone, and when several goroutines saw the deadlock at once it could print it
more than once. It now prints Go's blocks, once, and exits with status 2. The report still appears only when every
goroutine waits on a nil channel or an empty `select {}`; any other deadlock hangs
([Limitations](Limitations.md#what-converts-but-doesnt-behave-exactly-like-go)).

**Every fatal report reaches `debug.SetCrashOutput`.** Go copies whatever it prints while dying to the file set by
`runtime/debug.SetCrashOutput`. A converted program did so for a Go panic, but not when a .NET exception escaped a
goroutine, nor for the deadlock report: those went to stderr alone. They now reach the crash file too.
<!-- Deadlock: merge 01986cdd81 (4078bdc81b): red first at master 3da19d0e2e (the line alone; the three-goroutine case
     twice); behavioral DeadlockGoroutineBlocks; golib + hand-owned runtime only, 0 corpus lines; no platform named.
     Limitations.md:135-138#ab9ea8 and GoFeatureSupport.md:117#21160f already state it. Crash output: merge 456b550a46 (b60eef7b07):
     builtin.WriteFatalReport; GolibTests BackstopCrashOutputTests red with the crash file empty, green, linux. -->

**A profiled program no longer hangs.** With the block or mutex profiler on, the first event on a stack the profiler
had not seen could deadlock the thread that recorded it: building the profiler's own record was itself sampled by the
memory profiler, which waited on the lock its caller already held. At the default `runtime.MemProfileRate` that was a
rare draw; at `runtime.MemProfileRate = 1` it hung every time. The profiler's own records are now made outside the
memory profiler, as Go makes them.

**`runtime.GOARCH` is `arm64` on Apple silicon.** It reported `amd64` on every Mac, because the macOS packages are
built once, for `darwin/amd64`. It now reads the architecture of the running process, and the `GOARCH` setting that
`debug.ReadBuildInfo` returns and `go/build`'s default context follow it. On an Intel Mac and on `win-x64` it still
reads `amd64`. Everything decided by architecture when the packages are built still follows `amd64`
([Known issues](KnownIssues.md#on-apple-silicon-runtimegoarch-reports-amd64)). The tests of `debug/buildinfo`,
`net/http/pprof`, `go/build` and `go/internal/gcimporter` now match Go's on Apple silicon.

**`runtime.FuncForPC` returns one `*Func` per function.** It returned a different `*Func` for each call site of a
function, and not the one `runtime.Frames` reports for the same frame. Now `runtime.FuncForPC(pc) == frame.Func` holds,
two call sites of one function give the same `*Func`, and its `Entry()` is the function's, as in Go. Measured on
windows, linux and macOS.

**`os.Args` under `dotnet prog.dll` on Windows.** Started as `dotnet prog.dll a b`, a converted program read
`os.Args` as `[dotnet prog.dll a b]`, so every program that reads `os.Args[1:]` took the DLL as its first argument.
It now reads `[prog.dll a b]`, the shape the same launch gives on linux. Started through its own executable, the
program was already right, and it is still the launch to use for a program that runs itself again with
`os.Args[0]`.

**A zero-byte UDP datagram reads on macOS.** Reading a zero-length datagram could fail with
`address family not supported by protocol family`, where Go returns a zero source address. Measured on Apple silicon
and fixed for macOS and linux.

**Timers on macOS fire on time.** On the hosted macOS runners a 5 ms timer fired 9 to 13 ms late at the median, where
Go's own timers on the same machines are late by 0.7 to 1.3 ms. go2cs's timer service now waits as Go's runtime does
on macOS, and fires within the 1.5 ms its test allows, on Apple silicon and on Intel. This serves `time.Sleep`,
timers, tickers and I/O deadlines.

**CPU profiles on macOS name the program's own code.** The CPU profiler read each thread's identity and CPU time
through a .NET API that gives wrong answers on macOS, so nearly every sample was charged to `_ExternalCode`. It now
reads them from the kernel, as the samples themselves are keyed. One .NET limit remains on macOS: in a loop that
makes no calls, most samples are charged to the loop's caller.

**A nine-argument libc call on Apple silicon passes its ninth argument where Go 1.26 does.** go2cs still placed it
where Go 1.24 did. `internal/syscall/unix`'s `getnameinfo` is such a call.

**`unsafe.Sizeof` answers Go's size or refuses by name.** For a type golib cannot size the Go way, `unsafe.Sizeof`
fell back to .NET's marshalling size, which is not Go's: one `dnsmessage` type measured 48 bytes where Go says 32. It
now panics, naming the type. Every type the standard library passes to it is sized the Go way, so converted
standard-library code is unchanged, and a Native AOT program no longer carries a throwing marshalling stub for each
type that reaches it.
<!-- Batch 311, i9's Sizeof fallback (claude/i9-sizeof-fallback 70befb6059): unsafe 01498f4afc4. The message:
     "runtime error: unsafe.Sizeof: the Go size of <T> cannot be derived (GoReflect.TryGoSizeOf declined it)".
     dnsmessage's SVCParam marshalled as 48 where Go says 32; a struct with a [1<<62]int64 field as 16. Every T the
     windows corpus passes to Sizeof<T> answers through TryGoSizeOf (15 types probed under JIT, plus 3 TypeScript
     boxes). -->

**In a converted test, `t.Deadline()` reports the deadline that ends the test.** go2cs's test runner can end a
single test that runs past a per-test limit, which Go does not have, and `t.Deadline()` reported only the deadline
of the whole package run. A test that sized a wait from it could wait past the limit that ended it, and the run
reported it abandoned instead of its own result. `t.Deadline()` now reports the earlier of the two.
<!-- Batch 312, P4's T.Deadline seat (from C2; claude/p4-tdeadline 99bb4bc5bd; fix 0417718b99, COORD ruling (A)
     2026-11-16). internal/poll's TestSplicePipePool, 45m budget and 30m per-test floor: abandoned at 1802.75 s
     before (hosted A-7); after, it fails with its disclosed "leaked descriptors" signature at 1620.06 s, not
     abandoned (hosted run 1759). GolibTests TestDeadlinePerTestTests 6/6. -->
<!-- Profiler: merge c64f8404c8 (ca23b88d6e: "at MemProfileRate = 1 the first block event on a new stack hangs every
     time"; GolibTests ProfilerOwnAllocationTests, child-process bound; found as an 84-minute hang of GolibTests Debug in
     batch 283's battery; no platform named). GOARCH: merge 26186c1739 (28cfcf657e darwin, 030725e30e windows: Windows on
     Arm UNMEASURED, no package ships for it; osx-arm64 debug/buildinfo 211, net/http/pprof 15, go/build 57, gcimporter
     634 validate per the merge subject); KnownIssues.md:38-56#b91ee1 already describes the behaviour; NEWS 1.26.10.3 (line
     229) called it planned work. FuncForPC: merge 40b021f8c4 (b20b20c5db; rows read as master on linux, darwin both
     RIDs and windows). os.Args: merge 813f55b5e8 (6a87a161f4: GolibTests WindowsCommandLineTests, windows/amd64; the
     self-re-exec limitation is disclosed in the hand-own's header). recvfrom: merge 872df69010 (9d31496d45:
     TestUDPZeroByteBuffer on osx-arm64; RecvfromUnspecifiedSourceTests 3/3 on both macs and on linux). macOS timers,
     CPU sampler and the nine-argument call: merge fb885891b3 (5c37f97453: median lateness 12.9 / 8.9 ms against a
     1.5 ms bar, Go's AfterFunc 1.34 / 0.70 ms, run 42595674585; 878610cb62: 17503 of 17703 samples to _ExternalCode;
     b1270b96dc: VariadicStackSlot arity 9; ruling (i) on the hog-as-leaf floor); darwin run 42617443400, 0 failed on
     both RIDs. -->

**On Linux, `runtime.SetDefaultGOMAXPROCS` no longer throws.** Three runtime routines that Go writes only in
assembly, `sched_getaffinity`, `osyield` and `exit`, converted to methods that threw `NotImplementedException` on
linux-x64 and linux-arm64, and `SetDefaultGOMAXPROCS` always reaches the first. They now have bodies: the raw
`sched_getaffinity` system call, libc's `sched_yield`, and the `exit_group` system call. A known limit remains:
`SetDefaultGOMAXPROCS` returns without resetting the setting, so a converted program keeps the GOMAXPROCS it had,
where Go goes back to its default. Measured on linux/amd64.
<!-- merge fe454ae807 (G, claude/g-la64-r3-ledger, carrying g-linux-stub-bodies 0282fe03f4). Bodies:
     src/core/runtime/linux/stubs2_impl.cs (456c49f81f; measured under WSL, linux/amd64, SDK 10.0.403: 5 of 11 tests
     failed with the stub's NotImplementedException before, 12 of 12 pass after; the arm64 file is the hosted leg's).
     The remaining no-op: BOARD 2026-11-11, "runtime.SetDefaultGOMAXPROCS is a SILENT NO-OP on a public API", OPEN
     (34225f5b97). If the owner would rather not announce a fix that leaves a known no-op, drop this section. -->

**`sync.Mutex` takes a free lock at once.** An uncontended `Lock` and `Unlock` pair took 321.7 ns and allocated 64
bytes; it now takes 66.4 ns and allocates nothing. With four goroutines contending, a pair went from 2,191 ns to 105
ns. Measured on linux-x64. One difference from Go is now easier to see: a goroutine that unlocks and at once locks
again gets the lock back more often than a waiting goroutine does, because go2cs's mutex has no starvation mode,
Go's bound on that.

**Comparing pointers to struct fields is cheap.** `p == q`, for two pointers to a field reached through several
embedded structs, cost time that grew with the depth: at five levels an unequal compare took 717 ns, against Go's
3.4. It now takes about 6 ns at one level and at five (linux-x64). The same change fixes a rare wrong answer: pointers into two
different values could compare equal when their internal allocation numbers collided.

**Less copying and boxing.** A generated struct now implements `IEquatable<T>`, so a struct used as a map key is
compared without boxing, and its hash code is computed without allocating. A named map or channel compared with
`nil` no longer goes through reflection. `unicode/utf8` and `fmt` no longer copy a string argument on one internal
call each. Together these made the converted TypeScript compiler's checker about 8% faster on its first 25 test
units on linux.

**Faster start-up for programs that convert pointer-held types to `error`.** Finding a type's pointer-receiver
`Error` method read every extension method of every loaded package; it now reads only the type's own. A program that
loads many packages started about 40 to 65 ms faster under the JIT on linux/amd64; a small program sees no change.
<!-- Mutex: merge 6ea085d498 (bcea59d256: linux-x64 JIT, runtime 10.0.14, 4 cores, median of 9 rounds; Windows
     UNMEASURED; fairness: TestMutexFairness median 0.015 -> 0.14 s in context, all passing; BOARD row 5a7903f870, no
     starvation mode). C51: merge 2db66518be, docs/phase4/DATA-c51-field-view-equality.md sections 3 and 4 (716.52 ->
     6.03 ns at 5 hops, 28.73 -> 5.94 at 1 hop; Go 3.42-3.45; "the one intended difference", 29-bit allocation ids;
     FieldViewIdentityTests.FieldsOfDistinctRootsSharingAnIdAreDistinct red at base). S2-S4: merge 08944002b7
     (76b31d027c: HashCodeCombineAllocationTests 128,000 B in 1,000 calls -> 0; the TypeScript checker's first 25 perf
     units median 34.7 s against 37.6 s, 3 alternated runs, identical output, linux; corpus 2 lines on each of four
     targets). A2: merge d9d52c0abd, docs/phase4/DATA-startup-method-sets-a2.md (program "wide", paired medians -42 to
     -66 ms against an A/A control of -9 to +14 ms; hello and stringer within noise; one cloud linux/amd64 VM). -->

**A `switch` on a variable reads as written.** `switch os { … }` used to copy `os` into a temporary named `exprᴛ1`
and test the temporary. When nothing can change the variable between the first case and the last, the converted
`switch` now tests it in place; otherwise the temporary is named after it (`osᴛ1`). A `select` case on a channel
variable names its temporary after the channel (`quitᴛ1`).
<!-- Batch 312, P1's Tour item 3 (claude/p1-vp-switch-temp 672c430fc7; converter 061ae3868d4, switchTagInPlace and
     switchTagTempPrefix; behavioral fixture over one plain identifier 8968472cde8; regen c6bd89f8b4c). -->

**Converting for another platform reads that platform's Go settings.** With `-platforms` naming a target other than
the converting host, go2cs now selects files gated on Go's tool tags (`goexperiment.*`, `amd64.v1`, `arm64.v8.0`)
for the target, as `go build` does, where it used the host's. Go 1.26 turns the `dwarf5` experiment off for darwin
and on for windows and linux, so a darwin conversion on a windows host used to drop both halves of such a file pair.
In the standard library this adds one file to the darwin build of `internal/goexperiment`.

**A module's `package main` keeps its own `package_info.cs` when converted again.** A second conversion into the same
output used to warn, falsely, that the file belonged to another package and replaced it with the template, dropping
anything kept outside its marker sections.

**A promoted interface method named like a type of its package converts.** When a struct satisfies an interface
only through its pointer, and a method promoted from an embedded interface field shares its name with a type of the
package (method `Driver` beside type `Driver`), the converted method took the type's renamed spelling, `ΔDriver`,
and did not compile. modernc.org/sqlite's tests reach this through `database/sql/driver.Connector`. The method now
keeps its Go name, in its struct's method holder.
<!-- Batch 311, P1's class C forwarder name (claude/p1-promoted-forwarder-name f9b44d1399): converter e9db328467c
     (getSanitizedFunctionName for the forwarder and its call; CS0102 same package, CS1929 against a foreign
     interface; the TS session's class C, 2026-11-16); behavioral PromotedForwarderTypeNameCollision with the TS
     probe's t3 repro as its second arm (2b72404403d). The holder placement: P1's OQ2 x class C fix d61ca26f4f..
     f36b6b0d48 (CS0102 x4 in go2cs.slnx at the union). The GO2CS2006 flow-insensitivity note is a BOARD row only
     (f9b44d1399). -->
<!-- R, batch 281: d914d8b360 (claude/r-tooltags, 14bea07f1e): src/go2cs/directiveOperations.go loaderToolTags per
     target; footprint darwin +1 file, windows and linux 0. P2, batch 282: e85dc60698 (claude/p2-pkginfo-doc-warning):
     BOARD 2026-11-10 "a main package in a module refused its own package_info.cs on every reconvert"; reach 0
     committed files (std has no main package). The guard (packageInfoBelongsHere) and the tool-tag lookup both ship
     in 1.26.10.3. -->

**On Windows, the go2cs tool's first-use Go download no longer fails when another process briefly holds a file.**
The tool unpacks Go into a staging directory and renames it into place, and Windows denies that rename while a
virus scanner or the search indexer still has a file in it open. The install now retries the rename for up to two
seconds, as the go command does for the same errors, and when it still fails, names both directories. Measured on
Windows: the new held-handle test failed before the change and passes after it.
<!-- merge a713a73c06 (P2, claude/p2-toolchain-rename-retry): src/go2cs/toolchainProvision.go renameIntoPlace;
     TestToolchainInstallSurvivesAHandleHeldAfterUnpacking (Windows only), red at bbc0265c07 and green at 0afbc1b282
     on the i7. First seen on the i9 as "Access is denied" from TestToolchainProvisionFromModuleProxy. -->

**On linux and macOS, a `-tests` run handles an interrupt as `go test` does.** Ctrl+C now stops every process the
run started, the converted test host included, and go2cs exits with status 130. Every child starts with SIGINT and
SIGQUIT at their default handling, even when the run itself was started with them ignored (`cmd &`), so a converted
test of signal handling sees what it would see under `go test`. See [`-tests`](CommandLine.md).
<!-- Merge 58648c8d89 (claude/g-harness-sigint; red ec4605661e, green 2b491dc713); CommandLine.md's -test-action row.
     The os row run under an inherited ignore read DIVERGED at base and PASS 3831/2 at the fix; a normal run's
     comparison.json is byte-identical. -->

**A short-lived converted program starts faster with one project setting.** Since 1.26.10.2 a converted program
optimizes its hot code from its first moment. That keeps a long-running program fast, and costs a short-lived one
part of its start. On `win-x64` (a 6-core laptop), published programs started 30 to 50 ms sooner with .NET's own
delay, and 100 to 170 ms sooner when pinned to one CPU, about a quarter of a small program's run. The same change
made a loop of TLS handshakes 41% slower, and 1.9 times slower on one CPU. So for a short-lived tool, a CLI, or a
container with one or two CPUs, set `<GoCallCountingDelayMs>runtime</GoCallCountingDelayMs>` in the converted project;
for a service, a server or a benchmark, keep the default. The setting applies under the JIT and ReadyToRun, not
Native AOT. The default does not change. See
[start-up or steady state](Tuning.md#start-up-or-steady-state-the-call-counting-delay).

**Server garbage collection is a documented option for an allocation-heavy, long-running program.** On `linux-x64`
(a 4-vCPU cloud VM), a large converted program, the TypeScript type checker, took about a third less time over a
slice of its compiler tests with server GC on and DATAS off, and used about 15% more peak memory. The memory cost
grows with the core count, so measure on the machine you deploy to. The settings take effect only in an executable.
Converted projects keep .NET's default, workstation GC. See [.NET runtime settings](Tuning.md#net-runtime-settings).
<!-- docs/phase4/DATA-call-counting-delay.md (G-LAPTOP, AMD Ryzen 5 PRO 6650U, 6 cores / 12 threads, .NET 10.0.11;
     tables 3.2 and 3.3: PerfStartup / PerfMidStart published, PerfTlsHandshake JIT +41% and x1.90), merge 6996aafb79;
     docs/phase4/SCOUT-ts-perf.md section 4 and docs/phase4/data/ts-perf/experiments.tsv (35.83 s -> 24.37 s, 1,252 ->
     1,447 MB peak RSS), merges fd2b6f5331 and c864b69655; owner ruling 2026-11-13 "an option, not the
     converted-project default". "Since 1.26.10.2": Tuning.md at nuget-1.26.10.3 attributed most of the 1.26.10.1 ->
     1.26.10.2 JIT speed-up to the delay of 0. -->

**A Native AOT publish is smaller and lighter to build, and the README walkthrough's program now publishes.**
golib's pointer box named a type that named the pointer box again, one level deeper each time, and Native AOT
expanded that cycle until it reached its nesting limit, compiling every method past the limit as one that always
throws. The cycle is gone. A program that uses only golib went from 654 such cut-offs to none and from a 19.7 MB
executable to 11.2 MB. A larger benchmark program's executable shrank by about a quarter, and the AOT compiler's
peak memory by a third or more (measured on `win-x64`). With the 1.26.10.3 packages the README walkthrough's
program, which uses github.com/fatih/color, did not publish with Native AOT on `linux-x64`: the compiler stopped at
the native string table's 2 GiB ceiling. Now it publishes, and its string table is 250 MiB. A program that
`go2cs -recurse` converted now publishes with `dotnet publish -r <rid> -p:PublishAot=true`, run in the program's
own folder or naming the program's project file: the converted libraries are compiled into the program and are not
published on their own. Through 1.26.10.3 that command failed on the converted analyzer projects, and a folder
converted with `-recurse=nuget` published each library on its own. See
[Tuning](Tuning.md#choosing-a-deployment). A program under the JIT runs as before, with one benchmark 2.7% slower.

**Tuning has a new section on Native AOT build time.** One pass of the AOT compiler, which folds identical method
bodies, takes most of its time on go2cs programs. Turning it off with `IlcFoldIdenticalMethodBodies=false` built
one benchmark program 4.5 times faster and a large program in 7 minutes instead of 32, for an executable 11% to 15%
larger. go2cs keeps the .NET default. See
[Native AOT method-body folding](Tuning.md#native-aot-method-body-folding).
<!-- Lever G: merge 055f4e6230 (i9, claude/i9-golib-elemview-cycle c08c2bba8c): golib's ж<T> no longer names
     ElemRefBox<T> in its view lookup (an internal virtual the element box overrides); the cycle had returned with
     4ad8e8df66 (2026-10-21), so 1.26.10.3 carries it. docs/phase4/DATA-ts-aot-b4.md section 15 (win-x64): the
     golib-only consumer 654 IL3054 lines -> 0, image 19,697,664 -> 11,214,848 B; PerfStartup image 120.5 -> 87.6 MB
     (-27.3%), ILC peak private 6.54 -> 3.84 GB (-41.3%); parsedemo image -26.2%, ILC peak -33.8%. JIT cost: +2.7%
     PerfFieldView (the merge message). The walkthrough program: P1's AOT rows 1-3, 2026-11-16, linux-x64 (mailbox
     20261116T041238Z-P1.md): at master colordemo publishes with the property in its csproj, .strtab 262,013,197
     bytes, 0 'will always throw'; at 1.26.10.3 ILC fails at StringTableBuilder.CreateIndex with 6,188 such lines,
     on both routes. P3's runs A and B, 2026-11-15, read the same failure. The command-line property: P1's seat
     E1+E2 (proposed for batch 311) makes it work; reword the sentence if that seat lands in this release.
     2026-11-18 (P4, HOOK 311): E1+E2 landed in 311 (claude/p1-aot-publish b6326e52b3; af20562ff29, f358d950693);
     reworded from docs/Tuning.md's table and its provenance note: NETSDK1207 on the netstandard2.0 analyzer projects
     (go2cs-gen, go2cs-arrays-gen) on a local -recurse; a -recurse=nuget folder published each library as its own .so;
     the analyzer projects keep PublishAot false (repoguard TestAnalyzerProjectsKeepPublishAotOff) and the -recurse
     root's Directory.Build.targets sets IsPublishable false on a Library.
     Folding: merge d39b1ffe00 (P4); Tuning.md's table (win-x64: PerfStartup 6 min 29 s -> 1 min 27 s, parsedemo
     32 min 20 s -> 7 min 6 s; executables +15% and +11%); the owner's ruling keeps the SDK default. -->

**A new page, [Go feature support](GoFeatureSupport.md), says feature by feature what converts.** Each Go feature is
marked supported, supported with limits, or not supported yet, grouped by area. Each row links to how the feature
maps to C#, to a working example from the behavioral tests, or to the page that describes its limit. A pass over the
user-facing pages fixed what an audit found wrong or out of date, across the README, Limitations, Known issues, the
Roadmap and Consuming Go from C#, and [Command line](CommandLine.md) now lists every flag `go2cs -h` prints. To mark a
repository or a package page as converted by go2cs, use the badge served from go2cs.net
([how](ConvertingAModule.md#publishing-your-conversion-the-package-page)). The nugetgo pack now puts it first in the
block it writes into a package's README. The repository also has a code of conduct (the Contributor Covenant 2.1) and
a pull-request template. [Validated modules](ValidatedModules.md) is refreshed: nine Go modules whose own test
suites go2cs converts and runs, each read with Go 1.26.10 on the 1.26.10.3 packages, and the README links it
beside the standard-library roster.
<!-- merges 48665b310f and e02f6729d0 (docs/GoFeatureSupport.md, linked from docs/README.md and docs/Features.md);
     bce71f3ae9 (P2, the user-facing docs audit; the guard TestCommandLineDocListsEveryFlag); 6869ec2b54 and
     8792630525 (docs/images/go2cs-badge.svg, ConvertingAModule.md, src/tools/nugetgo/NugetgoReadme.psm1);
     c978595f0c (R: CODE_OF_CONDUCT.md, .github/PULL_REQUEST_TEMPLATE.md). Validated modules: merges 65be87fd89 (P4,
     batch 308; docs/ValidatedModules.md, 9 rows, read with Go 1.26.10 on go.* 1.26.10.3) and 64a5cc7e24 (P4, batch
     309; docs/README.md's Explore list). -->

<!-- HOOK 311 (P4), FILLED 2026-11-18 from the ledger line of batch 311 (859f1227cb; master b2eaf5290a ->
     f36b6b0d48). Each seat and where it went:
     - P1 OQ2 method holder (0f79b486cb): already in this draft ("Every Go method is in its receiver type's method
       holder", under Upgrading from 1.26.10.3; f2fcf670d8 wrote it).
     - P1 AOT E1+E2 (b6326e52b3): the Native AOT paragraph's command-line sentence reworded.
     - P1 CLASS C forwarder name (f9b44d1399) and the OQ2 x class C holder fix (d61ca26f4f..f36b6b0d48): Smaller
       converter fixes.
     - P5 W19 + D4 (988a4cec58): D4 in the array-view section; W19 a sqlite-cs bullet. D4 does not cure the field
       view's open-slice limit, so that sentence stays.
     - C2 W18-A alias lift reuse (bd2a09c423), carrying P1's W18 roots: two sqlite-cs bullets. The windows sqlite WAL
       pass stays blocked until batch 312 (W9a); the demo GUARD stands.
     - R array reassign cut A (99f34a588e): its own section.
     - H native-box reference floor (3ce394cbc0): its own section.
     - i9 Sizeof fallback (70befb6059): Runtime fixes.
     - No news item, with the reason: G DATA-methodset-scout (1752fc07fb, a record); H W8 should-fix (9d3401725c,
       IHeaderDataCarrier made internal, no behaviour change); H pin entries once (0a51ac2399, test diagnostic only);
       P5 alloc loop harden (4fe430f549, a test); R hand-own pointer root (8a21aede04, ten hand-owned files respelled
       to @unsafe.Pointer.To<T>, same behaviour, plus a repoguard rule); P4 AOT docs (e7a4d94f7d, already in this
       draft). -->

<!-- HOOK 312 (P4), FILLED 2026-11-18 from the ledger line of batch 312 (afe47908f0; master f36b6b0d48 ->
     b63418fafc), the last batch before the .4 freeze. Each seat and where it went:
     - The sqlite demo GUARD is LIFTED, per platform: the file demo (sqlite-cs cf739c54a5, demo/file) is
       byte-identical to native Go, WAL pass included, on windows (i9, union 2fb422386c) and on linux (P4, union
       33d60d2ab9, after P2's paren-address fix), no stand-ins, both with -inline-arrays=modernc.org/libc#largeInteger
       (COORD 312 landing post). The sqlite lead now says so; the windows seed is C2's ruling C (0881fa6371) with its
       known issue (docs/KnownIssues.md) and the per-type -inline-arrays form (native-struct section).
     - P2 W9a mutex (bd2f1e3c38): the sqlite section, "sync.Mutex is Go's eight bytes".
     - P2 paren-address (b346c4d6a4, union fix; converter 2289a7c49b4; std runtime/<t>/mheap.cs -1/+1 x4 at
       33d60d2ab9): the sqlite section.
     - P2 wall C (a7540ab86d): a sqlite-cs bullet; W-conv-copy, W-embed-array (40699e4fe6) and the one-clone union fix
       (ad5e15194c): the new struct-copy section.
     - P2 zero-length array clone (54aa3a5b85) and its InotifyEvent suppressions (0a34f2f4d6): P2's own bullet under
       Upgrading from 1.26.10.3, as written.
     - C1 -tests -recurse skip (bcc0b03286): C1's own sentence in the -recurse section, as written.
     - P4 T.Deadline (99bb4bc5bd): Runtime fixes. P1 Tour 3 switch temps (672c430fc7): Smaller converter fixes.
       R class B slice proxy (9b6a116c33): a TypeScript readiness bullet.
     - No news item: P5 cpu-time harden (fc18c360b6, a test); P4 ide-mode cites (978a497b63, docs citations);
       P4 news-310 and news-311 (this draft).
     The .4 freeze is in effect: no batch follows before the release; only release-cut items change this draft. -->

## November 8, 2026 — Faster Native AOT, and a Release standard library from source

<!-- Written ahead and HELD for release day (the .2 precedent, e665f4ea51): "November XX" becomes the publish date as the
     last change before the release tag (GoCorpusMigration.md, the release ritual, item 1). Moved from
     docs/DRAFT-news-1-26-10-3.md by the 1.26.10.3 release-controls batch (claude/g-release-controls-1263, COORD
     2026-11-08), every PENDING item that had landed flipped against its committed record; what still waits on the
     publish is marked PENDING with the publish as its trigger. Flipped 2026-11-09 by COORD: the nugetgo .3 wave published
     (S1-S4: nine packages, gojq 0.12.19 and timefmt-go 0.1.8 among them) and the twelve 1.26.10.1/.2-built revisions
     were deprecated by the owner; the post-publish smoke (Test-PostPublishRestore.ps1) read 9 of 9 PASS. -->

**NuGet 1.26.10.3.** The second fix release on the Go 1.26 line. The converted standard library is still built from
Go 1.26.10. Converting a module uses a Go 1.26 toolchain, which the go2cs dotnet tool downloads on first use, and
the .NET 10 SDK. One change below breaks both
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
  The nugetgo revisions built against 1.26.10.1 and 1.26.10.2 are deprecated on nuget.org, each naming its 1.26.10.3
  revision as the alternate.
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
handshake take about 14% less time. Both are measured on `win-x64`, on one development machine, at the default x64
instruction set. Building a converted program for `x86-64-v3` (`<IlcInstructionSet>x86-64-v3</IlcInstructionSet>`)
gives no gain across the benchmark suite on the same machine, and an
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
Debug, about 21 times slower on a TLS handshake under the JIT (on `win-x64`, on one development machine), and the
publish failed. A solution file an earlier
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

**gojq for .NET.** [`itchyny/gojq`](https://github.com/itchyny/gojq),
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
so when two of a project's dependencies need different releases of one go.* package, the restore fails and names
them (NU1107). A direct reference to that package overrides the exact versions with NU1608 warnings, and the mixed
set can then fail at run time with `MissingFieldException`, so keep every go.* package at one release, or treat
NU1608 as an error (`<WarningsAsErrors>NU1608</WarningsAsErrors>`). A read of such a variable
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
