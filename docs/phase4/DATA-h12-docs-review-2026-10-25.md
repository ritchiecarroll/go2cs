# DATA: the H12 user-docs hard review, claim changes and code findings

**Point-in-time record, 2026-10-25, lane C2.** It lists every edit of the H12 review that changed a CLAIM (not just
wording) in `docs/ConversionStrategies.md`, the `docs/ConversionStrategies-Reference/` pages and
`docs/ConsumingGoFromCSharp.md`, and the places where go2cs was found to behave differently from Go. The review read
the pages against the 1.26 hop tip `3e3f18925a` (Go 1.26.10). Line numbers are those of the unedited pages at that
tip. Wording-only edits (tense, history moved into comments, fleet vocabulary removed, figures moved into comments,
platforms named, cross-references made into links) are not listed one by one.

## Code findings: go2cs behaves differently from Go

Each was run on linux/amd64 at `3e3f18925a`: the probe converted with the tip's converter, built Release with
`GoTargetOS=linux` on .NET 10, and compared with `go run` at go1.26.10.

1. **A pointer-receiver method value on a package-level variable binds to a copy.** `var g ctr; f := g.inc; f();
   fmt.Println(g.a)` prints 1 in Go and 0 converted. The emission is `var f = Ꮡ(g).inc;`, and `Ꮡ<T>(in T)` boxes a
   copy. A local variable is correct (it is boxed through `heap(..., out var Ꮡl)`). The converter knows the package
   var has no box of its own (lazyPackageVarOperations.go:157-163), but nothing boxes it.
2. **A shift on a named integer type by a variable count.** `type word uint64; w << s` emits `w << (int)(s)`, and the
   generated operator (NumericTypeTemplate.cs:128) takes an `int` count and converts it to `uint64`. A count of
   `1<<32` truncates to 0 (Go 0, converted 1). A negative signed count shifts to 0 with no panic, where Go panics
   with "runtime error: negative shift amount".
3. **An index-only range over a nil `*[N]T` that is a function parameter or a pointer receiver panics**, where Go
   ranges N times (Go prints 3 and 4; the converted program panics with the nil-dereference report). A
   pointer-to-array variable is correct: it passes `range(p, N)`. The pages now state this divergence.

Two claims first read only from the code were also run and hold: generic `println` prints the underlying value of
a named basic type, and an empty variadic call passes a nil slice (`parts == nil` is true).


## docs/ConversionStrategies.md 1-2144

### MATERIAL
- L220 + L261 (At a glance, Initialization row and "Provided by" legend): OLD `var names = []string{…}` becomes "a static field", provided by "converter"; legend lists four generators. NEW "a static field, or for a larger pure-data value a [GoLazy] holder class built on its first read", provided by "converter · LazyHolderGenerator"; legend lists LazyHolderGenerator. Evidence: lazyPackageVarOperations.go:97, :574; LazyHolderGenerator.cs:129; src/core/unicode/tables.cs:2884.
- L1341 (init order, which vars are built lazily): OLD an eligible initializer may read vars "eligible and never assigned". NEW "never assigned or addressed". Evidence: lazyPackageVarOperations.go:187.
- L1343: OLD the eager list omitted test-file vars. NEW adds "a var declared in a _test.go file". Evidence: lazyPackageVarOperations.go:120-123.
- L1344-1346: OLD "A var whose address is taken keeps its heap box…" with no exception. NEW adds "A var whose address only a pointer-receiver method value takes (f := X.M) has no box of its own, so it stays eager." Evidence: lazyPackageVarOperations.go:157-163; a probe.

### EDITORIAL
- L431 ᴛ glyph row adds "a lazy var's holder class" (lazyᴛ_Lu).
- L1065 .editorconfig provenance comment: darwin and windows sections omitted.
- L2020 fold probe comment re-run at 3e3f18925a, same output.
- L1341-1349 re-wrapped.

### UNVERIFIED
- L1817: an untyped float const beyond float64 range is also a static readonly GoBigConst (visitValueSpec.go:1196-1201); page names only the integer case.
- L955: module initializers run in file-compile order (Roslyn), not re-measured.

### OTHER PAGES
- variable-initialization-order.md should carry the same three eligibility rules.
- At-a-glance cross-checks for: Source Generators (LazyHolderGenerator, [GoLazy]); uintptr a golib struct; complex128 = Complex, complex64 golib struct; AdapterBinder; recover(calledByDefer) not a divergence; GoroutineThreadPool; range(n), index-only forms, range(p, N) for nil *[N]T.

### CODE FINDING (inferred, not run)
- Pointer-receiver method value on an unboxed package var binds to a copy: `var g ctr; f := g.inc; f(); fmt.Println(g.a)` (Go prints 1) converts to `var f = Ꮡ(g).inc;`, and Ꮡ<T>(in T) (builtin.cs:2457) boxes a copy, so 0 expected. A local var is correct (heap(..., out var Ꮡl)). Probe a probe.

### Gates
- Docs repoguard ok; planted bad line named, restored byte-identical. docsamples unchanged (22 notes).

## docs/ConversionStrategies.md 2145-4324

### MATERIAL
- L2792 (Nil and Zero Values table, `var e Embedded`): OLD "The embedded field lives in a heap box." NEW "The constructor builds each embedded field." Evidence: visitStructType.go:977 `/*embed*/ public T T;`; UnsafeOperations.cs.target:38-42; commit 814ff707c7; new(nil) from visitValueSpec.go:426-431.
- L3023-3027 (struct that embeds another type, `new(nil)`): OLD embedded field in a heap box reached through a `ref` property, only a constructor allocates the box. NEW an embed is an ordinary field named for its type, marked /*embed*/; the NilType constructor builds each embed (a value embed by `new T(nil)`, a pointer embed as a nil ж<T> box rather than C# null, so a promoted access panics as Go does).
- L2731 (float literal keeps its Go spelling): OLD a float literal always gains C#'s D/F suffix. NEW except in a typed const declaration, which writes a float64 value bare and a float32 with `f`. Evidence: visitValueSpec.go:1393-1394; math/asinh.cs:49; runtime/stubs.cs:156; a probe.
- L3119 (Built-in Functions table, recover()): OLD `recover()` or `recover(calledByDefer)`. NEW also `recoverChecked()` (a function literal that is not itself the deferred call), and each form returns nil unless called directly by the deferred function, as in Go. Evidence: recoverForms.go:48-51; builtin.cs:386; DeferFinallyLowering/main.cs.target:122; merge a432e47926.

### EDITORIAL
- L2782 clause defining ж<T> and Ꮡ with a link to the glyph table.

### CODE FINDING (inferred from emission and template, NOT run)
- Named integer shift by a variable count: `type word uint64; w << s` emits `w << (int)(s)`; generated operator `operator <<(T value, int shift) => (T)value.m_value.Lsh((uint64)shift)` (src/gen/go2cs-gen/Templates/InheritedType/NumericTypeTemplate.cs:128). (a) unsigned count >= 2^31 (shl(1, 1<<32)): Go 0; (int) truncation gives count 0, result 1. (b) negative signed count: Go panics "negative shift amount"; (int)-1 -> (uint64) huge -> 0, no panic. Page text L2413/L2537 states the intended rule; left. Probe: a probe, output its output.

### Gates
- docsamples: 0 rewritten, 22 notes (baseline). Docs repoguard ok.

## docs/ConversionStrategies.md 4325-6523

### MATERIAL
- L5426 (Generics, after the totalArea explanation): OLD said only that a method-set interface constraint becomes a plain `where` clause naming the converted interface, nothing on an inline (anonymous) method constraint. NEW paragraph: an inline constraint with methods lifts to a named interface `<declaration>_<parameter>` (`func Show[T interface{ Name() string }]` declares `Show_T`, `where T : Show_T`); a self-naming constraint (`type Ord[T interface{ Less(T) bool }]`) lifts to a generic interface, `where T : Ord_T<T>`. Evidence: src/tests/Behavioral/InlineConstraintLift/main.go:22 -> main.cs.target:28,33; ord.go:7 -> ord.cs.target:10; src/go2cs/visitFuncDecl.go:475; merge 41adc5b67c. Note quoted; positive control done.

### EDITORIAL
- L4422 why the run() sample is partial (//go:noinline, SliceAliasing/main.go:15), link to #a-partial-method-with-a-body-keeps-its-own-frame.
- L4543 gloss for @string at first use, link to Strings.
- L5386 "(see the next rule)" -> named link.
- L6182 gloss for the ʗp suffix; two lines re-wrapped to <=120.

### UNVERIFIED
- L4431 "for every element type" (growslice rounding): GoSizeClasses.UnroundedElementTypes exception; stated on Reference slices-and-arrays.md:772; left.
- L4742 "one conversion per program run": cache eviction not checked.

### OTHER PAGES
- generic-constraints.md:4 "comparable maps to golib's CRTP comparable<T>" is stale (no clause emitted; constraintOperations.go:1295-1302, :2938; ReverseSortNaNOrder.cs.target:70); :761 calls it retired; :763-765 visible history ("Until the B1 per-kind box split (2026-08-26)").

### Gates
- docsamples: 0 drift, 22 notes (baseline). Docs repoguard ok before/after.

## docs/ConversionStrategies.md 6524-8824
### MATERIAL
- Orig 6980-6981 (Loops, index-only range reads no element): OLD "A pointer to an array passes its type's length, range(p, N), so a nil one ranges N times." NEW a pointer-to-array VARIABLE passes range(p, N) (nil ranges N times); a pointer PARAMETER or RECEIVER is a ref to the array and ranges as range(p): a nil one panics with Go's nil-dereference error where Go ranges N times. Evidence: rangeIndexOnly_test.go:262, :265; ж.PointerExtensions.cs:613; a probe (Go 3/4; C# panics, MEASURED on linux/amd64).
- Orig 8287-8288 (unrecovered panic): OLD only "panic: ... on stderr, exit 2". NEW the report opens with Go's panic chain ([recovered], [recovered, repanicked]) and a traceback with Go names/files/lines; links Limitations for shorter tracebacks. Evidence: CrashReport.cs:191-195, :275; Limitations.md:88-98.
- Orig 8539 (LockOSThread): OLD "holds trivially, each goroutine owns its thread". NEW holds because the goroutine owns its thread, and a goroutine that ends while locked retires its thread instead of handing it on. Evidence: GoroutineThreadPool.cs:150-154.
- Orig 8543-8544 (sync.Mutex): OLD "blocks its thread on a .NET semaphore". NEW "blocks its thread in golib's runtime semaphore, which waits on a .NET event". Evidence: internal/sync/runtime_impl.cs:126; RuntimeSemaphore.cs:53.
### EDITORIAL
- Orig 8301 "Each goroutine is a dedicated OS thread" -> "runs on an OS thread of its own, from start to finish".
### OTHER PAGES / CODE
- CODE: nil *[N]T param/receiver index-only range panics (Go ranges N). MEASURED linux/amd64: Go prints 3, 4; C# panics nil deref at main.go:5.
- labels-and-loop-variables.md ~334-339 does not state the nil divergence.
- defer-panic-recover.md:572-588 stale ("minus the goroutine stack-trace lines"; "It previously printed to stdout"; ~597 "the runtime used to shut down").
- No Reference page states inlined-function panic frames from IL (manual-conversions runtime.Stack section).

## docs/ConversionStrategies.md 8825-10974
### MATERIAL
- L9024 Struct Types: OLD "A struct that embeds another type is built with new(nil)." NEW "A struct that embeds a defined type or a pointer to one, other than an interface of its own package, is built with new(nil)." Evidence: visitValueSpec.go:426; visitStructType.go:1106-1176 (:1152).
- L10291 Pointers table: OLD &x -> Ꮡx "address of a local that lives in a box". NEW "of a local or package-level variable that lives in a box". Evidence: AnonymousStructs.cs.target:17-18; LazyPackageVars/tables/tables.cs:40-41.
- After L10347 Pointers, NEW paragraph: an address-taken package var is a static box ᏑX plus ref property X => ref ᏑX.Value; when lazily built its [GoLazy] holder holds the box, and &X is the same box before and after the first read. Evidence: 2e803726dd; LazyPackageVars main.cs.target:31; TestLazyPackageVarCarriesItsBox (lazyPackageVar_test.go:407).
- L10564 receiver rule: OLD "returns or compares its receiver, or takes a field's address, takes the box". NEW "returns, stores or compares". Evidence: ReceiverPointerValue main.cs.target initSelf `r.next = Ꮡr;`.
- L10702 Implicit Deref bullet: OLD v.M() and p.M() both stay as written, no exception. NEW a method that needs the pointer is called on a boxed variable's box: c.Set(100) -> Ꮡc.Set(100). Evidence: ReceiverFieldAddress main.cs.target:32.
- After L10836, NEW paragraph: exception to "~p reads, p.Value writes": a local made by (*T)(unsafe.Pointer(...)) where T holds a managed field, or assigned from a same-package function returning such a pointer over a byte array/slice, reads/writes fields by name through a generated property at Go's offset. Evidence: fieldAccess.go:22-56 (:43); 1d0d7e4869; FieldAccessReinterpret main.cs.target:52, :80; TestFieldAccessCrossesOneReturn.
### EDITORIAL: golib/go2cs-gen glosses (L8868, L9108-9110, L9376-9380, L9611, L10704); L9863 named link; L10405-10407 and L10684 partial-method //go:noinline clauses.
### OTHER: unsafe.Pointer section (~L11220) omits the one-return step; ж<array>.at<T>(i) makes a fresh ElemRefBox per call (performance only).

## docs/ConversionStrategies.md 10975-end
### MATERIAL
- L11267-11268: OLD internal/unsafeheader's tests pass under go2cs "on linux and windows". NEW "on linux/amd64" (no windows reading at go1.26.10; DATA-rebank-prep-c1-2026-10-22.md §2; PLAN-rebank-go126.md:442). Old claim kept in a platform comment.
- L11335 + new row (Source Generators table): OLD lede read as a complete list of single-marker generators. NEW lede "reacts to a marker: an attribute, a partial declaration, a receiver or parameter shape, or a literal in an any slot. These are the generators converted code shows most often:" plus a LazyHolderGenerator row ([GoLazy] holder -> the var's ref accessor). Evidence: LazyHolderGenerator.cs:16-44, :129. Comment lists the generators left to the reference.
- New paragraph after L11996 (Manually-Converted): a frame in a hand-owned file reports its C# position (GoPositionMap only for converted files), except each goǃ( line in a hand-owned file has a hand-kept record mapping to the Go go statement, so `created by` names the Go position. Evidence: positionMapOperations.go:560-565; sync/waitgroup.cs:22-28; net/windows/lookup_windows.cs:55; TestHandOwnedGoStatementsMapToGo.
- L12110-12111: OLD math/bits Add64/Sub64/Mul64 (Mul64 on Math.BigMul). NEW Add/Sub/Mul and their 64-bit forms (Mul and Mul64 on Math.BigMul). Evidence: manualTypeOperations.go:2764-2771; bits_impl.cs:74...:147.
- L12035-12036: OLD "Each go2cs goroutine is a .NET thread." NEW "keeps one .NET thread while it runs." Evidence: Goroutine.cs:1100; GoroutineThreadPool.cs:45,48.
### EDITORIAL: commandLineOptions.go re-cite :297-307; wraps.
### OTHER: CS.md:8284 comment "hand-owned file has no GoPositionMap record" no longer always true; source-generators.md:6-44 omits LazyHolderGenerator and RuntimeInitHookGenerator ("fourteen", now sixteen); manual-conversions.md omits goǃ( records and math/bits leaves; ValidatedTestPackages.md:1425 E3 text refuted on linux (re-bank); stale code comment bits_impl.cs:32-34.

## Reference: manual-conversions.md
### MATERIAL
- Top: added manualConversionVars, and a hand-owned var (and every var whose initializer reads it) is always built eagerly (manualTypeOperations.go:133; bf3033b816; lazyPackageVar_test.go:287, :440).
- Sockaddr (old 158): dangling "SOCKET-ADDRESS family / L10" reference -> syscall/windows/syscall_windows_impl.cs (header line 8).
- Mmap (old 162): OLD Mmap's slice is a snapshot. NEW golib's native-backed slice<T> makes unsafe.Slice over a native pointer alias the mapping (unsafe.cs:1027-1057, slice.cs:244-298); heading renamed (old anchor kept).
- StructField Anonymous (old 776-780), reflectlite field order (old 2484-2490), EMBEDDED section (old 2726-2764): OLD an embed is a partial property over a backing box. NEW an embed is a /*embed*/ field declared in place in Go declaration order, tag comment after it (visitStructType.go:941-977; sample ReflectBridgeClosure/main.cs.target:35-40); the partial-property form described as older converted code; removed the stale "declaration-order cargo, not a re-sort" remedy claim.
- StructField.Tag (old 2006, 2020): OLD tags reach metadata through GoTagAttribute. NEW through member records (GoMemberRecordAttribute.cs, GoReflect.MemberRecords.cs:113); [GoTag] is the hand-written form.
- New paragraphs: math/bits Add/Sub/Mul and Add64/Sub64/Mul64 hand-owned leaves (Mul, Mul64 on Math.BigMul; linux/amd64 figures; MathBitsLeafTests/MathBitsLeaves guards); hand-kept GoPositionMap records for goǃ( sites (TestHandOwnedGoStatementsMapToGo); a panic inside a JIT-inlined Go function keeps its frame, restored from the IL (managed_impl.cs:4609-4626; PanicFramesInlinedPanicTests.cs:14-19).
- testing.F cite testing.cs:890-904 -> :906-920.
- Detached testing.T: "Go 1.24.13" -> "Go 1.26.10 (linux/amd64)" (a probe). Init frame-name table headed Go 1.26.10 (probe3), covers eager vars; a [GoLazy] var's initializer runs in its holder's static constructor. Test-variant frame naming go1.23.12 -> go1.26.10 linux/amd64 (probe4). Breakpoint linux first line re-checked at go1.26.10 (probe1).
- HashTrieMap and weak amendments restated as the current surface (GOROOT 1.24.13 vs 1.26.10 effectively unchanged there).
- cgo_import_dynamic counts (Go 1.23.12) -> qualitative; old and 1.26.10 figures in a comment. "340 one-arg handles" -> "several hundred" (407 at 1.26.10 in comment).
- sigqueue path runtime/sigqueue.cs -> runtime/windows/sigqueue.cs.
- Goroutine threads: "thread-pool injection"/"Thread.Start captures" replaced: threads reused through GoroutineThreadPool.
- Panic backstop: report shape includes Go's panic chain and `goroutine N [running]:` traceback (CrashReport.cs; 9195a9cb41, 8db3213c27), link to defer-panic-recover crash-report section.
- Timer service: "time's timer service thread" -> golib's GoTimer service thread; one-heap statement present tense.
### EDITORIAL: fleet vocabulary (S1/S2, R1/R4/R5, L10, W1b, D2, D3, M1, r36, r39, r57c, r56d, Q49, COORD/ledger/lane/post-hop) to comments; "the r39d rule" -> "the inverse-atomic rule" (x6), "increment-6" -> "the gate-and-read rule"; roster figures (PASS 491, eight roster rows) to comments.
### HISTORY-KEPT: netpoller staging/supersession; Mmap snapshot-era probe; embed field-order history; timer/knob D2/D3/M1/r36/r39; cgo 1.23.12 figures; R/L/W/Q labels.
### UNVERIFIED: func literal frame inside a [GoLazy] initializer; auto implements on a promoted-embed box (old ~2179); darwin/windows breakpoint lines; variadic Native AOT claims.
### CODE COMMENTS stale (not fixed): golib/GoReflect.FieldAccess.cs:504-517 (embeds as partial properties), :411-424; math/bits/bits_impl.cs header ~32-36; managed_impl.cs goInitFrameName "Go 1.24.13"; tracked encoding/json/decode_test.cs:194 old embed shape (-tests output).
### NEW HEADING: sockaddr heading renamed; old anchor `the-sockaddr-family-on-linux--l10s-mirror-arm-for-arm-as-the-socket-pollers-prerequisite-and-mmaps-slice-is-a-snapshot` kept as <a id>; new `the-sockaddr-family-on-linux--the-windows-mirror-arm-for-arm-as-the-socket-pollers-prerequisite-and-mmaps-slice-aliases-the-mapping`; README.md:460 to update.

## Reference: pointers, implicit-dereferencing, multi-assignment, type-aliasing, purego
### MATERIAL (~40; highlights)
- purego.md:5: OLD Go 1.24.13 sha256block_amd64.go declares bodyless blockAMD64. NEW at 1.26.10 bodyless blockAVX2 and blockSHANI. :7 subtle xor_amd64.go -> xor_asm.go. :9 OLD math/big arith_decl.go eight bodyless functions and support_adx. NEW six (addVV, subVV, lshVU, rshVU, mulAddVWW, addMulVVWW) and hasADX. :13 vendor/…/sha3 -> crypto/internal/fips140/sha3. :17-35 crypto/elliptic re-check at 1.26.10 holds (no change).
- multi-assignment.md:417-436 (docsamples NOTE): sample re-cited to main.cs.target:71-74, 93-98; prose corrected: no box when the callee is ref-lowered. :30-37 strconv sample in no corpus file -> ParallelAssignmentHazard. :268-283 tabwriter flush sample matches tabwriter.cs:524-537 incl. recovers: true.
- type-aliasing.md:27: alias-array range emits `foreach (var (_, e) in w.ΔRangeSnapshot())` (AliasStructComposite golden :41), not `in w`.
- pointers.md:100: an addressed global built on first read keeps its box in the [GoLazy] holder (2e803726dd). After :120 NEW persistent element boxes paragraph (Slotted: one box per (backing, absolute index) in a weak table, 64-slot chunks; slice overloads fresh box; ElemBoxPersistTests).
- pointers.md: names that no longer exist corrected: CanonicalStorage/CanonicalElement (ElemRefBox classifies at construction), readSlotViaInterface -> GoReflect.ReadPointerSlot -> IGoReflectPointer.ReadReflectSlot, TryGetElementWindow -> ElementWindow.TryGet, canonicalRecordIfaceName -> canonicalImplementRecordIfaceName (which now COLLAPSES to the _package tail).
- pointers.md:351: captured for-init variable is not a remaining gap (per-iteration copy). :691 "back to nuint" -> "to golib's uintptr struct". :703-705 runtime.mapiterinit/hiter Sizeof guard gone at 1.26.10 (swiss maps). :715 slices.Compact -> slices' overlaps. :854 indirect key in internal/runtime/maps map.go:458. :896 bmap shapes gone -> mprof.go:248.
- pointers.md hand-own statuses corrected (Windows): ptrout locations; DnsQuery hand-owned; x509 verifier works (certchain_impl hand-owns CertGetCertificateChain etc.); netpoll contracts hand-owned (runtime_netpoll_impl.cs); WSARecvFrom hand-owned (zsyscall_windows_wsa_impl.cs:1246-1275, UdpLoopbackRoundTrip); readReparseLink and readReparseLinkHandle hand-owned; syscall.Readlink "identical defect, LATENT" stale; lookup_windows DNS walk safe.
- pointers.md:1166: "stub-fmt fallback for %v/%p" stale: PrintPointer backs ToString and print/println; %p through converted fmt.
- pointers.md samples re-cited: CopySid SyscallN form at 1.26.10; newIndirectNode from internal/sync/hashtriemap.go; span sample -> PointerCastSliceReinterpret. :2540 every pointee shape takes the box form. :832-834 sha3 scoped to Go 1.23; "Since Go 1.24".
### EDITORIAL: dates, lanes, rulings, fleet terms, corpus figures to comments; tense; links; Windows named for Windows-only claims; two illustrations labelled. multi-assignment.md:483 wrapped link fixed.
### UNVERIFIED: pointers.md:2093-2103 createMountPoint fresh -tests emission; :818 syscall.Readlink runtime; type-aliasing.md:22 illustrative.
### RE-CITES OWED in CS.md: :10347 pointers.md:320#ca6b44 -> :341; :10559 :168#a48ecd -> :189 (text changed), :180#8a1144 -> :201 (changed), :195#25611d -> :216, :199#3edc27 -> :220 (changed); :11269 pointers.md:2246-2255#cc40fb -> :2268-2277.
### NEW HEADINGS: pointers "These three arms ALIAS instead of boxing a copy" (old #these-three-arms-now-alias-instead-of-boxing-a-copy-2026-08-03-r38-gob kept); pointers "Nine coupled rules from the flag, flate, binary and syntax packages" (old #the-club-41-mop-up-batch-flagflatebinarysyntax-roots kept); purego heading renamed earlier, README/index show the old title.
### CODE COMMENT: commandLineOptions.go:291 names xor_amd64.go.

## Reference: interfaces, empty-interface, type-switch, value-receiver-delegates, source-generators, golib-namespace
### MATERIAL (highlights; ~40)
- interfaces.md:1614 drifted sample rewritten: zvars golden holds `[GoLazy] partial class lazyᴛcompareTests { … }`; sentence + link on the lazy holder.
- source-generators.md: list omitted LazyHolderGenerator (GO2CS0009) and RuntimeInitHookGenerator; both added; count comment "sixteen at 3e3f18925a". TypeGenerator entry adds IGoMethodSource (MethodSource.cs), IGoMethodSet (MethodSet.cs), MethodSetKeep's [DynamicDependency] on ToString.
- type-switch.md:141: OLD each adapter registers from a [ModuleInitializer]. NEW the initializer queues the package's registration; golib runs it on the registry's first miss. type-switch.md:157 dropped "a generic class cannot host a module initializer". interfaces.md:184 "Registration is startup-time" -> at the first registry miss.
- Named-interface assertions and type-switch labels resolve at run time through AdapterBinder; TryTypeAssert tier order corrected: case T -> IжAdapter/IValueAdapter unwrap -> itab -> AdapterRegistry -> Implements<T> -> shell construction; the IGoAdapter marker covers IValueAdapter.
- interfaces.md:205: OLD three hand-written interfaces (error/fmt.Stringer/io.Reader). NEW only error (golib/error.cs:35). interfaces.md:235: OLD GetGoTypeName serves "stub fmt %T / testing shim". NEW its callers are runtime panic texts; value adapters detected by the IValueAdapter marker, not a ᴠ-infix name probe (GoReflect.cs:398-420).
- Go 1.24 shapes rewritten for 1.26.10: (*timers).run delegate `(Func<ж<timers>, int64, ж<synctestBubble>, int64>)(run)` (runtime/time.cs:1261); hpke kdf.hash.New -> crypto/tls c.hash.New into tls13.ExpandLabel (key_schedule.cs:25-62); hpke SupportedKDFs labelled Go 1.24; strconv NewDecimal lives in internal/strconv/export_test.go.
- Stale samples rewritten from the tree: parseControl(rangeˢ) (parse.cs:752-753); HeapKeywordVar StandardBox form; SystemCollidingTypeName records (old GoImplicitConv pointer-box record no longer arises, GO2CS0008); gob -tests anchors (package_info_internal_test.cs at gob_internal_test_package, package_test_info.cs at gob_test_package); `((image.Image)new image.ΔRGBAжImage(Ꮡdst))`; `unchecked((nint)…L)`; (*parse.Tree).ErrorContext.
- go2cs's testing package is hand-owned (examples built on converted testing.T corrected).
- Uncomparable-array residual: Go side re-measured at go1.26.10 linux/amd64: all four shapes panic (self-compared and distinct).
- Broken in-page references fixed (multi-hop value-embed chains -> struct-embedding.md; "GUARD OWED above"/"append-disambiguation root above" to comments).
### EDITORIAL: ~200 wording edits; ~25 dated bold leads and fleet vocabulary (B3..B9, X3, r27, r39d, r56g, R5, Tier-C, wave 1, Phase-1/4, BOARD, GUARD OWED, banked, rulings) to comments; point-in-time verdict/corpus figures to comments; platforms named (PerfIface windows/amd64 .NET 9; Tour "Hello, ??" windows/amd64).
### HISTORY-KEPT: 15 comments (dyn ᴛAs renderer, name-only structural probe, two-dictionary ladder, stub-era As<T>, go.runtime namespace origin, fmt stub CRLF, etc.).
### UNVERIFIED: array-of-any residual go2cs side (array.cs:874-880 code-read); interfaces.md:239 valued(d) InvalidCastException; :1726-1733 external variant partition merge; :974-1007 internal/strconv NewDecimal internal at 1.26; :174 Native AOT fmt-only growth 3.2 MB vs MethodSet.cs remarks +2.59 MB (two measurements).
### OTHER: stale tracked 1.24 corpus files src/core/strconv decimal_test.cs, internal_test.cs, fp_test.cs (moved to internal/strconv in 1.26); README.md:443 golib-namespace heading; multi-result :43 history block.
### NEW HEADING: golib-namespace.md "The reflection bridge answers a read where the answer EXISTS" (#the-reflection-bridge-answers-a-read-where-the-answer-exists); old #...--four-members-that-did-not-2026-08-19 kept.

## Reference: package-conversion, slices-and-arrays, strings, compiled-library, deterministic-output, packages-that-do-not-type-check
### MATERIAL (34; highlights)
- package-conversion.md:450-454: OLD manualConversionFuncs keyed by NAME, platform-blind. NEW every entry carries a GOOS scope (goosScope); File.readdir scoped windows+darwin (manualTypeOperations.go:41-64, :505, :1775).
- package-conversion ~526: OLD -platforms merge takes everything from the first target in -platforms order. NEW mergePlatformProjectFile reads only converting targets; carriesConvertedPlatformAxis re-merges a per-GOOS block even when copies are identical; companions from the first converting target (platformEmit.go:866-900, :985-1040, :1155-1167).
- package-conversion:214: nuget props default `<GoStdLibVersion>1.26.*</GoStdLibVersion>` (converting toolchain's minor); restore takes the newest release of the minor; setting the property pins it (toolchainResolution.go:511-526; moduleConverter.go:903-913).
- package-conversion ~1326: GoCompileRuntimeIdentifier adds darwin -> osx-x64/osx-arm64 (moduleConverter.go:996-1016).
- package-conversion ~866: OLD a type satisfying an interface without a cast gets no glue (duck-typing shells). NEW exported-interface satisfactions recorded without a cast; others resolve through AdapterBinder shells.
- package-conversion ~1086: OLD package vars are static field initializers. NEW field initializers plus the ordered static ctor; a pure-data var built on first read.
- package-conversion ~387: OLD GoTargetOS default exists because the corpus is the Windows emission. NEW windows is the reference target; -p:GoTargetOS=linux|darwin selects another flavor.
- package-conversion:283-300: Go 1.24 `package macOS` example -> crypto/internal/entropy/v1.0.0 -> entropy_package. ~888: dotted-tag example CoverageRedesign (Go 1.24) -> goexperiment.GreenTeaGC (Go 1.26). ~898 release-tag text generic. ~597 "Three guards" listed two -> "Two guards". ~576 added PublishReadyToRun applies to dotnet publish only (csproj-template.xml:57-59). 1704-1712 L10 probe table -> statement; count to comment.
- slices-and-arrays: three stale "known gap" claims closed: zero-var local now `holder z = new();` (ZeroValueStructVar main.cs.target:23); named-array zero value (NamedArrayZeroValue); `[][4]int` with nested element literals composite literal takes the same factory (CompositeLiteralElements main.cs.target:99). :24 OLD slice nil-compare conflates nil with empty; NEW `pm{} == nil` is false as in Go (slice.cs:1243). Removed stale ZERO-VALUED struct array-field null clause.
- slices-and-arrays: append samples now show the stack buffer args (packᴷ, mapsᴷ, ptrsᴷ) from goldens (AppendNilSliceElement main.cs.target:14,19,23; InterfaceCasting.cs.target:161-162).
- slices-and-arrays ~1696: Go 1.24 p256Curve/mlkem768 and a wrong emitted type -> CrossPkgUser golden `ж<Holder<ж<CrossPkgLib.Sensor>>>`, Go 1.26 `var p224/p256/...` emitting `ж<nistCurve<P224PointжnistPoint>>` (crypto/elliptic/nistec.cs:14).
- slices-and-arrays ~250: named-result sample now the current emission (netip.cs:734-736; crypto/tls/common.cs:891, :899).
- slices-and-arrays: NEW persistent element boxes paragraph: &a[i] gets one box per (backing, index) when the element type is BoxShape<T>.Slotted, held in a weak table in 64-slot chunks; other types get a fresh box (ж.ElemRefBox.cs:142-190; builtin.cs:2676-2706; ElemBoxPersistTests). :39 adds the index-only range(p, N) rule (nil *[N]T ranges N times) and the receiver form range(b).
- packages-that-do-not-type-check:10: OLD TypeOf hands back nil, every site tolerates nil. NEW recordUntypedOperands records types.Typ[types.Invalid] (untypedOperands.go:18-31). :9 adds the GO2CS1003 per-error line. :14 adds guard untypedRegionShapes_test.go. :3 summary link anchor fixed.
- deterministic-output:7 note re-cited conversionDriver.go:550-561 -> :539-550.
### EDITORIAL: ~120 (fleet vocabulary, ~10 dates, history tense, figures to comments).
### HISTORY-KEPT: ~45 comments.
### UNVERIFIED: &ptr.items[i].buf[j] gap (emits Ꮡ(~h).items[i].buf.at<nint>(j); CS1061 claim dropped, gap kept); semTable generated backing; ~300 ms subprocess and MAX_PATH figures (Windows named); Sockaddr suppression measurement.
### OTHER: ConversionStrategies.md:1028 cites package-conversion.md:431 -> now :536; strings.md left unwrapped so CS.md:8117 strings.md:73#0b25b7 stays valid; stale code comment visitImportSpec.go packageKeepsDirectorySegment (macOS -> macos).

## Reference: struct-types, struct-embedding, defer-panic-recover, nil-and-zero-values
### MATERIAL
- defer-panic-recover.md:574-582: OLD crash report is `panic: <message>` only, "minus the goroutine stack-trace lines"; goǃ on "its own dedicated thread". NEW Go's panic chain ([recovered], [recovered, repanicked]), blank line, `goroutine N [running]:`, traceback with Go frame names and positions; inlined explicit-panic frames restored; shorter in some shapes (Limitations.md); reused goroutine threads. Evidence: CrashReport.cs:192, :275, :196-224; managed_impl.cs:171; GoroutineThreadPool.cs; Limitations.md:88-98.
- defer-panic-recover.md:301-303: OLD wall "empty variadic passes empty slice where Go passes nil". NEW not a wall; parts == nil is true as in Go (slice.cs:1847-1878 IsNilSpan; a probe).
- defer-panic-recover.md:683-686: OLD behavioral tests build against baseline src/core with no runtime package. NEW DivideByZeroPanic prints the same either way; typed path guarded by math/bits TestDiv32PanicZero (bits_test.cs:1303; runtime/panicvalues_impl.cs).
- defer-panic-recover.md:714: OLD computed string from "a stub (the baseline fmt.Sprintf does)". NEW from hand-written C# (builtin.cs:293-311).
- defer-panic-recover.md:1096: OLD pprof sample `defer(Ꮡb.of(profileBuilder.Ꮡdeck).reset, ref ᒐ)`. NEW golden `defer(Ꮡb.of(builder.Ꮡc).reset, ref ᒐ)` (DeferValueFieldPtrReceiver main.cs.target:28); pprof now takes the finally lowering (proto.cs:694).
- struct-types.md:117-128 drifted sample rewritten: ptrElems inside `[GoLazy] partial class lazyᴛptrElems` (AnonStructComposedTypes main.cs.target:8-16).
- struct-types.md:1309-1312: OLD reflect.MakeFunc of a variadic func throws NotImplementedException. NEW built via makeVariadicFunc{N}/makeVariadicAction{N} (reflect/makefunc_impl.cs:67-73; GoReflect.MakeVariadicDelegate.cs).
- struct-types.md:1144-1154: OLD html/template variadic literal defer fails CS0411 "blocking 243 verdicts". NEW cast to its family delegate (html/template/examplefiles_test.cs:117).
- struct-types.md:537, :919-920: removed claims that go/* packages still need residuals to compile and go/doc's remaining block (moved to comments).
- struct-types.md:338-339: OLD IFormattable impl is "the hand-finished io stub's dyn machinery". NEW only a hand-written interface can inherit it (ImplementGenerator.cs:482).
- struct-types.md:24: OLD Go 1.24 crypto/internal/hpke SupportedKEMs example. NEW the guard's map<uint16, kindsᴛ1> (MapAnonStructValue main.cs.target:8-16); go1.26.10 has no crypto/internal/hpke.
- struct-types.md:558-567: OLD generic embed example from Go 1.23 internal/concurrent. NEW internal/sync HashTrieMap; Go 1.26 unique embeds *canonMap[T] (unique/handle.cs:70); "BANKED:" -> "Known gap:".
- struct-types.md:726 NOTE: re-cited internalTestLocalInterfaceStamp_test.go :87/:101 -> :88/:102 as an illustration note with quotes.
- struct-embedding.md:6-17, 21, 45, 49, 53, 144-168: OLD generator-owned `partial ref` accessor / ʗT backing field as the embed form. NEW every embed is a converter-declared /*embed*/ field (internal when unexported); the property form is legacy, still read for hand-owned files (visitStructType.go:969-977; StructDeclarationSyntaxExtensions.cs:625-677).
- struct-embedding.md:533-548: OLD every embed of a NAMED type keeps its generated promotion shape. NEW every embed carries /*embed*/; go2cs-gen promotes only through embeds whose type has members (visitStructType.go:947-976).
- struct-embedding.md:606-612 illustration replaced by real emission (io/io_test.cs:24-28) as a source sample; :614-618 wording follows.
- struct-embedding.md:659-660: OLD `new ж<T>(nil)`. NEW `new StandardBox<T>(nil)` (ж<T> is abstract, ж.cs:88).
- struct-embedding.md:59: OLD Go 1.23 sha3 cshakeState embeds *state. NEW the guard's Outer/*Inner shape (PointerEmbedBoxReceiver golden).
- nil-and-zero-values.md:416-423: OLD assignDescriber(ж<holder> Ꮡh, …). NEW `assignDescriber(ref holder h, ж<Setting> Ꮡs)` source sample (PointerInterfaceStructField main.cs.target:25-27, :43).
- nil-and-zero-values.md:303-307: OLD the emission side of typed nil "still collapses" (open). NEW covered by the OrTypedNil boundary rule (symbols.go:123; TypedNilInterface.cs.target:85).
### EDITORIAL: ~45 "now"/history fixes; dated stages and fleet terms (B2, Phase C, phase 1a, Phase-4 row R6, wave-1, Charter §9, B10b, IMP-1/2/HTTP-3, GUARD OWED, banked) and figures (243, 137 of 139, 557 verdicts...) to comments; an unplatformed 1.2-1.8 ms figure to a comment.
### HISTORY-KEPT: crash backstop stdout/Exit(0); empty-variadic wall; stub fmt.Sprintf; owner ruling (A) 2026-10-16; Go 1.23 sha3; Go 1.24 hpke; CS0411 html/template history; MakeFunc NotImplementedException; emission-side typed nil.
### UNVERIFIED: struct-embedding:51 cross-package promoted value-receiver gap; struct-types:567 unqualified promoted calls through a generic embed; struct-embedding:55 foreign sync.Mutex+Locker adapter; defer-panic-recover:1094 zc.Unlock exclusion; CS9244.
### CODE COMMENTS stale: ImplementGenerator.cs:482, ValueAdapterImplTemplate.cs:89, builtin.cs:296, builtin.cs:85-90; dead constant symbols.go:94 NilSafeDerefAccessor; possibly stale Limitations.md:97.
### NEW HEADINGS (dates dropped, old anchors kept): struct-embedding "An embedded PREDECLARED type is a plain field marked /*embed*/" (old #...-2026-09-05; README.md:317, ConversionStrategies-Reference.md:283); struct-types "reflect.MakeFunc is Value.Call's exact inverse — ..." (old #...-2026-08-29; README.md:303, ConversionStrategies-Reference.md:269).

## Reference: generic-constraints, shadowing, maps-and-channels, labels-and-loop-variables, expression-switch
### MATERIAL (30; highlights)
- generic-constraints.md:4: OLD comparable maps to golib's CRTP comparable<T>. NEW emits no C# constraint (ReverseSortNaNOrder.cs.target:70; constraintOperations.go:1295-1302); comparable section restated present tense.
- generic-constraints: crypto/internal/mlkem768 -> crypto/internal/fips140/mlkem (x2; field.go:129, field_test.go:81); internal/trace shift-count sample `… ConvertToUInt64<EI>((id % ConvertToType<EI>(8))))` (internal/trace/base.cs:166); re-open paragraph names CS1929 and recursion through nested generic args (ImplementGenerator.cs:283-303, :2389-2420); refused proxy shapes are fatal to a conversion, so the stdlib reaches none (conversionDriver.go:305-308); p256Curve marked Go 1.24 source (Go 1.25+ p256 = &nistCurve[*nistec.P256Point]); HashStr returns (uint32, uint32) sourced to bytealg.cs:35-36; rangeNum Go 1.26 form `rangeNum<int8, int64>(v.Int(), v.Type())` (reflect/iter.cs:51).
- labels-and-loop-variables: known divergence added (nil *[N]T param/receiver index-only range panics where Go ranges N; measured linux/amd64); defer/go capture cites DeferLoopCapture main.cs.target:52-56; two `defer http2bufPools[index].Put(…)` production sites (h2_bundle.go:9290, :9294); ForVariants removed from per-iteration goldens list; array snapshot heading `.Clone()` -> `ΔRangeSnapshot()` (ArrayRangeSnapshot.cs.target:18).
- maps-and-channels: OLD nil channel uses SendIsReady/ReceiveIsReady null checks (only in src/archived). NEW the struct holds a core reference and every probe checks for the absent core (channel.cs:1574-1610); removed "io suite remains blocked / Δio never aliased" (io_test.cs:19); two "Known residual" paragraphs refuted by probes (alias type args rooted, PackageAliasRootedTypeArgs; local NodePtr conversion emits `new main_NodePtr(Ꮡ(new Node(V: 9)))`, `(main_Tally)m` without outer parens); Go 1.23.1 AST scan redone at 1.26.10; "~100 µs" thread cost (no platform) to comment; pointer-element send rules present tense (visitSendStmt.go:133-144).
- expression-switch: else-drop sample was pre-fix emission; now exec_windows.cs:42-52; break/fallthrough after-shape sourced (time/format.cs:1312-1319).
- shadowing: mlkem768 -> fips140/mlkem; exported named-alias set redone at 1.26.10; "io/fs does not yet validate" and "log does not bank" to history comments (both validated); dangling section reference removed.
### EDITORIAL: ~120 sentences retensed; fleet vocabulary (B1..B10, F6, F14b, F15b, R10, L11, r38-gob, r43f, lanes, board, banked, Phase-4/4D, option-a ruling, GUARD OWED, CNR) to comments; ~25 dates to comments; figures to comments; ~14 above/below references -> links; two illustrations labelled.
### UNVERIFIED: ExampleChunk_People; generated Equals samples; new main_NodePtr not compiled; shadowing ~201-545 -tests narrative mechanics.
### OTHER: mlkem768 at interfaces.md:955, pointers.md:848, slices-and-arrays.md:1696 (also p256Curve); stale code comment constraintOperations.go:2936-2937; inconsistent casts internal/trace/base.cs:161/:165/:166.
### NEW HEADINGS: labels "range over an ARRAY VALUE iterates a COPY — the snapshot is the range EXPRESSION's ΔRangeSnapshot()" (old #...-clone kept); shadowing "An Example/Benchmark-ONLY test file is dropped from the compile set" (old #...-phase-4d-file-exclusion kept; README.md:125).

## docs/ConsumingGoFromCSharp.md and Reference: comments, named-numeric, README, constants, native-narrow, floating-point, var-init-order, multi-result
### MATERIAL (pre-edit lines)
- comments.md:636 drifted sample re-cut: now `[GoLazy] partial class lazyᴛunitMap { internal static map<@string, uint64> unitMap = …` (time/format.cs:1771) with a link to the lazy-var rule.
- variable-initialization-order.md:39: OLD a moved var with a multi-value inner call is the "one remaining warn-and-stay-inline fallback (globalDeclHoist)". NEW it relocates; the spill is a local of its initᴛ method. Evidence: visitValueSpec.go:771-782, convExprList.go:285; a probe (no warning).
- variable-initialization-order.md:108: OLD GOARCH, GOOS, GO386, GO_LDSO, Version each envOr. NEW only GOARCH, GOOS, GO386 call envOr; GO_LDSO and Version are plain reads. Evidence: internal/buildcfg/cfg.cs:28,52,56.
- variable-initialization-order.md:72: `Symbols.PackageTestInitHookMethod` -> `PackageTestInitHookMethod` in symbols.go:166.
- variable-initialization-order.md:165/175/181: lazy eligibility "never assigned or addressed" (lazyPackageVarOperations.go:187); method-value case f := X.M stays eager (:157-163); _test.go vars stay eager (:120-123).
- ConsumingGoFromCSharp.md:253-256: OLD "One gap: generic println prints String()". NEW removed: print/println print the underlying value, type parameters included. Evidence: builtin.cs:3171-3172, GoPrintText.cs:31, PrintFloatArgumentTests.cs:78, 45c89027f5; a probe emits println((v).OrTypedNilOf()).
- ConsumingGoFromCSharp.md:156: `public static Duration ΔSecond` -> get-only `ΔSecond => 1000000000` (time.cs:958).
- constants.md: narrow tightened-const shift `(byte)(cb << (int)(k))` -> `((byte)cb).Lsh(k)` (UntypedConstDefine main.cs.target:75); "static readonly Untyped* wrapper" -> "Untyped* wrapper property" (utf8.cs:17); iota `ΔKind Invalid = /* iota */ 0` -> `=>` (reflect/type.cs:298); "the uintptr alias" -> golib's uintptr struct; `unchecked((Class)…)` -> `unchecked((ΔClass)…)` (bidi/trieval.cs:37); UL literal statement present tense citing fips140 sha512 _K (sha512block.cs:15); removed a named guard ("resolveBuildTags/convBasicLit converter unit coverage") that does not exist.
- native-and-narrow-integers.md:28 narrow untyped const shift `((byte)cb).Lsh(k)`; :34 `(int64)(sbyte)(c)` -> `(int64)(int8)(c)` (NegativeShiftCount main.cs.target:51); :66 rngCooked sample shows its [GoLazy] holder (math/rand/rng.cs:21-22).
- named-numeric-types.md:330 two32 sample `internal static UntypedInt two32 => …` (UntypedConstArithmetic main.cs.target:7,16); :197 runtime/internal/math -> internal/runtime/math (x2).
- multi-result-and-comma-ok.md:48 TryTypeAssert gate runs before a shell is built (ᴛAs retired, TypeGenerator.cs:501-505); :50 removed nonexistent fmt.Stringer.As<T> (only error.As<T>, golib/error.cs:43); :63 edwards25519 inline example (it relocates) -> golden defaultBox = makeBox(7).Item1 (GlobalTupleVarDecl.cs.target:22).
### EDITORIAL: dated bold leads and fleet vocabulary (Phase-4, B7a, charter §9, L11, REC-C, IP-4, banked, ~20 dates) moved to comments; "no longer"/"previously"/"now" rewritten.
### HISTORY-KEPT: native-narrow TARGET32BIT strategy comment; multi-result ᴛAs retirement and itab-cache folding.
### UNVERIFIED: nugetgo uuid/hashset publication (external); println fix from code; des/nistec UL tables; point-in-time "cleared N CSxxxx" asides.
### OTHER: README headings with fleet vocabulary from shadowing.md "(Phase-4D file exclusion)" and pointers.md "club-41 mop-up batch"; README missing: defer-panic-recover.md#only-the-function-the-defer-calls-directly-recovers, labels ΔrangeSnapshot heading, manual-conversions sockaddr linux heading, struct-embedding.md#an-embedded-predeclared-type-is-a-plain-field-marked-embed; guard failure multi-assignment.md:483 (wrapped link) is gR2's.
