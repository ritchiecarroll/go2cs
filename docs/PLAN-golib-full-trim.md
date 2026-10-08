# PLAN — golib's trim warnings: a quiet build first, a full trim only if it is reachable

> **STATUS: PROPOSED — a scout for the owner (G, 2026-10-08), nothing ruled.** The owner asked whether golib's trim
> and AOT analysis warnings can be removed so that a consumer can publish with Native AOT and a FULL trim. Today a
> consumer needs `TrimMode=partial`, which compiles every go.* assembly whole. The owner added a second goal of its own:
> even if a full trim stays out of reach, the build should be quiet, with every warning answered at the right place
> and never by a blanket suppression. Every number here is measured at master `541766413e` on lane G's windows machine unless the
> text says otherwise. The scripts and raw logs are named in section 8.

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
keyed a call site by its method's metadata token. Its fix, `claude/i9-aot-metadata-token` (`dcd82dc87d`), is in
neither master nor TRAIN FL, so at master **a windows Native AOT consumer that calls `runtime.Caller` fails under
either trim mode.** On linux the same partial publish of the published packages runs (the hosted `aot-smoke` record
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
     The i9 fix for P's windows failure: claude/i9-aot-metadata-token dcd82dc87d, an ancestor of neither master
     541766413e nor claude/coord-trainFL-union (checked 2026-10-08). -->

The scripts live in the lane's scratch area, not in the repository; the comment above says what each one did, so any
reading can be repeated against master `541766413e`.
