# PLAN — golib's trim warnings: a quiet build first, a full trim only if it is reachable

> **STATUS: PROPOSED — a scout for the owner (G, 2026-10-08), nothing ruled.** The owner asked whether golib's trim
> and AOT analysis warnings can be removed so that a consumer can publish with Native AOT and a FULL trim. Today a
> consumer needs `TrimMode=partial`, which compiles every go.* assembly whole. The owner added a second goal of its own:
> even if a full trim stays out of reach, the build should be quiet, with every warning answered at the right place
> and never by a blanket suppression. Every number here is measured at master `541766413e` on lane G's windows machine unless the
> text says otherwise. The scripts and raw logs are named in section 8.
>
> **AMENDED 2026-10-08 (G).** The owner ruled the three open questions (section 6). Stages 1 and 2 are cut and seated
> in TRAIN FL as rows 17 (`claude/g-trim-golib-sites`) and 18 (`claude/g-trim-registry`): with stage 2 the
> package consumer RUNS under Native AOT and a full trim. Section 9 proposes stage 3, the dynamic-code class; it is a
> plan only, for the next train, and nothing in it is ruled.

---

## 1. The short answer

**A quiet build is reachable, in four stages, without changing what the converter writes; the one generated addition
is a registration per type, emitted by go2cs-gen (stage 2).** Every one of the
69 warnings has a home: 17 are answered inside golib (an annotation, a small rewrite, or a suppression whose proof
fits in one sentence), 39 are answered by one new piece of generated code that golib's reflection can lean on (a
type registry, the same shape as the GoZero factory registry ruled on 2026-09-28), and 13 are honest
`RequiresUnreferencedCode` / `RequiresDynamicCode` marks at the boundary of the Go features that create types or
code at run time, and of the frame machinery.

**A full trim under Native AOT is NOT reachable by clearing the warnings alone.** Three things the warnings do not
cover stand in the way, and each needs its own design:

1. **The method-set scan.** golib finds every Go method by scanning assemblies for extension methods at run time
   (`TypeExtensions.ExtensionMethodRegistry`). This raises no trim warning, but under a full trim the methods it is
   looking for are the ones the trimmer removes, because nothing calls them directly. Interface satisfaction, type
   assertions and `fmt`'s method lookups all depend on it.
2. **Dynamic code.** 57 more sites build generic instantiations or IL at run time (`MakeGenericType`,
   `MakeGenericMethod`, `DynamicMethod`, `TypeBuilder`). They are reported only when the AOT analyzer is on (IL3050).
   They fail under Native AOT **in either trim mode**: they are an AOT limit, not a trim limit.
3. **Frames.** `runtime.Caller` and the panic machinery read `StackFrame.GetMethod()`, whose answer under Native
   AOT depends on metadata a full trim does not keep.

The owner's instinct is right: the warnings are the visible part. The plan below ends with a quiet build whatever
happens to the full trim (stages 1 to 4), and puts the full trim behind its own measured gate (stage 5).

**What a full trim would buy**, measured on the package-consumer program (section 4), Native AOT on windows, one
12-core machine: the publish drops from **109 minutes to 34 seconds** (about 190 times faster), and the executable
from **243 MB to 12.4 MB** (about 20 times smaller). The partial publish compiles every go.* method whole, including
deep recursive generic instantiations nothing can use (its log carries 296 IL3054 "generic cycle" cut-offs, such as
`array<array<array<array<array<traceAdvancerState>>>>>`). The full trim never builds them, because nothing reaches
them. How the 109 minutes divide between those and the rest was not measured.

**How far away it is**, measured: under a full trim exactly three types lost the field metadata golib needs (two of
them golib's own `@string` and `uintptr`), and with golib's tripwire told to report instead of refuse, this small
program ran to completion. So the distance is not "everything breaks". It is a short list of types for this
program, and an unknown list for a program that leans on `reflect`, `fmt` and interface satisfaction. Stage 5's
gate measures that list.

---

## 2. What is measured

| Reading | Value |
|---|---|
| golib build at master, trim analyzer on (golib's own `PublishTrimmed=True` turns it on) | **69** distinct warnings: IL2070 25, IL2075 11, IL2026 10, IL2067 8, IL2111 5, IL2060 4, IL2055 3, IL2059 1, IL2090 1, IL2091 1 |
| the same build with `EnableAotAnalyzer=true` | **126**: the 69 plus **57 IL3050** (dynamic code) |
| golib's existing answers | 1 `UnconditionalSuppressMessage` (`ж.PointerExtensions.cs:417`, IL2070); 19 `DynamicallyAccessedMembers` on generic parameters; no `RequiresUnreferencedCode`, no `RequiresDynamicCode`, no `DynamicDependency`, no `IsDynamicCodeSupported` check anywhere |
| a whole-program trim of the consumer (ILLink, not AOT, `TrimMode=full`, `TrimmerSingleWarn=false`) | **153** warnings: golib 125 (108 + 17 in `golib/runtime`), the converted `runtime` package 28 (all in its hand-owned `managed_impl.cs` and `memmove_impl.cs`), every other go.* assembly 0 |
| the same consumer compiled by Native AOT with `TrimMode=full` (ILC analyzes only what is reachable) | **98**: golib 67 (56 + 11 in `golib/runtime`), `runtime` 30 (its hand-owned files again, now with IL3000 1 and IL3050 1), `internal/abi` 1 (IL3050), every other go.* assembly 0 |

The owner's table and this reading agree on the total (69) and on every code but two: the table reads IL2070 24 and
IL2075 12, this reading 25 and 11. One row sits under the other code here; which one was not traced.

The publish-time reading matters for scope: **golib is where a full trim starts, and the hand-owned companion files
of converted packages are where it continues**: `runtime/managed_impl.cs` and `runtime/memmove_impl.cs` (28 under
ILLink), and `internal/abi/type_impl.cs:873` (one IL3050, `MakeGenericType` in `synthesizeArrayType`). One more is
specific to Native AOT: `runtime/managed_impl.cs:2422` reads `Assembly.Location` to open a portable PDB (IL3000),
which is empty in a single-file or AOT program. No GENERATED file reports a trim warning of its own in this closure;
a larger closure may (the BOARD's 2026-10-06 publish saw `unsafe` and `slices` report IL2104).

---

## 3. Every warning classified

Six classes. A site's class is decided by where the `Type` it reflects over comes from, because that decides who can
vouch for the members it needs.

### A — annotate or rewrite inside golib (11)

The type is golib's own, or a fixed BCL shape, so golib can state what it needs and the trimmer can check it.

| Site | IL | Answer |
|---|---|---|
| `GoReflect.FieldAccess.cs:653`, `:671` | 2070, 2075 | `ValueSlot` on `ж<T>`: golib's own box. Read it through a `DynamicallyAccessedMembers(PublicProperties)` path, or through the generic `ж<T>` property directly. |
| `GoStructSynthesis.cs:554` | 2075 | `Value` on a field of type `ж<T>`: the same. |
| `ж.SliceHeaderBox.cs:133` | 2075 | `GetMethod` on golib's own box type: annotate the field that holds the type. |
| `GoReflect.TypeLayout.cs:796`, `:797` | 2075 | `Key` / `Value` of a map entry read by name from `KeyValuePair<,>`: enumerate through golib's map interface instead of by property name. |
| `GoMemProfile.cs:84` | 2026 | `Assembly.GetType("go.runtime.pprof_package")`: a `DynamicDependency` naming that type and assembly keeps it when the assembly is present. |
| `GoReflect.TypeLayout.cs:1610` | 2026 | `Delegate.CreateDelegate(type, target, "Invoke")` by NAME: pass the `MethodInfo` instead. |
| `builtin.cs:3039` | 2075 | `GetField("_out")` on an error wrapper golib defines itself: name the wrapper's type statically. |
| `builtin.cs:1590` | 2090 | `ZeroFacts<T>` asks `GetConstructor` whether a Go struct needs constructing. go2cs-gen already registers a factory for exactly those structs (`GoZeroFactory<T>`), so ask the registry instead and drop the reflection. |
| `array.cs:477` | 2091 | `GoZero(T template)` carries `DynamicallyAccessedMembers` on `T` for its `Activator` fallback. With the registry answering above, the fallback and the annotation can go, and the cascade with them. |

### S — safe, and suppressible with a one-sentence proof (6)

| Site | IL | Proof |
|---|---|---|
| `GoReflect.TypeLayout.cs:1287`, `:1564`; `GoReflect.ValueMarshalling.cs:454`, `:468` (two) | 2070, 2075 | `GetMethod("Invoke")` on a delegate type. Every delegate type declares `Invoke`, and the runtime cannot construct a delegate type whose `Invoke` was removed. **The suppression lands only after a Native AOT probe confirms the metadata is there under a full trim** (stage 1 measures it). |
| `Q44RegistryCensus.cs:96` | 2070 | A diagnostic census, off by default and enabled only by an environment variable (its own header). Mark the census entry point `RequiresUnreferencedCode`; it is not product behaviour. |

### R — rooted by a type registry, then suppressed with that justification (29)

The type comes from a Go **value** (`object.GetType()` on whatever a program passes to `reflect`, `fmt`, a type
assertion, an interface conversion). No annotation can follow that flow: `object.GetType()` is where the trimmer's
knowledge ends. What golib needs from such a type is fixed and small: its instance fields, its constructors, and a
few named members of the converted shapes (`m_value`, `NilInstance`, `op_Equality`, the interface's methods).

**The answer is the GoZero shape again.** go2cs-gen already emits, per converted assembly, a module initializer that
registers what golib needs. Extend it so that every converted Go type is registered through a parameter annotated
with `DynamicallyAccessedMembers(Fields | Constructors | the named members)`. A `typeof(T)` flowing into an annotated
parameter is exactly what both trimmers understand: ILLink keeps those members, and the Native AOT compiler also keeps
their reflection METADATA, which is what is missing today. golib's own runtime types (`slice<T>`, `map<K,V>`,
`array<T>`, `ж<T>`, `@string`, `uintptr` and the other scalar wrappers) are registered once, by hand, in golib's own
initializer. Section 4 shows these are not optional: two of the three types a full trim stripped were golib's own.

Each R site then carries an `UnconditionalSuppressMessage` whose justification names the registry. The run-time
tripwire that already exists, `GoFieldMetadata.InstanceFields`, which refuses a struct that "occupies N bytes and
reports no instance fields", stays as the check that the registry did its job.

| Members needed | Sites |
|---|---|
| instance fields | `GoFieldMetadata.cs:55`, `GoLayoutFacts.cs:48`, `GoLibcCall.cs:348`, `GoReflect.FieldAccess.cs:385`, `GoReflect.TypeLayout.cs:746`, `GoReflect.ValueMarshalling.cs:547`, `:552`, `GoReflect.cs:370`, `builtin.cs:2014` |
| the `m_value` field of a defined-type wrapper | `GoReflect.cs:191`, `GoReflect.FinalizerBinding.cs:252`, `GoReflect.ValueMarshalling.cs:963` |
| constructors | `GoReflect.FieldAccess.cs:499`, `GoReflect.ValueMarshalling.cs:941`, `GoReflect.cs:398`, `:421` |
| the parameterless constructor (`Activator.CreateInstance`: a Go struct's zero value) | `GoReflect.TypeLayout.cs:901`, `:1224`, `:1265`, `GoReflect.ValueMarshalling.cs:540`, `:598`, `:614` |
| named members (`NilInstance`, `op_Equality`, a declared property) | `GoReflect.ValueMarshalling.cs:95`, `:193`, `GoReflect.FieldAccess.cs:559` |
| an interface's methods and embedded interfaces (a Go interface's type string) | `GoReflect.TypeNaming.cs:484`, `:490`, `:492`, `GoReflect.cs:541` |

The one existing suppression, `ж.PointerExtensions.cs:417`, is this class too. Its justification today is "referenced
by the converted code that reinterprets them". That holds for ILLink, which keeps every field of a type it keeps, but
it does not hold under Native AOT, which keeps a field's reflection metadata only when something says reflection
needs it. It should cite the registry instead.

The six `Activator.CreateInstance` sites deserve a second look in stage 2: four of them build a zero value, and the
GoZero factory registry already answers that question for the structs where it matters. Asking the registry first
may remove those sites rather than suppress them.

### G — generic instantiation at run time (10, plus most of the 57 IL3050)

| Sites | IL |
|---|---|
| `GoReflect.MethodSets.cs:695`, `GoReflect.TypeLayout.cs:1395`, `:1597` (`MakeGenericType`) | 2055 |
| `ж.SliceHeaderBox.cs:138`, `GoReflect.MakeVariadicDelegate.cs:58`, `GoReflect.TypeLayout.cs:1599`, `builtin.cs:3456` (`MakeGenericMethod`) | 2060 |
| `AdapterBinder.cs:269` (`RunClassConstructor` on a type just made with `MakeGenericType`) | 2059 |
| `GoReflect.ValueMarshalling.cs:670`, `:677` (`Activator.CreateInstance` on an `array<T>` made at run time) | 2067 |

Under a trim alone these are answerable: the generic definitions are found statically, so the trimmer keeps them, and
the type arguments are registry types (class R). With the registry in place, each gets a justified suppression.

Under Native AOT they are a different problem, reported as IL3050. The compiler must see every value-type
instantiation ahead of time, and a call like `typeof(GoReflect).GetMethod(nameof(newBox)).MakeGenericMethod(t)`
names one it cannot see. That is 52 of the 57 IL3050 rows. Two answers, in order of preference:

1. **Generate the instantiation.** For the converted types a program actually has, the registry can carry closed
   delegates for golib's per-type helpers (zero, box, field box, slice window, map entry operations), so golib calls a
   delegate the generator instantiated instead of making a generic method at run time. This is the same move as the
   GoZero factory, applied to more helpers.
2. **Mark the boundary.** Composite types that a program builds through `reflect` at run time (`reflect.SliceOf`,
   `reflect.MapOf`, `reflect.ChanOf`, a function type with more results than any generated `ValueTuple` family) have
   no ahead-of-time instantiation by construction. Those reflect entry points get `RequiresDynamicCode`, and a Native
   AOT build that calls them is told so at compile time.

### E — code emitted at run time: no Native AOT at all (10 of the 69, 5 of the IL3050)

| Site | What it does |
|---|---|
| `GoDelegateSynthesis.cs:155` (IL2026 ×5, IL2111 ×5) | `reflect.FuncOf`: defines a delegate type with `TypeBuilder` |
| `GoStructSynthesis.cs:192` (IL3050) | `reflect.StructOf`: defines a dynamic assembly |
| `ж.Contracts.cs:143`, `GoReflect.FieldAccess.cs:642` (IL3050) | `DynamicMethod` field-reference accessors |
| `TypeExtensions.ExtensionMethodRegistry.cs:440`, `:444` (IL3050) | `Expression.GetActionType` / `GetFuncType` |

Native AOT has no `Reflection.Emit`. `reflect.FuncOf` and `reflect.StructOf` are the capability boundary C2 recorded
on the BOARD (2026-10-06): they get `RequiresUnreferencedCode` and `RequiresDynamicCode` on their public entry points,
and nothing more is owed.

The `DynamicMethod` field accessors are different, and they are the most important finding in this class. They serve
an ordinary `reflect.Value.Field(i)` on an addressable struct, not an exotic feature. go2cs-gen already generates a
field accessor for converted types (the "A17" accessor the IL path imitates). Stage 3 makes golib use the generated
accessor whenever one exists, and keeps the `DynamicMethod` path behind `RuntimeFeature.IsDynamicCodeSupported` for
synthesized types only.

### F — frames (3)

| Site | Use |
|---|---|
| `GoFrame.cs:443`, `runtime/Goroutine.cs:1117`, `runtime/RuntimePanicCheck.cs:81` | `StackFrame.GetMethod()`: which Go function a frame belongs to (`runtime.Caller`, the panic and recover machinery) |

Under Native AOT `GetMethod()` answers only for methods whose metadata was kept. The windows `MetadataToken` failure
recorded in docs/KnownIssues.md (fixed on `claude/i9-aot-metadata-token`) is the same family. These sites need
`RequiresUnreferencedCode` on a narrow internal boundary for the quiet build. For the full trim they need a
position-record route that does not go through `MethodBase` at all, and that is the riskiest change in this plan
(stage 5).

### Not warned, and the largest risk: the method-set scan

`TypeExtensions.ExtensionMethodRegistry` (`LoadAssemblyExtensionMethods`) builds every Go method set by calling
`Assembly.GetTypes()` and `Type.GetMethods()` over each loaded assembly at run time. Neither call raises a trim
warning here. Under a full trim, though, a converted method that nothing calls directly is exactly what gets removed,
and a Go method reached only through an interface value is reached only through this scan. Under Native AOT,
`GetMethods()` also returns only methods whose metadata was kept.

The answer is again generated: go2cs-gen already knows every type's method set (the implement and member-record
generators read it), so it can register method sets the way the GoZero factories are registered, and golib can stop
scanning. That is a large change to the hottest path in golib (`StructurallyImplements`, the adapters, `fmt`), and
it comes last.

---

## 4. The measured baseline: the package consumer under a full trim

The program is `src/tests/PackageTests/PackageSymbols/Program.cs`: a `sort.Slice` whose comparator asks
`runtime.Caller` for its frames. It is built over **project references** to master's golib, runtime and sort, so the
publish measures master's assemblies, and none of go.lib's packed defaults apply. That puts the SDK's own default for
a Native AOT consumer, a full trim, under test. win-x64, one 12-core windows machine.

| Arm | Publish | Wall | Executable | Runs |
|---|---|---|---|---|
| N: ILLink full trim, self-contained, not AOT (control) | 0 | 94 s | 162 KB host + 31 MB folder | **exit 0**, `PACKAGE-SYMBOLS: zsortfunc.go:73` (go.sort's own frame, read from its symbol file) |
| F: Native AOT, `TrimMode=full` | 0 | **34 s** | **12.4 MB** | **exit 2 at startup**: `TypeInitializationException` over `go2cs: go.internal.cpu_package+option occupies 32 bytes and reports no instance fields — its field metadata was removed, most likely by trimming` |
| P: Native AOT, `TrimMode=partial` (go.lib's packed default) | 0 | **6,541 s (109 min)** | **243 MB** | **exit 2 at the first `runtime.Caller`**: `There is no metadata token available for the given member`, in `runtime.internCallerFrame` |
| F′: arm F with golib's field-metadata tripwire made to REPORT instead of throw (a local instrument, restored byte-identical) | 0 | 125 s (golib rebuilt) | 12.4 MB | **exit 0**; three types reported (below) |

**Arm P's failure is not the trim.** It is the windows defect recorded in docs/KnownIssues.md: the converted runtime
keyed a call site by its method's metadata token. Its fix is not on master yet, so at master a windows Native AOT
consumer that calls `runtime.Caller` fails under either trim mode. The fix rides TRAIN FL: the i9's change restated
on the per-call caller-line work as `claude/i9-aot-metadata-token-on-g` (`cfe3d6bae7`, row 11, an ancestor of the FL
union `9b7dfdb2ec`), and **this failure is expected to clear when TRAIN FL lands**; C1's `aot-smoke` at that tip is
the release gate. On linux the same partial publish of the published packages runs (the hosted `aot-smoke` record
in docs/KnownIssues.md).

**Which types lose their field metadata under a full trim (arm F′).** Exactly three were refused in this program:

| Type | Size | Whose |
|---|---|---|
| `go.internal.cpu_package+option` | 32 bytes | converted (`internal/cpu`) |
| `go.@string` | 16 bytes | **golib's own** |
| `go.uintptr` | 8 bytes | **golib's own** |

Two of the three are golib's core value types, which says the registry in stage 2 must cover golib's own types, not
only converted ones. With the tripwire quiet, this small program ran to the end and exited 0. That is not evidence
that the full trim is safe. The tripwire exists because golib derives a struct's size, layout and call arguments
from those fields, and a type that "reports no fields" is answered wrongly without a sound. This program reaches
little of golib's reflection; a program that formats, compares or reflects over these types would not be so lucky.

The frame line reads `none` under arm F′, the one Native AOT arm that ran to the end, where the trimmed JIT control
reads `zsortfunc.go:73`. The fixture's own script prints that line for AOT without judging it, so it is not counted as
a trim failure here. It is the frame question of class F all the same.

---

## 5. The staged order

Each stage leaves the build quieter than it found it and changes no converted output except where it says so. Risky
items come last.

| Stage | What | Warnings answered | Risk |
|---|---|---|---|
| **1** | Class A and S inside golib: annotations, the three small rewrites, the delegate-`Invoke` suppressions after a Native AOT probe confirms them, the Q44 census marked. One GolibTests arm per rewrite. | 17 | low: golib only, no generated change |
| **2** | The type registry: go2cs-gen registers every converted Go type through an annotated parameter in the module initializer it already emits; golib registers its own runtime types. The 29 class-R sites and the 10 class-G sites get suppressions that cite it, and the one existing suppression is re-justified. A guard test: a type missing from the registry is refused by name, which is the existing `GoFieldMetadata` tripwire plus one for constructors. | 39 | medium: one generated line per type, in every converted assembly; a corpus regeneration |
| **3** | The honest boundaries: `RequiresUnreferencedCode` / `RequiresDynamicCode` on `reflect.FuncOf`, `reflect.StructOf` and the run-time composite constructors, and on the narrow frame boundary (class F). `IsDynamicCodeSupported` fallbacks where a generated path exists (the field accessors). The converted runtime's 28 publish-time warnings get the same treatment, in its two hand-owned files. | 13 + the AOT set | medium: touches the reflect surface's annotations, which converted callers see |
| **4** | **A quiet build.** golib and the hand-owned runtime files build with zero IL warnings, the AOT analyzer is turned on in golib's own build so IL3050 cannot creep back, and every remaining suppression carries a justification naming its proof. | all 69, and the 57 IL3050 either answered or behind a boundary | low, given 1 to 3 |
| **5** | **The full trim, behind its own gate.** The method-set registry generated by go2cs-gen (replacing the scan), the frame route that does not need `MethodBase`, and the generated instantiations for class G under Native AOT. Then the package consumer, and the behavioral suite's reflection-heavy projects, published with Native AOT and `TrimMode=full`, run and compare. Only if that gate is green does go.lib's packed default move from `partial` to `full`. | n/a | **high**: the method-set path is golib's hottest, and every converted program depends on it |

Stages 1 to 4 are worth doing even if stage 5 is never reached: they turn 69 unexplained warnings into a short list of
stated limits, and they keep a regression in golib's reflection from arriving silently.

---

## 6. Open questions for the owner

- **OQ-1.** Is the quiet build (stages 1 to 4) wanted on its own, with stage 5 sized separately afterwards? This plan
  recommends yes.
- **OQ-2.** Stage 2 adds one generated registration per converted type. Is a corpus-wide regeneration for that
  acceptable, and should it ride the face lift's records (the type-level record mechanism of PLAN-marker-comment-parity
  §5.4 to §5.6) rather than a separate initializer?
- **OQ-3.** Under Native AOT, should `reflect.FuncOf` and `reflect.StructOf` fail at compile time (the analyzer's
  answer once `RequiresDynamicCode` is on them), or at run time with a Go panic naming the limit?

**RULED, the owner, 2026-10-08 (ledger 11:4x):** OQ-1 yes, the quiet build first and stage 5 separately. OQ-2 "why
not add now": stage 2 in TRAIN FL. It was built as go2cs-gen output, not on the record line, because an attribute on
a declaration does not keep metadata under Native AOT (measured; section 9.1). OQ-3: a RUN-TIME Go panic naming the
limit by default, PLUS an opt-in consumer build switch (an MSBuild property) that makes those paths fail the AOT build
for a consumer who wants it caught at compile time. Section 9.5 is the design.

---

## 7. What this plan does not claim

- It does not observe the method-set scan failing under a full trim. The consumer never asks an interface question
  that needs a scanned method, so it ran (arm F′). That the scan is exposed is read from the code, not observed.
- Its AOT numbers are windows only, on one 12-core machine. The linux and macOS hosted records (56 min, 256 to 293
  min) are for the partial publish of the published packages and are quoted, not re-measured.
- It does not size stage 5. Its cost depends on the method-set registry's design, which is a plan of its own.
- It reads one consumer. The behavioral suite's reflection-heavy projects are the stage-5 gate, not this scout's.

---

## 8. Instruments

<!-- G, 2026-10-08, all at master 541766413e on lane G's windows machine, worktree claude/g-golib-full-trim-plan (no source change).
     Warning census: scratch efT2.sh (two golib builds, arm A as master builds it, arm B with EnableAotAnalyzer and
     EnableSingleFileAnalyzer); distinct rows by file(line,col) and code; the site digest by sites.pl.
     Consumer arms: scratch efT3.sh; the consumer csproj carries the arm in FtArm, because a GLOBAL PublishTrimmed or
     PublishAot reaches the netstandard2.0 generator project and fails it (NETSDK1124). TrimmerSingleWarn=false on
     every arm, so warnings are per member across the closure; byasm.pl splits them per package directory.
     Arms F and P: scratch efT3b.sh. ILC's link step calls vswhere.exe from PATH, so the VS Installer directory must be
     on it (the first F attempt failed there, MSB3073, and is not a reading). Arm F': scratch efT3c.sh, which copied
     GoFieldMetadata.scout.cs (the tripwire reporting each refused type once on stderr instead of throwing) over
     golib/GoFieldMetadata.cs for one publish and restored it: sha 481a488a53f56397 before and after, tree clean.
     Arm P wall 6541 s, exe 243,076,096 bytes (output folder 1.36 GB with symbols); arm F exe 12,368,384 bytes.
     The fix for P's windows failure: claude/i9-aot-metadata-token-on-g cfe3d6bae7, TRAIN FL row 11 (merged 06:51),
     an ancestor of the FL union 9b7dfdb2ec and not of master 541766413e (checked 2026-10-08). It supersedes the
     original cut dcd82dc87d, which is an ancestor of neither; the first version of this plan checked only that cut and
     wrongly said the fix was missing from TRAIN FL (corrected by COORD, 2026-10-08). -->
     <!-- Arm P was measured at master, without the fix, so its exit 2 is a reading of master; it is not re-run here. -->

The scripts live in the lane's scratch area, not in the repository; the comment above says what each one did, so any
reading can be repeated against master `541766413e`.

---

## 9. Stage 3, the dynamic-code class (PROPOSED, 2026-10-08)

> **STATUS: PROPOSED (G), a plan only, no cut; for the train after TRAIN FL.** Every site and line here is read at
> the TRAIN FL fixup tip `9b7dfdb2ec`, the base stages 1 and 2 were cut on.

### 9.1 Where stages 1 and 2 left things (measured)

- Stage 2's registry works: on the package consumer, Native AOT with a full trim RUNS (exit 0), and no type loses its
  field metadata (3 before). The executable grows from 12.4 to 24.5 MB, because every Go type of the closure is kept;
  COORD accepted that price (ledger 19:36).
- An attribute on a type does NOT keep its metadata under Native AOT; a `[DynamicDependency]` on a kept method does.
  Measured on the FL union, three local probes: type-level `DynamicallyAccessedMembers` left all 3 types stripped; a
  `typeof(T)` into an annotated parameter, and an empty module initializer carrying `[DynamicDependency]`, both left 0.
- Generic structs keep their fields too: a probe that formats `Pair[string, int]` and `Box[Plain]` through `fmt` prints
  exactly Go's output under a full trim with stage 2, where the base printed `{}` for both.
- **The next stop is dynamic code, not trimming.** The same probe then asks `reflect.TypeOf(p).Field(1).Type`, and the
  program stops with `GoReflect.readSlot[reflect_package+rtype] is missing native code. MethodInfo.MakeGenericMethod()
  is not compatible with AOT compilation`. That is `ReadPointerSlot` (`GoReflect.FieldAccess.cs:98`), one of the 57
  IL3050 sites this section is about.

### 9.2 The 57 sites, in four families

The AOT analyzer reports 57 IL3050 rows in golib at the FL union, the same count as at master. By what they build:
`MakeGenericMethod` 32, `MakeGenericType` 20, `DynamicMethod` 2, `Expression` delegate types 2, a dynamic assembly 1.
By what they are FOR, which is what decides the answer:

| Family | Sites | Where |
|---|---|---|
| **V. A value operation over a run-time type** (`reflect.Value`'s slots, fields, elements, slices, maps; nil pointers; new boxes; pointer reinterprets; header boxes; a type assertion to a run-time type) | 33 | `GoReflect.FieldAccess.cs` `ReadPointerSlot` :98, `WritePointerSlot` :110, `FieldAliasBox` :607, `isReadonlyZeroSizeField` :651, `ElementAliasBoxOfBox` :701, `ElementAliasBoxOfValue` :719, `SliceWindow` :739 :771, `GrowSlice` :806, `SetMapEntry` :850, `DeleteMapEntry` :874, `TryGetMapEntry` :900; `GoReflect.ValueMarshalling.cs` `CanonicalNilPointer` :91 :114, the default and container factories :648 :697 :714, `MakeSizedArray` :672, `TryByteSliceView` :786, `TryByteSliceAs` :828; `GoReflect.PointerConversions.cs` `AliasSliceAsArrayPointer` :66, `TryConvertPointer` :131 :137, `TryReinterpretValue` :198 :202, `WithChanCargo` :232; `GoReflect.TypeLayout.cs` `firstArrayElement` :861; `ж.SliceHeaderBox.cs` :137 :142 :143; `ж.HeaderSliceBox.cs` :112 :113; `builtin.cs` `TryTypeAssert` :3456 |
| **F. A function type built at run time** (Go's multiple results as a `ValueTuple`, a variadic tail, the variadic delegate families and their trampolines) | 12 | `GoReflect.TypeLayout.cs` `TryFuncShape` :1322, `MakeGoFuncType` :1374, `makeGoResultType` :1391 to :1396 and :1400, `buildVariadicInvoker` :1602 :1604; `GoReflect.MakeVariadicDelegate.cs` :58 |
| **M. Method sets and interface shells** (a generic Go method's closed extension, a box target, a delegate type for an extension, a generic adapter shell, a generic conversion operator) | 7 | `runtime/TypeExtensions.ExtensionMethodRegistry.cs` `CreateStaticDelegate` :415 :416 :440 :444, `GetExtensionMethods` :323; `runtime/TypeExtensions.cs` :143; `AdapterBinder.cs` :260 |
| **E. Code emitted at run time** (`reflect.StructOf`'s dynamic assembly; the `DynamicMethod` field accessors) | 5 | `GoStructSynthesis.cs` :192; `GoReflect.FieldAccess.cs` `buildFieldAccessor` :646 :671 :685; `ж.Contracts.cs` :143 |

Outside golib the same class had two more in master's Native AOT publish of the consumer (section 2, not re-read at
the FL union): `internal/abi/type_impl.cs:873` (`synthesizeArrayType`, `MakeGenericType`) and one in
`runtime/managed_impl.cs`, both hand-owned companion files.

### 9.3 The principle

Native AOT compiles a generic instantiation only when compiled code names it. golib reaches its generic helpers from a
`Type` it learned at run time (`MakeGenericMethod(t)`), which names nothing. Three answers, in order of preference:

1. **Instance dispatch on a value golib already holds.** If the operation's input is a value, its generic type already
   exists, because the value does: `ж<T>`, `slice<T>`, `map<K,V>`, `array<T>`, `channel<T>` and the generated wrappers
   were constructed by compiled code. A non-generic interface those types implement (a slot read and write, a window, a
   grow, a map get, set and delete, an element alias) is compiled for every instantiation the program has, so golib
   calls it instead of making a generic method. No generator change; golib only. This answers the measured stop:
   `ReadPointerSlot` holds the box, and `ж<T>` reading its own slot as `object` needs no `MakeGenericMethod`.
2. **A creation hook generated per Go type.** An operation that starts from a `Type` alone (a new box, a default value,
   a sized array, a canonical nil pointer, a container of a given type) has no value to dispatch on. go2cs-gen writes, on
   each Go type's generated part, a static member that names the instantiation (for example a `GoTypeOps<T>` object
   reached through a static field). The field is initialized on first use, so a program pays nothing for types it never
   creates by reflection, and ILC compiles `GoTypeOps<T>` because the generated code names it. golib finds the member
   through stage 2's registry, which already keeps the type's fields.
3. **The boundary.** What is left builds a type the program never had: `reflect.StructOf`, `reflect.FuncOf` with a
   signature no compiled code uses, `reflect.SliceOf`, `MapOf`, `ChanOf` or `ArrayOf` over a combination no compiled
   code mentions, a composite deeper than the generated hooks reach. These carry `RequiresDynamicCode` and the owner's
   OQ-3 behaviour (9.5).

### 9.4 Family by family

- **V (33).** Most take a value: the slot pair, the field and element aliases, the slice window and grow, the three map
  operations, the byte-slice views, the pointer reinterprets, the channel cargo, the header boxes (`SliceHeaderBox` and
  `HeaderSliceBox` close `Describe`, `ElementZero` and `Words` over the slice's element, which the slice value already
  knows). These move to answer 1. `isReadonlyZeroSizeField` asks a per-type FACT through `GoZeroSizeFacts<T>`; the fact
  can be computed from the field metadata stage 2 keeps, with no instantiation. Answer 2 takes the few that create from a
  type alone: `CanonicalNilPointer`, the default and container factories, `MakeSizedArray`. `TryTypeAssert` (a type
  assertion to a type known only at run time) is the one to size first: it may need answer 2 for the asserted type.
- **F (12).** A Go func type is a delegate. Every delegate type a converted program USES exists, so a func type built
  from Go parts can first look for the existing delegate with that signature (registered, as stage 2 registers types)
  and build a new one only when none exists. The variadic families are golib's own, so their instantiations over
  registered element types can be generated the same way. A signature no compiled code uses is answer 3.
- **M (7).** These belong to the method-set scan, which stage 5 replaces with a method-set registry generated by
  go2cs-gen. Until then they are answer 3, and a program that needs them fails with the OQ-3 panic, not silently.
- **E (5).** `reflect.StructOf` and `reflect.FuncOf` are the capability boundary C2 recorded on the BOARD (2026-10-06):
  answer 3. The `DynamicMethod` field accessors serve an ordinary `reflect.Value.Field(i)`, so they matter more:
  go2cs-gen already generates a field accessor for converted types (the "A17" accessor the IL path imitates), and golib
  should take it whenever it exists, keeping the `DynamicMethod` only for a synthesized type.

### 9.5 The owner's OQ-3, as a design

- **By default, a Go panic at run time.** Every remaining dynamic path checks `RuntimeFeature.IsDynamicCodeSupported`
  first and, where it is false, panics with a Go error that names the operation and the limit (for example `reflect:
  StructOf needs code generated at run time, which a Native AOT program does not have`). A JIT program never sees it.
- **Opt-in, a build failure.** A consumer property in go.lib's packed targets, proposed name
  `GoAotFailOnDynamicCode` (default off), sets `TrimmerSingleWarn=false` and adds IL3050 to `WarningsAsErrors`. The
  remaining dynamic paths carry `RequiresDynamicCode` rather than a suppression, so ILC reports each one the PROGRAM
  REACHES (it analyzes reachable code only) as an error at publish. A program that never reaches one publishes as
  before. The property is what makes "caught at compile time" possible without making every consumer pay for it.
- **Why the annotation does not spread into the corpus.** Converted projects do not run the AOT analyzer, so a
  `RequiresDynamicCode` on a golib entry point raises nothing in a converted project's build. Only ILC sees it, at a
  consumer's publish, and only on the paths that program reaches.
- **The gate.** A planted `reflect.StructOf` in a probe program: with the property, the publish fails naming it; without
  it, the publish succeeds and the program panics with the message above.

### 9.6 Two items owed by earlier stages

- **`GoReflect.MemberRecords.cs:200`** (IL2070, open since stage 2). `ParamDimsRecords` calls `GetMethods` on a type
  that carries a `[GoParamDims]` record, to find the method the record names. Rooting every method of a package class
  would undo the trim. The generator writes each record, so it knows the method's signature, and stage 2's registration
  can carry one `[DynamicDependency("<documentation signature>", typeof(<pkg>_package))]` per recorded method: the
  method's metadata is then kept exactly where a record needs it. GenTests' registration guard grows an arm: every
  `[GoParamDims]` record's method is registered.
- **`builtin.cs:1590` and `array.cs:477`** (the BOARD row of 2026-10-08). A generic struct that needs construction has
  no GoZero factory, because a module initializer cannot name an open generic. Answer 2 fits: the creation hook on the
  generated part of a generic needy struct can name its own closed instantiation's zero, so both sites read the hook
  and stop asking for a constructor. GoZeroResidualTests gains the `array<T>` case first, red.

### 9.7 Frames (class F of section 3)

The three `StackFrame.GetMethod()` sites stay as they are in stage 3 and get `RequiresUnreferencedCode` on a narrow
internal boundary in stage 4, for the quiet build. Under Native AOT the package consumer's frame line reads `none`
where the trimmed JIT reads `zsortfunc.go:73`: Go file and line for a frame needs a position route that does not go
through `MethodBase`. That is stage 5.

### 9.8 Order, risk and gates

| Sub-stage | What | Risk |
|---|---|---|
| **3a** | Answer 1 for family V's value-taking sites, including the measured stop; `isReadonlyZeroSizeField` from metadata. golib only. | low: golib only, no generated change |
| **3b** | `MemberRecords:200` through the registration; the deferred zero-value pair through a creation hook. | low to medium |
| **3c** | Answer 2: the generated creation hooks, for family V's create-from-type sites and family F's variadic families. | medium: one generated member per Go type; generator output changes corpus-wide |
| **3d** | Answer 3: `RequiresDynamicCode` on what remains, the OQ-3 panic, and the `GoAotFailOnDynamicCode` property. | medium: touches go.lib's packed targets |
| **3e** | Family F's func types through existing delegates. | medium to high |
| (5) | Family M with the method-set registry; frames. | high, as section 5 says |

Gates for each sub-stage, beyond those stages 1 and 2 ran: a Native AOT probe battery under a full trim, one program per
family (`reflect.Value` `Field`, `Set`, `Index`, `Slice`, `Append`, `MapIndex`, `SetMapIndex`, `New`, `Zero`; func
types through `reflect.TypeOf(f)` and `Call`; `MakeFunc`; a type assertion to a run-time type; `fmt` of each), its
output compared with Go's; and the IL3050 census, predicted per sub-stage and read file by file, as stages 1 and 2 did.

### 9.9 Questions for the owner

- **OQ-4.** The opt-in property's name (`GoAotFailOnDynamicCode` is proposed) and its scope: IL3050 only, or the
  trimming analyzer's IL2026 at the same boundaries as well?
- **OQ-5.** Answer 2 adds one static member to every Go type's generated part (generated output only, nothing in the
  committed corpus). Is that acceptable in principle, with its size cost measured in 3c before it seats?

### 9.10 What this section does not claim

- It does not size stage 3. The sub-stage order is by risk, not by measured cost.
- It does not know yet how many of family V's create-from-type sites a real program reaches; the probe battery of 3a
  measures it.
- Whether the generated creation hooks reach deep enough composites (a `slice` of a `map` of a generic struct) without
  the generic-cycle cut-offs ILC already reports under a partial trim is a measurement owed in 3c.

<!-- G, 2026-10-08, at the FL fixup tip 9b7dfdb2ec. IL3050 census: scratch efR2a (arm B minus arm A, 57 rows); the
     families by enclosing member (il3050-rows.pl, enclosing.pl). The measured stop: efR2d G4 probe-seat (Native AOT,
     full trim, stage 2 seat ca1a2cf81d): output lines 1 and 2 equal Go's, then NotSupportedException at
     GoReflect.readSlot[reflect_package+rtype]. The attribute-on-type falsification: scratch efZ1 (arms C, D), the
     registration that works: efZ2 (arm E), efZ3 (arm G). -->
