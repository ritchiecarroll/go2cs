# OQ2 method holders: the DONE record (2026-11-15)

Point-in-time record for the OQ2 cut on claude/p1-oq2-method-holder (lane P1). Amended with dated blocks, never
rewritten. The design is docs/phase4/DESIGN-promoted-forwarder-placement.md and its 2026-11-15 amendment; the
suppression generator is src/tools/pushnuget/New-GoApiCompatMoveSuppressions.ps1.

## API-compat suppressions for 1.26.11.1 (counts read from the committed files)

230 CompatibilitySuppressions.xml files under src/core, 13,234 entries, 0 refused by the generator:

| class | files | entries |
|---|---|---|
| reachable (neither under internal/ nor vendor/) | 130 | 9,995 |
| internal (a path segment `internal/`) | 87 | 2,721 |
| vendored (under vendor/) | 13 | 518 |

By diagnostic: CP0002 12,788 (8,273 method moves, 4,515 Δ-drop renames: 2,173 whose documentation id carried
the Δ, 2,342 whose id did not and whose return or value type now names the Go-named type), CP0001 369, CP0006 59,
CP0008 16, CP0021 2. The guard over the packed feed reads 0 failures and 0 unnecessary entries (362 packages,
525/525 assets), so every break the engine reports is one of the two announced kinds.

Breaks other than a method move or the Δ drop: none. The generator's first run refused 20 entries, runtime's
Pinner methods whose receiver parameter dropped its Δ (`Δp` to `p`); the rename-and-move test now lets a parameter
name keep or lose its Δ (fixture Dial.Spin and Dial.Spun, and Spun's negative).

## Test files changed by claude/p1-oq2-method-holder (f2fcf670d8) vs origin/master

Scope: `git diff --name-status origin/master HEAD`, 77 non-behavioral test files + 1477 behavioral files.
"Holder" = the method moved to `<pkg>ᴛ<Type>`; "Δ drop" = the U+0394 rename removed.

### src/go2cs unit tests (20)

- src/go2cs/methodHolder_test.go — NEW: 17 tests of holder emission: self alias, builtin/import qualified only
  in holder, method expr names holder, golib namespace-name shadowing, golib friend assemblies (batch 289 names)
- src/go2cs/methodHolderHandOwned_test.go — NEW (A): a hand-owned file declares holders only for the types it keeps
- src/go2cs/foreignNameCollisions_test.go — DELETED: foreign Δ-rename derivation (foreignNameCollisions.go retired)
- src/go2cs/funcMethodCollision_test.go — DELETED: func-vs-method Δ rename (funcMethodCollision.go retired)
- src/go2cs/rootShadowQualification_test.go — deleted TestSiblingTestDeclaratorsContributeAliasShadow: the sibling-test
  import-alias-shadow fold and its comment writer were retired with holders (commit c07e0b7671)
- src/go2cs/foreignTypeAliases_test.go — alias to a method-named type keeps its Go name (no ΔKind);
  foreignCollisionAliases resets removed; NEW TestForeignNameSharedWithAMethodIsNotRenamed
- src/go2cs/testConversion_test.go — 5 test-variant guards rewritten: Δ-renamed test methods/funcs/vars keep
  Go names, sit in holders; testMethodRenames gone; bridge-var test re-armed with type `union` + var `closed`
- src/go2cs/stdlibMetadata_test.go — syscall exports no aliases now (they were Δ renames); asserts moved to
  implement records (DLLError/SockaddrNetlink/Errno) and os's io/fs aliases; NuGet test: no syscallꓸ alias
- src/go2cs/sameNameImportAlias_test.go — want `afoo.Kind`/`bfoo.Kind` instead of qualified ΔKind; comments updated
- src/go2cs/sameNamePackageInterface_test.go — expected adapter names `HandlerжHandler` (was ΔHandler); comment updated
- src/go2cs/collectionExpressionTargets_test.go — shadowed `len` now qualified only inside the holder declaring len();
  fixture gains method each(), new assert that len stays bare outside the holder
- src/go2cs/methodExpressionWrapper_test.go — wrapper expectation now calls `mainᴛS.value(...)` (holder-qualified)
- src/go2cs/topLevelSeparation_test.go — F now gets a blank line (class switch from C's holder); new func O keeps
  the "written under the closing brace" adjacency case
- src/go2cs/crossAssemblyLiftDedup_test.go — removed testMethodRenames setup (test Δ-rename registry retired)
- src/go2cs/embedDirective_test.go — removed testMethodRenames setup (x2) and the go/types import
- src/go2cs/importSegmentShadow_test.go — removed testMethodRenames setup
- src/go2cs/internalTestLocalInterfaceStamp_test.go — removed testMethodRenames setup and go/types import
- src/go2cs/packageLevelAnonStructLift_test.go — removed testMethodRenames setup (x2) and go/types import
- src/go2cs/usingStaticNamespaceAlias_test.go — removed testMethodRenames setup/cleanup (x2)
- src/go2cs/internal/repoguard/docsProvenanceNotes_test.go — re-cited bufio.cs:17->18 (holder file gained
  `using static bufio_package;`), planted/expected lines shifted by one

### src/tests/GolibTests (43)

- src/tests/GolibTests/MethodHolderRuntimeTests.cs — NEW: runtime maps a holder to its receiver type's package
- src/tests/GolibTests/GolibNamespaceNamesTests.cs — NEW: renders golib's namespace-`go` type list, guards the
  generated src/go2cs/golibNamespaceNames.go (holder files read package names via using static)
- src/tests/GolibTests/GoUnexportedMethodPackageTests.cs — NEW tests + fixtures: unexported method in a
  [GoMethodHost] holder belongs to its receiver type's package (StructurallyImplements)
- src/tests/GolibTests/RuntimePanicCheckFixtures.cs — NEW holder fixtures runtimeᴛheld, runtime_(internal_)testᴛheld
- src/tests/GolibTests/RuntimePanicCheckTests.cs — new tests: `runtimeᴛX` holders count as runtime frames (2 old
  negative asserts dropped, now positive); Entry/FileLine/Name calls rewritten to receiver form `f.Entry()`
- src/tests/GolibTests/PublicizedRecvOverloadTests.cs — reads overloads from [GoMethodHost] holders, not package class
- src/tests/GolibTests/CopyBoundReceivers.allowlist.txt — Δ drop: ΔfuncTab->funcTab (11 rows), ΔfuncInfo->funcInfo
- src/tests/GolibTests/AbiMapGroupTests.cs — Δ drop abi.ΔMapType->MapType; +`using go.@internal;` (holder namespace)
- src/tests/GolibTests/AbiTypeForTests.cs — Δ drop (ΔType, ΔInterfaceType); abi.InterfaceType(d) -> d.InterfaceType();
  +using go.@internal
- src/tests/GolibTests/AbiUnknownArrayLengthTests.cs — abi.Len(d)/abi.ArrayType(d) -> receiver form; +using go.@internal
- src/tests/GolibTests/DefinedArrayDescriptorTests.cs — Δ drop (ΔArrayType, ΔType); abi.ArrayType(x) -> x.ArrayType()
- src/tests/GolibTests/GoMethodTextTests.cs — syscall_package.Error(errno) -> errno.Error(); big.ΔInt -> Int
- src/tests/GolibTests/GoModInfoComposeTests.cs — Δdebug.String(ref info) -> info.String(); +using go.runtime
- src/tests/GolibTests/GodebugUpdateHookTests.cs — godebug.Value(s) -> s.Value(); +using go.@internal
- src/tests/GolibTests/InternalSyncMutexContentionTests.cs — isync.Lock(Ꮡmu) -> Ꮡmu.Lock() etc; +using go.@internal
- src/tests/GolibTests/RuntimeCPUStatsTests.cs — metrics.Float64(v) -> v.Float64(); +using go.runtime
- src/tests/GolibTests/RuntimeGCPacerKnobTests.cs — metrics.Kind/Uint64(v) -> v.Kind()/v.Uint64(); +using go.runtime
- src/tests/GolibTests/RuntimeStopTheWorldContractTests.cs — metrics.Kind/Uint64(v) -> receiver form; +using go.runtime
- src/tests/GolibTests/SyncTestPullsTests.cs — synctest.Release/Run(b,..) -> b.Release()/b.Run(..); +using go.@internal
- src/tests/GolibTests/P256FieldDifferentialTests.cs — +using go.crypto.@internal.fips140.nistec (fiat holder scope)
- src/tests/GolibTests/P384FieldDifferentialTests.cs — same using added
- src/tests/GolibTests/P521FieldDifferentialTests.cs — same using added
- src/tests/GolibTests/DeepEqualAllocationTests.cs — Δ drop reflect.ΔType -> Type
- src/tests/GolibTests/GenericMethodSourceTests.cs — Δ drop reflect ΔType/ΔMethod/ΔValue
- src/tests/GolibTests/InlineArrayFamilyTests.cs — Δ drop reflect ΔValue
- src/tests/GolibTests/MulUnbalancedDiscriminatorTests.cs — Δ drop big.ΔInt -> Int
- src/tests/GolibTests/NamedArrayViewFloorTests.cs — Δ drop reflect.ΔType/ΔPointer (master test, fixed post-merge)
- src/tests/GolibTests/NativeArrayOpenerTests.cs — Δ drop reflect ΔType/ΔValue
- src/tests/GolibTests/NativeArrayReflectStoreTests.cs — Δ drop reflect ΔValue
- src/tests/GolibTests/PrintFloatArgumentTests.cs — Δ drop time_package.ΔSecond -> Second
- src/tests/GolibTests/ReflectInterfaceCopyChargeTests.cs — Δ drop: `using ΔValue = ..ΔValue` -> `using Value = ..Value`
- src/tests/GolibTests/ReflectSetterTests.cs — Δ drop reflect ΔValue
- src/tests/GolibTests/RuntimeErrorClassesTests.cs — Δ drop runtime.ΔError -> Error
- src/tests/GolibTests/RuntimeErrorPanicValueTests.cs — Δ drop runtime.ΔError, reflect.ΔValue
- src/tests/GolibTests/RuntimeMetricsHistogramTests.cs — Δ drop metrics.ΔFloat64Histogram
- src/tests/GolibTests/RuntimeTypeAssertionTests.cs — Δ drop runtime.ΔError (x3)
- src/tests/GolibTests/SliceBoundsFollowupTests.cs — Δ drop runtime.ΔError
- src/tests/GolibTests/SliceBoundsR1aTests.cs — Δ drop runtime.ΔError
- src/tests/GolibTests/SyncOnceShapeTests.cs — Δ drop reflect.ΔType
- src/tests/GolibTests/SyncTestFakeTimeTests.cs — Δ drop time.ΔSecond
- src/tests/GolibTests/SynctestTestHookTests.cs — Δ drop time_package.ΔHour
- src/tests/GolibTests/TimeOnGoTimerTests.cs — Δ drop time_package.ΔHour
- src/tests/GolibTests/WindowsUnnamedUnixSockaddrTests.cs — Δ drop syscall.ΔSockaddr

### src/tests/GenTests (7)

- src/tests/GenTests/MethodHolderGeneratorTests.cs — NEW: generators write twins/method set/ToString into holders
- src/tests/GenTests/MethodHolderNameAnalyzerTests.cs — NEW: GO2CS0013 analyzer (shared name binds elsewhere in holder)
- src/tests/GenTests/BoundedGoroutineInterceptorGeneratorTests.cs — NEW test: go-stmt interceptor resolves method groups
  in another type's holder (fixture with GoMethodHost holders)
- src/tests/GenTests/ValueEmbedStaticHopTests.cs — 2 NEW tests: embed-method call and value adapter name the holder
- src/tests/GenTests/ChanDirSigGeneratorTests.cs — chan-dir entries asserted in holder parts (mainᴛR/AssignD/S)
- src/tests/GenTests/MemberRecordGeneratorTests.cs — one ParamDims keep-alive per class (main_package + mainᴛholder)
- src/tests/GenTests/GoTypeDefinitionCommentTests.cs — NEW test for gen fix 9d593b0ebd: defined pointer pointee via file
  alias (latent gen bug, previously masked by the global alias the ΔLocation rename produced)

### Other src/tests (1)

- src/tests/CSharpConsumer/Program.cs — Δ drop: time.ΔSecond -> Second; uuid.ΔVersion -> Version (+check text/comment)

### src/core committed test sources (6)

- src/core/weak/pointer_test.cs — Δ drop time.ΔSecond -> time.Second
- src/core/math/bits/bits_test.cs — `runtimeꓸError` collision alias -> `runtime.Error` (x6)
- src/core/math/bits/package_test_info.cs — dropped `global using runtimeꓸError = ...ΔError` (Δ drop)
- src/core/crypto/sha3/package_test_info.cs — sha3.go GoPositionMap re-encoded (production sha3.cs gained holder lines)
- src/core/net/netip/package_test_info.cs — netip.go/uint128.go position maps re-encoded, call-map lines shift (holders)
- src/core/text/tabwriter/package_test_info.cs — tabwriter.go GoPositionMap re-encoded (holder lines)

### Behavioral summary (src/tests/Behavioral, 1477 files, 465 dirs)

Only .cs / .cs.target / package_info.cs changed in existing projects; no .go, .csproj or expected-output file changed
in any pre-existing project. Non-.cs files appear only in the two new projects.

- Holder re-mint, no Δ effect: 350 projects (16 of them change only a sub-package's .cs + package_info.cs, no golden)
- Holder re-mint + Δ drop: 58 projects (3 sub-package-only: CollidingPackageNames, CrossPackageExportedVarAddress,
  RootedUsingCapture)
- Δ drop only (no method of their own moved; refs like reflect.ΔType/time.ΔHour/runtime.ΔError/syscall.ΔHandle
  lose Δ, `global using pkgꓸT = ...ΔT` aliases go): 54 projects; 15 change only package_info.cs
- New projects (all files A, incl .go/.csproj/go.mod/goldens): MethodHolderAlias, MethodHolderNames
- Runner BehavioralTests/ (4 .cs): +[TestMethod] Check/MethodHolderAlias & MethodHolderNames in each of 4 suites
- Unexplained by OQ2: none found (residual-line scan after removing holder/Δ patterns came back clean)

Holder sub-effects seen inside the 408 holder projects (all OQ2 consequences):
- package_info.cs: <MethodHolders> section + GoMethodHost lines; GoPositionMap re-encodes (410 dirs)
- GoBoundedGoroutine line numbers re-cited (12): ClosureWriteVisibility DeferCallOrder DeferLoopCapture
  DeferVariadicCallee FuncLitArgCapture GoDeferConstArgs GoStmtReceiverBoxGroup GoStmtReceiverLambda
  NamedChannelType RefLoweredDeferChain RefPrimaryFieldReceiver SelectStatement
- `using System.Runtime.*;` -> per-type aliases since a holder file has `using static` (9): DeferArgEnclosingCapture
  DeferThunkFrames EmbedDepthMethodPromotion FuncLitArgCapture NoInlinePartial PointerFieldSelectors
  UnexportedEmbeddedMarker XpkgPromotedMethodSet XpkgPromotedMidLib
- method expressions name the holder (7): FirstClassFunctions FuncForPCName MethodExprDotImport MethodExpression
  MethodValueFuncNames PromotedMethodExpr CrossPkgUser
- method-group disambiguating casts dropped (3): FuncMethodSameNameCollision FuncValueMethodClash SortMethodSelfCapture
- builtin/import qualification only inside the declaring holder (5): ClearBuiltinShadow ReservedNameShadows
  SortArrayType SortMethodSelfCapture (`using main = main_package; main.Sort`)
  ImportSegmentTypeShadow (`main_package.sync` inside holder)
- ᴛ-suffixed type rename dropped: ReservedTypeMethodCollision (`sliceᴛ` -> `slice`)
- qualified/alias spellings of former Δ types simplified: SameNameImportAlias, ShadowedImportConstUser,
  RenamedTypeAliasReexport, DotImportRenamedType, TransitiveAliasPreload (now `CrossPkgLib_package.Status`)

### Read after the list: the retired sibling-test alias-shadow fold

Master folded every `func` declarator of a package and of its in-package `_test.go` half into the shadow set
(nameCollisionAnalysisOperations.go, `case *ast.FuncDecl`). OQ2 keeps only the holder's own methods
(`holderDeclares`). No guard is lost for free functions: Go forbids one identifier in both the package block and a
file block, so a package-level function (production or in-package test) named like an import of any file of the
same package is a Go compile error. Only a METHOD could shadow an import alias (hash/maphash's `bits()` method over
`import "math/bits"`), and a method now sits in its receiver's holder, where `holderDeclares` qualifies the alias,
test-file methods included (`declaredInTestFile`).

## The batch 308 merge (405d912c27, 4a93a61cb1, 138e3a2e10)

- A seeded reconvert of the merge commit's src/core with the merged converter, one target per run on linux/amd64,
  windows/amd64, darwin/amd64 and linux/arm64, differs from the tree in one emitted file on every target:
  runtime/heapdump.cs, byte-identical across the four (308's array-pointer view over OQ2's dropped Δ), +5/-5.
- Named, not applied: internal/syscall/windows/registry/windows/value.cs.auto lags 308's own view emission at
  master as well (a review sibling, not compiled); log/syslog/log.syslog.csproj is a windows-only residue that
  reads the same at master.
- GolibTests' TheConvertersListIsGolibsNamespace read red: 308 added AnyElementArrayViewCheck, IHeldStorage,
  INativeViewAnchor and InlineArrayShape to golib's namespace `go`, and src/go2cs/golibNamespaceNames.go did not
  list them. The test's rendering was copied over. No Go identifier in GOROOT/src or src/tests/Behavioral spells
  any of the four, so no emission moves.
- Two provenance notes both sides had moved were re-cited at the merged lines (convCallExpr.go's Offsetof fold,
  pointers.md's sliceHeader row).
- The first battery's stdlib linux/amd64 build stopped on "No space left on device" (80 MSB3021/MSB3027); after
  cleanup the second battery built all four targets with 0 errors.

Battery at 138e3a2e10 (linux/amd64 box, Go 1.26.11): go test, CNR, GolibTests 2814/0, GenTests 564/0, go2cs.slnx
0 errors, stdlib on four targets 0 errors, runner 10/10 PASS, leg B PASS, -tests sample 11 rows PASS. The runner ran
with -IgnoreDiskPreflight (13-14 GB free against its 25 GB floor).

## The batch 309 merge (bc4923b670, fba44db7a6)

- The same four-target seeded reconvert of the merge commit differs from the tree in exactly runtime/alg.cs and
  runtime/lock_spinbit.cs on every target, byte-identical across the four (309's array loads over OQ2's dropped Δ),
  +3/-3, as predicted before the run. Windows adds the two extras named above, unchanged.
- Seven provenance notes both sides had moved were re-cited (visitValueSpec.go five times, convUnaryExpr.go,
  convCallExpr.go). GolibTests' namespace-name test passes without a change: 309 added no golib type to `go`.
- Battery at fba44db7a6: go test, CNR, GolibTests 2875/0, GenTests 564/0, go2cs.slnx 0 errors, stdlib on four targets
  0 errors, runner 10/10 PASS, leg B PASS, -tests sample 11 rows PASS (runner with -IgnoreDiskPreflight).
