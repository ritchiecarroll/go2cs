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

<!-- 2026-11-23 .5 docs pass (P1, UX-2), re-derived at master 97cca467a2 from
     `git log --first-parent nuget-1.26.11.1..97cca467a2` (batches 318-325 and trains 1-7). Every bullet below is
     LANDED; the marks name the first-parent merge on master (or the direct commit), and each source comment names the
     merged branch tip it was read from. No bullet is QUEUED: every seat the 2026-11-20 draft marked QUEUED has landed
     (trains 1, 2 and 4). The header's note on the two new codes is superseded: both are registered at 97cca467a2
     (GO2CS2013, the shape witness's; GO2CS2014, the deferred-marker refusal's) and are named below, as is every other
     code this draft spells (GO2CS0014-0018, 0020, 0021, 2005, 2015), each read from DiagnosticCodes.md at
     97cca467a2. Not here, as internal: CI and workflow changes, test-only and record-only merges, the BOARD and
     runbook amendments, the compat-suppression cleanup (batch 325), the CNR host guard (batch 324), the M:N design and
     probe rows that change no shipped behavior (train 7's N5 design), and the union fixes of each train. A change made
     and cured inside this cycle has no bullet (train 3's M:N twin-in-method-set regression, cured by g-twin-methodset).
     GoBreakFloor is still 1.26.11.1 at 97cca467a2 (src/core/golib/buildTransitive/go.lib.targets), so no binary break
     is announced on master; the planned variadic<T> break (wall E) has not landed.
     HELD for release day, on the .2 and .4 precedent (e665f4ea51, 3f0edb682a): the entry and the README block stay in
     this draft until the release-controls commit moves them into docs/NEWS.md and docs/README.md on release day (both
     precedents kept the held entry off master's NEWS.md until then: .2 on a branch held unmerged, .4 excluded from
     batch 316 and merged at 86e387d2e4 just before the tag). The proposed README block is at the end of this file. -->

---

## November XX, 2026 — Code that reads closer to Go, cgo packages in a module, and tests that use testify

<!-- Written ahead and HELD for release day (the .2 and .4 precedent, e665f4ea51 and 3f0edb682a): "November XX" (the
     month too) and "X.X.X.X" are placeholders, set as the last change before the release tag, here, in the README
     block's first line and in its link anchor (GoCorpusMigration.md, the release ritual, item 1). -->

**NuGet X.X.X.X.** Converted C# reads closer to the Go it came from: an array literal is `array<nint>([1, 2, 3])`,
package `main` keeps its methods beside its functions, and a stack trace names each function as Go does.
`go2cs -recurse -cgo` converts a module's cgo packages with the module, and modules whose tests use testify convert
and match Go's test results. C# written by hand against converted code may need the changes listed first, under
*Upgrading from 1.26.11.1*.
<!-- 2026-11-23 (P1): the lead summarizes the bullets below; each claim carries its source there. -->

**Upgrading from 1.26.11.1.**

- **[LANDED 07483fc78a]** **An unnamed array literal now reads `array<nint>([1, 2, 3])`.**
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
  <!-- 2026-11-23 (P1): landed by train 1, merge 07483fc78a (claude/p1-vp-array-literal cc3d1c1d26). -->
- **[LANDED 098b83f9a1]** **Package `main` keeps its methods in the package class.** On every
  converted platform, a method of a type declared in package `main` is now a member of `main_package`, beside the
  package's functions, as the Go file reads; it was a member of a separate holder class, `mainᴛ<Type>`. A type of
  `main` keeps its holder only where one of its methods has the name of something the package reads bare (a
  package-level name, a universe name such as `len`, an import's name, or a member of golib's `builtin` class),
  which the method would otherwise hide. Library packages keep their holders. C# source written by hand against a
  converted program that names `mainᴛ<Type>` names `main_package` instead.
  <!-- P1 main-holders, owner ruling 2026-11-18 (FLEET 11-19), READY at a2035f6656 2026-11-19 21:12Z
       (inbox/COORD/20261119T211252Z-P1.md; P3's read 20261119T212356Z-P3.md). The rule and the clash list:
       docs/ConversionStrategies.md, "Package main keeps its methods in the package class" (383eeb05c3, b9195efa78);
       the converter 4d22819305 and 3a5109de87; a type a test file adds a method to keeps its holder, a2035f6656. -->
  <!-- 2026-11-23 (P1): landed by train 1, merge 098b83f9a1 (claude/p1-main-holders ea04c09f95). Dropped the bullet's
       last sentence, "A converted file whose last method sits in a holder now closes it with a bare `}`.": since
       train 4's c1-glyph-methods (027073dd1f, converter 187c6f88d0) no converted file switches into a holder, so the
       sentence no longer describes the emission. -->
- **[LANDED 027073dd1f]** **A library method's body is now a static method of the package class.** On every
  converted platform, the body of a Go method in a library package is written as Go wrote it, in the package class,
  named with a `⁔` after the Go name (`Add⁔`), its receiver the first parameter. The method with the Go name, in the
  receiver type's method holder (`timeᴛTime`), is a one-line forwarder to it that `package_info.cs` declares, so
  calls, interfaces and method sets read as before and a stack trace shows one frame, named as Go names the method
  (`time.Time.Month`). The forwarders keep the methods' names, access and signatures, so C# source that calls a Go
  method compiles unchanged; the `⁔` body is internal and is not for hand-written code to call. A type of `main` that
  keeps its holder (above) takes the same form.
  <!-- 2026-11-23 (P1): train 4, merge 027073dd1f (claude/c1-glyph-methods 94ec8ab494); the converter 187c6f88d0 ("a
       method with a holder takes the suffix form"), 149dab1f70 (promoted forwarders), the corpus regen 0bf1e9dabf.
       The text: docs/ConversionStrategies.md, "A library method's body stands in the package class, named
       `<Name>⁔`" and "A method named like a built-in does not hide it". -->
- **[LANDED 0f93005fbd]** **A Go method on a named slice or array type takes the
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
  <!-- 2026-11-23 (P1): landed by train 4, merge 0f93005fbd (claude/r-r7-slice-array 59c61e4734). The member list read
       again at 97cca467a2 from ISliceTypeTemplate.ClaimableMembers (it also lists Clone, claimed before this
       change). -->
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
- **[LANDED 1c9c6a224e, da3273f975]** **golib's `GoFrame` is a plain struct, not a `ref struct`.** `GoFrame` holds
  the deferred calls of one call of a converted function. The C# compiler no longer refuses a copy, a store, boxing or
  a capture of it; the source generator does, with error `GO2CS0016`, which names the site. A frame may only be a
  local initialized to `default`, a `ref` parameter or argument, or the receiver of its own members. The rule also
  runs where `RunAnalyzers` is `false`, and cannot be turned off. With `RunAnalyzers` set to `false`, one part is not
  checked: a use that binds only with code another source generator writes, such as a `defer(…, ref ᒐ)` whose lambda
  names a generated member, because the analyzer that checks those uses does not run. F# and Visual Basic get no
  check. Hand-written C# that follows the converted `defer` pattern keeps the frame a local of its function and passes
  it only by `ref`. See [Public surface](PublicSurface.md) and [Diagnostic codes](DiagnosticCodes.md).
  <!-- 2026-11-23 (P1), CQ-6: the ref-struct drop 1b8cc2658c (with 95165eb212), landed by train 3, merge 1c9c6a224e
       (claude/g-mn-proto-b1 1cfee4a63e); GO2CS0016 (GoFrameRuleGenerator) and its analyzer net (GoFrameRuleAnalyzer)
       landed by train 3, merge da3273f975 (claude/h-mn-origin-table 1fc98cd07c). The caveat: GoFrameRuleAnalyzer's
       remarks ("A project that sets it keeps the generator's rule and loses only this net"), the GO2CS0016 row of
       DiagnosticCodes.md, PLAN-public-surface-stability.md (item 8, 2026-11-20). Disclosed in PublicSurface.md, tier 2,
       in the same pass. -->
- **[LANDED e25362aedc; the `Δ`-name cure <train-11 merge>]** **A Go name spelled with a letter go2cs uses in generated
  names refuses its package.** A package that declares an identifier spelled with one of those letters (`Ꮡ`, `ᴛ`, `ж`
  and the others), or a `Δ` name equal to one go2cs generates in that package, stops with error `GO2CS2015`, which
  names each identifier: such a name could bind silently to a generated one. Every other `Δ` name converts, and a
  struct field, method or type whose name starts with `Δ` keeps its Go name in reflection, encoding and `fmt` output.
  Rename a refused identifier ([Diagnostic codes](DiagnosticCodes.md)).
  <!-- 2026-11-24 (P2, .5 Δ cure seat claude/p2-delta-go-name): the cure retires the Δ-led field, method and type arm
       and its KnownIssues entry (#delta-led-field-method-type-refused); this entry's link pointed there. Text
       adjusted to the cured state; P4 owns this file and may reword. -->
  <!-- 2026-11-23 (P1): train 4, merge e25362aedc (claude/i9-marker-glyph-refusal b1105e20a5); the refusal a0806c5d38,
       the folds 577eb7138e, 7286bd1e0e, b1105e20a5; behavioral DeltaNameEdgeCases 1cea7c8bdb. The text: the GO2CS2015
       row of DiagnosticCodes.md and KnownIssues.md "A Go struct field, method or type whose name starts with `Δ`
       refuses its package". -->
- **[LANDED 27edc19447]** **One C# type for each anonymous Go type in a package.** A Go struct or interface type
  written without a name now becomes one C# type in each package, which every site that spells it uses, so some
  generated names a hand-written C# file could have named (`Box_T`, `pair_T`) are gone. Those names were never a promise:
  [Public surface](PublicSurface.md) now lists them as names that can change in any release. Name the declared Go
  type instead.
  <!-- 2026-11-23 (P1): train 2, merge 27edc19447 (claude/p4-lift-dedup 29f9c49496); the converter bdaa9df223 ("one
       lift per anonymous type; a public site reuses a lift declared public"); COORD ruling 2026-11-20 on board (jj),
       cited in PublicSurface.md tier 3. -->
- **[QUEUED claude/i9-alias-keyword-escape (train 12)]** **A Go type named `record` is spelled `Δrecord`.** At the start
  of a C# member, `record` begins a record declaration, so a Go type declared with that name now takes the `Δ` prefix,
  like other names C# reserves. In the converted standard library this renames `net/http/fcgi`'s `record` type, and its
  method holder `fcgiᴛrecord` becomes `fcgiᴛΔrecord`. C# source written by hand that names the old spelling names the new
  one. See [Shadowing](ConversionStrategies-Reference/shadowing.md).
  <!-- 2026-11-25 (i9): owner ruling COORD 020915Z ("Δrecord, as ruled"); the seat claude/i9-alias-keyword-escape
       bf4cc3a23d; release-smoke's binary-compatibility guard read the holder's CP0001 on the train 12 union (417fd87146,
       run 45583774592), suppressed in src/core/net/http/fcgi/CompatibilitySuppressions.xml (COORD 2026-11-25T12:24:43Z). -->
- **[QUEUED claude/p5-darwin-arm64-d6 (tip in its READY post)]** **On Apple silicon the packages carry the `arm64`
  standard library.** The `osx-arm64` assets of every package that differs by platform are now the `darwin/arm64`
  build, where they were the `darwin/amd64` build that `osx-x64` also carries. On an Apple-silicon Mac,
  `internal/goarch` reports `arm64` and Go's `_arm64.go` files are in the packages, as `runtime.GOARCH` already
  reported. Go declares 24 of `syscall`'s `darwin/amd64` constants for no other Mac, among them `F_MARKDEPENDENCY`,
  `SYS_PROFIL` and the `SIOC…` and `SO_RESTRICT…` sets, so they are gone from the `osx-arm64` asset, as they are from
  Go on that Mac. An assembly built for `osx-arm64` against an earlier package that reads one of them fails with
  `MissingMethodException`. Take every `go.*` package from this release or later; go2cs's build check stops a build
  that mixes them.
  <!-- P5 D6 (docs/phase4/DESIGN-darwin-arm64-flavour.md section 3), branch claude/p5-darwin-arm64-d6. The breaks: the D6
       census (2026-11-27, at 4a56f17a3a, the 20 packages with an arch folder built at both arches, os/net/time control at
       zero) read 33 in 5 packages, of which only syscall is reachable (internal/cpu 3, internal/runtime/gc/scan 4,
       internal/runtime/startlinetest and runtime/race/internal/amd64v1 one whole-type removal each); a second read against
       the published 1.26.11.1 osx-arm64 assets reads the same 33 and nothing else. P2's floor ruling
       (2026-11-27T20:49:08Z): suppress all 33, the floor is the release D6 ships in, and this line names osx-arm64
       consumers, the 24 constants and the go.* release. The "build check" is GoBreakFloor (go.lib.targets). On release day
       docs/KnownIssues.md "On Apple silicon, the standard library is the amd64 build" is rewritten with this entry: until
       then it describes the published packages. -->

**What changed.**

- **[LANDED 5cd4536a09]** **A generic under a method-set constraint that is not
  self-referential now converts within its own package.** On every converted platform, a generic function or type
  whose constraint lists methods, and whose signature puts the type parameter where go2cs's constraint proxy cannot
  carry it (a generic named type, or the type parameter in a result, a map, a channel or a pointer), takes a shape
  witness: the type parameter stays the Go type, and its method calls go through a witness type that
  `package_info.cs` declares. Before, go2cs refused these with `GO2CS2005`. A site the witness cannot place yet, such
  as a constraint or type argument from another package, is refused with `GO2CS2013`, which names the site. See
  [Diagnostic codes](DiagnosticCodes.md).
  <!-- R shape-witness stage 1, READY 2026-11-19 11:53Z at 202d0ee3ad (inbox/COORD/20261119T115335Z-R.md), P3's
       review fixes at 529de5fc92, tip 5a4655646b (20261119T201550Z-R.md, run 8 20261119T224840Z-R.md). Stage 1 is
       same-package constraints only; %T and stack traces naming a carrier are stage 3. Design:
       docs/phase4/DESIGN-constraint-proxy-nonself.md section 12. Footprint in the standard library: one package, on
       all four targets. -->
  <!-- 2026-11-23 (P1): landed by train 1, merge 5cd4536a09 (claude/r-shape-witness-on-p5 68a7818821, the stage-1 seat
       rebased on the GODEBUG adapter). "a new error code" became GO2CS2013, registered at 97cca467a2. -->
- **[LANDED f4f933aac7]** **The converter stops where it would have left a marker in
  its output.** Some names are known only after a package's records are final, so the converter writes a marker and
  rewrites it afterwards. When that pass cannot finish a file, go2cs now stops with error `GO2CS2014`, which names the
  file and line, where it warned and left a marker that does not compile. This applies on every converted platform.
  See [Diagnostic codes](DiagnosticCodes.md).
  <!-- P5 godebug adapter seat, READY 2026-11-19 12:37Z at 95d7d7b04f (inbox/COORD/20261119T123751Z-P5.md),
       renumbered at cbc0aa3cc8 (20261119T185441Z-P5.md). The one corpus file it moves:
       internal/godebug/godebug.cs.auto, the same line on all four targets. -->
  <!-- 2026-11-23 (P1): landed by train 1, merge f4f933aac7 (claude/p5-dot5-godebug-adapter cbc0aa3cc8). "a new error
       code" became GO2CS2014. Dropped as internal (a review copy, not shipped code): "A package written entirely by
       hand now has the markers in its review copies (`.cs.auto`) rewritten too." -->
- **[LANDED 4029e7f3d4, 027073dd1f]** **Stack traces and `runtime.FuncForPC` name functions as Go does.** Every
  function literal now carries its Go name in a `[GoFunc]` attribute (`[GoFunc("main.func1")] () => …`, and
  `(*runner).fork.func1` for one inside a method), and a frame of a method body is named as Go names the method. A
  stack trace, a panic's trace and `runtime.FuncForPC` print those names, on every converted platform.
  <!-- 2026-11-23 (P1): train 6, merge 4029e7f3d4 (claude/c1-literal-names-t6 6d33e12c75): golib GoFuncAttribute
       832b50c1e8, the converter aaf504e10b ("every function literal carries GoFunc"), runtime e67d2eac5c ("a literal's
       frame is named from its GoFunc attribute first"), behavioral LiteralFrameNames be4a4eae9c (red first); the corpus
       regen 561f10a45b (560 files on four targets). Method frames: train 4, merge 027073dd1f (c1-glyph-methods), runtime
       82b890a1b1 ("a function literal in a method is named under the method's receiver, as Go names it") and
       GlyphLibraryFrameNames f4175e4135. Samples: ConversionStrategies.md (GetPrintLn.func1) and
       defer-panic-recover.md ((*runner).fork.func1). -->
- **[LANDED a61eec7463]** **`%p` prints the number `unsafe.Pointer(p)` carries.** `fmt`'s `%p`, `reflect.Value`'s
  `Pointer` and `UnsafePointer`, and the order `fmt` prints a map with pointer keys in, now read the same number as
  `unsafe.Pointer(p)`, so `fmt.Sprintf("%p", p) == fmt.Sprintf("%v", unsafe.Pointer(p))` holds as in Go. A pointer to
  storage that holds no references reports its pinned address, and the pin lasts as long as the pointer's box. A
  number read from `reflect.Value.UnsafeAddr` and turned back into a pointer, including through the field's
  underlying type, now names the field, where its first dereference panicked.
  <!-- 2026-11-23 (P1): train 6, merge a61eec7463 (claude/h-pfmt-identity 8426a92c29), COORD's %p ruling 2026-11-22,
       option A (1721a77ac4). The text: ConversionStrategies-Reference/pointers.md, the order-token paragraph and the
       convert-back paragraph (gojq's code tests); docs/phase4/DATA-pfmt-identity-2026-11-22.md. -->
- **[LANDED 8de0f7ece2, f74b4d7385, 640d45c9eb]** **`go2cs -cgo` combines with `-recurse`.** A module's cgo packages
  convert with the module; under `-recurse=nuget -cgo` they reference the `go.*` packages, for the machine's own
  operating system and architecture. A cgo package whose C use go2cs refuses is skipped by name and the rest of the
  module converts (a single-package `-cgo` run still stops). A `-cgo` conversion can also target another operating
  system or architecture with `-platforms` when `CC` names that target's C compiler (for example
  `zig cc -target <triple>`). Such a module uses the standard library converted without cgo, so `net` resolves host
  names with Go's own resolver and `os/user` reads the user database files; a package that imports `runtime/cgo` or
  `plugin` is refused by name. Packing a cgo package for every platform is still to come. See
  [Limitations](Limitations.md#cgo).
  <!-- 2026-11-23 (P1): train 3, merges 8de0f7ece2 (claude/c2-cgo-nuget-pack ef5c439ee1: -cgo with -recurse 32c6b517e5;
       another target with CC 07378119f9 and 3fe912eafd; runtime/cgo and plugin 4e98cf5fab), f74b4d7385
       (claude/c2-cgo-route-skip 1207fc9db1: a route refusal skips its package, 2b4e84400e) and 640d45c9eb (re-seat
       78e369a4a9: no C compiler, no target check). The text: Limitations.md "One target", DotnetTool.md (the
       `-recurse=nuget -cgo` paragraph) and CommandLine.md's `-cgo` row. -->
- **[LANDED 1f2a9afdfc, 5f2f2d0aa3]** **More of C's memory reads as Go's under `-cgo`.** A pointer field of a C struct
  (`l.next`) is read and written at its Go offset in C memory, and `*pp` and `*pp = v`, where `pp` is a `**C.T`, load
  and store the C pointer itself.
  <!-- 2026-11-23 (P1): train 4, merge 1f2a9afdfc (claude/h-cgo-sb2a ad632f3c2b: [GoCgoStruct] afdfa55880, the golib
       read and write 8352e4c998, go2cs-gen's properties 0b223db1a0; three of the four cgo phase 3 layout pins
       retired); train 6, merge 5f2f2d0aa3 (claude/h-cgo-1c 487a168cb1: the **C.T load and store 68220bfa6f).
       docs/phase4/DESIGN-cgo-phase3-marshaling.md 18.9 and 18.11. -->
- **[LANDED 90e20f2d49, 3d7d5e6ec0]** **A pointer word stored through `*unsafe.Pointer` or into a func variable is
  kept.** `*p` and `*p = v`, where `p` is a `*unsafe.Pointer` that names a word of C memory or of Go memory rebuilt
  from a number, read and write that word. Storing a func value's word into a func variable through `unsafe.Pointer`
  now sets the func, where the write was lost. A word that is not a func value's word, or that names a closure
  nothing else holds, panics by name ([Limitations](Limitations.md)).
  <!-- 2026-11-23 (P1): train 1, merge 90e20f2d49 (claude/h-cgo-32 c41bb9d3a1: the word doors df230c8d43; nil-holding
       managed slots a40cceb435 and d02c2dda3b); train 5, merge 3d7d5e6ec0 (claude/h-tc13-funcword 4dac8b5fc6: the
       func-word lost write, w4). The text: ConversionStrategies-Reference/pointers.md "The pointer WORD through a
       `*unsafe.Pointer`" and Limitations.md's two func-word bullets. -->
- **[LANDED ebbb1c850f, 0f96e28773]** **`GOMAXPROCS`
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
  <!-- 2026-11-23 (P1): landed by train 1, merges ebbb1c850f (claude/p2-set-default-gomaxprocs d017656199) and
       0f96e28773 (claude/p2-gomaxprocs-update 55efd75b6e). -->
- **[LANDED bfdb234ea5]** **`sync.RWMutex` is Go's own code.** On every converted
  platform, `sync.RWMutex` is now the converted `rwmutex.go`, Go's five fields over the semaphore golib already
  provides, where it was a hand-written class around a managed monitor. A copy of a `RWMutex` is a separate lock, as
  in Go, and `reflect` sees its fields. golib's semaphore now drops the state of a word nothing is waiting on, so a
  lock that once blocked no longer keeps its owner alive, and a waiter that asks to queue first (Go's `lifo`) is
  queued first.
  <!-- P2 RWMutex, re-READY 2026-11-19 19:07Z at 7df4159494 (inbox/COORD/20261119T190738Z-P2.md; P3's read
       20261119T195639Z-P3.md). The record: docs/ConversionStrategies-Reference/manual-conversions.md, "sync.RWMutex
       is Go's own code, and an empty semaphore bucket is retired" (dd2a8479cb); lifo 7df4159494. Rows re-read on
       linux: sync, internal/sync and runtime. golib's API only gained an overload (ApiCompat 0). -->
  <!-- 2026-11-23 (P1): landed by train 1, merge bfdb234ea5 (claude/p2-rwmutex-go-layout 7df4159494). -->
- **[LANDED cfab663076, bf262dff62]** **`sync.Pool` and `atomic.CompareAndSwapPointer` behave as Go's.** A
  `sync.Pool` ages its caches at every garbage collection, as Go's runtime does.
  `atomic.CompareAndSwapPointer` compares the pointers' addresses: an old value that names the stored address through
  another `unsafe.Pointer` swaps, where it reported `true` and swapped nothing, and a nil address panics as in Go.
  <!-- 2026-11-23 (P1): train 2, merges cfab663076 (claude/p5-pool-cleanup-per-gc 4e855b3716: efa75d1c8c "age
       sync.Pool at every collection, as Go's clearpools does") and bf262dff62 (claude/c1-atomic-casp-address
       aad61a927c: 8c80148c92 and 23528457b0; GolibTests AtomicCompareAndSwapPointerAddressTests). -->
- **[LANDED c869fc13a7]** **A blank import's comment says what happens to the package's
  `init`.** On every converted platform, Go's `import _ "image/png"` converts to the comment
  `// import _ "image/png" (init forced in ImportInitializers)`. The other two endings are `nothing to initialize`
  and, for a test file's blank import of the package under test, `compiled into this assembly; no hook`.
  <!-- P4 blank-import note, READY 2026-11-19 08:13Z at 7de4e2e267 (inbox/COORD/20261119T081355Z-P4.md); landing order
       after Tour item 4 and P1's main-holders (COORD). The text: docs/ConversionStrategies.md, the blank-import
       paragraph, and ConversionStrategies-Reference/package-conversion.md. -->
  <!-- 2026-11-23 (P1): landed by train 1, merge c869fc13a7 (claude/p4-blank-import-note 7de4e2e267); the three
       endings read again at 97cca467a2 in visitImportSpec.go. -->
- **[QUEUED claude/c2-d-named-functype (tip in its READY post)]** **A defined func type is a delegate of its own.**
  On every converted platform, a Go func type declared with `type`, such as `context.CancelFunc` or
  `bufio.SplitFunc`, is now a C# delegate named for it, whether or not it has methods; one without methods was
  written as its underlying `Action` or `Func<…>` delegate wherever it was used. So `%T`, reflect's `Name()` and a
  type switch now tell `type F func(string) error` from `func(string) error`, as Go does. Thirty exported types of the
  standard library change this way, among them `context.CancelFunc` and `CancelCauseFunc`, `bufio.SplitFunc`,
  `io/fs.WalkDirFunc`, `path/filepath.WalkFunc`, `go/types.Qualifier` and `archive/zip`'s `Compressor` and
  `Decompressor`. Compiled C# code that passed or received `Action` or `Func<…>` there must be rebuilt; C# source
  that assigns a lambda to one of these types compiles unchanged, and source that assigns an `Action` or `Func<…>`
  value wraps it, as in `new CancelFunc(action)`. See
  [Every defined func type declares its own delegate](ConversionStrategies-Reference/struct-types.md#every-defined-func-type-declares-its-own-delegate).
  <!-- C2's D seat, COORD rulings 2026-11-23 02:36Z (cure 1) and 05:38Z (the break floor moves at the .5 release,
       F = R; this line and the binary-compatibility suppressions are owed by the seat). The thirty types are the
       exported collapse markers of the corpus at 1079ca371f, the same on windows, linux and darwin. -->
- **[LANDED 00b0dc37dd]** **Converting a module: two packages that spell one
  anonymous struct now agree on its layout by themselves.** When code such as ccgo's output declares the same
  anonymous struct in two packages (modernc.org/sqlite's `TLARGE_INTEGER` and modernc.org/libc's `largeInteger`),
  a `go2cs -recurse` run now seeds the declaring import so that both packages hold the struct's arrays the same way,
  and a value of it crosses between them by value. Before, this took `-inline-arrays=<import path>#<TypeName>` by
  hand, which still works; converting modernc.org/sqlite for Windows no longer needs it. This was measured on
  windows/amd64, linux/amd64, linux/arm64 and darwin/amd64. See [`-inline-arrays`](CommandLine.md).
  <!-- C2 seat (3)+(A), the -recurse importer seed (W18 ruling C item 3, scope b), merged with master at 18960ca538
       (inbox/COORD/20261120T065353Z-C2.md); the dotnet legs were run by P2 (20261120T065907Z-P2.md onward). The rule:
       docs/ConversionStrategies-Reference/slices-and-arrays.md, "A struct read from native memory holds its arrays
       inline, and seeds make two packages agree" (f8ad290f77, ae7eaf1ee4). Check C2's final READY post before release:
       READY waited for the CNR leg when this draft was started. -->
  <!-- 2026-11-23 (P1): landed by train 2, merge 00b0dc37dd (claude/c2-recurse-importer-seed 752d4d63db). The sqlite
       sentence: KnownIssues.md "Converting modernc sqlite for Windows needs a seed for libc's `largeInteger`" (master
       seeds it; 1.26.11.1 needs the flag). -->
<!-- 2026-11-23 (P1): no bullet for train 2's merge ac764e5c5d (claude/i9-xxh3-neon-forward 18191cd31c, which forwards
     zeebo/xxh3's arm64 NEON stubs to accumScalar and accumBlockScalar and widens GO2CS2011): its commit message says
     the rows are inert today, because plain xxh3 v1.1.0 stops earlier at its unsafe header views and the fork the
     1.26.11.1 notes describe has no NEON stubs, so no user sees a change yet. -->
- **[LANDED b72a8a405c, 61b23a9888, 2506d6d48f]** **Faster reslicing, smaller Native AOT builds, faster start-up.**
  Reslicing inlines again: a string-building benchmark that ran about twice as slow on 1.26.11.1 as on 1.26.10.2
  under the JIT and ReadyToRun is back at the 1.26.10.2 speed (measured on win-x64). golib's pointer boxes keep their
  per-type tables in one shared holder, so a Native AOT publish of the converted TypeScript parser demo on win-x64 has
  about 29% fewer symbol names and a 13% smaller image, and the AOT compiler's peak memory is about 25% lower. A
  program with many types builds its method sets at start-up faster: about 0.3 s less for a large converted program,
  measured on linux-x64.
  <!-- 2026-11-23 (P1). Reslicing: train 1, merge b72a8a405c (claude/r-window-inline ad1c7acf24; golib slice.cs Window
       and WindowRare), docs/phase4/DATA-pgo-window-inline.md sections 4 and 5: PerfString JIT median 588/582 ms at the
       cut against the .2 record's 606/610 and master's bimodal 1156/677; ReadyToRun 1114/1104 against .2's 1171/1199
       and master's 2294/2346 (Windows 11 x64, .NET 10.0.11). Lever E: train 1, merge 61b23a9888 (claude/i9-lever-e
       43982146a0); the draft's earlier source comment, kept: "i9 lever E, READY 2026-11-18 04:54Z at 43982146a0
       (inbox/COORD/20261118T045402Z-i9.md). The readings: the BOARD row "lever E seat" in 43982146a0 (parsedemo at
       f36b6b0d48, win-x64: names -29.18%, image -12.64%, ILC peak -24.56%). JIT timing within noise; process
       allocation about 53 KB lower per run." Start-up: train 1, merge 2506d6d48f (claude/p5-b3-a1-keys ec60592f47,
       golib 0a59f08ab1), docs/phase4/DATA-startup-method-sets-a1.md: BigAst wall -297 ms paired median, 30/30 faster
       (cloud linux/amd64, 4 vCPU, JIT). -->
- **[LANDED 69feb33d6b, d0659896d7]** **A converted program runs on a newer .NET when .NET 10 is missing.** A
  converted executable, Windows GUI program or test host that references the `go.*` packages rolls forward to the
  newest installed .NET when no .NET 10 runtime is present, and a go2cs clone builds with a newer .NET SDK. The
  packages are validated on .NET 10; set `RollForward` to `Disable` or `LatestMinor` in your project to require it.
  A `RollForward` the project sets, or a `runtimeconfig.template.json`, takes precedence.
  <!-- 2026-11-23 (P1): train 1, merge 69feb33d6b (claude/i9-rollforward-major 9b672cc964: Exe, WinExe and the test
       host; a project's own value and a runtimeconfig template win), and merge d0659896d7 (claude/i9-sdk11-latestmajor
       6e65454fec: global.json rollForward latestMajor, floor 10.0.400). The text: docs/README.md Requirements. -->
- **[LANDED dcfc9c99a1]** **A conversion keeps its Go build cache apart from yours.** The `go build` runs that read
  the Go compiler's escape decisions for each converted package now use their own cache, `go2cs/probe-cache` under the
  user cache directory, or the directory named by `-probe-cache` or `GO2CS_PROBE_CACHE`, not your `GOCACHE`. The cache
  is reused across conversions and is safe to delete ([Known issues](KnownIssues.md#a-conversion-keeps-a-go-build-cache-of-its-own)).
  <!-- 2026-11-23 (P1): train 1, merge dcfc9c99a1 (claude/p4-probe-cache 7f14c1bb4d; board hh, COORD 2026-11-20
       09:41Z). The text: CommandLine.md's `-probe-cache` row and the KnownIssues.md entry. -->
- **[QUEUED claude/p5-stackappend-optin (tip in its READY post)]** **The stack-append buffer is emitted only where it
  is needed: `-stdlib` and `-tests`.** Go 1.25 and later give the first `append` to a slice that does not escape a
  32-byte buffer, which only `cap()` can observe. go2cs 1.26.11.1 reproduced it in every conversion, at the cost of a
  `go build -gcflags=-m` per package and target. The new option `-stack-append=on|off|auto` controls it; the default,
  `auto`, keeps it on for `-stdlib` and `-tests`, whose converted tests compare against Go's own, and turns it off for
  a plain or `-recurse` conversion, which then runs none of those builds and creates no probe cache. With it off,
  `cap()` after such an `append` can read lower than Go's; pass `-stack-append=on` to reproduce Go's
  ([Known issues](KnownIssues.md#by-default-cap-after-appending-to-a-slice-that-does-not-escape-can-differ-from-go)).
  <!-- 2026-11-24 (P5, train 12): P3's review of 57f318f4b0, finding 3 (COORD 19:52Z). The owner's ruling: COORD
       2026-11-24 17:50Z, option 2 of P5's scout. The text: CommandLine.md's -stack-append entry and the KnownIssues
       entry. -->
- **[LANDED 1c9c6a224e … e280cae69d]** **An experimental M:N scheduler, off by default.** go2cs can run goroutines
  on a small pool of threads, as Go does, instead of one operating-system thread each. The technique is **async
  twins**: a second copy of each function that waits by giving its thread back to the scheduler. It is opt-in: a
  program gets it only when its projects are built with twins, which works only inside a go2cs repository clone, and
  it runs with `GO2CS_MN=1`. <!-- (b)-dependent --> The published `go.*` packages are not built with twins, and a program that does not opt
  in runs each goroutine on its own thread, as before. The diagnostics `GO2CS0014`, `GO2CS0015`, `GO2CS0017`,
  `GO2CS0018`, `GO2CS0019`, `GO2CS0020` and `GO2CS0021` come only from a build with twins
  ([Async twins](AsyncTwins.md), [Known issues](KnownIssues.md#goroutines-on-a-shared-pool-of-threads-mn-are-experimental-and-off-unless-you-build-and-run-for-them)).
  <!-- 2026-11-24 (P4, row 18): "async twins" named and docs/AsyncTwins.md linked (COORD 2026-11-21 15:49Z, step 0).
       GO2CS0019 added: it landed with row 7's N1 (train 9, b2c9435159) and is reported only in a build with twins
       (DiagnosticCodes.md row 0019 at cb6def82a2). -->
  <!-- 2026-11-23 (P1): the M:N rows, by first-parent merge: train 3 1c9c6a224e (claude/g-mn-proto-b1 1cfee4a63e, the
       scheduler, the twin generator, GO2CS0014/0015/0017/0018) and da3273f975 (claude/h-mn-origin-table 1fc98cd07c);
       train 4 467912f059 (g-mn-row6-parkstep), 817c346d85 (g-mn-gosched-park); train 5 13c748a241
       (g-mn-row7-lock-sleep), 2b46f750ce (c1-mn-polish: the "experimental" prefix and the KnownIssues section),
       f9ba32f48b (g-mn-fv-fields, GO2CS0020), f82c70ab45 (g-mn-convoy); train 6 46702b196c (g-mn-twins-p1, GO2CS0021)
       and f0f44ca4eb (g-mn-twins-p45, GoStdlibTwins default off); train 7 e280cae69d (g-mn-refblock, the Go-order
       note). The text: KnownIssues.md "Goroutines on a shared pool of threads (M:N) are experimental". GO2CS0016 is
       not listed here: it is the GoFrame rule, reported in every build (Upgrading, above). -->
- **Fixes.**
  - **[LANDED 4029f81426, 1dfb01cfdc]** A deferred call that writes through a slice, an element pointer or an
    `unsafe` view of a named array result, taken before `return v`, now changes the value the function returns, as in
    Go. A function literal's named array result that escapes in a function with a `defer` has its storage, where every
    index through it panicked.
    <!-- 2026-11-23 (P1): train 3, merges 4029f81426 (claude/p5-doorc-assign-limit d7df22c615: the escaping result's
         storage 901eb8fbf2, the for-clause tuple copy bb687e3e32; the return shape recorded as a known issue) and
         1dfb01cfdc (claude/p5-return-defer-inplace e12b25a4a6: the return assigns in place; the known issue removed).
         docs/phase4/DESIGN-array-pointer-doors.md 21 and 22: the return shape predates door C. -->
  - **[LANDED d4f9ca9311]** A pointer parameter that a function literal assigns is read through its new target after
    the assignment, where the function went on reading the old one, and it may hold nil and pass it on as in Go.
    `for _, n = range n.kids` into a pointer parameter converts and repoints it.
    <!-- 2026-11-23 (P1): train 2, merge d4f9ca9311 (claude/p1-f5-closure-rebind e9be5c1897: 65036cc094 "a silent wrong
         read, and an endless loop", e8c6ef082d (CS0029 before), 6cf0ee95c5 (DerefOrNull)); behavioral
         PointerParamClosureRepoint. -->
  - **[LANDED 224bbe2bf2, fb8b8ec897]** A method promoted through an unexported embedded struct of another package, at
    any depth, is called on the outer value, so it compiles (goldmark's `ast.Link`) and its writes are kept; a promoted
    call across another package's embedded struct takes one step.
    <!-- 2026-11-23 (P1): train 1, merge 224bbe2bf2 (claude/c1-promoted-unexported-embed e2a2e3a066: 1e98a50fb4,
         1855b4590a, 7dc112e959; board pp); train 2, merge fb8b8ec897 (claude/c2-1b-one-hop 402041735d, TS perf item
         1b). -->
  - **[LANDED 532d067c08]** A value of another package's anonymous interface that embeds named interfaces takes that
    package's type, and passes where each embedded interface is expected.
    <!-- 2026-11-23 (P1): train 1, merge 532d067c08 (claude/p4-anon-iface-embed 03db19b86a, 84442b47ed; board ii). The
         text: ConversionStrategies-Reference/interfaces.md, "Another package's anonymous interface that embeds named
         interfaces takes that package's lift". -->
  - **[LANDED c57acd45e0]** A Go package named `extension`, a C# 14 contextual keyword, converts and is imported under
    its own name.
    <!-- 2026-11-23 (P1): train 1, merge c57acd45e0 (claude/p4-alias-keyword c97ceeaf61, 1ac5ba4b23; CS9306; board qq).
         The text: ConversionStrategies-Reference/shadowing.md. -->

**Converting and testing modules.**

- **[LANDED bb6267d0e4]** **Modules whose tests use testify convert and match Go.** github.com/stretchr/objx (v0.5.2
  and v0.5.3), whose tests use testify, go.yaml.in/yaml/v3 v3.0.5 and github.com/pmezard/go-difflib v1.0.0 are
  validated: each converted test suite matches Go's results (on linux/amd64), and objx's runs convert the testify
  packages its tests import, with testify's own dependencies. See [Validated modules](ValidatedModules.md).
  <!-- 2026-11-23 (P1): train 6, merge bb6267d0e4 (claude/c2-stack-union 2de868d53e, carrying C2's stack 9a6c4e1019):
       the banks 519ffacf51 (go-difflib, 14 matched, vet disclosed), 00fe5194b8 (objx v0.5.2, 255 matched),
       67d79b0b1f (objx v0.5.3), 9a6c4e1019 (yaml v3 v3.0.5, 704 matched). The text: ValidatedModules.md. -->
- **[LANDED bb6267d0e4]** **A converted test assembly is compiled without optimization**, so a line that test code
  reads (`t.Logf`, testify's failure locations) matches Go, while the package under test stays optimized. Set
  `-p:GoTestOptimize=true` to build it optimized, for a benchmark run. Optimized code elsewhere can still report an
  earlier line ([Known issues](KnownIssues.md#in-optimized-code-runtimecaller-can-report-an-earlier-line-of-the-calling-function)).
  <!-- 2026-11-23 (P1): train 6, merge bb6267d0e4, carrying claude/c2-m2-test-switch d881e25220 (golib de46ae4938 "a
       converted test assembly is compiled without optimization"; docs de97f0e231 and dc6af56c5c). -->
- **[LANDED bb6267d0e4, 1397c07b06]** **`-tests` on a module.** A module-cache release is converted and tested from a
  copy, so a run leaves the module cache unchanged, and a release that ships no `go.mod` converts. `-tests -recurse`
  finds what a test file imports under the run's build tags. `-test-oracle-vet-off` runs Go's side without vet, for a
  package whose own `go test` fails at vet before any test runs, and states each vet finding; it is refused for a
  package vet passes. `-module-version` names the version of a module copy, and the run, its record and its proof
  page say the version is claimed. See [Command line](CommandLine.md).
  <!-- 2026-11-23 (P1): train 6, merge bb6267d0e4 (C2's stack: 85efd7b257 "every module-cache release converts and
       tests from a copy", 601dc1d111 (cache byte-unchanged), 7a624b68af (go.mod-less release), a39148c5ae (the test
       closure with the run's tags), 3f94b4818a and f3fa1f7362 (-test-oracle-vet-off)); train 1, merge 1397c07b06
       (claude/c1-mint-module-evidence 292da5d1e8, -module-version). The text: CommandLine.md's rows. -->
- **[LANDED 19fd193789]** **A converted test project references what its extension-method
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
  <!-- 2026-11-23 (P1): landed by train 1, merge 19fd193789 (claude/c1-test-refs-ext a750c69c8c). -->
- **[LANDED 30a590a94f]** **Test comparison: the Go side builds a module that reaches
  `math/big` through a linkname.** The `go test` run that a converted test suite is compared against now drops the
  `math_big_pure_go` tag everywhere except on `math/big`'s own row, so a module that pulls `math/big` by linkname
  links. A module's proof page now records the build tags of that Go run. Measured on windows/amd64, linux/amd64 and
  darwin/amd64: `math/big` is the only standard-library row whose files change.
  <!-- C1 CLASS D, merged with master at 2a7225ce67 (inbox/COORD/20261120T072952Z-C1.md, 20261120T073802Z-C1.md);
       the fix 366fca939d, the tags on proof pages 8d9104967f, the go list reading ae9a2a8e55
       (docs/phase4/data/oracle-tags-1120/). -->
  <!-- 2026-11-23 (P1): landed by train 1, merge 30a590a94f (claude/c1-oracle-tags 3443259102). -->

**The go2cs tool.**

- **[LANDED 6379495c3b]** **A `-recurse=nuget` conversion no longer reads an older local go2cs root.** Such a
  conversion reads standard-library names only from the metadata built into go2cs, and says once when it finds a local
  go2cs standard library that it does not read. With go2cs 1.26.11.1 and earlier, it read `~/go2cs` (or `GO2CSPATH`,
  or `-go2cspath`) ahead of that metadata and could emit names the published packages lack
  ([Known issues](KnownIssues.md)).
  <!-- The entry: batch 319 (2d6cf69fd6, P4 a0d69468d4), measured on windows-amd64 and linux-x64. The fix is i9's .5
       seat, not cut when this draft was started (COORD 2026-11-20 08:02Z): replace this mark with its branch and tip
       when it is READY, and check the wording against its READY post. -->
  <!-- 2026-11-23 (P1): the mark was "[QUEUED i9, not cut yet]"; the fix landed by train 1, merge 6379495c3b
       (claude/i9-nuget-metadata-precedence 188e9db532; the converter 954b184a3d). Its wording read from KnownIssues.md
       "A `-recurse=nuget` conversion reads an older local go2cs root" (725bfc923c, "Fixed in the next go2cs
       release"). The first draft's text, replaced: "The known issue added after 1.26.11.1, where such a conversion
       read `~/go2cs` (or `GO2CSPATH`, or `-go2cspath`) ahead of the metadata embedded in go2cs and emitted names the
       published packages lack, is fixed, and its entry leaves [Known issues](KnownIssues.md)." -->
- **[LANDED 01693f142c, 47ce71a520]** **Each published go2cs binary names its commit.** `go2cs -version` of a
  release archive reports the commit it was built from, unmodified; some 1.26.11.1 archives (linux-x64, osx-arm64)
  reported `vcs.modified=true`.
  <!-- 2026-11-23 (P1): batch 321, merge 01693f142c (claude/p4-release-vcs-clean 02bc5861f9: "the published 1.26.11.1
       linux-x64 and osx-arm64 archives" read modified=true; win-x64 false); batch 323, merge 47ce71a520
       (claude/p4-release-record-first 20c4b956e2: the tool packed from the clean record commit). -->
- **[LANDED cfec06486f]** **The "converted by go2cs" badge shows on nuget.org.** The badge a converted module's
  README carries is now served from cdn.jsdelivr.net, a host nuget.org shows images from
  ([Converting a module](ConvertingAModule.md)).
  <!-- 2026-11-23 (P1): train 1, merge cfec06486f (claude/p2-nugetgo-tools-12611 ffaebde09e; readmeValidationBadge.go
       go2csBadge). -->
- **[QUEUED claude/p4-cli-help (tip in its READY post)]** **`go2cs -help` lists the ten options a first conversion
  needs.** `go2cs -help all` lists every option, sorted into common, advanced and experimental, and
  `go2cs -help <option>` explains one, for example `go2cs -help tests`. Help prints to standard output and exits 0. A
  mistyped option names the nearest ones (`there is no option -test-actoin. Did you mean -test-action?`) instead of
  printing every option. [Command line](CommandLine.md) is written from the same table: its common options link to
  an option reference that explains every option.
  <!-- 2026-11-24 (P4): the owner's CLI-help item, train 12; rulings: -stdlib and -cgo are common, the reference
       covers all options sorted common, advanced, experimental. -->

**Known issues.**

- **[LANDED a9ee17b1eb, 82110652f9]** **Fixed: under Native AOT, a type assertion to `error` that the method set
  decides answers as Go's does.** With the go.* 1.26.11.1 packages, a Native AOT executable could answer `ok == false`
  for `p.(error)` on a value whose method set has `Error() string`. golib's `error` now declares a run-time shell
  that holds any value, so Native AOT has code for it
  ([Known issues](KnownIssues.md#under-native-aot-a-type-assertion-to-error-that-the-method-set-decides-can-answer-false)).
  <!-- 2026-11-23 (P1): train 3, merge a9ee17b1eb (claude/p2-aot-structural-hit 24fcc0026b: golib 05728981b3,
       KnownIssues 782e563b50 "reads fixed in the next release"), re-seated at 82110652f9 and 42d78181d4. Measured on
       linux-x64 (KnownIssues.md source comment). -->
- **[LANDED bb6267d0e4]** **Newly described: in optimized code, `runtime.Caller` can report an earlier line of the
  calling function.** A program reports Go's lines in a `Debug` build, or at a large cost with the JIT settings the
  entry names; converted tests are not affected
  ([Known issues](KnownIssues.md#in-optimized-code-runtimecaller-can-report-an-earlier-line-of-the-calling-function)).
  <!-- 2026-11-23 (P1): train 6, merge bb6267d0e4 (dc6af56c5c), measured on linux-x64. -->

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
<!-- 2026-11-23 (P1): dropped, as the draft said to once the fix shipped (it landed, 6379495c3b): "- **[LANDED
     2d6cf69fd6]** [Known issues](KnownIssues.md) names the stale local go2cs root that a `-recurse=nuget` conversion
     reads (see the known issue above; drop both bullets together if the fix ships). <!- - Batch 319, P4 a0d69468d4.
     - ->" The known-issue bullet itself stays, rewritten as the fix, under The go2cs tool. -->

---

## README NEWS block, held for release day

<!-- 2026-11-23 .5 docs pass (P1), UX-2: the docs/README.md NEWS block for this entry, written ahead on the 1.26.11.1
     precedent (3f0edb682a, the README block beside the NEWS entry). It stays here, not in docs/README.md, until the
     release-controls commit on release day, as the entry does (header comment above). On that day it replaces the
     1.26.11.1 block (its heading, HELD comment and paragraph; the "All announcements" line stays), and "November XX",
     "X.X.X.X" and the anchor's "november-xx" become the release's date and version, as in the entry's heading. The
     anchor below is the entry heading's, as kramdown derives it. It quotes no roster figure (check-roster-format 2e). -->

```markdown
## 📰 NEWS — Code that reads closer to Go, cgo packages in a module, and tests that use testify

<!-- HELD for release day with the NEWS.md entry (the .2 and .4 precedent): "November XX" (the month too), "X.X.X.X"
     and the anchor's "november-xx" become the publish date and version as the last change before the release tag.
     This block quotes no roster figure (check-roster-format 2e). -->
**November XX, 2026, NuGet X.X.X.X.** Converted C# reads closer to the Go it came from: an array literal is
`array<nint>([1, 2, 3])`, package `main` keeps its methods beside its functions, and a stack trace names each function
as Go does. `go2cs -recurse -cgo` converts a module's cgo packages with the module, and modules whose tests use
testify convert and match Go's test results. C# written by hand against converted code may need the small changes
the notes list first.
[Full announcement](NEWS.md#november-xx-2026--code-that-reads-closer-to-go-cgo-packages-in-a-module-and-tests-that-use-testify).
```
