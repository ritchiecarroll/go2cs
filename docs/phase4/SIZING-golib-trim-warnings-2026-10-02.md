# SIZING — golib's trim-analysis warnings (IL2xxx), 2026-10-02

Point-in-time record. Sizing only: nothing here is cut. Source read at `483d4ea217` (TRAIN M row 25);
the record count is re-measured at the TRAIN M union `e2008427b1` in *Count* below.

## What is being sized

golib sets `PublishTrimmed=True` unconditionally, so the SDK runs the trim analyzer on every golib
build and reports IL2xxx records. Each record is classified as one of:

- **(i)** a true `[DynamicallyAccessedMembers]` annotation, with where it propagates;
- **(ii)** `[RequiresUnreferencedCode]` / `[RequiresDynamicCode]`, with the call sites that would newly warn;
- **(iii)** `[UnconditionalSuppressMessage]`, with the justification it would carry.

## Count

**78 records at 70 source positions**, on both trees. A golib Release build with `--no-incremental`
at the union reports 70 distinct `file(line,col): IL####` positions; one of them,
`GoDelegateSynthesis.cs:155`, carries ten records (five IL2026 and five IL2111, one per member), which
is the difference. Per file and per code the union's set equals the set read at `483d4ea217`. golib
differs between the two trees in 24 files, so the line numbers in the tables are that tree's and may
sit a few lines off at the union.

By code at the union, by position: IL2070 23, IL2067 12, IL2075 11, IL2026 6, IL2055 4, IL2060 4,
IL2090 4, IL2091 3, IL2059 1, IL2072 1, IL2111 1. No IL3xxx record: the AOT analyzer is not enabled.

## The fact every justification rests on

No project in the tree declares `IsTrimmable`, `IsAotCompatible`, a trimmer root or an ILLink
descriptor, and golib has no `[DynamicDependency]` and no `RuntimeFeature.IsDynamicCodeSupported`
guard. The **only** thing that keeps reflected members alive is `TrimMode=partial`, set in
`src/tests/Performance/Directory.Build.targets` for the Native AOT performance publish: under it golib
and every `go.*` assembly are kept whole and only framework assemblies are trimmed.

That property is not set anywhere else. The converted-project template sets `PublishTrimmed=True` for
every non-library project and no `TrimMode`, so a converted application published outside the
performance tree takes the SDK's default mode. **Finding 1 (not measured by a publish):** if that
default trims every assembly, the "silent" rows below are live for any published converted
application, and none of the 71 suppressions would be true for it. The suppressions are honest only
once the keep is a declared property of golib and the template rather than of one test tree.

Where the analyzer runs: golib; every non-library converted project (the behavioral projects and the
performance projects, the latter with `SuppressTrimAnalysisWarnings`). No corpus library runs it, so a
"corpus" count below is call sites that exist, not warnings a corpus build prints.

## Totals

| class | records | edit sites |
|---|---|---|
| (i) annotation | 5 | 3 type parameters on 3 conversion helpers; 1 flag on 6 assert declarations; 2 `ZeroFacts` declarations |
| (ii) requires-attribute | 2 | `TryTypeAssert(object, Type, out object)`; the four `NilType<T>` operators |
| (iii) suppression | 71 | about 45 methods |

No record needs `T` annotated on `slice<T>`, `ж<T>` or `array<T>`. Closing either type-parameter chain
by annotation instead would touch about 136 of golib's 444 generic declarations (the fields chain) or
about 148 (the constructor chain), 239 / 254 corpus generic declarations and 69 / 81 behavioral ones
(lower bounds from a heuristic fixpoint count). The `GoZero<T>()` remarks in `builtin.cs` already rule
against that cascade.

## Table

Counts in "if (ii) instead" are golib / corpus / generator call sites, by source-line grep.

### Reflection bridge — `GoReflect.*` and `FinalizerBinding` (25 records, all (iii))

One justification covers 22: the reflected type is a converted or generated Go type in an assembly
that is not trimmed. The other three name golib's own members.

| record | code | suppress on | members kept by | if the member were trimmed |
|---|---|---|---|---|
| ValueMarshalling.cs:95 | IL2070 | `CanonicalNilPointer` | generated `NilInstance` | silent: an untyped null instead of the typed nil |
| ValueMarshalling.cs:193 | IL2070 | `IsNilGoValue` | `op_Equality(T, NilType)` on Go types | silent: answers "not nil" |
| ValueMarshalling.cs:497, :504, :509 | IL2067, IL2070, IL2075 | `tryCopyGoStructFields` | Go struct's ctor and fields | silent: a partial copy that returns true; a struct built without its field initializers |
| ValueMarshalling.cs:555, :571 | IL2067 ×2 | `ZeroValueOf` | Go struct's parameterless ctor | silent: zeroed without initializers |
| ValueMarshalling.cs:627, :634 | IL2067 ×2 | `MakeSizedArray` | golib's `array<E>` ctors | loud under AOT for an instantiation not precompiled |
| ValueMarshalling.cs:898 | IL2070 | `wrapperConstructorOf` | generated wrapper ctor | silent: the named/unnamed assignability rule disappears |
| ValueMarshalling.cs:920 | IL2075 | `TryUnwrapWrapperValue` | generated `m_value` | guarded: reads as "not a wrapper" |
| GoReflect.cs:191 | IL2070 | `ValueAdapterWrappedType` | adapter shell's `m_value` | silent: the shell class is reported as the dynamic type |
| GoReflect.cs:370 | IL2070 | `StructFieldsComparable` | Go struct fields | silent: comparable = true |
| GoReflect.cs:398, :421 | IL2070 ×2 | `TryAdapterWrappedType` | adapter ctor | guarded: classifies as Pointer |
| GoReflect.cs:541 | IL2070 | `ContainerInterfaceArguments` | golib container interfaces | guarded: no element type |
| FieldAccess.cs:358 | IL2070 | `collectGoFields` | Go struct fields | silent: the struct reads as `struct{}` |
| FieldAccess.cs:471 | IL2070 | `reorderToGoDeclarationOrder` | generated all-fields ctor | silent: metadata order, wrong `Field(i)` |
| FieldAccess.cs:531 | IL2070 | `embedTagOf` | embed property carrying `[GoTag]` | silent: an untagged embed |
| FieldAccess.cs:625, :643 | IL2070, IL2075 | `buildFieldAccessor` | golib's `ж<T>.ValueSlot` | trim: no. **AOT: broken — see Finding 3** |
| TypeNaming.cs:461, :467, :469 | IL2070 ×2, IL2075 | `goInterfaceTypeString` | lifted interface's methods | silent: prints `interface {}` |
| FinalizerBinding.cs:252 | IL2070 | `tryPointeeOf` | generated `m_value` | guarded, loud: the finalizer argument is rejected |

If (ii) instead: `TryAdapterWrappedType` sits under `KindOf` (49 / 30 / 0), `ElementType` (23 / 27 / 0)
and `GoTypeName` (36 / 32 / 0), so the attribute would surface on some sixteen public entry points and
through every hand-owned reflect bridge file. Four of the records are inside `static t =>` lambdas;
whether a method-level suppression reaches a lambda body is not confirmed by a build.

### Dynamic construction — `TypeLayout`, `MethodSets`, `MakeVariadicDelegate`, the two synthesis files (26 records, all (iii))

| record | code | suppress on | why it is trim-safe | AOT |
|---|---|---|---|---|
| TypeLayout.cs:746 | IL2070 | `namedSliceValueField` | wrapper's `m_value`, Go type | — |
| TypeLayout.cs:796, :797 | IL2075 ×2 | `firstMapEntry`, with `[DynamicDependency]` on `KeyValuePair<,>` | **a framework type, trimmed even in partial mode**; the only record needing a real keep | silent null dims |
| TypeLayout.cs:901, :1224, :1265 | IL2067 ×3 | `FieldArrayDims`, `FieldChanDir`, `FieldChanCargo` (one shared lambda) | Go struct's ctor | silent wrong dims if the ctor were trimmed |
| TypeLayout.cs:1287 | IL2070 | `TryFuncShape` | a delegate's `Invoke` is always kept | — |
| TypeLayout.cs:1388, :1392 | IL2055 ×2 | `makeGoResultType` | `ValueTuple` parameters carry no requirement | throws for an instantiation not pre-generated; `reflect.FuncOf` turns it into a Go panic |
| TypeLayout.cs:1575, :1608, :1610 | IL2070, IL2055, IL2060 | `buildVariadicInvoker` | delegate `Invoke`; golib's own trampolines | unguarded, reached from `reflect.Value.Call` on a variadic func |
| TypeLayout.cs:1621 | IL2026 | `rebindToVariadicFamily` | `Invoke` on a live delegate | — |
| MethodSets.cs:645 | IL2055 | `InstantiateFamily` | golib's delegate families | a `NotSupportedException` escapes the existing catch |
| MakeVariadicDelegate.cs:58 | IL2060 | `BuildVariadicMakeFactory` | golib's own trampolines | unguarded, reached from `reflect.MakeFunc` of a variadic type |
| GoDelegateSynthesis.cs:155 | IL2026 ×5, IL2111 ×5 | `mint` (two attributes clear all ten) | members of `MulticastDelegate` pulled in by `DefineType`'s parent | Reflection.Emit — see Finding 3 |
| GoStructSynthesis.cs:554 | IL2075 | `emitOne` | golib's box `Value` | Reflection.Emit — see Finding 3 |

Three of these (`TypeLayout.cs:1388`, `:1392`, `:1608`) fire only because the generic definition is not
statically visible; naming it by `typeof` per arity may clear them with no attribute (not verified).

### Builtins, boxes, zero and layout facts (22 records: 5 (i), 2 (ii), 15 (iii))

| record | code | class | change | if the member were trimmed |
|---|---|---|---|---|
| builtin.cs:1589 | IL2090 | i | `PublicParameterlessConstructor` on `T` of `ZeroFacts<T>` and `ZeroIsDefault<T>` — or route `ZeroFacts<T>` through the registered `GoZeroFactory<T>` and remove the reflection | silent: `clear` and `GoZero` return `default` |
| builtin.cs:2013 | IL2070 | iii | `OrderedFacts<T>.Classify` | silent: `min`/`max` lose NaN and -0 rules for named floats |
| builtin.cs:2994 | IL2075 | iii | `IsProcessStandardError`; reads a private field of a framework type | guarded, silent: the text path instead of the byte-exact one. **Framework assemblies are trimmed in partial mode, so this one may already be on its fallback in the AOT builds** |
| builtin.cs:3275 | IL2090 | i | add `NonPublicFields` to `T` on `TryTypeAssert<T>` and its five forwarders | silent: a zero struct returned with ok = true |
| builtin.cs:3409 | IL2060 | ii | both requires-attributes on `TryTypeAssert(object, Type, out object)`; 2 / 0 / 0 | loud under AOT for a value-type target |
| TypeParamConversions.cs:72, :99 | IL2091 ×2 | i | three `NonPublic*` flags on `T` of `ConvertToType<T>` (both overloads) and `ConvertToUInt64<T>`; 22 corpus sites, none analyzed | loud: throws by name |
| array.cs:468 | IL2091 | iii | `array<T>.GoZeroLike`; self-guarding | inherits the `ZeroFacts` default |
| ж.PointerExtensions.cs:370 ×2, :388 ×2 | IL2067 ×4 | iii | **remove** the annotation on `FieldTypes`' parameter; the suppression already on that method then covers it. One edit, four records | silent: the address route |
| ж.SliceHeaderBox.cs:115, :134, :139 | IL2090, IL2075, IL2060 | iii | the static ctor; `:134` wants a `[DynamicDependency]` on `unsafe.Pointer.FromBox` | **silent and dangerous: `Applies` = false sends a slice header down the address route** |
| ж.HeaderSliceBox.cs:95 | IL2090 | iii | the static ctor | same |
| NilType.cs:274 | IL2072 | ii | the four `NilType<T>` operators; 0 / 0 / 0 — `NilType<T>` has no user outside its own file | loud, and already wrong untrimmed for a class without a public parameterless ctor. Reads as dead code |
| GoZeroSize.cs:86 | IL2070 | iii | `GoZeroSizeFacts.Classify` | **silent, and the worst here: with no field metadata every struct classifies as zero-size** |
| GoLayoutFacts.cs:48 | IL2070 | iii | `GoLayoutFacts<T>.Diverges` | loud but wrong: `OverNativeMemory` panics |
| Q44RegistryCensus.cs:96 | IL2070 | iii | `PointeeContainsReferences` | diagnostic only |
| GoLibcCall.cs:188 | IL2075 | iii | `DispatchArgsStruct` | **silent: libc is called with no arguments** (darwin) |
| GoLibcCall.cs:291 | IL2075 | iii | `CallStoringResultIntoBlock` | loud |

Nine of the fifteen suppressions are one root cause: a recursive walk over a struct surrogate's
`GetFields`. An annotation does not flow through `FieldInfo.FieldType`, so annotating `T` only moves
the warning one line down.

### Stack frames and three singletons (5 records, all (iii))

| record | code | suppress on | on a null answer |
|---|---|---|---|
| GoFrame.cs:438 | IL2026 | `ReRaisedByTheDeferredDelegate` | the panic site is left unowned; `runtime.Callers` from a deferred call loses the spliced frames |
| runtime/Goroutine.cs:1023 | IL2026 | `CreatorFrame` | no creator; or, if only the creator's frame is missing, the **next** frame is named |
| runtime/RuntimePanicCheck.cs:81 | IL2026 | `RaisedInRuntimePackage` | a runtime panic stays recoverable; a metadata-less user frame over a runtime caller reads as a fatal throw |
| GoMemProfile.cs:84 | IL2026 | `findPprofPackage` | null is the designed answer |
| AdapterBinder.cs:253 | IL2059 | `BuildGenericShellFactory` | caught, falls back to the other shell tier |

If (ii) instead, the attribute would land on `GoFrame.Run` (1 / 2545 / 0), `Goroutine.Start`
(35 / 16, then 888 `goǃ` sites / 0) and `recover` (59 / 302 / 1).

## Findings

1. **The keep is not declared.** See above. This is the decision the rest of the order depends on.
2. **Silent misbehaviour if a Go type's metadata is ever trimmed.** `GoZeroSizeFacts.Classify`
   (true on no fields), `GoLibcCall.DispatchArgsStruct`, the two header boxes' `Applies = false`,
   `ZeroFacts<T>.Classify`, the dyn-struct copy in `TryTypeAssert<T>`, `collectGoFields`,
   `StructFieldsComparable`, `tryCopyGoStructFields`. The first three warrant a loud guard whatever
   happens to the warning.
3. **Paths that cannot work under Native AOT, none of them among the 78** (the AOT analyzer is not
   enabled in golib, so there are no IL3xxx records): `reflect.StructOf` and wide `reflect.FuncOf`
   (Reflection.Emit, and `StructOf` has no catch), `GoReflect.FieldAliasBox` (`DynamicMethod`),
   `reflect.Value.Call` and `reflect.MakeFunc` on variadic funcs (`MakeGenericMethod` over
   instantiations nothing pre-generates). No performance benchmark's source calls `reflect`.
4. **Pragmas do not survive publish.** golib carries 33 `#pragma warning disable IL…` lines in seven
   files. They silence the build analyzer only, so the publish-time analysis still reports what they
   hide; two files disable IL3050 with no runtime guard behind it.
5. **A stale comment.** `GoMemProfile.findPprofPackage` says runtime/pprof's test binary compiles the
   package in; that project references the package assembly instead, so the entry-assembly arm
   cannot succeed there and the assembly-qualified arm does the work.

## What the Native AOT performance tests exercise

Fifteen benchmark projects. Every one prints through `fmt`, so fmt's reflection-backed formatting runs
in all of them. `PerfIfaceShell` is the only one that reaches `AdapterBinder`; `PerfChannel` and
`PerfTlsHandshake` start goroutines, so `CreatorFrame` runs, and nothing reads its answer. None imports
`reflect` or calls `sort.Slice`, and none reaches a panic path except on error. Whether crypto/tls pulls
any of the Emit paths in is not determined.

## Recommended cut order

0. **Decide the keep.** Declare it where it is true: `TrimMode=partial` (or explicit roots for golib
   and the `go.*` assemblies) in the converted-project template, with a guard. Every suppression below
   cites it. This changes a visible emitted project file, so it is a ruling, not a lane cut.
1. **Removals that cost nothing** (up to 8 records): drop the annotation on `FieldTypes` (4); delete or
   attribute `NilType<T>` (1); name the generic definition by `typeof` (3, if it verifies).
2. **The true annotations** (5 records): `ZeroFacts` through `GoZeroFactory<T>`, the two conversion
   helpers, `TryTypeAssert<T>`.
3. **Loud guards** on the three dangerous silent rows, each with a test that makes it fail.
4. **The suppression batches**, one seat per table above, each justification naming the keep from
   step 0; `firstMapEntry` and `SliceHeaderBox` carry a `[DynamicDependency]`.
5. **The pragmas** converted to attributes, so build and publish agree.
6. **A separate pass for AOT**: turn the AOT analyzer on in golib, take the IL3xxx count, and put
   `[RequiresDynamicCode]` or an `IsDynamicCodeSupported` fallback on the paths in Finding 3.

## How it was read

Four read-only passes over the source, one per table, no build. Call-site counts are source-line greps
with whole-line comments filtered; the generic-declaration chain counts are a heuristic fixpoint and
are lower bounds. Statements about what the trimmer or Native AOT keeps are from knowledge of the
runtime, not from a publish, and are marked where they carry a conclusion.

